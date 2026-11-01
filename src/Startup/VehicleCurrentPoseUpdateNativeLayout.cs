namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-3 switch-entry 3 layout. Flag and payload meanings remain unknown.
/// </summary>
public static class VehicleCurrentPoseUpdateNativeLayout
{
    public const uint DispatcherFileOffset = 0x81EF6D;
    public const uint SwitchTableVirtualAddress = 0xC2260C;
    public const uint SwitchTableFileOffset = 0x821A0C;
    public const int SwitchEntryIndex = 3;
    public const int SwitchArgumentAdjustment = -0x53;
    public const int DispatcherValue = 0x56;
    public const uint CaseFileOffset = 0x81F056;
    public const int CaseLocalDwordValue = 0xD3;
    public const int AllocationSize = 0x58;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xB095B0;
    public const uint ConstructorCallSiteFileOffset = 0x81F077;
    public const uint BaseConstructorHelperFileOffset = 0xB47600;
    public const int BaseConstructorArgument = 0x56;
    public const uint BodyReaderCallSiteFileOffset = 0x81F08E;
    public const uint BodyReaderFileOffset = 0x83A530;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint DwordReaderFileOffset = 0x3568E0;
    public const uint ThreeByteReaderFileOffset = 0x336B50;
    public const uint FinalWordReaderFileOffset = 0x6097B0;
    public const uint AlternateByteReaderFileOffset = 0x356CF0;
    public const uint VtableVirtualAddress = 0x1C72958;
    public const string RttiName = ".?AVCurrentPoseUpdate@Vehicle@firefall@@";

    public const int FlagFieldOffset = 0x28;
    public const int FirstConditionalByteFieldOffset = 0x29;
    public const int FirstConditionalWordFieldOffset = 0x2A;
    public const int FollowingByteFieldOffset = 0x2C;
    public const int SecondConditionalWordFieldOffset = 0x2E;
    public const int FirstThreeByteFieldOffset = 0x30;
    public const int FirstDwordRunOffset = 0x34;
    public const int FirstDwordRunCount = 3;
    public const int FirstByteUnionOffset = 0x40;
    public const int FirstWordRunOffset = 0x44;
    public const int FirstWordRunCount = 4;
    public const int SecondThreeByteFieldOffset = 0x4C;
    public const int FinalWordRunOffset = 0x50;
    public const int FinalWordRunCount = 3;
    public const byte FirstByteChoiceFlag = 0x01;
    public const byte SecondWordChoiceFlag = 0x02;
    public const byte ThreeByteChoiceFlags = 0x0C;
    public const byte FirstUnionChoiceFlags = 0x30;
    public const byte FinalThreeByteChoiceFlags = 0xC0;
}
