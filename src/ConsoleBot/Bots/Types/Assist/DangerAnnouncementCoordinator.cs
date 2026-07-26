using System;

namespace ConsoleBot.Bots.Types.Assist;

public class DangerAnnouncementCoordinator
{
    private readonly object _sync = new();
    private readonly Func<DateTimeOffset> _nowProvider;
    private DateTimeOffset? _lastAnnouncement;

    public DangerAnnouncementCoordinator(Func<DateTimeOffset> nowProvider)
    {
        _nowProvider = nowProvider ?? throw new ArgumentNullException(nameof(nowProvider));
    }

    public bool TryClaimAnnouncement(TimeSpan cooldown)
    {
        var now = _nowProvider();
        lock (_sync)
        {
            if (_lastAnnouncement.HasValue && now - _lastAnnouncement.Value < cooldown)
            {
                return false;
            }

            _lastAnnouncement = now;
            return true;
        }
    }
}
