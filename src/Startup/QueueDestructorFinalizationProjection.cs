using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum QueueDestructorFinalizationCaptureKind
{
    Missing,
    Unreached,
    Captured
}

public readonly struct QueueDestructorFinalizationCapture<T> where T : struct
{
    private QueueDestructorFinalizationCapture(
        QueueDestructorFinalizationCaptureKind kind,
        T? value)
    {
        Kind = kind;
        Value = value;
    }

    public QueueDestructorFinalizationCaptureKind Kind { get; }
    public T? Value { get; }

    public static QueueDestructorFinalizationCapture<T> Missing() =>
        new(QueueDestructorFinalizationCaptureKind.Missing, null);

    public static QueueDestructorFinalizationCapture<T> Unreached() =>
        new(QueueDestructorFinalizationCaptureKind.Unreached, null);

    public static QueueDestructorFinalizationCapture<T> Captured(T value) =>
        new(QueueDestructorFinalizationCaptureKind.Captured, value);
}

public readonly record struct QueueDestructorFinalizationCaptures(
    uint FramePointer,
    QueueDestructorFinalizationCapture<uint> ReceiverPointer,
    QueueDestructorFinalizationCapture<uint> PriorExceptionChain);

public enum QueueDestructorFinalizationOutcome
{
    MissingReceiverCaptureBoundary,
    MissingPriorExceptionChainCaptureBoundary,
    PlainReturnReached
}

public enum QueueDestructorFinalizationActionKind
{
    CaptureReceiverPointerFromFrame,
    ProjectFirstDwordStore,
    CapturePriorExceptionChainFromFrame,
    ProjectRestoreFsExceptionChain,
    RestoreEcx,
    RestoreEdi,
    RestoreEsi,
    RestoreEbx,
    RestoreStackPointerFromFrame,
    RestoreEbp,
    PlainReturn
}

public readonly record struct QueueDestructorFinalizationAction(
    QueueDestructorFinalizationActionKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? CapturedValue = null);

public readonly record struct QueueDestructorFinalizationPlan(
    QueueDestructorFinalizationCaptures Captures,
    QueueDestructorFinalizationOutcome Outcome,
    ImmutableArray<QueueDestructorFinalizationAction> Actions,
    uint ReceiverFrameSlotAddress,
    uint ExceptionChainFrameSlotAddress,
    uint? FirstDwordStoreAddress,
    uint FirstDwordStoreValue,
    uint? RestoredExceptionChain);

public readonly record struct QueueDestructorFinalizationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes);

/// <summary>
/// Projects the ordinary destructor tail from separately supplied stack
/// captures. Actions are evidence projections, not native memory operations;
/// the first-DWORD store does not establish a type or ownership relationship.
/// </summary>
public static class QueueDestructorFinalizationProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x12C03AF;
    public const uint EndRawFileOffsetExclusive = 0x12C03CA;
    public const uint ReceiverReloadRawFileOffset = 0x12C03AF;
    public const uint FirstDwordStoreRawFileOffset = 0x12C03B2;
    public const uint PriorExceptionChainReloadRawFileOffset = 0x12C03B8;
    public const uint FsExceptionChainRestoreRawFileOffset = 0x12C03BB;
    public const uint RestoreEcxRawFileOffset = 0x12C03C2;
    public const uint RestoreEdiRawFileOffset = 0x12C03C3;
    public const uint RestoreEsiRawFileOffset = 0x12C03C4;
    public const uint RestoreEbxRawFileOffset = 0x12C03C5;
    public const uint RestoreStackPointerRawFileOffset = 0x12C03C6;
    public const uint RestoreEbpRawFileOffset = 0x12C03C8;
    public const uint PlainReturnRawFileOffset = 0x12C03C9;

    public const uint ReceiverFrameSlotOffset = 0x14;
    public const uint PriorExceptionChainFrameSlotOffset = 0x0C;
    public const uint FirstDwordStoreValue = 0x01C734CC;

    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EndVirtualAddressExclusive =
        EndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint FirstDwordStoreValueVirtualAddress = FirstDwordStoreValue;

    public const string CapturedTailBytes =
        "8B 45 EC C7 00 CC 34 C7 01 8B 4D F4 64 89 0D 00 00 00 00 " +
        "59 5F 5E 5B 8B E5 5D C3";

    public static IReadOnlyList<QueueDestructorFinalizationInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new QueueDestructorFinalizationInstructionEvidence(
                "Capture the receiver pointer from [EBP-0x14]",
                ReceiverReloadRawFileOffset,
                "8B 45 EC"),
            new QueueDestructorFinalizationInstructionEvidence(
                "Store immediate 0x01C734CC to the first DWORD at the captured receiver pointer",
                FirstDwordStoreRawFileOffset,
                "C7 00 CC 34 C7 01"),
            new QueueDestructorFinalizationInstructionEvidence(
                "Capture the prior exception-chain value from [EBP-0x0C]",
                PriorExceptionChainReloadRawFileOffset,
                "8B 4D F4"),
            new QueueDestructorFinalizationInstructionEvidence(
                "Restore FS:[0] from the captured prior exception-chain value",
                FsExceptionChainRestoreRawFileOffset,
                "64 89 0D 00 00 00 00"),
            new QueueDestructorFinalizationInstructionEvidence(
                "Restore ECX",
                RestoreEcxRawFileOffset,
                "59"),
            new QueueDestructorFinalizationInstructionEvidence(
                "Restore EDI",
                RestoreEdiRawFileOffset,
                "5F"),
            new QueueDestructorFinalizationInstructionEvidence(
                "Restore ESI",
                RestoreEsiRawFileOffset,
                "5E"),
            new QueueDestructorFinalizationInstructionEvidence(
                "Restore EBX",
                RestoreEbxRawFileOffset,
                "5B"),
            new QueueDestructorFinalizationInstructionEvidence(
                "Restore ESP from EBP",
                RestoreStackPointerRawFileOffset,
                "8B E5"),
            new QueueDestructorFinalizationInstructionEvidence(
                "Restore EBP",
                RestoreEbpRawFileOffset,
                "5D"),
            new QueueDestructorFinalizationInstructionEvidence(
                "Return with a plain near RET",
                PlainReturnRawFileOffset,
                "C3")
        });

    public static QueueDestructorFinalizationPlan Project(
        QueueDestructorFinalizationCaptures captures)
    {
        uint receiverFrameSlotAddress =
            unchecked(captures.FramePointer - ReceiverFrameSlotOffset);
        uint exceptionChainFrameSlotAddress =
            unchecked(captures.FramePointer - PriorExceptionChainFrameSlotOffset);

        var actions =
            ImmutableArray.CreateBuilder<QueueDestructorFinalizationAction>(11);
        actions.Add(new QueueDestructorFinalizationAction(
            QueueDestructorFinalizationActionKind.CaptureReceiverPointerFromFrame,
            ReceiverReloadRawFileOffset,
            Address: receiverFrameSlotAddress,
            CapturedValue: captures.ReceiverPointer.Value));

        if (captures.ReceiverPointer.Kind == QueueDestructorFinalizationCaptureKind.Missing)
        {
            RequireUnreached(
                captures.PriorExceptionChain,
                nameof(captures),
                "The prior-exception-chain capture must be unreached after the receiver capture is missing.");
            return new QueueDestructorFinalizationPlan(
                captures,
                QueueDestructorFinalizationOutcome.MissingReceiverCaptureBoundary,
                actions.ToImmutable(),
                receiverFrameSlotAddress,
                exceptionChainFrameSlotAddress,
                null,
                FirstDwordStoreValue,
                null);
        }

        RequireCaptured(
            captures.ReceiverPointer,
            nameof(captures),
            "The receiver pointer is loaded on every ordinary-tail path.");
        uint receiverPointer = captures.ReceiverPointer.Value!.Value;

        actions.Add(new QueueDestructorFinalizationAction(
            QueueDestructorFinalizationActionKind.ProjectFirstDwordStore,
            FirstDwordStoreRawFileOffset,
            Address: receiverPointer,
            CapturedValue: FirstDwordStoreValue));

        actions.Add(new QueueDestructorFinalizationAction(
            QueueDestructorFinalizationActionKind.CapturePriorExceptionChainFromFrame,
            PriorExceptionChainReloadRawFileOffset,
            Address: exceptionChainFrameSlotAddress,
            CapturedValue: captures.PriorExceptionChain.Value));

        if (captures.PriorExceptionChain.Kind == QueueDestructorFinalizationCaptureKind.Missing)
        {
            return new QueueDestructorFinalizationPlan(
                captures,
                QueueDestructorFinalizationOutcome.MissingPriorExceptionChainCaptureBoundary,
                actions.ToImmutable(),
                receiverFrameSlotAddress,
                exceptionChainFrameSlotAddress,
                receiverPointer,
                FirstDwordStoreValue,
                null);
        }

        RequireCaptured(
            captures.PriorExceptionChain,
            nameof(captures),
            "The prior exception-chain value is loaded on every path reaching this instruction.");
        uint priorExceptionChain = captures.PriorExceptionChain.Value!.Value;

        actions.Add(new QueueDestructorFinalizationAction(
            QueueDestructorFinalizationActionKind.ProjectRestoreFsExceptionChain,
            FsExceptionChainRestoreRawFileOffset,
            CapturedValue: priorExceptionChain));
        actions.Add(new QueueDestructorFinalizationAction(
            QueueDestructorFinalizationActionKind.RestoreEcx,
            RestoreEcxRawFileOffset));
        actions.Add(new QueueDestructorFinalizationAction(
            QueueDestructorFinalizationActionKind.RestoreEdi,
            RestoreEdiRawFileOffset));
        actions.Add(new QueueDestructorFinalizationAction(
            QueueDestructorFinalizationActionKind.RestoreEsi,
            RestoreEsiRawFileOffset));
        actions.Add(new QueueDestructorFinalizationAction(
            QueueDestructorFinalizationActionKind.RestoreEbx,
            RestoreEbxRawFileOffset));
        actions.Add(new QueueDestructorFinalizationAction(
            QueueDestructorFinalizationActionKind.RestoreStackPointerFromFrame,
            RestoreStackPointerRawFileOffset));
        actions.Add(new QueueDestructorFinalizationAction(
            QueueDestructorFinalizationActionKind.RestoreEbp,
            RestoreEbpRawFileOffset));
        actions.Add(new QueueDestructorFinalizationAction(
            QueueDestructorFinalizationActionKind.PlainReturn,
            PlainReturnRawFileOffset));

        return new QueueDestructorFinalizationPlan(
            captures,
            QueueDestructorFinalizationOutcome.PlainReturnReached,
            actions.ToImmutable(),
            receiverFrameSlotAddress,
            exceptionChainFrameSlotAddress,
            receiverPointer,
            FirstDwordStoreValue,
            priorExceptionChain);
    }

    private static void RequireCaptured<T>(
        QueueDestructorFinalizationCapture<T> capture,
        string parameterName,
        string message)
        where T : struct
    {
        if (capture.Kind != QueueDestructorFinalizationCaptureKind.Captured
            || capture.Value is null)
        {
            throw new ArgumentException(message, parameterName);
        }
    }

    private static void RequireUnreached<T>(
        QueueDestructorFinalizationCapture<T> capture,
        string parameterName,
        string message)
        where T : struct
    {
        if (capture.Kind != QueueDestructorFinalizationCaptureKind.Unreached
            || capture.Value is not null)
        {
            throw new ArgumentException(message, parameterName);
        }
    }
}
