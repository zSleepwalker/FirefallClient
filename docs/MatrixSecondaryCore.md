# Matrix secondary selectors: core entries

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
