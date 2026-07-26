using ConsoleBot.Bots.Types.Assist;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ConsoleBot.Tests.Bots.Types.Assist;

public class DangerAnnouncementCoordinatorTests
{
    [Fact]
    public void TryClaimAnnouncement_ReturnsFalse_InsideCooldown_AfterSuccessfulClaim()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var coordinator = new DangerAnnouncementCoordinator(() => now);
        var cooldown = TimeSpan.FromSeconds(5);

        var first = coordinator.TryClaimAnnouncement(cooldown);
        var second = coordinator.TryClaimAnnouncement(cooldown);

        Assert.True(first);
        Assert.False(second);
    }

    [Fact]
    public void TryClaimAnnouncement_ReturnsTrue_AfterCooldownElapsed()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var coordinator = new DangerAnnouncementCoordinator(() => now);
        var cooldown = TimeSpan.FromSeconds(5);

        Assert.True(coordinator.TryClaimAnnouncement(cooldown));

        now = now.AddSeconds(5);

        Assert.True(coordinator.TryClaimAnnouncement(cooldown));
    }

    [Fact]
    public async Task TryClaimAnnouncement_AllowsExactlyOneWinner_WhenCalledConcurrentlyWithinCooldown()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var coordinator = new DangerAnnouncementCoordinator(() => now);
        var cooldown = TimeSpan.FromSeconds(5);
        var startGate = new ManualResetEventSlim(false);
        const int callerCount = 40;

        var tasks = Enumerable.Range(0, callerCount)
            .Select(_ => Task.Run(() =>
            {
                startGate.Wait();
                return coordinator.TryClaimAnnouncement(cooldown);
            }))
            .ToArray();

        startGate.Set();

        var results = await Task.WhenAll(tasks);

        Assert.Equal(1, results.Count(r => r));
        Assert.Equal(callerCount - 1, results.Count(r => !r));
    }
}
