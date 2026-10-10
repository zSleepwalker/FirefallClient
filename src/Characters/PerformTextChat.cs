namespace Firefall.Characters;

/// <summary>
/// Optional nested descriptor stored at PerformTextChat +0x50.
/// The first two DWORDs are copied by a shared-text value helper.
/// </summary>
public readonly record struct PerformTextChatOptionalDescriptor(
    uint SharedTextValue,
    uint SharedTextOwner,
    uint Value5C);

/// <summary>
/// Native Character::Commands::PerformTextChat, metadata ID 145.
/// Conditional fields are nullable where the native constructor leaves storage untouched.
/// </summary>
public sealed class PerformTextChat
{
    public const ushort MessageId = 145;
    public const int TargetOffset = 0x18;
    public const int SharedText28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int HasNestedPairOffset = 0x30;
    public const int Value38Offset = 0x38;
    public const int Value3COffset = 0x3C;
    public const int HasSharedText44Offset = 0x40;
    public const int SharedText44Offset = 0x44;
    public const int Value48Offset = 0x48;
    public const int HasOptionalDescriptorOffset = 0x4C;
    public const int OptionalDescriptorOffset = 0x50;
    public const uint OptionalDescriptorVtable = 0x1C80CC4;

    public PerformTextChat(
        ulong target,
        uint sharedText28,
        byte value2C,
        bool hasNestedPair,
        uint? value38,
        uint? value3C,
        bool hasSharedText44,
        uint? sharedText44,
        byte value48,
        PerformTextChatOptionalDescriptor? optionalDescriptor)
    {
        Target = target;
        SharedText28 = sharedText28;
        Value2C = value2C;
        HasNestedPair = hasNestedPair;
        Value38 = value38;
        Value3C = value3C;
        HasSharedText44 = hasSharedText44;
        SharedText44 = sharedText44;
        Value48 = value48;
        OptionalDescriptor = optionalDescriptor;
    }

    public ulong Target { get; }
    public uint SharedText28 { get; }
    public byte Value2C { get; }
    public bool HasNestedPair { get; }
    public uint? Value38 { get; }
    public uint? Value3C { get; }
    public bool HasSharedText44 { get; }
    public uint? SharedText44 { get; }
    public byte Value48 { get; }
    public PerformTextChatOptionalDescriptor? OptionalDescriptor { get; }
}
