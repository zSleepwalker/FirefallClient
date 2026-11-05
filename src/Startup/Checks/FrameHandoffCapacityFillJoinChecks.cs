using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityFillJoinChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE maps the full 13-byte raw range to its recorded virtual range",
            FrameHandoffCapacityFillJoinProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityFillJoinProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityFillJoinProjection.PeImageBase == 0x00400000
            && FrameHandoffCapacityFillJoinProjection.TextSectionRva == 0x00001000
            && FrameHandoffCapacityFillJoinProjection.TextSectionRawFileOffset == 0x00000400
            && FrameHandoffCapacityFillJoinProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityFillJoinProjection.EntryRawFileOffset == 0x00BE6C4A
            && FrameHandoffCapacityFillJoinProjection.EntryVirtualAddress == 0x00FE784A
            && FrameHandoffCapacityFillJoinProjection.EndRawFileOffsetExclusive == 0x00BE6C57
            && FrameHandoffCapacityFillJoinProjection.EndVirtualAddressExclusive == 0x00FE7857
            && FrameHandoffCapacityFillJoinProjection.CapturedWindowByteCount == 13
            && FrameHandoffCapacityFillJoinProjection.CapturedWindowBytes
                == "8B5DFC85FF74518BCF8BFBEB47"
            && FrameHandoffCapacityFillJoinProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityFillJoinProjection.EntryRawFileOffset)
                == FrameHandoffCapacityFillJoinProjection.EntryVirtualAddress
            && FrameHandoffCapacityFillJoinProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityFillJoinProjection.EndRawFileOffsetExclusive)
                == FrameHandoffCapacityFillJoinProjection.EndVirtualAddressExclusive
            && FrameHandoffCapacityFillJoinProjection.ShortBranchTarget(
                0x00BE6C4F, 0x51)
                == FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset
            && FrameHandoffCapacityFillJoinProjection.ShortBranchTarget(
                0x00BE6C55, 0x47)
                == FrameHandoffCapacityFillJoinProjection.NonzeroJumpTargetRawFileOffset
            && InstructionEvidenceMatches());

        FrameHandoffCapacityFillJoinPlan gateFalse =
            FrameHandoffCapacityFillJoinProjection.Project(
                new(false, 0x1000, 0x2000, 0xAABBCCDD));
        check("A false validated-join gate preserves identity and flags its supplied read as unreached",
            gateFalse.Outcome == FrameHandoffCapacityFillJoinOutcome.JoinNotValidated
            && gateFalse.Capture.EntryEbp == 0x1000
            && gateFalse.Capture.EntryEdi == 0x2000
            && !gateFalse.Capture.CaptureValidatedJoin
            && gateFalse.FrameMinus4ReadAddress == 0x0FFC
            && !gateFalse.FrameMinus4ReadReached
            && !gateFalse.FrameMinus4ReadCaptured
            && gateFalse.EbxAfterFrameMinus4Read is null
            && gateFalse.UnreachedFrameMinus4ReadCaptureProvided
            && !gateFalse.EntryEdiTestReached
            && gateFalse.EntryEdiZeroFlag is null
            && !gateFalse.ZeroBranchReached
            && gateFalse.ZeroBranchTaken is null
            && !gateFalse.EcxEntryEdiMoveReached
            && gateFalse.EcxAfterMove is null
            && !gateFalse.EdiLoadedEbxMoveReached
            && gateFalse.EdiAfterMove is null
            && !gateFalse.JumpReached
            && gateFalse.JumpTargetRawFileOffset is null
            && gateFalse.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFillJoinProjection.EntryRawFileOffset);

        FrameHandoffCapacityFillJoinPlan missingRead =
            FrameHandoffCapacityFillJoinProjection.Project(
                new(true, 0x1004, 0x1234, null));
        check("A missing frame-minus-four read stops at raw 0xBE6C4A before TEST",
            missingRead.Outcome == FrameHandoffCapacityFillJoinOutcome.MissingFrameMinus4Read
            && missingRead.FrameMinus4ReadAddress == 0x1000
            && missingRead.FrameMinus4ReadReached
            && !missingRead.FrameMinus4ReadCaptured
            && missingRead.EbxAfterFrameMinus4Read is null
            && !missingRead.UnreachedFrameMinus4ReadCaptureProvided
            && !missingRead.EntryEdiTestReached
            && missingRead.EntryEdiZeroFlag is null
            && !missingRead.ZeroBranchReached
            && !missingRead.EcxEntryEdiMoveReached
            && !missingRead.EdiLoadedEbxMoveReached
            && !missingRead.JumpReached
            && missingRead.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFillJoinProjection.EntryRawFileOffset);

        FrameHandoffCapacityFillJoinPlan zeroFrameAndZeroEntry =
            FrameHandoffCapacityFillJoinProjection.Project(
                new(true, 0x2000, 0, 0));
        check("A zero frame DWORD is a valid read and zero entry EDI takes the direct JZ route",
            zeroFrameAndZeroEntry.Outcome
                == FrameHandoffCapacityFillJoinOutcome.ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            && zeroFrameAndZeroEntry.FrameMinus4ReadAddress == 0x1FFC
            && zeroFrameAndZeroEntry.FrameMinus4ReadReached
            && zeroFrameAndZeroEntry.FrameMinus4ReadCaptured
            && zeroFrameAndZeroEntry.EbxAfterFrameMinus4Read == 0
            && zeroFrameAndZeroEntry.EntryEdiTestReached
            && zeroFrameAndZeroEntry.EntryEdiZeroFlag == true
            && zeroFrameAndZeroEntry.ZeroBranchReached
            && zeroFrameAndZeroEntry.ZeroBranchTaken == true
            && zeroFrameAndZeroEntry.BranchTargetRawFileOffset
                == FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset
            && !zeroFrameAndZeroEntry.EcxEntryEdiMoveReached
            && zeroFrameAndZeroEntry.EcxAfterMove is null
            && !zeroFrameAndZeroEntry.EdiLoadedEbxMoveReached
            && zeroFrameAndZeroEntry.EdiAfterMove is null
            && !zeroFrameAndZeroEntry.JumpReached
            && zeroFrameAndZeroEntry.JumpTargetRawFileOffset is null
            && zeroFrameAndZeroEntry.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset);

        FrameHandoffCapacityFillJoinPlan nonzeroEntry =
            FrameHandoffCapacityFillJoinProjection.Project(
                new(true, 0x3000, 0x87654321, 0xA1B2C3D4));
        check("A nonzero entry EDI is copied to ECX before the freshly loaded EBX replaces EDI",
            nonzeroEntry.Outcome
                == FrameHandoffCapacityFillJoinOutcome.NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
            && nonzeroEntry.FrameMinus4ReadAddress == 0x2FFC
            && nonzeroEntry.FrameMinus4ReadReached
            && nonzeroEntry.FrameMinus4ReadCaptured
            && nonzeroEntry.EbxAfterFrameMinus4Read == 0xA1B2C3D4
            && nonzeroEntry.EntryEdiTestReached
            && nonzeroEntry.EntryEdiZeroFlag == false
            && nonzeroEntry.ZeroBranchReached
            && nonzeroEntry.ZeroBranchTaken == false
            && nonzeroEntry.BranchTargetRawFileOffset is null
            && nonzeroEntry.EcxEntryEdiMoveReached
            && nonzeroEntry.EcxAfterMove == 0x87654321
            && nonzeroEntry.EdiLoadedEbxMoveReached
            && nonzeroEntry.EdiAfterMove == 0xA1B2C3D4
            && nonzeroEntry.JumpReached
            && nonzeroEntry.JumpTargetRawFileOffset
                == FrameHandoffCapacityFillJoinProjection.NonzeroJumpTargetRawFileOffset
            && nonzeroEntry.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityFillJoinProjection.NonzeroJumpTargetRawFileOffset);

        FrameHandoffCapacityFillJoinPlan wrappedFrameAddress =
            FrameHandoffCapacityFillJoinProjection.Project(
                new(true, 2, 1, uint.MaxValue));
        check("The frame-minus-four effective address wraps as a 32-bit DWORD",
            wrappedFrameAddress.FrameMinus4ReadAddress == 0xFFFFFFFE
            && wrappedFrameAddress.FrameMinus4ReadReached
            && wrappedFrameAddress.EbxAfterFrameMinus4Read == uint.MaxValue
            && wrappedFrameAddress.EcxAfterMove == 1
            && wrappedFrameAddress.EdiAfterMove == uint.MaxValue
            && wrappedFrameAddress.JumpTargetRawFileOffset
                == FrameHandoffCapacityFillJoinProjection.NonzeroJumpTargetRawFileOffset);

        FrameHandoffCapacityFillJoinPlan freshFrameRead =
            FrameHandoffCapacityFillJoinProjection.Project(
                new(true, 0x4004, 0xFEDCBA98, 0x13579BDF));
        check("The nonzero route takes EDI from this frame read while ECX retains entry EDI",
            freshFrameRead.Capture.EntryEdi == 0xFEDCBA98
            && freshFrameRead.EbxAfterFrameMinus4Read == 0x13579BDF
            && freshFrameRead.EcxAfterMove == freshFrameRead.Capture.EntryEdi
            && freshFrameRead.EdiAfterMove == freshFrameRead.EbxAfterFrameMinus4Read
            && freshFrameRead.Outcome
                == FrameHandoffCapacityFillJoinOutcome.NonzeroEntryEdiJumpToRawFileOffsetBE6C9E);
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
            (0x00BE6C4A, 0x00FE784A, "8B 5D FC", null, null),
            (0x00BE6C4D, 0x00FE784D, "85 FF", null, null),
            (0x00BE6C4F, 0x00FE784F, "74 51", 0x00BE6CA2, 0x00FE78A2),
            (0x00BE6C51, 0x00FE7851, "8B CF", null, null),
            (0x00BE6C53, 0x00FE7853, "8B FB", null, null),
            (0x00BE6C55, 0x00FE7855, "EB 47", 0x00BE6C9E, 0x00FE789E)
        };
        uint nextRawOffset = FrameHandoffCapacityFillJoinProjection.EntryRawFileOffset;
        string instructionBytes = string.Empty;

        foreach (FrameHandoffCapacityFillJoinInstructionEvidence instruction
            in FrameHandoffCapacityFillJoinProjection.Evidence)
        {
            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacityFillJoinProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            if (instruction.BranchTargetRawFileOffset is uint targetRaw
                && instruction.BranchTargetVirtualAddress
                    != FrameHandoffCapacityFillJoinProjection.RawFileOffsetToVirtualAddress(
                        targetRaw))
            {
                return false;
            }

            nextRawOffset += (uint)(bytes.Length / 2);
            instructionBytes += bytes;
        }

        return nextRawOffset == FrameHandoffCapacityFillJoinProjection.EndRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(FrameHandoffCapacityFillJoinProjection.CapturedWindowBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            && !FrameHandoffCapacityFillJoinProjection.Evidence.Any(instruction =>
                instruction.Bytes.StartsWith("F2 ", StringComparison.OrdinalIgnoreCase)
                || instruction.Bytes.StartsWith("F3 ", StringComparison.OrdinalIgnoreCase))
            && FrameHandoffCapacityFillJoinProjection.Evidence
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.VirtualAddress,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }
}
