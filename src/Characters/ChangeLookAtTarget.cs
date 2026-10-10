namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ChangeLookAtTarget, metadata ID 133.
/// The optional six-DWORD payload is retained positionally.
/// </summary>
public sealed class ChangeLookAtTarget
{
    public const ushort MessageId = 133;
    public const int OptionalFlagOffset = 0x28;
    public const int OptionalValuesOffset = 0x30;

    public ChangeLookAtTarget(ulong target, IEnumerable<uint> optionalValues)
    {
        ArgumentNullException.ThrowIfNull(optionalValues);
        uint[] copiedValues = optionalValues.ToArray();
        if (copiedValues.Length is not 0 and not 6)
            throw new ArgumentException("Expected either no optional values or exactly six.", nameof(optionalValues));

        Target = target;
        OptionalValues = Array.AsReadOnly(copiedValues);
    }

    public ulong Target { get; }
    public bool HasOptionalValues => OptionalValues.Count != 0;
    public IReadOnlyList<uint> OptionalValues { get; }
}
