namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::SetBountyVar, metadata ID 166.
/// The two references are opaque 32-bit process-local native pointers.
/// </summary>
public readonly record struct SetBountyVar(
    ulong Target,
    uint Value,
    uint OpaqueReference1,
    uint OpaqueReference2)
{
    public const ushort MessageId = 166;
    public const int ValueOffset = 0x28;
    public const int OpaqueReference1Offset = 0x2C;
    public const int OpaqueReference2Offset = 0x30;
}
