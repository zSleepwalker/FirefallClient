namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::CameraPoseUpdate, metadata ID 177.
/// The two optional payloads are kept positional; their field meanings are
/// not established. The common constructor leaves both absent.
/// </summary>
public readonly record struct CameraPoseUpdate(
    ulong Target,
    CameraPoseUpdatePair? Pair = null,
    CameraPoseUpdateTriple? Triple = null)
{
    public const ushort MessageId = 177;
    public const int PairPresentOffset = 0x28;
    public const int PairOffset = 0x30;
    public const int TriplePresentOffset = 0x38;
    public const int TripleOffset = 0x3C;
}

public readonly record struct CameraPoseUpdatePair(uint First, uint Second);

public readonly record struct CameraPoseUpdateTriple(uint First, uint Second, uint Third);
