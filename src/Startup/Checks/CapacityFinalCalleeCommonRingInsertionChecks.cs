using System;
using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

namespace Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonRingInsertionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw/IDA/PE-VA anchors, 19-byte evidence, and pre-DEC bound match",
            CapacityFinalCalleeCommonRingInsertionProjection.EntryRawFileOffset == 0x012C941C
            && CapacityFinalCalleeCommonRingInsertionProjection.EntryFlatIdaAddress == 0x012C941C
            && CapacityFinalCalleeCommonRingInsertionProjection.EntryVirtualAddress == 0x016CA01C
            && CapacityFinalCalleeCommonRingInsertionProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonRingInsertionProjection.EntryRawFileOffset)
                == CapacityFinalCalleeCommonRingInsertionProjection.EntryVirtualAddress
            && CapacityFinalCalleeCommonRingInsertionProjection.BoundaryRawFileOffsetExclusive
                == 0x012C942F
            && CapacityFinalCalleeCommonRingInsertionProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonRingInsertionProjection.BoundaryRawFileOffsetExclusive)
                == 0x016CA02F
            && EvidenceMatches());

        CapacityFinalCalleeCommonRingInsertionPlan allMissing =
            CapacityFinalCalleeCommonRingInsertionProjection.Project(new(
                EntryEAX: null,
                EntryESI: null,
                LateEntryEDX: 0x4444));
        check("A blocked first store preserves independent operand captures and leaves late EDX unused",
            allMissing.Outcome
                == CapacityFinalCalleeCommonRingInsertionOutcome.MissingEntryEAXForFirstWrite
            && allMissing.Writes is
                [{
                    RawFileOffset: 0x012C941C,
                    Address: null,
                    AddressCaptured: false,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && allMissing.EntryEAXReadReached
            && allMissing.EntryESIReadReached
            && !allMissing.LateEntryEDXReadReached
            && allMissing.UnreachedLateEntryEDXCaptureProvided
            && allMissing.LateEntryEDXUnusedBecauseFirstStoreBlocked
            && !allMissing.BoundaryReached
            && !allMissing.BoundaryInstructionEntered
            && !allMissing.ExactCleanCompletion
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C941C);

        CapacityFinalCalleeCommonRingInsertionPlan missingEAX =
            CapacityFinalCalleeCommonRingInsertionProjection.Project(new(
                EntryEAX: null,
                EntryESI: 0x3333,
                LateEntryEDX: 0x4444));
        CapacityFinalCalleeCommonRingInsertionPlan missingESI =
            CapacityFinalCalleeCommonRingInsertionProjection.Project(new(
                EntryEAX: 0x1111,
                EntryESI: null,
                LateEntryEDX: 0x4444));
        check("First-store missing address and value operands are represented independently",
            missingEAX.Outcome
                == CapacityFinalCalleeCommonRingInsertionOutcome.MissingEntryEAXForFirstWrite
            && missingEAX.Writes is
                [{
                    Address: null,
                    AddressCaptured: false,
                    ValueCaptured: true,
                    Value: 0x3333,
                    Committed: false
                }]
            && missingEAX.LateEntryEDXUnusedBecauseFirstStoreBlocked
            && missingEAX.UnreachedLateEntryEDXCaptureProvided
            && missingESI.Outcome
                == CapacityFinalCalleeCommonRingInsertionOutcome.MissingEntryESIForFirstWrite
            && missingESI.Writes is
                [{
                    Address: 0x111D,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && missingESI.LateEntryEDXUnusedBecauseFirstStoreBlocked
            && missingESI.UnreachedLateEntryEDXCaptureProvided
            && !missingEAX.LateEntryEDXReadReached
            && !missingESI.LateEntryEDXReadReached);

        CapacityFinalCalleeCommonRingInsertionPlan missingLateEDX =
            CapacityFinalCalleeCommonRingInsertionProjection.Project(new(
                EntryEAX: 0x1000,
                EntryESI: 0x2000,
                LateEntryEDX: null));
        check("A missing second-store EDX preserves the earlier commit and the independent source value",
            missingLateEDX.Outcome
                == CapacityFinalCalleeCommonRingInsertionOutcome.MissingLateEntryEDXForSecondWrite
            && missingLateEDX.Writes is
                [
                    {
                        RawFileOffset: 0x012C941C,
                        Address: 0x100C,
                        Value: 0x2000,
                        Committed: true
                    },
                    {
                        RawFileOffset: 0x012C941F,
                        Address: null,
                        AddressCaptured: false,
                        ValueCaptured: true,
                        Value: 0x2000,
                        Committed: false
                    }
                ]
            && missingLateEDX.EntryEAXReadReached
            && missingLateEDX.EntryESIReadReached
            && missingLateEDX.LateEntryEDXReadReached
            && !missingLateEDX.UnreachedLateEntryEDXCaptureProvided
            && !missingLateEDX.LateEntryEDXUnusedBecauseFirstStoreBlocked
            && !missingLateEDX.BoundaryReached
            && !missingLateEDX.ExactCleanCompletion
            && missingLateEDX.StopAtRawFileOffsetExclusive == 0x012C941F);

        CapacityFinalCalleeCommonRingInsertionPlan complete =
            CapacityFinalCalleeCommonRingInsertionProjection.Project(new(
                EntryEAX: 0x1000,
                EntryESI: 0x2000,
                LateEntryEDX: 0x3000));
        check("Complete path preserves all five ordered stores and stops before DEC at 942F",
            complete.Outcome
                == CapacityFinalCalleeCommonRingInsertionOutcome.CompletedToBoundaryBeforeDec
            && complete.Writes is
                [
                    { RawFileOffset: 0x012C941C, Address: 0x100C, Value: 0x2000, Committed: true },
                    { RawFileOffset: 0x012C941F, Address: 0x3008, Value: 0x2000, Committed: true },
                    { RawFileOffset: 0x012C9422, Address: 0x2008, Value: 0x1000, Committed: true },
                    { RawFileOffset: 0x012C9425, Address: 0x200C, Value: 0x3000, Committed: true },
                    { RawFileOffset: 0x012C9428, Address: 0x2018, Value: 0, Committed: true }
                ]
            && complete.EntryRawFileOffset == 0x012C941C
            && complete.BoundaryRawFileOffsetExclusive == 0x012C942F
            && complete.StopAtRawFileOffsetExclusive == 0x012C942F
            && complete.BoundaryReached
            && !complete.BoundaryInstructionEntered
            && complete.ExactCleanCompletion
            && !complete.DestinationEntered
            && complete.InstructionEvidence.SequenceEqual(
                CapacityFinalCalleeCommonRingInsertionProjection.Instructions)
            && complete.LateEntryEDXReadReached
            && !complete.UnreachedLateEntryEDXCaptureProvided
            && !complete.LateEntryEDXUnusedBecauseFirstStoreBlocked);

        CapacityFinalCalleeCommonRingInsertionPlan zeroAndWrap =
            CapacityFinalCalleeCommonRingInsertionProjection.Project(new(
                EntryEAX: 0xFFFFFFF4,
                EntryESI: 0xFFFFFFF8,
                LateEntryEDX: 0xFFFFFFF8));
        check("DWORD addresses wrap while zero addresses and the zero constant remain captured",
            zeroAndWrap.Writes is
                [
                    { Address: 0, AddressCaptured: true, Value: 0xFFFFFFF8 },
                    { Address: 0, AddressCaptured: true, Value: 0xFFFFFFF8 },
                    { Address: 0, AddressCaptured: true, Value: 0xFFFFFFF4 },
                    { Address: 4, AddressCaptured: true, Value: 0xFFFFFFF8 },
                    { Address: 0x10, AddressCaptured: true, Value: 0, ValueCaptured: true }
                ]);

        CapacityFinalCalleeCommonRingInsertionPlan aliased =
            CapacityFinalCalleeCommonRingInsertionProjection.Project(new(
                EntryEAX: 0x1000,
                EntryESI: 0x1000,
                LateEntryEDX: 0x1000));
        check("Aliased destinations remain five distinct ordered writes with their own values",
            aliased.Writes is
                [
                    { RawFileOffset: 0x012C941C, Address: 0x100C, Value: 0x1000, Committed: true },
                    { RawFileOffset: 0x012C941F, Address: 0x1008, Value: 0x1000, Committed: true },
                    { RawFileOffset: 0x012C9422, Address: 0x1008, Value: 0x1000, Committed: true },
                    { RawFileOffset: 0x012C9425, Address: 0x100C, Value: 0x1000, Committed: true },
                    { RawFileOffset: 0x012C9428, Address: 0x1018, Value: 0, Committed: true }
                ]
            && aliased.Writes.Length == 5);

        check("MOVs preserve flags without claiming standalone flags are known",
            complete.MovInstructionsPreserveFlags
            && !complete.StandaloneEntryFlagsKnown
            && complete.IncomingExternalFlagsCanBeRetained);
    }

    private static bool EvidenceMatches()
    {
        var expected = new (
            uint Raw,
            uint Virtual,
            string Bytes)[]
        {
            (0x012C941C, 0x016CA01C, "89 70 0C"),
            (0x012C941F, 0x016CA01F, "89 72 08"),
            (0x012C9422, 0x016CA022, "89 46 08"),
            (0x012C9425, 0x016CA025, "89 56 0C"),
            (0x012C9428, 0x016CA028, "C7 46 18 00 00 00 00")
        };
        ImmutableArray<CapacityFinalCalleeCommonRingInsertionInstructionEvidence> instructions =
            CapacityFinalCalleeCommonRingInsertionProjection.Instructions;
        if (instructions.Length != expected.Length)
            return false;

        uint nextRawOffset = CapacityFinalCalleeCommonRingInsertionProjection.EntryRawFileOffset;
        string capturedBytes = string.Empty;
        for (int i = 0; i < instructions.Length; i++)
        {
            CapacityFinalCalleeCommonRingInsertionInstructionEvidence instruction =
                instructions[i];
            var expectedInstruction = expected[i];
            if (instruction.RawFileOffset != expectedInstruction.Raw
                || instruction.FlatIdaAddress != expectedInstruction.Raw
                || instruction.VirtualAddress != expectedInstruction.Virtual
                || instruction.RawFileOffset != nextRawOffset
                || instruction.Bytes != expectedInstruction.Bytes
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonRingInsertionProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(Normalize(instruction.Bytes).Length / 2);
            capturedBytes += Normalize(instruction.Bytes);
        }

        return nextRawOffset
                == CapacityFinalCalleeCommonRingInsertionProjection.BoundaryRawFileOffsetExclusive
            && capturedBytes == Normalize(
                CapacityFinalCalleeCommonRingInsertionProjection.CapturedWindowBytes)
            && capturedBytes == "89700C89720889460889560CC7461800000000";
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
