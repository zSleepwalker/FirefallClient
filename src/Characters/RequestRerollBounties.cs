namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::RequestRerollBounties, metadata ID 164.
/// The subclass owns a copied vector of DWORD values.
/// </summary>
public sealed class RequestRerollBounties
{
    public const ushort MessageId = 164;
    public const int ValuesOffset = 0x28;
    public const int NativeVectorHeaderSize = 0x0C;

    public RequestRerollBounties(ulong target, IEnumerable<uint> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        Target = target;
        Values = Array.AsReadOnly(values.ToArray());
    }

    public ulong Target { get; }
    public IReadOnlyList<uint> Values { get; }
}
