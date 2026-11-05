using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete parent-store and post-store window ends at 927C",
            CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection.EntryRawFileOffset
                == 0x012C926B
            && CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection.EntryVirtualAddress
                == 0x016C9E6B
            && CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection
                .PostStoreZeroGateRawFileOffset == 0x012C9278
            && CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection
                .ContinuationBoundaryRawFileOffsetExclusive == 0x012C927C
            && CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection
                .ZeroBranchTargetRawFileOffset == 0x012C92BD
            && CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection
                .NonzeroFallthroughRawFileOffset == 0x012C927C
            && Normalize(CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection
                .CapturedWindowBytes) == "3951107505894110EB0389411485C07441"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan equalZero =
            Project(
                entryECX: 0x1000,
                entryEDX: 0x12345678,
                freshECXPlus10: 0x12345678,
                entryEAX: 0);
        check("Equal parent path stores at ECX+10, jumps to 9278, then TEST takes zero",
            equalZero.Outcome
                == CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome.ZeroTo92BD
            && equalZero.ParentLifecycle.Outcome
                == CapacityFinalCalleeLargeIndexedFieldStoreLifecycleOutcome
                    .EqualStoreToContinuation
            && equalZero.ParentGate.BranchSelectedDestinationRawFileOffset == 0x012C9270
            && equalZero.ParentGate.BranchTaken == false
            && equalZero.ParentLifecycle.StoreFieldOffset == 0x10
            && equalZero.ParentLifecycle.StoreAddress == 0x1010
            && equalZero.ParentLifecycle.StoreValue == 0
            && equalZero.Writes is
                [{
                    RawFileOffset: 0x012C9270,
                    Address: 0x1010,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Value: 0,
                    Committed: true
                }]
            && equalZero.ParentLifecycle.FinalJumpReached
            && equalZero.ParentLifecycle.FinalJumpDestinationRawFileOffset == 0x012C9278
            && equalZero.ChildEAXDerivedFromParentStore
            && equalZero.ChildEAX == equalZero.ParentLifecycle.StoreValue
            && equalZero.ParentGate.CompareLeft == equalZero.ParentGate.CompareRight
            && equalZero.ParentGate.CompareLeft != equalZero.ChildEAX
            && equalZero.PostStoreZeroGate?.Outcome
                == CapacityFinalCalleeLargePostStoreZeroGateOutcome.ZeroTo92BD
            && equalZero.PostStoreZeroGate?.TestResult == 0
            && equalZero.PostStoreZeroGate?.JZTaken == true
            && equalZero.PostStoreZeroGate?.SelectedDestinationRawFileOffset == 0x012C92BD
            && equalZero.ReachedInstructions.SequenceEqual(new uint[]
                { 0x012C926B, 0x012C926E, 0x012C9270, 0x012C9273, 0x012C9278, 0x012C927A })
            && equalZero.Events.Select(item => item.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind.Compare,
                CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind
                    .ConditionalBranch,
                CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind.Store,
                CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind
                    .UnconditionalJump,
                CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind.Test,
                CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleEventKind
                    .ConditionalZeroBranch
            })
            && equalZero.ExactBoundedCompletion
            && equalZero.StopAtRawFileOffsetExclusive == 0x012C92BD
            && !equalZero.ContinuationEntered);

        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan unequalMaximum =
            Project(
                entryECX: 0x2000,
                entryEDX: 2,
                freshECXPlus10: 1,
                entryEAX: uint.MaxValue);
        check("Unequal parent path stores at ECX+14, falls to 9278, then TEST takes nonzero",
            unequalMaximum.Outcome
                == CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome.NonzeroTo927C
            && unequalMaximum.ParentLifecycle.Outcome
                == CapacityFinalCalleeLargeIndexedFieldStoreLifecycleOutcome
                    .UnequalStoreToContinuation
            && unequalMaximum.ParentGate.BranchSelectedDestinationRawFileOffset == 0x012C9275
            && unequalMaximum.ParentGate.BranchTaken == true
            && unequalMaximum.ParentLifecycle.StoreFieldOffset == 0x14
            && unequalMaximum.ParentLifecycle.StoreAddress == 0x2014
            && unequalMaximum.ParentLifecycle.StoreValue == uint.MaxValue
            && unequalMaximum.Writes is
                [{
                    RawFileOffset: 0x012C9275,
                    Address: 0x2014,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Value: uint.MaxValue,
                    Committed: true
                }]
            && !unequalMaximum.ParentLifecycle.FinalJumpReached
            && unequalMaximum.ChildEAXDerivedFromParentStore
            && unequalMaximum.ChildEAX == uint.MaxValue
            && unequalMaximum.ParentGate.CompareLeft != unequalMaximum.ChildEAX
            && unequalMaximum.PostStoreZeroGate?.Outcome
                == CapacityFinalCalleeLargePostStoreZeroGateOutcome.NonzeroTo927C
            && unequalMaximum.PostStoreZeroGate?.TestResult == uint.MaxValue
            && unequalMaximum.PostStoreZeroGate?.JZTaken == false
            && unequalMaximum.PostStoreZeroGate?.SelectedDestinationRawFileOffset == 0x012C927C
            && unequalMaximum.PostStoreZeroGate?.TestFlags is
                {
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: null,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                }
            && unequalMaximum.ReachedInstructions.SequenceEqual(new uint[]
                { 0x012C926B, 0x012C926E, 0x012C9275, 0x012C9278, 0x012C927A })
            && unequalMaximum.ExactBoundedCompletion
            && unequalMaximum.StopAtRawFileOffsetExclusive == 0x012C927C
            && !unequalMaximum.ContinuationEntered);

        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan missingECX =
            Project(entryECX: null, entryEDX: 7, freshECXPlus10: 7, entryEAX: 9);
        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan missingFresh =
            Project(entryECX: 0x3000, entryEDX: 7, freshECXPlus10: null, entryEAX: 9);
        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan missingEDX =
            Project(entryECX: 0x3000, entryEDX: null, freshECXPlus10: 7, entryEAX: 9);
        check("Each missing parent capture stops before a child TEST and preserves parent reads",
            missingECX.Outcome
                == CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome
                    .ParentLifecycleIncomplete
            && missingECX.ParentGate.Outcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingEntryECX
            && missingECX.Reads.IsEmpty
            && missingECX.Writes.IsEmpty
            && missingECX.ParentFlagsAtStore is null
            && !missingECX.ChildGateEntered
            && missingECX.ReachedInstructions.SequenceEqual(new[] { 0x012C926Bu })
            && missingFresh.Outcome
                == CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome
                    .ParentLifecycleIncomplete
            && missingFresh.ParentGate.Outcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingFreshECXPlus10
            && missingFresh.Reads is
                [{
                    RawFileOffset: 0x012C926B,
                    Address: 0x3010,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null
                }]
            && !missingFresh.ChildGateEntered
            && missingFresh.ParentFlagsAtStore is null
            && missingEDX.Outcome
                == CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome
                    .ParentLifecycleIncomplete
            && missingEDX.ParentGate.Outcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingEntryEDX
            && missingEDX.Reads is
                [{
                    RawFileOffset: 0x012C926B,
                    Address: 0x3010,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Value: 7
                }]
            && !missingEDX.ChildGateEntered
            && missingEDX.ParentFlagsAtStore is null);

        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan missingEqualStoreEAX =
            Project(entryECX: 0x4000, entryEDX: 4, freshECXPlus10: 4, entryEAX: null);
        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan missingUnequalStoreEAX =
            Project(entryECX: 0x4000, entryEDX: 4, freshECXPlus10: 5, entryEAX: null);
        check("Missing late EAX keeps each attempted store and CMP flags without a child",
            missingEqualStoreEAX.Outcome
                == CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome
                    .ParentStoreValueMissing
            && missingEqualStoreEAX.Writes is
                [{
                    RawFileOffset: 0x012C9270,
                    Address: 0x4010,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && missingEqualStoreEAX.ParentFlagsAtStore is
                { Zero: true }
            && missingEqualStoreEAX.FlagsAtStop is { Zero: true, AuxiliaryCarry: false }
            && !missingEqualStoreEAX.ChildGateEntered
            && missingEqualStoreEAX.StopAtRawFileOffsetExclusive == 0x012C9270
            && missingUnequalStoreEAX.Outcome
                == CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleOutcome
                    .ParentStoreValueMissing
            && missingUnequalStoreEAX.Writes is
                [{
                    RawFileOffset: 0x012C9275,
                    Address: 0x4014,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && missingUnequalStoreEAX.ParentFlagsAtStore is
                { Zero: false }
            && missingUnequalStoreEAX.FlagsAtStop is { Zero: false }
            && !missingUnequalStoreEAX.ChildGateEntered
            && missingUnequalStoreEAX.StopAtRawFileOffsetExclusive == 0x012C9275);

        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan wrappedEqual =
            Project(
                entryECX: 0xFFFFFFF0,
                entryEDX: 8,
                freshECXPlus10: 8,
                entryEAX: 9);
        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan wrappedUnequal =
            Project(
                entryECX: 0xFFFFFFF8,
                entryEDX: 8,
                freshECXPlus10: 9,
                entryEAX: 10);
        check("Wrapped addresses and read/store aliasing retain independent values and order",
            wrappedEqual.Reads is
                [{
                    Address: 0,
                    ValueCaptured: true,
                    Value: 8
                }]
            && wrappedEqual.Writes is
                [{
                    Address: 0,
                    ValueCaptured: true,
                    Value: 9,
                    Committed: true
                }]
            && wrappedEqual.Reads[0].Address == wrappedEqual.Writes[0].Address
            && wrappedEqual.Reads[0].Value != wrappedEqual.Writes[0].Value
            && wrappedEqual.ParentLifecycle.StoreAddress == 0
            && wrappedEqual.ChildEAX == 9
            && wrappedUnequal.ParentGate.BranchTaken == true
            && wrappedUnequal.ParentLifecycle.StoreAddress == 0xC
            && wrappedUnequal.Writes[0].Address == 0xC
            && wrappedUnequal.Reads[0].Address == 8);

        CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan replacedFlags =
            Project(entryECX: 0x5000, entryEDX: 2, freshECXPlus10: 1, entryEAX: 1);
        check("TEST replaces CMP flags, resets CF, and leaves AF undefined",
            replacedFlags.ParentFlagsAtStore is
                {
                    Carry: true,
                    Parity: true,
                    AuxiliaryCarry: true,
                    Zero: false
                }
            && replacedFlags.FlagsAtStop is
                {
                    Carry: false,
                    Parity: false,
                    AuxiliaryCarry: null,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            && replacedFlags.ParentFlagsAtStore?.Carry
                != replacedFlags.FlagsAtStop?.Carry
            && replacedFlags.ParentFlagsAtStore?.Parity
                != replacedFlags.FlagsAtStop?.Parity
            && replacedFlags.ParentFlagsAtStore?.AuxiliaryCarry
                != replacedFlags.FlagsAtStop?.AuxiliaryCarry
            && replacedFlags.PostStoreZeroGate?.JZTaken == false
            && replacedFlags.StopAtRawFileOffsetExclusive == 0x012C927C);
    }

    private static CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecyclePlan Project(
        uint? entryECX,
        uint? entryEDX,
        uint? freshECXPlus10,
        uint? entryEAX)
    {
        var parentGate = new CapacityFinalCalleeLargeIndexedFieldGateCapture(
            entryECX,
            entryEDX,
            freshECXPlus10);
        var parentCapture = new CapacityFinalCalleeLargeIndexedFieldStoreLifecycleCapture(
            parentGate,
            entryEAX);
        return CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection.Project(
            new(parentCapture));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C926B, "39 51 10", null, null),
            (0x012C926E, "75 05", 0x012C9275, 0x016C9E75),
            (0x012C9270, "89 41 10", null, null),
            (0x012C9273, "EB 03", 0x012C9278, 0x016C9E78),
            (0x012C9275, "89 41 14", null, null),
            (0x012C9278, "85 C0", null, null),
            (0x012C927A, "74 41", 0x012C92BD, 0x016C9EBD)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        var instructions =
            CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection.Instructions;
        foreach (CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleInstructionEvidence instruction
            in instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != unchecked(instruction.RawFileOffset + 0x00400C00)
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

        return instructions.Length == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection
                    .ContinuationBoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeIndexedFieldStoreZeroLifecycleProjection
                    .CapturedWindowBytes)
            && instructions.Select(instruction =>
                (instruction.RawFileOffset,
                    instruction.Bytes,
                    instruction.BranchTargetRawFileOffset,
                    instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
