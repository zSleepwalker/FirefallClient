using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonIndexedCountTagLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        var parentInstructions =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Instructions;
        var childInstructions =
            CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.ProductionInstructionEvidence;
        check("The exact 93D4 parent boundary is the masked-tag child entry",
            CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.EntryRawFileOffset
                == 0x012C93C0
            && CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.ParentHandoffRawFileOffsetExclusive
                == 0x012C93D4
            && CapacityFinalCalleeCommonIndexedCountClassificationProjection
                .BoundaryRawFileOffsetExclusive
                == CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.ChildEntryRawFileOffset
            && CapacityFinalCalleeCommonMaskedTagEqualityGateProjection.EntryRawFileOffset
                == 0x012C93D4
            && parentInstructions[^1].RawFileOffset == 0x012C93D2
            && parentInstructions[^1].Bytes == "2B C1"
            && childInstructions[0].RawFileOffset == 0x012C93D4
            && childInstructions[0].Mnemonic == "MOV ECX, EAX");

        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan missingParentPointer =
            CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.Project(
                new(
                    Parent: new(EntryEBX: null, FreshIndexedDWORD: 0x200, LateEntryECX: 31),
                    LateEntryEDI: 0x80000000,
                    FreshEDXPlus4: 0x80000007));
        check("A blocked parent keeps its unused ECX and child captures out of the handoff",
            missingParentPointer.Outcome
                == CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ParentBlocked
            && !missingParentPointer.ExactParentHandoffCompleted
            && missingParentPointer.ParentPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.MissingEntryEBX
            && missingParentPointer.ParentPlan.UnreachedFreshIndexedDWORDObservationProvided
            && missingParentPointer.ParentPlan.UnreachedLateEntryECXCaptureProvided
            && missingParentPointer.ChildPlan is null
            && !missingParentPointer.ParentHandoffInstructionEntered
            && missingParentPointer.ChildEntryEAX is null
            && missingParentPointer.ChildEntryEDX is null
            && missingParentPointer.LateEntryEDICaptureProvidedButUnused
            && missingParentPointer.FreshEDXPlus4CaptureProvidedButUnused
            && missingParentPointer.OrderedReads.IsEmpty
            && missingParentPointer.StopAtRawFileOffsetExclusive == 0x012C93C0);

        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan missingParentRead =
            CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.Project(
                new(
                    Parent: new(EntryEBX: 0x20, FreshIndexedDWORD: null, LateEntryECX: 30),
                    LateEntryEDI: 0x10,
                    FreshEDXPlus4: 0x4008));
        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan missingParentECX =
            CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.Project(
                new(
                    Parent: new(EntryEBX: 0x24, FreshIndexedDWORD: 0x300, LateEntryECX: null),
                    LateEntryEDI: 0x10,
                    FreshEDXPlus4: 0x4008));
        check("Missing parent observations retain reached reads and never admit late child data",
            missingParentRead.ParentPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingFreshIndexedDWORDAt93C0
            && missingParentRead.ParentPlan.IndexedDWORDReadAttempted
            && missingParentRead.OrderedReads is
            [
                {
                    Stage: CapacityFinalCalleeCommonIndexedCountTagLifecycleReadStage
                        .ParentIndexedDWORDAt93C0,
                    AbsoluteAddress: 0x20,
                    ReadReached: true,
                    ValueCaptured: false
                }
            ]
            && missingParentRead.ParentPlan.UnreachedLateEntryECXCaptureProvided
            && missingParentRead.ChildPlan is null
            && missingParentRead.LateEntryEDICaptureProvidedButUnused
            && missingParentRead.FreshEDXPlus4CaptureProvidedButUnused
            && missingParentRead.StopAtRawFileOffsetExclusive == 0x012C93C0
            && missingParentECX.ParentPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingLateEntryECXAt93C2
            && missingParentECX.ParentPlan.FreshEDX == 0x300
            && missingParentECX.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C93C2
            && missingParentECX.ChildPlan is null
            && missingParentECX.LateEntryEDICaptureProvidedButUnused
            && missingParentECX.FreshEDXPlus4CaptureProvidedButUnused);

        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan equalParent =
            CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.Project(
                new(
                    Parent: new(EntryEBX: 0x204, FreshIndexedDWORD: 0x200, LateEntryECX: 31),
                    LateEntryEDI: 0x80000000,
                    FreshEDXPlus4: 0x80000007));
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags equalParentFlags = new(
            Carry: false,
            Parity: true,
            Auxiliary: null,
            Zero: true,
            Sign: false,
            Overflow: false);
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags childMaskFlags = new(
            Carry: false,
            Parity: true,
            Auxiliary: null,
            Zero: false,
            Sign: true,
            Overflow: false);
        check("Equal-31 handoff derives zero count and preserves flags through child AND/SHL order",
            equalParent.ExactParentHandoffCompleted
            && equalParent.ParentHandoffInstructionEntered
            && !equalParent.DestinationEntered
            && equalParent.ParentPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
            && equalParent.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C93D4
            && equalParent.ParentPlan.JumpSelectedDestinationRawFileOffset == 0x012C93D4
            && equalParent.ParentPlan.UnreachedFreshIndexedDWORDObservationProvided == false
            && equalParent.ParentPlan.UnreachedLateEntryECXCaptureProvided == false
            && equalParent.ChildEntryEAX == 0
            && equalParent.ChildEntryEDX == 0x200
            && equalParent.ChildEntryEDI == 0x80000000
            && equalParent.ChildFreshReadAddress == 0x204
            && equalParent.ChildPlan?.FreshReadAddress == 0x204
            && equalParent.ChildPlan.ReadAttempts[0].Value == 0x80000007
            && equalParent.ChildPlan.EAXValue == 0x80000000
            && equalParent.ChildPlan.ShiftCountMasked == 0
            && equalParent.ChildPlan.ShiftFlags == childMaskFlags
            && equalParent.ChildFlagsAtEntry == equalParentFlags
            && equalParent.ChildOrderedEvents.Length == 7
            && equalParent.ChildOrderedEvents[0].FlagsAfter == equalParentFlags
            && equalParent.ChildOrderedEvents[1].FlagsAfter == equalParentFlags
            && equalParent.ChildOrderedEvents[2].FlagsAfter == equalParentFlags
            && equalParent.ChildOrderedEvents[3].Stage
                == CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.MaskEAXLowTagBits
            && equalParent.ChildOrderedEvents[3].FlagsAfter == childMaskFlags
            && equalParent.ChildOrderedEvents[4].Stage
                == CapacityFinalCalleeCommonMaskedTagEqualityGateInstructionStage.ShiftEBXByCL
            && equalParent.ChildOrderedEvents[4].FlagsAfter == childMaskFlags
            && equalParent.ChildPlan.JzSelectedDestinationRawFileOffset == 0x012C9403
            && equalParent.ChildPlan.ExactCleanBoundedDestination
            && equalParent.ChildBoundedTargetNotEntered
            && equalParent.StopAtRawFileOffsetExclusive == 0x012C9403
            && equalParent.OrderedReads is
            [
                {
                    Stage: CapacityFinalCalleeCommonIndexedCountTagLifecycleReadStage
                        .ParentIndexedDWORDAt93C0,
                    AbsoluteAddress: 0x204,
                    ValueCaptured: true,
                    Value: 0x200
                },
                {
                    Stage: CapacityFinalCalleeCommonIndexedCountTagLifecycleReadStage
                        .ChildTagDWORDAt93D8,
                    AbsoluteAddress: 0x204,
                    ValueCaptured: true,
                    Value: 0x80000007
                }
            ]
            && equalParent.OrderedReads[0].AbsoluteAddress
                == equalParent.OrderedReads[1].AbsoluteAddress);

        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan unequalParent =
            CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.Project(
                new(
                    Parent: new(EntryEBX: 0x100, FreshIndexedDWORD: 0x300, LateEntryECX: 30),
                    LateEntryEDI: 0x10,
                    FreshEDXPlus4: 0x4008));
        check("Non-equal parent subtraction supplies the child count and unequal exit boundary",
            unequalParent.ExactParentHandoffCompleted
            && unequalParent.ParentPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.NonEqualAt93D4
            && unequalParent.ParentPlan.ShiftResultECX == 15
            && unequalParent.ParentPlan.SubtractResult == 10
            && unequalParent.ChildEntryEAX == 10
            && unequalParent.ChildEntryEDX == 0x300
            && unequalParent.ChildEntryEDI == 0x10
            && unequalParent.ChildFreshReadAddress == 0x304
            && unequalParent.ChildPlan?.ShiftCountMasked == 10
            && unequalParent.ChildPlan?.ShiftResult == 0x4000
            && unequalParent.ChildPlan?.MaskResult == 0x4008
            && unequalParent.ChildPlan?.JzTaken == false
            && unequalParent.ChildPlan?.JzSelectedDestinationRawFileOffset == 0x012C93E4
            && unequalParent.ChildBoundedTargetNotEntered
            && unequalParent.StopAtRawFileOffsetExclusive == 0x012C93E4
            && unequalParent.OrderedReads.Length == 2
            && unequalParent.OrderedReads[0].AbsoluteAddress == 0x100
            && unequalParent.OrderedReads[1].AbsoluteAddress == 0x304);

        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan missingLateEDI =
            CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.Project(
                new(
                    Parent: new(EntryEBX: 0x40, FreshIndexedDWORD: 0x500, LateEntryECX: 31),
                    LateEntryEDI: null,
                    FreshEDXPlus4: 0x80000007));
        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan missingChildTag =
            CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.Project(
                new(
                    Parent: new(EntryEBX: 0x44, FreshIndexedDWORD: 0x600, LateEntryECX: 30),
                    LateEntryEDI: 0x10,
                    FreshEDXPlus4: null));
        check("Late EDI and fresh tag observations are consumed only at their reached child reads",
            missingLateEDI.ExactParentHandoffCompleted
            && missingLateEDI.Outcome
                == CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildStoppedAt93D6
            && missingLateEDI.ChildPlan?.Outcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingEntryEDI
            && missingLateEDI.ChildEntryEAX == 0
            && missingLateEDI.ChildEntryEDX == 0x500
            && missingLateEDI.ChildPlan?.EntryEDXOperandConsumed == false
            && missingLateEDI.FreshEDXPlus4CaptureProvidedButUnused
            && missingLateEDI.OrderedReads.Length == 1
            && missingLateEDI.StopAtRawFileOffsetExclusive == 0x012C93D6
            && missingLateEDI.ChildFlagsAtStop == equalParentFlags
            && missingLateEDI.FlagsAtStop == equalParentFlags
            && missingLateEDI.ChildOrderedEvents.All(item => item.FlagsAfter == equalParentFlags)
            && missingChildTag.ExactParentHandoffCompleted
            && missingChildTag.Outcome
                == CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildStoppedAt93D8
            && missingChildTag.ChildPlan?.Outcome
                == CapacityFinalCalleeCommonMaskedTagEqualityGateOutcome.MissingFreshEDXPlus4
            && missingChildTag.ChildEntryEAX == 10
            && missingChildTag.ChildEntryEDX == 0x600
            && missingChildTag.ChildFreshReadAddress == 0x604
            && missingChildTag.ChildPlan?.EAXValue == 10
            && missingChildTag.ChildPlan?.EBXValue == 0x10
            && missingChildTag.ChildPlan?.FreshReadReached == true
            && missingChildTag.ChildFlagsAtStop == missingChildTag.ChildFlagsAtEntry
            && missingChildTag.ChildOrderedEvents[^1].FlagsAfter
                == missingChildTag.ChildFlagsAtEntry
            && missingChildTag.StopAtRawFileOffsetExclusive == 0x012C93D8);

        CapacityFinalCalleeCommonIndexedCountTagLifecyclePlan wrappedTagAddress =
            CapacityFinalCalleeCommonIndexedCountTagLifecycleProjection.Project(
                new(
                    Parent: new(
                        EntryEBX: 0x88,
                        FreshIndexedDWORD: 0xFFFFFFFE,
                        LateEntryECX: 31),
                    LateEntryEDI: 0,
                    FreshEDXPlus4: 0));
        check("Child [EDX+4] address arithmetic wraps independently after the parent read",
            wrappedTagAddress.ExactParentHandoffCompleted
            && wrappedTagAddress.ChildEntryEDX == 0xFFFFFFFE
            && wrappedTagAddress.ChildFreshReadAddress == 2
            && wrappedTagAddress.OrderedReads.Length == 2
            && wrappedTagAddress.OrderedReads[0].AbsoluteAddress == 0x88
            && wrappedTagAddress.OrderedReads[0].Value == 0xFFFFFFFE
            && wrappedTagAddress.OrderedReads[1].AbsoluteAddress == 2
            && wrappedTagAddress.OrderedReads[1].Value == 0
            && wrappedTagAddress.ChildPlan?.FreshReadAddress == 2);
    }
}
