# Configuration Reference

This document covers every option available in `config.json` and `muleconfig.json`, inferred from the ConsoleBot source code.

Both files are passed as command-line arguments when starting the bot:

```
dotnet ConsoleBot.dll --config config.json --muleconfig muleconfig.json
```

`muleconfig` is optional. If omitted, muling is disabled.

---

## config.json Structure

```
{
  "bot":               { ... }   // required
  "externalMessaging": { ... }   // optional
  "map":               { ... }   // required
}
```

---

## `bot` — Core Options

| Key | Type | Required | Default | Description |
|---|---|---|---|---|
| `realm` | string | ✓ | — | IP address or hostname of the Battle.net realm server |
| `keyOwner` | string | ✓ | — | CD-key owner name used when connecting |
| `gameNamePrefix` | string | ✓ | — | Prefix for generated game names (e.g. `"wk"` → games named `wk1`, `wk2`, …) |
| `gamePassword` | string | | — | Password for created games; omit or leave empty for passwordless games |
| `gameDescriptions` | string[] | ✓ | — | List of descriptions used when creating games (one is picked randomly) |
| `difficulty` | string | ✓ | — | Game difficulty. See [Difficulty values](#difficulty) |
| `channelToJoin` | string | | — | Battle.net channel to join while idle between games |
| `gamefolder` | string | ✓ | — | Absolute path to the Diablo II installation folder (e.g. `"D:\\Games\\Diablo 09\\"`) |
| `botType` | string | ✓ | — | Which bot to run. See [Valid botType values](#valid-bottype-values) |
| `logFile` | string | ✓ | — | Path to the log output file |
| `humanization` | object | | see below | Timing randomization settings |
| `pickitThresholdScaling` | object | | see below | Item pickup threshold scaling by category |

### Valid `botType` Values

| Value | Bot | Config section required |
|---|---|---|
| `andariel` | Andariel killer (Act 1) | `bot.andariel` |
| `mephisto` | Mephisto killer (Act 3) | `bot.mephisto` |
| `travincal` | Travincal Council killer (Act 3) | `bot.travincal` |
| `pindle` | Pindleskin killer (Act 5) | `bot.pindle` |
| `cows` | Secret Cow Level runner | `bot.cows` |
| `baal` | Baal runner (Act 5) | `bot.baal` |
| `cs` | Chaos Sanctuary / Diablo runner (Act 4) | `bot.cs` |
| `cube` | Horadric Cube recipe crafter | `bot.cube` |
| `assist` | Follower bot that assists another character | `bot.assist` |
| `test` | Internal test bot | — |

---

## Per-Bot Configuration Sections

### Single-Account Bots (`andariel`, `mephisto`, `travincal`, `pindle`)

These bots use a single account and character. Configure the relevant section under `bot`:

```json
"andariel": {
  "username": "myaccount",
  "password": "mypassword",
  "character": "MyChar"
}
```

The same structure applies to `mephisto`, `travincal`, and `pindle` — just change the top-level key.

| Key | Type | Required | Default | Description |
|---|---|---|---|---|
| `username` | string | ✓ | — | Battle.net account username |
| `password` | string | ✓ | — | Battle.net account password |
| `character` | string | ✓ | — | Character name to log in with |
| `resurrectMerc` | bool | | `true` | Automatically resurrect the mercenary when entering town |

---

### Multi-Client Bots (`cows`, `baal`, `cs`)

These bots connect multiple accounts simultaneously. All three share the following base structure, plus a bot-specific field.

**`bot.cows`**

| Key | Type | Required | Default | Description |
|---|---|---|---|---|
| `portalCharacterName` | string | ✓ | — | Name of the character who opens the cow portal |
| `shouldCreateGames` | bool | | `true` | Whether this bot creates games or joins existing ones |
| `accounts` | AccountConfig[] | ✓ | — | List of accounts to connect (see [AccountConfig](#accountconfig)) |

**`bot.baal`**

| Key | Type | Required | Default | Description |
|---|---|---|---|---|
| `portalCharacterName` | string | ✓ | — | Name of the character who opens the Baal portal |
| `shouldCreateGames` | bool | | `true` | Whether this bot creates games or joins existing ones |
| `accounts` | AccountConfig[] | ✓ | — | List of accounts to connect |

**`bot.cs`**

| Key | Type | Required | Default | Description |
|---|---|---|---|---|
| `teleportCharacterName` | string | ✓ | — | Name of the character who teleports to clear the Chaos Sanctuary |
| `shouldCreateGames` | bool | | `true` | Whether this bot creates games or joins existing ones |
| `accounts` | AccountConfig[] | ✓ | — | List of accounts to connect |

#### AccountConfig

Each entry in an `accounts` array has these fields:

| Key | Type | Required | Default | Description |
|---|---|---|---|---|
| `username` | string | ✓ | — | Battle.net account username |
| `password` | string | ✓ | — | Battle.net account password |
| `character` | string | ✓ | — | Character name to log in with |
| `resurrectMerc` | bool | | `true` | Automatically resurrect the mercenary when entering town |

---

### `bot.cube` — Cube Recipe Bot

Extends the single-account bot with recipe configuration.

| Key | Type | Required | Description |
|---|---|---|---|
| `username` | string | ✓ | Battle.net account username |
| `password` | string | ✓ | Battle.net account password |
| `character` | string | ✓ | Character name |
| `resurrectMerc` | bool | | Resurrect merc (default: `true`) |
| `recipeRequirements` | RecipeRequirement[] | ✓ | Items that must be present in the cube to transmute |
| `recipeResult` | RecipeResult | ✓ | The expected output item to keep |

**RecipeRequirement**

At least one of `quality`, `itemNames`, or `classification` must be set.

| Key | Type | Description |
|---|---|---|
| `amount` | int | Number of this item required |
| `quality` | string | Quality filter. See [QualityType values](#qualitytype) |
| `itemNames` | string[] | Specific item names to match. See [ItemName values](#itemname) |
| `classification` | string | Item category filter. See [ClassificationType values](#classificationtype) |

**RecipeResult**

At least one of `quality`, `itemNames`, or `classification` must be set.

| Key | Type | Description |
|---|---|---|
| `quality` | string | Expected output quality. See [QualityType values](#qualitytype) |
| `itemNames` | string[] | Specific item names to keep. See [ItemName values](#itemname) |
| `classification` | string | Item category to keep. See [ClassificationType values](#classificationtype) |

---

### `bot.assist` — Assist Bot

Follows another character and assists with combat. Does not create games.

| Key | Type | Required | Description |
|---|---|---|---|
| `accounts` | AccountConfig[] | ✓ | List of accounts to connect as followers |
| `hostCharacterName` | string | | Name of the character who hosts/creates the game |
| `leadCharacterName` | string | ✓ | Name of the character to follow and assist |

---

## `bot.humanization` — Timing Randomization

Makes the bot behave less uniformly by adding random delays to various actions. All values are integers.

| Key | Unit | Default | Description |
|---|---|---|---|
| `enabled` | bool | `false` | Enable or disable all humanization delays |
| `actionJitterMinMs` | ms | `0` | Minimum extra delay added to individual actions |
| `actionJitterMaxMs` | ms | `150` | Maximum extra delay added to individual actions |
| `townTaskPauseMinMs` | ms | `300` | Minimum pause between town tasks (buying, repairing, etc.) |
| `townTaskPauseMaxMs` | ms | `1500` | Maximum pause between town tasks |
| `waypointPauseMinMs` | ms | `400` | Minimum pause after using a waypoint |
| `waypointPauseMaxMs` | ms | `1800` | Maximum pause after using a waypoint |
| `preGameCreateDelayMinSeconds` | sec | `3` | Minimum wait before creating a new game |
| `preGameCreateDelayMaxSeconds` | sec | `15` | Maximum wait before creating a new game |
| `shortBreakEveryGamesMin` | games | `8` | Take a short break after at least this many games |
| `shortBreakEveryGamesMax` | games | `20` | Take a short break after at most this many games |
| `shortBreakDurationMinSeconds` | sec | `90` | Minimum duration of a short break |
| `shortBreakDurationMaxSeconds` | sec | `300` | Maximum duration of a short break |
| `longBreakEveryGamesMin` | games | `40` | Take a long break after at least this many games |
| `longBreakEveryGamesMax` | games | `80` | Take a long break after at most this many games |
| `longBreakDurationMinSeconds` | sec | `600` | Minimum duration of a long break |
| `longBreakDurationMaxSeconds` | sec | `1800` | Maximum duration of a long break |
| `joinStaggerMinSeconds` | sec | `5` | For multi-client bots: minimum delay between each account joining |
| `joinStaggerMaxSeconds` | sec | `25` | For multi-client bots: maximum delay between each account joining |

---

## `bot.pickitThresholdScaling` — Pickup Threshold Scaling

Controls how aggressively the pickit threshold is applied per item category. Values are percentages (0–100). All default to `100`.

| Key | Default | Description |
|---|---|---|
| `defaultPercent` | `100` | Fallback threshold for categories not listed below |
| `ringPercent` | `100` | Threshold scaling for rings |
| `glovesPercent` | `100` | Threshold scaling for gloves |
| `bootsPercent` | `100` | Threshold scaling for boots |
| `helmsPercent` | `100` | Threshold scaling for helms |
| `armorsPercent` | `100` | Threshold scaling for body armors |
| `amuletsPercent` | `100` | Threshold scaling for amulets |
| `shieldsPercent` | `100` | Threshold scaling for shields |
| `weaponsPercent` | `100` | Threshold scaling for weapons |
| `beltsPercent` | `100` | Threshold scaling for belts |

A value of `100` means the pickit filter runs at full strength for that category. Lower values relax the filter (more items pass). See [pickit-threshold-scaling](../src/ConsoleBot/Pickit/) for implementation details.

---

## `externalMessaging` — Telegram Notifications (Optional)

This entire section is optional. If omitted, a no-op messaging client is used and no notifications are sent.

| Key | Type | Required | Default | Description |
|---|---|---|---|---|
| `telegramApiKey` | string | ✓ | — | Telegram Bot API token |
| `telegramChatId` | long | ✓ | — | Chat ID where notifications are sent |
| `telegramAdminUserId` | long | ✓ | — | Telegram user ID allowed to send admin commands to the bot |
| `receiveMessages` | bool | | `true` | Whether the bot listens for incoming Telegram commands |

---

## `map` — Map API

| Key | Type | Required | Description |
|---|---|---|---|
| `apiUrl` | URL | ✓ | Base URL of the local map/navigation API service (e.g. `"http://localhost:8080"`) |

---

## muleconfig.json Structure

```
{
  "mule": { ... }
}
```

---

## `mule` — Mule Account Configuration

Defines which mule accounts accept which items. An item is sent to the first mule account whose `matchesAny` rules match it.

| Key | Type | Required | Description |
|---|---|---|---|
| `accounts` | MuleAccount[] | ✓ | List of mule account definitions |

### MuleAccount

| Key | Type | Default | Description |
|---|---|---|---|
| `username` | string | — | Battle.net account username |
| `password` | string | — | Battle.net account password |
| `matchesAny` | MuleRule[] | `[]` | Item is sent here if **any** rule matches |
| `includedCharacters` | string[] | `[]` | If set, only these characters on the account are used as mule targets |
| `excludedCharacters` | string[] | `[]` | Characters on the account to skip when muling |

### MuleRule

A rule matches when **all** of its filters match the item.

| Key | Type | Default | Description |
|---|---|---|---|
| `matchesAll` | MuleFilter[] | `[]` | All filters in this list must match for the rule to apply |

### MuleFilter

Each filter matches on one or more item attributes. At least one attribute field should be set.

| Key | Type | Description |
|---|---|---|
| `notFilter` | bool? | If `true`, inverts this filter (item must NOT match) |
| `itemName` | string | Match a specific item name. See [ItemName values](#itemname) |
| `qualityType` | string | Match item quality. See [QualityType values](#qualitytype) |
| `classificationType` | string | Match item category. See [ClassificationType values](#classificationtype) |

---

## Appendix: Enum Reference

### Difficulty

Used in `bot.difficulty`.

| Value |
|---|
| `normal` |
| `nightmare` |
| `hell` |

---

### QualityType

Used in mule filters, cube recipe requirements, and cube recipe results.

| Value |
|---|
| `NotApplicable` |
| `Inferior` |
| `Normal` |
| `Superior` |
| `Magical` |
| `Set` |
| `Rare` |
| `Unique` |
| `Crafted` |

---

### ClassificationType

Used in mule filters, cube recipe requirements, and cube recipe results.

| Value | Value | Value | Value |
|---|---|---|---|
| `AmazonBow` | `AmazonJavelin` | `AmazonSpear` | `Amulet` |
| `AntidotePotion` | `Armor` | `Arrows` | `AssassinKatar` |
| `Axe` | `BarbarianHelm` | `Belt` | `BodyPart` |
| `Bolts` | `Boots` | `Bow` | `Circlet` |
| `Club` | `Crossbow` | `Dagger` | `DruidPelt` |
| `Ear` | `Elixir` | `Essence` | `Gem` |
| `Gloves` | `Gold` | `GrandCharm` | `Hammer` |
| `HealthPotion` | `Helm` | `Herb` | `Javelin` |
| `Jewel` | `Key` | `LargeCharm` | `Mace` |
| `ManaPotion` | `NecromancerShrunkenHead` | `PaladinShield` | `Polearm` |
| `QuestItem` | `RejuvenationPotion` | `Ring` | `Rune` |
| `Scepter` | `Scroll` | `Shield` | `SmallCharm` |
| `SorceressOrb` | `Spear` | `Staff` | `StaminaPotion` |
| `Sword` | `ThawingPotion` | `ThrowingAxe` | `ThrowingKnife` |
| `ThrowingPotion` | `Token` | `Tome` | `Torch` |
| `Wand` | | | |

---

### ItemName

Used in mule filters, cube recipe requirements, and cube recipe results. Values are PascalCase enum names.

<details>
<summary>Click to expand full ItemName list</summary>

| | | | |
|---|---|---|---|
| `Cap` | `SkullCap` | `Helm` | `FullHelm` |
| `GreatHelm` | `Crown` | `Mask` | `QuiltedArmor` |
| `LeatherArmor` | `HardLeatherArmor` | `StuddedLeather` | `RingMail` |
| `ScaleMail` | `ChainMail` | `BreastPlate` | `SplintMail` |
| `PlateMail` | `FieldPlate` | `GothicPlate` | `FullPlateMail` |
| `AncientArmor` | `LightPlate` | `Buckler` | `SmallShield` |
| `LargeShield` | `KiteShield` | `TowerShield` | `GothicShield` |
| `LeatherGloves` | `HeavyGloves` | `ChainGloves` | `LightGauntlets` |
| `Gauntlets` | `Boots` | `HeavyBoots` | `ChainBoots` |
| `LightPlatedBoots` | `Greaves` | `Sash` | `LightBelt` |
| `Belt` | `HeavyBelt` | `PlatedBelt` | `BoneHelm` |
| `BoneShield` | `SpikedShield` | `WarHat` | `Sallet` |
| `Casque` | `Basinet` | `WingedHelm` | `GrandCrown` |
| `DeathMask` | `GhostArmor` | `SerpentskinArmor` | `DemonhideArmor` |
| `TrellisedArmor` | `LinkedMail` | `TigulatedMail` | `MeshArmor` |
| `Cuirass` | `RussetArmor` | `TemplarCoat` | `SharktoothArmor` |
| `EmbossedPlate` | `ChaosArmor` | `OrnatePlate` | `MagePlate` |
| `Defender` | `RoundShield` | `Scutum` | `DragonShield` |
| `Pavise` | `AncientShield` | `DemonhideGloves` | `SharkskinGloves` |
| `HeavyBracers` | `BattleGauntlets` | `WarGauntlets` | `DemonhideBoots` |
| `SharkskinBoots` | `MeshBoots` | `BattleBoots` | `WarBoots` |
| `DemonhideSash` | `SharkskinBelt` | `MeshBelt` | `BattleBelt` |
| `WarBelt` | `GrimHelm` | `GrimShield` | `BarbedShield` |
| `WolfHead` | `HawkHelm` | `Antlers` | `FalconMask` |
| `SpiritMask` | `JawboneCap` | `FangedHelm` | `HornedHelm` |
| `AssaultHelmet` | `AvengerGuard` | `Targe` | `Rondache` |
| `HeraldicShield` | `AerinShield` | `CrownShield` | `PreservedHead` |
| `ZombieHead` | `UnravellerHead` | `GargoyleHead` | `DemonHead` |
| `Circlet` | `Coronet` | `Tiara` | `Diadem` |
| `Shako` | `Hydraskull` | `Armet` | `GiantConch` |
| `SpiredHelm` | `Corona` | `Demonhead` | `DuskShroud` |
| `Wyrmhide` | `ScarabHusk` | `WireFleece` | `DiamondMail` |
| `LoricatedMail` | `Boneweave` | `GreatHauberk` | `BalrogSkin` |
| `HellforgePlate` | `KrakenShell` | `LacqueredPlate` | `ShadowPlate` |
| `SacredArmor` | `ArchonPlate` | `Heater` | `Luna` |
| `Hyperion` | `Monarch` | `Aegis` | `Ward` |
| `BrambleMitts` | `VampireboneGloves` | `Vambraces` | `CrusaderGauntlets` |
| `OgreGauntlets` | `WyrmhideBoots` | `ScarabshellBoots` | `BoneweaveBoots` |
| `MirroredBoots` | `MyrmidonGreaves` | `SpiderwebSash` | `VampirefangBelt` |
| `MithrilCoil` | `TrollBelt` | `ColossusGirdle` | `BoneVisage` |
| `TrollNest` | `BladeBarrier` | `AlphaHelm` | `GriffonHeaddress` |
| `HuntersGuise` | `SacredFeathers` | `TotemicMask` | `JawboneVisor` |
| `LionHelm` | `RageMask` | `SavageHelmet` | `SlayerGuard` |
| `AkaranTarge` | `AkaranRondache` | `ProtectorShield` | `GildedShield` |
| `RoyalShield` | `MummifiedTrophy` | `FetishTrophy` | `SextonTrophy` |
| `CantorTrophy` | `HierophantTrophy` | `BloodSpirit` | `SunSpirit` |
| `EarthSpirit` | `SkySpirit` | `DreamSpirit` | `CarnageHelm` |
| `FuryVisor` | `DestroyerHelm` | `ConquerorCrown` | `GuardianCrown` |
| `SacredTarge` | `SacredRondache` | `KurastShield` | `ZakarumShield` |
| `VortexShield` | `MinionSkull` | `HellspawnSkull` | `OverseerSkull` |
| `SuccubusSkull` | `BloodlordSkull` | `Elixir` | `hpo` |
| `mpo` | `hpf` | `mpf` | `StaminaPotion` |
| `AntidotePotion` | `RejuvenationPotion` | `FullRejuvenationPotion` | `ThawingPotion` |
| `TomeOfTownPortal` | `TomeofIdentify` | `Amulet` | `TopoftheHoradricStaff` |
| `Ring` | `Gold` | `ScrollofInifuss` | `KeytotheCairnStones` |
| `Arrows` | `Torch` | `Bolts` | `ScrollofTownPortal` |
| `ScrollofIdentify` | `Heart` | `Brain` | `Jawbone` |
| `Eye` | `Horn` | `Tail` | `Flag` |
| `Fang` | `Quill` | `Soul` | `Scalp` |
| `Spleen` | `Key` | `TheBlackTowerKey` | `PotionofLife` |
| `AJadeFigurine` | `TheGoldenBird` | `LamEsensTome` | `HoradricCube` |
| `HoradricScroll` | `MephistosSoulstone` | `BookofSkill` | `KhalimsEye` |
| `KhalimsHeart` | `KhalimsBrain` | `Ear` | `ChippedAmethyst` |
| `FlawedAmethyst` | `Amethyst` | `FlawlessAmethyst` | `PerfectAmethyst` |
| `ChippedTopaz` | `FlawedTopaz` | `Topaz` | `FlawlessTopaz` |
| `PerfectTopaz` | `ChippedSapphire` | `FlawedSapphire` | `Sapphire` |
| `FlawlessSapphire` | `PerfectSapphire` | `ChippedEmerald` | `FlawedEmerald` |
| `Emerald` | `FlawlessEmerald` | `PerfectEmerald` | `ChippedRuby` |
| `FlawedRuby` | `Ruby` | `FlawlessRuby` | `PerfectRuby` |
| `ChippedDiamond` | `FlawedDiamond` | `Diamond` | `FlawlessDiamond` |
| `PerfectDiamond` | `MinorHealingPotion` | `LightHealingPotion` | `HealingPotion` |
| `GreaterHealingPotion` | `SuperHealingPotion` | `MinorManaPotion` | `LightManaPotion` |
| `ManaPotion` | `GreaterManaPotion` | `SuperManaPotion` | `ChippedSkull` |
| `FlawedSkull` | `Skull` | `FlawlessSkull` | `PerfectSkull` |
| `Herb` | `SmallCharm` | `LargeCharm` | `GrandCharm` |
| `rps` | `rpl` | `bps` | `bpl` |
| `ElRune` | `EldRune` | `TirRune` | `NefRune` |
| `EthRune` | `IthRune` | `TalRune` | `RalRune` |
| `OrtRune` | `ThulRune` | `AmnRune` | `SolRune` |
| `ShaelRune` | `DolRune` | `HelRune` | `IoRune` |
| `LumRune` | `KoRune` | `FalRune` | `LemRune` |
| `PulRune` | `UmRune` | `MalRune` | `IstRune` |
| `GulRune` | `VexRune` | `OhmRune` | `LoRune` |
| `SurRune` | `BerRune` | `JahRune` | `ChamRune` |
| `ZodRune` | `Jewel` | `MalahsPotion` | `ScrollofKnowledge` |
| `ScrollofResistance` | `KeyofTerror` | `KeyofHate` | `KeyofDestruction` |
| `DiablosHorn` | `BaalsEye` | `MephistosBrain` | `StandardOfHeroes` |
| `HandAxe` | `Axe` | `DoubleAxe` | `MilitaryPick` |
| `WarAxe` | `LargeAxe` | `BroadAxe` | `BattleAxe` |
| `GreatAxe` | `GiantAxe` | `Wand` | `YewWand` |
| `BoneWand` | `GrimWand` | `Club` | `Scepter` |
| `GrandScepter` | `WarScepter` | `SpikedClub` | `Mace` |
| `MorningStar` | `Flail` | `WarHammer` | `Maul` |
| `GreatMaul` | `ShortSword` | `Scimitar` | `Sabre` |
| `Falchion` | `CrystalSword` | `BroadSword` | `LongSword` |
| `WarSword` | `TwoHandedSword` | `Claymore` | `GiantSword` |
| `BastardSword` | `Flamberge` | `GreatSword` | `Dagger` |
| `Dirk` | `Kris` | `Blade` | `ThrowingKnife` |
| `ThrowingAxe` | `BalancedKnife` | `BalancedAxe` | `Javelin` |
| `Pilum` | `ShortSpear` | `Glaive` | `ThrowingSpear` |
| `Spear` | `Trident` | `Brandistock` | `Spetum` |
| `Pike` | `Bardiche` | `Voulge` | `Scythe` |
| `Poleaxe` | `Halberd` | `WarScythe` | `ShortStaff` |
| `LongStaff` | `GnarledStaff` | `BattleStaff` | `WarStaff` |
| `ShortBow` | `HuntersBow` | `LongBow` | `CompositeBow` |
| `ShortBattleBow` | `LongBattleBow` | `ShortWarBow` | `LongWarBow` |
| `LightCrossbow` | `Crossbow` | `HeavyCrossbow` | `RepeatingCrossbow` |
| `RancidGasPotion` | `OilPotion` | `ChokingGasPotion` | `ExplodingPotion` |
| `StranglingGasPotion` | `FulminatingPotion` | `DecoyGidbinn` | `TheGidbinn` |
| `WirtsLeg` | `HoradricMalus` | `HellForgeHammer` | `HoradricStaff` |
| `ShaftoftheHoradricStaff` | `Hatchet` | `Cleaver` | `TwinAxe` |
| `Crowbill` | `Naga` | `MilitaryAxe` | `BeardedAxe` |
| `Tabar` | `GothicAxe` | `AncientAxe` | `BurntWand` |
| `PetrifiedWand` | `TombWand` | `GraveWand` | `Cudgel` |
| `RuneScepter` | `HolyWaterSprinkler` | `DivineScepter` | `BarbedClub` |
| `FlangedMace` | `JaggedStar` | `Knout` | `BattleHammer` |
| `WarClub` | `MarteldeFer` | `Gladius` | `Cutlass` |
| `Shamshir` | `Tulwar` | `DimensionalBlade` | `BattleSword` |
| `RuneSword` | `AncientSword` | `Espandon` | `DacianFalx` |
| `TuskSword` | `GothicSword` | `Zweihander` | `ExecutionerSword` |
| `Poignard` | `Rondel` | `Cinquedeas` | `Stiletto` |
| `BattleDart` | `Francisca` | `WarDart` | `Hurlbat` |
| `WarJavelin` | `GreatPilum` | `Simbilan` | `Spiculum` |
| `Harpoon` | `WarSpear` | `Fuscina` | `WarFork` |
| `Yari` | `Lance` | `LochaberAxe` | `Bill` |
| `BattleScythe` | `Partizan` | `BecDeCorbin` | `GrimScythe` |
| `JoStaff` | `Quarterstaff` | `CedarStaff` | `GothicStaff` |
| `RuneStaff` | `EdgeBow` | `RazorBow` | `CedarBow` |
| `DoubleBow` | `ShortSiegeBow` | `LargeSiegeBow` | `RuneBow` |
| `GothicBow` | `Arbalest` | `SiegeCrossbow` | `Ballista` |
| `ChuKoNu` | `KhalimsFlail` | `KhalimsWill` | `Katar` |
| `WristBlade` | `HatchetHands` | `Cestus` | `Claws` |
| `BladeTalons` | `ScissorsKatar` | `Quhab` | `WristSpike` |
| `Fascia` | `HandScythe` | `GreaterClaws` | `GreaterTalons` |
| `ScissorsQuhab` | `Suwayyah` | `WristSword` | `WarFist` |
| `BattleCestus` | `FeralClaws` | `RunicTalons` | `ScissorsSuwayyah` |
| `Tomahawk` | `SmallCrescent` | `EttinAxe` | `WarSpike` |
| `BerserkerAxe` | `FeralAxe` | `SilverEdgedAxe` | `Decapitator` |
| `ChampionAxe` | `GloriousAxe` | `PolishedWand` | `GhostWand` |
| `LichWand` | `UnearthedWand` | `Truncheon` | `MightyScepter` |
| `SeraphRod` | `Caduceus` | `TyrantClub` | `ReinforcedMace` |
| `DevilStar` | `Scourge` | `LegendaryMallet` | `OgreMaul` |
| `ThunderMaul` | `Falcata` | `Ataghan` | `ElegantBlade` |
| `HydraEdge` | `PhaseBlade` | `ConquestSword` | `CrypticSword` |
| `MythicalSword` | `LegendSword` | `HighlandBlade` | `BalrogBlade` |
| `ChampionSword` | `ColossusSword` | `ColossusBlade` | `BoneKnife` |
| `MithrilPoint` | `FangedKnife` | `LegendSpike` | `FlyingKnife` |
| `FlyingAxe` | `WingedKnife` | `WingedAxe` | `HyperionJavelin` |
| `StygianPilum` | `BalrogSpear` | `GhostGlaive` | `WingedHarpoon` |
| `HyperionSpear` | `StygianPike` | `Mancatcher` | `GhostSpear` |
| `WarPike` | `OgreAxe` | `ColossusVoulge` | `Thresher` |
| `CrypticAxe` | `GreatPoleaxe` | `GiantThresher` | `WalkingStick` |
| `Stalagmite` | `ElderStaff` | `Shillelagh` | `ArchonStaff` |
| `SpiderBow` | `BladeBow` | `ShadowBow` | `GreatBow` |
| `DiamondBow` | `CrusaderBow` | `WardBow` | `HydraBow` |
| `PelletBow` | `GorgonCrossbow` | `ColossusCrossbow` | `DemonCrossbow` |
| `EagleOrb` | `SacredGlobe` | `SmokedSphere` | `ClaspedOrb` |
| `JaredsStone` | `StagBow` | `ReflexBow` | `MaidenSpear` |
| `MaidenPike` | `MaidenJavelin` | `GlowingOrb` | `CrystallineGlobe` |
| `CloudySphere` | `SparklingBall` | `SwirlingCrystal` | `AshwoodBow` |
| `CeremonialBow` | `CeremonialSpear` | `CeremonialPike` | `CeremonialJavelin` |
| `HeavenlyStone` | `EldritchOrb` | `DemonHeart` | `VortexOrb` |
| `DimensionalShard` | `MatriarchalBow` | `GrandMatronBow` | `MatriarchalSpear` |
| `MatriarchalPike` | `MatriarchalJavelin` | `EssenceOfHatred` | `EssenceOfTerror` |
| `EssenceOfPain` | `EssenceOfSuffering` | `EssenceOfAnguish` | `TokenOfAbsolution` |

</details>
