using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveCallbackRecycleCapture(
    bool? PriorCallbackReturnedNormally,
    uint FrameEbp,
    uint PayloadEbx,
    uint? GlobalDwordAtFirstRead,
    uint? CallbackEaxAtCopy,
    bool? RecycleHelperReturnedNormally);

public enum UnreliableReceiveCallbackRecycleOutcome
{
    MissingPriorCallbackReturn,
    UnreachedCaptureProvided,
    PriorCallbackDidNotReturnNormally,
    MissingGlobalDwordAtFirstRead,
    MissingCallbackEaxAtCopy,
    MissingRecycleHelperReturn,
    RecycleHelperDidNotReturnNormally,
    RecycleHelperReturnedNormally
}

public readonly record struct UnreliableReceiveCallbackRecyclePlan(
    UnreliableReceiveCallbackRecycleCapture Capture,
    UnreliableReceiveCallbackRecycleOutcome Outcome,
    bool? PriorCallbackReturnedNormally,
    bool GlobalDwordRead,
    uint? CapturedGlobalDword,
    uint? ProjectedEcx,
    bool StackCleanupReached,
    byte? StackCleanupBytes,
    bool CallbackEaxCopied,
    uint? CapturedCallbackEax,
    bool FrameDwordWriteReached,
    uint? FrameDwordAddress,
    uint? FrameDwordWritten,
    bool PayloadPushReached,
    ImmutableArray<uint> StackPushesInInstructionOrder,
    bool RecycleHelperCallReached,
    uint? CallEcx,
    bool? RecycleHelperReturnedNormally,
    bool NormalCallBoundaryReached,
    bool UnreachedGlobalDwordCaptureProvided,
    bool UnreachedCallbackEaxCaptureProvided,
    bool UnreachedRecycleHelperReturnCaptureProvided,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveCallbackRecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the callback's first global-DWORD read, stack cleanup, frame copy,
/// payload push, and opaque recycle-helper call through its normal return.
/// </summary>
public static class UnreliableReceiveCallbackRecycleProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE5B64;
    public const uint GlobalDwordReadRawFileOffset = EntryRawFileOffset;
    public const uint StackCleanupRawFileOffset = 0x00BE5B6A;
    public const uint CallbackEaxCopyRawFileOffset = 0x00BE5B6D;
    public const uint FrameDwordWriteRawFileOffset = 0x00BE5B6F;
    public const uint PayloadPushRawFileOffset = 0x00BE5B72;
    public const uint RecycleHelperCallRawFileOffset = 0x00BE5B73;
    public const uint NormalCallBoundaryRawFileOffset = 0x00BE5B78;
    public const uint EndRawFileOffsetExclusive = NormalCallBoundaryRawFileOffset;

    public const uint EntryVirtualAddress = 0x00FE6764;
    public const uint GlobalDwordVirtualAddress = 0x01DFFF84;
    public const uint RecycleHelperTargetRawFileOffset = 0x00BD5C60;
    public const uint RecycleHelperTargetVirtualAddress = 0x00FD6860;
    public const uint FrameDwordOffset = 8;
    public const byte StackCleanupAmount = 0x10;

    public const string CapturedWindowBytes =
        "8B 0D 84 FF DF 01 83 C4 10 8B F0 89 75 08 53 E8 E8 00 FF FF";

    public static ImmutableArray<UnreliableReceiveCallbackRecycleInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveCallbackRecycleInstructionEvidence(
                "Load the DWORD at absolute VA 0x01DFFF84 into ECX",
                GlobalDwordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(GlobalDwordReadRawFileOffset),
                "8B 0D 84 FF DF 01"),
            new UnreliableReceiveCallbackRecycleInstructionEvidence(
                "On this path, clean up 0x10 bytes left by the preceding callback",
                StackCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(StackCleanupRawFileOffset),
                "83 C4 10"),
            new UnreliableReceiveCallbackRecycleInstructionEvidence(
                "Copy the full callback EAX value to ESI",
                CallbackEaxCopyRawFileOffset,
                RawFileOffsetToVirtualAddress(CallbackEaxCopyRawFileOffset),
                "8B F0"),
            new UnreliableReceiveCallbackRecycleInstructionEvidence(
                "Write ESI to the DWORD at EBP+8",
                FrameDwordWriteRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameDwordWriteRawFileOffset),
                "89 75 08"),
            new UnreliableReceiveCallbackRecycleInstructionEvidence(
                "Push the payload value in EBX",
                PayloadPushRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadPushRawFileOffset),
                "53"),
            new UnreliableReceiveCallbackRecycleInstructionEvidence(
                "Call the helper whose PE-mapped target is raw 0xBD5C60; the normal-return boundary is immediately after this call",
                RecycleHelperCallRawFileOffset,
                RawFileOffsetToVirtualAddress(RecycleHelperCallRawFileOffset),
                "E8 E8 00 FF FF"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FrameDwordAddress(uint frameEbp) =>
        unchecked(frameEbp + FrameDwordOffset);

    public static UnreliableReceiveCallbackRecyclePlan Project(
        UnreliableReceiveCallbackRecycleCapture capture)
    {
        if (capture.PriorCallbackReturnedNormally is not bool priorCallbackReturnedNormally)
        {
            return Stop(
                capture,
                HasAnyAfterPriorCallback(capture)
                    ? UnreliableReceiveCallbackRecycleOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveCallbackRecycleOutcome.MissingPriorCallbackReturn,
                stopAt: EntryRawFileOffset,
                unreachedGlobalDwordCaptureProvided: capture.GlobalDwordAtFirstRead.HasValue,
                unreachedCallbackEaxCaptureProvided: capture.CallbackEaxAtCopy.HasValue,
                unreachedRecycleHelperReturnCaptureProvided:
                    capture.RecycleHelperReturnedNormally.HasValue);
        }

        if (!priorCallbackReturnedNormally)
        {
            return Stop(
                capture,
                HasAnyAfterPriorCallback(capture)
                    ? UnreliableReceiveCallbackRecycleOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveCallbackRecycleOutcome.PriorCallbackDidNotReturnNormally,
                priorCallbackReturnedNormally: false,
                stopAt: EntryRawFileOffset,
                unreachedGlobalDwordCaptureProvided: capture.GlobalDwordAtFirstRead.HasValue,
                unreachedCallbackEaxCaptureProvided: capture.CallbackEaxAtCopy.HasValue,
                unreachedRecycleHelperReturnCaptureProvided:
                    capture.RecycleHelperReturnedNormally.HasValue);
        }

        if (capture.GlobalDwordAtFirstRead is not uint globalDword)
        {
            return Stop(
                capture,
                HasAnyAfterGlobalRead(capture)
                    ? UnreliableReceiveCallbackRecycleOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveCallbackRecycleOutcome.MissingGlobalDwordAtFirstRead,
                priorCallbackReturnedNormally: true,
                unreachedCallbackEaxCaptureProvided: capture.CallbackEaxAtCopy.HasValue,
                unreachedRecycleHelperReturnCaptureProvided:
                    capture.RecycleHelperReturnedNormally.HasValue,
                stopAt: GlobalDwordReadRawFileOffset);
        }

        if (capture.CallbackEaxAtCopy is not uint callbackEax)
        {
            return Stop(
                capture,
                capture.RecycleHelperReturnedNormally.HasValue
                    ? UnreliableReceiveCallbackRecycleOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveCallbackRecycleOutcome.MissingCallbackEaxAtCopy,
                priorCallbackReturnedNormally: true,
                globalDwordRead: true,
                capturedGlobalDword: globalDword,
                projectedEcx: globalDword,
                stackCleanupReached: true,
                stackCleanupBytes: StackCleanupAmount,
                unreachedRecycleHelperReturnCaptureProvided:
                    capture.RecycleHelperReturnedNormally.HasValue,
                stopAt: CallbackEaxCopyRawFileOffset);
        }

        uint frameDwordAddress = FrameDwordAddress(capture.FrameEbp);
        ImmutableArray<uint> pushes = ImmutableArray.Create(capture.PayloadEbx);

        if (capture.RecycleHelperReturnedNormally is not bool helperReturnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveCallbackRecycleOutcome.MissingRecycleHelperReturn,
                priorCallbackReturnedNormally: true,
                globalDwordRead: true,
                capturedGlobalDword: globalDword,
                projectedEcx: globalDword,
                stackCleanupReached: true,
                stackCleanupBytes: StackCleanupAmount,
                callbackEaxCopied: true,
                capturedCallbackEax: callbackEax,
                frameDwordWriteReached: true,
                frameDwordAddress: frameDwordAddress,
                frameDwordWritten: callbackEax,
                payloadPushReached: true,
                stackPushesInInstructionOrder: pushes,
                recycleHelperCallReached: true,
                callEcx: globalDword,
                stopAt: RecycleHelperCallRawFileOffset);
        }

        if (!helperReturnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveCallbackRecycleOutcome.RecycleHelperDidNotReturnNormally,
                priorCallbackReturnedNormally: true,
                globalDwordRead: true,
                capturedGlobalDword: globalDword,
                projectedEcx: globalDword,
                stackCleanupReached: true,
                stackCleanupBytes: StackCleanupAmount,
                callbackEaxCopied: true,
                capturedCallbackEax: callbackEax,
                frameDwordWriteReached: true,
                frameDwordAddress: frameDwordAddress,
                frameDwordWritten: callbackEax,
                payloadPushReached: true,
                stackPushesInInstructionOrder: pushes,
                recycleHelperCallReached: true,
                callEcx: globalDword,
                recycleHelperReturnedNormally: false,
                stopAt: RecycleHelperCallRawFileOffset);
        }

        return Stop(
            capture,
            UnreliableReceiveCallbackRecycleOutcome.RecycleHelperReturnedNormally,
            priorCallbackReturnedNormally: true,
            globalDwordRead: true,
            capturedGlobalDword: globalDword,
            projectedEcx: globalDword,
            stackCleanupReached: true,
            stackCleanupBytes: StackCleanupAmount,
            callbackEaxCopied: true,
            capturedCallbackEax: callbackEax,
            frameDwordWriteReached: true,
            frameDwordAddress: frameDwordAddress,
            frameDwordWritten: callbackEax,
            payloadPushReached: true,
            stackPushesInInstructionOrder: pushes,
            recycleHelperCallReached: true,
            callEcx: globalDword,
            recycleHelperReturnedNormally: true,
            normalCallBoundaryReached: true,
            selectedDestinationRawFileOffset: NormalCallBoundaryRawFileOffset,
            stopAt: NormalCallBoundaryRawFileOffset);
    }

    private static bool HasAnyAfterPriorCallback(
        UnreliableReceiveCallbackRecycleCapture capture) =>
        capture.GlobalDwordAtFirstRead is not null
        || capture.CallbackEaxAtCopy is not null
        || capture.RecycleHelperReturnedNormally is not null;

    private static bool HasAnyAfterGlobalRead(
        UnreliableReceiveCallbackRecycleCapture capture) =>
        capture.CallbackEaxAtCopy is not null
        || capture.RecycleHelperReturnedNormally is not null;

    private static UnreliableReceiveCallbackRecyclePlan Stop(
        UnreliableReceiveCallbackRecycleCapture capture,
        UnreliableReceiveCallbackRecycleOutcome outcome,
        bool? priorCallbackReturnedNormally = null,
        bool globalDwordRead = false,
        uint? capturedGlobalDword = null,
        uint? projectedEcx = null,
        bool stackCleanupReached = false,
        byte? stackCleanupBytes = null,
        bool callbackEaxCopied = false,
        uint? capturedCallbackEax = null,
        bool frameDwordWriteReached = false,
        uint? frameDwordAddress = null,
        uint? frameDwordWritten = null,
        bool payloadPushReached = false,
        ImmutableArray<uint> stackPushesInInstructionOrder = default,
        bool recycleHelperCallReached = false,
        uint? callEcx = null,
        bool? recycleHelperReturnedNormally = null,
        bool normalCallBoundaryReached = false,
        bool unreachedGlobalDwordCaptureProvided = false,
        bool unreachedCallbackEaxCaptureProvided = false,
        bool unreachedRecycleHelperReturnCaptureProvided = false,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            priorCallbackReturnedNormally,
            globalDwordRead,
            capturedGlobalDword,
            projectedEcx,
            stackCleanupReached,
            stackCleanupBytes,
            callbackEaxCopied,
            capturedCallbackEax,
            frameDwordWriteReached,
            frameDwordAddress,
            frameDwordWritten,
            payloadPushReached,
            stackPushesInInstructionOrder.IsDefault
                ? ImmutableArray<uint>.Empty
                : stackPushesInInstructionOrder,
            recycleHelperCallReached,
            callEcx,
            recycleHelperReturnedNormally,
            normalCallBoundaryReached,
            unreachedGlobalDwordCaptureProvided,
            unreachedCallbackEaxCaptureProvided,
            unreachedRecycleHelperReturnCaptureProvided,
            selectedDestinationRawFileOffset,
            stopAt);
}
