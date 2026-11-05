using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveCallbackScalarCapture(
    uint FrameEbp,
    bool? PriorIndirectCallReturnedNormally,
    uint? EntryEcx,
    byte? FrameByteAt8,
    uint? PriorCallbackEax,
    bool? HelperReturnedNormally);

public enum UnreliableReceiveCallbackScalarOutcome
{
    MissingPriorIndirectCallReturn,
    UnreachedCaptureProvided,
    PriorIndirectCallDidNotReturnNormally,
    MissingFrameByteAt8,
    MissingPriorCallbackEax,
    MissingHelperReturn,
    HelperDidNotReturnNormally,
    HelperReturnedNormally
}

public readonly record struct UnreliableReceiveCallbackScalarPlan(
    UnreliableReceiveCallbackScalarCapture Capture,
    UnreliableReceiveCallbackScalarOutcome Outcome,
    bool? PriorIndirectCallReturnedNormally,
    bool FrameByteRead,
    uint? FrameByteAddress,
    byte? CapturedCl,
    uint? CapturedEdx,
    uint? ProjectedEcx,
    bool UnreachedFrameByteCaptureProvided,
    bool UnreachedPriorCallbackEaxCaptureProvided,
    bool UnreachedHelperReturnCaptureProvided,
    bool HelperCallReached,
    bool? HelperReturnedNormally,
    bool NormalCallBoundaryReached,
    byte? StackCleanupBytes,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveCallbackScalarInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the frame-byte and prior-EAX scalar handoff through the opaque helper's normal return.
/// </summary>
public static class UnreliableReceiveCallbackScalarProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE5B25;
    public const uint FrameByteReadRawFileOffset = EntryRawFileOffset;
    public const uint PriorCallbackEaxCopyRawFileOffset = 0x00BE5B28;
    public const uint HelperCallRawFileOffset = 0x00BE5B2A;
    public const uint HelperCallContinuationRawFileOffset = 0x00BE5B2F;
    public const uint StackCleanupRawFileOffset = HelperCallContinuationRawFileOffset;
    public const uint EndRawFileOffsetExclusive = 0x00BE5B32;

    public const uint EntryVirtualAddress = 0x00FE6725;
    public const uint HelperCallTargetAddressFromIda = 0x00BD58E0;
    public const uint FrameByteOffset = 8;
    public const byte ExplicitPushCount = 0;
    public const byte StackCleanupAmount = 4;

    public const string CapturedWindowBytes =
        "8A 4D 08 8B D0 E8 B1 FD FE FF 83 C4 04";

    public static ImmutableArray<UnreliableReceiveCallbackScalarInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveCallbackScalarInstructionEvidence(
                "Read the frame byte at EBP+8 into CL; ECX upper bits are unchanged",
                FrameByteReadRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameByteReadRawFileOffset),
                "8A 4D 08"),
            new UnreliableReceiveCallbackScalarInstructionEvidence(
                "Copy the full prior callback EAX value into EDX",
                PriorCallbackEaxCopyRawFileOffset,
                RawFileOffsetToVirtualAddress(PriorCallbackEaxCopyRawFileOffset),
                "8B D0"),
            new UnreliableReceiveCallbackScalarInstructionEvidence(
                "Call the opaque helper; normal continuation is raw 0xBE5B2F",
                HelperCallRawFileOffset,
                RawFileOffsetToVirtualAddress(HelperCallRawFileOffset),
                "E8 B1 FD FE FF"),
            new UnreliableReceiveCallbackScalarInstructionEvidence(
                "On normal helper return, add four to ESP; no preceding push is shown in this window",
                StackCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(StackCleanupRawFileOffset),
                "83 C4 04"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FrameByteAddress(uint frameEbp) =>
        unchecked(frameEbp + FrameByteOffset);

    public static uint ReplaceLowByte(uint entryEcx, byte frameByte) =>
        (entryEcx & 0xFFFFFF00u) | frameByte;

    public static UnreliableReceiveCallbackScalarPlan Project(
        UnreliableReceiveCallbackScalarCapture capture)
    {
        if (capture.PriorIndirectCallReturnedNormally is not bool priorReturnedNormally)
        {
            return Stop(
                capture,
                HasAnyAfterPriorCall(capture)
                    ? UnreliableReceiveCallbackScalarOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveCallbackScalarOutcome.MissingPriorIndirectCallReturn,
                stopAt: EntryRawFileOffset,
                unreachedFrameByteCaptureProvided: capture.FrameByteAt8.HasValue,
                unreachedPriorCallbackEaxCaptureProvided: capture.PriorCallbackEax.HasValue,
                unreachedHelperReturnCaptureProvided: capture.HelperReturnedNormally.HasValue);
        }

        if (!priorReturnedNormally)
        {
            return Stop(
                capture,
                HasAnyAfterPriorCall(capture)
                    ? UnreliableReceiveCallbackScalarOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveCallbackScalarOutcome.PriorIndirectCallDidNotReturnNormally,
                priorIndirectCallReturnedNormally: false,
                stopAt: EntryRawFileOffset,
                unreachedFrameByteCaptureProvided: capture.FrameByteAt8.HasValue,
                unreachedPriorCallbackEaxCaptureProvided: capture.PriorCallbackEax.HasValue,
                unreachedHelperReturnCaptureProvided: capture.HelperReturnedNormally.HasValue);
        }

        uint frameByteAddress = FrameByteAddress(capture.FrameEbp);
        if (capture.FrameByteAt8 is not byte frameByte)
        {
            return Stop(
                capture,
                HasAnyAfterFrameByte(capture)
                    ? UnreliableReceiveCallbackScalarOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveCallbackScalarOutcome.MissingFrameByteAt8,
                priorIndirectCallReturnedNormally: true,
                frameByteAddress: frameByteAddress,
                unreachedPriorCallbackEaxCaptureProvided: capture.PriorCallbackEax.HasValue,
                unreachedHelperReturnCaptureProvided: capture.HelperReturnedNormally.HasValue,
                stopAt: FrameByteReadRawFileOffset);
        }

        uint? projectedEcx = capture.EntryEcx is uint entryEcx
            ? ReplaceLowByte(entryEcx, frameByte)
            : null;

        if (capture.PriorCallbackEax is not uint priorCallbackEax)
        {
            return Stop(
                capture,
                capture.HelperReturnedNormally.HasValue
                    ? UnreliableReceiveCallbackScalarOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveCallbackScalarOutcome.MissingPriorCallbackEax,
                priorIndirectCallReturnedNormally: true,
                frameByteRead: true,
                frameByteAddress: frameByteAddress,
                capturedCl: frameByte,
                projectedEcx: projectedEcx,
                unreachedHelperReturnCaptureProvided: capture.HelperReturnedNormally.HasValue,
                stopAt: PriorCallbackEaxCopyRawFileOffset);
        }

        if (capture.HelperReturnedNormally is not bool helperReturnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveCallbackScalarOutcome.MissingHelperReturn,
                priorIndirectCallReturnedNormally: true,
                frameByteRead: true,
                frameByteAddress: frameByteAddress,
                capturedCl: frameByte,
                capturedEdx: priorCallbackEax,
                projectedEcx: projectedEcx,
                helperCallReached: true,
                stopAt: HelperCallContinuationRawFileOffset);
        }

        if (!helperReturnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveCallbackScalarOutcome.HelperDidNotReturnNormally,
                priorIndirectCallReturnedNormally: true,
                frameByteRead: true,
                frameByteAddress: frameByteAddress,
                capturedCl: frameByte,
                capturedEdx: priorCallbackEax,
                projectedEcx: projectedEcx,
                helperCallReached: true,
                helperReturnedNormally: false,
                stopAt: HelperCallContinuationRawFileOffset);
        }

        return Stop(
            capture,
            UnreliableReceiveCallbackScalarOutcome.HelperReturnedNormally,
            priorIndirectCallReturnedNormally: true,
            frameByteRead: true,
            frameByteAddress: frameByteAddress,
            capturedCl: frameByte,
            capturedEdx: priorCallbackEax,
            projectedEcx: projectedEcx,
            helperCallReached: true,
            helperReturnedNormally: true,
            normalCallBoundaryReached: true,
            stackCleanupBytes: StackCleanupAmount,
            selectedDestinationRawFileOffset: EndRawFileOffsetExclusive,
            stopAt: EndRawFileOffsetExclusive);
    }

    private static bool HasAnyAfterPriorCall(UnreliableReceiveCallbackScalarCapture capture) =>
        capture.FrameByteAt8 is not null
        || capture.PriorCallbackEax is not null
        || capture.HelperReturnedNormally is not null;

    private static bool HasAnyAfterFrameByte(UnreliableReceiveCallbackScalarCapture capture) =>
        capture.PriorCallbackEax is not null
        || capture.HelperReturnedNormally is not null;

    private static UnreliableReceiveCallbackScalarPlan Stop(
        UnreliableReceiveCallbackScalarCapture capture,
        UnreliableReceiveCallbackScalarOutcome outcome,
        bool? priorIndirectCallReturnedNormally = null,
        bool frameByteRead = false,
        uint? frameByteAddress = null,
        byte? capturedCl = null,
        uint? capturedEdx = null,
        uint? projectedEcx = null,
        bool unreachedFrameByteCaptureProvided = false,
        bool unreachedPriorCallbackEaxCaptureProvided = false,
        bool unreachedHelperReturnCaptureProvided = false,
        bool helperCallReached = false,
        bool? helperReturnedNormally = null,
        bool normalCallBoundaryReached = false,
        byte? stackCleanupBytes = null,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            priorIndirectCallReturnedNormally,
            frameByteRead,
            frameByteAddress,
            capturedCl,
            capturedEdx,
            projectedEcx,
            unreachedFrameByteCaptureProvided,
            unreachedPriorCallbackEaxCaptureProvided,
            unreachedHelperReturnCaptureProvided,
            helperCallReached,
            helperReturnedNormally,
            normalCallBoundaryReached,
            stackCleanupBytes,
            selectedDestinationRawFileOffset,
            stopAt);
}
