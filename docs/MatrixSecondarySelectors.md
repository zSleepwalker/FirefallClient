# Matrix secondary selectors

Detailed static analysis of the secondary command dispatcher and its verified selector cases, extracted verbatim from [MatrixConnection.md](MatrixConnection.md) to keep the transport and connection notes navigable.

## Secondary command dispatcher and verified selector cases

The secondary-dispatch target `0x81CC50` is a large raw function
(`0x4B9E` bytes) with two separate selector tables. It reads a leading byte
from its input structure. Byte0 and values `0xF0..0xFF` use the secondary
selector path. For other values, it decrements the leading byte, compares the
result with `0x35`, then uses that decremented value as the index into the
54-byte map at VA `0xC2240C` (raw `0x82180C`). Thus nonzero leading bytes
`0x01..0x36` map to indices `0x00..0x35`; the map result selects a seven-entry
table at VA `0xC223F0` (raw `0x8217F0`). The observed leading-byte ranges are:

| Leading byte | Route index | Raw target | Observation |
| ---: | ---: | ---: | --- |
| `0x01..0x0E` | 0 | `0x81CCAD` | Nested selector table |
| `0x0F..0x12`, `0x20..0x21`, `0x29..0x33`, `0x36` | 1 | `0x8217D9` | Returns zero |
| `0x13..0x19` | 2 | `0x81EE63` | Three dispatcher values (`0x53..0x55`); value `0x53` documented below |
| `0x1A..0x1F` | 3 | `0x81EF6D` | Nine switch entries; entry 0 documented below |
| `0x22..0x25` | 4 | `0x81F206` | Three dispatcher values (`0x53..0x55`); value `0x53` documented below |
| `0x26..0x28` | 5 | `0x81F2EF` | Value `0x53` documented below |
| `0x34..0x35` | 6 | `0x81F34A` | Value `0x53` documented below |

Leading byte0 or `0xF0..0xFF` uses a second table at VA `0xC22630`
(raw `0x821A30`) when its selector is `0x20..0x7A`; values outside that
selector range return zero. Leading bytes `0x37..0xEF` also reach the
zero-return path. These comparisons establish dispatch boundaries only;
RTTI-backed secondary-selector identities are documented below.

The five case-bearing primary routes (indices 2..6) have 17 directly selected
subcases: three at route2, nine at route3, three at route4, and one each at
routes5 and6. All 17 subcases now have native layouts.
Route0's nested table has a separate, complete set of 40 documented selectors
(`0x53..0x7A`); route1 is the zero-return route. The secondary selector table
records 91 dispatch targets. Selectors `0x20..0x5F` now have 64 RTTI-backed
class and body-reader layouts; selectors `0x60..0x67`, `0x68..0x6F`, and
`0x70..0x77` each add eight verified layouts; selectors `0x78..0x7A` add the
final three. All 91 targets now have distinct, RTTI-backed message identities
and reconstructed reader layouts.

### Secondary-selector entries `0x20..0x7A`

This is the separate second table used by the leading-byte-zero and
`0xF0..0xFF` path in `0x81CC50`. Its index is `selector - 0x20`, with accepted
selectors `0x20..0x7A`, and the DWORD table is at VA `0xC22630` (raw file
offset `0x821A30`). It is distinct from the keyed-receiver factory's selector
range `0x02..0x35` and route0's nested Character selectors `0x53..0x7A`.
Secondary selectors `0x53..0x7A` overlap route0's Character selector values
numerically, but belong to separate tables and dispatch paths. In particular,
secondary selectors `0x78..0x7A` below are `VendorProductsResponse`,
`VendorPurchaseResponse`, and `ConductorGlobalAnnouncement`; they are not
route0's `OpenMovieDialog`, `PrivateDialog`, and `PublicDialog` cases.
The original PE confirms `.text` raw offsets map to VA with `+0x400C00`;
`.rdata` vtable/COL locations map with `+0x401400`, while `.data` TypeDescriptor
locations map with `+0x401C00`. Vtable names below were resolved through their
MSVC complete-object locators, not by treating VAs as raw offsets.
Selectors outside `0x20..0x7A` reach the shared zero-return block at raw
`0x8217D9`.

| Selector | Table index | Target VA / raw | Alloc size | Constructor (raw) | Vtable VA / RTTI | Body reader (raw) |
| ---: | ---: | ---: | ---: | ---: | --- | ---: |
| `0x20` | 0 | `0xC1FFBB` / `0x81F3BB` | `0x38` | `0x813400` | `0x1C6E258` / `EncounterToUIMessage` | `0x853B10` |
| `0x21` | 1 | `0xC20023` / `0x81F423` | `0x50` | `0x813630` | `0x1C6E288` / `VotekickInitiated` | `0x853A20` |
| `0x22` | 2 | `0xC2008B` / `0x81F48B` | `0x40` | `0x8138A0` | `0x1C6E278` / `VotekickResponded` | `0x8538D0` |
| `0x23` | 3 | `0xC200F3` / `0x81F4F3` | `0x30` | `0x813AF0` | `0x1C6E228` / `ScoreBoardEnable` | `0x356CF0` |
| `0x24` | 4 | `0xC2015D` / `0x81F55D` | `0x40` | `0x813BA0` | `0x1C6E248` / `ScoreBoardInit` | `0x853720` |
| `0x25` | 5 | `0xC201C5` / `0x81F5C5` | `0x30` | `0x813EA0` | `0x1C6E238` / `ScoreBoardSetWinner` | `0x337180` |
| `0x26` | 6 | `0xC2022F` / `0x81F62F` | `0x28` | `0x813F50` | `0x1C6E1F8` / `ScoreBoardClear` | none |
| `0x27` | 7 | `0xC20283` / `0x81F683` | `0x58` | `0x813FC0` | `0x1C6E218` / `ScoreBoardAddPlayer` | `0x851490` |
| `0x28` | 8 | `0xC202EB` / `0x81F6EB` | `0x30` | `0x814170` | `0x1C6E208` / `ScoreBoardRemovePlayer` | `0x356F80` |
| `0x29` | 9 | `0xC20356` / `0x81F756` | `0x48` | `0x814230` | `0x1C6E1D8` / `ScoreBoardUpdatePlayerStats` | `0x851410` |
| `0x2A` | 10 | `0xC203BE` / `0x81F7BE` | `0x40` | `0x814390` | `0x1C6E1E8` / `ScoreBoardUpdatePlayerStatsFromStats` | `0x851370` |
| `0x2B` | 11 | `0xC20426` / `0x81F826` | `0x48` | `0x814500` | `0x1C6E1B8` / `ScoreBoardUpdatePlayerStatus` | `0x8506B0` |
| `0x2C` | 12 | `0xC2048E` / `0x81F88E` | `0x38` | `0x8145F0` | `0x1C6E1A8` / `MatchLoadingState` | `0x356CF0`, `0x8503E0` |
| `0x2D` | 13 | `0xC20502` / `0x81F902` | `0x30` | `0x814780` | `0x1C6E1C8` / `MatchEndAck` | `0x356F80` (shared continuation) |
| `0x2E` | 14 | `0xC2054A` / `0x81F94A` | `0x40` | `0x814840` | `0x1C6E188` / `ServerProfiler_SendNames` | `0x34E700` twice |
| `0x2F` | 15 | `0xC205BE` / `0x81F9BE` | `0x38` | `0x8149D0` | `0x1C6E178` / `ServerProfiler_SendFrame` | `0x34D6D0` |
| `0x30` | 16 | `0xC20628` / `0x81FA28` | `0x48` | `0x814B50` | `0x1C6E198` / `TempConsoleMessage` | `0x84F7F0` |
| `0x31` | 17 | `0xC20690` / `0x81FA90` | `0x28` | `0x814EF0` | `0x1C6E148` / `ReloadStaticData` | none |
| `0x32` | 18 | `0xC206E4` / `0x81FAE4` | `0x30` | `0x814F60` | `0x1C6E138` / `EncDebugChatMessage` | `0x12EE010` |
| `0x33` | 19 | `0xC2074E` / `0x81FB4E` | `0x30` | `0x8150F0` | `0x1C6E168` / `SendRadioMessage` | `0x84F650` |
| `0x34` | 20 | `0xC207B6` / `0x81FBB6` | `0x30` | `0x8151B0` | `0x1C6E158` / `NpcBehaviorInfo` | `0x12EE010` (shared with `0x32`) |
| `0x35` | 21 | `0xC207FE` / `0x81FBFE` | `0x38` | `0x815340` | `0x1C6E108` / `NpcMonitoringLog` | `0x84F550` |
| `0x36` | 22 | `0xC20866` / `0x81FC66` | `0x48` | `0x815560` | `0x1C6E128` / `NpcNavigationInfo` | `0x84F490` |
| `0x37` | 23 | `0xC208CE` / `0x81FCCE` | `0x40` | `0x815730` | `0x1C6E118` / `NpcHostilityDebugInfo` | `0x84E200` |
| `0x38` | 24 | `0xC20936` / `0x81FD36` | `0x48` | `0x815890` | `0x1C6E0D8` / `NpcPositionalDebugInfo` | `0x84D830` |
| `0x39` | 25 | `0xC2099E` / `0x81FD9E` | `0x40` | `0x815A80` | `0x1C6E0F8` / `NpcShapeDebugInfo` | `0x84C960` |
| `0x3A` | 26 | `0xC20A06` / `0x81FE06` | `0x50` | `0x815C70` | `0x1C6E0E8` / `NpcVoxelInfo` | `0x84BC10` |
| `0x3B` | 27 | `0xC20A6E` / `0x81FE6E` | `0x38` | `0x815E40` | `0x1C6E0B8` / `DebugDrawInfo` | `0x609E50` |
| `0x3C` | 28 | `0xC20AD8` / `0x81FED8` | `0x30` | `0x815F90` | `0x1C6E0A8` / `NpcDevCmdResponse` | `0x12EE010` twice |
| `0x3D` | 29 | `0xC20B4C` / `0x81FF4C` | `0x60` | `0x8161A0` | `0x1C6E0C8` / `DevRequestObjectPositions` | `0x84BA20` |
| `0x3E` | 30 | `0xC20BB4` / `0x81FFB4` | `0x38` | `0x8163A0` | `0x1C6E088` / `DevRequestSpawnTables` | `0x84B470` |
| `0x3F` | 31 | `0xC20C1E` / `0x82001E` | `0x38` | `0x8164E0` | `0x1C6E078` / `DevRequestResourceNodeDebug` | `0x84E230` |
| `0x40` | 32 | `0xC20C88` / `0x820088` | `0x48` | `0x816630` | `0x1C6E098` / `MissionObjectiveUpdated` | `0x84A7F0` |
| `0x41` | 33 | `0xC20CF0` / `0x8200F0` | `0x38` | `0x816880` | `0x1C6E048` / `MissionStatusChanged` | `0x84A640` |
| `0x42` | 34 | `0xC20D58` / `0x820158` | `0x30` | `0x816960` | `0x1C6E038` / `MissionReturnToChanged` | `0x84A520` |
| `0x43` | 35 | `0xC20DC0` / `0x8201C0` | `0x40` | `0x816A20` | `0x1C6E068` / `MissionsAvailable` | `0x84A480` |
| `0x44` | 36 | `0xC20E28` / `0x820228` | `0x30` | `0x816B80` | `0x1C6E058` / `MissionActivationAck` | `0x849AB0` |
| `0x45` | 37 | `0xC20E90` / `0x820290` | `0x30` | `0x816C40` | `0x1C6E008` / `BountyStatusChanged` | `0x8499F0` |
| `0x46` | 38 | `0xC20EF8` / `0x8202F8` | `0x30` | `0x816D00` | `0x1C6E028` / `BountyAbortAck` | `0x8498D0` |
| `0x47` | 39 | `0xC20F60` / `0x820360` | `0x50` | `0x816DC0` | `0x1C6E018` / `BountyActivationAck` | `0x849790` |
| `0x48` | 40 | `0xC20FC8` / `0x8203C8` | `0x40` | `0x816FC0` | `0x1C6DFD8` / `BountyListActiveAck` | `0x849760` |
| `0x49` | 41 | `0xC21030` / `0x820430` | `0x40` | `0x817120` | `0x1C6DFF8` / `BountyListActiveDetailsAck` | `0x8496A0` |
| `0x4A` | 42 | `0xC21098` / `0x820498` | `0x40` | `0x817260` | `0x1C6DFE8` / `BountyListAvailableAck` | `0x849600` |
| `0x4B` | 43 | `0xC21100` / `0x820500` | `0x30` | `0x8173A0` | `0x1C6DFB8` / `BountyClearAck` | `0x849560` |
| `0x4C` | 44 | `0xC21168` / `0x820568` | `0x30` | `0x817460` | `0x1C6DFA8` / `BountyClearPreviousAck` | `0x8494C0` |
| `0x4D` | 45 | `0xC211D0` / `0x8205D0` | `0x40` | `0x817520` | `0x1C6DFC8` / `BountyListPreviousAck` | `0x849420` |
| `0x4E` | 46 | `0xC21238` / `0x820638` | `0x38` | `0x817660` | `0x1C6DF88` / `BountyRerollResponse` | `0x849380` |
| `0x4F` | 47 | `0xC212A0` / `0x8206A0` | `0x30` | `0x817800` | `0x1C6DF78` / `DisplayUiTrackBounty` | `0x3370A0` (direct) |
| `0x50` | 48 | `0xC2130B` / `0x82070B` | `0x38` | `0x8178C0` | `0x1C6DF98` / `Achievements` | `0x848FC0` |
| `0x51` | 49 | `0xC21375` / `0x820775` | `0x30` | `0x817A10` | `0x1C6DF48` / `AchievementUnlocked` | `0x356F80` (shared continuation) |
| `0x52` | 50 | `0xC213BD` / `0x8207BD` | `0x30` | `0x817AD0` | `0x1C6DF38` / `TotalAchievementPoints` | `0x3370A0` (shared continuation) |
| `0x53` | 51 | `0xC21405` / `0x820805` | `0x38` | `0x817B90` | `0x1C6DF68` / `MissionCompletionCounts` | `0x848610`, `0x12EE010` |
| `0x54` | 52 | `0xC21479` / `0x820879` | `0x38` | `0x817D60` | `0x1C6DF58` / `ContentUnlocked` | `0x847DC0` |
| `0x55` | 53 | `0xC214E1` / `0x8208E1` | `0x30` | `0x817F20` | `0x1C6DF08` / `ClientUIAction` | `0x12EE010` (shared continuation) |
| `0x56` | 54 | `0xC21529` / `0x820929` | `0x30` | `0x8180B0` | `0x1C6DF28` / `ArcCompletionHistoryUpdate` | `0x847A70` |
| `0x57` | 55 | `0xC21593` / `0x820993` | `0x38` | `0x818220` | `0x1C6DF18` / `JobLedgerEntriesUpdate` | `0x6095C0` |
| `0x58` | 56 | `0xC215FD` / `0x8209FD` | `0x30` | `0x818370` | `0x1C6DED8` / `TrackRecipe` | `0x3370A0` (shared continuation) |
| `0x59` | 57 | `0xC21645` / `0x820A45` | `0x28` | `0x818430` | `0x1C6DEF8` / `ClearTrackedRecipe` | none |
| `0x5A` | 58 | `0xC21699` / `0x820A99` | `0x30` | `0x8184A0` | `0x1C6DEE8` / `SlotTech` | `0x3370A0` (shared continuation) |
| `0x5B` | 59 | `0xC216E1` / `0x820AE1` | `0x40` | `0x818560` | `0x1C6DEB8` / `InteractableStatusChanged` | `0x846A50` |
| `0x5C` | 60 | `0xC21749` / `0x820B49` | `0x38` | `0x818640` | `0x1C6DEA8` / `SendTipMessage` | `0x846960` |
| `0x5D` | 61 | `0xC217B1` / `0x820BB1` | `0x30` | `0x818730` | `0x1C6DEC8` / `DebugEventSample` | `0x846850` |
| `0x5E` | 62 | `0xC21819` / `0x820C19` | `0x38` | `0x818800` | `0x1C6DE88` / `DebugLagPlayerSample` | `0x846760` |
| `0x5F` | 63 | `0xC21881` / `0x820C81` | `0x48` | `0x8188E0` | `0x1C6DE78` / `DebugLagSimulationSample` | `0x8465C0` |
| `0x60` | 64 | `0xC218E9` / `0x820CE9` | `0x30` | `0x818A10` | `0x1C6DE98` / `DebugLagRaiaSample` | `0x846580` |
| `0x61` | 65 | `0xC21951` / `0x820D51` | `0x58` | `0x818AE0` | `0x1C6DE48` / `DebugEncounterVolumes` | `0x846480` |
| `0x62` | 66 | `0xC219B9` / `0x820DB9` | `0x40` | `0x818CE0` | `0x1C6DE38` / `Trail` | `0x844AD0` |
| `0x63` | 67 | `0xC21A21` / `0x820E21` | `0x38` | `0x818E40` | `0x1C6DE68` / `EncounterDebugNotification` | `0x844A10` |
| `0x64` | 68 | `0xC21A89` / `0x820E89` | `0x60` | `0x819070` | `0x1C6DE58` / `EncounterUIScopeIn` | `0x8448B0` |
| `0x65` | 69 | `0xC21AF1` / `0x820EF1` | `0x50` | `0x819310` | `0x1C6DE08` / `EncounterUIUpdate` | `0x842940` |
| `0x66` | 70 | `0xC21B59` / `0x820F59` | `0x30` | `0x8194D0` | `0x1C6DE28` / `EncounterUIScopeOut` | `0x356F80` (shared continuation) |
| `0x67` | 71 | `0xC21BA1` / `0x820FA1` | `0x90` | `0x819590` | `0x1C6DE18` / `DisplayUiNotification` | `0x8428B0` |
| `0x68` | 72 | `0xC21C0C` / `0x82100C` | `0x38` | `0x819750` | `0x1C6DDF8` / `DisplayMoneyBombBanner` | `0x8420B0` |
| `0x69` | 73 | `0xC21C74` / `0x821074` | `0x68` | `0x819900` | `0x1C6DDC8` / `SetPreloadPosition` | `0x6087E0` |
| `0x6A` | 74 | `0xC21CDE` / `0x8210DE` | `0x38` | `0x8199B0` | `0x1C6DDE8` / `PlaySoundId` | `0x841F80` |
| `0x6B` | 75 | `0xC21D46` / `0x821146` | `0x40` | `0x819A90` | `0x1C6DDD8` / `PlaySoundIdAtLocation` | `0x841ED0` |
| `0x6C` | 76 | `0xC21DAE` / `0x8211AE` | `0x38` | `0x819B60` | `0x1C6DD98` / `PlayDialogScriptMessage` | `0x841DE0` |
| `0x6D` | 77 | `0xC21E16` / `0x821216` | `0x40` | `0x819CC0` | `0x1C6DDB8` / `StopDialogScriptMessage` | `0x841D40` |
| `0x6E` | 78 | `0xC21E7E` / `0x82127E` | `0x38` | `0x819E20` | `0x1C6DDA8` / `PingMap` | `0x841C60` |
| `0x6F` | 79 | `0xC21EE6` / `0x8212E6` | `0x38` | `0x819EF0` | `0x1C6DD78` / `PingMapMarker` | `0x841B70` |
| `0x70` | 80 | `0xC21F4E` / `0x82134E` | `0x40` | `0x819FC0` | `0x1C6DD68` / `EncounterPublicInfo` | `0x841AA0` |
| `0x71` | 81 | `0xC21FB6` / `0x8213B6` | `0x38` | `0x81A120` | `0x1C6DD88` / `RequestActiveEncounters_Response` | `0x841950` |
| `0x72` | 82 | `0xC2201E` / `0x82141E` | `0x30` | `0x81A260` | `0x1C6DD48` / `ShoppingListInit` | `0x5B9670` |
| `0x73` | 83 | `0xC22088` / `0x821488` | `0x50` | `0x81A3D0` | `0x1C6DD38` / `SetClientDailyInfo` | `0x8417D0` |
| `0x74` | 84 | `0xC220F0` / `0x8214F0` | `0x38` | `0x81A570` | `0x1C6DD58` / `GlobalCounterUpdate` | `0x841730` |
| `0x75` | 85 | `0xC22158` / `0x821558` | `0x38` | `0x81A720` | `0x1C6DD08` / `GlobalCounterMilestoneInfo` | `0x841430`, `0x355E50` |
| `0x76` | 86 | `0xC221CC` / `0x8215CC` | `0x38` | `0x81A930` | `0x1C6DCF8` / `ChatMessageList` | `0x83ECF0` |
| `0x77` | 87 | `0xC22236` / `0x821636` | `0x50` | `0x81AA70` | `0x1C6DD28` / `CurrentLoadoutResponse` | `0x83E000` |
| `0x78` | 88 | `0xC2229E` / `0x82169E` | `0x60` | `0x81AC60` | `0x1C6DD18` / `VendorProductsResponse` | `0x83DF60` |
| `0x79` | 89 | `0xC22306` / `0x821706` | `0x48` | `0x81AE20` | `0x1C6DCE8` / `VendorPurchaseResponse` | `0x83B9F0` |
| `0x7A` | 90 | `0xC2236E` / `0x82176E` | `0x88` | `0x81B000` | `0x1C6DCD8` / `ConductorGlobalAnnouncement` | `0x83B8F0` |

All 91 entries have 91 distinct, nonzero table destinations. No shared case or
zero-valued table slot occurs. Selector `0x26`
does have a distinct allocation-failure branch to the common zero-return block;
that is a case-path result, not a zero table entry. Selectors `0x28` and `0x2D`
share a body-reader continuation at raw `0x81F72E`, but their selector-table
destinations and RTTI-identified types remain distinct. Selectors `0x32` and
`0x34` likewise have separate table targets, constructors, and RTTI, while
sharing the NUL-terminated text reader at raw `0x12EE010` through continuation
`0x81FB27`.

All 91 constructors call a selector-specific base-metadata initializer
before storing the derived vtable. For selectors `0x20..0x27` these helpers
are `0x823020`, `0x823090`, `0x823100`, `0x823170`, `0x8231E0`, `0x823250`,
`0x8232C0`, and `0x823330`; for `0x28..0x2F` they are `0x8233C0`, `0x823430`,
`0x8234A0`, `0x823510`, `0x823580`, `0x8236F0`, `0x823760`, and `0x8237D0`;
for `0x30..0x37` they are `0x823840`, `0x8238B0`, `0x823920`, `0x823990`,
`0x823A00`, `0x823A70`, `0x823AE0`, and `0x823C50`; selectors `0x38..0x3F`
use `0x823DB0`, `0x823F20`, `0x824090`, `0x824100`, `0x824170`, `0x8241E0`,
`0x824250`, and `0x8243C0`; selectors `0x40..0x47` use `0x824430`,
`0x8244A0`, `0x824510`, `0x824580`, `0x8246E0`, `0x824750`, `0x8247C0`,
and `0x824830`; selectors `0x48..0x4F` use `0x8248A0`, `0x824910`,
`0x824980`, `0x8249F0`, `0x824A60`, `0x824AD0`, `0x824B40`, and `0x824BB0`;
selectors `0x50..0x57` use `0x824C20`, `0x824D80`, `0x824DF0`, `0x824E60`,
`0x824FD0`, `0x825040`, `0x8250B0`, and `0x825120`. The
`0x8250B0` initializer also installs a base vtable before initializing the
common metadata fields. Selectors `0x58..0x5F` use `0x825190`, `0x825200`,
`0x825270`, `0x8252E0`, `0x825350`, `0x8253C0`, `0x825430`, and `0x8254A0`;
selectors `0x60..0x67` use `0x825510`, `0x825580`, `0x825700`, `0x825770`,
`0x8257E0`, `0x8258C0`, `0x825930`, and `0x8259A0`; selectors `0x68..0x6F`
use `0x825AA0`, `0x825B10`, `0x825B80`, `0x825BF0`, `0x825C60`, `0x825CD0`,
`0x825D40`, and `0x825DB0`; selectors `0x70..0x77` use `0x825E20`, `0x825E90`,
`0x825F00`, `0x825F70`, `0x825FE0`, `0x826050`, `0x826130`, and `0x8262A0`;
selectors `0x78..0x7A` use `0x826310`, `0x826380`, and `0x826400`.
Each helper sets a base flag DWORD at `+0x08` to 1, stores its
descriptor-anchor pointer at `+0x0C`, stores the constructor-supplied type
DWORD at `+0x10`, copies the parser input's first two DWORDs to `+0x18/+0x1C`,
and zeros DWORDs at `+0x20/+0x24`. Thus the message body begins at `+0x28`.
The descriptor anchors are opaque metadata pointers; class identities in the
table come from the section-aware vtable RTTI names. The cases for selectors
`0x28..0x2F` write parser-local DWORD literals `0x08..0x0F` into a separate
stack slot. The cases for selectors `0x30..0x37` write `0x10..0x17` there.
Their roles are unknown; numerical coincidence with table indexes does not
establish that they are selector indexes or message discriminators.
Selectors `0x38..0x3F` likewise store parser-local DWORD literals `0x18..0x1F`;
these are recorded separately from selector/table indexes and their roles are
unknown. Selectors `0x40..0x4F` store parser-local DWORD literals `0x20..0x2F`
in the same separate slot; their roles are also unresolved and they are not
the constructor type IDs or table indexes.
Selectors `0x58..0x5F` store parser-local DWORD literals `0x38..0x3F` in that
separate slot. These remain distinct observations from the selector, native
constructor type ID, and descriptor anchor; no role is assigned to the values.
Selectors `0x60..0x67` similarly store parser-local DWORD literals `0x40..0x47`
in that separate slot; their roles are unknown and distinct from selector,
constructor type ID, and descriptor anchor.
Selectors `0x68..0x6F` similarly store parser-local DWORD literals `0x48..0x4F`;
their roles remain unknown and distinct from selector, type ID, and descriptor
anchor. Selectors `0x70..0x77` similarly store parser-local DWORD literals
`0x50..0x57`; selectors `0x78..0x7A` store `0x58..0x5A` in the same separate
slot. These values remain unknown and distinct from selector, type ID, and
descriptor anchor.

Body reads, with field labels deliberately limited to offsets and widths:

- `0x20` `EncounterToUIMessage`: raw QWORD at `+0x28`, then
  NUL-terminated shared-text reads at `+0x30` and `+0x34`.
- `0x21` `VotekickInitiated`: raw QWORD at `+0x28`; shared text at `+0x30`
  and `+0x34`; DWORD at `+0x38`; raw QWORD at `+0x40`; byte at `+0x48`.
- `0x22` `VotekickResponded`: raw QWORD at `+0x28`; shared text at `+0x30`
  and `+0x34`; byte at `+0x38`; WORDs at `+0x3A` and `+0x3C`.
- `0x23` `ScoreBoardEnable` and `0x25` `ScoreBoardSetWinner`: one byte at
  `+0x28` via their respective readers. Neither has a body gate.
- `0x24` `ScoreBoardInit`: a three-pointer record vector at
  `+0x28/+0x2C/+0x30`, followed by bytes at `+0x34`, `+0x35`, `+0x38`,
  `+0x39`, then shared text at `+0x3C`. The collection reader first clears
  existing `0xA8`-stride records. It then reads a byte run count, parses that
  many records when nonzero, and reads another run count only when the prior
  count was `0xFF`; zero ends the sequence. Each record reader at `0x852B70`
  reads QWORDs at `+0x00/+0x08`, shared text at `+0x10`, DWORD at `+0x14`,
  byte at `+0x18`, DWORD at `+0x1C`, bytes at `+0x20/+0x24/+0x29/+0x28`,
  then nested readers at `+0x2C`, `+0x94`, `+0x88` in that order. The
  `+0x2C` reader consumes 18 DWORDs, a byte-run collection of four-DWORD
  items, a DWORD at `+0x80`, and a byte at `+0x84`. The `+0x94` reader
  inserts byte-run entries of three DWORDs (first DWORD used as the tree key);
  it is followed by a DWORD at `+0x9C` and byte at `+0xA0`. The final `+0x88`
  collection contains `0x5C`-stride records. Names and semantics for these
  nested values remain unknown.
- `0x26` `ScoreBoardClear`: no body-reader call; its allocation is exactly the
  `0x28`-byte base object.
- `0x27` `ScoreBoardAddPlayer`: raw QWORDs at `+0x28/+0x30`, shared text at
  `+0x38`, DWORD at `+0x3C`, byte at `+0x40`, DWORD at `+0x44`, byte at
  `+0x48`, then byte at `+0x4C`, byte at `+0x51`, and byte at `+0x50`.
- `0x28` `ScoreBoardRemovePlayer`: raw QWORD at `+0x28`.
- `0x29` `ScoreBoardUpdatePlayerStats`: raw QWORD at `+0x28`, then a
  four-byte wire gate. Gate value 1 reads a WORD-length-prefixed blob into
  `+0x34`; other values reset/default that blob destination. The gate is not
  retained by this reader. The constructor initializes `+0x34` to the
  static value `0x1C80CC4`; neither the gate's application meaning nor the
  blob's semantic type is inferred.
- `0x2A` `ScoreBoardUpdatePlayerStatsFromStats`: raw QWORD at `+0x28`, then a
  byte-counted collection at `+0x30` (begin/end/capacity pointers at
  `+0x30/+0x34/+0x38`). Count `0xFF` continues with another count byte; zero
  ends the collection. Each collection item reads a DWORD, then a nested
  structure whose DWORD count frames repeated NUL-terminated shared-text
  entries followed by DWORD tags. The nested tag reader dispatches values
  1..16; the exact per-tag payload layouts remain unresolved (one branch is
  observed to read NUL-terminated shared text). Native top-level entries
  have `0x10`-byte stride. These count/tag values are framing observations; no
  application meanings or field names are assigned.
- `0x2B` `ScoreBoardUpdatePlayerStatus`: raw QWORD at `+0x28`, then byte at
  `+0x30`, DWORD at `+0x34`, byte at `+0x38`, byte at `+0x3C`, byte at
  `+0x41`, and byte at `+0x40`, in that read order. The constructor
  initializes WORD `+0x40` to 1; meanings of the scalar fields remain open.
- `0x2C` `MatchLoadingState`: byte at `+0x28`, then a byte-counted vector at
  `+0x2C/+0x30/+0x34`. Count `0xFF` continues with another count byte; zero
  ends the vector. Each `0x10`-stride item reads raw QWORD, NUL-terminated
  shared text, then one byte.
- `0x2D` `MatchEndAck`: raw QWORD at `+0x28`; it reuses selector `0x28`'s
  `0x356F80` body-reader continuation. On allocation failure, its case
  branches to selector `0x28`'s zero-object block at `0x81F72C`, then reaches
  the same reader at `0x81F72E`.
- `0x2E` `ServerProfiler_SendNames`: two vectors at
  `+0x28/+0x2C/+0x30` and `+0x34/+0x38/+0x3C`, each read by `0x34E700`.
  Each vector uses a byte count with `0xFF` continuation and zero termination;
  each item reads a WORD followed by NUL-terminated shared text and has
  `0x08`-byte native stride.
- `0x2F` `ServerProfiler_SendFrame`: root vector at
  `+0x28/+0x2C/+0x30`, with byte counts and `0xFF` continuation. Each root
  entry is a WORD followed by a recursively counted child vector. Root entries
  have `0x10`-byte native stride (WORD at `+0`, child vector at `+4`). Each
  nested entry reads nine WORDs before its child vector, uses `0x20`-byte
  native stride (first WORD at `+0`, child vector at `+4`, eight trailing
  WORDs at `+0x10..+0x1E`), and recursively repeats the same nested shape.
  These WORDs remain offset-only fields.
- `0x30` `TempConsoleMessage`: four NUL-terminated shared-text reads at
  `+0x28`, `+0x2C`, `+0x30`, and `+0x34`, then a WORD-length-prefixed byte
  blob at `+0x38`. The prefix is a 16-bit little-endian length; zero skips the
  blob read. Its constructor writes `0x1C80CC4` to `+0x38`; that initializer
  value is recorded without assigning it a semantic name.
- `0x31` `ReloadStaticData`: allocation is `0x28` bytes, exactly the common
  message header. No body-reader call follows its constructor.
- `0x32` `EncDebugChatMessage`: one NUL-terminated shared-text field at
  `+0x28` via `0x12EE010`.
- `0x33` `SendRadioMessage`: DWORDs at `+0x28` and `+0x2C`, in that order,
  through the four-byte stream readers `0x3370A0` and `0x353310`.
- `0x34` `NpcBehaviorInfo`: one NUL-terminated shared-text field at `+0x28`
  via the same reader and continuation as selector `0x32`.
- `0x35` `NpcMonitoringLog`: NUL-terminated shared text at `+0x28`, one byte
  at `+0x2C`, then NUL-terminated shared text at `+0x30`.
- `0x36` `NpcNavigationInfo`: raw QWORD at `+0x28`, followed by two distinct
  byte-counted vectors. The first vector at `+0x30/+0x34/+0x38` uses count
  bytes (`0xFF` continues with another count; zero ends); each item is three
  DWORDs (12 wire bytes) and is appended with 12-byte native stride. The
  reader zero-extends each count byte into its item-loop counter and
  decrements it at raw `0x84E390`; the `jnz` at `0x84E391` returns to the
  item reader at `0x84E294`. The second vector at `+0x3C/+0x40/+0x44` uses
  the same count framing and has `0x70`-byte native entries. Each entry begins
  with seven 32-bit values. Its byte counter is incremented and compared with
  the current chunk count at `0x84F374`; `jb` at `0x84F376` returns to raw
  `0x84EC90` while the count is not reached. The instruction-aligned gate
  producer starts at raw `0x84EF2E` (raw `0x84EF30` is inside its `mulss`
  instruction): it squares and sums staged scalar values, subtracts a
  referenced DWORD, ANDs with a referenced 128-bit value, then uses `comiss`
  against a referenced scalar. The following `jb` at `0x84EF54` targets
  cleanup/exit at raw `0x84F3A4`; because `comiss` sets CF for below and
  unordered comparisons, the branch can cover either case. The referenced
  constants and the predicate's application meaning remain unresolved. When
  the branch is not taken, the reader consumes three more DWORDs and one byte.
- `0x37` `NpcHostilityDebugInfo`: raw QWORD at `+0x28`, then a byte-counted
  vector at `+0x30/+0x34/+0x38`. Each wire item is a QWORD followed by a DWORD
  (12 wire bytes), stored at `0x10`-byte native stride. Count `0xFF` continues
  with another count; zero ends the vector.
- `0x38` `NpcPositionalDebugInfo`: raw QWORD at `+0x28`, shared text at
  `+0x30`, then a byte-counted record vector at `+0x34/+0x38/+0x3C`. Each
  record reads a QWORD followed by eight DWORDs (0x28 wire bytes and native
  stride), then the message has a byte at `+0x40`. Count `0xFF` continues
  with another count; zero ends the vector.
- `0x39` `NpcShapeDebugInfo`: raw QWORD at `+0x28`, shared text at `+0x30`,
  then a byte-counted record vector at `+0x34/+0x38/+0x3C`. Each record reads
  a leading byte, sixteen DWORDs, four more DWORDs, and a trailing byte
  (0x52 fixed wire bytes); the native stride is `0x58`. The sixteen-DWORD
  reader processes four groups of four DWORDs. Count `0xFF` continues with
  another count; zero ends the vector.
- `0x3A` `NpcVoxelInfo`: raw QWORD at `+0x28`; a byte-counted three-DWORD
  vector at `+0x30/+0x34/+0x38` (12 wire bytes and native stride); a
  byte-counted DWORD vector at `+0x3C/+0x40/+0x44` (four wire bytes and
  native stride); then a byte at `+0x48`. Both vectors use byte counts with
  `0xFF` continuation and zero termination.
- `0x3B` `DebugDrawInfo`: a byte-counted byte vector at
  `+0x28/+0x2C/+0x30`. Each item consumes one byte and has one-byte native
  stride. Count `0xFF` continues with another count; zero ends the vector.
  The payload remains opaque.
- `0x3C` `NpcDevCmdResponse`: two NUL-terminated shared-text reads at `+0x28`
  and `+0x2C`, using the same text reader in sequence.
- `0x3D` `DevRequestObjectPositions`: bytes at `+0x28` and `+0x29`; a
  byte-counted three-DWORD vector at `+0x2C/+0x30/+0x34`; then byte-counted
  DWORD vectors at `+0x38/+0x3C/+0x40`, `+0x44/+0x48/+0x4C`, and
  `+0x50/+0x54/+0x58`, in that order. Items are 12 or four wire bytes,
  respectively, with matching native strides. All four vectors use byte
  counts with `0xFF` continuation and zero termination.
- `0x3E` `DevRequestSpawnTables`: a byte-counted record vector at
  `+0x28/+0x2C/+0x30`. Each item reads NUL-terminated shared text, five DWORDs,
  one byte, then another NUL-terminated shared text; its native stride is
  `0x20`. Count `0xFF` continues with another count; zero ends the vector.
- `0x3F` `DevRequestResourceNodeDebug`: a byte-counted three-DWORD vector at
  `+0x28/+0x2C/+0x30`, with 12 wire bytes and `0x0C` native stride. Count
  `0xFF` continues with another count; zero ends the vector.
- `0x40` `MissionObjectiveUpdated`: raw QWORD at `+0x28`, DWORD at `+0x30`,
  NUL-terminated shared text at `+0x34/+0x38`, byte at `+0x3C`, then DWORD
  at `+0x40`.
- `0x41` `MissionStatusChanged`: raw QWORD at `+0x28`, then one byte at
  `+0x30`.
- `0x42` `MissionReturnToChanged`: DWORD at `+0x28`, then one byte at
  `+0x2C`.
- `0x43` `MissionsAvailable`: a byte-counted vector at
  `+0x28/+0x2C/+0x30`, followed by raw QWORD at `+0x38`. Each wire item is a
  DWORD then a byte (five bytes); entries are stored at eight-byte native
  stride. The count helper loops over each chunk and reads another count byte
  only when the current count is `0xFF`; zero ends the vector.
- `0x44` `MissionActivationAck` and `0x46` `BountyAbortAck`: DWORD at `+0x28`,
  then bytes at `+0x2C` and `+0x2D`.
- `0x45` `BountyStatusChanged`: DWORD at `+0x28`, then one byte at `+0x2C`.
- `0x47` `BountyActivationAck`: DWORD at `+0x28`, bytes at `+0x2C/+0x2D/+0x2E`,
  DWORDs at `+0x30/+0x34`, byte at `+0x38`, raw QWORD at `+0x40`, then
  NUL-terminated shared text at `+0x48`. This reader is called with the object
  in ECX and the stream as its stack argument; selectors `0x40..0x46` use the
  opposite register/stack ordering. Its constructor writes zero at
  `+0x28/+0x2C/+0x2E/+0x30/+0x34/+0x38/+0x40/+0x44/+0x48`. The zeroing
  WORD store at `+0x2C` also initializes byte `+0x2D`; no allocator zero-fill
  behavior is inferred.
- `0x48` `BountyListActiveAck`: byte at `+0x28`, then one wire byte at `+0x2C`
  stored zero-extended as a DWORD, then a WORD-length-prefixed byte span passed
  to the embedded object at `+0x30`. Its constructor initializes byte `+0x28`,
  DWORD `+0x2C`, and the four-DWORD embedded-object region at `+0x30..+0x3C`;
  the first embedded DWORD is `0x1C80CC4`, the remaining three are zero.
- `0x49` `BountyListActiveDetailsAck` and `0x4A` `BountyListAvailableAck`:
  byte at `+0x28`, then a WORD-length-prefixed byte span passed to the embedded
  object at `+0x2C`. Each constructor initializes byte `+0x28`, stores
  `0x1C80CC4` at `+0x2C`, and zeros DWORDs `+0x30/+0x34/+0x38`.
- `0x4B` `BountyClearAck` and `0x4C` `BountyClearPreviousAck`: byte at `+0x28`,
  then one wire byte at `+0x2C` stored zero-extended as a DWORD. Each
  constructor initializes byte `+0x28` and DWORD `+0x2C`; the second reader
  consumes one byte, not the entire initialized DWORD.
- `0x4D` `BountyListPreviousAck`: byte at `+0x28`, then a WORD-length-prefixed
  byte span passed to the embedded object at `+0x2C`. Its constructor
  initializes byte `+0x28`, stores `0x1C80CC4` at `+0x2C`, and zeros DWORDs
  `+0x30/+0x34/+0x38`.
- `0x4E` `BountyRerollResponse`: DWORD at `+0x28`, byte at `+0x2C`, then
  NUL-terminated shared text at `+0x30`. The constructor initializes DWORD
  `+0x28`, byte `+0x2C`, and DWORD `+0x30`; it does not write the intervening
  bytes `+0x2D..+0x2F`, and this reader does not consume them.
- `0x4F` `DisplayUiTrackBounty`: one DWORD at `+0x28`, read directly by the
  case through `0x3370A0`; unlike `0x48..0x4E`, it has no separate body-reader
  function.
- `0x50` `Achievements`: a byte-counted record vector at
  `+0x28/+0x2C/+0x30`. Each wire record is a DWORD followed by one byte (five
  bytes); native records have eight-byte stride. Count `0xFF` continues with
  another count byte; zero ends the sequence.
- `0x51` `AchievementUnlocked`: one raw QWORD at `+0x28`, via the shared
  `0x356F80` reader continuation (`0x81F72E`, call at `0x81F73C`).
- `0x52` `TotalAchievementPoints`: one DWORD at `+0x28`, via the shared
  `0x3370A0` reader continuation (`0x8206E3`, call at `0x8206F1`).
- `0x53` `MissionCompletionCounts`: a byte-counted record vector at
  `+0x28/+0x2C/+0x30`, followed by NUL-terminated shared text at `+0x34`.
  Each record is a DWORD, QWORD, then DWORD (16 wire bytes) stored at
  24-byte native stride. Count `0xFF` continues with another count byte; zero
  ends the vector. The reader clears the vector before processing chunks.
- `0x54` `ContentUnlocked`: NUL-terminated shared text at `+0x28`, then a
  DWORD at `+0x2C` and raw QWORD at `+0x30`, in that order.
- `0x55` `ClientUIAction`: one NUL-terminated shared-text field at `+0x28`
  through the shared `0x12EE010` continuation (`0x81FB27`, call at `0x81FB34`).
- `0x56` `ArcCompletionHistoryUpdate`: a byte-counted sequence of 16-byte
  records inserted into a tree rooted at `+0x28`; each record is a key DWORD,
  QWORD, then trailing DWORD. Count `0xFF` continues with another count byte;
  zero ends the sequence. Its constructor allocates a `0x20` sentinel and
  initializes its three pointer links to itself. It writes WORD `0x0101` at
  sentinel `+0x0C`; this is one word store spanning two adjacent bytes, not
  evidence for two independent byte fields. The reader resets the tree before
  reading and inserts records through `0x8479F0`; no field meanings are
  inferred.
- `0x57` `JobLedgerEntriesUpdate`: a byte-counted DWORD vector at
  `+0x28/+0x2C/+0x30`. Each item is four wire bytes and four-byte native
  stride. Count `0xFF` continues with another count byte; zero ends the vector.
- `0x58` `TrackRecipe`: one DWORD at `+0x28`, via the same reader and
  continuation as selector `0x52`.
- `0x59` `ClearTrackedRecipe`: no body-reader call; its allocation is exactly
  the `0x28`-byte base object.
- `0x5A` `SlotTech`: one DWORD at `+0x28`, via the same reader and
  continuation as selectors `0x52` and `0x58`.
- `0x5B` `InteractableStatusChanged`: byte at `+0x28`, DWORD at `+0x2C`, raw
  QWORD at `+0x30`, then byte at `+0x38`.
- `0x5C` `SendTipMessage`: byte at `+0x28`, DWORDs at `+0x2C/+0x30`, WORD at
  `+0x34`, then byte at `+0x36`.
- `0x5D` `DebugEventSample`: WORDs at `+0x28/+0x2A`, then bytes at
  `+0x2C/+0x2D`.
- `0x5E` `DebugLagPlayerSample`: five WORDs at `+0x28`, `+0x2A`, `+0x2C`,
  `+0x2E`, and `+0x30`.
- `0x5F` `DebugLagSimulationSample`: thirteen WORDs at two-byte intervals
  from `+0x28` through `+0x40`.
- `0x60` `DebugLagRaiaSample`: four WORDs at `+0x28`, `+0x2A`, `+0x2C`,
  and `+0x2E`.
- `0x61` `DebugEncounterVolumes`: four byte-counted vectors. The first
  contains four-float (`0x10`-wire-byte) records at `+0x28`; the next two
  contain `0x40`-wire-byte records at `+0x34` and `+0x40`; the last starts at
  `+0x4C` and contains NUL-terminated shared text followed by `0x40` bytes.
  Their native strides are respectively `0x10`, `0x40`, `0x40`, and `0x44`.
  Each count is a byte; `0xFF` continues with another count and zero ends that
  vector. The record payloads remain offset-only.
- `0x62` `Trail`: DWORD at `+0x28`, byte at `+0x2C`, then a byte-counted
  vector at `+0x30` of three-float records (12 wire bytes and 12-byte native
  stride). Count `0xFF` continues; zero ends the vector.
- `0x63` `EncounterDebugNotification`: raw QWORD at `+0x28`, then
  NUL-terminated shared-text reads at `+0x30` and `+0x34`.
- `0x64` `EncounterUIScopeIn`: raw QWORD at `+0x28`; a WORD-length-prefixed
  byte blob at `+0x30`; WORD at `+0x40`; a tree at `+0x44` with its root
  pointer and count at `+0x44/+0x48`; then the embedded blob at `+0x4C`
  receives all remaining bytes in the current input slice, if any. Tree
  entries read a QWORD key, QWORD, DWORD, then a byte-counted vector of tagged
  entries. Both count levels use `0xFF` continuation and zero termination.
  Tagged entries have `0x18`-byte native stride. Tag `0` reads a DWORD to
  `+8` and clears DWORD `+0x0C`; tag `1` reads a DWORD to `+0x10`; tags `2`/`3`
  read a QWORD to `+8`; tag `4` reads one byte to
  WORD `+0x14`; tag `6` reads a WORD to `+0x14`; tag `7` reads a byte to WORD
  `+0x14`, then reads a DWORD to `+0x10` when bit `0x02` is set, otherwise a
  QWORD to `+8`; tag `8` writes WORD 1 without a payload. Special tag `0xFE`
  writes WORD 0, and `0xFF` takes a no-payload path. Tag `5` and values
  `0x09..0xFD` reach the same invalid/unsupported path. These tag values and
  fields have no assigned application semantics.
- `0x65` `EncounterUIUpdate`: raw QWORD at `+0x28`; a WORD-length-prefixed
  byte blob at `+0x30`; then all remaining bytes in the current input slice
  are copied into the embedded blob at `+0x40`, if any remain.
- `0x66` `EncounterUIScopeOut`: raw QWORD at `+0x28` through the shared
  `0x356F80` reader continuation at raw `0x81F72E`. The allocation-failure
  branch targets the shared zero-object block at raw `0x81F72C`; this is
  static control flow only and does not assert runtime consequences.
- `0x67` `DisplayUiNotification`: DWORDs at `+0x28/+0x2C`; a byte-counted
  vector at `+0x30`; then byte at `+0x8C`. Each vector record reads a byte,
  NUL-terminated shared text, DWORD, and float. Records have `0x10`-byte
  native stride; count `0xFF` continues with another count and zero ends the
  vector.
- `0x68` `DisplayMoneyBombBanner`: DWORD at `+0x28`, NUL-terminated shared
  text at `+0x2C`, then DWORD at `+0x30`.
- `0x69` `SetPreloadPosition`: sixteen DWORDs at four-byte intervals from
  `+0x28` through `+0x64` (four groups of four in the reader loops).
- `0x6A` `PlaySoundId`: raw QWORDs at `+0x28` and `+0x30`.
- `0x6B` `PlaySoundIdAtLocation`: raw QWORD at `+0x28`, then DWORDs at
  `+0x30`, `+0x34`, and `+0x38`.
- `0x6C` `PlayDialogScriptMessage`: DWORD at `+0x28`, then a vector at
  `+0x2C/+0x30/+0x34` of opaque eight-byte wire records stored at eight-byte
  native stride.
- `0x6D` `StopDialogScriptMessage`: DWORD at `+0x28`, the same eight-byte
  record vector at `+0x2C/+0x30/+0x34`, then one byte at `+0x38`.
- `0x6E` `PingMap`: DWORDs at `+0x28`, `+0x2C`, `+0x30`, and `+0x34`.
- `0x6F` `PingMapMarker`: raw QWORD at `+0x28`, then DWORD at `+0x30`.
- `0x70` `EncounterPublicInfo`: raw QWORD at `+0x28`, then the remaining
  input bytes are passed to the embedded object at `+0x30` through virtual
  method `+0x10`. The reader advances and decrements the stream by that
  remaining-byte count.
- `0x71` `RequestActiveEncounters_Response`: the remaining input bytes are
  passed to the embedded object at `+0x28` through virtual method `+0x10`;
  there are no preceding scalar reads.
- `0x72` `ShoppingListInit`: a byte-counted tree at `+0x28`. Each entry
  consumes two DWORDs and is inserted through the tree helper. Count `0xFF`
  continues with another count byte; zero terminates. The two DWORD roles and
  tree-key semantics remain unknown.
- `0x73` `SetClientDailyInfo`: DWORD at `+0x28`, then a byte-counted sequence
  at `+0x2C`. Each entry reads one DWORD and passes it to helper `0x38EC20`.
  Count `0xFF` continues; zero terminates. The helper's application semantics
  are unresolved.
- `0x74` `GlobalCounterUpdate`: NUL-terminated shared text at `+0x28`, then a
  raw QWORD at `+0x30`.
- `0x75` `GlobalCounterMilestoneInfo`: two separately byte-counted
  collections at `+0x28` and `+0x30`. Each first-collection entry reads
  NUL-terminated text followed by a nested byte-counted collection; each
  nested item reads two DWORDs, NUL-terminated text, and a DWORD. Each
  second-collection entry reads NUL-terminated text and a DWORD. The outer and
  nested counts use `0xFF` continuation and zero termination.
- `0x76` `ChatMessageList`: byte-counted vector at `+0x28`, with
  `0x40`-byte native stride. Each item reads a QWORD at record `+0`, shared
  text at `+8/+0xC`, and bytes at `+0x10/+0x11/+0x12`; then readers at
  `+0x14`, `+0x28`, and `+0x38` process a gated WORD-length-prefixed blob, a
  gated optional QWORD, and a nested value whose field layout is not yet
  resolved. The first helper always consumes its gate byte; when set it reads a
  WORD-length-prefixed byte sequence, and when clear it clears an existing
  referenced blob. The QWORD helper consumes an independent gate byte and
  reads/stores eight bytes only when set. Count `0xFF` continues; zero
  terminates.
- `0x77` `CurrentLoadoutResponse`: QWORD at `+0x28`, DWORDs at `+0x30/+0x34`,
  shared text at `+0x38/+0x3C`, DWORD at `+0x40`, then a byte-counted vector
  at `+0x44`. Vector records have `0x2E0`-byte native stride; each begins
  with DWORD at record `+0`, text at `+4`, nested readers at `+8/+0x14/+0x20`,
  DWORDs at `+0x2C/+0x30`, and a byte gate at `+0x34`. Before testing the
  gate, a DWORD is read unconditionally at `+0x2D4`. When the gate is
  nonzero, readers then fill `+0x38`, `+0xF8`, `+0x1D8`, `+0x1E4`,
  `+0x1F0`, `+0x2D0`, `+0x2D4`, `+0x2D8`, and finally `+0xF4`; thus
  `+0x2D4` is read both before the gate and on its nonzero path. Count `0xFF`
  continues; zero terminates.
- `0x78` `VendorProductsResponse`: DWORD at `+0x28`, QWORD at `+0x30`, DWORD
  at `+0x38`, shared NUL-terminated text at `+0x3C`, DWORD at `+0x40`, then
  byte-counted vectors at `+0x44` and `+0x50`. Each `+0x44` item is a DWORD
  followed by a four-byte float payload; its native stride is eight bytes.
  The `+0x50` vector has `0x30`-byte native records. Each record reads a QWORD
  at `+0`, DWORDs at `+8/+0xC/+0x10`, then two nested byte-counted vectors
  at `+0x14/+0x20`, and a trailing byte at `+0x2C` normalized to a boolean.
  The first nested vector has `0x18`-byte native records carrying QWORD, text,
  and two DWORDs; the second has eight-byte records carrying two NUL-terminated
  text fields. All three vector levels use `0xFF` continuation and zero
  termination.
- `0x79` `VendorPurchaseResponse`: byte at `+0x28`, QWORDs at `+0x30/+0x38`,
  DWORD at `+0x40`, then shared NUL-terminated text at `+0x44`. The reader is
  straight-line and has no field gate or count loop.
- `0x7A` `ConductorGlobalAnnouncement`: the reader first processes a nested
  block at `+0x28`: DWORDs at `+0x28/+0x2C/+0x30`, NUL-terminated text at
  `+0x34/+0x38`, gated values at `+0x3C/+0x4C`, text at `+0x80`, then gated
  values at `+0x5C/+0x70`; a final byte is read at `+0x84`. Each `+0x3C/+0x4C`
  gate consumes one byte, then a DWORD and a separately gated optional DWORD.
  The `+0x5C` gate consumes one byte, then NUL-terminated text and an optional
  DWORD with its own byte gate. The `+0x70` gate consumes one byte, then two
  DWORDs and NUL-terminated text. These are presence gates, not count loops.
  Field semantics and text encoding remain unknown.

These are positional wire layouts only; RTTI class names do not establish the
meaning of any field. None of the `0x58..0x5F` body readers has a count/vector
loop or a field gate. `TrackRecipe` and `SlotTech` reuse the DWORD reader
continuation at `0x8206E3`; `ClearTrackedRecipe` has no body reader. The other
five readers are straight-line scalar reads in the field orders above. Their
primitive helper calls were checked against the one-byte, WORD, DWORD, and
QWORD load/store widths and cursor increments in `FirefallClient.exe.original`.
The `0x60..0x7A` records above likewise remain offset/width observations;
their count framing and tag dispatch are described separately from field
semantics. All nineteen `0x68..0x7A` cases have body-reader calls; none has a
no-reader success epilog.

The shared NUL-terminated text reader used by selectors `0x53..0x55`, `0x68`,
and `0x74..0x7A` reads bytewise through the terminator in repeating 256-byte
chunks; 256 is a chunk size, not a total-string limit. These text fields have
no assigned application semantics or encoding.

Each case in `0x68..0x7A` pushes a descriptor-anchor immediate, a
selector-specific pointer VA, and `0x1E60904` before calling raw `0x3571B0`;
the target bytes are a single `ret`, so no effect for those pushed values is
established. Each derived constructor instead calls its type-specific
base-metadata helper, which stores the descriptor anchor at `+0x0C`, type ID
at `+0x10`, common flag at `+0x08`, parser-context DWORDs at `+0x18/+0x1C`,
and zeroes `+0x20/+0x24` before the derived constructor installs its vtable.
The anchors are opaque metadata pointers, not wire fields.

All eight cases in `0x70..0x77` test the allocation result, branch on null,
and still reach their body reader after either the constructor path or the
zero-object path; static flow alone does not establish the runtime consequence
of the allocation-failure path. Their reader-argument setup differs by case
and is byte-checked; no no-reader success epilog occurs in this batch.
The final cases `0x78..0x7A` likewise test the allocation result and reach their
body readers on both the constructor and zero-object branches. All three have
reader calls and no no-reader success epilog; the allocation-failure
consequence remains a static-flow observation only.

The vectors in selectors `0x6C` and `0x6D` use one-byte chunk counts. Each
count gives the number of eight-byte records in that chunk; `0xFF` means read
another count, and zero terminates the vector. The parser grows the native
eight-byte-stride vector and no overall record limit is established. The
counted collections in `0x72`, `0x73`, `0x75`, `0x76`, and `0x77` also use
one-byte counts with `0xFF` continuation and zero termination as directly
observed; their per-entry widths and nested layouts are listed above. Selectors
`0x70` and `0x71` consume the entire remaining input as embedded blob data.
Selectors `0x76` and `0x77` include the explicit conditional gates described
above.
The `0x70` and `0x71` constructors initialize their embedded members at
`+0x30` and `+0x28` with `slBlob` vtable VA `0x1C80CC4`. The same vtable
initializes the embedded members for selectors `0x74`–`0x76` at `+0x30`,
`+0x30`, and `+0x28`, respectively. Its virtual `+0x10` target is VA
`0x16CC5D0` / raw `0x12CB9D0`: the method calls resize helper `0x12CBCB0`
with the remaining-byte count and a false preserve flag, then, when the blob
backing handle is non-null, copies that many bytes from the input pointer into
the blob's storage via the `memcpy` thunk. Readers `0x5EF1F0`, `0x5EF090`, and
`0x5EEE60` advance the input pointer and decrement remaining length by that
same count after the virtual call. This establishes raw-byte storage, not the
higher-level meaning or schema of those bytes.

The `0x48..0x4E` body-reader functions are called with the stream in ECX and
the message-object pointer on the stack. Selector `0x4F` instead calls the
DWORD helper with the stream in ECX and the destination pointer
`object+0x28` on the stack. The scalar readers consume 1 or 4 wire bytes as
listed; the `0x5BCFA0` helper consumes one byte and stores its zero-extended
value as a DWORD at `+0x2C`.

The shared `0x12EE170` blob reader consumes a 16-bit little-endian length.
Length zero skips the embedded object's virtual `+0x10` copy method. A
nonzero length is checked against available stream bytes, passed with the
source pointer to that method, and then advances the stream by the payload
length. The embedded object's contents and semantics remain unresolved.
`0x12EE010` reads bytewise through the NUL terminator, using an internal
256-byte temporary chunk and repeating until termination; this is a chunk
size, not a total-string limit. Encoding and malformed-input behavior remain
unresolved. These selectors contain no count-framed item loops; no field
semantics are inferred from the RTTI names.

The cases for `0x40..0x47` push a descriptor-anchor VA, a distinct setup
argument, and `0x1E60904` to `0x3571B0`; that target is a one-byte `ret`
(`nullsub`), so no effect is attributed to the call or its arguments. Each
then allocates the size in the table, writes a separate parser-local DWORD
`0x20..0x27` (role unknown), and calls a constructor whose base helper receives
native type ID `0x40..0x47`. The descriptor anchor is copied to header `+0x0C`;
the common header writes `+0x08=1`, type ID at `+0x10`, parser-input DWORDs at
`+0x18/+0x1C`, and zeros at `+0x20/+0x24`. These parser-local values, constructor
type IDs, and table selectors (`0x40..0x47`) are separate observed values;
their numerical relationship does not establish semantic equivalence. The
derived vtables resolve through section-aware COL/TypeDescriptor reads in the
original image. RTTI class names do not establish the meaning of the wire
fields.

Selectors `0x48..0x4F` follow the same setup-call and allocation sequence, with
distinct per-case setup arguments and descriptor anchors recorded in their
layout classes. They store parser-local DWORDs `0x28..0x2F` separately, call
constructors whose base helpers receive type IDs `0x48..0x4F`, and install
distinct RTTI-resolved vtables. These selectors, parser-local values, metadata
anchors, and constructor type IDs remain separate observed values; their
numerical relationships do not establish semantic meaning. The `0x4F`
allocation-failure path zeroes ESI and then forms the direct DWORD-reader
destination as `[ESI+0x28]`; this records the static path only and does not
assert a runtime consequence.

Selectors `0x50..0x57` also call the one-byte-return setup target, allocate the
sizes in the table, store parser-local DWORD literals `0x30..0x37`, and invoke
constructors whose metadata helpers receive type IDs `0x50..0x57`. The literals,
selectors, constructor type IDs, and descriptor anchors remain separate
observations; their numeric relationships do not establish semantic meaning.
The eight derived vtables resolve through section-aware RTTI and have zero
complete-object offsets. Selector `0x56` uses an extended constructor that
initializes its base metadata, installs the derived vtable, and allocates its
tree sentinel. The allocation-failure routes either enter a local zero-object
block or a shared zero-object continuation before the reader calls; this is
static control flow only and does not assert runtime consequences.

Selectors `0x58..0x5F` use the same setup-call/allocation pattern. Their
parser-local DWORD literals are `0x38..0x3F`; the constructors pass native
type IDs `0x58..0x5F` to the selector-specific metadata helpers and install
distinct RTTI-resolved vtables with zero complete-object offsets.
`TrackRecipe` and `SlotTech` share the `0x8206E3` DWORD-reader continuation;
`ClearTrackedRecipe` has no body reader. The remaining readers consume the
ordered byte/DWORD/QWORD/WORD scalar fields recorded above. The checks verify
each reader's destination address, helper call, primitive width, and cursor
increment. No count loop or field gate occurs in these readers. Allocation
failure for `TrackRecipe` and `SlotTech` joins the shared zero-object
continuation at `0x8206E1`; `ClearTrackedRecipe` returns through the common
zero-return block at `0x8217D9`. Selectors `0x5B..0x5F` use local
zero-object blocks immediately before their readers. These are static paths
only; no runtime consequence is asserted.

Selectors `0x60..0x67` use the same setup-call/allocation pattern. Their
parser-local DWORD literals are `0x40..0x47`; the constructors pass native
type IDs `0x60..0x67` to the selector-specific metadata helpers and install
distinct RTTI-resolved vtables with zero complete-object offsets. Checks pin
the case setup arguments, allocator sizes, constructor/helper calls, metadata
anchors, derived-vtable stores, and reader argument order. Selectors
`0x60..0x65` and `0x67` continue through their readers after a local
zero-object block if allocation fails. Selector `0x66` instead branches to
the shared zero-object block at `0x81F72C` on failure and jumps to the shared
QWORD-reader continuation at `0x81F72E` after successful construction. These
are static control-flow observations only; no runtime consequence is
asserted.

Binary fingerprints include table indexes and targets, case setup arguments,
allocation sizes and failure blocks, constructor type/metadata/vtable order,
reader field arguments, primitive-reader calls, byte-count consumption,
native vector increments, and the count-loop continuation and termination
branches. They cover selector `0x36`'s conditional tail and its `0x70` native
stride as well as selector `0x43`'s byte count, DWORD-plus-byte items, and
eight-byte stride, plus the `0x48..0x4F` scalar/embedded-object/text-reader
argument order, byte/DWORD/WORD widths, zero-length blob gate, and shared-text
NUL/chunk loop. The `0x50..0x57` checks pin the table targets and RTTI,
allocation-failure branches, constructor/header stores, all reader field
arguments, the five-/16-/four-byte counted record shapes, native strides, and
the `0x56` tree-sentinel word initializer and count loop. The `0x58..0x5F`
checks pin the new targets/RTTI, allocation and failure paths, metadata
initializers, scalar field order, and primitive load/store widths. These checks
assert wire framing only; unresolved field, gate, count, predicate, and tag
meanings remain unassigned. The `0x60..0x67` fingerprints additionally cover
the four-vector and variable-record readers, the `0xFF` count continuations,
the shared `0x66` reader path, embedded-blob remainder copies, and the `0x64`
tag-dispatch table and conditional payload paths.

On allocation failure, selectors `0x20..0x25`, `0x27..0x2C`, and `0x2E..0x2F`
set the object register to zero but still fall through to their body-reader
call. Selector `0x2D` branches through selector `0x28`'s zero-object block and
shared reader continuation. Selector `0x26` instead branches directly to the
shared zero-return block. Selector `0x31` also branches directly to the shared
zero-return block; selectors through `0x4E` continue through their readers with
the object register set to zero on allocation failure. Selector `0x4F` instead
continues to its direct DWORD helper after forming the destination from the
zero object register. Selectors `0x50`, `0x53`, `0x54`, `0x56`, and `0x57`
enter local zero-object blocks; `0x51`, `0x52`, and `0x55` reuse the zero-object
continuations at `0x81F72C`, `0x8206E1`, and `0x81FB25`, respectively, before
their readers. These are static control-flow observations only; their
runtime consequence is not asserted.
Selectors `0x58` and `0x5A` also branch to the shared zero-object continuation
at `0x8206E1` before the DWORD reader; `0x59` branches to the common zero-return
block at `0x8217D9`. Selectors `0x5B..0x5F` each branch to a local
zero-object block before their scalar-reader call. These are static
control-flow observations only; their runtime consequence is not asserted.
Selectors `0x60..0x65` and `0x67` likewise branch to local zero-object blocks
before their readers; selector `0x66` branches to `0x81F72C`, then reaches the
shared reader at `0x81F72E`. These are static control-flow observations only;
their runtime consequence is not asserted.
Selectors `0x32` and `0x34` share the zero-object continuation at raw
`0x81FB25` and the text-reader continuation at `0x81FB27`. These are static
control-flow observations only; their runtime consequence is not asserted.

### Route index 2, dispatcher value `0x53`: `AreaVisualData::LootObjectCollected`

The route2 block `0x81EE63` subtracts `0x53` from its dispatcher value; equality
branches to case `0x81EF1C`. That case requests `0x40` bytes from allocator
`0x12B7FB0`, stores local DWORD `0xCD` (role unknown), calls constructor
`0xADBAA0`, then body reader `0x83AD90`. The constructor passes `0x53` to helper
`0xB07660` and installs vtable VA `0x1C723C0`, whose RTTI is
`firefall::AreaVisualData::LootObjectCollected`.

The body reader writes a DWORD at object+`0x28`, followed by raw QWORDs at
`+0x30` and `+0x38`, using readers `0x3370A0` and `0x356F80`. Field meanings
remain unknown.

#### Conditional post-parse consumer for route 2

The route selector in `0x81CC50` maps leading bytes `0x13..0x19` to route index
2 and parser block `0x81EE63`; its `0x53` comparison selects the case at
`0x81EF1C`. This is a parser-family choice, not the later receiver selector.
The constructor's base helper writes the metadata pointer at object+`0x0C`
(`0x1BB0224`, whose string is `msg-83`), discriminator `0x53` at `+0x10`, and
the two envelope-key DWORDs at `+0x18/+0x1C`. The parser's local `0xCD` is at
`[esp+0x20]`, not the object discriminator. After the body reader, `0x81EF58`
restores EAX from ESI, preserving the constructed object pointer (or null on
the allocation-failure path).

That result is preserved by both direct parser callers: `0x821BA0` calls
`0x81CC50` at `0x821D87`, and its epilog does not overwrite EAX; wrapper
`0x821DE0` similarly calls at `0x821E1E` and returns EAX unchanged. In the
registered ReplayReader path, the indirect parser call at `0xBD6D3C` moves its
EAX result into EDI at `0xBD6D41`, stores that same value in the queued
12-byte result record at `0xBD6D63`, then calls the queue helper at `0xBD6D66`.
The queue-consumer and callback-context gates described in “Parser result path
and dialog-message primary vftables” remain conditional; this trace does not
assert that every parsed route-2 object reaches application dispatch.

On the callback-context `+8` branch, `0x7FDC60` first requires the low byte of
message `+0x18` to be zero. It then diverts a message whose `+0x10` equals the
nested-message sentinel; otherwise it requires a non-null context `+8` and
calls `0x81BB90` with the original message pointer. `0x81BB90` reads only
message `+0x10`, subtracts `0x20`, and selects the table entry at VA `0xC1D6DC`
(raw `0x81CADC`). Discriminator `0x53` reaches VA `0xC1D001` (raw `0x81C401`),
which makes the virtual call at `0x81C420` through receiver slot `+0xD0`,
passing the same message pointer. The concrete receiver was established from
its constructor/vtable bytes and RTTI chain, not inferred from message RTTI:
the `tfGameProtocolHandler` vtable at VA `0x1C71EC0` has VA `0xECB180`
(`0xACA580` raw `.text`) in slot `+0xD0`. The receiver method therefore is
`0xACA580`; the `0x81BB90` table does not branch on vptr or `+0x0C`.

The handler uses message `+0x34` as a candidate C-string pointer, falling back
to the empty string at VA `0x1C5EA68` when null, and compares it bytewise with
`"missions"` at VA `0x1BAE9E0`. On the equal branch it gets owner state through
VA `0x89E8C0` (raw `0x49DCC0`), resets the end pointer of the vector at
`[owner+0x43B8]+0x18` to its begin pointer, and calls VA `0xCF0D80` (raw
`0x8F0180`) with message `+0x28/+0x2C` as the source range. That helper's copy
routine VA `0xCF26D0` (raw `0x8F1AD0`) copies 24-byte records and stores the
new end pointer at vector `+4`. This is an observed conditional state update;
the record semantics and runtime range validity are not established. The
non-equal comparison branch instead calls VA `0xD38A40` (raw `0x937E40`) with
the address of message `+0x28`.

Boundary remains unresolved: the route-2 reader writes the DWORD at `+0x28`
and QWORDs at `+0x30/+0x38`; the derived constructor zeros `+0x30..+0x3C`,
but neither writes `+0x2C`. The receiver uses `+0x2C` as a range endpoint.
Also, `+0x34` is part of the parsed QWORD at `+0x30`, but the handler treats
that DWORD as a possible C-string pointer. No semantic names are assigned to
these fields. Distinct parser families with discriminator `0x53` retain
different `+0x0C` metadata pointers and vptrs, while this callback selector
collapses them onto the same `+0xD0` receiver method. Read-only IDA
disassembly/byte reads and the recorded binary-evidence checks agree with the
requested original executable at the relevant code sites; the IDB and sample
were not modified or executed.

Value `0x54` selects case `0x81EECB` (`AreaVisualData::AudioEmitterSpawned`).
It allocates `0x40` bytes, stores local DWORD `0xCE`, calls constructor
`0xADBB40`, then reader `0x83ACA0`. The constructor passes `0x54` to base
helper `0xB076D0` and installs vtable VA `0x1C72358` (RTTI
`firefall::AreaVisualData::AudioEmitterSpawned`). Its reader writes a QWORD at
`+0x28`, DWORDs at `+0x30/+0x34/+0x38`, and a WORD at `+0x3C`; helper readers
are `0x356850`, `0x3568E0`, and `0x336AC0`.

Value `0x55` selects case `0x81EE79` (`AreaVisualData::ParticleEffectSpawned`).
It allocates `0x70` bytes, stores local DWORD `0xCF`, calls constructor
`0xADBBC0`, then reader `0x83AAA0`. The constructor passes `0x55` to base
helper `0xB07740`, installs vtable VA `0x1C72368` (RTTI
`firefall::AreaVisualData::ParticleEffectSpawned`), and directly zeroes
DWORDs at `+0x28/+0x2C/+0x30`, bytes at `+0x40/+0x58/+0x64`, a DWORD at
`+0x5C`, and a WORD at `+0x60`. The reader processes, in order, a QWORD at
`+0x28`, DWORDs at `+0x30/+0x34/+0x38/+0x3C`, a conditional record at `+0x40`
(one input presence byte and, when nonzero, three DWORDs at `+0x44/+0x48/+0x4C`),
four WORDs at `+0x50..+0x56`, a byte at `+0x58`, a DWORD at `+0x5C`, a WORD
at `+0x60`, and a second conditional record at `+0x64` (one input presence
byte and, when nonzero, DWORDs at `+0x68/+0x6C`). Helpers `0x5C0270` and
`0x83A9C0` implement the conditional records; their present values are copied
into native fields, while an absent input clears only the presence byte.
No field meanings are inferred from the RTTI names.

### Route index 3, switch entry 0: `Vehicle::AbilityActivated`

The route3 block `0x81EF6D` computes `arg_0 - 0x53`: instruction bytes
`83 C0 AD` sign-extend the encoded immediate `0xAD` to `-0x53`. It bounds the
result to indices `0..8` before indexing the nine-entry table at VA `0xC2260C` (raw
`0x821A0C`). Entry0 reaches case `0x81EF83`, which allocates `0x30` bytes
and writes stack local `[esp+0x20]=0xD0` at raw `0x81EF91` (role unknown; not
the message discriminator). It calls constructor `0xB093C0`, then reader
`0x83A840`. The constructor passes `0x53` to helper `0xB474B0` and
installs vtable VA `0x1C72998` (RTTI `firefall::Vehicle::AbilityActivated`).

The reader writes DWORDs at object+`0x28` and `+0x2C` through `0x3370A0`.
Their meanings remain unknown.

#### Conditional post-parse consumer for route 3

The base constructor stores discriminator `0x53` at message+`0x10`, just as
route 2 does. The callback path is conditional: message+`0x18` must have a
zero low byte, message+`0x10` must not be the nested-message sentinel, and the
callback context's `+8` receiver must be non-null before `0x7FDC60` calls
`0x81BB90` with this object. The receiver dispatcher selects solely on
message+`0x10`; it does not check the message vptr or metadata pointer.
Consequently, discriminator `0x53` reaches the same receiver slot `+0xD0` and
method `0xACA580` described for route 2.

That method reads message+`0x34` as a candidate C-string pointer and compares
it to `"missions"`. On equality, it uses message+`0x28/+0x2C` as a source
range for the vector at owner+`0x43B8`. On inequality, it passes the address
of message+`0x28` to the map helper at `0x937E40`, which walks records in
24-byte steps until the supplied end pointer. The two DWORDs parsed by
`Vehicle::AbilityActivated` therefore serve as that helper's range endpoints
on this path, but their semantics are not otherwise named.

There is a static extent mismatch: this parser requests `0x30` bytes and reads
only through `+0x2C`, while the shared method loads a pointer from `+0x34`.
That address lies beyond the recorded allocation extent. This establishes a
potential out-of-extent read only if a route-3 object reaches this conditional
handler path; the IDA evidence does not establish that it happens at runtime,
the validity of the loaded pointer, or the meaning/range validity of
`+0x28/+0x2C`. Read-only disassembly and original-file fingerprints support
the parser, callback gate, discriminator dispatch, and handler offsets; no
runtime behavior is inferred.

Switch entry1 (dispatcher value `0x54`) selects case `0x81EFD4`,
`Vehicle::AbilityFailed`. It allocates `0x30` bytes, stores local DWORD
`0xD1`, calls constructor `0xB09440`, then reader `0x83A7A0`. The constructor
passes `0x54` to base helper `0xB47520` and installs vtable VA `0x1C72978`
(RTTI `firefall::Vehicle::AbilityFailed`). The reader writes DWORDs at
`+0x28/+0x2C` through `0x3370A0`.

#### Switch entry2 (`0x55`): `Vehicle::PublicCombatLog`

Entry2 selects case `0x81F025`, allocates `0x40` bytes, stores local DWORD
`0xD2`, and calls constructor `0xB094C0`. Its success path joins the shared
reader tail at `0x81CF5B`, which calls body reader `0x5F3350` at `0x81CF67`.
The constructor passes `0x55` to base helper `0xB47590`, installs vtable VA
`0x1C72988` (RTTI `firefall::Vehicle::PublicCombatLog`), zeroes the byte at
`+0x28`, seeds DWORD `+0x2C` with `0x1C80CC4`, and zeroes DWORDs `+0x30`,
`+0x34`, and `+0x38`.

The body reader consumes a DWORD at `+0x28`. Only value `1` continues to
`0x12EE170`, which reads a little-endian WORD length and passes that span to
the embedded blob object at `+0x2C` through its virtual `+0x10` method,
rather than copying payload bytes inline at that offset. A zero length
performs no copy. Other values take the reader's
error path. The gate's application meaning and blob contents remain unknown.

#### Switch entry3 (`0x56`): `Vehicle::CurrentPoseUpdate`

Entry3 selects `0x81F056`, allocates `0x58` bytes, stores local DWORD `0xD3`,
and calls constructor `0xB095B0`; the body reader `0x83A530` is called at
`0x81F08E`. The constructor passes `0x56` to `0xB47600` and installs vtable VA
`0x1C72958` (RTTI `firefall::Vehicle::CurrentPoseUpdate`).

The reader begins at object `+0x28` with a flag byte:

1. Bit `0x01` selects a byte at `+0x29`; otherwise it reads a WORD at `+0x2A`.
2. It always reads a byte at `+0x2C`. If bit `0x02` is clear, it reads a WORD
   at `+0x2E`.
3. If any bit in `0x0C` is set, it zeroes DWORD `+0x30` and copies three raw
   bytes there; otherwise it reads DWORDs `+0x34`, `+0x38`, and `+0x3C`.
4. For `flags & 0x30 == 0`, it reads four WORDs at `+0x44..+0x4A`. The exact
   value `0x10` selects one byte at `+0x40`; `0x20` selects bytes `+0x41..+0x43`;
   `0x30` reads none of these union fields.
5. For `flags & 0xC0 == 0`, it reads WORDs at `+0x50`, `+0x52`, and `+0x54`.
   If the masked value is `0x40` or `0x80`, it zeroes DWORD `+0x4C` and copies
   three bytes there; `0xC0` reads no final payload.

These are observed layout and branch conditions only; the flag and field
meanings are unresolved.

#### Switch entry4 (`0x57`): `Vehicle::TookDebugWeaponHitPublic`

Entry4 selects `0x81F0A8`, allocates `0xA8` bytes, stores local DWORD `0xD4`,
and calls constructor `0xB09630`. It joins the shared body-reader tail at
`0x81CD94` (`0x81CDA0` calls `0x5F60F0`). The constructor passes `0x57` to
`0xB47680`, installs vtable VA `0x1C72968` (RTTI
`firefall::Vehicle::TookDebugWeaponHitPublic`), and zeroes bytes `+0x58` and
`+0x88`.

The reader consumes a byte at `+0x28`, DWORDs at `+0x2C/+0x30`, a WORD at
`+0x34`, six DWORDs at `+0x38..+0x4C`, and another DWORD at `+0x50`.
`0x5F4FD0` then reads a presence byte for state at `+0x58`; when present,
`0x5F4D60` reads a QWORD, three DWORDs, four values through the shared
four-value reader, and two bytes into a temporary record before `0x5F4F60`
updates the object. When absent, it clears the existing presence byte.

`0x5F5FB0` reads a second presence byte at `+0x88`. When set, it reads a raw
QWORD, then `0x5F5420` consumes a sequence of counted, 64-byte records using
`0x6087E0`; `0x5F5EB0` applies that opaque state. The absent path clears any
existing state. The embedded record and field semantics are unresolved.

#### Switch entry5 (`0x58`): `Vehicle::ForcedMovement`

Entry5 selects `0x81F0DC`, allocates `0x1E0` bytes, stores local DWORD `0xD5`,
and calls constructor `0xB09710`. The constructor passes `0x58` to `0xB476F0`,
installs vtable VA `0x1C72938` (RTTI
`firefall::Vehicle::ForcedMovement`), then initializes the embedded `0x1B8`
state at `+0x28` through `0x92B8A0`. Relative to that embedded state, the
initializer sets DWORDs at `+0/+0x08/+0xEC/+0xF0/+0x110/+0x114/+0x190..+0x19C`
and WORDs at `+0x0C/+0xE8/+0x1A4` to zero, sets `+0x6C/+0xD8/+0x174` WORDs
to `0x0101`, and sets `+0x04` to its zero argument.

The body reader `0x92F800` is called at `0x81D62F`. It reads a selector byte
into embedded `+0x08`, a DWORD into `+0x04`, then calls `0x5FDA00` for an
optional QWORD. The QWORD halves, when present, are copied to embedded
`+0x110/+0x114`; otherwise those slots are initialized from data at VA
`0x1E68530`. The selector is bounds-checked to `0..13`, then indexes the
14-entry table at VA `0xD308BC` (raw `0x92FCBC`). The reader also clears
embedded byte `+0x0D` on its common return. No case names or state semantics
are inferred; each case below records only its observed reads:

| State case | Reader target (raw) | Additional reads, relative to embedded state, in order |
|---:|---:|---|
| 0 | `0x92FC83` | None |
| 1 | `0x92F8E8` | Three DWORDs at `+0x118`, `+0x184`, `+0x178`; DWORD `+0x190`; derives `+0x194 = +0x190 + 1` |
| 2 | `0x92F92E` | Three DWORDs `+0x118`, three DWORDs `+0x178`, DWORDs `+0x190/+0x194/+0x19C`, bytes `+0x1A4/+0x1A5` |
| 3 | `0x92F990` | DWORDs `+0x190/+0x194`; three DWORDs `+0x10`; 16 bytes at `+0x1C` then constant WORD `1` at `+0x2C`; three DWORDs `+0x118`; 16 bytes at `+0x124` then constant WORD `1` at `+0x134`; DWORDs `+0x198/+0x19C`; bytes `+0x1A4/+0x1A5` |
| 4 | `0x92FA16` | Three DWORDs `+0x118`; DWORDs `+0x190/+0x194/+0x19C`; three DWORDs `+0x184` |
| 5 | `0x92FA5E` | Three DWORDs `+0x178`; DWORDs `+0x190/+0x194`; byte `+0x0C` |
| 6 | `0x92FA94` | Three DWORDs `+0x178`; DWORDs `+0x190/+0x194/+0x19C` |
| 7 | `0x92FBCF` | DWORDs `+0x190/+0x194` |
| 8 | `0x92FAC6` | Three DWORDs `+0x118`; DWORDs `+0x190/+0x194/+0x198` |
| 9 | `0x92FAF8` | DWORDs `+0x190/+0x194/+0x198`; three DWORDs `+0x178` |
| 10 | `0x92FB32` | DWORDs `+0x190/+0x194`; three DWORDs `+0x178`; DWORDs `+0x198/+0x19C`; byte `+0x1A5` |
| 11 | `0x92FB87` | DWORDs `+0x190/+0x194`; three DWORDs `+0x118`; 16 bytes at `+0x124` then constant WORD `1` at `+0x134` |
| 12 | `0x92FBC1` | Three DWORDs `+0x118`; DWORDs `+0x190/+0x194` |
| 13 | `0x92FBED` | DWORDs `+0x190/+0x194`; QWORD `+0x110`; three DWORDs `+0x118`; DWORDs `+0x1A8/+0x1AC/+0x19C/+0x198`; byte `+0x1A4`; DWORDs `+0x1A0/+0x1B0` |

`0x384C80` reads three DWORDs; `0x384C20` reads four DWORDs and writes its
following WORD marker. Its shared `0x384B40` reader treats the four values as
floats and applies a numeric range check. The discriminator variants remain
opaque despite this structural mapping.

#### Switch entry6 (`0x59`): `Vehicle::ForcedMovementCancelled`

Entry6 selects case `0x81F110`, `Vehicle::ForcedMovementCancelled`. It
allocates `0x30` bytes, stores local DWORD `0xD6`, calls constructor
`0xB097A0`, then reader `0x83A440`. The constructor passes `0x59` to base
helper `0xB47760` and installs vtable VA `0x1C72928` (RTTI
`firefall::Vehicle::ForcedMovementCancelled`). The reader writes a DWORD at
`+0x28` and a WORD at `+0x2C`, through `0x3370A0` and `0x336AC0`.

#### Switch entry7 (`0x5A`): `Vehicle::FlipPunch`

Entry7 selects `0x81F161`, allocates `0x40` bytes, stores local DWORD `0xD7`,
and calls constructor `0xB09820`; body reader `0x83A370` is called at
`0x81F198`. The constructor passes `0x5A` to `0xB477D0` and installs vtable
VA `0x1C72948` (RTTI `firefall::Vehicle::FlipPunch`). The reader consumes six
DWORDs, in order, at `+0x28/+0x2C/+0x30/+0x34/+0x38/+0x3C`. Their meanings
remain unknown.

#### Switch entry8 (`0x5B`): `Vehicle::DebugMovementUpdate`

Entry8 selects `0x81F1B2`, allocates `0xA0` bytes, stores local DWORD `0xD8`,
and calls constructor `0xB098A0`. The constructor passes `0x5B` to
`0xB47840`, installs vtable VA `0x1C728F8` (RTTI
`firefall::Vehicle::DebugMovementUpdate`), and seeds WORD `+0x88` with
`0x0101`.

Reader `0x83A230` reads a byte at `+0x28`, then delegates at `+0x2C` to
`0x608370`. That helper reads DWORDs `+0x2C/+0x30/+0x34`, reads four values
through `0x384B40` and copies their 16 bytes from temporary storage to
`+0x78..+0x87`, writes WORD `1` at `+0x88`, and reads DWORDs
`+0x8C/+0x90/+0x94`. The outer reader then reads WORD `+0x98` and DWORD
`+0x9C`. Field meanings remain unknown.

### Route index 4, dispatcher value `0x53`: `Deployable::TookHit`

The route4 block `0x81F206` subtracts `0x53` and branches to case `0x81F29E`
on equality. The case allocates `0x60` bytes, stores local DWORD `0xD9` (role
unknown), calls constructor `0xB27E80`, then reader `0x83A120`. The constructor
passes `0x53` to helper `0xB47C40`, installs vtable VA `0x1C727A8` (RTTI
`firefall::Deployable::TookHit`), and zeroes byte `+0x28` and WORD `+0x50`.

The body reader calls conditional-record reader `0x5F0CF0` for object+`0x28`,
reads bytes at `+0x50` and `+0x51`, a WORD at `+0x58`, then a byte at `+0x5A`.
The conditional record's internal field meanings and all outer field meanings
remain unknown.

#### Conditional post-parse consumer for route 4

The constructor stores discriminator `0x53` at message+`0x10`, so this
message shares route 2 and route 3's callback selector. Subject to the same
callback gates described above, `0x81BB90` sends it through receiver slot
`+0xD0` to `0xACA580`, without checking message RTTI or vptr. That method
reads message+`0x34` as a candidate C-string pointer. For this message,
`+0x34` is also the high DWORD of the QWORD read into the conditional record
at `+0x30/+0x34` when the record's presence byte at `+0x28` is nonzero. The
record helper also conditionally stores a nested QWORD at `+0x40/+0x44`,
then a DWORD at `+0x48` and byte at `+0x4C`. If the outer record is absent,
the parser clears its presence byte (and clears the nested-presence byte only
when already set); it does not clear the QWORD at `+0x30/+0x34`. The
constructor likewise initializes `+0x28` and `+0x50`, not the QWORD.

The same handler uses message `+0x28/+0x2C` as source-range endpoints for
either the `"missions"` vector branch or the map-update branch. Here `+0x28`
is the conditional record's presence byte, and the parser does not initialize
`+0x2C`; neither is identified as a range by the route-4 body reader. These
are static field overlaps between different parser/consumer views, not proof
that route 4 reaches the handler or that the candidate pointer/range is valid.
Unlike route 3, the `+0x34` read is within this class's `0x60`-byte allocation,
but its value is conditionally populated as part of a raw QWORD. Keep the
field's semantic interpretation unresolved.

Value `0x54` selects case `0x81F24D`, `Deployable::AbilityProjectileFired`.
It allocates `0x68` bytes, stores local DWORD `0xDA`, calls constructor
`0xB27F40`, then reader `0x839F50`. The constructor passes `0x54` to base
helper `0xB47CB0`, installs vtable VA `0x1C72764` (RTTI
`firefall::Deployable::AbilityProjectileFired`), and directly zeroes byte
`+0x58`. The reader writes eight WORDs at `+0x28..+0x36`, DWORDs at
`+0x38/+0x3C`, byte `+0x40`, DWORD `+0x44`, byte `+0x48`, DWORDs at
`+0x4C/+0x50`, then an optional QWORD record at `+0x58`: a presence byte
followed, when nonzero, by QWORD `+0x60`. Reader helpers are `0x336AC0`,
`0x34B740`, `0x6097B0`, `0x3568E0`, `0x353310`, `0x337180`,
`0x3370A0`, and `0x5FDA00` (which uses `0x356F80` for the QWORD).

Value `0x55` selects case `0x81F21C`, `Deployable::PublicCombatLog`. It
allocates `0x40` bytes, stores local DWORD `0xDB`, calls constructor
`0xB27FE0`, then joins the shared reader tail at `0x81CF5B`, which calls
`0x5F3350` with `this = object+0x28`. The constructor passes `0x55` to base
helper `0xB47D20`, installs vtable VA `0x1C72754` (RTTI
`firefall::Deployable::PublicCombatLog`), zeroes byte `+0x28`, and initializes
an embedded blob object at `+0x2C` with vtable VA `0x1C80CC4` and three zero
DWORDs at `+0x30/+0x34/+0x38`. The reader first consumes a DWORD and requires
it to equal `1`; the nonmatching path reaches an error stub. On the matching
path, `0x12EE170` reads a WORD byte-count followed by that many bytes and
passes the span to the embedded blob's indirect vtable slot `+0x10`, then
advances the input cursor. Blob semantics and the indirect destination remain
unresolved; RTTI alone does not establish combat-log meaning.

### Route index 5, dispatcher value `0x53`: `Turret::WeaponProjectileFired`

The route5 block `0x81F2EF` accepts dispatcher value `0x53` and enters case
`0x81F2F9`. It allocates `0x48` bytes, stores local DWORD `0xDC` (role
unknown), calls constructor `0xB36520`, then body reader `0x839EB0`. The
constructor passes `0x53` to helper `0xB47D90`, installs vtable VA `0x1C726E8`
(RTTI `firefall::Turret::WeaponProjectileFired`), and zeroes byte `+0x38`.

The body reader uses `0x3370A0` at `+0x28`, `0x3568E0` at `+0x2C`, `+0x30`,
and `+0x34`, then helper `0x5C0270` at `+0x38`. Field meanings and the role of
the final helper remain unresolved.

#### Conditional post-parse consumer for route 5

The constructor stores discriminator `0x53` at message+`0x10`, so under the
same callback gates route 5 also reaches the shared slot `+0xD0` method
`0xACA580`. It loads message+`0x34` as a candidate C-string pointer. In this
message layout that field is populated by the third `0x3568E0` DWORD reader,
not by a text reader; the constructor only zeroes the byte at `+0x38`.

The shared method then interprets message `+0x28/+0x2C` as a source range for
its vector or map update. Route 5 parses a DWORD at `+0x28` and another DWORD
at `+0x2C`; the parser does not establish that these values are valid range
endpoints or that the DWORD at `+0x34` is a valid C-string pointer. All three
fields are within the `0x48`-byte allocation. This is a static collision
between parser and consumer interpretations, conditional on the callback
gates; it does not establish route-5 runtime delivery or field semantics.

### Route index 6, dispatcher value `0x53`: `LootStoreExtension::LootObjectCollected`

The route6 block `0x81F34A` accepts dispatcher value `0x53` and enters case
`0x81F354`. It allocates `0x40` bytes, stores local DWORD `0xDD` (role
unknown), calls constructor `0xB43A70`, then body reader `0x839DB0`. The
constructor passes `0x53` to helper `0xB48040`, installs vtable VA `0x1C72608`
(RTTI `firefall::LootStoreExtension::LootObjectCollected`), and initializes
DWORDs at `+0x30`, `+0x34`, `+0x38`, and `+0x3C` to zero.

The body reader writes a DWORD at object+`0x28`, then raw QWORDs at `+0x30` and
`+0x38`, through `0x3370A0` and `0x356F80`. Field meanings remain unknown.

#### Conditional post-parse consumer for route 6

The constructor stores discriminator `0x53` at message+`0x10`, so the same
callback gates and receiver dispatch can route this message through slot
`+0xD0` to `0xACA580`. That method treats message+`0x34` as a candidate
C-string pointer; here it is the high DWORD of the first QWORD read at
`+0x30/+0x34`. The constructor initially zeros that QWORD, but the body reader
overwrites it from the stream.

The handler also consumes message `+0x28/+0x2C` as source-range endpoints.
Route 6 reads a DWORD at `+0x28` and leaves `+0x2C` untouched in the traced
constructor and body reader; these are not established as valid range
endpoints. Both the candidate field and range operands are within the
`0x40`-byte allocation. The consumer path remains conditional, and neither
runtime reachability nor field semantics are established.

Nested-table entry0 maps selector `0x53`: route0 block `0x81CCAD` dispatches it
through the table at VA `0xC22444` (raw `0x821844`) to raw case
`0x81CCC3`. That case allocates `0x48` bytes, calls constructor `0x524C30`,
then body reader `0x5F6B10`. Constructor RTTI identifies the object as
`firefall::Character::MarketRequestComplete`. The reader stores, in order,
a byte at object+`0x28`, raw QWORD at+`0x30`, byte at+`0x38`, and three
NUL-terminated shared-text values at+`0x3C/+0x40/+0x44`. Field meanings
remain unknown; this case's RTTI name does not identify the remaining table cases.
`MatrixSecondaryCommandDispatcherNativeLayout.cs` and
`CharacterMarketRequestCompleteNativeLayout` preserve the route bounds and
this bounded object layout without implementing a parser.

#### Conditional post-parse consumer for nested selector 0x53

The route0 constructor calls base helper `0x5A0400` with `0x53`, which stores
that discriminator at message+`0x10`. It therefore shares the same conditional
callback gates and receiver selection as the other `0x53` message families:
when message+`0x18` has a zero low byte, `+0x10` is not the nested sentinel,
and callback context `+8` is non-null, `0x81BB90` dispatches to receiver slot
`+0xD0` / method `0xACA580`. This table is selected by the stored discriminator,
not the route0 nested selector or the message RTTI.

The shared method treats message+`0x34` as a candidate C-string pointer. For
this type, it is the high DWORD of the raw QWORD parsed at `+0x30/+0x34`; the
reader does not parse that DWORD as text. The method also uses `+0x28/+0x2C`
as source-range endpoints, whereas the route0 reader writes a single byte at
`+0x28` and does not initialize `+0x2C` in the traced constructor/reader.
The discriminator match alone does not establish that this message reaches
the handler or that either candidate pointer/range is valid. These are
conditional static field overlaps; do not infer runtime behavior or assign
semantic names to the raw fields.

The same table's entry1 maps selector `0x54` to VA `0xC1D914` / raw case
`0x81CD14`. It allocates `0x40` bytes, calls constructor `0x524DF0`, then
body reader `0x5F68D0`. The constructor installs vtable VA `0x1C67268`
(RTTI `firefall::Character::ReceiveWeaponTweaks`) and initializes an embedded
blob object at `+0x2C` with vtable VA `0x1C80CC4`. The reader processes two
raw bytes into object `+0x28` and `+0x29`, then passes the current stream
cursor and all remaining bytes to the blob object's vtable slot `+0x10`.
That slot resolves to raw `0x12CB9D0`, which resizes the blob to the supplied
length with preservation disabled and copies those bytes; the reader advances
the cursor and subtracts that length from the remaining count. The body
therefore has two observed byte fields followed by a tail blob, without a
separate blob-length prefix. The case also stores local DWORD `0x5C`, whose
role is unknown. The two byte meanings and blob contents remain unresolved;
RTTI alone does not establish weapon semantics. Its body reader attempts a
stream refill when a byte read sees zero remaining, but the exact malformed
short-input behavior is not established.
`CharacterReceiveWeaponTweaksNativeLayout` records the offsets and call
targets without implementing a parser.

#### Conditional post-parse consumer for nested selector 0x54

The shared discriminator table maps `0x54` to switch target VA `0xC1D02C` /
raw `0x81C42C`, which calls receiver slot `+0xD4` at raw `0x81C44B`.
The `tfGameProtocolHandler` vtable entry points to VA `0xECB3E0` / raw method
`0xACA7E0`. Under that conditional routing, the method builds an event payload
with labels `contentType`, `contentId`, and `duration`, then invokes the event
sink's vtable slot `+0xAC` with event code `130`.

The `contentType` helper treats message `+0x28` as a shared-text pointer.
The other two helpers record DWORD values from `+0x2C` and `+0x30`; the payload
builder reserves eight 8-byte entries. These source interpretations overlap
the parser layout: `+0x28/+0x29` are two bytes, `+0x2C` starts the embedded
blob object, and `+0x30` is the first DWORD in that object's state. This is a
static parser/consumer interpretation conflict, not proof the message reaches
this handler or that the event is emitted at runtime. The labels do not resolve
the underlying field meanings. The handler method's target and label strings
are checked against the original executable by `Startup.Checks`.

### Selector 0x55: `TookDebugWeaponHitPublic`

Entry2 of the same table maps selector `0x55` to VA `0xC1D965` / raw case
`0x81CD65`. It allocates `0xA8` bytes, stores local DWORD `0x5D` (role
unknown), then calls constructor `0x524EE0` and reader `0x5F60F0`. The
constructor passes `0x55` to `0x5A04E0`, installs vtable VA `0x1C67258`
(RTTI `firefall::Character::TookDebugWeaponHitPublic`), and zeros object
bytes `+0x58` and `+0x88`. The RTTI label does not establish field semantics.

The reader's fixed scalar prefix is listed in **input order** below. Input
positions are nominal offsets from the reader's initial cursor, assuming the
individual helpers obtain the requested bytes:

| Input offset | Width | Object offset | Observation |
| ---: | ---: | ---: | --- |
| `0x00` | 1 byte | `+0x00` | Raw byte |
| `0x01` | DWORD | `+0x04` | Raw DWORD |
| `0x05` | DWORD | `+0x08` | Raw DWORD |
| `0x09` | WORD | `+0x0C` | Raw WORD |
| `0x0B`, `0x0F`, `0x13`, `0x17`, `0x1B`, `0x1F` | DWORD each | `+0x10..+0x24` | Six raw DWORDs |
| `0x23` | DWORD | `+0x28` | Raw DWORD |

This is nominally `0x27` input bytes before the first conditional tail.
Tail `+0x30` first consumes a gate byte. If nonzero, `0x5F4D60` reads
`0x26` payload bytes into a temporary record: QWORD at temporary `+0`,
three DWORDs at `+8/+0xC/+0x10`, four DWORD bit patterns at `+0x14..+0x20`
that `0x384B40` subsequently uses as floats, then two bytes at `+0x24/+0x25`.
The record is copied into object `+0x38..+0x5D`; the object marker at `+0x30`
is set to one when its prior value is zero. If the gate is zero, a nonzero
object marker is cleared without an observed clear of the rest of the record.
Thus, on this branch the tail consumes nominally `1 + 0x26` bytes. The
constructor's zero at object `+0x58` lies inside the four-float byte range
`+0x4C..+0x5B`, which this reader can overwrite.

Tail `+0x60` then consumes another gate byte. On its nonzero branch, it reads
a QWORD and enters `0x5F5420`. That routine reads a count byte and parses
that many records, each exactly `0x40` bytes (`0x6087E0` reads sixteen
DWORDs per record). A count of `0xFF` causes another count byte and record
block to be read; any other count ends the sequence, with zero representing
no records. The temporary vector is copied into the object beginning at
`+0x74` (begin/end/capacity at `+0x74/+0x78/+0x7C`); the associated marker is
at `+0x60`. A zero gate releases the current vector when the marker is
nonzero, clears those three DWORDs, and clears the marker.

The nonzero `+0x60` path supplies a 12-byte local prefix to `0x5F5EB0`, but its
QWORD reader writes only the first eight bytes. At raw `0x5F6032..0x5F6042`,
the caller zeroes a separate three-DWORD vector temporary at `[esp+0x20]`;
the QWORD scratch begins at `[esp+0x14]` (`0x5F604A`) and is not included in
those stores. `0x356F80` writes two DWORDs and advances the stream by eight
bytes (`0x356FA5..0x356FB5`). The same scratch address is passed to
`0x5F5EB0` at `0x5F6069..0x5F6070`. That helper copies source DWORDs at
`+0/+4/+8` to object `+0x68/+0x6C/+0x70` (`0x5F5EE9/0x5F5EEF/0x5F5EF5`);
therefore its third input DWORD, the local at `[esp+0x1C]`, is not explicitly
initialized or populated by the QWORD read before being copied to object
`+0x70`. It is not a third wire DWORD. The reader's constructor only zeros
object `+0x58` and `+0x88`; it does not initialize `+0x70`. This establishes
the local data-flow discrepancy, not its runtime consequence. The receiver
uses only message `+0x28`, so no traced consumer explains the copied value.
Refill and short-input behavior of the scalar helpers is also not established.
`CharacterTookDebugWeaponHitPublicNativeLayout` preserves the observed
offsets, widths, branch boundaries, and record size without implementing a
parser or naming semantic fields.

#### Conditional post-parse consumer for nested selector 0x55

The shared discriminator table maps `0x55` to switch target VA `0xC1D057` /
raw `0x81C457`, which calls receiver slot `+0xD8` at raw `0x81C476`.
The `tfGameProtocolHandler` vtable entry points to VA `0xECB4E0` / raw method
`0xACA8E0`. The method loads message `+0x28` and treats it as a SharedText
pointer, retaining the referenced value before calling helper `0x8EDBD0`.
That helper builds an event payload under key `"action"` from the text (using
a fallback when null), passes its string data and length through `0xC240F0`,
and invokes the event sink's vtable slot `+0xAC` with code `197`.

The parsed object instead records `+0x28` as one of its raw DWORDs via the
generic four-byte reader `0x3370A0`. This is a conditional static
representation conflict, not proof that this message reaches the handler or
that the value is a valid SharedText pointer. No semantic meaning is assigned
to the field or event.

### Selector 0x56: `TookDebugWeaponHit`

Entry3 maps selector `0x56` to VA `0xC1D9BA` / raw case `0x81CDBA`. It
allocates `0xA8` bytes, stores local DWORD `0x5E` (role unknown), then calls
constructor `0x524FC0`. The constructor passes `0x56` to `0x5A0550`, installs
vtable VA `0x1C67248` (RTTI `firefall::Character::TookDebugWeaponHit`), and
zeros object bytes `+0x58` and `+0x88`. After construction, the case branches
to shared body-reader setup `0x81CD94`; its call at `0x81CDA0` invokes the
same reader `0x5F60F0` used by selector `0x55`.

This proves the two selectors share the same observed field-reading routine,
not that their distinct RTTI classes have identical semantics. The reader's
offset/width ledger and conditional tails are recorded under selector
`0x55`; no semantic field names are inferred for either class.

#### Conditional post-parse consumer for nested selector 0x56

The shared discriminator table maps `0x56` to switch target VA `0xC1D082` /
raw `0x81C482`, which calls receiver slot `+0xDC` at raw `0x81C4A1`. The
`tfGameProtocolHandler` vtable entry contains VA `0xECEA30`, which maps through
the PE `.text` section to raw file offset `0xACDE30`: image base `0x400000`,
section RVA `0x1000`, and raw pointer `0x400` give
`0x400 + (0xECEA30 - 0x400000 - 0x1000)`. This is a function entry, not an
interior instruction target. The earlier `0xACEA30`/`0xACE430` interpretations
were arithmetic errors and do not describe this vtable slot.

At raw `0xACDE30`, the method obtains the current context and checks its state
pointer at `+0x43D0`; a zero pointer enters a diagnostic call with code `1653`.
It then passes the parsed message's `+0x28` address to helper `0x927F60`, using
the state pointer as `this`. The helper checks the DWORD at input `+4`, treats
input `+0` as a tree-container pointer, and merges each source node into the
context tree at `+0x30`. It looks up nodes using the four-byte key at source
node `+0x10`; on a missing key it inserts a node, then copies source DWORDs
`+0x14/+0x18/+0x1C` into destination `+0x14/+0x18/+0x10`. After processing a
nonzero-count collection it calls the event sink vtable slot `+0xAC` with code
`222` and null payload arguments.

There is a notable parser/consumer representation mismatch. The selector
`0x56` case passes message `+0x28` as the body-reader destination; the shared
reader writes a byte at `+0x28` and a DWORD at `+0x2C`. The consumer instead
interprets those positions as a tree-container pointer and its count. The
constructor's observed explicit stores do not initialize the `+0x28` field.
This is a static layout conflict only; it does not establish that the selector
reaches the method, that the collection is malformed at runtime, or that a
runtime failure occurs.

### Selector 0x57: `DebugWeaponStats`

Entry4 maps selector `0x57` to VA `0xC1D9E7` / raw case `0x81CDE7`. It
allocates `0x38` bytes, stores local DWORD `0x5F` (role unknown), then calls
constructor `0x5250A0` and body reader `0x5F4A10`. The constructor passes
`0x57` to `0x5A05C0` and installs vtable VA `0x1C67238` (RTTI
`firefall::Character::DebugWeaponStats`). The reader consumes eight WORDs
in order into object offsets `+0x28,+0x2A,+0x2C,+0x2E,+0x30,+0x32,+0x34,+0x36`.
These occupy the final 16 bytes of the allocation. Their meanings remain
unknown; the RTTI name does not establish their semantics.

#### Conditional post-parse consumer for nested selector 0x57

The outer handler switch uses table VA `0xC1D6DC`; selector `0x57` indexes
entry `0x37`, whose target VA `0xC1D0AD` maps to raw case `0x81C4AD`. That
case calls `tfGameProtocolHandler` slot `+0xE0` at raw `0x81C4CC`. Its vtable
entry contains VA `0xECEF10`, mapped to raw `0xACE310`.

At raw `0xACE310`, the consumer obtains the current context and reads its
state pointer at `+0x43D0`. If that pointer is zero, it enters a diagnostic
call with code `1653`; the observed path then continues to use the state
pointer. The method resets the end pointer of a vector at state `+0x90` to its
begin pointer, then calls `0x929920` with the DWORDs at message `+0x28` and
`+0x2C` as the input range's begin and end addresses. The helper treats the
range as DWORD elements, computes its length as `(end - begin) / 4`, grows
the destination vector when its capacity is insufficient, and copies the
range. The consumer then calls the event sink vtable slot `+0xAC` with code
`227` and null payload arguments, including when the helper sees an empty
range.

The parser/consumer representations do not line up directly: the parser
reader writes eight WORDs from `+0x28` through `+0x36`, while the consumer
interprets the first two DWORDs (`+0x28/+0x2C`) as a pointer range. The
constructor's observed stores initialize metadata through `+0x24` and install
the vtable; no explicit initialization of `+0x28..+0x36` was observed.
These are static access patterns only and do not establish that the selector
reaches this consumer or that the range is invalid at runtime.

### Selector 0x58: `RewardInfo`

Entry5 maps selector `0x58` to VA `0xC1DA38` / raw case `0x81CE38`. It
allocates `0x48` bytes, stores local DWORD `0x60` (role unknown), then calls
constructor `0x525120` and reader `0x5F47C0`. The constructor passes `0x58`
to `0x5A0630`, installs vtable VA `0x1C67218` (RTTI
`firefall::Character::RewardInfo`), and zeros DWORDs at object `+0x30/+0x34`.
The reader then fills a byte at `+0x28`, a byte at `+0x29`, a raw QWORD at
`+0x30`, and three raw DWORDs at `+0x38/+0x3C/+0x40`. These reads nominally
consume `0x16` input bytes and write through object `+0x43`; the constructor's
zeroes at `+0x30/+0x34` are overwritten by the QWORD read. Gaps at
`+0x2A..+0x2F` and the final allocation bytes `+0x44..+0x47` are not written
by this reader. Field meanings remain unknown; the RTTI name does not prove
reward-related semantics.

#### Conditional post-parse consumer for nested selector 0x58

The outer handler switch uses table VA `0xC1D6DC`; selector `0x58` indexes
entry `0x38`, whose target VA `0xC1D0D8` maps to raw case `0x81C4D8`. That case
calls `tfGameProtocolHandler` slot `+0xE4` at raw `0x81C4F7`. The vtable entry
contains VA `0xECB580`, mapped to raw `0xACA980`.

At raw `0xACA980`, the consumer reads the context state pointer at `+0x43C8`.
If it is zero, the method emits diagnostic code `1651` and continues to the
state-update helper `0x909120`. It passes that helper the DWORD at message
`+0x28` and literal `1`; the remaining `RewardInfo` fields are not read by
this consumer. The helper searches its ordered tree using the message DWORD
as the key (node comparison value at `+0x14`). A matching key returns without
the follow-up. If the key is absent and the DWORD is zero, it returns false;
if absent and nonzero, it clears/resets the container and calls
`0x9092A0` with the pair `{1, messageDWORD}` and a zero flag.

The `0x9092A0` follow-up has conditional side effects. One branch calls
`SendFindServiceProviderRequest(1)` when its local selection path sets the
request flag; a successful control-routine result also sets the state byte at
`+8` and calls the event sink slot `+0xAC` with code `219` and null payload
arguments. This is a transitive, branch-dependent path, not an unconditional
effect of receiving selector `0x58`. The consumer itself returns the helper's
result and does not directly emit event `219`.

There is a static parser/consumer representation mismatch: the reader writes
bytes at message `+0x28/+0x29`, while the consumer reads a DWORD at `+0x28`,
which also covers the unwritten `+0x2A/+0x2B` gap. The constructor's observed
zeroes begin at `+0x30`, so they do not establish initialization of those four
bytes. This is an observed layout conflict only; it does not prove that the
selector reaches the consumer or establish a runtime outcome.

### Selector 0x59: `ProgressionXpRefresh`

Entry6 maps selector `0x59` to VA `0xC1DA89` / raw case `0x81CE89`. It
allocates `0x38` bytes, stores local DWORD `0x61` (role unknown), then calls
constructor `0x5251B0` and reader `0x5F4170`. The constructor passes `0x59`
to `0x5A06A0`, installs vtable VA `0x1C67228` (RTTI
`firefall::Character::ProgressionXpRefresh`), and zeros the vector header at
object `+0x28/+0x2C/+0x30`.

The reader treats that header as begin/end/capacity pointers, resets end to
begin, then reads count bytes. For each count it reads that many records of
five raw DWORDs (`0x14` bytes each) and inserts them into the vector.
`0x5200F0` copies the record as 16 bytes plus a DWORD and advances vector end
by `0x14`. A count of `0xFF` reads 255 records and then causes another count
byte; any count below `0xFF` terminates the sequence, with zero meaning no
records in that block. Nominal input consumption is one count byte plus
`count * 0x14` bytes per block. No per-reader cap on repeated `0xFF` blocks is
established. The five DWORD meanings and the RTTI's semantic relation to
progression remain unknown. `CharacterProgressionXpRefreshNativeLayout`
records the vector header, record width, and count continuation marker
without assigning field names or implementing a parser.

#### Conditional post-parse consumer for nested selector 0x59

The outer handler switch uses table VA `0xC1D6DC`; selector `0x59` indexes
entry `0x39`, whose target VA `0xC1D103` maps to raw case `0x81C503`. That case
calls `tfGameProtocolHandler` slot `+0xE8` at raw `0x81C522`. The vtable entry
contains VA `0xECB5D0`, mapped to raw `0xACA9D0`.

The vtable target begins with a context lookup and reads the state pointer at
context `+0x43C8`. If it is zero, the method emits diagnostic code `1651` and
continues to use it. It then treats the pointed-to object as a tree container:
`[state]` is a sentinel pointer and the DWORD at sentinel `+4` is passed to
tree-clear helper `0x90A7B0`. After that call, it resets sentinel fields
`+0/+4/+8` to the sentinel, clears the size DWORD at state `+4`, and sets byte
state `+8` to `1`.

The method does not load the dispatched message argument or any
`ProgressionXpRefresh` payload fields in its observed body. Consequently the
parser's vector header and five-DWORD records are not consumed by this receiver
path. This is a static observation about the receiver instructions; it does
not establish why the sender includes those fields, whether another route
consumes them, or what semantic effect the state-tree reset has.

### Selector 0x5A: `ReceivedDeferredXP`

Entry7 maps selector `0x5A` to VA `0xC1DADC` / raw case `0x81CEDC`. It
allocates `0x38` bytes, stores local DWORD `0x62` (role unknown), then calls
constructor `0x5252A0` and reader `0x5F3A80`. The constructor passes `0x5A`
to `0x5A0710`, installs vtable VA `0x1C67208` (RTTI
`firefall::Character::ReceivedDeferredXP`), and zeros the vector header at
object `+0x28/+0x2C/+0x30`.

The reader resets vector end to begin, reads one count byte, then parses that
many records of four raw DWORDs (`0x10` bytes each) into the vector. A count
of `0xFF` parses 255 records and causes another count byte; any count below
`0xFF` terminates the sequence, with zero meaning no records in that block.
The reader stores each 16-byte record and advances vector end by `0x10`;
`0x5F3880` is the vector-growth helper. Nominal input consumption is one
count byte plus `count * 0x10` bytes per block. No per-reader cap on repeated
`0xFF` blocks is established. The four DWORD meanings and the RTTI's
semantic relation to XP remain unknown. `CharacterReceivedDeferredXPNativeLayout`
records the vector header, record width, and count continuation marker without
assigning field names or implementing a parser.

#### Conditional post-parse consumer for nested selector 0x5A

The outer handler switch uses table VA `0xC1D6DC`; selector `0x5A` indexes
entry `0x3A`, whose target VA `0xC1D12E` maps to raw case `0x81C52E`. That case
calls `tfGameProtocolHandler` slot `+0xEC` at raw `0x81C54D`. The vtable entry
contains VA `0xECB630`, mapped to raw `0xACAA30`.

The consumer reads the DWORD at message `+0x28`, gets the current context
through `0x49DCC0`, then calls `0x4BCBD0` with arguments
`(messageDWORD, 9, 0, 0)`. It does not read the vector end/capacity pointers
or iterate the 16-byte records. Since the parser stores a vector begin pointer
at `+0x28`, this receiver passes that pointer value as the control routine's
first DWORD; the target's intended interpretation is not established.

`0x4BCBD0` performs additional global/configuration lookups and has early
false-return branches. One branch dispatches event code `50` through the
event sink at vtable slot `+0xAC` with null payload arguments, then may call
`0x94E4E0` when its context field at `+0x4140` is nonzero before returning
true. These are conditional helper effects, not guaranteed consequences of
the selector. The `ReceivedDeferredXP` record meanings and the relation of
this control call to the parsed records remain unresolved.

### Selector 0x5B: `PublicCombatLog`

Entry8 maps selector `0x5B` to VA `0xC1DB2F` / raw case `0x81CF2F`. It
allocates `0x40` bytes, stores local DWORD `0x63` (role unknown), then calls
constructor `0x525390` and reader `0x5F3350`. The constructor passes `0x5B`
to `0x5A07A0`, installs vtable VA `0x1C671F8` (RTTI
`firefall::Character::PublicCombatLog`), zeros byte `+0x28`, and initializes
an embedded blob at `+0x2C` with vtable VA `0x1C80CC4`.

At this case's call site, the reader's `this` points to object `+0x28`, and
the input stream is passed separately. The reader consumes a raw DWORD and
continues only when its value is `1`; on that branch it calls
`0x12EE170` with destination `this+4`, which maps to the embedded blob at
object `+0x2C`. The helper reads a WORD length followed by that many bytes.
Thus the nominal successful body is `[raw DWORD 1][WORD length][blob bytes]`,
with minimum six bytes for a zero-length blob. Other DWORD values reach a
diagnostic path whose exact failure/return behavior is not established.
The reader does not store the raw DWORD into object `+0x28`; it is only the
observed gate. Blob contents and field semantics remain unknown.

#### Conditional post-parse consumer for nested selector 0x5B

The outer handler switch uses table VA `0xC1D6DC`; selector `0x5B` indexes
entry `0x3B`, whose target VA `0xC1D159` maps to raw case `0x81C559`. That case
calls `tfGameProtocolHandler` slot `+0xF0` at raw `0x81C578`. The vtable entry
contains VA `0xECB260`, mapped to raw `0xACA660`.

The receiver reads a QWORD at message `+0x30/+0x34`, checks byte `+0x38`, and
uses byte `+0x28` and DWORD `+0x2C` in selected branches. It chooses the
empty/sentinel route when the QWORD is zero or when its high DWORD is
`0xFFFFFFFF` and its low DWORD masked with `0xFFFFFF00` equals
`0xFFFFFF00` (the value range `0xFFFFFFFFFFFFFF00..0xFFFFFFFFFFFFFFFF`).
Otherwise it takes the ordinary QWORD route. In either route, byte `+0x38`
selects a flagged or unflagged helper.

The consumer gets a state pointer from current context `+0x43BC`. A zero
pointer causes diagnostic `1647`, after which the code continues into that
state. For a zero/sentinel QWORD it calls `0xB901F0` when byte `+0x38` is set,
or `0xB903B0` when it is clear. For an ordinary QWORD with the byte set, it
calls `0xB90A30` on the map subobject at state `+8`; with the byte clear, it
searches that map through `0xB90810` and passes the returned range to
`0x36F480`. These helpers perform ordered-tree lookup/update work, but the
records' field meanings and the control meaning of the byte remain unknown.

The parser's successful branch reads a raw DWORD equal to `1`, then delegates
its embedded body at `+0x2C` to a WORD-length-prefixed byte reader. The
consumer's QWORD and byte accesses overlap that embedded object, but the
captured bytes do not establish semantic field names or a higher-level record
schema.

### Selector 0x5C: `PrivateCombatLog`

Entry9 maps selector `0x5C` to VA `0xC1DB81` / raw case `0x81CF81`. It
allocates `0x40` bytes, stores local DWORD `0x64` (role unknown), then calls
constructor `0x525480`. The constructor passes `0x5C` to `0x5A0810`, installs
vtable VA `0x1C671E8` (RTTI `firefall::Character::PrivateCombatLog`), zeros
byte `+0x28`, and initializes the blob subobject at `+0x2C`.

After construction, the case branches to the shared reader setup at raw
`0x81CF5B`; it passes `this = object+0x28` and the input stream to the same
reader `0x5F3350` used by selector `0x5B`. The reader's `this+4` destination
therefore maps to object `+0x2C`, and its successful body format is the same
observed DWORD-1 gate followed by a WORD-length-prefixed blob. Shared parsing
does not prove that `PublicCombatLog` and `PrivateCombatLog` have identical
semantics; the non-1 diagnostic path remains unresolved.

#### Selector 0x5C post-parse consumer

The outer handler switch maps selector `0x5C` through VA `0xC1D184` / raw case
`0x81C584`, which invokes slot `+0xF4` at raw `0x81C5A3`. The slot at raw
`0x1870BB4` contains VA `0xECC460`, mapping to receiver raw `0xACB860`.
The earlier tentative target at VA `0x859010` was based on the wrong vtable
offset and is not this receiver.

The receiver reads the parsed message's byte at `+0x28`, embedded blob object
at `+0x2C`, blob data pointer at `+0x30`, WORD at `+0x34`, and byte at `+0x36`.
It passes the `+0x2C` value to `0x391470`, which maps a numeric key through a
global table and produces shared text; empty lookup uses a separate fallback
path. A nonzero `+0x30` value is passed to the first method of global interface
`0x1E00100`, then appended through `0xC23FE0`. The `+0x28` byte is used as a
key for a 12-byte-stride global table; a matching entry conditionally
contributes two more text values. The WORD at `+0x34` indexes a global table
of floats and is passed to `0xC242F0`; the byte at `+0x36` is zero-extended
and passed to `0xC243E0`.

Finally, the receiver dispatches application event `122` through sink slot
`+0xAC`. This connects the parsed `PrivateCombatLog` object to an application
event path, but the encoded field labels and the higher-level meaning of the
values remain unresolved. These observations are from the receiver's static
data flow; the shared parser body does not establish semantic equivalence with
selector `0x5B`.

### Selector 0x5D: `AnimationUpdated`

Entry10 maps selector `0x5D` to VA `0xC1DBAB` / raw case `0x81CFAB`. It
allocates `0x30` bytes, stores local DWORD `0x65` (role unknown), then calls
constructor `0x525570` and body reader `0x5F30F0`. The constructor passes
`0x5D` to `0x5A0880` and installs vtable VA `0x1C671D8` (RTTI
`firefall::Character::AnimationUpdated`). The reader stores a WORD at object
`+0x28` followed by a byte at `+0x2A`, nominally consuming three input bytes.
The field meanings remain unknown; the RTTI name does not establish
animation-related semantics.

#### Selector 0x5D post-parse consumer

The outer handler switch maps selector `0x5D` through VA `0xC1D1AF` / raw case
`0x81C5AF`, which calls slot `+0xF8` at raw `0x81C5CE`. The corresponding
vtable entry at raw `0x1870BB8` contains VA `0xECCB40`, mapped to receiver raw
`0xACBF40`.

The receiver returns when handler context `+8` is null. Otherwise it reads a
DWORD at context `+8` as the reference counter, then combines that value's
high 16 bits with the two message WORDs at `+0x28` and `+0x2A`. Each candidate
is moved by one `0x10000` period when the distance exceeds `0x8000`, producing
two extended values near the reference counter. These are arithmetic
observations; the code does not establish whether they represent ticks,
sequence numbers, or another domain.

The receiver forwards the extended pair, message byte `+0x2C`, and a mode
derived from byte `+0x2D` to relay helper `0x3343B0`. The relay accepts indices
through 10, modes through 3, and only when its state byte `+6` is zero and
byte `+5` is nonzero. It selects a 16-byte record at `this + 16*(mode+5)` and
passes it to queue helper `0x334460`. That helper stores a 16-byte record
containing the extended pair, the message index byte, and a selected endpoint
of the pair; the endpoint choice is controlled by relay-state byte `+0x40`.
The observed write cursor wraps modulo `0xBB8` (3000 records).

The RTTI label and WORD/byte locations give no stronger semantic names for
the transmitted values. Downstream record consumers and the precise domain
of the extended pair remain unresolved.

### Selector 0x5E: `RaiaNPCDebugging`

Entry11 maps selector `0x5E` to VA `0xC1DBFC` / raw case `0x81CFFC`. It
allocates `0x30` bytes, stores local DWORD `0x66` (role unknown), then calls
constructor `0x5255F0`. The constructor passes `0x5E` to `0x5A08F0`, installs
vtable VA `0x1C671B8` (RTTI `firefall::Character::RaiaNPCDebugging`), and
zeros object field `+0x28`.

The case branches to shared reader setup `0x81FB27`. The caller passes the
input stream and the address of object `+0x28` to helper `0x12EE010`, also
documented as the SDB block-name reader in `src/Serialization/README.md`.
That helper consumes raw bytes through and including the first NUL, copies
the preceding bytes into the shared-text field, and reads in chunks of at
most `0x100` bytes using a `0x104`-byte temporary buffer. The chunk size is
not an established total-string limit; no overall maximum or encoding is
proven. The field’s text meaning remains unknown, and the RTTI name does not
establish NPC-debugging semantics.

#### Selector 0x5E post-parse consumer

The outer handler switch maps selector `0x5E` through VA `0xC1D1DA` / raw case
`0x81C5DA`, then invokes slot `+0xFC` at raw `0x81C5F9`. The vtable entry at
raw `0x1870BBC` contains VA `0xECCBE0`, mapped to receiver raw `0xACBFE0`.
That receiver checks handler-context pointer `+8`, reads a counter through
context `+8`, and consumes five WORDs at message offsets `+0x28`, `+0x2A`,
`+0x2C`, `+0x2E`, and `+0x30`. Each value is extended around the same
reference counter using the same `0x10000` wrap period and `0x8000` half-range
seen in selector `0x5D`. Under gates on global state `0x1E0004C`, the receiver
converts five signed differences to floats and passes them to `0x481430` with
five distinct state/output locations.

This is a static parser/consumer mismatch: the selector-`0x5E` parser allocates
`0x30` bytes and reads one NUL-terminated shared-text field into `+0x28`,
whereas this consumer treats the same message as at least five WORDs through
`+0x30` (read end `+0x32`). The receiver's final WORD therefore extends two
bytes past the declared allocation. The parser supplies a text field rather
than five explicit WORD values; whether that field's storage overlaps the
receiver's other WORD reads is unresolved. The constructor initializes base
fields only through `+0x24`; whether the allocator initializes the remaining
portion is not established. Do not assign semantics to these conflicting
fields or infer runtime impact from the static mismatch alone.

### Selector 0x5F: `WeaponProjectileFired`

Entry12 maps selector `0x5F` to VA `0xC1DC2D` / raw case `0x81D02D`. It
allocates `0x38` bytes, stores local DWORD `0x67` (role unknown), then calls
constructor `0x525700`. The constructor passes `0x5F` to helper `0x5A0960`,
installs vtable VA `0x1C671C8` (RTTI
`firefall::Character::WeaponProjectileFired`), and zeros object field
`+0x30`.

Body reader `0x5F2990` reads four WORDs into object fields `+0x28`, `+0x2A`,
`+0x2C`, and `+0x2E`. It then calls `0x5F27E0` with `this = object+0x30`.
That helper consumes one gate byte; a nonzero byte causes three more WORDs
to be read and stored at `+0x32`, `+0x34`, and `+0x36`, with marker byte
`+0x30` set to one. A zero gate clears the marker when it is set. The WORD
reader checks for two available input bytes before reading and advances the
stream by two. Thus the observed nominal body size is 9 bytes without the
optional tail or 15 bytes with it. Field meanings remain unknown; the RTTI
name alone does not establish projectile semantics.

#### Selector 0x5F post-parse consumer

The outer handler switch maps selector `0x5F` through VA `0xC1D205` / raw case
`0x81C605`, then invokes slot `+0x100` at raw `0x81C624`. The vtable entry at
raw `0x1870BC0` contains VA `0xECCF30`, mapped to receiver raw `0xACC330`.
This routine reads 13 WORDs, in observed order at offsets `+0x28`, `+0x2A`,
`+0x2C`, `+0x2E`, `+0x32`, `+0x30`, `+0x36`, `+0x34`, `+0x38`, `+0x3A`,
`+0x3C`, `+0x3E`, and `+0x40`. Each is zero-extended, converted to a float,
and conditionally passed to `0x481430` with a separate label and output
location when global state `0x1E0004C` has byte `+4` set and byte `+5` clear.

The parsed layout does not provide 13 WORD values: its fixed prefix is four
WORDs, followed by a gate byte and an optional three-WORD tail. Its maximum
body length is 15 bytes, and the allocated object is `0x38` bytes. The
receiver's last WORD at `+0x40` reads through `+0x41`, ten bytes beyond that
allocation; its `+0x38..+0x40` WORD reads are beyond the declared object too.
Whether allocator padding or adjacent storage changes runtime consequences is
not established. Preserve this as a static parser/consumer extent mismatch,
not a runtime conclusion or an inferred projectile schema.

### Selector 0x60: `AbilityProjectileFired`

Entry13 maps selector `0x60` to VA `0xC1DC7E` / raw case `0x81D07E`. It
allocates `0x68` bytes, stores local DWORD `0x68` (role unknown), then calls
constructor `0x5257A0`. That constructor passes `0x60` to base helper
`0x5A09F0`, installs vtable VA `0x1C671A8` (RTTI
`firefall::Character::AbilityProjectileFired`), and zeros the presence byte
at object `+0x58`. The role of the base-helper argument remains unknown.

Body reader `0x5F22A0` reads eight WORD fields at `+0x28`, `+0x2A`,
`+0x2C`, `+0x2E`, `+0x30`, `+0x32`, `+0x34`, and `+0x36`; five DWORD
fields at `+0x38`, `+0x3C`, `+0x44`, `+0x4C`, and `+0x50`; and byte fields
at `+0x40` and `+0x48`. It then calls `0x5FDA00` with `this = object+0x58`.
That helper consumes a gate byte at input offset `0x26`; when nonzero, it
reads an eight-byte QWORD through `0x356F80`, stores it at `+0x60/+0x64`,
and sets the presence byte at `+0x58` to one. A zero gate clears a set
presence byte. The nominal body length is `0x27` bytes without the QWORD
and `0x2F` bytes with it. The reader does not write object `+0x54..+0x57`;
that gap and all field meanings remain unresolved. RTTI alone does not
establish ability-projectile semantics.

#### Selector 0x60 post-parse consumer

The outer handler switch maps selector `0x60` through VA `0xC1D230` / raw case
`0x81C630`, then invokes slot `+0x104` at raw `0x81C64F`. The vtable entry at
raw `0x1870BC4` contains VA `0xECD580`, mapped to receiver raw `0xACC980`.
The receiver reads four WORDs from message offsets `+0x28`, `+0x2A`, `+0x2C`,
and `+0x2E`, the parser's first four WORD fields. Each read is zero-extended
and converted to a float. When global state `0x1E0004C` has byte `+4` set and
byte `+5` clear, it passes the value to `0x481430` with a distinct descriptor
and output-state offset; those identifiers are recorded in
`CharacterAbilityProjectileFiredConsumerNativeLayout`.

The receiver does not consume the parser's remaining four WORDs, its later
DWORD/byte fields, or its optional QWORD as metrics. The input field meanings
and metric descriptor semantics remain unknown; neither the RTTI name nor
these downstream uses establish ability-projectile semantics.

### Selector 0x61: `ProjectileHitReported`

Entry14 maps selector `0x61` to VA `0xC1DCCF` / raw case `0x81D0CF`. It
allocates `0x30` bytes, stores local DWORD `0x69` (role unknown), then calls
constructor `0x525840`. The constructor passes `0x61` to helper `0x5A0A60`
and installs vtable VA `0x1C67198` (RTTI
`firefall::Character::ProjectileHitReported`).

Body reader `0x5F20E0` reads WORDs into object fields `+0x28` and `+0x2A`,
then bytes into `+0x2C` and `+0x2D`. The observed nominal body length is
six bytes. Field meanings remain unknown; the RTTI name does not establish
projectile-hit semantics.

#### Selector 0x61 post-parse receiver

The outer handler switch maps selector `0x61` through VA `0xC1D25B` / raw case
`0x81C65B`, then calls slot `+0x108` at raw `0x81C67A` with the current
message pointer. The vtable entry at raw `0x1870BC8` contains VA `0xECA340`,
mapped to receiver raw `0xAC9740`. This receiver loads its pointer argument
into `ESI`, sets `ECX` to `0x16` (22), and executes `REP LODSD`; it performs
no writes and leaves the final loaded DWORD (source offset `+0x54`) in `EAX`.
With the normal x86 direction-flag-clear entry state, this reads through
message offset `+0x57` (end-exclusive `+0x58`).

The parser declared a `0x30`-byte object and writes only the six-byte body at
`+0x28..+0x2D`. The receiver's sequential 88-byte read therefore extends
`0x28` bytes beyond the declared allocation. The reason for this bulk read,
the caller's use of the return value, and any runtime consequence are
unresolved; do not infer a 22-DWORD message schema from the receiver alone.

### Selector 0x62: `Stumble`

Entry15 maps selector `0x62` to VA `0xC1DD20` / raw case `0x81D120`. It
allocates `0x30` bytes, stores local DWORD `0x6A` (role unknown), then calls
constructor `0x5258C0`. The constructor passes `0x62` to helper `0x5A0AD0`
and installs vtable VA `0x1C67188` (RTTI
`firefall::Character::Stumble`).

Body reader `0x5F1ED0` reads WORDs into object fields `+0x28` and `+0x2A`,
then reads a byte into `+0x2C`. The observed nominal body length is five
bytes. Field meanings remain unknown; the RTTI name does not establish
stumble-event semantics.

#### Selector 0x62 post-parse consumer

The outer handler switch maps selector `0x62` through VA `0xC1D286` / raw case
`0x81C686`, then invokes slot `+0x10C` at raw `0x81C6A5`. The vtable entry at
raw `0x1870BCC` contains VA `0xECC250`, mapped to receiver raw `0xACB650`.
When global pointer `0x1E68354` refers to state with byte `+0x40` set, the
receiver combines the two parser WORDs at message `+0x28` into a DWORD key
and uses it with context map state at `this+0x4C`. It then reads a begin/end
pointer pair at message `+0x30/+0x34`, computes `(end - begin) / 12`, and
copies/processes the resulting 12-byte entries when the count is nonzero.

The parser allocates `0x30` bytes and writes only through `+0x2C`; the
receiver's pointer pair at `+0x30/+0x34` lies eight bytes beyond the declared
object extent. A second path calls `0x60FB60`; when it returns a nonzero
context, the receiver passes the key, the address of the same pointer pair,
and the status byte at `+0x2C` to `0x60FBD0`. That byte is mapped as `0 -> 0`,
`1 -> 1`, and any other value to `2`. The pointer pair's provenance and
runtime validity are unresolved; the static extent discrepancy alone does
not establish runtime impact or the meaning of the event.

### Selector 0x63: `QuickChat`

Entry16 maps selector `0x63` to VA `0xC1DD71` / raw case `0x81D171`. It
allocates `0x30` bytes, stores local DWORD `0x6B` (role unknown), then calls
constructor `0x525940`. The constructor passes `0x63` to helper `0x5A0B40`
and installs vtable VA `0x1C67178` (RTTI
`firefall::Character::QuickChat`).

On the successful allocation path, the case jumps to shared reader
continuation `0x8206E3`, which reads a DWORD into object field `+0x28`
through helper `0x3370A0`; nominal body length is four bytes. The allocation
failure branch at `0x81D189` instead enters `0x8206E1`, zeroes ESI, and
continues into the same field reader, which forms destination address
`0x28`. The runtime consequence of that abnormal path is unresolved.
Field meaning remains unknown; RTTI alone does not establish chat semantics.

#### Selector 0x63 post-parse receiver

The outer handler switch maps selector `0x63` through VA `0xC1D2B1` / raw case
`0x81C6B1`, then invokes slot `+0x110` at raw `0x81C6D0`. The vtable entry at
raw `0x1870BD0` contains VA `0xECD7E0`, mapped to receiver raw `0xACCBE0`.
The receiver reads a type-string pointer from message `+0x34` and a text
pointer from `+0x30`; it compares the type string against two static byte
sequences. For either match, it obtains the current chat through `0x6F4540`
and posts a notification through `0x6E3460`, using the text pointer or an
empty-string fallback. If neither comparison matches, it logs values that
include the DWORD at `+0x28`. The comparison bytes and field semantics remain
unresolved.

The parser allocates `0x30` bytes and writes only the DWORD at `+0x28`. The
receiver's pointer fields at `+0x30` and `+0x34` therefore extend eight bytes
past the declared allocation (`+0x34..+0x37` is the furthest pointer read).
This static parser/receiver mismatch does not establish whether the pointers
are valid or what runtime effect follows.

### Selector 0x64: `ProximityTextChat`

Entry17 maps selector `0x64` to VA `0xC1DDA2` / raw case `0x81D1A2`. It
allocates `0x48` bytes, stores local DWORD `0x6C` (role unknown), then calls
constructor `0x5259C0`. The constructor passes `0x64` to helper `0x5A0BB0`,
installs vtable VA `0x1C67158` (RTTI
`firefall::Character::ProximityTextChat`), zeros the DWORD at object `+0x28`,
and clears the optional-blob presence byte at `+0x30`.

Body reader `0x5F1BC0` reads a NUL-terminated shared-text field at `+0x28`
through `0x12EE010`, then bytes at `+0x2C`, `+0x2D`, and `+0x2E`. It calls
`0x5F19A0` with `this = object+0x30`; that helper always consumes a gate
byte. When nonzero, it reads a little-endian WORD length and that many bytes
through `0x12EE170`, then updates the reference-backed blob field using
`0x5F1920`. A zero gate clears an existing blob through `0x49AC30` when its
presence marker is set.

#### Selector 0x64 post-parse receiver

The outer handler switch maps selector `0x64` through VA `0xC1D2DC` / raw
`0x81C6DC`, then calls slot `+0x114` at raw `0x81C6FB`. The protocol-handler
vtable entry at raw `0x1870BD4` contains VA `0xECD900`, mapped to the receiver
at raw `0xACCD00`. It reads a WORD at message `+0x40`; values other than `2`
are sent to the unhandled-message logger `0x2FC4F0`. For value `2`, it checks
the transport-context pointer at handler `+0x10`. A null context logs diagnostic
code `91`, but execution then continues to `0x977150` either way, passing
addresses of message fields `+0x28`, `+0x30`, `+0x4C`, and `+0x44`.

The `0x977150` sender consumes the `+0x28` and `+0x30` arguments through
collection helpers and traverses the structure rooted at `+0x44`. The
`+0x4C` address is first passed to `0x975660`, whose decompilation remains
unresolved; its exact meaning and whether that helper dereferences it are
unknown. The parser's allocation is `0x48` bytes, so `message+0x4C` is four
bytes beyond the declared extent. The receiver forms and forwards that address,
but does not directly load from it. This static extent mismatch does not by
itself establish runtime validity or impact.

The nominal input length is the number of text bytes through and including
the terminating NUL, plus three bytes and the gate byte, plus
`2 + blobLength` when the gate is nonzero. Thus an empty text with no blob
consumes five bytes; an empty text with an empty blob consumes seven. The
text reader processes at most `0x100` bytes per chunk using a `0x104`-byte
temporary buffer; no overall text limit or encoding is established. The WORD
blob length permits at most `0xFFFF` payload bytes. Field meanings remain
unknown; RTTI alone does not establish proximity-chat semantics.

### Selector 0x65: `JumpActioned`

Entry18 maps selector `0x65` to VA `0xC1DDF3` / raw case `0x81D1F3`. It
allocates `0x30` bytes, stores local DWORD `0x6D` (role unknown), then calls
constructor `0x525AF0`. The constructor passes `0x65` to helper `0x5A0C20`
and installs vtable VA `0x1C67168` (RTTI
`firefall::Character::JumpActioned`).

On the successful allocation path, the case jumps to shared reader
continuation `0x81D21F`, which reads a WORD into object field `+0x28`
through helper `0x336AC0`; nominal body length is two bytes. The allocation
failure branch at `0x81D20B` instead enters `0x81D21D`, zeroes ESI, and
continues into the same field reader, which forms destination address
`0x28`. The runtime consequence of that abnormal path is unresolved.
Field meaning remains unknown; RTTI alone does not establish jump-action
semantics.

#### Selector 0x65 post-parse receiver

The outer handler switch maps selector `0x65` through VA `0xC1D307` / raw
`0x81C707`, then calls slot `+0x118` at raw `0x81C726`. The protocol-handler
vtable entry at raw `0x1870BD8` contains VA `0xECD970`, mapped to the receiver
at raw `0xACCD70`. It checks the handler context at `+0x10`; a null value logs
diagnostic `91`, then execution still calls `0x977750`. The call passes the
address of message `+0x28`, then addresses `+0x30` and `+0x40`.

The `0x977750` helper performs a keyed lookup using the DWORD pair at
message `+0x28/+0x2C`. It treats the addresses at message `+0x30` and `+0x40`
as polymorphic objects: it reads each address's first DWORD as a vtable and
calls virtual slot `+0x0C`. The `+0x40` object's returned range is iterated to
build event `360` records. When the keyed entry has a nonzero-length
associated field, a separate branch can dispatch event `359` using the key.
The `+0x30` object's returned byte sequence is also traversed, including
eight-byte reads and special `0xFF` handling; the exact record meaning remains
unresolved.

The parser requests `0x30` bytes and writes only the WORD at `+0x28`. The
receiver's `+0x30` object starts at the end of that declared allocation, while
its `+0x40` object starts `0x10` bytes beyond it; the helper's virtual calls
read vtable pointers there. These are static parser/receiver extent conflicts.
The requested allocation size does not establish actual runtime capacity or
the cause or effect of the observed accesses.

### Selector 0x66: `JumpRolled`

Entry19 maps selector `0x66` to VA `0xC1DE47` / raw case `0x81D247`. It
allocates `0x30` bytes, stores local DWORD `0x6E` (role unknown), then calls
constructor `0x525B70`. The constructor passes `0x66` to helper `0x5A0C90`
and installs vtable VA `0x1C67148` (RTTI
`firefall::Character::JumpRolled`).

On the successful allocation path, the case jumps to the same reader
continuation `0x81D21F` used by selector `0x65`; it reads a WORD into object
field `+0x28` through helper `0x336AC0`, for a nominal two-byte body.
Selector `0x66` has its own RTTI and constructor, so the shared parser does
not establish semantic equivalence with `JumpActioned`. The allocation
failure branch at `0x81D25F` enters `0x81D21D`, zeroes ESI, and continues
into the same reader, which forms destination address `0x28`; the runtime
consequence remains unresolved. Field meaning is unknown.

#### Selector 0x66 post-parse receiver

The outer handler switch maps selector `0x66` through VA `0xC1D332` / raw
`0x81C732`, then calls slot `+0x11C` at raw `0x81C751`. The protocol-handler
vtable entry at raw `0x1870BDC` contains VA `0xECD9B0`, mapped to the receiver
at raw `0xACCDB0`. It checks handler context `+0x10`; a null value logs
diagnostic `91`, but execution continues and calls `0x977F20` with the address
of message `+0x28`.

The helper looks up the eight-byte key at message `+0x28/+0x2C`. On a hit, it
removes the associated entry, then synchronously drains its `+4` count by
calling `0x977630` for each linked item before finalizing with `0x975020` and
releasing through `0x12C8D60`. The key's interpretation, entry type, and
per-item effects remain unknown. Both key DWORDs are within the parser's
requested `0x30`-byte allocation; the parser itself reads only the WORD at
`+0x28`.

### Selector 0x67: `Respawned`

Entry20 maps selector `0x67` to VA `0xC1DE71` / raw case `0x81D271`. It
allocates `0x30` bytes, stores local DWORD `0x6F` (role unknown), then calls
constructor `0x525BF0`. The constructor passes `0x67` to helper `0x5A0D00`
and installs vtable VA `0x1C67138` (RTTI
`firefall::Character::Respawned`).

On the successful allocation path, the case calls body reader `0x5F1550`.
That reader reads a WORD into object `+0x28` through `0x336AC0`, then bytes
at `+0x2A` and `+0x2B` through `0x356CF0` and `0x337180`; the nominal body
length is four bytes. Allocation failure branches at `0x81D289` to
`0x81D29B`, zeroes ESI, then joins the body-reader setup at `0x81D29D`;
the consequence of that abnormal path remains unresolved. The field meanings
are unknown, and the RTTI name does not establish respawn semantics.

#### Selector 0x67 post-parse receiver

The outer handler switch maps selector `0x67` through VA `0xC1D35D` / raw
`0x81C75D`, then calls slot `+0x120` at raw `0x81C77C`. The protocol-handler
vtable entry at raw `0x1870BE0` contains VA `0xECD9F0`, mapped to receiver raw
`0xACCDF0`.

The receiver compares message `+0x30` and `+0x34` as begin/end pointers and,
when nonempty, computes a count using a `0x10`-byte stride. It processes each
record by the byte at entry `+0`: kind `0` uses the pointer at `+4` as shared
text, kind `1` formats the DWORD at `+8`, and kind `2` formats the float at
`+0xC`; other kinds are skipped. The exact format strings and meanings are
unknown. For an empty range it instead builds shared text from message
`+0x28`. After aggregating, it conditionally appends shared text from `+0x2C`
and a nonzero byte at `+0x8C`, then dispatches event `167`; event semantics are
unresolved.

The parser requests only `0x30` bytes and writes the WORD at `+0x28` and bytes
at `+0x2A/+0x2B`. Thus the receiver's vector begin/end header starts at and
extends beyond the declared allocation, and its optional byte at `+0x8C` is
also beyond it. These are static extent mismatches, not proof of actual runtime
capacity or impact.

### Selector 0x68: `CalledForHelp`

Entry21 maps selector `0x68` to VA `0xC1DEC2` / raw case `0x81D2C2`. It
allocates `0x30` bytes, stores local DWORD `0x70` (role unknown), then calls
constructor `0x525C70`. The constructor passes `0x68` to helper `0x5A0D70`
and installs vtable VA `0x1C67118` (RTTI
`firefall::Character::CalledForHelp`).

On the successful allocation path, the case jumps to reader continuation
`0x81D21F`, shared by selectors `0x65` and `0x66`; it reads a WORD into
object field `+0x28` through helper `0x336AC0`, for a nominal two-byte body.
The allocation-failure branch at `0x81D2DA` enters `0x81D21D`, zeroes ESI,
and continues into the same reader, which forms destination address `0x28`.
The runtime consequence of that abnormal path is unresolved. Field meaning
remains unknown; shared parsing does not establish semantic equivalence with
the other selectors.

#### Selector 0x68 post-parse receiver

The outer handler switch maps selector `0x68` through VA `0xC1D388` / raw
`0x81C788`, then calls slot `+0x124` at raw `0x81C7A7`. The protocol-handler
vtable entry at raw `0x1870BE4` contains VA `0xECE040`, mapped to receiver raw
`0xACD440`. It reads the DWORD at message `+0x28` and looks it up in registry
`0x1E5A830` through `0x327890`. If the lookup has no usable entry, it logs the
key through `0x2FC4F0`. On a hit, it builds event `176` from several converted
registry-entry values and message fields `+0x2C` and `+0x30`; the associated
labels and field meanings remain unknown.

The parser requests `0x30` bytes and writes a WORD at `+0x28` plus bytes
`+0x2A/+0x2B`. The receiver reads a DWORD key at `+0x28` and accesses a DWORD
at `+0x30`; that latter field starts exactly at the declared allocation end,
so its four-byte read is outside the requested object extent. This static
extent mismatch does not establish actual runtime capacity or impact.

### Selector 0x69: `TookHit`

Entry22 maps selector `0x69` to VA `0xC1DEF3` / raw case `0x81D2F3`. It
allocates `0x60` bytes, stores local DWORD `0x71` (role unknown), then calls
constructor `0x525CF0`. The constructor passes `0x69` to helper `0x5A0DE0`,
installs vtable VA `0x1C67128` (RTTI
`firefall::Character::TookHit`), clears byte `+0x28`, and clears WORD
`+0x50`.

The case calls body reader `0x5F0FC0`. Its first sub-reader `0x5F0CF0`
consumes a gate byte for the record at `+0x28`; when set, it reads a QWORD,
then `0x5FDA00` consumes another gate byte and an optional QWORD, followed by
a DWORD and byte. The temporary values are routed through `0x5F0B20` or
`0x5F0C30`, which copy QWORDs at `+0x30` and optionally `+0x40`, a DWORD at
`+0x48`, and a byte at `+0x4C`; the higher-level meaning of these state
helpers remains unresolved. The body reader then reads bytes at `+0x50` and
`+0x51`, a WORD at `+0x58` via `0x336AC0`, and a byte at `+0x5A`.
With the outer gate clear the body consumes six bytes; when set, its length
depends on the nested gate. Field meanings remain unknown.

#### Selector 0x69 post-parse receiver

The outer handler switch maps selector `0x69` through VA `0xC1D3B3` / raw
`0x81C7B3`, then calls slot `+0x128` at raw `0x81C7D2`. The protocol-handler
vtable entry at raw `0x1870BE8` contains VA `0xECE210`, mapped to receiver raw
`0xACD610`. The receiver copies `0x40` bytes from message `+0x28` using
`0x37F810`; that helper performs four 16-byte reads and writes, including the
block at source `+0x30`.

The receiver then adds the global float at VA `0x1C81CA8` to the copied float
whose message source is `+0x60`. It obtains an owner through `0x49DDD0`; when
the owner's byte at `+0x18C` is set and its pointer at `+0x14C8` is nonzero,
the receiver sets byte `+0x108` on that pointed-to object and copies the
modified `0x40`-byte block to its `+0xC8` field. The meanings of this copied
record, the global adjustment, and the state fields are unknown.

The parser requests `0x60` bytes. The receiver's copy from `+0x28` spans
through `+0x67`, so its final eight source bytes lie beyond the declared
allocation; the adjusted float at `+0x60` is itself wholly outside it. This
is a static extent mismatch and does not establish actual runtime capacity or
impact.

### Selector 0x6A: `AlmostHit`

Entry23 maps selector `0x6A` to VA `0xC1DF44` / raw case `0x81D344`. It
allocates `0x30` bytes, stores local DWORD `0x72` (role unknown), then calls
constructor `0x525DD0`. The constructor passes `0x6A` to helper `0x5A0E50`,
installs vtable VA `0x1C67108` (RTTI
`firefall::Character::AlmostHit`), and clears the QWORD at `+0x28`.

The successful path joins the shared continuation at `0x81F72E`, which forms
the destination `object+0x28` and calls `0x356F80` to read one QWORD (nominal
body length eight bytes). Allocation failure branches at `0x81D35C` to
`0x81F72C`, zeroes ESI, then joins the same reader setup, which forms address
`0x28`; the consequence of that abnormal path is unresolved. The field
meaning remains unknown; the RTTI name does not prove message semantics.

#### Selector 0x6A post-parse receiver

The outer handler switch maps selector `0x6A` through VA `0xC1D3DE` / raw
`0x81C7DE`, then calls slot `+0x12C` at raw `0x81C7FD`. The protocol-handler
vtable entry at raw `0x1870BEC` contains VA `0xECEAD0`, mapped to receiver raw
`0xACDED0`.

The receiver first requires the global object at VA `0x1DFFFE8` to have a
nonzero pointer at `+0x28`. It treats the QWORD at message `+0x30` as a lookup
key, rejecting zero and values from `0xFFFFFFFFFFFFFF00` through
`0xFFFFFFFFFFFFFFFF`. For other values, it clears the low byte of the key's
low DWORD and searches the table obtained through `0x35E700` using `0x4093E0`.
If the found entry has a nonzero pointer at `+0x10`, it calls that object's
virtual slot `+0x70`; the return value is then passed to `0x369080` along with
the QWORD at message `+0x28` and constant `12`. That helper branches on the
signed QWORD and routes through distinct manager-side helpers; their meanings
remain unknown.

The parser requests `0x30` bytes and reads a QWORD at `+0x28`. The receiver's
lookup key is a QWORD at `+0x30`, entirely beyond the requested extent. This
is a static extent mismatch and does not establish actual runtime capacity or
impact.

### Selector 0x6B: `DealtHit`

Entry24 maps selector `0x6B` to VA `0xC1DF75` / raw case `0x81D375`. It
allocates `0x58` bytes, stores local DWORD `0x73` (role unknown), then calls
constructor `0x525E60`. The constructor passes `0x6B` to helper `0x5A0EC0`,
installs vtable VA `0x1C670F8` (RTTI
`firefall::Character::DealtHit`), clears byte `+0x28`, and clears WORD
`+0x50`.

The successful path joins the reader setup at `0x81D3A1` and calls
`0x5F0800`. Its first sub-reader `0x5F0CF0` consumes a gate byte for the
record at `+0x28`; when set it reads a QWORD, a nested gate byte and optional
QWORD, then a DWORD and byte. The temporary values are routed through
`0x5F0B20` or `0x5F0C30` into offsets `+0x30`, `+0x38`, `+0x40`, `+0x48`,
and `+0x4C`. The reader then consumes bytes at `+0x50` and `+0x51`. With the
outer gate clear, the nominal minimum body length is three bytes; a set gate
adds the record fields. Allocation failure branches at `0x81D38D` to
`0x81D39F`, zeroes ESI, and continues into the same reader; its abnormal
runtime consequence is unresolved. Field meanings and message semantics
remain unknown.

#### Selector 0x6B post-parse receiver

The outer handler switch maps selector `0x6B` through VA `0xC1D409` / raw
`0x81C809`, then calls slot `+0x130` at raw `0x81C828`. The protocol-handler
vtable entry at raw `0x1870BF0` contains VA `0xECEB70`, mapped to receiver raw
`0xACDF70`. That wrapper calls `0x368EA0` with the address of message `+0x30`,
the QWORD at `+0x28`, constant `12`, and duration value `0x3C`.

The shared helper treats the bytes at `+0x30/+0x34/+0x38` as three floats,
updates position-related state when manager byte `+0xE1` is set, and can
compute their Euclidean distance from the global vector at VAs
`0x1E62C44/0x1E62C48/0x1E62C4C` before a conditional callback through
`0x100AE90`. It also sends the QWORD at `+0x28` through `0x369080` and
registers a follow-up timer for `0x3C * 1000` milliseconds. The meanings of
the QWORD, callback, and timer action remain unresolved.

The parser's conditional record uses byte `+0x38` as a nested gate, while the
receiver's helper reads `+0x38..+0x3B` as its third float. This is an observed
overlay between parser and consumer views; the relationship between those
representations is unresolved. All these reads remain within the parser's
requested `0x58`-byte allocation.

### Selector 0x6C: `Killed`

Entry25 maps selector `0x6C` to VA `0xC1DFC6` / raw case `0x81D3C6`. It
allocates `0x40` bytes, stores local DWORD `0x74` (role unknown), then calls
constructor `0x525F20`. The constructor passes `0x6C` to helper `0x5A0F30`,
installs vtable VA `0x1C670E8` (RTTI
`firefall::Character::Killed`), and clears the QWORD at `+0x30`.

The success path joins reader setup at `0x81D3F2` and calls `0x5F0560`. In
reader order it reads a WORD into `+0x28`, a QWORD into `+0x30`, then bytes
into `+0x38`, `+0x39`, and `+0x3A` (13 nominal input bytes). Allocation
failure branches to `0x81D3F0`, zeroes ESI, and reaches the same reader setup;
the abnormal-path consequence is unresolved. The RTTI label does not prove
message semantics, and all field meanings remain unknown.

#### Selector 0x6C post-parse receiver

The outer handler switch maps selector `0x6C` through VA `0xC1D434` / raw
`0x81C834`, then calls slot `+0x134` at raw `0x81C853`. The protocol-handler
vtable entry at raw `0x1870BF4` contains VA `0xECEBA0`, mapped to receiver raw
`0xACDFA0`. This wrapper passes global `0x1DFFFFC`, the DWORD at message
`+0x28`, constant `12`, zero, and the address of message `+0x2C` to helper
`0x466790`.

The helper looks up an object using the DWORD through `0x421640`; depending on
the returned object's fields and a pair of DWORDs beginning at message
`+0x2C`, it either updates or searches manager-owned contexts. Several paths
allocate a `0xB0`-byte context and call update routine `0x465A10`; other paths
log or return without that update. The object's role, message fields, and
context semantics are unresolved.

The parser requests `0x40` bytes. The receiver's `+0x28` DWORD and the
helper's access to the pair at `+0x2C/+0x30` remain within that extent; no
allocation-boundary mismatch is apparent in this path.

### Selector 0x6D: `WarnLockTargeted`

Entry26 maps selector `0x6D` to VA `0xC1E017` / raw case `0x81D417`. It
allocates `0x28` bytes, stores local DWORD `0x75` (role unknown), then calls
constructor `0x525FB0`. The constructor passes `0x6D` to helper `0x5A0FA0`
and installs vtable VA `0x1C670C8` (RTTI
`firefall::Character::WarnLockTargeted`). This case path has no explicit
body-reader call. At raw `0x81D435` it loads the two DWORDs at `[esi+4]` and
`[esi]`, passes them to constructor `0x525FB0` at `0x81D43C`, then reaches the
common return epilogue at `0x81D441` without another parser/read call.
Allocation failure branches to `0x8217D9`, the switch's zero-return epilogue.
The common decoder `0x821BA0` has already read a raw QWORD and compact
positive-UInt16 selector before calling `0x81CC50`. When the QWORD's first byte
selects the secondary table, the compact selector is passed as the parser's
selector argument; the `0x6D` case uses the two DWORDs at the QWORD address as
constructor arguments. The base initializer stores those words at object
`+0x18/+0x1C`, zeros `+0x20/+0x24`, and finishes at the requested allocation
end `+0x28`. It invokes no additional selector-specific body reader. This
narrows the no-reader observation to bytes beyond the shared QWORD/selector
preamble; whether such trailing bytes are valid or accounted for elsewhere
remains unresolved, as do all field meanings.

#### Selector 0x6D post-parse receiver

The outer handler switch maps selector `0x6D` through VA `0xC1D45F` / raw
`0x81C85F`, then calls slot `+0x138` at raw `0x81C87E`. The protocol-handler
vtable entry at raw `0x1870BF8` contains VA `0xECEBC0`, mapped to receiver raw
`0xACDFC0`.

The receiver calls `0x465D40` only if the global object at VA `0x1DFFFFC`
contains a nonzero manager pointer. It forwards the DWORD at message `+0x28`,
constant `12`, the manager pointer, and a normalized Boolean from byte
`+0x38`. The helper maintains manager-owned entries keyed by the DWORD,
removes matching entries, and can update manager state; when its Boolean and
stored-key conditions hold it may also query a selected object and dispatch
additional state. The behavior of that state and the message's semantics
remain unknown.

The parser allocates `0x28` bytes and has no explicit body-reader call in this
case. The receiver nevertheless reads a DWORD at `+0x28` (four bytes beyond
the allocation) and a Boolean byte at `+0x38` (17 bytes beyond its end). These
are static extent mismatches, not proof of actual runtime capacity or impact.

### Selector 0x6E: `CurrentPoseUpdate`

Entry27 maps selector `0x6E` to VA `0xC1E054` / raw case `0x81D454`. It
allocates `0x58` bytes, stores local DWORD `0x76` (role unknown), and calls
constructor `0x526030`. The constructor passes `0x6E` to helper `0x5A1010`
and installs vtable VA `0x1C670D8` (RTTI
`firefall::Character::CurrentPoseUpdate`).

Reader setup at `0x81D480` calls `0x5EFE30`. The reader uses the raw control
byte at `+0x28`: bit `0x01` selects a byte at `+0x29` instead of a WORD at
`+0x2A`; it always reads a byte at `+0x2C`; when bit `0x02` is clear it reads
a WORD at `+0x2E`. If `(control & 0x0C) == 0`, it reads DWORDs at
`+0x34/+0x38/+0x3C`; otherwise it copies three bytes into the field starting
at `+0x30`. The `control & 0x30` cases select four WORDs at `+0x44..+0x4A`,
one byte at `+0x40`, three bytes at `+0x41..+0x43`, or no mid-group read
when the mask is `0x30`. Finally, `control & 0xC0` selects three WORDs at
`+0x50/+0x52/+0x54` when clear, a three-byte copy at `+0x4C` for `0x40` or
`0x80`, or no tail-group read for `0xC0`. Allocation failure zeroes ESI and
uses the same reader setup. These are observed branch masks and field widths;
their meanings and message semantics remain unknown.

#### Selector 0x6E post-parse receiver

The outer handler switch maps selector `0x6E` through VA `0xC1D48A` / raw
`0x81C88A`, then calls slot `+0x13C` at raw `0x81C8A9`. The protocol-handler
vtable entry at raw `0x1870BFC` contains VA `0xECE2A0`, mapped to receiver raw
`0xACD6A0`.

The receiver creates an event payload, formats message fields `+0x28/+0x2C/
+0x30` as three floats using distinct label strings, and optionally looks up
the DWORD at `+0x34` in registry `0x1E5A4F0`. A usable entry is converted to
text through `0x13122D0` and appended. It dispatches event `239` through the
object at global `0x1E002B8`, vtable slot `+0xAC`, then cleans up the payload.
The relationship between this receiver's float/key view and the parser's
control-byte and conditional-field layout is unresolved.

The parser requests `0x58` bytes; all receiver field accesses described above
are within that requested extent. The event's meaning and the fields' semantic
roles remain unknown.

### Selector 0x6F: `ConfirmedPoseUpdate`

Entry28 maps selector `0x6F` to VA `0xC1E0A6` / raw case `0x81D4A6`. It
allocates `0xC0` bytes, stores local DWORD `0x77` (role unknown), and calls
constructor `0x5260B0`. The constructor passes `0x6F` to helper `0x5A1090`,
installs vtable VA `0x1C670B8` (RTTI
`firefall::Character::ConfirmedPoseUpdate`), initializes WORD `+0x88` to
`0x0101`, and clears DWORD `+0xB0`.

Reader setup at `0x81D4D5` calls `0x5EFAB0`, which reads a record through
`0x5EF8D0` at `object+0x28` and then a WORD at `+0xB8`. The record reader
first reads a WORD at `+0xB4` and gate bytes at `+0xA6/+0xA7`. If bit `0x01`
of `+0xA6` is set, it reads DWORDs at `+0x2C/+0x30/+0x34`, a four-DWORD
block copied to `+0x78..+0x87`, and a WORD at `+0xA4`. It always reads
DWORDs at `+0x98/+0x9C/+0xA0`; bit `0x02` of `+0xA6` adds DWORDs at
`+0x8C/+0x90/+0x94`. It then reads WORDs at `+0xA8/+0xAA/+0xAC` and
processes the field at `+0xB0` through byte-gated nested reader `0x5FFB00`.
Allocation failure zeroes ESI before the same reader setup. Nested contents, field
meanings, and message semantics remain unresolved.

#### Selector 0x6F post-parse receiver

The outer handler switch maps selector `0x6F` through VA `0xC1D4B5` / raw
`0x81C8B5`, then calls slot `+0x140` at raw `0x81C8D4`. The protocol-handler
vtable entry at raw `0x1870C00` contains VA `0xECE400`, mapped to receiver raw
`0xACD800`.

The receiver creates an event payload and appends a typed value of kind `5`
using message DWORDs at `+0x28/+0x2C`. If the DWORD at `+0x30` is nonzero, it
looks it up in registry `0x1E5A4F0`; a usable entry is converted to text by
`0x13122D0` and appended. It dispatches event `239` through the object at
global `0x1E002B8`, vtable slot `+0xAC`, then releases the payload.

The parser requests `0xC0` bytes; all receiver reads described here are within
that allocation. The relationship between the receiver's three DWORD view
and the parser's conditional record layout, plus the event's meaning, remains
unresolved.

### Selector 0x70: `PublicDebugMovementUpdate`

Entry29 maps selector `0x70` to VA `0xC1E0FA` / raw case `0x81D4FA`. The case
allocates `0x5A0` bytes, stores local DWORD `0x78` (role unknown), and calls
constructor `0x526180`. The constructor passes `0x70` to helper `0x5A1100`,
installs vtable VA `0x1C670A8` (RTTI
`firefall::Character::PublicDebugMovementUpdate`), then runs initializer
`0x60E6A0`.

The allocation is exactly a `0x28`-byte object prefix followed by seven
`0xC8`-byte records. In each record, the reader consumes a WORD at `+0x00`;
uses the record at `+0x04` for three DWORDs at record `+0x04/+0x08/+0x0C`,
a 16-byte block at `+0x50`, a WORD set to 1 at `+0x60`, and three DWORDs at
`+0x64/+0x68/+0x6C`; consumes another WORD at `+0x70`; and reads a gate byte
at `+0x74` before a conditional helper-managed nested value. The constructor
initializes each record's WORD `+0x60` to `0x0101` and gate byte `+0x74` to 0;
the reader overwrites the WORD with 1. Record starts are object offsets
`+0x28 + i*0xC8`, for `i=0..6`. The gate's variable payload and field meanings
remain unresolved. Allocation failure zeroes ESI before entering the shared
reader setup; the consequences of that path are not inferred.

The route-0 receiver dispatches through slot `+0x144` (table VA
`0xC1D6DC`, raw table `0x81CADC`) to VA `0xECEBF0` / raw function
`0xACDFF0`. Its switch case is VA `0xC1D4E0` / raw `0x81C8E0`, with the
indirect call through slot `+0x144` at raw `0x81C8FF`. The receiver appends a
kind-5 typed value from the DWORDs at message `+0x28/+0x2C` through helper
`0xC245A0`. It then treats the DWORD at message `+0x30` as a nested object's
vtable pointer, calls slot `+0x0C`, and, for a nonzero result, calls slot
`+0x18` with a pointer to `unk_1B53F77` and value 1 before passing that object
to helper `0xC24780`. Finally it dispatches event 363 through
`uiManager` global VA `0x1E002B8`, slot `+0xAC`. The message `+0x30` location
falls inside the parser's first record; the relationship between that record
and the receiver's object view is not established, nor are the nested object's
semantics or event payload meaning.

### Selector 0x71: `ForcedMovement`

Entry30 maps selector `0x71` to VA `0xC1E14F` / raw case `0x81D54F`. It
allocates `0x1E8` bytes and writes stack local `[esp+0x20]=0x79` at raw
`0x81D560` (role unknown; not the message discriminator), then calls
constructor `0x526230`. The constructor passes `0x71` to helper `0x5A1170`,
installs vtable VA `0x1C67098` (RTTI
`firefall::Character::ForcedMovement`), and initializes the nested state at
object `+0x28` through `0x92B8A0`.

The initialized nested state spans `+0x28..+0x1DF`. Constructor writes are:
zero DWORDs at `+0x28/+0x2C/+0x30`, `+0x114/+0x118`, `+0x138/+0x13C`,
and `+0x1B8/+0x1BC/+0x1C0/+0x1C4`; WORDs `+0x34=0`, `+0x94=0x0101`,
`+0x100=0x0101`, `+0x110=0`, `+0x19C=0x0101`, and `+0x1CC=0`.
Reader `0x5EF690` invokes `0x92F800` on the nested state, then reads a
separate WORD at object `+0x1E0`. The nested reader reads a DWORD at `+0x2C`,
a discriminator byte at `+0x30`, a conditional pair at `+0x138/+0x13C`,
and dispatches 14 discriminator cases. The case-specific tail layouts and
field meanings remain unresolved. Allocation failure zeroes ESI before the
reader setup.

The route-0 receiver dispatches through slot `+0x148` (table VA `0xC1D6DC`,
raw table `0x81CADC`) to VA `0xECED30` / raw function `0xACE130`. Its switch
case is VA `0xC1D50B` / raw `0x81C90B`, with the indirect call through slot
`+0x148` at raw `0x81C92A`. The receiver passes a pointer to the nested state
at message `+0x28` to helper `0xC24780` with label `byte_1BAEDDC` and value 1,
then dispatches event 365 through `uiManager` global VA `0x1E002B8`, slot
`+0xAC`. The receiver does not directly inspect individual state fields;
interpretation of the state and event remains unresolved.

### Selector 0x72: `ForcedMovementCancelled`

Entry31 maps selector `0x72` to VA `0xC1E1A3` / raw case `0x81D5A3`. It
allocates `0x30` bytes, stores local DWORD `0x7A` (role unknown), and calls
constructor `0x5262C0`. The constructor passes `0x72` to helper `0x5A11E0`
and installs vtable VA `0x1C67078` (RTTI
`firefall::Character::ForcedMovementCancelled`). Reader `0x5EF490` reads a
DWORD at object `+0x28` with `0x3370A0`, then a WORD at `+0x2C` with
`0x336AC0`. The derived constructor does not initialize those two fields.
Allocation failure zeroes ESI before the reader setup. Field meanings remain
unknown.

### Selector 0x73: `GrappleClimbPermission`

Entry32 maps selector `0x73` to VA `0xC1E1F4` / raw case `0x81D5F4`. It
allocates `0x1E0` bytes, stores local DWORD `0x7B` (role unknown), and calls
constructor `0x526340`. The constructor passes `0x73` to helper `0x5A1250`,
installs vtable VA `0x1C67088` (RTTI
`firefall::Character::GrappleClimbPermission`), and initializes a `0x1B8`-byte
nested state at object `+0x28` through `0x92B8A0`. Its explicit initialized
DWORD and WORD offsets/values match selector `0x71`; the state reaches the end
of this allocation, with no trailing WORD.

The body reader is `0x92F800`, shared with selector `0x71`: it reads the
DWORD at nested `+0x04`, discriminator byte at `+0x08`, and conditional pair
at `+0x110/+0x114`, then dispatches 14 discriminator cases. Those nested
offsets correspond to object `+0x2C`, `+0x30`, and `+0x138/+0x13C`.
Case-specific tail layouts and field meanings remain unresolved. Allocation
failure zeroes ESI before the reader setup.

### Conditional receiver application for Character selectors 0x70–0x73

When callback routing selects the protocol handler, dispatcher raw `0x81BB90`
subtracts `0x20` from the message discriminator and indexes its selector table
at VA `0xC1D6DC` / raw
`0x81CADC`. For selectors `0x70`–`0x73`, the table targets dispatcher cases
at VA/raw pairs `0xC1D4E0/0x81C8E0`, `0xC1D50B/0x81C90B`,
`0xC1D536/0x81C936`, and `0xC1D561/0x81C961`. Each case calls
`tfGameProtocolHandler` at `4 + 4*(selector - 0x20)`, respectively slots
`+0x144`, `+0x148`, `+0x14C`, and `+0x150`. The vtable is VA `0x1C71EC0`,
mapped through the PE `.rdata` section to file offset `0x1870AC0`; the slot
entries resolve to handler VAs `0xECEBF0`, `0xECED30`, `0xECB670`, and
`0xECB6C0` (raw `.text` offsets `0xACDFF0`, `0xACE130`, `0xACAA70`, and
`0xACAAC0`, using VA = raw + `0x400C00`). RTTI is
`.?AVtfGameProtocolHandler@@`. These are receiver methods reached from the
dispatcher; selector RTTI alone is not used to infer their behavior.

| Selector | Receiver slot / raw handler | Observed application path |
|---|---|---|
| `0x70` | `+0x144` / `0xACDFF0` | Passes message DWORD reads at `+0x28/+0x2C` to `0xC245A0`. It then treats the DWORD at `+0x30` as a virtual object, probes slot `+0x0C`, and conditionally invokes slot `+0x18` and helper `0xC24780`. Dispatches numeric application/UI code `363` through global `0x1E002B8`, vtable slot `+0xAC`. |
| `0x71` | `+0x148` / `0xACE130` | Passes the address of message `+0x28` to `0xC24780`, whose first operation calls the vtable at the first DWORD's slot `+0x0C`; if active, it copies the following state. Dispatches numeric application/UI code `365` through the same manager slot. |
| `0x72` | `+0x14C` / `0xACAA70` | Gets an owner-state pointer at singleton `0x49DCC0` offset `+0x43C8` (diagnostic `1651` if null), then passes the address of message `+0x28` to list walker `0x9090A0`. It visits nodes, passes each node's DWORDs at `+0x10/+0x14` to updater `0x909120`, and has no numeric event dispatch in this receiver method. |
| `0x73` | `+0x150` / `0xACAAC0` | Gets an owner-state pointer at singleton `0x49DCC0` offset `+0x43B8` (diagnostic `1646` if null), then calls `0x8EDD10` with the DWORD at message `+0x28` and the address of `+0x2C`. That helper updates its `+0x28/+0x2C` state, merges list entries against existing 20-byte records, and dispatches numeric application/UI code `213` through manager slot `+0xAC`. Missing records may be materialized as 84-byte records from lookups; their semantics remain unknown. |

These receiver paths have verified producer-consumer layout discrepancies; no
conversion or repair step was found in the traced dispatch path:

* Selector `0x70` parses seven `0xC8`-stride records beginning at `+0x28`.
  The receiver consumes only the first record's `+0x28/+0x2C` locations as
  DWORDs (the parser writes a WORD at `+0x28`) and treats the serialized DWORD
  at `+0x30` as a virtual-object vtable pointer. The parser's reader writes
  raw record data there, not a vtable. Its optional gate is byte `+0x74` in
  each record; that gate controls the parser's nested-value reader and is
  distinct from the receiver's virtual-call result test.
* Selector `0x71` constructs a `0x1B8`-byte inline state at `+0x28` and
  initializes its first DWORD to zero. The reader consumes the discriminator
  byte at `+0x30` first, then the DWORD at `+0x2C`, and a 14-case tail. The receiver
  passes the address of `+0x28` to a helper that treats the DWORD stored there
  as a vtable pointer. It does not read the trailing WORD at `+0x1E0`.
* Selector `0x72` parses a DWORD at `+0x28` and WORD at `+0x2C`; the receiver
  treats the DWORD at `+0x28` as a pointer to a linked-list sentinel and
  ignores the WORD. No pointer conversion was observed.
* Selector `0x73` likewise initializes `+0x28` to zero and parses its nested
  state beginning with the discriminator byte at `+0x30`, then the DWORD at
  `+0x2C`; the receiver passes the value at
  `+0x28` as an application-state value and the address of `+0x2C` where the
  helper dereferences a linked-list head. No conversion was observed.

The byte-level mappings and call sites are checked against the original
executable. The pointer interpretations and downstream runtime consequences
remain unresolved; this record does not infer a corrected wire format.

The nested table at raw `0x821844` has 40 four-byte entries for selectors
`0x53` through `0x7A` inclusive; entry `0x7A` is the last one and points to
VA `0xC1E3EF` / raw `0x81D7EF`. The decoder's secondary dispatch subtracts
`0x20` and accepts values through `0x5A`, giving an inclusive upper bound of
`0x7A`; values above it take the default path.

### Selector 0x74: `AbilityActivated`

Entry33 maps selector `0x74` to VA `0xC1E249` / raw case `0x81D649`. The case
allocates `0x40` bytes, stores local DWORD `0x7C` (role unknown), and calls
constructor `0x5263D0`. The constructor passes selector `0x74` to helper
`0x5A12C0` and installs vtable VA `0x1C67068` (RTTI
`firefall::Character::AbilityActivated`). It initializes a 16-byte state at
object `+0x30`: DWORD `0x1C80CC4` followed by three zero DWORDs.

Reader `0x5EF1F0` reads DWORDs at `+0x28` and `+0x2C` through `0x3370A0`.
It then invokes virtual slot `+0x10` on the state at `+0x30`, passing the
remaining stream length and input pointer. The shared `slBlob` method resizes
the blob to that length without preserving prior contents and copies the
remaining bytes verbatim. Their semantic interpretation remains unknown. On
allocation failure, ESI is zeroed before the reader setup; runtime consequences
are not inferred.

### Selector 0x75: `AbilityFailed`

Entry34 maps selector `0x75` to VA `0xC1E29A` / raw case `0x81D69A`. It
allocates `0x40` bytes, stores local DWORD `0x7D` (role unknown), and calls
constructor `0x5264C0`. The constructor passes selector `0x75` to helper
`0x5A1330` and installs vtable VA `0x1C67058` (RTTI
`firefall::Character::AbilityFailed`). Its initialized 16-byte state at
`+0x30` matches selector `0x74`: DWORD `0x1C80CC4` followed by three zero
DWORDs.

Reader `0x5EF090` has the same observed field-access pattern as `0x5EF1F0`:
DWORDs at `+0x28` and `+0x2C` through `0x3370A0`, then a virtual slot `+0x10`
call on `+0x30` using the remaining stream length. That slot resolves to the
shared `slBlob` reader, which resizes and copies the remaining bytes verbatim.
Shared layout and reader shape do not establish matching event semantics. The
blob's semantic meaning and allocation-failure consequences remain unresolved.

### Selector 0x76: `AbilityCooldowns`

Entry35 maps selector `0x76` to VA `0xC1E2EB` / raw case `0x81D6EB`. It
allocates `0x38` bytes, stores local DWORD `0x7E` (role unknown), and calls
constructor `0x5265B0`. The constructor passes selector `0x76` to helper
`0x5A13C0`, installs vtable VA `0x1C67038` (RTTI
`firefall::Character::AbilityCooldowns`), and initializes a 16-byte state at
object `+0x28` with DWORD `0x1C80CC4` followed by three zero DWORDs.

Reader `0x5EEE60` passes the `slBlob` state at `+0x28` to its virtual slot
`+0x10` with the remaining stream length and input pointer. The slot resizes
and copies the remaining bytes verbatim, after which the reader advances past
those bytes. In the shared `slBlob` reader at raw `0x12CB9D0`, the first DWORD
of the blob object is used as its vtable and the second DWORD (`+4`) is loaded
as a storage-object pointer before copying from the storage object's `+4`
data field. The `AbilityCooldowns` receiver instead computes
`(message[+0x2C] - message[+0x28]) >> 6` and walks records beginning at
`message+0x28`; those positions are respectively the initialized `slBlob`
vtable and its storage-object pointer in the parser's layout. The parsed
pointer reaches the receiver unchanged on the traced callback route, and no
intervening conversion is established. This is a static layout/consumer
mismatch; runtime consequences are not established. The blob contents'
semantics and allocation failure consequences remain unresolved.

### Selector 0x77: `NPCInteraction`

Entry36 maps selector `0x77` to VA `0xC1E33C` / raw case `0x81D73C`. It
requests `0x30` bytes and writes stack local `[esp+0x20]=0x7F` at raw
`0x81D74A` (role unknown; not the message discriminator), then calls
constructor `0x5266A0` at raw `0x81D761`. The constructor passes selector
`0x77` to base helper `0x5A1430`, installs vtable VA `0x1C67048` (RTTI
`firefall::Character::NPCInteraction`), and zero-initializes DWORDs at
`+0x28` and `+0x2C`. The base helper's last object stores are at `+0x20` and
`+0x24`; together, base plus derived construction writes through exclusive
`+0x30`. This describes observed writes, not allocator capacity.

The success path jumps to shared reader setup at raw `0x81F72E`, which reads
one raw QWORD into `+0x28` via `0x356F80`; it copies eight bytes and advances
the input stream by eight, ending at exclusive `+0x30`. The parser carries
the constructor's EAX pointer in ESI (`0x81D766`), passes `ESI+0x28` to the
reader, then restores EAX from ESI at `0x81F741`; no conversion or write past
the QWORD is visible. The object's vtable writer (`0x5EED50`) and virtual
reader (`0x5EECE0`) likewise serialize/deserialize only the raw QWORD at
`+0x28`; neither populates `+0x30`. The allocation-failure branch reaches
`0x81F72C`, zeroes ESI, and joins the same setup. The resulting failure-path
address calculation is recorded as observed; its runtime consequence and the
QWORD's field meanings remain unknown.

### Conditional `tfGameProtocolHandler` application for selectors 0x74–0x77

For messages that take the `0x81BB90` callback branch, the recipient is the
concrete `tfGameProtocolHandler` constructed at raw
`0xAC8CA0`; its constructor writes vtable VA `0x1C71EC0` at raw `.rdata`
`0x1870AC0`. The RTTI-backed table bytes and all four selected method pointers
were checked in the requested original. `.text` code addresses map to raw file
offsets by subtracting `0x400C00`; the receiver vtable resides in `.rdata` and
is translated by the parsed PE section table.

Dispatcher `0x81BB90` subtracts `0x20` and bounds the discriminator at `0x7A`.
Its jump table at VA `0xC1D6DC` / raw `0x81CADC` reaches these methods and
issues the recorded virtual calls:

| Selector | Switch target VA / raw | Receiver slot | Method VA / raw |
| --- | --- | --- | --- |
| `0x74` | `0xC1D58C` / `0x81C98C` | `+0x154` | `0xECE940` / `0xACDD40` |
| `0x75` | `0xC1D5B7` / `0x81C9B7` | `+0x158` | `0xECE9E0` / `0xACDDE0` |
| `0x76` | `0xC1D5E2` / `0x81C9E2` | `+0x15C` | `0xECEE10` / `0xACE210` |
| `0x77` | `0xC1D60D` / `0x81CA0D` | `+0x160` | `0xECEEA0` / `0xACE2A0` |

The slots agree with `4 + 4*(selector - 0x20)` and with the neighbouring
dialog slots already verified at `0x78`–`0x7A`.

* **`0x74 AbilityActivated` → `0xACDD40`.** The method calls accessor
  `0x49DCC0`, then uses owner field `+0x43C8`; a missing field reaches an
  assertion call (code `0x673`) but is not followed by a null-return branch.
  It hashes/compares the key at message `+0x28` in the map at owner-state
  `+0x2C`, inserting a record when absent. The first two DWORDs at message
  `+0x30/+0x34` are copied to the selected record `+0x18/+0x1C`; these remain
  opaque state words. Helper `0x909B40` builds a tag-`1` application payload
  using the record value and calls the global `uiManager` (`0x1E002B8`)
  dispatch slot `+0xAC` with numeric code `230` and null dispatch arguments.
  No event name is assigned to `230`.
* **`0x75 AbilityFailed` → `0xACDDE0`.** It uses the same accessor and asserted
  owner field `+0x43C8`, then passes the *addresses* of message fields `+0x28`
  and `+0x30` to `0x909C60`. That helper resets/initializes a per-owner tree,
  passes `**(+0x28)` and `*(+0x28)` as begin/end to `0x90C580`. The latter
  pointer is also used as the tree-header sentinel; each source node's pair at
  `+0x10/+0x14` is copied by `0x82C2E0` and inserted into the fresh owner tree
  through `0x8410B0`. Separately, the helper dereferences message `+0x30` and
  treats that value as a tree-header pointer: it loads the header's first
  pointer and compares it with the header sentinel before traversing nodes.
  This conflicts with the parser, which initializes message `+0x30` as an
  embedded `slBlob` whose first DWORD is vtable VA `0x1C80CC4`; no intervening
  conversion is established. Source/destination field meanings and runtime
  consequence remain unknown. No `uiManager` event dispatch was observed in
  this helper.
* **`0x76 AbilityCooldowns` → `0xACE210`.** The receiver treats state
  `+0x28/+0x2C` as range endpoints, computes `(end-begin)/0x40`, and walks
  `0x40`-byte records. It forwards each record's QWORD `+0`, nullable strings
  `+0x8/+0xC` (empty-string fallback), bytes `+0x10/+0x11/+0x12`, and address
  `+0x14` to `0x6E2590`. For a zero byte at `+0x14`, `0x6E2590` selects a
  native channel label from byte `+0x10`, lowercases it, and passes the
  `+0x0C` string as notification text, `+0x08` as author, the QWORD at `+0`
  as author ID, and byte `+0x11` as display flags to `0x6E3460`. That helper
  applies the channel gates documented in the chat-display section, emits
  runtime event `335` through `uiManager`, and writes the chat file log. Its
  byte-keyed payload properties are independently identified there; byte
  `+0x11` contributes the five documented low-byte flags. The `+0x12` byte is
  not used by this zero-byte branch.

  A nonzero byte at `+0x14` instead requires a current chat and calls
  `0x6E3670` with the record, `+0x08` string, byte `+0x10`, byte `+0x12`, and
  the object at `+0x18`. The helper obtains a data pointer and length through
  that object's storage object and virtual `+0x0C` method, then switches on
  byte `+0x12`; the 19 case targets and selected outputs are documented below.
  These traces establish consumer behavior, not record semantics or cooldown
  timing. Separately, the parser/receiver state mismatch above remains
  unresolved.
* **`0x77 NPCInteraction` → `0xACE2A0`.** It returns without dispatch if the
  `0x49DDD0` root accessor is null or root `+0x4D20` is null; otherwise
  `0x49DCC0` supplies the owner state. The QWORD at message `+0x28` is passed
  as two DWORD key components to helper `0x4C9250`, which looks up/inserts a
  record in owner-state map `+0x4338`. The helper swaps `0x20` bytes between
  that record `+0x14` and the address message `+0x30`, stores the opaque
  two-DWORD result of indirect helper `0x1AC48D8` at record `+0x38/+0x3C`, then
  builds a tag-`5` payload containing the key and dispatches numeric code `356`
  through `uiManager` slot `+0xAC`. Neither the QWORD's identity nor code
  `356`'s event name is established.

The nested parser returns its ESI object pointer in EAX; its known direct
callers (`0x821BA0` and `0x821DE0`) return that result, and the registered
callback path queues the returned pointer value. When message `+0x18`'s low
byte is zero, `+0x10` does not match the nested sentinel, and callback-context
`+8` is non-null, the message pointer is not replaced on the receiving path:
callback dispatch loads it from `[EDI]` at raw `0x7FDC6E`, pushes that value
into `0x81BB90` at `0x7FDD4F`, and
`0x81BB90` loads its argument into EDI at `0x81BB95`. The selector-`0x77` case
pushes that same EDI value at `0x81CA2B` before the virtual call through
receiver slot `+0x160` (`0x81CA2C`). The receiver's vtable pointer at VA
`0x1C71EC0` / raw `.rdata` `0x1870AC0` has
`0xECEEA0` in that slot, mapping to handler raw `0xACE2A0`. The handler loads
its message argument from `[EBP+8]`, pushes its DWORDs at `+0x2C` and `+0x28`,
and passes the same message address plus `0x30` to `0x4C9250`.

**Conditional extent caveat (unresolved):** if selector `0x77` takes the
`0x81BB90` callback branch, it requests `0x30` bytes and its
constructor plus parser reader write only through exclusive `+0x30`. The
receiver passes the one-past-requested pointer `message+0x30` to
`0x4C9250`; at `0x4C927E` it loads that first argument into EBX and at
`0x4C92B9` pushes the same EBX while setting ECX to map-record `+0x14`, then
calls `0x51C9D0`. That function swaps `0x20` bytes with map-record state,
covering message offsets `[+0x30,+0x50)`. Those bytes are not populated by
either the selector body reader or the class's virtual stream reader. A
direct-call scan of the requested original's `.text` finds one constructor
call (raw `0x81D761`), one call to `0x4C9250` (raw `0xACE2CC`), and one call to
`0x51C9D0` (raw `0x4C92BD`); IDA xrefs also connect the latter two only through
this handler/helper pair. The generic `NPCInteraction` writer and reader
touch only `+0x28`. No alternative producer or field-transforming parser path
was found in the analyzed route; this does not exclude an untraced indirect entry.
These facts establish a producer/consumer extent contradiction, not the
runtime allocation extent: the allocator may return slack, but actual block
capacity/ownership and the resulting runtime consequence remain unknown. Do
not equate the `0x30` request with actual block capacity.

### Selector 0x78: `OpenMovieDialog`

Entry37 maps selector `0x78` to VA `0xC1E36D` / raw case `0x81D76D`. It
allocates `0x30` bytes, stores local DWORD `0x80` (role unknown), and calls
constructor `0x526730`. The constructor passes selector `0x78` to helper
`0x5A14A0`, installs vtable VA `0x1C67028` (RTTI
`firefall::Character::OpenMovieDialog`), and zero-initializes DWORD `+0x28`.

The success path joins shared reader setup at raw `0x81FB27`; allocation
failure first zeroes ESI at `0x81FB25` before joining it. The setup passes
`+0x28` to reader `0x12EE010`, which consumes a NUL-terminated byte sequence
into the shared text state. The meaning of the field and the allocation-failure
consequence remain unresolved.

### Selector 0x79: `PrivateDialog`

Entry38 maps selector `0x79` to VA `0xC1E39E` / raw case `0x81D79E`. It
allocates `0x40` bytes and writes stack local `[esp+0x20]=0x81` at raw
`0x81D7AC` (role unknown; not the message discriminator), then calls
constructor `0x526840`. The constructor passes selector `0x79` to helper
`0x5A1510`, installs vtable VA `0x1C67018` (RTTI
`firefall::Character::PrivateDialog`), and zero-initializes DWORDs at `+0x30`
and `+0x34`.

Reader `0x5EE900` reads a DWORD at `+0x28`, a raw QWORD at `+0x30`, and a
DWORD at `+0x38`, using readers `0x3370A0`, `0x356F80`, and `0x3370A0`
respectively; the last read ends at `+0x3C` (exclusive). The allocation-failure
branch zeroes ESI before reaching this reader; its runtime consequence and the
fields' meanings remain unresolved.

The conditional application-receiver route indicates a native extent
disagreement rather than a pointer conversion. The selector case at raw
`0x81D79E` requests
`0x40` bytes from `0x12B7FB0`, constructs `PrivateDialog` with `0x526840`, and
reads the body through `0x5EE900`. The registered parser returns its object
pointer in EAX; the caller at `0x821BA0` returns that pointer unchanged, and
the ReplayReader result queue stores the same pointer. Whether a queued object
takes the protocol-handler branch depends on the envelope-key and nested
sentinel tests described below. On that branch, `0x7FDC60` loads the queued
pointer at `0x7FDC6E`, pushes it at `0x7FDD4E`, and calls `0x81BB90` at
`0x7FDD4F`. The `0x79` case at raw
`0x81CA63` pushes the same parser argument and calls receiver slot `+0x168`
at `0x81CA82`. That `tfGameProtocolHandler` vtable slot contains VA
`0xECEF00` (raw `.text` `0xACE300`), whose first instruction tail-jumps to
`0x991510` (VA `0xD92110`). No message-object copy, field expansion, or
adjustment appears on this route. The discriminator switch assigns distinct
slots with `4 + 4*(id - 0x20)`; an exact-pointer scan of the requested original
found the `0xECEF00` vtable target only once, at raw `.rdata` `0x1870C28`
(slot `+0x168`). There is no evidence this thunk is shared by another
discriminator in that receiver route.

At `0x991510`, the handler reads `success` from `+0x28`, `product_id` from
`+0x30/+0x34`, `price_id` from `+0x38/+0x3C`, `vendor_id` from `+0x40`, and a
four-byte code-string pointer from `+0x44`; the pointed-to C string is external
to the message object. Its fixed fields therefore extend through `+0x48`
(exclusive), eight bytes beyond the parser's requested `0x40`. The parser's
reader ends at `+0x3C`; it does not read the second price DWORD or either
receiver field at `+0x40/+0x44`. The concrete constructor zeroes `+0x30` and
`+0x34`; its base helper writes through `+0x24`. No observed constructor or
reader store initializes `+0x3C`, `+0x40`, or `+0x44`. On a successful
response with a null or empty code string, the handler passes the `+0x44` slot
by reference to its fallback-string helper.

The allocation helper forwards the requested size to backend `0x12C8A20`.
For a `0x40` request, that allocator's small-block calculation uses an
eight-byte header and a rounded `0x48` total chunk. When a selected free block
has less than `0x10` bytes remaining, it keeps the whole block rather than
splitting it; a returned payload can consequently have eight bytes of slack.
The specific free block selected is runtime heap state, so this does not prove
that this message has a `0x48`-byte payload area. Even if it does, the
`+0x3C/+0x40/+0x44` contents are not serialized or initialized by the traced
parser path. The parser/receiver mismatch remains unresolved; no larger
`PrivateDialog` layout is inferred.

### Selector 0x7A: `PublicDialog`

Entry39 maps selector `0x7A` to VA `0xC1E3EF` / raw case `0x81D7EF`. It
allocates `0x30` bytes, stores local DWORD `0x82` (role unknown), and calls
constructor `0x5268D0`. The constructor passes selector `0x7A` to helper
`0x5A1580` and installs vtable VA `0x1C66FF8` (RTTI
`firefall::Character::PublicDialog`).

Reader `0x5EE730` reads DWORDs at `+0x28` and `+0x2C` through `0x3370A0`.
The allocation-failure branch zeroes ESI before reaching this reader; its
runtime consequence and the fields' meanings remain unresolved.

### Message discriminator provenance cross-check

The four examples below trace constructor-written message `+0x10` values and
the conditional `0x81BB90` slot mapping only; they do not establish a global
selector namespace. In each parser case, the seemingly discriminator-like
DWORD is a stack local at `[esp+0x20]`, not a field in the allocated message:
selector `0x71` writes `0x79` at raw `0x81D560`, selector
`0x77` writes `0x7F` at `0x81D74A`, selector `0x79` writes `0x81` at
`0x81D7AC`, and route-3 `Vehicle::AbilityActivated` writes `0xD0` at
`0x81EF91`. Those local values remain semantically unassigned.

The constructors instead place their parser/message selector in the first
stack argument to the base helper, then that helper stores the argument at
message `+0x10`. For `Character::ForcedMovement` (`0x71`), `NPCInteraction`
(`0x77`), `PrivateDialog` (`0x79`), and `Vehicle::AbilityActivated` (`0x53`),
the constructor's stack immediate is respectively `0x71`, `0x77`, `0x79`,
and `0x53`; helper store instructions at `0x5A1191`, `0x5A1451`, `0x5A1531`,
and `0xB474D1` write that first helper argument to `[this+0x10]`. The helper
also writes a class-specific pointer to `+0x0C` (`0x1B5BDBC`, `0x1B5BD84`,
`0x1B5BD7C`, `0x1BB04E0`, respectively), copies the two constructor arguments
to `+0x18/+0x1C`, and zeros `+0x20/+0x24`; these other fields are not the
discriminator. The parser call sites pass source DWORDs `[ESI]` and
`[ESI+4]` as the constructor arguments. The helper temporarily installs its
common vptr `0x1C61B84` at `+0`, then its class-specific base vptr
(`0x1C65AF0`, `0x1C65AD0`, `0x1C65A70`, `0x1C72554`, respectively); each
derived constructor replaces that same primary vptr at `+0`. The four
concrete message vtables have RTTI Complete Object Locator offsets of zero.
For selector `0x71`, the separate state initializer
at `0x92B8A0` begins with a zero DWORD at message `+0x28`, not a secondary
vptr.

The receiver chain preserves the allocated message pointer: callback helper
`0x7FDC60` loads it from `[record]` into EBX at `0x7FDC6E`, loads the receiver
from callback-context `+8` at `0x7FDD47`, pushes the unchanged EBX at
`0x7FDD4E`, and calls `0x81BB90` at `0x7FDD4F`. The dispatcher loads its
message argument into EDI at `0x81BB95`, reads the DWORD at `[EDI+0x10]` at
`0x81BBB7`, subtracts `0x20`, bounds it through `0x7A`, and indexes the
receiver switch table. Each representative case sets ECX to the receiver and
pushes the unchanged EDI message pointer before its virtual call:

| Construction selector / stored discriminator | Parser stack local | Switch target / slot-call raw | Receiver slot | Vtable method VA / raw |
| --- | --- | --- | --- | --- |
| `0x71` Character `ForcedMovement` | `0x79` at `0x81D560` | `0x81C90B` / `0x81C92A` | `+0x148` | `0xECED30` / `0xACE130` |
| `0x77` Character `NPCInteraction` | `0x7F` at `0x81D74A` | `0x81CA0D` / `0x81CA2C` | `+0x160` | `0xECEEA0` / `0xACE2A0` |
| `0x79` Character `PrivateDialog` | `0x81` at `0x81D7AC` | `0x81CA63` / `0x81CA82` | `+0x168` | `0xECEF00` / `0xACE300` |
| `0x53` Vehicle `AbilityActivated` | `0xD0` at `0x81EF91` | `0x81C401` / `0x81C420` | `+0xD0` | `0xECB180` / `0xACA580` |

For messages that actually reach `0x81BB90`, the slot formula is
`4 + 4*(id-0x20)`. In particular, the `0xD0` parser stack local for Vehicle
`AbilityActivated` is not its discriminator; `0xD0` is that selector's
receiver-vtable byte offset, while the message field is `0x53`. The unchanged
message pointer and primary-vtable calls are proven only for this conditional
dispatcher path, not for every parser family.

### Parser-family collisions and callback route gate

The route-specific parser families below all construct different RTTI types
with the same value, `message+0x10 = 0x53`. The route-local stack DWORDs shown
are separate parser temporaries, not namespace IDs. Character route 0 and
Vehicle route 3 use selector tables; AreaVisualData route 2, Deployable route
4, Turret route 5, and LootStoreExtension route 6 branch directly in their
route dispatchers.

| Parser route | Route dispatcher / selector table | Case raw offset | Message class | Allocation | Parser stack local | Constructor → base helper | `+0x0C` metadata pointer | Primary vtable / RTTI |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Character nested route 0 | `0x81CCAD` / VA `0xC22444` | `0x81CCC3` | `Character::MarketRequestComplete` | `0x48` | `0x5B` at `0x81CCD1` | `0x524C30` → `0x5A0400` | `0x1B5BEAC` (`msg-83…`) | `0x1C67278` / `.?AVMarketRequestComplete@Character@firefall@@` |
| AreaVisualData route 2 | `0x81EE63` / direct case | `0x81EF1C` | `AreaVisualData::LootObjectCollected` | `0x40` | `0xCD` at `0x81EF2A` | `0xADBAA0` → `0xB07660` | `0x1BB0224` (`msg-83…`) | `0x1C723C0` / `.?AVLootObjectCollected@AreaVisualData@firefall@@` |
| Vehicle route 3 | `0x81EF6D` / VA `0xC2260C` | `0x81EF83` | `Vehicle::AbilityActivated` | `0x30` | `0xD0` at `0x81EF91` | `0xB093C0` → `0xB474B0` | `0x1BB04E0` (`msg-83…`) | `0x1C72998` / `.?AVAbilityActivated@Vehicle@firefall@@` |
| Deployable route 4 | `0x81F206` / direct case | `0x81F29E` | `Deployable::TookHit` | `0x60` | `0xD9` at `0x81F2AC` | `0xB27E80` → `0xB47C40` | `0x1BB03C4` (`msg-83…`) | `0x1C727A8` / `.?AVTookHit@Deployable@firefall@@` |
| Turret route 5 | `0x81F2EF` / direct case | `0x81F2F9` | `Turret::WeaponProjectileFired` | `0x48` | `0xDC` at `0x81F307` | `0xB36520` → `0xB47D90` | `0x1BB033C` (`msg-83…`) | `0x1C726E8` / `.?AVWeaponProjectileFired@Turret@firefall@@` |
| LootStoreExtension route 6 | `0x81F34A` / direct case | `0x81F354` | `LootStoreExtension::LootObjectCollected` | `0x40` | `0xDD` at `0x81F362` | `0xB43A70` → `0xB48040` | `0x1BB0280` (`msg-83…`) | `0x1C72608` / `.?AVLootObjectCollected@LootStoreExtension@firefall@@` |

The base helpers temporarily install a common vptr and then a class-specific
base vptr at `+0x00`; the derived constructors replace that with each
route-specific primary vptr. The helpers initialize `+0x08` to 1, store the
route-specific metadata pointer at `+0x0C`, store the compact parser selector
at `+0x10`, copy the stream QWORD to `+0x18/+0x1C`, and zero `+0x20/+0x24`.
They do not write `+0x14` in the observed common initialization sequence; its
meaning is unresolved. The metadata pointers differ, but their strings all
begin `msg-83`; neither that text nor the pointer is proven to be the runtime
namespace selector. The distinct derived RTTI/vptrs and parser-route cases
establish the parser family. A C# snapshot and byte checks for these
constructors and fields live in
`MatrixMessageNamespaceRoutingNativeLayout.cs` and
`StartupBinaryEvidence.cs`.

The stream parser at raw `0x821BA0` reads the envelope QWORD before the compact
selector and passes the QWORD address into `0x81CC50`; each route-specific
constructor passes its two source DWORDs to the base helper, which copies them
to `+0x18/+0x1C`. `tfGameSocketHandler` callback `0x7FDC60` first tests only
the low byte at `message+0x18`. If nonzero, it calls `0x800420`, which consumes
the full `+0x18/+0x1C` key through lookup helper `0x800070` and the socket
handler registries at `+0x24` and `+0x2C`. If the low byte is zero, it compares
`message+0x10` with the nested-message sentinel at `0x1DFC030`; a match takes
the nested path. Otherwise, a non-null callback-context `+8` receiver is
passed the unchanged message pointer at `0x81BB90`. The actual QWORD values
are runtime input and no sample stream was supplied, so none of the six
classes can be assigned to one of these callback branches statically.

The nonzero-key branch is backed by two ordered, QWORD-keyed trees embedded in
the `tfGameSocketHandler` object. At raw `0x7FD670`, its constructor calls
`0x805130` twice and stores the resulting tree headers at `+0x24` and `+0x2C`;
the same constructor stores the factory pointer at `+0x14` (VA `0xC1BDB0` /
raw `0x81B1B0`). `0x800420` copies the message selector and both key DWORDs,
then passes the full key to `0x800070`. That helper searches `this+0x24` and
`this+0x2C` through `0x8013B0`; the tree search compares the key with node
`+0x10/+0x14`, and the mapped pointer is at node `+0x18`. On a first-tree
miss, after the `0x800BF0` creation checks allow it, the callback at context
`+0x14` is called with the QWORD; its dispatcher `0x81B1B0` accepts low-byte
values `0x02..0x35`; the full 52-entry selector mapping is resolved below.
Entry creation uses `0x806910` and tree insertion uses `0x8064C0`; a separate
guarded path at `0x800300` inserts a QWORD-keyed entry into the `+0x2C` tree.
These observations establish the keyed lookup and registration mechanism, not
the meaning of an individual factory selector. The mapped construction cases
are recorded below using their constructors, installed primary vtables, RTTI,
and subsequent receiver call path.

### QWORD-keyed receiver factory: selectors 0x02..0x35

Factory `0x81B1B0` receives the key used by the `0x800070` registry-miss path.
It reads only the low byte of that argument for table selection:
`movzx eax, al; add eax, -2; cmp eax, 0x33; ja 0x81BA93; jmp
[0xC1C6A8+eax*4]`. Thus the table has 52 entries and accepts low-byte values
`0x02..0x35`; table VA `0xC1C6A8` maps to raw `0x81BAA8`. The low byte is a
factory selector, not evidence by itself of a message namespace. The full
QWORD remains the registry key at message `+0x18/+0x1C`. `0x800420` obtains the
registered object through `0x800070`, then uses the returned pointer as the
receiver in its compact-selector dispatch.

The table index is exactly `selector - 2`: selector `0x02` maps to index 0 and
`0x35` to index `0x33`; selector `0x01` underflows the unsigned bound and
`0x36` maps to `0x34`, so both take the out-of-range return. Within the table,
selectors `0x0F` and `0x11` also point to that same zero-return target
`0x81BA93` / VA `0xC1C693`; an in-range selector therefore does not imply
construction.

Each constructing case allocates the listed size through `0x12B7FB0`, calls
its constructor, and the constructor installs the listed RTTI-backed primary
vtable. Text targets use the verified raw-to-VA delta `+0x400C00`; vtable file
offsets use the original PE section map (`.rdata`), not that `.text` delta.
The `.rdata` mapping yields each vtable file offset, and the preceding pointer
resolves its Complete Object Locator (COL) and RTTI name.

| Key low byte | Case raw / VA | Allocation | Constructor raw | Primary vtable VA / raw | Primary slots `+04 / +08 / +0C / +18` (raw) | RTTI |
| --- | --- | ---: | --- | --- | --- | --- |
| `02` | `81B1F2 / C1BDF2` | `B80` | `53F980` | `1C66080 / 1864C80` | `540380 / 541220 / 5441B0 / 54DE00` | `.?AVBaseController@Character@firefall@@` |
| `03` | `81B230 / C1BE30` | `D80` | `54E7B0` | `1C65F1C / 1864B1C` | `54F4A0 / 5503D0 / 5536C0 / 55E160` | `.?AVNPCController@Character@firefall@@` |
| `04` | `81B26E / C1BE6E` | `11D8` | `55E830` | `1C65EF8 / 1864AF8` | `55EAA0 / 55EB10 / 561CF0 / 56B310` | `.?AVMissionAndMarkerController@Character@firefall@@` |
| `05` | `81B2AC / C1BEAC` | `12C0` | `56B970` | `1C65ED4 / 1864AD4` | `56BC20 / 56C0D0 / 56F3A0 / 576C80` | `.?AVCombatController@Character@firefall@@` |
| `06` | `81B2EA / C1BEEA` | `538` | `577320` | `1C65E8C / 1864A8C` | `577410 / 577480 / 5795A0 / 57FBD0` | `.?AVLocalEffectsController@Character@firefall@@` |
| `07` | `81B328 / C1BF28` | `A0` | `57FF80` | `1C65EB0 / 1864AB0` | `5800E0 / 580150 / 580470 / 580F40` | `.?AVSpectatorController@Character@firefall@@` |
| `08` | `81B366 / C1BF66` | `4F8` | `581070` | `1C65E2C / 1864A2C` | `581480 / 5817D0 / 5835C0 / 588500` | `.?AVObserverView@Character@firefall@@` |
| `09` | `81B3A4 / C1BFA4` | `420` | `588830` | `1C65DF4 / 18649F4` | `5889F0 / 588A90 / 589040 / 58A440` | `.?AVEquipmentView@Character@firefall@@` |
| `0A` | `81B3E2 / C1BFE2` | `58` | `58A530` | `1C65DAC / 18649AC` | `58A620 / 58A6E0 / 58AA90 / 58B4C0` | `.?AVAIObserverView@Character@firefall@@` |
| `0B` | `81B41D / C1C01D` | `1258` | `58B5F0` | `1C65D88 / 1864988` | `58B850 / 58BA50 / 58EA40 / 595090` | `.?AVCombatView@Character@firefall@@` |
| `0C` | `81B45B / C1C05B` | `A8` | `595680` | `1C65D58 / 1864958` | `595760 / 595820 / 595E30 / 596250` | `.?AVMovementView@Character@firefall@@` |
| `0D` | `81B499 / C1C099` | `438` | `5962A0` | `1C65D10 / 1864910` | `5963D0 / 596470 / 597590 / 59ACA0` | `.?AVTinyObjectView@Character@firefall@@` |
| `0E` | `81B4D7 / C1C0D7` | `1F8` | `59AEB0` | `1C65D34 / 1864934` | `59AFC0 / 59B060 / 59C600 / 59FF40` | `.?AVDynamicProjectileView@Character@firefall@@` |
| `10` | `81B515 / C1C115` | `88` | `AD9580` | `1C723D8 / 1870FD8` | `AD9790 / AD9830 / AD9BD0 / ADA370` | `.?AVObserverView@Melding@firefall@@` |
| `12` | `81B553 / C1C153` | `58` | `ADA400` | `1C7239C / 1870F9C` | `ADA4D0 / ADA570 / ADAB00 / ADBA10` | `.?AVObserverView@MeldingBubble@firefall@@` |
| `14` | `81B58E / C1C18E` | `158` | `ADBCE0` | `1C72378 / 1870F78` | `ADBE80 / ADBF40 / ADD790 / AE0EF0` | `.?AVObserverView@AreaVisualData@firefall@@` |
| `15` | `81B5CC / C1C1CC` | `628` | `AE10D0` | `1C72334 / 1870F34` | `AE1280 / AE1340 / AE1E80 / AE4170` | `.?AVParticleEffectsView@AreaVisualData@firefall@@` |
| `16` | `81B60A / C1C20A` | `760` | `AE42E0` | `1C722EC / 1870EEC` | `AE44A0 / AE4540 / AE5970 / AE9410` | `.?AVMapMarkerView@AreaVisualData@firefall@@` |
| `17` | `81B648 / C1C248` | `438` | `AE9640` | `1C72310 / 1870F10` | `AE9770 / AE9810 / AEA930 / AEE040` | `.?AVTinyObjectView@AreaVisualData@firefall@@` |
| `18` | `81B686 / C1C286` | `758` | `AEE240` | `1C722C8 / 1870EC8` | `AEE3F0 / AEE4B0 / AEF200 / AF1A60` | `.?AVLootObjectView@AreaVisualData@firefall@@` |
| `19` | `81B6C4 / C1C2C4` | `5C8` | `AF1C00` | `1C722A4 / 1870EA4` | `AF1D30 / AF1DD0 / AF3170 / AF68C0` | `.?AVForceShieldView@AreaVisualData@firefall@@` |
| `1B` | `81B702 / C1C302` | `570` | `B0A990` | `1C72868 / 1871468` | `B0AD20 / B0ADB0 / B0CE40 / B120B0` | `.?AVBaseController@Vehicle@firefall@@` |
| `1C` | `81B740 / C1C340` | `1120` | `B12490` | `1C72824 / 1871424` | `B12650 / B127E0 / B14E20 / B18EB0` | `.?AVCombatController@Vehicle@firefall@@` |
| `1D` | `81B77E / C1C37E` | `478` | `B19380` | `1C727E8 / 18713E8` | `B196E0 / B197A0 / B1B630 / B20100` | `.?AVObserverView@Vehicle@firefall@@` |
| `1E` | `81B7BC / C1C3BC` | `10F8` | `B204A0` | `1C727C4 / 18713C4` | `B20660 / B20720 / B22A80 / B26990` | `.?AVCombatView@Vehicle@firefall@@` |
| `1F` | `81B7FA / C1C3FA` | `B8` | `B26D40` | `1C72784 / 1871384` | `B26E50 / B26F20 / B27670 / B27E20` | `.?AVMovementView@Vehicle@firefall@@` |
| `21` | `81B838 / C1C438` | `78` | `AF6AE0` | `1C72254 / 1870E54` | `AF6C40 / AF6CE0 / AF6E50 / AF7260` | `.?AVAIObserverView@Anchor@firefall@@` |
| `23` | `81B873 / C1C473` | `1710` | `B281A0` | `1C72730 / 1871330` | `B285C0 / B286B0 / B2BCF0 / B34910` | `.?AVObserverView@Deployable@firefall@@` |
| `24` | `81B8B1 / C1C4B1` | `48` | `B34FC0` | `1C72700 / 1871300` | `B35130 / B35200 / B354B0 / B35C40` | `.?AVNPCObserverView@Deployable@firefall@@` |
| `25` | `81B8EC / C1C4EC` | `48` | `B35CC0` | `1C726C4 / 18712C4` | `B35DB0 / B35E50 / B36010 / B364C0` | `.?AVHardpointView@Deployable@firefall@@` |
| `27` | `81B927 / C1C527` | `128` | `B37030` | `1C72648 / 1871248` | `B37220 / B37290 / B37AA0 / B397C0` | `.?AVBaseController@Turret@firefall@@` |
| `28` | `81B965 / C1C565` | `D8` | `B39900` | `1C72618 / 1871218` | `B39AA0 / B39B60 / B3A450 / B3BFA0` | `.?AVObserverView@Turret@firefall@@` |

Each constructor calls the shared helper at raw `0xBCAD70`. It stores the
key's low/high DWORDs at receiver offsets `+0x18/+0x1C`, initializes fields
at `+0x08`, `+0x10`, and `+0x24` to zero, copies its first argument to `+0x20`,
and first writes the helper's unnamed vtable pointer at VA `0x1C660B4` to
offset `+0`. The derived constructor then replaces that pointer with the
listed RTTI-backed vtable. The six added COL offsets for selectors
`0x1B..0x21` are all zero, and no secondary vptr stores were observed in
those constructor bodies. The five constructors at selectors `0x23`, `0x24`,
`0x25`, `0x27`, and `0x28` also call this helper; their vtables map through
`.rdata`, their RTTI TypeDescriptors resolve through the PE section table, and
all five COL subobject offsets are zero.

These are the eight dispatch entries for selectors `0x0A..0x11`; the default
rows have no allocator or constructor:

| Selector | Table index | Entry raw | Target VA / raw | Result |
| --- | ---: | --- | --- | --- |
| `0A` | 8 | `81BAC8` | `C1BFE2 / 81B3E2` | `AIObserverView@Character` |
| `0B` | 9 | `81BACC` | `C1C01D / 81B41D` | `CombatView@Character` |
| `0C` | 10 | `81BAD0` | `C1C05B / 81B45B` | `MovementView@Character` |
| `0D` | 11 | `81BAD4` | `C1C099 / 81B499` | `TinyObjectView@Character` |
| `0E` | 12 | `81BAD8` | `C1C0D7 / 81B4D7` | `DynamicProjectileView@Character` |
| `0F` | 13 | `81BADC` | `C1C693 / 81BA93` | shared zero-return path |
| `10` | 14 | `81BAE0` | `C1C115 / 81B515` | `ObserverView@Melding` |
| `11` | 15 | `81BAE4` | `C1C693 / 81BA93` | shared zero-return path |

The bounded continuation for selectors `0x12..0x19` is:

| Selector | Table index | Entry raw | Target VA / raw | Result |
| --- | ---: | --- | --- | --- |
| `12` | 16 | `81BAE8` | `C1C153 / 81B553` | `MeldingBubble::ObserverView` |
| `13` | 17 | `81BAEC` | `C1C693 / 81BA93` | shared zero-return path |
| `14` | 18 | `81BAF0` | `C1C18E / 81B58E` | `AreaVisualData::ObserverView` |
| `15` | 19 | `81BAF4` | `C1C1CC / 81B5CC` | `AreaVisualData::ParticleEffectsView` |
| `16` | 20 | `81BAF8` | `C1C20A / 81B60A` | `AreaVisualData::MapMarkerView` |
| `17` | 21 | `81BAFC` | `C1C248 / 81B648` | `AreaVisualData::TinyObjectView` |
| `18` | 22 | `81BB00` | `C1C286 / 81B686` | `AreaVisualData::LootObjectView` |
| `19` | 23 | `81BB04` | `C1C2C4 / 81B6C4` | `AreaVisualData::ForceShieldView` |

The eight entries for selectors `0x1A..0x21` are:

| Selector | Table index | Entry raw | Target VA / raw | Result |
| --- | ---: | --- | --- | --- |
| `1A` | 24 | `81BB08` | `C1C693 / 81BA93` | shared zero-return path |
| `1B` | 25 | `81BB0C` | `C1C302 / 81B702` | `Vehicle::BaseController` |
| `1C` | 26 | `81BB10` | `C1C340 / 81B740` | `Vehicle::CombatController` |
| `1D` | 27 | `81BB14` | `C1C37E / 81B77E` | `Vehicle::ObserverView` |
| `1E` | 28 | `81BB18` | `C1C3BC / 81B7BC` | `Vehicle::CombatView` |
| `1F` | 29 | `81BB1C` | `C1C3FA / 81B7FA` | `Vehicle::MovementView` |
| `20` | 30 | `81BB20` | `C1C693 / 81BA93` | shared zero-return path |
| `21` | 31 | `81BB24` | `C1C438 / 81B838` | `Anchor::AIObserverView` |

The eight entries for selectors `0x22..0x29` are:

| Selector | Table index | Entry raw | Target VA / raw | Result |
| --- | ---: | --- | --- | --- |
| `22` | 32 | `81BB28` | `C1C693 / 81BA93` | shared zero-return path |
| `23` | 33 | `81BB2C` | `C1C473 / 81B873` | `Deployable::ObserverView` |
| `24` | 34 | `81BB30` | `C1C4B1 / 81B8B1` | `Deployable::NPCObserverView` |
| `25` | 35 | `81BB34` | `C1C4EC / 81B8EC` | `Deployable::HardpointView` |
| `26` | 36 | `81BB38` | `C1C693 / 81BA93` | shared zero-return path |
| `27` | 37 | `81BB3C` | `C1C527 / 81B927` | `Turret::BaseController` |
| `28` | 38 | `81BB40` | `C1C565 / 81B965` | `Turret::ObserverView` |
| `29` | 39 | `81BB44` | `C1C693 / 81BA93` | shared zero-return path |

Selectors `0x1B..0x1F` install five RTTI-named `Vehicle` receiver types;
selector `0x21` installs `Anchor::AIObserverView`. Each constructor calls
`0xBCAD70` and writes the two key DWORDs at receiver `+0x18/+0x1C`; the
constructor's initial helper vptr at VA `0x1C660B4` is replaced by the
primary vtable listed above. Section-aware `.rdata` mapping resolves each
vtable's Complete Object Locator and TypeDescriptor; all six COL subobject
offsets are zero. This bounded snapshot records the installed primary vptr
and does not claim a hierarchy-wide inventory of secondary subobjects.

This family assignment comes from the concrete constructor-installed vtables
and PE-resolved RTTI names, not from treating the low byte as a namespace ID.
The earlier six additions include five RTTI-named `Character` views and one
`Melding::ObserverView`; the current seven include `MeldingBubble::ObserverView`
and six `AreaVisualData` views; the selector `0x1A..0x21` entries add five
`Vehicle` classes and one `Anchor` class. The latest five add three
`Deployable` classes and two `Turret` classes. These are receiver types, not
parsed-message types. This evidence also corrects the provisional “message factory”
interpretation: these objects are registered against QWORD keys, not created
as per-message parser objects.

The receiver's primary slots are reached through the separate compact selector
at message `+0x10`: selector 1 calls `+0x08`; 2 calls `+0x18`; 3 and 4 call
`+0x0C`; 7 and 8 call `+0x04`; and 9 calls `+0x18`. Selectors 5 and 6 share
the direct helper at `0x800300` and do not call one of these primary slots.
The `+0x04` methods for `BaseController`, `NPCController`, `CombatController`,
and `ObserverView` dispatch message IDs from `message+0x10` over respective
observed ranges `0x53..0xC4`, `0x53..0xC4`, `0x56..0xA8`, and `0x5B..0xAB`,
subject to a class-specific condition. For BaseController and NPCController,
the condition is the non-null virtual-receiver pointer at `+0x2C`, not a
Boolean state flag; the corresponding fields in the other two classes are not
classified here. They first forward to an optional nested object at receiver
`+0x10`; the dispatcher tables themselves are not decoded here. The `+0x04`
methods for `MissionAndMarkerController`,
`LocalEffectsController`, `SpectatorController`, and `EquipmentView` forward
to that optional nested object and conditionally emit the fixed
"this view doesn't have any valid events" message, without a message-ID
argument. These forwarding/logging defaults are not empty stubs and differ
from the dispatchers' invalid-message-ID reporting.

For the newly mapped slots, `Vehicle::BaseController +0x04` handles exactly
message ID `0x5B` when a receiver at `+0x2C` is present; it first forwards to
an optional nested receiver at `+0x10`, and its invalid-ID path calls
`VIBE_LogUnhandledMessageId` with `message+0x10`. The `+0x08` compact-selector
body handler consumes byte data and a length, dispatches byte values through
`0xC1`, and logs values above that bound; individual payload cases remain
unmapped. Its `+0x0C` body handler accepts data and length and may forward to a
nested receiver at `+0x10`, but its large body is not exhaustively mapped. The
`Vehicle::CombatController +0x04` method first forwards to an optional nested
receiver at `+0x10`, then handles message IDs `0x53..0x5A` when a receiver at
`+0x2C` is present; its invalid-ID path calls `VIBE_LogUnhandledMessageId`
with `message+0x10`. `Vehicle::ObserverView`, `CombatView`, and `MovementView`
handle only IDs `0x55`, `0x57`, and `0x56`, respectively. Their invalid IDs
also use the ID-bearing logger; each valid path separately emits
`"received %s"` with `"msg-85"`, `"msg-87"`, or `"msg-86"` and may forward to
a receiver at `+0x2C`. By contrast, `Anchor::AIObserverView` forwards to an
optional nested receiver and may emit the fixed
`"this view doesn't have any valid events"` text through
`VIBE_LogEmitWithArgList`; that log call carries no message ID. The fixed
diagnostic is distinct from an invalid-ID report. These observations describe
the slot code, not message semantics inferred from the class labels.

For the newly mapped `Deployable` and `Turret` receivers, the vtable `+0x04`
targets were read from the installed vtables and traced as actual method bodies.
`Deployable::ObserverView` dispatches only IDs `0x54` and `0x55`, logs
`"received %s"` with `"msg-84"` / `"msg-85"` on those valid paths, and calls
`VIBE_LogUnhandledMessageId` with the supplied ID otherwise.
`Deployable::NPCObserverView` and `Turret::ObserverView` each handle only
`0x53`, using `"msg-83"` on the valid path and the same ID-bearing invalid
logger otherwise. These three methods first forward through an optional
receiver at `+0x10`; when the event receiver at `+0x2C` is present, valid IDs
are forwarded to it. `Deployable::HardpointView` and `Turret::BaseController`
instead forward to the optional `+0x10` receiver and may emit the fixed
`"this view doesn't have any valid events"` diagnostic when `+0x2C` is set;
their method bodies do not inspect a message ID. All five slot `+0x18` methods
store the `dword_1AC4D24` result at receiver `+0x28` and tail-call
`0xBCADF0`. The compact dispatcher routes selectors 1 and 3/4 through slots
`+0x08` and `+0x0C`, respectively; the five receivers' methods at those slots
are mapped by address, while their payload semantics remain outside this
bounded pass.

For selectors `0x0A..0x0C`, the `+0x04` implementations are message-ID
dispatchers: `AIObserverView` handles `0x69`, `CombatView` has a table for
`0x54..0x6C`, and `MovementView` handles `0x6E`; their invalid-ID branches
call `VIBE_LogUnhandledMessageId` with `message+0x10`. The `0x69` and `0x6E`
valid branches separately log `"received %s"` with fixed `"msg-105"` /
`"msg-110"` text, then may forward to an event receiver at `+0x2C`; each also
forwards to a separate optional receiver at `+0x10` before the ID gate.
The new `AreaVisualData::ObserverView`, `ParticleEffectsView`, and
`LootObjectView` `+0x04` methods dispatch only `0x54`, `0x55`, and `0x53`,
respectively; their invalid-ID branches also report `message+0x10` through
`VIBE_LogUnhandledMessageId`, while their valid paths may forward to the
receiver at `+0x2C`. By contrast, the `+0x04` methods for `TinyObjectView`,
`DynamicProjectileView`, `Melding::ObserverView`,
`MeldingBubble::ObserverView`, `AreaVisualData::MapMarkerView`,
`AreaVisualData::TinyObjectView`, and `AreaVisualData::ForceShieldView`
forward to optional nested receivers and conditionally call
`VIBE_LogEmitWithArgList` with the fixed text
`"this view doesn't have any valid events"` without a message-ID argument.
Across the 36 constructing receivers mapped through selector `0x35`, `+0x04`
has nineteen message-ID dispatchers and seventeen forwarding/logging defaults.
The fixed no-events
diagnostics are not invalid-ID reports or empty stubs.

All thirty-six mapped `+0x18` targets are short state-refresh wrappers: they call
the indirect helper at `dword_1AC4D24`, store its result at receiver `+0x28`,
and call `orchestrator_BCADF0`. They ignore payload arguments at this entry
point but are not no-op stubs. The `+0x08` and `+0x0C` targets are compact-selector
body handlers reached with data pointers and lengths; their individual payload
fields remain outside this bounded pass. The
`AIObserverView +0x08` target at `0x58A6E0` has a recovered disassembly
boundary (size `0x394`, ending in `retn 8` at `0x58AA71`); decompilation and
payload semantics remain unresolved. The earlier targets at `0x5536C0` and
`0x5795A0` have a function-boundary limitation, so none are classified
as empty stubs.

The exact table, constructor/vtable/RTTI snapshots, compact-selector call
sites, and original-image fingerprints are kept separately in
`MatrixKeyedReceiverFactoryNativeLayout.cs`,
`MatrixMessageNamespaceRoutingNativeLayout.cs`, and
`StartupBinaryEvidence.cs`. All selectors `0x02..0x35` are now bounded:
36 entries construct RTTI-backed receivers and sixteen table entries share the
zero-return target `0x81BA93` / VA `0xC1C693`.

Each case calls allocator raw `0x12B7FB0` / VA `0x16B8BB0`; its listed
constructor then calls the shared base-key helper raw `0xBCAD70` / VA
`0xFCB970`, which stores the full key at `+0x18/+0x1C`. The final four
constructing cases are:

| Selector | Table entry raw | Case raw / VA | Allocation | Constructor raw / VA | Primary vtable VA / raw | COL VA / raw | TypeDescriptor VA / raw | RTTI |
| --- | --- | --- | ---: | --- | --- | --- | --- | --- |
| `2D` | `81BB54` | `81B9A3 / C1C5A3` | `100` | `AF7300 / EF7F00` | `1C72278 / 1870E78` | `1CCA910 / 18C9510` | `1E29EC0 / 1A282C0` | `.?AVObserverView@Outpost@firefall@@` |
| `30` | `81BB60` | `81B9E1 / C1C5E1` | `11A0` | `B3C0F0 / F3CCF0` | `1C725E4 / 18711E4` | `1CCBA18 / 18CA618` | `1E2A3F0 / 1A287F0` | `.?AVObserverView@ResourceNode@firefall@@` |
| `33` | `81BB6C` | `81BA1F / C1C61F` | `1690` | `AFC100 / EFCD00` | `1C721E0 / 1870DE0` | `1CCA8A0 / 18C94A0` | `1E29EEC / 1A282EC` | `.?AVObserverView@CarryableObject@firefall@@` |
| `35` | `81BB74` | `81BA59 / C1C659` | `758` | `B43B10 / F44710` | `1C72574 / 1871174` | `1CCBA04 / 18CA604` | `1E2A3B4 / 1A287B4` | `.?AVLootObjectView@LootStoreExtension@firefall@@` |

Each derived constructor calls the shared base-key helper `0xBCAD70` before
installing its primary vtable. Text methods use raw+`0x400C00` for VA
conversion; vtable and RTTI addresses use the original PE `.rdata` section map.
The COL subobject offset is zero for each. The four `+0x04` methods are at raw
`AF7420`, `B3C320`, `AFC430`, and `B43CC0`: the first three forward through an
optional receiver at `+0x10` and conditionally emit the fixed
`"this view doesn't have any valid events"` diagnostic, without consuming a
message ID. The `0x35` method accepts exactly message ID `0x53`; invalid IDs
call `VIBE_LogUnhandledMessageId` with the value at `message+0x10`, while the
valid route may log `"received %s"` with `"msg-83"` and forward to a nested
receiver at `+0x2C`.

For these four receivers the `+0x08/+0x0C/+0x18` raw targets are, respectively,
`AF74C0/AF8DC0/AFBBF0`, `B3C3C0/B3EAB0/B43640`,
`AFC4D0/AFF670/B070E0`, and `B43D80/B44AD0/B47330`. The `+0x08/+0x0C`
targets receive compact-body data pointers and lengths; their payload fields
and consumer tails remain unresolved. Each `+0x18` target stores the
`dword_1AC4D24` result at receiver `+0x28` and calls `0xBCADF0`.

The following matrix is the complete 52-entry table summary. A zero-return
entry targets `0x81BA93`; every other selector constructs the RTTI type shown.

| Selector | Factory result | Selector | Factory result | Selector | Factory result | Selector | Factory result |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `02` | `Character::BaseController` | `03` | `Character::NPCController` | `04` | `Character::MissionAndMarkerController` | `05` | `Character::CombatController` |
| `06` | `Character::LocalEffectsController` | `07` | `Character::SpectatorController` | `08` | `Character::ObserverView` | `09` | `Character::EquipmentView` |
| `0A` | `Character::AIObserverView` | `0B` | `Character::CombatView` | `0C` | `Character::MovementView` | `0D` | `Character::TinyObjectView` |
| `0E` | `Character::DynamicProjectileView` | `0F` | zero | `10` | `Melding::ObserverView` | `11` | zero |
| `12` | `MeldingBubble::ObserverView` | `13` | zero | `14` | `AreaVisualData::ObserverView` | `15` | `AreaVisualData::ParticleEffectsView` |
| `16` | `AreaVisualData::MapMarkerView` | `17` | `AreaVisualData::TinyObjectView` | `18` | `AreaVisualData::LootObjectView` | `19` | `AreaVisualData::ForceShieldView` |
| `1A` | zero | `1B` | `Vehicle::BaseController` | `1C` | `Vehicle::CombatController` | `1D` | `Vehicle::ObserverView` |
| `1E` | `Vehicle::CombatView` | `1F` | `Vehicle::MovementView` | `20` | zero | `21` | `Anchor::AIObserverView` |
| `22` | zero | `23` | `Deployable::ObserverView` | `24` | `Deployable::NPCObserverView` | `25` | `Deployable::HardpointView` |
| `26` | zero | `27` | `Turret::BaseController` | `28` | `Turret::ObserverView` | `29` | zero |
| `2A` | zero | `2B` | zero | `2C` | zero | `2D` | `Outpost::ObserverView` |
| `2E` | zero | `2F` | zero | `30` | `ResourceNode::ObserverView` | `31` | zero |
| `32` | zero | `33` | `CarryableObject::ObserverView` | `34` | zero | `35` | `LootStoreExtension::LootObjectView` |

The `0xBD7060` fan-out is a separate conditional side path: `0x7FDC60` reaches
it when the message predicate at `message+0x18` returns zero and context
`+0xB0` is non-null; its selector is `3` for a zero callback argument and `2`
otherwise. `0xBD7060` serializes the message QWORD and message data into an
output stream in this order: raw eight bytes from message `+0x18/+0x1C`, a
compact positive UInt16 encoding of message `+0x10`, then the message vtable's
slot-`+0` serialization. It then passes the selector separately to
`0xBD7460`. The latter stores that selector in the recipient callback's local
header at `0xBD7482`; it is not serialized into the stream. That function
iterates 32-bit recipient pointers in its owner's vector `[+0x2C,+0x30)`
(capacity at `+0x34`), stepping four bytes at a time. For each item it
performs an indirect `__thiscall` through vtable slot `+0x0C`, passing the
output-buffer pointer separately from an eight-byte local header: DWORD
`owner[+0x20]` at header `+0`, WORD output length at `+4`, selector byte at
`+6`, and zero byte at `+7`. The length is narrowed to WORD when the header
is populated. The exact concrete type of every vector entry is not established.

A representative registration path is visible at raw `0x7FE1C0`: it lazily
allocates a `0x48`-byte state object, calls `0xBD6E10`, appends a recipient
pointer to the state's `+0x2C` vector with `0xBD8C90`, then calls the
recipient's vtable slot `+4`. The constructor copies the caller's pair at
`this+0x0C` to state `+0x18/+0x1C`, stores its QWORD argument at `+0x08/+0x0C`,
and zeroes vector fields `+0x2C/+0x30/+0x34`. A concrete recipient on this
observed insertion path is `nl::DefaultReplayWriter`: `0x7FE360` constructs it
at `0xBD7EA0`, whose final vtable VA `0x1C73ADC` has matching RTTI; its slot
`+0x0C` resolves to VA `0xFD9010` / raw `0xBD8410`. That method checks the
record/input lengths through `0x12A0660` and returns a Boolean. This is a
representative recipient of the observed `0x7FE1C0` insertion path, not proof
that every `0xBD7460` vector or the `0x7FDC60` context's `+0xB0` state uses
that class: no direct call from `0x7FDC60` to `0x7FE1C0` was found, and the
shared `+0xB0` offset alone does not establish object identity. The
`context+8` route to `0x81BB90` remains separate; its concrete
`tfGameProtocolHandler` recipient and selector-`0x53` slot mapping below do
not identify the fan-out recipient type or message namespace.

The concrete `tfGameProtocolHandler` has primary vtable VA `0x1C71EC0`, with
COL VA `0x1CCA404` and object offset zero. Its RTTI hierarchy contains
`tfGameProtocolHandler` and `MessageHandler` base descriptors, both at
subobject offset zero; no secondary vtable group was observed for this class.
On the conditional `0x81BB90` path, selector `0x53` indexes slot `+0xD0`,
target VA `0xECB180` / raw `0xACA580`. That method reads `message+0x34`.
Consequently, a `Vehicle::AbilityActivated` object of allocated size `0x30`
would have an apparent extent mismatch if it reached this slot; the unknown
envelope-key branch means that runtime path has not been established. This is
an unresolved conditional observation, not a claim that the parser routes
Vehicle messages to that method.

The original executable (`FirefallClient.exe.original`, 40,197,848 bytes,
SHA-256 `E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235`)
was read directly; `.text` VAs are raw offsets plus `0x400C00`, and `.rdata`
references are translated using its PE section table. No native execution,
IDB edits, or sample edits were performed. This resolves the static routing
conditions and parser-family collisions; it does not repair or reinterpret
the already-recorded field/extent conflicts.
