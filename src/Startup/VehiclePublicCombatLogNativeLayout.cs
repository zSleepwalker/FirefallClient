namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-3 switch-entry 2 layout. Payload meanings remain unknown.
/// </summary>
public static class VehiclePublicCombatLogNativeLayout
{
    public const uint DispatcherFileOffset = 0x81EF6D;
    public const uint SwitchTableVirtualAddress = 0xC2260C;
    public const uint SwitchTableFileOffset = 0x821A0C;
    public const int SwitchEntryIndex = 2;
    public const int SwitchArgumentAdjustment = -0x53;
    public const int DispatcherValue = 0x55;
    public const uint CaseFileOffset = 0x81F025;
    public const int CaseLocalDwordValue = 0xD2;
    public const int AllocationSize = 0x40;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xB094C0;
    public const uint ConstructorCallSiteFileOffset = 0x81F04A;
    public const uint BaseConstructorHelperFileOffset = 0xB47590;
    public const int BaseConstructorArgument = 0x55;
    public const uint BodyReaderCallSiteFileOffset = 0x81CF67;
    public const uint BodyReaderFileOffset = 0x5F3350;
    public const uint LengthPrefixedBlobReaderFileOffset = 0x12EE170;
    public const uint VtableVirtualAddress = 0x1C72988;
    public const string RttiName = ".?AVPublicCombatLog@Vehicle@firefall@@";

    public const int DwordGateFieldOffset = 0x28;
    public const int LengthPrefixedBlobFieldOffset = 0x2C;
    public const int LengthPrefixWidth = 2;
    public const uint ConstructorSeededDwordValue = 0x1C80CC4;
}
