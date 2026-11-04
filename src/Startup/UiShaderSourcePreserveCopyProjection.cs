namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Captures inputs to the replacement path's preserved-data copy gate. A missing value means that read
/// was not captured; no pointer is dereferenced and allocation success is not modeled.
/// </summary>
public readonly record struct UiShaderSourcePreserveCopyCapture(
    uint RequestedLength,
    uint? SizeAccessorResult,
    byte? PreserveByte);

/// <summary>
/// Projects the replacement-path copy count and whether the captured gates reach the copy call.
/// The size is the virtual accessor result, not a prior header length.
/// </summary>
public readonly record struct UiShaderSourcePreserveCopyPlan(
    bool SizeAccessorCallReached,
    bool PreserveByteTestReached,
    uint CopyByteCount,
    bool CopyCallReached);

public static class UiShaderSourcePreserveCopyProjectionRules
{
    public const string ImmutableSampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressOffsetFromRawFileOffset = 0x00400C00;

    public const uint ZeroRequestBranchRawFileOffset = 0x12CBD4B;
    public const string ZeroRequestBranchBytesHex =
        "85F6750E8B5F08897708897704E9C7000000";
    public const uint SizeAccessorCallRawFileOffset = 0x12CBD61;
    public const string SizeAccessorCallBytesHex = "FF500C";
    public const uint SizeAccessorVtableSlotOffset = 0x0C;
    public const uint SizeMinimumRawFileOffset = 0x12CBD68;
    public const string SizeMinimumBytesHex = "3BC60F47C6";
    public const uint PreserveByteCompareRawFileOffset = 0x12CBDAE;
    public const string PreserveByteCompareBytesHex = "807D0C00";
    public const int PreserveByteFrameArgumentOffset = 0x0C;
    public const uint ZeroCopyCountTestRawFileOffset = 0x12CBDC0;
    public const string ZeroCopyCountTestBytesHex = "85D2";
    public const uint OldDataPointerLoadRawFileOffset = 0x12CBDC8;
    public const string OldDataPointerLoadBytesHex = "8B4804";
    public const uint NewDataPointerLoadRawFileOffset = 0x12CBDD0;
    public const string NewDataPointerLoadBytesHex = "FF7004";
    public const uint CopyCallRawFileOffset = 0x12CBDD3;
    public const string CopyCallBytesHex = "E81C1B2D00";

    public static uint ToCodeVirtualAddress(uint rawFileOffset) =>
        checked(rawFileOffset + CodeVirtualAddressOffsetFromRawFileOffset);

    public static UiShaderSourcePreserveCopyPlan Project(
        UiShaderSourcePreserveCopyCapture capture)
    {
        if (capture.RequestedLength == 0)
        {
            return new UiShaderSourcePreserveCopyPlan(
                SizeAccessorCallReached: false,
                PreserveByteTestReached: false,
                CopyByteCount: 0,
                CopyCallReached: false);
        }

        uint accessorResult = Required(capture.SizeAccessorResult, nameof(capture.SizeAccessorResult));
        byte preserveByte = Required(capture.PreserveByte, nameof(capture.PreserveByte));
        uint copyByteCount = accessorResult < capture.RequestedLength
            ? accessorResult
            : capture.RequestedLength;

        return new UiShaderSourcePreserveCopyPlan(
            SizeAccessorCallReached: true,
            PreserveByteTestReached: true,
            CopyByteCount: copyByteCount,
            CopyCallReached: copyByteCount != 0 && preserveByte != 0);
    }

    private static T Required<T>(T? capturedValue, string parameterName)
        where T : struct =>
        capturedValue ?? throw new ArgumentException(
            "The captured read is required on this projected branch.",
            parameterName);
}
