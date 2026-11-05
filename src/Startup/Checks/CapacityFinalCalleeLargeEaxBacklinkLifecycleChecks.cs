using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeEaxBacklinkLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed gate starts at 927C and admits only the 9288 not-below fallthrough",
            CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.EntryRawFileOffset
                == 0x012C927C
            && CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.ChildEntryRawFileOffset
                == 0x012C9288
            && CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9292
            && CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.ChildZeroDestinationRawFileOffset
                == 0x012C92A4
            && CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection
                .ChildNonzeroDestinationRawFileOffset == 0x012C9292);

        CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan differsFromGlobal =
            CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.Project(new(
                EntryEAX: 0x200,
                FreshGlobalValue: 0x100,
                ChildEntryECX: 0xAABBCCDD,
                ChildEntryEDX: 0x500,
                FreshEDXPlus10: 0));
        check("Child EAX is derived from parent EAX, not the differing global threshold value",
            differsFromGlobal.ParentGate.CompareLeft == 0x200
            && differsFromGlobal.ParentGate.CompareRight == 0x100
            && differsFromGlobal.ParentGate.JBTaken == false
            && differsFromGlobal.ChildReached
            && differsFromGlobal.ChildEAXDerivedFromParentEntry
            && differsFromGlobal.ChildEntryEAX == 0x200
            && differsFromGlobal.ChildGate?.Capture.EntryEAX == 0x200
            && differsFromGlobal.FirstStoreAddress == 0x218
            && differsFromGlobal.FirstStoreValue == 0xAABBCCDD
            && differsFromGlobal.FreshEDXPlus10Value == 0
            && differsFromGlobal.Outcome
                == CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome.ChildZeroTo92A4);

        CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan missingECX =
            CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.Project(new(
                EntryEAX: 0x1000,
                FreshGlobalValue: 0x100,
                ChildEntryECX: null,
                ChildEntryEDX: 0x200,
                FreshEDXPlus10: 0x99));
        check("Missing child ECX stops at 9288 after the EAX-derived store address is attempted",
            missingECX.ChildReached
            && missingECX.ChildEAXDerivedFromParentEntry
            && missingECX.FirstStoreAttempted
            && missingECX.FirstStoreAddressCaptured
            && missingECX.FirstStoreAddress == 0x1018
            && !missingECX.FirstStoreValueCaptured
            && !missingECX.FirstStoreCommitted
            && missingECX.UnreachedChildEDXCaptureProvided
            && missingECX.UnreachedFreshEDXPlus10CaptureProvided
            && !missingECX.FreshReadReached
            && missingECX.FlagsAfterFirstStore == missingECX.ParentCompareFlags
            && missingECX.ParentCompareFlagsRemainUntilTest
            && missingECX.FlagsAtStop == missingECX.ParentCompareFlags
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C9288);

        CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan missingEDX =
            CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.Project(new(
                EntryEAX: 0x1000,
                FreshGlobalValue: 0x100,
                ChildEntryECX: 0x44,
                ChildEntryEDX: null,
                FreshEDXPlus10: 0x77));
        check("Missing child EDX stops at 928B with the known committed store and parent CMP flags",
            missingEDX.FirstStoreCommitted
            && missingEDX.FirstStoreAddress == 0x1018
            && missingEDX.FirstStoreValue == 0x44
            && missingEDX.FreshReadReached
            && !missingEDX.FreshReadAddressCaptured
            && missingEDX.UnusableFreshEDXPlus10CaptureProvided
            && !missingEDX.TestReached
            && missingEDX.ParentCompareFlagsRemainUntilTest
            && missingEDX.FlagsAtStop == missingEDX.ParentCompareFlags
            && missingEDX.Outcome
                == CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome.MissingChildEntryEDX
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C928B);

        CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan missingFreshValue =
            CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.Project(new(
                EntryEAX: 0x1000,
                FreshGlobalValue: 0x100,
                ChildEntryECX: 0x55,
                ChildEntryEDX: 0x200,
                FreshEDXPlus10: null));
        check("A missing fresh value preserves the address, committed write, and parent CMP flags",
            missingFreshValue.FirstStoreCommitted
            && missingFreshValue.FreshReadReached
            && missingFreshValue.FreshReadAddressCaptured
            && missingFreshValue.FreshReadAddress == 0x210
            && !missingFreshValue.TestReached
            && missingFreshValue.FlagsAtStop == missingFreshValue.ParentCompareFlags
            && missingFreshValue.Outcome
                == CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome
                    .MissingChildFreshEDXPlus10
            && missingFreshValue.StopAtRawFileOffsetExclusive == 0x012C928B);

        CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan below =
            CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.Project(new(
                EntryEAX: 0x80,
                FreshGlobalValue: 0x100,
                ChildEntryECX: 0x10,
                ChildEntryEDX: 0x200,
                FreshEDXPlus10: 0));
        check("A below-threshold parent reaches 8E76 without consuming any child captures",
            below.Outcome
                == CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome.ParentUnsignedBelow
            && below.ParentGate.JBTaken == true
            && below.ParentGate.JBSelectedDestinationRawFileOffset == 0x012C8E76
            && below.RetainedParentEAX == 0x80
            && below.NoChildReached
            && below.UnreachedChildLateCapturesProvided
            && below.ChildGate is null
            && below.FlagsAtStop == below.ParentCompareFlags
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan missingGlobal =
            CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.Project(new(
                EntryEAX: 0x1234,
                FreshGlobalValue: null,
                ChildEntryECX: 0x10,
                ChildEntryEDX: 0x20,
                FreshEDXPlus10: 0));
        check("A missing global retains known EAX while leaving child captures unused and flags unknown",
            missingGlobal.Outcome
                == CapacityFinalCalleeLargeEaxBacklinkLifecycleOutcome
                    .MissingParentFreshGlobalValue
            && missingGlobal.ParentEAXCaptured
            && missingGlobal.RetainedParentEAX == 0x1234
            && missingGlobal.ParentGate.CompareLeft == 0x1234
            && !missingGlobal.ParentGate.CompareReached
            && missingGlobal.ParentGate.CompareFlags is null
            && missingGlobal.NoChildReached
            && missingGlobal.UnreachedChildLateCapturesProvided
            && missingGlobal.FlagsAtStop is null
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C927C);

        CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan zero =
            CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.Project(new(
                EntryEAX: 0x100,
                FreshGlobalValue: 0x80,
                ChildEntryECX: 0xDEADBEEF,
                ChildEntryEDX: 0x200,
                FreshEDXPlus10: 0));
        check("The fresh TEST overrides parent CMP flags and the zero JZ stops at 92A4",
            zero.FirstStoreCommitted
            && zero.FreshReadAddress == 0x210
            && zero.FreshEDXPlus10Value == 0
            && zero.TestReached
            && zero.TestFlags is
                {
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: null,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                }
            && zero.FlagsAtStop?.Zero == true
            && zero.FlagsAtStop != zero.ParentCompareFlags
            && zero.ChildGate?.JzTaken == true
            && zero.ChildGate.JzSelectedDestinationRawFileOffset == 0x012C92A4
            && zero.ExactBoundedCompletion
            && !zero.ChildDestinationEntered
            && zero.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan nonzero =
            CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.Project(new(
                EntryEAX: 0x100,
                FreshGlobalValue: 0x80,
                ChildEntryECX: 0x80,
                ChildEntryEDX: 0x200,
                FreshEDXPlus10: 0x80000001));
        check("The nonzero fresh TEST uses logical flags and stops at 9292 before the next gate",
            nonzero.TestFlags is
                {
                    Carry: false,
                    Parity: false,
                    AuxiliaryCarry: null,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                }
            && nonzero.FlagsAtStop?.Sign == true
            && nonzero.ChildGate?.JzTaken == false
            && nonzero.ChildGate.JzSelectedDestinationRawFileOffset == 0x012C9292
            && nonzero.ExactBoundedCompletion
            && !nonzero.ChildGate!.FollowingCMPReached
            && !nonzero.ChildGate.TargetReadReached
            && nonzero.StopAtRawFileOffsetExclusive == 0x012C9292);

        CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan wrapped =
            CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.Project(new(
                EntryEAX: 0xFFFFFFF0,
                FreshGlobalValue: 1,
                ChildEntryECX: 0xFFFFFFFF,
                ChildEntryEDX: 0xFFFFFFF8,
                FreshEDXPlus10: 1));
        check("Both child addresses independently use unsigned 32-bit wraparound",
            wrapped.ChildGate?.FirstStoreAddress == 8
            && wrapped.FreshReadAddress == 8
            && wrapped.ChildGate?.MemoryOperations[0].Address == 8
            && wrapped.ChildGate?.MemoryOperations[1].Address == 8
            && wrapped.ChildGate?.JzTaken == false
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C9292);

        CapacityFinalCalleeLargeEaxBacklinkLifecyclePlan alias =
            CapacityFinalCalleeLargeEaxBacklinkLifecycleProjection.Project(new(
                EntryEAX: 0x100,
                FreshGlobalValue: 0x80,
                ChildEntryECX: 0x40,
                ChildEntryEDX: 0x108,
                FreshEDXPlus10: 0x80));
        check("An aliased fresh read is independent of the preceding store's value",
            alias.ChildGate?.MemoryOperations.Select(operation => operation.Address)
                .SequenceEqual(new uint?[] { 0x118, 0x118 }) == true
            && alias.ChildGate?.MemoryOperations[0].Value == 0x40
            && alias.ChildGate.MemoryOperations[0].Committed
            && alias.ChildGate.MemoryOperations[1].Value == 0x80
            && alias.ChildGate.MemoryOperations[1].ValueCaptured
            && alias.FreshEDXPlus10Value == 0x80
            && alias.TestReached
            && alias.TestFlags?.Zero == false
            && alias.ChildGate.JzTaken == false);
    }
}
