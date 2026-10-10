namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::VendorTokenMachineRequest, metadata ID 200.
/// Field meanings beyond the observed request modes are unknown.
/// </summary>
public readonly record struct VendorTokenMachineRequest(
    ulong Target,
    byte Mode,
    uint Value2C,
    uint Value30)
{
    public const ushort MessageId = 200;
    public const int TargetOffset = 0x18;
    public const int ModeOffset = 0x28;
    public const int Reserved29Offset = 0x29;
    public const byte Reserved29Value = 0;
    public const int Value2COffset = 0x2C;
    public const int Value30Offset = 0x30;
}
