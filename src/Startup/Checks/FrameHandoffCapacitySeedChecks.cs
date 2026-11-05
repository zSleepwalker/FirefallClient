using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacitySeedChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE section mapping and raw window match the flat-IDA instruction bytes through the join",
            FrameHandoffCapacitySeedProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacitySeedProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacitySeedProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacitySeedProjection.EntryRawFileOffset == 0x00BE6B50
            && FrameHandoffCapacitySeedProjection.EntryVirtualAddress == 0x00FE7750
            && FrameHandoffCapacitySeedProjection.ReceiverPlus08ReadRawFileOffset == 0x00BE6B63
            && FrameHandoffCapacitySeedProjection.ReceiverPlus08ReadVirtualAddress == 0x00FE7763
            && FrameHandoffCapacitySeedProjection.JoinRawFileOffset == 0x00BE6B70
            && FrameHandoffCapacitySeedProjection.JoinVirtualAddress == 0x00FE7770
            && FrameHandoffCapacitySeedProjection.EndRawFileOffsetExclusive == 0x00BE6B70
            && FrameHandoffCapacitySeedProjection.RawFileOffsetToVirtualAddress(0x00BE6B50)
                == 0x00FE7750
            && FrameHandoffCapacitySeedProjection.RawFileOffsetToVirtualAddress(0x00BE6B70)
                == 0x00FE7770
            && FrameHandoffCapacitySeedProjection.CapturedWindowBytes
                == "55 8B EC 83 EC 14 53 8B D9 56 BE 01 00 00 00 89 5D EC "
                    + "57 8B 53 08 85 D2 0F 45 F2 EB 03 8D 49 00"
            && FrameHandoffCapacitySeedProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x20
            && InstructionEvidenceMatches());

        check("The unconditional short jump targets raw 0xBE6B70 and the projection stops before that join",
            ShortBranchTarget(
                0x00BE6B6B,
                0x03) == FrameHandoffCapacitySeedProjection.JoinRawFileOffset
            && FrameHandoffCapacitySeedProjection.Instructions[^2]
                .BranchTargetRawFileOffset == FrameHandoffCapacitySeedProjection.JoinRawFileOffset
            && FrameHandoffCapacitySeedProjection.Instructions[^1].RawFileOffset
                + (uint)FrameHandoffCapacitySeedProjection.Instructions[^1].Bytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                == FrameHandoffCapacitySeedProjection.EndRawFileOffsetExclusive
            && FrameHandoffCapacitySeedProjection.ReceiverAddress(0, 0x08) == 0x08
            && FrameHandoffCapacitySeedProjection.ReceiverAddress(uint.MaxValue - 7, 0x08) == 0);

        FrameHandoffCapacitySeedPlan missing =
            FrameHandoffCapacitySeedProjection.Project(new(0xE000, 0x1234, null));
        check("A missing receiver read preserves the known ESI seed and receiver provenance without reaching the join",
            missing.Outcome
                == FrameHandoffCapacitySeedOutcome.MissingReceiverPlus08Capture
            && missing.Capture.EntryFrameEbp == 0xE000
            && missing.EbxReceiverFromEntryEcx == 0x1234
            && missing.ReceiverProvenancePreserved
            && missing.ReceiverFrameStoreProjected
            && missing.ReceiverFrameStoreAddress == 0xDFEC
            && missing.ReceiverFrameStoreValue == 0x1234
            && missing.EsiSeedEstablished
            && missing.EsiSeedBeforeReceiverRead == 1
            && missing.ReceiverPlus08ReadAddress == 0x123C
            && missing.ReceiverPlus08ReadReached
            && !missing.ReceiverPlus08Captured
            && missing.ReceiverPlus08ReadValue is null
            && !missing.TestEdxEdxReached
            && missing.ZeroFlag is null
            && !missing.ConditionalMoveReached
            && missing.ConditionalMoveCopiedEdxToEsi is null
            && missing.EsiAfterConditionalMove is null
            && !missing.JumpToJoinReached
            && missing.ReachedJoinRawFileOffset is null
            && missing.EsiAtJoin is null
            && missing.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySeedProjection.ReceiverPlus08ReadRawFileOffset);

        FrameHandoffCapacitySeedPlan zero =
            FrameHandoffCapacitySeedProjection.Project(new(0xA000, 0x8000, 0));
        FrameHandoffCapacitySeedPlan nonzero =
            FrameHandoffCapacitySeedProjection.Project(new(0xB000, 0x9000, 0x80000000));
        check("Zero retains the initial seed while nonzero copies the independent read into ESI",
            zero.Outcome == FrameHandoffCapacitySeedOutcome.ZeroReadKeepsInitialSeed
            && zero.ReceiverFrameStoreAddress == 0x9FEC
            && zero.ReceiverFrameStoreValue == 0x8000
            && zero.ReceiverPlus08ReadAddress == 0x8008
            && zero.TestEdxEdxReached
            && zero.ZeroFlag == true
            && zero.ConditionalMoveReached
            && zero.ConditionalMoveCopiedEdxToEsi == false
            && zero.EsiAfterConditionalMove == 1
            && zero.JumpToJoinReached
            && zero.ReachedJoinRawFileOffset == 0x00BE6B70
            && zero.EsiAtJoin == 1
            && zero.StopAtRawFileOffsetExclusive == 0x00BE6B70
            && nonzero.Outcome == FrameHandoffCapacitySeedOutcome.NonzeroReadReplacesSeed
            && nonzero.ReceiverFrameStoreAddress == 0xAFEC
            && nonzero.ReceiverFrameStoreValue == 0x9000
            && nonzero.ReceiverPlus08ReadAddress == 0x9008
            && nonzero.ZeroFlag == false
            && nonzero.ConditionalMoveCopiedEdxToEsi == true
            && nonzero.EsiAfterConditionalMove == 0x80000000
            && nonzero.EsiAtJoin == 0x80000000
            && nonzero.ReachedJoinRawFileOffset == 0x00BE6B70
            && nonzero.StopAtRawFileOffsetExclusive == 0x00BE6B70);

        FrameHandoffCapacitySeedPlan wrappingAddress =
            FrameHandoffCapacitySeedProjection.Project(new(0x10, uint.MaxValue - 7, 2));
        check("Receiver-plus-eight address formation wraps as a 32-bit address",
            wrappingAddress.ReceiverPlus08ReadAddress == 0
            && wrappingAddress.ReceiverPlus08Captured
            && wrappingAddress.ReceiverPlus08ReadValue == 2
            && wrappingAddress.EsiAtJoin == 2
            && wrappingAddress.ReachedJoinRawFileOffset == 0x00BE6B70);

        FrameHandoffCapacitySeedPlan wrappingFrameStore =
            FrameHandoffCapacitySeedProjection.Project(new(0x0C, 0x5678, null));
        check("The receiver frame-store address wraps as a 32-bit address before a missing receiver read",
            wrappingFrameStore.ReceiverFrameStoreProjected
            && wrappingFrameStore.ReceiverFrameStoreAddress == 0xFFFFFFF8
            && wrappingFrameStore.ReceiverFrameStoreValue == 0x5678
            && wrappingFrameStore.ReceiverPlus08ReadReached
            && !wrappingFrameStore.ReceiverPlus08Captured
            && wrappingFrameStore.EsiSeedEstablished
            && wrappingFrameStore.EsiSeedBeforeReceiverRead == 1
            && !wrappingFrameStore.JumpToJoinReached
            && wrappingFrameStore.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySeedProjection.ReceiverPlus08ReadRawFileOffset);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            uint Virtual,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x00BE6B50, 0x00FE7750, "55", null, null),
            (0x00BE6B51, 0x00FE7751, "8B EC", null, null),
            (0x00BE6B53, 0x00FE7753, "83 EC 14", null, null),
            (0x00BE6B56, 0x00FE7756, "53", null, null),
            (0x00BE6B57, 0x00FE7757, "8B D9", null, null),
            (0x00BE6B59, 0x00FE7759, "56", null, null),
            (0x00BE6B5A, 0x00FE775A, "BE 01 00 00 00", null, null),
            (0x00BE6B5F, 0x00FE775F, "89 5D EC", null, null),
            (0x00BE6B62, 0x00FE7762, "57", null, null),
            (0x00BE6B63, 0x00FE7763, "8B 53 08", null, null),
            (0x00BE6B66, 0x00FE7766, "85 D2", null, null),
            (0x00BE6B68, 0x00FE7768, "0F 45 F2", null, null),
            (0x00BE6B6B, 0x00FE776B, "EB 03", 0x00BE6B70, 0x00FE7770),
            (0x00BE6B6D, 0x00FE776D, "8D 49 00", null, null)
        };
        uint nextRawOffset = FrameHandoffCapacitySeedProjection.EntryRawFileOffset;
        string instructionBytes = string.Empty;

        foreach (FrameHandoffCapacitySeedInstructionEvidence instruction
            in FrameHandoffCapacitySeedProjection.Instructions)
        {
            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset)
                return false;

            if (instruction.VirtualAddress
                != FrameHandoffCapacitySeedProjection.RawFileOffsetToVirtualAddress(
                    instruction.RawFileOffset))
            {
                return false;
            }

            if (instruction.BranchTargetRawFileOffset is uint targetRaw
                && instruction.BranchTargetVirtualAddress
                    != FrameHandoffCapacitySeedProjection.RawFileOffsetToVirtualAddress(targetRaw))
            {
                return false;
            }

            nextRawOffset += (uint)(bytes.Length / 2);
            instructionBytes += bytes;
        }

        return nextRawOffset == FrameHandoffCapacitySeedProjection.EndRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(FrameHandoffCapacitySeedProjection.CapturedWindowBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            && FrameHandoffCapacitySeedProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.VirtualAddress,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);
}
