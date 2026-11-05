using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCleanupCounterGateCapture(
    bool EntryValidated,
    uint? FreshCounterDword,
    bool? IncomingCarryFlag);

public enum CapacityFinalCalleeCleanupCounterGateOutcome
{
    EntryNotValidated,
    MissingFreshCounterDword,
    DecrementedToZero,
    DecrementedNonzero
}

public readonly record struct CapacityFinalCalleeCleanupCounterGateFlags(
    bool? Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public enum CapacityFinalCalleeCleanupCounterGateAccessKind
{
    Read,
    Write
}

public readonly record struct CapacityFinalCalleeCleanupCounterGateCounterAccess(
    uint RawFileOffset,
    CapacityFinalCalleeCleanupCounterGateAccessKind Kind,
    uint Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeCleanupCounterGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? ImmediateValue = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCleanupCounterGatePlan(
    CapacityFinalCalleeCleanupCounterGateCapture Capture,
    CapacityFinalCalleeCleanupCounterGateOutcome Outcome)
{
    public bool CounterReadReached { get; init; }
    public bool FreshCounterDwordCaptured { get; init; }
    public bool CounterWriteReached { get; init; }
    public uint? CounterBefore { get; init; }
    public uint? CounterAfter { get; init; }
    public ImmutableArray<CapacityFinalCalleeCleanupCounterGateCounterAccess>
        CounterAccesses { get; init; } =
            ImmutableArray<CapacityFinalCalleeCleanupCounterGateCounterAccess>.Empty;
    public CapacityFinalCalleeCleanupCounterGateFlags? FlagsAfterDecrement { get; init; }
    public bool? CarryAtStop =>
        FlagsAfterDecrement?.Carry ?? Capture.IncomingCarryFlag;
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? SelectedDestinationRawFileOffset { get; init; }
    public uint? SelectedDestinationVirtualAddress { get; init; }
    public bool MovEcxReached { get; init; }
    public uint? EcxValue { get; init; }
    public bool CallReached { get; init; }
    public bool CalleeEntered { get; init; }
    public bool UnreachedFreshCounterDwordCaptureProvided { get; init; }
    public bool UnreachedIncomingCarryFlagCaptureProvided { get; init; }
    public bool ExactBoundedGateCompletion { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the cleanup-counter DEC/JNZ gate and stops before its selected destination
/// or the following direct call.
/// </summary>
public static class CapacityFinalCalleeCleanupCounterGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C942F;
    public const uint EntryFlatIdaAddress = EntryRawFileOffset;
    public const uint EntryVirtualAddress = 0x016CA02F;
    public const uint CounterDecrementRawFileOffset = EntryRawFileOffset;
    public const uint JnzRawFileOffset = 0x012C9435;
    public const uint ZeroPathRawFileOffset = 0x012C9437;
    public const uint MovEcxRawFileOffset = ZeroPathRawFileOffset;
    public const uint CallRawFileOffset = 0x012C943C;
    public const uint BoundaryRawFileOffsetExclusive = CallRawFileOffset;
    public const uint NonzeroBranchTargetRawFileOffset = 0x012C9441;
    public const uint CounterVirtualAddress = 0x01EDADE8;
    public const uint ZeroPathEcxValue = 0x01EDADC8;
    public const uint DirectCallTargetRawFileOffset = 0x012C78A0;
    public const uint DirectCallTargetVirtualAddress = 0x016C84A0;
    public const byte JnzRelativeDisplacement = 0x0A;

    public const string CapturedWindowBytes =
        "FF 0D E8 AD ED 01 75 0A B9 C8 AD ED 01 E8 5F E4 FF FF";

    public static ImmutableArray<CapacityFinalCalleeCleanupCounterGateInstructionEvidence>
        Instructions { get; } =
            ImmutableArray.Create(
                Instruction(
                    "Decrement the dword at PE VA 1EDADE8",
                    CounterDecrementRawFileOffset,
                    "FF 0D E8 AD ED 01",
                    peVirtualAddressOperand: CounterVirtualAddress),
                Instruction(
                    "Jump to raw 12C9441 when the decremented counter is nonzero",
                    JnzRawFileOffset,
                    "75 0A",
                    branchTargetRawFileOffset: NonzeroBranchTargetRawFileOffset),
                Instruction(
                    "Load PE VA 1EDADC8 into ECX on the zero fallthrough",
                    MovEcxRawFileOffset,
                    "B9 C8 AD ED 01",
                    immediateValue: ZeroPathEcxValue),
                Instruction(
                    "Direct call to raw 12C78A0 follows the bounded window",
                    CallRawFileOffset,
                    "E8 5F E4 FF FF",
                    branchTargetRawFileOffset: DirectCallTargetRawFileOffset,
                    branchTargetVirtualAddress: DirectCallTargetVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint CalculateJnzTargetRawFileOffset() =>
        unchecked(ZeroPathRawFileOffset + JnzRelativeDisplacement);

    public static CapacityFinalCalleeCleanupCounterGatePlan Project(
        CapacityFinalCalleeCleanupCounterGateCapture capture)
    {
        var counterAccesses =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCleanupCounterGateCounterAccess>();
        var reached = ImmutableArray.CreateBuilder<uint>();
        bool counterReadReached = false;
        bool freshCounterCaptured = false;
        bool counterWriteReached = false;
        bool jnzReached = false;
        bool movEcxReached = false;

        CapacityFinalCalleeCleanupCounterGatePlan Finish(
            CapacityFinalCalleeCleanupCounterGateOutcome outcome,
            uint stopAt,
            uint? counterBefore = null,
            uint? counterAfter = null,
            CapacityFinalCalleeCleanupCounterGateFlags? flags = null,
            bool? jnzTaken = null,
            uint? selectedDestination = null,
            uint? ecxValue = null,
            bool exactBoundedGateCompletion = false) =>
                new(capture, outcome)
                {
                    CounterReadReached = counterReadReached,
                    FreshCounterDwordCaptured = freshCounterCaptured,
                    CounterWriteReached = counterWriteReached,
                    CounterBefore = counterBefore,
                    CounterAfter = counterAfter,
                    CounterAccesses = counterAccesses.ToImmutable(),
                    FlagsAfterDecrement = flags,
                    JnzReached = jnzReached,
                    JnzTaken = jnzTaken,
                    SelectedDestinationRawFileOffset = selectedDestination,
                    SelectedDestinationVirtualAddress =
                        selectedDestination is uint rawDestination
                            ? RawFileOffsetToVirtualAddress(rawDestination)
                            : null,
                    MovEcxReached = movEcxReached,
                    EcxValue = ecxValue,
                    CallReached = false,
                    CalleeEntered = false,
                    UnreachedFreshCounterDwordCaptureProvided =
                        !counterReadReached && capture.FreshCounterDword.HasValue,
                    UnreachedIncomingCarryFlagCaptureProvided =
                        !capture.EntryValidated && capture.IncomingCarryFlag.HasValue,
                    ExactBoundedGateCompletion = exactBoundedGateCompletion,
                    ReachedInstructions = reached.ToImmutable(),
                    StopAtRawFileOffsetExclusive = stopAt
                };

        if (!capture.EntryValidated)
        {
            return Finish(
                CapacityFinalCalleeCleanupCounterGateOutcome.EntryNotValidated,
                EntryRawFileOffset);
        }

        counterReadReached = true;
        if (capture.FreshCounterDword is not uint counterBefore)
        {
            counterAccesses.Add(new(
                CounterDecrementRawFileOffset,
                CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
                CounterVirtualAddress,
                ValueCaptured: false,
                Value: null));
            return Finish(
                CapacityFinalCalleeCleanupCounterGateOutcome.MissingFreshCounterDword,
                CounterDecrementRawFileOffset);
        }

        freshCounterCaptured = true;
        counterAccesses.Add(new(
            CounterDecrementRawFileOffset,
            CapacityFinalCalleeCleanupCounterGateAccessKind.Read,
            CounterVirtualAddress,
            ValueCaptured: true,
            Value: counterBefore));
        reached.Add(CounterDecrementRawFileOffset);

        uint counterAfter = unchecked(counterBefore - 1);
        counterWriteReached = true;
        counterAccesses.Add(new(
            CounterDecrementRawFileOffset,
            CapacityFinalCalleeCleanupCounterGateAccessKind.Write,
            CounterVirtualAddress,
            ValueCaptured: true,
            Value: counterAfter));

        CapacityFinalCalleeCleanupCounterGateFlags flags =
            new(
                Carry: capture.IncomingCarryFlag,
                Parity: (BitOperations.PopCount((uint)(byte)counterAfter) & 1) == 0,
                AuxiliaryCarry: (counterBefore & 0x0F) == 0,
                Zero: counterAfter == 0,
                Sign: (counterAfter & 0x80000000) != 0,
                Overflow: counterBefore == 0x80000000);
        jnzReached = true;
        reached.Add(JnzRawFileOffset);
        bool jnzTaken = counterAfter != 0;
        if (jnzTaken)
        {
            return Finish(
                CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedNonzero,
                NonzeroBranchTargetRawFileOffset,
                counterBefore,
                counterAfter,
                flags,
                jnzTaken: true,
                selectedDestination: NonzeroBranchTargetRawFileOffset,
                exactBoundedGateCompletion: true);
        }

        movEcxReached = true;
        reached.Add(MovEcxRawFileOffset);
        return Finish(
            CapacityFinalCalleeCleanupCounterGateOutcome.DecrementedToZero,
            BoundaryRawFileOffsetExclusive,
            counterBefore,
            counterAfter,
            flags,
            jnzTaken: false,
            selectedDestination: ZeroPathRawFileOffset,
            ecxValue: ZeroPathEcxValue,
            exactBoundedGateCompletion: true);
    }

    private static CapacityFinalCalleeCleanupCounterGateInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null,
        uint? immediateValue = null,
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
                branchTargetVirtualAddress
                    ?? (branchTargetRawFileOffset is uint targetRaw
                        ? RawFileOffsetToVirtualAddress(targetRaw)
                        : null));
}
