namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::NonDevDebugCommand, metadata ID 193.
/// Its trailing shared-text reference is opaque process-local data.
/// </summary>
public readonly record struct NonDevDebugCommand(
    ulong Target,
    byte Value28,
    uint Value2C,
    uint SharedTextReference)
{
    public const ushort MessageId = 193;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int SharedTextReferenceOffset = 0x30;
}
