using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;

namespace soft.square.net.WebApi.Modules.ModuleName.UnitTests.Core.NoteAggregate;

public class NoteRules
{
    private static readonly DateTimeOffset Now = TestClock.Start;

    [Fact]
    public void CreatesANoteAtVersionZero()
    {
        var note = Note.Create("user-1", "  A title  ", null, Now);

        note.Title.ShouldBe("A title");
        note.Body.ShouldBe("");
        note.CreatedBy.ShouldBe("user-1");
        note.CreatedAt.ShouldBe(Now);
        note.Version.ShouldBe(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NeedsATitle(string title) =>
        Should.Throw<DomainException>(() => Note.Create("user-1", title, "body", Now));

    [Fact]
    public void RejectsATitleThatIsTooLong() =>
        Should.Throw<DomainException>(() => Note.Create("user-1", new string('x', Note.TitleMaxLength + 1), "body", Now));

    [Fact]
    public void RejectsABodyThatIsTooLong() =>
        Should.Throw<DomainException>(() => Note.Create("user-1", "Title", new string('x', Note.BodyMaxLength + 1), Now));

    [Fact]
    public void NeedsAnAuthor() =>
        Should.Throw<DomainException>(() => Note.Create(" ", "Title", "body", Now));

    [Fact]
    public void AnEditChangesTheTextAndBumpsTheVersion()
    {
        var note = Note.Create("user-1", "Title", "body", Now);

        note.Edit("New title", "new body", Now.AddMinutes(5));

        note.Title.ShouldBe("New title");
        note.Body.ShouldBe("new body");
        note.UpdatedAt.ShouldBe(Now.AddMinutes(5));
        note.Version.ShouldBe(1);
    }

    [Fact]
    public void AnInvalidEditChangesNothing()
    {
        var note = Note.Create("user-1", "Title", "body", Now);

        Should.Throw<DomainException>(() => note.Edit("", "other", Now));

        note.Title.ShouldBe("Title");
        note.Version.ShouldBe(0);
    }
}
