using NSubstitute;
using ShieldCommander.Core.Services;
using ShieldCommander.Core.Services.Commands;

namespace ShieldCommander.Core.Tests.Commands;

public class TerminateProcessCommandTests
{
    private readonly IAdbRunner _runner = Substitute.For<IAdbRunner>();

    [Test]
    public async Task ForceStop_Succeeds_DoesNotFallBackToKill()
    {
        _runner.RunShellAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("EXIT:0");

        var command = new TerminateProcessCommand(123, "com.example.app");
        var result = await command.ExecuteAsync(_runner);

        await Assert.That(result.Success).IsTrue();
        await _runner.Received(1).RunShellAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ForceStop_Fails_FallsBackToKill_Succeeds()
    {
        _runner.RunShellAsync(Arg.Is<string>(s => s.Contains("force-stop")), Arg.Any<CancellationToken>())
            .Returns("EXIT:1");
        _runner.RunShellAsync(Arg.Is<string>(s => s.Contains("kill -9")), Arg.Any<CancellationToken>())
            .Returns("EXIT:0");

        var command = new TerminateProcessCommand(123, "com.example.app");
        var result = await command.ExecuteAsync(_runner);

        await Assert.That(result.Success).IsTrue();
    }

    [Test]
    public async Task ForceStop_Fails_Kill_Fails_NotSuccess()
    {
        _runner.RunShellAsync(Arg.Is<string>(s => s.Contains("force-stop")), Arg.Any<CancellationToken>())
            .Returns("EXIT:1");
        _runner.RunShellAsync(Arg.Is<string>(s => s.Contains("kill -9")), Arg.Any<CancellationToken>())
            .Returns("EXIT:1");

        var command = new TerminateProcessCommand(123, "com.example.app");
        var result = await command.ExecuteAsync(_runner);

        await Assert.That(result.Success).IsFalse();
    }

    [Test]
    public async Task ForceStop_OutputContainsError_FallsBackToKill()
    {
        _runner.RunShellAsync(Arg.Is<string>(s => s.Contains("force-stop")), Arg.Any<CancellationToken>())
            .Returns("Error\nEXIT:0");
        _runner.RunShellAsync(Arg.Is<string>(s => s.Contains("kill -9")), Arg.Any<CancellationToken>())
            .Returns("EXIT:0");

        var command = new TerminateProcessCommand(123, "com.example.app");
        var result = await command.ExecuteAsync(_runner);

        await Assert.That(result.Success).IsTrue();
        await _runner.Received(2).RunShellAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
