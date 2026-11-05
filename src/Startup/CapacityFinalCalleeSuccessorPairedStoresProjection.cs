using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeSuccessorPairedStoresCapture(
    uint? EntryECX,
    uint? FreshGlobalValue,
    uint? EntryEAX,
    uint? EntryESI,
    uint? FreshEsiPlus14);

public enum CapacityFinalCalleeSuccessorPairedStoresOutcome
{
    MissingEntryECX,
    MissingFreshGlobalValue,
    EntryECXUnsignedBelowGlobalToValidation,
    MissingEntryEAXForFirstStore,
    MissingEntryESIForFreshRead,
    MissingFreshEsiPlus14,
    FreshEsiPlus14ZeroToTarget,
    FreshEsiPlus14NonzeroToBoundary
}

public readonly record struct CapacityFinalCalleeSuccessorPairedStoresCompareFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeSuccessorPairedStoresTestFlags(
    bool Carry,
    bool Parity,
    bool Zero,
    bool Sign,
    bool Overflow);

public enum CapacityFinalCalleeSuccessorPairedStoresReadKind
{
    FreshGlobalValue,
    FreshEsiPlus14
}

public readonly record struct CapacityFinalCalleeSuccessorPairedStoresRead(
    uint RawFileOffset,
    CapacityFinalCalleeSuccessorPairedStoresReadKind Kind,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeSuccessorPairedStoresWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeSuccessorPairedStoresInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeSuccessorPairedStoresPlan(
    CapacityFinalCalleeSuccessorPairedStoresCapture Capture,
    CapacityFinalCalleeSuccessorPairedStoresOutcome Outcome)
{
    public bool EntryECXCaptured { get; init; }
    public bool CompareGlobalReadReached { get; init; }
    public bool CompareGlobalValueCaptured { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeSuccessorPairedStoresCompareFlags? CompareFlags { get; init; }
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
    public bool EntryESICaptured { get; init; }
    public uint? FreshEsiPlus14Address { get; init; }
    public bool FreshEsiPlus14ReadReached { get; init; }
    public bool FreshEsiPlus14Captured { get; init; }
    public uint? FreshEsiPlus14 { get; init; }
    public bool TestFreshEcxReached { get; init; }
    public CapacityFinalCalleeSuccessorPairedStoresTestFlags? TestFreshEcxFlags { get; init; }
    public bool ZeroBranchReached { get; init; }
    public bool? ZeroBranchTaken { get; init; }
    public uint? ZeroBranchSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<CapacityFinalCalleeSuccessorPairedStoresRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeSuccessorPairedStoresRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeSuccessorPairedStoresWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeSuccessorPairedStoresWrite>.Empty;
    public bool UnreachedFreshGlobalValueCaptureProvided { get; init; }
    public bool UnreachedEntryEAXCaptureProvided { get; init; }
    public bool UnreachedEntryESICaptureProvided { get; init; }
    public bool UnreachedFreshEsiPlus14CaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the unsigned guard, paired stores, and fresh ESI+14 zero gate only.
/// Fresh memory captures remain independent of the projected stores.
/// </summary>
public static class CapacityFinalCalleeSuccessorPairedStoresProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C906E;
    public const uint EntryVirtualAddress = 0x016C9C6E;
    public const uint CompareRawFileOffset = 0x012C906E;
    public const uint GuardBranchRawFileOffset = 0x012C9074;
    public const uint FirstStoreRawFileOffset = 0x012C907A;
    public const uint SecondStoreRawFileOffset = 0x012C907D;
    public const uint FreshReadRawFileOffset = 0x012C9080;
    public const uint TestEcxRawFileOffset = 0x012C9083;
    public const uint ZeroBranchRawFileOffset = 0x012C9085;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C908B;

    public const uint ValidationDestinationRawFileOffset = 0x012C8E76;
    public const uint ZeroDestinationRawFileOffset = 0x012C8EF2;
    public const uint GlobalValueVirtualAddress = 0x01EDADD8;
    public const uint FirstStoreDisplacement = 0x10;
    public const uint SecondStoreDisplacement = 0x18;
    public const uint FreshReadDisplacement = 0x14;

    public const string CapturedWindowBytes =
        "3B 0D D8 AD ED 01 0F 82 FC FD FF FF 89 48 10 89 41 18 "
        + "8B 4E 14 85 C9 0F 84 67 FE FF FF";

    public static ImmutableArray<CapacityFinalCalleeSuccessorPairedStoresInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry ECX with the fresh global DWORD",
                CompareRawFileOffset, "3B 0D D8 AD ED 01",
                peVirtualAddressOperand: GlobalValueVirtualAddress),
            Instruction("Jump to validation when unsigned entry ECX is below the global",
                GuardBranchRawFileOffset, "0F 82 FC FD FF FF",
                branchTargetRawFileOffset: ValidationDestinationRawFileOffset),
            Instruction("Store entry ECX at entry EAX+10 on the non-below path",
                FirstStoreRawFileOffset, "89 48 10"),
            Instruction("Store entry EAX at entry ECX+18",
                SecondStoreRawFileOffset, "89 41 18"),
            Instruction("Load a fresh DWORD from entry ESI+14 into ECX",
                FreshReadRawFileOffset, "8B 4E 14"),
            Instruction("Test fresh ECX against zero",
                TestEcxRawFileOffset, "85 C9"),
            Instruction("Jump to EF2 when fresh ECX is zero",
                ZeroBranchRawFileOffset, "0F 84 67 FE FF FF",
                branchTargetRawFileOffset: ZeroDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFirstStoreAddress(uint entryEax) =>
        unchecked(entryEax + FirstStoreDisplacement);

    public static uint DeriveSecondStoreAddress(uint entryEcx) =>
        unchecked(entryEcx + SecondStoreDisplacement);

    public static uint DeriveFreshEsiPlus14Address(uint entryEsi) =>
        unchecked(entryEsi + FreshReadDisplacement);

    public static CapacityFinalCalleeSuccessorPairedStoresPlan Project(
        CapacityFinalCalleeSuccessorPairedStoresCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeSuccessorPairedStoresRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeSuccessorPairedStoresWrite>();
        bool entryEcxCaptured = false;
        bool compareGlobalReadReached = false;
        bool compareGlobalCaptured = false;
        bool compareReached = false;
        uint? compareResult = null;
        CapacityFinalCalleeSuccessorPairedStoresCompareFlags? compareFlags = null;
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
        bool entryEsiCaptured = false;
        uint? freshAddress = null;
        bool freshReadReached = false;
        bool freshCaptured = false;
        uint? freshValue = null;
        bool testReached = false;
        CapacityFinalCalleeSuccessorPairedStoresTestFlags? testFlags = null;
        bool zeroBranchReached = false;
        bool? zeroBranchTaken = null;
        uint? zeroBranchDestination = null;
        bool unreachedGlobal = false;
        bool unreachedEax = false;
        bool unreachedEsi = false;
        bool unreachedFresh = false;

        CapacityFinalCalleeSuccessorPairedStoresPlan Finish(
            CapacityFinalCalleeSuccessorPairedStoresOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryECXCaptured = entryEcxCaptured,
                CompareGlobalReadReached = compareGlobalReadReached,
                CompareGlobalValueCaptured = compareGlobalCaptured,
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
                EntryESICaptured = entryEsiCaptured,
                FreshEsiPlus14Address = freshAddress,
                FreshEsiPlus14ReadReached = freshReadReached,
                FreshEsiPlus14Captured = freshCaptured,
                FreshEsiPlus14 = freshValue,
                TestFreshEcxReached = testReached,
                TestFreshEcxFlags = testFlags,
                ZeroBranchReached = zeroBranchReached,
                ZeroBranchTaken = zeroBranchTaken,
                ZeroBranchSelectedDestinationRawFileOffset = zeroBranchDestination,
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                UnreachedFreshGlobalValueCaptureProvided = unreachedGlobal,
                UnreachedEntryEAXCaptureProvided = unreachedEax,
                UnreachedEntryESICaptureProvided = unreachedEsi,
                UnreachedFreshEsiPlus14CaptureProvided = unreachedFresh,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryECX is not uint entryEcx)
        {
            unreachedGlobal = capture.FreshGlobalValue.HasValue;
            unreachedEax = capture.EntryEAX.HasValue;
            unreachedEsi = capture.EntryESI.HasValue;
            unreachedFresh = capture.FreshEsiPlus14.HasValue;
            return Finish(
                CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingEntryECX,
                CompareRawFileOffset);
        }

        entryEcxCaptured = true;
        compareGlobalReadReached = true;
        reads.Add(new(
            CompareRawFileOffset,
            CapacityFinalCalleeSuccessorPairedStoresReadKind.FreshGlobalValue,
            GlobalValueVirtualAddress,
            capture.FreshGlobalValue.HasValue,
            capture.FreshGlobalValue));
        if (capture.FreshGlobalValue is not uint globalValue)
        {
            unreachedEax = capture.EntryEAX.HasValue;
            unreachedEsi = capture.EntryESI.HasValue;
            unreachedFresh = capture.FreshEsiPlus14.HasValue;
            return Finish(
                CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingFreshGlobalValue,
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
            unreachedEsi = capture.EntryESI.HasValue;
            unreachedFresh = capture.FreshEsiPlus14.HasValue;
            return Finish(
                CapacityFinalCalleeSuccessorPairedStoresOutcome
                    .EntryECXUnsignedBelowGlobalToValidation,
                ValidationDestinationRawFileOffset,
                exactCleanExitAdmitted: !unreachedEax && !unreachedEsi && !unreachedFresh);
        }

        if (capture.EntryEAX is not uint entryEax)
        {
            unreachedEsi = capture.EntryESI.HasValue;
            unreachedFresh = capture.FreshEsiPlus14.HasValue;
            return Finish(
                CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingEntryEAXForFirstStore,
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

        freshReadReached = true;
        if (capture.EntryESI is not uint entryEsi)
        {
            unreachedFresh = capture.FreshEsiPlus14.HasValue;
            reads.Add(new(
                FreshReadRawFileOffset,
                CapacityFinalCalleeSuccessorPairedStoresReadKind.FreshEsiPlus14,
                Address: null,
                ValueCaptured: false,
                Value: null));
            return Finish(
                CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingEntryESIForFreshRead,
                FreshReadRawFileOffset);
        }

        entryEsiCaptured = true;
        freshAddress = DeriveFreshEsiPlus14Address(entryEsi);
        reads.Add(new(
            FreshReadRawFileOffset,
            CapacityFinalCalleeSuccessorPairedStoresReadKind.FreshEsiPlus14,
            freshAddress,
            capture.FreshEsiPlus14.HasValue,
            capture.FreshEsiPlus14));
        if (capture.FreshEsiPlus14 is not uint esiPlus14)
        {
            return Finish(
                CapacityFinalCalleeSuccessorPairedStoresOutcome.MissingFreshEsiPlus14,
                FreshReadRawFileOffset);
        }

        freshCaptured = true;
        freshValue = esiPlus14;
        testReached = true;
        testFlags = CalculateTestFlags(esiPlus14);
        zeroBranchReached = true;
        zeroBranchTaken = testFlags.Value.Zero;
        zeroBranchDestination = zeroBranchTaken.Value
            ? ZeroDestinationRawFileOffset
            : BoundaryRawFileOffsetExclusive;
        return Finish(
            zeroBranchTaken.Value
                ? CapacityFinalCalleeSuccessorPairedStoresOutcome.FreshEsiPlus14ZeroToTarget
                : CapacityFinalCalleeSuccessorPairedStoresOutcome.FreshEsiPlus14NonzeroToBoundary,
            zeroBranchDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeSuccessorPairedStoresCompareFlags
        CalculateCompareFlags(uint left, uint right, uint result) =>
        new(
            left < right,
            HasEvenParity((byte)result),
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static CapacityFinalCalleeSuccessorPairedStoresTestFlags
        CalculateTestFlags(uint value) =>
        new(
            false,
            HasEvenParity((byte)value),
            value == 0,
            (value & 0x80000000) != 0,
            false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeSuccessorPairedStoresInstructionEvidence Instruction(
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
