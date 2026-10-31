namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-3 switch-entry layout. Field meanings remain unknown.
/// </summary>
public static class VehicleAbilityActivatedNativeLayout
{
    public const uint DispatcherFileOffset = 0x81EF6D;
    public const uint SwitchTableVirtualAddress = 0xC2260C;
    public const uint SwitchTableFileOffset = 0x821A0C;
    public const int SwitchEntryIndex = 0;
    public const int SwitchArgumentAdjustment = 0xAD;
    public const uint CaseFileOffset = 0x81EF83;
    public const int CaseLocalDwordValue = 0xD0;
    public const int AllocationSize = 0x30;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xB093C0;
    public const uint ConstructorCallSiteFileOffset = 0x81EFA4;
    public const uint BaseConstructorHelperFileOffset = 0xB474B0;
    public const int BaseConstructorArgument = 0x53;
    public const uint BodyReaderCallSiteFileOffset = 0x81EFBA;
    public const uint BodyReaderFileOffset = 0x83A840;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint VtableVirtualAddress = 0x1C72998;
    public const string RttiName = ".?AVAbilityActivated@Vehicle@firefall@@";

    public const int FirstDwordFieldOffset = 0x28;
    public const int SecondDwordFieldOffset = 0x2C;
}
