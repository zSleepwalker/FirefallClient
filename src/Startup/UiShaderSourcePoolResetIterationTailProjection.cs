namespace Firefall.Reconstruction.Startup;

public enum UiShaderSourcePoolResetIterationTailEntryMode
{
    CallbackReturned,
    RemovalCompleted
}

public enum UiShaderSourcePoolResetIterationTailOutcome
{
    NextIteration,
    ReleaseBoundary
}

public readonly record struct UiShaderSourcePoolResetIterationTailCapture(
    UiShaderSourcePoolResetIterationTailEntryMode EntryMode,
    uint? CallbackCountBeforeIncrement,
    uint IndexBeforeIncrement,
    uint LoopBound,
    uint? ReloadedBeginPointer);

public readonly record struct UiShaderSourcePoolResetIterationTailProjection(
    UiShaderSourcePoolResetIterationTailCapture Capture,
    bool BeginReloadInstructionReached,
    uint? ReloadedBeginPointer,
    bool CallbackCountIncrementInstructionReached,
    uint? CallbackCountAfterIncrement,
    uint IndexAfterIncrement,
    bool UnsignedBelowBound,
    UiShaderSourcePoolResetIterationTailOutcome Outcome,
    uint BranchTargetRawFileOffset,
    uint BranchTargetVirtualAddress);

/// <summary>
/// Projects the single reset-iteration tail after a normally returning
/// callback or completed removal. Captures are required only for values whose
/// instructions are reached in the selected entry mode. Arithmetic describes
/// the captured DWORD values only; no callback, memory access, traversal, or
/// candidate calculation is performed.
/// </summary>
public static class UiShaderSourcePoolResetIterationTailProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint FunctionRawFileOffset = 0x12CDC50;
    public const uint CallbackReturnedEntryRawFileOffset = 0x12CDDD7;
    public const uint RemovalCompletedEntryRawFileOffset = 0x12CDDE0;
    public const uint TailStartRawFileOffset = CallbackReturnedEntryRawFileOffset;
    public const uint TailEndRawFileOffsetExclusive = 0x12CDDE6;
    public const uint TailStartVirtualAddress = 0x016CE9D7;
    public const uint TailEndVirtualAddressExclusive = 0x016CE9E6;

    public const uint BeginReloadRawFileOffset = 0x12CDDD7;
    public const uint StackCleanupRawFileOffset = 0x12CDDDA;
    public const uint CallbackCountIncrementRawFileOffset = 0x12CDDDD;
    public const uint IndexIncrementRawFileOffset = 0x12CDDE0;
    public const uint BoundCompareRawFileOffset = 0x12CDDE1;
    public const uint UnsignedBelowBranchRawFileOffset = 0x12CDDE4;
    public const uint NextIterationRawFileOffset = 0x12CDDB2;
    public const uint ReleaseBoundaryRawFileOffset = 0x12CDDE6;
    public const uint NextIterationVirtualAddress =
        NextIterationRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint ReleaseBoundaryVirtualAddress =
        ReleaseBoundaryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const int ReloadedBeginStackOffset = -0x34;
    public const int CallbackCountStackOffset = -0x18;
    public const int LoopBoundThisOffset = 0x20;
    public const uint StackCleanupBytes = 4;
    public const uint DwordWidth = 4;

    public const string CapturedTailBytes =
        "8B 55 CC 83 C4 04 FF 45 E8 47 3B 7B 20 72 CC";

    public static UiShaderSourcePoolResetIterationTailProjection Project(
        UiShaderSourcePoolResetIterationTailCapture capture)
    {
        bool beginReloadReached;
        bool callbackCountIncrementReached;
        uint? callbackCountAfterIncrement;

        switch (capture.EntryMode)
        {
            case UiShaderSourcePoolResetIterationTailEntryMode.CallbackReturned:
                if (!capture.CallbackCountBeforeIncrement.HasValue)
                {
                    throw new ArgumentException(
                        "The callback count is required when the reached increment is projected.",
                        nameof(capture));
                }

                if (!capture.ReloadedBeginPointer.HasValue)
                {
                    throw new ArgumentException(
                        "The reloaded begin pointer is required when its load is reached.",
                        nameof(capture));
                }

                beginReloadReached = true;
                callbackCountIncrementReached = true;
                callbackCountAfterIncrement =
                    unchecked(capture.CallbackCountBeforeIncrement.Value + 1u);
                break;

            case UiShaderSourcePoolResetIterationTailEntryMode.RemovalCompleted:
                if (capture.CallbackCountBeforeIncrement.HasValue)
                {
                    throw new ArgumentException(
                        "The callback count is not read or incremented from the removal entry.",
                        nameof(capture));
                }

                if (capture.ReloadedBeginPointer.HasValue)
                {
                    throw new ArgumentException(
                        "The begin pointer reload is skipped from the removal entry.",
                        nameof(capture));
                }

                beginReloadReached = false;
                callbackCountIncrementReached = false;
                callbackCountAfterIncrement = null;
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(capture),
                    capture.EntryMode,
                    "Unknown iteration-tail entry mode.");
        }

        uint indexAfterIncrement = unchecked(capture.IndexBeforeIncrement + 1u);
        bool unsignedBelowBound = indexAfterIncrement < capture.LoopBound;
        bool continueIteration = unsignedBelowBound;

        return new UiShaderSourcePoolResetIterationTailProjection(
            capture,
            beginReloadReached,
            beginReloadReached ? capture.ReloadedBeginPointer : null,
            callbackCountIncrementReached,
            callbackCountAfterIncrement,
            indexAfterIncrement,
            unsignedBelowBound,
            continueIteration
                ? UiShaderSourcePoolResetIterationTailOutcome.NextIteration
                : UiShaderSourcePoolResetIterationTailOutcome.ReleaseBoundary,
            continueIteration ? NextIterationRawFileOffset : ReleaseBoundaryRawFileOffset,
            continueIteration ? NextIterationVirtualAddress : ReleaseBoundaryVirtualAddress);
    }
}
