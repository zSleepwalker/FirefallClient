# Startup: frame handoff capacity

`FrameHandoffHelperEntryProjection` preserves independent receiver+0C/+10 reads, wrapping sum and low-bit gate, then compares receiver+8 unsigned against `(receiver10+4)>>2` with DWORD wrap before the shift. Both bypass branches join `BE64C5`; otherwise receiver10 is pushed and opaque `BE6B50` called. Missing reads preserve earlier captures and stop at their instruction, while missing/nonreturn call evidence stops at `BE64C0`. No receiver writes occur in this window.

`FrameHandoffHelperSlotProjection` models the `BE64C5` continuation's mask write to receiver+0C separately from its fresh reload. Independent receiver+8/+10/+4 and indexed-slot reads determine the zero fallthrough at `BE64F2` or nonzero branch to `BE650E`. `UnreliableReceiveHighCountLifecycleProjection` composes successful result forwarding through callback/counter/shared return; counter receiver mismatch and unreached callback captures block counter entry before writes.

`HighCountObjectHelperReturnProjection` models `BE6458..BE6477`: independent reads at helper-EAX+8 and retained-ESI+8/+4, wrapping decrement and masked index, indexed DWORD reload, then full-EAX address arithmetic and plain RET. Missing register prerequisites mark supplied dependent reads unreached; zero register bases remain captured values, not absence.

`FrameHandoffHelperEntrySlotProjection` admits the three exact unflagged `BE64C5` entry routes and checks receiver identity before slot reads/writes, without equating independent reload values. `FrameHandoffCapacitySeedProjection` records the receiver store at frame-14 before the receiver+8 read, seeds ESI with one, and replaces that seed only for a nonzero read before the `BE6B70` join.

`HighCountObjectHelperLifecycleProjection` joins the helper entry and indexed return, independently loading `[first loaded DWORD]` at `BE6456` only on the nonzero route. The zero route supplies retained ESI zero and rejects any nonzero-only bridge capture before tail effects. Optional return register identity captures are checked only after entry/bridge prerequisites; the composed EAX/ESI values come from the entry and bridge.

`FrameHandoffEmptySlotCallProjection` preserves the push-0x10/direct-call path and zero-result indirect call through cell `1AC4424`, whose returned ECX is independently captured rather than assumed zero. Missing direct EAX stops at the copy instruction before cleanup/test. `FrameHandoffSlotPublicationProjection` separates the `6508` indexed store from the `650E` fresh-base read, then masks EDI, forms ECX and pops EDI/EBX before the test at `651C`; its EBX input is the byte offset computed by the preceding slot projection.

`FrameHandoffFinalWriteReturnProjection` tests ECX, conditionally reads a pointer at frame+8 and then its pointee DWORD before writing to ECX, and independently increments receiver+10 with DWORD wrap before RET4. `FrameHandoffCapacityLoopProjection` projects one iteration only: CMP of wrapped ESI-EDX against one governs the zero-difference branch, followed by the unsigned ESI/eight exit or unsigned `(0FFFFFFF-ESI)<ESI` failure gate; otherwise ESI doubles and selects the loop entry.

`FrameHandoffCapacitySeedLoopProjection` derives a single iteration's ESI/EDX from the seed and validates optional identities before projecting it. `FrameHandoffSlotCallPublicationProjection` carries native receiver ESI, slot byte offset EBX and wrapping sum EDI into publication; nonzero-slot routes skip calls/stores, while zero-slot routes require unflagged call results. `FrameHandoffPublicationReturnProjection` validates formed ECX and receiver ESI before final reads, writes and increment.

`FrameHandoffContinuationLifecycleProjection` uses the effective call-derived publication capture to compose the complete `BE64C5..BE652F` continuation. `FrameHandoffCapacityPrefixProjection` preserves the receiver+C read, ordered frame-8/frame-4 stores and unsigned scaling gate before the call. `FrameHandoffCapacityCallProjection` models cleanup4 before the returned EAX frame store, the nonzero return gate and independent indirect-call return; invalid entry routes and unreached captures cannot authorize successful admission.

`FrameHandoffCapacityTransferProjection` models `BE6BCA..BE6BFD`: signed SAR, independent frame reads, ordered scalar pushes and an opaque indirect-call normal boundary. Its admission rejects unreached later captures. `FrameHandoffCapacityTransferReturnProjection` models the fresh receiver read before cleanup12, wrapping addition of independently captured return EAX and entry EDI, independent frame-8 reload, frame-10 store and unsigned branch to `BE6C10` or `BE6C57`; it makes no callee-preservation claim.

`FrameHandoffCapacityPrefixCallProjection` derives zero bypass, guarded indirect entry or direct scaled-push entry from the prefix, rejects conflicting route captures and validates the frame identity before call effects. Completed prefix stores and pushes remain visible when a later call is blocked.

`FrameHandoffHelperLifecycleProjection` composes the three admitted entry routes through the full frame continuation, validating receiver identity before slot effects. `FrameHandoffCapacityTransferLifecycleProjection` validates receiver/frame identities before the first transfer return reads, but retains independent post-call EDI/ESI and opaque EAX captures. `FrameHandoffCapacitySecondTransferProjection` models `BE6C10..BE6C34`, including signed scaling, the frame-C store, independent call-cell/frame-10 reads and ordered pushes before the opaque call.

`FrameHandoffCapacitySecondReturnGateProjection` preserves the fresh frame-C read before cleanup12 and the independently captured post-call EBX/EAX addition. Its lifecycle composition validates EBP without assuming EBX preservation. `FrameHandoffCapacityPrefixTransferProjection` requires clean prefix-call admission, retaining independent post-call EDI on call routes and deriving it on the zero bypass. `FrameHandoffCapacityAboveTransferProjection` preserves the first push before the call-cell load at `BE6C64`.

`FrameHandoffCapacityRepeatStoreProjection` represents `BE6C41..BE6C4A` with one compressed zero-store descriptor, explicitly captured DF and normal REP completion. It does not assume forward direction or enumerate writes; a missing fresh frame-8 reload retains completed REP effects.

`FrameHandoffCapacityLoopPrefixProjection` admits only the exact unsigned exit of one loop iteration, deriving prefix ESI/EDX without running additional iterations. `FrameHandoffCapacityFillJoinProjection` independently reloads frame-4 before selecting the final-repeat or direct-tail route. `FrameHandoffCapacityGateRepeatProjection` derives nonzero REP inputs from the return gate while rejecting repeat-only captures on the zero bypass.

`FrameHandoffCapacityAboveSecondTransferProjection` preserves independent receiver/frame reads, signed scaling and pushes through the second above-branch call. `FrameHandoffCapacityAboveReturnGateProjection` preserves cleanup24 before the returned EAX addition and fresh frame-4 reload, without assuming post-call EBX/ESI preservation.

`FrameHandoffCapacitySharedRepeatProjection` models the separate `BE6C9E..BE6CA2` compressed directional zero store without a subsequent frame-8 reload. `FrameHandoffCapacityFinalCallGateProjection` independently reloads frame-14 and receiver4 before the optional direct call, exposing outgoing EDI only on the no-call bypass. `FrameHandoffCapacityFinalReturnProjection` preserves the ordered receiver8 ADD, receiver4 overwrite and RET4 without claiming a full EAX result.

`FrameHandoffCapacityFinalLifecycleProjection` admits the return tail only
after the exact clean final-gate outcome. It validates the shared frame and
the no-call EDI identity before the receiver8 read; opaque-call register
captures remain independent. `FrameHandoffCapacityAboveLifecycleProjection`
preserves both calls and the return gate's cleanup-before-LEA ordering,
requiring normal call boundaries and a matching frame for the final reload.
`FrameHandoffCapacityFillSharedRepeatProjection` derives repeat count and
start from the fill join's moves, retaining zero bypass, unresolved DF,
explicit normal completion, and partial plans without claiming stores.

The final direct callee at raw `12C8D60` does not establish unconditional
register preservation. Its exit window `12C9441..12C945C` tests mask `0x02` of the
byte at encoded PE VA `1EDAF84`, restores EDI/ESI/EBX, then branches over an
indirect call through encoded PE VA `1AC427C` when that bit is clear.
The call at raw `12C9452` occurs after the restores when the bit is set;
its register outputs remain unresolved. This is not a whole-callee contract.

`FrameHandoffCapacityRepeatFillJoinProjection` links the first completed
REP/fresh-frame8 reload or exact zero-count bypass to the fresh frame4
fill join. The REP route derives EDI from the independent reload; the
bypass requires an independent EDI capture. Both validate frame identity
before the fill read and retain completed store descriptors when a later
reload is missing.

`FrameHandoffCapacityAboveRepeatProjection` admits shared REP only from the
exact nonzero above-return gate, deriving count/start and treating optional
identity captures as cross-checks rather than missing prerequisites. Its
zero branch rejects repeat-only captures; missing DF and incomplete REP
retain partial effects and cannot admit the final join.

`FrameHandoffCapacityFillFinalLifecycleProjection` derives final-gate
admission only after the clean fill/shared-REP route and validates its
frame before the fresh frame14 read. On the final no-call bypass, EBX
must agree with the earlier fill frame4 reload before any final writes;
the called route retains independent register captures.
`CapacityFinalCalleeExitProjection` models only raw `12C9441..12C945C`,
with ordered TEST/pop/gated-call/epilogue captures. Encoded data operands
are PE VAs, separate from raw instruction offsets; restored values are
not promised as outputs of the subsequent opaque call.

`FrameHandoffCapacityFirstFillLifecycleProjection` derives its return-gate
plan from the admitted second-transfer lifecycle, so supplied later plans
cannot bypass a failed call. `FrameHandoffCapacityAboveFinalLifecycleProjection`
composes the above return/REP route through RET4, validating EBX and ESI
continuity only on the final no-call bypass and propagating unreached gate
captures. The final callee's raw `12C8D60..12C8D78` entry anchor confirms
zero ECX jumps directly to the epilogue after frame allocation, bypassing
the global mask read and EBX/ESI/EDI saves; nonzero behavior remains bounded.

`FrameHandoffCapacityFirstFinalLifecycleProjection` preserves all eight
first-gate/fill-branch/final-call combinations. It derives the downstream
fill capture from the admitted first fill plan, retains partial first REP
effects, and does not invent a missing ESI continuity contract.
`CapacityFinalCalleeEntryProjection` records frame allocation and ordered
stack stores with DWORD-wrapped addresses. Its zero path requires an
independent saved-EBP reload before RET; its nonzero path stops after the
three register saves, with no claim about the subsequent call.

`FrameHandoffCapacityOverflowTailProjection` keeps loop overflow distinct
from the unsigned size guard: raw `BE6CC0` pushes encoded VA `1C607A4`,
then calls through encoded VA `1AC45C0`. A captured normal return ends at
`BE6CCB`; neither nonreturn nor an epilogue is inferred from the call.
The separate callee-prefix and first-validation anchors pin the mask-gated
call at `12C8D82` and the fresh global/ESI4 reads and unsigned/tag branches
at `12C8D88..12C8DA7`.

`FrameHandoffCapacityLoopOverflowProjection` admits the overflow call from
only an exact taken JB in one bounded loop iteration, distinguishing a
reached-but-not-taken guard from an earlier exit.
`CapacityFinalCalleePrefixCallProjection` derives pre-call ESI with wrapping
subtraction, uses the prior mask-test flags without a new global read,
and exposes outgoing ESI only when the opaque call is bypassed.
