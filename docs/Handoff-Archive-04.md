# Reverse-engineering handoff archive 04

## Earlier suggested next work (superseded where noted above)

1. The differing-layout path at raw `0x1311BD0` now has a partial native trace:
   it builds a 0x20-byte result holder, reserves `rowCount * destinationStride`
   bytes, invokes destination layout callback `+0x1C` per row, updates a
   destination-field bitset from `0x1311100` return values, and optionally
   calls callback `+0x20` with row count, first-row address, and a stride
   pointer. The identical-layout path has now been traced too: it bypasses
   mapping, allocation, row initialization and conversion, keeps the source
   row pointer, then reaches the common callback/result assignment. **Resolved**:
   the destination-field bitset (`Sdb_CreateFieldConversionWarnedBitset`, raw
   `0x1312B40`) is a one-warning-per-field-per-call suppressor for unsupported
   type-conversion pairs, not a correctness mechanism — see
   `src\Serialization\README.md` for the full evidence trail (bitset seeded
   all-true, cleared on first conversion failure per field, gates the
   `"Could not convert field %s (%s) to %s"` log call at raw `0x1311847`).
   Example `+0x1C`/`+0x20` callback bodies are documented, but their full
   per-table behavior and ownership remain unresolved. The `+0x24` teardown
   call path is now traced through database reset; see current findings above.
   Full offsets are in `src\Serialization\README.md`.
2. **Resolved for the mapped-table tree:** `Sdb_ResetLoadedDatabaseState`
   (`0x1312030`) traverses the tree at `this+0x34`, whose nodes contain the
   catalog table hash at `+0x10` and result-holder pointer at `+0x14`. The
   mapper inserts `[catalogEntry+4, resultHolder]` into this tree; reset
   follows the holder's catalog pointer to catalog callback `+0x24`, calls it
   with holder row count `+4` and exposed rows pointer `+0x0C`, releases its
   refcounted handle at `+0x18`, then frees it. This supersedes the earlier
   hypothesis that the mapped value's `+0x00` was a vtable: it is a catalog
   entry pointer. The separate tree at `this+0x28` is now modeled as an
   auxiliary buffer map with a derived displacement key and pointer-like
   value; key/value semantics and its conditional value ownership remain
   unresolved. Function
   names and offsets for both tree cleanup paths are documented in
   `src\Serialization\README.md`.
3. **Resolved**: the parser's database object has a static, compiled-in table
   catalog at `this+4` (count at catalog `+4`, pointer array at `+8`, 575
   entries in the shipped binary). Each entry **is itself** the destination
   layout descriptor (hash at `+4`, stride `+8`, field count `+14`, field
   array `+18`, init callback `+1C`, finalize callback `+20`) — there is no
   extra indirection and no runtime registration call; the catalog is static
   data established before any traced code runs. Two different example
   callback pairs are now decompiled and documented (hash `0x8E3A6BCC`: clear
   two bytes + build a row-handle view array; hash `0x4CCBB136`: write real
   per-field float defaults + a **NULL** finalize callback, confirming the
   mapper's "use the raw row address directly" fallback is real). A further
   finding: both structs' `+0x00` field, previously guessed as a vtable, is
   actually a pointer to a packed, NUL-terminated ASCII string holding the
   hex text of that struct's own hash (verified against two independent
   examples) — almost certainly a debug-name aid generated alongside the hash
   at build time, since the real source name is not retained anywhere found.
   **Correction (this session)**: the previous guess above for `+0x24`/
   `+0x28`/`+0x2C` was wrong, confirmed by dumping raw struct bytes for both
   examples. `+0x24` is **not** a hash — it is a genuine third callback-
   pointer slot, the teardown counterpart to `+0x20`'s row-handle-array
   builder: example `0x8E3A6BCC` has `+0x24` = raw `0x712920`
   (`Sdb_RowHandleArrayDestroyCallback_Example`), which, given a non-null
   array pointer, invokes each element's vtable slot 0 (`(**v5)(v5, 0)`) at
   an 8-byte stride before freeing the array via the generic allocator —
   exactly undoing what `+0x20`'s builder constructs. Example `0x4CCBB136`
   has `+0x24 = 0` (NULL), consistent with its `+0x20` also being NULL (no
   handle array is ever built for that table, so there is nothing to tear
   down). A full scan of all575 compiled catalog entries found573 cases where
   `+0x28` points to lowercase `"0x"` plus eight hex digits parsing to the
   `+0x2C` DWORD. Entry index10 instead points to `"hands"` and a relocated
   `"backpack"` pointer; entry index19 is all-`0xFFFFFFFF`. The relocation
   table confirms +0x28 is pointer-valued for574 entries and +0x2C for the
   single `"backpack"` exception. The repeated pairing is not a universal
   invariant or semantic name; traced parser/mapper/reset paths do not consume
   either field, so retain them as opaque raw DWORDs.
   **(b) Resolved**: type codes 1/3/9/11 seen in example #2's field array
   were cross-checked field-by-field against the init callback's exact byte
   offsets (`src\Serialization\README.md`) — every write's offset and width
   matches `SdbFieldTypes`' documented storage size for each type with zero
   discrepancy, independently confirming the storage-width table against
   real runtime callback behavior.
4. If a genuine SDB fixture is located within an authorized workspace,
   compare parser/field output against it before claiming compatibility.
5. **`src\Characters\MessageCatalog.csv` has 169 `Character::Commands`
   classes, all now represented by C# native-layout snapshots.** Exact
   same-name file matching finds 163; the six remaining classes use grouped
   helper models (`SlotVisualRequest`, `UnslotAllModulesRequest`,
   `SlotModuleRequest`, `UpdateShoppingList`, `LogActivityPush`, and
   `LogDirectActivityRequest`). See `src\Characters\README.md` for offsets,
   constructor/caller evidence, and explicit unknowns. The raw-image
   constructor pipeline remains reusable for additional classes outside this
   catalog: RTTI string → type descriptor → COL → vtable → raw-byte-search
   the vtable VA for code refs → decompile constructor hits. Keep native
   layouts distinct from wire encodings; selected message encoders have
   separate evidence and regression checks.
6. **Startup follow-up completed:** LookingGlass' secondary
   `nl::ReplayReader::GameInterface` vtable, reader initialization, failure
   path, replay-end auto-quit, and disabled `GoLookingGlass` UI binding are
   documented in `docs\Startup.md`. The file format and other possible entry
   paths remain unknown. `tfWorld` layout and selected virtual behaviors are
   covered by `docs\WorldMode.md`.

   Recent additions: `LootDistributionSetVotes` (219) snapshots a DWORD and
   copied opaque u64 vector; caller `0x6F2C90` deduplicates values from parsed
   records. `UiQueryResponse` (59) snapshots three DWORDs and a copied
   u32-to-u32 map; caller `0x976E80` builds the map from matched UI queries.
   `LootDistributionSetState` (218) snapshots a DWORD and byte with varying
   caller values; `RequestTrackerUpdate` (217) snapshots two positional
   DWORDs and submits on one validated caller branch. `LeaveArc` (214) is
   kind-only with caller `0x927EE0`; `RequestFrameLevelReset` (211) stores
   one DWORD with no found production xref to its populated constructor.

   The previous batch added forty-four message snapshots: `JobLedgerOperation` (215)
   contains a shared-text reference and copied u32 vector; observed callers
   `0x9282C0`/`0x928400` add an absent or remove a matching list value before
   submission. `ExecuteTinkeringPlan` (237) has a DWORD and three copied
   vectors of opaque 16-byte elements. `QueueUnstuck` (178) is kind-only.
   `NPCCombatUpdate` (233), `EquipExperimentalLoadout` (236),
   `Fabrication_Finalize` (246), `Fabrication_Claim` (247),
   `Fabrication_GenerateResult` (245), and `Fabrication_ApplyAction` (244)
   preserve positional scalar fields. For these other than the cited
   JobLedger callers, no direct populated-constructor production xrefs were
   found; meanings and wire formats remain unknown. Constructor/destructor
   names and evidence comments were saved to the client IDB.
   `Fabrication_FetchInstance` (242) adds one positional DWORD at `+0x28`;
   no populated-constructor caller was found.
   `Fabrication_Start` (243) adds a DWORD and byte at `+0x28/+0x2C`, with no
   populated-constructor caller found.

   `Fabrication_FetchAllInstances` (240) is kind-only; constructor `0x53DFB0`
   writes no subclass payload and no direct production caller was found.
   `Fabrication_FetchAllRecipes` (241) is likewise kind-only; constructor
   `0x53E0E0` writes no subclass payload.
   `SetSteamUserId` (235) copies a 64-bit positional value to `+0x28`;
   interpreting it as Steam User ID follows the RTTI class name but lacks
   caller confirmation.
   `FlushCharacterCache` (231) copies five positional bytes at `+0x28..+0x2C`;
   no populated-constructor caller was found.
   `BagInventorySettings` (234) has an optional nested object at `+0x2C`
   with a copied shared-object handle at `+0x30/+0x34` and trailing DWORD
   `+0x38`; its pointer semantics remain intentionally opaque. Nested vtable
   slot 1 resolves to deleting wrapper `0x5A9E20`; reference-pair cleanup
   behavior was not established. `TutorialEventTriggeredCmd` (239) stores
   DWORDs at `+0x28/+0x30/+0x34` and a byte at `+0x38`; `+0x2C` is left
   untouched by both observed constructors, and field semantics remain unknown.
   The elite-level commands `RevertAllEliteLevelsUpgrades` (228),
   `SelectEliteLevelsAward` (229), and `ResetAllEliteLevelsUpgrades_Debug`
   (230) include two kind-only forms and a one-DWORD form; `RunTeamManagerCommand`
   (232) has two bytes and a retained shared-text reference.
   Reward commands `ClaimDailyRewardStreak` (223), `BuyBackPreviousDay` (224),
   and `AcceptRewards` (225) each store one positional DWORD.
   `ClaimDailyRewardItem` (222) has two observed constructors that zero a
   DWORD at `+0x28` but no populated copy form found; `FlushRewards` (226)
   and `RerollEliteLevelsAwardList` (227) are kind-only.
   `RequestMissionAvailability` (171) has three DWORDs; `RequestNewActivity`
   (172) is kind-only; `RequestPushMission` (173) has one DWORD.
   `TimedDailyRewardRequest` (201) and `SeatChangeRequest` (216) each have
   one byte, while `RequestSelfRevive` (209) is kind-only.
   `MapOpened` (109) and `BattleframePurchased` (110) are kind-only;
   `CollectLoot` (111) has four DWORDs; `TempSlotAbilities` (112) initializes
   four zero DWORDs and a `0xFFFFFFFF` sentinel; `SinAcquire_Source` (113)
   has two DWORDs; and `BroadcastWeaponTweaks` (114) has two bytes, a
   reference-counted pair, and a trailing DWORD.
   Challenge messages 94-106 are now modeled with positional fields:
   `ChallengeSwapTeam` has six DWORDs; `ChallengeSetRoleAndTeam` has four
   DWORDs, a byte, and a DWORD; `ChallengeSetMatchParameters` has three
   DWORDs; `ChallengeSetPowerPrivilege` has four DWORDs and a byte; and
   `ChallengeStartMatch` has two DWORDs. A default `ChallengeSetMatchParameters`
   constructor was not confidently resolved. IDs 94-101 are modeled too:
   `ChallengeCreate` and `ChallengeInvitationSquadInfo` copy opaque u64
   identifier vectors, and the latter plus `ChallengeInvitation` and
   `ChallengeKick` retain opaque shared-text references. Challenge
   constructors have been named/commented in IDA; the
   `ChallengeSetMatchParameters` default form remains unlocated. Regression
   checks cover all thirteen Challenge models and passed.

   `PickupCarryableObjectByProximity` (189) and `DropCarryableObject` (190)
   are modeled as opaque 64-bit identifiers at `+0x28`. The pickup populated
   constructor is used by callers `0x93E450` and `0x93E590`; the drop
   populated constructor has no direct static caller in the current IDB.
   `SlotGearRequest` (206) is also modeled with the observed untouched gap at
   `+0x2C`, a positional u32 at `+0x28`, an 8-byte value at `+0x30`, a byte
   at `+0x38`, and a DWORD at `+0x3C`. Caller `0x4D6710` matches a slot-list
   entry before constructing and submitting it.
   `LFGCheckin` (107) and `LFGLeave` (108) are now modeled with two positional
   DWORDs each at `+0x28/+0x2C`. The populated `LFGLeave` form is used in
   `0x6E3060`; no direct populated `LFGCheckin` caller was found.
   `JoinSquadLeadersArc` (213) and `ResourceLocationInfosRequest` (187) have
   no subclass payload in their observed constructors. `LeaveEncounterParty`
   (212) copies two DWORDs. `UpdateChatPartyMembers` (185) copies an opaque
   vector of 16-byte records, followed by DWORDs at `+0x38/+0x3C`, with an
   untouched gap at `+0x34`.
   `MatchQueue` (90) has two copied vectors (8-byte and 4-byte elements),
   an opaque qword, positional scalar fields, and two retained shared-text
   references; its four observed constructor callers are recorded in the
   Character README.
   `ResetTutorialId` (220) and `DismissTutorialId` (221) are modeled with
   their tutorial-ID DWORDs; the observed Lua-facing caller range is 1..14.
   `DeployableHardpointSelection` (181) and
   `ResourceNodeBeaconCalldownRequest` (182) each copy three positional
   DWORDs at `+0x28/+0x2C/+0x30`; populated-constructor callers are documented.
   `CameraPoseUpdate` (177) is modeled with the inherited target and two
   optional, positional payload blocks. The simple constructor is used by two
   camera-pose-change paths; the richer overload has no static caller.
   `AiSignal` (192) and `AiError` (191) preserve their copied opaque fields
   and process-local shared-text references without assigning semantics.
   `LogLongTimeWithoutPush` (176) is kind-only, `GeographicalReportRequest`
   (184) has one positional byte, and `NonDevDebugCommand` (193) copies a
   byte, a DWORD, and a shared-text reference. Character regression checks
   passed and the solution build succeeded with zero warnings/errors.
   Name matching shows **163/169** source-file matches; the remaining six
   catalog names are represented by helper models (`SlotVisualRequest`,
   `UnslotAllModulesRequest`, `SlotModuleRequest`, `UpdateShoppingList`,
   `LogActivityPush`, and `LogDirectActivityRequest`), so all catalog entries
   are modeled. The IDB was saved after naming/comment updates; clean generated
   Character build artifacts before wrapping up.

   Character command coverage is complete for the current 169-entry catalog.
   Next, map another subsystem from the binary RTTI/message catalog. Preserve
   uncertain fields as positional/opaque and avoid inferring wire encodings
   from native object constructors alone.

The earlier stop-for-day boundary was explicitly resumed by subsequent user
requests. Continue with bounded, evidence-backed static reconstruction as
directed; preserve saved source and IDB state, and do not execute or patch the
sample. No recurring session automation is configured. Overall reconstruction
remains incomplete.

Scene-factory follow-up: tfPhysicsDrawInterface has 14 verified in-text vtable slots (slot 9 forwards to slot 8; slot 13 logs the model-release string), its constructor field layout is saved in SceneFactoryNativeLayout, and the 0x64B528 publish stores root[0] into [[0x1E00060]+0xF8] (pointer deref, correcting earlier wording). Scene class identities remain unresolved (indirect via global 0x1DFE970).

## Scene factory named lookup (static)

Global `0x1DFE970` is a statically initialized `oRenderSystem` (vtable
`0x1C7870C`); slot `+0x3C` returns global `0x1DE05FC`, an `oD3DRenderAPI`
(ctor raw `0xF05FE0`, published at `0xED17DC`). Its slot `+0x30` (raw
`0xF255A0`) allocates a `0x18F48` scene and runs `0xF0FCD0(name)`, which
registers it by index in the 32-slot table at VA `0x1EDACD8`. No name-keyed
registry found; create-by-name reading is interpretation. Observed: scene
is `oD3DScene` (vtable `0x1C7A334`, base `oScene` `0x1C77A94`); the name is
copied to `oScene` shared text at `obj+8` by `0xE11E40` (no compare/hash in
the ctor); full table logs "Too many scenes were allocated." but still
returns the object. `0xED196B` stores static `0x1E72DF8` into `0x1DE05FC`
after deleting the previous `0x1DE0600` object (teardown reading is
interpretation); `0xED1AC4` also matches a store pattern but is undecoded.
Details in
`docs\MatrixConnection.md`; constants in `SceneFactoryNativeLayout`, pinned by
`StartupBinaryEvidence`.

## oRenderSystem init/shutdown callers and manager objects (static)
Single direct init caller: raw `0xE53C72` (in `0xE53AC0`, called from `0xE550D5`). Shutdown `0xED1A10` has no direct caller found. Init builds oD3DRenderAPI (`0x1DE05FC`), oD3DTextureManager (`0x1DE0600`), oD3DShaderManager (`0x1DE0604`), oD3DModelManager (`0x1DE0608`); see MatrixConnection.md. Constants in `SceneFactoryNativeLayout`, fingerprint check in `StartupBinaryEvidence`. Unresolved: shutdown caller, init argument meaning.


## oEngineSystem construction (latest pass)

Ctor is raw `0xE4DFA0` (sole caller `0xE4E660` = slot 1 of factory vtable `0x1C77C1C`; factory static at VA `0x1E00460`, list head `0x1E4B0A4` walked by `0x12C3A80` from `ClientApplicationBootstrap` `0x2EAE70` <- `0x2E9630` <- `CrtInitializeAndInvokeClient` `0x136ADBA`). Vtable has 26 slots (see MatrixConnection.md). Slot 0 caller remains unresolved.

## Factory list consumers (engine systems)
- Static pass: the factory lists (0x1E4B0A4 classes, 0x1E4B0A8 instances) have only a create walker (0x12C3A80, slot +4) and a teardown walker (0x12C3AF0, slot +8); no init loop calls oEngineSystem slot 0 (0xE54BF0). The list is a generic singleton registry (Network, SoundThread, fsAssetDB, uiManager, oParticleInterface, ...). Next: find the real caller of 0xE54BF0 (see docs\MatrixConnection.md 'Factory list consumers').

Latest pass: engine-system slot callers (cells 1DFC82C/1E60EA4; slot 0 caller unresolved) and crash hook setter 0x12C38D0 / RedHanded trigger 0x2EAEBF; see MatrixConnection.md and CrashHandler.md.

## Session handoff — selector 0x76 consumer

- `0xACE210` sends each `0x40`-byte record through `0x6E2590`. With record
  `+0x14 == 0`, `+0x0C` is notification text, `+0x08` author, `+0` author ID,
  `+0x10` selects the native channel label, and `+0x11` supplies event335
  display flags. The shared display path applies the existing channel gates,
  dispatches `335`, and writes the chat file log.
- With `+0x14 != 0`, the consumer requires a current chat and calls
  `0x6E3670` with `+0x08`, `+0x10`, `+0x12`, and the object at `+0x18`.
  That helper reads storage-object data and size and switches on `+0x12`;
  the 19 targets are now mapped in the structured-chat follow-up below.
- The parser/receiver `slBlob` versus range-endpoint mismatch is unchanged and
  remains static evidence only; no runtime outcome is claimed. The build and
  all 1,427 startup/binary checks passed against the original executable.
- Next: resolve the nonzero `+0x12` switch if static evidence is available;
  otherwise continue with selector `0x75` tree semantics or selector `0x77`
  state extent. See `docs\MatrixConnection.md` and the matching
  `CharacterAbilityApplicationNativeLayout` constants/checks.

## Structured-chat switch follow-up

- Original-binary disassembly resolves all 19 entries of the `0x6E3670`
  switch: table VA `0xAE5254` maps to raw `0x6E4654`, and every target is
  translated through the PE sections. The IDA database's old function ends
  at raw `0x6E3B12`; later case bodies are not assigned to a function. Do not
  interpret the decompiler's invalid-table warning as invalid sample bytes.
- Saved `StructuredChatMessageNativeLayout.cs`, the complete selector/reader
  map, exhaustive byte-selector selection checks, binary table/call/literal
  evidence, and detailed notes in `docs\MatrixConnection.md`. Cases7/8 share
  a target but differ in whether they notify before removing an entry.
- Literal-backed paths include `CHAT_DUEL_RESULT` localization, code353 with
  `player_name/item_type_id/quality`, code266 with `inviter/player`, and
  code345/346 selected by the decoded pending-state flag. Numeric event names
  remain unassigned. This does not resolve the selector0x76 representation
  conflict.
- Next: reconstruct individual payload readers (`0x6FDBB0`, `0x6FDC30`,
  `0x6FDD50`, `0x6FE240` are small starting points), then follow owner
  helpers. The native caller ignores the switch's Boolean result; returning
  true alone is not evidence that an event was dispatched.

## Structured-chat simple payload follow-up

- Seven simple readers (selectors1/2/3/4/11/15/16/19) now have a field ledger
  in `StructuredChatMessageNativeLayout.PayloadFields` and
  `docs\MatrixConnection.md`: 15 primitive calls, destination offsets,
  native widths, and wire widths. Strings are raw NUL-terminated bytes.
- Helper `0x3849C0` reads one wire byte and zero-extends it into a native
  DWORD; do not treat native alignment or field width as wire padding.
  Minimum consumed sizes are 2,10,6,1,10,4,1 bytes for the seven readers.
- Saved the already-decoded case19 pending decision as a C# capture.
  Every nonzero byte selects345, owner collection+34, and `PENDING_IN`;
  zero selects346, collection+28, and no status property. Removal result is
  ignored by the native caller, so absence does not suppress notification.
- Next: nested readers `0x709750` (case5), `0x709560` (case9), `0x703910`
  (case10 optional record), `0x705570` (case12 chunked map), plus the
  case14/17 collection helpers. Keep these consumer payloads separate from
  selector0x76's still-unresolved parser/receiver representation mismatch.

## Structured-chat optional-value follow-up

- Case5's nested optional (`0x709650`, destination+28) and case10's optional
  (`0x703910`) now have exact wire/native ledgers and binary checks.
  Any nonzero presence byte reads14/28 value bytes (15/29 with presence);
  zero consumes one byte and clears only an existing native flag.
- Case10 has a four-byte native alignment gap before its QWORD, but no
  corresponding wire padding. Its assignment helper `0x6F7D20` copies
  fields, not that padding.
- Case5 initializes/reads14 temporary bytes and copies16 via SSE on both
  assignment paths. The two-byte tail has no write on the traced local path.
  Do not model it as extra wire bytes or claim a runtime consequence.
- Saved constants, ten field descriptors, consumed-size helpers, exhaustive
  presence checks, and source fingerprints in the existing structured-chat
  layout/checks. Next: remaining case5 prefix/suffix fields, case9's
  `0x709560`, case12's `0x705570`, and case14/17 collections.

## Structured-chat collection and case5 follow-up

- Case5 now has all13 fixed primitive fields, its optional member, and wire
  consumption arithmetic recorded: text length+37 absent, +51 present.
  Native gaps are not serialized padding. The final field is a WORD.
- Cases9/14/17 collection readers are mapped in
  `StructuredChatCollectionNativeLayout.cs`: QWORD entries, DWORD/WORD/raw
  four-byte entries, and DWORD entries respectively. They use count-byte
  groups and continue only after255; a shorter final group needs no extra
  zero terminator.
- Case14 consumes10 bytes per entry but appends12 native bytes; entry+6/+7
  are native padding. Case17 consumes its trailing byte only after the
  complete chunk sequence. All field semantics remain conservative.
- Next: case12's `0x705570` chunked map, nested `0x7035C0` DWORD vector,
  and `0x70A340` map. Its decompiler has a JUMPOUT and a path that needs
  direct instruction tracing before a complete ledger can be claimed.
  The selector0x76 parser/receiver representation conflict is still open.

## Structured-chat nested map follow-up

- Case12's `0x705570` payload is now traced: counted outer entries contain
  a DWORD key, counted DWORD vector (`0x7035C0`), and counted DWORD key set
  (`0x70A340`). All three use count255 continuation; an empty map is one
  byte, a single entry with empty nested collections is seven bytes.
- The key-set JUMPOUT is an exception branch, not missing decode code.
  Existing-key insertion flag0 goes through `_CxxThrowException`; new
  keys get flag1. The set is cleared before the first chunk only, so
  duplicate checks span continued chunks of that set. Outer keys have a
  different lookup/assignment path; do not generalize this policy to them.
- Original PE metadata identifies a thrown pointer to
  `sl::serialize::unpack_exception` (`.PAVunpack_exception@serialize@sl@@`);
  the call passes the address of an allocated pointer, not a by-value
  exception object. Binary checks cover the insertion polarity, imported
  throw thunk, and PE-translated ThrowInfo/catchable-type chain.
- No runtime/caller-visible failure is inferred. All19 switch targets and
  their payload shapes now have traces; next semantic work is the owner
  comparison/update helper family (`0x6FE010`, `0x6F9840`, `0x6F6420`),
  named subscribers for numeric events, or the pre-existing parser/receiver
  mismatch. Read-only IDA works through the existing `ida_rpc.py` helper
  even though this session's built-in MCP client initialization fails.

## Structured-chat case12 state update follow-up

- Verified case12's owner state at `+0x1FC`: unsigned-QWORD tree keys at
  node `+0x10`, stored map at node `+0x18`. `0x6F6600` is a key count,
  not the decompiler's apparent constant0: `0x701630` writes a local through
  ECX before the caller reloads it. The original instructions are checked.
- Equality `0x6FE010 -> 0x6E2060 -> 0x6FD3E0` is outer-key lookup plus
  positional DWORD vector equality plus inner-set membership equality.
  Vector order matters; outer map and inner set iteration order do not.
- Existing equal maps skip assignment/notification; existing changed maps
  assign and notify. A missing owner key with a nonempty decoded map
  assigns and notifies; an empty decoded map still creates/stores an entry
  but skips notification. The post-decode C# capture records this decision.
- Numeric event268 carries the same QWORD as `chatId` and `entityId`.
  Named subscribers and the gameplay meaning remain unresolved.
- Normal copy paths replace rather than merge: repeated outer payload
  keys get the later vector/set, including clearing on empty input.
  The owner-map copy clears its old entries too. No allocator failure
  recovery, valid runtime delivery of selector0x76, or wire parser is
  claimed.
- Next focused trace: resolve `0x48C2B0`'s case12 prerequisite and its other
  structured-chat callers, or find event268's named subscriber. Keep the
  original selector0x76 `slBlob`/record-range mismatch separate from these
  already-decoded consumer decisions.

## Structured-chat shared prerequisite follow-up

- Resolved `0x48C2B0` in cases9,10,11,12,13,18. Each case tests AL before
  handling. Cases9,10,11,12,18 return false at `0x6E3AF2` on failure;
  case13 skips its body but returns true via `0x6E4583`. Do not generalize
  the failure return across all gated cases. The current IDA xref
  list omits these switch bodies; original bytes verify all six calls.
- The helper requires nonzero DWORD `+0x118` and an unsigned-greater-than1
  result after signed division of wrapped `(end+0x110 - begin+0x10C)` by104.
  For valid ranges it requires at least two104-byte entries. It is not
  range/order validation: negative quotients can pass the unsigned test.
  The C# capture preserves subtraction wrap, signed division, and that
  final unsigned comparison, including boundary/reversed-range cases.
- Still unknown: the owner word and104-byte entry semantics. A next bounded
  investigation can trace their writers/constructors, or event268's
  subscriber names, without conflating them with selector0x76's conflicting
  parser-side representation.
- Current saved validation: the solution builds without warnings/errors;
  all1,519 startup/original-binary checks pass. Analysis remained read-only
  for both the executable and IDA database.

## Structured-chat cases13/18 consumer follow-up

- `0x6E9F40` compares owner `+0x120/+0x124` with `+0x08/+0x0C`, including
  equality for zero pairs. Case13 requires equality; case18 requires
  inequality. Neither field pair's semantic identity is established.
- Case13's owner `+0x170` cache is keyed by NUL-terminated text bytes.
  Hit lookup `0x6FD280` refreshes a separate key-list position through
  `0x700680`, without notifying. Miss dispatches event265 with `player`
  text before storing the caller QWORD plus text through `0x6FD0E0`.
  Reentrant subscriber behavior remains open; capacity/eviction was
  resolved in the follow-up below.
- Case18 also requires caller QWORD equality with owner `+0x120`; the
  helper `0x37C5F0` is two DWORD comparisons, not membership lookup.
  Its success path displays localized `TIP_JOIN_ANOTHER_INSTANCE` in
  category `notification`, then returns false. Case13 returns true even
  when its gates skip all processing. Do not interpret the dispatcher
  Boolean as notification delivery.
- Saved C# captures preserve the gated lookup/refresh/notify-before-store
  decisions and displayed-but-false return without implementing a parser
  or reproducing native allocations. Binary evidence anchors the exact
  comparisons, branch polarity, literals, list update, and call order.
- Next bounded targets: owner `+0x120`/`+0x08` writers and the104-byte
  entry type. The unresolved selector0x76 parser-side
  representation still limits claims about actual consumer delivery.

## Structured-chat text-cache capacity follow-up

- Miss insertion `0x6FD0E0` trims only when cache byte `+0x14` is nonzero.
  It passes unsigned `capacity+0x10 - 1` to `0x700570` before adding the
  new entry. Capacity0 wraps the target to `0xFFFFFFFF` and requests no
  eviction; this is not a native input-rejection branch.
- Trim removes keys from the list head, erasing corresponding text-tree
  entries through `0x702F50 -> 0x6F9310`. New entries and hits append to
  the tail. This establishes least-recently-accessed eviction, not
  lexical-key order. Tree/list count assertions guard the resulting
  count. Default capacity/enablement and allocation-failure behavior
  are not fabricated.
- C# records only the pre-append trim target and removal count, preserving
  disabled, positive-capacity, capacity1, and unsigned-wrap behavior.
  It does not claim successful allocation or append for arbitrary counts.
- Validation after these follow-ups: build has zero warnings/errors and
  all1,569 startup/original-binary checks pass. The original executable and
  IDA database were only read; all saved changes are C#, checks, and docs.

## Companion-binary cross-check and dump handoff

- User-directed expansion to the other eight files: all nine IDA instances
  are reachable on ports13337..13345. Queried companion instances by their
  direct local endpoints without changing the selected database or writing
  database metadata. New addresses in companion notes are PE VAs, not client
  raw offsets.
- Added `docs\CompanionBinaryConnections.md` with every sample size/hash,
  import/export connections, native metadata field ledger, and remaining
  inference limits. The live client differs from `.original` before this
  work; tests deliberately use the immutable original-client hash.
- RedHanded's eleven actual IDs are `0x10000`, `0x10002..0x1000B`, not the
  truncated low values printed by the decompiler. Each typed record is CRLF
  terminated and a final blank CRLF line completes input. Parent waits before
  closing its writer, so EOF cannot be the receiver's completion contract.
- Receiver skips repeated separators and preserves the third-field remainder,
  including later pipes. Existing `R5DumperUserInfo.Parse` is explicitly a
  convenience subset, not native-equivalent. No new parser was implemented.
- `r5dumper` builds12-byte native descriptors with unchanged types, sizes4/8
  for integer buffers and string-length+1 for text. It passes null exception
  and callback information to the real dbghelp writer export (ordinal31,
  RVA `0x5E800`). Clarified file-deletion differences between OpenProcess
  failure and dump-writer failure.
- DumpTruck has a separate2048-byte HWStats stdout pipe; actual byte counts
  go to sink slot `+0x18` with a final NUL. Do not mix this with dump-user-info.
- Verified the exact FirefallProcess child import against Awesomium's export,
  and all86 original-client Awesomium imports against the supplied DLL.
  DeploymentCommon has a CLR header and managed bootstrap, not an observed
  native static-import edge in the checked client/helpers.
- New `tests\CompanionTools` project runs pure decisions without samples,
  or hash-guarded cross-binary evidence with `--samples <system-bin-directory>`.
  Next: trace stream-owner initialization, RedHanded debug handler arguments,
  HWStats compression sink, or DeploymentCommon's managed callers.
- Validation: solution build has zero warnings/errors;46 companion checks
  pass with all nine samples,19 pure checks pass without samples, and the
  existing1,569 startup/original-client checks remain passing.

## Companion metadata source names

- Followed RedHanded wWinMain `0x404540` from option locals through object
  initialization (`0x404A72..0x404ACE`) and monitor this-pointer setup at
  `0x404B36`. Owner `+0x20/+0x24/+0x28/+0x2C/+0x30` are application
  name/source revision/build number/environment mnemonic/environment name.
- `atoi` feeds source revision and build number. CMOVE at
  `0x4049CA/0x4049DC` replaces zero with117123/1962; negative nonzero
  values keep their DWORD bit patterns and later use unsigned `%u`.
  Added a pure post-atoi capture; it is not a native CRT parser.
- Debug loop `0x409880` calls the metadata handler in exception and
  RIP-event paths. Both forward `DEBUG_EVENT.dwThreadId`, proven by event
  base `[ebp-0x70]` and argument `[ebp-0x68]`. The handler saves argument1
  unchanged and formats it as stream `0x1000A`: thread ID, not exception
  code. Updated ledger, source names, and hash-guarded instruction checks.
- Next: resolve source-text `0x10006` and process-memory `0x10008`, or
  follow DumpTruck's hardware-report sink into compression.
- Validation: solution build has zero warnings/errors;57 companion checks
  pass with the nine samples and26 pure checks pass without samples.

## Companion source bytes and memory metrics

- RedHanded source stream `0x10006` comes from exception parameters, not
  a proven codec. Codes `0x012ED500/501` require parameter count >=2;
  parameter0 is byte count, parameter1 is remote address. Length <=1024
  reaches ReadProcessMemory (including zero); larger length is
  `<<TOOLONG>>`, failed read is `<<BADMEM>>`, unrelated exception is
  `EXCEPTION`, and null/insufficient-parameter input keeps
  `<<UNSPECIFIED>>`. Code501 selects flags2 before count/length checks.
- Added `CrashMetadataNativeLayout.cs` to preserve those gates without
  accessing target memory. The native caller checks the API BOOL but
  supplies no bytes-read pointer. `%s` output still truncates embedded NUL
  and does not escape CRLF.
- Preferred stream `0x10008` helper `0x406B60` pairs PDH Virtual Bytes with
  ID Process counters, returning the full EDX:EAX QWORD (the decompiler's
  LONG return type is misleading). Caller `0x407A3B` falls back only when
  both halves are zero. Fallback reads x86 PROCESS_MEMORY_COUNTERS
  PagefileUsage at+0x20 and zero-extends it, without checking API BOOL.
  Preserve the distinct metrics and the observed post-call field value.
- Stream `0x10009` separately chooses the unsigned full-QWORD minimum of
  total-page-file and total-virtual memory, or zero on status failure.
- Next bounded target: DumpTruck HWStats sink slot+0x18 and compression,
  or the original client's source-exception producer. DeploymentCommon
  managed loading remains a separate unresolved connection.
- Validation: solution build has zero warnings/errors;400 companion checks
  pass with the nine samples and364 pure checks pass without samples.

## DumpTruck hardware blob and compression layer

- Submit `0x40AC1A` supplies slBlob vtable `0x4BDF44`; sink slot+0x18
  resolves to append `0x41BAE0`. It grows by count and copies exactly
  count bytes, so HWStats's final NUL is included in the subsequently
  compressed input range. This is accumulation, not per-pipe-read compression.
- Only generator exit0 creates hardware report layer3. Nonzero exit logs
  and omits that layer but continues packaging. Compression false logs,
  closes/destroys layer/blob and takes submit failure cleanup.
- Compressor `0x409540` initializes56-byte z_stream (version1.2.8) and
  allocates0x4000 scratch, but literal avail_out is4 at `0x4095E5`.
  Z_FINISH4 is separate from that output count. Only return0/1 is accepted;
  appends4-avail_out bytes, repeats on avail_out0, otherwise returns success.
  Added exact pure decision captures rather than replacing the4 with16KiB.
- Layer begin `0x426CD0` writes DWORD type and DWORD placeholder length;
  close `0x426D30` patches current-position minus payload-start at
  payload-start minus4. Hardware header is type3/length/data at+0/+4/+8,
  with little-endian DWORD fields and payload-only length. Normal report
  completion writes DWORD0 before finalizing the serializer.
- Saved `HardwareReportNativeLayout.cs`, matching native-byte checks, and
  detailed ledger. Outer report headers/other layer bodies still need a
  separate trace. No target, compressor, or report uploader was executed.
- Validation: solution build has zero warnings/errors;457 companion checks
  pass with the nine samples and414 pure checks pass without samples.

## DumpTruck outer body and optional-file paths

- Body serializer starts on the new request's empty blob at+0x2C; first
  explicit write `0x40A879` copies magic `0xDEFEC8ED` (bytes ED C8 FE DE)
  from VA `0x476314`. Producer layer order is1/core,4/application name,
  2/main log,5/awesomium.log,3/hardware, then DWORD0 with no length field.
- Type4 is uncompressed owner+0x0C text plus NUL, even if empty.
  Initialization proves app->+0x0C and msg->+0x10; do not call it a
  message layer. Conditional layers and open/compression-failure behavior
  are documented in `docs\DumpTruckReport.md`.
- File compressor `0x409660` allocates16KiB input/output but requests4
  input bytes and advertises4 output bytes. feof chooses flush0/4.
  Only zlib result-2 takes its explicit failure branch, unlike memory
  compressor0/1 acceptance. Full output drains again; space plus second
  EOF query determines more input versus success. C# captures this branch
  behavior rather than running zlib or implementing a compressor.
- Optional logs use signed ftell >0x40000 to select prefix/marker/tail
  clipping into a256KiB logical blob (2048-byte prefix request). Then
  memory compression is used; <=threshold, including negative observed
  ftell, selects file compression. No log-specific trailing NUL append
  is observed; do not apply HWStats's trailing-NUL rule to logs.
- Saved `CrashReportNativeLayout.cs`, new focused report documentation,
  and hash-guarded instruction checks. An authorized offline save_poo
  fixture remains the smallest independent validation of emitted bytes;
  no target process or network operation was used to generate one.
- Validation: solution build has zero warnings/errors;562 companion checks
  pass with the nine identified samples and512 pure checks pass without
  samples. All changes in this batch are C#, checks, and documentation.

## Original-client exception handoff to RedHanded

- Original helper raw `0x12C3470` prepares four DWORDs: source strlen/pointer
  and message strlen/pointer. Null inputs select empty literals; lengths
  exclude NUL and are not capped here. Import VA `0x1AC41B0` is verified
  as KERNEL32!RaiseException in the original PE.
- Fatal call raw `0x12C35CF` uses500/flags1 after IsDebuggerPresent.
  Assert call raw `0x12C36DF` uses502/flags0 after the same debugger gate;
  formatted assert raw `0x12C3972` uses502/flags0 with source ASSERT and
  formatted stack text. A direct DWORD scan found no501 immediate in the
  original client, not proof of no computed/other producer.
- RedHanded message decoder `0x4088A0` accepts500..502, requires4 parameters,
  reads message at record+0x1C/+0x20 with length <=1024. Its source decoder
  accepts only500/501 with count2. Thus assertion502 source becomes EXCEPTION
  even though its message is read. Added C# pure captures and byte checks.
- Snapshot502 enters state1 only from0/2 and joins metadata capture; the
  subsequent target-termination check is state3. Next task is all state
  writers and unsigned2000ms throttle rather than guessing state2.
- Validation: solution build has zero warnings/errors;848 companion checks
  pass against the nine samples and793 pure checks pass without samples.

## Snapshot admission and fatal throttle

- RedHanded helper `0x409850` admits state0 or unsigned modular
  timeGetTime-ownerTick >2000; equality2000 suppresses. First-chance fatal
  handling duplicates it; second-chance fatal calls the helper.
- Fatal join `0x4099C1` sets state3 and replaces tick using a second clock
  call before testing eligibility. Metadata may be suppressed, but shared
  state3 check still terminates the monitored process with0xFFFFFFFF.
- Snapshot502 admits only prior0/2, sets1 and updates tick, with no elapsed
  gate in that branch. Other states preserve their values; prior3 still
  reaches termination. Do not equate time passage with snapshot readmission.
- Added pure branch/transition captures and boundary/wraparound checks.
  State2 writer/lifecycle remains a future trace, explicitly not named from
  its admission role alone.
- Validation:906 companion checks pass with all samples and849 pure checks
  pass without samples. Next independent trace is client debug-string
  LOGPATH/DUMPAUTH emission; managed DeploymentCommon trace is delegated.

## Debug-string LOGPATH and DUMPAUTH handoff

- Original client raw `0x12BBC60` composes console.log and emits
  RED5-LOGPATH=%s newline at `0x12BBD55`. Raw `0x63AEF0` emits
  RED5-DUMPAUTH=!%s newline at `0x63B15B`. Both gate on IsDebuggerPresent;
  IAT VA `0x1AC434C` is independently verified OutputDebugStringA.
- RedHanded event8 consumer requires Unicode WORD0 and reported length>5,
  allocates length+1, reads exactly the reported16-bit count via
  ReadProcessMemory, and checks BOOL. No1024-byte cap applies here.
- Successful reads trim trailing NUL/CR/LF, then require case-sensitive
  RED5- prefix and LOGPATH=/DUMPAUTH= fields. Bare LOGPATH/DUMPAUTH are
  not recognized by this consumer (older doc corrected). Suffix offsets
  13/14 update owner+0x0C/+0x10. The literal exclamation in DUMPAUTH
  is retained; no actual credential values were read or saved.
- Saved `DebugStringHandoffNativeLayout.cs` read decisions and native checks.
  Next: downstream exclamation-marker handling, remaining snapshot-state
  writers, or other producer sites. DeploymentCommon agent is independent.
- Build succeeds;939 checks pass with all samples and877 pure checks pass
  without samples. Next bounded investigation is downstream auth forwarding,
  retaining opaque values and without contacting the legacy services.

## DeploymentCommon managed WOW64 follow-up

- Independent read-only agent traced Wow getters `0x06000053/54` and
  ModuleContainsFunction `0x06000055`. Process bitness is IntPtr.Size==8;
  OS bitness short-circuits x64, otherwise requires export availability,
  successful IsWow64Process, and a true out flag.
- ModuleContainsFunction uses GetModuleHandle/GetProcAddress, not LoadLibrary.
  False may indicate missing export/API failure, not solely32-bit Windows.
- Saved pure `WowNativeLayout.cs`, [IL/metadata ledger](DeploymentCommonWow.md),
  and complete IL byte checks. Added DeploymentCommon project reference to
  CompanionTools (its first managed companion behavior checks).
- Build succeeds;958 checks pass with all samples and893 pure checks pass.
  Agent has finished; no additional agent work is scheduled by this batch.
- Follow-ons: NativeMethods.ClientToScreen/MoveWindow or
  Utils.SHGetSpecialFolderPath, independently of native auth forwarding.

## DumpTruck opaque auth forwarding

- Verified RedHanded owner+0x10 -> --auth= variadic launcher pair at
  0x404C90/94; direct CreateProcessA, DETACHED_PROCESS, no command shell.
  Correct IAT slot is0x5442E8 (verified against the native call/import).
- DumpTruck options+8 auth has a forge-byte+0x14 AND empty gate before
  stored-credential helper0x404850. Submit handler independently calls
  0x4024E0 only if auth is empty: this reads registry LoginName, **not**
  a prompt. Nonempty output gets literal ! prefix; empty still continues.
- Important constructor boundary:0x409830 is not a plain auth copy.
  It invokes0x409460 to escape &,<,> in that order before storing at its
  own+8. Request formatter0x409B8C uses this preescaped field in report XML;
  exclamation is not stripped in this traced path.
- Saved pure fallback decisions and C-string-visible XML transform with
  synthetic fixtures. No actual registry values, credentials, uploads,
  or target execution involved. Full stored-credential processing and
  server semantics remain unresolved.
- Build succeeds;981 sample-backed checks and910 pure checks pass.
  Next native trace: who writes debugger owner state2 after a snapshot.

## RIP_EVENT and bounded state2-writer search

- Monitor0x406820 calls the debug loop and cleans up its process helper.
  No state2 writer was identified in the launch monitor, debug loop, or
  reviewed dump-capture helper. Do not equate state2 with completed/ready.
  Further alias/data-flow work is still needed; the three generic
  immediate2 stores are not linked to the debugger owner.
- Adjacent concrete result: RIP_EVENT9 uses state0/2 admission but no
  time throttle, passes a null exception pointer to0x407630 at0x409A86,
  and terminates withFFFFFFFF at0x409AA1 regardless of admission.
  Unlike fatal exceptions, this branch does not setstate3 or update tick.
- Saved pure CaptureRipTransition and byte/branch checks. Pending work
  remains state2 writer aliasing or managed native window/path helpers.
- Build succeeds;988 sample-backed checks and916 pure checks pass.

## DeploymentCommon native utility boundaries

- Method tokens104/105 are user32 ClientToScreen/MoveWindow imports.
  No direct call/callvirt/function-pointer token candidate found in managed
  method bodies; do not infer a window-positioning algorithm.
- Utils.GetCommonPath token128 body RVA6A10 (IL RVA6A1C, raw4C1C)
  creates capacity260, calls shell folder query with fCreate=false,
  **pops BOOL**, and returns builder text on normal return. False alone
  does not invoke managed fallback.
- Catch System.Exception protects IL0/34, handler34/50; logged exception
  flow reaches Environment.GetFolderPath backup. Shortcut routines use
  shell/backup pairs25/16 and22/11; complete COM shortcut logic untraced.
- Saved NativeUtilityNativeLayout and documentation. Companion SampleImage
  now reads managed import metadata, IL, and catch regions without loading
  assemblies; also guards the prior WOW64 P/Invoke signature/flags evidence.
- Build succeeds;998 sample-backed checks and921 pure checks pass.
  Complete managed shortcut creation/removal is a follow-on, not yet
  implemented or inferred from the shell directory pairs alone.

## DeploymentCommon shortcut creation/removal

- Utils.CreateShortcut125 body RVA66A0 has423 IL bytes; RemoveShortcuts126
  body RVA6854 has429. Complete IL checks saved with corrected native
  transcription (user cleanup's product combine has no String.Format).
- Creation saves three links: desktop play, Start Menu product play, and
  Start Menu uninstall. Play targets Launcher.exe but uses FirefallClient.exe
  as icon; uninstall targets uninstall\FirefallInstaller.exe with /uninstall.
  Null descriptor icon/arguments means no setter, not property clearing.
- Removal processes selected common-or-fallback folders, then explicitly
  user DesktopDirectory/StartMenu. Existing links only; directories only
  when existing and empty; recursive=false. No deduplication across passes
  and no method-local catch. Reconstructed code has no side effects.
- Created reusable read-only session scratch managed-il.ps1 to resolve IL
  opcodes/member/string tokens with framework metadata APIs, no target load.
- Follow-ons: ProductIdentifierDisplay mapping and install-root data source;
  Awesomium process-role agent is independent of this managed trace.
- Build succeeds;1013 sample-backed checks and933 pure checks pass.

## DeploymentCommon product identity and install root

- ProductIdentifier2A is constant Firefall_Beta; display2B is Firefall.
  Shortcut labels are therefore Firefall in this sample despite the beta
  registry product key.
- GetInstallLocation31 body RVA2658 prefers present nonempty WPF installDir,
  castclass string; nonstring overrides throw. Otherwise GetInstallRegistryKey44
  queries HKLM product InstallLocation and uses isinst string, yielding null
  for nonstring/missing values.
- Both selected string branches remove every exact " --culture" substring.
  Not an argument parser: "=en-US" suffix survives. Pre-replacement nonempty
  override may become empty without registry fallback. Null/empty distinct.
- Saved pure single-observation model, synthetic edge cases, complete method
  IL and user-string checks. Repeated native WPF reads/races and OS access
  errors are not modeled or silently caught.
- Build succeeds;1030 sample-backed checks and947 pure checks pass.

## Awesomium exported child gate and process roles

- Independent agent finished the cross-binary trace; C++ ChildProcessMain
  export ordinal217 RVA3A710 is imported directly by FirefallProcess.
  `_Awe_ChildProcessMain@4` ordinal339 RVA4C840 is a wrapper, not the
  imported entry. No export literally named AwesomiumProcessMain.
- Parsed type must have nonzero length before entering content main;
  absent/empty returns0. Byte checks corrected initial report offsets:
  length compare1003A6B1, entry test1003A721, call1003A719.
- Exact role table1124D400 has8 entries/stride8, key+0/callback+4:
  empty, renderer, plugin, worker, ppapi, ppapi-broker, utility, gpu-process.
  Empty-key BrowserMain mapping is excluded by this exported entry gate
  but may be reachable by other entry paths.
- Unknown nonempty type invokes a supplied delegate's vtable+10; only
  no-delegate fallback returns1 with unknown-type diagnostic.
- Saved table constants and pure split entry/dispatch decisions plus
  sample-backed export/IAT/table/branch checks. Agent is finished/idle;
  no callback internals, argv parser, or delegate implementation traced.
- Build succeeds;1086 sample-backed checks and991 pure checks pass.

## DeploymentCommon permissions launch descriptor

- Utils.ResetInstallFolderPermissions12F body RVA6E18 builds a system-folder
  icacls.exe path (SpecialFolder37) and quoted-directory /RESET /T /Q /C args.
  CreateNoWindow=true, ErrorDialog=false, UseShellExecute=false.
- Process.Start Boolean is popped. No WaitForExit or ExitCode observation:
  method return does not prove permissions changed.
- System.Exception catch IL0/79, handler79/50 logs exception message via
  LogLine and LogPersistentEvent("Permissions",...). It cannot observe
  later child exit failure.
- Saved inert PermissionLaunchDescriptor, full IL/string/catch checks,
  and synthetic inputs. No process or permission operations performed.
- Build succeeds;1092 sample-backed checks and995 pure checks pass.
  Next independent work: managed INI wrapper boundaries; the browser type
  extraction agent has a separate read-only assignment.

## DeploymentCommon INI wrapper boundaries

- IniFile constructorD8 directly stores supplied path. Read importE2 is
  uint/char[] with flags0106; writesE3/E4 are uint with0100. No SetLastError.
- GetStringDA RVA5328 starts512, doubles only for count==nSize-1 through
 32768, then throws OutOfMemoryException if still saturated. Count0 null;
  other positive counts copied exactly. GetterD9 advertises32766.
  No nSize-2 retry branch, including multi-string enumeration callers.
- GetBoolDD uses instance Equals true/1, OrdinalIgnoreCase. Other text
  returns caller default (including false/0); null causes NullReferenceException
  before default. Two-arg overload uses false default.
- GetSections/GetSectionKeys split NUL with RemoveEmptyEntries, null=>empty
  arrays. KeyExists requestedKey is instance receiver: null throws only
  if a candidate is visited. SetString returns native result!=0;
  DeleteSectionKeys writes empty-string section content and ignores result.
- Saved pure read/default/key/write decisions and full IL/import checks.
  No target config read/write or INI parser implementation.
- Build succeeds;1145 sample-backed checks and1043 pure checks pass.

## Awesomium bare/empty parsed switch values

- Independent type-extraction agent finished one new bounded policy:
  recognized --type and --type= both store empty values; the parser does
  not use a following standalone renderer token as the value.
- Splitter1028BC60 initializes value length+10 to0 at1028BC98, skips
  suffix copy when equals absent at1028BD26; map-copy sequence1028D8A5.
  Windows wide command-line/argv calls1028E234/1028E19E.
- Saved inert length/next-token decision, native byte/literal checks,
  and evidence doc. No argv parser implementation. Prefix exclusivity,
  duplicate-switch precedence, and name normalization remain unresolved.
- Agent is finished/idle; no further agent task has been started here.
- Build succeeds;1158 sample-backed checks and1053 pure checks pass.
  Next independent file: HWStats output evidence beyond its saved pipe
  and DumpTruck type3 compression handoff.

## HWStats DriveSpace source and float/output decisions

- Corrected old human label:4037F0 is DriveSpace, not memory. It scans
  C-Z fixed-drive type3, calls403690, and uses GetDiskFreeSpaceExW's
  FreeBytesAvailableToCaller field, not total size or total free.
- Helper failure zeros both output floats. Successful helper clobbers EAX;
  caller ignores that return and uses output slots. No real API called.
- Arithmetic retains signed low-shift int->float and extra high>>8 term
  in GB output. Synthetic2^40 bytes yields MB1048576/GB1025. Stored
  source is instruction-equivalent, not a generic bytes/GB conversion.
- Drive and total lines require GB>1 else MB>1 else none. Exactly1GiB
  prints using MB; exactly1MiB suppresses row but still contributes to
  totals because accumulation occurs first.
- Saved DriveSpaceNativeLayout and byte/import/constant/format checks.
  Follow-ons: RAM %ull format/source and system-affinity popcount;
  OS/DirectX/BIOS registry key or NETAPI MAC helper.
- Build succeeds;1187 sample-backed checks and1078 pure checks pass.

## HWStats RAM variadic arguments and system-affinity count

- DriveSpace batch committed/pushed as8a5a180.
- Instruction-verified4035A0: memory BOOL ignored, only dwLength64
  initialized, ullTotalPhys+8 used regardless. No invented failure default.
- Threshold uses low32 of totalPhys>>20 >=1024. Small path pushes original
  physical bytes, not MB, with literal `ram = %ull MB\n`; saved argument
  descriptor deliberately does not implement CRT formatting.
- GB path uses corrected unsigned DWORD conversion, float rounding/scaling,
  then double vararg promotion. Synthetic2^52 bytes wraps threshold input
  to0;2^51 correctly takes unsigned GB path.
- Affinity BOOL also ignored; both masks initialized0. The loop counts
  system mask bits, not process mask, and has zero high DWORD (max32).
- Saved RamAndProcessorNativeLayout, exact native checks and docs.
- Build succeeds;1205 sample-backed checks and1092 pure checks pass.
- Follow-ons: BIOS key406F30 and OS description403A20; NETAPI MAC helper.
  State2 writer in RedHanded remains unresolved, not a named ready state.

## HWStats BIOS registry root and typed output

- RAM/affinity batch committed/pushed as8e8adee.
- System section's five calls40415B/16F/183/197/1AB share literal
  HARDWARE/DESCRIPTION/SYSTEM; writer4030F0 calls406F30 with no middle
  component and a1024-byte output buffer.
- Slash-copy406EC0 maps ASCII '/' to backslash only. Opened path retains
  trailing backslash; RegOpenKeyExA uses HKLM and KEY_ALL_ACCESS F003F.
- Helper queries value/type, copies type even on query failure, closes
  key and maps nonzero open/query status to80004005. Open failure leaves
  caller type output uninitialized; no invented failure type in capture.
- Writer admits only type1/7 and successful helper status; prints raw
  narrow `%s = %s\n` without multi-string joining or terminator insertion.
- Saved BiosRegistryNativeLayout and exact five-call/native checks.
- Build succeeds;1237 sample-backed checks and1114 pure checks pass.
  Follow-ons: OS description403A20 and NETAPI MAC helper.

## HWStats MAC source and first-success policy

- BIOS registry batch committed/pushed as529fc73.
- Traced4041D0: Netbios NCBENUM37 (256-byte stack buffer), then per LANA
  NCBRESET32 and NCBASTAT33 with wildcard and600-byte shared buffer48EFC0.
  Clears each64-byte NCB, but not the enumeration buffer.
- Enumeration/reset returns ignored. Status AL==0 picks first successful
  attempt; only then six shared bytes are copied into caller destination.
- Failure returns0 without touching destination. Report caller ignores it,
  does not preinitialize MAC bytes, and prints six zero-extended bytes with
  literal `macaddr = %02x:%02x:%02x:%02x:%02x:%02x\n`.
- No invented zero address, physical-adapter preference, real hardware
  access, target execution, or Netbios requests in reconstruction.
- Saved MacAddressNativeLayout and byte checks. Build succeeds;1255
  sample-backed checks and1121 pure checks pass. Follow-on: OS description.

## OS-description guards and Awesomium duplicate switch values

- MAC source batch committed/pushed as28b6fd7.
- HWStats403A20: GetVersionExW must succeed, platform must be2 and unsigned
  major>4. Native-system-info export falls back to GetSystemInfo.
- Only major6 requires kernel32/GetProductInfo lookup; missing module or
  export returnsfalse without destination copy. ProductInfo BOOL itself
  is ignored; inputs use major/minor and zero service-pack arguments.
- Saved admission reasons, not a successful-format guarantee. Native
  wcscpy_s passes4096 wchar capacity while this caller has2048 wchar region;
  no output maximum/overflow conclusion. Caller ignores builder return and
  does not preinitialize description buffer. Exact text remains follow-on.
- GPT-6 Luna High agent3356aa23 completed Awesomium duplicate-map trace:
  admitted switches update same normalized-key slot in argument order;
  last wins including empty. Child type getter reads that map. No claim
  about aliases, full normalization, or tokens after a positional argument.
- Integrated agent source/doc and byte checks. Build succeeds;1280
  sample-backed checks and1137 pure checks pass.
- Follow-ons: Awesomium token admission around positional/end-marker
  arguments; HWStats exact OS/edition/service-pack/build text.

## OS-name fragments and Awesomium token-admission state

- Previous OS-guard/duplicate-map batch committed/pushed as56a3c8f.
- Recovered all major6 name literals and18 recognized product-code
  editions. The48-byte product-index table at403FA8 selects19 DWORD
  destinations at403F5C (last is no-edition continuation).
- Captured major5 minors0/1/2 names, SM_SERVERR2/suite/architecture
  priorities, exact missing separators, and the lack of an IA64 Standard
  fallback. Unknown major6 minor yields literal Windows ?; unknown edition
  appends nothing. Do not modernize Windows Server 8 or other literals.
- Captured Microsoft prefix, optional service-pack separator, build-prefix
  ordering, unsigned-long build insertion and closing-helper input byte29.
  Suffixes require major>=6 and arch9/0. C# saves fragments, not CRT stream
  locale/padding/failure behavior or outer DirectX line.
- GPT-6 Luna High agent6ee465fa completed token admission: plain positional
  argument leaves switch scanning enabled; exact -- turns it off stickily.
  Later type entries reach map only while scanning and prefix admission
  pass. Saved capture uses classified booleans, not a raw argv parser.
- Build succeeds;1417 sample-backed checks and1210 pure checks pass.
- Next independent GPT-6 Luna High trace started as agent
  e3046fa9-6eb9-4c09-a32f-b6f7623e07db: DumpTruck HWStats stdout pipe,
  lifetime/read-result and captured-byte admission. It owns only a new
  DumpTruck source/doc and will return inline check expressions; parent
  must integrate/validate before committing that future work.
- Remaining known boundaries: complete CRT/DirectX output behavior,
  Awesomium name normalization, and unresolved RedHanded state2 writer.
  No target execution, sample/IDA modification or real hardware collection.

## DumpTruck hardware capture and HWStats DirectX buffer

- OS-text/token-admission batch committed/pushed as68f05d2.
- Agent e3046fa9 completed: DumpTruck42AB90 makes both pipe endpoints
  inheritable, passes child handle inheritance, and retains endpoints until
  common cleanup. Poll/read requests2048 bytes, forwards actual read count,
  uses Sleep1 plus GetExitCodeProcess rather than timeout/EOF wait.
- Failed peek follows empty-pipe decision. Child exit or exit-query failure
  appends one NUL; process/read failure skips NUL. Pipe failure returns
  directly before common cleanup. PROCESS_INFORMATION failure contents
  remain unknown; no invented invalid-handle effects.
- Integrated HardwareSurveyCaptureNativeLayout and byte checks for each
  branch/close path. Existing HardwareReportNativeLayout still controls
  generator success vs omit-layer behavior and later compression.
- HWStats404020 DirectX query initializes128-byte buffer and byte count127,
  uses HKLM KEY_READ and RegQueryValueExW without type output/status test.
  Doubles returned byte count modulo32 bits and requires result<128 before
  writing a zero WORD at that doubled byte offset.64 ordinary bytes fails;
  no query-failure default or conventional terminator arithmetic invented.
- Saved DirectXVersionNativeLayout and buffer/format evidence. Build
  succeeds;1455 sample-backed checks and1238 pure checks pass.
  Both findings are inert; no target or
  hardware/registry access. Agent has finished; no active agent assignment.
- Follow-ons: DumpTruck submission-layer serialization boundaries and
  Awesomium switch-name normalization. RedHanded state2 writer unresolved.
