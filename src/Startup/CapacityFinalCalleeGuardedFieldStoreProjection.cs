using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeGuardedFieldStoreCapture(
    uint? EntryEAX,
    uint? FreshGlobalForCompare,
    uint? EntryECX,
    uint? EntryESI,
    uint? FreshEsiPlus10);

public enum CapacityFinalCalleeGuardedFieldStoreOutcome
{
    MissingEntryEAXForCompare,
    MissingFreshGlobalForCompare,
    EntryEAXUnsignedBelowGlobalToValidation,
    MissingEntryECXForStore,
    MissingEntryESIForRead,
    MissingFreshEsiPlus10,
    FreshEsiPlus10ZeroToTarget,
    FreshEsiPlus10NonzeroToBoundary
}

public readonly record struct CapacityFinalCalleeGuardedFieldStoreCompareFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeGuardedFieldStoreTestFlags(
    bool Carry,
    bool Parity,
    bool Zero,
    bool Sign,
    bool Overflow);

public enum CapacityFinalCalleeGuardedFieldStoreReadKind
{
    FreshGlobalForCompare,
    FreshEsiPlus10
}

public readonly record struct CapacityFinalCalleeGuardedFieldStoreRead(
    uint RawFileOffset,
    CapacityFinalCalleeGuardedFieldStoreReadKind Kind,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeGuardedFieldStoreWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeGuardedFieldStoreInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeGuardedFieldStorePlan(
    CapacityFinalCalleeGuardedFieldStoreCapture Capture,
    CapacityFinalCalleeGuardedFieldStoreOutcome Outcome)
{
    public bool EntryEAXCaptured { get; init; }
    public bool CompareGlobalReadReached { get; init; }
    public bool CompareGlobalCaptured { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeGuardedFieldStoreCompareFlags? CompareFlags { get; init; }
    public bool? EntryEAXUnsignedBelowGlobal { get; init; }
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public bool EntryECXCaptured { get; init; }
    public bool StoreReached { get; init; }
    public uint? StoreAddress { get; init; }
    public uint? StoreValue { get; init; }
    public uint? StoreDisplacement { get; init; }
    public bool EntryESICaptured { get; init; }
    public uint? FreshEsiPlus10Address { get; init; }
    public bool FreshEsiPlus10ReadReached { get; init; }
    public bool FreshEsiPlus10Captured { get; init; }
    public uint? FreshEsiPlus10 { get; init; }
    public bool TestECXReached { get; init; }
    public uint? TestECXValue { get; init; }
    public CapacityFinalCalleeGuardedFieldStoreTestFlags? TestECXFlags { get; init; }
    public bool ZeroBranchReached { get; init; }
    public bool? ZeroBranchTaken { get; init; }
    public uint? ZeroBranchSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<CapacityFinalCalleeGuardedFieldStoreRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeGuardedFieldStoreRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeGuardedFieldStoreWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeGuardedFieldStoreWrite>.Empty;
    public bool UnreachedFreshGlobalForCompareCaptureProvided { get; init; }
    public bool UnreachedEntryECXCaptureProvided { get; init; }
    public bool UnreachedEntryESICaptureProvided { get; init; }
    public bool UnreachedFreshEsiPlus10CaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the unsigned global guard, guarded field store, and independent
/// fresh ESI+10 load/test without executing or emulating the target.
/// </summary>
public static class CapacityFinalCalleeGuardedFieldStoreProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9058;
    public const uint EntryVirtualAddress = 0x016C9C58;
    public const uint CompareRawFileOffset = 0x012C9058;
    public const uint GuardBranchRawFileOffset = 0x012C905E;
    public const uint StoreRawFileOffset = 0x012C9064;
    public const uint FreshReadRawFileOffset = 0x012C9067;
    public const uint TestEcxRawFileOffset = 0x012C906A;
    public const uint ZeroBranchRawFileOffset = 0x012C906C;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C906E;

    public const uint ValidationDestinationRawFileOffset = 0x012C8E76;
    public const uint ZeroDestinationRawFileOffset = 0x012C9080;
    public const uint GlobalValueVirtualAddress = 0x01EDADD8;
    public const uint StoreDisplacement = 0x18;
    public const uint FreshReadDisplacement = 0x10;

    public const string CapturedWindowBytes =
        "3B 05 D8 AD ED 01 0F 82 12 FE FF FF 89 48 18 "
        + "8B 4E 10 85 C9 74 12";

    public static ImmutableArray<CapacityFinalCalleeGuardedFieldStoreInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry EAX with the fresh global DWORD",
                CompareRawFileOffset, "3B 05 D8 AD ED 01",
                peVirtualAddressOperand: GlobalValueVirtualAddress),
            Instruction("Jump to validation when unsigned entry EAX is below the global",
                GuardBranchRawFileOffset, "0F 82 12 FE FF FF",
                branchTargetRawFileOffset: ValidationDestinationRawFileOffset),
            Instruction("Store entry ECX at entry EAX+18 on the non-below path",
                StoreRawFileOffset, "89 48 18"),
            Instruction("Load a fresh DWORD from entry ESI+10 into ECX",
                FreshReadRawFileOffset, "8B 4E 10"),
            Instruction("Test the freshly loaded ECX against zero",
                TestEcxRawFileOffset, "85 C9"),
            Instruction("Jump to 9080 when the fresh ECX value is zero",
                ZeroBranchRawFileOffset, "74 12",
                branchTargetRawFileOffset: ZeroDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveStoreAddress(uint entryEax) =>
        unchecked(entryEax + StoreDisplacement);

    public static uint DeriveFreshEsiPlus10Address(uint entryEsi) =>
        unchecked(entryEsi + FreshReadDisplacement);

    public static CapacityFinalCalleeGuardedFieldStorePlan Project(
        CapacityFinalCalleeGuardedFieldStoreCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeGuardedFieldStoreRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeGuardedFieldStoreWrite>();
        bool entryEaxCaptured = false;
        bool compareGlobalReadReached = false;
        bool compareGlobalCaptured = false;
        bool compareReached = false;
        uint? compareResult = null;
        CapacityFinalCalleeGuardedFieldStoreCompareFlags? compareFlags = null;
        bool? entryEaxUnsignedBelowGlobal = null;
        bool branchReached = false;
        bool? branchTaken = null;
        uint? branchDestination = null;
        bool entryEcxCaptured = false;
        bool storeReached = false;
        uint? storeAddress = null;
        uint? storeValue = null;
        bool entryEsiCaptured = false;
        uint? freshAddress = null;
        bool freshReadReached = false;
        bool freshCaptured = false;
        uint? freshValue = null;
        bool testReached = false;
        CapacityFinalCalleeGuardedFieldStoreTestFlags? testFlags = null;
        bool zeroBranchReached = false;
        bool? zeroBranchTaken = null;
        uint? zeroBranchDestination = null;
        bool unreachedGlobal = false;
        bool unreachedEcx = false;
        bool unreachedEsi = false;
        bool unreachedFresh = false;

        CapacityFinalCalleeGuardedFieldStorePlan Finish(
            CapacityFinalCalleeGuardedFieldStoreOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEAXCaptured = entryEaxCaptured,
                CompareGlobalReadReached = compareGlobalReadReached,
                CompareGlobalCaptured = compareGlobalCaptured,
                CompareReached = compareReached,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                EntryEAXUnsignedBelowGlobal = entryEaxUnsignedBelowGlobal,
                BranchReached = branchReached,
                BranchTaken = branchTaken,
                BranchSelectedDestinationRawFileOffset = branchDestination,
                EntryECXCaptured = entryEcxCaptured,
                StoreReached = storeReached,
                StoreAddress = storeAddress,
                StoreValue = storeValue,
                StoreDisplacement = storeReached ? StoreDisplacement : null,
                EntryESICaptured = entryEsiCaptured,
                FreshEsiPlus10Address = freshAddress,
                FreshEsiPlus10ReadReached = freshReadReached,
                FreshEsiPlus10Captured = freshCaptured,
                FreshEsiPlus10 = freshValue,
                TestECXReached = testReached,
                TestECXValue = testReached ? freshValue : null,
                TestECXFlags = testFlags,
                ZeroBranchReached = zeroBranchReached,
                ZeroBranchTaken = zeroBranchTaken,
                ZeroBranchSelectedDestinationRawFileOffset = zeroBranchDestination,
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                UnreachedFreshGlobalForCompareCaptureProvided = unreachedGlobal,
                UnreachedEntryECXCaptureProvided = unreachedEcx,
                UnreachedEntryESICaptureProvided = unreachedEsi,
                UnreachedFreshEsiPlus10CaptureProvided = unreachedFresh,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEAX is not uint entryEax)
        {
            unreachedGlobal = capture.FreshGlobalForCompare.HasValue;
            unreachedEcx = capture.EntryECX.HasValue;
            unreachedEsi = capture.EntryESI.HasValue;
            unreachedFresh = capture.FreshEsiPlus10.HasValue;
            return Finish(
                CapacityFinalCalleeGuardedFieldStoreOutcome.MissingEntryEAXForCompare,
                CompareRawFileOffset);
        }

        entryEaxCaptured = true;
        compareGlobalReadReached = true;
        reads.Add(new(
            CompareRawFileOffset,
            CapacityFinalCalleeGuardedFieldStoreReadKind.FreshGlobalForCompare,
            GlobalValueVirtualAddress,
            capture.FreshGlobalForCompare.HasValue,
            capture.FreshGlobalForCompare));
        if (capture.FreshGlobalForCompare is not uint globalValue)
        {
            unreachedEcx = capture.EntryECX.HasValue;
            unreachedEsi = capture.EntryESI.HasValue;
            unreachedFresh = capture.FreshEsiPlus10.HasValue;
            return Finish(
                CapacityFinalCalleeGuardedFieldStoreOutcome.MissingFreshGlobalForCompare,
                CompareRawFileOffset);
        }

        compareGlobalCaptured = true;
        uint difference = unchecked(entryEax - globalValue);
        compareResult = difference;
        compareFlags = CalculateCompareFlags(entryEax, globalValue, difference);
        compareReached = true;
        bool unsignedBelow = compareFlags.Value.Carry;
        entryEaxUnsignedBelowGlobal = unsignedBelow;
        branchReached = true;
        branchTaken = unsignedBelow;
        branchDestination = unsignedBelow
            ? ValidationDestinationRawFileOffset
            : StoreRawFileOffset;
        if (unsignedBelow)
        {
            unreachedEcx = capture.EntryECX.HasValue;
            unreachedEsi = capture.EntryESI.HasValue;
            unreachedFresh = capture.FreshEsiPlus10.HasValue;
            return Finish(
                CapacityFinalCalleeGuardedFieldStoreOutcome
                    .EntryEAXUnsignedBelowGlobalToValidation,
                ValidationDestinationRawFileOffset,
                exactCleanExitAdmitted:
                    !unreachedEcx && !unreachedEsi && !unreachedFresh);
        }

        if (capture.EntryECX is not uint entryEcx)
        {
            unreachedEsi = capture.EntryESI.HasValue;
            unreachedFresh = capture.FreshEsiPlus10.HasValue;
            return Finish(
                CapacityFinalCalleeGuardedFieldStoreOutcome.MissingEntryECXForStore,
                StoreRawFileOffset);
        }

        entryEcxCaptured = true;
        storeAddress = DeriveStoreAddress(entryEax);
        storeValue = entryEcx;
        writes.Add(new(StoreRawFileOffset, storeAddress.Value, entryEcx));
        storeReached = true;

        freshReadReached = true;
        if (capture.EntryESI is not uint entryEsi)
        {
            unreachedFresh = capture.FreshEsiPlus10.HasValue;
            reads.Add(new(
                FreshReadRawFileOffset,
                CapacityFinalCalleeGuardedFieldStoreReadKind.FreshEsiPlus10,
                Address: null,
                ValueCaptured: false,
                Value: null));
            return Finish(
                CapacityFinalCalleeGuardedFieldStoreOutcome.MissingEntryESIForRead,
                FreshReadRawFileOffset);
        }

        entryEsiCaptured = true;
        freshAddress = DeriveFreshEsiPlus10Address(entryEsi);
        reads.Add(new(
            FreshReadRawFileOffset,
            CapacityFinalCalleeGuardedFieldStoreReadKind.FreshEsiPlus10,
            freshAddress,
            capture.FreshEsiPlus10.HasValue,
            capture.FreshEsiPlus10));
        if (capture.FreshEsiPlus10 is not uint esiPlus10)
        {
            return Finish(
                CapacityFinalCalleeGuardedFieldStoreOutcome.MissingFreshEsiPlus10,
                FreshReadRawFileOffset);
        }

        freshCaptured = true;
        freshValue = esiPlus10;
        testReached = true;
        testFlags = CalculateTestFlags(esiPlus10);
        zeroBranchReached = true;
        zeroBranchTaken = testFlags.Value.Zero;
        zeroBranchDestination = zeroBranchTaken.Value
            ? ZeroDestinationRawFileOffset
            : BoundaryRawFileOffsetExclusive;
        return Finish(
            zeroBranchTaken.Value
                ? CapacityFinalCalleeGuardedFieldStoreOutcome
                    .FreshEsiPlus10ZeroToTarget
                : CapacityFinalCalleeGuardedFieldStoreOutcome
                    .FreshEsiPlus10NonzeroToBoundary,
            zeroBranchDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeGuardedFieldStoreCompareFlags
        CalculateCompareFlags(uint left, uint right, uint result) =>
        new(
            left < right,
            HasEvenParity((byte)result),
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static CapacityFinalCalleeGuardedFieldStoreTestFlags
        CalculateTestFlags(uint value) =>
        new(
            false,
            HasEvenParity((byte)value),
            value == 0,
            (value & 0x80000000) != 0,
            false);

    private static bool HasEvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeGuardedFieldStoreInstructionEvidence Instruction(
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
