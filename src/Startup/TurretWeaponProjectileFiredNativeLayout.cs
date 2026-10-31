namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-5 subcase layout. Field meanings remain unknown.
/// </summary>
public static class TurretWeaponProjectileFiredNativeLayout
{
    public const uint DispatcherFileOffset = 0x81F2EF;
    public const int DispatcherValue = 0x53;
    public const uint CaseFileOffset = 0x81F2F9;
    public const int CaseLocalDwordValue = 0xDC;
    public const int AllocationSize = 0x48;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xB36520;
    public const uint ConstructorCallSiteFileOffset = 0x81F31A;
    public const uint BaseConstructorHelperFileOffset = 0xB47D90;
    public const int BaseConstructorArgument = 0x53;
    public const uint BodyReaderCallSiteFileOffset = 0x81F330;
    public const uint BodyReaderFileOffset = 0x839EB0;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint ScalarReaderFileOffset = 0x3568E0;
    public const uint OpaqueFieldReaderFileOffset = 0x5C0270;
    public const uint VtableVirtualAddress = 0x1C726E8;
    public const string RttiName = ".?AVWeaponProjectileFired@Turret@firefall@@";

    public const int DwordFieldOffset = 0x28;
    public const int FirstRepeatedReaderFieldOffset = 0x2C;
    public const int SecondRepeatedReaderFieldOffset = 0x30;
    public const int ThirdRepeatedReaderFieldOffset = 0x34;
    public const int OpaqueFieldOffset = 0x38;
}
