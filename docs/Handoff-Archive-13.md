# Reverse-engineering handoff archive 13

## Continued: Pooled acquisition and singleton publication boundaries

Length callback follow-through review found missing partial stack state:
length is already pushed before unresolved payload+8 or payload+4 reads.
Preserved that single push and added assertions; pinned runner passes.
Luna `5771a700-1616-43d5-b041-eacd07257703` owns unreliable residue/counter/
joined-header composition; `74b92c04-9b25-4b67-93f4-2b1671d1159c`
read-only traces BD58E0 register read widths, searching prior art first.

Pushed accumulated immutable anchors and length partial-push correction
as `06ab2ba`. Occupied-slot composition reviewed/registered and passes;
fixed a fixture expecting Missing instead of UnreachedPayloadWordCapture.
Sole immutable writer `2c2daee2-e3c4-49e2-873f-0048598b1297` owns unreliable
marker/scalar callback windows. Luna `1e3c4f4c-3a48-43a9-99c6-4fb60aa0781d`
owns unreliable scalar callback code/checks. Luna
`a1d9c809-cdf8-4092-8542-5942a3275dd6` owns minimal reliable zero-header
empty-slot publication composition; nonzero callback stops at result boundary.

Pushed occupied-slot composition as `465814b`. Nonzero-marker callback
reviewed/registered and passes. Zero-marker trace confirms receiver+14
compare, optional diagnostic, then independently reloaded receiver+14 target:
Luna `b095b705-1b54-4806-9358-1cda3678b243` owns presence/diagnostic gate,
`655aabf2-006a-449d-9354-eb28abce343a` owns four-push direct callback,
`516a7cd2-4c67-4ca8-8011-d0a0b5a96b86` traces normal continuation `BE5B64`.

Pushed marker callback as `96e17d6`. Unreliable scalar callback
reviewed/registered and passes. Sole evidence writer now
`5d883aa3-81c2-494e-bfcb-96064f234a19` owns zero-marker presence/direct
callback anchors, preserving completed pending marker/scalar anchors.
BD58E0 prior-art trace found XOR core already modeled; narrower read-only
owner `148e6c4c-2cf8-429b-b5a4-00170c286e0a` verifies its diagnostic guard
and post-call reloads before any new native-boundary projection is assigned.

Pushed unreliable scalar handoff as `9b30dff`. Residue-join composition,
reliable empty-publication composition, unreliable callback presence/direct
code now reviewed, registered, passing the pinned runner.
Luna `7ae3b8a7-9123-49a0-9b8b-ad68491ad34c` composes presence gate and
independently reloaded direct callback target, without forcing target equality.

Pushed reviewed joins/direct callbacks as `28ced0e`. Successor refinements:
length callback's entry flag is renamed to boundary validation, since a
normal zero-result diagnostic also falls through cleanup to `BE53BF`.
Luna `8c70c4c4-2490-48aa-a28f-1df6dec47335` composes result/cleanup/length.
Likewise `BE5B32` is reached by zero marker OR normal nonzero-marker scalar
continuation; presence author is changing zero-only marker gate to explicit
boundary validation, and presence-composition author notified.
Luna `3762ac38-ad8b-4ba6-8653-d5cdbf4f238f` composes marker/scalar.
Luna `e5867f3c-327a-41af-9cc1-8a40c59a71b2` owns BD58E0 diagnostic-entry
projection; sole evidence writer `0f3b12cb-5838-4f41-b1b1-192a508553cd`
pins its entry window, preserving all pending unreliable callback anchors.

Direct-callback continuation raw IDA confirms `[BE5B64,BE5B78)`:
global read, cleanup16, ESI=callbackEAX, frame+8 write, payload push,
call BD5C60. Luna `3fd602d1-4fe9-4d7c-a670-e6c533b7e550` owns it.
Luna `e2a7a633-9f01-4abe-94e6-b3204112b7dd` owns TEST ESI / zero-result
forwarding handoff `[BE5B78,BE5B9E)`. Nonzero-result stores and unsigned
receiver+4C threshold `[BE5BAC,BE5BC4)` owner is
`a3c3722a-0c1f-45ae-acea-20edf5b3dbb4`; independent frame-4 reload
precedes both result+20/+24 stores.
Read-only Luna `e3a87479-5439-4484-ba2f-f3594f7d7121` traces XOR helper
post-diagnostic length/table boundary starting `BD5912`, stopping before loop.

Boundary API corrections and marker/scalar, presence/direct, result/length
compositions are reviewed and registered. Coordinator corrected nullable
direct-plan test member access. Sole evidence writer now
`309093e3-ef01-40f8-8d97-80a79f4bb6f5` for unreliable result continuations.
Zero-result return tail `[BE5B9E,BE5BAC)` owner:
`0c3bd73d-e266-403a-95a8-cafe74c8b467`.

Pushed reviewed callback compositions/boundary corrections as `b9eb465`.
New selector join `[BE5AF0,BE5B64)` owner
`d3e6c937-fe2a-4621-956a-8338d2228e3c`; reliable length/scalar/publication
join owner `5a8ee11f-7a89-40ab-85f8-94e634ce8463`.
Recycle bridge, zero-result forwarding, zero-return epilogue, and XOR
diagnostic entry are registered pending final validation. Zero-result author
is fixing positional-argument compile errors and nonreturn stop boundaries.
Result-store author is removing an invented receiver-nonzero gate and
correcting MOVZX/store instruction offsets before registration.

XOR setup trace confirms physical `[EBP+8]` length operand (IDA's stack-frame
offset `14` is not the physical displacement), then AL zero-extension,
wrapped EBX+length end, and two table DWORD reads at VA `1BB7490/94+AL*8`.
Both table reads precede empty/wrapped-end unsigned JNB. Owner
`10edbe70-2208-4673-94a7-b062d65748f6` models `[BD5912,BD5939)`.
Sole evidence writer `df73a305-4d2f-42cb-bc4a-570c5037e24e` appends that
window and zero-return tail, preserving all preceding anchors.
Read-only below-threshold continuation trace owner
`d3bd75ea-3d38-4b94-a0e9-4427ae7528d6` starts `BE5C0F`.

Below-threshold trace completed; raw evidence shows shared frame-address
handoff/return `[BE5C0F,BE5C26)`, LEA frame+8 push, LEA receiver+3C ECX,
call BE64A0, AL-only 1 epilogue. Trace's mapped VAs were incorrect:
correct entry/end are `FE680F/FE6826`. Code owner
`575ce81a-cfc2-4ba6-b079-06853506c362` has the correction.
High-count cleanup `[BE5BC4,BE5C0F)` read-only owner
`9d5b48be-6cb3-48dc-babf-7d53b34ef560`.
Presence/direct-to-recycle composition owner
`31199ed5-b488-4e8b-87c2-6db65b9c919e`.
XOR epilogue `[BD594E,...return)` read-only owner
`7b24813e-525e-4fed-af8f-7d8e43421a65`.

Result-store refinement is complete/reviewed/registered: only zero result
invalidates entry; receiver zero is allowed. MOVZX and result+24 store have
separate correct offsets `BE5BB5/BE5BB8`.
Evidence writer `e6ecff04-4249-4a95-aacc-2bf50629fd3f` now owns only the
shared frame-address handoff anchor; preceding anchor writer released.
Zero-result forwarding/return composition owner
`ee68218e-d8d1-4eaa-851f-4c0c58cb900a`.

Pushed reviewed receive result tails, selector joins, and XOR entry/setup code
as `8e06a58`; pinned runner passed. XOR epilogue and zero-forward/return join
now reviewed/registered and pass with all accumulated immutable anchors.
Successor owners: `5f84eaaa-9cbb-4e2f-af96-e05e47249b60` composes diagnostic
entry/table setup; `fc326a45-be57-48fe-a6e4-6d46fb626554` composes result
stores/below-threshold shared return. No active evidence writer after
`bedcbd43-2e18-4b05-b95c-e64c9eaa5a8d` released.

Pushed accumulated immutable anchors, XOR return, zero-forward return join as
`f363e70`. High-count forwarding/callback/counter pairs now reviewed,
registered, and pass the pinned runner. Coordinator corrected first-call
missing/nonreturn stops to `BE5BDB` and a wrap fixture: `FFFFFFB4+4C=0`.
High-count callback/counter composition owner
`d52247b7-0faa-49cc-8d79-cabaa1d5ae55` must reject flagged zero-path
receiver+48 captures before admitting the join.
Sole evidence writer `4dc1f0ab-5cfc-4b73-9601-2db1bc7f4e69` pins suffixes.
Read-only helper/prior-art traces: BE64A0 owner
`a686b2f8-a5a5-46e2-8ef3-09286a0d317a`; BE6440 owner
`b1f34702-0c12-4d52-9bae-1d0834046999`.

Pushed reviewed high-count forwarding/callback/counter pairs as `a7b555f`.
XOR entry/setup and below-threshold return compositions reviewed/registered;
validation currently waits on high-count suffix author's same-task local
shadowing compile fix. `385035ec-b4de-43f6-90c0-ce50f3aeee93` owns the
result-stores/high-count-forward composition.
Suffix immutable anchors completed/reviewed; no active evidence writer.

Entry/setup, below-threshold return, and high-count suffix compositions pass
the pinned runner after coordinator fixture fixes (matching frame in partial
XOR table capture; invalid counter gate with no later counter capture).
Suffix selected destination now denotes clean completed join `BE5C0F`,
not predecessor callback boundary `BE5BFE`.
Read-only helper traces completed: BE64A0's first block gates an opaque
BE6B50 call on low-two-bit sum and unsigned capacity arithmetic;
BE6440 passes local frame-C address to BE6B30 then reads its EAX result.
Code owners `efca7624-c0c8-408c-8b37-66d62ac231b3` (BE64A0 entry) and
`05a883e5-b613-4dc1-96ef-7238a683196a` (BE6440 entry).

Pushed reviewed conditional joins/suffix anchors as `bb1f3be`.
High-count result/forward composition reviewed/registered and passes; fixed
its partial-capture fixture and final selected destination to `BE5BF1`.
High-count suffix/shared-return owner
`2295d27f-9730-4b53-90ca-ab86cfb40a79`.
Sole helper-entry evidence writer
`2699cc1c-225a-4611-9407-16f8ab518f75`.

Pushed high-count result forwarding join as `566ab69`. Current active work:
frame helper entry `efca7624-c0c8-408c-8b37-66d62ac231b3`, object helper entry
`05a883e5-b613-4dc1-96ef-7238a683196a`, high-count return join
`2295d27f-9730-4b53-90ca-ab86cfb40a79`, serialized evidence writer
`2699cc1c-225a-4611-9407-16f8ab518f75`, and read-only frame-helper
continuation trace `fa0b7940-fab7-4ed9-9ee4-76d659bb985d` from `BE64C5`.
All use GPT-6 Luna. Do not stage their files until completed and reviewed.

Object-helper entry stop correction and high-count shared return reviewed,
registered, passing; coordinator corrected a fixture claiming an unreached
EAX capture when its value was absent. Shared-helper entry anchors released
and reviewed. Frame-helper entry author is still refining missing-read
boundaries/partial read metadata and call-site stops; do not stage its pair.
New read-only capacity-callee trace `4b7ad275-30b9-45f6-a83d-5f04b1d0380b`
starts `BE6B50`. Object-helper shared continuation read-only owner
`9df91d6b-ac9c-4cea-8d03-1bfd00496496` starts `BE6458`.

Frame-helper entry refinement released, reviewed, registered, passing.
Coordinator corrected wrap fixture to put FFFFFFFC in the actual +10
operand. Object-helper continuation trace finished: BE6458..6477 reads
helper-EAX+8, retained-ESI+8 and +4, then indexed DWORD and returns full EAX;
zero bases remain legal captures. Implementation owner
`fa99b242-1cf8-4389-ad60-39a14e7d3687` owns new ObjectHelperReturn pair.
Frame-helper BE64C5 slot-gate implementation owner
`57bde316-a7a9-4698-a2be-8cfdc6af3472` owns new FrameHandoffHelperSlot pair.
Full high-count lifecycle composition owner
`8f935e49-06a8-407d-9b82-b6ac2e929234` owns new Lifecycle pair.
All are GPT-6 Luna; coordinator retains Program/docs/evidence ownership.

Latest pushed HEAD `0b0f9a0` includes corrected helper entry;
`f42c604` includes object-helper entry, high-count shared return, and helper
entry immutable anchors. Continuation evidence sole writer is now
`28d19084-76d5-4b58-af52-c32d0748126d`; await release before staging.
Capacity trace `4b7ad275-30b9-45f6-a83d-5f04b1d0380b` reported an apparent
mapped-VA query rather than flat raw BE6B50. Coordinator requested raw-only
byte/disassembly correction; its SEH/receiver+2C report is not accepted
evidence and must not drive implementation.

Corrected raw-only capacity trace now verified: BE6B50 seeds ESI=1,
reads receiver+8 into EDX, copies nonzero EDX to ESI and joins BE6B70.
Seed implementation owner `77e7efa5-3d4e-44a0-b45c-56e6fbdd07b5`;
its new CapacitySeed pair is not yet reviewed. Discard earlier SEH trace.
Slot gate and full high-count lifecycle reviewed/registered/passing.
Coordinator corrected indexed address fixture (base 5 + index*4 = 9,
not receiver+9), removed unreached fixture captures, and made lifecycle
callback admission reject all unreached flags explicitly.
Continuation anchors released and verified. ObjectHelperReturn author
is refining missing register/read capture flags; do not stage its pair.
Read-only empty-slot continuation owner
`544c0b47-1d4f-4237-9c9a-ff767051e92e` starts BE64F2.

Latest pushed HEAD `6470a19` includes slot gate, complete high-count
lifecycle, and both continuation immutable anchors. Entry/slot composition
owner `dfb89016-ea41-4076-a6e3-40da502abe2b` owns new EntrySlot pair only.
No active evidence writer. Coordinator is awaiting ObjectHelperReturn
refinement and CapacitySeed/empty-slot continuation completion before
reviewing their released artifacts.

ObjectHelperReturn refinement released, reviewed, registered and passing;
its missing address prerequisites now flag supplied reads unreached.
Object-helper entry/return composition owner
`c6cf04d5-1a2b-4d2d-925d-8b607473291e` owns new ObjectHelperLifecycle pair,
including independent DWORD reload at BE6456 on the nonzero branch.

Latest pushed HEAD `af31dbd` includes object-helper indexed return.
Seed author is adding the native BE6B5F frame-14 receiver store and
correcting mapping wording; do not stage CapacitySeed pair yet.
Empty-slot trace completed: F2 call prefix joins6508 with direct nonzero
ECX or independently captured ECX after indirect call (not assumed zero).
6508 reloads receiver4 and writes indexed slot;650E freshly reloads4
and reads indexed slot, then masks EDI3 and forms ECX.
EmptySlotCall pair owner `2c02fa0e-8c5f-45c4-be5f-3c1dd1f3fe90`;
SlotPublication pair owner `3cf19304-4cb6-43cb-bc63-a3ca911add8b`.
Sole evidence writer `2d663193-e14e-4c28-ba5a-a47270a58ec9` adds seed,
call-prefix, publication anchors. Await release before staging evidence.

EntrySlot and CapacitySeed pairs released, reviewed/registered/passing.
Coordinator fixed identity-mismatch fixture that simultaneously required
receiver capture both reached and unreached. Capacity seed now includes
native frame-14 store and true PE raw+400C00 mapping. New read-only
loop trace `55cfa08f-a6f3-495a-adaa-48bc4b0af2c8` starts BE6B70, stops
BE6B91/failure branch and verifies exact compare operands before any
loop implementation.

Latest pushed HEAD `e022868` includes CapacitySeed and EntrySlot pairs.
Seed/call/publication anchors released, reviewed; coordinator clarified
6508 is the reload before write and 651C window includes POP EDI/EBX.
No active evidence writer. ObjectHelperLifecycle author correcting
flagged-zero-bridge admission and prereq identity gating; do not stage.
Final frame-helper tail read-only owner
`d1e7e2cf-b347-49d3-89dd-c60ae19b0400` starts651C through actualRET4.

Latest pushed HEAD `f13baee` includes seed/call/publication immutable
anchors. ObjectHelperLifecycle refinement released, reviewed/registered,
passing. EmptySlotCall author correcting PUSH evidence6A0A→native6A10
and missingEAX partial steps (stopF9 beforecopy/cleanup/test).
Final tail trace verifiedTWOoptionalreads: [EBP8] at6520 then[pointer]
at6523 before[ECX]write6525, INC[ESI10]6527, RET4 ends652F.
FinalWriteReturn owner `862f36dc-78fe-4ef1-bb75-a21a0c28373c`;
CapacityLoop one-iteration owner `02839568-aecc-49e8-ba1e-6a242928f9f7`.
Loop CMP(EAX,1), not SUB carry, admits zero-difference growth route;
failure gate is unsigned(0FFFFFFF-ESI)<ESI. No frame argument read.

Latest pushed HEAD `50999bc` includes strict ObjectHelperLifecycle.
EmptySlotCall/SlotPublication pairs released, reviewed/registered/passing.
Coordinator fixed still-unmodified PUSH evidence to6A10, absentcapture
fixtureflags, and wrap addressfixtureFFFFFFFC+FFFFFFFE=FFFFFFFA.
New SlotCallPublication composition owner
`88078855-a11b-4710-8f94-0cb8e38a6287` owns pair only. Critical native
identity: publicationEBX is slot.FirstSlotByteOffset, not arraybase;
publicationEDI is wrapping receiver10+freshreceiverC;ESIreceiver.

Latest pushed HEAD `f4e8ac7` includes EmptySlotCall/SlotPublication pairs.
Evidence sole writer `fd86aba2-b88d-404f-851d-acd2f8389948` adds final
tail and capacity loop anchors; await release before staging.
Read-only capacity prefix trace
`1dce1408-cf28-42f6-8eec-786e46037db3` startsBE6B91 stopsBE6BCA.

FinalWriteReturn and CapacityLoop pairs reviewed/registered/passing.
CapacityLoop selecteddestination70 differs from exclusivewindow91
on doubling route intentionally; this is a single iteration, not a loop
runner. Publication→final return composition owner
`d88a3db0-045b-4012-bec7-0b04199a70a0` owns new PublicationReturn pair;
must validate formedECX/receiverESI before finaleffects and reject all
publication unreached flags.

Latest pushed HEAD `cade0fe` includes FinalWriteReturn/CapacityLoop.
CapacitySeedLoop one-iteration composition owner
`ddf47521-7de6-4afd-8d4c-57479a11b973` owns pair only. Remaining active:
SlotCallPublication88078855, PublicationReturnd88a3db0,
evidencefd86aba2, capacityprefixtrace1dce1408. Use exact full IDs above;
wait for their notifications, do not poll or duplicate their scopes.

Final/loop evidence released and reviewed. Coordinator corrected final
tail VA range +10 error: raw651C..652F mapsFE711C..FE712F, not
FE712C..FE713F. Pinned runner passes. No active evidence writer.
Capacity prefix trace completed: independentreceiverCread, ESI-=EDX,
frame-8shiftedCstore andframe-4zero, zero/unsigned3FFFFFFFguard before
scaledpush/call; directreturncleanup4 beforeframe-4EAXstore/test.
Prefix implementation owner `5e030654-4305-43f5-9905-4601430e25c4`
owns new CapacityPrefix pair only throughbeforecallB5.
Post-call read-onlytrace owner
`ee9585e1-6267-4b8b-ae63-c552c9250ed8` startsBE6BCA.

Latest pushed HEAD `036eecb` pins final/loop native anchors.
CapacitySeedLoop, SlotCallPublication, PublicationReturn pairs reviewed,
registered and passing. Coordinator added explicit unreached optional
seed-loop identity flags and guarded formedECX without null-forgiving.
Whole continuation composition owner
`1d833ee8-e59f-4376-9a91-08fb9199b226` owns ContinuationLifecycle pair.
Capacity call suffix owner `a49bbeac-5ee5-41a0-9965-4cc7c58870ab`
owns CapacityCall pair. Sole evidence writer
`4940b18d-783c-40f4-9512-5fd0221f7b51` adds prefix/call anchors;
await release before staging.

Latest pushed HEAD `55f1a66` includes three helper compositions.
Prefix/call anchors released and reviewed, no active evidence writer.
CapacityPrefix author refining read-attempt/captured distinction and
explicitreceiverCreadaddress; do notstagepairyet.
Post-call trace consumed only through firstopaquehandoff:
Transfer pair owner `77f0d27f-d500-4023-bebe-7f31ab6d036d` implements
BE6BCA..FD. Return-join read-only owner
`83375c4c-050c-44eb-9a8f-5d1c1467a78f` tracesFD..BE6C10.
Do not use broad latercall/zero-fill report before bounded verification.

Latest pushed HEAD `77c9d36` pins prefix/call anchors.
CapacityPrefix/CapacityCall/ContinuationLifecycle reviewed/registered,
passing. Coordinator rejected invalid enum call routes (previously fell
through to direct call), added fixture, and corrected continuation fixture
whose flagged first-stage was already blocked, not nominally complete.
PrefixCall composition owner `76dc753e-9c3a-453c-af27-e51cbb14a729`
owns new pair. Active Transfer77f0d27f and ReturnJoin83375c4c remain.

Reviewed prefix/call/continuation pushed as `655f15b`.
First transfer owner `77f0d27f` released; native order/bytes reviewed,
refinement requested for explicit unreached captures and SuccessAdmission.
ReturnJoin trace `83375c4c` completed; implementation owner
`704fdb55-0bcc-4fc5-909e-b4a2b3b7cc15` owns TransferReturn pair.
HelperLifecycle owner `2340766a-cce7-4135-9f0b-9de81acfc8d1`
owns entry-through-continuation composition.

Custom reverse-engineer trace `6b2e43a1` lacked IDA tools; no verified
window returned. Coordinator instead verified raw `BE6C10..BE6C57`
via IDA disasm/get_bytes and offline PE bytes (identical).
SecondTransfer prefix owner `a4f503a1-53b0-4812-b4cc-267b48656710`
owns only `BE6C10..BE6C34` projection/check pair. Evidence writer
`dd6afd80-0ca2-4a90-b6a9-35e51ba5730a` exclusively owns
StartupBinaryEvidence anchors for first transfer, return join, second prefix.

Verified second-prefix bytes:
`8D04BD000000008BD6C1F8022BD78955F48D1C8500000000A19C48AC015351FF75F0FFD0`.
At C10 EAX=EDI*4, C17 EDX=ESI, C19 signed SAR EAX2,
C1C EDX-=EDI, C1E frame-C store, C21 EBX=EAX*4;
C28 independent call-cell DWORD load, C2D/C2E push EBX/ECX,
C2F independently read/push frame-10, C32 call EAX, normal C34.
Cell zero is a captured target, not an invented guard.

Next bounded continuation: C34 fresh frame-C read into ECX before
C37 cleanup12; C3A EDX=EBX+opaque return EAX; TEST/JZ C3D/C3F
to C4A. Nonzero C41 XOR EAX, C43 EDI=EDX, C45 REP STOSD,
C47 independent frame-8 reload EDI. DF direction is not established
in this bounded slice: do not assume forward writes. C4A fresh
frame-4 load EBX, TEST EDI/JZ C4D/C4F to CA2, else C51 ECX=EDI,
C53 EDI=EBX, C55 jump C9E. This later portion is trace only;
no implementation admission yet. Preserve reloads and opaque call outputs.

FirstTransfer and TransferReturn pairs reviewed/registered and passing.
Coordinator added first-transfer SuccessAdmission after author supplied
unreached flags but omitted that property. TransferLifecycle owner
`4719867f-8c66-49a9-baa7-ec03e5652946` owns composition pair.
Return EntryEDI/ESI remain independent post-call captures, not assumed
preserved from the first call's inputs.

PrefixCall reviewed/registered/passing; transfer immutable anchors reviewed
and passing. HelperLifecycle production reviewed; author `2340766a`
refining fixtures (null IncomingEcx wrongly expected unreached; final
formed ECX/slot offset must follow wrapping sum, not fixed fixture values).
SecondReturnGate owner `c70f8a09-aeeb-4378-af21-0d95a310950a`
owns only C34..C41 read/cleanup/LEA/TEST/JZ pair. No REP effects.
LoopPrefix owner `1095361c-cf6e-428e-bfd7-b69bd3af1de9` owns
single-iteration-to-prefix-call composition. Reject doubling/failure routes.

HelperLifecycle fixture refinement reviewed; HelperLifecycle,
TransferLifecycle and SecondTransfer pairs registered/passing.
PrefixTransfer owner `7fc27eed-eff7-4bd3-9ded-7e8f0374e3d8`
owns prefix-call-to-first-transfer composition. Zero bypass has no call,
so EDI must match shifted prefix value; call routes retain independently
captured post-call EDI. No assumption that frame reloads equal prior stores.

Latest reviewed code pushed `0f02297`. No evidence writer active.
AboveTransfer owner `2495e4e5-1616-4958-a1f6-9db4c399e6d7`
owns bounded C57..C6D pair. Coordinator raw IDA trace:
`8D3CB5000000008BC783E0FC50A19C48AC015152FFD0`.
LEA EDI=ESI*4, MOV/AND EAX, PUSH EAX happens BEFORE call-cell
DWORD load. Then PUSH ECX/EDX, CALL EAX, normal C6D.
Next C6D..C8F bytes (trace only):
`8B43048B55F48D0C072BD103D0C1FA028D1C95000000005351FF75FCFF159C48AC01`.
Fresh receiver4 EAX, independent frame-C EDX, ECX=EDI+EAX,
SUB/ADD/SAR EDX2, EBX=EDX*4, PUSH EBX/ECX/fresh frame-4,
indirect call cell1AC489C. Do not assume incoming EDI/EBX preserved
across opaque call without conditional identity capture.

DF trace `86b86726` verified helper has no CLD/STD/POPF; calls opaque.
Second REP is at CA0, not C9E (C9E XOR EAX). Direction requires
capture; no ABI-clear assumption. RepeatStore owner
`b7d4d10f-3c9f-42bf-a6b0-12a2ed562f02` owns C41..C4A pair,
compressed directional write descriptor only; completion independently
captured, no unbounded array. SecondReturnGate production/checks reviewed,
registered; serial validation blocked by active LoopPrefix CS7036 at line125,
owner notified. Author had run builds/tests despite no-test instruction.
SecondTransferLifecycle owner `81f5c642-0db0-4b10-ab5a-3065d40ff9d1`
owns second call/return composition; post-call EBX independent.
Evidence writer `dde6ad0e-9ef9-4fc0-bc98-21d519b0cea6`
exclusively owns return-gate/repeat/above-prefix immutable anchors.

SecondReturnGate/SecondTransferLifecycle/PrefixTransfer/AboveTransfer/
RepeatStore reviewed, registered and passing. Corrected repeat XOR bytes
from author's equivalent-but-not-native `31C0` to actual `33C0`.
Corrected second-lifecycle fixture: missing return read is ReturnBlocked,
even when its partial child also flags later EAX capture.
Evidence author initially used raw C34/C41/C57 instead of FULL BE6Cxx;
coordinator rejected; corrected anchors now pass. Await formal release
before staging evidence file.
LoopPrefix refinement active: call-frame mismatch belongs after prefix
stores, not an early prefix-identity gate. Nullable check errors addressed.
FillJoin owner `84d394c4-71ab-406f-a003-ea805137b3e1` owns C4A..C57.
GateRepeat owner `23ecce24-7bd7-4188-adcf-1c038f18b8a8` owns
second-return-gate-to-directional-REP composition.

Latest pushed `0f51764` includes corrected anchors.
AboveSecondTransfer owner `7d8afd4e-4e45-4ad3-889e-42b7d5f235e9`
owns C6D..C8F pair. AboveReturnGate owner
`57f5217a-46f5-496c-8f43-82e432e87bf5` owns C8F..C9E:
`83C4188D3C038B5DFC85F674068BCE`; cleanup24 before LEA,
fresh frame-4 read, TEST ESI/JZ CA2 else MOV ECX=ESI.
Shared second REP raw C9E..CA2 is `33C0F3AB`, distinct from
earlier C41 REP slice (no fresh frame-8 read afterward).
Final capacity tail CA2..CB1 trace only:
`8B7DEC8B4F0485C97405E8AF206E00`; fresh frame-14 read EDI,
fresh receiver4 read ECX, TEST/JZ CB1, else direct call raw12C8D60.
No release/free/ownership inference. Later CB1 receiver8 ADD and
receiver4 store require bounded verification before implementation.

Coordinator verified fill/above continuation PE bytes identical to IDA.
Exclusive evidence writer `e8ca5c8d-96e3-4fed-89c8-f8309ae28dda`
owns FillJoin/AboveSecondTransfer/AboveReturnGate anchors.
Verified final raw BE6CB1..BE6CC0 via IDA:
`017708895F045F5E5B8BE55DC20400`.
ADD DWORD[entryEDI+8], entryESI (independent old value read, wrap),
MOV DWORD[entryEDI+4], entryEBX, POP EDI/ESI/EBX,
ESP=EBP, POP EBP, RET4. No full EAX return guarantee in this slice.
Future bounded task can implement these final effects after tail gate;
do not infer freeing from the preceding direct call.

LoopPrefix refinement reviewed; removed incorrect fixture expectation
MissingEntryRoute (route is derived, actual missing direct-call status).
FillJoin reviewed; removed unrelated bogus CoordinatorId constant.
Both registered; validation blocked by active AboveSecondTransfer
BuildPlan missing unreachedReceiver4 parameter (owner notified).
Fill/above immutable anchors reviewed/released, pending validation commit.
FinalReturn owner `754371f6-8106-404c-a355-659d6ccd649f`
owns BE6CB1..BE6CC0 final ADD/store/RET4 pair.
FinalCallGate owner `898bd171-cc83-4d77-b181-d6a803c752b4`
owns BE6CA2..BE6CB1 independent frame-14/receiver4 reads and call gate.

SharedRepeat owner `bc51d180-4962-408e-a005-61a770c10ab6`
owns BE6C9E..BE6CA2 pair, reusing compressed store descriptor.
Final-tail evidence writer `7a3dc276-9399-4141-bd3b-9b4cce6ee860`
now exclusively owns StartupBinaryEvidence (previous anchors released
but uncommitted; wait for new writer release before whole-file staging).
Coordinator PE verified BE6C9E..BE6CC0:
`33C0F3AB8B7DEC8B4F0485C97405E8AF206E00017708895F045F5E5B8BE55DC20400`.

LoopPrefix/FillJoin/AboveSecondTransfer/AboveReturnGate/GateRepeat
reviewed/registered and passing. GateRepeat fixture corrections: supplied
EDX identity must equal wrapping gate LEA; optional absent EBP identity
cannot be expected true; missing gate read with supplied later EAX is
GateHasUnreachedCaptures. Native projections unchanged.
Both evidence writers released and all six pending anchors reviewed/passing.
FinalCallGate/FinalReturn/SharedRepeat released but NOT reviewed/registered.
AboveLifecycle owner `5094e5ec-fc17-433d-91bf-d4624c202794`
owns above first/second transfer and scalar-return composition.

FinalCallGate/FinalReturn/SharedRepeat pairs reviewed/registered/passing.
FinalLifecycle owner `daed1d32-ee99-43ce-99ed-57e0b7751e99`
owns final call/return composition; no-call EDI derived, opaque-call
EDI independent. FillSharedRepeat owner
`3d5bfa78-759c-4bcf-ad26-39b941e64945` owns fill-join/shared-REP
composition. All future composition scopes preserve independent reloads
and require explicit DF/completion captures. No evidence writer active.

Latest pushed HEAD `d098627`; canonical pinned offline runner passed all
registered checks. Active compositions: AboveLifecycle5094e5ec,
FinalLifecycledaed1d32, FillSharedRepeat3d5bfa78 (own new pairs only).
Read-only final callee trace owner `1b9437d8-0d61-43a8-b37d-a8f83832a562`
examines raw12C8D60 normal-return EDI/ESI/EBX preservation; no ownership
semantics inferred. Continue fresh Luna successors after releases.
Potential next scopes: compose AboveReturnGate with SharedRepeat,
compose GateRepeat into FillJoin (zero gate bypass leaves incoming EDI
independent; nonzero completed repeat uses fresh frame-8 EDI), then
join-through-FinalLifecycle. Keep exact branch/outcome and unreached
admission rather than stop-offset-only gates. Do not revisit completed
committed pairs unnecessarily.

FinalLifecycle pair reviewed/registered; wrapping fixture corrected to
match frame-14-derived EDI on no-call bypass. Validation currently blocked
by active AboveLifecycle CS0136 local-name collisions; owner notified.
AboveRepeat owner `6e127817-efd8-4412-be6f-c4cf5fd8cb98` owns
above return gate/shared-REP composition. Final callee trace reports
local saves/pops but indirect call after pops at raw12C9452 unresolved;
same-task refinement requested for exact exit gate/bytes and raw-EA wording.
Do not replace independent post-call capture policy from this partial trace.
Refinement confirmed: raw12C9441 TEST encoded-VA1EDAF84 byte mask02;
pops9448..944A preserve flags; JZ944B skips post-pop indirectcall9452.
Exact 27-byte exit anchor added to StartupBinaryEvidence. FinalLifecycle,
AboveLifecycle and FillSharedRepeat pairs reviewed/registered, awaiting
serial validation while active successor files compile. AboveLifecycle
fixture property references corrected to ReturnGateSuccessAdmission.
RepeatFillJoin owner `36ef45fb-9d9c-41f8-b17d-54c629d5fe61` notified of
active pattern-local CS0136 collisions. FillFinalLifecycle owner
`9f6cf4a4-e205-4175-b6ef-11f5d81a60cd`; bounded final callee exit projection
owner `5791c8ee-66b2-4029-bb10-ea1ad86d1aa4`. Successors own only their
production/check pairs; coordinator owns registration/docs/evidence/Git.

Verified three released lifecycle pairs and native exit anchor; pushed
99121d0. RepeatFillJoin reviewed/registered; fixture EDX corrected to
0x3100 (entryEBX0x3000 + helperEAX0x100), pinned offline runner passed.
AboveRepeat revision requested: optional repeat identities must not become
prerequisites; propagate REP unreached capture flags. Owner retains pair.
FirstFillLifecycle successor `834265e9-1dda-45c5-8fad-64d6c4758ea2`
owns SecondTransferLifecycle-to-RepeatFillJoin bounded composition.

AboveRepeat revised/reviewed/registered; optional identity cross-checks
and REP unreached flag propagation corrected, pinned offline runner passed.
AboveFinalLifecycle successor `cad06a4e-850b-4749-bfa6-cd2031f48e4b`
owns above repeat-to-final tail, checking EBX/ESI continuity only on the
final no-call bypass. Active FillFinalLifecycle, CalleeExit and
FirstFillLifecycle files remain unregistered/unreviewed.

AboveRepeat pushed0d6f878. FillFinalLifecycle and CalleeExit reviewed,
registered and passed coordinator pinned offline runner. FillFinal fixtures
now explicitly supply call status when asserting it is unreached.
FirstFinalLifecycle owner `3a735978-00dd-466c-bdd9-6bb200dfc7ca` composes
RepeatFillJoin-to-FillFinal without inventing missing ESI continuity.
Entry trace owner `e9182693-1628-40dd-afc8-9d433bdf345a` owns read-only
bounded raw12C8D60 entry/ECXzero path evidence, not files.

FillFinal and CalleeExit pushed4841023. FirstFill and AboveFinal reviewed,
registered and pinned offline runner passed. AboveFinal missing-gate flags
propagated; nullable check fixed; incomplete-REP fixture uses nonzero ESI.
Native 24-byte callee entry anchor verified with IDA and offline PE.
Entry projection owner `f0bd6ba2-4ebc-4ebe-b5c8-d3d6c503f673` active.
AboveBranchLifecycle owner `bb922be1-e409-4817-85a5-0972327641b3` active.
FirstFinal owner notified to fix mismatched fixture parameter names;
its files remain unregistered. All agents use GPT-6 Luna.

FirstFill/AboveFinal/entry anchor pushedc8ba0a9. FirstFinal reviewed and
returned for route-admission correction: either first gate route may feed
either fill branch; preserve all eight route combinations. Fixtures that
assert unreached call status must actually supply it.
SecondBranchLifecycle owner `0b49b470-4e0c-47e4-8e25-1313521ceafc`
owns second-transfer-to-final composition. Failure trace owner
`9066c5b2-73fc-45da-88d4-754578bdbad0` owns bounded read-only capacity
failure tail. Neither owns registration/docs/evidence/Git.

FirstFinal revision and CalleeEntry reviewed/registered; validation blocked
by active AboveBranch nullable-struct access errors, owner notified.
Callee prefix trace `edbcfcb7-c830-426a-9d2b-8f79fd105a8d` owns read-only
raw12C8D78 early mask-gated call prefix. No assumptions from names.

Overflow trace verified rawBE6CC0..CCB PUSH encodedVA1C607A4 then
CALL cellVA1AC45C0, target unresolved/nonreturn not proven. Size guard
instead entersBE6BC4 cell1AC4424. OverflowTail pair owner
`16d6609a-fd2e-4331-99aa-54c2d6491b20`.
Prefix raw12C8D78..88 verified LEAESI[ECX-8], JZmaskclear, PUSHVA1EDAF88,
CALLcellVA1AC4278. PrefixCall owner `89c3977c-e72f-4347-ad9c-d4057b291cdd`.
First validation88..A7 verified freshglobal EBX, unsignedESI comparison,
freshESI4 EAX read and lowtwo-bit==1 branch to12C8E76. Three immutable
anchors added, pending serial validation with entry/FirstFinal stagedpair.

OverflowTail reviewed/registered. LoopOverflow successor
`73445c13-c26d-4865-b15d-ca40848eaa02` owns one-iteration overflow
composition; FirstValidation owner `259bb22c-6493-4dbf-985a-776c814b6ce4`
owns fresh global/ESI4 scalar gate. PrefixCall/AboveBranch released but
returned for compile alignment fixes; SecondBranch active literal type
correction requested. Entry/FirstFinal/Overflow and anchors awaiting
shared compilation before commit. No active files staged.

Entry/FirstFinal/OverflowTail and three new anchors verified/pushed82a2f6d.
LoopOverflow and PrefixCall reviewed/registered and pinned runner passed;
loop fixtures corrected for absent captures and reached-not-taken guard.
PrefixLifecycle `f588296e-5df4-421c-b0c8-0d513c34d416` active.
AboveBranch and SecondBranch released but NOT yet reviewed/registered.
FirstValidation `259bb22c-6493-4dbf-985a-776c814b6ce4` active.

LoopOverflow/PrefixCall pushed46d67be. FirstValidation now released;
agent ran checks despite prohibition, coordinator must review/register
and validate independently. ValidationLifecycle successor
`c3748d39-6a7b-4e86-9ae5-f36ad7e35382` active, owns prefix-call-to-first
validation composition. Latest canonical runner passed before this release.

FirstValidation, PrefixLifecycle and ValidationLifecycle reviewed/registered;
validation terminal admission corrected to require clear unreached-read
flags. AboveBranch and SecondBranch reviewed/registered but fixture runner
failures returned to their owners for capture/route corrections.
Continuation trace `410e1359-078c-4942-914d-cf96e5b11bb5` active;
EntryValidation pair `1f97c2b4-c4b3-421c-a124-0ffc0ae23dec` active.
No sample execution or IDB changes; latest pushedHEAD46d67be.

Coordinator offline runner passed after branch fixture corrections and
EntryValidation review/registration. Six released production/check pairs
are ready for checkpoint: FirstValidation, PrefixLifecycle,
ValidationLifecycle, EntryValidation, AboveBranch and SecondBranch.
Nonreturn fixture explicitly supplies shared-repeat captures rather than
asserting flags for absent values. Wrapped above fixture retains exact
no-call EDI/ESI/EBP continuity and wrapped receiver+4 address 3.
New immutable native anchor pins raw12C8DA7..12C8DED, 70 bytes, with
distinct branches to12C8EF8,12C8E45,12C8E3A and opaque cell1AC42A8.
AlignmentDispatch pair owner `c71aebe7-aa41-4e5d-8a6a-aba6d0cf5039`
active, do not stage until release. Read-only successors:
first-return dispatch `707d1845-46a5-492a-b20f-5d30cb0e1634`,
tag branch `f24535df-8be7-4ffc-b03a-58467db22f0a`,
computed-zero branch `9f44de5f-cfaa-4185-86f7-bf23450ff31f`.

Checkpoint pushed da893be. The three custom reverse-engineer trace agents
did not have IDA tools; their unverified address translations are not
authoritative. Coordinator directly verified first-return bytes and
tag/zero destinations through read-only IDA tools instead.
New active implementation owners, all GPT-6 Luna:
DispatchLifecycle `5bcec49c-2ea5-476d-8080-acb046d924f0`;
TagValidation `aab0f8c4-035d-4599-8856-6f35a5713658`.
Tag window raw12C8EF8..12C8F21 gates unsigned ESI>=EDX,
fresh[EDX+4] bit0 clear, bit1 set, then EDX!=fresh[PEVA1EDADE0].
Exits respectively12C8E76,12C8E76,12C92DB,12C90CA; equality
fallsF21. Computed-zero tail raw12C8E3A..12C8E45 still reads and
writes global PEVA1EDAF78 via SUB ECX before JMP12C9441:
even entry ECX0 must not erase the native read/write evidence.

AlignmentDispatch released, reviewed and registered. Coordinator fixed
captured-zero ESI-read fixture to take the entryECX branch before later
LEA overwrites EAX; this is not a production semantic change. Canonical
offline runner passed including new tag and computed-zero anchors.
ZeroExitLifecycle `3ae486b0-c3bb-4e01-ae1b-bce0ad28ecd0` active
composes only clean computed-zero route, preserving fresh global read/write
before existing exit projection. DispatchLifecycle and TagValidation remain
active; keep all three active pairs out of this checkpoint staging.

Alignment checkpoint pushed ebd116d. TagValidation released/reviewed/
registered and coordinator checks passed; agent ran builds despite request,
so coordinator validation remains authoritative. DispatchLifecycle released/
reviewed/registered but four fixture failures returned to owner for revision.
New active pair owners: AlignmentTagLifecycle
`c6f505d0-245f-4d87-beaa-02900fa28374`; EntryAlignmentLifecycle
`cd871ff2-84ee-4e35-9ac1-a021084f7998`. ZeroExitLifecycle still active.
Coordinator next-window trace raw12C8F21..12C8F71, 80 bytes:
fresh global1EDADD4 + entryEDI ->ECX; writes1EDADE0=entryESI,
1EDADD4=ECX,[ESI+4]=ECX|1; fresh compare1EDADDC optionally clears
1EDADDC and1EDADD0; fresh unsigned ECX<=global1EDADE4 exits9441;
otherwise EDX0, ECX immediatePEVA1EDADC8, direct callraw12C7BF0
atF6C endsF71. No implementation owner assigned for this window yet.

Equality continuation owner now `42e24d7d-d1fd-41f9-82c8-93f53116c3d7`.
NonzeroValidation owner `c096c7e6-7461-4ac5-b395-87cb1e1c8df2` active:
raw12C8E45..12C8E76 49bytes; ADD EDI,EAX, unsigned ESI<EBX exitE76,
fresh global1EDADDC equality exit90A2, logical EAX>>3/store[EBP-10],
unsigned shiftedEAX>=20h exitF76, ordered fresh[ESI+8]/[ESI+C]
stores to[EBP-4]/[EBP-14], then ESI!=ECX exitE7C or equalityE76.
E76 is indirect call[PEVA1AC4A24], not proven nonreturning.
ZeroExitLifecycle reviewed revision now requires exact RET boundary and
all exit unreached flags clear; registered pending runner. AlignmentTag
returned for forwarding nested unreached flags and nullable-bool fixture
compilation. EntryAlignment active writer temporarily blocked latest build
with identifier/property errors; owner informed. Do not edit active files.

EntryAlignment released/reviewed but returned for distinct successful clean
alignment branch outcomes (rather than labelling all non-call paths blocked)
and aggregate forwarded unreached flags. AlignmentTag revised forwarding
reviewed and registered; ZeroExit clean terminal revision registered.
Latest coordinator runner shell3328 blocked by active NonzeroValidation
nullable-record-struct accesses at203,231,251,314; owner informed.
Next validation must include Dispatch, TagValidation, ZeroExit, AlignmentTag
registrations plus new equality/nonzero/shared-call anchors. EntryAlignment
and active EqualityContinuation/NonzeroValidation not yet registered.
Latest pushed HEAD remains ebd116d; uncommitted reviewed pairs require
passing coordinator runner before explicit-path checkpoint staging.

Coordinator runner3329 compiled and only failed Dispatch's blocked-first-
route fixture group; owner received final focused correction. TagValidation,
ZeroExit, AlignmentTag and EqualityContinuation registered checks pass.
Equality scalar released/reviewed and successor TagEqualityLifecycle
`49f2234f-8130-4937-b216-6ba428ca419f` active.
EntryAlignment revision and NonzeroValidation still awaiting releases.
Do not stage active pairs; reviewed uncommitted scalar/branch pairs can
checkpoint after remaining Dispatch fixture passes.

Latest runner3334 now passes Dispatch, Tag, ZeroExit, AlignmentTag,
EqualityContinuation and EntryAlignment; only NonzeroValidation's first
unsigned/equal-global fixture group fails and owner has focused correction.
All seven reviewed/registered pairs are pending explicit checkpoint staging.
Active successors: AlignmentNonzeroLifecycle
`9665dc5f-4051-44ab-997e-2bba5aa06255`, TagEqualityLifecycle
`49f2234f-8130-4937-b216-6ba428ca419f`, call-continuation trace
`4da05d36-3e1c-4c17-b7bf-d39cbbb8bf8d` (corrected after agent queried
mapped VA instead of raw IDA EA; only direct raw addresses are authoritative).

Canonical runner3335 passed all registered checks; seven pairs and anchors
checkpointed/pushed9a3fbe9. Active TagEqualityLifecycle is released but
not yet reviewed/registered. AlignmentNonzeroLifecycle active.
New FrameComparisonCall owner `5d175f79-1d10-4737-bcd4-9bdd82646acb`
models verified raw12C8E7C..12C8E87 bytes3B75FC7506FF15244AAC01:
fresh[EBP-4] compareESI, JNZ skips opaque cell1AC4A24 call atE81.
Coordinator independently verified11bytes and added pending immutableanchor.
PriorE76 call and optionalE81 call use same cell but are separate events;
neither nonreturn nor registerpreservation is proven.

FrameComparisonCall and revised TagEqualityLifecycle reviewed/registered.
Latest runner3339 blocked by active AlignmentNonzero wrong constantname at594;
owner informed. New TagEqualityExitLifecycle
`3519701b-7ab4-44a6-b9b2-1fde02775cbf` active on only clean capacity
exit9441; preserves other bounded destinations without inventing laterF71
continuation. Latest pushed checkpoint9a3fbe9 remains authoritative.

Next standalone SizeComparisonCall owner
`f961de7e-a4a5-45dd-8d34-1a4be28662fa` active on verified
raw12C8E87..12C8E9A19bytes: fresh[ESI+4] maskFFFFFFF8, independent
entryEAX SHL3, CMP, equality skips optional opaque cell1AC4A24 callE94.
Earlier calls may alter EAX; no shift-result continuity assumed.
Runner3344 still blocked by active AlignmentNonzero nullable check
accesses235,242,244,246; owner informed. FrameComparisonCall and
TagEqualityLifecycle pending canonical validation/checkpoint; registrations
already present. Both pending frame/size immutableanchors need runner too.

Runner3345 compiled; onlyFrameComparisonCall fresh-read fixture group fails,
owner has correction. TagEqualityLifecycle registeredchecks pass.
AlignmentNonzero released/reviewed but returned for production admissionbug:
sharedprelude required terminal2writes/alignedEDI, incorrectly rejecting
cleancomputedzero/call paths with3writes and changedEDI. Owner must move
terminal checks into route-specific guards. Not registered until revision.
TagEqualityExit and SizeComparisonCall remain active. Latestpush9a3fbe9.

Next unassigned native scalar window beginsraw12C8E9A:
19bytes8B4DF08D04CDF0ADED018945F08B45FC3B45F0 throughEAD.
Fresh[EBP-10]->ECX; LEA EAX=PEVA1EDADF0+ECX*8 (not memoryread),
storeEAX[EBP-10]; fresh[EBP-4]->EAX; CMP EAX,fresh[EBP-10].
The last read remains independent despite known precedingwrite.
Coordinator observed disasm nextJZ EAD->EBE, but no complete successor
anchor/pair yet; don't claim whole continuation from these19bytes alone.

FrameComparison fixture revised; SizeComparison standalone and
AlignmentNonzero admission revision reviewed/registered. TagEqualityExit
returned to reject any supplied optional ExitCapture on noncapacity routes,
even all-null struct; its active rename blocked runner3351 atline83.
New FrameSizeLifecycle owner `94bfd6b4-d5df-4e35-b6c1-18aa7b4b3391`
active; requires independent EAX always and post-call ESI/ESP after framecall,
deriving only skip-route continuity.
Four reviewed pendingpairs: FrameComparison, TagEqualityLifecycle,
SizeComparison, AlignmentNonzero; latestpushed9a3fbe9.

Runner3355 compiled and onlyfailed AlignmentNonzero's normalcall/
contamination fixture group; owner correcting call guard/fixture. Other
pending FrameComparison/SizeComparison/TagEquality checks passed.
DescriptorReload owner `f78b2ff5-14ce-49c6-ac76-da3e31a9de49` active
on full30bytes raw12C8E9A..12C8EB8, coordinator immutableanchor added.
FreshCMP[EBP-10] must remain independent of prior LEA/store; four routes
endEB8/EBE without incorporating next opaquecall.

Canonical runner3358 passed all registeredpairs including revised
TagEqualityExit. AlignmentNonzero owner's final callguard fix appears on
disk but await explicit release before staging its files. Pending reviewed
checkpoint is FrameComparison, TagEqualityLifecycle, SizeComparison,
AlignmentNonzero and TagEqualityExit plus frame/size/descriptoranchors.
New FrameMaskUpdate owner `00078532-a7f3-4135-b8ba-78b764236cb5`
active on raw12C8EBE..12C8ED5 23bytes. Fresh[EBP-14] vsfresh[EBP-4],
mismatchED5; equality fresh[EBP-8], SHL entryCL&31, NOT, freshglobal
PEVA1EDADC8 ANDwrite thenJMP EF8. No reuseframe8=1 or priorCL across
opaqueEB8call. FrameSize and DescriptorReload remain active.

Coordinator additional unassigned window raw12C8ED5..12C8EE9:
20bytes3B45F0740F3BC372053970087406FF15244AAC01.
Fresh[EBP-10] compareentryEAX equalityEE9; unsignedEAX<entryEBX
toEE3 opaque cell1AC4A24call; otherwise fresh[EAX+8] compareentryESI
equalityEE9, inequalitycallEE3. Normalcapturedreturn endsEE9.
FollowingEE9 freshlyreloads[EBP-4] toECX thenwrites[ECX+C]=EAX and
[EAX+8]=ECX, freshglobal1EDADD8->EBX atEF2 before tagrouteEF8.
This is observed disasm only beyondEE9; don't inferpointerownership.

Latest checkpoint2ca46b2 pushed after canonicalrunner3361passed.
Five releasedcomparison/equalitypairs nowcommitted; only activeFrameSize,
DescriptorReload, FrameMaskUpdate files remainuncommitted.
New DescriptorComparisonCall owner `84de9ca2-03df-4613-bfa0-ead88dd90b0e`
active on verified20bytes raw12C8ED5..12C8EE9. Coordinatorowns registrations,
evidenceanchors, docsandGit. AllagentsGPT-6Luna, no sampleexecution/IDBchanges.

DescriptorReload and FrameMaskUpdate released/reviewed/registered;
canonicalrunner3366 passed. FrameSize released but returned for clean
size-terminal admission and nested blocked-capture flag corrections.
New SizeDescriptorLifecycle owner `554fd250-7e96-4564-9b32-112353430b71`
active; independent EBP/EBX always, independent ESI after sizeopaque call.
DescriptorComparisonCall stillactive. Stage only reviewed scalarpairs,
registrations/docs and pending descriptorcomparisonanchor for checkpoint.

Latestcheckpointdff4655 pushed. Active FrameMaskTagLifecycle
`d28e7c84-cd24-4e3d-82e3-02ab30cc0f6a` requires independenttagESI/EDX
after cleanmaskupdate; FrameMismatchED5 staysbounded if no tagcaptures.
Otheractive owners FrameSize revision94bfd6b4, SizeDescriptor554fd250,
DescriptorComparisonCall84de9ca2. Coordinator should keep nextrunner serialized
and avoid staging activepairs until released/reviewed.
