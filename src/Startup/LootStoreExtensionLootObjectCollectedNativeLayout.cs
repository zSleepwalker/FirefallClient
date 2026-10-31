namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-6 subcase layout. RTTI identifies the type; field meanings remain unknown.
/// </summary>
public static class LootStoreExtensionLootObjectCollectedNativeLayout
{
    public const uint DispatcherFileOffset = 0x81F34A;
    public const int DispatcherValue = 0x53;
    public const uint CaseFileOffset = 0x81F354;
    public const int CaseLocalDwordValue = 0xDD;
    public const int AllocationSize = 0x40;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xB43A70;
    public const uint ConstructorCallSiteFileOffset = 0x81F375;
    public const uint BaseConstructorHelperFileOffset = 0xB48040;
    public const int BaseConstructorArgument = 0x53;
    public const uint BodyReaderCallSiteFileOffset = 0x81F38B;
    public const uint BodyReaderFileOffset = 0x839DB0;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint VtableVirtualAddress = 0x1C72608;
    public const string RttiName = ".?AVLootObjectCollected@LootStoreExtension@firefall@@";

    public const int DwordFieldOffset = 0x28;
    public const int FirstQwordFieldOffset = 0x30;
    public const int SecondQwordFieldOffset = 0x38;
}
