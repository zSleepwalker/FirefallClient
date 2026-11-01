namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-4 value 0x54 layout. RTTI identity only; payload meanings remain unknown.
/// </summary>
public static class DeployableAbilityProjectileFiredNativeLayout
{
    public const uint DispatcherFileOffset = 0x81F206;
    public const int DispatcherValue = 0x54;
    public const uint CaseFileOffset = 0x81F24D;
    public const int CaseLocalDwordValue = 0xDA;
    public const int AllocationSize = 0x68;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xB27F40;
    public const uint ConstructorCallSiteFileOffset = 0x81F26E;
    public const uint BaseConstructorHelperFileOffset = 0xB47CB0;
    public const int BaseConstructorArgument = 0x54;
    public const uint BodyReaderCallSiteFileOffset = 0x81F284;
    public const uint BodyReaderFileOffset = 0x839F50;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint AlternateWordReaderFileOffset = 0x34B740;
    public const uint ScalarReaderFileOffset = 0x6097B0;
    public const uint DwordReaderFileOffset = 0x3568E0;
    public const uint AlternateDwordReaderFileOffset = 0x353310;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint RawDwordReaderFileOffset = 0x3370A0;
    public const uint ConditionalQwordReaderFileOffset = 0x5FDA00;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint VtableVirtualAddress = 0x1C72764;
    public const string RttiName = ".?AVAbilityProjectileFired@Deployable@firefall@@";

    public const int FirstWordFieldOffset = 0x28;
    public const int SecondWordFieldOffset = 0x2A;
    public const int ThirdWordFieldOffset = 0x2C;
    public const int FourthWordFieldOffset = 0x2E;
    public const int FifthWordFieldOffset = 0x30;
    public const int SixthWordFieldOffset = 0x32;
    public const int SeventhWordFieldOffset = 0x34;
    public const int EighthWordFieldOffset = 0x36;
    public const int FirstDwordFieldOffset = 0x38;
    public const int SecondDwordFieldOffset = 0x3C;
    public const int ByteFieldOffset = 0x40;
    public const int ThirdDwordFieldOffset = 0x44;
    public const int SecondByteFieldOffset = 0x48;
    public const int FourthDwordFieldOffset = 0x4C;
    public const int FifthDwordFieldOffset = 0x50;
    public const int ConditionalQwordFlagOffset = 0x58;
    public const int ConditionalQwordValueOffset = 0x60;
    public const int ConstructorZeroedByteOffset = 0x58;
}
