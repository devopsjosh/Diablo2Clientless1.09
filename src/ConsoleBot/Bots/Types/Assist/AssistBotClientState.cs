using D2NG.Core.D2GS.Objects;
using D2NG.Core.D2GS;
using System;

namespace ConsoleBot.Bots.Types.Assist;

public class AssistBotClientState
{
    public bool ShouldHeal { get; set; }
    public bool ShouldGoToTown { get; set; }
    public bool NextGame { get; set; }
    public bool ShouldStop { get; set; }
    public bool ShouldFollow { get; set; } = true;
    public bool GoNextLevel { get; set; }
    public Waypoint? GoToWaypoint { get; set; }
    public LeecherState LeecherState { get; set; } = LeecherState.WaitingForSafe;
    public Point EntryLocation { get; set; }
    public bool SafeSignalReceived { get; set; }
    public bool DangerAnnouncementHandledForCurrentRetreat { get; set; }
    public DateTimeOffset? EnteringLeaderZoneStartedAt { get; set; }
}
