namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-2 subcase layout. RTTI identifies the type; field meanings remain unknown.
/// </summary>
public static class AreaVisualDataLootObjectCollectedNativeLayout
{
    public const uint DispatcherFileOffset = 0x81EE63;
    public const int DispatcherValue = 0x53;
    public const uint CaseFileOffset = 0x81EF1C;
    public const int CaseLocalDwordValue = 0xCD;
    public const int AllocationSize = 0x40;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xADBAA0;
    public const uint ConstructorCallSiteFileOffset = 0x81EF3D;
    public const uint BaseConstructorHelperFileOffset = 0xB07660;
    public const int BaseConstructorArgument = 0x53;
    public const uint BodyReaderCallSiteFileOffset = 0x81EF53;
    public const uint BodyReaderFileOffset = 0x83AD90;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint VtableVirtualAddress = 0x1C723C0;
    public const string RttiName = ".?AVLootObjectCollected@AreaVisualData@firefall@@";

    public const int DwordFieldOffset = 0x28;
    public const int FirstQwordFieldOffset = 0x30;
    public const int SecondQwordFieldOffset = 0x38;
}
