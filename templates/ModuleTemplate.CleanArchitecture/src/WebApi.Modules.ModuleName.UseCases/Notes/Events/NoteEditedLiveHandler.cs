using System.Text.Json;
using soft.square.net.WebApi.Contracts.Live;
using soft.square.net.WebApi.Contracts.Messaging;

namespace soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Events;

/// <summary>
/// The example integration event handler: when a note was edited (on any replica, in any pool), tell the browsers that
/// watch it, on the live topic <c>modulename:note:{id}</c>. The event arrives only after the edit was committed, so the
/// browsers re-read data that is already there.
/// <para>Handlers run in a tenant scope of this module's container, once per event (the module's inbox drops a second
/// delivery). Changes made through the module's DbContext are saved by the platform with the inbox row: do not call
/// SaveChanges here. When a handler throws, the event is retried, then dead-lettered.</para>
/// </summary>
public sealed class NoteEditedLiveHandler(ILiveNotifier live) : IIntegrationEventHandler
{
    /// <summary>The live message type browsers receive.</summary>
    public const string MessageType = "note-edited";

    public Task HandleAsync(IntegrationEnvelope envelope, CancellationToken ct)
    {
        if (envelope.Type != ModuleNameEvents.NoteEdited) return Task.CompletedTask; // other note events: nothing to do yet

        var note = envelope.DataAs<NoteEdited>();
        return live.PublishAsync(ModuleNameEvents.NoteTopic(note.Id), MessageType,
            JsonSerializer.SerializeToElement(new { note.Id, note.Version }, JsonSerializerOptions.Web), sequence: note.Version, ct);
    }
}
