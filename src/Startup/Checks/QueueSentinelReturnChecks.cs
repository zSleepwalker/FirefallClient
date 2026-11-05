using Firefall.Reconstruction.Startup;

internal static class QueueSentinelReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Queue-sentinel return anchors preserve the pinned sample, raw bounds, and raw-to-VA mapping",
            QueueSentinelReturnProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && QueueSentinelReturnProjectionRules.EntryRawFileOffset == 0x12CE280
            && QueueSentinelReturnProjectionRules.EndRawFileOffsetExclusive == 0x12CE30A
            && QueueSentinelReturnProjectionRules.EntryVirtualAddress == 0x16CEE80
            && QueueSentinelReturnProjectionRules.EntryVirtualAddress
                - QueueSentinelReturnProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueSentinelReturnProjectionRules.EntryRawFileOffset
            && QueueSentinelReturnProjectionRules.CapturedBodyBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x8A
            && QueueSentinelReturnProjectionRules.CapturedBodyBytes
                == "51 56 8B F1 57 8B 46 08 39 06 74 1D 68 7C 67 C5 01 "
                    + "68 D6 02 00 00 68 1B 43 B7 01 68 0F 43 B7 01 FF 15 "
                    + "A0 5E C1 01 83 C4 10 0F AE E8 83 7E 10 00 74 1D "
                    + "68 8C 61 C1 01 68 D7 02 00 00 68 27 43 B7 01 68 "
                    + "EB 42 B7 01 FF 15 A0 5E C1 01 83 C4 10 8B 36 8B "
                    + "3D BC 03 E0 01 85 F6 75 19 56 68 5A 02 00 00 68 "
                    + "93 43 B7 01 68 6F 43 B7 01 FF 15 A0 5E C1 01 83 "
                    + "C4 10 83 CA FF 8D 47 10 F0 0F C1 10 56 8B CF E8 "
                    + "5A F7 FF FF 5F 5E 59 C3");

        check("Ordered read, reporter, fence, counter, helper, and return anchors match the disassembly",
            QueueSentinelReturnProjectionRules.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.ReceiverRelativeOffset,
                        instruction.DiagnosticId, instruction.TargetRawFileOffset,
                        instruction.TargetVirtualAddress))
                .SequenceEqual(new
                    (uint RawFileOffset, string Bytes, int? ReceiverRelativeOffset,
                        uint? DiagnosticId, uint? TargetRawFileOffset,
                        uint? TargetVirtualAddress)[]
                {
                    (0x12CE285, "8B 46 08", 8, null, null, null),
                    (0x12CE288, "39 06", 0, null, null, null),
                    (0x12CE28A, "74 1D", null, null, null, null),
                    (0x12CE2A0, "FF 15 A0 5E C1 01", null, 0x2D6, null, 0x1C15EA0),
                    (0x12CE2A9, "0F AE E8", null, null, null, null),
                    (0x12CE2AC, "83 7E 10 00", 0x10, null, null, null),
                    (0x12CE2B0, "74 1D", null, null, null, null),
                    (0x12CE2C6, "FF 15 A0 5E C1 01", null, 0x2D7, null, 0x1C15EA0),
                    (0x12CE2CF, "8B 36", 0, null, null, null),
                    (0x12CE2D1, "8B 3D BC 03 E0 01", null, null, null, 0x1E003BC),
                    (0x12CE2D7, "85 F6", null, null, null, null),
                    (0x12CE2D9, "75 19", null, null, null, null),
                    (0x12CE2EB, "FF 15 A0 5E C1 01", null, 0x25A, null, 0x1C15EA0),
                    (0x12CE2F4, "83 CA FF", null, null, null, null),
                    (0x12CE2F7, "8D 47 10", null, null, null, null),
                    (0x12CE2FA, "F0 0F C1 10", null, null, null, null),
                    (0x12CE2FE, "56", null, null, null, null),
                    (0x12CE2FF, "8B CF", null, null, null, null),
                    (0x12CE301, "E8 5A F7 FF FF", null, null, 0x12CDA60, null),
                    (0x12CE309, "C3", null, null, null, null)
                })
            && QueueSentinelReturnProjectionRules.Instructions.All(instruction =>
                HasBodySlice(instruction.RawFileOffset, instruction.Bytes))
            && QueueSentinelReturnProjectionRules.FirstDiagnosticBranchRawFileOffset + 2 + 0x1D
                == QueueSentinelReturnProjectionRules.FenceRawFileOffset
            && QueueSentinelReturnProjectionRules.SecondDiagnosticBranchRawFileOffset + 2 + 0x1D
                == QueueSentinelReturnProjectionRules.LaterNodePointerReadRawFileOffset
            && QueueSentinelReturnProjectionRules.NullNodeDiagnosticBranchRawFileOffset + 2 + 0x19
                == QueueSentinelReturnProjectionRules.CounterXaddRawFileOffset - 6
            && QueueSentinelReturnProjectionRules.StackHandoffCallRawFileOffset + 5 - 0x8A6
                == QueueSentinelReturnProjectionRules.StackHandoffHelperRawFileOffset
            && QueueSentinelReturnProjectionRules.EndRawFileOffsetExclusive
                == QueueSentinelReturnProjectionRules.PlainReturnRawFileOffset + 1
            && QueueSentinelReturnProjectionRules.CounterXaddDeltaSetupRawFileOffset + 3
                == QueueSentinelReturnProjectionRules.CounterAddressSetupRawFileOffset
            && QueueSentinelReturnProjectionRules.CounterAddressSetupRawFileOffset + 3
                == QueueSentinelReturnProjectionRules.CounterXaddRawFileOffset
            && QueueSentinelReturnProjectionRules.PoolCounterOffset == 0x10
            && QueueSentinelReturnProjectionRules.CounterXaddDeltaBits == uint.MaxValue
            && QueueSentinelReturnProjectionRules.DiagnosticReporterFunctionPointerVirtualAddress
                == 0x1C15EA0
            && QueueSentinelReturnProjectionRules.PoolPointerGlobalVirtualAddress == 0x1E003BC);

        QueueSentinelReturnProjection ordinaryReturn =
            QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 0x11223344,
                capturedInitialReceiverWord0ForComparison: 0x11223344,
                diagnostic2D6ReporterReturned: null,
                capturedPostFenceReceiverWord10: 0,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: 0x55667788,
                capturedPoolPointer: 0,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: 0,
                handoffHelperReturned: true);
        check("Equal initial words skip diagnostic 0x2D6 and later zero word skips 0x2D7",
            ordinaryReturn.Outcome == QueueSentinelReturnOutcome.PlainReturnReached
            && ordinaryReturn.Actions.Select(action => action.Kind).SequenceEqual(
            [
                QueueSentinelReturnActionKind.CaptureInitialReceiverWord8,
                QueueSentinelReturnActionKind.CaptureInitialReceiverWord0ForComparison,
                QueueSentinelReturnActionKind.LoadFence,
                QueueSentinelReturnActionKind.CapturePostFenceReceiverWord10,
                QueueSentinelReturnActionKind.CaptureLaterNodePointer,
                QueueSentinelReturnActionKind.LoadPoolPointer,
                QueueSentinelReturnActionKind.ProjectCounterXaddArithmetic,
                QueueSentinelReturnActionKind.PushCapturedNode,
                QueueSentinelReturnActionKind.SetPoolInEcx,
                QueueSentinelReturnActionKind.CallStackHandoffHelper,
                QueueSentinelReturnActionKind.PlainReturn
            ])
            && ordinaryReturn.Actions.All(action =>
                action.DiagnosticId is not (0x2D6 or 0x2D7 or 0x25A))
            && ordinaryReturn.Actions[0].CapturedValue == 0x11223344
            && ordinaryReturn.Actions[1].CapturedValue == 0x11223344
            && ordinaryReturn.Actions[4].CapturedValue == 0x55667788
            && ordinaryReturn.ProjectedCounterAddress == 0x10
            && ordinaryReturn.ProjectedCounterAfterXadd == uint.MaxValue
            && ordinaryReturn.Actions
                .Single(action => action.Kind == QueueSentinelReturnActionKind.CallStackHandoffHelper)
                .SecondaryValue == 0
            && ordinaryReturn.HandoffHelperReturned == true);

        QueueSentinelReturnProjection returnedDiagnosticsAndNullNode =
            QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 8,
                capturedInitialReceiverWord0ForComparison: 4,
                diagnostic2D6ReporterReturned: true,
                capturedPostFenceReceiverWord10: 1,
                diagnostic2D7ReporterReturned: true,
                capturedLaterNodePointer: 0,
                capturedPoolPointer: 0xFFFFFFF8,
                nullNodeDiagnosticReporterReturned: true,
                capturedCounterBeforeXadd: uint.MaxValue,
                handoffHelperReturned: true);
        check("Each reached diagnostic needs its captured return, and a returning null-node report still reaches handoff",
            returnedDiagnosticsAndNullNode.Outcome == QueueSentinelReturnOutcome.PlainReturnReached
            && returnedDiagnosticsAndNullNode.Actions
                .Where(action => action.DiagnosticId.HasValue)
                .Select(action => (action.DiagnosticId, action.ReporterReturned))
                .SequenceEqual(new (uint? DiagnosticId, bool? ReporterReturned)[]
                {
                    (0x2D6, true), (0x2D7, true), (0x25A, true)
                })
            && Array.FindIndex(returnedDiagnosticsAndNullNode.Actions.ToArray(), action =>
                action.Kind == QueueSentinelReturnActionKind.LoadPoolPointer)
                < Array.FindIndex(returnedDiagnosticsAndNullNode.Actions.ToArray(), action =>
                    action.Kind == QueueSentinelReturnActionKind.ReportNullNode)
            && returnedDiagnosticsAndNullNode.Actions
                .Single(action => action.Kind == QueueSentinelReturnActionKind.ProjectCounterXaddArithmetic)
                is
                {
                    TargetAddress: 8,
                    CapturedValue: uint.MaxValue,
                    SecondaryValue: 0xFFFFFFF8,
                    CounterBeforeValue: uint.MaxValue,
                    CounterAfterValue: 0xFFFFFFFE
                }
            && returnedDiagnosticsAndNullNode.ProjectedCounterAddress == 8
            && returnedDiagnosticsAndNullNode.ProjectedCounterAfterXadd == 0xFFFFFFFE
            && returnedDiagnosticsAndNullNode.Actions
                .Single(action => action.Kind == QueueSentinelReturnActionKind.PushCapturedNode)
                .CapturedValue == 0
            && returnedDiagnosticsAndNullNode.Actions
                .Single(action => action.Kind == QueueSentinelReturnActionKind.SetPoolInEcx)
                .CapturedValue == 0xFFFFFFF8
            && returnedDiagnosticsAndNullNode.Actions
                .Single(action => action.Kind == QueueSentinelReturnActionKind.CallStackHandoffHelper)
                .TargetRawFileOffset == 0x12CDA60
            && returnedDiagnosticsAndNullNode.HandoffHelperReturned == true
            && !QueueSentinelReturnProjectionRules.CounterXaddOldValueUsed);

        QueueSentinelReturnProjection handoffCallBoundary =
            QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 1,
                capturedInitialReceiverWord0ForComparison: 1,
                diagnostic2D6ReporterReturned: null,
                capturedPostFenceReceiverWord10: 0,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: 0x44,
                capturedPoolPointer: 0x1000,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: 9);
        QueueSentinelReturnProjection handoffDidNotReturn =
            QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 1,
                capturedInitialReceiverWord0ForComparison: 1,
                diagnostic2D6ReporterReturned: null,
                capturedPostFenceReceiverWord10: 0,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: 0x44,
                capturedPoolPointer: 0x1000,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: 9,
                handoffHelperReturned: false);
        check("The helper call is a boundary unless a true nullable return capture enables the plain RET",
            handoffCallBoundary.Outcome
                == QueueSentinelReturnOutcome.HandoffHelperReturnUnavailable
            && handoffCallBoundary.HandoffHelperReturned is null
            && handoffCallBoundary.Actions[^1].Kind
                == QueueSentinelReturnActionKind.CallStackHandoffHelper
            && handoffCallBoundary.Actions.All(action =>
                action.Kind != QueueSentinelReturnActionKind.PlainReturn)
            && handoffDidNotReturn.Outcome == QueueSentinelReturnOutcome.HandoffHelperDidNotReturn
            && handoffDidNotReturn.Actions[^1].Kind
                == QueueSentinelReturnActionKind.CallStackHandoffHelper
            && handoffDidNotReturn.Actions.All(action =>
                action.Kind != QueueSentinelReturnActionKind.PlainReturn));

        QueueSentinelReturnProjection missingCounterBefore =
            QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 1,
                capturedInitialReceiverWord0ForComparison: 1,
                diagnostic2D6ReporterReturned: null,
                capturedPostFenceReceiverWord10: 0,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: 0x44,
                capturedPoolPointer: 0x1000,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: null);
        check("Missing independent counter-before capture stops at the XADD boundary",
            missingCounterBefore.Outcome
                == QueueSentinelReturnOutcome.CounterBeforeXaddUnavailable
            && missingCounterBefore.ProjectedCounterAddress is null
            && missingCounterBefore.ProjectedCounterAfterXadd is null
            && missingCounterBefore.Actions.All(action =>
                action.Kind != QueueSentinelReturnActionKind.ProjectCounterXaddArithmetic
                && action.Kind != QueueSentinelReturnActionKind.CallStackHandoffHelper));

        QueueSentinelReturnProjection unknownFirstReporterReturn =
            QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 1,
                capturedInitialReceiverWord0ForComparison: 2,
                diagnostic2D6ReporterReturned: null,
                capturedPostFenceReceiverWord10: null,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: null,
                capturedPoolPointer: null,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: null);
        check("An uncaptured first reporter return stops before the fence and all later reads",
            unknownFirstReporterReturn.Outcome
                == QueueSentinelReturnOutcome.FirstDiagnosticReporterReturnUnavailable
            && unknownFirstReporterReturn.Actions.Select(action => action.Kind).SequenceEqual(
            [
                QueueSentinelReturnActionKind.CaptureInitialReceiverWord8,
                QueueSentinelReturnActionKind.CaptureInitialReceiverWord0ForComparison,
                QueueSentinelReturnActionKind.ReportReceiverWordMismatch
            ]));

        QueueSentinelReturnProjection firstReporterDidNotReturn =
            QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 1,
                capturedInitialReceiverWord0ForComparison: 2,
                diagnostic2D6ReporterReturned: false,
                capturedPostFenceReceiverWord10: null,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: null,
                capturedPoolPointer: null,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: null);
        check("A false first reporter-return capture prevents the opaque helper call",
            firstReporterDidNotReturn.Outcome
                == QueueSentinelReturnOutcome.FirstDiagnosticReporterDidNotReturn
            && firstReporterDidNotReturn.Actions.All(action =>
                action.Kind != QueueSentinelReturnActionKind.CallStackHandoffHelper
                && action.Kind != QueueSentinelReturnActionKind.ProjectCounterXaddArithmetic));

        QueueSentinelReturnProjection secondReporterDidNotReturn =
            QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 1,
                capturedInitialReceiverWord0ForComparison: 1,
                diagnostic2D6ReporterReturned: null,
                capturedPostFenceReceiverWord10: 7,
                diagnostic2D7ReporterReturned: false,
                capturedLaterNodePointer: null,
                capturedPoolPointer: null,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: null);
        check("The post-fence diagnostic also stops before the independent node read when its reporter does not return",
            secondReporterDidNotReturn.Outcome
                == QueueSentinelReturnOutcome.SecondDiagnosticReporterDidNotReturn
            && secondReporterDidNotReturn.Actions[^1].Kind
                == QueueSentinelReturnActionKind.ReportNonzeroReceiverWord10
            && secondReporterDidNotReturn.Actions.All(action =>
                action.Kind != QueueSentinelReturnActionKind.CaptureLaterNodePointer
                && action.Kind != QueueSentinelReturnActionKind.CallStackHandoffHelper));

        QueueSentinelReturnProjection unknownSecondReporterReturn =
            QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 1,
                capturedInitialReceiverWord0ForComparison: 1,
                diagnostic2D6ReporterReturned: null,
                capturedPostFenceReceiverWord10: 7,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: null,
                capturedPoolPointer: null,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: null);
        check("An uncaptured second reporter return stops before the independent receiver +0 read",
            unknownSecondReporterReturn.Outcome
                == QueueSentinelReturnOutcome.SecondDiagnosticReporterReturnUnavailable
            && unknownSecondReporterReturn.Actions[^1].Kind
                == QueueSentinelReturnActionKind.ReportNonzeroReceiverWord10
            && unknownSecondReporterReturn.Actions.All(action =>
                action.Kind != QueueSentinelReturnActionKind.CaptureLaterNodePointer));

        QueueSentinelReturnProjection unknownNullReporterReturn =
            QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 1,
                capturedInitialReceiverWord0ForComparison: 1,
                diagnostic2D6ReporterReturned: null,
                capturedPostFenceReceiverWord10: 0,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: 0,
                capturedPoolPointer: 0,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: null);
        check("The global pool is captured before a null-node diagnostic whose return is unknown",
            unknownNullReporterReturn.Outcome
                == QueueSentinelReturnOutcome.NullNodeDiagnosticReporterReturnUnavailable
            && unknownNullReporterReturn.Actions[^2].Kind == QueueSentinelReturnActionKind.LoadPoolPointer
            && unknownNullReporterReturn.Actions[^1].DiagnosticId == 0x25A
            && unknownNullReporterReturn.ProjectedCounterAddress is null
            && unknownNullReporterReturn.Actions.All(action =>
                action.Kind != QueueSentinelReturnActionKind.CallStackHandoffHelper));

        QueueSentinelReturnProjection nullReporterDidNotReturn =
            QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 1,
                capturedInitialReceiverWord0ForComparison: 1,
                diagnostic2D6ReporterReturned: null,
                capturedPostFenceReceiverWord10: 0,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: 0,
                capturedPoolPointer: 0x2000,
                nullNodeDiagnosticReporterReturned: false,
                capturedCounterBeforeXadd: null);
        check("A false null-node reporter return stops before counter projection and the opaque helper",
            nullReporterDidNotReturn.Outcome
                == QueueSentinelReturnOutcome.NullNodeDiagnosticReporterDidNotReturn
            && nullReporterDidNotReturn.Actions[^1].DiagnosticId == 0x25A
            && nullReporterDidNotReturn.Actions.All(action =>
                action.Kind != QueueSentinelReturnActionKind.ProjectCounterXaddArithmetic
                && action.Kind != QueueSentinelReturnActionKind.CallStackHandoffHelper));

        bool rejectsUnreachedCaptures =
            ThrowsArgumentException(() => QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 5,
                capturedInitialReceiverWord0ForComparison: 5,
                diagnostic2D6ReporterReturned: true,
                capturedPostFenceReceiverWord10: 0,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: null,
                capturedPoolPointer: null,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: null,
                handoffHelperReturned: true))
            && ThrowsArgumentException(() => QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 5,
                capturedInitialReceiverWord0ForComparison: 6,
                diagnostic2D6ReporterReturned: false,
                capturedPostFenceReceiverWord10: 0,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: null,
                capturedPoolPointer: null,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: null,
                handoffHelperReturned: true))
            && ThrowsArgumentException(() => QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 5,
                capturedInitialReceiverWord0ForComparison: 5,
                diagnostic2D6ReporterReturned: null,
                capturedPostFenceReceiverWord10: 0,
                diagnostic2D7ReporterReturned: true,
                capturedLaterNodePointer: null,
                capturedPoolPointer: null,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: null))
            && ThrowsArgumentException(() => QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 5,
                capturedInitialReceiverWord0ForComparison: 5,
                diagnostic2D6ReporterReturned: null,
                capturedPostFenceReceiverWord10: 0,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: 0,
                capturedPoolPointer: null,
                nullNodeDiagnosticReporterReturned: true,
                capturedCounterBeforeXadd: null,
                handoffHelperReturned: true))
            && ThrowsArgumentException(() => QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 5,
                capturedInitialReceiverWord0ForComparison: 5,
                diagnostic2D6ReporterReturned: null,
                capturedPostFenceReceiverWord10: 0,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: 0x44,
                capturedPoolPointer: 0x1000,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: null,
                handoffHelperReturned: true))
            && ThrowsArgumentException(() => QueueSentinelReturnProjectionRules.Project(
                capturedInitialReceiverWord8: 5,
                capturedInitialReceiverWord0ForComparison: 6,
                diagnostic2D6ReporterReturned: false,
                capturedPostFenceReceiverWord10: null,
                diagnostic2D7ReporterReturned: null,
                capturedLaterNodePointer: null,
                capturedPoolPointer: null,
                nullNodeDiagnosticReporterReturned: null,
                capturedCounterBeforeXadd: 7));
        check("Unreached reporter returns and later independent captures are rejected",
            rejectsUnreachedCaptures);
    }

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }

    private static bool HasBodySlice(uint rawFileOffset, string expected)
    {
        string[] body =
            QueueSentinelReturnProjectionRules.CapturedBodyBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        uint captureStart = QueueSentinelReturnProjectionRules.EntryRawFileOffset;
        if (rawFileOffset < captureStart)
        {
            return false;
        }

        string[] instruction = expected.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int start = checked((int)(rawFileOffset - captureStart));
        return start + instruction.Length <= body.Length
            && body.Skip(start).Take(instruction.Length).SequenceEqual(instruction);
    }
}
