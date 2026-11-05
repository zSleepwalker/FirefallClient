using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed instruction and destination evidence is exact",
            EvidenceMatches());

        CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan missingGlobal =
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Project(
                new(null, 0x20, 1, 0x40, 0x80));
        check("A missing parent global read stops at the MOV and preserves its read attempt",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .MissingFreshGlobalValue
            && missingGlobal.Parent.ReadAttempts.Length == 1
            && missingGlobal.Parent.ReadAttempts[0] is
            {
                RawFileOffset: 0x012C93A1,
                AbsoluteAddress: 0x01EDADCC,
                ReadReached: true,
                ValueCaptured: false,
                Value: null
            }
            && missingGlobal.Child is null
            && missingGlobal.FlagsAtStop is null
            && missingGlobal.LateEBXCaptureProvidedButUnused
            && missingGlobal.LateESICaptureProvidedButUnused
            && missingGlobal.ParentSelectedDestinationRawFileOffset is null
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C93A1
            && missingGlobal.OrderedMemoryEffects.Length == 1);

        CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan missingParentOperand =
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Project(
                new(0x10, null, 3, 0x40, null));
        check("Missing parent operands retain the reached read and do not enter the child",
            missingParentOperand.Outcome
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .MissingEntryEDX
            && missingParentOperand.Parent.FreshGlobalReadReached
            && missingParentOperand.Parent.FreshGlobalValueCaptured
            && missingParentOperand.Parent.ShiftInstructionReached
            && !missingParentOperand.Parent.ShiftExecuted
            && missingParentOperand.Parent.FlagsAtStop is null
            && missingParentOperand.Child is null
            && missingParentOperand.LateEBXCaptureProvidedButUnused
            && !missingParentOperand.LateESICaptureProvidedButUnused
            && missingParentOperand.StopAtRawFileOffsetExclusive == 0x012C93A6);

        CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan nonzeroBypass =
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 2, null, null));
        check("A nonzero TEST branch bypasses the child and leaves parent TEST flags at stop",
            nonzeroBypass.Outcome
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .NonzeroTestResultBypassesChild
            && nonzeroBypass.Parent.ShiftResult == 0x80000000
            && nonzeroBypass.Parent.TestResult == 0x80000000
            && nonzeroBypass.Parent.JnzTaken == true
            && nonzeroBypass.ParentSelectedDestinationRawFileOffset == 0x012C93C0
            && nonzeroBypass.Child is null
            && !nonzeroBypass.LateEBXCaptureProvidedButUnused
            && !nonzeroBypass.LateESICaptureProvidedButUnused
            && nonzeroBypass.FlagsAtStop == nonzeroBypass.ParentFlagsAtStop
            && nonzeroBypass.ParentFlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonzeroBypass.ParentTestFlagsPreserved
            && nonzeroBypass.FlagsPreserved
            && nonzeroBypass.ExactNonzeroParentBypass
            && nonzeroBypass.ExactBoundedExitAdmitted
            && nonzeroBypass.StopAtRawFileOffsetExclusive == 0x012C93C0
            && nonzeroBypass.OrderedMemoryEffects.Length == 1);

        CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan nonzeroWithUnusedPointers =
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 2, 0x10, 0x20));
        check("A nonzero branch with unused late pointers is not an exact clean exit",
            nonzeroWithUnusedPointers.Outcome
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome
                    .NonzeroTestResultBypassesChild
            && nonzeroWithUnusedPointers.LateEBXCaptureProvidedButUnused
            && nonzeroWithUnusedPointers.LateESICaptureProvidedButUnused
            && !nonzeroWithUnusedPointers.ExactNonzeroParentBypass
            && !nonzeroWithUnusedPointers.ExactBoundedExitAdmitted
            && nonzeroWithUnusedPointers.ParentSelectedDestinationRawFileOffset
                == 0x012C93C0
            && nonzeroWithUnusedPointers.StopAtRawFileOffsetExclusive == 0x012C93C0);

        CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan derivedHandoff =
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Project(
                new(0x80000000, 0x20000000, 1, 0x80, 0x400));
        check("The child OR operands derive only from the parent MOV and shifted EDX",
            derivedHandoff.ParentOutcome
                == CapacityFinalCalleeCommonFreshMaskGateOutcome
                    .ZeroTestResultToBounded93AC
            && derivedHandoff.ExactZeroChildHandoff
            && derivedHandoff.Parent.JnzTaken == false
            && derivedHandoff.Parent.JnzSelectedDestinationRawFileOffset == 0x012C93AC
            && derivedHandoff.Parent.StopAtRawFileOffsetExclusive == 0x012C93AC
            && !derivedHandoff.Parent.EntryEDXCaptureProvidedButUnused
            && !derivedHandoff.Parent.EntryECXCaptureProvidedButUnused
            && !derivedHandoff.Parent.UnreachedEntryEDXCaptureProvided
            && !derivedHandoff.Parent.UnreachedEntryECXCaptureProvided
            && derivedHandoff.Parent.ShiftResult == 0x40000000
            && derivedHandoff.Parent.TestResult == 0
            && derivedHandoff.ChildEntryEAX == 0x80000000
            && derivedHandoff.ChildEntryEDX == 0x40000000
            && derivedHandoff.Child?.OrResult == 0xC0000000
            && derivedHandoff.Child?.Capture.EntryEAX == 0x80000000
            && derivedHandoff.Child?.Capture.EntryEDX == 0x40000000
            && derivedHandoff.Child?.Capture.LateEBX == 0x80
            && derivedHandoff.Child?.Capture.LateESI == 0x400
            && derivedHandoff.ChildFlagsOverrideParentTest
            && !derivedHandoff.ParentTestFlagsPreserved
            && derivedHandoff.ChildORFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && derivedHandoff.FlagsAtStop == derivedHandoff.ChildORFlags
            && derivedHandoff.ChildStoresAndJumpPreserveFlags
            && derivedHandoff.FlagsPreserved
            && derivedHandoff.ExactChildCompletion
            && derivedHandoff.ChildWrites.Length == 5
            && derivedHandoff.ChildJumpReached
            && !derivedHandoff.ChildJumpTargetEntered
            && derivedHandoff.ChildJumpTargetRawFileOffset == 0x012C942F
            && derivedHandoff.ChildJumpTargetVirtualAddress == 0x016CA02F
            && derivedHandoff.StopAtRawFileOffsetExclusive == 0x012C942F
            && derivedHandoff.ExactBoundedExitAdmitted);

        CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan zeroCapture =
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Project(
                new(0, 0, 0, 0, 0));
        check("Captured zero operands and pointers execute the child path as valid values",
            zeroCapture.Outcome
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome.Completed
            && zeroCapture.ChildEntryEAX == 0
            && zeroCapture.ChildEntryEDX == 0
            && zeroCapture.Child?.OrResult == 0
            && zeroCapture.ChildORFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroCapture.Child?.Writes.Length == 5
            && zeroCapture.Child?.FirstLocalStoreAddress == 0
            && zeroCapture.Child?.FirstLocalStoreValue == 0
            && zeroCapture.Child?.SecondLocalStoreAddress == 0x18
            && zeroCapture.Child?.ThirdLocalStoreAddress == 0x0C
            && zeroCapture.Child?.FourthLocalStoreAddress == 8
            && zeroCapture.ChildJumpReached
            && zeroCapture.StopAtRawFileOffsetExclusive == 0x012C942F);

        CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan partialLate =
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Project(
                new(0x10, 0, 0, 0x50, null));
        check("The child global write survives an incomplete first local-store attempt",
            partialLate.Outcome
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome.MissingLateESI
            && partialLate.ChildGlobalStoreReached
            && partialLate.ChildGlobalStoreCommitted
            && partialLate.Child?.GlobalStoreValue == 0x10
            && partialLate.Child?.FirstLocalStoreAddress == 0x50
            && partialLate.Child?.FirstLocalStoreValue is null
            && partialLate.Child is { FirstLocalStoreReached: false }
            && partialLate.FirstChildLocalStoreAttempted
            && !partialLate.FirstChildLocalStoreCommitted
            && partialLate.ChildWrites.Length == 1
            && partialLate.OrderedMemoryEffects.Length == 3
            && partialLate.OrderedMemoryEffects[0].Kind
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind
                    .FreshGlobalRead
            && partialLate.OrderedMemoryEffects[1] is
            {
                Kind: CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind
                    .ChildGlobalWrite,
                RawFileOffset: 0x012C93AE,
                Address: 0x01EDADCC,
                Value: 0x10,
                Attempted: true,
                Committed: true
            }
            && partialLate.OrderedMemoryEffects[2] is
            {
                Kind: CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind
                    .ChildLocalStore,
                RawFileOffset: 0x012C93B3,
                Address: 0x50,
                Value: null,
                InstructionReached: false,
                Attempted: true,
                ValueCaptured: false,
                Committed: false
            }
            && partialLate.FlagsAtStop == partialLate.ChildORFlags
            && partialLate.ChildStoresAndJumpPreserveFlags
            && !partialLate.ExactChildCompletion
            && !partialLate.ExactBoundedExitAdmitted
            && partialLate.StopAtRawFileOffsetExclusive == 0x012C93B3);

        CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan missingEBX =
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Project(
                new(0x40, 0x10, 1, null, 0x30));
        check("A missing late EBX preserves captured ESI without committing its local store",
            missingEBX.Outcome
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome.MissingLateEBX
            && missingEBX.Child?.FirstLocalStoreAddress is null
            && missingEBX.Child?.FirstLocalStoreValue == 0x30
            && missingEBX.OrderedMemoryEffects.Length == 3
            && missingEBX.OrderedMemoryEffects[2] is
            {
                Kind: CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleMemoryEffectKind
                    .ChildLocalStore,
                RawFileOffset: 0x012C93B3,
                Address: null,
                Value: 0x30,
                InstructionReached: false,
                Attempted: true,
                ValueCaptured: true,
                Committed: false
            }
            && missingEBX.ChildGlobalStoreCommitted
            && !missingEBX.ExactChildCompletion
            && missingEBX.StopAtRawFileOffsetExclusive == 0x012C93B3);

        CapacityFinalCalleeCommonFreshMaskSelfRingLifecyclePlan aliasAndWrap =
            CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Project(
                new(1, 0, 0, 8, 0xFFFFFFF0));
        check("Aliased DWORD-wrapped local stores remain separate and instruction-ordered",
            aliasAndWrap.Outcome
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleOutcome.Completed
            && aliasAndWrap.ChildWrites.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C93AEu, 0x01EDADCCu, 1u),
                    (0x012C93B3u, 8u, 0xFFFFFFF0u),
                    (0x012C93B5u, 8u, 8u),
                    (0x012C93B8u, 0xFFFFFFFCu, 0xFFFFFFF0u),
                    (0x012C93BBu, 0xFFFFFFF8u, 0xFFFFFFF0u)
                })
            && aliasAndWrap.OrderedMemoryEffects.Select(effect => effect.RawFileOffset)
                .SequenceEqual(new[]
                {
                    0x012C93A1u,
                    0x012C93AEu,
                    0x012C93B3u,
                    0x012C93B5u,
                    0x012C93B8u,
                    0x012C93BBu
                })
            && aliasAndWrap.FlagsPreserved
            && aliasAndWrap.StopAtRawFileOffsetExclusive == 0x012C942F);
    }

    private static bool EvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes, uint? BranchTarget)[]
        {
            (0x012C93A1, "A1CCADED01", null),
            (0x012C93A6, "D3E2", null),
            (0x012C93A8, "85D0", null),
            (0x012C93AA, "7514", 0x012C93C0),
            (0x012C93AC, "0BC2", null),
            (0x012C93AE, "A3CCADED01", null),
            (0x012C93B3, "8933", null),
            (0x012C93B5, "895E18", null),
            (0x012C93B8, "89760C", null),
            (0x012C93BB, "897608", null),
            (0x012C93BE, "EB6F", 0x012C942F)
        };
        uint nextRawOffset = 0x012C93A1;
        int evidenceIndex = 0;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleInstructionEvidence
            instruction in
                CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Instructions)
        {
            if (evidenceIndex >= expected.Length)
            {
                return false;
            }

            var expectedInstruction = expected[evidenceIndex];
            string instructionBytes = Convert.ToHexString(instruction.Bytes.AsSpan());
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.RawFileOffset != expectedInstruction.Raw
                || instructionBytes != expectedInstruction.Bytes
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || instruction.BranchTargetRawFileOffset
                    != expectedInstruction.BranchTarget
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)instruction.Bytes.Length;
            bytes += instructionBytes;
            evidenceIndex++;
        }

        return CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Instructions
                .Length == expected.Length
            && nextRawOffset == 0x012C93C0
            && bytes == string.Concat(expected.Select(item => item.Bytes))
            && CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection.Instructions
                .Select(instruction => instruction.RawFileOffset)
                .SequenceEqual(expected.Select(item => item.Raw))
            && CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                .TryGetInstructionEvidence(0x012C93AA, out var branch)
            && branch.BranchTargetRawFileOffset == 0x012C93C0
            && CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                .TryGetInstructionEvidence(0x012C93BE, out var jump)
            && jump.BranchTargetRawFileOffset == 0x012C942F
            && CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                .TryGetBoundedDestinationEvidence(0x012C93AC, out var zeroDestination)
            && zeroDestination.Kind
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationKind
                    .ZeroTestFallthrough
            && zeroDestination.BeforeTargetInstruction
            && CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                .TryGetBoundedDestinationEvidence(0x012C93C0, out var nonzeroDestination)
            && nonzeroDestination.Kind
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationKind
                    .NonzeroTestBranch
            && CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                .TryGetBoundedDestinationEvidence(0x012C942F, out var jumpDestination)
            && jumpDestination.Kind
                == CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleDestinationKind
                    .ChildDirectJump
            && jumpDestination.VirtualAddress == 0x016CA02F
            && CapacityFinalCalleeCommonFreshMaskSelfRingLifecycleProjection
                .RawFileOffsetToVirtualAddress(0x012C93A1) == 0x016C9FA1;
    }
}
