namespace Firefall.Reconstruction.Awesomium;

public enum LogSynchronizationInitAction
{
    ReturnWithoutChangingConfiguration,
    AllocateCriticalSectionThenMarkInitialized,
    ReuseStoredMutexThenMarkInitialized,
    BuildNameAndAttemptMutexCreation
}

/// <summary>Inert initialization decisions and name transform; creates no synchronization objects.</summary>
public static class LogSynchronizationNativeLayout
{
    public const uint InitializerVa = 0x10289AE0;
    public const uint InitializedByteVa = 0x11596B20;
    public const uint CriticalSectionPointerVa = 0x11596B24;
    public const uint ModeVa = 0x11596B28;
    public const uint MutexHandleVa = 0x11596B2C;
    public const uint MutexWaitImportVa = 0x112404D0;
    public const uint MutexReleaseImportVa = 0x112404D4;
    public const uint CriticalSectionEnterVa = 0x103B78C0;
    public const uint CriticalSectionLeaveVa = 0x103B78D0;
    public const string MutexNamePrefix = @"Global\";

    public static LogSynchronizationInitAction CaptureEntry(
        bool alreadyInitialized, int requestedMode, uint observedStoredMutex) =>
        alreadyInitialized ? LogSynchronizationInitAction.ReturnWithoutChangingConfiguration
        : requestedMode != 0 ? LogSynchronizationInitAction.AllocateCriticalSectionThenMarkInitialized
        : observedStoredMutex != 0 ? LogSynchronizationInitAction.ReuseStoredMutexThenMarkInitialized
        : LogSynchronizationInitAction.BuildNameAndAttemptMutexCreation;

    public static string CaptureMutexName(string observedPathAfterNativeCopy) =>
        MutexNamePrefix + observedPathAfterNativeCopy.Replace('\\', '/');

    public static bool CaptureInitializedAfterMutexCreation(uint observedReturnedHandle) =>
        observedReturnedHandle != 0;

    public static bool CaptureInitializerCanChangeState(bool alreadyInitialized) =>
        !alreadyInitialized;
}
