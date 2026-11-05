using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveZeroForwardReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed byte window joins the zero-result handoff to the return tail",
            UnreliableReceiveZeroForwardReturnProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveZeroForwardReturnProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveZeroForwardReturnProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveZeroForwardReturnProjection.StartRawFileOffset == 0x00BE5B78
            && UnreliableReceiveZeroForwardReturnProjection.StartVirtualAddress == 0x00FE6778
            && UnreliableReceiveZeroForwardReturnProjection.EndRawFileOffsetExclusive == 0x00BE5BAC
            && UnreliableReceiveZeroForwardReturnProjection.EndVirtualAddressExclusive == 0x00FE67AC
            && UnreliableReceiveZeroForwardReturnProjection.RawFileOffsetToVirtualAddress(0x00BE5B78)
                == 0x00FE6778
            && UnreliableReceiveZeroForwardReturnProjection.RawFileOffsetToVirtualAddress(0x00BE5BAC)
                == 0x00FE67AC
            && UnreliableReceiveZeroForwardReturnProjection.CapturedBytes
                == UnreliableReceiveZeroResultProjection.CapturedHandoffBytes
                    + " "
                    + UnreliableReceiveZeroReturnProjection.CapturedTailBytes
            && UnreliableReceiveZeroForwardReturnProjection.CapturedBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x34);

        check("Only the exact returned zero-result boundary admits the tail",
            VerifyExactTailAdmission());

        check("Zero ESI and independent helper EAX preserve the tail's AL-only result",
            VerifyNativeZeroAndIndependentEax());

        check("Missing tail gates reject EAX and report unreached captures",
            VerifyMissingTailGates());

        check("Missing or nonreturning first helpers leave provided tail captures unreached",
            VerifyUnreachedTailCaptures());

        check("The nonzero ESI route stops at the branch target without entering the tail",
            VerifyNonzeroEsiRoute());

        check("Partial first-plan pushes survive failures before tail admission",
            VerifyPartialPushesOnFailure());
    }

    private static bool VerifyExactTailAdmission()
    {
        UnreliableReceiveZeroForwardReturnPlan reached = Project(
            retainedEsi: 0,
            handoffReturnedNormally: true,
            priorForwardCallReturnedNormally: true,
            helperReturnEax: 0x12345678);
        UnreliableReceiveZeroForwardReturnPlan missingHandoffBoundary =
            ProjectWithZeroResult(
                new(
                    RetainedEsi: 0,
                    ReceiverEdi: 0x1000,
                    PriorRecycleHelperReturnedNormally: true,
                    ReceiverDwordAtOffset00: 0x2000,
                    ReceiverDwordAtOffset04: 0x3000,
                    LoadedSlotDwordAtOffset04: 0x4000,
                    ReceiverDwordAtOffset18: 0x5000,
                    HandoffHelperReturnedNormally: null),
                new(true, 0xAABBCCDD));
        UnreliableReceiveZeroForwardReturnPlan nonreturningHandoff =
            ProjectWithZeroResult(
                new(
                    RetainedEsi: 0,
                    ReceiverEdi: 0x1000,
                    PriorRecycleHelperReturnedNormally: true,
                    ReceiverDwordAtOffset00: 0x2000,
                    ReceiverDwordAtOffset04: 0x3000,
                    LoadedSlotDwordAtOffset04: 0x4000,
                    ReceiverDwordAtOffset18: 0x5000,
                    HandoffHelperReturnedNormally: false),
                new(true, 0xAABBCCDD));

        return reached.ZeroResultPlan.Outcome
                == UnreliableReceiveZeroResultOutcome.HandoffReturnedToExclusiveBoundary
            && reached.ZeroResultPlan.HandoffReturnBoundaryReached == true
            && reached.ZeroReturnTailAdmitted
            && reached.ZeroReturnPlan?.ReturnReached == true
            && reached.ZeroReturnPlan?.ReturnImmediateBytes == 4
            && reached.StopAtRawFileOffsetExclusive == 0x00BE5BAC
            && missingHandoffBoundary.ZeroResultPlan.StopAtRawFileOffsetExclusive == 0x00BE5B99
            && missingHandoffBoundary.ZeroResultPlan.HandoffReturnBoundaryReached is null
            && !missingHandoffBoundary.ZeroReturnTailAdmitted
            && missingHandoffBoundary.ZeroReturnPlan is null
            && missingHandoffBoundary.UnreachedTailCaptureProvided
            && nonreturningHandoff.ZeroResultPlan.Outcome
                == UnreliableReceiveZeroResultOutcome.HandoffHelperDidNotReturnNormally
            && nonreturningHandoff.ZeroResultPlan.HandoffReturnBoundaryReached == false
            && !nonreturningHandoff.ZeroReturnTailAdmitted
            && nonreturningHandoff.ZeroReturnPlan is null
            && nonreturningHandoff.UnreachedTailCaptureProvided;
    }

    private static bool VerifyNativeZeroAndIndependentEax()
    {
        UnreliableReceiveZeroForwardReturnPlan nativeZero = Project(
            retainedEsi: 0,
            handoffReturnedNormally: true,
            priorForwardCallReturnedNormally: true,
            helperReturnEax: null);
        UnreliableReceiveZeroForwardReturnPlan independentNonzeroEax = Project(
            retainedEsi: 0,
            handoffReturnedNormally: true,
            priorForwardCallReturnedNormally: true,
            helperReturnEax: 0xA1B2C3D7,
            selectedEcx: 0x55667788);

        return nativeZero.ZeroResultPlan.EsiIsZero == true
            && nativeZero.ZeroReturnTailAdmitted
            && nativeZero.ZeroReturnPlan?.AlAfterXor == 0
            && nativeZero.ZeroReturnPlan?.EaxAfterXorAl is null
            && independentNonzeroEax.ZeroResultPlan.EsiIsZero == true
            && independentNonzeroEax.ZeroResultPlan.SelectedEcxValue == 0x55667788
            && independentNonzeroEax.ZeroReturnPlan?.Capture.HelperReturnEax == 0xA1B2C3D7
            && independentNonzeroEax.ZeroReturnPlan?.AlAfterXor == 0
            && independentNonzeroEax.ZeroReturnPlan?.EaxAfterXorAl == 0xA1B2C300
            && independentNonzeroEax.StopAtRawFileOffsetExclusive == 0x00BE5BAC;
    }

    private static bool VerifyMissingTailGates()
    {
        UnreliableReceiveZeroForwardReturnPlan missingGate = Project(
            retainedEsi: 0,
            handoffReturnedNormally: true,
            priorForwardCallReturnedNormally: null,
            helperReturnEax: 0xA1B2C3D7);
        UnreliableReceiveZeroForwardReturnPlan falseGate = Project(
            retainedEsi: 0,
            handoffReturnedNormally: true,
            priorForwardCallReturnedNormally: false,
            helperReturnEax: 0xA1B2C3D7);

        return missingGate.ZeroReturnTailAdmitted
            && missingGate.Outcome
                == UnreliableReceiveZeroForwardReturnOutcome.MissingPriorForwardCallReturnCapture
            && missingGate.ZeroReturnPlan?.PriorForwardCallNormalGatePassed is null
            && missingGate.ZeroReturnPlan?.UnreachedHelperReturnEaxCaptureProvided == true
            && missingGate.ZeroReturnPlan?.AlAfterXor is null
            && missingGate.ZeroReturnPlan?.StopAtRawFileOffsetExclusive == 0x00BE5B9E
            && falseGate.ZeroReturnTailAdmitted
            && falseGate.Outcome
                == UnreliableReceiveZeroForwardReturnOutcome.PriorForwardCallDidNotReturnNormally
            && falseGate.ZeroReturnPlan?.PriorForwardCallNormalGatePassed == false
            && falseGate.ZeroReturnPlan?.UnreachedHelperReturnEaxCaptureProvided == true
            && falseGate.ZeroReturnPlan?.AlAfterXor is null
            && falseGate.ZeroReturnPlan?.StopAtRawFileOffsetExclusive == 0x00BE5B9E;
    }

    private static bool VerifyUnreachedTailCaptures()
    {
        UnreliableReceiveZeroForwardReturnPlan missingFirstGate =
            ProjectWithZeroResult(
                new(
                    RetainedEsi: 0,
                    ReceiverEdi: 0x1000,
                    PriorRecycleHelperReturnedNormally: null,
                    ReceiverDwordAtOffset00: 0x2000,
                    ReceiverDwordAtOffset04: 0x3000,
                    LoadedSlotDwordAtOffset04: 0x4000,
                    ReceiverDwordAtOffset18: 0x5000,
                    HandoffHelperReturnedNormally: true),
                new(true, 0xAABBCCDD));
        UnreliableReceiveZeroForwardReturnPlan failedFirstGate =
            ProjectWithZeroResult(
                new(
                    RetainedEsi: 0,
                    ReceiverEdi: 0x1000,
                    PriorRecycleHelperReturnedNormally: false,
                    ReceiverDwordAtOffset00: 0x2000,
                    ReceiverDwordAtOffset04: 0x3000,
                    LoadedSlotDwordAtOffset04: 0x4000,
                    ReceiverDwordAtOffset18: 0x5000,
                    HandoffHelperReturnedNormally: true),
                new(true, 0xAABBCCDD));

        return missingFirstGate.ZeroResultPlan.Outcome
                == UnreliableReceiveZeroResultOutcome.MissingPriorRecycleHelperReturnCapture
            && !missingFirstGate.ZeroReturnTailAdmitted
            && missingFirstGate.ZeroReturnPlan is null
            && missingFirstGate.UnreachedTailCaptureProvided
            && missingFirstGate.ZeroResultPlan.UnreachedHandoffReturnCaptureProvided
            && failedFirstGate.ZeroResultPlan.Outcome
                == UnreliableReceiveZeroResultOutcome.PriorRecycleHelperDidNotReturnNormally
            && !failedFirstGate.ZeroReturnTailAdmitted
            && failedFirstGate.ZeroReturnPlan is null
            && failedFirstGate.UnreachedTailCaptureProvided
            && failedFirstGate.ZeroResultPlan.UnreachedHandoffReturnCaptureProvided;
    }

    private static bool VerifyNonzeroEsiRoute()
    {
        UnreliableReceiveZeroForwardReturnPlan nonzeroEsi = Project(
            retainedEsi: 0x12345678,
            handoffReturnedNormally: true,
            priorForwardCallReturnedNormally: true,
            helperReturnEax: 0xA1B2C3D7);

        return nonzeroEsi.ZeroResultPlan.Outcome
                == UnreliableReceiveZeroResultOutcome.NonzeroEsiBranch
            && nonzeroEsi.ZeroResultPlan.StopAtRawFileOffsetExclusive == 0x00BE5BAC
            && nonzeroEsi.ZeroResultPlan.SelectedDestinationRawFileOffset == 0x00BE5BAC
            && !nonzeroEsi.ZeroReturnTailAdmitted
            && nonzeroEsi.ZeroReturnPlan is null
            && nonzeroEsi.UnreachedTailCaptureProvided
            && nonzeroEsi.StopAtRawFileOffsetExclusive == 0x00BE5BAC;
    }

    private static bool VerifyPartialPushesOnFailure()
    {
        UnreliableReceiveZeroForwardReturnPlan partial = ProjectWithZeroResult(
            new(
                RetainedEsi: 0,
                ReceiverEdi: 0x1000,
                PriorRecycleHelperReturnedNormally: true,
                ReceiverDwordAtOffset00: 0x2000,
                ReceiverDwordAtOffset04: 0x3000,
                LoadedSlotDwordAtOffset04: null,
                ReceiverDwordAtOffset18: 0x5000,
                HandoffHelperReturnedNormally: true),
            new(true, 0xAABBCCDD));

        return partial.ZeroResultPlan.Outcome
                == UnreliableReceiveZeroResultOutcome
                    .MissingLoadedSlotOffset04CaptureWithUnreachedLaterCaptures
            && partial.ZeroResultPlan.ArgumentsPushedInInstructionOrder
                .SequenceEqual(new uint[] { 0x3000 })
            && !partial.ZeroReturnTailAdmitted
            && partial.ZeroReturnPlan is null
            && partial.UnreachedTailCaptureProvided
            && partial.StopAtRawFileOffsetExclusive == 0x00BE5B86;
    }

    private static UnreliableReceiveZeroForwardReturnPlan Project(
        uint retainedEsi,
        bool? handoffReturnedNormally,
        bool? priorForwardCallReturnedNormally,
        uint? helperReturnEax,
        uint selectedEcx = 0x55667788) =>
        ProjectWithZeroResult(
            new(
                RetainedEsi: retainedEsi,
                ReceiverEdi: 0x1000,
                PriorRecycleHelperReturnedNormally: true,
                ReceiverDwordAtOffset00: 0x2000,
                ReceiverDwordAtOffset04: 0x3000,
                LoadedSlotDwordAtOffset04: selectedEcx,
                ReceiverDwordAtOffset18: 0x5000,
                HandoffHelperReturnedNormally: handoffReturnedNormally),
            new(priorForwardCallReturnedNormally, helperReturnEax));

    private static UnreliableReceiveZeroForwardReturnPlan ProjectWithZeroResult(
        UnreliableReceiveZeroResultCapture zeroResult,
        UnreliableReceiveZeroReturnCapture zeroReturn) =>
        UnreliableReceiveZeroForwardReturnProjection.Project(
            new(zeroResult, zeroReturn));
}
