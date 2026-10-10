namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::NotifyDialogScriptComplete, metadata ID 143.
/// Both subclass DWORDs are retained positionally.
/// </summary>
public readonly record struct NotifyDialogScriptComplete(
    ulong Target,
    uint Value1,
    uint Value2)
{
    public const ushort MessageId = 143;
    public const int Value1Offset = 0x28;
    public const int Value2Offset = 0x2C;
}
