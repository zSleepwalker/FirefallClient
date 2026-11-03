namespace Firefall.Reconstruction.DeploymentCommon;

public enum DownloadWorkerExceptionAction
{
    RetryPart,
    RecordFirstFatalException,
    KeepFirstFatalException
}

public static class DownloadWorkerNativeLayout
{
    public const int DoFileDownloadMethodRow = 0x7C;
    public const uint DoFileDownloadBodyRva = 0x38B0;
    public const int ThreadRunnerMethodRow = 0x7D;
    public const uint ThreadRunnerBodyRva = 0x3E10;
    public const int DoDownloadMethodRow = 0x9E;
    public const uint DoDownloadBodyRva = 0x4550;

    public static DownloadWorkerExceptionAction CaptureWorkerException(
        bool isWebException,
        int webExceptionStatus,
        string? message,
        int globalRecoverableErrorCount,
        bool exceptionSlotOccupied)
    {
        if (isWebException
            && globalRecoverableErrorCount < 10
            && (webExceptionStatus == 14 || message == "test"))
            return DownloadWorkerExceptionAction.RetryPart;

        return exceptionSlotOccupied
            ? DownloadWorkerExceptionAction.KeepFirstFatalException
            : DownloadWorkerExceptionAction.RecordFirstFatalException;
    }

    public static bool ShouldThrowFromDoFileDownload(bool exceptionSlotOccupied) =>
        exceptionSlotOccupied;
}
