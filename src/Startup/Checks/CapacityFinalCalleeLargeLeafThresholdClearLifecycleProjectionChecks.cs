using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeLeafThresholdClearLifecycleProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeLargeLeafThresholdClearPlan below = Project(
            entryEdx: 0,
            freshGlobal: 1,
            entryEbp: uint.MaxValue,
            freshEbpMinusC: 0xABCDEF01,
            freshChildEbpMinus4: null);
        check("Unsigned-below branches to 8E76 without entering the child store",
            below.ParentPlan.BranchTaken == true
            && below.ParentPlan.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && below.Outcome
                == CapacityFinalCalleeLargeLeafThresholdClearOutcome.UnsignedBelowBranchTarget
            && !below.ChildReached
            && below.ChildPlan is null
            && below.Writes is [{ RawFileOffset: 0x012C9211, Value: 0 }]
            && below.Reads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C920B, 0x012C9214 })
            && below.ExactCleanExitAdmitted
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeLeafThresholdClearPlan belowWithUnusedChild =
            Project(
                entryEdx: 0,
                freshGlobal: 1,
                entryEbp: 0x100,
                freshEbpMinusC: 0xABCDEF01,
                freshChildEbpMinus4: 0x1234);
        check("An unused fresh child capture contaminates the bounded 8E76 clean exit",
            belowWithUnusedChild.ParentPlan.ExactCleanAdmission
            && belowWithUnusedChild.UnreachedFreshChildCaptureProvided
            && !belowWithUnusedChild.ChildReached
            && belowWithUnusedChild.ChildPlan is null
            && !belowWithUnusedChild.ExactCleanExitAdmitted
            && belowWithUnusedChild.Reads.Length == 2
            && belowWithUnusedChild.Writes.Length == 1
            && belowWithUnusedChild.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeLeafThresholdClearPlan storedZero = Project(
            entryEdx: 0x80000000,
            freshGlobal: 0x7FFFFFFF,
            entryEbp: 4,
            freshEbpMinusC: 0xDEADBEEF,
            freshChildEbpMinus4: 0);
        check("Unsigned-not-below enters the child, derives its EBP, and clears the independent fresh pointer",
            storedZero.Outcome == CapacityFinalCalleeLargeLeafThresholdClearOutcome.StoredZero
            && storedZero.ParentPlan.BranchTaken == false
            && storedZero.ParentPlan.BranchSelectedDestinationRawFileOffset == 0x012C921D
            && storedZero.ChildReached
            && storedZero.ChildEntryEBP == 4
            && storedZero.ParentCompareStoreAddress == 0
            && storedZero.ParentCompareStoreValue == 0x80000000
            && storedZero.ChildFreshReadAddress == storedZero.ParentCompareStoreAddress
            && storedZero.ChildFreshReadValue == 0
            && storedZero.ChildFreshReadValue != storedZero.ParentCompareStoreValue
            && storedZero.ChildPlan is { StoreAddress: 0, StoreValue: 0 }
            && storedZero.Writes.Select(write => (write.RawFileOffset, write.Address, write.Value))
                .SequenceEqual(new[]
                {
                    (0x012C9211u, (uint?)0, (uint?)0x80000000),
                    (0x012C9220u, (uint?)0, (uint?)0)
                })
            && storedZero.Reads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C920B, 0x012C9214, 0x012C921D })
            && storedZero.Accesses.Select(access => access.RawFileOffset).SequenceEqual(
                new uint[]
                {
                    0x012C920B, 0x012C920B, 0x012C9211, 0x012C9214,
                    0x012C9217, 0x012C921D, 0x012C9220
                })
            && storedZero.ParentCompareFlags is { Carry: false, Overflow: true }
            && storedZero.ChildFreshReadValue == 0
            && storedZero.Capture.ParentThreshold.FreshGlobalValue == 0x7FFFFFFF
            && storedZero.FlagsAtStop == storedZero.ParentCompareFlags
            && storedZero.ChildMovPreservesParentCompareFlags
            && storedZero.ExactCleanExitAdmitted
            && storedZero.StopAtRawFileOffsetExclusive == 0x012C9226);

        CapacityFinalCalleeLargeLeafThresholdClearPlan wrapped = Project(
            entryEdx: 0x80000000,
            freshGlobal: 0x7FFFFFFF,
            entryEbp: 0,
            freshEbpMinusC: 0xCAFEBABE,
            freshChildEbpMinus4: uint.MaxValue);
        check("The maximum fresh pointer preserves EBP wrapping and an aliased independent reload",
            wrapped.ChildEntryEBP == 0
            && wrapped.ParentCompareStoreAddress == 0xFFFFFFFC
            && wrapped.ParentCompareStoreValue == 0x80000000
            && wrapped.ChildFreshReadAddress == 0xFFFFFFFC
            && wrapped.ChildFreshReadValue == uint.MaxValue
            && wrapped.ChildFreshReadValue != wrapped.ParentCompareStoreValue
            && wrapped.ChildPlan is { StoreAddress: uint.MaxValue, StoreValue: 0 }
            && wrapped.ParentCompareFlags is { Carry: false, Overflow: true }
            && wrapped.FlagsAtStop == wrapped.ParentCompareFlags
            && wrapped.ChildMovPreservesParentCompareFlags
            && wrapped.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeLeafThresholdClearPlan missingGlobal =
            CapacityFinalCalleeLargeLeafThresholdClearLifecycleProjection.Project(new(
                new(
                    EntryEDX: 0x20,
                    FreshGlobalValue: null,
                    EntryEBP: 0x100,
                    FreshEbpMinusC: 0xABC),
                FreshChildEbpMinus4: 0x1234));
        check("A missing global read blocks the child while retaining its attempted read",
            missingGlobal.ParentPlan.Outcome
                == CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingFreshGlobalValue
            && missingGlobal.Reads is
                [{ RawFileOffset: 0x012C920B, Address: 0x01EDADD8, ValueCaptured: false }]
            && !missingGlobal.ChildReached
            && missingGlobal.ChildPlan is null
            && missingGlobal.UnreachedFreshChildCaptureProvided
            && !missingGlobal.ExactCleanExitAdmitted
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C920B);

        CapacityFinalCalleeLargeLeafThresholdClearPlan missingEbpMinusC =
            CapacityFinalCalleeLargeLeafThresholdClearLifecycleProjection.Project(new(
                new(
                    EntryEDX: 0x20,
                    FreshGlobalValue: 0x20,
                    EntryEBP: 0x100,
                    FreshEbpMinusC: null),
                FreshChildEbpMinus4: 0x1234));
        check("A missing parent EBP-C read stops at 9214 and never reaches the child",
            missingEbpMinusC.Reads[^1] is
                {
                    RawFileOffset: 0x012C9214,
                    Address: 0xF4,
                    AddressCaptured: true,
                    ValueCaptured: false
                }
            && missingEbpMinusC.Writes is
                [{ RawFileOffset: 0x012C9211, Address: 0xFC, Value: 0x20 }]
            && missingEbpMinusC.ParentPlan.CompareFlags is { Zero: true }
            && !missingEbpMinusC.ChildReached
            && missingEbpMinusC.UnreachedFreshChildCaptureProvided
            && !missingEbpMinusC.ExactCleanExitAdmitted
            && missingEbpMinusC.StopAtRawFileOffsetExclusive == 0x012C9214);

        CapacityFinalCalleeLargeLeafThresholdClearPlan missingChild = Project(
            entryEdx: 0x40,
            freshGlobal: 0x20,
            entryEbp: 0x100,
            freshEbpMinusC: 0xDEADBEEF,
            freshChildEbpMinus4: null);
        check("A missing fresh child pointer retains the attempted 921D read and stops before 9220",
            missingChild.Outcome
                == CapacityFinalCalleeLargeLeafThresholdClearOutcome
                    .MissingChildFreshFrameMinus4
            && missingChild.ChildReached
            && missingChild.ChildEntryEBP == 0x100
            && missingChild.Reads[^1] is
                {
                    RawFileOffset: 0x012C921D,
                    Address: 0xFC,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null
                }
            && missingChild.Writes is
                [{ RawFileOffset: 0x012C9211, Address: 0xFC, Value: 0x40 }]
            && missingChild.Accesses[^2].RawFileOffset == 0x012C9217
            && missingChild.Accesses[^1].RawFileOffset == 0x012C921D
            && missingChild.FlagsAtStop == missingChild.ParentCompareFlags
            && missingChild.ChildMovPreservesParentCompareFlags
            && !missingChild.ExactCleanExitAdmitted
            && missingChild.StopAtRawFileOffsetExclusive == 0x012C921D);

        CapacityFinalCalleeLargeLeafThresholdClearPlan missingParent =
            CapacityFinalCalleeLargeLeafThresholdClearLifecycleProjection.Project(new(
                new(
                    EntryEDX: 0x40,
                    FreshGlobalValue: 0x20,
                    EntryEBP: null,
                    FreshEbpMinusC: 0xDEADBEEF),
                FreshChildEbpMinus4: 0x1234));
        check("A missing parent EBP prevents both parent store and child entry",
            missingParent.ParentPlan.Outcome
                == CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingEntryEBP
            && missingParent.Reads.Length == 1
            && missingParent.Writes.IsEmpty
            && !missingParent.ChildReached
            && missingParent.ChildPlan is null
            && missingParent.UnreachedFreshChildCaptureProvided
            && !missingParent.ExactCleanExitAdmitted
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C9211);
    }

    private static CapacityFinalCalleeLargeLeafThresholdClearPlan Project(
        uint entryEdx,
        uint freshGlobal,
        uint entryEbp,
        uint freshEbpMinusC,
        uint? freshChildEbpMinus4) =>
        CapacityFinalCalleeLargeLeafThresholdClearLifecycleProjection.Project(new(
            new(
                EntryEDX: entryEdx,
                FreshGlobalValue: freshGlobal,
                EntryEBP: entryEbp,
                FreshEbpMinusC: freshEbpMinusC),
            freshChildEbpMinus4));
}
