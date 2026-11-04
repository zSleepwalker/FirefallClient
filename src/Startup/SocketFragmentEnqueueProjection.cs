namespace Firefall.Reconstruction.Startup;

public readonly record struct SocketFragmentEnqueueCapture(
    uint ReceiverAddress,
    uint PayloadArgument,
    ushort PayloadFirstWord,
    bool? DiagnosticCallReturnedNormally,
    uint? AppendCallReturnEax);

public enum SocketFragmentEnqueueDiagnosticCallOutcome
{
    NotReached,
    MissingReturnCapture,
    DidNotReturnNormally,
    ReturnedNormally
}

public enum SocketFragmentEnqueueAppendCallOutcome
{
    NotReached,
    MissingReturnCapture,
    ReturnedNormally
}

public readonly record struct SocketFragmentEnqueueProjection(
    SocketFragmentEnqueueCapture Capture,
    ushort MaskedPayloadWord,
    bool ReachesDiagnosticCall,
    SocketFragmentEnqueueDiagnosticCallOutcome DiagnosticCallOutcome,
    bool ReachesAppendCall,
    SocketFragmentEnqueueAppendCallOutcome AppendCallOutcome,
    uint? AppendReceiverAddress,
    uint? AppendPayloadArgument,
    uint? WrapperReturnEax);

public readonly record struct SocketFragmentEnqueueInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? TargetRawFileOffset = null,
    uint? AbsoluteVirtualAddress = null,
    uint? NormalReturnContinuationRawFileOffset = null);

/// <summary>
/// Projects the captured wrapper diagnostic gate and normal append-call tail only.
/// It does not interpret payload fields or model either callee's effects.
/// </summary>
public static class SocketFragmentEnqueueProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x00BE47C0;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint WrapperEndRawFileOffsetExclusive = 0x00BE480A;
    public const uint WrapperEndVirtualAddressExclusive =
        WrapperEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint TailStartRawFileOffset = 0x00BE47FA;
    public const uint TailStartVirtualAddress =
        TailStartRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint TailEndRawFileOffsetExclusive = 0x00BE480A;

    public const uint SavedReceiverLoadRawFileOffset = 0x00BE47C9;
    public const uint PayloadArgumentLoadRawFileOffset = 0x00BE47C5;
    public const uint PayloadWordReadRawFileOffset = 0x00BE47D0;
    public const uint PayloadMaskRawFileOffset = 0x00BE47D3;
    public const ushort PayloadMask = 0x07FF;
    public const uint DiagnosticThresholdLoadRawFileOffset = 0x00BE47D6;
    public const ushort DiagnosticThreshold = 0x0570;
    public const uint DiagnosticBranchRawFileOffset = 0x00BE47DE;
    public const uint DiagnosticBranchTargetRawFileOffset = 0x00BE47FA;
    public const uint DiagnosticCallRawFileOffset = 0x00BE47F1;
    public const uint DiagnosticCallVirtualAddress =
        DiagnosticCallRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint DiagnosticCallCellVirtualAddress = 0x01C15EA0;
    public const uint DiagnosticCallNormalReturnRawFileOffset = 0x00BE47F7;
    public const uint DiagnosticArgumentOneVirtualAddress = 0x01B58A05;
    public const uint DiagnosticArgumentTwoVirtualAddress = 0x01B58997;
    public const uint DiagnosticPushedImmediate = 0xB2;

    public const uint AppendPayloadPushRawFileOffset = 0x00BE47FA;
    public const uint AppendReceiverSetupRawFileOffset = 0x00BE47FB;
    public const uint AppendCallRawFileOffset = 0x00BE47FE;
    public const uint AppendCallVirtualAddress =
        AppendCallRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint AppendCallNormalReturnRawFileOffset = 0x00BE4803;
    public const uint AppendCallNormalReturnVirtualAddress =
        AppendCallNormalReturnRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint AppendFunctionRawFileOffset = 0x012CE310;
    public const uint AppendFunctionVirtualAddress =
        AppendFunctionRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint AppendCallReceiverDisplacement = 0x38;
    public const uint AppendCallNextRawFileOffset = 0x00BE4803;
    public const uint WrapperReturnInstructionRawFileOffset = 0x00BE4807;
    public const uint WrapperReturnInstructionVirtualAddress =
        WrapperReturnInstructionRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const ushort WrapperReturnStackBytes = 4;

    public const string CapturedWrapperBytes =
        "55 8B EC 51 56 8B 75 08 57 8B F9 B9 FF 07 00 00 " +
        "66 8B 06 66 23 C1 B9 70 05 00 00 66 3B C1 76 1A " +
        "6A 00 68 B2 00 00 00 68 97 89 B5 01 68 05 8A B5 01 " +
        "FF 15 A0 5E C1 01 83 C4 10 56 8D 4F 38 E8 0D 9B 6E 00 " +
        "5F 5E 59 5D C2 04 00";

    public const string CapturedNormalTailBytes =
        "56 8D 4F 38 E8 0D 9B 6E 00 5F 5E 59 5D C2 04 00";

    public static IReadOnlyList<SocketFragmentEnqueueInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new SocketFragmentEnqueueInstructionEvidence(
                "Load the payload argument into ESI",
                PayloadArgumentLoadRawFileOffset,
                "8B 75 08"),
            new SocketFragmentEnqueueInstructionEvidence(
                "Save the incoming receiver in EDI",
                SavedReceiverLoadRawFileOffset,
                "8B F9"),
            new SocketFragmentEnqueueInstructionEvidence(
                "Load the first captured 16-bit payload word",
                PayloadWordReadRawFileOffset,
                "66 8B 06"),
            new SocketFragmentEnqueueInstructionEvidence(
                "Mask that word with 0x07FF before the unsigned threshold comparison",
                PayloadMaskRawFileOffset,
                "66 23 C1"),
            new SocketFragmentEnqueueInstructionEvidence(
                "Skip the diagnostic call when the masked word is at most 0x0570",
                DiagnosticBranchRawFileOffset,
                "76 1A",
                DiagnosticBranchTargetRawFileOffset),
            new SocketFragmentEnqueueInstructionEvidence(
                "Call through the diagnostic cell and continue at stack cleanup",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                AbsoluteVirtualAddress: DiagnosticCallCellVirtualAddress,
                NormalReturnContinuationRawFileOffset: DiagnosticCallNormalReturnRawFileOffset),
            new SocketFragmentEnqueueInstructionEvidence(
                "Push the original payload argument for the append call",
                AppendPayloadPushRawFileOffset,
                "56"),
            new SocketFragmentEnqueueInstructionEvidence(
                "Form the append receiver from the saved receiver plus 0x38",
                AppendReceiverSetupRawFileOffset,
                "8D 4F 38"),
            new SocketFragmentEnqueueInstructionEvidence(
                "Call the append target and continue directly into the epilogue",
                AppendCallRawFileOffset,
                "E8 0D 9B 6E 00",
                AppendFunctionRawFileOffset,
                NormalReturnContinuationRawFileOffset: AppendCallNormalReturnRawFileOffset),
            new SocketFragmentEnqueueInstructionEvidence(
                "Restore saved registers and return without testing EAX",
                AppendCallNormalReturnRawFileOffset,
                "5F 5E 59 5D C2 04 00")
        });

    public static SocketFragmentEnqueueProjection Project(
        SocketFragmentEnqueueCapture capture)
    {
        ushort maskedPayloadWord = (ushort)(capture.PayloadFirstWord & PayloadMask);
        bool reachesDiagnosticCall = maskedPayloadWord > DiagnosticThreshold;

        if (!reachesDiagnosticCall && capture.DiagnosticCallReturnedNormally.HasValue)
            throw new ArgumentException(
                "A return outcome cannot be captured for the unreached diagnostic call.",
                nameof(capture.DiagnosticCallReturnedNormally));

        bool reachesAppendCall =
            !reachesDiagnosticCall || capture.DiagnosticCallReturnedNormally == true;
        if (!reachesAppendCall && capture.AppendCallReturnEax.HasValue)
            throw new ArgumentException(
                "An append-call return cannot be captured when that call is unreached.",
                nameof(capture.AppendCallReturnEax));

        SocketFragmentEnqueueDiagnosticCallOutcome diagnosticCallOutcome =
            !reachesDiagnosticCall
                ? SocketFragmentEnqueueDiagnosticCallOutcome.NotReached
                : capture.DiagnosticCallReturnedNormally switch
                {
                    null => SocketFragmentEnqueueDiagnosticCallOutcome.MissingReturnCapture,
                    false => SocketFragmentEnqueueDiagnosticCallOutcome.DidNotReturnNormally,
                    true => SocketFragmentEnqueueDiagnosticCallOutcome.ReturnedNormally
                };
        SocketFragmentEnqueueAppendCallOutcome appendCallOutcome =
            !reachesAppendCall
                ? SocketFragmentEnqueueAppendCallOutcome.NotReached
                : capture.AppendCallReturnEax.HasValue
                    ? SocketFragmentEnqueueAppendCallOutcome.ReturnedNormally
                    : SocketFragmentEnqueueAppendCallOutcome.MissingReturnCapture;

        return new(
            capture,
            maskedPayloadWord,
            reachesDiagnosticCall,
            diagnosticCallOutcome,
            reachesAppendCall,
            appendCallOutcome,
            reachesAppendCall
                ? unchecked(capture.ReceiverAddress + AppendCallReceiverDisplacement)
                : null,
            reachesAppendCall ? capture.PayloadArgument : null,
            reachesAppendCall ? capture.AppendCallReturnEax : null);
    }
}
