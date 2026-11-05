namespace Firefall.Reconstruction.Startup;

public readonly record struct ContextQueueAppendCaptures(
    uint? CapturedThisPointer,
    uint? CapturedCursorEax,
    uint? CapturedInitialPointerAtEsp24,
    uint? CapturedPayloadPointerEdi,
    uint? CapturedValueAtEsp28,
    uint? AppendReturnEax);

public enum ContextQueueAppendOutcome
{
    AwaitingThisPointerCapture,
    AwaitingCursorCapture,
    AwaitingInitialPointerCapture,
    AwaitingPayloadPointerCapture,
    AwaitingValueCapture,
    AwaitingAppendReturnCapture,
    AppendNormalReturnBoundary
}

public enum ContextQueueAppendActionKind
{
    CaptureThisPointer,
    ComputeReceiver,
    CaptureCursorEax,
    CaptureInitialPointerAtEsp24,
    SubtractInitialPointerFromCursor,
    ZeroExtendLowWord,
    CapturePayloadPointerEdi,
    AttemptDeltaStore,
    CaptureValueAtEsp28,
    PushPayloadPointerEdi,
    AttemptValueStore,
    CallAppend,
    StopAtCaptureBoundary,
    StopAtAppendReturnCaptureBoundary,
    CaptureAppendReturnEax,
    StopAtAppendNormalReturnBoundary
}

public readonly record struct ContextQueueAppendAction(
    ContextQueueAppendActionKind Kind,
    uint RawFileOffset,
    uint? Value = null,
    uint? Address = null,
    uint? BranchTargetRawFileOffset = null,
    uint? TargetVirtualAddress = null,
    uint? CallReceiverAddress = null);

public readonly record struct ContextQueueAppendPlan(
    ContextQueueAppendCaptures Captures,
    ContextQueueAppendOutcome Outcome,
    IReadOnlyList<ContextQueueAppendAction> Actions,
    uint? ReceiverAddress,
    uint? WrappedDelta,
    uint? StoredDelta,
    uint? AppendReturnEax);

public readonly record struct ContextQueueAppendInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects the ordered captures and stores immediately before the opaque
/// append call, then stops at that call's normal-return boundary.
/// </summary>
public static class ContextQueueAppendProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";

    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const string IdaAddressMapping =
        "Base-zero IDA EAs are raw file offsets; PE code VA is raw + 0x400C00, not RVA.";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint ProjectionStartRawFileOffset = 0x00BD3456;
    public const uint AppendCallRawFileOffset = 0x00BD346E;
    public const uint AppendReturnBoundaryRawFileOffset = 0x00BD3473;
    public const uint EvidenceEndRawFileOffsetExclusive = 0x00BD3477;
    public const uint ProjectionStartVirtualAddress =
        ProjectionStartRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint AppendCallVirtualAddress =
        AppendCallRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint AppendReturnBoundaryVirtualAddress =
        AppendReturnBoundaryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EvidenceEndVirtualAddressExclusive =
        EvidenceEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint ReceiverAdjustment = 0x260;
    public const int InitialPointerStackOffset = 0x24;
    public const int ValueStackOffset = 0x28;
    public const int PayloadDeltaOffset = 4;
    public const int PayloadValueOffset = 0x18;
    public const uint AppendCalleeRawFileOffset = 0x12CE310;
    public const uint AppendCalleeVirtualAddress =
        AppendCalleeRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedEvidenceBytes =
        "8D 8E 60 02 00 00 2B 44 24 24 0F B7 C0 89 47 04 " +
        "8B 44 24 28 57 89 47 18 E8 9D AE 6F 00 83 7D 0C 02";

    public static IReadOnlyList<ContextQueueAppendInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new ContextQueueAppendInstructionEvidence(
                "Form receiver this + 0x260",
                ProjectionStartRawFileOffset,
                "8D 8E 60 02 00 00"),
            new ContextQueueAppendInstructionEvidence(
                "Subtract the independently captured [esp+0x24] pointer from incoming EAX",
                0x00BD345C,
                "2B 44 24 24"),
            new ContextQueueAppendInstructionEvidence(
                "Keep only the low word of the wrapping DWORD subtraction",
                0x00BD3460,
                "0F B7 C0"),
            new ContextQueueAppendInstructionEvidence(
                "Attempt to store the zero-extended delta DWORD at EDI + 4",
                0x00BD3463,
                "89 47 04"),
            new ContextQueueAppendInstructionEvidence(
                "Reload the independent [esp+0x28] value",
                0x00BD3466,
                "8B 44 24 28"),
            new ContextQueueAppendInstructionEvidence(
                "Push EDI without changing the payload pointer",
                0x00BD346A,
                "57"),
            new ContextQueueAppendInstructionEvidence(
                "Attempt to store the captured value at EDI + 0x18",
                0x00BD346B,
                "89 47 18"),
            new ContextQueueAppendInstructionEvidence(
                "Call the opaque append routine",
                AppendCallRawFileOffset,
                "E8 9D AE 6F 00",
                AppendCalleeRawFileOffset,
                AppendCalleeVirtualAddress),
            new ContextQueueAppendInstructionEvidence(
                "First instruction beyond the normal-return boundary; not projected",
                AppendReturnBoundaryRawFileOffset,
                "83 7D 0C 02")
        });

    public static ContextQueueAppendPlan Project(ContextQueueAppendCaptures captures)
    {
        var actions = new List<ContextQueueAppendAction>(14);

        if (!captures.CapturedThisPointer.HasValue)
        {
            RejectLaterCaptures(captures, thisReached: false, cursorReached: false,
                initialPointerReached: false, payloadReached: false, valueReached: false,
                appendReturnReached: false);
            actions.Add(new ContextQueueAppendAction(
                ContextQueueAppendActionKind.StopAtCaptureBoundary,
                ProjectionStartRawFileOffset));
            return Boundary(captures, ContextQueueAppendOutcome.AwaitingThisPointerCapture,
                actions);
        }

        uint receiver = unchecked(captures.CapturedThisPointer.Value + ReceiverAdjustment);
        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.CaptureThisPointer,
            ProjectionStartRawFileOffset,
            Value: captures.CapturedThisPointer.Value));
        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.ComputeReceiver,
            ProjectionStartRawFileOffset,
            Value: receiver,
            Address: captures.CapturedThisPointer.Value));

        if (!captures.CapturedCursorEax.HasValue)
        {
            RejectLaterCaptures(captures, thisReached: true, cursorReached: false,
                initialPointerReached: false, payloadReached: false, valueReached: false,
                appendReturnReached: false);
            actions.Add(new ContextQueueAppendAction(
                ContextQueueAppendActionKind.StopAtCaptureBoundary,
                0x00BD345C));
            return Boundary(captures, ContextQueueAppendOutcome.AwaitingCursorCapture,
                actions, receiver);
        }

        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.CaptureCursorEax,
            0x00BD345C,
            Value: captures.CapturedCursorEax.Value));

        if (!captures.CapturedInitialPointerAtEsp24.HasValue)
        {
            RejectLaterCaptures(captures, thisReached: true, cursorReached: true,
                initialPointerReached: false, payloadReached: false, valueReached: false,
                appendReturnReached: false);
            actions.Add(new ContextQueueAppendAction(
                ContextQueueAppendActionKind.StopAtCaptureBoundary,
                0x00BD345C));
            return Boundary(captures, ContextQueueAppendOutcome.AwaitingInitialPointerCapture,
                actions, receiver);
        }

        uint wrappedDelta = unchecked(
            captures.CapturedCursorEax.Value - captures.CapturedInitialPointerAtEsp24.Value);
        uint storedDelta = (ushort)wrappedDelta;
        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.CaptureInitialPointerAtEsp24,
            0x00BD345C,
            Value: captures.CapturedInitialPointerAtEsp24.Value));
        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.SubtractInitialPointerFromCursor,
            0x00BD345C,
            Value: wrappedDelta));
        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.ZeroExtendLowWord,
            0x00BD3460,
            Value: storedDelta));

        if (!captures.CapturedPayloadPointerEdi.HasValue)
        {
            RejectLaterCaptures(captures, thisReached: true, cursorReached: true,
                initialPointerReached: true, payloadReached: false, valueReached: false,
                appendReturnReached: false);
            actions.Add(new ContextQueueAppendAction(
                ContextQueueAppendActionKind.StopAtCaptureBoundary,
                0x00BD3463));
            return Boundary(captures, ContextQueueAppendOutcome.AwaitingPayloadPointerCapture,
                actions, receiver, wrappedDelta, storedDelta);
        }

        uint payload = captures.CapturedPayloadPointerEdi.Value;
        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.CapturePayloadPointerEdi,
            0x00BD3463,
            Value: payload));
        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.AttemptDeltaStore,
            0x00BD3463,
            Address: unchecked(payload + (uint)PayloadDeltaOffset),
            Value: storedDelta));

        if (!captures.CapturedValueAtEsp28.HasValue)
        {
            RejectLaterCaptures(captures, thisReached: true, cursorReached: true,
                initialPointerReached: true, payloadReached: true, valueReached: false,
                appendReturnReached: false);
            actions.Add(new ContextQueueAppendAction(
                ContextQueueAppendActionKind.StopAtCaptureBoundary,
                0x00BD3466));
            return Boundary(captures, ContextQueueAppendOutcome.AwaitingValueCapture,
                actions, receiver, wrappedDelta, storedDelta);
        }

        uint value = captures.CapturedValueAtEsp28.Value;
        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.CaptureValueAtEsp28,
            0x00BD3466,
            Value: value));
        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.PushPayloadPointerEdi,
            0x00BD346A,
            Value: payload));
        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.AttemptValueStore,
            0x00BD346B,
            Address: unchecked(payload + (uint)PayloadValueOffset),
            Value: value));
        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.CallAppend,
            AppendCallRawFileOffset,
            Address: payload,
            TargetVirtualAddress: AppendCalleeVirtualAddress,
            CallReceiverAddress: receiver));

        if (!captures.AppendReturnEax.HasValue)
        {
            actions.Add(new ContextQueueAppendAction(
                ContextQueueAppendActionKind.StopAtAppendReturnCaptureBoundary,
                AppendReturnBoundaryRawFileOffset));
            return Boundary(captures, ContextQueueAppendOutcome.AwaitingAppendReturnCapture,
                actions, receiver, wrappedDelta, storedDelta);
        }

        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.CaptureAppendReturnEax,
            AppendReturnBoundaryRawFileOffset,
            Value: captures.AppendReturnEax.Value));
        actions.Add(new ContextQueueAppendAction(
            ContextQueueAppendActionKind.StopAtAppendNormalReturnBoundary,
            AppendReturnBoundaryRawFileOffset,
            Value: captures.AppendReturnEax.Value));
        return new ContextQueueAppendPlan(
            captures,
            ContextQueueAppendOutcome.AppendNormalReturnBoundary,
            actions.AsReadOnly(),
            receiver,
            wrappedDelta,
            storedDelta,
            captures.AppendReturnEax);
    }

    private static ContextQueueAppendPlan Boundary(
        ContextQueueAppendCaptures captures,
        ContextQueueAppendOutcome outcome,
        List<ContextQueueAppendAction> actions,
        uint? receiver = null,
        uint? wrappedDelta = null,
        uint? storedDelta = null) =>
        new(captures, outcome, actions.AsReadOnly(), receiver, wrappedDelta, storedDelta, null);

    private static void RejectLaterCaptures(
        ContextQueueAppendCaptures captures,
        bool thisReached,
        bool cursorReached,
        bool initialPointerReached,
        bool payloadReached,
        bool valueReached,
        bool appendReturnReached)
    {
        RejectIfUnreached(nameof(captures.CapturedThisPointer), captures.CapturedThisPointer,
            thisReached);
        RejectIfUnreached(nameof(captures.CapturedCursorEax), captures.CapturedCursorEax,
            cursorReached);
        RejectIfUnreached(nameof(captures.CapturedInitialPointerAtEsp24),
            captures.CapturedInitialPointerAtEsp24, initialPointerReached);
        RejectIfUnreached(nameof(captures.CapturedPayloadPointerEdi),
            captures.CapturedPayloadPointerEdi, payloadReached);
        RejectIfUnreached(nameof(captures.CapturedValueAtEsp28),
            captures.CapturedValueAtEsp28, valueReached);
        RejectIfUnreached(nameof(captures.AppendReturnEax), captures.AppendReturnEax,
            appendReturnReached);
    }

    private static void RejectIfUnreached(string name, uint? value, bool reached)
    {
        if (!reached && value.HasValue)
        {
            throw new ArgumentException($"The {name} capture is not reached on this path.");
        }
    }
}
