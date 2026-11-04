using Firefall.Reconstruction.Startup;

internal static class QueuePopAttemptChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Queue-pop attempt anchors use the pinned sample and raw-offset-to-VA mapping",
            QueuePopAttemptProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && QueuePopAttemptProjection.EntryRawFileOffset == 0x12CE400
            && QueuePopAttemptProjection.EntryVirtualAddress == 0x16CF000
            && QueuePopAttemptProjection.EntryRawFileOffset
                + QueuePopAttemptProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == QueuePopAttemptProjection.EntryVirtualAddress
            && QueuePopAttemptProjection.FunctionCaptureEndRawFileOffsetExclusive
                == 0x12CE475
            && QueuePopAttemptProjection.FunctionCaptureEndRawFileOffsetExclusive
                + QueuePopAttemptProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CF075
            && QueuePopAttemptProjection.TagCaptureRawFileOffset == 0x12CE410
            && QueuePopAttemptProjection.HeadCaptureRawFileOffset == 0x12CE413
            && QueuePopAttemptProjection.PairHeadOffset == 0
            && QueuePopAttemptProjection.PairHighTagOffset == 4);

        check("Queue-pop raw bytes preserve diagnostic, null-link, and one-attempt CAS branches",
            QueuePopAttemptProjection.CapturedFunctionBytes
                == "55 8B EC 83 E4 F8 83 EC 0C 53 56 57 8B F1 8B FF "
                    + "8B 46 04 8B 1E 89 44 24 10 89 5C 24 14 85 DB 75 1D "
                    + "53 68 00 03 00 00 68 3F 43 B7 01 68 63 43 B7 01 FF 15 "
                    + "A0 5E C1 01 8B 44 24 20 83 C4 10 8B 13 85 D2 75 0E "
                    + "3B 46 04 75 C7 33 C0 5F 5E 5B 8B E5 5D C3 33 FF 89 "
                    + "44 24 10 0B FB 8D 48 01 33 DB 8B C7 0B DA 8B 54 "
                    + "24 10 F0 0F C7 0E 3B C7 75 A1 3B 54 24 10 75 9B"
            && QueuePopAttemptProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.BranchTargetRawFileOffset))
                .SequenceEqual(new (uint RawFileOffset, string Bytes, uint? BranchTargetRawFileOffset)[]
                {
                    (0x12CE410, "8B 46 04", null),
                    (0x12CE413, "8B 1E", null),
                    (0x12CE41D, "85 DB", null),
                    (0x12CE41F, "75 1D", 0x12CE43E),
                    (0x12CE421, "53 68 00 03 00 00", null),
                    (0x12CE431, "FF 15 A0 5E C1 01", null),
                    (0x12CE43E, "8B 13", null),
                    (0x12CE440, "85 D2", null),
                    (0x12CE442, "75 0E", 0x12CE452),
                    (0x12CE444, "3B 46 04", null),
                    (0x12CE447, "75 C7", 0x12CE410),
                    (0x12CE449, "33 C0 5F 5E 5B 8B E5 5D C3", null),
                    (0x12CE467, "F0 0F C7 0E", null),
                    (0x12CE46B, "3B C7", null),
                    (0x12CE46D, "75 A1", 0x12CE410),
                    (0x12CE46F, "3B 54 24 10", null),
                    (0x12CE473, "75 9B", 0x12CE410)
                })
            && QueuePopAttemptProjection.NullHeadDiagnosticReportId == 0x300
            && QueuePopAttemptProjection.DiagnosticCallCellAddress == 0x01C15EA0
            && QueuePopAttemptProjection.NullNextReturnEndRawFileOffsetExclusive
                == 0x12CE452
            && QueuePopAttemptProjection.SuccessContinuationBoundaryRawFileOffset
                == 0x12CE475);

        QueuePopAttemptPlan missingDiagnosticReturn =
            QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(0),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Missing()));
        check("A null captured head reports 0x300 and stops at the missing diagnostic-return boundary",
            missingDiagnosticReturn.Outcome
                == QueuePopAttemptOutcome.MissingDiagnosticReturnBoundary
            && missingDiagnosticReturn.ReturnValue is null
            && missingDiagnosticReturn.Actions[^1]
                == new QueuePopAttemptAction(
                    QueuePopAttemptActionKind.StopAtMissingDiagnosticReturnBoundary,
                    0x12CE431)
            && missingDiagnosticReturn.Actions.Any(action =>
                action.Kind == QueuePopAttemptActionKind.ReportNullHeadDiagnostic
                && action.Value == 0x300)
            && !missingDiagnosticReturn.Actions.Any(action =>
                action.Kind == QueuePopAttemptActionKind.ReturnZeroAfterUnchangedHighTag));

        QueuePopAttemptPlan nullHeadNullLink =
            QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(0),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Captured(true),
                    next: QueuePopAttemptCapture<uint>.Captured(0),
                    laterTag: QueuePopAttemptCapture<uint>.Captured(7)));
        check("A returned null-head diagnostic proceeds to the attempted link read and is not clean empty",
            nullHeadNullLink.Outcome == QueuePopAttemptOutcome.ReturnedZeroAfterNullNext
            && nullHeadNullLink.ReturnValue == 0
            && nullHeadNullLink.Actions.Any(action =>
                action.Kind == QueuePopAttemptActionKind.ReadCapturedHeadLink
                && action.InstructionRawFileOffset == 0x12CE43E
                && action.Value == 0)
            && nullHeadNullLink.Actions.Any(action =>
                action.Kind == QueuePopAttemptActionKind.CaptureLaterReceiverHighTag
                && action.InstructionRawFileOffset == 0x12CE444)
            && nullHeadNullLink.Actions[^1].InstructionRawFileOffset == 0x12CE449);

        QueuePopAttemptPlan unchangedNullNext =
            QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(0x1234),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Unreached(),
                    next: QueuePopAttemptCapture<uint>.Captured(0),
                    laterTag: QueuePopAttemptCapture<uint>.Captured(7)));
        QueuePopAttemptPlan changedNullNextTag =
            QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(0x1234),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Unreached(),
                    next: QueuePopAttemptCapture<uint>.Captured(0),
                    laterTag: QueuePopAttemptCapture<uint>.Captured(10)));
        check("A null next link compares the separately recaptured tag before returning zero or retrying",
            unchangedNullNext.Outcome == QueuePopAttemptOutcome.ReturnedZeroAfterNullNext
            && unchangedNullNext.ReturnValue == 0
            && changedNullNextTag.Outcome == QueuePopAttemptOutcome.RetryBoundary
            && changedNullNextTag.ReturnValue is null
            && changedNullNextTag.Actions[^1]
                == new QueuePopAttemptAction(
                    QueuePopAttemptActionKind.StopAtUnmodeledRetryBoundary,
                    0x12CE447,
                    BranchTargetRawFileOffset: 0x12CE410)
            && !changedNullNextTag.Actions.Any(action =>
                action.Kind == QueuePopAttemptActionKind.AttemptLockedCompareExchange));

        QueuePopAttemptPlan successBoundary =
            QueuePopAttemptProjection.Project(
                Captures(
                    capturedHighTag: uint.MaxValue,
                    head: QueuePopAttemptCapture<uint>.Captured(0x12345678),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Unreached(),
                    next: QueuePopAttemptCapture<uint>.Captured(0x87654321),
                    returnedHead: QueuePopAttemptCapture<uint>.Captured(0x12345678),
                    returnedHighTag: QueuePopAttemptCapture<uint>.Captured(uint.MaxValue)));
        check("A matching returned pair stops exactly at the success continuation boundary",
            successBoundary.Outcome == QueuePopAttemptOutcome.SuccessBoundary
            && successBoundary.ReturnValue is null
            && successBoundary.ExpectedPair == new QueuePopAttemptPair(0x12345678, uint.MaxValue)
            && successBoundary.ReplacementPair == new QueuePopAttemptPair(0x87654321, 0)
            && successBoundary.CompareExchangeReturnedPair
                == new QueuePopAttemptPair(0x12345678, uint.MaxValue)
            && successBoundary.Actions[^1]
                == new QueuePopAttemptAction(
                    QueuePopAttemptActionKind.StopAtSuccessBoundary,
                    0x12CE475)
            && !successBoundary.Actions.Any(action =>
                action.InstructionRawFileOffset > 0x12CE475));

        QueuePopAttemptPlan lowMismatch =
            QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(1),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Unreached(),
                    next: QueuePopAttemptCapture<uint>.Captured(2),
                    returnedHead: QueuePopAttemptCapture<uint>.Captured(3),
                    returnedHighTag: QueuePopAttemptCapture<uint>.Unreached()));
        QueuePopAttemptPlan highMismatch =
            QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(1),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Unreached(),
                    next: QueuePopAttemptCapture<uint>.Captured(2),
                    returnedHead: QueuePopAttemptCapture<uint>.Captured(1),
                    returnedHighTag: QueuePopAttemptCapture<uint>.Captured(4)));
        check("Either explicitly compared returned half can stop at the retry branch without a retry loop",
            lowMismatch.Outcome == QueuePopAttemptOutcome.RetryBoundary
            && lowMismatch.Actions[^1].InstructionRawFileOffset == 0x12CE46D
            && lowMismatch.Actions.All(action =>
                action.Kind != QueuePopAttemptActionKind.CompareReturnedHighTag)
            && highMismatch.Outcome == QueuePopAttemptOutcome.RetryBoundary
            && highMismatch.Actions[^1].InstructionRawFileOffset == 0x12CE473);

        QueuePopAttemptPlan missingPair =
            QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(1),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Unreached(),
                    next: QueuePopAttemptCapture<uint>.Captured(2),
                    returnedHead: QueuePopAttemptCapture<uint>.Missing(),
                    returnedHighTag: QueuePopAttemptCapture<uint>.Unreached()));
        check("A missing optional returned pair stops at the CAS result boundary",
            missingPair.Outcome == QueuePopAttemptOutcome.MissingCompareExchangeReturnBoundary
            && missingPair.ExpectedPair == new QueuePopAttemptPair(1, 7)
            && missingPair.ReplacementPair == new QueuePopAttemptPair(2, 8)
            && missingPair.CompareExchangeReturnedPair is null);

        bool missingDiagnosticRejectsNextCapture = ThrowsArgumentException(
            () => QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(0),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Missing(),
                    next: QueuePopAttemptCapture<uint>.Captured(0))));
        bool nullHeadRejectsUnreachedDiagnostic = ThrowsArgumentException(
            () => QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(0),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Unreached())));
        bool nonnullHeadRejectsReachedDiagnostic = ThrowsArgumentException(
            () => QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(1),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Captured(true))));
        bool nonnullNextRejectsUnreachedLowReturn = ThrowsArgumentException(
            () => QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(1),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Unreached(),
                    next: QueuePopAttemptCapture<uint>.Captured(2),
                    returnedHead: QueuePopAttemptCapture<uint>.Unreached())));
        bool lowMismatchRejectsReachedHighReturn = ThrowsArgumentException(
            () => QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(1),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Unreached(),
                    next: QueuePopAttemptCapture<uint>.Captured(2),
                    returnedHead: QueuePopAttemptCapture<uint>.Captured(3),
                    returnedHighTag: QueuePopAttemptCapture<uint>.Captured(7))));
        bool nullNextRejectsReachedCasReturn = ThrowsArgumentException(
            () => QueuePopAttemptProjection.Project(
                Captures(
                    head: QueuePopAttemptCapture<uint>.Captured(1),
                    diagnosticReturned: QueuePopAttemptCapture<bool>.Unreached(),
                    next: QueuePopAttemptCapture<uint>.Captured(0),
                    laterTag: QueuePopAttemptCapture<uint>.Captured(7),
                    returnedHead: QueuePopAttemptCapture<uint>.Captured(1))));
        check("Projection rejects captures from instruction paths that were not reached",
            missingDiagnosticRejectsNextCapture
            && nullHeadRejectsUnreachedDiagnostic
            && nonnullHeadRejectsReachedDiagnostic
            && nonnullNextRejectsUnreachedLowReturn
            && lowMismatchRejectsReachedHighReturn
            && nullNextRejectsReachedCasReturn);
    }

    private static QueuePopAttemptCaptures Captures(
        uint capturedHighTag = 7,
        QueuePopAttemptCapture<uint>? head = null,
        QueuePopAttemptCapture<bool>? diagnosticReturned = null,
        QueuePopAttemptCapture<uint>? next = null,
        QueuePopAttemptCapture<uint>? laterTag = null,
        QueuePopAttemptCapture<uint>? returnedHead = null,
        QueuePopAttemptCapture<uint>? returnedHighTag = null) =>
        new(
            ReceiverPairPointer: 0x10203040,
            CapturedHighTag: QueuePopAttemptCapture<uint>.Captured(capturedHighTag),
            CapturedHead: head ?? QueuePopAttemptCapture<uint>.Captured(1),
            DiagnosticReturned:
                diagnosticReturned ?? QueuePopAttemptCapture<bool>.Unreached(),
            CapturedNextHead: next ?? QueuePopAttemptCapture<uint>.Unreached(),
            LaterHighTag: laterTag ?? QueuePopAttemptCapture<uint>.Unreached(),
            CompareExchangeReturnedHead:
                returnedHead ?? QueuePopAttemptCapture<uint>.Unreached(),
            CompareExchangeReturnedHighTag:
                returnedHighTag ?? QueuePopAttemptCapture<uint>.Unreached());

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
