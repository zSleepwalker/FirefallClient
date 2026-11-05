using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonHighSizeCeilingGateProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw range, flat/VA mapping, branch destinations, and bytes match",
            CapacityFinalCalleeCommonHighSizeCeilingGateProjection.EntryRawFileOffset
                == 0x012C9361
            && CapacityFinalCalleeCommonHighSizeCeilingGateProjection.EntryVirtualAddress
                == 0x016C9F61
            && CapacityFinalCalleeCommonHighSizeCeilingGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonHighSizeCeilingGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeCommonHighSizeCeilingGateProjection.EntryVirtualAddress
            && CapacityFinalCalleeCommonHighSizeCeilingGateProjection.BelowOrEqualDestinationRawFileOffset
                == 0x012C936F
            && CapacityFinalCalleeCommonHighSizeCeilingGateProjection.AboveDestinationRawFileOffset
                == 0x012C9384
            && CapacityFinalCalleeCommonHighSizeCeilingGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonHighSizeCeilingGateProjection.BelowOrEqualDestinationRawFileOffset)
                == 0x016C9F6F
            && CapacityFinalCalleeCommonHighSizeCeilingGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonHighSizeCeilingGateProjection.AboveDestinationRawFileOffset)
                == 0x016C9F84
            && CapacityFinalCalleeCommonHighSizeCeilingGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C936F
            && Normalize(CapacityFinalCalleeCommonHighSizeCeilingGateProjection.CapturedWindowBytes)
                == "3DFFFF00007607B91F000000EB15"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonHighSizeCeilingGatePlan unknown =
            CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Project(new(EntryEAX: null));
        CapacityFinalCalleeCommonHighSizeCeilingGatePlan zero =
            CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Project(new(EntryEAX: 0));
        check("Unknown EAX is not treated as zero and stops at 9361",
            unknown.Outcome
                == CapacityFinalCalleeCommonHighSizeCeilingGateOutcome.MissingEntryEAX
            && !unknown.CompareReached
            && !unknown.JBEReached
            && unknown.Events.IsEmpty
            && unknown.FlagsAtStop is null
            && unknown.StopAtRawFileOffsetExclusive == 0x012C9361
            && NoMemoryEffects(unknown)
            && zero.Outcome
                == CapacityFinalCalleeCommonHighSizeCeilingGateOutcome
                    .UnsignedBelowOrEqualToCeilingAt936FBeforeBSR
            && zero.CompareReached
            && zero.JBEReached
            && zero.JBETaken == true
            && zero.JBESelectedDestinationRawFileOffset == 0x012C936F
            && zero.StopAtRawFileOffsetExclusive == 0x012C936F);

        CapacityFinalCalleeCommonHighSizeCeilingGatePlan equal =
            CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Project(
                new(EntryEAX: 0x0000FFFF));
        CapacityFinalCalleeCommonHighSizeCeilingGatePlan belowByOne =
            CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Project(
                new(EntryEAX: 0x0000FFFE));
        check("Borrow and equality independently select unsigned JBE to 936F",
            belowByOne.CompareFlags is
            {
                Carry: true,
                Zero: false
            }
            && belowByOne.JBETaken == true
            && equal.CompareFlags is
            {
                Carry: false,
                Zero: true
            }
            && equal.JBETaken == true
            && new[] { belowByOne, equal }.All(plan =>
                plan.Outcome
                    == CapacityFinalCalleeCommonHighSizeCeilingGateOutcome
                        .UnsignedBelowOrEqualToCeilingAt936FBeforeBSR
                && plan.StopAtRawFileOffsetExclusive == 0x012C936F
                && plan.Events.Length == 2
                && !plan.MoveReached
                && !plan.JMPReached
                && NoMemoryEffects(plan)));

        CapacityFinalCalleeCommonHighSizeCeilingGatePlan firstAbove =
            CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Project(
                new(EntryEAX: 0x00010000));
        CapacityFinalCalleeCommonHighSizeCeilingGatePlan uintMax =
            CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Project(
                new(EntryEAX: uint.MaxValue));
        CapacityFinalCalleeCommonHighSizeCeilingGatePlan signBit =
            CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Project(
                new(EntryEAX: 0x80000000));
        check("Above-ceiling values execute MOV/JMP and stop before the store at 9384",
            firstAbove.CompareResult == 1
            && uintMax.CompareResult == 0xFFFF0000
            && signBit.CompareResult == 0x7FFF0001
            && new[] { firstAbove, uintMax, signBit }.All(plan =>
                plan.Outcome
                    == CapacityFinalCalleeCommonHighSizeCeilingGateOutcome
                        .UnsignedAboveCeilingAt9384BeforeStore
                && plan.JBETaken == false
                && plan.JBESelectedDestinationRawFileOffset == 0x012C9368
                && plan.MoveReached
                && plan.MoveResultECX == 31
                && plan.JMPReached
                && plan.JMPTaken == true
                && plan.JMPSelectedDestinationRawFileOffset == 0x012C9384
                && plan.StopAtRawFileOffsetExclusive == 0x012C9384
                && plan.Events.Select(e => e.Kind).SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonHighSizeCeilingGateEventKind.Compare,
                    CapacityFinalCalleeCommonHighSizeCeilingGateEventKind.BranchBelowOrEqual,
                    CapacityFinalCalleeCommonHighSizeCeilingGateEventKind.Move,
                    CapacityFinalCalleeCommonHighSizeCeilingGateEventKind.Jump
                })
                && NoMemoryEffects(plan)));

        check("CMP computes full-width AF/PF/OF and later MOV/JMP preserve its flags",
            zero.CompareFlags is
            {
                Carry: true,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && equal.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && firstAbove.CompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && uintMax.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && signBit.CompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && new[] { firstAbove, uintMax, signBit }.All(plan =>
                plan.FlagsAtStop == plan.CompareFlags
                && plan.Events[^2].Flags == plan.CompareFlags
                && plan.Events[^1].Flags == plan.CompareFlags));

        check("Every path stays bounded and performs no frame/global memory access",
            new[] { unknown, zero, belowByOne, equal, firstAbove, uintMax, signBit }
                .All(NoMemoryEffects));
    }

    private static bool NoMemoryEffects(CapacityFinalCalleeCommonHighSizeCeilingGatePlan plan) =>
        !plan.FrameReadReached
        && !plan.GlobalReadReached
        && plan.Reads.IsEmpty
        && plan.Writes.IsEmpty
        && !plan.TargetEntered;

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C9361, "3D FF FF 00 00", null, null),
            (0x012C9366, "76 07", 0x012C936F, 0x016C9F6F),
            (0x012C9368, "B9 1F 00 00 00", null, null),
            (0x012C936D, "EB 15", 0x012C9384, 0x016C9F84)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonHighSizeCeilingGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonHighSizeCeilingGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonHighSizeCeilingGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonHighSizeCeilingGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCommonHighSizeCeilingGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonHighSizeCeilingGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonHighSizeCeilingGateProjection.Instructions
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
