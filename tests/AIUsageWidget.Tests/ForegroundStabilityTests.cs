using AIUsageWidget.Tracking;

namespace AIUsageWidget.Tests;

public sealed class ForegroundStabilityTests
{
    private sealed class Clock : TimeProvider
    {
        public long Milliseconds;
        public override long TimestampFrequency => 1000;
        public override long GetTimestamp() => Milliseconds;
    }

    private static ForegroundInfo Terminal(nint handle = 1, uint process = 10, long generation = 1) =>
        new(ForegroundKind.Terminal, new TerminalWindow(new(0, 0, 900, 600), new(0, 0, 1920, 1080), 1)
        { Handle = handle, ProcessId = process, ForegroundGeneration = generation });

    [Fact]
    public void TransientTerminalNeverBecomesReady()
    {
        var clock = new Clock();
        var gate = new ForegroundStability(clock);
        Assert.False(gate.IsReady(Terminal()));
        clock.Milliseconds = 540;
        Assert.False(gate.IsReady(Terminal()));
        Assert.False(gate.IsReady(ForegroundInfo.Other));
        clock.Milliseconds = 2000;
        Assert.False(gate.IsReady(Terminal()));
    }

    [Fact]
    public void StableTerminalAppearsAtThresholdAndMovementDoesNotRestartDelay()
    {
        var clock = new Clock();
        var gate = new ForegroundStability(clock);
        var terminal = Terminal();
        Assert.False(gate.IsReady(terminal));
        clock.Milliseconds = 999;
        Assert.False(gate.IsReady(terminal));
        clock.Milliseconds = 1000;
        Assert.True(gate.IsReady(terminal));
        Assert.True(gate.IsReady(terminal with { Terminal = terminal.Terminal! with { Bounds = new(500, 500, 900, 600) } }));
    }

    [Theory]
    [InlineData(2, 10, 1)] // A second window, including another window in the same Terminal process.
    [InlineData(1, 11, 1)] // A reused window handle in another process.
    [InlineData(1, 10, 2)] // Focus left and returned between polling ticks.
    public void EveryNewIdentityMustWaitAgain(int handle, uint process, long generation)
    {
        var clock = new Clock();
        var gate = new ForegroundStability(clock);
        Assert.False(gate.IsReady(Terminal()));
        clock.Milliseconds = 1000;
        Assert.True(gate.IsReady(Terminal()));
        var next = Terminal(handle, process, generation);
        Assert.False(gate.IsReady(next));
        clock.Milliseconds = 1999;
        Assert.False(gate.IsReady(next));
        clock.Milliseconds = 2000;
        Assert.True(gate.IsReady(next));
    }

    [Theory]
    [InlineData(ForegroundKind.Other)]
    [InlineData(ForegroundKind.Self)]
    public void LeavingTerminalCancelsPendingOrVisibleAttachment(ForegroundKind kind)
    {
        var clock = new Clock();
        var gate = new ForegroundStability(clock);
        gate.IsReady(Terminal());
        clock.Milliseconds = 1000;
        Assert.True(gate.IsReady(Terminal()));
        Assert.False(gate.IsReady(new(kind)));
        Assert.False(gate.IsReady(Terminal()));
        clock.Milliseconds = 2000;
        Assert.True(gate.IsReady(Terminal()));
        gate.Reset();
        Assert.False(gate.IsReady(Terminal()));
    }

    [Fact]
    public void MissingWindowIdentityCannotAttach()
    {
        var gate = new ForegroundStability(new Clock());
        Assert.False(gate.IsReady(Terminal(handle: 0)));
        Assert.False(gate.IsReady(new(ForegroundKind.Terminal)));
    }
}
