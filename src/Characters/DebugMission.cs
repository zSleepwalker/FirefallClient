namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::DebugMission, metadata ID 154.
/// SharedTextReference is an opaque 32-bit process-local shared-text pointer.
/// </summary>
public readonly record struct DebugMission(
    ulong Target,
    uint Value,
    uint SharedTextReference)
{
    public const ushort MessageId = 154;
    public const int ValueOffset = 0x28;
    public const int SharedTextReferenceOffset = 0x2C;
}
