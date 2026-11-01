namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-3 switch-entry 7 layout. Payload meanings remain unknown.
/// </summary>
public static class VehicleFlipPunchNativeLayout
{
    public const uint DispatcherFileOffset = 0x81EF6D;
    public const uint SwitchTableVirtualAddress = 0xC2260C;
    public const uint SwitchTableFileOffset = 0x821A0C;
    public const int SwitchEntryIndex = 7;
    public const int SwitchArgumentAdjustment = -0x53;
    public const int DispatcherValue = 0x5A;
    public const uint CaseFileOffset = 0x81F161;
    public const int CaseLocalDwordValue = 0xD7;
    public const int AllocationSize = 0x40;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xB09820;
    public const uint ConstructorCallSiteFileOffset = 0x81F182;
    public const uint BaseConstructorHelperFileOffset = 0xB477D0;
    public const int BaseConstructorArgument = 0x5A;
    public const uint BodyReaderCallSiteFileOffset = 0x81F198;
    public const uint BodyReaderFileOffset = 0x83A370;
    public const uint DwordReaderFileOffset = 0x3568E0;
    public const uint VtableVirtualAddress = 0x1C72948;
    public const string RttiName = ".?AVFlipPunch@Vehicle@firefall@@";

    public const int FirstDwordFieldOffset = 0x28;
    public const int DwordFieldCount = 6;
}
