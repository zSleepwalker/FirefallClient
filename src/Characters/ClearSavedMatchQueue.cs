namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ClearSavedMatchQueue, metadata ID 91.
/// The two initialized words are preserved positionally; no wire encoding is inferred.
/// </summary>
public sealed class ClearSavedMatchQueue
{
    public const ushort MessageId = 91;
    public const int Value18Offset = 0x18;
    public const int Value1COffset = 0x1C;
    public const int Value20Offset = 0x20;
    public const int Value24Offset = 0x24;
    public const int InitializedPrefixSize = 0x28;

    public ClearSavedMatchQueue(uint value18, uint value1C)
    {
        Value18 = value18;
        Value1C = value1C;
    }

    public uint Value18 { get; }
    public uint Value1C { get; }
    public uint Value20 => 0;
    public uint Value24 => 0;
}
