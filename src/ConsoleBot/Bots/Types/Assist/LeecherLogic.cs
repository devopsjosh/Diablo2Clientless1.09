using System;

namespace ConsoleBot.Bots.Types.Assist;

public static class LeecherLogic
{
    public enum RetreatPortalSource
    {
        LeaderPortal,
        OwnPortal
    }

    public static bool IsSafeCommand(string message, string senderCharacterName, string leadCharacterName, string safeCommand)
    {
        return string.Equals(senderCharacterName, leadCharacterName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(message, safeCommand, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsNextGameCommand(string message, string senderCharacterName, string leadCharacterName)
    {
        return string.Equals(senderCharacterName, leadCharacterName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(message, "ng", StringComparison.OrdinalIgnoreCase);
    }

    public static bool ShouldHandleSafeCommand(bool isLeecher, string message, string senderCharacterName, string leadCharacterName, string safeCommand)
    {
        return isLeecher && IsSafeCommand(message, senderCharacterName, leadCharacterName, safeCommand);
    }

    public static RetreatPortalSource[] GetRetreatPortalAttemptOrder(bool isInTown, bool hasLeadPlayer)
    {
        if (isInTown)
        {
            return [];
        }

        if (hasLeadPlayer)
        {
            return [RetreatPortalSource.LeaderPortal, RetreatPortalSource.OwnPortal];
        }

        return [RetreatPortalSource.OwnPortal];
    }

    public static bool ShouldRetreat(bool monsterInDanger, bool leaderInSameZone)
    {
        return monsterInDanger || !leaderInSameZone;
    }

    public static bool ShouldCorrectEntryLocationDrift(double driftDistance, int threshold)
    {
        return driftDistance > threshold;
    }

    public static bool ShouldAbortEnteringLeaderZone(TimeSpan elapsedInState, TimeSpan timeout)
    {
        return elapsedInState >= timeout;
    }

    public static void ResetPerCycleStateForWaitingForSafe(AssistBotClientState state)
    {
        state.SafeSignalReceived = false;
        state.EntryLocation = null;
        state.DangerAnnouncementHandledForCurrentRetreat = false;
        state.EnteringLeaderZoneStartedAt = null;
    }

    public static LeecherState GetNextLeecherState(
        LeecherState current,
        bool safeSignalReceived,
        bool leaderInSameZone,
        bool monsterInDanger,
        bool portalActionSucceeded)
    {
        return current switch
        {
            LeecherState.WaitingForSafe when safeSignalReceived && leaderInSameZone => LeecherState.StationaryInZone,
            LeecherState.WaitingForSafe when safeSignalReceived => LeecherState.EnteringLeaderZone,
            LeecherState.WaitingForSafe => LeecherState.WaitingForSafe,
            LeecherState.EnteringLeaderZone when leaderInSameZone => LeecherState.StationaryInZone,
            LeecherState.EnteringLeaderZone => LeecherState.EnteringLeaderZone,
            LeecherState.StationaryInZone when ShouldRetreat(monsterInDanger, leaderInSameZone) => LeecherState.RetreatingToTown,
            LeecherState.StationaryInZone => LeecherState.StationaryInZone,
            LeecherState.RetreatingToTown when portalActionSucceeded => LeecherState.WaitingForSafe,
            LeecherState.RetreatingToTown => LeecherState.RetreatingToTown,
            _ => LeecherState.WaitingForSafe
        };
    }
}
