using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup.CapacityFinalCalleeLargeNextThresholdStoresLifecycleProjection;

internal static class CapacityFinalCalleeLargeNextThresholdStoresLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan missingBoth =
            Projection.Project(new(
                EntryECX: null,
                FreshGlobalValue: null,
                ChildEntryEAX: null));
        check("Missing parent CMP operands retain the read attempt and create no child",
            missingBoth.Outcome
                == CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                    .MissingBothParentCompareOperands
            && missingBoth.ParentGate.Reads.Length == 1
            && missingBoth.ParentGate.Reads[0].Address == 0x01EDADD8
            && missingBoth.ParentCompareLeft is null
            && missingBoth.ParentCompareRight is null
            && missingBoth.FlagsAtStop is null
            && missingBoth.NoChildReached
            && missingBoth.StopAtRawFileOffsetExclusive == 0x012C9292);

        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan missingECX =
            Projection.Project(new(
                EntryECX: null,
                FreshGlobalValue: 0x12345678,
                ChildEntryEAX: null));
        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan missingGlobal =
            Projection.Project(new(
                EntryECX: 0x12345678,
                FreshGlobalValue: null,
                ChildEntryEAX: null));
        check("Missing parent ECX or global retains the known same-CMP operand without flags",
            missingECX.Outcome
                == CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                    .MissingParentEntryECX
            && missingECX.ParentCompareLeft is null
            && missingECX.ParentCompareRight == 0x12345678
            && missingECX.FlagsAtStop is null
            && missingECX.NoChildReached
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C9292
            && missingGlobal.Outcome
                == CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                    .MissingParentFreshGlobalValue
            && missingGlobal.ParentCompareLeft == 0x12345678
            && missingGlobal.ParentCompareRight is null
            && missingGlobal.ParentGate.Reads[0].Address == 0x01EDADD8
            && !missingGlobal.ParentGate.Reads[0].ValueCaptured
            && missingGlobal.FlagsAtStop is null
            && missingGlobal.NoChildReached
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9292);

        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan below =
            Projection.Project(new(
                EntryECX: 0,
                FreshGlobalValue: 1,
                ChildEntryEAX: null));
        check("Parent unsigned-below branches to 8E76 without an EAX capture or child",
            below.Outcome
                == CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                    .ParentUnsignedBelowTo8E76
            && below.ParentBranchTaken == true
            && below.ParentBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && below.NoChildReached
            && !below.UnreachedChildEntryEAXCaptureProvided
            && below.FlagsAtStop is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Overflow: false
            }
            && below.ControlFlowBoundedCompletion
            && below.ExactCleanCompletion
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan belowWithLateEAX =
            Projection.Project(new(
                EntryECX: 0,
                FreshGlobalValue: 1,
                ChildEntryEAX: 0xCAFEBABE));
        check("A supplied late EAX remains explicitly unused on the parent below branch",
            belowWithLateEAX.NoChildReached
            && belowWithLateEAX.UnreachedChildEntryEAXCaptureProvided
            && !belowWithLateEAX.ExactCleanCompletion
            && belowWithLateEAX.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan missingChildEAX =
            Projection.Project(new(
                EntryECX: 0x200,
                FreshGlobalValue: 0x100,
                ChildEntryEAX: null));
        check("Missing child EAX attempts the first store with known ECX value and unknown address",
            missingChildEAX.Outcome
                == CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome
                    .ChildMissingFirstStoreAddress
            && missingChildEAX.ChildReached
            && missingChildEAX.ChildEntryECXDerivedFromParentEntryECX
            && missingChildEAX.ChildEntryECX == 0x200
            && !missingChildEAX.ChildEntryEAXCaptured
            && missingChildEAX.ChildStores?.FirstStoreReached == true
            && missingChildEAX.ChildStores.FirstStoreAddressAttempted
            && missingChildEAX.ChildStores.FirstStoreAddress is null
            && missingChildEAX.ChildStores.FirstStoreValue == 0x200
            && !missingChildEAX.ChildStores.FirstStoreCommitted
            && missingChildEAX.ChildStores.Writes.IsEmpty
            && missingChildEAX.FlagsAtStop == missingChildEAX.ParentCompareFlags
            && missingChildEAX.ChildStores.FlagsUnaffected
            && missingChildEAX.StopAtRawFileOffsetExclusive == 0x012C929E);

        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan zero =
            Projection.Project(new(
                EntryECX: 0,
                FreshGlobalValue: 0,
                ChildEntryEAX: 0));
        check("Zero values take the exact not-below handoff and commit both ordered stores",
            zero.Outcome
                == CapacityFinalCalleeLargeNextThresholdStoresLifecycleOutcome.ChildCompleted
            && zero.ParentCompareLeft == 0
            && zero.ParentCompareRight == 0
            && zero.ParentBranchTaken == false
            && zero.ParentBranchSelectedDestinationRawFileOffset == 0x012C929E
            && zero.ChildEntryECXDerivedFromParentEntryECX
            && zero.ChildStores?.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeLargeNextPairedStoresWrite(0x012C929E, 0x10, 0),
                new CapacityFinalCalleeLargeNextPairedStoresWrite(0x012C92A1, 0x18, 0)
            }) == true
            && zero.ChildStores.FirstStoreCommitted
            && zero.ChildStores.SecondStoreCommitted
            && zero.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Overflow: false
            }
            && zero.FlagsAtStop == zero.ParentCompareFlags
            && zero.FlagsUnaffectedByChildStores
            && zero.ExactCleanCompletion
            && zero.StopAtRawFileOffsetExclusive == 0x012C92A4
            && !zero.BoundaryInstructionExecuted
            && !zero.FreshReadReachedAfterStores);

        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan maximum =
            Projection.Project(new(
                EntryECX: uint.MaxValue,
                FreshGlobalValue: uint.MaxValue,
                ChildEntryEAX: uint.MaxValue));
        check("Maximum DWORD values preserve CMP flags and wrapped store addresses",
            maximum.ChildStores?.FirstStoreAddress == 0x0F
            && maximum.ChildStores.FirstStoreValue == uint.MaxValue
            && maximum.ChildStores.SecondStoreAddress == 0x17
            && maximum.ChildStores.SecondStoreValue == uint.MaxValue
            && maximum.ChildStores.Writes.Length == 2
            && maximum.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Overflow: false
            }
            && maximum.FlagsAtStop == maximum.ParentCompareFlags
            && maximum.ExactCleanCompletion
            && maximum.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan overflowFlags =
            Projection.Project(new(
                EntryECX: 0x80000000,
                FreshGlobalValue: 1,
                ChildEntryEAX: 0x100));
        check("Stores retain parent CMP CF, OF, AF, and PF when EAX differs from ECX",
            overflowFlags.ParentCompareLeft == 0x80000000
            && overflowFlags.ParentCompareRight == 1
            && overflowFlags.ChildStores?.FirstStoreAddress == 0x110
            && overflowFlags.ChildStores.FirstStoreValue == 0x80000000
            && overflowFlags.ChildStores.SecondStoreAddress == 0x80000018
            && overflowFlags.ChildStores.SecondStoreValue == 0x100
            && overflowFlags.ChildStores.Writes.Length == 2
            && overflowFlags.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Overflow: true
            }
            && overflowFlags.FlagsAtStop == overflowFlags.ParentCompareFlags
            && overflowFlags.ExactCleanCompletion
            && overflowFlags.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan parityFlags =
            Projection.Project(new(
                EntryECX: 1,
                FreshGlobalValue: 0,
                ChildEntryEAX: 0xFFFFFFF8));
        check("Unsigned non-below uses ECX versus global and preserves odd-parity CMP flags",
            parityFlags.ParentCompareLeft == 1
            && parityFlags.ParentCompareRight == 0
            && parityFlags.ChildStores?.FirstStoreAddress == 0x8
            && parityFlags.ChildStores.SecondStoreAddress == 0x19
            && parityFlags.ChildStores.Writes.Length == 2
            && parityFlags.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Overflow: false
            }
            && parityFlags.FlagsAtStop == parityFlags.ParentCompareFlags);

        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan wrapped =
            Projection.Project(new(
                EntryECX: 0xFFFFFFF0,
                FreshGlobalValue: 0,
                ChildEntryEAX: 0xFFFFFFF8));
        check("Both effective store addresses wrap at DWORD width without coalescing writes",
            wrapped.ChildStores?.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeLargeNextPairedStoresWrite(
                    0x012C929E,
                    0x08,
                    0xFFFFFFF0),
                new CapacityFinalCalleeLargeNextPairedStoresWrite(
                    0x012C92A1,
                    0x08,
                    0xFFFFFFF8)
            }) == true
            && wrapped.ChildStores.Writes.Length == 2
            && wrapped.ChildStores.Writes[0].Address == wrapped.ChildStores.Writes[1].Address
            && wrapped.ChildStores.Writes[0].Value != wrapped.ChildStores.Writes[1].Value
            && wrapped.FlagsAtStop == wrapped.ParentCompareFlags
            && wrapped.ExactCleanCompletion);

        CapacityFinalCalleeLargeNextThresholdStoresLifecyclePlan alias =
            Projection.Project(new(
                EntryECX: 0xF8,
                FreshGlobalValue: 0,
                ChildEntryEAX: 0x100));
        check("Aliased store addresses retain both instruction-ordered distinct values",
            alias.ChildStores?.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeLargeNextPairedStoresWrite(0x012C929E, 0x110, 0xF8),
                new CapacityFinalCalleeLargeNextPairedStoresWrite(0x012C92A1, 0x110, 0x100)
            }) == true
            && alias.ChildStores.Writes.Length == 2
            && alias.ChildStores.Writes[0].Address == alias.ChildStores.Writes[1].Address
            && alias.ChildStores.Writes[0].Value != alias.ChildStores.Writes[1].Value
            && alias.FlagsAtStop == alias.ParentCompareFlags
            && alias.ExactCleanCompletion
            && alias.StopAtRawFileOffsetExclusive == 0x012C92A4);
        check("The lifecycle orders the independent threshold read before both stores",
            alias.MemoryOperations.Select(item => (item.RawFileOffset, item.Committed))
                .SequenceEqual(new[]
                {
                    (0x012C9292u, false),
                    (0x012C929Eu, true),
                    (0x012C92A1u, true)
                })
            && alias.ReachedInstructions.SequenceEqual(new uint[]
                { 0x012C9292, 0x012C9298, 0x012C929E, 0x012C92A1 })
            && missingChildEAX.MemoryOperations.Length == 2
            && missingChildEAX.MemoryOperations[1].Value == 0x200
            && missingChildEAX.MemoryOperations[1].Address is null
            && !missingChildEAX.MemoryOperations[1].Committed);
    }
}
