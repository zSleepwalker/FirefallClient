using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("All five native instruction windows use full raw/flat addresses and mapped VAs",
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.EntryRawFileOffset
                == 0x012C926B
            && CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.EntryVirtualAddress
                == 0x016C9E6B
            && CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection
                .ContinuationBoundaryRawFileOffsetExclusive == 0x012C9278
            && CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection
                .RawFileOffsetToVirtualAddress(0x012C9278) == 0x016C9E78
            && Normalize(CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection
                .CapturedWindowBytes) == "3951107505894110EB03894114"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan equal =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Project(new(
                new(
                    EntryECX: 0x1000,
                    EntryEDX: 0x12345678,
                    FreshECXPlus10: 0x12345678),
                EntryEAX: 0));
        check("Equality composes the parent's 9270 path, stores at captured ECX+10, then jumps",
            equal.ParentGateExactCompletion
            && equal.ParentGateOutcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.EqualToFallthrough9270
            && equal.ParentGate.BranchSelectedDestinationRawFileOffset == 0x012C9270
            && equal.ChildBaseCaptured
            && equal.ChildBase == 0x1000
            && equal.StoreInstructionReached
            && equal.StoreFieldOffset == 0x10
            && equal.StoreAddress == 0x1010
            && equal.StoreValueCaptured
            && equal.StoreValue == 0
            && equal.Writes is
                [{
                    RawFileOffset: 0x012C9270,
                    Address: 0x1010,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Value: 0,
                    Committed: true
                }]
            && equal.FinalJumpReached
            && equal.FinalJumpDestinationRawFileOffset == 0x012C9278
            && equal.Events.Select(item => item.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.Compare,
                CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.ConditionalBranch,
                CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.Store,
                CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.UnconditionalJump
            })
            && equal.Events[1].SelectedDestinationRawFileOffset == 0x012C9270
            && equal.Events[3].SelectedDestinationRawFileOffset == 0x012C9278
            && equal.ReachedInstructions.SequenceEqual(new uint[]
                { 0x012C926B, 0x012C926E, 0x012C9270, 0x012C9273 })
            && equal.FlagsAtStop == equal.ParentGate.FlagsAtStop
            && equal.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.ExactCleanCompletion
            && equal.StopAtRawFileOffsetExclusive == 0x012C9278
            && !equal.ContinuationEntered);

        CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan unequal =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Project(new(
                new(
                    EntryECX: 0xFFFFFFF8,
                    EntryEDX: 0x22222222,
                    FreshECXPlus10: 0x11111111),
                EntryEAX: 0xAABBCCDD));
        check("Inequality composes the parent's 9275 branch and stores at wrapped captured ECX+14",
            unequal.ParentGateExactCompletion
            && unequal.ParentGateOutcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.UnequalToJnz9275
            && unequal.ParentGate.BranchSelectedDestinationRawFileOffset == 0x012C9275
            && unequal.ChildBase == 0xFFFFFFF8
            && unequal.StoreInstructionReached
            && unequal.StoreFieldOffset == 0x14
            && unequal.StoreAddress == 0xC
            && unequal.Writes is
                [{
                    RawFileOffset: 0x012C9275,
                    Address: 0xC,
                    ValueCaptured: true,
                    Value: 0xAABBCCDD,
                    Committed: true
                }]
            && !unequal.FinalJumpReached
            && unequal.FinalJumpDestinationRawFileOffset is null
            && unequal.Events.Select(item => item.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.Compare,
                CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.ConditionalBranch,
                CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.Store
            })
            && unequal.Events[1].SelectedDestinationRawFileOffset == 0x012C9275
            && unequal.ReachedInstructions.SequenceEqual(new uint[]
                { 0x012C926B, 0x012C926E, 0x012C9275 })
            && unequal.FlagsAtStop == unequal.ParentGate.FlagsAtStop
            && unequal.FlagsAtStop is
            {
                Zero: false,
                Sign: true
            }
            && unequal.ExactCleanCompletion
            && unequal.StopAtRawFileOffsetExclusive == 0x012C9278
            && !unequal.ContinuationEntered);

        CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan missingParentECX =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Project(new(
                new(
                    EntryECX: null,
                    EntryEDX: 0x22222222,
                    FreshECXPlus10: 0x11111111),
                EntryEAX: 0x33333333));
        check("A missing parent ECX keeps EDX but leaves child EAX unused and issues no store",
            missingParentECX.ParentGateOutcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingEntryECX
            && missingParentECX.ParentGate.CompareRight == 0x22222222
            && !missingParentECX.ParentGate.FreshECXPlus10ReadReached
            && missingParentECX.ParentGate.Reads.IsEmpty
            && !missingParentECX.ChildBaseCaptured
            && missingParentECX.ChildBase is null
            && !missingParentECX.StoreInstructionReached
            && missingParentECX.Writes.IsEmpty
            && missingParentECX.UnreachedEntryEAXCaptureProvided
            && missingParentECX.FlagsAtStop is null
            && missingParentECX.ReachedInstructions.SequenceEqual(new[] { 0x012C926Bu })
            && !missingParentECX.ExactCleanCompletion
            && missingParentECX.StopAtRawFileOffsetExclusive == 0x012C926B);

        CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan missingFresh =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Project(new(
                new(
                    EntryECX: 0x200,
                    EntryEDX: 0x12345678,
                    FreshECXPlus10: null),
                EntryEAX: 0x87654321));
        check("A missing parent field value retains its attempted read and EDX, not child EAX",
            missingFresh.ParentGateOutcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingFreshECXPlus10
            && missingFresh.ParentGate.CompareRight == 0x12345678
            && missingFresh.ParentGate.FreshECXPlus10ReadReached
            && missingFresh.Reads is
                [{
                    RawFileOffset: 0x012C926B,
                    Address: 0x210,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null
                }]
            && !missingFresh.ChildBaseCaptured
            && !missingFresh.StoreInstructionReached
            && missingFresh.Writes.IsEmpty
            && missingFresh.UnreachedEntryEAXCaptureProvided
            && missingFresh.FlagsAtStop is null
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C926B);

        CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan missingParentEDX =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Project(new(
                new(
                    EntryECX: 0x200,
                    EntryEDX: null,
                    FreshECXPlus10: 0x12345678),
                EntryEAX: 0x87654321));
        check("A missing parent EDX retains the fresh read/value but cannot reach either store",
            missingParentEDX.ParentGateOutcome
                == CapacityFinalCalleeLargeIndexedFieldGateOutcome.MissingEntryEDX
            && missingParentEDX.ParentGate.CompareLeft == 0x12345678
            && missingParentEDX.ParentGate.CompareRight is null
            && missingParentEDX.Reads is
                [{
                    Address: 0x210,
                    AddressCaptured: true,
                    ValueCaptured: true,
                    Value: 0x12345678
                }]
            && !missingParentEDX.ChildBaseCaptured
            && !missingParentEDX.StoreInstructionReached
            && missingParentEDX.Writes.IsEmpty
            && missingParentEDX.UnreachedEntryEAXCaptureProvided
            && missingParentEDX.FlagsAtStop is null
            && missingParentEDX.StopAtRawFileOffsetExclusive == 0x012C926B);

        CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan missingEqualStoreEAX =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Project(new(
                new(
                    EntryECX: 0x1000,
                    EntryEDX: 0,
                    FreshECXPlus10: 0),
                EntryEAX: null));
        check("Missing late EAX retains the equal-store address without committing or jumping",
            missingEqualStoreEAX.ParentGateExactCompletion
            && missingEqualStoreEAX.StoreInstructionReached
            && missingEqualStoreEAX.ChildBase == 0x1000
            && missingEqualStoreEAX.StoreFieldOffset == 0x10
            && missingEqualStoreEAX.StoreAddress == 0x1010
            && !missingEqualStoreEAX.StoreValueCaptured
            && missingEqualStoreEAX.Writes is
                [{
                    RawFileOffset: 0x012C9270,
                    Address: 0x1010,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && !missingEqualStoreEAX.UnreachedEntryEAXCaptureProvided
            && !missingEqualStoreEAX.FinalJumpReached
            && missingEqualStoreEAX.ReachedInstructions.SequenceEqual(
                new uint[] { 0x012C926B, 0x012C926E, 0x012C9270 })
            && missingEqualStoreEAX.FlagsAtStop
                == missingEqualStoreEAX.ParentGate.FlagsAtStop
            && missingEqualStoreEAX.FlagsAtStop?.Zero == true
            && !missingEqualStoreEAX.ExactCleanCompletion
            && missingEqualStoreEAX.StopAtRawFileOffsetExclusive == 0x012C9270);

        CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan missingUnequalStoreEAX =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Project(new(
                new(
                    EntryECX: 0x3000,
                    EntryEDX: 2,
                    FreshECXPlus10: 1),
                EntryEAX: null));
        check("Missing late EAX on inequality retains ECX+14 but stops before fallthrough",
            missingUnequalStoreEAX.ParentGateExactCompletion
            && missingUnequalStoreEAX.StoreInstructionReached
            && missingUnequalStoreEAX.StoreFieldOffset == 0x14
            && missingUnequalStoreEAX.StoreAddress == 0x3014
            && missingUnequalStoreEAX.Writes is
                [{
                    RawFileOffset: 0x012C9275,
                    Address: 0x3014,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && !missingUnequalStoreEAX.FinalJumpReached
            && missingUnequalStoreEAX.FlagsAtStop
                == missingUnequalStoreEAX.ParentGate.FlagsAtStop
            && missingUnequalStoreEAX.FlagsAtStop?.Zero == false
            && !missingUnequalStoreEAX.ExactCleanCompletion
            && missingUnequalStoreEAX.StopAtRawFileOffsetExclusive == 0x012C9275);

        CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan wrappedEqual =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Project(new(
                new(
                    EntryECX: 0xFFFFFFF0,
                    EntryEDX: 0x55555555,
                    FreshECXPlus10: 0x55555555),
                EntryEAX: uint.MaxValue));
        check("Equal-path child address wraps from captured ECX independently of memory data",
            wrappedEqual.Reads[0].Address == 0
            && wrappedEqual.ChildBase == 0xFFFFFFF0
            && wrappedEqual.StoreAddress == 0
            && wrappedEqual.StoreValue == uint.MaxValue
            && wrappedEqual.Writes[0].Committed
            && wrappedEqual.ParentGate.CompareLeft == 0x55555555
            && wrappedEqual.ParentGate.CompareRight == 0x55555555
            && wrappedEqual.ParentGate.FlagsAtStop?.Zero == true);

        CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan zeroValues =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Project(new(
                new(
                    EntryECX: 0,
                    EntryEDX: 0,
                    FreshECXPlus10: 0),
                EntryEAX: 0));
        check("Captured zero EAX is a committed store and MOV/JMP preserve all CMP flags",
            zeroValues.StoreAddress == 0x10
            && zeroValues.StoreValueCaptured
            && zeroValues.StoreValue == 0
            && zeroValues.Writes[0].Committed
            && zeroValues.FlagsAtStop == zeroValues.ParentGate.CompareFlags
            && zeroValues.Events[^1].Kind
                == CapacityFinalCalleeLargeIndexedFieldStoreLifecycleEventKind.UnconditionalJump
            && zeroValues.Events[^1].Flags == zeroValues.ParentGate.CompareFlags
            && zeroValues.ExactCleanCompletion);

        CapacityFinalCalleeLargeIndexedFieldStoreLifecyclePlan alias =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Project(new(
                new(
                    EntryECX: 0x1000,
                    EntryEDX: 0xABCDEF01,
                    FreshECXPlus10: 0xABCDEF01),
                EntryEAX: 0x76543210));
        check("The equal-path fresh read and store may alias without replacing the retained read",
            alias.Reads.Length == 1
            && alias.Reads[0].Address == alias.StoreAddress
            && alias.Reads[0].Value == 0xABCDEF01
            && alias.Reads[0].RawFileOffset == 0x012C926B
            && alias.Writes.Length == 1
            && alias.Writes[0].RawFileOffset == 0x012C9270
            && alias.Writes[0].Value == 0x76543210
            && alias.Reads[0].Value != alias.Writes[0].Value
            && alias.FlagsAtStop == alias.ParentGate.FlagsAtStop);

        check("The projection stops before 9278 and contains no TEST at that boundary",
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Instructions
                .All(instruction => instruction.RawFileOffset
                    != CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection
                        .ContinuationBoundaryRawFileOffsetExclusive)
            && equal.ReachedInstructions.All(raw =>
                raw != CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection
                    .ContinuationBoundaryRawFileOffsetExclusive)
            && unequal.ReachedInstructions.All(raw =>
                raw != CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection
                    .ContinuationBoundaryRawFileOffsetExclusive)
            && !equal.ContinuationEntered
            && !unequal.ContinuationEntered);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C926B, "39 51 10", null, null),
            (0x012C926E, "75 05", 0x012C9275, 0x016C9E75),
            (0x012C9270, "89 41 10", null, null),
            (0x012C9273, "EB 03", 0x012C9278, 0x016C9E78),
            (0x012C9275, "89 41 14", null, null)
        };
        uint[] expectedVas =
        {
            0x016C9E6B, 0x016C9E6E, 0x016C9E70, 0x016C9E73, 0x016C9E75
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        CapacityFinalCalleeLargeIndexedFieldStoreLifecycleInstructionEvidence[] instructions =
            CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.Instructions.ToArray();
        foreach (CapacityFinalCalleeLargeIndexedFieldStoreLifecycleInstructionEvidence instruction
            in instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return instructions.Length == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection
                    .ContinuationBoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeIndexedFieldStoreLifecycleProjection.CapturedWindowBytes)
            && instructions.Select(instruction =>
                (instruction.RawFileOffset,
                    instruction.Bytes,
                    instruction.BranchTargetRawFileOffset,
                    instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected)
            && instructions.Select(instruction => instruction.VirtualAddress)
                .SequenceEqual(expectedVas);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
