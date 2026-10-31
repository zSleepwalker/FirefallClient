namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-4 subcase layout. Field meanings remain unknown.
/// </summary>
public static class DeployableTookHitNativeLayout
{
    public const uint DispatcherFileOffset = 0x81F206;
    public const int DispatcherValue = 0x53;
    public const uint CaseFileOffset = 0x81F29E;
    public const int CaseLocalDwordValue = 0xD9;
    public const int AllocationSize = 0x60;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xB27E80;
    public const uint ConstructorCallSiteFileOffset = 0x81F2BF;
    public const uint BaseConstructorHelperFileOffset = 0xB47C40;
    public const int BaseConstructorArgument = 0x53;
    public const uint BodyReaderCallSiteFileOffset = 0x81F2D5;
    public const uint BodyReaderFileOffset = 0x83A120;
    public const uint ConditionalRecordReaderFileOffset = 0x5F0CF0;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint VtableVirtualAddress = 0x1C727A8;
    public const string RttiName = ".?AVTookHit@Deployable@firefall@@";

    public const int ConditionalRecordOffset = 0x28;
    public const int FirstByteFieldOffset = 0x50;
    public const int SecondByteFieldOffset = 0x51;
    public const int WordFieldOffset = 0x58;
    public const int FinalByteFieldOffset = 0x5A;
}
