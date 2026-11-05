using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class NetworkIoErrorCallbackExitReturnTransferChecks
{
    public static void Run(Action<string, bool> check)
    {
        ArgumentNullException.ThrowIfNull(check);

        check("The callback-exit transfer is the near RET at E7 with no native execution",
            NetworkIoErrorCallbackExitReturnTransferProjection.EntryRawFileOffset
                == 0x00BBC9E7
            && NetworkIoErrorCallbackExitReturnTransferProjection.RETRawFileOffset
                == 0x00BBC9E7
            && NetworkIoErrorCallbackExitReturnTransferProjection.RETOpcode == 0xC3
            && NetworkIoErrorCallbackExitReturnTransferProjection.RETIsNear
            && NetworkIoErrorCallbackExitReturnTransferProjection.RETImmediateByteCount == 0
            && NetworkIoErrorCallbackExitReturnTransferProjection.RETInstructionByteLength == 1
            && NetworkIoErrorCallbackExitReturnTransferProjection.CapturedWindowBytes == "C3"
            && NetworkIoErrorCallbackExitReturnTransferProjection.RETInstructionBytes
                .SequenceEqual(new byte[] { 0xC3 })
            && NetworkIoErrorCallbackExitReturnTransferProjection.DwordWidth == 4
            && NetworkIoErrorCallbackExitReturnTransferProjection.ReturnAddressBytes == 4
            && NetworkIoErrorCallbackExitReturnTransferProjection.StackPopIncrement == 4
            && NetworkIoErrorCallbackExitReturnTransferProjection.ReturnTargetStackOffset == 0
            && !NetworkIoErrorCallbackExitReturnTransferProjection.ExecutesNativeInstructions);

        NetworkIoErrorCallbackExitReturnTransferPlan zeroTarget = Project(
            returnTarget: 0,
            freshEBP: 0x2000,
            freshSavedEBP: 0xAABBCCDD);
        check("Zero is a valid unvalidated runtime destination and RET pops from the reset ESP",
            zeroTarget.ParentPlan.ExactCleanCompletion
            && zeroTarget.ParentPlan.ESP == 0x2004
            && zeroTarget.ParentPlan.EBP == 0xAABBCCDD
            && zeroTarget.ParentPlan.RestoredEBP == 0xAABBCCDD
            && zeroTarget.ParentPlan.EBPSourceAtStop
                == NetworkIoErrorCallbackExitFrameResetEBPSource.FreshSavedEBPDWORD
            && zeroTarget.ParentPlan.EBX
                == zeroTarget.ParentPlan.ParentExitEbxRestorePlan.EBX
            && zeroTarget.Outcome
                == NetworkIoErrorCallbackExitReturnTransferOutcome
                    .LogicalRETTransferCompleted
            && zeroTarget.Disposition
                == NetworkIoErrorCallbackExitReturnTransferDisposition
                    .BoundedAfterLogicalRETTransfer
            && zeroTarget.ReturnTargetCaptureState
                == NetworkIoErrorCallbackExitReturnTransferCaptureState.Captured
            && zeroTarget.ReturnTargetReadAddress == 0x2004
            && zeroTarget.Reads is [
                { InstructionRawFileOffset: 0x00BBC9E7, Address: 0x2004, Width: 4,
                    Attempted: true, Captured: true, Value: 0, Committed: false }
            ]
            && zeroTarget.Transfer is {
                TargetVirtualAddress: 0,
                ESPBeforeRET: 0x2004,
                ESPAfterRET: 0x2008,
                ReturnAddressBytes: 4,
                InstructionRawFileOffset: 0x00BBC9E7
            }
            && zeroTarget.SelectedDestinationVirtualAddress == 0
            && zeroTarget.Transfer?.ESPAfterRET
                == unchecked(zeroTarget.Capture.ParentExitFrameResetInputs
                    .FreshPostCallEBP!.Value + 8)
            && zeroTarget.Transfer?.ESPAfterRET
                != unchecked(zeroTarget.ParentPlan.RestoredEBP!.Value + 8)
            && zeroTarget.StopAtRawFileOffsetExclusive is null
            && zeroTarget.LastReachedRawFileOffset == 0x00BBC9E7
            && zeroTarget.ReachedInstructionRawFileOffsets
                .SequenceEqual(new uint[] { 0x00BBC9E7 })
            && zeroTarget.ReturnTransferBoundaryReached
            && zeroTarget.ChildBoundedCompletionBoundaryReached
            && zeroTarget.ExactChildCompletion
            && zeroTarget.BoundedCompletionBoundaryReached
            && zeroTarget.ExactBoundedCompletion
            && zeroTarget.ExactCleanCompletion
            && zeroTarget.LogicalRETStackAdjustmentProjected
            && zeroTarget.FlagsAtStop is null
            && !zeroTarget.NativeInstructionsExecuted
            && !zeroTarget.NativeReadsExecuted
            && !zeroTarget.NativeRETExecuted
            && !zeroTarget.NativeMemoryWritesCommitted
            && !zeroTarget.ReturnTargetEntered
            && !zeroTarget.ReturnTargetIsRawFileOffset);

        NetworkIoErrorCallbackExitReturnTransferPlan maximumTarget = Project(
            returnTarget: uint.MaxValue,
            freshEBP: 0x3000,
            freshSavedEBP: 0x76543210);
        check("A maximum DWORD remains an independent runtime scalar destination",
            maximumTarget.ReturnTargetReadAddress == 0x3004
            && maximumTarget.Reads.Single().Value == uint.MaxValue
            && maximumTarget.Transfer?.TargetVirtualAddress == uint.MaxValue
            && maximumTarget.SelectedDestinationVirtualAddress == uint.MaxValue
            && maximumTarget.ExactCleanCompletion
            && !maximumTarget.ReturnTargetEntered
            && !maximumTarget.ReturnTargetIsRawFileOffset);

        NetworkIoErrorCallbackExitReturnTransferPlan wrapsToZero = Project(
            returnTarget: 0x12345678,
            freshEBP: 0xFFFFFFF8,
            freshSavedEBP: 0xA1B2C3D4);
        check("RET advances from the frame-reset ESP with DWORD wraparound",
            wrapsToZero.ParentPlan.ESP == 0xFFFFFFFC
            && wrapsToZero.ParentPlan.EBP == 0xA1B2C3D4
            && wrapsToZero.Transfer?.ESPBeforeRET == 0xFFFFFFFC
            && wrapsToZero.Transfer?.ESPAfterRET == 0
            && wrapsToZero.Transfer?.ESPAfterRET
                == unchecked(wrapsToZero.Capture.ParentExitFrameResetInputs
                    .FreshPostCallEBP!.Value + 8)
            && wrapsToZero.ESPAfterRET == 0
            && wrapsToZero.ExactCleanCompletion);

        NetworkIoErrorCallbackExitReturnTransferPlan missingTarget = Project(
            returnTarget: null,
            freshEBP: 0x4000,
            freshSavedEBP: 0xB1B1B1B1);
        check("A missing fresh return target attempts its E7 read and leaves the parent complete",
            missingTarget.ParentPlan.ExactCleanCompletion
            && missingTarget.ParentPlan.ESP == 0x4004
            && missingTarget.Outcome
                == NetworkIoErrorCallbackExitReturnTransferOutcome
                    .MissingFreshReturnTargetDWORD
            && missingTarget.Disposition
                == NetworkIoErrorCallbackExitReturnTransferDisposition.BoundedBeforeRET
            && missingTarget.ProjectionEntered
            && missingTarget.ReturnTargetCaptureState
                == NetworkIoErrorCallbackExitReturnTransferCaptureState.Missing
            && missingTarget.Reads is [
                { InstructionRawFileOffset: 0x00BBC9E7, Address: 0x4004, Width: 4,
                    Attempted: true, Captured: false, Value: null, Committed: false }
            ]
            && missingTarget.ReachedInstructionRawFileOffsets.IsEmpty
            && missingTarget.LastReachedRawFileOffset == 0x00BBC9E6
            && missingTarget.StopAtRawFileOffsetExclusive == 0x00BBC9E7
            && missingTarget.SelectedDestinationVirtualAddress is null
            && missingTarget.Transfer is null
            && missingTarget.ESPBeforeRET == 0x4004
            && missingTarget.ESPAfterRET is null
            && !missingTarget.ReturnTransferBoundaryReached
            && !missingTarget.ChildBoundedCompletionBoundaryReached
            && !missingTarget.ExactChildCompletion
            && !missingTarget.BoundedCompletionBoundaryReached
            && !missingTarget.ExactBoundedCompletion
            && !missingTarget.ExactCleanCompletion
            && missingTarget.ParentExactBoundedCompletion
            && missingTarget.FlagsAtStop == missingTarget.ParentPlan.FlagsAtStop);

        NetworkIoErrorCallbackExitReturnTransferPlan aliasedStack = Project(
            returnTarget: 0xDEADBEEF,
            freshEBP: 0x2000,
            freshSavedEBP: 0xA1A2A3A4,
            parentPreCallESP: 0x2000);
        check("The fresh target read is independent of the saved-EBP read and aliased earlier POPs",
            aliasedStack.ParentPlan.Reads.Single().Address == 0x2000
            && aliasedStack.ParentPlan.Reads.Single().Value == 0xA1A2A3A4
            && aliasedStack.ParentPlan.ESP == 0x2004
            && aliasedStack.ReturnTargetReadAddress == 0x2004
            && aliasedStack.ParentPlan.ParentExitEbxRestorePlan.ParentExitCallReturnPlan
                .ParentExitPopPlan.Reads.Any(read =>
                    read.Address == 0x2004 && read.Value == 0x4444)
            && aliasedStack.Reads.Single().Value == 0xDEADBEEF
            && aliasedStack.Reads.Single().Value
                != aliasedStack.ParentPlan.Reads.Single().Value
            && aliasedStack.Transfer?.ESPBeforeRET == 0x2004
            && aliasedStack.Transfer?.ESPAfterRET == 0x2008);

        NetworkIoErrorCallbackExitReturnTransferPlan missingEBP = Project(
            returnTarget: 0x55,
            freshEBP: null,
            freshSavedEBP: 0x66);
        NetworkIoErrorCallbackExitReturnTransferPlan missingSavedEBP = Project(
            returnTarget: 0x55,
            freshEBP: 0x7000,
            freshSavedEBP: null);
        NetworkIoErrorCallbackExitReturnTransferPlan pendingParent = Project(
            returnTarget: 0x55,
            freshEBP: 0x8000,
            freshSavedEBP: 0x66,
            returnedNormally: null);
        NetworkIoErrorCallbackExitReturnTransferPlan falseParent = Project(
            returnTarget: 0x55,
            freshEBP: 0x8000,
            freshSavedEBP: 0x66,
            returnedNormally: false);
        NetworkIoErrorCallbackExitReturnTransferPlan rejectedParent = Project(
            returnTarget: 0x55,
            freshEBP: 0x8000,
            freshSavedEBP: 0x66,
            comparedByte: 0);
        NetworkIoErrorCallbackExitReturnTransferPlan dirtyParent = Project(
            returnTarget: 0x55,
            freshEBP: 0x8000,
            freshSavedEBP: 0x66,
            callbackReturnedNormally: true);
        check("Incomplete and rejected parents retain bounds and leave the target unused",
            missingEBP.ParentPlan.Outcome
                == NetworkIoErrorCallbackExitFrameResetOutcome.MissingFreshPostCallEBP
            && missingEBP.ParentPlan.StopBeforeRawFileOffset == 0xBBC9E4
            && missingSavedEBP.ParentPlan.Outcome
                == NetworkIoErrorCallbackExitFrameResetOutcome.MissingFreshSavedEBPDWORD
            && missingSavedEBP.ParentPlan.StopBeforeRawFileOffset == 0xBBC9E6
            && pendingParent.ParentPlan.Outcome
                == NetworkIoErrorCallbackExitFrameResetOutcome
                    .ParentNotAtMoveESPToEBPBoundary
            && falseParent.ParentPlan.Outcome
                == NetworkIoErrorCallbackExitFrameResetOutcome
                    .ParentNotAtMoveESPToEBPBoundary
            && rejectedParent.ParentPlan.ParentExitEbxRestorePlan.ParentExitCallReturnPlan
                .ParentExitPopPlan.ParentPostLoopByteGatePlan.Outcome
                == NetworkIoErrorCallbackPostLoopByteGateOutcome.BranchTo8B0
            && dirtyParent.ParentPlan.ParentExitEbxRestorePlan.ParentExitCallReturnPlan
                .ParentExitPopPlan.ParentPostLoopByteGatePlan.ParentLoopGatePlan
                .ParentReturnPlan.ParentPreparationPlan.ParentPassPlan.Attempts[0]
                .Capture.CallbackReturnedNormally == true
            && ParentRejectedAndUnused(missingEBP)
            && ParentRejectedAndUnused(missingSavedEBP)
            && ParentRejectedAndUnused(pendingParent)
            && ParentRejectedAndUnused(falseParent)
            && ParentRejectedAndUnused(rejectedParent)
            && ParentRejectedAndUnused(dirtyParent));

        NetworkIoErrorCallbackExitReturnTransferPlan original = Project(
            returnTarget: 0x01020304,
            freshEBP: 0x9000,
            freshSavedEBP: 0x11223344);
        NetworkIoErrorCallbackExitReturnTransferPlan reprojected =
            NetworkIoErrorCallbackExitReturnTransferProjection.Project(
                original.Capture with { });
        check("Reprojection preserves immutable capture semantics and the ordered parent prefix",
            reprojected.Capture.FreshReturnTargetDWORD
                == original.Capture.FreshReturnTargetDWORD
            && reprojected.Outcome == original.Outcome
            && reprojected.ReturnTargetCaptureState == original.ReturnTargetCaptureState
            && reprojected.Reads.SequenceEqual(original.Reads)
            && reprojected.Transfer == original.Transfer
            && reprojected.ParentPlan.Outcome == original.ParentPlan.Outcome
            && reprojected.ParentPlan.Reads
                .Select(read => (read.RawFileOffset, read.Address, read.Width,
                    read.Attempted, read.Captured, read.Value, read.NativeReadExecuted))
                .SequenceEqual(original.ParentPlan.Reads
                    .Select(read => (read.RawFileOffset, read.Address, read.Width,
                        read.Attempted, read.Captured, read.Value, read.NativeReadExecuted)))
            && reprojected.ParentPlan.InstructionOrder
                .Select(instruction => (instruction.Kind, instruction.RawFileOffset,
                    instruction.ESPBefore, instruction.ESPAfter, instruction.EBPBefore,
                    instruction.EBPAfter, instruction.Value, instruction.Completed))
                .SequenceEqual(original.ParentPlan.InstructionOrder
                    .Select(instruction => (instruction.Kind, instruction.RawFileOffset,
                        instruction.ESPBefore, instruction.ESPAfter, instruction.EBPBefore,
                        instruction.EBPAfter, instruction.Value, instruction.Completed)))
            && ParentEffectPrefixPreserved(original)
            && ParentEffectPrefixPreserved(reprojected)
            && original.OrderedEffects.Select(effect => effect.Sequence)
                .SequenceEqual(Enumerable.Range(0, original.OrderedEffects.Length))
            && reprojected.OrderedEffects.Length == original.OrderedEffects.Length);

        check("The successful RET effect follows the complete parent prefix and fresh target read",
            ParentEffectPrefixPreserved(zeroTarget)
            && zeroTarget.OrderedEffects.Length
                == zeroTarget.ParentPlan.OrderedEffects.Length + 2
            && zeroTarget.OrderedEffects
                .Skip(zeroTarget.ParentPlan.OrderedEffects.Length)
                .Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    NetworkIoErrorCallbackExitReturnTransferOrderedEffectKind
                        .ReturnTargetDWORDRead,
                    NetworkIoErrorCallbackExitReturnTransferOrderedEffectKind
                        .LogicalRETTransfer
                })
            && zeroTarget.OrderedEffects[^2].Read == zeroTarget.Reads[0]
            && zeroTarget.OrderedEffects[^1].Transfer == zeroTarget.Transfer
            && zeroTarget.OrderedEffects.Select(effect => effect.Sequence)
                .SequenceEqual(Enumerable.Range(0, zeroTarget.OrderedEffects.Length))
            && zeroTarget.Reads.All(read => !read.Committed));
    }

    private static NetworkIoErrorCallbackExitReturnTransferPlan Project(
        uint? returnTarget,
        uint? freshEBP,
        uint? freshSavedEBP,
        uint parentPreCallESP = 0x2000,
        byte? comparedByte = 1,
        bool? returnedNormally = true,
        bool? callbackReturnedNormally = null) =>
        NetworkIoErrorCallbackExitReturnTransferProjection.Project(Inputs(
            returnTarget,
            freshEBP,
            freshSavedEBP,
            parentPreCallESP,
            comparedByte,
            returnedNormally,
            callbackReturnedNormally));

    private static NetworkIoErrorCallbackExitReturnTransferInputs Inputs(
        uint? returnTarget,
        uint? freshEBP,
        uint? freshSavedEBP,
        uint parentPreCallESP,
        byte? comparedByte,
        bool? returnedNormally,
        bool? callbackReturnedNormally) =>
        new(
            new(
                new(
                    new(
                        new(
                            new(
                                new(
                                    new(
                                        new(
                                    ParentPassCapture: CleanMissingKeyParent(
                                        callbackReturnedNormally),
                                    FreshEBP: 0x1000,
                                    FreshDispatchKeyDWORD: 0,
                                    FreshESP: 0x2000,
                                    FreshVptrDWORD: 0),
                                FreshSlot20TargetDWORD: 0,
                                OpaqueCallReturnedNormally: true),
                            FreshPostCallEBP: 0x1000,
                            FreshLocalMinus8DWORD: 0,
                            FreshLocalMinus10DWORD: 0,
                            FreshPostCallEDI: 99),
                        FreshLocalMinus24DWORD: 0x2000,
                        FreshComparedBYTE: comparedByte,
                        FreshLocalMinus28DWORD: 0x2222),
                    parentPreCallESP,
                    FreshPopEDIDWORD: 0x3333,
                    FreshPopESIDWORD: 0x4444),
                FreshOpaqueCallTargetDWORD: 0x1234,
                OpaqueCallReturnedNormally: returnedNormally),
            FreshPostCallESP: 0x2000,
            FreshSavedEBXDWORD: 0x5678),
            freshEBP,
            freshSavedEBP),
            returnTarget);

    private static NetworkIoCompletionPassCapture CleanMissingKeyParent(
        bool? callbackReturnedNormally) =>
        new(
            StopFlagAtEntry: false,
            AttemptCaptures: ImmutableArray.Create(new NetworkIoCompletionAttemptCapture(
                DequeueSucceeded: false,
                OverlappedPointerFromDequeue: 0x1234,
                CompletionKeyPointerFromDequeue: 0x88,
                GetLastErrorAfterDequeue: 0x2222,
                DiagnosticReturnedNormally: null,
                CompletionKeyPointerAfterDiagnostic: null,
                OverlappedPointerAtSuccessDispatch: null,
                FailedCompletionError: 0x1111,
                CompletionKeyPointerAtErrorDispatch: null,
                CallbackReturnedNormally: callbackReturnedNormally)),
            StopFlagAfterPass: null);

    private static bool ParentRejectedAndUnused(
        NetworkIoErrorCallbackExitReturnTransferPlan plan) =>
        !plan.ProjectionEntered
        && plan.Reads.IsEmpty
        && plan.Transfer is null
        && plan.ReturnTargetCaptureState
            == NetworkIoErrorCallbackExitReturnTransferCaptureState.SuppliedUnused
        && plan.ParentExactBoundedCompletion
            == plan.ParentPlan.ExactBoundedCompletion
        && plan.BoundedCompletionBoundaryReached
            == plan.ParentPlan.BoundedCompletionBoundaryReached
        && plan.ExactBoundedCompletion
            == plan.ParentPlan.ExactBoundedCompletion
        && plan.LastReachedRawFileOffset == plan.ParentPlan.LastReachedRawFileOffset
        && plan.StopAtRawFileOffsetExclusive == plan.ParentPlan.StopBeforeRawFileOffset;

    private static bool ParentEffectPrefixPreserved(
        NetworkIoErrorCallbackExitReturnTransferPlan plan)
    {
        if (plan.OrderedEffects.Length < plan.ParentPlan.OrderedEffects.Length)
        {
            return false;
        }

        for (int index = 0; index < plan.ParentPlan.OrderedEffects.Length; index++)
        {
            NetworkIoErrorCallbackExitReturnTransferOrderedEffect actual =
                plan.OrderedEffects[index];
            NetworkIoErrorCallbackExitFrameResetOrderedEffect expected =
                plan.ParentPlan.OrderedEffects[index];
            if (actual.ParentEffect != expected
                || actual.Kind
                    != NetworkIoErrorCallbackExitReturnTransferOrderedEffectKind
                        .ParentFrameResetEffect
                || actual.Read is not null
                || actual.Transfer is not null)
            {
                return false;
            }
        }

        return true;
    }
}
