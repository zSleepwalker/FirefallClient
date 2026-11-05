using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct QueueCleanupEmptyAttemptCapture(
    uint ReceiverPointer,
    uint CapturedInitialTagFromReceiverPlus4,
    uint CapturedInitialHeadFromReceiver,
    bool? NullHeadReporterReturned,
    uint? CapturedHeadNextValue,
    uint? CapturedLaterTagFromReceiverPlus4,
    bool? QueuePopReturnedNormally);

public enum QueueCleanupEmptyAttemptOutcome
{
    NullHeadReporterReturnUnavailable,
    NullHeadReporterDidNotReturn,
    HeadNextCaptureUnavailable,
    LaterTagCaptureUnavailable,
    TagMismatchRetriesLoop,
    EmptyHeadNextEntersSecondaryDrainBoundary,
    QueuePopReturnUnavailableAtCallBoundary,
    QueuePopDidNotReturnAtCallBoundary,
    QueuePopReturnReachesExplicitLoopRepeat
}

public enum QueueCleanupEmptyAttemptActionKind
{
    CaptureInitialReceiverTag,
    CaptureInitialReceiverHead,
    ReportNullHead,
    CaptureAndCompareHeadNextToZero,
    SetHeadNextZeroFlag,
    CaptureAndCompareLaterReceiverTag,
    RetryAtLoopHeader,
    EnterSecondaryDrainBoundary,
    CallQueuePop,
    RepeatAtLoopHeader
}

public readonly record struct QueueCleanupEmptyAttemptAction(
    QueueCleanupEmptyAttemptActionKind Kind,
    uint RawFileOffset,
    uint? CapturedValue = null,
    bool? ConditionMet = null,
    bool? CallReturnedNormally = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

public readonly record struct QueueCleanupEmptyAttemptPlan(
    QueueCleanupEmptyAttemptCapture Capture,
    QueueCleanupEmptyAttemptOutcome Outcome,
    ImmutableArray<QueueCleanupEmptyAttemptAction> Actions,
    bool? HeadNextWasZero,
    bool? LaterTagMatchedInitialTag);

public readonly record struct QueueCleanupEmptyAttemptInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects one captured pass through the cleanup loop. Receiver fields and
/// subsequent observations are independent captures; this performs no memory
/// reads, atomic snapshot, loop execution, or helper-side effects.
/// </summary>
public static class QueueCleanupEmptyAttemptProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint LoopHeaderRawFileOffset = 0x12C0348;
    public const uint LoopEndRawFileOffsetExclusive = 0x12C0382;
    public const uint InitialTagCaptureRawFileOffset = 0x12C0348;
    public const uint InitialHeadCaptureRawFileOffset = 0x12C034B;
    public const uint NullHeadTestRawFileOffset = 0x12C034D;
    public const uint NullHeadDiagnosticBranchRawFileOffset = 0x12C034F;
    public const uint NullHeadDiagnosticIdPushRawFileOffset = 0x12C0352;
    public const uint NullHeadReporterCallRawFileOffset = 0x12C0361;
    public const uint NullHeadReporterFunctionPointerSlotVirtualAddress = 0x1C15EA0;
    public const uint HeadNextCompareRawFileOffset = 0x12C036A;
    public const uint HeadNextSetZeroFlagRawFileOffset = 0x12C036D;
    public const uint LaterTagCompareRawFileOffset = 0x12C0370;
    public const uint TagMismatchBranchRawFileOffset = 0x12C0373;
    public const uint HeadNextZeroTestRawFileOffset = 0x12C0375;
    public const uint EmptyBoundaryBranchRawFileOffset = 0x12C0377;
    public const uint QueuePopReceiverSetupRawFileOffset = 0x12C0379;
    public const uint QueuePopCallRawFileOffset = 0x12C037B;
    public const uint LoopRepeatJumpRawFileOffset = 0x12C0380;
    public const uint SecondaryDrainBoundaryRawFileOffset = 0x12C0382;
    public const uint QueuePopTargetRawFileOffset = 0x12CE400;
    public const uint NullHeadDiagnosticId = 0x324;

    public const uint LoopHeaderVirtualAddress =
        LoopHeaderRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint LoopEndVirtualAddressExclusive =
        LoopEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint QueuePopTargetVirtualAddress =
        QueuePopTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedLoopBytes =
        "8B 5E 04 8B 3E 85 FF 75 19 57 68 24 03 00 00 " +
        "68 57 43 B7 01 68 33 43 B7 01 FF 15 A0 5E C1 01 " +
        "83 C4 10 83 3F 00 0F 94 C0 3B 5E 04 75 D3 84 C0 " +
        "75 09 8B CE E8 80 E0 00 00 EB C6";

    public static IReadOnlyList<QueueCleanupEmptyAttemptInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Capture receiver+4 as the initial tag in EBX",
                InitialTagCaptureRawFileOffset,
                "8B 5E 04"),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Independently capture the receiver head at receiver+0 in EDI",
                InitialHeadCaptureRawFileOffset,
                "8B 3E"),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Test the captured head for null",
                NullHeadTestRawFileOffset,
                "85 FF"),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Skip the null-head reporter when the captured head is nonzero",
                NullHeadDiagnosticBranchRawFileOffset,
                "75 19",
                HeadNextCompareRawFileOffset,
                HeadNextCompareRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Push diagnostic ID 0x324 for a null head",
                NullHeadDiagnosticIdPushRawFileOffset,
                "68 24 03 00 00"),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Call the indirect diagnostic reporter",
                NullHeadReporterCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                TargetVirtualAddress: NullHeadReporterFunctionPointerSlotVirtualAddress),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Compare the captured head's next DWORD with zero",
                HeadNextCompareRawFileOffset,
                "83 3F 00"),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Save the head-next zero comparison in AL",
                HeadNextSetZeroFlagRawFileOffset,
                "0F 94 C0"),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Independently compare the later receiver+4 tag with the original EBX capture",
                LaterTagCompareRawFileOffset,
                "3B 5E 04"),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Retry at the loop header on a later-tag mismatch",
                TagMismatchBranchRawFileOffset,
                "75 D3",
                LoopHeaderRawFileOffset,
                LoopHeaderVirtualAddress),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Test the saved head-next zero flag only after tag agreement",
                HeadNextZeroTestRawFileOffset,
                "84 C0"),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Branch to the secondary-drain boundary when head-next was zero",
                EmptyBoundaryBranchRawFileOffset,
                "75 09",
                SecondaryDrainBoundaryRawFileOffset,
                SecondaryDrainBoundaryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Set the receiver argument for the queue-pop call",
                QueuePopReceiverSetupRawFileOffset,
                "8B CE"),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Call queue pop; its return value is not consumed here",
                QueuePopCallRawFileOffset,
                "E8 80 E0 00 00",
                QueuePopTargetRawFileOffset,
                QueuePopTargetVirtualAddress),
            new QueueCleanupEmptyAttemptInstructionEvidence(
                "Explicitly jump back to the loop header after a normal queue-pop return",
                LoopRepeatJumpRawFileOffset,
                "EB C6",
                LoopHeaderRawFileOffset,
                LoopHeaderVirtualAddress)
        });

    /// <summary>
    /// Consumes only values captured at their respective instructions. A null
    /// return capture means control-flow continuation is not established.
    /// Captures for paths not reached by this pass are rejected.
    /// </summary>
    public static QueueCleanupEmptyAttemptPlan Project(
        QueueCleanupEmptyAttemptCapture capture)
    {
        var actions = ImmutableArray.CreateBuilder<QueueCleanupEmptyAttemptAction>(8);
        actions.Add(new QueueCleanupEmptyAttemptAction(
            QueueCleanupEmptyAttemptActionKind.CaptureInitialReceiverTag,
            InitialTagCaptureRawFileOffset,
            CapturedValue: capture.CapturedInitialTagFromReceiverPlus4));
        actions.Add(new QueueCleanupEmptyAttemptAction(
            QueueCleanupEmptyAttemptActionKind.CaptureInitialReceiverHead,
            InitialHeadCaptureRawFileOffset,
            CapturedValue: capture.CapturedInitialHeadFromReceiver));

        if (capture.CapturedInitialHeadFromReceiver == 0)
        {
            actions.Add(new QueueCleanupEmptyAttemptAction(
                QueueCleanupEmptyAttemptActionKind.ReportNullHead,
                NullHeadReporterCallRawFileOffset,
                CapturedValue: NullHeadDiagnosticId,
                CallReturnedNormally: capture.NullHeadReporterReturned));
            if (capture.NullHeadReporterReturned is null or false)
            {
                Reject(capture.CapturedHeadNextValue is null
                    && capture.CapturedLaterTagFromReceiverPlus4 is null
                    && capture.QueuePopReturnedNormally is null,
                    nameof(capture),
                    "Head-next, later-tag, and queue-pop captures are unreachable unless the null-head reporter returns.");
                return new QueueCleanupEmptyAttemptPlan(
                    capture,
                    capture.NullHeadReporterReturned is null
                        ? QueueCleanupEmptyAttemptOutcome.NullHeadReporterReturnUnavailable
                        : QueueCleanupEmptyAttemptOutcome.NullHeadReporterDidNotReturn,
                    actions.ToImmutable(),
                    null,
                    null);
            }
        }
        else
        {
            Reject(capture.NullHeadReporterReturned is null,
                nameof(capture),
                "The null-head reporter return is unreachable for a non-null captured head.");
        }

        if (capture.CapturedHeadNextValue is not uint headNext)
        {
            Reject(capture.CapturedLaterTagFromReceiverPlus4 is null
                && capture.QueuePopReturnedNormally is null,
                nameof(capture),
                "Later-tag and queue-pop captures are unreachable before the head-next comparison.");
            return new QueueCleanupEmptyAttemptPlan(
                capture,
                QueueCleanupEmptyAttemptOutcome.HeadNextCaptureUnavailable,
                actions.ToImmutable(),
                null,
                null);
        }

        bool headNextWasZero = headNext == 0;
        actions.Add(new QueueCleanupEmptyAttemptAction(
            QueueCleanupEmptyAttemptActionKind.CaptureAndCompareHeadNextToZero,
            HeadNextCompareRawFileOffset,
            CapturedValue: headNext,
            ConditionMet: headNextWasZero));
        actions.Add(new QueueCleanupEmptyAttemptAction(
            QueueCleanupEmptyAttemptActionKind.SetHeadNextZeroFlag,
            HeadNextSetZeroFlagRawFileOffset,
            ConditionMet: headNextWasZero));

        if (capture.CapturedLaterTagFromReceiverPlus4 is not uint laterTag)
        {
            Reject(capture.QueuePopReturnedNormally is null,
                nameof(capture),
                "The queue-pop return capture is unreachable before the later-tag comparison.");
            return new QueueCleanupEmptyAttemptPlan(
                capture,
                QueueCleanupEmptyAttemptOutcome.LaterTagCaptureUnavailable,
                actions.ToImmutable(),
                headNextWasZero,
                null);
        }

        bool tagsMatch = laterTag == capture.CapturedInitialTagFromReceiverPlus4;
        actions.Add(new QueueCleanupEmptyAttemptAction(
            QueueCleanupEmptyAttemptActionKind.CaptureAndCompareLaterReceiverTag,
            LaterTagCompareRawFileOffset,
            CapturedValue: laterTag,
            ConditionMet: tagsMatch));

        if (!tagsMatch)
        {
            Reject(capture.QueuePopReturnedNormally is null,
                nameof(capture),
                "The queue-pop return is unreachable after a later-tag mismatch.");
            actions.Add(new QueueCleanupEmptyAttemptAction(
                QueueCleanupEmptyAttemptActionKind.RetryAtLoopHeader,
                TagMismatchBranchRawFileOffset,
                ConditionMet: true,
                TargetRawFileOffset: LoopHeaderRawFileOffset,
                TargetVirtualAddress: LoopHeaderVirtualAddress));
            return new QueueCleanupEmptyAttemptPlan(
                capture,
                QueueCleanupEmptyAttemptOutcome.TagMismatchRetriesLoop,
                actions.ToImmutable(),
                headNextWasZero,
                false);
        }

        if (headNextWasZero)
        {
            Reject(capture.QueuePopReturnedNormally is null,
                nameof(capture),
                "The queue-pop return is unreachable when the captured head-next value is zero.");
            actions.Add(new QueueCleanupEmptyAttemptAction(
                QueueCleanupEmptyAttemptActionKind.EnterSecondaryDrainBoundary,
                SecondaryDrainBoundaryRawFileOffset,
                TargetRawFileOffset: SecondaryDrainBoundaryRawFileOffset,
                TargetVirtualAddress:
                    SecondaryDrainBoundaryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset));
            return new QueueCleanupEmptyAttemptPlan(
                capture,
                QueueCleanupEmptyAttemptOutcome.EmptyHeadNextEntersSecondaryDrainBoundary,
                actions.ToImmutable(),
                true,
                true);
        }

        actions.Add(new QueueCleanupEmptyAttemptAction(
            QueueCleanupEmptyAttemptActionKind.CallQueuePop,
            QueuePopCallRawFileOffset,
            CallReturnedNormally: capture.QueuePopReturnedNormally,
            TargetRawFileOffset: QueuePopTargetRawFileOffset,
            TargetVirtualAddress: QueuePopTargetVirtualAddress));

        if (capture.QueuePopReturnedNormally is not true)
        {
            return new QueueCleanupEmptyAttemptPlan(
                capture,
                capture.QueuePopReturnedNormally is null
                    ? QueueCleanupEmptyAttemptOutcome.QueuePopReturnUnavailableAtCallBoundary
                    : QueueCleanupEmptyAttemptOutcome.QueuePopDidNotReturnAtCallBoundary,
                actions.ToImmutable(),
                false,
                true);
        }

        actions.Add(new QueueCleanupEmptyAttemptAction(
            QueueCleanupEmptyAttemptActionKind.RepeatAtLoopHeader,
            LoopRepeatJumpRawFileOffset,
            CallReturnedNormally: true,
            TargetRawFileOffset: LoopHeaderRawFileOffset,
            TargetVirtualAddress: LoopHeaderVirtualAddress));
        return new QueueCleanupEmptyAttemptPlan(
            capture,
            QueueCleanupEmptyAttemptOutcome.QueuePopReturnReachesExplicitLoopRepeat,
            actions.ToImmutable(),
            false,
            true);
    }

    private static void Reject(bool condition, string parameterName, string message)
    {
        if (!condition)
        {
            throw new ArgumentException(message, parameterName);
        }
    }
}
