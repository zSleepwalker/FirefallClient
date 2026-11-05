using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonNodeSelfRingLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan parentZero =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(0, null, 0x3000, 0));
        check("The parent-zero size path derives EBX from LEA and enters the exact zero child",
            parentZero.Outcome
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.Completed
            && parentZero.Parent.ParentLifecycle?.ECXSource
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource.ParentZeroXor
            && parentZero.Parent.ParentLifecycle?.DerivedECX == 0
            && parentZero.Parent.ParentLifecycle?.DerivedEBX == 0x01EDAEF8
            && parentZero.Parent.ParentLifecycle?.DerivedEDX == 1
            && parentZero.ParentDerivedNodeEBX == 0x01EDAEF8
            && parentZero.ChildEntryEAX == 0
            && parentZero.ChildEntryEDX == 1
            && parentZero.ChildEntryEBX == 0x01EDAEF8
            && parentZero.ChildEntryESI == 0x3000
            && parentZero.Parent.ParentLifecycle?.LateEntryEBPProvidedButUnused == false
            && parentZero.Parent.ParentLifecycle?.LateEntryEBPUnusedOnBypass == false
            && parentZero.Parent.FreshMaskGate?.EntryEDXCaptureProvidedButUnused == false
            && parentZero.Parent.FreshMaskGate?.EntryECXCaptureProvidedButUnused == false
            && !parentZero.Parent.LateFreshGlobalValueProvidedButUnused
            && parentZero.ExactZeroChildHandoff
            && parentZero.Internal93ACEntered
            && parentZero.ExactChildCompletion
            && parentZero.ExactBoundedExitAdmitted
            && parentZero.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan indexZero =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(0x100, 0x2000, 0x3000, 0));
        check("Index-zero derives the base EBX and preserves zero-count shift flags before TEST",
            indexZero.Parent.ParentLifecycle?.ECXSource
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource.IndexChildFinalLEA
            && indexZero.Parent.ParentLifecycle?.DerivedECX == 0
            && indexZero.ParentDerivedNodeEBX == 0x01EDAEF8
            && indexZero.Parent.ChildECX == 0
            && indexZero.Parent.ChildEDX == 1
            && indexZero.Parent.FlagsAtGateEntry.HasValue
            && indexZero.Parent.FlagsAfterShift == indexZero.Parent.FlagsAtGateEntry
            && indexZero.Parent.ShiftCountZeroPreservedParentFlags
            && indexZero.Parent.FreshMaskGate?.TestReached == true
            && ConvertFlags(indexZero.Parent.FlagsAtStop)
                == ConvertFlags(indexZero.Parent.FreshMaskGate?.TestFlags)
            && indexZero.ExactChildCompletion);

        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan index31 =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(0x00FFFF00, 0x2000, 0x3000, 0x7FFFFFFF));
        check("Index-thirty-one derives the scaled LEA EBX and shifts node EDX into the high bit",
            index31.Parent.ParentLifecycle?.ParentLifecycle?.IndexLifecycle?.FinalIndexECX == 31
            && index31.Parent.ParentLifecycle?.DerivedECX == 31
            && index31.ParentDerivedNodeEBX == 0x01EDAF74
            && index31.Parent.FreshMaskGate?.ShiftInputEDX == 1
            && index31.Parent.FreshMaskGate?.ShiftCountMasked == 31
            && index31.Parent.FreshMaskGate?.ShiftResult == 0x80000000
            && index31.Parent.FreshMaskGate?.TestResult == 0
            && index31.ChildEntryEAX == 0x7FFFFFFF
            && index31.ChildEntryEDX == 0x80000000
            && index31.ChildEntryEBX == 0x01EDAF74
            && index31.ChildEntryESI == 0x3000
            && index31.ExactZeroChildHandoff
            && index31.ExactChildCompletion);

        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan aboveCeiling =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(0x01000000, null, 0x3000, 0x80000000));
        check("Above-ceiling derives EBX from index thirty-one and bypasses the child on nonzero TEST",
            aboveCeiling.Parent.ParentLifecycle?.ECXSource
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource
                    .ParentAboveCeilingMove31
            && aboveCeiling.Parent.ParentLifecycle?.DerivedECX == 31
            && aboveCeiling.ParentDerivedNodeEBX == 0x01EDAF74
            && aboveCeiling.Parent.FreshMaskGate?.ShiftResult == 0x80000000
            && aboveCeiling.Parent.FreshMaskGate?.TestResult == 0x80000000
            && aboveCeiling.Outcome
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome
                    .NonzeroTestResultBypassesChild
            && aboveCeiling.Parent.FreshMaskGate?.JnzTaken == true
            && aboveCeiling.Parent.FreshMaskGate?.JnzSelectedDestinationRawFileOffset
                == 0x012C93C0
            && aboveCeiling.Child is null
            && aboveCeiling.ExactNonzeroParentBypass
            && aboveCeiling.ExactBoundedExitAdmitted
            && aboveCeiling.ParentTestFlagsPreserved
            && aboveCeiling.StopAtRawFileOffsetExclusive == 0x012C93C0);

        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan missingEDI =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(null, 0x2000, 0x3000, 0));
        check("Missing parent EDI stops before every node store and the global read",
            missingEDI.Outcome
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingEntryEDIAt9354
            && missingEDI.Parent.Writes.IsEmpty
            && missingEDI.Parent.FreshMaskGate is null
            && missingEDI.Child is null
            && missingEDI.OrderedMemoryEffects.IsEmpty
            && missingEDI.StopAtRawFileOffsetExclusive == 0x012C9354);

        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan missingEBP =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(0x100, null, 0x3000, 0));
        check("Missing required late EBP stops at its parent frame store without entering either child",
            missingEBP.Outcome
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingLateEntryEBPAt9374
            && missingEBP.Parent.ParentLifecycle?.FrameStoreAttempted == true
            && missingEBP.Parent.ParentLifecycle?.FrameStoreCommitted == false
            && missingEBP.Parent.Writes.IsEmpty
            && missingEBP.Parent.FreshMaskGate is null
            && missingEBP.Child is null
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C9374);

        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan missingESI =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(0, null, null, 0));
        check("Missing ESI preserves only earlier parent writes and stops before node initialization",
            missingESI.Outcome
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.MissingEntryESIAt9384
            && missingESI.Parent.Writes.Length == 0
            && missingESI.Parent.FreshMaskGate is null
            && missingESI.Child is null
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9384);

        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan missingGlobal =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(0, null, 0x3000, null));
        check("A missing fresh global read stops at 93A1 without shifting, testing, or entering the child",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome
                    .MissingFreshGlobalValueAt93A1
            && missingGlobal.Parent.Writes.Length == 3
            && missingGlobal.Parent.OrderedMemoryEffects.Length == 4
            && missingGlobal.Parent.FreshGlobalReadReached
            && !missingGlobal.Parent.FreshGlobalValueCaptured
            && missingGlobal.Parent.FreshGlobalReadAfterAllNodeWrites
            && missingGlobal.Parent.FreshGlobalValueForwardedFromNodeWrite == false
            && missingGlobal.Parent.FreshMaskGate?.ShiftInstructionReached == false
            && missingGlobal.Parent.FreshMaskGate?.TestReached == false
            && missingGlobal.Parent.FreshMaskGate?.JnzReached == false
            && missingGlobal.Child is null
            && missingGlobal.FlagsAtStop == ConvertFlags(missingGlobal.Parent.FlagsAtStop)
            && missingGlobal.ParentTestFlagsPreserved
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C93A1);

        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan dirtyUnusedEBP =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(0, 0x2000, 0x3000, 0));
        check("A dirty unused EBP blocks the fresh read and zero-child handoff at 93A1",
            dirtyUnusedEBP.Outcome
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.ParentNotCleanAt93A1
            && dirtyUnusedEBP.Parent.ParentLifecycle?.LateEntryEBPProvidedButUnused == true
            && dirtyUnusedEBP.Parent.ParentLifecycle?.LateEntryEBPUnusedOnBypass == true
            && dirtyUnusedEBP.Parent.ParentLifecycle?.LateEntryEBPConsumed == false
            && dirtyUnusedEBP.Parent.Writes.Length == 3
            && dirtyUnusedEBP.Parent.FreshMaskGate is null
            && !dirtyUnusedEBP.Parent.FreshGlobalReadReached
            && dirtyUnusedEBP.Child is null
            && dirtyUnusedEBP.StopAtRawFileOffsetExclusive == 0x012C93A1);

        uint aliasedGlobalESI = 0x01EDADCC - 0x10;
        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan independentGlobal =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(0x00FFFF00, 0x2000, aliasedGlobalESI, 1));
        check("All frame/node writes precede an independent aliased global read and five child writes",
            independentGlobal.Outcome
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.Completed
            && independentGlobal.Parent.Writes.Length == 4
            && independentGlobal.Parent.Writes[0].Stage
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.ParentFrameStore
            && independentGlobal.Parent.Writes[^1].Stage
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeThirdStore
            && independentGlobal.Parent.Writes[^1].Address == 0x01EDADCC
            && independentGlobal.Parent.Writes[^1].Value == 0
            && independentGlobal.Parent.FreshMaskGate?.Capture.FreshGlobalValue == 1
            && independentGlobal.Parent.FreshMaskGate?.TestResult == 0
            && independentGlobal.FreshGlobalReadAfterAllParentWrites
            && !independentGlobal.FreshGlobalValueForwardedFromNodeWrite
            && independentGlobal.OrderedMemoryEffects.Select(effect => effect.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ParentFrameWrite,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ParentNodeWrite,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ParentNodeWrite,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ParentNodeWrite,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .IndependentFreshGlobalRead,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ChildGlobalWrite,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ChildPointerStore,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ChildPointerStore,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ChildPointerStore,
                    CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ChildPointerStore
                })
            && independentGlobal.ExactOrderedMemoryEffects
            && independentGlobal.ExactChildCompletion);

        uint nodeEBX = 0x01EDAEF8;
        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan aliasedChildStores =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(0, null, nodeEBX - 0x18, 0));
        check("Aliased child pointer stores remain separate and retain instruction order",
            aliasedChildStores.Child?.Writes.Length == 5
            && aliasedChildStores.Child?.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C93AEu, 0x01EDADCCu, 1u),
                    (0x012C93B3u, nodeEBX, nodeEBX - 0x18),
                    (0x012C93B5u, nodeEBX, nodeEBX),
                    (0x012C93B8u, nodeEBX - 0x0Cu, nodeEBX - 0x18),
                    (0x012C93BBu, nodeEBX - 0x10, nodeEBX - 0x18)
                }) == true
            && aliasedChildStores.OrderedMemoryEffects.Count(effect =>
                effect.Kind
                    == CapacityFinalCalleeCommonNodeSelfRingLifecycleMemoryEffectKind
                        .ChildPointerStore
                && effect.Address == nodeEBX) == 2
            && aliasedChildStores.ExactOrderedMemoryEffects
            && aliasedChildStores.Child?.FlagsPreserved == true);

        CapacityFinalCalleeCommonNodeSelfRingLifecyclePlan wrapped =
            CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection.Project(
                new(0, null, 0xFFFFFFF0, 0));
        check("Node and child pointer addresses wrap as DWORDs without dropping writes",
            wrapped.Outcome == CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.Completed
            && wrapped.Parent.Writes.Select(write => write.Address).SequenceEqual(new[]
            {
                0x0000000Cu, 0x00000004u, 0x00000000u
            })
            && wrapped.Child?.Writes.Select(write => write.Address).SequenceEqual(new[]
            {
                0x01EDADCCu, 0x01EDAEF8u, 0x00000008u, 0xFFFFFFFCu, 0xFFFFFFF8u
            }) == true
            && wrapped.OrderedMemoryEffects.Length == 9
            && wrapped.ExactOrderedMemoryEffects
            && wrapped.ExactChildCompletion);

        check("Parent TEST flags survive a nonzero bypass; child OR replaces them and stores/JMP preserve OR flags",
            ConvertFlags(aboveCeiling.Parent.FlagsAtStop)
                == ConvertFlags(aboveCeiling.Parent.FreshMaskGate?.TestFlags)
            && aboveCeiling.ParentTestFlagsPreserved
            && index31.ChildORFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && index31.ChildFlagsOverrideParentTest
            && index31.FlagsAtStop == index31.ChildORFlags
            && index31.ChildStoresAndJumpPreserveFlags
            && index31.FlagsPreserved);

        check("The zero TEST handoff enters 93AC and stops after the 942F jump without entering its target",
            parentZero.Parent.FreshMaskGate?.Outcome
                == CapacityFinalCalleeCommonFreshMaskGateOutcome.ZeroTestResultToBounded93AC
            && parentZero.Parent.FreshMaskGate?.TestReached == true
            && parentZero.Parent.FreshMaskGate?.TestResult == 0
            && parentZero.Parent.FreshMaskGate?.JnzReached == true
            && parentZero.Parent.FreshMaskGate?.JnzTaken == false
            && parentZero.Parent.FreshMaskGate?.JnzSelectedDestinationRawFileOffset
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection
                    .ZeroDestinationRawFileOffset
            && parentZero.Parent.FreshMaskGate?.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleProjection
                    .ChildEntryRawFileOffset
            && parentZero.Internal93ACEntered
            && parentZero.Child?.JumpTargetRawFileOffset == 0x012C942F
            && parentZero.Child?.JumpTargetVirtualAddress == 0x016CA02F
            && parentZero.ChildJumpReached
            && !parentZero.ChildJumpTargetEntered
            && parentZero.StopAtRawFileOffsetExclusive == 0x012C942F
            && parentZero.ExactBoundedExitAdmitted);
    }

    private static CapacityFinalCalleeCommonNodeSelfRingLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonFreshMaskGateFlags? flags) =>
        flags is CapacityFinalCalleeCommonFreshMaskGateFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;

    private static CapacityFinalCalleeCommonNodeSelfRingLifecycleFlags? ConvertFlags(
        CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags? flags) =>
        flags is CapacityFinalCalleeCommonNodeFreshMaskLifecycleFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;
}
