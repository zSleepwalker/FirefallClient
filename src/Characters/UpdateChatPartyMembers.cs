namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::UpdateChatPartyMembers, metadata ID 185.
/// The vector entries are opaque 16-byte records; the DWORDs remain positional.
/// </summary>
public sealed class UpdateChatPartyMembers
{
    public const ushort MessageId = 185;
    public const int TargetOffset = 0x18;
    public const int MembersOffset = 0x28;
    public const int VectorElementSize = 16;
    public const int UnwrittenGapOffset = 0x34;
    public const int UnwrittenGapSize = 4;
    public const int Value38Offset = 0x38;
    public const int Value3COffset = 0x3C;

    public UpdateChatPartyMembers(
        ulong target,
        ReadOnlyMemory<byte> members,
        uint value38,
        uint value3C)
    {
        if (members.Length % VectorElementSize != 0)
            throw new ArgumentException(
                $"Vector length must be a multiple of {VectorElementSize} bytes.",
                nameof(members));

        Target = target;
        Members = members.ToArray();
        Value38 = value38;
        Value3C = value3C;
    }

    public ulong Target { get; }
    public ReadOnlyMemory<byte> Members { get; }
    public uint Value38 { get; }
    public uint Value3C { get; }
}
