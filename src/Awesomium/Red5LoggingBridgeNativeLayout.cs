namespace Firefall.Reconstruction.Awesomium;

public enum Red5LoggingBridgeEntryAction
{
    ReturnReady,
    ReturnUnavailableAfterPriorFailure,
    AttemptCreation
}

public enum Red5LoggingBridgeCreationAction
{
    RegisterErrorLogMethod,
    MarkUnavailableAndLogFailure
}

public enum Red5MethodCallbackAction
{
    QueueMappedMethodCallback,
    LogErrorMessage,
    LogUnknownCallback,
    ReturnWithoutLogging
}

public enum Red5MappedCallbackArgumentAction
{
    PreserveAllArguments,
    SeparateTrailingCallableLikeValue
}

public enum Red5LuaCallbackRegistrationAction
{
    RejectUnavailableContextOrInvalidReference,
    RejectUnsupportedMethodName,
    RegisterNewMethod,
    ReplaceExistingMethod
}

public enum Red5LuaWebBindingAction
{
    ReportUsage,
    RejectUnsupportedNameType,
    DispatchToWebObject
}

public enum Red5LuaWebFunctionInvocationAction
{
    ReturnWithoutInvocationForUnavailableCallbacksObject,
    ReturnWithoutInvocationForMissingFunction,
    InvokeSynchronously,
    InvokeAsynchronously
}

public enum Red5LuaWebValueAction
{
    ReturnWithoutSettingForUnavailableObject,
    SetPropertyAsynchronously
}

public enum Red5LuaUrlFilterAction
{
    LogAndSkip,
    StoreInReplacementList
}

public enum Red5UrlFilterCallbackAction
{
    ReturnFalseForUnchangedContext,
    ReturnFalseForMatchingFilter,
    ReturnTrueForNoMatchingFilter
}

public enum Red5DispatchObjectDeleteAction
{
    ResetAndPreserveAllocation,
    ResetAndReleaseAllocation
}

public readonly record struct Red5ParsedUrl(
    bool IsHttps,
    string Host,
    int Port,
    string Path,
    string Query);

public readonly record struct ClientApiServerClockOffsetEntry(string Key, int OffsetSeconds);
public readonly record struct ClientApiRequestTimerDefaults(uint Tick, uint PreviousTick);

public enum ClientApiResponseObservedKind : uint
{
    WorkerInitialized = 0,
    ResetDefault = 1,
    HandleClosing = 3,
    CloseAfterStatusDispatch = 4
}

public enum ClientApiWinHttpCallbackStatus : uint
{
    Redirect = 0x4000,
    SecureFailure = 0x10000,
    HeadersAvailable = 0x20000,
    DataAvailable = 0x40000,
    ReadComplete = 0x80000,
    WriteComplete = 0x100000,
    RequestError = 0x200000
}

public static class ClientApiWinHttpErrorResultLayout
{
    public const uint AsyncResultSize = 8;
    public const uint ErrorCodeOffset = 4;
    public const int FailureFallbackHResult = unchecked((int)0x80004005u);

    public static int CaptureFailureHResult(int errorCode)
    {
        int result = errorCode > 0
            ? unchecked((int)(0x80070000u | ((uint)errorCode & 0xFFFF)))
            : errorCode;
        return result >= 0 ? FailureFallbackHResult : result;
    }
}

public static class ClientApiWinHttpSecureFailureLayout
{
    public const uint StatusDataSize = 4;
    public const uint LabeledFlagMask = 0x8000007F;
    public const uint FailureErrorCode = 12175;

    public static uint CaptureLabeledFlags(uint flags) => flags & LabeledFlagMask;
}

public static class ClientApiWinHttpPreSendFailureLayout
{
    public const uint RequestHandleOffset = 0;
    public const uint ResultOffset = 0x14;
    public const uint ResponseKindOffset = 8;
    public const uint ResponseKind = 4;

    public static bool ShouldSubmitRequest(int storedFailure) => storedFailure == 0;
}

public static class ClientApiWinHttpDataAvailableLayout
{
    public const uint BytesAvailableOffset = 0;
    public const uint StatusDataSize = 4;
    public const uint ResponseKindOffset = 8;
    public const uint EmptyBodyResponseKind = 1;

    public static bool ShouldStartRead(uint bytesAvailable) => bytesAvailable != 0;

    public static uint CaptureResponseKindOnEmptyData(uint existingKind) =>
        existingKind == 0 ? EmptyBodyResponseKind : existingKind;
}

public static class ClientApiWinHttpOpenRequestFlagLayout
{
    public const uint HttpsSchemeOffset = 0x0C;
    public const uint BypassProxyCacheOffset = 0x60;
    public const uint SecureFlag = 0x00800000;
    public const uint RefreshFlag = 0x00000100;

    public static uint CaptureFlags(uint httpsSchemeFlag, byte bypassProxyCache) =>
        (httpsSchemeFlag == 1 ? SecureFlag : 0)
        | (bypassProxyCache != 0 ? RefreshFlag : 0);
}

public static class ClientApiRouteWorkerTimeoutLayout
{
    public const uint CompletionOffset = 0x1C;
    public const uint TimestampOffset = 0x28;
    public const uint TimestampSize = 8;
    public const ulong TimeoutMicroseconds = 120_000_000;

    public static bool ShouldExpire(bool hasCompletion, ulong nowMicroseconds, ulong startMicroseconds) =>
        !hasCompletion && unchecked(nowMicroseconds - startMicroseconds) > TimeoutMicroseconds;
}

public static class ClientApiRouteWorkerPoolLayout
{
    public const uint EnabledOffset = 0x20;
    public const uint WorkerLimitOwnerOffset = 0x28;
    public const uint WorkerLimitOffsetFromOwner = 0x24;
    public const uint WorkerSize = 0x30;

    public static bool ShouldAppendWorker(bool enabled, uint workersWithCompletion,
        uint workerCount, uint workerLimit) =>
        enabled && workersWithCompletion >= workerCount && workerCount < workerLimit;
}

public static class ClientApiRoutePumpThreadLayout
{
    public const uint EnabledOffset = 5;
    public const uint StopOffset = 4;
    public const uint WaitTimeoutMilliseconds = 3000;
    public const uint ActiveRouteCountOffset = 176;
}

/// <summary>
/// Observed redirect header-replay gate. Host comparison models ASCII
/// case-insensitivity; CRT locale-specific byte behavior is not simulated.
/// </summary>
public static class ClientApiRedirectHeaderReplayPolicy
{
    public static bool ShouldReplay(string requestHost, string redirectedHost, uint headerCount,
        bool replayAlreadyAttempted) =>
        headerCount != 0
        && !replayAlreadyAttempted
        && !string.Equals(requestHost, redirectedHost, StringComparison.OrdinalIgnoreCase);
}

public enum Red5LuaWebRequestLoggingAction
{
    Disable,
    Enable
}

public enum Red5LuaLoadUrlAction
{
    ReportInvalidFrame,
    ReportInvalidUrlValue,
    ReturnWithoutLoading,
    LoadAndReturnTrue
}

public enum Red5NaturalDimensionsAction
{
    KeepExistingDimensions,
    UpdateAndNotify
}

public enum Red5TypeTargetFlagAction
{
    LeaveUnchanged,
    UpdateFlag
}

public enum Red5QueuedJavaScriptContinuationAction
{
    SkipUndefinedContinuation,
    InvokeSynchronously,
    InvokeAsynchronously
}

public enum Red5BridgePointerCleanupAction
{
    PreserveSharedSentinel,
    ClearNullPointer,
    ReleaseAndClearPointer
}

/// <summary>Observed Red5 setup and callback decisions; invokes no browser or logging APIs.</summary>
public static class Red5LoggingBridgeNativeLayout
{
    public const uint InitializerRaw = 0xD668D0;
    public const uint CallbackHandlerRaw = 0xD65A60;
    public const uint CallbackHandlerVtableVa = 0x1C7656C;
    public const uint CallbackQueueConsumerRaw = 0xD64AE0;
    public const uint LuaAddWebCallbackRaw = 0xCCAD70;
    public const uint LuaCallbackRegistrationRaw = 0xCCC130;
    public const uint LuaCallbackReferenceCreateRaw = 0xC56170;
    public const uint LuaCallbackReferenceReleaseRaw = 0xC56250;
    public const uint LuaAddWebValueRaw = 0xCCAF10;
    public const uint LuaCallWebFuncRaw = 0xCCB060;
    public const uint LuaWebFrameBindingTableRaw = 0xCCB9A0;
    public const uint LuaLoadUrlRaw = 0xCCABA0;
    public const uint LuaGetUrlRaw = 0xCCACC0;
    public const uint LuaLoadUrlImplementationRaw = 0xD65690;
    public const uint LuaSetUrlFiltersRaw = 0xCCB1C0;
    public const uint LuaLogWebRequestsRaw = 0xCCB3D0;
    public const uint LuaClearCookiesRaw = 0xCCB4E0;
    public const uint LuaWebFrameInitializerRaw = 0xCCBD90;
    public const uint LuaWebFrameInternalConstructorRaw = 0xD64060;
    public const uint LuaClearCookiesForwardingThunkRaw = 0xD65A10;
    public const uint LuaSetNaturalDimsRaw = 0xCCB5B0;
    public const uint LuaGetNaturalDimsRaw = 0xCCB710;
    public const uint LuaEnableTypeTargetRaw = 0xCCB7C0;
    public const uint LuaIsTypeTargetRaw = 0xCCB8C0;
    public const uint LuaUrlFilterCallbackRaw = 0xCBDDE0;
    public const uint LuaUrlFilterListMatcherRaw = 0xCCCC10;
    public const uint LuaUrlFilterListUpdateRaw = 0xCCCB20;
    public const uint LuaEnableTypeTargetHelperRaw = 0xCCD0F0;
    public const uint LuaUrlFilterCandidateTextHelperRaw = 0xCBB5E0;
    public const uint LuaWebValueSetterRaw = 0xD651B0;
    public const uint LuaWebFunctionDispatcherRaw = 0xD65250;
    public const uint LuaWebFrameContentObjectOffset = 0x418;
    public const uint LuaWebFrameInternalObjectOffset = 0x41C;
    public const uint LuaWebFrameInternalContextOffset = 0x1C;
    public const uint LuaWebFrameInternalWebViewOffset = 0x20;
    public const uint LuaWebFrameUrlOffset = 0x448;
    public const uint LuaWebFrameTypeTargetSourceOffset = 0x398;
    public const uint LuaWebRequestLoggingFlagOffset = 0x458;
    public const uint LuaWebFrameTypeTargetFlagOffset = 0x459;
    public const uint LuaWebFrameNaturalWidthOffset = 0x45C;
    public const uint LuaWebFrameNaturalHeightOffset = 0x460;
    public const uint LuaWebViewLoadUrlSlotByteOffset = 0x5C;
    public const uint LuaWebViewUrlLoadSlotByteOffset = 0x64;
    public const uint LuaWebViewPrepareSlotByteOffset = 0x74;
    public const float LuaNaturalFitScaleDimensionThreshold = 1f;
    public const float LuaNaturalFitScaleMinimum = 0.2f;
    public const float LuaNaturalFitScaleMaximum = 5f;
    public const float LuaNaturalFitScalePercentMultiplier = 100f;
    public const float LuaNaturalFitScaleRoundingBias = 0.5f;
    public const uint LuaWebFrameVtableVa = 0x1C765FC;
    public const uint LuaWebFrameLoadUrlVtableSlotVa = 0x1C76658;
    public const uint LuaClearCookiesVtableSlotVa = 0x1C76664;
    public const uint LuaClearCookiesNestedMethodSlotByteOffset = 0x04;
    public const uint LuaWebFrameTargetDataOffset = 0x730;
    public const uint LuaTypeTargetDispatcherGlobalVa = 0x1E002B8;
    public const uint LuaLocalUrlPrefixVa = 0x1BD49F8;
    public const uint LuaLocalUrlSeparatorVa = 0x1BD4988;
    public const uint LuaWebFrameViewportWidthOffset = 0x420;
    public const uint LuaWebFrameViewportHeightOffset = 0x424;
    public const uint LuaWebFrameScalePercentOffset = 0x464;
    public const uint LuaValueToJavaScriptRaw = 0xCBBDB0;
    public const uint LuaCallbackArgumentConversionRaw = 0xCBBE00;
    public const uint LuaCallbackInvocationWrapperRaw = 0xCD6C10;
    public const uint CallbackBridgeCleanupRaw = 0xD64710;
    public const uint CallbackBridgeDestructorRaw = 0xD64250;
    public const uint CallbackBridgeDeletingDestructorRaw = 0xD64180;
    public const uint CallbackQueuedRecordCleanupRaw = 0xD648A0;
    public const uint CallbackQueueConsumerRttiLocatorPointerVa = 0x1C765F8;
    public const uint CallbackQueueConsumerVtableSlotVa = 0x1C7664C;
    public const uint CallbackQueueConsumerClassDescriptorVa = 0x1E2E238;
    public const uint CallbackRegistrationVtableSlotVa = 0x1C7662C;
    public const uint CallbackMethodLookupRaw = 0xCCD640;
    public const uint CallbackCallablePredicateRaw = 0xD64320;
    public const uint CallbackQueueAppendRaw = 0xD66B70;
    public const uint SetCustomMethodImportVa = 0x1AC4EE0;
    public const uint SetPropertyAsyncImportVa = 0x1AC4EF0;
    public const uint CallbackIsUndefinedImportVa = 0x1AC4EE8;
    public const uint CallbackInvokeImportVa = 0x1AC4EDC;
    public const uint CallbackInvokeAsyncImportVa = 0x1AC4EEC;
    public const uint LuaWebFunctionHasMethodImportVa = 0x1AC4EF4;
    public const uint Red5ObjectNameVa = 0x1BD4BAC;
    public const uint ErrorLogMethodNameVa = 0x1BD4B98;
    public const uint CallbackErrorLogMethodNameVa = 0x1BD4994;
    public const uint CallbackMessageFormatVa = 0x1BD498C;
    public const uint CallbackUnknownFormatVa = 0x1BD48F0;
    public const uint CallbackMethodCallNameVa = 0x1BD4544;
    public const uint CallbackCallablePredicateVa = 0x1BD4570;
    public const uint CallbackQueueCallMethodNameVa = 0x1BD47B4;
    public const uint CallbackQueueLogFormatVa = 0x1BD478C;
    public const uint CallbackQueueArgumentLogFormatVa = 0x1BD47C0;
    public const uint CallbackQueueReturnLogFormatVa = 0x1BD4730;
    public const uint CallbackQueueAsyncStateVa = 0x1E686C4;
    public const uint LuaCallWebFuncUsageVa = 0x1BC761C;
    public const uint LuaAddWebValueUsageVa = 0x1BC7734;
    public const uint LuaWebFunctionTargetExpressionVa = 0x1BD46B8;
    public const uint LuaWebFunctionArgumentLogFormatVa = 0x1BD4754;
    public const uint LuaWebFunctionCallLogFormatVa = 0x1BD4760;
    public const uint LuaWebFunctionReturnLogFormatVa = 0x1BD49D4;
    public const uint LuaWebFunctionMissingFormatVa = 0x1BD49A8;
    public const uint LuaWebCallbacksUnavailableFormatVa = 0x1BD4A10;
    public const uint LuaLogWebRequestsUsageVa = 0x1BC7584;
    public const uint LuaClearCookiesUsageVa = 0x1BC75D4;
    public const uint LuaLoadUrlInvalidElementVa = 0x1BC7484;
    public const uint LuaGetUrlInvalidElementVa = 0x1BC76E4;
    public const uint LuaGetNaturalDimsInvalidElementVa = 0x1BC750C;
    public const uint LuaEnableTypeTargetUsageVa = 0x1BC7534;
    public const uint LuaEnableTypeTargetInvalidElementVa = 0x1BC780C;
    public const uint LuaIsTypeTargetUsageVa = 0x1BC7834;
    public const uint LuaIsTypeTargetInvalidElementVa = 0x1BC7858;
    public const uint LuaGetUrlEmptyFallbackVa = 0x1C5EA68;
    public const uint LuaSetUrlFiltersUsageVa = 0x1BC7698;
    public const uint LuaUrlFilterWildcardVa = 0x1BC78C4;
    public const uint LuaUrlFilterIllegalFormatVa = 0x1BC78C8;
    public const uint LuaUrlFilterListOffset = 0x468;
    public const uint LuaUrlFilterStringCompareImportVa = 0x1AC4894;
    public const uint LuaUrlFilterUrlParseCompareImportVa = 0x1AC48A8;
    public const uint LuaUrlFilterUrlParseCopyImportVa = 0x1AC486C;
    public const uint LuaUrlFilterUrlParseFindCharacterImportVa = 0x1AC48C8;
    public const uint LuaUrlFilterUrlParsePortImportVa = 0x1AC48D4;
    public const uint LuaUrlFilterUrlParseRaw = 0xBCE800;
    public const uint LuaUrlFilterUrlParseHttpSchemeVa = 0x1BB625C;
    public const uint LuaUrlFilterUrlParseHttpsSchemeVa = 0x1BB6264;
    public const uint LuaUrlFilterUrlParseDefaultPathVa = 0x1BB626C;
    public const uint ClientApiRequestUrlLookupRaw = 0xBC1460;
    public const uint ClientApiRequestUrlLookupUpdateRaw = 0xBC1680;
    public const uint ClientApiRequestResetRaw = 0xBC1280;
    public const uint ClientApiRequestAutomaticRedirectLimitOffset = 0x3C;
    public const uint ClientApiRequestAutomaticRedirectLimitDefault = 5;
    public const uint ClientApiWinHttpSetAutomaticRedirectLimitRaw = 0xBC2F44;
    public const uint ClientApiWinHttpSetOptionImportVa = 0x1AC4D08;
    public const uint ClientApiWinHttpGetProxyForUrlRaw = 0xBC2E7F;
    public const uint ClientApiWinHttpQueryProxyInfoRaw = 0xBC2EB9;
    public const uint ClientApiWinHttpSetProxyInfoRaw = 0xBC2EEB;
    public const uint ClientApiWinHttpGetProxyForUrlImportVa = 0x1AC4D00;
    public const uint ClientApiWinHttpQueryOptionImportVa = 0x1AC4D04;
    public const uint ClientApiWinHttpProxyOption = 0x26;
    public const uint ClientApiWinHttpProxyInfoSize = 0x0C;
    public const uint ClientApiWinHttpAutoproxyConfigUrlFlag = 0x02;
    public const uint ClientApiProxyStringGlobalFreeImportVa = 0x1AC4320;
    public const uint ClientApiWinHttpProxyStringFreeOneRaw = 0xBC2F18;
    public const uint ClientApiWinHttpProxyStringFreeTwoRaw = 0xBC2F25;
    public const uint ClientApiWinHttpSessionOpenRaw = 0xBC3A4B;
    public const uint ClientApiWinHttpSessionEnableFeatureRaw = 0xBC3A71;
    public const uint ClientApiWinHttpSessionSecureProtocolsRaw = 0xBC3A9A;
    public const uint ClientApiWinHttpSessionPlatformCheckRaw = 0xBC39DA;
    public const uint ClientApiWinHttpOpenImportVa = 0x1AC4D1C;
    public const uint ClientApiWinHttpCheckPlatformImportVa = 0x1AC4D18;
    public const uint ClientApiWinHttpEnableFeatureOption = 79;
    public const uint ClientApiWinHttpSslRevertImpersonationFeature = 2;
    public const uint ClientApiWinHttpSecureProtocolsOption = 84;
    public const uint ClientApiWinHttpTls10Protocol = 0x80;
    public const uint ClientApiWinHttpOpenAsyncFlag = 0x10000000;
    public const uint ClientApiRequestUrlSchemeOffset = 0x0C;
    public const uint ClientApiRequestUrlHostOffset = 0x10;
    public const uint ClientApiRequestUrlPortOffset = 0x14;
    public const uint ClientApiRequestUrlPathOffset = 0x18;
    public const uint ClientApiRequestUrlQueryOffset = 0x1C;
    public const uint ClientApiRequestUrlParserRaw = 0xBCE800;
    public const uint ClientApiRequestUrlHostLowercaseCallRaw = 0xBCE99A;
    public const uint ClientApiRequestUrlHostLowercaseHelperRaw = 0x12B9B00;
    public const uint ClientApiRequestHostLowercaseImportVa = 0x1AC4978;
    public const uint ClientApiRequestHeaderTreeHeadOffset = 0x20;
    public const uint ClientApiRequestHeaderCountOffset = 0x24;
    public const uint ClientApiRequestHeaderInsertRaw = 0xBC1990;
    public const uint ClientApiRequestTreeCountIncrementRaw = 0x31E95F;
    public const uint ClientApiResponseResetRaw = 0xBC11D0;
    public const uint ClientApiResponseCtorRaw = 0x3845D0;
    public const uint ClientApiResponseDestroyRaw = 0x3844B0;
    public const uint ClientApiResponseTreeDestroyRaw = 0x326EF0;
    public const uint ClientApiResponseSize = 0x20;
    public const uint ClientApiResponseRequestOffset = 0x04;
    public const uint ClientApiResponseKindOffset = 0x08;
    public const uint ClientApiWinHttpStatusCallbackRaw = 0xBC26E0;
    public const uint ClientApiWinHttpStatusDispatchRaw = 0xBC1DA0;
    public const uint ClientApiWinHttpHandleClosingStatus = 0x800;
    public const uint ClientApiWinHttpCloseHandleImportVa = 0x1AC4CF8;
    public const uint ClientApiWinHttpReceiveResponseImportVa = 0x1AC4CE0;
    public const uint ClientApiWinHttpQueryHeadersImportVa = 0x1AC4CE4;
    public const uint ClientApiWinHttpQueryDataAvailableImportVa = 0x1AC4CE8;
    public const uint ClientApiWinHttpReadDataImportVa = 0x1AC4CEC;
    public const uint ClientApiWinHttpCrackUrlImportVa = 0x1AC4CF0;
    public const uint ClientApiWinHttpRedirectHostCompareImportVa = 0x1AC48A8;
    public const uint ClientApiWinHttpRedirectCrackUrlRaw = 0xBC1F8C;
    public const uint ClientApiWinHttpRedirectHostCompareRaw = 0xBC1FF0;
    public const uint ClientApiWinHttpRedirectHostCompareHelperRaw = 0x2FC540;
    public const uint ClientApiWinHttpRedirectHeaderFlagsRaw = 0xBC207B;
    public const uint ClientApiWinHttpRedirectAddHeadersRaw = 0xBC2085;
    public const uint ClientApiWinHttpRedirectReplayFlagStoreRaw = 0xBC20B5;
    public const uint ClientApiWinHttpSetStatusCallbackImportVa = 0x1AC4D0C;
    public const uint ClientApiWinHttpSendRequestImportVa = 0x1AC4D10;
    public const uint ClientApiWinHttpCallbackContextAllocRaw = 0xBC2D0E;
    public const uint ClientApiWinHttpCallbackContextInitRaw = 0xBC2D6B;
    public const uint ClientApiWinHttpWorkerDetachRaw = 0xBC2890;
    public const uint ClientApiWinHttpWorkerDetachContextCleanupFlagStoreRaw = 0xBC28CB;
    public const uint ClientApiWinHttpWorkerDetachHandleCloseRaw = 0xBC28D7;
    public const uint ClientApiWinHttpCallbackContextCleanupFlagTestRaw = 0xBC2724;
    public const uint ClientApiWinHttpCallbackContextWorkerStoreRaw = 0xBC3280;
    public const uint ClientApiWinHttpRequestErrorCompareRaw = 0xBC2580;
    public const uint ClientApiWinHttpRequestErrorLengthCheckRaw = 0xBC2599;
    public const uint ClientApiWinHttpRequestErrorCodeLoadRaw = 0xBC25BB;
    public const uint ClientApiWinHttpFailureHelperRaw = 0xBC1C40;
    public const uint ClientApiWinHttpFailureHResultMapRaw = 0xBC1D18;
    public const uint ClientApiWinHttpSecureFailureStatusCompareRaw = 0xBC1E15;
    public const uint ClientApiWinHttpSecureFailureLengthCheckRaw = 0xBC1E23;
    public const uint ClientApiWinHttpSecureFailureFlagLoadRaw = 0xBC1E2D;
    public const uint ClientApiWinHttpSecureFailureFlagTestsRaw = 0xBC1E55;
    public const uint ClientApiWinHttpSecureFailureReportRaw = 0xBC1F18;
    public const uint ClientApiWinHttpPreSendFailureLastErrorCallRaw = 0xBC31AD;
    public const uint ClientApiWinHttpPreSendFailureHelperCallRaw = 0xBC31BB;
    public const uint ClientApiWinHttpPreSendFailureResultStoreRaw = 0xBC31C3;
    public const uint ClientApiWinHttpPreSendFailureGateRaw = 0xBC31D8;
    public const uint ClientApiWinHttpPreSendRequestCallRaw = 0xBC3213;
    public const uint ClientApiWinHttpPreSendFailureHandleCloseRaw = 0xBC3222;
    public const uint ClientApiWinHttpPreSendFailureHandleClearRaw = 0xBC322E;
    public const uint ClientApiWinHttpPreSendFailureResponseKindStoreRaw = 0xBC3273;
    public const uint ClientApiWinHttpGetLastErrorImportVa = 0x1AC4378;
    public const uint ClientApiWinHttpDataAvailableEmptyBranchRaw = 0xBC2423;
    public const uint ClientApiWinHttpDataAvailableTerminalCallbackRaw = 0xBC2767;
    public const uint ClientApiWinHttpOpenRequestSecureFlagRaw = 0xBC2CCD;
    public const uint ClientApiWinHttpOpenRequestRefreshFlagRaw = 0xBC2CE3;
    public const uint ClientApiWinHttpOpenRequestRefreshFlagResetRaw = 0xBC131C;
    public const uint ClientApiWinHttpOpenRequestArgumentsRaw = 0xBC2D92;
    public const uint ClientApiWinHttpOpenRequestCallRaw = 0xBC2DB7;
    public const uint ClientApiWinHttpOpenRequestImportVa = 0x1AC4CFC;
    public const uint ClientApiWinHttpCallbackContextHandleStoreRaw = 0xBC2DEC;
    public const uint ClientApiWinHttpCallbackContextResultStoreRaw = 0xBC3264;
    public const uint ClientApiWinHttpCallbackContextResponseStoreRaw = 0xBC3270;
    public const uint ClientApiWinHttpCallbackContextSize = 0x1C;
    public const uint ClientApiWinHttpCallbackContextHandleOffset = 0x00;
    public const uint ClientApiWinHttpCallbackContextCleanupFlagOffset = 0x11;
    public const uint ClientApiWinHttpCallbackContextResultOffset = 0x14;
    public const uint ClientApiWinHttpCallbackContextResponseOffset = 0x18;
    public const uint ClientApiRequestTrackRaw = 0xBC43C0;
    public const uint ClientApiRequestSubmitTickStoreRaw = 0xBC4512;
    public const uint ClientApiRequestOpenTickStoreRaw = 0xBC2DE1;
    public const uint ClientApiRequestCallbackTickStoreRaw = 0xBC37D5;
    public const uint ClientApiRouteWorkerPumpRaw = 0xBC3F80;
    public const uint ClientApiRouteWorkerCompletionGateRaw = 0xBC3FD0;
    public const uint ClientApiRouteWorkerTimeoutCheckRaw = 0xBC3FE2;
    public const uint ClientApiRouteWorkerPoolGateRaw = 0xBC408C;
    public const uint ClientApiRouteWorkerAllocationRaw = 0xBC40DC;
    public const uint ClientApiRouteWorkerCtorCallRaw = 0xBC4101;
    public const uint ClientApiRouteWorkerAppendCallRaw = 0xBC411A;
    public const uint ClientApiRouteWorkerInitialPumpCallRaw = 0xBC4124;
    public const uint ClientApiRouteWorkerCtorRaw = 0xBC27A0;
    public const uint ClientApiRouteWorkerAppendRaw = 0xBC55C0;
    public const uint ClientApiRouteWorkerCtorTimestampSequenceRaw = 0xBC2831;
    public const uint ClientApiPumpRequestWorkerTimestampStoreRaw = 0xBC2C0F;
    public const uint ClientApiPumpRoutesRaw = 0xBC4550;
    public const uint ClientApiRoutePumpThreadRaw = 0xBDE510;
    public const uint ClientApiRoutePumpThreadEnabledGateRaw = 0xBDE53E;
    public const uint ClientApiRoutePumpThreadStopWaitRaw = 0xBDE580;
    public const uint ClientApiRoutePumpThreadPumpCallRaw = 0xBDE5F4;
    public const uint ClientApiRequestResetTickOffset = 0x68;
    public const uint ClientApiRequestTrackedTickOffset = 0x6C;
    public const uint ClientApiRequestOpenTickOffset = 0x70;
    public const uint ClientApiRequestCallbackTickOffset = 0x74;
    public const uint ClientApiRequestUrlLookupKeyBuilderRaw = 0xBC1040;
    public const uint ClientApiRequestUrlLookupTreeSearchRaw = 0xBC4D60;
    public const uint ClientApiRequestUrlLookupTreeLowerBoundRaw = 0xBC61C0;
    public const uint ClientApiRequestUrlLookupTreeInsertOrReplaceRaw = 0xBC4CB0;
    public const uint ClientApiRequestDateHeaderParserRaw = 0xBCB5A0;
    public const uint ClientApiPumpRequestWorkerRaw = 0xBC2B30;
    public const uint ClientApiRequestDateHeaderNameVa = 0x1C737C4;
    public const uint ClientApiRequestDateHeaderFormatVa = 0x1BB5F90;
    public const uint ClientApiRequestTimeImportVa = 0x1AC48D8;
    public const uint ClientApiRequestDateScanImportVa = 0x1AC48FC;
    public const uint ClientApiRequestMonthCompareImportVa = 0x1AC4898;
    public const uint ClientApiRequestUtcTimeImportVa = 0x1AC4988;
    public const uint ClientApiRequestTimerImportVa = 0x1AC4D24;
    public const uint ClientApiRequestRandImportVa = 0x1AC48F8;
    public const uint ClientApiRequestHashCreateImportVa = 0x1AC4038;
    public const uint ClientApiRequestHashDataImportVa = 0x1AC402C;
    public const uint ClientApiRequestHashGetValueImportVa = 0x1AC4010;
    public const uint ClientApiRequestHashDestroyImportVa = 0x1AC4030;
    public const uint ClientApiRequestSignatureHexDigitsVa = 0x1C58590;
    public const uint ClientApiRequestUrlLookupLowercaseRaw = 0x12B9A20;
    public const uint ClientApiRequestUrlLookupLowercaseTextRaw = 0x12B9B00;
    public const uint ClientApiRequestUrlLookupFormatVa = 0x1C73534;
    public const uint ClientApiRequestQueryFormatVa = 0x1C57568;
    public const string ClientApiRequestQueryNativeFormat =
        "ver=%d&tc=%u&nonce=%04x%04x%04x%04x&uid=%s&host=%s&path=%s&hbody=%s&cid=%llu";
    public const uint ClientApiRequestSignatureHeaderNameVa = 0x1BB5328;
    public const string ClientApiRequestSignatureHeaderName = "X-Red5-Signature";
    public const uint ClientApiRequestHeaderFormatVa = 0x1C736C8;
    public const string ClientApiRequestHeaderNativeFormat = "%s: %s\r\n";
    public const uint ClientApiRequestAddHeadersImportVa = 0x1AC4CF4;
    public const uint ClientApiRequestUrlLookupFormatSharedTextRaw = 0x12B9230;
    public const uint LuaUrlFilterUrlMessageBuilderRaw = 0x12E4100;
    public const uint LuaUrlFilterUrlMessageFormatVa = 0x1C576D0;
    public const uint LuaUrlFilterUrlComponentEscapeRaw = 0x12E4810;
    public const uint LuaUrlFilterUrlComponentEscapeAllowlistVa = 0x16E55D4;
    public const uint LuaUrlFilterUrlComponentEscapeHexDigitsVa = 0x1C80D70;
    public const uint LuaUrlFilterDispatchTableVa = 0x1C747B8;
    public const uint LuaUrlFilterDispatchTableCallbackSlotVa = 0x1C747BC;
    public const uint LuaUrlFilterDispatchFirstEntryPointerVa = 0x10BDEC0;
    public const uint LuaUrlFilterDispatchFirstEntryRaw = 0xCBD2C0;
    public const uint LuaUrlFilterCallbackPointerVa = 0x10BE9E0;
    public const uint LuaUrlFilterDispatchObjectConstructorRaw = 0xCBC410;
    public const uint LuaUrlFilterDispatchObjectPrimaryVtableVa = 0x1C747CC;
    public const uint LuaUrlFilterDispatchObjectPrimaryCleanupSlotVa = 0x1C747D0;
    public const uint LuaUrlFilterDispatchObjectSetupSlotVa = 0x1C747D4;
    public const uint LuaUrlFilterDispatchObjectDeleteSlotVa = 0x1C747DC;
    public const uint LuaUrlFilterDispatchObjectPrimaryCleanupSlotPointerVa = 0x10BCD60;
    public const uint LuaUrlFilterDispatchObjectSetupSlotPointerVa = 0x10BCD80;
    public const uint LuaUrlFilterDispatchObjectDeleteSlotPointerVa = 0x10BD0D0;
    public const uint LuaUrlFilterDispatchObjectFinalPrimaryVtableVa = 0x1C747E4;
    public const uint LuaUrlFilterDispatchObjectPrimaryCleanupSlotRaw = 0xCBC160;
    public const uint LuaUrlFilterDispatchObjectPrimaryReleaseSlotRaw = 0xCBC150;
    public const uint LuaUrlFilterDispatchObjectSetupSlotRaw = 0xCBC180;
    public const uint LuaUrlFilterDispatchObjectDeleteSlotRaw = 0xCBC4D0;
    public const uint LuaUrlFilterDispatchObjectStateUpdateRaw = 0xCBC270;
    public const uint LuaUrlFilterDispatchObjectResetRaw = 0xCBC510;
    public const uint LuaUrlFilterDispatchObjectResetListHelperRaw = 0xCBE080;
    public const uint LuaUrlFilterDispatchObjectListConstructorRaw = 0xCBE310;
    public const uint LuaUrlFilterDispatchObjectListSentinelFactoryRaw = 0xCBF080;
    public const uint LuaUrlFilterDispatchObjectListNodeAllocateRaw = 0x12B7FB0;
    public const uint LuaUrlFilterDispatchObjectListVectorInsertRaw = 0xCBEE50;
    public const uint LuaUrlFilterDispatchObjectResetWrapperRaw = 0xCBC4D0;
    public const uint LuaUrlFilterDispatchObjectStateStringStorageOffset = 0x28;
    public const uint LuaUrlFilterDispatchObjectPendingModeFlagOffset = 0x68;
    public const uint LuaUrlFilterDispatchObjectSecondaryVtableOffset = 0x04;
    public const uint LuaUrlFilterDispatchObjectFactoryRaw = 0xCBB7A0;
    public const uint LuaUrlFilterDispatchObjectSetupRaw = 0xBFB310;
    public const uint LuaUrlFilterDispatchObjectOwnerCleanupRaw = 0xBF8AC0;
    public const uint LuaUrlFilterDispatchObjectBasePathFieldOffset = 0x5C8;
    public const uint LuaUrlFilterDispatchObjectBasePathEmptyFallbackVa = 0x1BBA0FC;
    public const uint LuaUrlFilterDispatchObjectFirstArgumentFieldOffset = 0x9A4;
    public const uint LuaUrlFilterDispatchObjectSecondArgumentFieldOffset = 0x9A8;
    public const uint LuaUrlFilterDispatchObjectFirstArgumentEmptyFallbackVa = 0x1BBA15C;
    public const uint LuaUrlFilterDispatchObjectSecondArgumentEmptyFallbackVa = 0x1BBA160;
    public const uint LuaUrlFilterDispatchObjectStorageFieldOffset = 0xA04;
    public const uint LuaUrlFilterDispatchObjectSetupOwnerFlagOffset = 0x5C;
    public const uint LuaUrlFilterDispatchObjectSetupSlotByteOffset = 0x08;
    public const uint CallbackLogHelperRaw = 0x2FC4F0;
    public const uint CallbackUnknownLogHelperRaw = 0x2FCC60;
    public const uint CreationFailureFormatVa = 0x1BD4B2C;
    public const byte AttemptedWithoutSuccessByteOffset = 0x72;
    public const byte SucceededByteOffset = 0x73;
    public const string Red5ObjectName = "Red5";
    public const string ErrorLogMethodName = "errorlog";
    public const string CreationFailureFormat = "Unable to create Red5 JS object: %s";
    public const int ErrorLogMethodTrailingArgument = 0;

    public static Red5LoggingBridgeEntryAction CaptureEntry(
        byte observedSucceeded, byte observedAttemptedWithoutSuccess) =>
        observedSucceeded != 0 ? Red5LoggingBridgeEntryAction.ReturnReady
        : observedAttemptedWithoutSuccess != 0
            ? Red5LoggingBridgeEntryAction.ReturnUnavailableAfterPriorFailure
            : Red5LoggingBridgeEntryAction.AttemptCreation;

    public static Red5LoggingBridgeCreationAction CaptureCreationResult(bool objectCreated) =>
        objectCreated ? Red5LoggingBridgeCreationAction.RegisterErrorLogMethod
            : Red5LoggingBridgeCreationAction.MarkUnavailableAndLogFailure;

    public static Red5MethodCallbackAction CaptureMethodCallback(
        bool remoteObjectMatches,
        bool mappedMethodEntrySelected,
        string methodName,
        uint argumentCount,
        bool firstArgumentIsString)
    {
        if (!remoteObjectMatches)
        {
            return Red5MethodCallbackAction.LogUnknownCallback;
        }

        if (mappedMethodEntrySelected)
        {
            return Red5MethodCallbackAction.QueueMappedMethodCallback;
        }

        if (!string.Equals(methodName, ErrorLogMethodName, StringComparison.Ordinal))
        {
            return Red5MethodCallbackAction.LogUnknownCallback;
        }

        return argumentCount != 0 && firstArgumentIsString
            ? Red5MethodCallbackAction.LogErrorMessage
            : Red5MethodCallbackAction.ReturnWithoutLogging;
    }

    public static Red5MappedCallbackArgumentAction CaptureMappedCallbackArguments(
        bool hasArguments,
        bool trailingArgumentIsObject,
        bool trailingArgumentPassesCallableLikePredicate) =>
        hasArguments && trailingArgumentIsObject && trailingArgumentPassesCallableLikePredicate
            ? Red5MappedCallbackArgumentAction.SeparateTrailingCallableLikeValue
            : Red5MappedCallbackArgumentAction.PreserveAllArguments;

    public static Red5LuaCallbackRegistrationAction CaptureLuaCallbackRegistration(
        bool registrationContextAvailable,
        bool callbackReferenceValid,
        string methodName,
        bool methodAlreadyRegistered)
    {
        if (!registrationContextAvailable || !callbackReferenceValid)
        {
            return Red5LuaCallbackRegistrationAction.RejectUnavailableContextOrInvalidReference;
        }

        if (methodName.IndexOf('\'') >= 0 || methodName.IndexOf('\\') >= 0)
        {
            return Red5LuaCallbackRegistrationAction.RejectUnsupportedMethodName;
        }

        return methodAlreadyRegistered
            ? Red5LuaCallbackRegistrationAction.ReplaceExistingMethod
            : Red5LuaCallbackRegistrationAction.RegisterNewMethod;
    }

    public static Red5LuaWebBindingAction CaptureCallWebFuncBinding(
        uint luaArgumentCount,
        bool functionNameIsStringOrNumber) =>
        luaArgumentCount < 2 ? Red5LuaWebBindingAction.ReportUsage
        : functionNameIsStringOrNumber ? Red5LuaWebBindingAction.DispatchToWebObject
        : Red5LuaWebBindingAction.RejectUnsupportedNameType;

    public static Red5LuaWebBindingAction CaptureAddWebValueBinding(
        uint luaArgumentCount,
        bool propertyNameIsStringOrNumber) =>
        luaArgumentCount < 3 ? Red5LuaWebBindingAction.ReportUsage
        : propertyNameIsStringOrNumber ? Red5LuaWebBindingAction.DispatchToWebObject
        : Red5LuaWebBindingAction.RejectUnsupportedNameType;

    public static Red5LuaWebFunctionInvocationAction CaptureCallWebFuncInvocation(
        bool callbacksObjectAvailable,
        bool functionExists,
        bool asynchronousInvokeEnabled) =>
        !callbacksObjectAvailable
            ? Red5LuaWebFunctionInvocationAction.ReturnWithoutInvocationForUnavailableCallbacksObject
        : !functionExists
            ? Red5LuaWebFunctionInvocationAction.ReturnWithoutInvocationForMissingFunction
        : asynchronousInvokeEnabled
            ? Red5LuaWebFunctionInvocationAction.InvokeAsynchronously
            : Red5LuaWebFunctionInvocationAction.InvokeSynchronously;

    public static Red5LuaWebValueAction CaptureAddWebValueDispatch(bool webObjectAvailable) =>
        webObjectAvailable
            ? Red5LuaWebValueAction.SetPropertyAsynchronously
            : Red5LuaWebValueAction.ReturnWithoutSettingForUnavailableObject;

    public static Red5LuaUrlFilterAction CaptureSetUrlFilter(string? filter) =>
        !string.IsNullOrEmpty(filter) && filter[^1] == '*'
            ? Red5LuaUrlFilterAction.StoreInReplacementList
            : Red5LuaUrlFilterAction.LogAndSkip;

    public static int CaptureDispatchObjectSetupMode(bool incomingFlag, bool ownerFlag) =>
        incomingFlag && ownerFlag ? 2 : 0;

    public static bool CaptureDispatchObjectSetsPendingModeFlag(
        int mode,
        bool stateStringStoragePresent,
        bool helperPresent,
        bool firstResultPresent) =>
        mode != 0
        && stateStringStoragePresent
        && mode != 1
        && helperPresent
        && !firstResultPresent;

    public static Red5DispatchObjectDeleteAction CaptureDispatchObjectDelete(bool releaseAllocation) =>
        releaseAllocation
            ? Red5DispatchObjectDeleteAction.ResetAndReleaseAllocation
            : Red5DispatchObjectDeleteAction.ResetAndPreserveAllocation;

    public static Red5ParsedUrl? CaptureUrlComponents(string? candidateUrl)
    {
        if (candidateUrl is null)
            return null;

        int nulIndex = candidateUrl.IndexOf('\0');
        int length = nulIndex >= 0 ? nulIndex : candidateUrl.Length;
        var source = candidateUrl[..Math.Min(length, 2048)];
        var isHttps = false;
        int schemeSeparator = source.IndexOf(':');
        if (schemeSeparator >= 0
            && source.AsSpan(schemeSeparator).StartsWith("://", StringComparison.Ordinal))
        {
            var scheme = source[..schemeSeparator];
            if (string.Equals(scheme, "http", StringComparison.OrdinalIgnoreCase))
            {
                isHttps = false;
            }
            else if (string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                isHttps = true;
            }
            else
            {
                return null;
            }

            source = source[(schemeSeparator + 3)..];
        }

        int pathStart = source.IndexOf('/');
        var authority = pathStart >= 0 ? source[..pathStart] : source;
        var pathAndQuery = pathStart >= 0 ? source[(pathStart + 1)..] : string.Empty;
        int authorityPortSeparator = authority.IndexOf(':');
        var host = authorityPortSeparator >= 0 ? authority[..authorityPortSeparator] : authority;
        if (host.Length == 0)
            return null;

        int port = isHttps ? 443 : 80;
        if (authorityPortSeparator >= 0
            && !TryCaptureDecimalPort(authority[(authorityPortSeparator + 1)..], out port))
            return null;

        int fragmentStart = pathAndQuery.IndexOf('#');
        if (fragmentStart >= 0)
            pathAndQuery = pathAndQuery[..fragmentStart];

        var query = string.Empty;
        int queryStart = pathAndQuery.IndexOf('?');
        if (queryStart >= 0)
        {
            query = pathAndQuery[(queryStart + 1)..];
            pathAndQuery = pathAndQuery[..queryStart];
        }

        return new Red5ParsedUrl(isHttps, host, port, "/" + pathAndQuery, query);
    }

    public static string[] CaptureDefaultPortHostSuffixLookupKeys(Red5ParsedUrl? url)
    {
        if (url is not { } parsed || parsed.Port != (parsed.IsHttps ? 443 : 80))
            return Array.Empty<string>();

        var keys = new List<string>();
        for (int period = parsed.Host.IndexOf('.'); period >= 0; period = parsed.Host.IndexOf('.', period + 1))
            keys.Add(CaptureAsciiLowercase(parsed.Host[period..]));

        return keys.ToArray();
    }

    public static ClientApiServerClockOffsetEntry[] CaptureClientApiServerClockOffsetEntries(
        Red5ParsedUrl? url,
        int offsetSeconds)
    {
        string? hostPortKey = CaptureUrlHostPortLookupKey(url);
        if (hostPortKey is null)
            return Array.Empty<ClientApiServerClockOffsetEntry>();

        var entries = new List<ClientApiServerClockOffsetEntry>
        {
            new(hostPortKey, offsetSeconds)
        };
        foreach (string key in CaptureDefaultPortHostSuffixLookupKeys(url))
            entries.Add(new ClientApiServerClockOffsetEntry(key, offsetSeconds));

        return entries.ToArray();
    }

    public static int CaptureClientApiAdjustedTimestamp(int localTimestamp, int offsetSeconds) =>
        unchecked(localTimestamp + offsetSeconds);

    public static ClientApiRequestTimerDefaults CaptureClientApiRequestTimerDefaults(uint tick) =>
        new(tick, unchecked(tick - 1));

    public static string CaptureClientApiRequestQuery(
        uint adjustedTimestamp,
        ushort nonce0,
        ushort nonce1,
        ushort nonce2,
        ushort nonce3,
        string uid,
        string host,
        string path,
        string body,
        ulong clientId) =>
        System.FormattableString.Invariant(
            $"ver=2&tc={adjustedTimestamp}&nonce={nonce0:x4}{nonce1:x4}{nonce2:x4}{nonce3:x4}&uid={uid}&host={host}&path={path}&hbody={body}&cid={clientId}");

    public static string CaptureClientApiRequestSignature(
        ReadOnlySpan<byte> key,
        ReadOnlySpan<byte> query) =>
        Convert.ToHexString(System.Security.Cryptography.HMACSHA1.HashData(key, query))
            .ToLowerInvariant();

    public static string CaptureClientApiRequestHeaderLine(string name, string value) =>
        string.Concat(name, ": ", value, "\r\n");

    public static string? CaptureUrlHostPortLookupKey(Red5ParsedUrl? url) =>
        url is { } parsed
            ? CaptureAsciiLowercase(string.Concat(
                parsed.Host,
                ":",
                parsed.Port.ToString(System.Globalization.CultureInfo.InvariantCulture)))
            : null;

    public static string CaptureUrlComponentEscape(ReadOnlySpan<byte> value)
    {
        const string hexDigits = "0123456789ABCDEF";
        var escaped = new char[checked(value.Length * 3)];
        int length = 0;
        foreach (byte current in value)
        {
            if (current is >= (byte)'a' and <= (byte)'z'
                or >= (byte)'A' and <= (byte)'Z'
                or >= (byte)'0' and <= (byte)'9'
                or (byte)'-' or (byte)'.' or (byte)'_' or (byte)'~')
            {
                escaped[length++] = (char)current;
                continue;
            }

            escaped[length++] = '%';
            escaped[length++] = hexDigits[current >> 4];
            escaped[length++] = hexDigits[current & 0x0F];
        }

        return new string(escaped, 0, length);
    }

    private static string CaptureAsciiLowercase(string value)
    {
        var characters = value.ToCharArray();
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] is >= 'A' and <= 'Z')
                characters[i] = (char)(characters[i] + ('a' - 'A'));
        }

        return new string(characters);
    }

    private static bool TryCaptureDecimalPort(string value, out int port)
    {
        port = 0;
        int index = 0;
        while (index < value.Length && value[index] is ' ' or '\t' or '\n' or '\r' or '\f' or '\v')
            index++;

        bool negative = index < value.Length && value[index] == '-';
        if (negative || (index < value.Length && value[index] == '+'))
            index++;

        int digitStart = index;
        while (index < value.Length && value[index] is >= '0' and <= '9')
        {
            int digit = value[index] - '0';
            if (port > (65535 - digit) / 10)
                return false;
            port = (port * 10) + digit;
            index++;
        }

        return !negative && index > digitStart && port is >= 1 and <= 65535;
    }

    public static bool CaptureUrlFilterListMatch(
        IReadOnlyList<string> filters,
        string? candidateText)
    {
        candidateText ??= string.Empty;
        foreach (var filter in filters)
        {
            if (string.IsNullOrEmpty(filter) || filter[^1] != '*')
                continue;

            var prefix = filter.AsSpan(0, filter.Length - 1);
            if (candidateText.AsSpan().StartsWith(prefix, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    public static Red5UrlFilterCallbackAction CaptureUrlFilterCallback(
        bool cachedContextUnchanged,
        IReadOnlyList<string> filters,
        string? candidateText) =>
        cachedContextUnchanged
            ? Red5UrlFilterCallbackAction.ReturnFalseForUnchangedContext
        : CaptureUrlFilterListMatch(filters, candidateText)
            ? Red5UrlFilterCallbackAction.ReturnFalseForMatchingFilter
            : Red5UrlFilterCallbackAction.ReturnTrueForNoMatchingFilter;

    public static Red5LuaWebRequestLoggingAction CaptureLogWebRequests(bool luaValueIsTruthy) =>
        luaValueIsTruthy
            ? Red5LuaWebRequestLoggingAction.Enable
            : Red5LuaWebRequestLoggingAction.Disable;

    public static Red5LuaLoadUrlAction CaptureLoadUrl(
        bool frameIsValid,
        bool urlValueIsConvertible,
        bool webObjectAvailable) =>
        !frameIsValid ? Red5LuaLoadUrlAction.ReportInvalidFrame
        : !urlValueIsConvertible ? Red5LuaLoadUrlAction.ReportInvalidUrlValue
        : webObjectAvailable ? Red5LuaLoadUrlAction.LoadAndReturnTrue
        : Red5LuaLoadUrlAction.ReturnWithoutLoading;

    public static string CaptureGetUrlResult(string? currentUrl) => currentUrl ?? string.Empty;

    public static Red5NaturalDimensionsAction CaptureSetNaturalDimensions(
        float currentWidth,
        float currentHeight,
        float requestedWidth,
        float requestedHeight) =>
        currentWidth != requestedWidth || currentHeight != requestedHeight
            ? Red5NaturalDimensionsAction.UpdateAndNotify
            : Red5NaturalDimensionsAction.KeepExistingDimensions;

    public static int CaptureNaturalFitScalePercent(
        int viewportWidth,
        int viewportHeight,
        float naturalWidth,
        float naturalHeight)
    {
        var scale = 1f;
        if (naturalWidth >= LuaNaturalFitScaleDimensionThreshold
            && naturalHeight >= LuaNaturalFitScaleDimensionThreshold)
        {
            var widthScale = viewportWidth / naturalWidth;
            var heightScale = viewportHeight / naturalHeight;
            scale = heightScale <= widthScale ? heightScale : widthScale;
        }
        else if (naturalWidth >= LuaNaturalFitScaleDimensionThreshold)
        {
            scale = viewportWidth / naturalWidth;
        }
        else if (naturalHeight >= LuaNaturalFitScaleDimensionThreshold)
        {
            scale = viewportHeight / naturalHeight;
        }

        scale = Math.Clamp(scale, LuaNaturalFitScaleMinimum, LuaNaturalFitScaleMaximum);
        return (int)(scale * LuaNaturalFitScalePercentMultiplier + LuaNaturalFitScaleRoundingBias);
    }

    public static Red5TypeTargetFlagAction CaptureEnableTypeTarget(byte currentFlag, bool requestedTruthy) =>
        (currentFlag != 0) == requestedTruthy
            ? Red5TypeTargetFlagAction.LeaveUnchanged
            : Red5TypeTargetFlagAction.UpdateFlag;

    public static bool CaptureIsTypeTarget(byte currentFlag) => currentFlag != 0;

    public static Red5QueuedJavaScriptContinuationAction CaptureQueuedJavaScriptContinuation(
        bool continuationValueIsUndefined,
        bool asynchronousInvokeEnabled) =>
        continuationValueIsUndefined ? Red5QueuedJavaScriptContinuationAction.SkipUndefinedContinuation
        : asynchronousInvokeEnabled ? Red5QueuedJavaScriptContinuationAction.InvokeAsynchronously
        : Red5QueuedJavaScriptContinuationAction.InvokeSynchronously;

    public static Red5BridgePointerCleanupAction CaptureBridgePointerCleanup(
        bool pointerIsNull,
        bool pointerMatchesSharedSentinel) =>
        pointerMatchesSharedSentinel ? Red5BridgePointerCleanupAction.PreserveSharedSentinel
        : pointerIsNull ? Red5BridgePointerCleanupAction.ClearNullPointer
        : Red5BridgePointerCleanupAction.ReleaseAndClearPointer;
}
