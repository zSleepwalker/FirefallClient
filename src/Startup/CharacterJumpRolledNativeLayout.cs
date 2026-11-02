namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x66 raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterJumpRolledNativeLayout
{
    public const int InnerSelector = 0x66;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D247;
    public const uint CaseLocalDwordValue = 0x6E;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x525B70;
    public const uint BaseConstructorHelperFileOffset = 0x5A0C90;
    public const int BaseConstructorArgument = 0x66;
    public const uint AllocationFailureContinuationFileOffset = 0x81D21D;
    public const uint BodyReaderContinuationFileOffset = 0x81D21F;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint VtableVirtualAddress = 0x1C67148;
    public const string RttiName = ".?AVJumpRolled@Character@firefall@@";

    public const int WordFieldOffset = 0x28;
    public const int NominalInputByteCount = 2;
}

public static class CharacterJumpRolledConsumerNativeLayout
{
    public const int MessageDiscriminator = CharacterJumpRolledNativeLayout.InnerSelector;
    public const uint ProtocolHandlerDispatchTableVirtualAddress = 0xC1D6DC;
    public const int ProtocolHandlerDispatchTableSelectorBias = 0x20;
    public const int ProtocolHandlerVtableSlotOffset = 0x11C;
    public const uint ProtocolHandlerVtableTargetVirtualAddress = 0xECD9B0;
    public const uint ProtocolHandlerVtableTargetFileOffset = 0xACCDB0;
    public const uint ConsumerFunctionFileOffset = 0xACCDB0;

    public const int TransportContextFieldOffset = 0x10;
    public const int MessageKeyFieldOffset = CharacterJumpRolledNativeLayout.WordFieldOffset;
    public const int MessageKeyFieldByteCount = 8;
    public const uint KeyedDrainRoutineFileOffset = 0x977F20;
    public const uint KeyLookupRoutineFileOffset = 0x979C80;
    public const uint DrainItemRoutineFileOffset = 0x977630;
    public const uint DrainFinalizeRoutineFileOffset = 0x975020;
    public const uint DrainReleaseRoutineFileOffset = 0x12C8D60;
    public const int MissingTransportDiagnosticCode = 91;
}
