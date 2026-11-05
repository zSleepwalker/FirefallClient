using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeBacklinkNextFieldGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The full raw, flat IDA, VA, instruction, and branch anchors match",
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.EntryRawFileOffset
                == 0x012C9288
            && CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.EntryFlatIdaAddress
                == 0x012C9288
            && CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.EntryVirtualAddress
                == 0x016C9E88
            && CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.EntryVirtualAddress
            && CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9292
            && CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.JzDestinationRawFileOffset
                == 0x012C92A4
            && Normalize(
                CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.CapturedWindowBytes)
                == "8948188B4A1085C97412"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan allMissing =
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Project(new(
                EntryEAX: null,
                EntryECX: null,
                EntryEDX: null,
                FreshEDXPlus10: null));
        check("All missing first-store operands retain an attempted uncommitted write and stop",
            allMissing.Outcome
                == CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome
                    .MissingBothFirstStoreOperands
            && allMissing.FirstStoreAttempted
            && !allMissing.FirstStoreAddressCaptured
            && !allMissing.FirstStoreValueCaptured
            && !allMissing.FirstStoreCommitted
            && allMissing.MemoryOperations is
                [{
                    RawFileOffset: 0x012C9288,
                    Kind: CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperationKind.Write,
                    Address: null,
                    AddressCaptured: false,
                    Value: null,
                    ValueCaptured: false,
                    Committed: false
                }]
            && allMissing.ReachedInstructions.SequenceEqual(new uint[] { 0x012C9288 })
            && !allMissing.FreshReadReached
            && !allMissing.TestReached
            && !allMissing.JzReached
            && allMissing.FlagsAtStop is null
            && !allMissing.ExactCleanCompletion
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C9288);

        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan missingEAX =
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Project(new(
                EntryEAX: null,
                EntryECX: 0x12345678,
                EntryEDX: 0x100,
                FreshEDXPlus10: 0x87654321));
        check("A missing address operand retains the independent store value and skips later reads",
            missingEAX.Outcome
                == CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome
                    .MissingEntryEAXForFirstStore
            && !missingEAX.FirstStoreAddressCaptured
            && missingEAX.FirstStoreAddress is null
            && missingEAX.FirstStoreValueCaptured
            && missingEAX.FirstStoreValue == 0x12345678
            && !missingEAX.FirstStoreCommitted
            && missingEAX.MemoryOperations.Length == 1
            && missingEAX.MemoryOperations[0].Value == 0x12345678
            && missingEAX.MemoryOperations[0].ValueCaptured
            && missingEAX.UnreachedEntryEDXCaptureProvided
            && missingEAX.UnreachedFreshEDXPlus10CaptureProvided
            && !missingEAX.FreshReadReached
            && !missingEAX.TestReached
            && !missingEAX.JzReached
            && missingEAX.StopAtRawFileOffsetExclusive == 0x012C9288);

        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan missingECX =
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Project(new(
                EntryEAX: 0x100,
                EntryECX: null,
                EntryEDX: 0x200,
                FreshEDXPlus10: 0));
        check("A missing store value retains the independent wrapped-capable address and stops",
            missingECX.Outcome
                == CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome
                    .MissingEntryECXForFirstStore
            && missingECX.FirstStoreAddressCaptured
            && missingECX.FirstStoreAddress == 0x118
            && !missingECX.FirstStoreValueCaptured
            && missingECX.FirstStoreValue is null
            && !missingECX.FirstStoreCommitted
            && missingECX.MemoryOperations.Length == 1
            && missingECX.MemoryOperations[0].Address == 0x118
            && !missingECX.MemoryOperations[0].Committed
            && !missingECX.FreshReadReached
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C9288);

        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan missingEDX =
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0x44,
                EntryEDX: null,
                FreshEDXPlus10: 0x99));
        check("EDX is a late independent consumer: the store commits, then the read stops at 928B",
            missingEDX.Outcome
                == CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome
                    .MissingEntryEDXForFreshRead
            && missingEDX.FirstStoreCommitted
            && missingEDX.FirstStoreAddress == 0x118
            && missingEDX.FirstStoreValue == 0x44
            && missingEDX.FreshReadReached
            && !missingEDX.FreshReadAddressCaptured
            && missingEDX.FreshReadAddress is null
            && !missingEDX.FreshReadValueCaptured
            && missingEDX.UnusableFreshEDXPlus10CaptureProvided
            && missingEDX.MemoryOperations.Length == 2
            && missingEDX.MemoryOperations[0].Committed
            && missingEDX.MemoryOperations[1] is
                {
                    RawFileOffset: 0x012C928B,
                    Kind: CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperationKind.Read,
                    Address: null,
                    AddressCaptured: false,
                    Value: null,
                    ValueCaptured: false
                }
            && !missingEDX.TestReached
            && !missingEDX.JzReached
            && missingEDX.FlagsAtStop is null
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C928B);

        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan missingFresh =
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0x55,
                EntryEDX: 0x200,
                FreshEDXPlus10: null));
        check("A known fresh-read address is retained when its value is missing",
            missingFresh.Outcome
                == CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome
                    .MissingFreshEDXPlus10
            && missingFresh.FirstStoreCommitted
            && missingFresh.FreshReadReached
            && missingFresh.FreshReadAddressCaptured
            && missingFresh.FreshReadAddress == 0x210
            && !missingFresh.FreshReadValueCaptured
            && missingFresh.MemoryOperations.Length == 2
            && missingFresh.MemoryOperations[1] is
                {
                    RawFileOffset: 0x012C928B,
                    Address: 0x210,
                    AddressCaptured: true,
                    Value: null,
                    ValueCaptured: false
                }
            && !missingFresh.TestReached
            && !missingFresh.JzReached
            && missingFresh.FlagsAtStop is null
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C928B);

        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan zero =
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0xDEADBEEF,
                EntryEDX: 0x200,
                FreshEDXPlus10: 0));
        check("TEST uses the fresh zero value and JZ stops at 92A4 without entering its read",
            zero.Outcome
                == CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.ZeroTo92A4
            && zero.FirstStoreCommitted
            && zero.FirstStoreValue == 0xDEADBEEF
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
            && zero.JzSelectedDestinationRawFileOffset == 0x012C92A4
            && zero.ReachedInstructions.SequenceEqual(
                new uint[] { 0x012C9288, 0x012C928B, 0x012C928E, 0x012C9290 })
            && zero.MemoryOperations.Select(operation => operation.Kind).SequenceEqual(
                new[]
                {
                    CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperationKind.Write,
                    CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperationKind.Read
                })
            && zero.TargetReadReached == false
            && zero.FollowingCMPReached == false
            && zero.ExactCleanCompletion
            && zero.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan nonzero =
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0x80,
                EntryEDX: 0x200,
                FreshEDXPlus10: 0x80000001));
        check("TEST sets logical flags and the nonzero JZ fallthrough stops before CMP at 9292",
            nonzero.Outcome
                == CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.NonzeroTo9292
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
            && nonzero.JzSelectedDestinationRawFileOffset == 0x012C9292
            && !nonzero.FollowingCMPReached
            && !nonzero.TargetReadReached
            && nonzero.ExactCleanCompletion
            && nonzero.StopAtRawFileOffsetExclusive == 0x012C9292);

        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan wrapped =
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Project(new(
                EntryEAX: 0xFFFFFFF0,
                EntryECX: 0xFFFFFFFF,
                EntryEDX: 0xFFFFFFF8,
                FreshEDXPlus10: 1));
        check("Both effective addresses use independent unsigned 32-bit wraparound",
            wrapped.FirstStoreAddress == 8
            && wrapped.FreshReadAddress == 8
            && CapacityFinalCalleeLargeBacklinkNextFieldGateProjection
                .DeriveFirstStoreAddress(0xFFFFFFF0) == 8
            && CapacityFinalCalleeLargeBacklinkNextFieldGateProjection
                .DeriveFreshReadAddress(0xFFFFFFF8) == 8
            && wrapped.MemoryOperations[0].Address == 8
            && wrapped.MemoryOperations[1].Address == 8
            && wrapped.JzTaken == false
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C9292);

        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan aliasedFreshValueDiffers =
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0x40,
                EntryEDX: 0x108,
                FreshEDXPlus10: 0x80));
        check("An aliased fresh read remains ordered and TEST uses its distinct fresh value",
            aliasedFreshValueDiffers.FirstStoreAddress == 0x118
            && aliasedFreshValueDiffers.FirstStoreValue == 0x40
            && aliasedFreshValueDiffers.FreshReadAddress == 0x118
            && aliasedFreshValueDiffers.FreshECX == 0x80
            && aliasedFreshValueDiffers.MemoryOperations.Length == 2
            && aliasedFreshValueDiffers.MemoryOperations[0] is
                {
                    Kind: CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperationKind.Write,
                    Address: 0x118,
                    Value: 0x40,
                    Committed: true
                }
            && aliasedFreshValueDiffers.MemoryOperations[1] is
                {
                    Kind: CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperationKind.Read,
                    Address: 0x118,
                    Value: 0x80,
                    ValueCaptured: true
                }
            && aliasedFreshValueDiffers.TestResult == 0x80
            && aliasedFreshValueDiffers.TestFlags?.Zero == false
            && aliasedFreshValueDiffers.JzTaken == false
            && aliasedFreshValueDiffers.MemoryOperations[0].RawFileOffset
                < aliasedFreshValueDiffers.MemoryOperations[1].RawFileOffset);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C9288, "89 48 18", null, null),
            (0x012C928B, "8B 4A 10", null, null),
            (0x012C928E, "85 C9", null, null),
            (0x012C9290, "74 12", 0x012C92A4, 0x016C9EA4)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeBacklinkNextFieldGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeBacklinkNextFieldGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeBacklinkNextFieldGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeLargeBacklinkNextFieldGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeBacklinkNextFieldGateProjection.Instructions
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
