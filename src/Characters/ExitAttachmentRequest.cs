namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ExitAttachmentRequest, metadata ID 140.
/// The observed constructor has no subclass payload fields.
/// </summary>
public readonly record struct ExitAttachmentRequest(ulong Target)
{
    public const ushort MessageId = 140;
}
