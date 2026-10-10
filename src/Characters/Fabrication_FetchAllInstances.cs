namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::Fabrication_FetchAllInstances, metadata ID 240.
/// Its constructor initializes no subclass payload fields.
/// </summary>
public readonly record struct Fabrication_FetchAllInstances(ulong Target)
{
    public const ushort MessageId = 240;
    public const int TargetOffset = 0x18;
}
