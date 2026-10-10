namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::DeployableHardpointSelection, metadata ID 181.
/// The three subclass DWORDs are retained positionally.
/// </summary>
public readonly record struct DeployableHardpointSelection(
    ulong Target,
    uint Value28,
    uint Value2C,
    uint Value30)
{
    public const ushort MessageId = 181;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
}
