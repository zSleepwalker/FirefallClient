namespace Firefall.Reconstruction.Startup;

public readonly record struct StructuredChatCollectionSnapshot(
    byte Selector,
    uint ReaderFileOffset,
    int NativeCollectionOffset,
    int WireEntryByteCount,
    int NativeValueByteCount,
    uint ContinuationCompareFileOffset);

/// <summary>Observed count-byte chunked collections inside structured-chat payloads.</summary>
public static class StructuredChatCollectionNativeLayout
{
    public const byte ContinuationCount = 0xFF;
    public const int Case9PrefixWireByteCount = 9;
    public const int Case14PrefixWireByteCount = 24;
    public const int Case17MinimumPayloadByteCount = 19;
    public const uint Case12ReaderFileOffset = 0x705570;
    public const uint Case12VectorReaderFileOffset = 0x7035C0;
    public const uint Case12KeySetReaderFileOffset = 0x70A340;
    public const uint Case12KeySetClearFileOffset = 0x70A2E0;
    public const uint Case12KeySetInsertFileOffset = 0x38EC20;
    public const uint Case12VectorAssignmentFileOffset = 0x47A770;
    public const uint Case12KeySetAssignmentFileOffset = 0x706C20;
    public const int Case12KeyWireByteCount = 4;
    public const int Case12MinimumPayloadByteCount = 1;
    public const int Case12MinimumEntryWireByteCount = 6;
    public const int Case12NativeNodeValueOffset = 0x0C;
    public const int Case12NativeValueVectorOffset = 0;
    public const int Case12NativeValueKeySetOffset = 0x20;
    public const uint UnpackExceptionThrowThunkFileOffset = 0x159E27A;
    public const uint CxxThrowExceptionImportVirtualAddress = 0x1AC4770;
    public const uint UnpackExceptionPointerThrowInfoVirtualAddress = 0x1DB8108;
    public const uint UnpackExceptionPointerCatchableArrayVirtualAddress = 0x1DB80FC;
    public const uint UnpackExceptionPointerCatchableTypeVirtualAddress = 0x1DB80E0;
    public const uint UnpackExceptionPointerTypeDescriptorVirtualAddress = 0x1E125C4;
    public const string UnpackExceptionPointerRttiName = ".PAVunpack_exception@serialize@sl@@";

    public static IReadOnlyList<StructuredChatCollectionSnapshot> Collections { get; } =
        Array.AsReadOnly(new[]
        {
            new StructuredChatCollectionSnapshot(9, 0x7093F0, 0x0C, 8, 8, 0x70950A),
            new StructuredChatCollectionSnapshot(14, 0x5DE550, 0x18, 10, 12, 0x5DE6BE),
            new StructuredChatCollectionSnapshot(17, 0x6002C0, 0x14, 4, 4, 0x60046B)
        });

    public static IReadOnlyList<StructuredChatPayloadFieldSnapshot> SurroundingFields { get; } =
        Array.AsReadOnly(new[]
        {
            new StructuredChatPayloadFieldSnapshot(0x709560, 0, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x70956F, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x709560, 1, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x709579, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x709560, 2, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x709583, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x709560, 3, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x70958D, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x709560, 4, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x709597, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x709560, 5, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x7095A1, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x709560, 6, 2, 2,
                StructuredChatFieldEncoding.RawUInt16LittleEndian, 0x7095AC, 0x34B740),
            new StructuredChatPayloadFieldSnapshot(0x709560, 8, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x7095B6, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x6FE070, 0x00, 8, 8,
                StructuredChatFieldEncoding.RawUInt64LittleEndian, 0x6FE0A6, 0x356F80),
            new StructuredChatPayloadFieldSnapshot(0x6FE070, 0x08, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x6FE0B1, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x6FE070, 0x0C, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x6FE0BC, 0x3568E0),
            new StructuredChatPayloadFieldSnapshot(0x6FE070, 0x10, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x6FE0C7, 0x3568E0),
            new StructuredChatPayloadFieldSnapshot(0x6FE070, 0x14, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x6FE0D2, 0x3568E0),
            new StructuredChatPayloadFieldSnapshot(0x6FE190, 0x00, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x6FE1C8, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x6FE190, 0x04, 4, null,
                StructuredChatFieldEncoding.NullTerminatedBytes, 0x6FE1D2, 0x12EE010),
            new StructuredChatPayloadFieldSnapshot(0x6FE190, 0x08, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x6FE1DD, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x6FE190, 0x0C, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x6FE1E8, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x6FE190, 0x10, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x6FE1F3, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x6FE190, 0x48, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x6FE207, 0x337180)
        });
}
