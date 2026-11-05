using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeExitCapture(
    bool EntryValidated,
    byte? TestedByte,
    uint? StackPopEdiValue,
    uint? StackPopEsiValue,
    uint? StackPopEbxValue,
    uint? CallCellTargetVirtualAddress,
    bool? CallReturnedNormally,
    uint? CalleeOutputEbx,
    uint? CalleeOutputEdi,
    uint? CalleeOutputEsi,
    uint? StackPopEbpValue);

public enum CapacityFinalCalleeExitOutcome
{
    EntryNotValidated,
    MissingTestedByte,
    MissingStackPopEdiValue,
    MissingStackPopEsiValue,
    MissingStackPopEbxValue,
    CallSkipped,
    MissingCallCellTarget,
    MissingCallReturnStatus,
    CallDidNotReturnNormally,
    MissingStackPopEbpValue,
    ReturnedNormally
}

public readonly record struct CapacityFinalCalleeExitInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    byte? ImmediateValue = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeExitPlan(
    CapacityFinalCalleeExitCapture Capture,
    CapacityFinalCalleeExitOutcome Outcome,
    bool TestReadReached,
    bool TestedByteCaptured,
    byte? TestedByte,
    bool? TestedBitIsSet,
    bool StackPopEdiReached,
    uint? PoppedEdi,
    bool StackPopEsiReached,
    uint? PoppedEsi,
    bool StackPopEbxReached,
    uint? PoppedEbx,
    bool BranchReached,
    bool? BranchTaken,
    bool PushImmediateReached,
    uint? PushedImmediateVirtualAddress,
    bool CallCellReadReached,
    bool CallCellTargetCaptured,
    uint? CallCellTargetVirtualAddress,
    bool CallReached,
    bool? CallReturnedNormally,
    bool CalleeOutputRegistersReached,
    uint? CalleeOutputEbx,
    uint? CalleeOutputEdi,
    uint? CalleeOutputEsi,
    bool MovEspFromEbpReached,
    bool StackPopEbpReached,
    uint? PoppedEbp,
    bool ReturnReached,
    bool UnreachedTestedByteCaptureProvided,
    bool UnreachedStackPopEdiCaptureProvided,
    bool UnreachedStackPopEsiCaptureProvided,
    bool UnreachedStackPopEbxCaptureProvided,
    bool UnreachedCallCellTargetCaptureProvided,
    bool UnreachedCallReturnStatusProvided,
    bool UnreachedCalleeOutputEbxCaptureProvided,
    bool UnreachedCalleeOutputEdiCaptureProvided,
    bool UnreachedCalleeOutputEsiCaptureProvided,
    bool UnreachedStackPopEbpCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects only the observed callee-exit window and supplied scalar captures; it does not infer
/// execution inside the indirect callee.
/// </summary>
public static class CapacityFinalCalleeExitProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x012C9441;
    public const uint EntryVirtualAddress = 0x016CA041;
    public const uint BranchDestinationRawFileOffset = 0x012C9458;
    public const uint BranchDestinationVirtualAddress = 0x016CA058;
    public const uint CallRawFileOffset = 0x012C9452;
    public const uint ReturnRawFileOffset = 0x012C945B;
    public const uint ReturnBoundaryRawFileOffsetExclusive = 0x012C945C;
    public const uint ReturnBoundaryVirtualAddressExclusive = 0x016CA05C;

    public const uint TestByteVirtualAddress = 0x01EDAF84;
    public const byte TestMask = 0x02;
    public const uint PushedImmediateVirtualAddress = 0x01EDAF88;
    public const uint CallCellVirtualAddress = 0x01AC427C;

    public const uint TestRawFileOffset = 0x012C9441;
    public const uint PopEdiRawFileOffset = 0x012C9448;
    public const uint PopEsiRawFileOffset = 0x012C9449;
    public const uint PopEbxRawFileOffset = 0x012C944A;
    public const uint BranchRawFileOffset = 0x012C944B;
    public const uint PushImmediateRawFileOffset = 0x012C944D;
    public const uint MovEspFromEbpRawFileOffset = 0x012C9458;
    public const uint PopEbpRawFileOffset = 0x012C945A;

    public const string CapturedWindowBytes =
        "F6 05 84 AF ED 01 02 5F 5E 5B 74 0B 68 88 AF ED 01 FF 15 7C 42 AC 01 8B E5 5D C3";

    public static ImmutableArray<CapacityFinalCalleeExitInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Test bit 1 of the byte at PE VA 1EDAF84",
                TestRawFileOffset, "F6 05 84 AF ED 01 02",
                peVirtualAddressOperand: TestByteVirtualAddress,
                immediateValue: TestMask),
            Instruction("Pop EDI", PopEdiRawFileOffset, "5F"),
            Instruction("Pop ESI", PopEsiRawFileOffset, "5E"),
            Instruction("Pop EBX", PopEbxRawFileOffset, "5B"),
            Instruction("JZ to the shared MOV ESP,EBP epilogue",
                BranchRawFileOffset, "74 0B",
                branchTargetRawFileOffset: BranchDestinationRawFileOffset,
                branchTargetVirtualAddress: BranchDestinationVirtualAddress),
            Instruction("Push PE VA 1EDAF88 on the nonzero fallthrough",
                PushImmediateRawFileOffset, "68 88 AF ED 01",
                peVirtualAddressOperand: PushedImmediateVirtualAddress),
            Instruction("Call through the cell at PE VA 1AC427C",
                CallRawFileOffset, "FF 15 7C 42 AC 01",
                peVirtualAddressOperand: CallCellVirtualAddress),
            Instruction("Set ESP from EBP",
                MovEspFromEbpRawFileOffset, "8B E5"),
            Instruction("Pop EBP", PopEbpRawFileOffset, "5D"),
            Instruction("Return", ReturnRawFileOffset, "C3"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeExitPlan Project(
        CapacityFinalCalleeExitCapture capture)
    {
        if (!capture.EntryValidated)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeExitOutcome.EntryNotValidated,
                stopAt: EntryRawFileOffset);
        }

        if (capture.TestedByte is not byte testedByte)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeExitOutcome.MissingTestedByte,
                testReadReached: true,
                stopAt: TestRawFileOffset);
        }

        bool testedBitIsSet = (testedByte & TestMask) != 0;
        if (capture.StackPopEdiValue is not uint poppedEdi)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeExitOutcome.MissingStackPopEdiValue,
                testReadReached: true,
                testedBitIsSet: testedBitIsSet,
                stackPopEdiReached: true,
                stopAt: PopEdiRawFileOffset);
        }

        if (capture.StackPopEsiValue is not uint poppedEsi)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeExitOutcome.MissingStackPopEsiValue,
                testReadReached: true,
                testedBitIsSet: testedBitIsSet,
                stackPopEdiReached: true,
                poppedEdi: poppedEdi,
                stackPopEsiReached: true,
                stopAt: PopEsiRawFileOffset);
        }

        if (capture.StackPopEbxValue is not uint poppedEbx)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeExitOutcome.MissingStackPopEbxValue,
                testReadReached: true,
                testedBitIsSet: testedBitIsSet,
                stackPopEdiReached: true,
                poppedEdi: poppedEdi,
                stackPopEsiReached: true,
                poppedEsi: poppedEsi,
                stackPopEbxReached: true,
                stopAt: PopEbxRawFileOffset);
        }

        bool branchTaken = !testedBitIsSet;
        if (branchTaken)
        {
            return ProjectEpilogue(
                capture,
                CapacityFinalCalleeExitOutcome.CallSkipped,
                testedBitIsSet,
                poppedEdi,
                poppedEsi,
                poppedEbx,
                pushImmediateReached: false,
                callCellReadReached: false,
                callCellTargetCaptured: false,
                callCellTargetVirtualAddress: null,
                callReached: false,
                callReturnedNormally: null,
                calleeOutputRegistersReached: false,
                calleeOutputEbx: null,
                calleeOutputEdi: null,
                calleeOutputEsi: null);
        }

        if (capture.CallCellTargetVirtualAddress is not uint callCellTarget)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeExitOutcome.MissingCallCellTarget,
                testReadReached: true,
                testedBitIsSet: true,
                stackPopEdiReached: true,
                poppedEdi: poppedEdi,
                stackPopEsiReached: true,
                poppedEsi: poppedEsi,
                stackPopEbxReached: true,
                poppedEbx: poppedEbx,
                branchReached: true,
                branchTaken: false,
                pushImmediateReached: true,
                pushedImmediateVirtualAddress: PushedImmediateVirtualAddress,
                callCellReadReached: true,
                stopAt: CallRawFileOffset);
        }

        if (capture.CallReturnedNormally is not bool callReturnedNormally)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeExitOutcome.MissingCallReturnStatus,
                testReadReached: true,
                testedBitIsSet: true,
                stackPopEdiReached: true,
                poppedEdi: poppedEdi,
                stackPopEsiReached: true,
                poppedEsi: poppedEsi,
                stackPopEbxReached: true,
                poppedEbx: poppedEbx,
                branchReached: true,
                branchTaken: false,
                pushImmediateReached: true,
                pushedImmediateVirtualAddress: PushedImmediateVirtualAddress,
                callCellReadReached: true,
                callCellTargetCaptured: true,
                callCellTargetVirtualAddress: callCellTarget,
                callReached: true,
                stopAt: CallRawFileOffset);
        }

        if (!callReturnedNormally)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeExitOutcome.CallDidNotReturnNormally,
                testReadReached: true,
                testedBitIsSet: true,
                stackPopEdiReached: true,
                poppedEdi: poppedEdi,
                stackPopEsiReached: true,
                poppedEsi: poppedEsi,
                stackPopEbxReached: true,
                poppedEbx: poppedEbx,
                branchReached: true,
                branchTaken: false,
                pushImmediateReached: true,
                pushedImmediateVirtualAddress: PushedImmediateVirtualAddress,
                callCellReadReached: true,
                callCellTargetCaptured: true,
                callCellTargetVirtualAddress: callCellTarget,
                callReached: true,
                callReturnedNormally: false,
                stopAt: CallRawFileOffset);
        }

        return ProjectEpilogue(
            capture,
            CapacityFinalCalleeExitOutcome.ReturnedNormally,
            testedBitIsSet: true,
            poppedEdi,
            poppedEsi,
            poppedEbx,
            pushImmediateReached: true,
            callCellReadReached: true,
            callCellTargetCaptured: true,
            callCellTargetVirtualAddress: callCellTarget,
            callReached: true,
            callReturnedNormally: true,
            calleeOutputRegistersReached: true,
            calleeOutputEbx: capture.CalleeOutputEbx,
            calleeOutputEdi: capture.CalleeOutputEdi,
            calleeOutputEsi: capture.CalleeOutputEsi);
    }

    private static CapacityFinalCalleeExitPlan ProjectEpilogue(
        CapacityFinalCalleeExitCapture capture,
        CapacityFinalCalleeExitOutcome successOutcome,
        bool testedBitIsSet,
        uint poppedEdi,
        uint poppedEsi,
        uint poppedEbx,
        bool pushImmediateReached,
        bool callCellReadReached,
        bool callCellTargetCaptured,
        uint? callCellTargetVirtualAddress,
        bool callReached,
        bool? callReturnedNormally,
        bool calleeOutputRegistersReached,
        uint? calleeOutputEbx,
        uint? calleeOutputEdi,
        uint? calleeOutputEsi)
    {
        if (capture.StackPopEbpValue is not uint poppedEbp)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeExitOutcome.MissingStackPopEbpValue,
                testReadReached: true,
                testedBitIsSet: testedBitIsSet,
                stackPopEdiReached: true,
                poppedEdi: poppedEdi,
                stackPopEsiReached: true,
                poppedEsi: poppedEsi,
                stackPopEbxReached: true,
                poppedEbx: poppedEbx,
                branchReached: true,
                branchTaken: !testedBitIsSet,
                pushImmediateReached: pushImmediateReached,
                pushedImmediateVirtualAddress: pushImmediateReached
                    ? PushedImmediateVirtualAddress
                    : null,
                callCellReadReached: callCellReadReached,
                callCellTargetCaptured: callCellTargetCaptured,
                callCellTargetVirtualAddress: callCellTargetVirtualAddress,
                callReached: callReached,
                callReturnedNormally: callReturnedNormally,
                calleeOutputRegistersReached: calleeOutputRegistersReached,
                calleeOutputEbx: calleeOutputEbx,
                calleeOutputEdi: calleeOutputEdi,
                calleeOutputEsi: calleeOutputEsi,
                movEspFromEbpReached: true,
                stackPopEbpReached: true,
                stopAt: PopEbpRawFileOffset);
        }

        return BuildPlan(
            capture,
            successOutcome,
            testReadReached: true,
            testedBitIsSet: testedBitIsSet,
            stackPopEdiReached: true,
            poppedEdi: poppedEdi,
            stackPopEsiReached: true,
            poppedEsi: poppedEsi,
            stackPopEbxReached: true,
            poppedEbx: poppedEbx,
            branchReached: true,
            branchTaken: !testedBitIsSet,
            pushImmediateReached: pushImmediateReached,
            pushedImmediateVirtualAddress: pushImmediateReached
                ? PushedImmediateVirtualAddress
                : null,
            callCellReadReached: callCellReadReached,
            callCellTargetCaptured: callCellTargetCaptured,
            callCellTargetVirtualAddress: callCellTargetVirtualAddress,
            callReached: callReached,
            callReturnedNormally: callReturnedNormally,
            calleeOutputRegistersReached: calleeOutputRegistersReached,
            calleeOutputEbx: calleeOutputEbx,
            calleeOutputEdi: calleeOutputEdi,
            calleeOutputEsi: calleeOutputEsi,
            movEspFromEbpReached: true,
            stackPopEbpReached: true,
            poppedEbp: poppedEbp,
            returnReached: true,
            stopAt: ReturnBoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeExitInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null,
        byte? immediateValue = null,
        uint? branchTargetRawFileOffset = null,
        uint? branchTargetVirtualAddress = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            peVirtualAddressOperand,
            immediateValue,
            branchTargetRawFileOffset,
            branchTargetVirtualAddress);

    private static CapacityFinalCalleeExitPlan BuildPlan(
        CapacityFinalCalleeExitCapture capture,
        CapacityFinalCalleeExitOutcome outcome,
        bool testReadReached = false,
        bool? testedBitIsSet = null,
        bool stackPopEdiReached = false,
        uint? poppedEdi = null,
        bool stackPopEsiReached = false,
        uint? poppedEsi = null,
        bool stackPopEbxReached = false,
        uint? poppedEbx = null,
        bool branchReached = false,
        bool? branchTaken = null,
        bool pushImmediateReached = false,
        uint? pushedImmediateVirtualAddress = null,
        bool callCellReadReached = false,
        bool callCellTargetCaptured = false,
        uint? callCellTargetVirtualAddress = null,
        bool callReached = false,
        bool? callReturnedNormally = null,
        bool calleeOutputRegistersReached = false,
        uint? calleeOutputEbx = null,
        uint? calleeOutputEdi = null,
        uint? calleeOutputEsi = null,
        bool movEspFromEbpReached = false,
        bool stackPopEbpReached = false,
        uint? poppedEbp = null,
        bool returnReached = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            testReadReached,
            testReadReached && capture.TestedByte.HasValue,
            testReadReached ? capture.TestedByte : null,
            testedBitIsSet,
            stackPopEdiReached,
            poppedEdi,
            stackPopEsiReached,
            poppedEsi,
            stackPopEbxReached,
            poppedEbx,
            branchReached,
            branchTaken,
            pushImmediateReached,
            pushedImmediateVirtualAddress,
            callCellReadReached,
            callCellTargetCaptured,
            callCellTargetVirtualAddress,
            callReached,
            callReached ? callReturnedNormally : null,
            calleeOutputRegistersReached,
            calleeOutputRegistersReached ? calleeOutputEbx : null,
            calleeOutputRegistersReached ? calleeOutputEdi : null,
            calleeOutputRegistersReached ? calleeOutputEsi : null,
            movEspFromEbpReached,
            stackPopEbpReached,
            poppedEbp,
            returnReached,
            !testReadReached && capture.TestedByte.HasValue,
            !stackPopEdiReached && capture.StackPopEdiValue.HasValue,
            !stackPopEsiReached && capture.StackPopEsiValue.HasValue,
            !stackPopEbxReached && capture.StackPopEbxValue.HasValue,
            !callCellReadReached && capture.CallCellTargetVirtualAddress.HasValue,
            !callReached && capture.CallReturnedNormally.HasValue,
            !calleeOutputRegistersReached && capture.CalleeOutputEbx.HasValue,
            !calleeOutputRegistersReached && capture.CalleeOutputEdi.HasValue,
            !calleeOutputRegistersReached && capture.CalleeOutputEsi.HasValue,
            !stackPopEbpReached && capture.StackPopEbpValue.HasValue,
            stopAt);
}
