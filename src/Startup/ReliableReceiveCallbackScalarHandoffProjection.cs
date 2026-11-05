using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct ReliableReceiveCallbackScalarHandoffCapture(
    bool? PriorCallbackReturnedNormally,
    uint? PriorCallbackEax,
    uint? EntryEcx,
    byte? RetainedBl,
    bool? HelperReturnedNormally);

public enum ReliableReceiveCallbackScalarHandoffOutcome
{
    MissingPriorCallbackReturn,
    UnreachedCaptureProvided,
    PriorCallbackDidNotReturnNormally,
    MissingPriorCallbackEax,
    MissingRetainedBl,
    MissingHelperReturn,
    HelperDidNotReturnNormally,
    HelperReturnedNormally
}

public readonly record struct ReliableReceiveCallbackScalarHandoffPlan(
    ReliableReceiveCallbackScalarHandoffCapture Capture,
    ReliableReceiveCallbackScalarHandoffOutcome Outcome,
    bool? PriorCallbackReturnedNormally,
    uint? CapturedEdx,
    byte? CapturedCl,
    uint? ProjectedEcx,
    bool HelperCallReached,
    bool? HelperReturnedNormally,
    byte? StackCleanupBytes,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveCallbackScalarHandoffInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the scalar-register handoff to an opaque helper, preserving the
/// distinction between the captured ECX low byte and unknown upper bits.
/// </summary>
public static class ReliableReceiveCallbackScalarHandoffProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE53DD;
    public const uint MoveEaxToEdxRawFileOffset = EntryRawFileOffset;
    public const uint MoveBlToClRawFileOffset = 0x00BE53DF;
    public const uint HelperCallRawFileOffset = 0x00BE53E1;
    public const uint HelperCallContinuationRawFileOffset = 0x00BE53E6;
    public const uint StackCleanupRawFileOffset = HelperCallContinuationRawFileOffset;
    public const uint EndRawFileOffsetExclusive = 0x00BE53E9;

    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint HelperCallVirtualAddress =
        HelperCallRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint HelperCallContinuationVirtualAddress =
        HelperCallContinuationRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EndVirtualAddressExclusive =
        EndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint HelperCallTargetAddressFromIda = 0x00BD58E0;
    public const byte ExplicitPushCount = 0;
    public const byte StackCleanupAmount = 4;

    public const string CapturedWindowBytes =
        "8B D0 8A CB E8 FA 04 FF FF 83 C4 04";

    public static ImmutableArray<ReliableReceiveCallbackScalarHandoffInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveCallbackScalarHandoffInstructionEvidence(
                "Copy the full prior callback EAX value into EDX",
                MoveEaxToEdxRawFileOffset,
                RawFileOffsetToVirtualAddress(MoveEaxToEdxRawFileOffset),
                "8B D0"),
            new ReliableReceiveCallbackScalarHandoffInstructionEvidence(
                "Copy BL into CL only; ECX upper bits are not overwritten",
                MoveBlToClRawFileOffset,
                RawFileOffsetToVirtualAddress(MoveBlToClRawFileOffset),
                "8A CB"),
            new ReliableReceiveCallbackScalarHandoffInstructionEvidence(
                "Call the opaque helper; normal continuation is raw 0xBE53E6",
                HelperCallRawFileOffset,
                RawFileOffsetToVirtualAddress(HelperCallRawFileOffset),
                "E8 FA 04 FF FF"),
            new ReliableReceiveCallbackScalarHandoffInstructionEvidence(
                "On normal helper return, add four to ESP; this window does not show the source of that stack item",
                StackCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(StackCleanupRawFileOffset),
                "83 C4 04"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReplaceLowByte(uint entryEcx, byte retainedBl) =>
        (entryEcx & 0xFFFFFF00u) | retainedBl;

    public static ReliableReceiveCallbackScalarHandoffPlan Project(
        ReliableReceiveCallbackScalarHandoffCapture capture)
    {
        if (capture.PriorCallbackReturnedNormally is not bool priorReturnedNormally)
        {
            return Stop(
                capture,
                HasAnyAfterPriorCallback(capture)
                    ? ReliableReceiveCallbackScalarHandoffOutcome.UnreachedCaptureProvided
                    : ReliableReceiveCallbackScalarHandoffOutcome.MissingPriorCallbackReturn,
                priorCallbackReturnedNormally: null,
                stopAt: EntryRawFileOffset);
        }

        if (!priorReturnedNormally)
        {
            return Stop(
                capture,
                HasAnyAfterPriorCallback(capture)
                    ? ReliableReceiveCallbackScalarHandoffOutcome.UnreachedCaptureProvided
                    : ReliableReceiveCallbackScalarHandoffOutcome.PriorCallbackDidNotReturnNormally,
                priorCallbackReturnedNormally: false,
                stopAt: EntryRawFileOffset);
        }

        if (capture.PriorCallbackEax is not uint priorCallbackEax)
        {
            return Stop(
                capture,
                HasAnyAfterEax(capture)
                    ? ReliableReceiveCallbackScalarHandoffOutcome.UnreachedCaptureProvided
                    : ReliableReceiveCallbackScalarHandoffOutcome.MissingPriorCallbackEax,
                priorCallbackReturnedNormally: true,
                stopAt: MoveEaxToEdxRawFileOffset);
        }

        if (capture.RetainedBl is not byte retainedBl)
        {
            return Stop(
                capture,
                HasAnyAfterBl(capture)
                    ? ReliableReceiveCallbackScalarHandoffOutcome.UnreachedCaptureProvided
                    : ReliableReceiveCallbackScalarHandoffOutcome.MissingRetainedBl,
                priorCallbackReturnedNormally: true,
                capturedEdx: priorCallbackEax,
                stopAt: MoveBlToClRawFileOffset);
        }

        uint? projectedEcx = capture.EntryEcx is uint entryEcx
            ? ReplaceLowByte(entryEcx, retainedBl)
            : null;

        if (capture.HelperReturnedNormally is not bool helperReturnedNormally)
        {
            return Stop(
                capture,
                ReliableReceiveCallbackScalarHandoffOutcome.MissingHelperReturn,
                priorCallbackReturnedNormally: true,
                capturedEdx: priorCallbackEax,
                capturedCl: retainedBl,
                projectedEcx: projectedEcx,
                helperCallReached: true,
                stopAt: HelperCallContinuationRawFileOffset);
        }

        if (!helperReturnedNormally)
        {
            return Stop(
                capture,
                ReliableReceiveCallbackScalarHandoffOutcome.HelperDidNotReturnNormally,
                priorCallbackReturnedNormally: true,
                capturedEdx: priorCallbackEax,
                capturedCl: retainedBl,
                projectedEcx: projectedEcx,
                helperCallReached: true,
                helperReturnedNormally: false,
                stopAt: HelperCallContinuationRawFileOffset);
        }

        return Stop(
            capture,
            ReliableReceiveCallbackScalarHandoffOutcome.HelperReturnedNormally,
            priorCallbackReturnedNormally: true,
            capturedEdx: priorCallbackEax,
            capturedCl: retainedBl,
            projectedEcx: projectedEcx,
            helperCallReached: true,
            helperReturnedNormally: true,
            stackCleanupBytes: StackCleanupAmount,
            selectedDestinationRawFileOffset: EndRawFileOffsetExclusive,
            stopAt: EndRawFileOffsetExclusive);
    }

    private static bool HasAnyAfterPriorCallback(
        ReliableReceiveCallbackScalarHandoffCapture capture) =>
        capture.PriorCallbackEax is not null
        || capture.EntryEcx is not null
        || capture.RetainedBl is not null
        || capture.HelperReturnedNormally is not null;

    private static bool HasAnyAfterEax(
        ReliableReceiveCallbackScalarHandoffCapture capture) =>
        capture.RetainedBl is not null
        || capture.HelperReturnedNormally is not null;

    private static bool HasAnyAfterBl(
        ReliableReceiveCallbackScalarHandoffCapture capture) =>
        capture.HelperReturnedNormally is not null;

    private static ReliableReceiveCallbackScalarHandoffPlan Stop(
        ReliableReceiveCallbackScalarHandoffCapture capture,
        ReliableReceiveCallbackScalarHandoffOutcome outcome,
        bool? priorCallbackReturnedNormally,
        uint? capturedEdx = null,
        byte? capturedCl = null,
        uint? projectedEcx = null,
        bool helperCallReached = false,
        bool? helperReturnedNormally = null,
        byte? stackCleanupBytes = null,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            priorCallbackReturnedNormally,
            capturedEdx,
            capturedCl,
            projectedEcx,
            helperCallReached,
            helperReturnedNormally,
            stackCleanupBytes,
            selectedDestinationRawFileOffset,
            stopAt);
}
