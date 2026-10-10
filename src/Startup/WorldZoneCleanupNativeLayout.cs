namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Proven offsets and call boundaries in tfWorld's zone cleanup. Resource
/// identities, ownership internals and indirect mode transitions remain unresolved.
/// </summary>
public static class TfWorldZoneCleanupNativeLayout
{
    public const uint MethodFileOffset = 0x64E210;
    public const uint ConnectionCleanupMethodFileOffset = 0x494A40;
    public const int ConnectionPointerOffset = 0x10;
    public const int RequiredConnectionState = 5;
    public const uint RuntimeMessageId = 97;
    public const int RuntimeReceiverVirtualOffset = 0xAC;
    public const int Byte2F1Offset = 0x2F1;
    public const int ObserverPointerOffset = 0x28E8;
    public const int ObserverFirstComparedWordOffset = 0x120;
    public const int ObserverSecondComparedWordOffset = 0x124;
    public const uint MatchingObserverCallFileOffset = 0x6E9C50;
    public const int ModeObjectGetterVirtualOffset = 0x24;
    public const int ModeObjectMember74Offset = 0x74;
    public const int TemporarilyClearedWordOffset = 0x0C;
    public const uint TemporaryZeroCallFileOffset = 0x406310;
    public const int ModeObjectMember78Offset = 0x78;
    public const int Member78RequiredPointerOffset = 0x10;
    public const uint Member78CleanupMethodFileOffset = 0x976430;
    public const int ModeObjectMember50Offset = 0x50;
    public const int Member50ClearedByteOffset = 0x1AB;
    public const int Member50SetByteOffset = 0x1A9;
    public const int LocalClientMember4Offset = 4;
    public const int LocalClientReleasedPointerOffset = 0x5590;
    public const uint ReleasedObjectCleanupMethodFileOffset = 0x861020;

    public static bool SelectObserverCall(uint word08, uint word0C, uint word120, uint word124) =>
        word08 == word120 && word0C == word124;
}
