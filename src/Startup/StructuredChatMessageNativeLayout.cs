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
    RawUInt16LittleEndian,
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

public readonly record struct StructuredChatMapUpdateSnapshot(
    bool CompareExistingMap,
    bool AssignDecodedMap,
    int? EventCode);

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
    public const uint Case5PayloadReaderFileOffset = 0x709750;
    public const int Case5MinimumPayloadByteCount = 37;
    public const int Case12MapChangedNotificationCode = 268;
    public const int Case12OwnerCollectionOffset = 0x1FC;
    public const int Case12OwnerNodeKeyOffset = 0x10;
    public const int Case12OwnerNodeValueOffset = 0x18;
    public const string Case12ChatIdField = "chatId";
    public const string Case12EntityIdField = "entityId";
    public const uint Case12OwnerKeyCountFileOffset = 0x6F6600;
    public const uint Case12OwnerFindOrCreateFileOffset = 0x6F6420;
    public const uint Case12MapEqualsFileOffset = 0x6FE010;
    public const uint Case12ValueEqualsFileOffset = 0x6E2060;
    public const uint Case12KeySetEqualsFileOffset = 0x6FD3E0;
    public const uint Case12MapAssignmentFileOffset = 0x6F9840;
    public const uint MultiEntryPrerequisiteFileOffset = 0x48C2B0;
    public const int PrerequisiteWordOffset = 0x118;
    public const int PrerequisiteRangeBeginOffset = 0x10C;
    public const int PrerequisiteRangeEndOffset = 0x110;
    public const int PrerequisiteEntryNativeByteCount = 104;

    public static IReadOnlyList<(byte Selector, uint CallFileOffset, uint FailureTarget, bool FailureReturn)>
        MultiEntryPrerequisiteCalls { get; } = Array.AsReadOnly(new (byte, uint, uint, bool)[]
        {
            (9, 0x6E3E7B, 0x6E3AF2, false),
            (10, 0x6E3EFF, 0x6E3AF2, false),
            (11, 0x6E3F6E, 0x6E3AF2, false),
            (12, 0x6E40D2, 0x6E3AF2, false),
            (13, 0x6E44A4, 0x6E4583, true),
            (18, 0x6E4425, 0x6E3AF2, false)
        });

    /// <summary>Case5 fields in wire order, with the optional member between native +0x20 and +0x40.</summary>
    public static IReadOnlyList<StructuredChatPayloadFieldSnapshot> Case5FixedFields { get; } =
        Array.AsReadOnly(new[]
        {
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x00, 4, 1,
                StructuredChatFieldEncoding.ByteZeroExtendedToDword, 0x70975F, 0x3849C0),
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x04, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x70976A, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x08, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x709774, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x09, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x70977E, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x0C, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x709789, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x10, 8, 8,
                StructuredChatFieldEncoding.RawUInt64LittleEndian, 0x709794, 0x356F80),
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x18, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x70979F, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x1C, 4, null,
                StructuredChatFieldEncoding.NullTerminatedBytes, 0x7097A9, 0x12EE010),
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x20, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x7097B4, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x40, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x7097C8, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x41, 1, 1,
                StructuredChatFieldEncoding.Byte, 0x7097D2, 0x337180),
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x44, 4, 4,
                StructuredChatFieldEncoding.RawUInt32LittleEndian, 0x7097DD, 0x3370A0),
            new StructuredChatPayloadFieldSnapshot(0x709750, 0x48, 2, 2,
                StructuredChatFieldEncoding.RawUInt16LittleEndian, 0x7097E8, 0x336AC0)
        });

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

    /// <summary>
    /// Captures case12 after the owner prerequisite and payload reader succeed.
    /// An absent owner entry is created even when its decoded map is empty.
    /// The equality input is used only when an owner entry already exists.
    /// </summary>
    public static StructuredChatMapUpdateSnapshot CaptureCase12MapUpdate(
        bool ownerKeyPresent, uint decodedOuterEntryCount, bool mapsEqual) =>
        ownerKeyPresent
            ? new(true, !mapsEqual, mapsEqual ? null : Case12MapChangedNotificationCode)
            : new(false, true, decodedOuterEntryCount == 0 ? null : Case12MapChangedNotificationCode);

    /// <summary>
    /// Captures the native arithmetic, not validation of a managed collection.
    /// Subtraction wraps, division is signed, and the final comparison is unsigned.
    /// </summary>
    public static bool PassesMultiEntryPrerequisite(uint nativeWord118, uint rangeBegin, uint rangeEnd)
    {
        if (nativeWord118 == 0)
            return false;

        int byteLength = unchecked((int)(rangeEnd - rangeBegin));
        uint entryCount = unchecked((uint)(byteLength / PrerequisiteEntryNativeByteCount));
        return entryCount > 1;
    }

    public static int Case5OptionalConsumedByteCount(byte presence) =>
        1 + (presence == 0 ? 0 : Case5OptionalValueWireByteCount);

    public static int Case10OptionalConsumedByteCount(byte presence) =>
        1 + (presence == 0 ? 0 : Case10OptionalValueWireByteCount);
}
