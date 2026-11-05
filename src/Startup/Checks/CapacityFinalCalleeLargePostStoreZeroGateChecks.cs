using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargePostStoreZeroGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window, addresses, VA mapping, and JZ target match",
            CapacityFinalCalleeLargePostStoreZeroGateProjection.EntryRawFileOffset
                == 0x012C9278
            && CapacityFinalCalleeLargePostStoreZeroGateProjection.EntryFlatIdaAddress
                == 0x012C9278
            && CapacityFinalCalleeLargePostStoreZeroGateProjection.EntryVirtualAddress
                == 0x016C9E78
            && CapacityFinalCalleeLargePostStoreZeroGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargePostStoreZeroGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargePostStoreZeroGateProjection.EntryVirtualAddress
            && CapacityFinalCalleeLargePostStoreZeroGateProjection.JZRawFileOffset
                == 0x012C927A
            && CapacityFinalCalleeLargePostStoreZeroGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C927C
            && CapacityFinalCalleeLargePostStoreZeroGateProjection.BranchTargetRawFileOffset
                == 0x012C92BD
            && CapacityFinalCalleeLargePostStoreZeroGateProjection
                .RawFileOffsetToVirtualAddress(0x012C92BD) == 0x016C9EBD
            && CapacityFinalCalleeLargePostStoreZeroGateProjection
                .CalculateBranchTargetRawFileOffset() == 0x012C92BD
            && Normalize(
                CapacityFinalCalleeLargePostStoreZeroGateProjection.CapturedWindowBytes)
                == "85C07441"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargePostStoreZeroGatePlan missing =
            CapacityFinalCalleeLargePostStoreZeroGateProjection.Project(new(EntryEAX: null));
        check("Missing EAX stops at TEST with no flags or branch decision",
            missing.Outcome
                == CapacityFinalCalleeLargePostStoreZeroGateOutcome.MissingEntryEAX
            && !missing.TestReached
            && !missing.EntryEAXCaptured
            && missing.TestResult is null
            && missing.FlagsAtStop is null
            && !missing.JZReached
            && missing.JZTaken is null
            && missing.SelectedDestinationRawFileOffset is null
            && !missing.DestinationEntered
            && !missing.ExactBoundedBranchCompletion
            && missing.ReachedInstructions.IsEmpty
            && missing.StopAtRawFileOffsetExclusive == 0x012C9278);

        CapacityFinalCalleeLargePostStoreZeroGatePlan zero =
            CapacityFinalCalleeLargePostStoreZeroGateProjection.Project(new(EntryEAX: 0));
        check("Zero EAX takes JZ and stops at its bounded 92BD destination",
            zero.Outcome == CapacityFinalCalleeLargePostStoreZeroGateOutcome.ZeroTo92BD
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
            && zero.JZReached
            && zero.JZTaken == true
            && zero.SelectedDestinationRawFileOffset == 0x012C92BD
            && zero.SelectedDestinationVirtualAddress == 0x016C9EBD
            && zero.StopAtRawFileOffsetExclusive == 0x012C92BD
            && zero.ReachedInstructions.SequenceEqual(
                new uint[] { 0x012C9278, 0x012C927A })
            && !zero.DestinationEntered
            && zero.ExactBoundedBranchCompletion);

        CapacityFinalCalleeLargePostStoreZeroGatePlan one =
            CapacityFinalCalleeLargePostStoreZeroGateProjection.Project(new(EntryEAX: 1));
        check("EAX one has odd low-byte parity and stops at the 927C fallthrough",
            one.Outcome == CapacityFinalCalleeLargePostStoreZeroGateOutcome.NonzeroTo927C
            && one.TestFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && one.JZTaken == false
            && one.SelectedDestinationRawFileOffset == 0x012C927C
            && one.SelectedDestinationVirtualAddress == 0x016C9E7C
            && one.StopAtRawFileOffsetExclusive == 0x012C927C
            && !one.DestinationEntered
            && one.ExactBoundedBranchCompletion);

        CapacityFinalCalleeLargePostStoreZeroGatePlan allOnes =
            CapacityFinalCalleeLargePostStoreZeroGateProjection.Project(
                new(EntryEAX: uint.MaxValue));
        check("uint.MaxValue keeps DWORD sign and even low-byte parity",
            allOnes.TestResult == uint.MaxValue
            && allOnes.TestFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && allOnes.JZTaken == false
            && allOnes.StopAtRawFileOffsetExclusive == 0x012C927C);

        CapacityFinalCalleeLargePostStoreZeroGatePlan highBit =
            CapacityFinalCalleeLargePostStoreZeroGateProjection.Project(
                new(EntryEAX: 0x80000000));
        check("The sign-bit EAX sets sign while zero low byte has even parity",
            highBit.TestFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && highBit.JZTaken == false
            && highBit.StopAtRawFileOffsetExclusive == 0x012C927C);

        CapacityFinalCalleeLargePostStoreZeroGatePlan evenParity =
            CapacityFinalCalleeLargePostStoreZeroGateProjection.Project(new(EntryEAX: 3));
        check("TEST parity uses all low-byte bits and detects even parity",
            evenParity.TestFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            });

        CapacityFinalCalleeLargePostStoreZeroGatePlan oddParity =
            CapacityFinalCalleeLargePostStoreZeroGateProjection.Project(new(EntryEAX: 7));
        check("TEST parity distinguishes an odd number of low-byte bits",
            oddParity.TestFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
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
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C9278, "85 C0", null, null),
            (0x012C927A, "74 41", 0x012C92BD, 0x016C9EBD)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargePostStoreZeroGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargePostStoreZeroGateInstructionEvidence instruction
            in CapacityFinalCalleeLargePostStoreZeroGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargePostStoreZeroGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargePostStoreZeroGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeLargePostStoreZeroGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeLargePostStoreZeroGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargePostStoreZeroGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargePostStoreZeroGateProjection.Instructions
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
