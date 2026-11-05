using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveFrameHandoffCapture(
    bool ValidatedEntry,
    uint FrameEbp,
    uint ReceiverEdi,
    bool? HelperReturnedNormally,
    uint? HelperReturnEax);

public enum UnreliableReceiveFrameHandoffOutcome
{
    EntryNotValidated,
    MissingHelperReturnCapture,
    HelperDidNotReturnNormally,
    ReturnReached
}

public enum UnreliableReceiveFrameHandoffInstruction
{
    LeaFramePlusEightIntoEax,
    PushFramePlusEightAddress,
    LeaReceiverPlus3CIntoEcx,
    CallHelper,
    PopEdi,
    PopEsi,
    SetAlToOne,
    PopEbx,
    MoveEspFromEbp,
    PopEbp,
    RetFour
}

public readonly record struct UnreliableReceiveFrameHandoffPlan(
    UnreliableReceiveFrameHandoffCapture Capture,
    UnreliableReceiveFrameHandoffOutcome Outcome,
    uint? FramePlusEightAddress,
    uint? FrameAddressPushedToHelperStack,
    uint? ReceiverPlus3CAddressPassedInEcx,
    bool HelperCallReached,
    bool? HelperReturnedNormally,
    uint? CapturedHelperReturnEax,
    bool UnreachedHelperReturnEaxCaptureProvided,
    bool EdiPopped,
    bool EsiPopped,
    bool AlSetToOne,
    uint? EaxAfterAlSet,
    bool EbxPopped,
    bool EspRestoredFromEbp,
    bool EbpPopped,
    bool RetInstructionReached,
    uint? RetImmediateBytes,
    ImmutableArray<UnreliableReceiveFrameHandoffInstruction> InstructionsExecuted,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveFrameHandoffInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the validated frame handoff through its helper call and epilogue.
/// </summary>
public static class UnreliableReceiveFrameHandoffProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE5C0F;
    public const uint EntryVirtualAddress = 0x00FE680F;
    public const uint HelperCallRawFileOffset = 0x00BE5C16;
    public const uint HelperReturnRawFileOffset = 0x00BE5C1B;
    public const uint HelperCallTargetRawFileOffset = 0x00BE64A0;
    public const uint HelperCallTargetVirtualAddress = 0x00FE70A0;
    public const uint EndRawFileOffsetExclusive = 0x00BE5C26;
    public const uint EndVirtualAddressExclusive = 0x00FE6826;

    public const uint FramePlusEightOffset = 8;
    public const uint ReceiverPlus3COffset = 0x3C;
    public const uint RetImmediateByteCount = 4;

    public const string CapturedWindowBytes =
        "8D4508508D4F3CE8850800005F5EB0015B8BE55DC20400";

    public static ImmutableArray<UnreliableReceiveFrameHandoffInstruction> InstructionsInOrder { get; } =
        ImmutableArray.Create(
            UnreliableReceiveFrameHandoffInstruction.LeaFramePlusEightIntoEax,
            UnreliableReceiveFrameHandoffInstruction.PushFramePlusEightAddress,
            UnreliableReceiveFrameHandoffInstruction.LeaReceiverPlus3CIntoEcx,
            UnreliableReceiveFrameHandoffInstruction.CallHelper,
            UnreliableReceiveFrameHandoffInstruction.PopEdi,
            UnreliableReceiveFrameHandoffInstruction.PopEsi,
            UnreliableReceiveFrameHandoffInstruction.SetAlToOne,
            UnreliableReceiveFrameHandoffInstruction.PopEbx,
            UnreliableReceiveFrameHandoffInstruction.MoveEspFromEbp,
            UnreliableReceiveFrameHandoffInstruction.PopEbp,
            UnreliableReceiveFrameHandoffInstruction.RetFour);

    public static ImmutableArray<UnreliableReceiveFrameHandoffInstructionEvidence> Evidence { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveFrameHandoffInstructionEvidence(
                "Form the address EBP+8 in EAX",
                EntryRawFileOffset,
                RawFileOffsetToVirtualAddress(EntryRawFileOffset),
                "8D4508"),
            new UnreliableReceiveFrameHandoffInstructionEvidence(
                "Push the EBP+8 address, not the DWORD at that address",
                0x00BE5C12,
                RawFileOffsetToVirtualAddress(0x00BE5C12),
                "50"),
            new UnreliableReceiveFrameHandoffInstructionEvidence(
                "Form the address EDI+0x3C in ECX without reading from it",
                0x00BE5C13,
                RawFileOffsetToVirtualAddress(0x00BE5C13),
                "8D4F3C"),
            new UnreliableReceiveFrameHandoffInstructionEvidence(
                "Call raw 0xBE64A0; normal return continues at raw 0xBE5C1B",
                HelperCallRawFileOffset,
                RawFileOffsetToVirtualAddress(HelperCallRawFileOffset),
                "E885080000",
                HelperCallTargetRawFileOffset,
                HelperCallTargetVirtualAddress),
            new UnreliableReceiveFrameHandoffInstructionEvidence(
                "Pop EDI",
                HelperReturnRawFileOffset,
                RawFileOffsetToVirtualAddress(HelperReturnRawFileOffset),
                "5F"),
            new UnreliableReceiveFrameHandoffInstructionEvidence(
                "Pop ESI",
                0x00BE5C1C,
                RawFileOffsetToVirtualAddress(0x00BE5C1C),
                "5E"),
            new UnreliableReceiveFrameHandoffInstructionEvidence(
                "Set only AL to one between the ESI and EBX pops",
                0x00BE5C1D,
                RawFileOffsetToVirtualAddress(0x00BE5C1D),
                "B001"),
            new UnreliableReceiveFrameHandoffInstructionEvidence(
                "Pop EBX",
                0x00BE5C1F,
                RawFileOffsetToVirtualAddress(0x00BE5C1F),
                "5B"),
            new UnreliableReceiveFrameHandoffInstructionEvidence(
                "Restore ESP from EBP",
                0x00BE5C20,
                RawFileOffsetToVirtualAddress(0x00BE5C20),
                "8BE5"),
            new UnreliableReceiveFrameHandoffInstructionEvidence(
                "Pop EBP",
                0x00BE5C22,
                RawFileOffsetToVirtualAddress(0x00BE5C22),
                "5D"),
            new UnreliableReceiveFrameHandoffInstructionEvidence(
                "Return with immediate 4",
                0x00BE5C23,
                RawFileOffsetToVirtualAddress(0x00BE5C23),
                "C20400"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FramePlusEightAddress(uint frameEbp) =>
        unchecked(frameEbp + FramePlusEightOffset);

    public static uint ReceiverPlus3CAddress(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverPlus3COffset);

    public static uint RelativeCallTarget(uint instructionRawFileOffset, int displacement) =>
        unchecked(instructionRawFileOffset + 5u + (uint)displacement);

    public static UnreliableReceiveFrameHandoffPlan Project(
        UnreliableReceiveFrameHandoffCapture capture)
    {
        if (!capture.ValidatedEntry)
        {
            return Stop(
                capture,
                UnreliableReceiveFrameHandoffOutcome.EntryNotValidated,
                unreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint framePlusEightAddress = FramePlusEightAddress(capture.FrameEbp);
        uint receiverPlus3CAddress = ReceiverPlus3CAddress(capture.ReceiverEdi);
        ImmutableArray<UnreliableReceiveFrameHandoffInstruction> beforeReturn =
            InstructionsInOrder.Take(4).ToImmutableArray();

        if (capture.HelperReturnedNormally is not bool helperReturnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveFrameHandoffOutcome.MissingHelperReturnCapture,
                framePlusEightAddress,
                framePlusEightAddress,
                receiverPlus3CAddress,
                helperCallReached: true,
                unreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue,
                instructionsExecuted: beforeReturn,
                stopAt: HelperCallRawFileOffset);
        }

        if (!helperReturnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveFrameHandoffOutcome.HelperDidNotReturnNormally,
                framePlusEightAddress,
                framePlusEightAddress,
                receiverPlus3CAddress,
                helperCallReached: true,
                helperReturnedNormally: false,
                unreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue,
                instructionsExecuted: beforeReturn,
                stopAt: HelperCallRawFileOffset);
        }

        uint? eaxAfterAlSet = capture.HelperReturnEax is uint helperReturnEax
            ? (helperReturnEax & 0xFFFFFF00) | 1
            : null;
        return new UnreliableReceiveFrameHandoffPlan(
            capture,
            UnreliableReceiveFrameHandoffOutcome.ReturnReached,
            framePlusEightAddress,
            framePlusEightAddress,
            receiverPlus3CAddress,
            HelperCallReached: true,
            HelperReturnedNormally: true,
            CapturedHelperReturnEax: capture.HelperReturnEax,
            UnreachedHelperReturnEaxCaptureProvided: false,
            EdiPopped: true,
            EsiPopped: true,
            AlSetToOne: true,
            EaxAfterAlSet: eaxAfterAlSet,
            EbxPopped: true,
            EspRestoredFromEbp: true,
            EbpPopped: true,
            RetInstructionReached: true,
            RetImmediateBytes: RetImmediateByteCount,
            InstructionsExecuted: InstructionsInOrder,
            StopAtRawFileOffsetExclusive: EndRawFileOffsetExclusive);
    }

    private static UnreliableReceiveFrameHandoffPlan Stop(
        UnreliableReceiveFrameHandoffCapture capture,
        UnreliableReceiveFrameHandoffOutcome outcome,
        uint? framePlusEightAddress = null,
        uint? frameAddressPushedToHelperStack = null,
        uint? receiverPlus3CAddressPassedInEcx = null,
        bool helperCallReached = false,
        bool? helperReturnedNormally = null,
        bool unreachedHelperReturnEaxCaptureProvided = false,
        ImmutableArray<UnreliableReceiveFrameHandoffInstruction> instructionsExecuted = default,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            framePlusEightAddress,
            frameAddressPushedToHelperStack,
            receiverPlus3CAddressPassedInEcx,
            helperCallReached,
            helperReturnedNormally,
            CapturedHelperReturnEax: null,
            UnreachedHelperReturnEaxCaptureProvided: unreachedHelperReturnEaxCaptureProvided,
            EdiPopped: false,
            EsiPopped: false,
            AlSetToOne: false,
            EaxAfterAlSet: null,
            EbxPopped: false,
            EspRestoredFromEbp: false,
            EbpPopped: false,
            RetInstructionReached: false,
            RetImmediateBytes: null,
            InstructionsExecuted: instructionsExecuted.IsDefault
                ? ImmutableArray<UnreliableReceiveFrameHandoffInstruction>.Empty
                : instructionsExecuted,
            StopAtRawFileOffsetExclusive: stopAt);
}
