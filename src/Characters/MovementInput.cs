namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::MovementInput, metadata ID 115. The caller
/// copies a partially understood descriptor; the process-local shared
/// reference within it is intentionally not represented here.
/// </summary>
public sealed class MovementInput
{
    public const ushort MessageId = 115;
    public const int Value28Offset = 0x28;
    public const int Value2AOffset = 0x2A;
    public const int Value2EOffset = 0x2E;
    public const int DescriptorOffset = 0x34;
    public const int DescriptorPrefixSize = 0x88;
    public const int NativeConstantOffset = 0x94;
    public const ushort NativeConstantValue = 0x0101;
    public const int SharedReferenceOffset = 0xBC;
    public const int ValueC0Offset = 0xC0;
    public const int ValueC4Offset = 0xC4;
    public const int FlagC6Offset = 0xC6;

    public MovementInput(
        ulong target,
        ushort value28,
        uint value2A,
        uint value2E,
        ReadOnlyMemory<byte> descriptorPrefix,
        ushort valueC0,
        ushort valueC4,
        byte flagC6)
    {
        if (descriptorPrefix.Length != DescriptorPrefixSize)
            throw new ArgumentException($"Expected exactly {DescriptorPrefixSize} descriptor bytes.", nameof(descriptorPrefix));

        Target = target;
        Value28 = value28;
        Value2A = value2A;
        Value2E = value2E;
        DescriptorPrefix = descriptorPrefix.ToArray();
        ValueC0 = valueC0;
        ValueC4 = valueC4;
        FlagC6 = flagC6;
    }

    public ulong Target { get; }
    public ushort Value28 { get; }
    public uint Value2A { get; }
    public uint Value2E { get; }
    public ReadOnlyMemory<byte> DescriptorPrefix { get; }
    public ushort ValueC0 { get; }
    public ushort ValueC4 { get; }
    public byte FlagC6 { get; }
}
