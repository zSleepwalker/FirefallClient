namespace Firefall.Reconstruction.Startup;

public enum QueueAppendAttemptCaptureKind
{
    Missing,
    Unreached,
    Captured
}

public readonly struct QueueAppendAttemptCapture<T> where T : struct
{
    private QueueAppendAttemptCapture(QueueAppendAttemptCaptureKind kind, T? value)
    {
        Kind = kind;
        Value = value;
    }

    public QueueAppendAttemptCaptureKind Kind { get; }
    public T? Value { get; }

    public static QueueAppendAttemptCapture<T> Missing() =>
        new(QueueAppendAttemptCaptureKind.Missing, null);

    public static QueueAppendAttemptCapture<T> Unreached() =>
        new(QueueAppendAttemptCaptureKind.Unreached, null);

    public static QueueAppendAttemptCapture<T> Captured(T value) =>
        new(QueueAppendAttemptCaptureKind.Captured, value);
}

public readonly record struct QueueAppendAttemptPair(uint Tail, uint Tag);

public readonly record struct QueueAppendAttemptCaptures(
    uint ReceiverAddress,
    uint CapturedNewNode,
    QueueAppendAttemptCapture<uint> CapturedTag,
    QueueAppendAttemptCapture<uint> CapturedTail,
    QueueAppendAttemptCapture<bool> DiagnosticReturned,
    QueueAppendAttemptCapture<uint> CompareExchangeReturnedEax,
    QueueAppendAttemptCapture<uint> CompareExchangeReturnedEdx);

public enum QueueAppendAttemptOutcome
{
    MissingDiagnosticReturnBoundary,
    DiagnosticDidNotReturnBoundary,
    MissingCompareExchangeEaxBoundary,
    RetryBoundary,
    MissingCompareExchangeEdxBoundary,
    SuccessBoundary
}

public enum QueueAppendAttemptActionKind
{
    CaptureReceiverTag,
    CaptureReceiverTail,
    ReportNullTailDiagnostic,
    StopAtMissingDiagnosticReturnBoundary,
    StopAtDiagnosticDidNotReturnBoundary,
    RestoreSavedTailIntoEsi,
    AttemptLockedCompareExchange,
    CompareReturnedEax,
    RestoreCapturedNewNodeIntoEax,
    RestoreReceiverIntoEbx,
    RestorePairPointerIntoEcx,
    StopAtMissingCompareExchangeEaxBoundary,
    StopAtRetryBoundary,
    CompareReturnedEdx,
    StopAtMissingCompareExchangeEdxBoundary,
    StopAtSuccessBoundary
}

public readonly record struct QueueAppendAttemptAction(
    QueueAppendAttemptActionKind Kind,
    uint InstructionRawFileOffset,
    uint? Value = null,
    uint? DiagnosticId = null,
    QueueAppendAttemptPair? ExpectedPair = null,
    QueueAppendAttemptPair? ReplacementPair = null,
    uint? BranchTargetRawFileOffset = null);

public readonly record struct QueueAppendAttemptPlan(
    QueueAppendAttemptCaptures Captures,
    QueueAppendAttemptOutcome Outcome,
    IReadOnlyList<QueueAppendAttemptAction> Actions,
    uint ReceiverPairPointer,
    QueueAppendAttemptPair? ExpectedPair,
    QueueAppendAttemptPair? ReplacementPair,
    QueueAppendAttemptPair? CompareExchangeReturnedPair);

public readonly record struct QueueAppendAttemptInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects exactly one captured queue-append CMPXCHG8B attempt. It does not
/// model allocation, native memory, an atomic whole-operation, or retrying.
/// </summary>
public static class QueueAppendAttemptProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";

    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const string IdaAddressMapping =
        "Base-zero IDA EAs are raw file offsets; code VA is raw + 0x400C00, not RVA.";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint AttemptStartRawFileOffset = 0x12CE364;
    public const uint AttemptStartVirtualAddress =
        AttemptStartRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint AttemptEndRawFileOffsetExclusive = 0x12CE3C3;
    public const uint AttemptEndVirtualAddressExclusive =
        AttemptEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint TagCaptureRawFileOffset = 0x12CE364;
    public const int TagOffset = 0x0C;
    public const uint TailCaptureRawFileOffset = 0x12CE367;
    public const int TailOffset = 8;
    public const uint SavedTailStackStoreRawFileOffset = 0x12CE369;
    public const uint NullTailTestRawFileOffset = 0x12CE36D;
    public const uint NullTailDiagnosticBranchRawFileOffset = 0x12CE36F;
    public const uint DiagnosticId = 0x2EC;
    public const uint DiagnosticCallRawFileOffset = 0x12CE381;
    public const uint DiagnosticCallCellVirtualAddress = 0x01C15EA0;
    public const uint DiagnosticReturnBoundaryRawFileOffset = 0x12CE387;
    public const uint RestoreNewNodeIntoEaxRawFileOffset = 0x12CE387;

    public const uint CasExpectedTagSetupRawFileOffset = 0x12CE38E;
    public const uint CasDesiredTagSetupRawFileOffset = 0x12CE390;
    public const uint SavedTagStoreRawFileOffset = 0x12CE395;
    public const uint CasExpectedTailSetupRawFileOffset = 0x12CE39B;
    public const uint CasDesiredTailSetupRawFileOffset = 0x12CE39D;
    public const uint CasExpectedEaxSetupRawFileOffset = 0x12CE39F;
    public const uint CasPairPointerLoadRawFileOffset = 0x12CE3A1;
    public const uint CompareExchangeRawFileOffset = 0x12CE3A5;
    public const uint RestoreSavedTailIntoEsiRawFileOffset = 0x12CE3A9;
    public const uint CompareReturnedEaxRawFileOffset = 0x12CE3AD;
    public const uint RestoreNewNodeEaxRawFileOffset = 0x12CE3AF;
    public const uint RestoreReceiverEbxRawFileOffset = 0x12CE3B3;
    public const uint RestorePairPointerEcxRawFileOffset = 0x12CE3B7;
    public const uint CompareReturnedEaxRetryBranchRawFileOffset = 0x12CE3BB;
    public const uint CompareReturnedEdxRawFileOffset = 0x12CE3BD;
    public const uint CompareReturnedEdxRetryBranchRawFileOffset = 0x12CE3C1;
    public const uint RetryTargetRawFileOffset = AttemptStartRawFileOffset;
    public const uint SuccessBoundaryRawFileOffset = AttemptEndRawFileOffsetExclusive;
    public const int PairTailOffset = 0;
    public const int PairTagOffset = 4;

    public const string CapturedAttemptBytes =
        "8B 5B 0C 8B 31 89 74 24 18 85 F6 75 1D 56 68 EC 02 00 00 " +
        "68 DF 42 B7 01 68 03 43 B7 01 FF 15 A0 5E C1 01 8B 44 24 1C " +
        "83 C4 10 8B D3 8D 4B 01 33 FF 89 54 24 1C 33 DB 0B FE 0B D8 " +
        "8B C7 8B 74 24 10 F0 0F C7 0E 8B 74 24 18 3B C7 8B 44 24 0C " +
        "8B 5C 24 14 8B 4C 24 10 75 A7 3B 54 24 1C 75 A1";

    public static IReadOnlyList<QueueAppendAttemptInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new QueueAppendAttemptInstructionEvidence(
                "Capture receiver + 0x0C tag before the separate receiver + 8 tail read",
                TagCaptureRawFileOffset,
                "8B 5B 0C"),
            new QueueAppendAttemptInstructionEvidence(
                "Capture the receiver + 8 tail separately and save it",
                TailCaptureRawFileOffset,
                "8B 31"),
            new QueueAppendAttemptInstructionEvidence(
                "Save the captured tail for post-CAS register restoration",
                SavedTailStackStoreRawFileOffset,
                "89 74 24 18"),
            new QueueAppendAttemptInstructionEvidence(
                "Test the captured tail",
                NullTailTestRawFileOffset,
                "85 F6"),
            new QueueAppendAttemptInstructionEvidence(
                "Skip diagnostic 0x2EC only when the captured tail is nonzero",
                NullTailDiagnosticBranchRawFileOffset,
                "75 1D",
                0x12CE38E),
            new QueueAppendAttemptInstructionEvidence(
                "Call the diagnostic through the indirect cell with report ID 0x2EC",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                TargetVirtualAddress: DiagnosticCallCellVirtualAddress),
            new QueueAppendAttemptInstructionEvidence(
                "Reload the captured new node after a returning diagnostic",
                RestoreNewNodeIntoEaxRawFileOffset,
                "8B 44 24 1C"),
            new QueueAppendAttemptInstructionEvidence(
                "Set the expected high/tag half from the first receiver + 0x0C capture",
                CasExpectedTagSetupRawFileOffset,
                "8B D3"),
            new QueueAppendAttemptInstructionEvidence(
                "Set the desired tag to the captured tag plus one with DWORD wrap",
                CasDesiredTagSetupRawFileOffset,
                "8D 4B 01"),
            new QueueAppendAttemptInstructionEvidence(
                "Save the expected tag for its later returned-EDX comparison",
                SavedTagStoreRawFileOffset,
                "89 54 24 1C"),
            new QueueAppendAttemptInstructionEvidence(
                "Form the expected low half from the saved tail",
                CasExpectedTailSetupRawFileOffset,
                "0B FE"),
            new QueueAppendAttemptInstructionEvidence(
                "Form the desired low half from the captured new node",
                CasDesiredTailSetupRawFileOffset,
                "0B D8"),
            new QueueAppendAttemptInstructionEvidence(
                "Load the expected low half into EAX",
                CasExpectedEaxSetupRawFileOffset,
                "8B C7"),
            new QueueAppendAttemptInstructionEvidence(
                "Load the receiver + 8 pair address",
                CasPairPointerLoadRawFileOffset,
                "8B 74 24 10"),
            new QueueAppendAttemptInstructionEvidence(
                "Attempt one locked CMPXCHG8B against receiver + 8",
                CompareExchangeRawFileOffset,
                "F0 0F C7 0E"),
            new QueueAppendAttemptInstructionEvidence(
                "Restore the saved tail into ESI after CMPXCHG8B",
                RestoreSavedTailIntoEsiRawFileOffset,
                "8B 74 24 18"),
            new QueueAppendAttemptInstructionEvidence(
                "Compare returned EAX with the captured tail",
                CompareReturnedEaxRawFileOffset,
                "3B C7"),
            new QueueAppendAttemptInstructionEvidence(
                "Restore the captured new node into EAX before the first retry branch",
                RestoreNewNodeEaxRawFileOffset,
                "8B 44 24 0C"),
            new QueueAppendAttemptInstructionEvidence(
                "Restore the receiver into EBX",
                RestoreReceiverEbxRawFileOffset,
                "8B 5C 24 14"),
            new QueueAppendAttemptInstructionEvidence(
                "Restore receiver + 8 into ECX",
                RestorePairPointerEcxRawFileOffset,
                "8B 4C 24 10"),
            new QueueAppendAttemptInstructionEvidence(
                "Retry from the separate captures when returned EAX differs",
                CompareReturnedEaxRetryBranchRawFileOffset,
                "75 A7",
                RetryTargetRawFileOffset),
            new QueueAppendAttemptInstructionEvidence(
                "Only after returned EAX matches, compare returned EDX with saved tag",
                CompareReturnedEdxRawFileOffset,
                "3B 54 24 1C"),
            new QueueAppendAttemptInstructionEvidence(
                "Retry from the separate captures when returned EDX differs",
                CompareReturnedEdxRetryBranchRawFileOffset,
                "75 A1",
                RetryTargetRawFileOffset)
        });

    public static QueueAppendAttemptPlan Project(QueueAppendAttemptCaptures captures)
    {
        uint capturedTag = RequireCaptured(captures.CapturedTag, nameof(captures.CapturedTag));
        uint capturedTail = RequireCaptured(captures.CapturedTail, nameof(captures.CapturedTail));
        uint pairPointer = unchecked(captures.ReceiverAddress + (uint)TailOffset);
        var actions = new List<QueueAppendAttemptAction>
        {
            new(
                QueueAppendAttemptActionKind.CaptureReceiverTag,
                TagCaptureRawFileOffset,
                capturedTag),
            new(
                QueueAppendAttemptActionKind.CaptureReceiverTail,
                TailCaptureRawFileOffset,
                capturedTail)
        };

        if (capturedTail == 0)
        {
            actions.Add(new QueueAppendAttemptAction(
                QueueAppendAttemptActionKind.ReportNullTailDiagnostic,
                DiagnosticCallRawFileOffset,
                DiagnosticId: DiagnosticId));

            if (captures.DiagnosticReturned.Kind == QueueAppendAttemptCaptureKind.Missing)
            {
                RequireUnreached(
                    captures.CompareExchangeReturnedEax,
                    nameof(captures.CompareExchangeReturnedEax));
                RequireUnreached(
                    captures.CompareExchangeReturnedEdx,
                    nameof(captures.CompareExchangeReturnedEdx));
                actions.Add(new QueueAppendAttemptAction(
                    QueueAppendAttemptActionKind.StopAtMissingDiagnosticReturnBoundary,
                    DiagnosticReturnBoundaryRawFileOffset));
                return Plan(
                    captures,
                    QueueAppendAttemptOutcome.MissingDiagnosticReturnBoundary,
                    actions,
                    pairPointer);
            }

            if (captures.DiagnosticReturned.Kind == QueueAppendAttemptCaptureKind.Unreached)
            {
                throw InvalidCapture(
                    nameof(captures.DiagnosticReturned),
                    "A null captured tail reaches the diagnostic call.");
            }

            if (!captures.DiagnosticReturned.Value!.Value)
            {
                RequireUnreached(
                    captures.CompareExchangeReturnedEax,
                    nameof(captures.CompareExchangeReturnedEax));
                RequireUnreached(
                    captures.CompareExchangeReturnedEdx,
                    nameof(captures.CompareExchangeReturnedEdx));
                actions.Add(new QueueAppendAttemptAction(
                    QueueAppendAttemptActionKind.StopAtDiagnosticDidNotReturnBoundary,
                    DiagnosticReturnBoundaryRawFileOffset));
                return Plan(
                    captures,
                    QueueAppendAttemptOutcome.DiagnosticDidNotReturnBoundary,
                    actions,
                    pairPointer);
            }
        }
        else
        {
            RequireUnreached(
                captures.DiagnosticReturned,
                nameof(captures.DiagnosticReturned));
        }

        var expectedPair = new QueueAppendAttemptPair(capturedTail, capturedTag);
        var replacementPair = new QueueAppendAttemptPair(
            captures.CapturedNewNode,
            unchecked(capturedTag + 1));

        actions.Add(new QueueAppendAttemptAction(
            QueueAppendAttemptActionKind.AttemptLockedCompareExchange,
            CompareExchangeRawFileOffset,
            pairPointer,
            ExpectedPair: expectedPair,
            ReplacementPair: replacementPair));
        actions.Add(new QueueAppendAttemptAction(
            QueueAppendAttemptActionKind.RestoreSavedTailIntoEsi,
            RestoreSavedTailIntoEsiRawFileOffset,
            capturedTail));

        if (captures.CompareExchangeReturnedEax.Kind == QueueAppendAttemptCaptureKind.Missing)
        {
            RequireUnreached(
                captures.CompareExchangeReturnedEdx,
                nameof(captures.CompareExchangeReturnedEdx));
            actions.Add(new QueueAppendAttemptAction(
                QueueAppendAttemptActionKind.StopAtMissingCompareExchangeEaxBoundary,
                CompareReturnedEaxRawFileOffset));
            return Plan(
                captures,
                QueueAppendAttemptOutcome.MissingCompareExchangeEaxBoundary,
                actions,
                pairPointer,
                expectedPair,
                replacementPair);
        }

        uint returnedEax = RequireCaptured(
            captures.CompareExchangeReturnedEax,
            nameof(captures.CompareExchangeReturnedEax));
        actions.Add(new QueueAppendAttemptAction(
            QueueAppendAttemptActionKind.CompareReturnedEax,
            CompareReturnedEaxRawFileOffset,
            returnedEax));
        actions.Add(new QueueAppendAttemptAction(
            QueueAppendAttemptActionKind.RestoreCapturedNewNodeIntoEax,
            RestoreNewNodeEaxRawFileOffset,
            captures.CapturedNewNode));
        actions.Add(new QueueAppendAttemptAction(
            QueueAppendAttemptActionKind.RestoreReceiverIntoEbx,
            RestoreReceiverEbxRawFileOffset,
            captures.ReceiverAddress));
        actions.Add(new QueueAppendAttemptAction(
            QueueAppendAttemptActionKind.RestorePairPointerIntoEcx,
            RestorePairPointerEcxRawFileOffset,
            pairPointer));

        if (returnedEax != capturedTail)
        {
            RequireUnreached(
                captures.CompareExchangeReturnedEdx,
                nameof(captures.CompareExchangeReturnedEdx));
            actions.Add(new QueueAppendAttemptAction(
                QueueAppendAttemptActionKind.StopAtRetryBoundary,
                CompareReturnedEaxRetryBranchRawFileOffset,
                BranchTargetRawFileOffset: RetryTargetRawFileOffset));
            return Plan(
                captures,
                QueueAppendAttemptOutcome.RetryBoundary,
                actions,
                pairPointer,
                expectedPair,
                replacementPair);
        }

        if (captures.CompareExchangeReturnedEdx.Kind == QueueAppendAttemptCaptureKind.Missing)
        {
            actions.Add(new QueueAppendAttemptAction(
                QueueAppendAttemptActionKind.StopAtMissingCompareExchangeEdxBoundary,
                CompareReturnedEdxRawFileOffset));
            return Plan(
                captures,
                QueueAppendAttemptOutcome.MissingCompareExchangeEdxBoundary,
                actions,
                pairPointer,
                expectedPair,
                replacementPair);
        }

        uint returnedEdx = RequireCaptured(
            captures.CompareExchangeReturnedEdx,
            nameof(captures.CompareExchangeReturnedEdx));
        var returnedPair = new QueueAppendAttemptPair(returnedEax, returnedEdx);
        actions.Add(new QueueAppendAttemptAction(
            QueueAppendAttemptActionKind.CompareReturnedEdx,
            CompareReturnedEdxRawFileOffset,
            returnedEdx));

        if (returnedEdx != capturedTag)
        {
            actions.Add(new QueueAppendAttemptAction(
                QueueAppendAttemptActionKind.StopAtRetryBoundary,
                CompareReturnedEdxRetryBranchRawFileOffset,
                BranchTargetRawFileOffset: RetryTargetRawFileOffset));
            return Plan(
                captures,
                QueueAppendAttemptOutcome.RetryBoundary,
                actions,
                pairPointer,
                expectedPair,
                replacementPair,
                returnedPair);
        }

        actions.Add(new QueueAppendAttemptAction(
            QueueAppendAttemptActionKind.StopAtSuccessBoundary,
            SuccessBoundaryRawFileOffset));
        return Plan(
            captures,
            QueueAppendAttemptOutcome.SuccessBoundary,
            actions,
            pairPointer,
            expectedPair,
            replacementPair,
            returnedPair);
    }

    private static QueueAppendAttemptPlan Plan(
        QueueAppendAttemptCaptures captures,
        QueueAppendAttemptOutcome outcome,
        List<QueueAppendAttemptAction> actions,
        uint pairPointer,
        QueueAppendAttemptPair? expectedPair = null,
        QueueAppendAttemptPair? replacementPair = null,
        QueueAppendAttemptPair? returnedPair = null) =>
        new(
            captures,
            outcome,
            Array.AsReadOnly(actions.ToArray()),
            pairPointer,
            expectedPair,
            replacementPair,
            returnedPair);

    private static T RequireCaptured<T>(
        QueueAppendAttemptCapture<T> capture,
        string parameterName) where T : struct
    {
        if (capture.Kind != QueueAppendAttemptCaptureKind.Captured)
        {
            throw InvalidCapture(parameterName, "A reached capture is required.");
        }

        return capture.Value!.Value;
    }

    private static void RequireUnreached<T>(
        QueueAppendAttemptCapture<T> capture,
        string parameterName) where T : struct
    {
        if (capture.Kind != QueueAppendAttemptCaptureKind.Unreached)
        {
            throw InvalidCapture(parameterName, "This capture is not reached on the selected path.");
        }
    }

    private static ArgumentException InvalidCapture(string parameterName, string message) =>
        new(message, parameterName);
}
