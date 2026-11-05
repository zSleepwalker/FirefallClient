using System;
using System.Collections.Immutable;
using System.Linq;
using Firefall.Reconstruction.Startup;

namespace Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonZeroSlotStoreTailChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The offsets, raw-to-VA mapping, instruction bytes, and short-jump target match",
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.EntryRawFileOffset
                == 0x012C9468
            && CapacityFinalCalleeCommonZeroSlotStoreTailProjection.EntryFlatIdaAddress
                == 0x012C9468
            && CapacityFinalCalleeCommonZeroSlotStoreTailProjection.EntryVirtualAddress
                == 0x016CA068
            && CapacityFinalCalleeCommonZeroSlotStoreTailProjection.RawFileOffsetToVirtualAddress(
                0x012C9468) == 0x016CA068
            && CapacityFinalCalleeCommonZeroSlotStoreTailProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9475
            && CapacityFinalCalleeCommonZeroSlotStoreTailProjection.RawFileOffsetToVirtualAddress(
                0x012C9475) == 0x016CA075
            && CapacityFinalCalleeCommonZeroSlotStoreTailProjection.JumpTargetRawFileOffset
                == 0x012C942F
            && CapacityFinalCalleeCommonZeroSlotStoreTailProjection.JumpTargetVirtualAddress
                == 0x016CA02F
            && InstructionEvidenceMatches());

        var incomingFlags = new CapacityFinalCalleeCommonZeroSlotStoreTailFlags(
            Carry: true,
            Parity: false,
            AuxiliaryCarry: null,
            Zero: true,
            Sign: true,
            Overflow: false);

        CapacityFinalCalleeCommonZeroSlotStoreTailPlan missingECX =
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.Project(new(
                EntryECX: null,
                EntryESI: 0x2000,
                LateEntryEDX: 0x3000,
                IncomingFlags: incomingFlags));
        check("A missing ECX keeps the independent ESI source but blocks the first store and later EDX use",
            missingECX.Outcome
                == CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingEntryECXForFirstStore
            && missingECX.Writes is
                [{
                    RawFileOffset: 0x012C9468,
                    Address: null,
                    AddressCaptured: false,
                    ValueCaptured: true,
                    Value: 0x2000,
                    Attempted: true,
                    Committed: false
                }]
            && missingECX.EntryECXReadReached
            && missingECX.EntryESIReadReached
            && missingECX.FirstStoreAttempted
            && !missingECX.FirstStoreCommitted
            && !missingECX.LateEntryEDXReadReached
            && missingECX.UnreachedLateEntryEDXCaptureProvided
            && missingECX.LateEntryEDXUnusedBecauseFirstStoreBlocked
            && missingECX.FlagsAtStop == incomingFlags
            && !missingECX.ExactBoundedExitAdmitted
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C9468);

        CapacityFinalCalleeCommonZeroSlotStoreTailPlan missingESI =
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.Project(new(
                EntryECX: 0x1000,
                EntryESI: null,
                LateEntryEDX: 0x3000));
        check("A missing ESI keeps the first destination but blocks the store and leaves EDX unused",
            missingESI.Outcome
                == CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.MissingEntryESIForFirstStore
            && missingESI.Writes is
                [{
                    Address: 0x1000,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Attempted: true,
                    Committed: false
                }]
            && missingESI.FirstStoreAttempted
            && !missingESI.FirstStoreCommitted
            && !missingESI.LateEntryEDXReadReached
            && missingESI.UnreachedLateEntryEDXCaptureProvided
            && missingESI.LateEntryEDXUnusedBecauseFirstStoreBlocked
            && !missingESI.JumpReached
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9468);

        CapacityFinalCalleeCommonZeroSlotStoreTailPlan missingEDX =
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.Project(new(
                EntryECX: 0x1000,
                EntryESI: 0x2000,
                LateEntryEDX: null,
                IncomingFlags: incomingFlags));
        check("A missing late EDX preserves the first commit and records the blocked second-store attempt",
            missingEDX.Outcome
                == CapacityFinalCalleeCommonZeroSlotStoreTailOutcome
                    .MissingLateEntryEDXForSecondStore
            && missingEDX.Writes is
                [
                    {
                        RawFileOffset: 0x012C9468,
                        Address: 0x1000,
                        Value: 0x2000,
                        Attempted: true,
                        Committed: true
                    },
                    {
                        RawFileOffset: 0x012C946A,
                        Address: 0x2018,
                        AddressCaptured: true,
                        ValueCaptured: false,
                        Value: null,
                        Attempted: true,
                        Committed: false
                    }
                ]
            && missingEDX.LateEntryEDXReadReached
            && !missingEDX.UnreachedLateEntryEDXCaptureProvided
            && !missingEDX.LateEntryEDXUnusedBecauseFirstStoreBlocked
            && !missingEDX.ThirdStoreAttempted
            && !missingEDX.FourthStoreAttempted
            && !missingEDX.JumpReached
            && missingEDX.FlagsAtStop == incomingFlags
            && missingEDX.FlagsPreserved
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C946A);

        CapacityFinalCalleeCommonZeroSlotStoreTailPlan complete =
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.Project(new(
                EntryECX: 0x1000,
                EntryESI: 0x2000,
                LateEntryEDX: 0x3000,
                IncomingFlags: incomingFlags));
        check("All four ordered stores commit before the direct jump, preserving incoming flags",
            complete.Outcome
                == CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.CompletedToJumpTarget
            && complete.Writes is
                [
                    {
                        RawFileOffset: 0x012C9468,
                        Address: 0x1000,
                        Value: 0x2000,
                        Attempted: true,
                        Committed: true
                    },
                    {
                        RawFileOffset: 0x012C946A,
                        Address: 0x2018,
                        Value: 0x3000,
                        Attempted: true,
                        Committed: true
                    },
                    {
                        RawFileOffset: 0x012C946D,
                        Address: 0x200C,
                        Value: 0x2000,
                        Attempted: true,
                        Committed: true
                    },
                    {
                        RawFileOffset: 0x012C9470,
                        Address: 0x2008,
                        Value: 0x2000,
                        Attempted: true,
                        Committed: true
                    }
                ]
            && complete.Writes.Length == 4
            && complete.LateEntryEDXReadReached
            && !complete.UnreachedLateEntryEDXCaptureProvided
            && complete.JumpReached
            && complete.JumpTargetRawFileOffset == 0x012C942F
            && complete.JumpTargetVirtualAddress == 0x016CA02F
            && !complete.JumpTargetEntered
            && complete.BoundaryReached
            && !complete.BoundaryInstructionEntered
            && complete.ExactBoundedExitAdmitted
            && complete.StopAtRawFileOffsetExclusive == 0x012C942F
            && complete.InstructionEvidence.SequenceEqual(
                CapacityFinalCalleeCommonZeroSlotStoreTailProjection.Instructions)
            && complete.IncomingFlagsKnown
            && complete.FlagsAtStop == incomingFlags
            && complete.MoveInstructionsPreserveFlags
            && complete.JumpPreservesFlags
            && complete.IncomingFlagsCanBeRetained
            && complete.FlagsPreserved);

        CapacityFinalCalleeCommonZeroSlotStoreTailPlan zeroPointers =
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.Project(new(0, 0, 0));
        check("Zero pointers remain captured values and produce four explicit zero-address stores",
            zeroPointers.Outcome
                == CapacityFinalCalleeCommonZeroSlotStoreTailOutcome.CompletedToJumpTarget
            && zeroPointers.Writes is
                [
                    { Address: 0, Value: 0, AddressCaptured: true, ValueCaptured: true },
                    { Address: 0x18, Value: 0, AddressCaptured: true, ValueCaptured: true },
                    { Address: 0x0C, Value: 0, AddressCaptured: true, ValueCaptured: true },
                    { Address: 8, Value: 0, AddressCaptured: true, ValueCaptured: true }
                ]
            && zeroPointers.JumpReached
            && !zeroPointers.JumpTargetEntered
            && !zeroPointers.IncomingFlagsKnown
            && zeroPointers.FlagsAtStop is null
            && zeroPointers.FlagsPreserved);

        CapacityFinalCalleeCommonZeroSlotStoreTailPlan wrapped =
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.Project(new(
                EntryECX: 0xFFFFFFFC,
                EntryESI: 0xFFFFFFF0,
                LateEntryEDX: 0xAABBCCDD));
        check("ESI-based store addresses wrap as unsigned DWORD addresses",
            wrapped.Writes is
                [
                    { Address: 0xFFFFFFFC, Value: 0xFFFFFFF0 },
                    { Address: 8, Value: 0xAABBCCDD },
                    { Address: 0xFFFFFFFC, Value: 0xFFFFFFF0 },
                    { Address: 0xFFFFFFF8, Value: 0xFFFFFFF0 }
                ]
            && wrapped.Writes.Length == 4
            && wrapped.JumpReached);

        CapacityFinalCalleeCommonZeroSlotStoreTailPlan aliased =
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.Project(new(
                EntryECX: 0x1008,
                EntryESI: 0x1000,
                LateEntryEDX: 0x1008));
        check("Aliased destinations remain distinct writes in original instruction order",
            aliased.Writes is
                [
                    { RawFileOffset: 0x012C9468, Address: 0x1008, Value: 0x1000 },
                    { RawFileOffset: 0x012C946A, Address: 0x1018, Value: 0x1008 },
                    { RawFileOffset: 0x012C946D, Address: 0x100C, Value: 0x1000 },
                    { RawFileOffset: 0x012C9470, Address: 0x1008, Value: 0x1000 }
                ]
            && aliased.Writes.Length == 4
            && aliased.Writes[0].Address == aliased.Writes[3].Address
            && aliased.Writes[0].RawFileOffset < aliased.Writes[3].RawFileOffset);
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
            (0x012C9468, 0x016CA068, "89 31", null, null),
            (0x012C946A, 0x016CA06A, "89 56 18", null, null),
            (0x012C946D, 0x016CA06D, "89 76 0C", null, null),
            (0x012C9470, 0x016CA070, "89 76 08", null, null),
            (0x012C9473, 0x016CA073, "EB BA", 0x012C942F, 0x016CA02F)
        };
        ImmutableArray<CapacityFinalCalleeCommonZeroSlotStoreTailInstructionEvidence> instructions =
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.Instructions;
        if (instructions.Length != expected.Length)
            return false;

        uint nextRawOffset =
            CapacityFinalCalleeCommonZeroSlotStoreTailProjection.EntryRawFileOffset;
        string capturedBytes = string.Empty;
        for (int i = 0; i < instructions.Length; i++)
        {
            CapacityFinalCalleeCommonZeroSlotStoreTailInstructionEvidence instruction =
                instructions[i];
            var expectedInstruction = expected[i];
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != expectedInstruction.Raw
                || instruction.FlatIdaAddress != expectedInstruction.Raw
                || instruction.VirtualAddress != expectedInstruction.Virtual
                || instruction.RawFileOffset != nextRawOffset
                || instruction.Bytes != expectedInstruction.Bytes
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonZeroSlotStoreTailProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || instruction.BranchTargetRawFileOffset
                    != expectedInstruction.BranchTargetRaw
                || instruction.BranchTargetVirtualAddress
                    != expectedInstruction.BranchTargetVirtual
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonZeroSlotStoreTailProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            capturedBytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeCommonZeroSlotStoreTailProjection
                    .BoundaryRawFileOffsetExclusive
            && capturedBytes == Normalize(
                CapacityFinalCalleeCommonZeroSlotStoreTailProjection.CapturedWindowBytes)
            && capturedBytes == "893189561889760C897608EBBA";
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
