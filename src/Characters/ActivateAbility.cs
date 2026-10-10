namespace Firefall.Characters;

/// <summary>
/// An eight-byte positional entry observed in ActivateAbility's inline vector.
/// The meaning of either DWORD is unknown.
/// </summary>
public readonly record struct ActivateAbilityEntry(uint First, uint Second);

/// <summary>
/// Native snapshot of Character::Commands::ActivateAbility, metadata ID 134.
/// The optional trailing DWORDs are null when their runtime values were not observed.
/// </summary>
public sealed class ActivateAbility
{
    public const ushort MessageId = 134;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int EntriesOffset = 0x30;
    public const int InlineStorageOffset = 0x4C;
    public const int ObservedInlineCapacity = 4;
    public const int Value6COffset = 0x6C;
    public const int Value70Offset = 0x70;
    public const int Value74Offset = 0x74;
    public const int Value78Offset = 0x78;

    public ActivateAbility(
        ulong target,
        uint value28,
        byte value2C,
        IEnumerable<ActivateAbilityEntry> entries,
        byte value78,
        uint? value6C = null,
        uint? value70 = null,
        uint? value74 = null)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Target = target;
        Value28 = value28;
        Value2C = value2C;
        Entries = Array.AsReadOnly(entries.ToArray());
        Value6C = value6C;
        Value70 = value70;
        Value74 = value74;
        Value78 = value78;
    }

    public ulong Target { get; }
    public uint Value28 { get; }
    public byte Value2C { get; }
    public IReadOnlyList<ActivateAbilityEntry> Entries { get; }
    public uint? Value6C { get; }
    public uint? Value70 { get; }
    public uint? Value74 { get; }
    public byte Value78 { get; }
}
