using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeRejectFieldLifecycleProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window and exact bounded parent and child edges match",
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection
                .RejectGateEntryRawFileOffset == 0x012C9263
            && CapacityFinalCalleeLargeRejectFieldLifecycleProjection
                .FieldGateEntryRawFileOffset == 0x012C926B
            && CapacityFinalCalleeLargeRejectFieldLifecycleProjection
                .ParentBelowDestinationRawFileOffset == 0x012C8E76
            && CapacityFinalCalleeLargeRejectFieldLifecycleProjection
                .ChildEqualDestinationRawFileOffset == 0x012C9270
            && CapacityFinalCalleeLargeRejectFieldLifecycleProjection
                .ChildUnequalDestinationRawFileOffset == 0x012C9275
            && Normalize(CapacityFinalCalleeLargeRejectFieldLifecycleProjection
                    .CapturedWindowBytes) == "3BCB0F820BFCFFFF3951107505"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeRejectFieldLifecyclePlan missingParentECX =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: null, EntryEBX: 1),
                ChildEDX: 0,
                ChildFreshECXPlus10: 0));
        check("Missing parent ECX prevents child derivation and marks both late captures unused",
            missingParentECX.Outcome
                == CapacityFinalCalleeLargeRejectFieldLifecycleOutcome.ParentGateIncomplete
            && missingParentECX.ParentGate.Outcome
                == CapacityFinalCalleeLargeIndexedRejectGateOutcome.MissingEntryECX
            && !missingParentECX.FieldGateReached
            && missingParentECX.ChildECXSource
                == CapacityFinalCalleeLargeRejectFieldLifecycleChildECXSource.None
            && missingParentECX.ChildEntryECX is null
            && missingParentECX.UnreachedChildEDXCaptureProvided
            && missingParentECX.UnreachedChildFreshECXPlus10CaptureProvided
            && missingParentECX.ReachedInstructions.IsEmpty
            && missingParentECX.OrderedReads.IsEmpty
            && missingParentECX.FlagsAtStop is null
            && !missingParentECX.ExactCleanExitAdmitted
            && missingParentECX.StopAtRawFileOffsetExclusive == 0x012C9263);

        CapacityFinalCalleeLargeRejectFieldLifecyclePlan missingParentEBX =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: 0x2000, EntryEBX: null),
                ChildEDX: null,
                ChildFreshECXPlus10: null));
        CapacityFinalCalleeLargeRejectFieldLifecyclePlan missingBothParentOperands =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: null, EntryEBX: null),
                ChildEDX: 0,
                ChildFreshECXPlus10: null));
        check("Either or both missing parent operands stop before child capture and retain no flags",
            missingParentEBX.ParentGate.Outcome
                == CapacityFinalCalleeLargeIndexedRejectGateOutcome.MissingEntryEBX
            && missingParentEBX.ParentGate.CompareLeft == 0x2000
            && missingParentEBX.ParentGate.CompareRight is null
            && !missingParentEBX.FieldGateReached
            && !missingParentEBX.UnreachedChildEDXCaptureProvided
            && !missingParentEBX.UnreachedChildFreshECXPlus10CaptureProvided
            && missingParentEBX.FlagsAtStop is null
            && missingParentEBX.StopAtRawFileOffsetExclusive == 0x012C9263
            && missingBothParentOperands.ParentGate.Outcome
                == CapacityFinalCalleeLargeIndexedRejectGateOutcome.MissingEntryECXAndEBX
            && !missingBothParentOperands.FieldGateReached
            && missingBothParentOperands.UnreachedChildEDXCaptureProvided
            && !missingBothParentOperands.UnreachedChildFreshECXPlus10CaptureProvided
            && missingBothParentOperands.StopAtRawFileOffsetExclusive == 0x012C9263);

        CapacityFinalCalleeLargeRejectFieldLifecyclePlan parentBelow =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: 0, EntryEBX: 1),
                ChildEDX: 0,
                ChildFreshECXPlus10: 0));
        CapacityFinalCalleeLargeRejectFieldLifecyclePlan parentBelowNoUnused =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: 0, EntryEBX: 1),
                ChildEDX: null,
                ChildFreshECXPlus10: null));
        check("Parent JB stops at 8E76 without running or deriving the later field gate",
            parentBelow.Outcome
                == CapacityFinalCalleeLargeRejectFieldLifecycleOutcome
                    .ParentUnsignedBelowTo8E76WithUnusedChildCaptures
            && parentBelow.ParentGate.CompareFlags?.Carry == true
            && parentBelow.ParentGate.BranchTaken == true
            && parentBelow.ParentGate.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && !parentBelow.FieldGateReached
            && parentBelow.ChildECXSource
                == CapacityFinalCalleeLargeRejectFieldLifecycleChildECXSource.None
            && parentBelow.ChildEntryECX is null
            && parentBelow.UnreachedChildEDXCaptureProvided
            && parentBelow.UnreachedChildFreshECXPlus10CaptureProvided
            && parentBelow.ReachedInstructions.SequenceEqual(new uint[]
                { 0x012C9263, 0x012C9265 })
            && parentBelow.OrderedReads.IsEmpty
            && parentBelow.FlagsAtStop?.Carry == true
            && !parentBelow.ExactCleanExitAdmitted
            && parentBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && parentBelowNoUnused.Outcome
                == CapacityFinalCalleeLargeRejectFieldLifecycleOutcome
                    .ParentUnsignedBelowTo8E76
            && parentBelowNoUnused.ExactCleanExitAdmitted
            && !parentBelowNoUnused.UnreachedChildEDXCaptureProvided
            && !parentBelowNoUnused.UnreachedChildFreshECXPlus10CaptureProvided);

        CapacityFinalCalleeLargeRejectFieldLifecyclePlan childBorrow =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: 5, EntryEBX: 4),
                ChildEDX: 1,
                ChildFreshECXPlus10: 0));
        check("Child borrow replaces a non-carry parent CMP and follows the child's unsigned branch",
            childBorrow.ParentGate.CompareFlags?.Carry == false
            && childBorrow.ChildFieldGate?.CompareLeft == 0
            && childBorrow.ChildFieldGate?.CompareRight == 1
            && childBorrow.ChildFieldGate?.CompareResult == uint.MaxValue
            && childBorrow.ChildFieldGate?.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && childBorrow.FlagsAtStop is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && childBorrow.ChildFieldGate?.BranchTaken == true
            && childBorrow.ChildFieldGate?.BranchSelectedDestinationRawFileOffset == 0x012C9275
            && childBorrow.Outcome
                == CapacityFinalCalleeLargeRejectFieldLifecycleOutcome.ChildUnequalTo9275
            && childBorrow.ReachedInstructions.SequenceEqual(new uint[]
                { 0x012C9263, 0x012C9265, 0x012C926B, 0x012C926E })
            && childBorrow.OrderedReads.SequenceEqual(new[]
            {
                new CapacityFinalCalleeLargeIndexedFieldGateRead(
                    0x012C926B, 0x15, true, true, 0)
            })
            && childBorrow.StopAtRawFileOffsetExclusive == 0x012C9275
            && childBorrow.ExactCleanExitAdmitted
            && !childBorrow.FieldStoreReached
            && !childBorrow.MemoryWriteReached);

        CapacityFinalCalleeLargeRejectFieldLifecyclePlan childOverflow =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: 0x80000000, EntryEBX: 0),
                ChildEDX: 1,
                ChildFreshECXPlus10: 0x80000000));
        check("Child signed overflow remains distinct from parent flags and the JNZ condition",
            childOverflow.ParentGate.CompareFlags?.Carry == false
            && childOverflow.ChildFieldGate?.CompareResult == 0x7FFFFFFF
            && childOverflow.ChildFieldGate?.CompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && childOverflow.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && childOverflow.ChildFieldGate?.BranchTaken == true
            && childOverflow.ChildFieldGate?.BranchSelectedDestinationRawFileOffset == 0x012C9275
            && childOverflow.StopAtRawFileOffsetExclusive == 0x012C9275);

        CapacityFinalCalleeLargeRejectFieldLifecyclePlan missingFresh =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: 0x2000, EntryEBX: 0x1000),
                ChildEDX: 0x76543210,
                ChildFreshECXPlus10: null));
        check("Child EDX is captured independently when the fresh indexed read is missing",
            missingFresh.Outcome
                == CapacityFinalCalleeLargeRejectFieldLifecycleOutcome
                    .ChildMissingFreshECXPlus10
            && missingFresh.ExactCleanHandoffToFieldGate
            && missingFresh.ChildECXSource
                == CapacityFinalCalleeLargeRejectFieldLifecycleChildECXSource.ParentEntryECX
            && missingFresh.ChildEntryECX == 0x2000
            && missingFresh.ChildEDXSource
                == CapacityFinalCalleeLargeRejectFieldLifecycleChildEDXSource
                    .ExplicitIndependentLateCapture
            && missingFresh.ChildEntryEDX == 0x76543210
            && missingFresh.ChildFieldGate?.EntryEDXCaptured == true
            && missingFresh.ChildFieldGate?.FreshECXPlus10ReadReached == true
            && missingFresh.ChildFieldGate?.FreshECXPlus10Address == 0x2010
            && missingFresh.ChildFieldGate?.FreshECXPlus10Captured == false
            && missingFresh.ChildFieldGate?.CompareRight == 0x76543210
            && missingFresh.ChildFieldGate?.CompareReached == false
            && missingFresh.ChildFieldGate?.BranchReached == false
            && missingFresh.ChildFieldGate?.StopAtRawFileOffsetExclusive == 0x012C926B
            && missingFresh.FlagsAtStop
                == ConvertParentFlags(missingFresh.ParentGate.CompareFlags)
            && missingFresh.ReachedInstructions.SequenceEqual(new uint[]
                { 0x012C9263, 0x012C9265, 0x012C926B })
            && missingFresh.OrderedReads.Length == 1
            && missingFresh.OrderedReads[0] is
            {
                RawFileOffset: 0x012C926B,
                Address: 0x2010,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && !missingFresh.ExactCleanExitAdmitted
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C926B);

        CapacityFinalCalleeLargeRejectFieldLifecyclePlan missingEDX =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: 0x3000, EntryEBX: 0x2000),
                ChildEDX: null,
                ChildFreshECXPlus10: 0xAABBCCDD));
        check("Fresh memory evidence is retained independently when late child EDX is missing",
            missingEDX.Outcome
                == CapacityFinalCalleeLargeRejectFieldLifecycleOutcome.ChildMissingEDX
            && missingEDX.ChildEntryECX == 0x3000
            && missingEDX.ChildEDXSource
                == CapacityFinalCalleeLargeRejectFieldLifecycleChildEDXSource.None
            && missingEDX.ChildEntryEDX is null
            && missingEDX.ChildFreshECXPlus10Source
                == CapacityFinalCalleeLargeRejectFieldLifecycleFreshValueSource
                    .ExplicitIndependentMemoryEvidence
            && missingEDX.ChildFreshECXPlus10 == 0xAABBCCDD
            && missingEDX.ChildFieldGate?.FreshECXPlus10Captured == true
            && missingEDX.ChildFieldGate?.CompareLeft == 0xAABBCCDD
            && missingEDX.ChildFieldGate?.CompareRight is null
            && missingEDX.ChildFieldGate?.CompareReached == false
            && missingEDX.ChildFieldGate?.BranchReached == false
            && missingEDX.ChildFieldGate?.StopAtRawFileOffsetExclusive == 0x012C926B
            && missingEDX.FlagsAtStop
                == ConvertParentFlags(missingEDX.ParentGate.CompareFlags)
            && missingEDX.OrderedReads.Length == 1
            && missingEDX.OrderedReads[0].ValueCaptured
            && missingEDX.OrderedReads[0].Value == 0xAABBCCDD
            && !missingEDX.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeRejectFieldLifecyclePlan missingBothChildOperands =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: 0x4000, EntryEBX: 0x3000),
                ChildEDX: null,
                ChildFreshECXPlus10: null));
        check("Both child CMP operands can be missing without consumer-gating one by the other",
            missingBothChildOperands.Outcome
                == CapacityFinalCalleeLargeRejectFieldLifecycleOutcome
                    .ChildMissingFreshECXPlus10AndEDX
            && missingBothChildOperands.ChildFieldGate?.EntryEDXCaptured == false
            && missingBothChildOperands.ChildFieldGate?.FreshECXPlus10ReadReached == true
            && missingBothChildOperands.ChildFieldGate?.FreshECXPlus10Captured == false
            && missingBothChildOperands.ChildFieldGate?.CompareLeft is null
            && missingBothChildOperands.ChildFieldGate?.CompareRight is null
            && missingBothChildOperands.ChildFieldGate?.CompareReached == false
            && missingBothChildOperands.ChildFieldGate?.StopAtRawFileOffsetExclusive
                == 0x012C926B
            && missingBothChildOperands.FlagsAtStop
                == ConvertParentFlags(missingBothChildOperands.ParentGate.CompareFlags)
            && missingBothChildOperands.OrderedReads.Length == 1
            && !missingBothChildOperands.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeRejectFieldLifecyclePlan wrappedECX =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: 0xFFFFFFF8, EntryEBX: 0),
                ChildEDX: 8,
                ChildFreshECXPlus10: 8));
        check("Child address derivation wraps from retained parent ECX without clamping",
            wrappedECX.ChildECXSource
                == CapacityFinalCalleeLargeRejectFieldLifecycleChildECXSource.ParentEntryECX
            && wrappedECX.ChildEntryECX == 0xFFFFFFF8
            && wrappedECX.ChildFieldGate?.FreshECXPlus10Address == 8
            && wrappedECX.ChildFieldGate?.CompareLeft == 8
            && wrappedECX.ChildFieldGate?.CompareRight == 8
            && wrappedECX.ChildFieldGate?.CompareResult == 0
            && wrappedECX.ChildFieldGate?.BranchTaken == false
            && wrappedECX.ChildFieldGate?.BranchSelectedDestinationRawFileOffset == 0x012C9270
            && wrappedECX.StopAtRawFileOffsetExclusive == 0x012C9270
            && wrappedECX.ExactCleanExitAdmitted
            && !wrappedECX.FieldStoreReached);

        CapacityFinalCalleeLargeRejectFieldLifecyclePlan zeroAliases =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: 0, EntryEBX: 0),
                ChildEDX: 0,
                ChildFreshECXPlus10: 0));
        CapacityFinalCalleeLargeRejectFieldLifecyclePlan maximumAliases =
            CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Project(new(
                ParentCapture: new(EntryECX: uint.MaxValue, EntryEBX: uint.MaxValue),
                ChildEDX: uint.MaxValue,
                ChildFreshECXPlus10: uint.MaxValue));
        check("Zero and maximum aliased values retain distinct parent and child compare stages",
            zeroAliases.ParentGate.CompareFlags?.Zero == true
            && zeroAliases.ChildEntryECX == 0
            && zeroAliases.ChildFieldGate?.FreshECXPlus10Address == 0x10
            && zeroAliases.ChildFieldGate?.CompareResult == 0
            && zeroAliases.ChildFieldGate?.CompareFlags?.Zero == true
            && zeroAliases.ChildFieldGate?.BranchSelectedDestinationRawFileOffset == 0x012C9270
            && zeroAliases.StopAtRawFileOffsetExclusive == 0x012C9270
            && maximumAliases.ParentGate.CompareFlags?.Zero == true
            && maximumAliases.ChildEntryECX == uint.MaxValue
            && maximumAliases.ChildEntryEDX == uint.MaxValue
            && maximumAliases.ChildFreshECXPlus10 == uint.MaxValue
            && maximumAliases.ChildFieldGate?.FreshECXPlus10Address == 0x0F
            && maximumAliases.ChildFieldGate?.CompareResult == 0
            && maximumAliases.ChildFieldGate?.CompareFlags?.Zero == true
            && maximumAliases.ChildFieldGate?.BranchSelectedDestinationRawFileOffset
                == 0x012C9270
            && maximumAliases.StopAtRawFileOffsetExclusive == 0x012C9270
            && !maximumAliases.FieldStoreReached
            && !maximumAliases.MemoryWriteReached);
    }

    private static CapacityFinalCalleeLargeRejectFieldLifecycleFlags? ConvertParentFlags(
        CapacityFinalCalleeLargeIndexedRejectGateFlags? flags) =>
        flags is CapacityFinalCalleeLargeIndexedRejectGateFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.Auxiliary,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C9263, "3B CB", null, null),
            (0x012C9265, "0F 82 0B FC FF FF", 0x012C8E76, 0x016C9A76),
            (0x012C926B, "39 51 10", null, null),
            (0x012C926E, "75 05", 0x012C9275, 0x016C9E75)
        };
        uint nextRawOffset = 0x012C9263;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeIndexedRejectGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress != unchecked(instruction.RawFileOffset + 0x00400C00)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != unchecked(targetRaw + 0x00400C00)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Instructions.Length
                == expected.Length
            && nextRawOffset == 0x012C9270
            && bytes == "3BCB0F820BFCFFFF3951107505"
            && CapacityFinalCalleeLargeRejectFieldLifecycleProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
