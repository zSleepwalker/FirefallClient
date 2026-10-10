namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::AiSignal, metadata ID 192.
/// Text references and the copied 16-byte block are opaque process-local data.
/// </summary>
public readonly record struct AiSignal(
    ulong Target,
    uint FirstSharedTextReference,
    uint SecondSharedTextReference,
    AiSignalOpaqueBlock Block,
    uint Value70,
    uint Value74)
{
    public const ushort MessageId = 192;
    public const int FirstSharedTextReferenceOffset = 0x28;
    public const int SecondSharedTextReferenceOffset = 0x2C;
    public const int BlockOffset = 0x30;
    public const int Value70Offset = 0x70;
    public const int Value74Offset = 0x74;
}

public readonly record struct AiSignalOpaqueBlock(uint Word0, uint Word1, uint Word2, uint Word3);
