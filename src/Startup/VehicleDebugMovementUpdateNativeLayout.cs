namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-3 switch-entry 8 layout. Payload meanings remain unknown.
/// </summary>
public static class VehicleDebugMovementUpdateNativeLayout
{
    public const uint DispatcherFileOffset = 0x81EF6D;
    public const uint SwitchTableVirtualAddress = 0xC2260C;
    public const uint SwitchTableFileOffset = 0x821A0C;
    public const int SwitchEntryIndex = 8;
    public const int SwitchArgumentAdjustment = -0x53;
    public const int DispatcherValue = 0x5B;
    public const uint CaseFileOffset = 0x81F1B2;
    public const int CaseLocalDwordValue = 0xD8;
    public const int AllocationSize = 0xA0;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xB098A0;
    public const uint ConstructorCallSiteFileOffset = 0x81F1D6;
    public const uint BaseConstructorHelperFileOffset = 0xB47840;
    public const int BaseConstructorArgument = 0x5B;
    public const uint BodyReaderCallSiteFileOffset = 0x81F1EC;
    public const uint BodyReaderFileOffset = 0x83A230;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint NestedBodyReaderFileOffset = 0x608370;
    public const uint DwordReaderFileOffset = 0x3568E0;
    public const uint FourValueReaderFileOffset = 0x384B40;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint FinalDwordReaderFileOffset = 0x3370A0;
    public const uint VtableVirtualAddress = 0x1C728F8;
    public const string RttiName = ".?AVDebugMovementUpdate@Vehicle@firefall@@";

    public const int FirstByteFieldOffset = 0x28;
    public const int NestedReadFieldOffset = 0x2C;
    public const int FirstNestedDwordOffset = 0x2C;
    public const int NestedDwordCount = 3;
    public const int FourValueInputOffset = 0x38;
    public const int FourValueInputSize = 0x10;
    public const int FourValueCopyOffset = 0x78;
    public const int ConstructorSeededWordOffset = 0x88;
    public const int ConstructorSeededWordValue = 0x0101;
    public const int NestedReaderWordOffset = 0x88;
    public const int FinalDwordRunOffset = 0x8C;
    public const int FinalDwordRunCount = 3;
    public const int TrailingWordFieldOffset = 0x98;
    public const int TrailingDwordFieldOffset = 0x9C;
}
