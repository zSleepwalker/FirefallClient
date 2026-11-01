namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-3 switch-entry 1 layout. RTTI identity only; payload meanings remain unknown.
/// </summary>
public static class VehicleAbilityFailedNativeLayout
{
    public const uint DispatcherFileOffset = 0x81EF6D;
    public const uint SwitchTableVirtualAddress = 0xC2260C;
    public const uint SwitchTableFileOffset = 0x821A0C;
    public const int SwitchEntryIndex = 1;
    public const int SwitchArgumentAdjustment = -0x53;
    public const int DispatcherValue = 0x54;
    public const uint CaseFileOffset = 0x81EFD4;
    public const int CaseLocalDwordValue = 0xD1;
    public const int AllocationSize = 0x30;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xB09440;
    public const uint ConstructorCallSiteFileOffset = 0x81EFF5;
    public const uint BaseConstructorHelperFileOffset = 0xB47520;
    public const int BaseConstructorArgument = 0x54;
    public const uint BodyReaderCallSiteFileOffset = 0x81F00B;
    public const uint BodyReaderFileOffset = 0x83A7A0;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint VtableVirtualAddress = 0x1C72978;
    public const string RttiName = ".?AVAbilityFailed@Vehicle@firefall@@";

    public const int FirstDwordFieldOffset = 0x28;
    public const int SecondDwordFieldOffset = 0x2C;
}
