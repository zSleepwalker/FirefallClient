# Reverse-engineering handoff

ClientApi/WinHTTP details are in [ClientApiWinHttp.md](ClientApiWinHttp.md).

## Latest continuation navigation (November 4)

The [verified import and receiver mappings](#november-4-continuation-verified-import-and-receiver-mappings)
and subsequent sections supersede earlier dynamic-import assumptions,
PrivateDialog receiver identification, AES "block-size" interpretation,
and tracker `+0x20` uncertainty. Active-sample identity and raw/VA mapping
are recorded there; do not merge evidence from `.original` without comparison.

Application shutdown's service-cell dispatch now reaches the concrete console
file-close path. Current continuation is the UI shader-source `slBlob`
lifecycle: read gates/tails, capacity reuse, preservation copy, pooled/nonpool
allocation and release, and wrapper copy/destruction have captured code and
immutable PE anchors. See the latest appended sections for corrections and
active assignments. Singleton teardown and source-initialization copy are
active; callback publication and composite container semantic identity remain
parked. UI lifecycle, arithmetic/import regressions, and immutable backing-slot
projection code/checks are committed.
The traceback fallback's state-0 unwind map is decoded and has no restore action.
VM-frame insertion/removal is a bounded unresolved stop, not an ongoing scan.
Normal native CRT return selects `exit(status)`.
Exit registration and its logger cleanup predicate are resolved.
Generic wrapper field `+8` is a duplicated diagnostic label:
MSVCR120.dll!_strdup/free; the earlier constructor candidate is another destructor.
Logger unwind binding is now composed
from the recorded guard initialization and exact EH action. Assertion interception is a bounded
unresolved stop. Mode-16 fixed scatter and normal
buffer release are resolved.
Shutdown-request callers
and texture writer dispatch/serialization are bounded unresolved stops,
not ongoing broad scans.
Descriptor `+0x24/+0x28` are structure pointers, both nonnull on normal
startup, not boolean configuration fields.
Replay timing callback `0x65C1F0` is bound through the object's vtable `+8`.
App `+0xB78` is a one-iteration timing override, not a stop flag.
The worker's queue-work guard and normal null write-prefix override are resolved.
The normal application startup descriptor explicitly enables shutdown autosave;
constructor allocation/readiness and conditional shutdown-export trigger are resolved.
Local bind port and remote-peer
endpoint are distinguished; SavedLanguage qualifies for settings.con export,
but automatic persistence remains conditional on the documented shutdown path.
Endpoint matching and language selector default/setter/availability are resolved. Reader nonnull `+4`
attachment and localization row ordering are unresolved bounded stops,
not ongoing broad scans. Reset sample
identity is confirmed and the exact triples clear before holder destruction.
Publication owner identity/consumers remain unresolved; the constructor's reciprocal link
is distinct. Arming
`0xBDFD80` forwards no request bytes to its concrete diagnostic-only target;
the loader's success/reset callback order is resolved.
Case-0 destination replacement, borrowed metadata ownership,
outer discriminator framing, type-1 byte copying, metadata-binding hashes,
and localization missing-ID/language fallback are resolved. ManagedUDP packet byte snapshots and provider identity
`tfGlobalizationManager` are resolved. Inspected closing-state maintenance
has no independent timeout; sentinel producers, localization accessor,
field assignment, and service allocation are resolved in bounded scope.
IOCP stop observation,
keyed-close ownership, and extension format-operand mapping are now
resolved in the later sections. Completed traces distinguish GUI logout event 83
from application HTTP drain, readiness from manager admission, and returned
VM-stack cleanup from the earlier partial-parse callable-selection effect.
All remain static evidence, not observed runtime behavior.

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

## What is saved

- **Generated provider initialization prefix saved:** `NativeGeneratedTransformProviderPrefix`
  reconstructs count60..100 modulo draw BEFORE264 raw low-byte draws,
  headerWORD0200/byte3, initialDWORD+4=0/byte+18=1 and table at+C.
  Exactly265 UInt32 calls for this prefix ONLY. FromSeed uses saved MT;
  independent seed0 table SHA256 regression included.
  Read-only agent7ed40f8c-f5e1-42fe-9c6a-e812a685ac4e completed bounded
  construction trace. Outputs+10/+14 are arena-allocated assembled spans;
  original region allocation uses executable/read/write protection, but the
  reconstruction never allocates or executes them. Captured mode0 byte images
  are saved; no named cipher identity is established.
  Builder-local1285290/1285970 now defined through explicit verified ends
  128596C/1285E27; ECX-this/one-stack-argument prototypes recover decompile.
  `NativeGeneratedProviderBoundaryPlanner` saves modeDWORD+4==0:
  opening10/closing8 operation selections, raw operand descriptor codes,
  header-derived choices, offsets8/12 and final16-bit value8.
  `NativeGeneratedDescriptorFactories` now saves numeric code classification,
  code-to-operand sparse writes and no/unary/binary operation constructors.
  Preserve captured unassigned bytes; absent operands are not zero-filled.
  Immediate1299D00 uses floored bitWidth>>3 and LE byte counts1/2/4/8,
  including noncanonical bit widths; saved in NativeGeneratedImmediateBytes.
  Initial mode0 boundary candidates now saved; MOV EBP,ESP has89E5/8BEC
  alternatives in static table order. All other boundary steps have one.
  Immediate operand1284EF0 and post-candidate1286780 now saved:
  latter discards unsigned byte-sum%5 draws only when byte+18==0,
  rather than inserting junk bytes. Initial byte+18=1 disables it.
  IDA immediate/module-path switches are restored with SWI_USER protection;
  automatic reanalysis had replaced both mappings with erroneous PE VAs.
  Fixed data-section mapping for these objects is VA=raw+401C00, verified
  through original PE section headers; never apply a blanket406C00 delta.
  Named earlier temporary check logs have been cleaned up.
  NativeGeneratedBodyDecisionRules saves exact seven opcode pairs, mode0
  scratch/count-register parity and bounded table scale/index/window choices.
  Pairs are ADD/SUB, SUB/ADD, INC/DEC, DEC/INC, ROL/ROR, ROR/ROL, XOR/XOR.
  Reverse helper1287C20 reverses12-byte byte-vector records, not bytes.
  Generator builds forward/inverse fragment lists then reverses inverse.
  NativeGeneratedConditionalBranches preserves original JZ/JNZ range gates:
  short accepts0..255 except128/129 (no negatives), so130..255 wrap rel8;
  near rejects Int32 min..min+5 and subtracts6 only for negative offsets.
  NativeGeneratedByteLoopWrapper now saves exact captured-body initial-mode0
  byte concatenation and24 RNG draws per wrapper. Body fragments precede
  both wrapper builders; correct earlier assumption of interleaved body draws.
  NativeGeneratedScratchTemplates now saves exact initial scratch INC/DEC
  and byte ROL/ROR candidates, including count1's extra implicit-one form.
  NativeGeneratedImmediateMoveTemplates saves preparatory8/32-bit register
  MOV alternatives in compact/ModRM table order, including captured pointers.
  NativeGeneratedArithmeticTemplates saves initial BL/EBX ADD/SUB/XOR:
  one80/81 immediate candidate (no83), two ordered register-source
  candidates, one EDI or scaled-ESI table-memory candidate.
  Memory encoder129A0C0 and scale checker129A5B0 had faulty IDA switch
  mappings; restored direct/byte-indexed tables and validated calculated
  targets against original bytes. SWI_USER re-applied after autoanalysis;
  cached decompilation/block recognition can remain incomplete.
  NativeGeneratedInitialModeZeroImage now assembles both logical byte images
  with supplied captured table address and complete replayable UInt32 ledger.
  All-zero sample stream:60 iterations/735 draws/two405-byte images.
  Constant2:62 unary iterations/625 draws. Constant4:64 CL-path rotations/955.
  Scripted register and indexed table streams check local/global inverse
  fragment reversal and independent forward/inverse candidate draws.
  NativeGeneratedArenaPolicy saves wrapped length+16,64/128/256 preferred
  lists then large-list fallback, first-tie best-fit, remainder>=32 splitting
  and64KiB region rounding. Original PE IAT proves VirtualAlloc with
  MEM_RESERVE|MEM_COMMIT/PAGE_EXECUTE_READWRITE; model never allocates.
  NativeGeneratedProviderLifetimePlan saves RNG/table then output-release
  order and verified tail-jump/two-stack-word/RET8 callback image facts.
  Native release returns outputs to arena, not VirtualFree on that path.
  NativeGeneratedInitialModeZeroByteTransform saves managed positive-length
  low-byte effects from recorded operation steps, not machine-code execution.
  Logical forward/inverse round-trips cover all byte values/count boundaries
  for32 MT seeds. Empty input stays explicitly unsupported because of native
  short-JZ quirks. Request57 opt-in reconstructed-provider verifier applies
  managed inverse before MD5 while preserving existing selection bypasses.
  Arena singleton xrefs and adjacent1251220..1251900 helpers were checked;
  no true region teardown was established (do not invent VirtualFree).
  Mode1 boundary schedule/default-header bytes are now saved separately:
  six PUSHes; saved RCX/RDX loads from RSP+32/+24; six reverse POPs andRET.
  Original memory encoder emits unusual no-index SIB E4, not24; preserve it.
  Native constructor still writes mode0; no mode1 entry path established.
  Independent seed0 semantic golden now saved:61 iterations,684 pre-wrapper
  draws,732 total and exact513-byte output SHA256/first16 bytes.
  Explicit captured-control0 mode0 generation now saves exact native
  post-candidate discard sites, full replay ledger and discard totals.
  Do not apply discards globally: direct immediate arithmetic, table-base MOV,
  scratch loads and wrappers omit the helper. Constructor still writes1;
  no native control0 entry/setter was established. Default behavior unchanged.
  Incoming/outgoing direct provider consumers discard callback return values;
  thunk-entry callers and a meaningful generated return remain unproven.
  Mode1 memory-shape validation and separate no-SIB fast-path predicate saved:
  32/64-bit address registers, equal widths, no ESP/RSP index, scale1/2/4/8,
  index required for scaled forms; returned flag means address-size override,
  not successful validation. Explicit diagnostic results cover8712 shapes.
  Repaired mode1 direct/compressed switches at129A4C1/129A734 from original
  tables129A584/129A7D8+129A7E4; all six known switches protected and saved.
  Validator boundary now excludes tables and the separate129A7F0 predicate;
  cached decompiler block recognition may still remain incomplete.
  Mode1 captured ModRM/SIB/displacement field projection now saved with
  address-prefix selection and REX X/B bits. Default boundary MOVs reuse it.
  Preserve no-index R12/R13 native fast-path quirks (raw rm4/rm5, no canonical
  extra bytes), E4 SIB, signed displacement limits and absent-base DWORDs.
  Checks cover18657 accepted boundary combinations, not machine execution.
  Mode1 ordinary immediate MOV templates now saved: byte candidates unchanged;
  64-bit destination/source32 uses C7 only; source64 adds compact imm64 first
  and signed32-fit C7 second. Original CDQ128624F proves signed-DWORD table
  pointer expansion. Extended destinations remain outside this catalog.
  Mode1 scratch/ordinary-register unary templates saved: BL/RBX parity;
  one FE or REX.W FF INC/DEC candidate, no compact40..4F mode1 candidate.
  Mode1 ADD/SUB/XOR and operand-decision components saved: BL/RBX, CL/RCX
  or DL/RDX; one REX.W81 imm32, two ordered register forms, one RDI/RSI table
  form.264-byte table bounds checked for byte/64-bit reads and pointer wrap.
  Captured mode1 wrapper saved:21 selections versus mode0's24, RDX OR-test,
  RAX increment/RDX decrement, retained branch displacement quirks.
  Shared NativeGeneratedDefaultHeaderImage now composes complete logical
  forward/inverse default-header mode0/1 images with captured control byte.
  The old mode0 APIs remain compatible; constructor0/1 facts are distinct
  from explicitly supplied mode/control overrides. Native mode1/control0
  entry paths still unproven. No host pointer capture or native allocation.
  Mode1 zero stream:729 draws/two468-byte images; constant2/4:619/949 draws;
  control0 adds124/576 discarded draws respectively; seed0 total726.
  NativeGeneratedDefaultHeaderByteTransform evaluates recorded byte steps,
  not machine-code arrays.32-seed semantic/round-trip/full replay checks and
  independent seed0 digest retained. Arbitrary headers, empty-input safety,
  native region teardown and full CPU/ABI effects remain unresolved.
  **Completed boundary for today:** supported generated-provider projection,
  managed effects, numerical arena/lifetime and IDA metadata. Do not start
  another subsystem today. Suggested next work only after the user resumes:
  native override entry paths/arena teardown, or another requested subsystem.
  Final verification: complete Startup checks passed against both original
  and installed binaries; full solution build passed with zero warnings/errors.
  IDB saved after verifying all six protected switch mappings. No executable
  patches, native/generated-code execution, network requests or temporary
  source artifacts were introduced. Build outputs are not reconstructed source.
  No native execution; no callback delegates created.
  Calls12866EE/1286700 both virtual slot0; do not infer a UInt64 RNG method
  merely from decompiler temporary widths. No generated code executed.

- **Captured module-directory provider saved:** `NativeModulePathProvider`
  reconstructs126CBD0/126D9E0. GetModuleFileNameW(NULL) starts260 units,
  retries GetLastError122 with260+4*previous regardless returned count,
  no last-error clearing; terminal non122 needs nonzero return.
  Captured secure splitter result0, drive3/other parts256 capacities.
  Selector enum0 full path,1 filename,2 filename+extension,3 drive+directory;
  unsupported selector checked AFTER query/split. Selector3 feeds alias254.
  No module path or CRT query executed. Original PE jump tableVA166DAE0
  ->raw126CEE0; corrected IDA switch metadata and cases restore decompilation.
  Original/installed suites and solution build passed; native module/splitter
  names and corrected switch/ABI persisted in IDB. Generated provider
  prefix is now saved above; its remaining builder primitives are next.

- **Captured folder-prefix alias projection saved:** `NativeFolderPrefixAlias`
  reconstructs126DAC0: remove one trailing slash/backslash, ordinal `\\?\`
  prefix removal (static initializer2E33D0), fixed provider-ID order
  254,255,37,41,36,38,42,5,28,26,35,0,25,40,7. Per-unit captured towupper
  prefix match, whole-string/backslash boundary, first match not longest,
  replace root+one backslash with `<decimalID>` without separator.
  No match falls back to original input, not modified candidate.
  Root availability/case captures can remain unknown; unknown earlier
  provider is not skipped. No GetTempPathW/SHGetFolderPathW/locale calls.
  Original/installed suites and solution build passed; symbols/IDB saved.
  Provider254 is now saved above as module-directory selector3.
  Metadata ownership and generated activation provider remain boundaries.

- **Metadata text tail saved:** `NativeMetadataTextTail` reconstructs1283720
  shortening (>64 UTF16 units→first31 + literal dots +last30) and
  strict UTF16→UTF8 range/scalar encoders1258C70/1258E90. Stored NUL
  preserved; no BOM/terminator/replacement characters. Shortening may split
  valid surrogate pairs; explicit invalid-unit outcomes retain those cases.
  Every valid Unicode scalar cross-checked against standard UTF8.
  Input boundary is captured normalized/locale-towlower text, not raw paths.
  Earlier126DAC0 prefix aliasing and126D7D0 prefix removal are now saved
  above; provider identities remain partial. Original/installed checks and
  whole solution build passed; meaningfully named functions and IDB saved.

- **Mode-zero descriptor and zlib boundaries saved:** `MatrixModeZeroDescriptorCodec`
  writes WORD2, Windows-version DWORD, two raw16-byte digests and two
  opaque DWORD/ushort-length narrow-byte groups (+24/+28 and+40/+44).
  Provider metadata is captured after1273530 conversion; its UTF8 tail is
  now recovered separately above.
  `NativeWindowsVersionCode` projects captured GetVersionExW284 and
  exact native packed-version codes; no host OS query.
  124E720 compresses via11E8910 (native zlib1.2.8/default6/window15/memory8),
  appends original LE32 size, then124BC10 MD5 over compressed bytes+size.
  `NativeZlibBufferFraming` saves framing and captured inflater projection.
  **Correction:** incoming124AC10 calls120C250/1256A60 before reading key;
  the MD5-verified prefix is compressed, not the routing key.
  Incoming allocation is footer DWORD, prefilledFF; stream-end1 and unused
  input0 required, but shorter output accepted with FF suffix retained.
  Pre-transform serializer/framing only; generated provider+10 callback
  and queue/send effects remain unresolved. Original/installed checks and
  whole solution build passed; descriptor/zlib/version names and IDB saved.
  Next: generated provider1285060/1286F00 and metadata source ownership.

- **Windows-drive storage serial MD5 saved:** `NativeWindowsDriveSerialFingerprint`
  projects127C730/127C550. GetWindowsDirectoryW260, reject only zero,
  force UTF16[2]=NUL; device prefix `\\.\` +first2 units.
  CreateFileW read80000000/share3/open3; reject handleFFFFFFFF only.
  IOCTL2D1400,12 zero input bytes; first output8 and returned8, allocate
  header+4 size, second returned exactly size. Serial offsetDWORD+24;
  CString copied and standard MD5 raw bytes, no trim/case/hex conversion.
  Offset0 reads header bytes, not automatic empty/missing; valid empty
  serial succeeds with MD5(empty). Unsupported offset/NUL reads and
  incomplete captures report unknown native outcomes, not false/zero.
  Captured projection only; no directory/drive/IOCTL execution.
  Mode-zero result exposes WindowsDriveSerialSlot alias and
  ProjectCapturedFingerprints drives+adapters overload under a distinct
  method name. Original/installed byte evidence and complete solution build
  passed; native helper names/comments persisted in IDB. Downstream
  descriptor serialization/framing is now saved above.

- **Offline adapter-address MD5 saved:** `NativeAdapterAddressFingerprint.cs`
  projects127D0C0 with captured API statuses and registry-filter decisions.
  Original PE IAT proves1AC412C = GetAdaptersInfo; initial probe MUST return111,
  second call zero. Select types6/71 with address length>=6, then filter;
  copy exact declared lengths, sort via native prefix/introsort model and
  MD5-concatenate without counts/separators/deduplication. Success with no
  selected addresses yields MD5(empty); API failure leaves output unwritten.
  Filter127CCD0 uses HKLM Network/{4d36e972-...}/adapter/Connection and
  PnpInstanceID; missing/unopened key ALLOWS, available key's failed query,
  non-REG_SZ or failed data read REJECTS. Prefix before first backslash is
  case-insensitive pci/usb/bth. ASCII/C-locale captured projection only;
  unresolved filter/non-ASCII comparisons never invent a digest.
  No host adapters/registry were queried. Upstream digest consumers and
  original identifier semantics remain bounded: mode-zero selector124B7C0
  copies this digest at124B93A only on true return, else retains initialized
  zeros and proceeds. `MatrixSynchronizationModeZeroDigestSlots` saves
  opaque first slot+4 and adapter slot+14, independent copy decisions,
  explicit unresolved attempts and exact16-byte capture requirements.
  First helper127C730 is now saved; downstream descriptor clone/transform
  at124E010 and generator-wrapper1285060 remain next targets.

- **Byte-buffer introsort saved:** `NativeByteVectorSort.cs` reconstructs
  sort127E8A0, partition127EB60, median127F3D0, order-three127F8F0,
  insertion127F6F0 and count-initialized wrappers127E4F0/127E6D0.
  Threshold32 before budget check; positive ideal becomes half+quarter;
  recurse smaller side/tail-iterate larger; exhausted budget -> heap sort.
  Ninther starts at42 records (native compares last-index difference to40).
  Partition dynamically reads its first pivot representative; absorbing
  empty prefixes can collapse comparisons between otherwise differing bytes.
  Insertion's first-element branch shifts the whole prefix, preserving
  [02,empty,01] ->[01,02,empty]. Original partition-range bytes verified
  before IDA-only function/type edits; caller-cleanup ABI recovers result pair.
  One movzx instruction remains data/JUMPOUT in IDA; original Capstone
  decode is authoritative there. Fixed-width checks cover2048 records,
  duplicates, all rotations, threshold and forced-budget paths.
  Concrete next caller:127D0C0 enumerates adapter-like records, filters,
  sorts copied addresses and MD5-hashes their concatenation. Verify import
  off1AC412C and filter127CCD0 before assigning device-ID semantics.

- **Owning byte-buffer heap algorithms saved:** `NativeByteVectorHeap.cs`
  corrects the adjacent-routine scope: 12-byte begin/end/capacity headers,
  not scalar triplets. Comparator127C8F0 compares unsigned bytes only through
  the common prefix and returns false without a length tie-break, including
  empty buffers. Prefix equivalence is nontransitive across arbitrary lengths;
  do not substitute ordinary lexicographic ordering or assert an unproved
  equal-width caller invariant. Make127F550, sort127F610, adjust127FA80,
  push127FBD0 and duplicate pops127FB10/127FC50 are projected over copied
  buffer contents. Right child wins equivalent-child comparisons; push stops
  at top; sort requires an existing heap. Pointer/capacity/allocator behavior
  is not emulated. Whole outer sort127E8A0 is now saved separately above.
  No general payload meaning is established by the heap alone.

- **MT array seeds / inclusive range saved:** `NativeMt19937` now supports
  nonempty seed-word spans, standard init_by_array1280010 (19650218,
  multipliers1664525/1566083941, finalstate0=80000000). Canonical four-word
  vector and independent Python stdlib goldens cover lengths1/623/624/625/1000
  across2000 outputs; empty input is rejected, not defaulted.
  `NativeRandomInclusive32` saves127FD70: virtual draw BEFORE unsigned
  lower/upper validation, diagnostic1C271921 for reversed bounds,
  inclusive modulo width with bias, and full[0,FFFFFFFF] width0 fault
  reported explicitly. Singleton/failed draws advance MT state.
  Native constructors twist immediately; managed model lazily twists
  before first output with same sequence. No native faults/activation.
  Float wrappers and profiler virtual60 provider remain useful next targets.

- **Outgoing profiler name requests saved:** `NativeProfilerNameRequests.cs`
  reconstructs concrete RTTI `tfServerProfiler` vtable1C61B50 slot8/6AAB20.
  Mode2 first; selector1 matrix30/channel1 (>3, logmask01), selector0
  firefall18/channel2 (>4, logmask02, raw routing pair878/87C, tick*1000
  atobserverB0). Both nonzero observer820. Wire normal buffers preserve
  duplicated/ordered root/node WORD lists,255 chunks/zero terminators.
  Matrix30 header alone; firefall18 has rawLE64 prefix then header18.
  Frame61's endpoint virtual8 ABI matches, but global virtual60 concrete
  provider is **not proved**. Global1DFC82C statically points to cell1E00470;
  runtime installer/provider unresolved. Do not claim end-to-end binding.

- **Profiler derived-node subtree snapshot codec saved:**
  `NativeProfilerNodeSnapshot.cs` projects E8CE00/E8CE70 + E8DD00/E8DDA0.
  Signed32 count -> child bodies -> raw expansion byte ->80 physical
  samples packed26 bytes (omit native padding2) -> raw next-slotDWORD ->
  captured CString/NUL. Empty-name leaf2090 bytes. Negative count reads
  no children; encode re-counts actual children. Kind/ID are not saved;
  derived factoryE8CEA0 creates kind0/IDFFFF, so DTOs stay separate from
  live tree. Iterative600-depth encode/decode, every truncated leaf prefix,
  raw flags/FFFF index, independent bytes and explicit malformed/cyclic
  input errors saved. No network61/whole-file framing or object activation.

- **Profiler display-name getter saved:** `NativeProfilerDisplayNames.cs`
  projects virtual+10/E8CD70. Nonempty cached shared text+8E4 overrides
  kind/table lookup; getter itself does not populate/clear the cache.
  Kind1 root names/E648E0 and kind2 node names/E64920 use full WORD IDs.
  Missing ->`<Waiting For Server>`; present-empty ->empty; kind3
  ->`[Exclusive]`; other kinds ->`<Error>`. Snapshot writerE8CE00 copies
  resolved C-string into cache before base/name output; readerE8CE70 loads
  cache. No invented invalidation. Getter144 analysis bytes defined from
  original instructions; executable unchanged. Full profiler node snapshot
  codec remains a bounded follow-up.

- **Profiler graph histories coupled:** `NativeProfilerFrameRefresh.Apply`
  saves E8DB00 root/child refresh and E8D760 regular/exclusive refresh.
  Important native mismatch: roots are found forward by WORD ID, but their
  direct children are paired **positionally**, not by ID. Old[20,40], new[40]
  writes40's sample to20's history. Deeper children use forward-ID matching;
  missing children are skipped without decoding/aggregation. Only processed
  source children contribute; stale history stays untouched. Empty children
  append zero roots; absent roots do not advance. Zero-filled80-slot node
  histories, iterative bottom-up updates, explicit mismatch/skip counters
  and partial-write unsupported-half outcomes saved. No native GUI execution.
  Next useful boundary: outgoing missing-profiler-name request codec via
  endpoint virtual8; alternatively cached profiler-name resolution.

- **Profiler selection recovery saved:** `NativeProfilerTree.MergeWithSelection`
  retains exact visited objects (including exclusive children), otherwise
  searches preorder by kind/WORD-ID, recovers first stale duplicates, and
  expands ancestors only. `FindVisibleRow` preserves expanded-child preorder
  excluding the hidden container, whose expansion is an explicit argument.
  E72480 is generic visible-row lookup, **not handle creation**. Frame owner+0C
  is selected-node pointer; its null-row0 fallback runs even when refresh
  is suppressed. E8D3F0 identity lookup ignores collapsed flags. New nodes
  begin collapsed. No GUI execution; numeric history and names remain separate.

- **Profiler persistent tree saved:** `NativeAlgorithms\NativeProfilerTree.cs`
  reconstructs rootE8D440/childE8D010 structural merging. Native kinds
  root1/node2/exclusive3; WORD ID8DC. Forward-only sibling cursors retain
  stale nodes and can create duplicate IDs on reorder (old[1,2], new[2,1]
  ->[1old,2,1new]). No global dictionary normalization or erasure.
  Every regular node gets same-ID exclusive first child; roots do not.
  Stable managed references, parent links and iterative600-level handling
  saved. `MergeStructure` remains structural-only; optional selection is now
  saved separately. History updates/name resolution and native allocation
  behavior remain separate.

- **Profiler61 numeric samples/history:** saved
  `NativeAlgorithms\NativeProfilerSamples.cs`. Decoder1303020 copies
  three raw WORDs and expands five binary16 values through table1C165B8,
  float32×65536 then signed64 truncation/low32. All65536 table entries
  checked (finite exact; nonfinite classified). Finite negative wrapping
  retained; exceptional nonfinite CRT behavior explicitly unsupported.
  Saved exact28-byte logical sample fields, per-child saturating exclusive
  DWORD4 subtraction/divisor, wrapping inclusive/root aggregates, and
  filled80-slot history (template/index0, modulo80). Numeric functions
  E8D760/E8DB00; initializerE8E090. UI tree matching/materialization and
  metric units remain unknown; no rendering/native execution.

- **Profiler frame61 name recovery:** saved
  `MatrixServerProfilerFrame61NativeLayout.cs`. Roots use owner+190 names;
  nodes use+1B0 names. Missing IDs preserve root order / node preorder and
  duplicates; present-empty names are known. Iterative collector covers
  deep trees. Missing-name request selection uses unsigned elapsed>5000,
  updates marker18C before endpoint lookup, and advances it even on null
  endpoint. Unknown virtual8 request carries owner1D0 and two WORD vectors.
  Independent suppression byte1E49A5A gates opaque E8D440/E8DB00 refresh,
  not collection/requests. Frame entryE65290 had false data classification
  in E65300..E653DE; corrected222 analysis bytes, no executable patches.
  Numeric samples, structure and selection are saved in the later units above.
  Open: outgoing missing-name request encoding and native rendering details.

- **Profiler names60 persistent state:** saved
  `MatrixServerProfilerNames60NativeLayout.cs`. ConsumerE64A00 updates
  independent WORD-key/shared-text maps at global1E6A470+190/+1B0.
  Native lookupE671C0 hashes both key bytes and compares WORD, not BYTE.
  Existing keys retained, duplicates last-value-wins, empty replaces text;
  empty vectors do not clear maps. Enabled handler stores tick-5000
  wrapping at+18C; absent consumer changes nothing. Raw-text copies
  and gates saved, native refcounts/order omitted. Name categories and
  frame61 consumerE65290 remain unresolved.

- **Native MT19937 saved:** `NativeAlgorithms\NativeMt19937.cs` reconstructs
  seed127FFD0, twist127FF20 and UInt32 output127FEA0. Vtable1C7EBAC
  slot0 is output; slot4/127FE40 is deletion. Standard624-word /397-lag
  twist and9908B0DF table proven; native pre-shift masks resolve to
  standard tempering. Canonical5489 outputs plus separate native split-loop
  projections cover2000 outputs for0,1,5489,FFFFFFFF and reseeding.
  No native activation; array seeds/inclusive bounds are now saved above.
  Float wrappers remain open.

- **Synchronization dispatch clarified:** `MatrixSynchronizationDispatchNativeLayout.cs`
  saves mode0's LE16 key followed by two LE32 values. Helper124E6E0
  reads both DWORDs through1206B20; **second DWORD==8 is a value gate,
  not a buffer length**. Key1 / value8 select native activation; first
  DWORD seeds the624-word MT19937 recurrence at127FFD0. Opaque activation
  is not executed. Short reads precede diagnostics; trailing bytes are
  ignored by this field path. Nonzero mode124C010 is hash-map lookup and
  virtual callback, **not proven queuing**; prior tentative wording corrected.
  Saved raw diagnostic codes and plain-FNV32 numeric-key/split-bucket
  projection. Shared hash prefix extraction preserves chat/SDB final mix.
  Next boundaries: callback registration/handlers and external transform;
  float generator wrappers remain untraced; inclusive unsigned range is saved.

- **All35 incoming original class names:** saved
  `MatrixServerMessageNativeTypes.cs`, exposed separately from numeric logs
  through catalog entry `OriginalType`. Factory33D4B0/jump-table33E250,
  distinct constructors, vtable/COL/type descriptor and exact names are
  recorded and binary checked using PE section-aware translation.
  Key clarifications:57 SynchronizationRequest;58 GamePaused;59 SuperPong
  (older SuperPing C# labels came from diagnostics);60 ServerProfiler_SendNames;
  61 ServerProfiler_SendFrame;67 ReceiveEmergencyChat. No field meanings
  inferred solely from names.

- **SynchronizationRequest57 consumer envelope:** saved offline
  `MatrixSynchronizationRequest57NativeLayout.cs`. Consumer124B3F0
  rejects nonzero owner gate70 and buffers shorter than20; mode1C can
  invoke an unknown mutable-buffer external hook before verification.
  Standard MD5 of preceding bytes must match the final16. Leading LE16
  becomes registered-handler/direct dispatch key; remaining semantics unresolved.
  Verify only already post-transform bytes; no hook or downstream execution.
  Golden digest, prefixes, mutations, MD5 padding boundaries, copy isolation
  and dispatch selections are covered. Whole-remainder57 wire boundary
  remains unchanged. Next useful targets: direct dispatcher124B7C0 and
  registered key helper124C010; profiler60/61 consumer identities.

- **Native sigscan traffic names and request32 wire:** RTTI identifies
  RequestSigscan32 vtable1C62384 and SigscanData34 vtable1C62354.
  Saved offline `MatrixRequestSigscan32Wire.cs`: exactly6 application bytes,
  code32 + LE32 cache key + raw flag; **no entity64 prefix**.
  Producer flag is first argument==0; second argument is the key.
  Sender31D350/encoder31E7B0 submit channel1; reader345D20 mirrors fields.
  No live requests/scanning performed; flags/key formal identities and
  returned-snapshot caller free remain unproven.

- **Message34 accumulation resolved:** application+C44 is embedded RTTI
  slBlob (vtable1C80CC4), not an arbitrary transformation pointer.
  Virtual18 appends raw bytes; virtual14 clears logical length with **no
  argument** (corrects earlier decompiler "consume length" claim).
  Saved accumulation/snapshot/clear and cached deep-copy projections.
  Snapshot includes previous chunks; zero flag preserves prior pending/cache.
  Helper30EDA0 returns an independent copied pair/data. Request30EED0
  compares cache keyC40 and pairC3C, frees old cached data/pair on miss,
  requests message32 then waits for34 completion. Full request32 encoding,
  callback synchronization and returned-copy caller free remain untraced.

- **Sibling chat initialization:** Login63B250 gates on sampled application
  A28 object's byte34, constructs chat at Login14, generates a 64-bit pair
  via BC9A20, forces type1 and copies explicit CString argument into chat10.
  Generator is native lock-xadd old DWORD counter1DFC588 shifted8, OR base
  pair1E487F8, clear low8; returns EDX:EAX despite misleading DWORD
  pseudocode. Saved pure sampled-counter/base projection and wrapping next
  counter. Do not call it an authenticated/account ID getter.
  LookingGlass65A880 constructs fresh zero/empty identity at+68; later
  replay producers untraced. Source/meaning of Login CString still opaque.

- **World tfChat identity source recovered:** successful World setup tail
  64B59C..64B5F4 loads World owner+10, copies owner+848/+84C into chat+8/+C
  with low byte forced1, and copies owner+850 CString into shared text+10.
  Saved raw-byte `MatrixChatIdentityNativeLayout.cs`; no invalid-ID filter
  invented. This resolves the World source of the chat-log checksum text.
  The owner pair also feeds authentication; formal account/character field
  identities and Login/LookingGlass producers are still not proven.

- **tfChat file lifecycle:** RTTI proves the former generic mode-specific
  object is tfChat (vtable1C67CDC/type descriptor1E12EF8). Saved bounded
  initialization/flush/shutdown/OpenChatLog selections in
  `MatrixChatFileNativeLayout.cs`. Path is cached VIBE base +`chat.log`;
  first-attempt flag0 uses `wt`, later uses `at`, and the flag becomes1
  even when opening fails. UTF8 path/mode conversion leads to `_wfopen`;
  content uses CRT text mode. OpenChatLog flushes present FILE then invokes
  ShellExecuteA, requiring chat but not a successful file open. Models
  perform no IO/viewer launch. Text+10 original name/source remains unresolved.

- **Chat file-log checksum:** post-runtime6F46F0 uses mode FILE*+18,
  native timestamp/label/text formatting, and folded modified-FNV32 suffix.
  Saved raw-byte prefix, hash input and pre-CRT line generation in
  `MatrixChatLogNativeLayout.cs`. Input includes prefix NUL, author ID with
  low8bits cleared, exact static25 bytes, and first32 bytes of mode text+10
  (positional meaning unresolved). Hash12F1780 uses FNV1a plus final mix,
  not CRC/plain FNV; shared existing SDB mix via linked
  `src\Shared\EngineByteHash32.cs`. SDB seeds/cipher behavior preserved.
  Native time locale, log-file opening/lifetime and CRT newline translation
  remain outside the model. PE imports also establish `_stricmp` for the
  chat channel parser; no ordinal comparison assumption is made.

- **Shared display36/67 resolved:** server67 is **zone chat**, not whisper:
  fixed category1 label `"ZONE"` is lowercased to `"zone"`. Native6E3460
  emits runtime335 channel/author/text, optional nonzero author_id, and
  dev/mentor/ranger/publisher/use_encounter_icon from bits0/1/2/3/6.
  `MatrixZoneChatPayload` now names these proven fields. Saved projection,
  announcement broadcast mapping and distinct category2/4/11 gates.
  High DWORD flags are not extra roles; all nonzero IDs are forwarded,
  without challenge/LFG validity filtering. Post-runtime6F46F0 consumer and
  callback reentrancy remain unresolved.

- **All35 normal-buffer server bodies34-68 saved:** final34 body is a
  WORD-length opaque blob plus raw flag;36 is announcement type/data raw
  texts. Only exact `"sysmsg"` displays with label `"broadcast"` and an
  existing mode. Nonzero34 flag snapshots the post-virtual-operation manager
  buffer; zero does not clear existing pending state. Added all35 minimum-
  packet/one-byte-short coverage checks and `MatrixServerMessageBodies.md`
  index. This is wire-layout coverage, not complete field/consumer semantics
  or alternate-header-buffer support.34 blob transformation and snapshot
  lifetime remain unresolved.

- **Matrix63-68:** saved two/seven WORD samples63/64, original-log-named
  LFGMatchFound65 and LFGLeaderChange66, positional QWORD/text/text/DWORD67,
  and68's two mandatory EnterZone-schema metadata maps (no presence byte on
  wire; destination optional becomes present). Separate LFG+48/+50 gates
  and nonmonotonic WORD-sample destinations are modeled. Corrected misleading
  native quick-chat labels at6F4A50/6E8BB0 to bounded LFG roles; actual RTTI
  Character command144 remains unchanged. Full LFG/67 consumers and sample
  meanings remain unresolved. Normal-buffer bodies now cover37-68;34/36
  are the remaining initial catalog bodies to inspect.

- **Matrix56-61:** saved ForceUnqueue reason/QWORD list/DWORD;57's whole-
  remaining-buffer body (not length-prefixed);58's shared five-QWORD/boolean
  clock parameters;59 SuperPing QWORD list/DWORD;60's two WORD/raw-text lists;
  61's recursive WORD records. Native61 roots stride16, nodes stride32:
  nodes serialize nine WORDs before their child list, not native memory
  order. Iterative decoder covers600-level depth and255/256 chunks.
  ForceUnqueue runtime291 reasons/local-player ID mask and clock58's separate
  stored/running pause flags, rebase, retained repeated bound and native
  assertion conditions are modeled. Full57/60/61 consumer semantics remain
  unresolved; do not invent event names or text encodings.

- **Challenge51-55 continuation:** saved QWORD-only51/53/55, QWORD/chunked
  native24-byte-record list52, and QWORD/DWORD54 wire bodies. Native24-byte
  records contain only QWORD/text/byte/DWORD/byte on wire, not padding.
  `MatrixChallengeUpdateNativeLayout.cs` preserves distinct ID policies:
  51/52/54 accept exact or invalid stored ID;53/55 require exact equality.
  Runtime boundaries are275/273/276/277/278, respectively. The word selector
  chooses first matching16-byte join record, or raw all-FF index. Meanings
  of that selected word and runtime276/278 remain unresolved.

- **Challenge follow-up continuation:** server48 cancellation is a single
  QWORD;49 invitation response is QWORD/name/status;50 kicked notification
  is QWORD/kicker-name/kickee-QWORD/kickee-name. Saved normal decoders in
  `MatrixServerMessages48To50Wire.cs` and snapshot selections in
  `MatrixChallengeFollowupNativeLayout.cs`. Cancellation selects every
  exact invitation-ID match. Response runtime272 accepts only status0;
  all8 reason names plus explicit `UNKNOWN` for8..255 are recovered.
  Kicked runtime274 uses kicker/kickee labels; matching local ID selects
  active+E4 teardown and runtime275. Native null-local getter becomes0 for
  that comparison; no valid-identity assumption or reentrancy simulation.

- **Found-match/challenge continuation:** saved server44-47 bodies in
  `MatrixServerMessages44To47Wire.cs`. Native logs establish44 as
  FoundMatchUpdate with matchmaker/match IDs,45 as a challenge join response,
  and46/47 as invitation variants sharing a consumer with final flags0/1.
  `MatrixChallengeNativeLayout.cs` captures join-ID gating, active-object
  release selection and runtime269/270/277 reason precedence; deeper party
  behavior remains unresolved. Added bounded found-match deadline/suppression
  selections to `MatrixPvpQueueNativeLayout.cs`. List reader reuse now lives
  in `MatrixChunkedListWire.cs`; native16-byte record padding is never assumed
  to be wire padding, and raw join-flag bytes normalize only at the handler.

- **Adjacent server-body continuation:** `MatrixServerMessages41To43Wire.cs`
  now decodes41's opaque scalar prefix/optional12-byte entries/blob,42's
  PvP queue record list plus comparison word, and43's DWORD/WORD body.
  Both list readers use255-chunk counts and preserve order/duplicates.
  PvP consumer `0x85D560` accepts nonempty records on wildcard9 or a matching
  raw comparison word, but runtime289 `"queued"` reports original input
  presence even if mismatch clears stored records. Saved bounded selection
  in `MatrixPvpQueueNativeLayout.cs`; text/record field identities and deeper
  callbacks are unresolved. Handler43 has no observed payload use, but its
  wire body is still present. See the final `docs\MatrixConnection.md` section.

- **Latest Matrix lifecycle continuation:** queue status is server message62,
  not ExitZone; runtime295 exposes the two raw DWORD properties. Actual
  ExitZone40 is header-only and uses outgoing header-only ExitZoneAck19 on
  channel1. Both wire/state models are saved. The follow-up at `0xBD2090`
  establishes a socket destroy request, not unconditional object release:
  active states select Destroyed8, disconnected/NotOpened select Closed9,
  and terminal states assert. `GameSocketDestroyNativeLayout.cs` preserves
  this selection and the first atomic-marker gate. Deeper world cleanup
  (`0x64E210`, secondary connection cleanup `0x494A40`) is saved as raw
  offsets/call boundaries in `WorldZoneCleanupNativeLayout.cs`. Follow-up
  static evidence records that the state-5 test is assertion-only with visible
  fallthrough; the downstream action pointer gates `0x7FEB50`; helper
  `0x976430` records two link chains, value/node release order, an event367
  callback, node+0x20 nested-range walking followed by separate slot release,
  and two 16-entry range helpers that can replace their backing pointers. The
  `0x9795B0` call takes the `0x3F7680` full-tree clear path; `0x3F7300` clears
  child nodes and the owner count, while `0x3F7430` handles partial erasure.
  `0x975020` adds two nested sentinel walks and tree-like node-removal helpers.
  Two other callers pass node+0x10 values to it with different guards; the
  `0x977F20` value+4 loop is a synchronous drain because `0x978440` directly
  decrements that field. This supports a compatible shape, not a proven type.
  Container identities and callback effects remain unresolved. Local-client
  cleanup `0x64E210` has no observed direct pending-mode enqueue: its current
  xref lists only caller `0x494B00` (state5), and the guarded `0x369A30` path
  forwards an opaque local record to `0x101D710`. This is not a transition
  proof or a binary-wide negative. Reset helpers `0x4A2FB0`/`0x431C90` are
  bounded. The `0x861020` path also
  releases a guarded record through `0xD74A20`; its `0x3C8` header, state
  comparison, token-vector removal, and cleanup are saved without claiming an
  app-mode transition. Its separate +0x32C virtual cleanup object is populated
  by a runtime vtable result; slot0 argument1 is observed, but its target is
  unknown and is not named as a destructor. `0x424790` atomically
  releases the pointer at released-object+0x320. Its tag-0x17 final-release
  branch routes the header to the shared-text allocator's lock-free free
  stack; other tag meanings remain unresolved. General ownership remains
  unresolved. Startup checks
  against original and installed binaries and the full solution build passed.
  Deferred close is now
  traced: Destroyed tick checks keepalive/retries, assembles data, then the
  assembler tail requests pump close if still Destroyed. The close callback
  sets+D94 and enqueues token1; queued dispatch changes Destroyed8 to Closed9
  with channel cleanup, or active1..5 to DisconnectedNotify6. Saved in
  `GameSocketCloseCompletionNativeLayout.cs`. See the final
  sections of `docs\MatrixConnection.md`.

- `Firefall.Reconstruction.slnx` builds the partial C# reconstruction. Start
  with the root `README.md` for the subsystem list and check commands.
- `src\Serialization\` contains the SDB block reader, schema-inspection CLI,
  type table, fixed-width field conversions, and runtime row-layout helpers.
  `tests\Serialization\` has synthetic checks. The most recent pass added
  float/integer and half-float conversion paths, runtime-layout comparison,
  first-match field-hash mapping, source/destination null-bit operations, and
  bounds-checked raw row/field access. A helper expands parsed table metadata
  into the compared runtime layout, normalizing type bytes >= 27 to zero.
  `SdbRowConverter.ConvertRows` now reconstructs the bounded field-hash
  fallback for scalar conversions, null propagation and zero-initialized
  unmapped fields; it does not model native callbacks or the identical-layout
  fast path. The native differing-layout row-buffer allocation and callback
  sequence is documented in `src\Serialization\README.md`.
  SDB table fields formerly called `UnknownA` and `UnknownB` are now named
  `RowStride` and `NullBitmapOffset`, and the
  field's `Unknown` is now `ValueOffset`, based on row-mapper reads.
  See `src\Serialization\README.md` for exact instruction offsets and limits.
- Other documented areas: `src\Containers\`, `src\Collections\`,
  `src\Graphics\Nvapi\`, `src\Characters\`, `src\Scripting\LuaCache\`, and
  `docs\Startup.md` / `docs\Platform.md`. These are separate partial
  reconstructions, not a replacement client.
- **App-mode dispatch update (this session):** `Client_ApplyPendingAppModes`
  (`0x30D850`) maps queued mode values 1/2/3 to RTTI-confirmed
  `tfLogin`, `tfWorld`, and `tfLookingGlass` objects, allocated at `0xA58`,
  `0x2904`, and `0x1368` bytes, respectively. `tfLogin` is also identified by
  its initialization-failure string; `tfLookingGlass`'s RTTI hierarchy
  includes `nl::ReplayReader::GameInterface`. Follow-up read-only traces
  identified the second interface vtable and replay-reader initialization,
  failure, completion, and disabled UI binding; mode 2's constructor, scene,
  layer objects, and selected virtual behavior are documented in
  `docs\WorldMode.md`. `src\Startup\` now records the replay interface and
  tested `tfWorld` layout constants. Full evidence and uncertainty boundaries
  are in `docs\Startup.md` and `docs\WorldMode.md`.
- **`src\Characters\` major update (this session)**: found the full, dense
  array of all 169 `firefall::Character::Commands` message-class vtables
  (raw `0x1864cd4`-`0x1865774`, 16 bytes/entry, zero gaps) by noticing two
  newly-traced classes' vtables sat back-to-back and walking the array
  outward. Dumped the complete catalog (class name + vtable + 3 vtable-slot
  addresses + COL/type-descriptor addresses, all raw) to
  `src\Characters\MessageCatalog.csv` — this is now the master index for all
  future Character::Commands work. Also documented and validated a general,
  reusable **RTTI name → type-descriptor → COL → vtable → constructor**
  raw-byte-search pipeline (IDA's automatic xref analysis is unreliable in
  this raw-loaded IDB) — see `src\Characters\README.md` "Technique" section.
  Used it to fully trace two new classes, `ResetTutorialId` and
  `DismissTutorialId`: both are a single `tutorialId` u32 field at `+0x28`
  with a `[1,14]` range check in both a production and debug-logging
  constructor variant; `DismissTutorialId` is built from a Lua-script-exposed
  dispatcher (raw `0xa2ba30`), confirming it's reachable from in-game script.
  **Caveat**: the 3 generic vtable slots per class are *not* per-class
  Sender/Encoder/Gate functions — IDA has merged many of these small stubs
  into large undifferentiated blobs (e.g. raw `0x9B73F0`, 0xEF8 bytes), so
  they resist cheap per-class tracing; the constructor-via-vtable-xref step
  of the pipeline is the practical way to learn a class's fields instead.
- The active IDA database at
  `D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe.i64`
  contains saved function names and annotations, including the renamed
  `Sdb_InitConvertedTableResult` and lifecycle comments for
  `Sdb_ResetLoadedDatabaseState`. The original executable was not modified.

## Current findings (October 2 continuation)

These updates supersede older next-step suggestions below where they overlap.

- **SDB result-holder teardown is resolved.** The tree at database `this+0x34`
  maps catalog table hashes to native 0x20-byte result holders. The holder
  stores catalog pointer at `+0`, row count at `+4`, exposed rows pointer at
  `+0x0C`, and a refcounted handle at `+0x18`. Reset follows the holder's
  catalog pointer to the optional catalog `+0x24` callback, calls it with the
  holder's count and exposed rows pointer, then releases/frees the holder.
  `SdbNativeMappedTableResult` models the verified 32-bit layout. Catalog
  compiled catalog. Its opaque tail shows 573/575 hex-string/DWORD pairs and
  two exceptions; no semantic meaning or consumer is established. The separate
  database tree at `this+0x28` is now laid out as an auxiliary buffer map whose
  key is also used as a backing-buffer displacement; key/value semantics and
  conditional value ownership remain unresolved.
- **SDB row-handle dispatch is bounded.** The example handle vtable at PE VA
  `0x1C6883C` is at raw file offset `0x186743C` by the executable's section
  table (the PE headers give this `.rdata` section a VA-minus-raw delta of
  `0x401400`). Its first two pointers map to raw `0x70D3F0` (slot 0) and
  `0x716B20` (slot 1). Slot 0 stamps `unk_1C68830` into the object and
  conditionally calls raw offset `0x12C8D60`; slot 1 clears global state and a
  16-bit member at `+8` for 12-byte records in a global range. The relationship of
  those slot effects to row-handle ownership is not established.
  `Sdb_InitConvertedTableResult` initializes holder `+0x18` to null; the
  mapped-row path has no assignment after initialization, so the non-null
  producer remains unknown. In the base-zero IDA database, `0x13122D0` is a
  raw file offset with 283 code-call xrefs. The decoder at raw `0x131233C`
  uses bit 0 as an encoding selector: set means displacement `(value>>1)+2`
  and a `u16` length read at `dword_1E4BAD0+value`; clear means displacement
  `(value>>1)&0x7FFFFF` and length from the DWORD's high byte. Its fingerprint
  is `8B 0B 8A 03 D1 E9 24 01 74 05 8D 79 02 EB 08 8B F9 81 E7 FF FF 7F 00
  84 C0 74 0B A1 D0 BA E4 01 0F B7 34 01 EB 04 0F B6 73 03`. A 15-function
  sample across the binary shows `arg1` pointing at runtime object/table
  fields (including direct `this+offset`, table slots, and map-result fields),
  not a consistent `.data` constant or a common type-tag class. The returned
  values are used as string-like data in sampled callers; exact producer-side
  meaning of the two encoded forms and common ownership remain unresolved.
  Examples: table slots at raw `0x3090F0`; record field `+0x10` at `0x38BCD0`;
  map/member sources at `0x63D160`, `0x4B6B40`, `0x6A8610`, `0xAB60C0`,
  `0x1333080`, and `0x9E6C50`; direct fields `+0x94/+0xB0` at `0x1338B00`.
  The wrappers at raw `0x2FE4D0`/`0x2FE4F0` pass `this` and `this+4`,
  respectively, as `arg1`; wrapper fingerprints begin `55 8B EC 56 8B 75 08
  56 51 51 E8` and `55 8B EC 56 8B 75 08 8D 41 04 56 50 51 E8`. This sample
  classifies only observed callers, not all 283 xrefs. Full evidence and
  uncertainty boundaries are in `src\Serialization\README.md`.
- **`tfLogin` mode behavior has a bounded native snapshot.** The `0xA58`-byte
  mode loads `LoginUI.xml`, registers 15 observed input actions, selects a
  background zone, and handles login, character-refresh, and zone-list
  responses. Auto-login settings and request branches are cataloged in
  `src\Startup\LoginModeNativeLayout.cs`; behavior and uncertainty boundaries
  are in `docs\LoginMode.md`. The observed Demo/DefaultSettings path can remove
  saved UI settings and keybinds; this is documented only and is not performed
  by the reconstruction.
- **Login response callbacks are matched to their handler bodies.** The
  constructor creates three `0x20`-byte wrapper objects with the owner at
  wrapper `+4` and target function pointer at `+0x10`. Their targets map to
  the Login, character-refresh, and zone-list response handlers at raw
  `0x63B7C0`, `0x63C5F0`, and `0x63D270`, respectively (constructor target
  VAs `0xA3C3C0`, `0xA3D1F0`, `0xA3DE70`). Raw
  `Login_QueueLoginRequest` (`0x63B550`) copies the first wrapper/member pair
  from `+0x9D8/+0x9DC` to request `+0x50/+0x54`. This establishes the Login
  request's callback target. Its vtable slot `+4` reaches raw `0x643380`,
  which tail-calls the stored target with the Login owner as `this`; the
  request manager's worker delivery path and the other two attachment sites
  are now resolved as described below.   OS completion signaling is now traced below.
- **All three Login callback request attachments are identified.** Character
  refresh builder raw `0x63ECE0` copies Login `+0x9E8/+0x9EC` into request
  `+0x50/+0x54`, sets native type 1 at `+8`, formats
  `%s/api/v2/characters/list`, and gates successful submissions with byte
  `+0x9F0`. The state-2 branch of auto-login dispatcher `0x63E390` constructs
  the zone-list request with callback pair `+0xA00/+0xA04`, native type 2,
  and `%s/api/v1/server/list`; it is conditional, not unconditional. All
  three request paths share constructor `0x3843F0`, preparation `0x636550`,
  and submission wrapper `0x6499F0`. Worker inspection now proves type 1
  selects GET and type 2 selects POST.
  The state jump table at VA `0xA3F574` maps to raw `.text` `0x63E974`;
  uncorrected raw-loaded IDA decompilation misses its branch bodies.
  Catalog and checks are in `src\Startup\`; details in `docs\LoginMode.md`.
- **Request-manager callback delivery is now traced.** Manager pump
  `0xBC4550` visits route pump `0xBC3F80`, then worker `0xBC2B30`.
  A nonnull worker `+0x1C` completion whose first DWORD is zero reaches
  result handling. Completion `+0x18` points to the response, whose `+4`
  points to the request. Raw `0xBC37E2` acquires a strong callback from
  request weak pair `+0x50/+0x54` using `0xBC4A80` and atomic helper
  `0x327730`; raw `0xBC37FC` invokes acquired callback slot `+4` with the
  response pointer. This connects to the Login wrappers' `0x643380`
  trampoline. Expired callbacks are skipped, not resurrected.
  Worker `0xBC2B30` calls imported `WinHttpOpenRequest`; its corrected
  `.text` table at raw `0xBC38C0` maps types 2/3/4/5 to
  POST/PUT/DELETE/HEAD, with GET as the default. This mapping and response/
  completion layout snapshots are in `ClientApiRequestNativeLayout.cs`.
  OS completion signaling is now established below; detailed retry/error
  handling and thread affinity remain incomplete.
- **WinHTTP completion signaling is now established.** The registered
  five-argument status callback is raw `0xBC26E0` / VA `0xFC32E0`;
  raw `0xBC1DA0` is its internal dispatcher, not the OS entry.
  Worker `0xBC31E9` registers it via `WinHttpSetStatusCallback` and
  `0xBC31F8` passes the `0x1C`-byte completion record as SendRequest context.
  Completion `+0` is the actual live request handle. HANDLE_CLOSING status
  `0x800` clears it at `0xBC2728`, making the worker's zero-handle gate
  eligible. Terminal internal dispatcher results close the handle; empty
  READ_COMPLETE sets result kind 1 if unset. A distinct synchronous
  failure path also clears the handle. Context byte `+0x11` selects
  callback-owned immediate cleanup versus retaining it for the worker;
  its teardown ownership transfer is now established below. Layout and result-preservation
  behavior are saved in `ClientApiWinHttpCompletionNativeLayout`.
- **Detached WinHTTP cleanup ownership is bounded.** Teardown `0xBC2890`
  sets active completion byte `+0x11` at `0xBC28CB`; route destructor
  `0xBC3B80` calls it before freeing workers, so HANDLE_CLOSING owns the
  detached response/context cleanup. Idle pruning bypasses workers with
  active `+0x1C` completions. Teardown closes a separate worker handle at
  `+0x20`, not visibly completion request handle `+0`; full request-close/
  cancellation guarantees remain unknown and are not claimed.
- **Zone-list fields and bounded selection are reconstructed.** Decoder
  `0x635BE0` builds 24-byte records from `zone_id`, `zone_name`,
  `protocol_version`, `build`, `matrix_url`, `players`. Handler `0x63D270`
  gates signed auto-login state >=3 and swap-removes records whose protocol
  differs from `0x4C5F` or whose players WORD is zero. It selects with
  `rand()%count` among all candidates for filter `random` or among native
  zone-name matches otherwise, then overwrites Login `+0xA20` with
  `matrix_url` (not zone name/ID). Failure logs, clears that text, and
  increments state as success does. `LoginZoneListSelection.cs` models
  the pure bounded behavior with injected zone matching/random sample;
  SDB/localization and CRT locale are not guessed.
- **Auto-login state 5 now connects selection to connection preparation.**
  Corrected jump-table case 5 reaches `0x63E890`, searches character records
  by the selected pair, and calls `0x63EE30` on record `+0xC8` properties.
  Keys `name/is_dev/race/gender` are verified; it resets/allocates a
  `0xBB0` connection object at Login `+0x1C`. Then `0x63F230` uses matrix
  destination/override text, validates host/port, gates unsigned connection
  state <2, and calls `0x494CA0`. That reachable branch sets connection
  state2 and calls `0x494E30`; the helper's message24 branch is only for
  states6/7 and is not reached from this gate. No successful World-mode
  transition is inferred.
- **Matrix connection's oracle-ticket entry is traced.** `0x494E30`
  builds a type-2 POST `%s/api/v1/oracle/ticket`, attaches callback
  pair connection `+0xB98/+0xB9C`, and uses the shared request manager.
  This wrapper is 16 bytes (owner+4,target+8,adjustment+C), unlike Login's
  32-byte wrapper; its target is raw `0x495460`. Oracle parsing
  `0x4956C0` checks response kind1 and status/100==2, extracts literal-backed
  `matrix_url/ticket/datacenter/operator_override/session_id/hostname`,
  and reaches `0x495F20`, which allocates/opens a `0xDF8` transport via
  `0xBD00B0/0xBD0960`, then sets connection state3. Failure while state2
  updates delay `min(300,current+2+(current+1)/2)` and schedules another
  ticket request. C# bounded delay model and native snapshots are in
  `MatrixConnectionNativeLayout.cs`; evidence and unknowns in
  `docs\MatrixConnection.md`. No handshake/World transition is inferred.
- **Matrix transport and initial packet are preserved.** `0xBD0960`
  creates an overlapped UDP socket, binds up to five preferred ports plus
  two ephemeral candidates, registers with a pump, and posts a 1400-byte
  `WSARecvFrom`. `MatrixTransportNativeLayout.cs` models the pure bind
  candidate calculation including 16-bit wrap. The open-success application
  packet is message `0x11`, with fixed size30 plus injected text/blob bytes,
  encoded by `0x349E60`; **`0x338120` is its destructor**. Unknown field
  meanings remain offset-named in `MatrixInitialPacket.cs`. `0xBD3B20`
  submits that buffer on channel1; raw switch translation establishes
  shared channel1/2 fragment queues and unreliable channel3 promotion to2.
  `MatrixTransportFragmentPlan.cs` models only queued header/slice plans,
  not final UDP framing. Continue with queue draining/sequence assignment
  or IOCP receive dispatch; no complete handshake or World-mode edge proven.
- **Shared channel1/2 queue promotion/retry is partly decoded.** Constructor
  `0xBE3CE0` creates `0x5A8` queues; `0xBE49E0` promotes up to512 outstanding
  fragments, assigns wrapping WORD sequences, and considers a 256-sequence
  window. `0xBE4DE0` prepares retries with a distinct 250..2500 millisecond
  policy and header attempt markers. Observed failure thresholds are16
  attempts or15000ms full-window timeout, conditional on socket `+0x100`.
  `GameSocketReliableQueueNativeLayout.cs` saves pure interval/window helpers.
  Continue from acknowledgment handler `0xBE43B0` or actual queue-to-datagram
  emission; selected-entry flags and final framing remain incomplete.
- **Fragment-to-datagram boundary is now bounded.** `0xBD4340` assembles
  data only in socket states5/8, beginning with an opaque network-order
  DWORD prefix and offering ACK/control blocks before data. `0xBD4240`
  serializes fragment header/sequence big-endian; `0xBD58E0` XORs payload
  with FF/AA/CC for markers1/2/3. `MatrixDatagramFragmentEncoder.cs`
  preserves this portion without claiming a complete datagram. `0xBD4780`
  appends optional probe-like filler, not a general compression/encryption
  pass. `0xBD2980` reaches Winsock sendto/send and explicitly reports errors.
  Still unknown: handshake states, opaque prefix semantics, full ACK/control
  layout, connected-send flag setter. Read-only child “Matrix receive
  dispatch” is tracing the separate IOCP receive boundary.
- **IOCP receive boundary is integrated.** RTTI proves `nl::Network`,
  its embedded `nl::Network::PumpThread`, and `nl::GameSocket`. Network
  associates each socket with IOCP using the socket as completion key;
  PumpThread `0xBBC860` routes successful nonnull operations to socket
  slot+18 (`0xBD2E30`), which calls `WSAGetOverlappedResult`, decodes
  sender sockaddr, enters packet parser `0xBD3030`, and normally reposts
  `WSARecvFrom`. `NetworkIoCompletionNativeLayout.cs` preserves layout
  and dispatch evidence. Error/suppression paths do not all repost.
  The read-only child is finished; continue at packet entry or handshake
  response routing, not by re-requesting its completed IOCP trace.
- **Network shutdown ownership is partly bounded.** Network vtable slot+8
  (`0xBBD940`) calls WSOCK32 ordinals22/3 (shutdown mode2, close) on a
  socket handle supplied by pointer, writes `-1`, and posts a completion to
  wake the pump. GameSocket deleting destruction closes its handle at+`0x118`
  through `0xBE36B0`; slot+`0x1C` (`0xBD2D10`) is only the queue/close
  notification. Helpers `0xBC3970`/`0xBC3990` call `WinHttpCloseHandle`
  and clear `this+0x20`/`this`; no direct `E8`/`E9` caller or `.rdata`
  VA/vtable pointer was found for those helpers. The eventual close of Network IOCP handle+`0xDC`
  and standalone-drain owner remain unresolved. See
  `docs\MatrixConnection.md` for exact references and caveats.
- **Outer admission and zero-context handshake are saved.**
  `GameSocketPacketAdmission.cs` models exactly5..1400 bytes, BE context
  comparison, and sender-address/port gates. `GameSocketHandshake.cs`
  encodes POKE/KISS and decodes ABRT/HEHE/HUGG with exact stage/port
  behavior and abort map. Native replies write candidate+224, word+228,
  remoteport+248, and expectedcontext+114, but do not themselves advance
  socket state. `0xBD3140` now records the chunk header, sequence, shared
  body slice and time sample; lengths2/3 pass the header gate despite an
  incomplete sequence prefix. This is a declared-chunk overread observation,
  not proof of an allocation-boundary fault.
- **Control routing and the transport Connected gate are established.**
  Logged native state IDs0..9 are cataloged in
  `GameSocketStateAndControlCatalog.cs`. Translated `0xBD3510`/`0xBD3760`
  switches separate immediate keepalive/clock/close controls from deferred
  ACKs for queues1/2. A deferred clock reply while socket state3 and
  clock-object sample count>=2 calls `0xBD1B70` to set5/Connected.
  `0xBD3E50` now has channel0..3 dispatch and token1..3 provenance recorded
  in `MatrixConnection.md`: close completion queues1, completion errors and
  failed binding queue2, and successful binding queues3. The exact chunk
  record offsets and short sequence-prefix boundary case are also recorded.
  `GameSocketChunkHeader.cs` remains a header-only model; full reassembly,
  reliable-window policy, and callback lifetime are not reconstructed.
- **Handshake tick progression is now linked.** `0xBD4BC0` consumes
  receive queue+260 then translated state table+rawBD505C. Nonzero HEHE
  candidate causes Initializing1→Transferring2 and immediate KISS; HUGG
  context causes2→Connecting3 with clock object, or2→Connected5 without
  one. Queue activator `0xBE3F60` proves HUGG word+228 is a sequence seed
  and initializes reliable retry to1000ms. Transfer timeout restarts1
  using separate byte+11D, not a direct KISS resend.
  `GameSocketTickNativeLayout.cs` models only ready-context guards.
  Next: Connected tick calls `0xBD4A20/0xBD5080` and virtual+14
  `0xBD5170` likely delivers pending owner events; verify rather than infer.
- **Queued delivery is now verified, not inferred.** `0xBD5170` invokes
  owner slot4 for channel1, slot8(flag1) for channel2, slot8(flag0) for
  channel3, and slotC when state6 transitions to7. No Connected callback
  was established. Connecting3 releases early data-channel fragments;
  successful normal data handlers assume ownership rather than the caller
  always freeing them. Native primary MatrixConnection vtable1C642D0 maps
  these slots to4904A0/4904C0/490540. Next concrete trace: channel1 wrapper
  4904A0→33CE40; other channels forward via owner+910.
- **Channel1's object is a descriptor, not raw packet bytes.** Wrapper4904A0
  adds4 to ECX: dispatcher33CE40 uses the secondary interface at connection+4.
  Descriptor DWORD+10 IDs34..68 map to35 slots+4..8C; all mappings are saved
  under `src\Startup\MatrixServerMessages\`. Native names are numeric msg-N.
  Msg35→490AD0 asserts receiver+8 (connection+C) state3, copies descriptor
  +28/+2C to connection+860/+864 and a reference field+34 to+86C, samples
  clock into+818, changes connection state4, combines config masks at+83D.
  Scalar model and guards are saved; field semantics and wire decoding remain
  unresolved. Next: msg37 handler490C10 and the descriptor creation path in
  reliable receive, accounting for the secondary receiver adjustment.
- **EnterZone is now anchored to msg37.** Original log argument pushes prove
  descriptor instance+28, zone ID+30, match+58, zone-name text+68. Handler
  490C10 asserts connection4 then writes6; copy491070 maps descriptor+28 to
  owner+878, plus64-byte block+80→owner+8D0. Preview/Disabled clears low3;
  ForceZone/Asset/DataPreview set1/4/2. Spectator byte+C0 sets owner+83D bit2.
  Scalar C# model saved. Native state4 log is "Connected to matrix".
  Next: queued descriptor creation/reassembly and downstream EnterZone
  initialization; do not identify connection6 with socket6 or World mode.
- **Reliable receive storage/decode boundary is linked.** BE51F0 classifies
  WORD delta from queue+2A:0..255 window,256..32767 future drop,>=32768 old
  drop. Ring+78 indexed by(head+478+delta)%256; new fragments reverse XOR
  once before storage. BE56D0→BE5440 consumes contiguous slots, appends
  continuation into+594, final-decodes via plain pointer+14, stamps decoded
  object+20 channel/+24 size, queues+60, then advances/recycles fragment
  and sets ACK-pending+32. The `socket+120` limit is checked on continuation
  only; the final append has no visible same check. Codes3/4 propagate
  through BD5080 to pump-close. The ring/window model remains non-mutating.
  Constructor BE3CE0 zeros the queue tag/decoder fields; MatrixConnection
  passes its codec pair at+824 to BD0960, which installs pairs in queue+10/+14
  for channel1 and channels2/3 (3 shares2). Decoder ABI and selective ACK
  details remain partial.
- **Decoder addresses are now proven from constructor stores.** Native48F12C
  installs owner+828 VA73EEE0(raw33E2E0), tag+824=B968;48F14E installs
  owner+830 VAC227A0(raw821BA0), tag+82C=4C5F. Socket open copies pair1 to
  channel1, pair2 to2/3 and tag WORDs to socket+30/+32 (POKE/KISS). Channel1
  decoder calls34A020 then33D4B0. Its decompiler reports bad SP: verify
  arguments/instruction framing before reconstructing wire fields.
- **Shared channel2/3 dispatch is now mapped.** `0x821BA0` reads a raw QWORD
  and compact positive16 code; codes1..6/9 reach factory `0xBC9B80`, code8
  reaches `0xBC9DB0`, and code7/out-of-range reaches shared secondary-dispatch
  block `0x821D7C` then `0x81CC50`. The three route targets and byte-map/table
  addresses are in MatrixConnection.md. The inner `0x81CC50` tables are
  structurally mapped with a leading-byte decrement (accepted inputs1..0x36
  map to indices0..0x35);
  selector0x53 builds `MarketRequestComplete`, and selector0x54 builds
  `ReceiveWeaponTweaks` with two raw bytes followed by a remaining-stream
  blob. Selector0x55's `TookDebugWeaponHitPublic` reader has a verified
  39-byte scalar prefix and two conditional tails, including a count-blocked
  vector of 64-byte records. The copied third DWORD at object+0x70 has no
  observed input write in the reader path; its origin remains unresolved.
  Selector0x56 constructs the distinct RTTI class `TookDebugWeaponHit` and
  shares the same `0x5F60F0` body reader; this does not prove semantic
  equivalence.
  Selector0x57 constructs `DebugWeaponStats` and reads eight raw WORDs at
  object offsets+0x28 through+0x36; their meanings are unknown.
  Selector0x58 constructs `RewardInfo` and reads two bytes, one QWORD, and
  three DWORDs at object offsets+0x28..+0x40; meanings remain unknown.
  Selector0x59 constructs `ProgressionXpRefresh` and reads count-blocked
  records of five raw DWORDs (20 bytes) into the vector at object+0x28; field
  meanings remain unknown.
  Selector0x5A constructs `ReceivedDeferredXP` and reads count-blocked
  records of four raw DWORDs (16 bytes) into its vector; meanings remain
  unknown.
  Selector0x5B constructs `PublicCombatLog` and accepts raw DWORD gate1
  followed by a WORD-length-prefixed blob; the non-1 diagnostic path remains
  unresolved.
  Selector0x5C constructs distinct RTTI `PrivateCombatLog` and branches to
  the same reader with `this = object+0x28`; parser reuse does not prove
  semantic equivalence with selector0x5B.
  Selector0x5D constructs `AnimationUpdated` and reads a WORD plus a byte at
  object+0x28/+0x2A; meanings remain unknown.
  Selector0x5E constructs `RaiaNPCDebugging` and reads a NUL-terminated
  shared-text field at object+0x28 in 256-byte chunks; encoding and text
  meaning remain unknown.
  Selector0x5F constructs `WeaponProjectileFired`, reads four WORDs at
  object+0x28..+0x2E, then consumes a gate byte and an optional three-WORD
  tail at object+0x30..+0x36; field meanings remain unknown.
  Selector0x60 constructs `AbilityProjectileFired`, reads its fixed scalar
  fields through object+0x50, then consumes a gate byte and optional QWORD
  at +0x60 with presence at +0x58; meanings remain unknown.
  Selector0x61 constructs `ProjectileHitReported` and reads WORDs at
  object+0x28/+0x2A then bytes at +0x2C/+0x2D; meanings remain unknown.
  Selector0x62 constructs `Stumble` and reads WORDs at object+0x28/+0x2A
  and a byte at +0x2C; meanings remain unknown.
  Selector0x63 constructs `QuickChat` and reads a DWORD at object+0x28
  through shared continuation 0x8206E3; allocation-failure behavior remains
  unresolved.
  Selector0x64 constructs `ProximityTextChat`, reads NUL-terminated shared
  text at +0x28 and bytes at +0x2C..+0x2E, then consumes an optional
  WORD-length-prefixed blob behind a gate byte at +0x30; meanings unknown.
  Selector0x65 constructs `JumpActioned` and reads a WORD at object+0x28
  through shared continuation 0x81D21F; allocation-failure behavior remains
  unresolved.
  Selector0x66 constructs `JumpRolled` and shares that WORD continuation;
  its allocation-failure path also zeroes ESI before reading.
  Selector0x67 constructs `Respawned` and reads WORD +0x28 plus bytes +0x2A/+0x2B; allocation failure zeroes ESI before reading.
  Selector0x68 constructs `CalledForHelp` and shares the same WORD continuation; allocation-failure behavior remains unresolved.
  Selector0x69 constructs `TookHit` with a gated record at +0x28, bytes +0x50/+0x51, WORD +0x58, and byte +0x5A.
  Selector0x6A constructs `AlmostHit` and reads one QWORD at +0x28; allocation-failure behavior remains unresolved.
  Selector0x6B constructs `DealtHit` with the gated record at +0x28 and bytes +0x50/+0x51; allocation-failure behavior remains unresolved.
  Selector0x6C constructs `Killed` and reads WORD +0x28, QWORD +0x30, and bytes +0x38..+0x3A; allocation-failure behavior remains unresolved.
  Selector0x6D constructs `WarnLockTargeted`; the case has no body-reader call and allocation failure takes the zero-return switch epilogue.
  Selector0x6E constructs `CurrentPoseUpdate` and reads variable scalar groups controlled by masks in byte +0x28; field meanings remain unresolved.
  Selector0x6F constructs `ConfirmedPoseUpdate` and reads a conditional nested record at +0x28 plus WORD +0xB8; nested semantics remain unresolved.
  Selector0x70 parses seven 0xC8-stride records; receiver slot +0x144 reads DWORDs +0x28/+0x2C and probes +0x30 as a vtable, dispatching numeric code363.
  Selector0x71 parses a 14-case nested state; receiver slot +0x148 passes its zero-initialized +0x28 DWORD to a vtable helper and dispatches numeric code365.
  Selector0x72 reads DWORD +0x28/WORD +0x2C; receiver slot +0x14C treats +0x28 as a list pointer and updates owner state at +0x43C8; no event dispatch observed.
  Selector0x73 parses the same 14-case nested state; receiver slot +0x150 treats +0x2C as a list pointer, uses zero +0x28 as state, and dispatches numeric code213. Mismatches and unknown consequences are documented.
  Selector0x74 constructs `AbilityActivated`, reads DWORDs +0x28/+0x2C, and
  copies remaining input into an `slBlob` at +0x30 through virtual +0x10.
  Selector0x75 has the same parser shape; its receiver passes addresses
  +0x28/+0x30 to a helper that treats the latter field as a tree-header
  pointer, conflicting with its initialized slBlob vtable. No intervening
  conversion is known; runtime consequences and field meanings remain unknown.
  Selector0x76 copies remaining input into an `slBlob` at +0x28, but its
  receiver interprets +0x28/+0x2C as begin/end pointers to 0x40-byte records.
  This parser/consumer representation mismatch is confirmed statically; any
  intervening mutation and runtime consequence remain unknown. The records'
  semantics and cooldown timing are also unresolved.
  In the parser-message path, selector `0x77` constructs `NPCInteraction` and
  reads one raw QWORD at +0x28;
  its receiver uses it as a two-DWORD map key and dispatches numeric UI code
  356, with no event name assigned. The parser requests 0x30 bytes and writes
  only through exclusive +0x30, while the unchanged object pointer reaches a
  consumer that swaps 0x20 bytes at [+0x30,+0x50). Its vtable reader/writer
  also touch only +0x28; no alternative direct constructor/callsite or body
  transform explains the consumer state. This parser-message consumer is not
  evidence that the keyed selector-0x02/0x03 Character dispatchers reuse that
  method. An untraced indirect entry is not excluded. Actual block extent and
  runtime consequences remain unknown.
  Route0 Character selector0x78 constructs `OpenMovieDialog` and passes input
  through the shared NUL-terminated text reader at +0x28; field meaning and
  allocation-failure consequence remain unknown.
  Route0 Character selector0x79 constructs `PrivateDialog` and reads DWORD
  +0x28, QWORD +0x30, and DWORD +0x38; this ends at +0x3C despite a +0x40
  request, while the same parsed pointer reaches event-229 handling that reads
  through +0x48. The actual allocator block extent remains unresolved.
  Route0 Character selector0x7A constructs `PublicDialog` and reads DWORDs
  +0x28/+0x2C; field meanings and allocation-failure consequence remain
  unknown.
  The Route0 Character nested selector table 0x53..0x7A is fully traced, and
  all 17 case-bearing primary-route subcases have native layouts. Route3
  entries2,3,4,5,7,8 are
  mapped in `docs/MatrixConnection.md`; next trace parsed-object consumers.
  Field semantics remain open.
  RTTI names the results `nl::RoutedMessage` and
  `nl::RoutedMultipleMessage`; QWORD and message semantics remain unknown.
- **Header/factory roles corrected by instructions.** 34A020 reads compact
  positive U16 (nonzero byte, or zero+LE WORD strictly>255); malformed
  extended0..255 throws. Matching writer320900 is reconstructed and all
  65535 valid values round-trip checked. Factory is33D4B0, not34A020;
  table raw33E250 covers34..68. Next focused bodies: msg35 factory target
  33D557→constructor339AB0/read3570B0; msg37 corresponding factory case.
  Keep bad-SP decoder pseudocode separate from verified instructions.
- **Msg35 normal-buffer wire body is saved.** Factory33D557 allocates50h
  via339AB0 then body3570B0 reads8 raw bytes(+28) via356F80 and two blobs
  (+30/+40) via12EE170, each raw LE ushort length+bytes. Packet minimum13;
  blob cap65535. C# decoder includes prefix truncation, exact cap, malformed
  compact headers, managed-copy and trailing-byte checks. The word pair
  at owner+860/+864 is one opaque u64; semantic names still unresolved.
  Next: EnterZone/msg37 factory/body and shared-text reader12EE010,
  without assuming its bytes are UTF-8 or synthesizing object references.
- **EnterZone absent-metadata wire variant saved.** Factory33D629 allocatesC8
  via339EA0, body356E60→zone-fields356770. Zone fields use raw LE scalars
  and two NUL byte strings; optional metadata3565C0 starts with presence
  byte and, when present, parses two trees355140/355E50. C# explicitly
  rejects that present variant. Absent minimum103 bytes. Suffix is56 raw
  bytes, bool-normalized byte, raw spectator byte: seven native padding
  bytes are not serialized. Shared span reader reused by msg35.
  Next concrete boundary: those two optional-metadata tree readers.
- **EnterZone present metadata is now decoded.** Readers355140/355E50
  parse separate NUL-byte-key maps with raw LE16/32-bit values, chunked
  u8 counts (continue on255, including terminal0 for exact multiples).
  Lookup3550A0/355DB0 proves unsigned byte ordering and last-value-wins.
  Copy3563E0 uses input+0/+8: both adjacent temporary maps survive, not
  just the first decompiler local. Saved C# preserves opaque value bits,
  owned key bytes, absent versus present-empty, and the unchanged suffix.
  Full `Decode` supports both variants; restricted entry point remains.
  Startup binary-evidence checker renamed to match its broader scope.
  Next: downstream EnterZone state6 initialization and outgoing message24;
  do not infer a World app-mode request from connection state6.
- **EnterZone downstream clock/response is now linked.** Five suffix QWORDs
  +90..B0 and normalized+B8 initialize clock owner+918 via8FA620; +A0 is
  double rate, +98 reference tick, +B0 offset, +A8 upper limit, +B8 limit flag.
  Sampler493650→advance8FA700 follows. Limit flag owner+948 selects runtime110
  `"paused":1`, independently of spectator. Global subsystem386900 receives
  owner byte+8A0; downstream owner+910 requires DWORD+28 zero then sets byte+18.
  Owner+B94 selects msg24: gate498350 requires socket and signed state>4,
  stamps timeGetTime*1000, copies owner+878 context, sender499840→49A520 emits
  raw u64 context then compact24 (9 bytes), channel2. Saved separate client
  message and downstream-plan C# models. Getter6F4540 selects existing mode
  object's pointer (Login+14, World+28E8, LookingGlass+68), not a new mode.
  Clock initialization/unscaled projection saved; scaled numeric conversion
  and native floating-point exception flags remain the next bounded trace.
- **Finite clock scaling is now saved.** Original8FA780 subtracts reference
  with unsigned64 wrap, helper159E17E converts to double, MULSD applies rate,
  helper159E060 truncates into unsigned64 bits, then reference/offset wrap.
  C# supports finite in-range values including negative scaled elapsed bits;
  near-unit tolerance skips conversion, limits apply before monotonic cursor.
  Nonfinite/out-of-range conversion remains explicitly unsupported because
  native integer-indefinite return and floating exception flags are not simulated.
- **Socket synchronized-clock value projection saved.** BE8410 returns full
  EDX:EAX, not DWORD. Source is timeGetTime*1000 minus clock+18 reference,
  unsigned64→double→single, MULSS rate+14, helper159DE7F→u64 bits, add base+20,
  then sign-of-wrapped-difference monotonic clamp against cached+60.
  Do not replace its single-precision stage with EnterZone's double arithmetic.
  Shared bounded clock arithmetic, original-instruction fingerprints and synthetic
  rounding/wrap checks are saved. Native critical section+28/calibration remains
  outside the value model; numeric exception flags remain an explicit boundary.
- **Adjacent messages38/39 are scalar updates, not proven zone exit.**
  38→491380 copies descriptor+28's16 bytes to owner+8D0 (first16 bytes
  of EnterZone block), no state/clock change. Factory33D68F→33A340 alloc38h;
  reader3545D0 reads raw LE8+4+4, ctor words0/0/3F800000/0. 39→4913A0 calls
  global+C64 subsystem386900 with raw desc+28 byte then stores owner+8A0.
  Factory33D6F2→33A420 alloc30h;337180 reads raw byte. Saved packet decoders
  and opaque bit-preserving models, sizes17/2 bytes. Formal value semantics
  unresolved; next likely lifecycle target is msg62 handler491140.
- **Lifecycle target correction and actual ExitZone saved.** Msg62/491140 is
  queue status: raw DWORDs+28/+2C become runtime295 queue_position/queue_length,
  variant tag1, receiver+AC, no state write. Factory33DF77→33C310 alloc30h;
  reader34B610 raw4+4, packet9. Actual ExitZone msg40/491230 is header-only,
  factory33D757→33A4E0 alloc28h, packet1. Cleanup494B00 requires downstream
  +910, releases nested+B4, asserts state6/7→5 and calls current-mode64E210
  (resource resets/runtime97). Post-cleanup+B94 selects release socket/state1;
  zero selects ExitZoneAck/state4, send allowed iff socket && signed state>3.
  Ack native log identifies code19, encoder499970 emits only that compact code,
  channel1 via498D70. Saved bounded branch/cleanup guards; full lifetime/resource
  reset behavior and external-call reentrancy remain unresolved.
- **Character-refresh outgoing receiver is now explicit.** Raw `0x63CEF2`
  loads global VA `0x1E002B8` (raw `.data` `0x19FE6B8`). The indirect call
  at `0x63CF05` uses vtable `+0xAC` with message `0x89`, local buffer, and
  zero. The translated file value is zero; reading raw EA `0x1E002B8`
  instead reads unrelated resource bytes. The runtime class is now identified
  as `uiManager`; the message-89 subscriber and connection to a successful
  Login-to-World transition remain unresolved.
- **Login request callback attachment is now directly traced.** Raw
  `Login_QueueLoginRequest` (`0x63B550`) allocates a `0x78`-byte request
  object, writes type 2 at object `+8`, and copies the Login callback pair at
  `+0x9D8/+0x9DC` to request `+0x50/+0x54` before submission through the
  global request manager. The Login object gates submission with `+0x9E0`
  and records the caller's option byte at `+0x9E2` on success. The copied
  pair is the constructor wrapper targeting the Login response handler.
  Its callback vtable trampoline and worker delivery gate are now known.
  Shared submitter `0xBC43C0` validates the request, performs a per-route
  lookup/create, and links the request into a `0x38`-byte tracker containing
  the manager pointer, request shared text at `+0x10`, and a DWORD at `+0x14`.
  That submission path does not consume the callback pair; the worker pump
  consumes it after completion as documented above.
  The login-response routine's retry branch is also now bounded: option byte
  `+0x9E2` must be nonzero and response text nonempty; it decrements the
  counter at `+0x9E4` (constructor value 3), waits 500 ms while the remaining
  count is positive, then calls `Login_StartAutoLogin`. The interactive
  request passes option 0 and the auto-login request passes option 1. Checks
  and constants for these observations are saved with the Login layout.
  C# constants and checks are in `src\Startup\LoginModeNativeLayout.cs` and
  `src\Startup\Checks\Program.cs`; see `docs\LoginMode.md`.
- **Login-to-mode transition boundary was checked directly.** Full decompilation
  of `Client_ApplyPendingAppModes` (`0x30D850`) confirms request IDs 1/2/3 map
  to Login/World/LookingGlass, respectively; an earlier delegated report had
  reversed IDs 2 and 3. Queue entries are `{modeId, payload}` at application
  `+0xB84` (head/index `+0xB90`, count `+0xB94`). The character-refresh
  callback copies the first two DWORDs of a matched `0xD0`-byte record to
  `tfLogin+0xA40/+0xA44`, then crosses an indirect vtable `+0xAC` call with
  message `0x89`. Separately, raw `0x6385C0` is present in the `tfLogin`
  vtable at raw `0x18662C0 + 0x0C`; its conditional branch at `0x638B89`
  passes request pair `{2,0}` to `AppMode_EnqueuePendingModeRequest`
  (`0x30DEB0`). A separate Login-mode routine at raw `0x63C1E2` posts ID
  `0x145` through the shared `uiManager` endpoint; it is not the
  `Login_HandleLoginResponse` callback at `0x63B7C0`. These establish a World
  queue path and a Login-mode event dispatch, but not that this event causes
  that update path or that successful login reaches its queue condition. The
  selected pair is not labeled a GUID and the complete successful transition
  remains unresolved. Details and layout constants are in `docs\LoginMode.md`
  and `src\Startup\`.
- **tfLogin `0x638B89` guard (static).** Observed: the default switch case
  (`0x638AB5`, `mov [esi+1Ch],0`) loads the pointer cell at VA `0x1E5CAA4`
  and tests byte `[pointee+0x40]` (`0x638ABC..0x638AC6`); zero jumps to
  `0x638B89` (`{2,0}` to
  `0x30DEB0` on global `0x1DFFD98`), nonzero checks cvar-like objects
  `0x1E50C8C`/`0x1E6075C` (+0x40) and queues mode 3 via `0x30D590`. The only
  code reference to `0x1E5CAA4` is that read; its writer is not found
  statically, and the 0x145 login-response link is unproven. `0x30DEB0`: if
  `[app+0xC]!=0` and mode==1 it dedupes against current-mode `[+0xA28]`,
  otherwise pushes `{mode,payload}` into the `+0xB84` queue (`0x315060`);
  `0x30D850` drains it, mode 2 constructs `AppMode_World_Ctor`.
  Interpretation: this is a world-load request; trigger unresolved. Bytes are
  checked in `StartupBinaryEvidence`.
- **tfLogin `0x6385C0` switch (static).** Observed: per-frame update; selector
  `[[esi+0x1C]+0xC]` (`0x6388D8`), 5-entry table raw `0x638D1C` (VA `0xA3991C`):
  case 1 -> `0x6389F2` (sends message `0x146`, string from `[node+0x10]`),
  cases 0/2/3/4 -> `0x638C3A`, >4 -> `0x638AB3` (then `0x638AB5` guard).
  `0x63C1E2` (msg `0x145`) is in a separate function and was not seen
  producing a `[node+0xC]` value. `0x1E5CAA4` has one byte/reloc reference
  (`0x638ABD`); its writer is indirect and unfound. Producer of the list at
  `tfLogin+0x20` unresolved. Checked in `StartupBinaryEvidence`.
- **World scene-factory return boundary.** Raw `0x9B73F0`, called from
  `TfWorld_Initialize` (`0x64B460`), returns `EDI` through `EAX` at raw
  `0x9B82B4` (`8B C7`); that epilogue returns at `0x9B82C6` (`C3`). The
  factory allocates a `0xA0` root aggregate (constructor `0x9B82F0`, cleanup
  `0x9B8490`), loads `RenderScene`, `WorldMapScene`, and `SinvironmentScene`
  into root offsets `0/4/8`, and owns multiple child allocations. The root has
  no observed vfptr; its child at `+0x14` is `tfPhysicsDrawInterface`
  (`0x188` bytes, vtable VA `0x1C70968`, RTTI `.?AVtfPhysicsDrawInterface@@`).
  It passes a registration helper's result into root `+0x10`; teardown releases
  that handle and the scene references. `TfWorld_Initialize` stores the root at
  `tfWorld+0x14` (`0x64B508`) and publishes root `[0]` to global
  `0x1E00060+0xF8`. Another static caller at `0x9B2D1A` stores the result at
  its object base. Field meanings beyond these observed roles and registration
  target semantics remain unresolved. Details and original-file fingerprints
  are in `docs\MatrixConnection.md` and `SceneFactoryNativeLayout`.
- **LookingGlass entry points expanded.** The shared mode-3 enqueue helper
  (`0x30D590`) has six static callers: `0x306050`, `0x3D60A0`, `0x6385C0`,
  `0x65D400`, `0x6AE3E0`, and `0x6AF120`. Several request paths update the
  replay reader directly if LookingGlass is already active and enqueue mode 3
  otherwise; the specific `GoLookingGlass` UI binding remains disabled. This
  corrects any interpretation that the disabled UI handler makes mode 3
  unreachable. `AppMode_TryQueueLookingGlassRequest` (`0x306050`) packages a
  text value and two DWORDs with registry-dependent text handling. The caller
  offsets are cataloged in `AppModeCatalog`; request semantics and complete
  external entry conditions remain partly unknown.
- **LookingGlass request context is now traced through construction.** The
  mode-3 enqueue helper copies `0x34` bytes from the caller's request context
  into application `+0x9F0`; `Client_ApplyPendingAppModes` passes that address
  to the LookingGlass constructor, which copies it to mode `+0x18`. Two
  shared-text references at context `+0x14/+0x1C` are explicitly retained.
  A separate DWORD from application `+0x9E8` is passed to the constructor and
  stored at mode `+0x58`. Other fields and the extra DWORD remain opaque.
  `LookingGlassNativeLayout` records these offsets, and IDA names the copy
  helpers `SharedTextContext_AssignFrom` and
  `LookingGlass_CopyQueuedRequestContext`.
- **Default initial mode is confirmed.** In the guarded application-init path,
  helper `0x307224` checks queue count at application `+0xB94`; if empty, it
  calls `AppMode_EnqueueModeIdWithZeroPayload` (`0x324080`) with ID 1 and then
  drains the queue immediately through `Client_ApplyPendingAppModes`. This
  establishes Login as the observed default startup mode only when no pending
  request already exists; it does not resolve a later Login-to-World response
  transition.
- **Another Login request source is observed.** Helper `0x30D790` performs an
  opaque state update, then enqueues `{1,0}` if a selected observer object
  is missing, has null `+0x820`, or signed `+0x0C <= 3`; otherwise it calls a
  different routine on that object. The meaning of that object and the
  fallback's product semantics remain unknown. Documented in `docs\Startup.md`.
- **LookingGlass replay behavior is traced, format is not.** Its
  `GameInterface` subobject is at `+4`; the reader is built at object
  `+0x1334` (4916).
  Initialization errors and replay-end auto-quit behavior have concrete
  functions/strings in `docs\Startup.md`. The `GoLookingGlass` UI route is
  explicitly disabled, but this does not prove that no other route can enter
  mode 3.
- **`tfWorld` mode 2 has a bounded native snapshot.** Its size is `0x2904`,
  owner/context is at `+0x10`, scene pointer at `+0x14`, and four layer
  members and their constructor IDs are recorded in
  `TfWorldNativeLayout`. `docs\WorldMode.md` records selected virtual methods
  and the bounded 32-byte slot-11 record handling, while separating unknown
  layer/owner/record semantics. Slots 17/18 are confirmed as simple getters
  for `+0x2E8/+0x2EC`; the constructor initializes the former to zero and the
  latter to a pointer to an opaque `0x2C`-byte block.
- **Selected remote-observer dispatch gates are documented.** Messages 174,
  175, and 194 use a shared observer availability and signed-threshold gate;
  the UpdateShoppingList builder also applies that gate inline. Its source map
  has a unique key at entry `+0x14` and an embedded inner set; the builder
  copies outer entries into one ordered set and unions inner-set values into a
  second, then serializes both with 255-item chunks. The two descriptor words
  at `+0x18/+0x1C` form a 64-bit routing key whose low byte is forced to 4;
  exact entity identity and element semantics remain hypotheses. Four
  post-send calls are collection cleanup, not more dispatches. Details and
  evidence are in `docs\RemoteCharacterDispatch.md`; wire models remain
  unchanged. Alternate-candidate tracing found no additional producer for
  the persistent `+0x00` map: `0xA242A0` and `0x9F65E0` are view readers,
  `0x4A0700` is teardown, and writers at `0xACDD40`/`0x909AE0` target a
  distinct text-keyed map at state `+0x2C`.
- Replay, World, SDB callback, Login, SDB catalog-metadata, and selected
  remote-observer dispatch investigations completed read-only analysis; their
  results were integrated here to avoid concurrent file writes. The original
  executable remains untouched. IDA names/comments and the IDB were saved.
- UpdateShoppingList collection/source and alternate-channel traces completed
  read-only analysis and were integrated. The Login-to-World report was
  reconciled against the dispatcher and integrated with its uncertainty
  boundaries; the app-mode queue mapping conflict is resolved.

## Verification and evidence cautions

- Latest October 2 continuation additionally validates zone-list filtering/
  swap order, signed state progression, WinHTTP close signaling and teardown
  ownership, oracle callback layout, and capped retry-delay sequence. The
  solution builds with zero warnings/errors; binary-backed checks use the
  `.original` backup as well as the installed file. New names/comments are
  saved in the IDB. Native request/callback code remains offline-only.

- October 2 continuation: all Startup checks, including 14 optional
  binary-backed assertions for callback attachment, verb selection, completion
  dispatch, and translated global data, passed against the installed
  `FirefallClient.exe` and separately against `FirefallClient.exe.original`.
  The solution build passed with zero warnings/errors.
  Eleven verified function names and nine evidence comments were saved in the
  existing IDB. No executable patches were made. The earlier idle reports have
  been integrated; none was waiting for input or approval.
- Executable baseline caution: the installed file and `.original` backup have
  equal lengths but differ at 13 bytes: raw `0x2EAD58-0x2EAD59`,
  `0x30682D`, `0x1250EB3-0x1250EB4`, and `0x178E5A4-0x178E5AB`.
  This continuation made no executable writes and did not restore these
  differences; their origin is not established here. None overlaps the
  newly recorded request/callback instructions, which pass the binary-backed
  checks in both files. Do not infer whole-file equality from those checks.
- WinHTTP status callback registration/signaling and context byte `+0x11`
  cleanup-ownership follow-ups completed and were integrated immediately.
  Next connection-system boundary:
  `0x494E30` after state-5 character preparation. Keep OS callback completion
  distinct from app-mode transition.

- Latest Login continuation: callback wrapper targets now match all three
  response-handler bodies; its dispatch trampoline forwards the Login owner
  to the stored target. Startup checks passed, and
  `dotnet build Firefall.Reconstruction.slnx -v:q` succeeded with zero
  warnings/errors after recording the request callback-pair attachment and
  bounded retry gate/counter.

- Latest continuation verification: Character, all 15 Startup, Serialization,
  Collections, and LuaCache checks passed; `dotnet build
  Firefall.Reconstruction.slnx -v:q` succeeded with zero warnings and errors.
  Startup checks passed with pending-mode, LookingGlass-call-site,
  request-context, and initial-mode assertions; the full solution build
  succeeded with zero warnings and errors. Login, app-mode queue,
  LookingGlass-call-path, `tfWorld`, and observer-dispatch IDA names/comments
  were saved to the IDB at
  `D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe.i64`.
- A workspace scan found no standalone `.sdb`, `.db`, or `.dat` fixture under
  the reconstruction folder; current Serialization verification remains
  synthetic-only.
- Latest verification after the row-converter type guard:
  `dotnet run --project tests\Serialization\SerializationChecks.csproj -v:q`
  passed; `dotnet build Firefall.Reconstruction.slnx -v:q` passed with zero
  warnings and errors. These checks use synthetic SDB data, **not** a shipped
  SDB file.
- Current equipment-request continuation: `dotnet run --project
  src\Characters\Checks\Characters.Checks.csproj -v:q` passed, including
  message 208 empty/nested/chunk-boundary payload checks; solution build
  passed with zero warnings/errors. IDA names/comments and IDB were saved;
  generated Character bin/obj directories were removed after verification.
- Repair/salvage continuation: focused Character checks passed for message
  203 (1..100 caller constraint, little-endian u64 payload) and message
  202 (1..200 caller constraint, 16-byte records, complete body order).
  `dotnet build Firefall.Reconstruction.slnx -v:q` passed with zero warnings
  and errors; IDA annotations were saved and Character bin/obj cleaned.
- Teleport/respawn continuation: focused Character checks and full solution
  build passed after adding conservative native-layout records. `RequestTeleport`
  has a real caller and opaque u64 payload; `RequestRespawn` is kind-only
  (metadata ID 198) and is queued from a timed/flag-gated caller. Its concrete
  writer was independently verified at 0x5062D0/0x50BA00 to emit handle + ID
  (9 bytes before transport framing); `RequestRespawn.EncodeBody()` now covers
  this. IDA names/comments and IDB were saved; generated Character outputs
  are cleaned after final checks.
- Duel request continuation: `DuelRequest` (188) now has a conservative
  descriptor/object-layout model and documented real chat-related callers
  (modes 4 and 3/5). Shared text and two pair vectors are confirmed copied;
  semantics and class-specific wire order remain unknown. Focused checks
  and the full solution build passed; IDA annotations saved; generated
  Character outputs cleaned.
- Transfer-request continuation: `RequestTransfer` (199) has a Lua-reachable
  builder and verified 14-byte body writer (target + ID + u32 + byte).
  `FindNearbyResourceAreas` is instead message 183 and has a kind-only
  constructor; the two RTTI/vtable identities are now corrected in the code,
  notes, and IDB.
- Vehicle-calldown continuation: `VehicleCalldownRequest` (179) has a
  populated native-layout model with its 0x40-byte opaque copied block,
  positional values, and two trailing flag bytes. Two construction/send
  call paths are documented; no class-specific wire writer was identified,
  so no packet encoder was inferred. IDA annotations are saved.
- Deployable-calldown continuation: `DeployableCalldownRequest` (180) has a
  populated native-layout model with the same copied 0x40-byte block and
  positional tail as VehicleCalldownRequest, but a u32 rather than u16 at
  `+0x28`. Its discovered state-gated caller is documented; no wire encoder
  was inferred. IDA annotations are saved.
- Bounty/achievement-request continuation: the former provisional traces
  have been aligned to catalog RTTI identity. `ClearPreviousBounties` (162)
  has a u32 at `+0x28`; `RequestRerollBounties` (164) owns a copied u32
  vector at `+0x28`; `RequestAchievementStatus` (169) has a u32 at `+0x28`.
  Each caller/layout is documented without overclaiming field meanings.
- Movement/projectile class identities were rechecked against the correct
  vtable VA for each catalog row: `MovementInput` (115) has a complex
  descriptor with a process-local shared reference; `MovementInputFake`
  (116) has scalar fields plus an optional descriptor and shared reference;
  `FireWeaponProjectile` (121) has four u32s and an optional three-u32 block.
  Their constructors and discovered uses are documented; no wire order was
  inferred for these three.
- Weapon-command continuation: `SelectWeapon` (125) stores a u32 and two
  bytes, `ReloadWeapon` (126) stores one u32, `AcquireWeaponTarget` (128)
  stores four positional u32s and a byte, and `LoseWeaponTarget` (129)
  stores three positional u32s. Their constructors and real call paths are
  documented. Semantic field names and packet encoders remain unverified.
- Adjacent weapon commands: `ReportProjectileHit` (122) has an unaligned
  sequence of u16 and byte fields; `SelectFireMode` (123) and `UseScope`
  (124) each store a u32 and byte; `CancelReload` (127) stores a u32.
  Constructor/caller evidence is documented, without guessing wire formats.
- Firing-state commands: `FireInputIgnored` (117) stores a u32 and byte;
  `FireBurst` (118), `FireEnd` (119), and `FireCancel` (120) each store a
  u32. Their populated constructors are reached by gameplay/control paths;
  fields remain positional pending stronger semantic evidence.
- Effect/target commands: `NPCApplyEffect` (130) has three positional u32s,
  `NPCRemoveEffect` (131) one u32, `ChangeLookAtTarget` (133) an optional
  six-u32 group, and `TargetAbility` (136) one byte. Constructor layouts
  are documented; no static construction callers were resolved for these
  populated variants.
- Interaction commands: `DockToPlayer` (132) copies five u32 fields;
  `NPCInteractWithTarget` (135) writes five u32s with a gap at `+0x2C`.
  Their populated constructors currently have no resolved static call refs.
- Ability/emote commands: `DeactivateAbility` (137) writes a u32 and byte
  and is constructed from a guarded dispatch path; `PerformEmote` (142)
  writes a u32 and signed u16 and has an update-path caller. Field meanings
  and packet order remain open.
- Consumable/weapon-control commands: `ActivateConsumable` (138) carries
  two positional u32s from a guarded state path; `SetNoSpreadFlag` (139)
  carries two bytes; `ExitAttachmentRequest` (140) is kind-only. Their
  layouts and available caller/constructor evidence are in the Character
  README.
- Achievement/bounty queries: `ListPreviousBounties` (163) stores a u32
  and has a caller; `ListAchievements` (168) stores one byte; and
  `RequestAllAchievements` (170) is kind-only. Field meanings and unobserved
  wire layouts remain explicitly unknown.
- `AbortCampaignMission` (152) stores one positional u32 and has a
  production caller; `AnimationUpdate` (149) stores two u32s and a byte
  with no populated caller found; `TryResumeTutorialChain` (153) is
  kind-only with a timed caller.
- `CallForHelp` (151) stores a u32 and has a timed/state-gated caller;
  `ListAvailableBounties` (160) stores two u32s and has a caller;
  `SetDialogTag` (147) stores a process-local shared-text reference, one
  u32, and two bytes, with no populated caller identified. These are
  RTTI-matched names/IDs; meanings and wire layouts remain unknown.
- Added the nearby dialog/loadout commands `NotifyDialogScriptComplete`
  (143), `PerformQuickChatCommand` (144), `PerformDialog` (146),
  `SetEffectsFlag` (148), `SelectLoadout` (150), and `DebugMission` (154).
  The broad field layout of `PerformDialog` is positional; `DebugMission`
  contains an opaque shared-text pointer.
- Expanded bounty coverage with `AssignBounties` (155), `AbortBounty`
  (156), `ActivateBounty` (157), `ListActiveBounties` (158),
  `ListActiveBountyDetails` (159), `ClearBounties` (161), `TrackBounty`
  (165), `SetBountyVar` (166), and kind-only `RefreshBounties` (167).
  Caller evidence and unassigned field semantics are in the Character README.
- `ActivateAbility` (134) now has both constructors and a real build/send
  path traced. The populated constructor at `0x531840` copies three DWORDs
  at `+0x6C..+0x74`, but its only known send path uses the debug constructor
  and leaves those fields untouched even though writer `0x5BEEA0` emits them;
  their values may be uninitialized. The C# model preserves them as nullable.
  `NPCSetInteractionType` (141) has a conservative
  snapshot for its byte plus copied u64 entry vector; bitset semantics and
  wire order remain unknown.
- `SendMailToPlayer` (84) stores two positional u32s, three shared-text
  references, and a copied vector of 24-byte records. One populated caller
  was found; record semantics and wire serialization remain unknown.
- Latest continuation added native snapshots for `SendMailToPlayer` (84),
  `ListItemForSale` (83), `ClearSavedMatchQueue` (91), `MatchAccept` (93),
  `PlayerReady` (88), `ToggleMarketplace` (86), `FillBuyOrder` (85),
  `PerformTextChat` (145), `MatchmakerSetPenalties` (92), `JoinGroupLeader`
  (87), `FetchQueueInfo` (89), `ClientUIEvent` (196),
  `VendorPurchaseRequest` (238), `VendorTokenMachineRequest` (200),
  `FindServiceProvider` (195), `UpdateFriendStatus` (249),
  `FriendsListRequest` (248), `ClientQueryInteractionStatus` (186), and
  `ClaimBountyRewards` (250), with regression checks. It also extended
  `ActivateAbility`'s partial model with nullable fields copied by its
  uncalled populated constructor.
  `dotnet run --project
  src\Characters\Checks\Characters.Checks.csproj -v:q` passed, followed by
  `dotnet build Firefall.Reconstruction.slnx -v:q` with zero warnings or
  errors. Generated Character `bin`/`obj` directories were removed. The
  `MessageCatalog.csv` header was corrected: `vtable_entry_raw` is the COL
  slot and `vtable_raw` is the actual vtable address used in code references.
  The span also contains an unrelated `nl::MessageBaseT<firefall::Character::
  Fabrication_Finalize_Response>` RTTI record at `0x1865754`, intentionally
  excluded from the 169-row Character-command CSV.
  IDA constructor names/comments were saved to the IDB.
- `ListItemForSale` (83) now has a positional native-layout snapshot with
  two retained shared-text references and a trailing byte; no populated
  caller or field semantics were recovered.
- `ClearSavedMatchQueue` (91) has a minimum initialized prefix model:
  two positional u32s at `+0x18/+0x1C`, zeroed words at `+0x20/+0x24`,
  and a real caller that packs a discriminator-2 target. The words remain
  unnamed and no wire body is claimed.
- `MatchAccept` (93) has an inherited target pair, a u32 at `+0x28`,
  two u32s at `+0x30/+0x34`, and one byte at `+0x38`; no populated caller
  was found.
- `PlayerReady` (88) has two positional bytes and a presence-flagged vector
  of copied 8-byte entries at `+0x30`; entries are opaque u64s, with no
  populated caller or wire layout established.
- `ToggleMarketplace` (86) stores one byte at `+0x28`; its empty constructor
  sets that byte to 1, but the byte's semantics and caller remain unknown.
- `FillBuyOrder` (85) has eight positional u32s at `+0x28..+0x44` and a
  real parsed-input caller; field semantics and wire order remain unknown.
- `MatchmakerSetPenalties` (92) stores u32/byte/u32/byte fields from
  `+0x28` through `+0x34`; no populated caller or field meanings were found.
- `JoinGroupLeader` (87) has a single observed constructor with no subclass
  fields; only the inherited target is initialized.
- `FetchQueueInfo` (89) copies and retains 4-byte references in a vector at
  `+0x28`, followed by three positional u32s; no caller or field meanings
  were found.
- Movement-simulation continuation: `SetMovementSimulation` (197) stores
  one u16 at `+0x28`. Its only found tick caller requires a set state byte,
  an observer connection, and a >1000 ms interval; it then clears the state
  and refreshes the timer. Field meaning and packet layout remain unknown.
- Latest Character-command validation: added checks for the corrected
  `MovementInput`/`MovementInputFake`/`FireWeaponProjectile` layouts and
  firing/weapon command layouts 117-129, plus NPC/effect/target layouts
  84, 130-144, 146-170. The Character checks passed and
  `dotnet build Firefall.Reconstruction.slnx -v:q` passed with zero warnings
  or errors. The IDB was saved after naming/commenting the weapon-command
  constructors; generated Character `bin`/`obj` directories were cleaned.
- This continuation **corrected** a prior mistake and resolved the catalog
  question: the runtime table catalog at database-object `+4` is a static,
  compiled-in array of 575 entries (count `+4`, pointer array `+8`); no code
  mutates it after global init, so there is no runtime "registration" routine
  to find. The catalog **entry itself** (not a nested `entry+0x14` descriptor
  as previously stated) is passed directly into `Sdb_ConvertRowsByFieldMapping`
  as the destination schema; `entry+0x14` was a misreading of the *locally
  parsed file's* tree-node offset, not the catalog entry's. A worked example
  (table hash `0x8E3A6BCC`) shows the catalog entry layout (`+0x08` stride,
  `+0x14` field count, `+0x18` field array, `+0x1C`/`+0x20` callbacks) and
  decompiles both its per-row init callback (clears a field before conversion)
  and its finalize callback (builds an array of lightweight row-handle view
  objects over the real converted data). See `src\Serialization\README.md`.
- Two requested read-only sub-agent traces could not access the active IDA
  database in their isolated contexts. They returned no new findings; direct
  analysis in the parent session remains available.
- IDA opened the PE as a raw image at base zero. Its function addresses are
  **file offsets**; absolute operands and indirect jump targets are PE virtual
  addresses. Translate through `docs\BinaryLayout.md` before interpreting
  them. The decompilation around `0x1311100` and `0x1310600` misreads switch
  tables/raw bytes; use corrected raw disassembly as evidence.
- Numeric conversions model output bytes under normal masked floating-point
  exceptions. Native exception flags/control-state effects and rounding-mode
  changes are not reproduced.

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

## Awesomium switch-name conversion and ASCII getter

- Hardware capture/DirectX batch committed/pushed ascc8aa49.
- Parsed name conversion1029A9C0 feeds builder1028D7F0. Its helper10007AB0
  lowercases ASCII A-Z only over stored byte length, then recognized prefix
  length is removed before map key insertion. Lookup folds narrow key too.
- Original wide values are copied unchanged; no value lowercasing.
- Generic conversion optionally diagnoses WORD>7F (global11596AF8==1
  and helper10288E40<=3). Diagnostic destructor1029AA58 not traced; do not
  guarantee continuation or non-ASCII aliases.
- Returning conversion copies low byte per WORD using1000B350; no UTF-8.
  Separate child-type getter1028C8A0 first rejects any non-ASCII WORD via
  10299E00 and returns empty. ASCII values preserve case; empty fails the
  existing nonempty child-entry gate.
- Saved SwitchNameNativeLayout and native checks. Build succeeds;1741
  sample-backed checks and1519 pure checks pass.
- Follow-ons: diagnostic cleanup10289EC0 behavior, and DumpTruck report
  layer serialization. No target execution, sample edits or remote calls.

## Awesomium severity-three conversion diagnostic

- Switch-name batch committed/pushed asf2cdfb7.
- Diagnostic constructor stores severity3, not4. Destructor10289EC0 calls
  optional interceptor11596B1C first; nonzero AL skips built-in logging and
  severity handling. Unconsumed severity3 selects callback11596B18 or
  default display helper10288EA0. Callback implementations remain unknown.
- Default helper returns if original message empty/display-enable byte0,
  or message conversion yields empty. Otherwise builds debug_message.exe
  beside current executable and calls CreateProcessW. Success waits
  INFINITE then closes thread/process; failure shows Fatal error message
  box. No unconditional termination or exit-code observation in helper.
- Returning paths can continue to name narrowing, but callbacks, child wait
  and UI still forbid guaranteed continuation. No helpers/UI executed.
- Saved ConversionDiagnosticNativeLayout and exact guards. Build succeeds;
  1764 sample-backed checks and1536 pure checks pass.
- Follow-ons: message conversion10296210; callback installation if needed.
  Keep logger sinks and helper internals out of the saved pure model.

## Awesomium diagnostic conversion scalar boundary

- Diagnostic-routing batch committed/pushed as626ab04.
- Wrapper10296210 initializes wide output, reserves using102B7A10 and calls
  conversion10295F10, then ignores its aggregate-error byte.
- Reader102B78A0 uses shared UTF-8 multibyte decoder102B7D10 in mode-1,
  then validates scalar outsideD800-DFFF and <=10FFFF. Loop replaces
  rejected scalar withFFFD and sets sticky error0, but continues.
- Saved scalar append and UTF-16 pair arithmetic, not a byte-stream parser.
  Malformed input does not imply empty converted message. Embedded scalar0
  remains a stored WORD; later null-terminated API behavior is separate.
- Loop length comparison is signed-positive; synthetic high-bit DWORD
  lengths skip decoding only if allocation/reservation reaches that point.
- Exact malformed-byte consumption, allocator failure and helper argv
  interpretation remain unresolved. No samples/codecs/helpers executed.
- Saved DiagnosticEncodingNativeLayout and native guards. Build succeeds;
  1791 sample-backed checks and1559 pure checks pass.

## DeploymentCommon exact ranges and GPU diagnostic configuration

- Diagnostic scalar-boundary batch committed/pushed as b14b0c4.
- GPT-6 Luna High agent e77c64d8-25dd-4020-93ef-cecd1cc1ff90 resolved
  DoFileDownload MethodDef7C/body RVA38B0. Parts start at0, first end is
  min(total,10485760), not min(total-1,10485759). Later start=end+1,
  end=min(total-1,end+10485760); signed Int64 comparison and unchecked
  arithmetic preserved. DownloadObject constructor9D/body44F4 uses .part0;
  request9E/body4550 formats supplied inclusive endpoints directly.
- Corrected DownloadPlanning instead of adding a duplicate planner. Small
  positive totals request one byte past EOF; remote response is unresolved.
  Added focused evidence docs and boundary/sample checks.
- Direct Awesomium trace found threshold setter10288E70 preserves negative
  inputs and clamps signed >=3 to static fallback3. Getter10288E40 returns
  storage11596B04; another writer1028A2A0 remains untraced.
- Interceptor setter10288E50 stores/returns supplied DWORD. GPU init1019CAA0
  installs1019C9C0 only without initialization-failure byte and without
  single-process/in-process-gpu switches. Teardown1019C7F0 clears it,
  not restoring a prior pointer. Callback attempts unresolved virtual
  forwarding and normally returns AL0, so does not consume severity3.
- Build succeeds,1588 pure and1829 sample-backed checks pass. No target,
  callback, downloader, registry/hardware query or service was executed.
- Follow-ons: DownloadObject non206 status/retry decisions; other threshold
  writer1028A2A0 or explicit setter caller10033BC0. These are not completed.

## Awesomium logging setup precedence

- Range/GPU-interceptor batch committed/pushed as68603b7.
- Traced setup10033BC0: ASCII getters for type/log path/log-level,
  numeric converter output initialized0 but success byte ignored,
  disable-logging tested by presence. Normal logger-init return is followed
  unconditionally by log-level setter10288E70, regardless of init success.
- Init1028A2A0 receives destination0/1, argument3=0, append=nonempty ASCII
  type and diagnostic-check mode0. The mode goes to11596AF8, so optional
  mode1 diagnostics are disabled by this observed caller.
- Corrected prior "other writer" shorthand:1028A51B passes address11596B04
  to verbose constructor102B5720. Nonempty v plus successful numeric parse
  writes unchecked negated result through that address, but the outer
  log-level setter subsequently overrides it on normal return.
- Numeric failure can retain a prefix value or saturate signed Int32.
  Outer setup uses that output even on failure; verbose constructor
  requires success. Saved observation-based decisions, not a byte parser.
- Added LoggingSetupNativeLayout and focused docs/native checks. Build
  succeeds;1637 pure and1882 sample-backed checks pass. No logger, file
  deletion/open, mutex, target, callback or services executed.
- Agent1eee0217-8d6a-414a-9e36-3b30e6ad8841 is assigned a bounded independent
  DownloadObject response/retry trace. Do not mark complete before receiving,
  integrating and validating its report.
- Further logger follow-ons: verbose module match semantics and actual
  file-opening helper10289C70; no implementation or completion claimed.

## DeploymentCommon response and shared retry boundary

- Logging setup batch committed/pushed as17e754f.
- Agent1eee0217-8d6a-414a-9e36-3b30e6ad8841 finished its response trace;
  integrated DownloadResponseNativeLayout and evidence docs.
- Method9E/bodyRVA4550: observed status206 reads stream directly, status200
  sleeps1500ms then increments local6 and retries when incremented count<=10.
  Other statuses log and read only if GetResponse returned normally; HTTP
  error exceptions before this boundary are not modeled or suppressed.
- Final MD5 mismatch increments the same local6 and uses the same strict
  >10 terminal comparison. Counter starts0 outside retry loop, not reset
  between attempts. Saved pure decisions perform no request/sleep/hash.
- Build succeeds;1663 pure and1910 sample-backed checks pass. No target
  assembly loaded/executed, no chunk files or network requests created.
- Both recently assigned agents are finished; no active assignment remains.
- Follow-ons remain logger file opening10289C70, verbose module matching,
  or downloader outer exception propagation; none marked complete.

## Awesomium log-file opening

- Response/retry batch committed/pushed as0dcf696.
- Open helper10289C70 returns true for any already nonzero stored handle,
  even syntheticFFFFFFFF. Zero first prepares path, then destinations1/3
  request file opening; other destinations return true without an open.
- Configured/fallback attempts use GENERIC_WRITE, share3, OPEN_ALWAYS4,
  normal attributes80. Zero/FFFFFFFF configured result tries .\debug.log.
  Accepted handle seeks0 from FILE_END2, ignores result and returns true.
  Failed fallback clears storage11596B0C and returns false. No close here.
- Default builder10289A20 uses GetModuleFileNameW260 with ignored return,
  scans WCHAR terminator, truncates after last backslash when found, then
  appends debug.log. Missing backslash does not add a separator. Allocation
  failure/path API errors remain unresolved; no safe fallback invented.
- Saved LogFileNativeLayout and focused docs plus native guards. Build
  succeeds;1703 pure and1954 sample-backed checks pass. No log/files/API
  execution. True does not prove a valid stored handle or successful seek.
- Next bounded trace: destructor10289EC0 actual log-file write and error
  return handling, or independent downloader outer exception propagation.

## Awesomium log sink admission and writes

- File-opening batch committed/pushed as0e0cd83.
- Unconsumed destructor10289EC0: destinations2/3 call OutputDebugStringA
  and stderr regardless of severity; others call stderr only signed
  severity>=2. Destination0 therefore does not suppress all diagnostics.
- stderr uses fprintf("%s") and fflush on CRT array11553420+40, whose
  static FILE descriptor is2. Results ignored. File block admits any
  destination except0/2, attempts synchronization, calls open helper,
  checks only AL, then seeks EOF again and writes stored string byte length.
- WriteFile BOOL and bytes-written local ignored, no partial-write retry,
  FlushFileBuffers, close or result-driven failure branch in this path.
  Null-terminated debugger/stderr output differs from length-based write.
- Synthetic unknown destinations can enter file block while zero-handle
  open helper skips actual open and returns true; do not normalize them
  or equate attempted output with delivery.
- Saved LogSinkNativeLayout, focused docs and native guards. Build succeeds;
  1835 pure and2089 sample-backed checks pass. No target/sink/filesystem or
  synchronization API executed.
- Follow-ons: mutex name/setup10289AE0 (default path or caller path,
  backslash normalization, Global prefix, initialized flag/failure branch);
  downloader outer exception propagation. No active agent assignment.

## Awesomium logger synchronization setup

- Sink-admission batch committed/pushed as8edea89.
- Initializer10289AE0 skips changes when byte11596B20!=0. Otherwise stores
  supplied mode11596B28; nonzero mode attempts24-byte critical-section
  allocation, zero mode reuses nonzero mutex11596B2C or builds/creates one.
- Null critical allocation stores pointer0 yet marks initialized1. Mutex
  creation0 stores0 and returns before flag assignment, allowing later retry.
  Existing/nonzero mutex is not validated. Flag is not an interlocked once.
- Nonnull path (even empty) copied via WCHAR terminator; null invokes
  default builder10289A20. Copied path backslashes become forward slashes,
  exact Global\ prefix prepended; case/colon/dot segments untouched.
- CreateMutexW null security/initial ownerFALSE. Only zero is failure; no
  GetLastError or existing-object branch. Saved names are observed copied
  strings, not Win32 acceptance guarantees or a real lock implementation.
- Build succeeds;1881 pure and2139 sample-backed checks pass. No target,
  API, file or mutex executed/created.
- Agent0c7dcf0a-3f05-4c4e-974a-fd9bdbe8191e is assigned immediate managed
  downloader worker/caller exception propagation. Await its report before
  completing that task; no outcome claimed yet.

## DeploymentCommon worker exception propagation

- Synchronization batch committed/pushed as c82c959.
- Agent0c7dcf0a-3f05-4c4e-974a-fd9bdbe8191e finished. Integrated
  DownloadWorkerNativeLayout and focused worker evidence docs.
- ThreadRunner Method7D/bodyRVA3E10 catches System.Exception around
  DoDownload: WebException plus global recoverable count<10 and either
  status14 or exact message "test" requeues/reset event and increments
  part/global recoverable counters. This is separate from row9E's shared
  response200/MD5 retry counter.
- Nonqualifying exception records first fatal slot under lock, stops
  running, aborts other workers and signals queued events. Already occupied
  fatal slot is preserved. Terminal custom range exception is not WebException.
- DoDownload finally marks complete/signals supplied event. Outer row7C
  waits events/workers, logs captured error and throws retrieved exception
  object, not IL rethrow. Empty slot only permits later assembly; it does
  not guarantee all later filesystem/reporting operations succeed.
- Build succeeds;2122 pure and2386 sample-backed checks pass. No target,
  worker threads, network, files or callbacks executed.
- Agent assignment complete; no active agent assignment remains.
- Follow-ons: Awesomium verbose rule matching102B53A0, or managed worker
  cancellation/Thread.Abort aftermath. Neither is captured as completed.

## Awesomium verbose rule source and precedence

- Worker-propagation batch committed/pushed as4192d02.
- Lookup102B53A0 scans36-byte records forward and returns first matching
  rule level+1C; no-match/empty rules negate live threshold via object+10,
  unchecked. Kind+20 exactly1 selects full original source; otherwise module.
- Pattern containing either slash creates kind1 (constructor102B58A7);
  append102B5650 preserves order/level/kind. Source module102B5210 strips
  through last slash/backslash, last-dot extension, then exact "-inl".
  No Unicode decode, lowercase, filesystem canonicalization or NUL truncation
  in saved observed byte-span transformation.
- Matcher102B52B0 literals case-sensitive, separator spellings equivalent,
  question mark consumes one byte, star recursively tries suffix positions.
  Saved only match-observation selection, not wildcard/rule-text parser.
- Added VerboseRuleNativeLayout and focused docs. Build succeeds;
  2146 pure and2414 sample-backed checks pass. No target/logger execution.
- No active agent assignment. Follow-on: numeric failure while constructing
  per-module rule still appends output; rule-text helper102A0670 split and
  empty/duplicate handling unresolved. Keep prior snapshot-state2-writer
  task pending; no new evidence resolves it.

## Awesomium verbose construction error retention

- Verbose lookup batch committed/pushed as3b7c8b9.
- Followed102A0670 ->102A03F0 ->102A0150: comma fragments trimmed on both
  edges using exact bytes09/0A/0B/0C/0D/20. Empty trimmed fragments skipped.
  Pair102A04D0 clears outputs, first "=" separates key, consecutive "="
  skipped before one remaining value suffix. No independent key/value trim.
- Pair error sets sticky false but still appends retained/empty outputs.
  Constructor likewise continues with produced pairs despite aggregate false.
  Numeric conversion102B58E6 status controls diagnostic cleanup only,
  both outcomes append the observed output level at102B5911. Duplicate
  patterns remain append-ordered; lookup earlier first-match wins.
- Saved evidence docs/ledger and native guards, deliberately no parser.
  Build succeeds;2146 pure and2418 sample-backed checks pass. Derived text
  examples are static deductions, not runtime parser captures.
- Agentbdfbfb74-e45b-490e-baa3-793c603663af has bounded managed downloader
  cancellation/cleanup assignment; await and integrate report. No claim
  that Thread.Abort means completed cancellation or disposed resources.
- Next static follow-ons: verify verbose module levels relative to caller
  source-span construction1028A790; allocator/aliasing behavior if required.

## DeploymentCommon cancellation and bounded cleanup

- Verbose construction batch committed/pushed as f9c2dc8.
- Agent bdfbfb74-e45b-490e-baa3-793c603663af completed cancellation trace.
  Integrated DownloadCancellationNativeLayout and focused evidence document.
- First fatal worker clears mIsRunning, requests Thread.Abort only for
  other enumerated threads, drains queue and signals ManualResetEvent
  values. ThreadAbortException skips ordinary diagnostic block, but CLR
  automatic rethrow behavior is not suppressed or modeled as graceful exit.
- Outer row7C waits individual events then live workers. Timer finally
  try28D/69 handler2F6/C disposes nonnull timer local; no whole-method
  timer cleanup and no individual wait-handle disposal in that method.
  Dispose does not establish completion of an in-flight timer callback.
- Added exact finally-region metadata matcher to SampleImage and checks.
  Corrected agent-proposed queue-drain guard against immutable bytes:
  actual span has one getter/type-test, not duplicated getter sequence.
- Build succeeds;2150 pure and2428 sample-backed checks pass. No workers,
  targets, timers, cancellation requests, network or filesystem APIs run.
- No active agent assignment remains. Follow-ons: verbose source-span
  caller1028A790 and negative threshold fallback arithmetic; cancellation
  effects beyond immediate managed boundary remain unresolved.

## Awesomium verbose caller extent and fallback arithmetic

- Cancellation batch committed/pushed as173681b.
- Level helper1028A790 mode==1/minimum<=3/extent0 can diagnose N > 0U,
  then continues on returning paths and reloads threshold. Configured path
  decrements supplied extent modulo32, not strlen; synthetic0 yieldsFFFFFFFF.
- Representative caller1006D799 passes41-byte source literal with extent42,
  then signed level>=20 admits severity-20 diagnostic. Not generalized to
  every caller or malformed pointer validity.
- Unconfigured fallback is max(unchecked(-minimum),-1), verified NEG then
  CMP/JG flags. Configured no-match fallback remains raw negation, so3
  gives-1 versus-3, Int32.MinValue gives-1 versusInt32.MinValue.
- Saved VerboseLevelNativeLayout and updated evidence docs/native guards.
  Build succeeds;2224 pure and2505 sample-backed checks pass. No target,
  source-pointer dereference, diagnostic, callback or logger executed.
- No active agent assignment. Follow-ons: named mutex collisions and
  initialization reset writers (xref11596B20), or client-side logging
  bridge into Awesomium; retain unresolved state2 snapshot writer.

## Awesomium synchronization reset xref closure

- Caller-extent batch committed/pushed as7ee9dba.
- Direct xrefs in this Awesomium sample: initialized byte11596B20 is tested
  and set1 only in initializer10289AE0; no direct reset writer found.
  Critical-section pointer11596B24/mutex11596B2C are initialized and used
  for enter/leave or wait/release; no direct close/destroy/reset xref found.
  This is scoped to direct refs in this image, not proof about indirect
  writes or code outside it.
- Corrected distinction: reconfiguration1028A2A0 closes global log file
  handle11596B0C at1028A684 and stores0 at1028A68A. It does not close
  mutex or free critical-section object. Destinations0/2 retain stored
  path object; other destinations replace it.
- Added lifecycle decisions/docs and instruction guards. Build succeeds;
  2238 pure and2521 sample-backed checks pass. No target, file, logger,
  mutex or critical-section API executed.
- Remaining global reset/concurrent mutation via indirect stores is
  unresolved. Do not equate missing direct xrefs with proof of impossibility.

## FirefallClient Red5 logging-method setup

- Added an inert C# decision model and evidence checks for client raw
  initializer `0xD668D0`, plus a focused evidence doc.
- On the original client image, the initializer returns ready when byte
  `this+0x73` is nonzero; returns unavailable without retry when `this+0x73`
  is zero and `this+0x72` is nonzero; otherwise attempts setup. It requests
  the `Red5` JS object and, after validity succeeds, registers `errorlog` via
  imported `JSObject::SetCustomMethod` with trailing argument0, then sets
  `this+0x73`. Failure selects a bounded string index 0..7, logs
  `Unable to create Red5 JS object: %s`, and sets `this+0x72`.
- `SetCustomMethod` has two observed sites: raw function `0xD64530` and
  initializer `0xD668D0`. The other setup-gated call in raw function
  `0xD651B0` is `JSObject::SetPropertyAsync` via IAT slot `0x1AC4EF0`, not a
  third `SetCustomMethod` site. The separate WebPreferences constructor has
  one direct call at raw `0xCBC1B1`; its adjacent initialization is not
  evidence of a logging preference.
- Checked the call sites, decorated imports, UTF-16 object/method names and
  ASCII failure format against `FirefallClient.exe.original`. Build succeeds;
  2256 pure and2543 sample-backed checks pass. No browser, script, logging
  API, or callback was run.
- Follow-up: identify the mapped callback queue consumer and the
  JavaScript-side call site. The pending state2 snapshot-writer question
  also remains open.

## FirefallClient Red5 callback routing

- Corrected the Red5 call-site record: the call in raw function `0xD651B0`
  uses `JSObject::SetPropertyAsync`; only the raw `0xD64530` and `0xD668D0`
  sites observed here call `JSObject::SetCustomMethod`.
- RTTI associates the `JSMethodHandler` subobject table at VA `0x1C7656C`
  with raw callback function `0xD65A60` (VA `0x1166660`). The handler gets
  the saved JSObject remote ID through IAT VA `0x1AC4F00` and compares it
  against the incoming ID at raw `0xD65AF9`; mismatch enters the unknown
  callback logger.
- On the fallback path after the mapped-object lookup, raw `0xD65A60`
  compares the incoming method name with UTF-16 `errorlog` at VA `0x1BD4994`
  via `WebString::operator==` (IAT VA `0x1AC4EA4`). If no argument is
  present, or argument zero is not a string, it returns without message
  logging. Otherwise it reads argument zero, verifies `JSValue::IsString`,
  converts with `JSValue::ToString`, and calls helper raw `0x2FC4F0` with
  format `%s - %s` (VA `0x1BD498C`) and logger object VA `0x1E68688`.
  The first format value is the frame label when non-null, otherwise the
  empty-string pointer VA `0x1C5EA68`.
- Unknown names use format VA `0x1BD48F0` and logger object VA `0x1E68668`
  through helper raw `0x2FCC60`. No target or callback was executed.
- The mapped path uses an ordered string-keyed tree rooted at offset `+0x474`
  of the enclosing object. Raw `0xCCD640`/`0xCCDC70` compare the method key
  with `strcmp`; a lookup miss or second sentinel check falls through to the
  `errorlog` special case. On a match, raw `0xD65A60` creates a 12-byte
  record with retained method-name text, a `JSValue`, and a `JSArray`; the
  lookup result itself is not retained in the record.
- For nonempty arrays, the handler checks the final argument; if it is an
  object, raw `0xD64320` invokes a cached predicate from ASCII source VA
  `0x1BD4570`. The predicate requires truthy `object`, `constructor`, `call`,
  and `apply` properties. A defined, truthy result causes the handler to
  separate that final value and pop it from the copied array. Otherwise it
  preserves all arguments. Raw `0xD66B70` appends the record to the handler's
  collection at `this+0x34`.
- Added callback route and trailing-callable-like-argument decision models,
  plus sample guards for the lookup, predicate, array pop, and queue append.
  At that point, the queue consumer and actual JavaScript caller of
  `Red5.errorlog` remained unresolved.
- Validation: solution build succeeded with 0 warnings/errors; all 2556
  CompanionTools checks passed, including original-sample checks; `git diff
  --check` is clean.
- Validation: solution build succeeded with 0 warnings/errors; all 2552
  CompanionTools checks passed, including original-sample checks; `git diff
  --check` is clean.

## FirefallClient Red5 callback queue dispatch

- Corrected the queued record interpretation: its first field is retained
  method-name text, not a retained tree lookup result. Consumer raw
  `0xD64AE0` uses that key to perform a fresh lookup in the mapping tree.
- RTTI identifies `0xD64AE0` as virtual slot20 of the primary
  `uiWebFrameInternal_17` vtable (table VA `0x1C765FC`, slot VA
  `0x1C7664C`; complete-object locator VA `0x1CD0FE0`, object offset0).
- The consumer drains the queue at `this+0x4C` while the pending count at
  `this+0x5C` is nonzero. A missing current mapping discards that queued
  callback. Otherwise it retrieves the mapped Lua state/reference, converts
  the JavaScript arguments through `0xCBBE00`, and dispatches the Lua
  callback via adapter `0x1029540` inside wrapper `0xCD6C10`. The `JSValue`
  in the record is a separate, optional JavaScript continuation.
- An undefined continuation, tested through imported `JSValue::IsUndefined`
  at IAT VA `0x1AC4EE8`, skips only that JavaScript continuation. When
  defined, Lua return values are marshalled into a `JSArray` and passed to
  its `call` method with a null receiver.
- Global VA `0x1E686C4` offset `+8` selects imported
  `JSObject::InvokeAsync` at IAT VA `0x1AC4EEC` when nonzero, or synchronous
  `JSObject::Invoke` at `0x1AC4EDC` otherwise. The synchronous path logs the
  return value; conditional diagnostics include the callback name and
  argument strings.
- Added an inert decision model and original-sample guards for the vtable
  slot, queue count, fresh method lookup, `call` method string, both invocation
  imports, and diagnostic formats. The JavaScript caller of `Red5.errorlog`
  remains unresolved.

## FirefallClient Red5 bridge teardown

- The deleting-destructor wrapper at raw `0xD64180` calls `0xD64250`; that
  destructor calls cleanup routine `0xD64710` before destroying its member
  containers.
- Cleanup drains the pending queue at `this+0x4C` with count at `this+0x5C`.
  Each queued record is passed to raw `0xD648A0`, which destroys its
  `JSArray` at `+8`, destroys its `JSValue` at `+4`, and decrements/releases
  the retained method-name text at `+0`. The queue item is then freed. This
  loop does not call `JSObject::Invoke` or `InvokeAsync`.
- Cleanup resets `this+0x24` to `-1`. The pointer at `this+4` is compared
  with shared sentinel `[dword_1E002B8+0x5D0]`; a distinct non-null pointer
  receives virtual slot0 with argument1 and is then cleared, while the
  sentinel value remains untouched. Its semantic type is unresolved.
- Added a decision model and sample guards for the destructor relationship,
  queue-record release, field destructors, and pointer sentinel handling.

## FirefallClient Lua/web bridge source check

- Installed `system\gui\components\Test\WebTest.lua` loads local
  `awesomium_test.html`, registers `myfunc`/`MoveMe` with `AddWebCallback`,
  exposes several `AddWebValue`s, and returns a nested Lua table from
  `TestCallback`. The HTML calls `Red5.myfunc` with a trailing JavaScript
  callback, calls `Red5.MoveMe`, and defines `Red5Callbacks.test_from_lua`
  for Lua's `CallWebFunc`. This establishes the generic bidirectional test
  contract, but does not show an `errorlog` call. The native
  `AddWebCallback` implementation is traced below.
- The main UI Lua source reads `ingame_host` and loads
  `host .. "/panelmanager"`; it also loads its selected panel URL in that
  frame. A case-insensitive search of the installed `system\gui` text/script
  sources found no `errorlog` call. The actual caller remains unresolved;
  dynamically loaded page code is a possibility, not confirmed evidence.

## FirefallClient AddWebCallback Lua registration and dispatch

- Raw `0xCCAD70` is the Lua `AddWebCallback` binding. It validates the
  callback argument, obtains a callback reference through `0xC56170`, and
  calls `0xCCC130` with the method name, Lua state, and reference.
- Registration helper `0xCCC130` requires a nonzero field at `this+0x418`,
  rejects reference values `-1` and `-2`, and rejects names containing `'`
  or `\`. It stores the Lua state and reference in the method tree at
  `this+0x474`; replacement releases the old reference through `0xC56250`
  before removing and replacing the entry.
- The helper calls virtual slot `+0x30` on the object at `this+0x41C`. A
  vtable at VA `0x1C765FC` shares the `uiWebFrameInternal_17` RTTI locator
  and has `0xD64530` at slot VA `0x1C7662C`; `0xD64530` gates and calls
  `JSObject::SetCustomMethod`. The field's dynamic vptr assignment was not
  independently located, so this is a strong slot match rather than a fully
  closed runtime pointer trace.
- Queue consumer `0xD64AE0` converts JavaScript arguments through
  `0xCBBE00`, then dispatches the registered Lua callback via adapter
  `0x1029540` inside wrapper `0xCD6C10`. The queued `JSValue` at `+4` is the
  optional trailing JavaScript continuation, not the registered Lua
  callback. The consumer invokes that continuation with marshalled Lua return
  values when it is defined; the undefined check skips only the continuation.
- Updated the inert C# decisions, sample guards, and Red5 bridge evidence
  document to reflect registration/replacement and the two-stage
  JavaScript-to-Lua-to-JavaScript flow. The installed `WebTest.lua` and HTML
  corroborate the generic callback flow, but do not show a call to
  `Red5.errorlog`; its JavaScript caller remains unresolved.
- Raw `0xCCAF10` is `AddWebValue`: it accepts string/numeric property names,
  dispatches through frame virtual slot `+0x54` to `0xD651B0`, marshals the
  value via `0xCBBDB0`, and uses `JSObject::SetPropertyAsync`.
- Raw `0xCCB060` is `CallWebFunc`: it dispatches through virtual slot `+0x58`
  to `0xD65250`, which resolves `Red5Callbacks`, checks the named function
  with `JSObject::HasMethod`, marshals Lua arguments into a `JSArray`, then
  selects `InvokeAsync` or `Invoke` using the shared `0x1E686C4+8` state.
  The local WebTest pair corroborates the `Red5Callbacks` target. Both
  bindings and their invocation decisions are recorded in the inert C# model
  and sample-backed checks; no executable or script was run.
- Raw `0xCCB9A0` registers twelve Lua WebFrame names from `LoadUrl` through
  `IsTypeTarget`. Twelve small stubs at `0xCCE8B0`-`0xCCE960` forward to
  their respective implementations. The names, stub bytes, and targets are
  recorded in the bridge document, along with traced URL, filter, dimension,
  logging, and type-target behavior.
- `SetUrlFilters` (`0xCCB1C0`) replaces the list at frame offset `+0x468`;
  `0xCCCB20` retains only nonempty patterns ending in `*` using `strncmp`,
  logging other values. `LogWebRequests` (`0xCCB3D0`) stores Lua truthiness
  at frame offset `+0x458` despite a boolean usage string. `ClearCookies`
  (`0xCCB4E0`) calls internal-object slot `+0x68`, resolved by the object
  constructor to thunk `0xD65A10`. The thunk calls nested-object slot `+0x04`;
  that final method remains unresolved. Its missing-argument usage string
  incorrectly names `LogWebRequests`.
- `LoadUrl` (`0xCCABA0`) converts numeric URLs to text and calls associated
  object slot `+0x5C` when the object at frame offset `+0x418` exists; it
  pushes `true` after dispatch and pushes no value when that object is null.
  `GetUrl` (`0xCCACC0`) returns the pointer at `+0x448` or an empty string.
- `SetNaturalDims` (`0xCCB5B0`) writes float dimensions at `+0x45C/+0x460`
  and calls `0xCCC5C0` only on change; `GetNaturalDims` (`0xCCB710`) returns
  both fields. `EnableTypeTarget` (`0xCCB7C0`) applies Lua truthiness via
  `0xCCD0F0`, which updates flag `+0x459` and may synchronize the active
  target; `IsTypeTarget` (`0xCCB8C0`) returns that flag. Its usage string
  misleadingly advertises a boolean argument that its code does not read.
- `LoadUrl`'s vtable slot `+0x5C` resolves to `0xD65690`. This handles
  `local://` specially using a suffix and base-path helper chain; ordinary
  URLs use `0x2EE810`, and both paths call nested-view slot `+0x64`. The
  natural-dimension helper computes a clamped, rounded scale percentage and
  synchronizes nested-view slots `+0x38/+0x3C` only when the value changes.
- Stored URL filters are consumed by `0xCCCC10`, which treats the final `*`
  as a prefix wildcard and uses case-sensitive `strncmp` for the preceding
  bytes. Callback `0xCBDDE0` returns false for an unchanged context key;
  otherwise it negates the match result. Constructor `0xCBC410` installs the
  table at VA `0x1C747B8` as an object's secondary vtable at offset `+0x04`;
  the callback occupies its second slot. Setup routine `0xBFB310` gets the
  object through factory `0xCBB7A0` using owner fields `+0x9A4/+0x9A8`,
  stores it at `+0xA04`, then calls primary slot `+0x08`. Its mapped callback
  VA is `0x10BE9E0`; the owner's API type and callback allow/cancel semantics
  remain unresolved. Owner cleanup `0xBF8AC0` calls the `+0xA04` object's
  primary slot `+0x10` with `1`, then clears that field. The slot at VA
  `0x1C747DC` points to `0xCBC4D0`, which calls reset `0xCBC510` and uses the
  low argument bit to select final release. Owner cleanup also releases and
  clears the shared-text values at `+0x9A8/+0x9A4`. Their meanings remain
  unknown. Setup copies its incoming path into owner field `+0x5C8`, adds a
  trailing separator as needed, and converts `/` to `\`; empty input uses
  `"./"` (`0x1BBA0FC`). Empty `+0x9A4` uses `"web"` (`0x1BBA15C`) composed
  with that base; empty `+0x9A8` uses `"./"` (`0x1BBA160`). These support a
  root/web-subdirectory interpretation, but the API purpose remains unknown.
  Secondary slot `+0x00` is raw
  `0xCBD2C0`; its normal
  return path sets `EAX=0` and consumes one stack argument after substantial
  object processing. Its contract remains unidentified. Callback `0xCBD2C0`
  passes text from incoming-object virtual methods to
  parser `0xBCE800`. The parser uses `strncpy` with a 2048-byte bound,
  recognizes `http://`/`https://` case-insensitively via `_stricmp`, defaults
  prefixless text to HTTP, parses optional ports with `atoi` (valid 1–65535,
  default 80/443), and fills output offsets `+0/+4/+8/+0x0C/+0x10` with
  secure flag, host, port, slash-prefixed path, and query. It removes the
  fragment before splitting the query, but only when a slash path exists;
  without a slash, query/fragment text remains part of the parsed host. The
  first-colon port split does not support IPv6 literals. Builder `0x12E4100`
  applies bytewise component escaper `0x12E4810` to parsed host/path and a
  configured identifier. It preserves ASCII alphanumerics and `-._~`, and
  emits uppercase `%HH` for other bytes. A separate local formatter uses
  `"Red5 %s %s %s"` at VA `0x1C576D0`; this alone does not establish network
  transmission. These details are sample-guarded; callback meaning remains
  unresolved. The primary table at file offset `0x18733CC` maps slot
  `+0x08` to VA `0x10BCD80` (raw `0xCBC180`). That method calls `0xCBC270`,
  invokes the helper at object `+0x08` twice using configuration at `+0x14`,
  stores results at `+0x0C/+0x10`, then conditionally calls primary slot
  `+0x04`. Setup passes flag `2` only if both its stack-byte argument and
  owner byte `+0x5C` are nonzero; else it passes `0`. Primary slots `+0x00`
  and `+0x04` point to `0xCBC150` and `0xCBC160`, which call virtual slot
  `+0x18` on the nonnull helper objects at `+0x08`, and `+0x0C/+0x10`,
  respectively. They appear to release those helpers; interface semantics
  remain unknown. Constructor capacity field `+0x2C` starts at `7`; `CBC270`
  enters only for nonzero mode and storage pointer `+0x28`. For mode other
  than `1`, it sets pending byte `+0x68` when helper `+0x08` exists and result
  `+0x0C` is absent. Reset routine `0xCBC510` (called by wrapper `0xCBC4D0`)
  calls `0xCBC270` with mode `2` only when `+0x68` is set, then clears it.
  It invokes cleanup on nonnull members `+0x0C/+0x10`, indirectly releases
  nonnull `+0x08`, calls `0xCBE080` with member `+0x48`, and conditionally
  passes buffers `+0x30/+0x18` to `0x12C8D60` when capacities are at least
  `8`. It restores observed capacities to `7`, clears pointer/length fields,
  invokes an unresolved initializer on `+0x14`, and leaves primary vtable
  `0x1C747E4` (the secondary-vtable source is unresolved). Wrapper
  `0xCBC4D0` always resets first and tests argument bit 0 for allocation
  release. Sample-backed assertions cover this cleanup sequence; exact
  helper identities and the user-facing meaning remain unresolved. The
  `0xCBE080` list helper passes an optional pointer at list `+0x08` to the
  shared allocator release path, clears it and `+0x0C/+0x10`, self-links the
  sentinel at `+0x00`, clears count `+0x04`, walks forward links releasing
  nodes, and tail-jumps to the release path with the sentinel. This matches
  circular doubly-linked-list cleanup; exact types remain unknown. Constructor
  helper `0xCBE310` allocates its `0x14`-byte sentinel through `0xCBF080`, then
  seeds 16 copies of that sentinel into a vector-like submember via
  `0xCBEE50`. The pool-like role and later values `7/8` are unresolved.

# Round 23: Auto-login AES key setup

## Observed
- `Login_StartAutoLogin` (`0x63DE80`) calls `0x12E3960` on accepted branches
  with a non-empty input string. The callee stores that string at
  object offsets `+0x3C` and `+0x48`, then clears `+0x40`. These offsets match
  the first signing-key field, `uid`, and optional second signing-key field
  read by `0x12E4100`, but identity between the object instances used by the
  login and ClientApi paths is not established.
- `0x12E3960` hashes two separate suffixed forms of the input with CryptoAPI
  SHA-1 (`CALG_SHA1`, 20-byte digest). The second digest is converted into a
  28-byte `PLAINTEXTKEYBLOB` for `CALG_AES_128` (`0x660E`): its first 16 bytes
  become the AES key. `CryptSetKeyParam` with parameter 1 sets the first
  digest as the IV. The key object is stored at `this+0x58`. Embedded suffix
  values are intentionally not reproduced in the source reconstruction.

## Unresolved
- The AES mode, the consumer of the imported key/IV, and the ownership/lifetime
  of the key object remain unknown. The relationship between these derived
  values and the optional second ClientApi HMAC key is also unresolved.

# Round 24: AES buffer helpers

## Observed
- `0x12E4680` passes the AES key object at parent offset `+0x58` to
  `0x12EF0D0`. That helper calls CryptoAPI `CryptDecrypt` on mutable blob
  storage. The former "configured block-size" interpretation is superseded:
  the later verified publication trace below identifies address alignment,
  not cipher block size.
- `0x12EF1E0` contains a corresponding `CryptEncrypt` call on the same key
  wrapper type. Neither `0x12E4680` nor `0x12EF1E0` has direct code xrefs in
  the current IDB; their higher-level callers may be indirect or unrecognized.

## Unresolved
- The AES mode, the encryption/decryption dispatch path, and the application
  data protected by these wrappers are not established. Do not model cipher
  output until the stream contract and dispatch caller are resolved.

# November 4 continuation: verified import and receiver mappings

The following checks used the active `FirefallClient.exe` image, 40,197,848
bytes, SHA-256
`97362D668FF64778193A64759265E4AE90842043C4A15B627DF9902FD9135445`.
They did not use `FirefallClient.exe.original`. Analysis was read-only; no
executable execution, patches, or IDB changes were made.

## CryptoAPI imports: correct the raw-address interpretation

The absolute operands `0x1AC4004` through `0x1AC406C` are PE virtual
addresses, not raw offsets. Reading raw `0x1AC4000` samples `.rsrc`, not the
IAT. Its repeated `07`/`06` bytes do not establish unresolved or dynamically
resolved imports. The empty IDA import enumeration is also not evidence that
the PE import table is empty.

The PE header is at raw `0x170`, image base is `0x400000`, and `.rdata`
starts at RVA `0x16C4000`, raw `0x16C2C00`. The six slots are ordinary
`ADVAPI32.dll` imports:

| IAT VA | RVA | Raw offset | Import |
| --- | --- | --- | --- |
| `0x1AC4004` | `0x16C4004` | `0x16C2C04` | `CryptDecrypt` |
| `0x1AC4008` | `0x16C4008` | `0x16C2C08` | `CryptDuplicateKey` |
| `0x1AC400C` | `0x16C400C` | `0x16C2C0C` | `CryptSetKeyParam` |
| `0x1AC4014` | `0x16C4014` | `0x16C2C14` | `CryptAcquireContextA` |
| `0x1AC403C` | `0x16C403C` | `0x16C2C3C` | `CryptImportKey` |
| `0x1AC406C` | `0x16C406C` | `0x16C2C6C` | `CryptEncrypt` |

The import directory is RVA `0x19BCDCC`, raw `0x19BB9CC`, size `0x280`.
Its ADVAPI32 descriptor at raw `0x19BBB20` has OriginalFirstThunk RVA
`0x19BD04C`, DLL-name RVA `0x19C0A32` (raw `0x19BF632`), and FirstThunk
RVA `0x16C4000`. The DLL name is `ADVAPI32.dll`; its 31 imports end with
a null thunk. This does not exclude unrelated dynamic resolution or runtime
IAT replacement elsewhere.

Exact six-byte call starts, all raw offsets:

| Callsite | Instruction bytes | API |
| --- | --- | --- |
| `0x12EF15A` | `FF 15 04 40 AC 01` | `CryptDecrypt` |
| `0x12EF103` / `0x12EF238` | `FF 15 08 40 AC 01` | `CryptDuplicateKey` |
| `0x12EF0A0` | `FF 15 0C 40 AC 01` | `CryptSetKeyParam` |
| `0x12EEF8E` | `FF 15 14 40 AC 01` | `CryptAcquireContextA` |
| `0x12EEFBE` | `FF 15 3C 40 AC 01` | `CryptImportKey` |
| `0x12EF298` | `FF 15 6C 40 AC 01` | `CryptEncrypt` |

The bounded mode/lifetime continuation verified the following:

- Constructor `0x12EEF20` imports `CALG_AES_128` (`0x660E`) with a 16-byte
  key. In constructor, IV setter, encrypt/decrypt, and inspected parent
  construction/replacement paths, no explicit `KP_MODE=4` or padding setter
  was observed. Setter `0x12EF0A0` uses `KP_IV=1` (`6A 01` at
  `0x12EF09B`). A whole-image literal-call scan additionally found import
  jump thunk `0x15A0CD0` (`FF 25 0C 40 AC 01`), with no recorded IDA xrefs.
  These checks do not exclude arbitrary indirect callers. CBC/default
  padding remains a provider-default interpretation, not an observed
  application setting.
- Decrypt duplicates the key at `0x12EF103` and passes that duplicate to
  `CryptDecrypt` (`FF 75 FC` at `0x12EF157`). Encrypt duplicates at
  `0x12EF238`, but reloads the original wrapper at `0x12EF28B`
  (`8B 45 F4`) and passes its original key at `0x12EF295` (`FF 70 04`).
  Both paths destroy their duplicates on success and failure. Both crypto
  calls use `Final=TRUE`; original-key encryption alone therefore does not
  establish cross-message CBC chaining.
- Parent `+0x58` is a pointer to an allocated eight-byte key/provider
  wrapper, not inline handles. Replacement calls destructor `0x12EF000`
  at `0x12E3B14`, frees the old allocation at `0x12E3B1B`, stores the new
  pointer at `0x12E3B23`, then sets its IV at `0x12E3B29`. Other replacement
  paths occur at `0x12E4089..0x12E40A1` and through `0x12E4EA0`.
- Explicit reset `0x12E37A0` destroys/frees the wrapper at
  `0x12E386E/0x12E3875` and clears the pointer at `0x12E387A`
  (`C7 46 58 00 00 00 00`). Owner teardown `0x31C3A0` destroys/frees it at
  `0x31C438/0x31C43F`. Wrapper destructor `0x12EF000` destroys its key
  handle at `0x12EF00B`, clears wrapper `+4`, releases the provider at
  `0x12EF039`, and clears wrapper `+0`.

Next bounded task: establish the parent type and actual instance provenance
between auto-login key setup and ClientApi signing. Cipher consumers remain
unresolved.

## PrivateDialog receiver identity and numeric event 229

Keep selector `0x79` separate from selector `0x59`. For the former,
dispatcher entry VA `0xC1D840` / raw `0x81CC40` points to VA `0xC1D663` /
raw `0x81CA63`. The call at raw `0x81CA82` is
`FF 90 68 01 00 00` (receiver slot `+0x168`). Its vtable entry VA
`0x1C72028` / raw `0x1870C28` contains VA `0xECEF00`, raw `0xACE300`.
Bytes `E9 0B 32 EC FF` there tail-jump to raw `0x991510`.

Selector `0x59` instead calls slot `+0xE8` at raw `0x81C522` and reaches
raw `0xACA9D0`. That method clears/resets the tree at context `+0x43C8`
and sets state byte `+8`; it does not read the dispatched message argument.
The earlier coordinator assignment naming it as the PrivateDialog receiver
was incorrect.

At raw `0x991662`, `FF 90 AC 00 00 00` calls the `uiManager` loaded from
VA `0x1E002B8`, with arguments `(229, &stack_record, 0)`. The preceding
pushes at raw `0x991658` are `6A 00 52 68 E5 00 00 00`. Its vtable slot
`+0xAC` resolves to raw `0xBF9D80`. No event name is inferred.

The consumer reads byte `+0x28`, DWORD `+0x40`, QWORDs `+0x30/+0x34`
and `+0x38/+0x3C`, and a NUL-scanned pointer at `+0x44`. The record-building
path uses keys `success`, `vendor_id`, `product_id`, `price_id`, and `code`.
On the successful empty-code path, it passes address `message+0x44` to a
fallback helper with literal `PURCHASE_SUCCESSFUL`. These observations do
not resolve the previously documented parser/consumer representation
mismatch. Next bounded task: check constructor/parser `0x5EE900` provenance
for fields `+0x3C/+0x40/+0x44` and any intermediate transform.

The subsequent bounded check reconfirmed parser `0x5EE900` writes only
DWORD `+0x28`, QWORD `+0x30`, and DWORD `+0x38`, ending at exclusive
`+0x3C`. Consumer DWORD `+0x40` is read at `0x9915EA` (`FF 76 40`);
pointer `+0x44` is loaded at `0x99154E` (`8B 46 44`). Direct xref/pointer
searches did not locate the parser constructor/caller in that pass, so no
initializing transform was established. This is not proof that no indirect
initializer exists, nor a demonstration of runtime out-of-bounds behavior.
Next check starts at the known parser factory `0x81CC50` and its actual
selector-`0x79` branch rather than repeating the negative reference scans.

That factory linkage is now established. Route0 subtracts `0x53` at raw
`0x81CCAD`; selector `0x79`'s entry at raw `0x8218DC` / VA `0xC224DC`
contains `9E E3 C1 00`, selecting raw `0x81D79E`. That branch requests
`0x40` bytes (`6A 40`) and calls constructor `0x526840` at `0x81D7BF`.
The constructor passes the unadjusted allocation to base `0x5A1510`,
installs vptr VA `0x1C67018`, zeros payload `+0x30/+0x34`, and returns
the same pointer (`8B C6` at `0x526863`). Neither constructor initializes
`+0x3C/+0x40/+0x44`.

The factory moves EAX to ESI at `0x81D7C4`, pushes that pointer at
`0x81D7CA`, and calls reader `0x5EE900` at `0x81D7D5`
(`E8 26 11 DD FF`), without adjustment. The factory returns the same
pointer. Separately, dispatcher `0x81CA81` pushes its incoming message
pointer unchanged before the slot-`+0x168` call. Therefore the constructors
do not reconcile the mismatch: `+0x3C` is inside the requested allocation
but unwritten on this path; `+0x40/+0x44` are beyond the request. Actual
allocator usable size and runtime effects remain unknown. The next narrow
check is pointer identity or transformation between parser return and
consumer invocation, rather than another constructor scan.

## Signing-parent ownership and pending instance alias

The signing/crypto parent is a `0xB0`-byte object constructed at raw
`0x12E36F0` and owned at `tfApplication+0xA28`. Its constructor installs
no vtable; no native parent-type name was established. Application RTTI
names `.?AVtfApplication@@`.

| Raw address | Observed pointer flow |
| --- | --- |
| `0x3031B4` | `89 87 28 0A 00 00`: store constructed parent at application `+0xA28` |
| `0x30DC12` | `FF B6 28 0A 00 00`: pass application's parent to Login constructor |
| `0x636654/0x636657` | Load second argument; store at Login `+0x24` |
| `0x63DF0B/0x63DF13` | Load `[Login+0x24]` into ECX; call setup `0x12E3960` |
| `0x636579` | Load application global from PE VA `0x1DFFD98`, raw `0x19FE198` |
| `0x63659F` | Load that application's `+0xA28` into EDX before tail-transfer to `0xBC1990` |
| `0xBC1A5D/0xBC1A6D` | Restore parent into ECX; call signing builder `0x12E4100` |

These are concrete owner/member paths, not yet proof that Login creation's
application instance equals the global application's instance. Shared
synchronization helpers and nesting counter `+0x30` do not establish that
alias either. The next bounded task traces the application global writer
and mode-construction receiver provenance; do not claim the shared instance
until that edge is verified.

The bootstrap-path alias is now established. Wrapper receiver VA
`0x1DFFD88` owns the global slot at `+0x10`, VA `0x1DFFD98`.
Initializer `0x2EC530` preserves its wrapper receiver in ESI at
`0x2EC534`, constructs the allocated application through `0x303080`
at `0x2EC593`, and stores the returned application at `[ESI+0x10]`
at `0x2EC59C` (`89 46 10`). Constructor `0x303080` returns its
unchanged receiver. The initializer's observed callers at
`0x33402B`, `0x335A7E`, and `0x480568` test the global for zero
and supply wrapper VA `0x1DFFD88`.

Bootstrap loads `[VA 0x1DFFD98]` into ECX at `0x2EB184` and calls
application initialization `0x3063D0` at `0x2EB18F`. That receiver is
preserved through local `[EBP-0x40]`, restored at `0x30788F`, and passed
to mode creation by the call at `0x30798E`. Mode creation preserves the
receiver at `0x30D87E` and passes `[application+0xA28]` to Login at
`0x30DC12`, where the previously verified constructor stores it at
Login `+0x24`. Thus bootstrap-created Login and request preparation
reference the same application slot and signing parent, not just matching
types. This is path-scoped: raw `0x2EC6A4` also explicitly zeros the
global. Permanent uniqueness, concurrent reinitialization, and lifetime
immutability were not proven. The global is in `.data`, whose VA/raw
delta is `0x401C00`.

## PrivateDialog parsed-pointer delivery, ReplayReader branch

The parsed-pointer continuation establishes a concrete conditional
ReplayReader path, not every live Matrix owner path:

- Raw `0x821D87` calls parser `0x81CC50` and preserves its EAX result
  through return. The registered parser call at `0xBD6D3C` returns
  pointer P, which is moved to EDI at `0xBD6D41`, stored in local record
  `+8` at `0xBD6D63`, and enqueued through `0xBD8B80` at `0xBD6D66`.
- Queue helper `0xBD8B80` copies the pointer DWORD unchanged at
  `0xBD8BE7..0xBD8BEA`. Transfer at `0xBD6612..0xBD661B` invokes the
  same record-copy helper for queue `+0xC8`. Iterator helper `0xBD8AD0`
  and record-pointer-storage growth `0xBD8DB0` do not reconstruct messages.
- Delivery loads record `+8` into EDI at `0xBD6699` and into local
  `var_54` at `0xBD669C`. Routing byte 2 or 3 passes `&var_54` to
  context vtable slot `+8`, with selector 1 or 0 respectively.
- On the previously established ReplayReader callback binding,
  `0x7FDC4F..0x7FDC52` forwards the reference argument to `0x7FDC60`.
  That method dereferences it into EBX at `0x7FDC6E`. Its zero-key-byte,
  non-nested-sentinel, non-null-receiver branch pushes P unchanged at
  `0x7FDD4E` into dispatcher `0x81BB90` at `0x7FDD4F`.

No adjustment, replacement allocation, or reparse appears on this specific
branch between parser result and receiver. Nested-message child parsing is
a different branch and excluded. Callback arguments permit replacement:
`0xBD6713` reloads the possibly changed local pointer after the call.
Callbacks or other aliases can also mutate contents. The evidence therefore
does not prove receiver-only fields are globally untouched, nor establish
runtime consequences of the static representation mismatch.

## AES destination is slBlob, not a cursor stream

The concrete vtable at VA `0x1C80CC4` / raw `0x187F8C4` has RTTI
`.?AVslBlob@@`, with `slBufferInterface` in its hierarchy. Its
TypeDescriptor is VA `0x1E35F90`, raw `0x1A34390`.

| Slot | Target raw offset | Observed operation |
| --- | --- | --- |
| `+0x04` | `0x12CB960` | Writable pointer at supplied offset; may detach backing |
| `+0x08` | `0x12CB910` | Pointer at offset without writable-detachment step |
| `+0x0C` | `0x12CB9C0` | Return backing header `+0x0C`, or zero: logical length |
| `+0x10` | `0x12CB9D0` | Resize without preservation, then copy replacement bytes |
| `+0x14` | `0x12CBA00` | Resize to zero without preservation |
| `+0x18` | `0x12CBA10` | Append bytes |
| `+0x1C` | `0x12CBAC0` | Absolute resize with preservation |
| `+0x20` | `0x12CBA80` | Grow by increment and return old-end pointer |
| `+0x24` | `0x12CBA50` | Append nonempty C string without its terminator |

Slot `+0x1C` immediately changes logical length, not just capacity.
Its helper `0x12CBCB0` either reuses suitable unique backing and writes
header `+0x0C` at `0x12CBD33`, or allocates/preserves
`min(oldLength,newLength)` bytes and writes the new length at `0x12CBDFD`.
Capacity is separate at header `+0`; pooled backing explicitly has
capacity `0x800` and initial length zero. Slot `+4(0)` may detach the backing
even without changing length, then returns its data pointer without a
cursor advance.

The AES helpers therefore save the old logical length, enlarge it, obtain
writable storage, and copy input to `base+oldLength`. On success they
resize to old length plus reported output length; on failure they resize
back to old length. This restores logical contents/length, not necessarily
allocation identity, capacity, or discarded-tail bytes; no wiping was
established. Wrapper `0x12E4680` uses a fresh empty destination and publishes
its backing only after decrypt success. Generic source/backing alias safety
and wrapping length arithmetic were not established. The next check is the
wrapper's exact backing-header/object fields and post-publication size logic.

## Signing reset and GUI-logout entry

Reset `0x12E37A0` releases/zeros parent shared-text fields `+0x44`,
`+0x48`, `+0x3C`, and `+0x40` at raw `0x12E380D`, `0x12E3834`,
`0x12E385B`, and `0x12E38BB`. It destroys/frees wrapper `+0x58`,
clears readiness byte `+0x34` at `0x12E3881`, and clears DWORDs
`+0x38/+0x50/+0x54` at `0x12E3885/0x12E388C/0x12E3893`.
Nesting counter `+0x30` brackets the operation.

Observed direct reset callers:

| Callsite | Condition |
| --- | --- |
| `0x30D718` in `0x30D6E0` | Second argument nonzero; parent is application `+0xA28` |
| `0x63AA08` in `0x63A9B0` | Login parent `+0x24` reports readiness nonzero |
| `0x12E3D61` in `0x12E3B80` | Either of its first two text inputs is empty; returns false |
| `0x12E40C5` in code at `0x12E3DA0` | Corresponding empty-input branch; returns false |

The last code entry is not defined as a function in the current IDB;
its instructions, not a recovered function declaration, establish that path.
Function `0x63F5C0` calls cleanup `0x63A9B0` at `0x63F5CB`, then logs
`GUI Requested logout` at `0x63F5DA` (text VA `0x1B62944`, raw
`0x1761544`). This is concrete logout evidence, not a semantic guess from
the reset helper's name.

Preparation `0xBC1990` rejects null parent or false readiness before
signing builder `0x12E4100`, returning `AL=0` at `0xBC1AF9`.
Builder `0x12E4100` separately tests readiness at `0x12E4195` and calls
diagnostic handler `[VA 0x1C15EA0]` at `0x12E41AC` if false, but has no
explicit failure return there if the diagnostic returns. Diagnostic
termination behavior is unresolved. Cached/prepared/in-flight requests and
concurrent interleavings were not examined; this does not establish
cancellation. The next bounded trace follows immediate GUI-logout cleanup
to any actual request drain/cancel boundary.

## AES publication fields and realignment correction

The bounded instruction trace of `0x12E4680` resolves its object/header
layout. These offsets are structure-relative; instruction addresses are raw
offsets in the active sample identified above.

| Structure | Offset | Observed meaning |
| --- | --- | --- |
| slBlob | `+0` | Vtable |
| slBlob | `+4` | Backing header pointer |
| slBlob | `+8` | Ownership/control pointer |
| slBlob | `+0xC` | Byte-address alignment requirement |
| Backing header | `+0` | Capacity |
| Backing header | `+4` | Data pointer, not byte count |
| Backing header | `+8` | Allocation-to-data displacement |
| Backing header | `+0xC` | Logical byte length |

Constructor `0x12CB700..0x12CB765` allocates capacity plus alignment;
for nonzero alignment its displacement is
`alignment - (allocationAddress % alignment)`. Even an already aligned
allocation advances by a full alignment unit. Pool initialization at
`0x12CBF9D..0x12CBFBA` sets capacity `0x800`, data pointer `header+0x10`,
and zero displacement/length.

At `0x12E4739..0x12E4767`, the decrypt call receives backing data pointer,
virtual logical length, and fresh empty destination. Copy instructions
`0x12EF13A..0x12EF144` establish pointer/count ordering. Reading backing
`+4` as a length would invert those arguments.

Success `0x12E4776..0x12E477E` publishes by reference-counted pair
assignment, **not swap**: `0x12CC439..0x12CC44F` acquires incoming
ownership, and `0x12CBECA..0x12CBED0` replaces object `+8/+4`, releasing
prior ownership. The destination object's alignment remains unchanged.
Then `0x12E4783..0x12E47A3` checks nonzero alignment, nonnull header, and
`unsigned(dataPointer) % alignment != 0`. Only if misaligned does it
preserve-resize to the **same logical length**. Resize rejects misaligned
reuse at `0x12CBD03..0x12CBD13`, allocates using object alignment at
`0x12CBD9D..0x12CBDA6`, preserves bytes at `0x12CBDBC..0x12CBDD3`,
and restores the same length at `0x12CBDFD`.

This is publication followed by optional realignment, not padding removal
or cipher-block length normalization. Decrypt failure bypasses publication.
Allocation exceptions, generic aliasing, and overflow behavior remain
unproven.

## Logout cleanup: established effects and remaining indirect edge

The bounded GUI-logout trace found more than signing reset, but no concrete
ClientApi cancellation boundary. Call `0x63A9C3 -> 0x490090` destroys/nulls
connection field `+0x820` and sets connection state `+0xC=1`.
Virtual destructor call `0x63A9D3` was reported as reaching
`0x48F290 -> 0x48F360`, using original-file vtable evidence; that target
needs active-sample verification before treating the mapping as definitive.
Within that candidate destructor, `0x48F43C -> 0x308500(1)` clears
application tree `+0xA50` and count `+0xA54`; these fields are **not**
established as ClientApi request storage. Call `0x63F61D -> 0x30D790`
performs the mode transition/enqueue.

Requests that subsequently reach `0xBC1990` face the established readiness
rejection. Already-prepared/in-flight requests remain unresolved. In
particular, callback `0x48F41C` and indirect owned-object cleanup prevent a
global "logout never cancels requests" conclusion. Follow-on work is scoped
to that callback/active-sample mapping and to one concrete caller's handling
of preparation failure.

## Logout callback verified against the active sample

The candidate destructor mapping above is no longer dependent solely on
`.original`: the relevant constructor, vtable, callback, and callee bytes
match the active sample, although the two files differ globally. Callback
raw `0x48F41C` resolves to raw `0xBF9D80`, VA `0xFFA980`.
Initializer `0x3EE8CA` uses wrapper VA `0x1E002A8`;
`0x3E4573` constructs `0xBF7A80`, and `0x3E457C` stores its result
at wrapper `+0x10`, the uiManager global VA `0x1E002B8`.
Constructor `0xBF7ABB` installs vtable VA `0x1C73EBC`; slot `+0xAC`
at raw `0x1872B68` contains VA `0xFFA980`.

Logout supplies event arguments `(83,0,0)`. Dispatcher `0xBF9D80`
checks receiver byte `+0x672` at `0xBF9DBA`. If enabled, it uses event
container `+0x6F0`; the null-payload branch selects shared payload at
`0xBF9E21`. Call `0xBF9F2D -> 0xBF9C40` iterates recipients, invoking
`0xC38400` at `0xBF9D5B`. This establishes a gated event notification,
not cancellation. Registered event-83 handlers remain the next boundary.

## Login ignores failed signing preparation before manager submission

One concrete caller contradicts a stronger reading of "readiness rejection":
`0xBC1990` rejects **signing preparation**, not necessarily request
admission. `Login_QueueLoginRequest` at raw `0x63B550` allocates
`0x78` bytes at `0x63B5D9`, constructs the request at `0x63B5FE`
(URL parsing at `0x384487`), sets type `2` at `0x63B60D`, and copies
callback pair `+0x50/+0x54` at `0x63B653..0x63B659`.

Call `0x63B65C -> 0x636550` first prepares request collection `+0x20`
at `0x636571..0x636574`, then loads app global VA `0x1DFFD98`,
reads app `+0xA28`, and tail-jumps to `0xBC1990`
(`0x636579..0x6365A9`). The caller does **not** test its returned
`AL`; it immediately submits the request at `0x63B663`.

Submission `0x6499F0 -> 0xBC43C0` instead checks manager byte
`+0x21` enabled, `+0x20` clear, and validation `0xBC1380`.
Accepted requests use a per-route tracker, pass through `0xBC3F40`,
and receive timestamp field `+0x6C`. Manager success makes login set
in-flight byte `+0x9E0` and option `+0x9E2`
(`0x63B668..0x63B69F`). Manager failure, not signing failure, instead
destroys the request via slot zero with flag one, logs, and returns zero
(`0x63B66C..0x63B68F`).

Thus a request may enter tracker storage after signature construction was
skipped. This does not establish immediate WinHTTP handle creation, actual
network transmission, or a feasible concurrent logout/login interleaving.
The next trace follows one tracker worker and any later readiness/header
gate. Earlier references to queued requests encountering `0xBC1990`
must not be read as proof of end-to-end admission failure.

## Event 83 registration remains dynamic; delivery queues callbacks

The bounded recipient trace did not identify a concrete event-83 handler.
`0xC38438..0xC38446` requires the event entry in listener collection
`+0x168`; `0xC3847C` queues its callback record/payload at listener
`+0x4C4`, and `0xC384D3` inserts the listener into manager `+0x5EC`.
Zero name filter bypasses recipient-name matching at the dispatcher.
Registration paths include XML `0xC35AD8` and script `0xC35CF0`.
Name IDs derive from first-seen-name count at `0xBF9AE6..0xBF9AFE`;
recovering numeric 83 therefore needs registration order, not a string
guess. No concrete cancellation or request effect was established. The
next independent question is callback queue draining/lifetime, not another
unbounded search for a guessed logout event name.

## Admitted login request can reach WinHTTP construction conditionally

The tracker trace extends the ignored signing-preparation result to an
actual static network-construction path. Code addresses below are raw
offsets in the active sample; API operand addresses, where used, are PE
VAs, not raw IDB addresses.

`0xBC3F40` clears request `+0x64` at `0xBC3F6C` and enqueues it
through `0x12CE310` with tracker `+0x10` at `0xBC3F73`.
Enqueue increments queue `+0x10` at `0x12CE32A`, so tracker `+0x20`
is the pending queue count. It stores the request in node `+4` at
`0x12CE3E5`, then publishes the next-node pointer at `0x12CE3ED`.

Pump `0xBC3F80` calls existing workers at `0xBC3FCB`. Its growth branch
requires positive pending count, completion count at least worker count,
and worker count below manager `+0x24` (`0xBC408C..0xBC40AC`).
Construction at `0xBC4101` supplies tracker `+0x10`; worker stores the
queue at `+0` at `0xBC27CE`. Constructor call
`0xBC2846 -> 0xBC38F0` reaches `WinHttpConnect` at `0xBC392A`
and stores its result in worker `+0x20`.

Worker `0xBC2B30` requires a nonempty queue and completion `+0x1C=0`,
then dequeues at `0xBC2BBC`. Helper `0x12CE400` decrements queue count
at `0x12CE47D`, extracts node `+4` at `0x12CE485`, and recycles the
queue node, not the request. Response `+4` receives the request at
`0xBC2C77`; callback context `+0x18` receives the response at
`0xBC2D47`. A future retry deadline instead requeues at `0xBC2BF0`.
Otherwise, nonzero worker connection handle (`0xBC2D7A`) permits
`WinHttpOpenRequest` at `0xBC2DB7`; type 2 selects UTF-16 `POST`.

No signing-context readiness or named-signature-header requirement was
found along this selected route before request construction. Generic header
serialization follows construction and is skipped when request `+0x24=0`
at `0xBC2F65`. Setup-error context `+0x14` gates the eventual
`WinHttpSendRequest` branch at `0xBC31D8`, call `0xBC3213`.
This is conditional static reachability, not observed traffic, successful
handle creation, absent/preexisting signature contents, server acceptance,
or exploitability. Next work follows failed-handle/setup-error completion
and ownership after dequeue.

## UI event delivery is deferred and retains payload ownership

`0xC38400` does not invoke registered callback code inline.
It obtains the registration pointer at `0xC3844C`, copies/retains the
payload pair through `0xC22760` at `0xC38466` (atomic control-block
`+4` increment at `0xC2277F`), then queues through `0xC42180`.
That helper allocates/reuses a 12-byte slot, copies the registration pointer
unchanged at `0xC421DF..0xC421E1`, and retains payload ownership at
`0xC421F7`. The caller releases its temporary pair at
`0xC38488..0xC384B1`. The queue owns a payload reference but only
borrows the registration-record pointer; record lifetime across unregister
or replacement is unresolved.

Scheduling at `0xC384D3` inserts the listener into manager `+0x5EC`;
`0xC53B90` suppresses duplicate listener-pointer entries.
Manager routine raw `0xC02730` is vtable slot `+0x40` (raw table
entry `0x1872AFC`, value VA `0x1003330`).
It requires manager `+0x672 != 0` at `0xC0276B..0xC02772`,
snapshots scheduled listeners at `0xC034B0..0xC034CF`, clears the
scheduling collection at `0xC034D7..0xC0353F`, and calls listener
drain `0xC38CA0` at `0xC0357B`.

Drain loads registration fields `+0/+8` and payload at
`0xC38D38..0xC38D59`, calling `0xC3B3C0` at `0xC38D5C`.
That requires listener `+0x7C` nonzero, context `+0x78` nonnull,
and nonempty callback text (`0xC3B40F..0xC3B42F`). Its path through
`0xC289C0`, `0xCD7890`, `0xCD6C10`, thunk `0xC28940`, and
`0xCD7110` ends at unresolved runtime-selected `call edi` at
`0xCD728C`. No concrete script/event-83 identity is established.

After the invocation attempt, `0xC38D88..0xC38DA6` decrements payload
ownership counters and conditionally calls virtual cleanup; then
`0xC38DA9..0xC38DB7` removes the front queue entry. Continuation
requires processed count at most manager `+0x50` and a nonempty queue
(`0xC38DBB..0xC38DC8`). Listener destruction instead calls
`0xC422C0` at `0xC33ABE`, releasing pending payloads and freeing queue
storage without invoking callbacks.

Delivery thus needs a subsequent manager-processing pass. Its frame-call
provenance, execution success, registration lifetime, and concrete callback
target remain unresolved; none establishes request cancellation.

## Failed HTTP construction: retry versus terminal completion

All code addresses here are raw offsets. API operands `0x1AC4CFC`
(`WinHttpOpenRequest`) and `0x1AC4CF8` (`WinHttpCloseHandle`) are
PE IAT VAs; names follow the existing subsystem documentation.
After a null open result, `test esi,esi` at `0xBC2DE4` branches to
`0xBC3236`. Context `+0` remains its initialized zero.
At `0xBC3264`, context `+0x14` receives `0xBC1C40`'s normalized
negative error: positive codes become `0x8007xxxx`, and other nonnegative
results become `0x80004005`. Context `+0x18` receives the response at
`0xBC3270`, response `+8=4` at `0xBC3273`, and worker `+0x1C`
receives context at `0xBC3280`.

A nonzero setup error at `0xBC31D8` instead branches to `0xBC321B`,
closes/zeros context handle `+0`, and joins the same response-kind-4
completion setup. In the same worker call, the dequeue loop exits on
nonzero completion (`0xBC2BB0`). `0xBC32A4..0xBC32BE` processes
only an existing context whose handle is zero and clears worker `+0x1C`;
negative context error leads to logging via `0xBC33CD`.

Retry guard `0xBC358F` requires request counter `+0x3E` below limit
`+0x3D` and `0xBC1130(response)` true. That helper accepts response
kinds 2 and 4, so the failed-open response satisfies its kind predicate,
but retry still depends on the counter. Retry increments `+0x3E`,
sets request `+0x60=1`, and writes a backoff deadline at `+0x48/+0x4C`.
Call `0xBC3612 -> 0x12CE310` puts the same request back on the worker's
queue. If the count has now reached the limit, the path closes/zeros
worker connection `+0x20` and calls reconnect helper `0xBC38F0`.
It clears response `+4` at `0xBC367B`, returns the response to the
worker reuse list through `0xBC5250`, and frees context through
`0x12C8D60`, without invoking the request callback.

When the counter is already at the limit (`0xBC3595`), terminal completion
records a tick at request `+0x74` (`0xBC37D5`), acquires the callback
at `0xBC37E2`, and calls its slot `+4(response)` at `0xBC37FC`.
Request slot zero with flag one follows at `0xBC380C`. Constructor
`0x3843F0` installs vtable VA `0x1C60A78` (raw `0x185F678`);
its slot zero points to VA `0x723D00`, raw `0x323100`, which runs
destructor `0x322FF0` and frees via `0x12C8D60`. The terminal path
also clears response `+4`, returns response to the reuse list
(`0xBC3866`), frees context (`0xBC386A`), and releases the callback pair.
Thus responses are recycled, contexts freed, and requests either requeued
or destroyed after terminal callback. Login callback identity and its
response-kind-4 handling are the next bounded check.

## Login response callback is weakly acquired and translates kind 4

The concrete callback wrapper is built at raw `0x636743..0x63679D`,
size `0x20`, owner `+4=tfLogin`, target `+0x10=VA 0xA3C3C0`,
adjustment `+0x14=0`. RTTI descriptor raw `0x1A10C78` (name
`0x1A10C80`) identifies
`sl::member_callback<tfLogin, HTTP::Client::Response const &>`.
Vtable VA `0x1C674A8`, raw `0x18660A8`, slot `+4` contains
VA `0xA43F80`, raw `0x643380`. Its trampoline preserves the response
argument and tail-jumps to raw `0x63B7C0`.

Control-block setup `0x6456FA..0x64570E` initializes counters
`+4/+8=1`, wrapper at `+0xC`; `0x645744..0x645747` installs the
Login-owned pair at `+0x9D8/+0x9DC`. Request preparation increments
control-block `+8` at `0x63B632` before copying the pair at
`0x63B653/0x63B659`: this request reference is weak, not ownership of
Login. Terminal completion `0xBC37E2 -> 0xBC4A80 -> 0x327730`
acquires only while strong count `+4` remains nonzero.
Null/expired callbacks skip invocation at `0xBC37F4..0xBC37F6`;
request destruction still follows. Acquired-pair release is at
`0xBC3876..0xBC38A2`.

Response-kind switch `0x63B864..0x63B88B`, table raw `0x63C5C4`,
maps kind 4 to `0x63B8A7`, loading `WinHTTP Error` (raw text
`0x17B3EE0`). Formatting at `0x63B8B3..0x63B8C0` produces
`Connection failure: type=WinHTTP Error`. The non-HTTP/error flag
skips body-error parsing at `0x63BF4D..0x63BF54`; metadata gets
`error`, request `url`, and `status=0` at `0x63BF32..0x63BF48`.
The path dispatches event `0x145` at `0x63C1EE..0x63C1F5`, clears
Login `+0x9E0` at `0x63C20D` and `+0x9E1` at `0x63C405`.
Nonempty error prevents state `+0xA48` advancing from 1 to 2.

If option `+0x9E2` is set, the path decrements `+0x9E4`; a positive
remainder waits 500 ms then calls the auto-login entry
(`0x63C460..0x63C480`), otherwise it calls `0x63E1E0`.
Finally it clears `+0x9E2` at `0x63C4C6`. Actual callback execution,
UI interpretation, and retry eligibility after logout remain unproven;
the weak acquisition is a concrete lifetime gate, not evidence that every
logout has already expired the callback.

## Registration removal: record release without local queue invalidation

The worker corrected its initial VA label: all code addresses in this
trace are raw-file/rawbasezero IDB offsets, not runtime VAs. PE metadata
gives image base `0x400000`, `.text` RVA `0x1000`, raw start `0x400`;
raw `0xC35D10` has prologue `55 8B EC 83 EC 10` and maps to
VA `0x1036910`.

Script removal entry `0xC32680` calls `0xC35D10` at `0xC32764`.
The helper resolves the dynamic ID through manager `+0x6E8`, rejects
`-1` at `0xC35D55..0xC35D58`, and exits if listener registration
collection `+0x168` lacks an entry (`0xC35D62..0xC35D76`).
For an existing entry it unsubscribes through `0xBF9BD0` at
`0xC35D80`, destroys the record at `0xC35D8E`, frees through
`0x12C8D60` at `0xC35D95`, nulls the node's record pointer at
`0xC35D9E`, then erases the node at `0xC35DAC`.

Inspected unsubscribe `0xBF9BD0` only unlinks matching listener pointers
and frees manager `+0x6F0` subscriber-list nodes
(`0xBF9C00..0xBF9C27`); destructor `0xC33E60` releases record
text fields `+0/+8`. Neither inspected helper clears listener queue
`+0x4C4`, and no drain intervenes in this local removal path.
Replacement through `0xC35BB0` instead updates the existing record's
`+8` in place (`0xC35C29..0xC35C5D`).

Outstanding-queue reachability and script-call scheduling remain unresolved;
no runtime safety or vulnerability conclusion follows. Follow-on work
traces scheduling, not merely the presence of a borrowed pointer.

## Script unbind is immediate; callback execution precedes queue removal

The scheduling trace identifies the binding rather than assuming a
script API name: raw `0xC3AE3B..0xC3AE57` registers
`Component.UnbindEvent` with target VA `0x104E880`, raw `0xC4DC80`.
Its wrapper reaches `0xC32680`, which calls removal `0xC35D10`
immediately at `0xC32764`; no defer/isolated-phase guard was found on
this path. Drain invokes the VM synchronously at `0xC38D5C`;
protected thunk `0xC28940` enters `0xCD7110`, whose native dispatch
calls at `0xCD728C`. Queue removal at `0xC38DA9` follows callback
return. Thus native unbind is statically callable during callback
execution, not established as restricted to a separate phase. Concrete
script invocation, actual outstanding registrations, and runtime outcomes
remain unobserved. This does not establish a concurrency scenario.

## Signing reset alone does not disable callback-level login retry

Cleanup `0x63A9B0` cleans Login `+0x1C` and conditionally resets
signing at `0x63AA08`; the inspected path does not clear retry eligibility.
Auto-login entry `0x63DE80` resets signing again before selecting
credentials. Queue entry `0x63B550` checks in-flight byte `+0x9E0`
and endpoint availability, not a logout-state predicate on that path.
Mode replacement logic `0x30D850`, comparison at `0x30D93F`, skips
replacement when requested mode equals current mode; requesting Login
mode therefore does not necessarily destroy a current Login object.

Actual destruction releases callback control at
`0x636EC2..0x636EEC`; weak acquisition
`0xBC4A80 -> 0x327730` rejects zero strong count, but the inspected
control block demonstrably protects the callback wrapper, not independent
ownership of Login. A surviving eligible callback has a static retry
path after reset. Scheduler ordering/interleaving and concrete runtime
occurrence remain unproven. Next work follows credential selection and
whether signing is reestablished before queuing that retry.

## Bind replacement affects already-queued deliveries

Record constructor raw `0xC53E50` sets `{+0 event text, +4 event ID,
+8 callback text}`, retaining each nonnull shared text through its
`[text-4]` counter. Existing-record replacement at
`0xC35C29..0xC35C5D` releases old callback text `+8`, assigns/retains
the replacement, and preserves record identity.
Queue copy `0xC421DF..0xC421F7` stores only the record pointer and
retained payload pair, not a callback-text snapshot.
Drain `0xC38D38..0xC38D5C` rereads record `+0/+8` at delivery.
Therefore replacement before drain changes which callback text an already
queued delivery uses. Entry `0xC32460` identifies
`Component.BindEvent(event, function)`; callback strings remain dynamic,
not identified concrete script handlers.

## Auto-login restores readiness before its selected queue invocation

Raw `0x63DEAE -> 0x63A9B0` performs the previously described
readiness-gated reset. Credential selection then follows this order:

| Branch | Acquisition guard | Setup call and return guard |
| --- | --- | --- |
| Steam | Virtual `+0x0C` true at `0x63DED0/0x63DED2`, virtual `+0x1C` nonzero 64-bit ID at `0x63DEE5/0x63DEE7`, ticket from virtual `+0x24` succeeds at `0x63DF04/0x63DF06` | `0x63DF13 -> 0x12E3960`, selector 1; false at `0x63DF18/0x63DF1A` goes to branch failure |
| Youxi | Otherwise nonempty shared text at global VA `0x1E4B2DC`, copied through `0x2FCB30`, acquisition guarded at `0x63DF99/0x63DF9B` | `0x63DFA8 -> 0x12E3960`, selector 2; false at `0x63DFAD/0x63DFAF` fails |
| 369 | Otherwise global byte VA `0x1E4B2E1` nonzero; `0x2FD5A0` obtains text from global VA `0x1E4B2E4`, acquisition guarded at `0x63E017/0x63E019` | `0x63E026 -> 0x12E3960`, selector 3; false at `0x63E02B/0x63E02D` fails |
| Reboot/cache | Otherwise `0x635E00` succeeds; `0x63B700` reads `LoginName`, overridden by nonempty Login `+0xA10`; empty account rejected at `0x63E146/0x63E148` | `0x63E167 -> 0x63AEF0(account, empty, empty, 1)`; false at `0x63E16C/0x63E16E` leaves success unset |

A selected ticket branch's acquisition/setup failure does not fall through
to another provider. Provider-specific diagnostics identify these branch
names; nonempty error text reaches `0x63E0C1 -> 0x63E1E0`.

Ticket setup rejects null/empty input at `0x12E39B5/0x12E39B7` and
`0x12E39BD/0x12E39C0`. After derivation and wrapper initialization,
`0x12E3B3D` stores parent readiness `+0x34=1` and `0x12E3B41`
stores selector `+0x38`; the false path is `0x12E3B46`.
Cache setup `0x63AEF0` attempts file recovery; `0x12E4AC0` calls
`CryptUnprotectData`. Nonempty recovered secret reaches
`0x63B102 -> 0x12E3B80`, guarded at `0x63B107/0x63B109`;
`0x12E3D50` sets readiness and `0x12E3D56` selector zero.
This is a DPAPI-protected cached secret; specifically calling it a password
or token is not established.

Missing/empty recovered secret takes `0x63B1BE -> 0x12E3DA0`,
guarded at `0x63B1C3/0x63B1C5`. That setup requires nonempty account
and second text input; this auto-login path supplies empty second input,
so it reaches reset `0x12E40C5` and returns false. The setup's general
successful path would set readiness at `0x12E40B4`.

Successful setup sets `BL=1`; `0x63E1A1/0x63E1A3` gates
`0x63E1AD -> 0x63B550(1)`. Unlike the earlier generic ignored-prep
case, this selected auto-login route restores readiness before queue
invocation. It nevertheless ignores the queue call's return and sets
success at `0x63E1B2`: that is not proof of manager admission or login
success. Follow-on work traces its caller-visible result/state.

## Null event payload produces a fresh callback table with event text

The shared default at **PE VA** `0x1E68660` is a zero-initialized
`{object,control}` pair in `.data`'s virtual-only tail, not an allocated
payload. No constructor/writer xrefs were found within the bounded trace.
Raw file offset `0x1E68660` is unrelated; its bytes must not be interpreted
as this global's initializer.

Raw `0xBF9E21` copies the default pair. Retain `0xC2277F` acts only
on nonnull controls, as does the queued copy at `0xC421F7`; the zero
default therefore retains nothing. Conversion at
`0xC25E89 -> 0xCD4550` allocates a fresh table per callback, and
`0xC25F43..0xC25F95` inserts `"event"` with the registration's
event text. Thus the default callback argument is not nil or empty.
Table mutations are not shared across recipients through this default:
there is no shared default payload object. Event 83's text is still
unresolved; arbitrary nonnull payload conversion is a separate next trace.

## Auto-login return reports setup/attempt, not manager acceptance

The bounded queue-result report confirms that submission failure
(`0x63B668..0x63B66A`) destroys the request at `0x63B672`, logs at
`0x63B687`, returns zero, and bypasses the in-flight/retry-option
writes at `0x63B698/0x63B69F`. Auto-login ignores that result at
`0x63E1AD`, sets `BL=1` at `0x63E1B2`, and returns it in `AL`
at `0x63E10E` or `0x63E1C5`; shared-text cleanup preserves it.

Update routine raw `0x6385C0` is a direct caller gated by
Login `+0xA4C` nonzero (`0x638BCD`), parent readiness false
(`0x638BEE..0x638BFD`), global flag **VA** `0x1E4B0B0` clear
(`0x638BFF`), and helper `0x63DDC0` true (`0x638C07..0x638C0E`).
A returned one branches at `0x638C19`, bypassing error dispatch
`0x638C35`. The worker corrected its initial VA label: code addresses
are raw offsets. Bytes `38 1D B0 B0 E4 01` at raw `0x638BFF`
independently establish that `0x1E4B0B0` is an absolute PE VA operand.
No new callback retry is armed by failed submission: `+0x9E2` remains
unchanged and no newly in-flight request is established. Later update
eligibility cannot be inferred merely from unchanged `+0xA4C`, because
the successful setup also restored readiness, another update guard.
The next task traces that guard and helper explicitly.

## Successful preparation blocks later update attempts after queue rejection

The update eligibility helper raw `0x63DDC0` returns true for any of:
the object at **VA** `0x1E46774` has virtual `+0x0C` true and virtual
`+0x1C` nonzero 64-bit result (`0x63DDC0..0x63DDE0`); shared text
at **VA** `0x1E4B2DC` has nonzero length
(`0x63DDE2..0x63DDFD`); or byte at **VA** `0x1E4B2E1` is
nonzero (`0x63DDFF..0x63DE0D`). Shared-text length uses DWORD
`[p-0xC]` when WORD `[p-8]==0`, otherwise unsigned WORD `[p-6]`.

Login constructor clears its `+0xA4C` at `0x63691A`, then calls
`0x63DE10` at `0x636AB0`. That helper enables only if Login
`+0xA4C==0`, application byte `+0xA45!=0`, and application DWORD
`+0xA4C!=0`, using app global **VA** `0x1DFFD98`.
It sets Login `+0xA4C=1` at `0x63DE39` before testing parent
readiness. If readiness is false, `0x63DE6A` tail-calls auto-login
without the update caller's eligibility/global guards. No subsequent
explicit clear of Login `+0xA4C` was found in the inspected displacement
references; this is a bounded search result.

Successful credential setup sets readiness at `0x12E3B3D` before queue
submission. Submission rejection does not roll that back. Later updates
exit at readiness guard `0x638BFD`, so they do not reach the setup/reset
entry again even if Login `+0xA4C` remains set. The update auto branch
also requires cached Login `+0x1C==0` at `0x6388D2`.
External resets can re-enable attempts. No fixed retry timer or update
frequency was established; the conditional update path is not a proven
repeated-attempt loop.

## Nonnull event payload layout and VM conversion

This is an in-memory x86 layout, not established wire framing. All code
addresses below are raw offsets in the identified active sample.
Converter `0xC25E30` reads payload `{+0 begin, +4 end}` directly,
iterating eight-byte `{name_pointer,value_pointer}` entries
(`0xC25EA6..0xC25EB3`, `0xC25F0C..0xC25F28`), not a payload
vtable. Nonempty names use explicit shared-text header lengths as above;
null/empty names receive integer keys 1, 2, ... counting only unnamed
entries (`0xC25EF7..0xC25F08`).

Value pointer `+0` selects tags 0 through 8 via table raw `0xC25AB8`,
whose entries are PE VAs. Null value pointers take a diagnostic/assertion
path and are still dereferenced if it returns; they are not ordinary null
encodings. The following are observed internal VM tags, not assumed
version-specific Lua constants.

| Input tag | Raw target | Observed conversion |
| --- | --- | --- |
| 0 | `0xC258DD` | String pointer `value+4`, or fallback VA `0x1C5EA68`; NUL-terminated push through `0xC27FE0` |
| 1 | `0xC25906` | Copy DWORD at `value+0x10`, VM tag 19; no floating conversion |
| 2 | `0xC25930` | Widen binary32 at `value+0x14` to binary64 with `CVTPS2PD`, VM tag 3 |
| 3 | `0xC2594E` | Normalize byte `value+0x18` to 0/1, VM tag 1 |
| 4 | `0xC2597D` | QWORD at `value+8` through object-registry lookup `0xC54160`; wrapper or VM null, not numeric conversion |
| 5 | `0xC259A2` | QWORD at `value+8` through `0xC549A0`; zero gives VM tag 0, otherwise userdata tag 71/subtype 1, exact QWORD at userdata `+0x20` |
| 6 | `0xC259C7` | Recurse through `0xC25E30(vm,[value+0x1C],0)` |
| 7 | `0xC259EB` | Stream from `value+0x20`, parsed through `0xC25790`, forwarding normalized byte `+0x18`; format/result unresolved |
| 8 | `0xC25A6C` | QWORD conversion through `0xC548C0`, like tag 5 but userdata subtype 2 |
| >8 | `0xC25A91` | VM tag 0 |

Fresh table allocation `0xCD4550` is pushed with VM tag 69 at
`0xC25E91..0xC25E9A`. Payload entries are inserted first.
If event text is nonnull, the subsequent reserved `"event"` insertion
at `0xC25F43..0xC25F95` uses `0xC285C0(vm,-3)`, overwriting
the entire existing 16-byte value and popping key/value
(`0xC285D6..0xC285F7`, `0xC2862B`). Short strings are
content-interned by `0xCD4E50`; thus reserved event text wins over a
payload `"event"` entry. With null event text this insertion is skipped.
Duplicate payload keys are last-write-wins. No runtime samples or malformed
payload execution were used; recursive/cyclic bounds and tag-7 parsing
remain separate unresolved questions.

## Submission rejection gates do not establish normal-startup failure

Wrapper raw `0x6499F0` gets manager from
`*(global at VA 0x1DFFF70 + 0xA0)`. Submission `0xBC43C0`
returns zero before route lookup/enqueue for manager `+0x21==0`
(`0xBC440E..0xBC4412`), manager `+0x20!=0`
(`0xBC4418..0xBC441C`), or validation `0xBC1380` false at
`0xBC4424`. Accepted linking is at `0xBC4503`.

Validation calls URL predicate `0xBCDF20(request+0xC)`. It requires
nonnull/nonempty host at request `+0x10`
(`0xBCDF20..0xBCDF39`), signed port `+0x14` in 1..65535
(`0xBCDF3B..0xBCDF47`), and nonempty path `+0x18` whose first byte
is `/` (`0xBCDF4F..0xBCDF63`). Content-type text at `+0x28`
and body logical length must agree on emptiness. Body length comes from
request stream `+0x2C`, slot `+0xC`, at
`0xBC13B4/0xBC13DC`; slBlob slot maps to `0x12CB9C0`.
These validators do not check method `+8`, scheme `+0xC`, or a literal
MIME value. Type 2 alone is not a rejection predicate.

Manager constructor `0xBC4190` clears both flag bytes at `0xBC4222`
(`66 C7 46 20 00 00`). Initializer `0xBC39B0` enables `+0x21`
at `0xBC3AB2` only after successful `WinHttpOpen`.
Global setup `0xBBD110` constructs at `0xBBD37B`, publishes at
`0xBBD38D`, initializes at `0xBBD393`, and succeeds only if
initialization succeeds. Normal app startup call `0x307208` takes a
failure branch on zero rather than continuing to mode construction.
Manager `+0x20` is set as drain-in-progress at `0xBC4673` in
`0xBC4620`, with observed caller application shutdown `0x3097B7`;
it is cleared after drain completion/timeout.

Neither flag establishes a normal-startup constructor rejection: enable is
already set after successful startup and drain begins clear. Validation
failure depends on actual request fields, not just auto-login invocation.
Malformed URL/body or shutdown-reentrant constructor reachability was not
established. The ignored submission result remains a precise code fact,
not a demonstrated startup failure.

## Tag 7 is permissive JSON-like text parsing, not binary framing

Raw `0xC259EB..0xC25A14` wraps the stream at value `+0x20`;
byte `+0x18!=0` enables extensions. Parser wrapper
`0xC257C5..0xC2581D` obtains stream length, appends a NUL if empty
or unterminated, then supplies adjusted length minus one to its parser.
There is no length prefix. It parses one value without checking exhaustion
of trailing input.

Dispatcher table raw `0xC256F8`, selector `0xC2570C`, handles
NUL as nil, either quote byte (`0x22/0x27`) as string,
`[` as array, `{` as object, and other input as scalar token.
Whitespace and commas (`0x20/0x09/0x0D/0x0A/0x2C`) are skipped;
commas are not strictly enforced. Object keys require colon and collections
use `]`/`}`; nesting limit is 32 (helpers `0x12E6250`,
`0x322F50`, `0x384040`).

String parser `0x12E6841..0x12E69DD` accepts both quote styles,
copies unescaped bytes without UTF-8 validation, and supports
`b/f/n/r/t/v`, octal-like escapes, `x` plus two hex digits, and `u`
plus four. Hex/Unicode escapes use UTF-8-style encoder `0x12CFEA0`;
surrogate-pair combination was not shown. Reaching NUL without a closing
quote is accepted by the inspected loop
(`0x12E6822..0x12E69F2`).

Bare tokens admit alphanumerics and `_ + - .`, with buffer at most 255
characters; exact `true/false/null` literals were verified in raw rdata.
Numeric-looking tokens become float32-rounded VM numbers only if absolute
converted value is strictly below 4,194,304
(`0xC2567F..0xC256C6`); larger values remain strings. Full-token
numeric validation was not established. Extension mode enables `0x...`,
`sdb`, and `mat` branches; some invalid/unresolved extension values yield
successful nil.

Normal return is one with a 16-byte VM value: nil tag 0, Boolean 1,
number 3, table `0x45`, and one-based integer array keys tag `0x13`.
EH metadata verifies exception handler `0xC25837`: it logs
`JSON error: %s`, resumes at `0xC25856`, and returns zero.
Caller `0xC25A3D..0xC25A4B` then appends nil; rollback was not
visible in this bounded trace and is the next explicit stack-contract
check. Empty input fails, whereas embedded NUL can select successful nil.
Malformed input is not uniformly failure, and failure-nil must remain
distinct from successful nil.

## Application shutdown drain pumps; timeout does not itself cancel

Raw `0x3097B7` passes 3000 ms to `0xBC4620`, ignores its Boolean
result, then calls `0xBBD570(3000)` at `0x3097C7`. This is application
shutdown, not GUI logout `0x63F5C0`.
Drain sets manager `+0x20=1` at `0xBC4673`, blocking new submissions.
After sampling `timeGetTime` at `0xBC467E`, each iteration aggregates
route pending count `+0x20` and workers with nonzero completion pointer
`+0x1C` (`0xBC46AD..0xBC46D8`).
Empty route tree (`0xBC46A1`) or aggregate zero (`0xBC4720`)
returns one and clears the drain flag at `0xBC476C`: accounting drained,
not proof that HTTP operations succeeded.

Nonzero aggregate calls `0xBC4728 -> 0xBBDB60`, reaching route pump
at `0xBBDD98`. It then calls IAT **VA** `0x1AC4188` at
`0xBC472D`; zero triggers `Sleep(1)` at `0xBC4736`. Import identity
of the former remains unresolved pending direct PE import parsing.
Negative timeout loops indefinitely (`0xBC4741`); otherwise unsigned
32-bit elapsed ticks must be strictly less than timeout to continue
(`0xBC474F`). Equality expires, returns zero, and clears the flag at
`0xBC475A`. There is no recount after pumping and before this deadline
test, so work finishing in that pump can still yield timeout return zero.

Drain itself neither dequeues/destroys requests nor closes HTTP handles
on exit. Normal worker completion follows the previously traced callbacks,
request destruction, response recycling, and context free.
On shutdown's final-release arm, `0xBBD602 -> 0xBC4280` destroys the
manager, then its global field is zeroed at `0xBBD60E`.
Manager destructor closes session handle at `0xBC42BB`. Route
destructor `0xBC3B80` tears down workers and dequeues pending requests,
calling indirect deleting-destructor slot zero at
`0xBC3C1A..0xBC3C29`, without visible ordinary completion callbacks.
Worker teardown marks active context `+0x11=1` at `0xBC28CB`,
closes connection `+0x20` at `0xBC28D7`, and destroys queued
requests/pooled responses.

Detached context/response destruction belongs to the previously traced
HANDLE_CLOSING callback. No direct close of completion request handle
`+0` was visible in this teardown scope; descendant-handle closure and
indirect destructor guarantees remain unresolved. Timeout therefore leaves
residual work to subsequent teardown, not an internal cancellation branch.

## Parser exception restores machine frame, not output-stack cursor

Raw `0xC257BE` saves machine ESP at `[ebp-0x10]`; this is distinct
from output cursor `[a1+8]`, advanced by 16 bytes by parser value writes.
Handler `0xC25837` logs and returns continuation **VA**
`0x1026456` at `0xC25850`, mapping to raw `0xC25856`.
There `xor eax,eax` produces status zero before common frame epilogue.
No output-cursor restoration was found in that handler.

Tag-7 caller `0xC25A14` consumes the EAX status, logs if greater than
one, and for nonpositive status appends a nil at the current output cursor
(`0xC25A3B..0xC25A49`). Positive status skips the fallback write.
Thus any values appended before an exception remain before fallback nil;
the nil is not rollback. The exact outer table insertion/stack effect of
a concrete partial parse remains the next bounded check. Machine-stack
exception unwinding must not be conflated with VM output-stack cleanup.

## Drain scheduling call is SwitchToThread, not a message pump

Direct PE import/OFT parsing resolves the previously unnamed drain call.
All three entries are named imports, not ordinals:

| DLL / import | Hint | OFT RVA / raw offset | IAT VA / raw offset |
| --- | --- | --- | --- |
| KERNEL32.dll / SwitchToThread | 1212 | `0x19BD1D4 / 0x19BBDD4` | `0x1AC4188 / 0x16C2D88` |
| KERNEL32.dll / Sleep | 1202 | `0x19BD3C0 / 0x19BBFC0` | `0x1AC4374 / 0x16C2F74` |
| WINMM.dll / timeGetTime | 148 | `0x19BDD70 / 0x19BC970` | `0x1AC4D24 / 0x16C3924` |

Raw `0xBC472D` (PE VA `0xFC532D`) has bytes
`FF 55 F0 85 C0 75 05 6A 01 FF 55 EC`: the saved SwitchToThread
pointer is called, its nonzero result skips Sleep, and zero calls Sleep(1).
This is thread scheduling, not application-message pumping. The distinct
preceding `0xBBDB60` call is the established route-pump path.

## Partial tag-7 failure can redirect the immediate table insertion

A concrete static path refines the previous "extra values plus fallback
nil" finding. Consider one payload entry with key `K`, tag-7 text bytes
`5B 22 5C 75 00` (array opening, quote, backslash, `u`, NUL), and no
event-text insertion (`a3=0`). This is a hypothetical byte fixture traced
through instructions, not an executed sample.

Array parsing pushes table `A` at raw `0xC252BE..0xC252CC`,
rejects the immediate closing-bracket probe at
`0xC252E0..0xC252ED`, then pushes index 1 and recurses at
`0xC252EF..0xC25313`. Quoted parsing consumes its opening quote
at `0x12E67DC..0x12E67E3`, finds insufficient bytes for the Unicode
escape at `0x12E6838..0x12E686D`, and constructs/throws at
`0x12E6AF4..0x12E6B3B`, before pushing a string value.
The exception bypasses normal array insertion and the two-slot removal
at `0xC25318..0xC25333`.

Let VM top before outer conversion be `T`; each slot is `0x10` bytes:

| Point | Stack suffix | Top |
| --- | --- | --- |
| Outer table push `0xC25E91..0xC25E9A` | `O` | `T+0x10` |
| Outer key push | `O,K` | `T+0x20` |
| Partial array push | `O,K,A` | `T+0x30` |
| Array index push | `O,K,A,1` | `T+0x40` |
| Throw/catch returning status zero | unchanged | `T+0x40` |
| Tag-7 fallback nil `0xC25A41..0xC25A4B` | `O,K,A,1,nil` | `T+0x50` |
| Outer insertion returns | `O,K,A` | `T+0x30` |

Outer `0xC25F15` supplies index -3. Resolver
`0xC27730..0xC27736` uses the current top, making `A` the target;
`0xC285CA..0xC285D6` selects integer key 1, and
`0xC285EF..0xC285F7` selects nil value. Cleanup at `0xC2862B`
pops exactly two slots. This performs `A[1]=nil`, not `O[K]=nil`
or `O[K]=A`; the outer key remains stranded.

With one entry and `a3=0`, local conversion returns success with three
slots `O,K,A` instead of one. Its epilogue and immediate wrapper
`0xC25FB0..0xC25FC8` do not restore VM top. This is bounded to the
established catch behavior and normal insertion return. Higher callback
argument construction/protected-call restoration is the next explicit
check; no observed runtime outcome or end-to-end vulnerability is claimed.

## Final network release destroys the manager before its timed pump

The bounded final-release trace corrects any implication that the
`BBD570(3000)` timeout gates manager destruction: the final-release arm
destroys/frees the manager and clears network owner `+0xA0` **before**
its timed tracking-vector pump. The worker corrected its initial label:
all code addresses in this trace are raw offsets. Instruction raw
`0x307200` encodes global operand VA `0x1DFFF70`, not a raw offset.

Network count is owner `+8`, reached through global **VA** `0x1DFFF70`.
First acquisition sets owner-thread ID `+0xBC` and count one.
Subsequent acquisition uses an ordinary non-atomic increment; acquisition
and release assert current-thread ID equals the owner rather than guarding
the count with a mutex. Zero count release returns; count greater than one
decrements and returns without teardown. At exactly one, teardown runs
while count remains one, then stores zero at the end. The release function
does not return a success/failure Boolean.

After manager destruction it pumps while tracking vector `+0xA4/+0xA8`
is nonempty, sleeping 10 ms between attempts while unsigned elapsed ticks
are at most the supplied 3000 ms. Expiration proceeds to cleanup, not
failure; pump/sleep duration is not a strict wall-clock bound.
The subsequent thread helper receives timeout zero, which it translates
to INFINITE for WaitForSingleObject. Thus 3000 ms does not bound the join.

Normal startup acquires once with flag one; another observed direct caller
acquires only when count was zero. This supports an ordinary single-ref
path, not an unconditional guarantee: app shutdown does not force/check
count one before release, and only the final-reference arm tears down.
The next task identifies tracking-vector elements and removal after manager
destruction.

Exact final-release sites: first acquisition sets owner ID at `0xBBD1BC`
and count one at `0xBBD1C2`; later acquisition increments at `0xBBD1AE`.
Release zero check is `0xBBD578`; decrement-only path
`0xBBD5AD..0xBBD5BA`; final zero store `0xBBD6E0`.
Manager destruction is `0xBBD602 -> 0xBC4280`, global-owner `+0xA0`
clear at `0xBBD60E`. Tracking pump call is `0xBBD642`, elapsed
comparison `0xBBD669`. Join `0xBBD670 -> 0x12BFF70(owner+0x48,0)`
maps zero to INFINITE at `0x12BFFC0`, calls WaitForSingleObject at
`0x12BFFC5`, and converges on handle/running cleanup at
`0x12C003B..0x12C0048`, including diagnostic paths.

## Enclosing callback restores top but does not repair partial-parse selection

The bounded higher-caller trace uses the stipulated one-entry fixture with
null event-text parameter (`a5=0`); the actual nonnull-event branch remains
the next check. Let entry VM top be `T0` and each slot occupy 16 bytes.
Lookup at raw `0xC3B46C` pushes handler `H` and nonnil result `F`.
Conversion at `0xC3B4C1` leaves `H,F,O,K,A` on the previously traced
failure path. Its Boolean success becomes argument count **one** at
`0xC3B4CD`, not the actual three-slot growth.

Protected call `0xC3B551` requests one result and uses saved-top-plus-one
as error-handler index (selecting `H`). Selection at `0xC289F1`
uses current top minus `16*(nargs+1)`, selecting `K` as the callable
with argument `A`, not intended `F(O)`.
On a returned success (`0xCD7370`) or protected failure
(`0xCD7890 -> 0xCD6B00`), top becomes K-slot plus 16,
leaving `H,F,O,result-or-error`. Cleanup
`0xC3B5BC -> 0xC278F3` resets top exactly to `T0`.

Thus returned-path cleanup bounds leftover live slots; it does not repair
the earlier callable selection. The runtime `__call` target for `K`
through `0xCD7080` remains unresolved; this trace does not establish
reaching native indirect `0xCD728C`, actual execution, or an end-to-end
vulnerability.

## Post-manager tracking vector owns socket handlers, not HTTP contexts

The network owner's `+0xA4/+0xA8/+0xAC` are pointer-vector
begin/end/capacity. Representative producer raw `0xBE1040` allocates
`0xA0` bytes, constructs via `0xBEC2E0`, and registers through
`0xBE10CE -> 0xBBD730`. Constructor logging identifies UDP with
label `SimpleSocketReplayReader`; the vector also includes Matrix game
UDP sockets. Append `0xBBD7E3 -> 0xBBF410` stores the pointer at
`0xBBF467` and advances end four bytes at `0xBBF469`.

Registration calls virtual `+8`, resolved to `0xBEC280`, storing network
back-pointer at socket `+0x3C`; it does not AddRef the socket.
Vtable **VA** `0x1C73D78`, raw `0x1872978`, slot `+0xC` resolves
to `0xBEC2B0`: nonnull `this` invokes deleting-destructor slot zero
with flag one. This is owning polymorphic deletion, not refcount release.

Reader destruction `0xBED1C0` clears socket owner `+0x40` and changes
open states to 4, requesting close through `0xBED0F0`.
Close completion `0xBED590` enqueues sentinel 1; pump virtual `+0x14`,
raw `0xBEDAA0`, consumes it, changes state 4 to 5, clears queues, and
returns false. Network pump destroys at `0xBBDD39`, compacts the vector
and decrements end at `0xBBDD55..0xBBDD5B`.

Post-manager pumping explicitly skips HTTP manager work if `+0xA0==0`
at `0xBBDD8E..0xBBDD98`, while socket handlers/network state remain.
This establishes structural separation, not complete callback-lifetime
safety: owner callbacks and virtual network close operations remain
indirect. Remaining-vector cleanup after timeout and ordering against the
infinite thread join are the next bounded questions.

## Actual event callback also redirects partial-parse insertion

The actual drain at raw `0xC38D38..0xC38D5C` calls
`0xC3B3C0(handler=record[8] or "", payload=entry[4], result=NULL,
eventtext=record[0] or "")`. Fallback is a verified NUL string;
event text is nonnull even when empty. It is forwarded into
`0xC25E30` at `0xC3B4B9..0xC3B4C1`.

On normal conversion the suffix is `H,F,O`, and reserved insertion
sets `O.event=eventtext`; the protected call selects `F(O)`.
On the established partial-parse fixture the suffix is `H,F,O,K,A`.
Reserved insertion pushes `"event"` and event text, then resolves
`-3` relative to current top; it sets **`A.event=eventtext`**, pops
those two slots, and leaves `H,F,O,K,A`. Conversion still reports true
and the caller supplies nargs one, selecting **`K(A)`**.

This path requires dispatcher enabled, VM/context present, nonempty handler,
nonnil callback lookup, and successful conversion stack reservation.
Protected returned paths still restore entry top through
`0xC3B5BC -> 0xC278F3`. The actual native target and runtime occurrence
remain unresolved; static wrong-selection bookkeeping alone is not an
end-to-end security claim.

## Residual socket deletion follows the worker join

Raw `0xBBD670 -> 0x12BFF70` first calls thread virtual `+8`,
resolved here to bare-return `0x2EB8C0`, sets network owner stop byte
`+0x4C=1`, and waits INFINITE. Returned join closes thread handle
`+0x54` and clears running byte `+0x4D`.
Only afterward does `0xBBD690..0xBBD6B0` invoke virtual `+0xC`
on each residual socket-vector entry; `0xBBD6B8` resets end to begin,
retaining capacity.

Representative deleting chain is `0xBEC2B0 -> slot0(1) ->
0xBEC440 -> 0xBEC480`. Call `0xBEC4FF -> 0xBE36B0` performs
`shutdown(socket,2)`, `closesocket`, and handle invalidation before
object cleanup/free. Then final release closes IOCP at `0xBBD6CA`
and calls WSACleanup at `0xBBD6DA`.

No explicit pre-join close-all was found in this path. Worker `0xBBDE6C`
can request socket virtual `+0x10(1)`, but universal pre-join closure is
unproven. A blocking-join conclusion cannot be inferred merely from that
ordering; the next task traces stop-byte reads, IOCP wait timeout, and wake
mechanisms.

## Extension dispatch: numeric handles and table lookup, format mapping pending

Raw `0xC25790` forwards its third-argument extension flag to
`0xC25190` at `0xC25822..0xC2582D`. Standard true/false/null
handling precedes the extension gate; false flag skips these branches.
The bounded extension report establishes:

- `0xC2548D..0xC254EE` tests case-sensitive prefix `0x`, scans one
  64-bit value initialized to zero, logs on scan failure but continues.
  `0xC549A0` yields userdata VM tag 71 for nonzero or nil tag 0 for zero.
- `0xC254F3..0xC25616` matches whole token `sdb`, then scans its next
  lexeme into a 128-byte text buffer and DWORD, requiring two conversions.
  Failed conversion or unknown table key in a 52-entry lookup yields nil
  (`0xC2557C..0xC25597`). A normal nonzero result yields userdata;
  zero yields nil. Key index 14 uses a separate string/node path.
- `0xC25621..0xC256D0` matches whole token `mat` and passes its next
  lexeme through `0xC27FE0`.

The worker read operands `0x1BBACAC/0x1BBACD0` as nontext without
establishing their raw mapping. Their format strings and the `mat`
helper interpretation remain withheld pending a focused VA-to-raw check;
the latter report conflicts with the earlier NUL-string-push identification.
Do not treat this partial report as exact extension delimiters or confirmed
material-object semantics.

## Extension formats resolved; mat is text, not a material wrapper

Direct PE mapping corrects the unresolved reads above. Image base is
`0x400000`, `.rdata` RVA/raw `0x16C4000/0x16C2C00`.
Format operand **VA** `0x1BBACAC` maps to raw `0x17B98AC`,
bytes `%127[^:]:%u\0`; **VA** `0x1BBACD0` maps to raw
`0x17B98D0`, bytes `0x%llx\0`. Both parser calls target imported
cdecl `sscanf`. Reading the operands as raw offsets caused the earlier
nontext result; it was not missing/dynamically decoded format data.

Case-sensitive `0x` scans a zero-initialized uint64, logs scan failure but
continues, and passes the result to `0xC549A0` for nil or userdata
tag `0x47`. Exact `sdb` consumes another lexeme, scans a table name
up to 127 bytes before colon and an unsigned ID, and requires two
conversions. A form such as `sdb "ItemType:123"` is needed because
the unquoted token alphabet excludes colon. The 52-name lookup is
case-sensitive; index 14 is `LocalizedString`, whose special output path
is the next bounded trace.

Exact `mat` consumes another lexeme and pushes its text through
`0xC27FE0` (ECX=VM, EDX=C string). That helper produces string tags
`0x44/0x54`, or nil for null. The previous material-derived-object
interpretation is superseded; no material lookup was established.

## IOCP worker observes stop through finite dequeue passes

Let network owner be `N`, embedded PumpThread `T=N+0x48`.
Join writes `T+4=N+0x4C`; worker reads the same byte at raw
`0xBBC878` and `0xBBC9CE`. Thread handle is `T+0xC=N+0x54`,
running byte `T+5=N+0x4D`.
Dequeue at `0xBBC911` calls GetQueuedCompletionStatus through
IAT **VA** `0x1AC432C`, raw `0x16C2F2C`, using IOCP from
`[global VA 0x1DFFF70]+0xDC`.

Each outer pass sets `EBX=500` at `0xBBC8B6` and initial timeout zero
at `0xBBC8DB` (pushed `0xBBC8F0`). A failed dequeue with null
OVERLAPPED enters `0xBBC949`: first occurrence schedules 500 ms and
clears EBX; another such failure exits the inner pass. Any dispatched
completion resets next timeout to zero at `0xBBC9BB`.
Inner passes cap at 100 dequeue attempts (`0xBBC9C1..0xBBC9C5`),
then reread stop at `0xBBC9CE`. Thus at most one 500-ms dequeue wait
occurs per pass, not 100 times 500 ms; a quiet worker needs no external
completion to observe stop.

PumpThread vtable **VA** `0x1C734D4`, slot `+8`, contains
**VA** `0x6EC4C0`, raw `0x2EB8C0`, a bare return. Join calls it
before setting stop (`0x12BFF99..0x12BFF9F`); it posts nothing.
A real post at `0xBBDA03..0xBBDA10`, after socket closure in
`0xBBD940`, sends `(port,0,key,NULL)` and dispatches key virtual
`+0x1C`: a close notification, not stop sentinel.

Finite waits establish a path to stop observation, not a universal bound
on the infinite join: maintenance takes critical section `N+0x6C` and
calls socket virtual `+0x10(1)` at `0xBBDE6C`; completion callbacks
can also delay progress. Intervening work returning and scheduling/visibility
remain assumptions. The next trace follows keyed-close ownership and
failed-post behavior, rather than claiming deadlock from the join alone.

## LocalizedString extension returns text through unresolved service slots

Raw `0xC255B5..0xC25603` handles lookup index 14 through singleton
virtual `+0xD0`, then returned-object virtual
`+0x108(this,&temporary,uint32_id)`. Concrete targets remain unresolved.
The result object's first DWORD supplies a NUL-terminated byte string;
null selects verified empty string **VA** `0x1C5EA58`, raw
`0x185D658`. `0xC27FE0 -> 0xCD5020` interns through strlen/strcmp:
the output is text, not userdata, but its Unicode encoding is unproven.
Scan failure/unknown key yields nil at `0xC25608` while reporting parse
success. Ordinary SDB references produce subtype-2 userdata; the separate
`0xC549A0` handle path produces subtype 1. Zero handles yield nil.
Missing-row/localization fallback semantics require resolving service slots.

## Keyed close notification borrows the vector-owned socket pointer

Reader RTTI is `nl::SimpleSocketReplayReader`; destructor callsites
raw `0xBE1214/0xBE12A4` load socket from reader `+0xC` and call
`0xBED1C0`. That helper locks socket `+0x88`, clears owner `+0x40`,
and branches on state `+0x54`: states 1/2 become 4 at `0xBED241`
and request close; state 3 becomes 5 and drains through `0xBEC5B0`
(diagnosing a noninvalid handle); state 0 becomes 5 directly. Other states
reach diagnostic `0xBED29B`, without a retry branch.

Close `0xBED0F0` acts only if socket byte `+0x81==0`; it sets the
byte **before** calling network virtual `+8(socket,&handle)` at
`0xBED16F`. It diagnoses missing network back-pointer `+0x3C` and
handle remaining valid afterward. The network target `0xBBD940`
diagnoses invalid inputs/port, closes the handle, stores -1, then posts
`(port,0,socket,NULL)` at `0xBBDA10`.
The key is the socket pointer with no extra AddRef; lifetime comes from
the owning network vector.

Successful dequeue invokes socket slot `+0x1C` at `0xBBC9B8`.
ManagedUDP vtable **VA** `0x1C73D78` resolves it to `0xBED590`,
which diagnoses valid handle or already-set byte `+0x80`, sets that
byte, and enqueues sentinel 1. `0xBEDAA0` consumes 1, changes
state 4 to 5, drains, and returns false; vector pump deletes at
`0xBBDD39` through `0xBEC2B0 -> slot0(0xBEC440,1) ->
0xBEC480 -> free`.

Post return BOOL propagates out of `0xBBD940`, but `0xBED0F0`
ignores it. No result-based diagnostic, retry, rollback, reference
adjustment, or fallback was found in that local path; the pre-post port
diagnostic is not failure handling. Without another event, state 4 remains
retained and `+0x81` suppresses another close/post. Alternative maintenance
or pump transitions are the next bounded check; diagnostic termination and
global absence of recovery remain unresolved.

## ManagedUDP maintenance does not resolve closing state in inspected paths

Vtable raw `0x1872978` resolves slot `+0x10` to `0xBED960`
and `+0x14` to `0xBEDAA0`. Network maintenance passes one at
`0xBBDE6A..0xBBDE6C`; that argument immediately skips work at
`0xBED98E..0xBED992`. Argument zero processes only states 1/2
(`0xBED9B6..0xBED9C1`), excluding closing state 4.

In the socket pump, empty queue with state 4 returns true at
`0xBEDC84..0xBEDC93`, retaining the handler. Ordinary packets skip
delivery outside states 1/2. Sentinel 2 calls close helper `0xBED0F0`,
whose already-set `+0x81` suppresses another post
(`0xBED135..0xBED142`). Sentinel 1 diagnoses a noninvalid handle,
then changes 4 to 5 (`0xBEDBF8..0xBEDC20`).
Neither inspected maintenance nor empty-queue branch supplies independent
timeout/fallback completion after failed posting. Other sentinel producers
remain the next bounded question, not a basis for a universal no-recovery
claim.

## Localization accessor is uiManager +0x974

The parser singleton is the established uiManager global **VA**
`0x1E002B8`, not a separate unknown singleton. Its vtable
**VA** `0x1C73EBC`, slot `+0xD0` at raw `0x1872B8C`,
contains **VA** `0x1024560`, raw `0xC23960`.
That function returns `[this+0x974]` with a null diagnostic guard.
The bounded trace did not resolve assignment/type of `+0x974`,
service slot `+0x108`, or missing-ID behavior. Follow-on scope is those
field stores, not repeating the already verified singleton bootstrap.

## ManagedUDP sentinel producers narrowed

The queue at socket `+8` uses raw helper `0x12CE310`.
Close callback `0xBED590` checks handle `+0x44==-1` and flag
`+0x80==0` diagnostically, sets the flag, and enqueues 1 at
`0xBED5E2`. Error callback `0xBED5F0` optionally logs a nonzero
error then unconditionally enqueues 2 at `0xBED66D`; receive-error
dispatch uses virtual `+0x20`. Ordinary receive allocations enqueue at
`0xBED839`.

Forced state-5 stores `0xBED255/0xBED281` in `0xBED1C0` lie in a
switch restricted to incoming states 0..3, not a state-4 fallback.
No additional sentinel-1 producer was found in the inspected references;
indirect coverage is incomplete. This narrows the no-fallback result
without converting it into a global absence claim. Next work follows
ManagedUDP's distinct receive/error path rather than extrapolating
GameSocket semantics.

## Localization field assignment and service allocation

Raw `0xBFB51A` (PE VA `0xFFC11A`) stores `ESI` into
`[EDI+0x974]`, sourced from uiManager initializer's third argument.
Caller `0x30B1EE` pushes service global **VA** `0x1DFFFC0`;
`0x30B200` calls uiManager slot `+0x1C`.
Service creation requests `0xA5C` bytes at `0x310C76`, invokes
constructor raw `0x3DA9D0` (VA `0x7DB5D0`) at `0x310C93`,
and stores its result into wrapper `+0x10` at `0x310C9C`, backing
that global.

Concrete service vptr and slot `+0x108` remain unresolved. The next
check is constructor `0x3DA9D0`'s vptr stores and any this adjustment,
not uiManager bootstrap again.

## ManagedUDP error suppression differs by failure layer

The active-sample instruction trace separates these cases:

| Failure layer / raw range | Observed handling |
| --- | --- |
| IOCP dequeue with nonnull OVERLAPPED, `0xBBC973..0xBBC983` | Error 995 skips callback entirely; all others, including 996 and 10038, reach slot `+0x20` unchanged |
| Receive WSAGetOverlappedResult(FALSE), `0xBED738..0xBED766` | Error 996 returns without callback/repost; 995/10038 become zero before slot `+0x20`; others pass unchanged |
| ManagedUDP slot `+0x20`, `0xBED5F0` | Nonzero logs, zero suppresses logging only; both enqueue sentinel 2 at `0xBED66D` |

Socket `+0x98` owns persistent `0xA0`-byte receive operation with
zero-initialized first `0x14` bytes OVERLAPPED, sockaddr at operation
`+0x18`, and address length `+0x98`, reset to `0x80`.
Socket `+0x50` is receive buffer and `+0x4C` its capacity, default
`0x4000`. Receive `0xBED690` locks socket `+0x88` and skips
everything if closing byte `+0x81` is set.

Only successful result retrieval reaches WSARecvFrom repost at
`0xBED8F5`, reusing the operation/buffer even for zero-byte success.
Nonempty data first enqueues a separate `0x38`-byte packet at
`0xBED839`; its payload ownership is the next check before assuming
independence from the reused receive buffer. Neither error layer frees or
replaces persistent receive storage; destructor `0xBEC480` frees it.
Socket `+0x48` is a distinct retained send item, not this operation.
These are ManagedUDP-specific findings, not inherited GameSocket behavior.

## Localization service slot +0x108 resolves to 0x3E0620

Constructor raw `0x3DA9D0` stores its primary vptr at `0x3DAA03`:
**VA** `0x1C63014`, raw `0x1861C14`. Slot `+0x108` at raw
`0x1861D1C` / **VA** `0x1C6311C` points directly to raw
`0x3E0620`, **VA** `0x7E1220`, not a this-adjusting thunk.
Constructor returns its original pointer.

Target `0x3E0620` calls provider global **VA** `0x1E00100`,
virtual `+0(out,id,0)`, at raw `0x3E065C`.
Null returned text selects empty **VA** `0x1C5EA68`, raw
`0x185D668`. This fallback is distinct from the parser's outer empty
string address. Missing-ID-to-null behavior remains unproven until provider
dispatch is resolved; the next scope is that global's assignment/slot zero,
not the already resolved service/bootstrap chain.

## ManagedUDP queued packets snapshot receive bytes and lend the blob

Raw `0xBED796..0xBED839` allocates `0x38`-byte packet records:

| Offset | Observed field |
| --- | --- |
| `+0` | 64-bit timestamp |
| `+8` | Endpoint: flags `+8`, IPv4 `+0xC`, IPv6 `+0x10..+0x1F`, port `+0x20` |
| `+0x28` | Blob vptr |
| `+0x2C/+0x30` | Backing descriptor / ownership control |
| `+0x34` | Alignment, initially zero |

`0xBED7E6` calls blob slot `+0x1C(0)`; `0xBED7F6` calls slot
`+0x10(socket.recvBuffer,receivedLength)`, target `0x12CB9D0`.
That helper resizes fresh backing and copies exactly receivedLength bytes
to descriptor data at `0x12CB9F0`. This is a byte snapshot, not an alias
of the persistent receive buffer. Queue `0xBED839 -> 0x12CE310`
stores the packet pointer without cloning/AddRef; only then does
`0xBED8F5` rearm reception.

Dequeue `0xBEDAF9 -> 0x12CE400` recycles the queue node and returns
the packet. States 1/2 deliver through owner slot `+4` at
`0xBEDB3C..0xBEDB4E` with borrowed blob pointer, endpoint pointer,
and timestamp low/high. Reader construction installs reader `+8` as owner
at `0xBE110F`; interface vtable **VA** `0x1C73C88`, slot `+4`,
resolves to `0xBE1450`. Matching endpoint forwards the borrowed blob
synchronously to `0xBE0110`; mismatches only log. Retention beyond that
call is the next explicit check.

After callback, or immediately outside states 1/2,
`0xBEDB51..0xBEDB86` decrements packet control strong `+4`;
zero calls control slot zero, then weak `+8` is decremented and slot
`+4` called on zero. Packet storage is freed afterward.
Queue drain `0xBEC674..0xBEC6BC` applies the same release/free without
callbacks. Owner `+0x40` is assertion-checked, not safely null-guarded;
delivery has no explicit surrounding lock in this inspected path.
Concurrent shutdown/callback safety remains unresolved rather than inferred
from the assertion.

## Localization provider is tfGlobalizationManager

Provider global **VA** `0x1E00100`, raw `0x19FE500`, is singleton
wrapper **VA** `0x1E000F0` plus `0x10`.
Assignment raw `0x312799` / VA `0x713399` stores the constructed
`0x24`-byte provider through wrapper `+0x10`.
RTTI identifies `tfGlobalizationManager`; constructor raw `0x38FC50`,
VA `0x790850`, writes vptr at `0x38FC58`:
**VA** `0x1C629B8`, raw `0x18615B8`.

Slot zero points to **VA** `0x7912F0`, raw `0x3906F0`, leading
through `0x3909C0` to `0x6A8AB0`.
ID zero bypasses lookup with initial verified empty text.
Raw `0x6A8B3C` tests null lookup output, and
`0x6A8B5E..0x6A8B6D` checks selector/AL before capacity handling.
Lookup `0x6A8BE0`, actual missing-ID outcome, and language/fallback
selection remain unresolved. The next scope is that lookup's internals,
not the now-complete provider bootstrap.

## ReplayReader discriminator 1 copies into reader-owned blob storage

Raw `0xBE1508 -> 0xBE0110` enters the parser synchronously.
At `0xBE0140..0xBE0192`, packet bytes become a stack-local borrowed
cursor and the discriminator is consumed. The discriminator-1 branch
`0xBE01F9..0xBE023F` selects a reader-owned blob through `0xBDFA90`
(container at reader `+0xC`), grows it, gets writable data plus old
length, and copies remaining packet bytes. Explicit memcpy occurs at
`0x336B72..0x336B79` through `0x159D8F4`.

Destination helper `0x12CBCB0` reuses suitable unique backing or allocates
and installs new backing at `0x12CBE07/0x12CBE15`.
No packet-control increment or persistent packet-pointer store was found
on this path. Its stored bytes therefore survive independently of immediate
packet release. Other discriminators, their length checks, and their
ownership behavior remain separate follow-on scope.

## Localization ID remap, language fields, and English fallback

All code addresses below are raw offsets; table addresses are explicitly
PE VAs in runtime storage, not file-backed strings.
Lookup `0x6A6B00` uses unsigned 32-bit ID against records at **VA**
`0x1E55E48`, count **VA** `0x1E55E4C`, stride `0x1C`, key
at record `+0x18`. Lower-bound helper `0x6AA040` compares unsigned;
`0x6A6B60..0x6A6B69` rejects end/nonmatching keys.

Optional remap `0x6A8BEE..0x6A8C1F` runs only when the flag's low
byte is zero, using pointer/count **VA** `0x1E55DD8/0x1E55DDC`,
stride 8, original ID `+4`, replacement ID `+0`. No match preserves
the original ID.
Manager `+4` supplies language selector at `0x3909D9`; byte-verified
language table **VA** `0x1DE0868` establishes:

| Selector | Language | Text pointer offset in record |
| --- | --- | --- |
| 0 | en | `+0x10` |
| 1 | ko | `+0xC` |
| 2 | de | `+0` |
| 3 | fr | `+4` |
| 4 | zh | `+0x14` |
| 5 | ru | `+8` |

Null/empty selected text falls back to English `+0x10` at
`0x6A8C6F..0x6A8C84`; out-of-range selectors directly select English.
Missing ID sets `*out=0`, returns `AL=0`. Existing record returns
`AL=1` even if English fallback is null/empty, so result existence and
nonempty translated text are distinct.
Actual IDs/text depend on loaded runtime data. Producer/loading, remap
ordering, and table ownership are the next bounded questions.

## ReplayReader outer discriminator and truncation ledger

Raw `0xBE018C` reads one unsigned discriminator byte after shortage
check `0xBE0170..0xBE0180`; cursor advances at
`0xBE018E..0xBE0192` and remaining length decrements at
`0xBE01C0..0xBE01C3`. Comparisons `0xBE01C6..0xBE01D0`
recognize only 0 and 1. Let supplied byte length be N.

| Discriminator | Raw branch | Reads / effects | Successful consumption |
| --- | --- | --- | --- |
| 0 | `0xBE0258` | Copy 24 bytes into `this+0xB0..0xC7` via `0x336B50`; `0x12EE170` reads LE16 L and supplies L bytes to embedded destination `this+0xC8`, slot `+0x10` | `27+L`; trailing bytes ignored here |
| 1 | `0xBE01F9` | `0xBDFA90` selects reader-owned blob with argument N-1; nonnull destination follows established append/copy at `0xBE023F` | N; null result logs and consumes only discriminator |
| 2..255 | `0xBE01D2` | Log unsigned discriminator at `0xBE01DD`, return through `0xBE01E5` | 1; no further owner-field writes observed |

Borrowed cursor shortage slot at raw `0x1860D2C` resolves to
`0x3570F0 -> 0x159E27A -> _CxxThrowException`: shortage is not
a clean unknown-case return. Fixed 24-byte check
`0x336B5D..0x336B70`, missing LE16 check
`0x12EE17E..0x12EE18F`, and missing L-byte payload check
`0x12EE1CD..0x12EE1DA` invoke it before their respective reads/call.
Case 0 is not transactional: its fixed 24-byte write precedes later
length/payload checks. L zero skips destination invocation at
`0x12EE1C8..0x12EE1CB`, leaving prior destination bytes unchanged.

After case-0 success, nonzero `this+0xD8` is cleared at `0xBE0288`;
the 24 bytes propagate to `[[this+4]+0x90..+0xA7]` and owner
byte `+0x8E=1` at `0xBE02C3..0xBE02E4`.
Zero `+0xD8` skips propagation. Destination type and enable-state
meaning are the next bounded check; field semantics remain unassigned.

## Localization metadata initializer found; row source/order unresolved

Raw `0x32C070` resolves fields from a generic DWORD-pointer object
through hash lookup `0x1310670`, then stores table owner/data/count.
Remap metadata assignment `0x32C191..0x32C1A0` uses hash
`0xD1ACBFD5`; primary metadata assignment
`0x32C462..0x32C481` uses hash `0x97DFAFF9`.
This is evidence of generic object-backed metadata binding, not a proven
SDB-specific row loader.

Consumer `0x669090` uses primary count/stride `0x1C`, performs key
lookup, and copies selected text. It is not the producer. Small assignment
helpers `0x7403D0/0x740450` have no direct xrefs in the bounded IDA
search. Row origins, copying, sort validation, and owner-retain/release
remain unresolved; lower-bound lookup alone does not establish that a
loader enforces ordering. Next work verifies exact owner fields and their
replacement/lifetime mechanism.

## Replay case-0 destination replaces bytes; request arms propagation

Raw constructor `0xBDFBC0` installs slBlob vptr **VA**
`0x1C80CC4` at reader `+0xC8`, zeros `+0xCC..+0xD7`,
and clears `+0xD8`. Slot `+0x10`, **VA** `0x16CC5D0`
(raw `0x12CB9D0`), resizes to L then copies at offset zero:
case-0 payload handling is replacement, not append. Destructor
`0xBDFCB0` releases its reference-counted backing.

Entry `0xBDFD80` arms `+0xD8` before forwarding through
`[this+8]` slot `+8`; case-0 success clears it before publishing to
the owner. Concrete forwarded operation/caller is the next scope.
The fixed `+0xB0..+0xC7` remains an opaque 24-byte record.
Zero-length destination preservation and nontransactional earlier writes
remain as previously established.

## Localization globals borrow their containing table object's storage

The verified metadata layout is:

| Table | Context VA | Rows VA | Count VA |
| --- | --- | --- | --- |
| Remap | `0x1E55DD4` | `0x1E55DD8` | `0x1E55DDC` |
| Primary | `0x1E55E44` | `0x1E55E48` | `0x1E55E4C` |

These are zero-initialized `.data` virtual-tail globals, **without
file-backed raw offsets**. Mechanical section-delta subtraction would
identify unrelated bytes in another section.
Raw `0x32C187..0x32C1A0` and `0x32C457..0x32C481` store
lookup result rows, incoming `this` (saved as ESI at `0x32C07F`),
and count. Lookup `0x1310670` searches the tree at object `+0x34`;
node `+0x14` points to a holder. Success returns holder `+0xC`
and writes holder `+4` to count. Missing key explicitly returns zero
and count zero at `0x13106B5..0x13106C5`, still storing context.

The global triple stores do not retain/release anything. Alternate setters
`0x7403D0/0x740450` likewise overwrite three DWORDs; inspected
context-global references are assignments only.
Containing-object reset `0x1312030` supplies actual cleanup:
optional catalog-entry callback `+0x24` gets rows/count at
`0x1312091..0x13120A2`; holder `+0x18` provides ownership handle.
Atomic decrement of handle strong `+4` invokes slot zero on resulting
zero, then weak `+8` invokes slot `+4` on resulting zero.
Holder cleanup/free helper is `0x12C8D60` at `0x13120E5`.
Instruction `lock xadd` plus branch tests the updated count; misleading
decompiler old-count comparisons must not substitute for it.
The corresponding retain site remains unverified.

Loading entry `0x130FF0A..0x130FF10` resets an already-loaded object
before replacement; app `0x309331` calls it with context **VA**
`0x1E4BAB0`. Metadata callback `0x32C070` has no discovered direct
call; its actual **VA** `0x72CC70` appears at raw `0x1860980`,
**VA** `0x1C61D80`, reported slot `+4` of that context's table.
The precise indirect trigger and global-rebinding order remain the next
check. Row source format and sorted-order validation remain unestablished.

## Metadata callback is invoked after successful decoded-block parsing

Raw constructor `0x32C028` installs vtable **VA** `0x1C61D7C`
into context **VA** `0x1E4BAB0`. Raw table bytes at `0x186097C`
are `40 CC 72 00 70 CC 72 00 00 0F 73 00`: slot `+4`
is **VA** `0x72CC70`, raw `0x32C070`; slot `+8` is **VA**
`0x730F00`, raw `0x330300`.
Exact success invocation is `0x13104B7: mov eax,[edi]`,
`0x13104B9: mov ecx,edi`, `0x13104BB: FF 50 04`.

On inspected normal success, decoded-block parsing completes, context
loaded byte `+9=1` is stored at `0x13104A7`, temporary text is
released, then slot `+4` binds metadata before success return.
Already-loaded replacement resets at `0x130FF10`; reset invokes slot
`+8` at `0x1312065` **before** clearing loaded byte and destroying
holders/backing. Open failures return false without success callback;
parser-false calls reset at `0x131047A`, then returns false.
Exception unwinding is outside this bounded result.

One separate metadata triple, **VA** `0x1E4ED14/0x1E4ED18/0x1E4ED1C`,
is cleared by reset helper `0x7129A0` and rebound through
`0x712A00` at `0x32C0AE`. This does **not** by itself establish
clear/reset coverage of localization/remap triples `0x1E55DD4...`
and `0x1E55E44...`; those exact slot-`+8` helper paths are the next
check rather than extrapolating from another table.

## Replay propagation arming forwards no request bytes

The prior "header request" label is not established by its implementation.
Raw `0xBDFD80` sets `this+0xD8=1`, loads interface `this+8`,
and tail-jumps slot `+8` without stack arguments or serialized bytes.
SimpleSocketReplayReader constructor `0xBE1040` installs interface
vtable **VA** `0x1C73C88`; its slot `+8` contains **VA**
`0xFE2200`, raw `0xBE1600`.

That target only checks `[interface-4]` (owner `+4`), diagnoses zero
with code `0x3D`, then returns. It is not a request writer.
Separate successful case-0 publication still clears armed `+0xD8`,
copies the opaque 24-byte record to owner `+0x90..+0xA7`, and sets
owner `+0x8E=1`. Bounded xref/direct-call searches found no callers
of the arming entry; no specific request frame or record semantics were
established. Next work follows owner construction and concrete consumption
of the published record/byte.

## Replay reader reciprocal link is not its publication owner

The bounded owner-consumer search did not identify a concrete publication
owner type or a consumer of owner `+0x8E/+0x90..+0xA7`.
Constructor raw `0xBDFBC0` initializes reader `+4` to null, stores its
second argument at reader `+8`, and writes the reader pointer to that
argument's `+4`. Destructor `0xBDFCB0` verifies/clears that reciprocal
link. These are distinct fields; constructor argument two does not by
itself establish reader `+4`'s later owner.

Case-0 publication, when armed, diagnoses null reader `+4` but continues
to dereference it, locks through owner `+0x58`, copies the 24 bytes to
owner `+0x90..+0xA7`, sets `+0x8E=1`, and unlocks.
Parser callers include wrapper `0xBDF820` and callback `0xBE1450`.
Actual nonnull assignment to reader `+4` is the next scope before
guessing owner semantics from offset coincidences.

## Reset clears localization/remap triples before holder destruction

The worker's reported SHA ending `544a` was a transcription error.
Fresh file hashing and IDA server health confirm the active sample ending
`5445`. Its reset-byte observations therefore apply to the identified sample.
Raw `0x330300` directly stores immediate zero to all six globals:

| Triple | Rows clear raw | Context clear raw | Count clear raw |
| --- | --- | --- | --- |
| Remap, VAs `0x1E55DD4/0x1E55DD8/0x1E55DDC` | `0x330346` | `0x330350` | `0x33035A` |
| Primary, VAs `0x1E55E44/0x1E55E48/0x1E55E4C` | `0x330495` | `0x33049F` | `0x3304A9` |

These are straight-line `C7 05 [global] 00 00 00 00` stores, with
no helper between either triple's stores. Separate setters
`0x7403D0/0x740450` are not called here; success callback
`0x32C070` directly rebinds the triples at the previously recorded sites.

The earlier missing-direct-xref reachability caveat is retracted:
raw table `0x186097C` slot `+8` bytes `00 0F 73 00` resolve to
VA `0x730F00`, raw `0x330300`; raw `0x1312065` bytes
`FF 50 08` invoke that slot. Together with the established constructor
table assignment, this supplies the indirect reset path. It clears the
borrowed global triples before containing-holder destruction on inspected
normal reset paths. Exception/concurrent-reader behavior is not established.

## Bounded stops: reader attachment and localization row ordering

The attachment investigation did not resolve a nonnull writer to reader
`+4`. Helper raw `0xBDF9E0` receives reader `+0xC`, so its
`[this+4]=0` initializes reader `+0x10`, not the publication-owner
field. A broad field scan returned tens of thousands of unrelated matches;
it supplies no defensible attach site and was not promoted to evidence.
The worker's separate claim of invalid vtable bytes at operand
`0x1C73CC0` is withheld pending the correct PE-VA-to-raw read.

The row-order investigation likewise found no new producer proof.
Lower-bound `0x6AA040` assumes primary rows sorted by unsigned key
`+0x18`, stride `0x1C`; it neither sorts nor validates them.
Checked catalog lookup `0x1310670` returns holder payload fields but
does not establish an ordering flag/source-order contract. Row construction
and sort validation remain unresolved, not evidence that validation is
globally absent.

Further broad rechecks of those two gaps are paused. New independent
scopes are the ReplayReader endpoint comparison and the globalization
language selector's setter/default, both with concrete entry points.

The vtable-byte concern is corrected: constructor raw `0xBDFBFF`
encodes **VA** `0x1C73CC0`; its raw `.rdata` location is
`0x18728C0` and contains pointer-like entries. The earlier nonpointer
claim read VA as raw. Reported first DWORDs `0xFE0890/0xFE0980`
are stored VA values, not yet-converted raw targets; they must not be used
as raw code offsets. Full method boundary/remaining table entries were not
established by merely reading adjacent DWORDs. This does not resolve the
attachment writer.

The worker confirmed the first two stored values are **VAs**
`0xFE0890/0xFE0980`, mapping to raw `0xBDFC90/0xBDFD80`.
Only those two targets are confirmed; adjacent RTTI/data must not be
classified wholesale as method slots.

## Replay endpoint comparison uses common family bits and exact port

Raw `0xBE1450` builds its configured endpoint from `[this+4]` fields
`+0x60..+0x79` and compares it with the packet endpoint (packet `+8`).

| Field | Packet offset | Configured owner offset |
| --- | --- | --- |
| Family flags | `+8` | `+0x60` |
| IPv4 bytes | `+0xC..+0xF` | `+0x64..+0x67` |
| IPv6 bytes | `+0x10..+0x1F` | `+0x68..+0x77` |
| Port | `+0x20..+0x21` | `+0x78..+0x79` |

Helper `0xBCC440`, called at `0xBE14D6`, compares address bytes as
one DWORD for IPv4 or four DWORDs for IPv6 without swapping them.
It selects by intersection of family bits, not whole-byte equality:
common bit 1 selects IPv4 before common bit 2/IPv6. With no common bits,
it succeeds only when packet flags are zero. Port is separately compared
as 16 bits at `0xBE14DF` in every case.
Mismatch formats/logs endpoints and does not forward the blob; match
calls `0xBE0110` at `0xBE1508`. Connection closure is not established
by the mismatch branch.

Related `0xBE1890` discriminator 2 with nonnull receiver calls
`0xBE0D60(endpoint,10000)`. That helper compares 40-byte endpoint-list
records and appends fields/metadata if no match and capacity allows.
This is not proof that the list initializes fixed owner `+0x60` fields.
Their concrete writer is the next bounded check.

## Globalization selector defaults and availability policy

Raw constructor `0x38FC61` sets manager `+4=0`, English.
Numeric setter `0x390550` accepts only unsigned selector <=5 and
availability predicate `0x390640` before writing. Availability accepts
English/German/French/Chinese, rejects Korean, and requires boolean
`Globalization.RussianEnabled` (default false) for Russian.

String setter `0x3905D0` uses mapper `0x10DB480`, exact
case-sensitive `en/ko/de/fr/zh/ru`; unknown strings, including empty or
null-backed text, map to English before availability validation.
Invalid numeric selectors instead leave state unchanged.
App raw `0x307153` reads `Config.ForceLanguage`: present calls the
string setter at `0x307182`; absent uses `0x390530`, applying config
state `+0x3C` only when unsigned <=5.

After a write, differing config-state `+0x3C` causes indirect slot
`+0x88` call at `0x390597` (string equivalent `0x39062A`), with
arguments `(VA 0x1E4C310,language,0,0)`. Concrete target and persistence
remain unresolved; next work follows this notification.
Language table **VA** `0x1DE0868`, raw `0x19DEC68`, is in `.data`,
not `.rdata`. Codes are four-byte-spaced literals at **VA**
`0x1C03878..0x1C0388C`, raw `0x1802478..0x180248C`.

## Language setter updates SavedLanguage through oConsoleSystem

The indirect receiver is **VA** `0x1E69348`, RTTI `oConsoleSystem`.
Constructor raw `0xE58770` writes vptr **VA** `0x1C77EA8`
at `0xE587A3`. Slot `+0x88` targets **VA** `0x1242360`,
raw `0xE41760`, formats language with `%i`, and forwards through
slots `+0x98` then `+0x80` to raw `0xE41360`
(VA `0x1241F60`).

Argument **VA** `0x1E4C310` is console variable `SavedLanguage`,
description `Current Language`, default -1, bounds -1..5, flags `0x8C0`,
constructed at raw `0x2B3320`. Constructor `0xDD5158` writes
`[variable+0x5C]=variable`, so **VA** `0x1E4C36C` aliases this
variable. The call updates its text/numeric state including `+0x3C`,
not merely a notification. Disk persistence is not established by the
variable name; save/export filtering is the next explicit check.

## Fixed ManagedUDP endpoint writer found; interpretation pending

The fixed fields consumed by `0xBE1450` are written in `0xBEC830`,
called from constructor `0xBE1040` with `(0,0)` after ManagedUDP
state construction and callback-interface attachment.
Reported stores set flags at state `+0x60`, copy **VA**
`0x1E6853C` into IPv4 `+0x64` when bit 1 applies, copy
**VA** `0x1E68540..0x1E6854C` into IPv6 `+0x68..0x74` when
bit 2 applies, and set port `+0x78=0`.
Constructor `0xBEC2E0` also initializes flags and port zero.

The worker corrected its labels: these are raw-zero code offsets.
Raw `0xBECAB2` references absolute operand **VA** `0x1E6853C`.
Constructor `0xBE1074` computes interface `outer+8` and
`0xBE10D3` stores state at `outer+0xC`; callback
`0xBE147E` reads `[callback_this+4]`, the same state slot after
the +8 interface adjustment. This is separate from the publication-owner
field `reader+4` in the other base object.
Runtime values and local-bind versus remote-peer meaning are unresolved.
In particular, zero initial port does not prove a packet
matching failure: later bind/getsockname-related field writes must be traced.
Endpoint-list insertion `0xBE0D60` remains a separate path.

## SavedLanguage qualifies for config export, not unconditional autosave

Raw exporter `0xE426B0` requires variable flags `+0xC & 0x40`
at `0xE42711`; SavedLanguage flags `0x8C0` qualify.
Without flag `0x400`, it reads current string `+0x1C`, omits unchanged
defaults, and emits `SetSave SavedLanguage "<value>"`
(`0xE427C7..0xE42874`).
Routine `0xE8AEB0` invokes exporter through console slot `+0x48`,
builds `%s.con`, and writes through `0x12BB0B0` with verified mode
`wb`. Automatic export checks `eng.autoSaveConfig`, default zero;
config-name default is `settings` at `0xE55416..0xE5543E`.

Thus `settings.con` export capability is established, not unconditional
disk persistence after language changes. Absolute directory, trigger
timing, and flag `0x800` semantics remain unresolved. Next scope follows
one actual automatic-export caller and its guard.

## ManagedUDP peer endpoint is distinct from assigned local bind port

Raw `0xBEC830` builds a bind address through
`0xBE3990 -> 0xBE3B50` from network owner `+0x30`, port zero.
Bind is at `0xBEC9A9`, getsockname at `0xBECA23`, decode through
`0xBE3A00` at `0xBECA75`. The assigned local port is stored at
**state `+0x58`** at `0xBECA7E`, not remote port `+0x78`.

Connect routine `0xBECF50` calls `0xBE35D0`; only successful connect
reaches `0xBED064 -> 0x483640`, copying peer flags/address/port to
**state `+0x60..+0x79`**. Those are the fields compared by
`0xBE1450`, so zero constructor input does not establish a zero peer
comparison after connection.
Network family mode `+0x2C` is selected by `0xBBD110`;
`0xBE3510` maps 1 to AF_INET(2), 2 to AF_INET6(23).
Endpoint bits 1/2 represent IPv4/IPv6 presence.
PE imports/local export verification resolves WSOCK32 ordinals
2/4/6 as bind/connect/getsockname. Relevant PE globals are zero-fill;
raw-IDB `0B` bytes at their VA numbers are not initialized address values.
Next work follows the concrete ReplayReader connect wrapper and state.

## Replay peer connect occurs during reader construction

Raw caller `0x38262B` supplies a new `0x18`-byte reader in ECX and
endpoint pointer on stack to constructor `0xBE1040`.
Endpoint text comes from caller-object `+0xC`, parsed at
`0x382608 -> 0xBCBF90` as `host:port` or `[host]:port`,
with endpoint port at `+0x18`.

Constructor binds at `0xBE1120 -> 0xBEC830(0,0)`, then copies a
`0x20`-byte endpoint by value onto the stack at
`0xBE1129..0xBE1137` and calls `0xBECF50` with socket state
from reader `+0xC`. An extra decompiler EDI parameter is not an
established stack argument.
Connect returns AL zero for invalid address, zero port, state `+0x54`
not one, or connect failure. Success copies peer endpoint at `0xBED064`,
changes state to two, returns AL one.

The constructor nevertheless returns the reader pointer on connection
failure. Primary slot `+4`, raw `0xBE12D0`, checks stored peer address
and nonzero port, not socket state. RTTI confirms
`nl::SimpleSocketReplayReader`; the concrete caller's readiness/fallback
handling is the next question before inferring its success result.

## Autosave is shutdown-conditional, with an initialization enable option

Raw `0xE553F0` logs `Shutting Down`; guard
`0xE55416..0xE5541F` skips export when autosave variable `+0x40`
is zero. Call `0xE5543E` uses console slot `+0xEC`, resolved through
raw entry `0x1876B94` to `0xE8AEB0`. Variable registrations
`0x2D2EB7..0x2D2EBC` and `0x2D2EF7..0x2D2EFC` supply
defaults zero and `settings`.

Initialization raw `0x10D8CCA..0x10D8CDB` executes command
`eng.autoSaveConfig 1` only if its first argument byte `+0x11`
is nonzero. Producer/default of that option remains the next bounded
check. Persistence is conditional on shutdown, not immediate or guaranteed;
final output directory remains unresolved.

## Reader construction stores allocation; wrapper exposes inverted readiness

Raw `0x382634..0x382642` stores the constructed reader at owner
`+0x1A4`; `0x382662..0x382681` creates/stores wrapper `+0x1A0`,
borrowing the reader through wrapper `+8`.
Check `0x382686` tests wrapper existence, not connected readiness.
No connect-failure-based fallback/release was found in that construction
path. Constructor returning a pointer proves allocation, not connection.

Wrapper raw `0xBE00E0..0xBE00F3` loads the borrowed reader, invokes
slot `+4 -> 0xBE12D0`, and returns logical **not ready**.
Actual consumer of that predicate is the next bounded check.
Owner reset `0x382760` clears fields and deletes wrapper first at
`0x382806`, then reader at `0x382822`, each via slot zero with flag one.

## Normal application initialization enables shutdown autosave

`ClientApplicationInitialize`, raw `0x3063D0`, constructs descriptor
at `[ebp-0x12C]` with `0x306D82 -> 0x3248F0`.
Constructor `0x32490C` zeros descriptor word `+0x10`, including
byte `+0x11`. Application initialization then unconditionally sets that
byte one within this block at raw `0x306E1C` (`var_11B`).
Call `0x30705A` passes the descriptor to `0x10D8A40`, whose guard
at `0x10D8CC6` tests that same `+0x11` before the command call
at `0x10D8CCC`.

Thus the normal inspected initialization path explicitly enables
`eng.autoSaveConfig`, despite the console variable's registration default
zero. The descriptor field is left unnamed; no symbolic option name was
established. Export remains conditional on its later shutdown guard, current
variable state, and successful file operations, not immediate persistence
on each language setter. Output path and write-error propagation are the
next bounded questions.

## Wrapper readiness affects worker status/timing, not a proven error return

RTTI identifies wrapper `nl::SocketReplayReader`; vtable **VA**
`0x1C73CC0`, slot `+0x14` at raw `0x18728D4`, contains
**VA** `0xFE0CE0`, raw `0xBE00E0`.
Worker raw `0xBD62F0` holds it at `+0x24` and calls this slot at
`0xBD6756..0xBD675E`.
The guard at `0xBD6760..0xBD676D` requires nonzero inverted
readiness and stack byte `[esp+0x17]==0`. True sets worker `+0x54=1`
at `0xBD6782`, adjusts timing, then stores wrapper virtual `+0x18`
result into `+0x55` at `0xBD67F6`; otherwise both bytes clear at
`0xBD67FE`. This establishes status/timing behavior, not an error return.
The guard byte's producer and status consumers are the next scope;
worker/outer-owner RTTI remains unverified.

## Config export write prefix and nonuniform write failures

Raw `0xE8B02B -> 0x13192D0` selects write-directory field `+4`;
`0x3A5910` extracts the prefixed path.
Initialization `0x10D8BDA -> 0x1317970` uses descriptor `+0x18`,
otherwise default `localassets` (**VA** `0x1C0357C`, raw
`0x180217C`). Relative prefixes resolve against captured process CWD,
`0x13178B0 -> 0x12B6E90`, using GetCurrentDirectoryW.
Thus the default is `<captured-CWD>\localassets\settings.con`;
the normal application descriptor's override is the next explicit check,
and no actual absolute user directory is asserted.

Writer `0x12BB0B0 -> 0x12BA030` uses `_wfopen("wb")`.
Failed open attempts directory creation and retries; there is no mount
lookup on this inspected path. Final failure returns false and exporter
logs `Couldn't open %s.con for writing` at `0xE8B07C`.
Short fwrite invokes a diagnostic callback, then returns true if it returns;
fclose errors are ignored. These are distinct from cleanly propagated
open failure, not proof every export succeeded. No writes were executed.

## Worker readiness guard marks queue work; status has an app consumer

Raw worker `0xBD62F0` initializes stack guard byte zero at
`0xBD64EF`, sets it one at `0xBD656E` after nonzero worker
`+0xC4`, and processes its queue-like `+0xB4` structure.
At `0xBD6768..0xBD676D`, set guard prevents the inverted-readiness
timing path and clears `+0x54/+0x55` at
`0xBD67FB..0xBD6804`, even when virtual return is true.
This is a per-call work/queue-path guard, not established EOF/error.

Concrete caller raw `0x65C1F0` tests worker `+0x54` at `0x65C28E`;
with zero input count and `0x384390(worker)` false, it sets an
application field `+0xB78` at `0x65C295..0x65C2A3`.
Worker also sets `+0x54` on an earlier state-flag path at `0xBD63E6`
that returns false, so the byte must not be interpreted as a simple error
or EOF. No `+0x55` consumer or semantic label was verified.
Next scope follows the application flag and its consumer, not guessing
semantics from readiness inversion alone.

## Normal application leaves write-prefix override null

Raw descriptor constructor `0x324919` initializes `+0x18=null`.
No write to that field was found between normal app descriptor construction
at `0x306D82` and dispatch at `0x30705A`; the inspected null/empty
guard therefore selects `localassets` as write prefix.
At `0x306EE3`, app config `+0xA6C` looks up
`Development -> InstallDir`; `0x306EF6` stores it at
`[EBP-0x110]`, descriptor **`+0x1C`, not +0x18**.
Verified literals are **VA** `0x1B40774/0x1B40780`,
raw `0x173F374/0x173F380`.

Config lookup supports parent fallback and recursion-depth guarding.
The installed-directory field is a distinct option whose consumer remains
the next scope. The static result still names no absolute user path:
relative write-prefix resolution uses runtime captured CWD.

## Application +0xB78 requests a one-iteration timing override

Raw bootstrap `0x2EAE70` loads app global **VA** `0x1DFFD98`
for initialization (`0x2EB184`), run loop (`0x2EB198`), and shutdown
(`0x2EB1F9`), establishing the same app base for this writer/consumer.
Writer `0x65C1F0` sets app byte `+0xB78` at `0x65C2A3`
only after successful `0xBD62F0`, zero argument a2, worker `+0x54`
nonzero, and `0x384390(worker)` false.
That helper is true for `+0x55` or `+0xB1` set, or both
`+0x8C/+0x8D` clear. Its false case therefore requires `+0x55`
and `+0xB1` clear and at least one of `+0x8C/+0x8D` set.
Branch/store bytes were checked at `0x65C28E..0x65C2AA`.

Run loop `0x3098C0` reads app `+0xB78` at `0x309955`,
clears it at `0x30996C`, and sets its iteration-local timing scale zero
at `0x309973`. Adjacent code converts time using that scale and passes
the result to virtual `+0x20`. Continuation uses separate global byte
**VA** `0x1E4B0B0`, not `+0xB78`.
Thus this is a one-iteration timing override, not an exit/stop flag or
proof of worker EOF/error. Invocation binding of `0x65C1F0` remains
the next scope because no direct xrefs were reported.

## InstallDir field has two separately gated initialization consumers

Raw `0x10D8A40` loads descriptor `+0x1C` at `0x10D8D57`.
Nonnull text passes to `0xE9BDA0` at `0x10D8D7F` if descriptor
`+0x24` allows it; another guarded path passes the field to
`0xEBA0E0` at `0x10D8DC9` if `+0x28` allows it.
Null keeps a sentinel fallback. Separate write-prefix `+0x18` handling
remains independent. Null descriptor uses a different initialization path
at `0x10D8DD3`; exit records descriptor presence in `this+5` at
`0x10D8E04..0x10D8E10`.

The bounded pass supports a resource/path-prefix input, not a proven
physical read root. Actual default/app values of gates `+0x24/+0x28`
and concrete consumer roles are the next check; it did not trace those
helpers to file opens or independently reconfirm constructor defaults.

## Replay timing callback is the app-owned object's virtual tick

For the identified active sample, constructor raw `0x65A190`
(VA `0xA5AD90`) stores primary vptr **VA** `0x1C6781C` at
raw `0x65A204`. Vtable entry `+8`, **VA** `0x1C67824`
(raw `0x1866424`), contains **VA** `0xA5CDF0`
(raw `0x65C1F0`). This dispatch has no this-adjustment.
The constructor result is stored in app `+0xB98` at raw `0x30DB84`.

App tick dispatch at raw `0x30A009` invokes this object's virtual `+8`
with `(mode, tickArgument)` when app `+0xB98` is nonnull and
`+0xB94` is zero. Wrapper raw `0x302A30` (VA `0x703630`)
supplies global app **VA** `0x1DFFD98`; startup stores that wrapper
address at raw `0x306E55`.
This resolves concrete indirect invocation despite absent direct function
xrefs. Scheduler registration ownership, invocation ordering, and thread
remain unproven and are the narrower follow-on scope.

## InstallDir consumer gates are pointers populated by normal startup

The active-sample follow-on resolves the earlier gate-value uncertainty.
All code addresses in this section are raw offsets.
App initialization `0x3063D0` calls descriptor constructor `0x3248F0`
at `0x306D82`; construction zeroes `+0x24/+0x28`, but startup
replaces both before `0x30705A` calls `0x10D8A40`.
At `0x306FC6`, descriptor `+0x24` (`var_108`) receives
`&var_138`, a local three-DWORD structure. At `0x307048`,
`+0x28` (`var_104`) receives `&var_14C`, another local structure.
Both are nonnull pointer gates, not booleans. The second consumer reads
fields through at least pointee `+0x10`; complete pointee semantics
remain unresolved.

The `+0x24` test at `0x10D8D50` guards `0xE9BDA0`
at `0x10D8D7F`, which receives selected InstallDir text as a3.
An initial follow-on incorrectly reported forwarding that text to
`0xBBD110` at `0xE9BE03`; the instruction-level correction below
shows a separate networking initialization call with argument 1 instead.
The other consumer `0xEBA0E0` receives the text as a6 and passes
it to `0xEC2D60` at `0xEBA1EE` while building/registering
configuration state. These are concrete initialization routes, not proof
of a physical read root or write destination. Descriptor `+0x18` and
its independent `localassets` write-prefix default are unchanged.
The next bounded scope follows the first sink `0xBBD110`.

## Replay tick registration enters a service through descriptor +0x48

Startup raw `0x306E55` (VA `0x707A55`) stores callback
**VA** `0x703630` at descriptor `+0x48`: stack `[ebp-0xE4]`
relative to descriptor `[ebp-0x12C]`.
Raw `0x30705A` passes that descriptor to `0x10D8A40`.
At raw `0x10D8C58..0x10D8C83`, a nonnull descriptor is
forwarded through service virtual slot zero; false return aborts initialization.
The service is reached through the global at **VA** `0x1E60EA4`.
Raw `0x2D2D60/0x2D2D70` establish aliases involving **VAs**
`0x1DFC82C`, `0x1E60E3C`, and `0x1E60EA4`.
Their exact indirection levels and originating object initialization are the
next narrow check, not grounds for naming a scheduler owner.

This proves descriptor handoff, not retention or invocation of `+0x48`.
Concrete service vptr/slot-zero target, callback ordering, and thread remain
unresolved; no scheduler ownership claim is yet justified.

## InstallDir expands virtual-texture location strings; BBD110 is separate

The active-sample instruction check corrects the earlier claimed InstallDir
forwarding to `0xBBD110`. All instruction addresses here are raw.
At `0xE9BDD6`, EDI preserves InstallDir from `[ebp+8]`, the
third logical argument after ECX/EDX. At `0xE9BDF5`, ECX instead
loads network object `[VA 0x1DFFF70]`; `0xE9BDFB` compares
its DWORD `+8` with zero, and `0xE9BE01` pushes **1** before
`0xE9BE03` calls `0xBBD110`. This is a zero initialization-count
test, not a null field, and InstallDir is not passed.
`0xBBD110` increments existing count at `0xBBD1AE` or initializes
it to 1 at `0xBBD1C2`; literals `NetLib starting up...` and
`WSAStartup failed.` corroborate networking initialization.

Actual InstallDir use occurs at `0xE9BEE9..0xE9BEF4` and
`0xE9BF6F..0xE9BF7A`: `0x12B8E70` receives EDI and
literal `%INSTALLDIR%` (**VA** `0x1BE7B58`, raw `0x17E6758`)
to replace occurrences in local strings `[ebp-0x44]` and `[ebp-0x3C]`.
Replacement helper `0x12B9170` ensures capacity and copies bytes at
`0x12B91E6..0x12B91EF`, rather than retaining the InstallDir pointer.
At `0xE9C504`, the expanded strings pass to `0xEA1330`;
`0xEA13CB..0xEA13DC` copies them into string objects at **VAs**
`0x1E70D68/0x1E70D6C`. Helper `0x2FCC90` confirms allocation
and byte-copy behavior; local string references are released during cleanup.

The literals `Opening virtual texture` and `Installed textures: %s`
support installed/alternate virtual-texture location semantics. The input is
borrowed for expansion and the expanded results are copied; original upstream
InstallDir ownership and exact backing-store opening remain unresolved.
The next scope follows a reader of global string **VA** `0x1E70D68`.

## Descriptor service aliases point to a backed pointer cell

The bounded service-target pass resolves indirection, not the concrete object.
With `.data VA = raw + 0x401C00`, **VA** `0x1DFC82C` is
file-backed at raw `0x19FAC2C`, containing `70 04 E0 01`:
the address of cell **VA** `0x1E00470` (raw `0x19FE870`,
initially zero). These specific cells must not be classified as virtual-only.
Accessor raw `0xE4E630` (VA `0x124F230`) is
`B8 70 04 E0 01; C3`, returning the cell address.

Raw `0x2D2D60/0x2D2D65` loads `[VA 0x1DFC82C]` and stores
it to `[VA 0x1E60E3C]`; `0x2D2D70/0x2D2D75` copies that
to `[VA 0x1E60EA4]`. Thus these aliases carry the cell address,
not the service object. Raw `0x10D8C6D..0x10D8C7F` executes
`eax=[alias]; ecx=[eax]; eax=[ecx]; eax=[eax]; call eax`.
Concrete object publication, constructor/vptr, and slot-zero target remain
unresolved. The final attempted inspection failed with a Python helper-scope
NameError, not evidence that the object lacks a writer.
The next narrow check is the cell/accessor's concrete write references.

## Expanded virtual-texture locations open index files in primary/fallback order

Reader raw `0xE9D960` (VA `0x129E560`) is called at raw
`0xEA156D` after ECX receives **VA** `0x1E6F3C8`.
Its fields `+0x19A0/+0x19A4` are consequently the string globals
**VA** `0x1E70D68/0x1E70D6C`. These globals are virtual-only.
Raw `0xE9D9D7` loads the first string; a null pointer substitutes
empty literal **VA** `0x1C5EA68`.
Formatting via `0xDD89B0` uses `%s_idx` at **VA**
`0x1BE7DDC` (raw `0x17E69DC`), then converts UTF-8 path/mode
with `MultiByteToWideChar(65001,0,...,-1,...)`.

Raw `0xE9DA4D` calls IAT **VA** `0x1AC4704`
(raw `0x16C3304`), verified PE import `MSVCR120!_wfopen`,
with mode `rb`. Nonnull result branches at `0xE9DA60` directly
to `0xE9DB17`, bypassing fallback.
Only a null first result loads the second location at `0xE9DA66`,
formats it with the other `%s_idx` literal **VA** `0x1BE7DB0`
(raw `0x17E69B0`), and opens at `0xE9DAD3`, also `rb`.
This proves primary/fallback **index-file** opening, not texture-payload
or archive opening. Managed strings own copied characters; this reader
borrows their pointers. The next scope starts at successful-open branch
`0xE9DB17` to reconstruct the index's actual reads.

## Service pointer publication path and concrete object vptr

For the same active sample, initializer raw `0xE4F630`
(VA `0x1250230`) installs vptr **VA** `0x1C77C1C` at static
owner **VA** `0x1E00460`, clearing its `+0x10` pointer cell
`0x1E00470`. Owner vtable slot `+4` contains **VA** `0x124F260`.
In that method, raw `0xE4E6EC` (VA `0x124F2EC`) bytes
`89 46 10` publish constructor-returned EAX to `[esi+0x10]`.
Constructor raw `0xE4DFA0` (VA `0x124EBA0`) installs object
vptr **VA** `0x1C77C2C` at raw `0xE4DFDE`.
This supplies a concrete construction/publication path for the service
cell; actual invocation with this static owner remains unverified.
The next scope resolves object slot zero and descriptor `+0x48` handling.

## Virtual-texture index outer layout and read/cleanup boundaries

Successful file opening reaches raw `0xE9DB17` with EBX=`FILE*`.
IAT **VA** `0x1AC490C` (raw `0x16C350C`) is `fread`;
**VA** `0x1AC49A8` (raw `0x16C35A8`) is `fclose`.
Assuming complete preceding reads, native little-endian x86 fields are:

| File offset | Width | Observed use |
|---|---|---|
| `0` | DWORD | Compared with 1 at raw `0xE9DB2E`; preceding fread count unchecked. |
| `4` | DWORD N | Read to object `+0x184C` at `0xE9DB5D..0xE9DB69`; count unchecked. |
| `8` | N records of `0x4C` bytes | Read at `0xE9DB9E..0xE9DBAF`; returned item count compared with N. |
| `8+0x4C*N` onward | Seven DWORD lengths L_i, each followed by L_i bytes | Reads/loop at `0xE9DC45..0xE9DD22`. |

Raw `0xE9DB6B..0xE9DB82` allocates `(N+1)*0x4C` bytes into
object `+0x1848`; helper `0xEB35D0` uses 32-bit multiplication
and zeroes successful allocations. No range/overflow guard was observed.
For i=0..6, destination sizes are `((0x400 >> i)^2) << 4`:
`0x1000000,0x400000,0x100000,0x40000,0x10000,0x4000,0x1000`.
Pointers occupy object `+0x1850+4*i`. Raw `0xE9DCFA` calls
`0x13090D0` (VA `0x1709CD0`) with ECX=payload, EDX=L_i
and stack `(destination,size)`; its return value is ignored.
Byte-copy/back-reference processing is observed, but codec identity is
the next scope. Raw `0xE9DD2E` calls `0xE9DD50`, which walks
`0x4C` records and passes DWORDs `+0x38/+0x3C/+0x40/+0x44`
to `0xE9C990`; semantic field names remain unresolved.

Version mismatch or record-allocation failure logs conditionally and closes
at `0xE9DB55`, then reaches `0xE9DAFF` to attempt a zeroed
one-record allocation and returns AL=1, not false.
Short record/length reads invoke callback **VA** `0x1C15EA0`;
if it returns, processing continues. Payload fread count is unchecked.
Payload allocation failure also invokes that callback, without a dedicated
close branch if it returns. Normal completion closes at `0xE9DD34`
and returns AL=1 at `0xE9DD42`.
No file-length, trailing-data, or N/L_i range validation was observed in
this bounded pass. Callback termination/throwing behavior and cleanup after
non-returning callbacks remain unestablished; no malformed file was executed.

## Concrete service retains descriptor callback at object +0x100

Object vtable **VA** `0x1C77C2C` (raw `0x187682C`) begins
with bytes `F0 57 25 01`, slot-zero target **VA** `0x12557F0`
(raw `0xE54BF0`). Raw `0xE54C33` computes object `+0xB8`
and calls `0xE551F0` to copy the descriptor.
At raw `0xE55317` (VA `0x1255F17`), bytes
`8B 47 48 89 46 48` copy descriptor `+0x48` to that retained
descriptor's `+0x48`, hence service object **+0x100**.
This proves callback retention, not invocation, thread affinity, or actual
initialization through the static-owner publication path.
The next scope follows this specific implementation's `+0x100` reads/calls.

## Index block decoder grammar: literals and extended backreferences

Raw `0x13090D0` takes 32-bit source pointer/length in ECX/EDX
and destination pointer/capacity on stack. EAX is a 32-bit output byte
count, not the pointer type inferred by the decompiler.
All instruction addresses below are raw; code VA adds `0x400C00`.
No standard-codec identity or signature check has been established.

| Item | Observed decoding |
|---|---|
| Initial control | `c=*src++ & 31`; upper three bits discarded, so first item is literal (`0x13090E4..0x1309101`). |
| Literal, c less than 32 | Copy c+1 source bytes, range 1..32 (`0x1309231..0x1309257`). |
| Match, h=c>>5 in 1..6 | Length h+2, range 3..8; q=c&31 (`0x130911C..0x1309175`). |
| Match, h=7 | Length 9 plus sum of extension bytes; read another extension while previous byte is 255, including the terminating non-255 byte. |
| Ordinary distance | Read b; distance=256*q+b+1. |
| Far distance | If q=31 and b=255, read u,v; distance=8192+256*u+v, range 8192..73727. This two-byte extension is big-endian. |

Matches copy from output-distance with forward advancement, supporting overlap;
distance one has a repeated-byte optimization (`0x13091A1..0x130922F`).
Source length zero returns zero before reading. Literal input-span checking
precedes output truncation; insufficient literal input returns zero.
Match extension, offset, and far-distance bytes have no preceding source-end
checks in the inspected code. Unsigned backreference-start comparison against
destination start rejects an earlier source with return zero.
Pointer arithmetic is 32-bit, without explicit overflow checks observed.

After match metadata, source cursor greater than or equal to end finishes
after that match; equality is not required. Otherwise the next control is
prefetched. Literals finish when copied input reaches end; there is no
explicit terminator token (`0x1309181..0x1309192`,
`0x1309259..0x1309268`).
Normal completion returns produced bytes. An item exceeding output capacity
writes only the remaining capacity and immediately returns capacity, without
validating the remaining stream. Exact fill alone does not terminate decoding.
The truncated-match branch calls imported memcpy, so the explicit loop's
overlap semantics must not be assumed for that branch.
No actual compressed block was decoded or sample altered; checking a real
block's first two items remains a possible separate format-validation step.

## Index record rectangle fields reserve a separate occupancy bitmap

The bounded active-sample trace resolves record DWORDs
`+0x38/+0x3C/+0x40/+0x44` as x/y/width/height in the
separate occupancy structure, not decompressed-table indices.
Raw `0xE9DDA9` calls `0xE9C990` (VA `0x129D590`).
That routine tests rectangle bits with base `x+2016*y`, then index
`base+2016*r+c`; byte addressing uses arithmetic shift right by 3
and mask `1 << (index & 7)`. Any occupied bit returns false before
writes; otherwise it sets every tested bit.
Nonpositive dimensions succeed without writes; no coordinate bounds checks
were observed in this routine.

At raw `0xE9ED35`, `0xE9CA60` searches y-major/x-minor for
free space and reserves it. Decompressed tables independently use
`table[i]+16*j`: raw `0xE9E652` reads a QWORD and `0xEA05C6`
writes one. The record-to-table mapping is not yet resolved.
The next scope connects coordinate conversion at `0xE9EE17..0xE9F2D9`
to table index j rather than inferring a direct correspondence.

## Retained replay tick callback is invoked synchronously by service slot +8

Concrete service vtable **index 8 (byte offset +0x20)** targets **VA** `0x1256590`,
raw `0xE55990`, with EBX=this.
Raw `0xE55B5C` bytes `8B 83 00 01 00 00` load `[this+0x100]`.
A test/null branch gates the call; `0xE55B66` pushes unchanged
`[ebp+8]`, `0xE55B69` bytes `FF D0` call it, and `add esp,4`
performs caller cleanup. The callback receives one 32-bit argument,
with no explicit context argument.

Earlier nonzero service `+0x12C` returns before this callback.
Observed order is a global-service virtual `+0xF8` call, retained
callback, then raw `0xE4FE10`.
This is synchronous on the service method's invoking thread; thread
identity remains unproven. No direct parent calls were found in the
bounded pass, so the next scope resolves an external indirect slot-8 caller.

## Texture coordinates map to row-major 16-byte entries by level

The active-sample trace establishes `j=(1024 >> L)*ty+tx`.
All instruction addresses here are raw.
At `0xE9EE17..0xE9EFAD`, placement coordinates become signed
32-bit `X=(placementX<<6) SAR L`, `Y=(placementY<<6) SAR L`;
tile starts truncate X/126 and Y/126 toward zero.
Division uses signed high-product multiplication by `0x82082083`,
addition, arithmetic shift by 6 and sign correction, not unsigned division.
Counts are `trunc((end+125)/126)-start`.
Level progression is L=6,5,...,0, giving widths 16,32,...,1024;
the inner loop increments tx and outer loop ty.

Raw `0xE9F2B6..0xE9F2E1` computes the row-major j and stores
it at a working object's `+0x20`. Alternate call path
`0xE9F333..0xE9F359 -> 0xEA0430` passes `(L,tx,ty)`;
`0xEA0451..0xEA046B` checks signed coordinates against zero and
`1024 SAR L`, branching separately for invalid values.
Table pointer is `[owner+0x1850+4*L]`, entry address table+16*j.
Raw `0xE9E64F..0xE9E65F` reads entry QWORD `+0`;
`0xEA05BD..0xEA05D3` writes it.
Raw `0xEA0352` compares DWORD `+8` with an XOR-derived value;
`0xEA0126` reads DWORD `+0xC` for allocation/read length.
No further semantic field names are established.
Arithmetic wraps at 32 bits and shifts use CL. The next check follows
the writer of entry `+8` to determine its stored-value construction.

## Run loop calls service vtable index 8, not byte offset +8

The preceding service callback section's original "slot +8" wording was
ambiguous: the resolved entry is **index 8, byte offset +0x20**.
Table **VA** `0x1C77C4C` contains **VA** `0x1256590`
(raw `0xE55990`). Run-loop raw `0x3099CD..0x3099DB`
loads alias **VA** `0x1E60EA4`, dereferences the object, loads its
vptr, and selects `[vptr+0x20]`; raw `0x3099DE` calls it.
Raw `0x3099DA` pushes one rounded/scaled timestep, subsequently
forwarded unchanged to the retained callback.
This closes the synchronous runloop -> service -> callback chain,
but does not by itself prove a main-thread identity.
The exact bootstrap callsite and timing-zero destination are undergoing a
narrow operand check; the prior local-only B78 interpretation remains the
authority until that check, not an asserted write to persistent app `+0xB70`.

## Startup-thread chain and precise timing operand corrections

The narrow operand check confirms bootstrap **call opcode** raw `0x2EB19E`
(VA `0x6EBD9E`), bytes `E8 1D E7 01 00`, targeting raw
`0x3098C0` (VA `0x70A4C0`). Earlier raw `0x2EB198`
denotes the preceding `mov ecx,[VA 0x1DFFD98]`, not the call.
At raw `0x309973`, `movss [esp+0x14],xmm2` zeros the
iteration-local stack multiplier. It does **not** modify persistent app
`+0xB70`. Raw `0x30996C` separately clears app byte `+0xB78`.
Service table entry **VA** `0x1C77C4C` is base `0x1C77C2C`
plus byte offset `0x20`, zero-based index 8.

The independently traced PE entry raw `0x136ADB0`
(VA `0x176B9B0`) calls initialization then jumps into CRT startup
raw `0x136ADBA`. CRT pushes four WinMain-shaped arguments and
calls wrapper raw `0x2E9630` (VA `0x6EA230`) at raw
`0x136AEDA`. Wrapper bytes `E8 3B 18 00 00 C2 10 00`
prove `call bootstrap; ret 0x10`, targeting raw `0x2EAE70`.
Together these checks join PE startup -> wrapper -> bootstrap -> runloop
-> service index-8 tick -> retained app callback synchronously on the
PE startup thread. Numeric thread identity is not known.

## Virtual-texture entry +8 stores XOR of three CRC32 results

For the active sample, raw `0xEA0560` receives ECX=request and
stack owner argument; L is sign-extended request byte `+0x24`,
j is request DWORD `+0x20`, and table is `[owner+0x1850+4*L]`.
Raw `0xEA073F` (VA `0x12A133F`) stores ESI to entry
`table+16*j+8`. Its exact construction is:
CRC32(buffer at request `+0x14`, `0x4000` bytes) XOR
CRC32(buffer at request `+0x18`, `0x4000` bytes) XOR
CRC32(buffer at request `+0x1C`, `0x4000` bytes).
The helper's 256 table entries match reflected IEEE CRC32, with
initialization and final XOR `0xFFFFFFFF`; table **VA** `0x1C788D0`
maps to raw `0x18774D0`.

Raw `0xEA0352` (VA `0x12A0F52`) compares the identical
32-bit construction against entry `+8`; equality succeeds.
This identifies a stored content-check value, without assigning further
buffer/channel semantics. The producer-address clarification resolves a typo:
raw `0xEA01E3`, bytes `E8 D8 82 FF FF`, calls raw `0xE984C0`
(VA `0x12990C0`) with `&request[0x14]` for the three buffer pointers;
the earlier `EA984C0` spelling is invalid. Current independent scope follows the
entry's QWORD `+0` and DWORD `+0xC` storage instead.

## Texture entries store data-file byte offsets and encoded byte lengths

The active-sample trace resolves entry QWORD `+0` as an absolute byte
offset into the per-level data file, not a file-pointer position.
Raw `0xEA05A6` (VA `0x12A11A6`) calls GetFileSizeEx on
handle `[owner+0xF58+0x48*L]`; its QWORD result is stored at
entry `+0` at raw `0xEA05CD/0xEA05D3`.
Raw `0xEA05DF` invokes the encoder, obtaining payload pointer/length;
`0xEA075D` passes them to `0xE97C40`. Actual WriteFile at
`0xE97CD9` uses OVERLAPPED low/high offsets both `0xFFFFFFFF`,
requesting append at EOF. Raw `0xEA0793` stores the unmasked DWORD
encoded byte count directly at entry `+0xC`.

The reader loads that count at raw `0xEA0126`, allocates exactly it,
and calls its read wrapper at `0xEA01C9`. ReadFile at `0xE97EC7`
receives entry `+0` as OVERLAPPED low/high byte offsets.
File object is owner `+0xF48+0x48*L`, handle at object `+0x10`.
Setup formats `%s%i` at raw `0xEA15A3` and opens at `0xEA1610`;
maintenance uses owner `+0x19A0` as prefix. Data files are therefore
prefix-plus-decimal-level, distinct from `%s_idx` index files.

Observed publication order is offset, CRC, append, length, size assertion,
then `[owner+0x19B0+8*L]=offset+length`.
EOF sampling precedes the append wrapper's lock. Same-owner concurrent-writer
exclusion and durable index publication remain unproved; the next scope
follows caller serialization and index-save order rather than assuming atomicity.

## Service +0x12C is a byte shutdown-request latch

The active sample's constructor raw `0xE4E058` and initialization
`0xE54C50` clear one byte at service `+0x12C`.
Vtable **index 2, byte offset +8**, target raw `0xE55580`
(VA `0x1256180`), sets it to 1 with
`C6 81 2C 01 00 00 01; C3`.
Tick raw `0xE559C0` tests the same byte; nonzero skips tick work
and returns AL=1. Application caller at `0x3099DE` interprets this
as `Offset renderer requested application shutdown.`
This supports shutdown-request semantics, not an ordinary pause flag.
Initialization clearing is proven; successful in-place resumption is not.
The next scope resolves conditions of callers of this specific object's
byte-offset-8 setter, distinct from its byte-offset-0x20 tick method.

## Texture caller saves the index before outermost data-file flush

Active-sample raw `0xEA95F0` (VA `0x12AA1F0`) calls owner-begin
`0xE9E200`, appends at `0xEA968F`, saves at `0xEA972E`,
then calls owner-end at `0xEA9757`.
Begin increments non-atomic owner DWORD `+0x1994`, acquiring the
filesystem lock at `+0x1998` only on transition 0->1.
Acquisition at raw `0xE9E293` retries `_wrename` to
`%s_tmp_lock_file`. This is not demonstrated same-owner thread exclusion.

The index save orders fflush at raw `0xE9E0A2`, _commit at
`0xE9E0B0`, fclose, destination _wunlink, then _wrename at
`0xE9E15A`. Write/flush/commit/close results are unchecked in the
inspected path; rename failure invokes the assertion callback then returns
success if it returns.
Data-file FlushFileBuffers happens at `0xE9E366`, only on outermost
owner-end and **after index publication**; its results are unchecked.
These observations do not establish crash-atomic publication or an exclusive
same-owner writer. The next scope follows dispatch into `0xEA95F0`
for concrete thread/mutex discipline.

## Renderer shutdown-request caller remains a bounded unresolved stop

The setter entry is confirmed at raw `0x1876834`, containing **VA**
`0x1256180`, vtable index 2 / byte offset 8.
No direct E8/E9 reference was found in raw `[0,0x1700000)`.
A bounded scan of 478 alias-immediate occurrences with limited linear
provenance (at most 150 instructions/512 bytes each) found no confirmed
same-object byte-offset-8 call. Branch/interprocedural coverage is incomplete,
so absence of a caller is not established.
The reported alias chain repeats already-resolved cell-address forwarding
and supplies no new concrete requester. This gap is parked; the next
independent lifecycle scope resolves the known virtual `+0xF8` phase before
the retained app callback in `0xE55990`, not another broad alias scan.

## Texture writer dispatch remains unproven after bounded reference checks

Raw `0xEA95F0..0xEA975F` has no IDA incoming references,
no direct .text E8/E9 calls/jumps into its body, and no exact raw/RVA/VA
pointer found in the inspected file-wide scan. An interior-VA match at
raw `0x2F362E` is another call's rel32 operand, not callback evidence.
This negative coverage does not establish unreachability.
Incoming ECX is saved at `0xEA95F9/0xEA9600` and reused as
owner for begin `0xEA960E`, save `0xEA972E`, end `0xEA9757`.
Counter updates at `0xE9E22A..0xE9E233` and `0xE9E32F`
are ordinary increment/decrement; same-thread or mutex exclusion remains
unproved. The dispatch gap is parked rather than repeating broad scans.
The next independent format scope starts at concrete decoder `0xE984C0`.

## Pre-callback global phase services oConsoleSystem

The global at **VA** `0x1E69348` is RTTI `oConsoleSystem`,
zero-filled virtual-tail .data without a raw file offset.
Constructor raw `0xE587A3`, bytes `C7 05 48 93 E6 01 A8 7E C7 01`,
installs vptr **VA** `0x1C77EA8` (raw `0x1876AA8`).
Vtable byte offset `+0xF8`, raw `0x1876BA0`, contains
**VA** `0x128AD80`, raw `0xE8A180`.
Renderer tick raw `0xE55B56`, bytes `FF 90 F8 00 00 00`,
invokes it before the retained callback at `0xE55B69`.

The method is reentrancy-guarded console servicing. It conditionally uses
timeGetTime and an elapsed interval before virtual `+0xDC`, then calls
raw `0xE89EC0` with buffer **VA** `0x1DFCAC4`.
Queued-console-command processing is an interpretation, not yet a complete
buffer/dispatch proof. The next scope resolves the helper's `+0x98`
dispatch and concrete buffer consumption.

## Texture payload decoder header, modes, and three-buffer ownership

Raw `0xE984C0` (VA `0x12990C0`) receives encoded pointer/byte
count N in ECX/EDX and stack arguments: three-output-pointer array,
optional scratch, scratch capacity. Reader call raw `0xEA01E3`
passes `&request[0x14],NULL,0`. It previously allocates/zeroes three
separate `0x4000` buffers at `0xEA0060..0xEA00C9`.
Decoder does not allocate or replace these output buffers.

| Encoded offset | Width | Observed use |
|---|---|---|
| `0` | byte | LZMA property for compressed modes. |
| `1` | LE DWORD | LZMA dictionary size. |
| `5` | LE 3-byte integer S | Scratch/output-size field. |
| `8` | byte | Outer mode. |
| `9` onward | variable | Payload; mode 16 first has three property bytes here, then payload at 12. |

Raw `0xE984FF..0xE98523` checks unsigned
`S-0x7801 <= 0x107FF`, hence `0x7801..0x18000`.
No explicit N>=9 guard precedes header access/N-9, and no mode-specific
equality with reconstructed output size was observed.
Dispatch table raw `0xE98D40` selects:
mode 0 (`0xE985F2`), LZMA to scratch then scatter/postprocess;
modes 1/15 (`0xE988E7/0xE989B3`), LZMA then three consecutive
`0x4000` copies; mode 13 (`0xE98970`), direct copies from encoded
offsets `9,0x4009,0x8009`; mode 16 (`0xE989D0`), three LZMA
streams to scratch offsets `0,0x3000,0x7800` with capacities
`0x3000,0x4800,0x4800`, then scatter. Other modes fail.

First codec call raw `0xE9862E` targets `0x12ABBD0`
(VA `0x16AC7D0`). Property parser `0x12ABA30` proves LZMA:
property p<225, lc=p%9, lp=(p/9)%5, pb=p/45, dictionary minimum
`0x1000`. This is distinct from index decoder `0x13090D0`.
Compressed modes reuse sufficiently large supplied scratch or allocate
exactly S; owned scratch is released at `0xE988BD..0xE988CB`.
Failure zeroes all three outputs and returns AL=0; success AL=1.
Reader releases encoded input at `0xEA03B3`; outputs remain attached
to the request, with eventual destructor not yet traced.

## Console helper consumes statements and updates the last-value variable

Raw `0xE8A044..0xE8A058` loads the **vptr** from static object
**VA** `0x1E69348`, then sets ECX to that object's address.
It does not load an object pointer from a pointer global.
Virtual byte offset `+0x98` at table **VA** `0x1C77EA8`
targets raw `0xE41660` (VA `0x1242260`), which tail-dispatches
through byte offset `+0x80` to raw `0xE41360`.
This updates a console-variable descriptor, including text/numeric values,
modification counters, and optional descriptor `+0x54` callback.
It is **not an event-queue dispatch**.

Here arguments are `(descriptor VA 0x1E6B670, output-or-empty,0,0)`.
Constructor raw `0x2D4870` identifies variable `_`, described as
`Last value returned from a console statement.` Pointer-identical normalized
output/current value skips the update.
Parser raw `0xE89C50` returns parserEnd-inputStart, retained in EDI.
At `0xE8A075..0xE8A0B7`, if `0<consumed<=buffer length`,
memmove shifts the remaining suffix to the front, cursor `+0x18` is
adjusted, `+0x1C` conditionally cleared, and the buffer resized.
Nonzero wait/deadline state preserves the tail; exhaustion or zero consumed
clears the buffer. Temporary output text is reference-count released at
`0xE8A11F`. The next scope resolves concrete wait/deadline producers.

## Texture mode zero fully writes three 32x32 grids of 16-byte records

For the active sample, mode-zero scatter raw `0xE98690..0xE9871E`
operates on decompressed buffer B and three output buffers O[j].
Each output is 1024 records of 16 bytes, exactly `0x4000` bytes.
No color-channel interpretation is established.

| Record bytes, i=0..1023 | Source/write |
|---|---|
| `0` | Reconstructed fourth residual sequence for output j. |
| `1` | Reconstructed third sequence. |
| `2..7` | Six bytes from `B+0x3000+j*0x1800+i*6`. |
| `8..9` | LE16 reconstructed from second sequence. |
| `0xA..0xB` | LE16 reconstructed from first sequence. |
| `0xC..0xF` | Four bytes from `B+j*0x1000+i*4`. |

Scatter consumes B offsets `0..0x77FF`. Destination advances by 16,
four-byte source by four, six-byte source by six; final record begins
at output `0x3FF0` and its last DWORD ends at `0x3FFF`.
At raw `0xE9873E`, a residual reader starts at B+`0x7800`,
bit position zero, persisting across twelve reads in output order 0,1,2.
Per output it decodes `0xC00` signed words for helper `0xE981F0`
at record `+0xA`, then `0xC00` for `+8`, then `0x400`
for helper `0xE983A0` at `+1`, then `0x400` for `+0`.

Both helpers reconstruct 32x32 grids, record stride `0x10`, row stride
`0x200`: first row predicts from left, later first columns from above.
Interior selects left if `(top-left-left) > (top-left-above)`, else above.
Triplet helper uses signed 16-bit differences and word-wrapping additions;
for signed triplet (a,b,c), it computes
`s=a-(c SAR 1)`, `t=s-(b SAR 1)`, then stores
`LE16(((b+t)<<11)|((c+s)<<5)|t)`, without separate component masks.
Byte helper adds signed residuals to prediction and clamps to 0..255.

Successful mode zero writes every byte of all three outputs; none depends
on omitted-write prezeroing. Residual failures accumulate even though later
helpers may still execute; final failure at raw `0xE98881` clears every
output and returns AL=0. Residual scratch is filled by its reader, not
explicitly initialized here. Bounds use returned decoded length
`[ebp-0x184C]-0x7800`; equality with header S and exact residual
bit grammar remain the next bounded check. Final bit-cursor exhaustion is
not required in the observed path.

## Console wait state is shared countdown/deadline storage; setters unresolved

The bounded active-sample check identifies shared globals, not console-object
fields: **VA** `0x1E49A74` is a signed pump-count countdown, used
at raw `0xE89EF8..0xE89F19`; **VA** `0x1E49A78` is a
deadline, used at raw `0xE89F1C..0xE89F4B`.
Unsigned `low32(clock64/1000) < deadline` blocks continuation;
equality resumes and clears the deadline. The comparison is not wrap-safe;
milliseconds are not yet proven as the clock unit.
Raw `0xE8A0BC..0xE8A0D2` preserves the compacted remaining
buffer if either state is nonzero and arg_4 is zero.

No nonzero setter/address-taking occurrence was found in the bounded pass;
computed/indirect writes were not exhaustively covered. This producer gap
is parked rather than assigning a guessed wait-command binding.
Adjacent verified `ExecSkip` registration at raw `0x2D4992/0x2D49AD`
targets **VA** `0x128A2E0`, raw `0xE896E0`, and writes
different global **VA** `0x1E49A70`, not either wait field.
The next concrete scope follows that handler and its actual consumer.

## Residual symbols use gamma-like codes with signed-16 truncation

Active-sample helper raw `0x1308A80` (VA `0x1709680`) is called
at raw `0xE9877E`, bytes `E8 FD 02 47 00`.
ECX points to `{tail,bitpos}`, EDX is actual decoded length L minus
`0x7800`, and stack arguments are word destination/count.
Physical bits are consumed LSB-first within bytes.
Each symbol reads k zeroes then 1, followed by k payload bits accumulated
most-significant first with initial accumulator 1, yielding positive integer U.
At raw `0x1308B44`, CWDE sign-extends **16-bit U-1** to s;
even s maps to s/2, odd s to `-((s+1)/2)`.
Ordinary logical codes `1,010,011` yield `0,-1,+1`.
This truncation must not be replaced with assumed full-range zigzag semantics.
No encoded-width cap is enforced; accumulation wraps at 32 bits.

Raw `0x1308AB0..0x1308ABE` checks the byte index only before
a complete symbol, with no further unary/payload-loop bounds checks.
Success means the requested word count was produced, not that every
internal bit read was bounded. Updated bit position is always stored.
LZMA wrapper raw `0x12ABCF3` writes actual decoded length L back
through `[ebp-0x184C]`. Header S is capacity: success does not require
L=S. Residual bounds use L-`0x7800`, not S-`0x7800`.
The next check follows the matching writer from the known texture encoder
to establish intended signed mapping and maximum width.

## ExecSkip skips parsed segments, not only executable commands

The concrete active-sample chain is registration raw `0x2D4992/0x2D49AD`
-> handler `0xE896E0` -> shared global **VA** `0x1E49A70`
-> queue consumer raw `0xE89F70`.
Handler requires argument count at least two; missing count prints syntax
without assignment. Helper `0xDD5C00(0)` converts the first argument
using verified MSVCR120!atoi; extra arguments are ignored.
Raw `0xE896FA` replaces the shared count, not adds to it.
Only signed-positive values activate skipping.

Each skip still calls parser raw `0xE89790`, decrements the count at
`0xE89F8B`, bypasses dispatcher `0xE89C50`, and consumes the
parsed prefix. Unquoted semicolon/newline terminates a segment; leading
`//` consumes its line. Parsing/substitution still happens.
Thus "next N buffered parser segments" is more precise than "N commands."
The tail executes normally once the count reaches zero.
Buffer exhaustion clears surplus count at raw `0xE8A0F0`, rather
than carrying it into subsequently supplied text. Wait-tail exit and early
wait returns bypass that clear and preserve state.
The next scope identifies concrete `0xE89EC0` callers and their buffers
to bound shared-state interleaving; no concurrent runtime occurrence is claimed.

## Inverse residual writer exists; predictor pairing remains unproved

Raw `0x1308970` (VA `0x1709570`) is a concrete inverse bit writer.
At `0x13089A0..0x13089B8` it loads/sign-extends a word d,
computes m=2*d for nonnegative d or m=-1-2*d for negative d, then
U=m+1. Raw `0x13089D0..0x1308A53` emits k zeroes, one,
and k MSB-first suffix bits, with physical byte bits advanced LSB-first.
Full signed-16 inputs permit k<=16 and at most 33 bits per symbol;
no destination-capacity checking was observed.

The decoder's DEC/TEST/CWDE/SAR-or-NEG at raw
`0x1308B41..0x1308B4E` means exact arithmetic round-trip only
for d in [-16384,16383], at most 31 bits. Exhaustive local arithmetic
verification reported 32,768 mismatches outside that interval. This is
not proof that texture predictors constrain residuals to that range, nor
proof that this writer is the texture predictor's producer.
No direct calls or literal pointers to the writer were found in the
bounded pass. Known texture call raw `0xEA05DF -> 0xE993B0`
reaches indirect thunk `0x15D2614` at `0xE9942E`.
The next scope resolves that thunk/import or callback before asserting
an encoder-to-predictor linkage.

## Console file buffers share skip state but bypass waits

The active-sample mapped-byte CALL-rel32 scan found three direct callers
of raw `0xE89EC0`; IDA xrefs missed two undefined-code sites.

| Caller / callsite (raw) | Buffer | arg4 |
|---|---|---|
| `0xE8A180 / 0xE8A1EB` | Global **VA** `0x1DFCAC4`. | 0 |
| `0xE8A850 / 0xE8A994` | Invocation-local pre-push ESP+0x20, label `ProcessFile` (corrected below). | 1 |
| `0xE8A9F0 / 0xE8AB3A` | Separate invocation-local pre-push ESP+0x20, label `PrintConfig`. | 1 |

First local caller allocates storage at `0xE8A8A2`, stores it at
buffer `+8`, and calls loader `0x12BB660` at `0xE8A928`.
The loader uses _fileno, _fstat64, fread, fclose. Buffer vtable **VA**
`0x1C77A30` (raw `0x1876630`) accessor `0xDF47F0`
returns backing pointer plus offset. Cleanup releases backing storage at
`0xE8A9AE`. Second caller independently allocates at `0xE8AA43`,
loads at `0xE8AAC9`, releases at `0xE8AB5E`.

All three use absolute-global skip check `0xE89F70` and decrement
**VA** `0x1E49A70`; no caller saves/restores it. Preserved skip
state can therefore affect another buffer on a subsequent invocation.
arg4=1 bypasses wait entry checks and wait-tail suspension, but not skipping.
Normal exhaustion clears skip, potentially clearing another buffer's
preserved remainder count.
This proves shared state, not actual interleaving/concurrency.
Normal caller's non-atomic guard does not establish mutual exclusion with
either local caller. The next scope resolves their registrations/invocation.

## Texture encoder thunk is the ordinary VCOMP120 _vcomp_fork import

In the active sample, raw `0xE9942E` bytes `E8 E1 91 73 00`
call raw `0x15D2614`; following `83 C4 24` cleans 36 stack bytes.
Thunk bytes `FF 25 B0 4C AC 01` jump through IAT **VA**
`0x1AC4CB0`, raw `0x16C38B0`.
The PE descriptor for VCOMP120.DLL has FirstThunk RVA `0x16C4C94`;
entry 7 names `_vcomp_fork`. This is a normal PE import, not dynamic
resolution or raw numeric `0x1AC4CB0` file backing.

Assembly supplies nine 32-bit stack arguments; the decompiler rendered only
eight, so its call signature is not authoritative. Reported leading values
are 1 and 6, followed by an outlined callback and six data arguments.
The callback/data labels require exact raw/VA operand clarification before
assigning their target or roles. Local pointers refer to stack storage;
no ownership transfer is visible, and retention beyond the call is not
established by that observation alone.
The next scope resolves the actual outlined callback and one encoder operation,
not an assumed connection to residual writer `0x1308970`.

## OpenMP callback maps to raw 15A1BA0 and copies three planes for selector zero

Raw `0xE99425`, bytes `68 A0 27 9A 01`, pushes callback
**VA** `0x19A27A0`, hence raw `0x15A1BA0`, not raw `0x19A27A0`.
The fork's nine arguments are
`(1,6,callback,&var_5C,&var_50,0x1C9DB,&var_38,incomingEDX,stackArg0)`.
The outlined callback consumes six stack parameters; inferred decompiler
signatures are unreliable here.
Selector zero copies three `0x4000`-byte planes to contiguous scratch;
calls at raw `0x15A1CD2/0x15A1CE9/0x15A1D00` resolve to
imported memcpy. Raw `0x15A1D86` then calls `0x12A9E30`.
No direct callback call to residual writer `0x1308970` was found;
transitive pairing and selector-to-outer-mode identity remain unproved.
The next scope follows output encoding through `0x12A9D70 -> 0x12A9BD0`.

## PrintConfig binds to immediate local execution; ProcessFile label corrected

Active-sample console vptr **VA** `0x1C77EA8` has byte-offset
`+0xE8` at raw `0x1876B90`, containing **VA** `0x128B450`
(raw `0xE8A850`), and `+0xF0` at raw `0x1876B98`,
containing **VA** `0x128B5F0` (raw `0xE8A9F0`).
The earlier BufferFile label for `0xE8A850` was incorrect:
raw `0xE8A8E3` references `ProcessFile`, **VA** `0x1BE26C4`.
BufferFile is instead raw `0xE8A6C0`, byte-offset `+0xE4`.

PrintConfig registration raw `0x2D4C92` supplies its name at **VA**
`0x1BE278C`; `0x2D4CAD` stores callback **VA** `0x128BD00`
(raw `0xE8B100`), and `0x2D4D09` links its descriptor into head
**VA** `0x1E48990`.
Callback requires `[args+0x208]>=2`, obtaining filename from base
`[args+0x20C]` plus the second argument offset from `[args+0x204]`.
Raw `0xE8B13C` pushes that pointer; `0xE8B147` invokes console
virtual byte-offset `+0xF0`, reaching `0xE8A9F0`.
It forwards the filename to path helper `0x1318870`, loads through
`0x12BB660`, then calls `0xE89EC0` at `0xE8AB3A` with
local buffer and flag 1. This is the concrete immediate-execution chain.
Filename normalization and thread identity remain unverified.

No concrete registration/caller of ProcessFile was established. That gap
does not justify attributing unrelated `+0xE8` calls to this object.
The next concrete scope follows BufferFile at `0xE8A6C0` instead.

## BufferFile copies loaded text into the global console queue

The active sample supplies a concrete caller at raw `0xE4E2BB`,
bytes `FF 96 E4 00 00 00`, on static console object **VA**
`0x1E69348`. Raw `0xE4E210` formats `!%s.con` (literal raw
`0x17DD31C`) before that call and ignores its result.
BufferFile byte-offset `+0xE4` targets raw `0xE8A6C0`;
at `0xE8A7F8` it invokes console byte-offset `+0xE0`,
target `0xE8A580`, which selects global buffer **VA** `0x1DFCAC4`.

BufferFile owns a temporary loaded by `0x12BB660` in rb mode.
Complete nonempty reads add a trailing NUL; empty input keeps its initial NUL.
Queueing copies strlen(temp) bytes, so an embedded NUL truncates queued text.
The temporary is released at raw `0xE8A813`.
Helper `0xDF3210` writes at the queue's current cursor without resetting it;
ordinary mode appends LF at `0xE8A616`, NUL at `0xE8A62C`,
then backs the cursor onto the terminator. Text mode may add indentation.
Path rejection, open/status failure, or short read returns false without
queueing. After invoking the queue, BufferFile returns true without checking
queue errors; that success does not prove allocation/write success.
The next scope resolves the formatted filename's startup source and ordering,
without guessing the meaning of its leading exclamation mark.

## Encoder helper writes a five-byte property/dictionary prefix

Active-sample raw `0x12A9CD0` (VA `0x16AA8D0`) requires
prefix capacity at least five, sets it to five, writes property
`b0+9*(b1+5*b2)` from object bytes `+0x32598/+0x3259C/+0x325A0`,
then an unaligned DWORD at output+1. The dictionary-like DWORD derives
from object `+0x3BD08`, minimum 4096, rounded to a 2x/3x power-of-two
bucket. Five is the prefix length, not total decoded output size.
Raw `0x12A9E30` initializes a processing object, writes this prefix,
then invokes `0x12A9D70` and releases the object. Prefix failure skips
processing. Its fallback `0x12A9D70 -> 0x12A9BD0` subtracts
consumed input from the input-count pointer; it is not the prefix's output
length publication. The optional processing callback is null here.
Selector zero is proven as a staging branch only, not outer mode zero.
Codec implementation identity and outer publication remain unproved by this
pass; the next scope follows status/count pointers from raw `0x15A1D86`
into candidate selection.

## Startup attempts console configuration twice, with processing inside init

Raw `0xE4E210` takes no parameters. Its filename source is
`[VA 0x1E6A684]+0x1C`, transformed by `0x13192D0 -> 0x3A5910`,
not a caller-supplied filename.
Formatting `!%s.con` occurs at raw `0xE4E2AD`, then BufferFile
at `0xE4E2BB`; raw `0xE4E327` immediately invokes console
virtual `+0xF8`, raw `0xE8A180`, which attempts queue processing.
Execution is therefore not necessarily deferred until the main loop, though
reentrancy/wait/countdown gates can postpone it.

Service initialization raw `0xE54BF0` orders:
callback(0), conditional configuration load/process, `0xE4E350`
command-line --cmd execution, `0xE52650` (Initializing Debug Menus),
conditional configuration load/process again, callback(1).
Both config gates use service byte `+0xC9`, copied from descriptor
`+0x11`; normal startup sets it to 1 at raw `0x306E1C`.
Each pass rereads the global source, so identical filenames are not guaranteed.
Concrete basename and leading-! semantics remain unresolved.
Literal settings at raw `0x17DD9E4` (VA `0x1BDEDE4`) is not
yet bound to this source. The next check follows registration around raw
`0x2D2EF3..0x2D2EF8` to global **VA** `0x1E6A684`.

## Texture selector zero publishes outer mode one and enters minimum-size selection

Raw `0x15A1D2C` initializes candidate count to capacity.
At `0x15A1D86`, `0x12A9E30` receives payload destination buffer+9,
count pointer, staged `0xC000` input, properties, prefix destination
buffer, and prefix-length pointer initially 5.
Output callback raw `0x12A9A50` copies/advances bytes and decreases
remaining capacity; `0x12A9E0D` publishes capacity-minus-remaining.
Overflow sets status 7; allocation failure returns 2.
Raw `0x15A1D8E` reports nonzero status, but if the reporting function
returns, header/count publication still proceeds.

Raw `0x15A1DB2` writes DWORD `[buffer+5]=0x0100C000`,
proving LE24 size `0xC000` and mode byte **1**.
Raw `0x15A1DB9` adds nine header bytes to payload count.
The dispatch table maps selector zero to this branch: **selector 0 produces
outer mode 1, not mode 0**.
Raw `0xE99454` compares three candidate lengths signed, selecting strict
improvements and preserving earlier ties, without separate status filtering
observed. Raw `0xE99756` frees staging buffers; `0xE9976B` frees
losing encoded buffers. `0xE99861..0xE9986B` transfers winning
pointer/count to the incoming destination pair.
Raw `0xEA075D` passes it to the already-traced append wrapper;
`0xEA0793` records count and `0xEA0797` releases the winning buffer.
The next scope resolves selector 1/2 header/mode construction.

## Startup basename is registered as eng.autoSaveConfigName, default settings

Raw `0x2D2ED7`, bytes `B9 28 A6 E6 01`, selects console-variable
object **VA** `0x1E6A628`.
Raw `0x2D2EF2` pushes default settings at **VA** `0x1BDEDE4`;
`0x2D2EF7` pushes name eng.autoSaveConfigName at **VA**
`0x1BDEDF0`. Constructor raw `0xDD5158` stores self to object
`+0x5C`, the alias cell **VA** `0x1E6A684`; `0xDD5166`
copies the default into object `+0x1C`.
This binds settings to the filename source rather than merely locating a literal.
Raw `0xE54E10` pushes the same object and `0xE54E15` invokes
console byte-offset `+0x98`, reaching `0xE41360`; `0xE41555..0xE41558`
copies replacement text into `+0x1C`.
Whether transformation `0x13192D0 -> 0x3A5910` changes that basename
and what leading ! means remain the next concrete path-resolution scope.

## Configuration path transform prepends manager prefix; ! bypasses root search

Active-sample raw `0x1319350` normalizes slash to backslash.
For ordinary input, raw `0x1319488..0x131948E` concatenates
path-manager `+4` prefix and input into result `+4`; result `+0`
retains normalized input. Raw `0x3A5926/0x3A5934` copies result
`+4` without another transformation.
Thus settings is not renamed, but the formatted source is prefix+settings,
not necessarily the bare basename.

In BufferFile path helper, raw `0x131892F` recognizes leading `!`;
`0x1318938` skips exactly that character, retaining the remaining
normalized path at result `+4`. Root-list searching belongs to the non-!
branch. Prefix matching may populate result `+0` but does not replace `+4`.
Raw `0xE8A7C0` copies result `+4` for loading.
This proves bypass semantics, not a particular directory. The next scope
resolves manager `+4` initialization before joining it to earlier write-prefix
evidence. The final import probe in this bounded pass failed; no import
conclusion relies on that failed probe.

## Candidate ordinals one/two use selectors 15/16, with distinct layouts

Worker tables raw `0x15A23B8/0x15A23CC` (VAs
`0x19A2FB8/0x19A2FCC`) map candidate ordinals 1/2 to selectors
15/16, not literal selectors 1/2 (which dispatch to default).
Mode 15 compresses `0xC000` bytes at raw `0x15A1EFC` into B+9;
`0x15A1F28` stores `[B+5]=0x0F00C000`, count=payload+9.

Mode 16 scatter at raw `0x15A2080..0x15A213F` separates each
16-byte block into source fields `+12:4`, `+2:6`, `+8:4`, `+0:2`.
Across three `0x4000` inputs, scratch regions are
`0..0x3000`, `0x3000..0x7800`, `0x7800..0xA800`,
`0xA800..0xC000`. Calls at `0x15A21D2/0x15A2273/0x15A2314`
compress lengths `0x3000/0x4800/0x4800` contiguously from B+12.
Raw `0x15A234F/0x15A2351` copies first-stream five property bytes;
`0x15A235A` stores `[B+5]=0x1000C000`; per-stream first property
bytes go to B+9/+0xA/+0xB at `0x15A2364/0x15A2370/0x15A237A`.
The reported returned count is sum(compressed lengths)+24, while
constructed data ends at B+12+sum. This discrepancy requires the next
producer/consumer operand check, not an assumed padding explanation.

Both alternate candidates conditionally preprocess when the worker's final
argument is less than 3. Helper raw `0xE98D70` applies index-DWORD
replacement via `0xEABE8D` and six-byte index replacement via
`0xEAC612/0xEAC615`, preserving endpoint fields.
Color/alpha-index interpretation was reported; exact transform semantics
and residual-writer linkage remain unproved beyond the observed field writes.

## Ordinary configuration read prefix is captured-CWD\localassets\

The active-sample trace joins the read-path prefix to concrete startup stores.
Raw `0x13178EA` (VA `0x17184EA`), bytes `E8 A1 F5 F9 FF`,
captures CWD through `0x12B6E90`; IAT **VA** `0x1AC431C`
is verified GetCurrentDirectoryW.
Normal setup raw `0x10D8BCE`, bytes `B9 7C 35 C0 01`,
selects localassets. Raw `0x13179FE` concatenates that relative
directory with the captured root; `0x1317A06/0x1317A10`
assign manager `+4`, with normalization adding a trailing backslash.
Raw `0x1319488` supplies that prefix for basename transformation.

Thus the ordinary/default settings input resolves to
`<captured-CWD>\localassets\settings.con`, matching the independently
traced write-prefix behavior. The leading ! bypasses root-list searching,
not this already-prepended physical prefix. Actual launch CWD remains
unknown, and changed basename/descriptor configuration is not asserted
to take the default path. Normal descriptor `+0x18` null is already
documented; the next concrete scope is intervening --cmd execution.

## Mode-16 count overstatement and per-stream budget are instruction-confirmed

The follow-on initially mislabeled raw IDB addresses as VAs. Correct locations:
raw `0x15A237D/0x15A2383/0x15A2386` map to VAs
`0x19A2F7D/0x19A2F83/0x19A2F86`; raw `0xE989DB`
maps to VA `0x12995DB`. Subtracted locations `0x11A177D/0xA97DB`
contain unrelated bytes and must not be used.
At the correct producer locations, `2B 34 FA` subtracts B from the
final cursor, `83 C6 0C` adds 12, and `89 74 FA 04` stores
the result. This confirms count=sum(compressed lengths)+24 while
constructed extent is B+12+sum, a 12-byte overstatement.
No padding-initialization or runtime consequence is established here.

Consumer raw `0xE989DB` adds 12 to skip its header. Each stream
advances the input cursor by the decoder-updated input count.
The available budget is saved as incoming EDX-9 at
`0xE984DA/0xE984F3`; raw `0xE98A97/0xE98AF0/0xE98B4E`
reuse that same budget without subtracting prior consumption.
No final input-end equality check was observed after stream three.
The current scope resolves wrapper/internal termination semantics before
assigning meaning to unused trailing bytes.

## Startup --cmd dispatches the first statement of each matching option directly

Raw `0xE4E350` calls `0x12BE760` to collect exact case-sensitive
cmd entries from argument-pair vector **VA** `0x1E004FC`, in order.
Option parser `0x12BD190` requires `--name=value`, splitting at
the first equals sign; matching outer single/double quotes are removed,
mismatched quotes retained. Bare --cmd does not consume the next argument.
Raw `0xE4E3CD..0xE4E3E9` passes each value with format `%s\n`
to console byte-offset `+0xD4`, raw `0xE8A200 -> 0xE89C50`.

Tokenizer `0xE89790` separates unquoted whitespace; either quote
toggles the same quoting state, backslash escapes quote/backslash, and
unquoted semicolon or any LF terminates parsing. This chain dispatches
only the first statement, not its remainder; repeated --cmd options dispatch
separately. Callbacks run directly at raw `0xE89D97`.
Variable assignments invoke console `+0x98` at `0xE89E1A`
with `(variable,value,1,0)`. Flags can reject or latch writes; shared
**VA** `0x1E49A59` selects logging-only behavior.
Thus changing eng.autoSaveConfigName before pass two is structurally possible,
not yet unconditional. Current scope resolves its concrete flags/lookup and
normal logging state. The offset clarification confirms 28 decimal means
hex `+0x1C`, not `+0x28`: raw `0xE4E237` bytes
`A1 84 A6 E6 01` load the alias, and `0xE4E243` bytes
`8B 40 1C` read its object's current text.

## LZMA wrapper publishes consumed input separately from produced output

The active-sample wrapper raw `0x12ABBD0` starts
`55 8B EC 81 EC A0 00 00 00` (VA `0x16AC7D0`).
It saves output capacity from *a2 and available input from *a4, clears
both counts, rejects input budgets below five, then restores input budget
before decoder dispatch. Decoder updates input consumption while reducing
its local remaining budget. Returned *a2 is output produced; *a4 is
input consumed, not the original available budget.
The bounded report examined a separate caller `0xEB3840/0xEB388C`;
that caller's produced-length equality and missing consumed-length equality
must not be attributed to mode 16. Its exact mode-16 call/status/end-marker
checks remain the next narrower scope.

## Basename flags allow immediate updates; exact registry publication still open

Registration raw `0x2D2EEB`, bytes `6A 00`, supplies flags zero
for eng.autoSaveConfigName. Constructor `0xDD513E` stores them at
descriptor `+0xC`. Canonical `0x200`, variable `0x10`, and latch
`0x400` gates at `0xE413CF/0xE413FD/0xE414C3` are all clear;
assignment at `0xE41555..0xE41558` changes current text `+0x1C`.
Fallback lookup `0xE4098F..0xE409C7` compares the unique suffix
autoSaveConfigName after the first dot; full-name lookup uses registry
**VA** `0x1E692CC`. Publication of this exact descriptor to that
registry remains unproved by this pass.

Logging-only byte **VA** `0x1E49A59` is loader-zeroed virtual-tail
.data, not the unrelated raw-mapped byte 05. Its initial zero selects
normal dispatch at `0xE89CAA`; setter `0xE89EB6` writes it.
PrintConfig brackets its work with 1/0 at `0xE8AB2B/0xE8AB45`.
Once lookup resolves the descriptor, --cmd can therefore change the
basename before pass two; flags do not block it. Exact registry insertion
and startup ordering are the next check, not assumed lookup success.

## Mode-16 accepts zero-error decoder completion without exact produced lengths

The active-sample mode-16 calls to raw `0x12ABBD0` are:
raw `0xE98AD1` bytes `E8 FA 30 41 00`,
`0xE98B2F` bytes `E8 9C 30 41 00`, and
`0xE98B8D` bytes `E8 3E 30 41 00`.
Capacities are `0x3000/0x4800/0x4800`; nonzero EAX goes to
`0xE98881`, clearing all three outputs and returning AL=0.
No exact-produced-length comparison appears between calls.

Core raw `0x12AB350` reports status 1 for terminal marker with zero code,
2 at capacity, 3 for insufficient input, and 4 for capacity with zero pending
length/code. Wrapper `0x12ABCDD..0x12ABCE5` converts otherwise
successful status 3 to error 6; statuses 1/2/4 remain successful.
The branch then performs fixed reconstruction and returns AL=1 through
`0xE988BD..0xE988E6`.
The next scope checks scratch initialization and fixed read extents rather
than assuming success guarantees all expected scratch bytes were produced.

## Mode-16 outer dispatch bounds scratch but does not initialize it

Raw `0xE984C0` reads the low 24 bits of the header DWORD at
`[ECX+5]` as the declared extent and the high byte as the mode. It admits
extents `0x7801..0x18000` and modes `0`, `1`, `13`, `15`, and `16`; other values
take the failure path at `0xE98881`. Mode dispatch is an indirect jump.

For an admitted mode, the second stack argument is treated as an optional
scratch pointer and the third as its available size: the function reuses the
supplied pointer only when nonnull and the declared extent does not exceed
that size. Otherwise it requests a buffer sized to the declared extent via
`0x12C8A20`; a successful temporary allocation is released through
`0x12C8D60` after dispatch. The caller-side mode-16 fixed reads are not
identified by this outer wrapper alone.

No scratch clearing is visible in raw `0xE984C0` before the mode handler.
Its `0xE98881` failure path instead zeroes three separate output pointers,
each for `0x4000` bytes, sets the return byte to zero, and releases a
temporary scratch allocation if one was obtained. On a successful dispatch,
the wrapper sets the return byte to one and frees only its temporary
allocation; it has no produced-length equality check here. Consequently,
this function does not establish whether bytes beyond each LZMA-produced
prefix were preinitialized elsewhere or which bytes the mode-16
reconstruction reads. The indirect mode-16 handler and its allocation
helper remain the next bounded checks; do not infer fresh-allocation
contents or a runtime outcome from this wrapper.

## Basename descriptor enters the pending list and startup console registry

Constructor raw `0xDD51EB` loads pending head **VA** `0x1E48990`,
stores it at descriptor `+0x14`, then replaces the head with this descriptor.
Flag-4 exclusion is clear for eng.autoSaveConfigName.
Service startup raw `0xE54C73/0xE54C79` supplies that pending head
to console virtual `+0x10`, **VA** `0x1242B00`.
The walker follows `+0x14`; this descriptor's type method raw
`0xDD5E70` returns 1, selecting console `+0x18`, **VA** `0x12426B0`.
Raw `0xE41BBC..0xE41BC6` passes name/descriptor to registry
**VA** `0x1E692CC` helper **VA** `0x11F3770`.
Raw `0xDF2BFB` publishes its bucket node and `0xDF2C19`
stores the descriptor payload.

Initializer-table **VA** `0x1AC7874` contains registry initializer
`0x6D2B20`; descriptor initializer `0x6D3AD0` occurs later at
table **VA** `0x1AC78E8`.
An existing non-placeholder same-name descriptor instead takes alias branch
raw `0xE41AF1`, writing descriptor `+0x5C`; publication does not
guarantee that the original object becomes canonical.
The next scope resolves console `+0xBC` at raw `0xE54DBA`
and dispatch-readiness ordering before startup configuration/--cmd processing.

## Console startup +0xBC target: corrected vtable arithmetic

Startup raw `0xE54DBA` bytes `FF 90 BC 00 00 00` calls
console vtable byte offset `+0xBC`, after registry registration and before
the configuration/--cmd passes. Correct table backing is raw `0x1876AA8`;
adding `0xBC` yields raw `0x1876B64`. A subsequent byte check corrects
the initially reported target: bytes `C0 BC 25 01` contain **VA**
`0x125BCC0`, target raw `0xE5B0C0`, not `0x125BDC0/0xE551C0`.
The initial report used raw `0x187C564` by arithmetic error and interpreted
unrelated bytes as an import thunk. That target/import interpretation is
discarded; no readiness behavior follows from it.
The actual body and formatting-only effect are recorded below.

## Mode-16 fixed reconstruction reads scratch through exclusive offset C000

The active-sample post-decode scatter is independent of returned produced
lengths on the successful-call path. Each output contains 1024 sixteen-byte
records; scratch ranges below are inclusive and relative to S.

| Output record field | Width / stride | Output 0 | Output 1 | Output 2 |
|---|---|---|---|---|
| `+0xC` | 4 / 4 | `0000..0FFF` | `1000..1FFF` | `2000..2FFF` |
| `+2` | 4+2 / 6 | `3000..47FF` | `4800..5FFF` | `6000..77FF` |
| `+8` | 4 / 4 | `7800..87FF` | `8800..97FF` | `9800..A7FF` |
| `+0` | 2 / 2 | `A800..AFFF` | `B000..B7FF` | `B800..BFFF` |

Raw `0xE98C1B` bytes `8B 44 38 FC` and `0xE98C26`
bytes `8B 47 FC` load DWORD planes. Six-byte planes use DWORD
loads at `0xE98C78/0xE98C9E/0xE98CBC` and word loads at
`0xE98C85/0xE98CAA/0xE98CD0`; two-byte planes use word
loads at `0xE98CE8/0xE98CFF/0xE98D16`.
Loop raw `0xE98D24..0xE98D32` increments destination offset by
`0x10`, continuing while less than `0x4000`; last record starts
at `0x3FF0`. Combined scratch reads end exclusively at S+`0xC000`.
Raw `0xE98D39` jumps to successful cleanup `0xE988BD`.
This establishes fixed read extents, not a runtime malformed-input outcome.

## Console startup +BC increments formatting depth, not readiness

Correct target raw `0xE5B0C0` (VA `0x125BCC0`) consists of
`FF 05 10 9A E4 01 C3`: increment DWORD **VA** `0x1E49A10`,
then return. It consumes no stack arguments and has no callees.
Startup raw `0xE54DBA` invokes it with the console object's address
in ECX before configuration/command processing.
Raw `0xE5ABA6` multiplies this counter by a configured width; later
memset prefixes output, capped at 32 characters. This is formatting depth,
not a dispatch-readiness gate or callback.
The next scope resolves paired decrement/reset and width binding.

## Tile records own buffers until explicit pending drain cleanup

The active-sample reader raw `0xE9FFF0` receives caller-provided
`0x28`-byte tile-record storage, not a proven refcounted request object.
Wrapper `0xEA0430` invokes it. Cleanup helper raw `0xEA0860`
(VA `0x12A1460`) loops three times from record `+0x14`:
`0xEA0889` calls free `0x12C8D60`, then `0xEA088E` nulls
the field, advancing four bytes. Thus `+0x14/+0x18/+0x1C`
are individually released; the same loop frees `+8/+0xC/+0x10`.
This is explicit buffer cleanup, not a demonstrated C++ destructor.

Concrete producer raw `0xE9F359` invokes the reader wrapper;
`0xE9F360` calls `0xE9FEA0` to prepare the other three buffers.
Raw `0xE9F538` calls `0xEA9D70`; `0xEA9DF0..0xEA9E07`
shallow-copy exactly `0x28` bytes without observed reference increments.
Drain raw `0xE9F739` calls encoder `0xEA0560`, followed by
cleanup `0xEA0860` at `0xE9F740`, then advances by `0x28`.
This supports practical ownership retained by the pending collection until
drain; shallow copying alone does not establish shared ownership.

Reader failure exits retain buffers. Separate direct caller `0xEA95F0`
tests record byte `+0x25`; its false branch frees/nuls three buffers
starting at `0xEA96F0`. Allocation-exception cleanup and exact source
type remain unresolved. The next scope examines enclosing unwind and
pending-collection destruction rather than repeating the normal release loop.

## Console depth decrement/reset are conditional; width binding remains open

Vtable entries following +BC contain **VAs** `0x125BCD0/0x125BCF0`,
raw `0xE5B0D0/0xE5B0F0`.
Decrement checks `[ecx+4]!=0`, then decrements global depth and clamps
negative to zero; otherwise it returns unchanged. The next method clears
depth only with the same object-field condition.
Separate reset raw `0xE5D13E` explicitly stores zero to **VA**
`0x1E49A10`; cleanup branches converge before setting object `+4`
to 1 and invoking an indirect call. A matched startup increment/decrement
pair is not established by this pass.

Formatter raw `0xE5ABA6` multiplies depth by DWORD `+0x3C`
of the object reached through **VA** `0x1E6AC44`, caps prefix
length at 32, and uses configured/fallback character.
Names con.indentAmount / con.indentChar at raw
`0x17DE398/0x17DE3D0` are identified, but adjacent 2/space values
are not yet proven defaults. The next narrow scope binds their registration.
The report's renewed uncertainty about startup +BC does not invalidate the
already byte-verified target above; no new contrary operand was supplied.

## Pending tile-record unwind frees collection backing, not nested buffers

Enclosing function raw `0xE9E6B0` (VA `0x129F2B0`) has pending
collection at `[ebp-0x14A4]`; state becomes 1 at raw `0xE9EDF2`.
EH metadata raw `0x1975F08` (VA `0x1D77308`) has magic
`0x19930522` and two states. State 1->0 action **VA** `0x1A62C29`
is raw `0x1662029`, whose bytes
`8D 8D 5C EB FF FF E9 CC 7C 84 FF` select the collection
and tail-jump to raw `0xEA9D00`.
That cleanup frees collection backing and zeroes its three pointers,
without record traversal or nested-field release.
Normal drain therefore remains the proven nested-buffer reclamation path;
this unwind handler alone does not establish it.
The next scope checks one concrete exception-capable path between enqueue
and drain rather than inferring a throw from allocation names.

## Pending growth can reach std::_Xbad_alloc if the allocation reporter returns

The active-sample enqueue raw `0xE9F538` calls `0xEA9D70`;
loop branch `0xE9F65E -> 0xE9F160` permits another iteration
before drain. A full collection grows at `0xEA9DE4 -> 0xEAA5A8
-> 0xEAADF0`; allocation at `0xEAAE38` is followed by null test
and conditional call through **VA** `0x1AC4424` at
`0xEAAE45..0xEAAE49`. PE import resolves to
MSVCP120!std::_Xbad_alloc, not an inferred allocator name.
EH state remains 1 through this window. Its handler releases collection
backing only; existing records' nested-buffer cleanup is not reached.

This is conditional evidence: allocator null handling first invokes reporter
through **VA** `0x1C15EA0` at raw `0x12B8006`.
Whether it returns to the null/_Xbad_alloc path or terminates remains open.
The next scope resolves that reporter; no unconditional runtime throw or
observed resource consequence is claimed.

## Indentation consumer aliases are known; raw-string xrefs do not bind defaults

Formatter raw `0xE5AB97..0xE5ABBB` consumes amount through alias
**VA** `0x1E6AC44`, DWORD object `+0x3C`, multiplying depth
and capping at 32. Character at `0xE5ABD4..0xE5ABEB` comes
through alias **VA** `0x1E6ABD4`, text object `+0x1C`, taking
its first byte; fallback literal is **VA** `0x1C5EA68`.
The reported byte 0x12 at the numeric VA is not accepted as a literal
value without raw mapping; that address is a previously verified empty string.

No registration binding was found by raw-string xrefs. This is a raw-IDB
limitation, not evidence that constructors are absent.
The pass also miscomputed `0x17DE398+0x401400` as `0x181F798`;
the correct stored string VA is `0x1BDF798` (and con.indentChar
raw `0x17DE3D0` maps to VA `0x1BDF7D0`).
The next narrow check follows self-alias object constructors and stored
VAs, not another raw-string xref scan or adjacent-default inference.

## Indentation defaults and flags are bound by exact constructors

The active-sample CRT initializer raw `0x2D3350` (VA `0x6D3F50`)
selects con.indentAmount object **VA** `0x1E6ABE8`, name **VA**
`0x1BDF798`, default **VA** `0x1BDF7C8` bytes `32 00` (2).
Raw `0x2D336B`, bytes `68 80 00 00 00`, supplies flags `0x80`.
Character initializer raw `0x2D3310` (VA `0x6D3F10`) selects
object **VA** `0x1E6AB78`, name **VA** `0x1BDF7D0`, default
**VA** `0x1BDF7CC` bytes `20 00` (space).
Raw `0x2D332B`, bytes `6A 00`, supplies flags zero.
Both call constructor **VA** `0x11D5CA0`; raw `0xDD5158`
stores self at `+0x5C`, proving the formatter aliases.
CRT pointers raw `0x16C6524/0x16C6528` contain the respective
initializer VAs. Defaults are now constructor-bound, not inferred by adjacency.

## Allocation reporter cell maps to a return-capable assertion wrapper

Correct mapping of reporter **VA** `0x1C15EA0` is raw
`0x1814AA0`, not the initially suggested `0x1804AA0`.
Cell bytes `C0 37 6C 01` contain **VA** `0x16C37C0`,
target raw `0x12C2BC0`. Allocator raw `0x12B8006` bytes
`FF 15 A0 5E C1 01` call this cell.
The wrapper invokes raw `0x12C38E0`; both have explicit returns
at `0x12C2BD8/0x12C39A9`. The callee formats assertions,
flushes, and invokes downstream reporting.
The cell lies in read-only .rdata; the bounded literal-reference scan found
calls, not publication writes.
This proves return-capable structure only, not that downstream reporting
returns in the selected failure scenario. The next scope is raw `0x12C3470`.

## Assertion reporter uses a locally continuable exception on this path

Active-sample raw `0x12C38E0` formats assertion text using sprintf_s
through IAT **VA** `0x1AC48DC`, flushes via _flushall at
**VA** `0x1AC46E0`, then calls `0x12C3470` at `0x12C3972`.
Arguments include literal exception code `0x012ED502`, ASSERT text,
message buffer, and byte flag 1. The numeric code is not an address.

Raw `0x12C34FC` calls RaiseException through IAT **VA**
`0x1AC41B0` (raw `0x16C2DB0`) with code, flags zero and four
arguments containing string lengths/pointers. Local scope metadata at
raw `0x19AD398` selects filter raw `0x12C350B`, bytes
`83 C8 FF C3`, returning -1 / EXCEPTION_CONTINUE_EXECUTION.
On this flag-1 path, code can resume, restore its frame and return at
`0x12C3545`; its parent can log conditionally then return at
`0x12C39A9`. Neither inspected function directly calls terminate/abort/exit
or checks debugger presence.

Flag zero takes a different RaiseException call at `0x12C3524`,
with EXCEPTION_NONCONTINUABLE and no local try; that is not this
allocator reporter's selected call.
Local return-capable control flow does not prove process outcome:
vectored or outer reporting handlers can intervene. The next bounded
scope looks for code `0x012ED502` recognition and actual registration,
not an unconditional recovery claim.

## Assertion-handler search finds only emitters and a continue-search VEH

The bounded active-sample search for code bytes `02 D5 2E 01`
found raw `0x12C36DB/0x12C396E`, both loading the exception code
before calling `0x12C3470`, not recognizers.
Local filter raw `0x12C350B` remains unconditional continue-execution.
VEH registration at raw `0x125371A` supplies callback **VA**
`0x1654570`, raw `0x1253970`; its `33 C0 C2 04 00`
returns continue-search unconditionally.
No code-specific interception policy is established. Computed/external
handlers remain outside this bounded coverage; the gap is parked, not
grounds for asserting either unconditional recovery or termination.

## Startup success pairs depth increment with conditional decrement

Service initialization raw `0xE54DB5..0xE54DBF` supplies the static
console object's address and invokes +BC. Normal success calls the same
object's +C0 at raw `0xE550F1`, targeting `0xE5B0D0`.
Constructor raw `0xE5879A` zeros word object `+4`, while the
decrement requires that gate nonzero. Post-increment failure exit at
`0xE550A5` bypasses +C0; this function does not call +C4 reset.
Thus call pairing is proven on success, not unconditional numeric balance.
The next scope follows gate activation through the known depth reset at
raw `0xE5D13E` and object `+4` assignment.

## Console activation precedes startup's formatting increment/decrement

Console constructor raw `0xE5879A` zeros word **VA** `0x1E6934C`.
Service init raw `0xE54C67` invokes console vtable slot zero,
whose raw `0x1876AA8` bytes `90 D9 25 01` target
**VA** `0x125D990`, raw `0xE5CD90`.
That initializer preserves this in EBX and resets depth at `0xE5D13E`,
then raw `0xE5D148`, bytes `C6 43 04 01`, sets object byte
`+4` to 1 without changing its adjacent byte.
This activation precedes startup increment `0xE54DBA`, success
decrement `0xE550F1`, and intervening failure exits.
Failure exit `0xE550A5` itself neither decrements nor clears the gate;
an earlier branch can reach it before activation. Intervening indirect
callees have not been exhaustively checked for later mutation.

## Texture index save mirrors outer reads and excludes extra allocated slot

Writer raw `0xE9DE10` (VA `0x129EA10`) serializes version 1,
N=`[owner+0x184C]`, then exactly N records of `0x4C` bytes from
`[owner+0x1848]`, without skipping a leading record.
Reader allocates N+1 but reads N, so extra slot index N is not serialized;
calling it a sentinel remains an inference pending its initialization/use.

Seven table blocks follow in ascending level 0..6: LE DWORD compressed
length L_i followed by L_i bytes, next position `position+4+L_i`.
Decoded sizes are `0x1000000,0x400000,0x100000,0x40000,0x10000,0x4000,0x1000`.
Writer allocates `S+floor(5*S/100)+66` for a decoded size S.
Raw `0xE9E04B`, bytes `E8 90 AB 46 00`, calls compressor
`0x1308BE0` (VA `0x17097E0`) with ECX=input, EDX=size,
stack output. EAX becomes L_i directly, without added codec-header length.
Reader calls matching decoder `0x13090D0`.

The compressor's 32-byte literals, extended/far matches, and first-byte
OR `0x20` at raw `0x13090B9` support a FastLZ level-2-style
interpretation; external algorithm identity is not proven by a function name.
Writes of version/count/records/length/payload have no result checks in the
inspected path; flush/commit/close likewise remain unchecked.
Rename is tested and reports failure, then returns success if reporting returns.
Exact import identities here rely on earlier PE evidence, not fresh placeholder
slot enumeration. The next scope follows the extra record's concrete use.

## Extra index record is zeroed append capacity in the traced path

Allocator raw `0xEB35E2..0xEB35E9` multiplies dimensions;
`0xEB3643..0xEB3647` invokes verified MSVCR120!memset over
all `0x4C*(N+1)` bytes, including slot N.
Lookup raw `0xE9E466..0xE9E4ED` scans strictly `[0,N)` and
returns null on miss; it does not use a search sentinel.
Append raw `0xE9F81D..0xE9F833` computes base+N*`0x4C`,
increments owner count, then `0xE9F839..0xE9F8D3` fully populates
that record before save at `0xE9F8D8`.
This establishes slot N as append capacity in this path, not a search
sentinel. Alternate consumers are not exhausted. The next check follows
array resizing across repeated appends rather than assuming permanent spare
capacity after count increments.

## Console initializes a text log sink and tolerates open failure

Console initializer raw `0xE5D2CF` invokes virtual `+0xCC`,
target `0xE5EA00` (VA `0x125F600`).
At `0xE5EA7A`, helper `0x12BA030` converts UTF-8 path to wide
and calls _wfopen through **VA** `0x1AC4704`, mode wt.
Path is object `+0x10`, default console.log; FILE* is stored at
global **VA** `0x1E49A18`.
Writer raw `0xE5AF48` calls fprintf through **VA** `0x1AC48B0`
with format %s; null handle skips output, and `0xE5AF5D` conditionally
flushes. Open failure reports `Could not open %s for logging` through
`0x2FC4F0`, severity 5, then continues without propagated failure.
Shutdown virtual `+0xD0 -> 0xE5EEE0` closes/clears the handle.
The next scope resolves callback linkage into writer `0xE5AC70`,
not an assumed connection from arbitrary log calls.

## Texture misses restore spare capacity by reloading before append

Raw `0xE9EAC2` enters owner transaction `0xE9E200`, then
`0xE9EACD` reloads the index through `0xE9D960`.
Reload frees previous record storage at `0xE9D98C`, clears its pointer
at `0xE9D991`, loads persisted N, and allocates zeroed `(N+1)*0x4C`
at `0xE9DB7A`, publishing it at `0xE9DB82`.
Append consumes the spare and publishes count N+1, then saves.
No subsequent array reallocation was found before this operation returns.
Successful repeated misses therefore reload persisted N, allocate N+1,
append/save N+1, then on the next miss reload and allocate N+2.
This is not permanent spare capacity after every append.
Owner-end `0xE9E300` follows save at `0xE9F904`.
The next scope resolves the known filesystem helpers `0x1309560/0x1309650`;
non-atomic owner nesting does not itself prove cross-thread/process exclusion.

## Category logging reaches the console writer through a callback slot

Console vtable byte-offset `+0xB4`, raw `0x1876B5C`, contains
**VA** `0x125B870`, writer raw `0xE5AC70`.
Raw `0x3041AD` selects callback **VA** `0x14D9440`,
raw `0x10D8840`; raw `0x3041CA` stores EAX to callback slot
**VA** `0x1DF96F8`. The registration helper's exact returned-value
contract remains the next check before equating every stored EAX with
that selected callback.
Category constructor raw `0x12C1A4A` stores this slot's address at
category `+0x14`; generic emitter `0x12C208F` invokes the callback.
Callback raw `0x10D89F6` dispatches console `+0xB4`.

Concrete emission raw `0xE5EA9D` logs `Logging to %s` through
`0x325A90`, severity 2, category **VA** `0x1E6B5D8`.
Its initializer `0x2D3B38..0x2D3B41` invokes category constructor
`0x12C1A20`. This joins emission, generic emitter, callback, writer,
and conditional file output.
The calls are synchronous and pass stack-local text; exclusive thread ownership
is not established. Enable/context gates and nonnull FILE still govern output.
The next scope checks callback-slot initialization/replacement and startup order.

## Texture lock uses rename claim, heartbeat and stale-name reclamation

Owner entry raw `0xE9E22A..0xE9E293` increments ordinary nesting
count; only previous-zero entry acquires object `+0x1998`, ignoring
its return. Lock constructor formats `%s_tmp_lock_file` from the already
traced owner `+0x19A0` virtual-texture location prefix.
Helper `0x12BC5A0` generates a candidate in current directory using
`XXXX-XXXX-XXXX-XXXX.tmp`, seeded by timeGetTime XOR thread ID.
Stat rejection of existing nondirectory candidates is not exclusive creation.
Acquisition raw `0x1309695..0x13096B0` fopen wb creates the
candidate; null fails, fclose result is ignored.
UTF-8/wide conversion helper `0x12B7610` then `_wrename`s candidate
to lockname; exactly zero return succeeds.

On failed rename, the algorithm samples lock mtime, zero on stat failure.
A changed mtime records `{mtime,current_time}` in object `+0x28/+0x30`.
Unchanged nonzero mtime triggers unlink/retry if elapsed high DWORD is
nonzero or low DWORD exceeds 120 seconds. Unlink result is ignored;
one further rename failure deletes the candidate and returns false.
Outer raw `0x1309570..0x13095A4` logs failed attempts and sleeps
5000 ms, retrying indefinitely without overall timeout.

Success sets held byte `+0x38`, clears observation fields and starts a
heartbeat thread. Worker raw `0x1309880` polls stop after Sleep(100)
and roughly every 30,000 accumulated milliseconds opens lockname wb,
closes it, and ignores failures. Thread-create failure does not release
the claimed name; exact wrapper ownership is the next scope.
Final owner release flushes data, stops/joins heartbeat with INFINITE wait,
unlinks lockname unchecked, clears held, destroys the object and nulls `+0x1998`.

PE evidence verifies `_wrename` IAT **VA** `0x1AC4720`,
`_wunlink` `0x1AC471C`, `_wstat64` `0x1AC472C`,
`_time64` `0x1AC48D8`, Sleep `0x1AC4374`.
This is pathname claiming plus heartbeat/stale reclamation, not universal
writer exclusion or a proven same-owner thread mutex.

## Logging callback replacement is a direct store, not a helper return

The active-sample instruction correction at raw `0x3041AD..0x3041CA`
finds no registration-helper call: immediate callback **VA** `0x14D9440`
is selected and directly stored to **VA** `0x1DF96F8`.
The earlier returned-EAX caveat is resolved; that EAX is directly formed.
Slot initial backing raw `0x19F7AF8` contains `B0 21 6C 01`,
default **VA** `0x16C21B0`, raw `0x12C15B0`.
Four direct stores found at raw `0x3041CA`, `0x10D8A34`,
`0x10D8ADE`, `0x12C1A08` install `0x14D9440`; no fallback
reset was established.
Console shutdown `0xE5EEE0..0xE5EF1A` closes/nulls two FILE
cells, not this callback slot. No slot synchronization was observed.
The report did not resolve console-open ordering, but its suggested
initializer/open chain is already documented above and is not rescanned.
The next independent scope identifies the concrete default callback behavior.

## Default logging callback writes and flushes CRT streams synchronously

Initial callback raw `0x12C15B0` reaches fprintf at `0x12C18F9`
through IAT **VA** `0x1AC48B0`, raw `0x16C34B0`; PE
hint/name evidence identifies fprintf.
Format **VA** `0x1C15D20` (raw `0x1814920`) is
`%5s %s %7s (%05u) %s\n`, containing severity label, timestamp,
category, numeric identifier and message.
Severity 5 selects `__iob_func()+0x40`, otherwise `+0x20`,
consistent with CRT stderr/stdout. Both converge on fflush at raw
`0x12C1984`, IAT **VA** `0x1AC48B4`.

Null fourth argument takes direct fprintf; nonnull builds a `0x600`-byte
stack format, forcibly terminates it, then calls vfprintf using its va-list.
Category byte `[arg_4+7]!=0` substitutes BAD MEM rather than discarding
the message. No normal severity-discard path was found; absent console
handle skips coloring only. Formatting/write/flush results are unchecked.
Normal return is raw `0x12C19CF`.
Arguments are borrowed, formatting stack-local, with no thread handoff or
ownership transfer observed. Lock-shaped initialization/acquire/release is
the next scope; first-use concurrency safety is not assumed.

## Heartbeat startup failure retains held state until owner release

Thread construction raw `0x12BFE00` clears stop/started bytes
`+4/+5`; lock ctor `0x13092D0` sets `+0x1C=self`, held
`+0x38=0`. After rename acquisition, raw `0x1309560` sets held,
joins any prior thread via `0x12BFF70(0)`, clears stop, and invokes
virtual `+4`. False from that hook returns false without clearing held.
The later thread-create-shaped call likewise returns false on null HANDLE,
stored at `+0xC`; started byte reflects handle nonzero, ID is `+0x14`.
These terminal failures happen after claim, not because the retry loop gave up.

Join helper sets stop, waits only when started/handle are nonzero, maps timeout
zero to INFINITE, closes/nulls the handle and clears started.
Worker `0x1309880` can perform a final heartbeat open/close even when
stop is noticed during the wait.
Final owner release checks held independently of successful thread creation,
joins, removes the name and clears held. Other wrapper cleanup is gated.
Thread API names remain call-shape interpretations in this report; the next
scope verifies the PE import, start argument and virtual hook to close worker
entry linkage rather than assuming it from a function label.

## Default logger serializes its body with a Win32 critical section

PE evidence identifies Initialize/Enter/Leave/TryEnter/DeleteCriticalSection
IAT **VAs** `0x1AC4280/0x1AC4278/0x1AC427C/0x1AC4284/0x1AC4274`.
Logger globals are guard **VA** `0x1F11410`, critical section
`0x1F11414`, and depth DWORD `0x1F11444`.
Raw `0x12C161C` tests bit zero then uses a plain OR/store of the
guard **before** initializing the critical section, without atomic/interlocked
instructions in that branch. Concurrent first-use safety is therefore not
established; actual simultaneous first use was not observed.

Raw `0x12C164A..0x12C1677` tries entry, blocks via Enter on
failure, increments depth and sets unwind state zero. Formatting, CRT output
and flush run within the critical section. Raw `0x12C199C` decrements
depth and leaves normally.
Exit cleanup raw `0x16C17D0` asserts nonzero depth before deleting
the critical section. Registration resembles exit cleanup but its runtime-init
helper contract remains unverified.
The next narrow scope follows handler raw `0x16948DB` to its cleanup
action before assuming exceptional release.

## Heartbeat uses _beginthreadex and a virtual worker thunk

The interrupted follow-on verifies PE import **VA** `0x1AC4AB0`
as MSVCR120!_beginthreadex, correcting earlier CreateThread-shaped naming.
Raw `0x13095EA`, bytes `68 D0 09 6C 01`, supplies start
**VA** `0x16C09D0`, raw `0x12BFDD0`.
Thunk raw `0x12BFDD9..0x12BFDDE`, bytes
`8B 4D 08 8B 01 FF 50 0C`, calls argument virtual `+0xC`,
then _endthreadex(0). Startup hook at raw `0x13095D7`
uses virtual `+4`; false skips thread creation.
Exact lock vtable slots remain the narrow next check before joining this
thunk to worker `0x1309880`.

## Logger unwind metadata identifies one cleanup action

Handler raw `0x16948DB` loads metadata **VA** `0x1DAE670`
(raw `0x19AD270`), magic `0x19930522`, maxState 1, no try blocks.
Unwind map **VA** `0x1DAE694` (raw `0x19AD294`) has state
0->-1 cleanup **VA** `0x1A954D0`, raw `0x16948D0`.
This identifies the action but does not yet prove its instructions decrement
depth or leave the critical section; the next bounded task decodes that action.

## Lock vtable confirms heartbeat worker and identifies startup-hook target

Constructor raw `0x130930A` stores vptr **VA** `0x1C812B0`,
raw `0x187FEB0`. Slot `+0xC` contains **VA** `0x170A480`,
raw `0x1309880`, closing the _beginthreadex thunk-to-worker chain.
Slot `+4` contains **VA** `0x6EC4D0`, raw `0x2EB8D0`.
Raw `0x13095D7` is the already-known virtual-call site, not a proposed
hook implementation; the follow-on's wording to the contrary adds no gap.
The next narrow check decodes the actual slot-4 target.

## Logger cleanup selects a stack guard and tail-jumps

Raw `0x16948D0` bytes `8D 8D 7C F9 FF FF E9 F5 85 C5 FE`
select `[ebp-0x684]` in ECX and jump onward.
The report did not resolve that target; its numeric target requires an
exact rel32 arithmetic check before integration.
Absence of a direct LeaveCriticalSection in this small thunk neither proves
nor refutes exceptional release. Security-cookie calls in the main handler
do not resolve the guard cleanup and are not followed as a substitute.

## Heartbeat startup hook always succeeds for the concrete lock vtable

Raw `0x2EB8D0` (VA `0x6EC4D0`) consists of `B0 01 C3`,
mov al,1; ret, followed by padding.
The callsite raw `0x13095D7` loads slot +4, calls at `0x13095DA`,
and tests AL before the false branch to `0x130960B`.
For the proven lock vtable this gate therefore succeeds unconditionally;
the generic false-hook failure described earlier is not reachable with
this exact slot target. Thread creation can still fail independently.
The next scope checks wait-result handling in `0x12BFF70`, including
the timed join caller versus final owner's INFINITE wait.

## Logger cleanup relative-jump arithmetic resolves to raw 2ECED0

For the recorded cleanup bytes, E9 is at raw `0x16948D6`, next IP
`0x16948DB`; signed displacement `F5 85 C5 FE` is
`-0x13A7A0B`. The target is consequently raw `0x2ECED0`,
VA `0x6EDAD0`, not the earlier reported `0x2F6ECB`.
The follow-on mistakenly inspected a different numeric address and supplies
no target-body evidence. The final narrow read uses IDB raw `0x2ECED0`
directly; exceptional release is still unproven pending that read.

## Logger cleanup conditionally decrements its guarded object's counter

Correct raw target `0x2ECED0` tests guard byte `[ecx+4]`, returning
if zero; otherwise it loads object `[ecx]`, pushes it, decrements
`[object+0x30]` and calls LeaveCriticalSection through **VA** `0x1AC427C`.
The guard is stack `[ebp-0x684]`. This resolves the cleanup implementation.
The report's claim that it cannot decrement global `0x1F11444` is not
supported: if its object is `0x1F11414`, object+0x30 is precisely that
global. The next check verifies the guard's initialization/activation.

## Timed thread join clears ownership even after timeout or wait failure

Raw `0x12BFFB8..0x12BFFC5` maps timeout zero to `0xFFFFFFFF`
and calls IAT **VA** `0x1AC42C8`. Result `0x102` is treated
as timeout; other nonzero results take a diagnostic path.
With nonzero handle, the wrapper subsequently closes/nulls `+0xC` and
clears started byte `+5`, including after timeout/failure.
It returns close result, not wait status; without a handle it returns the
initial virtual-call result.
Caller raw `0x1309900` uses timeout 10000 if started, then closes
separate field `+8`; worker termination is not established by that timed path.
Import names need direct PE verification, not empty IDA import enumeration.
The report's VA for raw `0x12BFF70` was arithmetically wrong:
correct text VA is `0x16C0B70`, not `0x16CCB70`.

## Logger guard initialization closes the exceptional-release chain

Raw `0x12C164F` stores critical-section address **VA** `0x1F11414`
at `[ebp-0x684]`; `0x12C1657` sets guard byte `+4` to 1.
The previously recorded EH map selects this same stack guard through
cleanup `0x16948D0 -> 0x2ECED0`.
Together these observations establish conditional unwind decrement of
`[0x1F11414+0x30]`, namely depth **VA** `0x1F11444`,
then LeaveCriticalSection. Normal acquire increments at `0x12C1671`;
normal release decrements at `0x12C199C`.
The follow-on's uninspected-handler caveat does not reopen the already
recorded handler/map/action evidence. Exact new opcode-byte strings were
not returned; binding relies on its cited operand inspection.

## Wait import remains unresolved after failed IDA helper; local parsing follows

Wait call raw `0x12BFFC5` is confirmed as bytes
`FF 15 C8 42 AC 01`, operand **VA** `0x1AC42C8`.
The bounded IDA-side PE parser failed due to helper scope; no DLL/name
identity is inferred from that failure or from empty import enumeration.
The next check parses the immutable local PE with a normal inspection
script, avoiding repeated IDA helper-scope failures.
Separately, timed caller `0x1309900`'s concrete object identity is checked
before assigning its ten-second behavior to the heartbeat lock itself.

## Immutable PE inspection verifies WaitForSingleObject

Local standard-library PE parsing confirms **KERNEL32.dll!WaitForSingleObject**:
IAT RVA `0x16C42C8`, VA `0x1AC42C8`, raw `0x16C2EC8`,
bytes `DA FB 9B 01`, pointing to import-name RVA `0x19BFBDA`
(raw `0x19BE7DA`). This independently resolves call bytes at raw
`0x12BFFC5` without relying on raw-IDB import enumeration.
The first inspected window did not contain the later CloseHandle call;
that final import operand is checked with a narrowly extended window.

## Thread join's later CloseHandle import is independently verified

Extended immutable-file inspection finds raw `0x12C003B` bytes
`FF 15 88 43 AC 01`, calling **KERNEL32.dll!CloseHandle**.
IAT **VA** `0x1AC4388` is RVA `0x16C4388`, raw `0x16C2F88`;
hint/name RVA is `0x19BF84E`, hint `0x52`.
This supplies direct import evidence for the previously traced handle cleanup,
separate from whether waiting succeeded or the worker actually terminated.

## Ten-second join belongs to a distinct wrapper, not the heartbeat lock

Raw `0x1309900` resets this vptr to **VA** `0x1C80C5C`,
distinct from heartbeat lock **VA** `0x1C812B0`.
Its started-byte check gates a join at raw `0x1309933`,
bytes `E8 38 66 FB FF`, with timeout 10000 pushed at `0x130992C`.
Regardless of that condition, raw `0x1309938/0x130993A` pushes
field `+8` and calls IAT **VA** `0x1AC4940`.
That separate resource's type must not be inferred as a thread HANDLE
or this call confused with the verified CloseHandle for wrapper `+0xC`.
Constructor candidate raw `0x13093E0` stores the same vptr at
`0x13094CE`; exact field-8 creation and its cleanup import are the
next narrow check. Concrete class name and complete caller chain remain open.

## Logger ignores normalized exit-registration result; target identity still open

Raw `0x12C161C..0x12C1647` sets the first-use guard, initializes
the critical section, and passes cleanup **VA** `0x1AC23D0`
(raw `0x16C17D0`) to the runtime-init registration helper.
The helper normalizes its result to 0/-1; this caller ignores it and continues.
A bootstrap state of -1 selects one indirect forwarding routine, other state
selects another. Neither routine's identity is established by this pass,
so no atexit/onexit/private-list or failure-branch label follows from the
helper's existing name. The next check resolves exact operands/imports.

## Timed wrapper field +8 is CRT-freed; initializer remains open

Read-only inspection of the active sample identifies IAT **VA** `0x1AC4940`
as **MSVCR120.dll!free**. Its backed raw slot is `0x16C3540`,
containing bytes `E2 0E 9C 01`. At raw `0x1309938`, bytes
`FF 76 08 FF 15 40 49 AC 01` push `[this+8]` and call that import.
This is heap deallocation, not CloseHandle, and does not identify the
allocation routine or concrete resource type.

Raw `0x13093E0` is another destructor: it releases reference-counted fields
before the conditional join and `free(this+8)`. Its shared vptr store is
not evidence of construction. The shared vptr **VA** `0x1C80C5C` maps to
backed raw `0x187F85C`; it remains distinct from heartbeat vptr
`0x1C812B0`. A bounded successor searches actual initialization stores
and one field-8 producer; no heartbeat allocation ownership follows yet.

The parallel logger follow-up only reconfirmed adjacent critical-section
imports, not the requested helper's forwarding targets. Those imports
do not resolve or refute exit registration. The next check starts from
the direct E8 displacement at raw `0x12C1642`, rather than the neighboring
indirect calls.

## Exit helper direct edge resolved; timed vptr search corrected

Immutable bytes at raw `0x12C1642` are `E8 E1 9F 0A 00`.
Signed displacement `+0xA9FE1` resolves to raw `0x136B628`
(code **VA** `0x176C228`). This thin helper calls raw `0x136B57F`
at `0x136B62E`; its own body has no indirect call. The unresolved
forwarding targets belong to the latter body, not the thin wrapper.
The preceding callback push at raw `0x12C163D` is
`68 D0 23 AC 01`. Existing runtime-init names do not establish its contract.

The timed-wrapper follow-up searched the wrong table offset and byte
pattern, so its negative results are not useful evidence of absence.
Arithmetic correction: vptr **VA** `0x1C80C5C` minus rdata mapping
`0x401400` is raw `0x187F85C`, not `0x188F85C`.
Its little-endian bytes are `5C 0C C8 01`, not `5C 0C 80 01`.
The next pass verifies the destructor's actual immediate before searching
its packed bytes for initialization stores. Allocation origin remains open.

## Corrected vptr search locates wrapper initializer and field-8 producer

The corrected `5C 0C C8 01` search finds the vptr immediate at raw
`0x12BFE0F`, in initializer raw `0x12BFE00` (code **VA** `0x16C0A00`).
It zeroes `this+8` at `0x12BFE1B`, then stores the return value of
IAT **VA** `0x1AC493C(a2)` into that field at `0x12BFE5F`.
The import's identity and argument semantics remain unresolved; the
assignment alone is not evidence of a particular allocator.

The same vptr immediate is stored at raw `0x1309907` in the previously
traced destructor, which calls `free(this+8)`. The store at `0x13094CE`
also belongs to teardown. This establishes a concrete producer/cleanup
pair without yet assigning a name or resource type to the field.

## Exit registration sentinel path is MSVCR120 _onexit

At raw `0x136B595`, bytes `8B 35 A8 43 AC 01 FF D6` load and call
**KERNEL32.dll!DecodePointer**, IAT **VA** `0x1AC43A8`
(backed raw `0x16C2FA8`). The decoded first global is compared with -1
at raw `0x136B5A0`, bytes `83 F8 FF`. The predicate is therefore not
simply an untransformed bootstrap-state integer.

For that sentinel, raw `0x136B5A8` calls
**MSVCR120.dll!_onexit**, IAT **VA** `0x1AC479C`, with callback
`[ebp+8]`. Otherwise the body decodes globals **VA** `0x1F1B474`
and `0x1F1B470`, calls a helper, then re-encodes/stores the pair.
**KERNEL32.dll!EncodePointer** is IAT **VA** `0x1AC43C4`
(backed raw `0x16C2FC4`); it is not itself the list-registration helper.
The follow-up did not return that helper's exact target or argument contract,
so only the sentinel forwarding path is fully identified here.
Import-slot file bytes are lookup RVAs, not runtime function addresses.

## Thread wrapper owns a duplicated diagnostic label

Immutable PE import parsing identifies **VA** `0x1AC493C` as
**MSVCR120.dll!_strdup**, paired with `free` at `0x1AC4940`.
Initializer raw `0x12BFE00` therefore owns a duplicate of argument `a2`
in `this+8`. Join helper raw `0x12BFF70` uses it in timeout/error logging;
destructor raw `0x1309900` conditionally waits 10000 ms, then frees it.
This field is neither a thread HANDLE nor worker data inferred from that name.

Concrete caller raw `0x2EA330` supplies label **VA** `0x1C5E964`,
backed raw `0x185D564`, bytes `DumpKeyComboThread\0`.
That identifies this caller's diagnostic label, not every base-wrapper caller
or the heartbeat wrapper. The next trace follows this concrete worker and
its stop/teardown ordering.

## Non-sentinel exit helper reaches IAT 1AC47A0

Raw `0x136B5DB..0x136B5E7` arranges the encoded callback and pointers to
locals holding decoded globals **VA** `0x1F1B474/0x1F1B470`.
Call raw `0x136B5E7`, bytes `E8 B0 01 00 00`, reaches stub
raw `0x136B79C`, bytes `FF 25 A0 47 AC 01`, jumping through IAT
**VA** `0x1AC47A0`. Return is retained in EDI at `0x136B5EF`
and returned in EAX at `0x136B614`. Its import identity remains pending
immutable PE parsing; empty raw-IDB import queries do not resolve it.

Cleanup raw `0x16C17D0` conditionally calls the already documented
assertion reporter through **VA** `0x1C15EA0`, then passes critical-section
address **VA** `0x1F11414` to IAT **VA** `0x1AC4274`.
That slot was previously verified as **KERNEL32.dll!DeleteCriticalSection**;
the follow-up's empty import query does not reopen its identity.
The cleanup's assertion predicate and non-sentinel registration contract
remain the next bounded checks.

## Exit list forwarding and logger teardown predicate resolved

Immutable PE parsing identifies IAT **VA** `0x1AC47A0` as
**MSVCR120.dll!__dllonexit**. In the non-sentinel branch of raw
`0x136B57F`, the forwarded arguments are
`(encoded callback, &local[ebp-0x1C], &local[ebp-0x20])`.
Those locals hold the decoded globals `0x1F1B474/0x1F1B470`;
the updated values are encoded and stored back after the call.
This establishes client-side forwarding/state updates, not CRT internals
or a demonstrated callback-execution order.

Cleanup raw `0x16C17D0` compares logger depth **VA** `0x1F11444` with zero.
Nonzero takes the assertion path with stack arguments
`(0x1B73D63, 0x1B73D6F, 0x7F, 0)`; caller removes `0x10` bytes.
Zero skips that reporter. Either returning path passes **VA** `0x1F11414`
to the verified DeleteCriticalSection import `0x1AC4274`.
The continuable assertion does not guarantee continuation: an unhandled
or non-continuing exception/termination remains outside this straight-line path.

## DumpKeyComboThread concrete worker and bootstrap teardown

Constructor raw `0x2EA330` installs subclass vptr **VA** `0x1C5EA48`
(backed raw `0x185D648`), whose byte-offset `+0xC` is
**VA** `0x6EAE00`, code raw `0x2EA200`.
The generic start thunk raw `0x12BFDD0` invokes that virtual slot.
The worker checks stop byte `this+4`; otherwise it sleeps 200 ms
and polls GetAsyncKeyState for `0x11/0x12/0x10/0x2E`
(Ctrl, Alt, Shift, Delete). Its byte `+0x70` gates a subsequent
matching-poll action; exact transitions/action are a separate continuation.

Bootstrap constructs the local object at raw `0x2EB14C`, starts it at
`0x2EB161`, calls ClientApplicationShutdown at `0x2EB1F9`,
then destroys the object at `0x2EB24A`.
Destructor raw `0x2EA520` restores the base vptr and joins with 10000 ms.
Join raw `0x12BFF70` requests stop through byte `+4`, waits, then
closes the handle and clears started state even on WAIT_TIMEOUT (258).
Consequently this teardown ordering does not prove worker termination.

## Dump-key arming byte and indirect action sequence

Worker raw `0x2EA200` tests the high bit of each GetAsyncKeyState result.
Constructor raw `0x2EA330` clears `this+0x70`; a failed chord test clears
it again. A matching poll with zero arms it without action; a matching
poll with it already set takes the action. If that action returns, the byte
is set to 1 again. These branches do not alone establish a rising-edge-only
trigger: subsequent matching polls can repeat the action if control returns.

Raw `0x2EA259..0x2EA28D` calls global **VA** `0x1E002B8`'s virtual
slot `+0xFC` with argument 4, then the previously identified assertion
reporter through **VA** `0x1C15EA0` with arguments
`(0, 0x1C5EA58, 203, 0x1C5E950)`, then IAT **VA** `0x1AC43A4`
with `(-1, -2)`. The import identity, assertion strings, and virtual target
remain pending. A target name inferred solely from the last arguments
would not establish termination or dump output.

The follow-up incorrectly reported `0x2EA200 + 0x400C00 = 0x6FAE00`;
the sum is `0x6EAE00`, consistent with the previously recorded table entry.
Its inability to resolve the thunk as an IDA function does not invalidate
the earlier byte-backed virtual dispatch evidence.

## CRT return-path targets pending PE verification

A bounded exit-list reference pass found registration references but no
concrete client-side drain. This is not proof that no computed drain exists.
After application return, the inspected CRT wrapper conditionally calls
IAT **VA** `0x1AC4828` and `0x1AC47E4`; exact names and branch ordering
are pending immutable PE parsing.

The pass also reported apparent `0x05050505` bytes for globals
**VA** `0x1F1B474/0x1F1B470`, without establishing file backing.
Those are not accepted as loader initial values until the PE section's
raw-size/virtual-size bounds are checked. Do not extrapolate raw-IDB
contents into virtual-only globals.

## CRT post-application exit boundary and loader-zero list globals

Immutable PE parsing resolves IAT **VA** `0x1AC4828` to
**MSVCR120.dll!exit** and `0x1AC47E4` to **MSVCR120.dll!_cexit**.
After application return, the CRT saves EAX in **VA** `0x1E36614`.
If **VA** `0x1E36618` is zero, call raw `0x136AEEE` passes that status
to `exit`. Otherwise it skips `exit`; call raw `0x136AF56` invokes
`_cexit` only if **VA** `0x1E36610` is zero, then follows local
cleanup/return. Code following the external `exit` call is reached only
if that call returns. These imports do not expose CRT-internal callback order.

Both successful initialization/runloop and initialization failure in
bootstrap raw `0x2EAE70` reach ClientApplicationShutdown at `0x2EB1F9`
before application return. Which CRT exit-mode branch this standalone
sample selects remains a flag-initialization continuation.

PE `.data` has RVA `0x19C5000`, VirtualSize `0x15647C`,
PointerToRawData `0x19C3400`, and SizeOfRawData `0x71800`.
List globals **VA** `0x1F1B470/0x1F1B474` have RVAs
`0x1B1B470/0x1B1B474`: inside its virtual range but beyond raw backing.
Their loader initial values are zero, not the previously observed
raw-IDB `0x05050505`. They have no corresponding backed raw file offsets.

## Dump-key action reaches TerminateProcess if assertion reporting returns

Immutable PE parsing resolves IAT **VA** `0x1AC43A4` to
**KERNEL32.dll!TerminateProcess**. The worker passes pseudo-handle -1
and exit code -2 after the assertion-reporting call.
The fourth reporter argument **VA** `0x1C5E950` is
`"Dump key pressed"`; its second argument **VA** `0x1C5EA58`
points to zero-initialized data in the inspected mapping.
This identifies a termination request, not a proven dump-file write.
The continuable reporter can divert control; neither reaching the
TerminateProcess call nor returning from it is unconditional.

The worker's first virtual action, global **VA** `0x1E002B8` slot `+0xFC`
with argument 4, resolves to **VA** `0x1006BF0`, code raw `0xC05FF0`.
That wrapper conditionally forwards to a list-logging routine; the
list identity, meaning of argument 4, and concrete output are still open.
The table/worker mapping is confirmed: **VA** `0x6EAE00` maps to
raw `0x2EA200`. State byte `+0x70` allows repeated action on every
continuing matching iteration after the first, not merely a rising edge.

## Standalone native CRT return selects exit(status)

The sample's CLR directory is `(RVA=0, size=0)`. CRT exit-mode word
**VA** `0x1E36618` is file-backed at raw `0x1A34A18`, initially zero.
The bounded reference pass found its comparison but no in-image writer.
At raw `0x136AEE4`, bytes
`83 3D 18 66 E3 01 00 75 60 50 FF 15 28 48 AC 01`
select the fall-through `exit(EAX)` call for zero.
This supports the normal standalone native path, not an unconditional
claim against external modification.

Word **VA** `0x1E36610` is also file-backed and initially zero.
The observed write at raw `0x136AE4B` sets it to one only when
CRT initialization state is already nonzero and not one.
That state starts in the loader-zero-filled tail; first entry does not
take that write. Nonzero mode still follows the separately recorded
conditional `_cexit`/local-return branch.

## Dump-key logging wrapper traverses a diagnostic list, not a dump file

The supplied virtual target raw `0xC05FF0` conditionally forwards only if
`this+0xA00` is nonnull, calling raw `0xC294A0` with
`(4, *(this+0xA00+0x78))`. The latter walks a sentinel-terminated list:
head `+0x10`, sentinel `+0x30`, node next link `+8`.
Formatter raw `0xCCFB20` converts entries. Leading formatted entries
starting with `'C'` are skipped; after the first non-`'C'` entry,
subsequent entries are sent to the VIBE logger.
The list's semantic type and producer remain open.

Logging wrapper raw `0xA69E00` emits only when its numeric level is
`>= logger[4]`; argument 4 is this level, not a worker status.
Emitter raw `0x12C2030` dispatches through the configured callback
referenced by logger `+0x14`, passing context from `+8`.
This chain establishes formatted diagnostic logging, not dump-file output.
The follow-up relied on the previously supplied virtual-target resolution;
empty IDA data xrefs neither independently confirm nor disprove that slot.

## Application shutdown entry resolved; console close ordering remains open

Call raw `0x2EB1F9` has bytes `E8 D2 E3 01 00`, resolving to
ClientApplicationShutdown raw `0x3095D0`.
The inspected body drains queued objects before subsystem cleanup.
Concrete edges include call raw `0x309678 -> 0x30D150`,
virtual slots `+0x10/+0x14` through global **VA** `0x1E002B8`,
and call raw `0x309804 -> 0x10D8E30`; it returns at `0x3098BA`.

No direct console-object reference or callback-slot restoration was found
in that body. This does not establish transitive absence through its callees.
Console close raw `0xE5EEE0` itself does not restore callback slot
**VA** `0x1DF96F8`, but its actual shutdown caller and ordering relative
to application return remain unresolved. Consequently a blanket claim that
the inspected shutdown leaves the callback unchanged after file close would
overstate the evidence. The successor follows the concrete service teardown
edges rather than repeating direct-reference scans.

## Diagnostic list payload is tagged; virtual binding needs table-byte verification

In raw `0xC294A0`, the current node is saved at `[ebp-0x10]`;
formatter raw `0xCCFB20` reaches that local through its output-buffer
argument plus `0x60`. The node's first field points to a 16-byte tagged
value. For `(value+8) & 0x1F == 6`, it passes the first DWORD to
raw `0xCCF8C0`; otherwise that argument is zero. This representation
does not identify the semantic meaning of tag 6 or prove UI-event storage.
Raw `0xC293D0` manipulates 16-byte values with low-nibble tags 3 and 4,
normalizes tag-3 values via `0xCD6550`, and calls `0xC294A0(5, context)`.
The insertion path remains untraced.

The follow-up did not verify that uiManager's vptr **VA** `0x1C73EBC`
has `0xC05FF0` at slot `+0xFC`. Earlier statements resolving that virtual
edge are therefore provisional until exact table bytes are checked;
the independently inspected `0xC05FF0 -> 0xC294A0` chain remains valid
conditional on dispatch to that wrapper.
Formatter constants **VA** `0x1BBAD6C/0x1BBAEC4` also require
PE-aware string mapping before declaring them unreadable. The successor
checks those two concrete mapping-sensitive points.

## UI traceback virtual binding and format strings confirmed

PE mapping places uiManager vptr **VA** `0x1C73EBC` at raw `0x1872ABC`.
Slot `+0xFC`, **VA** `0x1C73FB8`, raw `0x1872BB8`, contains
`F0 6B 00 01`, namely **VA** `0x1006BF0`, code raw `0xC05FF0`.
Initializer store raw `0xBF7ABB` is `C7 03 BC 3E C7 01`;
another store occurs at `0xBF8AED`. This closes the provisional virtual edge.

Mapped format **VA** `0x1BBAEC4` is `"    at %s(%d): %s"`;
formatter control string **VA** `0x1BBAD6C` is `"lnS"`.
Together with the traversal this establishes stack-frame/traceback-style
logging. The earlier unreadable-string result arose from failing to map
VA constants before inspection. A nearby Lua 5.3 marker alone does not
identify the node type; exact frame enumeration/field semantics remain open.

## Shutdown service-cell slot +4 reaches console fclose before service return

Helper raw `0x30D150` tail-calls uiManager virtual `+0x34`;
shutdown's subsequent calls are slots `+0x10/+0x14` at
`0x309685/0x309690`. At `0x30979A`, service virtual `+0x64(0)`
resolves to raw `0xE4FB00`, storing zero into service `+0xB4`.
This is not a store to the separate category callback slot.

The aliases carry the address of the service pointer cell, not the static owner.
Raw `0x2D2D60..0x2D2D65` copies the DWORD at **VA** `0x1DFC82C`
(backed raw `0x19FAC2C`, bytes `70 04 E0 01`, value **VA**
`0x1E00470`) to alias **VA** `0x1E60E3C`; raw `0x2D2D70..0x2D2D75`
copies that DWORD to alias **VA** `0x1E60EA4`. The latter is in the
virtual-only data tail, so its value is established by these copy instructions,
not by reading an unrelated raw offset.

The exact shutdown operand sequence at raw `0x10D8F6A..0x10D8F73` is
`A1 A4 0E E6 01 8B 08 8B 01 FF 50 04`: load the alias value (the address
of cell `0x1E00470`), load the service object from that cell into ECX, load
its vptr, then call slot byte-offset `+4` with the service object as receiver.
This is not a call on the static owner at **VA** `0x1E00460`.

The service constructor raw `0xE4DFA0` installs object vptr **VA**
`0x1C77C2C`; the owner itself has a different vptr, **VA** `0x1C77C1C`.
With the `.rdata` mapping, the service vtable's slot `+4` is at raw
`0x1876830`; exact bytes `F0 5F 25 01` encode target **VA** `0x1255FF0`,
which maps to code raw `0xE553F0`. That method's normal path calls the
console object's virtual slot `+8` at raw `0xE55541`. Console vptr **VA**
`0x1C77EA8` has slot `+8` at raw `0x1876AB0`, bytes `20 FB 25 01`,
target **VA** `0x125FB20` / raw `0xE5EF20`.

At raw `0xE5EF68`, that method loads its receiver's vptr and calls slot
`+0xD0`. Console vtable raw `0x1876B78` contains bytes `E0 FA 25 01`,
target **VA** `0x125FAE0` / raw `0xE5EEE0`. The close body conditionally
passes the two global FILE handles **VA** `0x1E49A18` and `0x1E49A14`
to `fclose` through IAT **VA** `0x1AC49A8` (raw `0x16C35A8`), then
zeros each corresponding global. The handle closes are conditional on each
pointer being nonnull.

Thus the synchronous order is app shutdown raw `0x309804` calling
`0x10D8E30`, service slot `+4` -> `0xE553F0`, console slot `+8` ->
`0xE5EF20`, then console slot `+0xD0` -> `0xE5EEE0`/`fclose`.
This occurs before the service method returns and before app shutdown's later
generic nested-container drain call raw `0x309883 -> 0x12DB320`; bootstrap
cleanup then continues past shutdown and returns at `0x2EB2FF`.
`0xE5EF20` continues after the close call, so this proves the close operation's
position in the chain, not that the FILE globals remain null after every later
operation. The separate service `+0x64` clearing `+0xB4` is not restoration of
category callback slot **VA** `0x1DF96F8`.

## Traceback enumerates VM frames and requests line/source/name metadata

The raw `0xC294A0` traversal is a VM-context frame walk, not an OS stack
walk. For control string `"lnS"`, formatter raw `0xCCFB20` dispatches
`l` to raw `0xCCF500`: it computes a line-table index from node PC `+0x14`
relative to prototype code `+0x30`, then reads prototype line table `+0x38`.
`S` calls raw `0xCCF840` for source metadata from the value's prototype;
`n` calls raw `0xCCFF50` under node-flag conditions.

Correction to the earlier formatter summary: raw `0xCCF8C0` is gated
by uppercase `L`, absent from `"lnS"`. Appending the value is separately
gated by `f`, also absent here. Thus the generic tag-6/helper path is not
evidence that this traceback requests it.
The leading-`C` filter examines the local metadata field returned by this
formatting path; later `C` entries are emitted after the first non-`C`.
Exact field layout and frame insertion/removal remain separate questions.
The concrete metadata operations support active script-frame interpretation,
but do not by themselves establish Lua version or complete frame lifetime.

## Service deinitialization includes a conditional post-close logging attempt

Raw `0xE553F0..0xE55571` begins with logger helper `0x322C40`
at `0xE553FE`, console `+0xBC` at `0xE55410`, conditional console
`+0xEC` at `0xE5543E`, and entry `+0x7C` callbacks at
`0xE55444..0xE55480`. Registry/list cleanup includes `0xE54AB0`
and `0xDE12B0`, followed by console `+0x14` at `0xE55534`
and console `+8` at `0xE55541`.

Console deinit raw `0xE5EF20` closes/nulls the two FILE globals through
`+0xD0` at `0xE5EF68`, then calls console `+0xC` at `0xE5EF78`.
Later path processing invokes IAT **VA** `0x1AC471C` and conditionally
reports a result neither zero nor -1 (`0xE5F4A9..0xE5F511`).
Its import identity is pending; this is not evidence of reopening a log.
It clears byte `this+4` at `0xE5F597`.

After console deinit returns, service shutdown calls `0xE6F7C0`
at `0xE55546`, console `+0xC0` at `0xE55555`, then `0x322C40`
at `0xE55565`. That helper conditionally emits through console `+0xB0`
(raw `0xE5AB80`) with severity 2 when object/threshold checks pass.
Emitter `0x12C2030` uses the current category callback.
This proves an attempted post-close logging path, not unconditional output.
The sink's file writer is conditional on a FILE being nonnull.

An unclassified reference to callback slot **VA** `0x1DF96F8`
at raw `0x12C1A4D` is the next concrete check; do not conclude that
the four known setter sites exhaust possible callback changes.

## VM frame traversal verified; producer/pop remains a bounded stop

Raw `0xC05FF3/0xC05FFC/0xC06005` gates/loads the manager's `+0xA00`
object and its `+0x78` context. Raw `0xC294C6` starts from context
`+0x10`, `0xC294CB..0xC294E5` tests context `+0x30` sentinel,
and `0xC294D4` advances through node `+8`.
No insertion/pop or sentinel initialization was established by the bounded
follow-up. Therefore current-to-parent interpretation remains conditional
on the head being the active frame. Further repeated producer scans are
parked; the next task addresses concrete formatter fields and special cases.

## Callback reference is slot indirection; deinit path unlinks rather than reopens

Raw `0x12C1A4D` is inside the immediate of instruction `0x12C1A4A`,
`mov [ebx+0x14], offset 0x1DF96F8`, in constructor raw `0x12C1A20`.
It stores the address of the callback slot in a category object, not a new
callback into that slot. Emitter raw `0x12C2030` checks both that pointer
and the pointed-to callback before invocation. Setter raw `0x12C19F0`
changes the slot; exact selection of its fallback remains distinct from
the constructor's late-bound indirection.

Immutable PE parsing resolves IAT **VA** `0x1AC471C` as
**MSVCR120.dll!_wunlink**. Path processing at raw `0xE5F4A9`
converts UTF-8 via MultiByteToWideChar at `0xE5F4D9`
(code page `0xFDE9`, source length -1, buffer capacity `0x400` wide chars).
Call `0xE5F4EE` unlinks that path; results 0 and -1 pass the local check.
This is not a reopen/rename. At `0xE5EF78`, the receiver is the separately
loaded global **VA** `0x1E69348`, rather than merely the current
receiver's field. Later raw `0xE6F7C0` releases list `+0x48` entries'
shared-text members `+0x14` and resets list state.

The latest follow-up disputes the previously reported final-call target:
raw `0xE55565` was reported as `0x321C40`, not `0x322C40`.
It did not return exact displacement bytes, so the claimed post-close
logging attempt is provisional pending immutable-byte arbitration.
The independent `0xE5AB80 -> 0x12C2030` logging path does not by itself
prove that this service-deinit call reaches it.

## Traceback output fields and Lua/C metadata branches resolved

Raw `0xC294A0` supplies short source at formatter output `+0x24`,
current line at `+0x14`, and name at `+4` to
`"    at %s(%d): %s"`. Raw `0xCCF500` computes
`((nodePC - prototypeCode) >> 2) - 1` for the prototype line-table
index; absent line table returns -1.

Raw `0xCCF840` writes classification `what` at output `+0xC`,
source at `+0x10`, and line bounds at `+0x18/+0x1C`.
Mapped literals include `"Lua"`, `"main"`, `"C"`, `"=[C]"`, and `"=?"`.
The native branch sets `what="C"`, source `"=[C]"`, bounds -1.
The caller's leading-C suppression tests `what`, not the source/name;
after the first non-C frame it emits later C frames too. This is concrete
Lua/native classification evidence, not proof of a particular Lua version.

When output name `+4` is null, the caller uses **VA** `0x1E4891C`
if nonzero, otherwise `"~unnamed~"` (**VA** `0x1BBAE04`,
backed raw `0x17B9A04`). Raw `0xCCFF50`'s flag-4 case returns
`"hook"` and supplies `"?"` via its out-pointer. Native frames admitted
after the initial filter can therefore be emitted with line -1 and a
fallback name. The global fallback name's producer/lifetime is the next
bounded question.

## Immutable rel32 arbitration confirms post-close Shutdown Finished attempt

Python signed-rel32 decoding and IDA instruction-length checks agree:

| Raw call | Exact bytes | Raw target | Code VA |
|---|---|---|---|
| `0xE553FE` | `E8 3D D8 4C FF` | `0x322C40` | `0x723840` |
| `0xE55546` | `E8 75 A2 01 00` | `0xE6F7C0` | `0x12703C0` |
| `0xE55565` | `E8 D6 D6 4C FF` | `0x322C40` | `0x723840` |

This supersedes the disputed `0x321C40` target. That address is inside
an instruction beginning at `0x321C3F`, whereas `0x322C40` starts
`55 8B EC` and implements the object/threshold-gated logging helper.
The early message is `"--- Shutting Down ---"`; the final call passes
console **VA** `0x1E69348` and **VA** `0x1BDF628`,
`"--- Shutdown Finished ---"`, with severity 2.
Console slot `+0xB0` at raw `0x1876B58` contains
`80 B7 25 01`, target **VA** `0x125B780`, raw `0xE5AB80`.
Thus a conditional post-close logging attempt is established, not withdrawn.
Actual runtime emission still depends on helper/logger gates and current sink.

Console slot `+0xC0` at raw `0x1876B68` contains
`D0 BC 25 01`, target **VA** `0x125BCD0`, raw `0xE5B0D0`.
It returns when receiver byte `+4` is zero; otherwise it decrements/clamps
indentation depth **VA** `0x1E49A10`. This resolves that final virtual edge.

## Traceback fallback name is borrowed per-dispatch state with normal save/restore

Raw `0xC3B3C0` takes original argument `a2` into EBX.
At `0xC3B50A` it saves global **VA** `0x1E4891C` in ESI;
at `0xC3B514` it installs EBX in that global.
After dispatch raw `0xC289C0` and helper `0xBF7860`, raw `0xC3B582`
restores the saved pointer. This is a temporary borrowed-pointer override,
not an allocation/copy or clear-to-zero. Normal nested/reentrant returns
restore the prior value; exceptional restoration remains unverified.

The global lies in `.data`'s virtual tail at delta `0x8391C`,
beyond raw size `0x71800` and within VirtualSize `0x15647C`.
Its loader initial value is zero; raw-IDB apparent bytes are not initialization.
Traceback consumers raw `0xC294A0` and `0xC2C767` select the global
when nonzero, otherwise a `"~unnamed~"` literal (the latter at
**VA** `0x1BBB50C`, backed raw `0x17BA10C`).

Caller raw `0xC38CA0` supplies `a2` from per-entry field `v8[2]`
with a default when null, separately from `*v8`.
Format **VA** `0x1BBD77C` is `"%s - %s"`: the object's `+0x70`
label is first, this argument second. The argument also reaches string-cache/
push calls `0xC281A0 -> 0xCD5020` and callback dispatch.
The supported interpretation is a per-entry name/source used during dispatch,
not a proven application-specific script/command identifier.
Five explicit address references cover the two readers and save/install/restore;
this bounded scan does not exclude computed or aliased writes.

## Console deinit disables the final shutdown-message dispatch gate

Raw `0x322C40` forwards severity 2 only when console byte `+4` is nonzero,
byte `+5` is zero, and mutable threshold **VA** `0x1E6B5E8 <= 2`.
It otherwise returns without calling console `+0xB0`.
Therefore the earlier clear of `+4` at raw `0xE5F597` disables not just
indentation decrement but this message's own logging dispatch as well.
The final `"--- Shutdown Finished ---"` helper call remains present;
on the established state path it is gated off, absent intervening re-enabling.
This refines the earlier conditional-attempt description rather than claiming
an actual post-close sink invocation.

Category emitter raw `0x12C2030` invokes the current callback through the
slot pointer and does not itself choose a default sink.
Setter raw `0x12C19F0` selects **VA** `0x14D9440`, or default
**VA** `0x16C21B0` if the former address is null. The former is a fixed
nonnull image address here, so the alternate branch is not a normal
default-restore path. Separate routines inline the assignment; absent direct
xrefs to this setter do not prove the callback cannot change elsewhere.
Threshold initialization and byte `+5` producers remain a bounded continuation;
neither is inferred from arbitrary raw-IDB bytes.

## Console emission guard and logger threshold producers identified

Console constructor zeroes the `+4/+5` word. Config loader raw `0xE4E210`
sets byte `+5` at `0xE4E263`, then clears it at `0xE4E32D`;
the interval includes config dispatch and callbacks. This is scoped emission
suppression, not a threshold update. Calling it a reentrancy guard requires
more than these writes; exact failure/nesting behavior is the next check.

Raw `0x12C1F90` updates logger-state **VA** `0x1E6B5D8` field `+0x10`,
the threshold **VA** `0x1E6B5E8`, and can log a change message.
Other helpers temporarily assign 4 and restore a saved threshold.
The field is in `.data`'s virtual tail, initially loader-zero-filled;
0 is its loader initial value, not a claim about later runtime state.
No console-variable registry/config-key identity was established.

## Dispatch unwind funclet observed; state-to-action binding still required

The bounded EH follow-up reported pushed **VA** `0x1A4AA4B`
(code raw `0x1609E4B`) leading to local-wrapper cleanup via
raw `0x60C960` with `[ebp-0xA8]`, not an observed global-name restore.
It also identified state 0/-1 writes at `0xC3B544/0xC3B56F`.
However, it did not decode the exact handler/FuncInfo/unwind-map routing
or establish whether that pushed target is a handler versus a cleanup funclet.
Therefore normal-path restoration is proven, but a blanket exceptional
non-restoration conclusion remains provisional pending that binding.
Absence of IDA-decoded EH maps does not prevent reading their underlying bytes.

## Config-load suppression is unconditional normal-path mute, not a nesting barrier

Raw `0xE4E210` calls its severity-2 helper at `0xE4E256`,
then sets console byte `+5` at `0xE4E263`, before path construction
and BufferFile at `0xE4E2BB`. It does not test BufferFile's return.
A normal false return still reaches buffer cleanup, the pump at
`0xE4E327`, and the clear at `0xE4E32D`; no ordinary early return
bypasses that clear.

The code saves no previous byte value: it assigns 1 then 0.
Consequently nested calls could clear an outer suppression interval;
this is not a reentrancy barrier or nesting-preserving guard.
The inspected EH entry **VA** `0x1A5F460`, raw `0x165E860`,
and nearby cleanup stubs yielded no guard-reset action. Normal clear is
proven; exceptional restoration was not established by that bounded inspection.

Sampled helpers raw `0x322C40` and `0x6E1A80` test `+5`,
but sink raw `0xE5AC70` has no corresponding entry check.
Therefore suppression cannot be generalized to all output routes without
following a concrete command/caller; that is the next bounded task.

## Dispatch EH map resolved: state-0 cleanup does not restore fallback global

Correction: handler **VA** `0x1A4AA4B` maps to raw `0x1649E4B`,
not `0x1609E4B`. The latter was an unrelated cleanup thunk.
Prologue raw `0xC3B3C0` begins
`55 8B EC 6A FF 68 4B AA A4 01 64 A1 00 00 00 00 50 81 EC 9C 00 00 00`,
registering the actual handler and initializing EH state -1.
The handler checks the frame cookie, loads FuncInfo **VA** `0x1D5E1E4`,
and jumps to raw `0x159DE38`, whose import stub reaches
**MSVCR120.dll!__CxxFrameHandler3**, IAT **VA** `0x1AC4814`.

FuncInfo at backed raw `0x195CDE4` has magic `0x19930522`,
maxState 1, unwind-map **VA** `0x1D5E208`, and zero try-block/IP-map
counts/pointers. Its sole entry, raw `0x195CE08`, is
`toState=-1, action=VA 0x1A4AA40`.
Action raw `0x1649E40`, bytes
`8D 8D 68 FF FF FF E9 15 DA 5A FF`, passes `[ebp-0x98]`
to raw `0xBF7860`.

State becomes 0 at raw `0xC3B544` before dispatch `0xC289C0`,
and -1 at `0xC3B56F` before normal wrapper cleanup.
The map's state-0 action performs that local cleanup, not restoration of
global **VA** `0x1E4891C`; the explicit normal restore is `0xC3B582`.
This resolves that specific unwind action, not every possible exception path
or whether ordinary VM errors escape through C++ unwinding.

## Print command binding parked; parser dispatch and mute are distinct

A bounded command search found `"PrintLn"` and `"Print"` candidates
but did not bind their registration/callbacks. Their output behavior is
unresolved; further identical string/reference scans are parked.
The pass observed console `+5` checks around selected diagnostic calls
in raw `0xE89C50`, while indirect command-dispatch calls
`0xE89DE1/0xE89E1A` have no such check at those sites.
This does not prove a specific command bypasses suppression, but provides
a concrete parser branch to inspect next.

The follow-up's `0xB4E5AC70` unmapped-address observation is irrelevant:
`+0xB4` was a vtable byte offset, not a prefix to code raw `0xE5AC70`.
It does not invalidate the earlier independently traced sink.

## Protected VM dispatch returns status and has a C++ error-transfer path

At raw `0xC3B551`, caller `0xC3B3C0` invokes `0xC289C0`
with VM state in ECX, the Boolean result of `0xC25E30` in EDX,
and stack arguments `(1, savedTopIndex+1, 0, 0)`.
The final zero selects protected wrapper `0xCD7890`, bypassing
the direct `0xCD7110` branch. That wrapper invokes callback
**VA** `0x1029540` through raw `0xCD6C10`.

Raw `0xCD6C10` saves VM error-frame pointer `+0x2C`, installs
a local frame with status `+8` zero, invokes the callback, then restores
the prior pointer and saved VM counter. Raw `0xCD7890` handles nonzero
status by unwinding to the call slot, placing the error value there,
and restoring VM state. Success returns zero.
Caller `0xC3B3C0` tests the status at `0xC3B592`, returns `status==0`,
and populates its optional result byte from VM-top truthiness only on success.
The output byte begins zero; `0xC278C0` restores the saved VM top.

Raiser raw `0xCD6B70`, when an error frame exists, writes its status
at frame `+8` and calls thunk raw `0x159E27A` to `_CxxThrowException`.
Thus this error transfer is C++-based, not a demonstrated C longjmp.
The protected wrapper's exact catch/type routing remains unbound; status
plumbing alone does not prove which native exceptions it catches or lets escape.
Nor does every nonzero status necessarily invoke the traceback logger.
During the dispatch interval, the borrowed global supplies fallback names
only for traceback frames whose own name is null.

## Parser diagnostic suppression does not gate matched command callbacks

The follow-up addresses here are raw IDB offsets, despite its report's
generic VA label. In raw `0xE89C50`, the clear
**VA** `0x1E49A59` branch looks up variables through slot `+0x50`;
a match invokes callback field `+0x1C`. Otherwise command lookup uses
slot `+0x4C`. Single-token dispatch invokes `+0x38` at `0xE89DE1`;
multi-token dispatch invokes `+0x98` at `0xE89E1A` with parsed arguments.
Neither dispatch-site call is guarded by console byte `+5`.
On failed command lookup, a leading `':'` suppresses the diagnostic call
at `0xE89E53..0xE89E6A`; otherwise its helper applies the emission gates.
Assignment-related diagnostics in `0xE41360` use the same helper;
independent flag branches can still bypass assignment handling.

Raw `0xE89C50` returns consumed input length; its caller `0xE89EC0`
separately updates `_` through console `+0x98` when the statement value
changes. ExecSkip remains a distinct dispatch-bypass mechanism.
Diagnostic format operands **VA** `0x1BE26D4/0x1BE26EC` require
PE-aware string mapping before their text is assigned to these branches.
The follow-up's unreadable-at-VA result does not establish missing strings.
Console `+0xB0` is the previously byte-verified `0xE5AB80`, distinct
from sink slot `+0xB4` / `0xE5AC70`; the two should not be conflated.

## Parser diagnostic text and assignment feedback mapped from the PE

Format **VA** `0x1BE26D4`, backed raw `0x17E12D4`, is
`Unknown command "%s"`. Raw `0xE89E5E..0xE89E6A` supplies token 0
after lookup failure unless the input starts `':'`.
When global **VA** `0x1E49A59` is set, the alternate branch bypasses
lookup and reports token 0, later tokens with `" %s"`
(**VA** `0x1BE26EC`, raw `0x17E12EC`), then a CR-only string.
The first token's exact format/argument forwarding and PrintConfig's mode
lifetime are the next concrete check; do not assume literal-string escaping.

Setter raw `0xE41360`, when `a4` is set, rejects metadata flag `0x200`
with `"\"%s\" is a protected value and cannot be changed"`
(**VA** `0x1BDDDA4`, raw `0x17DC9A4`), and flag `0x10`
with the analogous read-only message (**VA** `0x1BDDD6C`,
raw `0x17DC96C`). Each passes the variable name and returns before assignment.
Mapped `%f` constants at raw `0x17DC964/0x17DC968/0x17DC99C`
format numeric corrections.
The deferred flag-`0x400` path stores/marks a latched value and can report
`"The value has been latched"` (**VA** `0x1BDDD18`, raw `0x17DC918`),
with no explicit third helper argument at that call.
Immediate application has no explicit success message in this function.

## Protected VM wrapper catches C++ error transfer and returns status

Raw `0xCD6C10` registers handler **VA** `0x1A508B0`,
code raw `0x164FCB0`, which loads FuncInfo **VA** `0x1D648CC`
and reaches MSVCR120 `__CxxFrameHandler3` via IAT `0x1AC4814`.
The single try block covers states 0 through 0, catch-high 1, one handler.
HandlerType **VA** `0x1D64914` has null type pointer and handler
**VA** `0x10D786C`, raw `0xCD6C6C`: a C++ `catch(...)`.
The catch preserves nonzero status, substitutes -1 for zero, and resumes at
`0xCD6C7F`, the shared VM error-frame/counter restoration path.

Raiser raw `0xCD6B70` calls MSVCR120 `_CxxThrowException`,
IAT **VA** `0x1AC4770`, with ThrowInfo **VA** `0x1DB81FC`.
Catchable type descriptors are `.PAUlua_longjmp@@` and `.PAX`,
both 4-byte pointer types. The raiser first writes its error code to the
installed frame's status slot. This error transfer is therefore caught and
converted to the protected wrapper's returned status, rather than necessarily
escaping the caller's borrowed-name interval.
The metadata alone does not establish foreign/SEH exception translation or
asynchronous-exception handling; those remain outside this resolved contract.

## PrintConfig echo formatting and normal mode lifetime resolved

Addresses here remain raw IDB/file offsets: the follow-up's subtraction
of the code bias from `0xE8A9F0` was incorrect.
At raw `0xE8AB23..0xE8AB45`, the successful setup path invokes console
`+0xF4(1)`, pumps its local buffer via `0xE89EC0` with flag 1,
then invokes `+0xF4(0)`. Setter raw `0xE89EB0` stores the low argument
byte into **VA** `0x1E49A59` at `0xE89EB3..0xE89EBB`.
The ordinary sequence does not preserve prior mode; exceptional restoration
remains an EH-map continuation.

In raw `0xE89C50`, nonzero token count and that mode select echo instead
of lookup. At `0xE89CCA..0xE89CD8`, token 0 is supplied directly as
the format to console `+0xB0`, severity 2, null varargs pointer.
There is no caller-side `"%s"` wrapper for this token.
Later tokens use raw `0x322C40` with the already PE-verified `" %s"`
format; the final output fragment is CR-only at `0xE89D2F..0xE89D3C`.
Each path applies console enabled/suppression/threshold gates.
This describes forwarding only, not a demonstrated runtime outcome.

Pump flag 1 bypasses wait/deadline checks but not ExecSkip's positive-count
segment consumption (`0xE89F70..0xE89F91`). Consequently PrintConfig
mode does not guarantee every parsed segment reaches the echo branch.

## Protected VM error recovery normalizes one error slot and restores call state

Raw `0xCD6B00..0xCD6B5A` selects error values by status: 4 uses
the value at `[VM+0x0C]+0x90`; 6 interns 23 bytes from
**VA** `0x1BC7E6C`; other nonzero statuses copy the 16-byte value at
`[VM+8]-16`. All branches leave top `[VM+8]` at error slot +16.
The reported control-byte contents at `0x1BC7E6C` came from reading a VA
as raw and are not accepted as that message. Exact message and cached-value
ownership are the next check. Raw `0xCD5AB0` processes open entries at or
above the error slot before normalization.

For the one-argument protected call, the function slot is `[VM+8]-32`.
The saved index +1 resolves to a stack-relative slot temporarily installed
at `[VM+0x58]`; raw `0xCD7890` restores the prior field.
Failure restores `[VM+0x10]`, byte `+0x6D`, word `+0x68`,
and invokes `0xCD6EB0`, leaving the normalized error as the top value.
The outer `0xC278C0(state,savedIndex)` restores the original entry top;
nonzero status makes `0xC3B3C0` return false.
The shared catch cleanup at `0xCD6C7F` restores error-frame pointer
`[VM+0x2C]` and word `+0x6A` before returning status.

## PrintConfig EH cleanup releases locals but does not reset echo mode

Raw `0xE8A9F0` registers handler **VA** `0x1A61950`
(raw `0x1660D50`), loading FuncInfo **VA** `0x1D76214`
(raw `0x1974E14`). Magic is `0x19930522`, maxState 4;
try-block/IP-map counts are zero. Unwind map **VA** `0x1D76238`
(raw `0x1974E38`) contains:

| State | Next | Action VA |
|---|---|---|
| 0 | -1 | `0x1A61930` |
| 1 | 0 | `0x1A61938` |
| 2 | 0 | `0x1A61940` |
| 3 | 2 | `0x1A61948` |

State returns to 2 at raw `0xE8AACE`, before setter `0xE8AB2B`
and pump `0xE8AB3A`. Its unwind action passes `[ebp-0x44]`
to `0xDF2F60`, then state 0 passes `[ebp-0x50]` to `0x3A59D0`.
These release the holder/shared-text locals, not the global mode flag.
The setter with 0 at `0xE8AB45` is normal-return-only on this path;
the decoded state-2 unwind does not reset it.

## VM fixed errors are interned strings; status 4 reuses an allocation-failure cache

PE-mapped **VA** `0x1BC7E6C`, raw `0x17C6A6C`, contains the
23 bytes `"error in error handling"`. Status 6 requests its interning
via raw `0xCD4E50` at `0xCD6B30..0xCD6B35`.
Status 4 instead reuses `[VM+0x0C]+0x90`; initializer raw `0xCD4D40`
interns `"not enough memory"` from **VA** `0x1BC7C94`,
raw `0x17C6894`, and stores that object at `+0x90` at `0xCD4D6C`.
State setup raw `0xCD5460` calls that initializer.
Allocator helper raw `0xD675A0` raises status 4 via `0xCD6B70`
after allocation failure, concretely linking that code to the cached message.

Normalizer `0xCD6B00` writes the object pointer at slot `+0`,
tag at `+8` as object type byte `+4 | 0x40` (string type 4 gives `0x44`),
and advances top by 16. Other statuses copy the complete prior top value.
Before normalization, `0xCD5AB0` unlinks open entries at/above the slot,
copying their 16-byte values into entry-owned storage. Flagged values may
take `0xCD7CF0`; another branch releases the 24-byte entry via `0xD675A0`.
The fixed values are VM-interned objects, not caller-owned text buffers;
status 6 may obtain an already interned object.

## PrintConfig admits only successful whole-file loads into its local buffer

Addresses below are raw file/IDB offsets; several VAs in the follow-up
were arithmetically inconsistent and are deliberately not propagated.
PrintConfig raw `0xE8A9F0` resolves its path through `0x1318870`
and takes a shared-text reference through `0x3A5910`.
The resolver rejects input `strlen > 0x200`; leading `!` strips one
character and bypasses ordinary root-candidate search.

Loader raw `0x12BB660` uses UTF-8/wide wrapper `0x12BA030` and
the verified `_wfopen` import with `"rb"` (**VA** `0x1C15568`,
raw `0x1814168`). The wrapper converts path/mode with CP_UTF8,
source length -1, without checking conversion results.
The loader uses `_fileno` and a stat helper, fails/closes on stat error,
and reads a 32-bit length at stat buffer `+0x18`.
For nonempty input it reserves length+1 and requires exact-length
`fread(buffer,1,length,file)`. A short read resets the buffer, closes,
and returns false. Empty input skips reserve/read and succeeds with the
initial NUL buffer. Exact stat import/layout and capacity arithmetic are
the next bounded checks, not inferred from the helper's name.

The local facade starts with a one-byte heap buffer via `0x12B8030`;
successful reads append NUL, and PrintConfig releases the allocation.
Embedded NULs are loaded but scanner raw `0xE89790` stops at the first,
so later file bytes are not parsed. Open/stat/read failures precede the
mode setter; successful load alone reaches echo-mode set/pump/normal clear.
Loader's known direct callers include PrintConfig and BufferFile,
not the previously named ProcessFile entry.

## VM allocator retries nonzero-size failure once behind a context gate

Raw `0xD675A0` is fastcall: ECX VM state, EDX pointer, stack old/new sizes.
With context `[VM+0x0C]`, it calls `[ctx]` as
`(userdata=[ctx+4], pointer, oldSize, newSize)`.
If allocation returns null with nonzero new size, context `+0x8C` nonnull
permits one `0xCD9EC0` retry-helper call and a second allocation attempt;
no gate or second failure raises status 4 through `0xCD6B70`.
The retry helper's exact effects and gate identity remain open.

On return, accounting `[ctx+0x0C]` changes by
`newSize - (pointer ? oldSize : 0)`.
Zero new size does not retry/raise; a null result still subtracts old size
for a nonnull pointer. Context owns callback/opaque-userdata references
in this path; the wrapper neither copies nor frees userdata.
Setup raw `0xCD5460` writes gate `+0x8C`, but callback/userdata
initialization was not established.

## VM allocation retry requests a collection pass; marker identity remains open

On eligible allocation failure, raw `0xD675A0` calls `0xCD9EC0`
with VM/context in ECX and flag 1 in EDX, then repeats the allocator
callback with the same arguments. Raw `0xCD9EC0` sets context byte
`+0x3E` for the pass and clears it afterward. If phase byte `+0x3D <= 1`,
it advances to 2 and runs `0xCD9110`, which unlinks/retains list entries
and dispatches cleanup; four `0xCD9BE0` phase/mask steps follow.
These concrete effects support collection-retry interpretation.
The flag toggle alone does not establish a read-side reentrancy barrier.

Raw `0xCD5460` writes **VA** `0x1C740F8` into context `+0x8C`
after initialization calls. The marker's apparent raw-IDB fill bytes were
read without mapping that VA and are not accepted as its contents.
Its exact object/type and the allocator/userdata producer remain unresolved.
No static xrefs to setup were found by this pass; callback/indirect setup
remains possible and is the next concrete continuation.

## Shared loader truncates stat size to DWORD and has no reserve-result check

Immutable PE parsing identifies loader imports `_fileno` at **VA**
`0x1AC4A8C`, `_fstat64` at `0x1AC4700`, `fread` at `0x1AC490C`,
and `fclose` at `0x1AC49A8`, all MSVCR120.
The stat buffer is `[ebp-0x48]`; size read `[ebp-0x30]` is offset `+0x18`.
No upper-DWORD read/validation occurs. A zero low DWORD succeeds as empty;
nonzero uses 32-bit `length+1` via LEA without an overflow branch,
then `fread` count equal to that low DWORD.
Exact read appends NUL at data+returnedCount; short read resets the buffer.
Stat/read failures close and return false; close result is ignored.
The import proves `_fstat64`, not a target-local complete structure declaration.

Both caller facades use vptr **VA** `0x1C77A30`, raw `0x1876630`.
Slot `+0x1C` resolves to raw `0xDF47A0 -> 0xDF43F0`;
accessor `+4` is `0xDF47F0`, returning backing pointer plus offset;
reset `+0x14` is `0xDF4350`, clearing length fields and first byte.
Growth uses raw `0xDF2DC0`; the observed reserve caller stores its result
and updates capacity/length without a failure result checked by the loader.
The exact underlying allocation/reporting policy is a follow-up, not a
demonstrated runtime consequence of null data.

## Concrete VM creation installs allocator/userdata before protected initialization

Creator raw `0xC347E0` calls `0xCD5720` at `0xC34834`,
supplying allocator **VA** `0x103CCB0` and incoming `a1` as userdata.
The creator allocates 912 bytes, storing allocator/userdata at result
`+0x78/+0x7C` (`0xCD576C/0xCD576E`; the earlier 120/124 were decimal).
These are not the separate initialization callback.
At `0xCD577F`, **VA** `0x10D6060` (raw `0xCD5460`) is loaded
into EDX; protected-call helper `0xCD6C10` is invoked at `0xCD58A4`
and dispatches that callback at `0xCD6C65`.

Initializer `0xCD5460` reads the context from argument `+0x0C`.
After helper `0xD67690`, raw `0xCD5492..0xCD5496` sets context byte
`+0x3F` to 1 and `+0x8C` to **VA** `0x1C740F8`.
Creation initially zeroes `+0x8C` at `0xCD57C7`, supporting a
late-initialization/retry-eligibility gate rather than a thread identifier.
PE mapping places that marker at raw `0x1872CF8`, whose leading byte is
NUL; this proves neither a source-level name nor overall initialization success.
The concrete allocator callback's implementation/userdata use is next.

## Buffer growth copies capacity and can discard old storage on returned failure

Raw `0xDF43F0` uses pointer `+8`, capacity `+0x10`, used length `+0x14`,
and shrink floor `+0x0C`. Growth passes old capacity, not used length,
to `0xDF2DC0`. Successful replacement copies
`min(oldCapacity,newCapacity)` bytes, then frees old storage.
If allocation returns null, that helper still frees old storage and returns null;
reserve stores the result and updates capacity/length without a null-result branch.
With no old pointer it returns the allocation directly; old pointer plus zero
new size frees and returns null.

Shrink at/below the floor only changes used length; otherwise it reallocates
to `max(requestedSize,floor)` and sets capacity/length to that value.
Allocator wrapper `0x12B8030` calls `0x12C8A20` and reports a null result
through **VA** `0x1C15EA0` before returning zero if reporting returns.
That reporter's continuable assertion mechanism is already documented;
the follow-up's empty target resolution does not reopen its identity.
Exact allocation-call flags/report arguments remain a bounded continuation.
All null-result continuation claims are conditional on reporter return.

## Concrete VM allocator uses global heap and ignores userdata on inspected paths

Allocator **VA** `0x103CCB0` maps to raw `0xC3C0B0`.
Initial creation calls `(userdata,NULL,8,912)`; callback/userdata are stored
at allocated base `+0x78/+0x7C`, correcting the earlier decimal-offset
misreading as hex `+0x120/+0x124`.
Owner `0xC347E0` retains the VM at its `+0x78`.
Initial null allocation returns null; the owner conditionally logs and returns false.
Later initialization failure uses `0xCD5510` to release VM allocations and
invokes `(userdata,base,912,0)` for the outer block.

Callback raw `0xC3C0B0` frees zero-size requests through `0x12CA6F0`;
nonzero goes through `0x12CAE30`, global heap **VA** `0x1E48918`,
and `0x12CA410`. Resize tries in-place adjustment `0x12C94D0`
before allocate/copy/free fallback. Inspected paths do not dereference
userdata or oldSize, so selection is global rather than userdata-specific.
A reported null-result path logs diagnostic 4528, frees a nonnull prior
pointer, and returns null. Its exact interaction with the VM wrapper's
same-pointer retry remains a pointer-lifetime follow-up, not a runtime finding.

## VM failed-resize callback releases the pointer before the wrapper's same-value retry

Raw `0xD675C0` saves the incoming pointer. Eligible recovery calls
`0xCD9EC0(VM,1)` at `0xD675E3..0xD675E8`, then
`0xD675ED..0xD675FA` reloads the same saved pointer and sizes for retry.
Callback raw `0xC3C0B0`, after `0x12CAE30` returns null, logs 4528
at `0xC3C120` and passes its original pointer to `0x12CA6F0`
at `0xC3C13E..0xC3C140` before returning null.

The inner resize helper first attempts `0x12C94D0`, then allocation
`0x12CA410`. Successful fallback copies/frees old storage at
`0x12CAF14..0x12CAF20`; failed fallback branches to null return
at `0x12CAEEA/0x12CAF25..0x12CAF2D`, preserving old storage there.
Thus the callback, not that failed inner allocation branch, releases the old
allocation before the wrapper retries with its saved pointer value.
This is a static resource-contract ledger, not proof of runtime reuse,
dereference, or exploitability. Zero-size requests bypass wrapper retry.

Layout is now explicit: allocation base B, VM=B+4, embedded context
S=B+0x78=VM+0x74; `0xCD54B0` sets `[VM+0x0C]=S`.
At `0xCD576C/0xCD576E`, bytes `89 06` and `89 5E 04`
store callback/userdata at `[ESI]/[ESI+4]`, with ESI=S.
Raw `0xCD5771` sets `[S+0x88]=VM`, and `0xCD57C7` clears
retry gate `[S+0x8C]`. This context is not the process-global heap object.

## Buffer OOM wrapper uses the known continuable assertion reporter

Raw `0x12B8030` calls allocator `0x12C8A20`; null formats
`"OOM allocating %lu bytes."` from **VA** `0x1C5EBB8`,
raw `0x185D7B8`, with requested size.
It calls reporter slot **VA** `0x1C15EA0` with the same
`0x1C5EA58` pointer twice, line `0xDA` (218), and the formatted buffer.
The previously verified reporter raises code `0x12ED502` with flags 1;
line 218 is not that flags argument. If reporting returns, this wrapper
returns null, preserving the conditional continuation in the buffer ledger.

The allocator's region-growth call at raw `0x12C771A` resolves to
`VirtualAlloc(NULL,size,0x3000,4)` (reserve+commit, read/write).
A failed call returns zero with no retry/fallback observed in that path.
This identifies one region-growth boundary, not all small-block allocation
policy; region metadata and matching release are the next task.

## Heap free coalesces chunk metadata and conditionally takes an internal lock

Raw `0x12CA6F0` returns immediately for a null pointer, without locking.
For nonnull input it uses chunk header `pointer-8`, coalesces adjacent free
chunks, and updates boundary tags, free-list/bitmap state, and heap top fields.
Insertion decrements heap `+0x20`, calling raw `0x12C78A0` at zero;
the direct-release path subtracts released bytes from heap `+0x1B0`.
Exact region-release ownership is a separate active trace.

Internal synchronization tests heap flag `+0x1BC & 2` and conditionally
enters/leaves the critical section at `+0x1C0`. The observed initializer
clears that flag; this alone does not establish a complete concurrency policy
or exclude outer locking/later reconfiguration.
Raw `0xC327D0` publishes raw `0x12CA210`'s allocator-object result
into global pointer slot **VA** `0x1E48918`; the slot is virtual-tail data,
not the raw-IDB bytes at its numeric VA. Callers load that pointer into ECX.
Concrete creation options and surrounding synchronization are the next check.

## Heap region growth publishes metadata only after successful VirtualAlloc

Addresses here are raw IDB/file offsets. The follow-up incorrectly treated
raw `0x12C771A` as a VA and subtracted the code bias; that alternative
`0xEC6B1A` is not accepted as the call site.
Allocation raw `0x12C8A20` first searches existing bins/free space.
Its request normalization uses 16 for n<11, otherwise `(n+11)&~7`,
rejecting requests at or above `0xFFFFFFC0`.
This is not a VirtualAlloc call for every allocation.

Initialization raw `0x12C6A90` obtains GetSystemInfo and uses allocation
granularity **VA** `0x1EDAFC0`, checked as a power of two.
Region helper raw `0x12C7690` computes
`(internalRequest + granularity + 0x2F) & ~(granularity-1)`,
checks wrap and configured heap limit `+0x1B8`, then requests reserve+commit,
read/write. Successful allocation updates total `+0x1B0` and high-water
`+0x1B4`; failure precedes those updates and metadata publication.

Inline region record `+0x1D8` contains base, size, next, flags.
Raw `0x12C7400` copies the prior 16-byte record near the old top region's
end and publishes the new inline record. Free-side trim raw `0x12C7BF0`
and collection raw `0x12C78A0` use VirtualQuery validation and
`VirtualFree(address,0,0x8000)` release.
PE-verified IAT VAs are `0x1AC42A8` VirtualQuery,
`0x1AC42B4` VirtualFree, and `0x1AC42B8` VirtualAlloc.
The linked traversal begins at `+0x1E0`; inline-seed retention,
flag-bit gates, and partial-release failure bookkeeping remain unresolved.

## Global VM heap disables its internal lock bit after creation

Raw `0xC327D0` conditionally creates the heap only when global pointer
**VA** `0x1E48918` is zero, publishing the result at
`0xC32BF4..0xC32C01`; otherwise it reuses the pointer.
Raw `0x12C6A90` initializes config flags to 7, and `0x12CA110`
copies/ORs 4 into heap `+0x1BC` and initializes CS storage `+0x1C0`
through IAT **VA** `0x1AC41BC` with `0x80001020`.
The import's exact name remains a PE-parser continuation.
Factory `0x12CA210` then clears mask 2, yielding flags 5,
and sets heap `+0x1E4` to 1.

Allocation/resize/free internal lock branches test mask 2 and are skipped
for this initial flags value. The inspected VM callback has no visible outer
heap lock. A separate factory `0x12CA280` can set/clear mask 2;
that does not establish reconfiguration of this concrete global heap.
No teardown/reset was established, and external serialization is unknown.
These observations describe flag selection, not overall thread safety.
The earlier follow-up's unresolved VirtualAlloc label does not reopen the
already PE-verified IAT **VA** `0x1AC42B8`.

## Region collector retains the inline root and commits accounting after full release

Raw `0x12C78A0` starts candidates at heap `+0x1E0`, with predecessor
record at `+0x1D8`: the inline record is the root, not itself traversed.
Raw `0x12C7400` copies the prior 16-byte root record into region metadata
before replacing the inline base/size/link/flags; the growth caller supplies
flags 1. Eligible linked records require bit 0 set, bit 3 clear, and the
observed free-block/boundary checks. No broader flag names follow yet.

Each release requires VirtualQuery success, BaseAddress and AllocationBase
equal to the query address, committed state `0x1000`, and RegionSize no
larger than remaining bytes, then VirtualFree(address,0,0x8000).
Only complete record release adds to returned bytes, subtracts heap `+0x1B0`,
and unlinks via predecessor `+8` (`0x12C7AB2..0x12C7ABF`).
On query/free failure, the record/counter remain, free-block metadata is
reinserted (`0x12C7AC4`), and traversal continues.
Heap `+0x20` becomes visited-record count capped at `0xFFF`, not released count.

Trim raw `0x12C7BF0` selects the top-containing record and avoids trimming
one containing the inline-root address. It uses the same flag gate and releases
an aligned tail. Record-size/counter/top updates occur only after the full
requested trim succeeds (`0x12C7CE0..0x12C7CE6`).
If a later span fails after earlier OS releases, no rollback of those releases
or partial bookkeeping update was observed. The separate collector can still
release other records; outer success reflects a nonzero reported released total.
This is static branch/bookkeeping evidence, not an observed partial failure.

## Global heap lock initialization import and construction chain identified

Immutable PE parsing resolves IAT **VA** `0x1AC41BC`
(raw `0x16C2DBC`) as **KERNEL32.dll!InitializeCriticalSectionAndSpinCount**.
Global allocator config setup raw `0x12C6A90`, call `0x12C6B14`,
passes CS **VA** `0x1EDAF88` and `0x80001020`;
heap initialization separately initializes its `+0x1C0` CS.
The internal heap lock mask is **2** (bit index 1), not mask 4:
factory clearing that mask leaves flags 5.

Observed construction chain is raw
`0xBFCA30 -> 0xC34100 -> 0xC327D0 -> 0x12CA210`.
Raw `0xC34100` allocates `0x500` bytes and calls the owner constructor
at `0xC3414D..0xC3416B`.
Global-heap publication remains null-guarded; a direct appinit/CRT caller,
external callback serialization, and balanced heap teardown were not established.
The next task follows the concrete UI-owner field rather than repeating
an unsuccessful direct-caller search for `0xBFCA30`.

## Automatic heap trim and periodic collection triggers identified

In raw `0x12CA6F0`, a free merging into the top chunk grows heap `+0x0C`.
If that size is strictly greater than threshold `+0x1C`, it calls
`0x12C7BF0(0)` at `0x12CA8A3..0x12CA8B0`.
Initialization `0x12C6A90` sets global **VA** `0x1EDAFC8` to
2 MiB; `0x12CA110` copies it to heap `+0x1C`.
Regular-bin insertion instead decrements countdown `+0x20` and calls
`0x12C78A0` when the prior value is 1 (`0x12CAD7B..0x12CAD82`).
Top-chunk/direct-release paths bypass that decrement.
Factory countdown starts at `0xFFF`.

Trim also invokes collection when its size argument is below `0xFFFFFFC0`
and a top chunk exists, even if its own trim reported zero.
Other observed trim callers `0x12C8D60/0x12C9C20/0x12CB030`
are not expanded here.
The new report disputes two prior details: trim VirtualFree may decommit
rather than release, and countdown reset may be max rather than min of
`0xFFF` and visited count. Neither conflicting interpretation is treated
as settled until exact call arguments and cmp/cmov bytes are recorded.

## Scripting-object VM initialization follows construction; UI +A00 binding remains open

Addresses below are raw IDB offsets, not the follow-up's generic VA label.
Raw `0xC34100`, after allocating/constructing the `0x500`-byte object,
calls `0xC34450` at `0xC3417B`. That routine invokes `0xC347E0`
at `0xC346BB`; the latter checks object `+0x78`, creates the VM through
`0xCD5720`, and stores the result at `0xC3480E..0xC3483C`.
Null creation and later chunk-load failure return failure; those later
failure paths do not locally clear the newly stored VM pointer.
Success reaches `0xC34BE1`.

On initialization failure, `0xC34100` calls `0xC341C0` with the newly
constructed object and original owner, then returns zero
(`0xC34186..0xC3418D`). Nonnull cleanup proceeds through `0xC33060`;
exact VM release/reset in that downstream path is the next task.
The null-object branch reports an assertion then calls `0xBFCF30(0)`.

UI-manager constructor raw `0xBF7A80` clears `+0xA00/+0xA04/+0xA08`
at `0xBF8186..0xBF819A`. Destructor `0xBF8AC0` checks `+0xA04`,
calls that object's virtual `+0x10`, then clears `+0xA04`
at `0xBF8AFA..0xBF8B0B`.
Caller `0xBFCA30` consumes its created object's `+0x70` label and a
collection at its own `+0x5E4`, not a proven `+0xA00` publication.
The missing UI-context identity edge remains a bounded stop; these objects
must not be merged solely from their shared VM field offset.

## Exact heap release operands and signed countdown reset supersede prior conflicts

The immutable-byte arbitration finds three IAT `0x1AC42B4` calls,
all `FF 15 B4 42 AC 01`, with these arguments:

| Raw call | Pushes immediately before call | Arguments |
|---|---|---|
| `0x12C7A91` | `68 00 80 00 00 6A 00 57` | `(EDI,0,0x8000)` |
| `0x12C7CC1` | `68 00 80 00 00 6A 00 56` | `(ESI,0,0x8000)` |
| `0x12C7DB5` | `68 00 80 00 00 6A 00 56` | `(ESI,0,0x8000)` |

The previously PE-verified VirtualFree import therefore receives MEM_RELEASE,
not MEM_DECOMMIT, including in the disputed trim path.
The third call belongs to raw `0x12C7D30..0x12C83A0`, whose role is
a new bounded continuation. No fixed 4096-byte RegionSize is inferred from
the reported `0x1000` query check without decoding its structure field.

At raw `0x12C7B4E..0x12C7B5C`, bytes
`B9 FF 0F 00 00 8B C6 3B F9 0F 4F CF 89 4B 20`
load ECX=`0xFFF`, compare EDI with ECX, select EDI with signed CMOVG,
and store ECX at heap `+0x20`.
Thus reset is `max(0xFFF, (int32_t)visitedCount)`, not a cap/minimum.
Earlier capped-count statements are superseded by these exact instructions.

## Shared chunk free differs from collection and trim; query State check clarified

Raw `0x12C7D30` is a shared free/coalescing routine, not inherently a
destructor. Its direct region-release branch requires chunk-header low two
bits clear. Batch-free caller `0x12C9C20` clears a nonnull pointer slot
before passing chunk header/size; adjacent entries may coalesce.
Splitting caller `0x12C9860` tags split chunks with mask 2, selecting the
ordinary free-list path instead of this VirtualFree branch.

Its query buffer has size `0x1C`. Offsets `+0/+4` (BaseAddress/
AllocationBase) must match ESI; `+0x10` (State) must equal `0x1000`;
`+0x0C` (RegionSize) must not exceed remaining extent.
This corrects the arbitration report's mistaken fixed-RegionSize wording.
Only after complete release at `0x12C7DB5` does it subtract
`previousSize + a3 + 16` from heap `+0x1B0`.
Query/free failure leaves accounting unchanged; a query-success/field-mismatch
path can still return nonzero, so that raw return is not blanket release success.

## Scripting initialization failure closes VM before freeing object and removing registry identity

Caller raw `0xBFCA30` derives a string prefix by splitting its input at `'.'`
and passes that string to `0xC34100`, not an owner object pointer.
The newly constructed object is ECX for `0xC34450`; the string is a
separate stack argument. On failure it calls `0xC341C0`, and the caller
does not insert the new object into its success-only structures.

For nonnull cleanup, `0xC341C0` calls `0xC33060(object)`, releases
the object through `0x12C8D60`, then calls `0xBFCF30` through global
**VA** `0x1E002B8` with the object's pointer value.
Raw `0xC33060` removes matching references via `0xBF9BD0`
(global-context list `+0x6F0`), performs `0xC3E990(object,1)`,
and releases removed-entry references through `0xC33E60`.

At `0xC33281`, bytes
`8B 43 78 85 C0 74 0E 8B 40 0C 8B 88 88 00 00 00 E8 7A 22 0A 00`
follow object `+0x78` through the VM context's `+0x88` main VM pointer
and invoke `0xCD5510`. That close drains/finalizes VM queues, releases
buffers, and frees base `VM-4`, size `0x390`, through the stored allocator.
No object `+0x78` clear is needed to describe this observed sequence:
the enclosing object is freed next, not shown reused.
The later `0xBFCF30` registry pointer usage remains a separate bounded check;
the order alone does not establish a stale-pointer dereference.

## Batch free consumes pointer slots in array order and may merge adjacent entries

Raw `0x12C9C20` receives heap in ECX, pointer array in EDX, count on stack;
veneer `0x12CB020` tail-jumps without changing those arguments.
The plain return leaves stack cleanup to the caller.
Null entries are skipped; nonnull entries are cleared at `0x12C9C65`
before header validation and handoff to `0x12C7D30` at `0x12C9CC4`.
Exactly adjacent next entries can be merged, updating the header and replacing
the next slot with the merged pointer for its subsequent iteration.
The routine does not sort the array.

Normal completion returns zero, not a released-byte/failed-entry count.
It conditionally trims at `0x12C9CDD..0x12C9CE3` when top size
exceeds threshold, then releases the optional lock.
The validation branch `0x12C9C6E..0x12C9C80` follows slot clearing;
its error call at `0x12C9D06` remains unidentified. INT3 padding alone
does not establish the external error callee's return/termination contract.
No higher-level direct caller beyond the veneer was established.

## Registry cleanup uses pointer identity; missing-key erase claim needs exact range binding

Raw `0xBFCF30` conditionally clears manager `+0x574` when equal to
the supplied pointer, removes the first matching pointer from array
`[+0x68C,+0x690)` by shifting the tail, then processes hash container `+0x5EC`.
Lookup `0xC0A2A0` receives a stack copy of the pointer, hashes its four bytes
through `0xC1F4D0`, and compares node `+8` keys.
This does not dereference the released target object's fields.
Range erase `0xC0A1B0` disposes registry nodes through `0x12C8D60`.

The follow-up reported sentinel/sentinel lookup results on miss and interpreted
an erase optimization as unconditional full clear via `0xC07560`.
Exact first/last versus head/begin comparisons were not returned; that
surprising miss behavior remains provisional until range arguments and branch
bytes are bound. Do not equate empty range with full range from symbol names.
Zero input reaches assertions (caller line 2392, cleanup line 1491) before
continuation if reporting returns; the known reporter is not newly unresolved.

## Registry miss is a no-op in a nonempty container, not a full clear

The follow-up supplied useful instruction bytes but mislabeled IDB offsets
as VAs and subtracted the code bias. Addresses below retain raw IDB offsets;
its alternative `0x7FC3E2/0x809729/0x8095BE` mappings are not propagated.
At raw `0xBFCFE2`, the erase call receives registry `this+0x5EC` in ECX
and stack arguments `(&key,begin,end)`, pushed end, begin, key pointer.
On miss, raw `0xC0A329` bytes `8B 45 08 89 38 89 78 04`
store the head into both endpoints.

Raw `0xC0A1BE`, bytes `3B 01 75 19 3B D1 75 15`, compares
begin with `head->next`, then end with head. Only that full-range pair
takes `0xC07560`; otherwise equal endpoints return without erasing.
Therefore an absent key in a nonempty registry is a no-op.
For an empty self-linked registry, head/head also denotes the full empty
range, and clearing merely resets the already empty container.
This supersedes the earlier missing-key/full-clear interpretation.
Container fields are head `+0`, count `+4`, bucket storage `+8`;
nodes use next `+0`, previous `+4`, pointer-value key `+8`.

## Batch-free validation calls CRT abort, not the continuable project reporter

The guard at raw `0x12C9C6E..0x12C9C80` checks chunk header
`h=pointer-8`: unsigned h below heap `+0x10`, or
`(DWORD[h+4] & 3)==1`, takes the error branch.
The lower-bound field is maintained from VirtualAlloc bases at `0x12C6E70`.
This guard does not compare a magic constant or establish full owner identity.
Slot clearing precedes it; there is no upper-bound check in this guard.

Raw `0x12C9D06`, bytes `FF 15 24 4A AC 01`, calls
**MSVCR120.dll!abort**, PE-verified IAT **VA** `0x1AC4A24`,
raw `0x16C3624`, without arguments.
With internal locking enabled, this branch precedes normal LeaveCriticalSection
at `0x12C9CF8` and has no local unlock. This is the external CRT abort
boundary, not an inference from the following INT3 padding.
The header's first-word interpretation remains a producer-side follow-up.

## Scripting success publishes into distinct prefix tree and pointer hash set

Raw `0xBFCA30` indexes manager `+0x5E4` by the created object's
shared-text prefix at `+0x70`. Helper `0xC07380` returns its mapped-value
slot; `0xBFCC05` stores the object pointer there. Insertion helpers
`0xC09C10 -> 0xC173D0/0xC17480` use node shared-text key `+0x10`
and pointer value `+0x14`, retaining the key and initially nulling the value.
The stored object pointer has no visible AddRef in this path.

Duplicate-prefix precheck at `0xBFCBAE` reports/asserts. If reporting
returns, lookup reuses the existing node and replaces its pointer value;
this is not duplicate-node insertion. Prior-object lifetime in that case
was not established.
Success path `0xC38400 -> 0xC53B90` separately inserts the raw object
pointer into hash set `+0x5EC`, distinct from the prefix-keyed tree.
No direct success-path assignment to selection slot `+0x574` was found
in the inspected routines; its removal-time equality clear does not prove one.

Raw `0xBFCF30` removes pointer-set/array entries but does not yet establish
prefix-tree removal. The next bounded trace follows that tree's lifetime
relative to object destruction. The containers' raw-pointer storage alone
does not establish the manager's complete ownership/refcount policy.

## Chunk first-word meaning differs between direct mappings and boundary-tag chunks

Direct-mapping helper raw `0x12C6E70` computes
`R=(granularity+request+30)&~(granularity-1)`, obtains a region,
sets `pad=(-base)&7`, header `h=base+pad`, and stores pad at h,
size `S=R-pad-16` at h+4 with low flags clear.
It writes fence values 7 at h+S+4 and 0 at h+S+8, returning h+8
(`0x12C6EB9..0x12C6F2B`). Paired raw `0x12C8D60`
recovers base by subtracting pad and extent `pad+S+16`.
Here the first word is mapping-base padding, not previous-chunk size.

Ordinary allocation raw `0x12CA410` uses chunk size
`max(16,(request+11)&~7)`, returns h+8, and stores allocated size|3.
For ordinary chunks with previous-in-use clear, h holds previous-size
boundary metadata; free/coalescing writes size into the following chunk's
first word while clearing that flag (`0x12C8218..0x12C8226`).
Thus the first word must not be universally labeled alignment adjustment.

Aligned helper raw `0x12C9860` rounds alignment to a power of two,
requests need+alignment+12, aligns the user pointer, and releases a large
enough prefix through ordinary coalescing. Its header uses
remainingSize|oldPreviousInUse|2 (`0x12C994D..0x12C996B`).
Alignment/remaining-size checks can reach abort.
Direct-mapping selection in `0x12C7690` is flag/threshold gated;
exact configuration/fallback is the next bounded question.

## Prefix replacement does not release old value locally; failure cleanup precedes publication

The bounded follow-up confirms raw `0xBFCBC1`'s duplicate assertion,
if returning, continues to `0xC07380` and stores the new pointer at
`0xBFCC05` without reading/releasing the prior mapped value locally.
Its broader replacement ownership policy remains unresolved.

Crucially, the only identified direct caller of `0xC341C0` is
`0xC34100`'s construction-failure path, before registry insertion.
Absence of prefix-tree erase in that failure cleanup does not establish
missing erase for successfully published objects.
Repeated tracing of that path is parked. The next task follows actual
manager shutdown/destructor and bulk prefix-container cleanup.

## Direct mapping is threshold gated and falls back to normal region growth

Raw `0x12C6A90` initializes **VA** `0x1EDAFC4` to `0x40000`
(256 KiB), trim threshold `0x1EDAFC8` to `0x200000`,
and flags `0x1EDAFCC/0x1EDAF84` to 7.
Raw `0x12C7690` attempts direct mapping when heap flags `+0x1BC`
include mask 1, normalized request meets the threshold, and top-size
field `+0x0C` is nonzero. Factory flags 5 preserve that direct-map mask.
The prior report's `0x12C327D0` publication address was a typo for
the previously verified raw `0xC327D0`.

Raw `0x12CA410` searches bins/available chunks first. A failed direct
attempt in `0x12C7690` falls through to normal region growth, not a new
bin search. Both paths account the full region extent at `+0x1B0`
and update peak `+0x1B4`; direct mapping also updates minimum base `+0x10`.
The new report gives direct VirtualAlloc flags `0x103000`, versus normal
`0x3000`; exact operands and its inconsistent round-up notation require
a narrow byte-backed follow-up before assigning further flag semantics.

## Direct allocation uses 0x103000; exact size expressions are mask operations

Raw helper `0x12C6E70` computes
`(granularity + normalizedRequest + 0x1E) & ~(granularity-1)`
with 32-bit arithmetic. Its inspected push sequence at
`0x12C6EAF..0x12C6EB9` supplies protection 4, allocation type
`0x103000`, computed size, and null address to the already PE-verified
VirtualAlloc slot **VA** `0x1AC42B8`.
The additional `0x100000` bit is MEM_TOP_DOWN; ordinary fallback instead
uses `0x3000` and expression
`(granularity + normalizedRequest + 0x2F) & ~(granularity-1)`.
Neither expression should be rewritten as conventional round-up of the whole
sum, which would add another granularity-1 before masking.

With heap limit `+0x1B8` nonzero, both paths reject unsigned
`current+size <= current` or sum above that limit; a zero limit skips that check.
Failed direct allocation falls through to the ordinary growth path.
The report's final alternative raw addresses `0x12862AF/0x1286B10`
are inconsistent with the raw-IDB basis and are not propagated.
The next trace follows direct-chunk resize through `0x12C94D0`.

## Manager shutdown drains published prefix payloads before releasing key nodes

Vptr **VA** `0x1C73EBC`, raw `0x1872ABC`, contains:
slot `+0x10` bytes `B0 9D FF 00` -> raw `0xBF91B0`;
`+0x14` bytes `D0 9D FF 00` -> raw `0xBF91D0`;
`+0x34` bytes `D0 AC FF 00` -> raw `0xBFA0D0`.
Shutdown calls `+0x34` via `0x30D150` before `+0x10/+0x14`.

With manager byte `+0x671` set, raw `0xBFA0D0` drains registry `+0x5E4`:
payload cleanup `0xC33060` and release/tracking precede
node erase `0xC09D10`. Node cleanup separately releases shared-text
key `+0x10` and frees the tree node.
Later `0xBF91D0` sweeps remaining entries; destructor `0xBF8AC0`
also clears the tree via `0xC0CBF0`.
This supplies the actual published-object bulk path missing from the
pre-publication failure trace.
Inside `0xC33060`, stored items receive virtual slot 0 with flag 1;
the dynamic item types/targets remain unresolved, so deleting-destructor
interpretation is not an established concrete deletion contract.
The readiness byte's producer/phase meaning is next.

## Direct-mapped resize is a no-write eligibility check followed by optional move

Raw `0x12C94D0` routes low-two-bit-zero chunk headers through
`0x12C6F40` at `0x12C9521..0x12C9529`.
That helper returns the same chunk only when normalized request >=`0x100`,
current size >=request+4, and size difference <=2*granularity.
Otherwise it returns null, making no calls, header writes, or accounting changes.
It does not grow/shrink the mapping itself.

Outer raw `0x12CAE30` converts user pointer to header and back.
On failed eligibility, successful fallback `0x12CA410` copies
min(request,old usable capacity), frees old allocation, and returns new.
Failed fallback returns null preserving old allocation in this helper;
the separately traced VM callback can then free it.
Ordinary chunks instead enter split/coalesce logic `0x12C953B..0x12C984B`,
whose complete transition cases are the next task.

## Ordinary in-place resize splits at 16 bytes except for positive top remainder

Raw `0x12C94D0` accepts ordinary header low bits 2/3 after validation.
Shrink retains the whole chunk for slack <16; otherwise it splits a tail
and sends it to `0x12C7D30` (`0x12C9538..0x12C9580`).
Growing into top requires combined size strictly greater than request,
then moves heap top pointer/size `+0x18/+0x0C`.
Unlike other split paths, this branch has no 16-byte remainder check;
positive aligned slack can be 8 (`0x12C9581..0x12C95C0`).

Growing into last remainder updates `+0x14/+8` for slack >=16,
or absorbs the whole remainder and clears those fields for smaller slack
(`0x12C95C1..0x12C963A`).
Other adjacent free chunks are unlinked from small bins (<`0x100`) or
tree bins, then absorbed for slack <16 or split/coalesced for larger slack
(`0x12C963B..0x12C9853`).
Busy/insufficient neighbors return null before the observed resize writes;
the outer fallback can allocate/copy/free while preserving old storage
if allocation itself fails.

These in-place transitions do not write region-byte accounting `+0x1B0`.
Consistency-error calls through **VA** `0x1AC4A24` are the already
PE-verified MSVCR120 abort boundary, not the raw-IDB apparent `0x08080808`.
The explicit normal-failure no-write observation does not generalize to
all partially executed consistency-error branches.

## Manager drain gate covers partial initialization, not only successful startup

Constructor raw `0xBF7A80` writes word `this+0x670=1`,
so byte `+0x671` starts zero, and clears byte `+0x672`.
Init raw `0xBFB310` first invokes virtual `+4`; nonzero returns zero
before arming cleanup. Otherwise it sets `+0x671=1` at `0xBFB350`.
Later failure at `0xBFB668` leaves that byte set; only success sets
`+0x672=1` at `0xBFB8DC`.
Thus `+0x671` supports partial-initialization cleanup, not just a success flag.

Shutdown helper `0x30D150` calls slot `+0x34` at `0x30D1EB`.
Target `0xBFA0D0` skips when `+0x671` is zero; otherwise it clears
the word at `+0x671` (both cleanup/success bytes) and drains the prefix
registry, calling `0xC33060` at `0xBFA278`, then `0xC09D10`
at `0xBFA29C`.
Subsequent slot `+0x10`, raw `0xBF91B0`, invokes the object at
manager `+0xA04`'s virtual `+0x10(1)` and clears `+0xA04`.
Slot `+0x14`, raw `0xBF91D0`, sweeps remaining prefix/nested tracking entries.
Neither changes the two initialization bytes.
The normal-startup indirect binding to `0xBFB310` remains the next task.

## Realloc copy bound uses ordinary size minus 4, not minus 8

Raw `0x12CAE30` reads size/flags at user pointer-4, masks `0xFFFFFFF8`,
and subtracts 8 for low-two-bits-zero direct mappings, otherwise 4.
Fallback copy at `0x12CAF14` uses min(original requested bytes, that bound),
then frees old allocation at `0x12CAF20`.
Raw `0x12C9D10` independently uses the same subtraction rule.
This supersedes earlier ordinary usable-capacity statements of chunkSize-8:
the observed copy ceiling is ordinary chunkSize-4, direct chunkSize-8.
Exact next-header/previous-size word lifetime explaining this distinction
is the next bounded check.

Null old pointer delegates to `0x12CA410`; nonnull with zero request frees
and returns zero. Requests >=`0xFFFFFFC0` return zero before that free path.
Wrapper `0x12B8030` passes requested size only and does not query capacity.
No separate usable-size helper call was found on the inspected copy path;
the bound is computed inline.

## Ordinary usable tail word becomes successor previous-size metadata only after free

Addresses below remain raw IDB offsets despite the follow-up's VA label.
Allocation raw `0x12C8A20` returns header+8, writes allocated size|3,
and marks the successor's previous-in-use bit
(`0x12C8BB5..0x12C8BCB`, `0x12C8CD8..0x12C8CFC`).
The successor's first word is not required for backward coalescing while
that bit is set. This accounts for ordinary usable size C-4: the last
four usable bytes overlap that otherwise-unused successor previous-size word.

Free raw `0x12C8D60`, including `0x12C92E0..0x12C92E9`,
clears the successor's previous-in-use bit and writes freed size at H+C.
Only then does that word serve as a valid free-chunk footer.
Direct mappings retain the distinct C-8 capacity rule.

The follow-up incorrectly described `(n+11)&~7` as ceil-to-8 of n+11
and overclaimed its slack. The mask rounds that sum down to a multiple of 8;
no `C-4 >= n+7` guarantee follows. Exact normalization comparisons and
small/upper-bound cases are the next arithmetic check.

## Manager init table slot confirmed; Login startup receiver remains conditional

Manager vtable **VA** `0x1C73EBC`, raw `0x1872ABC`, slot `+0x1C`
at raw `0x1872AD8` contains **VA** `0xFFBF10`, code raw `0xBFB310`.
This binds the known initialization routine to that concrete table.

Login mode init raw `0x636FF0` calls dispatch routine `0x30AF50`
at `0x63703E`. Its call `0x30B1F6` invokes virtual `+0x1C`
with ECX=EBX, obtained from global **VA** `0x1DFE970`'s virtual
`+0x3C` getter. The receiver's vptr was not established; matching slot
offset alone does not bind this startup call to the manager's init.
The next trace follows that getter's concrete target/publication and return.

## Request normalization boundaries verified by unsigned branches and offline arithmetic

Raw `0x12C8A20/0x12CA410` use unsigned n<11 for chunk size 16;
n>=11 uses `(n+11)&0xFFFFFFF8`. Representative exact values:
n=0..12 gives C=16; n=13..20 gives C=24.
Ordinary usable C-4 therefore covers each request, with zero slack at
n=12/20. For n>=11, slack is `7-((n+11) mod 8)`, from 0 through 7.
This corrects the earlier ceil/slack claim without changing the boundary-tag
ownership evidence.

Unsigned cutoff n>=`0xFFFFFFC0` is checked before addition.
Last formula-normalized input `0xFFFFFFBF` gives `0xFFFFFFC8`;
larger inputs select internal `0xFFFFFFFF` sentinel, whose downstream
return behavior is the next check, not an allocation-success guarantee.
For raw `0x12CAE30`, null old pointer delegates even n=0/oversized;
nonnull n=0 frees/returns null, and nonnull oversized returns null before
normalization. Offline integer checks verified these values without running
or allocating through the target.

## Login getter receiver and separately loaded UI manager must remain distinct

Raw `0x30B19B` invokes global **VA** `0x1DFE970`'s slot `+0x3C`;
`0x30B1AF` saves its return in EBX. Call `0x30B1F6`, bytes
`FF 50 1C`, uses ECX=EBX and that returned object's table.
The getter's concrete target/publication remains unresolved and is parked.
Initial zero bytes in its backed pointer slot do not establish a runtime vptr.

The UI-manager pointer is separately loaded at `0x30B1A9` and dispatched
at `0x30B200`. The follow-up below binds that distinct initialization dispatch.
The already verified manager table's `+0x1C -> 0xBFB310` entry does
not bind the preceding getter-returned object merely by slot coincidence.

## Allocator edge-case return contract complete within valid chunk-state assumptions

Oversized requests select internal size `0xFFFFFFFF`, not an immediate-null
branch. Raw `0x12C8A20` traverses its large-bin/top path, whereas
`0x12CA410` skips bins for that sentinel and checks top before growth.
Growth `0x12C7690` and direct helper `0x12C6E70` reject their wrapped
size before VirtualAlloc. For valid aligned chunk metadata the request fails;
the raw top checks do not independently rule out a stored impossible
`0xFFFFFFFF` top size. This is a static precondition, not a runtime finding.

Zero-byte raw allocation normalizes to chunk size 16 and may succeed or fail.
Null-pointer realloc delegates even zero/oversized requests; nonnull zero
frees, while nonnull oversized returns null before freeing.
Ordinary positive realloc failure preserves the old pointer in `0x12CAE30`.
VM callback `0xC3C0B0` instead reports failure then frees its original pointer
if reporting returns. Public allocation wrapper `0x12B8030` similarly reports
null before returning it. Both use the previously bound project reporter;
the follow-up's unresolved-indirect label does not reopen that mechanism.
No explicit errno assignment was found in the inspected oversized branches.
The allocator edge investigation is closed at these boundaries; the next
independent task follows VM finalization, not another allocation micro-case.

## Login UI initialization dispatch and inherited stack arguments

In the active sample `D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe`,
raw `0x30B1A9` loads UI-manager global VA `0x1E002B8`, saves the pointer
at `0x30B1B1`, and loads its vptr at `0x30B1BA`. Call `0x30B200`
(VA `0x70BE00`) uses that receiver and slot `+0x1C`. With the previously
verified constructor vptr VA `0x1C73EBC`, table raw `0x1872AD8` contains
`10 BF FF 00`, mapping VA `0xFFBF10` to raw `0xBFB310`.
This composes constructor/table evidence with the concrete call; it does not
assert that the global cannot hold a different runtime vptr.

Only the return from the separate call `0x30B1F6` is newly pushed before
`0x30B200`; seven prior arguments remain on the stack. Callee order is:
preceding return, zero, `[VA 0x1DFFFC0]`, `[VA 0x1DE0868 + EDX*4]`,
EDI, caller `arg_0`, caller `arg_8`, second getter return plus `0x48`.
`0xBFB310` ends with `retn 0x20`, consistent with eight stack arguments.
These are positional values, not recovered semantic names or ownership.

Login raw `0x636FF0` calls `0x30AF50` at `0x63703E` and tests AL
at `0x637043`. One input flag comes from
`byte[[VA 0x1E5C834] + 0x40] == 0`. Initialization failure passes through
`0x30AF50`'s common local object/text cleanup at `0x30BDF0` and returns
zero; Login then logs and returns zero. Success branches to `0x63706C`.
The next bounded investigation follows the eight inputs' retained fields
and ownership, without reopening the unidentified getter receiver.

## VM close finalizer phases precede internal and outer storage release

For the same active sample, raw `0xCD5510` calls `0xCD5AB0`, then
`0xCD9870`, releases internal buffers, and finally invokes the allocator
for `(VM - 4, 0x390, 0)`. This is the VM close reached from scripting
object `0xC33060`; freeing the VM is not proof of freeing that native object.

`0xCD9870` transfers context `+0x48`'s list to the tail of `+0x60`.
Helper `0xCD9580` drains `+0x60`, moving entries to `+0x40` and,
for qualifying tag-6 values, making the protected callback through
`0xCD7890`/`0xCD6C10`. Callback literal VA `0x10D9E50` maps to raw
`0xCD9250`, which calls `0xCD7410` and dispatches through `0xCD7110`.
The drain does not check the protected-call status and continues; no retry
was found on this close path. `0xCD9870` then cleans lists at context
`+0x48`, `+0x40`, and `+0x64`.

Afterward `0xCD5510` frees context `+0x18`, context `+0x6C` and zeros
its size, then, if present, VM `+0x1C` after draining its `+0x0C` chain
via `0xCD51E0`. The outer `0x390` block is last.
Instruction-level clarification: `0xCD98A7` calls the drain; `0xCD98BB`,
`0xCD98CA`, and `0xCD98D9` call `0xCD8F70` for the three lists above.
The drain removes its head at `0xCD95AA`, calls `0xCD7890` at
`0xCD96A7`, restores saved state at `0xCD96AC..0xCD96BA` without
testing the returned status, and tests/repeats at `0xCD96C1..0xCD96C5`.
`0xCD7890` tests the protected result at `0xCD78C7..0xCD78C9`;
its error path returns status at `0xCD7907`, without retry.

Internal buffer free calls occur at `0xCD5538`, `0xCD5547`, and
conditional `0xCD557A`. Only afterward, `0xCD558D` loads allocator
userdata from context `+4`, `0xCD5590` loads the callback from context
`+0`, and `0xCD5592` invokes the final outer-block free.
The ordinary `0xD675A0` bridge similarly loads userdata at `0xD675C5`,
callback at `0xD675CC`, and invokes it at `0xD675CE`.

## UI initialization retains a mixture of copied text and unclassified pointers

The eight positional arguments to raw `0xBFB310` have these observed
uses in the active sample; pointer stores alone do not prove ownership:

| Argument | Handling |
|---|---|
| 1: preceding virtual-call result | Passed to `0xCA6370`; stored at child `+4` at `0xCA639F`. Child pointer stored at UI `+0x24` at `0xBFB416`; no refcount acquisition identified. |
| 2: zero | No read found in this bounded trace. |
| 3: global VA `0x1DFFFC0` value | Stored at UI `+0x974` at `0xBFB51A`; virtual call through it at `0xBFB8C6`. Ownership unresolved. |
| 4: indexed global value | Copied into shared-text UI `+0x34` at `0xBFB48C..0xBFB4CB` through byte-copy helper `0x2FCC90`, not retained as the source pointer. |
| 5: EDI | Copied to shared-text UI `+0x5C8` at `0xBFB403..0xBFB419`, then path-normalized. |
| 6: outer caller `arg_0` | Passed to formatter `0xBFDE80` at `0xBFB748..0xBFB74D`; no direct field store identified here. |
| 7: outer caller `arg_8` | Conditional mode flag at `0xBFB84B..0xBFB864`, not stored. |
| 8: second getter result plus `0x48` | Stored at UI `+0xA08` only near success, `0xBFB8C9..0xBFB8CE`; ownership unresolved. |

Late initialization failures converge on `0xBFB668` and return zero via
`retn 0x20` without clearing these retained fields in that epilogue.
The caller's subsequent `0x30D150` call site is `0x30B204..0x30B209`;
its exact predicate and concrete retained-field cleanup are the next task,
not evidence that those resources have already been released.

## Published scripting objects are freed after VM close during tree drain

The active sample's manager shutdown slot `+0x34`, raw `0xBFA0D0`,
does not merely close each published object's VM. With the payload from
tree node `+0x14`, it calls `0xC33060` at `0xBFA278`, then allocator
free `0x12C8D60` at `0xBFA27F` with the same object pointer.
Identity removal `0xBFCF30` follows at `0xBFA28B`, and key release
`0xC09D10` at `0xBFA29C`. This closes the native-storage ownership
gap for this concrete tree-drain path.

`0x12C8D60` is allocator free logic, not an object-vtable destructor;
no separate top-level C++ destructor dispatch was identified here.
`0xBFCFF0` has a similar close/free/identity-remove sequence, but its
caller remains unresolved and is not required to establish bulk shutdown.
The next task applies these verified lifecycle boundaries to the existing
Startup reconstruction and focused synthetic checks, without inventing
native ownership rules for the still-unclassified initialization pointers.

## Failed UI initialization invokes shutdown, not the manager destructor

At raw `0x30B200..0x30B209`, a zero UI-initialization return falls through
to `0x30D150`; nonzero skips it via `jnz 0x30B232`.
`0x30D150` tail-dispatches UI slot `+0x34`, bound to `0xBFA0D0`.
That routine is gated by byte `+0x671` and clears the word at `+0x671`
(both initialization flags) and field `+0x950` before cleanup.
It does not invoke UI slot zero, so this failure handling is not proof
that all startup-retained fields are destroyed.

The manager's slot `+4` is getter `0xBFA0C0` for byte `+0x671`.
Consequently `0xBFB310`'s initial nonzero check is an already-initialized
guard, not an unidentified initialization operation.
Destructor-style slot zero maps to `0xBF82A0`, which calls `0xBF8AC0`.
That teardown releases copied shared-text `+0x5C8` directly and
`+0x34` through `0x30A9D0`'s release-and-zero helper.

Initialization allocates a `0x188`-byte child, initializes it with
`0xCA6370`, and stores it at UI `+0x24`. The external argument at child
`+4` has no observed retain/copy. The inspected parent teardown does not
directly release `+0x24`; tracing this child's concrete ownership is the
next bounded task. Ownership of directly stored `+0x974` and success-only
`+0xA08` remains unresolved; absence of a direct clear is not a leak finding.

## UI lifecycle reconstruction added; shutdown tests a byte, not the cleared word

`src\Startup\UiManagerNativeLayout.cs` now records the verified initialization
guard, partial/success flag snapshots, and ordered payload close/free/identity/
key-release call sites. Startup checks cover these synthetic contracts;
`docs\Startup.md` records their static-only scope.

Coordinator instruction verification corrected the initial projection:
raw `0xBFA102` compares **byte** `[this+0x671]` with zero and
`0xBFA109` skips shutdown when zero. Only after passing that guard does
`0xBFA112` clear **word** `[this+0x671]`; `0xBFA11B` also clears a
word at `+0x950`. Therefore ready byte `+0x672` alone cannot enable
shutdown. The code separates the byte predicate from the two-byte state
snapshot; exhaustive combinations include the `(partial=0, ready=1)` case.
This supersedes any wording that treats the cleared word as the guard.
Focused Startup checks and `git diff --check` passed.

## UI child has no established vptr or concrete storage-release chain

The bounded `+0x24` investigation stops at an ownership gap, not a leak
claim. Raw `0xBFB3DA` requests `0x188` bytes, conditionally invokes
`0xCA6370`, and stores the result at UI `+0x24` at `0xBFB416`.
`0xCA639F` stores the external dependency at child `+4`;
`0xCA63A2` zeros child `+0`, rather than assigning a child vptr.
The indirect call at `0xCA651E` uses the dependency's vtable.
Direct initializer `0xCA66F0` is invoked at `0xBFB542`.

Parent deleting wrapper `0xBF82A0` calls `0xBF8AC0`, then conditionally
frees parent storage at `0xBF82B3`. This is not a child free.
Neither that inspected destructor nor the initialization failure epilogue
establishes release of the stored child. Child teardown remains parked;
do not infer a destructor slot from its zero first word or conflate the
borrowed dependency's dispatch with a child vtable.

## UI success pointer aliases a mutable sixteen-byte record sequence

Raw `0xC95400` (VA `0x1096000`) checks its object's `+0x72C` flag,
loads UI global VA `0x1E002B8`, and reads UI `+0xA08`.
When nonnull, the descriptor becomes ECX for `0xC97380`, with a
16-byte record assembled from float fields `+0x338..+0x344`.
`0xC97380` uses descriptor begin/end/capacity pointers at `+0/+4/+8`,
copies four DWORDs at end, and advances end by `0x10`; full storage
reaches grow helper `0xC978F0`. This is a mutable alias, not a copied
container, and does not establish semantic record names or wire encoding.

Initializer `0xBFB8CE` stores its eighth argument verbatim at UI `+0xA08`
after the earlier failure checks. Caller `0x30B1B4`'s indirect getter
result plus `0x48` supplies that argument; the containing object's concrete
identity/lifetime is unresolved. A reconstruction task now records the
verified layout without implementing speculative ownership or allocation.

## Late service callback has no checked return; final UI dispatch remains separate

Raw `0xBFB518..0xBFB51A` stores explicit argument three at UI `+0x974`.
At `0xBFB8BC..0xBFB8C6`, the initializer reloads it as ECX, pushes
one, and invokes vtable `+0x30`. Its return is not tested: the next
operation loads argument eight and stores it at UI `+0xA08`. A later
UI virtual `+0xB0` call precedes ready-byte `+0x672` publication.
Thus this service callback is non-gating in the observed return-value
control flow, not proof of a notification's semantic purpose.

Helper `0xBFEEC0` returning false is not itself an initialization failure:
the branch at `0xBFB8A0` logs on true, while false falls through.
The callback receiver has no null guard at the dispatch; normal completion
still depends on a valid receiver. Earlier checks at `0xBFB532`,
`0xBFB542`, or `0xBFB642` can return through `0xBFB668`.
The report's empty direct xrefs to `0xBFB310` do not negate the already
verified virtual caller, and its numeric-VA byte observation does not
supersede the caller's recorded global argument load. Concrete service
implementation binding remains unresolved; it is not inferred from that load.

## Final initialization dispatch is gated before the ready byte is published

Raw `0xBFB8D6` calls the UI receiver's slot `+0xB0`, after `+0xA08`
publication and before ready-byte `+0x672` write at `0xBFB8DC`.
For constructor vptr VA `0x1C73EBC`, table raw `0x1872B6C` contains
`90 06 00 01`, mapping VA `0x1000690` to raw `0xBFFA90`.
The helper requires its slot-4 initialization check and ready byte.
On the constructor-initialized path, ready is still zero at this call,
so it returns without issuing the later component synchronization.
The initializer ignores its return and then sets ready.

When its gates pass, `0xBFFA90` can call component `+0x974`'s slot
`+0x1C` with `(0,0)` or `(1,value)`; that return is also ignored.
Concrete selection inputs and a post-ready caller are the next task.
An indirect callee throwing before the ready store is not ruled out merely
because no explicit throw was seen here. Runtime vptr substitution remains
outside the constructor-table binding.

The record sink's static constants and focused checks are now implemented
in `UiManagerNativeLayout` and documented in `Startup.md`.
Coordinator review corrected the eighth argument from a mistaken `+8`
stack-offset label to ordinal 8 / frame offset `+0x24`, and corrected the
getter source to **address addition** `getter()+0x48`, not a field load.
Startup checks passed with those corrections; no wire format, grow policy,
record meaning, or descriptor lifetime was inferred.

## Aliased record sink drain remains unbound; producer iteration is concrete

The bounded read/drain search did not identify a consumer or reset of the
aliased descriptor. Raw `0xC5EF40` calls producer `0xC95400` while
iterating objects; producer flag decimal 1836 is the already recorded
`+0x72C`, not a new field. Descriptor append and its sixteen-byte stride
remain verified, but a per-frame clear or drain is not established.

A separate offset match in raw `0x8DDA10` feeds a helper that treats its
first DWORD as a handle, not this vector descriptor; offset equality
does not identify the same object. The focused scan's lack of another
matching global/offset access is a bounded search result, not proof of
absence of indirect readers. Drain ownership is parked. The next task
traces `0xC5EF40`'s concrete iteration bounds and producer-call gates.

## UI synchronization selection and callback suppression predicates

Raw `0xBFFA90` requires nonzero slot-4 getter result and ready byte
`+0x672`; otherwise it returns zero. On reached paths it returns one,
including when it issues no component call. A 20-byte initialized helper
record supplies two output DWORDs, here named `h0` and `h1`; their
provider/meaning is not bound by this report.

Selection starts with `+0x58C`. Unless byte `+0x590` and that value are
both nonzero, `+0x588` replaces it. If the result is zero, `+0x584`
is the fallback. Component `+0x974`'s slot `+0x1C` receives:

| Predicate | Arguments |
|---|---|
| byte `+0x670 == 0`, or byte `+0x598 != 0` and (`h0 == 0` or `h1 != selected`) | `(1, selected)` |
| byte `+0x670 != 0`, byte `+0x598 == 0`, and `h0 == 1` | `(0, 0)` |
| Otherwise | No component call |

The component return is ignored. Raw `0xBFF930`, call site `0xBFFA3F`,
updates `+0x58C/+0x590/+0x594` and invokes UI slot `+0xB0`.
It does not itself test ready; this is a concrete caller, not proof that
every invocation occurs after ready publication. The callee suppresses
dispatch before readiness. A bounded code task now projects these exact
positional decisions without assigning speculative field semantics.

## Record producer iteration uses eight-byte entries and independent call gates

Raw `0xC5EF40` tests byte `+0x2D4` and compares its float `+0x2C0`
against VA `0x1C818E8` at `0xC5EF7D`. For ordered finite values,
the observed path proceeds at or above the threshold. Unordered behavior
and operand order require instruction-level clarification before a projection.

The loop starts at parent `+0x5C`, ends on equality with `+0x60`,
and advances eight bytes at `0xC5F026`. Each record's first DWORD
is an object pointer; its second DWORD is not read here.
A qualifying object has its `+0x1C` pointee's byte `+0x7F` set,
object byte `+0x40` set, and DWORD `+0x70 != 2`
(`0xC5EFF2..0xC5F005`). Call `0xC5F01D` passes the object as ECX
and parent `+0x2B0` as argument to `0xC95400`.
Coordinate change gates an earlier virtual `+0x30` call, not this producer.

Matching initializer `0xC5CFE0` sets `+0x2D4=1` at `0xC5D116`,
zeros `+0x5C/+0x60`, and initializes `+0x2C0` to float one.
Concrete calls include `0xC88495`, `0xCC18A7`, guarded `0xCEE4A9`,
and `0xCF17F7`, plus tail-call `0xCF28E0`. Each invocation or duplicate
record can repeat production; these entries do not establish frame cadence.
The next task traces the supplied `+0x2B0` argument's effect on the four
source floats rather than assuming an untransformed record copy.

## UI synchronization projection integrated; record-order clarification pending

The pure `UiManagerNativeLayout.ProjectStateSync` now models the verified
gates, selection precedence, and optional component-call arguments, using
neutral offset-based inputs rather than inferred field meanings. Focused
Startup checks cover all byte values for gates/flags, exact helper-output
zero/one comparisons, fallback precedence, and suppressed calls.
Startup checks and `git diff --check` passed during integration.

The producer follow-up confirms raw `0xC95516..0xC9554E` uses bit-preserving
MOVSS transfers rather than arithmetic with the supplied parent argument.
Earlier opaque callees may still modify source state. It also corrects the
global guard: `0xC95507..0xC95514` dereferences UI global VA `0x1E002B8`
without checking that global pointer, then checks the `+0xA08` descriptor.
The prior `Startup.md` claim of a null-checked UI global is superseded.

The report's claimed local-memory order conflicts with its own listed
source/destination pairs, so exact record order is awaiting a bounded
instruction clarification; no reordered code projection is accepted yet.
Its C5/C9 addresses are raw IDB offsets, despite its mistaken VA labels.
At `0xC5EF75..0xC5EF84`, COMISS compares global float against parent
`+0x2C0` and JA exits for ordered global greater than field. Unordered
flags do not take JA; an unmasked floating-point exception remains outside
that branch-only observation.

## Exact local record order corrected from source register stores

The bounded clarification resolves the conflicting report. Raw `0xC95516`,
`0xC95521`, `0xC95529`, and `0xC95531` load source `+0x338` into XMM2,
`+0x344` into XMM3, `+0x33C` into XMM0, and `+0x340` into XMM1.
Stores at `0xC9553A/0xC9553F/0xC95544/0xC95549` place XMM0/XMM2/
XMM1/XMM3 at `EBP-0x34/-0x30/-0x2C/-0x28`.
LEA `0xC9551E` and push `0xC95539` pass `EBP-0x34` to the append.
Therefore actual ascending record order is **33C, 338, 340, 344**.
The earlier report's destination associations, not that ordering, were wrong.

`UiManagerNativeLayout`'s positional source constants and their regression
check now use this exact order, superseding the earlier ascending-source
assumption. The code still does not assign semantic names or wire meaning.
The successor task follows only pre-append calls and their possible source
state updates, not another scan of the now-resolved copy instructions.

## Synchronization helper record is USER32 CURSORINFO

Read-only PE import parsing binds IAT VA `0x1AC4C24` (raw `0x16C3824`)
to `USER32.dll!GetCursorInfo`. Raw `0xBFFA90` initializes a twenty-byte
record with `cbSize=20`, zeroes its remaining sixteen bytes, and passes
it to that import. Thus previously neutral `h0` is CURSORINFO flags
at `+4`, and `h1` is the PE32 cursor handle at `+8`; point coordinates
occupy `+12/+16`. This resolves the record provider without guessing
from its size. The existing exact-zero/exact-one predicates remain unchanged.

The API return is ignored; failure does not select a separate branch.
Pre-zeroing does not prove output remains zero if the API returns failure.
A code task now updates captured-input naming and layout evidence without
calling WinAPI or introducing a success-shaped failure fallback.

## Pre-append helper writes derived fields, not the four record source fields

At raw `0xC95428..0xC95443`, producer `0xC95400` passes output
`this+0x2DC`, input `this+0x98`, and its argument to `0xCCA190`.
That helper writes derived fields `+0x2DC..+0x2FF` and a boolean
at `+0x300` (`0xCCA24A..0xCCA32C`). The boolean and a comparison
against `+0x2EC` gate subsequent work at `0xC95450`.
Indirect slots `+0x34/+0x20/+0x28/+0x74` remain unresolved.
The `+0x74` call requires the `+0x1C` pointee's byte `+0x80`;
its return feeds helpers including `0xC225E0` and `0xCA90B0`.

The parent `+0x2B0` is the caller's supplied argument, not necessarily
`this+0x2B0` in the producer. No local write to source `+0x338..+0x344`
was identified; opaque virtual side effects remain possible.
The report's VA `0x1696000` is arithmetic error: raw `0xC95400`
maps to VA `0x1096000`. The successor follows only `0xCCA190`'s
concrete output arithmetic, not the unresolved virtual targets.

## Captured cursor observation is represented in reconstruction code

The UI synchronization input now names the PE-verified outputs
`CursorFlags` and `CursorHandle`, keeping the latter a 32-bit value.
Layout constants preserve GetCursorInfo's IAT and CURSORINFO offsets.
Focused checks retain exact zero/one comparisons, cover flags outside a
byte and high-bit handle differences, and leave the other fields neutral.
No WinAPI call or inferred zero-output-on-failure behavior is implemented.
The delegated Startup checks reported 849 passes and no failures.
The next bounded trace follows the cursor-selection setter's actual
arguments and sources, not another pass over the resolved API binding.

## Derived producer state is nine float slots followed by an independent byte

Raw `0xC95430..0xC95443` passes output `this+0x2DC`, input
`this+0x98`, and the parent's `+0x2B0` argument to `0xCCA190`.
Its output is nine floats at relative `+0..+0x20` and a byte at
`+0x24` (object `+0x300`), not a proven matrix or rectangle type.
The byte is one iff input byte `+1` and parent byte `+0x24` are nonzero.
This stored byte must remain distinct from the helper's scalar return.

Direct helper `0xCCA350` derives a pair using input offsets `+8`,
`+0x1FC/+0x200`, and `+0x20C..+0x218`, including FSINCOS and
single-precision stores. `0xCCA190` may rotate/translate that pair
using parent angle `+0x14` and coordinates `+0/+4`.
Other output slots are anchored as follows:

| Output | Observed operands |
|---|---|
| `+8` | parent `+8` plus input `+0x204` |
| `+0xC` | parent `+0xC` plus input `+0x208` |
| `+0x10` | input `+4` times parent `+0x10` |
| `+0x14` | Angle-path result, import/branch clarification pending |
| `+0x18` | input `+0xC` times parent `+0x18` |
| `+0x1C` | input `+0x10` times parent `+0x1C` |
| `+0x20` | input `+0x18` plus parent `+0x20` |

Ordered-equality tests at `0xCCA1AE..0xCCA1BA` and
`0xCCA35B..0xCCA369` select the direct pair branches; NaN does not.
Angle normalization crosses IAT VA `0x1AC4820`. Its exact import,
constant mapping, and unusual sign/unordered branch report are a separate
bounded follow-up before accepting a complete arithmetic projection.
Earlier virtual calls remain unresolved, so this derived-state trace does
not prove absence of indirect writes to the later record source fields.

## Selection setter copies a shared-key tree payload and a separate byte flag

Raw `0xBFF930` receives a pointer whose first DWORD supplies a shared-text
lookup key. Lookup resolves a tree node; when it differs from UI `+0x6C8`
sentinel, the node's `+0x14` DWORD is read at `0xBFF9ED` and copied
to UI `+0x58C` at `0xBFFA14`. Argument two's byte is copied to
`+0x590` at `0xBFFA2B`; `+0x594` is copied from UI `+0x990`
at `0xBFFA37`. Constructor `0xBF7A80` initializes `+0x584/+0x588`
to zero at `0xBF7C54/0xBF7C5E`, not to proven nonzero cursor handles.

No WinAPI call in this setter proves the tree payload's origin. Its use
in the cursor comparison is evidence of consumption, not independent
proof that every payload is a cursor handle. The successor follows the
tree's concrete publisher. The report's unresolved label for final
`0xBFFA3F` does not reopen the already composed UI vtable binding to
`0xBFFA90`; runtime replacement remains the existing qualification.

## Derived angle uses CRT fmod; correction applies to negative nonzero remainders

Read-only PE mapping resolves stub raw `0x159DD00`, bytes
`FF 25 20 48 AC 01`, through IAT VA `0x1AC4820` / raw `0x16C3420`
to `MSVCR120.dll!_CIfmod`. The on-disk IAT points at import-name data,
not a resolved runtime target; no bound-import directory is present.
Empty raw-IDB import enumeration is not a dynamic-resolver finding.

Mapped float constants: VA `0x1C5E1C4` / raw `0x185CDC4` contains
`DB 0F C9 40` (bits `0x40C90FDB`, float two-pi); VA `0x1C817A4` /
raw `0x18803A4` is zero, and VA `0x1C81BE8` / raw `0x18807E8` is one.
VA `0x1EDB020` is virtual-tail loader-zero data, not a file-backed pair;
later runtime writes are not excluded.

Raw `0xCCA29F..0xCCA2A8` supplies the sum and two-pi to the CRT stub.
At `0xCCA2BA..0xCCA2CC`, SETNBE predicates select correction for an
ordered negative original sum. In that branch, UCOMISS remainder/zero,
LAHF, TEST AH/0x44, and JNP skip addition only for exact zero.
Otherwise two-pi is added, including for unordered remainder (still NaN).
This supersedes the earlier incorrect "add only if unordered" interpretation.
Exact zero includes either sign, so a captured signed-zero remainder
passes unchanged. Output float `+0x14` is distinct from output byte
`+0x24`; the epilogue does not establish a meaningful EAX boolean return.
The next code task projects only correction of a captured CRT remainder,
not an unverified managed emulation of x87 `_CIfmod`.

## Captured-remainder correction implemented without emulating the native CRT

`UiManagerAngleCorrectionProjection.cs` projects the observed correction
from an original sum and captured remainder, preserving either signed zero
unchanged and treating the derived output byte independently.
Focused checks cover negative sums, exact multiples, NaN, zero signs,
positive sums, and all byte values for each flag input.
Coordinator review corrected hexadecimal `+0x14/+0x24` offsets that had
been transcribed as decimal 14/24, distinguished helper `0xCCA190` from
import stub `0x159DD00`, and represented float bits as `0x40C90FDB`
rather than a misleading integer named as byte order.
The next task traces which producer pre-work these derived gates suppress,
without assuming the derived helper has a meaningful scalar return.

## A concrete cursor registration publishes LoadCursorA's result

Raw `0xBFD6D0` calls IAT VA `0x1AC4C2C`, PE-bound to
`USER32.dll!LoadCursorA`, with `(NULL, 0x7F8A)`. Its result is passed
with a shared-text key and UI tree `+0x6C8` to `0xC06D40`.
That helper inserts only for an absent key. Allocator `0xC166E0`
stores key at node `+0x10` and value at `+0x14` (`0xC1675D`);
`0xC16790` links via `0xC1A450`.
This proves one registration's payload source, not that every selector
lookup must return such a handle. The successor follows tree cleanup.

## Derived state rejects producer work before the append flag is reached

Raw `0xC9544D`'s virtual `+0x34` call occurs before testing derived
byte `+0x300`. Zero at `0xC95456..0xC95458` returns zero.
COMISS/JA at `0xC9545E..0xC9546D` likewise rejects ordered global
threshold greater than derived float `+0x2EC`. Equality passes;
unordered branch flags do not take JA if the comparison completes.
Rejecting either gate skips virtual `+0x20/+0x28`, conditional `+0x74`
selectors two/one, and subsequent append work.

Thus append also depends on these earlier gates, not just `+0x72C`
and descriptor nonnull. UI global itself remains unchecked before its
`+0xA08` dereference. The report's C9-address VA labels are erroneous;
these are raw IDB offsets. A code task now projects captured branch
outcomes without emulating FP exceptions or unresolved virtual side effects.

## Cursor-tree teardown releases key and node, not the registered handle

The concrete parent destructor path `0xBF82A0 -> 0xBF8AC0` reaches
tree erase helper `0xC0C370` at raw `0xBF8F55`.
Node cleanup `0xC0F590` releases the shared key at `+0x10`, then
frees the node. It does not read or release payload `+0x14` in this
traced path, and does not pass that handle to DestroyCursor.
This is the tree cleanup's policy, not proof about every lifecycle path.
The report's "IDA VA" labels for these offsets are superseded by the
raw-IDB convention.

The producer-gate reconstruction now distinguishes earlier rejection from
later append admission and tests zero/nonzero bytes, equality, NaN and
infinities. It does not simulate FP exceptions, virtual calls, or append.
Cursor registration constants distinguish the registration method
`0xBFD6D0` from an individual call site, and node payload store
`0xC1675D` from its enclosing constructor. The delegated focused Startup
run reported 858 passes and no failures.

## Selection lookup miss shares completion; cache equality bypasses lookup

Raw `0xBFF95E` compares UI `+0x990` with `+0x594`; equality bypasses
lookup, flag updates, temporary cleanup, and synchronization.
Otherwise the `+0x6C8` tree is searched once through
`0xC21760/0xC21850`, with no alternate-name lookup found.
Sentinel or zero payload reaches conditional logging at `0xBFFA0C`
when the log threshold permits. Shared-text release at `0xBFFA21`
precedes `+0x590` and `+0x594` updates and `0xBFFA3F`'s sync call.
Miss is not a separate explicit-zero return. Exact zero-store/cache-return
details are awaiting a bounded instruction clarification.

Clarification: on every cache-miss completion, raw `0xBFFA14`
unconditionally writes EBX to `+0x58C`. EBX is zeroed at `0xBFF9D4`;
sentinel skips the payload load at `0xBFF9ED`, so a missing key clears
the selection rather than preserving an old value. A zero payload can log,
then reaches the same store. At `0xBFF958`, EAX loads `+0x990`;
equality at `0xBFF95E` returns that loaded value through `0xBFFA45`.
The clarification's newly subtracted `0x7FF...` offsets are invalid:
its `0xBFF...` IDB addresses were already raw, not runtime VAs.
Only the instruction evidence and original raw addresses are retained.

## Record vector growth separates count arithmetic from failure guard behavior

Raw `0xC97380` preserves an in-range source's record offset across
`0xC978F0` growth and rebases before its four-DWORD copy.
Growth uses `(end-begin)>>4` and `(capacity-begin)>>4`; required count
is used plus one, guarded at maximum `0x0FFFFFFF`.
If `max - floor(capacity/2) >= capacity`, it selects the larger of
required and capacity plus half; otherwise it selects required.

`0xC97FA0` requests count times sixteen through
`0x12B7FB0 -> 0x12C8A20`, copies, frees old storage, then publishes
capacity/end/begin. Null allocation reaches indirect IAT VA `0x1AC4424`;
max-count paths also cross indirect guards. Their precise binding and
nonreturn behavior must be resolved before claiming old-buffer failure
preservation or continuation. A separate code task projects only bounded
count selection, not allocation, pointer publication, or guard behavior.
These are raw IDB addresses despite the report's VA labels.

## Bounded vector count projection implemented independently of allocation

`NativeRecordVectorCapacityProjection.cs` records the sixteen-byte stride,
maximum count, guard-before-increment decision, and overflow-aware
capacity-plus-half selection. Inputs above the established count domain
raise argument errors; the projection does not claim native metadata
validation or impose a `used <= capacity` invariant absent from its scope.
Guard results have no projected count or bytes, rather than a fabricated
successful capacity. Maximum projected byte size is `0xFFFFFFF0`.

Focused checks cover small odd/even capacities, required-count precedence,
the exact half-growth boundary, maximum-used guard, and out-of-domain
arguments. The delegated Startup run reported 865 passes and no failures.
Allocation outcome, source rebasing, pointer publication, and guard
nonreturn behavior remain separate static contracts.

## Vector guards bind to standard-library exception helpers

Read-only PE parsing binds IAT VA `0x1AC4424` to
`MSVCP120.dll!std::_Xbad_alloc(void)`. Raw `0xC97FFB` calls it
without explicit arguments on allocation failure, bytes `FF 15 24 44 AC 01`;
fallthrough is `0xC98001`. Under the normal runtime contract it throws.
The EXE alone does not establish that an externally replaced implementation
cannot return; this qualification is not evidence that normal failure
continues into copy/free/publication.

At maximum used count, raw `0xC9791C` calls IAT VA `0x1AC45C0`,
bound to `MSVCP120.dll!std::_Xlength_error(char const*)`, after pushing
VA `0x1C5EA14`, mapped string `"vector<T> too long"`.
Normal behavior throws length_error; hypothetical fallthrough at
`0xC97922` can reach the allocation helper's over-limit guard.
The report does not supply that guard's exact call site, so it remains
distinct from these two verified call sites.

Only under hypothetical returning exception helpers does copy helper
`0xC9BBD0`'s null-destination skip/advance become relevant; it does not
establish a normal failure-preservation guarantee. No runtime finding is
claimed. A code task now records the verified imports and call sites
without simulating hypothetical memory accesses or external exceptions.

## Cache marker has a concrete same-class increment, not a proven frame cadence

Constructor raw `0xBF80AF` initializes UI `+0x990` to zero.
The installed table VA `0x1C73EBC` binds slot `+0x40` to raw
`0xC02730`; it takes ECX as EDI at `0xC0275D`, increments
`[EDI+0x990]` at `0xC02816`, and stores arguments at `+0x988/+0x98C`.
The same table's `+0x104` entry binds selector `0xBFF930`, which
compares that instance's `+0x990` against captured `+0x594`.
This supports an instance-local version/invalidation interpretation, not
a frame/time counter or proof of a particular external virtual caller.
The successor verifies the increment's surrounding gates and arguments.

The vector reconstruction now includes exact exception-import names,
decorated names, call sites, and the length-error message.
`StartupBinaryEvidence` verifies their PE module/name associations and
instruction bytes, independently of the synthetic count checks.
Coordinator integration ran Startup checks with `--binary` against the
authorized immutable sample and passed. Method `0xC97FA0` is labeled as
the reallocation method, not an exact guard instruction address.

## Cursor-cache projection represents state availability, not native return payload

`UiCursorCacheProjection.cs` projects captured version equality and completed
lookup refreshes; a missing payload maps to the observed stored zero.
Its cache-hit payload describes available cached state, **not** native EAX,
which returns the loaded version on the bypass path.
Coordinator review corrected the delegated documentation's payload-return
claim, increment site `0xC02816` versus ECX-to-EDI instruction `0xC0275D`,
and version-load `0xBFF958` versus compare `0xBFF95E`.
Unproven associations between setter arguments and fields `+0x988/+0x98C`
were removed from this projection pending the separate method trace.
The model does not execute lookup, synchronization, or version increment.

## Invalidation requires ready and stores two DWORD arguments after old-state work

UI table raw `0x1872AFC` (`+0x40` bytes) contains `30 33 00 01`,
binding raw `0xC02730`. A separate global counter increments at
`0xC02765` before ready-byte `+0x672` check at `0xC0276B..0xC02772`.
Zero ready skips to `0xC041EC` / `retn 8`, bypassing the object
version increment and argument stores. On the normal enabled path,
`0xC06670` runs before writes, then the global-init branch reconverges.
`0xC02816` increments DWORD `+0x990` modulo 2^32 once on that path.
`0xC0281C/0xC02825` store stack DWORDs `[EBP+8]/[EBP+0xC]`
at `+0x988/+0x98C`. These are not established cursor-key/byte fields.

The earlier helper reads that old pair as a QWORD and performs a
divide-by-1000/difference threshold calculation before callbacks.
Its exact arithmetic helper and signedness are a bounded follow-up;
the report's `C159DD10` target spelling is not accepted without verification.
Post-store component `+0x974` slots `+0x40/+0x44` receive temporary-buffer
pointers at `0xC02831/0xC0284E`, not the stored arguments directly.
No exact clock units or external caller cadence is inferred.

## Old-pair callback gate uses unsigned quotient truncation and modular subtraction

Raw `0xC06686` calls local helper `0x159DD10`, not an import and not
the earlier misspelled `C159DD10`. Bytes `E8 85 76 99 00` bind that call.
Arguments form the old `+0x988/+0x98C` unsigned QWORD and divisor 1000;
the helper uses unsigned DIV, and the caller retains only quotient EAX.
It computes unsigned modular `quotient_low32 - [this+0x20]`,
skips below 5000, otherwise stores the quotient at `+0x20` before callbacks.
Lists `+0x6B0/+0x6D8` dispatch entry slot `+0x14`; list `+0x5E4`
uses `0xC376A0`. Units and invocation cadence remain unproven.
The report's final VA example for helper `0x159DD10` is arithmetic error:
its code VA under the established mapping is `0x199E910`.

The ready-gated invalidation projection is integrated with all-byte and
uint-wrap checks, preserving prior state when ready is zero and projecting
the full two-DWORD stores only on the normal enabled write path.
It does not execute the preceding old-pair callbacks or guarantee completion.
Default `+0x584/+0x588` nonzero provenance remains unresolved after its
bounded search; only candidate `0xBF9D80` is being checked next, without
restarting a broad offset scan.

## Old-pair arithmetic projection integrated; mistaken candidate evidence rejected

`UiOldPairCallbackGateProjection.cs` combines captured words, divides
unsigned by 1000, truncates the quotient to 32 bits, and projects the
modular threshold and conditional field update. Focused checks cover
threshold boundaries, flooring, high words, truncation, and subtraction wrap.
Coordinator review separated tree offset `0x5E4` from method `0xC376A0`;
the delegated concatenation `0x5E4C376A0` was not an address.

The adjacent-method report is rejected: it transformed already-raw
addresses and inspected unrelated code, then claimed to invalidate the
established vtable. Coordinator reads on active IDA port 13337 confirm:
raw `0x1872B6C` is `90 06 00 01`; raw `0xBFFA90` begins
`55 8B EC 83 EC 14 56 8B F1 8B 06 8B 40 04 FF D0`;
raw `0xBF9D80` begins `55 8B EC 83 E4 F8 6A FF`.
These preserve the prior selector binding and disprove that report's
claimed raw function boundary. A narrowly corrected candidate task uses
those exact IDB offsets; default cursor sources remain unresolved meanwhile.

## Callback walks advance live tree nodes after receiver-only dispatch

After raw `0xC0669C` stores field `+0x20`, `0xC06670` visits trees
`+0x6B0`, `+0x6D8`, then `+0x5E4`.
The first two use node `+0x14` as ECX and invoke slot `+0x14`
without explicit stack arguments at `0xC066B0..0xC066B5` and
`0xC06710..0xC06715`. The third invokes `0xC376A0` on node
`+0x14` at `0xC06770..0xC06773`.
For the first two, successor is not captured before dispatch: after return,
the code examines current-node byte `+0x0D`, follows links `+8/+0/+4`,
then compares against a freshly read head. No snapshot/refcount operation
was observed; this does not establish mutation or thread safety.

`0xC376A0` then walks its own `+0xE8`, `+0x118`, and `+0x120`
containers, in order. The first pair dispatches node `+0x14`, the third
entry `+8`, all through receiver-only slot `+0x14`.
The next tasks record this static order and trace a concrete manager
registry publisher, rather than simulating tree mutation.

Corrected candidate `0xBF9D80` is a real raw function, unlike the rejected
earlier report: it gates on ready, works through fields `+0x6F0/+0x71C`,
and releases temporary references. Its reported negative search for
"584/588" used decimal offsets `0x248/0x24C`, so it does not resolve
the requested **hexadecimal** `+0x584/+0x588` default sources.
Its supplied converted VA was also incorrect. No default-source conclusion
is accepted from that mismatch; the default-source boundary is parked.

## Static traversal evidence integrated without simulating callbacks

The old-pair projection now carries read-only primary, direct, and nested
traversal records, preserving dispatch order and separating call ranges,
method targets, receiver offsets, and slot byte offsets.
Coordinator review corrected `+0x0D` to a **node-byte offset**, not
a comparison value of thirteen. Successor-link reads remain explicitly
after dispatch; no snapshot or safety guarantee is introduced.
The delegated Startup checks reported 886 passes and no failures.
Successor work follows nested-container cleanup and manager registry
publication, not another reconstruction of the established arithmetic.

## Manager callback publication acquires the key, not an observed payload reference

Raw `0xBFDE80`, reached from initialization, passes UI `+0x6B0` to
`0xC3C9B0` at `0xBFE2CB`. That helper derives a shared-text key,
creates/configures a payload through `0xC63D80/0xC64C90`, and inserts
through `0xC06A60`; existing keys are rejected.
`0xC164C0` stores node `+0x10` with explicit atomic key incref and
copies the payload pointer to `+0x14` without an observed payload incref.
`0xC16570` links, and `0xC08A20` releases the temporary key.
Failure invokes `0xC64420` and payload slot zero. Success publishes
the pointer for the later callback; the report's "tree-owned" interpretation
awaits a concrete successful-payload disposal path.

## Nested E8 payload cleanup follows VM close, unlike the callback walker

Raw `0xC376A0` is traversal only in the inspected function; no unlink,
free, or payload-release operation was observed.
Concrete script close `0xC33060` calls `0xC33E60` at `0xC33206`,
then `0xC3E990` at `0xC3327C`, then VM close `0xCD5510` at
`0xC33291`. Its `+0xE8` cleanup loop begins at `0xC33326`.
For each nonnull node `+0x14` payload, raw `0xC33352..0xC3335D`
invokes slot zero with argument one. This is payload-directed cleanup
after VM close, not only key/node erasure. Concrete slot-zero target and
node-storage release remain unbound by this report.
The next task follows only remaining `+0x118/+0x120` cleanup surfaces.

## Manager callback-node erase does not establish successful payload destruction

The manager destructor `0xBF8AC0` erases `+0x6B0` through
`0xC0BD90`, with per-node helper `0xC0EB00`. That helper releases
key `+0x10` then frees the node; no payload `+0x14` destruction occurs
in this inspected path. Insertion failure separately calls `0xC64420`
then payload slot zero with flag one. Successful-payload ownership remains
parked; callback slot `+0x14` is not destructor evidence or a leak finding.

## Nested cleanup performs ordered payload passes before container resets

After VM close at raw `0xC33291`, `0xC33060` visits payload trees
`+0xF0`, `+0xE8`, `+0xE0`, `+0xD8`, `+0x100`, `+0x110`,
`+0x118`, then `+0x140`. The `+0x118` pass begins at `0xC336DA`
and dispatches node `+0x14` through slot zero with argument one at
`0xC33706..0xC33711`. The pass itself advances links, not key/node frees.
Containers are then reset in that same order. For `+0x118`, helper
`0xC0BE90` runs at `0xC33973..0xC33993`, followed by sentinel-link
and count reset; its body is not established by this report.

Later `+0x120` reaches `0xC3FC10` at `0xC33C2B`, before
`+0x118` helper `0xC0BD90` and stored-object release
(`0xC33C3A..0xC33C58`). Its exact policy is the next bounded task.
These slot-zero cleanup passes remain distinct from `0xC376A0`'s
slot-`+0x14` callback traversal; matching fields do not merge the operations.

## Nested list cleanup frees backing allocation and nodes, not observed callback objects

Raw `0xC3FC10` receives container base for `+0x120`.
Container `+0` points to sentinel, `+4` is count, and conditional
backing pointer `+8` reaches allocator transfer `0xC3FC1A`.
Only when that pointer is nonnull are container `+8/+0xC/+0x10`
zeroed. It self-links sentinel next/previous and clears count.
The forward walk reads node `+0`, saves next before node free
`0xC3FC52`, and finally tail-frees sentinel at `0xC3FC61`.
It does not read node `+8`; the backing pointer must not be conflated
with per-node callback objects. The initial report's payload-release
interpretation is superseded by this instruction-level distinction.
Its reopened VirtualFree gap is also invalid: IAT VA `0x1AC42B4`
was already PE-bound; raw numeric-VA fill is not an import value.

The static nested teardown sequence and slot/argument distinctions are
now represented in `StartupObjectLifecycleNativeLayout`. Coordinator review
made the evidence sequences read-only and named node `+0x10` as a key
field, not a refcount location. Additional immutable PE checks and a
bounded list-publication trace are active; no callback-object ownership
is inferred from node erasure.

## Immutable lifecycle checks caught a receiver-setup address mislabeled as a call

The new binary checks verify shutdown's byte predicate versus word clear,
the selector table entry, CURSORINFO import/initialization, exact MOVSS
record order, and the old-pair divide/threshold/store. Their first run also
caught a tightly related scripting cleanup check failure.
Coordinator byte reads show raw `0xC33C2B` is
`8D 8B 20 01 00 00` (LEA receiver), while the actual call is
`0xC33C31`, bytes `E8 DA BF 00 00`, targeting `0xC3FC10`.
The lifecycle constant and Startup documentation now use the call address.
All Startup checks with `--binary` passed after this correction.
Earlier references above to a call at `0xC33C2B` mean receiver setup,
not the CALL instruction, and are superseded here.

The nested-list publication search did not establish an insertion writer
or independent owner for entry `+8`; prefix-tree publication of the outer
script object is not proof of callback identity. That boundary is parked.
The successor traces only initializer `0xC41670`'s concrete list state,
not another unanchored ownership search.

## Constructor report requires raw-address correction before integration

The `0xC41670` report incorrectly labels requested raw offsets as VAs
and subtracts the code delta, including for the allocator and cleanup
helpers. It also treats caller's `this+0x120` receiver address as a
stack argument value. Its new initializer/allocation assertions are not
accepted pending a narrowly corrected raw-IDB verification.
The existing separation between container `+8` backing and node `+8`
callback remains supported by the earlier exact cleanup instructions.
IAT VA `0x1AC4424` remains PE-bound to standard `_Xbad_alloc`;
another empty import query does not reopen it.

## Correct raw initializer confirms sentinel and separate sixteen-slot allocation

Direct raw-IDB verification confirms caller `0xC32A73` loads ECX with
`this+0x120`, then calls `0xC41670`; `0x120` is not a stack count.
The initializer zeros container `+0/+4` and backing triple `+8/+0xC/+0x10`.
Wrapper `0xC52A10` requests a twelve-byte sentinel and stores its
pointer at container `+0`. A separate vector-helper count of sixteen
requests a sixty-four-byte backing buffer. Both use
`0x12B7FB0 -> 0x12C8A20`, but they are distinct allocation requests.

This supports sixteen DWORD backing slots, not sixteen list nodes.
The exact fill source/values and remaining structural fields are the next
bounded task before calling the composite container a specific library type.
The corrected trace supersedes only the rejected mapping/caller assumptions;
callback entry ownership remains unbound.

The report's apparent VA lookup discrepancy is not a new mapping gap:
the IDB is raw/base-zero and a numeric runtime VA must not be used directly
as an IDB address. Raw `0xCD5510 + 0x400C00 = VA 0x10D6110`,
not the report's `0x10D6120`; the callback arithmetic above is consistent.

## Backing-slot fill and arithmetic regression continuation

The bounded raw-IDB slot trace reports `0xC416E2` supplies a local sentinel
pointer to `0xC53290` with count `0x10`. Capacity helper `0xC52D10`
multiplies by four at `0xC52D23`; fill helper `0xC52E50`, called at
`0xC5333E`, fills the sixteen DWORD slots with that captured sentinel pointer.
The initializer also stores `7` at `+0x14`, `8` at `+0x18`, and float bits
`0x3F800000` at `+0x1C`. These are structural observations, not yet proof
of hash-key semantics or ownership. A structural projection and focused
checks are assigned separately from one bounded key/bucket operation trace.

Immutable PE checks now bind the angle import thunk and two-pi constant,
the original-sum sign/exact-zero correction predicate, and the ready gate,
version increment, and both full-DWORD invalidation stores. Coordinator
execution of the focused Startup checks against the immutable sample passed.
This validates captured instruction evidence, not native execution or x87
emulation.

The structural reconstruction is now implemented as an immutable sixteen-slot
captured-sentinel projection with focused geometry, high-bit-pointer, and
field-value checks. Coordinator review removed an unsupported stack-operand
constant and corrected the documentation: count sixteen is distinct from the
address of the local source pointer. No native allocator is emulated.
The bounded lookup-shape search found no anchored key/bucket operation;
semantic container identity remains parked, not disproved.
Active successors verify initializer PE anchors and separately trace UI child
`+0x24` release from the concrete manager destructor.

## Composite initializer PE anchors integrated; shutdown child transfer located

Coordinator execution of the immutable-binary Startup checks passed after
adding initializer/allocation/fill and separate backing/list teardown checks.
Raw `0xC416E2` is `89 45 F0`, storing the sentinel in `[EBP-0x10]`;
the immediate count `0x10` is pushed at `0xC416E7`. These are not a
positive frame-offset count read. The byte-backed checks are committed.

The bounded manager destructor inspection did not establish direct child
`+0x24` release; its thunk's conditional free uses the original manager.
A separate shutdown trace located a candidate concrete transfer:
`0xBFAD2A` loads child `+0x24`, `0xBFAD3B` calls `0xCA7330`,
`0xBFAD48` calls `0xBFAF40`, then `0xBFAD4D` clears the field.
Exact bytes and helper argument/free flow are assigned for verification.
The initial helper report is not yet accepted as a release conclusion:
it mixes address labels and contradicts its own reported outer-pointer
free. Its decimal displacement `336` must be read as hex `0x150`,
not field `0x336`. No dependency virtual target or ownership is inferred.

## Exact child shutdown bytes establish receiver-based native release

The verified raw-IDB bytes resolve the candidate shutdown sequence.
`0xBFAD2A` is `8B 4B 24`; `0xBFAD37` tests ECX and skips on null.
`0xBFAD3B` calls `0xCA7330`; `0xBFAD40` reloads the field, tests it again,
then pushes the same pointer before `0xBFAD48 -> 0xBFAF40`.
`0xBFAD4D` clears manager `+0x24` after the helper returns.

Helper `0xBFAF40` takes its actual receiver from ECX into ESI, not from
the pushed copy. It loads child `+0x150` into EDI and, when nonnull,
calls `0xCA6070` at `0xBFAF50`, then native free `0x12C8D60`
at `0xBFAF57` with that captured pointer. It loads dependency `+4`
and dispatches its vtable slot `+8` at `0xBFAF82`; the intervening
null diagnostic is not evidence that execution cannot continue.
`0xBFAF85` sets ECX=ESI, and `0xBFAF87` calls `0x12C8D60`
on the child itself. `0xBFAF90` is `C2 04 00`; the stack copy is
not read by this body. These are separate release actions, not proof
that child and nested pointer values cannot alias. The indirect
dependency target and its ownership contract remain unresolved.
The earlier report's first-stack-argument receiver and missing-child-free
interpretations are superseded. A bounded successor traces only the
preceding `0xCA7330` phase; the structural code integration is active.

## Child reset phase and shutdown naming correction

The `0xCA7330` bounded trace reports eight pairs of child DWORD pointer
fields spanning `+0x24..+0x60`. Each nonzero field dispatches virtual slot
`+8` and is cleared. The first pair calls at `0xCA734A/0xCA735D`
are followed by zero stores at `0xCA734D/0xCA7360`.
Additional conditional dispatch/clear fields are `+8,+0x14,+0xC,+0x10,+0x74`.
The child `+8` store at `0xCA7379` uses EBX after its loop count reaches
zero. A conditional diagnostic reads `+0x13C`; `0xCA73F2`
calls native free with child `+0x64`, then `0xCA73F7` clears `+0x68`,
not `+0x64`. The method returns with plain RET at `0xCA7401`.
No EDX argument semantics follow from the decompiler's fastcall label.
Its direct accesses do not include child `+4/+0x150`; untraced called
targets may have side effects.

The first structural integration mislabeled the `0xBFAD2A` sequence
as a destructor path. This is incorrect: it is inside gated shutdown
`0xBFA0D0`, not destructor `0xBF8AC0`. That code/docs correction is
assigned with the reset-phase integration before committing it.
The child release PE byte checks have independently passed and are pushed.
An independent successor traces only nested-child helper `0xCA6070`.

## Nested child cleanup remains distinct from outer child dependency

The bounded `0xCA6070` trace operates on the nested object captured from
outer child `+0x150`. It conditionally dispatches nested `+0x1C` through
vtable slot `+8` at `0xCA60AA`, then clears that field at `0xCA60AD`.
When nested `+0x10` is nonzero, native free at `0xCA60BB`
is followed by zeros at `+0x10/+0x14/+0x18` (`0xCA60C0/C7/CE`).
When nested `+4` is nonzero, native free at `0xCA60DC` is
followed by zeros at `+4/+8/+0xC` (`0xCA60E1/E8/EF`).
The plain RET is at `0xCA6105`. These conditional groups do not
establish field clears when their initial pointer is zero.

Nested-object storage is released later by caller `0xBFAF57`;
outer-child storage is released at `0xBFAF87`. Nested `+4`
must not be merged with outer-child `+4`, whose virtual dispatch remains
unresolved. Likewise a structure field at `+8` is not a vtable slot.
A bounded static layout/check implementation is assigned; no native
cleanup execution or inferred alias/refcount contract is introduced.

The corrected outer-child shutdown/reset layout and focused checks are now
committed and pushed. Coordinator review also removed a spurious second
"child +8 loop" constant: the store at `0xCA7379` follows the exhausted
eight-pair loop and uses its zero counter; it does not precede another
eight-iteration loop. The reset-phase PE assertions are awaiting completion.
An independent bounded successor examines only constructor/initializer
writes relating the child `+0x64` allocation and `+0x68` field.

## Backing allocation anchors and nested layout review

The bounded constructor/initializer trace establishes outer child
`+0x64/+0x68` zero stores at `0xCA63D9/0xCA63E0`.
Initializer `0xCA6748` requests `0x800` bytes through raw allocator
`0x12C8A20` at `0xCA674D`. The returned allocation is stored
at child `+0x64` at `0xCA67A0`; subsequent stores set
`+0x68=0x20` and `+0x6C=0`. These values do not alone prove
a capacity/count or record-size interpretation. A bounded successor
checks the allocation result's null branch and initialization stores.
Neutral layout/check integration and immutable PE anchors are active.

The nested cleanup model now has a standalone focused check registration.
Coordinator review corrected another concatenated instruction/address:
the RET address is raw `0xCA6105`, with byte `C3`, not
`0xCA6105C3`. The current focused Startup build/check run passed.
Reset-phase PE assertions add the sample SHA-256 predicate and exact
loop/free-versus-clear bytes; their final combined integration is pending.

## Nested cleanup and backing reconstruction integrated with binary evidence

The registered nested cleanup checks, neutral backing initialization layout,
reset-loop byte assertions, and nested cleanup/allocator PE anchors all pass
in a combined coordinator run against the pinned sample. The integration
is committed and pushed. Coordinator review corrected the backing layout's
mistyped constructor offsets `0xCA6379/0xCA6380` to the independently
byte-checked `0xCA63D9/0xCA63E0`.

The allocation-return trace observes EAX copied to EDI at `0xCA6752`,
tested at `0xCA6754`, and nonzero branching around a diagnostic at
`0xCA6756`. A null result reaches the same stores at `0xCA6799`
if that diagnostic returns; there is no intervening caller-side loop.
The comparison of child `+0x74` at `0xCA6799` supplies flags for
`0xCA67B1`: the intervening three stores do not change them.
It is not a second allocation-success test. A pure captured-continuation
projection is assigned, explicitly limited to reaching this join, alongside
a separate bounded trace of the remaining initializer return/caller use.

The captured allocation continuation is now implemented and registered with
focused checks. Its nullable result explicitly represents that the join was
not reached when a null-allocation diagnostic did not return. Coordinator
review named the boundary a continuation join, not a function return:
`0xCA6799` is not RET. High-bit captured pointers and the independent
`+0x74` gate are covered. This code/docs integration is pushed.
Exact initializer AL-return/caller failure bytes and the final `0xCA6C80`
boolean helper are the next independent bounded evidence scopes.

## Initializer return gates and failure-epilogue distinction

The allocation diagnostic-join PE checks passed coordinator execution and
are pushed. Their label now correctly calls the `0xCA6756` JNZ the
nonzero-result bypass, not a null-result bypass.

The later child initializer calls `0xCA6C20` at `0xCA6A61`.
A zero AL takes a path clearing child `+0x6C` and AL before return;
nonzero reaches `0xCA6AA3 -> 0xCA6C80`, whose AL is normalized
with TEST/SETNZ before return at `0xCA6ABD`.
The UI caller at `0xBFB542` tests AL and branches on zero to
`0xBFB668`. That target is a zero-result epilogue returning with
`retn 0x20`, not itself a cleanup call sequence. Earlier references
to "failure cleanup" at this exact target are too strong.
Nonzero continues UI initialization; it does not yet set ready `+0x672`.
Exact PE assertions and a captured two-gate projection are assigned.

The bounded `0xCA6C80` trace reports an initial call at `0xCA6D37`
to `0x12BB2A0`, then eleven required lookup calls to `0xCA7090`
and `0xCA71F0`. Only after these succeed does it replace eight
pairs at offsets `+0x24..+0x60`. Success sets BL=1 at `0xCA6F49`;
failure sets BL=0 at `0xCA6FCF`; `0xCA7061` transfers BL to AL.
The key addresses remain opaque and indirect object targets unresolved.
A separate bounded lookup-helper contract trace is active.

## Captured initializer gates integrated; required lookup terminal predicate

The captured two-helper result projection is registered, passes focused
checks, and is pushed. Coordinator review removed an unsupported caller
continuation address and named the `+0x6C` flag as a direct clear action,
not a claim about all indirect helper side effects. Optional second-helper
capture consistency is validated; nonzero bytes normalize to one.

The lookup-helper trace reports both `0xCA7090/0xCA71F0`
pass an output address through a local object's virtual slot `+0xC`,
then dispatch a returned value through child dependency `+4`.
Their dependency slots differ: `+0x16C` versus `+0x1A8`.
The terminal success predicate is signed dispatcher status nonnegative
and captured output DWORD nonzero. These helpers do not themselves
write or clear the output DWORD; unresolved called targets receive it.
Temporary virtual cleanup does not establish a refcount contract.
A terminal captured-predicate projection is assigned.

The allocation-null diagnostic formats via PE-bound `memset`
(raw stub `0x159D8FA`, IAT VA `0x1AC4AD4`) and `_snprintf`
(IAT VA `0x1AC4870`). The actual report call at `0xCA6790`
uses shared non-import global VA `0x1C15EA0`. Its exact callee
and return contract remain unresolved; the static return continuation
does not prove the diagnostic returns. A single initializer/writer
check is assigned, not another broad logger investigation.

## Required lookup projection integrated; diagnostic binding was already known

The terminal required-lookup predicate is registered and pushed after a
focused coordinator run. Its scope is explicitly the final dispatcher/output
test, not the preceding query stages.

The diagnostic publication check found that the previous handoff already
bound global VA `0x1C15EA0`: its `.rdata` raw offset is `0x1814AA0`,
bytes `C0 37 6C 01`, pointing to VA `0x16C37C0`, raw `0x12C2BC0`.
See the earlier "Allocation reporter cell maps to a return-capable assertion
wrapper" section. Thus the null-allocation call is not a new unresolved
callee/publication gap. The `.rdata` cell is image-initialized; prior
bounded references found callers, not publication writes. Downstream
termination behavior is not inferred from this binding. An immutable
caller-to-existing-cell regression extension is active.

Initializer AL-return PE checks are awaiting final combined integration;
coordinator review corrected their label to say that failure clears child
field `+0x6C`, not the whole child. The success pair-replacement trace
and exact required-lookup predicate bytes remain separate bounded tasks.

The terminal predicate bytes are now independently verified:
`0xCA7199` dispatches `[ESI+0x16C]`, then `0xCA719F` tests EAX
directly, `0xCA71A1` branches on sign, and `0xCA71A3/0xCA71A6`
test the output DWORD for zero. The second helper uses `[ESI+0x1A8]`
at `0xCA72CF` with corresponding tests at `0xCA72D5/0xCA72D7`
and `0xCA72D9/0xCA72DC`. Both return one only on the accepted
terminal path and otherwise zero. Global `0x1AC40AC` belongs to the
earlier query stage, not normalization of this terminal EAX result.
The active binary integration now includes these exact tests; one bounded
successor checks only the first-stage adapter identity and call contract.

## Success pair replacement consumes scratch pairs and preserves callout ordering

The bounded replacement trace establishes eight iterations, destination
pair offsets `+0x24/+0x28` through `+0x5C/+0x60`, and stride eight.
The new values come from sixteen consecutive scratch DWORDs, not a
broadcast pair. Scratch pair zero remains zero from the earlier
sixty-four-byte zeroing. Other pairs contain opaque lookup outputs and
some repeated captured values; explicit virtual `+4` calls precede
some scratch duplications, without establishing an ownership contract.

The old first field is read and conditionally dispatched through slot `+8`
before the second old field is read. That second read precedes the first
new store (`0xCA6F0A`); its conditional slot `+8` call/clear precedes
the second new store (`0xCA6F22`). Both new stores are unconditional
within a reached iteration. An atomic snapshot of all old fields would
erase this callout/read ordering. The active projection therefore covers
only captured scratch values and destination writes, not native releases
or mutation safety; exact loop byte checks are assigned separately.

The captured write projection is now registered, passes focused checks,
and is pushed. Coordinator review corrected misleading register labels:
EDI is the zero-based loop index, not the scratch base; ESI is the
destination cursor at child `+0x28`, not the original `this` pointer.
The projection copies values directly into immutable write records.

The first-stage slot VA `0x1AC40AC` is PE-bound to
`D3DCOMPILER_43.dll!D3DCompile` (IAT raw `0x16C2CAC`).
Calling it a status adapter is therefore misleading. The earlier helper
report described only the final pushed value, not the full API argument
frame. A bounded successor reconstructs the complete call arguments at
raw `0xCA70FA/0xCA725A`; shader/key meanings remain unassigned until
the pointer/length and literal arguments are verified.

The combined immutable PE assertions for initializer AL gates, the known
reporter-cell target and return bytes, terminal lookup predicates, and
replacement-loop read/call/write ordering passed coordinator execution and
are pushed. This closes their byte-regression integration, not the unresolved
indirect targets. Active bounded work is compiler-call argument framing
and failure-versus-shared scratch cleanup after the lookup gates.

## Compile argument frame and failure scratch cleanup bounded

The compiler framing trace reports eleven `D3DCompile` arguments at
raw `0xCA70FA/0xCA725A`: virtual `+4` return supplies the source
pointer, virtual `+0xC` return the source byte count; source name,
defines and include are null; entry/profile are
`general_drawing_vs`/`vs_3_0` or `general_drawing_ps`/`ps_3_0`;
both flags are zero; final arguments are local code/error blob outputs.
Pointer/size roles follow confirmed API argument positions, not concrete
virtual-target identification. Captured framing code and independent
PE/import/literal checks are assigned.

The scratch failure trace lists eleven lookup gates, not eight:
`0xCA6D8B,0xCA6DA9,0xCA6DD5,0xCA6DF3,0xCA6E11,0xCA6E2F,
0xCA6E4D,0xCA6E6B,0xCA6E89,0xCA6EB5,0xCA6EE1`.
Their false paths reach the eight-pair scratch cleanup at
`0xCA6F50..0xCA6F82`, before destination replacement.
Each nonzero scratch field dispatches virtual `+8` then clears.
This failure path performs no direct destination-pair writes here;
called-target side effects remain unresolved. It sets BL=0 at
`0xCA6FCF`; success sets BL=1 at `0xCA6F49`; the common tail
transfers BL to AL at `0xCA7061`.
The earlier initial direct-call failure bypasses scratch cleanup.
Its callee is not labeled a parser without separate evidence.
A bounded static failure-layout/check integration is assigned.

The scratch failure layout/check registration is now committed and pushed
after a focused coordinator run. Review distinguished the destination
replacement loop `0xCA6EF0` from failure entry `0xCA6F50`,
renamed result instructions so they are not described as calls, and kept
the `var_58..var_1C` suffixes as decompiler-local names rather than
unverified runtime stack coordinates. Exact machine geometry is assigned.

The shader-frame implementation initially reported completion without the
requested files appearing in this worktree. A same-task delivery correction
is pending; it is not integrated or committed. Independent active scopes
are compile import/literal PE assertions and code/error blob cleanup.

The shader-frame files were subsequently delivered to this worktree.
Coordinator review added separate raw helper anchors for both variants,
registered their focused checks, and pushed the verified capture-only
projection. It preserves all eleven arguments and does not compile code.
Independent source-object provenance, blob cleanup, and failure-loop byte
geometry traces are active alongside the compile-frame PE integration.

## Scratch machine coordinates and compile blob cleanup

The exact scratch failure loop initializes ESI=0 at `0xCA6F50`,
uses `[ESP+ESI*8+0x24]` and `[ESP+ESI*8+0x28]`, and branches
from `0xCA6F82` back to `0xCA6F52` through eight iterations.
These encoded ESP operands do not justify `[EBP-0x58]` addressing
from a decompiler-local suffix. Their structural constants/checks are pushed.
Misplaced duplicate shader-frame files under `newsrc/newChecks` were
removed after their corrected versions were integrated under `src`.

The bounded compiler blob trace reports code output `[EBP-4]` and
error output `[EBP+0x10]`; the original caller output address survives in EBX.
On negative compile status, error output is used for diagnostics then
conditionally cleaned through virtual `+8`; that branch does not clean
the code output, without establishing whether the API can leave it nonzero.
On accepted compile status, a nonzero error blob is cleaned and its slot
zeroed before code use. Code output is then cleaned on either terminal
lookup result. The shared failure cleanup therefore handles a selected
pointer, not an unconditional list of both outputs.
A bounded static path layout and immutable PE checks are active.

The source provenance trace identifies the helpers' first explicit argument
as CA6C80's local wrapper `v28`, initialized with vptr VA `0x1C80CC4`
and zero, not the EDI input. The initial `0x12BB2A0` call also
receives a formatted-text value via another local. The bounded trace did
not establish the wrapper's buffer writer or justify calling that routine
a parser. A concrete wrapper-vtable check is active.

The blob layout files are delivered but remain unregistered pending exact
verification of the shared cleanup's selected register. Their conditional
path claims must not be validated merely against matching constants.
The compile framing PE checks are also delivered; the active binary
integration agent is adding scratch/blob guards before coordinator review.

The shared blob-cleanup register selection is now independently confirmed:
the negative-compile path carries error output into common cleanup,
while terminal-predicate failure selects code output. After this verification
the static blob layout/check registration passed coordinator execution and
was committed/pushed. Independent initial-loader output transfer and
wrapper vtable binding remain active; combined PE regression integration
is waiting for the selected-pointer guard assertions.

The first wrapper-binding report contains a PE arithmetic error:
`0x1C80CC4 - 0x400000 = 0x1880CC4`, not `0x1C40CC4`.
Its resulting `.rsrc`/fill-byte interpretation is rejected.
The established `.rdata` mapping instead gives raw `0x187F8C4`;
a narrowly corrected slot `+4/+0xC` trace is requested. This is
not a new sample-layout gap or evidence that the wrapper lacks a vtable.

The loader output trace reports the wrapper receiver saved from EDX at
raw `0x12BB2D7`. Its slot `+0x1C` is called at `0x12BB3DA`,
then slot `+4` supplies the buffer at `0x12BB403`.
The chunk call at `0x12BB40E` uses IAT VA `0x1AC490C`
with pointer, length, one, and handle; a result other than one
selects receiver slot `+0x14` and failure.
The report's numeric-VA `0x08080808` fill is not an import identity;
a bounded immutable PE binding is assigned instead. IDA EAs here
are raw file offsets, not RVAs despite that report's mixed label.
No transfer semantics are inferred until the import is resolved.

## Corrected wrapper binding and combined compiler PE integration

The corrected wrapper trace confirms vtable VA `0x1C80CC4`
at raw `0x187F8C4`. Slot `+4` contains VA `0x16CC560`,
raw code `0x12CB960`; slot `+0xC` contains VA `0x16CC5C0`,
raw code `0x12CB9C0`. The latter returns zero for null wrapper
backing at `+4`, otherwise reads backing DWORD `+0xC`.
The former uses that size and an explicit argument before returning
backing data `+4` plus the argument or null; exact compile-site
argument/default resize behavior is a bounded successor, not assumed.
A captured size projection is assigned.

The combined PE checks for compiler IAT/literals/full argument frames,
scratch failure-loop geometry, and selected code/error blob cleanup
all passed coordinator execution and are committed/pushed.
This closes their regression integration. Active boundaries are wrapper
size projection, exact virtual-`+4` argument behavior, and loader chunk
import identity; the earlier resource-mapping gap remains rejected.

The loader chunk import is now PE-bound: IAT VA `0x1AC490C`,
raw `0x16C350C`, is `MSVCR120.dll!fread`.
The observed call is `fread(buffer, chunk, 1, handle)`, so the
accepted result is one complete element, not a byte count.
The cap is `0x01000000` bytes per chunk; the receiver's slot `+4`
receives the cumulative offset. A captured chunk-cap/result predicate
and independent PE anchors are assigned.

The compile source pointer calls explicitly pass zero to wrapper slot `+4`
at `0xCA70F4/0xCA7254` (`6A 00 FF 50 04`).
The latest wrapper report describes an unsigned argument/length comparison
and a preserve-helper call, but its final size-getter expression is
ambiguous between a field read and address arithmetic. Exact bytes and
branch/call order are requested before accepting that refinement or
registering the captured-size projection. No resize bypass is assumed.

The clarification confirms the size getter returns DWORD `[backing+0xC]`,
not its address. In slot `+4`, CMP at `0x12CB96E` and unsigned JBE
at `0x12CB971` select `0x12CB98D`; even offset zero calls
`0x12CBCB0` at `0x12CB992` with preserve flag one and current length.
Thus no helper-call bypass is implied by zero offset.
The captured size and chunk predicates are now registered, passed focused
checks, and pushed. Active successors trace only the preserve helper's
equal-size/null-backing behavior and the loader's requested-length clamp.
Independent PE checks cover the wrapper mapping/getter and fread anchors.

The wrapper/fread PE groups passed coordinator execution and are pushed.
Existing binary evidence already names this wrapper `slBlob`; subsequent
preserve-helper analysis must reuse that prior art rather than reopen its
identity. The bounded preserve trace reports equal-size access may take a
slow allocation/copy path when uniqueness/alignment conditions fail, and
zero requested length clears wrapper backing/control fields. The immediate
allocation-failure behavior is still under exact verification before projection.

The loader request is an unsigned minimum of the captured size and caller
stack cap at `0x12BB3D0/0x12BB3D3`; zero skips slot `+0x1C`.
A small extension of the existing read projection is active, separate from
the chunk-size cap. No file-size semantic provenance or overflow contract
is inferred beyond the observed captured arithmetic.

The unsigned requested-length extension passed coordinator focused checks
and is pushed. Its boundary matrix includes both sides of the signed high
bit and composes the total request with the separate chunk cap.
Exact CMOVA/zero-skip PE assertions are active.

The preserve failure report conditionally reaches an unguarded new-backing
read before wrapper pointer replacement when the allocator-output header is
null, but includes malformed address `0x12CBDDDF`. No failure projection
or runtime inevitability claim is accepted from that typo. A narrow caller
instruction-byte verification is active; lower allocator failure/reporting
contracts remain as previously documented.

## Immutable clamp checks correct zero-length setup claim

Exact PE bytes contradict the earlier trace's cap displacement and
setup ordering. `0x12BB3D0` is `3B 7D 0C` and `0x12BB3D3`
is `0F 47 7D 0C`: the cap is `[EBP+0xC]`, not `[EBP+8]`.
Receiver slot `+0x1C` is called at `0x12BB3DA` before
`0x12BB3E3` tests EDI for zero. Thus earlier statements here
that zero skips receiver setup are superseded. Coordinator removed
`IsRequestedLengthSetupNeeded`, corrected documentation, and pushed
the code plus exact CMOVA/setup/test regression checks after a combined
immutable-binary run. The unsigned minimum itself remains valid.
A bounded successor verifies only the post-setup zero branch and return.

The preserve caller anchors are also corrected: `0x12CBDD0`
reads the new header's `+4` field on the positive preserve-copy path;
`0x12CBDDF` is the no-copy path's `CMP ESI,[EBX]`, not the malformed
`0x12CBDDDF`. Later logical-length and wrapper field writes are
`0x12CBDFD`, `0x12CBE07`, `0x12CBE15`.
These conditional captured-null observations are being recorded as static
access-order evidence, not a clean failure emulation or inevitable runtime fault.

## Initial-zero versus cap-zero and zero-resize fast-path distinction

The post-setup follow-up also found an earlier initial-size zero test at
`0x12BB3C8`: initially zero skips the receiver setup, while a positive
initial size capped to zero calls slot `+0x1C` before skipping the read loop.
Both normal tails return one; setup's return is not tested.
Thus neither blanket "zero skips setup" nor "all zero requests call setup"
describes both paths. A captured two-input setup/read plan is assigned.

The zero-resize trace similarly distinguishes branches: a unique/aligned
in-place path can set logical length zero without detaching; only the
replacement zero path clears wrapper `+8/+4` then releases saved control.
Slot `+0x14` resolves to raw `0x12CBA00`, calling resize `(0,0)`.
The reported atomic-count test is under exact verification before acceptance.

The slow-preserve instruction evidence is integrated and pushed. Coordinator
review corrected concatenated code addresses `0x12CBDFD8/0x12CBE078/
0x12CBE158` to `0x12CBDFD/0x12CBE07/0x12CBE15`, corrected the
compare to `0x12CBDDF`, and separated allocation call versus target names.
Independent immutable PE checks of those anchors remain active.

Prior-art navigation: the earlier "AES destination is slBlob, not a cursor
stream" and "AES publication fields and realignment correction" sections
already bind RTTI, slot `+0x1C` as logical resize with preservation,
slot `+0x14` as zero resize without preservation, and backing/control
field geometry. Subsequent tasks must reuse those facts. The current
source-read connection adds its captured caller gates and PE regressions,
not a new wrapper identity or capacity-only interpretation of setup.

The captured setup/read plan is now implemented and pushed in
`UiShaderSourceReadProjectionRules.ProjectReadPlan`: setup is reached only
when the original captured size is nonzero, while the read loop is reached
only when the unsigned minimum of that size and the caller cap is nonzero.
Boundary cross-products cover the two distinct zero paths. The plan does
not emulate receiver null guards, cleanup, or allocation outcomes.

The exact zero-release follow-up supersedes the earlier unaccepted
old-count-zero wording: the first `LOCK XADD [control+4],-1` sets flags
from the arithmetic sum, and its following JNZ gates destruction on the
post-decrement value. The separate `control+8` sequence decrements the
returned old value before testing it. These distinct observations do not
establish initialization invariants or justify a shared-pointer type name.
Immutable PE checks and a branch-specific wrapper-clear layout are assigned.

Those checks and the wrapper-clear layout are now integrated: the focused
runner registers `UiShaderSourceClearChecks`, and immutable sample checks
bind the clear sequence, both atomic flag sequences, slot `+0x14` dispatch,
and slow-preserve access order. The release address `0x12CBE24` is named
as an entry target, not as the address of the zero-branch instruction
(`0x12CBD58`). The latest pushed evidence commit is `1dbe58f`.

Active follow-ups are the exact in-place predicate, bounded allocator
output-pair contract, captured read-tail continuation, and non-detaching
slot `+8` offset projection. A first read-tail custom-agent attempt lacked
MCP access and produced no fresh sample evidence; its successor must use
available MCP discovery or immutable local byte disassembly, not promote
the predecessor's repository-only observations to verified instructions.

## Capacity reuse predicate correction

The bounded fast-path trace resolves the size comparison as **capacity at
backing `+0`**, not old logical length at `+0xC`. Any earlier shorthand
that required requested size to be no greater than old length is superseded.
At raw `0x12CBCF5..0x12CBD13`, the gates test nonnull control,
control `+4 == 1`, unsigned requested size no greater than an earlier
capacity capture, and zero alignment or zero unsigned data-pointer modulo
alignment. Raw `0x12CBD15/0x12CBD17` re-reads capacity and takes JBE to
the length write `0x12CBD33`. The above-capacity alternative enters an
assertion block and can reach that write if the diagnostic returns;
the direct-edge projection must not classify every alternative as failure.
Separate capacity captures are required; no atomic snapshot is established.

The fast-path agent's reported entry VA `0x16DCCB0` was arithmetic error:
raw `0x12CBCB0 + 0x400C00 = VA 0x16CC8B0`. Raw anchors remain the
evidence basis. Capacity predicate code and immutable PE gates are assigned.

The read-tail and allocator-branch projections are integrated and pushed at
`5e295d9`. Chunk success updates offset/count and may continue; any
non-one `fread` result enters receiver clear and conditional normal return
zero. Completed or bypassed reads return one when cleanup/cookie checking
returns. A zero remaining-count capture is rejected as not an entered loop.
The allocator projection represents only its branch and initially zero pair,
not the eventual allocation result. Its checks are registered in Program.

The read-only slot `+8` getter review found another snapshot hazard:
virtual size is obtained before the later wrapper backing-pointer load.
The pending implementation must accept those as independent captures,
including nonzero earlier size and later null backing. It must not infer
the earlier size from the later pointer's nullness.

The corrected independent-size getter and separate-capacity reuse predicate
are registered and pushed at `16e3dcd`. Their focused checks include crossed
size/backing captures and distinct first/second capacity values.

Bounded successor findings awaiting implementation/PE integration:
preservation copies unsigned minimum of the later virtual-size accessor
result and requested size, not an assumed earlier backing-length snapshot.
The accessor is reached on the positive replacement path even when the
preserve flag later suppresses copying. The pool control helper
`0x12CC700` initializes visible count fields `+4/+8` to one when its
captured control allocation is nonnull, then writes output control before
header; this does not establish a general ownership type.

Constructor `0x12CB700` performs DWORD capacity-plus-alignment allocation
arithmetic. Its diagnostic tests zero capacity, not allocator nullness.
Nonzero alignment uses alignment minus unsigned remainder, advancing a
full alignment unit when already aligned. Zero alignment avoids division.
The constructor report's claimed raw `0xECB100` was an address-basis
error: the IDB EA is already raw `0x12CB700` (VA `0x16CC300`).
No allocator-return null guard or runtime-fault guarantee is inferred.

Immutable PE anchors for the read tail, non-detaching getter, allocator
entry zeroing/pool gate, and direct capacity reuse are pushed at `cee5160`.
The pinned-sample runner passes with all registered projection checks.
The next coherent integration owns three new projection/check pairs
(preservation copy, backing alignment arithmetic, and pool control output)
plus one serialized BinaryEvidence writer; no other writer should touch
that evidence file until its current assignment completes.

Coordinator review caught two pre-integration defects in pending code:
the alignment projection incorrectly returned header capacity zero for
nonzero input capacity, and the control helper used an unestablished
"weak reference count" name for field `+8`. Both are assigned back for
correction before staging. Passing mirrored constant checks alone does
not validate either claim; retain independent PE evidence.

Copy preservation and pooled control capture code are registered and
pushed at `ca56d6a`. Coordinator review also renamed the preserve-byte
offset to an EBP-relative argument, not an object field. The positive
replacement projection requires its accessor capture before testing any
preserve byte, and preserves all byte values' zero/nonzero behavior.
Primary/secondary control count names avoid unsupported ownership labels.
Alignment capacity correction remains pending; do not stage that pair
until its native capacity store and corrected focused checks are reviewed.

The corrected alignment projection and independent PE capacity-store
anchor are integrated and pushed at `afff299`, together with copy gate,
alignment branch, and control-initialization PE checks. Header capacity
now preserves the caller input; registered boundary matrices include
zero, one, `0x20`, and maximum DWORD capacity. The pinned sample run passes.

Active continuations: pooled control vtable targets, nonpool output-pair
writer, absolute resize adapter, and wrapper destructor layout. The
destructor's first report repeated the XADD prior-zero interpretation;
it has been returned for exact flag verification before any integration.
Do not accept that wording without inspecting XADD/JNZ versus XADD/DEC/JNZ.

The pool control vtable at VA `0x1C80C8C` / raw `0x187F88C`
maps slot zero to raw `0x12CC930` (load header `+C`, null guard,
tail to `0x12CCA10`) and slot four to `0x12CC940` (nonnull receiver
dispatches slot eight with deleting argument one). Neither bounded entry
directly clears header `+C`. Immediate tail/deleting-target traces are assigned.

Nonpool allocation follows `0x12CBFFC` to a separate header constructor
and pair helper `0x12CC7D0`; its control request is `0x10`, with vptr
VA `0x1C80CB0`, versus pooled request `0x14`/vptr `0x1C80C8C`.
The observed pair members may independently be null. Captured variant
support is assigned without inventing allocation success.

Absolute preserving resize slot `+0x1C`, raw `0x12CBAC0`, forwards
the requested DWORD and preserve argument one to `0x12CBCB0`, leaves
ECX unchanged, and returns EAX unchanged with RET four. This adapter does
not define a boolean result; the source loader ignores its result. An
immutable PE writer owns these adapter, destructor, and nonpool anchors.

Wrapper destructor and nonpool control variant code are pushed at
`4a031e3`. The corrected first XADD/JNZ predicate uses the post-decrement
sum; its nonzero branch bypasses both control callbacks. The second count
capture is required only on the first zero edge, after slot-zero callback.
Wrapper disposition is conditional on reached callbacks returning.

Pooled header return raw `0x12CCA10..0x12CCA7D` reports on initial
header `+8 != 0` (ID `0x3F`) and capacity `!= 0x800` (ID `0x40`).
If those diagnostics return, it separately rereads `+8`, clears header
`+4`, calls `0x12C8D60` with the negated later `+8` value in ECX,
atomically decrements pool `+0x10`, then hands the header to `0x12CDA60`.
Do not silently substitute the data pointer for that unusual negated
displacement argument. The bounded entry shows no heap fallback; its
callee effects are unresolved. A branch-specific layout is assigned.

Destructor counter-boundary matrices and immutable PE gates for wrapper
destruction, preserving adapter forwarding, and both control variants are
pushed at `cf5b3a6`. First/second counter values zero, one, two, high-bit,
and maximum DWORD are crossed with every deleting-flag byte. The sample
checks pass without executing the target.

The pooled deleting slot is mapped separately: vtable `+8` points to
raw `0x12CC9B0`, which replaces the vptr with VA `0x1C6077C` and
conditionally calls the allocator-release backend on flag bit zero.
This does not itself release header `+C`. Control dispatch/layout and
pooled return PE anchoring are active; the nonpool header-release trace
is also active and should not be duplicated.

Nonpool control vtable VA `0x1C80CB0` / raw `0x187F8B0` maps
slots zero/four/eight to raw `0x12CC980/0x12CC9A0/0x12CC9D0`.
Slot zero returns for null header; otherwise it calls `0x12C8D60`
with DWORD `(header data +4) - (displacement +8)`, then tail-calls
the same backend with the header pointer. This differs from the pooled
return's negated displacement. A zero computed first argument remains
an observed call, even if the backend null guard returns immediately.
No fields are directly cleared in that bounded nonpool body.
Captured nonpool release code and independent PE checks are assigned.

Pooled return/control dispatch PE windows are pushed at `425a649`.
Active work also includes pool-return and control-deleting layouts and
the immediate pooled acquisition interface; their implementations remain
uncommitted until their completion reports and coordinator review.

The pooled return and control-deleting layouts are now registered and
pushed at `fe27bd4`. All registered checks pass, including the control
release check file that briefly caused a concurrent build error while
its writer was still editing. Do not treat that transient report as a
remaining baseline failure.

Active successor IDs: `44e52414-d931-4053-8077-7324cc9ed841`
(nonpool captured release), `3614ab89-c0ab-421c-92b1-d95040533284`
(pooled acquisition interface), `7611a4ab-2088-4915-a104-fecce5b46f3d`
(nonpool immutable PE owner), and `84ff872f-2b85-44a8-b7d2-185110613bf7`
(nearest pool singleton writer). Keep their scopes separate.

## Pooled acquisition and singleton publication boundaries

The acquisition interface `0x12CDE60` calls `0x12CDAE0`, invokes
`0x12CE0F0` and retries on zero, then increments pool `+0x10` before
checking acquired-pointer modulo pool `+0x18`. The latter diagnostic has
identifier `0x254`. No ordinary zero return was observed, but this is not
a termination or allocation-success guarantee. The caller's explicit
zero-result branch remains distinct from the helper's ordinary return.

The six observed initialization guards call `0x12CC110` when global
VA `0x1E00510` is zero. With receiver VA `0x1E00500`, the helper
requests `0x28` bytes and conditionally configures the allocation through
`0x12CDB40` with literals 2064, 32, and one. Alias-derived publication
at raw `0x12CC1AA` writes receiver `+0x10`, then links the receiver
through global VA `0x1E4B0A8` and sets receiver byte `+4`.
The observed null-allocation continuation can publish null while setting
the flag; callers do not immediately recheck readiness. Captured candidate
and publication projections are assigned separately from acquisition internals.

Nonpool release projection, registered checks, and exact vtable/body PE
anchors are pushed at `ce70d1b`; the pinned-sample runner passes.
Pooled candidate and singleton publication code/PE remain active, alongside
one bounded wrapper-copy trace. The PE file is now exclusively assigned
to `d508d7b1-3120-4dd7-962a-d0628cc0f638` until its completion.

Captured pool-candidate initialization is registered and pushed at
`0354ef2`. Singleton publication code and candidate/publication PE checks
remain active. A bounded copy-constructor trace selected raw `0x12CB7F0`:
destination vptr and zero pair precede helper `0x12CC430`; source
alignment is read later. Captured constructor code and the old-destination
assignment-order trace have separate assignments; no deep backing copy
or atomic source snapshot is inferred.

Pool singleton captured publication, registered checks, and candidate/
publication PE anchors are pushed at `0e08ec7`. The complete pinned-sample
runner passes. The projection deliberately starts after allocator return;
own-entry guards and nearest teardown remain separate active traces.
The PE file is now assigned to
`330036bf-8b4d-421e-9e0d-a5306d064a8c` for copy-constructor/pair order.

Pair helper `0x12CC430` reads incoming control before backing and
increments incoming control `+4` before reading old destination control.
After conditional old-control callbacks, it stores captured incoming
control before saved backing. No explicit source/destination equality
test appears in this bounded body; no alias-safety conclusion follows.
Unlike the wrapper destructor's second XADD/DEC/JNZ, this helper's
second-counter decrement is followed directly by JNZ. Keep those
instruction sequences separate in captured projections.

The pair-order report's base-zero-equals-RVA wording is rejected:
the established flat IDB uses raw file EAs, while mapped PE RVAs differ.
Do not propagate that address-basis claim into code or documentation.

Initializer entry checks are diagnostic-only, not an idempotency guard:
global byte VA `0x1E4B095 == 0` reports `0x68`; receiver byte `+4 != 0`
reports `0x69`. If reporters return, allocation/publication continues even
on repeat calls. No interlocked instruction or lock is visible in the
bounded entry gates; external serialization remains unresolved. Entry-gate
capture support is assigned separately from the already-pushed continuation.

Copy-constructor/pair-helper immutable PE anchors are pushed at `786b56b`;
initializer entry diagnostic captures and all-byte matrix at `1bbcfd8`.
Copy code review found a fresh-destination path mismatch: its null-old-
control branch targets stores `0x12CC491/0x12CC495`, not callback-path
stores `0x12CC485/0x12CC489`. The copy agent is correcting those
uncommitted instruction anchors before registration and staging.

Copy and pair replacement projections are corrected, registered and
pushed at `83ce28e`, together with initializer entry PE gates. The pinned
sample runner passes. Copy uses direct stores `0x12CC491/0x12CC495`.

The nearest singleton teardown callback is raw `0x12CC1D0`, dispatched
through singleton vtable VA `0x1C80CA0` slot eight. It captures pool
and next link, advances the global list head, clears receiver `+C`
then `+10`, and drains/releases captured pool without directly clearing
receiver flag `+4`. Entry detach layout, later drain trace, source-copy
constructor projection, and a serialized PE writer are active.
The nearby `0x12CB830` method is source initialization with preserve
zero and backing-pointer-gated byte copying, not a proven alignment setter.

The bounded read-only drain trace reports no direct drain counter update.
After detaching, captured pool zero returns; a nonzero pool is popped through
`0x12CDAE0` with receiver `pool+8`. Each nonzero pop result is passed to
allocator backend `0x12C8D60`; an empty pop reaches the final tail release
of the captured pool. This establishes neither termination nor effects
inside either helper. A captured drain-step projection is assigned to
`35ccbcb1-d865-45a2-9245-cf02b88264ee`; immutable PE confirmation remains
with the current serialized evidence writer. Addresses here are raw file
offsets / flat IDB EAs, not PE RVAs.

Source initialization, singleton detach, and one-pop drain projections are
registered. Review corrected the constructor action sequence: requested-size
push `0x12CB890` and source push `0x12CB893` precede backing-data capture/push
at `0x12CB896`. The registered pinned-sample runner passes after that change.
Builds must be serialized: concurrent agent builds briefly locked the shared
Startup.Checks output; agents now leave final build/run to the coordinator.

Active next scopes are singleton list walker
(`d4da370d-ecde-417d-a96b-90c5c92489b1`), one-attempt tagged pop code
(`f6bc3de8-e947-496a-be73-2a63372b1738`), and teardown diagnostic PE
(`6d6af571-ce74-4918-bb1f-c54c7088ae90`, exclusive evidence-file writer).
The pop trace's reported VA `0x16DDAE0` was arithmetic error: raw
`0x12CDAE0` maps to code VA `0x16CE6E0`. Its pair-relative fields must
not be confused with the containing pool: the teardown passes `pool+8`.
