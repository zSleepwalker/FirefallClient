using Firefall.Reconstruction.Startup;

int failures = 0;

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (!condition)
        failures++;
}

var expected = new[]
{
    new AppModeSnapshot(ClientAppModeId.Login, "tfLogin", 0xA58, 0x6365B0, 0x1C676C0, 0x1E128C4, 0x1A10CC4),
    new AppModeSnapshot(ClientAppModeId.World, "tfWorld", 0x2904, 0x64AE40, 0x1C677BC, 0x1E12A9C, 0x1A10E9C),
    new AppModeSnapshot(ClientAppModeId.LookingGlass, "tfLookingGlass", 0x1368, 0x65A190, 0x1C6781C, 0x1E12C48, 0x1A11048)
};

Check("All three observed app modes are cataloged", AppModeCatalog.KnownModes.SequenceEqual(expected));
Check("Mode 1 maps to tfLogin", AppModeCatalog.Find(1) == expected[0]);
Check("Mode 2 maps to tfWorld", AppModeCatalog.Find(2) == expected[1]);
Check("Mode 3 maps to tfLookingGlass", AppModeCatalog.Find(3) == expected[2]);
Check("Unobserved modes are not guessed", AppModeCatalog.Find(0) is null && AppModeCatalog.Find(4) is null);
Check("Pending-mode queue records the verified mode mapping and queue offsets",
    AppModeCatalog.KnownPendingRequests.SequenceEqual(
    [
        new PendingAppModeRequestSnapshot(1, "tfLogin", 0x6365B0),
        new PendingAppModeRequestSnapshot(2, "tfWorld", 0x64AE40),
        new PendingAppModeRequestSnapshot(3, "tfLookingGlass", 0x65A190)
    ])
    && AppModeCatalog.PendingModeQueueInsertMethodFileOffset == 0x30DEB0
    && AppModeCatalog.EnqueueModeIdWithZeroPayloadMethodFileOffset == 0x324080
    && AppModeCatalog.ApplicationModeInitializationHelperFileOffset == 0x307224
    && AppModeCatalog.DefaultInitialModeEnqueueCallFileOffset == 0x30797C
    && AppModeCatalog.InitialPendingModeDrainCallFileOffset == 0x30798E
    && AppModeCatalog.DefaultInitialModeId == 1
    && AppModeCatalog.LoginFallbackRequestHelperFileOffset == 0x30D790
    && AppModeCatalog.QueueLookingGlassRequestMethodFileOffset == 0x30D590
    && AppModeCatalog.TryQueueLookingGlassRequestMethodFileOffset == 0x306050
    && AppModeCatalog.ConditionalLoginModeQueueMethodFileOffset == 0x6385C0
    && AppModeCatalog.PendingModeQueueStorageOffset == 0xB84
    && AppModeCatalog.PendingModeQueueHeadOffset == 0xB90
    && AppModeCatalog.PendingModeQueueCountOffset == 0xB94
    && AppModeCatalog.PendingModeRequestModeIdOffset == 0
    && AppModeCatalog.PendingModeRequestPayloadOffset == 4);
Check("LookingGlass queue helper has six observed call sites",
    AppModeCatalog.KnownLookingGlassQueueCallSites.Select(site => site.FileOffset)
        .SequenceEqual([0x306050u, 0x3D60A0u, 0x6385C0u, 0x65D400u, 0x6AE3E0u, 0x6AF120u])
    && AppModeCatalog.KnownLookingGlassQueueCallSites.All(site => !string.IsNullOrWhiteSpace(site.ObservedUse)));
Check("LookingGlass exposes the replay-reader interface",
    AppModeCatalog.KnownInterfaces.SequenceEqual(
    [
        new AppModeInterfaceSnapshot(
            ClientAppModeId.LookingGlass,
            "nl::ReplayReader::GameInterface",
            4,
            0x1C6786C,
            0x186646C,
            0x1E12AB4,
            0x1A10EB4,
            0x661473,
            0x65E870,
            0x65E940,
            0x65EB80)
    ]));
Check("LookingGlass reader pointer offset is recorded",
    LookingGlassNativeLayout.ReplayReaderPointerOffset == 0x1334
    && LookingGlassNativeLayout.ConstructorFileOffset == 0x65A190
    && LookingGlassNativeLayout.RequestContextArgumentOffset == 0x18
    && LookingGlassNativeLayout.RequestContextCopySize == 0x34
    && LookingGlassNativeLayout.RequestContextSourceAtApplicationOffset == 0x9F0
    && LookingGlassNativeLayout.AdditionalConstructorArgumentApplicationOffset == 0x9E8
    && LookingGlassNativeLayout.AdditionalConstructorArgumentDestinationOffset == 0x58
    && LookingGlassNativeLayout.RequestContextFirstSharedTextOffset == 0x14
    && LookingGlassNativeLayout.RequestContextSecondSharedTextOffset == 0x1C);
Check("tfWorld native allocation and owner/scene offsets are recorded",
    TfWorldNativeLayout.AllocationSize == 0x2904
    && TfWorldNativeLayout.OwnerContextOffset == 0x10
    && TfWorldNativeLayout.ScenePointerOffset == 0x14);
Check("tfWorld input-layer offsets preserve observed IDs",
    TfWorldNativeLayout.LayersByOffset.SequenceEqual(
    [
        new TfWorldLayerSnapshot(0x2FC, 0),
        new TfWorldLayerSnapshot(0xC4C, 9),
        new TfWorldLayerSnapshot(0x159C, 8),
        new TfWorldLayerSnapshot(0x1EEC, 9)
    ]));
Check("tfWorld layer-mode method and layer-stack address are recorded",
    TfWorldNativeLayout.InitializationMethodFileOffset == 0x64B460
    && TfWorldNativeLayout.ShutdownMethodFileOffset == 0x64B950
    && TfWorldNativeLayout.ObserverStateMethodFileOffset == 0x64BC40
    && TfWorldNativeLayout.LayerModeMethodFileOffset == 0x64C020
    && TfWorldNativeLayout.RecordInputMethodFileOffset == 0x64DCF0
    && TfWorldNativeLayout.CallbackStateResetMethodFileOffset == 0x651A60
    && TfWorldNativeLayout.MemberAt2E8GetterMethodFileOffset == 0x64A0C0
    && TfWorldNativeLayout.MemberAt2ECGetterMethodFileOffset == 0x64A0D0
    && TfWorldNativeLayout.LayerStackGlobalVirtualAddress == 0x1DFFEF4);
Check("tfWorld record-input size, state gate, and observed float offsets are recorded",
    TfWorldNativeLayout.RecordInputSize == 0x20
    && TfWorldNativeLayout.MemberAt2E8Offset == 0x2E8
    && TfWorldNativeLayout.MemberAt2ECOffset == 0x2EC
    && TfWorldNativeLayout.MemberAt2E8InitialValue == 0
    && TfWorldNativeLayout.MemberAt2ECAllocatedBlockSize == 0x2C
    && TfWorldNativeLayout.RecordInputRequiredSceneSubstate == 3
    && TfWorldNativeLayout.RecordInputStoredInModeOffset == 0x20
    && TfWorldNativeLayout.RecordInputFirstObservedFloatOffset == 8
    && TfWorldNativeLayout.RecordInputSecondObservedFloatOffset == 12);
Check("World scene-factory resources, owned allocations, and tfPhysicsDrawInterface RTTI are recorded",
    SceneFactoryNativeLayout.FactoryFileOffset == 0x9B73F0
    && SceneFactoryNativeLayout.RootAllocationSize == 0xA0
    && SceneFactoryNativeLayout.RootConstructorFileOffset == 0x9B82F0
    && SceneFactoryNativeLayout.RootDestructorFileOffset == 0x9B8490
    && SceneFactoryNativeLayout.RootReturnFileOffset == 0x9B82B4
    && SceneFactoryNativeLayout.KnownSceneResources.SequenceEqual(
    [
        new SceneFactoryResourceSnapshot("RenderScene", 0x00, 0x179BD28),
        new SceneFactoryResourceSnapshot("WorldMapScene", 0x04, 0x179BE38),
        new SceneFactoryResourceSnapshot("SinvironmentScene", 0x08, 0x179BE48)
    ])
    && SceneFactoryNativeLayout.KnownChildAllocations.SequenceEqual(
    [
        new SceneFactoryChildAllocationSnapshot(0x0C, 0x138, false),
        new SceneFactoryChildAllocationSnapshot(0x14, 0x188, false),
        new SceneFactoryChildAllocationSnapshot(0x18, 0x060, false),
        new SceneFactoryChildAllocationSnapshot(0x1C, 0x044, false),
        new SceneFactoryChildAllocationSnapshot(0x20, 0x044, false),
        new SceneFactoryChildAllocationSnapshot(0x24, 0x060, false),
        new SceneFactoryChildAllocationSnapshot(0x28, 0x03C, false),
        new SceneFactoryChildAllocationSnapshot(0x2C, 0x138, true),
        new SceneFactoryChildAllocationSnapshot(0x30, 0x024, false),
        new SceneFactoryChildAllocationSnapshot(0x34, 0x25C8, false),
        new SceneFactoryChildAllocationSnapshot(0x3C, 0x058, false),
        new SceneFactoryChildAllocationSnapshot(0x4C, 0x390, false),
        new SceneFactoryChildAllocationSnapshot(0x50, 0x1B4, true),
        new SceneFactoryChildAllocationSnapshot(0x54, 0xF28, false),
        new SceneFactoryChildAllocationSnapshot(0x58, 0x080, false),
        new SceneFactoryChildAllocationSnapshot(0x5C, 0x058, false),
        new SceneFactoryChildAllocationSnapshot(0x60, 0x02C, false),
        new SceneFactoryChildAllocationSnapshot(0x64, 0x2F4C, false),
        new SceneFactoryChildAllocationSnapshot(0x68, 0x044, false),
        new SceneFactoryChildAllocationSnapshot(0x6C, 0x008, false),
        new SceneFactoryChildAllocationSnapshot(0x70, 0x0C0, false),
        new SceneFactoryChildAllocationSnapshot(0x74, 0x0D8, false),
        new SceneFactoryChildAllocationSnapshot(0x78, 0x21C, false),
        new SceneFactoryChildAllocationSnapshot(0x84, 0x024, false),
        new SceneFactoryChildAllocationSnapshot(0x88, 0x048, true),
        new SceneFactoryChildAllocationSnapshot(0x90, 0x114, false)
    ])
    && SceneFactoryNativeLayout.PhysicsDrawInterfacePointerOffset == 0x14
    && SceneFactoryNativeLayout.PhysicsDrawInterfaceAllocationSize == 0x188
    && SceneFactoryNativeLayout.PhysicsDrawInterfaceVtableVirtualAddress == 0x1C70968
    && SceneFactoryNativeLayout.PhysicsDrawInterfaceRttiName == ".?AVtfPhysicsDrawInterface@@"
    && SceneFactoryNativeLayout.TfWorldScenePointerOffset == TfWorldNativeLayout.ScenePointerOffset);
Check("tfLogin native layout and lifecycle addresses are recorded",
    LoginModeNativeLayout.AllocationSize == 0xA58
    && LoginModeNativeLayout.ConstructorFileOffset == 0x6365B0
    && LoginModeNativeLayout.InitializeFileOffset == 0x636FF0
    && LoginModeNativeLayout.ShutdownFileOffset == 0x63B360
    && LoginModeNativeLayout.DeletingDestructorFileOffset == 0x636AD0
    && LoginModeNativeLayout.VtableVirtualAddress == 0x1C676C0
    && LoginModeNativeLayout.VtableFileOffset == 0x18662C0
    && LoginModeNativeLayout.CharacterRecordsBeginOffset == 0x9F4
    && LoginModeNativeLayout.CharacterRecordsEndOffset == 0x9F8
    && LoginModeNativeLayout.CharacterRecordStride == 0xD0
    && LoginModeNativeLayout.CharacterRecordMatchTextOffset == 8
    && LoginModeNativeLayout.SelectedCharacterPairFirstDestinationOffset == 0xA40
    && LoginModeNativeLayout.SelectedCharacterPairSecondDestinationOffset == 0xA44
    && LoginModeNativeLayout.CharacterRefreshHandlerFileOffset == 0x63C5F0
    && LoginModeNativeLayout.CharacterRefreshRequestMethodVtableOffset == 0xAC
    && LoginModeNativeLayout.CharacterRefreshRequestMessageId == 0x89
    && LoginModeNativeLayout.LoginRequestSubmissionMethodFileOffset == 0x63B550
    && LoginModeNativeLayout.LoginRequestCallbackPairOffset == 0x9D8
    && LoginModeNativeLayout.LoginRequestInFlightFlagOffset == 0x9E0
    && LoginModeNativeLayout.LoginRequestOptionFlagOffset == 0x9E2
    && LoginModeNativeLayout.LoginRequestObjectAllocationSize == 0x78
    && LoginModeNativeLayout.LoginRequestCallbackPairOffsetInObject == 0x50
    && LoginModeNativeLayout.LoginRequestTypeOffsetInObject == 0x08
    && LoginModeNativeLayout.LoginRequestTypeValue == 2
    && LoginModeNativeLayout.LoginAutoRetryCounterOffset == 0x9E4
    && LoginModeNativeLayout.LoginAutoRetryCounterInitialValue == 3
    && LoginModeNativeLayout.LoginAutoRetryDelayMilliseconds == 500);
Check("tfLogin observed UI layout and 15 input action names are recorded",
    LoginModeNativeLayout.UiLayout == "LoginUI.xml"
    && LoginModeNativeLayout.InputContextOffset == 0x50
    && LoginModeNativeLayout.KnownInputActions.SequenceEqual(
    [
        "LeftClick", "RightClick", "ArrowUp", "ArrowDown", "ArrowLeft",
        "ArrowRight", "ScrollUp", "ScrollDown", "ToStart", "ToEnd",
        "Delete", "Tab", "Submit", "Escape", "RunConsoleMacro"
    ]));
Check("tfLogin response handler field catalogs are recorded",
    LoginModeNativeLayout.KnownResponseHandlers.Select(handler => handler.MethodFileOffset)
        .SequenceEqual([0x63B7C0u, 0x63C5F0u, 0x63D270u])
    && LoginModeNativeLayout.KnownResponseHandlers[0].JsonKeys.Contains("can_login")
    && LoginModeNativeLayout.KnownResponseHandlers[1].JsonKeys.Contains("character_guid")
    && LoginModeNativeLayout.KnownResponseHandlers[2].Purpose == "Zone list response");
Check("tfLogin constructor callback wrappers target the three response handlers",
    LoginModeNativeLayout.ResponseCallbackObjectSize == 0x20
    && LoginModeNativeLayout.ResponseCallbackOwnerPointerOffset == 4
    && LoginModeNativeLayout.ResponseCallbackTargetPointerOffset == 0x10
    && LoginModeNativeLayout.ResponseCallbackOwnerAdjustmentOffset == 0x14
    && LoginModeNativeLayout.ResponseCallbackOwnerAdjustmentInitialValue == 0
    && LoginModeNativeLayout.ResponseCallbackDispatchMethodVtableOffset == 4
    && LoginModeNativeLayout.ResponseCallbackDispatchMethodFileOffset == 0x643380
    && LoginModeNativeLayout.ResponseCallbackVtableVirtualAddress == 0x1C674A8
    && LoginModeNativeLayout.ResponseCallbackVtableFileOffset == 0x18660A8
    && LoginModeNativeLayout.KnownResponseCallbacks.SequenceEqual(
    [
        new LoginResponseCallbackSnapshot("Login response", 0x9D8, 0xA3C3C0, 0x63B7C0),
        new LoginResponseCallbackSnapshot("Character refresh response", 0x9E8, 0xA3D1F0, 0x63C5F0),
        new LoginResponseCallbackSnapshot("Zone-list response", 0xA00, 0xA3DE70, 0x63D270)
    ]));
Check("All three Login response callbacks have observed request attachment sites",
    LoginModeNativeLayout.KnownRequestAttachments.SequenceEqual(
    [
        new LoginRequestAttachmentSnapshot("Login response", 0x63B550, 0x63B653,
            0x9D8, 2, "%s/api/v2/accounts/login", 0x9E0),
        new LoginRequestAttachmentSnapshot("Character refresh response", 0x63ECE0, 0x63EDBC,
            0x9E8, 1, "%s/api/v2/characters/list", 0x9F0),
        new LoginRequestAttachmentSnapshot("Zone-list response", 0x63E390, 0x63E56C,
            0xA00, 2, "%s/api/v1/server/list", null)
    ])
    && LoginModeNativeLayout.KnownRequestAttachments.All(request =>
        LoginModeNativeLayout.KnownResponseCallbacks.Any(callback =>
            callback.OwnerMemberOffset == request.OwnerCallbackPairOffset
            && callback.Purpose == request.Purpose))
    && LoginModeNativeLayout.RequestObjectConstructorFileOffset == 0x3843F0
    && LoginModeNativeLayout.RequestPreparationMethodFileOffset == 0x636550
    && LoginModeNativeLayout.RequestSubmissionWrapperFileOffset == 0x6499F0
    && LoginModeNativeLayout.RequestManagerSubmissionMethodFileOffset == 0xBC43C0);
Check("Zone-list request state switch is translated from code-section VAs",
    LoginModeNativeLayout.AutoLoginStateDispatchMethodFileOffset == 0x63E390
    && LoginModeNativeLayout.AutoLoginStateOffset == 0xA48
    && LoginModeNativeLayout.ZoneListRequestAutoLoginState == 2
    && LoginModeNativeLayout.AutoLoginStateJumpTableVirtualAddress == 0xA3F574
    && LoginModeNativeLayout.AutoLoginStateJumpTableFileOffset == 0x63E974
    && LoginModeNativeLayout.CharacterRefreshRequestInFlightFlagOffset == 0x9F0);
Check("Character-refresh outgoing dispatch uses a distinct runtime receiver global",
    LoginModeNativeLayout.CharacterRefreshRequestReceiverGlobalVirtualAddress == 0x1E002B8
    && LoginModeNativeLayout.CharacterRefreshRequestReceiverGlobalFileOffset == 0x19FE6B8);
Check("Shared request worker selects the verified WinHTTP verbs and default",
    new[] { 1, 2, 3, 4, 5 }.Select(ClientApiRequestNativeLayout.ResolveHttpMethod)
        .SequenceEqual(["GET", "POST", "PUT", "DELETE", "HEAD"])
    && new[] { int.MinValue, -1, 0, 6, int.MaxValue }
        .All(type => ClientApiRequestNativeLayout.ResolveHttpMethod(type) == "GET")
    && LoginModeNativeLayout.KnownRequestAttachments.Select(request => request.HttpMethod)
        .SequenceEqual(["POST", "GET", "POST"])
    && ClientApiRequestNativeLayout.AllocationSize == 0x78
    && ClientApiRequestNativeLayout.TypeOffset == 8
    && ClientApiRequestNativeLayout.WeakCallbackPointerOffset == 0x50
    && ClientApiRequestNativeLayout.WeakCallbackControlBlockOffset == 0x54
    && ClientApiRequestNativeLayout.MethodJumpTableVirtualAddress == 0xFC44C0
    && ClientApiRequestNativeLayout.MethodJumpTableFileOffset == 0xBC38C0
    && ClientApiRequestNativeLayout.WinHttpOpenRequestCallFileOffset == 0xBC2DB7);
Check("Request completion delivers the response through an acquired weak callback",
    ClientApiRequestNativeLayout.ManagerPumpMethodFileOffset == 0xBC4550
    && ClientApiRequestNativeLayout.RoutePumpMethodFileOffset == 0xBC3F80
    && ClientApiRequestNativeLayout.WorkerPumpMethodFileOffset == 0xBC2B30
    && ClientApiCallbackDeliveryNativeLayout.WorkerActiveCompletionPointerOffset == 0x1C
    && ClientApiCallbackDeliveryNativeLayout.CompletionRequestHandleOffset == 0
    && ClientApiCallbackDeliveryNativeLayout.RequiredCompletionRequestHandleValue == 0
    && ClientApiCallbackDeliveryNativeLayout.CompletionResponsePointerOffset == 0x18
    && ClientApiCallbackDeliveryNativeLayout.AcquireCallbackCallFileOffset == 0xBC37E2
    && ClientApiCallbackDeliveryNativeLayout.AcquireCallbackMethodFileOffset == 0xBC4A80
    && ClientApiCallbackDeliveryNativeLayout.TryAcquireStrongReferenceMethodFileOffset == 0x327730
    && ClientApiCallbackDeliveryNativeLayout.ControlBlockStrongCountOffset == 4
    && ClientApiCallbackDeliveryNativeLayout.ControlBlockWeakCountOffset == 8
    && ClientApiCallbackDeliveryNativeLayout.CallbackVirtualCallFileOffset == 0xBC37FC
    && ClientApiCallbackDeliveryNativeLayout.CallbackVirtualMethodOffset
        == LoginModeNativeLayout.ResponseCallbackDispatchMethodVtableOffset
    && ClientApiResponseNativeLayout.AllocationSize == 0x20
    && ClientApiResponseNativeLayout.RequestPointerOffset == 4
    && ClientApiResponseNativeLayout.ResultKindOffset == 8
    && ClientApiResponseNativeLayout.ResultKindInitialValue == 1
    && ClientApiResponseNativeLayout.StatusWordOffset == 0x0C
    && ClientApiResponseNativeLayout.MetadataTreeHeadOffset == 0x10
    && ClientApiResponseNativeLayout.MetadataTreeCountOffset == 0x14
    && ClientApiResponseNativeLayout.FirstTextOffset == 0x18
    && ClientApiResponseNativeLayout.SecondTextOffset == 0x1C);
Check("WinHTTP registered callback and handle-closing signaling are recorded",
    ClientApiWinHttpCompletionNativeLayout.CompletionAllocationSize == 0x1C
    && ClientApiWinHttpCompletionNativeLayout.CompletionRequestHandleOffset == 0
    && ClientApiWinHttpCompletionNativeLayout.CompletionCleanupFlagOffset == 0x11
    && ClientApiWinHttpCompletionNativeLayout.CompletionDispatchResultOffset == 0x14
    && ClientApiWinHttpCompletionNativeLayout.CompletionResponsePointerOffset == 0x18
    && ClientApiWinHttpCompletionNativeLayout.StatusCallbackFileOffset == 0xBC26E0
    && ClientApiWinHttpCompletionNativeLayout.StatusCallbackVirtualAddress == 0xFC32E0
    && ClientApiWinHttpCompletionNativeLayout.InternalStatusDispatcherFileOffset == 0xBC1DA0
    && ClientApiWinHttpCompletionNativeLayout.RegisterStatusCallbackCallFileOffset == 0xBC31E9
    && ClientApiWinHttpCompletionNativeLayout.RegisterStatusCallbackImportVirtualAddress == 0x1AC4D0C
    && ClientApiWinHttpCompletionNativeLayout.SendRequestCallFileOffset == 0xBC3213
    && ClientApiWinHttpCompletionNativeLayout.SendRequestContextPushFileOffset == 0xBC31F8
    && ClientApiWinHttpCompletionNativeLayout.WorkerTeardownMethodFileOffset == 0xBC2890
    && ClientApiWinHttpCompletionNativeLayout.MarkCallbackOwnedCleanupFileOffset == 0xBC28CB
    && ClientApiWinHttpCompletionNativeLayout.RouteDestructorFileOffset == 0xBC3B80
    && ClientApiWinHttpCompletionNativeLayout.WorkerAdditionalHandleOffset == 0x20
    && ClientApiWinHttpCompletionNativeLayout.CloseAdditionalHandleDuringTeardownFileOffset == 0xBC28D7
    && ClientApiWinHttpCompletionNativeLayout.ClearRequestHandleOnClosingFileOffset == 0xBC2728
    && ClientApiWinHttpCompletionNativeLayout.HandleClosingStatus == 0x800
    && ClientApiWinHttpCompletionNativeLayout.ReadCompleteStatus == 0x80000
    && ClientApiWinHttpCompletionNativeLayout.RequestErrorStatus == 0x200000);
Check("Handle closing supplies result 3 only when no result has been assigned",
    ClientApiWinHttpCompletionNativeLayout.ApplyHandleClosingResult(0) == 3
    && new[] { int.MinValue, -1, 1, 2, 3, 4, int.MaxValue }.All(result =>
        ClientApiWinHttpCompletionNativeLayout.ApplyHandleClosingResult(result) == result));
LoginZoneRecord Zone(uint id, ushort protocol = 0x4C5F, ushort players = 1) =>
    new(id, $"zone-{id}", protocol, "build", $"matrix-{id}", players);

var zoneRecords = new[] { Zone(1, 0), Zone(2), Zone(3, players: 0), Zone(4), Zone(5) };
Check("Zone decoder field names preserve the verified 24-byte layout",
    LoginZoneListNativeLayout.RecordDecoderFileOffset == 0x635BE0
    && LoginZoneListNativeLayout.RecordStride == 24
    && LoginZoneListNativeLayout.ZoneIdOffset == 0
    && LoginZoneListNativeLayout.ZoneNameOffset == 4
    && LoginZoneListNativeLayout.ProtocolVersionOffset == 8
    && LoginZoneListNativeLayout.BuildOffset == 12
    && LoginZoneListNativeLayout.MatrixUrlOffset == 16
    && LoginZoneListNativeLayout.PlayersOffset == 20
    && LoginModeNativeLayout.KnownResponseHandlers[2].JsonKeys.SequenceEqual(
        ["zone_list", "zone_id", "zone_name", "protocol_version", "build", "matrix_url", "players"]));
Check("Zone response is deferred below signed auto-login state 3",
    LoginZoneListSelection.Select(-1, "unchanged", zoneRecords, false, 0,
        _ => throw new InvalidOperationException("Deferred response must not match zones"))
        == new LoginZoneSelectionResult(LoginZoneSelectionOutcome.Deferred, -1, "unchanged", 0));
Check("Zone filtering preserves native swap-removal order and selects matrix_url",
    new[] { 0u, 1u, 2u, 3u }.Select(sample =>
        LoginZoneListSelection.Select(3, "random", zoneRecords, true, sample,
            _ => throw new InvalidOperationException("Random selection must not match zones")).DestinationText)
        .SequenceEqual(["matrix-5", "matrix-2", "matrix-4", "matrix-5"])
    && zoneRecords.Select(record => record.ZoneId).SequenceEqual([1u, 2u, 3u, 4u, 5u]));
Check("Named zone selection samples only matching eligible candidates",
    LoginZoneListSelection.Select(4, "filter", zoneRecords, false, 1, id => id >= 4)
        == new LoginZoneSelectionResult(LoginZoneSelectionOutcome.Selected, 5, "matrix-4", 2));
Check("Zone selection explicitly reports failure and clears destination",
    LoginZoneListSelection.Select(3, "filter", zoneRecords, false, 0, _ => false)
        == new LoginZoneSelectionResult(LoginZoneSelectionOutcome.NoMatchingCandidate, 4, "", 0)
    && LoginZoneListSelection.Select(3, "random", Array.Empty<LoginZoneRecord>(), true, 0, _ => true)
        == new LoginZoneSelectionResult(LoginZoneSelectionOutcome.NoMatchingCandidate, 4, "", 0));
Check("Zone response increments the native signed state with DWORD wraparound",
    LoginZoneListSelection.Select(int.MaxValue, "random", [Zone(9)], true, uint.MaxValue, _ => false)
        == new LoginZoneSelectionResult(LoginZoneSelectionOutcome.Selected, int.MinValue, "matrix-9", 1));
Check("Auto-login state 5 prepares selected character and requests matrix connection",
    LoginModeNativeLayout.ConnectSelectedCharacterAutoLoginState == 5
    && LoginModeNativeLayout.ConnectSelectedCharacterStateBranchFileOffset == 0x63E890
    && LoginModeNativeLayout.PrepareSelectedCharacterMethodFileOffset == 0x63EE30
    && LoginModeNativeLayout.RequestMatrixConnectionMethodFileOffset == 0x63F230
    && LoginModeNativeLayout.SelectedCharacterPropertiesOffsetInRecord == 0xC8
    && LoginModeNativeLayout.CharacterConnectionObjectMemberOffset == 0x1C
    && LoginModeNativeLayout.CharacterConnectionObjectAllocationSize == 0xBB0
    && LoginModeNativeLayout.CharacterConnectionConstructorCallSiteFileOffset == 0x63F0EE
    && LoginModeNativeLayout.CharacterConnectionStateOffset == 0x0C
    && LoginModeNativeLayout.CharacterConnectionRequiredStateUpperBoundExclusive == 2
    && LoginModeNativeLayout.MatrixConnectionRequestBoundaryFileOffset == 0x494CA0
    && LoginModeNativeLayout.KnownCharacterConnectionPropertyKeys
        .SequenceEqual(["name", "is_dev", "race", "gender"])
    && LoginModeNativeLayout.MatrixOverrideConfigSection == "Development"
    && LoginModeNativeLayout.MatrixOverrideConfigKey == "MatrixOverride");
Check("Lua System API bindings connect preload character setup to Matrix connection entry",
    LoginModeNativeLayout.SystemApiRegistrationRoutineFileOffset == 0x6429E0
    && LoginModeNativeLayout.KnownLuaSystemApiBindings.SequenceEqual(
    [
        new LoginSystemApiBindingSnapshot("SetupCharacter", 0x1B63184, 0x642DF7,
            0xA46A60, 0x645E60, 0x641950),
        new LoginSystemApiBindingSnapshot("StartInstanceConnection", 0x1B63194, 0x642E47,
            0xA46A70, 0x645E70, 0x641C50)
    ])
    && LoginModeNativeLayout.SetupCharacterPreparationCallSiteFileOffset == 0x641BF3
    && LoginModeNativeLayout.StartInstanceConnectionValidationCallSiteFileOffset == 0x641DD4
    && LoginModeNativeLayout.MatrixConnectionBeginCallSiteFileOffset == 0x63F57F
    && LoginModeNativeLayout.KnownCharacterConnectionPropertyKeys
        .SequenceEqual(["name", "is_dev", "race", "gender"]));
Check("Matrix destination source precedence and empty input are explicit",
    LoginMatrixDestinationSelection.Select("lua-url", "development-url", "login-url")
        == new LoginMatrixDestinationSelectionResult(
            "login-url", LoginMatrixDestinationSource.LoginSelectedMatrixUrl)
    && LoginMatrixDestinationSelection.Select("lua-url", "development-url", null)
        == new LoginMatrixDestinationSelectionResult(
            "development-url", LoginMatrixDestinationSource.DevelopmentMatrixOverride)
    && LoginMatrixDestinationSelection.Select("lua-url", null, "")
        == new LoginMatrixDestinationSelectionResult("lua-url", LoginMatrixDestinationSource.LuaArgument)
    && LoginMatrixDestinationSelection.Select(null, "", null)
        == new LoginMatrixDestinationSelectionResult("", LoginMatrixDestinationSource.None));
Check("tfLogin default update branch queues World mode {2,0} behind the zero guard",
    LoginModeNativeLayout.LoginUpdateMethodFileOffset == 0x6385C0
    && LoginModeNativeLayout.LoginUpdateSwitchDefaultTargetFileOffset == 0x638AB3
    && LoginModeNativeLayout.LoginUpdateGuardGlobalVa == 0x1E5CAA4
    && LoginModeNativeLayout.LoginUpdateGuardByteOffset == 0x40
    && LoginModeNativeLayout.LoginUpdateWorldModeGuardBranchFileOffset == 0x638AC6
    && LoginModeNativeLayout.LoginUpdateWorldModeQueueRequestBuildFileOffset == 0x638B89
    && LoginModeNativeLayout.LoginUpdateWorldModeQueueCallSiteFileOffset == 0x638BAF);
Check("Matrix connection requests oracle tickets through a distinct callback wrapper",
    MatrixConnectionNativeLayout.AllocationSize == 0xBB0
    && MatrixConnectionNativeLayout.PreferredMatrixTextOffset == 0xBA0
    && MatrixConnectionNativeLayout.PreferredMatrixTextInitializationFileOffset == 0x48F0F9
    && MatrixConnectionNativeLayout.OracleCallbackPairOffset == 0xB98
    && MatrixConnectionNativeLayout.OracleRequestInFlightFlagOffset == 0x1C
    && MatrixConnectionNativeLayout.OracleCallbackAllocationSize == 0x10
    && MatrixConnectionNativeLayout.OracleCallbackOwnerPointerOffset == 4
    && MatrixConnectionNativeLayout.OracleCallbackTargetPointerOffset == 8
    && MatrixConnectionNativeLayout.OracleCallbackOwnerAdjustmentOffset == 12
    && MatrixConnectionNativeLayout.OracleCallbackTargetVirtualAddress == 0x896060
    && MatrixConnectionNativeLayout.OracleCallbackTrampolineFileOffset == 0x496DF0
    && MatrixConnectionNativeLayout.QueueOracleTicketCallSiteFileOffset == 0x494D0B
    && MatrixConnectionNativeLayout.QueueOracleTicketMethodFileOffset == 0x494E30
    && MatrixConnectionNativeLayout.EmptyTextPredicateMethodFileOffset == 0x2FCD00
    && MatrixConnectionNativeLayout.ExistingConnectionResetMethodFileOffset == 0x490090
    && MatrixConnectionNativeLayout.CharacterMetadataApplyMethodFileOffset == 0x490390
    && MatrixConnectionNativeLayout.PreferredDestinationEmptyCheckCallSiteFileOffset == 0x494CF4
    && MatrixConnectionNativeLayout.OracleMatrixFieldEmptyCheckCallSiteFileOffset == 0x495257
    && MatrixConnectionNativeLayout.OracleCallbackAttachmentFileOffset == 0x49510C
    && MatrixConnectionNativeLayout.OracleRequestSubmissionFileOffset == 0x4953DF
    && MatrixConnectionNativeLayout.OracleResponseHandlerFileOffset == 0x495460
    && MatrixConnectionNativeLayout.OracleResponseParserFileOffset == 0x4956C0
    && MatrixConnectionNativeLayout.OpenMatrixConnectionMethodFileOffset == 0x495F20
    && MatrixConnectionNativeLayout.EndpointFormat == "%s/api/v1/oracle/ticket"
    && ClientApiRequestNativeLayout.ResolveHttpMethod(MatrixConnectionNativeLayout.OracleRequestType) == "POST"
    && MatrixConnectionNativeLayout.KnownResponseKeys.SequenceEqual(
        ["matrix_url", "ticket", "datacenter", "operator_override", "session_id", "hostname"]));
Check("UI manager singleton and global event routing boundaries are recorded",
    UiManagerNativeLayout.SingletonRegistrationStubFileOffset == 0x3EE8C0
    && UiManagerNativeLayout.SingletonInitializeMethodFileOffset == 0x3E44F0
    && UiManagerNativeLayout.SingletonListWalkerFileOffset == 0x12C3A80
    && UiManagerNativeLayout.ClientApplicationBootstrapFileOffset == 0x2EAE70
    && UiManagerNativeLayout.SingletonObjectVirtualAddress == 0x1E002A8
    && UiManagerNativeLayout.UiManagerGlobalPointerVirtualAddress == 0x1E002B8
    && UiManagerNativeLayout.UiManagerAllocationSize == 0xA10
    && UiManagerNativeLayout.UiManagerEventDispatchMethodFileOffset == 0xBF9D80
    && UiManagerNativeLayout.UiManagerEventNameToIdMethodFileOffset == 0xBF9AB0
    && UiManagerNativeLayout.UiManagerEventListenerRegisterMethodFileOffset == 0xBF9B40
    && UiManagerNativeLayout.UiManagerEventListenerUnregisterMethodFileOffset == 0xBF9BD0
    && UiManagerNativeLayout.UiManagerEventListenerDispatchFileOffset == 0xBF9C40
    && UiManagerNativeLayout.UiManagerEventListenerLookupFileOffset == 0xC0AA20
    && UiManagerNativeLayout.UiManagerEventCallbackRouteFileOffset == 0xC38400
    && UiManagerNativeLayout.UiManagerNamedSubscriptionFileOffset == 0xC35BB0
    && UiManagerNativeLayout.UiComponentRootXmlParserFileOffset == 0xC34F50
    && UiManagerNativeLayout.UiComponentEventsSectionParserFileOffset == 0xC35740
    && UiManagerNativeLayout.UiComponentEventsSectionParserCallSiteFileOffset == 0xC353B0
    && UiManagerNativeLayout.UiComponentEventNameToIdCallSiteFileOffset == 0xC358AF
    && UiManagerNativeLayout.UiComponentEventBindLookupCallSiteFileOffset == 0xC358FE
    && UiManagerNativeLayout.UiComponentEventListenerRegisterCallSiteFileOffset == 0xC35AD8
    && UiManagerNativeLayout.ComponentBindEventApiFileOffset == 0xC32460
    && UiManagerNativeLayout.ComponentBindEventNamedSubscriptionCallSiteFileOffset == 0xC32643
    && UiManagerNativeLayout.ComponentEventsSectionNameVirtualAddress == 0x1BBCC6C
    && UiManagerNativeLayout.ComponentEventElementNameVirtualAddress == 0x1BBCCDC
    && UiManagerNativeLayout.ComponentEventNameAttributeVirtualAddress == 0x1BBCD1C
    && UiManagerNativeLayout.ComponentEventBindAttributeVirtualAddress == 0x1BBCCD4
    && UiManagerNativeLayout.ComponentBindEventUsageVirtualAddress == 0x1BBC760
    && UiManagerNativeLayout.UiManagerEventDispatchVtableOffset == 0xAC
    && UiManagerNativeLayout.UiManagerEventRegistryOffset == 0x6F0
    && UiManagerNativeLayout.UiManagerEventNameMapOffset == 0x6E8
    && UiManagerNativeLayout.UiManagerEventNameArrayBeginOffset == 0x71C
    && UiManagerNativeLayout.UiManagerEventNameArrayEndOffset == 0x720
    && UiManagerNativeLayout.UiManagerEventNameArrayBeginInitialValue == 0
    && UiManagerNativeLayout.UiManagerEventNameArrayEndInitialValue == 0
    && UiManagerNativeLayout.UiManagerEventNameArrayBeginInitializationFileOffset == 0xBF7FF6
    && UiManagerNativeLayout.UiManagerEventNameArrayEndInitializationFileOffset == 0xBF8000
    && UiManagerNativeLayout.UiManagerEventReadyFlagOffset == 0x672
    && UiManagerNativeLayout.LoginModeMessage145GlobalLoadFileOffset == 0x63C1E2
    && UiManagerNativeLayout.LoginModeMessage145CallFileOffset == 0x63C1F5
    && UiManagerNativeLayout.LoginModeMessageId == 0x145);
Check("UI-set folder traversal and native file-enumeration imports are recorded",
    UiComponentLoadingNativeLayout.UiSetXmlLoaderFileOffset == 0x30AF50
    && UiComponentLoadingNativeLayout.FolderPathLoaderFileOffset == 0x30AB90
    && UiComponentLoadingNativeLayout.DirectoryEnumerationMethodFileOffset == 0x12B6A00
    && UiComponentLoadingNativeLayout.FolderSearchPatternVirtualAddress == 0x1C1548C
    && UiComponentLoadingNativeLayout.DirectoryEnumerationCallSiteFileOffset == 0x30AC07
    && UiComponentLoadingNativeLayout.FindFirstFileCallSiteFileOffset == 0x12B6AD0
    && UiComponentLoadingNativeLayout.FindNextFileFunctionPointerLoadFileOffset == 0x12B6AEA
    && UiComponentLoadingNativeLayout.FindNextFileCallSiteFileOffset == 0x12B6C06
    && UiComponentLoadingNativeLayout.FindCloseCallSiteFileOffset == 0x12B6C15
    && UiComponentLoadingNativeLayout.DirectoryAttributeTestFileOffset == 0x12B6B94
    && UiComponentLoadingNativeLayout.DirectoryToFileBranchFileOffset == 0x12B6BA9
    && UiComponentLoadingNativeLayout.DirectoryListAppendCallSiteFileOffset == 0x12B6BC5
    && UiComponentLoadingNativeLayout.FileListAppendCallSiteFileOffset == 0x12B6BEA
    && UiComponentLoadingNativeLayout.ChildDirectoryProcessCallSiteFileOffset == 0x30AC35
    && UiComponentLoadingNativeLayout.FileListLoopStartFileOffset == 0x30AC9C
    && UiComponentLoadingNativeLayout.FileLoadCallSiteFileOffset == 0x30AE85
    && UiComponentLoadingNativeLayout.FindFirstFileIatVirtualAddress == 0x1AC473C
    && UiComponentLoadingNativeLayout.FindNextFileIatVirtualAddress == 0x1AC4738
    && UiComponentLoadingNativeLayout.FindCloseIatVirtualAddress == 0x1AC4734
    && UiComponentLoadingNativeLayout.DirectoryAttributeMask == 0x10
    && UiComponentLoadingNativeLayout.ChildDirectoryProcessCallSiteFileOffset
        < UiComponentLoadingNativeLayout.FileListLoopStartFileOffset
    && UiComponentLoadingNativeLayout.UiSetFolderTraversalCallSites.SequenceEqual(
        [0x30B58Bu, 0x30B67Du]));
int retryDelay = 0;
var retryDelays = new List<int>();
for (int i = 0; i < 12; i++)
{
    retryDelay = MatrixConnectionNativeLayout.NextOracleRetryDelaySeconds(retryDelay);
    retryDelays.Add(retryDelay);
}
Check("Oracle retry delay follows the native recurrence and saturates at 300 seconds",
    retryDelays.SequenceEqual([2, 5, 10, 17, 28, 44, 68, 104, 158, 239, 300, 300]));
bool RejectsRetryDelay(int delay)
{
    try
    {
        MatrixConnectionNativeLayout.NextOracleRetryDelaySeconds(delay);
        return false;
    }
    catch (ArgumentOutOfRangeException)
    {
        return true;
    }
}
Check("Unobserved retry-delay inputs are explicitly rejected",
    RejectsRetryDelay(-1) && RejectsRetryDelay(301));
Check("Matrix transport is an overlapped UDP socket with distinct native state",
    MatrixTransportNativeLayout.AllocationSize == 0xDF8
    && MatrixTransportNativeLayout.OpenMethodFileOffset == 0xBD0960
    && MatrixTransportNativeLayout.NativeDatagramSocketType == 2
    && MatrixTransportNativeLayout.NativeUdpProtocol == 17
    && MatrixTransportNativeLayout.NativeOverlappedSocketFlag == 1
    && MatrixTransportNativeLayout.StateOffset == 0x108
    && MatrixTransportNativeLayout.SocketHandleOffset == 0x118
    && MatrixTransportNativeLayout.InitialReceiveBufferSize == 1400
    && MatrixTransportNativeLayout.InitialReceiveCallFileOffset == 0xBD1211
    && MatrixTransportNativeLayout.IoPendingError == 997
    && MatrixTransportNativeLayout.ResolveSocketAddressFamily(1) == 2
    && MatrixTransportNativeLayout.ResolveSocketAddressFamily(2) == 23);
Check("Failed local binds try five requested ports then two ephemeral ports",
    MatrixTransportNativeLayout.GetLocalBindPortAttempts(4500)
        .SequenceEqual(new ushort[] { 4500, 4501, 4502, 4503, 4504, 0, 0 })
    && MatrixTransportNativeLayout.GetLocalBindPortAttempts(0)
        .SequenceEqual(new ushort[] { 0, 0 }));
Check("Local port wrap remains zero rather than retrying port one",
    MatrixTransportNativeLayout.GetLocalBindPortAttempts(65535)
        .SequenceEqual(new ushort[] { 65535, 0, 0, 0, 0, 0, 0 })
    && MatrixTransportNativeLayout.GetLocalBindPortAttempts(65533)
        .SequenceEqual(new ushort[] { 65533, 65534, 65535, 0, 0, 0, 0 }));
bool invalidFamilyRejected = false;
try
{
    MatrixTransportNativeLayout.ResolveSocketAddressFamily(0);
}
catch (ArgumentOutOfRangeException)
{
    invalidFamilyRejected = true;
}
Check("An unobserved socket-family mode is explicitly rejected", invalidFamilyRejected);
var initialPacket = new MatrixInitialPacket(
    0x11, 0x0807060504030201, 0x11223344, 0xAABBCCDD, 0x5566, 0x77, 0x88,
    new byte[] { 0x41, 0x42 }, new byte[] { 0xDE, 0xAD });
Check("Initial matrix application packet preserves native field order and little endian widths",
    initialPacket.EncodeOpenSuccess().SequenceEqual(Convert.FromHexString(
        "1111AA07000000010203040506070844332211DDCCBBAA665577008841420000DEAD"))
    && initialPacket.EncodeOpenSuccess().Length == MatrixInitialPacket.FixedEncodedSize + 4);
var emptyInitialPacket = new MatrixInitialPacket(0, 0, 0, 0, 0, 0, 0,
    ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty);
Check("Empty initial packet is 30 bytes and reserves no wire length prefixes",
    emptyInitialPacket.EncodeOpenSuccess().Length == 30
    && emptyInitialPacket.EncodeOpenSuccess()[0] == 0x11
    && emptyInitialPacket.EncodeOpenSuccess()[2] == 0xAA
    && emptyInitialPacket.EncodeOpenSuccess()[3] == 0x07
    && emptyInitialPacket.EncodeOpenSuccess().AsSpan(4).IndexOfAnyExcept((byte)0) < 0);
Check("Native packet construction, encoding, destruction and transport submission are distinct",
    MatrixInitialPacket.ConstructorFileOffset == 0x338250
    && MatrixInitialPacket.DestructorFileOffset == 0x338120
    && MatrixInitialPacket.EncoderFileOffset == 0x349E60
    && MatrixInitialPacket.TransportSubmissionMethodFileOffset == 0xBD3B20);
var exactFragments = MatrixTransportFragmentPlan.Create(1, 200, 100);
Check("Exact transport segment multiples end in a full final fragment, not an empty one",
    exactFragments.Count == 2
    && exactFragments[0] == new MatrixTransportFragment(0x4868, 0, 100, true)
    && exactFragments[1] == new MatrixTransportFragment(0x4068, 100, 100, false));
var shortFinalFragments = MatrixTransportFragmentPlan.Create(2, 201, 100);
Check("Transport fragment headers pack channel, continuation and size including four header bytes",
    shortFinalFragments.SequenceEqual(new[]
    {
        new MatrixTransportFragment(0x8868, 0, 100, true),
        new MatrixTransportFragment(0x8868, 100, 100, true),
        new MatrixTransportFragment(0x8005, 200, 1, false)
    })
    && MatrixTransportFragmentPlan.MaximumQueuedFragmentSize == 1392
    && MatrixTransportFragmentPlan.MaximumPayloadPerFragment == 1388
    && MatrixTransportFragmentPlan.Create(1, 1388, 1388).Single()
        == new MatrixTransportFragment(0x4570, 0, 1388, false)
    && MatrixTransportFragmentPlan.Create(1, 0, 1388).Count == 0);
bool RejectsFragmentPlan(int channel, int length, int segmentSize)
{
    try
    {
        MatrixTransportFragmentPlan.Create(channel, length, segmentSize);
        return false;
    }
    catch (ArgumentOutOfRangeException)
    {
        return true;
    }
}
Check("Unsupported channels and fragment sizes cannot silently produce transport plans",
    RejectsFragmentPlan(0, 1, 100) && RejectsFragmentPlan(3, 1, 100)
    && RejectsFragmentPlan(1, -1, 100) && RejectsFragmentPlan(1, 1, 0)
    && RejectsFragmentPlan(1, 1, 1389));
uint transportRetry = 0;
var transportRetryIntervals = new List<uint>();
for (int i = 0; i < 9; i++)
{
    transportRetry = GameSocketReliableQueueNativeLayout.NextRetryIntervalMilliseconds(transportRetry);
    transportRetryIntervals.Add(transportRetry);
}
Check("Transport retry interval has its own bounded millisecond policy",
    transportRetryIntervals.SequenceEqual(new uint[] { 250, 375, 562, 843, 1264, 1896, 2500, 2500, 2500 })
    && GameSocketReliableQueueNativeLayout.NextRetryIntervalMilliseconds(249) == 373
    && GameSocketReliableQueueNativeLayout.MaximumOutstandingCount == 512
    && MatrixTransportFragmentPlan.DefaultMaximumSegmentSize == 1388);
Check("Transport send window uses unsigned 16-bit modular sequence distance",
    GameSocketReliableQueueNativeLayout.IsWithinSendWindow(0, 0)
    && GameSocketReliableQueueNativeLayout.IsWithinSendWindow(255, 0)
    && !GameSocketReliableQueueNativeLayout.IsWithinSendWindow(256, 0)
    && GameSocketReliableQueueNativeLayout.IsWithinSendWindow(0, 65535)
    && !GameSocketReliableQueueNativeLayout.IsWithinSendWindow(65535, 0));
bool invalidTransportIntervalRejected = false;
try
{
    GameSocketReliableQueueNativeLayout.NextRetryIntervalMilliseconds(2501);
}
catch (ArgumentOutOfRangeException)
{
    invalidTransportIntervalRejected = true;
}
Check("Unobserved transport retry intervals are explicitly rejected", invalidTransportIntervalRejected);
var fragmentBytes = new byte[] { 0xAA, 0x00, 0x55, 0xFF, 0xBB };
var wireFragment = MatrixTransportFragmentPlan.Create(1, 3, 100).Single() with { PayloadOffset = 1 };
Check("Datagram fragment header and sequence are big endian while application bytes stay in source order",
    MatrixDatagramFragmentEncoder.Encode(wireFragment, 0x1234, fragmentBytes)
        .SequenceEqual(Convert.FromHexString("400712340055FF")));
Check("Attempt markers transform only payload with the literal FF/AA/CC masks",
    MatrixDatagramFragmentEncoder.Encode(wireFragment, 0x1234, fragmentBytes, 1)
        .SequenceEqual(Convert.FromHexString("50071234FFAA00"))
    && MatrixDatagramFragmentEncoder.Encode(wireFragment, 0x1234, fragmentBytes, 2)
        .SequenceEqual(Convert.FromHexString("60071234AAFF55"))
    && MatrixDatagramFragmentEncoder.Encode(wireFragment, 0x1234, fragmentBytes, 3)
        .SequenceEqual(Convert.FromHexString("70071234CC9933"))
    && fragmentBytes.SequenceEqual(new byte[] { 0xAA, 0x00, 0x55, 0xFF, 0xBB }));
bool RejectsWireFragment(MatrixTransportFragment fragment, byte marker = 0)
{
    try
    {
        MatrixDatagramFragmentEncoder.Encode(fragment, 0, fragmentBytes, marker);
        return false;
    }
    catch (ArgumentException)
    {
        return true;
    }
}
Check("Malformed slices, contradictory headers and unsupported attempt markers are rejected",
    RejectsWireFragment(wireFragment with { PayloadOffset = -1 })
    && RejectsWireFragment(wireFragment with { PayloadOffset = 4 })
    && RejectsWireFragment(wireFragment with { InitialHeaderWord = 0x4008 })
    && RejectsWireFragment(wireFragment, 4));
Check("UDP completion port routes by socket key and independent receive/close virtual slots",
    NetworkIoCompletionNativeLayout.NetworkNativeTypeName == "nl::Network"
    && NetworkIoCompletionNativeLayout.PumpThreadNativeTypeName == "nl::Network::PumpThread"
    && MatrixTransportNativeLayout.NativeTypeName == "nl::GameSocket"
    && NetworkIoCompletionNativeLayout.CompletionPortHandleOffset == 0xDC
    && NetworkIoCompletionNativeLayout.EmbeddedPumpThreadOffset == 0x48
    && NetworkIoCompletionNativeLayout.DrainIterationLimit == 100
    && NetworkIoCompletionNativeLayout.IdleWaitMilliseconds == 500
    && NetworkIoCompletionNativeLayout.ResolveSuccessfulCompletionSlot(true) == 0x18
    && NetworkIoCompletionNativeLayout.ResolveSuccessfulCompletionSlot(false) == 0x1C);
Check("Game-socket receive callback uses retained OVERLAPPED and reposts the 1400-byte buffer",
    GameSocketReceiveNativeLayout.OperationAllocationSize == 0xA0
    && GameSocketReceiveNativeLayout.OverlappedOffset == 0
    && GameSocketReceiveNativeLayout.OverlappedSize == 0x14
    && GameSocketReceiveNativeLayout.RetainedOperationPointerOffset == 0xDF4
    && GameSocketReceiveNativeLayout.SenderSockaddrOffset == 0x18
    && GameSocketReceiveNativeLayout.SenderSockaddrLengthOffset == 0x98
    && GameSocketReceiveNativeLayout.SenderSockaddrCapacity == 128
    && GameSocketReceiveNativeLayout.ReceiveCompletionMethodFileOffset == 0xBD2E30
    && GameSocketReceiveNativeLayout.PacketReceiveEntryFileOffset == 0xBD3030
    && GameSocketReceiveNativeLayout.RepostReceiveCallFileOffset == 0xBD2FE2);
Check("UDP outer admission accepts exactly the observed 5..1400 byte range",
    GameSocketPacketAdmission.Classify(new byte[4], 0, true, 1)
        == GameSocketPacketAdmissionOutcome.InvalidLength
    && GameSocketPacketAdmission.Classify(new byte[5], 0, true, 1)
        == GameSocketPacketAdmissionOutcome.AcceptedZeroContext
    && GameSocketPacketAdmission.Classify(new byte[1400], 0, true, 1)
        == GameSocketPacketAdmissionOutcome.AcceptedZeroContext
    && GameSocketPacketAdmission.Classify(new byte[1401], 0, true, 1)
        == GameSocketPacketAdmissionOutcome.InvalidLength);
var admissionPacket = Convert.FromHexString("12345678AB");
Check("UDP outer admission reads a big-endian opaque context word and checks sender specificity/port",
    GameSocketPacketAdmission.Classify(admissionPacket, 0x12345678, true, 4500)
        == GameSocketPacketAdmissionOutcome.AcceptedNonzeroContext
    && GameSocketPacketAdmission.Classify(admissionPacket, 0x78563412, true, 4500)
        == GameSocketPacketAdmissionOutcome.MismatchedContextWord
    && GameSocketPacketAdmission.Classify(admissionPacket, 0x12345678, false, 4500)
        == GameSocketPacketAdmissionOutcome.InvalidSender
    && GameSocketPacketAdmission.Classify(admissionPacket, 0x12345678, true, 0)
        == GameSocketPacketAdmissionOutcome.InvalidSender);
Check("Zero-context handshake requests retain literal magic and exact network-order fields",
    GameSocketHandshake.EncodePoke(0x1234)
        .SequenceEqual(Convert.FromHexString("00000000504F4B4500041234"))
    && GameSocketHandshake.EncodeKiss(0x12345678, 0x9ABC)
        .SequenceEqual(Convert.FromHexString("000000004B495353123456789ABC")));
var heheBody = Convert.FromHexString("4845484512345678");
var huggBody = Convert.FromHexString("485547479ABC1194");
Check("Handshake reply decoding respects native stage and nonzero remote port",
    GameSocketHandshake.DecodeReplyBody(heheBody, 1)
        is GameSocketHandshakeReply.Hehe { ContextCandidate: 0x12345678 }
    && GameSocketHandshake.DecodeReplyBody(huggBody, 2)
        is GameSocketHandshakeReply.Hugg { SequenceSeed: 0x9ABC, RemotePort: 4500 }
    && GameSocketHandshake.DecodeReplyBody(heheBody, 2) is GameSocketHandshakeReply.Rejected
    && GameSocketHandshake.DecodeReplyBody(huggBody, 1) is GameSocketHandshakeReply.Rejected
    && GameSocketHandshake.DecodeReplyBody(Convert.FromHexString("4855474700010000"), 2)
        is GameSocketHandshakeReply.Rejected
    && GameSocketHandshake.DecodeReplyBody(heheBody.AsSpan(0, 7), 1)
        is GameSocketHandshakeReply.Rejected);
Check("Native handshake accepts trailing bytes and ABRT before stage-specific replies",
    GameSocketHandshake.DecodeReplyBody(Convert.FromHexString("4845484512345678FF"), 1)
        is GameSocketHandshakeReply.Hehe { ContextCandidate: 0x12345678 }
    && GameSocketHandshake.DecodeReplyBody(Convert.FromHexString("4142525403"), 99)
        is GameSocketHandshakeReply.Abort { Reason: 3, NativeErrorCode: 10 }
    && GameSocketHandshake.DecodeReplyBody(Convert.FromHexString("41425254FF"), 1)
        is GameSocketHandshakeReply.Abort { Reason: 255, NativeErrorCode: null }
    && GameSocketHandshake.DecodeReplyBody("ABRT"u8, 1)
        is GameSocketHandshakeReply.Rejected);
Check("Native logged socket states distinguish transport Connected from Login connection state",
    Enum.GetNames<NativeGameSocketState>().SequenceEqual(new[]
    {
        "NotOpened", "Initializing", "Transferring", "Connecting", "WaitingContact",
        "Connected", "DisconnectedNotify", "Disconnected", "Destroyed", "Closed"
    })
    && (int)NativeGameSocketState.Connected == 5
    && GameSocketStateAndControlCatalog.CanFinishConnecting(NativeGameSocketState.Connecting, 2)
    && !GameSocketStateAndControlCatalog.CanFinishConnecting(NativeGameSocketState.Connecting, 1)
    && !GameSocketStateAndControlCatalog.CanFinishConnecting(NativeGameSocketState.WaitingContact, 2));
var inspectedFragment = GameSocketChunkHeaderDecoder.Inspect(
    MatrixDatagramFragmentEncoder.Encode(wireFragment, 0x1234, fragmentBytes, 2));
Check("Chunk header inspection recovers network-order channel, marker and declared size",
    inspectedFragment.Outcome == GameSocketChunkHeaderOutcome.Accepted
    && inspectedFragment.Header == new GameSocketChunkHeader(1, 2, false, 7)
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("0002")).Outcome
        == GameSocketChunkHeaderOutcome.Accepted
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("4002")).Outcome
        == GameSocketChunkHeaderOutcome.Accepted);
Check("Chunk header errors preserve exact length and continuation gates",
    GameSocketChunkHeaderDecoder.Inspect(new byte[1]).Outcome
        == GameSocketChunkHeaderOutcome.TruncatedHeader
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("0001")).Outcome
        == GameSocketChunkHeaderOutcome.InvalidLength
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("0571")).Outcome
        == GameSocketChunkHeaderOutcome.InvalidLength
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("4007")).Outcome
        == GameSocketChunkHeaderOutcome.TruncatedChunk
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("C802")).Outcome
        == GameSocketChunkHeaderOutcome.InvalidContinuation
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("1002")).Outcome
        == GameSocketChunkHeaderOutcome.UnsupportedControlMarker);
Check("Header-only gate accepts 2/3-byte nonzero chunks despite the full sequence prefix being 4 bytes",
    GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("4002")).Outcome
        == GameSocketChunkHeaderOutcome.Accepted
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("4003AA")).Outcome
        == GameSocketChunkHeaderOutcome.Accepted
    && GameSocketQueuedReceiveNativeLayout.DataChannelSequencePrefixSize == 4);
Check("Context-ready tick transitions follow Initializing/Transferring without inventing timers",
    GameSocketTickNativeLayout.SelectReadyHandshakeState(NativeGameSocketState.Initializing, 1, 0, true)
        == NativeGameSocketState.Transferring
    && GameSocketTickNativeLayout.SelectReadyHandshakeState(NativeGameSocketState.Initializing, 0, 0, true) == null
    && GameSocketTickNativeLayout.SelectReadyHandshakeState(NativeGameSocketState.Transferring, 1, 1, true)
        == NativeGameSocketState.Connecting
    && GameSocketTickNativeLayout.SelectReadyHandshakeState(NativeGameSocketState.Transferring, 1, 1, false)
        == NativeGameSocketState.Connected
    && GameSocketTickNativeLayout.SelectReadyHandshakeState(NativeGameSocketState.Transferring, 1, 0, true) == null
    && GameSocketTickNativeLayout.SelectReadyHandshakeState(NativeGameSocketState.Connected, 1, 1, true) == null);
uint activeQueueRetry = GameSocketReliableQueueNativeLayout.InitialRetryIntervalMilliseconds;
var activeQueueRetryIntervals = new List<uint>();
for (int i = 0; i < 4; i++)
{
    activeQueueRetry = GameSocketReliableQueueNativeLayout.NextRetryIntervalMilliseconds(activeQueueRetry);
    activeQueueRetryIntervals.Add(activeQueueRetry);
}
Check("Activated reliable queues start retry interval at 1000 milliseconds",
    activeQueueRetryIntervals.SequenceEqual(new uint[] { 1500, 2250, 2500, 2500 })
    && GameSocketHandshake.HuggSequenceSeedOffset == 0x228);
Check("Connecting delivers only deferred control, not early application fragments",
    GameSocketQueuedReceiveNativeLayout.SelectRoute(NativeGameSocketState.Connecting, 0, false)
        == GameSocketReceivedFragmentRoute.DeferredControl
    && Enumerable.Range(1, 3).All(channel =>
        GameSocketQueuedReceiveNativeLayout.SelectRoute(NativeGameSocketState.Connecting, channel, false)
            == GameSocketReceivedFragmentRoute.ReleaseWithoutDelivery));
Check("Connected and Destroyed queued receives route all four channels unless suppressed",
    new[] { NativeGameSocketState.Connected, NativeGameSocketState.Destroyed }.All(state =>
        Enumerable.Range(0, 4).All(channel =>
            GameSocketQueuedReceiveNativeLayout.SelectRoute(state, channel, false) == channel switch
            {
                0 => GameSocketReceivedFragmentRoute.DeferredControl,
                1 => GameSocketReceivedFragmentRoute.ReliableChannel1,
                2 => GameSocketReceivedFragmentRoute.ReliableChannel2,
                _ => GameSocketReceivedFragmentRoute.UnreliableChannel3
            }
            && GameSocketQueuedReceiveNativeLayout.SelectRoute(state, channel, true)
                == GameSocketReceivedFragmentRoute.ReleaseWithoutDelivery)));
Check("Owner delivery separates channel 1 from channels 2/3 and preserves their flag",
    GameSocketQueuedReceiveNativeLayout.GetOwnerDelivery(1) == new GameSocketOwnerDelivery(4, null)
    && GameSocketQueuedReceiveNativeLayout.GetOwnerDelivery(2) == new GameSocketOwnerDelivery(8, 1)
    && GameSocketQueuedReceiveNativeLayout.GetOwnerDelivery(3) == new GameSocketOwnerDelivery(8, 0));
Check("Received-queue records and internal event tokens retain their observed native offsets",
    (int)GameSocketReceivedQueueEvent.CloseCompletion == 1
    && (int)GameSocketReceivedQueueEvent.PumpCloseRequest == 2
    && (int)GameSocketReceivedQueueEvent.SenderBindingSucceeded == 3
    && GameSocketQueuedReceiveNativeLayout.ParseNonzeroContextChunksMethodFileOffset == 0xBD3140
    && GameSocketQueuedReceiveNativeLayout.ReceivedQueueOffset == 0x260
    && GameSocketQueuedReceiveNativeLayout.QueueEventPointerThreshold == 0x10
    && GameSocketQueuedReceiveNativeLayout.FragmentHeaderWordOffset == 0
    && GameSocketQueuedReceiveNativeLayout.FragmentSequenceWordOffset == 2
    && GameSocketQueuedReceiveNativeLayout.FragmentBodySliceOffsetFieldOffset == 4
    && GameSocketQueuedReceiveNativeLayout.FragmentBodyObjectOffset == 8
    && GameSocketQueuedReceiveNativeLayout.FragmentArrivalTickOffset == 0x18
    && GameSocketQueuedReceiveNativeLayout.Channel0BodySliceOffset == 2
    && GameSocketQueuedReceiveNativeLayout.DataChannelBodySliceOffset == 4
    && GameSocketQueuedReceiveNativeLayout.Channel1QueueOffset == 0xD6C
    && GameSocketQueuedReceiveNativeLayout.Channel2QueueOffset == 0xD70
    && GameSocketQueuedReceiveNativeLayout.Channel3QueueOffset == 0xD74);
bool invalidHandshakeStateRejected = false;
bool invalidQueuedStateRejected = false;
bool invalidQueuedChannelRejected = false;
bool controlOwnerDeliveryRejected = false;
try { GameSocketTickNativeLayout.SelectReadyHandshakeState((NativeGameSocketState)10, 0, 0, true); }
catch (ArgumentOutOfRangeException) { invalidHandshakeStateRejected = true; }
try { GameSocketQueuedReceiveNativeLayout.SelectRoute(NativeGameSocketState.Initializing, 0, false); }
catch (ArgumentOutOfRangeException) { invalidQueuedStateRejected = true; }
try { GameSocketQueuedReceiveNativeLayout.SelectRoute(NativeGameSocketState.Connected, 4, false); }
catch (ArgumentOutOfRangeException) { invalidQueuedChannelRejected = true; }
try { GameSocketQueuedReceiveNativeLayout.GetOwnerDelivery(0); }
catch (ArgumentOutOfRangeException) { controlOwnerDeliveryRejected = true; }
Check("Invalid tick states and queued delivery domains fail explicitly",
    invalidHandshakeStateRejected && invalidQueuedStateRejected
    && invalidQueuedChannelRejected && controlOwnerDeliveryRejected);
Check("Matrix secondary receiver catalogs all 35 native descriptor IDs without guessing names",
    MatrixServerMessageCatalog.Entries.Count == 35
    && MatrixServerMessageCatalog.Get(34) == new MatrixServerMessageEntry(34, "msg-34", 4, 0x490AA0)
    && MatrixServerMessageCatalog.Get(35).HandlerFileOffset == MatrixMessage35NativeLayout.HandlerFileOffset
    && MatrixServerMessageCatalog.Get(68) == new MatrixServerMessageEntry(68, "msg-68", 0x8C, 0x4925F0));
Check("Message 35 advances connection state 3 to 4 with observed scalar writes, not World mode",
    MatrixMessage35NativeLayout.CaptureScalarUpdate(3, 0x11223344, 0xAABBCCDD, 123456, true, false, true)
        == new MatrixMessage35ObservedScalarUpdate(4, 0x11223344, 0xAABBCCDD, 123456, 5)
    && MatrixMessage35NativeLayout.ConnectionFirstWordOffset == 0x860
    && MatrixMessage35NativeLayout.ConnectionRuntimeFlagsOffset == 0x83D);
bool unknownMessageRejected = false;
bool wrongMessage35StateRejected = false;
try { MatrixServerMessageCatalog.Get(69); }
catch (ArgumentOutOfRangeException) { unknownMessageRejected = true; }
try { MatrixMessage35NativeLayout.CaptureScalarUpdate(4, 0, 0, 0, false, false, false); }
catch (ArgumentOutOfRangeException) { wrongMessage35StateRejected = true; }
Check("Unobserved matrix descriptor IDs and message-35 states fail explicitly",
    unknownMessageRejected && wrongMessage35StateRejected);
var enteredZone = new MatrixEnterZoneObservedFields(0x1234567804, 321, 0x1122334455667788, 0xA7);
Check("EnterZone advances connection state 4 to 6 and preserves identified scalar fields",
    MatrixEnterZoneNativeLayout.CaptureScalarUpdate(4, enteredZone, 1, new(false, false, false, false), false)
        == new MatrixEnterZoneObservedScalarUpdate(6, enteredZone, 1, 30)
    && MatrixServerMessageCatalog.Get(37).HandlerFileOffset == MatrixEnterZoneNativeLayout.HandlerFileOffset);
Check("Disabled preview clears low-three flags before independent force options set them",
    MatrixEnterZoneNativeLayout.CaptureScalarUpdate(4, enteredZone, 1, new(true, false, false, false), false)
        == new MatrixEnterZoneObservedScalarUpdate(6, enteredZone with { PreviewFlags = 0xA0 }, 1, 15)
    && MatrixEnterZoneNativeLayout.CaptureScalarUpdate(4, enteredZone, 1, new(true, true, true, false), true)
        == new MatrixEnterZoneObservedScalarUpdate(6, enteredZone with { PreviewFlags = 0xA5 }, 3, 30)
    && MatrixEnterZoneNativeLayout.CaptureScalarUpdate(4, enteredZone, 3, new(true, false, false, true), false)
        == new MatrixEnterZoneObservedScalarUpdate(6, enteredZone with { PreviewFlags = 0xA2 }, 3, 30));
bool RejectsEnterZone(int state, ulong instanceId)
{
    try
    {
        MatrixEnterZoneNativeLayout.CaptureScalarUpdate(state, enteredZone with { InstanceId = instanceId },
            0, new(false, false, false, false), false);
        return false;
    }
    catch (ArgumentOutOfRangeException) { return true; }
}
Check("EnterZone rejects wrong state, zero instance and sentinel regardless of low byte",
    RejectsEnterZone(3, 1) && RejectsEnterZone(4, 0)
    && RejectsEnterZone(4, 0xFFFFFFFFFFFFFF04) && RejectsEnterZone(4, ulong.MaxValue));
Check("Receive-window classification matches the native shifted-WORD gate across all 65536 distances",
    Enumerable.Range(0, 65536).All(distance =>
    {
        var result = GameSocketReliableReceiveWindow.Classify((ushort)distance, 0);
        bool nativeAcceptedBranch = unchecked((ushort)(distance - 256)) > 0x7EFF;
        var expected = distance < 256 ? GameSocketReceiveSequenceClass.WithinWindow
            : distance >= 0x8000 ? GameSocketReceiveSequenceClass.BeforeWindow
            : GameSocketReceiveSequenceClass.BeyondWindow;
        return result == expected
            && nativeAcceptedBranch == (result != GameSocketReceiveSequenceClass.BeyondWindow);
    }));
Check("Receive-ring indices wrap independently of the 16-bit packet sequence",
    GameSocketReliableReceiveWindow.GetRingSlot(0, 65535, 255) == 0
    && GameSocketReliableReceiveWindow.GetRingSlot(254, 65535, 1) == 0
    && GameSocketReliableReceiveWindow.GetRingSlot(255, 0, 0) == 255
    && GameSocketReliableReceiveWindow.Classify(65535, 0) == GameSocketReceiveSequenceClass.BeforeWindow
    && GameSocketReliableReceiveWindow.Classify(256, 0) == GameSocketReceiveSequenceClass.BeyondWindow);
Check("Reliable receive layout preserves fragment slices, continuation assembly, and decoded channel metadata",
    GameSocketReliableReceiveWindow.FragmentSequenceWordOffset == 2
    && GameSocketReliableReceiveWindow.FragmentBodySliceOffsetFieldOffset == 4
    && GameSocketReliableReceiveWindow.FragmentPayloadHeaderSize == 4
    && GameSocketReliableReceiveWindow.ContinuationFlagMask == 0x0800
    && GameSocketReliableReceiveWindow.ReassemblyBufferOffset == 0x594
    && GameSocketReliableReceiveWindow.ReassemblyLimitSocketOffset == 0x120
    && GameSocketReliableReceiveWindow.DecodedDeliveryQueueOffset == 0x60
    && GameSocketReliableQueueNativeLayout.ChannelOffset == 0x04
    && GameSocketReliableReceiveWindow.DecodedObjectChannelOffset == 0x20
    && GameSocketReliableReceiveWindow.DecodedObjectPayloadSizeOffset == 0x24
    && GameSocketReliableQueueNativeLayout.FailureFlagOffset == 0x1C
    && GameSocketReliableQueueNativeLayout.FailureCodeOffset == 0x20
    && GameSocketReliableQueueNativeLayout.AcknowledgmentPendingFlagOffset == 0x32);
bool invalidReceiveRingHeadRejected = false;
bool outOfWindowRingSlotRejected = false;
try { GameSocketReliableReceiveWindow.GetRingSlot(0, 0, 256); }
catch (ArgumentOutOfRangeException) { invalidReceiveRingHeadRejected = true; }
try { GameSocketReliableReceiveWindow.GetRingSlot(256, 0, 0); }
catch (ArgumentOutOfRangeException) { outOfWindowRingSlotRejected = true; }
Check("Receive-ring slot lookup rejects invalid head indices and out-of-window packets",
    invalidReceiveRingHeadRejected && outOfWindowRingSlotRejected);
Check("Matrix codec pairs map to the native GameSocket queue tag and decoder slots",
    MatrixChannelCodecNativeLayout.GetForChannel(1) == new MatrixChannelCodec(0xB968, 0x33E2E0, 0x73EEE0)
    && MatrixChannelCodecNativeLayout.GetForChannel(2) == new MatrixChannelCodec(0x4C5F, 0x821BA0, 0xC227A0)
    && MatrixChannelCodecNativeLayout.GetForChannel(2) == MatrixChannelCodecNativeLayout.GetForChannel(3)
    && MatrixChannelCodecNativeLayout.GameSocketOpenMethodFileOffset == 0xBD0960
    && MatrixChannelCodecNativeLayout.GameSocketOpenCallSiteFileOffset == 0x4961AD
    && MatrixChannelCodecNativeLayout.ReliableQueueConstructorFileOffset == 0xBE3CE0
    && MatrixChannelCodecNativeLayout.ConnectionChannel1TagOffset == 0x824
    && MatrixChannelCodecNativeLayout.ConnectionChannel1DecoderOffset == 0x828
    && MatrixChannelCodecNativeLayout.ConnectionOtherChannelsTagOffset == 0x82C
    && MatrixChannelCodecNativeLayout.ConnectionOtherChannelsDecoderOffset == 0x830
    && MatrixChannelCodecNativeLayout.GameSocketQueueTagOffset == 0x10
    && MatrixChannelCodecNativeLayout.GameSocketQueueDecoderOffset == 0x14
    && MatrixChannelCodecNativeLayout.GameSocketChannel1QueueOffset == 0xD6C
    && MatrixChannelCodecNativeLayout.GameSocketChannel2QueueOffset == 0xD70
    && MatrixChannelCodecNativeLayout.GameSocketChannel3QueueOffset == 0xD74);
Check("Secondary command dispatcher separates primary-byte, secondary-selector, and zero-return paths",
    MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(0)
        == MatrixSecondaryCommandInputPath.SecondarySelectorTable
    && MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(0xF0)
        == MatrixSecondaryCommandInputPath.SecondarySelectorTable
    && MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(1)
        == MatrixSecondaryCommandInputPath.PrimaryByteRouteMap
    && MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(0x35)
        == MatrixSecondaryCommandInputPath.PrimaryByteRouteMap
    && MatrixSecondaryCommandDispatcherNativeLayout.PrimaryByteMapMaximumLeadingByte == 0x36
    && MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(0x36)
        == MatrixSecondaryCommandInputPath.PrimaryByteRouteMap
    && MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(0x37)
        == MatrixSecondaryCommandInputPath.ReturnZero
    && MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(0xEF)
        == MatrixSecondaryCommandInputPath.ReturnZero
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0) is null
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(1) == 0
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x0E) == 0
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x0F) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x12) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x13) == 2
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x1A) == 3
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x1F) == 3
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x20) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x21) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x22) == 4
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x25) == 4
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x26) == 5
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x28) == 5
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x29) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x33) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x34) == 6
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x35) == 6
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x36) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x37) is null
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryTargetFileOffset(0) == 0x81CCAD
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryTargetFileOffset(1) == 0x8217D9
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryTargetFileOffset(6) == 0x81F34A
    && MatrixSecondaryCommandDispatcherNativeLayout.GetSecondarySelectorIndex(0x20) == 0
    && MatrixSecondaryCommandDispatcherNativeLayout.GetSecondarySelectorIndex(0x7A) == 0x5A
    && MatrixSecondaryCommandDispatcherNativeLayout.GetSecondarySelectorIndex(0x7B) is null);
Check("MarketRequestComplete selector 0x53 preserves its RTTI and native body field offsets",
    CharacterMarketRequestCompleteNativeLayout.InnerSelector == 0x53
    && CharacterMarketRequestCompleteNativeLayout.CaseFileOffset == 0x81CCC3
    && CharacterMarketRequestCompleteNativeLayout.AllocationSize == 0x48
    && CharacterMarketRequestCompleteNativeLayout.ConstructorFileOffset == 0x524C30
    && CharacterMarketRequestCompleteNativeLayout.BodyReaderFileOffset == 0x5F6B10
    && CharacterMarketRequestCompleteNativeLayout.VtableVirtualAddress == 0x1C67278
    && CharacterMarketRequestCompleteNativeLayout.FirstByteFieldOffset == 0x28
    && CharacterMarketRequestCompleteNativeLayout.RawQwordFieldOffset == 0x30
    && CharacterMarketRequestCompleteNativeLayout.SecondByteFieldOffset == 0x38
    && CharacterMarketRequestCompleteNativeLayout.FirstSharedTextFieldOffset == 0x3C
    && CharacterMarketRequestCompleteNativeLayout.SecondSharedTextFieldOffset == 0x40
    && CharacterMarketRequestCompleteNativeLayout.ThirdSharedTextFieldOffset == 0x44);
Check("ReceiveWeaponTweaks selector 0x54 preserves its RTTI and raw body field offsets",
    CharacterReceiveWeaponTweaksNativeLayout.InnerSelector == 0x54
    && CharacterReceiveWeaponTweaksNativeLayout.CaseFileOffset == 0x81CD14
    && CharacterReceiveWeaponTweaksNativeLayout.AllocationSize == 0x40
    && CharacterReceiveWeaponTweaksNativeLayout.ConstructorFileOffset == 0x524DF0
    && CharacterReceiveWeaponTweaksNativeLayout.BodyReaderFileOffset == 0x5F68D0
    && CharacterReceiveWeaponTweaksNativeLayout.VtableVirtualAddress == 0x1C67268
    && CharacterReceiveWeaponTweaksNativeLayout.FirstByteFieldOffset == 0x28
    && CharacterReceiveWeaponTweaksNativeLayout.SecondByteFieldOffset == 0x29
    && CharacterReceiveWeaponTweaksNativeLayout.TrailingBlobFieldOffset == 0x2C
    && CharacterReceiveWeaponTweaksNativeLayout.TrailingBlobVtableVirtualAddress == 0x1C80CC4
    && CharacterReceiveWeaponTweaksNativeLayout.TrailingBlobCopyMethodFileOffset == 0x12CB9D0
    && CharacterReceiveWeaponTweaksNativeLayout.TrailingBlobResizeMethodFileOffset == 0x12CBCB0);
Check("TookDebugWeaponHitPublic selector 0x55 records its scalar prefix and conditional-tail boundaries",
    CharacterTookDebugWeaponHitPublicNativeLayout.InnerSelector == 0x55
    && CharacterTookDebugWeaponHitPublicNativeLayout.CaseFileOffset == 0x81CD65
    && CharacterTookDebugWeaponHitPublicNativeLayout.CaseLocalDwordValue == 0x5D
    && CharacterTookDebugWeaponHitPublicNativeLayout.AllocationSize == 0xA8
    && CharacterTookDebugWeaponHitPublicNativeLayout.ConstructorFileOffset == 0x524EE0
    && CharacterTookDebugWeaponHitPublicNativeLayout.BodyReaderFileOffset == 0x5F60F0
    && CharacterTookDebugWeaponHitPublicNativeLayout.VtableVirtualAddress == 0x1C67258
    && CharacterTookDebugWeaponHitPublicNativeLayout.FirstByteFieldOffset == 0
    && CharacterTookDebugWeaponHitPublicNativeLayout.RawWordFieldOffset == 0x0C
    && CharacterTookDebugWeaponHitPublicNativeLayout.RawDwordFieldOffsets.SequenceEqual(
        new[] { 0x04, 0x08, 0x10, 0x14, 0x18, 0x1C, 0x20, 0x24, 0x28 })
    && CharacterTookDebugWeaponHitPublicNativeLayout.FixedPrefixByteCount == 0x27
    && CharacterTookDebugWeaponHitPublicNativeLayout.FirstConditionalTailMarkerOffset == 0x30
    && CharacterTookDebugWeaponHitPublicNativeLayout.FirstConditionalTailPayloadByteCount == 0x26
    && CharacterTookDebugWeaponHitPublicNativeLayout.FirstConditionalTailFourFloatGroupOffset == 0x4C
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailMarkerOffset == 0x60
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailCopiedPrefixOffset == 0x68
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailCopiedPrefixDwordCount == 3
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailVectorBeginOffset == 0x74
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailVectorEndOffset == 0x78
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailVectorCapacityOffset == 0x7C
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailRecordByteCount == 0x40
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailRecordDwordCount == 16
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailCountRepeatMarker == 0xFF
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailReaderByteCount == 0x121);
Check("TookDebugWeaponHit selector 0x56 has a distinct RTTI type and shares selector 0x55's body reader",
    CharacterTookDebugWeaponHitNativeLayout.InnerSelector == 0x56
    && CharacterTookDebugWeaponHitNativeLayout.CaseFileOffset == 0x81CDBA
    && CharacterTookDebugWeaponHitNativeLayout.CaseLocalDwordValue == 0x5E
    && CharacterTookDebugWeaponHitNativeLayout.AllocationSize == 0xA8
    && CharacterTookDebugWeaponHitNativeLayout.ConstructorFileOffset == 0x524FC0
    && CharacterTookDebugWeaponHitNativeLayout.ConstructorHelperFileOffset == 0x5A0550
    && CharacterTookDebugWeaponHitNativeLayout.BodyReaderFileOffset
        == CharacterTookDebugWeaponHitPublicNativeLayout.BodyReaderFileOffset
    && CharacterTookDebugWeaponHitNativeLayout.VtableVirtualAddress == 0x1C67248
    && CharacterTookDebugWeaponHitNativeLayout.SharedBodyReaderSetupFileOffset == 0x81CD94
    && CharacterTookDebugWeaponHitNativeLayout.ConstructorZeroedByteField1Offset == 0x58
    && CharacterTookDebugWeaponHitNativeLayout.ConstructorZeroedByteField2Offset == 0x88);
Check("DebugWeaponStats selector 0x57 records eight raw WORD fields",
    CharacterDebugWeaponStatsNativeLayout.InnerSelector == 0x57
    && CharacterDebugWeaponStatsNativeLayout.CaseFileOffset == 0x81CDE7
    && CharacterDebugWeaponStatsNativeLayout.CaseLocalDwordValue == 0x5F
    && CharacterDebugWeaponStatsNativeLayout.AllocationSize == 0x38
    && CharacterDebugWeaponStatsNativeLayout.ConstructorFileOffset == 0x5250A0
    && CharacterDebugWeaponStatsNativeLayout.BodyReaderFileOffset == 0x5F4A10
    && CharacterDebugWeaponStatsNativeLayout.VtableVirtualAddress == 0x1C67238
    && CharacterDebugWeaponStatsNativeLayout.FirstWordFieldOffset == 0x28
    && CharacterDebugWeaponStatsNativeLayout.WordFieldCount == 8
    && CharacterDebugWeaponStatsNativeLayout.WordFieldOffsets.SequenceEqual(
        new[] { 0x28, 0x2A, 0x2C, 0x2E, 0x30, 0x32, 0x34, 0x36 }));
Check("RewardInfo selector 0x58 records its byte, QWORD, and DWORD fields",
    CharacterRewardInfoNativeLayout.InnerSelector == 0x58
    && CharacterRewardInfoNativeLayout.CaseFileOffset == 0x81CE38
    && CharacterRewardInfoNativeLayout.CaseLocalDwordValue == 0x60
    && CharacterRewardInfoNativeLayout.AllocationSize == 0x48
    && CharacterRewardInfoNativeLayout.ConstructorFileOffset == 0x525120
    && CharacterRewardInfoNativeLayout.BodyReaderFileOffset == 0x5F47C0
    && CharacterRewardInfoNativeLayout.VtableVirtualAddress == 0x1C67218
    && CharacterRewardInfoNativeLayout.FirstByteFieldOffset == 0x28
    && CharacterRewardInfoNativeLayout.SecondByteFieldOffset == 0x29
    && CharacterRewardInfoNativeLayout.RawQwordFieldOffset == 0x30
    && CharacterRewardInfoNativeLayout.RawDwordFieldOffsets.SequenceEqual(new[] { 0x38, 0x3C, 0x40 })
    && CharacterRewardInfoNativeLayout.NominalInputByteCount == 0x16
    && CharacterRewardInfoNativeLayout.ReaderLastWrittenFieldEndOffset == 0x44
    && CharacterRewardInfoNativeLayout.ConstructorZeroedDwordField1Offset == 0x30
    && CharacterRewardInfoNativeLayout.ConstructorZeroedDwordField2Offset == 0x34);
Check("ProgressionXpRefresh selector 0x59 records its count-blocked 20-byte vector",
    CharacterProgressionXpRefreshNativeLayout.InnerSelector == 0x59
    && CharacterProgressionXpRefreshNativeLayout.CaseFileOffset == 0x81CE89
    && CharacterProgressionXpRefreshNativeLayout.CaseLocalDwordValue == 0x61
    && CharacterProgressionXpRefreshNativeLayout.AllocationSize == 0x38
    && CharacterProgressionXpRefreshNativeLayout.ConstructorFileOffset == 0x5251B0
    && CharacterProgressionXpRefreshNativeLayout.BodyReaderFileOffset == 0x5F4170
    && CharacterProgressionXpRefreshNativeLayout.RecordAppendFileOffset == 0x5200F0
    && CharacterProgressionXpRefreshNativeLayout.VtableVirtualAddress == 0x1C67228
    && CharacterProgressionXpRefreshNativeLayout.VectorBeginOffset == 0x28
    && CharacterProgressionXpRefreshNativeLayout.VectorEndOffset == 0x2C
    && CharacterProgressionXpRefreshNativeLayout.VectorCapacityOffset == 0x30
    && CharacterProgressionXpRefreshNativeLayout.RecordDwordCount == 5
    && CharacterProgressionXpRefreshNativeLayout.RecordByteCount == 0x14
    && CharacterProgressionXpRefreshNativeLayout.CountFieldByteCount == 1
    && CharacterProgressionXpRefreshNativeLayout.CountBlockRepeatMarker == 0xFF);
Check("ReceivedDeferredXP selector 0x5A records its count-blocked 16-byte vector",
    CharacterReceivedDeferredXPNativeLayout.InnerSelector == 0x5A
    && CharacterReceivedDeferredXPNativeLayout.CaseFileOffset == 0x81CEDC
    && CharacterReceivedDeferredXPNativeLayout.CaseLocalDwordValue == 0x62
    && CharacterReceivedDeferredXPNativeLayout.AllocationSize == 0x38
    && CharacterReceivedDeferredXPNativeLayout.ConstructorFileOffset == 0x5252A0
    && CharacterReceivedDeferredXPNativeLayout.BodyReaderFileOffset == 0x5F3A80
    && CharacterReceivedDeferredXPNativeLayout.VectorGrowthHelperFileOffset == 0x5F3880
    && CharacterReceivedDeferredXPNativeLayout.VtableVirtualAddress == 0x1C67208
    && CharacterReceivedDeferredXPNativeLayout.VectorBeginOffset == 0x28
    && CharacterReceivedDeferredXPNativeLayout.VectorEndOffset == 0x2C
    && CharacterReceivedDeferredXPNativeLayout.VectorCapacityOffset == 0x30
    && CharacterReceivedDeferredXPNativeLayout.RecordDwordCount == 4
    && CharacterReceivedDeferredXPNativeLayout.RecordByteCount == 0x10
    && CharacterReceivedDeferredXPNativeLayout.CountFieldByteCount == 1
    && CharacterReceivedDeferredXPNativeLayout.CountBlockRepeatMarker == 0xFF);
Check("PublicCombatLog selector 0x5B records its gated blob body without naming its fields",
    CharacterPublicCombatLogNativeLayout.InnerSelector == 0x5B
    && CharacterPublicCombatLogNativeLayout.CaseFileOffset == 0x81CF2F
    && CharacterPublicCombatLogNativeLayout.CaseLocalDwordValue == 0x63
    && CharacterPublicCombatLogNativeLayout.AllocationSize == 0x40
    && CharacterPublicCombatLogNativeLayout.ConstructorFileOffset == 0x525390
    && CharacterPublicCombatLogNativeLayout.BodyReaderFileOffset == 0x5F3350
    && CharacterPublicCombatLogNativeLayout.WordLengthPrefixedBlobReaderFileOffset == 0x12EE170
    && CharacterPublicCombatLogNativeLayout.VtableVirtualAddress == 0x1C671F8
    && CharacterPublicCombatLogNativeLayout.ConstructorZeroedByteFieldOffset == 0x28
    && CharacterPublicCombatLogNativeLayout.EmbeddedBlobFieldOffset == 0x2C
    && CharacterPublicCombatLogNativeLayout.EmbeddedBlobVtableVirtualAddress == 0x1C80CC4
    && CharacterPublicCombatLogNativeLayout.BodyReaderThisObjectOffset == 0x28
    && CharacterPublicCombatLogNativeLayout.BlobDestinationOffsetFromReaderThis == 4
    && CharacterPublicCombatLogNativeLayout.RequiredRawDwordValue == 1
    && CharacterPublicCombatLogNativeLayout.RawDwordWidth == 4
    && CharacterPublicCombatLogNativeLayout.BlobLengthPrefixByteCount == 2
    && CharacterPublicCombatLogNativeLayout.MinimumSuccessBodyPrefixByteCount == 6);
Check("PrivateCombatLog selector 0x5C has distinct RTTI and shares the public-log body reader",
    CharacterPrivateCombatLogNativeLayout.InnerSelector == 0x5C
    && CharacterPrivateCombatLogNativeLayout.CaseFileOffset == 0x81CF81
    && CharacterPrivateCombatLogNativeLayout.CaseLocalDwordValue == 0x64
    && CharacterPrivateCombatLogNativeLayout.AllocationSize == 0x40
    && CharacterPrivateCombatLogNativeLayout.ConstructorFileOffset == 0x525480
    && CharacterPrivateCombatLogNativeLayout.ConstructorHelperFileOffset == 0x5A0810
    && CharacterPrivateCombatLogNativeLayout.BodyReaderFileOffset
        == CharacterPublicCombatLogNativeLayout.BodyReaderFileOffset
    && CharacterPrivateCombatLogNativeLayout.SharedBodyReaderSetupFileOffset == 0x81CF5B
    && CharacterPrivateCombatLogNativeLayout.VtableVirtualAddress == 0x1C671E8
    && CharacterPrivateCombatLogNativeLayout.ConstructorZeroedByteFieldOffset == 0x28
    && CharacterPrivateCombatLogNativeLayout.EmbeddedBlobFieldOffset == 0x2C
    && CharacterPrivateCombatLogNativeLayout.EmbeddedBlobVtableVirtualAddress == 0x1C80CC4
    && CharacterPrivateCombatLogNativeLayout.BodyReaderThisObjectOffset == 0x28
    && CharacterPrivateCombatLogNativeLayout.BlobDestinationOffsetFromReaderThis == 4);
Check("AnimationUpdated selector 0x5D records its WORD and byte fields",
    CharacterAnimationUpdatedNativeLayout.InnerSelector == 0x5D
    && CharacterAnimationUpdatedNativeLayout.CaseFileOffset == 0x81CFAB
    && CharacterAnimationUpdatedNativeLayout.CaseLocalDwordValue == 0x65
    && CharacterAnimationUpdatedNativeLayout.AllocationSize == 0x30
    && CharacterAnimationUpdatedNativeLayout.ConstructorFileOffset == 0x525570
    && CharacterAnimationUpdatedNativeLayout.ConstructorHelperFileOffset == 0x5A0880
    && CharacterAnimationUpdatedNativeLayout.BodyReaderFileOffset == 0x5F30F0
    && CharacterAnimationUpdatedNativeLayout.VtableVirtualAddress == 0x1C671D8
    && CharacterAnimationUpdatedNativeLayout.RawWordFieldOffset == 0x28
    && CharacterAnimationUpdatedNativeLayout.RawByteFieldOffset == 0x2A
    && CharacterAnimationUpdatedNativeLayout.NominalInputByteCount == 3);
Check("RaiaNPCDebugging selector 0x5E records its NUL-terminated shared-text field",
    CharacterRaiaNpcDebuggingNativeLayout.InnerSelector == 0x5E
    && CharacterRaiaNpcDebuggingNativeLayout.CaseFileOffset == 0x81CFFC
    && CharacterRaiaNpcDebuggingNativeLayout.CaseLocalDwordValue == 0x66
    && CharacterRaiaNpcDebuggingNativeLayout.AllocationSize == 0x30
    && CharacterRaiaNpcDebuggingNativeLayout.ConstructorFileOffset == 0x5255F0
    && CharacterRaiaNpcDebuggingNativeLayout.ConstructorHelperFileOffset == 0x5A08F0
    && CharacterRaiaNpcDebuggingNativeLayout.SharedTextReaderSetupFileOffset == 0x81FB27
    && CharacterRaiaNpcDebuggingNativeLayout.SharedTextReaderFileOffset == 0x12EE010
    && CharacterRaiaNpcDebuggingNativeLayout.VtableVirtualAddress == 0x1C671B8
    && CharacterRaiaNpcDebuggingNativeLayout.SharedTextFieldOffset == 0x28
    && CharacterRaiaNpcDebuggingNativeLayout.SharedTextChunkByteLimit == 0x100
    && CharacterRaiaNpcDebuggingNativeLayout.SharedTextTemporaryBufferByteCount == 0x104
    && CharacterRaiaNpcDebuggingNativeLayout.SharedTextTerminatorByte == 0);
Check("WeaponProjectileFired selector 0x5F records its prefix and optional tail",
    CharacterWeaponProjectileFiredNativeLayout.InnerSelector == 0x5F
    && CharacterWeaponProjectileFiredNativeLayout.CaseFileOffset == 0x81D02D
    && CharacterWeaponProjectileFiredNativeLayout.CaseLocalDwordValue == 0x67
    && CharacterWeaponProjectileFiredNativeLayout.AllocationSize == 0x38
    && CharacterWeaponProjectileFiredNativeLayout.ConstructorFileOffset == 0x525700
    && CharacterWeaponProjectileFiredNativeLayout.ConstructorHelperFileOffset == 0x5A0960
    && CharacterWeaponProjectileFiredNativeLayout.BodyReaderFileOffset == 0x5F2990
    && CharacterWeaponProjectileFiredNativeLayout.OptionalTailReaderFileOffset == 0x5F27E0
    && CharacterWeaponProjectileFiredNativeLayout.WordReaderFileOffset == 0x34B740
    && CharacterWeaponProjectileFiredNativeLayout.VtableVirtualAddress == 0x1C671C8
    && CharacterWeaponProjectileFiredNativeLayout.PrefixWordFieldOffset == 0x28
    && CharacterWeaponProjectileFiredNativeLayout.PrefixWordCount == 4
    && CharacterWeaponProjectileFiredNativeLayout.OptionalTailInputGateOffset == 8
    && CharacterWeaponProjectileFiredNativeLayout.OptionalTailMarkerFieldOffset == 0x30
    && CharacterWeaponProjectileFiredNativeLayout.OptionalTailPayloadFieldOffset == 0x32
    && CharacterWeaponProjectileFiredNativeLayout.OptionalTailPayloadWordCount == 3
    && CharacterWeaponProjectileFiredNativeLayout.InputByteCountWithoutOptionalTail == 9
    && CharacterWeaponProjectileFiredNativeLayout.InputByteCountWithOptionalTail == 15);
Check("AbilityProjectileFired selector 0x60 records its fixed fields and optional QWORD",
    CharacterAbilityProjectileFiredNativeLayout.InnerSelector == 0x60
    && CharacterAbilityProjectileFiredNativeLayout.CaseFileOffset == 0x81D07E
    && CharacterAbilityProjectileFiredNativeLayout.CaseLocalDwordValue == 0x68
    && CharacterAbilityProjectileFiredNativeLayout.AllocationSize == 0x68
    && CharacterAbilityProjectileFiredNativeLayout.ConstructorFileOffset == 0x5257A0
    && CharacterAbilityProjectileFiredNativeLayout.BaseConstructorHelperFileOffset == 0x5A09F0
    && CharacterAbilityProjectileFiredNativeLayout.BaseConstructorArgument == 0x60
    && CharacterAbilityProjectileFiredNativeLayout.BodyReaderFileOffset == 0x5F22A0
    && CharacterAbilityProjectileFiredNativeLayout.OptionalQwordReaderFileOffset == 0x5FDA00
    && CharacterAbilityProjectileFiredNativeLayout.QwordReaderFileOffset == 0x356F80
    && CharacterAbilityProjectileFiredNativeLayout.VtableVirtualAddress == 0x1C671A8
    && CharacterAbilityProjectileFiredNativeLayout.WordField0Offset == 0x28
    && CharacterAbilityProjectileFiredNativeLayout.WordField1Offset == 0x2A
    && CharacterAbilityProjectileFiredNativeLayout.WordField2Offset == 0x2C
    && CharacterAbilityProjectileFiredNativeLayout.WordField3Offset == 0x2E
    && CharacterAbilityProjectileFiredNativeLayout.WordField4Offset == 0x30
    && CharacterAbilityProjectileFiredNativeLayout.WordField5Offset == 0x32
    && CharacterAbilityProjectileFiredNativeLayout.WordField6Offset == 0x34
    && CharacterAbilityProjectileFiredNativeLayout.WordField7Offset == 0x36
    && CharacterAbilityProjectileFiredNativeLayout.DwordField0Offset == 0x38
    && CharacterAbilityProjectileFiredNativeLayout.DwordField1Offset == 0x3C
    && CharacterAbilityProjectileFiredNativeLayout.ByteField0Offset == 0x40
    && CharacterAbilityProjectileFiredNativeLayout.DwordField2Offset == 0x44
    && CharacterAbilityProjectileFiredNativeLayout.ByteField1Offset == 0x48
    && CharacterAbilityProjectileFiredNativeLayout.DwordField3Offset == 0x4C
    && CharacterAbilityProjectileFiredNativeLayout.DwordField4Offset == 0x50
    && CharacterAbilityProjectileFiredNativeLayout.OptionalQwordPresenceFieldOffset == 0x58
    && CharacterAbilityProjectileFiredNativeLayout.OptionalQwordFieldOffset == 0x60
    && CharacterAbilityProjectileFiredNativeLayout.OptionalQwordInputGateOffset == 0x26
    && CharacterAbilityProjectileFiredNativeLayout.OptionalQwordByteCount == 8
    && CharacterAbilityProjectileFiredNativeLayout.InputByteCountWithoutOptionalQword == 0x27
    && CharacterAbilityProjectileFiredNativeLayout.InputByteCountWithOptionalQword == 0x2F);
Check("ProjectileHitReported selector 0x61 records two WORDs and two bytes",
    CharacterProjectileHitReportedNativeLayout.InnerSelector == 0x61
    && CharacterProjectileHitReportedNativeLayout.CaseFileOffset == 0x81D0CF
    && CharacterProjectileHitReportedNativeLayout.CaseLocalDwordValue == 0x69
    && CharacterProjectileHitReportedNativeLayout.AllocationSize == 0x30
    && CharacterProjectileHitReportedNativeLayout.ConstructorFileOffset == 0x525840
    && CharacterProjectileHitReportedNativeLayout.BaseConstructorHelperFileOffset == 0x5A0A60
    && CharacterProjectileHitReportedNativeLayout.BaseConstructorArgument == 0x61
    && CharacterProjectileHitReportedNativeLayout.BodyReaderFileOffset == 0x5F20E0
    && CharacterProjectileHitReportedNativeLayout.VtableVirtualAddress == 0x1C67198
    && CharacterProjectileHitReportedNativeLayout.WordField0Offset == 0x28
    && CharacterProjectileHitReportedNativeLayout.WordField1Offset == 0x2A
    && CharacterProjectileHitReportedNativeLayout.ByteField0Offset == 0x2C
    && CharacterProjectileHitReportedNativeLayout.ByteField1Offset == 0x2D
    && CharacterProjectileHitReportedNativeLayout.NominalInputByteCount == 6);
Check("Stumble selector 0x62 records two WORDs and a byte",
    CharacterStumbleNativeLayout.InnerSelector == 0x62
    && CharacterStumbleNativeLayout.CaseFileOffset == 0x81D120
    && CharacterStumbleNativeLayout.CaseLocalDwordValue == 0x6A
    && CharacterStumbleNativeLayout.AllocationSize == 0x30
    && CharacterStumbleNativeLayout.ConstructorFileOffset == 0x5258C0
    && CharacterStumbleNativeLayout.BaseConstructorHelperFileOffset == 0x5A0AD0
    && CharacterStumbleNativeLayout.BaseConstructorArgument == 0x62
    && CharacterStumbleNativeLayout.BodyReaderFileOffset == 0x5F1ED0
    && CharacterStumbleNativeLayout.VtableVirtualAddress == 0x1C67188
    && CharacterStumbleNativeLayout.WordField0Offset == 0x28
    && CharacterStumbleNativeLayout.WordField1Offset == 0x2A
    && CharacterStumbleNativeLayout.ByteFieldOffset == 0x2C
    && CharacterStumbleNativeLayout.NominalInputByteCount == 5);
Check("QuickChat selector 0x63 records its DWORD field and shared reader continuation",
    CharacterQuickChatNativeLayout.InnerSelector == 0x63
    && CharacterQuickChatNativeLayout.CaseFileOffset == 0x81D171
    && CharacterQuickChatNativeLayout.CaseLocalDwordValue == 0x6B
    && CharacterQuickChatNativeLayout.AllocationSize == 0x30
    && CharacterQuickChatNativeLayout.ConstructorFileOffset == 0x525940
    && CharacterQuickChatNativeLayout.BaseConstructorHelperFileOffset == 0x5A0B40
    && CharacterQuickChatNativeLayout.BaseConstructorArgument == 0x63
    && CharacterQuickChatNativeLayout.AllocationFailureContinuationFileOffset == 0x8206E1
    && CharacterQuickChatNativeLayout.BodyReaderContinuationFileOffset == 0x8206E3
    && CharacterQuickChatNativeLayout.DwordReaderFileOffset == 0x3370A0
    && CharacterQuickChatNativeLayout.VtableVirtualAddress == 0x1C67178
    && CharacterQuickChatNativeLayout.DwordFieldOffset == 0x28
    && CharacterQuickChatNativeLayout.NominalInputByteCount == 4);
Check("ProximityTextChat selector 0x64 records shared text and optional blob framing",
    CharacterProximityTextChatNativeLayout.InnerSelector == 0x64
    && CharacterProximityTextChatNativeLayout.CaseFileOffset == 0x81D1A2
    && CharacterProximityTextChatNativeLayout.CaseLocalDwordValue == 0x6C
    && CharacterProximityTextChatNativeLayout.AllocationSize == 0x48
    && CharacterProximityTextChatNativeLayout.ConstructorFileOffset == 0x5259C0
    && CharacterProximityTextChatNativeLayout.BaseConstructorHelperFileOffset == 0x5A0BB0
    && CharacterProximityTextChatNativeLayout.BaseConstructorArgument == 0x64
    && CharacterProximityTextChatNativeLayout.BodyReaderFileOffset == 0x5F1BC0
    && CharacterProximityTextChatNativeLayout.SharedTextReaderFileOffset == 0x12EE010
    && CharacterProximityTextChatNativeLayout.OptionalBlobReaderFileOffset == 0x5F19A0
    && CharacterProximityTextChatNativeLayout.WordLengthPrefixedBlobReaderFileOffset == 0x12EE170
    && CharacterProximityTextChatNativeLayout.OptionalBlobUpdateHelperFileOffset == 0x5F1920
    && CharacterProximityTextChatNativeLayout.OptionalBlobClearHelperFileOffset == 0x49AC30
    && CharacterProximityTextChatNativeLayout.VtableVirtualAddress == 0x1C67158
    && CharacterProximityTextChatNativeLayout.SharedTextFieldOffset == 0x28
    && CharacterProximityTextChatNativeLayout.SharedTextTerminatorByte == 0
    && CharacterProximityTextChatNativeLayout.SharedTextChunkByteLimit == 0x100
    && CharacterProximityTextChatNativeLayout.SharedTextTemporaryBufferByteCount == 0x104
    && CharacterProximityTextChatNativeLayout.ByteField0Offset == 0x2C
    && CharacterProximityTextChatNativeLayout.ByteField1Offset == 0x2D
    && CharacterProximityTextChatNativeLayout.ByteField2Offset == 0x2E
    && CharacterProximityTextChatNativeLayout.OptionalBlobPresenceFieldOffset == 0x30
    && CharacterProximityTextChatNativeLayout.FixedByteCountAfterSharedTextBeforeBlobGate == 3
    && CharacterProximityTextChatNativeLayout.OptionalBlobGateByteCount == 1
    && CharacterProximityTextChatNativeLayout.OptionalBlobLengthPrefixByteCount == 2
    && CharacterProximityTextChatNativeLayout.MaximumOptionalBlobPayloadByteCount == 0xFFFF
    && CharacterProximityTextChatNativeLayout.EmptyTextInputByteCountWithoutOptionalBlob == 5
    && CharacterProximityTextChatNativeLayout.EmptyTextInputByteCountWithEmptyOptionalBlob == 7);
Check("JumpActioned selector 0x65 records its shared WORD reader continuation",
    CharacterJumpActionedNativeLayout.InnerSelector == 0x65
    && CharacterJumpActionedNativeLayout.CaseFileOffset == 0x81D1F3
    && CharacterJumpActionedNativeLayout.CaseLocalDwordValue == 0x6D
    && CharacterJumpActionedNativeLayout.AllocationSize == 0x30
    && CharacterJumpActionedNativeLayout.ConstructorFileOffset == 0x525AF0
    && CharacterJumpActionedNativeLayout.BaseConstructorHelperFileOffset == 0x5A0C20
    && CharacterJumpActionedNativeLayout.BaseConstructorArgument == 0x65
    && CharacterJumpActionedNativeLayout.AllocationFailureContinuationFileOffset == 0x81D21D
    && CharacterJumpActionedNativeLayout.BodyReaderContinuationFileOffset == 0x81D21F
    && CharacterJumpActionedNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterJumpActionedNativeLayout.VtableVirtualAddress == 0x1C67168
    && CharacterJumpActionedNativeLayout.WordFieldOffset == 0x28
    && CharacterJumpActionedNativeLayout.NominalInputByteCount == 2);
Check("JumpRolled selector 0x66 records its shared WORD reader continuation",
    CharacterJumpRolledNativeLayout.InnerSelector == 0x66
    && CharacterJumpRolledNativeLayout.CaseFileOffset == 0x81D247
    && CharacterJumpRolledNativeLayout.CaseLocalDwordValue == 0x6E
    && CharacterJumpRolledNativeLayout.AllocationSize == 0x30
    && CharacterJumpRolledNativeLayout.ConstructorFileOffset == 0x525B70
    && CharacterJumpRolledNativeLayout.BaseConstructorHelperFileOffset == 0x5A0C90
    && CharacterJumpRolledNativeLayout.BaseConstructorArgument == 0x66
    && CharacterJumpRolledNativeLayout.AllocationFailureContinuationFileOffset == 0x81D21D
    && CharacterJumpRolledNativeLayout.BodyReaderContinuationFileOffset == 0x81D21F
    && CharacterJumpRolledNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterJumpRolledNativeLayout.VtableVirtualAddress == 0x1C67148
    && CharacterJumpRolledNativeLayout.WordFieldOffset == 0x28
    && CharacterJumpRolledNativeLayout.NominalInputByteCount == 2);
Check("CalledForHelp selector 0x68 records its shared WORD reader continuation",
    CharacterCalledForHelpNativeLayout.InnerSelector == 0x68
    && CharacterCalledForHelpNativeLayout.CaseFileOffset == 0x81D2C2
    && CharacterCalledForHelpNativeLayout.CaseLocalDwordValue == 0x70
    && CharacterCalledForHelpNativeLayout.AllocationSize == 0x30
    && CharacterCalledForHelpNativeLayout.ConstructorFileOffset == 0x525C70
    && CharacterCalledForHelpNativeLayout.BaseConstructorHelperFileOffset == 0x5A0D70
    && CharacterCalledForHelpNativeLayout.BaseConstructorArgument == 0x68
    && CharacterCalledForHelpNativeLayout.AllocationFailureContinuationFileOffset == 0x81D21D
    && CharacterCalledForHelpNativeLayout.BodyReaderContinuationFileOffset == 0x81D21F
    && CharacterCalledForHelpNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterCalledForHelpNativeLayout.VtableVirtualAddress == 0x1C67118
    && CharacterCalledForHelpNativeLayout.WordFieldOffset == 0x28
    && CharacterCalledForHelpNativeLayout.NominalInputByteCount == 2);
Check("Shared channel-2/3 compact-code routing groups the observed native cases",
    Enumerable.Range(1, 6).All(code =>
        MatrixOtherChannelDecoderNativeLayout.SelectRoute(code)
            == MatrixOtherChannelDecoderRoute.RoutedMessage)
    && MatrixOtherChannelDecoderNativeLayout.SelectRoute(7)
        == MatrixOtherChannelDecoderRoute.SecondaryDispatcher
    && MatrixOtherChannelDecoderNativeLayout.SelectRoute(8)
        == MatrixOtherChannelDecoderRoute.RoutedMultipleMessage
    && MatrixOtherChannelDecoderNativeLayout.SelectRoute(9)
        == MatrixOtherChannelDecoderRoute.RoutedMessage
    && MatrixOtherChannelDecoderNativeLayout.SelectRoute(0)
        == MatrixOtherChannelDecoderRoute.SecondaryDispatcher
    && MatrixOtherChannelDecoderNativeLayout.SelectRoute(10)
        == MatrixOtherChannelDecoderRoute.SecondaryDispatcher
    && MatrixOtherChannelDecoderNativeLayout.RoutedMessageSerializeMethodFileOffset == 0xBCB070
    && MatrixOtherChannelDecoderNativeLayout.RoutedMultipleMessageSerializeMethodFileOffset == 0xBCB120
    && MatrixOtherChannelDecoderNativeLayout.RoutedMessageBlobSliceOffsetFieldOffset == 0x38
    && MatrixOtherChannelDecoderNativeLayout.RoutedMessageBlobSliceLengthFieldOffset == 0x3C
    && MatrixOtherChannelDecoderNativeLayout.RoutedMultipleMessageBlobSliceOffsetFieldOffset == 0x38
    && MatrixOtherChannelDecoderNativeLayout.RoutedMultipleMessageBlobSliceLengthFieldOffset == 0x3A);
bool controlCodecRejected = false;
try { MatrixChannelCodecNativeLayout.GetForChannel(0); }
catch (ArgumentOutOfRangeException) { controlCodecRejected = true; }
Check("Control channel cannot silently select an application codec", controlCodecRejected);
Check("Compact positive integers preserve short and zero-escaped little-endian WORD boundaries",
    CompactPositiveUInt16Codec.Encode(1).SequenceEqual(Convert.FromHexString("01"))
    && CompactPositiveUInt16Codec.Encode(255).SequenceEqual(Convert.FromHexString("FF"))
    && CompactPositiveUInt16Codec.Encode(256).SequenceEqual(Convert.FromHexString("000001"))
    && CompactPositiveUInt16Codec.Encode(65535).SequenceEqual(Convert.FromHexString("00FFFF")));
Check("Compact positive integer encoding round-trips every supported value",
    Enumerable.Range(1, ushort.MaxValue).All(value =>
    {
        byte[] bytes = CompactPositiveUInt16Codec.Encode((uint)value);
        return CompactPositiveUInt16Codec.Decode(bytes)
            == new CompactPositiveUInt16Result(CompactPositiveUInt16Outcome.Accepted, (uint)value, bytes.Length);
    }));
Check("Compact decoder reports truncated and noncanonical escapes, and consumes only the header",
    CompactPositiveUInt16Codec.Decode([]).Outcome == CompactPositiveUInt16Outcome.Truncated
    && CompactPositiveUInt16Codec.Decode(Convert.FromHexString("00")).Outcome == CompactPositiveUInt16Outcome.Truncated
    && CompactPositiveUInt16Codec.Decode(Convert.FromHexString("00FF")).Outcome == CompactPositiveUInt16Outcome.Truncated
    && new[] { "000000", "000100", "00FF00" }.All(hex =>
        CompactPositiveUInt16Codec.Decode(Convert.FromHexString(hex))
            == new CompactPositiveUInt16Result(CompactPositiveUInt16Outcome.NonCanonicalExtendedValue, null, 0))
    && CompactPositiveUInt16Codec.Decode(Convert.FromHexString("23DEAD"))
        == new CompactPositiveUInt16Result(CompactPositiveUInt16Outcome.Accepted, 35, 1));
bool RejectsCompactValue(uint value)
{
    try { CompactPositiveUInt16Codec.Encode(value); return false; }
    catch (ArgumentOutOfRangeException) { return true; }
}
Check("Compact encoder rejects zero and values wider than 16 bits",
    RejectsCompactValue(0) && RejectsCompactValue(65536) && RejectsCompactValue(uint.MaxValue));
var message35Bytes = Convert.FromHexString("2308070605040302010200AABB0100CC");
var message35 = MatrixMessage35Wire.Decode(message35Bytes);
Check("Message 35 reads a raw little-endian QWORD and two WORD-length-prefixed blobs",
    message35.Outcome == MatrixMessage35ReadOutcome.Accepted
    && message35.BytesConsumed == 16
    && message35.Payload is { ValueAt28: 0x0102030405060708 } message35Payload
    && message35Payload.BlobAt30.Span.SequenceEqual(Convert.FromHexString("AABB"))
    && message35Payload.BlobAt40.Span.SequenceEqual(Convert.FromHexString("CC")));
var emptyMessage35 = new byte[MatrixMessage35Wire.MinimumPacketSize];
emptyMessage35[0] = 35;
var emptyMessage35Result = MatrixMessage35Wire.Decode(emptyMessage35);
Check("Message 35 accepts zero opaque value and empty blobs in a freshly created descriptor",
    emptyMessage35Result.Outcome == MatrixMessage35ReadOutcome.Accepted
    && emptyMessage35Result.BytesConsumed == 13
    && emptyMessage35Result.Payload is { ValueAt28: 0 } emptyMessage35Payload
    && emptyMessage35Payload.BlobAt30.IsEmpty && emptyMessage35Payload.BlobAt40.IsEmpty);
Check("Every truncated msg-35 prefix reports failure without a partial success object",
    Enumerable.Range(0, message35Bytes.Length).All(length =>
    {
        var decoded = MatrixMessage35Wire.Decode(message35Bytes.AsSpan(0, length));
        return decoded.Outcome != MatrixMessage35ReadOutcome.Accepted
            && decoded.Payload == null && decoded.BytesConsumed == 0;
    }));
Check("Message 35 distinguishes invalid headers and leaves trailing bytes unconsumed",
    MatrixMessage35Wire.Decode(Convert.FromHexString("002300")).Outcome == MatrixMessage35ReadOutcome.NonCanonicalHeader
    && MatrixMessage35Wire.Decode(Convert.FromHexString("22")).Outcome == MatrixMessage35ReadOutcome.UnexpectedMessageCode
    && MatrixMessage35Wire.Decode(message35Bytes.Concat(new byte[] { 0xDE, 0xAD }).ToArray()).BytesConsumed == 16);
var maximumMessage35Blob = new byte[MatrixMessage35Wire.MinimumPacketSize + ushort.MaxValue];
maximumMessage35Blob[0] = 35;
maximumMessage35Blob[9] = 0xFF;
maximumMessage35Blob[10] = 0xFF;
var maximumMessage35BlobResult = MatrixMessage35Wire.Decode(maximumMessage35Blob);
Check("Message 35 accepts the exact 65535-byte blob limit and rejects a one-byte truncation",
    maximumMessage35BlobResult.Outcome == MatrixMessage35ReadOutcome.Accepted
    && maximumMessage35BlobResult.Payload?.BlobAt30.Length == ushort.MaxValue
    && maximumMessage35BlobResult.BytesConsumed == maximumMessage35Blob.Length
    && MatrixMessage35Wire.Decode(maximumMessage35Blob.AsSpan(0, maximumMessage35Blob.Length - 1)).Outcome
        == MatrixMessage35ReadOutcome.TruncatedPayload);
message35Bytes[11] = 0;
Check("Decoded message-35 blobs own managed copies rather than borrowing caller memory",
    message35.Payload?.BlobAt30.Span[0] == 0xAA);
(byte[] Packet, int MetadataFlagOffset) MakeEnterZonePacket()
{
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    writer.Write((byte)37);
    writer.Write(0x1234567804UL);
    writer.Write(321u);
    writer.Write(0x0102030405060708UL);
    writer.Write((byte)0xA7);
    writer.Write(new byte[] { 0xFF, 0x81, 0 });
    writer.Write((ushort)0x1234);
    writer.Write(0xAABBCCDDu);
    writer.Write((byte)0x42);
    writer.Write(0x1122334455667788UL);
    writer.Write((byte)0xFE);
    writer.Write(0x33445566u);
    writer.Write(new byte[] { 0x41, 0xFF, 0 });
    int metadataOffset = (int)stream.Position;
    writer.Write((byte)0);
    writer.Write(Enumerable.Range(0, 56).Select(i => (byte)i).ToArray());
    writer.Write((byte)0x80);
    writer.Write((byte)5);
    return (stream.ToArray(), metadataOffset);
}
var (enterZoneWireBytes, enterZoneMetadataOffset) = MakeEnterZonePacket();
var enterZoneWireResult = MatrixEnterZoneWire.Decode(enterZoneWireBytes);
Check("EnterZone absent-metadata variant preserves named fields, opaque values and raw text bytes",
    enterZoneWireResult.Outcome == MatrixEnterZoneReadOutcome.Accepted
    && enterZoneWireResult.BytesConsumed == 107
    && enterZoneWireResult.Payload is { } enterZonePayload
    && enterZonePayload.NestedMetadata == null
    && enterZonePayload.Zone.ObservedFields == enteredZone
    && enterZonePayload.Zone.ValueAt38 == 0x0102030405060708
    && enterZonePayload.Zone.WordAt48 == 0x1234 && enterZonePayload.Zone.WordAt4C == 0xAABBCCDD
    && enterZonePayload.Zone.ByteAt50 == 0x42 && enterZonePayload.Zone.ByteAt60 == 0xFE
    && enterZonePayload.Zone.WordAt64 == 0x33445566
    && enterZonePayload.Zone.TextBytesAt44.Span.SequenceEqual(Convert.FromHexString("FF81"))
    && enterZonePayload.Zone.ZoneNameBytes.Span.SequenceEqual(Convert.FromHexString("41FF")));
Check("EnterZone normalizes the suffix bool but retains raw spectator byte and omits memory padding",
    enterZoneWireResult.Payload is { NormalizedFlagAtB8: true, SpectatorByteAtC0: 5 } enterZoneSuffix
    && enterZoneSuffix.ConnectAsSpectator
    && enterZoneSuffix.RawFieldsAt80.Length == 56
    && enterZoneSuffix.RawFieldsAt80.Span.SequenceEqual(Enumerable.Range(0, 56).Select(i => (byte)i).ToArray()));
Check("Every EnterZone truncated prefix fails explicitly without returning a partial packet",
    Enumerable.Range(0, enterZoneWireBytes.Length).All(length =>
    {
        var decoded = MatrixEnterZoneWire.Decode(enterZoneWireBytes.AsSpan(0, length));
        return decoded.Outcome != MatrixEnterZoneReadOutcome.Accepted && decoded.Payload == null;
    }));
var unsupportedEnterZone = enterZoneWireBytes.ToArray();
unsupportedEnterZone[enterZoneMetadataOffset] = 0xFF;
Check("Earlier restricted decoder still explicitly rejects present metadata",
    MatrixEnterZoneWire.DecodeWithoutNestedMetadata(unsupportedEnterZone)
        == new MatrixEnterZoneReadResult(MatrixEnterZoneReadOutcome.UnsupportedNestedMetadata, null, 0));
var minimalEnterZone = new byte[MatrixEnterZoneWire.MinimumPacketSizeWithoutNestedMetadata];
minimalEnterZone[0] = 37;
Check("Minimal EnterZone packet is 103 bytes, not a serialized copy of its padded C8-byte descriptor",
    MatrixEnterZoneWire.Decode(minimalEnterZone).BytesConsumed == 103
    && MatrixEnterZoneWire.Decode(enterZoneWireBytes.Concat(new byte[] { 0xFE }).ToArray())
        .BytesConsumed == 107);
byte[] MakePresentEnterZone(byte[] metadataBytes)
{
    using var stream = new MemoryStream();
    stream.Write(enterZoneWireBytes.AsSpan(0, enterZoneMetadataOffset));
    stream.WriteByte(0xFF);
    stream.Write(metadataBytes);
    stream.Write(enterZoneWireBytes.AsSpan(enterZoneMetadataOffset + 1));
    return stream.ToArray();
}
byte[] MakeEnterZoneMetadata(
    IReadOnlyList<(byte[] Key, ushort Value)> words16,
    IReadOnlyList<(byte[] Key, uint Value)> words32)
{
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    void WriteMap<T>(IReadOnlyList<(byte[] Key, T Value)> entries, Action<T> writeValue)
    {
        int position = 0;
        int count;
        do
        {
            count = Math.Min(entries.Count - position, byte.MaxValue);
            writer.Write((byte)count);
            for (int i = 0; i < count; i++)
            {
                var entry = entries[position++];
                writer.Write(entry.Key);
                writer.Write((byte)0);
                writeValue(entry.Value);
            }
        }
        while (count == byte.MaxValue);
    }
    WriteMap(words16, value => writer.Write(value));
    WriteMap(words32, value => writer.Write(value));
    return stream.ToArray();
}
var emptyEnterZoneMetadata = MatrixEnterZoneWire.Decode(MakePresentEnterZone([0, 0]));
var minimumPresentEnterZone = new byte[MatrixEnterZoneWire.MinimumPacketSizeWithEmptyNestedMetadata];
minimumPresentEnterZone[0] = 37;
minimumPresentEnterZone[44] = 0x80;
Check("Present empty metadata remains distinct from absent metadata and accepts any nonzero presence byte",
    emptyEnterZoneMetadata is { Outcome: MatrixEnterZoneReadOutcome.Accepted, BytesConsumed: 109 }
    && emptyEnterZoneMetadata.Payload?.NestedMetadata is { Word16Entries.Count: 0, Word32Entries.Count: 0 }
    && MatrixEnterZoneWire.Decode(minimumPresentEnterZone).BytesConsumed == 105);
var enterZoneMetadataBytes = MakeEnterZoneMetadata(
    [(new byte[] { 0xFF }, 0xFFFF), ("z"u8.ToArray(), 2), ("a"u8.ToArray(), 1),
     (Array.Empty<byte>(), 0x8000), (new byte[] { 0xFF }, 0xABCD),
     ("a"u8.ToArray(), 0x1234), (new byte[] { 0xFF, 1 }, 0x42)],
    [(new byte[] { 0xFF }, 0x7FC00001), ("a"u8.ToArray(), uint.MaxValue),
     (Array.Empty<byte>(), 0), ("a"u8.ToArray(), 0x80000000), (new byte[] { 0xFE }, 0x12345678)]);
var presentEnterZoneBytes = MakePresentEnterZone(enterZoneMetadataBytes);
var presentEnterZone = MatrixEnterZoneWire.Decode(presentEnterZoneBytes);
var enterZoneMetadata = presentEnterZone.Payload?.NestedMetadata;
Check("Metadata WORD map sorts unsigned byte keys, keeps empty/prefix keys and overwrites duplicates",
    enterZoneMetadata?.Word16Entries.Count == 5
    && enterZoneMetadata.Word16Entries.Select(entry => Convert.ToHexString(entry.KeyBytes.Span))
        .SequenceEqual(new[] { "", "61", "7A", "FF", "FF01" })
    && enterZoneMetadata.Word16Entries.Select(entry => entry.ValueBits)
        .SequenceEqual(new ushort[] { 0x8000, 0x1234, 2, 0xABCD, 0x42 }));
Check("Metadata DWORD map stays separate and preserves opaque bit patterns without float interpretation",
    enterZoneMetadata?.Word32Entries.Count == 4
    && enterZoneMetadata.Word32Entries.Select(entry => Convert.ToHexString(entry.KeyBytes.Span))
        .SequenceEqual(new[] { "", "61", "FE", "FF" })
    && enterZoneMetadata.Word32Entries.Select(entry => entry.ValueBits)
        .SequenceEqual(new uint[] { 0, 0x80000000, 0x12345678, 0x7FC00001 }));
Check("Present metadata consumes both maps before the unchanged 58-byte suffix and permits trailing bytes",
    presentEnterZone is { Outcome: MatrixEnterZoneReadOutcome.Accepted } && presentEnterZone.Payload is { } presentPayload
    && presentEnterZone.BytesConsumed == presentEnterZoneBytes.Length
    && presentPayload.NormalizedFlagAtB8 && presentPayload.SpectatorByteAtC0 == 5
    && presentPayload.RawFieldsAt80.Span.SequenceEqual(enterZoneWireResult.Payload!.RawFieldsAt80.Span)
    && MatrixEnterZoneWire.Decode(presentEnterZoneBytes.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed
        == presentEnterZoneBytes.Length);
Check("Every prefix of present metadata fails explicitly without leaking partially decoded maps",
    Enumerable.Range(0, presentEnterZoneBytes.Length).All(length =>
    {
        var result = MatrixEnterZoneWire.Decode(presentEnterZoneBytes.AsSpan(0, length));
        return result.Outcome != MatrixEnterZoneReadOutcome.Accepted && result.Payload == null;
    }));
Check("Both metadata maps honor chunk boundaries at 0, 1, 254, 255, 256, 510 and 511 entries",
    new[] { 0, 1, 254, 255, 256, 510, 511 }.All(count =>
    {
        byte[] Key(int i) => System.Text.Encoding.ASCII.GetBytes(i.ToString("X4"));
        var words16 = Enumerable.Range(0, count).Select(i => (Key(i), (ushort)i)).ToArray();
        var words32 = Enumerable.Range(0, count).Select(i => (Key(i), 0x80000000u | (uint)i)).ToArray();
        var encoded = MakeEnterZoneMetadata(words16, words32);
        int word16Size = count * 7 + count / 255 + 1;
        var packet = MakePresentEnterZone(encoded);
        var decoded = MatrixEnterZoneWire.Decode(packet);
        return encoded.Length == count * 16 + 2 * (count / 255 + 1)
            && encoded[word16Size] == Math.Min(count, 255)
            && (count % 255 != 0 || encoded[word16Size - 1] == 0)
            && decoded.Outcome == MatrixEnterZoneReadOutcome.Accepted
            && decoded.BytesConsumed == packet.Length
            && decoded.Payload?.NestedMetadata is { } metadata
            && metadata.Word16Entries.Count == count && metadata.Word32Entries.Count == count
            && metadata.Word16Entries.Select(entry => entry.ValueBits).SequenceEqual(words16.Select(entry => entry.Item2))
            && metadata.Word32Entries.Select(entry => entry.ValueBits).SequenceEqual(words32.Select(entry => entry.Item2));
    }));
presentEnterZoneBytes[enterZoneMetadataOffset + 2] = 1;
Check("Metadata keys own managed copies rather than borrowing the packet backing array",
    enterZoneMetadata?.Word16Entries[3].KeyBytes.Span.SequenceEqual(new byte[] { 0xFF }) == true);
var message24 = new MatrixMessage24(0x1122334455667788);
Check("Message 24 encodes instance context before its compact code, with no extra body",
    message24.Encode().SequenceEqual(Convert.FromHexString("887766554433221118"))
    && message24.Encode().Length == MatrixMessage24.EncodedSize
    && MatrixMessage24.TransportChannel == 2
    && new MatrixMessage24(0).Encode().SequenceEqual(Convert.FromHexString("000000000000000018")));
Check("Message-24 gate tests signed state greater than 4 and a nonnull transport before stamping",
    new[] { int.MinValue, -1, 0, 3, 4, 5, 6, 9, int.MaxValue }.All(state =>
    {
        var selected = MatrixMessage24NativeLayout.SelectDispatch(state, true, 0x1234567804,
            uint.MaxValue, 0xFF);
        return state <= 4
            ? selected is { Outcome: MatrixMessage24DispatchOutcome.ConnectionStateNotPermitted,
                TimestampAtE0: null, SelectsDiagnosticLog: false, Message: null }
            : selected is { Outcome: MatrixMessage24DispatchOutcome.ReadyForSubmission,
                TimestampAtE0: 4294967295000, SelectsDiagnosticLog: true }
                && selected.Message?.InstanceContext == 0x1234567804;
    })
    && MatrixMessage24NativeLayout.SelectDispatch(6, false, 1, 123, 2)
        == new MatrixMessage24DispatchDecision(MatrixMessage24DispatchOutcome.MissingTransport, null, false, null)
    && MatrixMessage24NativeLayout.SelectDispatch(6, true, 1, 123, 1)
        is { TimestampAtE0: 123000, SelectsDiagnosticLog: false });
var clockFields = new byte[56];
ulong[] clockWords = [1000, 900, 0x3FF0000000000000, 2000, 100];
for (int i = 0; i < clockWords.Length; i++)
    System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(clockFields.AsSpan(16 + i * 8), clockWords[i]);
var clockParameters = MatrixClockParameters.FromEnterZoneRawFields(clockFields, false);
Check("EnterZone suffix fields +90..B8 describe the clock's seed, reference, double scale, limit and offset",
    clockParameters == new MatrixClockParameters(1000, 900, 0x3FF0000000000000, 2000, 100, false)
    && clockParameters.Scale == 1
    && MatrixClockNativeLayout.ConnectionClockObjectOffset + MatrixClockNativeLayout.HasUpperLimitOffset
        == MatrixEnterZoneDownstreamNativeLayout.ConnectionClockHasUpperLimitOffset);
var initialClock = MatrixClockNativeLayout.Initialize(clockParameters);
var advancedClock = MatrixClockNativeLayout.AdvanceWithoutRateConversion(initialClock, 1200);
Check("Clock initialization adds the offset and unscaled advancement never moves its cursor backward",
    initialClock.CurrentTick == 1100 && initialClock.LastDeltaLowWord == 0
    && advancedClock.CurrentTick == 1300 && advancedClock.LastDeltaLowWord == 200
    && MatrixClockNativeLayout.AdvanceWithoutRateConversion(advancedClock, 1100)
        is { CurrentTick: 1300, LastDeltaLowWord: 0 });
var limitedClock = MatrixClockNativeLayout.Initialize(clockParameters with
    { HasUpperLimit = true, UpperLimitTickAtA8 = 1050 });
Check("Clock limits apply at initialization and projection but cannot move an existing cursor backward",
    limitedClock.CurrentTick == 1050
    && MatrixClockNativeLayout.AdvanceWithoutRateConversion(limitedClock, 1200).CurrentTick == 1050
    && MatrixClockNativeLayout.AdvanceWithoutRateConversion(limitedClock with { CurrentTick = 1300 }, 1200)
        is { CurrentTick: 1300, LastDeltaLowWord: 0 });
var wrappingClockParameters = clockParameters with { InitialTickAt90 = ulong.MaxValue - 3, OffsetBitsAtB0 = 0 };
var wrappingClock = MatrixClockNativeLayout.AdvanceWithoutRateConversion(
    MatrixClockNativeLayout.Initialize(wrappingClockParameters), 5);
Check("Clock comparisons use signed wrapping differences and delta storage keeps only its low DWORD",
    wrappingClock is { CurrentTick: 5, LastDeltaLowWord: 9 }
    && MatrixClockNativeLayout.Initialize(wrappingClockParameters with { OffsetBitsAtB0 = 10 }).CurrentTick == 6
    && MatrixClockNativeLayout.AdvanceWithoutRateConversion(
        MatrixClockNativeLayout.Initialize(clockParameters with { InitialTickAt90 = 0, OffsetBitsAtB0 = 0 }),
        (1UL << 32) + 7).LastDeltaLowWord == 7);
bool RejectsRestrictedClockScale(double scale)
{
    try
    {
        var parameters = clockParameters with { ScaleBitsAtA0 = unchecked((ulong)BitConverter.DoubleToInt64Bits(scale)) };
        MatrixClockNativeLayout.AdvanceWithoutRateConversion(MatrixClockNativeLayout.Initialize(parameters), 1200);
        return false;
    }
    catch (NotSupportedException) { return true; }
}
Check("The near-unit-only entry point honors the native tolerance and rejects rate conversion",
    MatrixClockNativeLayout.AdvanceWithoutRateConversion(MatrixClockNativeLayout.Initialize(
        clockParameters with { ScaleBitsAtA0 = unchecked((ulong)BitConverter.DoubleToInt64Bits(Math.BitDecrement(1.001))) }),
        1200).CurrentTick == 1300
    && RejectsRestrictedClockScale(Math.BitIncrement(1.001))
    && RejectsRestrictedClockScale(2) && RejectsRestrictedClockScale(double.NaN)
    && RejectsRestrictedClockScale(double.PositiveInfinity));
MatrixClockSnapshot AdvanceAtScale(double scale, ulong sourceTick)
{
    var parameters = clockParameters with { ScaleBitsAtA0 = unchecked((ulong)BitConverter.DoubleToInt64Bits(scale)) };
    return MatrixClockNativeLayout.Advance(MatrixClockNativeLayout.Initialize(parameters), sourceTick);
}
Check("Finite clock rates scale unsigned elapsed ticks, truncate fractions and preserve monotonic output",
    AdvanceAtScale(2, 1200) is { CurrentTick: 1600, LastDeltaLowWord: 500 }
    && AdvanceAtScale(0.5, 1201) is { CurrentTick: 1150, LastDeltaLowWord: 50 }
    && AdvanceAtScale(0, 1200) is { CurrentTick: 1100, LastDeltaLowWord: 0 }
    && AdvanceAtScale(-0.5, 1200) is { CurrentTick: 1100, LastDeltaLowWord: 0 }
    && AdvanceAtScale(1.0005, 10000900).CurrentTick == 10001000);
var scaledLimitedParameters = clockParameters with { ScaleBitsAtA0 = 0x4000000000000000,
    HasUpperLimit = true, UpperLimitTickAtA8 = 1500 };
Check("Rate-scaled projection still applies the upper limit before the monotonic cursor test",
    MatrixClockNativeLayout.Advance(MatrixClockNativeLayout.Initialize(scaledLimitedParameters), 1200)
        is { CurrentTick: 1500, LastDeltaLowWord: 400 });
Check("Large unsigned elapsed values round to double before rate multiplication",
    AdvanceAtScale(0.5, ulong.MaxValue).CurrentTick == (1UL << 63) + 1000
    && AdvanceAtScale(-0.5, ulong.MaxValue).CurrentTick == (1UL << 63) + 1000);
bool RejectsUnsupportedClockConversion(double scale, ulong sourceTick)
{
    try { AdvanceAtScale(scale, sourceTick); return false; }
    catch (NotSupportedException) { return true; }
}
Check("Nonfinite and out-of-range clock conversions remain explicit unsupported boundaries",
    RejectsUnsupportedClockConversion(double.NaN, 1200)
    && RejectsUnsupportedClockConversion(double.PositiveInfinity, 1200)
    && RejectsUnsupportedClockConversion(double.MaxValue, 1200)
    && RejectsUnsupportedClockConversion(2, ulong.MaxValue)
    && RejectsUnsupportedClockConversion(-2, ulong.MaxValue));
var downstreamPayload = enterZoneWireResult.Payload! with { RawFieldsAt80 = clockFields, NormalizedFlagAtB8 = true };
var downstreamPlan = MatrixEnterZoneDownstreamNativeLayout.CapturePlan(downstreamPayload, true, true, 0, 0x80);
Check("Paused runtime message 110 follows the clock limit flag, separately from message-24 request selection",
    downstreamPlan.EmitRuntime110Paused && downstreamPlan.AttemptMessage24
    && downstreamPlan.SetDownstreamByteAt18 && downstreamPlan.GlobalSubsystemInputByte == 0x42
    && MatrixEnterZoneDownstreamNativeLayout.RuntimeMessageId == 110
    && MatrixEnterZoneDownstreamNativeLayout.RuntimePropertyName == "paused"
    && MatrixEnterZoneDownstreamNativeLayout.CapturePlan(
        downstreamPayload with { NormalizedFlagAtB8 = false }, true, true, 0, 0)
        is { EmitRuntime110Paused: false, AttemptMessage24: false });
bool RejectsDownstream(bool hasGlobal, bool hasDownstream, uint requiredZeroWord)
{
    try
    {
        MatrixEnterZoneDownstreamNativeLayout.CapturePlan(downstreamPayload, hasGlobal, hasDownstream, requiredZeroWord, 0);
        return false;
    }
    catch (ArgumentException) { return true; }
}
Check("Native downstream prerequisites are not silently replaced with success-shaped defaults",
    RejectsDownstream(false, true, 0) && RejectsDownstream(true, false, 0) && RejectsDownstream(true, true, 1));
Check("Final EnterZone lookup follows the existing mode without requesting a new app mode",
    MatrixEnterZoneDownstreamNativeLayout.FindModeObjectPointerOffset(1) == 0x14
    && MatrixEnterZoneDownstreamNativeLayout.FindModeObjectPointerOffset(2) == 0x28E8
    && MatrixEnterZoneDownstreamNativeLayout.FindModeObjectPointerOffset(3) == 0x68
    && MatrixEnterZoneDownstreamNativeLayout.FindModeObjectPointerOffset(0) == null
    && MatrixEnterZoneDownstreamNativeLayout.CaptureModeSpecificWordUpdate(0, 0, 1) == (1u, 1u)
    && MatrixEnterZoneDownstreamNativeLayout.CaptureModeSpecificWordUpdate(0, 0, 0) == (1u, 0u)
    && MatrixEnterZoneDownstreamNativeLayout.CaptureModeSpecificWordUpdate(uint.MaxValue, 7, 0x80) == (uint.MaxValue, 7u));
var synchronizedClock = new GameSocketSynchronizedClockSnapshot(0x3F800000, 0, 0, 0);
Check("Socket synchronized clock rounds through double then single before applying its float rate",
    GameSocketSynchronizedClock.SampleFinite(synchronizedClock, 1_000_001).LastSampleAt60 == 1_000_001_024
    && GameSocketSynchronizedClock.SampleFinite(synchronizedClock with { RateBitsAt14 = 0x3DCCCCCD },
        1_000_001).LastSampleAt60 == 100_000_104);
Check("Synchronized clock subtracts its local reference and adds a separate synchronized base",
    GameSocketSynchronizedClock.SampleFinite(synchronizedClock with
        { LocalReferenceTickAt18 = 1_000_000, SynchronizedBaseTickAt20 = 500 }, 1001)
        .LastSampleAt60 == 1500);
Check("Synchronized clock never moves backward and compares wrapping differences by their sign bit",
    GameSocketSynchronizedClock.SampleFinite(synchronizedClock with { LastSampleAt60 = 2000 }, 1)
        .LastSampleAt60 == 2000
    && GameSocketSynchronizedClock.SampleFinite(synchronizedClock with
        { SynchronizedBaseTickAt20 = ulong.MaxValue - 999, LastSampleAt60 = ulong.MaxValue - 500 }, 1)
        .LastSampleAt60 == 0);
bool RejectsUnsupportedSocketClock(uint rateBits)
{
    try
    {
        GameSocketSynchronizedClock.SampleFinite(synchronizedClock with { RateBitsAt14 = rateBits }, 1_000_001);
        return false;
    }
    catch (NotSupportedException) { return true; }
}
Check("Socket clock invalid float results remain explicit rather than producing plausible timestamps",
    RejectsUnsupportedSocketClock(0x7FC00001) && RejectsUnsupportedSocketClock(0x7F800000)
    && RejectsUnsupportedSocketClock(0x7F7FFFFF));
byte[] message38Bytes = Convert.FromHexString("2688776655443322110100C07FFFFFFF80");
var message38Decoded = MatrixScalarUpdateWire.Decode(message38Bytes);
Check("Message 38 decodes the packed u64/u32/u32 block without interpreting its bit patterns",
    message38Decoded is { Outcome: MatrixScalarUpdateReadOutcome.Accepted, BytesConsumed: 17 }
    && message38Decoded.Payload == new MatrixMessage38Payload(0x1122334455667788, 0x7FC00001, 0x80FFFFFF)
    && MatrixScalarUpdateWire.Message38DefaultWordBitsAt30 == 0x3F800000
    && MatrixServerMessageCatalog.Get(38).HandlerFileOffset == MatrixScalarUpdateNativeLayout.Message38HandlerFileOffset
    && MatrixScalarUpdateNativeLayout.Message38DestinationBlockOffset == MatrixEnterZoneNativeLayout.ConnectionOpaqueBlockOffset);
Check("Message 39 retains its raw byte and selects the same subsystem input before storing it",
    MatrixScalarUpdateWire.Decode(new byte[] { 39, 0xFF })
        is { Outcome: MatrixScalarUpdateReadOutcome.Accepted, BytesConsumed: 2, Payload: MatrixMessage39Payload { ByteAt28: 0xFF } }
    && MatrixScalarUpdateNativeLayout.CaptureMessage39Update(0xFF, true) == new MatrixMessage39ObservedUpdate(0xFF, 0xFF)
    && MatrixServerMessageCatalog.Get(39).HandlerFileOffset == MatrixScalarUpdateNativeLayout.Message39HandlerFileOffset);
Check("Both scalar-update decoders reject every incomplete prefix without returning partial data",
    new[] { message38Bytes, new byte[] { 39, 0xFF } }.All(packet =>
        Enumerable.Range(0, packet.Length).All(length =>
        {
            var decoded = MatrixScalarUpdateWire.Decode(packet.AsSpan(0, length));
            return decoded.Outcome != MatrixScalarUpdateReadOutcome.Accepted && decoded.Payload == null;
        })));
Check("Scalar-update framing rejects noncanonical and unrelated codes but exposes trailing bytes",
    MatrixScalarUpdateWire.Decode(new byte[] { 0, 38, 0 }).Outcome == MatrixScalarUpdateReadOutcome.NonCanonicalHeader
    && MatrixScalarUpdateWire.Decode(new byte[] { 37, 0 }).Outcome == MatrixScalarUpdateReadOutcome.UnexpectedMessageCode
    && MatrixScalarUpdateWire.Decode(message38Bytes.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == 17
    && MatrixScalarUpdateWire.Decode(new byte[] { 39, 0, 0xAA }).BytesConsumed == 2);
bool message39MissingSubsystemRejected = false;
try { MatrixScalarUpdateNativeLayout.CaptureMessage39Update(0, false); }
catch (ArgumentException) { message39MissingSubsystemRejected = true; }
Check("Message-39 subsystem absence is an explicit native prerequisite failure", message39MissingSubsystemRejected);
var queueStatusBytes = Convert.FromHexString("3EFFFFFFFF00000080");
var queueStatus = MatrixScalarUpdateWire.Decode(queueStatusBytes);
Check("Message 62 is queue status, preserving both DWORDs and selecting runtime message 295",
    queueStatus is { Outcome: MatrixScalarUpdateReadOutcome.Accepted, BytesConsumed: 9,
        Payload: MatrixQueueStatusPayload { QueuePositionBits: uint.MaxValue, QueueLengthBits: 0x80000000 } }
    && MatrixQueueStatusNativeLayout.CaptureNotification(new(5, 25))
        == new MatrixQueueStatusRuntimeNotification(295, 5, 25)
    && MatrixQueueStatusNativeLayout.PositionPropertyName == "queue_position"
    && MatrixQueueStatusNativeLayout.LengthPropertyName == "queue_length"
    && MatrixServerMessageCatalog.Get(62).HandlerFileOffset == MatrixQueueStatusNativeLayout.HandlerFileOffset);
Check("Queue-status prefixes fail explicitly and trailing data is not counted as its body",
    Enumerable.Range(0, queueStatusBytes.Length).All(length =>
        MatrixScalarUpdateWire.Decode(queueStatusBytes.AsSpan(0, length)).Outcome != MatrixScalarUpdateReadOutcome.Accepted)
    && MatrixScalarUpdateWire.Decode(queueStatusBytes.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == 9);
Check("ExitZone msg-40 is header-only, distinct from queue status and its outgoing acknowledgement",
    MatrixExitZoneNativeLayout.Decode(new byte[] { 40 })
        == new MatrixExitZoneReadResult(MatrixExitZoneReadOutcome.Accepted, 1)
    && MatrixExitZoneNativeLayout.Decode(new byte[] { 40, 0xAA }).BytesConsumed == 1
    && MatrixExitZoneNativeLayout.Decode(Array.Empty<byte>()).Outcome == MatrixExitZoneReadOutcome.TruncatedHeader
    && MatrixExitZoneNativeLayout.Decode(new byte[] { 0, 40, 0 }).Outcome == MatrixExitZoneReadOutcome.NonCanonicalHeader
    && MatrixExitZoneNativeLayout.Decode(new byte[] { 62 }).Outcome == MatrixExitZoneReadOutcome.UnexpectedMessageCode
    && MatrixExitZoneAck.Encode().SequenceEqual(new byte[] { 19 })
    && MatrixExitZoneAck.TransportChannel == 1);
Check("Known ExitZone cleanup requires state 6 or 7 and passes through connection state 5",
    MatrixExitZoneNativeLayout.CaptureCleanupState(6, true, true) == 5
    && MatrixExitZoneNativeLayout.CaptureCleanupState(7, true, true) == 5);
bool RejectsExitZoneCleanup(int state, bool hasDownstream, bool hasMode)
{
    try { MatrixExitZoneNativeLayout.CaptureCleanupState(state, hasDownstream, hasMode); return false; }
    catch (ArgumentException) { return true; }
}
Check("ExitZone state and downstream/current-mode prerequisites fail explicitly",
    RejectsExitZoneCleanup(4, true, true) && RejectsExitZoneCleanup(6, false, true)
    && RejectsExitZoneCleanup(6, true, false));
Check("Disconnect-on-exit branch selects socket release, optional hook and connection state 1",
    MatrixExitZoneNativeLayout.SelectAfterCleanup(5, 0x80, true, true, 0xFF)
        == new MatrixExitZoneBranchSelection(1, true, true, true, false, false)
    && MatrixExitZoneNativeLayout.SelectAfterCleanup(5, 1, false, false, 0xFF)
        == new MatrixExitZoneBranchSelection(1, false, false, false, false, false));
Check("Stay-connected branch selects ExitZoneAck only with transport and signed post-cleanup state greater than 3",
    MatrixExitZoneNativeLayout.SelectAfterCleanup(5, 0, true, true, 1)
        == new MatrixExitZoneBranchSelection(4, false, false, false, true, true)
    && MatrixExitZoneNativeLayout.SelectAfterCleanup(5, 0, false, true, 1)
        == new MatrixExitZoneBranchSelection(4, false, false, false, false, false)
    && MatrixExitZoneNativeLayout.SelectAfterCleanup(int.MinValue, 0, true, false, 1)
        == new MatrixExitZoneBranchSelection(4, false, false, false, false, false)
    && MatrixExitZoneNativeLayout.SelectAfterCleanup(5, 0, true, false, 2)
        == new MatrixExitZoneBranchSelection(4, false, false, false, true, false));
Check("Socket destroy is a state selection, not unconditional object deallocation",
    new[] { NativeGameSocketState.Initializing, NativeGameSocketState.Transferring, NativeGameSocketState.WaitingContact }
        .All(state => GameSocketDestroyNativeLayout.SelectStateBranch(state)
            == new GameSocketDestroyBranch(NativeGameSocketState.Destroyed, true, false, false, false))
    && new[] { NativeGameSocketState.Connecting, NativeGameSocketState.Connected }
        .All(state => GameSocketDestroyNativeLayout.SelectStateBranch(state)
            == new GameSocketDestroyBranch(NativeGameSocketState.Destroyed, false, false, false, false))
    && GameSocketDestroyNativeLayout.SelectStateBranch(NativeGameSocketState.NotOpened)
        == new GameSocketDestroyBranch(NativeGameSocketState.Closed, false, true, true, true)
    && new[] { NativeGameSocketState.DisconnectedNotify, NativeGameSocketState.Disconnected }
        .All(state => GameSocketDestroyNativeLayout.SelectStateBranch(state)
            == new GameSocketDestroyBranch(NativeGameSocketState.Closed, false, false, true, true)));
bool RejectsSocketDestroyState(NativeGameSocketState state)
{
    try { GameSocketDestroyNativeLayout.SelectStateBranch(state); return false; }
    catch (ArgumentOutOfRangeException) { return true; }
}
Check("Socket destroy rejects terminal/unknown states and captures metadata only on the first atomic request",
    RejectsSocketDestroyState(NativeGameSocketState.Destroyed)
    && RejectsSocketDestroyState(NativeGameSocketState.Closed)
    && RejectsSocketDestroyState((NativeGameSocketState)10)
    && RejectsSocketDestroyState((NativeGameSocketState)(-1))
    && GameSocketDestroyNativeLayout.SelectFirstRequestCapture(0)
    && !GameSocketDestroyNativeLayout.SelectFirstRequestCapture(6)
    && !GameSocketDestroyNativeLayout.SelectFirstRequestCapture(uint.MaxValue));
Check("World cleanup observer gate compares both raw words; connection state and socket state remain separate",
    TfWorldZoneCleanupNativeLayout.SelectObserverCall(0x80000000, uint.MaxValue, 0x80000000, uint.MaxValue)
    && !TfWorldZoneCleanupNativeLayout.SelectObserverCall(1, 2, 1, 3)
    && !TfWorldZoneCleanupNativeLayout.SelectObserverCall(1, 2, 3, 2)
    && TfWorldZoneCleanupNativeLayout.EnteredZoneCleanupCallerFileOffset == 0x494B00
    && TfWorldZoneCleanupNativeLayout.EnteredZoneCleanupCallFileOffset == 0x494BDD
    && TfWorldZoneCleanupNativeLayout.CleanupGuardedMetadataLookupMethodFileOffset == 0x1010AC0
    && TfWorldZoneCleanupNativeLayout.CleanupGuardedRecordDispatchMethodFileOffset == 0x369A30
    && TfWorldZoneCleanupNativeLayout.CleanupGuardedRecordDispatchTargetFileOffset == 0x101D710
    && TfWorldZoneCleanupNativeLayout.CleanupGuardedMetadataLookupIndex == 0x6A
    && TfWorldZoneCleanupNativeLayout.ConnectionCleanupCallFileOffset == 0x64E27D
    && TfWorldZoneCleanupNativeLayout.ConnectionStateFieldOffset == 0x0C
    && TfWorldZoneCleanupNativeLayout.RequiredConnectionState == 5
    && TfWorldZoneCleanupNativeLayout.ConnectionDownstreamPointerOffset == 0x910
    && TfWorldZoneCleanupNativeLayout.DownstreamActionPointerOffset == 0x08
    && TfWorldZoneCleanupNativeLayout.ConnectionDownstreamHelperFileOffset == 0x7FEB50
    && TfWorldZoneCleanupNativeLayout.DownstreamHelperCallFileOffset == 0x494AB9
    && TfWorldZoneCleanupNativeLayout.CleanupFirstListHeadOffset == 0x20
    && TfWorldZoneCleanupNativeLayout.CleanupFirstNodeForwardOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupFirstNodeValuePointerOffset == 0x10
    && TfWorldZoneCleanupNativeLayout.CleanupFirstListResetWordOffset == 0x24
    && TfWorldZoneCleanupNativeLayout.CleanupFirstNodeHelperFileOffset == 0x977E20
    && TfWorldZoneCleanupNativeLayout.CleanupFirstValueHelperFileOffset == 0x975020
    && TfWorldZoneCleanupNativeLayout.CleanupSecondListHeadOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNodeForwardOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNodeDataOffset == 0x10
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNodeNestedRangeOffset == 0x20
    && TfWorldZoneCleanupNativeLayout.CleanupSecondListResetWordOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNodeHelperFileOffset == 0x976D30
    && TfWorldZoneCleanupNativeLayout.CleanupSecondListHelperFileOffset == 0x9795B0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeCleanupHelperFileOffset == 0x3F7680
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeWholeTreeClearHelperFileOffset == 0x3F7300
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeItemHelperFileOffset == 0x3F7430
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeRecursiveNodeHelperFileOffset == 0x3F72C0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNodeNestedRangeCountOffset == 0x24
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeTreeHeaderFirstLinkOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeTreeHeaderSecondLinkOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeTreeHeaderThirdLinkOffset == 8
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeNodeForwardLinkOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeNodeRecursiveChildOffset == 8
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeNodeSentinelByteOffset == 0x0D
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueMethodFileOffset == 0x975020
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerOneFileOffset == 0x976200
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerOneCallFileOffset == 0x976254
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoFileOffset == 0x977F20
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoCallFileOffset == 0x977F78
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoGuardWordOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoWaitHelperFileOffset == 0x977630
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoWaitCallFileOffset == 0x977F6B
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoWaitRemovalCallFileOffset == 0x977726
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoGuardDecrementFileOffset == 0x978673
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValuePreCleanVirtualCallFileOffset == 0x977ECC
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValuePreCleanEventPushFileOffset == 0x977EC5
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValuePreCleanVirtualSlotOffset == 0xAC
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValuePreCleanEventId == 0x169
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueFirstTreeHeadOffset == 0x08
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSecondTreeHeadOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueFirstTreeEntryPointerOffset == 0x14
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSecondTreeEntryPointerOffset == 0x18
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSharedTextPointerOffset == 0x18
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSpanRangeOffset == 0x10
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSpanElementStride == 24
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueFirstEntryHelperFileOffset == 0x9751F0
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSecondEntryHelperFileOffset == 0x88AD60
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueFirstTreeClearFileOffset == 0x97AE50
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSecondTreeClearFileOffset == 0x97AA70
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueFirstTreeRemoveNodeFileOffset == 0x97BCC0
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSecondTreeRemoveNodeFileOffset == 0x978440
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSpanRangeReleaseFileOffset == 0x979A20
    && TfWorldZoneCleanupNativeLayout.CleanupNestedEntrySharedTextPointerAOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupNestedEntrySharedTextPointerBOffset == 8
    && TfWorldZoneCleanupNativeLayout.CleanupNestedEntrySpanRangeOffset == 0x10
    && TfWorldZoneCleanupNativeLayout.CleanupNestedTreeNodeLeftPointerOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupNestedTreeNodeParentPointerOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupNestedTreeNodeRightPointerOffset == 8
    && TfWorldZoneCleanupNativeLayout.CleanupNestedTreeNodeColorByteOffset == 0x0C
    && TfWorldZoneCleanupNativeLayout.CleanupNestedTreeNodeSentinelByteOffset == 0x0D
    && TfWorldZoneCleanupNativeLayout.CleanupFirstRangeStartOffset == 0x28
    && TfWorldZoneCleanupNativeLayout.CleanupFirstRangeEndOffset == 0x2C
    && TfWorldZoneCleanupNativeLayout.CleanupFirstRangeHelperFileOffset == 0x97CBB0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondRangeStartOffset == 8
    && TfWorldZoneCleanupNativeLayout.CleanupSecondRangeEndOffset == 0x0C
    && TfWorldZoneCleanupNativeLayout.CleanupSecondRangeHelperFileOffset == 0x97C690
    && TfWorldZoneCleanupNativeLayout.CleanupRangeBeginPointerOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupRangeEndPointerOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupRangeCapacityEndPointerOffset == 8
    && TfWorldZoneCleanupNativeLayout.CleanupRangeRequestedCount == 0x10
    && TfWorldZoneCleanupNativeLayout.CleanupRangeElementStride == 4
    && TfWorldZoneCleanupNativeLayout.CleanupRangeHelpersMayReplaceBackingStorage
    && TfWorldZoneCleanupNativeLayout.CleanupFirstRangePostValueOffset == 0x34
    && TfWorldZoneCleanupNativeLayout.CleanupFirstRangePostCountOffset == 0x38
    && TfWorldZoneCleanupNativeLayout.CleanupSecondRangePostValueOffset == 0x14
    && TfWorldZoneCleanupNativeLayout.CleanupSecondRangePostCountOffset == 0x18
    && TfWorldZoneCleanupNativeLayout.CleanupRangePostValue == 7
    && TfWorldZoneCleanupNativeLayout.CleanupRangePostCount == 8
    && TfWorldZoneCleanupNativeLayout.LocalClientResetMethodFileOffset == 0x4A2FB0
    && TfWorldZoneCleanupNativeLayout.LocalClientMember4ResetMethodFileOffset == 0x431C90
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupManagerCallFileOffset == 0x8610B5
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupRecordPointerOffset == 0x328
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerOffset == 0x32C
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerInitializeFileOffset == 0x8602CD
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerProviderFileOffset == 0x860810
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerFactoryCallFileOffset == 0x860F6F
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerStoreFileOffset == 0x860F76
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerCallFileOffset == 0x8610C8
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerVtableSlotOffset == 0
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerCallArgument == 1
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectResourceManagerCleanupMethodFileOffset == 0xD74A20
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupRecordLeadingDword == 0x3C8
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupRecordTokenOffset == 0x398
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupRecordManagerStateByteOffset == 0x39C
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerStateByteOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerLockOffset == 0x38
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerActivityCountOffset == 0x68
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerTokenArrayBeginPointerOffset == 0xB8
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerTokenArrayEndPointerOffset == 0xBC
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerGlobalPointerVirtualAddress == 0x1DFFD98
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerGlobalHolderOffset == 0xB98
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerGetterVirtualOffset == 0x24
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerResultMemberOffset == 0x0C
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerTokenHookFileOffset == 0xD935F0
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerPostRemovalHelperFileOffset == 0xD78900
    && TfWorldZoneCleanupNativeLayout.ReferenceReleaseHelperFileOffset == 0x424790
    && TfWorldZoneCleanupNativeLayout.ReferenceReleaseHelperCallFileOffset == 0x8610D4
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectReferencePointerOffset == 0x320
    && TfWorldZoneCleanupNativeLayout.ReferenceAllocationTagOffsetFromPayload == -8
    && TfWorldZoneCleanupNativeLayout.ReferenceAllocationCountOffsetFromPayload == -4
    && TfWorldZoneCleanupNativeLayout.SharedTextAllocatorNodeTag == 0x17
    && TfWorldZoneCleanupNativeLayout.SharedTextAllocatorSingletonMethodFileOffset == 0x12B9F70
    && TfWorldZoneCleanupNativeLayout.SharedTextAllocatorNodeReleaseMethodFileOffset == 0x12CDEC0
    && TfWorldZoneCleanupNativeLayout.SharedTextAllocatorNodePushMethodFileOffset == 0x12CDA60
    && TfWorldZoneCleanupNativeLayout.SharedTextAllocatorNodeReleaseCallFileOffset == 0x4247C2
    && TfWorldZoneCleanupNativeLayout.SharedTextAllocatorNodeReferenceCountOffset == 0x10
    && TfWorldZoneCleanupNativeLayout.RuntimeMessageId == 97);
Check("Destroyed-state tick skips an in-flight close and otherwise checks keepalive/retries before assembly",
    GameSocketCloseCompletionNativeLayout.SelectDestroyedTick(true, false, false)
        == DestroyedSocketTickSelection.CloseAlreadyRequested
    && GameSocketCloseCompletionNativeLayout.SelectDestroyedTick(false, false, true)
        == DestroyedSocketTickSelection.RequestPumpClose
    && GameSocketCloseCompletionNativeLayout.SelectDestroyedTick(false, true, false)
        == DestroyedSocketTickSelection.RequestPumpClose
    && GameSocketCloseCompletionNativeLayout.SelectDestroyedTick(false, true, true)
        == DestroyedSocketTickSelection.AssembleOutgoingThenCheckState
    && GameSocketCloseCompletionNativeLayout.SelectAssemblerTailClose(NativeGameSocketState.Destroyed)
    && !GameSocketCloseCompletionNativeLayout.SelectAssemblerTailClose(NativeGameSocketState.Connected));
bool RejectsCloseCompletion(bool handleInvalid, bool recorded)
{
    try { GameSocketCloseCompletionNativeLayout.CaptureCompletionEvent(handleInvalid, recorded); return false; }
    catch (ArgumentException) { return true; }
}
Check("Close callback queues token1 and rejects valid handles or repeated completion",
    GameSocketCloseCompletionNativeLayout.CaptureCompletionEvent(true, false) == 1
    && RejectsCloseCompletion(false, false) && RejectsCloseCompletion(true, true));
Check("Queued close events clean a Destroyed socket, notify active sockets and ignore already closed/disconnected",
    GameSocketCloseCompletionNativeLayout.SelectQueuedCloseEvent(NativeGameSocketState.Destroyed)
        == new QueuedSocketCloseSelection(NativeGameSocketState.Closed, true, false)
    && Enumerable.Range(1, 5).All(state =>
        GameSocketCloseCompletionNativeLayout.SelectQueuedCloseEvent((NativeGameSocketState)state)
            == new QueuedSocketCloseSelection(NativeGameSocketState.DisconnectedNotify, false, false))
    && new[] { NativeGameSocketState.Disconnected, NativeGameSocketState.Closed }.All(state =>
        GameSocketCloseCompletionNativeLayout.SelectQueuedCloseEvent(state) == new QueuedSocketCloseSelection(state, false, true)));
bool RejectsQueuedCloseState(NativeGameSocketState state)
{
    try { GameSocketCloseCompletionNativeLayout.SelectQueuedCloseEvent(state); return false; }
    catch (ArgumentOutOfRangeException) { return true; }
}
Check("Queued close event rejects NotOpened, DisconnectedNotify and unknown states explicitly",
    RejectsQueuedCloseState(NativeGameSocketState.NotOpened)
    && RejectsQueuedCloseState(NativeGameSocketState.DisconnectedNotify)
    && RejectsQueuedCloseState((NativeGameSocketState)10));
var message41AbsentBytes = Convert.FromHexString("29FFFFFFFF0000008080FF3412FE000300DEADBE");
var message41Absent = MatrixServerMessages41To43Wire.Decode(message41AbsentBytes);
Check("Message41 preserves raw scalar widths, absent optional list and length-prefixed blob",
    message41Absent is { Outcome: MatrixServer41To43ReadOutcome.Accepted, BytesConsumed: 20,
        Payload: MatrixMessage41Payload { WordAt28: uint.MaxValue, WordAt2C: 0x80000000,
            ByteAt30: 0x80, ByteAt31: 0xFF, WordAt32: 0x1234, ByteAt34: 0xFE, OptionalEntries: null } }
    && ((MatrixMessage41Payload)message41Absent.Payload).BlobAt48.Span.SequenceEqual(Convert.FromHexString("DEADBE")));
var message41EntryBytes = Convert.FromHexString("08070605040302011020F080");
var message41PresentBytes = message41AbsentBytes[..14].Concat(new byte[] { 0xFE, 2 })
    .Concat(message41EntryBytes).Concat(message41EntryBytes).Concat(new byte[] { 0, 0 }).ToArray();
var message41Present = MatrixServerMessages41To43Wire.Decode(message41PresentBytes);
var message41PresentPayload = message41Present.Payload as MatrixMessage41Payload;
Check("Message41 any nonzero optional marker is present and entries consume twelve wire bytes, not native sixteen",
    message41Present.Outcome == MatrixServer41To43ReadOutcome.Accepted
    && message41Present.BytesConsumed == message41PresentBytes.Length
    && message41PresentPayload?.OptionalEntries is { Count: 2 } entries41
    && entries41[0] == new MatrixMessage41Entry(0x0102030405060708, 0x10, 0x20, 0xF0, 0x80)
    && entries41[0] == entries41[1] && message41PresentPayload.BlobAt48.IsEmpty
    && MatrixMessages41And43NativeLayout.SelectMessage41TransportByte(0xFE) == 0
    && MatrixMessages41And43NativeLayout.SelectMessage41TransportByte(0xFF) == 1);
var message41EmptyPresent = MatrixServerMessages41To43Wire.Decode(
    message41AbsentBytes[..14].Concat(new byte[] { 1, 0, 0, 0 }).ToArray());
Check("Message41 present-empty list differs from absence and packet minimums are exact",
    message41EmptyPresent is { BytesConsumed: 18, Payload: MatrixMessage41Payload { OptionalEntries.Count: 0 } }
    && MatrixServerMessages41To43Wire.Decode(message41AbsentBytes[..14].Concat(new byte[] { 0, 0, 0 }).ToArray())
        .BytesConsumed == MatrixServerMessages41To43Wire.Message41MinimumPacketSize);
var pvpRecordBytes = Convert.FromHexString("FFFFFFFF00000080004100");
var pvpBytes = new byte[] { 42, 2 }.Concat(pvpRecordBytes).Concat(pvpRecordBytes)
    .Concat(Convert.FromHexString("78563412")).ToArray();
var pvpRead = MatrixServerMessages41To43Wire.Decode(pvpBytes);
Check("Message42 PvP queue records preserve order/duplicates, both raw texts and the trailing DWORD",
    pvpRead.Outcome == MatrixServer41To43ReadOutcome.Accepted && pvpRead.BytesConsumed == pvpBytes.Length
    && pvpRead.Payload is MatrixPvpQueuePayload { Records.Count: 2, WordAt34: 0x12345678 } pvpPayload
    && pvpPayload.Records[0].WordAt0 == uint.MaxValue && pvpPayload.Records[0].WordAt4 == 0x80000000
    && pvpPayload.Records[0].TextBytesAt8.IsEmpty
    && pvpPayload.Records[0].TextBytesAtC.Span.SequenceEqual(new byte[] { 0x41 })
    && pvpPayload.Records[1].WordAt0 == uint.MaxValue);
var message43Bytes = Convert.FromHexString("2BFFFFFFFF00FF");
Check("Message43 has a raw DWORD/WORD body even though its handler has no observed payload use",
    MatrixServerMessages41To43Wire.Decode(message43Bytes)
        is { BytesConsumed: 7, Payload: MatrixMessage43Payload { WordAt28: uint.MaxValue, WordAt2C: 0xFF00 } }
    && MatrixServerMessages41To43Wire.Decode(Convert.FromHexString("2A0009000000"))
        is { BytesConsumed: 6, Payload: MatrixPvpQueuePayload { Records.Count: 0, WordAt34: 9 } });
Check("Messages41-43 reject all strict prefixes, malformed/foreign headers and ignore trailing bytes for consumed length",
    new[] { message41AbsentBytes, message41PresentBytes, pvpBytes, message43Bytes }.All(packet =>
        Enumerable.Range(0, packet.Length).All(length =>
            MatrixServerMessages41To43Wire.Decode(packet.AsSpan(0, length)).Outcome != MatrixServer41To43ReadOutcome.Accepted)
        && MatrixServerMessages41To43Wire.Decode(packet.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == packet.Length)
    && MatrixServerMessages41To43Wire.Decode(new byte[] { 0, 41, 0 }).Outcome == MatrixServer41To43ReadOutcome.NonCanonicalHeader
    && MatrixServerMessages41To43Wire.Decode(new byte[] { 40 }).Outcome == MatrixServer41To43ReadOutcome.UnexpectedMessageCode);
var pvp255Body = Enumerable.Range(0, 255).SelectMany(_ => pvpRecordBytes).ToArray();
var pvp255Bytes = new byte[] { 42, 255 }.Concat(pvp255Body)
    .Concat(new byte[] { 0, 9, 0, 0, 0 }).ToArray();
var message41Entries255 = Enumerable.Range(0, 255).SelectMany(_ => message41EntryBytes).ToArray();
var message41Bytes255 = message41AbsentBytes[..14].Concat(new byte[] { 1, 255 })
    .Concat(message41Entries255).Concat(new byte[] { 0, 0, 0 }).ToArray();
Check("Both message41/42 list readers require a terminal zero chunk after exactly255 entries",
    MatrixServerMessages41To43Wire.Decode(pvp255Bytes) is { Payload: MatrixPvpQueuePayload { Records.Count: 255 } }
    && MatrixServerMessages41To43Wire.Decode(message41Bytes255)
        is { Payload: MatrixMessage41Payload { OptionalEntries.Count: 255 } }
    && MatrixServerMessages41To43Wire.Decode(new byte[] { 42, 255 }.Concat(pvp255Body)
        .Concat(new byte[] { 9, 0, 0, 0 }).ToArray()).Outcome == MatrixServer41To43ReadOutcome.TruncatedPayload
    && MatrixServerMessages41To43Wire.Decode(message41Bytes255[..^1]).Outcome == MatrixServer41To43ReadOutcome.TruncatedPayload);
Check("PvP queue matching accepts wildcard9, clears mismatches but reports queued from original input presence",
    MatrixPvpQueueNativeLayout.SelectUpdate(true, 9, 3) == new MatrixPvpQueueUpdateSelection(true, false, true)
    && MatrixPvpQueueNativeLayout.SelectUpdate(true, 3, 3) == new MatrixPvpQueueUpdateSelection(true, false, true)
    && MatrixPvpQueueNativeLayout.SelectUpdate(true, 4, 3) == new MatrixPvpQueueUpdateSelection(false, true, true)
    && MatrixPvpQueueNativeLayout.SelectUpdate(false, 9, 3) == new MatrixPvpQueueUpdateSelection(false, true, false)
    && MatrixPvpQueueNativeLayout.SelectHandlerConsumer(true, true, true)
    && !MatrixPvpQueueNativeLayout.SelectHandlerConsumer(true, false, true)
    && !MatrixPvpQueueNativeLayout.SelectHandlerConsumer(true, true, false));
var pvp256Bytes = new byte[] { 42, 255 }.Concat(pvp255Body).Concat(new byte[] { 1 })
    .Concat(pvpRecordBytes).Concat(new byte[] { 9, 0, 0, 0 }).ToArray();
Check("PvP queue chunk continuation reads a second nonempty chunk without requiring a final zero after count1",
    MatrixServerMessages41To43Wire.Decode(pvp256Bytes)
        is { Payload: MatrixPvpQueuePayload { Records.Count: 256 } } pvp256
    && pvp256.BytesConsumed == pvp256Bytes.Length);
bool RejectsMissingPvpDownstream()
{
    try { MatrixPvpQueueNativeLayout.SelectHandlerConsumer(false, true, true); return false; }
    catch (ArgumentException) { return true; }
}
Check("PvP queue missing downstream is an explicit native prerequisite failure", RejectsMissingPvpDownstream());
var foundMatchBytes = Convert.FromHexString("2C785634120807060504030201FF1111222233334444");
Check("Message44 FoundMatchUpdate has twenty-one body bytes without native alignment padding",
    MatrixServerMessages44To47Wire.Decode(foundMatchBytes)
        is { BytesConsumed: 22, Payload: MatrixFoundMatchUpdatePayload { MatchmakerId: 0x12345678,
            MatchId: 0x0102030405060708, StateByte: 0xFF,
            WordAt3A: 0x1111, WordAt3C: 0x2222, WordAt3E: 0x3333, WordAt40: 0x4444 } });
var joinEmptyBytes = new byte[] { 45 }.Concat(Convert.FromHexString("0807060504030201"))
    .Concat(new byte[] { 0x80, 0, 0xFE }).Concat(Convert.FromHexString("00000080"))
    .Concat(new byte[] { 0 }).Concat(Convert.FromHexString("FFFFFFFF")).ToArray();
var joinEntryBytes = Convert.FromHexString("FFFFFFFF41000080FE01");
var joinBytes = joinEmptyBytes[..16].Concat(new byte[] { 2 }).Concat(joinEntryBytes).Concat(joinEntryBytes)
    .Concat(joinEmptyBytes[^4..]).ToArray();
var joinRead = MatrixServerMessages44To47Wire.Decode(joinBytes);
Check("Message45 retains raw boolean bytes and variable-length challenge records",
    joinRead.Outcome == MatrixServer44To47ReadOutcome.Accepted && joinRead.BytesConsumed == joinBytes.Length
    && joinRead.Payload is MatrixChallengeJoinResponsePayload { ChallengeId: 0x0102030405060708,
        JoinSucceededByte: 0x80, CreatedByte: 0, SquadReasonByte: 0xFE,
        WordAt34: 0x80000000, WordAt44: uint.MaxValue, Entries.Count: 2 } joinPayload
    && joinPayload.Entries[0].WordAt0 == uint.MaxValue
    && joinPayload.Entries[0].TextBytesAt4.Span.SequenceEqual(new byte[] { 0x41 })
    && joinPayload.Entries[0].TextBytesAt8.IsEmpty
    && joinPayload.Entries[0].ByteAtC == 0x80 && joinPayload.Entries[0].ByteAtD == 0xFE
    && joinPayload.Entries[0].ByteAtE == 1
    && MatrixServerMessages44To47Wire.Decode(joinEmptyBytes).BytesConsumed == 21);
var invitation46Bytes = new byte[] { 46 }.Concat(Convert.FromHexString("0807060504030201FFFFFFFFFFFFFFFFFF8000")).ToArray();
var invitation47Bytes = invitation46Bytes.ToArray();
invitation47Bytes[0] = 47;
Check("Messages46/47 have identical two-QWORD/text bodies and distinct consumer variant flags",
    MatrixServerMessages44To47Wire.Decode(invitation46Bytes)
        is { Payload: MatrixChallengeInvitationPayload { MessageCode: 46, QwordAt28: 0x0102030405060708,
            QwordAt30: ulong.MaxValue } } invitation46
    && ((MatrixChallengeInvitationPayload)invitation46.Payload).NameBytes.Span.SequenceEqual(new byte[] { 0xFF, 0x80 })
    && MatrixServerMessages44To47Wire.Decode(invitation47Bytes)
        is { Payload: MatrixChallengeInvitationPayload { MessageCode: 47 } }
    && !MatrixChallengeNativeLayout.SelectInvitationVariant(46) && MatrixChallengeNativeLayout.SelectInvitationVariant(47));
Check("Messages44-47 reject strict prefixes/noncanonical/foreign headers and consume exactly their own bodies",
    new[] { foundMatchBytes, joinEmptyBytes, joinBytes, invitation46Bytes, invitation47Bytes }.All(packet =>
        Enumerable.Range(0, packet.Length).All(length =>
            MatrixServerMessages44To47Wire.Decode(packet.AsSpan(0, length)).Outcome != MatrixServer44To47ReadOutcome.Accepted)
        && MatrixServerMessages44To47Wire.Decode(packet.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == packet.Length)
    && MatrixServerMessages44To47Wire.Decode(new byte[] { 0, 44, 0 }).Outcome == MatrixServer44To47ReadOutcome.NonCanonicalHeader
    && MatrixServerMessages44To47Wire.Decode(new byte[] { 43 }).Outcome == MatrixServer44To47ReadOutcome.UnexpectedMessageCode);
var join255Bytes = joinEmptyBytes[..16].Concat(new byte[] { 255 })
    .Concat(Enumerable.Range(0, 255).SelectMany(_ => joinEntryBytes)).Concat(new byte[] { 0 })
    .Concat(joinEmptyBytes[^4..]).ToArray();
Check("Message45 uses chunked255 counts and no serialized sixteenth native-record padding byte",
    MatrixServerMessages44To47Wire.Decode(join255Bytes)
        is { Payload: MatrixChallengeJoinResponsePayload { Entries.Count: 255 } } join255
    && join255.BytesConsumed == join255Bytes.Length
    && MatrixServerMessages44To47Wire.Decode(join255Bytes[..^1]).Outcome == MatrixServer44To47ReadOutcome.TruncatedPayload);
Check("Challenge join response gates missing/mismatched/invalid stored IDs before selecting release or apply",
    MatrixChallengeNativeLayout.SelectJoinResponse(null, 1, 1, 1, 0).Selection == MatrixChallengeJoinSelection.IgnoreUnexpectedResponse
    && MatrixChallengeNativeLayout.SelectJoinResponse(1, 2, 1, 1, 0).Selection == MatrixChallengeJoinSelection.IgnoreUnexpectedResponse
    && MatrixChallengeNativeLayout.SelectJoinResponse(0, 1, 1, 0, 0).Selection == MatrixChallengeJoinSelection.IgnoreUnexpectedResponse
    && MatrixChallengeNativeLayout.SelectJoinResponse(0xFFFFFFFFFFFFFF42, 1, 1, 0, 0).Selection
        == MatrixChallengeJoinSelection.IgnoreUnexpectedResponse
    && MatrixChallengeNativeLayout.SelectJoinResponse(1, 1, 0, 0, 0)
        == new MatrixChallengeJoinNotificationSelection(MatrixChallengeJoinSelection.ReleaseActiveChallenge, false, null, false));
Check("Challenge success normalizes raw bytes and selects exact runtime reason precedence",
    MatrixChallengeNativeLayout.SelectJoinResponse(0, 1, 0x80, 0xFE, 0)
        == new MatrixChallengeJoinNotificationSelection(MatrixChallengeJoinSelection.ApplyJoinedChallenge, true, "Created", true)
    && MatrixChallengeNativeLayout.SelectJoinResponse(1, 1, 1, 1, 0x80)
        == new MatrixChallengeJoinNotificationSelection(MatrixChallengeJoinSelection.ApplyJoinedChallenge, true, "Squad", true)
    && MatrixChallengeNativeLayout.SelectJoinResponse(1, 1, 1, 0, 0)
        == new MatrixChallengeJoinNotificationSelection(MatrixChallengeJoinSelection.ApplyJoinedChallenge, false, "Accepted", true));
Check("FoundMatch ordinary deadline gate and state2/3 notification suppression are distinct",
    MatrixPvpQueueNativeLayout.SelectFoundMatchNotificationSuppression(2, 2)
    && MatrixPvpQueueNativeLayout.SelectFoundMatchNotificationSuppression(3, 3)
    && !MatrixPvpQueueNativeLayout.SelectFoundMatchNotificationSuppression(1, 1)
    && !MatrixPvpQueueNativeLayout.SelectFoundMatchNotificationSuppression(2, 3)
    && MatrixPvpQueueNativeLayout.SelectFoundMatchDeadlineRefresh(1, 2, 100)
    && MatrixPvpQueueNativeLayout.SelectFoundMatchDeadlineRefresh(2, 2, 0)
    && !MatrixPvpQueueNativeLayout.SelectFoundMatchDeadlineRefresh(2, 2, 100)
    && !MatrixPvpQueueNativeLayout.SelectFoundMatchDeadlineRefresh(2, 4, 0)
    && MatrixPvpQueueNativeLayout.ComputeFoundMatchDeadline(uint.MaxValue - 999, 1) == 0);
var cancellationBytes = Convert.FromHexString("300807060504030201");
var invitationResponseBytes = Convert.FromHexString("310807060504030201FF8000FE");
var kickedBytes = Convert.FromHexString("3208070605040302014100FFFFFFFFFFFFFFFFFF8000");
Check("Messages48-50 decode cancellation, raw invitation-response status, and kicked-player fields",
    MatrixServerMessages48To50Wire.Decode(cancellationBytes)
        is { BytesConsumed: 9, Payload: MatrixChallengeInvitationCancelPayload { ChallengeId: 0x0102030405060708 } }
    && MatrixServerMessages48To50Wire.Decode(invitationResponseBytes)
        is { Payload: MatrixChallengeInvitationResponsePayload { StatusByte: 0xFE } } responseRead
    && ((MatrixChallengeInvitationResponsePayload)responseRead.Payload).NameBytes.Span.SequenceEqual(new byte[] { 0xFF, 0x80 })
    && MatrixServerMessages48To50Wire.Decode(kickedBytes)
        is { Payload: MatrixChallengeKickedPayload { ChallengeId: 0x0102030405060708, KickeeId: ulong.MaxValue } } kickedRead
    && ((MatrixChallengeKickedPayload)kickedRead.Payload).KickerNameBytes.Span.SequenceEqual(new byte[] { 0x41 })
    && ((MatrixChallengeKickedPayload)kickedRead.Payload).KickeeNameBytes.Span.SequenceEqual(new byte[] { 0xFF, 0x80 }));
Check("Messages49/50 empty text minima are eleven/nineteen bytes and all body prefixes fail",
    MatrixServerMessages48To50Wire.Decode(new byte[] { 49 }.Concat(new byte[10]).ToArray()).BytesConsumed == 11
    && MatrixServerMessages48To50Wire.Decode(new byte[] { 50 }.Concat(new byte[18]).ToArray()).BytesConsumed == 19
    && new[] { cancellationBytes, invitationResponseBytes, kickedBytes }.All(packet =>
        Enumerable.Range(0, packet.Length).All(length =>
            MatrixServerMessages48To50Wire.Decode(packet.AsSpan(0, length)).Outcome != MatrixServer48To50ReadOutcome.Accepted)
        && MatrixServerMessages48To50Wire.Decode(packet.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == packet.Length)
    && MatrixServerMessages48To50Wire.Decode(new byte[] { 0, 48, 0 }).Outcome == MatrixServer48To50ReadOutcome.NonCanonicalHeader
    && MatrixServerMessages48To50Wire.Decode(new byte[] { 47 }).Outcome == MatrixServer48To50ReadOutcome.UnexpectedMessageCode);
var expectedChallengeResponseReasons = new[]
{
    "Accepted", "Declined", "DoesNotExist", "Offline", "TeamFull", "MatchStarting",
    "TargetNotChallengeable", "SquadTeamFull"
};
Check("Invitation response accepted is strictly status0 and reason uses all eight native entries or explicit UNKNOWN",
    Enumerable.Range(0, 256).All(status =>
    {
        var notification = MatrixChallengeFollowupNativeLayout.CaptureResponse(new byte[] { 0x80 }, (byte)status);
        return notification.RuntimeMessageId == 272 && notification.Accepted == (status == 0)
            && notification.NameBytes.Span.SequenceEqual(new byte[] { 0x80 })
            && notification.Reason == (status < 8 ? expectedChallengeResponseReasons[status] : "UNKNOWN");
    }));
var invitationSnapshot = new[]
{
    new MatrixChallengeInvitationSnapshot(new byte[] { 1 }, 7),
    new MatrixChallengeInvitationSnapshot(new byte[] { 2 }, 7),
    new MatrixChallengeInvitationSnapshot(new byte[] { 3 }, 0xFFFFFFFFFFFFFF07),
    new MatrixChallengeInvitationSnapshot(new byte[] { 4 }, 7)
};
Check("Invitation cancellation selects every exact64-bit match, including adjacent duplicates",
    MatrixChallengeFollowupNativeLayout.SelectCancellationMatches(invitationSnapshot, 7).SequenceEqual(new[] { 0, 1, 3 })
    && MatrixChallengeFollowupNativeLayout.SelectCancellationMatches(invitationSnapshot, 8).Count == 0
    && invitationSnapshot.Length == 4);
Check("Kicked handler requires current-mode/active pointer and exact ID; local comparison retains native zero fallback",
    MatrixChallengeFollowupNativeLayout.ActiveChallengeModeMemberOffset == 0xE4
    && MatrixChallengeFollowupNativeLayout.SelectKickedHandler(true, 7, 7)
    && MatrixChallengeFollowupNativeLayout.SelectKickedHandler(true, 0, 0)
    && !MatrixChallengeFollowupNativeLayout.SelectKickedHandler(false, 7, 7)
    && !MatrixChallengeFollowupNativeLayout.SelectKickedHandler(true, null, 7)
    && !MatrixChallengeFollowupNativeLayout.SelectKickedHandler(true, 7, 0xFFFFFFFFFFFFFF07)
    && MatrixChallengeFollowupNativeLayout.SelectLocalKickedBranch(7, 7)
    && !MatrixChallengeFollowupNativeLayout.SelectLocalKickedBranch(7, null)
    && MatrixChallengeFollowupNativeLayout.SelectLocalKickedBranch(0, null));
var challengeIdOnlyBytes = new[] { 51, 53, 55 }.Select(code =>
    new byte[] { (byte)code }.Concat(Convert.FromHexString("0807060504030201")).ToArray()).ToArray();
var challengeRecordBytes = Convert.FromHexString("0807060504030201FF8000FEFFFFFFFF80");
var challengeRecordsBytes = new byte[] { 52 }.Concat(Convert.FromHexString("0807060504030201"))
    .Concat(new byte[] { 2 }).Concat(challengeRecordBytes).Concat(challengeRecordBytes).ToArray();
var challengeWordBytes = Convert.FromHexString("360807060504030201FFFFFFFF");
Check("Challenge51/53/55 bodies are QWORD-only;54 appends one raw DWORD",
    challengeIdOnlyBytes.All(packet => MatrixServerMessages51To55Wire.Decode(packet)
        is { BytesConsumed: 9, Payload: MatrixChallengeIdOnlyPayload { ChallengeId: 0x0102030405060708 } } result
        && ((MatrixChallengeIdOnlyPayload)result.Payload).MessageCode == packet[0])
    && MatrixServerMessages51To55Wire.Decode(challengeWordBytes)
        is { BytesConsumed: 13, Payload: MatrixChallengeWordUpdatePayload { WordAt30: uint.MaxValue } });
var challengeRecordsRead = MatrixServerMessages51To55Wire.Decode(challengeRecordsBytes);
Check("Challenge52 native24-byte records contain a QWORD, raw NUL text, byte, DWORD and byte without padding",
    challengeRecordsRead.Outcome == MatrixServer51To55ReadOutcome.Accepted
    && challengeRecordsRead.BytesConsumed == challengeRecordsBytes.Length
    && challengeRecordsRead.Payload is MatrixChallengeRecordListPayload { Records.Count: 2 } challengeRecords
    && challengeRecords.Records[0].QwordAt0 == 0x0102030405060708
    && challengeRecords.Records[0].TextBytesAt8.Span.SequenceEqual(new byte[] { 0xFF, 0x80 })
    && challengeRecords.Records[0].ByteAtC == 0xFE && challengeRecords.Records[0].WordAt10 == uint.MaxValue
    && challengeRecords.Records[0].ByteAt14 == 0x80
    && MatrixServerMessages51To55Wire.Decode(new byte[] { 52 }.Concat(new byte[9]).ToArray()).BytesConsumed == 10);
Check("Challenge51-55 all strict prefixes fail and trailing bytes do not enlarge consumed length",
    challengeIdOnlyBytes.Concat(new[] { challengeRecordsBytes, challengeWordBytes }).All(packet =>
        Enumerable.Range(0, packet.Length).All(length =>
            MatrixServerMessages51To55Wire.Decode(packet.AsSpan(0, length)).Outcome != MatrixServer51To55ReadOutcome.Accepted)
        && MatrixServerMessages51To55Wire.Decode(packet.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == packet.Length)
    && MatrixServerMessages51To55Wire.Decode(new byte[] { 0, 51, 0 }).Outcome == MatrixServer51To55ReadOutcome.NonCanonicalHeader
    && MatrixServerMessages51To55Wire.Decode(new byte[] { 50 }).Outcome == MatrixServer51To55ReadOutcome.UnexpectedMessageCode);
var challengeRecords255 = challengeRecordsBytes[..9].Concat(new byte[] { 255 })
    .Concat(Enumerable.Range(0, 255).SelectMany(_ => challengeRecordBytes)).Concat(new byte[] { 0 }).ToArray();
Check("Challenge52 records require terminal zero after count255 and preserve all entries",
    MatrixServerMessages51To55Wire.Decode(challengeRecords255)
        is { Payload: MatrixChallengeRecordListPayload { Records.Count: 255 } }
    && MatrixServerMessages51To55Wire.Decode(challengeRecords255[..^1]).Outcome == MatrixServer51To55ReadOutcome.TruncatedPayload);
Check("Challenge51/52/54 accept invalid stored ID as well as exact match, but53/55 do not",
    new uint[] { 51, 52, 54 }.All(code =>
        MatrixChallengeUpdateNativeLayout.SelectHandler(code, true, 0, 7).Action != MatrixChallengeUpdateAction.Ignore
        && MatrixChallengeUpdateNativeLayout.SelectHandler(code, true, 0xFFFFFFFFFFFFFF80, 7).Action != MatrixChallengeUpdateAction.Ignore)
    && new uint[] { 53, 55 }.All(code =>
        MatrixChallengeUpdateNativeLayout.SelectHandler(code, true, 0, 7).Action == MatrixChallengeUpdateAction.Ignore)
    && Enumerable.Range(51, 5).All(code =>
        MatrixChallengeUpdateNativeLayout.SelectHandler((uint)code, true, 7, 8).Action == MatrixChallengeUpdateAction.Ignore
        && MatrixChallengeUpdateNativeLayout.SelectHandler((uint)code, false, 7, 7).Action == MatrixChallengeUpdateAction.Ignore
        && MatrixChallengeUpdateNativeLayout.SelectHandler((uint)code, true, null, 7).Action == MatrixChallengeUpdateAction.Ignore));
Check("Challenge51-55 select their distinct release/copy/word/notify and runtime boundaries",
    MatrixChallengeUpdateNativeLayout.SelectHandler(51, true, 7, 7) == new MatrixChallengeUpdateSelection(MatrixChallengeUpdateAction.ReleaseActiveChallenge, 275)
    && MatrixChallengeUpdateNativeLayout.SelectHandler(52, true, 7, 7) == new MatrixChallengeUpdateSelection(MatrixChallengeUpdateAction.ReplaceRecords, 273)
    && MatrixChallengeUpdateNativeLayout.SelectHandler(53, true, 7, 7) == new MatrixChallengeUpdateSelection(MatrixChallengeUpdateAction.NotifyOnly, 276)
    && MatrixChallengeUpdateNativeLayout.SelectHandler(54, true, 7, 7) == new MatrixChallengeUpdateSelection(MatrixChallengeUpdateAction.SelectWord, 277)
    && MatrixChallengeUpdateNativeLayout.SelectHandler(55, true, 7, 7) == new MatrixChallengeUpdateSelection(MatrixChallengeUpdateAction.NotifyOnly, 278));
var challengeSelectionRecords = new[]
{
    new MatrixChallengeJoinEntry(5, default, default, 0, 0, 0),
    new MatrixChallengeJoinEntry(7, default, default, 0, 0, 0),
    new MatrixChallengeJoinEntry(7, default, default, 0, 0, 0)
};
Check("Challenge word selection picks first matching16-byte record or raw all-FF index and retains notification flag",
    MatrixChallengeUpdateNativeLayout.CaptureWordSelection(7, challengeSelectionRecords, true)
        == new MatrixChallengeWordSelection(7, 1, true)
    && MatrixChallengeUpdateNativeLayout.CaptureWordSelection(8, challengeSelectionRecords, false)
        == new MatrixChallengeWordSelection(8, uint.MaxValue, false)
    && MatrixChallengeUpdateNativeLayout.CaptureWordSelection(7, Array.Empty<MatrixChallengeJoinEntry>(), true).IndexAt24 == uint.MaxValue);
Check("tfLogin auto-login keys and endpoint template are recorded",
    LoginModeNativeLayout.KnownAutoLoginSettings.SequenceEqual(
    [
        "MaxTimes", "Account", "SavedPassword", "CharacterName",
        "DevMatrixAddress", "SpectatorMode", "SpectatorDirectorControl",
        "SpectatorTargetGuid", "RandomReplay"
    ])
    && LoginModeNativeLayout.LoginEndpointFormat == "%s/api/v2/accounts/login");

MatrixServer56To61Checks.Run(Check);
MatrixServer63To68Checks.Run(Check);
MatrixServer34And36Checks.Run(Check);
MatrixChatDisplayChecks.Run(Check);
MatrixChatLogChecks.Run(Check);
MatrixChatFileChecks.Run(Check);
MatrixChatIdentityChecks.Run(Check);
MatrixRequestSigscan32Checks.Run(Check);
MatrixSynchronization57Checks.Run(Check);
GameSocketAcknowledgmentBodyChecks.Run(Check);
MatrixServerMessageNativeTypeChecks.Run(Check);
MatrixSynchronizationDispatchChecks.Run(Check);
NativeMt19937Checks.Run(Check);
NativeByteVectorHeapChecks.Run(Check);
NativeByteVectorSortChecks.Run(Check);
NativeAdapterAddressFingerprintChecks.Run(Check);
NativeWindowsDriveSerialFingerprintChecks.Run(Check);
MatrixSynchronizationModeZeroDigestSlotsChecks.Run(Check);
MatrixModeZeroDescriptorChecks.Run(Check);
NativeMetadataTextTailChecks.Run(Check);
NativeFolderPrefixAliasChecks.Run(Check);
NativeModulePathProviderChecks.Run(Check);
NativeGeneratedTransformProviderPrefixChecks.Run(Check);
NativeGeneratedDescriptorFactoryChecks.Run(Check);
NativeGeneratedBodyDecisionChecks.Run(Check);
NativeGeneratedConditionalBranchChecks.Run(Check);
NativeGeneratedByteLoopWrapperChecks.Run(Check);
NativeGeneratedScratchTemplateChecks.Run(Check);
NativeGeneratedArithmeticTemplateChecks.Run(Check);
NativeGeneratedInitialModeZeroImageChecks.Run(Check);
NativeGeneratedArenaPolicyChecks.Run(Check);
NativeGeneratedModeOneBoundaryChecks.Run(Check);
NativeGeneratedModeOneMemoryShapeChecks.Run(Check);
NativeGeneratedModeOneMemoryEncodingChecks.Run(Check);
NativeGeneratedModeOneImmediateMoveChecks.Run(Check);
NativeGeneratedModeOneUnaryChecks.Run(Check);
NativeGeneratedModeOneArithmeticChecks.Run(Check);
NativeGeneratedModeOneWrapperChecks.Run(Check);
NativeGeneratedDefaultHeaderImageChecks.Run(Check);
MatrixServerProfilerNamesChecks.Run(Check);
MatrixServerProfilerFrameChecks.Run(Check);
NativeProfilerSampleChecks.Run(Check);
NativeProfilerTreeChecks.Run(Check);
NativeProfilerFrameRefreshChecks.Run(Check);
NativeProfilerDisplayNameChecks.Run(Check);
NativeProfilerNodeSnapshotChecks.Run(Check);
NativeProfilerNameRequestChecks.Run(Check);

if (args.Length != 0)
{
    if (args.Length != 2 || args[0] != "--binary")
    {
        Console.Error.WriteLine("Usage: Startup.Checks [--binary <FirefallClient.exe path>]");
        return 2;
    }
    StartupBinaryEvidence.Verify(args[1], Check);
}

return failures == 0 ? 0 : 1;
