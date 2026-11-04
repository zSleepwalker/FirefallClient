using Firefall.Reconstruction.Awesomium;
using Firefall.Reconstruction.DumpTruck;
using Firefall.Reconstruction.DeploymentCommon;
using Firefall.Reconstruction.FirefallProcess;
using Firefall.Reconstruction.HWStats;
using Firefall.Reconstruction.R5Dumper;
using Firefall.Reconstruction.RedHanded;

if (args.Length != 0 && (args.Length != 2 || args[0] != "--samples"))
{
    Console.Error.WriteLine("Usage: CompanionTools.Checks [--samples <system-bin-directory>]");
    return 2;
}

int passed = 0, failed = 0;
void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (condition) passed++; else failed++;
}

foreach (var entry in new (int Minimum, int WithoutConfiguration)[]
{
    (int.MinValue, -1), (-20, 20), (-1, 1), (0, 0), (1, -1), (2, -1),
    (3, -1), (int.MaxValue, -1)
})
    Check($"Awesomium no-configuration level uses signed max after negation {entry.Minimum}",
        VerboseLevelNativeLayout.CaptureLevelWithoutConfiguration(entry.Minimum) == entry.WithoutConfiguration);
foreach (var entry in new (uint Extent, uint Length)[]
{
    (0, uint.MaxValue), (1, 0), (42, 41), (0x80000000, 0x7FFFFFFF),
    (uint.MaxValue, uint.MaxValue - 1)
})
    Check($"Awesomium verbose source extent decrements modulo32 {entry.Extent}",
        VerboseLevelNativeLayout.CaptureLookupByteLength(entry.Extent) == entry.Length);
foreach (int mode in new[] { -1, 0, 1, 2 })
foreach (int minimum in new[] { int.MinValue, 0, 3, 4, int.MaxValue })
foreach (uint extent in new uint[] { 0, 1, uint.MaxValue })
    Check($"Awesomium zero-extent diagnostic mode={mode}, minimum={minimum}, extent={extent}",
        VerboseLevelNativeLayout.CaptureZeroExtentDiagnosticAdmission(mode, minimum, extent)
        == (mode == 1 && minimum <= 3 && extent == 0));
Check("Awesomium configured no-match fallback differs from unconfigured clamped level",
    VerboseRuleNativeLayout.CaptureLevel([], 3) == -3
    && VerboseLevelNativeLayout.CaptureLevelWithoutConfiguration(3) == -1
    && VerboseRuleNativeLayout.CaptureLevel([], int.MinValue) == int.MinValue
    && VerboseLevelNativeLayout.CaptureLevelWithoutConfiguration(int.MinValue) == -1);
Check("DeploymentCommon abort requests exclude current worker only",
    DownloadCancellationNativeLayout.GetAbortDisposition(true) == DownloadWorkerAbortDisposition.SkipCurrentThread
    && DownloadCancellationNativeLayout.GetAbortDisposition(false) == DownloadWorkerAbortDisposition.RequestAbortOtherThread);
Check("DeploymentCommon queue drain signals only ManualResetEvent values",
    DownloadCancellationNativeLayout.ShouldSignalDrainedQueueEvent(true)
    && !DownloadCancellationNativeLayout.ShouldSignalDrainedQueueEvent(false));
Check("DeploymentCommon outer wait continues while any worker is alive",
    DownloadCancellationNativeLayout.ShouldContinueWaitingForWorkers(true)
    && !DownloadCancellationNativeLayout.ShouldContinueWaitingForWorkers(false));
Check("DeploymentCommon wait-region timer cleanup requires nonnull local",
    DownloadCancellationNativeLayout.ShouldDisposeTimerOnWaitExit(true)
    && !DownloadCancellationNativeLayout.ShouldDisposeTimerOnWaitExit(false));
foreach (var entry in new (string Source, string Module)[]
{
    (@"dir\widget-inl.cc", "widget"), ("dir/widget.test.cc", "widget.test"),
    ("widget-INL.cc", "widget-INL"), (".hidden", ""), ("dir/", ""),
    ("widget-inl", "widget"), ("x-inl.more.cc", "x-inl.more"),
    (@"a/b\c-inl.h", "c"), ("noextension", "noextension"), ("", "")
})
{
    byte[] source = System.Text.Encoding.ASCII.GetBytes(entry.Source);
    var slice = VerboseRuleNativeLayout.CaptureModuleSlice(source);
    Check($"Awesomium verbose module byte slice: {entry.Source}",
        source.AsSpan(slice.Offset, slice.Length).SequenceEqual(
            System.Text.Encoding.ASCII.GetBytes(entry.Module)));
}
Check("Awesomium verbose source slicing preserves high and embedded NUL bytes",
    VerboseRuleNativeLayout.CaptureModuleSlice(new byte[] { 0x80, 0, 0x81, (byte)'.', (byte)'h' })
        == new VerboseSourceSlice(0, 3));
foreach (var pattern in new (string Value, bool Full)[]
{
    ("*", false), ("widget", false), ("", false), ("dir/*", true),
    (@"dir\*", true), ("/", true), (@"\", true)
})
    Check($"Awesomium verbose rule selects full path only with separator: {pattern.Value}",
        VerboseRuleNativeLayout.CaptureUsesFullSourcePath(
            System.Text.Encoding.ASCII.GetBytes(pattern.Value)) == pattern.Full);
Check("Awesomium verbose first matching rule wins rather than later or greatest level",
    VerboseRuleNativeLayout.CaptureLevel(
        [new(false, 99), new(true, 2), new(true, 9)], 3) == 2);
foreach (int minimum in new[] { int.MinValue, -3, 0, 3, int.MaxValue })
    Check($"Awesomium verbose no-match fallback negates live minimum {minimum}",
        VerboseRuleNativeLayout.CaptureLevel([], minimum) == unchecked(-minimum)
        && VerboseRuleNativeLayout.CaptureLevel([new(false, 100)], minimum) == unchecked(-minimum));
foreach (bool webException in new[] { false, true })
foreach (int status in new[] { 0, 14, 15 })
foreach (string? message in new string?[] { null, "test", "Test", "test " })
foreach (int budget in new[] { -1, 0, 9, 10, 11 })
foreach (bool occupied in new[] { false, true })
{
    bool retry = webException && budget < 10 && (status == 14 || message == "test");
    Check($"DeploymentCommon worker disposition web={webException}, status={status}, message={message}, count={budget}, occupied={occupied}",
        DownloadWorkerNativeLayout.CaptureWorkerException(webException, status, message, budget, occupied)
        == (retry ? DownloadWorkerExceptionAction.RetryPart
            : occupied ? DownloadWorkerExceptionAction.KeepFirstFatalException
            : DownloadWorkerExceptionAction.RecordFirstFatalException));
}
Check("DeploymentCommon outer downloader throws exactly when captured exception slot is occupied",
    DownloadWorkerNativeLayout.ShouldThrowFromDoFileDownload(true)
    && !DownloadWorkerNativeLayout.ShouldThrowFromDoFileDownload(false));
foreach (bool initialized in new[] { false, true })
foreach (int mode in new[] { int.MinValue, -1, 0, 1, 2, int.MaxValue })
foreach (uint mutex in new uint[] { 0, 1, uint.MaxValue })
    Check($"Awesomium synchronization entry initialized={initialized}, mode={mode}, mutex={mutex:X8}",
        LogSynchronizationNativeLayout.CaptureEntry(initialized, mode, mutex) ==
        (initialized ? LogSynchronizationInitAction.ReturnWithoutChangingConfiguration
            : mode != 0 ? LogSynchronizationInitAction.AllocateCriticalSectionThenMarkInitialized
            : mutex != 0 ? LogSynchronizationInitAction.ReuseStoredMutexThenMarkInitialized
            : LogSynchronizationInitAction.BuildNameAndAttemptMutexCreation));
foreach (var name in new (string Path, string Expected)[]
{
    ("", @"Global\"), (@"C:\Game\debug.log", @"Global\C:/Game/debug.log"),
    (@".\debug.log", @"Global\./debug.log"),
    (@"\\server\share\debug.log", @"Global\//server/share/debug.log"),
    ("C:/Game/../debug.log", @"Global\C:/Game/../debug.log"),
    ("mixed\\UPPER/\u0100.log", "Global\\mixed/UPPER/\u0100.log")
})
    Check($"Awesomium mutex name preserves non-backslash code units: {name.Path}",
        LogSynchronizationNativeLayout.CaptureMutexName(name.Path) == name.Expected);
foreach (uint handle in new uint[] { 0, 1, 0x80000000, uint.MaxValue })
    Check($"Awesomium mutex creation only zero prevents initialized flag {handle:X8}",
        LogSynchronizationNativeLayout.CaptureInitializedAfterMutexCreation(handle) == (handle != 0));
foreach (bool initialized in new[] { false, true })
    Check($"Awesomium logger initializer can change sync state only before its flag is set: {initialized}",
        LogSynchronizationNativeLayout.CaptureInitializerCanChangeState(initialized) == !initialized);
foreach (byte succeeded in new byte[] { 0, 1, 0x80, byte.MaxValue })
foreach (byte attemptedWithoutSuccess in new byte[] { 0, 1, 0x80, byte.MaxValue })
    Check($"Awesomium Red5 bridge state success={succeeded}, prior failure={attemptedWithoutSuccess}",
        Red5LoggingBridgeNativeLayout.CaptureEntry(succeeded, attemptedWithoutSuccess) ==
        (succeeded != 0 ? Red5LoggingBridgeEntryAction.ReturnReady
            : attemptedWithoutSuccess != 0
                ? Red5LoggingBridgeEntryAction.ReturnUnavailableAfterPriorFailure
                : Red5LoggingBridgeEntryAction.AttemptCreation));
foreach (bool created in new[] { false, true })
    Check($"Awesomium Red5 bridge creation result: {created}",
        Red5LoggingBridgeNativeLayout.CaptureCreationResult(created) ==
        (created ? Red5LoggingBridgeCreationAction.RegisterErrorLogMethod
            : Red5LoggingBridgeCreationAction.MarkUnavailableAndLogFailure));
foreach (var callback in new[]
{
    (RemoteMatch: false, MethodEntrySelected: false, Method: "errorlog", Count: 1U, String: true,
        Expected: Red5MethodCallbackAction.LogUnknownCallback),
    (RemoteMatch: true, MethodEntrySelected: true, Method: "errorlog", Count: 1U, String: true,
        Expected: Red5MethodCallbackAction.QueueMappedMethodCallback),
    (RemoteMatch: true, MethodEntrySelected: false, Method: "other", Count: 1U, String: true,
        Expected: Red5MethodCallbackAction.LogUnknownCallback),
    (RemoteMatch: true, MethodEntrySelected: false, Method: "errorlog", Count: 1U, String: true,
        Expected: Red5MethodCallbackAction.LogErrorMessage),
    (RemoteMatch: true, MethodEntrySelected: false, Method: "errorlog", Count: 0U, String: false,
        Expected: Red5MethodCallbackAction.ReturnWithoutLogging),
    (RemoteMatch: true, MethodEntrySelected: false, Method: "errorlog", Count: 1U, String: false,
        Expected: Red5MethodCallbackAction.ReturnWithoutLogging)
})
    Check($"Awesomium Red5 callback routes {callback.Method} with {callback.Count} argument(s)",
        Red5LoggingBridgeNativeLayout.CaptureMethodCallback(
            callback.RemoteMatch, callback.MethodEntrySelected, callback.Method, callback.Count, callback.String)
        == callback.Expected);
foreach (var registration in new[]
{
    (Context: false, Reference: true, Name: "myfunc", Existing: false,
        Expected: Red5LuaCallbackRegistrationAction.RejectUnavailableContextOrInvalidReference),
    (Context: true, Reference: false, Name: "myfunc", Existing: false,
        Expected: Red5LuaCallbackRegistrationAction.RejectUnavailableContextOrInvalidReference),
    (Context: true, Reference: true, Name: "bad'name", Existing: false,
        Expected: Red5LuaCallbackRegistrationAction.RejectUnsupportedMethodName),
    (Context: true, Reference: true, Name: @"bad\name", Existing: false,
        Expected: Red5LuaCallbackRegistrationAction.RejectUnsupportedMethodName),
    (Context: true, Reference: true, Name: "myfunc", Existing: false,
        Expected: Red5LuaCallbackRegistrationAction.RegisterNewMethod),
    (Context: true, Reference: true, Name: "myfunc", Existing: true,
        Expected: Red5LuaCallbackRegistrationAction.ReplaceExistingMethod)
})
    Check($"Awesomium Red5 Lua callback registration context={registration.Context}, reference={registration.Reference}, existing={registration.Existing}",
        Red5LoggingBridgeNativeLayout.CaptureLuaCallbackRegistration(
            registration.Context, registration.Reference, registration.Name, registration.Existing)
        == registration.Expected);
foreach (var call in new[]
{
    (Count: 0U, NameValid: true, Expected: Red5LuaWebBindingAction.ReportUsage),
    (Count: 1U, NameValid: true, Expected: Red5LuaWebBindingAction.ReportUsage),
    (Count: 2U, NameValid: false, Expected: Red5LuaWebBindingAction.RejectUnsupportedNameType),
    (Count: 2U, NameValid: true, Expected: Red5LuaWebBindingAction.DispatchToWebObject)
})
    Check($"Awesomium CallWebFunc binding argument count={call.Count}, valid name={call.NameValid}",
        Red5LoggingBridgeNativeLayout.CaptureCallWebFuncBinding(call.Count, call.NameValid)
        == call.Expected);
foreach (var value in new[]
{
    (Count: 0U, NameValid: true, Expected: Red5LuaWebBindingAction.ReportUsage),
    (Count: 2U, NameValid: true, Expected: Red5LuaWebBindingAction.ReportUsage),
    (Count: 3U, NameValid: false, Expected: Red5LuaWebBindingAction.RejectUnsupportedNameType),
    (Count: 3U, NameValid: true, Expected: Red5LuaWebBindingAction.DispatchToWebObject)
})
    Check($"Awesomium AddWebValue binding argument count={value.Count}, valid name={value.NameValid}",
        Red5LoggingBridgeNativeLayout.CaptureAddWebValueBinding(value.Count, value.NameValid)
        == value.Expected);
foreach (var invocation in new[]
{
    (ObjectAvailable: false, FunctionExists: true, Async: false,
        Expected: Red5LuaWebFunctionInvocationAction.ReturnWithoutInvocationForUnavailableCallbacksObject),
    (ObjectAvailable: true, FunctionExists: false, Async: true,
        Expected: Red5LuaWebFunctionInvocationAction.ReturnWithoutInvocationForMissingFunction),
    (ObjectAvailable: true, FunctionExists: true, Async: false,
        Expected: Red5LuaWebFunctionInvocationAction.InvokeSynchronously),
    (ObjectAvailable: true, FunctionExists: true, Async: true,
        Expected: Red5LuaWebFunctionInvocationAction.InvokeAsynchronously)
})
    Check($"Awesomium CallWebFunc object available={invocation.ObjectAvailable}, function exists={invocation.FunctionExists}, async={invocation.Async}",
        Red5LoggingBridgeNativeLayout.CaptureCallWebFuncInvocation(
            invocation.ObjectAvailable, invocation.FunctionExists, invocation.Async)
        == invocation.Expected);
foreach (bool objectAvailable in new[] { false, true })
    Check($"Awesomium AddWebValue object available={objectAvailable}",
        Red5LoggingBridgeNativeLayout.CaptureAddWebValueDispatch(objectAvailable)
        == (objectAvailable
            ? Red5LuaWebValueAction.SetPropertyAsynchronously
            : Red5LuaWebValueAction.ReturnWithoutSettingForUnavailableObject));
foreach (bool incomingFlag in new[] { false, true })
foreach (bool ownerFlag in new[] { false, true })
    Check($"Awesomium URL dispatch setup mode incoming={incomingFlag}, owner flag={ownerFlag}",
        Red5LoggingBridgeNativeLayout.CaptureDispatchObjectSetupMode(incomingFlag, ownerFlag)
        == (incomingFlag && ownerFlag ? 2 : 0));
foreach (var urlCase in new (string Input, Red5ParsedUrl? Expected)[]
{
    ("HTTP://Example.invalid", new Red5ParsedUrl(false, "Example.invalid", 80, "/", "")),
    ("hTtPs://Example.invalid/a/b?x=1#fragment",
        new Red5ParsedUrl(true, "Example.invalid", 443, "/a/b", "x=1")),
    ("http://example.invalid?x=1#fragment",
        new Red5ParsedUrl(false, "example.invalid?x=1#fragment", 80, "/", "")),
    ("example.invalid:8443/path?x=1#fragment",
        new Red5ParsedUrl(false, "example.invalid", 8443, "/path", "x=1")),
    ("example.invalid/path#fragment?ignored",
        new Red5ParsedUrl(false, "example.invalid", 80, "/path", "")),
    ("http://example.invalid:80suffix/path",
        new Red5ParsedUrl(false, "example.invalid", 80, "/path", "")),
    ("http://example.invalid: +80/path",
        new Red5ParsedUrl(false, "example.invalid", 80, "/path", "")),
    ("ftp://example.invalid/path", null),
    ("http:///path", null),
    ("http://example.invalid:0/path", null),
    ("http://example.invalid:65536/path", null),
    ("http://example.invalid:-1/path", null),
    ("http://[::1]/path", null),
    (new string('a', 2049), new Red5ParsedUrl(false, new string('a', 2048), 80, "/", ""))
})
    Check($"Awesomium native URL component parser: {urlCase.Input}",
        Red5LoggingBridgeNativeLayout.CaptureUrlComponents(urlCase.Input) == urlCase.Expected);
Check("Awesomium native URL component parser rejects null",
    Red5LoggingBridgeNativeLayout.CaptureUrlComponents(null) is null);
foreach (var keyCase in new (Red5ParsedUrl? Url, string? Expected)[]
{
    (new Red5ParsedUrl(false, "www.Example.COM", 80, "/", ""), "www.example.com:80"),
    (new Red5ParsedUrl(true, "Example.COM", 443, "/", ""), "example.com:443"),
    (null, null)
})
    Check($"ClientApi URL host-port lookup key: {keyCase.Expected}",
        Red5LoggingBridgeNativeLayout.CaptureUrlHostPortLookupKey(keyCase.Url) == keyCase.Expected);
foreach (var entryCase in new (Red5ParsedUrl? Url, int Value, string Expected)[]
{
    (new Red5ParsedUrl(false, "www.Example.COM", 80, "/", ""), 37,
        "www.example.com:80=37,.example.com=37,.com=37"),
    (new Red5ParsedUrl(true, "api.Example.COM", 8443, "/", ""), 12,
        "api.example.com:8443=12"),
    (null, 99, "")
})
    Check($"ClientApi server clock-offset entries: {entryCase.Expected}",
        string.Join(",", Red5LoggingBridgeNativeLayout.CaptureClientApiServerClockOffsetEntries(
            entryCase.Url, entryCase.Value).Select(entry => $"{entry.Key}={entry.OffsetSeconds}"))
        == entryCase.Expected);
foreach (var suffixCase in new (Red5ParsedUrl? Url, string Expected)[]
{
    (new Red5ParsedUrl(false, "www.Example.COM", 80, "/", ""), ".example.com,.com"),
    (new Red5ParsedUrl(true, "a.B.Example.COM", 443, "/", ""), ".b.example.com,.example.com,.com"),
    (new Red5ParsedUrl(false, "www.Example.COM", 8080, "/", ""), ""),
    (new Red5ParsedUrl(true, "Example", 443, "/", ""), ""),
    (null, "")
})
    Check($"ClientApi URL suffix lookup candidates: {suffixCase.Expected}",
        string.Join(",", Red5LoggingBridgeNativeLayout.CaptureDefaultPortHostSuffixLookupKeys(
            suffixCase.Url)) == suffixCase.Expected);
Check("ClientApi adjusted request timestamp applies the signed server clock offset with 32-bit wraparound",
    Red5LoggingBridgeNativeLayout.CaptureClientApiAdjustedTimestamp(1_700_000_000, -300)
        == 1_699_999_700
    && Red5LoggingBridgeNativeLayout.CaptureClientApiAdjustedTimestamp(int.MaxValue, 1)
        == int.MinValue);
Check("ClientApi request timing defaults use the current tick and its wrapping predecessor",
    Red5LoggingBridgeNativeLayout.CaptureClientApiRequestTimerDefaults(1234)
        == new ClientApiRequestTimerDefaults(1234, 1233)
    && Red5LoggingBridgeNativeLayout.CaptureClientApiRequestTimerDefaults(0)
        == new ClientApiRequestTimerDefaults(0, uint.MaxValue));
Check("ClientApi request query formats adjusted time, four nonce words, and 64-bit client ID",
    Red5LoggingBridgeNativeLayout.CaptureClientApiRequestQuery(
        1_700_000_000, 1, 0xABCD, 15, ushort.MaxValue,
        "user", "api.example.com", "/v1", "x=1", ulong.MaxValue)
    == "ver=2&tc=1700000000&nonce=0001abcd000fffff&uid=user&host=api.example.com"
        + "&path=/v1&hbody=x=1&cid=18446744073709551615");
Check("ClientApi signatures use lowercase-hex HMAC-SHA1",
    Red5LoggingBridgeNativeLayout.CaptureClientApiRequestSignature(
        System.Text.Encoding.ASCII.GetBytes("key"),
        System.Text.Encoding.ASCII.GetBytes("The quick brown fox jumps over the lazy dog"))
    == "de7c9b85b8b78aa6bc8a7a36f70a90701c9db4d9");
Check("ClientApi request header lines use colon-space and CRLF",
    Red5LoggingBridgeNativeLayout.CaptureClientApiRequestHeaderLine(
        "X-Red5-Signature", "signature") == "X-Red5-Signature: signature\r\n");
foreach (var escapeCase in new (byte[] Input, string Expected)[]
{
    (Array.Empty<byte>(), ""),
    (new byte[] { (byte)'A', (byte)'z', (byte)'0', (byte)'9', (byte)'-', (byte)'.', (byte)'_', (byte)'~' },
        "Az09-._~"),
    (new byte[] { (byte)'/', (byte)'?', (byte)'&', (byte)'=', (byte)' ' }, "%2F%3F%26%3D%20"),
    (new byte[] { 0x00, 0x7F, 0x80, 0xFF }, "%00%7F%80%FF")
})
    Check($"Awesomium URL component escaping: {escapeCase.Expected}",
        Red5LoggingBridgeNativeLayout.CaptureUrlComponentEscape(escapeCase.Input)
        == escapeCase.Expected);
foreach (bool releaseAllocation in new[] { false, true })
    Check($"Awesomium URL dispatch deletion release allocation={releaseAllocation}",
        Red5LoggingBridgeNativeLayout.CaptureDispatchObjectDelete(releaseAllocation)
        == (releaseAllocation
            ? Red5DispatchObjectDeleteAction.ResetAndReleaseAllocation
            : Red5DispatchObjectDeleteAction.ResetAndPreserveAllocation));
foreach (int mode in new[] { 0, 1, 2, -1 })
foreach (bool stateStringStorage in new[] { false, true })
foreach (bool helperPresent in new[] { false, true })
foreach (bool firstResultPresent in new[] { false, true })
    Check($"Awesomium URL dispatch pending mode flag mode={mode}, storage={stateStringStorage}, helper={helperPresent}, first={firstResultPresent}",
        Red5LoggingBridgeNativeLayout.CaptureDispatchObjectSetsPendingModeFlag(
            mode, stateStringStorage, helperPresent, firstResultPresent)
        == (mode != 0 && stateStringStorage && mode != 1 && helperPresent && !firstResultPresent));
foreach (var filter in new (string? Value, Red5LuaUrlFilterAction Expected)[]
{
    (null, Red5LuaUrlFilterAction.LogAndSkip),
    ("", Red5LuaUrlFilterAction.LogAndSkip),
    ("*", Red5LuaUrlFilterAction.StoreInReplacementList),
    ("https://host/path/*", Red5LuaUrlFilterAction.StoreInReplacementList),
    ("https://host/path/*suffix", Red5LuaUrlFilterAction.LogAndSkip),
    ("https://host/path/", Red5LuaUrlFilterAction.LogAndSkip)
})
    Check($"Awesomium SetUrlFilters accepts only nonempty trailing-wildcard patterns: {filter.Value}",
        Red5LoggingBridgeNativeLayout.CaptureSetUrlFilter(filter.Value) == filter.Expected);
foreach (var match in new[]
{
    (Filters: new[] { "https://host/path/*" }, Candidate: "https://host/path/page", Expected: true),
    (Filters: new[] { "https://host/path/*" }, Candidate: "https://host/path", Expected: false),
    (Filters: new[] { "https://host/path/*" }, Candidate: "https://host/other/page", Expected: false),
    (Filters: new[] { "https://host/path/*" }, Candidate: "HTTPS://host/path/page", Expected: false),
    (Filters: new[] { "*" }, Candidate: "", Expected: true),
    (Filters: new[] { "https://one/*", "https://two/*" }, Candidate: "https://two/page", Expected: true),
    (Filters: new[] { "" }, Candidate: "anything", Expected: false)
})
    Check($"Awesomium URL filter list uses ordinal prefix matching: {match.Candidate}",
        Red5LoggingBridgeNativeLayout.CaptureUrlFilterListMatch(match.Filters, match.Candidate)
        == match.Expected);
foreach (var callback in new[]
{
    (ContextUnchanged: true, Filters: new[] { "*" }, Candidate: "any",
        Expected: Red5UrlFilterCallbackAction.ReturnFalseForUnchangedContext),
    (ContextUnchanged: false, Filters: new[] { "https://host/*" }, Candidate: "https://host/page",
        Expected: Red5UrlFilterCallbackAction.ReturnFalseForMatchingFilter),
    (ContextUnchanged: false, Filters: new[] { "https://host/*" }, Candidate: "https://other/page",
        Expected: Red5UrlFilterCallbackAction.ReturnTrueForNoMatchingFilter)
})
    Check($"Awesomium URL filter callback decision, context unchanged={callback.ContextUnchanged}",
        Red5LoggingBridgeNativeLayout.CaptureUrlFilterCallback(
            callback.ContextUnchanged, callback.Filters, callback.Candidate) == callback.Expected);
foreach (bool truthy in new[] { false, true })
    Check($"Awesomium LogWebRequests stores Lua truthiness={truthy}",
        Red5LoggingBridgeNativeLayout.CaptureLogWebRequests(truthy)
        == (truthy ? Red5LuaWebRequestLoggingAction.Enable : Red5LuaWebRequestLoggingAction.Disable));
foreach (var load in new[]
{
    (FrameValid: false, UrlConvertible: true, WebObjectAvailable: true,
        Expected: Red5LuaLoadUrlAction.ReportInvalidFrame),
    (FrameValid: true, UrlConvertible: false, WebObjectAvailable: true,
        Expected: Red5LuaLoadUrlAction.ReportInvalidUrlValue),
    (FrameValid: true, UrlConvertible: true, WebObjectAvailable: false,
        Expected: Red5LuaLoadUrlAction.ReturnWithoutLoading),
    (FrameValid: true, UrlConvertible: true, WebObjectAvailable: true,
        Expected: Red5LuaLoadUrlAction.LoadAndReturnTrue)
})
    Check($"Awesomium LoadUrl decision, valid frame={load.FrameValid}, convertible URL={load.UrlConvertible}, web object={load.WebObjectAvailable}",
        Red5LoggingBridgeNativeLayout.CaptureLoadUrl(
            load.FrameValid, load.UrlConvertible, load.WebObjectAvailable) == load.Expected);
foreach (string? url in new string?[] { null, "", "https://example.invalid/" })
    Check($"Awesomium GetUrl null-pointer fallback: {url ?? "<null>"}",
        Red5LoggingBridgeNativeLayout.CaptureGetUrlResult(url) == (url ?? string.Empty));
foreach (var dimensions in new[]
{
    (CurrentWidth: 800f, CurrentHeight: 600f, RequestedWidth: 800f, RequestedHeight: 600f,
        Expected: Red5NaturalDimensionsAction.KeepExistingDimensions),
    (CurrentWidth: 800f, CurrentHeight: 600f, RequestedWidth: 1024f, RequestedHeight: 600f,
        Expected: Red5NaturalDimensionsAction.UpdateAndNotify),
    (CurrentWidth: 800f, CurrentHeight: 600f, RequestedWidth: 800f, RequestedHeight: 768f,
        Expected: Red5NaturalDimensionsAction.UpdateAndNotify),
    (CurrentWidth: float.NaN, CurrentHeight: 600f, RequestedWidth: float.NaN, RequestedHeight: 600f,
        Expected: Red5NaturalDimensionsAction.UpdateAndNotify)
})
    Check($"Awesomium SetNaturalDims comparison {dimensions.CurrentWidth}x{dimensions.CurrentHeight} -> {dimensions.RequestedWidth}x{dimensions.RequestedHeight}",
        Red5LoggingBridgeNativeLayout.CaptureSetNaturalDimensions(
            dimensions.CurrentWidth, dimensions.CurrentHeight,
            dimensions.RequestedWidth, dimensions.RequestedHeight) == dimensions.Expected);
foreach (var scale in new[]
{
    (ViewportWidth: 800, ViewportHeight: 600, NaturalWidth: 800f, NaturalHeight: 600f, Expected: 100),
    (ViewportWidth: 400, ViewportHeight: 300, NaturalWidth: 800f, NaturalHeight: 600f, Expected: 50),
    (ViewportWidth: 1600, ViewportHeight: 1200, NaturalWidth: 800f, NaturalHeight: 600f, Expected: 200),
    (ViewportWidth: 800, ViewportHeight: 600, NaturalWidth: 0.5f, NaturalHeight: 0.5f, Expected: 100),
    (ViewportWidth: 800, ViewportHeight: 600, NaturalWidth: 10000f, NaturalHeight: 10000f, Expected: 20),
    (ViewportWidth: 10000, ViewportHeight: 10000, NaturalWidth: 100f, NaturalHeight: 100f, Expected: 500)
})
    Check($"Awesomium natural-dimension fit scale is clamped/rounded: {scale.NaturalWidth}x{scale.NaturalHeight} in {scale.ViewportWidth}x{scale.ViewportHeight}",
        Red5LoggingBridgeNativeLayout.CaptureNaturalFitScalePercent(
            scale.ViewportWidth, scale.ViewportHeight,
            scale.NaturalWidth, scale.NaturalHeight) == scale.Expected);
foreach (var flag in new[]
{
    (Current: (byte)0, Requested: false, Expected: Red5TypeTargetFlagAction.LeaveUnchanged),
    (Current: (byte)0, Requested: true, Expected: Red5TypeTargetFlagAction.UpdateFlag),
    (Current: (byte)1, Requested: false, Expected: Red5TypeTargetFlagAction.UpdateFlag),
    (Current: (byte)1, Requested: true, Expected: Red5TypeTargetFlagAction.LeaveUnchanged),
    (Current: (byte)2, Requested: true, Expected: Red5TypeTargetFlagAction.LeaveUnchanged)
})
    Check($"Awesomium EnableTypeTarget flag transition {flag.Current} -> {flag.Requested}",
        Red5LoggingBridgeNativeLayout.CaptureEnableTypeTarget(flag.Current, flag.Requested) == flag.Expected
        && Red5LoggingBridgeNativeLayout.CaptureIsTypeTarget(flag.Current) == (flag.Current != 0));
foreach (var mappedArguments in new[]
{
    (HasArguments: false, IsObject: false, CallableLike: false,
        Expected: Red5MappedCallbackArgumentAction.PreserveAllArguments),
    (HasArguments: true, IsObject: false, CallableLike: false,
        Expected: Red5MappedCallbackArgumentAction.PreserveAllArguments),
    (HasArguments: true, IsObject: true, CallableLike: false,
        Expected: Red5MappedCallbackArgumentAction.PreserveAllArguments),
    (HasArguments: true, IsObject: true, CallableLike: true,
        Expected: Red5MappedCallbackArgumentAction.SeparateTrailingCallableLikeValue)
})
    Check($"Awesomium Red5 mapped callback argument handling, object={mappedArguments.IsObject}, callable={mappedArguments.CallableLike}",
        Red5LoggingBridgeNativeLayout.CaptureMappedCallbackArguments(
            mappedArguments.HasArguments, mappedArguments.IsObject, mappedArguments.CallableLike)
        == mappedArguments.Expected);
foreach (var invocation in new[]
{
    (Undefined: true, Async: false, Expected: Red5QueuedJavaScriptContinuationAction.SkipUndefinedContinuation),
    (Undefined: true, Async: true, Expected: Red5QueuedJavaScriptContinuationAction.SkipUndefinedContinuation),
    (Undefined: false, Async: false, Expected: Red5QueuedJavaScriptContinuationAction.InvokeSynchronously),
    (Undefined: false, Async: true, Expected: Red5QueuedJavaScriptContinuationAction.InvokeAsynchronously)
})
    Check($"Awesomium Red5 JavaScript continuation undefined={invocation.Undefined}, async={invocation.Async}",
        Red5LoggingBridgeNativeLayout.CaptureQueuedJavaScriptContinuation(
            invocation.Undefined, invocation.Async) == invocation.Expected);
foreach (var pointerCleanup in new[]
{
    (IsNull: true, IsSentinel: false, Expected: Red5BridgePointerCleanupAction.ClearNullPointer),
    (IsNull: true, IsSentinel: true, Expected: Red5BridgePointerCleanupAction.PreserveSharedSentinel),
    (IsNull: false, IsSentinel: true, Expected: Red5BridgePointerCleanupAction.PreserveSharedSentinel),
    (IsNull: false, IsSentinel: false, Expected: Red5BridgePointerCleanupAction.ReleaseAndClearPointer)
})
    Check($"Awesomium Red5 bridge pointer cleanup null={pointerCleanup.IsNull}, sentinel={pointerCleanup.IsSentinel}",
        Red5LoggingBridgeNativeLayout.CaptureBridgePointerCleanup(
            pointerCleanup.IsNull, pointerCleanup.IsSentinel) == pointerCleanup.Expected);
foreach (uint fileHandle in new uint[] { 0, 1, 0x80000000, uint.MaxValue })
    Check($"Awesomium configuration closes nonzero prior file handle {fileHandle:X8}",
        LogFileNativeLayout.CaptureClosesPriorFileHandle(fileHandle) == (fileHandle != 0));
foreach (int destination in new[] { int.MinValue, -1, 0, 1, 2, 3, 4, int.MaxValue })
    Check($"Awesomium configuration replaces path only outside destinations zero and two: {destination}",
        LogFileNativeLayout.CaptureReplacesConfiguredPath(destination) == (destination != 0 && destination != 2));
foreach (bool consumed in new[] { false, true })
foreach (int destination in new[] { int.MinValue, -1, 0, 1, 2, 3, 4, int.MaxValue })
foreach (int severity in new[] { int.MinValue, -1, 0, 1, 2, 3, 4, int.MaxValue })
{
    var sinks = LogSinkNativeLayout.CaptureSinkAdmission(consumed, destination, severity);
    Check($"Awesomium sink admission consumed={consumed}, destination={destination}, severity={severity}",
        sinks.OutputDebugString == (!consumed && destination is 2 or 3)
        && sinks.StandardErrorAndFlush == (!consumed && (destination is 2 or 3 || severity >= 2))
        && sinks.EnterFileOutputBlock == (!consumed && destination != 0 && destination != 2));
}
foreach (bool entered in new[] { false, true })
foreach (bool opened in new[] { false, true })
    Check($"Awesomium file write admission entered={entered}, helper={opened}",
        LogSinkNativeLayout.CaptureFileWriteAdmission(entered, opened) == (entered && opened));
foreach (uint stored in new uint[] { 0, 1, 0x80000000, uint.MaxValue })
foreach (int destination in new[] { int.MinValue, -1, 0, 1, 2, 3, 4, int.MaxValue })
    Check($"Awesomium log file entry handle={stored:X8}, destination={destination}",
        LogFileNativeLayout.CaptureEntry(stored, destination) ==
        (stored != 0 ? LogFileEntryAction.ReturnTrueForNonzeroStoredHandle
            : destination is 1 or 3 ? LogFileEntryAction.PreparePathThenAttemptConfiguredOpen
            : LogFileEntryAction.PreparePathThenReturnTrueWithoutOpen));
foreach (uint result in new uint[] { 0, 1, 0x80000000, uint.MaxValue })
foreach (bool fallback in new[] { false, true })
    Check($"Awesomium log open result={result:X8}, fallback={fallback}",
        LogFileNativeLayout.CaptureOpenResult(result, fallback) ==
        (result != 0 && result != uint.MaxValue ? LogFileOpenAction.SeekEndThenReturnTrue
            : fallback ? LogFileOpenAction.ClearStoredHandleThenReturnFalse : LogFileOpenAction.AttemptFallbackOpen));
foreach (int status in new[] { 200, 201, 206, 416, 500 })
foreach (int count in new[] { 0, 9, 10, 11 })
{
    var expected = status == 200
        ? new DownloadResponseDecision(count >= 10 ? DownloadResponseAction.RetryLimitExceeded
            : DownloadResponseAction.RetryRequest, count + 1)
        : new DownloadResponseDecision(status == 206 ? DownloadResponseAction.ReadResponseStream
            : DownloadResponseAction.ReadUnexpectedStatusResponseStream, count);
    Check($"DeploymentCommon observed response status {status}, prior retry count {count}",
        DownloadResponseNativeLayout.CaptureStatusCode(status, count) == expected);
}
foreach (int count in new[] { 0, 9, 10, 11, int.MaxValue })
    Check($"DeploymentCommon MD5 mismatch increments shared counter {count}",
        DownloadResponseNativeLayout.CaptureMd5Mismatch(count) ==
        new DownloadResponseDecision(unchecked(count + 1) > 10
            ? DownloadResponseAction.RetryLimitExceeded : DownloadResponseAction.RetryRequest,
            unchecked(count + 1)));
Check("DeploymentCommon status-200 retry then MD5 mismatch share one counter",
    DownloadResponseNativeLayout.CaptureMd5Mismatch(
        DownloadResponseNativeLayout.CaptureStatusCode(200, 9).RetryCount)
        == new DownloadResponseDecision(DownloadResponseAction.RetryLimitExceeded, 11));
long downloadChunkSize = DownloadPlanning.ChunkSize;
var downloadRangeCases = new (long Total, (uint Part, long Start, long End)[] Expected)[]
{
    (-1, []), (long.MinValue, []), (0, []),
    (1, [(0, 0, 1)]),
    (downloadChunkSize - 1, [(0, 0, downloadChunkSize - 1)]),
    (downloadChunkSize, [(0, 0, downloadChunkSize)]),
    (downloadChunkSize + 1, [(0, 0, downloadChunkSize)]),
    (downloadChunkSize + 2, [(0, 0, downloadChunkSize), (1, downloadChunkSize + 1, downloadChunkSize + 1)]),
    (2 * downloadChunkSize, [(0, 0, downloadChunkSize), (1, downloadChunkSize + 1, 2 * downloadChunkSize - 1)]),
    (2 * downloadChunkSize + 1, [(0, 0, downloadChunkSize), (1, downloadChunkSize + 1, 2 * downloadChunkSize)]),
    (2 * downloadChunkSize + 2, [(0, 0, downloadChunkSize), (1, downloadChunkSize + 1, 2 * downloadChunkSize),
        (2, 2 * downloadChunkSize + 1, 2 * downloadChunkSize + 1)])
};
var downloadPartMd5s = new Dictionary<uint, string> { [0] = "m0", [1] = "m1", [2] = "m2" };
foreach (var test in downloadRangeCases)
{
    var chunks = DownloadPlanning.PlanChunks("range", test.Total, downloadPartMd5s);
    Check($"DeploymentCommon zero-based inclusive range boundaries for total {test.Total}",
        chunks.Select(x => (x.PartNumber, x.StartOffset, x.EndOffset)).SequenceEqual(test.Expected)
        && chunks.Select(x => x.Length).SequenceEqual(test.Expected.Select(r => r.End - r.Start + 1))
        && chunks.Select(x => x.LocalChunkPath).SequenceEqual(test.Expected.Select(
            r => "range.part" + r.Part.ToString(System.Globalization.CultureInfo.InvariantCulture)))
        && chunks.Select(x => x.PartMd5).SequenceEqual(test.Expected.Select(
            r => "m" + r.Part.ToString(System.Globalization.CultureInfo.InvariantCulture))));
}
Check("DeploymentCommon missing zero-based part digest remains absent",
    DownloadPlanning.PlanChunks("range", 1, new Dictionary<uint, string> { [1] = "wrong" })
        .Single().PartMd5 is null);

foreach (var entry in new (uint Scalar, DiagnosticScalarAppend Expected)[]
{
    (0, new(true, 0, null)), (0x41, new(true, 0x41, null)),
    (0x7F, new(true, 0x7F, null)), (0x80, new(true, 0x80, null)),
    (0xD7FF, new(true, 0xD7FF, null)),
    (0xD800, new(false, 0xFFFD, null)), (0xDFFF, new(false, 0xFFFD, null)),
    (0xE000, new(true, 0xE000, null)), (0xFDD0, new(true, 0xFDD0, null)),
    (0xFFFE, new(true, 0xFFFE, null)), (0xFFFF, new(true, 0xFFFF, null)),
    (0x10000, new(true, 0xD800, 0xDC00)), (0x1F600, new(true, 0xD83D, 0xDE00)),
    (0x10FFFF, new(true, 0xDBFF, 0xDFFF)),
    (0x110000, new(false, 0xFFFD, null)), (uint.MaxValue, new(false, 0xFFFD, null))
})
    Check($"Awesomium diagnostic scalar append {entry.Scalar:X}",
        DiagnosticEncodingNativeLayout.CaptureScalarAppend(entry.Scalar) == entry.Expected);
foreach (var length in new (uint Count, bool Admitted)[]
{
    (0, false), (1, true), (127, true), (int.MaxValue, true),
    (0x80000000, false), (uint.MaxValue, false)
})
    Check($"Awesomium diagnostic decode loop uses signed-positive input length {length.Count}",
        DiagnosticEncodingNativeLayout.CaptureDecodeLoopAdmission(length.Count) == length.Admitted);
Check("Awesomium diagnostic conversion error remains sticky through later valid scalars",
    DiagnosticEncodingNativeLayout.CaptureAggregateConversionSuccess(true, true)
    && !DiagnosticEncodingNativeLayout.CaptureAggregateConversionSuccess(true, false)
    && !DiagnosticEncodingNativeLayout.CaptureAggregateConversionSuccess(false, true)
    && !DiagnosticEncodingNativeLayout.CaptureAggregateConversionSuccess(false, false));
foreach (bool disabled in new[] { false, true })
foreach (bool childType in new[] { false, true })
foreach (int observedLevel in new[] { int.MinValue, -2, 0, 2, 3, 4, int.MaxValue })
{
    var setup = LoggingSetupNativeLayout.CaptureNormalSetup(disabled, childType, observedLevel);
    Check($"Awesomium normal logging setup disabled={disabled}, type={childType}, output={observedLevel}",
        setup.OutputDestination == (disabled ? 0 : 1)
        && setup.AppendExistingLog == childType && setup.DiagnosticChecksMode == 0
        && setup.MinimumSeverityAfterNormalSetup == (observedLevel >= 3 ? 3 : observedLevel));
}
foreach (bool nonempty in new[] { false, true })
foreach (bool succeeded in new[] { false, true })
foreach (int observedVerbose in new[] { int.MinValue, -1, 0, 2, int.MaxValue })
    Check($"Awesomium verbose threshold write nonempty={nonempty}, success={succeeded}, output={observedVerbose}",
        LoggingSetupNativeLayout.CaptureVerboseThresholdAssignment(7, nonempty, succeeded, observedVerbose)
        == (nonempty && succeeded ? unchecked(-observedVerbose) : 7));
Check("Awesomium normal caller threshold override supersedes an earlier verbose assignment",
    LoggingSetupNativeLayout.CaptureVerboseThresholdAssignment(0, true, true, 5) == -5
    && LoggingSetupNativeLayout.CaptureNormalSetup(false, false, 2).MinimumSeverityAfterNormalSetup == 2);
foreach (int severity in new[] { int.MinValue, -1, 0, 1, 2, 3, 4, int.MaxValue })
    Check($"Awesomium minimum severity assignment signed upper clamp {severity}",
        ConversionDiagnosticNativeLayout.CaptureMinimumSeverityAssignment(severity)
            == (severity >= 3 ? 3 : severity));
foreach (bool initializationFailed in new[] { false, true })
foreach (bool singleProcess in new[] { false, true })
foreach (bool inProcessGpu in new[] { false, true })
    Check($"Awesomium GPU interceptor installation failed={initializationFailed}, single={singleProcess}, inProcess={inProcessGpu}",
        ConversionDiagnosticNativeLayout.CaptureGpuInterceptorInstallation(
            initializationFailed, singleProcess, inProcessGpu)
        == (!initializationFailed && !singleProcess && !inProcessGpu));
Check("Awesomium normally returning GPU interceptor does not consume severity-three default handling",
    ConversionDiagnosticNativeLayout.CaptureRoute(true,
        ConversionDiagnosticNativeLayout.GpuInterceptorConsumesDiagnostic, false)
        == SeverityThreeDiagnosticRoute.DefaultDisplayHelper);
foreach (bool present in new[] { false, true })
foreach (bool consumed in new[] { false, true })
foreach (bool handler in new[] { false, true })
    Check($"Awesomium severity-three route interceptor={present}, consumed={consumed}, handler={handler}",
        ConversionDiagnosticNativeLayout.CaptureRoute(present, consumed, handler) ==
        (present && consumed ? SeverityThreeDiagnosticRoute.ConsumedByInterceptor
            : handler ? SeverityThreeDiagnosticRoute.CustomHandler
            : SeverityThreeDiagnosticRoute.DefaultDisplayHelper));
foreach (bool message in new[] { false, true })
foreach (bool enabled in new[] { false, true })
foreach (bool converted in new[] { false, true })
    Check($"Awesomium default diagnostic admission message={message}, enabled={enabled}, converted={converted}",
        ConversionDiagnosticNativeLayout.CaptureDefaultDisplayAdmission(message, enabled, converted) ==
        (message && enabled && converted ? DiagnosticDisplayAction.AttemptHelperLaunch
            : DiagnosticDisplayAction.ReturnWithoutDisplay));
Check("Awesomium default diagnostic waits after launch and falls back to UI on launch failure",
    ConversionDiagnosticNativeLayout.CaptureLaunchResult(true) == DiagnosticLaunchAction.WaitIndefinitelyThenCloseHandles
    && ConversionDiagnosticNativeLayout.CaptureLaunchResult(false) == DiagnosticLaunchAction.ShowMessageBox);
for (int value = 0; value <= byte.MaxValue; value++)
    Check($"Awesomium byte lowercase preserves non-ASCII and folds only A-Z, byte {value:X2}",
        SwitchNameNativeLayout.CaptureAsciiLowercaseByte((byte)value)
        == (value >= 0x41 && value <= 0x5A ? value + 0x20 : value));
foreach (ushort unit in new ushort[] { 0, 0x41, 0x7F, 0x80, 0xFF, 0x100, 0x141, 0xD800, 0xFFFF })
    Check($"Awesomium returning converter keeps only low byte of code unit {unit:X4}",
        SwitchNameNativeLayout.CaptureNarrowedCodeUnit(unit) == (unit & 0xFF));
foreach (var value in new (string Text, bool Ascii)[]
{
    ("", true), ("Renderer", true), ("\0A\0", true), ("\u007F", true),
    ("\u0080", false), ("\u0141", false), ("render\u0100er", false),
    ("\uD800", false), ("\uD83D\uDE00", false)
})
    Check($"Awesomium wide ASCII value admission for {Convert.ToHexString(System.Text.Encoding.Unicode.GetBytes(value.Text))}",
        SwitchNameNativeLayout.CaptureWideAsciiAdmission(value.Text) == value.Ascii);
foreach (var diagnostic in new (bool Mode, int Threshold, string Text, bool Expected)[]
{
    (false, 0, "\u0100", false), (true, 4, "\u0100", false),
    (true, 3, "\u0100", true), (true, int.MinValue, "\u0100", true),
    (true, 3, "TYPE", false), (true, 3, "", false)
})
    Check($"Awesomium converter diagnostic admission mode={diagnostic.Mode}, threshold={diagnostic.Threshold}, length={diagnostic.Text.Length}",
        SwitchNameNativeLayout.CaptureConversionDiagnosticAdmission(diagnostic.Mode,
            diagnostic.Threshold, diagnostic.Text) == diagnostic.Expected);
Check("Awesomium uppercase name bytes fold to type and mixed-case value passes ASCII gate",
    new byte[]
    {
        SwitchNameNativeLayout.CaptureAsciiLowercaseByte((byte)'T'),
        SwitchNameNativeLayout.CaptureAsciiLowercaseByte((byte)'Y'),
        SwitchNameNativeLayout.CaptureAsciiLowercaseByte((byte)'P'),
        SwitchNameNativeLayout.CaptureAsciiLowercaseByte((byte)'E')
    }.SequenceEqual("type"u8.ToArray())
    && SwitchNameNativeLayout.CaptureWideAsciiAdmission("Renderer"));
foreach (var value in new (bool Open, uint Count, DirectXVersionReadDecision Expected)[]
{
    (false, 127, new(DirectXVersionReadAction.UseInitializedEmptyBuffer, null)),
    (false, uint.MaxValue, new(DirectXVersionReadAction.UseInitializedEmptyBuffer, null)),
    (true, 0, new(DirectXVersionReadAction.WriteWideTerminatorThenCloseKey, 0)),
    (true, 20, new(DirectXVersionReadAction.WriteWideTerminatorThenCloseKey, 40)),
    (true, 63, new(DirectXVersionReadAction.WriteWideTerminatorThenCloseKey, 126)),
    (true, 64, new(DirectXVersionReadAction.RangeCheckFailure, null)),
    (true, 127, new(DirectXVersionReadAction.RangeCheckFailure, null)),
    (true, uint.MaxValue, new(DirectXVersionReadAction.RangeCheckFailure, null)),
    (true, 0x80000000, new(DirectXVersionReadAction.WriteWideTerminatorThenCloseKey, 0)),
    (true, 0x80000040, new(DirectXVersionReadAction.RangeCheckFailure, null))
})
    Check($"DirectX Version buffer gate opened={value.Open}, byte count={value.Count}",
        DirectXVersionNativeLayout.CaptureReadDecision(value.Open, value.Count) == value.Expected);
foreach (var startup in new (bool Pipe, bool Process, bool Query, HardwareSurveyCaptureDecision Expected)[]
{
    (false, true, true, new(HardwareSurveyCaptureAction.ReturnFailure, 0, 0, uint.MaxValue, false, false)),
    (true, false, true, new(HardwareSurveyCaptureAction.ReturnFailure, 0, 0, uint.MaxValue, false, true)),
    (true, true, false, new(HardwareSurveyCaptureAction.AppendNulAndReturn, 0, 0, uint.MaxValue, true, true)),
    (true, true, true, new(HardwareSurveyCaptureAction.PollPipe, 0, 0, null, false, false))
})
    Check($"HWStats capture startup pipe={startup.Pipe}, process={startup.Process}, query={startup.Query}",
        HardwareSurveyCaptureNativeLayout.CaptureStartup(startup.Pipe, startup.Process, startup.Query) == startup.Expected);
foreach (var poll in new (bool Peek, uint Available, uint ExitCode, HardwareSurveyCaptureDecision Expected)[]
{
    (true, 1, 259, new(HardwareSurveyCaptureAction.ReadPipe, 0x800, 0, null, false, false)),
    (true, 0x1000, 7, new(HardwareSurveyCaptureAction.ReadPipe, 0x800, 0, null, false, false)),
    (false, 0, 259, new(HardwareSurveyCaptureAction.SleepThenQueryExitCode, 0, 0, null, false, false)),
    (true, 0, 259, new(HardwareSurveyCaptureAction.SleepThenQueryExitCode, 0, 0, null, false, false)),
    (false, 17, 0, new(HardwareSurveyCaptureAction.AppendNulAndReturn, 0, 0, 0, true, true)),
    (true, 0, 7, new(HardwareSurveyCaptureAction.AppendNulAndReturn, 0, 0, 7, true, true))
})
    Check($"HWStats capture poll peek={poll.Peek}, available={poll.Available}, exit={poll.ExitCode}",
        HardwareSurveyCaptureNativeLayout.CapturePipePoll(poll.Peek, poll.Available, poll.ExitCode) == poll.Expected);
foreach (uint count in new uint[] { 0, 17, 0x800 })
{
    Check($"HWStats successful read forwards actual count {count}",
        HardwareSurveyCaptureNativeLayout.CaptureReadResult(true, count)
        == new HardwareSurveyCaptureDecision(HardwareSurveyCaptureAction.ForwardBytesThenPoll, 0, count, null, false, false));
    Check($"HWStats failed read does not forward count {count} or append NUL",
        HardwareSurveyCaptureNativeLayout.CaptureReadResult(false, count)
        == new HardwareSurveyCaptureDecision(HardwareSurveyCaptureAction.ReturnFailure, 0, 0, uint.MaxValue, false, true));
}
Check("HWStats failed exit query appends NUL and enters cleanup",
    HardwareSurveyCaptureNativeLayout.CaptureFollowupExitCodeQuery(false)
    == new HardwareSurveyCaptureDecision(HardwareSurveyCaptureAction.AppendNulAndReturn, 0, 0, uint.MaxValue, true, true));
Check("HWStats successful exit query resumes pipe polling",
    HardwareSurveyCaptureNativeLayout.CaptureFollowupExitCodeQuery(true)
    == new HardwareSurveyCaptureDecision(HardwareSurveyCaptureAction.PollPipe, 0, 0, null, false, false));
foreach (var entry in new (ulong Bytes, RamLineArguments Expected)[]
{
    (0, new(RamAndProcessorNativeLayout.SmallRamFormat, 0, 0, null)),
    ((1UL << 30) - 1, new(RamAndProcessorNativeLayout.SmallRamFormat, 0x3FFFFFFF, 0, null)),
    (1UL << 30, new(RamAndProcessorNativeLayout.GigabytesRamFormat, null, null, 1)),
    ((1UL << 30) + (1UL << 20) - 1, new(RamAndProcessorNativeLayout.GigabytesRamFormat, null, null, 1)),
    ((1UL << 30) + (1UL << 20), new(RamAndProcessorNativeLayout.GigabytesRamFormat, null, null, 1.0009765625)),
    (1UL << 51, new(RamAndProcessorNativeLayout.GigabytesRamFormat, null, null, 2097152)),
    (1UL << 52, new(RamAndProcessorNativeLayout.SmallRamFormat, 0, 0x100000, null))
})
    Check($"RAM native variadic argument capture {entry.Bytes}",
        RamAndProcessorNativeLayout.CaptureRamArguments(entry.Bytes) == entry.Expected);
foreach (var masks in new (uint Process, uint System, int Count)[]
{
    (0, 0, 0), (1, 0, 0), (0, 1, 1), (1, 0xFF, 8),
    (uint.MaxValue, 0x80000000, 1), (1, uint.MaxValue, 32),
    (0xFFFF, 0xAAAAAAAA, 16)
})
    Check($"Processor count uses system mask {masks.System:X8}, not process mask {masks.Process:X8}",
        RamAndProcessorNativeLayout.CaptureProcessorCount(masks.Process, masks.System) == masks.Count);
Check("BIOS requests preserve native order",
    BiosRegistryNativeLayout.ValueNames.SequenceEqual(
        ["Identifier", "SystemBiosDate", "SystemBiosVersion", "VideoBiosDate", "VideoBiosVersion"]));
foreach (uint status in new uint[] { 0, 2, 5, 234, uint.MaxValue })
    Check($"BIOS helper collapses registry status {status} to HRESULT",
        BiosRegistryNativeLayout.CaptureQueryHelperResult(status) ==
        (status == 0 ? 0 : 0x80004005));
foreach (uint type in new uint[] { 0, 1, 2, 3, 4, 7, 11, uint.MaxValue })
{
    Check($"BIOS success admits only REG_SZ and REG_MULTI_SZ, type {type}",
        BiosRegistryNativeLayout.CapturePrintAdmission(0, type) == (type is 1 or 7));
    Check($"BIOS helper failure suppresses type {type}",
        !BiosRegistryNativeLayout.CapturePrintAdmission(0x80004005, type));
}
foreach (var selection in new (byte[] Statuses, int? Expected)[]
{
    ([], null), ([1], null), ([0], 0), ([0, 1, 0], 0),
    ([1, 0, 0], 1), ([0xFF, 0xFE, 0], 2), ([1, 2, 3], null)
})
    Check($"MAC first zero ASTAT return in {Convert.ToHexString(selection.Statuses)}",
        MacAddressNativeLayout.CaptureFirstSuccessfulAttempt(selection.Statuses) == selection.Expected);
foreach (var gate in new (bool Version, uint Platform, uint Major, bool Module, bool Export, OsDescriptionGate Expected)[]
{
    (false, 0, 0, false, false, OsDescriptionGate.VersionQueryFailed),
    (false, 2, 6, true, true, OsDescriptionGate.VersionQueryFailed),
    (true, 1, 6, true, true, OsDescriptionGate.PlatformRejected),
    (true, uint.MaxValue, 6, true, true, OsDescriptionGate.PlatformRejected),
    (true, 2, 0, true, true, OsDescriptionGate.MajorVersionRejected),
    (true, 2, 4, true, true, OsDescriptionGate.MajorVersionRejected),
    (true, 2, 5, false, false, OsDescriptionGate.ContinueFormatting),
    (true, 2, 6, false, true, OsDescriptionGate.ProductInfoModuleMissing),
    (true, 2, 6, true, false, OsDescriptionGate.ProductInfoExportMissing),
    (true, 2, 6, true, true, OsDescriptionGate.ContinueFormatting),
    (true, 2, 7, false, false, OsDescriptionGate.ContinueFormatting),
    (true, 2, 10, false, false, OsDescriptionGate.ContinueFormatting),
    (true, 2, uint.MaxValue, false, false, OsDescriptionGate.ContinueFormatting)
})
    Check($"OS builder gate for version={gate.Version}, platform={gate.Platform}, major={gate.Major}, module={gate.Module}, export={gate.Export}",
        OsDescriptionNativeLayout.CaptureGate(gate.Version, gate.Platform, gate.Major, gate.Module, gate.Export) == gate.Expected);
Check("OS builder chooses native system info only when export is available",
    OsDescriptionNativeLayout.CaptureSystemInfoSource(true) == SystemInfoSource.GetNativeSystemInfo
    && OsDescriptionNativeLayout.CaptureSystemInfoSource(false) == SystemInfoSource.GetSystemInfo);
Check("Awesomium duplicate normalized key overwrites earlier value",
    DuplicateSwitchNativeLayout.CaptureSameNormalizedKeyValue("renderer", "plugin") == "plugin");
Check("Awesomium duplicate normalized key can clear earlier value",
    DuplicateSwitchNativeLayout.CaptureSameNormalizedKeyValue("renderer", "") == "");
foreach (var admission in new (bool Scanning, bool Marker, bool Prefix, bool Next, bool Admitted)[]
{
    (false, false, false, false, false), (false, false, true, false, false),
    (false, true, false, false, false), (false, true, true, false, false),
    (true, false, false, true, false), (true, false, true, true, true),
    (true, true, false, false, false), (true, true, true, false, false)
})
    Check($"Awesomium admission scanning={admission.Scanning}, marker={admission.Marker}, prefix={admission.Prefix}",
        SwitchAdmissionNativeLayout.CaptureNextSwitchScanningState(admission.Scanning, admission.Marker) == admission.Next
        && SwitchAdmissionNativeLayout.CaptureCurrentSwitchMapAdmission(
            admission.Scanning, admission.Marker, admission.Prefix) == admission.Admitted);
Check("Awesomium plain positional preserves scanning for a later type switch",
    !SwitchAdmissionNativeLayout.CaptureCurrentSwitchMapAdmission(true, false, false)
    && SwitchAdmissionNativeLayout.CaptureCurrentSwitchMapAdmission(
        SwitchAdmissionNativeLayout.CaptureNextSwitchScanningState(true, false), false, true));
Check("Awesomium exact marker blocks a later type switch",
    !SwitchAdmissionNativeLayout.CaptureCurrentSwitchMapAdmission(
        SwitchAdmissionNativeLayout.CaptureNextSwitchScanningState(true, true), false, true));
foreach (var name in new (uint Minor, byte Type, string Expected)[]
{
    (0, 1, "Windows Vista "), (0, 3, "Windows Server 2008 "),
    (1, 1, "Windows 7 "), (1, 0, "Windows Server 2008 R2 "),
    (2, 1, "Windows 8 "), (2, 3, "Windows Server 8 "),
    (3, 1, "Windows ? "), (uint.MaxValue, 3, "Windows ? ")
})
    Check($"NT6 exact name minor={name.Minor}, product type={name.Type}",
        OsDescriptionTextNativeLayout.CaptureNt6Name(name.Minor, name.Type) == name.Expected);
foreach (var edition in new (uint Code, string Expected)[]
{
    (1, "Ultimate Edition"), (2, "Home Basic Edition"), (3, "Home Premium Edition"),
    (4, "Enterprise Edition"), (6, "Business Edition"), (7, "Standard Edition"),
    (8, "Datacenter Edition"), (9, "Small Business Server"), (10, "Enterprise Edition"),
    (11, "Starter Edition"), (12, "Datacenter Edition (core installation)"),
    (13, "Standard Edition (core installation)"), (14, "Enterprise Edition (core installation)"),
    (15, "Enterprise Edition for Itanium - based Systems"), (17, "Web Server Edition"),
    (18, "Cluster Server Edition"), (25, "Small Business Server Premium Edition"), (48, "Professional"),
    (0, ""), (5, ""), (16, ""), (49, ""), (uint.MaxValue, "")
})
    Check($"NT6 edition exact product code {edition.Code}",
        OsDescriptionTextNativeLayout.CaptureNt6Edition(edition.Code) == edition.Expected);
foreach (var name in new (uint Minor, byte Type, ushort Architecture, ushort Suites, bool R2, string Expected)[]
{
    (0, 1, 0, 0, false, "Windows 2000 Professional"),
    (0, 3, 0, 0x82, false, "Windows 2000 Datacenter Server"),
    (0, 3, 0, 2, false, "Windows 2000 Advanced Server"),
    (0, 3, 0, 0, false, "Windows 2000 Server"),
    (1, 1, 0, 0x200, false, "Windows XP Home Edition"),
    (1, 3, 9, 0, true, "Windows XP Professional"),
    (2, 1, 9, 0, false, "Windows XP Professional x64 Edition"),
    (2, 1, 0, 0, false, "Windows Server 2003, "),
    (2, 3, 0, 0, true, "Windows Server 2003 R2, Standard Edition"),
    (2, 3, 6, 0x82, false, "Windows Server 2003, Datacenter Edition for Itanium-based Systems"),
    (2, 3, 6, 2, false, "Windows Server 2003, Enterprise Edition for Itanium-based Systems"),
    (2, 3, 6, 0, false, "Windows Server 2003, "),
    (2, 3, 9, 0x82, false, "Windows Server 2003, Datacenter x64 Edition"),
    (2, 3, 9, 2, false, "Windows Server 2003, Enterprise x64 Edition"),
    (2, 3, 9, 0, false, "Windows Server 2003, Standard x64 Edition"),
    (2, 3, 9, 0x4000, false, "Windows Server 2003, Standard x64 Edition"),
    (2, 3, 0, 0x4082, false, "Windows Server 2003, Compute Cluster Edition"),
    (2, 3, 0, 0x82, false, "Windows Server 2003, Datacenter Edition"),
    (2, 3, 0, 2, false, "Windows Server 2003, Enterprise Edition"),
    (2, 3, 0, 0x400, false, "Windows Server 2003, Web Edition"),
    (2, 3, 0, 0, false, "Windows Server 2003, Standard Edition"),
    (2, 3, 0, 0xA000, false, "Windows Storage Server 2003Standard Edition"),
    (2, 3, 0, 0x8000, false, "Windows Home ServerStandard Edition"),
    (2, 1, 9, 0x2000, false, "Windows Storage Server 2003"),
    (2, 1, 9, 0xA000, true, "Windows Server 2003 R2, "),
    (3, 1, 0, 0, false, "")
})
    Check($"NT5 exact name minor={name.Minor}, product={name.Type}, arch={name.Architecture}, suites={name.Suites:X4}, R2={name.R2}",
        OsDescriptionTextNativeLayout.CaptureNt5Name(name.Minor, name.Type, name.Architecture, name.Suites, name.R2) == name.Expected);
foreach (var suffix in new (uint Major, ushort Architecture, string Expected)[]
{
    (5, 9, ""), (6, 9, ", 64-bit"), (6, 0, ", 32-bit"),
    (6, 6, ""), (10, 9, ", 64-bit"), (uint.MaxValue, ushort.MaxValue, "")
})
    Check($"OS architecture suffix major={suffix.Major}, architecture={suffix.Architecture}",
        OsDescriptionTextNativeLayout.CaptureArchitectureSuffix(suffix.Major, suffix.Architecture) == suffix.Expected);
foreach (var entry in new (ulong Bytes, DriveSpaceAmounts Amounts, DriveSpaceDisplayUnit Unit)[]
{
    (0, new(0, 0), DriveSpaceDisplayUnit.None),
    (1UL << 20, new(1, 1f / 1024), DriveSpaceDisplayUnit.None),
    (2UL << 20, new(2, 1f / 512), DriveSpaceDisplayUnit.Megabytes),
    (1UL << 30, new(1024, 1), DriveSpaceDisplayUnit.Megabytes),
    (2UL << 30, new(2048, 2), DriveSpaceDisplayUnit.Gigabytes),
    (1UL << 40, new(1048576, 1025), DriveSpaceDisplayUnit.Gigabytes),
    (1UL << 51, new(-2147483648, -2095104), DriveSpaceDisplayUnit.None),
    (1UL << 52, new(0, 4096), DriveSpaceDisplayUnit.Gigabytes)
})
    Check($"DriveSpace available-byte arithmetic {entry.Bytes}",
        DriveSpaceNativeLayout.CaptureAvailableSpace(true, entry.Bytes) == entry.Amounts
        && DriveSpaceNativeLayout.CaptureDisplayUnit(entry.Amounts) == entry.Unit
        && DriveSpaceNativeLayout.CaptureAvailableSpace(false, entry.Bytes) == new DriveSpaceAmounts(0, 0));
Check("DriveSpace totals accumulate even individually suppressed one-megabyte drives",
    DriveSpaceNativeLayout.CaptureAccumulation(
        DriveSpaceNativeLayout.CaptureAvailableSpace(true, 1UL << 20),
        DriveSpaceNativeLayout.CaptureAvailableSpace(true, 1UL << 20))
            == new DriveSpaceAmounts(2, 1f / 512));
foreach (byte letter in new byte[] { 0, (byte)'A', (byte)'B', (byte)'C', (byte)'Z', (byte)'c', (byte)'z', (byte)'[' })
    Check($"DriveSpace helper letter {letter}",
        DriveSpaceNativeLayout.CaptureHelperDriveLetter(letter)
            == (letter is (byte)'C' or (byte)'Z' or (byte)'c' or (byte)'z' ? (char)letter : 'C'));
foreach (uint driveType in new uint[] { 0, 1, 2, 3, 4, 5, 6, uint.MaxValue })
    Check($"DriveSpace fixed-drive type gate {driveType}",
        DriveSpaceNativeLayout.CaptureDriveAdmission(driveType) == (driveType == 3));
foreach (uint suffixLength in new uint[] { 0, 1, 8, 1024, uint.MaxValue })
{
    Check($"Recognized bare browser switch does not infer a value from another argument ({suffixLength})",
        CommandSwitchValueNativeLayout.CaptureValueLength(false, suffixLength)
            == new SwitchValueLengthDecision(0, false));
    Check($"Recognized browser switch with equals preserves suffix length {suffixLength}",
        CommandSwitchValueNativeLayout.CaptureValueLength(true, suffixLength)
            == new SwitchValueLengthDecision(suffixLength, false));
}
foreach (int capacity in new[] { 512, 1024, 2048, 4096, 8192, 16384, 32768 })
{
    Check($"INI capacity {capacity}, zero count returns null",
        IniFileNativeLayout.CaptureReadAttempt(capacity, 0)
            == new IniReadDecision(IniReadAction.ReturnNull, null, 0));
    Check($"INI capacity {capacity}, nSize-2 accepted without retry even for multi-string queries",
        IniFileNativeLayout.CaptureReadAttempt(capacity, (uint)capacity - 2)
            == new IniReadDecision(IniReadAction.CopyReturnedCharacters, null, (uint)capacity - 2));
    Check($"INI capacity {capacity}, nSize-1 grows or throws at cap",
        IniFileNativeLayout.CaptureReadAttempt(capacity, (uint)capacity - 1)
            == (capacity < 32768
                ? new IniReadDecision(IniReadAction.RetryWithLargerBuffer, capacity * 2, 0)
                : new IniReadDecision(IniReadAction.ThrowValueTooLarge, null, 0)));
}
foreach (bool fallback in new[] { false, true })
foreach (string value in new[] { "true", "TRUE", "TrUe", "1", "false", "0", "yes", "", " true ", "1 " })
    Check($"INI Boolean value '{value}', supplied default={fallback}",
        IniFileNativeLayout.CaptureBooleanValue(value, fallback)
            == (value is "true" or "TRUE" or "TrUe" or "1" || fallback));
bool nullIniBooleanThrows = false;
try { IniFileNativeLayout.CaptureBooleanValue(null, true); }
catch (NullReferenceException) { nullIniBooleanThrows = true; }
Check("INI missing/empty GetString result throws in Boolean wrapper rather than using default",
    nullIniBooleanThrows);
Check("INI key search is ordinal-ignore-case and empty enumeration skips null-key dereference",
    IniFileNativeLayout.CaptureKeyExists("WRITEPATH", new[] { "WritePath" })
    && !IniFileNativeLayout.CaptureKeyExists("WritePath ", new[] { "WritePath" })
    && !IniFileNativeLayout.CaptureKeyExists(null, Array.Empty<string>()));
bool nullIniKeyThrows = false;
try { IniFileNativeLayout.CaptureKeyExists(null, new[] { "WritePath" }); }
catch (NullReferenceException) { nullIniKeyThrows = true; }
Check("INI null requested key throws when a candidate exists", nullIniKeyThrows);
foreach (uint nativeReturn in new uint[] { 0, 1, 2, uint.MaxValue })
    Check($"INI SetString native return {nativeReturn}",
        IniFileNativeLayout.CaptureSetStringResult(nativeReturn) == (nativeReturn != 0)
        && IniFileNativeLayout.DiscardsSectionDeleteResult);
foreach (string? directory in new string?[] { @"X:\synthetic", @"X:\synthetic folder", "", null })
{
    var launch = PermissionLaunchNativeLayout.CaptureDescriptor(directory);
    Check($"Permission launch descriptor only, synthetic directory length={directory?.Length ?? -1}",
        launch == new PermissionLaunchDescriptor(37, "icacls.exe",
            $"\"{directory}\" /RESET /T /Q /C", true, false, false, false, false)
        && PermissionLaunchNativeLayout.DiscardsProcessStartBoolean);
}
foreach (bool hasDelegate in new[] { false, true })
{
    foreach (string? type in new string?[] { null, "" })
        Check($"Browser export excludes absent/empty type (delegate={hasDelegate})",
            ChildProcessRoleNativeLayout.CaptureExportEntry(type, hasDelegate)
                == new ChildProcessRoute(ChildProcessRouteKind.ImmediateReturn, null, 0));
    foreach (var role in ChildProcessRoleNativeLayout.Roles)
    {
        var known = new ChildProcessRoute(ChildProcessRouteKind.KnownCallback, role.CallbackVa, null);
        Check($"Content role {role.Key} exact match (delegate={hasDelegate})",
            ChildProcessRoleNativeLayout.CaptureContentDispatch(role.Key, hasDelegate) == known);
        if (role.Key.Length != 0)
            Check($"Export dispatches nonempty role {role.Key} (delegate={hasDelegate})",
                ChildProcessRoleNativeLayout.CaptureExportEntry(role.Key, hasDelegate) == known);
    }
    foreach (string unknown in new[] { "Renderer", "renderer ", "renderer-extra", "unknown", " browser" })
        Check($"Unknown role {unknown} uses delegate only when available",
            ChildProcessRoleNativeLayout.CaptureExportEntry(unknown, hasDelegate)
                == (hasDelegate ? new ChildProcessRoute(ChildProcessRouteKind.DelegateFallback, null, null)
                    : new ChildProcessRoute(ChildProcessRouteKind.ImmediateReturn, null, 1)));
}
foreach (var entry in new (bool Present, object? App, object? Registry, string? Expected)[]
{
    (false, @"C:\ignored", @"C:\registry", @"C:\registry"),
    (true, null, @"C:\registry", @"C:\registry"),
    (true, "", @"C:\registry", @"C:\registry"),
    (true, @"C:\override", @"C:\registry", @"C:\override"),
    (true, @"C:\override --culture=en-US", @"C:\registry", @"C:\override=en-US"),
    (true, " --culture --culture", @"C:\registry", ""),
    (true, @"C:\ --Culture", null, @"C:\ --Culture"),
    (false, null, @"C:\ --culture --culture", @"C:\"),
    (false, null, "", ""), (false, null, null, null),
    (false, null, 123, null), (false, 123, @"C:\registry", @"C:\registry")
})
    Check($"Install root selection synthetic present={entry.Present}, result={entry.Expected ?? "<null>"}",
        InstallLocationNativeLayout.CaptureInstallLocation(entry.Present, entry.App, entry.Registry) == entry.Expected);
bool rejectedNonstringOverride = false;
try
{
    InstallLocationNativeLayout.CaptureInstallLocation(true, 123, @"C:\registry");
}
catch (InvalidCastException)
{
    rejectedNonstringOverride = true;
}
Check("WPF installDir nonstring throws rather than silently selecting registry",
    rejectedNonstringOverride);
Check("Native product registry identity differs from shortcut display name",
    InstallLocationNativeLayout.ProductIdentifier == "Firefall_Beta"
    && InstallLocationNativeLayout.ProductIdentifierDisplay == "Firefall"
    && BuildInformation.InstallRegistryKeyPath(InstallLocationNativeLayout.ProductIdentifier)
        == @"Software\Red 5 Studios\Firefall_Beta\");
foreach (string? display in new string?[] { "Firefall", "Firefall Beta", "", null })
{
    var shortcuts = ShortcutNativeLayout.CaptureCreation(display);
    Check($"Shortcut property plan for synthetic display {display ?? "<null>"}",
        shortcuts.Length == 3
        && shortcuts[0] == new ShortcutDefinition(ShortcutFolder.SelectedDesktop,
            $"Play {display}.lnk", $"Play {display}", @"system\bin\Launcher.exe",
            @"system\bin", @"system\bin\FirefallClient.exe", null)
        && shortcuts[1] == (shortcuts[0] with { Folder = ShortcutFolder.SelectedStartMenuProductDirectory })
        && shortcuts[2] == new ShortcutDefinition(ShortcutFolder.SelectedStartMenuProductDirectory,
            $"Uninstall {display}.lnk", $"Uninstall {display}", @"uninstall\FirefallInstaller.exe",
            "uninstall", null, "/uninstall"));
}
foreach (bool exists in new[] { false, true })
foreach (uint entries in new uint[] { 0, 1, 2, uint.MaxValue })
    Check($"Shortcut removal exists={exists}, remaining entries={entries}",
        ShortcutNativeLayout.CaptureFileRemoval(exists) == exists
        && ShortcutNativeLayout.CaptureDirectoryRemoval(exists, entries) == (exists && entries == 0)
        && !ShortcutNativeLayout.RecursiveDirectoryRemoval);
foreach (bool nativeBool in new[] { false, true })
{
    Check($"Common folder ignores native BOOL {nativeBool} on normal return",
        NativeUtilityNativeLayout.CaptureCommonPathSource(nativeBool, false)
            == CommonFolderPathSource.ShellStringBuilder);
    Check($"Common folder selects managed fallback only on exception (native BOOL {nativeBool})",
        NativeUtilityNativeLayout.CaptureCommonPathSource(nativeBool, true)
            == CommonFolderPathSource.ManagedSpecialFolderFallback);
}
Check("Shell path helper requests capacity260 without creating folders",
    NativeUtilityNativeLayout.ShellPathBufferCapacity == 260
    && !NativeUtilityNativeLayout.CreateShellFolder
    && NativeUtilityNativeLayout.CommonDesktopDirectoryId == 25
    && NativeUtilityNativeLayout.DesktopDirectoryFallbackId == 16
    && NativeUtilityNativeLayout.CommonStartMenuId == 22
    && NativeUtilityNativeLayout.StartMenuFallbackId == 11);
foreach (var entry in new[]
{
    (Forge: false, Empty: false, Action: AuthInitializationAction.KeepProvidedAuth),
    (Forge: true, Empty: false, Action: AuthInitializationAction.KeepProvidedAuth),
    (Forge: false, Empty: true, Action: AuthInitializationAction.LeaveEmpty),
    (Forge: true, Empty: true, Action: AuthInitializationAction.TryStoredCredentials)
})
    Check($"Auth initialization forge={entry.Forge}, empty={entry.Empty}",
        AuthHandoffNativeLayout.CaptureInitialization(entry.Forge, entry.Empty) == entry.Action);
foreach (var entry in new[]
{
    (AuthEmpty: false, LoginEmpty: false, Action: AuthSubmitFallbackAction.KeepProvidedAuth),
    (AuthEmpty: false, LoginEmpty: true, Action: AuthSubmitFallbackAction.KeepProvidedAuth),
    (AuthEmpty: true, LoginEmpty: false, Action: AuthSubmitFallbackAction.UseMarkedRegistryLoginName),
    (AuthEmpty: true, LoginEmpty: true, Action: AuthSubmitFallbackAction.LeaveEmptyAfterRegistryLookup)
})
    Check($"Submit auth fallback empty={entry.AuthEmpty}, registry empty={entry.LoginEmpty}",
        AuthHandoffNativeLayout.CaptureSubmitFallback(entry.AuthEmpty, entry.LoginEmpty) == entry.Action);
foreach (var entry in new (string? Input, string Expected)[]
{
    (null, ""), ("", ""), ("!opaque", "!opaque"),
    ("!a&b<c>\"'", "!a&amp;b&lt;c&gt;\"'"),
    ("&amp;&lt;&gt;", "&amp;amp;&amp;lt;&amp;gt;"),
    ("&&<<>>", "&amp;&amp;&lt;&lt;&gt;&gt;"),
    ("!value\0ignored&", "!value"), ("\0<&>", ""), ("line\r\n", "line\r\n")
})
    Check($"Auth XML synthetic input {entry.Input?.Length ?? -1} bytes, expected {entry.Expected.Length}",
        AuthHandoffNativeLayout.CaptureXmlText(entry.Input) == entry.Expected);
foreach (int pointerBytes in new[] { 4, 8 })
foreach (bool exportAvailable in new[] { false, true })
foreach (bool apiSucceeded in new[] { false, true })
foreach (bool reportedWow64 in new[] { false, true })
{
    bool expected = pointerBytes == 8
        || (exportAvailable && apiSucceeded && reportedWow64);
    Check($"Managed OS bitness pointer={pointerBytes}, export={exportAvailable}, success={apiSucceeded}, wow64={reportedWow64}",
        WowNativeLayout.CaptureIs64BitProcess(pointerBytes) == (pointerBytes == 8)
        && WowNativeLayout.CaptureIs64BitOperatingSystem(
            pointerBytes, exportAvailable, apiSucceeded, reportedWow64) == expected);
}
Check("Producer records all eleven actual stream IDs, not decompiler-truncated low bytes",
    DumperHandoffNativeLayout.Fields.Select(field => field.StreamType).SequenceEqual(
        new uint[] { 0x10000, 0x10002, 0x10003, 0x10004, 0x10005, 0x10006,
            0x10007, 0x10008, 0x10009, 0x1000A, 0x1000B }));
Check("Metadata record and empty-line terminator use CRLF, not EOF",
    DumperHandoffNativeLayout.LineTerminator == UserStreamNativeLayout.LineTerminator
    && DumperHandoffNativeLayout.EndOfMetadata == "\r\n");
Check("Minidump descriptors are twelve native bytes with fixed-width integer buffers",
    UserStreamNativeLayout.DescriptorNativeByteCount == 12
    && UserStreamNativeLayout.DescriptorTypeOffset == 0
    && UserStreamNativeLayout.DescriptorBufferSizeOffset == 4
    && UserStreamNativeLayout.DescriptorBufferPointerOffset == 8
    && UserStreamNativeLayout.Int32BufferByteCount == 4
    && UserStreamNativeLayout.Int64BufferByteCount == 8);
Check("Metadata owner names and thread stream preserve their independently traced offsets",
    DumperHandoffNativeLayout.OwnerApplicationNameOffset == 0x20
    && DumperHandoffNativeLayout.OwnerSourceRevisionOffset == 0x24
    && DumperHandoffNativeLayout.OwnerBuildNumberOffset == 0x28
    && DumperHandoffNativeLayout.OwnerEnvironmentMnemonicOffset == 0x2C
    && DumperHandoffNativeLayout.OwnerEnvironmentNameOffset == 0x30
    && DumperHandoffNativeLayout.ThreadIdStreamType == 0x1000A);
foreach (var numbers in new (int Source, int Build, uint ExpectedSource, uint ExpectedBuild)[]
{
    (0, 0, 117123, 1962),
    (1, 0, 1, 1962),
    (0, 1, 117123, 1),
    (117123, 1962, 117123, 1962),
    (-1, -1, uint.MaxValue, uint.MaxValue),
    (int.MinValue, int.MaxValue, 0x80000000, 0x7FFFFFFF)
})
{
    Check($"Post-atoi build defaults source={numbers.Source}, build={numbers.Build}",
        DumperHandoffNativeLayout.CaptureBuildNumbers(numbers.Source, numbers.Build)
            == new DumperBuildNumbers(numbers.ExpectedSource, numbers.ExpectedBuild));
}
foreach (bool success in new[] { false, true })
foreach (var counts in new (uint Requested, uint Written)[]
{
    (0, 0), (1, 0), (1, 1), (100, 99), (100, 100), (100, 101),
    (uint.MaxValue, uint.MaxValue - 1), (uint.MaxValue, uint.MaxValue)
})
{
    var decision = DumperHandoffNativeLayout.CapturePipeWrite(success, counts.Requested, counts.Written);
    bool complete = success && counts.Requested == counts.Written;
    Check($"Parent write success={success}, requested={counts.Requested}, written={counts.Written}",
        decision.WaitForDumper == complete
        && decision.TerminateExitCode == (complete ? null : (uint?)0xBADC0DE));
}

foreach (bool hasRecord in new[] { false, true })
foreach (uint code in new uint[] { 0, 0xC0000005,
    CrashMetadataNativeLayout.EmbeddedSourceExceptionCode,
    CrashMetadataNativeLayout.FullMemorySourceExceptionCode })
foreach (uint count in new uint[] { 0, 1, 2, uint.MaxValue })
foreach (uint length in new uint[] { 0, 1, 1024, 1025, uint.MaxValue })
foreach (bool readSucceeded in new[] { false, true })
{
    bool custom = code == 0x012ED500 || code == 0x012ED501;
    bool read = hasRecord && custom && count >= 2 && length <= 1024;
    uint flags = hasRecord && code == 0x012ED501 ? 2u : 0x40u;
    DebugSourceKind kind = !hasRecord || (custom && count < 2)
        ? DebugSourceKind.Unspecified
        : !custom ? DebugSourceKind.Exception
        : length > 1024 ? DebugSourceKind.TooLong
        : readSucceeded ? DebugSourceKind.RemoteBytes : DebugSourceKind.BadMemory;
    Check($"Source gate record={hasRecord}, code={code:X}, count={count}, length={length}, read={readSucceeded}",
        CrashMetadataNativeLayout.CaptureDebugSource(hasRecord, code, count, length, readSucceeded)
            == new DebugSourceDecision(kind, flags, read, read ? length : 0));
}
foreach (var measurement in new (ulong Preferred, uint PageFile, ulong Expected, bool Fallback)[]
{
    (0, 0, 0, true),
    (0, 123, 123, true),
    (0, uint.MaxValue, uint.MaxValue, true),
    (1, uint.MaxValue, 1, false),
    (0x100000000, 123, 0x100000000, false),
    (ulong.MaxValue, 0, ulong.MaxValue, false)
})
{
    Check($"Process-memory source preferred={measurement.Preferred}, pagefile={measurement.PageFile}",
        CrashMetadataNativeLayout.CaptureProcessMemory(measurement.Preferred, measurement.PageFile)
            == new ProcessMemoryMetadataDecision(measurement.Expected, measurement.Fallback));
}
foreach (bool success in new[] { false, true })
foreach (var limits in new (ulong PageFile, ulong Virtual, ulong Expected)[]
{
    (0, 0, 0), (1, 2, 1), (2, 1, 1), (0x100000000, 0xFFFFFFFF, 0xFFFFFFFF),
    (0xFFFFFFFF, 0x100000000, 0xFFFFFFFF), (ulong.MaxValue, ulong.MaxValue, ulong.MaxValue)
})
{
    Check($"Memory ceiling status={success}, pagefile={limits.PageFile}, virtual={limits.Virtual}",
        CrashMetadataNativeLayout.CaptureMemoryCeiling(success, limits.PageFile, limits.Virtual)
            == (success ? limits.Expected : 0));
}

foreach (uint exitCode in new uint[] { 0, 1, 259, uint.MaxValue })
foreach (bool compressed in new[] { false, true })
{
    Check($"Hardware packaging exit={exitCode}, compression={compressed}",
        HardwareReportNativeLayout.CaptureHardwareReport(exitCode, compressed)
            == new HardwareReportDecision(exitCode == 0, exitCode != 0,
                exitCode == 0 && !compressed, exitCode == 0 && !compressed));
}
foreach (int result in new[] { int.MinValue, -5, -1, 0, 1, 2, int.MaxValue })
foreach (uint remaining in new uint[] { 0, 1, 3, 4, 5, uint.MaxValue })
{
    bool accepted = result == 0 || result == 1;
    var outcome = !accepted ? DeflateDrainOutcome.Failure
        : remaining == 0 ? DeflateDrainOutcome.Repeat : DeflateDrainOutcome.Success;
    Check($"Hardware deflate result={result}, avail_out={remaining}",
        HardwareReportNativeLayout.CaptureDeflateDrain(result, remaining)
            == new DeflateDrainDecision(outcome, accepted ? unchecked(4u - remaining) : 0));
}

Check("Report layer order distinguishes uncompressed app text from the message option",
    CrashReportNativeLayout.LayersInProducerOrder.Select(layer => layer.Type)
        .SequenceEqual(new uint[] { 1, 4, 2, 5, 3 })
    && CrashReportNativeLayout.LayersInProducerOrder.Single(layer => !layer.Compressed).Type == 4
    && CrashReportNativeLayout.OwnerApplicationNameOffset == 0x0C
    && CrashReportNativeLayout.OwnerMessageOffset == 0x10);
Check("Client exception arguments preserve independent length/pointer pairs without NUL lengths",
    DebugExceptionHandoffNativeLayout.CaptureArguments(3, 0x12345678, 4, 0x87654321)
        == new DebugExceptionArguments(3, 0x12345678, 4, 0x87654321)
    && DebugExceptionHandoffNativeLayout.CaptureRaiseFlags(true) == 0
    && DebugExceptionHandoffNativeLayout.CaptureRaiseFlags(false) == 1);
foreach (uint state in new uint[] { 0, 1, 2, 3, uint.MaxValue })
foreach (uint previous in new uint[] { 0, uint.MaxValue - 1000 })
foreach (uint elapsed in new uint[] { 0, 1999, 2000, 2001, uint.MaxValue })
{
    uint now = unchecked(previous + elapsed);
    bool eligible = state == 0 || elapsed > 2000;
    Check($"Fatal dump admission state={state}, previous={previous}, elapsed={elapsed}",
        DebugExceptionHandoffNativeLayout.CaptureDumpEligibility(state, previous, now) == eligible
        && DebugExceptionHandoffNativeLayout.CaptureFatalTransition(state, previous, now, unchecked(now + 1))
            == new DebugSnapshotTransition(eligible, 3, unchecked(now + 1), true));
}
foreach (uint state in new uint[] { 0, 1, 2, 3, 4, uint.MaxValue })
{
    bool admitted = state == 0 || state == 2;
    Check($"Continuable snapshot admission state={state}",
        DebugExceptionHandoffNativeLayout.CaptureSnapshotTransition(state, 10, 20)
            == new DebugSnapshotTransition(admitted, admitted ? 1u : state,
                admitted ? 20u : 10u, !admitted && state == 3));
    Check($"RIP_EVENT state {state} preserves state/tick and always terminates",
        DebugExceptionHandoffNativeLayout.CaptureRipTransition(state, uint.MaxValue)
            == new DebugSnapshotTransition(state == 0 || state == 2, state, uint.MaxValue, true));
}
foreach (ushort unicode in new ushort[] { 0, 1, 2, ushort.MaxValue })
foreach (ushort length in new ushort[] { 0, 1, 5, 6, 13, 14, ushort.MaxValue })
{
    bool read = unicode == 0 && length > 5;
    Check($"Debug string read unicode={unicode}, reported length={length}",
        DebugStringHandoffNativeLayout.CaptureRead(unicode, length)
            == new DebugStringReadDecision(read, read ? length : 0u, read ? (uint)length + 1 : 0));
}
foreach (uint code in new uint[] { 0, 0x012ED4FF, 0x012ED500, 0x012ED501,
    0x012ED502, 0x012ED503, uint.MaxValue })
foreach (uint count in new uint[] { 0, 2, 3, 4, uint.MaxValue })
foreach (uint length in new uint[] { 0, 1024, 1025, uint.MaxValue })
foreach (bool success in new[] { false, true })
{
    bool recognized = code is 0x012ED500 or 0x012ED501 or 0x012ED502;
    bool read = recognized && count >= 4 && length <= 1024;
    var kind = !recognized ? PrivateExceptionMessageKind.NotPrivateCode
        : count < 4 ? PrivateExceptionMessageKind.NoMessage
        : length > 1024 ? PrivateExceptionMessageKind.TooLong
        : success ? PrivateExceptionMessageKind.RemoteBytes : PrivateExceptionMessageKind.BadMemory;
    Check($"Private exception message code={code:X}, parameters={count}, length={length}, read={success}",
        DebugExceptionHandoffNativeLayout.CaptureMessage(code, count, length, success)
            == new PrivateExceptionMessageDecision(kind, read, read ? length : 0));
}
foreach (int length in new[] { int.MinValue, -1, 0, 0x3FFFF, 0x40000, 0x40001, int.MaxValue })
{
    Check($"Log clipping uses signed ftell threshold, observed={length}",
        CrashReportNativeLayout.SelectsClippedLogPath(length) == (length > 262144));
}
foreach (int result in new[] { int.MinValue, -6, -5, -2, -1, 0, 1, 2, int.MaxValue })
foreach (uint remaining in new uint[] { 0, 1, 4, 5, uint.MaxValue })
foreach (bool eof in new[] { false, true })
{
    var outcome = result == -2 ? FileDeflateOutcome.Failure
        : remaining == 0 ? FileDeflateOutcome.RepeatDrain
        : eof ? FileDeflateOutcome.Success : FileDeflateOutcome.ReadInput;
    Check($"File deflate result={result}, avail_out={remaining}, EOF={eof}",
        CrashReportNativeLayout.CaptureFileDrain(result, remaining, eof)
            == new FileDeflateDecision(outcome, result == -2 ? 0 : unchecked(4u - remaining))
        && CrashReportNativeLayout.SelectFileFlush(eof) == (eof ? 4 : 0));
}

if (args.Length == 2)
{
    string root = args[1];
    SampleImage Load(string name, long size, string hash) => new(Path.Combine(root, name), size, hash);
    var red = Load("RedHanded.exe", RedHandedLayout.FileSize, RedHandedLayout.Sha256);
    var dumper = Load("r5dumper.exe", R5DumperLayout.FileSize, R5DumperLayout.Sha256);
    var truck = Load("DumpTruck.exe", DumpTruckLayout.FileSize, DumpTruckLayout.Sha256);
    Check("HWStats pipe endpoints are inheritable and child inherits handles",
        truck.Matches(0x42ABE2, "C785A8F7FFFF00010000")
        && truck.Matches(0x42AC0A, "6800080000")
        && truck.Matches(0x42AC16, "C785B8F7FFFFFFFFFFFF508D85E8F7FFFF")
        && truck.Matches(0x42AC27, "C785C4F7FFFF0C000000")
        && truck.Matches(0x42AC31, "50C785CCF7FFFF01000000")
        && truck.Matches(0x42AC50, "FF1518514700")
        && truck.ImportSlotNames(0x475118, "CreatePipe")
        && truck.Matches(0x42AC93, "6A01")
        && truck.Matches(0x42ACA0, "FF1510514700")
        && truck.ImportSlotNames(0x475110, "CreateProcessA"));
    Check("HWStats CreatePipe failure returns before common cleanup",
        truck.Matches(0x42AC50, "FF151851470085C0751483C8FF5F5E5B")
        && truck.Matches(0x42AC60, "8B4DF833CDE89C3104008BE55DC3"));
    Check("HWStats CreateProcess failure joins cleanup without appending NUL",
        truck.Matches(0x42ACA0, "FF151051470085C00F8425010000")
        && truck.Matches(0x42ADD3, "83CBFF8B85E8F7FFFF8B350852470083F8FF"));
    Check("HWStats ReadFile failure joins cleanup without appending NUL",
        truck.Matches(0x42AD00, "8B35B4504700")
        && truck.Matches(0x42AD45, "FFD685C00F8484000000")
        && truck.Matches(0x42ADD3, "83CBFF8B85E8F7FFFF8B350852470083F8FF"));
    Check("HWStats normal exit and exit-query failure append one NUL through sink slot +0x18",
        truck.Matches(0x42AD7E, "8B85E0F7FFFF3D030100007536")
        && truck.Matches(0x42ADC1, "8BD88BCF8B076A0168F16B4700FF5018")
        && truck.Matches(0x42ADAE, "8B078BCF6A0168F16B470083CBFFFF")
        && truck.Matches(0x42ADBC, "FF5018EB15")
        && truck.Ascii(0x476BF1) == "");
    Check("HWStats common cleanup conditionally closes both pipe endpoints and process handles",
        truck.Matches(0x42ADDC, "8B350852470083F8FF740D50FFD6")
        && truck.Matches(0x42ADF4, "8B85B8F7FFFF83F8FF740D50FFD6")
        && truck.Matches(0x42AE0C, "8B85D4F7FFFF83F8FF740D50FFD6")
        && truck.Matches(0x42AE24, "8B8DD0F7FFFF83F9FF740351FFD6")
        && truck.ImportSlotNames(0x475208, "CloseHandle"));
    Check("HWStats polls, reads requested chunks, and forwards actual ReadFile count",
        truck.Matches(0x42ACFA, "8B1DB8504700")
        && truck.ImportSlotNames(0x4750B8, "PeekNamedPipe")
        && truck.Matches(0x42AD00, "8B35B4504700")
        && truck.ImportSlotNames(0x4750B4, "ReadFile")
        && truck.Matches(0x42ACEC, "FF151C52470085C00F84B40000008B1D")
        && truck.ImportSlotNames(0x47521C, "GetExitCodeProcess")
        && truck.Matches(0x42AD28, "76546A008D85E4F7FFFF506800080000")
        && truck.Matches(0x42AD45, "FFD685C00F8484000000")
        && truck.Matches(0x42AD4F, "FFB5E4F7FFFF")
        && truck.Matches(0x42AD60, "FF5018")
        && truck.Matches(0x42AD84, "3D030100007536")
        && truck.Matches(0x42AD8D, "FF15C8514700")
        && truck.ImportSlotNames(0x4751C8, "Sleep")
        && truck.Matches(0x42ADA0, "FF151C52470085C00F8558FFFFFF"));
    var hardware = Load("HWStats.exe", HWStatsLayout.FileSize, HWStatsLayout.Sha256);
    var child = Load("FirefallProcess.exe", FirefallProcessLayout.FileSize, FirefallProcessLayout.Sha256);
    var browser = Load("awesomium.dll", 23470048, AwesomiumLayout.Sha256);
    Check("Awesomium diagnostic wrapper initializes wide result and ignores aggregate conversion flag",
        browser.Matches(0x10296216, "C7461407000000C746100000000033C0668906")
        && browser.Calls(0x10296236, DiagnosticEncodingNativeLayout.ReserveHelperVa)
        && browser.Calls(0x1029623E, DiagnosticEncodingNativeLayout.ConversionLoopVa)
        && browser.Matches(0x10296243, "83C4185F8BC65E5BC3")
        && browser.Calls(0x10288F3E, DiagnosticEncodingNativeLayout.WrapperVa));
    Check("Awesomium diagnostic loop uses signed length and appends replacement while recording failure",
        browser.Matches(0x10295F17, "B301")
        && browser.Matches(0x10295F21, "85F67E51")
        && browser.Calls(0x10295F3C, DiagnosticEncodingNativeLayout.ScalarReaderVa)
        && browser.Matches(0x10295F45, "84C0740F")
        && browser.Calls(0x10295F4E, DiagnosticEncodingNativeLayout.Utf16AppendVa)
        && browser.Matches(0x10295F58, "68FDFF0000")
        && browser.Calls(0x10295F5D, DiagnosticEncodingNativeLayout.Utf16AppendVa)
        && browser.Matches(0x10295F65, "32DB")
        && browser.Matches(0x10295F76, "5E8AC35B"));
    Check("Awesomium decoded-scalar validity excludes surrogate range and values above 10FFFF",
        browser.Matches(0x102B78B2, "3C80721F8D484080F93E7314")
        && browser.Matches(0x102B78BE, "6AFF")
        && browser.Calls(0x102B78C8, 0x102B7D10)
        && browser.Matches(0x102B78D2, "83C8FF")
        && browser.Matches(0x102B78DE, "3D00D8000072113D00E0000072073DFFFF10007603"));
    Check("Awesomium wide append uses one BMP code unit or native surrogate-pair arithmetic",
        browser.Matches(0x102B7C16, "81FBFFFF0000774D")
        && browser.Calls(0x102B7C5E, 0x10276E10)
        && browser.Matches(0x102B7C9C, "8BCBC1E90A81E94028000066890C78")
        && browser.Matches(0x102B7CB3, "81E3FF03000081CB00DC000066895C7E02")
        && browser.Matches(0x102B7CC6, "B802000000"));
    Check("Awesomium verbose caller gates zero-extent diagnostic and reloads threshold afterward",
        browser.Matches(0x1028A796, "833DF86A591101")
        && browser.Matches(0x1028A7AC, "83F8037F4E")
        && browser.Matches(0x1028A7B9, "85F67542")
        && browser.Ascii(0x1127B19C) == "N > 0U"
        && browser.Calls(0x1028A7F5, ConversionDiagnosticNativeLayout.DestructorVa)
        && browser.Matches(0x1028A7FA, "A1046B59118B0DFC6A591185C97422"));
    Check("Awesomium verbose caller decrements supplied extent and clamps only without configuration",
        browser.Matches(0x1028A814, "4E508954240C89742410")
        && browser.Calls(0x1028A81E, VerboseRuleNativeLayout.LookupVa)
        && browser.Matches(0x1028A82B,
            "F7D88944240483F8FFC7442408FFFFFFFF8D4424047F048D4424088B00"));
    Check("Awesomium representative verbose call uses source literal length including terminator",
        browser.Ascii(0x1124DB7C) == @"browser\download\download_manager_impl.cc"
        && browser.Matches(0x1006D78A, "6A2A33DB687CDB2411")
        && browser.Calls(0x1006D799, VerboseLevelNativeLayout.LevelGetterVa)
        && browser.Matches(0x1006D7AF, "83F8140F8C9F0000006AEC"));
    Check("Awesomium verbose construction supplies comma/equals delimiters and explicit trim table",
        browser.Matches(0x102B581E, "6A2C6A3D")
        && browser.Calls(0x102B582F, 0x102A0670)
        && browser.Calls(0x102A01DB, 0x1029B0D0)
        && browser.Matches(0x1029B0DE, "6810C12711")
        && browser.Matches(0x1127C110, "090A0B0C0D2000"));
    Check("Awesomium pair construction skips empty fragments but retains output after sticky failure",
        browser.Matches(0x102A0730, "3959100F84E6020000")
        && browser.Calls(0x102A0770, 0x102A04D0)
        && browser.Matches(0x102A0778, "84C07504885C2413")
        && browser.Calls(0x102A090F, 0x100C21A0));
    Check("Awesomium pair extraction finds first equals then skips consecutive equals for a single value",
        browser.Calls(0x102A054B, 0x1000A850)
        && browser.Matches(0x102A0550, "8BF083FEFF7466")
        && browser.Calls(0x102A055D, 0x100038B0)
        && browser.Calls(0x102A059F, 0x1009F3F0)
        && browser.Matches(0x102A05A4, "83F8FF7529")
        && browser.Calls(0x102A05FC, 0x1000E260));
    Check("Awesomium per-module numeric failure still reaches rule append with converter output",
        browser.Calls(0x102B58E6, LoggingSetupNativeLayout.IntegerConversionWrapperVa)
        && browser.Matches(0x102B58EE, "84C07515F644241008740E")
        && browser.Calls(0x102B5902, ConversionDiagnosticNativeLayout.DestructorVa)
        && browser.Calls(0x102B5911, VerboseRuleNativeLayout.AppendRuleVa));
    Check("Awesomium verbose source slice uses reverse separator/dot searches and exact inl suffix",
        browser.Ascii(0x1127B0F0) == "\\/"
        && browser.Matches(0x102B522F, "C7442414F0B02711")
        && browser.Calls(0x102B523F, 0x102993E0)
        && browser.Matches(0x102B5247, "83F8FF7406400106294604")
        && browser.Matches(0x102B5252, "6AFF6A2E56")
        && browser.Calls(0x102B5257, 0x10299370)
        && browser.Calls(0x102B5265, 0x102994D0)
        && browser.Ascii(0x1127EB2C) == "-inl"
        && browser.Matches(0x102B5281, "6A048D543AFC682CEB271152")
        && browser.Calls(0x102B528D, 0x1000A5D0)
        && browser.Matches(0x102B5295, "85C08BC6750E83C7FC897E04"));
    Check("Awesomium verbose rule chooses full source only for kind one and returns first matching level",
        browser.Matches(0x102B53D0, "837E20018B450874048D442420")
        && browser.Calls(0x102B540D, VerboseRuleNativeLayout.WildcardMatcherVa)
        && browser.Matches(0x102B5415, "84C0751783C6243BF775B0")
        && browser.Matches(0x102B5420, "8B4B108B01F7D8")
        && browser.Matches(0x102B5430, "8B461C"));
    Check("Awesomium verbose rules append level and path kind in thirty-six-byte records",
        browser.Calls(0x102B58A7, 0x1012F1D0)
        && browser.Matches(0x102B58AC, "83F8FF740BC784240401000001000000")
        && browser.Calls(0x102B5911, VerboseRuleNativeLayout.AppendRuleVa)
        && browser.Matches(0x102B56FB, "8B4E1C894B1C8B562089532083470424"));
    Check("Awesomium verbose matcher uses literal byte and equivalent separator comparisons",
        browser.Matches(0x102B52E6, "3C2A74433C2F740C3C3F74163C5C74043A06EB08")
        && browser.Matches(0x102B52FA, "8A063C2F74083C5C0F858A000000")
        && browser.Matches(0x102B5308, "4942464F")
        && browser.Matches(0x102B5340, "803A2A75144249")
        && browser.Calls(0x102B537A, VerboseRuleNativeLayout.WildcardMatcherVa)
        && browser.Matches(0x102B5382, "84C075D7464F89742410897C241475DE"));
    Check("Awesomium logger configuration locks according to persistent mode then closes only prior file handle",
        browser.Calls(0x1028A64E, LogSynchronizationNativeLayout.InitializerVa)
        && browser.Matches(0x1028A656, "393D286B5911")
        && browser.Matches(0x1028A65E, "8B152C6B59116AFF52FF15D0042411")
        && browser.ImportSlotNames(LogSynchronizationNativeLayout.MutexWaitImportVa, "WaitForSingleObject")
        && browser.Matches(0x1028A664, "6AFF52FF15D0042411")
        && browser.Matches(0x1028A66F, "8B0D246B5911")
        && browser.Calls(0x1028A675, LogSynchronizationNativeLayout.CriticalSectionEnterVa)
        && browser.Matches(0x1028A67A, "A10C6B59113BC7740D50")
        && browser.ImportSlotNames(LogFileNativeLayout.CloseHandleImportVa, "CloseHandle")
        && browser.Matches(LogFileNativeLayout.ConfiguredHandleCloseVa, "FF1590032411")
        && browser.Matches(0x1028A68A, "893D0C6B5911"));
    Check("Awesomium reconfiguration preserves file path for destinations zero/two and updates it otherwise",
        browser.Matches(0x1028A697, "A3184A5511")
        && browser.Matches(0x1028A69C, "3BC70F84A4000000")
        && browser.Matches(0x1028A6A4, "83F8020F849B000000")
        && browser.Matches(0x1028A6AD, "8B0D086B5911")
        && browser.Calls(0x1028A6F6, 0x100080A0)
        && browser.Matches(0x1028A6FB, "83BC2450010000007514")
        && browser.ImportSlotNames(0x112404D8, "DeleteFileW"));
    Check("Awesomium logger synchronization flag gates mode changes and existing mutex reuse",
        browser.Matches(0x10289AEE, "803D206B5911008B5424600F8555010000")
        && browser.Matches(0x10289AFF, "8B44245CA3286B591185C00F851701000039052C6B59110F8531010000"));
    Check("Awesomium synchronization name selects null-path default and replaces backslashes over copied length",
        browser.Matches(0x10289B2E, "85D27421")
        && browser.Calls(0x10289B58, LogFileNativeLayout.DefaultPathBuilderVa)
        && browser.Matches(0x10289BA0, "6683385C7508BA2F00000066891083C0023BC175EB")
        && browser.Matches(0x10289BB5, "6808B12711")
        && browser.Matches(0x1127B108, "47006C006F00620061006C005C000000"));
    Check("Awesomium mutex create stores returned handle and zero returns before initialized flag assignment",
        browser.ImportSlotNames(0x112404E0, "CreateMutexW")
        && browser.Matches(0x10289BE4, "506A006A00FF15E0042411A32C6B5911")
        && browser.Matches(0x10289BF8, "85C0751C")
        && browser.Matches(0x10289C14, "83C458C3")
        && browser.Matches(0x10289C25, "EB26")
        && browser.Matches(0x10289C4D, "C605206B591101"));
    Check("Awesomium critical-section allocation failure still reaches initialized flag assignment",
        browser.Matches(0x10289C27, "6A18")
        && browser.Matches(0x10289C31, "85C0740E")
        && browser.Calls(0x10289C37, 0x10288CF0)
        && browser.Matches(0x10289C41, "EB0AC705246B591100000000C605206B591101"));
    Check("Awesomium sink admission uses debugger destinations two/three and signed stderr severity threshold",
        browser.Matches(0x10289F3B, "83F802740C83F8037407833E027C4AEB15")
        && browser.ImportSlotNames(0x112404E4, "OutputDebugStringA")
        && browser.Matches(0x10289F5B, "FF15E4042411")
        && browser.Matches(0x10289FA5, "85C00F849300000083F8020F848A000000"));
    Check("Awesomium stderr uses third static CRT stream and ignores formatted-output/flush results",
        browser.Matches(0x1004E6A8, "B820345511C3")
        && browser.Word(LogSinkNativeLayout.StandardStreamsVa + 0x10) == 0
        && browser.Word(LogSinkNativeLayout.StandardStreamsVa + 0x30) == 1
        && browser.Word(LogSinkNativeLayout.StandardStreamsVa + 0x50) == 2
        && browser.Ascii(0x1127ADE0) == "%s"
        && browser.Calls(0x10289F75, 0x1004E6A8)
        && browser.Matches(0x10289F7A, "83C04050")
        && browser.Calls(0x10289F7E, 0x1004F011)
        && browser.Calls(0x10289F83, 0x1004E6A8)
        && browser.Matches(0x10289F88, "83C04050")
        && browser.Calls(0x10289F8C, 0x1004E9E7)
        && browser.Matches(0x10289F91, "83C4106A006A00"));
    Check("Awesomium file output checks helper byte but ignores seek and write outcomes",
        browser.Calls(0x10289F98, LogSinkNativeLayout.SynchronizationInitializerVa)
        && browser.Calls(0x10289FDA, LogFileNativeLayout.OpenHelperVa)
        && browser.Matches(0x10289FDF, "84C0743B")
        && browser.Matches(0x10289FE9, "6A026A006A0051FF158C032411")
        && browser.ImportSlotNames(LogSinkNativeLayout.WriteFileImportVa, "WriteFile")
        && browser.Matches(0x1028A004, "8B4C24246A008D542414528B150C6B5911515052FF15F8022411")
        && browser.Matches(0x1028A01E, "833D286B591100750E")
        && browser.ImportSlotNames(0x112404D4, "ReleaseMutex")
        && browser.Matches(0x1028A02D, "FF15D4042411EB0B"));
    Check("Awesomium default log path uses unchecked module filename and optional backslash truncation",
        browser.ImportSlotNames(0x11240300, "GetModuleFileNameW")
        && browser.Matches(0x10289A3C, "68040100008D44240C506A00FF150003241133C9")
        && browser.Matches(0x10289A70, "668B0883C0026685C975F5")
        && browser.Calls(0x10289C94, LogFileNativeLayout.DefaultPathBuilderVa)
        && browser.Matches(0x10289A99, "C74424105C000000")
        && browser.Calls(0x10289AA1, 0x1007DD70)
        && browser.Matches(0x10289AA6, "83F8FF740B6AFF40508BCE")
        && browser.Calls(0x10289AB1, 0x100076A0)
        && browser.Matches(0x10289AB6, "6A0968F4B02711")
        && browser.Calls(0x10289ABF, 0x10047380)
        && browser.Matches(0x1127B0F4, "640065006200750067002E006C006F0067000000"));
    Check("Awesomium log open entry accepts any nonzero stored handle and gates file opening on destination one or three",
        browser.Matches(LogFileNativeLayout.OpenHelperVa, "833D0C6B5911007403B001C3")
        && browser.Matches(0x10289CA5, "8B0D184A55115683F901740583F9037565")
        && browser.Matches(0x10289CB6, "8378140872028B00"));
    Check("Awesomium log configured and fallback opens preserve write/share/open-always flags",
        browser.ImportSlotNames(0x112403E4, "CreateFileW")
        && browser.Matches(0x10289CC4, "6A0068800000006A046A006A03680000004050FFD6")
        && browser.Matches(0x10289CD9, "A30C6B591183F8FF740485C07527")
        && browser.Matches(0x10289CE7, "6A0068800000006A046A006A0368000000406818B12711FFD6")
        && browser.Matches(0x10289D00, "A30C6B591183F8FF741585C07411"));
    Check("Awesomium log accepted handle attempts EOF seek without inspecting result and fallback failure clears storage",
        browser.ImportSlotNames(0x1124038C, "SetFilePointer")
        && browser.Matches(0x10289D0E, "6A026A006A0050FF158C032411B0015EC3")
        && browser.Matches(0x10289D1F, "C7050C6B59110000000032C05EC3")
        && browser.Matches(0x1127B118, "2E005C00640065006200750067002E006C006F0067000000"));
    Check("Awesomium logging setup initializes output and ignores numeric conversion status",
        browser.Matches(0x10033BD7, "33DB")
        && browser.Matches(0x10033D34, "895C2414")
        && browser.Calls(0x10033D6E, LoggingSetupNativeLayout.IntegerConversionWrapperVa)
        && browser.Matches(0x10033D73, "B80C57261183C408"));
    Check("Awesomium caller supplies destination, child append and disabled-check mode before threshold override",
        browser.Matches(0x10033E54, "33C9399C2450010000530F95C133D2385C24170F94C251535250")
        && browser.Calls(0x10033E6E, LoggingSetupNativeLayout.InitializeLoggerVa)
        && browser.Matches(0x10033E73, "8B44242850")
        && browser.Calls(0x10033E78, ConversionDiagnosticNativeLayout.MinimumSeveritySetterVa)
        && browser.Matches(0x1028A2CE, "8B8C2454010000")
        && browser.Matches(0x1028A2E1, "890DF86A5911"));
    Check("Awesomium verbose configuration receives threshold address and conditionally writes its negated output",
        browser.Matches(0x1028A51B, "68046B5911")
        && browser.Calls(0x1028A555, LoggingSetupNativeLayout.VerboseConfigurationConstructorVa)
        && browser.Matches(0x102B57BC, "8B4610895C24203BC3744E")
        && browser.Calls(0x102B57E7, LoggingSetupNativeLayout.IntegerConversionWrapperVa)
        && browser.Matches(0x102B57EF, "84C0740D8B4C24208B5510F7D9890A"));
    Check("Awesomium numeric failure can retain prefix output and saturates at signed Int32 limits",
        browser.Matches(0x1029C43F, "C70600000000")
        && browser.Matches(0x1029C3CF, "C70600000000")
        && browser.Matches(0x1029C451, "8A018D50D080FA0977EE")
        && browser.Matches(0x1029C478, "0FB6C00106413BCD75CF")
        && browser.Matches(0x1029C489, "C706FFFFFF7F5E32C0")
        && browser.Matches(0x1029C419, "C706000000805E32C0")
        && browser.Matches(0x1029C8D2, "32DB"));
    Check("Awesomium minimum severity getter and signed setter preserve the fallback DWORD",
        browser.Matches(ConversionDiagnosticNativeLayout.MinimumSeverityGetterVa, "A1046B5911C3")
        && browser.Matches(ConversionDiagnosticNativeLayout.MinimumSeveritySetterVa,
            "837C2404037D0C8D4424048B00A3046B5911C3B890B027118B08890D046B5911C3")
        && browser.Word(ConversionDiagnosticNativeLayout.MinimumSeverityFallbackVa) == 3);
    Check("Awesomium interceptor setter stores and returns the supplied pointer without calling it",
        browser.Matches(ConversionDiagnosticNativeLayout.InterceptorSetterVa,
            "8B442404A31C6B5911C3"));
    Check("Awesomium GPU initialization gates interceptor installation on failure byte and switch absence",
        browser.Matches(0x1019CAC1, "389E940000007460")
        && browser.Ascii(0x11265E8C) == "single-process"
        && browser.Ascii(0x11265BEC) == "in-process-gpu"
        && browser.Calls(0x1019CB7F, 0x1028C3C0)
        && browser.Matches(0x1019CB84, "84C0755A")
        && browser.Calls(0x1019CBD4, 0x1028C3C0)
        && browser.Matches(0x1019CBD9, "C64424130184C07405C644241300")
        && browser.Matches(0x1019CC41, "807C241300740D68C0C91910")
        && browser.Calls(0x1019CC4D, ConversionDiagnosticNativeLayout.InterceptorSetterVa));
    Check("Awesomium GPU teardown clears global interceptor rather than restoring an earlier callback",
        browser.Matches(0x1019C7F3, "6A00")
        && browser.Calls(0x1019C802, ConversionDiagnosticNativeLayout.InterceptorSetterVa));
    Check("Awesomium GPU interceptor attempts message forwarding then normally returns false",
        browser.Calls(0x1019C9F2, 0x100038B0)
        && browser.Matches(0x1019C9F7, "6AFF5657")
        && browser.Calls(0x1019CA0F, 0x100038B0)
        && browser.Calls(0x1019CA33, 0x1015DD90)
        && browser.Calls(0x1019CA3E, 0x10190450)
        && browser.Matches(0x1019CA43, "8B50048D48048B420456FFD0")
        && browser.Matches(0x1019CA93, "32C0"));
    Check("Awesomium converter constructs severity-three diagnostic and preserves LastError",
        browser.Matches(0x1029AA0E, "6A0368A7010000")
        && browser.Matches(0x10289D30, "8B44240C")
        && browser.Matches(0x10289D40, "8906")
        && browser.ImportSlotNames(0x11240308, "GetLastError")
        && browser.Matches(0x10289D65, "8986AC000000")
        && browser.ImportSlotNames(0x11240328, "SetLastError")
        && browser.Matches(0x1028A0DB, "8B96AC00000052"));
    Check("Awesomium diagnostic interceptor can bypass severity dispatch",
        browser.Matches(0x10289EEC, "A11C6B591185C0743B")
        && browser.Matches(0x10289F12, "FFD083C41484C07415")
        && browser.Matches(0x10289F20, "0F82B5010000")
        && browser.Matches(0x10289F2B, "E9A3010000"));
    Check("Awesomium severity three selects custom handler or default display, distinct from severity four",
        browser.Matches(0x1028A042, "83F8047533")
        && browser.Matches(0x1028A07A, "83F8037548")
        && browser.Matches(0x1028A07F, "833D186B591100")
        && browser.Matches(0x1028A08E, "7412")
        && browser.Matches(0x1028A09A, "FF15186B5911")
        && browser.Calls(0x1028A0AC, ConversionDiagnosticNativeLayout.DefaultDisplayHelperVa));
    Check("Awesomium default diagnostic display requires nonempty original and converted messages and enable byte",
        browser.Matches(0x10288EBC, "837E10000F8446010000")
        && browser.Matches(0x10288EC6, "803D136B5911000F8439010000")
        && browser.Calls(0x10288F3E, 0x10296210)
        && browser.Matches(0x10288F46, "837C2470007515"));
    Check("Awesomium diagnostic launch uses debug_message.exe and blocks waiting after successful creation",
        browser.Matches(0x1127B0C0, Convert.ToHexString(System.Text.Encoding.Unicode.GetBytes(
            ConversionDiagnosticNativeLayout.HelperFileName + "\0")))
        && browser.Matches(0x10288F01, "68C0B02711")
        && browser.ImportSlotNames(0x112404DC, "CreateProcessW")
        && browser.Matches(0x10288FA9, "FF15DC04241185C07423")
        && browser.Matches(0x10288FB7, "6AFF51FF15D0042411")
        && browser.ImportSlotNames(0x112404D0, "WaitForSingleObject")
        && browser.ImportSlotNames(0x11240390, "CloseHandle")
        && browser.Matches(0x10288FCB, "FFD68B44240C50FFD6"));
    Check("Awesomium failed diagnostic launch selects Fatal error message box",
        browser.Matches(0x1127B0A8, Convert.ToHexString(System.Text.Encoding.Unicode.GetBytes(
            ConversionDiagnosticNativeLayout.MessageBoxTitle + "\0")))
        && browser.Matches(0x10288FE5, "681000040068A8B02711506A00")
        && browser.Matches(0x10288FF2, "FF15C8BD5711"));
    Check("Awesomium names lower only ASCII uppercase bytes through length-bounded loop",
        browser.Matches(0x10007ACD, "8B561003D03BCA7415")
        && browser.Matches(0x10007AD6, "8A018D50BF0FBEC080FA19770383C020880141EBD6")
        && browser.Calls(0x1028D82F, SwitchNameNativeLayout.AsciiLowercaseVa)
        && browser.Calls(0x1028C4C4, SwitchNameNativeLayout.AsciiLowercaseVa));
    Check("Awesomium wide converter conditionally diagnoses code units above ASCII",
        browser.Matches(0x1029A9D5, "833DF86A591101")
        && browser.Matches(0x1029A9E4, "83F8037F66")
        && browser.Matches(0x1029AA00, "66833C417F7707")
        && browser.Matches(0x1029AA10, "68A7010000")
        && browser.Calls(SwitchNameNativeLayout.DiagnosticDestructorCallVa, 0x10289EC0));
    Check("Awesomium returning wide converter copies low bytes in two-byte source strides",
        browser.Matches(0x1029AA6B, "8B56108D0450")
        && browser.Calls(0x1029AA99, SwitchNameNativeLayout.LowByteCopyVa)
        && browser.Matches(0x1000B3A0, "8A18")
        && browser.Matches(0x1000B3F2, "881C08")
        && browser.Matches(0x1000B40B, "83C002894424143B4424187588")
        && browser.Calls(0x1028DDCF, SwitchNameNativeLayout.WideToNarrowVa));
    Check("Awesomium ASCII value getter rejects non-ASCII code units before conversion",
        browser.Calls(0x1028C8C5, 0x1028C480)
        && browser.Calls(0x1028C8CF, SwitchNameNativeLayout.WideAsciiPredicateVa)
        && browser.Matches(0x1028C8D7, "84C0751E")
        && browser.Matches(0x1028C8DB, "C746140F000000C74610000000008806")
        && browser.Calls(0x1028C8FF, SwitchNameNativeLayout.WideToNarrowVa)
        && browser.Matches(0x10299E1D, "66833C417F770A403BC672E9")
        && browser.Matches(0x10299E29, "5FB0015EC35F32C05EC3"));
    Check("Awesomium map builder removes prefix from folded name but copies wide value directly",
        browser.Calls(0x1028D86D, 0x1028BB70)
        && browser.Matches(0x1028D877, "8BE855")
        && browser.Calls(0x1028D893, 0x100038B0)
        && browser.Calls(0x1028D8A0, 0x1028D310)
        && browser.Matches(0x1028D8A5, "6AFF53578BC8")
        && browser.Calls(0x1028D8AB, 0x10007FA0));
    Check("Awesomium parsed switches update the same map in admitted argument order",
        browser.Calls(0x1028DFA6, 0x1028DCD0)
        && browser.Calls(0x1028DDB4, 0x1028BC60)
        && browser.Calls(0x1028DDCF, 0x1029A9C0)
        && browser.Calls(0x1028DDDC, 0x1028D7F0)
        && browser.Matches(0x1028DE67, "FF442414")
        && browser.Matches(0x1028DE80, "83C51C"));
    Check("Awesomium existing normalized key returns its value slot for unconditional assignment",
        browser.Matches(0x1028D332, "3B6E047424837D20108B4D1C8D450C72028B00")
        && browser.Calls(0x1028D34E, 0x1000A900)
        && browser.Matches(0x1028D353, "85C00F89E3000000")
        && browser.Matches(0x1028D444, "8D4528")
        && browser.Calls(0x1028D82F, 0x10007AB0)
        && browser.Calls(0x1028D8A0, 0x1028D310)
        && browser.Calls(0x1028D8AB, 0x10007FA0));
    Check("Awesomium child type getter reads the updated switch map",
        browser.Calls(0x1003A694, 0x1028C8A0)
        && browser.Ascii(0x11265DD8) == "type"
        && browser.Calls(0x1028C4C4, 0x10007AB0)
        && browser.Calls(0x1028C4D4, 0x100F7110)
        && browser.Calls(0x1028C585, 0x10007FA0));
    Check("Awesomium exact end marker clears initially enabled scanning state stickily",
        browser.Calls(0x1028DFA6, SwitchAdmissionNativeLayout.ParsedSwitchLoopVa)
        && browser.Matches(0x1028DD0F, "C644240B01")
        && browser.Calls(SwitchAdmissionNativeLayout.EndOfSwitchComparisonCallVa, 0x10014B20)
        && browser.Matches(0x1028DD9A, "85C00F95C2205424137455")
        && browser.Matches(SwitchAdmissionNativeLayout.EndOfSwitchMarkerVa, "2D002D000000"));
    Check("Awesomium admitted tokens use splitter and native prefix validator",
        browser.Calls(SwitchAdmissionNativeLayout.SwitchSplitCallVa, 0x1028BC60)
        && browser.Calls(SwitchAdmissionNativeLayout.PrefixValidatorCallVa, 0x1028BB70)
        && browser.Calls(0x1028BBF4, 0x10014B20)
        && browser.Matches(SwitchAdmissionNativeLayout.SwitchPrefixTableVa, "F0B32711ECB32711E8B32711")
        && browser.Matches(0x1127B3E8, "2F0000002D0000002D002D000000"));
    Check("Awesomium switch map and positional append paths remain distinct",
        browser.Calls(0x1028DDCF, 0x1029A9C0)
        && browser.Calls(SwitchAdmissionNativeLayout.SwitchMapBuilderCallVa, 0x1028D7F0)
        && browser.Calls(SwitchAdmissionNativeLayout.PositionalAppendCallVa, 0x10155F50));
    var dbghelp = Load("dbghelp_red5.dll", 1213200,
        "C06430B8CB025BE506BE50A756488E1BCC3827C4F45158D93E4E3EEB98CE1E4F");
    var deployment = Load("DeploymentCommon.dll", 76800,
        "33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0");
    var client = Load("FirefallClient.exe.original", 40197848,
        "E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235");
    var samples = new[] { red, dumper, truck, hardware, child, browser, dbghelp, deployment, client };
    Check("All nine identified samples have I386 PE headers; only DeploymentCommon has a CLR header",
        samples.All(sample => sample.IsI386)
        && samples.Count(sample => sample.HasClrHeader) == 1 && deployment.HasClrHeader);
    Check("Companion VAs use their actual image bases, not the client's raw-image address convention",
        red.ImageBase == 0x400000 && dumper.ImageBase == 0x400000
        && truck.ImageBase == 0x400000 && browser.ImageBase == 0x10000000
        && dbghelp.ImageBase == 0x3000000);

    foreach (var field in DumperHandoffNativeLayout.Fields)
    {
        string valueFormat = field.Kind switch
        {
            "str" => "%s", "i32" => "%u", "i64" => "%llu",
            _ => throw new InvalidDataException($"Unobserved stream kind: {field.Kind}")
        };
        Check($"RedHanded stream {field.StreamType:X} uses {field.Kind}, decimal ID, and CRLF",
            red.Matches(field.StreamTypePushVa, "68")
            && red.Word(field.StreamTypePushVa + 1) == field.StreamType
            && red.Ascii(field.FormatStringVa) == $"%d|{field.Kind}|{valueFormat}\r\n"
            && red.Calls(field.FormatCallVa, 0x40FD60));
    }
    Check("RedHanded pipe capacity is 16KiB and handle inheritance is enabled for child creation",
        red.Matches(0x4070FD, "6800400000")
        && red.ImportSlotNames(0x5440E8, "CreatePipe")
        && red.Matches(0x407117, "FF15E8405400")
        && red.Matches(0x407443, "68000000086A01")
        && red.Matches(0x407457, "FF15E8425400")
        && red.ImportSlotNames(0x5442E8, "CreateProcessA"));
    Check("RedHanded writes once, requires exact byte count, then waits or terminates with BADC0DE",
        red.Matches(0x40749B, "FF15D8405400")
        && red.ImportSlotNames(0x5440D8, "WriteFile")
        && red.Matches(0x4074C3, "3B854CDFFFFF7560")
        && red.Matches(0x4074EB, "FF15D0405400")
        && red.ImportSlotNames(0x5440D0, "WaitForSingleObject")
        && red.Matches(0x407574, "68DEC0AD0B")
        && red.ImportSlotNames(0x5440D4, "TerminateProcess"));
    Check("RedHanded calls the same dumper launcher for ordinary and full-memory dumps",
        red.Calls(DumperHandoffNativeLayout.PrimarySpawnCallVa, DumperHandoffNativeLayout.SpawnVa)
        && red.Matches(0x40787C, "C785CCFCFFFF40000000")
        && red.Matches(0x408320, "6A02")
        && red.Calls(DumperHandoffNativeLayout.FullMemorySpawnCallVa, DumperHandoffNativeLayout.SpawnVa));
    Check("RedHanded appends a blank CRLF line after all eleven metadata records",
        red.Matches(0x408167, "6A02689C575400")
        && red.Ascii(0x54579C) == DumperHandoffNativeLayout.EndOfMetadata
        && red.Calls(0x408174, 0x40FF30));
    Check("RedHanded parses signed option numbers and substitutes defaults only for zero",
        red.ImportSlotNames(0x5447C4, "atoi")
        && red.Matches(0x4048C2, "8B35C447540051FFD6")
        && red.Matches(0x4048F1, "51FFD6")
        && red.Matches(0x4049BF, "8B442428B983C9010085C00F44C1B9AA070000")
        && red.Matches(0x4049D6, "8B44242C85C00F44C1"));
    Check("RedHanded initializes stream-owner fields from app/build/environment option locals",
        red.Matches(0x404A72, "89842480000000")
        && red.Matches(0x404A82, "8B442428898424840000008B44242C89842488000000")
        && red.Matches(0x404AAB, "8984248C000000")
        && red.Matches(0x404ACE, "89842490000000")
        && red.Matches(0x404B36, "8D4C246850")
        && red.Calls(0x404B3B, 0x406820));
    Check("Stream-owner initializer names the source revision, build number, and environment options",
        red.Matches(0x40489E, "B9E4515400")
        && red.Ascii(0x5451E4) == "sourcerev"
        && red.Matches(0x4048D2, "B9F4515400")
        && red.Ascii(0x5451F4) == "buildnumber"
        && red.Matches(0x4048FB, "B900525400")
        && red.Ascii(0x545200) == "envmnemonic"
        && red.Matches(0x40495D, "B90C525400")
        && red.Ascii(0x54520C) == "envname");
    Check("Both exception and RIP-event handlers pass the DEBUG_EVENT thread ID to stream1000A",
        red.Matches(0x4098B6, "8D4D90")
        && red.Calls(0x4098B9, 0x458B20)
        && red.Matches(0x4099FB, "8D459C5051FF75988BCF")
        && red.Calls(DumperHandoffNativeLayout.ExceptionHandlerCallVa,
            DumperHandoffNativeLayout.BuildMetadataVa)
        && red.Matches(0x409A7B, "6A000F45C851FF75988BCF")
        && red.Calls(DumperHandoffNativeLayout.RipHandlerCallVa,
            DumperHandoffNativeLayout.BuildMetadataVa)
        && red.Matches(0x407665, "8B4D08")
        && red.Matches(0x407670, "898DC4FCFFFF")
        && red.Matches(0x408031, "FFB5C4FCFFFF")
        && red.Matches(0x40803D, "680A000100"));
    Check("Source reads require a private exception, two parameters, and length at most1024",
        red.Matches(0x40789A, "8B062D00D52E01741748740A")
        && red.Matches(0x4078B0, "C785CCFCFFFF02000000")
        && red.Matches(0x4078BA, "837E1002727C8B761481FE000400007761")
        && red.Matches(0x407908, "FF7018")
        && red.Matches(CrashMetadataNativeLayout.ReadSourceCallVa, "FF15BC405400")
        && red.ImportSlotNames(0x5440BC, "ReadProcessMemory")
        && red.Matches(0x407916, "85C07528"));
    Check("Source failures use explicit markers rather than fabricated source text",
        red.Ascii(0x5456B0) == CrashMetadataNativeLayout.UnspecifiedSourceMarker
        && red.Ascii(0x5456C0) == CrashMetadataNativeLayout.BadMemorySourceMarker
        && red.Ascii(0x5456CC) == CrashMetadataNativeLayout.TooLongSourceMarker
        && red.Ascii(0x5456D8) == CrashMetadataNativeLayout.ExceptionSourceMarker
        && red.Matches(0x407871, "68B0565400")
        && red.Matches(0x4078A6, "68D8565400")
        && red.Matches(0x40791A, "68C0565400")
        && red.Matches(0x40792C, "68CC565400"));
    bool ClientMatches(uint raw, string hex) => client.Matches(raw + 0x400C00, hex);
    Check("Original client resolves its exception import to KERNEL32 RaiseException",
        client.ImportSlotNames(DebugExceptionHandoffNativeLayout.ClientRaiseExceptionImportVa, "RaiseException")
        && client.ImportsFrom("KERNEL32.dll").Contains("RaiseException"));
    Check("Original client builds source/message strlen-pointer pairs and passes four DWORD arguments",
        ClientMatches(0x12C34CD, "894DD4897DD8")
        && ClientMatches(0x12C34E1, "894DDC8955E0")
        && ClientMatches(0x12C34F3, "8D45D4506A046A0056FF15B041AC01")
        && ClientMatches(0x12C351B, "8D45D4506A046A0156FF15B041AC01"));
    Check("Original-client fatal path uses noncontinuable500 while assertion paths use continuable502",
        ClientMatches(0x12C35C5, "6A00578BD3B900D52E01")
        && client.Calls(0x12C35CF + 0x400C00, 0x12C3470 + 0x400C00)
        && ClientMatches(0x12C36D5, "6A01538BD7B902D52E01")
        && client.Calls(0x12C36DF + 0x400C00, 0x12C3470 + 0x400C00)
        && ClientMatches(0x12C396D, "B902D52E01")
        && client.Calls(0x12C3972 + 0x400C00, 0x12C3470 + 0x400C00));
    Check("Original client attempts Red5 setup only before either cached state byte is set",
        ClientMatches(Red5LoggingBridgeNativeLayout.InitializerRaw + 0x2A, "8A477384C00F85DE0000003847720F85D5000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.InitializerRaw + 0x3E, "BAAC4BBD01")
        && ClientMatches(Red5LoggingBridgeNativeLayout.InitializerRaw + 0x81, "FF15C84EAC01")
        && ClientMatches(Red5LoggingBridgeNativeLayout.InitializerRaw + 0x8B, "FF15C44EAC01")
        && ClientMatches(Red5LoggingBridgeNativeLayout.InitializerRaw + 0xA8, "FF15E04EAC01")
        && ClientMatches(Red5LoggingBridgeNativeLayout.InitializerRaw + 0xB7, "C6477301B301")
        && ClientMatches(Red5LoggingBridgeNativeLayout.InitializerRaw + 0xFB, "C647720132DB"));
    Check("Original client imports the JSObject custom-method registration API",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.SetCustomMethodImportVa,
            "?SetCustomMethod@JSObject@Awesomium@@QAEXABVWebString@2@_N@Z"));
    Check("Original client constants name the Red5 JS object and errorlog method",
        client.Matches(Red5LoggingBridgeNativeLayout.Red5ObjectNameVa, "52006500640035000000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ErrorLogMethodNameVa,
            "6500720072006F0072006C006F0067000000")
        && client.Ascii(Red5LoggingBridgeNativeLayout.CreationFailureFormatVa)
            == Red5LoggingBridgeNativeLayout.CreationFailureFormat);
    Check("Original client has one additional guarded SetCustomMethod call site and one WebPreferences constructor site",
        ClientMatches(0xD64530 + 0x4B, "FF15E04EAC01")
        && client.ImportSlotNames(0x1AC4E10, "??0WebPreferences@Awesomium@@QAE@XZ")
        && ClientMatches(0xCBC1B1, "FF15104EAC01"));
    Check("Original client AddWebCallback binds a Lua callback reference to the named JavaScript method",
        client.Calls(0xCCAE67 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaCallbackReferenceCreateRaw + 0x400C00)
        && client.Calls(0xCCAE74 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaCallbackRegistrationRaw + 0x400C00)
        && ClientMatches(0xCCC162, "83B81804000000")
        && ClientMatches(0xCCC172, "83FFFF")
        && ClientMatches(0xCCC17B, "83FFFE")
        && ClientMatches(0xCCC1A5, "6A270F45D853FF15C848AC01")
        && ClientMatches(0xCCC1C5, "6A5C0F45F056FF15C848AC01")
        && client.Calls(0xCCC20C + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaCallbackReferenceReleaseRaw + 0x400C00)
        && client.Calls(0xCCC22B + 0x400C00, 0xCCD210 + 0x400C00)
        && ClientMatches(0xCCC233, "8930897804")
        && ClientMatches(0xCCC238, "8B4424188B881C0400008B01FF5030")
        && client.Matches(Red5LoggingBridgeNativeLayout.CallbackQueueConsumerRttiLocatorPointerVa, "E00FCD01")
        && client.Matches(Red5LoggingBridgeNativeLayout.CallbackRegistrationVtableSlotVa, "30511601"));
    Check("Original client routes the other guarded Red5 call through SetPropertyAsync, not SetCustomMethod",
        ClientMatches(0xD651B0 + 0x68, "FF15F04EAC01")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.SetPropertyAsyncImportVa,
            "?SetPropertyAsync@JSObject@Awesomium@@QAEXABVWebString@2@ABVJSValue@2@@Z"));
    Check("Original client Lua WebFrame registration names and forwards all twelve methods",
        new (uint Va, string Name, uint StubRaw, string StubBytes)[]
        {
            (0x1BC7880, "LoadUrl", 0xCCE8B0, "558BEC5DE9E7C2FFFF"),
            (0x1BC77DC, "GetUrl", 0xCCE8C0, "558BEC5DE9F7C3FFFF"),
            (0x1BC77E4, "AddWebCallback", 0xCCE8D0, "558BEC5DE997C4FFFF"),
            (0x1BC77F4, "AddWebValue", 0xCCE8E0, "558BEC5DE927C6FFFF"),
            (0x1BC7800, "CallWebFunc", 0xCCE8F0, "558BEC5DE967C7FFFF"),
            (0x1BC779C, "SetUrlFilters", 0xCCE900, "558BEC5DE9B7C8FFFF"),
            (0x1BC77AC, "LogWebRequests", 0xCCE910, "558BEC5DE9B7CAFFFF"),
            (0x1BC77BC, "SetNaturalDims", 0xCCE920, "558BEC5DE987CCFFFF"),
            (0x1BC77CC, "ClearCookies", 0xCCE930, "558BEC5DE9A7CBFFFF"),
            (0x1BC775C, "GetNaturalDims", 0xCCE940, "558BEC5DE9C7CDFFFF"),
            (0x1BC776C, "EnableTypeTarget", 0xCCE950, "558BEC5DE967CEFFFF"),
            (0x1BC7780, "IsTypeTarget", 0xCCE960, "558BEC5DE957CFFFFF")
        }.All(method =>
            client.Ascii(method.Va) == method.Name && ClientMatches(method.StubRaw, method.StubBytes))
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaWebFrameBindingTableRaw + 0x2D + 0x400C00,
            0xCD5020 + 0x400C00));
    Check("Original client binds AddWebValue and CallWebFunc to the expected WebFrame virtual slots",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaAddWebValueRaw + 0xCD, "FF5054")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaCallWebFuncRaw + 0xD8, "FF5058")
        && client.Matches(0x1C76650, "B05D1601")
        && client.Matches(0x1C76654, "505E1601")
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaAddWebValueUsageVa)
            == "Usage: FRAME:AddWebValue('name', value)"
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaCallWebFuncUsageVa)
            == "Usage: FRAME:CallWebFunc('name', ...)");
    Check("Original client CallWebFunc target expression names Red5Callbacks",
        client.Matches(Red5LoggingBridgeNativeLayout.LuaWebFunctionTargetExpressionVa + 14,
            "5200650064003500430061006C006C006200610063006B0073"));
    Check("Original client CallWebFunc initializes its target and checks method existence",
        client.Calls(Red5LoggingBridgeNativeLayout.LuaWebFunctionDispatcherRaw + 0x400C00 + 0xA1,
            0xD63E20 + 0x400C00)
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaWebFunctionDispatcherRaw + 0x135,
            "FF15F44EAC01"));
    Check("Original client CallWebFunc selects asynchronous or synchronous JavaScript invocation",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaWebFunctionDispatcherRaw + 0x2D9,
            "FF15EC4EAC01")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaWebFunctionDispatcherRaw + 0x2FB,
            "FF15DC4EAC01")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.LuaWebFunctionHasMethodImportVa,
            "?HasMethod@JSObject@Awesomium@@QBE_NABVWebString@2@@Z")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.CallbackInvokeAsyncImportVa,
            "?InvokeAsync@JSObject@Awesomium@@QAEXABVWebString@2@ABVJSArray@2@@Z")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.CallbackInvokeImportVa,
            "?Invoke@JSObject@Awesomium@@QAE?AVJSValue@2@ABVWebString@2@ABVJSArray@2@@Z"));
    Check("Original client CallWebFunc diagnostics identify unavailable or missing functions",
        client.Ascii(Red5LoggingBridgeNativeLayout.LuaWebFunctionMissingFormatVa)
            == "'%s': JavaScript function '%s' not found"
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaWebCallbacksUnavailableFormatVa)
            == "'%s': JavaScript callback object is not available (attempting %s)"
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaWebFunctionArgumentLogFormatVa) == "   #%d: %s"
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaWebFunctionCallLogFormatVa)
            == "'%s': Executing JavaScript function '%s'"
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaWebFunctionReturnLogFormatVa)
            == "Javascript function returned: %s");
    Check("Original client converts Lua WebFrame values and sets properties asynchronously",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaWebValueSetterRaw + 0x68, "FF15F04EAC01")
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaWebValueSetterRaw + 0x34 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaValueToJavaScriptRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaWebFunctionDispatcherRaw + 0x172 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaValueToJavaScriptRaw + 0x400C00)
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.SetPropertyAsyncImportVa,
            "?SetPropertyAsync@JSObject@Awesomium@@QAEXABVWebString@2@ABVJSValue@2@@Z"));
    Check("Original client replaces URL-filter storage and keeps only filters ending in a wildcard",
        client.Calls(Red5LoggingBridgeNativeLayout.LuaSetUrlFiltersRaw + 0x18F + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterListUpdateRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterListUpdateRaw + 0x29 + 0x400C00,
            0x2F89A0 + 0x400C00)
        && ClientMatches(0xCCCBBE, "FF159448AC01")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.LuaUrlFilterStringCompareImportVa, "strncmp")
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaUrlFilterWildcardVa) == "*"
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaUrlFilterIllegalFormatVa)
            == "SetUrlFilters: Illegal URL filter: %s"
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaSetUrlFiltersUsageVa)
            == "Usage: FRAME:SetUrlFilters('protocol://host/somepath/*', 'filter2', ...)");
    Check("Original client compares candidate text against stored filter prefixes and inverts the match result",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterListMatcherRaw + 0x44, "488945FC")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterListMatcherRaw + 0x52, "7729")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterListMatcherRaw + 0x70, "FF159448AC01")
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterCallbackRaw + 0xD1 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterListMatcherRaw + 0x400C00)
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterCallbackRaw + 0xE4, "0F94C3")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectConstructorRaw + 0x3D,
            "C706CC47C701")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectConstructorRaw + 0x43,
            "C74604B847C701")
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectSetupRaw + 0x52E + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectFactoryRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectFactoryRaw + 0x47 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectConstructorRaw + 0x400C00)
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectSetupRaw + 0x533,
            "8987040A0000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectSetupRaw + 0x554,
            "FF5008")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectSetupRaw + 0x53B,
            "385520740B38575CB8020000000F45D0")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectSetupRaw + 0x165,
            "C6005C")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectSetupRaw + 0x499,
            "685CA1BB01")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectSetupRaw + 0x501,
            "6860A1BB01")
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectSetupRaw
                + 0x4BA + 0x400C00,
            0x12BBEC0 + 0x400C00)
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectBasePathEmptyFallbackVa,
            "2E2F00")
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectFirstArgumentEmptyFallbackVa,
            "77656200")
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectSecondArgumentEmptyFallbackVa,
            "2E2F00")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectConstructorRaw + 0x67,
            "C7462C07000000C7462800000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectPrimaryReleaseSlotRaw,
            "8B490885C974058B01FF6018C3")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectPrimaryCleanupSlotRaw,
            "568BF18B4E0C85C97405")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectStateUpdateRaw + 0x41,
            "837E2800")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectStateUpdateRaw + 0x100,
            "C6466801")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw + 0x83,
            "C6466800")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectOwnerCleanupRaw + 0x28A,
            "8B8BA4090000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectOwnerCleanupRaw + 0x3A,
            "8B8B040A0000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectOwnerCleanupRaw + 0x46,
            "6A01FF5010")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectOwnerCleanupRaw + 0x4B,
            "C783040A000000000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectOwnerCleanupRaw + 0x26E,
            "8B8BA809000083E910")
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectOwnerCleanupRaw
                + 0x277 + 0x400C00,
            0x12C8D60 + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectOwnerCleanupRaw
                + 0x2C6 + 0x400C00,
            0x12C8D60 + 0x400C00)
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectOwnerCleanupRaw + 0x27C,
            "C783A809000000000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectOwnerCleanupRaw + 0x2CB,
            "C783A409000000000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchFirstEntryRaw + 0xAEB,
            "33C0")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchFirstEntryRaw + 0xB10,
            "C20400")
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw
                + 0x7E + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectStateUpdateRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetWrapperRaw
                + 0x06 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw + 0x400C00)
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectDeleteSlotRaw + 0x06,
            "E835000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectDeleteSlotRaw + 0x0B,
            "F6450801")
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchTableVa, "C0DE0B01")
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchTableCallbackSlotVa, "E0E90B01")
        && Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchFirstEntryPointerVa
            == Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchFirstEntryRaw + 0x400C00
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectPrimaryVtableVa, "50CD0B01")
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectPrimaryCleanupSlotVa, "60CD0B01")
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectSetupSlotVa, "80CD0B01")
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectDeleteSlotVa, "D0D00B01")
        && Red5LoggingBridgeNativeLayout.LuaUrlFilterCallbackPointerVa
            == Red5LoggingBridgeNativeLayout.LuaUrlFilterCallbackRaw + 0x400C00
        && Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectPrimaryCleanupSlotPointerVa
            == Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectPrimaryCleanupSlotRaw + 0x400C00
        && Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectSetupSlotPointerVa
            == Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectSetupSlotRaw + 0x400C00
        && Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectDeleteSlotPointerVa
            == Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectDeleteSlotRaw + 0x400C00);
    Check("Original client URL parser recognizes HTTP and HTTPS case-insensitively",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseCompareImportVa, "_stricmp")
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseHttpSchemeVa) == "http"
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseHttpsSchemeVa) == "https"
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseRaw + 0x5D,
            "8B3DA848AC01"));
    Check("Original client URL parser copies at most 2048 characters and uses atoi ports",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseCopyImportVa, "strncpy")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseFindCharacterImportVa,
            "strchr")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParsePortImportVa, "atoi")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseRaw + 0x19,
            "6800080000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseRaw + 0xE3,
            "B8BB010000B950000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseRaw + 0x100,
            "3Dfeff0000"));
    Check("Original client URL parser initializes slash path",
        client.Ascii(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseDefaultPathVa) == "/");
    Check("Original client URL parser splits fragments",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseRaw + 0x149, "6A23"));
    Check("Original client URL parser splits queries",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseRaw + 0x15C, "6A3F"));
    Check("Original client URL component escape preserves only alphanumerics and RFC unreserved punctuation",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlComponentEscapeRaw + 0xAB,
            "0FBEC383C0D383F8517777")
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlComponentEscapeAllowlistVa,
            "000001")
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlComponentEscapeAllowlistVa + 50,
            "00")
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlComponentEscapeAllowlistVa + 81,
            "00")
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlComponentEscapeHexDigitsVa)
            == "0123456789ABCDEF");
    Check("Original ClientApi URL lookup does exact lookup before default-port suffix fallbacks",
        client.Calls(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupRaw + 0x62 + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupKeyBuilderRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupRaw + 0x6E + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupTreeSearchRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupRaw + 0x176 + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupTreeSearchRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupTreeSearchRaw + 0x0C + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupTreeLowerBoundRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupKeyBuilderRaw + 0x81 + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupLowercaseTextRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupKeyBuilderRaw + 0x48 + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupFormatSharedTextRaw + 0x400C00)
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlParseFindCharacterImportVa,
            "strchr"));
    Check("Original ClientApi URL lookup lowercases direct and suffix candidates",
        client.Calls(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupLowercaseTextRaw + 0x39 + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupLowercaseRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupLowercaseTextRaw + 0x48 + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupLowercaseRaw + 0x400C00));
    Check("Original ClientApi URL update stores the same value under direct and suffix keys",
        client.Calls(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupUpdateRaw + 0x30 + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupKeyBuilderRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupUpdateRaw + 0x7B + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupTreeSearchRaw + 0x400C00)
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupUpdateRaw + 0xDB + 0x400C00,
            "895014")
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupUpdateRaw + 0x1CF + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupTreeInsertOrReplaceRaw + 0x400C00)
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupUpdateRaw + 0x1DB + 0x400C00,
            "8908"));
    Check("Original ClientApi URL lookup format literal",
        client.Ascii(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlLookupFormatVa)
            == "%s:%d");
    Check("Original ClientApi request query format literal",
        client.Ascii(Red5LoggingBridgeNativeLayout.ClientApiRequestQueryFormatVa)
            == Red5LoggingBridgeNativeLayout.ClientApiRequestQueryNativeFormat);
    Check("Original ClientApi stores its signed request under the signature header name",
        client.Ascii(Red5LoggingBridgeNativeLayout.ClientApiRequestSignatureHeaderNameVa)
            == Red5LoggingBridgeNativeLayout.ClientApiRequestSignatureHeaderName);
    Check("Original ClientApi formats map entries and submits them with WinHttpAddRequestHeaders",
        client.Ascii(Red5LoggingBridgeNativeLayout.ClientApiRequestHeaderFormatVa)
            == Red5LoggingBridgeNativeLayout.ClientApiRequestHeaderNativeFormat
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestAddHeadersImportVa,
            "WinHttpAddRequestHeaders")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw
            + 0x539 + 0x400C00, "FF15F44CAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw
            + 0x64E + 0x400C00, "FF15F44CAC01"));
    Check("Original ClientApi uses rand and CryptoAPI SHA-1 for the request nonce and signatures",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestRandImportVa, "rand")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestHashCreateImportVa,
            "CryptCreateHash")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestHashDataImportVa,
            "CryptHashData")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestHashGetValueImportVa,
            "CryptGetHashParam")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestHashDestroyImportVa,
            "CryptDestroyHash")
        && client.Ascii(Red5LoggingBridgeNativeLayout.ClientApiRequestSignatureHexDigitsVa)
            == "0123456789abcdef");
    Check("Original ClientApi learns a per-host clock offset from the HTTP Date header",
        client.Calls(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x831 + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestDateHeaderParserRaw + 0x400C00)
        && client.Ascii(Red5LoggingBridgeNativeLayout.ClientApiRequestDateHeaderNameVa) == "date"
        && client.Ascii(Red5LoggingBridgeNativeLayout.ClientApiRequestDateHeaderFormatVa)
            == "%3s, %d %3s %d %d:%d:%d %3s"
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestTimeImportVa, "_time64")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestDateScanImportVa, "sscanf")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestMonthCompareImportVa,
            "_strnicmp")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestUtcTimeImportVa,
            "_mkgmtime64"));
    Check("Original ClientApi request reset initializes timer fields from timeGetTime",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestTimerImportVa,
            "timeGetTime")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRequestResetRaw + 0xE7 + 0x400C00,
            "FF15244DAC018947684889476C894770894774"));
    Check("Original ClientApi worker records tracking, open, and pre-callback ticks",
        client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRequestSubmitTickStoreRaw + 0x400C00,
            "89466C")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRequestOpenTickStoreRaw + 0x400C00,
            "894170")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRequestCallbackTickStoreRaw + 0x400C00,
            "894174"));
    Check("Original ClientApi status callback handles handle-closing and close-handle transitions",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpCloseHandleImportVa,
            "WinHttpCloseHandle")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpStatusCallbackRaw + 0x2C + 0x400C00,
            "81FA00080000753D")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpStatusCallbackRaw + 0x3D + 0x400C00,
            "C7400803000000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpStatusCallbackRaw + 0x9D + 0x400C00,
            "C7400804000000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpStatusCallbackRaw + 0xA5 + 0x400C00,
            "FF15F84CAC01"));
    Check("Original ClientApi response uses a 0x20-byte object with distinct reset and worker kinds",
        client.Matches(Red5LoggingBridgeNativeLayout.ClientApiResponseResetRaw + 0x08 + 0x400C00,
            "C7470400000000C7470801000000C7470C000000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiResponseResetRaw + 0x4D + 0x400C00,
            "C7471400000000")
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiResponseCtorRaw + 0x60 + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiResponseResetRaw + 0x400C00)
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x106 + 0x400C00,
            "6A20")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x147 + 0x400C00,
            "894E04C7460800000000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiResponseDestroyRaw + 0x7D + 0x400C00,
            "C7461C00000000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiResponseDestroyRaw + 0x88 + 0x400C00,
            "8B4E1885C9")
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiResponseDestroyRaw + 0xDE + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiResponseTreeDestroyRaw + 0x400C00)
        && (uint)ClientApiResponseObservedKind.HandleClosing == 3
        && (uint)ClientApiResponseObservedKind.CloseAfterStatusDispatch == 4);
    Check("Original ClientApi WinHTTP callback context is 0x1C bytes with verified field stores",
        client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpCallbackContextAllocRaw + 0x400C00,
            "6A1C")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpCallbackContextInitRaw + 0x400C00,
            "C6471000C6471100C7471400000000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpCallbackContextHandleStoreRaw + 0x400C00,
            "8937")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpCallbackContextResultStoreRaw
            + 0x400C00, "894714")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpCallbackContextResponseStoreRaw
            + 0x400C00, "894718"));
    Check("ClientApi worker teardown marks its callback context for handle-close cleanup",
        client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpCallbackContextWorkerStoreRaw
            + 0x400C00, "89781C")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpCloseHandleImportVa,
            "WinHttpCloseHandle")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpWorkerDetachRaw + 0x34 + 0x400C00,
            "8B431C85C07404C6401101")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpWorkerDetachContextCleanupFlagStoreRaw
            + 0x400C00, "C6401101")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpWorkerDetachHandleCloseRaw
            + 0x400C00, "FF15F84CAC01C7432000000000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpCallbackContextCleanupFlagTestRaw
            + 0x400C00, "807E1100C70600000000")
        && Red5LoggingBridgeNativeLayout.ClientApiWinHttpCallbackContextCleanupFlagOffset == 0x11);
    Check("Original ClientApi registers the callback and sends its context as WinHttpSendRequest dwContext",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSetStatusCallbackImportVa,
            "WinHttpSetStatusCallback")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSendRequestImportVa,
            "WinHttpSendRequest")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x6B9 + 0x400C00,
            "FF150C4DAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x6C8 + 0x400C00,
            "57")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x6E3 + 0x400C00,
            "FF15104DAC01"));
    Check("Original ClientApi status dispatcher uses the verified response-stage WinHTTP imports",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpReceiveResponseImportVa,
            "WinHttpReceiveResponse")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpQueryHeadersImportVa,
            "WinHttpQueryHeaders")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpQueryDataAvailableImportVa,
            "WinHttpQueryDataAvailable")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpReadDataImportVa,
            "WinHttpReadData")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpStatusDispatchRaw + 0x3B9 + 0x400C00,
            "8B35E44CAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpStatusDispatchRaw + 0x3E4 + 0x400C00,
            "FFD6")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpStatusDispatchRaw + 0x5C3 + 0x400C00,
            "FF15E84CAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpStatusDispatchRaw + 0x659 + 0x400C00,
            "FF15EC4CAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpStatusDispatchRaw + 0x90E + 0x400C00,
            "FF15E04CAC01")
        && (uint)ClientApiWinHttpCallbackStatus.Redirect == 0x4000
        && (uint)ClientApiWinHttpCallbackStatus.SecureFailure == 0x10000
        && (uint)ClientApiWinHttpCallbackStatus.HeadersAvailable == 0x20000
        && (uint)ClientApiWinHttpCallbackStatus.DataAvailable == 0x40000
        && (uint)ClientApiWinHttpCallbackStatus.ReadComplete == 0x80000
        && (uint)ClientApiWinHttpCallbackStatus.WriteComplete == 0x100000
        && (uint)ClientApiWinHttpCallbackStatus.RequestError == 0x200000);
    Check("ClientApi REQUEST_ERROR extracts dwError and maps it to a failure HRESULT",
        client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpRequestErrorCompareRaw
            + 0x400C00, "81FA00002000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpRequestErrorLengthCheckRaw
            + 0x400C00, "837D0C08741A")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpRequestErrorCodeLoadRaw
            + 0x400C00, "FF7604")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpFailureHResultMapRaw
            + 0x400C00, "7E090FB7F681CE0000078085F6C745FCFFFFFFFFB9054000800F49F1")
        && ClientApiWinHttpErrorResultLayout.AsyncResultSize == 8
        && ClientApiWinHttpErrorResultLayout.ErrorCodeOffset == 4);
    foreach (var entry in new (int ErrorCode, int Result)[]
    {
        (0, unchecked((int)0x80004005u)),
        (1, unchecked((int)0x80070001u)),
        (0x123456, unchecked((int)0x80073456u)),
        (unchecked((int)0x80072EE7u), unchecked((int)0x80072EE7u))
    })
        Check($"ClientApi WinHTTP error HRESULT mapping {entry.ErrorCode:X8}",
            ClientApiWinHttpErrorResultLayout.CaptureFailureHResult(entry.ErrorCode) == entry.Result);
    uint[] secureFailureBits = { 1, 2, 4, 8, 0x10, 0x20, 0x40 };
    Check("Original ClientApi SECURE_FAILURE reads a DWORD status mask",
        client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSecureFailureStatusCompareRaw
            + 0x400C00, "81FA00000100")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSecureFailureLengthCheckRaw
            + 0x400C00, "837D0C04")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSecureFailureFlagLoadRaw
            + 0x400C00, "8B06"));
    Check("Original ClientApi SECURE_FAILURE labels the observed mask bits",
        secureFailureBits.Select((bit, index) => client.Matches(
            Red5LoggingBridgeNativeLayout.ClientApiWinHttpSecureFailureFlagTestsRaw
                + (uint)(0x16 * index) + 0x400C00,
            $"A8{bit:X2}")).All(matches => matches)
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSecureFailureFlagTestsRaw
            + 0x16 * 7 + 0x400C00, "85C0790E"));
    Check("Original ClientApi SECURE_FAILURE reports through the shared failure helper",
        client.Calls(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSecureFailureReportRaw + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiWinHttpFailureHelperRaw + 0x400C00)
        && ClientApiWinHttpSecureFailureLayout.StatusDataSize == 4
        && ClientApiWinHttpSecureFailureLayout.FailureErrorCode == 12175
        && ClientApiWinHttpSecureFailureLayout.CaptureLabeledFlags(0xFFFFFFFF) == 0x8000007F);
    Check("ClientApi suppresses WinHttpSendRequest when an earlier setup failure is stored",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpGetLastErrorImportVa,
            "GetLastError")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpPreSendFailureLastErrorCallRaw
            + 0x400C00, "FF157843AC01")
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiWinHttpPreSendFailureHelperCallRaw
            + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiWinHttpFailureHelperRaw + 0x400C00)
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpPreSendFailureResultStoreRaw
            + 0x400C00, "894714")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpPreSendFailureGateRaw
            + 0x400C00, "837F1400753D")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x6E3 + 0x400C00,
            "FF15104DAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpPreSendFailureHandleCloseRaw
            + 0x400C00, "FF15F84CAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpPreSendFailureHandleClearRaw
            + 0x400C00, "C70700000000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpPreSendFailureResponseKindStoreRaw
            + 0x400C00, "C7400804000000")
        && ClientApiWinHttpPreSendFailureLayout.ShouldSubmitRequest(0)
        && !ClientApiWinHttpPreSendFailureLayout.ShouldSubmitRequest(unchecked((int)0x80070005u))
        && ClientApiWinHttpPreSendFailureLayout.RequestHandleOffset == 0
        && ClientApiWinHttpPreSendFailureLayout.ResultOffset == 0x14
        && ClientApiWinHttpPreSendFailureLayout.ResponseKindOffset == 8
        && ClientApiWinHttpPreSendFailureLayout.ResponseKind == 4);
    Check("ClientApi DATA_AVAILABLE zero-byte completion marks EOF without starting a read",
        client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpDataAvailableEmptyBranchRaw
            + 0x400C00, "8B7D0C85FF75198B43183978087507C7400801000000B801000000")
        && ClientApiWinHttpDataAvailableLayout.BytesAvailableOffset == 0
        && ClientApiWinHttpDataAvailableLayout.StatusDataSize == 4
        && ClientApiWinHttpDataAvailableLayout.ResponseKindOffset == 8
        && ClientApiWinHttpDataAvailableLayout.ShouldStartRead(1)
        && !ClientApiWinHttpDataAvailableLayout.ShouldStartRead(0)
        && ClientApiWinHttpDataAvailableLayout.CaptureResponseKindOnEmptyData(0) == 1
        && ClientApiWinHttpDataAvailableLayout.CaptureResponseKindOnEmptyData(3) == 3);
    Check("ClientApi callback preserves DATA_AVAILABLE terminal kind while closing the handle",
        client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpDataAvailableTerminalCallbackRaw
            + 0x400C00, "85C07420837E140075038946148B4618837808007507C740080400000057FF15F84CAC01"));
    Check("ClientApi WinHttpOpenRequest selects secure and refresh flags from request fields",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpOpenRequestImportVa,
            "WinHttpOpenRequest")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpOpenRequestSecureFlagRaw
            + 0x400C00, "8D410C33F68338018985C8FBFFFFB8000080000F44F0")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpOpenRequestRefreshFlagRaw
            + 0x400C00, "80796000740681CE00010000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpOpenRequestRefreshFlagResetRaw
            + 0x400C00, "C6476000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpOpenRequestArgumentsRaw
            + 0x400C00, "566A006A006A00")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpOpenRequestCallRaw
            + 0x400C00, "FF15FC4CAC01")
        && new[]
        {
            (Https: 0u, Bypass: (byte)0, Flags: 0u),
            (Https: 1u, Bypass: (byte)0, Flags: 0x00800000u),
            (Https: 0u, Bypass: (byte)1, Flags: 0x00000100u),
            (Https: 1u, Bypass: (byte)1, Flags: 0x00800100u),
            (Https: 2u, Bypass: (byte)0xFF, Flags: 0x00000100u)
        }.All(entry => ClientApiWinHttpOpenRequestFlagLayout.CaptureFlags(entry.Https, entry.Bypass)
            == entry.Flags)
        && ClientApiWinHttpOpenRequestFlagLayout.HttpsSchemeOffset
            == Red5LoggingBridgeNativeLayout.ClientApiRequestUrlSchemeOffset
        && ClientApiWinHttpOpenRequestFlagLayout.BypassProxyCacheOffset == 0x60);
    Check("ClientApi route worker strictly times out idle workers after 120 seconds",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestTimerImportVa,
            "timeGetTime")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRouteWorkerCompletionGateRaw
            + 0x400C00, "837F1C00740C")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRouteWorkerTimeoutCheckRaw
            + 0x400C00, "FF15244DAC016A0068E80300006A0050E8399C9D002B47288945EC1B572C75073D000E2707766A")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRouteWorkerCtorTimestampSequenceRaw
            + 0x400C00, "FF15244DAC01B9E8030000F7E18BCE89462889562C")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerTimestampStoreRaw
            + 0x400C00, "89472889572C")
        && ClientApiRouteWorkerTimeoutLayout.CompletionOffset == 0x1C
        && ClientApiRouteWorkerTimeoutLayout.TimestampOffset == 0x28
        && ClientApiRouteWorkerTimeoutLayout.TimestampSize == 8
        && ClientApiRouteWorkerTimeoutLayout.TimeoutMicroseconds == 120_000_000
        && !ClientApiRouteWorkerTimeoutLayout.ShouldExpire(
            false, ClientApiRouteWorkerTimeoutLayout.TimeoutMicroseconds, 0)
        && ClientApiRouteWorkerTimeoutLayout.ShouldExpire(
            false, ClientApiRouteWorkerTimeoutLayout.TimeoutMicroseconds + 1, 0)
        && !ClientApiRouteWorkerTimeoutLayout.ShouldExpire(true, ulong.MaxValue, 0)
        && ClientApiRouteWorkerTimeoutLayout.ShouldExpire(false, 0, 1));
    Check("Original ClientApi redirect callback cracks the target and replays stored headers once",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpCrackUrlImportVa,
            "WinHttpCrackUrl")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpRedirectHostCompareImportVa,
            "_stricmp")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestHostLowercaseImportVa,
            "tolower")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiRequestAddHeadersImportVa,
            "WinHttpAddRequestHeaders")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpRedirectCrackUrlRaw + 0x400C00,
            "FF15F04CAC01")
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiWinHttpRedirectHostCompareRaw + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiWinHttpRedirectHostCompareHelperRaw + 0x400C00)
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpRedirectHostCompareHelperRaw
            + 0x18 + 0x400C00, "FF15A848AC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpRedirectHeaderFlagsRaw + 0x400C00,
            "6800000080")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpRedirectAddHeadersRaw + 0x400C00,
            "FF15F44CAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpRedirectReplayFlagStoreRaw + 0x400C00,
            "C6431001")
        && client.Calls(Red5LoggingBridgeNativeLayout.ClientApiRequestUrlHostLowercaseCallRaw + 0x400C00,
            Red5LoggingBridgeNativeLayout.ClientApiRequestUrlHostLowercaseHelperRaw + 0x400C00));
    Check("Original ClientApi request header count is a resettable tree size",
        client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRequestResetRaw + 0x41 + 0x400C00,
            "C7472400000000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRequestHeaderInsertRaw + 0x102 + 0x400C00,
            "8D4B20E8064F8DFF")
        && client.Calls(0x496A1F + 0x400C00, 0x31D720 + 0x400C00)
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRequestTreeCountIncrementRaw + 0x400C00,
            "8B5D1840894704")
        && Red5LoggingBridgeNativeLayout.ClientApiRequestHeaderTreeHeadOffset == 0x20
        && Red5LoggingBridgeNativeLayout.ClientApiRequestHeaderCountOffset == 0x24);
    Check("ClientApi redirect replay is gated by header count, one-shot flag, and host-only comparison",
        !ClientApiRedirectHeaderReplayPolicy.ShouldReplay("api.example", "API.EXAMPLE", 1, false)
        && ClientApiRedirectHeaderReplayPolicy.ShouldReplay("api.example", "other.example", 1, false)
        && !ClientApiRedirectHeaderReplayPolicy.ShouldReplay("api.example", "other.example", 0, false)
        && !ClientApiRedirectHeaderReplayPolicy.ShouldReplay("api.example", "other.example", 1, true)
        && Red5LoggingBridgeNativeLayout.ClientApiRequestUrlSchemeOffset == 0x0C
        && Red5LoggingBridgeNativeLayout.ClientApiRequestUrlHostOffset == 0x10
        && Red5LoggingBridgeNativeLayout.ClientApiRequestUrlPortOffset == 0x14
        && Red5LoggingBridgeNativeLayout.ClientApiRequestUrlPathOffset == 0x18
        && Red5LoggingBridgeNativeLayout.ClientApiRequestUrlQueryOffset == 0x1C);
    Check("ClientApi worker applies the request's five-redirect automatic limit",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSetOptionImportVa,
            "WinHttpSetOption")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiRequestResetRaw + 0x4D + 0x400C00,
            "66C7473C0502C6473E00")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x3FD + 0x400C00,
            "6A040FB6463C898584FBFFFF8D8584FBFFFF506A59FF37FF15084DAC01")
        && Red5LoggingBridgeNativeLayout.ClientApiRequestAutomaticRedirectLimitOffset == 0x3C
        && Red5LoggingBridgeNativeLayout.ClientApiRequestAutomaticRedirectLimitDefault == 5
        && Red5LoggingBridgeNativeLayout.ClientApiWinHttpSetAutomaticRedirectLimitRaw == 0xBC2F44);
    Check("ClientApi worker resolves per-URL proxy settings and applies proxy info",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpGetProxyForUrlImportVa,
            "WinHttpGetProxyForUrl")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpQueryOptionImportVa,
            "WinHttpQueryOption")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSetOptionImportVa,
            "WinHttpSetOption")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiProxyStringGlobalFreeImportVa,
            "GlobalFree")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpGetProxyForUrlRaw + 0x400C00,
            "FF15004DAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpQueryProxyInfoRaw + 0x400C00,
            "FF15044DAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSetProxyInfoRaw + 0x400C00,
            "FF15084DAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x2D6 + 0x400C00,
            "8B40048D490C0F57C0")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x310 + 0x400C00,
            "C7855CFBFFFF02000000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x31A + 0x400C00,
            "C78570FBFFFF01000000")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x373 + 0x400C00,
            "C78588FBFFFF0C000000508D8594FBFFFF506A26FF37FF15044DAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiPumpRequestWorkerRaw + 0x3AE + 0x400C00,
            "6A0C8D8594FBFFFF506A26FF37FF15084DAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpProxyStringFreeOneRaw + 0x400C00,
            "FFD6")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpProxyStringFreeTwoRaw + 0x400C00,
            "FFD6")
        && Red5LoggingBridgeNativeLayout.ClientApiWinHttpProxyOption == 0x26
        && Red5LoggingBridgeNativeLayout.ClientApiWinHttpProxyInfoSize == 0x0C
        && Red5LoggingBridgeNativeLayout.ClientApiWinHttpAutoproxyConfigUrlFlag == 0x02);
    Check("ClientApi opens asynchronous WinHTTP sessions with observed TLS settings",
        client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpCheckPlatformImportVa,
            "WinHttpCheckPlatform")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSessionPlatformCheckRaw + 0x400C00,
            "FF15184DAC01")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSessionPlatformCheckRaw + 6 + 0x400C00,
            "85C07514")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpOpenImportVa, "WinHttpOpen")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSetOptionImportVa,
            "WinHttpSetOption")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSessionOpenRaw + 0x400C00,
            "FF151C4DAC01")
        && client.Matches(0xBC3A27 + 0x400C00, "6800000010")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSessionEnableFeatureRaw - 0x0D
            + 0x400C00, "6A04516A4F50C745EC02000000FFD7")
        && client.Matches(Red5LoggingBridgeNativeLayout.ClientApiWinHttpSessionSecureProtocolsRaw - 0x11
            + 0x400C00, "6A048D45ECC745EC80000000506A54FF36FFD7")
        && Red5LoggingBridgeNativeLayout.ClientApiWinHttpEnableFeatureOption == 79
        && Red5LoggingBridgeNativeLayout.ClientApiWinHttpSslRevertImpersonationFeature == 2
        && Red5LoggingBridgeNativeLayout.ClientApiWinHttpSecureProtocolsOption == 84
        && Red5LoggingBridgeNativeLayout.ClientApiWinHttpTls10Protocol == 0x80
        && Red5LoggingBridgeNativeLayout.ClientApiWinHttpOpenAsyncFlag == 0x10000000);
    Check("Original client URL message builder escapes host, path and configured identifier components",
        client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlMessageBuilderRaw + 0xFD + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlComponentEscapeRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlMessageBuilderRaw + 0x114 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlComponentEscapeRaw + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlMessageBuilderRaw + 0x138 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlComponentEscapeRaw + 0x400C00)
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaUrlFilterUrlMessageFormatVa)
            == "Red5 %s %s %s");
    Check("Original client URL dispatch reset clears helper fields",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw + 0x48,
            "C7460C00000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw + 0x5A,
            "C7461000000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw + 0x6D,
            "C7460800000000"));
    Check("Original client URL dispatch reset clears its first buffer",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw + 0x9F,
            "C7464407000000C7464000000000"));
    Check("Original client URL dispatch reset clears its second buffer",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw + 0xC1,
            "C7462C07000000C7462800000000"));
    Check("Original client URL dispatch reset writes final primary vtable",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw + 0xE8,
            "C706E447C701")
        && Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectFinalPrimaryVtableVa == 0x1C747E4);
    Check("Original client URL dispatch reset calls member helper",
        client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw
                + 0x8A + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetListHelperRaw + 0x400C00));
    Check("Original client URL dispatch reset frees both buffers through shared helper",
        client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw
                + 0x98 + 0x400C00,
            0x12C8D60 + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw
                + 0xBA + 0x400C00,
            0x12C8D60 + 0x400C00));
    Check("Original client URL dispatch reset initializes member and secondary vtable",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw + 0xDA,
            "FF15704EAC01")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetRaw + 0xE5,
            "894604"));
    Check("Original client URL dispatch reset drains its linked-list member",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetListHelperRaw + 0x0F,
            "C7470800000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetListHelperRaw + 0x16,
            "C7470C00000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetListHelperRaw + 0x1D,
            "C7471000000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetListHelperRaw + 0x28,
            "89008B07894004C7470400000000")
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetListHelperRaw
                + 0x0A + 0x400C00,
            0x12C8D60 + 0x400C00)
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetListHelperRaw
                + 0x42 + 0x400C00,
            0x12C8D60 + 0x400C00)
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectResetListHelperRaw + 0x51,
            "E98AAC6000"));
    Check("Original client URL dispatch constructor allocates sentinel and seeds vector",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListConstructorRaw + 0x2B,
            "6A006A00")
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListConstructorRaw
                + 0x3C + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListSentinelFactoryRaw + 0x400C00)
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListConstructorRaw + 0x41,
            "8906")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListConstructorRaw + 0x77,
            "6A10")
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListConstructorRaw
                + 0x81 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListVectorInsertRaw + 0x400C00)
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListConstructorRaw + 0x86,
            "C7461407000000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListConstructorRaw + 0x8F,
            "C7461808000000"));
    Check("Original client URL dispatch sentinel factory allocates and self-links nodes",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListSentinelFactoryRaw + 0x03,
            "6A14")
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListSentinelFactoryRaw
                + 0x05 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListNodeAllocateRaw + 0x400C00)
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListSentinelFactoryRaw + 0x1E,
            "8BC88BD0EB03")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaUrlFilterDispatchObjectListSentinelFactoryRaw + 0x27,
            "89088D480485C974028911"));
    Check("Original client LogWebRequests stores Lua truthiness in its frame flag",
        client.Ascii(Red5LoggingBridgeNativeLayout.LuaLogWebRequestsUsageVa)
            == "usage: FRAME:LogWebRequests(boolean)"
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaLogWebRequestsRaw + 0x7F, "888758040000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaLogWebRequestsRaw + 0x92, "888758040000"));
    Check("Original client ClearCookies binding calls virtual slot +0x68; its usage string is mismatched",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaClearCookiesRaw + 0x55, "FF5068")
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaWebFrameInitializerRaw + 0x400C00 + 0x177,
            Red5LoggingBridgeNativeLayout.LuaWebFrameInternalConstructorRaw + 0x400C00)
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaWebFrameInitializerRaw + 0x18E,
            "89861C040000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaWebFrameInternalConstructorRaw + 0x74,
            "C707FC65C701")
        && client.Matches(Red5LoggingBridgeNativeLayout.LuaClearCookiesVtableSlotVa, "10661601")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaClearCookiesForwardingThunkRaw,
            "8B491C8B01FF6004")
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaClearCookiesUsageVa)
            == "usage: FRAME:LogWebRequests()");
    Check("Original client LoadUrl forwards its URL through the frame object's virtual slot +0x5C",
        ClientMatches(0xCCAC38, "8B8F1C040000508B11FF525C")
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaLoadUrlInvalidElementVa)
            == "Invalid Element - Element Destroyed?");
    Check("Original client GetUrl returns the stored URL or an empty-string fallback",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaGetUrlRaw + 0x2F, "8B874804000085C0BF68EAC5010F45F8")
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaGetUrlEmptyFallbackVa) == ""
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaGetUrlInvalidElementVa)
            == "Invalid Element - Element Destroyed?");
    Check("Original client SetNaturalDims updates both float fields and notifies only on change",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaSetNaturalDimsRaw + 0xE4, "F30F11975C040000")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaSetNaturalDimsRaw + 0xEC, "F30F118F60040000")
        && client.Calls(Red5LoggingBridgeNativeLayout.LuaSetNaturalDimsRaw + 0xF4 + 0x400C00,
            0xCCC5C0 + 0x400C00));
    Check("Original client GetNaturalDims returns the width and height fields as Lua numbers",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaGetNaturalDimsRaw + 0x4F, "F30F108760040000")
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaGetNaturalDimsInvalidElementVa)
            == "Invalid Element - Element Destroyed?");
    Check("Original client routes LoadUrl through its WebFrame vtable and handles local URLs specially",
        client.Matches(Red5LoggingBridgeNativeLayout.LuaWebFrameLoadUrlVtableSlotVa, "90621601")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaLoadUrlImplementationRaw + 0x9E,
            "FF159448AC01")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaLoadUrlImplementationRaw + 0x259,
            "FF5264")
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaLoadUrlImplementationRaw + 0x4D,
            "FF5074")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.LuaUrlFilterStringCompareImportVa, "strncmp")
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaLocalUrlPrefixVa) == "local://"
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaLocalUrlSeparatorVa) == "/");
    Check("Original client natural-dimension helper computes a clamped scale percentage",
        client.Matches(0x1C81BE8, "0000803F")
        && client.Matches(0x1C81A18, "CDCC4C3E")
        && client.Matches(0x1C81F20, "0000A040")
        && client.Matches(0x1C820AC, "0000C842")
        && client.Matches(0x1C81AD4, "0000003F")
        && ClientMatches(0xCCC67E, "FF5038")
        && ClientMatches(0xCCC68E, "FF503C"));
    Check("Original client EnableTypeTarget uses Lua truthiness and updates its target flag",
        client.Calls(Red5LoggingBridgeNativeLayout.LuaEnableTypeTargetRaw + 0x85 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaEnableTypeTargetHelperRaw + 0x400C00)
        && ClientMatches(Red5LoggingBridgeNativeLayout.LuaEnableTypeTargetHelperRaw + 0x15,
            "889159040000")
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaEnableTypeTargetUsageVa)
            == "usage: FRAME:EnableTypeTarget(boolean)"
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaEnableTypeTargetInvalidElementVa)
            == "Invalid Element - Element Destroyed?");
    Check("Original client IsTypeTarget reads the stored flag as a boolean",
        ClientMatches(Red5LoggingBridgeNativeLayout.LuaIsTypeTargetRaw + 0x52, "388F59040000")
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaIsTypeTargetUsageVa)
            == "usage: FRAME:IsTypeTarget(boolean)"
        && client.Ascii(Red5LoggingBridgeNativeLayout.LuaIsTypeTargetInvalidElementVa)
            == "Invalid Element - Element Destroyed?");
    Check("Original client errorlog callback validates its object ID, method name, and first string argument",
        ClientMatches(0xD65AF9, "FF15004FAC0139450C0F8515020000")
        && ClientMatches(0xD65B02, "0F8515020000")
        && client.Matches(Red5LoggingBridgeNativeLayout.CallbackHandlerVtableVa, "60661601")
        && ClientMatches(0xD65B15, "8D8E74040000E8207BF6FF")
        && ClientMatches(0xD65B24, "3B86740400000F840F010000")
        && ClientMatches(0xD65B33, "0F8406010000")
        && client.Calls(0xD65B1B + 0x400C00,
            Red5LoggingBridgeNativeLayout.CallbackMethodLookupRaw + 0x400C00)
        && ClientMatches(0xD65BA9, "8BCFFF15244EAC0185C0746E48")
        && ClientMatches(0xD65BC5, "FF151C4EAC01")
        && client.Calls(0xD65BDF + 0x400C00,
            Red5LoggingBridgeNativeLayout.CallbackCallablePredicateRaw + 0x400C00)
        && client.ImportSlotNames(0x1AC4EDC,
            "?Invoke@JSObject@Awesomium@@QAE?AVJSValue@2@ABVWebString@2@ABVJSArray@2@@Z")
        && ClientMatches(0xD65BF5, "8D4E04FF15D44EAC01")
        && ClientMatches(0xD65C02, "FF15044FAC01")
        && ClientMatches(0xD65C08, "8D4E08FF15084FAC01")
        && client.ImportSlotNames(0x1AC4F08, "?Pop@JSArray@Awesomium@@QAEXXZ")
        && client.Calls(0xD65C19 + 0x400C00,
            Red5LoggingBridgeNativeLayout.CallbackQueueAppendRaw + 0x400C00)
        && ClientMatches(0xD65C3F, "BA9449BD018D4C2414E8035BF5FF")
        && ClientMatches(0xD65C57, "FF15A44EAC01")
        && ClientMatches(0xD65C78, "FF15244EAC0185C0")
        && ClientMatches(0xD65C7E, "85C00F84C9000000")
        && ClientMatches(0xD65C94, "FF154C4FAC01")
        && ClientMatches(0xD65C9A, "84C00F84AD000000")
        && ClientMatches(0xD65CAF, "FF15544FAC01"));
    Check("Original client forwards errorlog text to the logger with the observed format and logger object",
        ClientMatches(0xD65CF2, "688886E601E8F46759FF")
        && ClientMatches(0xD65D42, "686886E601E8146F59FF")
        && client.Calls(0xD65CF7 + 0x400C00,
            Red5LoggingBridgeNativeLayout.CallbackLogHelperRaw + 0x400C00)
        && client.Calls(0xD65D47 + 0x400C00,
            Red5LoggingBridgeNativeLayout.CallbackUnknownLogHelperRaw + 0x400C00)
        && client.Matches(Red5LoggingBridgeNativeLayout.CallbackErrorLogMethodNameVa,
            "6500720072006F0072006C006F0067000000")
        && client.Ascii(Red5LoggingBridgeNativeLayout.CallbackMessageFormatVa) == "%s - %s"
        && client.Ascii(Red5LoggingBridgeNativeLayout.CallbackUnknownFormatVa)
            == "uiWebFrame='%s', unknown callback was used by Javascript: %s"
        && client.Matches(Red5LoggingBridgeNativeLayout.CallbackMethodCallNameVa,
            "630061006C006C000000")
        && client.Ascii(Red5LoggingBridgeNativeLayout.CallbackCallablePredicateVa)
            == "(function (object) { return !!(object && object.constructor && object.call && object.apply); })");
    Check("Original client queue consumer is uiWebFrameInternal_17 vtable slot 20",
        client.Matches(Red5LoggingBridgeNativeLayout.CallbackQueueConsumerRttiLocatorPointerVa,
            "E00FCD01")
        && client.Matches(Red5LoggingBridgeNativeLayout.CallbackQueueConsumerVtableSlotVa, "E0561601")
        && client.Ascii(Red5LoggingBridgeNativeLayout.CallbackQueueConsumerClassDescriptorVa)
            == ".?AVuiWebFrameInternal_17@@");
    Check("Original client drains queued callbacks and re-resolves each retained method-name key",
        ClientMatches(Red5LoggingBridgeNativeLayout.CallbackQueueConsumerRaw + 0x46,
            "83785C000F8453060000")
        && ClientMatches(0xD64BA4,
            "8D8B74040000E8C190F6FF")
        && client.Calls(0xD64BAA + 0x400C00,
            0xCCDC70 + 0x400C00));
    Check("Original client invokes queued callback through call using synchronous or asynchronous import",
        ClientMatches(0xD64EBE, "FF15E84EAC01")
        && ClientMatches(0xD64EC4, "84C00F85DD010000")
        && ClientMatches(0xD64F6C, "BAB447BD018D4C2430E8D667F5FF")
        && ClientMatches(0xD64FB8, "FF15EC4EAC01")
        && ClientMatches(0xD64FD0, "FF15DC4EAC01")
        && ClientMatches(0xD64F82, "A1C486E601")
        && ClientMatches(0xD64F9E, "83780800")
        && ClientMatches(0xD64FAE, "7413")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.CallbackIsUndefinedImportVa,
            "?IsUndefined@JSValue@Awesomium@@QBE_NXZ")
        && client.ImportSlotNames(0x1AC4EC8,
            "?ToObject@JSValue@Awesomium@@QAEAAVJSObject@2@XZ")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.CallbackInvokeAsyncImportVa,
            "?InvokeAsync@JSObject@Awesomium@@QAEXABVWebString@2@ABVJSArray@2@@Z")
        && client.ImportSlotNames(Red5LoggingBridgeNativeLayout.CallbackInvokeImportVa,
            "?Invoke@JSObject@Awesomium@@QAE?AVJSValue@2@ABVWebString@2@ABVJSArray@2@@Z")
        && client.Matches(Red5LoggingBridgeNativeLayout.CallbackQueueCallMethodNameVa,
            "630061006C006C000000")
        && client.Ascii(Red5LoggingBridgeNativeLayout.CallbackQueueLogFormatVa)
            == "Executing callback '%s' from JavaScript"
        && client.Ascii(Red5LoggingBridgeNativeLayout.CallbackQueueArgumentLogFormatVa)
            == "   #%d: %s"
        && client.Ascii(Red5LoggingBridgeNativeLayout.CallbackQueueReturnLogFormatVa)
            == "Javascript callback returned: %s");
    Check("Original client converts JavaScript arguments and dispatches the registered Lua callback before its optional continuation",
        client.Calls(0xD64C90 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaCallbackArgumentConversionRaw + 0x400C00)
        && ClientMatches(0xD64E0E, "BA40950201")
        && client.Calls(0xD64E52 + 0x400C00,
            Red5LoggingBridgeNativeLayout.LuaCallbackInvocationWrapperRaw + 0x400C00));
    Check("Original client bridge cleanup runs from its scalar-deleting destructor",
        client.Calls(Red5LoggingBridgeNativeLayout.CallbackBridgeDeletingDestructorRaw + 6 + 0x400C00,
            Red5LoggingBridgeNativeLayout.CallbackBridgeDestructorRaw + 0x400C00)
        && ClientMatches(0xD6418B, "F64508017407")
        && client.Calls(0xD642A3 + 0x400C00,
            Red5LoggingBridgeNativeLayout.CallbackBridgeCleanupRaw + 0x400C00));
    Check("Original client cleanup drains queued records and releases record JSArray and JSValue fields",
        ClientMatches(0xD6475B, "C74324FFFFFFFF")
        && ClientMatches(0xD64762, "A1B802E0013B88D0050000")
        && ClientMatches(0xD6476F, "85C974068B016A01FF10C7430400000000")
        && ClientMatches(0xD64780, "837B5C007457")
        && client.Calls(0xD647CB + 0x400C00,
            Red5LoggingBridgeNativeLayout.CallbackQueuedRecordCleanupRaw + 0x400C00)
        && client.Calls(0xD647D2 + 0x400C00, 0x12C8D60 + 0x400C00)
        && ClientMatches(0xD648C9, "8D4E08C745FC01000000FF154C4EAC01")
        && ClientMatches(0xD648D9, "8D4E04C645FC00FF15604EAC01")
        && client.ImportSlotNames(0x1AC4E4C, "??1JSArray@Awesomium@@QAE@XZ")
        && client.ImportSlotNames(0x1AC4E60, "??1JSValue@Awesomium@@QAE@XZ"));
    Check("RedHanded message decoder admits500/501/502 with four parameters and length at most1024",
        red.Matches(0x40890B, "05002BD1FE83F8020F87CD040000")
        && red.Matches(0x408919, "837E10047324")
        && red.Matches(0x408943, "8B7E1C81FF000400007624")
        && red.Matches(0x4089A5, "FF7620")
        && red.Matches(DebugExceptionHandoffNativeLayout.MessageReadCallVa, "FF15BC405400")
        && red.Ascii(0x545BA4) == DebugExceptionHandoffNativeLayout.NoMessageMarker
        && red.Ascii(0x545CC4) == DebugExceptionHandoffNativeLayout.TooLongMessageMarker
        && red.Ascii(0x54629C) == DebugExceptionHandoffNativeLayout.BadMemoryMessageMarker);
    Check("Code502 snapshot path sets owner state1 and joins metadata handling rather than fatal state3",
        red.Matches(0x409911, "3D02D52E017554")
        && red.Matches(0x409924, "83F8020F85EF000000")
        && red.Calls(0x40992D, 0x409740)
        && red.Matches(0x409932, "C7470401000000")
        && red.Matches(0x409941, "E993000000"));
    Check("Dump throttle uses unsigned tick subtraction and strict greater-than2000ms",
        red.Matches(0x409853, "837E04007504B001")
        && red.ImportSlotNames(0x5448E8, "timeGetTime")
        && red.Matches(0x40985D, "FF15E84854002B06B9D00700003BC85E1BC0F7D8")
        && red.Matches(0x4099B1, "FF15E84854002B073DD00700000F97C3"));
    Check("Fatal path sets state3 and refreshes tick before possibly skipping metadata",
        red.Matches(0x4099C1, "C7470403000000FF15E8485400890784DB7448")
        && red.Matches(0x409A1C, "837F0403")
        && red.ImportSlotNames(0x5440D4, "TerminateProcess"));
    Check("RIP_EVENT captures only in state0/2 with null exception, then terminates regardless",
        red.Matches(0x409A47, "8B470483C40C85C0740583F8027544")
        && red.Ascii(0x546534) == "RIP error=%u type=%u"
        && red.Matches(0x409A7B, "6A00")
        && red.Calls(DebugExceptionHandoffNativeLayout.RipMetadataCallVa, 0x407630)
        && red.Matches(0x409A9A, "8B473C6AFFFF30")
        && red.Matches(DebugExceptionHandoffNativeLayout.RipTerminateCallVa, "FF15D4405400"));
    Check("Client LOGPATH producer gates on debugger presence and emits formatted console-log path",
        client.Ascii(0x1C155F4) == DebugStringHandoffNativeLayout.ClientLogPathFormat
        && client.Ascii(0x1C1556C) == "console.log"
        && client.ImportSlotNames(DebugStringHandoffNativeLayout.ClientOutputDebugStringImportVa,
            "OutputDebugStringA")
        && ClientMatches(0x12BBD1F, "FF15C043AC0185C0743A")
        && ClientMatches(0x12BBD3E, "68F455C101")
        && ClientMatches(0x12BBD55, "FF154C43AC01"));
    Check("Client DUMPAUTH producer explicitly prefixes its opaque value with exclamation",
        client.Ascii(0x1B61C14) == DebugStringHandoffNativeLayout.ClientDumpAuthFormat
        && ClientMatches(0x63B134, "FF15C043AC0185C0742C")
        && ClientMatches(0x63B143, "68141CB601")
        && ClientMatches(0x63B15B, "FF154C43AC01"));
    Check("RedHanded reads only ANSI debug strings with reported length above five",
        red.Matches(0x409ACB, "66837DA00075D5")
        && red.Matches(0x409AD2, "668B45A26683F80576CB")
        && red.Matches(0x409ADC, "0FB7F08D4E01")
        && red.Matches(DebugStringHandoffNativeLayout.RemoteReadCallVa, "FF15BC405400"));
    Check("Debug-string trimming removes trailing NUL CR and LF then terminates local buffer",
        red.Matches(0x409B07, "8A4433FF84C074083C0D74043C0A75034E75ED")
        && red.Matches(0x409B1B, "C6043300"));
    Check("Only RED5-prefixed fields update owner values using exact suffix offsets",
        red.Ascii(0x54654C) == DebugStringHandoffNativeLayout.Prefix
        && red.Ascii(0x546554) == DebugStringHandoffNativeLayout.LogPathField
        && red.Ascii(0x546570) == DebugStringHandoffNativeLayout.DumpAuthField
        && red.ImportSlotNames(0x5447AC, "strncmp")
        && red.Matches(0x409B24, "6A05684C655400")
        && red.Matches(0x409B55, "8D430D508D4F0C")
        && red.Calls(DebugStringHandoffNativeLayout.LogPathAssignCallVa, 0x406420)
        && red.Matches(0x409B8B, "8D430E508D4F10")
        && red.Calls(DebugStringHandoffNativeLayout.DumpAuthAssignCallVa, 0x406420));
    Check("RedHanded forwards owner auth into explicit DumpTruck auth option without a command shell",
        red.Ascii(0x5452E4) == "--auth="
        && red.Matches(0x404BFD, "89542450")
        && red.Matches(0x404C90, "FF74247468E4525400")
        && red.Calls(0x404CAC, 0x4042D0)
        && red.Matches(0x40437F, "6A08")
        && red.Matches(0x404391, "FF15E8425400")
        && red.ImportSlotNames(0x5442E8, "CreateProcessA"));
    Check("DumpTruck parses auth option into owner+8 and requires forge plus empty auth for stored fallback",
        truck.Ascii(0x475D60) == "auth"
        && truck.Matches(0x40540F, "8D4908")
        && truck.Calls(0x405412, 0x408710)
        && truck.Matches(0x405479, "807E1400744A8D4E08")
        && truck.Calls(0x405482, 0x409240)
        && truck.Matches(0x405487, "84C0743E")
        && truck.Calls(0x405491, AuthHandoffNativeLayout.StoredCredentialFallbackVa));
    Check("Submit fallback reads registry login only when auth is empty and adds literal exclamation",
        truck.Calls(0x403B8B, 0x409240)
        && truck.Matches(0x403B90, "84C00F8483000000")
        && truck.Calls(0x403B9C, AuthHandoffNativeLayout.RegistryLoginReaderVa)
        && truck.Ascii(0x475C04) == AuthHandoffNativeLayout.RegistryLoginMarkerFormat
        && truck.Matches(0x403BCC, "68045C4700")
        && truck.Calls(0x403BD2, 0x410F60));
    Check("Request constructor XML-escapes auth before copying it into its distinct owner+8",
        truck.Calls(0x403E1C, AuthHandoffNativeLayout.RequestContextConstructorVa)
        && truck.Matches(0x409942, "8B55108D4D0C")
        && truck.Calls(0x40995A, AuthHandoffNativeLayout.XmlTextEscapeVa)
        && truck.Calls(0x40996B, 0x4100D0));
    Check("Auth XML substitution order is ampersand, less-than, greater-than",
        truck.Ascii(0x475D88) == "&amp;" && truck.Ascii(0x475DC0) == "&"
        && truck.Ascii(0x475DC4) == "&lt;" && truck.Ascii(0x475DCC) == "<"
        && truck.Ascii(0x475DD0) == "&gt;" && truck.Ascii(0x475DD8) == ">"
        && truck.Matches(0x4094E4, "68885D4700")
        && truck.Matches(0x4094F2, "68C05D4700")
        && truck.Calls(0x4094FE, 0x410BB0)
        && truck.Matches(0x409504, "68C45D470068CC5D4700")
        && truck.Calls(0x409510, 0x410BB0)
        && truck.Matches(0x409516, "68D05D470068D85D4700")
        && truck.Calls(0x409522, 0x410BB0));
    Check("Request XML formatter receives preescaped auth from context+8 without removing marker",
        truck.Matches(0x409B78, "8B430885C00F45F8")
        && truck.Ascii(0x475E54) == AuthHandoffNativeLayout.NativeReportAuthFormat
        && truck.Ascii(0x475E84) == "application/xml"
        && truck.Matches(0x409B80, "5768545E470068845E470056")
        && truck.Calls(0x409B8C, AuthHandoffNativeLayout.RequestAuthFormatterVa));
    Check("PDH identifies the process before reading a LARGE Virtual Bytes counter",
        red.Ascii(0x545468) == CrashMetadataNativeLayout.VirtualBytesCounterPattern
        && red.Matches(0x406CFD, "6868545400")
        && red.Ascii(0x545484) == CrashMetadataNativeLayout.ProcessIdCounterName
        && red.Matches(0x406E84, "6884545400")
        && red.ImportSlotNames(0x54497C, "PdhGetFormattedCounterValue")
        && red.Matches(0x406F1A, "8B1D7C495400")
        && red.Matches(0x406F36, "6800010000")
        && red.Matches(0x406F48, "398560F7FFFF7411")
        && red.Matches(0x406F6A, "6800040000FF760CFFD3")
        && red.Matches(0x406F7C, "8B9D50F7FFFF8BB554F7FFFF")
        && red.Matches(0x406FD5, "8BC38BD6"));
    Check("Zero in both preferred-result halves selects unchecked DWORD PagefileUsage",
        red.Calls(CrashMetadataNativeLayout.ProcessVirtualBytesCallVa,
            CrashMetadataNativeLayout.ProcessVirtualBytesVa)
        && red.Matches(0x407A3B, "85C9755185C0754D")
        && red.Matches(0x407A60, "C78534FDFFFF28000000")
        && red.Matches(CrashMetadataNativeLayout.ProcessMemoryInfoCallVa, "FF15F8475400")
        && red.ImportSlotNames(0x5447F8, "GetProcessMemoryInfo")
        && red.Matches(0x407A7A, "8B8554FDFFFF8985BCFCFFFFC785E8FCFFFF00000000")
        && red.Matches(0x407EF6, "FFB5E8FCFFFF")
        && red.Matches(0x407F02, "FFB5BCFCFFFF"));
    Check("Global memory ceiling compares full unsigned QWORDs and uses zero on API failure",
        red.ImportSlotNames(0x5440B8, "GlobalMemoryStatusEx")
        && red.Matches(0x4079AE, "FF15B840540085C07450")
        && red.Matches(0x4079B8, "8B8510FDFFFF3B8520FDFFFF7729720E")
        && red.Matches(0x4079C8, "8B850CFDFFFF3B851CFDFFFF7319")
        && red.Matches(0x407988, "660F1385DCFCFFFF")
        && red.Matches(0x407A08, "8B85E0FCFFFF8985B8FCFFFF8B85DCFCFFFF8985B4FCFFFF"));
    Check("r5dumper reads 512-byte chunks, appends as a C string, and extracts CRLF lines",
        dumper.Matches(0x401320, "6800020000")
        && dumper.ImportSlotNames(0x404008, "ReadFile")
        && dumper.Matches(0x40133D, "C644041000")
        && dumper.Ascii(UserStreamNativeLayout.CrLfLiteralVa) == "\r\n"
        && dumper.Matches(0x4013A9, "68DC41400053")
        && dumper.Matches(0x4014C1, "837C247C00")
        && dumper.Matches(0x4014C6, "0F84C2030000"));
    Check("Native tokenizer skips repeated separators and preserves the entire third-field remainder",
        dumper.Matches(0x40115F, "6A7C")
        && dumper.Matches(0x4011C4, "6A7C")
        && dumper.Matches(0x4011F6, "83F8027585")
        && dumper.Matches(0x401208, "8B085156")
        && dumper.Matches(0x401213, "FF1568404000"));
    Check("r5dumper copies the original stream type into twelve-byte descriptors",
        dumper.Matches(0x401AF5, "8B46048947FC")
        && dumper.Matches(0x401B03, "894704")
        && dumper.Matches(0x401B0F, "890783C70C")
        && dumper.Matches(0x402180, "B804000000C3")
        && dumper.Matches(0x4021A0, "B808000000C3")
        && dumper.Matches(0x401270, "8B411C40C3"));
    Check("r5dumper passes user streams but null exception and callback information",
        dumper.Matches(0x401B6A, "6A008D4C240C51")
        && dumper.Matches(0x401B75, "6A00")
        && dumper.Matches(0x401B8E, "FF15D0644000"));
    Check("Bundled dbghelp actually exports the named writer, ordinal31, without forwarding",
        dbghelp.Export(UserStreamNativeLayout.DbgHelpWriteExport)
            == (UserStreamNativeLayout.DbgHelpWriteExportRva,
                (uint)UserStreamNativeLayout.DbgHelpWriteExportOrdinal, false));
    Check("DumpTruck redirects HWStats stdout through a separate 2048-byte pipe",
        truck.Matches(0x42ABE2, "C785A8F7FFFF00010000")
        && truck.Matches(0x42AC0A, "6800080000")
        && truck.Matches(0x42AC50, "FF1518514700")
        && truck.ImportSlotNames(0x475118, "CreatePipe")
        && truck.Matches(0x42AD33, "6800080000")
        && truck.Matches(0x42AD60, "FF5018")
        && truck.Matches(0x42ADC7, "6A0168F16B4700")
        && truck.Matches(0x42ADCE, "FF5018"));
    Check("DumpTruck appends a NUL to its output sink rather than treating hardware text as dump metadata",
        truck.Ascii(0x476BF1) == ""
        && DumpTruckLayout.HwReportReadChunkByteCount == 0x800
        && DumpTruckLayout.HwReportSinkVtableSlotOffset == 0x18);
    Check("HWStats stdout accumulates in slBlob before compression",
        truck.Matches(0x40AC1A, "C7458044DF4B00")
        && truck.Word(HardwareReportNativeLayout.BlobVtableVa
            + DumpTruckLayout.HwReportSinkVtableSlotOffset) == HardwareReportNativeLayout.BlobAppendVa
        && truck.Calls(HardwareReportNativeLayout.RunGeneratorCallVa, DumpTruckLayout.HwReportRunnerVa)
        && truck.Matches(0x40AC63, "8B458485C074088B500C8B4804")
        && truck.Calls(HardwareReportNativeLayout.CompressCallVa, HardwareReportNativeLayout.CompressVa));
    Check("slBlob append grows by the supplied byte count and copies exactly that count",
        truck.Matches(0x41BAEA, "FF500C")
        && truck.Matches(0x41BAF2, "6A018BCE8D141F52")
        && truck.Calls(0x41BAFA, 0x41BD80)
        && truck.Matches(0x41BB09, "53FF750803C750")
        && truck.Calls(0x41BB10, 0x46FCEE));
    Check("Nonzero HWStats exit omits hardware, while compression failure aborts packaging",
        truck.Matches(0x40AC46, "85C00F85AD000000")
        && truck.Matches(0x40AC85, "84C07564")
        && truck.Calls(0x40AD0A, 0x40C940)
        && truck.Matches(0x40ACE8, "E9E6FBFFFF"));
    Check("Hardware compression allocates16KiB but advertises four output bytes per finish call",
        truck.Matches(0x40958B, "B900400000")
        && truck.Matches(0x4095E0, "BA04000000C745A804000000")
        && truck.Calls(0x4095F2, HardwareReportNativeLayout.DeflateVa)
        && truck.Matches(0x409603, "B9040000002BC8")
        && truck.Calls(0x409610, 0x40DA40));
    Check("Hardware compression accepts only zlib0/1 and repeats only on full output",
        truck.Matches(0x4095F7, "85C0784383F8017F3E")
        && truck.Matches(0x409618, "85C074C4")
        && truck.Calls(0x40961F, HardwareReportNativeLayout.DeflateEndVa)
        && truck.Calls(0x409641, HardwareReportNativeLayout.DeflateEndVa)
        && truck.Ascii(0x475DDC) == "1.2.8"
        && truck.Calls(0x40957B, HardwareReportNativeLayout.DeflateInitVa));
    Check("Successful hardware report begins layer3 and report termination writes DWORD0",
        truck.Matches(0x40AC4E, "6A03")
        && truck.Calls(0x40AC5A, HardwareReportNativeLayout.LayerBeginVa)
        && truck.Calls(0x40ACF4, HardwareReportNativeLayout.LayerEndVa)
        && truck.Matches(0x40AD15, "C745AC00000000")
        && truck.Calls(0x40AD23, 0x40C8B0));
    Check("Report layers store DWORD type and length, then patch payload-only length at close",
        truck.Calls(0x426CE4, 0x40C8B0)
        && truck.Matches(0x426CEF, "C74508FFFFFFFF")
        && truck.Calls(0x426CF6, 0x40C980)
        && truck.Matches(0x40C8F0, "8B008901")
        && truck.Matches(0x40C9C0, "8B008901")
        && truck.Matches(0x426D3C, "8BF02B7704")
        && truck.Matches(0x426D90, "8B0F8B470483E804508B11FF52085F8930"));
    Check("Report starts with native little-endian DEFEC8ED magic in a fresh request blob",
        truck.Word(CrashReportNativeLayout.MagicLiteralVa) == CrashReportNativeLayout.Magic
        && truck.Matches(0x40A863, "6814634700")
        && truck.Calls(CrashReportNativeLayout.MagicWriteCallVa, 0x40C980)
        && truck.Matches(0x40D91B, "C7462C44DF4B00")
        && truck.Matches(0x40D922, "C7463000000000C7463400000000C7463800000000")
        && truck.Matches(0x40A854, "8D472C")
        && truck.Calls(0x40A85E, 0x425DA0)
        && truck.Matches(0x425E0A, "C7410800000000C7410C00000000"));
    Check("Producer assigns layer1/core, layer2/log, layer5/browser log in report order",
        truck.Matches(0x40A8FA, "6A01")
        && truck.Calls(0x40A909, HardwareReportNativeLayout.LayerBeginVa)
        && truck.Calls(0x40A91B, CrashReportNativeLayout.FileCompressorVa)
        && truck.Matches(0x40AA99, "6A02")
        && truck.Calls(0x40AAA3, CrashReportNativeLayout.OptionalLogWriterVa)
        && truck.Matches(0x40AB7F, "6A05")
        && truck.Calls(0x40AB89, CrashReportNativeLayout.OptionalLogWriterVa));
    Check("Application-name layer4 copies stored text plus NUL without compression",
        truck.Matches(0x40A9A1, "C7459C04000000")
        && truck.Calls(0x40A9B5, 0x40C8B0)
        && truck.Matches(0x40AA08, "8B460C")
        && truck.Matches(0x40AA23, "408D8D4CFFFFFF50")
        && truck.Calls(0x40AA2E, 0x426DB0)
        && truck.Matches(0x40AA33, "8B4E0C")
        && truck.Calls(0x40AA42, 0x46FCEE)
        && truck.Calls(0x40AA51, HardwareReportNativeLayout.LayerEndVa));
    Check("App option initializes owner+0C while msg initializes owner+10",
        truck.Matches(0x40541C, "B9685D4700")
        && truck.Ascii(0x475D68) == "app"
        && truck.Matches(0x40542D, "8D490C")
        && truck.Calls(0x405430, 0x408710)
        && truck.Matches(0x40543A, "B96C5D4700")
        && truck.Ascii(0x475D6C) == "msg"
        && truck.Matches(0x40544B, "8D4910")
        && truck.Calls(0x40544E, 0x408710));
    Check("File deflate requests four input bytes and advertises four output bytes",
        truck.Matches(0x409757, "6A046A01")
        && truck.Matches(0x40975F, "FF15A4554700")
        && truck.ImportSlotNames(0x4755A4, "fread")
        && truck.Matches(0x409781, "C745A404000000")
        && truck.Calls(0x40979C, HardwareReportNativeLayout.DeflateVa)
        && truck.Matches(0x409793, "F7D81BC083E0048BD0"));
    Check("File deflate rejects only -2 and uses avail_out plus EOF for continuation",
        truck.Matches(0x4097A1, "83F8FE7459")
        && truck.Matches(0x4097A9, "B9040000002BC8")
        && truck.Calls(0x4097B7, 0x40DA40)
        && truck.Matches(0x4097BF, "85C074BD")
        && truck.ImportSlotNames(0x47559C, "feof")
        && truck.Matches(0x4097C9, "85C07508")
        && truck.Calls(0x4097D8, HardwareReportNativeLayout.DeflateEndVa));
    Check("Optional log writer uses signed 256KiB threshold and a 2048-byte prefix",
        truck.Matches(0x40A0AD, "FF1594554700")
        && truck.ImportSlotNames(0x475594, "ftell")
        && truck.Matches(0x40A0B9, "3D000004000F8FAC000000")
        && truck.Matches(0x40A18E, "6800000400")
        && truck.Matches(0x40A1B8, "6800080000")
        && truck.Matches(0x40A238, "68485F4700")
        && truck.Ascii(0x475F48) == CrashReportNativeLayout.ClippedLogMarkerFormat
        && truck.Calls(0x40A12B, CrashReportNativeLayout.FileCompressorVa)
        && truck.Calls(0x40A340, HardwareReportNativeLayout.CompressVa));

    var childImports = child.ImportsFrom(FirefallProcessLayout.ImportModule);
    Check("FirefallProcess imports exactly Awesomium ChildProcessMain, which the DLL exports",
        childImports.SequenceEqual(new[] { FirefallProcessLayout.ImportSymbol })
        && child.Matches(0x40103E, "FF15DC604000")
        && child.ImportSlotNames(FirefallProcessLayout.ChildProcessMainIatVa, FirefallProcessLayout.ImportSymbol)
        && browser.Export(FirefallProcessLayout.ImportSymbol) is { } childExport
        && browser.ImageBase + childExport.Rva == AwesomiumLayout.ChildProcessMainVa
        && !childExport.Forwarded);
    var clientImports = client.ImportsFrom("awesomium.dll");
    Check("All86 original client Awesomium imports resolve to exports in the bundled DLL",
        clientImports.Count == AwesomiumLayout.ClientImportCount
        && clientImports.All(symbol => browser.Export(symbol) is not null));
    Check("Browser child C++ export and C wrapper have distinct identities and direct wrapper call",
        browser.Export(ChildProcessRoleNativeLayout.CppEntryExport) is { } cppEntry
        && cppEntry.Rva == ChildProcessRoleNativeLayout.CppEntryRva
        && cppEntry.Ordinal == 217 && !cppEntry.Forwarded
        && browser.Export(ChildProcessRoleNativeLayout.CWrapperExport) is { } cEntry
        && cEntry.Rva == ChildProcessRoleNativeLayout.CWrapperRva
        && cEntry.Ordinal == 339 && !cEntry.Forwarded
        && browser.Matches(0x1004C840, "8B44240450E8C6DEFEFF83C404C20400")
        && browser.Calls(0x1004C845, 0x1003A710)
        && child.ImportSlotNames(0x4060DC, ChildProcessRoleNativeLayout.CppEntryExport)
        && child.Matches(0x40103E, "FF15DC604000"));
    Check("Browser child entry tests parsed type length and returns zero before content main when absent/empty",
        browser.Matches(0x1003A6B1, "837C2430007425")
        && browser.Calls(0x1003A719, 0x1003A630)
        && browser.Matches(0x1003A721, "84C07445")
        && browser.Matches(0x1003A76A, "33C05E83C460C3"));
    for (int index = 0; index < ChildProcessRoleNativeLayout.Roles.Count; index++)
    {
        var role = ChildProcessRoleNativeLayout.Roles[index];
        uint entryVa = ChildProcessRoleNativeLayout.RoleTableVa
            + (uint)(index * ChildProcessRoleNativeLayout.RoleEntryByteCount);
        Check($"Browser role table entry {index} preserves key pointer and callback VA",
            browser.Word(entryVa) == role.KeyVa
            && browser.Ascii(role.KeyVa) == role.Key
            && browser.Word(entryVa + 4) == role.CallbackVa);
    }
    Check("Browser dispatch requires full exact match and invokes matched callback indirectly",
        browser.Calls(0x10064997, 0x1000A5D0)
        && browser.Matches(0x1006499F, "85C0750D3BF772093BF70F95C085C07434")
        && browser.Matches(0x100649F7, "FFD0"));
    Check("Browser unknown role delegates at vtable+10 or returns one without delegate",
        browser.Matches(0x100649C5, "85C9743E")
        && browser.Matches(0x100649D0, "8B118B5210")
        && browser.Matches(0x100649D7, "FFD2")
        && browser.Matches(0x10064A07, "BE01000000")
        && browser.Matches(0x10064A70, "5F8BC65E5D5B81C4B8000000C3"));
    Check("Browser switch splitting initializes value length and bypasses copying when no equals occurs",
        browser.Matches(CommandSwitchValueNativeLayout.ParsedValueLengthInitializationVa, "897510")
        && browser.Matches(CommandSwitchValueNativeLayout.NoSeparatorBranchVa, "83FEFF7444")
        && browser.Matches(CommandSwitchValueNativeLayout.MapValueCopySequenceVa,
            "6AFF53578BC8E8F0A6D7FF"));
    Check("Browser type lookup and recognized wide double-dash/equals delimiters match literals",
        browser.Ascii(CommandSwitchValueNativeLayout.TypeKeyVa) == "type"
        && browser.Matches(CommandSwitchValueNativeLayout.WideDoubleDashVa, "2D002D000000")
        && browser.Matches(CommandSwitchValueNativeLayout.WideEqualsVa, "3D000000"));
    Check("Browser command initialization uses wide Windows command-line/argv entry calls",
        browser.Matches(0x1028E234, "FF15EC042411")
        && browser.Matches(0x1028E19E, "FF1598BC5711"));
    Check("HWStats DriveSpace iterates C through Z and admits only fixed drive type3",
        hardware.Ascii(0x443160) == DriveSpaceNativeLayout.Header
        && hardware.Matches(0x40381C, "B343BF43000000")
        && hardware.Matches(0x403843, "FF156401440083F803")
        && hardware.ImportSlotNames(0x440164, "GetDriveTypeW")
        && hardware.Matches(0x40394B, "80FB5A0F8ECFFEFFFF"));
    Check("HWStats space helper uses caller-available bytes, not total size or total free bytes",
        hardware.ImportSlotNames(0x440168, "GetDiskFreeSpaceExW")
        && hardware.Matches(0x4036E0, "8D45E4508D45F050")
        && hardware.Matches(0x4036E8, "FF1568014400")
        && hardware.Matches(0x403717, "8B55E48BCA8B75E8")
        && hardware.Matches(0x4036F2, "85DB7402890385FF")
        && hardware.Matches(0x403700, "C70700000000"));
    Check("HWStats space arithmetic retains signed low-shift conversion and extra high-part term",
        hardware.Matches(0x40371F, "81E2FFFF0F008BC6C1E914C1E00C0BC8C1EE08")
        && hardware.Matches(0x40374E, "0F5BC9660F5AC00F5BD2")
        && hardware.Matches(0x403758, "F30F59054C634800F30F58C80F28C1F30F590558634800F30F58D0")
        && hardware.Word(0x48634C) == 0x35800000
        && hardware.Word(0x486358) == 0x3A800000
        && hardware.Word(0x486398) == 0x3F800000);
    Check("HWStats space row and total use strict above-one thresholds with preserved formats",
        hardware.Matches(0x403889, "0F2FD0")
        && hardware.Matches(0x403896, "7649")
        && hardware.Matches(0x4038E1, "0F2FC87662")
        && hardware.Matches(0x403954, "0F2FE0764C")
        && hardware.Matches(0x4039A5, "0F2FD8764F")
        && hardware.Ascii(0x44316C) == DriveSpaceNativeLayout.DriveGigabytesFormat
        && hardware.Ascii(0x44317C) == DriveSpaceNativeLayout.DriveMegabytesFormat
        && hardware.Ascii(0x442FEC) == DriveSpaceNativeLayout.TotalGigabytesFormat
        && hardware.Ascii(0x44304C) == DriveSpaceNativeLayout.TotalMegabytesFormat);
    Check("HWStats RAM initializes only status length then reads total physical bytes without checking BOOL",
        hardware.Matches(0x4035B6, "C745B840000000")
        && hardware.Matches(0x4035C0, "FF15740144008B55C08BCA8B75C4")
        && hardware.ImportSlotNames(0x440174, "GlobalMemoryStatusEx")
        && hardware.Matches(0x4035D0, "0FACC114C1E81481F9000400007309"));
    Check("HWStats small RAM path passes raw physical-byte DWORDs to the literal percent-ull format",
        hardware.Ascii(0x443120) == RamAndProcessorNativeLayout.SmallRamFormat
        && hardware.Matches(0x4035DF, "56526820314400EB30")
        && hardware.Ascii(0x443130) == RamAndProcessorNativeLayout.GigabytesRamFormat
        && hardware.Calls(0x403619, 0x406A20));
    Check("HWStats large RAM path restores unsigned conversion before float rounding and binary scaling",
        hardware.Matches(0x4035E8, "660F6EC1F30FE6C0C1E91F")
        && hardware.Matches(0x4035F6, "F20F5804CD00644800")
        && hardware.Matches(0x4035FF, "660F5AC0F30F5905586348000F5AC0")
        && hardware.Word(0x486400) == 0 && hardware.Word(0x486404) == 0
        && hardware.Word(0x486408) == 0 && hardware.Word(0x48640C) == 0x41F00000);
    Check("HWStats affinity initializes both masks and loads the system output without testing API BOOL",
        hardware.Matches(0x403621, "C745B000000000")
        && hardware.Matches(0x40362B, "C745B400000000")
        && hardware.ImportSlotNames(0x440170, "GetProcessAffinityMask")
        && hardware.Matches(0x40363E, "FF15700144008B75B433D2")
        && hardware.Matches(0x403651, "8BC683E00183C8007401470FACD601D1EA")
        && hardware.Ascii(0x443140) == RamAndProcessorNativeLayout.ProcessorCountKey
        && hardware.Calls(0x403670, 0x406AE0));
    foreach (var request in new (int Index, uint BaseVa, uint ValueVa, uint SetupVa, uint CallVa)[]
    {
        (0, 0x443B50, 0x443B2C, 0x404154, 0x40415B),
        (1, 0x443B80, 0x443B6C, 0x404163, 0x40416F),
        (2, 0x443BE0, 0x443BA0, 0x404177, 0x404183),
        (3, 0x443C10, 0x443BFC, 0x40418B, 0x404197),
        (4, 0x443C50, 0x443C30, 0x40419F, 0x4041AB)
    })
    {
        Check($"HWStats BIOS request {request.Index} root/value arguments and writer",
            hardware.Ascii(request.BaseVa) == BiosRegistryNativeLayout.SourceBasePath
            && hardware.Ascii(request.ValueVa) == BiosRegistryNativeLayout.ValueNames[request.Index]
            && hardware.Matches(request.SetupVa, "BA" + Convert.ToHexString(BitConverter.GetBytes(request.BaseVa)))
            && hardware.Matches(request.Index == 0 ? 0x40414F : request.CallVa - 5,
                "68" + Convert.ToHexString(BitConverter.GetBytes(request.ValueVa)))
            && hardware.Calls(request.CallVa, BiosRegistryNativeLayout.ValueWriterVa));
    }
    Check("HWStats BIOS query uses 1024-byte buffer and no middle subkey",
        hardware.Matches(0x403112, "6800040000")
        && hardware.Matches(0x403126, "33D28BC8")
        && hardware.Calls(0x40312A, BiosRegistryNativeLayout.QueryHelperVa));
    Check("HWStats registry copy replaces slashes without case normalization",
        hardware.Matches(0x406EF7, "BB5C000000")
        && hardware.Matches(0x406F00, "803C372F8D76010FB65437FF0F44D38856FF4875EB")
        && hardware.Calls(0x4070BE, BiosRegistryNativeLayout.SlashCopyHelperVa)
        && hardware.Calls(0x4070C8, BiosRegistryNativeLayout.SlashCopyHelperVa)
        && hardware.Calls(0x4070D2, BiosRegistryNativeLayout.SlashCopyHelperVa));
    Check("HWStats BIOS open appends trailing slash and requests HKLM KEY_ALL_ACCESS",
        hardware.Ascii(0x443C2C) == "\\"
        && hardware.Matches(0x40710A, "682C3C4400")
        && hardware.Matches(0x40711D, "683F000F006A00566802000080")
        && hardware.ImportSlotNames(0x440008, "RegOpenKeyExA")
        && hardware.Matches(0x40712A, "FF1508004400"));
    Check("HWStats registry helper queries and closes then collapses error status",
        hardware.ImportSlotNames(0x44000C, "RegQueryValueExA")
        && hardware.ImportSlotNames(0x440004, "RegCloseKey")
        && hardware.Matches(0x407146, "FF150C004400")
        && hardware.Matches(0x407157, "8901FF1504004400")
        && hardware.Matches(0x4071AD, "F7DF1BFF81E7054000808BC7"));
    Check("HWStats BIOS type and helper-result gates precede narrow percent-s output",
        hardware.Matches(0x40313A, "83F801741A83F8077415")
        && hardware.Matches(0x403159, "85F6754B")
        && hardware.Ascii(0x443C7C) == BiosRegistryNativeLayout.NativeFormat
        && hardware.Matches(0x403198, "687C3C4400"));
    Check("HWStats MAC enumerates with zeroed 64-byte NCB and 256-byte destination",
        hardware.ImportSlotNames(0x440584, "Netbios")
        && hardware.Matches(0x4041E6, "6A40")
        && hardware.Matches(0x4041F8, "8B1D84054400")
        && hardware.Matches(0x40420D, "B800010000C685B8FEFFFF37")
        && hardware.Matches(0x404219, "668985C0FEFFFF")
        && hardware.Matches(0x404227, "FFD333F680BDF8FEFFFF00"));
    Check("HWStats MAC resets each enumerated LANA and ignores reset status",
        hardware.Matches(0x404250, "8A8435F9FEFFFF")
        && hardware.Matches(0x40425A, "8885E8FEFFFF")
        && hardware.Matches(0x404266, "C685B8FEFFFF3250FFD36A40"));
    Check("HWStats MAC ASTAT uses same LANA, wildcard name and shared 600-byte buffer",
        hardware.Matches(0x404280, "8A8435F9FEFFFF")
        && hardware.Ascii(0x443C6C) == MacAddressNativeLayout.AdapterStatusCallName
        && hardware.Matches(0x404287, "686C3C4400")
        && hardware.Matches(0x40429B, "C685B8FEFFFF33")
        && hardware.Matches(0x4042AB, "C785BCFEFFFFC0EF4800")
        && hardware.Matches(0x4042B5, "B858020000668985C0FEFFFF"));
    Check("HWStats MAC stops on first zero AL, else advances against enumerated count",
        hardware.Matches(0x4042C8, "FFD384C07423")
        && hardware.Matches(0x4042CE, "0FB685F8FEFFFF463BF00F8C62FFFFFF")
        && hardware.Matches(0x4042DE, "33C05F5E5B")
        && hardware.Matches(0x404325, "B801000000"));
    foreach (var copy in new (uint ReadVa, uint StoreVa, byte Offset)[]
    {
        (0x4042F1, 0x4042F9, 0), (0x4042FD, 0x404302, 1), (0x404305, 0x40430A, 2),
        (0x40430D, 0x404312, 3), (0x404315, 0x40431A, 4), (0x40431D, 0x404322, 5)
    })
        Check($"HWStats MAC copies shared status byte {copy.Offset} into destination",
            hardware.Matches(copy.ReadVa, "A0" + Convert.ToHexString(
                BitConverter.GetBytes(MacAddressNativeLayout.AdapterStatusBufferVa + copy.Offset)))
            && hardware.Matches(copy.StoreVa, copy.Offset == 0 ? "8807" : $"8847{copy.Offset:X2}"));
    Check("HWStats MAC caller ignores helper return and promotes all six unsigned bytes to printf",
        hardware.Calls(0x4043C2, MacAddressNativeLayout.HelperVa)
        && hardware.Matches(0x4043C7, "0FB645F1500FB645F0500FB645EF500FB645EE500FB645ED500FB645EC50")
        && hardware.Ascii(0x443BB4) == MacAddressNativeLayout.NativeFormat
        && hardware.Calls(0x4043EE, 0x406A20));
    Check("HWStats OS builder initializes EX structure size and rejects failed version query",
        hardware.Matches(0x403A51, "681C010000")
        && hardware.Matches(0x403A8E, "C785B8FEFFFF1C010000")
        && hardware.ImportSlotNames(0x440160, "GetVersionExW")
        && hardware.Matches(0x403A9F, "FF156001440085C00F8494040000"));
    Check("HWStats DirectX registry read clears 128-byte buffer and opens HKLM KEY_READ",
        hardware.Matches(0x40404F, "33C06A7E5066898568FFFFFF")
        && hardware.Matches(0x404071, "68190002006A0068983A44006802000080")
        && hardware.Matches(0x443A98, Convert.ToHexString(System.Text.Encoding.Unicode.GetBytes(
            DirectXVersionNativeLayout.Subkey + "\0")))
        && hardware.ImportSlotNames(0x440000, "RegOpenKeyExW")
        && hardware.Matches(0x404082, "FF150000440085C07556"));
    Check("HWStats DirectX Version query uses byte capacity 127 with no type output",
        hardware.Matches(0x404092, "C78560EDFFFF7F000000")
        && hardware.Matches(0x4040A4, "6A006A0068D03A4400")
        && hardware.Matches(0x443AD0, Convert.ToHexString(System.Text.Encoding.Unicode.GetBytes(
            DirectXVersionNativeLayout.ValueName + "\0")))
        && hardware.ImportSlotNames(0x440020, "RegQueryValueExW")
        && hardware.Matches(0x4040B3, "FF15200044008B8560EDFFFF03C03D80000000"));
    Check("HWStats DirectX query result is ignored; doubled count gates terminator and close",
        hardware.Matches(0x4040C6, "0F83FD000000")
        && hardware.Matches(0x4040D2, "33C966898C0568FFFFFFFF1504004400")
        && hardware.Matches(0x443AE0, Convert.ToHexString(System.Text.Encoding.Unicode.GetBytes(
            DirectXVersionNativeLayout.CombinedWideFormat + "\0")))
        && hardware.Calls(0x4041C9, 0x43AFBA)
        && hardware.ImportSlotNames(0x440558, "_swprintf"));
    Check("HWStats OS builder resolves native system info with ordinary system-info fallback",
        hardware.Ascii(0x4430CC) == OsDescriptionNativeLayout.NativeSystemInfoExport
        && hardware.ImportSlotNames(0x440158, "GetModuleHandleW")
        && hardware.ImportSlotNames(0x44017C, "GetProcAddress")
        && hardware.ImportSlotNames(0x44015C, "GetSystemInfo")
        && hardware.Matches(0x403AC4, "85C0740B")
        && hardware.Matches(0x403ACF, "FFD0EB0D")
        && hardware.Matches(0x403ADA, "FF155C014400"));
    Check("HWStats OS builder rejects non-NT platform and unsigned major at most four",
        hardware.Matches(0x403AE0, "83BDC8FEFFFF020F8554040000")
        && hardware.Matches(0x403AED, "83BDBCFEFFFF040F8647040000")
        && hardware.Matches(0x403F41, "32C0"));
    Check("HWStats OS product-info availability guard is exclusive to major six",
        hardware.Matches(0x403B1F, "83BDBCFEFFFF060F85C9000000")
        && hardware.Matches(0x44318C, Convert.ToHexString(System.Text.Encoding.Unicode.GetBytes(
            OsDescriptionNativeLayout.KernelModule + "\0")))
        && hardware.Matches(0x4432A8, Convert.ToHexString(System.Text.Encoding.Unicode.GetBytes(
            OsDescriptionNativeLayout.KernelModule + "\0")))
        && hardware.Ascii(0x4432C4) == OsDescriptionNativeLayout.ProductInfoExport
        && hardware.Matches(0x403B96, "85C00F841C010000")
        && hardware.Matches(0x403BAA, "85C00F8408010000"));
    Check("HWStats OS product code starts zero and API BOOL is ignored with zero service-pack arguments",
        hardware.Matches(0x403A59, "C785B0FEFFFF00000000")
        && hardware.Matches(0x403BB9, "6A006A00FFB5C0FEFFFFFFB5BCFEFFFF")
        && hardware.Matches(0x403BC9, "FFD08B85B0FEFFFF")
        && hardware.Matches(0x403CC0, "C685B7FEFFFF00"));
    Check("HWStats OS destination copy passes 4096 wchar capacity and caller ignores builder result",
        hardware.ImportSlotNames(0x44055C, "wcscpy_s")
        && hardware.Matches(0x403F01, "6800100000FFB5ACFEFFFFFF155C054400")
        && hardware.Matches(0x403F2D, "C685B7FEFFFF01")
        && hardware.Matches(0x404044, "8D8D68EDFFFF")
        && hardware.Calls(0x40404A, OsDescriptionNativeLayout.BuilderVa)
        && hardware.Matches(0x40404F, "33C06A7E")
        && hardware.Matches(0x4040F0, "8D8568FDFFFF"));
    Check("HWStats NT6 name branches distinguish workstation and minor zero/one/two",
        hardware.Matches(0x403B38, "85C07514807DD2017507")
        && hardware.Matches(0x403B50, "83F80175133845D27507")
        && hardware.Matches(0x403B68, "83F8027514807DD2017507"));
    foreach (var literal in new (uint SetupVa, uint TextVa, string Expected)[]
    {
        (0x403B08, 0x4431A8, OsDescriptionTextNativeLayout.MicrosoftPrefix),
        (0x403B42, 0x4431C0, OsDescriptionTextNativeLayout.CaptureNt6Name(0, 1)),
        (0x403B49, 0x4431E0, OsDescriptionTextNativeLayout.CaptureNt6Name(0, 3)),
        (0x403B5A, 0x44320C, OsDescriptionTextNativeLayout.CaptureNt6Name(1, 1)),
        (0x403B61, 0x443224, OsDescriptionTextNativeLayout.CaptureNt6Name(1, 3)),
        (0x403B73, 0x443254, OsDescriptionTextNativeLayout.CaptureNt6Name(2, 1)),
        (0x403B7A, 0x44326C, OsDescriptionTextNativeLayout.CaptureNt6Name(2, 3)),
        (0x403B81, 0x443290, OsDescriptionTextNativeLayout.CaptureNt6Name(3, 1)),
        (0x403C21, 0x443664, "Windows Server 2003 R2, "),
        (0x403CE1, 0x443698, "Windows Storage Server 2003"),
        (0x403CEF, 0x4436D0, "Windows Home Server"),
        (0x403D04, 0x4436F8, "Windows XP Professional x64 Edition"),
        (0x403D0B, 0x443740, "Windows Server 2003, "),
        (0x403D39, 0x443798, "Datacenter Edition for Itanium-based Systems"),
        (0x403D44, 0x443818, "Enterprise Edition for Itanium-based Systems"),
        (0x403D58, 0x443874, "Datacenter x64 Edition"),
        (0x403D69, 0x4438A4, "Enterprise x64 Edition"),
        (0x403D70, 0x44376C, "Standard x64 Edition"),
        (0x403D7E, 0x4438D4, "Compute Cluster Edition"),
        (0x403D89, 0x443904, "Datacenter Edition"),
        (0x403D94, 0x44392C, "Enterprise Edition"),
        (0x403D9B, 0x443954, "Web Edition"),
        (0x403DA7, 0x4437F4, "Standard Edition"),
        (0x403DCF, 0x44396C, "Windows XP "),
        (0x403DEC, 0x443984, "Home Edition"),
        (0x403DF3, 0x4439A0, "Professional"),
        (0x403E10, 0x4439BC, "Windows 2000 "),
        (0x403E26, 0x4439D8, "Professional"),
        (0x403E34, 0x4439F4, "Datacenter Server"),
        (0x403E3B, 0x443A18, "Advanced Server"),
        (0x403E44, 0x443A38, "Server"),
        (0x403E71, 0x443A48, OsDescriptionTextNativeLayout.ServicePackSeparator),
        (0x403E94, 0x443A50, OsDescriptionTextNativeLayout.BuildPrefix),
        (0x403ECD, 0x443A64, OsDescriptionTextNativeLayout.CaptureArchitectureSuffix(6, 9)),
        (0x403ED9, 0x443A78, OsDescriptionTextNativeLayout.CaptureArchitectureSuffix(6, 0))
    })
        Check($"HWStats OS literal {literal.TextVa:X}",
            hardware.Matches(literal.SetupVa, "BA" + Convert.ToHexString(BitConverter.GetBytes(literal.TextVa)))
            && hardware.Matches(literal.TextVa, Convert.ToHexString(System.Text.Encoding.Unicode.GetBytes(literal.Expected + "\0"))));
    Check("HWStats product-code bounds and exact native index bytes",
        hardware.Matches(0x403BD1, "4883F82F771E")
        && hardware.Matches(OsDescriptionTextNativeLayout.ProductIndexTableVa,
            "00010203120405060708090A0B0C0D120E0F121212121212101212121212121212121212121212121212121212121211")
        && hardware.Word(OsDescriptionTextNativeLayout.ProductJumpTableVa + 18 * 4) == 0x403BF5);
    foreach (var edition in new (uint Code, uint Index, uint BlockVa, uint TextVa)[]
    {
        (1, 0, 0x403BE5, 0x4432D4), (2, 1, 0x403C39, 0x443340),
        (3, 2, 0x403C32, 0x443314), (4, 3, 0x403C40, 0x443368),
        (6, 4, 0x403C47, 0x443390), (7, 5, 0x403C9C, 0x4435CC),
        (8, 6, 0x403C5C, 0x443404), (9, 7, 0x403C88, 0x44342C),
        (10, 8, 0x403C6A, 0x4434A8), (11, 9, 0x403C4E, 0x4433B4),
        (12, 10, 0x403C63, 0x443458), (13, 11, 0x403CA6, 0x4435F0),
        (14, 12, 0x403C74, 0x4434D0), (15, 13, 0x403C7E, 0x443520),
        (17, 14, 0x403CB0, 0x44363C), (18, 15, 0x403C55, 0x4433D4),
        (25, 16, 0x403C92, 0x443580), (48, 17, 0x403C2B, 0x4432F8)
    })
        Check($"HWStats product code {edition.Code} index, destination and wide edition",
            hardware.Word(OsDescriptionTextNativeLayout.ProductJumpTableVa + edition.Index * 4) == edition.BlockVa
            && hardware.Matches(edition.BlockVa, "BA" + Convert.ToHexString(BitConverter.GetBytes(edition.TextVa)))
            && hardware.Matches(edition.TextVa, Convert.ToHexString(System.Text.Encoding.Unicode.GetBytes(
                OsDescriptionTextNativeLayout.CaptureNt6Edition(edition.Code) + "\0"))));
    Check("HWStats OS service-pack admission and unsigned build insertion preserve writer sequence",
        hardware.Matches(0x403E60, "668B0183C1026685C075F5")
        && hardware.Matches(0x403E6B, "2BCAD1F9741D")
        && hardware.Calls(0x403E7C, OsDescriptionTextNativeLayout.WideTextWriterVa)
        && hardware.Calls(0x403E89, OsDescriptionTextNativeLayout.WideTextWriterVa)
        && hardware.ImportSlotNames(0x4402E4, "??6?$basic_ostream@_WU?$char_traits@_W@std@@@std@@QAEAAV01@K@Z")
        && hardware.Matches(0x403E8E, "FFB5C4FEFFFF")
        && hardware.Calls(0x403EB2, OsDescriptionTextNativeLayout.ClosingDelimiterWriterVa)
        && hardware.Matches(0x406344, "C745E44C3A4400")
        && hardware.Ascii(0x443A4C) == ((char)OsDescriptionTextNativeLayout.ClosingDelimiterInputByte).ToString());
    Check("HWStats NT5.2 name priority is ServerR2 metric then storage, home, workstation AMD64",
        hardware.Matches(0x403BF5, "83BDBCFEFFFF050F8552020000")
        && hardware.Matches(0x403C08, "83F8020F85B9010000")
        && hardware.ImportSlotNames(0x4405AC, "GetSystemMetrics")
        && hardware.Matches(0x403C11, "6A59")
        && hardware.Matches(0x403C19, "85C00F84B6000000")
        && hardware.Matches(0x403CDA, "A9002000007407")
        && hardware.Matches(0x403CE8, "A9008000007407")
        && hardware.Matches(0x403CF6, "807DD201750F6683BDD4FDFFFF09"));
    Check("HWStats NT5.2 editions skip workstation and branch on IA64 versus AMD64",
        hardware.Matches(0x403D1B, "807DD2010F8492000000")
        && hardware.Matches(0x403D25, "668B85D4FDFFFF6683F8067519")
        && hardware.Matches(0x403D32, "8B45D084C07907")
        && hardware.Matches(0x403D40, "A8027473")
        && hardware.Matches(0x403D4B, "6683F8098B45D07523"));
    Check("HWStats NT5.2 remaining architecture edition priority preserves suite tests",
        hardware.Matches(0x403D77, "A9004000007407")
        && hardware.Matches(0x403D85, "84C07907")
        && hardware.Matches(0x403D90, "A8027407")
        && hardware.Matches(0x403DA0, "A9000400007505"));
    Check("HWStats NT5.1 personal suite and NT5.0 workstation/datacenter/enterprise gates",
        hardware.Matches(0x403DCA, "83F801753D")
        && hardware.Matches(0x403DDF, "F745D000020000")
        && hardware.Matches(0x403E0C, "85C07544")
        && hardware.Matches(0x403E20, "807DD2017507")
        && hardware.Matches(0x403E30, "84C07907")
        && hardware.Matches(0x403E40, "A8027505"));
    Check("HWStats architecture suffix requires major at least six and AMD64 or x86",
        hardware.Matches(0x403EB7, "83BDBCFEFFFF067229")
        && hardware.Matches(0x403EC7, "6683F8097507")
        && hardware.Matches(0x403ED4, "6685C07510"));
    Check("DeploymentCommon fatal worker clears running flag and excludes CurrentThread from abort requests",
        deployment.Matches(0x00403640, "02177D28000004")
        && deployment.Matches(0x00403FB7, "02167D280000040207287B000006")
        && deployment.Matches(0x00403FDC, "288F00000A092E06096FAE00000A"));
    Check("DeploymentCommon queue drain signals event and ThreadAbortException skips ordinary diagnostic block",
        deployment.Matches(0x0040401E,
            "120428A900000A75710000016FB000000A26")
        && deployment.Matches(0x00404048, "07757B0000013AFA000000"));
    Check("DeploymentCommon outer wait observes events then worker liveness",
        deployment.Matches(0x00403B4B, "11056F9900000A")
        && deployment.Matches(0x00403B8E, "FE0682000006739D00000A803A000004")
        && deployment.ManagedIlMatches(DownloadCancellationNativeLayout.WorkerAlivePredicateMethodRow,
            DownloadCancellationNativeLayout.WorkerAlivePredicateBodyRva, "026F7B00000A2A"));
    Check("DeploymentCommon timer disposal belongs to wait-region finally, not a whole-method cleanup",
        deployment.Matches(0x00403B20,
            "0202FE067E000006739700000A1420D007000020D0070000739800000A2513117D2A000004")
        && deployment.Matches(0x00403BB2, "11122C0711126F4000000ADC")
        && deployment.ManagedFinallyMatches(DownloadWorkerNativeLayout.DoFileDownloadMethodRow,
            0x28D, 0x69, 0x2F6, 0xC));
    Check("DeploymentCommon worker cancellation finally regions release enumerator and lock",
        deployment.ManagedFinallyMatches(DownloadWorkerNativeLayout.ThreadRunnerMethodRow,
            0x1B6, 0x23, 0x1D9, 0xE)
        && deployment.ManagedFinallyMatches(DownloadWorkerNativeLayout.ThreadRunnerMethodRow,
            0x62, 0x1C2, 0x224, 8)
        && deployment.ManagedFinallyMatches(DownloadWorkerNativeLayout.DoDownloadMethodRow,
            0, 0x4B6, 0x4B6, 0x1C));
    Check("DeploymentCommon outer enumerator and stream cleanup have bounded protected regions",
        deployment.ManagedFinallyMatches(DownloadWorkerNativeLayout.DoFileDownloadMethodRow,
            0x194, 0xC8, 0x25C, 8)
        && deployment.ManagedFinallyMatches(DownloadWorkerNativeLayout.DoFileDownloadMethodRow,
            0x298, 0x1E, 0x2B6, 0xE)
        && deployment.ManagedFinallyMatches(DownloadWorkerNativeLayout.DoFileDownloadMethodRow,
            0x405, 0xA8, 0x4AD, 0xE)
        && deployment.ManagedFinallyMatches(DownloadWorkerNativeLayout.DoFileDownloadMethodRow,
            0x3F8, 0xC5, 0x4BD, 0xC));
    Check("DeploymentCommon worker invokes DoDownload inside System.Exception catch",
        deployment.Matches(0x00403E57, "120028A800000A120028A900000A6F9E000006DDE0020000")
        && deployment.ManagedCatchMatches(DownloadWorkerNativeLayout.ThreadRunnerMethodRow,
            0x3B, 0x18, 0x53, 0x2E0, "System.Exception"));
    Check("DeploymentCommon worker retry classification uses global budget and per-part slots",
        deployment.Matches(0x00403E6F,
            "0B027B29000004251308288700000A07757800000139210100000775780000010C027B270000041F0A3C0D010000086FAA00000A1F0E2E15086F2D00000A72F41D0070281C00000A39EE000000")
        && deployment.Matches(0x00403EBC,
            "120028A800000A086F9C000006120028A800000A256F9900000617586F9A000006"));
    Check("DeploymentCommon worker increments global count and requeues the reset event pair",
        deployment.Matches(0x00403F53,
            "02257B2700000417587D27000004120028A900000A75710000012C12120028A900000A75710000016FAB00000A26027B2900000425130A288700000A02286E000006066F8400000ADE08110A289600000ADCDDA5010000"));
    Check("DeploymentCommon worker preserves first fatal slot and reports the caught exception",
        deployment.Matches(0x00403FAA, "02287A0000062C05DDA301000002167D280000040207287B000006")
        && deployment.Matches(0x00404053,
            "72191F0070120028A800000A6F870000068C6D000001076F2D00000A281500000A28F9000006"));
    Check("DeploymentCommon terminal retry exception reaches completion finally",
        deployment.Matches(0x0040483B,
            "72522500700228870000068C6D000001281B00000A28F900000672F32500700228870000068C6D000001281B00000A73650000067A")
        && deployment.Matches(0x00404A12,
            "021728980000060375710000012C0C0375710000016FB000000A26DC"));
    Check("DeploymentCommon outer downloader reports then throws the captured exception",
        deployment.Matches(0x00403BBE,
            "02287A0000062C21725A1B007002287A0000066F2D00000A281E00000A28F900000602287A0000067A")
        && deployment.ManagedUserStringsMatch(
            (0x1DF4, "test"),
            (0x1DFE, "Part {0} has encountered {1} total potentially recoverable errors.  {2}.  Status {3}.  Global recoverable error count {4}/{5}  Trying again."),
            (0x1F19, "Caught an exception while downloading part number {0}: {1}"),
            (0x1F8F, "A WebException occurred: "),
            (0x1B5A, "An error occurred while using the multi-part downloader: ")));
    Check("DeploymentCommon response status and shared retry counter instruction boundaries",
        deployment.Matches(0x004047A1, "110D6F6500000A20CE0000003BBE000000")
        && deployment.Matches(0x00404808, "110D6F6500000A20C8000000335A")
        && deployment.Matches(0x00404820, "11061758130611061F0A300F")
        && deployment.Matches(0x00404836, "DD45FEFFFF")
        && deployment.Matches(0x00404984, "11061758130611061F0A300F")
        && deployment.Matches(0x0040499A, "38E1FCFFFF"));
    Check("DeploymentCommon response and MD5 retry diagnostic literals",
        deployment.ManagedUserStringsMatch(
            (0x2497, "Got an unexpected response code from range request with offset of {0}-{3} for chunk {2}: {1}"),
            (0x16B8, "Trying download again"),
            (0x2552, "Exceeded max number of retries (10), starting download over again for chunk {0}"),
            (0x25F3, "Exceeded the maximum number of retry attempts for range request on chunk {0}."),
            (0x272A, "Downloaded file part {0}, but MD5 didn't match.  Expected {1} but got {2}."),
            (0x27C1, "Trying again"),
            (0x27DB, "Exceeded max number of retries (10), starting chunk download over again for chunk {0}"),
            (0x2888, "Exceeded the maximum number of retry attempts (MD5 mismatch) for chunk {0}.")));
    Check("DeploymentCommon first download range uses zero start and inclusive total-size clamp",
        deployment.Matches(0x0040397A, "166A0C02287200000608200000A0006A58287E00000A0D")
        && deployment.Matches(0x00403991, "161304"));
    Check("DeploymentCommon constructs each zero-based part with its corresponding digest lookup",
        deployment.Matches(0x004039A0,
            "080902286A0000061104022868000006027B2B00000411046F8000000A2D03142B0D027B2B00000411046F8100000A02FE066600000673A2000006739D000006")
        && deployment.ManagedIlMatches(0x9D, 0x44F4,
            "02281300000A0203288C0000060204288E000006020572752200700E048C6D00000128BD00000A2890000006020E052892000006020E042888000006020E06288A000006020E077D3B0000042A"));
    Check("DeploymentCommon later ranges advance end plus one with signed total-size comparison",
        deployment.Matches(0x00403A1A,
            "09176A580C022872000006176A5909200000A0006A58287E00000A0D080228720000063F"));
    Check("DeploymentCommon cached length and HTTP Range header use the supplied inclusive endpoints",
        deployment.Matches(0x00404571, "02288D00000602288B00000659176A580C")
        && deployment.Matches(0x004046F1,
            "D062000001285E00000A72051500701F24285F00000A130B720D24007002288B0000068C5900000102288D0000068C59000001281500000A130C110B110A6F6000000A188D0100000113121112167241150070A2111217110CA211126F6100000A26")
        && deployment.ManagedUserStringsMatch(
            (0x2275, ".part"), (0x1505, "AddWithoutValidate"),
            (0x240D, "bytes={0}-{1}"), (0x1541, "Range")));
    Check("DeploymentCommon is a managed deployment assembly, not an observed native import of these helpers",
        deployment.ImportsFrom("mscoree.dll").Contains("_CorDllMain")
        && new[] { red, dumper, truck, hardware, child, browser, client }
            .All(sample => sample.ImportsFrom("DeploymentCommon.dll").Count == 0));
    Check("Managed process bitness compares IntPtr.Size exactly with eight",
        deployment.Matches(deployment.ImageBase + WowNativeLayout.Is64BitProcessIlRva,
            "284200000A1EFE012A"));
    Check("Managed OS bitness short-circuits x64 and guards the WOW64 API success/out result",
        deployment.Matches(deployment.ImageBase + WowNativeLayout.Is64BitOperatingSystemIlRva,
            "28530000062C02172A72F0110070720A12007028550000062C102857000006120028560000062C02062A162A"));
    Check("Managed export detection uses already-loaded module handle and compares pointers to zero",
        deployment.Matches(deployment.ImageBase + WowNativeLayout.ModuleContainsFunctionIlRva,
            "0228580000060A067E4300000A284400000A2C12060328590000067E4300000A284400000A2A162A"));
    Check("Managed window P/Invokes retain WinApi convention without SetLastError flags",
        deployment.ManagedImportMatches(NativeUtilityNativeLayout.ClientToScreenMethodRow,
            "user32.dll", "ClientToScreen", "00020218101175", 0x0100)
        && deployment.ManagedImportMatches(NativeUtilityNativeLayout.MoveWindowMethodRow,
            "user32.dll", "MoveWindow", "000602180808080802", 0x0100));
    Check("Managed shell folder import signature and flags match its observed boundary",
        deployment.ManagedImportMatches(NativeUtilityNativeLayout.ShellFolderImportMethodRow,
            "shell32.dll", "SHGetSpecialFolderPath", "0004021812650802", 0x0100));
    Check("Common folder full IL pops native BOOL and falls back only through the exception path",
        deployment.ManagedIlMatches(NativeUtilityNativeLayout.GetCommonPathMethodRow,
            NativeUtilityNativeLayout.GetCommonPathBodyRva,
            "200401000073EF00000A0A7E4300000A060216282701000626066F4100000A0CDE390B725B390070076F2D00000A281E00000A28F900000672FC390070725B390070076F2D00000A281E00000A28FC000006DE0003282900000A2A082A"));
    Check("Common folder fallback is protected by the observed System.Exception catch region",
        deployment.ManagedCatchMatches(NativeUtilityNativeLayout.GetCommonPathMethodRow,
            0, 34, 34, 50, "System.Exception"));
    Check("WOW64 probe P/Invoke signature and flags match guarded IL call",
        deployment.ManagedImportMatches(0x56, "kernel32.dll", "IsWow64Process", "000202181002", 0x0140)
        && deployment.ManagedImportMatches(0x57, "kernel32.dll", "GetCurrentProcess", "000018", 0x0146)
        && deployment.ManagedImportMatches(0x58, "kernel32.dll", "GetModuleHandle", "0001180E", 0x0106)
        && deployment.ManagedImportMatches(0x59, "kernel32.dll", "GetProcAddress", "000218180E", 0x0142));
    Check("Managed shortcut creation complete IL preserves three links and exact COM property calls",
        deployment.ManagedIlMatches(ShortcutNativeLayout.CreateShortcutMethodRow,
            ShortcutNativeLayout.CreateShortcutBodyRva,
            "729F37007028F9000006283100000672FD360070282600000A0A733B01000A0B1F191F1028280100060C07087209380070282B000006281B00000A282600000A6F3C01000A74AB0000010D09067213370070282600000A6F3D01000A097223380070282B000006281B00000A6F3E01000A090672D7360070282600000A6F3F01000A09066F4001000A096F4101000A1F161F0B28280100061304110472600E0070282B000006281B00000A282600000A13051105282500000A2D08110528D600000A260711057209380070282B000006281B00000A282600000A6F3C01000A74AB0000010D09067213370070282600000A6F3D01000A097223380070282B000006281B00000A6F3E01000A090672D7360070282600000A6F3F01000A09066F4001000A096F4101000A0711057235380070282B000006281B00000A282600000A6F3C01000A74AB0000010D28310000067259380070282600000A1306091106726D380070282600000A6F3D01000A0972993800706F4201000A0972AF380070282B000006281B00000A6F3E01000A0911066F4001000A096F4101000A72CB38007028F90000062A"));
    Check("Managed shortcut string tokens match target, icon, description, and folder plans",
        deployment.ManagedUserStringsMatch(
            (0x36FD, @"system\bin"), (0x3809, "Play {0}.lnk"), (0x3713, "Launcher.exe"),
            (0x3823, "Play {0}"), (0x36D7, "FirefallClient.exe"),
            (0x0E60, ShortcutNativeLayout.StartMenuProductDirectoryFormat),
            (0x3835, "Uninstall {0}.lnk"), (0x3859, "uninstall"),
            (0x386D, "FirefallInstaller.exe"), (0x3899, "/uninstall"),
            (0x38AF, "Uninstall {0}"), (0x393F, "Red 5 Studios")));
    Check("Managed product getter IL and string tokens distinguish beta key from display",
        deployment.ManagedIlMatches(InstallLocationNativeLayout.ProductIdentifierMethodRow,
            InstallLocationNativeLayout.ProductIdentifierBodyRva, "72370800702A")
        && deployment.ManagedIlMatches(InstallLocationNativeLayout.ProductDisplayMethodRow,
            InstallLocationNativeLayout.ProductDisplayBodyRva, "72530800702A")
        && deployment.ManagedUserStringsMatch(
            (0x837, InstallLocationNativeLayout.ProductIdentifier),
            (0x853, InstallLocationNativeLayout.ProductIdentifierDisplay)));
    Check("Managed install root IL retains override precedence, castclass, and global substring replacement",
        deployment.ManagedIlMatches(InstallLocationNativeLayout.GetInstallLocationMethodRow,
            InstallLocationNativeLayout.GetInstallLocationBodyRva,
            "281F00000A6F2000000A72FD0800706F2100000A2C49281F00000A6F2000000A72FD0800706F2200000A7439000001282300000A2D29281F00000A6F2000000A72FD0800706F2200000A7439000001721309007072290900706F2400000A2A722B09007028440000060A06282300000A2D1106721309007072290900706F2400000A0A062A")
        && deployment.ManagedUserStringsMatch(
            (0x8FD, InstallLocationNativeLayout.ApplicationPropertyName),
            (0x913, InstallLocationNativeLayout.RemovedCultureSubstring), (0x929, ""),
            (0x92B, InstallLocationNativeLayout.RegistryValueName)));
    Check("Managed install registry accessor IL uses LocalMachine and isinst string without coercion",
        deployment.ManagedIlMatches(InstallLocationNativeLayout.GetInstallRegistryKeyMethodRow,
            InstallLocationNativeLayout.GetInstallRegistryKeyBodyRva,
            "7E2A00000A72250C0070282A000006281B00000A6F2E00000A0A062C0D06026F2F00000A75390000012A142A")
        && deployment.ManagedUserStringsMatch((0xC25, @"Software\Red 5 Studios\{0}\")));
    Check("Managed permission launch complete IL ignores Start BOOL and does not observe process exit",
        deployment.ManagedIlMatches(PermissionLaunchNativeLayout.ResetPermissionsMethodRow,
            PermissionLaunchNativeLayout.ResetPermissionsBodyRva,
            "733001000A0B071F25282900000A723A3D0070282600000A72503D007002281B00000A734B01000A0C08176F4C01000A08166F4D01000A08166F4E01000A086F5001000A070A066F3601000A26DE320D727C3D0070096F2D00000A281E00000A28F900000672B43D0070727C3D0070096F2D00000A281E00000A28FC000006DE002A")
        && deployment.ManagedUserStringsMatch(
            (0x3D3A, PermissionLaunchNativeLayout.ExecutableName),
            (0x3D50, PermissionLaunchNativeLayout.ArgumentFormat),
            (0x3D7C, PermissionLaunchNativeLayout.ExceptionLogPrefix),
            (0x3DB4, PermissionLaunchNativeLayout.PersistentEventName)));
    Check("Managed permission launch catches System.Exception over construction and start, not child exit",
        deployment.ManagedCatchMatches(PermissionLaunchNativeLayout.ResetPermissionsMethodRow,
            0, 79, 79, 50, "System.Exception"));
    Check("Managed INI read/write imports preserve observed charset asymmetry and unsigned results",
        deployment.ManagedImportMatches(0xE2, "kernel32.dll", "GetPrivateProfileString",
            "0006090E0E0E1D03090E", 0x0106)
        && deployment.ManagedImportMatches(0xE3, "kernel32.dll", "WritePrivateProfileString",
            "0004090E0E0E0E", 0x0100)
        && deployment.ManagedImportMatches(0xE4, "kernel32.dll", "WritePrivateProfileSection",
            "0003090E0E0E", 0x0100));
    Check("Managed INI read full IL retains 512 start, nSize-1 retry, max32768, and null/too-large branches",
        deployment.ManagedIlMatches(IniFileNativeLayout.GetStringMethodRow,
            IniFileNativeLayout.GetStringBodyRva,
            "20000200000A068D450000010B0304722909007007078E69027B5200000428E20000060C082D02142A06185A0A086E078E6917596A330806200080000031C7086E078E6917596A331272902E00700304281500000A73E900000A7A08E08D450000010D0709086E28EA00000A0973EB00000A130411046FEC00000A2D02142A11042A")
        && deployment.ManagedIlMatches(0xD9, 0x531F, "20FE7F00002A"));
    Check("Managed INI Boolean wrapper uses instance Equals on true/1, not a null-safe Boolean parser",
        deployment.ManagedIlMatches(IniFileNativeLayout.GetBoolWithDefaultMethodRow,
            IniFileNativeLayout.GetBoolWithDefaultBodyRva,
            "02030428DA0000060A0672D42E0070196F7D00000A2D0E0672DE2E0070196F7D00000A2C02172A052A")
        && deployment.ManagedIlMatches(0xDC, 0x53DA, "0203041628DD0000062A")
        && deployment.ManagedUserStringsMatch((0x2ED4, "true"), (0x2EDE, "1")));
    Check("Managed INI key existence and deletion preserve instance compare and ignored section-write result",
        deployment.ManagedIlMatches(IniFileNativeLayout.KeyExistsMethodRow,
            IniFileNativeLayout.KeyExistsBodyRva,
            "020328DF0000060A060D1613042B190911049A0B0407196F7D00000A2C04170CDE0F1104175813041104098E6932E0162A082A")
        && deployment.ManagedIlMatches(IniFileNativeLayout.DeleteSectionKeysMethodRow,
            IniFileNativeLayout.DeleteSectionKeysBodyRva, "037229090070027B5200000428E4000006262A")
        && deployment.ManagedIlMatches(0xDB, 0x53B8, "030405027B5200000428E30000060A062C02172A162A"));
    Check("Managed INI section/key enumeration passes null selectors and splits NUL with RemoveEmptyEntries",
        deployment.ManagedIlMatches(0xDE, 0x5420,
            "02141428DA0000060A062D07168D390000012A06178D450000010B07176FED00000A2A")
        && deployment.ManagedIlMatches(0xDF, 0x5450,
            "02031428DA0000060A062D07168D390000012A06178D450000010B07176FED00000A2A"));
    Check("Managed shortcut removal complete IL preserves common plus user passes and nonrecursive empty-directory gates",
        deployment.ManagedIlMatches(ShortcutNativeLayout.RemoveShortcutsMethodRow,
            ShortcutNativeLayout.RemoveShortcutsBodyRva,
            "72D538007028F90000061F191F1028280100067209380070282B000006281B00000A282600000A0A06282700000A2C0606285500000A1F161F0B28280100060B07723F390070282600000A0C08282B000006282600000A0D097209380070282B000006281B00000A282600000A1304097235380070282B000006281B00000A282600000A13051104282700000A2C071104285500000A1105282700000A2C071105285500000A09282500000A2C1109284301000A8E692D070916284401000A08282500000A2C1108284301000A8E692D070816284401000A1F10282900000A7209380070282B000006281B00000A282600000A0A06282700000A2C0606285500000A1F0B282900000A0B07723F390070282600000A0C08282B000006282600000A0D097209380070282B000006281B00000A282600000A1304097235380070282B000006281B00000A282600000A13051104282700000A2C071104285500000A1105282700000A2C071105285500000A09282500000A2C1109284301000A8E692D070916284401000A08282500000A2C1108284301000A8E692D070816284401000A72CB38007028F90000062A"));
}
else
{
    Console.WriteLine("Binary evidence checks not run: supply --samples with the authorized local sample directory.");
}

Console.WriteLine($"{passed} passed; {failed} failed.");
return failed == 0 ? 0 : 1;
