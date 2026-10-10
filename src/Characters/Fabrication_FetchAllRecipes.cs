namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::Fabrication_FetchAllRecipes, metadata ID 241.
/// Its constructor initializes no subclass payload fields.
/// </summary>
public readonly record struct Fabrication_FetchAllRecipes(ulong Target)
{
    public const ushort MessageId = 241;
    public const int TargetOffset = 0x18;
}
