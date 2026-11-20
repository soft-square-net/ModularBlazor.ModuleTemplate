namespace soft.square.net.WebApi.Modules.ModuleName.UnitTests;

/// <summary>A clock that stands still until a test moves it.</summary>
public sealed class TestClock(DateTimeOffset now) : TimeProvider
{
    public static readonly DateTimeOffset Start = new(2026, 11, 20, 9, 0, 0, TimeSpan.Zero);

    public DateTimeOffset Now { get; private set; } = now;

    public TestClock() : this(Start) { }

    public void Advance(TimeSpan by) => Now += by;

    public override DateTimeOffset GetUtcNow() => Now;
}
