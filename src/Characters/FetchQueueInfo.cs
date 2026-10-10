namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::FetchQueueInfo, metadata ID 89.
/// The vector contains retained opaque 32-bit references.
/// </summary>
public sealed class FetchQueueInfo
{
    public const ushort MessageId = 89;
    public const int TargetOffset = 0x18;
    public const int ReferencesOffset = 0x28;
    public const int ReferenceSize = 4;
    public const int Value38Offset = 0x38;
    public const int Value3COffset = 0x3C;
    public const int Value40Offset = 0x40;

    public FetchQueueInfo(
        ulong target,
        IEnumerable<uint> references,
        uint value38,
        uint value3C,
        uint value40)
    {
        ArgumentNullException.ThrowIfNull(references);
        Target = target;
        References = Array.AsReadOnly(references.ToArray());
        Value38 = value38;
        Value3C = value3C;
        Value40 = value40;
    }

    public ulong Target { get; }
    public IReadOnlyList<uint> References { get; }
    public uint Value38 { get; }
    public uint Value3C { get; }
    public uint Value40 { get; }
}
