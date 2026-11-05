using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeIndexedRejectGateProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw bytes, flat IDA offsets, VA mapping, and relative branch target match",
            CapacityFinalCalleeLargeIndexedRejectGateProjection.EntryRawFileOffset
                == 0x012C9263
            && CapacityFinalCalleeLargeIndexedRejectGateProjection.EntryVirtualAddress
                == 0x016C9E63
            && CapacityFinalCalleeLargeIndexedRejectGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C926B
            && CapacityFinalCalleeLargeIndexedRejectGateProjection.BranchTargetRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeLargeIndexedRejectGateProjection.FallthroughRawFileOffset
                == 0x012C926B
            && CapacityFinalCalleeLargeIndexedRejectGateProjection.RawFileOffsetToVirtualAddress(
                0x012C9263) == 0x016C9E63
            && CapacityFinalCalleeLargeIndexedRejectGateProjection.RawFileOffsetToVirtualAddress(
                0x012C8E76) == 0x016C9A76
            && Normalize(CapacityFinalCalleeLargeIndexedRejectGateProjection.CapturedWindowBytes)
                == "3BCB0F820BFCFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeIndexedRejectGatePlan missingECX =
            CapacityFinalCalleeLargeIndexedRejectGateProjection.Project(new(
                EntryECX: null,
                EntryEBX: 0x12345678));
        check("Missing ECX stops at CMP while retaining known EBX",
            missingECX.Outcome
                == CapacityFinalCalleeLargeIndexedRejectGateOutcome.MissingEntryECX
            && !missingECX.EntryECXCaptured
            && missingECX.EntryEBXCaptured
            && missingECX.CompareLeft is null
            && missingECX.CompareRight == 0x12345678
            && !missingECX.CompareReached
            && missingECX.CompareResult is null
            && missingECX.CompareFlags is null
            && !missingECX.BranchReached
            && missingECX.ReachedInstructions.IsEmpty
            && missingECX.Events.IsEmpty
            && !missingECX.ExactCleanExitAdmitted
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C9263);

        CapacityFinalCalleeLargeIndexedRejectGatePlan missingEBX =
            CapacityFinalCalleeLargeIndexedRejectGateProjection.Project(new(
                EntryECX: 0x89ABCDEF,
                EntryEBX: null));
        check("Missing EBX stops at CMP while retaining known ECX",
            missingEBX.Outcome
                == CapacityFinalCalleeLargeIndexedRejectGateOutcome.MissingEntryEBX
            && missingEBX.EntryECXCaptured
            && !missingEBX.EntryEBXCaptured
            && missingEBX.CompareLeft == 0x89ABCDEF
            && missingEBX.CompareRight is null
            && !missingEBX.CompareReached
            && !missingEBX.BranchReached
            && missingEBX.StopAtRawFileOffsetExclusive == 0x012C9263);

        CapacityFinalCalleeLargeIndexedRejectGatePlan missingBoth =
            CapacityFinalCalleeLargeIndexedRejectGateProjection.Project(new(
                EntryECX: null,
                EntryEBX: null));
        check("Missing both entry operands stops without a partial comparison",
            missingBoth.Outcome
                == CapacityFinalCalleeLargeIndexedRejectGateOutcome.MissingEntryECXAndEBX
            && !missingBoth.EntryECXCaptured
            && !missingBoth.EntryEBXCaptured
            && missingBoth.CompareLeft is null
            && missingBoth.CompareRight is null
            && !missingBoth.CompareReached
            && !missingBoth.BranchReached
            && missingBoth.ReachedInstructions.IsEmpty
            && !missingBoth.ExactCleanExitAdmitted
            && missingBoth.StopAtRawFileOffsetExclusive == 0x012C9263);

        CapacityFinalCalleeLargeIndexedRejectGatePlan zero =
            CapacityFinalCalleeLargeIndexedRejectGateProjection.Project(new(
                EntryECX: 0,
                EntryEBX: 0));
        check("Zero equals zero and falls through with the CMP zero/parity flags",
            zero.Outcome
                == CapacityFinalCalleeLargeIndexedRejectGateOutcome
                    .UnsignedNotBelowToFallthrough
            && zero.CompareLeft == 0
            && zero.CompareRight == 0
            && zero.CompareResult == 0
            && zero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.BranchReached
            && zero.BranchTaken == false
            && zero.BranchSelectedDestinationRawFileOffset == 0x012C926B
            && zero.ReachedInstructions.SequenceEqual(new uint[]
                { 0x012C9263, 0x012C9265 })
            && zero.Events.Select(item => item.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeIndexedRejectGateEventKind.Compare,
                CapacityFinalCalleeLargeIndexedRejectGateEventKind.Branch
            })
            && zero.ExactCleanExitAdmitted
            && zero.StopAtRawFileOffsetExclusive == 0x012C926B);

        CapacityFinalCalleeLargeIndexedRejectGatePlan equalNonzero =
            CapacityFinalCalleeLargeIndexedRejectGateProjection.Project(new(
                EntryECX: 0x76543210,
                EntryEBX: 0x76543210));
        check("Equal nonzero operands produce zero without taking unsigned JB",
            equalNonzero.CompareResult == 0
            && equalNonzero.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalNonzero.BranchTaken == false
            && equalNonzero.BranchSelectedDestinationRawFileOffset == 0x012C926B
            && equalNonzero.StopAtRawFileOffsetExclusive == 0x012C926B);

        CapacityFinalCalleeLargeIndexedRejectGatePlan borrow =
            CapacityFinalCalleeLargeIndexedRejectGateProjection.Project(new(
                EntryECX: 0,
                EntryEBX: 1));
        check("Unsigned borrow takes JB and preserves the full subtraction flags",
            borrow.CompareResult == 0xFFFFFFFF
            && borrow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && borrow.BranchTaken == true
            && borrow.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && borrow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && borrow.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeIndexedRejectGatePlan signedOverflow =
            CapacityFinalCalleeLargeIndexedRejectGateProjection.Project(new(
                EntryECX: 0x80000000,
                EntryEBX: 1));
        check("Signed overflow does not substitute for the unsigned JB condition",
            signedOverflow.CompareResult == 0x7FFFFFFF
            && signedOverflow.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedOverflow.BranchTaken == false
            && signedOverflow.BranchSelectedDestinationRawFileOffset == 0x012C926B);

        CapacityFinalCalleeLargeIndexedRejectGatePlan auxiliaryCarry =
            CapacityFinalCalleeLargeIndexedRejectGateProjection.Project(new(
                EntryECX: 0x10,
                EntryEBX: 1));
        check("Auxiliary carry reflects the low-nibble subtraction",
            auxiliaryCarry.CompareResult == 0x0F
            && auxiliaryCarry.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && auxiliaryCarry.BranchTaken == false);

        CapacityFinalCalleeLargeIndexedRejectGatePlan oddParity =
            CapacityFinalCalleeLargeIndexedRejectGateProjection.Project(new(
                EntryECX: 2,
                EntryEBX: 1));
        check("Parity is computed from the low result byte",
            oddParity.CompareResult == 1
            && oddParity.CompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false
            });

        CapacityFinalCalleeLargeIndexedRejectGatePlan maximum =
            CapacityFinalCalleeLargeIndexedRejectGateProjection.Project(new(
                EntryECX: uint.MaxValue,
                EntryEBX: 0));
        check("The maximum DWORD operand retains unsigned, sign, and parity distinctions",
            maximum.CompareLeft == 0xFFFFFFFF
            && maximum.CompareRight == 0
            && maximum.CompareResult == 0xFFFFFFFF
            && maximum.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && maximum.BranchTaken == false
            && maximum.BranchSelectedDestinationRawFileOffset == 0x012C926B);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C9263, "3B CB", null, null),
            (0x012C9265, "0F 82 0B FC FF FF", 0x012C8E76, 0x016C9A76)
        };
        uint nextRawOffset = 0x012C9263;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeIndexedRejectGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeIndexedRejectGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress != unchecked(instruction.RawFileOffset + 0x00400C00)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != unchecked(targetRaw + 0x00400C00)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeLargeIndexedRejectGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset == 0x012C926B
            && bytes == "3BCB0F820BFCFFFF"
            && CapacityFinalCalleeLargeIndexedRejectGateProjection.Instructions
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
