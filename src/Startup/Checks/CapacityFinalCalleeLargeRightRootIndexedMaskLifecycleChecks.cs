using System;
using System.Buffers.Binary;
using System.Linq;
using System.Numerics;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleProjection;

internal static class CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed windows retain the verified indexed-address and mask bytes",
            InstructionEvidenceMatches());

        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan[] countCases =
            Enumerable.Range(0, 32)
                .Select(index =>
                {
                    uint count = (uint)index;
                    uint edi = count << 3;
                    uint? lateGlobal = count < 2 ? null : 1u << (int)count;
                    return Project(BelowRoute(edi), lateGlobal);
                })
                .ToArray();

        check("Only the exact below-92F6 parent edge supplies counts 0 through 31 and original EDI",
            countCases.Select((plan, index) =>
                plan.ParentLifecycle.Outcome
                    == CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                        .UnequalBelowUnsignedThresholdTo92F6
                && plan.ExactCleanParentAdmission
                && plan.ParentSizeGateCount == (uint)index
                && plan.ParentDerivedEDI == (uint)index << 3
                && plan.ParentLifecycle.SizeGateCount
                    == plan.ParentDerivedEDI >> 3
                && plan.ChildIndexAndEDIUseParentOperands
                && plan.IndexedAddressECX == (uint)index
                && plan.IndexedAddressEDI == plan.ParentDerivedEDI
                && plan.IndexedAddressEDX
                    == CapacityFinalCalleeCommonIndexedAddressGateProjection
                        .DeriveAddress((uint)index)
                && plan.ParentIndexedCompareFlags
                    == ExpectedIndexedCompareFlags(plan.ParentDerivedEDI!.Value)
                && plan.FlagsAtParent92F6
                    == ConvertSizeCompareFlags(plan.ParentLifecycle.UnequalSizeGate!.CompareFlags)
                && plan.IndexedLEAAttempted
                && plan.IndexedLEAIsAddressOnly
                && plan.Internal92F6Entered
                && plan.ParentFlagsPreservedThroughLEAAndMOV
                && (index >= 2
                    || plan.FlagsAtStop == plan.ParentIndexedCompareFlags)
                && plan.ExactCleanCompletion
                && plan.ParentWrites.SequenceEqual(plan.ParentLifecycle.OrderedWrites)
                && plan.IndexedMaskWrites.IsEmpty
                && !plan.OpaqueCallEntered
                && !plan.OuterZeroDestinationEntered
                && !plan.OuterNonzeroDestinationEntered
                && !plan.OuterCalleeEntered
                && (index < 2
                    ? plan.Outcome
                            == CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                                .UnequalBelowStopsBeforeOpaque9304
                        && !plan.Indexed930AMoveAttempted
                        && !plan.LateChildFreshGlobalReadAttempted
                        && plan.OpaqueCallInstructionAtStop
                        && plan.StopAtRawFileOffsetExclusive == 0x012C9304
                    : plan.Outcome
                            == CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                                .UnequalBelowNonzeroTestToBounded9334
                        && plan.Indexed930AMoveAttempted
                        && plan.Internal930AEntered
                        && plan.ChildMaskSelectionGate?.ShiftCount == (uint)index
                        && plan.ChildMaskSelectionGate?.DestinationValueAfterShift
                            == 1u << index
                        && plan.LateChildFreshGlobalReadAttempted
                        && plan.LateChildFreshGlobalReadAddress == 0x01EDADC8
                        && plan.ChildMaskSelectionGate?.TestResult == 1u << index
                        && plan.StopAtRawFileOffsetExclusive == 0x012C9334))
                .All(result => result));

        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan zeroSelected =
            Project(BelowRoute(0x28), lateChildGlobal: 0x20);
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan zeroRejected =
            Project(BelowRoute(0x28), lateChildGlobal: ~0x20u);
        check("The independent late global selects only 931B or 9334 after the mask TEST",
            zeroSelected.ExactCleanCompletion
            && zeroSelected.Outcome
                == CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                    .UnequalBelowNonzeroTestToBounded9334
            && zeroSelected.ChildMaskSelectionGate?.TestResult == 0x20
            && zeroSelected.ChildMaskSelectionGate?.JnzTaken == true
            && zeroSelected.StopAtRawFileOffsetExclusive == 0x012C9334
            && zeroRejected.ExactCleanCompletion
            && zeroRejected.Outcome
                == CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                    .UnequalBelowZeroTestToBounded931B
            && zeroRejected.ChildMaskSelectionGate?.TestResult == 0
            && zeroRejected.ChildMaskSelectionGate?.JnzTaken == false
            && zeroRejected.StopAtRawFileOffsetExclusive == 0x012C931B
            && !zeroSelected.OuterNonzeroDestinationEntered
            && !zeroRejected.OuterZeroDestinationEntered
            && !zeroSelected.OuterCalleeEntered
            && !zeroRejected.OuterCalleeEntered);

        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan missingLateGlobal =
            Project(BelowRoute(0x28), lateChildGlobal: null);
        check("A missing fresh mask global stops after SHL and retains its flags, not the old CMP flags",
            missingLateGlobal.ExactCleanParentAdmission
            && missingLateGlobal.Indexed930AMoveAttempted
            && missingLateGlobal.LateChildFreshGlobalReadAttempted
            && missingLateGlobal.LateChildFreshGlobalReadAddress == 0x01EDADC8
            && !missingLateGlobal.LateChildFreshGlobalValueCaptured
            && !missingLateGlobal.ChildMaskSelectionGate!.TestReached
            && missingLateGlobal.MissingFreshGlobalPreservesShiftFlags
            && missingLateGlobal.FlagsAtStop
                == missingLateGlobal.ChildMaskSelectionGate?.ShiftFlags
            && missingLateGlobal.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && missingLateGlobal.FlagsAtStop != missingLateGlobal.FlagsAtParent92F6
            && !missingLateGlobal.ExactCleanAdmission
            && !missingLateGlobal.ActualExactBoundedCompletion
            && missingLateGlobal.StopAtRawFileOffsetExclusive == 0x012C9311
            && !missingLateGlobal.OpaqueCallEntered
            && !missingLateGlobal.OuterCalleeEntered);

        CapacityFinalCalleeCommonIndexedMaskLifecyclePlan missingIndexedECX =
            CapacityFinalCalleeCommonIndexedMaskLifecycleProjection.Project(new(
                new(EntryECX: null, EntryEDI: 0x10),
                FreshGlobalValue: 1));
        CapacityFinalCalleeCommonIndexedMaskLifecyclePlan missingIndexedEDI =
            CapacityFinalCalleeCommonIndexedMaskLifecycleProjection.Project(new(
                new(EntryECX: 2, EntryEDI: null),
                FreshGlobalValue: 1));
        check("Missing ECX stops before LEA while missing EDI follows completed LEA and MOV",
            missingIndexedECX.Outcome
                == CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ParentMissingEntryECX
            && !missingIndexedECX.ParentAddressGate.LEAReached
            && !missingIndexedECX.ChildReached
            && missingIndexedECX.UnreachedChildGlobalCaptureProvided
            && missingIndexedECX.StopAtRawFileOffsetExclusive == 0x012C92F6
            && missingIndexedEDI.Outcome
                == CapacityFinalCalleeCommonIndexedMaskLifecycleOutcome.ParentMissingEntryEDI
            && missingIndexedEDI.ParentAddressGate.LEAReached
            && missingIndexedEDI.ParentAddressGate.MoveEAXReached
            && !missingIndexedEDI.ParentAddressGate.CompareReached
            && !missingIndexedEDI.ChildReached
            && missingIndexedEDI.UnreachedChildGlobalCaptureProvided
            && missingIndexedEDI.StopAtRawFileOffsetExclusive == 0x012C92FF);

        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan wrappedStores =
            Project(BelowRoute(0x28, lateESI: 0xFFFFFFFC), lateChildGlobal: null);
        check("Parent wrapping writes remain ordered before the independent late read",
            wrappedStores.ParentBelow92F6Admission
            && wrappedStores.ParentReads.SequenceEqual(
                wrappedStores.ParentLifecycle.Reads)
            && wrappedStores.ParentWrites.SequenceEqual(
                wrappedStores.ParentLifecycle.OrderedWrites)
            && wrappedStores.ParentWrites.Select(write => write.Address)
                .Contains(0u)
            && wrappedStores.ParentWrites.Select(write => write.Address)
                .Contains(0x24u)
            && wrappedStores.ParentWrites.All(write =>
                write.RawFileOffset
                    < CapacityFinalCalleeCommonMaskSelectionGateProjection
                        .FreshGlobalMoveRawFileOffset)
            && wrappedStores.LateChildFreshGlobalReadAttempted
            && wrappedStores.LateChildFreshGlobalReadAddress == 0x01EDADC8
            && wrappedStores.IndexedMaskWrites.IsEmpty
            && wrappedStores.MissingFreshGlobalPreservesShiftFlags
            && wrappedStores.StopAtRawFileOffsetExclusive == 0x012C9311);

        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan aliasedLateRead =
            Project(
                BelowRoute(0x28, lateESI: 0x01EDADC4, freshRootGlobal: 0),
                lateChildGlobal: 0x29);
        check("The late 1EDADC8 read independently observes the prior aliased parent store",
            aliasedLateRead.ExactCleanCompletion
            && aliasedLateRead.ParentWrites.Any(write =>
                write.RawFileOffset == 0x012C92C2
                && write.Address == 0x01EDADC8
                && write.Value == 0x29)
            && aliasedLateRead.ParentReads.All(read =>
                read.RawFileOffset
                    < CapacityFinalCalleeCommonMaskSelectionGateProjection
                        .FreshGlobalMoveRawFileOffset)
            && aliasedLateRead.ParentWrites.All(write =>
                write.RawFileOffset
                    < CapacityFinalCalleeCommonMaskSelectionGateProjection
                        .FreshGlobalMoveRawFileOffset)
            && aliasedLateRead.Capture.LateChildFreshGlobalValue == 0x29
            && aliasedLateRead.LateChildFreshGlobalReadAttempted
            && aliasedLateRead.LateChildFreshGlobalReadAddress == 0x01EDADC8
            && aliasedLateRead.LateChildFreshGlobalValueCaptured
            && aliasedLateRead.ChildMaskSelectionGate?.TestResult == 0x20
            && aliasedLateRead.CombinedOrderedMemoryEffects.Last().Value == 0x29
            && aliasedLateRead.CombinedOrderedMemoryEffects.Last().ValueCaptured
            && !aliasedLateRead.CombinedOrderedMemoryEffects.Last().ForwardedFromPriorWrite
            && aliasedLateRead.StopAtRawFileOffsetExclusive == 0x012C9334
            && aliasedLateRead.ParentWrites.SequenceEqual(
                aliasedLateRead.ParentLifecycle.OrderedWrites)
            && aliasedLateRead.IndexedMaskWrites.IsEmpty);

        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan dirtyLowRoute =
            Project(BelowRoute(8), lateChildGlobal: 1);
        check("An unread fresh child capture contaminates the otherwise bounded 9304 stop",
            dirtyLowRoute.Outcome
                == CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                    .UnequalBelowStopsBeforeOpaque9304WithUnusedFreshGlobalValue
            && dirtyLowRoute.UnreachedLateChildFreshGlobalCaptureProvided
            && dirtyLowRoute.CommonIndexedMaskLifecycle?
                .UnreachedChildGlobalCaptureProvided == true
            && !dirtyLowRoute.LateChildFreshGlobalReadAttempted
            && dirtyLowRoute.OpaqueCallInstructionAtStop
            && !dirtyLowRoute.OpaqueCallEntered
            && dirtyLowRoute.ExactBoundedCompletion
            && dirtyLowRoute.ActualExactBoundedCompletion
            && !dirtyLowRoute.ExactCleanCompletion
            && !dirtyLowRoute.ExactCleanAdmission
            && dirtyLowRoute.StopAtRawFileOffsetExclusive == 0x012C9304);

        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan equalParent =
            Project(EqualRoute(), lateChildGlobal: 0xFFFFFFFF);
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan largeParent =
            Project(BelowRoute(0x100), lateChildGlobal: 1);
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan missingRootValue =
            Project(MissingRootValueRoute(), lateChildGlobal: 1);
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan dirtyParent =
            Project(DirtyParentRoute(), lateChildGlobal: 1);
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan cleanEqualParent =
            Project(EqualRoute(), lateChildGlobal: null);
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan cleanLargeParent =
            Project(BelowRoute(0x100), lateChildGlobal: null);
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan missingBeforeOR =
            Project(new(
                Parent: new(
                    Parent: new(
                        EntryEDX: null,
                        FreshEDXPlus14: 0,
                        FreshGlobalValue: null,
                        LateEntryEAX: null,
                        LateEntryEDI: 0x28,
                        LateEntryESI: 0x80),
                    FreshRootGlobalDWORD: 1)),
                lateChildGlobal: null);
        check("Equal, above-threshold, missing, and dirty parent routes do not consume child captures",
            equalParent.Outcome
                == CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                    .ParentOtherRouteWithUnusedLateChildFreshGlobalValue
            && equalParent.ParentLifecycle.Outcome
                == CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                    .EqualRootStoredAndJumped
            && largeParent.Outcome
                == CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                    .ParentOtherRouteWithUnusedLateChildFreshGlobalValue
            && largeParent.ParentLifecycle.Outcome
                == CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                    .UnequalAtOrAboveUnsignedThresholdTo9354
            && missingRootValue.Outcome
                == CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                    .ParentOtherRouteWithUnusedLateChildFreshGlobalValue
            && dirtyParent.Outcome
                == CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleOutcome
                    .ParentOtherRouteWithUnusedLateChildFreshGlobalValue
            && new[] { equalParent, largeParent, missingRootValue, dirtyParent }.All(plan =>
                plan.UnreachedLateChildFreshGlobalCaptureProvided
                && plan.CommonIndexedMaskLifecycle is null
                && !plan.LateChildFreshGlobalReadAttempted
                && !plan.ExactCleanParentAdmission
                && !plan.ExactCleanAdmission
                && plan.ParentReads.SequenceEqual(plan.ParentLifecycle.Reads)
                && plan.ParentWrites.SequenceEqual(plan.ParentLifecycle.OrderedWrites))
            && equalParent.ExactBoundedCompletion
            && equalParent.ActualExactBoundedCompletion
            && !equalParent.ExactCleanCompletion
            && largeParent.ExactBoundedCompletion
            && largeParent.ActualExactBoundedCompletion
            && !largeParent.ExactCleanCompletion
            && !missingRootValue.ExactBoundedCompletion
            && !dirtyParent.ExactBoundedCompletion
            && cleanEqualParent.CleanBypass
            && cleanEqualParent.ExactBoundedCompletion
            && cleanEqualParent.ActualExactBoundedCompletion
            && cleanEqualParent.ExactCleanCompletion
            && !cleanEqualParent.UnreachedLateChildFreshGlobalCaptureProvided
            && cleanEqualParent.CommonIndexedMaskLifecycle is null
            && cleanEqualParent.StopAtRawFileOffsetExclusive == 0x012C9441
            && cleanLargeParent.CleanBypass
            && cleanLargeParent.ExactBoundedCompletion
            && cleanLargeParent.ActualExactBoundedCompletion
            && cleanLargeParent.ExactCleanCompletion
            && !cleanLargeParent.UnreachedLateChildFreshGlobalCaptureProvided
            && cleanLargeParent.CommonIndexedMaskLifecycle is null
            && cleanLargeParent.StopAtRawFileOffsetExclusive == 0x012C9354);

        check("A missing parent input before OR preserves six unknown flags without a child",
            missingBeforeOR.CommonIndexedMaskLifecycle is null
            && missingBeforeOR.ParentLifecycle.FlagsAtStop is null
            && missingBeforeOR.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && missingBeforeOR.ParentFlagsPreservedWithoutChild
            && !missingBeforeOR.CleanBypass
            && !missingBeforeOR.ActualExactBoundedCompletion);

        check("Combined effects interleave parent reads and writes before the independent late read",
            new[] { wrappedStores, aliasedLateRead }.All(plan =>
                ParentEffectsAreInterleaved(plan)
                && plan.CombinedOrderedMemoryEffects.Last() is
                {
                    Kind:
                        CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffectKind
                            .LateChildFreshGlobalRead,
                    RawFileOffset: 0x012C9311,
                    Address: 0x01EDADC8,
                    AddressCaptured: true,
                    ReadAttempted: true,
                    WriteAttempted: false,
                    Committed: false,
                    ForwardedFromPriorWrite: false
                }
                && plan.CombinedOrderedMemoryEffects
                    .Where(effect =>
                        effect.Kind
                            == CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffectKind
                                .ParentRead)
                    .All(effect =>
                        effect.ReadAttempted
                        && !effect.WriteAttempted
                        && !effect.Committed
                        && !effect.ForwardedFromPriorWrite)
                && plan.CombinedOrderedMemoryEffects
                    .Where(effect =>
                        effect.Kind
                            == CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffectKind
                                .ParentWrite)
                    .All(effect =>
                        !effect.ReadAttempted
                        && effect.WriteAttempted
                        && effect.Committed
                        && !effect.ForwardedFromPriorWrite)));

        check("The composition stops before the opaque call, mask destinations, or outer callee",
            countCases.Append(zeroSelected)
                .Append(zeroRejected)
                .Append(missingLateGlobal)
                .Append(wrappedStores)
                .Append(aliasedLateRead)
                .Append(dirtyLowRoute)
                .Append(equalParent)
                .Append(largeParent)
                .Append(missingRootValue)
                .Append(dirtyParent)
                .Append(cleanEqualParent)
                .Append(cleanLargeParent)
                .Append(missingBeforeOR)
                .All(plan =>
                    !plan.OpaqueCallEntered
                    && !plan.OuterZeroDestinationEntered
                    && !plan.OuterNonzeroDestinationEntered
                    && !plan.OuterCalleeEntered
                    && plan.IndexedMaskWrites.IsEmpty));
    }

    private static CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan Project(
        CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture parent,
        uint? lateChildGlobal) =>
        Projection.Project(new(parent, lateChildGlobal));

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture BelowRoute(
        uint lateEntryEDI,
        uint lateESI = 0x100,
        uint? freshRootGlobal = 0x101) =>
        new(new(
            Parent: new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 2,
                FreshGlobalValue: 2,
                LateEntryEAX: 0x200,
                LateEntryEDI: lateEntryEDI,
                LateEntryESI: lateESI),
            FreshRootGlobalDWORD: freshRootGlobal));

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture EqualRoute() =>
        new(new(
            Parent: new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0,
                FreshGlobalValue: null,
                LateEntryEAX: null,
                LateEntryEDI: 0x28,
                LateEntryESI: 0x80),
            FreshRootGlobalDWORD: 0x80));

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture
        MissingRootValueRoute() =>
            new(new(
                Parent: new(
                    EntryEDX: 0x200,
                    FreshEDXPlus14: 2,
                    FreshGlobalValue: 2,
                    LateEntryEAX: 0x200,
                    LateEntryEDI: 0x28,
                    LateEntryESI: 0x100),
                FreshRootGlobalDWORD: null));

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture
        DirtyParentRoute() =>
            new(new(
                Parent: new(
                    EntryEDX: 0x200,
                    FreshEDXPlus14: 0,
                    FreshGlobalValue: 0x10,
                    LateEntryEAX: null,
                    LateEntryEDI: 0x28,
                    LateEntryESI: 0x80),
                FreshRootGlobalDWORD: 0x80));

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes, uint? Target)[]
        {
            (0x012C92F6, "8D 14 CD F0 AD ED 01", null),
            (0x012C92FD, "8B C2", null),
            (0x012C92FF, "83 FF 10", null),
            (0x012C9302, "73 06", 0x012C930A),
            (0x012C9304, "FF 15 24 4A AC 01", null),
            (0x012C930A, "BF 01 00 00 00", null),
            (0x012C930F, "D3 E7", null),
            (0x012C9311, "8B 0D C8 AD ED 01", null),
            (0x012C9317, "85 F9", null),
            (0x012C9319, "75 19", 0x012C9334)
        };

        foreach (CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleInstructionEvidence
            instruction in Projection.Instructions)
        {
            byte[] instructionBytes = Convert.FromHexString(Normalize(instruction.Bytes));
            if (instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != Projection.RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != Projection.RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            if (instruction.BranchTargetRawFileOffset is uint branchTarget)
            {
                int displacement = instructionBytes.Length == 2
                    ? unchecked((sbyte)instructionBytes[1])
                    : BinaryPrimitives.ReadInt32LittleEndian(
                        instructionBytes.AsSpan(instructionBytes.Length - 4, 4));
                if ((long)instruction.RawFileOffset + instructionBytes.Length + displacement
                    != branchTarget)
                {
                    return false;
                }
            }
        }

        uint[] composedOffsets = expected.Select(item => item.Raw).ToArray();
        return Projection.EntryRawFileOffset
                == CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection
                    .EntryRawFileOffset
            && Projection.IndexedAddressEntryRawFileOffset == 0x012C92F6
            && Projection.MaskSelectionEntryRawFileOffset == 0x012C930A
            && Projection.OpaqueCallRawFileOffset == 0x012C9304
            && Projection.ZeroDestinationRawFileOffset == 0x012C931B
            && Projection.NonzeroDestinationRawFileOffset == 0x012C9334
            && Projection.OuterCalleeEntryRawFileOffset == 0x012C8E76
            && Projection.FreshGlobalVirtualAddress == 0x01EDADC8
            && Normalize(Projection.CapturedIndexedAddressWindowBytes)
                == "8D14CDF0ADED018BC283FF107306FF15244AAC01"
            && Normalize(Projection.CapturedMaskSelectionWindowBytes)
                == "BF01000000D3E78B0DC8ADED0185F97519"
            && Projection.Instructions
                .Where(instruction =>
                    instruction.RawFileOffset >= 0x012C92F6
                    && instruction.RawFileOffset <= 0x012C9319)
                .Select(instruction => instruction.RawFileOffset)
                .SequenceEqual(composedOffsets)
            && expected.All(item =>
            {
                CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleInstructionEvidence[]
                    matches = Projection.Instructions.Where(instruction =>
                        instruction.RawFileOffset == item.Raw).ToArray();
                return matches.Length == 1
                    && Normalize(matches[0].Bytes) == Normalize(item.Bytes)
                    && matches[0].BranchTargetRawFileOffset == item.Target;
            })
            && !Projection.Instructions.Any(instruction =>
                instruction.RawFileOffset >= 0x012C931B);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static bool ParentEffectsAreInterleaved(
        CapacityFinalCalleeLargeRightRootIndexedMaskLifecyclePlan plan)
    {
        uint[] expectedOffsets = plan.ParentReads
            .Select(read => read.RawFileOffset)
            .Concat(plan.ParentWrites.Select(write => write.RawFileOffset))
            .OrderBy(rawFileOffset => rawFileOffset)
            .ToArray();
        uint[] actualOffsets = plan.CombinedOrderedMemoryEffects
            .Where(effect =>
                effect.Kind
                    != CapacityFinalCalleeLargeRightRootIndexedMaskLifecycleMemoryEffectKind
                        .LateChildFreshGlobalRead)
            .Select(effect => effect.RawFileOffset)
            .ToArray();
        return actualOffsets.SequenceEqual(expectedOffsets)
            && plan.CombinedOrderedMemoryEffects
                .Take(actualOffsets.Length)
                .All(effect =>
                    effect.RawFileOffset
                        < CapacityFinalCalleeCommonMaskSelectionGateProjection
                            .FreshGlobalMoveRawFileOffset);
    }

    private static CapacityFinalCalleeCommonMaskSelectionGateFlags
        ExpectedIndexedCompareFlags(uint left)
    {
        uint right = CapacityFinalCalleeCommonIndexedAddressGateProjection.CompareThreshold;
        uint result = unchecked(left - right);
        return new(
            Carry: left < right,
            Parity: (BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static CapacityFinalCalleeCommonMaskSelectionGateFlags?
        ConvertSizeCompareFlags(
            CapacityFinalCalleeCommonTailSizeGateCompareFlags? flags) =>
        flags is CapacityFinalCalleeCommonTailSizeGateCompareFlags value
            ? new CapacityFinalCalleeCommonMaskSelectionGateFlags(
                value.Carry,
                value.Parity,
                value.Auxiliary,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;
}
