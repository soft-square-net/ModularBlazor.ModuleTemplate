using System.Globalization;
using soft.square.net.WebApi.Contracts.Live;

namespace soft.square.net.WebApi.Modules.ModuleName.UseCases;

/// <summary>
/// The module's integration events and live topics, in one place.
/// <list type="bullet">
/// <item>Event types are <c>&lt;module&gt;.&lt;event&gt;.v&lt;n&gt;</c>. Never change a published version's data: add a
/// <c>v2</c> next to it.</item>
/// <item>Other modules subscribe with their own copy of the data record; nothing here is shared with them.</item>
/// <item>The patterns this module subscribes to are listed twice: in the manifest (<see cref="Subscriptions"/>, read by
/// the host at install to bind the module's queue) and in <c>ServiceConfigs</c> (the handlers).</item>
/// <item>Live topics are <c>modulename:&lt;kind&gt;:&lt;key&gt;</c>; the manifest declares each kind with the
/// permission a subscriber needs (<see cref="LiveTopicKinds"/>).</item>
/// </list>
/// </summary>
public static class ModuleNameEvents
{
    /// <summary>Published when a note is edited, in the same save as the change (see <c>EditNoteHandler</c>).</summary>
    public const string NoteEdited = "modulename.note-edited.v1";

    /// <summary>Every note event of this module ('*' matches inside one word).</summary>
    public const string AnyNoteEvent = "modulename.note-*.v1";

    /// <summary>The patterns of the events this module handles (the manifest's <c>Subscriptions</c>).</summary>
    public static IReadOnlyList<string> Subscriptions { get; } = [AnyNoteEvent];

    /// <summary>What an event is about: <c>note:{id}</c>.</summary>
    public static string NoteSubject(int id) => $"note:{id.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>The live topic kind of one note: <c>modulename:note:{id}</c>.</summary>
    public const string NoteTopicKind = "note";

    /// <summary>The live topic kinds this module offers, with the permission a subscriber needs (the manifest's <c>LiveTopics</c>).</summary>
    public static IReadOnlyList<LiveTopic> LiveTopicKinds { get; } = [new(NoteTopicKind, ModuleNamePermissions.Read)];

    /// <summary>The live topic of one note; a browser subscribes to the same text through <c>ILiveChannel</c>.</summary>
    public static string NoteTopic(int id) => LiveTopics.Of("modulename", NoteTopicKind, id.ToString(CultureInfo.InvariantCulture));
}

/// <summary>The data of <see cref="ModuleNameEvents.NoteEdited"/>.</summary>
public sealed record NoteEdited(int Id, int Version, string Title);
