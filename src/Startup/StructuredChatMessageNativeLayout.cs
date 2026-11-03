namespace Firefall.Reconstruction.Startup;

public readonly record struct StructuredChatCaseSnapshot(
    byte Selector,
    uint CaseFileOffset,
    uint? PayloadReaderFileOffset);

public enum StructuredChatFieldEncoding
{
    NullTerminatedBytes,
    Byte,
    ByteZeroExtendedToDword,
    RawUInt32LittleEndian,
    RawUInt64LittleEndian
}

public readonly record struct StructuredChatPayloadFieldSnapshot(
    uint ReaderFileOffset,
    int NativeOffset,
    int NativeByteCount,
    int? FixedWireByteCount,
    StructuredChatFieldEncoding Encoding,
    uint ReadCallFileOffset,
    uint ReadHelperFileOffset);

public readonly record struct StructuredChatPendingNotificationSnapshot(
    int EventCode,
    int OwnerCollectionOffset,
    string? StatusType);

/// <summary>
/// Observed structured-chat dispatch behind the nonzero optional-data branch
/// of 0x6E2590. These are consumer cases, not a reconstruction of selector
/// 0x76's conflicting parser-side slBlob representation.
/// </summary>
public static class StructuredChatMessageNativeLayout
{
    public const uint DispatcherFileOffset = 0x6E3670;
    public const uint JumpTableVirtualAddress = 0xAE5254;
    public const uint JumpTableFileOffset = 0x6E4654;
    public const uint DefaultReturnFalseFileOffset = 0x6E3AF2;
    public const uint SharedReturnFileOffset = 0x6E3AF4;
    public const byte FirstSelector = 1;
    public const byte LastSelector = 19;

    public const uint DuelResultTemplateVirtualAddress = 0x1B6E374;
    public const string DuelResultTemplate = "CHAT_DUEL_RESULT";
    public const uint LocalizationHelperFileOffset = 0x390F20;
    public const uint DisplayHelperFileOffset = 0x6E3460;
    public const int ItemNotificationCode = 353;
    public const int InvitationNotificationCode = 266;
    public const int PendingIncomingNotificationCode = 345;
    public const int PendingOtherNotificationCode = 346;
    public const string PendingIncomingStatus = "PENDING_IN";
    public const uint Case5OptionalReaderFileOffset = 0x709650;
    public const uint Case10OptionalReaderFileOffset = 0x703910;
    public const uint Case10OptionalAssignmentFileOffset = 0x6F7D20;
    public const int OptionalValueNativeOffset = 8;
    public const int Case5OptionalValueWireByteCount = 14;
    public const int Case5OptionalCopyByteCount = 16;
    public const int Case10OptionalValueWireByteCount = 28;
    public const int Case10OptionalValueNativeExtent = 32;

    /// <summary>Fields relative to the optional reader's temporary value, excluding its presence byte.</summary>
    public static IReadOnlyList<StructuredChatPayloadFieldSnapshot> OptionalValueFields { get; } =
        Array.AsReadOnly(new[]
        {
            new StructuredChatPayloadFieldSnapshot(0x709650, 0x00, 8, 8,
                StructuredChatFieldEncoding.RawUInt64LittleEndian, 0x7096C8, 0x356F80),
            new StructuredChatPayloadFieldSnapshot(0x709650, 0x08, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x7096D3, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x709650, 0x0C, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x7096DD, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x709650, 0x0D, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x7096E7, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x703910, 0x00, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x703989, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x703910, 0x08, 8, 8,
                StructuredChatFieldEncoding.RawUInt64LittleEndian, 0x703994, 0x356F80),
            new StructuredChatPayloadFieldSnapshot(0x703910, 0x10, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x70399F, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x703910, 0x14, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x7039AA, 0x3568E0),
            new StructuredChatPayloadFieldSnapshot(0x703910, 0x18, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x7039B5, 0x3568E0),
            new StructuredChatPayloadFieldSnapshot(0x703910, 0x1C, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x7039C0, 0x3568E0)
        });

    public static IReadOnlyList<StructuredChatPayloadFieldSnapshot> PayloadFields { get; } =
        Array.AsReadOnly(new[]
        {
            new StructuredChatPayloadFieldSnapshot(0x6FDBB0, 0x00, 4, null,
                StructuredChatFieldEncoding.NullTerminatedBytes, 0x6FDBE9, 0x12EE010),
            new StructuredChatPayloadFieldSnapshot(0x6FDBB0, 0x04, 4, null,
                StructuredChatFieldEncoding.NullTerminatedBytes, 0x6FDBF3, 0x12EE010),
            new StructuredChatPayloadFieldSnapshot(0x6FDC30, 0x00, 4, null,
                StructuredChatFieldEncoding.NullTerminatedBytes, 0x6FDC69, 0x12EE010),
            new StructuredChatPayloadFieldSnapshot(0x6FDC30, 0x04, 4, 1,
                StructuredChatFieldEncoding.ByteZeroExtendedToDword, 0x6FDC73, 0x3849C0),
            new StructuredChatPayloadFieldSnapshot(0x6FDC30, 0x08, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x6FDC7E, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x6FDC30, 0x0C, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x6FDC89, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x6FDCC0, 0x00, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x6FDCF8, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x6FDCC0, 0x04, 4, null,
                StructuredChatFieldEncoding.NullTerminatedBytes, 0x6FDD02, 0x12EE010),
            new StructuredChatPayloadFieldSnapshot(0x6FDCC0, 0x08, 4, 1,
                StructuredChatFieldEncoding.ByteZeroExtendedToDword, 0x6FDD0C, 0x3849C0),
            new StructuredChatPayloadFieldSnapshot(0x6FDD50, 0x00, 4, null,
                StructuredChatFieldEncoding.NullTerminatedBytes, 0x6FDD85, 0x12EE010),
            new StructuredChatPayloadFieldSnapshot(0x6FDF10, 0x00, 8, 8,
                StructuredChatFieldEncoding.RawUInt64LittleEndian, 0x6FDF48, 0x356F80),
            new StructuredChatPayloadFieldSnapshot(0x6FDF10, 0x08, 4, null,
                StructuredChatFieldEncoding.NullTerminatedBytes, 0x6FDF52, 0x12EE010),
            new StructuredChatPayloadFieldSnapshot(0x6FDF10, 0x0C, 4, null,
                StructuredChatFieldEncoding.NullTerminatedBytes, 0x6FDF5C, 0x12EE010),
            new StructuredChatPayloadFieldSnapshot(0x6FE120, 0x00, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x6FE153, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x6FE240, 0x00, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x6FE273, 0x356CF0)
        });

    public static IReadOnlyList<StructuredChatCaseSnapshot> Cases { get; } =
        Array.AsReadOnly(new[]
        {
            new StructuredChatCaseSnapshot(1, 0x6E3710, 0x6FDBB0),
            new StructuredChatCaseSnapshot(2, 0x6E38AE, 0x6FDC30),
            new StructuredChatCaseSnapshot(3, 0x6E3998, 0x6FDCC0),
            new StructuredChatCaseSnapshot(4, 0x6E3A3C, 0x6FDD50),
            new StructuredChatCaseSnapshot(5, 0x6E3B12, 0x6FDDC0),
            new StructuredChatCaseSnapshot(6, 0x6E3D58, null),
            new StructuredChatCaseSnapshot(7, 0x6E3DAB, null),
            new StructuredChatCaseSnapshot(8, 0x6E3DAB, null),
            new StructuredChatCaseSnapshot(9, 0x6E3E79, 0x6FDE30),
            new StructuredChatCaseSnapshot(10, 0x6E3EFD, 0x6FDEA0),
            new StructuredChatCaseSnapshot(11, 0x6E3F6C, 0x6FDF10),
            new StructuredChatCaseSnapshot(12, 0x6E40D0, 0x6FDFA0),
            new StructuredChatCaseSnapshot(13, 0x6E44A2, null),
            new StructuredChatCaseSnapshot(14, 0x6E4213, 0x6FE070),
            new StructuredChatCaseSnapshot(15, 0x6E42B9, 0x6FE120),
            new StructuredChatCaseSnapshot(16, 0x6E4347, 0x6FE120),
            new StructuredChatCaseSnapshot(17, 0x6E43BB, 0x6FE190),
            new StructuredChatCaseSnapshot(18, 0x6E4423, null),
            new StructuredChatCaseSnapshot(19, 0x6E458A, 0x6FE240)
        });

    public static StructuredChatCaseSnapshot? Find(byte selector) =>
        selector is >= FirstSelector and <= LastSelector
            ? Cases[selector - FirstSelector]
            : null;

    /// <summary>Captures case19 after decoding; does not perform removal or dispatch.</summary>
    public static StructuredChatPendingNotificationSnapshot CapturePendingNotification(byte decodedFlag) =>
        decodedFlag != 0
            ? new(PendingIncomingNotificationCode, 0x34, PendingIncomingStatus)
            : new(PendingOtherNotificationCode, 0x28, null);

    public static int Case5OptionalConsumedByteCount(byte presence) =>
        1 + (presence == 0 ? 0 : Case5OptionalValueWireByteCount);

    public static int Case10OptionalConsumedByteCount(byte presence) =>
        1 + (presence == 0 ? 0 : Case10OptionalValueWireByteCount);
}
