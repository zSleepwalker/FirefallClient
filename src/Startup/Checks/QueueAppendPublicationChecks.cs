using Firefall.Reconstruction.Startup;

internal static class QueueAppendPublicationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Queue-append continuation pins the requested sample, raw range, and raw-to-VA mapping",
            QueueAppendPublicationProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && QueueAppendPublicationProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && QueueAppendPublicationProjection.ContinuationStartRawFileOffset == 0x12CE3C3
            && QueueAppendPublicationProjection.ContinuationEndRawFileOffsetExclusive == 0x12CE3F7
            && QueueAppendPublicationProjection.ContinuationEndRawFileOffsetExclusive
                - QueueAppendPublicationProjection.ContinuationStartRawFileOffset == 0x34
            && QueueAppendPublicationProjection.ContinuationStartRawFileOffset
                + QueueAppendPublicationProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CEFC3
            && QueueAppendPublicationProjection.ContinuationEndRawFileOffsetExclusive
                + QueueAppendPublicationProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CEFF7
            && QueueAppendPublicationProjection.CapturedContinuationBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x34
            && QueueAppendPublicationProjection.CapturedContinuationBytes
                == "83 3E 00 74 1A 6A 00 68 F0 02 00 00 68 F7 42 B7 01 "
                    + "68 4B 43 B7 01 FF 15 A0 5E C1 01 83 C4 10 8B 45 08 "
                    + "89 46 04 8B 44 24 0C 5F 89 06 5E 5B 8B E5 5D C2 04 00");

        check("Queue-append instruction slices pin every modeled store, path target, diagnostic, and return",
            QueueAppendPublicationProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.IndirectCallTargetSlotVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    uint? BranchTargetRawFileOffset,
                    uint? IndirectCallTargetSlotVirtualAddress)[]
                {
                    (0x12CE3C3, "83 3E 00", null, null),
                    (0x12CE3C6, "74 1A", 0x12CE3E2, null),
                    (0x12CE3C8, "6A 00", null, null),
                    (0x12CE3CA, "68 F0 02 00 00", null, null),
                    (0x12CE3CF, "68 F7 42 B7 01", null, null),
                    (0x12CE3D4, "68 4B 43 B7 01", null, null),
                    (0x12CE3D9, "FF 15 A0 5E C1 01", null, 0x1C15EA0),
                    (0x12CE3DF, "83 C4 10", null, null),
                    (0x12CE3E2, "8B 45 08", null, null),
                    (0x12CE3E5, "89 46 04", null, null),
                    (0x12CE3E8, "8B 44 24 0C", null, null),
                    (0x12CE3ED, "89 06", null, null),
                    (0x12CE3F4, "C2 04 00", null, null)
                })
            && QueueAppendPublicationProjection.DiagnosticId == 0x2F0
            && QueueAppendPublicationProjection.DiagnosticIndirectCallTargetSlotVirtualAddress
                == 0x1C15EA0
            && QueueAppendPublicationProjection.DiagnosticFirstAddressArgumentVirtualAddress
                == 0x1B7434B
            && QueueAppendPublicationProjection.DiagnosticSecondAddressArgumentVirtualAddress
                == 0x1B742F7
            && QueueAppendPublicationProjection.NextPointerIsZeroBranchRawFileOffset + 2 + 0x1A
                == QueueAppendPublicationProjection.NextPointerZeroBranchTargetRawFileOffset);

        var noNextCapture = QueueAppendPublicationProjection.Project(
            new QueueAppendPublicationCapture(
                CapturedOldTailPointer: 0x10203040,
                CapturedNextPointer: null,
                DiagnosticCallReturned: null,
                CapturedPayloadArgument: null,
                CapturedNewNodePointer: null));
        check("A missing reached next-pointer capture stops at its explicit instruction boundary",
            noNextCapture.Outcome == QueueAppendPublicationOutcome.AwaitingNextPointerCapture
            && noNextCapture.Actions.SequenceEqual(new[]
            {
                new QueueAppendPublicationAction(
                    QueueAppendPublicationActionKind.StopAtNextPointerCaptureBoundary,
                    0x12CE3C3,
                    Pointer: 0x10203040)
            })
            && noNextCapture.ReturnValue is null);

        var emptyNext = QueueAppendPublicationProjection.Project(
            new QueueAppendPublicationCapture(
                CapturedOldTailPointer: 0x10203040,
                CapturedNextPointer: 0,
                DiagnosticCallReturned: null,
                CapturedPayloadArgument: 0xAABBCCDD,
                CapturedNewNodePointer: 0x55667788));
        check("A zero next pointer skips reporting and rejects a supplied unreachable report-return capture",
            emptyNext.Outcome == QueueAppendPublicationOutcome.ReturnedCapturedNewNode
            && !emptyNext.Actions.Any(action =>
                action.Kind == QueueAppendPublicationActionKind.CallDiagnosticThroughIndirectCell)
            && emptyNext.Actions[1]
                == new QueueAppendPublicationAction(
                    QueueAppendPublicationActionKind.SkipDiagnosticOnNullNextPointer,
                    0x12CE3C6,
                    BranchTargetRawFileOffset: 0x12CE3E2)
            && Array.FindIndex(emptyNext.Actions.ToArray(), action =>
                action.Kind == QueueAppendPublicationActionKind.AttemptPayloadStoreToOldTail)
                < Array.FindIndex(emptyNext.Actions.ToArray(), action =>
                    action.Kind == QueueAppendPublicationActionKind.AttemptNewNodePublicationToOldTail)
            && ThrowsArgumentException(() => QueueAppendPublicationProjection.Project(
                new QueueAppendPublicationCapture(
                    0x10203040, 0, false, 1, 2))));

        var missingDiagnosticReturn = QueueAppendPublicationProjection.Project(
            new QueueAppendPublicationCapture(
                CapturedOldTailPointer: 0x10203040,
                CapturedNextPointer: 0x55667788,
                DiagnosticCallReturned: null,
                CapturedPayloadArgument: null,
                CapturedNewNodePointer: null));
        var diagnosticDoesNotReturn = QueueAppendPublicationProjection.Project(
            new QueueAppendPublicationCapture(
                CapturedOldTailPointer: 0x10203040,
                CapturedNextPointer: 0x55667788,
                DiagnosticCallReturned: false,
                CapturedPayloadArgument: null,
                CapturedNewNodePointer: null));
        check("A nonzero next pointer reports 0x2F0 and stops unless its opaque continuation is captured as returned",
            missingDiagnosticReturn.Outcome
                == QueueAppendPublicationOutcome.AwaitingDiagnosticReturnCapture
            && missingDiagnosticReturn.Actions[^2].Kind
                == QueueAppendPublicationActionKind.CallDiagnosticThroughIndirectCell
            && missingDiagnosticReturn.Actions[^1].Kind
                == QueueAppendPublicationActionKind.StopAtDiagnosticReturnCaptureBoundary
            && diagnosticDoesNotReturn.Outcome
                == QueueAppendPublicationOutcome.DiagnosticContinuationBoundary
            && diagnosticDoesNotReturn.Actions[^1].Kind
                == QueueAppendPublicationActionKind.StopAtOpaqueDiagnosticContinuationBoundary
            && ThrowsArgumentException(() => QueueAppendPublicationProjection.Project(
                new QueueAppendPublicationCapture(
                    0x10203040, 0x55667788, null, 1, 2)))
            && ThrowsArgumentException(() => QueueAppendPublicationProjection.Project(
                new QueueAppendPublicationCapture(
                    0x10203040, 0x55667788, false, 1, 2))));

        var returnedReportMissingPayload = QueueAppendPublicationProjection.Project(
            new QueueAppendPublicationCapture(
                CapturedOldTailPointer: 0x10203040,
                CapturedNextPointer: 0x55667788,
                DiagnosticCallReturned: true,
                CapturedPayloadArgument: null,
                CapturedNewNodePointer: null));
        var missingNewNode = QueueAppendPublicationProjection.Project(
            new QueueAppendPublicationCapture(
                CapturedOldTailPointer: 0xFFFFFFFC,
                CapturedNextPointer: 0,
                DiagnosticCallReturned: null,
                CapturedPayloadArgument: 0xAABBCCDD,
                CapturedNewNodePointer: null));
        check("Missing reached payload/new-node captures stop explicitly and preserve already reached payload ordering",
            returnedReportMissingPayload.Outcome
                == QueueAppendPublicationOutcome.AwaitingPayloadCapture
            && returnedReportMissingPayload.Actions[^1].RawFileOffset == 0x12CE3E2
            && missingNewNode.Outcome == QueueAppendPublicationOutcome.AwaitingNewNodeCapture
            && missingNewNode.Actions[^2]
                == new QueueAppendPublicationAction(
                    QueueAppendPublicationActionKind.AttemptPayloadStoreToOldTail,
                    0x12CE3E5,
                    Pointer: 0,
                    CapturedValue: 0xAABBCCDD)
            && missingNewNode.Actions[^1].RawFileOffset == 0x12CE3E8
            && ThrowsArgumentException(() => QueueAppendPublicationProjection.Project(
                new QueueAppendPublicationCapture(
                    0x10203040, null, null, null, 1)))
            && ThrowsArgumentException(() => QueueAppendPublicationProjection.Project(
                new QueueAppendPublicationCapture(
                    0x10203040, 0, null, null, 1))));

        var complete = QueueAppendPublicationProjection.Project(
            new QueueAppendPublicationCapture(
                CapturedOldTailPointer: 0x10203040,
                CapturedNextPointer: 0x55667788,
                DiagnosticCallReturned: true,
                CapturedPayloadArgument: 0xAABBCCDD,
                CapturedNewNodePointer: 0x66778899));
        check("A returned diagnostic continuation stores payload before publishing the captured node and returns that node",
            complete.Outcome == QueueAppendPublicationOutcome.ReturnedCapturedNewNode
            && complete.ReturnValue == 0x66778899
            && complete.Actions.Select(action => action.Kind).SequenceEqual(new[]
            {
                QueueAppendPublicationActionKind.CaptureOldTailNextPointer,
                QueueAppendPublicationActionKind.PushDiagnosticTrailingZeroArgument,
                QueueAppendPublicationActionKind.PushDiagnosticId,
                QueueAppendPublicationActionKind.PushDiagnosticSecondAddressArgument,
                QueueAppendPublicationActionKind.PushDiagnosticFirstAddressArgument,
                QueueAppendPublicationActionKind.CallDiagnosticThroughIndirectCell,
                QueueAppendPublicationActionKind.ContinueAfterDiagnosticReturns,
                QueueAppendPublicationActionKind.CapturePayloadArgument,
                QueueAppendPublicationActionKind.AttemptPayloadStoreToOldTail,
                QueueAppendPublicationActionKind.CaptureNewNodePointer,
                QueueAppendPublicationActionKind.AttemptNewNodePublicationToOldTail,
                QueueAppendPublicationActionKind.ReturnCapturedNewNodeAndPopArgument
            })
            && complete.Actions[^4]
                == new QueueAppendPublicationAction(
                    QueueAppendPublicationActionKind.AttemptPayloadStoreToOldTail,
                    0x12CE3E5,
                    Pointer: 0x10203044,
                    CapturedValue: 0xAABBCCDD)
            && complete.Actions[^2]
                == new QueueAppendPublicationAction(
                    QueueAppendPublicationActionKind.AttemptNewNodePublicationToOldTail,
                    0x12CE3ED,
                    Pointer: 0x10203040,
                    CapturedValue: 0x66778899)
            && complete.Actions[^1]
                == new QueueAppendPublicationAction(
                    QueueAppendPublicationActionKind.ReturnCapturedNewNodeAndPopArgument,
                    0x12CE3F4,
                    CapturedValue: 0x66778899));
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
}
