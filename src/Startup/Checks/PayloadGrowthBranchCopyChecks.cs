using Firefall.Reconstruction.Startup;

internal static class PayloadGrowthBranchCopyChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The two IDA raw-byte windows, exact branch/call boundaries, VA mapping, and sample identity are pinned",
            PayloadGrowthBranchCopyProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadGrowthBranchCopyProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadGrowthBranchCopyProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PayloadGrowthBranchCopyProjection.TakenBranchRawFileOffset == 0x00BE69A7
            && PayloadGrowthBranchCopyProjection.FallBranchRawFileOffset == 0x00BE6960
            && PayloadGrowthBranchCopyProjection.FallCallTargetReadRawFileOffset
                == 0x00BE6978
            && PayloadGrowthBranchCopyProjection.FallFrameDwordMinus10ReadRawFileOffset
                == 0x00BE697F
            && PayloadGrowthBranchCopyProjection.FallCallRawFileOffset == 0x00BE6982
            && PayloadGrowthBranchCopyProjection.FallNormalBoundaryRawFileOffsetExclusive
                == 0x00BE6984
            && PayloadGrowthBranchCopyProjection.TakenCallTargetReadRawFileOffset
                == 0x00BE69B4
            && PayloadGrowthBranchCopyProjection.TakenCallRawFileOffset == 0x00BE69BB
            && PayloadGrowthBranchCopyProjection.TakenNormalBoundaryRawFileOffsetExclusive
                == 0x00BE69BD
            && PayloadGrowthBranchCopyProjection.RawFileOffsetToVirtualAddress(0x00BE6960)
                == 0x00FE7560
            && PayloadGrowthBranchCopyProjection.RawFileOffsetToVirtualAddress(0x00BE69A7)
                == 0x00FE75A7
            && PayloadGrowthBranchCopyProjection.CallTargetDwordAddress == 0x01AC489C
            && string.Concat(PayloadGrowthBranchCopyProjection.FallCapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8D04BD000000008BD6C1F8022BD78955F48D1C8500000000A19C48AC015351"
                    + "FF75F0FFD0"
            && string.Concat(PayloadGrowthBranchCopyProjection.TakenCapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8D3CB5000000008BC783E0FC50A19C48AC015152FFD0"
            && VerifyInstructionEvidence());

        PayloadGrowthBranchCopyPlan unsignedEquality = Project(
            retainedEdi: uint.MaxValue,
            growthDeltaEsi: uint.MaxValue);
        PayloadGrowthBranchCopyPlan highBitTaken = Project(
            retainedEdi: 0x80000000,
            growthDeltaEsi: 0x7FFFFFFF);
        PayloadGrowthBranchCopyPlan highBitFall = Project(
            retainedEdi: 0x7FFFFFFF,
            growthDeltaEsi: 0x80000000);
        check("The prior JA selection is strictly unsigned, including equality and both high-bit orderings",
            !unsignedEquality.TakenBranch
            && !unsignedEquality.UnsignedGreaterThan
            && unsignedEquality.RouteRawFileOffset == 0x00BE6960
            && highBitTaken.TakenBranch
            && highBitTaken.UnsignedGreaterThan
            && highBitTaken.RouteRawFileOffset == 0x00BE69A7
            && !highBitFall.TakenBranch
            && !highBitFall.UnsignedGreaterThan
            && highBitFall.RouteRawFileOffset == 0x00BE6960);

        PayloadGrowthBranchCopyPlan fall = Project(
            retainedEdi: 0x60000000,
            growthDeltaEsi: 0x70000000,
            receiverEcx: 0x12345678,
            savedPointerEdx: 0xDEADBEEF,
            framePointer: 0x1000,
            callTarget: 0,
            frameMinus10: 0xABCDEF01,
            returnedNormally: true);
        check("The fall path preserves signed SAR before wrapped scaling, its frame-C-only write, and count/source/destination stack order",
            fall.Outcome == PayloadGrowthBranchCopyOutcome.ReturnedNormally
            && !fall.TakenBranch
            && fall.FallScaledEax == 0x80000000
            && fall.FallSignedArithmeticShiftResult == -0x20000000
            && fall.FallSignedArithmeticShiftResultBits == 0xE0000000
            && fall.FallScaledEbx == 0x80000000
            && fall.FallEdxAfterSubtract == 0x10000000
            && fall.FrameDwordMinusCWriteAddress == 0xFF4
            && fall.FrameDwordMinusCWriteValue == 0x10000000
            && fall.CallTargetRead
            && fall.CallTargetDword == 0
            && fall.FrameDwordMinus10ReadAddress == 0xFF0
            && fall.FrameDwordMinus10Read
            && fall.FrameDwordMinus10ReadValue == 0xABCDEF01
            && fall.CountPushed
            && fall.SourcePushed
            && fall.DestinationPushed
            && fall.CountStackArgument == 0x80000000
            && fall.SourceStackArgument == 0x12345678
            && fall.DestinationStackArgument == 0xABCDEF01
            && fall.CallEntryEsp4 == 0xABCDEF01
            && fall.CallEntryEsp8 == 0x12345678
            && fall.CallEntryEspC == 0x80000000
            && fall.CallRawFileOffset == 0x00BE6982
            && fall.CallReached
            && fall.CallReturnedNormally == true
            && fall.NormalBoundaryRawFileOffsetExclusive == 0x00BE6984
            && fall.StopAtRawFileOffsetExclusive == 0x00BE6984);

        PayloadGrowthBranchCopyPlan wrappedFall = Project(
            retainedEdi: uint.MaxValue,
            growthDeltaEsi: uint.MaxValue,
            framePointer: 8,
            callTarget: 0x100,
            frameMinus10: 0,
            returnedNormally: true);
        check("Fall-path DWORD arithmetic and frame addresses wrap, while a captured zero destination remains valid",
            wrappedFall.FallSignedArithmeticShiftResult == -1
            && wrappedFall.FallSignedArithmeticShiftResultBits == uint.MaxValue
            && wrappedFall.FallScaledEbx == 0xFFFFFFFC
            && wrappedFall.FallEdxAfterSubtract == 0
            && wrappedFall.FrameDwordMinusCWriteAddress == 0xFFFFFFFC
            && wrappedFall.FrameDwordMinusCWriteValue == 0
            && wrappedFall.FrameDwordMinus10ReadAddress == 0xFFFFFFF8
            && wrappedFall.FrameDwordMinus10Read
            && wrappedFall.FrameDwordMinus10ReadValue == 0);

        PayloadGrowthBranchCopyPlan taken = Project(
            retainedEdi: 0xB0000000,
            growthDeltaEsi: 0xA0000000,
            receiverEcx: 0x11223344,
            savedPointerEdx: 0x55667788,
            framePointer: 0x2000,
            callTarget: 0,
            frameMinus10: null,
            returnedNormally: true);
        check("The taken path scales ESI, applies the explicit mask, retains EDX as destination, and never writes or reloads frame locals",
            taken.Outcome == PayloadGrowthBranchCopyOutcome.ReturnedNormally
            && taken.TakenBranch
            && taken.TakenScaledEdi == 0x80000000
            && taken.TakenMaskedEax == 0x80000000
            && taken.CallTargetRead
            && taken.CallTargetDword == 0
            && taken.FrameDwordMinusCWriteAddress is null
            && taken.FrameDwordMinusCWriteValue is null
            && taken.FrameDwordMinus10ReadAddress is null
            && !taken.FrameDwordMinus10Read
            && taken.CountPushed
            && taken.SourcePushed
            && taken.DestinationPushed
            && taken.CountStackArgument == 0x80000000
            && taken.SourceStackArgument == 0x11223344
            && taken.DestinationStackArgument == 0x55667788
            && taken.CallEntryEsp4 == 0x55667788
            && taken.CallEntryEsp8 == 0x11223344
            && taken.CallEntryEspC == 0x80000000
            && taken.CallRawFileOffset == 0x00BE69BB
            && taken.CallReached
            && taken.CallReturnedNormally == true
            && taken.NormalBoundaryRawFileOffsetExclusive == 0x00BE69BD
            && taken.StopAtRawFileOffsetExclusive == 0x00BE69BD);

        PayloadGrowthBranchCopyPlan missingFallTarget = Project(
            callTarget: null,
            frameMinus10: null,
            returnedNormally: null);
        PayloadGrowthBranchCopyPlan missingFallDestination = Project(
            callTarget: 1,
            frameMinus10: null,
            returnedNormally: null);
        PayloadGrowthBranchCopyPlan laterFallCapture = Project(
            callTarget: null,
            frameMinus10: 0,
            returnedNormally: null);
        check("Fall-path missing captures stop in instruction order and reject evidence from unreached instructions",
            missingFallTarget.Outcome
                == PayloadGrowthBranchCopyOutcome.MissingCallTargetCapture
            && missingFallTarget.FrameDwordMinusCWriteValue == 0
            && !missingFallTarget.CallTargetRead
            && !missingFallTarget.CountPushed
            && missingFallTarget.StopAtRawFileOffsetExclusive == 0x00BE6978
            && missingFallDestination.Outcome
                == PayloadGrowthBranchCopyOutcome.MissingFrameDwordMinus10Capture
            && missingFallDestination.CallTargetRead
            && missingFallDestination.CountPushed
            && missingFallDestination.SourcePushed
            && !missingFallDestination.DestinationPushed
            && !missingFallDestination.CallReached
            && missingFallDestination.StopAtRawFileOffsetExclusive == 0x00BE697F
            && laterFallCapture.Outcome
                == PayloadGrowthBranchCopyOutcome.UnreachedCaptureProvided
            && !laterFallCapture.CallTargetRead);

        PayloadGrowthBranchCopyPlan missingTakenTarget = Project(
            retainedEdi: 1,
            growthDeltaEsi: 0,
            callTarget: null,
            frameMinus10: null,
            returnedNormally: null);
        PayloadGrowthBranchCopyPlan takenWithUnreachedFrame10 = Project(
            retainedEdi: 1,
            growthDeltaEsi: 0,
            callTarget: 0,
            frameMinus10: 0,
            returnedNormally: true);
        PayloadGrowthBranchCopyPlan takenStatusWithoutTarget = Project(
            retainedEdi: 1,
            growthDeltaEsi: 0,
            callTarget: null,
            frameMinus10: null,
            returnedNormally: true);
        check("Taken-path captures reject the fall-only frame-10 reload and any return status without a reached call",
            missingTakenTarget.Outcome
                == PayloadGrowthBranchCopyOutcome.MissingCallTargetCapture
            && missingTakenTarget.CountPushed
            && !missingTakenTarget.CallTargetRead
            && missingTakenTarget.StopAtRawFileOffsetExclusive == 0x00BE69B4
            && takenWithUnreachedFrame10.Outcome
                == PayloadGrowthBranchCopyOutcome.UnreachedCaptureProvided
            && takenWithUnreachedFrame10.CallTargetRead
            && takenWithUnreachedFrame10.CallReached
            && takenWithUnreachedFrame10.CallReturnedNormally == true
            && takenWithUnreachedFrame10.NormalBoundaryRawFileOffsetExclusive is null
            && takenWithUnreachedFrame10.StopAtRawFileOffsetExclusive == 0x00BE69BB
            && takenStatusWithoutTarget.Outcome
                == PayloadGrowthBranchCopyOutcome.UnreachedCaptureProvided
            && !takenStatusWithoutTarget.CallReached);

        PayloadGrowthBranchCopyPlan unknownFallReturn = Project(
            callTarget: 0x200,
            frameMinus10: 0x300,
            returnedNormally: null);
        PayloadGrowthBranchCopyPlan nonreturningTakenCall = Project(
            retainedEdi: 1,
            growthDeltaEsi: 0,
            callTarget: 0x200,
            frameMinus10: null,
            returnedNormally: false);
        check("Call-target DWORD, call reachability, and normal-return status remain separate; no unproven return is completed",
            unknownFallReturn.Outcome
                == PayloadGrowthBranchCopyOutcome.CallReturnStatusNotCaptured
            && unknownFallReturn.CallTargetDword == 0x200
            && unknownFallReturn.CallReached
            && unknownFallReturn.CallReturnedNormally is null
            && unknownFallReturn.NormalBoundaryRawFileOffsetExclusive is null
            && nonreturningTakenCall.Outcome
                == PayloadGrowthBranchCopyOutcome.CallDidNotReturn
            && nonreturningTakenCall.CallReached
            && nonreturningTakenCall.CallReturnedNormally == false
            && nonreturningTakenCall.NormalBoundaryRawFileOffsetExclusive is null);
    }

    private static PayloadGrowthBranchCopyPlan Project(
        uint retainedEdi = 0x20,
        uint growthDeltaEsi = 0x20,
        uint receiverEcx = 0x30,
        uint savedPointerEdx = 0x40,
        uint framePointer = 0x2000,
        uint? callTarget = 0x100,
        uint? frameMinus10 = 0x50,
        bool? returnedNormally = true) =>
        PayloadGrowthBranchCopyProjection.Project(new(
            retainedEdi,
            growthDeltaEsi,
            receiverEcx,
            savedPointerEdx,
            framePointer,
            callTarget,
            frameMinus10,
            returnedNormally));

    private static bool VerifyInstructionEvidence()
    {
        foreach (PayloadGrowthBranchCopyPath path in
            Enum.GetValues<PayloadGrowthBranchCopyPath>())
        {
            PayloadGrowthBranchCopyInstructionEvidence[] instructions =
                PayloadGrowthBranchCopyProjection.Instructions
                    .Where(instruction => instruction.Path == path)
                    .ToArray();
            if (instructions.Length == 0)
            {
                return false;
            }

            uint nextRawOffset = path == PayloadGrowthBranchCopyPath.Fall
                ? PayloadGrowthBranchCopyProjection.FallBranchRawFileOffset
                : PayloadGrowthBranchCopyProjection.TakenBranchRawFileOffset;
            string instructionBytes = string.Empty;
            foreach (PayloadGrowthBranchCopyInstructionEvidence instruction in instructions)
            {
                if (instruction.RawFileOffset != nextRawOffset
                    || instruction.VirtualAddress
                        != PayloadGrowthBranchCopyProjection.RawFileOffsetToVirtualAddress(
                            instruction.RawFileOffset))
                {
                    return false;
                }

                string bytes = string.Concat(
                    instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                instructionBytes += bytes;
                nextRawOffset += (uint)(bytes.Length / 2);
            }

            uint expectedEnd = path == PayloadGrowthBranchCopyPath.Fall
                ? PayloadGrowthBranchCopyProjection.FallNormalBoundaryRawFileOffsetExclusive
                : PayloadGrowthBranchCopyProjection.TakenNormalBoundaryRawFileOffsetExclusive;
            string expectedBytes = string.Concat(
                (path == PayloadGrowthBranchCopyPath.Fall
                    ? PayloadGrowthBranchCopyProjection.FallCapturedCodeBytes
                    : PayloadGrowthBranchCopyProjection.TakenCapturedCodeBytes)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (nextRawOffset != expectedEnd || instructionBytes != expectedBytes)
            {
                return false;
            }
        }

        return true;
    }
}
