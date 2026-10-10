namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::DismissTutorialId, metadata ID 221.
/// The tutorial ID is a positional DWORD; observed callers constrain it to 1..14.
/// </summary>
public readonly record struct DismissTutorialId(ulong Target, uint TutorialId)
{
    public const ushort MessageId = 221;
    public const int TargetOffset = 0x18;
    public const int TutorialIdOffset = 0x28;
    public const uint MinimumTutorialId = 1;
    public const uint MaximumTutorialId = 14;
}
