using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonIndexedAddressGateCapture(
    uint? EntryECX,
    uint? EntryEDI);

public enum CapacityFinalCalleeCommonIndexedAddressGateOutcome
{
    MissingEntryECX,
    MissingEntryEDI,
    BelowUnsignedThresholdToCleanBoundedEdge,
    AtOrAboveUnsignedThresholdToCleanBoundedEdge
}

public readonly record struct CapacityFinalCalleeCommonIndexedAddressGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeCommonIndexedAddressGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeCommonIndexedAddressGatePlan(
    CapacityFinalCalleeCommonIndexedAddressGateCapture Capture,
    CapacityFinalCalleeCommonIndexedAddressGateOutcome Outcome)
{
    public bool EntryECXCaptured { get; init; }
    public bool LEAReached { get; init; }
    public uint? LEAIndex { get; init; }
    public uint? LEAScaledIndex { get; init; }
    public uint? LEABaseAddress { get; init; }
    public uint? DerivedEDX { get; init; }
    public bool AddressOnlyLEA { get; init; }
    public bool MemoryReadPerformed => false;
    public bool MoveEAXReached { get; init; }
    public uint? DerivedEAX { get; init; }
    public bool EntryEDICaptured { get; init; }
    public bool UnreachedEntryEDICaptureProvided =>
        !EntryECXCaptured && Capture.EntryEDI.HasValue;
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeCommonIndexedAddressGateFlags? CompareFlags { get; init; }
    public bool JnbReached { get; init; }
    public bool? JnbTaken { get; init; }
    public uint? JnbSelectedDestinationRawFileOffset { get; init; }
    public bool ExactCleanBoundedEdge { get; init; }
    public bool OpaqueCallInstructionAtStop { get; init; }
    public bool OpaqueCallEntered => false;
    public bool OpaqueCallReturnObserved => false;
    public bool CalleeIdentityKnown => false;
    public ImmutableArray<uint> Writes { get; init; } = ImmutableArray<uint>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the address-only LEA and bounded CMP/JNB gate at 92F6 without entering the opaque call.
/// </summary>
public static class CapacityFinalCalleeCommonIndexedAddressGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C92F6;
    public const uint EntryVirtualAddress = 0x016C9EF6;
    public const uint LeaRawFileOffset = 0x012C92F6;
    public const uint MoveRawFileOffset = 0x012C92FD;
    public const uint CompareRawFileOffset = 0x012C92FF;
    public const uint JnbRawFileOffset = 0x012C9302;
    public const uint OpaqueCallRawFileOffset = 0x012C9304;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C930A;

    public const uint LEABaseVirtualAddress = 0x01EDADF0;
    public const uint CompareThreshold = 0x10;
    public const uint BranchTargetRawFileOffset = 0x012C930A;
    public const uint FallthroughRawFileOffset = 0x012C9304;

    public const string CapturedWindowBytes =
        "8D 14 CD F0 AD ED 01 8B C2 83 FF 10 73 06 FF 15 24 4A AC 01";

    public static ImmutableArray<CapacityFinalCalleeCommonIndexedAddressGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Form address EDX = ECX * 8 + 01EDADF0h with LEA; do not read memory",
                LeaRawFileOffset,
                "8D 14 CD F0 AD ED 01"),
            Instruction(
                "Copy the derived address from EDX to EAX",
                MoveRawFileOffset,
                "8B C2"),
            Instruction(
                "Compare entry EDI with unsigned threshold 10h",
                CompareRawFileOffset,
                "83 FF 10"),
            Instruction(
                "Branch to 930A when unsigned EDI is at least 10h",
                JnbRawFileOffset,
                "73 06",
                branchTargetRawFileOffset: BranchTargetRawFileOffset),
            Instruction(
                "Opaque indirect CALL at 9304; stop at this instruction without entering it",
                OpaqueCallRawFileOffset,
                "FF 15 24 4A AC 01"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveAddress(uint ecx) =>
        unchecked((ecx << 3) + LEABaseVirtualAddress);

    public static CapacityFinalCalleeCommonIndexedAddressGatePlan Project(
        CapacityFinalCalleeCommonIndexedAddressGateCapture capture)
    {
        CapacityFinalCalleeCommonIndexedAddressGatePlan Finish(
            CapacityFinalCalleeCommonIndexedAddressGateOutcome outcome,
            bool entryEcxCaptured = false,
            bool leaReached = false,
            uint? leaIndex = null,
            uint? leaScaledIndex = null,
            uint? derivedEdx = null,
            bool moveEaxReached = false,
            uint? derivedEax = null,
            bool entryEdiCaptured = false,
            bool compareReached = false,
            uint? compareLeft = null,
            uint? compareResult = null,
            CapacityFinalCalleeCommonIndexedAddressGateFlags? compareFlags = null,
            bool jnbReached = false,
            bool? jnbTaken = null,
            uint? jnbDestination = null,
            bool exactCleanBoundedEdge = false,
            uint stopAt = LeaRawFileOffset) =>
            new(capture, outcome)
            {
                EntryECXCaptured = entryEcxCaptured,
                LEAReached = leaReached,
                LEAIndex = leaIndex,
                LEAScaledIndex = leaScaledIndex,
                LEABaseAddress = leaReached ? LEABaseVirtualAddress : null,
                DerivedEDX = derivedEdx,
                AddressOnlyLEA = leaReached,
                MoveEAXReached = moveEaxReached,
                DerivedEAX = derivedEax,
                EntryEDICaptured = entryEdiCaptured,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareReached ? CompareThreshold : null,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JnbReached = jnbReached,
                JnbTaken = jnbTaken,
                JnbSelectedDestinationRawFileOffset = jnbDestination,
                ExactCleanBoundedEdge = exactCleanBoundedEdge,
                OpaqueCallInstructionAtStop = stopAt == OpaqueCallRawFileOffset,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryECX is not uint entryEcx)
        {
            return Finish(
                CapacityFinalCalleeCommonIndexedAddressGateOutcome.MissingEntryECX);
        }

        uint scaledIndex = unchecked(entryEcx << 3);
        uint derivedAddress = unchecked(scaledIndex + LEABaseVirtualAddress);

        if (capture.EntryEDI is not uint entryEdi)
        {
            return Finish(
                CapacityFinalCalleeCommonIndexedAddressGateOutcome.MissingEntryEDI,
                entryEcxCaptured: true,
                leaReached: true,
                leaIndex: entryEcx,
                leaScaledIndex: scaledIndex,
                derivedEdx: derivedAddress,
                moveEaxReached: true,
                derivedEax: derivedAddress,
                stopAt: CompareRawFileOffset);
        }

        uint compareResult = unchecked(entryEdi - CompareThreshold);
        CapacityFinalCalleeCommonIndexedAddressGateFlags compareFlags =
            CalculateCompareFlags(entryEdi, CompareThreshold, compareResult);
        bool jnbTaken = !compareFlags.Carry;
        uint destination = jnbTaken
            ? BranchTargetRawFileOffset
            : FallthroughRawFileOffset;

        return Finish(
            jnbTaken
                ? CapacityFinalCalleeCommonIndexedAddressGateOutcome
                    .AtOrAboveUnsignedThresholdToCleanBoundedEdge
                : CapacityFinalCalleeCommonIndexedAddressGateOutcome
                    .BelowUnsignedThresholdToCleanBoundedEdge,
            entryEcxCaptured: true,
            leaReached: true,
            leaIndex: entryEcx,
            leaScaledIndex: scaledIndex,
            derivedEdx: derivedAddress,
            moveEaxReached: true,
            derivedEax: derivedAddress,
            entryEdiCaptured: true,
            compareReached: true,
            compareLeft: entryEdi,
            compareResult: compareResult,
            compareFlags: compareFlags,
            jnbReached: true,
            jnbTaken: jnbTaken,
            jnbDestination: destination,
            exactCleanBoundedEdge: true,
            stopAt: destination);
    }

    private static CapacityFinalCalleeCommonIndexedAddressGateFlags CalculateCompareFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            left < right,
            HasEvenParity((byte)result),
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeCommonIndexedAddressGateInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);
}
