using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveResultStoresCapture(
    uint ReceiverEdi,
    uint ResultEsi,
    uint FrameEbp,
    uint? Receiver4,
    uint? FrameMinus4,
    uint? Receiver4C);

public enum UnreliableReceiveResultStoresOutcome
{
    InvalidEntry,
    UnreachedCaptureProvided,
    MissingReceiver4,
    MissingFrameMinus4,
    MissingReceiver4C,
    CounterBelowThreshold,
    CounterAtOrAboveThreshold
}

public readonly record struct UnreliableReceiveResultStoresPlan(
    UnreliableReceiveResultStoresCapture Capture,
    UnreliableReceiveResultStoresOutcome Outcome,
    bool ReceiverEdiNonzero,
    bool ResultEsiNonzero,
    bool UnreachedReceiver4CaptureProvided,
    bool UnreachedFrameMinus4CaptureProvided,
    bool UnreachedReceiver4CCaptureProvided,
    bool Receiver4Read,
    uint? Receiver4Address,
    uint? CapturedReceiver4,
    bool FrameMinus4Read,
    uint? FrameMinus4Address,
    uint? CapturedFrameMinus4,
    bool Receiver4Stored,
    uint? Receiver4StoreAddress,
    uint? Receiver4StoredValue,
    bool FrameMinus4LowWordStored,
    uint? FrameMinus4StoreAddress,
    uint? FrameMinus4LowWordStoredValue,
    bool Receiver4CRead,
    uint? Receiver4CAddress,
    uint? CapturedReceiver4C,
    bool? CounterBelowThreshold,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveResultStoresInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the result-field stores and unsigned receiver counter gate at the nonzero-result edge.
/// </summary>
public static class UnreliableReceiveResultStoresProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE5BAC;
    public const uint EntryVirtualAddress = 0x00FE67AC;
    public const uint Receiver4ReadRawFileOffset = EntryRawFileOffset;
    public const uint FrameMinus4ReadRawFileOffset = 0x00BE5BAF;
    public const uint Receiver4StoreRawFileOffset = 0x00BE5BB2;
    public const uint FrameMinus4LowWordZeroExtendRawFileOffset = 0x00BE5BB5;
    public const uint FrameMinus4LowWordStoreRawFileOffset = 0x00BE5BB8;
    public const uint Receiver4CReadRawFileOffset = 0x00BE5BBB;
    public const uint CounterBranchRawFileOffset = 0x00BE5BC2;
    public const uint BelowThresholdDestinationRawFileOffset = 0x00BE5C0F;
    public const uint AtOrAboveThresholdDestinationRawFileOffset = 0x00BE5BC4;
    public const uint EndRawFileOffsetExclusive = 0x00BE5BC4;

    public const uint Receiver4Offset = 4;
    public const uint FrameMinus4Offset = 4;
    public const uint Receiver4COffset = 0x4C;
    public const uint Receiver4StoreOffset = 0x20;
    public const uint FrameMinus4LowWordStoreOffset = 0x24;
    public const uint Receiver4CCounterThreshold = 0x00010000;

    public const string CapturedWindowBytes =
        "8B 47 04 8B 4D FC 89 46 20 0F B7 C1 89 46 24 81 7F 4C 00 00 01 00 72 4B";

    public static ImmutableArray<UnreliableReceiveResultStoresInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveResultStoresInstructionEvidence(
                "Read the DWORD at [EDI+4] into EAX",
                Receiver4ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(Receiver4ReadRawFileOffset),
                "8B 47 04"),
            new UnreliableReceiveResultStoresInstructionEvidence(
                "Independently reload the DWORD at [EBP-4] into ECX",
                FrameMinus4ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameMinus4ReadRawFileOffset),
                "8B 4D FC"),
            new UnreliableReceiveResultStoresInstructionEvidence(
                "Store the captured receiver DWORD at [ESI+20]",
                Receiver4StoreRawFileOffset,
                RawFileOffsetToVirtualAddress(Receiver4StoreRawFileOffset),
                "89 46 20"),
            new UnreliableReceiveResultStoresInstructionEvidence(
                "Zero-extend CX, the low WORD of the reloaded ECX, into EAX",
                FrameMinus4LowWordZeroExtendRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameMinus4LowWordZeroExtendRawFileOffset),
                "0F B7 C1"),
            new UnreliableReceiveResultStoresInstructionEvidence(
                "Store the zero-extended WORD at [ESI+24]",
                FrameMinus4LowWordStoreRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameMinus4LowWordStoreRawFileOffset),
                "89 46 24"),
            new UnreliableReceiveResultStoresInstructionEvidence(
                "Compare DWORD [EDI+4C] unsigned against 0x10000",
                Receiver4CReadRawFileOffset,
                RawFileOffsetToVirtualAddress(Receiver4CReadRawFileOffset),
                "81 7F 4C 00 00 01 00"),
            new UnreliableReceiveResultStoresInstructionEvidence(
                "Branch below threshold to raw 0xBE5C0F; otherwise continue at raw 0xBE5BC4",
                CounterBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(CounterBranchRawFileOffset),
                "72 4B"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint Receiver4Address(uint receiverEdi) =>
        unchecked(receiverEdi + Receiver4Offset);

    public static uint FrameMinus4Address(uint frameEbp) =>
        unchecked(frameEbp - FrameMinus4Offset);

    public static uint Receiver4CAddress(uint receiverEdi) =>
        unchecked(receiverEdi + Receiver4COffset);

    public static uint Receiver4StoreAddress(uint resultEsi) =>
        unchecked(resultEsi + Receiver4StoreOffset);

    public static uint FrameMinus4LowWordStoreAddress(uint resultEsi) =>
        unchecked(resultEsi + FrameMinus4LowWordStoreOffset);

    public static UnreliableReceiveResultStoresPlan Project(
        UnreliableReceiveResultStoresCapture capture)
    {
        bool receiverEdiNonzero = capture.ReceiverEdi != 0;
        bool resultEsiNonzero = capture.ResultEsi != 0;
        if (!resultEsiNonzero)
        {
            return Stop(
                capture,
                UnreliableReceiveResultStoresOutcome.InvalidEntry,
                receiverEdiNonzero,
                resultEsiNonzero,
                unreachedReceiver4CaptureProvided: capture.Receiver4.HasValue,
                unreachedFrameMinus4CaptureProvided: capture.FrameMinus4.HasValue,
                unreachedReceiver4CCaptureProvided: capture.Receiver4C.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint receiver4Address = Receiver4Address(capture.ReceiverEdi);
        if (capture.Receiver4 is not uint receiver4)
        {
            return Stop(
                capture,
                HasLaterCapture(capture)
                    ? UnreliableReceiveResultStoresOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveResultStoresOutcome.MissingReceiver4,
                receiverEdiNonzero,
                resultEsiNonzero,
                receiver4Address: receiver4Address,
                unreachedFrameMinus4CaptureProvided: capture.FrameMinus4.HasValue,
                unreachedReceiver4CCaptureProvided: capture.Receiver4C.HasValue,
                stopAt: Receiver4ReadRawFileOffset);
        }

        uint frameMinus4Address = FrameMinus4Address(capture.FrameEbp);
        if (capture.FrameMinus4 is not uint frameMinus4)
        {
            return Stop(
                capture,
                capture.Receiver4C.HasValue
                    ? UnreliableReceiveResultStoresOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveResultStoresOutcome.MissingFrameMinus4,
                receiverEdiNonzero,
                resultEsiNonzero,
                receiver4Read: true,
                receiver4Address: receiver4Address,
                capturedReceiver4: receiver4,
                frameMinus4Address: frameMinus4Address,
                unreachedReceiver4CCaptureProvided: capture.Receiver4C.HasValue,
                stopAt: FrameMinus4ReadRawFileOffset);
        }

        uint receiver4StoreAddress = Receiver4StoreAddress(capture.ResultEsi);
        uint frameMinus4StoreAddress = FrameMinus4LowWordStoreAddress(capture.ResultEsi);
        uint frameMinus4LowWord = frameMinus4 & 0xFFFF;

        uint receiver4CAddress = Receiver4CAddress(capture.ReceiverEdi);
        if (capture.Receiver4C is not uint receiver4C)
        {
            return Stop(
                capture,
                UnreliableReceiveResultStoresOutcome.MissingReceiver4C,
                receiverEdiNonzero,
                resultEsiNonzero,
                receiver4Read: true,
                receiver4Address: receiver4Address,
                capturedReceiver4: receiver4,
                frameMinus4Read: true,
                frameMinus4Address: frameMinus4Address,
                capturedFrameMinus4: frameMinus4,
                receiver4Stored: true,
                receiver4StoreAddress: receiver4StoreAddress,
                receiver4StoredValue: receiver4,
                frameMinus4LowWordStored: true,
                frameMinus4StoreAddress: frameMinus4StoreAddress,
                frameMinus4LowWordStoredValue: frameMinus4LowWord,
                receiver4CAddress: receiver4CAddress,
                stopAt: Receiver4CReadRawFileOffset);
        }

        bool belowThreshold = receiver4C < Receiver4CCounterThreshold;
        return Stop(
            capture,
            belowThreshold
                ? UnreliableReceiveResultStoresOutcome.CounterBelowThreshold
                : UnreliableReceiveResultStoresOutcome.CounterAtOrAboveThreshold,
            receiverEdiNonzero,
            resultEsiNonzero,
            receiver4Read: true,
            receiver4Address: receiver4Address,
            capturedReceiver4: receiver4,
            frameMinus4Read: true,
            frameMinus4Address: frameMinus4Address,
            capturedFrameMinus4: frameMinus4,
            receiver4Stored: true,
            receiver4StoreAddress: receiver4StoreAddress,
            receiver4StoredValue: receiver4,
            frameMinus4LowWordStored: true,
            frameMinus4StoreAddress: frameMinus4StoreAddress,
            frameMinus4LowWordStoredValue: frameMinus4LowWord,
            receiver4CRead: true,
            receiver4CAddress: receiver4CAddress,
            capturedReceiver4C: receiver4C,
            counterBelowThreshold: belowThreshold,
            selectedDestinationRawFileOffset: belowThreshold
                ? BelowThresholdDestinationRawFileOffset
                : AtOrAboveThresholdDestinationRawFileOffset,
            stopAt: belowThreshold
                ? BelowThresholdDestinationRawFileOffset
                : AtOrAboveThresholdDestinationRawFileOffset);
    }

    private static bool HasLaterCapture(UnreliableReceiveResultStoresCapture capture) =>
        capture.FrameMinus4.HasValue || capture.Receiver4C.HasValue;

    private static UnreliableReceiveResultStoresPlan Stop(
        UnreliableReceiveResultStoresCapture capture,
        UnreliableReceiveResultStoresOutcome outcome,
        bool receiverEdiNonzero,
        bool resultEsiNonzero,
        bool receiver4Read = false,
        uint? receiver4Address = null,
        uint? capturedReceiver4 = null,
        bool unreachedReceiver4CaptureProvided = false,
        bool unreachedFrameMinus4CaptureProvided = false,
        bool unreachedReceiver4CCaptureProvided = false,
        bool frameMinus4Read = false,
        uint? frameMinus4Address = null,
        uint? capturedFrameMinus4 = null,
        bool receiver4Stored = false,
        uint? receiver4StoreAddress = null,
        uint? receiver4StoredValue = null,
        bool frameMinus4LowWordStored = false,
        uint? frameMinus4StoreAddress = null,
        uint? frameMinus4LowWordStoredValue = null,
        bool receiver4CRead = false,
        uint? receiver4CAddress = null,
        uint? capturedReceiver4C = null,
        bool? counterBelowThreshold = null,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            receiverEdiNonzero,
            resultEsiNonzero,
            unreachedReceiver4CaptureProvided,
            unreachedFrameMinus4CaptureProvided,
            unreachedReceiver4CCaptureProvided,
            receiver4Read,
            receiver4Address,
            capturedReceiver4,
            frameMinus4Read,
            frameMinus4Address,
            capturedFrameMinus4,
            receiver4Stored,
            receiver4StoreAddress,
            receiver4StoredValue,
            frameMinus4LowWordStored,
            frameMinus4StoreAddress,
            frameMinus4LowWordStoredValue,
            receiver4CRead,
            receiver4CAddress,
            capturedReceiver4C,
            counterBelowThreshold,
            selectedDestinationRawFileOffset,
            stopAt);
}
