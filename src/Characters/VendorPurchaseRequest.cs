namespace Firefall.Characters;

/// <summary>A presence-flagged native DWORD. Value is meaningful only when IsPresent.</summary>
public readonly record struct NativeOptionalDword(bool IsPresent, uint Value);

/// <summary>
/// Native Character::Commands::VendorPurchaseRequest, metadata ID 238.
/// SharedText5C.Value is an opaque 32-bit process-local shared-text pointer.
/// </summary>
public readonly record struct VendorPurchaseRequest(
    ulong Target,
    ulong Value28,
    ulong Value30,
    ulong Value38,
    ulong Value40,
    NativeOptionalDword Value4C,
    NativeOptionalDword Value54,
    NativeOptionalDword SharedText5C)
{
    public const ushort MessageId = 238;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value30Offset = 0x30;
    public const int Value38Offset = 0x38;
    public const int Value40Offset = 0x40;
    public const int HasValue48Offset = 0x48;
    public const int Value4COffset = 0x4C;
    public const int HasValue50Offset = 0x50;
    public const int Value54Offset = 0x54;
    public const int HasSharedText58Offset = 0x58;
    public const int SharedText5COffset = 0x5C;
}
