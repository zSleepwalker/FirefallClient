using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan[] indexedRoutes =
            new[] { 2u, 5u, 31u }
                .SelectMany(index =>
                {
                    uint mask = 1u << (int)index;
                    return new[]
                    {
                        Project(index, maskGlobal: 0, freshSuccessorEDXPlus8: null,
                            freshSuccessorGlobal: null),
                        Project(index, maskGlobal: mask, freshSuccessorEDXPlus8: 0x80000010,
                            freshSuccessorGlobal: 0x80000000)
                    };
                })
                .ToArray();

        check("Indices 2, 5, and 31 join only the exact TEST/JNZ zero and nonzero exits",
            CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleProjection.EntryRawFileOffset
                == CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleProjection
                    .EntryRawFileOffset
            && indexedRoutes.Length == 6
            && indexedRoutes.Where(plan => plan.ZeroRouteSelected).All(plan =>
                plan.ParentTestBranchValidated
                && plan.Parent.ChildMaskSelectionGate?.TestResult == 0
                && plan.Parent.ChildMaskSelectionGate.JnzTaken == false
                && plan.Parent.ChildMaskSelectionGate.JnzSelectedDestinationRawFileOffset
                    == 0x012C931B
                && plan.ZeroRoute?.ParentExactCleanHandoffVerified == true
                && plan.ZeroRoute.Capture.ParentEntryECX == 0
                && plan.ZeroRoute.Capture.ParentEntryEDI
                    == plan.Parent.ChildMaskSelectionGate.DestinationValueAfterShift
                && plan.ZeroRoute.Capture.ChildEntryEDX == plan.Parent.IndexedAddressEDX
                && plan.ZeroRoute.Capture.ChildEntryEAX
                    == plan.Parent.CommonIndexedMaskLifecycle?.ParentDerivedEAX
                && plan.ZeroRoute.Capture.ChildEntryESI
                    == plan.Capture.Parent.Parent.Parent.Parent.LateEntryESI
                && plan.ZeroRoute.Outcome
                    == CapacityFinalCalleeCommonOrRingLifecycleOutcome
                        .CompletedTo012C9441
                && plan.ExactCleanCompletion
                && plan.StopAtRawFileOffsetExclusive == 0x012C9441)
            && indexedRoutes.Where(plan => plan.NonzeroRouteSelected).All(plan =>
                plan.ParentTestBranchValidated
                && plan.Parent.ChildMaskSelectionGate?.JnzTaken == true
                && plan.Parent.ChildMaskSelectionGate.JnzSelectedDestinationRawFileOffset
                    == 0x012C9334
                && plan.NonzeroRoute?.ParentExactBranchHandoff == true
                && plan.NonzeroRoute.Capture.ParentEntryEDX == plan.Parent.IndexedAddressEDX
                && plan.NonzeroRoute.Capture.ParentFreshEDXPlus8
                    == plan.Capture.FreshSuccessorEDXPlus8
                && plan.NonzeroRoute.Capture.ParentFreshGlobal1EDADD8
                    == plan.Capture.FreshSuccessorGlobal1EDADD8
                && plan.NonzeroRoute.Capture.ChildEntryESI
                    == plan.Capture.Parent.Parent.Parent.Parent.LateEntryESI
                && plan.NonzeroRoute.Outcome
                    == CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                        .CompletedTo012C9441
                && plan.ExactCleanCompletion
                && plan.StopAtRawFileOffsetExclusive == 0x012C9441));

        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan originalEDI28 =
            Project(5, maskGlobal: 0x40, freshSuccessorEDXPlus8: null,
                freshSuccessorGlobal: null, originalEDI: 0x28);
        check("The zero route OR uses the shifted mask 20 rather than original EDI 28",
            originalEDI28.Parent.IndexedAddressEDI == 0x28
            && originalEDI28.Parent.ChildMaskSelectionGate?.DestinationValueAfterShift
                == 0x20
            && originalEDI28.Parent.ChildMaskSelectionGate?.TestResult == 0
            && originalEDI28.ZeroRoute?.Capture.ParentEntryECX == 0x40
            && originalEDI28.ZeroRoute?.Capture.ParentEntryEDI == 0x20
            && originalEDI28.ZeroRoute?.ParentPlan?.OrResult == 0x60
            && originalEDI28.ZeroRoute?.GlobalWriteValue == 0x60
            && originalEDI28.ZeroRoute?.GlobalWriteValue != 0x68
            && originalEDI28.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan missingSuccessorEDX =
            Project(5, maskGlobal: 0x20, freshSuccessorEDXPlus8: null,
                freshSuccessorGlobal: 0x200);
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan missingSuccessorGlobal =
            Project(5, maskGlobal: 0x20, freshSuccessorEDXPlus8: 0x200,
                freshSuccessorGlobal: null);
        check("Missing successor reads preserve parent TEST flags until CMP executes",
            missingSuccessorEDX.NonzeroRoute is
            {
                Outcome:
                    CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                        .ParentMissingFreshEDXPlus8,
                StopAtRawFileOffsetExclusive: 0x012C9334,
                Parent.FreshEdxPlus8ReadReached: true,
                Parent.FreshGlobalReadReached: false,
                Child: null
            }
            && missingSuccessorEDX.UnreachedFreshSuccessorGlobal1EDADD8CaptureProvided
            && missingSuccessorEDX.FlagsAtStop
                == ParentTestFlags(missingSuccessorEDX)
            && missingSuccessorEDX.CombinedOrderedMemoryEffects.Last() is
            {
                Kind:
                    CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                        .SuccessorEDXPlus8Read,
                AddressCaptured: true,
                ValueCaptured: false,
                ReadAttempted: true,
                WriteAttempted: false,
                Committed: false
            }
            && missingSuccessorGlobal.NonzeroRoute is
            {
                Outcome:
                    CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                        .ParentMissingFreshGlobal1EDADD8,
                StopAtRawFileOffsetExclusive: 0x012C9337,
                Parent.FreshEdxPlus8ReadReached: true,
                Parent.FreshGlobalReadReached: true,
                Parent.CompareReached: false,
                Child: null
            }
            && missingSuccessorGlobal.FlagsAtStop
                == ParentTestFlags(missingSuccessorGlobal)
            && missingSuccessorGlobal.CombinedOrderedMemoryEffects.TakeLast(2).All(effect =>
                effect.ReadAttempted
                && !effect.WriteAttempted
                && !effect.Committed));

        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan[] thresholdCases =
        {
            Project(5, 0x20, 1, 2),
            Project(5, 0x20, 0x80000000, 0x80000000),
            Project(5, 0x20, 0x80000001, 0x80000000),
            Project(5, 0x20, 0x80000000, 0x7FFFFFFF)
        };
        check("The fresh CMP/JB uses unsigned below, equal, above, and high-bit operands",
            thresholdCases[0].NonzeroRoute?.Parent is
            {
                CompareLeft: 1,
                CompareRight: 2,
                CompareFlags.Carry: true,
                JBTaken: true,
                JBSelectedDestinationRawFileOffset: 0x012C8E76
            }
            && thresholdCases[0].NonzeroRoute?.Child is null
            && thresholdCases[0].NonzeroRoute?.UnreachedChildESICaptureProvided == false
            && thresholdCases[0].ExactCleanCompletion
            && thresholdCases[0].StopAtRawFileOffsetExclusive == 0x012C8E76
            && thresholdCases[0].BranchReached
            && thresholdCases[0].NonzeroBranchTargetRawFileOffset == 0x012C8E76
            && thresholdCases[0].BranchTargetRawFileOffset == 0x012C8E76
            && thresholdCases[1].NonzeroRoute?.Parent is
            {
                CompareLeft: 0x80000000,
                CompareRight: 0x80000000,
                CompareFlags.Carry: false,
                JBTaken: false
            }
            && thresholdCases[1].NonzeroRoute?.Child is not null
            && thresholdCases[1].ExactCleanCompletion
            && thresholdCases[2].NonzeroRoute?.Parent is
            {
                CompareLeft: 0x80000001,
                CompareRight: 0x80000000,
                CompareFlags.Carry: false,
                JBTaken: false
            }
            && thresholdCases[2].ExactCleanCompletion
            && thresholdCases[3].NonzeroRoute?.Parent is
            {
                CompareLeft: 0x80000000,
                CompareRight: 0x7FFFFFFF,
                CompareFlags.Carry: false,
                JBTaken: false
            }
            && thresholdCases[3].ExactCleanCompletion);

        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan missingParentESI =
            Project(5, maskGlobal: 0x20, freshSuccessorEDXPlus8: 0x80000010,
                freshSuccessorGlobal: 0x80000000, lateESI: null);
        check("Missing nested parent ESI stops at 92C2 before either internal child or fresh read",
            missingParentESI.Parent.StopAtRawFileOffsetExclusive == 0x012C92C2
            && missingParentESI.Parent.CommonIndexedMaskLifecycle is null
            && missingParentESI.ZeroRoute is null
            && missingParentESI.NonzeroRoute is null
            && !missingParentESI.Internal931BOrReached
            && !missingParentESI.Internal9334FirstReadAttempted
            && missingParentESI.UnreachedFreshSuccessorEDXPlus8CaptureProvided
            && missingParentESI.UnreachedFreshSuccessorGlobal1EDADD8CaptureProvided
            && !missingParentESI.ExactCleanCompletion
            && missingParentESI.CombinedOrderedMemoryEffects.All(effect =>
                effect.RawFileOffset < 0x012C9334));

        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan aliasing =
            Project(5, maskGlobal: 0x60, freshSuccessorEDXPlus8: 0x01EDAE18,
                freshSuccessorGlobal: 0x01EDAE18, originalEDI: 0x28,
                aliasNestedESIToIndexedBin: true);
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan independentFreshGlobals =
            Project(AliasedParentStoreRoute(), lateMaskGlobal: 0x29,
                freshSuccessorEDXPlus8: 0xF0000000,
                freshSuccessorGlobal: 0x01000000);
        check("Parent effects precede independent fresh reads and aliased stores remain ordered",
            aliasing.NonzeroRoute is not null
            && aliasing.NonzeroRoute.Parent.Reads is
            [
                { RawFileOffset: 0x012C9334, AddressCaptured: true, Value: 0x01EDAE18 },
                { RawFileOffset: 0x012C9337, Address: 0x01EDADD8, Value: 0x01EDAE18 }
            ]
            && aliasing.CombinedOrderedMemoryEffects
                .Take(aliasing.Parent.CombinedOrderedMemoryEffects.Length)
                .Select(effect => effect.RawFileOffset)
                .SequenceEqual(aliasing.Parent.CombinedOrderedMemoryEffects
                    .Select(effect => effect.RawFileOffset))
            && aliasing.CombinedOrderedMemoryEffects
                .Where(effect =>
                    effect.Kind
                        is CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .SuccessorEDXPlus8Read
                            or CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                                .SuccessorGlobal1EDADD8Read)
                .All(effect => !effect.ForwardedFromPriorWrite && !effect.Committed)
            && aliasing.NonzeroRoute.Writes.Length == 4
            && aliasing.NonzeroRoute.Writes[0].Address
                == aliasing.NonzeroRoute.Writes[2].Address
            && aliasing.NonzeroRoute.Writes[1].Address
                == aliasing.NonzeroRoute.Writes[3].Address
            && aliasing.NonzeroRoute.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C9343, 0x012C9346, 0x012C9349, 0x012C934C
                })
            && aliasing.NonzeroRoute.FlagsAtStop == aliasing.NonzeroRoute.Parent.CompareFlags
            && aliasing.ExactCleanCompletion
            && independentFreshGlobals.Parent.ParentWrites.Any(write =>
                write.RawFileOffset == 0x012C92C2
                && write.Address == 0x01EDADC8
                && write.Value == 0x29)
            && independentFreshGlobals.Capture.Parent.LateChildFreshGlobalValue == 0x29
            && independentFreshGlobals.NonzeroRoute?.Parent.Reads is
            [
                { RawFileOffset: 0x012C9334, Value: 0xF0000000 },
                { RawFileOffset: 0x012C9337, Address: 0x01EDADD8, Value: 0x01000000 }
            ]
            && independentFreshGlobals.Capture.FreshSuccessorEDXPlus8
                != independentFreshGlobals.Capture.Parent.LateChildFreshGlobalValue
            && independentFreshGlobals.Capture.FreshSuccessorGlobal1EDADD8
                != independentFreshGlobals.Capture.Parent.LateChildFreshGlobalValue
            && independentFreshGlobals.Capture.FreshSuccessorGlobal1EDADD8
                != independentFreshGlobals.Capture.FreshSuccessorEDXPlus8
            && independentFreshGlobals.Capture.FreshSuccessorGlobal1EDADD8
                != independentFreshGlobals.Capture.Parent.Parent.Parent.FreshRootGlobalDWORD
            && independentFreshGlobals.Capture.FreshSuccessorGlobal1EDADD8
                != independentFreshGlobals.Capture.Parent.Parent.Parent.Parent.FreshGlobalValue
            && independentFreshGlobals.NonzeroRoute.Parent.Reads[0].Value
                != independentFreshGlobals.Parent.ParentWrites
                    .Single(write => write.RawFileOffset == 0x012C92C2).Value
            && independentFreshGlobals.NonzeroRoute.Parent.Reads[1].Value
                != independentFreshGlobals.Capture.Parent.LateChildFreshGlobalValue
            && independentFreshGlobals.NonzeroRoute.Parent.Reads.All(read =>
                read.ValueCaptured && read.AddressCaptured)
            && independentFreshGlobals.NonzeroRoute.Parent.Reads[1].Address
                != 0x01EDADC8
            && independentFreshGlobals.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan low9304 =
            Project(1, maskGlobal: null, freshSuccessorEDXPlus8: 1,
                freshSuccessorGlobal: 2);
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan equalRoot =
            Project(EqualRootParent(), lateMaskGlobal: null,
                freshSuccessorEDXPlus8: 1, freshSuccessorGlobal: 2);
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan large9354 =
            Project(32, maskGlobal: null, freshSuccessorEDXPlus8: 1,
                freshSuccessorGlobal: 2, originalEDI: 0x100);
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan noExternalEntry =
            Project(MissingParentRootValue(), lateMaskGlobal: null,
                freshSuccessorEDXPlus8: 1, freshSuccessorGlobal: 2);
        check("Low, equal-root, large-root, and incomplete parent bypasses do not enter either child",
            low9304.Parent.StopAtRawFileOffsetExclusive == 0x012C9304
            && low9304.Parent.OpaqueCallInstructionAtStop
            && low9304.Parent.ExactCleanCompletion
            && low9304.ZeroRoute is null
            && low9304.NonzeroRoute is null
            && low9304.FreshSuccessorCapturesUnused
            && !low9304.ExactCleanCompletion
            && low9304.ActualExactBoundedCompletion
            && equalRoot.Parent.ParentLifecycle.Outcome
                == CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                    .EqualRootStoredAndJumped
            && equalRoot.ZeroRoute is null
            && equalRoot.NonzeroRoute is null
            && equalRoot.FreshSuccessorCapturesUnused
            && !equalRoot.ExactCleanCompletion
            && equalRoot.ActualExactBoundedCompletion
            && large9354.Parent.StopAtRawFileOffsetExclusive == 0x012C9354
            && large9354.ZeroRoute is null
            && large9354.NonzeroRoute is null
            && large9354.FreshSuccessorCapturesUnused
            && !large9354.ExactCleanCompletion
            && large9354.ActualExactBoundedCompletion
            && noExternalEntry.ZeroRoute is null
            && noExternalEntry.NonzeroRoute is null
            && !noExternalEntry.ParentTestBranchValidated
            && noExternalEntry.FreshSuccessorCapturesUnused
            && !noExternalEntry.ExactBoundedCompletion
            && !noExternalEntry.OuterCalleeEntered);

        CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan zeroAliasedStores =
            Project(5, maskGlobal: 0, freshSuccessorEDXPlus8: null,
                freshSuccessorGlobal: null, aliasNestedESIToIndexedBin: true);
        check("Zero-route global OR and four stores retain ordered effects and OR flags",
            zeroAliasedStores.ZeroRoute?.Writes.Length == 5
            && zeroAliasedStores.ZeroRoute.Writes[0] is
            {
                RawFileOffset: 0x012C931D,
                Address: 0x01EDADC8,
                IsParentGlobalWrite: true,
                Committed: true
            }
            && zeroAliasedStores.ZeroRoute.Writes.Skip(1).Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C9323, 0x012C9326, 0x012C9329, 0x012C932C
                })
            && zeroAliasedStores.ZeroRoute.Writes[1].Address
                == zeroAliasedStores.ZeroRoute.Writes[3].Address
            && zeroAliasedStores.ZeroRoute.Writes[2].Address
                == zeroAliasedStores.ZeroRoute.Writes[4].Address
            && zeroAliasedStores.FlagsAtStop
                is
                {
                    Carry: false,
                    Parity: false,
                    Auxiliary: null,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            && zeroAliasedStores.FlagsAtStop
                == ConvertOrFlags(zeroAliasedStores.ZeroRoute.FlagsAtStop)
            && zeroAliasedStores.ZeroRoute.ChildFlagsUnaffected
            && zeroAliasedStores.BranchTargetRawFileOffset == 0x012C9441
            && zeroAliasedStores.ExactCleanCompletion
            && zeroAliasedStores.Internal931BOrReached
            && !zeroAliasedStores.Internal9334FirstReadAttempted
            && !zeroAliasedStores.DestinationEntered
            && !zeroAliasedStores.EpilogueEntered);

        check("Nonzero threshold flags survive the four alternate stores unchanged",
            aliasing.NonzeroRoute?.Parent.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && aliasing.NonzeroRoute.FlagsAtStop == aliasing.NonzeroRoute.Parent.CompareFlags
            && aliasing.NonzeroRoute.ChildFlagsUnaffected
            && aliasing.Internal9334FirstReadAttempted
            && !aliasing.Internal931BOrReached
            && aliasing.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            });
    }

    private static CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan Project(
        uint index,
        uint? maskGlobal,
        uint? freshSuccessorEDXPlus8,
        uint? freshSuccessorGlobal,
        uint? originalEDI = null,
        uint? lateESI = 0x100,
        bool aliasNestedESIToIndexedBin = false) =>
        Project(
            BelowParent(index, originalEDI ?? (index << 3), lateESI,
                aliasNestedESIToIndexedBin),
            maskGlobal,
            freshSuccessorEDXPlus8,
            freshSuccessorGlobal);

    private static CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan Project(
        CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture parent,
        uint? lateMaskGlobal,
        uint? freshSuccessorEDXPlus8,
        uint? freshSuccessorGlobal) =>
        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleProjection.Project(new(
            new(parent, lateMaskGlobal),
            freshSuccessorEDXPlus8,
            freshSuccessorGlobal));

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture BelowParent(
        uint index,
        uint originalEDI,
        uint? lateESI,
        bool aliasNestedESIToIndexedBin)
    {
        uint? derivedESI = lateESI;
        if (aliasNestedESIToIndexedBin)
        {
            derivedESI = unchecked(0x01EDADF0 + (index << 3));
        }

        return new(
            Parent: new(
                Parent: new(
                    EntryEDX: 0x200,
                    FreshEDXPlus14: 2,
                    FreshGlobalValue: 2,
                    LateEntryEAX: 0x200,
                    LateEntryEDI: originalEDI,
                    LateEntryESI: derivedESI),
                FreshRootGlobalDWORD: 2));
    }

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture
        EqualRootParent() =>
            new(
                Parent: new(
                    Parent: new(
                        EntryEDX: 0x200,
                        FreshEDXPlus14: 0,
                        FreshGlobalValue: null,
                        LateEntryEAX: null,
                        LateEntryEDI: 0x28,
                        LateEntryESI: 0x80),
                    FreshRootGlobalDWORD: 0x80));

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture
        MissingParentRootValue() =>
            new(
                Parent: new(
                    Parent: new(
                        EntryEDX: 0x200,
                        FreshEDXPlus14: 2,
                        FreshGlobalValue: 2,
                        LateEntryEAX: 0x200,
                        LateEntryEDI: 0x28,
                        LateEntryESI: 0x100),
                    FreshRootGlobalDWORD: null));

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture
        AliasedParentStoreRoute() =>
            new(
                Parent: new(
                    Parent: new(
                        EntryEDX: 0x200,
                        FreshEDXPlus14: 2,
                        FreshGlobalValue: 2,
                        LateEntryEAX: 0x200,
                        LateEntryEDI: 0x28,
                        LateEntryESI: 0x01EDADC4),
                    FreshRootGlobalDWORD: 0));

    private static CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleFlags?
        ParentTestFlags(
            CapacityFinalCalleeLargeRightRootSmallBinRingLifecyclePlan plan)
    {
        CapacityFinalCalleeCommonMaskSelectionGateFlags? flags =
            plan.Parent.ChildMaskSelectionGate?.TestFlags;
        return flags is { } value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign,
                value.Overflow)
            : null;
    }

    private static CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleFlags?
        ConvertOrFlags(
            CapacityFinalCalleeCommonMaskOrStoreFlags? flags) =>
        flags is { } value
            ? new(value.Carry, value.Parity, value.AuxiliaryCarry, value.Zero, value.Sign,
                value.Overflow)
            : null;
}
