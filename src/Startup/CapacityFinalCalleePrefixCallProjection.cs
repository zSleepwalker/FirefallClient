using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleePrefixCallCapture(
    bool CaptureValidatedNonzeroEntry,
    uint EntryECX,
    bool? CapturedPriorTestMaskSet,
    uint? CallCellTargetVirtualAddress,
    bool? CallReturnedNormally);

public enum CapacityFinalCalleePrefixCallOutcome
{
    EntryNotValidated,
    ZeroEntryEcx,
    MissingPriorTestMaskCapture,
    CallSkipped,
    MissingCallCellTarget,
    MissingCallReturnStatus,
    CallDidNotReturnNormally,
    CallReturnedNormally
}

public readonly record struct CapacityFinalCalleePrefixCallInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    byte? ImmediateValue = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleePrefixCallPlan(
    CapacityFinalCalleePrefixCallCapture Capture,
    CapacityFinalCalleePrefixCallOutcome Outcome,
    bool LeaEsiReached,
    uint? DerivedPrecallESI,
    bool BranchReached,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset,
    bool PushImmediateReached,
    uint? PushedImmediateVirtualAddress,
    bool CallCellReadReached,
    bool CallCellTargetCaptured,
    uint? CallCellTargetVirtualAddress,
    bool CallReached,
    bool? CallReturnedNormally,
    uint? OutgoingESI,
    bool UnreachedPriorTestMaskCaptureProvided,
    bool UnreachedCallCellTargetCaptureProvided,
    bool UnreachedCallReturnStatusProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the final callee's LEA/JZ/push/indirect-call window. The JZ uses the explicitly
/// captured prior TEST result; no flags are reloaded here, and call outputs remain opaque.
/// </summary>
public static class CapacityFinalCalleePrefixCallProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8D78;
    public const uint EntryVirtualAddress = 0x016C9978;
    public const uint BranchRawFileOffset = 0x012C8D7B;
    public const uint PushImmediateRawFileOffset = 0x012C8D7D;
    public const uint CallRawFileOffset = 0x012C8D82;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8D88;
    public const uint BoundaryVirtualAddressExclusive = 0x016C9988;
    public const uint BranchDestinationRawFileOffset = 0x012C8D88;
    public const uint BranchDestinationVirtualAddress = 0x016C9988;

    public const uint PushedImmediateVirtualAddress = 0x01EDAF88;
    public const uint CallCellVirtualAddress = 0x01AC4278;
    public const uint EsiDisplacement = 8;

    public const string CapturedWindowBytes =
        "8D 71 F8 74 0B 68 88 AF ED 01 FF 15 78 42 AC 01";

    public static ImmutableArray<CapacityFinalCalleePrefixCallInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("LEA ESI,[ECX-8] from entry ECX",
                EntryRawFileOffset, "8D 71 F8"),
            Instruction("JZ to the exclusive window boundary using flags from the prior TEST",
                BranchRawFileOffset, "74 0B",
                branchTargetRawFileOffset: BranchDestinationRawFileOffset,
                branchTargetVirtualAddress: BranchDestinationVirtualAddress),
            Instruction("Push PE VA 1EDAF88 on the nonzero fallthrough",
                PushImmediateRawFileOffset, "68 88 AF ED 01",
                peVirtualAddressOperand: PushedImmediateVirtualAddress),
            Instruction("Call through the cell at PE VA 1AC4278",
                CallRawFileOffset, "FF 15 78 42 AC 01",
                peVirtualAddressOperand: CallCellVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DerivePrecallESI(uint entryEcx) =>
        unchecked(entryEcx - EsiDisplacement);

    public static CapacityFinalCalleePrefixCallPlan Project(
        CapacityFinalCalleePrefixCallCapture capture)
    {
        if (!capture.CaptureValidatedNonzeroEntry)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleePrefixCallOutcome.EntryNotValidated,
                unreachedPriorTestMaskCaptureProvided:
                    capture.CapturedPriorTestMaskSet.HasValue,
                stopAt: EntryRawFileOffset);
        }

        if (capture.EntryECX == 0)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleePrefixCallOutcome.ZeroEntryEcx,
                unreachedPriorTestMaskCaptureProvided:
                    capture.CapturedPriorTestMaskSet.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint derivedPrecallEsi = DerivePrecallESI(capture.EntryECX);
        if (capture.CapturedPriorTestMaskSet is not bool priorTestMaskSet)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleePrefixCallOutcome.MissingPriorTestMaskCapture,
                leaEsiReached: true,
                derivedPrecallEsi: derivedPrecallEsi,
                stopAt: BranchRawFileOffset);
        }

        if (!priorTestMaskSet)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleePrefixCallOutcome.CallSkipped,
                leaEsiReached: true,
                derivedPrecallEsi: derivedPrecallEsi,
                branchReached: true,
                branchTaken: true,
                selectedDestinationRawFileOffset: BranchDestinationRawFileOffset,
                outgoingEsi: derivedPrecallEsi,
                stopAt: BoundaryRawFileOffsetExclusive);
        }

        if (capture.CallCellTargetVirtualAddress is not uint callCellTarget)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleePrefixCallOutcome.MissingCallCellTarget,
                leaEsiReached: true,
                derivedPrecallEsi: derivedPrecallEsi,
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
                CapacityFinalCalleePrefixCallOutcome.MissingCallReturnStatus,
                leaEsiReached: true,
                derivedPrecallEsi: derivedPrecallEsi,
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
                CapacityFinalCalleePrefixCallOutcome.CallDidNotReturnNormally,
                leaEsiReached: true,
                derivedPrecallEsi: derivedPrecallEsi,
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

        return BuildPlan(
            capture,
            CapacityFinalCalleePrefixCallOutcome.CallReturnedNormally,
            leaEsiReached: true,
            derivedPrecallEsi: derivedPrecallEsi,
            branchReached: true,
            branchTaken: false,
            pushImmediateReached: true,
            pushedImmediateVirtualAddress: PushedImmediateVirtualAddress,
            callCellReadReached: true,
            callCellTargetCaptured: true,
            callCellTargetVirtualAddress: callCellTarget,
            callReached: true,
            callReturnedNormally: true,
            stopAt: BoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleePrefixCallInstructionEvidence Instruction(
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

    private static CapacityFinalCalleePrefixCallPlan BuildPlan(
        CapacityFinalCalleePrefixCallCapture capture,
        CapacityFinalCalleePrefixCallOutcome outcome,
        bool leaEsiReached = false,
        uint? derivedPrecallEsi = null,
        bool branchReached = false,
        bool? branchTaken = null,
        uint? selectedDestinationRawFileOffset = null,
        bool pushImmediateReached = false,
        uint? pushedImmediateVirtualAddress = null,
        bool callCellReadReached = false,
        bool callCellTargetCaptured = false,
        uint? callCellTargetVirtualAddress = null,
        bool callReached = false,
        bool? callReturnedNormally = null,
        uint? outgoingEsi = null,
        bool unreachedPriorTestMaskCaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            leaEsiReached,
            leaEsiReached ? derivedPrecallEsi : null,
            branchReached,
            branchTaken,
            selectedDestinationRawFileOffset,
            pushImmediateReached,
            pushedImmediateVirtualAddress,
            callCellReadReached,
            callCellTargetCaptured,
            callCellTargetVirtualAddress,
            callReached,
            callReached ? callReturnedNormally : null,
            branchTaken == true ? outgoingEsi : null,
            (!branchReached && capture.CapturedPriorTestMaskSet.HasValue)
                || unreachedPriorTestMaskCaptureProvided,
            !callCellReadReached && capture.CallCellTargetVirtualAddress.HasValue,
            !callReached && capture.CallReturnedNormally.HasValue,
            stopAt);
}
