namespace Firefall.Reconstruction.Startup;

public readonly record struct StartupPayloadDispatchSnapshot(
    int ContainerOffset,
    uint LoopFileOffset,
    uint DispatchFileOffset,
    int? PayloadNodeOffset,
    int VirtualSlotOffset,
    int ExplicitArgumentValue);

public static class StartupObjectLifecycleNativeLayout
{
    public const uint ScriptingObjectCleanupFileOffset = 0xC33060;
    public const uint VmCloseCallFileOffset = 0xC33291;
    public const uint VmCloseMethodFileOffset = 0xCD5510;

    public static IReadOnlyList<int> PayloadPassContainerOffsets { get; } =
        Array.AsReadOnly(new[] { 0xF0, 0xE8, 0xE0, 0xD8, 0x100, 0x110, 0x118, 0x140 });

    public static IReadOnlyList<int> PayloadResetContainerOffsets { get; } =
        Array.AsReadOnly(new[] { 0xF0, 0xE8, 0xE0, 0xD8, 0x100, 0x110, 0x118, 0x140 });

    public static IReadOnlyList<StartupPayloadDispatchSnapshot> ObservedPayloadDispatches { get; } =
        Array.AsReadOnly(new[]
        {
            new StartupPayloadDispatchSnapshot(0xE8, 0xC33326, 0xC3335D, 0x14, 0, 1),
            new StartupPayloadDispatchSnapshot(0x118, 0xC336DA, 0xC33706, 0x14, 0, 1)
        });

    public const uint CallbackWalkerFileOffset = 0xC376A0;
    public const int CallbackWalkerVirtualSlotOffset = 0x14;

    public const uint Container118ResetMethodFileOffset = 0xC0BE90;
    public const uint Container118ResetBlockStartFileOffset = 0xC33973;
    public const uint Container118ResetBlockEndFileOffset = 0xC33993;

    public const int PostPassContainer120Offset = 0x120;
    public const uint PostPassContainer120HelperFileOffset = 0xC3FC10;
    public const uint PostPassContainer120CallFileOffset = 0xC33C2B;
    public const uint PostPassContainer118ReleaseMethodFileOffset = 0xC0BD90;
    public const uint StoredObjectReleaseBlockStartFileOffset = 0xC33C3A;
    public const uint StoredObjectReleaseBlockEndFileOffset = 0xC33C58;

    public const uint Manager6B0DestructorFileOffset = 0xBF8AC0;
    public const uint ManagerKeyCleanupMethodFileOffset = 0xC0BD90;
    public const uint ManagerNodeReleaseMethodFileOffset = 0xC0EB00;
    public const int ManagerNodeKeyOffset = 0x10;
    public static IReadOnlyList<uint> ManagerDestructorPathFileOffsets { get; } =
        Array.AsReadOnly(new[] { Manager6B0DestructorFileOffset, ManagerKeyCleanupMethodFileOffset, ManagerNodeReleaseMethodFileOffset });

    public const uint InsertFailurePathHelperFileOffset = 0xC3C9B0;
    public const uint InsertFailurePathAnchorFileOffset = 0xC64420;
    public const int InsertFailurePayloadVirtualSlotOffset = 0;
    public const int InsertFailurePayloadExplicitArgumentValue = 1;
    public static IReadOnlyList<uint> InsertFailurePathFileOffsets { get; } =
        Array.AsReadOnly(new[] { InsertFailurePathHelperFileOffset, InsertFailurePathAnchorFileOffset });
}
