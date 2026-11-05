namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Captures used by the direct, non-assertion edge of slBlob's preserve resize.
/// A null value means that read was not captured; capacity reads are separate observations.
/// </summary>
public readonly record struct UiShaderSourceReuseCapture(
    uint RequestedLength,
    uint? ControlPointer,
    uint? ControlCount,
    uint? FirstCapacity,
    uint? Alignment,
    uint? DataPointer,
    uint? SecondCapacity);

/// <summary>
/// Projects only whether the captured branch selects the direct logical-length store.
/// It does not guarantee backing validity, ownership, or fault-free execution.
/// </summary>
public static class UiShaderSourceReuseProjectionRules
{
    public const int BackingCapacityFieldOffset = 0x00;
    public const int BackingDataPointerFieldOffset = 0x04;
    public const int BackingLogicalLengthFieldOffset = 0x0C;
    public const int ControlCountFieldOffset = 0x04;
    public const int ObjectAlignmentFieldOffset = 0x0C;

    public const uint ControlTestRawFileOffset = 0x12CBCF5;
    public const uint ControlNullBranchRawFileOffset = 0x12CBCF7;
    public const uint ControlCountCompareRawFileOffset = 0x12CBCF9;
    public const uint ControlCountBranchRawFileOffset = 0x12CBCFD;
    public const uint FirstCapacityCompareRawFileOffset = 0x12CBCFF;
    public const uint FirstCapacityBranchRawFileOffset = 0x12CBD01;
    public const uint AlignmentLoadRawFileOffset = 0x12CBD03;
    public const uint AlignmentZeroBranchRawFileOffset = 0x12CBD08;
    public const uint DataPointerLoadRawFileOffset = 0x12CBD0A;
    public const uint AlignmentDivideRawFileOffset = 0x12CBD0F;
    public const uint DataPointerRemainderBranchRawFileOffset = 0x12CBD13;
    public const uint SecondCapacityCompareRawFileOffset = 0x12CBD15;
    public const uint DirectLengthStoreBranchRawFileOffset = 0x12CBD17;
    public const uint DirectLengthStoreRawFileOffset = 0x12CBD33;

    public static bool SelectsDirectLengthStoreEdge(UiShaderSourceReuseCapture capture)
    {
        uint controlPointer = Required(capture.ControlPointer, nameof(capture.ControlPointer));
        if (controlPointer == 0)
            return false;

        uint controlCount = Required(capture.ControlCount, nameof(capture.ControlCount));
        if (controlCount != 1)
            return false;

        uint firstCapacity = Required(capture.FirstCapacity, nameof(capture.FirstCapacity));
        if (capture.RequestedLength > firstCapacity)
            return false;

        uint alignment = Required(capture.Alignment, nameof(capture.Alignment));
        if (alignment != 0)
        {
            uint dataPointer = Required(capture.DataPointer, nameof(capture.DataPointer));
            if (dataPointer % alignment != 0)
                return false;
        }

        uint secondCapacity = Required(capture.SecondCapacity, nameof(capture.SecondCapacity));
        return capture.RequestedLength <= secondCapacity;
    }

    private static uint Required(uint? capturedValue, string parameterName) =>
        capturedValue ?? throw new ArgumentException(
            "The captured read is required on this projected branch.",
            parameterName);
}
