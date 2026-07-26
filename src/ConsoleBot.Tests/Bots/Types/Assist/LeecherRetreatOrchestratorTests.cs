using ConsoleBot.Bots.Types.Assist;
using ConsoleBot.TownManagement;
using D2NG.Core;
using D2NG.Core.D2GS.Act;
using D2NG.Core.D2GS.Players;
using Moq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace ConsoleBot.Tests.Bots.Types.Assist;

public class LeecherRetreatOrchestratorTests
{
    [Fact]
    public async Task RetreatToTown_ReturnsTrueImmediately_AndDoesNotCallTownManagementService_WhenAlreadyInTown()
    {
        var townManagementService = new Mock<ITownManagementService>(MockBehavior.Strict);
        var client = new Client();

        var result = await LeecherRetreatOrchestrator.RetreatToTown(
            townManagementService.Object,
            client,
            leadPlayer: null,
            currentArea: Area.BloodMoor,
            isInTown: true);

        Assert.True(result);
        townManagementService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RetreatToTown_TriesLeaderPortalFirst_WhenLeaderKnown_AndSucceeds()
    {
        var townManagementService = new Mock<ITownManagementService>(MockBehavior.Strict);
        var client = new Client();
        var leadPlayer = CreateUninitializedPlayer();

        townManagementService
            .Setup(s => s.TakeTownPortalToArea(client, leadPlayer, Area.BloodMoor))
            .ReturnsAsync(true);

        var result = await LeecherRetreatOrchestrator.RetreatToTown(
            townManagementService.Object,
            client,
            leadPlayer,
            Area.BloodMoor,
            isInTown: false);

        Assert.True(result);
        townManagementService.Verify(s => s.TakeTownPortalToArea(client, leadPlayer, Area.BloodMoor), Times.Once);
        townManagementService.Verify(s => s.TakeTownPortalToTown(client), Times.Never);
        townManagementService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RetreatToTown_FallsBackToOwnPortal_WhenLeaderPortalFails()
    {
        var townManagementService = new Mock<ITownManagementService>(MockBehavior.Strict);
        var client = new Client();
        var leadPlayer = CreateUninitializedPlayer();

        townManagementService
            .Setup(s => s.TakeTownPortalToArea(client, leadPlayer, Area.BloodMoor))
            .ReturnsAsync(false);
        townManagementService
            .Setup(s => s.TakeTownPortalToTown(client))
            .ReturnsAsync(true);

        var result = await LeecherRetreatOrchestrator.RetreatToTown(
            townManagementService.Object,
            client,
            leadPlayer,
            Area.BloodMoor,
            isInTown: false);

        Assert.True(result);
        townManagementService.Verify(s => s.TakeTownPortalToArea(client, leadPlayer, Area.BloodMoor), Times.Once);
        townManagementService.Verify(s => s.TakeTownPortalToTown(client), Times.Once);
        townManagementService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RetreatToTown_ReturnsFalse_WhenBothPortalAttemptsFail()
    {
        var townManagementService = new Mock<ITownManagementService>(MockBehavior.Strict);
        var client = new Client();
        var leadPlayer = CreateUninitializedPlayer();

        townManagementService
            .Setup(s => s.TakeTownPortalToArea(client, leadPlayer, Area.BloodMoor))
            .ReturnsAsync(false);
        townManagementService
            .Setup(s => s.TakeTownPortalToTown(client))
            .ReturnsAsync(false);

        var result = await LeecherRetreatOrchestrator.RetreatToTown(
            townManagementService.Object,
            client,
            leadPlayer,
            Area.BloodMoor,
            isInTown: false);

        Assert.False(result);
        townManagementService.Verify(s => s.TakeTownPortalToArea(client, leadPlayer, Area.BloodMoor), Times.Once);
        townManagementService.Verify(s => s.TakeTownPortalToTown(client), Times.Once);
        townManagementService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RetreatToTown_SkipsLeaderPortal_AndUsesOwnPortalOnly_WhenLeadPlayerIsNull()
    {
        var townManagementService = new Mock<ITownManagementService>(MockBehavior.Strict);
        var client = new Client();

        townManagementService
            .Setup(s => s.TakeTownPortalToTown(client))
            .ReturnsAsync(true);

        var result = await LeecherRetreatOrchestrator.RetreatToTown(
            townManagementService.Object,
            client,
            leadPlayer: null,
            Area.BloodMoor,
            isInTown: false);

        Assert.True(result);
        townManagementService.Verify(s => s.TakeTownPortalToTown(client), Times.Once);
        townManagementService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RetreatToTown_ReturnsFalse_WhenLeadPlayerIsNullAndOwnPortalFails()
    {
        var townManagementService = new Mock<ITownManagementService>(MockBehavior.Strict);
        var client = new Client();

        townManagementService
            .Setup(s => s.TakeTownPortalToTown(client))
            .ReturnsAsync(false);

        var result = await LeecherRetreatOrchestrator.RetreatToTown(
            townManagementService.Object,
            client,
            leadPlayer: null,
            Area.BloodMoor,
            isInTown: false);

        Assert.False(result);
        townManagementService.Verify(s => s.TakeTownPortalToTown(client), Times.Once);
        townManagementService.VerifyNoOtherCalls();
    }

    private static Player CreateUninitializedPlayer()
    {
        return (Player)RuntimeHelpers.GetUninitializedObject(typeof(Player));
    }
}
