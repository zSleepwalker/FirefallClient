namespace Firefall.Reconstruction.Startup;

public enum ClientAppModeId
{
    Login = 1,
    World = 2,
    LookingGlass = 3
}

public readonly record struct AppModeSnapshot(
    ClientAppModeId Id,
    string NativeTypeName,
    int NativeAllocationSize,
    uint ConstructorFileOffset,
    uint VtableVirtualAddress,
    uint TypeDescriptorVirtualAddress,
    uint TypeDescriptorFileOffset);

public readonly record struct AppModeInterfaceSnapshot(
    ClientAppModeId AppModeId,
    string NativeBaseTypeName,
    int NativeSubobjectOffset,
    uint VtableVirtualAddress,
    uint VtableFileOffset,
    uint TypeDescriptorVirtualAddress,
    uint TypeDescriptorFileOffset,
    uint Method0FileOffset,
    uint Method1FileOffset,
    uint Method2FileOffset,
    uint Method3FileOffset);

public readonly record struct PendingAppModeRequestSnapshot(
    int ModeId,
    string NativeTypeName,
    uint ConstructorFileOffset);

public readonly record struct LookingGlassQueueCallSiteSnapshot(
    uint FileOffset,
    string ObservedUse);

public static class AppModeCatalog
{
    private static readonly AppModeSnapshot[] Modes =
    [
        new(ClientAppModeId.Login, "tfLogin", 0xA58, 0x6365B0, 0x1C676C0, 0x1E128C4, 0x1A10CC4),
        new(ClientAppModeId.World, "tfWorld", 0x2904, 0x64AE40, 0x1C677BC, 0x1E12A9C, 0x1A10E9C),
        new(ClientAppModeId.LookingGlass, "tfLookingGlass", 0x1368, 0x65A190, 0x1C6781C, 0x1E12C48, 0x1A11048)
    ];

    private static readonly AppModeInterfaceSnapshot[] Interfaces =
    [
        new(
            ClientAppModeId.LookingGlass,
            "nl::ReplayReader::GameInterface",
            4,
            0x1C6786C,
            0x186646C,
            0x1E12AB4,
            0x1A10EB4,
            0x661473,
            0x65E870,
            0x65E940,
            0x65EB80)
    ];

    private static readonly PendingAppModeRequestSnapshot[] PendingRequests =
    [
        new(1, "tfLogin", 0x6365B0),
        new(2, "tfWorld", 0x64AE40),
        new(3, "tfLookingGlass", 0x65A190)
    ];

    private static readonly LookingGlassQueueCallSiteSnapshot[] LookingGlassQueueCallSites =
    [
        new(0x306050, "Packages a text request and two DWORD values"),
        new(0x3D60A0, "Builds replay-reader state from decoded text fields"),
        new(0x6385C0, "Conditional application update path"),
        new(0x65D400, "Replay-reader request path"),
        new(0x6AE3E0, "Selects a text value for replay-reader state"),
        new(0x6AF120, "Uses a shared-text value for replay-reader state")
    ];

    public const uint PendingModeQueueInsertMethodFileOffset = 0x30DEB0;
    public const uint EnqueueModeIdWithZeroPayloadMethodFileOffset = 0x324080;
    public const uint ApplicationModeInitializationHelperFileOffset = 0x307224;
    public const uint DefaultInitialModeEnqueueCallFileOffset = 0x30797C;
    public const uint InitialPendingModeDrainCallFileOffset = 0x30798E;
    public const int DefaultInitialModeId = 1;
    public const uint LoginFallbackRequestHelperFileOffset = 0x30D790;
    public const uint QueueLookingGlassRequestMethodFileOffset = 0x30D590;
    public const uint TryQueueLookingGlassRequestMethodFileOffset = 0x306050;
    public const uint ConditionalLoginModeQueueMethodFileOffset = 0x6385C0;
    public const int PendingModeQueueStorageOffset = 0xB84;
    public const int PendingModeQueueHeadOffset = 0xB90;
    public const int PendingModeQueueCountOffset = 0xB94;
    public const int PendingModeRequestModeIdOffset = 0;
    public const int PendingModeRequestPayloadOffset = 4;

    public static IReadOnlyList<AppModeSnapshot> KnownModes => Modes;

    public static IReadOnlyList<AppModeInterfaceSnapshot> KnownInterfaces => Interfaces;

    public static IReadOnlyList<PendingAppModeRequestSnapshot> KnownPendingRequests => PendingRequests;

    public static IReadOnlyList<LookingGlassQueueCallSiteSnapshot> KnownLookingGlassQueueCallSites =>
        LookingGlassQueueCallSites;

    public static AppModeSnapshot? Find(int modeId)
    {
        foreach (var mode in Modes)
        {
            if ((int)mode.Id == modeId)
                return mode;
        }

        return null;
    }
}

public static class LookingGlassNativeLayout
{
    public const int ReplayReaderPointerOffset = 0x1334;
    public const uint ConstructorFileOffset = 0x65A190;
    public const int RequestContextArgumentOffset = 0x18;
    public const int RequestContextCopySize = 0x34;
    public const int RequestContextSourceAtApplicationOffset = 0x9F0;
    public const int AdditionalConstructorArgumentApplicationOffset = 0x9E8;
    public const int AdditionalConstructorArgumentDestinationOffset = 0x58;
    public const int RequestContextFirstSharedTextOffset = 0x14;
    public const int RequestContextSecondSharedTextOffset = 0x1C;
}
