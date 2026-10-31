namespace Firefall.Reconstruction.Startup;

public readonly record struct SceneFactoryResourceSnapshot(
    string Name,
    int RootPointerOffset,
    int NameFileOffset);

public readonly record struct SceneFactoryChildAllocationSnapshot(
    int RootPointerOffset,
    int AllocationSize,
    bool Conditional);

/// <summary>
/// Bounded layout and identity observations for the native world scene factory.
/// These values describe captured binary structure, not managed object types.
/// </summary>
public static class SceneFactoryNativeLayout
{
    private static readonly SceneFactoryResourceSnapshot[] SceneResources =
    [
        new("RenderScene", 0x00, 0x179BD28),
        new("WorldMapScene", 0x04, 0x179BE38),
        new("SinvironmentScene", 0x08, 0x179BE48)
    ];

    private static readonly SceneFactoryChildAllocationSnapshot[] ChildAllocations =
    [
        new(0x0C, 0x138, false),
        new(0x14, 0x188, false),
        new(0x18, 0x060, false),
        new(0x1C, 0x044, false),
        new(0x20, 0x044, false),
        new(0x24, 0x060, false),
        new(0x28, 0x03C, false),
        new(0x2C, 0x138, true),
        new(0x30, 0x024, false),
        new(0x34, 0x25C8, false),
        new(0x3C, 0x058, false),
        new(0x4C, 0x390, false),
        new(0x50, 0x1B4, true),
        new(0x54, 0xF28, false),
        new(0x58, 0x080, false),
        new(0x5C, 0x058, false),
        new(0x60, 0x02C, false),
        new(0x64, 0x2F4C, false),
        new(0x68, 0x044, false),
        new(0x6C, 0x008, false),
        new(0x70, 0x0C0, false),
        new(0x74, 0x0D8, false),
        new(0x78, 0x21C, false),
        new(0x84, 0x024, false),
        new(0x88, 0x048, true),
        new(0x90, 0x114, false)
    ];

    public const int FactoryFileOffset = 0x9B73F0;
    public const int RootAllocationSize = 0xA0;
    public const int RootConstructorFileOffset = 0x9B82F0;
    public const int RootDestructorFileOffset = 0x9B8490;
    public const int RootReturnFileOffset = 0x9B82B4;
    public const int SceneProviderGlobalVirtualAddress = 0x1DFE970;
    public const int SceneProviderGetterVtableOffset = 0x3C;
    public const int SceneLookupVtableOffset = 0x30;
    public const int RegistrationHandleOffset = 0x10;
    public const int RegistrationGlobalVirtualAddress = 0x1E001A0;
    public const int RegistrationDescriptorVirtualAddress = 0x1DFC4FC;
    public const int PhysicsDrawInterfacePointerOffset = 0x14;
    public const int PhysicsDrawInterfaceAllocationSize = 0x188;
    public const int PhysicsDrawInterfaceConstructorFileOffset = 0x9B5440;
    public const int PhysicsDrawInterfaceVtableVirtualAddress = 0x1C70968;
    public const int PhysicsDrawInterfaceVtableFileOffset = 0x186F568;
    public const int PhysicsDrawInterfaceCompleteObjectLocatorVirtualAddress = 0x1CC7700;
    public const int PhysicsDrawInterfaceTypeDescriptorVirtualAddress = 0x1E28A34;
    public const int PhysicsDrawInterfaceRttiNameFileOffset = 0x1A26E3C;
    public const string PhysicsDrawInterfaceRttiName = ".?AVtfPhysicsDrawInterface@@";
    public const int TfWorldInitializeFileOffset = 0x64B460;
    public const int TfWorldScenePointerOffset = 0x14;
    public const int OtherFactoryCallerFileOffset = 0x9B2D1A;
    public const int GlobalActiveSceneVirtualAddress = 0x1E00060;
    public const int GlobalActiveScenePointerOffset = 0xF8;

    public const int PhysicsDrawInterfaceBaseVtableVirtualAddress = 0x1C709B8;
    public const int PhysicsDrawInterfaceVtableSlotCount = 14;
    public const int PhysicsDrawInterfaceDestructorFileOffset = 0x9B6930;
    public const int PhysicsDrawInterfaceForwardThunkFileOffset = 0x9B6730;
    public const int PhysicsDrawInterfaceReleaseModelFileOffset = 0x9B68F0;
    public const int PhysicsDrawInterfaceReleaseModelMessageVirtualAddress = 0x1C70834;
    public const string PhysicsDrawInterfaceReleaseModelMessage = "DebugCollision:ReleaseModelObject(model %i)";
    public const int PhysicsDrawInterfaceSceneFieldOffset = 0x44;
    public const int PhysicsDrawInterfaceColorFieldOffset = 0x50;
    public const int PhysicsDrawInterfaceColorSourceVirtualAddress = 0x1DFF438;
    public const int PhysicsDrawInterfaceBoundsFieldOffset = 0x16C;
    public const int GlobalActiveSceneStoreFileOffset = 0x64B528;
    public const int GlobalActiveSceneFailureMessageVirtualAddress = 0x1B635CC;

    private static readonly int[] PhysicsDrawInterfaceVtableSlotFileOffsets =
    [
        0x9B6930, 0x9B5840, 0x9B5930, 0x9B59C0, 0x9B5AA0, 0x9B5D60, 0x9B5E60,
        0x9B62C0, 0x9B6560, 0x9B6730, 0x9B56F0, 0x9B6740, 0x9B6860, 0x9B68F0
    ];

    public static IReadOnlyList<int> PhysicsDrawInterfaceVtableSlots => PhysicsDrawInterfaceVtableSlotFileOffsets;

    public static IReadOnlyList<SceneFactoryResourceSnapshot> KnownSceneResources => SceneResources;
    public static IReadOnlyList<SceneFactoryChildAllocationSnapshot> KnownChildAllocations => ChildAllocations;
}
