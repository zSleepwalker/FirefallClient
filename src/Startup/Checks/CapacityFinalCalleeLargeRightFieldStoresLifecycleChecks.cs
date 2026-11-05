using System;
using System.Linq;
using System.Buffers.Binary;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeLargeRightFieldStoresLifecycleProjection;

internal static class CapacityFinalCalleeLargeRightFieldStoresLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed instruction evidence preserves the exact 92A4..92BD window",
            InstructionEvidenceMatches());

        CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan freshZero =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0,
                FreshGlobalValue: 0x10,
                LateEntryEAX: 0x300));
        check("A fresh zero stops at 92BD without entering the child or using late EAX",
            freshZero.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .ZeroTo92BDWithUnusedFreshGlobalValue
            && freshZero.ParentBranchReached
            && freshZero.ParentBranchTaken == true
            && freshZero.ParentBranchSelectedDestinationRawFileOffset == 0x012C92BD
            && freshZero.ParentStopAtRawFileOffsetExclusive == 0x012C92BD
            && freshZero.PairedStores is null
            && !freshZero.Child92B7Entered
            && !freshZero.Final92BDEntered
            && freshZero.UnusedLateEntryEAXCaptureProvided
            && freshZero.Outcome
                == CapacityFinalCalleeLargeRightFieldStoresLifecycleOutcome
                    .ParentStoppedWithUnusedLateEntryEAX
            && !freshZero.ExactCleanParentExitAdmitted
            && !freshZero.ExactCleanCompletion
            && freshZero.Writes.IsEmpty
            && freshZero.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan freshZeroClean =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0,
                FreshGlobalValue: null,
                LateEntryEAX: null));
        check("The exact zero bypass remains a clean parent exit when late EAX is absent",
            freshZeroClean.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome.ZeroTo92BD
            && freshZeroClean.ParentBranchSelectedDestinationRawFileOffset == 0x012C92BD
            && freshZeroClean.ExactCleanParentExitAdmitted
            && freshZeroClean.ExactCleanCompletion
            && !freshZeroClean.Child92B7Entered
            && freshZeroClean.Writes.IsEmpty
            && freshZeroClean.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan missingEntry =
            Projection.Project(new(
                EntryEDX: null,
                FreshEDXPlus14: 0x20,
                FreshGlobalValue: 0x10,
                LateEntryEAX: 0x300));
        CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan missingRead =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: null,
                FreshGlobalValue: null,
                LateEntryEAX: 0x300));
        check("Missing parent reads never admit the child and leave late EAX unused",
            missingEntry.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .MissingParentPrerequisiteWithUnusedFreshGlobalValue
            && missingEntry.Reads.IsEmpty
            && !missingEntry.ParentBranchReached
            && missingEntry.PairedStores is null
            && missingEntry.UnusedLateEntryEAXCaptureProvided
            && !missingEntry.ExactCleanCompletion
            && missingEntry.StopAtRawFileOffsetExclusive == 0x012C92A4
            && missingRead.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .MissingFreshEDXPlus14
            && missingRead.Reads.Length == 1
            && missingRead.Reads[0] is
            {
                Stage: CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.RightFieldGate,
                Address: 0x214,
                AddressCaptured: true,
                ValueCaptured: false
            }
            && missingRead.PairedStores is null
            && missingRead.UnusedLateEntryEAXCaptureProvided
            && missingRead.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan missingGlobal =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0x80000001,
                FreshGlobalValue: null,
                LateEntryEAX: 0x300));
        check("A missing threshold read preserves parent flags and leaves late EAX unused",
            missingGlobal.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .MissingFreshGlobalValue
            && missingGlobal.ParentBranchReached
            && missingGlobal.ParentBranchTaken == false
            && missingGlobal.ParentBranchSelectedDestinationRawFileOffset == 0x012C92AB
            && missingGlobal.ParentStopAtRawFileOffsetExclusive == 0x012C92AB
            && missingGlobal.Reads.Length == 2
            && missingGlobal.Reads[1] is
            {
                Stage: CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.ThresholdGate,
                Address: 0x01EDADD8,
                ValueCaptured: false
            }
            && missingGlobal.ParentFlagsAtStop == missingGlobal.ParentLifecycle.ParentTestFlags
            && missingGlobal.PairedStores is null
            && missingGlobal.UnusedLateEntryEAXCaptureProvided
            && !missingGlobal.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan unsignedBelow =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 1,
                FreshGlobalValue: 2,
                LateEntryEAX: 0x300));
        check("Unsigned-below branches to 8E76 without entering 92B7",
            unsignedBelow.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedBelowBranchTarget
            && unsignedBelow.ParentBranchReached
            && unsignedBelow.ParentBranchTaken == true
            && unsignedBelow.ParentBranchSelectedDestinationRawFileOffset == 0x012C8E76
            && unsignedBelow.ParentStopAtRawFileOffsetExclusive == 0x012C8E76
            && unsignedBelow.ParentCompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unsignedBelow.PairedStores is null
            && !unsignedBelow.Child92B7Entered
            && unsignedBelow.UnusedLateEntryEAXCaptureProvided
            && !unsignedBelow.ExactCleanCompletion
            && unsignedBelow.Writes.IsEmpty
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan equality =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 2,
                FreshGlobalValue: 2,
                LateEntryEAX: 0x100));
        check("Threshold equality falls through, derives defined CMP flags, and commits both stores",
            equality.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedNotBelowFallthrough
            && equality.ParentBranchReached
            && equality.ParentBranchTaken == false
            && equality.ParentBranchSelectedDestinationRawFileOffset == 0x012C92B7
            && equality.ParentStopAtRawFileOffsetExclusive == 0x012C92B7
            && equality.Child92B7Entered
            && equality.LateEntryEAXCaptureReached
            && equality.ParentCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equality.ChildIncomingEFlagsDefinedBits == 0x46
            && equality.EFlagsAtStopDefinedBits == 0x46
            && equality.StoresPreserveParentFlags
            && equality.FlagsAtStop == equality.ParentCompareFlags
            && equality.PairedStores?.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeLargeRightPairedStoresWrite(0x012C92B7, 0x114, 2),
                new CapacityFinalCalleeLargeRightPairedStoresWrite(0x012C92BA, 0x1A, 0x100)
            }) == true
            && equality.FirstStoreCommitted
            && equality.SecondStoreCommitted
            && equality.Writes.Length == 2
            && !equality.Final92BDEntered
            && equality.ExactCleanParentExitAdmitted
            && equality.ExactCleanCompletion
            && equality.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan unsignedAbove =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 3,
                FreshGlobalValue: 2,
                LateEntryEAX: 0x400));
        check("Unsigned-above follows the same child path and preserves the exact CMP result",
            unsignedAbove.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedNotBelowFallthrough
            && unsignedAbove.ParentBranchSelectedDestinationRawFileOffset == 0x012C92B7
            && unsignedAbove.PairedStores?.FirstStoreValue == 3
            && unsignedAbove.PairedStores?.SecondStoreValue == 0x400
            && unsignedAbove.ParentCompareFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && unsignedAbove.ChildIncomingEFlagsDefinedBits == 0x2
            && unsignedAbove.EFlagsAtStopDefinedBits == 0x2
            && unsignedAbove.StoresPreserveParentFlags
            && unsignedAbove.Writes.Length == 2
            && unsignedAbove.ExactCleanCompletion
            && unsignedAbove.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan missingLateEAX =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0x1234,
                FreshGlobalValue: 0x1234,
                LateEntryEAX: null));
        check("A missing late EAX reaches the first child store while retaining its known ECX value",
            missingLateEAX.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .UnsignedNotBelowFallthrough
            && missingLateEAX.Child92B7Entered
            && missingLateEAX.FirstStoreAttempted
            && missingLateEAX.PairedStores?.Outcome
                == CapacityFinalCalleeLargeRightPairedStoresOutcome.MissingFirstStoreAddress
            && missingLateEAX.PairedStores?.FirstStoreAddressAttempted == true
            && missingLateEAX.PairedStores?.FirstStoreValueAttempted == true
            && missingLateEAX.PairedStores?.FirstStoreAddress is null
            && missingLateEAX.PairedStores?.FirstStoreValue == 0x1234
            && !missingLateEAX.FirstStoreCommitted
            && !missingLateEAX.SecondStoreAttempted
            && missingLateEAX.Writes.IsEmpty
            && missingLateEAX.ParentStopAtRawFileOffsetExclusive == 0x012C92B7
            && missingLateEAX.StopAtRawFileOffsetExclusive == 0x012C92B7
            && !missingLateEAX.ExactCleanCompletion);

        const uint aliasedReadAddress = 0x01EDADD8;
        uint aliasedEntryEDX = unchecked(aliasedReadAddress - 0x14);
        CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan orderedAliases =
            Projection.Project(new(
                EntryEDX: aliasedEntryEDX,
                FreshEDXPlus14: 0x26,
                FreshGlobalValue: 0x25,
                LateEntryEAX: 0x2A));
        check("Aliased source reads remain independent and ordered before aliasing stores",
            orderedAliases.Reads.Length == 2
            && orderedAliases.Reads[0] is
            {
                Stage: CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.RightFieldGate,
                Address: aliasedReadAddress,
                Value: 0x26,
                ValueCaptured: true
            }
            && orderedAliases.Reads[1] is
            {
                Stage: CapacityFinalCalleeLargeRightFieldThresholdLifecycleStage.ThresholdGate,
                Address: aliasedReadAddress,
                Value: 0x25,
                ValueCaptured: true
            }
            && orderedAliases.ParentBranchSelectedDestinationRawFileOffset == 0x012C92B7
            && orderedAliases.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeLargeRightPairedStoresWrite(0x012C92B7, 0x3E, 0x26),
                new CapacityFinalCalleeLargeRightPairedStoresWrite(0x012C92BA, 0x3E, 0x2A)
            })
            && orderedAliases.Writes[0].Address == orderedAliases.Writes[1].Address
            && orderedAliases.Writes[0].Value != orderedAliases.Writes[1].Value
            && orderedAliases.ExactCleanCompletion
            && orderedAliases.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeRightFieldStoresLifecyclePlan wrapped =
            Projection.Project(new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0xFFFFFFF0,
                FreshGlobalValue: 0,
                LateEntryEAX: 0xFFFFFFEC));
        check("Both child effective addresses wrap as unsigned DWORDs without changing values",
            wrapped.Child92B7Entered
            && wrapped.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeLargeRightPairedStoresWrite(
                    0x012C92B7, 0, 0xFFFFFFF0),
                new CapacityFinalCalleeLargeRightPairedStoresWrite(
                    0x012C92BA, 8, 0xFFFFFFEC)
            })
            && wrapped.FirstStoreCommitted
            && wrapped.SecondStoreCommitted
            && wrapped.ExactCleanCompletion
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C92BD);
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
            (0x012C92BA, "89 41 18", null)
        };
        uint nextRawOffset = Projection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeRightFieldStoresLifecycleInstructionEvidence instruction
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
            && Projection.EntryVirtualAddress == 0x016C9EA4
            && Projection.ChildEntryRawFileOffset == 0x012C92B7
            && Projection.FinalBoundaryRawFileOffsetExclusive == 0x012C92BD
            && Projection.DefinedEFlagsBitsMask == 0x000008D7
            && nextRawOffset == Projection.FinalBoundaryRawFileOffsetExclusive
            && bytes == Normalize(Projection.CapturedWindowBytes)
            && Projection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
