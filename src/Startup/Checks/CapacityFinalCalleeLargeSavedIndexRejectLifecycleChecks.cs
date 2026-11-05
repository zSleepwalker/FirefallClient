using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSavedIndexRejectLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition uses the exact 9243-to-9263 edges and bounded child exits",
            CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection
                .ParentEqualDestinationRawFileOffset == 0x012C9243
            && CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection
                .RejectGateEntryRawFileOffset == 0x012C9263
            && CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection
                .ChildBelowDestinationRawFileOffset == 0x012C8E76
            && CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection
                .ChildNotBelowDestinationRawFileOffset == 0x012C926B
            && CapacityFinalCalleeLargeSavedIndexGateProjection.JnzDestinationRawFileOffset
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection
                    .RejectGateEntryRawFileOffset
            && CapacityFinalCalleeLargeIndexedRejectGateProjection.EntryRawFileOffset
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection
                    .RejectGateEntryRawFileOffset
            && CapacityFinalCalleeLargeSavedIndexGateProjection
                .BoundaryRawFileOffsetExclusive
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection
                    .ParentEqualDestinationRawFileOffset
            && CapacityFinalCalleeLargeIndexedRejectGateProjection
                .BoundaryRawFileOffsetExclusive
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection
                    .ChildNotBelowDestinationRawFileOffset);

        CapacityFinalCalleeLargeSavedIndexRejectLifecyclePlan derivedReload =
            CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection.Project(new(
                SavedIndexGateCapture: new(
                    EntryEDX: 0x2001,
                    FreshEDXPlus1C: 0x5000,
                    FreshIndexedDword: 0x2000,
                    FreshGlobalValue: 0x4000),
                ChildEntryECX: 0x3000));
        check("The child uses independent ECX and EBX from the parent's fresh global reload",
            derivedReload.Outcome
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
                    .ChildUnsignedBelowToTarget
            && derivedReload.SavedIndexGate.CompareRight == 0x2000
            && derivedReload.SavedIndexGate.EBXAfterFreshEDXPlus1C == 0x5000
            && derivedReload.SavedIndexGate.EBXAfterFreshGlobalReload == 0x4000
            && derivedReload.ChildEntryECX == 0x3000
            && derivedReload.ChildEntryECX
                != derivedReload.SavedIndexGate.Capture.EntryEDX
            && derivedReload.ChildEntryECXSource
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleChildECXSource
                    .ExplicitIndependentCapture
            && derivedReload.ChildEntryEBX == 0x4000
            && derivedReload.ChildEntryEBX
                != derivedReload.SavedIndexGate.Capture.FreshIndexedDword
            && derivedReload.ChildEntryEBX
                != derivedReload.SavedIndexGate.Capture.FreshEDXPlus1C
            && derivedReload.ChildEntryEBXSource
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleChildEBXSource
                    .ParentEBXAfterFreshGlobalReload
            && derivedReload.IndexedRejectGate?.CompareLeft == 0x3000
            && derivedReload.IndexedRejectGate?.CompareRight == 0x4000
            && derivedReload.IndexedRejectGate?.CompareResult == 0xFFFFF000
            && derivedReload.IndexedRejectGate?.CompareFlags?.Carry == true
            && derivedReload.IndexedRejectGate?.BranchTaken == true
            && derivedReload.IndexedRejectGate?.BranchSelectedDestinationRawFileOffset
                == 0x012C8E76
            && derivedReload.StopAtRawFileOffsetExclusive == 0x012C8E76
            && derivedReload.ExactCleanHandoffToRejectGate
            && derivedReload.ExactCleanExitAdmitted
            && derivedReload.ReachedInstructions.SequenceEqual(new uint[]
            {
                0x012C9231,
                0x012C9234,
                0x012C923B,
                0x012C9241,
                0x012C9263,
                0x012C9265
            })
            && derivedReload.OrderedReads.Select(read => read.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C9231, 0x012C9234, 0x012C923B })
            && derivedReload.OrderedReads.Select(read => read.Value)
                .SequenceEqual(new uint?[] { 0x5000, 0x2000, 0x4000 })
            && derivedReload.ChildEvents.IsEmpty
                == false
            && !derivedReload.ChildMemoryReadsReached
            && !derivedReload.MemoryWritesReached);

        CapacityFinalCalleeLargeSavedIndexRejectLifecyclePlan missingChildECX =
            CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection.Project(new(
                SavedIndexGateCapture: new(
                    EntryEDX: 0x2001,
                    FreshEDXPlus1C: 0x5000,
                    FreshIndexedDword: 0x2000,
                    FreshGlobalValue: 0x4000),
                ChildEntryECX: null));
        check("Missing child ECX keeps derived EBX and the parent's latest CMP flags",
            missingChildECX.Outcome
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
                    .MissingChildEntryECX
            && missingChildECX.RejectGateReached
            && missingChildECX.IndexedRejectGate?.EntryEBXCaptured == true
            && missingChildECX.IndexedRejectGate?.CompareRight == 0x4000
            && missingChildECX.IndexedRejectGate?.EntryECXCaptured == false
            && missingChildECX.IndexedRejectGate?.CompareReached == false
            && missingChildECX.ChildEntryEBX == 0x4000
            && missingChildECX.ChildEntryECX is null
            && missingChildECX.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingChildECX.FlagsAtStop
                == ConvertParentFlags(missingChildECX.SavedIndexGate.CompareFlags)
            && missingChildECX.ReachedInstructions.SequenceEqual(new uint[]
            {
                0x012C9231,
                0x012C9234,
                0x012C923B,
                0x012C9241
            })
            && !missingChildECX.ExactCleanExitAdmitted
            && missingChildECX.StopAtRawFileOffsetExclusive == 0x012C9263
            && !missingChildECX.ChildMemoryReadsReached
            && !missingChildECX.MemoryWritesReached);

        CapacityFinalCalleeLargeSavedIndexRejectLifecyclePlan equalParent =
            CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection.Project(new(
                SavedIndexGateCapture: new(
                    EntryEDX: 0x44,
                    FreshEDXPlus1C: 0x99,
                    FreshIndexedDword: 0x44,
                    FreshGlobalValue: 0xAB),
                ChildEntryECX: null));
        CapacityFinalCalleeLargeSavedIndexRejectLifecyclePlan equalWithUnusedECX =
            CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection.Project(new(
                SavedIndexGateCapture: new(
                    EntryEDX: 0x44,
                    FreshEDXPlus1C: 0x99,
                    FreshIndexedDword: 0x44,
                    FreshGlobalValue: 0xAB),
                ChildEntryECX: 0));
        check("Parent equality stops at 9243 and an unused child ECX contaminates clean admission",
            equalParent.Outcome
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome.ParentEqualTo9243
            && equalParent.SavedIndexGate.JnzTaken == false
            && !equalParent.RejectGateReached
            && equalParent.ReachedInstructions.SequenceEqual(new uint[]
            {
                0x012C9231,
                0x012C9234,
                0x012C923B,
                0x012C9241
            })
            && !equalParent.UnreachedChildEntryECXCaptureProvided
            && equalParent.ExactCleanExitAdmitted
            && equalParent.StopAtRawFileOffsetExclusive == 0x012C9243
            && equalWithUnusedECX.Outcome
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
                    .ParentEqualTo9243WithUnusedChildECX
            && equalWithUnusedECX.UnreachedChildEntryECXCaptureProvided
            && !equalWithUnusedECX.RejectGateReached
            && !equalWithUnusedECX.ExactCleanExitAdmitted
            && equalWithUnusedECX.StopAtRawFileOffsetExclusive == 0x012C9243);

        CapacityFinalCalleeLargeSavedIndexRejectLifecyclePlan missingLateGlobal =
            CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection.Project(new(
                SavedIndexGateCapture: new(
                    EntryEDX: 0x2001,
                    FreshEDXPlus1C: 0x5000,
                    FreshIndexedDword: 0x2000,
                    FreshGlobalValue: null),
                ChildEntryECX: 0x3000));
        check("A missing late parent global prevents the child and retains parent CMP flags",
            missingLateGlobal.Outcome
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
                    .ParentGateIncomplete
            && missingLateGlobal.SavedIndexGate.CompareReached
            && missingLateGlobal.SavedIndexGate.EBXAfterFreshEDXPlus1C == 0x5000
            && missingLateGlobal.SavedIndexGate.CompareFlags?.Carry == false
            && missingLateGlobal.SavedIndexGate.FreshGlobalReadReached
            && !missingLateGlobal.SavedIndexGate.FreshGlobalValueCaptured
            && missingLateGlobal.SavedIndexGate.StopAtRawFileOffsetExclusive
                == 0x012C923B
            && !missingLateGlobal.RejectGateReached
            && missingLateGlobal.ChildEntryEBX is null
            && missingLateGlobal.ChildEntryEBXSource
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleChildEBXSource.None
            && missingLateGlobal.UnreachedChildEntryECXCaptureProvided
            && missingLateGlobal.FlagsAtStop
                == ConvertParentFlags(missingLateGlobal.SavedIndexGate.CompareFlags)
            && !missingLateGlobal.ExactCleanExitAdmitted
            && missingLateGlobal.OrderedReads.Length == 3
            && missingLateGlobal.StopAtRawFileOffsetExclusive == 0x012C923B);

        CapacityFinalCalleeLargeSavedIndexRejectLifecyclePlan zeroChild =
            CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection.Project(new(
                SavedIndexGateCapture: new(
                    EntryEDX: 0,
                    FreshEDXPlus1C: 0,
                    FreshIndexedDword: 1,
                    FreshGlobalValue: 0),
                ChildEntryECX: 0));
        check("Captured child zeros compare exactly and take the bounded 926B fallthrough",
            zeroChild.Outcome
                == CapacityFinalCalleeLargeSavedIndexRejectLifecycleOutcome
                    .ChildUnsignedNotBelowToFallthrough
            && zeroChild.IndexedRejectGate?.CompareResult == 0
            && zeroChild.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroChild.IndexedRejectGate?.BranchTaken == false
            && zeroChild.IndexedRejectGate?.BranchSelectedDestinationRawFileOffset
                == 0x012C926B
            && zeroChild.StopAtRawFileOffsetExclusive == 0x012C926B
            && zeroChild.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeSavedIndexRejectLifecyclePlan maximumChild =
            CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection.Project(new(
                SavedIndexGateCapture: new(
                    EntryEDX: 0,
                    FreshEDXPlus1C: uint.MaxValue,
                    FreshIndexedDword: 1,
                    FreshGlobalValue: uint.MaxValue),
                ChildEntryECX: uint.MaxValue));
        check("Maximum child operands remain captured and equal without unsigned borrow",
            maximumChild.ChildEntryEBX == uint.MaxValue
            && maximumChild.IndexedRejectGate?.CompareLeft == uint.MaxValue
            && maximumChild.IndexedRejectGate?.CompareRight == uint.MaxValue
            && maximumChild.IndexedRejectGate?.CompareResult == 0
            && maximumChild.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && maximumChild.IndexedRejectGate?.BranchTaken == false
            && maximumChild.StopAtRawFileOffsetExclusive == 0x012C926B
            && maximumChild.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeSavedIndexRejectLifecyclePlan overflowChild =
            CapacityFinalCalleeLargeSavedIndexRejectLifecycleProjection.Project(new(
                SavedIndexGateCapture: new(
                    EntryEDX: 0x80000000,
                    FreshEDXPlus1C: 0xABCDEF01,
                    FreshIndexedDword: 1,
                    FreshGlobalValue: 1),
                ChildEntryECX: 0x80000000));
        check("Child signed overflow remains distinct from unsigned JB and preserves all flags",
            overflowChild.IndexedRejectGate?.CompareResult == 0x7FFFFFFF
            && overflowChild.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && overflowChild.IndexedRejectGate?.BranchTaken == false
            && overflowChild.IndexedRejectGate?.BranchSelectedDestinationRawFileOffset
                == 0x012C926B
            && overflowChild.StopAtRawFileOffsetExclusive == 0x012C926B
            && overflowChild.ExactCleanExitAdmitted);
    }

    private static CapacityFinalCalleeLargeSavedIndexRejectLifecycleFlags?
        ConvertParentFlags(CapacityFinalCalleeLargeSavedIndexGateFlags? flags) =>
            flags is CapacityFinalCalleeLargeSavedIndexGateFlags value
                ? new(
                    value.Carry,
                    value.Parity,
                    value.AuxiliaryCarry,
                    value.Zero,
                    value.Sign,
                    value.Overflow)
                : null;
}
