namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-4 value 0x55 layout. RTTI identity only; payload meanings remain unknown.
/// </summary>
public static class DeployablePublicCombatLogNativeLayout
{
    public const uint DispatcherFileOffset = 0x81F206;
    public const int DispatcherValue = 0x55;
    public const uint CaseFileOffset = 0x81F21C;
    public const int CaseLocalDwordValue = 0xDB;
    public const int AllocationSize = 0x40;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xB27FE0;
    public const uint ConstructorCallSiteFileOffset = 0x81F241;
    public const uint BaseConstructorHelperFileOffset = 0xB47D20;
    public const int BaseConstructorArgument = 0x55;
    public const uint BodyReaderCallSiteFileOffset = 0x81CF67;
    public const uint BodyReaderFileOffset = 0x5F3350;
    public const uint WordLengthPrefixedBlobReaderFileOffset = 0x12EE170;
    public const uint EmbeddedBlobVtableVirtualAddress = 0x1C80CC4;
    public const uint VtableVirtualAddress = 0x1C72754;
    public const string RttiName = ".?AVPublicCombatLog@Deployable@firefall@@";

    public const uint RequiredBodyDword = 1;
    public const int RequiredBodyDwordWidth = 4;
    public const int BlobLengthPrefixWidth = 2;
    public const int BlobFieldOffset = 0x2C;
    public const int ConstructorZeroedByteOffset = 0x28;
    public const int ConstructorZeroedFirstBlobDwordOffset = 0x30;
    public const int ConstructorZeroedSecondBlobDwordOffset = 0x34;
    public const int ConstructorZeroedThirdBlobDwordOffset = 0x38;
}
