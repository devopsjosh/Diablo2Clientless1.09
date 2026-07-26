using ConsoleBot.TownManagement;
using D2NG.Core;
using D2NG.Core.D2GS.Act;
using D2NG.Core.D2GS.Players;
using System.Threading.Tasks;

namespace ConsoleBot.Bots.Types.Assist;

public static class LeecherRetreatOrchestrator
{
    public static async Task<bool> RetreatToTown(ITownManagementService townManagementService, Client client, Player leadPlayer, Area currentArea, bool isInTown)
    {
        if (isInTown)
        {
            return true;
        }

        var retreatOrder = LeecherLogic.GetRetreatPortalAttemptOrder(isInTown, leadPlayer != null);
        foreach (var portalSource in retreatOrder)
        {
            if (portalSource == LeecherLogic.RetreatPortalSource.LeaderPortal
                && leadPlayer != null
                && await townManagementService.TakeTownPortalToArea(client, leadPlayer, currentArea))
            {
                return true;
            }

            if (portalSource == LeecherLogic.RetreatPortalSource.OwnPortal
                && await townManagementService.TakeTownPortalToTown(client))
            {
                return true;
            }
        }

        return false;
    }
}
