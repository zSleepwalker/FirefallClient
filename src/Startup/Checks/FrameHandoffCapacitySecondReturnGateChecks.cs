using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacitySecondReturnGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE identity and exact raw gate bytes cover BE6C34 through BE6C40",
            FrameHandoffCapacitySecondReturnGateProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacitySecondReturnGateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacitySecondReturnGateProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacitySecondReturnGateProjection.EntryRawFileOffset == 0x00BE6C34
            && FrameHandoffCapacitySecondReturnGateProjection.EntryVirtualAddress == 0x00FE7834
            && FrameHandoffCapacitySecondReturnGateProjection.NormalBoundaryRawFileOffsetExclusive
                == 0x00BE6C41
            && FrameHandoffCapacitySecondReturnGateProjection.NormalBoundaryVirtualAddressExclusive
                == 0x00FE7841
            && FrameHandoffCapacitySecondReturnGateProjection.ZeroBranchDestinationRawFileOffset
                == 0x00BE6C4A
            && FrameHandoffCapacitySecondReturnGateProjection.ZeroBranchDestinationVirtualAddress
                == 0x00FE784A
            && FrameHandoffCapacitySecondReturnGateProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacitySecondReturnGateProjection.EntryRawFileOffset)
                == FrameHandoffCapacitySecondReturnGateProjection.EntryVirtualAddress
            && FrameHandoffCapacitySecondReturnGateProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacitySecondReturnGateProjection.NormalBoundaryRawFileOffsetExclusive)
                == FrameHandoffCapacitySecondReturnGateProjection.NormalBoundaryVirtualAddressExclusive
            && Normalize(FrameHandoffCapacitySecondReturnGateProjection.CapturedWindowBytes)
                == "8B4DF483C40C8D140385C97409"
            && FrameHandoffCapacitySecondReturnGateProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 13
            && InstructionEvidenceMatches());

        FrameHandoffCapacitySecondReturnGatePlan invalidEntry =
            FrameHandoffCapacitySecondReturnGateProjection.Project(new(
                ValidatedNormalEntry: false,
                EntryEbp: 0x1000,
                EntryEbx: 0x2000,
                FrameMinusC: 0,
                HelperReturnEax: 1));
        check("An unvalidated entry does not read frame-minus-C or reach cleanup",
            invalidEntry.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.EntryNotValidated
            && invalidEntry.Reads.IsEmpty
            && !invalidEntry.FrameMinusCReadReached
            && !invalidEntry.StackCleanupReached
            && !invalidEntry.EdxLeaReached
            && !invalidEntry.TestReached
            && invalidEntry.UnreachedFrameMinusCCaptureProvided
            && invalidEntry.UnreachedHelperReturnEaxCaptureProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x00BE6C34);

        FrameHandoffCapacitySecondReturnGatePlan missingFrameMinusC =
            FrameHandoffCapacitySecondReturnGateProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbp: 0x10,
                EntryEbx: 0x20,
                FrameMinusC: null,
                HelperReturnEax: 0));
        check("Missing frame-minus-C stops at BE6C34 before cleanup and marks EAX unreached",
            missingFrameMinusC.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.MissingFrameMinusCCapture
            && missingFrameMinusC.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacitySecondReturnGateRead(0x00BE6C34, 4, null)
            })
            && missingFrameMinusC.FrameMinusCReadReached
            && !missingFrameMinusC.FrameMinusCCaptured
            && !missingFrameMinusC.StackCleanupReached
            && !missingFrameMinusC.EdxLeaReached
            && !missingFrameMinusC.HelperReturnEaxConsumed
            && !missingFrameMinusC.TestReached
            && !missingFrameMinusC.UnreachedFrameMinusCCaptureProvided
            && missingFrameMinusC.UnreachedHelperReturnEaxCaptureProvided
            && missingFrameMinusC.StopAtRawFileOffsetExclusive == 0x00BE6C34);

        FrameHandoffCapacitySecondReturnGatePlan missingHelperEax =
            FrameHandoffCapacitySecondReturnGateProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbp: 0x10,
                EntryEbx: 0x20,
                FrameMinusC: 0,
                HelperReturnEax: null));
        check("Missing helper EAX stops at the LEA after preserving the read and 12-byte cleanup",
            missingHelperEax.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.MissingHelperReturnEaxCapture
            && missingHelperEax.Reads.Single()
                == new FrameHandoffCapacitySecondReturnGateRead(0x00BE6C34, 4, 0)
            && missingHelperEax.FrameMinusCReadReached
            && missingHelperEax.FrameMinusCCaptured
            && missingHelperEax.StackCleanupReached
            && !missingHelperEax.EdxLeaReached
            && !missingHelperEax.HelperReturnEaxConsumed
            && !missingHelperEax.TestReached
            && !missingHelperEax.BranchReached
            && missingHelperEax.StopAtRawFileOffsetExclusive == 0x00BE6C3A);

        FrameHandoffCapacitySecondReturnGatePlan zeroBranch =
            FrameHandoffCapacitySecondReturnGateProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbp: 0x10,
                EntryEbx: 0x20,
                FrameMinusC: 0,
                HelperReturnEax: 0));
        FrameHandoffCapacitySecondReturnGatePlan nonzeroFallThrough =
            FrameHandoffCapacitySecondReturnGateProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbp: 0x10,
                EntryEbx: 0x20,
                FrameMinusC: uint.MaxValue,
                HelperReturnEax: 0));
        check("TEST ECX selects exactly JZ BE6C4A for zero or fallthrough BE6C41 otherwise",
            zeroBranch.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCZeroBranchTaken
            && zeroBranch.FrameMinusCIsZero == true
            && zeroBranch.BranchReached
            && zeroBranch.BranchTaken == true
            && zeroBranch.SelectedDestinationRawFileOffset == 0x00BE6C4A
            && zeroBranch.StopAtRawFileOffsetExclusive == 0x00BE6C4A
            && zeroBranch.EdxAfterLea == 0x20
            && zeroBranch.HelperReturnEaxConsumed
            && nonzeroFallThrough.Outcome
                == FrameHandoffCapacitySecondReturnGateOutcome.FrameMinusCNonzeroFallThrough
            && nonzeroFallThrough.FrameMinusCIsZero == false
            && nonzeroFallThrough.BranchReached
            && nonzeroFallThrough.BranchTaken == false
            && nonzeroFallThrough.SelectedDestinationRawFileOffset == 0x00BE6C41
            && nonzeroFallThrough.StopAtRawFileOffsetExclusive == 0x00BE6C41
            && nonzeroFallThrough.EdxAfterLea == 0x20);

        FrameHandoffCapacitySecondReturnGatePlan wrappedEntryEbx =
            FrameHandoffCapacitySecondReturnGateProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbp: 0,
                EntryEbx: 0xFFFFFFFE,
                FrameMinusC: 1,
                HelperReturnEax: 3));
        check("The gate-entry EBX capture is independent and EDX addition wraps as a DWORD",
            FrameHandoffCapacitySecondReturnGateProjection.FrameMinusCAddress(0) == 0xFFFFFFF4
            && FrameHandoffCapacitySecondReturnGateProjection
                .AddEntryEbxAndHelperReturnEax(0xFFFFFFFE, 3) == 1
            && wrappedEntryEbx.Capture.EntryEbx == 0xFFFFFFFE
            && wrappedEntryEbx.EdxAfterLea == 1
            && wrappedEntryEbx.FrameMinusCIsZero == false
            && wrappedEntryEbx.BranchTaken == false
            && wrappedEntryEbx.Reads.Single().Address == 0xFFFFFFF4
            && wrappedEntryEbx.StopAtRawFileOffsetExclusive == 0x00BE6C41);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x00BE6C34, "8B 4D F4", null, null),
            (0x00BE6C37, "83 C4 0C", null, null),
            (0x00BE6C3A, "8D 14 03", null, null),
            (0x00BE6C3D, "85 C9", null, null),
            (0x00BE6C3F, "74 09", 0x00BE6C4A, 0x00FE784A)
        };
        uint nextRawOffset = FrameHandoffCapacitySecondReturnGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (FrameHandoffCapacitySecondReturnGateInstructionEvidence instruction
            in FrameHandoffCapacitySecondReturnGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacitySecondReturnGateProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && (instruction.BranchTargetVirtualAddress
                        != FrameHandoffCapacitySecondReturnGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw))))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == FrameHandoffCapacitySecondReturnGateProjection.NormalBoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                FrameHandoffCapacitySecondReturnGateProjection.CapturedWindowBytes)
            && FrameHandoffCapacitySecondReturnGateProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
