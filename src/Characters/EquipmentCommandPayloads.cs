namespace Firefall.Characters;

/// <summary>
/// Native payloads for three Character::Commands equipment messages.
/// These are layout models, not wire encoders: the per-message serialization
/// and the meaning of several fields have not been verified.
/// Offsets are relative to the beginning of the native message object.
/// </summary>
public static class EquipmentCommandOffsets
{
    public const int Target = 0x18;
    public const int Payload = 0x28;
}

/// <summary>
/// SlotModuleRequest (native message 204). Constructor 0x539F60 copies a
/// two-DWORD value to +0x28 and a vector of two-DWORD elements to +0x30.
/// </summary>
public sealed class SlotModuleRequestPayload
{
    public const ushort MessageId = 204;
    public const int IdentifierOffset = 0x28;
    public const int EntriesOffset = 0x30;

    public SlotModuleRequestPayload(ulong target, ulong identifier, IEnumerable<ModuleSlotEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Target = target;
        Identifier = identifier;
        Entries = Array.AsReadOnly(entries.ToArray());
    }

    public ulong Target { get; }
    public ulong Identifier { get; }
    public IReadOnlyList<ModuleSlotEntry> Entries { get; }
}

/// <summary>
/// An eight-byte SlotModuleRequest vector element; the meanings of its
/// components have not been established.
/// </summary>
public readonly record struct ModuleSlotEntry(uint First, uint Second);

/// <summary>
/// UnslotAllModulesRequest (native message 205). Constructor 0x53A130
/// copies the two DWORDs at +0x28/+0x2C from a caller-provided value.
/// </summary>
public readonly record struct UnslotAllModulesRequestPayload(ulong Target, ulong Identifier)
{
    public const ushort MessageId = 205;
    public const int IdentifierOffset = 0x28;
}

/// <summary>
/// SlotVisualRequest (native message 207). Constructor 0x53A540 stores two
/// DWORDs, two bytes, two further DWORDs and a vector of DWORDs.
/// Their application-level meanings have not been established.
/// </summary>
public sealed class SlotVisualRequestPayload
{
    public const ushort MessageId = 207;
    public const int FirstOffset = 0x28;
    public const int SecondOffset = 0x2C;
    public const int FirstFlagOffset = 0x30;
    public const int SecondFlagOffset = 0x31;
    public const int ThirdOffset = 0x34;
    public const int FourthOffset = 0x38;
    public const int ValuesOffset = 0x3C;

    public SlotVisualRequestPayload(
        ulong target, uint first, uint second, byte firstFlag, byte secondFlag,
        uint third, uint fourth, IEnumerable<uint> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        Target = target;
        First = first;
        Second = second;
        FirstFlag = firstFlag;
        SecondFlag = secondFlag;
        Third = third;
        Fourth = fourth;
        Values = Array.AsReadOnly(values.ToArray());
    }

    public ulong Target { get; }
    public uint First { get; }
    public uint Second { get; }
    public byte FirstFlag { get; }
    public byte SecondFlag { get; }
    public uint Third { get; }
    public uint Fourth { get; }
    public IReadOnlyList<uint> Values { get; }
}
