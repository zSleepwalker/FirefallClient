# Reverse-engineering handoff archive 11

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
