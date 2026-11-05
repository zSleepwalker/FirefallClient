using System;
using System.Buffers.Binary;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeLargeRightFieldTagLifecycleProjection;

internal static class CapacityFinalCalleeLargeRightFieldTagLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed instruction evidence preserves raw 92A4..92C8",
            InstructionEvidenceMatches());

        CapacityFinalCalleeLargeRightFieldTagLifecyclePlan zeroBypass =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0,
                FreshGlobalValue: null,
                LateEntryEAX: null,
                LateEntryEDI: 0x20,
                LateEntryESI: 0x80));
        check("An exact fresh-zero bypass enters 92BD and completes only the common tag stores",
            zeroBypass.ParentRoute
                == CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute
                    .FreshZeroBypass92BD
            && zeroBypass.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome.ZeroTo92BD
            && zeroBypass.Reads.Length == 1
            && zeroBypass.Reads[0] is
            {
                Stage: CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.RightFieldGate,
                Address: 0x214,
                Value: 0,
                ValueCaptured: true
            }
            && zeroBypass.ParentWrites.IsEmpty
            && zeroBypass.ParentBranchReached
            && zeroBypass.ParentBranchTaken == true
            && zeroBypass.ParentBranchSelectedDestinationRawFileOffset
                == Projection.ZeroBypassRawFileOffset
            && zeroBypass.ParentStopAtRawFileOffsetExclusive
                == Projection.ZeroBypassRawFileOffset
            && zeroBypass.ParentLifecycle.ExactCleanCompletion
            && !zeroBypass.ParentLifecycle.UnusedFreshGlobalValueCaptureProvided
            && !zeroBypass.UnusedLateEntryEAXCaptureProvided
            && zeroBypass.ExactCleanParentHandoff
            && zeroBypass.Child92BDEntered
            && zeroBypass.ChildMOVAt92BDAttempted
            && zeroBypass.ChildMOVAt92BDExecuted
            && zeroBypass.ChildEAXBeforeOR == 0x20
            && zeroBypass.ChildEAXAfterOR == 0x21
            && zeroBypass.CommonTail?.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeCommonTailTagWritesWrite(0x012C92C2, 0x84, 0x21),
                new CapacityFinalCalleeCommonTailTagWritesWrite(0x012C92C5, 0xA0, 0x20)
            }) == true
            && zeroBypass.CommonTail?.OrInstructionReached == true
            && zeroBypass.ExactCleanCompletion
            && zeroBypass.Final92C8Entered == false
            && zeroBypass.FinalBoundaryInstructionExecuted == false
            && zeroBypass.StopAtRawFileOffsetExclusive
                == Projection.FinalBoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeLargeRightFieldTagLifecyclePlan notBelow =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 2,
                FreshGlobalValue: 2,
                LateEntryEAX: 0x200,
                LateEntryEDI: 0x40,
                LateEntryESI: 0x100));
        check("The exact not-below route retains parent reads and stores before entering 92BD",
            notBelow.ParentRoute
                == CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute
                    .ThresholdNotBelowPairedStores92BD
            && notBelow.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedNotBelowFallthrough
            && notBelow.Reads.Length == 2
            && notBelow.Reads[0] is
            {
                Stage: CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.RightFieldGate,
                Address: 0x214,
                Value: 2,
                ValueCaptured: true
            }
            && notBelow.Reads[1] is
            {
                Stage: CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.ThresholdGate,
                Address: 0x01EDADD8,
                Value: 2,
                ValueCaptured: true
            }
            && notBelow.ParentWrites.SequenceEqual(new[]
            {
                new CapacityFinalCalleeLargeRightPairedStoresWrite(0x012C92B7, 0x214, 2),
                new CapacityFinalCalleeLargeRightPairedStoresWrite(0x012C92BA, 0x1A, 0x200)
            })
            && notBelow.ParentBranchReached
            && notBelow.ParentBranchTaken == false
            && notBelow.ParentBranchSelectedDestinationRawFileOffset
                == Projection.PairedStoresEntryRawFileOffset
            && notBelow.ParentStopAtRawFileOffsetExclusive
                == Projection.PairedStoresEntryRawFileOffset
            && notBelow.ParentLifecycle.StopAtRawFileOffsetExclusive
                == Projection.ChildEntryRawFileOffset
            && notBelow.ParentLifecycle.PairedStores is
            {
                Outcome: CapacityFinalCalleeLargeRightPairedStoresOutcome.Completed,
                ExactCleanCompletion: true,
                FirstStoreCommitted: true,
                SecondStoreCommitted: true,
                BoundaryInstructionExecuted: false
            }
            && notBelow.ExactCleanParentHandoff
            && notBelow.Child92BDEntered
            && notBelow.ParentFlagsAtChildEntry == notBelow.ParentFlagsAtStop
            && notBelow.FlagsPreservedUntilTagOR
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
            && notBelow.ExactCleanCompletion
            && notBelow.StopAtRawFileOffsetExclusive
                == Projection.FinalBoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeLargeRightFieldTagLifecyclePlan missingParent =
            Projection.Project(new(
                EntryEDX: null,
                FreshEDXPlus14: 0x20,
                FreshGlobalValue: null,
                LateEntryEAX: 0x300,
                LateEntryEDI: 0x40,
                LateEntryESI: 0x80));
        check("Missing parent prerequisites stop before 92BD and expose every unused late capture",
            missingParent.ParentRoute
                == CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute.None
            && missingParent.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .MissingEntryEDXForFreshRead
            && missingParent.Reads.IsEmpty
            && !missingParent.Child92BDEntered
            && missingParent.UnusedLateEntryEAXCaptureProvided
            && missingParent.UnusedLateEntryEDICaptureProvided
            && missingParent.UnusedLateEntryESICaptureProvided
            && missingParent.Outcome
                == CapacityFinalCalleeLargeRightFieldTagLifecycleOutcome
                    .ParentStoppedWithUnusedChildCaptures
            && missingParent.OrderedWrites.IsEmpty
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeRightFieldTagLifecyclePlan below =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 1,
                FreshGlobalValue: 2,
                LateEntryEAX: 0x300,
                LateEntryEDI: 0x40,
                LateEntryESI: 0x80));
        check("Unsigned-below branches to 8E76 without entering 92BD",
            below.ParentRoute == CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute.None
            && below.ParentBranchReached
            && below.ParentBranchTaken == true
            && below.ParentBranchSelectedDestinationRawFileOffset
                == Projection.UnsignedBelowDestinationRawFileOffset
            && below.ParentStopAtRawFileOffsetExclusive
                == Projection.UnsignedBelowDestinationRawFileOffset
            && !below.Child92BDEntered
            && below.UnusedLateEntryEAXCaptureProvided
            && below.UnusedLateEntryEDICaptureProvided
            && below.UnusedLateEntryESICaptureProvided
            && below.OrderedWrites.IsEmpty
            && !below.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightFieldTagLifecyclePlan dirtyZeroBypass =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0,
                FreshGlobalValue: 0x10,
                LateEntryEAX: null,
                LateEntryEDI: 0x40,
                LateEntryESI: 0x80));
        check("An unused fresh global makes the zero bypass ineligible for the child",
            dirtyZeroBypass.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .ZeroTo92BDWithUnusedFreshGlobalValue
            && dirtyZeroBypass.ParentBranchSelectedDestinationRawFileOffset
                == Projection.ZeroBypassRawFileOffset
            && !dirtyZeroBypass.ExactCleanParentHandoff
            && dirtyZeroBypass.ParentRoute
                == CapacityFinalCalleeLargeRightFieldTagLifecycleParentRoute.None
            && !dirtyZeroBypass.Child92BDEntered
            && dirtyZeroBypass.UnusedLateEntryEDICaptureProvided
            && dirtyZeroBypass.UnusedLateEntryESICaptureProvided
            && dirtyZeroBypass.OrderedWrites.IsEmpty);

        CapacityFinalCalleeLargeRightFieldTagLifecyclePlan missingParentEAX =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 2,
                FreshGlobalValue: 2,
                LateEntryEAX: null,
                LateEntryEDI: 0x40,
                LateEntryESI: 0x80));
        check("A missing parent EAX reaches its first-store attempt but cannot reach the tag child",
            missingParentEAX.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedNotBelowFallthrough
            && missingParentEAX.ParentLifecycle.PairedStores?.FirstStoreReached == true
            && missingParentEAX.ParentLifecycle.PairedStores?.FirstStoreAddressAttempted == true
            && missingParentEAX.ParentLifecycle.PairedStores?.FirstStoreValue == 2
            && !missingParentEAX.ParentLifecycle.FirstStoreCommitted
            && !missingParentEAX.Child92BDEntered
            && missingParentEAX.UnusedLateEntryEDICaptureProvided
            && missingParentEAX.UnusedLateEntryESICaptureProvided
            && missingParentEAX.StopAtRawFileOffsetExclusive
                == Projection.PairedStoresEntryRawFileOffset
            && !missingParentEAX.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightFieldTagLifecyclePlan missingChildEDI =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0,
                FreshGlobalValue: null,
                LateEntryEAX: null,
                LateEntryEDI: null,
                LateEntryESI: 0x80));
        check("The child MOV is attempted at 92BD and missing EDI leaves parent flags unchanged",
            missingChildEDI.Child92BDEntered
            && missingChildEDI.ChildMOVAt92BDAttempted
            && !missingChildEDI.ChildMOVAt92BDExecuted
            && missingChildEDI.CommonTail?.Outcome
                == CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI
            && !missingChildEDI.ChildORReached
            && missingChildEDI.ParentFlagsAtChildEntry is not null
            && missingChildEDI.FlagsAtStop
                == ConvertFlags(missingChildEDI.ParentFlagsAtStop)
            && missingChildEDI.FlagsPreservedUntilTagOR
            && missingChildEDI.UnusedLateEntryESICaptureProvided
            && !missingChildEDI.UnusedLateEntryEDICaptureProvided
            && missingChildEDI.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonTailTagWritesProjection.LoadEAXRawFileOffset
            && !missingChildEDI.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightFieldTagLifecyclePlan missingChildESI =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0,
                FreshGlobalValue: null,
                LateEntryEAX: null,
                LateEntryEDI: 0x20,
                LateEntryESI: null));
        check("The known OR executes before missing ESI stops at the first tag store",
            missingChildESI.ChildMOVAt92BDExecuted
            && missingChildESI.ChildEAXBeforeOR == 0x20
            && missingChildESI.ChildEAXAfterOR == 0x21
            && missingChildESI.ChildORReached
            && missingChildESI.CommonTail?.Outcome
                == CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryESI
            && missingChildESI.CommonTail?.FirstStoreReached == false
            && missingChildESI.FirstTagStoreAttempted
            && !missingChildESI.FirstTagStoreCommitted
            && missingChildESI.TagORFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingChildESI.FlagsAtStop == missingChildESI.TagORFlags
            && missingChildESI.FlagsPreservedUntilTagOR
            && missingChildESI.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonTailTagWritesProjection.FirstStoreRawFileOffset
            && !missingChildESI.UnusedLateEntryEDICaptureProvided
            && !missingChildESI.UnusedLateEntryESICaptureProvided
            && !missingChildESI.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightFieldTagLifecyclePlan aliasesAndWraps =
            Projection.Project(new(
                EntryEDX: 0xFFFFFFF0,
                FreshEDXPlus14: 0,
                FreshGlobalValue: null,
                LateEntryEAX: null,
                LateEntryEDI: 4,
                LateEntryESI: 0));
        check("Fresh parent reads precede ordered wrapping tag writes even when all addresses alias",
            aliasesAndWraps.Reads.Length == 1
            && aliasesAndWraps.Reads[0] is
            {
                Address: 4,
                Value: 0,
                ValueCaptured: true
            }
            && aliasesAndWraps.TagWrites.SequenceEqual(new[]
            {
                new CapacityFinalCalleeCommonTailTagWritesWrite(0x012C92C2, 4, 5),
                new CapacityFinalCalleeCommonTailTagWritesWrite(0x012C92C5, 4, 4)
            })
            && aliasesAndWraps.TagWrites[0].Address == aliasesAndWraps.TagWrites[1].Address
            && aliasesAndWraps.TagWrites[0].Value != aliasesAndWraps.TagWrites[1].Value
            && aliasesAndWraps.OrderedWrites.Select(write => write.RawFileOffset).SequenceEqual(new[]
            {
                0x012C92C2u, 0x012C92C5u
            })
            && aliasesAndWraps.ExactCleanCompletion
            && aliasesAndWraps.StopAtRawFileOffsetExclusive
                == Projection.FinalBoundaryRawFileOffsetExclusive);

        CapacityFinalCalleeLargeRightFieldTagLifecyclePlan wrappedTagAddresses =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0,
                FreshGlobalValue: null,
                LateEntryEAX: null,
                LateEntryEDI: 4,
                LateEntryESI: 0xFFFFFFFC));
        check("Tag effective-address addition wraps independently while values remain captured",
            wrappedTagAddresses.TagWrites.SequenceEqual(new[]
            {
                new CapacityFinalCalleeCommonTailTagWritesWrite(0x012C92C2, 0, 5),
                new CapacityFinalCalleeCommonTailTagWritesWrite(0x012C92C5, 0, 4)
            })
            && wrappedTagAddresses.TagWrites[0].Address
                == wrappedTagAddresses.TagWrites[1].Address
            && wrappedTagAddresses.ExactCleanCompletion);
    }

    private static CapacityFinalCalleeCommonTailTagWritesFlags? ConvertFlags(
        CapacityFinalCalleeLargeRightFieldThresholdLifecycleFlags? flags) =>
        flags is { } value
            ? new(value.Carry, value.Parity, value.AuxiliaryCarry,
                value.Zero, value.Sign, value.Overflow)
            : null;

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
            (0x012C92C5, "89 3C 37", null)
        };
        uint nextRawOffset = Projection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeRightFieldTagLifecycleInstructionEvidence instruction
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
            && Projection.ChildEntryRawFileOffset == 0x012C92BD
            && Projection.FinalBoundaryRawFileOffsetExclusive == 0x012C92C8
            && nextRawOffset == Projection.FinalBoundaryRawFileOffsetExclusive
            && bytes == Normalize(Projection.CapturedWindowBytes)
            && Projection.Instructions.Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
