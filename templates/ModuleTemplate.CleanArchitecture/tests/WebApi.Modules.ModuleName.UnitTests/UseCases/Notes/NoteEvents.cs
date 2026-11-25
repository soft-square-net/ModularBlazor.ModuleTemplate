using System.Text.Json;
using soft.square.net.WebApi.Contracts.Live;
using soft.square.net.WebApi.Contracts.Messaging;
using soft.square.net.WebApi.Modules.ModuleName.UseCases;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Events;

namespace soft.square.net.WebApi.Modules.ModuleName.UnitTests.UseCases.Notes;

/// <summary>The module's integration events and live topics: names, patterns and the example handler.</summary>
public class NoteEvents
{
    private readonly ILiveNotifier _live = Substitute.For<ILiveNotifier>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static IntegrationEnvelope Envelope(string type, object data) =>
        new(Guid.NewGuid(), type, "acme", "note:7", DateTimeOffset.UtcNow, "corr", "modulename",
            JsonSerializer.SerializeToElement(data, IntegrationEnvelope.JsonOptions));

    [Fact]
    public void TheNamesAreValidAndTheManifestPatternsMatchTheEvents()
    {
        IntegrationTypes.IsValidType(ModuleNameEvents.NoteEdited).ShouldBeTrue();
        ModuleNameEvents.Subscriptions.ShouldAllBe(p => IntegrationTypes.IsValidPattern(p));
        ModuleNameEvents.Subscriptions.ShouldContain(p => IntegrationTypes.Matches(p, ModuleNameEvents.NoteEdited));
        ModuleNameEvents.NoteTopic(7).ShouldBe("modulename:note:7");
        ModuleNameEvents.LiveTopicKinds.ShouldContain(new LiveTopic("note", ModuleNamePermissions.Read));
    }

    [Fact]
    public async Task AnEditedNoteIsPushedToItsLiveTopic()
    {
        await new NoteEditedLiveHandler(_live).HandleAsync(Envelope(ModuleNameEvents.NoteEdited, new NoteEdited(7, 3, "Title")), Ct);

        await _live.Received(1).PublishAsync("modulename:note:7", NoteEditedLiveHandler.MessageType,
            Arg.Is<JsonElement?>(p => p!.Value.GetProperty("version").GetInt32() == 3), 3, Ct);
    }

    [Fact]
    public async Task OtherNoteEventsAreIgnored()
    {
        await new NoteEditedLiveHandler(_live).HandleAsync(Envelope("modulename.note-archived.v1", new { id = 7 }), Ct);

        await _live.DidNotReceiveWithAnyArgs().PublishAsync(default!, default!, default, default, default);
    }
}
