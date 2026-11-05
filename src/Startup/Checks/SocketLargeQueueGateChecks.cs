using Firefall.Reconstruction.Startup;

internal static class SocketLargeQueueGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Large-queue gate anchors pin the sample, VA mapping, independent read offsets, and route boundaries",
            SocketLargeQueueGateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && SocketLargeQueueGateProjection.EntryRawFileOffset == 0x00BD40BF
            && SocketLargeQueueGateProjection.EntryVirtualAddress == 0x00FD4CBF
            && SocketLargeQueueGateProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x00400C00
            && SocketLargeQueueGateProjection.DeferredControlRawFileOffset == 0x00BD4112
            && SocketLargeQueueGateProjection.HeaderDispatchRawFileOffset == 0x00BD412B
            && SocketLargeQueueGateProjection.HandoffRawFileOffset == 0x00BD4214
            && SocketLargeQueueGateProjection.DeferredControlVirtualAddress == 0x00FD4D12
            && SocketLargeQueueGateProjection.HeaderDispatchVirtualAddress == 0x00FD4D2B
            && SocketLargeQueueGateProjection.HandoffVirtualAddress == 0x00FD4E14
            && SocketLargeQueueGateProjection.ReceiverDword108Offset == 0x108
            && SocketLargeQueueGateProjection.ReceiverByteD95Offset == 0xD95
            && SocketLargeQueueGateProjection.PayloadMask == 0xC000
            && SocketLargeQueueGateProjection.DiagnosticCode == 0x4FA
            && SocketLargeQueueGateProjection.DiagnosticFirstPushedValue == 0
            && SocketLargeQueueGateProjection.DiagnosticFirstPushedPointer == 0x01B583AD
            && SocketLargeQueueGateProjection.DiagnosticSecondPushedPointer == 0x01B583AE);

        check("Gate instruction evidence pins the skip states, report call, byte branch, independent DWORD, and masked-word routes",
            SocketLargeQueueGateProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.BranchTargetRawFileOffset))
                .SequenceEqual(new (uint RawFileOffset, string Bytes, uint? BranchTargetRawFileOffset)[]
                {
                    (0x00BD40BF, "8B 86 08 01 00 00", null),
                    (0x00BD40C5, "83 F8 03 74 24", 0x00BD40EE),
                    (0x00BD40CA, "83 F8 05 74 1F", 0x00BD40EE),
                    (0x00BD40CF, "83 F8 08 74 1A", 0x00BD40EE),
                    (0x00BD40D4, "6A 00 68 FA 04 00 00 68 AD 83 B5 01 68 AE 83 B5 01", null),
                    (0x00BD40E5, "FF 15 A0 5E C1 01", null),
                    (0x00BD40EB, "83 C4 10", null),
                    (0x00BD40EE, "80 BE 95 0D 00 00 00 0F 85 19 01 00 00", 0x00BD4214),
                    (0x00BD40FB, "83 BE 08 01 00 00 03 75 27", 0x00BD412B),
                    (0x00BD4104, "B8 00 C0 00 00 66 85 07", null),
                    (0x00BD410C, "0F 85 02 01 00 00", 0x00BD4214)
                })
            && unchecked((int)0x00BD40C8 + 2 + 0x24) == 0x00BD40EE
            && unchecked((int)0x00BD40CD + 2 + 0x1F) == 0x00BD40EE
            && unchecked((int)0x00BD40D2 + 2 + 0x1A) == 0x00BD40EE
            && unchecked((int)0x00BD40F5 + 6 + 0x119) == 0x00BD4214
            && unchecked((int)0x00BD4102 + 2 + 0x27) == 0x00BD412B
            && unchecked((int)0x00BD410C + 6 + 0x102) == 0x00BD4214);

        SocketLargeQueueGatePlan missingInitial =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: null));
        check("A missing initial state is not substituted with a later capture",
            missingInitial.Outcome == SocketLargeQueueGateOutcome.MissingInitialState
            && missingInitial.LaterStateAddress is null
            && missingInitial.RouteRawFileOffset is null
            && missingInitial.StopAtRawFileOffsetExclusive == 0x00BD40BF);

        SocketLargeQueueGatePlan unreachableAfterMissingInitial =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: null, laterState: 3));
        check("A later state capture is rejected when the initial state read is missing",
            unreachableAfterMissingInitial.Outcome
                == SocketLargeQueueGateOutcome.UnreachedCaptureProvided
            && unreachableAfterMissingInitial.StopAtRawFileOffsetExclusive == 0x00BD40BF);

        foreach (uint skippedState in new uint[] { 3, 5, 8 })
        {
            SocketLargeQueueGatePlan skipped =
                SocketLargeQueueGateProjection.Project(
                    Capture(initialState: skippedState, gateByte: 1));
            check($"Initial state {skippedState} skips the diagnostic and a nonzero +0xD95 byte routes to handoff",
                skipped.Outcome == SocketLargeQueueGateOutcome.GateByteBranchesToHandoff
                && !skipped.DiagnosticCallReached
                && skipped.Capture.DiagnosticCallReturnedNormally is null
                && skipped.RouteRawFileOffset == 0x00BD4214
                && skipped.StopAtRawFileOffsetExclusive == 0x00BD4214);
        }

        SocketLargeQueueGatePlan unexpectedSkipReturn =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 5, diagnosticReturned: true));
        check("A diagnostic-return capture is rejected on an initial-state skip edge",
            unexpectedSkipReturn.Outcome
                == SocketLargeQueueGateOutcome.UnreachedCaptureProvided
            && !unexpectedSkipReturn.DiagnosticCallReached
            && unexpectedSkipReturn.GateByteAddress is null);

        SocketLargeQueueGatePlan missingDiagnosticReturn =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 4, gateByte: 0));
        SocketLargeQueueGatePlan nonreturningDiagnostic =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 4, diagnosticReturned: false));
        check("Other initial states require a captured normal diagnostic return before the byte read",
            missingDiagnosticReturn.Outcome
                == SocketLargeQueueGateOutcome.UnreachedCaptureProvided
            && missingDiagnosticReturn.GateByteAddress is null
            && nonreturningDiagnostic.Outcome
                == SocketLargeQueueGateOutcome.DiagnosticCallDidNotReturnNormally
            && nonreturningDiagnostic.DiagnosticCallReached
            && nonreturningDiagnostic.RouteRawFileOffset is null
            && nonreturningDiagnostic.StopAtRawFileOffsetExclusive == 0x00BD40EB);

        SocketLargeQueueGatePlan missingDiagnosticCapture =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 4));
        check("A missing diagnostic return remains nullable at its exact call-return boundary",
            missingDiagnosticCapture.Outcome
                == SocketLargeQueueGateOutcome.MissingDiagnosticReturnCapture
            && missingDiagnosticCapture.Capture.DiagnosticCallReturnedNormally is null
            && missingDiagnosticCapture.StopAtRawFileOffsetExclusive == 0x00BD40EB);

        SocketLargeQueueGatePlan diagnosticWithUnreachedState =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 4, diagnosticReturned: true, laterState: 3));
        check("A later-state capture is rejected before the intervening +0xD95 gate byte",
            diagnosticWithUnreachedState.Outcome
                == SocketLargeQueueGateOutcome.UnreachedCaptureProvided
            && diagnosticWithUnreachedState.GateByteAddress == 0x10203DD5
            && diagnosticWithUnreachedState.StopAtRawFileOffsetExclusive == 0x00BD40EE);

        SocketLargeQueueGatePlan missingGateByte =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 3));
        SocketLargeQueueGatePlan nonzeroWithUnreachedState =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 3, gateByte: 0xFF, laterState: 3));
        check("The byte gate is required, and a nonzero byte makes later state and payload captures unreachable",
            missingGateByte.Outcome == SocketLargeQueueGateOutcome.MissingGateByte
            && missingGateByte.GateByteAddress == 0x10203040u + 0xD95u
            && nonzeroWithUnreachedState.Outcome
                == SocketLargeQueueGateOutcome.UnreachedCaptureProvided
            && nonzeroWithUnreachedState.LaterStateAddress is null);

        SocketLargeQueueGatePlan missingLaterState =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 8, gateByte: 0));
        SocketLargeQueueGatePlan unrelatedLaterState =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 8, gateByte: 0, laterState: 5));
        SocketLargeQueueGatePlan unexpectedPayload =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 8, gateByte: 0, laterState: 5, payloadWord: 0xC000));
        check("A zero byte causes an independent later DWORD read; values other than 3 dispatch the header without a word read",
            missingLaterState.Outcome == SocketLargeQueueGateOutcome.MissingLaterState
            && unrelatedLaterState.Outcome
                == SocketLargeQueueGateOutcome.LaterStateBranchesToHeaderDispatch
            && unrelatedLaterState.RouteRawFileOffset == 0x00BD412B
            && unrelatedLaterState.PayloadWordAddress is null
            && unexpectedPayload.Outcome
                == SocketLargeQueueGateOutcome.UnreachedCaptureProvided);

        SocketLargeQueueGatePlan matchingStateMissingWord =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 4, diagnosticReturned: true, gateByte: 0, laterState: 3));
        SocketLargeQueueGatePlan matchingStateUnreachedDiagnostic =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 4, diagnosticReturned: false, gateByte: 0, laterState: 3));
        check("Later state 3 requires the payload word, while nonreturning diagnostics cannot reach any later read",
            matchingStateMissingWord.Outcome == SocketLargeQueueGateOutcome.MissingPayloadWord
            && matchingStateMissingWord.PayloadWordAddress == 0x55667788
            && matchingStateUnreachedDiagnostic.Outcome
                == SocketLargeQueueGateOutcome.UnreachedCaptureProvided);

        SocketLargeQueueGatePlan matchingPayload =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 4, diagnosticReturned: true, gateByte: 0,
                    laterState: 3, payloadWord: 0xC123));
        SocketLargeQueueGatePlan highBitOnlyPayload =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 3, gateByte: 0, laterState: 3, payloadWord: 0x8001));
        SocketLargeQueueGatePlan lowPayload =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 3, gateByte: 0, laterState: 3, payloadWord: 0x3FFF));
        check("The payload word uses a bit test, not equality: any 0xC000 bit hands off and no matching bit falls through",
            matchingPayload.Outcome
                == SocketLargeQueueGateOutcome.PayloadMaskBranchesToHandoff
            && matchingPayload.PayloadMaskedBits == 0xC000
            && matchingPayload.RouteRawFileOffset == 0x00BD4214
            && highBitOnlyPayload.Outcome
                == SocketLargeQueueGateOutcome.PayloadMaskBranchesToHandoff
            && highBitOnlyPayload.PayloadMaskedBits == 0x8000
            && lowPayload.Outcome
                == SocketLargeQueueGateOutcome.PayloadMaskFallsThroughToDeferredControl
            && lowPayload.PayloadMaskedBits == 0
            && lowPayload.RouteRawFileOffset == 0x00BD4112
            && lowPayload.StopAtRawFileOffsetExclusive == 0x00BD4112);

        SocketLargeQueueGatePlan independentStateReads =
            SocketLargeQueueGateProjection.Project(
                Capture(initialState: 5, gateByte: 0, laterState: 3, payloadWord: 0x4000));
        check("Initial and later +0x108 values remain separate; the later value independently selects the payload test",
            independentStateReads.Capture.InitialStateDword108 == 5
            && independentStateReads.Capture.LaterStateDword108 == 3
            && independentStateReads.Outcome
                == SocketLargeQueueGateOutcome.PayloadMaskBranchesToHandoff
            && independentStateReads.RouteRawFileOffset == 0x00BD4214);

        SocketLargeQueueGatePlan wrappedAddresses =
            SocketLargeQueueGateProjection.Project(
                new SocketLargeQueueGateCapture(
                    ReceiverPointer: uint.MaxValue - 0x100,
                    PayloadPointer: uint.MaxValue,
                    InitialStateDword108: 3,
                    DiagnosticCallReturnedNormally: null,
                    GateByteD95: 0,
                    LaterStateDword108: 3,
                    PayloadWord: 0));
        check("Receiver-field and payload addresses use 32-bit wrapping arithmetic without dereferencing",
            wrappedAddresses.InitialStateAddress == 7
            && wrappedAddresses.GateByteAddress == 0xC94
            && wrappedAddresses.LaterStateAddress == 7
            && wrappedAddresses.PayloadWordAddress == uint.MaxValue);
    }

    private static SocketLargeQueueGateCapture Capture(
        uint? initialState,
        bool? diagnosticReturned = null,
        byte? gateByte = null,
        uint? laterState = null,
        ushort? payloadWord = null) =>
        new(
            ReceiverPointer: 0x10203040,
            PayloadPointer: 0x55667788,
            InitialStateDword108: initialState,
            DiagnosticCallReturnedNormally: diagnosticReturned,
            GateByteD95: gateByte,
            LaterStateDword108: laterState,
            PayloadWord: payloadWord);
}
