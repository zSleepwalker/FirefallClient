using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeIndexedFieldGateProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw, flat IDA, VA mapping, bytes, and exact branch target match",
            CapacityFinalCalleeLargeIndexedFieldGateProjection.EntryRawFileOffset
                == 0x012C926B
            && CapacityFinalCalleeLargeIndexedFieldGateProjection.EntryVirtualAddress
                == 0x016C9E6B
            && CapacityFinalCalleeLargeIndexedFieldGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9270
            && CapacityFinalCalleeLargeIndexedFieldGateProjection.JnzDestinationRawFileOffset
                == 0x012C9275
            && CapacityFinalCalleeLargeIndexedFieldGateProjection.RawFileOffsetToVirtualAddress(
                0x012C926B) == 0x016C9E6B
            && CapacityFinalCalleeLargeIndexedFieldGateProjection.RawFileOffsetToVirtualAddress(
                0x012C9270) == 0x016C9E70
            && CapacityFinalCalleeLargeIndexedFieldGateProjection.RawFileOffsetToVirtualAddress(
                0x012C9275) == 0x016C9E75
            && Normalize(CapacityFinalCalleeLargeIndexedFieldGateProjection.CapturedWindowBytes)
                == "3951107505"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeIndexedFieldGatePlan allMissing =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.Project(new(
                EntryECX: null,
                EntryEDX: null,
                FreshECXPlus10: null));
        check("All missing captures stop at CMP without resolving or consuming memory",
            allMissing.Outcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingEntryECX
            && allMissing.CompareInstructionReached
            && !allMissing.EntryECXCaptured
            && !allMissing.FreshECXPlus10ReadReached
            && allMissing.FreshECXPlus10Address is null
            && !allMissing.FreshECXPlus10Captured
            && !allMissing.EntryEDXCaptured
            && !allMissing.CompareReached
            && !allMissing.BranchReached
            && allMissing.Reads.IsEmpty
            && allMissing.ReachedInstructions.SequenceEqual(new[] { 0x012C926Bu })
            && !allMissing.UnreachedFreshECXPlus10CaptureProvided
            && !allMissing.UnreachedEntryEDXCaptureProvided
            && !allMissing.ExactCleanCompletion
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C926B);

        CapacityFinalCalleeLargeIndexedFieldGatePlan missingECX =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.Project(new(
                EntryECX: null,
                EntryEDX: 0x22222222,
                FreshECXPlus10: 0x11111111));
        check("Missing ECX retains the independent EDX operand without consuming fresh memory",
            missingECX.Outcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingEntryECX
            && !missingECX.EntryECXCaptured
            && !missingECX.FreshECXPlus10ReadReached
            && missingECX.Reads.IsEmpty
            && missingECX.EntryEDXCaptured
            && missingECX.CompareRight == 0x22222222
            && !missingECX.CompareReached
            && missingECX.UnreachedFreshECXPlus10CaptureProvided
            && !missingECX.UnreachedEntryEDXCaptureProvided
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C926B);

        CapacityFinalCalleeLargeIndexedFieldGatePlan missingEDX =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.Project(new(
                EntryECX: 0x200,
                EntryEDX: null,
                FreshECXPlus10: 0x12345678));
        check("Known ECX retains the fresh memory address and value even when EDX is missing",
            missingEDX.Outcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingEntryEDX
            && missingEDX.EntryECXCaptured
            && missingEDX.FreshECXPlus10ReadReached
            && missingEDX.FreshECXPlus10Address == 0x210
            && missingEDX.FreshECXPlus10Captured
            && missingEDX.Reads.Length == 1
            && missingEDX.Reads[0] is
            {
                RawFileOffset: 0x012C926B,
                Address: 0x210,
                AddressCaptured: true,
                ValueCaptured: true,
                Value: 0x12345678
            }
            && !missingEDX.CompareReached
            && missingEDX.CompareLeft == 0x12345678
            && !missingEDX.BranchReached
            && missingEDX.UnreachedEntryEDXCaptureProvided == false
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C926B);

        CapacityFinalCalleeLargeIndexedFieldGatePlan missingFresh =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.Project(new(
                EntryECX: 0x200,
                EntryEDX: 0x12345678,
                FreshECXPlus10: null));
        check("Missing fresh memory retains its known address and stops at CMP",
            missingFresh.Outcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingFreshECXPlus10
            && missingFresh.FreshECXPlus10Address == 0x210
            && missingFresh.Reads.Length == 1
            && missingFresh.Reads[0] is
            {
                Address: 0x210,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && !missingFresh.FreshECXPlus10Captured
            && missingFresh.EntryEDXCaptured
            && missingFresh.CompareRight == 0x12345678
            && !missingFresh.CompareReached
            && !missingFresh.UnreachedEntryEDXCaptureProvided
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C926B);

        CapacityFinalCalleeLargeIndexedFieldGatePlan missingFreshAndEDX =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.Project(new(
                EntryECX: 0x200,
                EntryEDX: null,
                FreshECXPlus10: null));
        check("Missing field data and EDX remain distinct at the same CMP",
            missingFreshAndEDX.Outcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome
                    .MissingFreshECXPlus10AndEntryEDX
            && missingFreshAndEDX.FreshECXPlus10Address == 0x210
            && missingFreshAndEDX.Reads.Length == 1
            && !missingFreshAndEDX.FreshECXPlus10Captured
            && !missingFreshAndEDX.EntryEDXCaptured
            && !missingFreshAndEDX.CompareReached
            && missingFreshAndEDX.StopAtRawFileOffsetExclusive == 0x012C926B);

        CapacityFinalCalleeLargeIndexedFieldGatePlan wrappedECX =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.Project(new(
                EntryECX: 0xFFFFFFF5,
                EntryEDX: 0xAABBCCDD,
                FreshECXPlus10: 0xAABBCCDD));
        check("ECX+10 wraps as an unsigned 32-bit effective address",
            wrappedECX.FreshECXPlus10Address == 5
            && wrappedECX.Reads[0].Address == 5
            && wrappedECX.CompareLeft == 0xAABBCCDD
            && wrappedECX.CompareRight == 0xAABBCCDD
            && wrappedECX.CompareFlags?.Zero == true
            && wrappedECX.BranchTaken == false
            && wrappedECX.BranchSelectedDestinationRawFileOffset == 0x012C9270);

        CapacityFinalCalleeLargeIndexedFieldGatePlan zero =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.Project(new(
                EntryECX: 0,
                EntryEDX: 0,
                FreshECXPlus10: 0));
        check("Captured zero is distinct from unknown and takes the equal fallthrough",
            zero.FreshECXPlus10Captured
            && zero.CompareLeft == 0
            && zero.CompareResult == 0
            && zero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.BranchReached
            && zero.BranchTaken == false
            && zero.BranchSelectedDestinationRawFileOffset == 0x012C9270
            && zero.ExactCleanCompletion
            && zero.StopAtRawFileOffsetExclusive == 0x012C9270
            && !zero.MemoryWriteReached);

        CapacityFinalCalleeLargeIndexedFieldGatePlan maximum =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.Project(new(
                EntryECX: 0,
                EntryEDX: uint.MaxValue,
                FreshECXPlus10: uint.MaxValue));
        check("Equal maximum DWORD operands produce zero without being treated as missing",
            maximum.CompareLeft == uint.MaxValue
            && maximum.CompareRight == uint.MaxValue
            && maximum.CompareResult == 0
            && maximum.CompareFlags?.Zero == true
            && maximum.CompareFlags?.Carry == false
            && maximum.BranchTaken == false
            && maximum.StopAtRawFileOffsetExclusive == 0x012C9270);

        CapacityFinalCalleeLargeIndexedFieldGatePlan borrow =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.Project(new(
                EntryECX: 0,
                EntryEDX: 2,
                FreshECXPlus10: 1));
        check("Unsigned subtraction borrow sets carry and branches on nonzero",
            borrow.CompareResult == uint.MaxValue
            && borrow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && borrow.BranchTaken == true
            && borrow.BranchSelectedDestinationRawFileOffset == 0x012C9275
            && borrow.StopAtRawFileOffsetExclusive == 0x012C9275);

        CapacityFinalCalleeLargeIndexedFieldGatePlan overflow =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.Project(new(
                EntryECX: 0,
                EntryEDX: 1,
                FreshECXPlus10: 0x80000000));
        check("CMP exposes all six subtraction flags for signed overflow",
            overflow.CompareResult == 0x7FFFFFFF
            && overflow.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && overflow.BranchTaken == true
            && overflow.StopAtRawFileOffsetExclusive == 0x012C9275);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C926B, "39 51 10", null, null),
            (0x012C926E, "75 05", 0x012C9275, 0x016C9E75)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeIndexedFieldGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeIndexedFieldGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeIndexedFieldGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeIndexedFieldGateProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeIndexedFieldGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeLargeIndexedFieldGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeIndexedFieldGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeIndexedFieldGateProjection.Instructions
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
