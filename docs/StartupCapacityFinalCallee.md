# Startup: capacity final-callee projections

`CapacityFinalCalleeFirstValidationProjection` pins the ordered global
and `[ESI+4]` reads, unsigned comparison, and low-two-bit comparison,
including defined CMP/AND flags without defining AND's auxiliary carry.
`CapacityFinalCalleePrefixLifecycleProjection` derives the prefix mask
from the entry TEST; `CapacityFinalCalleeValidationLifecycleProjection`
derives bypass ESI and requires independent ESI after the opaque call.
Terminal validation branches with supplied unreached reads are retained
as partial evidence, not admitted as a clean continuation.
`CapacityFinalCalleeEntryValidationProjection` connects the entry/prefix
lifecycle to this validation without admitting validation captures on the
zero-entry RET path. Nonzero validation receives the exact derived prefix
capture, while post-call ESI remains independently captured.
`CapacityFinalCalleeAlignmentDispatchProjection` retains the two initial
frame writes, the AL-bit route, the fresh `[ESI]` subtraction, and the
independent entry-ECX/computed-ECX gates. Its opaque indirect call retains
pre-call register evidence only; captured normal return does not establish
post-call register preservation.
`CapacityFinalCalleeTagValidationProjection` distinguishes the unsigned
ESI/EDX guard, the two independently tested bits of the fresh `[EDX+4]`
DWORD, and the subsequent comparison against fresh global `1EDADE0`.
Branch destinations remain separate evidence routes; equality stops at
raw `12C8F21` without inferring later global updates.
`CapacityFinalCalleeAlignmentTagLifecycleProjection` admits only the clean
AL-bit route and derives tag ESI/EDX from its local register evidence.
`CapacityFinalCalleeZeroExitLifecycleProjection` retains the native global
SUB read/write even when the admitted ECX is zero, then forwards independent
exit captures. Successful return requires the exact RET boundary and clear
unreached exit-capture flags.
`CapacityFinalCalleeTagEqualityContinuationProjection` preserves the ordered
global and `[ESI+4]` writes before the fresh tag and unsigned-limit reads.
Optional tag clearing does not replace later reloads with inferred values.
The direct-call route records only pre-call registers and a captured return
gate, while the unsigned-limit branch stops at the exit entry.

`FrameHandoffCapacityDispatchLifecycleProjection` connects the admitted
first-transfer return to only its selected above or at/below branch.
Successor entry registers come from fresh return reads and local arithmetic;
opaque downstream call captures remain independent. Supplied unselected
branch evidence is retained as unreached rather than executed.
`CapacityFinalCalleeEntryAlignmentLifecycleProjection` derives the original
loaded EAX, masked ECX, and admitted ESI for the alignment window while
requiring independent EBP/ESP captures. Clean alignment branches remain
successful bounded destinations rather than failed calls.
`CapacityFinalCalleeNonzeroValidationProjection` preserves the initial EDI
addition, logical EAX shift, ordered frame stores and fresh memory reads.
The common `12C8E76` destination is an opaque indirect call, not a proven
nonreturning terminal.
`CapacityFinalCalleeFrameComparisonCallProjection` keeps the subsequent
`[EBP-4]` comparison as a fresh reload and models its optional indirect call
separately from the call at `12C8E76`.
`CapacityFinalCalleeTagEqualityLifecycleProjection` derives the equality
continuation registers from the admitted AL-bit path. Clean other tag
destinations remain admitted bounded routes only when no continuation-only
captures were supplied.
`CapacityFinalCalleeSizeComparisonCallProjection` masks a fresh `[ESI+4]`
DWORD and shifts an independently captured EAX before comparison. No prior
shift result is reused across unresolved calls.
`CapacityFinalCalleeAlignmentNonzeroLifecycleProjection` derives the
nonzero-route EAX/EDI/ESI/EBP locally but requires independent EBX evidence.
Each route validates its own final writes and registers, rather than imposing
the two-write prelude's register values on later three-write branches.
`CapacityFinalCalleeTagEqualityExitLifecycleProjection` admits the exit only
from the clean capacity branch at `12C9441`, retaining independent exit reads
and opaque-call outputs. Any supplied optional exit capture on another route
is marked unreached, including an otherwise empty capture.
`CapacityFinalCalleeDescriptorReloadProjection` distinguishes the scaled
LEA address and frame write from subsequent independent frame reloads.
`CapacityFinalCalleeFrameMaskUpdateProjection` captures the variable-count
shift with the x86 low-five-bit count, 32-bit complement, and fresh global
AND read/write without reusing earlier frame or register values.
`CapacityFinalCalleeFrameSizeLifecycleProjection` carries ESI/ESP only across
the frame-call bypass and requires independent post-call captures otherwise;
EAX is always independently captured. A reached but contaminated size
boundary is not admitted as a successful lifecycle.
`CapacityFinalCalleeDescriptorComparisonCallProjection` distinguishes the
two equality bypasses from the unsigned-below and memory-mismatch call
routes, preserving the fresh reads and exact comparison operand order.
`CapacityFinalCalleePairedFieldWritesProjection` records the two ordered
stores at raw `12C8EE9..12C8EF8`, followed by an independent global reload.
It retains both stores when that reload is missing, including aliased
destinations; it does not infer pointer ownership or list semantics.
`CapacityFinalCalleeFrameMaskTagLifecycleProjection` admits the tag join
only after a clean global-mask write and requires independent ESI/EDX.
A clean frame mismatch remains bounded at raw `12C8ED5`; supplying tag
captures on that route rejects admission without erasing earlier effects.
`CapacityFinalCalleeSizeDescriptorLifecycleProjection` joins only the clean
size boundary. It carries ESI across the size-call bypass, requires a fresh
ESI capture after an opaque call, and independently captures EBP/EBX.
Contaminated descriptor destinations retain their native effects but do not
count as admitted successful routes.
`CapacityFinalCalleePairedTagLifecycleProjection` preserves both ordered
stores through a blocked tag continuation and independently captures tag
registers and memory. `CapacityFinalCalleeDescriptorPairedLifecycleProjection`
carries EAX/EBP only across comparison bypasses; opaque call returns require
independent registers before either store is admitted.
`CapacityFinalCalleeDescriptorMaskLifecycleProjection` carries the initial
frame-minus-10 value as ECX across the no-call equality route, not the scaled
descriptor address or the later comparison reload. Its mask reads remain fresh.
`CapacityFinalCalleeLargeSizeFieldsProjection` reconstructs the bounded
raw `12C8F76..12C8FA8` gates, frame write, and paired stores without inferring
ownership. It preserves earlier effects on each missing-read stop.
`CapacityFinalCalleeAlternateFieldSelectionProjection` writes each candidate
address before testing the separately loaded DWORD. It stops at `12C8FC2`
or `12C8FFE`, retaining the first write when the second read is missing.
`CapacityFinalCalleeFieldDescentStepProjection` performs one bounded descent
step only. The selected pointer is freshly reloaded after its comparison,
and the loopback to `12C8FC5` is recorded without executing another iteration.
`CapacityFinalCalleeFieldClearTailProjection` retains the original CMP flags
across the frame store and EDX reload. Both the later frame read and the global
reload after the zero store remain independent, even at aliased addresses.
`CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection` models the
distinct raw `12C8EB8` opaque call before the mask window. It requires captured
post-call registers, rejects call-only captures on bypasses, and admits only
clean mask destinations.
`CapacityFinalCalleeConditionalFieldStoreProjection` uses an unsigned gate
before its fresh field comparison, writes exactly one selected field, and
tests the retained EAX value to select raw `12C8EF2` or `12C9058`.
`CapacityFinalCalleeLargeSizeAlternateLifecycleProjection` joins only the
clean equality route at raw `12C8FA8`. Other large-size destinations remain
bounded; the alternate reads are fresh even when the prior frame write
aliases their addresses.
`CapacityFinalCalleeIndexedRootMaskProjection` retains the fresh frame-C
read before the unequal branch, three independent index loads, and the
indexed write before its optional mask update. Missing reads stop at their
own instruction; zero, unequal, nonzero-store, and mask routes remain
bounded at raw `12C8EF8`, `12C903B`, `12C9058`, and `12C8EF2`.
`CapacityFinalCalleeDescentClearLifecycleProjection` joins only a clean FDD
terminal, carrying the original frame-derived EDX into the clear tail.
FC5 loopbacks stay single-step; contaminated captures preserve effects
without admitting the continuation.
`CapacityFinalCalleeSelectionDescentLifecycleProjection` carries either
clean FC2 selection's EAX into one descent step, with a separately captured
frame-minus-four reload. The FFE selection bypass cannot consume descent
captures, and FC5 is a bounded loopback rather than an iterative model.
`CapacityFinalCalleeGuardedFieldStoreProjection` covers raw `12C9058..12C906E`:
the unsigned global comparison precedes the ECX store, then a fresh
ESI-plus-10 read selects `12C9080` or `12C906E`.
`CapacityFinalCalleeSuccessorPairedStoresProjection` covers only the
`12C906E` entry, retaining its independent global comparison and ordered
stores before a fresh ESI-plus-14 read selects `12C8EF2` or `12C908B`.
The direct `12C9080` bypass does not execute the preceding paired stores.
`CapacityFinalCalleeIndexedConditionalLifecycleProjection` joins only the
clean unequal root comparison at `12C903B`, retaining frame-derived ECX.
The conditional EBX and EAX remain independent captures; derived ESI is
supplied only when its comparison consumer is reached. Other root exits
stay bounded and reject conditional captures.
`CapacityFinalCalleeSecondFieldTestProjection` models the direct `12C9080`
entry without the preceding stores. `CapacityFinalCalleeClearIndexedLifecycleProjection`
joins only a clean field-clear completion at FFE to indexed reads; fresh
frame and global captures remain independent from the clear-tail writes.
`CapacityFinalCalleeSelectionClearLifecycleProjection` admits clear-tail
execution only after a clean terminal selection/descent at FDD. FFE
bypasses and FC5 loopbacks remain bounded and cannot consume clear-tail
captures; missing reads preserve only effects reached before the stop.
`CapacityFinalCalleeFinalPairedTailProjection` guards the final ECX value
against a fresh global at `12C908B`, then performs the two ordered stores
and jumps to `12C8EF2`. `CapacityFinalCalleeConditionalGuardedLifecycleProjection`
joins only clean nonzero conditional-store routes to `12C9058`; retained
registers are supplied only at reached consumers, with independent later reads.
`CapacityFinalCalleeSecondTestTailLifecycleProjection` joins the fresh
nonzero second-field value at `12C908B` to the final guard, with EAX
remaining an independent live-in. `CapacityFinalCalleeAlternateTagGateProjection`
starts separately at `12C90A2`, masks a fresh DWORD, and retains byte-width
CMP flags. It is reached from the `12C8E51` branch, not by fallthrough from
the final paired stores.
`CapacityFinalCalleeGuardedSuccessorLifecycleProjection` selects the
`12C906E` paired path or the `12C9080` bypass test without projecting an
unselected child. `CapacityFinalCalleeAlternateFieldWritesProjection`
preserves the global write before a fresh low-bit clear and ordered
EDI-derived stores, stopping at the `12C9441` epilogue entry.
`CapacityFinalCalleeIndexedGuardedLifecycleProjection` joins only the
conditional nonzero path, leaving the root's direct table-write path bounded.
`CapacityFinalCalleePairedFinalLifecycleProjection` carries the successor's
fresh second-field value into the final guard, preserving all ordered stores.
`CapacityFinalCalleeGuardedFinalLifecycleProjection` extends both selected
second-field routes through that final guard. The alternate tag/write
lifecycle retains independent reads at an aliased address.
The alternate root gate at `12C90CA` and equal-path writes at `12C90D2`
remain separate bounded projections. The gated epilogue at `12C9441`
records independent stack reads, preserves TEST flags across POPs, and
requires fresh post-call EBP rather than assuming opaque-call preservation.
The alternate root lifecycle joins only equality to its ordered writes;
its clean unequal exit remains bounded. The aligned-size projection stores
the shifted value before the unsigned threshold branch. The alternate
epilogue lifecycle preserves preceding writes on missing stack evidence
or a nonreturning opaque call.
The alternate paired frame loads at `12C910C` retain separate fresh values
even when the first frame store aliases the second load. Both stores precede
the comparison; equality stops before the opaque call at `12C911C`.
The root-size lifecycle follows only the unequal root branch, while the
root-epilogue lifecycle follows only completed root writes. The second
frame gate at `12C9122` uses an independently captured frame value and
stops before its own opaque call when equal.
The shifted-frame gate at `12C912D` reloads `[EBP-4]` independently,
shifts with DWORD wrapping, and keeps undefined shift flags absent.
Equality bypasses the opaque call at `12C9137`.
The aligned-frame lifecycle enters the frame loads only through the clean
below-threshold branch. It supplies retained EBP only when the first frame
store consumes it and rejects later captures on the large-size bypass.
The paired-frame gate lifecycle carries retained EDX/EBP only across its
clean unequal edge. The second-shifted lifecycle leaves equality bounded
at the preceding opaque call and preserves independent subsequent reloads.
The alternate backlink gate preserves both comparisons without entering the
opaque call at raw 0x12C915A. Only the not-below path reads ECX+C; a fresh
field comparison equal to EDX selects raw 0x12C9160. The final-frame gate
then independently reloads EBP-14 and compares it with ECX, bounding equal
and unequal paths at raw 0x12C9167 and 0x12C917C respectively.
The preceding indexed-frame gate independently reloads EBP-4 and EBP-10,
computes the wrapping address 0x1EDADF0 plus eight times the first value,
and stores that address at EBP-C before comparing it with the second value.
LEA performs no memory read. Equal selects raw 0x12C9160; unequal selects
raw 0x12C9151, with the local write retained on both paths.
The paired-shifted lifecycle joins only the clean second comparison's
unequal raw 0x12C912D edge. It preserves the two preceding frame stores,
independently reloads EBP-4, and bounds the later comparison at raw
0x12C9137 or 0x12C913D. Supplied shifted captures on either earlier opaque
boundary are reported as unreached and prevent clean admission.
The alternate mask tail reloads EBP-8 and EBP-4 independently, masks CL
to five bits, complements the shifted value, and applies it to a fresh
global DWORD at VA 0x1EDADC8. Zero-count shifts preserve captured flags
(or remain unknown); the final AND defines its own flags. Unused entry
flags on nonzero shifts prevent clean exit admission.

The alternate successor gate bounds frame equality and fresh-field
equality at raw 0x12C9190, while unsigned-below and field inequality stop
before the opaque call at raw 0x12C918A. The separate successor-write
projection preserves both MOV events even when their addresses alias,
then stops at raw 0x12C92BD. The backlink-final lifecycle enters the final
frame comparison only through the earlier clean raw 0x12C9160 bypass.
The shifted-indexed lifecycle preserves two distinct EBP-4 captures on its
clean equality handoff. The indexed-backlink lifecycle carries the fresh
EBP-10 value into ECX while retaining the preceding local store. Both
compositions reject downstream evidence on their bounded bypass routes.
The common-tail tag-write projection retains its OR result and flags if
ESI is missing, and keeps both stores ordered even at aliased addresses.
The indexed-final lifecycle enters the final-frame gate only on indexed
equality, retaining its preceding store. Its ECX handoff is withheld until
the independent EBP-14 reload makes the consuming CMP reachable.
The large-frame threshold projection compares a fresh EDX+8 value with
EBX, stores it at EBP-14, then independently reloads EBP-10. The MOV
instructions preserve the original CMP flags: JB selects raw 0x12C8E76
or 0x12C91B9 from that earlier unsigned comparison, not the reloaded ECX.
The large-frame root gate requires both independent EDX-relative reloads
before its EBP-10 store. Missing prerequisites stop at their own instruction;
the final JZ selects raw 0x12C91D6 on equality and raw 0x12C91A8 otherwise.
The final-mask lifecycle carries the preceding CMP flags only into reached
zero-count shifts and admits a clean bounded unequal exit at raw 0x12C917C.
The large first-field test independently reloads EDX+14, stores the
LEA-derived address at EBP-4, then tests the fresh value. Its zero and
nonzero destinations are bounded at raw 0x12C91E3 and 0x12C91F0.
The successor-write lifecycle admits both clean gate equalities while
keeping contaminated opaque exits bounded. Mask-common and successor-common
lifecycles preserve all earlier writes when later caller registers are
missing; aliasing does not collapse their ordered write events. Mask-common
also retains shift/NOT flags when the fresh global read is missing.
The large backlink gate independently reloads EBP-14 before reading that
value's plus-C field. Missing EDX prevents CMP completion without discarding
the independently reached memory operand. The common root gate likewise
retains its global read when ESI is missing. Both gates compute all CMP
flags and stop at their exact branch destinations without entering them.
The second field test at raw91E3 independently reads EDX+10, overwrites
EBP-4 with that field's address, and tests the fresh value. Zero selects
9226; nonzero selects91F0. The common root store at raw92D0 writes EDI to
VA1EDADD0 and jumps to9441 without entering the epilogue or changing flags.
FinalMaskCommon and IndexedBacklinkFinal preserve independent downstream
captures and exact clean branch handoffs. CommonWritesRoot keeps both tag
stores before a fresh global read, including aliases. CommonRootStore
admits only fully evidenced equal or unequal exits, never missing operands.
LargeRootThreshold derives EBP only at the child store and retains the
latest CMP flags even if the subsequent frame reload is missing. The
common size gate shifts unsigned EDI by three before comparing against20h;
its destinations are92F6 and9354.
LargeRootFirstField and LargePairedFieldTest gate fresh field reads on
their exact predecessor branches. The paired path requires a zero first
field, preserves both writes to EBP-4, and retains first TEST flags when
the second read is missing. LargeThresholdBacklink independently reloads
the frame despite its preceding store. LargeSuccessorWrites retains the
fresh CMP operand when EDX is absent, records a reached first store when
EBX is missing, and preserves two distinct aliased writes on equality.
CommonRootSize preserves the parent CMP when caller EDI is missing and
enters the size gate only on root inequality. FinalMaskCommonRoot retains
three writes before its independent root read; unused root captures on
the bounded917C path prevent clean admission.
The bounded node reload loads fresh EAX through ECX, retains that address
in EDX, and stops at91F3 without iterating. The saved-frame gate independently
loads EBP-10 and tests it: zero selects92BD, nonzero selects9231.
NodeFieldSelection preserves a separate fresh saved EDX while inspecting
EAX+14 and, only after zero, EAX+10. LEAs preserve each CMP's flags.
CommonIndexedAddress forms the address without reading it and stops at
the opaque9304 call instruction or the clean930A branch.
BacklinkSuccessor derives late EBX from the independently reloaded frame
only after child equality reaches the first store. SavedFrameCommon enters
the tag stores only on the zero frame path. LeafThreshold preserves the
independent global CMP operands and its flags across the saved EDX store
and a fresh EBP-C reload; that reload cannot change the earlier JB decision.
NodeSelectionReload derives the node read address from the selected LEA,
not the previously captured field value. Aliased node reads remain fresh,
and the backward jump to91F3 is a bounded exit rather than another iteration.
RootPairedField composes the existing paired helper, retaining both frame
stores and the last completed root CMP or child TEST. LeafClearStore reads
fresh EBP-4 before writing literal zero; missing read data stops at921D,
and captured zero remains a valid write address.
NodeSelectionLeafThreshold uses the saved frame EDX, not the selected LEA
or EAX, for the global threshold comparison. Its EBP handoff is gated by
the completed child CMP. LeafThresholdClear reloads EBP-4 independently
of the preceding saved EDX store and retains the threshold CMP flags.
CommonSizeIndexed keeps the shifted size index separate from original EDI
and stops at the opaque9304 call or bounded930A/9354 exits. SavedIndexGate
uses fresh EDX+1C to address the independent indexed CMP operand, then
reloads global EBX without changing that CMP's JNZ decision.
CommonMaskSelection bounds the shifted mask TEST before either later path,
with zero-count SHL preserving incoming flags. SavedFrameCommonRoot enters
the root CMP only after the common tag stores complete at92C8, not at their
entry92BD; independent global captures never replace prior writes.
SavedFrameIndexed keeps child EDX independent of the saved ECX value.
CommonIndexedMask preserves the parent CMP flags when the child's masked
shift count is zero and its fresh global is missing; a reached TEST replaces
those flags before either bounded mask destination.
CommonMaskOrStore writes only the OR result to VA1EDADC8 without rereading
that global. SavedIndexedStoreGate performs a new EDX+1C read before the
indexed EAX store; missing EAX retains the attempted store address but cannot
reach TEST or JNZ, and both zero and nonzero destinations remain bounded.
CommonSuccessorThreshold compares the freshly loaded EDX+8 DWORD against
an independent VA1EDADD8 capture, exposing unsigned JB and all CMP flags
without entering either rejection or later writes.
CommonRingWrites preserves all four ordered link stores, including aliasing
and independently captured attempted operands, before the bounded9441 jump.
LargeMaskAndStore reads and updates VA1EDADCC independently, retaining known
global data when EAX is missing and stopping before the common tail at92BD.
SavedIndexStoreLifecycle admits only the exact equality9243 edge, retaining
original EDX but independently reloading the child index at the same address.
The child EAX remains an explicit capture and a missing value preserves the
attempted store and prior CMP flags without advancing to TEST.
SavedMaskShift retains the fresh shift count across later missing frame
evidence and computes NOT without changing SHL flags. MaskSelectionOr derives
both OR operands from the existing selection helper. MaskSuccessor instead
keeps child EDX and both successor reads independent of the earlier mask.
OrRing retains the global OR write before any later missing ring operand.
SavedMaskAnd passes the NOT result into the independent global update,
preserving shift flags until AND completes. IndexedReject bounds the register
CMP/JB at9263 without projecting the following field access.
IndexedFieldGate bounds the fresh [ECX+10] comparison at926B and preserves the
independent EDX operand when the memory evidence is missing. EaxGlobalThreshold
bounds927C's comparison against the separately captured absolute global.
Neither projection enters the selected store or rejection destination.
AlternateRing shares the four-store mechanism while retaining9343's distinct
instruction evidence. SavedIndexReject derives EBX only from the later absolute
reload. RejectField retains ECX into the fresh field comparison; IndexedFieldStore
selects +10 or +14 and preserves CMP flags through the bounded9278 handoff.
SavedIndexedZeroMask keeps both reloads independent of the preceding zero
store, restoring the parent TEST flags when the masked SHL count is zero.
SuccessorRing derives EAX from the fresh successor read and retains EDX,
admitting the alternate ring only at the exact not-below9343 edge.
PostStoreZero bounds9278's TEST/JZ, distinguishing a missing EAX from captured
zero and preserving the selected92BD or927C destination without entering it.
BacklinkNextField retains the9288 store attempt or commitment before independently
reading [EDX+10]. Its TEST uses the fresh value even when the address aliases
the preceding write, and stops before9292 or92A4.
HighSizeEntry retains SHR's intermediate flags but uses TEST's replacement
flags for the935B branch. EaxBacklink admits9288 only after an exact not-below
threshold edge, preserving CMP flags until the child's fresh-value TEST.
HighSizeCeiling performs a full unsigned comparison against FFFF, selecting
the936F BSR entry or setting ECX to31 before the9384 store boundary.
NextFieldThreshold keeps ECX and the absolute global operand independently
captured at9292 and bounds the unsigned branch before the paired stores.
IndexedFieldStoreZero derives TEST's EAX from the committed store value,
retaining CMP flags when an earlier store is incomplete and replacing them
only after TEST executes at9278.
BacklinkNextThreshold derives the next CMP's ECX from the independent fresh
read, not the stored backlink value. HighSizeCeilingLifecycle carries the
shifted EAX into the ceiling check. NextPairedStores retains both directional
MOV writes, including aliased destinations, before the92A4 read boundary.
HighSizeBitScan models the936F BSR, committed9374 frame store, masked SHR,
and final index LEA through9384. Zero BSR leaves its destination undefined
and prevents further projection. The actual AND and final LEA begin at
raw012C937E and012C9381; the subsequent store remains outside this helper.
NextThresholdStoresLifecycle admits the exact929E threshold fallthrough
before its two directional stores. Parent CMP flags survive both MOVs,
including a missing store-address stop; the independent global observation
remains ordered before writes. A bypassed late EAX capture is explicitly unused.
IndexedNodeInitialization models the9384 field store, address-only indexed
LEA, EDX initialization, and two ordered zero stores through93A1. Partial first
store operands survive without a committed write. RightFieldGate independently
reloads [EDX+14], tests that fresh value, and stops at92BD or92AB without
entering either destination.
RightFieldThresholdGate compares ECX with an independent absolute-global
DWORD and preserves either known operand when the other is missing. FreshMaskGate
reads its separate global before SHL/TEST/JNZ; known shift operands remain
visible independently during partial stops. Counts are masked to five bits,
and TEST replaces the shift flags before selecting93AC or93C0.
HighSizeIndexLifecycle derives the BSR input from the exact ceiling handoff
and reuses original EDI for the later shift, bounding the composed index to31.
Zero/above-ceiling bypasses leave a supplied late EBP explicitly unused;
missing EBP cannot admit clean completion. SelfRingTail preserves its committed
global OR store during partial local-store stops, then records four separate
ring writes before the unentered942F jump.
RightFieldThresholdLifecycle derives the comparison register from the fresh
field observation, retaining parent TEST flags until the independent CMP
executes. IndexedCountClassification retains its fresh indexed DWORD before
the31 comparison; the equal path clears EAX without assuming its incoming value,
while the alternate path shifts ECX and subtracts it from19h.
CleanupCounterGate records the independent global counter read and decrement
write, preserving incoming CF even when the counter observation is missing.
The nonzero branch stops before9441; the zero path sets ECX to the address
immediate and stops before the943C call, whose callee is never entered.
MaskedTagEqualityGate preserves the original count in ECX and shifted EDI
in EBX while masking an independent fresh tag DWORD for comparison. A zero
effective shift count preserves AND flags. RingInsertion retains five ordered
stores through942F, including incomplete attempts and earlier commits when
the second-store EDX capture is missing.
DualPointerThreshold loads both independent observations before its first
comparison, even when that comparison branches below. FreshMaskSelfRingLifecycle
derives both OR operands from the prior gate and rejects unused late pointers
on bypass. HighSizeNodeLifecycle derives the initialization index across all
three size paths, preserving parent flags and frame-before-node write order.
TraversalIteration models one indexed child selection and masked comparison,
stopping at equality, zero-child, or loop-back edges without another iteration.
The zero-child destination at raw `0x012C945C` is additional CMP/JB code after
the preceding RET, not a return instruction. ZeroSlotThresholdGate captures
both same-instruction operands independently. DualThresholdRingLifecycle
derives the insertion pointers from the two fresh reads and preserves CMP2
flags through five ordered stores, stopping before the cleanup decrement.
IndexedCountTagLifecycle derives the tag-gate count and pointer from the
indexed classification, with independent late tag data and preserved flags.
RightPairedStores models the two owner-field MOVs at raw `0x012C92B7`,
including same-instruction partial operands and distinct ordered alias stores.
ZeroSlotStoreTail keeps EDX as the owner-field source and stops at its direct
cleanup jump. NodeFreshMaskLifecycle carries node-derived index/bit operands
into the fresh global gate, rejecting dirty parent handoffs. TagTraversalLifecycle
derives a single iteration from the unequal tag edge. RingCleanupLifecycle
admits an independent counter decrement only after both non-borrowing thresholds
and all five ring stores; its incoming carry is necessarily clear.
RightFieldStoresLifecycle derives the paired stores' ECX from the fresh field
and preserves the threshold's typed flags. ZeroSlotStoresLifecycle carries the
CMP operands into the four-store tail, separating bounded exits from clean
completion when late pointer captures remain unused.
SelfRingCleanupLifecycle and NodeSelfRingLifecycle compose the exact fresh-mask
zero edge into ordered self-ring stores; cleanup observes its counter independently
after the stores. ZeroSlotCleanupLifecycle similarly preserves the four-store
sequence and full compare flags when the counter read is missing. DEC preserves
the admitted clear carry, and neither cleanup destination nor call is entered.
TraversalZeroSlotLifecycle derives the threshold slot from the single child LEA,
rejects unused child-plus-four captures, and distinguishes internal threshold
entry from its unentered outer destination. RightFieldTagLifecycle admits both
clean right-field routes into the shared MOV/OR/tag stores and stops before
raw `0x012C92C8`, preserving parent flags until OR executes.
RingCleanupEpilogueLifecycle admits only the exact nonzero cleanup edge into
the existing bounded epilogue. Nullable late ESP/EBP and fresh TEST/stack/call
observations remain independent. A captured normal opaque-call return leaves
all arithmetic flags unknown rather than forwarding pre-call TEST flags.
TraversalSlotStoresLifecycle carries the derived child-slot ECX and original
traversal EDX into four ordered pointer stores and their cleanup jump.
RightTagRootLifecycle retains both right-field routes and independently
observes the root DWORD at VA `0x01EDADDC` after tag writes, including aliases.
NodeRingCleanupLifecycle observes the counter after initialized-node and
self-ring writes, preserves OR flags on a missing read, and separates clean
completion from a bounded bypass carrying unused counter data.
IndexedTagTraversalLifecycle carries the classified count and loaded pointer
through one tag comparison and one traversal iteration, without repeating the
fresh tag read at raw `0x012C93D8`. ZeroCleanupEpilogueLifecycle joins the
four-store cleanup's exact nonzero branch to the existing epilogue; missing
entry registers and later observations remain explicit, and a normal opaque
call return invalidates all six arithmetic flags.
NodeCleanupEpilogueLifecycle adds the same bounded epilogue to the exact
initialized-node/self-ring cleanup path. It retains node and counter evidence,
keeps zero-counter and nonzero-mask bypasses distinct, and preserves DEC flags
when the internal TEST read is attempted without a captured byte.
IndexedTraversalZeroSlotLifecycle derives the threshold ECX from the indexed
traversal's child-slot LEA. It rejects dirty zero-child handoffs before the
threshold read, retains fresh child/global observations even when they alias,
and stops before either threshold destination.
TraversalSlotCleanupLifecycle appends an independent counter observation to
the four-store traversal route, distinguishing reached instruction attempts
from execution when operands are missing. RightRootSuccessorLifecycle carries
consumed EDI into the root-equal global store/jump or the root-unequal
SHR-by-three size gate, without entering either helper's selected destination.
IndexedTraversalSlotStoresLifecycle extends the indexed count/tag zero-child
route through the four link stores and its cleanup jump. The child-slot
address supplies ECX, while the original loaded indexed pointer supplies EDX;
neither the indexed-slot address nor the captured zero child is substituted.
NodeMaskIndexedLifecycle connects the nonzero node-mask branch to independent
indexed and tag reads using the initialized node's index and slot. It also
preserves the zero-mask bounded exit and partial write/read observations.
TraversalCleanupEpilogueLifecycle joins the exact nonzero counter handoff to
the gated epilogue, retaining ordered effects and nullable stack captures.
Completion requires the observed RET boundary; a normally returning opaque
call makes all arithmetic flags unknown even if a later capture is missing.
RightRootIndexedMaskLifecycle carries the already-classified small-bin count
into the address-only LEA and mask gate without repeating size classification.
Its independent global read follows the earlier root effects, including
aliases; dirty unused captures affect clean completion, not selected bounded
exits at the opaque-call instruction or mask-branch destinations.
IndexedTraversalSlotCleanupLifecycle appends the independent counter
read/DEC/write/JNZ to the exact indexed four-store handoff. Earlier missing
indexed, compare, child-read, and store operands remain reached attempts,
not executed instructions; the cleanup branch destinations stay unentered.
NodeIndexedTraversalLifecycle extends the exact nonzero indexed-mask/tag
unequal handoff through one traversal iteration. EBX comes from the completed
tag shift, EDX from the fresh indexed pointer, and EDI from the original size.
It does not reread the tag or enter another iteration at the loop boundary.
IndexedCleanupEpilogueLifecycle extends the indexed counter's exact nonzero
handoff to the gated epilogue, preserving ten parent effects before the TEST
and independent stack/call observations. RightRootSmallBinRingLifecycle
extends the mask branches through their OR/ring or successor-threshold/ring
tails, deriving inherited ESI and preserving independent fresh thresholds.
Neither ring tail enters the selected epilogue or abort destination.
NodeIndexedTraversalZeroSlotLifecycle connects only the clean zero-child
branch to a fresh threshold CMP/JB. The LEA child-slot address supplies ECX,
missing threshold data preserves the prior TEST flags, and unused child data
blocks admission without erasing the already selected bounded exit.
NodeIndexedTraversalSlotStoresLifecycle extends the admitted not-below
route with four ordered stores and the cleanup jump, retaining original
indexed EDX rather than the zero-child value. SmallBinEpilogueLifecycle
admits either completed ring tail directly to the gated epilogue without
a cleanup-counter decrement; equal-root bypasses remain distinct.
NodeIndexedTraversalSlotCleanupLifecycle observes the fresh counter
decrement after the four-store jump, preserving carry and bounded
zero/nonzero destinations without entering the cleanup call or epilogue.
NetworkIoCompletionPassProjection observes one bounded dequeue pass:
at most one 500-ms fallback, one hundred counted attempts, independent
error/key/OVERLAPPED reads and opaque return observations. Missing stop
data or unused captures cannot claim clean completion; null dispatch
keys remain bounded rather than invoking an inferred callback.
GameSocketSenderBindingProjection covers the optional post-admission
flag branch through token selection, opaque queue append and flag clear.
It keeps disabled bypasses, missing operands and unused captures distinct,
and separates historical TEST flags from flags invalidated by normal calls.
GameSocketCompletionErrorStateProjection starts at the internal
post-log/release continuation: it observes compare-exchange state ten,
conditionally copies the fresh field and queues token two. It does not
model the outer zero-error bypass, concurrency execution or SEH cleanup.
CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleProjection
joins the indexed node cleanup counter to the shared gated epilogue.
The zero-counter path requires a separately observed normal helper return;
the nonzero path enters directly. Pending calls preserve preceding flags,
while normal opaque returns invalidate them. Unused captures spoil clean
admission without erasing an observed bounded child return.
NetworkIoCompletionClockGateProjection models the predequeue unsigned
500-unit clock gate, with independent global, field and saved-stack reads.
Its cached clock target is not reread from the IAT, and dispatcher calls
remain opaque. GameSocketReceiveResultGateProjection models the separate
receive-result BOOL/error-996 branch and dynamic slot-0x20 callback,
stopping before success preparation or common cleanup. The error import
is identified as WSOCK32 ordinal 111, not a named API.
GameSocketDatagramContextDispatchProjection continues the exact clean
sender-binding tail using a fresh context DWORD, not the earlier admission
classification. It records wrapped parser arguments and opaque parser
return observations. The nonzero path independently reads the high-water
DWORD before its unsigned compare and optional original-count store;
all three endpoints stop before native pops or return instructions.
CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleProjection
joins only the nonzero child's exact masked-equality fallthrough to the
existing dual-threshold ring tail. It preserves distinct outer equality,
zero-child and loop-back exits, inherits the child-derived EDX, and retains
fresh threshold reads before the five stores. Completion stops before DEC.
NetworkDispatcherLockGateProjection starts after the dispatcher's first
opaque import call. It separates the zero-result fallback call from the
direct path, observes a fresh counter increment, and compares a byte
argument before the frame and receiver-byte clears. INC preserves the
preceding carry flag, including an unknown carry after a normal opaque
return. Both branch destinations remain unentered.
GameSocketCompletionErrorQueueAcquisitionProjection joins the exact
pre-call error-state boundary to the existing queue acquisition prefix.
Captured acquisition results assume a normal opaque return; missing
results stop at its result store. The tail-tag read remains unentered.
GameSocketReceiveSenderPreparationProjection joins a clean nonzero
receive-result branch to the independent operation reload and sender
helper call boundary. A zero operation requires explicit diagnostic-stack
observations, including an independent stack pointer after normal return.
Neither projection executes the imported or direct callees.
CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleProjection
joins the exact five committed ring stores to the independent cleanup
counter read and DEC/JNZ gate. The zero path stops before its cleanup
call, while the nonzero destination remains unentered; neither path
claims epilogue completion.
GameSocketReceivePacketCallPreparationProjection observes a normal
sender-helper return before pushing its independent result and freshly
read stack count. It stops before the fixed packet call at raw BD2F25;
packet execution, reposting and return code remain outside the contract.
GameSocketParserLocalSizeDispatchProjection observes the fresh local
vtable and slot-C target after constructor return. It derives the resize
receiver from an independently captured post-getter ESP, not the earlier
local address, and stops before the resize call. Pending or nonnormal
getter observations preserve the incoming flags at the call boundary.
CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleProjection
joins the five-store cleanup counter paths to the shared bit-1 TEST
epilogue. Only an observed normal zero-counter cleanup return enters
that epilogue; unused return captures dirty clean admission without
erasing an actual bounded RET.
GameSocketCompletionErrorQueueAttemptLifecycleProjection joins a clean
nonzero acquisition to one generic append attempt. It keeps tag and tail
reads independent, compares returned EAX before EDX, and stops at retry
or pre-publication success. CMPXCHG8B changes only ZF; the preceding OR
provides the preserved non-ZF flags. No retry or publication is executed.
GameSocketReceivePostPacketStackPreparationProjection observes fresh
post-packet registers and an independent operation read before the
three stack stores. The nonzero destination and zero-path diagnostic
call remain unentered; this is preparation evidence, not a full repost.
GameSocketParserPostResizePointerProjection observes independent
post-resize stack and DWORD reads, preserves branch-specific TEST/XOR
flags through two stores, and records EDI as 0x56E. It stops before the
first header-byte read, including when the derived address is zero.
NetworkDispatcherCollectionGateProjection preserves the signed threshold
gate and opaque logging boundary, then uses independent end/start and
first-entry reads. Selected empty, zero-entry, and nonzero-entry
continuations remain unentered.
CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleProjection
composes root indexed equality with both fresh threshold reads and the
five ring stores. Unsigned bypasses remain distinct from clean completion;
the decrement at raw 0x12C942F remains unentered.
GameSocketCompletionErrorQueuePublicationLifecycleProjection admits only
a clean single-CAS success. It preserves fresh link, stack cleanup,
payload, and node reload observations and distinct capture boundaries.
Its generic return-value projection does not establish an actual native
RET or the outer queue call's normal return.
CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleProjection
admits the fresh counter decrement only after validating the root
equality path's five ordered stores. It preserves carry and stops before
the nonzero shared epilogue or zero-path cleanup call.
GameSocketReceiveOperationReloadGateProjection models the direct
continuation and the earlier diagnostic's explicitly observed normal
return. Fresh post-call registers and an independent operation reload
drive the stack store and TEST; it stops before the next diagnostic
call or the nonzero destination, without claiming a repost.
GameSocketParserFirstByteFieldsProjection records the first byte's four
bit partitions and two byte stores, exhaustively covering all 256 scalar
values. Final ECX contains the low three bits while flags retain the
earlier bit-3 AND result; the second byte remains unread.
NetworkDispatcherEntryCallPreparationProjection preserves first-entry
TEST flags through zero-entry argument pushes or a fresh nonzero vptr
read. It stops before reading either indirect target or entering its call.
CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleProjection
joins the root ring/counter path to the shared bit-1 epilogue. A zero
counter requires explicit normal cleanup-call return; pending calls
preserve DEC flags and do not count as completed epilogues.
GameSocketCompletionErrorEpilogueProjection observes segment-relative
FS restoration and independent stack pops only after a clean normal
queue-call return. Its combined trace preserves parent effects and
stops before RET4; neither return target nor EAX is inferred.
GameSocketParserSecondByteScalarGateProjection forms the 11-bit scalar
from an independent second byte and compares its adjusted low word
against 0x56E. All 2,048 values preserve the selected JA or fallthrough
boundary; this does not establish semantic length or parser acceptance.
GameSocketCompletionErrorReturnProjection adds an independent captured
return target and RET4 stack adjustment to the error epilogue. It records
only a bounded logical transfer, without entering that target or inferring
an EAX result.
NetworkDispatcherPostCallCountGateProjection projects fresh post-vtable
ESI, EBX, collection end and start observations. INC, SUB, SAR2 and unsigned
CMP/JB select an unentered reload boundary or opaque-cell boundary; no
collection count or register observation is forwarded across the call.
CapacityFinalCalleeCommonNodeIndexedTraversalSecondIterationProjection
adds exactly one loopback pass, deriving its node and doubled index from
the first pass and retaining the original size operand. Each child and
tag read remains fresh; equality, zero and another loopback are bounded
without entering a third pass.
GameSocketParserCallerCountGateProjection compares the admitted scalar
against an independently captured caller DWORD using a full-width CMP/JA.
GameSocketReceiveBufferStatusGateProjection independently loads the
operation base, writes status 0x80 and rereads the operation field. Its
four historical-route/reload-outcome combinations retain correct register
lineage, with new register observations required after the F7F diagnostic.
NetworkDispatcherCellReturnAddressGateProjection captures new registers
after the opaque cell return, stores its EAX observation and compares the
two wrapped LEA addresses. It stops before either the equal destination
or the fence fallthrough, without carrying registers across the call.
CapacityFinalCalleeCommonNodeIndexedSecondEqualityRingLifecycleProjection
joins a clean second-pass equality to the shared dual-threshold ring
helper. The fresh EAX link and original root ESI remain distinct through
all five stores, with threshold bypasses retaining their bounded exits.
GameSocketParserCountConsumptionStoresProjection records a fresh stack
DWORD RMW, pointer increment, count subtraction and two further stores.
The final flags come from a separate fresh byte comparison, not from the
earlier arithmetic; the next JZ remains outside the projection.
NetworkIoErrorCallbackPreparationProjection extends the actual
missing-dispatch-key parent boundary with a fresh key reload, error
argument push and independent vptr load. It stops before reading slot
0x20 or calling its target and retains the error CMP flags.
GameSocketReceiveRepostCallPreparationProjection records nine ordered
argument pushes and three independent fresh reads before the repost
import boundary. The last stack-address argument is entry ESP + 0x24,
not + 0x18; missing captures retain reached EAX/ESP and arithmetic flags.
CapacityFinalCalleeCommonNodeIndexedSecondEqualityCleanupLifecycleProjection
admits the existing DEC/JNZ helper only after the exact five ring stores.
Its fresh counter read remains independent of aliased prior writes; the
selected nonzero target or zero-path call remains unentered.
NetworkDispatcherFencedPublicationProjection records both fence/store
routes, an independent field reload, byte publication and fresh counter
DEC. It preserves incoming CF and stops before the next PUSH at BBDEAD;
skipped-but-supplied captures prevent clean completion.
NetworkDispatcherFinalCallPreparationProjection derives the final local
argument address from the proven post-cell EBP, independently reads its
DWORD and pushes it using a fresh ESP. It stops before the indirect call
at raw BBDEB0; neither target identity nor call behavior is inferred.
NetworkDispatcherFinalReturnSehStoreProjection independently captures the
target and normal return, then a fresh post-call EBP/local DWORD. It records
the DWORD store to FS segment offset zero and stops before BBDEC0 POP;
no linear address, callee identity, or ABI is inferred.
NetworkDispatcherRegisterRestoreProjection records four independent POP
reads and logical register/ESP updates, stopping before the frame reset at
BBDEC4. Missing captures preserve earlier restored registers and flags.
NetworkDispatcherFrameResetProjection resets ESP from the proven post-call
EBP, then independently reads the saved EBP and advances ESP by four.
It stops before RET4 at BBDEC7 and does not infer a return value or target.
NetworkDispatcherReturnTransferProjection captures the fresh return target
at the derived ESP and models the wrapping eight-byte RET4 adjustment.
The destination is a runtime address, not a raw file offset, and is unentered.
GameSocketParserStoredByteSelectorGateProjection derives EBX from the
first header byte and selects BD3294 or BD326A using the fresh stack-byte
JZ and ordered selector comparisons. Neither destination is entered.
NetworkIoErrorCallbackReturnProjection captures the indirect slot-0x20
target independently and requires an explicit normal return before the
BBC986 jump to BBC9BB. Missing or pending captures do not inherit the
parent's completed boundary; no callback identity or ABI is inferred.
NetworkIoErrorCallbackLoopGateProjection uses fresh post-call EBP/EDI and
independent local loads before INC/CMP and the signed JL. It preserves
unknown CF through INC, then computes the full comparison flags and stops
before raw BBC8F0 or BBC9CB.
NetworkIoErrorCallbackPostLoopByteGateProjection independently reloads two
locals around a fresh byte comparison. The second MOV overwrites EAX but
preserves CMP8 flags; the selected BBC8B0 or BBC9DB remains unentered.
NetworkIoErrorCallbackExitPopProjection records independent EDI/ESI stack
reads using fresh ESP, retains EAX/CMP8 flags and stops before the indirect
call at BBC9DD. Rejected ancestors retain their actual bounded endpoints.
NetworkIoErrorCallbackExitCallReturnProjection independently captures the
call-cell DWORD and explicit normal return. It invalidates arithmetic flags
only after normal return and stops before POP EBX at BBC9E3.
NetworkIoErrorCallbackExitEbxRestoreProjection uses fresh post-call ESP and
an independent saved-EBX DWORD for POP EBX at BBC9E3. It advances ESP by four
with wrapping arithmetic, preserves unknown flags, and stops before MOV ESP,EBP.
NetworkIoErrorCallbackExitFrameResetProjection requires fresh post-call EBP
for MOV ESP,EBP and independently captures the saved DWORD for POP EBP.
It distinguishes the frame-reset source from restored EBP and stops before
RET at BBC9E7; missing saved input retains the completed frame reset.
NetworkIoErrorCallbackExitReturnTransferProjection captures an independent
runtime target DWORD at the reset ESP for the near RET at BBC9E7. Successful
transfer advances ESP by four and has no raw-file stop address; the target
remains unentered and unvalidated.
NetworkDispatcherEntryFramePrefixProjection admits a clean pending dispatcher
call from the immutable clock-gate capture without rewriting its return status.
Fresh entry ESP/EBP and a segment-relative FS:[0] DWORD supply four stack
pushes and SUB ESP,0Ch. It stops before PUSH EBX at BBDDC4; rejected callers
retain their actual endpoint and leave entry captures unused.
NetworkDispatcherEntryRegisterSaveProjection extends the accepted frame with
fresh EBX/ESI/EDI pushes at BBDDC4..BBDDC6, retaining partial stack state and
all SUB flags. The cookie global read at BBDDC7 remains unentered.
GameSocketParserDiagnosticCleanupVirtualCallReturnProjection independently
captures the slot-zero DWORD at BD34CF and its opaque normal-return observation.
Pending and abnormal returns retain last-known flags; normal return stops
at BD34D1 with arithmetic flags and post-call registers unknown.
NetworkDispatcherEntryCookiePreparationProjection captures the operation-time
DWORD at VA 1DC5000, XORs it with the derived frame EBP and pushes the result,
stopping at BBDDCF. Cookie validity and ABI semantics remain unmodeled.
GameSocketParserDiagnosticCleanupPostCallAddressProjection requires independent
post-call ESI for LEA EAX,[ESI+8], preserving unknown arithmetic flags and stopping
at BD34D4 before the second locked XADD. It does not inherit pre-call EBX or ESP.
NetworkDispatcherEntryFsPublicationProjection derives EAX as frame EBP-0Ch
and records a logical DWORD publication to segment-relative FS:[0] at BBDDD2,
stopping at BBDDD8. It preserves ESP, XOR flags and the distinct earlier FS read;
no linear-address or exception-ABI interpretation is made.
NetworkDispatcherEntryLockPreparationProjection consumes fresh entry ECX,
derives EBX as ESI+6Ch and records the byte local, stack push and DWORD local
in order before BBDDE5. NetworkDispatcherEntryImportReturnProjection captures
the fresh target at VA 1AC4284 and explicit return independently; normal return
requires fresh EAX for a clean BBDDEB boundary and forwards no other registers.
GameSocketParserDiagnosticCleanupSecondXaddProjection requires fresh post-call
EBX and operation-time DWORD independently for the locked exchange at BD34D4.
Its DEC/JNZ suffix preserves ADD carry, decrements the exchanged old DWORD and
selects unentered BD34DB or BD34E2. Per-plan lazy validation preserves immutable
semantic checks without repeatedly reconstructing the same ancestry.
NetworkDispatcherEntryReturnedTestBranchProjection reuses the existing lock-gate
TEST/JNZ without supplying post-call receiver state, stopping at BBDDEF or
BBDDF6 before any push, call or counter read. The second parser virtual-call
preparation independently captures DWORD[ESI] at BD34DB, sets ECX=ESI and stops
before the slot-four call at BD34DF. Missing reads stop at BD34DB, not after MOV.
GameSocketParserSelectorDiagnosticPreparationProjection records the five
diagnostic argument pushes at BD326A..BD3280. It derives ESP from the
count-consumption lineage but requires fresh ESI and an independent DWORD
at ESI+110 for the fifth push, preserving the first four pushes if either
capture is missing. The call at BD3281 remains unentered.
GameSocketParserSelectorDiagnosticReturnCleanupProjection records an explicit
opaque-call return observation and requires fresh post-call ESP for ADD ESP,14h
at BD3286. Normal return invalidates earlier arithmetic flags; the ADD derives
all six flags and stops before the DWORD increment at BD3289.
GameSocketParserSelectorDiagnosticCounterContinuationProjection requires
fresh post-call ESI and an independent DWORD at ESI+EC. Its wrapping INC
preserves CF from the preceding ADD, leaves ESP unchanged, and follows the
unconditional jump to raw BD3473 (code VA FD4073) without entering that target.
GameSocketParserDiagnosticDestinationPrefixProjection captures fresh post-call
EBP and independent DWORDs at EBP+C and derived ESP+14. CMP with 2 replaces
arithmetic flags; MOV EDI,56Eh and MOV EDX preserve them. It stops before
JAE at raw BD3480 and retains the committed EDI load if the EDX read is missing.
GameSocketParserDiagnosticDestinationBranchGateProjection uses only the
comparison carry flag: JAE selects raw BD31F8 for unsigned values at least 2;
otherwise the short jump selects BD34AD. Both paths preserve registers and
flags and stop before executing their selected target.
GameSocketParserDiagnosticCleanupStackPrefixProjection records the DWORD
store of FFFFFFFF at derived ESP+50, then independently reads ESP+3C into
ESI and applies TEST ESI,ESI. Missing input retains the committed store;
successful TEST leaves AF unknown and stops before JZ at BD34BB.
GameSocketParserDiagnosticCleanupZeroBranchProjection preserves the TEST
flags and selects BD34E2 for zero ESI or BD34BD otherwise. It adds no
memory effects and executes neither selected target.
GameSocketParserDiagnosticCleanupRegisterPreparationProjection derives
EBX and ECX as FFFFFFFF from OR EBX,-1 and MOV ECX,EBX without requiring
the prior EBX value. LEA EAX,[ESI+4] wraps without dereferencing memory.
The prefix preserves ESP and stops before LOCK XADD at BD34C5.
GameSocketParserDiagnosticCleanupXaddProjection records an operation-time
DWORD capture for LOCK XADD [EAX],ECX. It projects the wrapping sum with
FFFFFFFF, places the old DWORD in ECX, and derives ADD32 flags. This is a
bounded logical read-modify-write, not a concurrency simulation, and stops
before JNZ at BD34C9.
GameSocketParserDiagnosticCleanupXaddBranchProjection preserves the captured
RMW result and ADD32 flags. JNZ selects BD34E2 unless the old DWORD was 1,
in which case its zero result selects the fallthrough at BD34CB. Neither
destination is executed by this projection.
GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection reads a
fresh DWORD at derived ESI into EAX and sets ECX to ESI. The read remains
independent of earlier aliased stores; missing input preserves prior registers
and flags. It stops before the indirect slot-zero call at BD34CF.

`FrameHandoffCapacityAboveBranchLifecycleProjection` derives its tail
return-gate capture only from a clean above-capacity lifecycle.
`FrameHandoffCapacitySecondBranchLifecycleProjection` forwards only an
admitted second-transfer return gate into the first/fill/final chain.
Both retain earlier writes and pushes when downstream evidence is missing
or unreached; supplied later captures cannot manufacture successful admission.
