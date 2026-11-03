namespace Firefall.Reconstruction.DeploymentCommon;

public enum DownloadWorkerAbortDisposition
{
    SkipCurrentThread,
    RequestAbortOtherThread
}

public static class DownloadCancellationNativeLayout
{
    public const int DownloaderConstructorMethodRow = 0x67;
    public const uint DownloaderConstructorBodyRva = 0x3634;
    public const int WorkerAlivePredicateMethodRow = 0x82;
    public const uint WorkerAlivePredicateBodyRva = 0x38A5;

    public const int WorkerAbortCurrentThreadCheckIlOffset = 0x1C0;
    public const int WorkerAbortCallIlOffset = 0x1C9;
    public const int WorkerAbortExceptionTestIlOffset = 0x22C;
    public const int DrainedQueueEventSetIlOffset = 0x202;
    public const int DoFileDownloadWaitTimerFinallyTryOffset = 0x28D;
    public const int DoFileDownloadWaitTimerFinallyHandlerOffset = 0x2F6;

    public static DownloadWorkerAbortDisposition GetAbortDisposition(bool isCurrentThread) =>
        isCurrentThread
            ? DownloadWorkerAbortDisposition.SkipCurrentThread
            : DownloadWorkerAbortDisposition.RequestAbortOtherThread;

    public static bool ShouldSignalDrainedQueueEvent(bool valueIsManualResetEvent) =>
        valueIsManualResetEvent;

    public static bool ShouldContinueWaitingForWorkers(bool anyWorkerIsAlive) =>
        anyWorkerIsAlive;

    public static bool ShouldDisposeTimerOnWaitExit(bool timerLocalIsNonNull) =>
        timerLocalIsNonNull;
}
