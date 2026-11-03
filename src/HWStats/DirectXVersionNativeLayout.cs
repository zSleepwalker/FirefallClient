namespace Firefall.Reconstruction.HWStats;

public enum DirectXVersionReadAction
{
    UseInitializedEmptyBuffer,
    WriteWideTerminatorThenCloseKey,
    RangeCheckFailure
}

public readonly record struct DirectXVersionReadDecision(
    DirectXVersionReadAction Action, uint? TerminatorByteOffset);

/// <summary>Observed registry buffer decisions only; never reads the registry or formats a report.</summary>
public static class DirectXVersionNativeLayout
{
    public const uint SectionVa = 0x404020;
    public const string Subkey = @"SOFTWARE\Microsoft\DirectX";
    public const string ValueName = "Version";
    public const uint Hive = 0x80000002;
    public const uint AccessMask = 0x20019;
    public const uint BufferByteCount = 128;
    public const uint InitialQueryByteCount = 127;
    public const string CombinedWideFormat = "%s, DirectX Version %s";
    public const bool ChecksQueryReturn = false;
    public const bool RequestsValueType = false;

    public static DirectXVersionReadDecision CaptureReadDecision(
        bool keyOpened, uint observedQueryByteCount)
    {
        if (!keyOpened)
            return new(DirectXVersionReadAction.UseInitializedEmptyBuffer, null);
        uint doubledByteCount = unchecked(observedQueryByteCount + observedQueryByteCount);
        return doubledByteCount >= BufferByteCount
            ? new(DirectXVersionReadAction.RangeCheckFailure, null)
            : new(DirectXVersionReadAction.WriteWideTerminatorThenCloseKey, doubledByteCount);
    }
}
