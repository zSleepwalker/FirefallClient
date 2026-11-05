using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonDualThresholdRingLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The threshold fallthrough and ring stores meet at 941C and stop before the 942F DEC",
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.EntryRawFileOffset
                == 0x012C9403
            && CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.ChildEntryRawFileOffset
                == 0x012C941C
            && CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.CompletionRawFileOffset
                == 0x012C942F
            && CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection
                .IsExactFallthroughHandoff(
                    CapacityFinalCalleeCommonDualPointerThresholdProjection.Project(new(
                        FreshGlobalValue: 1,
                        EntryEDX: 1,
                        FreshEDXPlus8: 1))));

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan missingGlobal =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: null,
                ParentEntryEDX: 0x100,
                FreshEDXPlus8: 0x200,
                LateEntryESI: 0x300));
        check("Missing global stops at its first read with no child or writes",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                    .MissingFreshGlobalValue
            && missingGlobal.ParentOutcome
                == CapacityFinalCalleeCommonDualPointerThresholdOutcome
                    .MissingFreshGlobalValue
            && missingGlobal.ChildPlan is null
            && missingGlobal.Writes.IsEmpty
            && missingGlobal.OrderedMemoryEffects is
                [{ RawFileOffset: 0x012C9403, Attempted: true, Committed: false }]
            && missingGlobal.LateEntryESIProvidedButUnused
            && !missingGlobal.ExplicitDirtyExit
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9403);

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan missingParentEDX =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: 0x80,
                ParentEntryEDX: null,
                FreshEDXPlus8: 0x20,
                LateEntryESI: 0x30));
        check("Missing parent EDX retains only the global read and never creates child work",
            missingParentEDX.Outcome
                == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                    .MissingParentEntryEDX
            && missingParentEDX.ParentPlan.FreshGlobalCaptured
            && !missingParentEDX.ParentPlan.FreshEDXPlus8ReadReached
            && missingParentEDX.OrderedMemoryEffects.Length == 1
            && missingParentEDX.ChildPlan is null
            && missingParentEDX.Writes.IsEmpty
            && missingParentEDX.StopAtRawFileOffsetExclusive == 0x012C9409);

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan missingFreshField =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: 0x80,
                ParentEntryEDX: 0x100,
                FreshEDXPlus8: null,
                LateEntryESI: null));
        check("Missing fresh EDX+8 stops before CMP1 and does not fabricate a child EAX",
            missingFreshField.Outcome
                == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                    .MissingFreshEDXPlus8
            && missingFreshField.ParentPlan.FreshEDXPlus8ReadReached
            && missingFreshField.ParentPlan.FreshEDXPlus8Address == 0x108
            && !missingFreshField.ParentPlan.FirstCompareReached
            && missingFreshField.ChildPlan is null
            && missingFreshField.Writes.IsEmpty
            && missingFreshField.StopAtRawFileOffsetExclusive == 0x012C9409);

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan firstBorrow =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: 0x200,
                ParentEntryEDX: 0x100,
                FreshEDXPlus8: 0x500,
                LateEntryESI: 0x900));
        check("First unsigned borrow branches to 8E76 and leaves provided ESI on an explicit dirty exit",
            firstBorrow.Outcome
                == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                    .FirstUnsignedBelowBypassesChild
            && firstBorrow.ParentPlan.FirstJBTaken == true
            && firstBorrow.ParentDestinationRawFileOffset == 0x012C8E76
            && firstBorrow.ParentStopAtRawFileOffsetExclusive == 0x012C8E76
            && firstBorrow.ParentPlan.FirstCompareFlags?.Carry == true
            && !firstBorrow.ParentPlan.SecondCompareReached
            && !firstBorrow.ParentFallthroughInstructionEntered
            && firstBorrow.ChildPlan is null
            && firstBorrow.LateEntryESIProvidedButUnused
            && firstBorrow.ExplicitDirtyExit
            && firstBorrow.Writes.IsEmpty
            && firstBorrow.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new[] { 0x012C9403u, 0x012C9409u })
            && firstBorrow.FlagsAtStop == firstBorrow.ParentPlan.FirstCompareFlags);

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan secondBorrow =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: 0x180,
                ParentEntryEDX: 0x200,
                FreshEDXPlus8: 0x100,
                LateEntryESI: null));
        check("Second unsigned borrow bypasses the child after the first JB falls through",
            secondBorrow.Outcome
                == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                    .SecondUnsignedBelowBypassesChild
            && secondBorrow.ParentPlan.FirstJBTaken == false
            && secondBorrow.ParentPlan.SecondJBTaken == true
            && secondBorrow.ParentDestinationRawFileOffset == 0x012C8E76
            && secondBorrow.ParentStopAtRawFileOffsetExclusive == 0x012C8E76
            && secondBorrow.ParentPlan.SecondCompareFlags?.Carry == true
            && secondBorrow.ChildPlan is null
            && !secondBorrow.LateEntryESIProvidedButUnused
            && !secondBorrow.ExplicitDirtyExit
            && secondBorrow.Writes.IsEmpty
            && secondBorrow.FlagsAtStop == secondBorrow.ParentPlan.SecondCompareFlags);

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan missingESI =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: 0x1000,
                ParentEntryEDX: 0x2000,
                FreshEDXPlus8: 0x3000,
                LateEntryESI: null));
        check("Missing ESI retains the derived first-store address and CMP2 flags without reading EDX for store two",
            missingESI.Outcome
                == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                    .MissingLateEntryESIForFirstWrite
            && missingESI.ParentExactFallthroughHandoff
            && missingESI.ParentFallthroughInstructionEntered
            && missingESI.ParentPlan.FirstJBTaken == false
            && missingESI.ParentPlan.SecondJBTaken == false
            && missingESI.ParentDestinationRawFileOffset == 0x012C941C
            && missingESI.ParentStopAtRawFileOffsetExclusive == 0x012C941C
            && missingESI.ChildEntryEAX == 0x3000
            && missingESI.ChildLateEntryEDX is null
            && missingESI.ChildPlan?.EntryESIReadReached == true
            && missingESI.Writes is
                [{
                    RawFileOffset: 0x012C941C,
                    Address: 0x300C,
                    AddressCaptured: true,
                    Value: null,
                    ValueCaptured: false,
                    Committed: false
                }]
            && !missingESI.ChildLateEntryEDXReadReached
            && missingESI.FlagsAtStop == missingESI.ParentPlan.SecondCompareFlags
            && missingESI.ChildStoresPreserveParentFlags
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C941C
            && !missingESI.ExactCleanCompletion);

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan clean =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: 0x1000,
                ParentEntryEDX: 0x2000,
                FreshEDXPlus8: 0x3000,
                LateEntryESI: 0x4000));
        check("Exact fallthrough derives child EAX, delays parent EDX until write two, and commits five stores",
            clean.Outcome
                == CapacityFinalCalleeCommonDualThresholdRingLifecycleOutcome
                    .CompletedToBoundaryBeforeDec
            && clean.ParentExactFallthroughHandoff
            && clean.ParentFallthroughInstructionEntered
            && clean.ParentOutcome
                == CapacityFinalCalleeCommonDualPointerThresholdOutcome
                    .BothUnsignedNotBelowFallthrough
            && clean.ParentPlan.FirstJBTaken == false
            && clean.ParentPlan.SecondJBTaken == false
            && clean.ParentDestinationRawFileOffset == 0x012C941C
            && clean.ParentStopAtRawFileOffsetExclusive == 0x012C941C
            && clean.ChildEntryEAX == 0x3000
            && clean.ChildEntryESI == 0x4000
            && clean.ChildLateEntryEDX == 0x2000
            && clean.ChildLateEntryEDXReadReached
            && clean.Writes is
                [
                    { RawFileOffset: 0x012C941C, Address: 0x300C, Value: 0x4000, Committed: true },
                    { RawFileOffset: 0x012C941F, Address: 0x2008, Value: 0x4000, Committed: true },
                    { RawFileOffset: 0x012C9422, Address: 0x4008, Value: 0x3000, Committed: true },
                    { RawFileOffset: 0x012C9425, Address: 0x400C, Value: 0x2000, Committed: true },
                    { RawFileOffset: 0x012C9428, Address: 0x4018, Value: 0, Committed: true }
                ]
            && clean.ChildBoundaryReached
            && clean.ChildBoundaryRawFileOffsetExclusive == 0x012C942F
            && !clean.ChildBoundaryInstructionEntered
            && !clean.DecAtBoundaryEntered
            && clean.StopAtRawFileOffsetExclusive == 0x012C942F
            && clean.ExactCleanCompletion
            && !clean.DestinationEntered
            && clean.ChildStoresPreserveParentFlags
            && clean.FlagsAtStop == clean.ParentPlan.SecondCompareFlags
            && clean.FlagsAtStop is { Carry: false, Zero: false });

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan aliased =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: 0x1000,
                ParentEntryEDX: 0x1000,
                FreshEDXPlus8: 0x1000,
                LateEntryESI: 0x1000));
        check("Aliased child destinations remain five ordered writes and retain each source value",
            aliased.Writes is
                [
                    { Address: 0x100C, Value: 0x1000, Committed: true },
                    { Address: 0x1008, Value: 0x1000, Committed: true },
                    { Address: 0x1008, Value: 0x1000, Committed: true },
                    { Address: 0x100C, Value: 0x1000, Committed: true },
                    { Address: 0x1018, Value: 0, Committed: true }
                ]
            && aliased.Writes.Length == 5
            && aliased.ExactCleanCompletion);

        CapacityFinalCalleeCommonDualThresholdRingLifecyclePlan wrapping =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.Project(new(
                FreshGlobalValue: 0,
                ParentEntryEDX: 0xFFFFFFFC,
                FreshEDXPlus8: 0xFFFFFFF8,
                LateEntryESI: 0xFFFFFFF4));
        check("Threshold and child pointer arithmetic use independent wrapping DWORD values",
            wrapping.ParentPlan.FreshEDXPlus8Address == 4
            && wrapping.ParentPlan.FirstJBTaken == false
            && wrapping.ParentPlan.SecondJBTaken == false
            && wrapping.Writes is
                [
                    { Address: 4, Value: 0xFFFFFFF4 },
                    { Address: 4, Value: 0xFFFFFFF4 },
                    { Address: 0xFFFFFFFC, Value: 0xFFFFFFF8 },
                    { Address: 0, Value: 0xFFFFFFFC },
                    { Address: 0x0C, Value: 0 }
                ]
            && wrapping.ExactCleanCompletion);

        check("All parent reads precede the five child write attempts in the aggregate",
            clean.OrderedMemoryEffects.Select(effect => effect.RawFileOffset).SequenceEqual(
                new[]
                {
                    0x012C9403u,
                    0x012C9409u,
                    0x012C941Cu,
                    0x012C941Fu,
                    0x012C9422u,
                    0x012C9425u,
                    0x012C9428u
                })
            && clean.OrderedMemoryEffects.Take(2).All(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffectKind
                        .FreshRead
                && effect.Attempted
                && effect.ValueCaptured
                && !effect.Committed)
            && clean.OrderedMemoryEffects.Skip(2).All(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonDualThresholdRingLifecycleMemoryEffectKind
                        .ChildWriteAttempt
                && effect.Attempted
                && effect.Committed));

        check("Threshold and ring instruction evidence retain exact addresses and contiguous bytes",
            ThresholdEvidenceMatches()
            && RingEvidenceMatches());
    }

    private static bool ThresholdEvidenceMatches()
    {
        CapacityFinalCalleeCommonDualPointerThresholdInstructionEvidence[] instructions =
            CapacityFinalCalleeCommonDualPointerThresholdProjection.Instructions.ToArray();
        return instructions.Length == 6
            && instructions[0].RawFileOffset == 0x012C9403
            && instructions[1].RawFileOffset == 0x012C9409
            && instructions[4].RawFileOffset == 0x012C9414
            && instructions[5].RawFileOffset == 0x012C9416
            && instructions[3].BranchTargetRawFileOffset == 0x012C8E76
            && instructions[5].BranchTargetRawFileOffset == 0x012C8E76
            && instructions[^1].RawFileOffset
                + (uint)(Normalize(instructions[^1].Bytes).Length / 2)
                == CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection
                    .ChildEntryRawFileOffset;
    }

    private static bool RingEvidenceMatches()
    {
        CapacityFinalCalleeCommonRingInsertionInstructionEvidence[] instructions =
            CapacityFinalCalleeCommonRingInsertionProjection.Instructions.ToArray();
        if (instructions.Length != 5)
        {
            return false;
        }

        uint nextRawOffset =
            CapacityFinalCalleeCommonDualThresholdRingLifecycleProjection.ChildEntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonRingInsertionInstructionEvidence instruction
            in instructions)
        {
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonRingInsertionProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            {
                return false;
            }

            string normalized = Normalize(instruction.Bytes);
            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset == 0x012C942F
            && bytes == Normalize(
                CapacityFinalCalleeCommonRingInsertionProjection.CapturedWindowBytes)
            && instructions[^1].RawFileOffset == 0x012C9428
            && instructions[^1].Bytes == "C7 46 18 00 00 00 00";
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
