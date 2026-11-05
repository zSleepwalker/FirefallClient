using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonNodeFreshMaskLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan zeroParent =
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.Project(
                new(EntryEDI: 0, LateEntryEBP: null, EntryESI: 0x3000,
                    LateFreshGlobalValue: 0));
        check("Parent-zero completion derives node EDX=1 and ECX=0 before the fresh read",
            zeroParent.Outcome
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                    .ZeroTestResultToBounded93AC
            && zeroParent.ParentLifecycle?.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InitializedAt93A1
            && zeroParent.ParentNodeInitializationCompleted
            && zeroParent.NodeFreshMaskHandoffValidated
            && zeroParent.ChildEDX == 1
            && zeroParent.ChildEDXDerivedFromNodeMove
            && zeroParent.ChildECX == 0
            && zeroParent.ChildECXDerivedFromParentIndex
            && zeroParent.FreshGlobalReadAddress == 0x01EDADCC
            && zeroParent.Internal93A1Entered
            && zeroParent.StopAtRawFileOffsetExclusive == 0x012C93AC
            && zeroParent.ExactCleanHandoff
            && zeroParent.ExactCleanExitAdmitted);

        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan aboveCeiling =
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.Project(
                new(EntryEDI: 0x01000000, LateEntryEBP: null, EntryESI: 0x3000,
                    LateFreshGlobalValue: 0x80000000));
        check("Above-ceiling completion derives ECX=31 and branches using the captured global",
            aboveCeiling.ParentLifecycle?.ECXSource
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource
                    .ParentAboveCeilingMove31
            && aboveCeiling.ChildECX == 31
            && aboveCeiling.FreshMaskGate?.ShiftResult == 0x80000000
            && aboveCeiling.FreshMaskGate?.TestResult == 0x80000000
            && aboveCeiling.Outcome
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                    .NonzeroTestResultToBounded93C0
            && aboveCeiling.ParentLifecycle?.FrameStoreCommitted == false
            && aboveCeiling.ExactCleanExitAdmitted
            && aboveCeiling.StopAtRawFileOffsetExclusive == 0x012C93C0);

        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan indexZero =
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.Project(
                new(EntryEDI: 0x100, LateEntryEBP: 0x2000, EntryESI: 0x3000,
                    LateFreshGlobalValue: uint.MaxValue));
        check("Index-child ECX=0 preserves parent flags across SHL before TEST replaces them",
            indexZero.ParentLifecycle?.ECXSource
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource.IndexChildFinalLEA
            && indexZero.ChildECX == 0
            && indexZero.ChildEDX == 1
            && indexZero.FreshMaskGate?.ShiftCountMasked == 0
            && indexZero.FlagsAtGateEntry.HasValue
            && indexZero.FlagsAfterShift == indexZero.FlagsAtGateEntry
            && indexZero.ShiftCountZeroPreservedParentFlags
            && indexZero.TestOverwroteFlags
            && indexZero.FlagsAtStop == ConvertFlags(indexZero.FreshMaskGate?.TestFlags)
            && indexZero.FreshMaskGate?.TestResult == 1
            && indexZero.FreshMaskGate?.EntryEDXOperandConsumed == true
            && indexZero.FreshMaskGate?.EntryECXOperandConsumed == true
            && indexZero.FreshMaskGate?.EntryEDXCaptureProvidedButUnused == false
            && indexZero.FreshMaskGate?.EntryECXCaptureProvidedButUnused == false
            && indexZero.FreshMaskGate?.JnzReached == true
            && indexZero.ParentLifecycle?.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C9374, 0x012C9384, 0x012C9393, 0x012C939A
                }) == true
            && indexZero.ExactCleanHandoff
            && indexZero.ExactCleanExitAdmitted);

        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan index31 =
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.Project(
                new(EntryEDI: 0x00FFFF00, LateEntryEBP: 0x2000, EntryESI: 0x3000,
                    LateFreshGlobalValue: 0x80000000));
        check("Index-child ECX=31 is inherited from the parent LEA and selects the nonzero exit",
            index31.ParentLifecycle?.ParentLifecycle?.IndexLifecycle?.FinalIndexECX == 31
            && index31.ChildECX == 31
            && index31.ChildEDX == 1
            && index31.FreshMaskGate?.ShiftCountMasked == 31
            && index31.FreshMaskGate?.ShiftResult == 0x80000000
            && index31.Outcome
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                    .NonzeroTestResultToBounded93C0
            && index31.ExactCleanExitAdmitted);

        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan missingEDI =
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.Project(
                new(EntryEDI: null, LateEntryEBP: 0x2000, EntryESI: 0x3000,
                    LateFreshGlobalValue: 0xFFFFFFFF));
        check("Missing parent EDI stops before node initialization and leaves the late global unused",
            missingEDI.Outcome
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.MissingEntryEDIAt9354
            && missingEDI.ParentLifecycle?.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.MissingEntryEDIAt9354
            && missingEDI.Writes.IsEmpty
            && !missingEDI.FreshMaskGateReached
            && missingEDI.UnreachedLateFreshGlobalValueCaptureProvided
            && missingEDI.LateFreshGlobalValueProvidedButUnused
            && missingEDI.StopAtRawFileOffsetExclusive == 0x012C9354);

        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan missingEBP =
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.Project(
                new(EntryEDI: 0x100, LateEntryEBP: null, EntryESI: 0x3000,
                    LateFreshGlobalValue: 0xFFFFFFFF));
        check("Missing required late EBP stops at its frame store without reaching the fresh gate",
            missingEBP.Outcome
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                    .MissingLateEntryEBPAt9374
            && missingEBP.ParentLifecycle?.FrameStoreAttempted == true
            && missingEBP.ParentLifecycle?.FrameStoreCommitted == false
            && missingEBP.Writes.IsEmpty
            && !missingEBP.FreshMaskGateReached
            && missingEBP.LateFreshGlobalValueProvidedButUnused
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C9374);

        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan missingESI =
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.Project(
                new(EntryEDI: 0x100, LateEntryEBP: 0x2000, EntryESI: null,
                    LateFreshGlobalValue: 0xFFFFFFFF));
        check("Missing ESI retains the parent frame write and stops before the node/global accesses",
            missingESI.Outcome
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.MissingEntryESIAt9384
            && missingESI.ParentLifecycle?.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.MissingEntryESIAt9384
            && missingESI.Writes.Length == 1
            && missingESI.Writes[0].Stage
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.ParentFrameStore
            && !missingESI.FreshMaskGateReached
            && missingESI.LateFreshGlobalValueProvidedButUnused
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9384);

        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan missingGlobal =
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.Project(
                new(EntryEDI: 0x100, LateEntryEBP: 0x2000, EntryESI: 0x3000,
                    LateFreshGlobalValue: null));
        check("Missing global stops at 93A1 after all four stores and preserves parent flags",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                    .MissingFreshGlobalValueAt93A1
            && missingGlobal.FreshMaskGateReached
            && missingGlobal.Internal93A1Entered
            && missingGlobal.FreshGlobalReadReached
            && !missingGlobal.FreshGlobalValueCaptured
            && missingGlobal.FreshGlobalReadAddress == 0x01EDADCC
            && missingGlobal.Writes.Length == 4
            && missingGlobal.OrderedMemoryEffects.Length == 5
            && missingGlobal.OrderedMemoryEffects[^1] is
            {
                Kind: CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind
                    .FreshGlobalRead,
                RawFileOffset: 0x012C93A1,
                Address: 0x01EDADCC,
                ValueCaptured: false,
                ForwardedFromPriorWrite: false
            }
            && missingGlobal.FreshGlobalReadAfterAllNodeWrites
            && missingGlobal.FlagsAtStop
                == ConvertFlags(missingGlobal.ParentLifecycle?.FlagsAtStop)
            && missingGlobal.FreshMaskGate?.StopAtRawFileOffsetExclusive == 0x012C93A1
            && missingGlobal.FreshMaskGate?.ShiftInstructionReached == false
            && missingGlobal.FreshMaskGate?.TestReached == false
            && missingGlobal.FreshMaskGate?.JnzReached == false
            && missingGlobal.FreshMaskGate?.EntryEDXCaptureProvidedButUnused == true
            && missingGlobal.FreshMaskGate?.EntryECXCaptureProvidedButUnused == true
            && missingGlobal.FreshMaskGate?.UnreachedEntryEDXCaptureProvided == true
            && missingGlobal.FreshMaskGate?.UnreachedEntryECXCaptureProvided == true
            && missingGlobal.NodeFreshMaskHandoffValidated
            && missingGlobal.ExactCleanHandoff
            && !missingGlobal.BoundedBranchDestinationSelected
            && !missingGlobal.ExactCleanExitAdmitted);

        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan dirtyUnusedEBP =
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.Project(
                new(EntryEDI: 0, LateEntryEBP: 0x2000, EntryESI: 0x3000,
                    LateFreshGlobalValue: 0));
        check("Parent bypass retains supplied unused EBP but cannot claim a clean handoff",
            dirtyUnusedEBP.ParentLifecycle?.LateEntryEBPProvidedButUnused == true
            && dirtyUnusedEBP.ParentLifecycle?.LateEntryEBPUnusedOnBypass == true
            && dirtyUnusedEBP.ParentLifecycle?.LateEntryEBPConsumed == false
            && dirtyUnusedEBP.ParentLifecycle?.Capture.LateEntryEBP == 0x2000
            && dirtyUnusedEBP.ParentLifecycle?.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InitializedAt93A1
            && dirtyUnusedEBP.ParentNodeInitializationCompleted
            && dirtyUnusedEBP.Outcome
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome.ParentNotCleanAt93A1
            && dirtyUnusedEBP.FreshMaskGate is null
            && !dirtyUnusedEBP.NodeFreshMaskHandoffValidated
            && dirtyUnusedEBP.ChildEDX is null
            && dirtyUnusedEBP.ChildECX is null
            && !dirtyUnusedEBP.FreshMaskGateReached
            && !dirtyUnusedEBP.Internal93A1Entered
            && !dirtyUnusedEBP.FreshGlobalReadReached
            && dirtyUnusedEBP.LateFreshGlobalValueProvidedButUnused
            && dirtyUnusedEBP.UnreachedLateFreshGlobalValueCaptureProvided
            && dirtyUnusedEBP.Writes.Length == 3
            && dirtyUnusedEBP.OrderedMemoryEffects.Length == 3
            && dirtyUnusedEBP.OrderedMemoryEffects.All(effect =>
                effect.Kind == CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.Write)
            && dirtyUnusedEBP.Writes.Select(write => write.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C9384, 0x012C9393, 0x012C939A })
            && dirtyUnusedEBP.FlagsAtStop
                == ConvertFlags(dirtyUnusedEBP.ParentLifecycle?.FlagsAtStop)
            && !dirtyUnusedEBP.ParentExactCleanAdmission
            && !dirtyUnusedEBP.ExactCleanHandoff
            && !dirtyUnusedEBP.ExactCleanExitAdmitted
            && dirtyUnusedEBP.StopAtRawFileOffsetExclusive == 0x012C93A1);

        uint aliasESI = 0x01EDADCC
            - CapacityFinalCalleeCommonIndexedNodeInitializationProjection.ThirdStoreFieldOffset;
        CapacityFinalCalleeCommonNodeFreshMaskLifecyclePlan aliasRead =
            CapacityFinalCalleeCommonNodeFreshMaskLifecycleProjection.Project(
                new(EntryEDI: 0, LateEntryEBP: null, EntryESI: aliasESI,
                    LateFreshGlobalValue: 1));
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffect aliasWrite =
            aliasRead.OrderedMemoryEffects.Single(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind.Write
                && effect.Address == 0x01EDADCC);
        check("Aliasing third store does not forward into the independent late global read",
            aliasWrite.Value == 0
            && aliasRead.OrderedMemoryEffects[^1] is
            {
                Kind: CapacityFinalCalleeCommonNodeFreshMaskLifecycleMemoryEffectKind
                    .FreshGlobalRead,
                Address: 0x01EDADCC,
                Value: 1,
                ValueCaptured: true,
                ForwardedFromPriorWrite: false
            }
            && aliasRead.FreshGlobalReadAfterAllNodeWrites
            && !aliasRead.FreshGlobalValueForwardedFromNodeWrite
            && aliasRead.FreshMaskGate?.TestResult == 1
            && aliasRead.Outcome
                == CapacityFinalCalleeCommonNodeFreshMaskLifecycleOutcome
                    .NonzeroTestResultToBounded93C0);

        check("Both bounded branch targets remain unentered after the exact gate",
            zeroParent.BoundedBranchDestinationSelected
            && index31.BoundedBranchDestinationSelected
            && !zeroParent.ZeroDestinationInstructionEntered
            && !zeroParent.NonzeroDestinationInstructionEntered
            && !index31.ZeroDestinationInstructionEntered
            && !index31.NonzeroDestinationInstructionEntered
            && !zeroParent.InstructionBeyondBoundaryExecuted
            && !index31.InstructionBeyondBoundaryExecuted
            && zeroParent.StopAtRawFileOffsetExclusive == 0x012C93AC
            && index31.StopAtRawFileOffsetExclusive == 0x012C93C0);
    }

    private static CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonFreshMaskGateFlags? flags) =>
        flags is CapacityFinalCalleeCommonFreshMaskGateFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;

    private static CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? flags) =>
        flags is CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;
}
