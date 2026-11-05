using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonNodeRingCleanupLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan parentZero =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0, null, 0x3000, 0, 1));
        check("The parent-zero route derives all child inputs and enters cleanup only after its five stores",
            parentZero.Outcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .CounterDecrementedToZeroBeforeCall943C
            && parentZero.ParentChild.ParentDerivedNodeEBX == 0x01EDAEF8
            && parentZero.ParentChild.ChildEntryEAX == 0
            && parentZero.ParentChild.ChildEntryEDX == 1
            && parentZero.ParentChild.ChildEntryEBX == 0x01EDAEF8
            && parentZero.ParentChild.ChildEntryESI == 0x3000
            && parentZero.ExactParentZeroChildHandoff
            && parentZero.ExactParentWrites
            && parentZero.ExactFiveChildWrites
            && parentZero.ExactParentChildCompletion
            && parentZero.ExactCounterGateAdmission
            && parentZero.Internal942FEntryReached
            && !parentZero.DestinationEntered
            && parentZero.CleanupCounterReadAttempted
            && parentZero.CleanupCounterValueCaptured
            && parentZero.CleanupCounterWriteCommitted
            && parentZero.CounterBefore == 1
            && parentZero.CounterAfter == 0
            && parentZero.CounterGate.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && parentZero.JnzTaken == false
            && parentZero.SelectedDestinationRawFileOffset == 0x012C9437
            && parentZero.StopAtRawFileOffsetExclusive == 0x012C943C
            && parentZero.ExactCleanupCounterGateCompletion
            && parentZero.ExactBoundedExitAdmitted
            && !parentZero.FollowingCallReached
            && !parentZero.CalleeEntered);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan indexZero =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0x100, 0x2000, 0x3000, 0, 0));
        check("Index-zero composes the initialized node into a wrapping DEC and exact 9441 branch",
            indexZero.ParentChild.Parent.ParentLifecycle?.ECXSource
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource.IndexChildFinalLEA
            && indexZero.ParentChild.Parent.ParentLifecycle?.DerivedECX == 0
            && indexZero.ParentChild.ParentDerivedNodeEBX == 0x01EDAEF8
            && indexZero.ParentChild.ChildEntryEDX == 1
            && indexZero.Outcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .CounterDecrementedNonzeroTo9441
            && indexZero.CounterBefore == 0
            && indexZero.CounterAfter == uint.MaxValue
            && indexZero.CounterGate.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && indexZero.JnzTaken == true
            && indexZero.SelectedDestinationRawFileOffset == 0x012C9441
            && indexZero.StopAtRawFileOffsetExclusive == 0x012C9441
            && indexZero.ExactCleanupCounterGateCompletion
            && !indexZero.CounterGate.MovEcxReached
            && !indexZero.FollowingCallReached
            && !indexZero.CalleeEntered);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan index31 =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0x00FFFF00, 0x2000, 0x3000, 1, uint.MaxValue));
        check("Index-thirty-one derives the scaled node and preserves its high-bit mask through cleanup",
            index31.ParentChild.Parent.ParentLifecycle?.DerivedECX == 31
            && index31.ParentChild.ParentDerivedNodeEBX == 0x01EDAF74
            && index31.ParentChild.Parent.FreshMaskGate?.ShiftResult == 0x80000000
            && index31.ParentChild.ChildEntryEAX == 1
            && index31.ParentChild.ChildEntryEDX == 0x80000000
            && index31.ParentChild.ChildORFlags?.Carry == false
            && index31.CounterBefore == uint.MaxValue
            && index31.CounterAfter == uint.MaxValue - 1
            && index31.CounterGate.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && index31.SelectedDestinationRawFileOffset == 0x012C9441
            && index31.StopAtRawFileOffsetExclusive == 0x012C9441
            && index31.ExactParentChildCompletion
            && index31.ExactCleanupCounterGateCompletion);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan aboveCeiling =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0x01000000, null, 0x3000, 0x80000000, 7));
        check("A nonzero TEST selects 93C0 and leaves the late cleanup counter visibly unused",
            aboveCeiling.Outcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .NonzeroTestResultBypassesCleanupAt93C0
            && aboveCeiling.ParentChild.Parent.ParentLifecycle?.DerivedECX == 31
            && aboveCeiling.ParentChild.Parent.FreshMaskGate?.TestResult == 0x80000000
            && aboveCeiling.ParentChild.Parent.FreshMaskGate?.JnzTaken == true
            && aboveCeiling.ParentChild.Parent.FreshMaskGate?.JnzSelectedDestinationRawFileOffset
                == 0x012C93C0
            && !aboveCeiling.ChildReached
            && !aboveCeiling.Internal942FEntryReached
            && !aboveCeiling.CleanupCounterReadAttempted
            && aboveCeiling.LateFreshCleanupCounterDwordProvidedButUnused
            && aboveCeiling.UnreachedLateFreshCleanupCounterDwordCaptureProvided
            && aboveCeiling.SelectedDestinationRawFileOffset == 0x012C93C0
            && aboveCeiling.StopAtRawFileOffsetExclusive == 0x012C93C0
            && !aboveCeiling.ExactParentNonzeroBypass
            && aboveCeiling.ParentChild.ExactNonzeroParentBypass
            && !aboveCeiling.ExactCleanExitAdmitted
            && !aboveCeiling.ExactBoundedExitAdmitted
            && !aboveCeiling.DestinationEntered);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan cleanNonzeroBypass =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0x01000000, null, 0x3000, 0x80000000, null));
        check("The exact bounded nonzero bypass is admitted only when no unused counter capture is supplied",
            cleanNonzeroBypass.Outcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .NonzeroTestResultBypassesCleanupAt93C0
            && cleanNonzeroBypass.ParentChild.ExactNonzeroParentBypass
            && !cleanNonzeroBypass.LateFreshCleanupCounterDwordProvidedButUnused
            && cleanNonzeroBypass.ExactParentNonzeroBypass
            && cleanNonzeroBypass.ExactCleanExitAdmitted
            && cleanNonzeroBypass.ExactBoundedExitAdmitted
            && !cleanNonzeroBypass.Internal942FEntryReached
            && !cleanNonzeroBypass.DestinationEntered);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan missingEDI =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(null, 0x2000, 0x3000, 0, 5));
        check("Missing parent EDI stops before writes, global or counter reads, and cleanup entry",
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.EntryRawFileOffset
                == 0x012C9354
            && missingEDI.Outcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome.MissingEntryEDIAt9354
            && missingEDI.ParentChild.Parent.Writes.IsEmpty
            && missingEDI.OrderedMemoryEffects.IsEmpty
            && !missingEDI.CleanupCounterReadAttempted
            && missingEDI.LateFreshCleanupCounterDwordProvidedButUnused
            && missingEDI.StopAtRawFileOffsetExclusive == 0x012C9354);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan missingEBP =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0x100, null, 0x3000, 0, 5));
        check("Missing required late EBP stops at 9374 without any child or cleanup counter access",
            missingEBP.Outcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome.MissingLateEntryEBPAt9374
            && missingEBP.ParentChild.Parent.ParentLifecycle?.FrameStoreAttempted == true
            && missingEBP.ParentChild.Parent.ParentLifecycle?.FrameStoreCommitted == false
            && missingEBP.ParentChild.Parent.Writes.IsEmpty
            && !missingEBP.CleanupCounterReadAttempted
            && missingEBP.LateFreshCleanupCounterDwordProvidedButUnused
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C9374);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan missingESI =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0, null, null, 0, 5));
        check("Missing ESI stops before node stores, child entry, and the cleanup counter",
            missingESI.Outcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome.MissingEntryESIAt9384
            && missingESI.ParentChild.Parent.Writes.IsEmpty
            && missingESI.ParentChild.Parent.FreshMaskGate is null
            && !missingESI.CleanupCounterReadAttempted
            && missingESI.LateFreshCleanupCounterDwordProvidedButUnused
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9384);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan dirtyUnusedEBP =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0, 0x2000, 0x3000, 0, 5));
        check("A dirty unused EBP blocks fresh-mask admission and keeps the cleanup counter unused",
            dirtyUnusedEBP.Outcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome.ParentNotCleanAt93A1
            && dirtyUnusedEBP.ParentChild.Parent.ParentLifecycle?.LateEntryEBPProvidedButUnused
                == true
            && dirtyUnusedEBP.ParentChild.Parent.ParentLifecycle?.LateEntryEBPConsumed == false
            && dirtyUnusedEBP.ParentChild.Parent.Writes.Length == 3
            && !dirtyUnusedEBP.ParentChild.Parent.FreshGlobalReadReached
            && !dirtyUnusedEBP.CleanupCounterReadAttempted
            && dirtyUnusedEBP.LateFreshCleanupCounterDwordProvidedButUnused
            && dirtyUnusedEBP.StopAtRawFileOffsetExclusive == 0x012C93A1);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan missingFreshGlobal =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0, null, 0x3000, null, 5));
        check("A missing fresh global preserves completed parent stores but prevents child and counter access",
            missingFreshGlobal.Outcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .MissingFreshGlobalValueAt93A1
            && missingFreshGlobal.ParentChild.Parent.Writes.Length == 3
            && missingFreshGlobal.ParentChild.Parent.FreshGlobalReadReached
            && !missingFreshGlobal.ParentChild.Parent.FreshGlobalValueCaptured
            && !missingFreshGlobal.CleanupCounterReadAttempted
            && missingFreshGlobal.LateFreshCleanupCounterDwordProvidedButUnused
            && missingFreshGlobal.StopAtRawFileOffsetExclusive == 0x012C93A1);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan missingCounter =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0, null, 0x3000, 0, null));
        check("A missing cleanup counter attempts its read after all five child writes and preserves OR flags",
            missingCounter.Outcome
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleOutcome
                    .MissingFreshCounterDwordAt942F
            && missingCounter.ExactParentChildCompletion
            && missingCounter.ExactFiveChildWrites
            && missingCounter.Internal942FEntryReached
            && missingCounter.CleanupCounterReadAttempted
            && !missingCounter.CleanupCounterValueCaptured
            && !missingCounter.CleanupCounterWriteReached
            && !missingCounter.CleanupCounterDecrementExecuted
            && missingCounter.CounterGate.CounterAccesses.Length == 1
            && missingCounter.CounterGate.CounterAccesses[0].Kind
                == CapacityFinalCalleeCleanupCounterGateAccessKind.Read
            && !missingCounter.CounterGate.CounterAccesses[0].ValueCaptured
            && missingCounter.CounterGate.ReachedInstructions.IsEmpty
            && missingCounter.ParentORFlagsPreservedWhenCounterUnavailable
            && missingCounter.FlagsAtStop == missingCounter.ParentORFlags
            && missingCounter.DerivedIncomingCarryFlag == false
            && missingCounter.CounterGate.CarryAtStop == false
            && !missingCounter.CounterGate.JnzReached
            && !missingCounter.FollowingCallReached
            && !missingCounter.CalleeEntered
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan unusedCounterCapture =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0x100, null, 0x3000, 0, 9));
        check("A provided late counter remains unused when the parent route stops before its entry",
            unusedCounterCapture.LateFreshCleanupCounterDwordProvidedButUnused
            && unusedCounterCapture.CounterGate.UnreachedFreshCounterDwordCaptureProvided
            && !unusedCounterCapture.CounterGate.CounterReadReached
            && !unusedCounterCapture.CounterGate.CounterWriteReached
            && unusedCounterCapture.CounterGate.CounterAccesses.IsEmpty
            && unusedCounterCapture.StopAtRawFileOffsetExclusive == 0x012C9374);

        CapacityFinalCalleeCommonNodeRingCleanupLifecycleCapture aliasedCapture =
            new(
                0x00FFFF00,
                0x2000,
                unchecked(CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection
                    .CounterVirtualAddress - 0x1Cu),
                1,
                3);
        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan aliased =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                aliasedCapture);
        uint aliasAddress = CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection
            .CounterVirtualAddress;
        check("The aliased cleanup counter is freshly read after all committed parent and child writes",
            aliased.ParentChild.Outcome
                == CapacityFinalCalleeCommonNodeSelfRingLifecycleOutcome.Completed
            && aliased.ParentChild.Parent.Writes.Any(write =>
                write.Address == aliasAddress && write.Value == 31)
            && aliased.CounterGate.CounterAccesses.Length == 2
            && aliased.CounterGate.CounterAccesses[0].Kind
                == CapacityFinalCalleeCleanupCounterGateAccessKind.Read
            && aliased.CounterGate.CounterAccesses[0].Address == aliasAddress
            && aliased.CounterGate.CounterAccesses[0].Value == 3
            && aliased.CounterGate.CounterAccesses[1].Kind
                == CapacityFinalCalleeCleanupCounterGateAccessKind.Write
            && aliased.CounterGate.CounterAccesses[1].Address == aliasAddress
            && aliased.CounterGate.CounterAccesses[1].Value == 2
            && aliased.CounterBefore == 3
            && aliased.CounterAfter == 2
            && aliased.CounterReadAfterParentMemoryEffects
            && aliased.FreshCleanupCounterForwardedFromPriorWrite == false
            && aliased.ExactOrderedMemoryEffects
            && aliased.OrderedMemoryEffects.All(effect => !effect.ForwardedFromPriorWrite)
            && aliased.OrderedMemoryEffects[^2].Kind
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                    .CleanupCounterRead
            && !aliased.OrderedMemoryEffects[^2].Committed
            && aliased.OrderedMemoryEffects[^2].ValueCaptured
            && aliased.OrderedMemoryEffects[^2].Value == 3
            && aliased.OrderedMemoryEffects[^1].Kind
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                    .CleanupCounterWrite
            && aliased.OrderedMemoryEffects[^1].Committed
            && aliased.OrderedMemoryEffects[^1].ValueCaptured
            && aliased.OrderedMemoryEffects[^1].Value == 2);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan globalAlias =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0x00FFFF00, 0x2000, 0x01EDADCC - 0x10u, 1, 3));
        var globalAliasEffects = globalAlias.OrderedMemoryEffects
            .Where(effect => effect.Address == 0x01EDADCC)
            .ToArray();
        check("The fresh global read remains independent of the aliased node zero-store before the child global write",
            globalAlias.ParentChild.Parent.Writes.Any(write =>
                write.Address == 0x01EDADCC && write.Value == 0)
            && globalAlias.ParentChild.FreshGlobalReadAfterAllParentWrites
            && globalAlias.ParentChild.Parent.FreshMaskGate?.Capture.FreshGlobalValue == 1
            && globalAlias.ParentChild.Parent.FreshGlobalValueForwardedFromNodeWrite == false
            && globalAlias.ParentChild.Child?.Writes[0].Address == 0x01EDADCC
            && globalAlias.ParentChild.Child?.Writes[0].Value == 0x80000001
            && globalAliasEffects.Length == 3
            && globalAliasEffects[0].Kind
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                    .ParentNodeWrite
            && globalAliasEffects[0].Value == 0
            && globalAliasEffects[0].Committed
            && !globalAliasEffects[0].ForwardedFromPriorWrite
            && globalAliasEffects[1].Kind
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                    .IndependentFreshGlobalRead
            && globalAliasEffects[1].Value == 1
            && globalAliasEffects[1].ValueCaptured
            && !globalAliasEffects[1].Committed
            && !globalAliasEffects[1].ForwardedFromPriorWrite
            && globalAliasEffects[2].Kind
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleMemoryEffectKind
                    .ChildGlobalWrite
            && globalAliasEffects[2].Value == 0x80000001
            && globalAliasEffects[2].Committed
            && !globalAliasEffects[2].ForwardedFromPriorWrite
            && globalAlias.ExactOrderedMemoryEffects);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan wrapped =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0, null, 0xFFFFFFF0, 0, 1));
        check("Wrapping parent and child pointer stores remain ordered before the cleanup read and write",
            wrapped.ParentChild.Parent.Writes.Select(write => write.Address).SequenceEqual(
                new uint[] { 0x0000000Cu, 0x00000004u, 0x00000000u })
            && wrapped.ParentChild.Child?.Writes.Select(write => write.Address).SequenceEqual(
                new uint[]
                {
                    0x01EDADCCu,
                    0x01EDAEF8u,
                    0x00000008u,
                    0xFFFFFFFCu,
                    0xFFFFFFF8u
                })
                == true
            && wrapped.OrderedMemoryEffects[^2].Address
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection
                    .CounterVirtualAddress
            && wrapped.OrderedMemoryEffects[^1].Address
                == CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection
                    .CounterVirtualAddress
            && wrapped.ExactOrderedMemoryEffects
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C943C);

        CapacityFinalCalleeCommonNodeRingCleanupLifecyclePlan signTransition =
            CapacityFinalCalleeCommonNodeRingCleanupLifecycleProjection.Project(
                new(0, null, 0x3000, 0, 0x80000000));
        check("DEC flags replace child OR flags only after execution and take the nonzero branch",
            signTransition.DerivedIncomingCarryFlag == false
            && signTransition.IncomingCarryDerivedFromChildOR
            && signTransition.CounterGate.Capture.IncomingCarryFlag == false
            && signTransition.CounterAfter == 0x7FFFFFFF
            && signTransition.CounterGate.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signTransition.FlagsAtStop
                == new CapacityFinalCalleeCommonNodeRingCleanupLifecycleFlags(
                    false, true, true, false, false, true)
            && signTransition.JnzTaken == true
            && signTransition.StopAtRawFileOffsetExclusive == 0x012C9441);

        check("The cleanup DEC CF input is derived from child OR rather than an independent capture",
            signTransition.ParentChild.ChildORReached
            && signTransition.ParentChild.ChildORFlags?.Carry == false
            && signTransition.CounterGate.Capture.IncomingCarryFlag
                == signTransition.ParentChild.ChildORFlags?.Carry
            && signTransition.IncomingCarryDerivedFromChildOR
            && signTransition.CounterGate.FlagsAfterDecrement?.Carry == false
            && signTransition.ExactCleanupCounterGateCompletion
            && !signTransition.FollowingCallReached
            && !signTransition.CalleeEntered);
    }
}
