using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct ReliableReceiveSlotPublicationCapture(
    uint? FrameArgument8Dword);

public enum ReliableReceiveSlotPublicationOutcome
{
    MissingFrameArgument8Capture,
    FrameArgument8CaptureAvailable
}

public readonly record struct ReliableReceiveSlotPublicationPlan(
    uint FrameEbp,
    uint PayloadEdi,
    ReliableReceiveSlotPublicationCapture Capture,
    ReliableReceiveSlotPublicationOutcome Outcome,
    uint FrameArgument8Address,
    bool FrameArgument8Read,
    uint? CapturedPointer,
    bool PayloadDwordWriteProjected,
    uint? PayloadDwordWriteAddress,
    uint? PayloadDwordWriteValue,
    bool AlOneEffectProjected,
    uint? ProjectedEaxAfterAlOne,
    ImmutableArray<string> StackPopRegisters,
    ushort? ReturnStackBytes,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveSlotPublicationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the frame+8 pointer load, payload publication, AL update, and return sequence.
/// A projected write attempt does not guarantee that the memory access succeeds or that
/// execution reaches the later instructions.
/// </summary>
public static class ReliableReceiveSlotPublicationProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint PublicationStartRawFileOffset = 0x00BE53E9;
    public const uint PublicationStartVirtualAddress = 0x00FE5FE9;
    public const uint PublicationEndRawFileOffsetExclusive = 0x00BE53F7;
    public const uint FrameArgument8Offset = 0x08;

    public const uint FrameArgument8ReadRawFileOffset = 0x00BE53E9;
    public const uint PayloadDwordWriteRawFileOffset = 0x00BE53EC;
    public const uint AlOneRawFileOffset = 0x00BE53EE;
    public const uint StackPopsRawFileOffset = 0x00BE53F0;
    public const uint ReturnRawFileOffset = 0x00BE53F4;
    public const ushort ReturnStackBytes = 0x04;

    public const string CapturedPublicationBytes =
        "8B 45 08 89 38 B0 01 5F 5E 5B 5D C2 04 00";

    public static ImmutableArray<ReliableReceiveSlotPublicationInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveSlotPublicationInstructionEvidence(
                "Read the DWORD at EBP+8 as the independent frame argument",
                FrameArgument8ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameArgument8ReadRawFileOffset),
                "8B 45 08"),
            new ReliableReceiveSlotPublicationInstructionEvidence(
                "Attempt to write EDI to the DWORD addressed by the captured pointer",
                PayloadDwordWriteRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadDwordWriteRawFileOffset),
                "89 38"),
            new ReliableReceiveSlotPublicationInstructionEvidence(
                "Set AL to one, preserving EAX's upper 24 bits",
                AlOneRawFileOffset,
                RawFileOffsetToVirtualAddress(AlOneRawFileOffset),
                "B0 01"),
            new ReliableReceiveSlotPublicationInstructionEvidence(
                "Pop EDI, ESI, EBX, and EBP in that order",
                StackPopsRawFileOffset,
                RawFileOffsetToVirtualAddress(StackPopsRawFileOffset),
                "5F 5E 5B 5D"),
            new ReliableReceiveSlotPublicationInstructionEvidence(
                "Return and discard four bytes from the caller's stack",
                ReturnRawFileOffset,
                RawFileOffsetToVirtualAddress(ReturnRawFileOffset),
                "C2 04 00"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FrameArgument8Address(uint frameEbp) =>
        unchecked(frameEbp + FrameArgument8Offset);

    public static ReliableReceiveSlotPublicationPlan Project(
        uint frameEbp,
        uint payloadEdi,
        ReliableReceiveSlotPublicationCapture capture)
    {
        uint frameArgument8Address = FrameArgument8Address(frameEbp);
        if (capture.FrameArgument8Dword is not uint pointer)
        {
            return new ReliableReceiveSlotPublicationPlan(
                frameEbp,
                payloadEdi,
                capture,
                ReliableReceiveSlotPublicationOutcome.MissingFrameArgument8Capture,
                frameArgument8Address,
                FrameArgument8Read: false,
                CapturedPointer: null,
                PayloadDwordWriteProjected: false,
                PayloadDwordWriteAddress: null,
                PayloadDwordWriteValue: null,
                AlOneEffectProjected: false,
                ProjectedEaxAfterAlOne: null,
                StackPopRegisters: ImmutableArray<string>.Empty,
                ReturnStackBytes: null,
                StopAtRawFileOffsetExclusive: FrameArgument8ReadRawFileOffset);
        }

        return new ReliableReceiveSlotPublicationPlan(
            frameEbp,
            payloadEdi,
            capture,
            ReliableReceiveSlotPublicationOutcome.FrameArgument8CaptureAvailable,
            frameArgument8Address,
            FrameArgument8Read: true,
            CapturedPointer: pointer,
            PayloadDwordWriteProjected: true,
            PayloadDwordWriteAddress: pointer,
            PayloadDwordWriteValue: payloadEdi,
            AlOneEffectProjected: true,
            ProjectedEaxAfterAlOne: (pointer & 0xFFFFFF00) | 0x01,
            StackPopRegisters: ImmutableArray.Create("EDI", "ESI", "EBX", "EBP"),
            ReturnStackBytes: ReturnStackBytes,
            StopAtRawFileOffsetExclusive: PublicationEndRawFileOffsetExclusive);
    }
}
