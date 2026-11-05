using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityOverflowTailChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The overflow JB targets the bounded tail and the raw window ends after CALL",
            FrameHandoffCapacityOverflowTailProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityOverflowTailProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityOverflowTailProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityOverflowTailProjection.OverflowBranchRawFileOffset
                == 0x00BE6B87
            && FrameHandoffCapacityOverflowTailProjection.OverflowBranchTargetRawFileOffset
                == FrameHandoffCapacityOverflowTailProjection.OverflowEntryRawFileOffset
            && FrameHandoffCapacityOverflowTailProjection.OverflowBranchBytes
                == "0F 82 33 01 00 00"
            && FrameHandoffCapacityOverflowTailProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityOverflowTailProjection.OverflowEntryRawFileOffset)
                == FrameHandoffCapacityOverflowTailProjection.OverflowEntryVirtualAddress
            && FrameHandoffCapacityOverflowTailProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityOverflowTailProjection.NormalBoundaryRawFileOffsetExclusive)
                == FrameHandoffCapacityOverflowTailProjection.NormalBoundaryVirtualAddressExclusive
            && Normalize(FrameHandoffCapacityOverflowTailProjection.CapturedWindowBytes)
                == "68A407C601FF15C045AC01"
            && FrameHandoffCapacityOverflowTailProjection.Instructions.Any(instruction =>
                instruction.RawFileOffset
                    == FrameHandoffCapacityOverflowTailProjection.OverflowBranchRawFileOffset
                && instruction.Bytes == "0F 82 33 01 00 00")
            && FrameHandoffCapacityOverflowTailProjection.Instructions.Any(instruction =>
                instruction.RawFileOffset
                    == FrameHandoffCapacityOverflowTailProjection.OverflowBranchTargetRawFileOffset
                && instruction.Bytes == "68 A4 07 C6 01"));

        FrameHandoffCapacityOverflowTailPlan unvalidated =
            FrameHandoffCapacityOverflowTailProjection.Project(new(
                ValidatedOverflowEntry: false,
                CallTargetAddress: 0x12345678,
                CallReturnedNormally: true));
        check("An unvalidated overflow entry leaves the tail and supplied captures unreached",
            unvalidated.Outcome
                == FrameHandoffCapacityOverflowTailOutcome.EntryNotValidated
            && unvalidated.Pushes.IsEmpty
            && !unvalidated.PushReached
            && !unvalidated.CallReached
            && unvalidated.CallReturnedNormally is null
            && unvalidated.UnreachedCallTargetCaptureProvided
            && unvalidated.UnreachedCallReturnCaptureProvided
            && unvalidated.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityOverflowTailProjection.OverflowEntryRawFileOffset);

        FrameHandoffCapacityOverflowTailPlan missingTarget =
            FrameHandoffCapacityOverflowTailProjection.Project(new(
                ValidatedOverflowEntry: true,
                CallTargetAddress: null,
                CallReturnedNormally: true));
        check("A missing call target retains the immediate push and marks call status unreached",
            missingTarget.Outcome
                == FrameHandoffCapacityOverflowTailOutcome.MissingCallTargetCapture
            && missingTarget.Pushes.SequenceEqual(new[]
            {
                new FrameHandoffCapacityOverflowTailPush(
                    0x00BE6CC0,
                    "Immediate VA",
                    0x01C607A4)
            })
            && missingTarget.PushReached
            && !missingTarget.CallReached
            && missingTarget.CallReturnedNormally is null
            && missingTarget.UnreachedCallReturnCaptureProvided
            && missingTarget.StopAtRawFileOffsetExclusive == 0x00BE6CC5);

        FrameHandoffCapacityOverflowTailPlan zeroTarget =
            FrameHandoffCapacityOverflowTailProjection.Project(new(
                ValidatedOverflowEntry: true,
                CallTargetAddress: 0,
                CallReturnedNormally: true));
        check("A zero call target is a captured target, and push order precedes the call",
            zeroTarget.Outcome
                == FrameHandoffCapacityOverflowTailOutcome.ReturnedNormally
            && zeroTarget.Capture.CallTargetAddress == 0
            && zeroTarget.Pushes.SequenceEqual(new[]
            {
                new FrameHandoffCapacityOverflowTailPush(
                    FrameHandoffCapacityOverflowTailProjection.OverflowEntryRawFileOffset,
                    "Immediate VA",
                    FrameHandoffCapacityOverflowTailProjection.PushedValueVirtualAddress)
            })
            && zeroTarget.Pushes[0].RawFileOffset
                < FrameHandoffCapacityOverflowTailProjection.CallRawFileOffset
            && zeroTarget.Pushes[0].Value == 0x01C607A4
            && zeroTarget.CallReached
            && zeroTarget.CallReturnedNormally == true
            && zeroTarget.NormalBoundaryReached
            && zeroTarget.StopAtRawFileOffsetExclusive == 0x00BE6CCB);

        FrameHandoffCapacityOverflowTailPlan missingStatus = Project(callReturnedNormally: null);
        FrameHandoffCapacityOverflowTailPlan nonreturning = Project(callReturnedNormally: false);
        check("Missing or nonreturning calls stop at CALL, while normal return ends at raw 0xBE6CCB",
            missingStatus.Outcome
                == FrameHandoffCapacityOverflowTailOutcome.MissingCallReturnCapture
            && missingStatus.CallReached
            && missingStatus.CallReturnedNormally is null
            && missingStatus.Pushes.Length == 1
            && missingStatus.StopAtRawFileOffsetExclusive == 0x00BE6CC5
            && nonreturning.Outcome
                == FrameHandoffCapacityOverflowTailOutcome.CallDidNotReturnNormally
            && nonreturning.CallReached
            && nonreturning.CallReturnedNormally == false
            && !nonreturning.NormalBoundaryReached
            && nonreturning.StopAtRawFileOffsetExclusive == 0x00BE6CC5
            && missingStatus.NormalBoundaryReached == false);
    }

    private static FrameHandoffCapacityOverflowTailPlan Project(bool? callReturnedNormally) =>
        FrameHandoffCapacityOverflowTailProjection.Project(new(
            ValidatedOverflowEntry: true,
            CallTargetAddress: 0x87654321,
            CallReturnedNormally: callReturnedNormally));

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
