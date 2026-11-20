using Ardalis.Specification;
using Microsoft.Extensions.Options;
using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;
using soft.square.net.WebApi.Modules.ModuleName.UseCases;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Create;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Delete;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Edit;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.GetById;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.List;
using soft.square.net.WebApi.Modules.ModuleName.UseCases.Notes.Search;

namespace soft.square.net.WebApi.Modules.ModuleName.UnitTests.UseCases.Notes;

/// <summary>The note use cases over substituted repositories and platform contracts: no database, no host.</summary>
public class NoteHandlers
{
    private readonly IRepository<Note> _notes = Substitute.For<IRepository<Note>>();
    private readonly IReadRepository<Note> _reader = Substitute.For<IReadRepository<Note>>();
    private readonly IAccessScopes _access = Substitute.For<IAccessScopes>();
    private readonly TestClock _clock = new();
    private readonly IOptions<ModuleNameOptions> _options = Options.Create(new ModuleNameOptions { MaxPageSize = 50 });

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private Note Stored(int version = 0)
    {
        var note = Note.Create("user-1", "Title", "body", _clock.Now);
        for (var i = 0; i < version; i++) note.Edit("Title", "body", _clock.Now);
        _notes.FirstOrDefaultAsync(Arg.Any<ISpecification<Note>>(), Arg.Any<CancellationToken>()).Returns(note);
        return note;
    }

    [Fact]
    public async Task CreateNeedsASignedInCaller()
    {
        var result = await new CreateNoteHandler(_notes, _access, _clock).Handle(new CreateNoteCommand("Title", null), Ct);

        result.Status.ShouldBe(ResultStatus.Unauthorized);
        await _notes.DidNotReceiveWithAnyArgs().AddAsync(default!, Ct);
    }

    [Fact]
    public async Task CreateAddsANoteByTheCaller()
    {
        _access.UserId.Returns("user-1");
        _notes.AddAsync(Arg.Any<Note>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Note>());

        var result = await new CreateNoteHandler(_notes, _access, _clock).Handle(new CreateNoteCommand("Title", "body"), Ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.CreatedBy.ShouldBe("user-1");
        result.Value.CreatedAt.ShouldBe(TestClock.Start);
        await _notes.Received(1).AddAsync(Arg.Is<Note>(n => n.Title == "Title"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EditIsNotFoundForAMissingNote()
    {
        var result = await new EditNoteHandler(_notes, _clock).Handle(new EditNoteCommand(7, 0, "Title", null), Ct);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task EditConflictsOnAStaleVersionWithoutSaving()
    {
        Stored(version: 2);

        var result = await new EditNoteHandler(_notes, _clock).Handle(new EditNoteCommand(1, 1, "New", null), Ct);

        result.Status.ShouldBe(ResultStatus.Conflict);
        await _notes.DidNotReceiveWithAnyArgs().UpdateAsync(default!, Ct);
    }

    [Fact]
    public async Task EditSavesTheChange()
    {
        var note = Stored();
        _clock.Advance(TimeSpan.FromMinutes(1));

        var result = await new EditNoteHandler(_notes, _clock).Handle(new EditNoteCommand(1, 0, "New", "text"), Ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Title.ShouldBe("New");
        result.Value.Version.ShouldBe(1);
        result.Value.UpdatedAt.ShouldBe(TestClock.Start.AddMinutes(1));
        await _notes.Received(1).UpdateAsync(note, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteConflictsOnAStaleVersion()
    {
        Stored(version: 1);

        var result = await new DeleteNoteHandler(_notes).Handle(new DeleteNoteCommand(1, 0), Ct);

        result.Status.ShouldBe(ResultStatus.Conflict);
        await _notes.DidNotReceiveWithAnyArgs().DeleteAsync(default!, Ct);
    }

    [Fact]
    public async Task DeleteRemovesTheNote()
    {
        var note = Stored();

        var result = await new DeleteNoteHandler(_notes).Handle(new DeleteNoteCommand(1, 0), Ct);

        result.Status.ShouldBe(ResultStatus.NoContent);
        await _notes.Received(1).DeleteAsync(note, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdIsNotFoundForAMissingNote()
    {
        var result = await new GetNoteByIdHandler(_reader).Handle(new GetNoteByIdQuery(7), Ct);

        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    public async Task ListRejectsAPageOutsideTheSettings(int page, int pageSize)
    {
        var result = await new ListNotesHandler(_reader, _options).Handle(new ListNotesQuery(page, pageSize), Ct);

        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public async Task ListReturnsThePageAndTheTotal()
    {
        _reader.ListAsync(Arg.Any<ISpecification<Note>>(), Arg.Any<CancellationToken>())
            .Returns([Note.Create("user-1", "One", null, _clock.Now)]);
        _reader.CountAsync(Arg.Any<ISpecification<Note>>(), Arg.Any<CancellationToken>()).Returns(11);

        var result = await new ListNotesHandler(_reader, _options).Handle(new ListNotesQuery(2, 10), Ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Single().Title.ShouldBe("One");
        result.Value.Page.ShouldBe(2);
        result.Value.Total.ShouldBe(11);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" a ")]
    public async Task SearchNeedsAtLeastTwoCharacters(string text)
    {
        var result = await new SearchNotesHandler(_reader, _options).Handle(new SearchNotesQuery(text), Ct);

        result.Status.ShouldBe(ResultStatus.Invalid);
        await _reader.DidNotReceiveWithAnyArgs().ListAsync(default(ISpecification<Note>)!, Ct);
    }
}
