namespace Firefall.Reconstruction.Startup;

public readonly record struct LoginResponseHandlerSnapshot(
    string Purpose,
    uint MethodFileOffset,
    IReadOnlyList<string> JsonKeys,
    IReadOnlyList<string> ErrorKeys);

public readonly record struct LoginResponseCallbackSnapshot(
    string Purpose,
    int OwnerMemberOffset,
    uint TargetVirtualAddress,
    uint TargetFileOffset);

public readonly record struct LoginRequestAttachmentSnapshot(
    string Purpose,
    uint BuilderFileOffset,
    uint AttachmentFileOffset,
    int OwnerCallbackPairOffset,
    int NativeRequestType,
    string EndpointFormat,
    int? InFlightFlagOffset)
{
    public string HttpMethod => ClientApiRequestNativeLayout.ResolveHttpMethod(NativeRequestType);
}

/// <summary>
/// Verified addresses, object offsets, and literal catalogs from the 32-bit
/// tfLogin object. These observations do not implement the native login protocol.
/// </summary>
public static class LoginModeNativeLayout
{
    private static readonly string[] InputActions =
    [
        "LeftClick",
        "RightClick",
        "ArrowUp",
        "ArrowDown",
        "ArrowLeft",
        "ArrowRight",
        "ScrollUp",
        "ScrollDown",
        "ToStart",
        "ToEnd",
        "Delete",
        "Tab",
        "Submit",
        "Escape",
        "RunConsoleMacro"
    ];

    private static readonly string[] AutoLoginSettings =
    [
        "MaxTimes",
        "Account",
        "SavedPassword",
        "CharacterName",
        "DevMatrixAddress",
        "SpectatorMode",
        "SpectatorDirectorControl",
        "SpectatorTargetGuid",
        "RandomReplay"
    ];

    private static readonly LoginResponseHandlerSnapshot[] ResponseHandlers =
    [
        new(
            "Login response",
            0x63B7C0,
            [
                "can_login", "is_dev", "steam_auth_prompt", "skip_precursor",
                "cais_status", "state", "duration", "expires_at",
                "character_limit", "is_vip", "vip_expiration"
            ],
            ["login_failed", "ERR_INVALID_LOGIN"]),
        new(
            "Character refresh response",
            0x63C5F0,
            ["characters", "character_guid", "name", "visuals"],
            ["characterlist_failed"]),
        new(
            "Zone list response",
            0x63D270,
            ["zone_list", "zone_id", "zone_name", "protocol_version", "build", "matrix_url", "players"],
            [])
    ];

    private static readonly LoginResponseCallbackSnapshot[] ResponseCallbacks =
    [
        new("Login response", 0x9D8, 0xA3C3C0, 0x63B7C0),
        new("Character refresh response", 0x9E8, 0xA3D1F0, 0x63C5F0),
        new("Zone-list response", 0xA00, 0xA3DE70, 0x63D270)
    ];

    private static readonly LoginRequestAttachmentSnapshot[] RequestAttachments =
    [
        new("Login response", 0x63B550, 0x63B653, 0x9D8, 2,
            LoginEndpointFormat, 0x9E0),
        new("Character refresh response", 0x63ECE0, 0x63EDBC, 0x9E8, 1,
            CharacterListEndpointFormat, 0x9F0),
        new("Zone-list response", 0x63E390, 0x63E56C, 0xA00, 2,
            ZoneListEndpointFormat, null)
    ];

    public const int AllocationSize = 0xA58;
    public const uint ConstructorFileOffset = 0x6365B0;
    public const uint InitializeFileOffset = 0x636FF0;
    public const uint ShutdownFileOffset = 0x63B360;
    public const uint DeletingDestructorFileOffset = 0x636AD0;
    public const uint VtableVirtualAddress = 0x1C676C0;
    public const uint VtableFileOffset = 0x18662C0;
    public const uint BaseVtableVirtualAddress = 0x1C67710;
    public const uint BaseVtableFileOffset = 0x1866310;
    public const int ZoneObjectOffset = 0x10;
    public const int InputContextOffset = 0x50;
    public const int CharacterRecordsBeginOffset = 0x9F4;
    public const int CharacterRecordsEndOffset = 0x9F8;
    public const int CharacterRecordStride = 0xD0;
    public const int CharacterRecordMatchTextOffset = 8;
    public const int SelectedCharacterPairFirstDestinationOffset = 0xA40;
    public const int SelectedCharacterPairSecondDestinationOffset = 0xA44;
    public const uint CharacterRefreshHandlerFileOffset = 0x63C5F0;
    public const int CharacterRefreshRequestMethodVtableOffset = 0xAC;
    public const int CharacterRefreshRequestMessageId = 0x89;
    public const int LoginUpdateMethodVtableOffset = 0x0C;
    public const uint LoginUpdateMethodFileOffset = 0x6385C0;
    public const uint LoginUpdateWorldModeQueueCallSiteFileOffset = 0x638B89;
    public const uint LoginUpdateDefaultCaseFileOffset = 0x638AB5;
    public const uint LoginUpdateGuardGlobalVa = 0x1E5CAA4;
    public const uint LoginUpdateGuardByteOffset = 0x40;
    public const uint LoginUpdateGuardReadFileOffset = 0x638ABC;
    // Observed: selector is [[esi+0x20]->first node + 0xC]; cmp 4 / ja default; jmp [edx*4+table].
    public const uint LoginUpdateSwitchSelectorLoadFileOffset = 0x6388D8;
    public const uint LoginUpdateSwitchJumpTableVirtualAddress = 0xA3991C;
    public const uint LoginUpdateSwitchJumpTableFileOffset = 0x638D1C;
    public const uint LoginUpdateSwitchCaseCount = 5;
    public const uint LoginUpdateSwitchCase1FileOffset = 0x6389F2;
    public const uint LoginUpdateSwitchSharedCaseFileOffset = 0x638C3A;
    // The ja target is 0x638AB3 (mov [ecx],eax); 0x638AB5 is the next instruction.
    public const uint LoginUpdateSwitchDefaultTargetFileOffset = 0x638AB3;
    public const uint LoginUpdateCase1MessageSendFileOffset = 0x638A3B;
    public const uint LoginUpdateCase1MessageId = 0x146;
    public const uint LoginUpdateLookingGlassPathFileOffset = 0x638B1A;
    public const uint PendingRequestEnqueueActiveFlagOffset = 0xC;
    public const uint PendingRequestQueueOffset = 0xB84;
    public const uint PendingRequestEnqueueFlagCheckFileOffset = 0x30DED8;
    public const uint PendingRequestEnqueuePushFileOffset = 0x30E00A;
    public const uint LoginResponseNotificationCallSiteFileOffset = 0x63C1E2;
    public const int LoginResponseNotificationMessageId = 0x145;
    public const int ResponseCallbackObjectSize = 0x20;
    public const int ResponseCallbackOwnerPointerOffset = 0x04;
    public const int ResponseCallbackTargetPointerOffset = 0x10;
    public const int ResponseCallbackOwnerAdjustmentOffset = 0x14;
    public const int ResponseCallbackOwnerAdjustmentInitialValue = 0;
    public const int ResponseCallbackDispatchMethodVtableOffset = 0x04;
    public const uint ResponseCallbackDispatchMethodFileOffset = 0x643380;
    public const uint ResponseCallbackVtableVirtualAddress = 0x1C674A8;
    public const uint ResponseCallbackVtableFileOffset = 0x18660A8;
    public const uint LoginRequestSubmissionMethodFileOffset = 0x63B550;
    public const int LoginRequestCallbackPairOffset = 0x9D8;
    public const int LoginRequestInFlightFlagOffset = 0x9E0;
    public const int LoginRequestOptionFlagOffset = 0x9E2;
    public const int LoginRequestObjectAllocationSize = 0x78;
    public const int LoginRequestCallbackPairOffsetInObject = 0x50;
    public const int LoginRequestTypeOffsetInObject = 0x08;
    public const int LoginRequestTypeValue = 2;
    public const uint RequestObjectConstructorFileOffset = 0x3843F0;
    public const uint RequestPreparationMethodFileOffset = 0x636550;
    public const uint RequestSubmissionWrapperFileOffset = 0x6499F0;
    public const uint RequestManagerSubmissionMethodFileOffset = 0xBC43C0;
    public const int CharacterRefreshRequestInFlightFlagOffset = 0x9F0;
    public const uint AutoLoginStateDispatchMethodFileOffset = 0x63E390;
    public const int AutoLoginStateOffset = 0xA48;
    public const int ZoneListRequestAutoLoginState = 2;
    public const int ConnectSelectedCharacterAutoLoginState = 5;
    public const uint ConnectSelectedCharacterStateBranchFileOffset = 0x63E890;
    public const uint PrepareSelectedCharacterMethodFileOffset = 0x63EE30;
    public const uint RequestMatrixConnectionMethodFileOffset = 0x63F230;
    public const int SelectedCharacterPropertiesOffsetInRecord = 0xC8;
    public const int CharacterConnectionObjectMemberOffset = 0x1C;
    public const int CharacterConnectionObjectAllocationSize = 0xBB0;
    public const int CharacterConnectionStateOffset = 0x0C;
    public const int CharacterConnectionRequiredStateUpperBoundExclusive = 2;
    public const uint MatrixConnectionRequestBoundaryFileOffset = 0x494CA0;
    public const string MatrixOverrideConfigSection = "Development";
    public const string MatrixOverrideConfigKey = "MatrixOverride";
    public const uint AutoLoginStateJumpTableVirtualAddress = 0xA3F574;
    public const uint AutoLoginStateJumpTableFileOffset = 0x63E974;
    public const uint CharacterRefreshRequestReceiverGlobalVirtualAddress = 0x1E002B8;
    public const uint CharacterRefreshRequestReceiverGlobalFileOffset = 0x19FE6B8;
    public const int LoginAutoRetryCounterOffset = 0x9E4;
    public const int LoginAutoRetryCounterInitialValue = 3;
    public const int LoginAutoRetryDelayMilliseconds = 500;
    public const string UiLayout = "LoginUI.xml";
    public const string LoginEndpointFormat = "%s/api/v2/accounts/login";
    public const string CharacterListEndpointFormat = "%s/api/v2/characters/list";
    public const string ZoneListEndpointFormat = "%s/api/v1/server/list";
    public static IReadOnlyList<string> KnownCharacterConnectionPropertyKeys { get; } =
        Array.AsReadOnly(new[] { "name", "is_dev", "race", "gender" });

    public static IReadOnlyList<string> KnownInputActions => InputActions;
    public static IReadOnlyList<string> KnownAutoLoginSettings => AutoLoginSettings;
    public static IReadOnlyList<LoginResponseHandlerSnapshot> KnownResponseHandlers => ResponseHandlers;
    public static IReadOnlyList<LoginResponseCallbackSnapshot> KnownResponseCallbacks => ResponseCallbacks;
    public static IReadOnlyList<LoginRequestAttachmentSnapshot> KnownRequestAttachments => RequestAttachments;
}
