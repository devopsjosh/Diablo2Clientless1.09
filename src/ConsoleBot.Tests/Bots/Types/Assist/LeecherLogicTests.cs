using ConsoleBot.Bots.Types.Assist;
using D2NG.Core.D2GS;
using System;
using Xunit;

namespace ConsoleBot.Tests.Bots.Types.Assist;

public class LeecherLogicTests
{
    [Fact]
    public void IsSafeCommand_ReturnsTrue_ForExactMessageFromLeader_IgnoringCase()
    {
        var result = LeecherLogic.IsSafeCommand("SaFe", "LeadChar", "leadchar", "safe");

        Assert.True(result);
    }

    [Fact]
    public void IsSafeCommand_ReturnsFalse_ForExactMessageFromNonLeader()
    {
        var result = LeecherLogic.IsSafeCommand("safe", "RandomPlayer", "LeadChar", "safe");

        Assert.False(result);
    }

    [Fact]
    public void IsSafeCommand_ReturnsFalse_ForNonExactMessageFromLeader()
    {
        var result = LeecherLogic.IsSafeCommand("safe now", "LeadChar", "LeadChar", "safe");

        Assert.False(result);
    }

    [Fact]
    public void IsNextGameCommand_ReturnsTrue_ForExactMessageFromLeader_IgnoringCase()
    {
        var result = LeecherLogic.IsNextGameCommand("NG", "LeadChar", "leadchar");

        Assert.True(result);
    }

    [Fact]
    public void IsNextGameCommand_ReturnsFalse_ForExactMessageFromNonLeader()
    {
        var result = LeecherLogic.IsNextGameCommand("ng", "HostLeecher", "LeadChar");

        Assert.False(result);
    }

    [Fact]
    public void IsNextGameCommand_ReturnsFalse_WhenSentByHostAccountThatIsNotLead()
    {
        var result = LeecherLogic.IsNextGameCommand("ng", "HostChar", "LeadChar");

        Assert.False(result);
    }

    [Fact]
    public void IsNextGameCommand_ReturnsFalse_ForNonExactMessageFromLeader()
    {
        var result = LeecherLogic.IsNextGameCommand("ng please", "LeadChar", "LeadChar");

        Assert.False(result);
    }

    [Fact]
    public void IsNextGameCommand_ReturnsFalse_ForDifferentCommandFromLeader()
    {
        var result = LeecherLogic.IsNextGameCommand("safe", "LeadChar", "LeadChar");

        Assert.False(result);
    }

    [Fact]
    public void IsNextGameCommand_ReturnsFalse_WhenSenderCharacterNameIsNull()
    {
        var result = LeecherLogic.IsNextGameCommand("ng", senderCharacterName: null, leadCharacterName: "LeadChar");

        Assert.False(result);
    }

    [Fact]
    public void IsNextGameCommand_ReturnsFalse_WhenMessageIsNull()
    {
        var result = LeecherLogic.IsNextGameCommand(message: null, senderCharacterName: "LeadChar", leadCharacterName: "LeadChar");

        Assert.False(result);
    }

    [Fact]
    public void IsNextGameCommand_ReturnsFalse_WhenSenderCharacterNameIsEmpty()
    {
        var result = LeecherLogic.IsNextGameCommand("ng", senderCharacterName: string.Empty, leadCharacterName: "LeadChar");

        Assert.False(result);
    }

    [Fact]
    public void IsNextGameCommand_ReturnsFalse_WhenMessageHasSurroundingWhitespace()
    {
        var result = LeecherLogic.IsNextGameCommand(" ng ", senderCharacterName: "LeadChar", leadCharacterName: "LeadChar");

        Assert.False(result);
    }

    [Fact]
    public void ShouldHandleSafeCommand_ReturnsTrue_WhenLeecherAndSafeCommandFromLeader()
    {
        var result = LeecherLogic.ShouldHandleSafeCommand(
            isLeecher: true,
            message: "safe",
            senderCharacterName: "LeadChar",
            leadCharacterName: "leadchar",
            safeCommand: "SaFe");

        Assert.True(result);
    }

    [Fact]
    public void ShouldHandleSafeCommand_ReturnsFalse_WhenNotLeecherEvenIfSafeCommandFromLeader()
    {
        var result = LeecherLogic.ShouldHandleSafeCommand(
            isLeecher: false,
            message: "safe",
            senderCharacterName: "LeadChar",
            leadCharacterName: "LeadChar",
            safeCommand: "safe");

        Assert.False(result);
    }

    [Fact]
    public void GetRetreatPortalAttemptOrder_ReturnsNoAttempts_WhenAlreadyInTown()
    {
        var result = LeecherLogic.GetRetreatPortalAttemptOrder(isInTown: true, hasLeadPlayer: true);

        Assert.Empty(result);
    }

    [Fact]
    public void GetRetreatPortalAttemptOrder_UsesLeaderThenOwnPortal_WhenOutOfTownAndLeaderIsKnown()
    {
        var result = LeecherLogic.GetRetreatPortalAttemptOrder(isInTown: false, hasLeadPlayer: true);

        Assert.Equal(
            [LeecherLogic.RetreatPortalSource.LeaderPortal, LeecherLogic.RetreatPortalSource.OwnPortal],
            result);
    }

    [Fact]
    public void GetRetreatPortalAttemptOrder_UsesOwnPortalOnly_WhenOutOfTownAndLeaderIsUnknown()
    {
        var result = LeecherLogic.GetRetreatPortalAttemptOrder(isInTown: false, hasLeadPlayer: false);

        Assert.Equal([LeecherLogic.RetreatPortalSource.OwnPortal], result);
    }

    [Fact]
    public void ShouldRetreat_ReturnsTrue_WhenMonsterIsInDangerRange()
    {
        Assert.True(LeecherLogic.ShouldRetreat(monsterInDanger: true, leaderInSameZone: true));
    }

    [Fact]
    public void ShouldRetreat_ReturnsTrue_WhenLeaderLeavesZone()
    {
        Assert.True(LeecherLogic.ShouldRetreat(monsterInDanger: false, leaderInSameZone: false));
    }

    [Fact]
    public void ShouldRetreat_ReturnsFalse_WhenNoDangerAndLeaderInSameZone()
    {
        Assert.False(LeecherLogic.ShouldRetreat(monsterInDanger: false, leaderInSameZone: true));
    }

    [Fact]
    public void ShouldCorrectEntryLocationDrift_ReturnsFalse_WhenWithinThreshold()
    {
        var result = LeecherLogic.ShouldCorrectEntryLocationDrift(driftDistance: 5.0, threshold: 5);

        Assert.False(result);
    }

    [Fact]
    public void ShouldCorrectEntryLocationDrift_ReturnsTrue_WhenBeyondThreshold()
    {
        var result = LeecherLogic.ShouldCorrectEntryLocationDrift(driftDistance: 5.01, threshold: 5);

        Assert.True(result);
    }

    [Fact]
    public void ShouldAbortEnteringLeaderZone_ReturnsFalse_BeforeTimeout()
    {
        var result = LeecherLogic.ShouldAbortEnteringLeaderZone(
            elapsedInState: TimeSpan.FromSeconds(29),
            timeout: TimeSpan.FromSeconds(30));

        Assert.False(result);
    }

    [Fact]
    public void ShouldAbortEnteringLeaderZone_ReturnsTrue_AtOrAfterTimeout()
    {
        var resultAtTimeout = LeecherLogic.ShouldAbortEnteringLeaderZone(
            elapsedInState: TimeSpan.FromSeconds(30),
            timeout: TimeSpan.FromSeconds(30));
        var resultAfterTimeout = LeecherLogic.ShouldAbortEnteringLeaderZone(
            elapsedInState: TimeSpan.FromSeconds(31),
            timeout: TimeSpan.FromSeconds(30));

        Assert.True(resultAtTimeout);
        Assert.True(resultAfterTimeout);
    }

    [Fact]
    public void ResetPerCycleStateForWaitingForSafe_ClearsAllPerCycleState()
    {
        var state = new AssistBotClientState
        {
            SafeSignalReceived = true,
            EntryLocation = new Point(10, 20),
            DangerAnnouncementHandledForCurrentRetreat = true,
            EnteringLeaderZoneStartedAt = DateTimeOffset.UtcNow
        };

        LeecherLogic.ResetPerCycleStateForWaitingForSafe(state);

        Assert.False(state.SafeSignalReceived);
        Assert.Null(state.EntryLocation);
        Assert.False(state.DangerAnnouncementHandledForCurrentRetreat);
        Assert.Null(state.EnteringLeaderZoneStartedAt);
    }

    [Fact]
    public void GetNextLeecherState_WaitingForSafe_GoesToEnteringLeaderZone_WhenSafeReceivedAndNotInLeaderZone()
    {
        var next = LeecherLogic.GetNextLeecherState(
            current: LeecherState.WaitingForSafe,
            safeSignalReceived: true,
            leaderInSameZone: false,
            monsterInDanger: false,
            portalActionSucceeded: false);

        Assert.Equal(LeecherState.EnteringLeaderZone, next);
    }

    [Fact]
    public void GetNextLeecherState_WaitingForSafe_GoesDirectlyToStationaryInZone_WhenAlreadyInLeaderZone()
    {
        var next = LeecherLogic.GetNextLeecherState(
            current: LeecherState.WaitingForSafe,
            safeSignalReceived: true,
            leaderInSameZone: true,
            monsterInDanger: false,
            portalActionSucceeded: false);

        Assert.Equal(LeecherState.StationaryInZone, next);
    }

    [Fact]
    public void GetNextLeecherState_EnteringLeaderZone_GoesToStationaryInZone_WhenLeaderZoneReached()
    {
        var next = LeecherLogic.GetNextLeecherState(
            current: LeecherState.EnteringLeaderZone,
            safeSignalReceived: false,
            leaderInSameZone: true,
            monsterInDanger: false,
            portalActionSucceeded: false);

        Assert.Equal(LeecherState.StationaryInZone, next);
    }

    [Fact]
    public void GetNextLeecherState_EnteringLeaderZone_StaysEnteringLeaderZone_WhenLeaderZoneNotReached()
    {
        var next = LeecherLogic.GetNextLeecherState(
            current: LeecherState.EnteringLeaderZone,
            safeSignalReceived: false,
            leaderInSameZone: false,
            monsterInDanger: true,
            portalActionSucceeded: false);

        Assert.Equal(LeecherState.EnteringLeaderZone, next);
    }

    [Fact]
    public void GetNextLeecherState_StationaryInZone_GoesToRetreatingToTown_WhenMonsterDangerDetected()
    {
        var next = LeecherLogic.GetNextLeecherState(
            current: LeecherState.StationaryInZone,
            safeSignalReceived: false,
            leaderInSameZone: true,
            monsterInDanger: true,
            portalActionSucceeded: false);

        Assert.Equal(LeecherState.RetreatingToTown, next);
    }

    [Fact]
    public void GetNextLeecherState_StationaryInZone_GoesToRetreatingToTown_WhenLeaderLeavesZone()
    {
        var next = LeecherLogic.GetNextLeecherState(
            current: LeecherState.StationaryInZone,
            safeSignalReceived: false,
            leaderInSameZone: false,
            monsterInDanger: false,
            portalActionSucceeded: false);

        Assert.Equal(LeecherState.RetreatingToTown, next);
    }

    [Fact]
    public void GetNextLeecherState_RetreatingToTown_ReturnsToWaitingForSafe_WhenPortalActionSucceeded()
    {
        var next = LeecherLogic.GetNextLeecherState(
            current: LeecherState.RetreatingToTown,
            safeSignalReceived: false,
            leaderInSameZone: false,
            monsterInDanger: false,
            portalActionSucceeded: true);

        Assert.Equal(LeecherState.WaitingForSafe, next);
    }

    [Fact]
    public void GetNextLeecherState_RetreatingToTown_StaysRetreating_WhenPortalActionDidNotSucceed()
    {
        var next = LeecherLogic.GetNextLeecherState(
            current: LeecherState.RetreatingToTown,
            safeSignalReceived: false,
            leaderInSameZone: false,
            monsterInDanger: false,
            portalActionSucceeded: false);

        Assert.Equal(LeecherState.RetreatingToTown, next);
    }
}
