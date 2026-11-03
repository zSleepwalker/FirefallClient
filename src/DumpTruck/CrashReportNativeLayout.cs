namespace Firefall.Reconstruction.DumpTruck;

public readonly record struct CrashReportLayer(
    uint Type,
    string Name,
    bool Compressed,
    uint TypeInstructionVa);

public enum FileDeflateOutcome
{
    Failure,
    RepeatDrain,
    ReadInput,
    Success
}

public readonly record struct FileDeflateDecision(
    FileDeflateOutcome Outcome,
    uint BytesToAppend);

/// <summary>Producer-side report layout and file-compression decisions, not a file parser.</summary>
public static class CrashReportNativeLayout
{
    public const uint Magic = 0xDEFEC8ED;
    public const uint MagicLiteralVa = 0x476314;
    public const uint MagicWriteCallVa = 0x40A879;
    public const int MagicByteCount = 4;
    public const uint EndTag = 0;
    public const uint ApplicationNameLayerType = 4;
    public const int OwnerApplicationNameOffset = 0x0C;
    public const int OwnerMessageOffset = 0x10;
    public const uint FileCompressorVa = 0x409660;
    public const uint OptionalLogWriterVa = 0x40A070;
    public const int LogClippingThreshold = 0x40000;
    public const uint ClippedLogPrefixByteCount = 0x800;
    public const string ClippedLogMarkerFormat = "\r\n\r\n... clipping approximately %u bytes ...\r\n\r\n";
    public const uint FileInputRequestByteCount = 4;
    public const uint FileOutputAdvertisedByteCount = 4;
    public const int FileStreamErrorResult = -2;

    public static IReadOnlyList<CrashReportLayer> LayersInProducerOrder { get; } =
        Array.AsReadOnly(new[]
        {
            new CrashReportLayer(1, "core dump", true, 0x40A8FA),
            new CrashReportLayer(4, "application name", false, 0x40A9A1),
            new CrashReportLayer(2, "main log", true, 0x40AA99),
            new CrashReportLayer(5, "awesomium.log", true, 0x40AB7F),
            new CrashReportLayer(3, "hardware report", true, 0x40AC4E)
        });

    /// <summary>Models the signed comparison of the native ftell result, not successful seeking.</summary>
    public static bool SelectsClippedLogPath(int observedFtellResult) =>
        observedFtellResult > LogClippingThreshold;

    public static int SelectFileFlush(bool endOfFile) => endOfFile ? 4 : 0;

    /// <summary>
    /// Models the branch after deflate and the subsequent EOF query. This routine rejects
    /// only -2; other zlib failures are not silently upgraded to rejection in this capture.
    /// </summary>
    public static FileDeflateDecision CaptureFileDrain(
        int deflateResult, uint remainingOutputBytes, bool endOfFileAfterDrain)
    {
        if (deflateResult == FileStreamErrorResult)
            return new(FileDeflateOutcome.Failure, 0);

        var outcome = remainingOutputBytes == 0 ? FileDeflateOutcome.RepeatDrain
            : endOfFileAfterDrain ? FileDeflateOutcome.Success : FileDeflateOutcome.ReadInput;
        return new(outcome, unchecked(FileOutputAdvertisedByteCount - remainingOutputBytes));
    }
}
