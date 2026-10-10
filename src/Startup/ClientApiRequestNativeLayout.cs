namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Shared WinHTTP request observations reached from the Login request paths.
/// Addresses are raw offsets unless explicitly named VirtualAddress.
/// This does not perform network requests.
/// </summary>
public static class ClientApiRequestNativeLayout
{
    public const int AllocationSize = 0x78;
    public const int TypeOffset = 0x08;
    public const int WeakCallbackPointerOffset = 0x50;
    public const int WeakCallbackControlBlockOffset = 0x54;
    public const uint ConstructorFileOffset = 0x3843F0;
    public const uint ResetMethodFileOffset = 0xBC1280;
    public const uint ManagerPumpMethodFileOffset = 0xBC4550;
    public const uint RoutePumpMethodFileOffset = 0xBC3F80;
    public const uint WorkerPumpMethodFileOffset = 0xBC2B30;
    public const uint MethodJumpTableVirtualAddress = 0xFC44C0;
    public const uint MethodJumpTableFileOffset = 0xBC38C0;
    public const uint WinHttpOpenRequestCallFileOffset = 0xBC2DB7;
    public const uint WinHttpOpenRequestImportVirtualAddress = 0x1AC4CFC;

    // The native unsigned (type - 2) > 3 branch also defaults to GET.
    public static string ResolveHttpMethod(int nativeRequestType) => nativeRequestType switch
    {
        2 => "POST",
        3 => "PUT",
        4 => "DELETE",
        5 => "HEAD",
        _ => "GET"
    };
}

/// <summary>
/// Verified response fields passed to the acquired callback. The result kind,
/// metadata entries, and two text fields remain opaque.
/// </summary>
public static class ClientApiResponseNativeLayout
{
    public const int AllocationSize = 0x20;
    public const uint ConstructorFileOffset = 0x3845D0;
    public const uint ResetMethodFileOffset = 0xBC11D0;
    public const int RequestPointerOffset = 0x04;
    public const int ResultKindOffset = 0x08;
    public const int ResultKindInitialValue = 1;
    public const int StatusWordOffset = 0x0C;
    public const int MetadataTreeHeadOffset = 0x10;
    public const int MetadataTreeCountOffset = 0x14;
    public const int FirstTextOffset = 0x18;
    public const int SecondTextOffset = 0x1C;
}

/// <summary>
/// Bounded completion delivery: the completion request handle must be zero and the weak
/// callback must acquire a live strong reference before virtual dispatch.
/// These constants do not simulate concurrency or completion scheduling.
/// </summary>
public static class ClientApiCallbackDeliveryNativeLayout
{
    public const int WorkerActiveCompletionPointerOffset = 0x1C;
    public const int CompletionRequestHandleOffset = 0x00;
    public const int RequiredCompletionRequestHandleValue = 0;
    public const int CompletionResponsePointerOffset = 0x18;
    public const uint CompletionGateFileOffset = 0xBC32B5;
    public const uint AcquireCallbackCallFileOffset = 0xBC37E2;
    public const uint AcquireCallbackMethodFileOffset = 0xBC4A80;
    public const uint TryAcquireStrongReferenceMethodFileOffset = 0x327730;
    public const int ControlBlockStrongCountOffset = 0x04;
    public const int ControlBlockWeakCountOffset = 0x08;
    public const uint CallbackVirtualCallFileOffset = 0xBC37FC;
    public const int CallbackVirtualMethodOffset = 0x04;
}

/// <summary>
/// WinHTTP status callback registration and terminal handle-close signaling.
/// The worker completion record holds the actual WinHTTP request handle.
/// </summary>
public static class ClientApiWinHttpCompletionNativeLayout
{
    public const int CompletionAllocationSize = 0x1C;
    public const int CompletionRequestHandleOffset = 0x00;
    public const int CompletionCleanupFlagOffset = 0x11;
    public const int CompletionDispatchResultOffset = 0x14;
    public const int CompletionResponsePointerOffset = 0x18;
    public const uint StatusCallbackFileOffset = 0xBC26E0;
    public const uint StatusCallbackVirtualAddress = 0xFC32E0;
    public const uint InternalStatusDispatcherFileOffset = 0xBC1DA0;
    public const uint RegisterStatusCallbackCallFileOffset = 0xBC31E9;
    public const uint RegisterStatusCallbackImportVirtualAddress = 0x1AC4D0C;
    public const uint SendRequestCallFileOffset = 0xBC3213;
    public const uint SendRequestContextPushFileOffset = 0xBC31F8;
    public const uint StoreResponseInCompletionFileOffset = 0xBC2D47;
    public const uint StoreRequestHandleInCompletionFileOffset = 0xBC2DEC;
    public const uint StoreCompletionInWorkerFileOffset = 0xBC3280;
    public const uint ClearRequestHandleOnClosingFileOffset = 0xBC2728;
    public const uint CloseRequestHandleCallFileOffset = 0xBC2785;
    public const uint ClearRequestHandleOnSynchronousFailureFileOffset = 0xBC322E;
    public const uint WorkerTeardownMethodFileOffset = 0xBC2890;
    public const uint MarkCallbackOwnedCleanupFileOffset = 0xBC28CB;
    public const uint RouteDestructorFileOffset = 0xBC3B80;
    public const int WorkerAdditionalHandleOffset = 0x20;
    public const uint CloseAdditionalHandleDuringTeardownFileOffset = 0xBC28D7;
    public const uint HandleClosingStatus = 0x800;
    public const uint ReadCompleteStatus = 0x80000;
    public const uint RequestErrorStatus = 0x200000;
    public const int ResultKindOnEmptyRead = 1;
    public const int ResultKindOnHandleClosing = 3;
    public const int ResultKindOnTerminalDispatch = 4;

    // The native callback preserves any result already assigned by the dispatcher.
    public static int ApplyHandleClosingResult(int currentResultKind) =>
        currentResultKind == 0 ? ResultKindOnHandleClosing : currentResultKind;
}
