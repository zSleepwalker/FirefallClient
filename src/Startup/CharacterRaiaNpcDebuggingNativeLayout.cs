namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x5E shared-text field layout. The text meaning and encoding
/// remain unknown.
/// </summary>
public static class CharacterRaiaNpcDebuggingNativeLayout
{
    public const int InnerSelector = 0x5E;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81CFFC;
    public const uint CaseLocalDwordValue = 0x66;
    public const int AllocationSize = 0x30;
    public const uint ConstructorFileOffset = 0x5255F0;
    public const uint ConstructorHelperFileOffset = 0x5A08F0;
    public const uint SharedTextReaderSetupFileOffset = 0x81FB27;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint VtableVirtualAddress = 0x1C671B8;
    public const string RttiName = ".?AVRaiaNPCDebugging@Character@firefall@@";

    public const int SharedTextFieldOffset = 0x28;
    public const int SharedTextChunkByteLimit = 0x100;
    public const int SharedTextTemporaryBufferByteCount = 0x104;
    public const int SharedTextTerminatorByte = 0;
}
