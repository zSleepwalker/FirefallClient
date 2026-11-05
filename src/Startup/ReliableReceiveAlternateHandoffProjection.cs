using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct ReliableReceiveAlternateHandoffCapture(
    uint ReceiverBaseAddress,
    uint CapturedPayloadWordAddress,
    ushort CapturedDelta,
    bool? HelperReturnedNormally);

public enum ReliableReceiveAlternateHandoffOutcome
{
    MissingHelperReturnCapture,
    HelperDidNotReturnNormally,
    BranchTaken,
    FallthroughSelected
}

public readonly record struct ReliableReceiveAlternateHandoffPlan(
    ReliableReceiveAlternateHandoffCapture Capture,
    ReliableReceiveAlternateHandoffOutcome Outcome,
    bool CapturedPayloadAddressPushed,
    uint? PushedPayloadWordAddress,
    uint? HelperEcX,
    bool HelperCallReached,
    bool? HelperReturnedNormally,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveAlternateHandoffInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the alternate reliable-receive handoff after its prior unsigned-JA
/// delta gate, stopping at its helper-return gate or the following JB destination.
/// </summary>
public static class ReliableReceiveAlternateHandoffProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint HandoffStartRawFileOffset = 0x00BE528F;
    public const uint HandoffStartVirtualAddress = 0x00FE5E8F;
    public const uint HandoffEndRawFileOffsetExclusive = 0x00BE52A5;
    public const uint PayloadAddressPushRawFileOffset = 0x00BE528F;
    public const uint ReceiverEcXLoadRawFileOffset = 0x00BE5290;
    public const uint OpaqueHelperCallRawFileOffset = 0x00BE5296;
    public const uint OpaqueHelperCallReturnRawFileOffset = 0x00BE529B;
    public const uint DeltaCompareRawFileOffset = 0x00BE52A0;
    public const uint BranchRawFileOffset = 0x00BE52A3;
    public const uint BranchVirtualAddress = 0x00FE5EA3;
    public const uint BranchTakenDestinationRawFileOffset = 0x00BE52DF;
    public const uint BranchTakenDestinationVirtualAddress = 0x00FE5EDF;
    public const uint FallthroughDestinationRawFileOffset = HandoffEndRawFileOffsetExclusive;
    public const uint FallthroughDestinationVirtualAddress = 0x00FE5EA5;
    public const uint ReceiverEcXOffset = 0x047C;
    public const ushort CompareImmediate = 0x8000;

    public const string CapturedHandoffBytes =
        "51 8D 8E 7C 04 00 00 E8 65 10 00 00 B8 00 80 00 00 "
        + "66 3B D8 72 3A";

    public static ImmutableArray<ReliableReceiveAlternateHandoffInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveAlternateHandoffInstructionEvidence(
                "Push the captured payload-word address from the earlier LEA; do not read the word here",
                PayloadAddressPushRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadAddressPushRawFileOffset),
                "51"),
            new ReliableReceiveAlternateHandoffInstructionEvidence(
                "Load ECX with the wrapping 32-bit receiver address plus 0x47C",
                ReceiverEcXLoadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverEcXLoadRawFileOffset),
                "8D 8E 7C 04 00 00"),
            new ReliableReceiveAlternateHandoffInstructionEvidence(
                "Call the opaque helper; only the observed ECX and pushed address are modeled",
                OpaqueHelperCallRawFileOffset,
                RawFileOffsetToVirtualAddress(OpaqueHelperCallRawFileOffset),
                "E8 65 10 00 00"),
            new ReliableReceiveAlternateHandoffInstructionEvidence(
                "On normal helper return, load EAX with DWORD 0x8000",
                OpaqueHelperCallReturnRawFileOffset,
                RawFileOffsetToVirtualAddress(OpaqueHelperCallReturnRawFileOffset),
                "B8 00 80 00 00"),
            new ReliableReceiveAlternateHandoffInstructionEvidence(
                "Compare the previously captured modular delta in BX against AX",
                DeltaCompareRawFileOffset,
                RawFileOffsetToVirtualAddress(DeltaCompareRawFileOffset),
                "66 3B D8"),
            new ReliableReceiveAlternateHandoffInstructionEvidence(
                "Unsigned JB selects raw 0xBE52DF; not-taken falls through to raw 0xBE52A5",
                BranchRawFileOffset,
                BranchVirtualAddress,
                "72 3A",
                BranchTakenDestinationRawFileOffset,
                BranchTakenDestinationVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverEcXAddress(uint receiverBaseAddress) =>
        unchecked(receiverBaseAddress + ReceiverEcXOffset);

    public static uint RelativeCallTarget(uint instructionRawFileOffset, int displacement) =>
        unchecked(instructionRawFileOffset + 5u + (uint)displacement);

    public static bool IsValidEntryDelta(ushort capturedDelta) =>
        capturedDelta < 0x0100 || capturedDelta > 0x7FFF;

    public static ReliableReceiveAlternateHandoffPlan Project(
        ReliableReceiveAlternateHandoffCapture capture)
    {
        if (!IsValidEntryDelta(capture.CapturedDelta))
        {
            throw new ArgumentOutOfRangeException(
                nameof(capture),
                capture.CapturedDelta,
                "The alternate handoff is entered only for delta < 0x100 or delta > 0x7FFF.");
        }

        uint helperEcX = ReceiverEcXAddress(capture.ReceiverBaseAddress);
        if (capture.HelperReturnedNormally is not bool helperReturnedNormally)
        {
            return new ReliableReceiveAlternateHandoffPlan(
                capture,
                ReliableReceiveAlternateHandoffOutcome.MissingHelperReturnCapture,
                CapturedPayloadAddressPushed: true,
                capture.CapturedPayloadWordAddress,
                helperEcX,
                HelperCallReached: true,
                HelperReturnedNormally: null,
                BranchTaken: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: OpaqueHelperCallReturnRawFileOffset);
        }

        if (!helperReturnedNormally)
        {
            return new ReliableReceiveAlternateHandoffPlan(
                capture,
                ReliableReceiveAlternateHandoffOutcome.HelperDidNotReturnNormally,
                CapturedPayloadAddressPushed: true,
                capture.CapturedPayloadWordAddress,
                helperEcX,
                HelperCallReached: true,
                HelperReturnedNormally: false,
                BranchTaken: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: OpaqueHelperCallReturnRawFileOffset);
        }

        bool branchTaken = capture.CapturedDelta < CompareImmediate;
        return new ReliableReceiveAlternateHandoffPlan(
            capture,
            branchTaken
                ? ReliableReceiveAlternateHandoffOutcome.BranchTaken
                : ReliableReceiveAlternateHandoffOutcome.FallthroughSelected,
            CapturedPayloadAddressPushed: true,
            capture.CapturedPayloadWordAddress,
            helperEcX,
            HelperCallReached: true,
            HelperReturnedNormally: true,
            branchTaken,
            branchTaken
                ? BranchTakenDestinationRawFileOffset
                : FallthroughDestinationRawFileOffset,
            branchTaken
                ? BranchTakenDestinationRawFileOffset
                : FallthroughDestinationRawFileOffset);
    }
}
