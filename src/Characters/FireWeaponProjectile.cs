namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::FireWeaponProjectile, metadata ID 121.
/// Positional field meanings remain unknown.
/// </summary>
public sealed class FireWeaponProjectile
{
    public const ushort MessageId = 121;
    public const int Value28Offset = 0x28;
    public const int ValuesOffset = 0x2C;
    public const int OptionalFlagOffset = 0x38;
    public const int OptionalValuesOffset = 0x3C;

    public FireWeaponProjectile(
        ulong target,
        uint value28,
        uint value2C,
        uint value30,
        uint value34,
        IEnumerable<uint> optionalValues)
    {
        ArgumentNullException.ThrowIfNull(optionalValues);
        uint[] copiedValues = optionalValues.ToArray();
        if (copiedValues.Length is not 0 and not 3)
            throw new ArgumentException("Expected either no optional values or exactly three.", nameof(optionalValues));

        Target = target;
        Value28 = value28;
        Value2C = value2C;
        Value30 = value30;
        Value34 = value34;
        OptionalValues = Array.AsReadOnly(copiedValues);
    }

    public ulong Target { get; }
    public uint Value28 { get; }
    public uint Value2C { get; }
    public uint Value30 { get; }
    public uint Value34 { get; }
    public bool HasOptionalValues => OptionalValues.Count != 0;
    public IReadOnlyList<uint> OptionalValues { get; }
}
