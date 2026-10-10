namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::AiError, metadata ID 191.
/// The shared-text reference is opaque process-local data.
/// </summary>
public readonly record struct AiError(ulong Target, uint SharedTextReference)
{
    public const ushort MessageId = 191;
    public const int SharedTextReferenceOffset = 0x28;
}
