using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct SocketLargeQueueGateCapture(
    uint ReceiverPointer,
    uint PayloadPointer,
    uint? InitialStateDword108,
    bool? DiagnosticCallReturnedNormally,
    byte? GateByteD95,
    uint? LaterStateDword108,
    ushort? PayloadWord);

public enum SocketLargeQueueGateOutcome
{
    MissingInitialState,
    UnreachedCaptureProvided,
    MissingDiagnosticReturnCapture,
    DiagnosticCallDidNotReturnNormally,
    MissingGateByte,
    GateByteBranchesToHandoff,
    MissingLaterState,
    LaterStateBranchesToHeaderDispatch,
    MissingPayloadWord,
    PayloadMaskBranchesToHandoff,
    PayloadMaskFallsThroughToDeferredControl
}

public readonly record struct SocketLargeQueueGatePlan(
    SocketLargeQueueGateCapture Capture,
    SocketLargeQueueGateOutcome Outcome,
    bool DiagnosticCallReached,
    uint InitialStateAddress,
    uint? GateByteAddress,
    uint? LaterStateAddress,
    uint? PayloadWordAddress,
    ushort? PayloadMaskedBits,
    uint? RouteRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct SocketLargeQueueGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null);

/// <summary>
/// Projects the large queued-receive gate from raw BD40BF through its first
/// deferred-control, handoff, or header-dispatch boundary. The first and later
/// receiver DWORD reads are independent captures. The diagnostic is modeled
/// only through its captured normal return; no downstream route is expanded.
/// </summary>
public static class SocketLargeQueueGateProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x00BD40BF;
    public const uint InitialStateReadRawFileOffset = 0x00BD40BF;
    public const uint InitialStateCompareThreeRawFileOffset = 0x00BD40C5;
    public const uint InitialStateCompareFiveRawFileOffset = 0x00BD40CA;
    public const uint InitialStateCompareEightRawFileOffset = 0x00BD40CF;
    public const uint DiagnosticCallRawFileOffset = 0x00BD40E5;
    public const uint DiagnosticCallReturnRawFileOffset = 0x00BD40EB;
    public const uint GateByteCompareRawFileOffset = 0x00BD40EE;
    public const uint GateByteBranchRawFileOffset = 0x00BD40F5;
    public const uint LaterStateCompareRawFileOffset = 0x00BD40FB;
    public const uint HeaderDispatchBranchRawFileOffset = 0x00BD4102;
    public const uint PayloadMaskLoadRawFileOffset = 0x00BD4104;
    public const uint PayloadWordTestRawFileOffset = 0x00BD4109;
    public const uint PayloadMaskBranchRawFileOffset = 0x00BD410C;

    public const uint DeferredControlRawFileOffset = 0x00BD4112;
    public const uint HeaderDispatchRawFileOffset = 0x00BD412B;
    public const uint HandoffRawFileOffset = 0x00BD4214;

    public const uint ReceiverDword108Offset = 0x108;
    public const uint ReceiverByteD95Offset = 0xD95;
    public const ushort PayloadMask = 0xC000;
    public const uint DiagnosticCode = 0x4FA;
    public const uint DiagnosticFirstPushedValue = 0;
    public const uint DiagnosticFirstPushedPointer = 0x01B583AD;
    public const uint DiagnosticSecondPushedPointer = 0x01B583AE;

    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint DeferredControlVirtualAddress =
        DeferredControlRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint HeaderDispatchVirtualAddress =
        HeaderDispatchRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint HandoffVirtualAddress =
        HandoffRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public static ReadOnlyCollection<SocketLargeQueueGateInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new SocketLargeQueueGateInstructionEvidence(
                "Capture the initial receiver DWORD at +0x108",
                InitialStateReadRawFileOffset,
                "8B 86 08 01 00 00"),
            new SocketLargeQueueGateInstructionEvidence(
                "Initial state 3 skips the diagnostic to the common byte gate",
                InitialStateCompareThreeRawFileOffset,
                "83 F8 03 74 24",
                GateByteCompareRawFileOffset),
            new SocketLargeQueueGateInstructionEvidence(
                "Initial state 5 skips the diagnostic to the common byte gate",
                InitialStateCompareFiveRawFileOffset,
                "83 F8 05 74 1F",
                GateByteCompareRawFileOffset),
            new SocketLargeQueueGateInstructionEvidence(
                "Initial state 8 skips the diagnostic to the common byte gate",
                InitialStateCompareEightRawFileOffset,
                "83 F8 08 74 1A",
                GateByteCompareRawFileOffset),
            new SocketLargeQueueGateInstructionEvidence(
                "Other initial states push the observed diagnostic arguments and code 0x4FA",
                0x00BD40D4,
                "6A 00 68 FA 04 00 00 68 AD 83 B5 01 68 AE 83 B5 01"),
            new SocketLargeQueueGateInstructionEvidence(
                "Other initial states call the diagnostic through the observed indirect slot",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01"),
            new SocketLargeQueueGateInstructionEvidence(
                "A normal diagnostic return cleans 0x10 stack bytes before the common byte gate",
                DiagnosticCallReturnRawFileOffset,
                "83 C4 10"),
            new SocketLargeQueueGateInstructionEvidence(
                "A nonzero receiver byte at +0xD95 branches to the handoff boundary",
                GateByteCompareRawFileOffset,
                "80 BE 95 0D 00 00 00 0F 85 19 01 00 00",
                HandoffRawFileOffset),
            new SocketLargeQueueGateInstructionEvidence(
                "Only a zero +0xD95 byte reaches the later, independent +0x108 DWORD comparison",
                LaterStateCompareRawFileOffset,
                "83 BE 08 01 00 00 03 75 27",
                HeaderDispatchRawFileOffset),
            new SocketLargeQueueGateInstructionEvidence(
                "Later state 3 loads the 0xC000 payload mask and tests the word at EDI",
                PayloadMaskLoadRawFileOffset,
                "B8 00 C0 00 00 66 85 07"),
            new SocketLargeQueueGateInstructionEvidence(
                "A nonzero payload-mask test branches to handoff; zero falls through to deferred control",
                PayloadMaskBranchRawFileOffset,
                "0F 85 02 01 00 00",
                HandoffRawFileOffset)
        });

    /// <summary>
    /// Projects separately captured reads and stops at the first downstream
    /// route. A missing call-return capture or an unreached read is never
    /// treated as a normal return or as a default value.
    /// </summary>
    public static SocketLargeQueueGatePlan Project(
        SocketLargeQueueGateCapture capture)
    {
        uint initialStateAddress = AddReceiverOffset(
            capture.ReceiverPointer,
            ReceiverDword108Offset);

        if (capture.InitialStateDword108 is not uint initialState)
        {
            return HasAnyPostInitialCapture(capture)
                ? Stop(
                    capture,
                    SocketLargeQueueGateOutcome.UnreachedCaptureProvided,
                    initialStateAddress,
                    stopAt: EntryRawFileOffset)
                : Stop(
                    capture,
                    SocketLargeQueueGateOutcome.MissingInitialState,
                    initialStateAddress,
                    stopAt: InitialStateReadRawFileOffset);
        }

        bool diagnosticCallReached = !IsDiagnosticSkippedState(initialState);
        if (!diagnosticCallReached
            && capture.DiagnosticCallReturnedNormally is not null)
        {
            return Stop(
                capture,
                SocketLargeQueueGateOutcome.UnreachedCaptureProvided,
                initialStateAddress,
                diagnosticCallReached,
                stopAt: DiagnosticCallRawFileOffset);
        }

        if (diagnosticCallReached)
        {
            if (capture.DiagnosticCallReturnedNormally is not bool returnedNormally)
            {
                return HasPostDiagnosticReturnCapture(capture)
                    ? Stop(
                        capture,
                        SocketLargeQueueGateOutcome.UnreachedCaptureProvided,
                        initialStateAddress,
                        diagnosticCallReached,
                        stopAt: DiagnosticCallReturnRawFileOffset)
                    : Stop(
                        capture,
                        SocketLargeQueueGateOutcome.MissingDiagnosticReturnCapture,
                        initialStateAddress,
                        diagnosticCallReached,
                        stopAt: DiagnosticCallReturnRawFileOffset);
            }

            if (!returnedNormally)
            {
                return HasPostDiagnosticReturnCapture(capture)
                    ? Stop(
                        capture,
                        SocketLargeQueueGateOutcome.UnreachedCaptureProvided,
                        initialStateAddress,
                        diagnosticCallReached,
                        stopAt: DiagnosticCallReturnRawFileOffset)
                    : Stop(
                        capture,
                        SocketLargeQueueGateOutcome.DiagnosticCallDidNotReturnNormally,
                        initialStateAddress,
                        diagnosticCallReached,
                        stopAt: DiagnosticCallReturnRawFileOffset);
            }
        }

        uint gateByteAddress = AddReceiverOffset(
            capture.ReceiverPointer,
            ReceiverByteD95Offset);
        if (capture.GateByteD95 is not byte gateByte)
        {
            return HasLaterCaptures(capture)
                ? Stop(
                    capture,
                    SocketLargeQueueGateOutcome.UnreachedCaptureProvided,
                    initialStateAddress,
                    diagnosticCallReached,
                    gateByteAddress: gateByteAddress,
                    stopAt: GateByteCompareRawFileOffset)
                : Stop(
                    capture,
                    SocketLargeQueueGateOutcome.MissingGateByte,
                    initialStateAddress,
                    diagnosticCallReached,
                    gateByteAddress: gateByteAddress,
                    stopAt: GateByteCompareRawFileOffset);
        }

        if (gateByte != 0)
        {
            return HasLaterCaptures(capture)
                ? Stop(
                    capture,
                    SocketLargeQueueGateOutcome.UnreachedCaptureProvided,
                    initialStateAddress,
                    diagnosticCallReached,
                    gateByteAddress: gateByteAddress,
                    stopAt: GateByteBranchRawFileOffset,
                    route: HandoffRawFileOffset)
                : Stop(
                    capture,
                    SocketLargeQueueGateOutcome.GateByteBranchesToHandoff,
                    initialStateAddress,
                    diagnosticCallReached,
                    gateByteAddress: gateByteAddress,
                    stopAt: HandoffRawFileOffset,
                    route: HandoffRawFileOffset);
        }

        uint laterStateAddress = AddReceiverOffset(
            capture.ReceiverPointer,
            ReceiverDword108Offset);
        if (capture.LaterStateDword108 is not uint laterState)
        {
            return capture.PayloadWord is not null
                ? Stop(
                    capture,
                    SocketLargeQueueGateOutcome.UnreachedCaptureProvided,
                    initialStateAddress,
                    diagnosticCallReached,
                    gateByteAddress: gateByteAddress,
                    laterStateAddress: laterStateAddress,
                    stopAt: LaterStateCompareRawFileOffset)
                : Stop(
                    capture,
                    SocketLargeQueueGateOutcome.MissingLaterState,
                    initialStateAddress,
                    diagnosticCallReached,
                    gateByteAddress: gateByteAddress,
                    laterStateAddress: laterStateAddress,
                    stopAt: LaterStateCompareRawFileOffset);
        }

        if (laterState != 3)
        {
            return capture.PayloadWord is not null
                ? Stop(
                    capture,
                    SocketLargeQueueGateOutcome.UnreachedCaptureProvided,
                    initialStateAddress,
                    diagnosticCallReached,
                    gateByteAddress: gateByteAddress,
                    laterStateAddress: laterStateAddress,
                    stopAt: HeaderDispatchRawFileOffset,
                    route: HeaderDispatchRawFileOffset)
                : Stop(
                    capture,
                    SocketLargeQueueGateOutcome.LaterStateBranchesToHeaderDispatch,
                    initialStateAddress,
                    diagnosticCallReached,
                    gateByteAddress: gateByteAddress,
                    laterStateAddress: laterStateAddress,
                    stopAt: HeaderDispatchRawFileOffset,
                    route: HeaderDispatchRawFileOffset);
        }

        if (capture.PayloadWord is not ushort payloadWord)
        {
            return Stop(
                capture,
                SocketLargeQueueGateOutcome.MissingPayloadWord,
                initialStateAddress,
                diagnosticCallReached,
                gateByteAddress: gateByteAddress,
                laterStateAddress: laterStateAddress,
                payloadWordAddress: capture.PayloadPointer,
                stopAt: PayloadWordTestRawFileOffset);
        }

        ushort maskedBits = (ushort)(payloadWord & PayloadMask);
        bool maskMatches = maskedBits != 0;
        return Stop(
            capture,
            maskMatches
                ? SocketLargeQueueGateOutcome.PayloadMaskBranchesToHandoff
                : SocketLargeQueueGateOutcome.PayloadMaskFallsThroughToDeferredControl,
            initialStateAddress,
            diagnosticCallReached,
            gateByteAddress: gateByteAddress,
            laterStateAddress: laterStateAddress,
            payloadWordAddress: capture.PayloadPointer,
            payloadMaskedBits: maskedBits,
            stopAt: maskMatches ? HandoffRawFileOffset : DeferredControlRawFileOffset,
            route: maskMatches ? HandoffRawFileOffset : DeferredControlRawFileOffset);
    }

    public static bool IsDiagnosticSkippedState(uint state) =>
        state is 3 or 5 or 8;

    public static uint AddReceiverOffset(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    private static bool HasAnyPostInitialCapture(SocketLargeQueueGateCapture capture) =>
        capture.DiagnosticCallReturnedNormally is not null
        || capture.GateByteD95 is not null
        || capture.LaterStateDword108 is not null
        || capture.PayloadWord is not null;

    private static bool HasPostDiagnosticReturnCapture(SocketLargeQueueGateCapture capture) =>
        capture.GateByteD95 is not null
        || capture.LaterStateDword108 is not null
        || capture.PayloadWord is not null;

    private static bool HasLaterCaptures(SocketLargeQueueGateCapture capture) =>
        capture.LaterStateDword108 is not null
        || capture.PayloadWord is not null;

    private static SocketLargeQueueGatePlan Stop(
        SocketLargeQueueGateCapture capture,
        SocketLargeQueueGateOutcome outcome,
        uint initialStateAddress,
        bool diagnosticCallReached = false,
        uint? gateByteAddress = null,
        uint? laterStateAddress = null,
        uint? payloadWordAddress = null,
        ushort? payloadMaskedBits = null,
        uint? route = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            diagnosticCallReached,
            initialStateAddress,
            gateByteAddress,
            laterStateAddress,
            payloadWordAddress,
            payloadMaskedBits,
            route,
            stopAt);
}
