# Reverse-engineering handoff archive 12

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

The one-attempt tagged pop is reviewed and registered; pinned binary checks
pass with exact singleton teardown diagnostics, source constructor, walker,
and tagged pop windows. The reporter cell VA `0x1C15EA0` is NOT a PE
import: its mapped DWORD points to VA `0x16C37C0`, raw `0x12C2BC0`.
Returning diagnostic IDs `0x76/0x77/0x78` continue to detach.

The walker projection is not yet accepted: review found systematic raw/VA
conversion errors and unrelated entry bytes. Its author is correcting all
anchors against immutable raw `0x12C3AF0..0x12C3B3E`, VA
`0x16C46F0..0x16C473E`. Do not register its checks until corrected.
Other active scopes: teardown entry captures
`fe7189e8-85ac-42ae-aed1-0fddd47a2257`, push trace
`8200e808-cd08-4fff-bc28-4d7d9404d805`, and acquisition continuation
`57dca23a-a9a0-4649-aac9-3f70d64147a8`. The immutable evidence file
has no active writer after `db06904f-7a37-42a9-98a8-3e848a5c34f9`.

The walker author corrected all mapping errors and removed unrelated entry
bytes. Its raw `0x12C3AF0` entry and `0x12C3Bxx` instruction anchors
now match the immutable PE assertions; it is registered for integration.
Teardown entry diagnostic captures are pushed at `833ca7c`.

New active code scopes: tagged push
`fcfddf15-e1be-4e05-9cef-2f28401842ca`, successful-acquisition continuation
`c5a79a94-7b26-4fcc-a623-0753724be491`; exclusive PE writer
`6be31f83-b449-424d-9c1c-170bef4fc379` binds both. Adjacent singleton
callback identity/differences are read-only scope
`cc1189e2-11b8-422d-ba0e-4382bccf569a`.

Walker integration is pushed at `888de6c`; the pinned runner passes.
Adjacent raw `0x12CC280` is the same singleton vtable's slot-zero
deleting disposition, not a drain callback or new pool variant.
Its separate reconstruction is assigned to
`c49a0d04-2036-4a59-9f87-f7cd54c43141`, with PE assertions owned by
`8afc3aa0-7d68-4c63-81d9-28725d6f76ce`.
The helper starting `0x12CC2B0` has a distinct read-only drain trace
assigned to `50c24a1d-ecba-4ad8-94b7-8a068c6832fe`.

Acquisition review caught a hexadecimal diagnostic-ID mistake:
`68 54 02 00 00` pushes `0x254` (596 decimal), NOT decimal 254.
The acquisition author is correcting code/checks and adding explicit
receiver-relative counter/divisor target addresses before registration.
Push review also requires the diagnostic cell `0x1C15EA0` to be labeled
VA, never raw file offset. Its later node-link reread is an independent
capture, not automatically identical to the preceding attempted store.

Push, acquisition continuation, and singleton slot-zero disposition are
registered and the complete pinned-sample runner passes. Full push-window
validation caught a transcription error at raw `0x12CDAAF`: opcode `0B`
was mistakenly recorded as `8B`; corrected before integration.
The later node-link reread remains independently captured in the code.
Acquisition now uses `0x254` and receiver-relative wrapped targets.

The separate helper at raw `0x12CC2B0` drains `this+8`, calls the backend
for each popped item and finally `this`, then returns `this` with RET4.
Unlike the inline singleton drain, it has no null-receiver gate and no
tail transfer. Identity/vtable binding is assigned read-only to
`d68c81f5-e192-47a0-9852-8702f667df36`; no generalization is integrated yet.
Replenishment entry at raw `0x12CE0F0` is assigned read-only to
`2504b159-8bb3-4d39-82bd-25aeb40d431d`.

The preceding lifecycle integrations are pushed at `9a8ea5c`.
Replenishment entry raw `0x12CE0F0` maps to VA `0x16CECF0`; its
trace's `0x16CE1CF0` was another arithmetic typo and is rejected.
The bounded request is DWORD wrapping arithmetic
`((field14 + field1C) * field20) + field18 + 3`.
Request code/checks are assigned to
`536430c4-87ab-41bd-b41f-afd78e26a5df`; exclusive immutable PE writer
`bc3c7f42-927c-46fe-8c12-ea4e0219290b` binds entry and first push.
Post-allocation diagnostics through first secondary-stack push are
read-only scope `7c4a049a-3547-457a-9767-e0e0838f1276`.
Do not infer a clean allocation-failure return: the observed null-result
path reports `0xDA` then falls through to the push if its calls return.

Request arithmetic and immutable entry/result bytes are pushed at
`855aac7`; registered/pinned runner passes. Generic drain `0x12CC2B0`
has no direct pointer/caller binding found by the bounded full-file scans.
Its owning type remains unresolved; do not broaden that scan without new
evidence. The known singleton table instead has RTTI
`.?AV?$slSingleton@V?$slByteBucketPool@$0IAA@$0CA@@@@@` and slots
`0x12CC280/0x12CC110/0x12CC1D0`.

Current code assignment is replenishment result continuation
`33d8ad5a-314b-4981-9e48-9c4000a7c83a`. Read-only followups:
base alignment `8ccfebb6-d355-48ae-bfc2-4d30d8959599` and single entry
iteration `f138e29e-eef3-488d-a2a8-84809ad44685`. The evidence file
currently has no writer. Preserve the true sample hash above: one result
report transcribed it incorrectly; raw `0x12CE11E` maps to VA
`0x16CED1E`, not that report's `0x16DCD1E`.

Base-alignment trace confirms one captured divisor from pool `+0x18`,
wrapping block-plus-four, conditional round-up by `D-remainder`, and a
second remainder check with diagnostic `0x2BA` (698). Code is assigned
to `22718043-17af-45fa-9df2-a7365bb1510e`; exclusive PE writer
`4c9f4ccf-4912-4d97-91e6-abcfc84b8c80` binds that block.
The entered per-entry loop separately reads fields `+0x14/+0x1C/+0x18`,
pushes to the primary pair, increments its index, and rereads bound `+0x20`.
Its one-iteration code is assigned to
`88824771-7e52-452b-9cb1-4e7b3c609115`.
Pool configuration `0x12CDB40` field writes are the read-only next trace
`e35aa672-0ad8-4cbd-849c-ff45db668293`. No agents run shared builds;
the coordinator serializes registration and pinned-sample validation.

Pool configuration trace confirms `+0x14=size`, `+0x18=alignment`,
`+0x20=count`, and `+0x1C=padding` computed with wrapping DWORD
`((size-1+alignment)&~(alignment-1))-size`. Code is assigned to
`fdd16d57-9b2d-4873-a5ad-743a7095fe21`; config/iteration PE writer
`7d081a1d-7606-4707-ae50-97743f8de933` now owns the evidence file.
Other padding consumers are read-only scope
`54915460-32b6-4b8b-b7bf-7c58e00d054b`.

Result-continuation review found several one-byte anchor errors despite
mirrored checks: formatter call is raw `0x12CE13F`, ID push `0x12CE149`,
reporter `0x12CE158`, and pool-plus-eight LEA `0x12CE162`.
The author is correcting those and adding byte-slice checks before
registration. Base-alignment terminology must call field `+0x18` a
divisor/alignment, not an element size.

Pre-integration review also caught a wrong alignment test: DWORD max is
divisible by three, so the intended wrapping diagnostic counterexample
must use divisor seven (remainder three, adjustment four, wrapped candidate
three). The alignment author is correcting this and adding instruction
byte-slice assertions. The iteration author is separating the later base
reload from the earlier base capture, and making later base/bound captures
required only on reached post-push continuation. Neither scope is registered
yet. Result-continuation anchor corrections are likewise still pending.

A separate bounded reset/accounting trace near pop caller `0x12CDD55`
is assigned to `05be7256-0a53-4c7a-b936-12bef6ed1c69`.
Latest pushed code remains `855aac7`; active agents' new files and current
handoff/evidence changes are intentionally not staged until reviewed.

Result and base-alignment corrections are reviewed, registered, and the
standard runner passes. Config/iteration PE review found two mapping typos
(`0x12CDB40` maps to `0x16CE740`; `0x12CE1AD` to `0x16CEDAD`)
and windows ending inside instructions. The evidence author is correcting
the full RET4 config end and half-open loop boundary before pinned validation.

The corrected config return is RET `0x0C`, not RET4; its full function
is raw `[0x12CDB40,0x12CDC1E)`. Corrected config/loop/base-alignment
PE checks and registered iteration code pass the pinned runner.
Result/base-alignment code is pushed at `139243d`.

Configuration code review requires independent field captures across
reporting calls (later count, alignment checks, and padding size/alignment
reads); the author is correcting this before registration.
Reset `0x12CDC50` supplies the nearest other stride consumer and is split:
entry gate code `7241208e-7ce8-4b92-a830-7436861ea6be`,
candidate membership/callback code `3a675c98-1f00-4cc0-8262-da2ab5d8c18d`,
finalization trace `f1c4a86f-b092-4e98-b251-9caf1a669665`.
Exclusive PE writer `1c5dd201-df95-44cc-959a-9c60cd174adc` owns entry/
callback assertions and preserves the reviewed config/loop additions.
Actual callback instruction is raw `0x12CDDD4`, not the earlier
stride report's `0x12CDDD3`.

The entered iteration code is pushed at `ee95118`; complete registered
and pinned checks passed immediately before the commit. Additional caller
evidence confirms pool config triples `(12,8,1)` at raw `0x12C0254`
with receiver `this+0x20`, and `(32,128,16)` at `0x12B9F95` with
global receiver VA `0x1F113D8`. Shared text-pool singleton/accessor boundary
is assigned read-only to `ec0e6602-3cd6-4eb3-85fb-9739e3de4d34`.

Configuration code now separates entry args from later field reads and is
registered. Its validation is temporarily blocked by a concurrently written
reset-callback check syntax error, assigned back to that file's author.
No unrelated files were changed to bypass the shared compile.
Reset captured-pointer-array setup is the read-only next scope
`060e2f95-a301-438e-b9ef-c58594f4f533`.
Replenishment normal epilogue is bounded: both zero-bound skip and loop
exhaustion reach raw `0x12CE1F5`, direct cookie call `0x12CE1FD`
to `0x136AC96`, then RET at `0x12CE205` if the check returns.
No normalized replenishment success result is established.

Configuration and reset-entry code are reviewed, registered, and standard
runner passes; pinned checks also passed the corrected config/loop/reset
entry/callback PE windows. Reset finalization code is assigned to
`2da97b87-5d7d-43c9-96b6-153b7cc2e27b`; exclusive PE writer
`7d71eda9-f393-45b1-9785-4b3462a75925` preserves current additions and
binds final counter clear, conditional temporary-buffer release and return.
The normal reset return is `0x12CDE2E..0x12CDE43`; `0x12CDE54`
is inside a separate backedge, not an epilogue.
Reset callback projection awaits only corrected size/padding anchor labels
and byte-slice checks before registration.

Configuration/reset-entry code is pushed at `c6bc62c`.
Reset temporary primary-pop collection is a DWORD buffer descriptor
`begin/current-end/storage-end`, not a sorted pointer array in the bounded
setup. Append/growth boundary code is assigned to
`b14b936c-0ef0-4881-8bf7-9c79fc8f0760`.
Shared text-pool singleton getter `0x12B9F70` sets the ordinary guard low
bit before configuring global VA `0x1F113D8` with `(32,128,16)`.
Both it and guard VA `0x1F113D0` lie in zero-filled virtual data with no
file-backed global bytes. Captured getter code is assigned to
`71b6acc7-4b2b-4b5f-aa7a-cb378b4d5880`; no atomic-once/readiness claim
is established.

Pre-commit pinned validation found a failing reset-finalization PE assertion;
the exclusive evidence writer is comparing its literal/targets against the
sample before integration. Other pending windows pass. Callback check review
also corrected raw `0x12CDDB2` mapping to VA `0x16CE9B2` and a scan
slice length mismatch; its author is finishing that follow-up.
Reset secondary-block alignment is a distinct read-only scope
`e115f0f8-9e99-4fa4-8959-7c0362587b70`; do not reuse replenishment
anchors for that reset block without exact evidence.

The finalization PE mismatch was `85 09` versus actual `85 C9`
(`test ecx,ecx`) at raw `0x12CDE25`, not a branch-target error.
The evidence writer corrected it and is still adding singleton/append windows.
Raw helper `0x6A5500` is DWORD capacity selection: descriptor spare space,
used count and capacity use wrapped subtraction followed by arithmetic
right shift two; unsigned comparisons govern the branches. Tail transfer is
to raw `0x6A5900`, not a sorting operation. Bounded code/check reconstruction
is assigned to `1a88b91f-46e8-4925-94f0-f3a83ef159e1`, preserving an
explicit unresolved indirect-call continuation boundary.

Coordinator independently matched callback's 62 bytes, finalization's 67
bytes, and all 20 singleton getter instructions against the pinned PE.
Singleton raw `0x12B9F70` maps to VA `0x16BAB70`; both source checks
and PE assertions needed independently transcribed VA corrections.
The matched-candidate branch copies the last DWORD over the match and
shrinks saved end by four, then skips callback/count increment. It does
not preserve order. Code is assigned to
`866951b4-bfa5-4c6a-8b48-68bf63148bd1`.
New alignment/growth PE assertions are exclusively assigned to
`f8e951f9-3345-4c1b-becc-2532ea73028e`.

Raw `0x6A5900` storage replacement is not yet implemented: allocation is
requested count times four (limit `0x3FFFFFFF`), then opaque copy arguments
use descriptor reads after allocation; retained count uses separate reads
after copy. Conditional old-begin release precedes ordered publication
of begin, storage end, and end. Keep those cross-call reads independent.
The current growth author must finish compile fixes before another
serialized coordinator build; newly written incomplete check files have
temporarily blocked otherwise ready callback/finalization integration.

Pushed reviewed code batch `5bd7fc5` after serialized pinned validation:
reset callback, reset finalization, reset alignment, sharedtext getter, and
observed configuration caller cases. Append and growth checks are now also
registered and pass with the pinned PE. The append compare is raw
`0x12CDCFD`; `0x12CDCFF` is its branch, not the compare.
Growth caller constants from unrelated raw `0x12CE575/0x12CE59B` are
being removed before commit; append's own exact callsites are
`0x12CDD06/0x12CDD28`. Remaining storage implementation is assigned to
`3ddb246e-0de9-49b6-a2ee-72b0607dd814`.

Pushed append/growth/removal code at `a0c1d47`; registered checks plus
pending immutable PE assertions pass. Storage/wrapper PE writer
`c0124069-f05c-494e-a5de-22802f7154fc` owns the evidence file.
Static reset secondary gate at raw `0x12CDD4D` zeroes EDI and local
callback count, pops receiver+8, and on zero branches to `0x12CDE04`,
skipping the count load at `0x12CDE01`. Do not normalize these two
finalization entry edges. Gate projection is assigned to
`c2577844-95d7-4d7f-bfc1-4d783000e289`.
Stack allocation wrapper raw `0x12B7FB0` forwards requested bytes to
`0x12C8A20`, preserves its result in ESI across returning OOM diagnostics
`0xDA`, and restores zero EAX before the common cookie/RET path.
Wrapper code is assigned to `8e570df0-e0d6-455e-986f-8050e0b653f5`.

The primary repeat-pop bytes omitted by IDA's current function enumeration
remain readable without redefining code: raw `0x12CDD39` has
`8B4DE483C6048975D0E899FDFFFF8BF885FF7596`.
Repeat call `0x12CDD42` targets pop `0x12CDAE0`, and JNZ
`0x12CDD4B` targets append entry `0x12CDCE3`. Initial/repeat primary
gate code is assigned to `b8983307-26ba-437e-8713-a299399860d8`.

One concrete reset caller raw `0x12C02F0` passes receiver this+`0x20`
and callback VA `0x16C1520` at raw `0x12C0340`. That callback maps
to raw `0x12C0920`: one cdecl stack argument, test its first DWORD,
null skips all clears; nonnull calls opaque backend `0x12C8D60`, then
clears offsets 0/4/8 only if it returns. IDA's inferred multiargument
prototype is not evidence. Callback projection is assigned to
`c0cef6fb-3e75-4454-b998-5eae06071d57`.

Storage replacement code is reviewed and registered; serialized pinned checks
pass with its separate pre-copy/post-copy reads and opaque failure boundaries.
Iteration-tail implementation is assigned to
`bd460456-0ba9-4f47-8bf0-15885f10b8ed`: callback return at
`0x12CDDD7` reloads begin, cleans the argument, increments callback count,
then item index. Removal instead enters at `0x12CDDE0`, increments only
item index, and uses the fresh unsigned bound comparison. Keep these
entry modes distinct.

Storage code is pushed at `828539e`. Secondary initial gate is registered
and standard checks pass. Separate secondary release/repeat projection is
assigned to `2ab8156d-8a95-4a9e-86de-6449800697b4`: opaque release at
`0x12CDDE9`, next pop at `0x12CDDF1`, nonzero backedge to alignment,
zero fallthrough to `0x12CDE01` (unlike initial zero at `0x12CDE04`).
Queue/sentinel return helper raw `0x12CE280` is a new bounded trace,
assigned to `2794ccfa-9896-4b51-a571-9f75242dc4c2`; it is not the
singleton detach/drain.

Primary initial/repeat gate and concrete cleanup callback are now registered.
Coordinator corrected the primary check's pop code VA to `0x16CE6E0`
(raw `0x12CDAE0` plus `0x400C00`). Serialized registered/pinned checks
pass with primary and secondary gates, concrete callback, and storage
replacement. Immutable evidence remains uncommitted only while its sole
writer finishes the final gate/callback windows.

Code gate/callback batch is pushed at `22a0a7b`. The evidence writer finished
all reset/storage/wrapper additions, but the next serialized build was
temporarily blocked by an incomplete secondary-repeat check. Queue PE writer
`5e904995-cc62-470d-b496-78cb90799f4e` now owns that file and preserves
all pending windows. Do not stage it during that writer's active work.
Wrapper review requested corrected reporter/cleanup offsets (`0x12B8006`,
`0x12B800C`) and unreached-return validation; tail review requested corrected
callback increment offset `0x12CDDDD`.

Queue cleanup raw `0x12CE280` reads receiver+8 and receiver+0 separately,
reports `0x2D6` on mismatch, then LFENCE/later+10 reports `0x2D7`.
It later captures receiver+0 and global pool VA `0x1E003BC` before
null-node diagnostic `0x25A`; returning diagnostics can continue to
pool+10 XADD -1 and tagged push. Queue pop raw `0x12CE400` is a
separate sentinel-head CAS operation, not the pool pop `0x12CDAE0`.
Queue projection assignments:
`fab06ced-9924-426b-be2a-7fb923f5f6ae` cleanup;
`827dcf37-2460-4193-900b-9b2a230b4b5e` pop-attempt trace;
`1a24fbb4-5bab-491c-935d-925b2d416864` successful pop continuation.

Pushed reset tail/secondary-repeat/allocation wrapper code at `03aa6e8`
after registered and pinned checks passed. Secondary repeat address API
must say code VA, not IDA EA; the flat IDB EA remains the raw file offset.
One-attempt queue pop implementation is assigned to
`acf328bb-462a-4731-b349-e1a5d99baf27`; raw `0x12CE400`
maps to code VA `0x16CF000`, not the prior trace's transcribed value.
Queue constructor/global-pool setup trace is assigned to
`ac9be7d6-1c96-4462-8c8b-9cbcef3248db`.

Queue append raw `0x12CE310` is being traced independently by
`ecd10413-044e-49f1-914d-77907f6a93b9`. Its visible success path
publishes the payload in old tail+4 before the new-node link in old tail+0;
do not infer these stores occur before the tail-pair CAS. Constructor and
append scopes are separate from sentinel cleanup and pop.

Successful queue-pop continuation is reviewed, registered, and passes pinned
checks. A separately reloaded old head supplies payload before recycling;
receiver+10 and captured global-pool+10 each receive XADD -1 independently.
Cleanup's surrounding empty/retry attempt at raw `0x12C0348..0x12C0382`
is assigned to `66a7c789-a164-4aa0-849e-e38519bb9583`; it compares
a fresh tag only after reading head-next, and ignores queue-pop return.

Queue-pop completion code is pushed at `e7603fc`. A further focused check
now covers equal captured counter addresses without replacing the later
counter capture by the earlier arithmetic result. Standard validation is
temporarily blocked by the active sentinel-return author's unfinished
constant references; that author has been notified. Embedded pool drain
binding in raw `0x12C0382..0x12C03AF` is assigned as a read-only scope
to `ae98f90d-dcdd-47a7-a8df-0f8cac6dbfa5`.

All reviewed reset/storage/wrapper/queue cleanup/pop immutable PE additions
are pushed at `e3bac11`, including the independent-counter-address check.
Queue initializer projection is assigned to
`e3f1e1dd-16a5-46fb-a92b-bdd0ed583087`; one tail-CAS append attempt to
`07d6a4ff-e7a0-4f15-9b99-fc1b6ac6d9aa`; new initializer/append/cleanup
predicate PE assertions exclusively to
`e7c322e8-09f4-4ea1-8d63-10a72299fba4`.
The constructor configures the embedded pool this+`0x20` as `(12,8,1)`;
the sentinel pool's narrow initializer uses `(8,32,8)`. These are distinct
pool configurations and must not be conflated.

Sentinel-return review found its first implementation inferred plain return
after an opaque push without a captured normal return and omitted counter
arithmetic. The same author is correcting those before registration.
Append publication raw `0x12CE3C3..0x12CE3F7` is assigned to
`c2132f16-743b-49f5-9984-d5973cc8a282`: a later next-field diagnostic
`0x2F0` precedes payload-then-link stores; no full-append atomicity is claimed.

Queue pop-attempt is reviewed and registered. Finalization now explicitly
distinguishes initial-secondary-empty from post-drain entry and rejects
nonzero retained EDI on the former; a composed gate/finalization check
preserves the `0x12CDE04` edge. The next coordinator validation was blocked
only by an active append check's int-versus-uint Contains literal, reported
to its author.
Embedded-pool drain implementation is assigned to
`af15500b-2c9d-4965-a7f9-31963151fc39`; it preserves saved embedded-pool
and saved queue pointers independently.

Coordinator read-only PE import-name resolution identifies:
cell VA `0x1AC4424` = `?_Xbad_alloc@std@@YAXXZ`,
`0x1AC45C0` = `?_Xlength_error@std@@YAXPBD@Z`,
`0x1AC489C` = `memmove`, `0x1AC4870` = `_snprintf`.
The active evidence writer is adding exact-name assertions. Keep imported
identity distinct from captured runtime outcomes and do not silently
remove conservative call boundaries.

Coordinator independently matched pop-attempt 117 bytes, cleanup predicate
58 bytes, and append-attempt 95 bytes to the pinned PE. Append's VA literals
were wrong in both source and mirrored checks; corrected raw `0x12CE364`
to VA `0x16CEF64`, end `0x16CEFC3`, and derive source constants from
the mapping rather than transcribing them.

Additional reviewed queue code now registered: successful-CAS publication
(`0x12CE3C3`, 52 bytes), sentinel return (`0x12CE280`, 138 bytes), and
embedded drain (`0x12C0382`, 45 bytes). All embedded byte captures were
independently matched to the SHA-pinned sample. Drain initially rejected a
valid cleanup-return capture before discovering the next pop was zero;
author corrected this and added regressions. Mirrored pop/cleanup VAs were
also corrected to `0x16CE6E0` and `0x16CEE80`.

Initializer and acquisition code remain under review, not registered yet.
Initializer author is fixing compile errors and mixed raw/VA API naming.
Acquisition author is fixing an extra hex digit in VAs and unconditional
unreached-capture rejection that prevented missing-prefix boundaries.
Only coordinator runs the checks. A targeted DefaultItemExcludes override
was not useful: it also removed default obj/bin exclusions, causing duplicate
generated attributes. Use the ordinary runner once initializer checks are coherent.

Successor static traces, explicitly Luna: `e2575825-ed8e-4453-8fa2-00f05b910978`
for global sentinel initializer `0xBBFAC0`, and
`7d5c72ac-5ad3-46db-8258-f1223b6e6a15` for destructor continuation after
`0x12C03AF`. Evidence writer `e7c322e8-09f4-4ea1-8d63-10a72299fba4`
again exclusively owns `StartupBinaryEvidence.cs` to bind embedded drain.

Pushed `11c223e`: reset finalization now distinguishes the initial-empty
secondary edge retaining zero EDI from post-drain count loading. The shared
runner compiled and exercised this correction, then exposed three queue-pop
attempt assertions and an append-attempt low-mismatch fixture supplying an
unreached high-half capture. Authors are correcting these before the queue
batch is committed. Initializer/acquisition reviews are complete and checks
registered after their fixes; no exclusion override is needed.

The ordinary pinned runner now passes with all eight queue projections
registered: pop attempt, cleanup predicate, append attempt, append publication,
sentinel return, embedded drain, sentinel initialization, and append acquisition.
Further fixture corrections retained the explicit-half comparisons and missing
capture contracts, rather than weakening them. Verified helper VAs:
pop `0x16CE6E0`, release `0x16C9960`, queue cleanup `0x16CEE80`.
Global singleton initialization and ordinary destructor finalization are
successor code assignments; a fresh evidence agent exclusively owns the PE
authority for the final 27-byte destructor tail.

Pushed `7ad8401`: eight queue projections and focused checks plus their
documentation are persistent on the feature branch. The PE evidence file
remains unstaged while the final-tail evidence writer finishes. Caller trace
found distinct contracts: fragment enqueue uses receiver `this+0x38` and
forwards the append result; received/status queue uses `this+0x260` and
tests small integer pop results; owner callback queues use independently
loaded owners plus `0x60` and pointer-like callback/refcount operations.
Do not conflate these payload contracts.

New Luna code scopes: fragment wrapper
`76f5e272-dab3-4380-a912-38a5dca06e23`; received-queue pop gate
`9066607d-b4a2-4e34-81ef-f6d8e70d14ae`. Global initializer code
`e6eebd15-10d8-4ecc-9396-92fd3a7cf05c` and destructor finalization code
`2c2fbcd5-3a74-4215-81f7-3b52d200ffca` remain active. Evidence exclusive
writer is `3bdbc6f7-fd23-4c03-8563-a3271c6bba6a`.

Pushed `3e88041`: immutable queue lifecycle/drain/import-name evidence and
ordinary destructor finalization are persistent. Registered successor global
initializer (128-byte slice) and received-queue pop gate (26 bytes); both full
byte captures independently match the pinned PE and their behavioral checks
pass. The initializer author is correcting an individual instruction assertion;
new initializer PE assertions remain under exclusive writer
`bb247a66-2730-4086-888e-c3c0e2a71f8d` and are not yet ready.

First-owner callback delivery trace assigned Luna
`548427d4-5e04-4193-99e6-d892c867a1ef`, bounded to the first queue around
pop `0xBD5239`, not the second owner queue.

Global initializer checks now pass after correcting two mirrored helper-VA
literals, allocation `0x16B8BB0` and configuration `0x16CE740`; all 36
instruction anchors and the full 128-byte slice were already correct.
Received-queue pop gate and new immutable initializer authority pass the
ordinary pinned runner. Fragment wrapper review found its diagnostic return
was a nonnullable bool conflating skipped, unavailable, and nonreturn paths;
author is adding nullable capture and explicit outcome boundaries.

Pushed `ab649bf`: global initializer publication, received/status queue gate,
and immutable global-initializer evidence. Socket caller PE authority is now
exclusively assigned to Luna `f4bc6001-515e-4df2-a33e-fbe76d48258a`.
Context payload producer trace is Luna
`862e8b27-c3e8-46b6-8702-40d23398a0e3`, bounded before append `0xBD346E`;
do not overlap it with callback delivery or fragment enqueue work.

Fragment wrapper is reviewed and registered: 74 bytes independently match
the PE; nullable diagnostic and append returns now preserve missing/nonreturn/
unreached distinctions. Corrected its mirrored append target VA to `0x16CEF10`.
The ordinary pinned runner passes. Caller PE authority remains with
`f4bc6001-515e-4df2-a33e-fbe76d48258a`.

Trace successors implemented by Luna:
`7acba39f-de70-473c-b203-f70e781ad44f` owns first-owner callback continuation,
preserving an independently reloaded payload after the callback;
`5d640a2c-fec8-4166-9283-ae0856abac8a` owns context append preparation,
where payload+4 is a modulo-65536 cursor offset, not a length.
Neither scope includes an entire parser or retry loop.

Pushed `fb8a115`: fragment wrapper reconstruction and checks. Socket caller
PE writer finished; coordinator removed one spurious zero byte from the
mask instruction assertion and added actual mapping predicates to labels
that previously only claimed a mapping. All nine new literal byte assertions
independently match the pinned PE. Full runner is temporarily blocked by
in-progress owner-callback mixed generic capture arguments; author notified.

Additional bounded static trace Luna
`aba1711c-0f5a-420f-97f0-c54e00223c00` covers the first branch at
`0xBD40BF` for unsigned queue values at least 16, separate from small status
cases assigned `a8a0a4bf-2db2-473f-ac39-f231952d61c5`.

Context append preparation is reviewed/registered; 33 captured bytes match
the sample and the pinned runner passes. Socket caller immutable assertions
also pass after the shared callback file became compilable. Evidence is now
exclusively assigned `ab0b788a-9380-472b-b7f6-73f85ffa1d43` for context
and first-owner callback windows.

Small-status implementation assigned Luna
`26530961-d8b5-4a2f-a1f8-1ffa7e930ee7`. Important trace correction:
case 3 diagnoses if predicate AL is zero **or** the independently read word
is zero; AL zero branches directly to reporting, so that word is unreached.
Do not copy the trace narrative's erroneous AND condition.

Pushed `84a9da2`: context append preparation is persistent. New context/
callback PE assertions reviewed; context label claimed 33 bytes while its
literal contained only 29, now extended through the four-byte following
compare. All literal byte anchors independently match the sample.
Callback code review caught inverted mapping: author had treated IDA raw
`0xBD523E` as a VA and subtracted the mapping delta. Correct raw is
`0xBD523E`, VA `0xFD5E3E`; author is fixing all constants/checks plus
an unmatched parenthesis. Not registered or staged yet.

Deferred-control callback trace Luna
`8f589b01-67dd-4ffe-858b-4ce4df9e81de` is bounded to raw
`0xBD37D8` through the first returned-byte branch, no handler-wide scan.

Owner callback mapping/syntax corrected, reviewed and registered. Raw
`[0xBD523E,0xBD5277)` is projected; next seven-byte compare is evidence
only. Source VAs now derive from raw offsets. Pinned runner passes including
callback replacement-to-null and old-counter 0/1/2 paths, plus all pending
socket/context/callback immutable evidence. Deferred-control callback code
is assigned Luna `bb5847fc-cbb8-4e40-81f2-da79bfdad3a8`.

Pushed `a8826c2`: first-owner callback code and all reviewed socket/context/
callback PE assertions. Current exclusive PE writer Luna
`e6df34f4-4db3-4bfb-a1d2-33a9c84e7bd6` binds small-status and large-value
entry gates. Additional bounded recycle-wrapper trace Luna
`2967a9c9-1d8e-4568-a6bc-f3ee1911134f` covers `0xBD5C60`, stopping at the
already reconstructed pool-push helper.

Small-status and large-value gates reviewed/registered; ordinary pinned
runner passes. Coordinator corrected four mirrored small-status VA literals,
and author corrected the AL-zero JE anchor from the TEST at `0xBD3EE6`
to the actual branch at `0xBD3EE8`. Large-value word test is nonzero
`word & 0xC000`, not equality to `0xC000`; documented explicitly.
Small-status full 93-byte capture independently matches the PE.

Pushed `9089c0f`: small-status and large-value entry gates. All eleven
large-gate instruction windows independently match the sample, including
nonzero top-bit TEST bytes. Header-dispatch table trace is assigned Luna
`3f1e40c4-4eb3-4a2b-bff7-15a5dd836f43`, bounded to WORD>>14 table and
first opaque case handoffs only.

Deferred callback is reviewed/registered; byte-value sweep passes, but author
is fixing three missing-capture fixtures whose helper defaults accidentally
supplied later values. Predicate code is Luna
`fcddd56b-72b7-428e-813b-200f642896d5`. Predicate trace correction:
the first pair `this+8 | this+0x0C` short-circuits on nonzero, skipping
`this+0x10 | this+0x14`; preserve unreached captures. Zero status returns
AL zero without clearing upper EAX. New exclusive PE writer
`b3739e21-1d39-4d03-b929-7e5b1bad446b` binds predicate and deferred callback.

Callback fixture correction is complete and the ordinary pinned PE runner
passes with callback registration. Small/large status immutable additions
also passed; the exclusive PE writer still owns that file for the next pair
of anchors, so it is intentionally excluded from the callback commit.
Luna successors: `ae2c5fd8-37dc-43f9-bcc4-11166cc48dcf` owns header dispatch
production/check files, bounded at the first case call; table VA `0xFD4E2C`
maps to raw `0xBD422C`, four targets `BD4141/BD415A/BD418D/BD41C0`.
`1eeac3d1-4b5f-417b-b5b4-333d4c08bf6e` owns recycle-tail files,
starting at the late node+8 write and stopping at wrapper return.

Pushed `aabddd1` with callback code/checks/registration. New immutable
status/predicate/callback windows independently match the pinned PE
(37 literal windows); final runner is waiting for active header writer
to finish a tuple typing correction. State setter trace shows equality
does not early-return: raw `BD1BA3` JNZ skips only the report block;
equal state reaches the indirect reporter at `BD1BB9`.
State write `BD2035` remains beyond the entry boundary and is not assumed
reachable for requested state five. Luna `6f163dbf-5770-49ad-af46-f5008c82f3ea`
owns the bounded state-entry production/check pair.

Header dispatch is reviewed and registered; every instruction/table window
independently matches the PE. Active code writers are still compiling into
the default SDK glob, so defer the next serialized runner until they report
ready. Predicate author is correcting sequential missing-read validation:
`+C` cannot be captured without `+8`, nor `+14` without `+10`.
Header/state immutable writer is now
`4ee9f9e2-df4d-4427-a5e8-c8d2b1901faf`; recycle-prefix read-only trace is
`539db0c1-58d6-4c02-bad1-e7b2ac8f0487`, stopping before the tail writer's
`BD5C9C` entry. Avoid committing those active files until reviewed.

Predicate sequential-read revision and bounded state entry are reviewed and
registered, as is header dispatch. Their code/check files are ready for a
serialized runner after recycle writers finish. The 46 pending immutable
literal windows independently match the pinned PE. Recycle tail reviewer
found that missing receiver/counter captures still allowed helper/epilogue;
author is splitting those boundaries and rejecting later captures.
Luna successors: `6903d850-7cab-449c-a2ac-3cffc8d8cd8b` owns recycle-prefix
code/checks, `bdb5f485-f8d8-478a-b587-13b135d020bb` traces header helper
first handoffs, and `72dff284-e9f6-48df-93bf-32ce32b0fa3d` exclusively owns
the PE evidence file for recycle wrapper windows.

The ordinary pinned runner now passes with header dispatch, predicate,
state entry and recycle tail registered. Prefix files are not registered
yet and remain owned by `6903d850`; do not include them in this commit.
All 59 new immutable literal windows independently matched the PE.
Header helper trace found `BD5BA0` and `BD5C00` are identical plain-RET
getters, not RET-4 queue consumers. Their final `[ESI]` reload is independent
even when the initial nonzero comparison skips diagnostics. Getter code
belongs to Luna `86e26c28-2cc4-4ba8-8d8d-f7ea3688e907`; caller continuation
trace belongs to `1489fb32-5beb-4925-b96e-4042e9121b6e`. The pushed EDI
must not be assumed consumed by those getters.

Pushed `fed2c97` with reviewed header dispatch, state predicate, state entry,
recycle tail, registrations and PE anchors. Recycle-prefix review found
missing vtable captures could still permit later call returns, and two
slot-4 fixtures selected the earlier skip branch; author is correcting
these before registration. Getter immutable anchors are assigned to
`516c98da-bd64-4e07-8c0d-03194e8b7515`, sole evidence writer.

Recycle prefix sequential gates and invalid-path fixtures are corrected,
reviewed, registered and pass the pinned runner. Getter exact body length
is **35 bytes**, ending raw `BD5BC3` / `BD5C23` exclusive; independent
derivation corrected the initially suggested 36-byte bound before review.
Caller trace confirms getter EAX is copied to ECX for the next direct call
(`BE51F0` cases 1/2, `BE5A40` case 3); the earlier pushed EDI remains on
the stack. Luna `d22f22b0-121c-4239-82b4-e1f4c172be98` owns the bounded
post-getter call/AL-branch projection and checks.

Pushed `fcdb0d4` for the recycle prefix. Both 35-byte getter bodies plus
their evidence-only trailing CC independently match the sample; getter
registration and the ordinary pinned runner pass. Luna
`e626e9f0-313b-4a4b-97a8-c61ac4e254a0` now traces only `BE51F0`'s initial
guard/first opaque handoff. No broad parser expansion is assigned.

Pushed `eeffa75` for the getter pair and immutable anchors. Reliable receive
entry trace found byte receiver+C diagnostic followed by an independent
byte receiver+1C early-return gate. The early return clears AL only, not
full EAX, and uses RET 4. Luna `d92b68c2-8daf-4482-ab9f-d4f933246bee` owns
only that entry projection/checks, stopping before argument load `BE5227`.
Luna `6d4ba6c7-5da1-4fd7-b965-49918b2ba883` owns immutable PE anchors for
that entry and the post-getter case continuation. Existing continuation
code owner remains `d22f22b0-121c-4239-82b4-e1f4c172be98`.

Post-getter continuation is reviewed, registered, and passes the ordinary
pinned runner, including all 256 AL values on all three routes. Its immutable
anchors remain with the active evidence writer. Luna
`eb01b0b6-4311-4e57-9baf-0869ee389c02` traces only case-three receiver
entry `BE5A40`'s initial guards and first opaque handoff.

Pushed `0866de0` for post-getter routing. Ten new literal windows for its
immutable anchors and reliable entry independently match the sample.
Case-three entry code now belongs to Luna
`408ec12e-7bf9-4915-800c-6958d14b229d`, bounded only through its diagnostic
guard at `BE5A6B`; no sequence comparison/parser reconstruction yet.
Its immutable evidence writer is `3cc18dd5-03c0-4680-9fb6-a6820314f917`,
preserving pending continuation/reliable anchors. Reliable-entry code owner
remains `d92b68c2-8daf-4482-ab9f-d4f933246bee`, not yet reviewed.

Reliable entry review requests removal of unrelated diagnostic-data prefix
claims and an explicit invalid-later-capture outcome when the diagnostic
return is missing but the second flag is supplied. The author is revising.
Luna `53ac1b08-e3a6-477f-9795-cb85aaa793ce` traces only the next reliable
sequence gate `[BE5227,BE526C]`, stopping at the first opaque call or its
alternate JA boundary; no parser/callee expansion.

Reliable entry revision and unreliable entry are reviewed, registered, and
pass the ordinary pinned runner. Their exact 55-byte/43-byte windows
independently match the PE. New Luna modular delta code writer is
`e9260f1f-ea39-44ae-a548-3c445fa48bec`; immutable evidence writer is
`58843c58-b5e8-4c8e-a651-115da5609ff5`. Both stop at the `BE524D` JA:
`d=(payloadWord-receiverWord) mod 65536`, compare
`(d-0x100) mod 65536` unsigned against `0x7EFF`; fallthrough iff
`0x100 <= d <= 0x7FFF`. No call-stack interpretation from the trace is
integrated; the trace's stack-slot table was not relied on.

Pushed `0c0935f` for both receive entry gates. Reliable modular gate is
reviewed/registered and its exhaustive 65,536-residue checks plus independent
interval predicate pass; corrected wrap fixture receiver base from
`FFFFFFD5` to `FFFFFFD6` so +`2A` is actually zero.
Case-three arithmetic trace corrected the earlier width claim: it masks
DWORD EAX to `FFFF`, compares DWORD EAX with `8000`, then uses signed JGE
at `BE5A84` to `BE5AC2`. Masked range makes this equivalent to residue
at least `8000`; fallthrough is `BE5A86`. Luna code owner is
`f8bf2a36-6be5-4548-b62b-7d68d14e54e6`; PE owner is
`cdd2ca10-07ba-47e4-85b0-06959e4ec581`, preserving all pending entry/gate
anchors. Latest ordinary pinned runner passes.

Pushed `bffc19a` for reliable delta code. Case-three delta is now reviewed,
registered and passes exhaustive 65,536 residues including signed/DWORD
comparison boundaries. Full exact delta window independently matches PE.
All pending continuation/receive-entry/delta immutable anchors are reviewed
and pass the pinned runner; exclusive evidence ownership is released.
Luna `e30a7b4a-2904-4c60-b787-41573107accd` owns the next bounded helper
projection at `BE2F50`: null arg0 skips its signed threshold compare but
still forwards; nonnull arg0 and signed arg1 below [arg0+4] returns without
setting EAX. Forwarding passes arg2 and the stack-tail address, not a parsed
format. Raw `BE526C` maps to VA `FE5E6C`; ignore the trace typo `FE62EC`.

Pushed `a955e53` with case-three delta and accumulated receive PE authority.
Forwarding review found unnecessary bool+nullable arg2 allowed a missing
DWORD to masquerade as captured null; author is simplifying to nullable
DWORD, with native zero represented as `0`, and fixing signed fixtures
whose argument0/argument1 positions were swapped.
Reliable alternate trace corrected its gate interval typo (`>7FFF`, not
`>=8100`). New code owner `553ca362-8a01-45a1-8dec-3c953bdb5834` models
only `[BE528F,BE52A5)`: push existing ECX payload-word address, hand off with
ECX receiver+47C to `BE6300`, then on normal return compare preserved BX
with AX=8000 and JB to `BE52DF`. Evidence owner is
`e2e60009-f44d-4d36-9746-57fe19de7613`, preserving forwarding anchors.

Forwarding arg2 simplification and signed fixtures are corrected, reviewed,
registered and pass the pinned runner. The helper's 35-byte slice and both
caller cleanup slices independently match PE. Active read-only helper trace
`a676e252-22d7-4e77-a5f7-cee8a76dfb20` starts at `BE6300`, stopping at the
first guard/opaque handoff. No `BE2F80` parser or formatter work is assigned.

Pushed `a151b67` for guarded forwarding code. Alternate handoff is reviewed
and registered; corrected its wrap fixture to `FFFFFB84 + 47C == 0`.
Final runner currently waits on evidence writer's constant-to-ushort
compile correction at `StartupBinaryEvidence.cs:6782`; no production
behavior changes were requested there.

Alternate handoff now passes the pinned runner and its 22-byte window
independently matches PE. Removed an out-of-scope RET-4 assertion from its
immutable check so the helper normal-return boundary remains `BE529B`,
not the unrelated enclosing-function return at `BE52DC`.
Luna `9130ea7a-fe62-4f1d-8bc8-8899293c9043` owns only the embedded
word-growth guard `[BE6306,BE6325)`; PE owner is
`d2db6ac4-cd7a-4fdd-8d31-36e582bdecc8`, preserving pending forwarding and
alternate anchors. This guard uses wrapping DWORD arithmetic and unsigned
comparison before the opaque `BE68A0` call; no slot allocation/copy work
is assigned yet.

Pushed alternate handoff as `927d52c`. Growth guard is now registered and
passes the pinned runner with forwarding, alternate, and growth immutable
anchors. Corrected its missing-read helper argument name and changed branch
fixtures to exercise valid skips rather than accidentally supplying an
unreached call return. IDA confirms call raw `BE6320 -> BE68A0`.
Read-only slot trace agent declined the external sample path; coordinator
obtained IDA disassembly directly. Luna
`b4e44058-f80c-45ff-9f99-2794ed44d520` now owns only postguard slot-index
projection/checks `[BE6325,BE6352)`, not allocation/copy.
The prior trace's epilogue addresses were off: IDA shows INC at `BE6389`,
POP ESI `BE638C`, POP EBP `BE638D`, RET 4 at `BE638E` (ends `BE6391`).

Growth guard pushed as `0ee07c3`. Slot index, zero-slot allocation, and
conditional WORD-copy tail are now registered and pass the pinned runner,
including exact PE windows of 45, 28, and 35 bytes respectively.
Index review corrected test addresses (`2004`, not `2000`; zero index
retains base `FFFFFFFF`). Allocation review keeps missing EAX stopped before
cleanup and models post-indirect-call ECX independently. Copy review fixed
undefined local references, one misplaced argument, and wrapped slot fixture.
Immutable review corrected FF15 absolute operand decoding (no PC addition).
Allocation agent's reported IDA mismatch came from querying mapped VA rather
than raw IDB EA; coordinator raw `BE6300` disassembly agrees with local PE.
Luna `a2ef1bc7-b3d5-499c-9753-a86d98770dd9` remains read-only owner of the
first guard/handoff trace at raw `BE68A0`.

Slot lifecycle pushed as `b15ab87`. Reviewed doubling loop and growth buffer
allocation; corrected requested `10000000` fixture typo, allocator argument
from erroneous shift-by-four to multiply-by-four, and invalid-entry fixtures
which accidentally supplied the receiver read. Loop oracle independently
confirms count `80000000` cycles at zero; this is not a native reachability claim.
Composition author `2e71f010-e1cd-421e-b85b-198dc5e7a051` is correcting a
major integration error: BOTH guard skips reach index stage `BE6325`, not
terminal append completion. Do not register/commit composition until reviewed.
`97420018-4f6a-42f5-b5a9-d8918aa02252` owns first-copy projection/checks
`[BE691A,BE694D)`; `4dae6c7c-0e29-41b1-b0b5-3946dca9b05f` owns immutable
anchors, preserving pending loop/allocation anchors. No stage after first
copy has been assigned. Runner currently catches composition files mid-edit;
wait for author ready before next serialized run.

Composition correction returned: both skips continue index, and invalid gate
outcomes cannot pass solely on a boundary route. Registered pending runner.
First-copy review found wrong indirect opcode operand bytes and a return-value
capture conflating missing return with nonreturn; author is correcting those
two files. Immutable first-copy window is reviewed and matches the independent
51-byte PE capture. Read-only successor
`8391880f-d83e-473f-ba21-bed9f5a2efe5` traces `[BE694D,...]` to the first
opaque call on each unsigned branch path. No code for that successor yet.

Pinned runner now passes append composition, doubling, buffer allocation,
and accumulated immutable anchors. Corrected the composition's zero-candidate
fixture to slot `FFFFFFF8` with retained EDI `1234` (masked index four):
slot zero would instead produce candidate eight and require a WORD capture.
First-copy production/check files remain pending author correction and are
not registered or included in the next commit.

Pushed composition/doubling/buffer allocation as `894c9e3`. First-copy
correction is now reviewed and registered: separate return status versus
optional EAX, corrected FF15 bytes, and destination fixture `500+DEAD=E3AD`.
Pinned runner passes. Postcopy gate `[BE694D,BE6960)` is assigned to Luna
`eead46ff-2c2f-446d-916a-858ee542ac9f`, immutable owner
`a5a76f83-a0fc-4749-bdbf-0b0c93ce457e`. Its receiver+4 reload precedes
cleanup12; prior call EAX participates in saved-pointer LEA; frame-8 reload
precedes frame-10 store and unsigned JA to `BE69A7`.
Postcopy branch-body trace was read but not assigned: the fallthrough
call pushes a frame-10 RELOAD (not an assumed retained pointer), while
the taken branch pushes retained EDX. Keep those observations independent.

First-copy pushed as `f08987c`. Postcopy gate reviewed/registered and passes
the pinned runner, with corrected missing-frame fixture EDX `100`, not zero.
Next branch-copy code owner is Luna
`18869950-8206-425d-ae36-867cf091600a` for `[BE6960,BE6984)` and
`[BE69A7,BE69BD)`; immutable owner
`0e7f8d4c-c6ed-4c24-8fae-aa82e11df0e6` preserves pending gate anchors.
Do not stage evidence until writer completion.

Postcopy gate pushed as `c04a689`. Branch-copy code and immutable anchors
are reviewed and registered; pinned runner passes. Corrected fixtures which
selected the opposite unsigned branch and supplied a forbidden frame-10
capture on the taken path. A fall-path subtraction cannot underflow under
its EDI<=ESI precondition; zero-difference fixture now respects that.
PE windows independently match 36 and 22 bytes.
Luna `e0af090c-c85b-43de-a2ee-f16f39166049` owns a read-only exact trace
of zero-fill instructions after `BE6984` and `BE69DF` through join `BE69F2`.
Require explicit direction-flag semantics before any REP STOS projection.

Branch-copy pushed as `f721e5d`. Independent PE captures for successors:
`[BE6984,BE69A7)` 35 bytes, `[BE69BD,BE69DF)` 34 bytes,
`[BE69DF,BE69F2)` 19 bytes, publication `[BE69F2,BE6A10)` 30 bytes.
Luna `2a839f54-7688-4a20-b429-df84091d1b80` owns second-copy
production/checks; `022391ce-233d-4c7d-9b46-9ab5355ac52d` immutable
anchors. Luna `a5c902f7-2fac-43aa-8e79-9f9da34b434f` independently owns
publication production/checks; no publication evidence writer assigned yet.
Read-only `c6423e2e-c302-4db6-89b9-a47d383c4928` checks whether the enclosing
helper actually reads its stack argument; do not infer that role from RET 4.

Stack-argument trace confirms no incoming DWORD argument read anywhere in
`[BE68A0,BE6A1B)`; RET 4 only purges it. Normal epilogue restores EBX/ESI;
error indirect call at `BE6A15` has unresolved return/preservation behavior.
Zero-fill exact trace confirms conditional frame-8 reload: first fall REP
count zero skips that reload and retains incoming EDI for the later test.
DF is not established by these windows. Luna
`9cc8553a-578d-4067-b263-f31064da9410` owns symbolic zero-fill code/checks,
with explicit DF and separate completion captures per reached REP.
Agent byte dump included the next instruction's first byte; authoritative
exclusive windows remain coordinator's 35 and 19 bytes above.

Second-copy immutable writer completed; its 34-byte window matches the
independent capture and is reviewed pending production/checks readiness.
New sole evidence writer `9aada2cb-22d1-40cb-a5f6-4636601525a6` preserves
that pending anchor while pinning zero-fill/publication windows.
Read-only Luna `7a4fadd3-a8a7-4568-897c-6060db6fc1e1` independently traces
reliable receive alternate continuation at `BE52DF`; no code assigned there.

Second-copy and publication production/checks reviewed and registered;
pinned runner passes. Publication review corrected helper argument names,
the missing receiver+4 stop at `BE69F5`, and `FFFFFFFF+1==0` fixture.
No inferred publication occurs before independently captured counter read.
Finalization immutable writer remains active; do not stage its file yet.

Second-copy/publication pushed as `44bb37c`. Finalization evidence finished
and reviewed; new immutable owner `5b6bfd61-d346-47be-8eb0-68f4619b7994`
preserves its pending anchors while adding reliable slot `[BE5303,BE532F)`.
Slot production/check owner is `1f87b03c-4e9d-4649-874e-dd37ce62c664`.
Read-only successors `dc6bd1a9-4d4a-447d-b9cf-781bc962cb95` own high-delta
`[BE52A5,BE52DF)`; `46071380-0e5e-4420-af4e-5220f6bd639f` owns record
comparison `[BE532F,BE5353)`. All Luna. No receive continuation code
registered yet; zero-fill author remains active.

High-delta trace mapping typo rejected: `BE52A5+400C00=FE5EA5`, not
`4BE92A5`. Shared two-caller projection/checks assigned to Luna
`d915f27f-4086-4069-b20b-dfb030745423`, exact 34-byte windows
`[BE52A5,BE52C7)` / `[BE5353,BE5375)`. New sole immutable writer
`ad74a354-38c4-4fa9-acd9-e27a1c097aca` preserves pending finalization/slot
anchors. Caller receiver+8 reload precedes ADD ESP,18 on both paths.
Read-only Luna `8e7d7264-270d-4199-a3ba-2b1994ef06d9` traces the paired
counter/recycle/AL-only return tails; no code assignment yet.

Reliable slot code/checks reviewed/registered; qualified a missing cross-type
constant reference. Runner currently catches active zero-fill check helper
parameter typos; author notified. Record-word projection owner
`b2f26990-dac3-4fca-a17f-c921d4624ecb`; sole evidence owner
`304e4ba8-65c7-43a9-b9a3-576d835f4e3b` preserves pending anchors.
Correct record diagnostic literals are `1B58B3E` / `1B58B1B`, not earlier
trace's `1BB...` values.
Tail trace again incorrectly queried mapped VA in flat IDA; coordinator raw
disassembly confirms identical INC DWORD[EAX+20], global VA `1DFFF84`,
payload push, call `BD5C60`, AL-only one, RET 4. Luna
`03540207-aa5d-4b6b-8ed6-ee15ae7b91f3` owns paired tail code/checks.

Empty-slot callback `[BE538D,BE53A2)` independently matches its 21-byte PE
window. Luna `ebe91a7f-2b8e-4e46-bcc3-693baed3ab92` owns its code/checks:
header WORD bits 12/13 gate an indirect slot+C call, ECX=payload+8 address,
no explicit pushes. Zero loaded pointer is not a guard. Immutable writer
for this window is not yet assigned.
Read-only Luna `2718019d-6b5a-4b8b-bc65-2b8a9b063e60` owns unreliable
alternate continuation `BE5AC2`, stopping at the first opaque handoff.

Slot, forwarding, and symbolic zero-fill registrations now pass the pinned
offline runner. Corrected forwarding record-constructor named argument;
corrected zero-fill's premature rejection of legitimate frame-4 and final
REP captures before an otherwise fully captured first REP. Fault/missing
first completion still blocks later reads. Both existing successful two-REP
and maximum-count fixtures now exercise the corrected path.
Successor read-only traces: `fc43d3fd-cda5-41c8-8078-0eca3fef9aa9`
post-callback `BE53A2`, `fd5f3eb9-1ba8-490e-a28f-5f0ffd447023`
zero-header `BE53E9`, each stops at the next opaque handoff.

Pushed reviewed slot/forwarding/zero-fill code and accumulated immutable
anchors as `f70aad0`. Record WORD projection/checks now reviewed, registered,
and passing. Record immutable anchors were included in that prior commit.
Sole new immutable writer `ba5ec568-ab49-4f95-b274-dd246e2a7570` owns
callback/recycle windows. Luna `3914df5b-f54d-4f0a-9fec-cf5b1d7dbec7`
owns unreliable residue handoff code/checks `[BE5AC2,BE5AE4)`;
`7bceee3d-3184-4192-a2ee-802fac9b966d` traces its normal continuation.

Pushed record WORD code as `a49dbb8`. Paired recycle and empty-slot callback
code/checks now reviewed, registered, passing. Corrected recycle check helper
discarding receiver/payload fixture arguments. Empty-slot author had
substituted different header instructions; corrected to verified MOV BX,
SHR BX, AND BL bytes. Fixed later-normal-with-missing-target fixture and
DWORD raw-to-VA wrap expected `400BFF`, not `3FFFFF`.
Active code successors: `25c92ef6-5cc7-427c-94b6-6bac6298a09e`
forwarding-to-return composition; `028ba578-4400-4fcc-866b-40781a947369`
callback-result diagnostic; `facf7d53-325f-4a74-8f97-063ac1df3eae`
masked-length callback; `259158f9-28fb-4f03-ad44-790eadfd311c`
independent frame+8 slot publication suffix.

Pushed callback gate and paired recycle returns as `2340038`.
Completed callback/recycle immutable evidence owner `ba5ec568...` reviewed;
sole successor evidence writer `91f74030-0034-409c-bb97-fb80bf3eb508`
owns result/length/scalar/publication windows, preserving prior anchors.
Coordinator independently read raw `[BE53DD,BE53E9)`:
`8BD08ACBE8FA04FFFF83C404` (EDX=EAX, CL=BL, call BD58E0,
normal cleanup 4); Luna `0d95196c-54e8-4ede-8136-4422c18df574`
owns its code/checks, optional captured ECX upper bits, not whole-byte widening.
Unreliable continuation trace read: `[BE5AE4,BE5AF0)` is
`8B4F088D46FF83C41401411C`: receiver+8 read, ESI-1 in EAX,
cleanup 20, add to counter at loaded pointer+1C. Luna
`6a7be6fb-8fdd-4844-af5a-ca112ba41338` owns this code/check pair.
Trace's raw BE2F50 VA arithmetic was wrong: correct VA `FE3B50`,
not `FE6B50`; no mapping disagreement.

Forwarding composition, publication suffix, callback-result, masked-length
files reviewed/registered. Corrected composition's comparison of two distinct
enum types and moved tail projection after route/pointer validation so invalid
captures expose no projected tail effects. Runner blocked by masked-length
author's out-of-position named arguments; same author notified.
Luna `a40154ba-872b-4329-98e9-8d00744edc32` owns unreliable joined-header
preparation `[BE5AF0,BE5B16)`, including ESI=zero-extended AX after DWORD
length subtraction, independent WORD reload, frame stores and selector branch.

Seven new code/check registrations now pass pinned runner: forwarding-return
composition, callback-result, masked-length, publication, scalar handoff,
unreliable residue handoff/counter. Fixed composition mismatch tests to
require absent tail plans. Sole evidence writer now
`f3ab3f11-8915-46aa-a3f7-c8ddd2e2195c` adds unreliable windows and preserves
completed pending receive result/publication anchors.
Read-only Luna `f2c08a16-984e-425d-a41f-1ec7ef89d6af` traces unreliable
post-callback `BE5B25`, stopping at next opaque handoff.

Pushed seven callback/residue code stages as `43b9572`. Joined-header
projection reviewed/registered and passes exhaustive header oracle. Nonzero
marker callback owner `5f7d0aa1-1213-4a37-be6d-75ebb3c98e79`;
read-only zero-marker callback trace owner
`bb23bedf-bb08-47f4-87ab-a712189c8187` at `BE5B32`.
