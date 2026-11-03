namespace Firefall.Reconstruction.DeploymentCommon;

public enum DownloadResponseAction
{
    ReadResponseStream,
    ReadUnexpectedStatusResponseStream,
    RetryRequest,
    RetryLimitExceeded
}

public readonly record struct DownloadResponseDecision(DownloadResponseAction Action, int RetryCount);

/// <summary>Pure response and retry decisions observed in DownloadObject.DoDownload.</summary>
public static class DownloadResponseNativeLayout
{
    public const int DoDownloadMethodRow = 0x9E;
    public const uint DoDownloadBodyRva = 0x4550;
    public const int PartialContentStatusCode = 206;
    public const int OkStatusCode = 200;
    public const int MaxRetries = DownloadPlanning.MaxRetries;

    public static DownloadResponseDecision CaptureStatusCode(int statusCode, int retryCount)
    {
        if (statusCode == PartialContentStatusCode)
            return new(DownloadResponseAction.ReadResponseStream, retryCount);

        if (statusCode != OkStatusCode)
            return new(DownloadResponseAction.ReadUnexpectedStatusResponseStream, retryCount);

        return AdvanceRetry(retryCount);
    }

    public static DownloadResponseDecision CaptureMd5Mismatch(int retryCount) =>
        AdvanceRetry(retryCount);

    private static DownloadResponseDecision AdvanceRetry(int retryCount)
    {
        int incremented = unchecked(retryCount + 1);
        return new(
            incremented > MaxRetries ? DownloadResponseAction.RetryLimitExceeded : DownloadResponseAction.RetryRequest,
            incremented);
    }
}
