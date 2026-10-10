namespace Firefall.Characters;

/// <summary>
/// A 24-byte positional element in SendMailToPlayer's copied vector.
/// SharedTextReference is an opaque process-local native pointer.
/// </summary>
public readonly record struct SendMailToPlayerItem(
    uint Value0,
    uint SharedTextReference,
    uint Value2,
    uint Value3,
    uint Value4,
    uint Value5);

/// <summary>
/// Native Character::Commands::SendMailToPlayer, metadata ID 84.
/// This is an object snapshot; it does not encode the message on the wire.
/// </summary>
public sealed class SendMailToPlayer
{
    public const ushort MessageId = 84;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int Value2COffset = 0x2C;
    public const int SharedText30Offset = 0x30;
    public const int SharedText34Offset = 0x34;
    public const int SharedText38Offset = 0x38;
    public const int ItemsOffset = 0x3C;
    public const int ItemSize = 24;

    public SendMailToPlayer(
        ulong target,
        uint value28,
        uint value2C,
        uint sharedText30,
        uint sharedText34,
        uint sharedText38,
        IEnumerable<SendMailToPlayerItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Target = target;
        Value28 = value28;
        Value2C = value2C;
        SharedText30 = sharedText30;
        SharedText34 = sharedText34;
        SharedText38 = sharedText38;
        Items = Array.AsReadOnly(items.ToArray());
    }

    public ulong Target { get; }
    public uint Value28 { get; }
    public uint Value2C { get; }
    public uint SharedText30 { get; }
    public uint SharedText34 { get; }
    public uint SharedText38 { get; }
    public IReadOnlyList<SendMailToPlayerItem> Items { get; }
}
