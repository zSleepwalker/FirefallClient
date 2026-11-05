using System;
using System.Buffers.Binary;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeLargeRightTagRootLifecycleProjection;

internal static class CapacityFinalCalleeLargeRightTagRootLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed window preserves the native 92C8 CMP/JNZ evidence",
            InstructionEvidenceMatches());

        CapacityFinalCalleeLargeRightTagRootLifecyclePlan zeroBypass =
            Projection.Project(new(
                Parent: new(
                    EntryEDX: 0x200,
                    FreshEDXPlus14: 0,
                    FreshGlobalValue: null,
                    LateEntryEAX: null,
                    LateEntryEDI: 0x20,
                    LateEntryESI: 0x80),
                FreshRootGlobalDWORD: 0x80));
        check("The exact zero-bypass parent hands its consumed ESI to the equal root gate",
            zeroBypass.ParentLifecycle.ParentRoute
                == CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute
                    .FreshZeroBypass92BD
            && zeroBypass.ExactCleanParentHandoff
            && zeroBypass.RootGateEntered
            && zeroBypass.RootEntryESICaptured
            && zeroBypass.RootEntryESI == zeroBypass.Capture.Parent.LateEntryESI
            && zeroBypass.RootEntryESI == 0x80
            && zeroBypass.RootGlobalReadAttempted
            && zeroBypass.RootGlobalReadAddress == 0x01EDADDC
            && zeroBypass.RootGlobalCaptured
            && zeroBypass.FreshRootGlobalDWORD == 0x80
            && zeroBypass.RootCompareResult == 0
            && zeroBypass.RootCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroBypass.Outcome
                == CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                    .EqualToFallthrough92D0
            && zeroBypass.RootJnzReached
            && zeroBypass.RootJnzTaken == false
            && zeroBypass.RootJnzSelectedDestinationRawFileOffset
                == Projection.FallthroughRawFileOffset
            && zeroBypass.Reads.Select(read => read.RawFileOffset).SequenceEqual(new[]
            {
                0x012C92A4u, 0x012C92C8u
            })
            && zeroBypass.OrderedWrites.Select(write => write.RawFileOffset)
                .SequenceEqual(new[] { 0x012C92C2u, 0x012C92C5u })
            && zeroBypass.ExactCleanCompletion
            && zeroBypass.StopAtRawFileOffsetExclusive
                == Projection.FallthroughRawFileOffset
            && !zeroBypass.RootBranchTargetEntered
            && !zeroBypass.RootFallthroughEntered);

        CapacityFinalCalleeLargeRightTagRootLifecyclePlan notBelow =
            Projection.Project(new(
                Parent: new(
                    EntryEDX: 0x200,
                    FreshEDXPlus14: 2,
                    FreshGlobalValue: 2,
                    LateEntryEAX: 0x200,
                    LateEntryEDI: 0x40,
                    LateEntryESI: 0x100),
                FreshRootGlobalDWORD: 0x101));
        check("The exact not-below route retains paired stores before the unequal root branch",
            notBelow.ParentLifecycle.ParentRoute
                == CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute
                    .ThresholdNotBelowPairedStores92BD
            && notBelow.ParentLifecycle.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedNotBelowFallthrough
            && notBelow.ExactCleanParentHandoff
            && notBelow.Reads.Select(read => read.RawFileOffset).SequenceEqual(new[]
            {
                0x012C92A4u, 0x012C92ABu, 0x012C92C8u
            })
            && notBelow.Reads.Select(read => read.Address).SequenceEqual(new uint?[]
            {
                0x214u, 0x01EDADD8u, 0x01EDADDCu
            })
            && notBelow.OrderedWrites.Select(write => write.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.ParentFirstStore,
                CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.ParentSecondStore,
                CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.TagFirstStore,
                CapacityFinalCalleeLargeRightFieldTagLifecycleWriteKind.TagSecondStore
            })
            && notBelow.OrderedWrites.Select(write => write.RawFileOffset).SequenceEqual(new[]
            {
                0x012C92B7u, 0x012C92BAu, 0x012C92C2u, 0x012C92C5u
            })
            && notBelow.RootEntryESI == 0x100
            && notBelow.FreshRootGlobalDWORD == 0x101
            && notBelow.RootCompareResult == 0xFFFFFFFF
            && notBelow.RootCompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && notBelow.Outcome
                == CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                    .UnequalToBranchTarget92EC
            && notBelow.RootJnzTaken == true
            && notBelow.RootJnzSelectedDestinationRawFileOffset
                == Projection.BranchTargetRawFileOffset
            && notBelow.ExactCleanCompletion
            && notBelow.StopAtRawFileOffsetExclusive
                == Projection.BranchTargetRawFileOffset
            && !notBelow.RootBranchTargetEntered
            && !notBelow.RootFallthroughEntered
            && !notBelow.OpaqueEntryReached);

        CapacityFinalCalleeLargeRightTagRootLifecyclePlan missingRootGlobal =
            Projection.Project(new(
                Parent: new(
                    EntryEDX: 0x200,
                    FreshEDXPlus14: 0,
                    FreshGlobalValue: null,
                    LateEntryEAX: null,
                    LateEntryEDI: 0x20,
                    LateEntryESI: 0x80),
                FreshRootGlobalDWORD: null));
        check("A missing fresh root DWORD still attempts the read and preserves all prior OR flags",
            missingRootGlobal.RootGateEntered
            && missingRootGlobal.RootGlobalReadAttempted
            && missingRootGlobal.RootGlobalReadAddress == 0x01EDADDC
            && missingRootGlobal.Reads[^1] is
            {
                Stage: CapacityFinalCalleeLargeRightTagRootLifecycleReadStage.RootGate,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && !missingRootGlobal.RootGlobalCaptured
            && !missingRootGlobal.RootCompareReached
            && !missingRootGlobal.RootJnzReached
            && missingRootGlobal.Outcome
                == CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                    .MissingFreshRootGlobalDWORD
            && missingRootGlobal.TagORFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingRootGlobal.FlagsAtStop == missingRootGlobal.TagORFlags
            && missingRootGlobal.FlagsPreservedUntilRootCompare
            && !missingRootGlobal.ExactCleanCompletion
            && missingRootGlobal.StopAtRawFileOffsetExclusive
                == Projection.RootEntryRawFileOffset);

        CapacityFinalCalleeLargeRightTagRootLifecyclePlan missingParent =
            Projection.Project(new(
                Parent: new(
                    EntryEDX: null,
                    FreshEDXPlus14: 0,
                    FreshGlobalValue: null,
                    LateEntryEAX: 0x200,
                    LateEntryEDI: 0x20,
                    LateEntryESI: 0x80),
                FreshRootGlobalDWORD: 0x1234));
        check("A missing parent read prerequisite stops before 92C8 and marks the root capture unused",
            missingParent.ParentLifecycle.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .MissingEntryEDXForFreshRead
            && missingParent.Reads.IsEmpty
            && !missingParent.RootGateEntered
            && !missingParent.RootGlobalReadAttempted
            && missingParent.UnusedFreshRootGlobalDWORDCaptureProvided
            && missingParent.Outcome
                == CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                    .ParentStoppedWithUnusedFreshRootGlobalDWORD
            && missingParent.StopAtRawFileOffsetExclusive
                == Projection.EntryRawFileOffset
            && !missingParent.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightTagRootLifecyclePlan missingParentESI =
            Projection.Project(new(
                Parent: new(
                    EntryEDX: 0x200,
                    FreshEDXPlus14: 0,
                    FreshGlobalValue: null,
                    LateEntryEAX: null,
                    LateEntryEDI: 0x20,
                    LateEntryESI: null),
                FreshRootGlobalDWORD: 0));
        check("A missing consumed parent ESI stops at the first tag store without fabricating a root operand",
            missingParentESI.ParentLifecycle.ParentRoute
                == CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute
                    .FreshZeroBypass92BD
            && missingParentESI.ParentLifecycle.Outcome
                == CapacityFinalCalleeLargeRightFieldTagLifecycleOutcome
                    .ChildStoppedAtFirstTagStore
            && missingParentESI.ParentLifecycle.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonTailTagWritesProjection
                    .FirstStoreRawFileOffset
            && !missingParentESI.ParentLifecycle.LateEntryESICaptureReached
            && !missingParentESI.RootGateEntered
            && missingParentESI.RootEntryESI is null
            && !missingParentESI.RootGlobalReadAttempted
            && missingParentESI.UnusedFreshRootGlobalDWORDCaptureProvided
            && missingParentESI.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonTailTagWritesProjection
                    .FirstStoreRawFileOffset);

        CapacityFinalCalleeLargeRightTagRootLifecyclePlan dirtyParent =
            Projection.Project(new(
                Parent: new(
                    EntryEDX: 0x200,
                    FreshEDXPlus14: 0,
                    FreshGlobalValue: 0x10,
                    LateEntryEAX: null,
                    LateEntryEDI: 0x20,
                    LateEntryESI: 0x80),
                FreshRootGlobalDWORD: 0x80));
        check("A dirty zero-bypass parent cannot admit the root gate or consume its capture",
            dirtyParent.ParentLifecycle.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .ZeroTo92BDWithUnusedFreshGlobalValue
            && dirtyParent.ParentLifecycle.ParentRoute
                == CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute.None
            && !dirtyParent.RootGateEntered
            && dirtyParent.UnusedFreshRootGlobalDWORDCaptureProvided
            && dirtyParent.Reads.Select(read => read.RawFileOffset)
                .SequenceEqual(new[] { 0x012C92A4u })
            && dirtyParent.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeLargeRightFieldTagLifecycleProjection.ChildEntryRawFileOffset
            && dirtyParent.FlagsAtStop == dirtyParent.ParentLifecycle.FlagsAtStop
            && !dirtyParent.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightTagRootLifecyclePlan aliasedFreshRoot =
            Projection.Project(new(
                Parent: new(
                    EntryEDX: 0x200,
                    FreshEDXPlus14: 2,
                    FreshGlobalValue: 2,
                    LateEntryEAX: 0x01EDADC8,
                    LateEntryEDI: 0x20,
                    LateEntryESI: 0x01EDADD8),
                FreshRootGlobalDWORD: 0x11223344));
        check("Fresh root data stays independent when both parent and tag stores alias its address",
            aliasedFreshRoot.Reads.Select(read => read.Address).SequenceEqual(new uint?[]
            {
                0x214u, 0x01EDADD8u, 0x01EDADDCu
            })
            && aliasedFreshRoot.OrderedWrites.Select(write =>
                    (write.RawFileOffset, write.Address, write.Value))
                .SequenceEqual(new[]
                {
                    (0x012C92B7u, 0x01EDADDCu, 2u),
                    (0x012C92BAu, 0x1Au, 0x01EDADC8u),
                    (0x012C92C2u, 0x01EDADDCu, 0x21u),
                    (0x012C92C5u, 0x01EDADF8u, 0x20u)
                })
            && aliasedFreshRoot.FreshRootGlobalDWORD == 0x11223344
            && aliasedFreshRoot.ParentLifecycle.Capture.FreshGlobalValue == 2
            && aliasedFreshRoot.RootGate?.Capture.EntryESI
                == aliasedFreshRoot.Capture.Parent.LateEntryESI
            && aliasedFreshRoot.RootCompareResult
                == unchecked(0x01EDADD8u - 0x11223344u)
            && aliasedFreshRoot.RootJnzTaken == true
            && aliasedFreshRoot.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightTagRootLifecyclePlan wrappedAddresses =
            Projection.Project(new(
                Parent: new(
                    EntryEDX: 0xFFFFFFF0,
                    FreshEDXPlus14: 0,
                    FreshGlobalValue: null,
                    LateEntryEAX: null,
                    LateEntryEDI: 4,
                    LateEntryESI: 0xFFFFFFFC),
                FreshRootGlobalDWORD: 0xFFFFFFFF));
        check("Parent reads and ordered tag writes retain unchecked 32-bit address wrapping",
            wrappedAddresses.Reads.Select(read => read.Address).SequenceEqual(new uint?[]
            {
                4u, 0x01EDADDCu
            })
            && wrappedAddresses.OrderedWrites.Select(write =>
                    (write.RawFileOffset, write.Address, write.Value))
                .SequenceEqual(new[]
                {
                    (0x012C92C2u, 0u, 5u),
                    (0x012C92C5u, 0u, 4u)
                })
            && wrappedAddresses.RootEntryESI == 0xFFFFFFFC
            && wrappedAddresses.RootCompareResult == 0xFFFFFFFD
            && wrappedAddresses.RootJnzSelectedDestinationRawFileOffset
                == Projection.BranchTargetRawFileOffset
            && wrappedAddresses.ExactCleanCompletion);

        check("The root gate never enters either branch destination",
            zeroBypass.ExactCleanCompletion
            && notBelow.ExactCleanCompletion
            && aliasedFreshRoot.ExactCleanCompletion
            && wrappedAddresses.ExactCleanCompletion
            && zeroBypass.StopAtRawFileOffsetExclusive
                == Projection.FallthroughRawFileOffset
            && notBelow.StopAtRawFileOffsetExclusive
                == Projection.BranchTargetRawFileOffset
            && !zeroBypass.RootBranchTargetEntered
            && !zeroBypass.RootFallthroughEntered
            && !notBelow.RootBranchTargetEntered
            && !notBelow.RootFallthroughEntered);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes, uint? BranchTargetRaw)[]
        {
            (0x012C92A4, "8B 4A 14", null),
            (0x012C92A7, "85 C9", null),
            (0x012C92A9, "74 12", 0x012C92BD),
            (0x012C92AB, "3B 0D D8 AD ED 01", null),
            (0x012C92B1, "0F 82 BF FB FF FF", 0x012C8E76),
            (0x012C92B7, "89 48 14", null),
            (0x012C92BA, "89 41 18", null),
            (0x012C92BD, "8B C7", null),
            (0x012C92BF, "83 C8 01", null),
            (0x012C92C2, "89 46 04", null),
            (0x012C92C5, "89 3C 37", null),
            (0x012C92C8, "3B 35 DC AD ED 01", null),
            (0x012C92CE, "75 1C", 0x012C92EC)
        };
        uint nextRawOffset = Projection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeRightTagRootLifecycleInstructionEvidence instruction
            in Projection.Instructions)
        {
            byte[] instructionBytes = Convert.FromHexString(Normalize(instruction.Bytes));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != Projection.RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != Projection.RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            if (instruction.BranchTargetRawFileOffset is uint branchTarget)
            {
                int displacement = instructionBytes.Length == 2
                    ? unchecked((sbyte)instructionBytes[1])
                    : BinaryPrimitives.ReadInt32LittleEndian(
                        instructionBytes.AsSpan(instructionBytes.Length - 4, 4));
                if ((long)instruction.RawFileOffset + instructionBytes.Length + displacement
                    != branchTarget)
                {
                    return false;
                }
            }

            nextRawOffset += (uint)instructionBytes.Length;
            bytes += Convert.ToHexString(instructionBytes);
        }

        return Projection.EntryRawFileOffset == 0x012C92A4
            && Projection.RootEntryRawFileOffset == 0x012C92C8
            && Projection.ParentBoundaryRawFileOffsetExclusive == 0x012C92C8
            && Projection.BoundaryRawFileOffsetExclusive == 0x012C92D0
            && Projection.FreshRootGlobalDWORDVirtualAddress == 0x01EDADDC
            && Projection.BranchTargetRawFileOffset == 0x012C92EC
            && Projection.FallthroughRawFileOffset == 0x012C92D0
            && Projection.RawFileOffsetToVirtualAddress(0x012C92C8) == 0x016C9EC8
            && Normalize(Projection.CapturedWindowBytes)
                == "8B4A1485C974123B0DD8ADED010F82BFFBFFFF8948148941188BC783C801894604893C373B35DCADED01751C"
            && nextRawOffset == Projection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(Projection.CapturedWindowBytes)
            && Projection.Instructions.Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
