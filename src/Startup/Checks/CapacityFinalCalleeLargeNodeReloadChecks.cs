using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeNodeReloadChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The six-byte instruction window, raw/VA anchors, and short-jump target match",
            CapacityFinalCalleeLargeNodeReloadProjection.EntryRawFileOffset
                == 0x012C9205
            && CapacityFinalCalleeLargeNodeReloadProjection.EntryVirtualAddress
                == 0x016C9E05
            && CapacityFinalCalleeLargeNodeReloadProjection.BoundaryRawFileOffsetExclusive
                == 0x012C920B
            && CapacityFinalCalleeLargeNodeReloadProjection.JumpTargetRawFileOffset
                == 0x012C91F3
            && CapacityFinalCalleeLargeNodeReloadProjection.JumpTargetVirtualAddress
                == 0x016C9DF3
            && CapacityFinalCalleeLargeNodeReloadProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeNodeReloadProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeNodeReloadProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeLargeNodeReloadProjection.CapturedWindowBytes)
                == "8B018BD1EBE8"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeNodeReloadPlan missingECX =
            CapacityFinalCalleeLargeNodeReloadProjection.Project(new(
                EntryECX: null,
                FreshNodeDword: 0xAABBCCDD));
        check("Missing entry ECX prevents the read and all later MOV/JMP instructions",
            missingECX.Outcome
                == CapacityFinalCalleeLargeNodeReloadOutcome.MissingEntryECX
            && !missingECX.FreshNodeReadReached
            && missingECX.Reads.IsEmpty
            && !missingECX.EAXMoveReached
            && !missingECX.EDXMoveReached
            && !missingECX.JumpReached
            && missingECX.UnreachedFreshNodeCaptureProvided
            && !missingECX.ExactCleanCompletion);

        CapacityFinalCalleeLargeNodeReloadPlan missingFreshNode =
            CapacityFinalCalleeLargeNodeReloadProjection.Project(new(
                EntryECX: 0x12345678,
                FreshNodeDword: null));
        check("A missing fresh DWORD records one read and prevents later MOV/JMP instructions",
            missingFreshNode.Outcome
                == CapacityFinalCalleeLargeNodeReloadOutcome.MissingFreshNodeDword
            && missingFreshNode.FreshNodeReadReached
            && missingFreshNode.FreshNodeAddress == 0x12345678
            && missingFreshNode.Reads.Length == 1
            && missingFreshNode.Reads[0] is
            {
                RawFileOffset: 0x012C9205,
                Address: 0x12345678,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && !missingFreshNode.EAXMoveReached
            && !missingFreshNode.EDXMoveReached
            && !missingFreshNode.JumpReached
            && !missingFreshNode.ExactCleanCompletion);

        CapacityFinalCalleeLargeNodeReloadPlan zeroValue =
            CapacityFinalCalleeLargeNodeReloadProjection.Project(new(
                EntryECX: 0x12345678,
                FreshNodeDword: 0));
        check("Zero is a valid fresh node value and EDX retains the original ECX address",
            zeroValue.Outcome == CapacityFinalCalleeLargeNodeReloadOutcome.Completed
            && zeroValue.FreshNodeAddress == 0x12345678
            && zeroValue.FreshNodeCaptured
            && zeroValue.FreshNodeDword == 0
            && zeroValue.EAXCaptured
            && zeroValue.EAX == 0
            && zeroValue.EDXCaptured
            && zeroValue.EDX == 0x12345678
            && zeroValue.EDX != zeroValue.EAX
            && zeroValue.Reads.Length == 1
            && zeroValue.JumpReached
            && zeroValue.JumpTargetRawFileOffset == 0x012C91F3
            && zeroValue.JumpTargetVirtualAddress == 0x016C9DF3
            && zeroValue.BoundedStopRawFileOffset == 0x012C91F3
            && !zeroValue.MemoryWriteReached
            && zeroValue.IncomingFlagsPreserved
            && zeroValue.ExactCleanCompletion);

        CapacityFinalCalleeLargeNodeReloadPlan maximumValue =
            CapacityFinalCalleeLargeNodeReloadProjection.Project(new(
                EntryECX: 0xFFFFFFFF,
                FreshNodeDword: 0xFFFFFFFF));
        check("The maximum DWORD is captured without confusing fresh EAX with original-address EDX",
            maximumValue.Outcome == CapacityFinalCalleeLargeNodeReloadOutcome.Completed
            && maximumValue.FreshNodeAddress == 0xFFFFFFFF
            && maximumValue.EAX == 0xFFFFFFFF
            && maximumValue.EDX == 0xFFFFFFFF
            && maximumValue.FreshNodeDword == maximumValue.EAX
            && maximumValue.Reads.Length == 1
            && maximumValue.JumpReached
            && maximumValue.IncomingFlagsPreserved
            && maximumValue.ExactCleanCompletion);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C9205, "8B 01", null, null),
            (0x012C9207, "8B D1", null, null),
            (0x012C9209, "EB E8", 0x012C91F3, 0x016C9DF3)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeNodeReloadProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeNodeReloadInstructionEvidence instruction
            in CapacityFinalCalleeLargeNodeReloadProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeNodeReloadProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeNodeReloadProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeLargeNodeReloadProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeNodeReloadProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeNodeReloadProjection.Instructions
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
