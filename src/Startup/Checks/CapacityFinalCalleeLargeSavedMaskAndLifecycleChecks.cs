using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSavedMaskAndLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeLargeSavedMaskAndLifecyclePlan derivedNot =
            CapacityFinalCalleeLargeSavedMaskAndLifecycleProjection.Project(new(
                ParentCapture: new(
                    EntryEDX: 0,
                    FreshEDXPlus1C: 1,
                    EntryEBP: 0x200,
                    FreshEBPMinus8: 0x80000001),
                FreshGlobalDword: 0xFFFFFFFF));
        check("The child EAX is derived from parent NOT, not the fresh input or shift result",
            derivedNot.ParentPlan.ShiftInputEAX == 0x80000001
            && derivedNot.ParentPlan.ShiftResult == 2
            && derivedNot.ParentPlan.NotResult == 0xFFFFFFFD
            && derivedNot.ChildEAX == 0xFFFFFFFD
            && derivedNot.ChildEAX != derivedNot.ParentPlan.ShiftInputEAX
            && derivedNot.ChildEAX != derivedNot.ParentPlan.ShiftResult
            && derivedNot.ChildEAXSourceRawFileOffset
                == CapacityFinalCalleeLargeSavedMaskShiftProjection.NotRawFileOffset
            && derivedNot.ChildPlan?.Capture.EntryEAX == derivedNot.ParentPlan.NotResult
            && derivedNot.ChildPlan?.Capture.FreshGlobalDword == 0xFFFFFFFF
            && derivedNot.ChildPlan?.AndResult == 0xFFFFFFFD
            && derivedNot.Outcome
                == CapacityFinalCalleeLargeSavedMaskAndLifecycleOutcome
                    .ChildCompletedTo92BD);

        (uint CountSource, byte MaskedCount)[] countCases =
        [
            (0, 0),
            (1, 1),
            (31, 31),
            (32, 0),
            (uint.MaxValue, 31)
        ];
        const uint freshInput = 0x92345678;
        foreach ((uint countSource, byte maskedCount) in countCases)
        {
            uint shifted = freshInput << maskedCount;
            uint notResult = ~shifted;
            CapacityFinalCalleeLargeSavedMaskAndLifecyclePlan plan =
                CapacityFinalCalleeLargeSavedMaskAndLifecycleProjection.Project(new(
                    ParentCapture: new(
                        EntryEDX: 0x100,
                        FreshEDXPlus1C: countSource,
                        EntryEBP: 0x300,
                        FreshEBPMinus8: freshInput),
                    FreshGlobalDword: uint.MaxValue));

            check($"Count {countSource:X8} reaches the global AND with the NOT-derived EAX",
                plan.ParentPlan.ShiftCountMasked == maskedCount
                && plan.ParentPlan.ShiftInputEAX == freshInput
                && plan.ParentPlan.ShiftResult == shifted
                && plan.ParentPlan.NotResult == notResult
                && plan.ChildEAX == notResult
                && plan.ChildPlan?.AndEAX == notResult
                && plan.ChildPlan?.AndFreshGlobalDword == uint.MaxValue
                && plan.ChildPlan?.AndResult == notResult
                && plan.ChildReached
                && plan.ChildAndFlagsReplacedParentFlags
                && !plan.ParentFlagsPreservedThroughChild
                && plan.FlagsAtStop == ConvertAndFlags(plan.ChildPlan?.AndFlags)
                && plan.ExactCleanExitAdmitted
                && plan.StopAtRawFileOffsetExclusive
                    == CapacityFinalCalleeLargeSavedMaskAndLifecycleProjection
                        .CompletedStopRawFileOffsetExclusive
                && !plan.CommonTailEntered);
        }

        CapacityFinalCalleeLargeSavedMaskAndLifecycleCapture[] missingParentCaptures =
        [
            new(
                new(null, 1, 0x200, 0x80000001),
                FreshGlobalDword: 0xA5A55A5A),
            new(
                new(0x100, null, 0x200, 0x80000001),
                FreshGlobalDword: 0xA5A55A5A),
            new(
                new(0x100, 1, null, 0x80000001),
                FreshGlobalDword: 0xA5A55A5A),
            new(
                new(0x100, 1, 0x200, null),
                FreshGlobalDword: 0xA5A55A5A)
        ];
        CapacityFinalCalleeLargeSavedMaskShiftOutcome[] missingParentOutcomes =
        [
            CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingEntryEDX,
            CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingFreshEDXPlus1C,
            CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingEntryEBP,
            CapacityFinalCalleeLargeSavedMaskShiftOutcome.MissingFreshEBPMinus8
        ];
        for (int index = 0; index < missingParentCaptures.Length; index++)
        {
            CapacityFinalCalleeLargeSavedMaskAndLifecyclePlan plan =
                CapacityFinalCalleeLargeSavedMaskAndLifecycleProjection.Project(
                    missingParentCaptures[index]);
            check($"Missing parent prerequisite {missingParentOutcomes[index]} blocks the child and preserves parent state",
                plan.Outcome
                    == CapacityFinalCalleeLargeSavedMaskAndLifecycleOutcome.ParentIncomplete
                && plan.ParentPlan.Outcome == missingParentOutcomes[index]
                && plan.ChildPlan is null
                && !plan.ChildReached
                && plan.ChildEAX is null
                && !plan.ChildFreshGlobalReadReached
                && plan.UnreachedFreshGlobalDwordCaptureProvided
                && plan.OrderedReads.Length == plan.ParentPlan.OrderedReads.Length
                && plan.OrderedWrites.IsEmpty
                && plan.FlagsAtStop == ConvertShiftFlags(plan.ParentPlan.FlagsAtStop)
                && !plan.ExactCleanExitAdmitted
                && plan.StopAtRawFileOffsetExclusive
                    == plan.ParentPlan.StopAtRawFileOffsetExclusive
                && (index != 2 || plan.ParentPlan.ShiftCountSource == 1)
                && (index != 3 || plan.ParentPlan.ShiftCountMasked == 1));
        }

        CapacityFinalCalleeLargeSavedMaskAndLifecyclePlan missingGlobalCountZero =
            CapacityFinalCalleeLargeSavedMaskAndLifecycleProjection.Project(new(
                ParentCapture: new(
                    EntryEDX: 0x100,
                    FreshEDXPlus1C: 32,
                    EntryEBP: 8,
                    FreshEBPMinus8: 0xA5A55A5A),
                FreshGlobalDword: null));
        check("A missing child global retains the completed parent reads, zero count and unknown shift flags",
            missingGlobalCountZero.ParentPlan.ShiftCountMasked == 0
            && missingGlobalCountZero.ParentPlan.ShiftResult == 0xA5A55A5A
            && missingGlobalCountZero.ChildReached
            && missingGlobalCountZero.ChildFreshGlobalReadReached
            && missingGlobalCountZero.ChildFreshGlobalAddress == 0x01EDADCC
            && !missingGlobalCountZero.ChildFreshGlobalCaptured
            && !missingGlobalCountZero.UnreachedFreshGlobalDwordCaptureProvided
            && missingGlobalCountZero.ChildPlan?.Reads.Length == 1
            && missingGlobalCountZero.ChildPlan?.Writes.IsEmpty == true
            && missingGlobalCountZero.OrderedReads.Length == 3
            && missingGlobalCountZero.OrderedWrites.IsEmpty
            && missingGlobalCountZero.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingGlobalCountZero.ParentFlagsAtStop is
            {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingGlobalCountZero.ParentFlagsPreservedThroughChild
            && !missingGlobalCountZero.ChildAndFlagsReplacedParentFlags
            && missingGlobalCountZero.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeLargeSavedMaskAndLifecycleProjection
                    .ChildEntryRawFileOffset
            && !missingGlobalCountZero.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeSavedMaskAndLifecyclePlan missingGlobalKnownFlags =
            CapacityFinalCalleeLargeSavedMaskAndLifecycleProjection.Project(new(
                ParentCapture: new(
                    EntryEDX: 0x100,
                    FreshEDXPlus1C: 1,
                    EntryEBP: 8,
                    FreshEBPMinus8: 0x80000001),
                FreshGlobalDword: null));
        check("A missing child global preserves known parent shift flags without executing AND",
            missingGlobalKnownFlags.ParentFlagsAtStop is
            {
                Carry: true,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && missingGlobalKnownFlags.FlagsAtStop
                == ConvertShiftFlags(missingGlobalKnownFlags.ParentPlan.FlagsAtStop)
            && missingGlobalKnownFlags.ParentFlagsPreservedThroughChild
            && !missingGlobalKnownFlags.ChildAndFlagsReplacedParentFlags
            && missingGlobalKnownFlags.ChildPlan?.AndExecuted == false
            && missingGlobalKnownFlags.OrderedWrites.IsEmpty);

        CapacityFinalCalleeLargeSavedMaskAndLifecyclePlan wrappedAliases =
            CapacityFinalCalleeLargeSavedMaskAndLifecycleProjection.Project(new(
                ParentCapture: new(
                    EntryEDX: 0xFFFFFFE0,
                    FreshEDXPlus1C: 1,
                    EntryEBP: 4,
                    FreshEBPMinus8: 0x80000001),
                FreshGlobalDword: 0xFFFFFFFF));
        check("Wrapped parent reads preserve aliased captures and remain ordered before the independent global access",
            wrappedAliases.ParentPlan.OrderedReads[0].Address == 0xFFFFFFFC
            && wrappedAliases.ParentPlan.OrderedReads[1].Address == 0xFFFFFFFC
            && wrappedAliases.ParentPlan.OrderedReads[0].Value == 1
            && wrappedAliases.ParentPlan.OrderedReads[1].Value == 0x80000001
            && wrappedAliases.ChildFreshGlobalAddress == 0x01EDADCC
            && wrappedAliases.OrderedReads.Length == 3
            && wrappedAliases.OrderedReads[0].Address
                == wrappedAliases.OrderedReads[1].Address
            && wrappedAliases.OrderedReads[2] is
            {
                Stage: CapacityFinalCalleeLargeSavedMaskAndLifecycleStage.ChildGlobalAnd,
                RawFileOffset: 0x012C925B,
                Address: 0x01EDADCC,
                AddressCaptured: true,
                ValueCaptured: true,
                Value: 0xFFFFFFFF
            }
            && wrappedAliases.OrderedWrites.Length == 1
            && wrappedAliases.OrderedWrites[0] is
            {
                Stage: CapacityFinalCalleeLargeSavedMaskAndLifecycleStage.ChildGlobalAnd,
                RawFileOffset: 0x012C925B,
                Address: 0x01EDADCC,
                Value: 0xFFFFFFFD
            }
            && wrappedAliases.OrderedMemoryAccesses.Length == 4
            && wrappedAliases.OrderedMemoryAccesses[0].Kind
                == CapacityFinalCalleeLargeSavedMaskAndLifecycleAccessKind.Read
            && wrappedAliases.OrderedMemoryAccesses[1].Kind
                == CapacityFinalCalleeLargeSavedMaskAndLifecycleAccessKind.Read
            && wrappedAliases.OrderedMemoryAccesses[2].Kind
                == CapacityFinalCalleeLargeSavedMaskAndLifecycleAccessKind.Read
            && wrappedAliases.OrderedMemoryAccesses[3].Kind
                == CapacityFinalCalleeLargeSavedMaskAndLifecycleAccessKind.Write);

        CapacityFinalCalleeLargeSavedMaskAndLifecyclePlan wrappedToZero =
            CapacityFinalCalleeLargeSavedMaskAndLifecycleProjection.Project(new(
                ParentCapture: new(
                    EntryEDX: 0xFFFFFFE4,
                    FreshEDXPlus1C: 1,
                    EntryEBP: 8,
                    FreshEBPMinus8: 0x80000001),
                FreshGlobalDword: 0xFFFFFFFF));
        check("Both parent address calculations preserve unsigned wraparound to zero",
            wrappedToZero.OrderedReads[0].Address == 0
            && wrappedToZero.OrderedReads[1].Address == 0
            && wrappedToZero.OrderedReads[0].Value == 1
            && wrappedToZero.OrderedReads[1].Value == 0x80000001
            && wrappedToZero.ChildFreshGlobalAddress == 0x01EDADCC);

        CapacityFinalCalleeLargeSavedMaskAndLifecyclePlan completed =
            CapacityFinalCalleeLargeSavedMaskAndLifecycleProjection.Project(new(
                ParentCapture: new(
                    EntryEDX: 0,
                    FreshEDXPlus1C: 0,
                    EntryEBP: 8,
                    FreshEBPMinus8: 0),
                FreshGlobalDword: 0x0F0F0F0F));
        check("The child AND replaces parent flags and stops cleanly at 92BD without entering common tail",
            completed.ChildPlan?.AndResult == 0x0F0F0F0F
            && completed.ChildPlan?.AndFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && completed.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && completed.ChildAndFlagsReplacedParentFlags
            && !completed.ParentFlagsPreservedThroughChild
            && completed.OrderedReads.Length == 3
            && completed.OrderedWrites.Length == 1
            && completed.ExactCleanExitAdmitted
            && completed.StopAtRawFileOffsetExclusive == 0x012C92BD
            && !completed.CommonTailEntered);
    }

    private static CapacityFinalCalleeLargeSavedMaskAndLifecycleFlags? ConvertShiftFlags(
        CapacityFinalCalleeLargeSavedMaskShiftFlags? flags) =>
        flags is CapacityFinalCalleeLargeSavedMaskShiftFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.Auxiliary,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static CapacityFinalCalleeLargeSavedMaskAndLifecycleFlags? ConvertAndFlags(
        CapacityFinalCalleeLargeMaskAndStoreFlags? flags) =>
        flags is CapacityFinalCalleeLargeMaskAndStoreFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;
}
