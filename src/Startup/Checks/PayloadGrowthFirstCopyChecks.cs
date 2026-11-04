using Firefall.Reconstruction.Startup;

internal static class PayloadGrowthFirstCopyChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE sample hash, 51-byte raw range, VA mapping, and instruction bytes are pinned",
            PayloadGrowthFirstCopyProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadGrowthFirstCopyProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadGrowthFirstCopyProjection.StartRawFileOffset == 0x00BE691A
            && PayloadGrowthFirstCopyProjection.NormalBoundaryRawFileOffsetExclusive
                == 0x00BE694D
            && PayloadGrowthFirstCopyProjection.CapturedCodeLength == 51
            && PayloadGrowthFirstCopyProjection.StartVirtualAddress == 0x00FE751A
            && PayloadGrowthFirstCopyProjection.RawFileOffsetToVirtualAddress(0x00BE694D)
                == 0x00FE754D
            && string.Concat(PayloadGrowthFirstCopyProjection.CapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B4B048D04BD000000008945F48D14088B4308C1E0022BC203C1C1F802"
                    + "8D3C85000000008B45FC0345F4575250FF159C48AC01"
            && VerifyInstructionEvidence());

        PayloadGrowthFirstCopyPlan normal =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 3, 0x100, 0x400, 0x500,
                    0xDEAD, 0xABCD, true));
        check("The independent captures produce the exact opaque-call arguments and normal boundary",
            normal.Outcome == PayloadGrowthFirstCopyOutcome.ReturnedNormally
            && normal.ReceiverDword4ReadAddress == 0x1004
            && normal.ReceiverDword8ReadAddress == 0x1008
            && normal.FrameDwordMinus4ReadAddress == 0x1FFC
            && normal.FrameDwordMinusCReadAddress == 0x1FF4
            && normal.FrameDwordMinusCWriteAddress == 0x1FF4
            && normal.FrameDwordMinusCWriteValue == 12
            && normal.SourceEdx == 0x10C
            && normal.ReceiverDword8ShiftedLeft2 == 0x1000
            && normal.SubtractSourceResult == 0xEF4
            && normal.AddReceiverDword4Result == 0xFF4
            && normal.SignedArithmeticShiftResult == 0x3FD
            && normal.ByteCountEdi == 0xFF4
            && normal.DestinationEax == 0xE3AD
            && normal.CountStackArgument == 0xFF4
            && normal.SourceStackArgument == 0x10C
            && normal.DestinationStackArgument == 0xE3AD
            && normal.CallEntryEsp4 == 0xE3AD
            && normal.CallEntryEsp8 == 0x10C
            && normal.CallEntryEspC == 0xFF4
            && normal.CallReached
            && normal.CallReturnedNormally == true
            && normal.CallReturnEax == 0xABCD
            && normal.NormalBoundaryRawFileOffsetExclusive == 0x00BE694D
            && normal.StopAtRawFileOffsetExclusive == 0x00BE694D);

        PayloadGrowthFirstCopyPlan signBoundary =
            PayloadGrowthFirstCopyProjection.Project(
                new(0, 0, 0x20000000, 0, 0, 0, 0, null, true));
        PayloadGrowthFirstCopyPlan subtractUnderflow =
            PayloadGrowthFirstCopyProjection.Project(
                new(0, 0, 1, 0, 0, 0, 0, null, true));
        check("DWORD subtraction wraps and SAR sign-extends before the scaled byte count",
            signBoundary.FrameDwordMinusCWriteValue == 0x80000000
            && signBoundary.SourceEdx == 0x80000000
            && signBoundary.SubtractSourceResult == 0x80000000
            && signBoundary.AddReceiverDword4Result == 0x80000000
            && signBoundary.SignedArithmeticShiftResult == -0x20000000
            && signBoundary.SignedArithmeticShiftResultBits == 0xE0000000
            && signBoundary.ByteCountEdi == 0x80000000
            && subtractUnderflow.SubtractSourceResult == 0xFFFFFFFC
            && subtractUnderflow.SignedArithmeticShiftResult == -1
            && subtractUnderflow.SignedArithmeticShiftResultBits == uint.MaxValue
            && subtractUnderflow.ByteCountEdi == 0xFFFFFFFC);

        PayloadGrowthFirstCopyPlan wrappedAddressesAndIndependentReload =
            PayloadGrowthFirstCopyProjection.Project(
                new(0xFFFFFFFC, 2, uint.MaxValue, 5, 0, 7, 9, 1, true));
        check("Receiver/frame address arithmetic wraps and the frame-C reload remains independent of its write",
            wrappedAddressesAndIndependentReload.ReceiverDword4ReadAddress == 0
            && wrappedAddressesAndIndependentReload.ReceiverDword8ReadAddress == 4
            && wrappedAddressesAndIndependentReload.FrameDwordMinus4ReadAddress == 0xFFFFFFFE
            && wrappedAddressesAndIndependentReload.FrameDwordMinusCReadAddress == 0xFFFFFFF6
            && wrappedAddressesAndIndependentReload.FrameDwordMinusCWriteValue == 0xFFFFFFFC
            && wrappedAddressesAndIndependentReload.SourceEdx == 1
            && wrappedAddressesAndIndependentReload.DestinationEax == 16);

        PayloadGrowthFirstCopyPlan missingReceiver4 =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 1, null, null, null, null, null));
        PayloadGrowthFirstCopyPlan laterReceiver4 =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 1, null, 0, null, null, null));
        PayloadGrowthFirstCopyPlan missingReceiver8 =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 1, 0, null, null, null, null));
        PayloadGrowthFirstCopyPlan laterReceiver8 =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 1, 0, null, 0, null, null));
        PayloadGrowthFirstCopyPlan missingFrameMinus4 =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 1, 0, 0, null, null, null));
        PayloadGrowthFirstCopyPlan laterFrameMinus4 =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 1, 0, 0, null, 0, null));
        PayloadGrowthFirstCopyPlan missingFrameMinusC =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 1, 0, 0, 0, null, null));
        PayloadGrowthFirstCopyPlan laterFrameMinusC =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 1, 0, 0, 0, null, 0));
        check("Missing strict captures stop at their read gates and reject all later observations",
            missingReceiver4.Outcome
                == PayloadGrowthFirstCopyOutcome.MissingReceiverDword4Capture
            && missingReceiver4.StopAtRawFileOffsetExclusive == 0x00BE691A
            && laterReceiver4.Outcome == PayloadGrowthFirstCopyOutcome.UnreachedCaptureProvided
            && missingReceiver8.Outcome
                == PayloadGrowthFirstCopyOutcome.MissingReceiverDword8Capture
            && missingReceiver8.FrameDwordMinusCWriteValue == 4
            && missingReceiver8.StopAtRawFileOffsetExclusive == 0x00BE692A
            && laterReceiver8.Outcome == PayloadGrowthFirstCopyOutcome.UnreachedCaptureProvided
            && missingFrameMinus4.Outcome
                == PayloadGrowthFirstCopyOutcome.MissingFrameDwordMinus4Capture
            && missingFrameMinus4.StopAtRawFileOffsetExclusive == 0x00BE693E
            && laterFrameMinus4.Outcome
                == PayloadGrowthFirstCopyOutcome.UnreachedCaptureProvided
            && missingFrameMinusC.Outcome
                == PayloadGrowthFirstCopyOutcome.MissingFrameDwordMinusCCapture
            && missingFrameMinusC.DestinationEax is null
            && missingFrameMinusC.StopAtRawFileOffsetExclusive == 0x00BE6941
            && laterFrameMinusC.Outcome
                == PayloadGrowthFirstCopyOutcome.UnreachedCaptureProvided);

        PayloadGrowthFirstCopyPlan nonreturningCall =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 3, 0x100, 0x400, 0x500, 0xDEAD, null, false));
        PayloadGrowthFirstCopyPlan zeroReturn =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 3, 0x100, 0x400, 0x500, 0xDEAD, 0, true));
        PayloadGrowthFirstCopyPlan returnedWithoutEax =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 3, 0x100, 0x400, 0x500, 0xDEAD, null, true));
        PayloadGrowthFirstCopyPlan unknownReturnStatus =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 3, 0x100, 0x400, 0x500, 0xDEAD, null));
        PayloadGrowthFirstCopyPlan eaxWithoutReturn =
            PayloadGrowthFirstCopyProjection.Project(
                new(0x1000, 0x2000, 3, 0x100, 0x400, 0x500, 0xDEAD, 0xABCD, false));
        check("Return status, not optional unused EAX, controls the normal boundary",
            nonreturningCall.Outcome == PayloadGrowthFirstCopyOutcome.CallDidNotReturn
            && nonreturningCall.CallReached
            && nonreturningCall.CallReturnedNormally == false
            && nonreturningCall.CallReturnEax is null
            && nonreturningCall.NormalBoundaryRawFileOffsetExclusive is null
            && nonreturningCall.StopAtRawFileOffsetExclusive == 0x00BE6947
            && nonreturningCall.CallEntryEsp4 == 0xE3AD
            && zeroReturn.Outcome == PayloadGrowthFirstCopyOutcome.ReturnedNormally
            && zeroReturn.CallReturnEax == 0
            && zeroReturn.CallReturnedNormally == true
            && zeroReturn.NormalBoundaryRawFileOffsetExclusive == 0x00BE694D
            && returnedWithoutEax.Outcome == PayloadGrowthFirstCopyOutcome.ReturnedNormally
            && returnedWithoutEax.CallReturnEax is null
            && returnedWithoutEax.NormalBoundaryRawFileOffsetExclusive == 0x00BE694D
            && unknownReturnStatus.Outcome
                == PayloadGrowthFirstCopyOutcome.CallReturnStatusNotCaptured
            && unknownReturnStatus.CallReached
            && unknownReturnStatus.NormalBoundaryRawFileOffsetExclusive is null
            && eaxWithoutReturn.Outcome
                == PayloadGrowthFirstCopyOutcome.UnreachedCaptureProvided
            && eaxWithoutReturn.CallReturnEax is null
            && eaxWithoutReturn.NormalBoundaryRawFileOffsetExclusive is null);
    }

    private static bool VerifyInstructionEvidence()
    {
        uint nextRawOffset = PayloadGrowthFirstCopyProjection.StartRawFileOffset;
        string instructionBytes = string.Empty;
        foreach (PayloadGrowthFirstCopyInstructionEvidence instruction
            in PayloadGrowthFirstCopyProjection.Instructions)
        {
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != PayloadGrowthFirstCopyProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            instructionBytes += string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            nextRawOffset += (uint)instruction.Bytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Length;
        }

        return nextRawOffset
                == PayloadGrowthFirstCopyProjection.NormalBoundaryRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(PayloadGrowthFirstCopyProjection.CapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
