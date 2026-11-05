using System.Numerics;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateTagWritesLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The clean tag-three gate hands off at 90B0 and the field window ends at 9441",
            CapacityFinalCalleeAlternateTagWritesLifecycleProjection.EntryRawFileOffset
                == 0x012C90A2
            && CapacityFinalCalleeAlternateTagWritesLifecycleProjection
                .AlternateFieldWritesEntryRawFileOffset == 0x012C90B0
            && CapacityFinalCalleeAlternateTagWritesLifecycleProjection
                .TagThreeHandoffRawFileOffset == 0x012C90B0
            && CapacityFinalCalleeAlternateTagWritesLifecycleProjection
                .AlternateDestinationRawFileOffset == 0x012C8EF8
            && CapacityFinalCalleeAlternateTagWritesLifecycleProjection
                .FinalTargetRawFileOffset == 0x012C9441
            && Normalize(CapacityFinalCalleeAlternateTagGateProjection
                .CapturedWindowBytes) == "8B420483E0033C030F8548FEFFFF"
            && Normalize(CapacityFinalCalleeAlternateFieldWritesProjection
                .CapturedWindowBytes)
                == "893DD0ADED018BC7836204FE83C801894604893C3EE977030000");

        bool allNonThreeTagsBounded = true;
        for (uint tag = 0; tag < 3; tag++)
        {
            CapacityFinalCalleeAlternateTagWritesLifecyclePlan plan =
                CapacityFinalCalleeAlternateTagWritesLifecycleProjection.Project(
                    Capture(entryEdx: 0x100, gateFresh: 0x40 | tag));
            CapacityFinalCalleeAlternateTagGatePlan gate = plan.TagGate;
            allNonThreeTagsBounded &=
                plan.Outcome
                    == CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                        .MaskedTagNotThreeTo8EF8
                && plan.Admitted
                && plan.AlternateFieldWrites is null
                && !plan.AlternateFieldWritesReached
                && !plan.ExactTagThreeHandoffAdmitted
                && !plan.DownstreamEntryEDXDerived
                && plan.DownstreamEntryEDX is null
                && gate.AndResult == tag
                && gate.ByteCompareResult == unchecked((byte)(tag - 3))
                && gate.ByteCompareFlags == CompareFlags((byte)tag, 3)
                && gate.JnzTaken == true
                && gate.JnzSelectedDestinationRawFileOffset == 0x012C8EF8
                && gate.StopAtRawFileOffsetExclusive == 0x012C8EF8
                && !plan.UnreachedDownstreamEntryEDICaptureProvided
                && !plan.UnreachedDownstreamFreshEdxPlus4CaptureProvided
                && !plan.UnreachedDownstreamEntryESICaptureProvided;
        }
        check("Every masked tag other than three remains a clean bounded 8EF8 route",
            allNonThreeTagsBounded);

        CapacityFinalCalleeAlternateTagWritesLifecyclePlan completed =
            CapacityFinalCalleeAlternateTagWritesLifecycleProjection.Project(
                Capture(
                    entryEdx: 0x80,
                    gateFresh: 0xDEADBEEF,
                    entryEdi: 0x80000000,
                    downstreamFresh: 0xFFFFFFFF,
                    entryEsi: 0x200));
        CapacityFinalCalleeAlternateFieldWritesPlan? completedWrites =
            completed.AlternateFieldWrites;
        check("Only an exact clean tag-three gate reaches independent writes and final target 9441",
            completed.Outcome
                == CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                    .CompletedAt9441
            && completed.Admitted
            && completed.TagGate.Outcome
                == CapacityFinalCalleeAlternateTagGateOutcome
                    .MaskedTagThreeToBoundary
            && completed.TagGate.FreshDWORDAddress == 0x84
            && completed.TagGate.FreshDWORD == 0xDEADBEEF
            && completed.TagGate.ByteCompareFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && completed.AlternateFieldWritesReached
            && completed.ExactTagThreeHandoffAdmitted
            && completed.DownstreamEntryEDXDerived
            && completed.DownstreamEntryEDX == 0x80
            && completedWrites is
            {
                Outcome: CapacityFinalCalleeAlternateFieldWritesOutcome.Completed,
                Capture.EntryEDI: 0x80000000,
                Capture.EntryEDX: 0x80,
                Capture.FreshEdxPlus4: 0xFFFFFFFF,
                Capture.EntryESI: 0x200,
                FreshEdxPlus4Address: 0x84,
                FreshEdxPlus4Captured: true,
                FreshEdxPlus4: 0xFFFFFFFF,
                AndResult: 0xFFFFFFFE,
                AndFlags:
                {
                    Carry: false,
                    Parity: false,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                },
                EAXAfterOr: 0x80000001,
                OrFlags:
                {
                    Carry: false,
                    Parity: false,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                },
                JumpReached: true,
                JumpTargetRawFileOffset: 0x012C9441,
                StopAtRawFileOffsetExclusive: 0x012C9441,
                ExactCleanExitAdmitted: true
            }
            && completedWrites.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C90B0, 0x012C90B8, 0x012C90BF, 0x012C90C2
                })
            && completed.TagGate.Reads.Length == 1
            && completed.TagGate.Reads[0].Value == 0xDEADBEEF
            && completedWrites.Writes.Length == 4
            && !completed.UnreachedDownstreamFreshEdxPlus4CaptureProvided
            && !completed.UnreachedDownstreamEntryESICaptureProvided
            && completed.StopAtRawFileOffsetExclusive == 0x012C9441);

        bool allNonThreeTagsRejectDownstreamCaptures = true;
        for (uint tag = 0; tag < 3; tag++)
        {
            CapacityFinalCalleeAlternateTagWritesLifecyclePlan plan =
                CapacityFinalCalleeAlternateTagWritesLifecycleProjection.Project(
                    Capture(
                        entryEdx: 0x100,
                        gateFresh: tag,
                        entryEdi: 0,
                        downstreamFresh: 0,
                        entryEsi: 0));
            allNonThreeTagsRejectDownstreamCaptures &=
                plan.Outcome
                    == CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                        .MaskedTagNotThreeWithDownstreamCaptures
                && !plan.Admitted
                && plan.AlternateFieldWrites is null
                && !plan.DownstreamEntryEDXDerived
                && plan.UnreachedDownstreamEntryEDICaptureProvided
                && plan.UnreachedDownstreamFreshEdxPlus4CaptureProvided
                && plan.UnreachedDownstreamEntryESICaptureProvided
                && plan.StopAtRawFileOffsetExclusive == 0x012C8EF8;
        }
        check("Later captures cannot bypass any non-three tag route",
            allNonThreeTagsRejectDownstreamCaptures);

        CapacityFinalCalleeAlternateTagWritesLifecyclePlan missingGateAddress =
            CapacityFinalCalleeAlternateTagWritesLifecycleProjection.Project(
                Capture(entryEdx: null, gateFresh: 3,
                    entryEdi: 0x10, downstreamFresh: 0x20, entryEsi: 0x30));
        CapacityFinalCalleeAlternateTagWritesLifecyclePlan missingGateRead =
            CapacityFinalCalleeAlternateTagWritesLifecycleProjection.Project(
                Capture(entryEdx: 0x100, gateFresh: null,
                    entryEdi: 0x10, downstreamFresh: 0x20, entryEsi: 0x30));
        check("Missing gate address or gate read blocks all downstream captures",
            missingGateAddress.Outcome
                == CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                    .TagGateIncompleteWithDownstreamCaptures
            && missingGateAddress.TagGate.Outcome
                == CapacityFinalCalleeAlternateTagGateOutcome
                    .MissingEntryEDXForRead
            && missingGateAddress.TagGate.StopAtRawFileOffsetExclusive == 0x012C90A2
            && missingGateRead.Outcome
                == CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                    .TagGateIncompleteWithDownstreamCaptures
            && missingGateRead.TagGate.Outcome
                == CapacityFinalCalleeAlternateTagGateOutcome.MissingFreshDWORD
            && missingGateRead.TagGate.StopAtRawFileOffsetExclusive == 0x012C90A2
            && missingGateAddress.AlternateFieldWrites is null
            && missingGateRead.AlternateFieldWrites is null
            && !missingGateAddress.AlternateFieldWritesReached
            && !missingGateRead.AlternateFieldWritesReached
            && !missingGateAddress.DownstreamEntryEDXDerived
            && !missingGateRead.DownstreamEntryEDXDerived
            && missingGateAddress.UnreachedDownstreamEntryEDICaptureProvided
            && missingGateAddress.UnreachedDownstreamFreshEdxPlus4CaptureProvided
            && missingGateAddress.UnreachedDownstreamEntryESICaptureProvided
            && missingGateRead.UnreachedDownstreamEntryEDICaptureProvided
            && missingGateRead.UnreachedDownstreamFreshEdxPlus4CaptureProvided
            && missingGateRead.UnreachedDownstreamEntryESICaptureProvided
            && !missingGateAddress.Admitted
            && !missingGateRead.Admitted);

        CapacityFinalCalleeAlternateTagWritesLifecyclePlan missingEdi =
            CapacityFinalCalleeAlternateTagWritesLifecycleProjection.Project(
                Capture(
                    entryEdx: 0x100,
                    gateFresh: 3,
                    entryEdi: null,
                    downstreamFresh: 0x55667788,
                    entryEsi: 0x300));
        CapacityFinalCalleeAlternateFieldWritesPlan? missingEdiWrites =
            missingEdi.AlternateFieldWrites;
        check("Missing downstream EDI cannot fabricate the unused derived EDX",
            missingEdi.Outcome
                == CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                    .AlternateFieldWritesIncomplete
            && !missingEdi.Admitted
            && missingEdi.AlternateFieldWritesReached
            && missingEdi.ExactTagThreeHandoffAdmitted
            && !missingEdi.DownstreamEntryEDXDerived
            && missingEdi.DownstreamEntryEDX is null
            && missingEdiWrites is
            {
                Outcome: CapacityFinalCalleeAlternateFieldWritesOutcome.MissingEntryEDI,
                Capture.EntryEDI: null,
                Capture.EntryEDX: null,
                FreshEdxPlus4ReadReached: false,
                EntryEDXCaptured: false,
                UnreachedEntryEDXCaptureProvided: false,
                UnreachedFreshEdxPlus4CaptureProvided: true,
                UnreachedEntryESICaptureProvided: true,
                StopAtRawFileOffsetExclusive: 0x012C90B0
            }
            && missingEdi.UnreachedDownstreamFreshEdxPlus4CaptureProvided
            && missingEdi.UnreachedDownstreamEntryESICaptureProvided
            && missingEdi.StopAtRawFileOffsetExclusive == 0x012C90B0);

        CapacityFinalCalleeAlternateTagWritesLifecyclePlan missingDownstreamRead =
            CapacityFinalCalleeAlternateTagWritesLifecycleProjection.Project(
                Capture(
                    entryEdx: 0xFFFFFFFC,
                    gateFresh: 3,
                    entryEdi: 0,
                    downstreamFresh: null,
                    entryEsi: 0x200));
        CapacityFinalCalleeAlternateFieldWritesPlan? missingReadWrites =
            missingDownstreamRead.AlternateFieldWrites;
        check("A missing fresh read after the global write preserves that write and stops at B8",
            missingDownstreamRead.Outcome
                == CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                    .AlternateFieldWritesIncomplete
            && missingDownstreamRead.TagGate.Reads[0].Address == 0
            && missingDownstreamRead.TagGate.Reads[0].Value == 3
            && missingReadWrites is
            {
                Outcome: CapacityFinalCalleeAlternateFieldWritesOutcome
                    .MissingFreshEdxPlus4,
                GlobalWriteReached: true,
                GlobalWriteAddress: 0x01EDADD0,
                GlobalWriteValue: 0,
                EAXRetained: 0,
                FreshEdxPlus4Address: 0,
                FreshEdxPlus4ReadReached: true,
                FreshEdxPlus4Captured: false,
                AndInstructionReached: true,
                AndWriteReached: false,
                OrInstructionReached: false,
                UnreachedEntryESICaptureProvided: true,
                StopAtRawFileOffsetExclusive: 0x012C90B8
            }
            && missingReadWrites.Writes.Length == 1
            && missingReadWrites.Writes[0]
                == new CapacityFinalCalleeAlternateFieldWritesWrite(
                    0x012C90B0, 0x01EDADD0, 0)
            && missingDownstreamRead.DownstreamEntryEDXDerived
            && missingDownstreamRead.DownstreamEntryEDX == 0xFFFFFFFC
            && missingDownstreamRead.UnreachedDownstreamEntryESICaptureProvided
            && missingDownstreamRead.StopAtRawFileOffsetExclusive == 0x012C90B8);

        CapacityFinalCalleeAlternateTagWritesLifecyclePlan sameAddressFresh =
            CapacityFinalCalleeAlternateTagWritesLifecycleProjection.Project(
                Capture(
                    entryEdx: 0x01EDADCC,
                    gateFresh: 3,
                    entryEdi: 4,
                    downstreamFresh: 0xFFFFFFFF,
                    entryEsi: 0xFFFFFFFC));
        CapacityFinalCalleeAlternateFieldWritesPlan? sameAddressWrites =
            sameAddressFresh.AlternateFieldWrites;
        check("The later EDX+4 reload stays independent when gate read and global write alias",
            sameAddressFresh.TagGate.Reads[0].Address == 0x01EDADD0
            && sameAddressFresh.TagGate.Reads[0].Value == 3
            && sameAddressWrites is not null
            && sameAddressWrites.GlobalWriteAddress == 0x01EDADD0
            && sameAddressWrites.GlobalWriteValue == 4
            && sameAddressWrites.FreshEdxPlus4Address == 0x01EDADD0
            && sameAddressWrites.FreshEdxPlus4 == 0xFFFFFFFF
            && sameAddressWrites.AndWriteAddress == 0x01EDADD0
            && sameAddressWrites.AndWriteValue == 0xFFFFFFFE
            && sameAddressWrites.Writes.Select(write => write.Address)
                .SequenceEqual(new uint[]
                {
                    0x01EDADD0, 0x01EDADD0, 0, 0
                })
            && sameAddressWrites.Writes.Select(write => write.Value)
                .SequenceEqual(new uint[] { 4, 0xFFFFFFFE, 5, 4 })
            && sameAddressWrites.JumpTargetRawFileOffset == 0x012C9441
            && sameAddressFresh.Admitted);

        CapacityFinalCalleeAlternateTagWritesLifecyclePlan zeroCaptures =
            CapacityFinalCalleeAlternateTagWritesLifecycleProjection.Project(
                Capture(entryEdx: 0xFFFFFFFC, gateFresh: 0));
        CapacityFinalCalleeAlternateTagWritesLifecyclePlan missingTagValue =
            CapacityFinalCalleeAlternateTagWritesLifecycleProjection.Project(
                Capture(entryEdx: 0xFFFFFFFC, gateFresh: null));
        CapacityFinalCalleeAlternateTagWritesLifecyclePlan zeroTagThree =
            CapacityFinalCalleeAlternateTagWritesLifecycleProjection.Project(
                Capture(
                    entryEdx: 0xFFFFFFFC,
                    gateFresh: 3,
                    entryEdi: 4,
                    downstreamFresh: 0,
                    entryEsi: 0xFFFFFFFC));
        CapacityFinalCalleeAlternateFieldWritesPlan? zeroWrites =
            zeroCaptures.AlternateFieldWrites;
        CapacityFinalCalleeAlternateFieldWritesPlan? wrappedZeroWrites =
            zeroTagThree.AlternateFieldWrites;
        check("Captured zero values differ from missing inputs and DWORD address arithmetic wraps",
            zeroCaptures.Outcome
                == CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                    .MaskedTagNotThreeTo8EF8
            && zeroCaptures.TagGate.FreshDWORDCaptured
            && zeroCaptures.TagGate.FreshDWORD == 0
            && zeroCaptures.TagGate.ByteCompareFlags is
            {
                Carry: true,
                Parity: false,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && missingTagValue.TagGate.Outcome
                == CapacityFinalCalleeAlternateTagGateOutcome.MissingFreshDWORD
            && !missingTagValue.TagGate.FreshDWORDCaptured
            && !missingTagValue.TagGate.ExactCleanExitAdmitted
            && zeroTagThree.TagGate.FreshDWORDAddress == 0
            && zeroTagThree.TagGate.FreshDWORDCaptured
            && wrappedZeroWrites is not null
            && wrappedZeroWrites.FreshEdxPlus4Address == 0
            && wrappedZeroWrites.FreshEdxPlus4Captured
            && wrappedZeroWrites.FreshEdxPlus4 == 0
            && wrappedZeroWrites.AndResult == 0
            && wrappedZeroWrites.AndFlags is
            {
                Carry: false,
                Parity: true,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && wrappedZeroWrites.EAXAfterOr == 5
            && wrappedZeroWrites.EsiPlus4WriteAddress == 0
            && wrappedZeroWrites.EsiPlusEdiWriteAddress == 0
            && wrappedZeroWrites.JumpReached
            && wrappedZeroWrites.JumpTargetRawFileOffset == 0x012C9441
            && zeroWrites is null
            && zeroTagThree.Admitted);
    }

    private static CapacityFinalCalleeAlternateTagWritesLifecycleCapture Capture(
        uint? entryEdx,
        uint? gateFresh,
        uint? entryEdi = null,
        uint? downstreamFresh = null,
        uint? entryEsi = null) =>
        new(
            new CapacityFinalCalleeAlternateTagGateCapture(entryEdx, gateFresh),
            entryEdi,
            downstreamFresh,
            entryEsi);

    private static CapacityFinalCalleeAlternateTagGateCompareFlags CompareFlags(
        byte left,
        byte right)
    {
        byte result = unchecked((byte)(left - right));
        return new(
            left < right,
            (BitOperations.PopCount((uint)result) & 1) == 0,
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80) != 0,
            ((left ^ right) & (left ^ result) & 0x80) != 0);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
