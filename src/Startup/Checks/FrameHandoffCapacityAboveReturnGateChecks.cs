using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityAboveReturnGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE identity and exact 15 raw bytes cover BE6C8F through BE6C9D",
            FrameHandoffCapacityAboveReturnGateProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityAboveReturnGateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityAboveReturnGateProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityAboveReturnGateProjection.EntryRawFileOffset == 0x00BE6C8F
            && FrameHandoffCapacityAboveReturnGateProjection.EntryVirtualAddress == 0x00FE788F
            && FrameHandoffCapacityAboveReturnGateProjection.NormalBoundaryRawFileOffsetExclusive
                == 0x00BE6C9E
            && FrameHandoffCapacityAboveReturnGateProjection.NormalBoundaryVirtualAddressExclusive
                == 0x00FE789E
            && FrameHandoffCapacityAboveReturnGateProjection.ZeroBranchDestinationRawFileOffset
                == 0x00BE6CA2
            && FrameHandoffCapacityAboveReturnGateProjection.ZeroBranchDestinationVirtualAddress
                == 0x00FE78A2
            && FrameHandoffCapacityAboveReturnGateProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityAboveReturnGateProjection.EntryRawFileOffset)
                == FrameHandoffCapacityAboveReturnGateProjection.EntryVirtualAddress
            && FrameHandoffCapacityAboveReturnGateProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityAboveReturnGateProjection.NormalBoundaryRawFileOffsetExclusive)
                == FrameHandoffCapacityAboveReturnGateProjection.NormalBoundaryVirtualAddressExclusive
            && Normalize(FrameHandoffCapacityAboveReturnGateProjection.CapturedWindowBytes)
                == "83C4188D3C038B5DFC85F674068BCE"
            && FrameHandoffCapacityAboveReturnGateProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 15
            && InstructionEvidenceMatches());

        FrameHandoffCapacityAboveReturnGatePlan invalidEntry =
            FrameHandoffCapacityAboveReturnGateProjection.Project(new(
                ValidatedNormalEntry: false,
                EntryEbp: 0x10,
                EntryEbx: 0x20,
                EntryEsi: 0,
                HelperReturnEax: 1,
                FrameMinus4: 2));
        check("An unvalidated entry does not reach cleanup or consume later captures",
            invalidEntry.Outcome is null
            && invalidEntry.Reads.IsEmpty
            && !invalidEntry.StackCleanupReached
            && !invalidEntry.EdiLeaReached
            && !invalidEntry.FrameMinus4ReadReached
            && !invalidEntry.TestReached
            && !invalidEntry.BranchReached
            && !invalidEntry.EcxMoveReached
            && invalidEntry.UnreachedHelperReturnEaxCaptureProvided
            && invalidEntry.UnreachedFrameMinus4CaptureProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x00BE6C8F);

        FrameHandoffCapacityAboveReturnGatePlan missingHelperEax =
            FrameHandoffCapacityAboveReturnGateProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbp: 0x10,
                EntryEbx: 0x20,
                EntryEsi: 0x30,
                HelperReturnEax: null,
                FrameMinus4: 2));
        check("Missing helper EAX stops at BE6C92 after cleanup and before the LEA",
            missingHelperEax.Outcome is null
            && missingHelperEax.Reads.IsEmpty
            && missingHelperEax.StackCleanupReached
            && !missingHelperEax.EdiLeaReached
            && !missingHelperEax.HelperReturnEaxConsumed
            && !missingHelperEax.FrameMinus4ReadReached
            && !missingHelperEax.TestReached
            && !missingHelperEax.BranchReached
            && !missingHelperEax.EcxMoveReached
            && missingHelperEax.UnreachedFrameMinus4CaptureProvided
            && missingHelperEax.StopAtRawFileOffsetExclusive == 0x00BE6C92);

        FrameHandoffCapacityAboveReturnGatePlan missingFrameMinus4 =
            FrameHandoffCapacityAboveReturnGateProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbp: 0,
                EntryEbx: 0xFFFFFFFE,
                EntryEsi: 0,
                HelperReturnEax: 3,
                FrameMinus4: null));
        check("Missing frame-minus-four preserves cleanup and wrapped LEA, then stops at BE6C95",
            missingFrameMinus4.Outcome is null
            && missingFrameMinus4.StackCleanupReached
            && missingFrameMinus4.EdiLeaReached
            && missingFrameMinus4.EdiAfterLea == 1
            && missingFrameMinus4.HelperReturnEaxConsumed
            && missingFrameMinus4.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityAboveReturnGateRead(0x00BE6C95, 0xFFFFFFFC, null)
            })
            && missingFrameMinus4.FrameMinus4ReadReached
            && !missingFrameMinus4.FrameMinus4Captured
            && missingFrameMinus4.EbxAfterFrameMinus4Read is null
            && !missingFrameMinus4.TestReached
            && !missingFrameMinus4.BranchReached
            && !missingFrameMinus4.EcxMoveReached
            && missingFrameMinus4.StopAtRawFileOffsetExclusive == 0x00BE6C95);

        FrameHandoffCapacityAboveReturnGatePlan zeroBranch =
            FrameHandoffCapacityAboveReturnGateProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbp: 0x10,
                EntryEbx: 0xFFFFFFFE,
                EntryEsi: 0,
                HelperReturnEax: 3,
                FrameMinus4: 0xAABBCCDD));
        check("Zero entry ESI takes JZ, freshly loads EBX, and does not form ECX via MOV",
            zeroBranch.Outcome == FrameHandoffCapacityAboveReturnGateOutcome.ZeroBranchTaken
            && zeroBranch.StackCleanupReached
            && zeroBranch.EdiLeaReached
            && zeroBranch.EdiAfterLea == 1
            && zeroBranch.HelperReturnEaxConsumed
            && zeroBranch.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityAboveReturnGateRead(0x00BE6C95, 0x0C, 0xAABBCCDD)
            })
            && zeroBranch.FrameMinus4Captured
            && zeroBranch.EbxAfterFrameMinus4Read == 0xAABBCCDD
            && zeroBranch.TestReached
            && zeroBranch.EntryEsiIsZero == true
            && zeroBranch.BranchReached
            && zeroBranch.BranchTaken == true
            && zeroBranch.SelectedDestinationRawFileOffset == 0x00BE6CA2
            && !zeroBranch.EcxMoveReached
            && zeroBranch.EcxAfterMove is null
            && zeroBranch.StopAtRawFileOffsetExclusive == 0x00BE6CA2);

        FrameHandoffCapacityAboveReturnGatePlan nonzeroFallThrough =
            FrameHandoffCapacityAboveReturnGateProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbp: 4,
                EntryEbx: 7,
                EntryEsi: uint.MaxValue,
                HelperReturnEax: uint.MaxValue,
                FrameMinus4: 0x12345678));
        check("Nonzero entry ESI falls through and MOV copies the captured entry ESI to ECX",
            nonzeroFallThrough.Outcome
                == FrameHandoffCapacityAboveReturnGateOutcome.NonzeroFallThrough
            && nonzeroFallThrough.EdiAfterLea == 6
            && nonzeroFallThrough.EbxAfterFrameMinus4Read == 0x12345678
            && nonzeroFallThrough.TestReached
            && nonzeroFallThrough.EntryEsiIsZero == false
            && nonzeroFallThrough.BranchReached
            && nonzeroFallThrough.BranchTaken == false
            && nonzeroFallThrough.EcxMoveReached
            && nonzeroFallThrough.EcxAfterMove == uint.MaxValue
            && nonzeroFallThrough.SelectedDestinationRawFileOffset == 0x00BE6C9E
            && nonzeroFallThrough.StopAtRawFileOffsetExclusive == 0x00BE6C9E);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x00BE6C8F, "83 C4 18", null, null),
            (0x00BE6C92, "8D 3C 03", null, null),
            (0x00BE6C95, "8B 5D FC", null, null),
            (0x00BE6C98, "85 F6", null, null),
            (0x00BE6C9A, "74 06", 0x00BE6CA2, 0x00FE78A2),
            (0x00BE6C9C, "8B CE", null, null)
        };
        uint nextRawOffset = FrameHandoffCapacityAboveReturnGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (FrameHandoffCapacityAboveReturnGateInstructionEvidence instruction
            in FrameHandoffCapacityAboveReturnGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacityAboveReturnGateProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != FrameHandoffCapacityAboveReturnGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == FrameHandoffCapacityAboveReturnGateProjection.NormalBoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                FrameHandoffCapacityAboveReturnGateProjection.CapturedWindowBytes)
            && FrameHandoffCapacityAboveReturnGateProjection.Instructions
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
