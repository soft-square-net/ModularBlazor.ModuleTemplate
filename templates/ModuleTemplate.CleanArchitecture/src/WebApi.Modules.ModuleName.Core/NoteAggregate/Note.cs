namespace soft.square.net.WebApi.Modules.ModuleName.Core.NoteAggregate;

/// <summary>
/// The example aggregate: a note with a title and a body, stored per tenant. Its rules live here and throw
/// <see cref="DomainException"/> (the endpoints answer 409); state changes only through its methods. Replace it with
/// your module's own aggregates: one folder per aggregate, with its specifications.
/// </summary>
public sealed class Note : AggregateRoot<int>, ITenantOwned
{
    public const int TitleMaxLength = 200;
    public const int BodyMaxLength = 2000;

    private Note() { } // EF Core

    /// <summary>Set by the platform's TenantDbContext when the note is saved.</summary>
    public string TenantId { get; set; } = "";
    public string Title { get; private set; } = "";
    public string Body { get; private set; } = "";
    public string CreatedBy { get; private set; } = "";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Bumped by every change; a concurrency token, so a stale edit fails instead of overwriting.</summary>
    public int Version { get; private set; }

    public static Note Create(string createdBy, string title, string? body, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(createdBy)) throw new DomainException("A note needs its author.");
        var note = new Note { CreatedBy = createdBy, CreatedAt = now, UpdatedAt = now };
        note.SetText(title, body);
        return note;
    }

    public void Edit(string title, string? body, DateTimeOffset now)
    {
        SetText(title, body);
        UpdatedAt = now;
        Version++;
    }

    private void SetText(string title, string? body)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > TitleMaxLength)
            throw new DomainException($"A note needs a title (at most {TitleMaxLength} characters).");
        if (body is { Length: > BodyMaxLength })
            throw new DomainException($"A note's body is at most {BodyMaxLength} characters.");
        Title = title.Trim();
        Body = body ?? "";
    }
}
