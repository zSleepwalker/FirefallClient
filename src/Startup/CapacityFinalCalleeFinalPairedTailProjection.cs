using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeFinalPairedTailCapture(
    uint? EntryECX,
    uint? FreshGlobalForCompare,
    uint? EntryEAX);

public enum CapacityFinalCalleeFinalPairedTailOutcome
{
    MissingEntryECXForCompare,
    MissingFreshGlobalForCompare,
    EntryECXUnsignedBelowGlobalToValidation,
    MissingEntryEAXForFirstStore,
    PairedStoresAndJumpToTarget
}

public readonly record struct CapacityFinalCalleeFinalPairedTailCompareFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public enum CapacityFinalCalleeFinalPairedTailReadKind
{
    FreshGlobalForCompare
}

public readonly record struct CapacityFinalCalleeFinalPairedTailRead(
    uint RawFileOffset,
    CapacityFinalCalleeFinalPairedTailReadKind Kind,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeFinalPairedTailWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeFinalPairedTailInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeFinalPairedTailPlan(
    CapacityFinalCalleeFinalPairedTailCapture Capture,
    CapacityFinalCalleeFinalPairedTailOutcome Outcome)
{
    public bool EntryECXCaptured { get; init; }
    public bool CompareGlobalReadReached { get; init; }
    public bool CompareGlobalCaptured { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeFinalPairedTailCompareFlags? CompareFlags { get; init; }
    public bool? EntryECXUnsignedBelowGlobal { get; init; }
    public bool GuardBranchReached { get; init; }
    public bool? GuardBranchTaken { get; init; }
    public uint? GuardBranchSelectedDestinationRawFileOffset { get; init; }
    public bool EntryEAXCaptured { get; init; }
    public bool FirstStoreReached { get; init; }
    public uint? FirstStoreAddress { get; init; }
    public uint? FirstStoreValue { get; init; }
    public bool SecondStoreReached { get; init; }
    public uint? SecondStoreAddress { get; init; }
    public uint? SecondStoreValue { get; init; }
    public bool FinalJumpReached { get; init; }
    public uint? FinalJumpSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<CapacityFinalCalleeFinalPairedTailRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeFinalPairedTailRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeFinalPairedTailWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeFinalPairedTailWrite>.Empty;
    public bool UnreachedFreshGlobalForCompareCaptureProvided { get; init; }
    public bool UnreachedEntryEAXCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the unsigned guard, paired stores, and final jump without
/// executing or emulating the target.
/// </summary>
public static class CapacityFinalCalleeFinalPairedTailProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C908B;
    public const uint EntryVirtualAddress = 0x016C9C8B;
    public const uint CompareRawFileOffset = 0x012C908B;
    public const uint GuardBranchRawFileOffset = 0x012C9091;
    public const uint FirstStoreRawFileOffset = 0x012C9097;
    public const uint SecondStoreRawFileOffset = 0x012C909A;
    public const uint FinalJumpRawFileOffset = 0x012C909D;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C90A2;

    public const uint ValidationDestinationRawFileOffset = 0x012C8E76;
    public const uint FinalJumpDestinationRawFileOffset = 0x012C8EF2;
    public const uint GlobalValueVirtualAddress = 0x01EDADD8;
    public const uint FirstStoreDisplacement = 0x14;
    public const uint SecondStoreDisplacement = 0x18;

    public const string CapturedWindowBytes =
        "3B 0D D8 AD ED 01 0F 82 DF FD FF FF 89 48 14 "
        + "89 41 18 E9 50 FE FF FF";

    public static ImmutableArray<CapacityFinalCalleeFinalPairedTailInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry ECX with the fresh global DWORD",
                CompareRawFileOffset, "3B 0D D8 AD ED 01",
                peVirtualAddressOperand: GlobalValueVirtualAddress),
            Instruction("Jump to validation when unsigned entry ECX is below the global",
                GuardBranchRawFileOffset, "0F 82 DF FD FF FF",
                branchTargetRawFileOffset: ValidationDestinationRawFileOffset),
            Instruction("Store entry ECX at entry EAX+14 on the non-below path",
                FirstStoreRawFileOffset, "89 48 14"),
            Instruction("Store entry EAX at entry ECX+18",
                SecondStoreRawFileOffset, "89 41 18"),
            Instruction("Jump to 8EF2 after the paired stores",
                FinalJumpRawFileOffset, "E9 50 FE FF FF",
                branchTargetRawFileOffset: FinalJumpDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFirstStoreAddress(uint entryEax) =>
        unchecked(entryEax + FirstStoreDisplacement);

    public static uint DeriveSecondStoreAddress(uint entryEcx) =>
        unchecked(entryEcx + SecondStoreDisplacement);

    public static CapacityFinalCalleeFinalPairedTailPlan Project(
        CapacityFinalCalleeFinalPairedTailCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeFinalPairedTailRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeFinalPairedTailWrite>();
        bool entryEcxCaptured = false;
        bool compareGlobalReadReached = false;
        bool compareGlobalCaptured = false;
        bool compareReached = false;
        uint? compareResult = null;
        CapacityFinalCalleeFinalPairedTailCompareFlags? compareFlags = null;
        bool? unsignedBelow = null;
        bool guardBranchReached = false;
        bool? guardBranchTaken = null;
        uint? guardBranchDestination = null;
        bool entryEaxCaptured = false;
        bool firstStoreReached = false;
        uint? firstStoreAddress = null;
        uint? firstStoreValue = null;
        bool secondStoreReached = false;
        uint? secondStoreAddress = null;
        uint? secondStoreValue = null;
        bool finalJumpReached = false;
        uint? finalJumpDestination = null;
        bool unreachedGlobal = false;
        bool unreachedEax = false;

        CapacityFinalCalleeFinalPairedTailPlan Finish(
            CapacityFinalCalleeFinalPairedTailOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryECXCaptured = entryEcxCaptured,
                CompareGlobalReadReached = compareGlobalReadReached,
                CompareGlobalCaptured = compareGlobalCaptured,
                CompareReached = compareReached,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                EntryECXUnsignedBelowGlobal = unsignedBelow,
                GuardBranchReached = guardBranchReached,
                GuardBranchTaken = guardBranchTaken,
                GuardBranchSelectedDestinationRawFileOffset = guardBranchDestination,
                EntryEAXCaptured = entryEaxCaptured,
                FirstStoreReached = firstStoreReached,
                FirstStoreAddress = firstStoreAddress,
                FirstStoreValue = firstStoreValue,
                SecondStoreReached = secondStoreReached,
                SecondStoreAddress = secondStoreAddress,
                SecondStoreValue = secondStoreValue,
                FinalJumpReached = finalJumpReached,
                FinalJumpSelectedDestinationRawFileOffset = finalJumpDestination,
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                UnreachedFreshGlobalForCompareCaptureProvided = unreachedGlobal,
                UnreachedEntryEAXCaptureProvided = unreachedEax,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryECX is not uint entryEcx)
        {
            unreachedGlobal = capture.FreshGlobalForCompare.HasValue;
            unreachedEax = capture.EntryEAX.HasValue;
            return Finish(
                CapacityFinalCalleeFinalPairedTailOutcome.MissingEntryECXForCompare,
                CompareRawFileOffset);
        }

        entryEcxCaptured = true;
        compareGlobalReadReached = true;
        reads.Add(new(
            CompareRawFileOffset,
            CapacityFinalCalleeFinalPairedTailReadKind.FreshGlobalForCompare,
            GlobalValueVirtualAddress,
            capture.FreshGlobalForCompare.HasValue,
            capture.FreshGlobalForCompare));
        if (capture.FreshGlobalForCompare is not uint globalValue)
        {
            unreachedEax = capture.EntryEAX.HasValue;
            return Finish(
                CapacityFinalCalleeFinalPairedTailOutcome.MissingFreshGlobalForCompare,
                CompareRawFileOffset);
        }

        compareGlobalCaptured = true;
        uint difference = unchecked(entryEcx - globalValue);
        compareResult = difference;
        compareFlags = CalculateCompareFlags(entryEcx, globalValue, difference);
        compareReached = true;
        bool isUnsignedBelow = compareFlags.Value.Carry;
        unsignedBelow = isUnsignedBelow;
        guardBranchReached = true;
        guardBranchTaken = isUnsignedBelow;
        guardBranchDestination = isUnsignedBelow
            ? ValidationDestinationRawFileOffset
            : FirstStoreRawFileOffset;
        if (isUnsignedBelow)
        {
            unreachedEax = capture.EntryEAX.HasValue;
            return Finish(
                CapacityFinalCalleeFinalPairedTailOutcome
                    .EntryECXUnsignedBelowGlobalToValidation,
                ValidationDestinationRawFileOffset,
                exactCleanExitAdmitted: !unreachedEax);
        }

        if (capture.EntryEAX is not uint entryEax)
        {
            return Finish(
                CapacityFinalCalleeFinalPairedTailOutcome.MissingEntryEAXForFirstStore,
                FirstStoreRawFileOffset);
        }

        entryEaxCaptured = true;
        firstStoreAddress = DeriveFirstStoreAddress(entryEax);
        firstStoreValue = entryEcx;
        firstStoreReached = true;
        writes.Add(new(FirstStoreRawFileOffset, firstStoreAddress.Value, entryEcx));

        secondStoreAddress = DeriveSecondStoreAddress(entryEcx);
        secondStoreValue = entryEax;
        secondStoreReached = true;
        writes.Add(new(SecondStoreRawFileOffset, secondStoreAddress.Value, entryEax));

        finalJumpReached = true;
        finalJumpDestination = FinalJumpDestinationRawFileOffset;
        return Finish(
            CapacityFinalCalleeFinalPairedTailOutcome.PairedStoresAndJumpToTarget,
            FinalJumpDestinationRawFileOffset,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeFinalPairedTailCompareFlags
        CalculateCompareFlags(uint left, uint right, uint result) =>
        new(
            left < right,
            HasEvenParity((byte)result),
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeFinalPairedTailInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            peVirtualAddressOperand,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);
}
