using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup.CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleProjection;

internal static class CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition admits only the exact 9292 handoff and bounded destinations",
            Projection.EntryRawFileOffset == 0x012C9288
            && Projection.ChildEntryRawFileOffset == 0x012C9292
            && Projection.ParentZeroDestinationRawFileOffset == 0x012C92A4
            && Projection.ChildBelowDestinationRawFileOffset == 0x012C8E76
            && Projection.ChildNotBelowDestinationRawFileOffset == 0x012C929E
            && Projection.BoundaryRawFileOffsetExclusive == 0x012C929E);

        CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan discriminator =
            Projection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0x11,
                EntryEDX: 0x200,
                FreshEDXPlus10: 0x40,
                ChildFreshGlobalValue: 0x80));
        check("The 9292 child compares parent fresh ECX, not the differing stored ECX",
            discriminator.ChildReached
            && discriminator.ParentNonzeroExactHandoff
            && discriminator.ParentStoredEntryECX == 0x11
            && discriminator.ParentFreshECX == 0x40
            && discriminator.ChildECXDerivedFromParentFreshECX
            && discriminator.ChildEntryECX == 0x40
            && discriminator.ChildEntryECXDiffersFromParentStoredEntryECX
            && discriminator.ChildCompareLeft == 0x40
            && discriminator.ChildCompareRight == 0x80
            && discriminator.Outcome
                == CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .ChildUnsignedBelowTo8E76
            && discriminator.ChildBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && discriminator.StopAtRawFileOffsetExclusive == 0x012C8E76
            && discriminator.ControlFlowBoundedCompletion
            && discriminator.ExactCleanCompletion);

        CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan parentZero =
            Projection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0xDEADBEEF,
                EntryEDX: 0x200,
                FreshEDXPlus10: 0,
                ChildFreshGlobalValue: 0xCAFEBABE));
        check("Parent zero stops at 92A4 and leaves the late child global capture unused",
            parentZero.Outcome
                == CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .ParentZeroTo92A4
            && parentZero.NoChildReached
            && parentZero.ChildGate is null
            && parentZero.UnreachedChildFreshGlobalValueCaptureProvided
            && !parentZero.ChildFreshGlobalReadReached
            && parentZero.MemoryOperations.Select(operation => operation.Kind).SequenceEqual(
                new[]
                {
                    CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperationKind
                        .Write,
                    CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperationKind
                        .Read
                })
            && parentZero.FlagsAtStopSource
                == CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleFlagsSource.ParentTest
            && parentZero.FlagsAtStop is
                {
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: null,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                }
            && parentZero.ControlFlowBoundedCompletion
            && !parentZero.ExactCleanCompletion
            && parentZero.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan missingParentEAX =
            Projection.Project(new(
                EntryEAX: null,
                EntryECX: 0x11,
                EntryEDX: 0x200,
                FreshEDXPlus10: 0x40,
                ChildFreshGlobalValue: 0x80));
        CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan missingParentECX =
            Projection.Project(new(
                EntryEAX: 0x100,
                EntryECX: null,
                EntryEDX: 0x200,
                FreshEDXPlus10: 0x40,
                ChildFreshGlobalValue: 0x80));
        check("Each missing parent store operand stops at 9288 without entering the child",
            missingParentEAX.Outcome
                == CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .MissingParentEntryEAX
            && missingParentEAX.FirstStoreAttempted
            && !missingParentEAX.FirstStoreCommitted
            && missingParentEAX.UnreachedParentEntryEDXCaptureProvided
            && missingParentEAX.UnreachedParentFreshEDXPlus10CaptureProvided
            && missingParentEAX.UnreachedChildFreshGlobalValueCaptureProvided
            && missingParentEAX.NoChildReached
            && missingParentEAX.FlagsAtStop is null
            && missingParentEAX.StopAtRawFileOffsetExclusive == 0x012C9288
            && missingParentECX.Outcome
                == CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .MissingParentEntryECX
            && missingParentECX.FirstStoreAddress == 0x118
            && !missingParentECX.FirstStoreCommitted
            && missingParentECX.NoChildReached
            && missingParentECX.FlagsAtStop is null
            && missingParentECX.StopAtRawFileOffsetExclusive == 0x012C9288);

        CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan missingParentEDX =
            Projection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0x11,
                EntryEDX: null,
                FreshEDXPlus10: 0x40,
                ChildFreshGlobalValue: 0x80));
        CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan missingParentFresh =
            Projection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0x11,
                EntryEDX: 0x200,
                FreshEDXPlus10: null,
                ChildFreshGlobalValue: 0x80));
        check("Each missing parent read input stops at 928B without entering the child or inventing flags",
            missingParentEDX.Outcome
                == CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .MissingParentEntryEDX
            && missingParentEDX.FirstStoreCommitted
            && missingParentEDX.FreshReadReached
            && missingParentEDX.UnusableParentFreshEDXPlus10CaptureProvided
            && missingParentEDX.UnreachedChildFreshGlobalValueCaptureProvided
            && missingParentEDX.NoChildReached
            && missingParentEDX.FlagsAtStop is null
            && missingParentEDX.StopAtRawFileOffsetExclusive == 0x012C928B
            && missingParentFresh.Outcome
                == CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .MissingParentFreshEDXPlus10
            && missingParentFresh.FirstStoreCommitted
            && missingParentFresh.FreshReadAddress == 0x210
            && missingParentFresh.NoChildReached
            && missingParentFresh.FlagsAtStop is null
            && missingParentFresh.StopAtRawFileOffsetExclusive == 0x012C928B);

        CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan missingChildGlobal =
            Projection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0x11,
                EntryEDX: 0x200,
                FreshEDXPlus10: 0x40,
                ChildFreshGlobalValue: null));
        check("A missing 01EDADD8 reload at 9292 retains derived ECX and parent TEST flags",
            missingChildGlobal.Outcome
                == CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .MissingChildFreshGlobalValue
            && missingChildGlobal.ChildReached
            && missingChildGlobal.ChildECXDerivedFromParentFreshECX
            && missingChildGlobal.ChildEntryECX == 0x40
            && missingChildGlobal.ChildCompareLeft == 0x40
            && missingChildGlobal.ChildFreshGlobalReadReached
            && missingChildGlobal.ChildFreshGlobalAddress == 0x01EDADD8
            && !missingChildGlobal.ChildFreshGlobalValueCaptured
            && missingChildGlobal.MemoryOperations.Length == 3
            && missingChildGlobal.MemoryOperations[2] is
                {
                    RawFileOffset: 0x012C9292,
                    Kind: CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperationKind
                        .Read,
                    Address: 0x01EDADD8,
                    AddressCaptured: true,
                    Value: null,
                    ValueCaptured: false
                }
            && !missingChildGlobal.ChildCompareReached
            && !missingChildGlobal.ChildBranchReached
            && missingChildGlobal.ParentTestFlagsRemainUntilChildCompare
            && missingChildGlobal.FlagsAtStopSource
                == CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleFlagsSource.ParentTest
            && missingChildGlobal.FlagsAtStop is
                {
                    Carry: false,
                    Parity: false,
                    AuxiliaryCarry: null,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            && missingChildGlobal.StopAtRawFileOffsetExclusive == 0x012C9292);

        CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan childBelow =
            Projection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0x11,
                EntryEDX: 0x200,
                FreshEDXPlus10: 1,
                ChildFreshGlobalValue: 2));
        check("A completed child CMP replaces TEST flags and branches below to 8E76",
            childBelow.ChildCompareReached
            && childBelow.ChildCompareResult == 0xFFFFFFFF
            && childBelow.ChildCompareFlags is
                {
                    Carry: true,
                    Parity: true,
                    AuxiliaryCarry: true,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                }
            && childBelow.FlagsAtStopSource
                == CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleFlagsSource.ChildCompare
            && childBelow.FlagsAtStop is
                {
                    Carry: true,
                    Parity: true,
                    AuxiliaryCarry: true,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                }
            && childBelow.FlagsAtStop?.Carry != childBelow.ParentTestFlags?.Carry
            && childBelow.ChildBranchTaken == true
            && childBelow.ChildBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && !childBelow.ChildPairedStoreReached
            && childBelow.ControlFlowBoundedCompletion
            && childBelow.ExactCleanCompletion
            && childBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan childNotBelow =
            Projection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0x11,
                EntryEDX: 0x200,
                FreshEDXPlus10: 0x80,
                ChildFreshGlobalValue: 0x40));
        check("A completed not-below JB stops at 929E before its paired store",
            childNotBelow.Outcome
                == CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleOutcome
                    .ChildUnsignedNotBelowTo929E
            && childNotBelow.ChildBranchTaken == false
            && childNotBelow.ChildBranchSelectedDestinationRawFileOffset == 0x012C929E
            && childNotBelow.ChildCompareFlags is
                {
                    Carry: false,
                    Parity: false,
                    AuxiliaryCarry: false,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            && childNotBelow.FlagsAtStop is
                {
                    Carry: false,
                    Parity: false,
                    AuxiliaryCarry: false,
                    Zero: false,
                    Sign: false,
                    Overflow: false
                }
            && !childNotBelow.ChildPairedStoreReached
            && childNotBelow.ControlFlowBoundedCompletion
            && childNotBelow.ExactCleanCompletion
            && childNotBelow.StopAtRawFileOffsetExclusive == 0x012C929E);

        CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan aliasedFreshValueDiffers =
            Projection.Project(new(
                EntryEAX: 0x100,
                EntryECX: 0x40,
                EntryEDX: 0x108,
                FreshEDXPlus10: 0x80,
                ChildFreshGlobalValue: 0x90));
        check("Aliased store and fresh read remain ordered and the later global reload stays independent",
            aliasedFreshValueDiffers.MemoryOperations.Length == 3
            && aliasedFreshValueDiffers.MemoryOperations[0] is
                {
                    RawFileOffset: 0x012C9288,
                    Kind: CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperationKind
                        .Write,
                    Address: 0x118,
                    Value: 0x40,
                    Committed: true
                }
            && aliasedFreshValueDiffers.MemoryOperations[1] is
                {
                    RawFileOffset: 0x012C928B,
                    Kind: CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperationKind
                        .Read,
                    Address: 0x118,
                    Value: 0x80,
                    ValueCaptured: true
                }
            && aliasedFreshValueDiffers.MemoryOperations[2] is
                {
                    RawFileOffset: 0x012C9292,
                    Kind: CapacityFinalCalleeLargeBacklinkNextThresholdLifecycleMemoryOperationKind
                        .Read,
                    Address: 0x01EDADD8,
                    Value: 0x90,
                    ValueCaptured: true
                }
            && aliasedFreshValueDiffers.MemoryOperations[0].RawFileOffset
                < aliasedFreshValueDiffers.MemoryOperations[1].RawFileOffset
            && aliasedFreshValueDiffers.MemoryOperations[1].RawFileOffset
                < aliasedFreshValueDiffers.MemoryOperations[2].RawFileOffset
            && aliasedFreshValueDiffers.ParentStoredEntryECX == 0x40
            && aliasedFreshValueDiffers.ChildEntryECX == 0x80
            && aliasedFreshValueDiffers.ChildCompareRight == 0x90
            && aliasedFreshValueDiffers.ChildBranchTaken == true
            && aliasedFreshValueDiffers.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeBacklinkNextThresholdLifecyclePlan wrappedMaximum =
            Projection.Project(new(
                EntryEAX: uint.MaxValue - 0xF,
                EntryECX: uint.MaxValue,
                EntryEDX: uint.MaxValue - 0xF,
                FreshEDXPlus10: uint.MaxValue,
                ChildFreshGlobalValue: 1));
        check("Wrapped addresses, maximum fresh DWORD, and child CMP flags use unsigned 32-bit semantics",
            wrappedMaximum.FirstStoreAddress == 8
            && wrappedMaximum.FreshReadAddress == 0
            && wrappedMaximum.MemoryOperations[0].Address == 8
            && wrappedMaximum.MemoryOperations[1].Address == 0
            && wrappedMaximum.MemoryOperations[2].Address == 0x01EDADD8
            && wrappedMaximum.ParentFreshECX == uint.MaxValue
            && wrappedMaximum.ChildCompareResult == 0xFFFFFFFE
            && wrappedMaximum.ChildCompareFlags is
                {
                    Carry: false,
                    Parity: false,
                    AuxiliaryCarry: false,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                }
            && wrappedMaximum.ChildBranchTaken == false
            && wrappedMaximum.ChildBranchSelectedDestinationRawFileOffset == 0x012C929E
            && wrappedMaximum.StopAtRawFileOffsetExclusive == 0x012C929E);
    }
}
