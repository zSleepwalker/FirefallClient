namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::DeployableCalldownRequest, metadata ID 180.
/// Field names stay positional because their gameplay meanings are unknown.
/// The 0x40-byte block is copied verbatim by the native constructor.
/// </summary>
public sealed class DeployableCalldownRequest
{
    public const ushort MessageId = 180;
    public const int ValueOffset = 0x28;
    public const int ValuesOffset = 0x2C;
    public const int OpaqueBlockOffset = 0x38;
    public const int OpaqueBlockSize = 0x40;
    public const int TrailingValuesOffset = 0x78;
    public const int FlagsOffset = 0x88;

    public DeployableCalldownRequest(
        ulong target,
        uint value28,
        uint value2C,
        uint value30,
        uint value34,
        ReadOnlyMemory<byte> opaqueBlock,
        uint value78,
        uint value7C,
        uint value80,
        uint value84,
        byte flag88,
        byte flag89)
    {
        if (opaqueBlock.Length != OpaqueBlockSize)
            throw new ArgumentException($"Expected exactly {OpaqueBlockSize} opaque bytes.", nameof(opaqueBlock));

        Target = target;
        Value28 = value28;
        Value2C = value2C;
        Value30 = value30;
        Value34 = value34;
        OpaqueBlock = opaqueBlock.ToArray();
        Value78 = value78;
        Value7C = value7C;
        Value80 = value80;
        Value84 = value84;
        Flag88 = flag88;
        Flag89 = flag89;
    }

    public ulong Target { get; }
    public uint Value28 { get; }
    public uint Value2C { get; }
    public uint Value30 { get; }
    public uint Value34 { get; }
    public ReadOnlyMemory<byte> OpaqueBlock { get; }
    public uint Value78 { get; }
    public uint Value7C { get; }
    public uint Value80 { get; }
    public uint Value84 { get; }
    public byte Flag88 { get; }
    public byte Flag89 { get; }
}
