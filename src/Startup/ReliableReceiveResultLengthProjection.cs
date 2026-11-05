using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum ReliableReceiveResultLengthOutcome
{
    ResultOutcomeNotAdmitted,
    MissingLengthCapture,
    LengthEntryBoundaryNotValidated,
    LengthCallbackStopped,
    LengthCallbackReturnedNormally
}

public readonly record struct ReliableReceiveResultLengthPlan(
    ReliableReceiveCallbackResultPlan ResultPlan,
    ReliableReceiveLengthCallbackCapture? LengthCapture,
    ReliableReceiveLengthCallbackPlan? LengthPlan,
    ReliableReceiveResultLengthOutcome Outcome,
    bool ResultAdmittedLengthBoundary,
    bool? LengthEntryBoundaryValidated,
    bool DiagnosticCleanupProjected,
    ushort? DiagnosticCleanupStackBytes,
    uint? LengthEntryRawFileOffset,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveResultLengthInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Composes the callback-result gate with the length callback. The zero route
/// includes its visible stack cleanup; payload identity is not inferred.
/// </summary>
public static class ReliableReceiveResultLengthProjection
{
    public const string SamplePath =
        ReliableReceiveCallbackResultProjection.SamplePath;
    public const string SampleSha256 =
        ReliableReceiveCallbackResultProjection.SampleSha256;

    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        ReliableReceiveCallbackResultProjection.CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint DiagnosticCleanupRawFileOffset = 0x00BE53BC;
    public const uint DiagnosticCleanupStackBytes = 0x10;
    public const uint LengthEntryRawFileOffset = 0x00BE53BF;
    public const uint LengthEntryVirtualAddress =
        LengthEntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EndRawFileOffsetExclusive =
        ReliableReceiveLengthCallbackProjection.NormalCallBoundaryRawFileOffset;

    public const string DiagnosticCleanupBytes = "83 C4 10";

    public static ImmutableArray<ReliableReceiveResultLengthInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveResultLengthInstructionEvidence(
                "After a normal zero-result diagnostic return, add 0x10 to ESP before the length entry",
                DiagnosticCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(DiagnosticCleanupRawFileOffset),
                DiagnosticCleanupBytes));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static ReliableReceiveResultLengthPlan Project(
        ReliableReceiveCallbackResultCapture resultCapture,
        ReliableReceiveLengthCallbackCapture? lengthCapture)
    {
        ReliableReceiveCallbackResultPlan resultPlan =
            ReliableReceiveCallbackResultProjection.Project(resultCapture);
        bool resultAdmitsLengthBoundary =
            resultPlan.Outcome
                is ReliableReceiveCallbackResultOutcome.NonzeroEaxBranch
                or ReliableReceiveCallbackResultOutcome.ZeroOpaqueHandoffReturnedNormally;
        if (!resultAdmitsLengthBoundary)
        {
            return Stop(
                resultPlan,
                lengthCapture: null,
                lengthPlan: null,
                outcome: ReliableReceiveResultLengthOutcome.ResultOutcomeNotAdmitted,
                resultAdmittedLengthBoundary: false,
                stopAt: resultPlan.StopAtRawFileOffsetExclusive);
        }

        bool diagnosticCleanupProjected =
            resultPlan.Outcome
                == ReliableReceiveCallbackResultOutcome.ZeroOpaqueHandoffReturnedNormally;
        if (lengthCapture is not ReliableReceiveLengthCallbackCapture capture)
        {
            return Stop(
                resultPlan,
                lengthCapture: null,
                lengthPlan: null,
                outcome: ReliableReceiveResultLengthOutcome.MissingLengthCapture,
                resultAdmittedLengthBoundary: true,
                diagnosticCleanupProjected: diagnosticCleanupProjected,
                stopAt: LengthEntryRawFileOffset);
        }

        ReliableReceiveLengthCallbackPlan lengthPlan =
            ReliableReceiveLengthCallbackProjection.Project(capture);
        if (!capture.EntryBoundaryValidated)
        {
            return Stop(
                resultPlan,
                capture,
                lengthPlan,
                ReliableReceiveResultLengthOutcome.LengthEntryBoundaryNotValidated,
                resultAdmittedLengthBoundary: true,
                diagnosticCleanupProjected: diagnosticCleanupProjected,
                lengthEntryBoundaryValidated: false,
                stopAt: LengthEntryRawFileOffset);
        }

        if (lengthPlan.Outcome
            != ReliableReceiveLengthCallbackOutcome.IndirectCallReturnedNormally)
        {
            return Stop(
                resultPlan,
                capture,
                lengthPlan,
                ReliableReceiveResultLengthOutcome.LengthCallbackStopped,
                resultAdmittedLengthBoundary: true,
                diagnosticCleanupProjected: diagnosticCleanupProjected,
                lengthEntryBoundaryValidated: true,
                stopAt: lengthPlan.StopAtRawFileOffsetExclusive);
        }

        return Stop(
            resultPlan,
            capture,
            lengthPlan,
            ReliableReceiveResultLengthOutcome.LengthCallbackReturnedNormally,
            resultAdmittedLengthBoundary: true,
            diagnosticCleanupProjected: diagnosticCleanupProjected,
            lengthEntryBoundaryValidated: true,
            selectedDestinationRawFileOffset: EndRawFileOffsetExclusive,
            stopAt: EndRawFileOffsetExclusive);
    }

    private static ReliableReceiveResultLengthPlan Stop(
        ReliableReceiveCallbackResultPlan resultPlan,
        ReliableReceiveLengthCallbackCapture? lengthCapture,
        ReliableReceiveLengthCallbackPlan? lengthPlan,
        ReliableReceiveResultLengthOutcome outcome,
        bool resultAdmittedLengthBoundary,
        bool diagnosticCleanupProjected = false,
        bool? lengthEntryBoundaryValidated = null,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = LengthEntryRawFileOffset) =>
        new(
            resultPlan,
            lengthCapture,
            lengthPlan,
            outcome,
            resultAdmittedLengthBoundary,
            lengthEntryBoundaryValidated,
            diagnosticCleanupProjected,
            diagnosticCleanupProjected ? (ushort)DiagnosticCleanupStackBytes : null,
            resultAdmittedLengthBoundary ? LengthEntryRawFileOffset : null,
            selectedDestinationRawFileOffset,
            stopAt);
}
