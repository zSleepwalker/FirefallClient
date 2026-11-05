using Firefall.Reconstruction.Startup;

internal static class FrameHandoffSlotPublicationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE bytes, flat IDA instructions, address mapping, branch joins, and pre-test bound agree",
            FrameHandoffSlotPublicationProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffSlotPublicationProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffSlotPublicationProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffSlotPublicationProjection.PublicationStoreEntryRawFileOffset
                == 0x00BE6508
            && FrameHandoffSlotPublicationProjection.ExistingNonzeroSlotBranchEntryRawFileOffset
                == 0x00BE650E
            && FrameHandoffSlotPublicationProjection.RawFileOffsetToVirtualAddress(0x00BE6508)
                == 0x00FE7108
            && FrameHandoffSlotPublicationProjection.StopBeforeEcxTestRawFileOffsetExclusive
                == 0x00BE651C
            && FrameHandoffSlotPublicationProjection.EcxTestBoundaryBytes == "85 C9"
            && FrameHandoffSlotPublicationProjection.EcxTestBranchBytes == "74 07"
            && BranchTarget(
                FrameHandoffSlotPublicationProjection.EcxTestBranchRawFileOffset,
                0x07)
                == FrameHandoffSlotPublicationProjection.EcxTestBranchTargetRawFileOffset
            && Normalize(FrameHandoffSlotPublicationProjection.CapturedWindowBytes)
                == "8B4604890C038B460483E7038B04188D0CB85F5B"
            && FrameHandoffSlotPublicationProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x14
            && FrameHandoffSlotPublicationProjection.StorePathBranchBytes == "75 06"
            && FrameHandoffSlotPublicationProjection.ExistingSlotBranchBytes == "75 1C"
            && BranchTarget(
                FrameHandoffSlotPublicationProjection.StorePathBranchRawFileOffset,
                0x06)
                == FrameHandoffSlotPublicationProjection.StorePathBranchTargetRawFileOffset
            && BranchTarget(
                FrameHandoffSlotPublicationProjection.ExistingSlotBranchRawFileOffset,
                0x1C)
                == FrameHandoffSlotPublicationProjection.ExistingSlotBranchTargetRawFileOffset
            && InstructionEvidenceMatches());

        FrameHandoffSlotPublicationPlan missingEntry =
            FrameHandoffSlotPublicationProjection.Project(new(
                null,
                0x1000,
                0x2000,
                3,
                4,
                5,
                6,
                7));
        FrameHandoffSlotPublicationPlan invalidEntry =
            FrameHandoffSlotPublicationProjection.Project(new(
                (FrameHandoffSlotPublicationEntry)0xFF,
                0x1000,
                0x2000,
                3,
                4,
                5,
                6,
                7));
        check("Only an explicitly captured 6508 or 650E branch entry admits projection",
            missingEntry.Outcome
                == FrameHandoffSlotPublicationOutcome.MissingBranchEntryCapture
            && !missingEntry.StoreBaseReadReached
            && !missingEntry.StoreInstructionReached
            && !missingEntry.FreshBaseReadReached
            && missingEntry.UnreachedStoreBaseCaptureProvided
            && missingEntry.UnreachedEcxCaptureProvided
            && missingEntry.UnreachedFreshBaseCaptureProvided
            && missingEntry.UnreachedSlotDwordCaptureProvided
            && missingEntry.StopAtRawFileOffsetExclusive == 0x00BE6508
            && invalidEntry.Outcome
                == FrameHandoffSlotPublicationOutcome.InvalidBranchEntryCapture
            && !invalidEntry.StoreBaseReadReached
            && !invalidEntry.FreshBaseReadReached
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x00BE6508);

        FrameHandoffSlotPublicationPlan publicationPath =
            FrameHandoffSlotPublicationProjection.Project(new(
                FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                0xFFFFFFFC,
                0xFFFFFFE0,
                7,
                0xCAFEBABE,
                0x20,
                0x30,
                0xFFFFFFFE));
        check("The 6508 route stores using its first base, then independently reloads for the later slot read",
            publicationPath.Outcome
                == FrameHandoffSlotPublicationOutcome.ProjectionComplete
            && publicationPath.StoreBaseReadAddress == 0
            && publicationPath.StoreBaseReadValue == 0x20
            && publicationPath.StoreBaseReadReached
            && publicationPath.StoreBaseCaptured
            && publicationPath.StoreInstructionReached
            && publicationPath.StoreWriteProjected
            && publicationPath.StoreWriteAddress == 0
            && publicationPath.StoreWriteValue == 0xCAFEBABE
            && publicationPath.FreshBaseReadAddress == 0
            && publicationPath.FreshBaseReadValue == 0x30
            && publicationPath.FreshBaseReadReached
            && publicationPath.FreshBaseCaptured
            && publicationPath.SlotReadAddress == 0x10
            && publicationPath.SlotReadValue == 0xFFFFFFFE
            && publicationPath.SlotReadReached
            && publicationPath.SlotDwordCaptured
            && publicationPath.MaskedEdi == 3
            && publicationPath.ComputedEcxBeforeTest == 0x0A
            && publicationPath.StackPopRegisters.SequenceEqual(new[] { "EDI", "EBX" })
            && publicationPath.StopAtRawFileOffsetExclusive == 0x00BE651C
            && !publicationPath.UnreachedStoreBaseCaptureProvided
            && !publicationPath.UnreachedEcxCaptureProvided
            && !publicationPath.UnreachedFreshBaseCaptureProvided
            && !publicationPath.UnreachedSlotDwordCaptureProvided);

        FrameHandoffSlotPublicationPlan skippedStorePath =
            FrameHandoffSlotPublicationProjection.Project(new(
                FrameHandoffSlotPublicationEntry.ExistingNonzeroSlotBranchAt650E,
                0xFFFFFFFF,
                0xFFFFFFFC,
                5,
                0x12345678,
                0xDEADBEEF,
                8,
                0xFFFFFFFC));
        check("The 650E nonzero-slot branch skips the publication store and its 6508 captures",
            skippedStorePath.Outcome
                == FrameHandoffSlotPublicationOutcome.ProjectionComplete
            && !skippedStorePath.StoreBaseReadReached
            && !skippedStorePath.StoreBaseCaptured
            && !skippedStorePath.StoreInstructionReached
            && !skippedStorePath.StoreWriteProjected
            && skippedStorePath.StoreWriteAddress is null
            && skippedStorePath.StoreWriteValue is null
            && skippedStorePath.UnreachedStoreBaseCaptureProvided
            && skippedStorePath.UnreachedEcxCaptureProvided
            && skippedStorePath.FreshBaseReadAddress == 3
            && skippedStorePath.FreshBaseReadValue == 8
            && skippedStorePath.SlotReadAddress == 4
            && skippedStorePath.SlotReadValue == 0xFFFFFFFC
            && skippedStorePath.MaskedEdi == 1
            && skippedStorePath.ComputedEcxBeforeTest == 0
            && skippedStorePath.StopAtRawFileOffsetExclusive == 0x00BE651C);

        FrameHandoffSlotPublicationPlan missingStoreBase =
            FrameHandoffSlotPublicationProjection.Project(new(
                FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                0x1000,
                0x2000,
                1,
                0xAABBCCDD,
                null,
                3,
                4));
        FrameHandoffSlotPublicationPlan missingStoreEcx =
            FrameHandoffSlotPublicationProjection.Project(new(
                FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                0x3000,
                0x4000,
                2,
                null,
                5,
                6,
                7));
        check("Missing 6508 inputs block the store and mark later supplied captures unreached",
            missingStoreBase.Outcome
                == FrameHandoffSlotPublicationOutcome.MissingStoreBaseCapture
            && missingStoreBase.StoreBaseReadAddress == 0x1004
            && missingStoreBase.StoreBaseReadReached
            && !missingStoreBase.StoreBaseCaptured
            && !missingStoreBase.StoreInstructionReached
            && !missingStoreBase.StoreWriteProjected
            && missingStoreBase.UnreachedEcxCaptureProvided
            && missingStoreBase.UnreachedFreshBaseCaptureProvided
            && missingStoreBase.UnreachedSlotDwordCaptureProvided
            && missingStoreBase.StopAtRawFileOffsetExclusive == 0x00BE6508
            && missingStoreEcx.Outcome
                == FrameHandoffSlotPublicationOutcome.MissingStoreEcxCapture
            && missingStoreEcx.StoreBaseReadValue == 5
            && missingStoreEcx.StoreInstructionReached
            && !missingStoreEcx.StoreWriteProjected
            && missingStoreEcx.StoreWriteAddress == 0x4005
            && missingStoreEcx.StoreWriteValue is null
            && missingStoreEcx.UnreachedFreshBaseCaptureProvided
            && missingStoreEcx.UnreachedSlotDwordCaptureProvided
            && missingStoreEcx.StopAtRawFileOffsetExclusive == 0x00BE650B);

        FrameHandoffSlotPublicationPlan missingFreshBase =
            FrameHandoffSlotPublicationProjection.Project(new(
                FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                0x5000,
                0xFFFFFFF0,
                3,
                0x99887766,
                0x10,
                null,
                0x55));
        FrameHandoffSlotPublicationPlan missingSlotDword =
            FrameHandoffSlotPublicationProjection.Project(new(
                FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                0x6000,
                0x7000,
                6,
                0x1234,
                0x40,
                0x50,
                null));
        FrameHandoffSlotPublicationPlan skippedWithMissingFreshBase =
            FrameHandoffSlotPublicationProjection.Project(new(
                FrameHandoffSlotPublicationEntry.ExistingNonzeroSlotBranchAt650E,
                0x7000,
                0x8000,
                1,
                0xA5,
                0xDEAD,
                null,
                0x55));
        check("The independent fresh reload gates its read while preserving an earlier store",
            missingFreshBase.Outcome
                == FrameHandoffSlotPublicationOutcome.MissingFreshBaseCapture
            && missingFreshBase.StoreWriteProjected
            && missingFreshBase.StoreWriteAddress == 0
            && missingFreshBase.StoreWriteValue == 0x99887766
            && missingFreshBase.FreshBaseReadAddress == 0x5004
            && missingFreshBase.FreshBaseReadReached
            && !missingFreshBase.FreshBaseCaptured
            && missingFreshBase.SlotReadAddress is null
            && missingFreshBase.SlotReadValue is null
            && missingFreshBase.MaskedEdi is null
            && missingFreshBase.UnreachedSlotDwordCaptureProvided
            && missingFreshBase.StopAtRawFileOffsetExclusive == 0x00BE650E
            && missingSlotDword.Outcome
                == FrameHandoffSlotPublicationOutcome.MissingSlotDwordCapture
            && missingSlotDword.StoreWriteProjected
            && missingSlotDword.StoreWriteValue == 0x1234
            && missingSlotDword.FreshBaseReadValue == 0x50
            && missingSlotDword.SlotReadAddress == 0x7050
            && missingSlotDword.SlotReadReached
            && !missingSlotDword.SlotDwordCaptured
            && missingSlotDword.MaskedEdi == 2
            && missingSlotDword.ComputedEcxBeforeTest is null
            && missingSlotDword.StopAtRawFileOffsetExclusive == 0x00BE6514
            && skippedWithMissingFreshBase.Outcome
                == FrameHandoffSlotPublicationOutcome.MissingFreshBaseCapture
            && !skippedWithMissingFreshBase.StoreInstructionReached
            && !skippedWithMissingFreshBase.StoreWriteProjected
            && skippedWithMissingFreshBase.UnreachedStoreBaseCaptureProvided
            && skippedWithMissingFreshBase.UnreachedEcxCaptureProvided
            && skippedWithMissingFreshBase.UnreachedSlotDwordCaptureProvided
            && skippedWithMissingFreshBase.StopAtRawFileOffsetExclusive == 0x00BE650E);

        FrameHandoffSlotPublicationPlan wrapping =
            FrameHandoffSlotPublicationProjection.Project(new(
                FrameHandoffSlotPublicationEntry.ExistingNonzeroSlotBranchAt650E,
                uint.MaxValue,
                uint.MaxValue - 3,
                uint.MaxValue,
                null,
                null,
                0xFFFFFFFE,
                0xFFFFFFFF));
        check("DWORD receiver, base, and scaled-index address arithmetic wraps independently",
            FrameHandoffSlotPublicationProjection.ReceiverPlus04Address(uint.MaxValue) == 3
            && FrameHandoffSlotPublicationProjection.StoreWriteAddress(
                uint.MaxValue - 7,
                8) == 0
            && FrameHandoffSlotPublicationProjection.SlotReadAddress(
                uint.MaxValue - 3,
                8) == 4
            && FrameHandoffSlotPublicationProjection.ComputeEcxBeforeTest(
                uint.MaxValue,
                uint.MaxValue) == 11
            && wrapping.Outcome == FrameHandoffSlotPublicationOutcome.ProjectionComplete
            && wrapping.FreshBaseReadAddress == 3
            && wrapping.SlotReadAddress == 0xFFFFFFFA
            && wrapping.MaskedEdi == 3
            && wrapping.ComputedEcxBeforeTest == 11
            && wrapping.StopAtRawFileOffsetExclusive == 0x00BE651C);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x00BE6508, "8B 46 04"),
            (0x00BE650B, "89 0C 03"),
            (0x00BE650E, "8B 46 04"),
            (0x00BE6511, "83 E7 03"),
            (0x00BE6514, "8B 04 18"),
            (0x00BE6517, "8D 0C B8"),
            (0x00BE651A, "5F 5B")
        };
        uint nextRawOffset = FrameHandoffSlotPublicationProjection.PublicationStoreEntryRawFileOffset;
        string bytes = string.Empty;
        foreach (FrameHandoffSlotPublicationInstructionEvidence instruction
            in FrameHandoffSlotPublicationProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffSlotPublicationProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == FrameHandoffSlotPublicationProjection.StopBeforeEcxTestRawFileOffsetExclusive
            && bytes == Normalize(FrameHandoffSlotPublicationProjection.CapturedWindowBytes)
            && FrameHandoffSlotPublicationProjection.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected)
            && FrameHandoffSlotPublicationProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffSlotPublicationProjection.ExistingSlotBranchTargetRawFileOffset)
                == 0x00FE710E;
    }

    private static uint BranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
