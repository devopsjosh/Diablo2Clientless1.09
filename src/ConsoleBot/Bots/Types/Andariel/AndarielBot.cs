using ConsoleBot.Clients.ExternalMessagingClient;
using ConsoleBot.Helpers;
using ConsoleBot.Mule;
using ConsoleBot.TownManagement;
using D2NG.Core;
using D2NG.Core.D2GS;
using D2NG.Core.D2GS.Act;
using D2NG.Core.D2GS.Enums;
using D2NG.Core.D2GS.Objects;
using D2NG.Core.D2GS.Players;
using D2NG.Navigation.Extensions;
using D2NG.Navigation.Services.MapApi;
using D2NG.Navigation.Services.Pathing;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleBot.Bots.Types.Andariel;

public class AndarielBot : SingleClientBotBase, IBotInstance
{
    private readonly AndarielConfiguration _andarielConfiguration;
    private readonly IPathingService _pathingService;
    private readonly IMapApiService _mapApiService;
    private readonly ITownManagementService _townManagementService;

    public AndarielBot(
        IOptions<BotConfiguration> config,
        IOptions<AndarielConfiguration> andarielConfig,
        IExternalMessagingClient externalMessagingClient,
        IPathingService pathingService,
        IMapApiService mapApiService,
        IMuleService muleService,
        ITownManagementService townManagementService) : base(config.Value, andarielConfig.Value, externalMessagingClient, muleService)
    {
        _andarielConfiguration = andarielConfig.Value;
        _pathingService = pathingService;
        _mapApiService = mapApiService;
        _townManagementService = townManagementService;
    }

    public string GetName()
    {
        return "andariel";
    }

    public async Task Run()
    {
        var client = new Client();
        _externalMessagingClient.RegisterClient(client);
        await CreateGameLoop(client);
    }

    protected override async Task<bool> RunSingleGame(Client client)
    {
        if (client.Game.Me.Class != CharacterClass.Sorceress)
        {
            throw new NotSupportedException("Only sorceress is supported on Andariel");
        }

        var townManagementOptions = new TownManagementOptions(_accountConfig, Act.Act1);
        var townTaskResult = await _townManagementService.PerformTownTasks(client, townManagementOptions);
        if (townTaskResult.ShouldMule)
        {
            NeedsMule = true;
            return true;
        }
        else if (!townTaskResult.Succes)
        {
            return false;
        }

        Log.Information("Taking CatacombsLevel2 Waypoint");
        if (!await _townManagementService.TakeWaypoint(client, Waypoint.CatacombsLevel2))
        {
            Log.Warning("Taking CatacombsLevel2 waypoint failed");
            return false;
        }

        if (!client.Game.Me.Effects.ContainsKey(EntityEffect.Thunderstorm) && client.Game.Me.HasSkill(Skill.ThunderStorm))
        {
            client.Game.UseRightHandSkillOnLocation(Skill.ThunderStorm, client.Game.Me.Location);
        }

        if (!client.Game.Me.Effects.ContainsKey(EntityEffect.Shiverarmor) && client.Game.Me.HasSkill(Skill.ShiverArmor))
        {
            client.Game.UseRightHandSkillOnLocation(Skill.ShiverArmor, client.Game.Me.Location);
        }

        if (!client.Game.Me.Effects.ContainsKey(EntityEffect.Frozenarmor) && client.Game.Me.HasSkill(Skill.FrozenArmor) && !client.Game.Me.HasSkill(Skill.ShiverArmor))
        {
            client.Game.UseRightHandSkillOnLocation(Skill.FrozenArmor, client.Game.Me.Location);
        }

        var pathToCat3 = await _pathingService.GetPathFromWaypointToArea(
            client.Game.MapId,
            Difficulty.Normal,
            Area.CatacombsLevel2,
            Waypoint.CatacombsLevel2,
            Area.CatacombsLevel3,
            MovementMode.Teleport);
        if (!await MovementHelpers.TakePathOfLocations(client.Game, pathToCat3, MovementMode.Teleport))
        {
            Log.Warning($"Teleporting to {Area.CatacombsLevel3} warp failed at location {client.Game.Me.Location}");
            return false;
        }

        var warpToCat3 = client.Game.GetNearestWarp();
        if (warpToCat3 == null)
        {
            Log.Warning($"No warp found while trying to enter {Area.CatacombsLevel3} from {client.Game.Me.Location}");
            return false;
        }

        if (!await MovementHelpers.TakeWarp(client.Game, _pathingService, _mapApiService, MovementMode.Teleport, warpToCat3, Area.CatacombsLevel3))
        {
            Log.Warning($"Taking warp to {Area.CatacombsLevel3} failed at location {client.Game.Me.Location}");
            return false;
        }

        Log.Information($"Teleporting to {Area.CatacombsLevel4}");
        var pathToCat4 = await _pathingService.GetPathToArea(client.Game, Area.CatacombsLevel4, MovementMode.Teleport);
        if (!await MovementHelpers.TakePathOfLocations(client.Game, pathToCat4, MovementMode.Teleport))
        {
            Log.Warning($"Teleporting to {Area.CatacombsLevel4} warp failed at location {client.Game.Me.Location}");
            return false;
        }

        var warpToCat4 = client.Game.GetNearestWarp();
        if (warpToCat4 == null)
        {
            Log.Warning($"No warp found while trying to enter {Area.CatacombsLevel4} from {client.Game.Me.Location}");
            return false;
        }

        if (!await MovementHelpers.TakeWarp(client.Game, _pathingService, _mapApiService, MovementMode.Teleport, warpToCat4, Area.CatacombsLevel4))
        {
            Log.Warning($"Taking warp to {Area.CatacombsLevel4} failed at location {client.Game.Me.Location}");
            return false;
        }

        if (!GeneralHelpers.TryWithTimeout((_) => client.Game.GetNPCsByCode(NPCCode.Andarial).Count > 0, TimeSpan.FromSeconds(2)))
        {
            Log.Warning($"Finding Andariel failed while at location {client.Game.Me.Location}");
            return false;
        }

        var andariel = client.Game.GetNPCsByCode(NPCCode.Andarial).OrderBy(n => n.Location.Distance(client.Game.Me.Location)).First();
        var debugEnabled = _andarielConfiguration.Debug?.Enabled == true;
        var debugEveryTicks = Math.Max(1, _andarielConfiguration.Debug?.LogEveryTicks ?? 5);
        var previousPlayerLocation = client.Game.Me.Location;
        var previousAndarielLife = andariel.LifePercentage;
        var noDamageTicks = 0;
        var stationaryTicks = 0;
        var failedDirectTeleportTicks = 0;
        var lastDirectTeleportAttempt = DateTime.MinValue;

        Log.Information("Killing Andariel");
        if (!GeneralHelpers.TryWithTimeout((retryCount) =>
            {
                if (!client.Game.IsInGame())
                {
                    return true;
                }

                if (client.Game.Me.Mana < 35 && client.Game.UseManaPotion())
                {
                    HumanizationSettings.SleepThread(120);
                }

                var distanceToAndariel = andariel.Location.Distance(client.Game.Me.Location);

                if (andariel.LifePercentage >= previousAndarielLife - 0.1)
                {
                    noDamageTicks++;
                }
                else
                {
                    noDamageTicks = 0;
                }

                if (client.Game.Me.Location.Distance(previousPlayerLocation) <= 1)
                {
                    stationaryTicks++;
                }
                else
                {
                    stationaryTicks = 0;
                }

                var usedStatic = false;
                var usedFrozenOrb = false;
                var attemptedReposition = false;
                var attemptedDirectTeleport = false;
                var usedPathFallback = false;

                if (distanceToAndariel > 28)
                {
                    var canTryDirectTeleport = DateTime.Now.Subtract(lastDirectTeleportAttempt) > TimeSpan.FromMilliseconds(700);
                    if (canTryDirectTeleport)
                    {
                        attemptedDirectTeleport = true;
                        var locationBeforeTeleport = client.Game.Me.Location;
                        client.Game.TeleportToLocation(andariel.Location);
                        HumanizationSettings.SleepThread(160);

                        var locationAfterTeleport = client.Game.Me.Location;
                        if (locationAfterTeleport.Distance(locationBeforeTeleport) <= 2)
                        {
                            failedDirectTeleportTicks++;
                        }
                        else
                        {
                            failedDirectTeleportTicks = 0;
                        }

                        lastDirectTeleportAttempt = DateTime.Now;
                    }

                    if (!attemptedDirectTeleport || failedDirectTeleportTicks >= 2)
                    {
                        var pathToAndariel = _pathingService.GetPathToLocation(client.Game, andariel.Location, MovementMode.Teleport).GetAwaiter().GetResult();
                        if (pathToAndariel.Count > 0)
                        {
                            usedPathFallback = true;
                            // Take only part of the path to keep combat loop responsive.
                            var segmentedPath = pathToAndariel.Take(Math.Min(8, pathToAndariel.Count)).ToList();
                            MovementHelpers.TakePathOfLocations(client.Game, segmentedPath, MovementMode.Teleport).GetAwaiter().GetResult();
                        }
                    }

                    if (debugEnabled)
                    {
                        Log.Information("[andariel-debug] tick={Tick} action=approach from={From} toBoss={BossLocation} distance={Distance:0.0} directTeleport={DirectTeleport} failedDirectTeleportTicks={FailedDirectTeleportTicks} usedPathFallback={UsedPathFallback}",
                            retryCount,
                            client.Game.Me.Location,
                            andariel.Location,
                            distanceToAndariel,
                            attemptedDirectTeleport,
                            failedDirectTeleportTicks,
                            usedPathFallback);
                    }

                    previousPlayerLocation = client.Game.Me.Location;
                    previousAndarielLife = andariel.LifePercentage;
                    return false;
                }

                if (distanceToAndariel > 20)
                {
                    var teleportLocation = client.Game.Me.Location.GetPointBeforePointInSameDirection(andariel.Location, 12);
                    attemptedReposition = true;
                    client.Game.TeleportToLocation(teleportLocation);
                    HumanizationSettings.SleepThread(130);
                }

                if (distanceToAndariel < 18
                    && retryCount % 3 == 0
                    && ClassHelpers.CanStaticEntity(client, andariel.LifePercentage)
                    && client.Game.Me.Mana > 20)
                {
                    client.Game.RepeatRightHandSkillOnLocation(Skill.StaticField, client.Game.Me.Location);
                    HumanizationSettings.SleepThread(120);
                    usedStatic = true;
                }

                HumanizationSettings.SleepThread(200);
                if (client.Game.Me.Mana > 24)
                {
                    client.Game.UseRightHandSkillOnEntity(Skill.FrozenOrb, andariel);
                    HumanizationSettings.SleepThread(160);
                    usedFrozenOrb = true;
                }

                if (debugEnabled && (retryCount % debugEveryTicks == 0 || noDamageTicks >= 10 || stationaryTicks >= 8))
                {
                    Log.Information("[andariel-debug] tick={Tick} me={MeLocation} boss={BossLocation} dist={Distance:0.0} bossLife={BossLife:0.0}% meLife={MeLife}/{MeMaxLife} meMana={MeMana}/{MeMaxMana} castStatic={CastStatic} castOrb={CastOrb} reposition={Reposition} directTeleport={DirectTeleport} stationaryTicks={StationaryTicks} noDamageTicks={NoDamageTicks}",
                        retryCount,
                        client.Game.Me.Location,
                        andariel.Location,
                        distanceToAndariel,
                        andariel.LifePercentage,
                        client.Game.Me.Life,
                        client.Game.Me.MaxLife,
                        client.Game.Me.Mana,
                        client.Game.Me.MaxMana,
                        usedStatic,
                        usedFrozenOrb,
                        attemptedReposition,
                        attemptedDirectTeleport,
                        stationaryTicks,
                        noDamageTicks);

                    if (noDamageTicks >= 10)
                    {
                        Log.Warning("[andariel-debug] no-damage-window tick={Tick} bossLife={BossLife:0.0}% dist={Distance:0.0}", retryCount, andariel.LifePercentage, distanceToAndariel);
                    }

                    if (stationaryTicks >= 8)
                    {
                        Log.Warning("[andariel-debug] possible-stuck tick={Tick} location={Location} dist={Distance:0.0}", retryCount, client.Game.Me.Location, distanceToAndariel);
                    }
                }

                previousPlayerLocation = client.Game.Me.Location;
                previousAndarielLife = andariel.LifePercentage;

                return andariel.LifePercentage < 30;
            },
            TimeSpan.FromSeconds(50)))
        {
            Log.Warning($"Killing Andariel failed at location {client.Game.Me.Location}");
            return false;
        }

        if (!GeneralHelpers.TryWithTimeout((_) =>
        {
            client.Game.UseRightHandSkillOnEntity(Skill.FrozenOrb, andariel);

            if (!client.Game.IsInGame())
            {
                return true;
            }

            return GeneralHelpers.TryWithTimeout((_) => andariel.State == EntityState.Dead || andariel.State == EntityState.Dieing,
                TimeSpan.FromSeconds(0.7));
        }, TimeSpan.FromSeconds(50)))
        {
            Log.Warning($"Killing Andariel failed at location {client.Game.Me.Location}");
            return false;
        }

        if (!PickupNearbyItems(client))
        {
            Log.Warning($"Failed to pickup items at location {client.Game.Me.Location}");
            return false;
        }

        return true;
    }

    private static bool PickupNearbyItems(Client client)
    {
        var pickupItems = client.Game.Items.Values.Where(i => i.Ground && Pickit.Pickit.ShouldPickupItem(client.Game, i, true)).OrderBy(n => n.Location.Distance(client.Game.Me.Location));
        Log.Information($"Killed Andariel, picking up {pickupItems.Count()} items");
        foreach (var item in pickupItems)
        {
            if (item.Location.Distance(client.Game.Me.Location) > 30)
            {
                Log.Warning($"Skipped {item.GetFullDescription()} since it's at location {item.Location}, while player at {client.Game.Me.Location}");
                continue;
            }

            if (!client.Game.IsInGame())
            {
                return false;
            }

            InventoryHelpers.MoveInventoryItemsToCube(client.Game);
            if (client.Game.Inventory.FindFreeSpace(item) == null)
            {
                Log.Warning($"Skipped {item.GetFullDescription()} since inventory is full");
                continue;
            }

            if (!GeneralHelpers.TryWithTimeout((retryCount =>
            {
                if (client.Game.Me.Location.Distance(item.Location) >= 5)
                {
                    client.Game.TeleportToLocation(item.Location);
                    client.Game.MoveTo(item.Location);
                    HumanizationSettings.SleepThread(120);
                    return false;
                }
                else
                {
                    client.Game.MoveTo(item.Location);
                    client.Game.PickupItem(item);
                    HumanizationSettings.SleepThread(50);
                    if (client.Game.Inventory.FindItemById(item.Id) == null && !item.IsGold)
                    {
                        return false;
                    }
                }

                return true;
            }), TimeSpan.FromSeconds(3)))
            {
                Log.Warning($"Picking up item {item.GetFullDescription()} at location {item.Location} from location {client.Game.Me.Location} failed");
            }
        }

        return true;
    }
}