using Imposter.Abstractions;
using ShieldCommander.Core.Services;
using ShieldCommander.Core.Services.Commands;

[assembly: GenerateImposter(typeof(IAdbRunner))]

namespace ShieldCommander.Core.Tests.Commands;

public class TerminateProcessCommandTests
{
    [Test]
    public async Task ForceStop_Succeeds_DoesNotFallBackToKill()
    {
        var imposter = IAdbRunner.Imposter();
        imposter.RunShellAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
                .Returns(Task.FromResult("EXIT:0"));

        var runner = imposter.Instance();

        var command = new TerminateProcessCommand(123, "com.example.app");
        var result = await command.ExecuteAsync(runner);

        await Assert.That(result.Success).IsTrue();
        imposter.RunShellAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
                .Called(Count.Once());
    }

    [Test]
    public async Task ForceStop_Fails_FallsBackToKill_Succeeds()
    {
        var imposter = IAdbRunner.Imposter();
        imposter.RunShellAsync(Arg<string>.Is(s => s.Contains("force-stop")), Arg<CancellationToken>.Any())
                .Returns(Task.FromResult("EXIT:1"));

        imposter.RunShellAsync(Arg<string>.Is(s => s.Contains("kill -9")), Arg<CancellationToken>.Any())
                .Returns(Task.FromResult("EXIT:0"));

        var runner = imposter.Instance();
        var command = new TerminateProcessCommand(123, "com.example.app");
        var result = await command.ExecuteAsync(runner);

        await Assert.That(result.Success).IsTrue();
    }

    [Test]
    public async Task ForceStop_Fails_Kill_Fails_NotSuccess()
    {
        var imposter = IAdbRunner.Imposter();
        imposter.RunShellAsync(Arg<string>.Is(s => s.Contains("force-stop")), Arg<CancellationToken>.Any())
                .Returns(Task.FromResult("EXIT:1"));

        imposter.RunShellAsync(Arg<string>.Is(s => s.Contains("kill -9")), Arg<CancellationToken>.Any())
                .Returns(Task.FromResult("EXIT:1"));

        var runner = imposter.Instance();

        var command = new TerminateProcessCommand(123, "com.example.app");
        var result = await command.ExecuteAsync(runner);

        await Assert.That(result.Success).IsFalse();
    }

    [Test]
    public async Task ForceStop_OutputContainsError_FallsBackToKill()
    {
        var imposter = IAdbRunner.Imposter();
        imposter.RunShellAsync(Arg<string>.Is(s => s.Contains("force-stop")), Arg<CancellationToken>.Any())
                .Returns(Task.FromResult("Error\nEXIT:0"));

        imposter.RunShellAsync(Arg<string>.Is(s => s.Contains("kill -9")), Arg<CancellationToken>.Any())
                .Returns(Task.FromResult("EXIT:0"));

        var runner = imposter.Instance();

        var command = new TerminateProcessCommand(123, "com.example.app");
        var result = await command.ExecuteAsync(runner);

        await Assert.That(result.Success).IsTrue();
        imposter.RunShellAsync(Arg<string>.Any(), Arg<CancellationToken>.Any())
                .Called(Count.Twice());
    }
}
