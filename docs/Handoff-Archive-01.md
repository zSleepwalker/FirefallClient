# Reverse-engineering handoff archive 01

## Resume here (route0 nested selector-0x5E consumer traced)

The bounded continuation traced route0's nested `Character::PublicCombatLog`
selector `0x5B`, parser case raw `0x81CF2F`. The outer switch table at VA
`0xC1D6DC` maps it to receiver slot `+0xF0` / raw case `0x81C559`; the vtable
target VA `0xECB260` maps to raw `0xACA660`. That receiver branches on a QWORD
at message `+0x30/+0x34` (zero or a high sentinel range) and byte `+0x38`,
then performs ordered-tree helper calls through state pointer `+0x43BC`.
Missing state triggers diagnostic `1647`, then the method continues. Its field
meanings and higher-level schema remain unknown.

Selector `0x5A` is also documented: slot `+0xEC` targets raw `0xACAA30`, which
passes the DWORD at message `+0x28` and arguments `(9, 0, 0)` to `0x4BCBD0`.
The receiver does not iterate the 16-byte vector records; because `+0x28` is
the vector begin pointer, the control routine receives that pointer value as a
DWORD. Its conditional path may emit event `50` through slot `+0xAC`; intended
semantics remain unknown.

Selector `0x59` is also documented: slot `+0xE8` targets raw `0xACA9D0`, which
uses context state pointer `+0x43C8` to clear/reset its tree container and set
state byte `+8` to 1. The method does not load the dispatched message or its
parsed five-DWORD vector records. The reason for the unused payload and the
meaning of the tree reset remain unknown.

Selector `0x58` is also documented: slot `+0xE4` targets raw `0xACA980`, which
passes message DWORD `+0x28` and literal `1` to `0x909120`. On an absent,
nonzero key, the helper clears/resets its state tree and calls `0x9092A0`;
conditional branches can send `SendFindServiceProviderRequest(1)` and emit
event `219`. The parser writes only bytes `+0x28/+0x29` at that input location,
so `+0x2A/+0x2B` remain unwritten. This is a static representation conflict,
not a proven runtime failure.

Selector `0x56` is also documented: its slot `+0xDC` target VA `0xECEA30`
maps to raw `0xACDE30`; that method merges a source tree into owner-state
`+0x30` and emits event `222` when the input count is nonzero. The earlier
`0xACEA30`/`0xACE430` interpretations were mapping arithmetic errors.

Routes 2-6 also use discriminator `0x53` across different message layouts.
Route 3's `+0x34` read is outside its `0x30` allocation; route 4 overlays the
high DWORD of a conditional QWORD and a presence byte/range start; route 5
reads a DWORD at `+0x34`; route 6 uses a QWORD high dword and uninitialized
`+0x2C`. Route-2 details remain below.

Boundary: route 2 writes `+0x28`, `+0x30`, and `+0x38`, but not `+0x2C`;
the handler consumes `+0x2C` as a range endpoint and `+0x34` as a candidate
C-string pointer. Preserve these as unresolved fields. Read-only IDA checks
and original-file evidence agreed on the relevant parser, callback, selector,
and receiver instructions; IDA was not modified and the sample was not run.
The route-0 nested `Character::PrivateCombatLog` selector `0x5C` consumer is
traced in `docs/MatrixConnection.md` and
`CharacterPrivateCombatLogConsumerNativeLayout`. Its slot `+0xF4` maps to
receiver VA `0xECC460` / raw `0xACB860`; it reads fields `+0x28`, `+0x2C`,
`+0x30`, `+0x34`, and `+0x36`, then dispatches application event `122`.
The earlier tentative receiver VA `0x859010` was an address-mapping error;
use the verified target above.

The route-0 nested `Character::AnimationUpdated` selector `0x5D` consumer is
also traced. Slot `+0xF8` resolves to receiver VA `0xECCB40` / raw `0xACBF40`;
it unwraps two 16-bit values around a context counter and sends gated
16-byte records through a 3000-slot circular queue. Their application-level
meaning is not established.

The route-0 nested `Character::RaiaNPCDebugging` selector `0x5E` consumer is
also traced. Slot `+0xFC` resolves to receiver VA `0xECCBE0` / raw `0xACBFE0`.
It reads five WORDs through `message+0x30`, including two bytes beyond the
declared `0x30` allocation; the parser writes one NUL-terminated text field at
`+0x28`. This static layout conflict and its unresolved runtime implications
are documented in `docs/MatrixConnection.md`.

The route-0 nested `Character::WeaponProjectileFired` selector `0x5F`
consumer is also traced. Slot `+0x100` resolves to receiver VA `0xECCF30` /
raw `0xACC330`; it reads 13 WORDs while the parser allocates `0x38` bytes and
provides at most 15 body bytes. The receiver reads through `message+0x41`,
ten bytes past the declared allocation. Details and uncertainty are recorded
in `docs/MatrixConnection.md`.

The route-0 nested `Character::AbilityProjectileFired` selector `0x60`
consumer is also traced. Slot `+0x104` resolves to receiver VA `0xECD580` /
raw `0xACC980`; it reads four WORDs from `+0x28..+0x2E` and conditionally
submits each as a metric. The remaining parser fields are not consumed by
this metric path; see `docs/MatrixConnection.md`.

The route-0 nested `Character::ProjectileHitReported` selector `0x61` receiver
is also traced. Slot `+0x108` resolves to receiver VA `0xECA340` / raw
`0xAC9740`; with the normal x86 direction-flag-clear entry state it executes
`REP LODSD` for 22 DWORDs from the message pointer, reading through `+0x57`
despite the parser's `0x30`-byte allocation. The mismatch and uncertainties
are recorded in `docs/MatrixConnection.md`.

The route-0 nested `Character::Stumble` selector `0x62` receiver is also
traced. Slot `+0x10C` resolves to VA `0xECC250` / raw `0xACB650`. It treats
the message's `+0x28` DWORD as a key and reads a 12-byte-entry vector header
at `+0x30/+0x34`, eight bytes past the declared `0x30`-byte allocation. See
`docs/MatrixConnection.md` for the gated processing paths and caveats.

The route-0 nested `Character::QuickChat` selector `0x63` receiver is also
traced. Slot `+0x110` resolves to VA `0xECD7E0` / raw `0xACCBE0`; it interprets
`+0x30/+0x34` as text/type pointers and posts chat notifications for two
recognized type byte sequences. Those pointers lie beyond the parser's
`0x30`-byte allocation. Details and uncertainty are in
`docs/MatrixConnection.md`.

The route-0 nested `Character::ProximityTextChat` selector `0x64` receiver is
traced at slot `+0x114` (VA `0xECD900` / raw `0xACCD00`). It gates a call to
`0x977150` on a WORD at message `+0x40` equaling `2`. A null handler context
logs diagnostic `91` but does not suppress the sender call, which receives
addresses of fields `+0x28`, `+0x30`, `+0x4C`, and `+0x44`. The `+0x4C` address
lies four bytes beyond the parser's `0x48` allocation; the receiver only forms
and forwards the address, while downstream use remains unresolved.

The route-0 nested `Character::JumpActioned` selector `0x65` receiver is traced
at slot `+0x118` (VA `0xECD970` / raw `0xACCD70`). It passes message field
addresses `+0x28`, `+0x30`, and `+0x40` to `0x977750`; that helper treats the
latter two as polymorphic objects and calls virtual slot `+0x0C`. The parser
requests only `0x30` bytes, so the `+0x30` and `+0x40` object regions start at
and beyond the declared allocation boundary. Field meanings and actual runtime
capacity remain unknown.

The route-0 nested `Character::JumpRolled` selector `0x66` receiver is traced
at slot `+0x11C` (VA `0xECD9B0` / raw `0xACCDB0`). It sends the address of
message `+0x28` to `0x977F20`, which performs an eight-byte keyed lookup and,
on a hit, synchronously drains the associated `+4` count. The parser requests
`0x30` bytes and reads only a WORD at `+0x28`; key meaning and entry effects
remain unknown.

The route-0 nested `Character::Respawned` selector `0x67` receiver is traced at
slot `+0x120` (VA `0xECD9F0` / raw `0xACCDF0`). It reads a begin/end vector at
message `+0x30/+0x34`, processes 16-byte entries tagged `0/1/2` as text,
integer, or float values, then can append fields `+0x2C` and `+0x8C` before
dispatching event `167`. The parser requests only `0x30` bytes, so the vector
header and `+0x8C` read exceed its declared extent; runtime capacity and
semantics remain unknown.

The route-0 nested `Character::CalledForHelp` selector `0x68` receiver is traced
at slot `+0x124` (VA `0xECE040` / raw `0xACD440`). It looks up the DWORD at
message `+0x28` in registry `0x1E5A830` and, on a hit, builds event `176` from
registry values and message fields `+0x2C/+0x30`. The `+0x30` DWORD begins at
the end of the parser's `0x30`-byte allocation; field meaning and runtime
capacity remain unknown.

The route-0 nested `Character::TookHit` selector `0x69` receiver is traced at
slot `+0x128` (VA `0xECE210` / raw `0xACD610`). It copies `0x40` bytes from
message `+0x28`, adds a global float to the copied value sourced from `+0x60`,
and conditionally copies the modified block to state `+0xC8` after setting
flag `+0x108`. The parser's allocation is `0x60`; the copy reads eight bytes
beyond that declared extent. Meaning and runtime impact remain unknown.

The route-0 nested `Character::AlmostHit` selector `0x6A` receiver is traced at
slot `+0x12C` (VA `0xECEAD0` / raw `0xACDED0`). It validates a QWORD key at
message `+0x30`, performs a normalized lookup and conditionally calls a virtual
method at `+0x70`, then forwards the QWORD at `+0x28` to a signed-value helper
with constant `12`. The lookup key lies entirely beyond the parser's `0x30`
byte allocation; meanings and runtime impact remain unknown.

The route-0 nested `Character::DealtHit` selector `0x6B` receiver is traced at
slot `+0x130` (VA `0xECEB70` / raw `0xACDF70`). It passes a three-float view
starting at message `+0x30`, QWORD `+0x28`, and duration `0x3C` to shared helper
`0x368EA0`. The helper updates position-related state, conditionally computes
a distance callback, forwards the QWORD through `0x369080`, and registers a
`60000` ms follow-up timer. Parser byte `+0x38` is also a nested-record gate;
the overlap between those views remains unresolved.

The route-0 nested `Character::Killed` selector `0x6C` receiver is traced at
slot `+0x134` (VA `0xECEBA0` / raw `0xACDFA0`). It forwards the DWORD at
message `+0x28` and a pointer to `+0x2C` into helper `0x466790`, which looks up
an object and can allocate/update `0xB0`-byte contexts through `0x465A10`.
Field and context meanings remain unknown; its observed accesses are inside
the parser's `0x40`-byte allocation.

The route-0 nested `Character::WarnLockTargeted` selector `0x6D` receiver is
traced at slot `+0x138` (VA `0xECEBC0` / raw `0xACDFC0`). With a nonzero global
manager pointer, it passes DWORD `+0x28` and Boolean `+0x38` to helper
`0x465D40`, which manages keyed manager entries and can update state. The case
allocates `0x28` bytes and reads both fields beyond that declared extent;
meaning and runtime impact remain unknown.

The route-0 nested `Character::CurrentPoseUpdate` selector `0x6E` receiver is
traced at slot `+0x13C` (VA `0xECE2A0` / raw `0xACD6A0`). It formats three
message fields as floats, optionally resolves a DWORD at `+0x34` to text, and
dispatches event `239`. The parser's variable control/field layout and the
receiver's float/key view overlap; their relationship remains unresolved.

The route-0 nested `Character::ConfirmedPoseUpdate` selector `0x6F` receiver
is traced at slot `+0x140` (VA `0xECE400` / raw `0xACD800`). It appends a kind-5
typed value from message DWORDs `+0x28/+0x2C`, optionally resolves `+0x30` to
text, and dispatches event `239`. All observed fields fit the parser's `0xC0`
allocation; field and event semantics remain unknown.

The route-0 nested `Character::PublicDebugMovementUpdate` selector `0x70`
receiver is traced at slot `+0x144` (VA `0xECEBF0` / raw `0xACDFF0`). It
appends a kind-5 typed value from message DWORDs `+0x28/+0x2C`, conditionally
queries the object at `+0x30`, and dispatches event `363`. The `+0x30` object
view falls inside the first parser record, but their relationship and
semantics remain unresolved.

The route-0 nested `Character::ForcedMovement` selector `0x71` receiver is
traced at slot `+0x148` (VA `0xECED30` / raw `0xACE130`). It passes a pointer
to the nested message state at `+0x28` to helper `0xC24780` and dispatches
event `365`; individual field and event semantics remain unknown.

The existing movement receiver overview also records selectors `0x72`
(`ForcedMovementCancelled`) and `0x73` (`GrappleClimbPermission`) with their
verified dispatch slots and helper paths; avoid repeating those traces.
Selectors `0x74`–`0x77` likewise have receiver routing documented. The
`0x74`–`0x76` embedded objects are `slBlob` instances whose virtual `+0x10`
reader copies the remaining bytes verbatim; selector `0x76`'s parser-created
blob representation conflicts with the receiver's range interpretation.
Continue by tracing whether any intervening transformation explains that
mismatch, or investigate selector `0x77`'s extent mismatch, while keeping this
selector table distinct from route indices 2-6 and the secondary decoder
table.

All secondary selectors `0x20..0x7A` are now reconstructed and checked against
`FirefallClient.exe.original`. The final entries `0x78..0x7A` resolve to
`VendorProductsResponse`, `VendorPurchaseResponse`, and
`ConductorGlobalAnnouncement`; do not confuse them with route0's nested
Character selectors `OpenMovieDialog`, `PrivateDialog`, and `PublicDialog`.
Do not repeat the earlier decoder or keyed-receiver factory batches.
The decoder batches through `0x7A` and route-2 through route-6 consumer traces
were previously committed; recent updates add route0 nested receiver traces
for selectors `0x6F`, `0x70`, and `0x71`.
Use GPT-6 Luna with explicit High reasoning for implementation and review.
Verify field-address setup, primitive widths, count-loop boundaries and
helper calls against the original binary before committing and pushing.

- Matrix nested selector table raw `0x821844` (selectors `0x53..0x7A`) is
  fully traced; see the summary table in `docs/MatrixConnection.md`.
- Primary-dispatcher routes 2-6 hold 17 subcases; all 17 now have native
  layouts. The six formerly unmapped route3 entries (`2,3,4,5,7,8`) are reconstructed
  in `docs/MatrixConnection.md` and `src/Startup/Vehicle*NativeLayout.cs`.
- Historical IDA MCP notes identify instance `13337` as the analyzed
  `FirefallClient.exe` image
  (SHA-256 `97362D668FF64778193A64759265E4AE90842043C4A15B627DF9902FD9135445`)
  loaded as a flat `seg000` at base `0`. Its PE `.text` maps raw file offsets
  to runtime VA with `+0x400C00` (`ImageBase=0x400000`, section RVA `0x1000`,
  raw pointer `0x400`); `.rdata` maps with `+0x401400` (RVA `0x16C4000`,
  raw pointer `0x16C2C00`), and `.data` with `+0x401C00` (RVA `0x19C5000`,
  raw pointer `0x19C3400`). Dispatcher, vtable/COL, and TypeDescriptor pointers
  verify these mappings. The requested `FirefallClient.exe.original` SHA-256 is
  `E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235`
  (40,197,848 bytes);
  it differs from the analyzed image in 13 bytes across four ranges, none of
  which overlap this batch's code/vtable evidence.
- IDA MCP was unavailable during the earlier secondary-selector final batch;
  selectors `0x78..0x7A` were checked against the original PE with local
  read-only byte reads/disassembly and repository binary-evidence checks. For
  the route-2 consumer continuation above, IDA instance `13337` was reachable
  and read-only disassembly/byte reads matched the requested original file at
  the relevant code sites.
- Secondary selectors in `0x81CC50` use the separate 91-entry table at VA
  `0xC22630` / raw `0x821A30`: `[ebp+8] - 0x20`, bounded through index
  `0x5A`, then indexed by DWORD. The original binary has 91 nonzero, distinct
  destinations; all 91 selectors now have verified message identities/layouts.
  The table index is distinct from the keyed-receiver factory's
  `0x02..0x35` selectors and the route0 Character table's `0x53..0x7A`
  selectors. Secondary selectors `0x53..0x57` and route0 Character selectors
  with those same numbers are different tables and dispatch namespaces.
  Secondary selectors `0x70..0x77` are likewise distinct from route0's
  same-numbered Character selectors; secondary `0x78..0x7A` are distinct from
  route0's dialog cases. See the C# snapshots and
  `docs/MatrixConnection.md`.
- Final secondary-table entries (indexes are `selector - 0x20`):

  | Selector / index | Target VA / raw | Allocation | Constructor / base helper | Type / descriptor anchor | Vtable / RTTI | Reader |
  | --- | --- | ---: | --- | --- | --- | --- |
  | `0x78` / 88 | `0xC2229E` / `0x82169E` | `0x60` | `0x81AC60` / `0x826310` | `0x78` / `0x1B89A9C` | `0x1C6DD18` / `VendorProductsResponse` | `0x83DF60` |
  | `0x79` / 89 | `0xC22306` / `0x821706` | `0x48` | `0x81AE20` / `0x826380` | `0x79` / `0x1B89AA4` | `0x1C6DCE8` / `VendorPurchaseResponse` | `0x83B9F0` |
  | `0x7A` / 90 | `0xC2236E` / `0x82176E` | `0x88` | `0x81B000` / `0x826400` | `0x7A` / `0x1B89A94` | `0x1C6DCD8` / `ConductorGlobalAnnouncement` | `0x83B8F0` |

- These final readers were traced in wire order. `0x78` reads scalar fields at
  `+0x28..+0x40` and byte-counted vectors at `+0x44/+0x50`; the latter has
  `0x30`-byte records, nested vectors at `+0x14/+0x20`, and a booleanized tail
  byte. Nested item strides are `0x18` and 8 bytes. `0x79` reads byte,
  QWORD/QWORD, DWORD, and text at `+0x28/+0x30/+0x38/+0x40/+0x44`.
  `0x7A` reads DWORDs at `+0x28/+0x2C/+0x30`, text at `+0x34/+0x38`, gated
  values at `+0x3C/+0x4C`, text at `+0x80`, gated text/optional-DWORD at
  `+0x5C`, gated DWORD/DWORD/text at `+0x70`, and a final byte at `+0x84`.
  `0x78`'s vector counts use `0xFF` continuation and zero termination;
  `0x7A`'s gates are byte-presence gates, not count loops. All three allocation
  paths branch on null and still call the body reader after the constructor or
  zero-object path; none has a no-reader success epilog. This is static flow
  evidence only and does not establish allocation-failure runtime behavior.
- Previously recorded next work was to continue the existing “trace
  parsed-object consumers” task for the 17 case-bearing primary-route
  subcases; route3 entries `2,3,4,5,7,8` are now mapped. Existing unresolved
  decoder observations also remain:
  selector `0x2A` tagged variants, selector `0x36`'s float-derived predicate,
  and selector `0x3B`'s opaque payload. Keep these separate from the completed
  secondary-selector identities.
- The 91 verified secondary cases are RTTI-backed:
  `EncounterToUIMessage`, `VotekickInitiated`, `VotekickResponded`,
  `ScoreBoardEnable/Init/SetWinner/Clear/AddPlayer/RemovePlayer/
  UpdatePlayerStats/UpdatePlayerStatsFromStats/UpdatePlayerStatus`,
  `MatchLoadingState`, `MatchEndAck`, and `ServerProfiler_SendNames/
  ServerProfiler_SendFrame`, plus `TempConsoleMessage`, `ReloadStaticData`,
  `EncDebugChatMessage`, `SendRadioMessage`, `NpcBehaviorInfo`,
  `NpcMonitoringLog`, `NpcNavigationInfo`, `NpcHostilityDebugInfo`,
  `NpcPositionalDebugInfo`, `NpcShapeDebugInfo`, `NpcVoxelInfo`,
  `DebugDrawInfo`, `NpcDevCmdResponse`, `DevRequestObjectPositions`,
  `DevRequestSpawnTables`, `DevRequestResourceNodeDebug`,
  `MissionObjectiveUpdated`, `MissionStatusChanged`,
  `MissionReturnToChanged`, `MissionsAvailable`, `MissionActivationAck`,
  `BountyStatusChanged`, `BountyAbortAck`, `BountyActivationAck`,
  `BountyListActiveAck`, `BountyListActiveDetailsAck`,
  `BountyListAvailableAck`, `BountyClearAck`, `BountyClearPreviousAck`,
  `BountyListPreviousAck`, `BountyRerollResponse`, and `DisplayUiTrackBounty`.
  The latest batch adds `Achievements`, `AchievementUnlocked`,
  `TotalAchievementPoints`, `MissionCompletionCounts`, `ContentUnlocked`,
  `ClientUIAction`, `ArcCompletionHistoryUpdate`, and `JobLedgerEntriesUpdate`.
  Selectors `0x68..0x6F` add `DisplayMoneyBombBanner`, `SetPreloadPosition`,
  `PlaySoundId`, `PlaySoundIdAtLocation`, `PlayDialogScriptMessage`,
  `StopDialogScriptMessage`, `PingMap`, and `PingMapMarker`.
  Selectors `0x70..0x77` add `EncounterPublicInfo`,
  `RequestActiveEncounters_Response`, `ShoppingListInit`,
  `SetClientDailyInfo`, `GlobalCounterUpdate`,
  `GlobalCounterMilestoneInfo`, `ChatMessageList`, and
  `CurrentLoadoutResponse`.
  Selectors `0x78..0x7A` add `VendorProductsResponse`,
  `VendorPurchaseResponse`, and `ConductorGlobalAnnouncement`.
  Their common base initializer writes marker
  `+0x08`, descriptor anchor `+0x0C`, constructor type DWORD `+0x10`,
  parser-context copies `+0x18/+0x1C`, and zeros `+0x20/+0x24`; body begins
  `+0x28`. Selector `0x2A` contains a byte-counted collection with nested
  DWORD-counted tagged entries; tags 1..16 dispatch, but their full variant
  layouts remain unresolved. Selectors `0x2E/0x2F` use byte-counted (`0xFF`
  continuation) lists; `0x2F` recurses through nested 0x20-byte records.
  The new `0x30..0x37` layouts record four NUL-text fields then a WORD-length
  blob (`0x30`), no body reader (`0x31`), a shared NUL-text continuation
  (`0x32/0x34`), two DWORDs (`0x33`), text/byte/text (`0x35`), and the
  QWORD-plus-vector shapes at `0x36/0x37`. Selector `0x36`'s second vector
  has `0x70`-byte native records and a float-derived conditional tail; its
  predicate is unresolved. Selector `0x37`'s wire items are QWORD+DWORD with
  `0x10` native stride. Selectors `0x38..0x3F` continue with the RTTI-backed
  positional/shape/voxel/debug/dev-request layouts: counted records include
  QWORD-plus-eight-DWORD and byte-plus-20-DWORD-plus-byte shapes, counted
  three-DWORD/DWORD/byte vectors, paired shared-text reads, and a text/five
  DWORD/byte/text record. Selector `0x3B`'s byte payload remains opaque.
  Selectors `0x40..0x47` add the RTTI-backed mission/bounty cases; their
  bodies include scalar/text sequences and one byte-counted DWORD+byte vector
  with five-byte wire items and eight-byte native stride. Parser-local DWORD
  literals `0x08..0x27` for selectors `0x28..0x47` are recorded separately
  from table indexes and constructor type IDs; their role is unknown.
  Selectors `0x48..0x4F` add the RTTI-backed
  bounty list/clear/response and UI-track cases: their bodies are scalar bytes
  and DWORDs, WORD-length-prefixed spans sent to embedded objects, and a
  NUL-terminated shared-text field. The `0x5BCFA0` reader consumes one byte but
  stores its zero-extended value as a DWORD; the blob reader uses the embedded
  object's virtual `+0x10` method only for nonzero WORD lengths. The shared
  text reader uses 256-byte chunks until its NUL terminator, not a 256-byte
  total limit. Parser-local DWORD literals `0x20..0x2F` for selectors
  `0x40..0x4F` are separately recorded, with roles unknown. Selectors
  `0x50..0x57` add a five-byte DWORD-plus-byte record vector (eight-byte
  stride), QWORD and DWORD scalar readers, a 16-byte DWORD/QWORD/DWORD record
  vector (24-byte stride) followed by shared text, ordered text/DWORD/QWORD
  and text-only readers, a 16-byte counted tree-record sequence, and a
  four-byte DWORD vector (four-byte stride). The tree constructor allocates a
  `0x20` sentinel, self-links its three pointer fields, and writes the WORD
  `0x0101` at `+0x0C`; that is one adjacent-byte-spanning word store, not
  evidence for two independent byte fields. Count bytes use `0xFF`
  continuation and zero termination where observed. Parser-local DWORD
  literals `0x30..0x37` are recorded separately from selectors and
  constructor type IDs; their roles are unknown. Selectors `0x58..0x5F` add
  the RTTI-backed `TrackRecipe`, `ClearTrackedRecipe`, `SlotTech`,
  `InteractableStatusChanged`, `SendTipMessage`, `DebugEventSample`,
  `DebugLagPlayerSample`, and `DebugLagSimulationSample` cases. They carry a
  DWORD at `+0x28`, no body reader, another DWORD at `+0x28`, then ordered
  byte/DWORD/QWORD/byte; byte/two DWORDs/WORD/byte; two WORDs/two bytes; five
  WORDs; and thirteen WORDs, respectively. Their field meanings remain
  unresolved. Parser-local DWORD literals `0x38..0x3F` are recorded separately
  from selectors and constructor type IDs. Selectors `0x60..0x67` add the
  RTTI-backed `DebugLagRaiaSample`, `DebugEncounterVolumes`, `Trail`,
  `EncounterDebugNotification`, `EncounterUIScopeIn`, `EncounterUIUpdate`,
  `EncounterUIScopeOut`, and `DisplayUiNotification` cases. Their layouts
  include four WORDs; four independently counted record vectors; a three-float
  vector; ordered QWORD/text fields; length-prefixed and remaining-input
  blobs; a tree with nested tagged records; and a text-bearing vector with
  `0x10` native stride and a trailing byte. The `0x64` tagged-vector parser
  uses a byte count with `0xFF` continuation at both levels; tag 7 selects a
  DWORD or QWORD payload using bit `0x02`, tag 5 is unsupported, and tags
  `0xFE/0xFF` use special zero/no-payload paths. Tag and field meanings remain
  unresolved. Parser-local DWORD literals `0x40..0x47` are separate from
  selectors and constructor type IDs. Tag `5` and values `0x09..0xFD` reach
  the same invalid/unsupported path; application meanings remain unknown.
  Selectors `0x68..0x6F` add the scalar/text/vector layouts for
  `DisplayMoneyBombBanner`, `SetPreloadPosition`, `PlaySoundId`,
  `PlaySoundIdAtLocation`, `PlayDialogScriptMessage`,
  `StopDialogScriptMessage`, `PingMap`, and `PingMapMarker`. `0x68` reads
  DWORD/text/DWORD; `0x69` reads 16 DWORDs; `0x6A` reads two QWORDs; `0x6B`
  reads QWORD then three DWORDs; `0x6C/0x6D` read a DWORD and an opaque
  eight-byte-record vector, with `0x6D` adding a trailing byte; `0x6E` reads
  four DWORDs; `0x6F` reads QWORD then DWORD. Vector counts are one-byte
  chunks: `0xFF` continues, zero terminates. The text reader is NUL-terminated
  and uses `0x100`-byte chunks, not a total-string bound. Parser-local DWORD
  literals `0x48..0x4F` remain distinct from selector/type ID and unresolved.
  Selectors `0x70..0x77` add `EncounterPublicInfo`,
  `RequestActiveEncounters_Response`, `ShoppingListInit`,
  `SetClientDailyInfo`, `GlobalCounterUpdate`,
  `GlobalCounterMilestoneInfo`, `ChatMessageList`, and
  `CurrentLoadoutResponse`. `0x70` reads a QWORD then passes remaining input
  to a blob at `+0x30`; `0x71` passes all remaining input to a blob at `+0x28`.
  Both embedded members are `slBlob`; slot `+0x10` resizes then raw-copies the
  incoming bytes when its data pointer is non-null.
  `0x72` reads a byte-counted tree of two-DWORD items; `0x73` reads a DWORD and
  a byte-counted DWORD/helper sequence; `0x74` reads text then QWORD. `0x75`
  has two counted lists, including a nested two-DWORD/text/DWORD record.
  `0x76` has `0x40`-stride records with QWORD/text/text/three bytes and
  optional blob, optional QWORD, and opaque nested readers. `0x77` reads
  QWORD/DWORD/DWORD/text/text/DWORD and a byte-counted `0x2E0`-stride vector
  with an inner byte gate. Count bytes use `0xFF` continuation and zero
  termination where observed; fields are offset/width observations only.
  Parser-local DWORD literals `0x50..0x57` remain distinct from selectors,
  native type IDs, and anchors.
  The setup call at raw `0x3571B0` is a `ret` stub; no semantics are assigned
  to its arguments. The route0 Character selector namespace overlaps these
  numeric values but uses a separate dispatch table. All 91 verified targets are
  distinct and nonzero. Selectors `0x28/0x2D`
  and `0x32/0x34` share
  reader continuations despite distinct table destinations and types.
  Out-of-range selectors share zero-return raw `0x8217D9`; selectors `0x26`
  and `0x31` branch there on allocation failure. Selectors `0x20..0x25`,
  `0x27..0x30`, and `0x32..0x4E` continue through their body-reader calls with
  the object register zero, including `0x2D` through `0x28`'s continuation and
  `0x32/0x34` through their shared text-reader continuation.
  selector `0x4F` instead reaches its direct DWORD helper after forming the
  destination from zero ESI plus `0x28`. Selectors `0x50`, `0x53`, `0x54`,
  `0x56`, and `0x57` likewise reach readers from local zero-object blocks;
  `0x51`, `0x52`, and `0x55` use zero-object continuations `0x81F72C`,
  `0x8206E1`, and `0x81FB25`. These are static paths only; no runtime
  consequence is asserted. Selectors `0x58` and `0x5A` share the
  `0x8206E1/0x8206E3` zero-object/reader continuation; `0x59` returns to
  `0x8217D9` on allocation failure, and `0x5B..0x5F` reach local zero-object
  blocks before their scalar readers. Selectors `0x60..0x65` and `0x67` also
  reach local zero-object blocks before their readers; selector `0x66` branches
  to the shared zero-object block at `0x81F72C` and QWORD-reader continuation
  at `0x81F72E`. These are static paths only; no runtime consequence is
  asserted. Selectors `0x68..0x77` likewise test the allocation result and
  join their body readers from the zero-object path. Every case in this range
  has a body-reader call; none has a no-reader success epilog. These are static
  paths only, with no runtime consequence asserted.
- Keep this table distinct from the unrelated keyed-receiver factory
  `0x81B1B0`, route0's nested Character selectors `0x53..0x7A`, and primary
  routes 2-6. `.text` and `.rdata` section-aware vtable RTTI names were
  verified against the requested `FirefallClient.exe.original`.
- Selector-`0x02`'s actual `tfLocalCharacter::BaseNetController` adapter is
  now traced through callbacks for BaseController IDs `0x77..0x79`: exact
  dispatch slots, adapter/context initialization, event ID `285`, and the
  keyed lookup/update path are in `docs\MatrixConnection.md` and
  `CharacterLocalControllerApplicationNativeLayout.cs`. Callback `0x79`'s
  exact registry producer for Character-state entries is now traced to a
  `tfGenericCharacter` constructed at `0x422E80` and inserted by
  `0x943360 -> 0x35E700 -> 0x407F00`; see
  `CharacterKeyedRecipientNativeLayout.cs`. The keyed map can contain other
  producer types, so the class applies to matching generated keys, not every
  possible lookup. The `0x439A50` fallback's conditional six-DWORD record copy
  (`+0x4CA8/+0x4CB0..+0x4CC4`), descriptor field writes
  (`+0x4CFC/+0x4D00..+0x4D0C`), transition-gated `+0x4D10` update, byte
  clearing/setting, and gated downstream
  calls are documented in `docs\MatrixConnection.md`; the bounded pure model
  and binary fingerprints are in `CharacterKeyedRecipientFallbackModel.cs`
  and `Startup.Checks`. Those offset-level effects do not establish field
  meanings or recipient ownership. BaseController callback ID `0x79` is a
  separate namespace from Matrix nested selector `0x79` and does not imply
  `PrivateDialog` semantics for a keyed object. The
  selector-`0x03` MandMController path remains conditional; no NPC runtime
  binding was inferred, and this work is separate from `tfGameProtocolHandler`.
- The `tfGenericCharacter` insertion requests `0x5AE0` bytes, initializes the
  key at `+0x08/+0x0C` with a gate byte of `1`, and installs vtable VA
  `0x1C63998` at object offset zero (RTTI `.?AVtfGenericCharacter@@`, COL
  offset zero). Its unchanged allocation-base pointer is stored in node
  `+0x10`; the node allocation is `0x18` bytes. Getter/constructor/registry
  fingerprints and raw-to-VA mappings are checked against
  `FirefallClient.exe.original` by Startup.Checks.
- Latest parser-consumer pass: raw `0x81CC50`'s two direct callers (`0x821BA0`
  and `0x821DE0`) pass its result through EAX. ReplayReader's `0x65DAF2` call
  to `0x12B7FB0` is allocation (`0xCC` bytes via allocator backend
  `0x12C8A20`), not callback registration. Registration is the later
  `0x65DBD3` call to `0xBD6260`, which stores the callback at state `+0x1C`
  and context at `+0x20`; the constructor also stores the parser VA at object
  `+0x10`.
- The registered callback invocation is in raw `0xBD6B40`: it loads
  `[this+0x1C]` at `0xBD6D29` and calls it at `0xBD6D3C` with
  `(stream, 0, header_length, 0)`. A non-null EAX result is copied into a
  12-byte record and queued at `this+0xB4` through `0xBD8B80`. Consumer
  `0xBD62F0` drains that queue, stages records at `this+0xC8`, and dispatches
  through the stored `this+0x20` context's vtable slots `+4/+8`; it then
  decrements the message pointer's `+8` counter and calls its vtable slot
  `+8` when the old count is one. The enqueue contains no visible retain, so
  producer ownership/ref-transfer policy and tag meanings remain unknown.
- ReplayReader context vtable: `0x65DB3F` calls constructor `0x7FD670`;
  the returned EAX is copied to ESI and stored at `[edi+0x54]` at `0x65DB75`.
  The `0x65DBD3` registration passes that pointer to `0xBD6260`, which copies
  it directly to state `+0x20`. The vptr at object `+0` and the RTTI
  Complete Object Locator's object offset `0` confirm the pointer is not an
  adjusted secondary interface. Vtable VA `0x1C6D624` (raw `.rdata`
  `0x186C224`) names `.?AVtfGameSocketHandler@@`. Slot `+4` is VA `0xBFE7E0`
  / raw `0x7FDBE0` and only logs before `ret 4`; slot `+8` is VA `0xBFE800`
  / raw `0x7FDC00`.
- The constructor initializes context byte `+0x18` to zero, but
  `ReplayReader_Initialize` sets it to `1` at `0x65DBC5` before registration.
  Slot `+8`'s zero-mode path looks up a label from message byte `+0x18` via
  `0x809D20` and logs message fields `+0x0C/+0x10`; the registered context
  takes the nonzero path through `0x7FDC60`. There, `0xBCA850` parses entries
  from the message's 16-bit range `+0x38..+0x3A` when its `+0x10` value matches
  `dword_1DFC030`; returned children recurse through `0x7FDC60` and are
  released through message vtable slot `+8`. With a zero key low byte and a
  nonmatching nested sentinel, a non-null context `+8` is passed to `0x81BB90`.
  Another branch calls `0xBD7060` with the parsed message
  pointer plus a derived selector when the message predicate and context
  `+0xB0` allow it. The fan-out vector is now traced below; recipient identity
  is established for one insertion candidate but not every vector entry.
  Consumer record byte `+6` maps numerically as `1` -> context `+4`, `2` ->
  context `+8` with argument `1`, and `3` -> context `+8` with argument
  `0`; other bytes skip dispatch. These values have no inferred message
  meaning. The readiness probe at `0xBD68F4` remains separate. Details and
  byte fingerprints are in `docs/MatrixConnection.md` and
  `src/Startup/MatrixMessageParserFlowNativeLayout.cs`.
- The callback dispatch at `0x7FDC60` is conditional. It tests the low byte of
  message `+0x18`; nonzero goes through `0x800420`, which uses the full
  `+0x18/+0x1C` QWORD key and the socket-handler registries at `+0x24/+0x2C`.
  With a zero low byte, it compares message `+0x10` with the nested-message
  sentinel; only the non-nested branch with callback-context `+8` non-null
  reaches `0x81BB90`. The receiver at callback-context `+8` is identified as
  `tfGameProtocolHandler`:
  `ReplayReader_Initialize` calls the `tfLookingGlass` getter at raw
  `0x65DB10`, loads its result's `+0x78` field at `0x65DB38`, and passes that
  value to constructor `0x7FD670`. The getter is `tfLookingGlass` vtable slot
  `+0x24`; it returns the first DWORD of the context at `this+0x50`.
  `AppMode_LookingGlass_Initialize` calls the setup helper at `0x9B2D00`,
  whose factory `0x9B73F0` creates an opaque state block, initializes its
  `+0x78` receiver field, and constructs the `0xD8` receiver at `0xAC8CA0`.
  The receiver's vtable VA `0x1C71EC0` / raw `0x1870AC0` resolves by RTTI to
  `.?AVtfGameProtocolHandler@@`; the requested original's PE section table
  verifies these VA/raw mappings (including `.text` raw+`0x400C00`).
- **Nonzero-key registry and fan-out follow-up:** the `tfGameSocketHandler`
  constructor at raw `0x7FD670` initializes tree headers at `+0x24/+0x2C`
  with `0x805130` and stores a factory callback at `+0x14` (VA `0xC1BDB0` /
  raw `0x81B1B0`). `0x800420` passes the full message key at `+0x18/+0x1C`
  to `0x800070`; its tree search `0x8013B0` compares QWORD keys at node
  `+0x10/+0x14`, with mapped pointer at `+0x18`. `0x806910` creates entries
  and `0x8064C0` inserts them; a guarded path at `0x800300` also inserts into
  `+0x2C`. Factory `0x81B1B0` indexes a 52-entry absolute-VA table at
  `0xC1C6A8` / raw `0x81BAA8` using `lowByte-2` and accepts `0x02..0x35`.
  The first eight entries (`0x02..0x09`) now resolve to RTTI-backed
  `Character` controller/view classes: `BaseController`, `NPCController`,
  `MissionAndMarkerController`, `CombatController`, `LocalEffectsController`,
  `SpectatorController`, `ObserverView`, and `EquipmentView`. Their allocator
  sizes, constructor/vtable addresses, `.rdata` raw mappings, and primary
  slots are recorded in `src/Startup/MatrixKeyedReceiverFactoryNativeLayout.cs`.
  This proves these first cases create keyed Character receiver objects, not
  Matrix parser-message instances; the low byte is only the factory selector.
  Continuation mapped selectors `0x0A..0x11`: `0x0A..0x0E` construct
  `AIObserverView`, `CombatView`, `MovementView`, `TinyObjectView`, and
  `DynamicProjectileView` under RTTI namespace `Character`; `0x10` constructs
  `Melding::ObserverView`; `0x0F` and `0x11` share the zero-return path.
  The latest bounded batch adds selectors `0x1A..0x21`: `0x1A` and `0x20`
  share the zero-return path; `0x1B..0x1F` construct `Vehicle::BaseController`,
  `CombatController`, `ObserverView`, `CombatView`, and `MovementView`; `0x21`
  constructs `Anchor::AIObserverView`. Sizes are `0x570`, `0x1120`, `0x478`,
  `0x10F8`, `0xB8`, and `0x78`, respectively. Each constructing case calls
  allocator `0x12B7FB0`, then its constructor; each constructor calls base
  helper `0xBCAD70`, which stores the QWORD key at `+0x18/+0x1C` before the
  derived primary vtable is installed. Section-aware `.rdata` mapping,
  RTTI TypeDescriptors, and zero COL subobject offsets are pinned in
  `MatrixKeyedReceiverFactoryNativeLayout.cs` and original-image checks. The
  index remains `selector-2`: entries `0x02..0x21` include 27 constructors
  and five zero-return entries (`0x0F`, `0x11`, `0x13`, `0x1A`, `0x20`); the
  latest batch maps selectors `0x22..0x29`: `0x22`, `0x26`, and `0x29` are
  zero-return entries; `0x23`, `0x24`, `0x25`, `0x27`, and `0x28` construct
  RTTI-backed `Deployable::ObserverView` (`0x1710` bytes),
  `Deployable::NPCObserverView` (`0x48`), `Deployable::HardpointView` (`0x48`),
  `Turret::BaseController` (`0x128`), and `Turret::ObserverView` (`0xD8`).
  Their constructors call `0xBCAD70`; vtables map to `.rdata`, their RTTI
  TypeDescriptors resolve through the PE section table, and COL offsets are
  zero. The final batch maps selectors `0x2A..0x35`: `0x2A`, `0x2B`, `0x2C`,
  `0x2E`, `0x2F`, `0x31`, `0x32`, and `0x34` share the zero-return target;
  `0x2D`, `0x30`, `0x33`, and `0x35` construct `Outpost::ObserverView`
  (`0x100`), `ResourceNode::ObserverView` (`0x11A0`),
  `CarryableObject::ObserverView` (`0x1690`), and
  `LootStoreExtension::LootObjectView` (`0x758`). Their constructors call
  `0xBCAD70`; vtables map through the PE `.rdata` section, RTTI names resolve
  from TypeDescriptors, and all four COL subobject offsets are zero. The full
  `0x02..0x35` table contains 36 constructors and sixteen zero-return entries,
  all at `0x81BA93` / VA `0xC1C693`.
  Across the 36 mapped receivers, `+0x04` has nineteen message-ID dispatchers
  and seventeen forwarding/logging defaults. The mapped dispatchers accept
  exactly `0x54` and `0x55` for `Deployable::ObserverView`, exactly `0x53` for
  `Deployable::NPCObserverView`, exactly `0x53` for `Turret::ObserverView`,
  and exactly `0x53` for `LootStoreExtension::LootObjectView`; invalid IDs use
  `VIBE_LogUnhandledMessageId` with `message+0x10`. The new `Outpost`,
  `ResourceNode`, and `CarryableObject` `+0x04` methods forward to an optional
  receiver and conditionally emit the fixed no-valid-events diagnostic.
  `Deployable::HardpointView` and `Turret::BaseController` forward to the
  optional `+0x10` receiver and may emit the fixed no-valid-events diagnostic
  without inspecting an ID.
  The prior newly mapped dispatchers accept
  exactly `0x5B` for `Vehicle::BaseController`, `0x53..0x5A` for
  `Vehicle::CombatController`, exactly `0x55`
  (`Vehicle::ObserverView`), `0x57` (`CombatView`), and `0x56`
  (`MovementView`); their invalid-ID paths call
  `VIBE_LogUnhandledMessageId` with `message+0x10`. `Anchor::AIObserverView`
  emits the fixed no-valid-events log without a message-ID argument.
  `0x800420` uses the receiver for a separate compact-selector dispatch on
  message `+0x10`: its explicit selectors `0x07/0x08` call receiver `+0x04`,
  while values above `0x09` take the default `+0x04` call, allowing message
  IDs such as `0x77..0x7A` to reach the receiver's ID dispatcher.
  `+0x08/+0x0C` paths receive data/length forms, and the mapped `+0x18`
  wrappers store the `dword_1AC4D24` result at receiver `+0x28` and call
  `0xBCADF0`. Payload semantics remain out of scope.
  `AIObserverView +0x08` has a recovered disassembly boundary at `0x58A6E0`
  (size `0x394`), although Hex-Rays decompilation and payload semantics remain
  unresolved; this is not an unrecovered function boundary. Earlier
  `0x5536C0` / `0x5795A0` boundaries remain unresolved. See
  `docs/MatrixSecondarySelectors.md` for the complete 52-selector matrix,
  exact table entries, constructor/vtable/RTTI snapshots, slot roles, and
  binary evidence.
  The keyed Character dialog dispatch is now mapped for `0x77..0x7A`:
  factory selector `0x02` is `BaseController` with `+0x04` at raw `0x540380`
  (not `0x54F4A0`); selector `0x03` is `NPCController` with `+0x04` at
  `0x54F4A0`. BaseController handles `0x77/0x78/0x79` through receiver
  `+0x2C` vtable slots `+0x10/+0x14/+0x18`; `0x7A` is unsupported.
  NPCController handles `0x77/0x78` through `+0x14/+0x18`; `0x79/0x7A` are
  unsupported. Both methods refresh receiver `+0x28`, optionally forward
  `(receiver, message, message+0x24)` through a nested object at `+0x10`, then
  test the pointer at receiver `+0x2C` before ID dispatch. Constructors
  `0x53F980` and `0x54E7B0` explicitly initialize `+0x2C` to null; each
  specific dispatch case then loads that field into `ECX`, reads its vptr, and
  calls the recorded slot with the original message pointer as its sole stack
  argument. Thus `+0x2C` is a nullable virtual-receiver pointer, not a state
  flag. The selector-`0x02` factory allocates `0xB80` bytes and passes its two
  key words to `0x53F980`; selector `0x03` constructs the NPCController at
  `0x54E7B0`. The separate keyed state factory's selector-`0x02` state
  creation allocates `0x5AE0` bytes and calls `0x422E80`; its setup `+0x20`
  path reaches `0x426840`, while selector `0x03` is unhandled. The state
  `tfGenericCharacter` object is a state object, not the callback receiver.
  The selector-`0x02` setup allocates a separate `0x228`-byte adapter at
  `0x4A182C` and constructs `tfLocalCharacter::BaseNetController` at
  `0x4C66D0`. Its vtable is VA `0x1C64648` / raw `.rdata` `0x1863248`;
  constructor fields are private context `+0x170` and keyed receiver
  `+0x174`. Update method `0x4C7360` loads the receiver from `+0x174` and
  writes its own unadjusted `this` pointer to BaseController `+0x2C` at
  `0x4C7393`. The actual callback slots are `+0x10/+0x14/+0x18` ->
  `0x4C82E0/0x4C8300/0x4C9B80`; each target's `ret 4` matches the one
  message-pointer argument pushed by the dispatcher. The earlier
  `tfGenericCharacter` callback-vtable claim and associated `0x420720/0x420780`
  `ret 8` mismatch are withdrawn.
  The selector-`0x03` code in `0x4A17C0` can construct a `0xB0`-byte
  `MandMController` (`0x4C6360`, vtable VA `0x1C64640`) and its updater
  `0x4C6490` writes the adapter to receiver `+0x2C` at `0x4C64A1`. But
  `0x4A17C0` has only the traced caller `0x426840`, while the keyed state
  factory rejects selector `0x03`; this does not prove an NPCController
  binding. The separate `0x4A2F07` child `+0x2C` store likewise has no
  proven alias to either controller.
  The optional nested receiver at controller `+0x10` is distinct: shared
  helper `0xBCAD70` initializes it null at `0xBCAD91`; dispatch forwards
  `(controller, original message, message+0x24)` through its vtable `+0x04`
  when non-null. Its type and later non-null writer remain unresolved. This
  does not alter the conditional
  `0x7FDC60` global route: nonzero message
  `+0x18` low byte goes to `0x800420`, while the zero-low-byte, non-nested,
  non-null callback-context `+0x08` branch can reach the separate
  `tfGameProtocolHandler`. See `docs/MatrixConnection.md` for precise
  factory/constructor bytes, vtable targets, and unresolved NPC/nested edges.
- The gated `0xBD7060` path serializes the message QWORD and message data into
  its output stream (raw QWORD from message `+0x18`, compact positive UInt16
  from `+0x10`, then message-vtable slot `+0` output), then passes the
  separate selector to `0xBD7460`,
  which stores it in the recipient callback's local record, not the stream.
  That function traverses 32-bit recipient pointers in `[+0x2C,+0x30)`
  (capacity `+0x34`) and calls each recipient's vtable slot `+0x0C` with an
  eight-byte local header (`owner[+0x20]`, WORD output length, selector byte,
  zero byte) and a separate output-buffer pointer. A candidate registration
  path at `0x7FE1C0` allocates `0x48`
  bytes, constructs it with `0xBD6E10`, appends via `0xBD8C90`, then invokes
  recipient slot `+4`. `0x7FE360` constructs one RTTI-backed
  `nl::DefaultReplayWriter` at `0xBD7EA0`; its slot `+0x0C` is VA `0xFD9010`
  / raw `0xBD8410`, which checks record/input lengths via `0x12A0660` and
  returns a Boolean. This identifies a representative recipient in that
  insertion path, not every `0xBD7460` recipient: a direct link between the
  callback's `+0xB0` field and the `0x7FE1C0` owner is not established by the
  shared offset alone. Layouts and original-byte assertions are in
  `src/Startup/MatrixMessageNamespaceRoutingNativeLayout.cs`,
  `src/Startup/MatrixMessageParserFlowNativeLayout.cs`, and
  `src/Startup/Checks/StartupBinaryEvidence.cs`.
- On that conditional path, dispatcher `0x81BB90`'s 91-entry table covers
  `0x20..0x7A` and maps ID to receiver slot `4 + 4*(ID-0x20)`. The concrete dialog mappings are
  `0x78 OpenMovieDialog -> +0x164 -> 0xACE2E0`, `0x79 PrivateDialog ->
  +0x168 -> 0xACE300 -> tail-jump 0x991510`, and `0x7A PublicDialog ->
  +0x16C -> 0xACE390`. The OpenMovie path dispatches numeric application code
  `228`; the other two dispatch `229` and `357`. The `0xECEF00` vtable pointer
  occurs only once in the requested original, at raw `.rdata` `0x1870C28`.
  The exact app endpoint is
  `uiManager` global VA `0x1E002B8`, whose vtable slot `+0xAC` maps to
  `0xBF9D80`. That method indexes the dynamic event bucket at manager `+0x6F0`;
  names are allocated IDs by `0xBF9AB0` through the map at `+0x6E8` and name
  array at `+0x71C..+0x720`. On a map hit, the event ID is read from node
  `+0x14` (`0xBF9ADA`); on a miss, `(end-begin)/4` is passed as the new ID
  before the name pointer is appended (`0xBF9AE6`, `0xBF9AFE`, `0xBF9B2B`).
  XML `<Event>` parsing inserts its `name` before reading `bind`, and
  `Component.BindEvent` uses the same map. File enumeration is unsorted and
  script calls add runtime first insertions, so exact IDs/names and concrete
  subscribers for `228/229/357` remain unresolved.
- **Parser-family selector collisions (read-only original):** Character
  nested route 0 `MarketRequestComplete`, AreaVisualData route 2
  `LootObjectCollected`, Vehicle route 3 `AbilityActivated`, and Deployable
  route 4 `TookHit`, Turret route 5 `WeaponProjectileFired`, and
  LootStoreExtension route 6 `LootObjectCollected` all store `0x53` at
  message `+0x10`, but have distinct parser cases, constructors, derived
  vptrs/RTTI, and `+0x0C` metadata pointers. The common base helpers write
  `+0x00`, `+0x08`, `+0x0C`, `+0x10`, `+0x18/+0x1C`, and `+0x20/+0x24`; they do
  not write `+0x14` in the observed initialization sequence. Metadata strings
  begin `msg-83` and do not prove a namespace by themselves. The parser stack
  locals (`0x5B/0xCD/0xD0/0xD9/0xDC/0xDD` for these six `0x53` cases) are not
  message discriminators.
- `0x81BB90` still maps `0x53` to primary receiver slot `+0xD0` / raw handler
  `0xACA580`, but only if the message takes that conditional path. The concrete
  handler RTTI group has a COL object offset of zero and `tfGameProtocolHandler`
  plus `MessageHandler` base subobjects at offset zero; no secondary vtable
  group was observed. The handler reads message `+0x34`, so a Vehicle message
  of allocation size `0x30` would present an apparent extent mismatch if it
  reached that slot. Runtime envelope QWORD values were not available, so
  whether it does remains unresolved. Therefore the prior conclusion that
  these examples rule out a wrong-ID namespace was too broad; do not apply
  `0x81BB90`'s selector table universally. The route snapshot and original-byte
  assertions are in `src/Startup/MatrixMessageNamespaceRoutingNativeLayout.cs`,
  `docs/MatrixConnection.md`, and `src/Startup/Checks/StartupBinaryEvidence.cs`.
- Recorded payload keys/fields: `228` passes `vendor_id` from message `+0x28`;
  `229` passes `success` (`+0x28` byte), `product_id` (`+0x30/+0x34`),
  `price_id` (`+0x38/+0x3C`), `vendor_id` (`+0x40`), and a code-string
  pointer from `+0x44` (fallback value `PURCHASE_SUCCESSFUL`); `357` prepares
  `action_id` (`+0x30`) and `activated` (`+0x84`) but passes null payload arguments.
  These strings are payload property keys/values, not event names.
  The focused installed-source check confirms
  `system\gui\components\MainUI\Panels\Vendor\Vendor.xml:24` binds
  `ON_VENDOR_PURCHASE_RESPONSE` to `OnVendorPurchaseResponse`; its Lua handler
  at `Vendor.lua:258-282` creates a loot event, confirms successful purchase,
  and refreshes highlights, or clears pending state and plays an error voice
  line on failure. The content-level handler matches the `229` payload shape,
  but its registration-order-derived ID is not proven to be `229`. Native
  literals `ON_VENDOR_PURCHASE_RESPONSE` (raw `0x1749050`, VA `0x1B4A450`)
  and `ON_PLAY_MOVIE` (raw `0x1749578`, VA `0x1B4A978`) have no IDA xrefs;
  neither binds a name to these codes. The dispatch model now records all
  three name/callback associations as null.
- Selector `0x79`'s parser requests `0x40` bytes, reads through `+0x3B`, and
  returns the same object pointer to the `tfGameProtocolHandler` slot `+0x168`
  (`0xACE300 -> 0x991510`). That handler reads `vendor_id` at `+0x40` and a
  code-string pointer at `+0x44`, with fixed fields ending at `+0x48`
  (exclusive); no object conversion or field expansion occurs in the traced route. Allocator
  `0x12C8A20` can return a block with eight bytes of slack when it keeps an
  unsplittable free block, but actual capacity is runtime-dependent and the
  parser does not populate the additional fields. Preserve this as an
  unresolved parser/receiver extent conflict; see `docs/MatrixConnection.md`.
- The `tfGameProtocolHandler` entries for Character selectors `0x74`–`0x77`
  now map to slots `+0x154/+0x158/+0x15C/+0x160` and raw handlers
  `0xACDD40/0xACDDE0/0xACE210/0xACE2A0`; PE-translated vtable pointers and
  `0x81BB90` switch cases are checked against the original. `0x74` updates a
  keyed owner-state record and dispatches numeric UI code `230`; `0x75` passes
  field addresses into an owner-state updater; `0x76` walks `0x40`-byte records
  and calls chat processing; `0x77` gates on the LookingGlass state, updates a
  QWORD-keyed record, swaps `0x20` bytes from message `+0x30`, and dispatches
  numeric UI code `356`. No names are assigned to those codes. `0x75`/`0x76`
  wire-state semantics remain incomplete. For `0x77`, the parser request and
  constructor/reader writes end at exclusive `+0x30`; the same parsed pointer
  reaches the receiver unchanged, which swaps `0x20` bytes at `[+0x30,+0x50)`.
  The class's virtual stream reader/writer also handle only the QWORD at
  `+0x28`; the original `.text` scan found one direct constructor call and one
  direct call each to `0x4C9250` and `0x51C9D0`, with no parser body transform.
  This does not exclude an untraced indirect entry. Requested size is not
  actual runtime capacity: slack/ownership and runtime consequences remain
  unresolved. See `docs/MatrixConnection.md`.
  Evidence is in `docs/MatrixConnection.md` and
  `src/Startup/CharacterAbilityApplicationNativeLayout.cs`.
- The dynamic listener path is traced through `0xBF9C40` to `0xC38400`, which
  resolves a listener-local event callback and stages delivery in structures
  at listener `+0x4C4` and `uiManager+0x5EC`. The concrete listener and later
  callback execution are not statically identified. Details and binary
  fingerprints are in `docs/MatrixConnection.md` and the separate
  `src/Startup/DialogApplicationDispatchNativeLayout.cs`. Do not repeat the
  registration-order trace unless new static registration-order evidence is
  available. The conditional `0xBD7060` serialization and recipient iteration
  are now documented, including the eight-byte recipient header. No code/data
  xrefs to wrapper `0x821DE0` (VA `0xC229E0`) exist in the current IDA database;
  its caller remains unresolved. Continue with the `0x75` source/destination
  tree meaning or selector `0x77` extent mismatch. Selector `0x76`'s
  `+0x28/+0x2C` conflict is now tied to the shared `slBlob` vtable/storage-
  object layout, with no conversion found on the traced callback route. The
  zero-control consumer path is mapped to event335's text/author/author-ID/
  channel/display-flag fields. The nonzero structured-chat switch now has all
  19 targets and 14 payload-reader calls recorded, with selected literal-backed
  effects; individual payload and owner-state semantics remain incomplete.
  Continue read-only; do not modify the sample/IDB.
- Open items: field semantics, allocation-failure consequences, selector
  `0x55` copied but unwritten stack DWORD at object `+0x70` (source proven,
  runtime consequence unresolved), selector `0x6D` outer framing/body
  consumption beyond the shared QWORD/compact-selector preamble (the case
  stores QWORD halves at `+0x18/+0x1C` without a selector-specific reader), selector
  `0x75`'s `+0x28` source tree and `+0x30` tree-vs-`slBlob` mismatch and
  selector `0x76`'s parser/receiver range-vs-`slBlob` mismatch (receiver uses
  the blob vtable/storage-object positions as range endpoints), its nonzero
  `+0x14` structured-chat payloads, selector `0x77` trailing-state extent. Whether an
  intervening conversion or distinct object path explains either mismatch
  remains open.
- Workflow per batch: preserve native evidence in the existing layout,
  binary checks, and documentation; validate the focused build and binary
  checks, without committing or pushing unless the user requests it.

Latest Matrix transport follow-up: deferred-control opcodes2/3 carry two
network-order frontier WORDs and optional selective sequence WORDs.
`0xBE43B0` advances queue offsets `+0x2C/+0x2E`, retires cumulative and
selective matches, updates retry timing, and prepares retries for old entries
in its 256-slot scan. Outbound ACKs are assembled at `0xBE4F20` with the
matching receive frontiers at `+0x2A/+0x30` and a 256-entry selective bitmap.
Queue flag `+0x32` is initialized, set after contiguous receive consumption,
and cleared by the ACK builder; it can request a frontier-only ACK.
The Login message-145 notification routes through the `uiManager` singleton:
bootstrap walks `slSingleton<uiManager>` and its slot+4 initializer stores the
constructed `uiManager` at VA `0x1E002B8`. Its slot+0xAC dispatches through the
event-ID registry. Named subscriptions map strings to numeric IDs through
`uiManager+0x6E8` before registering listener nodes at `+0x6F0`; the
message-145 name/subscriber and any direct message-145-to-World-mode request
remain unresolved.
The offline body decoder is in `GameSocketAcknowledgmentBody.cs`, with checks
in `GameSocketAcknowledgmentBodyChecks.cs`; full queue mutation and resend
behavior remain outside the reconstruction.

The installed UI's separate join path is now traced from
`ZoneSelection.JoinWorld` through `MY_PRELOAD_EVENT` and the preloader's
`System.SetupCharacter`/`System.StartInstanceConnection` calls to
`MatrixConnection_BeginConnection` and its oracle-ticket callsite on the
permitted destination branch. A nil `dev_matrix_url` becomes empty shared text;
Login's selected URL or `Development/MatrixOverride` may take precedence.
`SetupCharacter`'s existing-connection reset has no observed direct store to
preferred Matrix text `+0xBA0`, so an earlier URL can survive and reach the
oracle request.
Constructor `0x48ED50` zeros `+0xBA0` for a new connection. The runtime
cause/completion of the `tfWorld` request and the dynamic name for message
`0x145` remain open.
Native API bindings and binary
fingerprints are saved in `LoginModeNativeLayout.cs`; source hashes are in
`MatrixConnection.md`, and flow details are in `LoginMode.md`.
Separately, `tfLogin::Update` (`0x6385C0`) has a traced default-branch
request for mode `{2,0}` when the object pointed to by the DWORD stored at VA
`0x1E5CAA4` has byte `+0x40` equal to zero; its guard writer and relationship
to message `0x145` remain unknown.
The component XML `<Events>` parser now maps each `name` through `0xBF9AB0`
before registering its `bind`; `Component.BindEvent` uses the same dynamic map.
The `uiManager` constructor starts its name array empty, so ID `0x145` is the
326th distinct first-time name insertion. The order needed to identify which
name occupies that slot is still open.
The Login UI folder walker uses CRT `_wfindfirst64i32` /
`_wfindnext64i32` / `_findclose`, processes directory entries before files,
and shows no explicit sort in the analyzed path. Component XML order is not
recoverable from lexical filenames alone.

The latest render-init pass identifies the six `0xED1650` args (fullscreen flag, display w/h, third display value, window client w/h, forwarded to oD3DRenderAPI slot +8 `0xF08680`), the host class `oEngineSystem` (`0xE54BF0` = vtable `0x1C77C2C` slot 0) and `0x1DFE9C8` as `oShaderBuilderSystem`; see MatrixConnection.md. Cvar-field mapping and the path to the main loop remain unresolved.

The supported generated transformation-provider boundary was completed, then
explicitly resumed by the user for another bounded reconstruction pass. The
latest work extends the tfWorld zone-cleanup layout from static binary evidence;
read-only IDA follow-up also verified four constructor callsites for the
generated provider. Both caller paths use the constructor that writes mode
DWORD+4=0 and control byte+0x18=1 before generation; no override store was
found in those observed paths. This is not an exhaustive global writer search,
so native non-default mode/control reachability remains unproven.
Overall client reconstruction remains incomplete. No recurring session
automation is configured.

The latest bounded world-cleanup pass confirms the raw callsite bytes for the
resource getter dispatch (`0x860F44`, vtable byte offset `+0x24`) and returned
object factory (`0x860F6F`, member `+0x10`, vtable byte offset `+0x40`), plus
the result store at `0x860F76`. Both targets remain indirect: no concrete
vtable or static candidate was resolved, so slot0's deleting-destructor
interpretation remains unproven. The slot0 cleanup call at `0x8610C8` passes
argument1; this alone is not destructor evidence. Fingerprint checks now pin
these observed instruction sequences without asserting their targets.

The latest arena lifetime follow-up confirms that the traced block-return path
recycles spans through list metadata without coalescing or releasing the
generated-output regions. `VirtualFree` is present in the generic allocator
release path for other allocation classes; this does not prove output-region
teardown. IDA xrefs undercount byte references, so the negative result remains
limited to the inspected paths rather than a binary-wide claim.
Two additional raw sites, `0x12ABD6C` and `0x12ABDBC`, contain identical
`VirtualFree(ECX,0,MEM_RELEASE)` call sequences after a non-null test. IDA has
no function boundary or xrefs for them, and the caller/ECX provenance is
unknown; they are not evidence of generated-output region release.

Latest tfLogin pass: update `0x6385C0` reads a single pointer cell (`this+0x20` = ctor arg1 = `App+0x9E8`), not a node list; selector case 1 sends message `0x146` with `node+0xC/+0x10` and virtual-deletes the node. Pass 2: `App+0x9E8/+0x9EC` looks like a shared_ptr-like `{ptr, ctrl}` (App dtor funclet `0x16000E9` -> release helper raw `0x636B30`); only direct stores are zeroing (`0x303104`, `0x30DB8A`), so node producers/class/selector names are still unproven (indirect path); evidence check added. Producers of the cell and cases 0/2/3/4 are unproven; see `docs\MatrixConnection.md`.

Latest scene pass: raw `0xED1AC4` is the placeholder store (`0x1E72DF8`) in oRenderSystem vtable slot `+0x38` (shutdown, `0xED1A10`, gated on byte `0x1E49B7F`); `0xED196B` is the init (`+0x34`, `0xED1650`) failure rollback, not shutdown. The scene-name strings are referenced only by the factory; no name-keyed registry found (direct refs only). See `docs\MatrixConnection.md`.
