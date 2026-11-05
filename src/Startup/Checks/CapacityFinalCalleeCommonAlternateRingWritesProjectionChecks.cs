using System;
using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

namespace Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonAlternateRingWritesProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Alternate raw/IDA/PE-VA anchors and full production evidence match",
            CapacityFinalCalleeCommonAlternateRingWritesProjection.EntryRawFileOffset
                == 0x012C9343
            && CapacityFinalCalleeCommonAlternateRingWritesProjection.EntryFlatIdaAddress
                == 0x012C9343
            && CapacityFinalCalleeCommonAlternateRingWritesProjection.EntryVirtualAddress
                == 0x016C9F43
            && CapacityFinalCalleeCommonRingWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonAlternateRingWritesProjection.EntryRawFileOffset)
                == CapacityFinalCalleeCommonAlternateRingWritesProjection.EntryVirtualAddress
            && CapacityFinalCalleeCommonAlternateRingWritesProjection.BranchRawFileOffset
                == 0x012C934F
            && CapacityFinalCalleeCommonAlternateRingWritesProjection.BranchNextRawFileOffset
                == 0x012C9354
            && CapacityFinalCalleeCommonAlternateRingWritesProjection.BranchTargetRawFileOffset
                == 0x012C9441
            && CapacityFinalCalleeCommonRingWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonAlternateRingWritesProjection.BranchTargetRawFileOffset)
                == 0x016CA041
            && CapacityFinalCalleeCommonAlternateRingWritesProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9441
            && EvidenceMatches());

        CapacityFinalCalleeCommonRingWritesPlan complete =
            CapacityFinalCalleeCommonAlternateRingWritesProjection.Project(new(
                EntryEDX: 0x1000,
                EntryEAX: 0x2000,
                EntryESI: 0x3000));
        check("Alternate route uses four ordered shared writes and branches to 012C9441",
            complete.EntryRawFileOffset == 0x012C9343
            && complete.Writes is
                [
                    { RawFileOffset: 0x012C9343, Address: 0x1008, Value: 0x3000, Committed: true },
                    { RawFileOffset: 0x012C9346, Address: 0x200C, Value: 0x3000, Committed: true },
                    { RawFileOffset: 0x012C9349, Address: 0x3008, Value: 0x2000, Committed: true },
                    { RawFileOffset: 0x012C934C, Address: 0x300C, Value: 0x1000, Committed: true }
                ]
            && complete.Outcome
                == CapacityFinalCalleeCommonRingWritesOutcome.CompletedTo012C9441
            && complete.BranchReached
            && complete.BranchTargetRawFileOffset == 0x012C9441
            && complete.BranchNextRawFileOffset == 0x012C9354
            && complete.StopAtRawFileOffsetExclusive == 0x012C9441
            && complete.ExactCleanCompletion
            && !complete.DestinationEntered
            && complete.FlagsUnaffected
            && complete.InstructionEvidence.SequenceEqual(
                CapacityFinalCalleeCommonAlternateRingWritesProjection.Evidence)
            && !complete.InstructionEvidence.SequenceEqual(
                CapacityFinalCalleeCommonRingWritesProjection.Instructions));

        CapacityFinalCalleeCommonRingWritesPlan allMissing =
            CapacityFinalCalleeCommonAlternateRingWritesProjection.Project(new(
                EntryEDX: null,
                EntryEAX: null,
                EntryESI: null));
        check("Alternate missing first-store operands stop before later reads or branch",
            allMissing.Outcome
                == CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryEDXForFirstWrite
            && allMissing.Writes is
                [{
                    RawFileOffset: 0x012C9343,
                    Address: null,
                    AddressCaptured: false,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && !allMissing.EntryEAXReadReached
            && !allMissing.BranchReached
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C9343
            && allMissing.FlagsUnaffected);

        CapacityFinalCalleeCommonRingWritesPlan missingEDX =
            CapacityFinalCalleeCommonAlternateRingWritesProjection.Project(new(
                EntryEDX: null,
                EntryEAX: 0x2222,
                EntryESI: 0x3333));
        CapacityFinalCalleeCommonRingWritesPlan missingESI =
            CapacityFinalCalleeCommonAlternateRingWritesProjection.Project(new(
                EntryEDX: 0x1111,
                EntryEAX: 0x2222,
                EntryESI: null));
        check("Alternate first-store address and value captures remain independent",
            missingEDX.Writes is
                [{
                    RawFileOffset: 0x012C9343,
                    Address: null,
                    AddressCaptured: false,
                    ValueCaptured: true,
                    Value: 0x3333,
                    Committed: false
                }]
            && !missingEDX.EntryEAXReadReached
            && missingEDX.UnreachedEntryEAXCaptureProvided
            && missingEDX.FlagsUnaffected
            && missingESI.Writes is
                [{
                    RawFileOffset: 0x012C9343,
                    Address: 0x1119,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && !missingESI.EntryEAXReadReached
            && missingESI.FlagsUnaffected);

        CapacityFinalCalleeCommonRingWritesPlan missingSecondEAX =
            CapacityFinalCalleeCommonAlternateRingWritesProjection.Project(new(
                EntryEDX: 0x1111,
                EntryEAX: null,
                EntryESI: 0x3333));
        check("Alternate missing second-store EAX preserves the first commit and stops at 9346",
            missingSecondEAX.Outcome
                == CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryEAXForSecondWrite
            && missingSecondEAX.Writes is
                [
                    {
                        RawFileOffset: 0x012C9343,
                        Address: 0x1119,
                        Value: 0x3333,
                        Committed: true
                    },
                    {
                        RawFileOffset: 0x012C9346,
                        Address: null,
                        AddressCaptured: false,
                        ValueCaptured: true,
                        Value: 0x3333,
                        Committed: false
                    }
                ]
            && !missingSecondEAX.BranchReached
            && missingSecondEAX.StopAtRawFileOffsetExclusive == 0x012C9346
            && missingSecondEAX.FlagsUnaffected);

        CapacityFinalCalleeCommonRingWritesPlan zeroAndWrap =
            CapacityFinalCalleeCommonAlternateRingWritesProjection.Project(new(
                EntryEDX: 0xFFFFFFF8,
                EntryEAX: 0xFFFFFFF4,
                EntryESI: 0xFFFFFFF8));
        check("Alternate DWORD address arithmetic wraps and retains address zero",
            zeroAndWrap.Writes.Length == 4
            && zeroAndWrap.Writes[0].Address == 0
            && zeroAndWrap.Writes[1].Address == 0
            && zeroAndWrap.Writes[2].Address == 0
            && zeroAndWrap.Writes[3].Address == 4
            && zeroAndWrap.Writes[0].AddressCaptured
            && zeroAndWrap.FlagsUnaffected);

        CapacityFinalCalleeCommonRingWritesPlan aliased =
            CapacityFinalCalleeCommonAlternateRingWritesProjection.Project(new(
                EntryEDX: 0x1000,
                EntryEAX: 0x1000,
                EntryESI: 0x1000));
        check("Alternate aliased stores remain four distinct attempted and committed writes",
            aliased.Writes.Length == 4
            && aliased.Writes is
                [
                    { RawFileOffset: 0x012C9343, Address: 0x1008, Value: 0x1000, Committed: true },
                    { RawFileOffset: 0x012C9346, Address: 0x100C, Value: 0x1000, Committed: true },
                    { RawFileOffset: 0x012C9349, Address: 0x1008, Value: 0x1000, Committed: true },
                    { RawFileOffset: 0x012C934C, Address: 0x100C, Value: 0x1000, Committed: true }
                ]
            && aliased.Writes.Count(static write => write.Committed) == 4
            && aliased.FlagsUnaffected);
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
            (0x012C9343, 0x016C9F43, "89 72 08", null, null),
            (0x012C9346, 0x016C9F46, "89 70 0C", null, null),
            (0x012C9349, 0x016C9F49, "89 46 08", null, null),
            (0x012C934C, 0x016C9F4C, "89 56 0C", null, null),
            (0x012C934F, 0x016C9F4F, "E9 ED 00 00 00", 0x012C9441, 0x016CA041)
        };
        ImmutableArray<CapacityFinalCalleeCommonRingWritesInstructionEvidence> evidence =
            CapacityFinalCalleeCommonAlternateRingWritesProjection.Evidence;
        if (evidence.Length != expected.Length)
            return false;

        uint nextRawOffset =
            CapacityFinalCalleeCommonAlternateRingWritesProjection.EntryRawFileOffset;
        string capturedBytes = string.Empty;
        for (int i = 0; i < evidence.Length; i++)
        {
            CapacityFinalCalleeCommonRingWritesInstructionEvidence instruction = evidence[i];
            var expectedInstruction = expected[i];
            if (instruction.RawFileOffset != expectedInstruction.Raw
                || instruction.FlatIdaAddress != expectedInstruction.Raw
                || instruction.VirtualAddress != expectedInstruction.Virtual
                || instruction.RawFileOffset != nextRawOffset
                || instruction.Bytes != expectedInstruction.Bytes
                || instruction.BranchTargetRawFileOffset != expectedInstruction.BranchTargetRaw
                || instruction.BranchTargetVirtualAddress
                    != expectedInstruction.BranchTargetVirtual
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonRingWritesProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(Normalize(instruction.Bytes).Length / 2);
            capturedBytes += Normalize(instruction.Bytes);
        }

        return nextRawOffset == 0x012C9354
            && unchecked(0x012C9354u + 0xEDu) == 0x012C9441u
            && capturedBytes == Normalize(
                CapacityFinalCalleeCommonAlternateRingWritesProjection.CapturedWindowBytes)
            && capturedBytes == "89720889700C89460889560CE9ED000000";
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
