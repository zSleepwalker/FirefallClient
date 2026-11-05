using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityRepeatFillJoinChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffCapacityRepeatFillJoinPlan forward =
            FrameHandoffCapacityRepeatFillJoinProjection.Project(NonzeroCapture(
                entryEbp: 0x2000,
                repeatFrameMinus8: 0x87654321,
                fillJoinEntryEdi: null,
                frameMinus4: 0xA1B2C3D4,
                directionFlagSet: false));
        check("A complete nonzero REP route derives fill EDI from its fresh frame-minus-eight reload",
            forward.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome.NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
            && forward.GateRepeatAdmissionValidated
            && forward.NonzeroRepeatAdmission
            && !forward.ZeroBranchBypassAdmission
            && forward.FillJoinAdmissionValidated
            && forward.DerivedFillJoinEntryEbp == 0x2000
            && forward.FillJoinEntryEbpMatches == true
            && forward.DerivedFillJoinEntryEdi == 0x87654321
            && forward.FillJoinPlan?.Capture.EntryEdi == 0x87654321
            && forward.FillJoinPlan?.Capture.FrameMinus4 == 0xA1B2C3D4
            && forward.FillJoinPlan?.EcxAfterMove == 0x87654321
            && forward.FillJoinPlan?.EdiAfterMove == 0xA1B2C3D4
            && forward.FillJoinPlan?.Outcome
                == FrameHandoffCapacityFillJoinOutcome.NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
            && forward.ExactFillJoinOutcome
            && forward.NoUnreachedFillJoinCaptures);

        FrameHandoffCapacityRepeatFillJoinPlan backward =
            FrameHandoffCapacityRepeatFillJoinProjection.Project(NonzeroCapture(
                entryEbp: 0x2000,
                repeatFrameMinus8: 0,
                fillJoinEntryEdi: 0,
                frameMinus4: 0,
                directionFlagSet: true));
        check("REP direction is preserved without imposing a fill-join direction assumption",
            backward.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome.ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            && backward.GateRepeatPlan.RepeatPlan?.Capture.DirectionFlagSet == true
            && backward.GateRepeatAdmissionValidated
            && backward.NonzeroRepeatAdmission
            && backward.DerivedFillJoinEntryEdi == 0
            && backward.ProvidedFillJoinEntryEdiMatches == true
            && backward.FillJoinPlan?.Outcome
                == FrameHandoffCapacityFillJoinOutcome.ZeroEntryEdiBranchToRawFileOffsetBE6CA2
            && backward.FillJoinPlan?.BranchTargetRawFileOffset
                == FrameHandoffCapacityFillJoinProjection.ZeroBranchTargetRawFileOffset
            && backward.ExactFillJoinOutcome
            && backward.NoUnreachedFillJoinCaptures);

        FrameHandoffCapacityRepeatFillJoinPlan zeroBypass =
            FrameHandoffCapacityRepeatFillJoinProjection.Project(new(
                GateRepeatCapture: new(
                    Gate(frameMinusC: 0, entryEbp: 0x3000),
                    RepeatEntryEcx: null,
                    RepeatEntryEdx: null,
                    RepeatEntryEbp: null,
                    DirectionFlagSet: null,
                    RepReturnedNormally: null,
                    FreshFrameMinus8Value: null),
                FillJoinEntryEbp: 0x3000,
                FillJoinEntryEdi: 0x4000,
                FrameMinus4: 0x5000));
        check("A clean zero REP bypass accepts an independent fill EDI and reaches the nonzero fill route",
            zeroBypass.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome.NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
            && zeroBypass.GateRepeatPlan.Outcome
                == FrameHandoffCapacityGateRepeatOutcome.ZeroBranchBypassedRepeat
            && zeroBypass.ZeroBranchBypassAdmission
            && !zeroBypass.NonzeroRepeatAdmission
            && zeroBypass.DerivedFillJoinEntryEdi == 0x4000
            && zeroBypass.FillJoinPlan?.Capture.EntryEdi == 0x4000
            && zeroBypass.FillJoinPlan?.EdiAfterMove == 0x5000
            && zeroBypass.ExactFillJoinOutcome
            && zeroBypass.NoUnreachedFillJoinCaptures);

        FrameHandoffCapacityRepeatFillJoinPlan missingZeroEdi =
            FrameHandoffCapacityRepeatFillJoinProjection.Project(new(
                GateRepeatCapture: new(
                    Gate(frameMinusC: 0),
                    null, null, null, null, null, null),
                FillJoinEntryEbp: 0x2000,
                FillJoinEntryEdi: null,
                FrameMinus4: 0xBEEF));
        check("The zero bypass does not invent an EDI when its independent fill capture is absent",
            missingZeroEdi.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome.MissingZeroBranchEntryEdi
            && missingZeroEdi.ZeroBranchBypassAdmission
            && missingZeroEdi.DerivedFillJoinEntryEdi is null
            && missingZeroEdi.FillJoinPlan is null
            && missingZeroEdi.UnreachedFillJoinFrameMinus4ReadCaptureProvided
            && missingZeroEdi.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityRepeatFillJoinProjection.FillJoinEntryRawFileOffset);

        FrameHandoffCapacityRepeatFillJoinPlan nonzeroEdiMismatch =
            FrameHandoffCapacityRepeatFillJoinProjection.Project(NonzeroCapture(
                entryEbp: 0x2000,
                repeatFrameMinus8: 0x87654321,
                fillJoinEntryEdi: 0x87654320,
                frameMinus4: 0x1000));
        check("A provided nonzero-route fill EDI that disagrees with the REP reload blocks before frame-minus-four",
            nonzeroEdiMismatch.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome.FillJoinEntryEdiMismatch
            && nonzeroEdiMismatch.GateRepeatAdmissionValidated
            && nonzeroEdiMismatch.NonzeroRepeatAdmission
            && nonzeroEdiMismatch.DerivedFillJoinEntryEdi == 0x87654321
            && nonzeroEdiMismatch.ProvidedFillJoinEntryEdiMatches == false
            && nonzeroEdiMismatch.FillJoinPlan is null
            && nonzeroEdiMismatch.UnreachedFillJoinFrameMinus4ReadCaptureProvided
            && nonzeroEdiMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityRepeatFillJoinProjection.FillJoinEntryRawFileOffset);

        FrameHandoffCapacityRepeatFillJoinPlan nonzeroEbpMismatch =
            FrameHandoffCapacityRepeatFillJoinProjection.Project(NonzeroCapture(
                entryEbp: 0x2000,
                fillJoinEntryEbp: 0x2004,
                repeatFrameMinus8: 0x1234,
                fillJoinEntryEdi: null,
                frameMinus4: 0x5678));
        FrameHandoffCapacityRepeatFillJoinPlan zeroEbpMismatch =
            FrameHandoffCapacityRepeatFillJoinProjection.Project(new(
                GateRepeatCapture: new(
                    Gate(frameMinusC: 0, entryEbp: 0x3000),
                    null, null, null, null, null, null),
                FillJoinEntryEbp: 0x3004,
                FillJoinEntryEdi: 0x7000,
                FrameMinus4: 0x8000));
        check("The gate-captured EBP identity is checked before the fresh fill read on both routes",
            nonzeroEbpMismatch.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome.FillJoinEntryEbpMismatch
            && nonzeroEbpMismatch.FillJoinEntryEbpMatches == false
            && nonzeroEbpMismatch.FillJoinPlan is null
            && nonzeroEbpMismatch.UnreachedFillJoinFrameMinus4ReadCaptureProvided
            && zeroEbpMismatch.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome.FillJoinEntryEbpMismatch
            && zeroEbpMismatch.ZeroBranchBypassAdmission
            && zeroEbpMismatch.FillJoinEntryEbpMatches == false
            && zeroEbpMismatch.FillJoinPlan is null
            && zeroEbpMismatch.UnreachedFillJoinFrameMinus4ReadCaptureProvided);

        FrameHandoffCapacityRepeatFillJoinPlan partialRepeat =
            FrameHandoffCapacityRepeatFillJoinProjection.Project(NonzeroCapture(
                entryEbp: 0x2000,
                repeatFrameMinus8: null,
                fillJoinEntryEdi: null,
                frameMinus4: 0xCAFE));
        check("A missing fresh REP reload preserves completed stores and flags the fill read as unreached",
            partialRepeat.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome.GateRepeatBlocked
            && partialRepeat.GateRepeatPlan.RepeatPlan?.Outcome
                == FrameHandoffCapacityRepeatStoreOutcome.MissingFreshFrameMinus8Capture
            && partialRepeat.GateRepeatPlan.RepeatPlan?.AllStoresCompleted == true
            && partialRepeat.GateRepeatPlan.RepeatPlan?.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(3, 0x3100, 4, 0)
            && partialRepeat.GateRepeatPlan.RepeatPlan?.EcxAfterRep == 0
            && partialRepeat.FillJoinPlan is null
            && partialRepeat.UnreachedFillJoinFrameMinus4ReadCaptureProvided
            && !partialRepeat.FillJoinAdmissionValidated);

        FrameHandoffCapacityRepeatFillJoinPlan wrapped =
            FrameHandoffCapacityRepeatFillJoinProjection.Project(NonzeroCapture(
                entryEbp: 3,
                repeatFrameMinus8: 0xFFFFFFFB,
                fillJoinEntryEdi: null,
                frameMinus4: uint.MaxValue));
        check("REP and fill-join frame effective addresses retain DWORD wraparound",
            wrapped.GateRepeatPlan.RepeatPlan?.FreshFrameMinus8ReadAddress == 0xFFFFFFFB
            && wrapped.FillJoinPlan?.FrameMinus4ReadAddress == 0xFFFFFFFF
            && wrapped.FillJoinPlan?.EdiAfterMove == uint.MaxValue
            && wrapped.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome.NonzeroEntryEdiJumpToRawFileOffsetBE6C9E
            && wrapped.ExactFillJoinOutcome);

        FrameHandoffCapacityRepeatFillJoinPlan missingFreshFrame4 =
            FrameHandoffCapacityRepeatFillJoinProjection.Project(NonzeroCapture(
                entryEbp: 0x2000,
                repeatFrameMinus8: 0x12345678,
                fillJoinEntryEdi: null,
                frameMinus4: null));
        check("A missing fresh frame-minus-four read blocks success after the complete REP reload",
            missingFreshFrame4.Outcome
                == FrameHandoffCapacityRepeatFillJoinOutcome.FillJoinBlocked
            && missingFreshFrame4.GateRepeatPlan.Outcome
                == FrameHandoffCapacityGateRepeatOutcome.ProjectionComplete
            && missingFreshFrame4.FillJoinPlan?.Outcome
                == FrameHandoffCapacityFillJoinOutcome.MissingFrameMinus4Read
            && missingFreshFrame4.FillJoinPlan?.FrameMinus4ReadReached == true
            && !missingFreshFrame4.ExactFillJoinOutcome
            && !missingFreshFrame4.UnreachedFillJoinFrameMinus4ReadCaptureProvided);
    }

    private static FrameHandoffCapacitySecondReturnGatePlan Gate(
        uint frameMinusC,
        uint entryEbp = 0x2000,
        uint entryEbx = 0x3000,
        uint helperReturnEax = 0x100) =>
        FrameHandoffCapacitySecondReturnGateProjection.Project(new(
            ValidatedNormalEntry: true,
            EntryEbp: entryEbp,
            EntryEbx: entryEbx,
            FrameMinusC: frameMinusC,
            HelperReturnEax: helperReturnEax));

    private static FrameHandoffCapacityRepeatFillJoinCapture NonzeroCapture(
        uint entryEbp,
        uint? repeatFrameMinus8,
        uint? fillJoinEntryEdi,
        uint? frameMinus4,
        bool directionFlagSet = false,
        uint? fillJoinEntryEbp = null) =>
        new(
            GateRepeatCapture: new(
                Gate(frameMinusC: 3, entryEbp: entryEbp),
                RepeatEntryEcx: 3,
                RepeatEntryEdx: 0x3100,
                RepeatEntryEbp: entryEbp,
                DirectionFlagSet: directionFlagSet,
                RepReturnedNormally: true,
                FreshFrameMinus8Value: repeatFrameMinus8),
            FillJoinEntryEbp: fillJoinEntryEbp ?? entryEbp,
            FillJoinEntryEdi: fillJoinEntryEdi,
            FrameMinus4: frameMinus4);
}
