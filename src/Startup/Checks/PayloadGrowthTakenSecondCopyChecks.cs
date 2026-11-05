using Firefall.Reconstruction.Startup;

internal static class PayloadGrowthTakenSecondCopyChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE identity, raw range, VA mapping, and exact instruction bytes are pinned",
            PayloadGrowthTakenSecondCopyProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadGrowthTakenSecondCopyProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadGrowthTakenSecondCopyProjection.StartRawFileOffset == 0x00BE69BD
            && PayloadGrowthTakenSecondCopyProjection.PriorCallRawFileOffset == 0x00BE69BB
            && PayloadGrowthTakenSecondCopyProjection.SecondCallRawFileOffset == 0x00BE69D9
            && PayloadGrowthTakenSecondCopyProjection.NormalBoundaryRawFileOffsetExclusive
                == 0x00BE69DF
            && PayloadGrowthTakenSecondCopyProjection.CapturedCodeLength == 34
            && PayloadGrowthTakenSecondCopyProjection.RawFileOffsetToVirtualAddress(
                PayloadGrowthTakenSecondCopyProjection.StartRawFileOffset) == 0x00FE75BD
            && PayloadGrowthTakenSecondCopyProjection.RawFileOffsetToVirtualAddress(
                PayloadGrowthTakenSecondCopyProjection.NormalBoundaryRawFileOffsetExclusive)
                == 0x00FE75DF
            && PayloadGrowthTakenSecondCopyProjection.PendingPriorCallArgumentBytes == 12
            && PayloadGrowthTakenSecondCopyProjection.SecondCallArgumentBytes == 12
            && string.Concat(PayloadGrowthTakenSecondCopyProjection.CapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B43048B55F48D0C072BD103D0C1FA028D1C95000000005351FF75FC"
                    + "FF159C48AC01"
            && VerifyInstructionEvidence());

        PayloadGrowthTakenSecondCopyPlan normal =
            PayloadGrowthTakenSecondCopyProjection.Project(
                new(0x1000, 0x2000, 0x20, true, 0x30, 0x120, 0xABC, true));
        check("The independent reads form wrapped SUB/ADD, signed SAR count, and destination/source/count call slots",
            normal.Outcome == PayloadGrowthTakenSecondCopyOutcome.ReturnedNormally
            && normal.PriorCallArgumentBytesStillPending == 12
            && normal.ReceiverDword4ReadAddress == 0x1004
            && normal.ReceiverDword4Read
            && normal.EaxReloadedFromReceiverDword4 == 0x30
            && normal.FrameDwordMinusCReadAddress == 0x1FF4
            && normal.FrameDwordMinusCRead
            && normal.FrameDwordMinusCReadValue == 0x120
            && normal.SourceEcx == 0x50
            && normal.EdxAfterSubtract == 0xD0
            && normal.EdxAfterAdd == 0x100
            && normal.SignedArithmeticShiftResult == 0x40
            && normal.SignedArithmeticShiftResultBits == 0x40
            && normal.CountEbx == 0x100
            && normal.FrameDwordMinus4ReadAddress == 0x1FFC
            && normal.FrameDwordMinus4Read
            && normal.FrameDwordMinus4ReadValue == 0xABC
            && normal.CountPushed
            && normal.SourcePushed
            && normal.DestinationPushed
            && normal.SecondCallArgumentBytesPushed == 12
            && normal.CallEntryEsp4 == 0xABC
            && normal.CallEntryEsp8 == 0x50
            && normal.CallEntryEspC == 0x100
            && normal.PendingArgumentBytesAtSecondCallEntry == 24
            && normal.CallRawFileOffset == 0x00BE69D9
            && normal.CallReached
            && normal.CallReturnedNormally == true
            && normal.NormalBoundaryRawFileOffsetExclusive == 0x00BE69DF
            && normal.StopAtRawFileOffsetExclusive == 0x00BE69DF);

        PayloadGrowthTakenSecondCopyPlan signedHighBitAndWrappedAddress =
            PayloadGrowthTakenSecondCopyProjection.Project(
                new(0xFFFFFFFC, 8, 0, true, 3, 0x80000003, 0, true));
        check("Address wrap, native-zero frame-4, and signed SAR are retained without using prior-call EAX",
            signedHighBitAndWrappedAddress.ReceiverDword4ReadAddress == 0
            && signedHighBitAndWrappedAddress.FrameDwordMinusCReadAddress == 0xFFFFFFFC
            && signedHighBitAndWrappedAddress.FrameDwordMinus4ReadAddress == 4
            && signedHighBitAndWrappedAddress.FrameDwordMinus4Read
            && signedHighBitAndWrappedAddress.FrameDwordMinus4ReadValue == 0
            && signedHighBitAndWrappedAddress.SourceEcx == 3
            && signedHighBitAndWrappedAddress.EdxAfterSubtract == 0x80000000
            && signedHighBitAndWrappedAddress.EdxAfterAdd == 0x80000003
            && signedHighBitAndWrappedAddress.SignedArithmeticShiftResult == -0x20000000
            && signedHighBitAndWrappedAddress.SignedArithmeticShiftResultBits == 0xE0000000
            && signedHighBitAndWrappedAddress.CountEbx == 0x80000000
            && signedHighBitAndWrappedAddress.CallEntryEsp4 == 0
            && signedHighBitAndWrappedAddress.NormalBoundaryRawFileOffsetExclusive
                == 0x00BE69DF);

        PayloadGrowthTakenSecondCopyPlan wrappedSubtractAndAdd =
            PayloadGrowthTakenSecondCopyProjection.Project(
                new(0, 0, uint.MaxValue, true, 2, 0, 0, true));
        PayloadGrowthTakenSecondCopyPlan wrappedCount =
            PayloadGrowthTakenSecondCopyProjection.Project(
                new(0, 0, 0, true, 0, 0xFFFFFFFF, 0, true));
        check("Subtraction/addition and post-SAR scaling use exact wrapping DWORD arithmetic",
            wrappedSubtractAndAdd.SourceEcx == 1
            && wrappedSubtractAndAdd.EdxAfterSubtract == uint.MaxValue
            && wrappedSubtractAndAdd.EdxAfterAdd == 1
            && wrappedSubtractAndAdd.SignedArithmeticShiftResult == 0
            && wrappedSubtractAndAdd.CountEbx == 0
            && wrappedCount.EdxAfterSubtract == uint.MaxValue
            && wrappedCount.EdxAfterAdd == uint.MaxValue
            && wrappedCount.SignedArithmeticShiftResult == -1
            && wrappedCount.SignedArithmeticShiftResultBits == uint.MaxValue
            && wrappedCount.CountEbx == 0xFFFFFFFC);

        PayloadGrowthTakenSecondCopyPlan missingPriorStatus =
            Project(
                priorNormal: null,
                receiver4: null,
                frameMinusC: null,
                frameMinus4: null,
                secondNormal: null);
        PayloadGrowthTakenSecondCopyPlan failedPriorCall =
            Project(
                priorNormal: false,
                receiver4: null,
                frameMinusC: null,
                frameMinus4: null,
                secondNormal: null);
        PayloadGrowthTakenSecondCopyPlan laterAfterMissingPrior =
            Project(priorNormal: null, receiver4: 0);
        PayloadGrowthTakenSecondCopyPlan missingReceiver4 = Project(
            receiver4: null,
            frameMinusC: null,
            frameMinus4: null,
            secondNormal: null);
        PayloadGrowthTakenSecondCopyPlan laterAfterMissingReceiver4 =
            Project(receiver4: null, frameMinusC: 0);
        PayloadGrowthTakenSecondCopyPlan missingFrameMinusC = Project(
            frameMinusC: null,
            frameMinus4: null,
            secondNormal: null);
        PayloadGrowthTakenSecondCopyPlan laterAfterMissingFrameMinusC =
            Project(frameMinusC: null, frameMinus4: 0);
        PayloadGrowthTakenSecondCopyPlan missingFrameMinus4 =
            Project(frameMinus4: null, secondNormal: null);
        PayloadGrowthTakenSecondCopyPlan laterAfterMissingFrameMinus4 =
            Project(frameMinus4: null, secondNormal: true);
        check("Each missing or nonreturning prerequisite gates later captures in strict instruction order",
            missingPriorStatus.Outcome
                == PayloadGrowthTakenSecondCopyOutcome.PriorCallReturnStatusNotCaptured
            && missingPriorStatus.StopAtRawFileOffsetExclusive == 0x00BE69BB
            && failedPriorCall.Outcome
                == PayloadGrowthTakenSecondCopyOutcome.PriorCallDidNotReturn
            && failedPriorCall.StopAtRawFileOffsetExclusive == 0x00BE69BB
            && laterAfterMissingPrior.Outcome
                == PayloadGrowthTakenSecondCopyOutcome.UnreachedCaptureProvided
            && !laterAfterMissingPrior.ReceiverDword4Read
            && missingReceiver4.Outcome
                == PayloadGrowthTakenSecondCopyOutcome.MissingReceiverDword4Capture
            && missingReceiver4.StopAtRawFileOffsetExclusive == 0x00BE69BD
            && laterAfterMissingReceiver4.Outcome
                == PayloadGrowthTakenSecondCopyOutcome.UnreachedCaptureProvided
            && !laterAfterMissingReceiver4.FrameDwordMinusCRead
            && missingFrameMinusC.Outcome
                == PayloadGrowthTakenSecondCopyOutcome.MissingFrameDwordMinusCCapture
            && missingFrameMinusC.ReceiverDword4Read
            && missingFrameMinusC.StopAtRawFileOffsetExclusive == 0x00BE69C0
            && laterAfterMissingFrameMinusC.Outcome
                == PayloadGrowthTakenSecondCopyOutcome.UnreachedCaptureProvided
            && !laterAfterMissingFrameMinusC.CountPushed
            && missingFrameMinus4.Outcome
                == PayloadGrowthTakenSecondCopyOutcome.MissingFrameDwordMinus4Capture
            && missingFrameMinus4.CountPushed
            && missingFrameMinus4.SourcePushed
            && !missingFrameMinus4.DestinationPushed
            && missingFrameMinus4.SecondCallArgumentBytesPushed == 8
            && missingFrameMinus4.StopAtRawFileOffsetExclusive == 0x00BE69D6
            && laterAfterMissingFrameMinus4.Outcome
                == PayloadGrowthTakenSecondCopyOutcome.UnreachedCaptureProvided
            && !laterAfterMissingFrameMinus4.CallReached);

        PayloadGrowthTakenSecondCopyPlan missingSecondStatus =
            Project(secondNormal: null);
        PayloadGrowthTakenSecondCopyPlan nonreturningSecondCall =
            Project(secondNormal: false);
        check("The second normal-return boundary is withheld unless its call returns",
            missingSecondStatus.Outcome
                == PayloadGrowthTakenSecondCopyOutcome.SecondCallReturnStatusNotCaptured
            && missingSecondStatus.CallReached
            && missingSecondStatus.CallReturnedNormally is null
            && missingSecondStatus.NormalBoundaryRawFileOffsetExclusive is null
            && missingSecondStatus.StopAtRawFileOffsetExclusive == 0x00BE69D9
            && missingSecondStatus.PendingArgumentBytesAtSecondCallEntry == 24
            && nonreturningSecondCall.Outcome
                == PayloadGrowthTakenSecondCopyOutcome.SecondCallDidNotReturn
            && nonreturningSecondCall.CallReached
            && nonreturningSecondCall.CallReturnedNormally == false
            && nonreturningSecondCall.NormalBoundaryRawFileOffsetExclusive is null
            && nonreturningSecondCall.StopAtRawFileOffsetExclusive == 0x00BE69D9);
    }

    private static PayloadGrowthTakenSecondCopyPlan Project(
        bool? priorNormal = true,
        uint? receiver4 = 0x30,
        uint? frameMinusC = 0x120,
        uint? frameMinus4 = 0xABC,
        bool? secondNormal = true) =>
        PayloadGrowthTakenSecondCopyProjection.Project(new(
            0x1000,
            0x2000,
            0x20,
            priorNormal,
            receiver4,
            frameMinusC,
            frameMinus4,
            secondNormal));

    private static bool VerifyInstructionEvidence()
    {
        uint nextRawOffset = PayloadGrowthTakenSecondCopyProjection.StartRawFileOffset;
        string instructionBytes = string.Empty;
        foreach (PayloadGrowthTakenSecondCopyInstructionEvidence instruction
            in PayloadGrowthTakenSecondCopyProjection.Instructions)
        {
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != PayloadGrowthTakenSecondCopyProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            instructionBytes += bytes;
            nextRawOffset += (uint)(bytes.Length / 2);
        }

        return nextRawOffset
                == PayloadGrowthTakenSecondCopyProjection.NormalBoundaryRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(PayloadGrowthTakenSecondCopyProjection.CapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
