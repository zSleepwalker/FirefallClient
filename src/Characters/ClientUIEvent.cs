namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ClientUIEvent, metadata ID 196.
/// SharedTextReference is an opaque 32-bit process-local shared-text pointer.
/// </summary>
public readonly record struct ClientUIEvent(ulong Target, uint SharedTextReference)
{
    public const ushort MessageId = 196;
    public const int TargetOffset = 0x18;
    public const int SharedTextReferenceOffset = 0x28;
}
