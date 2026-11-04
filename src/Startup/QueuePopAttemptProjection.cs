namespace Firefall.Reconstruction.Startup;

public enum QueuePopAttemptCaptureKind
{
    Missing,
    Unreached,
    Captured
}

public readonly struct QueuePopAttemptCapture<T> where T : struct
{
    private QueuePopAttemptCapture(QueuePopAttemptCaptureKind kind, T? value)
    {
        Kind = kind;
        Value = value;
    }

    public QueuePopAttemptCaptureKind Kind { get; }
    public T? Value { get; }

    public static QueuePopAttemptCapture<T> Missing() =>
        new(QueuePopAttemptCaptureKind.Missing, null);

    public static QueuePopAttemptCapture<T> Unreached() =>
        new(QueuePopAttemptCaptureKind.Unreached, null);

    public static QueuePopAttemptCapture<T> Captured(T value) =>
        new(QueuePopAttemptCaptureKind.Captured, value);
}

public readonly record struct QueuePopAttemptPair(uint Head, uint HighTag);

public readonly record struct QueuePopAttemptCaptures(
    uint ReceiverPairPointer,
    QueuePopAttemptCapture<uint> CapturedHighTag,
    QueuePopAttemptCapture<uint> CapturedHead,
    QueuePopAttemptCapture<bool> DiagnosticReturned,
    QueuePopAttemptCapture<uint> CapturedNextHead,
    QueuePopAttemptCapture<uint> LaterHighTag,
    QueuePopAttemptCapture<uint> CompareExchangeReturnedHead,
    QueuePopAttemptCapture<uint> CompareExchangeReturnedHighTag);

public enum QueuePopAttemptOutcome
{
    MissingDiagnosticReturnBoundary,
    DiagnosticDidNotReturnBoundary,
    MissingNextHeadBoundary,
    MissingLaterHighTagBoundary,
    ReturnedZeroAfterNullNext,
    RetryBoundary,
    MissingCompareExchangeReturnBoundary,
    MissingCompareExchangeHighTagBoundary,
    SuccessBoundary
}

public enum QueuePopAttemptActionKind
{
    CaptureReceiverHighTag,
    CaptureReceiverHead,
    ReportNullHeadDiagnostic,
    StopAtMissingDiagnosticReturnBoundary,
    StopAtDiagnosticDidNotReturnBoundary,
    ReadCapturedHeadLink,
    StopAtMissingNextHeadBoundary,
    CaptureLaterReceiverHighTag,
    StopAtMissingLaterHighTagBoundary,
    ReturnZeroAfterUnchangedHighTag,
    StopAtUnmodeledRetryBoundary,
    AttemptLockedCompareExchange,
    CompareReturnedHead,
    StopAtMissingCompareExchangeReturnBoundary,
    CompareReturnedHighTag,
    StopAtMissingCompareExchangeHighTagBoundary,
    StopAtSuccessBoundary
}

public readonly record struct QueuePopAttemptAction(
    QueuePopAttemptActionKind Kind,
    uint InstructionRawFileOffset,
    uint? Value = null,
    QueuePopAttemptPair? ExpectedPair = null,
    QueuePopAttemptPair? ReplacementPair = null,
    uint? BranchTargetRawFileOffset = null);

public readonly record struct QueuePopAttemptPlan(
    QueuePopAttemptCaptures Captures,
    QueuePopAttemptOutcome Outcome,
    IReadOnlyList<QueuePopAttemptAction> Actions,
    QueuePopAttemptPair? ExpectedPair,
    QueuePopAttemptPair? ReplacementPair,
    QueuePopAttemptPair? CompareExchangeReturnedPair,
    uint? ReturnValue);

public readonly record struct QueuePopAttemptInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null);

/// <summary>
/// Projects the captured reads and at most one queue-pop compare-exchange
/// attempt. It does not model native memory, the atomic operation, or retries.
/// </summary>
public static class QueuePopAttemptProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x12CE400;
    public const uint EntryVirtualAddress = 0x016CF000;
    public const uint TagCaptureRawFileOffset = 0x12CE410;
    public const uint HeadCaptureRawFileOffset = 0x12CE413;
    public const uint NullHeadTestRawFileOffset = 0x12CE41D;
    public const uint DiagnosticCallRawFileOffset = 0x12CE431;
    public const uint DiagnosticCallCellAddress = 0x01C15EA0;
    public const uint NullHeadDiagnosticReportId = 0x300;
    public const uint CapturedLinkReadRawFileOffset = 0x12CE43E;
    public const uint NextHeadTestRawFileOffset = 0x12CE440;
    public const uint NextHeadBranchRawFileOffset = 0x12CE442;
    public const uint LaterTagCaptureRawFileOffset = 0x12CE444;
    public const uint LaterTagRetryBranchRawFileOffset = 0x12CE447;
    public const uint NullNextReturnRawFileOffset = 0x12CE449;
    public const uint NullNextReturnEndRawFileOffsetExclusive = 0x12CE452;
    public const uint CompareExchangeRawFileOffset = 0x12CE467;
    public const uint CompareReturnedHeadRawFileOffset = 0x12CE46B;
    public const uint CompareReturnedHeadRetryBranchRawFileOffset = 0x12CE46D;
    public const uint CompareReturnedHighTagRawFileOffset = 0x12CE46F;
    public const uint CompareReturnedHighTagRetryBranchRawFileOffset = 0x12CE473;
    public const uint SuccessContinuationBoundaryRawFileOffset = 0x12CE475;
    public const uint FunctionCaptureEndRawFileOffsetExclusive =
        SuccessContinuationBoundaryRawFileOffset;

    public const int PairHeadOffset = 0;
    public const int PairHighTagOffset = 4;

    public const string CapturedFunctionBytes =
        "55 8B EC 83 E4 F8 83 EC 0C 53 56 57 8B F1 8B FF " +
        "8B 46 04 8B 1E 89 44 24 10 89 5C 24 14 85 DB 75 1D " +
        "53 68 00 03 00 00 68 3F 43 B7 01 68 63 43 B7 01 FF 15 " +
        "A0 5E C1 01 8B 44 24 20 83 C4 10 8B 13 85 D2 75 0E " +
        "3B 46 04 75 C7 33 C0 5F 5E 5B 8B E5 5D C3 33 FF 89 " +
        "44 24 10 0B FB 8D 48 01 33 DB 8B C7 0B DA 8B 54 " +
        "24 10 F0 0F C7 0E 3B C7 75 A1 3B 54 24 10 75 9B";

    public static IReadOnlyList<QueuePopAttemptInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new QueuePopAttemptInstructionEvidence(
                "Capture the receiver's high tag DWORD",
                TagCaptureRawFileOffset,
                "8B 46 04"),
            new QueuePopAttemptInstructionEvidence(
                "Capture the receiver's head DWORD separately",
                HeadCaptureRawFileOffset,
                "8B 1E"),
            new QueuePopAttemptInstructionEvidence(
                "Test the captured head",
                NullHeadTestRawFileOffset,
                "85 DB"),
            new QueuePopAttemptInstructionEvidence(
                "Branch around the diagnostic for a non-null captured head",
                0x12CE41F,
                "75 1D",
                CapturedLinkReadRawFileOffset),
            new QueuePopAttemptInstructionEvidence(
                "Pass diagnostic report ID 0x300 on the null-head path",
                0x12CE421,
                "53 68 00 03 00 00"),
            new QueuePopAttemptInstructionEvidence(
                "Call the diagnostic through its indirect cell",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01"),
            new QueuePopAttemptInstructionEvidence(
                "Attempt to read the captured head's next link after the diagnostic returns",
                CapturedLinkReadRawFileOffset,
                "8B 13"),
            new QueuePopAttemptInstructionEvidence(
                "Test the captured next link",
                NextHeadTestRawFileOffset,
                "85 D2"),
            new QueuePopAttemptInstructionEvidence(
                "Skip the null-next tag comparison for a nonzero next link",
                NextHeadBranchRawFileOffset,
                "75 0E",
                NullNextReturnEndRawFileOffsetExclusive),
            new QueuePopAttemptInstructionEvidence(
                "Capture the receiver high tag again only for a null next link",
                LaterTagCaptureRawFileOffset,
                "3B 46 04"),
            new QueuePopAttemptInstructionEvidence(
                "Retry from the separate tag/head captures if the later tag differs",
                LaterTagRetryBranchRawFileOffset,
                "75 C7",
                TagCaptureRawFileOffset),
            new QueuePopAttemptInstructionEvidence(
                "Return zero only when the later tag equals the captured tag",
                NullNextReturnRawFileOffset,
                "33 C0 5F 5E 5B 8B E5 5D C3"),
            new QueuePopAttemptInstructionEvidence(
                "Attempt one locked 64-bit compare-exchange with captured pairs",
                CompareExchangeRawFileOffset,
                "F0 0F C7 0E"),
            new QueuePopAttemptInstructionEvidence(
                "Compare the returned low DWORD against the captured head",
                CompareReturnedHeadRawFileOffset,
                "3B C7"),
            new QueuePopAttemptInstructionEvidence(
                "Retry when the returned low DWORD differs",
                CompareReturnedHeadRetryBranchRawFileOffset,
                "75 A1",
                TagCaptureRawFileOffset),
            new QueuePopAttemptInstructionEvidence(
                "Compare the returned high DWORD only after the low DWORD matches",
                CompareReturnedHighTagRawFileOffset,
                "3B 54 24 10"),
            new QueuePopAttemptInstructionEvidence(
                "Retry when the returned high DWORD differs",
                CompareReturnedHighTagRetryBranchRawFileOffset,
                "75 9B",
                TagCaptureRawFileOffset)
        });

    public static QueuePopAttemptPlan Project(QueuePopAttemptCaptures captures)
    {
        uint capturedHighTag = RequireCaptured(
            captures.CapturedHighTag,
            nameof(captures.CapturedHighTag));
        uint capturedHead = RequireCaptured(
            captures.CapturedHead,
            nameof(captures.CapturedHead));
        var actions = new List<QueuePopAttemptAction>
        {
            new(
                QueuePopAttemptActionKind.CaptureReceiverHighTag,
                TagCaptureRawFileOffset,
                capturedHighTag),
            new(
                QueuePopAttemptActionKind.CaptureReceiverHead,
                HeadCaptureRawFileOffset,
                capturedHead)
        };

        if (capturedHead == 0)
        {
            actions.Add(new QueuePopAttemptAction(
                QueuePopAttemptActionKind.ReportNullHeadDiagnostic,
                DiagnosticCallRawFileOffset,
                NullHeadDiagnosticReportId));

            if (captures.DiagnosticReturned.Kind == QueuePopAttemptCaptureKind.Missing)
            {
                RequireUnreached(captures.CapturedNextHead, nameof(captures.CapturedNextHead));
                RequireUnreached(captures.LaterHighTag, nameof(captures.LaterHighTag));
                RequireUnreached(
                    captures.CompareExchangeReturnedHead,
                    nameof(captures.CompareExchangeReturnedHead));
                RequireUnreached(
                    captures.CompareExchangeReturnedHighTag,
                    nameof(captures.CompareExchangeReturnedHighTag));
                actions.Add(new QueuePopAttemptAction(
                    QueuePopAttemptActionKind.StopAtMissingDiagnosticReturnBoundary,
                    DiagnosticCallRawFileOffset));
                return Plan(
                    captures,
                    QueuePopAttemptOutcome.MissingDiagnosticReturnBoundary,
                    actions);
            }

            if (captures.DiagnosticReturned.Kind == QueuePopAttemptCaptureKind.Unreached)
            {
                throw InvalidCapture(
                    nameof(captures.DiagnosticReturned),
                    "The null-head path reaches the diagnostic call.");
            }

            bool diagnosticReturned = captures.DiagnosticReturned.Value!.Value;
            if (!diagnosticReturned)
            {
                RequireUnreached(captures.CapturedNextHead, nameof(captures.CapturedNextHead));
                RequireUnreached(captures.LaterHighTag, nameof(captures.LaterHighTag));
                RequireUnreached(
                    captures.CompareExchangeReturnedHead,
                    nameof(captures.CompareExchangeReturnedHead));
                RequireUnreached(
                    captures.CompareExchangeReturnedHighTag,
                    nameof(captures.CompareExchangeReturnedHighTag));
                actions.Add(new QueuePopAttemptAction(
                    QueuePopAttemptActionKind.StopAtDiagnosticDidNotReturnBoundary,
                    DiagnosticCallRawFileOffset));
                return Plan(
                    captures,
                    QueuePopAttemptOutcome.DiagnosticDidNotReturnBoundary,
                    actions);
            }
        }
        else if (captures.DiagnosticReturned.Kind != QueuePopAttemptCaptureKind.Unreached)
        {
            throw InvalidCapture(
                nameof(captures.DiagnosticReturned),
                "A non-null captured head skips the diagnostic call.");
        }

        if (captures.CapturedNextHead.Kind == QueuePopAttemptCaptureKind.Missing)
        {
            RequireUnreached(captures.LaterHighTag, nameof(captures.LaterHighTag));
            RequireUnreached(
                captures.CompareExchangeReturnedHead,
                nameof(captures.CompareExchangeReturnedHead));
            RequireUnreached(
                captures.CompareExchangeReturnedHighTag,
                nameof(captures.CompareExchangeReturnedHighTag));
            actions.Add(new QueuePopAttemptAction(
                QueuePopAttemptActionKind.StopAtMissingNextHeadBoundary,
                CapturedLinkReadRawFileOffset));
            return Plan(
                captures,
                QueuePopAttemptOutcome.MissingNextHeadBoundary,
                actions);
        }

        uint capturedNextHead = RequireCaptured(
            captures.CapturedNextHead,
            nameof(captures.CapturedNextHead));
        actions.Add(new QueuePopAttemptAction(
            QueuePopAttemptActionKind.ReadCapturedHeadLink,
            CapturedLinkReadRawFileOffset,
            capturedHead));

        if (capturedNextHead == 0)
        {
            RequireUnreached(
                captures.CompareExchangeReturnedHead,
                nameof(captures.CompareExchangeReturnedHead));
            RequireUnreached(
                captures.CompareExchangeReturnedHighTag,
                nameof(captures.CompareExchangeReturnedHighTag));

            if (captures.LaterHighTag.Kind == QueuePopAttemptCaptureKind.Missing)
            {
                actions.Add(new QueuePopAttemptAction(
                    QueuePopAttemptActionKind.StopAtMissingLaterHighTagBoundary,
                    LaterTagCaptureRawFileOffset));
                return Plan(
                    captures,
                    QueuePopAttemptOutcome.MissingLaterHighTagBoundary,
                    actions);
            }

            uint laterHighTag = RequireCaptured(
                captures.LaterHighTag,
                nameof(captures.LaterHighTag));
            actions.Add(new QueuePopAttemptAction(
                QueuePopAttemptActionKind.CaptureLaterReceiverHighTag,
                LaterTagCaptureRawFileOffset,
                laterHighTag));

            if (laterHighTag != capturedHighTag)
            {
                actions.Add(new QueuePopAttemptAction(
                    QueuePopAttemptActionKind.StopAtUnmodeledRetryBoundary,
                    LaterTagRetryBranchRawFileOffset,
                    BranchTargetRawFileOffset: TagCaptureRawFileOffset));
                return Plan(captures, QueuePopAttemptOutcome.RetryBoundary, actions);
            }

            actions.Add(new QueuePopAttemptAction(
                QueuePopAttemptActionKind.ReturnZeroAfterUnchangedHighTag,
                NullNextReturnRawFileOffset,
                Value: 0));
            return Plan(
                captures,
                QueuePopAttemptOutcome.ReturnedZeroAfterNullNext,
                actions,
                returnValue: 0);
        }

        RequireUnreached(captures.LaterHighTag, nameof(captures.LaterHighTag));
        var expectedPair = new QueuePopAttemptPair(capturedHead, capturedHighTag);
        var replacementPair = new QueuePopAttemptPair(
            capturedNextHead,
            unchecked(capturedHighTag + 1));
        actions.Add(new QueuePopAttemptAction(
            QueuePopAttemptActionKind.AttemptLockedCompareExchange,
            CompareExchangeRawFileOffset,
            captures.ReceiverPairPointer,
            expectedPair,
            replacementPair));

        if (captures.CompareExchangeReturnedHead.Kind == QueuePopAttemptCaptureKind.Missing)
        {
            RequireUnreached(
                captures.CompareExchangeReturnedHighTag,
                nameof(captures.CompareExchangeReturnedHighTag));
            actions.Add(new QueuePopAttemptAction(
                QueuePopAttemptActionKind.StopAtMissingCompareExchangeReturnBoundary,
                CompareReturnedHeadRawFileOffset));
            return Plan(
                captures,
                QueuePopAttemptOutcome.MissingCompareExchangeReturnBoundary,
                actions,
                expectedPair,
                replacementPair);
        }

        uint returnedHead = RequireCaptured(
            captures.CompareExchangeReturnedHead,
            nameof(captures.CompareExchangeReturnedHead));
        actions.Add(new QueuePopAttemptAction(
            QueuePopAttemptActionKind.CompareReturnedHead,
            CompareReturnedHeadRawFileOffset,
            returnedHead));
        if (returnedHead != capturedHead)
        {
            RequireUnreached(
                captures.CompareExchangeReturnedHighTag,
                nameof(captures.CompareExchangeReturnedHighTag));
            actions.Add(new QueuePopAttemptAction(
                QueuePopAttemptActionKind.StopAtUnmodeledRetryBoundary,
                CompareReturnedHeadRetryBranchRawFileOffset,
                BranchTargetRawFileOffset: TagCaptureRawFileOffset));
            return Plan(
                captures,
                QueuePopAttemptOutcome.RetryBoundary,
                actions,
                expectedPair,
                replacementPair);
        }

        if (captures.CompareExchangeReturnedHighTag.Kind == QueuePopAttemptCaptureKind.Missing)
        {
            actions.Add(new QueuePopAttemptAction(
                QueuePopAttemptActionKind.StopAtMissingCompareExchangeHighTagBoundary,
                CompareReturnedHighTagRawFileOffset));
            return Plan(
                captures,
                QueuePopAttemptOutcome.MissingCompareExchangeHighTagBoundary,
                actions,
                expectedPair,
                replacementPair);
        }

        uint returnedHighTag = RequireCaptured(
            captures.CompareExchangeReturnedHighTag,
            nameof(captures.CompareExchangeReturnedHighTag));
        var returnedPair = new QueuePopAttemptPair(returnedHead, returnedHighTag);
        actions.Add(new QueuePopAttemptAction(
            QueuePopAttemptActionKind.CompareReturnedHighTag,
            CompareReturnedHighTagRawFileOffset,
            returnedHighTag));

        if (returnedHighTag != capturedHighTag)
        {
            actions.Add(new QueuePopAttemptAction(
                QueuePopAttemptActionKind.StopAtUnmodeledRetryBoundary,
                CompareReturnedHighTagRetryBranchRawFileOffset,
                BranchTargetRawFileOffset: TagCaptureRawFileOffset));
            return Plan(
                captures,
                QueuePopAttemptOutcome.RetryBoundary,
                actions,
                expectedPair,
                replacementPair,
                returnedPair);
        }

        actions.Add(new QueuePopAttemptAction(
            QueuePopAttemptActionKind.StopAtSuccessBoundary,
            SuccessContinuationBoundaryRawFileOffset));
        return Plan(
            captures,
            QueuePopAttemptOutcome.SuccessBoundary,
            actions,
            expectedPair,
            replacementPair,
            returnedPair);
    }

    private static QueuePopAttemptPlan Plan(
        QueuePopAttemptCaptures captures,
        QueuePopAttemptOutcome outcome,
        List<QueuePopAttemptAction> actions,
        QueuePopAttemptPair? expectedPair = null,
        QueuePopAttemptPair? replacementPair = null,
        QueuePopAttemptPair? returnedPair = null,
        uint? returnValue = null) =>
        new(
            captures,
            outcome,
            Array.AsReadOnly(actions.ToArray()),
            expectedPair,
            replacementPair,
            returnedPair,
            returnValue);

    private static uint RequireCaptured(
        QueuePopAttemptCapture<uint> capture,
        string parameterName)
    {
        if (capture.Kind != QueuePopAttemptCaptureKind.Captured)
        {
            throw InvalidCapture(parameterName, "A reached DWORD capture is required.");
        }

        return capture.Value!.Value;
    }

    private static void RequireUnreached(
        QueuePopAttemptCapture<uint> capture,
        string parameterName)
    {
        if (capture.Kind != QueuePopAttemptCaptureKind.Unreached)
        {
            throw InvalidCapture(parameterName, "This capture is not reached on the selected path.");
        }
    }

    private static ArgumentException InvalidCapture(string parameterName, string message) =>
        new(message, parameterName);
}
