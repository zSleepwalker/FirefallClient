using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct QueueAppendPublicationCapture(
    uint CapturedOldTailPointer,
    uint? CapturedNextPointer,
    bool? DiagnosticCallReturned,
    uint? CapturedPayloadArgument,
    uint? CapturedNewNodePointer);

public enum QueueAppendPublicationOutcome
{
    AwaitingNextPointerCapture,
    AwaitingDiagnosticReturnCapture,
    DiagnosticContinuationBoundary,
    AwaitingPayloadCapture,
    AwaitingNewNodeCapture,
    ReturnedCapturedNewNode
}

public enum QueueAppendPublicationActionKind
{
    CaptureOldTailNextPointer,
    StopAtNextPointerCaptureBoundary,
    SkipDiagnosticOnNullNextPointer,
    PushDiagnosticTrailingZeroArgument,
    PushDiagnosticId,
    PushDiagnosticSecondAddressArgument,
    PushDiagnosticFirstAddressArgument,
    CallDiagnosticThroughIndirectCell,
    StopAtDiagnosticReturnCaptureBoundary,
    StopAtOpaqueDiagnosticContinuationBoundary,
    ContinueAfterDiagnosticReturns,
    CapturePayloadArgument,
    StopAtPayloadCaptureBoundary,
    AttemptPayloadStoreToOldTail,
    CaptureNewNodePointer,
    StopAtNewNodeCaptureBoundary,
    AttemptNewNodePublicationToOldTail,
    ReturnCapturedNewNodeAndPopArgument
}

public readonly record struct QueueAppendPublicationAction(
    QueueAppendPublicationActionKind Kind,
    uint RawFileOffset,
    uint? Pointer = null,
    uint? CapturedValue = null,
    uint? DiagnosticId = null,
    uint? BranchTargetRawFileOffset = null,
    uint? IndirectCallTargetSlotVirtualAddress = null);

public readonly record struct QueueAppendPublicationPlan(
    QueueAppendPublicationCapture Capture,
    QueueAppendPublicationOutcome Outcome,
    ImmutableArray<QueueAppendPublicationAction> Actions,
    uint? ReturnValue);

public readonly record struct QueueAppendPublicationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? IndirectCallTargetSlotVirtualAddress = null);

/// <summary>
/// Projects only the post-success-CAS tail test and publication continuation.
/// The earlier CAS/acquisition, memory effects, ownership, and opaque
/// diagnostic continuation are not simulated.
/// </summary>
public static class QueueAppendPublicationProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";

    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint ContinuationStartRawFileOffset = 0x12CE3C3;
    public const uint ContinuationEndRawFileOffsetExclusive = 0x12CE3F7;
    public const uint NextPointerTestRawFileOffset = 0x12CE3C3;
    public const uint NextPointerIsZeroBranchRawFileOffset = 0x12CE3C6;
    public const uint DiagnosticCallRawFileOffset = 0x12CE3D9;
    public const uint DiagnosticReturnContinuationRawFileOffset = 0x12CE3DF;
    public const uint PayloadArgumentReloadRawFileOffset = 0x12CE3E2;
    public const uint PayloadStoreRawFileOffset = 0x12CE3E5;
    public const uint NewNodeReloadRawFileOffset = 0x12CE3E8;
    public const uint NewNodePublicationStoreRawFileOffset = 0x12CE3ED;
    public const uint ReturnRawFileOffset = 0x12CE3F4;

    public const uint DiagnosticId = 0x2F0;
    public const uint DiagnosticIndirectCallTargetSlotVirtualAddress = 0x1C15EA0;
    public const uint DiagnosticFirstAddressArgumentVirtualAddress = 0x1B7434B;
    public const uint DiagnosticSecondAddressArgumentVirtualAddress = 0x1B742F7;
    public const uint NextPointerZeroBranchTargetRawFileOffset = 0x12CE3E2;

    public const int OldTailPayloadOffset = 4;

    public const string CapturedContinuationBytes =
        "83 3E 00 74 1A 6A 00 68 F0 02 00 00 68 F7 42 B7 01 " +
        "68 4B 43 B7 01 FF 15 A0 5E C1 01 83 C4 10 8B 45 08 " +
        "89 46 04 8B 44 24 0C 5F 89 06 5E 5B 8B E5 5D C2 04 00";

    public static IReadOnlyList<QueueAppendPublicationInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new QueueAppendPublicationInstructionEvidence(
                "Test the captured old-tail next pointer",
                NextPointerTestRawFileOffset,
                "83 3E 00"),
            new QueueAppendPublicationInstructionEvidence(
                "Skip reporting when the next pointer is zero",
                NextPointerIsZeroBranchRawFileOffset,
                "74 1A",
                NextPointerZeroBranchTargetRawFileOffset),
            new QueueAppendPublicationInstructionEvidence(
                "Push the diagnostic trailing zero argument",
                0x12CE3C8,
                "6A 00"),
            new QueueAppendPublicationInstructionEvidence(
                "Push diagnostic ID 0x2F0",
                0x12CE3CA,
                "68 F0 02 00 00"),
            new QueueAppendPublicationInstructionEvidence(
                "Push the diagnostic second address argument",
                0x12CE3CF,
                "68 F7 42 B7 01"),
            new QueueAppendPublicationInstructionEvidence(
                "Push the diagnostic first address argument",
                0x12CE3D4,
                "68 4B 43 B7 01"),
            new QueueAppendPublicationInstructionEvidence(
                "Call through the indirect diagnostic cell",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                IndirectCallTargetSlotVirtualAddress:
                    DiagnosticIndirectCallTargetSlotVirtualAddress),
            new QueueAppendPublicationInstructionEvidence(
                "Clean up diagnostic arguments after a normal call return",
                DiagnosticReturnContinuationRawFileOffset,
                "83 C4 10"),
            new QueueAppendPublicationInstructionEvidence(
                "Reload the native payload argument from EBP+8",
                PayloadArgumentReloadRawFileOffset,
                "8B 45 08"),
            new QueueAppendPublicationInstructionEvidence(
                "Store the payload to old-tail offset 4",
                PayloadStoreRawFileOffset,
                "89 46 04"),
            new QueueAppendPublicationInstructionEvidence(
                "Reload the captured new-node pointer from stack offset 0x0C",
                NewNodeReloadRawFileOffset,
                "8B 44 24 0C"),
            new QueueAppendPublicationInstructionEvidence(
                "Publish the new-node pointer through the old-tail head field",
                NewNodePublicationStoreRawFileOffset,
                "89 06"),
            new QueueAppendPublicationInstructionEvidence(
                "Return EAX and pop the four-byte native argument",
                ReturnRawFileOffset,
                "C2 04 00")
        });

    /// <summary>
    /// Each optional value is a reached native capture. Supplying a later
    /// capture before its control-flow path reaches it is rejected.
    /// </summary>
    public static QueueAppendPublicationPlan Project(
        QueueAppendPublicationCapture capture)
    {
        var actions = ImmutableArray.CreateBuilder<QueueAppendPublicationAction>(12);

        if (!capture.CapturedNextPointer.HasValue)
        {
            RejectUnreachedCaptures(
                capture,
                diagnosticReturnReached: false,
                payloadReached: false,
                newNodeReached: false);
            actions.Add(new QueueAppendPublicationAction(
                QueueAppendPublicationActionKind.StopAtNextPointerCaptureBoundary,
                NextPointerTestRawFileOffset,
                Pointer: capture.CapturedOldTailPointer));
            return Boundary(capture, QueueAppendPublicationOutcome.AwaitingNextPointerCapture, actions);
        }

        uint nextPointer = capture.CapturedNextPointer.Value;
        actions.Add(new QueueAppendPublicationAction(
            QueueAppendPublicationActionKind.CaptureOldTailNextPointer,
            NextPointerTestRawFileOffset,
            Pointer: capture.CapturedOldTailPointer,
            CapturedValue: nextPointer));

        if (nextPointer == 0)
        {
            if (capture.DiagnosticCallReturned.HasValue)
            {
                throw Unreached(nameof(capture.DiagnosticCallReturned));
            }

            actions.Add(new QueueAppendPublicationAction(
                QueueAppendPublicationActionKind.SkipDiagnosticOnNullNextPointer,
                NextPointerIsZeroBranchRawFileOffset,
                BranchTargetRawFileOffset: NextPointerZeroBranchTargetRawFileOffset));
        }
        else
        {
            actions.Add(new QueueAppendPublicationAction(
                QueueAppendPublicationActionKind.PushDiagnosticTrailingZeroArgument,
                0x12CE3C8,
                CapturedValue: 0));
            actions.Add(new QueueAppendPublicationAction(
                QueueAppendPublicationActionKind.PushDiagnosticId,
                0x12CE3CA,
                CapturedValue: DiagnosticId,
                DiagnosticId: DiagnosticId));
            actions.Add(new QueueAppendPublicationAction(
                QueueAppendPublicationActionKind.PushDiagnosticSecondAddressArgument,
                0x12CE3CF,
                Pointer: DiagnosticSecondAddressArgumentVirtualAddress));
            actions.Add(new QueueAppendPublicationAction(
                QueueAppendPublicationActionKind.PushDiagnosticFirstAddressArgument,
                0x12CE3D4,
                Pointer: DiagnosticFirstAddressArgumentVirtualAddress));
            actions.Add(new QueueAppendPublicationAction(
                QueueAppendPublicationActionKind.CallDiagnosticThroughIndirectCell,
                DiagnosticCallRawFileOffset,
                DiagnosticId: DiagnosticId,
                IndirectCallTargetSlotVirtualAddress:
                    DiagnosticIndirectCallTargetSlotVirtualAddress));

            if (!capture.DiagnosticCallReturned.HasValue)
            {
                RejectUnreachedCaptures(
                    capture,
                    diagnosticReturnReached: false,
                    payloadReached: false,
                    newNodeReached: false);
                actions.Add(new QueueAppendPublicationAction(
                    QueueAppendPublicationActionKind.StopAtDiagnosticReturnCaptureBoundary,
                    DiagnosticCallRawFileOffset,
                    IndirectCallTargetSlotVirtualAddress:
                        DiagnosticIndirectCallTargetSlotVirtualAddress));
                return Boundary(
                    capture,
                    QueueAppendPublicationOutcome.AwaitingDiagnosticReturnCapture,
                    actions);
            }

            if (!capture.DiagnosticCallReturned.Value)
            {
                RejectUnreachedCaptures(
                    capture,
                    diagnosticReturnReached: true,
                    payloadReached: false,
                    newNodeReached: false);
                actions.Add(new QueueAppendPublicationAction(
                    QueueAppendPublicationActionKind.StopAtOpaqueDiagnosticContinuationBoundary,
                    DiagnosticCallRawFileOffset,
                    IndirectCallTargetSlotVirtualAddress:
                        DiagnosticIndirectCallTargetSlotVirtualAddress));
                return Boundary(
                    capture,
                    QueueAppendPublicationOutcome.DiagnosticContinuationBoundary,
                    actions);
            }

            actions.Add(new QueueAppendPublicationAction(
                QueueAppendPublicationActionKind.ContinueAfterDiagnosticReturns,
                DiagnosticReturnContinuationRawFileOffset));
        }

        if (!capture.CapturedPayloadArgument.HasValue)
        {
            RejectUnreachedCaptures(
                capture,
                diagnosticReturnReached: true,
                payloadReached: false,
                newNodeReached: false);
            actions.Add(new QueueAppendPublicationAction(
                QueueAppendPublicationActionKind.StopAtPayloadCaptureBoundary,
                PayloadArgumentReloadRawFileOffset));
            return Boundary(capture, QueueAppendPublicationOutcome.AwaitingPayloadCapture, actions);
        }

        uint payload = capture.CapturedPayloadArgument.Value;
        actions.Add(new QueueAppendPublicationAction(
            QueueAppendPublicationActionKind.CapturePayloadArgument,
            PayloadArgumentReloadRawFileOffset,
            CapturedValue: payload));
        actions.Add(new QueueAppendPublicationAction(
            QueueAppendPublicationActionKind.AttemptPayloadStoreToOldTail,
            PayloadStoreRawFileOffset,
            Pointer: unchecked(capture.CapturedOldTailPointer + (uint)OldTailPayloadOffset),
            CapturedValue: payload));

        if (!capture.CapturedNewNodePointer.HasValue)
        {
            actions.Add(new QueueAppendPublicationAction(
                QueueAppendPublicationActionKind.StopAtNewNodeCaptureBoundary,
                NewNodeReloadRawFileOffset));
            return Boundary(capture, QueueAppendPublicationOutcome.AwaitingNewNodeCapture, actions);
        }

        uint newNode = capture.CapturedNewNodePointer.Value;
        actions.Add(new QueueAppendPublicationAction(
            QueueAppendPublicationActionKind.CaptureNewNodePointer,
            NewNodeReloadRawFileOffset,
            CapturedValue: newNode));
        actions.Add(new QueueAppendPublicationAction(
            QueueAppendPublicationActionKind.AttemptNewNodePublicationToOldTail,
            NewNodePublicationStoreRawFileOffset,
            Pointer: capture.CapturedOldTailPointer,
            CapturedValue: newNode));
        actions.Add(new QueueAppendPublicationAction(
            QueueAppendPublicationActionKind.ReturnCapturedNewNodeAndPopArgument,
            ReturnRawFileOffset,
            CapturedValue: newNode));

        return new QueueAppendPublicationPlan(
            capture,
            QueueAppendPublicationOutcome.ReturnedCapturedNewNode,
            actions.ToImmutable(),
            newNode);
    }

    private static QueueAppendPublicationPlan Boundary(
        QueueAppendPublicationCapture capture,
        QueueAppendPublicationOutcome outcome,
        ImmutableArray<QueueAppendPublicationAction>.Builder actions) =>
        new(capture, outcome, actions.ToImmutable(), ReturnValue: null);

    private static void RejectUnreachedCaptures(
        QueueAppendPublicationCapture capture,
        bool diagnosticReturnReached,
        bool payloadReached,
        bool newNodeReached)
    {
        if (!diagnosticReturnReached && capture.DiagnosticCallReturned.HasValue)
        {
            throw Unreached(nameof(capture.DiagnosticCallReturned));
        }

        if (!payloadReached && capture.CapturedPayloadArgument.HasValue)
        {
            throw Unreached(nameof(capture.CapturedPayloadArgument));
        }

        if (!newNodeReached && capture.CapturedNewNodePointer.HasValue)
        {
            throw Unreached(nameof(capture.CapturedNewNodePointer));
        }
    }

    private static ArgumentException Unreached(string captureName) =>
        new($"The {captureName} capture is not reached on this path.");
}
