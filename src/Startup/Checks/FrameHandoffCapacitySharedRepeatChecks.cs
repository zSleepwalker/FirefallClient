using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacitySharedRepeatChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE identity and exact four-byte shared REP window agree",
            FrameHandoffCapacitySharedRepeatProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacitySharedRepeatProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacitySharedRepeatProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacitySharedRepeatProjection.EntryRawFileOffset == 0x00BE6C9E
            && FrameHandoffCapacitySharedRepeatProjection.RepStosdRawFileOffset == 0x00BE6CA0
            && FrameHandoffCapacitySharedRepeatProjection.EndRawFileOffsetExclusive
                == 0x00BE6CA2
            && FrameHandoffCapacitySharedRepeatProjection.EntryVirtualAddress == 0x00FE789E
            && FrameHandoffCapacitySharedRepeatProjection.EndVirtualAddressExclusive
                == 0x00FE78A2
            && FrameHandoffCapacitySharedRepeatProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacitySharedRepeatProjection.EntryRawFileOffset)
                == FrameHandoffCapacitySharedRepeatProjection.EntryVirtualAddress
            && FrameHandoffCapacitySharedRepeatProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacitySharedRepeatProjection.EndRawFileOffsetExclusive)
                == FrameHandoffCapacitySharedRepeatProjection.EndVirtualAddressExclusive
            && FrameHandoffCapacitySharedRepeatProjection.CapturedWindowByteCount == 4
            && Normalize(FrameHandoffCapacitySharedRepeatProjection.CapturedWindowBytes)
                == "33C0F3AB"
            && InstructionEvidenceMatches());

        FrameHandoffCapacitySharedRepeatPlan forward =
            FrameHandoffCapacitySharedRepeatProjection.Project(new(
                EntryValidatedNonzero: true,
                EntryEcx: 3,
                EntryEdi: 0x1000,
                DirectionFlagSet: false,
                RepCompletedNormally: true));
        FrameHandoffCapacitySharedRepeatPlan backward =
            FrameHandoffCapacitySharedRepeatProjection.Project(new(
                EntryValidatedNonzero: true,
                EntryEcx: 3,
                EntryEdi: 0x1000,
                DirectionFlagSet: true,
                RepCompletedNormally: true));
        check("The captured DF selects compressed four-byte stride and wrapped final EDI",
            IsComplete(forward)
            && forward.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(3, 0x1000, 4, 0)
            && forward.EdiAfterRep == 0x100C
            && backward.Outcome == FrameHandoffCapacitySharedRepeatOutcome.Completed
            && backward.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(3, 0x1000, -4, 0)
            && backward.EdiAfterRep == 0x0FF4);

        FrameHandoffCapacitySharedRepeatPlan maximumCount =
            FrameHandoffCapacitySharedRepeatProjection.Project(new(
                true, uint.MaxValue, 2, false, true));
        FrameHandoffCapacitySharedRepeatPlan forwardWrap =
            FrameHandoffCapacitySharedRepeatProjection.Project(new(
                true, 1, 0xFFFFFFFE, false, true));
        FrameHandoffCapacitySharedRepeatPlan backwardWrap =
            FrameHandoffCapacitySharedRepeatProjection.Project(new(
                true, 1, 2, true, true));
        check("Maximum ECX stays compressed and completed REP arithmetic wraps as a DWORD",
            IsComplete(maximumCount)
            && maximumCount.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(
                    uint.MaxValue, 2, 4, 0)
            && maximumCount.EcxAfterRep == 0
            && maximumCount.EdiAfterRep == 0xFFFFFFFE
            && forwardWrap.EdiAfterRep == 2
            && backwardWrap.EdiAfterRep == uint.MaxValue - 1);

        FrameHandoffCapacitySharedRepeatPlan missingDirection =
            FrameHandoffCapacitySharedRepeatProjection.Project(new(
                true, 5, 0x4000, null, true));
        check("Missing DF stops at REP before any store claim but retains XOR EAX zero",
            missingDirection.Outcome
                == FrameHandoffCapacitySharedRepeatOutcome.MissingDirectionFlagCapture
            && missingDirection.EntryValidationAccepted
            && missingDirection.XorEaxReached
            && missingDirection.EaxValue == 0
            && missingDirection.RepInstructionReached
            && missingDirection.DirectionFlagSet is null
            && !missingDirection.AllStoresCompleted
            && missingDirection.StoreDescriptor is null
            && missingDirection.EcxAfterRep is null
            && missingDirection.EdiAfterRep is null
            && missingDirection.UnreachedRepCompletionCaptureProvided
            && missingDirection.StopAtRawFileOffsetExclusive == 0x00BE6CA0);

        FrameHandoffCapacitySharedRepeatPlan missingCompletion =
            FrameHandoffCapacitySharedRepeatProjection.Project(new(
                true, 7, 0x5000, false, null));
        FrameHandoffCapacitySharedRepeatPlan incompleteRep =
            FrameHandoffCapacitySharedRepeatProjection.Project(new(
                true, 9, 0x6000, true, false));
        check("Missing or non-normal REP completion retains EAX zero without claiming any whole-store result",
            missingCompletion.Outcome
                == FrameHandoffCapacitySharedRepeatOutcome.MissingRepCompletionCapture
            && missingCompletion.EaxValue == 0
            && missingCompletion.DirectionFlagSet == false
            && !missingCompletion.AllStoresCompleted
            && missingCompletion.StoreDescriptor is null
            && missingCompletion.EcxAfterRep is null
            && missingCompletion.EdiAfterRep is null
            && missingCompletion.StopAtRawFileOffsetExclusive == 0x00BE6CA0
            && incompleteRep.Outcome
                == FrameHandoffCapacitySharedRepeatOutcome.RepDidNotCompleteNormally
            && incompleteRep.EaxValue == 0
            && incompleteRep.DirectionFlagSet == true
            && !incompleteRep.AllStoresCompleted
            && incompleteRep.StoreDescriptor is null
            && incompleteRep.EcxAfterRep is null
            && incompleteRep.EdiAfterRep is null
            && incompleteRep.StopAtRawFileOffsetExclusive == 0x00BE6CA0);

        FrameHandoffCapacitySharedRepeatPlan entryBlocked =
            FrameHandoffCapacitySharedRepeatProjection.Project(new(
                false, 5, 0x7000, true, true));
        FrameHandoffCapacitySharedRepeatPlan zeroCount =
            FrameHandoffCapacitySharedRepeatProjection.Project(new(
                true, 0, 0x8000, false, true));
        check("Blocked and zero-count entries reject downstream captures even when supplied",
            entryBlocked.Outcome
                == FrameHandoffCapacitySharedRepeatOutcome.EntryNotValidatedNonzero
            && !entryBlocked.EntryValidationAccepted
            && !entryBlocked.XorEaxReached
            && !entryBlocked.RepInstructionReached
            && entryBlocked.StoreDescriptor is null
            && entryBlocked.UnreachedDirectionFlagCaptureProvided
            && entryBlocked.UnreachedRepCompletionCaptureProvided
            && entryBlocked.StopAtRawFileOffsetExclusive == 0x00BE6C9E
            && zeroCount.Outcome
                == FrameHandoffCapacitySharedRepeatOutcome.EntryNotValidatedNonzero
            && !zeroCount.EntryValidationAccepted
            && !zeroCount.XorEaxReached
            && !zeroCount.RepInstructionReached
            && zeroCount.StoreDescriptor is null
            && zeroCount.UnreachedDirectionFlagCaptureProvided
            && zeroCount.UnreachedRepCompletionCaptureProvided
            && zeroCount.StopAtRawFileOffsetExclusive == 0x00BE6C9E);
    }

    private static bool IsComplete(FrameHandoffCapacitySharedRepeatPlan plan) =>
        plan.Outcome == FrameHandoffCapacitySharedRepeatOutcome.Completed
        && plan.EntryValidationAccepted
        && plan.XorEaxReached
        && plan.EaxValue == 0
        && plan.RepInstructionReached
        && plan.AllStoresCompleted
        && plan.StoreDescriptor.HasValue
        && plan.EcxAfterRep == 0
        && !plan.UnreachedDirectionFlagCaptureProvided
        && !plan.UnreachedRepCompletionCaptureProvided
        && plan.StopAtRawFileOffsetExclusive
            == FrameHandoffCapacitySharedRepeatProjection.EndRawFileOffsetExclusive;

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x00BE6C9E, "33 C0"),
            (0x00BE6CA0, "F3 AB")
        };
        uint nextRawOffset = FrameHandoffCapacitySharedRepeatProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (FrameHandoffCapacitySharedRepeatInstructionEvidence instruction
            in FrameHandoffCapacitySharedRepeatProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacitySharedRepeatProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == FrameHandoffCapacitySharedRepeatProjection.EndRawFileOffsetExclusive
            && bytes == Normalize(
                FrameHandoffCapacitySharedRepeatProjection.CapturedWindowBytes)
            && FrameHandoffCapacitySharedRepeatProjection.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
