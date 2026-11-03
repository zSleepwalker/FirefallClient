namespace Firefall.Reconstruction.Awesomium;

public enum LogFileEntryAction
{
    ReturnTrueForNonzeroStoredHandle,
    PreparePathThenReturnTrueWithoutOpen,
    PreparePathThenAttemptConfiguredOpen
}

public enum LogFileOpenAction
{
    AttemptFallbackOpen,
    SeekEndThenReturnTrue,
    ClearStoredHandleThenReturnFalse
}

/// <summary>Observed file-opening decisions only; performs no filesystem or API work.</summary>
public static class LogFileNativeLayout
{
    public const uint OpenHelperVa = 0x10289C70;
    public const uint DefaultPathBuilderVa = 0x10289A20;
    public const uint LoggerConfigurationVa = 0x1028A2A0;
    public const uint StoredHandleVa = 0x11596B0C;
    public const uint PathObjectPointerVa = 0x11596B08;
    public const uint DestinationStorageVa = 0x11554A18;
    public const uint ConfiguredHandleCloseVa = 0x1028A684;
    public const uint CloseHandleImportVa = 0x11240390;
    public const uint GenericWrite = 0x40000000;
    public const uint ShareReadWrite = 3;
    public const uint OpenAlways = 4;
    public const uint NormalAttributes = 0x80;
    public const uint SeekFromEnd = 2;
    public const string FallbackPath = @".\debug.log";
    public const string DefaultFileName = "debug.log";

    public static LogFileEntryAction CaptureEntry(uint observedStoredHandle, int destination) =>
        observedStoredHandle != 0 ? LogFileEntryAction.ReturnTrueForNonzeroStoredHandle
        : destination is 1 or 3 ? LogFileEntryAction.PreparePathThenAttemptConfiguredOpen
        : LogFileEntryAction.PreparePathThenReturnTrueWithoutOpen;

    public static LogFileOpenAction CaptureOpenResult(uint observedHandle, bool fallbackAttempt) =>
        observedHandle != 0 && observedHandle != uint.MaxValue
            ? LogFileOpenAction.SeekEndThenReturnTrue
            : fallbackAttempt ? LogFileOpenAction.ClearStoredHandleThenReturnFalse
            : LogFileOpenAction.AttemptFallbackOpen;

    public static bool CaptureClosesPriorFileHandle(uint observedHandle) =>
        observedHandle != 0;

    public static bool CaptureReplacesConfiguredPath(int destination) =>
        destination != 0 && destination != 2;
}
