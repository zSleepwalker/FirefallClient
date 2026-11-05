using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeFrameSecondFieldTestCapture(
    uint? EntryEDX,
    uint? EntryEBP,
    uint? FreshEAX);

public enum CapacityFinalCalleeLargeFrameSecondFieldTestOutcome
{
    MissingEntryEDXForFreshRead,
    MissingFreshEAX,
    MissingEntryEBPForStore,
    FreshEAXZeroToTarget,
    FreshEAXNonzeroToFallthrough
}

public readonly record struct CapacityFinalCalleeLargeFrameSecondFieldTestFlags(
    bool Carry,
    bool Parity,
    bool Zero,
    bool Sign,
    bool Overflow,
    bool? AuxiliaryCarry);

public readonly record struct CapacityFinalCalleeLargeFrameSecondFieldTestRead(
    uint RawFileOffset,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeFrameSecondFieldTestWrite(
    uint RawFileOffset,
    uint? Address,
    uint? Value,
    bool Completed);

public readonly record struct CapacityFinalCalleeLargeFrameSecondFieldTestInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeFrameSecondFieldTestPlan(
    CapacityFinalCalleeLargeFrameSecondFieldTestCapture Capture,
    CapacityFinalCalleeLargeFrameSecondFieldTestOutcome Outcome)
{
    public bool EntryEDXCaptured { get; init; }
    public bool FreshEAXReadReached { get; init; }
    public uint? FreshEDXPlus10Address { get; init; }
    public bool FreshEAXCaptured { get; init; }
    public uint? FreshEAX { get; init; }
    public bool LEAECXReached { get; init; }
    public uint? LEAECXValue { get; init; }
    public bool StoreReached { get; init; }
    public bool EntryEBPCaptured { get; init; }
    public uint? StoreAddress { get; init; }
    public uint? StoreValue { get; init; }
    public bool TestEAXReached { get; init; }
    public CapacityFinalCalleeLargeFrameSecondFieldTestFlags? TestEAXFlags { get; init; }
    public bool ZeroBranchReached { get; init; }
    public bool? ZeroBranchTaken { get; init; }
    public uint? ZeroBranchSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeFrameSecondFieldTestRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeFrameSecondFieldTestRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeFrameSecondFieldTestWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeFrameSecondFieldTestWrite>.Empty;
    public bool UnreachedEntryEBPCaptureProvided { get; init; }
    public bool UnreachedFreshEAXCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the ordered fresh EDX+10 load, address-only LEA, EBP-relative store,
/// and TEST/JZ window at raw 91E3 through 91F0.
/// </summary>
public static class CapacityFinalCalleeLargeFrameSecondFieldTestProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C91E3;
    public const uint EntryVirtualAddress = 0x016C9DE3;
    public const uint FreshReadRawFileOffset = 0x012C91E3;
    public const uint LeaEcxRawFileOffset = 0x012C91E6;
    public const uint StoreRawFileOffset = 0x012C91E9;
    public const uint TestEaxRawFileOffset = 0x012C91EC;
    public const uint ZeroBranchRawFileOffset = 0x012C91EE;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C91F0;

    public const uint ZeroDestinationRawFileOffset = 0x012C9226;
    public const uint FreshReadDisplacement = 0x10;
    public const uint StoreDisplacement = 4;

    public const string CapturedWindowBytes =
        "8B 42 10 8D 4A 10 89 4D FC 85 C0 74 36";

    public static ImmutableArray<CapacityFinalCalleeLargeFrameSecondFieldTestInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load a fresh DWORD from entry EDX+10 into EAX",
                FreshReadRawFileOffset, "8B 42 10"),
            Instruction("Form EDX+10 in ECX without reading the field",
                LeaEcxRawFileOffset, "8D 4A 10"),
            Instruction("Store ECX at entry EBP-4",
                StoreRawFileOffset, "89 4D FC"),
            Instruction("Test fresh EAX against zero",
                TestEaxRawFileOffset, "85 C0"),
            Instruction("Jump to 9226 when fresh EAX is zero",
                ZeroBranchRawFileOffset, "74 36",
                branchTargetRawFileOffset: ZeroDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshEDXPlus10Address(uint entryEDX) =>
        unchecked(entryEDX + FreshReadDisplacement);

    public static uint DeriveStoreAddress(uint entryEBP) =>
        unchecked(entryEBP - StoreDisplacement);

    public static CapacityFinalCalleeLargeFrameSecondFieldTestPlan Project(
        CapacityFinalCalleeLargeFrameSecondFieldTestCapture capture)
    {
        var reached = ImmutableArray.CreateBuilder<uint>();
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeFrameSecondFieldTestRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeFrameSecondFieldTestWrite>();
        bool entryEDXCaptured = false;
        bool freshReadReached = false;
        uint? freshAddress = null;
        bool freshCaptured = false;
        uint? freshValue = null;
        bool leaReached = false;
        uint? leaValue = null;
        bool storeReached = false;
        bool entryEBPCaptured = false;
        uint? storeAddress = null;
        uint? storeValue = null;
        bool testReached = false;
        CapacityFinalCalleeLargeFrameSecondFieldTestFlags? testFlags = null;
        bool branchReached = false;
        bool? branchTaken = null;
        uint? branchDestination = null;

        CapacityFinalCalleeLargeFrameSecondFieldTestPlan Finish(
            CapacityFinalCalleeLargeFrameSecondFieldTestOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEDXCaptured = entryEDXCaptured,
                FreshEAXReadReached = freshReadReached,
                FreshEDXPlus10Address = freshAddress,
                FreshEAXCaptured = freshCaptured,
                FreshEAX = freshValue,
                LEAECXReached = leaReached,
                LEAECXValue = leaValue,
                StoreReached = storeReached,
                EntryEBPCaptured = entryEBPCaptured,
                StoreAddress = storeAddress,
                StoreValue = storeValue,
                TestEAXReached = testReached,
                TestEAXFlags = testFlags,
                ZeroBranchReached = branchReached,
                ZeroBranchTaken = branchTaken,
                ZeroBranchSelectedDestinationRawFileOffset = branchDestination,
                ReachedInstructions = reached.ToImmutable(),
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                UnreachedEntryEBPCaptureProvided =
                    !storeReached && capture.EntryEBP.HasValue,
                UnreachedFreshEAXCaptureProvided =
                    !freshCaptured && capture.FreshEAX.HasValue,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        freshReadReached = true;
        reached.Add(FreshReadRawFileOffset);
        if (capture.EntryEDX is not uint entryEDX)
        {
            reads.Add(new(
                FreshReadRawFileOffset,
                Address: null,
                ValueCaptured: false,
                Value: null));
            return Finish(
                CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.MissingEntryEDXForFreshRead,
                FreshReadRawFileOffset);
        }

        entryEDXCaptured = true;
        freshAddress = DeriveFreshEDXPlus10Address(entryEDX);
        reads.Add(new(
            FreshReadRawFileOffset,
            freshAddress,
            capture.FreshEAX.HasValue,
            capture.FreshEAX));
        if (capture.FreshEAX is not uint freshEAX)
        {
            return Finish(
                CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.MissingFreshEAX,
                FreshReadRawFileOffset);
        }

        freshCaptured = true;
        freshValue = freshEAX;

        leaReached = true;
        reached.Add(LeaEcxRawFileOffset);
        leaValue = DeriveFreshEDXPlus10Address(entryEDX);

        storeReached = true;
        reached.Add(StoreRawFileOffset);
        if (capture.EntryEBP is not uint entryEBP)
        {
            writes.Add(new(
                StoreRawFileOffset,
                Address: null,
                Value: leaValue,
                Completed: false));
            return Finish(
                CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.MissingEntryEBPForStore,
                StoreRawFileOffset);
        }

        entryEBPCaptured = true;
        storeAddress = DeriveStoreAddress(entryEBP);
        storeValue = leaValue;
        writes.Add(new(StoreRawFileOffset, storeAddress, storeValue, Completed: true));

        testReached = true;
        reached.Add(TestEaxRawFileOffset);
        testFlags = CalculateTestFlags(freshEAX);

        branchReached = true;
        reached.Add(ZeroBranchRawFileOffset);
        branchTaken = freshEAX == 0;
        branchDestination = branchTaken.Value
            ? ZeroDestinationRawFileOffset
            : BoundaryRawFileOffsetExclusive;
        return Finish(
            branchTaken.Value
                ? CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.FreshEAXZeroToTarget
                : CapacityFinalCalleeLargeFrameSecondFieldTestOutcome.FreshEAXNonzeroToFallthrough,
            branchDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeLargeFrameSecondFieldTestFlags CalculateTestFlags(
        uint value) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)value),
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false,
            AuxiliaryCarry: null);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeLargeFrameSecondFieldTestInstructionEvidence Instruction(
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
