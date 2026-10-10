// Evidence-based reconstruction of firefall::Character::Commands messages
// 174 / 175 / 194. All addresses are raw FILE OFFSETS (IDA base 0).
// See README.md for the complete evidence table and open questions.
namespace Firefall.Characters;

/// <summary>
/// Model of the nl::MessageBase header fields that these three commands populate.
/// Native layout (32-bit), written by 0x5A6780 / 0x5A67F0 and inline in 0x9095B0 (0x909775..0x9097B7):
///   +0x00 vtable
///   +0x04 not written by the traced constructors (UNKNOWN)
///   +0x08 u32 = 1 (UNKNOWN meaning; constant in all three)
///   +0x0C char* debug name ("msg-174"/"msg-175"/"msg-194")
///   +0x10 u32 message id
///   +0x14 not written by the traced constructors (UNKNOWN)
///   +0x18 u32 target low dword  } packed together as one little-endian u64
///   +0x1C u32 target high dword }
///   +0x20 u32 = 0, +0x24 u32 = 0 (UNKNOWN; never read by the traced encoders)
///   +0x28.. per-message payload
/// </summary>
public abstract class CharacterCommandMessage
{
    /// <summary>
    /// Low byte every traced builder forces into the target id:
    /// <c>(*(ctrl-&gt;[+4]+8) &amp; 0xFFFFFF00) | 4</c> (e.g. `or ebx, 4` at 0x909794 in 0x9095B0).
    /// Meaning of the value 4 is UNKNOWN; constructors reject kinds outside 1..14.
    /// </summary>
    public const byte BuilderTargetKind = 4;

    protected CharacterCommandMessage(ulong target) => Target = target;

    /// <summary>MessageID packed by 0x320900 (see <see cref="CharacterCommandWire.WriteMessageId"/>).</summary>
    public abstract ushort MessageId { get; }

    /// <summary>Native debug/log name stored at +0x0C (used by "ROUTED - Sent %s").</summary>
    public abstract string DebugName { get; }

    /// <summary>Native RTTI class name (MSVC type descriptor).</summary>
    public abstract string NativeClassName { get; }

    /// <summary>
    /// 64-bit value at +0x18/+0x1C. Constructor diagnostics call it a "target"
    /// ("Invalid target: 0x%016llx", used by 0x5363E0). Builders copy it from
    /// the local controller's sub-object (+4) at offsets +8/+0xC, replacing the low byte.
    /// Exact entity semantics beyond "target id" are UNKNOWN.
    /// </summary>
    public ulong Target { get; }

    /// <summary>Byte count passed to the packer Reserve() before writing (performance hint only).</summary>
    public abstract int NativeReserveHint { get; }

    /// <summary>Writes the per-message payload that follows the MessageID.</summary>
    public abstract void WritePayload(BinaryWriter writer);

    /// <summary>
    /// Mirrors the constructors' target check (0x3265A0 validity + kind range at `cmp al/bl, 0Dh` at 0x536441 / 0x536675 / 0x90982A).
    /// Native code only reports the failure through an assert/log handler; it does not abort construction.
    /// </summary>
    public bool TargetPassesNativeCheck
    {
        get
        {
            if (!CharacterCommandWire.IsValidTarget(Target)) return true; // native skips the kind check for invalid ids
            byte kind = (byte)Target;
            return (byte)(kind - 1) <= 0x0D;
        }
    }

    /// <summary>Applies the builders' "low byte := 4" rule to a raw 64-bit id.</summary>
    public static ulong TargetFromControllerId(ulong rawId) => (rawId & ~0xFFUL) | BuilderTargetKind;
}

/// <summary>
/// Message 0xAE (174), RTTI <c>firefall::Character::Commands::LogDirectActivityRequest</c>
/// (vtable file 0x18651A8). Lua usage string: "Usage: ActivityDirector.LogDirectActivityRequest();".
/// Encoder 0x90FD40: target + MessageID only; no additional payload.
/// </summary>
public sealed class LogDirectActivityRequest(ulong target) : CharacterCommandMessage(target)
{
    public const ushort Id = 0xAE;
    public override ushort MessageId => Id;
    public override string DebugName => "msg-174";
    public override string NativeClassName => "firefall::Character::Commands::LogDirectActivityRequest";
    public override int NativeReserveHint => 9; // 0x90FD40 prologue: 9, or 11 if id > 0xFF
    public override void WritePayload(BinaryWriter writer) { }
}

/// <summary>
/// Message 0xAF (175), RTTI <c>firefall::Character::Commands::LogActivityPush</c>
/// (vtable file 0x1865198). Lua usage string: "Usage: ActivityDirector.LogActivityPush(boredGauge);".
/// Constructor 0x536610 stores the argument at +0x28; encoder 0x90FE20 packs it as 4 bytes (0x344440).
/// </summary>
public sealed class LogActivityPush(ulong target, uint boredGauge) : CharacterCommandMessage(target)
{
    public const ushort Id = 0xAF;

    /// <summary>
    /// +0x28. Named from the Lua usage string only. Signedness/range/units are UNKNOWN; the
    /// Lua binding 0xA2D770 takes the low 32 bits of an integer-tagged (0x13) argument, else
    /// uses converter 0xCD05E0, and falls back to 0 when conversion fails.
    /// </summary>
    public uint BoredGauge { get; } = boredGauge;

    public override ushort MessageId => Id;
    public override string DebugName => "msg-175";
    public override string NativeClassName => "firefall::Character::Commands::LogActivityPush";
    public override int NativeReserveHint => 13; // 13, or 15 if id > 0xFF
    public override void WritePayload(BinaryWriter writer) => writer.Write(BoredGauge);
}

/// <summary>Element of UpdateShoppingList set A (+0x28), copied from the source map's two DWORD value fields. Semantic names are unknown.</summary>
/// <param name="First">Source node +0x10, written first as u32 by 0x5BA110 (0x5BA167).</param>
/// <param name="Second">Source node +0x14, written second as u32 (0x5BA173); it is the map key and set ordering/uniqueness key.</param>
public readonly record struct ShoppingEntryA(uint First, uint Second);

/// <summary>Element of UpdateShoppingList set B (+0x30), aggregated from each source map entry's embedded inner set. Semantic names are unknown.</summary>
/// <param name="Value">Inner-set node +0x10, written as u32 by 0x5BA240 (0x5BA297).</param>
/// <param name="Tag">Inner-set node +0x14, written as 1 byte (0x5BA2A3); the set is ordered by (Value, Tag).</param>
public readonly record struct ShoppingEntryB(uint Value, byte Tag);

/// <summary>
/// Message 0xC2 (194), RTTI <c>firefall::Character::Commands::UpdateShoppingList</c>
/// (vtable file 0x1865068). Built inline by 0x9095B0 and encoded by 0x910040.
/// Payload = an eight-byte context key followed by two chunked ordered sets.
/// The low byte of the context key is forced to 4; its exact identity is unknown.
/// </summary>
public sealed class UpdateShoppingList : CharacterCommandMessage
{
    public const ushort Id = 0xC2;

    public UpdateShoppingList(ulong target, IEnumerable<ShoppingEntryA> setA, IEnumerable<ShoppingEntryB> setB)
        : base(target)
    {
        // Native std::set-like containers: unique + in-order iteration. Re-create both properties.
        SetA = setA.GroupBy(e => e.Second).Select(g => g.First()).OrderBy(e => e.Second).ToArray();
        SetB = setB.Distinct().OrderBy(e => e.Value).ThenBy(e => e.Tag).ToArray();
    }

    /// <summary>+0x28 (head,size). Unique by and sorted by <see cref="ShoppingEntryA.Second"/>; first-inserted wins on duplicates.</summary>
    public IReadOnlyList<ShoppingEntryA> SetA { get; }

    /// <summary>+0x30 (head,size). Unique and sorted by (Value, Tag).</summary>
    public IReadOnlyList<ShoppingEntryB> SetB { get; }

    public override ushort MessageId => Id;
    public override string DebugName => "msg-194";
    public override string NativeClassName => "firefall::Character::Commands::UpdateShoppingList";
    public override int NativeReserveHint => 9; // same as 174; sets grow the buffer on demand

    public override void WritePayload(BinaryWriter writer)
    {
        CharacterCommandWire.WriteChunkedSequence(writer, SetA, static (w, e) => { w.Write(e.First); w.Write(e.Second); });
        CharacterCommandWire.WriteChunkedSequence(writer, SetB, static (w, e) => { w.Write(e.Value); w.Write(e.Tag); });
    }
}
