using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonNodeMaskIndexedLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan equalAtIndexZero =
            Project(
                size: 0x100,
                mask: 1,
                freshIndexedDWORD: 0x200,
                freshTag: 0x100);
        check("The clean nonzero 93C0 edge composes the derived index into the fresh indexed/tag lifecycle",
            equalAtIndexZero.ParentNonzeroAdmission
            && equalAtIndexZero.ParentBoundedDestinationSelected
            && !equalAtIndexZero.ParentPlan.NonzeroDestinationInstructionEntered
            && equalAtIndexZero.ChildInternalEntry
            && equalAtIndexZero.ChildInternalEntryRawFileOffset == 0x012C93C0
            && equalAtIndexZero.DerivedIndexECX == 0
            && equalAtIndexZero.DerivedNodeEBX
                == CapacityFinalCalleeCommonIndexedNodeInitializationProjection.DeriveEBX(0)
            && equalAtIndexZero.DerivedParentEntryEDI == 0x100
            && equalAtIndexZero.ChildPlan?.ChildEntryEDI == 0x100
            && equalAtIndexZero.ChildPlan?.ParentPlan.Capture.LateEntryECX == 0
            && equalAtIndexZero.ChildPlan?.ParentPlan.Capture.EntryEBX
                == equalAtIndexZero.DerivedNodeEBX
            && equalAtIndexZero.ChildPlan?.ParentPlan.IndexedDWORDReadAddress
                == equalAtIndexZero.DerivedNodeEBX
            && equalAtIndexZero.ChildPlan?.ParentPlan.FreshEDX == 0x200
            && equalAtIndexZero.ChildPlan?.Outcome
                == CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome
                    .ChildEqualToBounded9403
            && equalAtIndexZero.ChildPlan?.ChildEntryEAX == 25
            && equalAtIndexZero.ChildPlan?.ChildPlan?.ShiftCountMasked == 25
            && equalAtIndexZero.ChildPlan?.ChildPlan?.JzSelectedDestinationRawFileOffset
                == 0x012C9403
            && equalAtIndexZero.ChildBoundedDestinationSelected
            && equalAtIndexZero.ChildBoundedDestinationNotEntered
            && equalAtIndexZero.ChildDestinationNotEntered
            && equalAtIndexZero.ExactCleanCompletion
            && equalAtIndexZero.StopAtRawFileOffsetExclusive == 0x012C9403);

        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan index31CountZero =
            Project(
                size: 0x00FFFF00,
                mask: 0x80000000,
                freshIndexedDWORD: 0x200,
                freshTag: 0x00FFFF00);
        check("The largest in-ceiling input derives index 31 and shifts its original EDI by zero",
            index31CountZero.DerivedIndexECX == 31
            && index31CountZero.DerivedNodeEBX
                == CapacityFinalCalleeCommonIndexedNodeInitializationProjection.DeriveEBX(31)
            && index31CountZero.ChildPlan?.ChildEntryEAX == 0
            && index31CountZero.ChildPlan?.ParentPlan.CompareLeft == 31
            && index31CountZero.ChildPlan?.ParentPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
            && index31CountZero.ChildPlan?.ChildPlan?.ShiftCountMasked == 0
            && index31CountZero.ChildPlan?.ChildPlan?.ShiftInputEBX == 0x00FFFF00
            && index31CountZero.ChildPlan?.ChildPlan?.ShiftResult == 0x00FFFF00
            && index31CountZero.ChildPlan?.ChildPlan?.JzSelectedDestinationRawFileOffset
                == 0x012C9403
            && index31CountZero.ChildBoundedDestinationNotEntered
            && index31CountZero.ExactCleanCompletion
            && index31CountZero.StopAtRawFileOffsetExclusive == 0x012C9403);

        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan index30 =
            Project(
                size: 0x00BFFF00,
                mask: 0x40000000,
                freshIndexedDWORD: 0x220,
                freshTag: 0x00BFFF00);
        check("A proper index-30 fixture follows the non-31 subtract and masked tag shift",
            index30.DerivedIndexECX == 30
            && index30.ChildPlan?.ParentPlan.Capture.LateEntryECX == 30
            && index30.ChildPlan?.ParentPlan.ShiftResultECX == 15
            && index30.ChildPlan?.ParentPlan.SubtractResult == 10
            && index30.ChildPlan?.ChildEntryEAX == 10
            && index30.ChildPlan?.ChildPlan?.ShiftCountMasked == 10
            && index30.ChildPlan?.ChildPlan?.ShiftInputEBX == 0x00BFFF00
            && index30.ChildPlan?.ChildPlan?.ShiftResult == 0xFFFC0000
            && index30.ChildPlan?.ChildPlan?.JzSelectedDestinationRawFileOffset
                == 0x012C9403
            && index30.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan aboveCeilingIndex31 =
            Project(
                size: 0x01000000,
                mask: 0x80000000,
                freshIndexedDWORD: 0x400,
                freshTag: 0x01000000);
        check("Above-ceiling parent ECX=31 is consumed without inventing a duplicate index",
            aboveCeilingIndex31.DerivedIndexECX == 31
            && aboveCeilingIndex31.ParentPlan.ParentLifecycle?.FrameStoreCommitted == false
            && aboveCeilingIndex31.ParentPlan.ParentLifecycle?.LateEntryEBPUnusedOnBypass == false
            && aboveCeilingIndex31.ChildPlan?.ParentPlan.Capture.LateEntryECX == 31
            && aboveCeilingIndex31.ChildPlan?.ParentPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
            && aboveCeilingIndex31.ChildPlan?.ChildPlan?.JzSelectedDestinationRawFileOffset
                == 0x012C9403
            && aboveCeilingIndex31.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan nonzeroSubtract =
            Project(
                size: 0x100,
                mask: 1,
                freshIndexedDWORD: 0x300,
                freshTag: 0);
        check("A non-31 derived index takes the subtract path and the unequal boundary remains unentered",
            nonzeroSubtract.ChildPlan?.ParentPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.NonEqualAt93D4
            && nonzeroSubtract.ChildPlan?.ParentPlan.ShiftResultECX == 0
            && nonzeroSubtract.ChildPlan?.ParentPlan.SubtractResult == 25
            && nonzeroSubtract.ChildPlan?.ChildEntryEAX == 25
            && nonzeroSubtract.ChildPlan?.ChildPlan?.ShiftCountMasked == 25
            && nonzeroSubtract.ChildPlan?.ChildPlan?.JzSelectedDestinationRawFileOffset
                == 0x012C93E4
            && nonzeroSubtract.ChildBoundedDestinationNotEntered
            && nonzeroSubtract.ExactCleanCompletion);

        foreach ((uint size, uint mask) in new[]
        {
            (0u, 0u),
            (0u, 1u),
            (0x100u, 0u),
            (0x100u, 1u),
            (0x00FFFF00u, 0u),
            (0x00FFFF00u, 0x80000000u),
            (0x01000000u, 0u),
            (0x01000000u, 0x80000000u)
        })
        {
            CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan plan =
                Project(size, mask, freshIndexedDWORD: 0x600, freshTag: 0);
            bool parentZeroMask = mask == 0;
            check($"Parent size 0x{size:X} TEST/JNZ selects the exact bounded route for mask 0x{mask:X}",
                plan.ParentPlan.FreshMaskGate?.TestReached == true
                && plan.ParentPlan.FreshMaskGate?.TestResult == mask
                && plan.ParentBoundedDestinationSelected
                && plan.ParentPlan.FreshMaskGate?.JnzTaken == !parentZeroMask
                && plan.ParentPlan.FreshMaskGate?.JnzSelectedDestinationRawFileOffset
                    == (parentZeroMask ? 0x012C93ACu : 0x012C93C0u)
                && plan.ParentPlan.ZeroDestinationInstructionEntered == false
                && plan.ParentPlan.NonzeroDestinationInstructionEntered == false
                && (parentZeroMask
                    ? plan.StopAtRawFileOffsetExclusive == 0x012C93AC
                    : plan.StopAtRawFileOffsetExclusive
                        == plan.ChildPlan?.StopAtRawFileOffsetExclusive)
                && (!parentZeroMask
                    ? plan.ChildInternalEntry
                        && plan.ChildPlan?.OrderedReads[0].RawFileOffset == 0x012C93C0
                        && plan.ChildDestinationNotEntered
                        && plan.ChildPlan?.StopAtRawFileOffsetExclusive
                            is 0x012C93E4 or 0x012C9403
                    : !plan.ChildInternalEntry
                        && plan.ChildPlan is null
                        && plan.OrderedEffects.All(effect =>
                            effect.Kind
                                != CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                                    .FreshIndexedDWORDRead)
                        && plan.FreshIndexedDWORDCaptureProvidedButUnused
                        && plan.FreshEDXPlus4CaptureProvidedButUnused
                        && plan.ExactBoundedCompletion
                        && !plan.ExactCleanCompletion));
        }

        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan cleanZero =
            Project(size: 0, mask: 0, freshIndexedDWORD: null, freshTag: null);
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan zeroParentDirtyCaptures =
            Project(size: 0, mask: 0, freshIndexedDWORD: 0x200, freshTag: 0x100);
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan dirtyUnusedEBP =
            CapacityFinalCalleeCommonNodeMaskIndexedLifecycleProjection.Project(
                new(
                    new(
                        EntryEDI: 0,
                        LateEntryEBP: 0x2000,
                        EntryESI: 0x3000,
                        LateFreshGlobalValue: 1),
                    FreshIndexedDWORD: 0x200,
                    FreshEDXPlus4: 0x100));
        check("A zero TEST stops at 93AC and unreached indexed/tag captures prevent clean completion",
            zeroParentDirtyCaptures.Outcome
                == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ParentZeroToBounded93AC
            && zeroParentDirtyCaptures.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C93AC
            && !zeroParentDirtyCaptures.ParentNonzeroAdmission
            && zeroParentDirtyCaptures.ChildPlan is null
            && zeroParentDirtyCaptures.FreshIndexedDWORDCaptureProvidedButUnused
            && zeroParentDirtyCaptures.FreshEDXPlus4CaptureProvidedButUnused
            && zeroParentDirtyCaptures.ExactBoundedCompletion
            && !zeroParentDirtyCaptures.ExactCleanCompletion
            && zeroParentDirtyCaptures.StopAtRawFileOffsetExclusive == 0x012C93AC
            && cleanZero.Outcome
                == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ParentZeroToBounded93AC
            && cleanZero.ParentBoundedDestinationSelected
            && cleanZero.ExactBoundedCompletion
            && cleanZero.ExactCleanCompletion
            && cleanZero.StopAtRawFileOffsetExclusive == 0x012C93AC
            && dirtyUnusedEBP.ParentPlan.Outcome
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.ParentNotCleanAt93A1
            && dirtyUnusedEBP.ParentPlan.ParentLifecycle?.LateEntryEBPUnusedOnBypass == true
            && dirtyUnusedEBP.Outcome
                == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ParentStopped
            && !dirtyUnusedEBP.ParentPlan.FreshGlobalReadReached
            && dirtyUnusedEBP.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C93A1
            && dirtyUnusedEBP.ChildPlan is null
            && !dirtyUnusedEBP.ParentNonzeroAdmission
            && dirtyUnusedEBP.FreshIndexedDWORDCaptureProvidedButUnused
            && dirtyUnusedEBP.FreshEDXPlus4CaptureProvidedButUnused
            && !dirtyUnusedEBP.ExactCleanCompletion);

        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan missingIndexed =
            Project(size: 0x100, mask: 1, freshIndexedDWORD: null, freshTag: 0x100);
        check("The independent 93C0 read attempt preserves all parent TEST flags when its value is missing",
            missingIndexed.ChildInternalEntry
            && missingIndexed.Outcome
                == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleOutcome.ChildStoppedAt93C0
            && missingIndexed.ChildPlan?.ParentPlan.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingFreshIndexedDWORDAt93C0
            && missingIndexed.ChildPlan?.OrderedReads.Length == 1
            && missingIndexed.OrderedEffects[^1] is
            {
                Kind: CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                    .FreshIndexedDWORDRead,
                RawFileOffset: 0x012C93C0,
                ValueCaptured: false,
                Attempted: true,
                Committed: false,
                ForwardedFromPriorWrite: false
            }
            && FlagsMatch(
                missingIndexed.ParentFlagsAt93C0,
                missingIndexed.ParentPlan.FreshMaskGate?.TestFlags)
            && FlagsMatch(missingIndexed.ParentFlagsAt93C0, missingIndexed.FlagsAtStop)
            && missingIndexed.ParentFlagsPreservedWhenIndexedReadOrCompareMissing
            && missingIndexed.FreshEDXPlus4CaptureProvidedButUnused
            && !missingIndexed.ExactCleanCompletion
            && missingIndexed.StopAtRawFileOffsetExclusive == 0x012C93C0);

        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan missingTag =
            Project(size: 0x100, mask: 1, freshIndexedDWORD: 0x200, freshTag: null);
        check("A missing fresh tag stops at its single read attempt after the index comparison",
            missingTag.ChildPlan?.Outcome
                == CapacityFinalCalleeCommonIndexedCountTagLifecycleOutcome.ChildStoppedAt93D8
            && missingTag.OrderedEffects.Count(effect =>
                effect.Kind == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                    .FreshTagDWORDRead) == 1
            && missingTag.OrderedEffects[^1] is
            {
                Kind: CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                    .FreshTagDWORDRead,
                RawFileOffset: 0x012C93D8,
                ValueCaptured: false,
                Attempted: true,
                Committed: false,
                ForwardedFromPriorWrite: false
            }
            && FlagsMatch(
                missingTag.FlagsAtStop,
                missingTag.ChildPlan?.ChildFlagsAtStop)
            && FlagsMatch(
                missingTag.ChildPlan?.ChildFlagsAtStop,
                missingTag.ChildPlan?.ChildFlagsAtEntry)
            && missingTag.StopAtRawFileOffsetExclusive == 0x012C93D8
            && !missingTag.ExactCleanCompletion
            && !missingTag.Duplicate93D8Read);

        uint indexZeroEBX =
            CapacityFinalCalleeCommonIndexedNodeInitializationProjection.DeriveEBX(0);
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan aliasedStoresAndReads =
            CapacityFinalCalleeCommonNodeMaskIndexedLifecycleProjection.Project(
                new(
                    new(
                        EntryEDI: 0x100,
                        LateEntryEBP: 0x2000,
                        EntryESI: unchecked(indexZeroEBX - 0x10),
                        LateFreshGlobalValue: 1),
                    FreshIndexedDWORD: unchecked(indexZeroEBX - 4),
                    FreshEDXPlus4: 0x100));
        check("Aliased node stores do not forward into independent indexed and tag reads",
            aliasedStoresAndReads.ParentPlan.Writes.Any(write =>
                write.Address == indexZeroEBX && write.Value == 0)
            && aliasedStoresAndReads.OrderedEffects.Where(effect =>
                effect.Address == indexZeroEBX).Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite,
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                        .FreshIndexedDWORDRead,
                    CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.FreshTagDWORDRead
                })
            && aliasedStoresAndReads.OrderedEffects.Where(effect =>
                effect.Address == indexZeroEBX && effect.Kind
                    != CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite)
                .All(effect => !effect.ForwardedFromPriorWrite)
            && aliasedStoresAndReads.ChildPlan?.ParentPlan.FreshEDX == indexZeroEBX - 4
            && aliasedStoresAndReads.OrderedEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                        .FreshIndexedDWORDRead
                && effect.Address == indexZeroEBX
                && effect.Value == indexZeroEBX - 4)
            && aliasedStoresAndReads.ChildPlan?.ChildFreshReadAddress == indexZeroEBX
            && !aliasedStoresAndReads.Duplicate93D8Read);

        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan wrappedTag =
            Project(
                size: 0x100,
                mask: 1,
                freshIndexedDWORD: 0xFFFFFFFE,
                freshTag: 0x100);
        check("The fresh tag address wraps from FFFFFFFE+4 to 2 independently of the indexed slot",
            wrappedTag.ChildPlan?.ChildEntryEDX == 0xFFFFFFFE
            && wrappedTag.ChildPlan?.ChildFreshReadAddress == 2
            && wrappedTag.ChildPlan?.OrderedReads.Length == 2
            && wrappedTag.ChildPlan?.OrderedReads[0].AbsoluteAddress
                == wrappedTag.DerivedNodeEBX
            && wrappedTag.ChildPlan?.OrderedReads[1].AbsoluteAddress == 2
            && wrappedTag.OrderedEffects[^1].Address == 2
            && !wrappedTag.Duplicate93D8Read);

        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan missingGlobal =
            CapacityFinalCalleeCommonNodeMaskIndexedLifecycleProjection.Project(
                new(
                    new(
                        EntryEDI: 0x100,
                        LateEntryEBP: 0x2000,
                        EntryESI: 0x3000,
                        LateFreshGlobalValue: null),
                    FreshIndexedDWORD: 0x200,
                    FreshEDXPlus4: 0x100));
        check("A missing mask global cannot enter 93C0 or consume the newly supplied child captures",
            missingGlobal.ParentPlan.FreshGlobalReadReached
            && !missingGlobal.ParentPlan.FreshGlobalValueCaptured
            && missingGlobal.ParentFlagsAt93C0 is null
            && missingGlobal.OrderedEffects[^1] is
            {
                Kind: CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                    .FreshGlobalRead,
                RawFileOffset: 0x012C93A1,
                Address: 0x01EDADCC,
                Attempted: true,
                Committed: false
            }
            && FlagsMatch(missingGlobal.FlagsAtStop, missingGlobal.ParentPlan.FlagsAtStop)
            && missingGlobal.ChildPlan is null
            && missingGlobal.FreshIndexedDWORDCaptureProvidedButUnused
            && missingGlobal.FreshEDXPlus4CaptureProvidedButUnused
            && !missingGlobal.ChildInternalEntry
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C93A1);

        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan ordered =
            Project(size: 0x100, mask: 1, freshIndexedDWORD: 0x200, freshTag: 0x100);
        check("Memory effects preserve parent writes, fresh global read, indexed read, then one tag read",
            ordered.OrderedEffects.Select(effect => effect.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite,
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite,
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite,
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite,
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.FreshGlobalRead,
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.FreshIndexedDWORDRead,
                CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.FreshTagDWORDRead
            })
            && ordered.OrderedEffects[^2].RawFileOffset == 0x012C93C0
            && ordered.OrderedEffects[^1].RawFileOffset == 0x012C93D8
            && ordered.OrderedEffects[4] is
            {
                Kind: CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind
                    .FreshGlobalRead,
                RawFileOffset: 0x012C93A1,
                Address: 0x01EDADCC,
                Attempted: true,
                Committed: false,
                ForwardedFromPriorWrite: false
            }
            && !ordered.Duplicate93D8Read);

        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan missingFramePointer =
            CapacityFinalCalleeCommonNodeMaskIndexedLifecycleProjection.Project(
                new(
                    new(
                        EntryEDI: 0x100,
                        LateEntryEBP: null,
                        EntryESI: 0x3000,
                        LateFreshGlobalValue: 1),
                    FreshIndexedDWORD: 0x200,
                    FreshEDXPlus4: 0x100));
        CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan missingNodeBase =
            CapacityFinalCalleeCommonNodeMaskIndexedLifecycleProjection.Project(
                new(
                    new(
                        EntryEDI: 0x100,
                        LateEntryEBP: 0x2000,
                        EntryESI: null,
                        LateFreshGlobalValue: 1),
                    FreshIndexedDWORD: 0x200,
                    FreshEDXPlus4: 0x100));
        check("Ordered effects retain attempted versus committed status at partial parent stores",
            missingFramePointer.ParentPlan.ParentLifecycle?.FrameStoreAttempted == true
            && missingFramePointer.ParentPlan.ParentLifecycle?.FrameStoreCommitted == false
            && missingFramePointer.OrderedEffects is
            [
                {
                    Kind: CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite,
                    RawFileOffset: 0x012C9374,
                    Attempted: true,
                    Committed: false
                }
            ]
            && missingFramePointer.StopAtRawFileOffsetExclusive == 0x012C9374
            && missingNodeBase.ParentPlan.ParentLifecycle?.FrameStoreCommitted == true
            && missingNodeBase.OrderedEffects is
            [
                {
                    Kind: CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite,
                    RawFileOffset: 0x012C9374,
                    Attempted: true,
                    Committed: true
                },
                {
                    Kind: CapacityFinalCalleeCommonNodeMaskIndexedLifecycleEffectKind.ParentWrite,
                    RawFileOffset: 0x012C9384,
                    Attempted: true,
                    Committed: false
                }
            ]
            && missingNodeBase.StopAtRawFileOffsetExclusive == 0x012C9384);
    }

    private static CapacityFinalCalleeCommonNodeMaskIndexedLifecyclePlan Project(
        uint size,
        uint mask,
        uint? freshIndexedDWORD,
        uint? freshTag) =>
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleProjection.Project(
            new(
                new(
                    EntryEDI: size,
                    LateEntryEBP: size is 0 or 0x01000000 ? null : 0x2000,
                    EntryESI: 0x3000,
                    LateFreshGlobalValue: mask),
                FreshIndexedDWORD: freshIndexedDWORD,
                FreshEDXPlus4: freshTag));

    private static bool FlagsMatch(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? actual,
        CapacityFinalCalleeCommonFreshMaskGateFlags? expected) =>
        actual is { } left
        && expected is { } right
        && left.Carry == right.Carry
        && left.Parity == right.Parity
        && left.Auxiliary == right.Auxiliary
        && left.Zero == right.Zero
        && left.Sign == right.Sign
        && left.Overflow == right.Overflow;

    private static bool FlagsMatch(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? actual,
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? expected) =>
        actual is { } left
        && expected is { } right
        && left.Carry == right.Carry
        && left.Parity == right.Parity
        && left.Auxiliary == right.Auxiliary
        && left.Zero == right.Zero
        && left.Sign == right.Sign
        && left.Overflow == right.Overflow;

    private static bool FlagsMatch(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? actual,
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? expected) =>
        actual is { } left
        && expected is { } right
        && left.Carry == right.Carry
        && left.Parity == right.Parity
        && left.Auxiliary == right.Auxiliary
        && left.Zero == right.Zero
        && left.Sign == right.Sign
        && left.Overflow == right.Overflow;

    private static bool FlagsMatch(
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? actual,
        CapacityFinalCalleeCommonMaskedTagEqualityGateFlags? expected) =>
        actual is { } left
        && expected is { } right
        && left.Carry == right.Carry
        && left.Parity == right.Parity
        && left.Auxiliary == right.Auxiliary
        && left.Zero == right.Zero
        && left.Sign == right.Sign
        && left.Overflow == right.Overflow;

    private static bool FlagsMatch(
        CapacityFinalCalleeCommonNodeMaskIndexedLifecycleFlags? actual,
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? expected) =>
        actual is { } left
        && expected is { } right
        && left.Carry == right.Carry
        && left.Parity == right.Parity
        && left.Auxiliary == right.Auxiliary
        && left.Zero == right.Zero
        && left.Sign == right.Sign
        && left.Overflow == right.Overflow;
}
