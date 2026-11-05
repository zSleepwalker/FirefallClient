using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityLoopChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample identity, raw range, VA mapping, and exact instruction bytes agree",
            FrameHandoffCapacityLoopProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityLoopProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityLoopProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityLoopProjection.EntryRawFileOffset == 0x00BE6B70
            && FrameHandoffCapacityLoopProjection.EntryVirtualAddress == 0x00FE7770
            && FrameHandoffCapacityLoopProjection.ExitRawFileOffset == 0x00BE6B91
            && FrameHandoffCapacityLoopProjection.FailureRawFileOffset == 0x00BE6CC0
            && FrameHandoffCapacityLoopProjection.EndRawFileOffsetExclusive == 0x00BE6B91
            && FrameHandoffCapacityLoopProjection.RawFileOffsetToVirtualAddress(0x00BE6B70)
                == 0x00FE7770
            && FrameHandoffCapacityLoopProjection.RawFileOffsetToVirtualAddress(0x00BE6B91)
                == 0x00FE7791
            && FrameHandoffCapacityLoopProjection.RawFileOffsetToVirtualAddress(0x00BE6CC0)
                == 0x00FE78C0
            && string.Concat(FrameHandoffCapacityLoopProjection.CapturedWindowBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8BC62BC283F801720583FE087313B8FFFFFF0F2BC63BC60F823301000003F6EBDF"
            && FrameHandoffCapacityLoopProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x21
            && InstructionEvidenceMatches());

        check("The signed short branches and near failure branch reach the raw-file destinations",
            FrameHandoffCapacityLoopProjection.ShortBranchTarget(
                FrameHandoffCapacityLoopProjection.ZeroDifferenceBranchRawFileOffset,
                0x05) == FrameHandoffCapacityLoopProjection.UpperConstantLoadRawFileOffset
            && FrameHandoffCapacityLoopProjection.ShortBranchTarget(
                FrameHandoffCapacityLoopProjection.ExitBranchRawFileOffset,
                0x13) == FrameHandoffCapacityLoopProjection.ExitRawFileOffset
            && FrameHandoffCapacityLoopProjection.RelativeBranchTarget(
                FrameHandoffCapacityLoopProjection.FailureBranchRawFileOffset,
                6,
                0x133) == FrameHandoffCapacityLoopProjection.FailureRawFileOffset
            && FrameHandoffCapacityLoopProjection.ShortBranchTarget(
                FrameHandoffCapacityLoopProjection.LoopBranchRawFileOffset,
                0xDF) == FrameHandoffCapacityLoopProjection.LoopRawFileOffset);

        FrameHandoffCapacityLoopPlan zeroDifference =
            FrameHandoffCapacityLoopProjection.Project(new(7, 7));
        check("A zero wrapped subtraction bypasses the ESI-versus-eight compare and takes one doubling iteration",
            zeroDifference.Outcome
                == FrameHandoffCapacityLoopOutcome.DoubleAndLoopToRawFileOffset70
            && zeroDifference.Capture.EntryEsi == 7
            && zeroDifference.Capture.EntryEdx == 7
            && zeroDifference.SubtractReached
            && zeroDifference.EaxAfterEntrySubtract == 0
            && zeroDifference.CompareEaxWithOneReached
            && zeroDifference.EntrySubtractResultIsZero == true
            && zeroDifference.ZeroDifferenceBranchTaken == true
            && !zeroDifference.CompareEsiWithEightReached
            && zeroDifference.EsiAtLeastEight is null
            && zeroDifference.ExitBranchTaken is null
            && zeroDifference.LoadUpperConstantReached
            && zeroDifference.EaxAfterUpperConstantLoad
                == FrameHandoffCapacityLoopProjection.UpperSubtractConstant
            && zeroDifference.EaxAfterUpperSubtract == 0x0FFFFFF8
            && zeroDifference.UpperSubtractResultLessThanEsi == false
            && zeroDifference.FailureBranchTaken == false
            && zeroDifference.EsiAfterDouble == 14
            && zeroDifference.LoopBackReached
            && zeroDifference.SelectedDestinationRawFileOffset == 0x00BE6B70
            && zeroDifference.StopAtRawFileOffsetExclusive == 0x00BE6B91);

        FrameHandoffCapacityLoopPlan belowThreshold =
            FrameHandoffCapacityLoopProjection.Project(new(7, 6));
        FrameHandoffCapacityLoopPlan atThreshold =
            FrameHandoffCapacityLoopProjection.Project(new(8, 7));
        check("A nonzero subtraction compares ESI with eight: below doubles, while eight exits",
            belowThreshold.Outcome
                == FrameHandoffCapacityLoopOutcome.DoubleAndLoopToRawFileOffset70
            && belowThreshold.EaxAfterEntrySubtract == 1
            && belowThreshold.EntrySubtractResultIsZero == false
            && belowThreshold.ZeroDifferenceBranchTaken == false
            && belowThreshold.CompareEsiWithEightReached
            && belowThreshold.EsiAtLeastEight == false
            && belowThreshold.ExitBranchTaken == false
            && belowThreshold.EaxAfterUpperSubtract == 0x0FFFFFF8
            && belowThreshold.EsiAfterDouble == 14
            && belowThreshold.LoopBackReached
            && atThreshold.Outcome == FrameHandoffCapacityLoopOutcome.ExitAtRawFileOffset91
            && atThreshold.EaxAfterEntrySubtract == 1
            && atThreshold.CompareEsiWithEightReached
            && atThreshold.EsiAtLeastEight == true
            && atThreshold.ExitBranchTaken == true
            && !atThreshold.LoadUpperConstantReached
            && !atThreshold.UpperSubtractReached
            && !atThreshold.CompareUpperSubtractWithEsiReached
            && !atThreshold.DoubleEsiReached
            && !atThreshold.LoopBackReached
            && atThreshold.SelectedDestinationRawFileOffset == 0x00BE6B91
            && atThreshold.StopAtRawFileOffsetExclusive == 0x00BE6B91);

        FrameHandoffCapacityLoopPlan failureGate =
            FrameHandoffCapacityLoopProjection.Project(new(0x08000000, 0x08000000));
        FrameHandoffCapacityLoopPlan failureBoundary =
            FrameHandoffCapacityLoopProjection.Project(new(0x07FFFFFF, 0x07FFFFFF));
        check("The unsigned upper-subtract comparison selects failure only above its exact boundary",
            failureGate.Outcome
                == FrameHandoffCapacityLoopOutcome.FailureAtRawFileOffset6CC0
            && failureGate.EntrySubtractResultIsZero == true
            && !failureGate.CompareEsiWithEightReached
            && failureGate.EaxAfterUpperSubtract == 0x07FFFFFF
            && failureGate.CompareUpperSubtractWithEsiReached
            && failureGate.UpperSubtractResultLessThanEsi == true
            && failureGate.FailureBranchTaken == true
            && !failureGate.DoubleEsiReached
            && !failureGate.LoopBackReached
            && failureGate.SelectedDestinationRawFileOffset == 0x00BE6CC0
            && failureGate.StopAtRawFileOffsetExclusive == 0x00BE6CC0
            && failureBoundary.Outcome
                == FrameHandoffCapacityLoopOutcome.DoubleAndLoopToRawFileOffset70
            && failureBoundary.EaxAfterUpperSubtract == 0x08000000
            && failureBoundary.UpperSubtractResultLessThanEsi == false
            && failureBoundary.EsiAfterDouble == 0x0FFFFFFE);

        FrameHandoffCapacityLoopPlan wrappedEntrySubtract =
            FrameHandoffCapacityLoopProjection.Project(new(1, 2));
        FrameHandoffCapacityLoopPlan wrappedUpperSubtract =
            FrameHandoffCapacityLoopProjection.Project(new(uint.MaxValue, uint.MaxValue));
        check("Both subtraction operations preserve DWORD wraparound and the projection stops after one loop branch",
            wrappedEntrySubtract.Outcome
                == FrameHandoffCapacityLoopOutcome.DoubleAndLoopToRawFileOffset70
            && wrappedEntrySubtract.EaxAfterEntrySubtract == uint.MaxValue
            && wrappedEntrySubtract.EntrySubtractResultIsZero == false
            && wrappedEntrySubtract.CompareEsiWithEightReached
            && wrappedEntrySubtract.EaxAfterUpperSubtract == 0x0FFFFFFE
            && wrappedEntrySubtract.EsiAfterDouble == 2
            && wrappedEntrySubtract.LoopBackReached
            && wrappedEntrySubtract.StopAtRawFileOffsetExclusive == 0x00BE6B91
            && wrappedUpperSubtract.Outcome
                == FrameHandoffCapacityLoopOutcome.FailureAtRawFileOffset6CC0
            && wrappedUpperSubtract.EaxAfterEntrySubtract == 0
            && wrappedUpperSubtract.EaxAfterUpperSubtract == 0x10000000
            && wrappedUpperSubtract.UpperSubtractResultLessThanEsi == true
            && !wrappedUpperSubtract.DoubleEsiReached
            && !wrappedUpperSubtract.LoopBackReached);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            uint Virtual,
            string Bytes,
            uint? TargetRaw,
            uint? TargetVirtual)[]
        {
            (0x00BE6B70, 0x00FE7770, "8B C6", null, null),
            (0x00BE6B72, 0x00FE7772, "2B C2", null, null),
            (0x00BE6B74, 0x00FE7774, "83 F8 01", null, null),
            (0x00BE6B77, 0x00FE7777, "72 05", 0x00BE6B7E, 0x00FE777E),
            (0x00BE6B79, 0x00FE7779, "83 FE 08", null, null),
            (0x00BE6B7C, 0x00FE777C, "73 13", 0x00BE6B91, 0x00FE7791),
            (0x00BE6B7E, 0x00FE777E, "B8 FF FF FF 0F", null, null),
            (0x00BE6B83, 0x00FE7783, "2B C6", null, null),
            (0x00BE6B85, 0x00FE7785, "3B C6", null, null),
            (0x00BE6B87, 0x00FE7787, "0F 82 33 01 00 00", 0x00BE6CC0, 0x00FE78C0),
            (0x00BE6B8D, 0x00FE778D, "03 F6", null, null),
            (0x00BE6B8F, 0x00FE778F, "EB DF", 0x00BE6B70, 0x00FE7770)
        };
        uint nextRawOffset = FrameHandoffCapacityLoopProjection.EntryRawFileOffset;
        string instructionBytes = string.Empty;

        foreach (FrameHandoffCapacityLoopInstructionEvidence instruction
            in FrameHandoffCapacityLoopProjection.Instructions)
        {
            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacityLoopProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            if (instruction.BranchTargetRawFileOffset is uint targetRaw
                && instruction.BranchTargetVirtualAddress
                    != FrameHandoffCapacityLoopProjection.RawFileOffsetToVirtualAddress(targetRaw))
            {
                return false;
            }

            nextRawOffset += (uint)(bytes.Length / 2);
            instructionBytes += bytes;
        }

        return nextRawOffset == FrameHandoffCapacityLoopProjection.EndRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(FrameHandoffCapacityLoopProjection.CapturedWindowBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            && FrameHandoffCapacityLoopProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.VirtualAddress,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }
}
