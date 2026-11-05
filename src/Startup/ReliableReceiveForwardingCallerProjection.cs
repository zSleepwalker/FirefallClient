using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum ReliableReceiveForwardingCallerRoute
{
    HighDelta,
    ExistingRecord
}

public readonly record struct ReliableReceiveForwardingCallerCapture(
    ReliableReceiveForwardingCallerRoute Route,
    uint ReceiverBaseAddress,
    uint? ReceiverDwordAtOffset4,
    uint? ReceiverDwordAtOffset24,
    bool? HelperReturnedNormally,
    uint? ReceiverDwordAtOffset8);

public enum ReliableReceiveForwardingCallerOutcome
{
    MissingReceiverOffset4Capture,
    UnreachedLaterCapture,
    MissingReceiverOffset24Capture,
    UnreachedHelperReturnCapture,
    MissingHelperReturnCapture,
    HelperDidNotReturnNormally,
    UnreachedReceiverOffset8Capture,
    MissingReceiverOffset8Capture,
    CleanupCompleted
}

public readonly record struct ReliableReceiveForwardingCallerPlan(
    ReliableReceiveForwardingCallerCapture Capture,
    ReliableReceiveForwardingCallerOutcome Outcome,
    bool ReceiverOffset4Read,
    uint? ReceiverOffset4Address,
    bool UnreachedReceiverOffset4CaptureProvided,
    bool ReceiverOffset24Read,
    uint? ReceiverOffset24Address,
    bool UnreachedReceiverOffset24CaptureProvided,
    ImmutableArray<uint> PushedValuesInInstructionOrder,
    bool HelperCallReached,
    uint? HelperTargetRawFileOffset,
    uint? HelperCallContinuationRawFileOffset,
    bool? HelperReturnedNormally,
    bool UnreachedHelperReturnCaptureProvided,
    bool ReceiverOffset8Read,
    uint? ReceiverOffset8Address,
    uint? ReceiverOffset8DwordLoadedIntoEax,
    bool UnreachedReceiverOffset8CaptureProvided,
    bool CleanupReached,
    ushort? CleanupBytes,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveForwardingCallerInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the two observed callers through their six-argument helper call and
/// the caller cleanup, without inferring any behavior after the captured window.
/// </summary>
public static class ReliableReceiveForwardingCallerProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint ReceiverOffset4 = 0x04;
    public const uint ReceiverOffset8 = 0x08;
    public const uint ReceiverOffset24 = 0x24;
    public const ushort CallerCleanupBytes = 0x18;
    public const uint HelperEntryReturnAddressStackOffset = 0;
    public const uint HelperReceiverOffset24ArgumentStackOffset = 0x04;
    public const uint HelperZeroArgumentStackOffset = 0x08;
    public const uint HelperFormatArgumentStackOffset = 0x0C;
    public const uint HelperReceiverOffset4ArgumentStackOffset = 0x10;
    public const uint HelperFirstLiteralArgumentStackOffset = 0x14;
    public const uint HelperTagArgumentStackOffset = 0x18;
    public const uint HelperTargetRawFileOffset = 0x00BE2F50;
    public const uint FormatLiteralPointer = 0x01BB8BAC;
    public const uint ExistingRecordLiteralPointer = 0x01BB8B3C;
    public const uint HighDeltaFirstLiteralPointer = 0x01BB8C78;
    public const uint ExistingRecordFirstLiteralPointer = 0x01BB8B60;
    public const uint HighDeltaTag = 0x01EF;
    public const uint ExistingRecordTag = 0x01FB;

    private readonly record struct RouteMetadata(
        uint StartRawFileOffset,
        uint EndRawFileOffsetExclusive,
        uint FirstTag,
        uint FirstLiteral,
        uint SecondLiteral,
        uint CallRawFileOffset,
        int CallDisplacement,
        uint CallReturnRawFileOffset,
        uint ReceiverOffset4ReadRawFileOffset,
        uint ReceiverOffset24ReadRawFileOffset,
        uint ReceiverOffset8ReadRawFileOffset,
        uint CleanupRawFileOffset,
        string CapturedWindowBytes);

    private static readonly RouteMetadata HighDelta = new(
        0x00BE52A5,
        0x00BE52C7,
        HighDeltaTag,
        HighDeltaFirstLiteralPointer,
        FormatLiteralPointer,
        0x00BE52BC,
        unchecked((int)0xFFFFDC8F),
        0x00BE52C1,
        0x00BE52AF,
        0x00BE52B9,
        0x00BE52C1,
        0x00BE52C4,
        "68 EF 01 00 00 68 78 8C BB 01 FF 76 04 68 AC 8B BB 01 "
        + "6A 00 FF 76 24 E8 8F DC FF FF 8B 46 08 83 C4 18");

    private static readonly RouteMetadata ExistingRecord = new(
        0x00BE5353,
        0x00BE5375,
        ExistingRecordTag,
        ExistingRecordFirstLiteralPointer,
        ExistingRecordLiteralPointer,
        0x00BE536A,
        unchecked((int)0xFFFFDBE1),
        0x00BE536F,
        0x00BE535D,
        0x00BE5367,
        0x00BE536F,
        0x00BE5372,
        "68 FB 01 00 00 68 60 8B BB 01 FF 76 04 68 3C 8B BB 01 "
        + "6A 00 FF 76 24 E8 E1 DB FF FF 8B 46 08 83 C4 18");

    public static ImmutableArray<ReliableReceiveForwardingCallerInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveForwardingCallerInstructionEvidence(
                "Both routes push the tag and route-specific first literal before reading receiver+4",
                HighDelta.StartRawFileOffset,
                RawFileOffsetToVirtualAddress(HighDelta.StartRawFileOffset),
                "68 EF 01 00 00 68 78 8C BB 01"),
            new ReliableReceiveForwardingCallerInstructionEvidence(
                "Push DWORD [receiver+4], then the shared format literal and zero",
                HighDelta.ReceiverOffset4ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(HighDelta.ReceiverOffset4ReadRawFileOffset),
                "FF 76 04 68 AC 8B BB 01 6A 00"),
            new ReliableReceiveForwardingCallerInstructionEvidence(
                "Push DWORD [receiver+0x24], call raw 0xBE2F50, read EAX from receiver+8, then clean 0x18 bytes",
                HighDelta.ReceiverOffset24ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(HighDelta.ReceiverOffset24ReadRawFileOffset),
                "FF 76 24 E8 8F DC FF FF 8B 46 08 83 C4 18"));

    public static ImmutableArray<uint> HelperArgumentStackOffsets { get; } =
        ImmutableArray.Create(
            HelperReceiverOffset24ArgumentStackOffset,
            HelperZeroArgumentStackOffset,
            HelperFormatArgumentStackOffset,
            HelperReceiverOffset4ArgumentStackOffset,
            HelperFirstLiteralArgumentStackOffset,
            HelperTagArgumentStackOffset);

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverAddress(uint receiverBaseAddress, uint offset) =>
        unchecked(receiverBaseAddress + offset);

    public static uint RelativeCallTarget(uint instructionRawFileOffset, int displacement) =>
        unchecked(instructionRawFileOffset + 5u + (uint)displacement);

    public static uint StartRawFileOffset(ReliableReceiveForwardingCallerRoute route) =>
        Metadata(route).StartRawFileOffset;

    public static uint EndRawFileOffsetExclusive(ReliableReceiveForwardingCallerRoute route) =>
        Metadata(route).EndRawFileOffsetExclusive;

    public static uint CallRawFileOffset(ReliableReceiveForwardingCallerRoute route) =>
        Metadata(route).CallRawFileOffset;

    public static uint CallReturnRawFileOffset(ReliableReceiveForwardingCallerRoute route) =>
        Metadata(route).CallReturnRawFileOffset;

    public static uint ComputedHelperTargetRawFileOffset(ReliableReceiveForwardingCallerRoute route)
    {
        RouteMetadata metadata = Metadata(route);
        return RelativeCallTarget(metadata.CallRawFileOffset, metadata.CallDisplacement);
    }

    public static string CapturedWindowBytes(ReliableReceiveForwardingCallerRoute route) =>
        Metadata(route).CapturedWindowBytes;

    public static ImmutableArray<uint> HelperArgumentsInStackOrder(
        ReliableReceiveForwardingCallerCapture capture)
    {
        RouteMetadata metadata = Metadata(capture.Route);
        if (capture.ReceiverDwordAtOffset4 is not uint receiverOffset4
            || capture.ReceiverDwordAtOffset24 is not uint receiverOffset24)
        {
            return ImmutableArray<uint>.Empty;
        }

        return ImmutableArray.Create(
            receiverOffset24,
            0u,
            metadata.SecondLiteral,
            receiverOffset4,
            metadata.FirstLiteral,
            metadata.FirstTag);
    }

    public static ReliableReceiveForwardingCallerPlan Project(
        ReliableReceiveForwardingCallerCapture capture)
    {
        RouteMetadata metadata = Metadata(capture.Route);
        uint receiverOffset4Address = ReceiverAddress(capture.ReceiverBaseAddress, ReceiverOffset4);
        uint receiverOffset24Address = ReceiverAddress(capture.ReceiverBaseAddress, ReceiverOffset24);
        uint receiverOffset8Address = ReceiverAddress(capture.ReceiverBaseAddress, ReceiverOffset8);
        var pushes = ImmutableArray.CreateBuilder<uint>(6);
        pushes.Add(metadata.FirstTag);
        pushes.Add(metadata.FirstLiteral);

        if (capture.ReceiverDwordAtOffset4 is not uint receiverOffset4)
        {
            bool laterCaptureProvided = capture.ReceiverDwordAtOffset24.HasValue
                || capture.HelperReturnedNormally.HasValue
                || capture.ReceiverDwordAtOffset8.HasValue;
            return Stop(
                capture,
                laterCaptureProvided
                    ? ReliableReceiveForwardingCallerOutcome.UnreachedLaterCapture
                    : ReliableReceiveForwardingCallerOutcome.MissingReceiverOffset4Capture,
                receiverOffset4Read: false,
                receiverOffset4Address: receiverOffset4Address,
                unreachedReceiverOffset4CaptureProvided: false,
                receiverOffset24Read: false,
                receiverOffset24Address: null,
                unreachedReceiverOffset24CaptureProvided: capture.ReceiverDwordAtOffset24.HasValue,
                pushes,
                helperCallReached: false,
                helperReturnedNormally: null,
                unreachedHelperReturnCaptureProvided: capture.HelperReturnedNormally.HasValue,
                receiverOffset8Read: false,
                receiverOffset8Address: null,
                helperReturnValue: null,
                unreachedReceiverOffset8CaptureProvided: capture.ReceiverDwordAtOffset8.HasValue,
                cleanupReached: false,
                cleanupBytes: null,
                metadata.ReceiverOffset4ReadRawFileOffset);
        }

        pushes.Add(receiverOffset4);
        pushes.Add(metadata.SecondLiteral);
        pushes.Add(0);

        if (capture.ReceiverDwordAtOffset24 is not uint receiverOffset24)
        {
            bool laterCaptureProvided =
                capture.HelperReturnedNormally.HasValue || capture.ReceiverDwordAtOffset8.HasValue;
            return Stop(
                capture,
                laterCaptureProvided
                    ? ReliableReceiveForwardingCallerOutcome.UnreachedLaterCapture
                    : ReliableReceiveForwardingCallerOutcome.MissingReceiverOffset24Capture,
                receiverOffset4Read: true,
                receiverOffset4Address,
                unreachedReceiverOffset4CaptureProvided: false,
                receiverOffset24Read: false,
                receiverOffset24Address,
                unreachedReceiverOffset24CaptureProvided: false,
                pushes,
                helperCallReached: false,
                helperReturnedNormally: null,
                unreachedHelperReturnCaptureProvided: capture.HelperReturnedNormally.HasValue,
                receiverOffset8Read: false,
                receiverOffset8Address: null,
                helperReturnValue: null,
                unreachedReceiverOffset8CaptureProvided: capture.ReceiverDwordAtOffset8.HasValue,
                cleanupReached: false,
                cleanupBytes: null,
                metadata.ReceiverOffset24ReadRawFileOffset);
        }

        pushes.Add(receiverOffset24);
        if (capture.HelperReturnedNormally is not bool helperReturnedNormally)
        {
            return Stop(
                capture,
                capture.ReceiverDwordAtOffset8.HasValue
                    ? ReliableReceiveForwardingCallerOutcome.UnreachedReceiverOffset8Capture
                    : ReliableReceiveForwardingCallerOutcome.MissingHelperReturnCapture,
                receiverOffset4Read: true,
                receiverOffset4Address,
                unreachedReceiverOffset4CaptureProvided: false,
                receiverOffset24Read: true,
                receiverOffset24Address,
                unreachedReceiverOffset24CaptureProvided: false,
                pushes,
                helperCallReached: true,
                helperReturnedNormally: null,
                unreachedHelperReturnCaptureProvided: false,
                receiverOffset8Read: false,
                receiverOffset8Address: null,
                helperReturnValue: null,
                unreachedReceiverOffset8CaptureProvided: capture.ReceiverDwordAtOffset8.HasValue,
                cleanupReached: false,
                cleanupBytes: null,
                metadata.CallReturnRawFileOffset);
        }

        if (!helperReturnedNormally)
        {
            return Stop(
                capture,
                capture.ReceiverDwordAtOffset8.HasValue
                    ? ReliableReceiveForwardingCallerOutcome.UnreachedReceiverOffset8Capture
                    : ReliableReceiveForwardingCallerOutcome.HelperDidNotReturnNormally,
                receiverOffset4Read: true,
                receiverOffset4Address,
                unreachedReceiverOffset4CaptureProvided: false,
                receiverOffset24Read: true,
                receiverOffset24Address,
                unreachedReceiverOffset24CaptureProvided: false,
                pushes,
                helperCallReached: true,
                helperReturnedNormally: false,
                unreachedHelperReturnCaptureProvided: false,
                receiverOffset8Read: false,
                receiverOffset8Address: null,
                helperReturnValue: null,
                unreachedReceiverOffset8CaptureProvided: capture.ReceiverDwordAtOffset8.HasValue,
                cleanupReached: false,
                cleanupBytes: null,
                metadata.CallReturnRawFileOffset);
        }

        if (capture.ReceiverDwordAtOffset8 is not uint receiverOffset8)
        {
            return Stop(
                capture,
                ReliableReceiveForwardingCallerOutcome.MissingReceiverOffset8Capture,
                receiverOffset4Read: true,
                receiverOffset4Address,
                unreachedReceiverOffset4CaptureProvided: false,
                receiverOffset24Read: true,
                receiverOffset24Address,
                unreachedReceiverOffset24CaptureProvided: false,
                pushes,
                helperCallReached: true,
                helperReturnedNormally: true,
                unreachedHelperReturnCaptureProvided: false,
                receiverOffset8Read: false,
                receiverOffset8Address,
                helperReturnValue: null,
                unreachedReceiverOffset8CaptureProvided: false,
                cleanupReached: false,
                cleanupBytes: null,
                metadata.ReceiverOffset8ReadRawFileOffset);
        }

        return Stop(
            capture,
            ReliableReceiveForwardingCallerOutcome.CleanupCompleted,
            receiverOffset4Read: true,
            receiverOffset4Address,
            unreachedReceiverOffset4CaptureProvided: false,
            receiverOffset24Read: true,
            receiverOffset24Address,
            unreachedReceiverOffset24CaptureProvided: false,
            pushes,
            helperCallReached: true,
            helperReturnedNormally: true,
            unreachedHelperReturnCaptureProvided: false,
            receiverOffset8Read: true,
            receiverOffset8Address,
            helperReturnValue: receiverOffset8,
            unreachedReceiverOffset8CaptureProvided: false,
            cleanupReached: true,
            cleanupBytes: CallerCleanupBytes,
            metadata.EndRawFileOffsetExclusive);
    }

    private static ReliableReceiveForwardingCallerPlan Stop(
        ReliableReceiveForwardingCallerCapture capture,
        ReliableReceiveForwardingCallerOutcome outcome,
        bool receiverOffset4Read,
        uint? receiverOffset4Address,
        bool unreachedReceiverOffset4CaptureProvided,
        bool receiverOffset24Read,
        uint? receiverOffset24Address,
        bool unreachedReceiverOffset24CaptureProvided,
        ImmutableArray<uint>.Builder pushes,
        bool helperCallReached,
        bool? helperReturnedNormally,
        bool unreachedHelperReturnCaptureProvided,
        bool receiverOffset8Read,
        uint? receiverOffset8Address,
        uint? helperReturnValue,
        bool unreachedReceiverOffset8CaptureProvided,
        bool cleanupReached,
        ushort? cleanupBytes,
        uint stopAtRawFileOffsetExclusive)
    {
        RouteMetadata metadata = Metadata(capture.Route);
        return new ReliableReceiveForwardingCallerPlan(
            capture,
            outcome,
            receiverOffset4Read,
            receiverOffset4Address,
            unreachedReceiverOffset4CaptureProvided,
            receiverOffset24Read,
            receiverOffset24Address,
            unreachedReceiverOffset24CaptureProvided,
            pushes.ToImmutable(),
            helperCallReached,
            helperCallReached ? HelperTargetRawFileOffset : null,
            helperCallReached ? metadata.CallReturnRawFileOffset : null,
            helperReturnedNormally,
            unreachedHelperReturnCaptureProvided,
            receiverOffset8Read,
            receiverOffset8Address,
            ReceiverOffset8DwordLoadedIntoEax: helperReturnValue,
            unreachedReceiverOffset8CaptureProvided,
            cleanupReached,
            cleanupBytes,
            stopAtRawFileOffsetExclusive);
    }

    private static RouteMetadata Metadata(ReliableReceiveForwardingCallerRoute route) =>
        route switch
        {
            ReliableReceiveForwardingCallerRoute.HighDelta => HighDelta,
            ReliableReceiveForwardingCallerRoute.ExistingRecord => ExistingRecord,
            _ => throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown caller route.")
        };
}
