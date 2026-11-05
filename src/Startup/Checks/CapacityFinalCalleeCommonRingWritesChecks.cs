using System;
using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

namespace Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonRingWritesChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw/IDA/PE-VA anchors and production instruction evidence match",
            CapacityFinalCalleeCommonRingWritesProjection.EntryRawFileOffset == 0x012C9323
            && CapacityFinalCalleeCommonRingWritesProjection.EntryFlatIdaAddress == 0x012C9323
            && CapacityFinalCalleeCommonRingWritesProjection.EntryVirtualAddress == 0x016C9F23
            && CapacityFinalCalleeCommonRingWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonRingWritesProjection.EntryRawFileOffset)
                == CapacityFinalCalleeCommonRingWritesProjection.EntryVirtualAddress
            && CapacityFinalCalleeCommonRingWritesProjection.BranchRawFileOffset == 0x012C932F
            && CapacityFinalCalleeCommonRingWritesProjection.BranchNextRawFileOffset == 0x012C9334
            && CapacityFinalCalleeCommonRingWritesProjection.BranchTargetRawFileOffset == 0x012C9441
            && CapacityFinalCalleeCommonRingWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonRingWritesProjection.BranchTargetRawFileOffset)
                == 0x016CA041
            && CapacityFinalCalleeCommonRingWritesProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9441
            && EvidenceMatches());

        CapacityFinalCalleeCommonRingWritesPlan allMissing =
            CapacityFinalCalleeCommonRingWritesProjection.Project(new(
                EntryEDX: null,
                EntryEAX: null,
                EntryESI: null));
        check("All missing operands stop at the first store with independent unknown address and value",
            allMissing.Outcome
                == CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryEDXForFirstWrite
            && allMissing.Writes is
                [{
                    RawFileOffset: 0x012C9323,
                    Address: null,
                    AddressCaptured: false,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && !allMissing.EntryEAXReadReached
            && !allMissing.UnreachedEntryEAXCaptureProvided
            && !allMissing.BranchReached
            && !allMissing.ExactCleanCompletion
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C9323);

        CapacityFinalCalleeCommonRingWritesPlan missingEDX =
            CapacityFinalCalleeCommonRingWritesProjection.Project(new(
                EntryEDX: null,
                EntryEAX: 0x2222,
                EntryESI: 0x3333));
        check("Missing first-use EDX retains the independent value and leaves supplied EAX unreached",
            missingEDX.Outcome
                == CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryEDXForFirstWrite
            && missingEDX.Writes is
                [{
                    Address: null,
                    AddressCaptured: false,
                    ValueCaptured: true,
                    Value: 0x3333,
                    Committed: false
                }]
            && missingEDX.EntryEDXReadReached
            && missingEDX.EntryESIReadReached
            && !missingEDX.EntryEAXReadReached
            && missingEDX.UnreachedEntryEAXCaptureProvided
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C9323);

        CapacityFinalCalleeCommonRingWritesPlan missingESI =
            CapacityFinalCalleeCommonRingWritesProjection.Project(new(
                EntryEDX: 0x1111,
                EntryEAX: 0x2222,
                EntryESI: null));
        check("Missing first-use ESI retains the independently computed address",
            missingESI.Outcome
                == CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryESIForFirstWrite
            && missingESI.Writes is
                [{
                    Address: 0x1119,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && !missingESI.EntryEAXReadReached
            && missingESI.UnreachedEntryEAXCaptureProvided
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9323);

        CapacityFinalCalleeCommonRingWritesPlan missingEAX =
            CapacityFinalCalleeCommonRingWritesProjection.Project(new(
                EntryEDX: 0x1111,
                EntryEAX: null,
                EntryESI: 0x3333));
        check("Missing second-store EAX keeps the prior commit and independent attempted value",
            missingEAX.Outcome
                == CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryEAXForSecondWrite
            && missingEAX.Writes is
                [
                    { RawFileOffset: 0x012C9323, Address: 0x1119, Value: 0x3333, Committed: true },
                    {
                        RawFileOffset: 0x012C9326,
                        Address: null,
                        AddressCaptured: false,
                        ValueCaptured: true,
                        Value: 0x3333,
                        Committed: false
                    }
                ]
            && missingEAX.EntryEAXReadReached
            && !missingEAX.UnreachedEntryEAXCaptureProvided
            && !missingEAX.BranchReached
            && missingEAX.StopAtRawFileOffsetExclusive == 0x012C9326);

        CapacityFinalCalleeCommonRingWritesPlan complete =
            CapacityFinalCalleeCommonRingWritesProjection.Project(new(
                EntryEDX: 0x1000,
                EntryEAX: 0x2000,
                EntryESI: 0x3000));
        check("Complete path preserves the four ordered stores and stops cleanly at 012C9441",
            complete.Outcome == CapacityFinalCalleeCommonRingWritesOutcome.CompletedTo012C9441
            && complete.Writes is
                [
                    { RawFileOffset: 0x012C9323, Address: 0x1008, Value: 0x3000, Committed: true },
                    { RawFileOffset: 0x012C9326, Address: 0x200C, Value: 0x3000, Committed: true },
                    { RawFileOffset: 0x012C9329, Address: 0x3008, Value: 0x2000, Committed: true },
                    { RawFileOffset: 0x012C932C, Address: 0x300C, Value: 0x1000, Committed: true }
                ]
            && complete.BranchReached
            && complete.BranchTargetRawFileOffset == 0x012C9441
            && complete.ExactCleanCompletion
            && complete.StopAtRawFileOffsetExclusive == 0x012C9441
            && !complete.DestinationEntered
            && complete.FlagsUnaffected);

        CapacityFinalCalleeCommonRingWritesPlan zeroAndWrap =
            CapacityFinalCalleeCommonRingWritesProjection.Project(new(
                EntryEDX: 0xFFFFFFFC,
                EntryEAX: 0xFFFFFFF8,
                EntryESI: 0xFFFFFFF8));
        check("DWORD address arithmetic wraps and retains address zero",
            zeroAndWrap.Writes.Length == 4
            && zeroAndWrap.Writes[0].Address == 4
            && zeroAndWrap.Writes[1].Address == 4
            && zeroAndWrap.Writes[2].Address == 0
            && zeroAndWrap.Writes[3].Address == 4
            && zeroAndWrap.Writes[2].AddressCaptured);

        CapacityFinalCalleeCommonRingWritesPlan maximum =
            CapacityFinalCalleeCommonRingWritesProjection.Project(new(
                EntryEDX: uint.MaxValue,
                EntryEAX: uint.MaxValue,
                EntryESI: uint.MaxValue));
        check("Maximum register values retain unsigned DWORD semantics",
            maximum.Writes is
                [
                    { Address: 7, Value: uint.MaxValue },
                    { Address: 0xB, Value: uint.MaxValue },
                    { Address: 7, Value: uint.MaxValue },
                    { Address: 0xB, Value: uint.MaxValue }
                ]);

        CapacityFinalCalleeCommonRingWritesPlan aliased =
            CapacityFinalCalleeCommonRingWritesProjection.Project(new(
                EntryEDX: 0x1000,
                EntryEAX: 0x1000,
                EntryESI: 0x1000));
        check("Aliased destinations remain four distinct ordered committed writes",
            aliased.Writes is
                [
                    { RawFileOffset: 0x012C9323, Address: 0x1008, Value: 0x1000, Committed: true },
                    { RawFileOffset: 0x012C9326, Address: 0x100C, Value: 0x1000, Committed: true },
                    { RawFileOffset: 0x012C9329, Address: 0x1008, Value: 0x1000, Committed: true },
                    { RawFileOffset: 0x012C932C, Address: 0x100C, Value: 0x1000, Committed: true }
                ]);
    }

    private static bool EvidenceMatches()
    {
        var expected = new (
            uint Raw,
            uint Virtual,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C9323, 0x016C9F23, "89 72 08", null, null),
            (0x012C9326, 0x016C9F26, "89 70 0C", null, null),
            (0x012C9329, 0x016C9F29, "89 46 08", null, null),
            (0x012C932C, 0x016C9F2C, "89 56 0C", null, null),
            (0x012C932F, 0x016C9F2F, "E9 0D 01 00 00", 0x012C9441, 0x016CA041)
        };
        ImmutableArray<CapacityFinalCalleeCommonRingWritesInstructionEvidence> instructions =
            CapacityFinalCalleeCommonRingWritesProjection.Instructions;
        if (instructions.Length != expected.Length)
            return false;

        uint nextRawOffset = CapacityFinalCalleeCommonRingWritesProjection.EntryRawFileOffset;
        string capturedBytes = string.Empty;
        for (int i = 0; i < instructions.Length; i++)
        {
            CapacityFinalCalleeCommonRingWritesInstructionEvidence instruction = instructions[i];
            var expectedInstruction = expected[i];
            if (instruction.RawFileOffset != expectedInstruction.Raw
                || instruction.FlatIdaAddress != expectedInstruction.Raw
                || instruction.VirtualAddress != expectedInstruction.Virtual
                || instruction.RawFileOffset != nextRawOffset
                || instruction.Bytes != expectedInstruction.Bytes
                || instruction.BranchTargetRawFileOffset != expectedInstruction.BranchTargetRaw
                || instruction.BranchTargetVirtualAddress != expectedInstruction.BranchTargetVirtual
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonRingWritesProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(Normalize(instruction.Bytes).Length / 2);
            capturedBytes += Normalize(instruction.Bytes);
        }

        return nextRawOffset == 0x012C9334
            && unchecked(0x012C9334u + 0x10Du) == 0x012C9441u
            && capturedBytes == Normalize(
                CapacityFinalCalleeCommonRingWritesProjection.CapturedWindowBytes)
            && capturedBytes == "89720889700C89460889560CE90D010000";
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
