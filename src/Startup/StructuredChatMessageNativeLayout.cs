namespace Firefall.Reconstruction.Startup;

public readonly record struct StructuredChatCaseSnapshot(
    byte Selector,
    uint CaseFileOffset,
    uint? PayloadReaderFileOffset);

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
}
