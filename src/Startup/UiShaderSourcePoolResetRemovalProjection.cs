namespace Firefall.Reconstruction.Startup;

public enum UiShaderSourcePoolResetRemovalBoundary
{
    CallbackBoundary,
    IndexIncrementBoundary
}

public enum UiShaderSourcePoolResetRemovalTailCaptureState
{
    Unreached,
    Captured
}

public readonly record struct UiShaderSourcePoolResetRemovalCapture(
    uint CursorEax,
    uint EndEsi,
    uint? TailDword);

public readonly record struct UiShaderSourcePoolResetRemovalProjection(
    UiShaderSourcePoolResetRemovalCapture Capture,
    UiShaderSourcePoolResetRemovalBoundary Boundary,
    UiShaderSourcePoolResetRemovalTailCaptureState TailCaptureState,
    uint? TailDwordReadAddress,
    uint? EndAfterRemoval,
    bool TailDwordLoadAttempted,
    uint? TailDwordValue,
    bool StoreInstructionReachedByControlFlow,
    uint? StoreDestinationCursor,
    uint? StoreValue,
    bool EndSavedToEbp30,
    uint? BranchTargetRawFileOffset);

/// <summary>
/// Projects the single captured cursor/end comparison and its two branch
/// outcomes. Tail reads and stores are represented as instruction-path
/// attempts only; this does not assert that any memory access is valid or
/// completes. No vector traversal, ordering, callback count, or callback
/// behavior is modeled.
/// </summary>
public static class UiShaderSourcePoolResetRemovalProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint ResetRemovalStartRawFileOffset = 0x12CDE44;
    public const uint ResetRemovalEndRawFileOffsetExclusive = 0x12CDE55;
    public const uint CursorEndCompareRawFileOffset = 0x12CDE44;
    public const uint EqualBranchRawFileOffset = 0x12CDE46;
    public const uint CallbackBoundaryRawFileOffset = 0x12CDDD3;
    public const uint TailDwordLoadRawFileOffset = 0x12CDE48;
    public const uint EndSubtractFourRawFileOffset = 0x12CDE4B;
    public const uint CursorStoreRawFileOffset = 0x12CDE4E;
    public const uint SaveEndToEbp30RawFileOffset = 0x12CDE50;
    public const uint IndexIncrementBranchRawFileOffset = 0x12CDE53;
    public const uint IndexIncrementBoundaryRawFileOffset = 0x12CDDE0;
    public const uint TailDwordWidth = 4;
    public const int Ebp30StackDisplacement = -0x30;

    public const string CapturedRemovalBytes =
        "3B C6 74 8B 8B 4E FC 83 C6 FC 89 08 89 75 D0 EB 8B";

    public static UiShaderSourcePoolResetRemovalProjection Project(
        UiShaderSourcePoolResetRemovalCapture capture)
    {
        if (capture.CursorEax == capture.EndEsi)
        {
            if (capture.TailDword.HasValue)
            {
                throw new ArgumentException(
                    "The tail DWORD is unreachable when cursor equals end.",
                    nameof(capture));
            }

            return new UiShaderSourcePoolResetRemovalProjection(
                capture,
                UiShaderSourcePoolResetRemovalBoundary.CallbackBoundary,
                UiShaderSourcePoolResetRemovalTailCaptureState.Unreached,
                null,
                null,
                TailDwordLoadAttempted: false,
                null,
                StoreInstructionReachedByControlFlow: false,
                null,
                null,
                EndSavedToEbp30: false,
                CallbackBoundaryRawFileOffset);
        }

        uint tailAddressAndReducedEnd =
            unchecked(capture.EndEsi - TailDwordWidth);
        if (!capture.TailDword.HasValue)
        {
            throw new ArgumentException(
                "The unequal cursor/end path requires its reached tail DWORD capture.",
                nameof(capture));
        }

        return new UiShaderSourcePoolResetRemovalProjection(
            capture,
            UiShaderSourcePoolResetRemovalBoundary.IndexIncrementBoundary,
            UiShaderSourcePoolResetRemovalTailCaptureState.Captured,
            tailAddressAndReducedEnd,
            tailAddressAndReducedEnd,
            TailDwordLoadAttempted: true,
            capture.TailDword,
            StoreInstructionReachedByControlFlow: true,
            capture.CursorEax,
            capture.TailDword,
            EndSavedToEbp30: true,
            IndexIncrementBoundaryRawFileOffset);
    }
}
