namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SetDialogTag, metadata ID 147.
/// SharedTextReference is an opaque 32-bit process-local shared-text pointer.
/// </summary>
public readonly record struct SetDialogTag(
    ulong Target,
    uint SharedTextReference,
    uint Value2,
    byte Value3,
    byte Value4)
{
    public const ushort MessageId = 147;
    public const int SharedTextReferenceOffset = 0x28;
    public const int Value2Offset = 0x2C;
    public const int Value3Offset = 0x30;
    public const int Value4Offset = 0x31;
}
