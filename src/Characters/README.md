# Characters — Character command messages

Partial reconstruction of `firefall::Character::Commands` messages. The IDA database is
read **and** annotated (renames/comments only; no code patching). All addresses are
**raw file offsets**: the IDA database is loaded at base 0.

The IDA labels "tfRemoteCharacter*" / "CallbackWireEncode*" came from earlier auto-naming and are
**not** evidence. Class names below are taken from MSVC RTTI.

Selected remote-observer send gates and the `UpdateShoppingList` source-map
builder are documented in [`RemoteCharacterDispatch.md`](../../docs/RemoteCharacterDispatch.md).
Its state-channel notes distinguish the persistent message-194 map from
adjacent script/view collections.

| File | Contents |
|---|---|
| `CharacterCommandMessages.cs` | Message models, native field offsets, set ordering rules |
| `CharacterObserverDispatchLayout.cs` | Selected observer gates, senders, and inline shopping-list builder offsets |
| `EquipmentCommandPayloads.cs` | Native-layout models for equipment requests 204/205/207; no unverified wire encoder |
| `SlotVisualMultiRequest.cs` | Native-layout model and verified payload serializer for request 208, including nested lists/text |
| `RepairRequest.cs` | Repair request 203: bounded sender, deep-copied identifiers, verified chunked payload |
| `SalvageRequest.cs` | Salvage request 202: 16-byte records, verified chunked payload and complete message body |
| `RequestTeleport.cs` | Request 210: opaque 64-bit value at +0x28; payload encoder not inferred |
| `RequestRespawn.cs` | Request 198: kind-only command with verified 9-byte body |
| `DuelRequest.cs` | Duel request 188: copied descriptor with shared-text refs, two pair vectors and mode byte; no wire writer reconstructed |
| `RequestTransfer.cs` | Lua-reachable request 199 with verified 14-byte body writer |
| `FindNearbyResourceAreas.cs` | Request 183: kind-only constructor found; no body writer reconstructed |
| `VehicleCalldownRequest.cs` | Request 179: populated native-layout model with a copied opaque 0x40-byte block; no wire writer reconstructed |
| `DeployableCalldownRequest.cs` | Request 180: populated native-layout model with a copied opaque 0x40-byte block; no wire writer reconstructed |
| `ClearPreviousBounties.cs` | Request 162: one positional u32 subclass field; target is retagged with kind 4 by its caller |
| `RequestRerollBounties.cs` | Request 164: copied vector of positional u32 values; caller sends a one-value vector |
| `MovementInput.cs` | Request 115: partially modeled complex descriptor and trailing values; embedded process-local shared reference omitted |
| `MovementInputFake.cs` | Request 116: positional scalar fields and optional complex descriptor; embedded process-local shared reference omitted |
| `FireWeaponProjectile.cs` | Request 121: four positional u32 fields and an optional three-u32 block |
| `SelectWeapon.cs` | Request 125: one u32 and two byte fields |
| `ReloadWeapon.cs` | Request 126: one u32 field |
| `AcquireWeaponTarget.cs` | Request 128: four positional u32s and one byte |
| `LoseWeaponTarget.cs` | Request 129: three positional u32s |
| `SelectFireMode.cs` | Request 123: one u32 and one byte |
| `UseScope.cs` | Request 124: one u32 and one byte |
| `CancelReload.cs` | Request 127: one u32 |
| `ReportProjectileHit.cs` | Request 122: unaligned sequence of u16s and bytes |
| `FireInputIgnored.cs` | Request 117: one u32 and one byte |
| `FireBurst.cs` | Request 118: one u32 |
| `FireEnd.cs` | Request 119: one u32 |
| `FireCancel.cs` | Request 120: one u32 |
| `NpcApplyEffect.cs` | Request 130: three positional u32 fields |
| `NpcRemoveEffect.cs` | Request 131: one positional u32 field |
| `ChangeLookAtTarget.cs` | Request 133: optional group of six u32 fields |
| `DockToPlayer.cs` | Request 132: five positional u32 fields |
| `NpcInteractWithTarget.cs` | Request 135: five positional u32 fields with a gap at +0x2C |
| `TargetAbility.cs` | Request 136: one positional byte |
| `DeactivateAbility.cs` | Request 137: one u32 and one byte |
| `PerformEmote.cs` | Request 142: one u32 and one signed u16 |
| `ActivateConsumable.cs` | Request 138: two positional u32 fields |
| `SetNoSpreadFlag.cs` | Request 139: two bytes |
| `ExitAttachmentRequest.cs` | Request 140: kind-only request |
| `ListPreviousBounties.cs` | Request 163: one positional u32 |
| `ListAchievements.cs` | Request 168: one positional byte |
| `RequestAllAchievements.cs` | Request 170: kind-only request |
| `AbortCampaignMission.cs` | Request 152: one positional u32 |
| `AnimationUpdate.cs` | Request 149: two positional u32s and one byte |
| `TryResumeTutorialChain.cs` | Request 153: kind-only command |
| `CallForHelp.cs` | Request 151: one positional u32 |
| `ListAvailableBounties.cs` | Request 160: two positional u32s |
| `SetDialogTag.cs` | Request 147: shared-text reference, u32, and two bytes |
| `NotifyDialogScriptComplete.cs` | Request 143: two positional u32s |
| `PerformQuickChatCommand.cs` | Request 144: one positional u32 |
| `PerformDialog.cs` | Request 146: positional values at observed offsets |
| `SetEffectsFlag.cs` | Request 148: two positional bytes |
| `SelectLoadout.cs` | Request 150: one u32 and one byte |
| `DebugMission.cs` | Request 154: u32 and opaque shared-text reference |
| `AssignBounties.cs` | Request 155: one positional u32 |
| `AbortBounty.cs` | Request 156: one positional u32 |
| `ActivateBounty.cs` | Request 157: one positional u32 |
| `ListActiveBounties.cs` | Request 158: one u32 and one byte |
| `ListActiveBountyDetails.cs` | Request 159: one u32 and one byte |
| `ClearBounties.cs` | Request 161: one positional u32 |
| `TrackBounty.cs` | Request 165: one positional u32 |
| `SetBountyVar.cs` | Request 166: u32 and two opaque references |
| `RefreshBounties.cs` | Request 167: kind-only request |
| `NpcSetInteractionType.cs` | Request 141: byte and copied vector of positional u64s |
| `ActivateAbility.cs` | Request 134: positional fields, copied 8-byte vector, and optional trailing DWORDs |
| `SendMailToPlayer.cs` | Request 84: positional fields, three shared-text references, and copied 24-byte records |
| `ListItemForSale.cs` | Request 83: positional fields, two shared-text references, and a trailing byte |
| `ClearSavedMatchQueue.cs` | Request 91: two positional u32s and a zeroed native tail |
| `MatchAccept.cs` | Request 93: one positional u32, a pair of u32s, and a byte |
| `PlayerReady.cs` | Request 88: two bytes and a presence-flagged vector of u64 entries |
| `ToggleMarketplace.cs` | Request 86: one positional byte |
| `FillBuyOrder.cs` | Request 85: eight positional u32 fields |
| `MatchmakerSetPenalties.cs` | Request 92: u32, byte, u32, byte |
| `JoinGroupLeader.cs` | Request 87: kind-only, inherited target only |
| `FetchQueueInfo.cs` | Request 89: copied vector of retained 32-bit references and three positional u32s |
| `ClientUIEvent.cs` | Request 196: opaque shared-text reference |
| `VendorPurchaseRequest.cs` | Request 238: four positional u64s, two optional u32s, and an optional shared-text pointer |
| `VendorTokenMachineRequest.cs` | Request 200: mode byte and two positional u32s |
| `FindServiceProvider.cs` | Request 195: one positional byte |
| `UpdateFriendStatus.cs` | Request 249: shared-text reference, byte, and optional shared-text reference |
| `FriendsListRequest.cs` | Request 248: one positional u32 |
| `ClientQueryInteractionStatus.cs` | Request 186: positional u64 and byte |
| `ClaimBountyRewards.cs` | Request 250: kind-only, inherited target only |
| `LootDistributionSetVotes.cs` | Request 219: u32 and copied vector of opaque u64s |
| `UiQueryResponse.cs` | Request 59: three positional u32s and copied u32-to-u32 map |
| `LootDistributionSetState.cs` | Request 218: positional u32 and byte |
| `RequestTrackerUpdate.cs` | Request 217: two positional u32 fields |
| `LeaveArc.cs` | Request 214: kind-only, inherited target only |
| `RequestFrameLevelReset.cs` | Request 211: one positional u32 |
| `JobLedgerOperation.cs` | Request 215: shared-text reference and copied u32 vector |
| `ExecuteTinkeringPlan.cs` | Request 237: positional u32 and three copied opaque 16-byte-element vectors |
| `QueueUnstuck.cs` | Request 178: kind-only, inherited target only |
| `NPCCombatUpdate.cs` | Request 233: two positional u32 fields |
| `EquipExperimentalLoadout.cs` | Request 236: one positional u32 field |
| `Fabrication_Finalize.cs` | Request 246: positional u32 and byte |
| `Fabrication_Claim.cs` | Request 247: two positional u32 fields |
| `Fabrication_GenerateResult.cs` | Request 245: three positional u32 fields |
| `Fabrication_ApplyAction.cs` | Request 244: three positional u32 fields |
| `Fabrication_FetchInstance.cs` | Request 242: one positional u32 field |
| `Fabrication_Start.cs` | Request 243: positional u32 and byte |
| `Fabrication_FetchAllInstances.cs` | Request 240: kind-only, inherited target only |
| `Fabrication_FetchAllRecipes.cs` | Request 241: kind-only, inherited target only |
| `SetSteamUserId.cs` | Request 235: copied 64-bit value at `+0x28` |
| `FlushCharacterCache.cs` | Request 231: five positional bytes |
| `BagInventorySettings.cs` | Request 234: optional nested shared-object handle and u32 |
| `TutorialEventTriggeredCmd.cs` | Request 239: positional u32s and byte, with an untouched gap |
| `ResetAllEliteLevelsUpgrades_Debug.cs` | Request 230: kind-only debug command |
| `RevertAllEliteLevelsUpgrades.cs` | Request 228: kind-only command |
| `SelectEliteLevelsAward.cs` | Request 229: one positional u32 |
| `RunTeamManagerCommand.cs` | Request 232: byte, shared-text reference, byte |
| `FlushRewards.cs` | Request 226: kind-only command |
| `RerollEliteLevelsAwardList.cs` | Request 227: kind-only command |
| `BuyBackPreviousDay.cs` | Request 224: one positional u32 |
| `AcceptRewards.cs` | Request 225: one positional u32 |
| `ClaimDailyRewardStreak.cs` | Request 223: one positional u32 |
| `ClaimDailyRewardItem.cs` | Request 222: subclass DWORD initialized to zero by both observed constructors |
| `SeatChangeRequest.cs` | Request 216: one positional byte |
| `RequestSelfRevive.cs` | Request 209: kind-only command |
| `TimedDailyRewardRequest.cs` | Request 201: one positional byte |
| `RequestNewActivity.cs` | Request 172: kind-only command |
| `RequestPushMission.cs` | Request 173: one positional u32 |
| `RequestMissionAvailability.cs` | Request 171: three positional u32s |
| `SinAcquire_Source.cs` | Request 113: two positional u32s |
| `BroadcastWeaponTweaks.cs` | Request 114: two bytes, embedded shared-reference pair, u32 |
| `TempSlotAbilities.cs` | Request 112: four zeroed u32s and all-ones sentinel |
| `CollectLoot.cs` | Request 111: four positional u32s |
| `MapOpened.cs` | Request 109: kind-only command |
| `BattleframePurchased.cs` | Request 110: kind-only command |
| `ChallengeStartMatch.cs` | Request 106: two positional u32s |
| `ChallengeSetPowerPrivilege.cs` | Request 105: four u32s and one byte |
| `ChallengeSetRoleAndTeam.cs` | Request 103: four u32s, byte, and u32 |
| `ChallengeSetMatchParameters.cs` | Request 104: three positional u32s |
| `ChallengeSwapTeam.cs` | Request 102: six positional u32s |
| `ChallengeReadyCheck.cs` | Request 101: two positional u32s |
| `ChallengeKick.cs` | Request 99: two positional u32s and shared-text pointer |
| `ChallengeSetReady.cs` | Request 100: two positional u32s and byte |
| `ChallengeInvitationSquadInfo.cs` | Request 98: four u32s, shared-text pointer, copied u64 vector |
| `ChallengeInvitationResponse.cs` | Request 97: two positional u32s and byte |
| `ChallengeLeave.cs` | Request 95: two positional u32s |
| `ChallengeInvitation.cs` | Request 96: two u32s, shared-text pointer, byte, u32 |
| `ChallengeCreate.cs` | Request 94: copied u64 vector |
| `PickupCarryableObjectByProximity.cs` | Request 189: opaque u64 object identifier |
| `DropCarryableObject.cs` | Request 190: opaque u64 object identifier |
| `SlotGearRequest.cs` | Request 206: positional u32, u64, byte, and u32 |
| `LFGCheckin.cs` | Request 107: two positional u32s |
| `LFGLeave.cs` | Request 108: two positional u32s |
| `JoinSquadLeadersArc.cs` | Request 213: kind-only command |
| `LeaveEncounterParty.cs` | Request 212: two positional u32s |
| `ResourceLocationInfosRequest.cs` | Request 187: kind-only command |
| `UpdateChatPartyMembers.cs` | Request 185: copied 16-byte-element vector and two positional u32s |
| `MatchQueue.cs` | Request 90: copied u64/u32 vectors and positional scalar/shared-text fields |
| `ResetTutorialId.cs` | Request 220: tutorial ID DWORD |
| `DismissTutorialId.cs` | Request 221: tutorial ID DWORD |
| `ResourceNodeBeaconCalldownRequest.cs` | Request 182: three positional u32s |
| `DeployableHardpointSelection.cs` | Request 181: three positional u32s |
| `CameraPoseUpdate.cs` | Request 177: target plus two optional opaque positional blocks |
| `AiSignal.cs` | Request 192: two opaque shared-text references, a copied 16-byte block, and two u32s |
| `AiError.cs` | Request 191: one opaque shared-text reference |
| `LogLongTimeWithoutPush.cs` | Request 176: kind-only message |
| `GeographicalReportRequest.cs` | Request 184: one positional byte |
| `NonDevDebugCommand.cs` | Request 193: byte, u32, and opaque shared-text reference |
| `PerformTextChat.cs` | Request 145: shared-text fields, conditional nested values, and an optional descriptor |
| `SetMovementSimulation.cs` | Request 197: one u16 subclass field; caller is guarded by a state byte and 1000 ms interval |
| `RequestAchievementStatus.cs` | Request 169: one positional u32 subclass field; constructor and two production call paths found |
| `CharacterCommandWire.cs` | Body encoder, MessageID packing, 255-chunk set writer, send gate, flush throttle |
| `MessageCatalog.csv` | **Full catalog of all 169 `Character::Commands` message classes** — RTTI name, vtable address, 3 vtable slot addresses, COL/type-descriptor addresses (see "Full message catalog" below) |
| `Checks\` | Console wire-format checks: 0xAE/0xAF/0xC2 bytes; chunk boundaries 0/1/254/255/256/510/511; MessageID; throttle. Excluded from the parent project via `DefaultItemExcludes` |

Run the checks from the repository root: `dotnet run --project src\Characters\Checks\Characters.Checks.csproj`. Exit code 0 means every check passed.

## Full message catalog (169 classes)

The 169 `Character::Commands` classes occupy 16-byte RTTI/vtable records in the raw
`0x1864cd4`–`0x1865774` span (each record begins with that class's
`RTTICompleteObjectLocator` VA, followed by the actual vtable address and two more virtual-method
VAs). There is one intervening non-command record at raw `0x1865754`, whose RTTI names
`nl::MessageBaseT<firefall::Character::Fabrication_Finalize_Response>`; the Character-command CSV
intentionally omits it. The span was
found by noticing `ResetTutorialId`'s and `DismissTutorialId`'s vtables sit back-to-back, then
walking the array outward in both directions, resolving each entry's RTTI chain to a class name.
The full dump (class name, record/vtable addresses, 3 slot addresses, COL address, type-descriptor
address, all as raw file offsets) is in **`MessageCatalog.csv`** — 169 command rows spanning
`ClaimBountyRewards` … `ListItemForSale`; the intervening non-command record is omitted.

This catalog is the master index for future reverse-engineering sessions: for any class in it, the
general pipeline below recovers its vtable; from there, searching for code references to the
vtable's own VA (not the COL) finds that class's constructor(s), which is the fastest way to learn
its field layout.

### Technique: RTTI name string → vtable → constructor

IDA's automatic cross-reference analysis is unreliable in this raw-loaded IDB (absolute immediates
get mis-resolved), so this project uses a manual, raw-byte-search pipeline instead. Given only an
RTTI class name (e.g. from the catalog or from `find_regex`):

1. Find the type descriptor's name string: `find_regex` for `AV<ClassName>@Commands@Character@firefall`.
2. `type_descriptor_raw = string_addr − 8` (the standard MSVC `{vfptr, spare, name[]}` layout).
3. Convert to VA using the PE section table (raw → VA, see below).
4. Raw-byte-search for the 20-byte pattern `{0,0,0, type_descriptor_VA, *}` (a standard 5-DWORD
   `RTTICompleteObjectLocator`: `signature=0, offset=0, cdOffset=0, pTypeDescriptor, pClassHierarchyDescriptor`).
   The match address is the COL.
5. Convert the COL's own address to VA, then raw-byte-search for that VA as a plain 4-byte LE value.
   The match address **+ 4** is the vtable (the COL VA occupies the vtable's "−4" slot, exactly like
   a normal MSVC vtable layout, it's just not auto-recognized by IDA here).
6. Raw-byte-search for the **vtable's own VA** as a 4-byte LE value. Matches land inside code that
   constructs instances of the class (every constructor writes the vtable pointer into `this`).

This fully reproduces the standard MSVC RTTI chain; it was validated against the three
already-documented classes (174/175/194) before being used to find new ones. It is reusable as-is
for any of the other 166 names already catalogued in `MessageCatalog.csv`.

**Caveat on the 3 per-class vtable slots**: these are *not* the Sender/Encoder/Gate functions
documented for 174/175/194 below (those are separate, non-virtual, free functions found via other
means, e.g. Lua bindings). The 3 vtable slots are generic `MessageBaseT<T>` virtual methods
(destructor-like, and two others). For many classes IDA has merged dozens of these small per-class
stubs into one large "function" (e.g. slot 0/1 of both `ResetTutorialId` and `DismissTutorialId`
land inside a single undifferentiated 0xEF8-byte blob at raw `0x9B73F0`), so they are not cheaply
traceable without manually splitting functions in IDA — the constructor-via-step-6 approach above is
the practical way to learn a class's fields.

### Newly traced classes

| Class | Vtable (raw) | Ctor (production) | Ctor (debug) | Field(s) | Notes |
|---|---|---|---|---|---|
| `ResetTutorialId` | 0x1864eb8 | `ResetTutorialId_Ctor_Production` 0x53bc50 (caller 0xa2b9c8) | `ResetTutorialId_Ctor_Debug` 0x53bb30 (no static callers) | `this+0x28`: `tutorialId` (u32), range `[1,14]` | Range check `(u8)(v-1) > 0xD`; debug ctor logs via `VIBE_FormatSharedText`/`dword_1C15EA0`, source lines 5965/5972 |
| `DismissTutorialId` | 0x1864ec8 | `DismissTutorialId_Ctor_Production` 0x53be20 (caller 0xa2bb78) | `DismissTutorialId_Ctor_Debug` 0x53bd00 | `this+0x28`: `tutorialId` (u32), range `[1,14]` | Identical shape to ResetTutorialId; source lines 5992/5999; constructed from `dispatch_control_routine_A2BA30` (raw 0xa2ba30), a **Lua-script-exposed** builder that pulls an integer argument from the script call stack — i.e. this command is reachable from in-game Lua scripts, not just normal gameplay code |

Both vtables sit immediately adjacent in the master array (`0x1864eb8`/`0x1864ec8`, 16 bytes apart),
confirming the array is compiler-emitted in source-declaration order — `ResetTutorialId` and
`DismissTutorialId` were declared next to each other in the original `.cpp`.

In `MessageCatalog.csv`, `vtable_entry_raw` is the 16-byte record's start (the slot holding the COL
VA); `vtable_raw` is the actual code-referenced vtable address and equals `vtable_entry_raw + 4`.
The `vtable_raw` values can be searched directly after converting them from raw offsets to VAs.

| Class | Vtable (raw) | Ctor(s) | Field(s) | Notes |
|---|---|---|---|---|
| `AiSignal` | 0x1865078 | `AiSignal_Ctor_Production` 0x538770 (no static callers); `AiSignal_Ctor_Debug` 0x538560 | `+0x28`/`+0x2C`: two shared-text refs; `+0x30..0x3F`: copied opaque 16-byte block; `+0x70`/`+0x74`: two u32s | kind `[1,14]`; log lines 5194/5204; gaps are preserved as unknown rather than assigned meaning |
| `AiError` | 0x1865098 | `AiError_Ctor_Production` 0x538470 (no static callers); `AiError_Ctor_Debug` 0x5382c0 | `+0x28`: 1 shared-text ref (error message?) | kind `[1,14]`; log lines 5164/5171 |
| `GeographicalReportRequest` | 0x1865118 | empty/default 0x537430; populated 0x537530 (caller 0x4BD820) | default initializes byte `+0x28` to zero; populated copies one byte there | metadata ID 184; value meaning unknown |
| `LogLongTimeWithoutPush` | 0x1865178 | 0x5366C0 (callers 0x909066 and 0xA2D95A) | no subclass payload | metadata ID 176 |
| `NonDevDebugCommand` | 0x1865088 | default 0x5388A0; populated 0x5389B0 (caller 0x48E746) | byte `+0x28`, DWORD `+0x2C`, shared-text reference `+0x30` | metadata ID 193; meanings remain positional |
| `CameraPoseUpdate` | 0x1865188 | `CameraPoseUpdate_Ctor_Simple` 0x5367d0 (real callers 0x864aba/0x864c74 — camera-pose-change handlers); `CameraPoseUpdate_Ctor_WithOptionalPose` 0x536910 (no static callers) | Simple ctor: no payload beyond kind+value, where (kind,value) is a packed **target id** (kind=2 local character, kind=7 observed/remote target) built by `routine_general_932510`/`leaf_linear_3264C0`. Rich ctor: `+0x28` bool hasPos + `+0x30`/`+0x34` 2×u32 (position?); `+0x38` bool hasOrient + `+0x3C`/`+0x40`/`+0x44` 3×u32 (orientation?) | **Misleading name**: the message reports *whose* camera pose changed (a target id), not literal pose data, in its only-ever-called variant; the richer position/orientation-carrying overload exists in the binary but has no found callers |
| `DropCarryableObject` | 0x18650b8 | `DropCarryableObject_Ctor_Production` 0x5381f0; `DropCarryableObject_Ctor_Debug` 0x5380c0 | `+0x28`/`+0x2C`: 64-bit value (object/entity id) | kind `[1,14]`; log lines 5137/5144 |
| `PickupCarryableObjectByProximity` | 0x18650a8 | `PickupCarryableObjectByProximity_Ctor_Production` 0x538010; `..._Ctor_Debug` 0x537ee0 | `+0x28`/`+0x2C`: 64-bit value (object/entity id) | Identical shape to `DropCarryableObject`; kind `[1,14]`; log lines 5110/5117 |
| `RequestSelfRevive` | 0x1864f78 | `RequestSelfRevive_Ctor` 0x53a830 (only variant found) | none — kind-only message | kind `[1,14]`; log line 5685 |
| `SlotGearRequest` | 0x1864f98 | `SlotGearRequest_Ctor_Production` 0x53a300; `SlotGearRequest_Ctor_Debug` 0x53a1e0 | `+0x28`: u32 (slot index?); `+0x30`/`+0x34`: 64-bit value (item id?); `+0x38`: byte flag; `+0x3C`: u32 | kind `[1,14]`; log lines 5586/5596 |
| `QueueUnstuck` | 0x1865158 | `QueueUnstuck_Ctor` 0x536a20 (only variant found) | none — kind-only message | kind `[1,14]`; log line 4820 |

`RequestSelfRevive` and `QueueUnstuck` are the simplest possible shape: a vtable pointer and a
`kind` validity check, no payload at all — both have exactly one constructor in the binary (no
separate debug/production split), suggesting the compiler folded the two variants together when
there's no field whose value needs formatting into the debug log message.

### Equipment request constructors (204, 205, 207)

These three have **real construction callers** in the equipment subsystem. The addresses in the
CSV identify the COL slot; add 4 to obtain the code-referenced vtable. Native payload layouts
are modeled in `EquipmentCommandPayloads.cs`, without guessing the packet body format.

| RTTI class / native ID | Vtable (raw) | Constructors (raw) | Native payload (offset from object) | Caller evidence |
|---|---|---|---|---|
| `SlotModuleRequest` / 204 | 0x1864ff8 | empty 0x539e10; populated 0x539f60 | `+0x28/+0x2c`: 64-bit caller value; `+0x30/+0x34/+0x38`: vector begin/end/capacity of 8-byte `(u32,u32)` records | 0x4d6424 in 0x4d62b0: only sends after validating the requested entries against equipment state; copies the caller's pair vector |
| `UnslotAllModulesRequest` / 205 | 0x1864fb8 | empty 0x53a020; populated 0x53a130 | `+0x28/+0x2c`: 64-bit caller value | 0x4d6669 in 0x4d6520: only sends when the entity has nonzero module entries |
| `SlotVisualRequest` / 207 | 0x1864fa8 | empty 0x53a3d0; populated 0x53a540 | `+0x28`, `+0x2c`: u32; `+0x30`, `+0x31`: byte; `+0x34`, `+0x38`: u32; `+0x3c/+0x40/+0x44`: vector begin/end/capacity of u32 | 0x4d6945 in 0x4d6850: checks a selected index against 32-byte records before constructing, passes a struct whose first DWORD becomes `+0x38` and whose subsequent three DWORDs are a vector header |

The base initializers at 0x5a7510 / 0x5a7580 / 0x5a7660 receive native IDs
204 / 205 / 207 respectively. Both pair-vector construction at 0x60c0e0 (element copy
loop 0x60c000) and u32-vector construction at 0x478e20 allocate/copy their source
ranges; they are not merely pointer assignments. All three populated constructors
receive a target built via 0x3264c0 with discriminator 2 from the local character
(`this+4`, fields `+8/+12` at their respective callers). The target-check helper
0x3265a0 and low-byte range test `[1,14]` report invalid values; no exception or
construction abort has been observed. Empty variants initialize payload fields to
zero and log invalid targets at source lines 5528 / 5556 / 5621; populated variants
log at 5536 / 5563 / 5633.

**Uncertainty:** Neither the semantic names of the six visual scalars nor the
meanings of the two pair-element components have been established; the 64-bit
values may be entity/item IDs but are intentionally labeled `Identifier` in C#.
These are constructor/object-layout models, **not** wire encoders. The three
vtable slots are generic methods and do not by themselves establish packet order.

### SlotVisualMultiRequest (208)

MSVC RTTI `SlotVisualMultiRequest@Commands@Character@firefall` (type descriptor
raw 0x1A084B8; name at +8), catalog COL record raw 0x1864F84, actual vtable
raw **0x1864F88** (VA 0x1C66388). Searching the four-byte vtable VA in code
found exactly two writes, raw 0x53A673 and 0x53A7B9. They belong to the
empty constructor **0x53A620** and populated constructor **0x53A760**;
both call base initializer 0x5A76D0 with message ID 208. The populated
constructor has a real caller at **0x4D6AE3** in **0x4D69D0**, which checks
a local controller and matches the request's selector against 32-byte
selection-table records (kind 0 selects first ID, kind 1 second ID). It
creates the request on the stack, dispatches it via 0x4F3500, then destroys
it at 0x4D6B08. A higher-level builder at 0x9FF2B0 supplies the vectors.

| Object offset | Native shape | Evidence |
|---|---|---|
| +0x18 | inherited 64-bit target/context | 0x4D6ABE builds it with 0x3264C0, discriminator 2 |
| +0x28/+0x2C | u32 selector / selector kind | 0x53A7B1, 0x53A7BD; the call-site handles kind 0/1 |
| +0x30..+0x20B | inline-capacity vector of 28-byte visual descriptors | 0x53A7C0 copies via 0x60BC70; initialized by 0x5A2890; 16 inline records at +0x4C (448 bytes). Vector/allocator metadata at +0x30..+0x4B |
| +0x20C..+0x217 | `{begin,end,capacity}` vector of 28-byte named descriptors | 0x53A7D2 copies via 0x5A7760 / 0x5B1AE0; two shared-text refs per element retained by 0x5F7E10 |

The complete native object spans **0x218 bytes**. The visual element
contains `{u32 at +0, byte at +4, u32 at +8, u32 at +12,
vector<u32> at +16}` (0x1C bytes). The named element contains
`{u32, text ref, u32, u32, text ref, u32, u32}` (also 0x1C bytes).
The meaning and source-level names of these descriptor fields remain unknown.

The **virtual payload serializer 0x5B8340** writes u32 selector, u32 kind,
a 255-chunked visual vector (0x5104B0), then a 255-chunked named vector
(0x510770). Each visual element writes `u32, byte, u32, u32,
chunked vector<u32>` (nested writer 0x523930). Each named element writes
`u32, NUL-terminated raw text bytes, u32, u32, NUL-terminated raw text
bytes, u32, u32`. `SlotVisualMultiRequest.WritePayload` implements this
verified order; it deliberately does **not** claim the outer send framing
or an encoding for the native shared text. The C# model accepts the text's
already-encoded bytes and rejects embedded NULs. The native empty ctor
zeroes both selectors and vectors; the populated ctor deep-copies both
vectors. The native destructor at 0x4D6B20 releases nested allocations
and retained text, including external storage if the visual list outgrows
its 16-element inline capacity.

### RepairRequest (203)

RTTI catalog record raw **0x1864FE4**, actual vtable raw **0x1864FE8**
(VA 0x1C663E8). A code-byte search for that VA found constructors
**0x539C10** (empty; writes zeros at +0x28/+0x2C/+0x30, target log line
5500) and **0x539D60** (populated; target log line 5507), both of which
pass message ID 203 to base initializer 0x5A74A0. The populated
constructor uses 0x3576A0 / 0x3575A0 to deep-copy a caller vector of
eight-byte elements into the 12-byte `{begin,end,capacity}` header at
object `+0x28`. The virtual serializer at **0x5B87C0** calls
**0x3430C0** to write this as 255-chunked *little-endian u64* elements
(byte count then elements, including a final zero count if exactly
255). `RepairRequest.WritePayload` models that payload, **not** transport
framing or an inferred full message body.

The real sender **0x4D60A0** (called at 0xA0C6EB from a higher-level
input wrapper) checks for a controller, rejects an empty source vector
and a count exceeding **100**, and looks up/validates each requested
eight-byte value against local state. It builds the inherited target
from the local character via 0x3264C0 with discriminator 2, calls the
populated constructor at **0x4D61EA**, dispatches at **0x4D6201**, and
returns 1 when the request is sent; otherwise 0. The C# model enforces
this *sender* count constraint; the native serializer itself can
handle longer vectors. The exact kind of repairable item represented by
each identifier remains unknown.

### SalvageRequest (202)

RTTI catalog record raw **0x1864FD4**, actual vtable raw **0x1864FD8**
(VA 0x1C663D8). The constructors **0x539A10** (empty) and **0x539B60**
(populated) both pass native message ID 202 to base initializer
0x5A7430; invalid-target diagnostic lines are 5473/5480. The empty
variant zeros the three DWORDs at `+0x28/+0x2C/+0x30`. The populated
variant deep-copies a vector of 16-byte records into that
`{begin,end,capacity}` header using 0x5A2550; a real caller is
**0x4D5FFC** in sender **0x4D5EA0**. The record's native format is
`{u64 at +0, u32 at +8, u32 at +12}`; field meanings are not known.
The virtual serializer **0x5B8860** forwards this vector to **0x510FA0**,
which writes 255-chunked byte counts and each record's three fields in
little-endian order. The concrete writer **0x500460** calls **0x508CA0**
with opcode 0xCA; the latter writes the inherited u64 target, MessageID
0xCA, then that vector payload before finalizing. `SalvageRequest.EncodeBody`
reproduces precisely this order, excluding connection framing.

The real sender rejects empty inputs and counts above **200** at
0x4D5EFB. It checks each record through 0x133B1C0 and, for certain
non-sentinel identifiers, a local state lookup and status-bit guard
at 0x4D5FAC before constructing the request. It derives the target
via 0x3264C0 using discriminator 2 and dispatches at **0x4D6015**.
The C# model enforces the sender's 1..200 count but does not reproduce
those local-state checks. Nothing in this trace identifies the business
meaning of the record's two u32 fields or the precise target typedef.

### RequestTeleport (210)

RTTI catalog record raw **0x1864F54**, actual vtable raw **0x1864F58**
(VA 0x1C66358). Raw vtable-VA byte search found constructor sites
**0x53A97A** (empty/debug variant at function 0x53A920) and **0x53AA6D**
(populated variant at 0x53AA20). Both invoke base initializer 0x5A7880
with message ID 210. The populated constructor copies two caller DWORDs
to object `+0x28/+0x2C`, i.e. an opaque 64-bit value; target validation
is the common `[1,14]` kind check, source lines 5705/5712.

Real caller **0x4D5CF0** invokes the populated constructor at 0x4D5D3E
using a two-DWORD value from the caller and a target produced by
0x3264C0 with discriminator 2 from the local character, then dispatches
through 0x4F31A0. The caller function is directly reached from
0xA0C6EB. No class-specific serializer/body writer was established in
this pass, so `RequestTeleport` is deliberately only a native-layout
record in C#; the `Value` meaning and wire payload format are unknown.

### RequestRespawn (message metadata ID 198)

RTTI catalog record raw **0x1865034**, actual vtable raw **0x1865038**
(VA 0x1C66438). Vtable-VA byte search produced one constructor write at
**0x5393EB**, inside **0x5393A0**. That constructor invokes base
initializer **0x5A7270 with ID 198**, then installs the vtable and
performs the common kind validation `[1,14]` (diagnostic line 5364).
It writes no subclass payload fields, so this is a kind-only command.

A real construction path is **0x4CB990**, with the constructor call at
**0x4CBA6D** and dispatch at 0x4CBA7E. It requires the flag
`(value & 0x10000000) != 0`, checks timing/order conditions, and enforces
at least **0xBB8 ms** since its timestamp at `this+249` before queueing
the command. Exact gameplay meaning of those gates and the constructor
argument remains unknown. The concrete body path is **0x4F2E80 →
0x4F9760 → 0x5062D0 → 0x50BA00**; `0x5062D0` passes ID `0xC6`,
the inherited 64-bit handle at `+0x18`, and the packer to `0x50BA00`.
That writer emits the 8-byte little-endian handle followed by the one-byte
ID and finalizes, for exactly **9 body bytes** (before transport framing).
`RequestRespawn.EncodeBody()` models this verified body.

The higher-level tick at **0x4B71F0** adds a 500 ms gate before calling
0x4CB990. The latter additionally requires flag `0x10000000`, checks
time/order conditions, and enforces a **3000 ms** cooldown via
`this+0x3E4` before updating its timestamp and queueing the request.
The handle is constructed from the owning entity's identifier and
retagged with kind 2. Since the real caller always supplies that valid
kind, the constructor's `[1,14]` diagnostic path is not expected on this
route. Meanings of the gameplay gates and exact typedef remain unknown.

### DuelRequest (188)

RTTI catalog record raw **0x18650C4**, actual vtable raw **0x18650C8**
(VA 0x1C664C8). Manual vtable-VA search found ctor writes in **0x537B40**
(empty/debug) and **0x537CF0** (populated); both initialize common message
metadata ID 188 through **0x5A6DA0**. The empty form initializes the
descriptor at object `+0x28` using **0x80ED00**, sets mode byte `+0x80`
to zero, and logs invalid target at source line 5082. The populated form
deep-copies the descriptor from the caller with **0x537DA0**, stores its
mode at `+0x80`, and logs invalid target at line 5090.

The copied descriptor occupies **0x58 bytes** beginning at `+0x28`:
one leading byte at relative `+0`, three shared-text references at
relative `+4`, `+0x10`, and `+0x20`; u32 fields at `+8`, `+0xC`,
`+0x18`, and `+0x1C`; two vectors of 8-byte `(u32,u32)` entries at
relative `+0x28` and `+0x34` (each a 12-byte `{begin,end,capacity}`
header); then u16 fields `+0x40/+0x42`, u32 `+0x44`, byte `+0x48`,
and u32 values `+0x50/+0x54`. Relative `+0x14` and `+0x24` are not
copied/initialized by the observed helpers. The mode byte is separate
at object `+0x80`.

Two real callers in the chat-related path construct this command:
**0x8FB840** calls the populated ctor with mode `4`; **0x8FBA70**
uses mode `3` or `5` depending on its boolean argument. The latter
checks the destination peer identity against local state before sending.
Both dispatch through **0x902990** when an observer connection exists.
These call paths are evidence of reachability, but do not establish the
exact meaning of the `DuelRequest` name, descriptor fields, shared-text
encoding, or the class-specific wire order; the vtable slot addresses
are not reliable serializer references in this raw-mapped database.
`DuelRequest.cs` is consequently a conservative object-layout model
only, and does not invent a packet encoder.

### VehicleCalldownRequest (179)

RTTI catalog record raw **0x1865164**, actual vtable raw **0x1865168**
(VA 0x1C66568). Its empty/debug constructor is **0x536B10** and its
populated constructor is **0x536C40**; both call common initializer
**0x5A69B0** with metadata ID **179**. The empty constructor zeros the
u16 at `+0x28` and initializes the two bytes at `+0x88/+0x89` to 1.
The populated constructor copies its u16 selector to `+0x28`, three u32
values to `+0x2C/+0x30/+0x34`, 0x40 opaque bytes to `+0x38..+0x77`,
four u32 values to `+0x78..+0x87`, and two bytes to `+0x88/+0x89`.
The gap at `+0x2A` is not written by these constructors. The last two
input bytes are read from descriptor offsets `+0x50/+0x51`; bytes beyond
that observed range are not modeled. These positions and field widths
are known, but their domain meanings are not.

Two populated-constructor callers were found: **0x953630** checks a
state byte at caller `+0x84` before constructing and dispatching the
request; **0xAB0940** conditionally constructs it after matching a pair
of values against current state and a successful helper lookup. Both
pass through **0x957D30**, a shared observer-send gate. These call paths
establish reachability, not the meaning of the request fields. No
class-specific body writer or wire order was established, so
`VehicleCalldownRequest.cs` is intentionally a native-layout model and
does not provide an encoder.

### DeployableCalldownRequest (180)

RTTI catalog record raw **0x1865134**, actual vtable raw **0x1865138**
(VA 0x1C66538). Its empty/debug constructor is **0x536D40** and its
populated constructor is **0x536E70**; both call common initializer
**0x5A6A20** with metadata ID **180**. The empty constructor zeros the
u32 at `+0x28` and initializes the two bytes at `+0x88/+0x89` to 1.
The populated constructor stores a u32 at `+0x28`, three u32 values at
`+0x2C/+0x30/+0x34`, copies 0x40 opaque bytes to `+0x38..+0x77`, stores
four u32 values at `+0x78..+0x87`, and stores two bytes at `+0x88/+0x89`.
The field widths and offsets match `VehicleCalldownRequest`, except that
this class has a u32 at `+0x28` where Vehicle's field is a u16 (leaving
`+0x2A` unwritten).

The populated constructor has one discovered caller, **0x951A20**. It
checks a state byte at caller `+0x84`, creates the request, then dispatches
it through **0x957AF0** when an observer connection is available. This
establishes reachability only; the positional fields' meanings and any
class-specific wire order remain unknown. `DeployableCalldownRequest.cs`
therefore preserves the native layout and copied opaque bytes without
inventing a packet encoder.

### ClearPreviousBounties (162)

RTTI catalog record raw **0x1865254**, actual vtable raw **0x1865258**
(VA 0x1C66658). Empty/debug constructor **0x534E00** and populated
constructor **0x534F20** initialize metadata ID **162** through
**0x5A6230**. The empty form zeros the subclass DWORD at `+0x28`; the
populated form copies its caller-supplied DWORD there. Both validate the
inherited handle kind against `[1,14]`.

The populated constructor is called by **0x984B60**, which takes a DWORD
argument, builds a handle from the current entity identifier with its
low-byte kind set to **4**, constructs the request, and routes it through
observer-send gate **0x9889B0** to **0x98A480**. `ClearPreviousBounties.cs`
preserves the DWORD positionally; its domain meaning and class-specific
wire order are not established.

### RequestRerollBounties (164)

RTTI catalog record raw **0x1865244**, actual vtable raw **0x1865248**
(VA 0x1C66648). Empty/debug constructor **0x5351E0** and populated
constructor **0x535370** initialize metadata ID **164** through
**0x5A6310**. The empty form zeros the three-DWORD vector header at
`+0x28`; the populated form deep-copies a vector of DWORDs into that
header using the common vector-copy helper **0x47B630**.

The one discovered populated-constructor caller is **0x984230**. It
places its DWORD argument in a one-element vector, builds a handle from
the current entity identifier with kind **4**, constructs the request,
then uses observer-send gate **0x988860** / dispatcher **0x98A0E0**.
`RequestRerollBounties.cs` models the copied vector. The elements'
business meaning and class-specific wire format remain unknown.

### MovementInput (115)

RTTI catalog record raw **0x1865554**, actual vtable raw **0x1865558**
(VA 0x1C66958). Empty/debug constructor **0x52F0A0** and populated
constructor **0x52F1E0** initialize metadata ID **115** through
**0x5A4C80**. The empty form zeros a u16 at `+0x28`, initializes
`+0x94` to `0x0101`, zeros the shared-reference slot `+0xBC`, and clears
selected trailing fields. The populated form writes u16 `+0x28`,
unaligned u32 values `+0x2A/+0x2E`, copies a complex descriptor to
`+0x34`, then writes a u16 at `+0xC0`, a u16 at `+0xC4`, and a byte at
`+0xC6`. The copy helper **0x51E840** copies 0x88 fixed bytes and manages
an embedded reference at descriptor-relative `+0x88`; that reference is
process-local and intentionally excluded from the C# snapshot model.

The populated constructor is called from **0x4C03C0** and the request is
passed to observer helper **0x4F9510**. The caller builds a descriptor
from existing movement/controller state; the individual fields' meanings
and class-specific wire order remain unknown. `MovementInput.cs` captures
the known fixed prefix and trailing values without claiming to reproduce
the omitted reference or a network body.

### MovementInputFake (116)

RTTI catalog record raw **0x1865544**, actual vtable raw **0x1865548**
(VA 0x1C66948). Empty/debug constructor **0x52F2F0** and populated
constructor **0x52F480** initialize metadata ID **116** through
**0x5A4CF0**. The known scalar positions are u16 `+0x28`, u32 values
`+0x2A/+0x2E/+0x32`, u16 `+0x36`, byte `+0x38`, and optional-presence
byte `+0x3C`. When the caller's optional descriptor is present, the
constructor copies its fixed 0x88-byte prefix to `+0x40`, manages an
embedded process-local reference at `+0xC8`, and writes a u16 at `+0xCC`.
It also writes a u16 at `+0xD0` and byte at `+0xD2`. No populated caller
was found; the descriptor and scalar semantics remain unknown.

`MovementInputFake.cs` models the fixed fields and optional fixed-byte
prefix. It deliberately does not serialize the embedded reference or
infer a class-specific packet format.

### FireWeaponProjectile (121)

RTTI catalog record raw **0x18654F4**, actual vtable raw **0x18654F8**
(VA 0x1C668F8). Empty/debug constructor **0x52FD10** and populated
constructor **0x52FE60** initialize metadata ID **121** through
**0x5A4FF0**. The populated constructor stores u32 values at
`+0x28/+0x2C/+0x30/+0x34`, sets presence byte `+0x38`, and conditionally
copies three more u32 values to `+0x3C/+0x40/+0x44`. The empty form zeros
the first value and optional-presence byte.

The populated constructor is used by the large state/weapon routine
**0x8858E0** at **0x885D95**. The surrounding path performs entity-state,
position, and weapon-related checks, but the meanings of the positional
values and exact wire order remain unverified. `FireWeaponProjectile.cs`
models the constructor fields only.

### SelectWeapon (125)

RTTI catalog record raw **0x18654A4**, actual vtable raw **0x18654A8**
(VA 0x1C668A8). The empty/debug constructor **0x530520** and populated
constructor **0x530630** initialize metadata ID **125**. The populated
form stores a u32 at `+0x28` and bytes at `+0x2C/+0x2D`; the empty form
sets the u32 to zero and initializes the following u16 to `0x0100`.

The populated constructor is called from **0x4AB0C0**, a control path
that updates local state and conditionally sends the request through
observer helper **0x4F8C00**. The positional field meanings and wire
representation are not established.

### ReloadWeapon (126)

RTTI catalog record raw **0x18654B4**, actual vtable raw **0x18654B8**
(VA 0x1C668B8). The empty/debug constructor **0x5306F0** and populated
constructor **0x530810** initialize metadata ID **126**. The populated
form stores one u32 at `+0x28`; the empty form sets it to zero.

The constructor is called at **0x8850D9** from **0x885080** when the
associated component reports ready. The caller then submits it through
observer helper **0x888B20**. The field meaning and wire format remain
unknown.

### AcquireWeaponTarget (128)

RTTI catalog record raw **0x1865494**, actual vtable raw **0x1865498**
(VA 0x1C66898). Empty/debug constructor **0x530A90** and populated
constructor **0x530BB0** initialize metadata ID **128**. The populated
constructor writes a u32 at `+0x28`, u32 values at `+0x30/+0x34/+0x38`,
and a byte at `+0x3C`. The empty form zeros those locations. The
intervening `+0x2C` bytes are not assigned by these constructors.

The populated constructor has two discovered paths: **0x43BE50** sends
through **0x44F7F0** after an interface readiness check, and **0x887BC0**
also references it in a larger dispatch routine. The caller provides the
`+0x38` value via an XMM register. No field semantics or wire ordering
are claimed.

### LoseWeaponTarget (129)

RTTI catalog record raw **0x1865464**, actual vtable raw **0x1865468**
(VA 0x1C66868). Empty/debug constructor **0x530C80** and populated
constructor **0x530D90** initialize metadata ID **129**. The populated
form stores u32 values at `+0x28/+0x30/+0x34`; the empty form zeros them.
The bytes at `+0x2C` are not assigned by these constructors.

The populated constructor is called from both **0x43BF50** and the
larger routine **0x886AF0**, then dispatched through observer helper
**0x44F860**. Field meanings and wire ordering remain unknown.

### SelectFireMode (123)

RTTI catalog record raw **0x18654C4**, actual vtable raw **0x18654C8**
(VA 0x1C668C8). Empty/debug constructor **0x530160** and populated
constructor **0x530290** initialize metadata ID **123**. The populated
form stores a u32 at `+0x28` and a byte at `+0x2C`; the empty form zeros
both.

The populated constructor is called from **0x8851A0** after the associated
component readiness check. The caller sends through observer helper
**0x888C00**. The fields' meanings and wire representation are unknown.

### UseScope (124)

RTTI catalog record raw **0x18654D4**, actual vtable raw **0x18654D8**
(VA 0x1C668D8). Empty/debug constructor **0x530340** and populated
constructor **0x530470** initialize metadata ID **124**. The populated
form stores a u32 at `+0x28` and a byte at `+0x2C`; the empty form zeros
both.

Two populated paths were found: **0x887140** passes byte value 1 and
**0x887200** passes byte value 0. Both send through observer helper
**0x888CE0**, then continue into a separate state-update path. No field
semantics or class-specific wire order is claimed.

### CancelReload (127)

RTTI catalog record raw **0x1865484**, actual vtable raw **0x1865488**
(VA 0x1C66888). Empty/debug constructor **0x5308C0** and populated
constructor **0x5309E0** initialize metadata ID **127**. The populated
form stores one u32 at `+0x28`; the empty form zeros it.

The populated constructor is called from **0x885110** after a component
readiness check and sent through observer helper **0x888B90**. The value's
meaning and wire format are unverified.

### ReportProjectileHit (122)

RTTI catalog record raw **0x18654E4**, actual vtable raw **0x18654E8**
(VA 0x1C668E8). Empty/debug constructor **0x52FF50** and populated
constructor **0x530080** initialize metadata ID **122**. The populated
form writes an unaligned sequence: u16 `+0x28`, u16 `+0x2A`, byte
`+0x2C`, u16 `+0x2D`, byte `+0x2F`, then u16 values `+0x30/+0x32`.
The empty form explicitly clears the first dword, byte at `+0x2C`, and
u16 at `+0x32`; it does not explicitly initialize every byte in the
populated sequence.

The populated constructor is called from **0x89CAF0** when a mode field
equals 1 or 2; each mode uses a distinct observer dispatch helper. The
class fields' semantic meanings and serialized order remain unknown.

### FireInputIgnored (117)

RTTI catalog record raw **0x1865534**, actual vtable raw **0x1865538**
(VA 0x1C66938). Empty/debug constructor **0x52F5E0** and populated
constructor **0x52F6F0** initialize metadata ID **117**. The populated
form writes a u32 at `+0x28` and byte at `+0x2C`; the empty form zeros
both.

The populated constructor is reached from control paths **0x4B2680**
(twice; byte values 0/1) and **0x4B2AD0** (byte value 1); those paths send
through observer helper **0x4F8DB0**. The positional u32 meaning and wire
format are unknown.

### FireBurst (118)

RTTI catalog record raw **0x1865514**, actual vtable raw **0x1865518**
(VA 0x1C66918). Empty/debug constructor **0x52F7A0** and populated
constructor **0x52F8C0** initialize metadata ID **118**. The populated
form writes one u32 at `+0x28`; the empty form zeros it.

Caller **0x884EA0** checks component readiness, constructs and sends the
request through observer helper **0x8889D0**, then caches the supplied
value at caller-object `+0x1464`. Its gameplay meaning and wire order are
not known.

### FireEnd (119)

RTTI catalog record raw **0x1865524**, actual vtable raw **0x1865528**
(VA 0x1C66928). Empty/debug constructor **0x52F970** and populated
constructor **0x52FA90** initialize metadata ID **119**. The populated
form writes one u32 at `+0x28`; the empty form zeros it.

Caller **0x884F40** checks a component, constructs and sends through
observer helper **0x888A40**, then caches the supplied value at caller
object `+0x1468`. Field semantics and wire order remain unknown.

### FireCancel (120)

RTTI catalog record raw **0x1865504**, actual vtable raw **0x1865508**
(VA 0x1C66908). Empty/debug constructor **0x52FB40** and populated
constructor **0x52FC60** initialize metadata ID **120**. The populated
form writes one u32 at `+0x28`; the empty form zeros it.

Caller **0x884FE0** checks component readiness, constructs and sends
through observer helper **0x888AB0**, then caches the supplied value at
caller-object `+0x1468`. Field semantics and wire order remain unknown.

### DockToPlayer (132)

RTTI catalog record raw **0x1865444**, actual vtable raw **0x1865448**
(VA 0x1C66848). Empty/debug constructor **0x531230** and populated
constructor **0x531360** initialize metadata ID **132**. The populated
form copies five u32s into `+0x28/+0x2C/+0x30/+0x34/+0x38`; the empty
form zeros the first two DWORDs only. No populated-constructor call xref
was found. Field meanings and wire layout remain unknown.

### NPCInteractWithTarget (135)

RTTI catalog record raw **0x1865404**, actual vtable raw **0x1865408**
(VA 0x1C66808). Empty/debug constructor **0x531930** and populated
constructor **0x531A70** initialize metadata ID **135**. The populated
form writes u32s at `+0x28`, `+0x30/+0x34`, and `+0x38/+0x3C`; the
intervening `+0x2C` bytes are not assigned by the constructor. The empty
form zeros all five written fields. No populated-constructor call xref was
found; semantics and wire order remain unknown.

### NPCApplyEffect (130)

RTTI catalog record raw **0x1865474**, actual vtable raw **0x1865478**
(VA 0x1C66878). Empty/debug constructor **0x530E50** and populated
constructor **0x530F80** initialize metadata ID **130**. The populated
form stores a u32 at `+0x28` and two more at `+0x30/+0x34`; the empty form
zeros those fields. The intervening bytes at `+0x2C` are not written by
these constructors. No populated-constructor code references were found
by the IDA xref query; the serialized semantics remain unknown.

### NPCRemoveEffect (131)

RTTI catalog record raw **0x1865454**, actual vtable raw **0x1865458**
(VA 0x1C66858). Empty/debug constructor **0x531050** and populated
constructor **0x531170** initialize metadata ID **131**. The populated
form stores one u32 at `+0x28`; the empty form zeros it. The constructor
has no populated-call xrefs in IDA; no field meaning or wire order is
claimed.

### ChangeLookAtTarget (133)

RTTI catalog record raw **0x1865424**, actual vtable raw **0x1865428**
(VA 0x1C66828). Empty/debug constructor **0x531440** and populated
constructor **0x531590** initialize metadata ID **133**. The optional
presence byte is at `+0x28`. When the caller-supplied descriptor's first
byte is nonzero, the constructor sets that flag and copies six u32 values
from descriptor offsets `+0x08..+0x1C` to object offsets
`+0x30..+0x44`; otherwise it leaves the optional group absent. The gap
between the flag and group is not interpreted. No populated-constructor
xrefs were found; the values' meanings and packet format remain unknown.

### TargetAbility (136)

RTTI catalog record raw **0x1865414**, actual vtable raw **0x1865418**
(VA 0x1C66818). Empty/debug constructor **0x531B50** and populated
constructor **0x531C70** initialize metadata ID **136**. The populated
form stores a byte at `+0x28`; the empty form zeros it. No populated
constructor xrefs were found, so the field meaning and wire format are
unknown.

### DeactivateAbility (137)

RTTI catalog record raw **0x18653F4**, actual vtable raw **0x18653F8**
(VA 0x1C667F8). Empty/debug constructor **0x531D30** and populated
constructor **0x531E60** initialize metadata ID **137**. The populated
form writes a u32 at `+0x28` and byte at `+0x2C`; the empty form zeros
both. A construction path is present at **0xAA9280**, guarded by several
state and identity checks, and then forwarded through helper **0xAB1030**.
Field meanings and class-specific wire order remain unknown.

### CallForHelp (151)

RTTI catalog record raw **0x1865334**, actual vtable raw **0x1865338**
(VA 0x1C66738). Empty/debug constructor **0x533960** and populated
constructor **0x533A60** initialize metadata ID **151**. The populated
form stores one u32 at `+0x28`; the empty form zeros it.

The populated constructor is called by **0x4AF640**, which is reached from
**0x4B8100**. That caller applies state/timing gates, supplies a current
timer value as the positional field, derives a kind-4 target, and dispatches
through **0x4F8D40**. No class-specific wire order is known.

### AbortCampaignMission (152)

RTTI catalog record raw **0x1865304**, actual vtable raw **0x1865308**
(VA 0x1C66708). Empty/debug constructor **0x533B10** and populated
constructor **0x533C30** initialize metadata ID **152**. The populated
form stores one u32 at `+0x28`; the empty form initializes it to zero.
Caller **0x908E70** passes its input unchanged, derives a kind-4 target,
and sends through **0x90CB50** when a remote observer view is available.
The value's meaning and wire order are unknown.

### AnimationUpdate (149)

RTTI catalog record raw **0x1865314**, actual vtable raw **0x1865318**
(VA 0x1C66718). Empty/debug constructor **0x5335A0** and populated
constructor **0x5336D0** initialize metadata ID **149**. The populated
form stores u32 values at `+0x28/+0x2C` and a byte at `+0x30`; the empty
form zeros all three fields. No populated-constructor caller was found.
Field meanings and wire order are unknown.

### TryResumeTutorialChain (153)

RTTI catalog record raw **0x18652F4**, actual vtable raw **0x18652F8**
(VA 0x1C666F8). The only discovered constructor is **0x533CE0**; it
initializes metadata ID **153**, installs the vtable, and has no subclass
payload fields. Caller **0x8EE120** constructs it behind an elapsed-time
gate, derives a kind-4 target, and dispatches via **0x8F0260** when a remote
observer view is available. No class-specific payload writer was identified.

### ListAvailableBounties (160)

RTTI catalog record raw **0x1865284**, actual vtable raw **0x1865288**
(VA 0x1C66688). Empty/debug constructor **0x534A10** and populated
constructor **0x534B40** initialize metadata ID **160**. The populated
form copies two u32 values to `+0x28/+0x2C`; the empty form zeros both.
Caller **0x984CE0** passes its two input values through unchanged, derives
a kind-4 target, and dispatches through **0x988B00**. Field meanings and
wire order remain unknown.

### MatchAccept (93)

RTTI catalog record raw **0x18656B4**, actual vtable raw **0x18656B8**
(VA 0x1C66AB8). Empty/debug constructor **0x52C490** and populated
constructor **0x52C5D0** initialize metadata ID **93**. The populated
form receives its inherited target at `+0x18`, stores a u32 at `+0x28`,
two u32s at `+0x30/+0x34`, and a byte at `+0x38`; the empty form zeros
these payload fields. No populated-constructor caller was resolved, so
field semantics and wire ordering remain unknown.

### PlayerReady (88)

RTTI catalog record raw **0x18656F4**, actual vtable raw **0x18656F8**
(VA 0x1C66AF8). Empty/debug constructor **0x52B8C0** and populated
constructor **0x52BA00** initialize metadata ID **88**. The populated form
stores bytes at `+0x28/+0x29`, a presence byte at `+0x2C`, and, when present,
copies a vector of 8-byte entries to `+0x30`. The copy helper
`CopyRepairIdentifierVector` deep-copies the two DWORDs of each entry without
retaining pointers; C# preserves them as opaque u64 values. No populated
caller was resolved, so field semantics and wire ordering remain unknown.

### ToggleMarketplace (86)

RTTI catalog record raw **0x1865714**, actual vtable raw **0x1865718**
(VA 0x1C66B18). Empty/debug constructor **0x52B5E0** and populated
constructor **0x52B700** initialize metadata ID **86**. The populated
form stores a byte at `+0x28`; the empty form sets it to 1. No populated
caller was resolved, so the byte's meaning and wire layout remain unknown.

### FillBuyOrder (85)

RTTI catalog record raw **0x1865734**, actual vtable raw **0x1865738**
(VA 0x1C66B38). Empty/debug constructor **0x52B3B0** and populated
constructor **0x52B500** initialize metadata ID **85**. The populated form
receives its inherited target at `+0x18` and writes eight positional u32s
at `+0x28/+0x2C`, `+0x30/+0x34`, `+0x38/+0x3C`, and `+0x40/+0x44`.
Caller **0xA2F440** constructs it at **0xA2F5B3** and sends through
**0xA42140** when the remote observer view is available. No field meanings
or class-specific wire order are inferred.

### MatchmakerSetPenalties (92)

RTTI catalog record raw **0x18656D4**, actual vtable raw **0x18656D8**
(VA 0x1C66AD8). Empty/debug constructor **0x52C280** and populated
constructor **0x52C3B0** initialize metadata ID **92**. The populated form
receives its inherited target at `+0x18` and stores a u32 at `+0x28`, byte
at `+0x2C`, u32 at `+0x30`, and byte at `+0x34`; the empty form clears
these fields. No populated-constructor caller was resolved, so field
meanings and wire order remain unknown.

### JoinGroupLeader (87)

RTTI catalog record raw **0x1865724**, actual vtable raw **0x1865728**
(VA 0x1C66B28). The only observed constructor, **0x52B7B0**, initializes
metadata ID **87** and has no subclass payload fields; the inherited target
is passed through the base initializer at `+0x18`. No populated-constructor
caller or class-specific wire writer was found.

### FetchQueueInfo (89)

RTTI catalog record raw **0x1865704**, actual vtable raw **0x1865708**
(VA 0x1C66B08). Empty/debug constructor **0x52BAF0** and populated
constructor **0x52BCC0** initialize metadata ID **89**. The populated form
copies a vector of 4-byte references at `+0x28`, retaining each reference,
then writes u32s at `+0x38/+0x3C/+0x40`. The vector's reference semantics
are preserved as opaque u32 values in C#; no populated caller, semantic
field names, or wire layout was established.

### ActivateAbility (134)

RTTI catalog record raw **0x1865434**, actual vtable raw **0x1865438**
(VA 0x1C66838). Empty/debug constructor **0x531690** initializes metadata
ID **134**, zeros a u32 at `+0x28` and byte at `+0x2C`, and initializes an
inline-capacity vector of 8-byte records beginning at `+0x30`; the observed
helper call requests capacity for four entries with inline storage at
`+0x4C`. It also zeros a byte at `+0x78`. A second constructor starts at
**0x531840**; IDA currently classifies part of its body as a jump table, but
raw disassembly reaches a `retn 0x1C`, and it writes the same vtable. It
stores a u32 at `+0x28`, a byte at `+0x2C`, copies the vector at `+0x30`,
copies three positional u32s to `+0x6C/+0x70/+0x74`, and stores a byte at
`+0x78`. No direct caller of this populated constructor was found.

The real build/send path is **0xAB0C80** (caller **0xAA8FC0**) and uses the
empty/debug constructor. It writes `+0x28` from an ability object field,
`+0x2C` from the slot index, conditionally constructs the inline vector only
for slot 4, and sets `+0x78` from a global flag. This path leaves
`+0x6C..+0x74` untouched even though the class writer **0x5BEEA0** serializes
them after the vector. Their runtime contents are therefore unknown and may
be uninitialized; the C# model represents them as nullable DWORDs. The
vector-entry meanings and byte semantics are unresolved, and no safe packet
encoder is inferred from this path.

### NPCSetInteractionType (141)

RTTI catalog record raw **0x1865384**, actual vtable raw **0x1865388**
(VA 0x1C66788). Empty/debug constructor **0x5323D0** and populated
constructor **0x532530** initialize metadata ID **141**. The populated
form stores a byte at `+0x28` and copies an array of 8-byte records into a
vector beginning at `+0x30`, with inline storage at `+0x40`. The current
constructor search found no direct populated-constructor caller xref.
Whether those records are bitset blocks or another kind of u64 element is
not established. `NpcSetInteractionType` snapshots the entries as opaque
u64 values and does not claim a wire layout.

### ClientUIEvent (196)

RTTI record raw **0x1865054**, actual vtable raw **0x1865058** (VA
**0x1C66458**). The empty/debug constructor **0x538F60** clears the
shared-text field at `+0x28`; populated constructor **0x539110** retains a
shared-text reference there. A caller at **0xA1D940** extracts a C-string
from parsed input and constructs the command, then attempts submission only
when `GetRemoteObserverViewSelectedField` returns a view. The send helper
also checks connection/state fields before dispatch. Text meaning and wire
encoding remain unknown; the C# snapshot keeps the process-local pointer
opaque.

### SetDialogTag (147)

RTTI catalog record raw **0x1865344**, actual vtable raw **0x1865348**
(VA 0x1C66748). Empty/debug constructor **0x533130** and populated
constructor **0x5332F0** initialize metadata ID **147**. The populated
form stores a shared-text reference at `+0x28`, a u32 at `+0x2C`, and
bytes at `+0x30/+0x31`. It retains the text reference and increments its
native reference count; the empty form clears that reference and zeros
the other fields. No populated-constructor caller or class-specific writer
was found. The text encoding, other values' meanings, and wire layout are
unknown; the C# snapshot keeps the shared-text pointer opaque.

### VendorPurchaseRequest (238)

RTTI record raw **0x1864DA4**, actual vtable raw **0x1864DA8** (VA
**0x1C661A8**). The populated constructor **0x53DCA0** copies four
positional 8-byte pairs into `+0x28/+0x30/+0x38/+0x40`, then copies three
presence-flagged fields beginning at `+0x48`. Those optionals contain
DWORDs at `+0x4C` and `+0x54`, then a retained shared-text pointer at
`+0x5C`; the default constructor **0x53DA50** clears the three flags.
Writer **0x5B5240** emits the four pairs in order, then delegates the
optionals to **0x5B4F10**. The first two optional values are written as
DWORDs; the third, when present, is written as null-terminated text rather
than as the process pointer. No gameplay meanings for these fields are
established.

The sole found populated-constructor caller **0x9916B0** submits only after
two validations and when a remote observer view exists. Its caller
**0xA180D0** builds the four pairs and marks the two optional DWORDs
present while leaving the shared-text optional absent. The model preserves
the positional pairs and native optional-presence state; it does not claim
a safe wire encoder because the text pointer is process-local.

### VendorTokenMachineRequest (200)

RTTI record raw **0x1865014**, actual vtable raw **0x1865018** (VA
**0x1C66418**). Empty constructor **0x539670** zeros the word at `+0x28`
and DWORDs at `+0x2C/+0x30`; populated constructor **0x539780** stores a
mode byte at `+0x28`, zero at `+0x29`, and the two DWORDs. Two callers,
**0x4D5AB0** and **0x4D5BE0**, construct modes **0** and **3** respectively
when the associated component has a nonzero object at `+0x174` whose byte
at `+0x40` is 12. Both take `+0x2C` from that object's `+0x44` and the final
DWORD from parsed input (integer-tagged values are used directly; other
values go through a conversion helper). Each then submits via
`GetRemoteObserverViewSelectedField` and helper **0x4F3130**. The two modes'
gameplay meanings and wire layout remain unknown.

### FindServiceProvider (195)

RTTI record raw **0x1865044**, actual vtable raw **0x1865048** (VA
**0x1C66448**). Empty constructor **0x538D90** clears the byte at `+0x28`;
populated constructor **0x538EB0** copies a caller-supplied byte there. The
send helper **0x909980** constructs the command using the active target and
attempts submission when `GetRemoteObserverViewSelectedField` returns a
view; its send path also requires a nonzero connection field and state above
3. Callers **0x9092A0** and **0xA19200** pass values derived from their
respective parser/selection paths; the first explicitly sends value 1 after
a successful local branch. The byte's meaning and wire layout are unknown.

### UpdateFriendStatus (249)

RTTI record raw **0x1864CF4**, actual vtable raw **0x1864CF8** (VA
**0x1C660F8**). Empty constructor **0x53EFC0** clears a shared-text
reference at `+0x28`, a byte at `+0x2C`, and the optional presence byte at
`+0x30`. Populated constructor **0x53F190** retains a required shared-text
reference at `+0x28`, copies the byte at `+0x2C`, and copies a
presence-flagged shared-text reference at `+0x30/+0x34`, retaining its
pointer when present.

Two state-change paths, **0x6EA1B0** and **0x6EA380**, are timer-gated,
send byte values **1** and **0** respectively, and leave the optional text
absent. A parsed-input path at **0x6ED5B0** creates shared-text values from
two strings, sends byte **0**, and supplies the optional shared-text
descriptor. All three submit only when a remote observer view exists; the
send helper also requires its connection field and state checks to pass.
The strings' meanings, byte semantics, and wire layout are unknown.

### FriendsListRequest (248)

RTTI record raw **0x1864D04**, actual vtable raw **0x1864D08** (VA
**0x1C66108**). Empty constructor **0x53EDF0** clears the DWORD at `+0x28`;
populated constructor **0x53EF10** copies a DWORD there. The caller
**0x6ED8E0** parses an integer-like input, subtracts one, and rejects a
negative result before constructing the message. It also requires an active
controller/context, then submits only if a remote observer view is available.
The field's meaning and wire layout remain unknown.

### ClientQueryInteractionStatus (186)

RTTI record raw **0x18650F4**, actual vtable raw **0x18650F8** (VA
**0x1C664F8**). Empty constructor **0x537860** clears the 8-byte field at
`+0x28` and the byte at `+0x30`; populated constructor **0x537970** copies
the 8-byte field and byte. Its only found caller, **0x4ABB80**, supplies the
pair from a parsed input buffer and derives the byte from whether a related
input flag is nonzero. The active target is copied from the current
controller, and submission is attempted only when a remote observer view
exists. Field meanings and wire layout are unknown.

### ClaimBountyRewards (250)

RTTI record raw **0x1864CD4**, actual vtable raw **0x1864CD8** (VA
**0x1C660D8**). The only found constructor, **0x53F2C0**, initializes
metadata ID **250** and the inherited target, with no subclass fields. Its
only found sender **0x984300** constructs the request from a target argument
and calls the send helper only when a remote observer view exists; that
helper also requires its connection and state fields to pass. Parent path
**0xA2A860** verifies parsed input and active context before calling the
sender. No class-specific wire layout was established.

### LootDistributionSetVotes (219)

RTTI record raw **0x1864ED4**, actual vtable raw **0x1864ED8** (VA
**0x1C662D8**). The empty constructor **0x53B8E0** clears the u32 at `+0x28`
and vector descriptor at `+0x2C`; populated constructor **0x53BA70** copies
the u32 and retains a vector of 8-byte entries at `+0x2C`. The shared
`CopyRepairIdentifierVector` helper confirms that entries are copied as two
DWORDs each; they are modeled as opaque u64s.

Caller **0x6F2C90** converts a parsed numeric value into `+0x28`, builds
unique 8-byte entries from parsed records, derives the active target, and
submits through the remote observer view. The class name suggests a voting
operation, but field and entry semantics plus wire layout remain unknown.

### UiQueryResponse (59)

RTTI record raw **0x1864CE4**, actual vtable raw **0x1864CE8** (VA
**0x1C660E8**). Default constructor **0x53F3D0** initializes the map at
`+0x34`; populated constructor **0x53F590** copies two DWORDs to `+0x28/+0x2C`,
a third DWORD to `+0x30`, and deep-copies the map at `+0x34`.

Caller **0x976E80** builds a numeric-keyed map from matched UI query records
and their supplied DWORD values, then constructs/submits the response via the
remote observer view. The keys and values are retained positionally; their
domain meanings and wire layout are unknown.

### LootDistributionSetState (218)

RTTI record raw **0x1864F04**, actual vtable raw **0x1864F08** (VA
**0x1C66308**). Default constructor **0x53B720** zeros a DWORD at `+0x28` and
byte at `+0x2C`. Populated constructor **0x53B830** copies a DWORD to `+0x28`
and a byte to `+0x2C`.

Caller **0x4CDD50** supplies the DWORD from a record at `+0x28`; its byte
starts at 2 and becomes 1 if a related value appears in a local collection.
Caller **0xA2BBE0** converts parsed text to a DWORD and obtains the byte from
a validation helper before constructing and submitting the message. These
observed values do not establish the protocol meaning of either field; no
wire layout is known.

### RequestTrackerUpdate (217)

RTTI record raw **0x1864EF4**, actual vtable raw **0x1864EF8** (VA
**0x1C662F8**). Default constructor **0x53B560** clears the eight bytes at
`+0x28`; populated constructor **0x53B670** copies two DWORDs to `+0x28` and
`+0x2C`.

Caller **0x4C9940** compares a parsed value at `+0x48` with a field in the
active context. If they differ, it constructs a different command (ID 134);
if equal, a separate request validation path determines whether it dispatches
ID 262 or submits `RequestTrackerUpdate` using DWORDs from `+0x28/+0x2C`.
Those values remain positional and opaque; wire layout is unknown.

### LeaveArc (214)

RTTI record raw **0x1864F14**, actual vtable raw **0x1864F18** (VA
**0x1C66318**). Constructor **0x53AFC0** initializes metadata ID 214 and the
vtable without writing subclass payload fields. Caller **0x927EE0** constructs
the message with its input target and submits it when a remote observer view
is available. Only the inherited target is modeled.

### RequestFrameLevelReset (211)

RTTI record raw **0x1864F64**, actual vtable raw **0x1864F68** (VA
**0x1C66368**). Default constructor **0x53AAD0** clears one DWORD at `+0x28`;
populated constructor **0x53ABF0** copies one DWORD to `+0x28`. No production
caller of the populated constructor was found in the current xref database,
so the value's source and meaning are unknown. Wire layout is also unknown.

### JobLedgerOperation (215)

RTTI record raw **0x1864F24**, actual vtable raw **0x1864F28** (VA
**0x1C66328**). Default constructor **0x53B0F0** clears a shared-text
reference at `+0x28` and the vector descriptor at `+0x2C`; populated
constructor **0x53B2D0** retains the shared-text pointer and copies a vector
of 4-byte values beginning at `+0x2C`. Destructor **0x53B220** releases both
the vector and shared-text reference. The C# model owns a copy of the
DWORD vector but deliberately does not emulate native shared-text ownership.

Callers **0x9282C0** and **0x928400** construct singleton vectors from their
parsed value. The first scans a local DWORD list and returns without
submitting if that value is already present; the second removes a matching
value before submitting. Both submit through the remote observer view.
Their shared-text values come from separate static C-string paths, but their
content and domain meaning are not assigned here. Wire encoding is unknown.

### ExecuteTinkeringPlan (237)

RTTI record raw **0x1864DB4**, actual vtable raw **0x1864DB8** (VA
**0x1C661B8**). Default constructor **0x53D7B0** initializes a DWORD at
`+0x28` and three empty vector descriptors at `+0x2C`, `+0x38`, and
`+0x44`; populated constructor **0x53D960** copies the DWORD and deep-copies
all three vectors. The shared helper **0x5A84E0** copies each element in
16-byte units, so the model retains each vector as copied bytes and rejects
lengths that do not contain whole elements. Destructor **0x53D8F0** releases
all three allocations. No direct production caller of the populated
constructor was found in the current xref database; field meaning and wire
layout remain unknown.

### QueueUnstuck (178)

RTTI record raw **0x1865154**, actual vtable raw **0x1865158** (VA
**0x1C66558**). Constructor **0x536A20** initializes metadata ID **178** and
the vtable but does not write subclass payload fields. No direct production
caller of the constructor was found in the current xref database; only the
inherited target is modeled.

### NPCCombatUpdate (233)

RTTI record raw **0x1864DD4**, actual vtable raw **0x1864DD8** (VA
**0x1C661D8**). Default constructor **0x53D000** initializes two DWORDs at
`+0x28/+0x2C`; populated constructor **0x53D130** copies two positional
DWORDs to those offsets. No direct production caller of the populated
constructor was found in the current xref database, so field meanings and
wire layout remain unknown.

### EquipExperimentalLoadout (236)

RTTI record raw **0x1864DC4**, actual vtable raw **0x1864DC8** (VA
**0x1C661C8**). Default constructor **0x53D5E0** clears one DWORD at
`+0x28`; populated constructor **0x53D700** copies a positional DWORD to
that offset. No direct production caller of the populated constructor was
found in the current xref database, so the value's meaning and wire layout
remain unknown.

### Fabrication_Finalize (246)

RTTI record raw **0x1864D14**, actual vtable raw **0x1864D18** (VA
**0x1C66118**). Default constructor **0x53E9F0** clears a DWORD at `+0x28`
and a byte at `+0x2C`; populated constructor **0x53EB20** copies those
positional values. No direct production caller of the populated constructor
was found in the current xref database, so their meanings and wire layout
remain unknown.

### Fabrication_Claim (247)

RTTI record raw **0x1864D24**, actual vtable raw **0x1864D28** (VA
**0x1C66128**). Default constructor **0x53EBF0** clears DWORDs at
`+0x28/+0x2C`; populated constructor **0x53ED20** copies two positional
DWORDs to those offsets. No direct production caller of the populated
constructor was found in the current xref database, so their meanings and
wire layout remain unknown.

### Fabrication_GenerateResult (245)

RTTI record raw **0x1864D34**, actual vtable raw **0x1864D38** (VA
**0x1C66138**). Default constructor **0x53E7F0** clears three DWORDs at
`+0x28/+0x2C/+0x30`; populated constructor **0x53E920** copies three
positional DWORDs to those offsets. No direct production caller of the
populated constructor was found in the current xref database, so their
meanings and wire layout remain unknown.

### Fabrication_ApplyAction (244)

RTTI record raw **0x1864D44**, actual vtable raw **0x1864D48** (VA
**0x1C66148**). Default constructor **0x53E5F0** clears three DWORDs at
`+0x28/+0x2C/+0x30`; populated constructor **0x53E720** copies three
positional DWORDs to those offsets. No direct production caller of the
populated constructor was found in the current xref database, so their
meanings and wire layout remain unknown.

### Fabrication_FetchInstance (242)

RTTI record raw **0x1864D54**, actual vtable raw **0x1864D58** (VA
**0x1C66158**). Default constructor **0x53E210** clears one DWORD at
`+0x28`; populated constructor **0x53E330** copies one positional DWORD to
that offset. No direct production caller of the populated constructor was
found in the current xref database, so the value's meaning and wire layout
remain unknown.

### Fabrication_Start (243)

RTTI record raw **0x1864D64**, actual vtable raw **0x1864D68** (VA
**0x1C66168**). Default constructor **0x53E3F0** clears a DWORD at `+0x28`
and byte at `+0x2C`; populated constructor **0x53E520** copies those
positional values. No direct production caller of the populated constructor
was found in the current xref database, so their meanings and wire layout
remain unknown.

### Fabrication_FetchAllInstances (240)

RTTI record raw **0x1864D84**, actual vtable raw **0x1864D88** (VA
**0x1C66188**). Constructor **0x53DFB0** initializes metadata ID **240** and
the vtable without writing subclass payload fields. Only the inherited target
is modeled; no direct production caller was found in the current xref database.

### Fabrication_FetchAllRecipes (241)

RTTI record raw **0x1864D74**, actual vtable raw **0x1864D78** (VA
**0x1C66178**). Constructor **0x53E0E0** initializes metadata ID **241** and
the vtable without writing subclass payload fields. Only the inherited target
is modeled; no direct production caller was found in the current xref database.

### SetSteamUserId (235)

RTTI record raw **0x1864DF4**, actual vtable raw **0x1864DF8** (VA
**0x1C661F8**). Default constructor **0x53D420** clears two DWORDs at
`+0x28/+0x2C`; populated constructor **0x53D530** copies the corresponding
two-DWORD value to those offsets. The C# snapshot combines them as one
64-bit `SteamUserId`, following the native class name; no direct production
caller was found in the current xref database to confirm its interpretation.
Wire layout remains unknown.

### FlushCharacterCache (231)

RTTI record raw **0x1864E14**, actual vtable raw **0x1864E18** (VA
**0x1C66218**). Default constructor **0x53CC00** clears five bytes at
`+0x28..+0x2C`; populated constructor **0x53CD10** copies five positional
bytes to those offsets. No direct production caller of the populated
constructor was found in the current xref database, so their meanings and
wire layout remain unknown.

### BagInventorySettings (234)

RTTI record raw **0x1864DE4**, actual vtable raw **0x1864DE8** (VA
**0x1C661E8**). Default constructor **0x53D200** clears the presence byte
at `+0x28`; populated constructor **0x53D330** sets it when the source
optional record is present. The nested object begins at `+0x2C`, uses native
vtable VA **0x1C80CC4**, and stores a shared-object pointer/control-block
pair at `+0x30/+0x34` plus a DWORD at `+0x38`. Helper **0x12CC430** copies
the reference-counted pair; no gameplay meaning is inferred for the pointer
or trailing DWORD. No direct production caller of the populated constructor
was found in the current xref database, and wire layout remains unknown. The
nested vtable's second entry resolves to deleting wrapper **0x5A9E20**, which
restores a base vtable and conditionally dispatches deallocation; the specific
reference-pair cleanup path was not established.

### TutorialEventTriggeredCmd (239)

RTTI record raw **0x1864D94**, actual vtable raw **0x1864D98** (VA
**0x1C66198**). Constructor **0x53DD90** initializes positional DWORDs at
`+0x28`, `+0x30`, and `+0x34` plus a byte at `+0x38`; populated constructor
**0x53DED0** copies those same fields from its arguments. Neither constructor
touches the gap at `+0x2C`. The constructor metadata initializer confirms
message ID **239**. No populated-constructor caller was identified; field
semantics and wire layout remain unknown.

### Elite-level and team-manager commands (228-232)

`RevertAllEliteLevelsUpgrades` (228; constructor **0x53C810**) and
`ResetAllEliteLevelsUpgrades_Debug` (230; constructor **0x53CAF0**) only
install their catalog vtables and initialize no subclass fields. The
`SelectEliteLevelsAward` constructor **0x53C920** sets its sole subclass
DWORD at `+0x28` (metadata ID 229). `RunTeamManagerCommand` (ID 232) has a
default constructor **0x53CE00** that clears byte `+0x28`, DWORD `+0x2C`,
and byte `+0x30`; populated constructor **0x53CF00** copies the first and
last bytes and retains a reference-counted shared-text value at `+0x2C`.
No direct production callers of these populated constructors were found.
Fields other than the shared-text reference remain positional, and wire
layouts are unknown.

### Reward commands (222-227)

`ClaimDailyRewardStreak` (223), `BuyBackPreviousDay` (224), and
`AcceptRewards` (225) each have a default constructor that zeroes one DWORD
at `+0x28` and a populated constructor that copies one DWORD there. For
`ClaimDailyRewardItem` (222), both observed constructor variants zero the
DWORD at `+0x28`; no populated copy constructor was found. The field's
meaning is not established.
`FlushRewards` (226; constructor **0x53C610**) and
`RerollEliteLevelsAwardList` (227; constructor **0x53C700**) initialize no
subclass fields. No direct production callers were identified; wire layouts
remain unknown.

### Activity, mission, and revive requests (171-173, 201, 209, 216)

`RequestMissionAvailability` (171) has three DWORDs at `+0x28/+0x2C/+0x30`;
its populated constructor **0x536020** copies the first pair and a final
DWORD. `RequestNewActivity` (172) is kind-only. `RequestPushMission` (173)
stores one DWORD at `+0x28`. `TimedDailyRewardRequest` (201) stores a byte
at `+0x28`; `RequestSelfRevive` (209) is kind-only; and `SeatChangeRequest`
(216) stores one byte at `+0x28`. Their constructor metadata IDs and layouts
were observed directly, but no production callers or wire layouts were
identified.

### Weapon and activity-state commands (109-114)

`MapOpened` (109) and `BattleframePurchased` (110) initialize no subclass
fields. `CollectLoot` (111) has four positional DWORDs at `+0x28..+0x34`.
`TempSlotAbilities` (112) initializes four DWORDs at `+0x28..+0x34` to zero
and writes `0xFFFFFFFF` at `+0x38`. `SinAcquire_Source` (113) copies two
DWORDs at `+0x28/+0x2C`. `BroadcastWeaponTweaks` (114) copies bytes at
`+0x28/+0x29`, then a reference-counted pair at `+0x30/+0x34` and a DWORD
at `+0x38`; its embedded subobject has vtable VA **0x1C80CC4**. No field
meanings beyond the observed reference-counted ownership are inferred, and
wire layouts remain unknown.

### Challenge match controls (102-106)

`ChallengeSwapTeam` (102) copies six DWORDs at `+0x28..+0x3C`.
`ChallengeSetRoleAndTeam` (103) copies DWORDs at `+0x28/+0x2C/+0x30/+0x34`,
a byte at `+0x38`, and a DWORD at `+0x3C`.
`ChallengeSetMatchParameters` (104) copies three DWORDs at
`+0x28/+0x2C/+0x30`.
`ChallengeSetPowerPrivilege` (105) copies four DWORDs at
`+0x28/+0x2C/+0x30/+0x34` and a byte at `+0x38`.
`ChallengeStartMatch` (106) copies two DWORDs at `+0x28/+0x2C`.
The observed default/populated constructor pairs are, respectively:
`ChallengeSwapTeam` **0x52D8C0/0x52DA10**,
`ChallengeSetRoleAndTeam` **0x52DAE0/0x52DC20**,
`ChallengeSetPowerPrivilege` **0x52DEE0/0x52E020**,
`ChallengeStartMatch` **0x52E0F0/0x52E220**,
`ChallengeReadyCheck` **0x52D6E0/0x52D810**,
`ChallengeKick` **0x52D250/0x52D410**,
`ChallengeSetReady` **0x52D4F0/0x52D620**,
`ChallengeInvitationSquadInfo` **0x52CF50/0x52D150**,
`ChallengeInvitationResponse` **0x52CD60/0x52CE90**,
`ChallengeLeave` **0x52C8D0/0x52CA00**,
`ChallengeInvitation` **0x52CAB0/0x52CC70**, and
`ChallengeCreate` **0x52C690/0x52C820**.
`ChallengeSetMatchParameters` has a populated constructor at **0x52DE20**;
its putative default-constructor pattern at **0x52DD4A** is not a defined
function in the current IDB and is not treated as confirmed.
Each has default initialization evidence for its payload fields except the
default form of `ChallengeSetMatchParameters`, which was not confidently
identified. Their field meanings, production callers, and wire layouts
remain unknown.

The adjacent challenge requests continue with:

* `ChallengeReadyCheck` (101): two DWORDs at `+0x28/+0x2C`.
* `ChallengeKick` (99): two DWORDs at `+0x28/+0x2C` and a shared-text
  reference at `+0x30`.
* `ChallengeSetReady` (100): two DWORDs at `+0x28/+0x2C` and a byte at
  `+0x30`.
* `ChallengeInvitationSquadInfo` (98): four DWORDs at `+0x28..+0x34`, a
  shared-text reference at `+0x38`, and a deep-copied vector of opaque u64
  identifiers at `+0x3C`.
* `ChallengeInvitationResponse` (97): two DWORDs at `+0x28/+0x2C` and a
  byte at `+0x30`.
* `ChallengeLeave` (95): two DWORDs at `+0x28/+0x2C`.
* `ChallengeInvitation` (96): two DWORDs at `+0x28/+0x2C`, a shared-text
  reference at `+0x30`, a byte at `+0x34`, and a DWORD at `+0x38`.
* `ChallengeCreate` (94): a deep-copied vector of opaque u64 identifiers
  at `+0x28`.

The identifier-vector element size is confirmed by `CopyRepairIdentifierVector`
(`0x3576A0`), which divides the source span by eight and copies both DWORDs
of each element. Constructor layouts do not establish these fields' semantic
names or wire encodings. No populated production callers were found.

### Carryable-object commands (189-190)

`PickupCarryableObjectByProximity` (189) and `DropCarryableObject` (190)
each copy an opaque 8-byte identifier to `+0x28`. The default constructors
zero that payload. The populated constructor for request 189 has two observed
callers, `0x93E450` and `0x93E590`; both create and submit the request after
their local rate/state gates. No direct production caller was found for the
populated request-190 constructor. The identifier's exact domain and wire
encoding remain unverified.

### SlotGearRequest (206)

The default constructor **0x53A1E0** initializes a DWORD at `+0x28`, an
8-byte value at `+0x30`, a byte at `+0x38`, and a DWORD at `+0x3C`; it does
not touch the four-byte gap at `+0x2C`. The populated constructor
**0x53A300** copies those same fields. Caller **0x4D6710** searches a slot
list for a match, builds the request, and submits it through
**0x4F3420**. These payload fields remain positional because their domain
semantics and wire encoding are not established.

### LFGCheckin and LFGLeave (107-108)

The `LFGCheckin` default/populated constructors are **0x52E2D0/0x52E400**;
the `LFGLeave` pair is **0x52E4D0/0x52E600**. Each has two DWORDs at
`+0x28/+0x2C`, zeroed by the default constructor and copied by the populated
form. `LFGLeave`'s populated constructor is called from **0x6E3060**; that
caller sends the request after local activity/rate checks. No direct
production caller for the populated `LFGCheckin` constructor was found.
Payload meanings and wire encodings remain unknown.

### Party, squad and resource-location requests (185, 187, 212-213)

`UpdateChatPartyMembers` (185) deep-copies a vector of opaque 16-byte
records to `+0x28`, then copies DWORDs at `+0x38/+0x3C`; the four-byte gap
at `+0x34` is untouched. Its default constructor zeros the vector header
and trailing DWORDs. The helper computes element count from byte length
divided by 16 and copies the entire vector.

`ResourceLocationInfosRequest` (187) initializes the inherited target and
vtable but no subclass payload. `LeaveEncounterParty` (212) copies two
DWORDs at `+0x28/+0x2C`; `JoinSquadLeadersArc` (213) initializes no
subclass payload. No field meanings or wire encodings are inferred.

### MatchQueue (90)

Default constructor **0x52BD90** initializes the payload at `+0x28..+0x70`,
leaving the 4-byte gap at `+0x34` untouched. Populated constructor
**0x52C000** deep-copies a vector of 8-byte elements at `+0x28`, copies an
opaque qword at `+0x38`, DWORDs at `+0x40/+0x44`, deep-copies a u32 vector
at `+0x48`, and then copies positional fields at `+0x54..+0x70`. The final
DWORDs at `+0x68/+0x70` are retained shared-text references; the byte at
`+0x6C` is copied verbatim. The populated constructor has four observed
production callers (**0x85B170, 0x85B610, 0x85B850, 0x85BA40**). Their
semantic meaning and wire serialization are not established.

### Tutorial ID requests (220-221)

`ResetTutorialId` (220) stores `tutorialId` at `+0x28`; the populated
constructor **0x53BC50** is called at **0xA2B9C8**. `DismissTutorialId`
(221) has the same field at the same offset; its populated constructor
**0x53BE20** is called at **0xA2BB78** from a Lua-script-exposed builder.
The traced call paths constrain IDs to 1..14; the native constructors still
store the supplied value and log an out-of-range value rather than rejecting
it. Their wire encodings are not established here.

### Deployable and resource beacon commands (181-182)

`DeployableHardpointSelection` (181) copies DWORDs at `+0x28/+0x2C/+0x30`;
its populated constructor **0x537080** is called from **0x4B3840**.
`ResourceNodeBeaconCalldownRequest` (182) has the same three-DWORD layout;
its populated constructor **0x537260** is called from **0x9542D0**. The
default form for request 182 installs the vtable without subclass payload
initialization. Field meanings and wire encodings are not inferred.

### SendMailToPlayer (84)

RTTI catalog record raw **0x1865744**, actual vtable raw **0x1865748**
(VA 0x1C66B48). Empty/debug constructor **0x52AFB0** and populated
constructor **0x52B260** initialize metadata ID **84**. The populated
constructor receives the inherited target at `+0x18`, copies two positional
DWORDs to `+0x28/+0x2C`, retains three
shared-text references at `+0x30/+0x34/+0x38`, and copies a vector at
`+0x3C` whose elements are 24 bytes. Each element has six DWORD slots;
the second slot is retained as a shared-text reference, while the other
slots remain positional.

Caller **0xA12C50** references the populated constructor at **0xA13027**
and dispatches through **0xA41B50** when a remote observer view is
available. The C# model snapshots the opaque 32-bit pointers and vector
records; it does not reconstruct shared-text ownership or wire encoding.

### ListItemForSale (83)

RTTI catalog record raw **0x1865764**, actual vtable raw **0x1865768**
(VA 0x1C66B68). Empty/debug constructor **0x52AC30** and populated
constructor **0x52AE70** initialize metadata ID **83**. The populated form
receives its inherited target at `+0x18`, writes positional u32s at
`+0x28/+0x2C`, `+0x30/+0x34`, and `+0x38/+0x3C`, retains shared-text
references at `+0x40` and `+0x50`, writes u32s at `+0x48/+0x4C`, and writes
a byte at `+0x54`. The empty form clears those fields and initializes the
byte to 2. No populated-constructor caller was resolved; field meanings and
wire order remain unknown.

### ClearSavedMatchQueue (91)

RTTI catalog record raw **0x18656C4**, actual vtable raw **0x18656C8**
(VA 0x1C66AC8). Constructor **0x52C170** calls the base initializer with
metadata ID **91**, writes the class vtable, and initializes a minimum
0x28-byte prefix: two positional u32s at `+0x18/+0x1C`, followed by zeroed
u32s at `+0x20/+0x24`. The caller at **0xA1BB20** builds the first pair
through target-packing helper **0x3264C0** (discriminator 2), then submits
the command only when a remote observer view is available. The two words
remain positional in C#; no class-specific wire format is inferred.

### NotifyDialogScriptComplete (143)

RTTI catalog record raw **0x18653A4**, actual vtable raw **0x18653A8**
(VA 0x1C667A8). Empty/debug constructor **0x532800** and populated
constructor **0x532910** initialize metadata ID **143**. The populated
form stores u32s at `+0x28/+0x2C`; the empty form zeros both. The populated
constructor is called from **0x4D0D50** at **0x4D0DA2**. Field meanings
and wire ordering are unknown.

### PerformQuickChatCommand (144)

RTTI catalog record raw **0x18653B4**, actual vtable raw **0x18653B8**
(VA 0x1C667B8). Empty/debug constructor **0x5329C0** and populated
constructor **0x532AC0** initialize metadata ID **144**. The populated
form stores a u32 at `+0x28`; the empty form zeros it. Caller **0x4B9280**
references the populated constructor at **0x4B92CE**. No field semantics
or wire order are inferred.

### PerformDialog (146)

RTTI catalog record raw **0x1865364**, actual vtable raw **0x1865368**
(VA 0x1C66768). Empty/debug constructor **0x532EB0** and populated
constructor **0x533020** initialize metadata ID **146**. The populated
form writes u32s at `+0x28`, `+0x30/+0x34`, `+0x38/+0x3C`, `+0x40`,
`+0x48`, `+0x50`, and `+0x58/+0x5C`, plus bytes at `+0x44/+0x4C`.
Neither constructor assigns `+0x2C` or `+0x54`. No caller for the
populated constructor was found; field meanings and wire order remain
unknown.

### PerformTextChat (145)

RTTI catalog record raw **0x1865374**, actual vtable raw **0x1865378**
(VA 0x1C66778). Empty/debug constructor **0x532B70** and populated
constructor body at **0x532D80** initialize metadata ID **145**. The
populated body retains a shared-text reference at `+0x28`, stores a byte at
`+0x2C`, and copies a nested descriptor at `+0x30` with helper
**0x60C790**:

| Offset | Native shape |
|---|---|
| `+0x30` | presence byte for the pair at `+0x38/+0x3C` |
| `+0x38/+0x3C` | two positional u32s, written only when `+0x30` is set |
| `+0x40` | presence byte for shared-text reference `+0x44` |
| `+0x44` | retained shared-text reference, copied only when `+0x40` is set |
| `+0x48` | byte argument |
| `+0x4C` | presence byte for the optional descriptor at `+0x50` |
| `+0x50` | descriptor vtable `0x1C80CC4` |
| `+0x54/+0x58` | two-word shared-text value copied by `0x12CC430` |
| `+0x5C` | positional u32 copied from the optional source |

The descriptor fields are now structurally traced, but their gameplay
meanings remain unknown. Conditional fields that the constructor does not
write are represented as nullable values in C# rather than silently forced
to zero.

Constructor references were found at **0x6E4BE4**, **0x6E8D02**,
**0x6EBD1A**, **0x6EC12E**, **0x6F363B**, and **0x6F3C0B**, from callers
**0x6E4AE0**, **0x6E8BB0**, **0x6EB870**, **0x6EBDE0**, **0x6F31B0**, and
**0x6F3780**, respectively. These sites pass through the same constructor
body and send through the remote observer view when available; no
class-specific wire order has been reconstructed.

### SetEffectsFlag (148)

RTTI catalog record raw **0x1865354**, actual vtable raw **0x1865358**
(VA 0x1C66758). Empty/debug constructor **0x5333F0** and populated
constructor **0x5334F0** initialize metadata ID **148**. The populated
form writes bytes at `+0x28/+0x29` with the first set to 1; the empty
form clears the u16 at `+0x28`. A caller at **0x4D5250** references the
populated constructor at **0x4D5374**. No semantic labels or wire layout
are established.

### SelectLoadout (150)

RTTI catalog record raw **0x1865324**, actual vtable raw **0x1865328**
(VA 0x1C66728). Empty/debug constructor **0x5337A0** and populated
constructor **0x5338B0** initialize metadata ID **150**. The populated
form stores a u32 at `+0x28` and byte at `+0x2C`; the empty form zeros
both. Caller **0x4A31A0** references the populated constructor at
**0x4A3268**. Field meanings and wire ordering remain unknown.

### DebugMission (154)

RTTI catalog record raw **0x18652D4**, actual vtable raw **0x18652D8**
(VA 0x1C666D8). Empty/debug constructor **0x533DF0** and populated
constructor **0x533FA0** initialize metadata ID **154**. The populated
form stores a u32 at `+0x28` and a shared-text reference at `+0x2C`; the
empty form clears both. No populated-constructor caller was found. The
reference is retained as an opaque 32-bit process-local pointer; its
meaning and wire representation are unknown.

### AssignBounties (155)

RTTI catalog record raw **0x18652E4**, actual vtable raw **0x18652E8**
(VA 0x1C666E8). Empty/debug constructor **0x534090** and populated
constructor **0x5341B0** initialize metadata ID **155**. The populated
form stores one u32 at `+0x28`; the empty form zeros it. Caller references
were found at **0x9840FA** in **0x9840B0** and at **0xA2A64E** in
**0xA2A530**. The field meaning and wire order are unknown.

### AbortBounty (156)

RTTI catalog record raw **0x18652C4**, actual vtable raw **0x18652C8**
(VA 0x1C666C8). Empty/debug constructor **0x534260** and populated
constructor **0x534380** initialize metadata ID **156**. The populated
form stores one u32 at `+0x28`; the empty form zeros it. Caller
**0x984130** references the populated constructor at **0x984179**.

### ActivateBounty (157)

RTTI catalog record raw **0x18652B4**, actual vtable raw **0x18652B8**
(VA 0x1C666B8). Empty/debug constructor **0x534430** and populated
constructor **0x534550** initialize metadata ID **157**. The populated
form stores one u32 at `+0x28`; the empty form zeros it. Caller
**0x9841B0** references the populated constructor at **0x9841F9**.

### ListActiveBounties (158)

RTTI catalog record raw **0x1865294**, actual vtable raw **0x1865298**
(VA 0x1C66698). Empty/debug constructor **0x534610** and populated
constructor **0x534740** initialize metadata ID **158**. The populated
form stores a u32 at `+0x28` and a byte at `+0x2C`; the empty form zeros
both. Caller **0x984BE0** references the populated constructor at
**0x984C2D**.

### ListActiveBountyDetails (159)

RTTI catalog record raw **0x18652A4**, actual vtable raw **0x18652A8**
(VA 0x1C666A8). Empty/debug constructor **0x534810** and populated
constructor body at **0x534940** initialize metadata ID **159**. The
populated code stores a u32 at `+0x28` and byte at `+0x2C`; the empty
form zeros both. The populated body lies in an IDA-misclassified region;
caller **0x984C60** references it at **0x984CAC**. No field semantics
or wire order are inferred.

### ClearBounties (161)

RTTI catalog record raw **0x1865274**, actual vtable raw **0x1865278**
(VA 0x1C66678). Empty/debug constructor **0x534C10** and populated
constructor **0x534D30** initialize metadata ID **161**. The populated
form stores one u32 at `+0x28`; the empty form zeros it. Caller
**0x984AE0** references the populated constructor at **0x984B2A**.

### TrackBounty (165)

RTTI catalog record raw **0x1865234**, actual vtable raw **0x1865238**
(VA 0x1C66638). Empty/debug constructor **0x535420** and populated
constructor **0x535540** initialize metadata ID **165**. The populated
form stores one u32 at `+0x28`; the empty form zeros it. Caller
**0x9862D0** references the populated constructor at **0x98635A**.

### SetBountyVar (166)

RTTI catalog record raw **0x1865214**, actual vtable raw **0x1865218**
(VA 0x1C66618). Empty/debug constructor **0x5355F0** and populated
constructor **0x535800** initialize metadata ID **166**. The populated
form stores a u32 at `+0x28`; pointer-sized values at `+0x2C/+0x30` are
copied with reference-count operations. Caller **0x984DE0** references
the populated constructor at **0x984E53**. The two values remain opaque
process-local references; meanings and wire format are unknown.

### RefreshBounties (167)

RTTI catalog record raw **0x1865224**, actual vtable raw **0x1865228**
(VA 0x1C66628). The only observed constructor, **0x535920**, initializes
metadata ID **167** and installs the vtable without subclass fields.
Absence of a populated constructor in the current search is not proof that
none exists.

### PerformEmote (142)

RTTI catalog record raw **0x1865394**, actual vtable raw **0x1865398**
(VA 0x1C66798). Empty/debug constructor **0x532640** and populated
constructor **0x532750** initialize metadata ID **142**. The populated
form stores a u32 at `+0x28` and signed u16 at `+0x2C`; the empty form
zeros both.

The populated constructor is referenced from **0x4B9320** (through a
selected-data path) and the update routine **0x4C7360**. The field meanings
and wire ordering are not established.

### ActivateConsumable (138)

RTTI catalog record raw **0x18653E4**, actual vtable raw **0x18653E8**
(VA 0x1C667E8). Empty/debug constructor **0x531F10** and populated
constructor **0x532040** initialize metadata ID **138**. The populated
form stores u32 fields at `+0x28/+0x2C`; the empty form zeros both.

The populated constructor is called at **0xAA916A** from **0xAA8FC0**
after an identity/state check and a helper-derived nonzero value. The
caller sends the request through **0xAB0C60**. Field meanings and wire
order remain unknown.

### SetNoSpreadFlag (139)

RTTI catalog record raw **0x18653C4**, actual vtable raw **0x18653C8**
(VA 0x1C667C8). Empty/debug constructor **0x5320F0** and populated
constructor **0x532210** initialize metadata ID **139**. The populated
form stores two bytes at `+0x28/+0x29`; the empty form clears the u16 at
`+0x28`. No populated-constructor callers were found.

### ExitAttachmentRequest (140)

RTTI catalog record raw **0x18653D4**, actual vtable raw **0x18653D8**
(VA 0x1C667D8). The only discovered constructor, **0x5322E0**, initializes
metadata ID **140**, installs the vtable, and performs the standard target
kind validation. It writes no subclass fields, so the C# model is
kind-only. No class-specific payload writer was identified.

### SetMovementSimulation (197)

RTTI catalog record raw **0x1865024**, actual vtable raw **0x1865028**
(VA 0x1C66428). Empty/debug constructor **0x5391E0** and populated
constructor **0x5392E0** initialize metadata ID **197** through
**0x5A7200**. The empty form zeros a u16 at `+0x28`; the populated form
copies a u16 into that field. Both validate the inherited handle kind
against `[1,14]`.

The only discovered caller is tick/update routine **0x4B9700**. It
constructs and sends this command only when its state byte at caller
`+0xC22` is set, an observer connection exists at caller `+0x18`, and
more than **1000 ms** have elapsed since its stored tick at `+0xC24`.
It passes the caller's u16 at `+0xC20` into the message, builds the target
handle from the identifier at the owning object's `+0x4`, sends through
**0x4F9290**, clears the state byte, and records the current tick. The
meaning of the u16 and the exact class-specific wire order are unknown;
`SetMovementSimulation.cs` is an object-layout model only.

### RequestAchievementStatus (169)

RTTI catalog record raw **0x18651F4**, actual vtable raw **0x18651F8**
(VA 0x1C665F8). Empty/debug constructor **0x535C30** and populated
constructor **0x535D50** initialize metadata ID **169** through
**0x5A6550**. The empty form zeros the DWORD at `+0x28`; the populated
form stores its first argument there. Both validate the inherited handle
kind against `[1,14]`.

One caller, **0x9368D0**, looks up a record by the supplied DWORD,
proceeds only when the record byte at `+0x14` is zero, and constructs
the request with the record's first DWORD and a handle whose kind is 4.
Another constructor use occurs inside state-dispatch routine **0xA0F510**.
This is `RequestAchievementStatus`, not the distinct
`Character::Commands::FireWeaponProjectile` class. The field meaning and
class-specific wire order remain unverified.

### ListPreviousBounties (163)

RTTI catalog record raw **0x1865264**, actual vtable raw **0x1865268**
(VA 0x1C66668). Empty/debug constructor **0x534FF0** and populated
constructor **0x535110** initialize metadata ID **163**. The populated
form stores one u32 at `+0x28`; the empty form zeros it.

The populated constructor is called at **0x984DAA** from **0x984D60**.
The value's meaning and the class-specific wire order are unknown.

### ListAchievements (168)

RTTI catalog record raw **0x1865204**, actual vtable raw **0x1865208**
(VA 0x1C66608). Empty/debug constructor **0x535A50** and populated
constructor **0x535B70** initialize metadata ID **168**. The populated
form writes a byte at `+0x28`; the empty form zeros it. No static caller
for the populated constructor was found.

### RequestAllAchievements (170)

RTTI catalog record raw **0x18651D4**, actual vtable raw **0x18651D8**
(VA 0x1C665D8). The only discovered constructor, **0x535E00**, initializes
metadata ID **170**, installs the vtable, and performs the standard target
kind validation. It writes no subclass payload fields, so the C# model is
kind-only; no class-specific payload writer was found.

### FindNearbyResourceAreas (183)

RTTI catalog record raw **0x1865104**, actual vtable raw **0x1865108**
(VA 0x1C66508). Its discovered constructor is **0x537320**, which
initializes common message metadata ID **183** through **0x5A6B70** and
installs this class's vtable. It writes no subclass payload, only the
inherited handle validation/logging, so the current model is kind-only.

The constructor is called from **0x972360**, a large movement/state
routine. No class-specific body writer or field payload was established.

### RequestTransfer (199)

RTTI catalog record raw **0x1865004**, actual vtable raw **0x1865008**
(VA 0x1C66408). Empty/debug constructor **0x539490** and populated
constructor **0x5395C0** initialize common message metadata ID **199
(0xC7)** through **0x5A72E0**. The empty form zeros `+0x28/+0x2C`; the
populated form stores a u32 at `+0x28` and one byte at `+0x2C`. Both
validate the inherited handle kind `[1,14]`.

The request is built from a Lua-exposed path at **0xA2D990** (entered
through thunk 0xA46480). It reads Lua argument 1 as the u32 value,
derives the byte option from Lua argument 2 truthiness and a local state
field equal to 2, performs table/bitmask checks against `0x1E4B5E0`,
constructs the message at **0xA2DB84** with a local-player handle
retagged to kind 2, sends it at **0xA2DB94** through 0xA3CC00, and
returns the send result to Lua. The exact meanings of the u32 and byte
remain uncertain.

The concrete body writer **0xA4BE20**, called through **0xA46210** with
ID 0xC7, emits inherited target u64, one-byte MessageID, u32 at `+0x28`,
and byte at `+0x2C`, in that order. `RequestTransfer.EncodeBody()`
models the exact **14-byte** body, excluding transport framing.

## Address translation (PE ImageBase 0x400000)
IDA is loaded raw, so its addresses are file offsets, **not** VAs. Absolute immediates inside code are still
unrelocated PE VAs, and IDA mis-resolves them as raw addresses. For example, the "jumptable … case 0" comments
on `push 0x1C5EA58` are spurious. Translate VA → raw with the section table, read from the PE headers:

| Section | RVA | Raw | VA − raw |
|---|---|---|---|
| .text | 0x1000 | 0x400 | 0x400C00 |
| .rdata | 0x16C4000 | 0x16C2C00 | 0x401400 |
| .data | 0x19C5000 | 0x19C3400 | 0x401C00 |

Every VA cited here was re-checked against the raw file bytes:

| VA | Raw | Verified content |
|---|---|---|
| 0x1C61C8C | 0x186088C | "Will not SendCommand %s -- too early!" |
| 0x1C61CB4 | 0x18608B4 | "ROUTED - Sent %s" |
| 0x1C606E4 | 0x185F2E4 | "tried to pack an uninitialized MessageID" |
| 0x1B57625 | 0x1756225 | byte `00` (MessageID escape) |
| 0x1C665A8 / 0x1C66598 / 0x1C66468 | 0x18651A8 / 0x1865198 / 0x1865068 | vtables. The COL at −4 names `.?AVLogDirectActivityRequest` / `LogActivityPush` / `UpdateShoppingList@Commands@Character@firefall@@` |
| 0x1C81144 / 0x1C80CC4 | 0x187FD44 / 0x187F8C4 | `.?AVblob_packer@serialize@sl@@` / `.?AVslBlob@@`. Packer slot 0 = VA 0x16EE500 → raw 0x12ED900 |
| 0x1AC4D24 | 0x16C3924 | IAT entry → `timeGetTime` |
| 0x1DFFD98 | 0x19FE198 | `.data` global. Zero in the file, so its value is set at runtime (UNKNOWN) |

No jump tables are involved in this scope. The mode switch in `0x494BF0` is a `dec/jz` chain.
## Messages
| Id | RTTI class | vtable (file) | Sender | Encoder | Gate | Body |
|---|---|---|---|---|---|---|
| 0xAE 174 | `LogDirectActivityRequest` | 0x18651A8 | 0x90EE20 | 0x90FD40 | 0x90CBC0 | `u64 target, u8 0xAE` (9 B) |
| 0xAF 175 | `LogActivityPush` | 0x1865198 | 0x90EF50 | 0x90FE20 | 0x90CC30 | `u64 target, u8 0xAF, u32 boredGauge` (13 B) |
| 0xC2 194 | `UpdateShoppingList` | 0x1865068 | 0x90F260 | 0x910040 | 0x90CF60 / inline | `u64 target, u8 0xC2, setA, setB` |

The integers are little-endian: the packer copies raw x86 values.
- Example: `LogDirectActivityRequest(target=0x0000000100000004)` encodes as `04 00 00 00 01 00 00 00 AE`.
- Set encoding (0x5BA110 / 0x5BA240): repeat `{u8 n=min(rem,255); n elements}` while `n==255`. An empty set encodes as `00`, and exactly 255 elements end with an extra `00`.
- **setA** (msg+0x28) element: `{u32 First, u32 Second}`, unique and ordered by `Second` (insert 0x5B9530).
- **setB** (msg+0x30) element: `{u32 Value, u8 Tag}`, ordered by `(Value, Tag)` (insert 0x5B9C40).

## Evidence
| Item | File offset |
|---|---|
| blob_packer vtable / slBlob vtable | 0x187FD44 / 0x187F8C4 |
| u64 / u32 / u32 / u16 / u8 writers | 0x325BE0 / 0x327310 / 0x344440 / 0x327210 / 0x3273D0 |
| MessageID packer: ≤0xFF → 1 byte; otherwise escape `00` (0x1756225) + u16; asserts at lines 50 and 57 | 0x320900 |
| Encoder call order: target 0x90FDB7 → id 0x90FDC0 → finalize (vtbl slot 0 = 0x12ED900) 0x90FDC9 | 0x90FD40 |
| Send on channel 2 (conn send routine) | call 0x90EEED → 0xBD3B20 |
| Base inits: "msg-174" 0x175A4B4, "msg-175" 0x175A4BC; "msg-194" 0x175A444 (inline) | 0x5A6780 / 0x5A67F0 / 0x9095B0 |
| Constructors (target check `kind-1 <= 0x0D`, "Invalid target: 0x%016llx" 0x175B4A8) | 0x5363E0 / 0x536610 |
| IsValid(target): ≠0 and not `FF..FFxx` | 0x3265A0 |
| Lua bindings; usage strings at 0x17A7130 / 0x17A7090 | 0xA2D680 (174) / 0xA2D770 (175) |
| Direct builders (no code xrefs found) | 0x908F20 (174) / 0x908FA0 (175) |
| C2 builder: target low byte forced to 4 (`or ebx,4` 0x909794) | 0x9095B0 |
| C2 flush throttle (timeGetTime IAT raw 0x16C3924) | 0x49B3D0, also called from 0x4A7980 |
| Local character / controller (+0x4D20) | 0x49DDD0 / 0x49DCC0 |
| Connection select (global VA 0x1DFFD98 / raw 0x19FE198, mode at +0xB80) | 0x494BF0 |
| Gate strings "Will not SendCommand %s -- too early!" / "ROUTED - Sent %s" | 0x186088C / 0x18608B4 |

The C2 message is filled from an outer tree held by the throttle owner (0x9095B0 `this`):
- each element `{u32,u32}` at node+0x10 goes into setA;
- the inner tree at node+0x18 supplies the `{u32,u8}` elements for setB.

## Unknowns (not guessed)
- What the target's kind byte `4` means, and what entity the target denotes.
- What the setA/setB fields mean. The name "shopping list" comes from RTTI only.
- The units and range of `boredGauge`. The Lua conversion path (tag 0x13 → int) is an inference.
- Message base fields +0x04, +0x08 (=1), +0x14, +0x20 and +0x24.
- Connection fields +0x0C (the value must be >3), +0x14 bit 8 ("ROUTED"), +0x820, and controller +0x43C8.
- What channel 2 means, and the transport framing added by 0xBD3B20 and below.
- Several functions have no code xrefs or table pointers, so their callers are unknown:
  - wrappers 0x90DF80 / 0x90DFA0 / 0x90E270;
  - encoders 0x90F750 / 0x90F780 / 0x90F870;
  - helpers 0x90C3C0 / 0x90C3E0 / 0x90C4A0;
  - the direct builders 0x908F20 / 0x908FA0.
- Who sets the throttle's dirty flag (+8).
