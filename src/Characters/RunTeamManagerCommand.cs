namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RunTeamManagerCommand, metadata ID 232.
/// The fields are retained positionally except for the reference-counted text pointer.
/// </summary>
public readonly record struct RunTeamManagerCommand(
    ulong Target,
    byte Value28,
    uint SharedTextReference,
    byte Value30)
{
    public const ushort MessageId = 232;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int SharedTextReferenceOffset = 0x2C;
    public const int Value30Offset = 0x30;
}
