namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolResetSecondaryRepeatCapture(
    uint SavedNodePointer,
    bool? ReleaseBackendReturnedNormally,
    uint? ReceiverEbx,
    uint? PopResultEax);

public enum UiShaderSourcePoolResetSecondaryRepeatBoundary
{
    ReleaseBackendReturnUnavailableAtCallBoundary,
    ReleaseBackendDidNotReturn,
    PopResultUnavailableAtCallBoundary,
    ZeroPopResultCountLoad,
    NonzeroPopResultAlignment
}

public readonly record struct UiShaderSourcePoolResetSecondaryRepeatProjection(
    UiShaderSourcePoolResetSecondaryRepeatCapture Capture,
    bool ReleaseBackendCallReached,
    uint ReleaseBackendArgumentPointer,
    bool ReleaseBackendReturnCaptured,
    bool? ReleaseBackendReturnedNormally,
    uint? ReceiverArgumentPointer,
    bool PopCallReached,
    bool PopResultSaved,
    uint? PopResultStackValue,
    bool PopResultTestReached,
    bool? PopResultWasZero,
    uint? BranchTargetRawFileOffset,
    uint? ContinuationRawFileOffset,
    uint StopRawFileOffset,
    bool AlignmentEntryReached,
    bool CountLoadInstructionReached,
    UiShaderSourcePoolResetSecondaryRepeatBoundary Boundary);

public readonly record struct UiShaderSourcePoolResetSecondaryRepeatInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? StackOffset = null,
    uint? BranchTargetRawFileOffset = null,
    uint? CallTargetRawFileOffset = null);

/// <summary>
/// Projects the bounded repeated-secondary release/pop sequence. It stops at
/// the first call boundary whose return or result was not captured, or at the
/// alignment/count-load continuation. It does not model the helper effects,
/// traversal, counts, or any deallocation semantics.
/// </summary>
public static class UiShaderSourcePoolResetSecondaryRepeatProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint ResetFunctionRawFileOffset = 0x12CDC50;
    public const uint RepeatStartRawFileOffset = 0x12CDDE6;
    public const uint RepeatEndRawFileOffsetExclusive = 0x12CDE01;
    public const uint RepeatStartCodeVirtualAddress =
        RepeatStartRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint RepeatEndCodeVirtualAddressExclusive =
        RepeatEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint SavedNodeLoadRawFileOffset = 0x12CDDE6;
    public const uint ReleaseBackendCallRawFileOffset = 0x12CDDE9;
    public const uint ReleaseBackendCallTargetRawFileOffset = 0x12C8D60;
    public const uint ReceiverArgumentFormationRawFileOffset = 0x12CDDEE;
    public const uint PopCallRawFileOffset = 0x12CDDF1;
    public const uint PopCallTargetRawFileOffset = 0x12CDAE0;
    public const uint PopResultSaveRawFileOffset = 0x12CDDF6;
    public const uint PopResultTestRawFileOffset = 0x12CDDF9;
    public const uint PopResultNonzeroBranchRawFileOffset = 0x12CDDFB;
    public const uint AlignmentEntryRawFileOffset = 0x12CDD68;
    public const uint CountLoadRawFileOffset = 0x12CDE01;
    public const uint InitialSecondaryZeroTargetRawFileOffset = 0x12CDE04;

    public const int SavedNodeStackOffset = -0x20;
    public const int SavedPopResultStackOffset = -0x20;
    public const uint ReceiverArgumentOffset = 8;

    public const string CapturedRepeatBytes =
        "8B 4D E0 E8 72 AF FF FF 8D 4B 08 E8 EA FC FF FF " +
        "89 45 E0 85 C0 0F 85 67 FF FF FF";

    public static IReadOnlyList<UiShaderSourcePoolResetSecondaryRepeatInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolResetSecondaryRepeatInstructionEvidence(
                "Load the saved node pointer from EBP-0x20 into ECX",
                SavedNodeLoadRawFileOffset,
                "8B 4D E0",
                StackOffset: SavedNodeStackOffset),
            new UiShaderSourcePoolResetSecondaryRepeatInstructionEvidence(
                "Call the backend release helper with the saved node",
                ReleaseBackendCallRawFileOffset,
                "E8 72 AF FF FF",
                CallTargetRawFileOffset: ReleaseBackendCallTargetRawFileOffset),
            new UiShaderSourcePoolResetSecondaryRepeatInstructionEvidence(
                "Form the next pop receiver argument as EBX plus eight",
                ReceiverArgumentFormationRawFileOffset,
                "8D 4B 08"),
            new UiShaderSourcePoolResetSecondaryRepeatInstructionEvidence(
                "Call the secondary pop helper",
                PopCallRawFileOffset,
                "E8 EA FC FF FF",
                CallTargetRawFileOffset: PopCallTargetRawFileOffset),
            new UiShaderSourcePoolResetSecondaryRepeatInstructionEvidence(
                "Save the pop result in EAX at EBP-0x20",
                PopResultSaveRawFileOffset,
                "89 45 E0",
                StackOffset: SavedPopResultStackOffset),
            new UiShaderSourcePoolResetSecondaryRepeatInstructionEvidence(
                "Test the captured pop result",
                PopResultTestRawFileOffset,
                "85 C0"),
            new UiShaderSourcePoolResetSecondaryRepeatInstructionEvidence(
                "A nonzero pop result branches back to the alignment entry",
                PopResultNonzeroBranchRawFileOffset,
                "0F 85 67 FF FF FF",
                BranchTargetRawFileOffset: AlignmentEntryRawFileOffset)
        });

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static UiShaderSourcePoolResetSecondaryRepeatProjection Project(
        UiShaderSourcePoolResetSecondaryRepeatCapture capture)
    {
        if (!capture.ReleaseBackendReturnedNormally.HasValue
            || !capture.ReleaseBackendReturnedNormally.Value)
        {
            if (capture.ReceiverEbx.HasValue || capture.PopResultEax.HasValue)
            {
                throw new ArgumentException(
                    "Receiver and pop-result captures are unreachable unless the release call returned normally.",
                    nameof(capture));
            }

            bool returnCaptured = capture.ReleaseBackendReturnedNormally.HasValue;
            return new UiShaderSourcePoolResetSecondaryRepeatProjection(
                capture,
                ReleaseBackendCallReached: true,
                ReleaseBackendArgumentPointer: capture.SavedNodePointer,
                ReleaseBackendReturnCaptured: returnCaptured,
                ReleaseBackendReturnedNormally: capture.ReleaseBackendReturnedNormally,
                ReceiverArgumentPointer: null,
                PopCallReached: false,
                PopResultSaved: false,
                PopResultStackValue: null,
                PopResultTestReached: false,
                PopResultWasZero: null,
                BranchTargetRawFileOffset: null,
                ContinuationRawFileOffset: null,
                StopRawFileOffset: ReleaseBackendCallRawFileOffset,
                AlignmentEntryReached: false,
                CountLoadInstructionReached: false,
                Boundary: returnCaptured
                    ? UiShaderSourcePoolResetSecondaryRepeatBoundary.ReleaseBackendDidNotReturn
                    : UiShaderSourcePoolResetSecondaryRepeatBoundary
                        .ReleaseBackendReturnUnavailableAtCallBoundary);
        }

        if (!capture.ReceiverEbx.HasValue)
        {
            throw new ArgumentException(
                "The EBX receiver capture is required when the release call returned normally.",
                nameof(capture));
        }

        uint receiverArgumentPointer =
            unchecked(capture.ReceiverEbx.Value + ReceiverArgumentOffset);
        if (!capture.PopResultEax.HasValue)
        {
            return new UiShaderSourcePoolResetSecondaryRepeatProjection(
                capture,
                ReleaseBackendCallReached: true,
                ReleaseBackendArgumentPointer: capture.SavedNodePointer,
                ReleaseBackendReturnCaptured: true,
                ReleaseBackendReturnedNormally: true,
                ReceiverArgumentPointer: receiverArgumentPointer,
                PopCallReached: true,
                PopResultSaved: false,
                PopResultStackValue: null,
                PopResultTestReached: false,
                PopResultWasZero: null,
                BranchTargetRawFileOffset: null,
                ContinuationRawFileOffset: null,
                StopRawFileOffset: PopCallRawFileOffset,
                AlignmentEntryReached: false,
                CountLoadInstructionReached: false,
                Boundary: UiShaderSourcePoolResetSecondaryRepeatBoundary
                    .PopResultUnavailableAtCallBoundary);
        }

        uint popResult = capture.PopResultEax.Value;
        bool popResultWasZero = popResult == 0;
        uint continuation = popResultWasZero
            ? CountLoadRawFileOffset
            : AlignmentEntryRawFileOffset;

        return new UiShaderSourcePoolResetSecondaryRepeatProjection(
            capture,
            ReleaseBackendCallReached: true,
            ReleaseBackendArgumentPointer: capture.SavedNodePointer,
            ReleaseBackendReturnCaptured: true,
            ReleaseBackendReturnedNormally: true,
            ReceiverArgumentPointer: receiverArgumentPointer,
            PopCallReached: true,
            PopResultSaved: true,
            PopResultStackValue: popResult,
            PopResultTestReached: true,
            PopResultWasZero: popResultWasZero,
            BranchTargetRawFileOffset: popResultWasZero
                ? null
                : AlignmentEntryRawFileOffset,
            ContinuationRawFileOffset: continuation,
            StopRawFileOffset: continuation,
            AlignmentEntryReached: !popResultWasZero,
            CountLoadInstructionReached: popResultWasZero,
            Boundary: popResultWasZero
                ? UiShaderSourcePoolResetSecondaryRepeatBoundary.ZeroPopResultCountLoad
                : UiShaderSourcePoolResetSecondaryRepeatBoundary
                    .NonzeroPopResultAlignment);
    }
}
