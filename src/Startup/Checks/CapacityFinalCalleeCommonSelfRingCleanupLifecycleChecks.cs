using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonSelfRingCleanupLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed parent, self-ring, and cleanup-counter instruction evidence is exact",
            InstructionEvidenceMatches());

        CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan zeroToZero =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 1, 0x80, 0x400, 1));
        check("A zero mask completes all self-ring stores before DEC reaches zero",
            zeroToZero.Outcome
                == CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                    .CounterDecrementedToZero
            && zeroToZero.FreshMaskOutcome
                == CapacityFinalCalleeCommonFreshMaskGateOutcome.ZeroTestResultToBounded93AC
            && zeroToZero.SelfRingOutcome
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome.Completed
            && zeroToZero.SelfRing.ChildEntryEAX == 0x80000000
            && zeroToZero.SelfRing.ChildEntryEDX == 0x40000000
            && zeroToZero.SelfRing.Child?.OrResult == 0xC0000000
            && zeroToZero.SelfRing.ChildORFlags is { Carry: false }
            && zeroToZero.ParentFlagsPreservedThroughSelfRing
            && zeroToZero.CleanupIncomingCarryFlagDerivedFromSelfRingOR
            && zeroToZero.CleanupCounterGate?.Capture.IncomingCarryFlag == false
            && zeroToZero.CleanupCounterGate?.Capture.EntryValidated == true
            && zeroToZero.CleanupCounterGate?.CounterBefore == 1
            && zeroToZero.CleanupCounterGate?.CounterAfter == 0
            && zeroToZero.CleanupCarryPreserved
            && zeroToZero.CleanupCounterGate?.FlagsAfterDecrement is
            {
                Carry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroToZero.AllSelfRingWritesCommitted
            && zeroToZero.SelfRing.ChildWrites.Length == 5
            && CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection
                .IsExactSelfRingHandoff(zeroToZero.SelfRing)
            && zeroToZero.SelfRing.ChildJumpReached
            && zeroToZero.Internal942FEntered
            && zeroToZero.CounterDecrementExecuted
            && !zeroToZero.DestinationEntered
            && zeroToZero.CleanupCounterGate?.JnzTaken == false
            && zeroToZero.CleanupCounterGate?.MovEcxReached == true
            && zeroToZero.CleanupCounterGate?.EcxValue == 0x01EDADC8
            && zeroToZero.CleanupEcxValue == 0x01EDADC8
            && !zeroToZero.EcxImmediateReadAsMemory
            && zeroToZero.StopAtRawFileOffsetExclusive == 0x012C943C
            && zeroToZero.ExactCounterBranchCompletion
            && zeroToZero.ExactBoundedExitAdmitted
            && !zeroToZero.CleanupJnzTargetEntered
            && !zeroToZero.CleanupCounterCallReached
            && !zeroToZero.CalleeEntered
            && !zeroToZero.StackAccessPerformed);

        check("The cleanup read and DEC write follow the five ordered, committed self-ring stores",
            zeroToZero.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C93A1,
                    0x012C93AE,
                    0x012C93B3,
                    0x012C93B5,
                    0x012C93B8,
                    0x012C93BB,
                    0x012C942F,
                    0x012C942F
                })
            && zeroToZero.OrderedMemoryEffects.Take(6).All(effect => effect.Committed
                || effect.Kind
                    == CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                        .FreshGlobalRead)
            && zeroToZero.OrderedMemoryEffects[6] is
            {
                Kind: CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                    .CleanupCounterRead,
                Address: 0x01EDADE8,
                Value: 1,
                Attempted: true,
                ValueCaptured: true,
                Committed: false
            }
            && zeroToZero.OrderedMemoryEffects[7] is
            {
                Kind: CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                    .CleanupCounterWrite,
                Address: 0x01EDADE8,
                Value: 0,
                Attempted: true,
                ValueCaptured: true,
                Committed: true
            });

        CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan zeroCounter =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 1, 0x80, 0x400, 0));
        check("A zero cleanup counter wraps to uint.MaxValue and selects raw 12C9441",
            zeroCounter.Outcome
                == CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                    .CounterDecrementedNonzero
            && zeroCounter.CleanupCounterGate?.CounterAfter == uint.MaxValue
            && zeroCounter.CleanupCounterGate?.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && zeroCounter.CleanupCounterGate?.JnzTaken == true
            && zeroCounter.CleanupCounterGate?.SelectedDestinationRawFileOffset
                == 0x012C9441
            && zeroCounter.StopAtRawFileOffsetExclusive == 0x012C9441
            && zeroCounter.ExactCounterBranchCompletion
            && !zeroCounter.CleanupJnzTargetEntered
            && !zeroCounter.CalleeEntered);

        CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan maxCounter =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 1, 0x80, 0x400, uint.MaxValue));
        check("Decrementing uint.MaxValue selects the nonzero branch without changing CF",
            maxCounter.CleanupCounterGate?.CounterAfter == 0xFFFFFFFE
            && maxCounter.CleanupCounterGate?.FlagsAfterDecrement is
            {
                Carry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && maxCounter.CleanupCounterGate?.SelectedDestinationRawFileOffset == 0x012C9441
            && maxCounter.CleanupCarryPreserved
            && maxCounter.ExactBoundedExitAdmitted);

        CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan overflowCounter =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 1, 0x80, 0x400, 0x80000000));
        check("The signed-boundary decrement sets OF while preserving the OR-derived CF",
            overflowCounter.CleanupCounterGate?.CounterAfter == 0x7FFFFFFF
            && overflowCounter.CleanupCounterGate?.FlagsAfterDecrement is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && overflowCounter.CleanupIncomingCarryFlagDerivedFromSelfRingOR
            && overflowCounter.CleanupCarryPreserved
            && overflowCounter.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan nonzeroMask =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 2, null, null, null));
        check("A nonzero mask bypasses the self-ring and counter when no late captures are supplied",
            nonzeroMask.Outcome
                == CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                    .NonzeroMaskBypassesChild
            && nonzeroMask.SelfRing.Parent.TestResult == 0x80000000
            && nonzeroMask.SelfRing.Parent.JnzTaken == true
            && nonzeroMask.SelfRing.ParentSelectedDestinationRawFileOffset == 0x012C93C0
            && !nonzeroMask.SelfRingReached
            && nonzeroMask.CleanupCounterGate is null
            && !nonzeroMask.Internal942FEntered
            && nonzeroMask.StopAtRawFileOffsetExclusive == 0x012C93C0
            && nonzeroMask.ExactNonzeroMaskBypass
            && nonzeroMask.ExactBoundedExitAdmitted);

        CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan dirtyBypass =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 2, 0x10, 0x20, 7));
        check("Late pointers and a supplied counter remain explicitly unused on the nonzero bypass",
            dirtyBypass.Outcome
                == CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                    .NonzeroMaskBypassesChild
            && dirtyBypass.LateEBXCaptureProvidedButUnused
            && dirtyBypass.LateESICaptureProvidedButUnused
            && dirtyBypass.LateFreshCounterCaptureProvidedButUnused
            && dirtyBypass.CleanupCounterGate is null
            && !dirtyBypass.ExactNonzeroMaskBypass
            && !dirtyBypass.ExactBoundedExitAdmitted
            && dirtyBypass.StopAtRawFileOffsetExclusive == 0x012C93C0);

        CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan missingParent =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Project(
                new(null, 0x20, 1, 0x40, 0x80, 5));
        check("A missing parent global leaves all later pointer and counter captures unused",
            missingParent.Outcome
                == CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                    .MissingFreshGlobalValue
            && missingParent.SelfRing.OrderedMemoryEffects.Length == 1
            && missingParent.LateEBXCaptureProvidedButUnused
            && missingParent.LateESICaptureProvidedButUnused
            && missingParent.LateFreshCounterCaptureProvidedButUnused
            && missingParent.CleanupCounterGate is null
            && !missingParent.Internal942FEntered
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C93A1);

        CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan missingParentOperands =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Project(
                new(0x10, null, null, 0x40, 0x80, 5));
        check("Missing parent EDX and ECX captures stop before the child and leave later inputs unused",
            missingParentOperands.Outcome
                == CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                    .MissingEntryEDXAndEntryECX
            && missingParentOperands.SelfRing.Parent.ShiftInstructionReached
            && !missingParentOperands.SelfRing.Parent.ShiftExecuted
            && !missingParentOperands.SelfRingReached
            && missingParentOperands.LateEBXCaptureProvidedButUnused
            && missingParentOperands.LateESICaptureProvidedButUnused
            && missingParentOperands.LateFreshCounterCaptureProvidedButUnused
            && missingParentOperands.CleanupCounterGate is null
            && missingParentOperands.StopAtRawFileOffsetExclusive == 0x012C93A6);

        CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan missingPointer =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 1, 0x80, null, 5));
        check("A missing late pointer preserves only committed parent writes and never enters 942F",
            missingPointer.Outcome
                == CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome.MissingLateESI
            && missingPointer.SelfRing.ChildGlobalStoreCommitted
            && missingPointer.SelfRing.FirstChildLocalStoreAttempted
            && !missingPointer.SelfRing.FirstChildLocalStoreCommitted
            && missingPointer.OrderedMemoryEffects.Length == 3
            && missingPointer.OrderedMemoryEffects[2] is
            {
                Kind: CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                    .ChildLocalStore,
                RawFileOffset: 0x012C93B3,
                Attempted: true,
                ValueCaptured: false,
                Committed: false
            }
            && missingPointer.LateFreshCounterCaptureProvidedButUnused
            && missingPointer.CleanupCounterGate is null
            && !missingPointer.Internal942FEntered
            && !missingPointer.AllSelfRingWritesCommitted
            && missingPointer.StopAtRawFileOffsetExclusive == 0x012C93B3);

        CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan missingBothPointers =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 1, null, null, 5));
        check("Missing both late pointers retain the global write but bypass the cleanup counter",
            missingBothPointers.Outcome
                == CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                    .MissingBothLatePointers
            && missingBothPointers.SelfRing.ChildGlobalStoreCommitted
            && missingBothPointers.SelfRing.FirstChildLocalStoreAttempted
            && !missingBothPointers.SelfRing.FirstChildLocalStoreCommitted
            && missingBothPointers.LateFreshCounterCaptureProvidedButUnused
            && missingBothPointers.CleanupCounterGate is null
            && !missingBothPointers.Internal942FEntered
            && missingBothPointers.StopAtRawFileOffsetExclusive == 0x012C93B3);

        CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan missingCounter =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 1, 0x80, 0x400, null));
        check("A missing fresh counter follows every self-ring write and preserves the OR flags",
            missingCounter.Outcome
                == CapacityFinalCalleeCommonSelfRingCleanupLifecycleOutcome
                    .MissingFreshCounterDword
            && missingCounter.AllSelfRingWritesCommitted
            && missingCounter.Internal942FEntered
            && !missingCounter.CounterDecrementExecuted
            && missingCounter.CleanupCounterGate?.CounterReadReached == true
            && !missingCounter.CleanupCounterGate!.CounterWriteReached
            && missingCounter.CleanupCounterGate.CounterAccesses.Length == 1
            && missingCounter.CleanupCounterGate.CounterAccesses[0] is
            {
                Kind: CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                Address: 0x01EDADE8,
                ValueCaptured: false,
                Value: null
            }
            && missingCounter.CleanupCounterGate.CarryAtStop == false
            && missingCounter.FlagsAtStop is { } missingCounterFlags
            && missingCounter.SelfRing.ChildORFlags is { } missingCounterOrFlags
            && missingCounterFlags.Carry == missingCounterOrFlags.Carry
            && missingCounterFlags.Parity == missingCounterOrFlags.Parity
            && missingCounterFlags.Auxiliary == missingCounterOrFlags.Auxiliary
            && missingCounterFlags.Zero == missingCounterOrFlags.Zero
            && missingCounterFlags.Sign == missingCounterOrFlags.Sign
            && missingCounterFlags.Overflow == missingCounterOrFlags.Overflow
            && missingCounter.OrderedMemoryEffects.Length == 7
            && missingCounter.OrderedMemoryEffects[^1] is
            {
                Kind: CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                    .CleanupCounterRead,
                Attempted: true,
                ValueCaptured: false,
                Committed: false
            }
            && !missingCounter.LateFreshCounterCaptureProvidedButUnused
            && !missingCounter.ExactCounterBranchCompletion
            && missingCounter.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonSelfRingCleanupLifecyclePlan aliasedCounter =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 1, 0x01EDADE8, 0x01EDADD0, 0x01EDADE8));
        check("Ordered aliased stores precede the independently captured counter read",
            aliasedCounter.OrderedMemoryEffects[2] is
            {
                Kind: CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                    .ChildLocalStore,
                Address: 0x01EDADE8,
                Value: 0x01EDADD0,
                Committed: true
            }
            && aliasedCounter.OrderedMemoryEffects[3] is
            {
                Kind: CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                    .ChildLocalStore,
                Address: 0x01EDADE8,
                Value: 0x01EDADE8,
                Committed: true
            }
            && aliasedCounter.OrderedMemoryEffects[6] is
            {
                Kind: CapacityFinalCalleeCommonSelfRingCleanupLifecycleMemoryEffectKind
                    .CleanupCounterRead,
                Address: 0x01EDADE8,
                Value: 0x01EDADE8,
                ValueCaptured: true
            }
            && aliasedCounter.CleanupCounterGate?.Capture.FreshCounterDword == 0x01EDADE8
            && aliasedCounter.CleanupCounterGate?.CounterBefore == 0x01EDADE8
            && aliasedCounter.CleanupCounterGate?.CounterAfter == 0x01EDADE7
            && aliasedCounter.OrderedMemoryEffects[2].RawFileOffset
                < aliasedCounter.OrderedMemoryEffects[6].RawFileOffset
            && aliasedCounter.OrderedMemoryEffects[3].RawFileOffset
                < aliasedCounter.OrderedMemoryEffects[6].RawFileOffset
            && aliasedCounter.ExactCounterBranchCompletion
            && aliasedCounter.StopAtRawFileOffsetExclusive == 0x012C9441);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes, uint? Operand, uint? Immediate,
            uint? BranchRaw, uint? BranchVirtual)[]
        {
            (0x012C93A1, "A1CCADED01", null, null, null, null),
            (0x012C93A6, "D3E2", null, null, null, null),
            (0x012C93A8, "85D0", null, null, null, null),
            (0x012C93AA, "7514", null, null, 0x012C93C0, 0x016C9FC0),
            (0x012C93AC, "0BC2", null, null, null, null),
            (0x012C93AE, "A3CCADED01", null, null, null, null),
            (0x012C93B3, "8933", null, null, null, null),
            (0x012C93B5, "895E18", null, null, null, null),
            (0x012C93B8, "89760C", null, null, null, null),
            (0x012C93BB, "897608", null, null, null, null),
            (0x012C93BE, "EB6F", null, null, 0x012C942F, 0x016CA02F),
            (0x012C942F, "FF0DE8ADED01", 0x01EDADE8, null, null, null),
            (0x012C9435, "750A", null, null, 0x012C9441, 0x016CA041),
            (0x012C9437, "B9C8ADED01", null, 0x01EDADC8, null, null),
            (0x012C943C, "E85FE4FFFF", null, null, 0x012C78A0, 0x016C84A0)
        };
        CapacityFinalCalleeCommonSelfRingCleanupLifecycleInstructionEvidence[] actual =
            CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection.Instructions
                .Where(instruction => expected.Any(item => item.Raw == instruction.RawFileOffset))
                .ToArray();
        return actual.Length == expected.Length
            && actual.Select(instruction =>
                (instruction.RawFileOffset,
                    Normalize(instruction.Bytes),
                    instruction.MemoryOperandVirtualAddress,
                    instruction.ImmediateValue,
                    instruction.BranchTargetRawFileOffset,
                    instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected)
            && actual.All(instruction =>
                instruction.FlatIdaAddress == instruction.RawFileOffset
                && instruction.VirtualAddress
                    == CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            && CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection
                .RawFileOffsetToVirtualAddress(0x012C942F) == 0x016CA02F
            && CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection
                .EntryRawFileOffset == 0x012C93A1
            && CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection
                .CleanupCounterEntryRawFileOffset
                    == CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection
                        .SelfRingJumpTargetRawFileOffset
            && CapacityFinalCalleeCleanupCounterGateProjection.CalculateJnzTargetRawFileOffset()
                == 0x012C9441
            && Normalize(
                CapacityFinalCalleeCommonSelfRingCleanupLifecycleProjection
                    .CleanupCounterWindowBytes)
                == "FF0DE8ADED01750AB9C8ADED01E85FE4FFFF";
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Where(Uri.IsHexDigit)).ToUpperInvariant();
}
