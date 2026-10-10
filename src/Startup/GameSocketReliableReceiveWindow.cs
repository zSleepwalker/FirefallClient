namespace Firefall.Reconstruction.Startup;

public enum GameSocketReceiveSequenceClass
{
    WithinWindow,
    BeforeWindow,
    BeyondWindow
}

/// <summary>Unsigned modular receive-window classification; no packet storage or acknowledgments are simulated.</summary>
public static class GameSocketReliableReceiveWindow
{
    public const int WindowSize = 256;
    public const int HalfSequenceSpace = 0x8000;
    public const uint ReceiveMethodFileOffset = 0xBE51F0;
    public const uint ConsumeContiguousMethodFileOffset = 0xBE56D0;
    public const uint AssembleAndDecodeMethodFileOffset = 0xBE5440;
    public const int NextReceiveSequenceOffset = 0x2A;
    public const int DecodedDeliveryQueueOffset = 0x60;
    public const int FragmentRingOffset = 0x78;
    public const int FragmentRingHeadIndexOffset = 0x478;
    public const int SequenceTrackerOffset = 0x47C;
    public const int ReassemblyBufferOffset = 0x594;
    public const int DecodeFunctionPointerOffset = 0x14;
    public const int DecodedObjectChannelOffset = 0x20;
    public const int DecodedObjectPayloadSizeOffset = 0x24;

    public static GameSocketReceiveSequenceClass Classify(ushort sequence, ushort nextExpectedSequence)
    {
        ushort distance = unchecked((ushort)(sequence - nextExpectedSequence));
        return distance < WindowSize ? GameSocketReceiveSequenceClass.WithinWindow
            : distance >= HalfSequenceSpace ? GameSocketReceiveSequenceClass.BeforeWindow
            : GameSocketReceiveSequenceClass.BeyondWindow;
    }

    public static int GetRingSlot(ushort sequence, ushort nextExpectedSequence, int headIndex)
    {
        if (headIndex is < 0 or >= WindowSize)
            throw new ArgumentOutOfRangeException(nameof(headIndex));
        if (Classify(sequence, nextExpectedSequence) != GameSocketReceiveSequenceClass.WithinWindow)
            throw new ArgumentOutOfRangeException(nameof(sequence), "Only in-window fragments occupy receive-ring slots.");
        return (headIndex + unchecked((ushort)(sequence - nextExpectedSequence))) % WindowSize;
    }
}
