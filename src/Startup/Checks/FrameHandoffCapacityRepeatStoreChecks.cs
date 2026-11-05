using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityRepeatStoreChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample identity, raw range, VA mapping, and exact instruction bytes agree",
            FrameHandoffCapacityRepeatStoreProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityRepeatStoreProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityRepeatStoreProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityRepeatStoreProjection.EntryRawFileOffset == 0x00BE6C41
            && FrameHandoffCapacityRepeatStoreProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityRepeatStoreProjection.EntryRawFileOffset) == 0x00FE7841
            && FrameHandoffCapacityRepeatStoreProjection.EndRawFileOffsetExclusive
                == 0x00BE6C4A
            && FrameHandoffCapacityRepeatStoreProjection.CapturedWindowByteCount == 9
            && FrameHandoffCapacityRepeatStoreProjection.CapturedWindowBytes
                == "33C08BFAF3AB8B7DF8"
            && InstructionEvidenceMatches());

        FrameHandoffCapacityRepeatStorePlan forward =
            FrameHandoffCapacityRepeatStoreProjection.Project(new(
                EntryEcxValidatedNonzero: true,
                EntryEdx: 0x1000,
                EntryEcx: 3,
                EntryEbp: 0x2000,
                DirectionFlagSet: false,
                RepReturnedNormally: true,
                FreshFrameMinus8Value: 0xAABBCCDD));
        FrameHandoffCapacityRepeatStorePlan backward =
            FrameHandoffCapacityRepeatStoreProjection.Project(new(
                EntryEcxValidatedNonzero: true,
                EntryEdx: 0x1000,
                EntryEcx: 3,
                EntryEbp: 0x2000,
                DirectionFlagSet: true,
                RepReturnedNormally: true,
                FreshFrameMinus8Value: 0x12345678));
        check("DF selects the signed four-byte stride and wrapped final EDI independently of the fresh reload",
            forward.Outcome == FrameHandoffCapacityRepeatStoreOutcome.Completed
            && forward.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(3, 0x1000, 4, 0)
            && forward.AllStoresCompleted
            && forward.EcxAfterRep == 0
            && forward.EdiAfterRep == 0x100C
            && forward.FreshFrameMinus8ReadAddress == 0x1FF8
            && forward.FreshFrameMinus8Value == 0xAABBCCDD
            && backward.Outcome == FrameHandoffCapacityRepeatStoreOutcome.Completed
            && backward.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(3, 0x1000, -4, 0)
            && backward.EdiAfterRep == 0x0FF4
            && backward.FreshFrameMinus8ReadAddress == 0x1FF8
            && backward.FreshFrameMinus8Value == 0x12345678);

        FrameHandoffCapacityRepeatStorePlan forwardWrap =
            FrameHandoffCapacityRepeatStoreProjection.Project(new(
                true, 0xFFFFFFFE, 1, 0, false, true, 0));
        FrameHandoffCapacityRepeatStorePlan backwardWrap =
            FrameHandoffCapacityRepeatStoreProjection.Project(new(
                true, 2, 1, 0, true, true, 0));
        FrameHandoffCapacityRepeatStorePlan maximumCount =
            FrameHandoffCapacityRepeatStoreProjection.Project(new(
                true, 2, uint.MaxValue, 0, false, true, 0));
        check("DWORD EDI arithmetic wraps and the maximum ECX count remains a single compressed descriptor",
            forwardWrap.EdiAfterRep == 2
            && forwardWrap.StoreDescriptor?.Count == 1
            && backwardWrap.EdiAfterRep == uint.MaxValue - 1
            && maximumCount.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(
                    uint.MaxValue, 2, 4, 0)
            && maximumCount.EcxAfterRep == 0
            && maximumCount.EdiAfterRep == 0xFFFFFFFE
            && maximumCount.AllStoresCompleted);

        FrameHandoffCapacityRepeatStorePlan missingDirection =
            FrameHandoffCapacityRepeatStoreProjection.Project(new(
                true, 0x4000, 5, 0x8000, null, true, 0xDEADBEEF));
        check("An unknown DF stops before REP effects while preserving the preceding EAX and EDI setup",
            missingDirection.Outcome
                == FrameHandoffCapacityRepeatStoreOutcome.MissingDirectionFlagCapture
            && missingDirection.EntryValidationAccepted
            && missingDirection.XorEaxReached
            && missingDirection.EaxValue == 0
            && missingDirection.MoveEdiFromEdxReached
            && missingDirection.EdiBeforeRep == 0x4000
            && missingDirection.RepInstructionReached
            && !missingDirection.AllStoresCompleted
            && missingDirection.StoreDescriptor is null
            && missingDirection.EcxAfterRep is null
            && missingDirection.EdiAfterRep is null
            && !missingDirection.FreshFrameMinus8ReadReached
            && missingDirection.UnreachedRepCompletionCaptureProvided
            && missingDirection.UnreachedFreshFrameMinus8CaptureProvided
            && missingDirection.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityRepeatStoreProjection.RepStosdRawFileOffset);

        FrameHandoffCapacityRepeatStorePlan missingCompletion =
            FrameHandoffCapacityRepeatStoreProjection.Project(new(
                true, 0x5000, 7, 0x9000, false, null, 0xAABBCCDD));
        FrameHandoffCapacityRepeatStorePlan partialRep =
            FrameHandoffCapacityRepeatStoreProjection.Project(new(
                true, 0x6000, 9, 0xA000, true, false, 0xCAFEBABE));
        check("Missing or non-normal REP completion cannot claim the full store or final ECX/EDI state",
            missingCompletion.Outcome
                == FrameHandoffCapacityRepeatStoreOutcome.MissingRepCompletionCapture
            && missingCompletion.EaxValue == 0
            && missingCompletion.EdiBeforeRep == 0x5000
            && missingCompletion.DirectionFlagSet == false
            && !missingCompletion.AllStoresCompleted
            && missingCompletion.StoreDescriptor is null
            && missingCompletion.EcxAfterRep is null
            && missingCompletion.EdiAfterRep is null
            && !missingCompletion.FreshFrameMinus8ReadReached
            && missingCompletion.UnreachedFreshFrameMinus8CaptureProvided
            && partialRep.Outcome
                == FrameHandoffCapacityRepeatStoreOutcome.RepDidNotReturnNormally
            && partialRep.EaxValue == 0
            && partialRep.EdiBeforeRep == 0x6000
            && partialRep.DirectionFlagSet == true
            && !partialRep.AllStoresCompleted
            && partialRep.StoreDescriptor is null
            && partialRep.EcxAfterRep is null
            && partialRep.EdiAfterRep is null
            && !partialRep.FreshFrameMinus8ReadReached
            && partialRep.UnreachedFreshFrameMinus8CaptureProvided);

        FrameHandoffCapacityRepeatStorePlan zeroEcx =
            FrameHandoffCapacityRepeatStoreProjection.Project(new(
                true, 0x7000, 0, 0xB000, false, true, 0x11111111));
        FrameHandoffCapacityRepeatStorePlan unvalidatedEntry =
            FrameHandoffCapacityRepeatStoreProjection.Project(new(
                false, 0x7000, 1, 0xB000, false, true, 0x22222222));
        check("Zero or unvalidated ECX is a wrong entry because the upstream zero branch bypasses this window",
            zeroEcx.Outcome == FrameHandoffCapacityRepeatStoreOutcome.WrongEntry
            && !zeroEcx.EntryValidationAccepted
            && !zeroEcx.XorEaxReached
            && !zeroEcx.MoveEdiFromEdxReached
            && !zeroEcx.RepInstructionReached
            && zeroEcx.StoreDescriptor is null
            && zeroEcx.UnreachedDirectionFlagCaptureProvided
            && zeroEcx.UnreachedRepCompletionCaptureProvided
            && zeroEcx.UnreachedFreshFrameMinus8CaptureProvided
            && zeroEcx.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityRepeatStoreProjection.EntryRawFileOffset
            && unvalidatedEntry.Outcome == FrameHandoffCapacityRepeatStoreOutcome.WrongEntry
            && !unvalidatedEntry.EntryValidationAccepted
            && !unvalidatedEntry.XorEaxReached);

        FrameHandoffCapacityRepeatStorePlan missingFreshRead =
            FrameHandoffCapacityRepeatStoreProjection.Project(new(
                true, 0xFFFFFFFC, 2, 4, false, true, null));
        check("A missing fresh [EBP-8] read preserves the completed compressed stores and final REP state",
            missingFreshRead.Outcome
                == FrameHandoffCapacityRepeatStoreOutcome.MissingFreshFrameMinus8Capture
            && missingFreshRead.AllStoresCompleted
            && missingFreshRead.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(2, 0xFFFFFFFC, 4, 0)
            && missingFreshRead.EcxAfterRep == 0
            && missingFreshRead.EdiAfterRep == 4
            && missingFreshRead.FreshFrameMinus8ReadReached
            && missingFreshRead.FreshFrameMinus8ReadAddress == 0xFFFFFFFC
            && missingFreshRead.FreshFrameMinus8Value is null
            && missingFreshRead.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityRepeatStoreProjection.FreshFrameMinus8ReloadRawFileOffset);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            uint Virtual,
            string Bytes)[]
        {
            (0x00BE6C41, 0x00FE7841, "33 C0"),
            (0x00BE6C43, 0x00FE7843, "8B FA"),
            (0x00BE6C45, 0x00FE7845, "F3 AB"),
            (0x00BE6C47, 0x00FE7847, "8B 7D F8")
        };
        uint nextRawOffset = FrameHandoffCapacityRepeatStoreProjection.EntryRawFileOffset;
        string instructionBytes = string.Empty;

        foreach (FrameHandoffCapacityRepeatStoreInstructionEvidence instruction
            in FrameHandoffCapacityRepeatStoreProjection.Instructions)
        {
            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacityRepeatStoreProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(bytes.Length / 2);
            instructionBytes += bytes;
        }

        return nextRawOffset == FrameHandoffCapacityRepeatStoreProjection.EndRawFileOffsetExclusive
            && instructionBytes == FrameHandoffCapacityRepeatStoreProjection.CapturedWindowBytes
            && FrameHandoffCapacityRepeatStoreProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
                .SequenceEqual(expected);
    }
}
