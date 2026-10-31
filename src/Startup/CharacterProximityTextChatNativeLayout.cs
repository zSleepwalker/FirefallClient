namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x64 shared-text and optional-blob layout. Field meanings remain unknown.
/// </summary>
public static class CharacterProximityTextChatNativeLayout
{
    public const int InnerSelector = 0x64;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D1A2;
    public const uint CaseLocalDwordValue = 0x6C;
    public const int AllocationSize = 0x48;
    public const uint ConstructorFileOffset = 0x5259C0;
    public const uint BaseConstructorHelperFileOffset = 0x5A0BB0;
    public const int BaseConstructorArgument = 0x64;
    public const uint BodyReaderFileOffset = 0x5F1BC0;
    public const uint SharedTextReaderFileOffset = 0x12EE010;
    public const uint OptionalBlobReaderFileOffset = 0x5F19A0;
    public const uint WordLengthPrefixedBlobReaderFileOffset = 0x12EE170;
    public const uint OptionalBlobUpdateHelperFileOffset = 0x5F1920;
    public const uint OptionalBlobClearHelperFileOffset = 0x49AC30;
    public const uint VtableVirtualAddress = 0x1C67158;
    public const string RttiName = ".?AVProximityTextChat@Character@firefall@@";

    public const int SharedTextFieldOffset = 0x28;
    public const int SharedTextTerminatorByte = 0;
    public const int SharedTextChunkByteLimit = 0x100;
    public const int SharedTextTemporaryBufferByteCount = 0x104;
    public const int ByteField0Offset = 0x2C;
    public const int ByteField1Offset = 0x2D;
    public const int ByteField2Offset = 0x2E;
    public const int OptionalBlobPresenceFieldOffset = 0x30;
    public const int FixedByteCountAfterSharedTextBeforeBlobGate = 3;
    public const int OptionalBlobGateByteCount = 1;
    public const int OptionalBlobLengthPrefixByteCount = 2;
    public const int MaximumOptionalBlobPayloadByteCount = 0xFFFF;
    public const int EmptyTextInputByteCountWithoutOptionalBlob = 5;
    public const int EmptyTextInputByteCountWithEmptyOptionalBlob = 7;
}
