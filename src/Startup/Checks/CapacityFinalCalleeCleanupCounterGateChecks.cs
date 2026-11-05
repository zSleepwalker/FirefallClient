using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCleanupCounterGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window, addresses, operands, branch, and direct-call evidence match",
            CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset
                == 0x012C942F
            && CapacityFinalCalleeCleanupCounterGateProjection.EntryFlatIdaAddress
                == 0x012C942F
            && CapacityFinalCalleeCleanupCounterGateProjection.EntryVirtualAddress
                == 0x016CA02F
            && CapacityFinalCalleeCleanupCounterGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeCleanupCounterGateProjection.EntryVirtualAddress
            && CapacityFinalCalleeCleanupCounterGateProjection.CounterVirtualAddress
                == 0x01EDADE8
            && CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathEcxValue
                == 0x01EDADC8
            && CapacityFinalCalleeCleanupCounterGateProjection.JnzRawFileOffset
                == 0x012C9435
            && CapacityFinalCalleeCleanupCounterGateProjection.ZeroPathRawFileOffset
                == 0x012C9437
            && CapacityFinalCalleeCleanupCounterGateProjection.NonzeroBranchTargetRawFileOffset
                == 0x012C9441
            && CapacityFinalCalleeCleanupCounterGateProjection.CalculateJnzTargetRawFileOffset()
                == 0x012C9441
            && CapacityFinalCalleeCleanupCounterGateProjection.CallRawFileOffset
                == 0x012C943C
            && CapacityFinalCalleeCleanupCounterGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C943C
            && CapacityFinalCalleeCleanupCounterGateProjection.DirectCallTargetRawFileOffset
                == 0x012C78A0
            && CapacityFinalCalleeCleanupCounterGateProjection.DirectCallTargetVirtualAddress
                == 0x016C84A0
            && Normalize(
                CapacityFinalCalleeCleanupCounterGateProjection.CapturedWindowBytes)
                == "FF0DE8ADED01750AB9C8ADED01E85FE4FFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCleanupCounterGatePlan invalidEntry =
            CapacityFinalCalleeCleanupCounterGateProjection.Project(
                new(
                    EntryValidated: false,
                    FreshCounterDword: 5,
                    IncomingCarryFlag: true));
        check("An unvalidated entry stops before the counter read and rejects later captures",
            invalidEntry.Outcome
                == CapacityFinalCalleeCleanupCounterGateOutcome.EntryNotValidated
            && !invalidEntry.CounterReadReached
            && !invalidEntry.FreshCounterDwordCaptured
            && !invalidEntry.CounterWriteReached
            && invalidEntry.CounterAccesses.IsEmpty
            && invalidEntry.FlagsAfterDecrement is null
            && !invalidEntry.JnzReached
            && invalidEntry.JnzTaken is null
            && !invalidEntry.MovEcxReached
            && invalidEntry.EcxValue is null
            && !invalidEntry.CallReached
            && !invalidEntry.CalleeEntered
            && invalidEntry.UnreachedFreshCounterDwordCaptureProvided
            && invalidEntry.UnreachedIncomingCarryFlagCaptureProvided
            && !invalidEntry.ExactBoundedGateCompletion
            && invalidEntry.ReachedInstructions.IsEmpty
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCleanupCounterGatePlan missingCounter =
            CapacityFinalCalleeCleanupCounterGateProjection.Project(
                new(
                    EntryValidated: true,
                    FreshCounterDword: null,
                    IncomingCarryFlag: false));
        check("A missing fresh counter attempts only the known-address read and performs no store",
            missingCounter.Outcome
                == CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword
            && missingCounter.CounterReadReached
            && !missingCounter.FreshCounterDwordCaptured
            && !missingCounter.CounterWriteReached
            && missingCounter.CounterAccesses.Length == 1
            && missingCounter.CounterAccesses[0].Kind
                == CapacityFinalCalleeCleanupCounterGateAccessKind.Read
            && missingCounter.CounterAccesses[0].Address == 0x01EDADE8
            && !missingCounter.CounterAccesses[0].ValueCaptured
            && missingCounter.CounterAccesses[0].Value is null
            && missingCounter.FlagsAfterDecrement is null
            && !missingCounter.JnzReached
            && !missingCounter.UnreachedIncomingCarryFlagCaptureProvided
            && missingCounter.CarryAtStop == false
            && !missingCounter.CallReached
            && !missingCounter.CalleeEntered
            && !missingCounter.ExactBoundedGateCompletion
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCleanupCounterGatePlan underflow =
            CapacityFinalCalleeCleanupCounterGateProjection.Project(
                new(
                    EntryValidated: true,
                    FreshCounterDword: 0,
                    IncomingCarryFlag: false));
        check("Decrementing zero wraps to uint.MaxValue, preserves CF, and takes JNZ",
            underflow.Outcome
                == CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero
            && underflow.CounterBefore == 0
            && underflow.CounterAfter == uint.MaxValue
            && underflow.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && underflow.JnzReached
            && underflow.JnzTaken == true
            && underflow.SelectedDestinationRawFileOffset == 0x012C9441
            && underflow.SelectedDestinationVirtualAddress == 0x016CA041
            && !underflow.MovEcxReached
            && underflow.EcxValue is null
            && underflow.ReachedInstructions.SequenceEqual(
                new uint[] { 0x012C942F, 0x012C9435 })
            && !underflow.CallReached
            && !underflow.CalleeEntered
            && underflow.ExactBoundedGateCompletion
            && underflow.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCleanupCounterGatePlan oneToZero =
            CapacityFinalCalleeCleanupCounterGateProjection.Project(
                new(
                    EntryValidated: true,
                    FreshCounterDword: 1,
                    IncomingCarryFlag: true));
        check("Decrementing one to zero preserves CF and sets up ECX with the zero-path immediate",
            oneToZero.Outcome
                == CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero
            && oneToZero.CounterBefore == 1
            && oneToZero.CounterAfter == 0
            && oneToZero.FlagsAfterDecrement is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && oneToZero.JnzReached
            && oneToZero.JnzTaken == false
            && oneToZero.SelectedDestinationRawFileOffset == 0x012C9437
            && oneToZero.SelectedDestinationVirtualAddress == 0x016CA037
            && oneToZero.MovEcxReached
            && oneToZero.EcxValue == 0x01EDADC8
            && oneToZero.ReachedInstructions.SequenceEqual(
                new uint[] { 0x012C942F, 0x012C9435, 0x012C9437 })
            && !oneToZero.CallReached
            && !oneToZero.CalleeEntered
            && oneToZero.ExactBoundedGateCompletion
            && oneToZero.StopAtRawFileOffsetExclusive == 0x012C943C);

        check("The counter read precedes its committed decrement write",
            oneToZero.CounterReadReached
            && oneToZero.FreshCounterDwordCaptured
            && oneToZero.CounterWriteReached
            && oneToZero.CounterAccesses.Select(access => access.Kind).SequenceEqual(
                new[]
                {
                    CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                    CapacityFinalCalleeCleanupCounterGateAccessKind.Write
                })
            && oneToZero.CounterAccesses[0].RawFileOffset == 0x012C942F
            && oneToZero.CounterAccesses[0].Address == 0x01EDADE8
            && oneToZero.CounterAccesses[0].ValueCaptured
            && oneToZero.CounterAccesses[0].Value == 1
            && oneToZero.CounterAccesses[1].RawFileOffset == 0x012C942F
            && oneToZero.CounterAccesses[1].Address == 0x01EDADE8
            && oneToZero.CounterAccesses[1].ValueCaptured
            && oneToZero.CounterAccesses[1].Value == 0);

        CapacityFinalCalleeCleanupCounterGatePlan overflow =
            CapacityFinalCalleeCleanupCounterGateProjection.Project(
                new(
                    EntryValidated: true,
                    FreshCounterDword: 0x80000000,
                    IncomingCarryFlag: null));
        check("The sign-bit transition sets DEC overflow while unknown incoming CF stays unknown",
            overflow.CounterAfter == 0x7FFFFFFF
            && overflow.FlagsAfterDecrement is
            {
                Carry: null,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && overflow.JnzTaken == true
            && overflow.SelectedDestinationRawFileOffset == 0x012C9441
            && !overflow.MovEcxReached
            && overflow.EcxValue is null
            && overflow.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCleanupCounterGatePlan oddParity =
            CapacityFinalCalleeCleanupCounterGateProjection.Project(
                new(
                    EntryValidated: true,
                    FreshCounterDword: 3,
                    IncomingCarryFlag: null));
        check("DEC parity uses the low byte and detects an odd population count",
            oddParity.CounterAfter == 2
            && oddParity.FlagsAfterDecrement is
            {
                Carry: null,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            });

        CapacityFinalCalleeCleanupCounterGatePlan auxiliaryBorrow =
            CapacityFinalCalleeCleanupCounterGateProjection.Project(
                new(
                    EntryValidated: true,
                    FreshCounterDword: 0x10,
                    IncomingCarryFlag: true));
        check("DEC auxiliary carry records a borrow from bit four independently of CF",
            auxiliaryBorrow.CounterAfter == 0x0F
            && auxiliaryBorrow.FlagsAfterDecrement is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: false
            });
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? Operand,
            uint? Immediate,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual) []
        {
            (0x012C942F, "FF 0D E8 AD ED 01", 0x01EDADE8, null, null, null),
            (0x012C9435, "75 0A", null, null, 0x012C9441, 0x016CA041),
            (0x012C9437, "B9 C8 AD ED 01", null, 0x01EDADC8, null, null),
            (0x012C943C, "E8 5F E4 FF FF", null, null, 0x012C78A0, 0x016C84A0)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCleanupCounterGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCleanupCounterGateInstructionEvidence instruction
            in CapacityFinalCalleeCleanupCounterGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCleanupCounterGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCleanupCounterGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCleanupCounterGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCleanupCounterGateProjection
                    .NonzeroBranchTargetRawFileOffset
            && bytes == Normalize(
                CapacityFinalCalleeCleanupCounterGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCleanupCounterGateProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.PeVirtualAddressOperand,
                        instruction.ImmediateValue,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Where(Uri.IsHexDigit)).ToUpperInvariant();
}
