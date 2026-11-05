namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolResetSecondaryGateCapture(
    uint ReceiverPointer,
    uint? PopResultEax);

public enum UiShaderSourcePoolResetSecondaryGateBoundary
{
    PopResultUnavailableAtCallBoundary,
    ZeroPopResultFinalizationPointerLoad,
    NonzeroPopResultAlignment
}

public readonly record struct UiShaderSourcePoolResetSecondaryGateProjection(
    UiShaderSourcePoolResetSecondaryGateCapture Capture,
    uint ReceiverArgumentPointer,
    uint ReturnedCountEdiAtBoundary,
    uint LocalCountAtBoundary,
    bool PopCallReached,
    bool PopResultSaved,
    uint? PopResultStackValue,
    bool PopResultTestReached,
    bool? PopResultWasZero,
    uint? BranchTargetRawFileOffset,
    uint StopRawFileOffset,
    bool FinalizationCountLoadSkipped,
    UiShaderSourcePoolResetSecondaryGateBoundary Boundary);

public readonly record struct UiShaderSourcePoolResetSecondaryGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? StackOffset = null,
    uint? BranchTargetRawFileOffset = null,
    uint? CallTargetRawFileOffset = null);

/// <summary>
/// Projects the bounded secondary-pop result gate. Only the receiver pointer
/// and returned EAX value are captured; the pop implementation, callbacks,
/// and all subsequent reset work are outside this projection.
/// </summary>
public static class UiShaderSourcePoolResetSecondaryGateProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint ResetFunctionRawFileOffset = 0x12CDC50;
    public const uint GateStartRawFileOffset = 0x12CDD4D;
    public const uint GateEndRawFileOffsetExclusive = 0x12CDD68;
    public const uint GateStartCodeVirtualAddress =
        GateStartRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint GateEndCodeVirtualAddressExclusive =
        GateEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint ReceiverLoadRawFileOffset = 0x12CDD4D;
    public const uint ReturnedCountClearRawFileOffset = 0x12CDD50;
    public const uint LocalCountInitializeRawFileOffset = 0x12CDD52;
    public const uint ReceiverArgumentFormationRawFileOffset = 0x12CDD55;
    public const uint PopCallRawFileOffset = 0x12CDD58;
    public const uint PopCallTargetRawFileOffset = 0x12CDAE0;
    public const uint PopResultSaveRawFileOffset = 0x12CDD5D;
    public const uint PopResultTestRawFileOffset = 0x12CDD60;
    public const uint PopResultZeroBranchRawFileOffset = 0x12CDD62;
    public const uint PopResultZeroBranchTargetRawFileOffset = 0x12CDE04;
    public const uint AlignmentStartRawFileOffset = 0x12CDD68;
    public const uint FinalizationCountLoadRawFileOffset = 0x12CDE01;
    public const uint FinalizationPointerLoadRawFileOffset = 0x12CDE04;

    public const int SavedReceiverStackOffset = -0x1C;
    public const int LocalCountStackOffset = -0x18;
    public const int SavedPopResultStackOffset = -0x20;
    public const uint ReceiverArgumentOffset = 8;
    public const uint InitialReturnedCount = 0;
    public const uint InitialLocalCount = 0;

    public const string CapturedGateBytes =
        "8B 5D E4 33 FF 89 7D E8 8D 4B 08 E8 83 FD FF FF " +
        "89 45 E0 85 C0 0F 84 9C 00 00 00";

    public const string AlignmentEntryBytes = "8B 4B 18";
    public const string FinalizationCountLoadBytes = "8B 7D E8";
    public const string FinalizationPointerLoadBytes = "8B 4D D8";

    public static IReadOnlyList<UiShaderSourcePoolResetSecondaryGateInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolResetSecondaryGateInstructionEvidence(
                "Load the receiver saved at EBP-0x1C",
                ReceiverLoadRawFileOffset,
                "8B 5D E4",
                StackOffset: SavedReceiverStackOffset),
            new UiShaderSourcePoolResetSecondaryGateInstructionEvidence(
                "Initialize the returned-count register EDI to zero",
                ReturnedCountClearRawFileOffset,
                "33 FF"),
            new UiShaderSourcePoolResetSecondaryGateInstructionEvidence(
                "Initialize the local count at EBP-0x18 to zero",
                LocalCountInitializeRawFileOffset,
                "89 7D E8",
                StackOffset: LocalCountStackOffset),
            new UiShaderSourcePoolResetSecondaryGateInstructionEvidence(
                "Form the wrapped receiver-plus-eight argument in ECX",
                ReceiverArgumentFormationRawFileOffset,
                "8D 4B 08"),
            new UiShaderSourcePoolResetSecondaryGateInstructionEvidence(
                "Call the pop helper directly",
                PopCallRawFileOffset,
                "E8 83 FD FF FF",
                CallTargetRawFileOffset: PopCallTargetRawFileOffset),
            new UiShaderSourcePoolResetSecondaryGateInstructionEvidence(
                "Save returned EAX at EBP-0x20",
                PopResultSaveRawFileOffset,
                "89 45 E0",
                StackOffset: SavedPopResultStackOffset),
            new UiShaderSourcePoolResetSecondaryGateInstructionEvidence(
                "Test the captured pop result",
                PopResultTestRawFileOffset,
                "85 C0"),
            new UiShaderSourcePoolResetSecondaryGateInstructionEvidence(
                "A zero pop result branches over the final count load",
                PopResultZeroBranchRawFileOffset,
                "0F 84 9C 00 00 00",
                BranchTargetRawFileOffset: PopResultZeroBranchTargetRawFileOffset)
        });

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static UiShaderSourcePoolResetSecondaryGateProjection Project(
        UiShaderSourcePoolResetSecondaryGateCapture capture)
    {
        uint receiverArgumentPointer =
            unchecked(capture.ReceiverPointer + ReceiverArgumentOffset);

        if (!capture.PopResultEax.HasValue)
        {
            return new UiShaderSourcePoolResetSecondaryGateProjection(
                capture,
                receiverArgumentPointer,
                InitialReturnedCount,
                InitialLocalCount,
                PopCallReached: true,
                PopResultSaved: false,
                PopResultStackValue: null,
                PopResultTestReached: false,
                PopResultWasZero: null,
                BranchTargetRawFileOffset: null,
                StopRawFileOffset: PopCallRawFileOffset,
                FinalizationCountLoadSkipped: false,
                UiShaderSourcePoolResetSecondaryGateBoundary.PopResultUnavailableAtCallBoundary);
        }

        uint popResult = capture.PopResultEax.Value;
        bool popResultWasZero = popResult == 0;
        return new UiShaderSourcePoolResetSecondaryGateProjection(
            capture,
            receiverArgumentPointer,
            InitialReturnedCount,
            InitialLocalCount,
            PopCallReached: true,
            PopResultSaved: true,
            PopResultStackValue: popResult,
            PopResultTestReached: true,
            PopResultWasZero: popResultWasZero,
            BranchTargetRawFileOffset: popResultWasZero
                ? PopResultZeroBranchTargetRawFileOffset
                : AlignmentStartRawFileOffset,
            StopRawFileOffset: popResultWasZero
                ? PopResultZeroBranchTargetRawFileOffset
                : AlignmentStartRawFileOffset,
            FinalizationCountLoadSkipped: popResultWasZero,
            Boundary: popResultWasZero
                ? UiShaderSourcePoolResetSecondaryGateBoundary.ZeroPopResultFinalizationPointerLoad
                : UiShaderSourcePoolResetSecondaryGateBoundary.NonzeroPopResultAlignment);
    }
}
