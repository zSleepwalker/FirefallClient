using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateEpilogueLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeAlternateEpilogueLifecyclePlan cleanBypass =
            CapacityFinalCalleeAlternateEpilogueLifecycleProjection.Project(
                Capture(
                    gated: GatedCapture(
                        entryEsp: 0xFFFFFFFC,
                        entryEbp: 0x12345678,
                        testedByte: 0,
                        popEdi: 0xA1,
                        popEsi: 0xB2,
                        popEbx: 0xC3,
                        popEbp: 0xD4,
                        returnTarget: 0x87654321)));
        check("A clean JMP 9441 handoff admits the bypass only through the exact 945C return",
            cleanBypass.ExactJmp9441HandoffAdmitted
            && cleanBypass.GatedEpilogueReached
            && cleanBypass.AlternateFieldWrites.ExactCleanExitAdmitted
            && cleanBypass.AlternateFieldWrites.JumpTargetRawFileOffset
                == 0x012C9441
            && cleanBypass.GatedEpilogue is { } cleanBypassEpilogue
            && cleanBypassEpilogue.Capture.EntryValidated
            && cleanBypassEpilogue.Capture.EntryEsp == 0xFFFFFFFC
            && cleanBypassEpilogue.Capture.EntryEbp == 0x12345678
            && cleanBypassEpilogue.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            && cleanBypassEpilogue.ReturnReached
            && cleanBypassEpilogue.ExactCleanExitAdmitted
            && cleanBypass.ExactCleanExitAdmitted
            && cleanBypass.Outcome
                == CapacityFinalCalleeAlternateEpilogueLifecycleOutcome.CompletedAt945C
            && cleanBypass.StopAtRawFileOffsetExclusive == 0x012C945C
            && cleanBypass.OrderedUpstreamWrites.Length == 4
            && cleanBypass.OrderedUpstreamWrites.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C90B0, 0x012C90B8, 0x012C90BF, 0x012C90C2
                })
            && cleanBypass.EpilogueStackAccesses.Select(access => access.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEdi,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEsi,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEbx,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEbp,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.ReturnTarget
                }));

        CapacityFinalCalleeAlternateEpilogueLifecyclePlan cleanCallReturn =
            CapacityFinalCalleeAlternateEpilogueLifecycleProjection.Project(
                Capture(
                    gated: GatedCapture(
                        entryEsp: 0x11111111,
                        entryEbp: 0x22222222,
                        testedByte: 2,
                        popEdi: 1,
                        popEsi: 2,
                        popEbx: 3,
                        callTarget: 0,
                        callReturnedNormally: true,
                        postCallEbp: 0xFFFFFFFC,
                        popEbp: 4,
                        returnTarget: 5)));
        check("A normal call return retains ordered writes and push/pop effects through exact 945C",
            cleanCallReturn.ExactCleanExitAdmitted
            && cleanCallReturn.GatedEpilogue is { } cleanCallReturnEpilogue
            && cleanCallReturnEpilogue.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.ReturnedNormally
            && cleanCallReturnEpilogue.CallReturnedNormally == true
            && cleanCallReturnEpilogue.PushImmediateReached
            && cleanCallReturnEpilogue.PushImmediateAddress == 0x11111119
            && cleanCallReturnEpilogue.StackPopEbpAddress == 0xFFFFFFFC
            && cleanCallReturnEpilogue.ReturnTargetStackAddress == 0
            && cleanCallReturn.EpilogueStackAccesses.Select(access => access.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEdi,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEsi,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEbx,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PushImmediate,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.PopEbp,
                    CapacityFinalCalleeGatedEpilogueStackAccessKind.ReturnTarget
                })
            && cleanCallReturn.OrderedUpstreamWrites.Length == 4
            && cleanCallReturn.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeAlternateEpilogueLifecyclePlan upstreamMissing =
            CapacityFinalCalleeAlternateEpilogueLifecycleProjection.Project(
                Capture(
                    alternate: AlternateCapture(entryEdi: null),
                    gated: GatedCapture(
                        entryValidated: true,
                        testedByte: 0,
                        popEdi: 1,
                        popEsi: 2,
                        popEbx: 3,
                        callTarget: 4,
                        callReturnedNormally: true,
                        postCallEbp: 5,
                        popEbp: 6,
                        returnTarget: 7)));
        check("An upstream missing EDI cannot be bypassed by epilogue validation and flags all late captures",
            !upstreamMissing.GatedEpilogueReached
            && upstreamMissing.GatedEpilogue is null
            && !upstreamMissing.ExactJmp9441HandoffAdmitted
            && upstreamMissing.UnreachedEntryValidatedClaimProvided
            && upstreamMissing.UnreachedTestedByteCaptureProvided
            && upstreamMissing.UnreachedStackPopEdiCaptureProvided
            && upstreamMissing.UnreachedStackPopEsiCaptureProvided
            && upstreamMissing.UnreachedStackPopEbxCaptureProvided
            && upstreamMissing.UnreachedCallCellTargetCaptureProvided
            && upstreamMissing.UnreachedCallReturnStatusProvided
            && upstreamMissing.UnreachedPostCallEbpCaptureProvided
            && upstreamMissing.UnreachedStackPopEbpCaptureProvided
            && upstreamMissing.UnreachedReturnTargetCaptureProvided
            && upstreamMissing.AlternateFieldWrites.Writes.IsEmpty
            && upstreamMissing.StopAtRawFileOffsetExclusive == 0x012C90B0
            && !upstreamMissing.ExactCleanExitAdmitted);

        CapacityFinalCalleeAlternateEpilogueLifecyclePlan upstreamContaminated =
            CapacityFinalCalleeAlternateEpilogueLifecycleProjection.Project(
                Capture(
                    alternate: AlternateCapture(
                        entryEdi: 0x10,
                        entryEdx: null,
                        freshEdxPlus4: 0x20,
                        entryEsi: 0x30),
                    gated: GatedCapture(
                        testedByte: 0,
                        popEdi: 1,
                        popEsi: 2,
                        popEbx: null)));
        check("Contaminated upstream captures block the child while preserving prior writes",
            !upstreamContaminated.GatedEpilogueReached
            && upstreamContaminated.GatedEpilogue is null
            && upstreamContaminated.AlternateFieldWrites
                .UnreachedFreshEdxPlus4CaptureProvided
            && upstreamContaminated.AlternateFieldWrites
                .UnreachedEntryESICaptureProvided
            && upstreamContaminated.OrderedUpstreamWrites.Length == 1
            && upstreamContaminated.OrderedUpstreamWrites[0]
                == new CapacityFinalCalleeAlternateFieldWritesWrite(
                    0x012C90B0, 0x01EDADD0, 0x10)
            && upstreamContaminated.UnreachedTestedByteCaptureProvided
            && upstreamContaminated.UnreachedStackPopEdiCaptureProvided
            && upstreamContaminated.UnreachedStackPopEsiCaptureProvided
            && !upstreamContaminated.UnreachedStackPopEbxCaptureProvided
            && upstreamContaminated.Outcome
                == CapacityFinalCalleeAlternateEpilogueLifecycleOutcome
                    .AlternateFieldWritesIncompleteWithEpilogueCaptures
            && upstreamContaminated.StopAtRawFileOffsetExclusive == 0x012C90B8);

        CapacityFinalCalleeAlternateEpilogueLifecyclePlan missingTest =
            CapacityFinalCalleeAlternateEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    testedByte: null,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 4,
                    callReturnedNormally: true,
                    postCallEbp: 5,
                    popEbp: 6,
                    returnTarget: 7)));
        CapacityFinalCalleeAlternateEpilogueLifecyclePlan missingPop =
            CapacityFinalCalleeAlternateEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    testedByte: 2,
                    popEdi: 1,
                    popEsi: null,
                    popEbx: 3,
                    popEbp: 4,
                    returnTarget: 5)));
        check("Downstream missing TEST and POP captures retain upstream writes and prior stack effects",
            missingTest.AlternateFieldWrites.Writes.Length == 4
            && missingTest.GatedEpilogue is { } missingTestEpilogue
            && missingTestEpilogue.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingTestedByte
            && missingTestEpilogue.TestReadReached
            && missingTestEpilogue.UnreachedStackPopEdiCaptureProvided
            && missingTest.EpilogueStackAccesses.IsEmpty
            && missingTest.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingPop.AlternateFieldWrites.Writes.Length == 4
            && missingPop.GatedEpilogue is { } missingPopEpilogue
            && missingPopEpilogue.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEsiValue
            && missingPopEpilogue.PoppedEdi == 1
            && missingPop.EpilogueStackAccesses.Length == 2
            && missingPop.EpilogueStackAccesses[0].Address == 0x1000
            && missingPop.EpilogueStackAccesses[1].Address == 0x1004
            && missingPop.EpilogueStackAccesses[1].Value is null
            && missingPop.StopAtRawFileOffsetExclusive == 0x012C9449);

        CapacityFinalCalleeAlternateEpilogueLifecyclePlan missingCall =
            CapacityFinalCalleeAlternateEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    testedByte: 2,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: null,
                    callReturnedNormally: true,
                    postCallEbp: 5,
                    popEbp: 6,
                    returnTarget: 7)));
        CapacityFinalCalleeAlternateEpilogueLifecyclePlan missingPostCallFrame =
            CapacityFinalCalleeAlternateEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    entryEbp: 0xABCDEF01,
                    testedByte: 2,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 4,
                    callReturnedNormally: true,
                    postCallEbp: null,
                    popEbp: 5,
                    returnTarget: 6)));
        check("Missing call target and post-call frame preserve all upstream writes and reached stack effects",
            missingCall.AlternateFieldWrites.Writes.Length == 4
            && missingCall.GatedEpilogue is { } missingCallEpilogue
            && missingCallEpilogue.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingCallCellTarget
            && missingCallEpilogue.StackAccesses.Length == 4
            && missingCallEpilogue.PushImmediateReached
            && missingCallEpilogue.UnreachedCallReturnStatusProvided
            && missingCall.StopAtRawFileOffsetExclusive == 0x012C9452
            && missingPostCallFrame.AlternateFieldWrites.Writes.Length == 4
            && missingPostCallFrame.GatedEpilogue is { } missingPostCallFrameEpilogue
            && missingPostCallFrameEpilogue.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingPostCallEbp
            && missingPostCallFrameEpilogue.Capture.EntryEbp == 0xABCDEF01
            && missingPostCallFrameEpilogue.MovEspFromEbpReached
            && missingPostCallFrameEpilogue.MovEspSourceEbp is null
            && missingPostCallFrame.EpilogueStackAccesses.Length == 4
            && missingPostCallFrame.StopAtRawFileOffsetExclusive == 0x012C9458);

        CapacityFinalCalleeAlternateEpilogueLifecyclePlan nonreturning =
            CapacityFinalCalleeAlternateEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    testedByte: 2,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 4,
                    callReturnedNormally: false,
                    postCallEbp: 5,
                    popEbp: 6,
                    returnTarget: 7)));
        check("A nonreturning call retains all alternate writes and reached pushes/POPs but denies clean exit",
            nonreturning.AlternateFieldWrites.Writes.Length == 4
            && nonreturning.GatedEpilogue is { } nonreturningEpilogue
            && nonreturningEpilogue.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallDidNotReturnNormally
            && nonreturningEpilogue.StackAccesses.Length == 4
            && nonreturningEpilogue.CallReturnedNormally == false
            && nonreturningEpilogue.UnreachedPostCallEbpCaptureProvided
            && nonreturningEpilogue.UnreachedStackPopEbpCaptureProvided
            && nonreturning.OrderedUpstreamWrites.Length == 4
            && !nonreturning.ExactCleanExitAdmitted
            && nonreturning.StopAtRawFileOffsetExclusive == 0x012C9452);

        CapacityFinalCalleeAlternateEpilogueLifecyclePlan contaminatedEpilogue =
            CapacityFinalCalleeAlternateEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    testedByte: 0,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 4,
                    callReturnedNormally: true,
                    postCallEbp: 5,
                    popEbp: 6,
                    returnTarget: 7)));
        check("A bypass contaminated with unreached call captures is not an exact clean exit",
            contaminatedEpilogue.GatedEpilogue is { } contaminatedEpiloguePlan
            && contaminatedEpiloguePlan.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            && contaminatedEpiloguePlan.ReturnReached
            && contaminatedEpiloguePlan.UnreachedCallCellTargetCaptureProvided
            && contaminatedEpiloguePlan.UnreachedCallReturnStatusProvided
            && contaminatedEpilogue.UnreachedCallCellTargetCaptureProvided
            && contaminatedEpilogue.UnreachedCallReturnStatusProvided
            && !contaminatedEpilogue.ExactCleanExitAdmitted
            && contaminatedEpilogue.Outcome
                == CapacityFinalCalleeAlternateEpilogueLifecycleOutcome.EpilogueContaminated
            && contaminatedEpilogue.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeAlternateEpilogueLifecyclePlan wrap =
            CapacityFinalCalleeAlternateEpilogueLifecycleProjection.Project(
                Capture(
                    alternate: AlternateCapture(
                        entryEdi: 4,
                        entryEdx: 0xFFFFFFFC,
                        freshEdxPlus4: 0,
                        entryEsi: 0xFFFFFFFC),
                    gated: GatedCapture(
                        entryEsp: 0xFFFFFFFC,
                        entryEbp: 0xFFFFFFFC,
                        testedByte: 0,
                        popEdi: 1,
                        popEsi: 2,
                        popEbx: 3,
                        popEbp: 4,
                        returnTarget: 5)));
        check("Wrapped upstream field addresses and independent frame stack arithmetic are preserved",
            wrap.ExactJmp9441HandoffAdmitted
            && wrap.OrderedUpstreamWrites.Select(write => write.Address)
                .SequenceEqual(new uint[] { 0x01EDADD0, 0, 0, 0 })
            && wrap.OrderedUpstreamWrites.Select(write => write.Value)
                .SequenceEqual(new uint[] { 4, 0, 5, 4 })
            && wrap.GatedEpilogue is { } wrapEpilogue
            && wrapEpilogue.Capture.EntryEsp == 0xFFFFFFFC
            && wrapEpilogue.Capture.EntryEbp == 0xFFFFFFFC
            && wrapEpilogue.StackPopEsiAddress == 0
            && wrapEpilogue.StackPopEbxAddress == 4
            && wrapEpilogue.StackPopEbpAddress == 0xFFFFFFFC
            && wrapEpilogue.ReturnTargetStackAddress == 0
            && wrap.ExactCleanExitAdmitted
            && wrap.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeAlternateEpilogueLifecyclePlan invalidEpilogueEntry =
            CapacityFinalCalleeAlternateEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    entryValidated: false,
                    testedByte: 0,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3)));
        check("A clean write handoff still respects the epilogue's own entry validation",
            invalidEpilogueEntry.ExactJmp9441HandoffAdmitted
            && invalidEpilogueEntry.GatedEpilogueReached
            && invalidEpilogueEntry.GatedEpilogue is { } invalidEpiloguePlan
            && invalidEpiloguePlan.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.EntryNotValidated
            && invalidEpilogueEntry.UnreachedTestedByteCaptureProvided
            && invalidEpilogueEntry.UnreachedStackPopEdiCaptureProvided
            && invalidEpilogueEntry.UnreachedStackPopEsiCaptureProvided
            && invalidEpilogueEntry.UnreachedStackPopEbxCaptureProvided
            && !invalidEpilogueEntry.ExactCleanExitAdmitted
            && invalidEpilogueEntry.StopAtRawFileOffsetExclusive == 0x012C9441);
    }

    private static CapacityFinalCalleeAlternateEpilogueLifecycleCapture Capture(
        CapacityFinalCalleeAlternateFieldWritesCapture? alternate = null,
        CapacityFinalCalleeGatedEpilogueCapture? gated = null) =>
        new(
            alternate ?? AlternateCapture(),
            gated ?? GatedCapture());

    private static CapacityFinalCalleeAlternateFieldWritesCapture AlternateCapture(
        uint? entryEdi = 0x10,
        uint? entryEdx = 0x20,
        uint? freshEdxPlus4 = 0x30,
        uint? entryEsi = 0x40) =>
        new(entryEdi, entryEdx, freshEdxPlus4, entryEsi);

    private static CapacityFinalCalleeGatedEpilogueCapture GatedCapture(
        bool entryValidated = true,
        uint entryEsp = 0x1000,
        uint entryEbp = 0x4000,
        byte? testedByte = 0,
        uint? popEdi = 0x1111,
        uint? popEsi = 0x2222,
        uint? popEbx = 0x3333,
        uint? callTarget = null,
        bool? callReturnedNormally = null,
        uint? postCallEbp = null,
        uint? popEbp = 0x4444,
        uint? returnTarget = 0x5555) =>
        new(
            entryValidated,
            entryEsp,
            entryEbp,
            testedByte,
            popEdi,
            popEsi,
            popEbx,
            callTarget,
            callReturnedNormally,
            postCallEbp,
            popEbp,
            returnTarget);
}
