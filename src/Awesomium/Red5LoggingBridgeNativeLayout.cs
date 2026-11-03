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
    public const uint LuaUrlFilterDispatchTableVa = 0x1C747B8;
    public const uint LuaUrlFilterDispatchTableCallbackSlotVa = 0x1C747BC;
    public const uint LuaUrlFilterDispatchFirstEntryPointerVa = 0x10BDEC0;
    public const uint LuaUrlFilterDispatchFirstEntryRaw = 0xCBD2C0;
    public const uint LuaUrlFilterCallbackPointerVa = 0x10BE9E0;
    public const uint LuaUrlFilterDispatchObjectConstructorRaw = 0xCBC410;
    public const uint LuaUrlFilterDispatchObjectPrimaryVtableVa = 0x1C747CC;
    public const uint LuaUrlFilterDispatchObjectPrimaryCleanupSlotVa = 0x1C747D0;
    public const uint LuaUrlFilterDispatchObjectSetupSlotVa = 0x1C747D4;
    public const uint LuaUrlFilterDispatchObjectPrimaryCleanupSlotPointerVa = 0x10BCD60;
    public const uint LuaUrlFilterDispatchObjectSetupSlotPointerVa = 0x10BCD80;
    public const uint LuaUrlFilterDispatchObjectPrimaryCleanupSlotRaw = 0xCBC160;
    public const uint LuaUrlFilterDispatchObjectPrimaryReleaseSlotRaw = 0xCBC150;
    public const uint LuaUrlFilterDispatchObjectSetupSlotRaw = 0xCBC180;
    public const uint LuaUrlFilterDispatchObjectStateUpdateRaw = 0xCBC270;
    public const uint LuaUrlFilterDispatchObjectResetRaw = 0xCBC510;
    public const uint LuaUrlFilterDispatchObjectResetWrapperRaw = 0xCBC4D0;
    public const uint LuaUrlFilterDispatchObjectStateStringStorageOffset = 0x28;
    public const uint LuaUrlFilterDispatchObjectPendingModeFlagOffset = 0x68;
    public const uint LuaUrlFilterDispatchObjectSecondaryVtableOffset = 0x04;
    public const uint LuaUrlFilterDispatchObjectFactoryRaw = 0xCBB7A0;
    public const uint LuaUrlFilterDispatchObjectSetupRaw = 0xBFB310;
    public const uint LuaUrlFilterDispatchObjectOwnerCleanupRaw = 0xBF8AC0;
    public const uint LuaUrlFilterDispatchObjectFirstArgumentFieldOffset = 0x9A4;
    public const uint LuaUrlFilterDispatchObjectSecondArgumentFieldOffset = 0x9A8;
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
