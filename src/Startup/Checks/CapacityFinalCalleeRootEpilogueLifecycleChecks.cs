using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeRootEpilogueLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition begins at the root gate, not the downstream root-write window",
            CapacityFinalCalleeRootEpilogueLifecycleProjection
                .AlternateRootEntryRawFileOffset
                == CapacityFinalCalleeAlternateRootGateProjection.EntryRawFileOffset
            && CapacityFinalCalleeRootEpilogueLifecycleProjection
                .AlternateRootEntryRawFileOffset == 0x012C90CA);

        CapacityFinalCalleeRootEpilogueLifecyclePlan cleanBypass =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    entryEsp: 0xFFFFFFFC,
                    entryEbp: 0x12345678,
                    testedByte: 0,
                    popEdi: 0xA1,
                    popEsi: 0xB2,
                    popEbx: 0xC3,
                    popEbp: 0xD4,
                    returnTarget: 0x87654321)));
        check("The clean equal root route hands off only at JMP 9441 and returns exactly at 945C",
            cleanBypass.AlternateRoot.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.Completed
            && cleanBypass.AlternateRoot.ExactCleanExitAdmitted
            && cleanBypass.AlternateRoot.FinalJumpTargetRawFileOffset == 0x012C9441
            && cleanBypass.AlternateRoot.FinalJumpTargetVirtualAddress == 0x016CA041
            && cleanBypass.AlternateRoot.StopAtRawFileOffsetExclusive == 0x012C9441
            && cleanBypass.ExactJmp9441HandoffAdmitted
            && cleanBypass.GatedEpilogueReached
            && cleanBypass.GatedEpilogue is { } bypassPlan
            && bypassPlan.Capture.EntryEsp == 0xFFFFFFFC
            && bypassPlan.Capture.EntryEbp == 0x12345678
            && bypassPlan.Outcome == CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            && bypassPlan.StackPopEsiAddress == 0
            && bypassPlan.StackPopEbxAddress == 4
            && bypassPlan.StackPopEbpAddress == 0x12345678
            && bypassPlan.ReturnTargetStackAddress == 0x1234567C
            && bypassPlan.ReturnReached
            && bypassPlan.ExactCleanExitAdmitted
            && cleanBypass.Outcome
                == CapacityFinalCalleeRootEpilogueLifecycleOutcome.CompletedAt945C
            && cleanBypass.ExactCleanExitAdmitted
            && cleanBypass.StopAtRawFileOffsetExclusive == 0x012C945C
            && cleanBypass.OrderedUpstreamWrites.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C90DA, 0x012C90E2, 0x012C90EB, 0x012C90EE
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

        CapacityFinalCalleeRootEpilogueLifecyclePlan cleanCall =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    entryEsp: 0xFFFFFFFC,
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
        check("The normal call route retains independent post-call EBP and wrapped stack effects",
            cleanCall.GatedEpilogue is { } callPlan
            && callPlan.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.ReturnedNormally
            && callPlan.Capture.EntryEbp == 0x22222222
            && callPlan.PushImmediateAddress == 4
            && callPlan.CallReturnedNormally == true
            && callPlan.PostCallEbp == 0xFFFFFFFC
            && callPlan.StackPopEbpAddress == 0xFFFFFFFC
            && callPlan.ReturnTargetStackAddress == 0
            && cleanCall.OrderedUpstreamWrites.Length == 4
            && cleanCall.ExactCleanExitAdmitted
            && cleanCall.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeRootEpilogueLifecyclePlan cleanUnequal =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
                Capture(root: RootCapture(
                    gateEntryEdx: 4,
                    gateFreshGlobal: 5,
                    downstreamFreshGlobal: null,
                    downstreamEdi: null,
                    downstreamEsi: null),
                    gated: GatedCapture(
                        entryValidated: false,
                        testedByte: null,
                        popEdi: null,
                        popEsi: null,
                        popEbx: null,
                        popEbp: null,
                        returnTarget: null)));
        check("A clean unequal root exit is exact at 90F6 without entering JMP 9441",
            cleanUnequal.AlternateRoot.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.GateUnequalToAlternateTarget
            && cleanUnequal.AlternateRoot.GateExactCleanAdmission
            && !cleanUnequal.AlternateRoot.GateEqualFallthroughSelected
            && !cleanUnequal.AlternateRoot.DownstreamWritesReached
            && cleanUnequal.AlternateRoot.ExactCleanExitAdmitted
            && cleanUnequal.AlternateRoot.FinalJumpTargetRawFileOffset is null
            && cleanUnequal.Outcome
                == CapacityFinalCalleeRootEpilogueLifecycleOutcome.RootGateUnequalAt90F6
            && !cleanUnequal.GatedEpilogueReached
            && !cleanUnequal.ExactJmp9441HandoffAdmitted
            && cleanUnequal.ExactCleanExitAdmitted
            && cleanUnequal.StopAtRawFileOffsetExclusive == 0x012C90F6);

        CapacityFinalCalleeRootEpilogueLifecyclePlan unequalWithEpilogueCaptures =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
                Capture(
                    root: RootCapture(gateEntryEdx: 4, gateFreshGlobal: 5,
                        downstreamFreshGlobal: null, downstreamEdi: null,
                        downstreamEsi: null),
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
        check("Unequal 90F6 rejects all later epilogue captures while retaining the clean route boundary",
            unequalWithEpilogueCaptures.Outcome
                == CapacityFinalCalleeRootEpilogueLifecycleOutcome
                    .RootGateUnequalAt90F6WithEpilogueCaptures
            && unequalWithEpilogueCaptures.AlternateRoot.ExactCleanExitAdmitted
            && !unequalWithEpilogueCaptures.GatedEpilogueReached
            && unequalWithEpilogueCaptures.GatedEpilogue is null
            && unequalWithEpilogueCaptures.UnreachedEntryValidatedClaimProvided
            && unequalWithEpilogueCaptures.UnreachedTestedByteCaptureProvided
            && unequalWithEpilogueCaptures.UnreachedStackPopEdiCaptureProvided
            && unequalWithEpilogueCaptures.UnreachedStackPopEsiCaptureProvided
            && unequalWithEpilogueCaptures.UnreachedStackPopEbxCaptureProvided
            && unequalWithEpilogueCaptures.UnreachedCallCellTargetCaptureProvided
            && unequalWithEpilogueCaptures.UnreachedCallReturnStatusProvided
            && unequalWithEpilogueCaptures.UnreachedPostCallEbpCaptureProvided
            && unequalWithEpilogueCaptures.UnreachedStackPopEbpCaptureProvided
            && unequalWithEpilogueCaptures.UnreachedReturnTargetCaptureProvided
            && !unequalWithEpilogueCaptures.ExactCleanExitAdmitted
            && unequalWithEpilogueCaptures.StopAtRawFileOffsetExclusive == 0x012C90F6);

        CapacityFinalCalleeRootEpilogueLifecyclePlan missingGate =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
                Capture(
                    root: RootCapture(
                        gateEntryEdx: null,
                        gateFreshGlobal: 7,
                        downstreamFreshGlobal: 8,
                        downstreamEdi: 9,
                        downstreamEsi: 10),
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
        check("A missing upstream gate read preserves its stop and rejects downstream root and epilogue captures",
            missingGate.Outcome
                == CapacityFinalCalleeRootEpilogueLifecycleOutcome
                    .RootLifecycleIncompleteWithEpilogueCaptures
            && missingGate.AlternateRoot.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.MissingGateEntryEDX
            && missingGate.AlternateRoot.UnreachedDownstreamFreshGlobalCaptureProvided
            && missingGate.AlternateRoot.UnreachedDownstreamEntryEDICaptureProvided
            && missingGate.AlternateRoot.UnreachedDownstreamEntryESICaptureProvided
            && missingGate.OrderedUpstreamWrites.IsEmpty
            && !missingGate.GatedEpilogueReached
            && missingGate.UnreachedEntryValidatedClaimProvided
            && missingGate.UnreachedTestedByteCaptureProvided
            && missingGate.UnreachedReturnTargetCaptureProvided
            && !missingGate.ExactCleanExitAdmitted
            && missingGate.StopAtRawFileOffsetExclusive == 0x012C90CA);

        CapacityFinalCalleeRootEpilogueLifecyclePlan missingRootStoreCapture =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
                Capture(
                    root: RootCapture(
                        gateEntryEdx: 7,
                        gateFreshGlobal: 7,
                        downstreamFreshGlobal: 0xFFFFFFFE,
                        downstreamEdi: 3,
                        downstreamEsi: null),
                    gated: GatedCapture(testedByte: 0)));
        check("A missing root store operand retains completed upstream gate/add evidence and blocks the epilogue",
            missingRootStoreCapture.AlternateRoot.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.MissingDownstreamEntryESI
            && missingRootStoreCapture.AlternateRoot.WritesPlan?.FreshGlobal == 0xFFFFFFFE
            && missingRootStoreCapture.AlternateRoot.WritesPlan?.AddResult == 1
            && missingRootStoreCapture.AlternateRoot.Writes.IsEmpty
            && !missingRootStoreCapture.GatedEpilogueReached
            && missingRootStoreCapture.UnreachedTestedByteCaptureProvided
            && !missingRootStoreCapture.ExactCleanExitAdmitted
            && missingRootStoreCapture.StopAtRawFileOffsetExclusive == 0x012C90DA);

        CapacityFinalCalleeRootEpilogueLifecyclePlan missingByte =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
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
        CapacityFinalCalleeRootEpilogueLifecyclePlan missingPop =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    entryEsp: 0xFFFFFFFC,
                    testedByte: 2,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: null,
                    popEbp: 4,
                    returnTarget: 5)));
        check("Missing TEST and POP values preserve root writes and all reached stack effects",
            missingByte.OrderedUpstreamWrites.Length == 4
            && missingByte.GatedEpilogue is { } bytePlan
            && bytePlan.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingTestedByte
            && bytePlan.TestReadReached
            && bytePlan.StackAccesses.IsEmpty
            && bytePlan.UnreachedStackPopEdiCaptureProvided
            && missingByte.StopAtRawFileOffsetExclusive == 0x012C9441
            && missingPop.OrderedUpstreamWrites.Length == 4
            && missingPop.GatedEpilogue is { } popPlan
            && popPlan.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbxValue
            && popPlan.PoppedEdi == 1
            && popPlan.PoppedEsi == 2
            && popPlan.StackAccesses.Select(access => access.Address)
                .SequenceEqual(new uint[] { 0xFFFFFFFC, 0, 4 })
            && popPlan.StackAccesses[2].Value is null
            && missingPop.EpilogueStackAccesses.Length == 3
            && missingPop.StopAtRawFileOffsetExclusive == 0x012C944A);

        CapacityFinalCalleeRootEpilogueLifecyclePlan missingCall =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
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
        CapacityFinalCalleeRootEpilogueLifecyclePlan nonreturningCall =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
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
        CapacityFinalCalleeRootEpilogueLifecyclePlan missingPostCallEbp =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    testedByte: 2,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    callTarget: 4,
                    callReturnedNormally: true,
                    postCallEbp: null,
                    popEbp: 6,
                    returnTarget: 7)));
        check("Missing or nonreturning CALL and missing post-call EBP preserve root writes and stop at their instruction",
            missingCall.OrderedUpstreamWrites.Length == 4
            && missingCall.GatedEpilogue is { } missingCallPlan
            && missingCallPlan.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingCallCellTarget
            && missingCallPlan.PushImmediateReached
            && missingCallPlan.StackAccesses.Length == 4
            && missingCall.StopAtRawFileOffsetExclusive == 0x012C9452
            && nonreturningCall.GatedEpilogue is { } nonreturningPlan
            && nonreturningPlan.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallDidNotReturnNormally
            && nonreturningPlan.StackAccesses.Length == 4
            && nonreturningPlan.UnreachedPostCallEbpCaptureProvided
            && !nonreturningCall.ExactCleanExitAdmitted
            && nonreturningCall.StopAtRawFileOffsetExclusive == 0x012C9452
            && missingPostCallEbp.GatedEpilogue is { } postCallPlan
            && postCallPlan.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingPostCallEbp
            && postCallPlan.Capture.EntryEbp == 0x4000
            && postCallPlan.MovEspFromEbpReached
            && postCallPlan.MovEspSourceEbp is null
            && missingPostCallEbp.EpilogueStackAccesses.Length == 4
            && missingPostCallEbp.StopAtRawFileOffsetExclusive == 0x012C9458);

        CapacityFinalCalleeRootEpilogueLifecyclePlan missingPopEbp =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    testedByte: 0,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    popEbp: null,
                    returnTarget: 5)));
        CapacityFinalCalleeRootEpilogueLifecyclePlan missingReturnTarget =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
                Capture(gated: GatedCapture(
                    testedByte: 0,
                    popEdi: 1,
                    popEsi: 2,
                    popEbx: 3,
                    popEbp: 4,
                    returnTarget: null)));
        check("Missing final POP EBP and RET target preserve earlier stack accesses but deny exact completion",
            missingPopEbp.OrderedUpstreamWrites.Length == 4
            && missingPopEbp.GatedEpilogue is { } popEbpPlan
            && popEbpPlan.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbpValue
            && popEbpPlan.StackPopEbpReached
            && popEbpPlan.ReturnTargetReadReached == false
            && popEbpPlan.StackAccesses.Length == 4
            && missingPopEbp.StopAtRawFileOffsetExclusive == 0x012C945A
            && missingReturnTarget.GatedEpilogue is { } returnPlan
            && returnPlan.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.MissingReturnTarget
            && returnPlan.PoppedEbp == 4
            && returnPlan.ReturnTargetReadReached
            && returnPlan.StackAccesses.Length == 5
            && returnPlan.StackAccesses[4].Value is null
            && !missingReturnTarget.ExactCleanExitAdmitted
            && missingReturnTarget.StopAtRawFileOffsetExclusive == 0x012C945B);

        CapacityFinalCalleeRootEpilogueLifecyclePlan contaminatedBypass =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
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
        check("A returned bypass with unreached call captures is contaminated, not a clean 945C exit",
            contaminatedBypass.GatedEpilogue is { } contaminatedPlan
            && contaminatedPlan.Outcome
                == CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            && contaminatedPlan.ReturnReached
            && contaminatedPlan.UnreachedCallCellTargetCaptureProvided
            && contaminatedPlan.UnreachedCallReturnStatusProvided
            && contaminatedBypass.Outcome
                == CapacityFinalCalleeRootEpilogueLifecycleOutcome.EpilogueContaminated
            && !contaminatedBypass.ExactCleanExitAdmitted
            && contaminatedBypass.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeRootEpilogueLifecyclePlan wrappedAliasedWrites =
            CapacityFinalCalleeRootEpilogueLifecycleProjection.Project(
                Capture(
                    root: RootCapture(
                        gateEntryEdx: 0x80000000,
                        gateFreshGlobal: 0x80000000,
                        downstreamFreshGlobal: 1,
                        downstreamEdi: 3,
                        downstreamEsi: 0xFFFFFFFC),
                    gated: GatedCapture(
                        entryEsp: 0xFFFFFFFC,
                        entryEbp: 0xFFFFFFFC,
                        testedByte: 0,
                        popEdi: 1,
                        popEsi: 2,
                        popEbx: 3,
                        popEbp: 4,
                        returnTarget: 5)));
        check("Wrapped root address arithmetic preserves ordered aliased writes and independent wrapped frames",
            wrappedAliasedWrites.AlternateRoot.Outcome
                == CapacityFinalCalleeAlternateRootLifecycleOutcome.Completed
            && wrappedAliasedWrites.OrderedUpstreamWrites.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x012C90DA, 0x012C90E2, 0x012C90EB, 0x012C90EE
                })
            && wrappedAliasedWrites.OrderedUpstreamWrites.Select(write => write.Address)
                .SequenceEqual(new uint[] { 0x01EDADDC, 0x01EDADD0, 0, 0 })
            && wrappedAliasedWrites.OrderedUpstreamWrites.Select(write => write.Value)
                .SequenceEqual(new uint[] { 0xFFFFFFFC, 4, 5, 4 })
            && wrappedAliasedWrites.OrderedUpstreamWrites[2].Address
                == wrappedAliasedWrites.OrderedUpstreamWrites[3].Address
            && wrappedAliasedWrites.GatedEpilogue is { } wrappedPlan
            && wrappedPlan.Capture.EntryEsp == 0xFFFFFFFC
            && wrappedPlan.Capture.EntryEbp == 0xFFFFFFFC
            && wrappedPlan.StackPopEsiAddress == 0
            && wrappedPlan.StackPopEbxAddress == 4
            && wrappedPlan.StackPopEbpAddress == 0xFFFFFFFC
            && wrappedPlan.ReturnTargetStackAddress == 0
            && wrappedAliasedWrites.ExactCleanExitAdmitted
            && wrappedAliasedWrites.StopAtRawFileOffsetExclusive == 0x012C945C);
    }

    private static CapacityFinalCalleeRootEpilogueLifecycleCapture Capture(
        CapacityFinalCalleeAlternateRootLifecycleCapture? root = null,
        CapacityFinalCalleeGatedEpilogueCapture? gated = null) =>
        new(root ?? RootCapture(), gated ?? GatedCapture());

    private static CapacityFinalCalleeAlternateRootLifecycleCapture RootCapture(
        uint? gateEntryEdx = 7,
        uint? gateFreshGlobal = 7,
        uint? downstreamFreshGlobal = 0x10,
        uint? downstreamEdi = 0,
        uint? downstreamEsi = 0) =>
        new(
            gateEntryEdx,
            gateFreshGlobal,
            downstreamFreshGlobal,
            downstreamEdi,
            downstreamEsi);

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
