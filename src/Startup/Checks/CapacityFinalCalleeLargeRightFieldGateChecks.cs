using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup.CapacityFinalCalleeLargeRightFieldGateProjection;

internal static class CapacityFinalCalleeLargeRightFieldGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw, flat IDA, VA, instruction, and branch anchors match",
            InstructionEvidenceMatches());

        CapacityFinalCalleeLargeRightFieldGatePlan missingEDX =
            Projection.Project(new(
                EntryEDX: null,
                FreshEDXPlus14: 0x12345678));
        check("Missing EDX leaves the supplied read value explicitly unreached",
            missingEDX.Outcome
                == CapacityFinalCalleeLargeRightFieldGateOutcome.MissingEntryEDXForFreshRead
            && missingEDX.FreshReadInstructionReached
            && !missingEDX.FreshReadAttempted
            && !missingEDX.FreshReadAddressCaptured
            && missingEDX.FreshReadAddress is null
            && !missingEDX.FreshReadValueCaptured
            && missingEDX.UnreachedFreshEDXPlus14CaptureProvided
            && missingEDX.ReadAttempts.IsEmpty
            && missingEDX.ReachedInstructions.SequenceEqual(new uint[] { 0x012C92A4 })
            && !missingEDX.TestReached
            && !missingEDX.JzReached
            && missingEDX.FlagsAtStop is null
            && !missingEDX.ExactCleanCompletion
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeRightFieldGatePlan missingFreshValue =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: null));
        check("A known effective address records the missing-value read attempt",
            missingFreshValue.Outcome
                == CapacityFinalCalleeLargeRightFieldGateOutcome.MissingFreshEDXPlus14
            && missingFreshValue.FreshReadInstructionReached
            && missingFreshValue.FreshReadAttempted
            && missingFreshValue.FreshReadAddressCaptured
            && missingFreshValue.FreshReadAddress == 0x214
            && !missingFreshValue.FreshReadValueCaptured
            && missingFreshValue.ReadAttempts.Length == 1
            && missingFreshValue.ReadAttempts[0] is
            {
                RawFileOffset: 0x012C92A4,
                Address: 0x214,
                AddressCaptured: true,
                Value: null,
                ValueCaptured: false,
                Attempted: true
            }
            && !missingFreshValue.TestReached
            && !missingFreshValue.JzReached
            && missingFreshValue.FlagsAtStop is null
            && !missingFreshValue.ExactCleanCompletion
            && missingFreshValue.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeRightFieldGatePlan zero =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0));
        check("TEST of zero sets its defined flags and selects 92BD without entering it",
            zero.Outcome == CapacityFinalCalleeLargeRightFieldGateOutcome.ZeroTo92BD
            && zero.FreshECX == 0
            && zero.TestResult == 0
            && zero.TestFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.JzReached
            && zero.JzTaken == true
            && zero.JzSelectedDestinationRawFileOffset == 0x012C92BD
            && !zero.JzTargetEntered
            && !zero.JzFallthroughEntered
            && zero.ReachedInstructions.SequenceEqual(
                new uint[] { 0x012C92A4, 0x012C92A7, 0x012C92A9 })
            && zero.ReadAttempts.Length == 1
            && zero.ExactCleanCompletion
            && zero.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeRightFieldGatePlan nonzero =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0x80000001));
        check("TEST of nonzero selects the unexecuted 92AB fallthrough with exact flags",
            nonzero.Outcome == CapacityFinalCalleeLargeRightFieldGateOutcome.NonzeroTo92AB
            && nonzero.FreshECX == 0x80000001
            && nonzero.TestResult == 0x80000001
            && nonzero.TestFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonzero.JzReached
            && nonzero.JzTaken == false
            && nonzero.JzSelectedDestinationRawFileOffset == 0x012C92AB
            && !nonzero.JzTargetEntered
            && !nonzero.JzFallthroughEntered
            && nonzero.ExactCleanCompletion
            && nonzero.StopAtRawFileOffsetExclusive == 0x012C92AB);

        CapacityFinalCalleeLargeRightFieldGatePlan wrapped =
            Projection.Project(new(
                EntryEDX: 0xFFFFFFEC,
                FreshEDXPlus14: 1));
        check("Fresh right-field address derivation wraps as an unsigned DWORD",
            wrapped.FreshReadAddress == 0
            && Projection.DeriveFreshReadAddress(0xFFFFFFEC) == 0
            && wrapped.ReadAttempts.Length == 1
            && wrapped.ReadAttempts[0].Address == 0
            && wrapped.ReadAttempts[0].AddressCaptured
            && wrapped.TestFlags?.Zero == false
            && wrapped.JzTaken == false
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C92AB);

        CapacityFinalCalleeLargeRightFieldGatePlan oddLowByte =
            Projection.Project(new(
                EntryEDX: 0,
                FreshEDXPlus14: 0x100));
        check("TEST parity uses only the low byte and zero remains distinct from missing",
            oddLowByte.FreshReadAddress == 0x14
            && oddLowByte.FreshReadValueCaptured
            && oddLowByte.TestResult == 0x100
            && oddLowByte.TestFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && oddLowByte.JzTaken == false
            && oddLowByte.Outcome
                == CapacityFinalCalleeLargeRightFieldGateOutcome.NonzeroTo92AB);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C92A4, "8B 4A 14", null, null),
            (0x012C92A7, "85 C9", null, null),
            (0x012C92A9, "74 12", 0x012C92BD, 0x016C9EBD)
        };
        uint nextRawOffset = Projection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeRightFieldGateInstructionEvidence instruction
            in Projection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != Projection.RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != Projection.RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return Projection.EntryRawFileOffset == 0x012C92A4
            && Projection.EntryFlatIdaAddress == 0x012C92A4
            && Projection.EntryVirtualAddress == 0x016C9EA4
            && Projection.RawFileOffsetToVirtualAddress(Projection.EntryRawFileOffset)
                == Projection.EntryVirtualAddress
            && Projection.BoundaryRawFileOffsetExclusive == 0x012C92AB
            && Projection.JzTargetRawFileOffset == 0x012C92BD
            && Projection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected)
            && nextRawOffset == Projection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(Projection.CapturedWindowBytes)
            && Projection.RawFileOffsetToVirtualAddress(0x012C92AB + 0x12)
                == 0x016C9EBD;
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
