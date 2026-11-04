using Firefall.Reconstruction.Startup;

internal static class OwnerCallbackQueueDeliveryChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("First-owner queue-delivery anchors map the captured VA range back to raw bytes",
            OwnerCallbackQueueDeliveryProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && OwnerCallbackQueueDeliveryProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && OwnerCallbackQueueDeliveryProjection.EntryRawFileOffset == 0x00BD523E
            && OwnerCallbackQueueDeliveryProjection.EntryVirtualAddress == 0x00FD5E3E
            && OwnerCallbackQueueDeliveryProjection.EntryVirtualAddressFromRawFileOffset
                == OwnerCallbackQueueDeliveryProjection.EntryVirtualAddress
            && OwnerCallbackQueueDeliveryProjection.EntryEndRawFileOffsetExclusive == 0x00BD5277
            && OwnerCallbackQueueDeliveryProjection.EntryEndVirtualAddressExclusive == 0x00FD5E77
            && OwnerCallbackQueueDeliveryProjection.EntryEndVirtualAddressFromRawFileOffset
                == OwnerCallbackQueueDeliveryProjection.EntryEndVirtualAddressExclusive
            && OwnerCallbackQueueDeliveryProjection.CapturedBytesEndRawFileOffsetExclusive == 0x00BD527E
            && OwnerCallbackQueueDeliveryProjection.CapturedBytesEndVirtualAddressExclusive == 0x00FD5E7E
            && OwnerCallbackQueueDeliveryProjection.CapturedBytesEndVirtualAddressFromRawFileOffset
                == OwnerCallbackQueueDeliveryProjection.CapturedBytesEndVirtualAddressExclusive
            && OwnerCallbackQueueDeliveryProjection.CommonContinuationRawFileOffset == 0x00BD5277
            && OwnerCallbackQueueDeliveryProjection.CommonContinuationVirtualAddress
                == 0x00FD5E77
            && OwnerCallbackQueueDeliveryProjection.EntryEndRawFileOffsetExclusive
                - OwnerCallbackQueueDeliveryProjection.EntryRawFileOffset == 0x39
            && OwnerCallbackQueueDeliveryProjection.CapturedBytesEndRawFileOffsetExclusive
                - OwnerCallbackQueueDeliveryProjection.EntryRawFileOffset == 0x40
            && OwnerCallbackQueueDeliveryProjection.CapturedEvidenceBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x40
            && OwnerCallbackQueueDeliveryProjection.CapturedEvidenceBytes
                == "89 44 24 14 85 C0 74 31 8B 8E 0C 01 00 00 8D 54 24 14 "
                    + "52 8B 01 FF 50 04 8B 4C 24 14 85 C9 74 19 83 CA FF "
                    + "8D 41 08 F0 0F C1 10 83 FA 01 75 0A 85 C9 74 06 8B 01 "
                    + "52 FF 50 08 83 BE 0C 01 00 00 00");

        check("Instruction bytes and branch destinations preserve the exact bounded continuation",
            OwnerCallbackQueueDeliveryProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    uint? BranchTargetRawFileOffset,
                    uint? BranchTargetVirtualAddress)[]
                {
                    (0x00BD523E, "89 44 24 14", null, null),
                    (0x00BD5242, "85 C0", null, null),
                    (0x00BD5244, "74 31", 0x00BD5277, 0x00FD5E77),
                    (0x00BD5246, "8B 8E 0C 01 00 00", null, null),
                    (0x00BD524C, "8D 54 24 14", null, null),
                    (0x00BD5250, "52", null, null),
                    (0x00BD5251, "8B 01", null, null),
                    (0x00BD5253, "FF 50 04", null, null),
                    (0x00BD5256, "8B 4C 24 14", null, null),
                    (0x00BD525A, "85 C9", null, null),
                    (0x00BD525C, "74 19", 0x00BD5277, 0x00FD5E77),
                    (0x00BD525E, "83 CA FF", null, null),
                    (0x00BD5261, "8D 41 08", null, null),
                    (0x00BD5264, "F0 0F C1 10", null, null),
                    (0x00BD5268, "83 FA 01", null, null),
                    (0x00BD526B, "75 0A", 0x00BD5277, 0x00FD5E77),
                    (0x00BD526D, "85 C9", null, null),
                    (0x00BD526F, "74 06", 0x00BD5277, 0x00FD5E77),
                    (0x00BD5271, "8B 01", null, null),
                    (0x00BD5273, "52", null, null),
                    (0x00BD5274, "FF 50 08", null, null)
                })
            && OwnerCallbackQueueDeliveryProjection.Instructions.All(HasCapturedSlice)
            && OwnerCallbackQueueDeliveryProjection.BoundaryInstructionEvidence
                == new OwnerCallbackQueueDeliveryInstructionEvidence(
                    "Evidence only: common continuation instruction, outside the projected range",
                    0x00BD5277,
                    "83 BE 0C 01 00 00 00",
                    EvidenceOnly: true)
            && HasCapturedSlice(OwnerCallbackQueueDeliveryProjection.BoundaryInstructionEvidence)
            && OwnerCallbackQueueDeliveryProjection.BoundaryInstructionEvidence.EvidenceOnly
            && OwnerCallbackQueueDeliveryProjection.BoundaryInstructionEvidence.RawFileOffset
                == OwnerCallbackQueueDeliveryProjection.EntryEndRawFileOffsetExclusive
            && ShortBranchTargets(0x00BD5244, "74 31", 0x00BD5277)
            && ShortBranchTargets(0x00BD525C, "74 19", 0x00BD5277)
            && ShortBranchTargets(0x00BD526B, "75 0A", 0x00BD5277)
            && ShortBranchTargets(0x00BD526F, "74 06", 0x00BD5277)
            && OwnerCallbackQueueDeliveryProjection.Instructions
                .Where(instruction => instruction.BranchTargetRawFileOffset is not null)
                .All(instruction =>
                    instruction.BranchTargetVirtualAddress
                        == instruction.BranchTargetRawFileOffset
                            + OwnerCallbackQueueDeliveryProjection.CodeVirtualAddressDeltaFromRawFileOffset));

        OwnerCallbackQueueDeliveryPlan zeroPop =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: Captured(0)));
        check("A zero pop result branches directly to the common continuation",
            zeroPop.Outcome == OwnerCallbackQueueDeliveryOutcome.PopResultZeroReachesContinuation
            && zeroPop.Actions[^2]
                == new OwnerCallbackQueueDeliveryAction(
                    OwnerCallbackQueueDeliveryActionKind.BranchToContinuationForNullPopResult,
                    0x00BD5244,
                    TargetRawFileOffset: 0x00BD5277,
                    TargetVirtualAddress: 0x00FD5E77,
                    ConditionMet: true)
            && zeroPop.Actions[^1].Kind
                == OwnerCallbackQueueDeliveryActionKind.ReachCommonContinuationBoundary
            && zeroPop.Actions.All(action =>
                action.Kind != OwnerCallbackQueueDeliveryActionKind.CallOpaqueCallbackVtableSlot
                && action.Kind != OwnerCallbackQueueDeliveryActionKind.DecrementPayloadCounter
                && action.Kind != OwnerCallbackQueueDeliveryActionKind.CallOpaqueReleaseVtableSlot));

        OwnerCallbackQueueDeliveryPlan callbackNullRewrite =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: Captured(0x11112222),
                callbackReceiver: Captured(0x33334444),
                callbackReturnedNormally: Captured(true),
                reloadedPayload: Captured(0)));
        OwnerCallbackQueueDeliveryAction callbackCall = callbackNullRewrite.Actions.Single(action =>
            action.Kind == OwnerCallbackQueueDeliveryActionKind.CallOpaqueCallbackVtableSlot);
        OwnerCallbackQueueDeliveryAction receiverCapture = callbackNullRewrite.Actions.Single(action =>
            action.Kind == OwnerCallbackQueueDeliveryActionKind.CaptureCallbackReceiver);
        check("The callback receiver and stack-local argument are independent; callback rewrite to null skips both counter and release",
            callbackNullRewrite.Outcome
                == OwnerCallbackQueueDeliveryOutcome.CallbackRewrotePayloadToNull
            && callbackNullRewrite.CallbackReceiver == 0x33334444
            && callbackNullRewrite.PayloadAfterCallback == 0
            && callbackCall.Receiver == 0x33334444
            && callbackCall.StackArgumentAddressOffset == 0x14
            && callbackCall.VtableSlotByteOffset == 4
            && receiverCapture.Pointer == 0x33334444
            && receiverCapture.AddressOffset == 0x10C
            && callbackCall.TargetRawFileOffset is null
            && callbackCall.TargetVirtualAddress is null
            && callbackCall.IndirectTargetOpaque
            && callbackCall.CallReturnedNormally == true
            && callbackNullRewrite.Actions
                .Where(action =>
                    action.InstructionRawFileOffset
                        >= OwnerCallbackQueueDeliveryProjection.CallbackStackArgumentSetupRawFileOffset
                    && action.InstructionRawFileOffset
                        <= OwnerCallbackQueueDeliveryProjection.CallbackCallRawFileOffset)
                .Select(action => action.Kind)
                .SequenceEqual(new[]
                {
                    OwnerCallbackQueueDeliveryActionKind.FormCallbackArgumentAddress,
                    OwnerCallbackQueueDeliveryActionKind.PushCallbackStackArgument,
                    OwnerCallbackQueueDeliveryActionKind.LoadCallbackVtable,
                    OwnerCallbackQueueDeliveryActionKind.CallOpaqueCallbackVtableSlot
                })
            && callbackNullRewrite.Actions[^2]
                == new OwnerCallbackQueueDeliveryAction(
                    OwnerCallbackQueueDeliveryActionKind.BranchToContinuationForNullPayload,
                    0x00BD525C,
                    TargetRawFileOffset: 0x00BD5277,
                    TargetVirtualAddress: 0x00FD5E77,
                    ConditionMet: true)
            && callbackNullRewrite.Actions.All(action =>
                action.Kind != OwnerCallbackQueueDeliveryActionKind.DecrementPayloadCounter
                && action.Kind != OwnerCallbackQueueDeliveryActionKind.CallOpaqueReleaseVtableSlot));

        OwnerCallbackQueueDeliveryPlan oldZero =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: Captured(0x11112222),
                callbackReceiver: Captured(0x33334444),
                callbackReturnedNormally: Captured(true),
                reloadedPayload: Captured(0xFFFFFFFC),
                counterBeforeXadd: Captured(0)));
        check("The independently reloaded payload supplies a wrapping counter address and old zero wraps to FFFFFFFF without release",
            oldZero.Outcome
                == OwnerCallbackQueueDeliveryOutcome.CounterOldValueNotOneReachesContinuation
            && oldZero.CallbackReceiver == 0x33334444
            && oldZero.PayloadAfterCallback == 0xFFFFFFFC
            && oldZero.PayloadCounterAddress == 4
            && oldZero.CapturedOldCounterValue == 0
            && oldZero.CounterAfterXadd == uint.MaxValue
            && oldZero.Actions.Single(action =>
                action.Kind == OwnerCallbackQueueDeliveryActionKind.FormPayloadCounterAddress)
                .AddressOffset == 8
            && oldZero.Actions.Single(action =>
                action.Kind == OwnerCallbackQueueDeliveryActionKind.DecrementPayloadCounter)
                == new OwnerCallbackQueueDeliveryAction(
                    OwnerCallbackQueueDeliveryActionKind.DecrementPayloadCounter,
                    0x00BD5264,
                    Pointer: 4,
                    BeforeValue: 0,
                    AfterValue: uint.MaxValue,
                    CapturedValue: 0,
                    XaddPriorValueUsed: true)
            && oldZero.Actions.All(action =>
                action.Kind != OwnerCallbackQueueDeliveryActionKind.CallOpaqueReleaseVtableSlot));

        OwnerCallbackQueueDeliveryPlan oldOne =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: Captured(0x11112222),
                callbackReceiver: Captured(0x33334444),
                callbackReturnedNormally: Captured(true),
                reloadedPayload: Captured(0x55556666),
                counterBeforeXadd: Captured(1),
                releaseCallReturnedNormally: Captured(true)));
        OwnerCallbackQueueDeliveryAction releaseCall = oldOne.Actions.Single(action =>
            action.Kind == OwnerCallbackQueueDeliveryActionKind.CallOpaqueReleaseVtableSlot);
        check("Only captured old counter one reaches the opaque release slot and normal release return reaches continuation",
            oldOne.Outcome == OwnerCallbackQueueDeliveryOutcome.ReleaseCallReturnedToContinuation
            && oldOne.PayloadAfterCallback == 0x55556666
            && oldOne.PayloadCounterAddress == 0x5555666E
            && oldOne.CapturedOldCounterValue == 1
            && oldOne.CounterAfterXadd == 0
            && releaseCall.Receiver == 0x55556666
            && releaseCall.StackArgumentValue == 1
            && releaseCall.VtableSlotByteOffset == 8
            && releaseCall.TargetRawFileOffset is null
            && releaseCall.TargetVirtualAddress is null
            && releaseCall.IndirectTargetOpaque
            && releaseCall.CallReturnedNormally == true
            && oldOne.Actions
                .Where(action =>
                    action.InstructionRawFileOffset
                        >= OwnerCallbackQueueDeliveryProjection.ReleaseVtableLoadRawFileOffset
                    && action.InstructionRawFileOffset
                        <= OwnerCallbackQueueDeliveryProjection.ReleaseCallRawFileOffset)
                .Select(action => action.Kind)
                .SequenceEqual(new[]
                {
                    OwnerCallbackQueueDeliveryActionKind.LoadReleaseVtable,
                    OwnerCallbackQueueDeliveryActionKind.PushReleaseArgument,
                    OwnerCallbackQueueDeliveryActionKind.CallOpaqueReleaseVtableSlot
                })
            && oldOne.Actions[^1].Kind
                == OwnerCallbackQueueDeliveryActionKind.ReachCommonContinuationBoundary);

        OwnerCallbackQueueDeliveryPlan oldTwo =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: Captured(0x11112222),
                callbackReceiver: Captured(0x33334444),
                callbackReturnedNormally: Captured(true),
                reloadedPayload: Captured(0x55556666),
                counterBeforeXadd: Captured(2)));
        check("Captured old counter two decrements to one and skips release",
            oldTwo.Outcome
                == OwnerCallbackQueueDeliveryOutcome.CounterOldValueNotOneReachesContinuation
            && oldTwo.CapturedOldCounterValue == 2
            && oldTwo.CounterAfterXadd == 1
            && oldTwo.Actions.All(action =>
                action.Kind != OwnerCallbackQueueDeliveryActionKind.CallOpaqueReleaseVtableSlot));

        OwnerCallbackQueueDeliveryPlan missingCallbackReturn =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: Captured(9),
                callbackReceiver: Captured(10),
                callbackReturnedNormally: OwnerCallbackQueueDeliveryCapture<bool>.Missing(),
                reloadedPayload: OwnerCallbackQueueDeliveryCapture<uint>.Unreached(),
                counterBeforeXadd: OwnerCallbackQueueDeliveryCapture<uint>.Unreached(),
                releaseCallReturnedNormally:
                    OwnerCallbackQueueDeliveryCapture<bool>.Unreached()));
        OwnerCallbackQueueDeliveryPlan callbackDidNotReturn =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: Captured(9),
                callbackReceiver: Captured(10),
                callbackReturnedNormally: Captured(false),
                reloadedPayload: OwnerCallbackQueueDeliveryCapture<uint>.Unreached(),
                counterBeforeXadd: OwnerCallbackQueueDeliveryCapture<uint>.Unreached(),
                releaseCallReturnedNormally:
                    OwnerCallbackQueueDeliveryCapture<bool>.Unreached()));
        check("Missing callback return stops at its indirect call; a false return capture also prevents the local reload",
            missingCallbackReturn.Outcome
                == OwnerCallbackQueueDeliveryOutcome.MissingCallbackReturnBoundary
            && missingCallbackReturn.Actions[^2].Kind
                == OwnerCallbackQueueDeliveryActionKind.CallOpaqueCallbackVtableSlot
            && missingCallbackReturn.Actions[^2].CallReturnedNormally is null
            && missingCallbackReturn.Actions[^1].Kind
                == OwnerCallbackQueueDeliveryActionKind.StopAtMissingCallbackReturnCapture
            && callbackDidNotReturn.Outcome
                == OwnerCallbackQueueDeliveryOutcome.CallbackDidNotReturn
            && callbackDidNotReturn.Actions[^2].CallReturnedNormally == false
            && callbackDidNotReturn.Actions[^1].Kind
                == OwnerCallbackQueueDeliveryActionKind.StopAtCallbackCallBoundary);

        OwnerCallbackQueueDeliveryPlan missingReleaseReturn =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: Captured(9),
                callbackReceiver: Captured(10),
                callbackReturnedNormally: Captured(true),
                reloadedPayload: Captured(11),
                counterBeforeXadd: Captured(1),
                releaseCallReturnedNormally:
                    OwnerCallbackQueueDeliveryCapture<bool>.Missing()));
        OwnerCallbackQueueDeliveryPlan releaseDidNotReturn =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: Captured(9),
                callbackReceiver: Captured(10),
                callbackReturnedNormally: Captured(true),
                reloadedPayload: Captured(11),
                counterBeforeXadd: Captured(1),
                releaseCallReturnedNormally: Captured(false)));
        check("Missing release return stops at the opaque call; false does not reach the shared continuation",
            missingReleaseReturn.Outcome
                == OwnerCallbackQueueDeliveryOutcome.MissingReleaseReturnBoundary
            && missingReleaseReturn.Actions[^2].Kind
                == OwnerCallbackQueueDeliveryActionKind.CallOpaqueReleaseVtableSlot
            && missingReleaseReturn.Actions[^2].CallReturnedNormally is null
            && missingReleaseReturn.Actions[^1].Kind
                == OwnerCallbackQueueDeliveryActionKind.StopAtMissingReleaseReturnCapture
            && releaseDidNotReturn.Outcome
                == OwnerCallbackQueueDeliveryOutcome.ReleaseCallDidNotReturn
            && releaseDidNotReturn.Actions[^2].CallReturnedNormally == false
            && releaseDidNotReturn.Actions[^1].Kind
                == OwnerCallbackQueueDeliveryActionKind.StopAtReleaseCallBoundary);

        OwnerCallbackQueueDeliveryPlan missingPopResult =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: OwnerCallbackQueueDeliveryCapture<uint>.Missing()));
        OwnerCallbackQueueDeliveryPlan missingCallbackReceiver =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: Captured(1),
                callbackReceiver: OwnerCallbackQueueDeliveryCapture<uint>.Missing()));
        OwnerCallbackQueueDeliveryPlan missingPayloadReload =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: Captured(1),
                callbackReceiver: Captured(2),
                callbackReturnedNormally: Captured(true),
                reloadedPayload: OwnerCallbackQueueDeliveryCapture<uint>.Missing()));
        OwnerCallbackQueueDeliveryPlan missingCounter =
            OwnerCallbackQueueDeliveryProjection.Project(Captures(
                poppedPayload: Captured(1),
                callbackReceiver: Captured(2),
                callbackReturnedNormally: Captured(true),
                reloadedPayload: Captured(3),
                counterBeforeXadd: OwnerCallbackQueueDeliveryCapture<uint>.Missing()));
        check("Missing captures produce explicit stops at the reached pop, callback-receiver, reload, and counter boundaries",
            missingPopResult.Outcome
                == OwnerCallbackQueueDeliveryOutcome.MissingPopResultBoundary
            && missingCallbackReceiver.Outcome
                == OwnerCallbackQueueDeliveryOutcome.MissingCallbackReceiverBoundary
            && missingPayloadReload.Outcome
                == OwnerCallbackQueueDeliveryOutcome.MissingPayloadReloadBoundary
            && missingCounter.Outcome
                == OwnerCallbackQueueDeliveryOutcome.MissingCounterBoundary
            && missingPopResult.Actions[^1].Kind
                == OwnerCallbackQueueDeliveryActionKind.StopAtMissingPopResultCapture
            && missingCallbackReceiver.Actions[^1].Kind
                == OwnerCallbackQueueDeliveryActionKind.StopAtMissingCallbackReceiverCapture
            && missingPayloadReload.Actions[^1].Kind
                == OwnerCallbackQueueDeliveryActionKind.StopAtMissingPayloadReloadCapture
            && missingCounter.Actions[^1].Kind
                == OwnerCallbackQueueDeliveryActionKind.StopAtMissingCounterCapture);

        bool rejectsUnreachedOrMissingCapturePaths =
            ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: OwnerCallbackQueueDeliveryCapture<uint>.Missing(),
                    callbackReceiver: Captured(1))))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: OwnerCallbackQueueDeliveryCapture<uint>.Unreached())))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: Captured(0), callbackReceiver: Captured(1))))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: Captured(1))))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: Captured(1),
                    callbackReceiver: OwnerCallbackQueueDeliveryCapture<uint>.Missing(),
                    callbackReturnedNormally: Captured(false))))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: Captured(1),
                    callbackReceiver: Captured(2),
                    callbackReturnedNormally:
                        OwnerCallbackQueueDeliveryCapture<bool>.Unreached())))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: Captured(1),
                    callbackReceiver: Captured(2),
                    callbackReturnedNormally: OwnerCallbackQueueDeliveryCapture<bool>.Missing(),
                    reloadedPayload: Captured(3))))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: Captured(1),
                    callbackReceiver: Captured(2),
                    callbackReturnedNormally: Captured(false),
                    reloadedPayload: Captured(3))))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: Captured(1),
                    callbackReceiver: Captured(2),
                    callbackReturnedNormally: Captured(true),
                    reloadedPayload: OwnerCallbackQueueDeliveryCapture<uint>.Unreached())))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: Captured(1),
                    callbackReceiver: Captured(2),
                    callbackReturnedNormally: Captured(true),
                    reloadedPayload: Captured(0),
                    counterBeforeXadd: Captured(1))))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: Captured(1),
                    callbackReceiver: Captured(2),
                    callbackReturnedNormally: Captured(true),
                    reloadedPayload: Captured(3),
                    counterBeforeXadd:
                        OwnerCallbackQueueDeliveryCapture<uint>.Unreached())))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: Captured(1),
                    callbackReceiver: Captured(2),
                    callbackReturnedNormally: Captured(true),
                    reloadedPayload: Captured(3),
                    counterBeforeXadd: OwnerCallbackQueueDeliveryCapture<uint>.Missing(),
                    releaseCallReturnedNormally: Captured(false))))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: Captured(1),
                    callbackReceiver: Captured(2),
                    callbackReturnedNormally: Captured(true),
                    reloadedPayload: Captured(3),
                    counterBeforeXadd: Captured(2),
                    releaseCallReturnedNormally: Captured(true))))
            && ThrowsArgumentException(() => OwnerCallbackQueueDeliveryProjection.Project(
                Captures(poppedPayload: Captured(1),
                    callbackReceiver: Captured(2),
                    callbackReturnedNormally: Captured(true),
                    reloadedPayload: Captured(3),
                    counterBeforeXadd: Captured(1),
                    releaseCallReturnedNormally:
                        OwnerCallbackQueueDeliveryCapture<bool>.Unreached())));
        check("Missing captures stop only at reached boundaries and any later capture is rejected, including after false call returns",
            rejectsUnreachedOrMissingCapturePaths);
    }

    private static OwnerCallbackQueueDeliveryCaptures Captures(
        OwnerCallbackQueueDeliveryCapture<uint>? poppedPayload = null,
        OwnerCallbackQueueDeliveryCapture<uint>? callbackReceiver = null,
        OwnerCallbackQueueDeliveryCapture<bool>? callbackReturnedNormally = null,
        OwnerCallbackQueueDeliveryCapture<uint>? reloadedPayload = null,
        OwnerCallbackQueueDeliveryCapture<uint>? counterBeforeXadd = null,
        OwnerCallbackQueueDeliveryCapture<bool>? releaseCallReturnedNormally = null) =>
        new(
            poppedPayload ?? OwnerCallbackQueueDeliveryCapture<uint>.Captured(1),
            callbackReceiver ?? OwnerCallbackQueueDeliveryCapture<uint>.Unreached(),
            callbackReturnedNormally ?? OwnerCallbackQueueDeliveryCapture<bool>.Unreached(),
            reloadedPayload ?? OwnerCallbackQueueDeliveryCapture<uint>.Unreached(),
            counterBeforeXadd ?? OwnerCallbackQueueDeliveryCapture<uint>.Unreached(),
            releaseCallReturnedNormally ?? OwnerCallbackQueueDeliveryCapture<bool>.Unreached());

    private static OwnerCallbackQueueDeliveryCapture<uint> Captured(uint value) =>
        OwnerCallbackQueueDeliveryCapture<uint>.Captured(value);

    private static OwnerCallbackQueueDeliveryCapture<bool> Captured(bool value) =>
        OwnerCallbackQueueDeliveryCapture<bool>.Captured(value);

    private static bool HasCapturedSlice(
        OwnerCallbackQueueDeliveryInstructionEvidence instruction)
    {
        string[] captured = OwnerCallbackQueueDeliveryProjection.CapturedEvidenceBytes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] bytes = instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int start = checked((int)(
            instruction.RawFileOffset - OwnerCallbackQueueDeliveryProjection.EntryRawFileOffset));
        return start >= 0
            && start + bytes.Length <= captured.Length
            && captured.Skip(start).Take(bytes.Length).SequenceEqual(bytes);
    }

    private static bool ShortBranchTargets(
        uint rawFileOffset,
        string instructionHex,
        uint expectedTarget)
    {
        byte[] instruction = Convert.FromHexString(
            instructionHex.Replace(" ", string.Empty, StringComparison.Ordinal));
        return instruction.Length == 2
            && instruction[0] is 0x74 or 0x75
            && rawFileOffset + instruction.Length + (sbyte)instruction[1] == expectedTarget;
    }

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
