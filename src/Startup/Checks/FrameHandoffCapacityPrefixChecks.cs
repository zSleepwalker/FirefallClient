using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityPrefixChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample window contains contiguous evidence from entry through PUSH EAX",
            FrameHandoffCapacityPrefixProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityPrefixProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityPrefixProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityPrefixProjection.EntryRawFileOffset == 0x00BE6B91
            && FrameHandoffCapacityPrefixProjection.EntryVirtualAddress == 0x00FE7791
            && FrameHandoffCapacityPrefixProjection.EndRawFileOffsetExclusive == 0x00BE6BB5
            && FrameHandoffCapacityPrefixProjection.EndVirtualAddressExclusive == 0x00FE77B5
            && FrameHandoffCapacityPrefixProjection.CapturedWindowBytes
                == "8B 7B 0C 2B F2 C1 EF 02 89 7D F8 C7 45 FC 00 00 00 00 "
                    + "8D 04 32 85 C0 74 20 3D FF FF FF 3F 77 13 C1 E0 02 50"
            && FrameHandoffCapacityPrefixProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x24
            && FrameHandoffCapacityPrefixProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityPrefixProjection.EntryRawFileOffset)
                == FrameHandoffCapacityPrefixProjection.EntryVirtualAddress
            && FrameHandoffCapacityPrefixProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityPrefixProjection.EndRawFileOffsetExclusive)
                == FrameHandoffCapacityPrefixProjection.EndVirtualAddressExclusive
            && InstructionEvidenceMatches());

        FrameHandoffCapacityPrefixPlan missing =
            FrameHandoffCapacityPrefixProjection.Project(new(0x1000, 0x2000, 3, 5, null));
        check("A missing receiver read stops at the first instruction without projecting frame stores",
            missing.Outcome == FrameHandoffCapacityPrefixOutcome.MissingReceiverCRead
            && missing.ReceiverCReadReached
            && missing.ReceiverCReadAddress == 0x100C
            && !missing.ReceiverCCaptured
            && missing.EdiAfterReceiverCRead is null
            && !missing.EsiSubtractReached
            && missing.EsiAfterSubtract is null
            && !missing.EdiShiftReached
            && missing.EdiAfterShift is null
            && missing.FrameStores.IsEmpty
            && !missing.LeaReached
            && missing.EaxAfterLea is null
            && !missing.TestReached
            && missing.ZeroFlag is null
            && !missing.ZeroBranchReached
            && !missing.CompareReached
            && !missing.ScaleReached
            && !missing.PushReached
            && missing.PushedValue is null
            && missing.BranchTargetRawFileOffset is null
            && missing.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityPrefixProjection.EntryRawFileOffset);

        FrameHandoffCapacityPrefixPlan wrappedMissingReceiver =
            FrameHandoffCapacityPrefixProjection.Project(
                new(uint.MaxValue - 7, 0x2000, 3, 5, null));
        check("The receiver-plus-C read address wraps even when its value is not captured",
            wrappedMissingReceiver.ReceiverCReadReached
            && wrappedMissingReceiver.ReceiverCReadAddress == 4
            && !wrappedMissingReceiver.ReceiverCCaptured
            && wrappedMissingReceiver.EdiAfterReceiverCRead is null
            && wrappedMissingReceiver.FrameStores.IsEmpty
            && wrappedMissingReceiver.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityPrefixProjection.EntryRawFileOffset);

        FrameHandoffCapacityPrefixPlan zero =
            FrameHandoffCapacityPrefixProjection.Project(new(0x3000, 0x5000, 0, 1, 0x1234));
        check("A wrapped subtract-and-sum producing zero takes the JZ route after ordered stores",
            zero.Outcome == FrameHandoffCapacityPrefixOutcome.ZeroSumBranchToRawFileOffsetCA
            && zero.ReceiverCReadReached
            && zero.ReceiverCReadAddress == 0x300C
            && zero.ReceiverCCaptured
            && zero.EdiAfterReceiverCRead == 0x1234
            && zero.EsiAfterSubtract == uint.MaxValue
            && zero.EdiAfterShift == 0x48D
            && zero.FrameStores.SequenceEqual(new[]
            {
                new FrameHandoffCapacityPrefixFrameStore(
                    0x00BE6B99, 0x4FF8, 0x48D),
                new FrameHandoffCapacityPrefixFrameStore(
                    0x00BE6B9C, 0x4FFC, 0)
            })
            && zero.EaxAfterLea == 0
            && zero.ZeroFlag == true
            && zero.ZeroBranchReached
            && zero.ZeroBranchTaken == true
            && !zero.CompareReached
            && zero.AboveGuardBranchTaken is null
            && !zero.ScaleReached
            && !zero.PushReached
            && zero.PushedValue is null
            && zero.BranchTargetRawFileOffset
                == FrameHandoffCapacityPrefixProjection.ZeroBranchTargetRawFileOffset
            && zero.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityPrefixProjection.ZeroBranchTargetRawFileOffset);

        FrameHandoffCapacityPrefixPlan threshold =
            FrameHandoffCapacityPrefixProjection.Project(
                new(0x4000, 0x6000, 0x3FFFFFFF, 0x40000000, 0xFFFFFFFF));
        check("The inclusive unsigned maximum passes the guard and scales with DWORD wraparound",
            threshold.Outcome == FrameHandoffCapacityPrefixOutcome.ScaledPushBeforeCall
            && threshold.EsiAfterSubtract == 0xFFFFFFFF
            && threshold.EdiAfterShift == 0x3FFFFFFF
            && threshold.EaxAfterLea == 0x3FFFFFFF
            && threshold.ZeroFlag == false
            && threshold.ZeroBranchTaken == false
            && threshold.CompareReached
            && threshold.AboveGuardBranchTaken == false
            && threshold.ScaleReached
            && threshold.EaxAfterScale == 0xFFFFFFFC
            && threshold.PushReached
            && threshold.PushedValue == 0xFFFFFFFC
            && threshold.BranchTargetRawFileOffset is null
            && threshold.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityPrefixProjection.EndRawFileOffsetExclusive);

        FrameHandoffCapacityPrefixPlan aboveGuard =
            FrameHandoffCapacityPrefixProjection.Project(
                new(0x5000, 0x7000, 0x40000000, 0x40000000, 8));
        check("A value above the unsigned maximum takes JA without scaling or pushing",
            aboveGuard.Outcome
                == FrameHandoffCapacityPrefixOutcome.AboveGuardBranchToRawFileOffsetBC4
            && aboveGuard.EsiAfterSubtract == 0
            && aboveGuard.EaxAfterLea == 0x40000000
            && aboveGuard.ZeroFlag == false
            && aboveGuard.ZeroBranchTaken == false
            && aboveGuard.CompareReached
            && aboveGuard.AboveGuardBranchTaken == true
            && !aboveGuard.ScaleReached
            && aboveGuard.EaxAfterScale is null
            && !aboveGuard.PushReached
            && aboveGuard.PushedValue is null
            && aboveGuard.BranchTargetRawFileOffset
                == FrameHandoffCapacityPrefixProjection.AboveGuardBranchTargetRawFileOffset
            && aboveGuard.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityPrefixProjection.AboveGuardBranchTargetRawFileOffset);

        FrameHandoffCapacityPrefixPlan wrappedSubAndSum =
            FrameHandoffCapacityPrefixProjection.Project(
                new(0x6000, 0x8000, 3, 5, 0x1000));
        check("The subtract and LEA independently wrap while their sum recovers entry ESI",
            wrappedSubAndSum.EsiAfterSubtract == 0xFFFFFFFE
            && wrappedSubAndSum.EaxAfterLea == 3
            && wrappedSubAndSum.Outcome
                == FrameHandoffCapacityPrefixOutcome.ScaledPushBeforeCall
            && wrappedSubAndSum.EaxAfterScale == 12
            && wrappedSubAndSum.PushedValue == 12);

        FrameHandoffCapacityPrefixPlan wrappedFrame =
            FrameHandoffCapacityPrefixProjection.Project(
                new(0x7000, 4, 8, 0, 0x40));
        check("Both frame destinations use unchecked 32-bit subtraction and preserve store order",
            wrappedFrame.FrameStores.SequenceEqual(new[]
            {
                new FrameHandoffCapacityPrefixFrameStore(
                    0x00BE6B99, 0xFFFFFFFC, 0x10),
                new FrameHandoffCapacityPrefixFrameStore(
                    0x00BE6B9C, 0, 0)
            })
            && wrappedFrame.Outcome
                == FrameHandoffCapacityPrefixOutcome.ScaledPushBeforeCall);

        FrameHandoffCapacityPrefixPlan pushedByteValue =
            FrameHandoffCapacityPrefixProjection.Project(
                new(0x8000, 0x9000, 0x30000001, 0x10000000, 0xFFFFFFFF));
        check("PUSH EAX at raw 0xBE6BB4 receives the scaled DWORD and stops before the call",
            FrameHandoffCapacityPrefixProjection.PushEaxRawFileOffset == 0x00BE6BB4
            && pushedByteValue.EaxAfterLea == 0x30000001
            && pushedByteValue.EaxAfterScale == 0xC0000004
            && pushedByteValue.PushReached
            && pushedByteValue.PushedValue == 0xC0000004
            && pushedByteValue.StopAtRawFileOffsetExclusive == 0x00BE6BB5);
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
            (0x00BE6B91, 0x00FE7791, "8B 7B 0C", null, null),
            (0x00BE6B94, 0x00FE7794, "2B F2", null, null),
            (0x00BE6B96, 0x00FE7796, "C1 EF 02", null, null),
            (0x00BE6B99, 0x00FE7799, "89 7D F8", null, null),
            (0x00BE6B9C, 0x00FE779C, "C7 45 FC 00 00 00 00", null, null),
            (0x00BE6BA3, 0x00FE77A3, "8D 04 32", null, null),
            (0x00BE6BA6, 0x00FE77A6, "85 C0", null, null),
            (0x00BE6BA8, 0x00FE77A8, "74 20", 0x00BE6BCA, 0x00FE77CA),
            (0x00BE6BAA, 0x00FE77AA, "3D FF FF FF 3F", null, null),
            (0x00BE6BAF, 0x00FE77AF, "77 13", 0x00BE6BC4, 0x00FE77C4),
            (0x00BE6BB1, 0x00FE77B1, "C1 E0 02", null, null),
            (0x00BE6BB4, 0x00FE77B4, "50", null, null)
        };
        uint nextRawOffset = FrameHandoffCapacityPrefixProjection.EntryRawFileOffset;
        string instructionBytes = string.Empty;

        foreach (FrameHandoffCapacityPrefixInstructionEvidence instruction
            in FrameHandoffCapacityPrefixProjection.Instructions)
        {
            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacityPrefixProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            if (instruction.BranchTargetRawFileOffset is uint targetRaw
                && instruction.BranchTargetVirtualAddress
                    != FrameHandoffCapacityPrefixProjection.RawFileOffsetToVirtualAddress(
                        targetRaw))
            {
                return false;
            }

            nextRawOffset += (uint)(bytes.Length / 2);
            instructionBytes += bytes;
        }

        return nextRawOffset == FrameHandoffCapacityPrefixProjection.EndRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(FrameHandoffCapacityPrefixProjection.CapturedWindowBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            && FrameHandoffCapacityPrefixProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.VirtualAddress,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }
}
