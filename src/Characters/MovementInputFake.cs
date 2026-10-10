namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::MovementInputFake, metadata ID 116. Its
/// optional descriptor contains an unmodeled process-local shared reference.
/// </summary>
public sealed class MovementInputFake
{
    public const ushort MessageId = 116;
    public const int Value28Offset = 0x28;
    public const int OptionalFlagOffset = 0x3C;
    public const int OptionalDescriptorOffset = 0x40;
    public const int OptionalDescriptorSize = 0x88;
    public const int SharedReferenceOffset = 0xC8;
    public const int OptionalValueOffset = 0xCC;
    public const int ValueD0Offset = 0xD0;
    public const int FlagD2Offset = 0xD2;

    public MovementInputFake(
        ulong target,
        ushort value28,
        uint value2A,
        uint value2E,
        uint value32,
        ushort value36,
        byte value38,
        ReadOnlyMemory<byte> optionalDescriptorPrefix,
        ushort? optionalValueCC,
        ushort valueD0,
        byte flagD2)
    {
        if (optionalDescriptorPrefix.Length is not 0 and not OptionalDescriptorSize)
            throw new ArgumentException(
                $"Expected either 0 or {OptionalDescriptorSize} optional descriptor bytes.",
                nameof(optionalDescriptorPrefix));
        if (optionalDescriptorPrefix.IsEmpty != (optionalValueCC is null))
            throw new ArgumentException("The optional descriptor and its trailing value must be supplied together.");

        Target = target;
        Value28 = value28;
        Value2A = value2A;
        Value2E = value2E;
        Value32 = value32;
        Value36 = value36;
        Value38 = value38;
        OptionalDescriptorPrefix = optionalDescriptorPrefix.ToArray();
        OptionalValueCC = optionalValueCC;
        ValueD0 = valueD0;
        FlagD2 = flagD2;
    }

    public ulong Target { get; }
    public ushort Value28 { get; }
    public uint Value2A { get; }
    public uint Value2E { get; }
    public uint Value32 { get; }
    public ushort Value36 { get; }
    public byte Value38 { get; }
    public bool HasOptionalDescriptor => !OptionalDescriptorPrefix.IsEmpty;
    public ReadOnlyMemory<byte> OptionalDescriptorPrefix { get; }
    public ushort? OptionalValueCC { get; }
    public ushort ValueD0 { get; }
    public byte FlagD2 { get; }
}
