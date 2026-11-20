using Microsoft.EntityFrameworkCore;
using soft.square.net.WebApi.Data;
using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;
using soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate.Specifications;

namespace soft.square.net.WebApi.Modules.ModuleName.IntegrationTests.Data;

/// <summary>The module's mapping and specifications through the platform's repository, on a real database.</summary>
public sealed class EfRepositoryNotes : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 9, 0, 0, TimeSpan.Zero);
    private readonly ModuleNameDatabase _database = new();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public void Dispose() => _database.Dispose();

    private async Task<Note> AddAsync(string title, string body = "", string tenant = "acme")
    {
        await using var context = _database.For(tenant);
        return await new EfRepository<Note>(context).AddAsync(Note.Create("user-1", title, body, Now), Ct);
    }

    [Fact]
    public async Task StoresANoteForItsTenantOnly()
    {
        var saved = await AddAsync("Shopping list");

        await using (var acme = _database.For("acme"))
        {
            var note = await new EfRepository<Note>(acme).GetByIdAsync(saved.Id, Ct);
            note.ShouldNotBeNull();
            note.TenantId.ShouldBe("acme");
        }
        await using (var globex = _database.For("globex"))
            (await new EfRepository<Note>(globex).GetByIdAsync(saved.Id, Ct)).ShouldBeNull();
    }

    [Fact]
    public async Task PagesNewestFirstAndCountsAll()
    {
        for (var i = 1; i <= 5; i++) await AddAsync($"Note {i}");
        await AddAsync("Other tenant", tenant: "globex");

        await using var context = _database.For("acme");
        var notes = new EfRepository<Note>(context);
        var spec = new NotesPageSpec(page: 2, pageSize: 2);

        (await notes.ListAsync(spec, Ct)).Select(n => n.Title).ShouldBe(["Note 3", "Note 2"]);
        (await notes.CountAsync(spec, Ct)).ShouldBe(5);
    }

    [Fact]
    public async Task SearchesTitleAndBodyIgnoringCase()
    {
        await AddAsync("Groceries", "milk, BREAD");
        await AddAsync("Bread recipe");
        await AddAsync("Holidays", "beach");

        await using var context = _database.For("acme");
        var notes = new EfRepository<Note>(context);
        var spec = new NoteSearchSpec("bread", page: 1, pageSize: 10);

        (await notes.ListAsync(spec, Ct)).Select(n => n.Title).ShouldBe(["Bread recipe", "Groceries"]);
        (await notes.CountAsync(spec, Ct)).ShouldBe(2);
    }

    [Fact]
    public async Task TheVersionIsAConcurrencyToken()
    {
        var saved = await AddAsync("Title");

        await using var first = _database.For("acme");
        await using var second = _database.For("acme");
        var mine = await new EfRepository<Note>(first).FirstOrDefaultAsync(new NoteByIdSpec(saved.Id), Ct);
        var theirs = await new EfRepository<Note>(second).FirstOrDefaultAsync(new NoteByIdSpec(saved.Id), Ct);

        mine!.Edit("Mine", null, Now);
        await new EfRepository<Note>(first).UpdateAsync(mine, Ct);

        theirs!.Edit("Theirs", null, Now);
        await Should.ThrowAsync<DbUpdateConcurrencyException>(() => new EfRepository<Note>(second).UpdateAsync(theirs, Ct));
    }

    [Fact]
    public async Task DeletesANote()
    {
        var saved = await AddAsync("Title");

        await using (var context = _database.For("acme"))
        {
            var notes = new EfRepository<Note>(context);
            await notes.DeleteAsync((await notes.GetByIdAsync(saved.Id, Ct))!, Ct);
        }
        await using (var context = _database.For("acme"))
            (await new EfRepository<Note>(context).CountAsync(Ct)).ShouldBe(0);
    }
}
