using Firefall.Reconstruction.Startup;

internal static class PayloadGrowthZeroFillChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw continuation windows, shared REP, sample identity, and instruction ordering are pinned",
            PayloadGrowthZeroFillProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadGrowthZeroFillProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadGrowthZeroFillProjection.FallStartRawFileOffset == 0x00BE6984
            && PayloadGrowthZeroFillProjection.FallEndRawFileOffsetExclusive == 0x00BE69A7
            && PayloadGrowthZeroFillProjection.TakenStartRawFileOffset == 0x00BE69DF
            && PayloadGrowthZeroFillProjection.TakenEndRawFileOffsetExclusive == 0x00BE69F2
            && PayloadGrowthZeroFillProjection.SharedStartRawFileOffset == 0x00BE69EE
            && PayloadGrowthZeroFillProjection.SharedEndRawFileOffsetExclusive == 0x00BE69F2
            && string.Concat(PayloadGrowthZeroFillProjection.FallCapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B4DF483C40C8D140385C9740933C08BFAF3AB8B7DF88B5DFC85FF74518BCF8BFBEB47"
            && string.Concat(PayloadGrowthZeroFillProjection.TakenCapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "83C4188D3C038B5DFC85F674068BCE33C0F3AB"
            && string.Concat(PayloadGrowthZeroFillProjection.SharedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "33C0F3AB"
            && VerifyInstructionEvidence());

        PayloadGrowthZeroFillPlan completedFall = Project(
            frameMinusC: 1,
            frameMinus8: 2,
            frameMinus4: 0x5000,
            directionFlag: false,
            firstRepCompleted: true,
            finalRepCompleted: true);
        check("Fall reads frame-C before cleanup, reloads frame-8 only after its first REP, then zeros the frame-4 destination",
            completedFall.Outcome == PayloadGrowthZeroFillOutcome.Completed
            && completedFall.FrameDwordMinusCRead
            && completedFall.FrameDwordMinusCReadAddress == 0x1FF4
            && completedFall.FrameDwordMinusCValue == 1
            && completedFall.StackCleanupBytes == 12
            && completedFall.CallEaxUsed == 0x20
            && completedFall.FirstRepDestination == 0x120
            && completedFall.FirstRepCount == 1
            && completedFall.FirstRep is { DirectionFlag: false, NormalCompletion: true }
            && completedFall.FirstRep.Value.Value == 0
            && completedFall.FirstRep.Value.AddressAt(0) == 0x120
            && completedFall.FirstRep.Value.EcxAfter == 0
            && completedFall.FirstRep.Value.FinalEdi == 0x124
            && completedFall.FrameDwordMinus8Read
            && completedFall.FrameDwordMinus8ReadAddress == 0x1FF8
            && completedFall.FrameDwordMinus8Value == 2
            && completedFall.EdiBeforeFinalRep == 2
            && completedFall.FrameDwordMinus4Read
            && completedFall.FrameDwordMinus4ReadAddress == 0x1FFC
            && completedFall.EbxAfterFrameDwordMinus4Reload == 0x5000
            && completedFall.FinalRep is
                { DwordCount: 2, Destination: 0x5000, DirectionFlag: false, NormalCompletion: true }
            && completedFall.FinalRep.Value.Value == 0
            && completedFall.FinalRep.Value.AddressAt(1) == 0x5004
            && completedFall.FinalRep.Value.EcxAfter == 0
            && completedFall.FinalRep.Value.FinalEdi == 0x5008
            && completedFall.NormalBoundaryRawFileOffsetExclusive == 0x00BE69F2);

        PayloadGrowthZeroFillPlan zeroFirstCount = Project(
            frameMinusC: 0,
            incomingEdi: 3,
            frameMinus4: 0xFFFFFFFE,
            directionFlag: false,
            finalRepCompleted: true);
        PayloadGrowthZeroFillPlan zeroAllCounts = Project(
            frameMinusC: 0,
            incomingEdi: 0,
            frameMinus4: 0,
            directionFlag: null,
            finalRepCompleted: null);
        check("A zero frame-C count skips both the first REP and frame-8 reload, retaining incoming EDI for the later gate",
            zeroFirstCount.Outcome == PayloadGrowthZeroFillOutcome.Completed
            && zeroFirstCount.FirstRep is null
            && zeroFirstCount.FrameDwordMinus8ReadAddress is null
            && !zeroFirstCount.FrameDwordMinus8Read
            && zeroFirstCount.EdiBeforeFinalRep == 3
            && zeroFirstCount.FinalRep is
                { DwordCount: 3, Destination: 0xFFFFFFFE, DirectionFlag: false, NormalCompletion: true }
            && zeroFirstCount.FinalRep.Value.AddressAt(1) == 2
            && zeroFirstCount.FinalRep.Value.FinalEdi == 10
            && zeroAllCounts.Outcome == PayloadGrowthZeroFillOutcome.Completed
            && zeroAllCounts.FirstRep is null
            && zeroAllCounts.FinalRep is null
            && zeroAllCounts.FrameDwordMinus8Read is false
            && zeroAllCounts.EdiBeforeFinalRep == 0);

        PayloadGrowthZeroFillPlan descendingFall = Project(
            frameMinusC: uint.MaxValue,
            frameMinus8: 1,
            frameMinus4: 2,
            directionFlag: true,
            firstRepCompleted: true,
            finalRepCompleted: true);
        PayloadGrowthZeroFillPlan wrappedFrameAddress = Project(
            framePointer: 3,
            frameMinusC: 1,
            frameMinus8: 1,
            frameMinus4: 1,
            directionFlag: false,
            firstRepCompleted: true,
            finalRepCompleted: true);
        check("REP counts and DWORD addresses wrap at 32 bits for DF-clear and DF-set iterations",
            descendingFall.Outcome == PayloadGrowthZeroFillOutcome.Completed
            && descendingFall.FirstRep is
                { DwordCount: uint.MaxValue, Destination: 0x120, DirectionFlag: true, NormalCompletion: true }
            && descendingFall.FirstRep.Value.AddressAt(1) == 0x11C
            && descendingFall.FirstRep.Value.AddressAt(uint.MaxValue - 1) == 0x128
            && descendingFall.FirstRep.Value.FinalEdi == 0x124
            && descendingFall.FirstRep.Value.EcxAfter == 0
            && descendingFall.FinalRep is
                { DwordCount: 1, Destination: 2, DirectionFlag: true, NormalCompletion: true }
            && descendingFall.FinalRep.Value.FinalEdi == 0xFFFFFFFE
            && wrappedFrameAddress.FrameDwordMinusCReadAddress == 0xFFFFFFF7
            && wrappedFrameAddress.FrameDwordMinus8ReadAddress == 0xFFFFFFFB
            && wrappedFrameAddress.FrameDwordMinus4ReadAddress == 0xFFFFFFFF);

        PayloadGrowthZeroFillPlan taken = Project(
            entry: PayloadGrowthZeroFillEntry.Taken,
            retainedEbx: 0xFFFFFFF0,
            callEax: 0x30,
            retainedEsi: 1,
            frameMinus4: 0x8888,
            directionFlag: true,
            finalRepCompleted: true);
        PayloadGrowthZeroFillPlan zeroTakenCount = Project(
            entry: PayloadGrowthZeroFillEntry.Taken,
            retainedEbx: 0x100,
            callEax: 0x20,
            retainedEsi: 0,
            frameMinus4: 0x40,
            directionFlag: null,
            finalRepCompleted: null);
        PayloadGrowthZeroFillPlan maximumTakenCount = Project(
            entry: PayloadGrowthZeroFillEntry.Taken,
            retainedEbx: uint.MaxValue,
            callEax: 1,
            retainedEsi: uint.MaxValue,
            frameMinus4: 0,
            directionFlag: false,
            finalRepCompleted: true);
        check("Taken cleanup precedes the wrapped EBX-plus-EAX destination and frame-4 reload; retained ESI alone gates the shared REP",
            taken.Outcome == PayloadGrowthZeroFillOutcome.Completed
            && taken.StackCleanupBytes == 24
            && taken.TakenEdiAfterLea == 0x20
            && taken.FrameDwordMinus4Read
            && taken.FrameDwordMinus4Value == 0x8888
            && taken.EbxAfterFrameDwordMinus4Reload == 0x8888
            && taken.FinalRep is
                { DwordCount: 1, Destination: 0x20, DirectionFlag: true, NormalCompletion: true }
            && taken.FinalRep.Value.Value == 0
            && taken.FinalRep.Value.FinalEdi == 0x1C
            && zeroTakenCount.Outcome == PayloadGrowthZeroFillOutcome.Completed
            && zeroTakenCount.StackCleanupBytes == 24
            && zeroTakenCount.TakenEdiAfterLea == 0x120
            && zeroTakenCount.FinalRep is null
            && maximumTakenCount.Outcome == PayloadGrowthZeroFillOutcome.Completed
            && maximumTakenCount.FinalRep is
                { DwordCount: uint.MaxValue, Destination: 0, DirectionFlag: false, NormalCompletion: true }
            && maximumTakenCount.FinalRep.Value.FinalEdi == 0xFFFFFFFC);

        PayloadGrowthZeroFillPlan noReturnStatus = Project(
            priorCallReturnedNormally: null,
            frameMinusC: null,
            callEax: null,
            retainedEbx: null,
            frameMinus8: null,
            frameMinus4: null,
            directionFlag: null,
            firstRepCompleted: null,
            finalRepCompleted: null);
        PayloadGrowthZeroFillPlan statusWithUnreachedFrame = Project(
            priorCallReturnedNormally: null,
            frameMinusC: 1);
        PayloadGrowthZeroFillPlan noReturn = Project(
            priorCallReturnedNormally: false,
            frameMinusC: null,
            callEax: null,
            retainedEbx: null,
            frameMinus8: null,
            frameMinus4: null,
            directionFlag: null,
            firstRepCompleted: null,
            finalRepCompleted: null);
        PayloadGrowthZeroFillPlan nonreturnWithLaterCapture = Project(
            priorCallReturnedNormally: false,
            frameMinusC: 1);
        PayloadGrowthZeroFillPlan missingFrameC = Project(
            frameMinusC: null,
            callEax: null,
            retainedEbx: null,
            frameMinus8: null,
            frameMinus4: null,
            directionFlag: null,
            firstRepCompleted: null,
            finalRepCompleted: null);
        PayloadGrowthZeroFillPlan laterThanMissingFrameC = Project(
            frameMinusC: null,
            frameMinus4: 0);
        check("Prior-call status and the first fall frame-C read gate every later capture",
            noReturnStatus.Outcome
                == PayloadGrowthZeroFillOutcome.PriorCallReturnStatusNotCaptured
            && noReturnStatus.StopAtRawFileOffsetExclusive == 0x00BE6984
            && statusWithUnreachedFrame.Outcome
                == PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
            && noReturn.Outcome == PayloadGrowthZeroFillOutcome.PriorCallDidNotReturn
            && nonreturnWithLaterCapture.Outcome
                == PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
            && missingFrameC.Outcome
                == PayloadGrowthZeroFillOutcome.MissingFrameDwordMinusCCapture
            && missingFrameC.StackCleanupBytes is null
            && missingFrameC.FrameDwordMinusCReadAddress == 0x1FF4
            && laterThanMissingFrameC.Outcome
                == PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided);

        PayloadGrowthZeroFillPlan missingCallEax = Project(
            frameMinusC: 0,
            callEax: null,
            incomingEdi: null,
            frameMinus4: null,
            directionFlag: null,
            finalRepCompleted: null);
        PayloadGrowthZeroFillPlan missingFrame8 = Project(
            frameMinusC: 1,
            frameMinus8: null,
            frameMinus4: null,
            firstRepCompleted: true,
            finalRepCompleted: null);
        PayloadGrowthZeroFillPlan unreachedFrame8 = Project(
            frameMinusC: 0,
            frameMinus8: 0);
        PayloadGrowthZeroFillPlan missingFirstRepStatus = Project(
            frameMinusC: 1,
            frameMinus8: null,
            frameMinus4: null,
            firstRepCompleted: null,
            finalRepCompleted: null);
        PayloadGrowthZeroFillPlan failedFirstRep = Project(
            frameMinusC: 1,
            frameMinus8: null,
            frameMinus4: null,
            directionFlag: false,
            firstRepCompleted: false,
            finalRepCompleted: null);
        PayloadGrowthZeroFillPlan missingDirection = Project(
            frameMinusC: 1,
            frameMinus8: null,
            firstRepCompleted: true,
            frameMinus4: null,
            directionFlag: null,
            finalRepCompleted: null);
        check("Missing, out-of-order, and faulted captures stop fall at the exact dependent instruction",
            missingCallEax.Outcome == PayloadGrowthZeroFillOutcome.MissingCallEaxCapture
            && missingCallEax.FrameDwordMinusCRead
            && missingCallEax.StackCleanupBytes == 12
            && missingCallEax.StopAtRawFileOffsetExclusive == 0x00BE698A
            && missingFrame8.Outcome
                == PayloadGrowthZeroFillOutcome.MissingFrameDwordMinus8Capture
            && missingFrame8.FirstRep is { NormalCompletion: true }
            && missingFrame8.FrameDwordMinus8ReadAddress == 0x1FF8
            && !missingFrame8.FrameDwordMinus8Read
            && unreachedFrame8.Outcome
                == PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
            && missingFirstRepStatus.Outcome
                == PayloadGrowthZeroFillOutcome.FirstRepCompletionNotCaptured
            && failedFirstRep.Outcome
                == PayloadGrowthZeroFillOutcome.FirstRepDidNotCompleteNormally
            && failedFirstRep.FirstRep is { NormalCompletion: false }
            && failedFirstRep.FirstRep.Value.EcxAfter is null
            && failedFirstRep.FirstRep.Value.FinalEdi is null
            && !failedFirstRep.FrameDwordMinus8Read
            && !failedFirstRep.FrameDwordMinus4Read
            && missingDirection.Outcome
                == PayloadGrowthZeroFillOutcome.DirectionFlagNotCaptured
            && !missingDirection.FrameDwordMinus8Read);

        PayloadGrowthZeroFillPlan missingFinalStatus = Project(
            frameMinusC: 0,
            incomingEdi: 1,
            frameMinus4: 0x4000,
            directionFlag: false,
            finalRepCompleted: null);
        PayloadGrowthZeroFillPlan failedFinalRep = Project(
            frameMinusC: 0,
            incomingEdi: 1,
            frameMinus4: 0x4000,
            directionFlag: true,
            finalRepCompleted: false);
        PayloadGrowthZeroFillPlan unreachedFinalStatus = Project(
            frameMinusC: 0,
            incomingEdi: 0,
            frameMinus4: 0x4000,
            finalRepCompleted: true);
        PayloadGrowthZeroFillPlan missingFrame4 = Project(
            frameMinusC: 0,
            incomingEdi: 1,
            frameMinus4: null,
            directionFlag: null,
            finalRepCompleted: null);
        check("A missing or faulted final REP gates completion, EDI, and ECX claims; zero-count branches reject final-REP captures",
            missingFinalStatus.Outcome
                == PayloadGrowthZeroFillOutcome.FinalRepCompletionNotCaptured
            && missingFinalStatus.FinalRep is { NormalCompletion: null }
            && missingFinalStatus.FinalRep.Value.EcxAfter is null
            && missingFinalStatus.FinalRep.Value.FinalEdi is null
            && failedFinalRep.Outcome
                == PayloadGrowthZeroFillOutcome.FinalRepDidNotCompleteNormally
            && failedFinalRep.FinalRep is { NormalCompletion: false }
            && failedFinalRep.FinalRep.Value.EcxAfter is null
            && failedFinalRep.FinalRep.Value.FinalEdi is null
            && failedFinalRep.NormalBoundaryRawFileOffsetExclusive is null
            && unreachedFinalStatus.Outcome
                == PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
            && missingFrame4.Outcome
                == PayloadGrowthZeroFillOutcome.MissingFrameDwordMinus4Capture
            && missingFrame4.FrameDwordMinus4ReadAddress == 0x1FFC
            && !missingFrame4.FrameDwordMinus4Read);

        PayloadGrowthZeroFillPlan missingTakenFrame4 = Project(
            entry: PayloadGrowthZeroFillEntry.Taken,
            frameMinus4: null,
            retainedEsi: 1,
            directionFlag: null,
            finalRepCompleted: null);
        PayloadGrowthZeroFillPlan missingTakenEsi = Project(
            entry: PayloadGrowthZeroFillEntry.Taken,
            retainedEsi: null,
            directionFlag: null,
            finalRepCompleted: null);
        PayloadGrowthZeroFillPlan missingTakenDirection = Project(
            entry: PayloadGrowthZeroFillEntry.Taken,
            retainedEsi: 1,
            directionFlag: null,
            finalRepCompleted: true);
        PayloadGrowthZeroFillPlan failedTakenRep = Project(
            entry: PayloadGrowthZeroFillEntry.Taken,
            retainedEsi: 1,
            directionFlag: false,
            finalRepCompleted: false);
        check("Taken frame-4 and retained-ESI gates precede the shared REP, whose fault prevents continuation claims",
            missingTakenFrame4.Outcome
                == PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
            && !missingTakenFrame4.FrameDwordMinus4Read
            && missingTakenEsi.Outcome
                == PayloadGrowthZeroFillOutcome.MissingRetainedEsiCapture
            && missingTakenEsi.FrameDwordMinus4Read
            && missingTakenDirection.Outcome
                == PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
            && failedTakenRep.Outcome
                == PayloadGrowthZeroFillOutcome.FinalRepDidNotCompleteNormally
            && failedTakenRep.FinalRep is { NormalCompletion: false }
            && failedTakenRep.FinalRep.Value.EcxAfter is null
            && failedTakenRep.FinalRep.Value.FinalEdi is null
            && failedTakenRep.NormalBoundaryRawFileOffsetExclusive is null);

        PayloadGrowthZeroFillOperation bounded = new(
            0x00BE69F0,
            1,
            0x1000,
            false,
            true);
        bool upperBoundRejected = false;
        try
        {
            _ = bounded.AddressAt(1);
        }
        catch (ArgumentOutOfRangeException)
        {
            upperBoundRejected = true;
        }

        PayloadGrowthZeroFillOperation unknownDirection = bounded with
        {
            DirectionFlag = null
        };
        bool missingDirectionRejected = false;
        try
        {
            _ = unknownDirection.AddressAt(0);
        }
        catch (InvalidOperationException)
        {
            missingDirectionRejected = true;
        }

        check("AddressAt is bounded and does not invent an address when DF is unknown",
            bounded.AddressAt(0) == 0x1000
            && upperBoundRejected
            && missingDirectionRejected
            && unknownDirection.FinalEdi is null);
    }

    private static PayloadGrowthZeroFillPlan Project(
        PayloadGrowthZeroFillEntry entry = PayloadGrowthZeroFillEntry.Fall,
        bool? priorCallReturnedNormally = true,
        uint framePointer = 0x2000,
        uint? retainedEbx = 0x100,
        uint? callEax = 0x20,
        uint? incomingEdi = null,
        uint? retainedEsi = null,
        uint? frameMinusC = 1,
        uint? frameMinus8 = null,
        uint? frameMinus4 = 3,
        bool? directionFlag = false,
        bool? firstRepCompleted = null,
        bool? finalRepCompleted = true) =>
        PayloadGrowthZeroFillProjection.Project(new(
            entry,
            priorCallReturnedNormally,
            framePointer,
            retainedEbx,
            callEax,
            incomingEdi,
            retainedEsi,
            entry == PayloadGrowthZeroFillEntry.Taken ? null : frameMinusC,
            entry == PayloadGrowthZeroFillEntry.Taken ? null : frameMinus8,
            frameMinus4,
            directionFlag,
            entry == PayloadGrowthZeroFillEntry.Taken ? null : firstRepCompleted,
            finalRepCompleted));

    private static bool VerifyInstructionEvidence()
    {
        foreach (PayloadGrowthZeroFillInstructionPath path
            in Enum.GetValues<PayloadGrowthZeroFillInstructionPath>())
        {
            PayloadGrowthZeroFillInstructionEvidence[] instructions =
                PayloadGrowthZeroFillProjection.Instructions
                    .Where(instruction => instruction.Path == path)
                    .ToArray();
            uint nextRawOffset = path == PayloadGrowthZeroFillInstructionPath.Fall
                ? PayloadGrowthZeroFillProjection.FallStartRawFileOffset
                : PayloadGrowthZeroFillProjection.TakenStartRawFileOffset;
            string instructionBytes = string.Empty;
            foreach (PayloadGrowthZeroFillInstructionEvidence instruction in instructions)
            {
                string bytes = string.Concat(
                    instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                if (instruction.RawFileOffset != nextRawOffset || bytes.Length % 2 != 0)
                {
                    return false;
                }

                instructionBytes += bytes;
                nextRawOffset += (uint)(bytes.Length / 2);
            }

            uint expectedEnd = path == PayloadGrowthZeroFillInstructionPath.Fall
                ? PayloadGrowthZeroFillProjection.FallEndRawFileOffsetExclusive
                : PayloadGrowthZeroFillProjection.TakenEndRawFileOffsetExclusive;
            string expectedBytes = string.Concat(
                (path == PayloadGrowthZeroFillInstructionPath.Fall
                    ? PayloadGrowthZeroFillProjection.FallCapturedCodeBytes
                    : PayloadGrowthZeroFillProjection.TakenCapturedCodeBytes)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (nextRawOffset != expectedEnd || instructionBytes != expectedBytes)
            {
                return false;
            }
        }

        return PayloadGrowthZeroFillProjection.SharedStartRawFileOffset
                == PayloadGrowthZeroFillProjection.TakenStartRawFileOffset
                    + 0x0F
            && PayloadGrowthZeroFillProjection.SharedEndRawFileOffsetExclusive
                == PayloadGrowthZeroFillProjection.TakenEndRawFileOffsetExclusive
            && string.Concat(PayloadGrowthZeroFillProjection.Instructions
                .Where(instruction =>
                    instruction.RawFileOffset
                        == PayloadGrowthZeroFillProjection.SharedStartRawFileOffset)
                .Select(instruction => instruction.Bytes.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries))
                .SelectMany(bytes => bytes))
                == string.Concat(PayloadGrowthZeroFillProjection.SharedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
