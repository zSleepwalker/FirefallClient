namespace Firefall.Reconstruction.DumpTruck;

public enum DeflateDrainOutcome
{
    Failure,
    Repeat,
    Success
}

public readonly record struct DeflateDrainDecision(
    DeflateDrainOutcome Outcome,
    uint BytesToAppend);

public readonly record struct HardwareReportDecision(
    bool AddHardwareLayer,
    bool LogGeneratorFailure,
    bool LogCompressionFailure,
    bool AbortPackaging);

/// <summary>PE-VA evidence and decisions for the HWStats blob and its report layer.</summary>
public static class HardwareReportNativeLayout
{
    public const uint BlobVtableVa = 0x4BDF44;
    public const uint BlobAppendVa = 0x41BAE0;
    public const uint RunGeneratorCallVa = 0x40AC3E;
    public const uint CompressCallVa = 0x40AC7D;
    public const uint CompressVa = 0x409540;
    public const uint DeflateInitVa = 0x45FA80;
    public const uint DeflateVa = 0x460210;
    public const uint DeflateEndVa = 0x460A90;
    public const uint ScratchAllocationByteCount = 0x4000;
    public const uint AdvertisedOutputByteCount = 4;
    public const int FinishFlush = 4;
    public const uint HardwareLayerType = 3;
    public const uint ReportEndTag = 0;
    public const uint LayerBeginVa = 0x426CD0;
    public const uint LayerEndVa = 0x426D30;
    public const int LayerTypeOffset = 0;
    public const int LayerPayloadLengthOffset = 4;
    public const int LayerHeaderByteCount = 8;

    public static HardwareReportDecision CaptureHardwareReport(
        uint generatorExitCode, bool compressionSucceeded) =>
        generatorExitCode != 0
            ? new(false, true, false, false)
            : new(true, false, !compressionSucceeded, !compressionSucceeded);

    /// <summary>
    /// The native loop accepts only zlib results 0/1 and finishes on nonzero avail_out,
    /// not solely on Z_STREAM_END. Subtraction follows native DWORD arithmetic.
    /// </summary>
    public static DeflateDrainDecision CaptureDeflateDrain(int result, uint remainingOutputBytes)
    {
        if (result != 0 && result != 1)
            return new(DeflateDrainOutcome.Failure, 0);

        return new(remainingOutputBytes == 0 ? DeflateDrainOutcome.Repeat : DeflateDrainOutcome.Success,
            unchecked(AdvertisedOutputByteCount - remainingOutputBytes));
    }
}
