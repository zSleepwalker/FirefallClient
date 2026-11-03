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

public enum Red5QueuedCallbackInvocationAction
{
    SkipUndefinedCallback,
    InvokeSynchronously,
    InvokeAsynchronously
}

/// <summary>Observed Red5 setup and callback decisions; invokes no browser or logging APIs.</summary>
public static class Red5LoggingBridgeNativeLayout
{
    public const uint InitializerRaw = 0xD668D0;
    public const uint CallbackHandlerRaw = 0xD65A60;
    public const uint CallbackHandlerVtableVa = 0x1C7656C;
    public const uint CallbackQueueConsumerRaw = 0xD64AE0;
    public const uint CallbackQueueConsumerRttiLocatorPointerVa = 0x1C765F8;
    public const uint CallbackQueueConsumerVtableSlotVa = 0x1C7664C;
    public const uint CallbackQueueConsumerClassDescriptorVa = 0x1E2E238;
    public const uint CallbackMethodLookupRaw = 0xCCD640;
    public const uint CallbackCallablePredicateRaw = 0xD64320;
    public const uint CallbackQueueAppendRaw = 0xD66B70;
    public const uint SetCustomMethodImportVa = 0x1AC4EE0;
    public const uint SetPropertyAsyncImportVa = 0x1AC4EF0;
    public const uint CallbackIsUndefinedImportVa = 0x1AC4EE8;
    public const uint CallbackInvokeImportVa = 0x1AC4EDC;
    public const uint CallbackInvokeAsyncImportVa = 0x1AC4EEC;
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

    public static Red5QueuedCallbackInvocationAction CaptureQueuedCallbackInvocation(
        bool callbackValueIsUndefined,
        bool asynchronousInvokeEnabled) =>
        callbackValueIsUndefined ? Red5QueuedCallbackInvocationAction.SkipUndefinedCallback
        : asynchronousInvokeEnabled ? Red5QueuedCallbackInvocationAction.InvokeAsynchronously
        : Red5QueuedCallbackInvocationAction.InvokeSynchronously;
}
