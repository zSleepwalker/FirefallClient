using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum QueueSentinelReturnOutcome
{
    InitialReceiverWord8Unavailable,
    InitialReceiverWord0Unavailable,
    FirstDiagnosticReporterReturnUnavailable,
    FirstDiagnosticReporterDidNotReturn,
    PostFenceReceiverWord10Unavailable,
    SecondDiagnosticReporterReturnUnavailable,
    SecondDiagnosticReporterDidNotReturn,
    LaterNodePointerUnavailable,
    PoolPointerUnavailable,
    NullNodeDiagnosticReporterReturnUnavailable,
    NullNodeDiagnosticReporterDidNotReturn,
    CounterBeforeXaddUnavailable,
    HandoffHelperReturnUnavailable,
    HandoffHelperDidNotReturn,
    PlainReturnReached
}

public enum QueueSentinelReturnActionKind
{
    CaptureInitialReceiverWord8,
    CaptureInitialReceiverWord0ForComparison,
    ReportReceiverWordMismatch,
    LoadFence,
    CapturePostFenceReceiverWord10,
    ReportNonzeroReceiverWord10,
    CaptureLaterNodePointer,
    LoadPoolPointer,
    ReportNullNode,
    ProjectCounterXaddArithmetic,
    PushCapturedNode,
    SetPoolInEcx,
    CallStackHandoffHelper,
    PlainReturn
}

public readonly record struct QueueSentinelReturnInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? ReceiverRelativeOffset = null,
    uint? DiagnosticId = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

public readonly record struct QueueSentinelReturnAction(
    QueueSentinelReturnActionKind Kind,
    uint InstructionRawFileOffset,
    uint? CapturedValue = null,
    uint? SecondaryValue = null,
    uint? DiagnosticId = null,
    bool? ReporterReturned = null,
    uint? TargetAddress = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null,
    uint? CounterBeforeValue = null,
    uint? CounterAfterValue = null);

public readonly record struct QueueSentinelReturnProjection(
    uint? InitialReceiverWord8,
    uint? InitialReceiverWord0ForComparison,
    bool? Diagnostic2D6ReporterReturned,
    uint? PostFenceReceiverWord10,
    bool? Diagnostic2D7ReporterReturned,
    uint? LaterNodePointer,
    uint? CapturedPoolPointer,
    bool? NullNodeDiagnosticReporterReturned,
    uint? CapturedCounterBeforeXadd,
    uint? ProjectedCounterAddress,
    uint? ProjectedCounterAfterXadd,
    bool? HandoffHelperReturned,
    QueueSentinelReturnOutcome Outcome,
    ImmutableArray<QueueSentinelReturnAction> Actions);

/// <summary>
/// Bounded projection of the return path at raw offsets 0x12CE280-0x12CE30A.
/// Inputs are independent captures at their observed reads. A reached diagnostic
/// requires its nullable reporter-return capture before later reads can be used.
/// LOCK XADD arithmetic uses independent captured pool and counter values, with
/// DWORD address and decrement wrapping only. No memory effect, whole atomic
/// operation, free, or null-pool rejection is projected. The stack-handoff helper
/// remains opaque, and plain RET is included only when its nullable return capture
/// is true.
/// </summary>
public static class QueueSentinelReturnProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x12CE280;
    public const uint EndRawFileOffsetExclusive = 0x12CE30A;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const int ReceiverWord8Offset = 0x08;
    public const int ReceiverWord0Offset = 0x00;
    public const int ReceiverWord10Offset = 0x10;
    public const int PoolCounterOffset = 0x10;
    public const uint DiagnosticReporterFunctionPointerVirtualAddress = 0x1C15EA0;
    public const uint PoolPointerGlobalVirtualAddress = 0x1E003BC;
    public const uint ReceiverWordMismatchDiagnosticId = 0x2D6;
    public const uint ReceiverWord10DiagnosticId = 0x2D7;
    public const uint NullNodeDiagnosticId = 0x25A;
    public const uint CounterXaddDeltaBits = 0xFFFFFFFF;
    public const bool CounterXaddOldValueUsed = false;
    public const uint StackHandoffHelperRawFileOffset = 0x12CDA60;

    public const uint InitialReceiverWord8ReadRawFileOffset = 0x12CE285;
    public const uint InitialReceiverWord0CompareRawFileOffset = 0x12CE288;
    public const uint FirstDiagnosticBranchRawFileOffset = 0x12CE28A;
    public const uint FirstDiagnosticCallRawFileOffset = 0x12CE2A0;
    public const uint FenceRawFileOffset = 0x12CE2A9;
    public const uint PostFenceReceiverWord10CompareRawFileOffset = 0x12CE2AC;
    public const uint SecondDiagnosticBranchRawFileOffset = 0x12CE2B0;
    public const uint SecondDiagnosticCallRawFileOffset = 0x12CE2C6;
    public const uint LaterNodePointerReadRawFileOffset = 0x12CE2CF;
    public const uint PoolPointerLoadRawFileOffset = 0x12CE2D1;
    public const uint NullNodeTestRawFileOffset = 0x12CE2D7;
    public const uint NullNodeDiagnosticBranchRawFileOffset = 0x12CE2D9;
    public const uint NullNodeDiagnosticCallRawFileOffset = 0x12CE2EB;
    public const uint CounterXaddDeltaSetupRawFileOffset = 0x12CE2F4;
    public const uint CounterAddressSetupRawFileOffset = 0x12CE2F7;
    public const uint CounterXaddRawFileOffset = 0x12CE2FA;
    public const uint NodeArgumentPushRawFileOffset = 0x12CE2FE;
    public const uint PoolReceiverSetupRawFileOffset = 0x12CE2FF;
    public const uint StackHandoffCallRawFileOffset = 0x12CE301;
    public const uint PlainReturnRawFileOffset = 0x12CE309;

    public const string CapturedBodyBytes =
        "51 56 8B F1 57 8B 46 08 39 06 74 1D 68 7C 67 C5 01 " +
        "68 D6 02 00 00 68 1B 43 B7 01 68 0F 43 B7 01 FF 15 " +
        "A0 5E C1 01 83 C4 10 0F AE E8 83 7E 10 00 74 1D " +
        "68 8C 61 C1 01 68 D7 02 00 00 68 27 43 B7 01 68 " +
        "EB 42 B7 01 FF 15 A0 5E C1 01 83 C4 10 8B 36 8B " +
        "3D BC 03 E0 01 85 F6 75 19 56 68 5A 02 00 00 68 " +
        "93 43 B7 01 68 6F 43 B7 01 FF 15 A0 5E C1 01 83 " +
        "C4 10 83 CA FF 8D 47 10 F0 0F C1 10 56 8B CF E8 " +
        "5A F7 FF FF 5F 5E 59 C3";

    public static IReadOnlyList<QueueSentinelReturnInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new QueueSentinelReturnInstructionEvidence(
                "Capture receiver +8 before comparing receiver +0",
                InitialReceiverWord8ReadRawFileOffset,
                "8B 46 08",
                ReceiverRelativeOffset: ReceiverWord8Offset),
            new QueueSentinelReturnInstructionEvidence(
                "Compare receiver +0 with the captured receiver +8 value",
                InitialReceiverWord0CompareRawFileOffset,
                "39 06",
                ReceiverRelativeOffset: ReceiverWord0Offset),
            new QueueSentinelReturnInstructionEvidence(
                "Skip diagnostic 0x2D6 when the values are equal",
                FirstDiagnosticBranchRawFileOffset,
                "74 1D"),
            new QueueSentinelReturnInstructionEvidence(
                "Call the diagnostic reporter for a receiver-word mismatch",
                FirstDiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                DiagnosticId: ReceiverWordMismatchDiagnosticId,
                TargetVirtualAddress: DiagnosticReporterFunctionPointerVirtualAddress),
            new QueueSentinelReturnInstructionEvidence(
                "Execute LFENCE before the later receiver +0x10 test",
                FenceRawFileOffset,
                "0F AE E8"),
            new QueueSentinelReturnInstructionEvidence(
                "Compare receiver +0x10 with zero",
                PostFenceReceiverWord10CompareRawFileOffset,
                "83 7E 10 00",
                ReceiverRelativeOffset: ReceiverWord10Offset),
            new QueueSentinelReturnInstructionEvidence(
                "Skip diagnostic 0x2D7 when receiver +0x10 is zero",
                SecondDiagnosticBranchRawFileOffset,
                "74 1D"),
            new QueueSentinelReturnInstructionEvidence(
                "Call the diagnostic reporter for nonzero receiver +0x10",
                SecondDiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                DiagnosticId: ReceiverWord10DiagnosticId,
                TargetVirtualAddress: DiagnosticReporterFunctionPointerVirtualAddress),
            new QueueSentinelReturnInstructionEvidence(
                "Independently capture receiver +0 as the later node pointer",
                LaterNodePointerReadRawFileOffset,
                "8B 36",
                ReceiverRelativeOffset: ReceiverWord0Offset),
            new QueueSentinelReturnInstructionEvidence(
                "Load the pool pointer from global VA 0x1E003BC",
                PoolPointerLoadRawFileOffset,
                "8B 3D BC 03 E0 01",
                TargetVirtualAddress: PoolPointerGlobalVirtualAddress),
            new QueueSentinelReturnInstructionEvidence(
                "Test the captured later node pointer for null",
                NullNodeTestRawFileOffset,
                "85 F6"),
            new QueueSentinelReturnInstructionEvidence(
                "Skip diagnostic 0x25A for a nonnull node",
                NullNodeDiagnosticBranchRawFileOffset,
                "75 19"),
            new QueueSentinelReturnInstructionEvidence(
                "Call the diagnostic reporter for a null node",
                NullNodeDiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                DiagnosticId: NullNodeDiagnosticId,
                TargetVirtualAddress: DiagnosticReporterFunctionPointerVirtualAddress),
            new QueueSentinelReturnInstructionEvidence(
                "Set the XADD source operand to -1",
                CounterXaddDeltaSetupRawFileOffset,
                "83 CA FF"),
            new QueueSentinelReturnInstructionEvidence(
                "Form the counter address from the captured pool pointer plus 0x10",
                CounterAddressSetupRawFileOffset,
                "8D 47 10"),
            new QueueSentinelReturnInstructionEvidence(
                "LOCK XADD the -1 operand at captured pool +0x10",
                CounterXaddRawFileOffset,
                "F0 0F C1 10"),
            new QueueSentinelReturnInstructionEvidence(
                "Push the captured node pointer as the helper argument",
                NodeArgumentPushRawFileOffset,
                "56"),
            new QueueSentinelReturnInstructionEvidence(
                "Set ECX to the independently captured pool pointer",
                PoolReceiverSetupRawFileOffset,
                "8B CF"),
            new QueueSentinelReturnInstructionEvidence(
                "Call the opaque stack-handoff helper",
                StackHandoffCallRawFileOffset,
                "E8 5A F7 FF FF",
                TargetRawFileOffset: StackHandoffHelperRawFileOffset),
            new QueueSentinelReturnInstructionEvidence(
                "Return with a plain near RET",
                PlainReturnRawFileOffset,
                "C3")
        });

    public static QueueSentinelReturnProjection Project(
        uint? capturedInitialReceiverWord8,
        uint? capturedInitialReceiverWord0ForComparison,
        bool? diagnostic2D6ReporterReturned,
        uint? capturedPostFenceReceiverWord10,
        bool? diagnostic2D7ReporterReturned,
        uint? capturedLaterNodePointer,
        uint? capturedPoolPointer,
        bool? nullNodeDiagnosticReporterReturned,
        uint? capturedCounterBeforeXadd,
        bool? handoffHelperReturned = null)
    {
        var actions = ImmutableArray.CreateBuilder<QueueSentinelReturnAction>();
        uint? counterAddress = null;
        uint? counterAfterXadd = null;

        QueueSentinelReturnProjection Finish(QueueSentinelReturnOutcome outcome)
        {
            if (outcome == QueueSentinelReturnOutcome.CounterBeforeXaddUnavailable)
            {
                RejectUnreached((nameof(handoffHelperReturned), handoffHelperReturned));
            }
            else if (outcome is not (QueueSentinelReturnOutcome.HandoffHelperReturnUnavailable
                or QueueSentinelReturnOutcome.HandoffHelperDidNotReturn
                or QueueSentinelReturnOutcome.PlainReturnReached))
            {
                RejectUnreached(
                    (nameof(capturedCounterBeforeXadd), capturedCounterBeforeXadd),
                    (nameof(handoffHelperReturned), handoffHelperReturned));
            }

            return new(
                capturedInitialReceiverWord8,
                capturedInitialReceiverWord0ForComparison,
                diagnostic2D6ReporterReturned,
                capturedPostFenceReceiverWord10,
                diagnostic2D7ReporterReturned,
                capturedLaterNodePointer,
                capturedPoolPointer,
                nullNodeDiagnosticReporterReturned,
                capturedCounterBeforeXadd,
                counterAddress,
                counterAfterXadd,
                handoffHelperReturned,
                outcome,
                actions.ToImmutable());
        }

        if (capturedInitialReceiverWord8 is not uint receiverWord8)
        {
            RejectUnreached(
                (nameof(capturedInitialReceiverWord0ForComparison), capturedInitialReceiverWord0ForComparison),
                (nameof(diagnostic2D6ReporterReturned), diagnostic2D6ReporterReturned),
                (nameof(capturedPostFenceReceiverWord10), capturedPostFenceReceiverWord10),
                (nameof(diagnostic2D7ReporterReturned), diagnostic2D7ReporterReturned),
                (nameof(capturedLaterNodePointer), capturedLaterNodePointer),
                (nameof(capturedPoolPointer), capturedPoolPointer),
                (nameof(nullNodeDiagnosticReporterReturned), nullNodeDiagnosticReporterReturned));
            return Finish(QueueSentinelReturnOutcome.InitialReceiverWord8Unavailable);
        }

        actions.Add(new QueueSentinelReturnAction(
            QueueSentinelReturnActionKind.CaptureInitialReceiverWord8,
            InitialReceiverWord8ReadRawFileOffset,
            CapturedValue: receiverWord8));

        if (capturedInitialReceiverWord0ForComparison is not uint receiverWord0ForComparison)
        {
            RejectUnreached(
                (nameof(diagnostic2D6ReporterReturned), diagnostic2D6ReporterReturned),
                (nameof(capturedPostFenceReceiverWord10), capturedPostFenceReceiverWord10),
                (nameof(diagnostic2D7ReporterReturned), diagnostic2D7ReporterReturned),
                (nameof(capturedLaterNodePointer), capturedLaterNodePointer),
                (nameof(capturedPoolPointer), capturedPoolPointer),
                (nameof(nullNodeDiagnosticReporterReturned), nullNodeDiagnosticReporterReturned));
            return Finish(QueueSentinelReturnOutcome.InitialReceiverWord0Unavailable);
        }

        actions.Add(new QueueSentinelReturnAction(
            QueueSentinelReturnActionKind.CaptureInitialReceiverWord0ForComparison,
            InitialReceiverWord0CompareRawFileOffset,
            CapturedValue: receiverWord0ForComparison));

        bool mismatch = receiverWord8 != receiverWord0ForComparison;
        if (mismatch)
        {
            actions.Add(new QueueSentinelReturnAction(
                QueueSentinelReturnActionKind.ReportReceiverWordMismatch,
                FirstDiagnosticCallRawFileOffset,
                CapturedValue: receiverWord0ForComparison,
                SecondaryValue: receiverWord8,
                DiagnosticId: ReceiverWordMismatchDiagnosticId,
                ReporterReturned: diagnostic2D6ReporterReturned,
                TargetVirtualAddress: DiagnosticReporterFunctionPointerVirtualAddress));
            if (diagnostic2D6ReporterReturned is null)
            {
                RejectUnreached(
                    (nameof(capturedPostFenceReceiverWord10), capturedPostFenceReceiverWord10),
                    (nameof(diagnostic2D7ReporterReturned), diagnostic2D7ReporterReturned),
                    (nameof(capturedLaterNodePointer), capturedLaterNodePointer),
                    (nameof(capturedPoolPointer), capturedPoolPointer),
                    (nameof(nullNodeDiagnosticReporterReturned), nullNodeDiagnosticReporterReturned));
                return Finish(QueueSentinelReturnOutcome.FirstDiagnosticReporterReturnUnavailable);
            }
            if (!diagnostic2D6ReporterReturned.Value)
            {
                RejectUnreached(
                    (nameof(capturedPostFenceReceiverWord10), capturedPostFenceReceiverWord10),
                    (nameof(diagnostic2D7ReporterReturned), diagnostic2D7ReporterReturned),
                    (nameof(capturedLaterNodePointer), capturedLaterNodePointer),
                    (nameof(capturedPoolPointer), capturedPoolPointer),
                    (nameof(nullNodeDiagnosticReporterReturned), nullNodeDiagnosticReporterReturned));
                return Finish(QueueSentinelReturnOutcome.FirstDiagnosticReporterDidNotReturn);
            }
        }
        else
        {
            RejectUnreached(
                (nameof(diagnostic2D6ReporterReturned), diagnostic2D6ReporterReturned));
        }

        actions.Add(new QueueSentinelReturnAction(
            QueueSentinelReturnActionKind.LoadFence,
            FenceRawFileOffset));

        if (capturedPostFenceReceiverWord10 is not uint receiverWord10)
        {
            RejectUnreached(
                (nameof(diagnostic2D7ReporterReturned), diagnostic2D7ReporterReturned),
                (nameof(capturedLaterNodePointer), capturedLaterNodePointer),
                (nameof(capturedPoolPointer), capturedPoolPointer),
                (nameof(nullNodeDiagnosticReporterReturned), nullNodeDiagnosticReporterReturned));
            return Finish(QueueSentinelReturnOutcome.PostFenceReceiverWord10Unavailable);
        }

        actions.Add(new QueueSentinelReturnAction(
            QueueSentinelReturnActionKind.CapturePostFenceReceiverWord10,
            PostFenceReceiverWord10CompareRawFileOffset,
            CapturedValue: receiverWord10));

        if (receiverWord10 != 0)
        {
            actions.Add(new QueueSentinelReturnAction(
                QueueSentinelReturnActionKind.ReportNonzeroReceiverWord10,
                SecondDiagnosticCallRawFileOffset,
                CapturedValue: receiverWord10,
                DiagnosticId: ReceiverWord10DiagnosticId,
                ReporterReturned: diagnostic2D7ReporterReturned,
                TargetVirtualAddress: DiagnosticReporterFunctionPointerVirtualAddress));
            if (diagnostic2D7ReporterReturned is null)
            {
                RejectUnreached(
                    (nameof(capturedLaterNodePointer), capturedLaterNodePointer),
                    (nameof(capturedPoolPointer), capturedPoolPointer),
                    (nameof(nullNodeDiagnosticReporterReturned), nullNodeDiagnosticReporterReturned));
                return Finish(QueueSentinelReturnOutcome.SecondDiagnosticReporterReturnUnavailable);
            }
            if (!diagnostic2D7ReporterReturned.Value)
            {
                RejectUnreached(
                    (nameof(capturedLaterNodePointer), capturedLaterNodePointer),
                    (nameof(capturedPoolPointer), capturedPoolPointer),
                    (nameof(nullNodeDiagnosticReporterReturned), nullNodeDiagnosticReporterReturned));
                return Finish(QueueSentinelReturnOutcome.SecondDiagnosticReporterDidNotReturn);
            }
        }
        else
        {
            RejectUnreached(
                (nameof(diagnostic2D7ReporterReturned), diagnostic2D7ReporterReturned));
        }

        if (capturedLaterNodePointer is not uint nodePointer)
        {
            RejectUnreached(
                (nameof(capturedPoolPointer), capturedPoolPointer),
                (nameof(nullNodeDiagnosticReporterReturned), nullNodeDiagnosticReporterReturned));
            return Finish(QueueSentinelReturnOutcome.LaterNodePointerUnavailable);
        }

        actions.Add(new QueueSentinelReturnAction(
            QueueSentinelReturnActionKind.CaptureLaterNodePointer,
            LaterNodePointerReadRawFileOffset,
            CapturedValue: nodePointer));

        if (capturedPoolPointer is not uint poolPointer)
        {
            RejectUnreached(
                (nameof(nullNodeDiagnosticReporterReturned), nullNodeDiagnosticReporterReturned));
            return Finish(QueueSentinelReturnOutcome.PoolPointerUnavailable);
        }

        actions.Add(new QueueSentinelReturnAction(
            QueueSentinelReturnActionKind.LoadPoolPointer,
            PoolPointerLoadRawFileOffset,
            CapturedValue: poolPointer,
            TargetVirtualAddress: PoolPointerGlobalVirtualAddress));

        if (nodePointer == 0)
        {
            actions.Add(new QueueSentinelReturnAction(
                QueueSentinelReturnActionKind.ReportNullNode,
                NullNodeDiagnosticCallRawFileOffset,
                CapturedValue: nodePointer,
                DiagnosticId: NullNodeDiagnosticId,
                ReporterReturned: nullNodeDiagnosticReporterReturned,
                TargetVirtualAddress: DiagnosticReporterFunctionPointerVirtualAddress));
            if (nullNodeDiagnosticReporterReturned is null)
            {
                return Finish(QueueSentinelReturnOutcome.NullNodeDiagnosticReporterReturnUnavailable);
            }
            if (!nullNodeDiagnosticReporterReturned.Value)
            {
                return Finish(QueueSentinelReturnOutcome.NullNodeDiagnosticReporterDidNotReturn);
            }
        }
        else
        {
            RejectUnreached(
                (nameof(nullNodeDiagnosticReporterReturned), nullNodeDiagnosticReporterReturned));
        }

        if (capturedCounterBeforeXadd is not uint counterBeforeXadd)
        {
            return Finish(QueueSentinelReturnOutcome.CounterBeforeXaddUnavailable);
        }

        counterAddress = unchecked(poolPointer + PoolCounterOffset);
        counterAfterXadd = unchecked(counterBeforeXadd + CounterXaddDeltaBits);
        actions.Add(new QueueSentinelReturnAction(
            QueueSentinelReturnActionKind.ProjectCounterXaddArithmetic,
            CounterXaddRawFileOffset,
            CapturedValue: CounterXaddDeltaBits,
            SecondaryValue: poolPointer,
            TargetAddress: counterAddress,
            CounterBeforeValue: counterBeforeXadd,
            CounterAfterValue: counterAfterXadd));
        actions.Add(new QueueSentinelReturnAction(
            QueueSentinelReturnActionKind.PushCapturedNode,
            NodeArgumentPushRawFileOffset,
            CapturedValue: nodePointer));
        actions.Add(new QueueSentinelReturnAction(
            QueueSentinelReturnActionKind.SetPoolInEcx,
            PoolReceiverSetupRawFileOffset,
            CapturedValue: poolPointer));
        actions.Add(new QueueSentinelReturnAction(
            QueueSentinelReturnActionKind.CallStackHandoffHelper,
            StackHandoffCallRawFileOffset,
            CapturedValue: nodePointer,
            SecondaryValue: poolPointer,
            TargetRawFileOffset: StackHandoffHelperRawFileOffset));

        if (handoffHelperReturned is null)
        {
            return Finish(QueueSentinelReturnOutcome.HandoffHelperReturnUnavailable);
        }
        if (!handoffHelperReturned.Value)
        {
            return Finish(QueueSentinelReturnOutcome.HandoffHelperDidNotReturn);
        }

        actions.Add(new QueueSentinelReturnAction(
            QueueSentinelReturnActionKind.PlainReturn,
            PlainReturnRawFileOffset));

        return Finish(QueueSentinelReturnOutcome.PlainReturnReached);
    }

    private static void RejectUnreached(params (string Name, object? Value)[] captures)
    {
        foreach ((string name, object? value) in captures)
        {
            if (value is not null)
            {
                throw new ArgumentException(
                    $"Capture '{name}' was supplied before its read or reporter return was reached.",
                    name);
            }
        }
    }
}
