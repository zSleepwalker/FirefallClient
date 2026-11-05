using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeFrameFirstFieldTestCapture(
    uint? EntryEDX,
    uint? EntryEBP,
    uint? FreshEAX);

public enum CapacityFinalCalleeLargeFrameFirstFieldTestOutcome
{
    MissingEntryEDXForFreshRead,
    MissingFreshEAX,
    MissingEntryEBPForStore,
    FreshEAXZeroToFallthrough,
    FreshEAXNonzeroToTarget
}

public readonly record struct CapacityFinalCalleeLargeFrameFirstFieldTestFlags(
    bool Carry,
    bool Parity,
    bool Zero,
    bool Sign,
    bool Overflow,
    bool? AuxiliaryCarry);

public readonly record struct CapacityFinalCalleeLargeFrameFirstFieldTestRead(
    uint RawFileOffset,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeFrameFirstFieldTestWrite(
    uint RawFileOffset,
    uint? Address,
    uint? Value,
    bool Completed);

public readonly record struct CapacityFinalCalleeLargeFrameFirstFieldTestInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeFrameFirstFieldTestPlan(
    CapacityFinalCalleeLargeFrameFirstFieldTestCapture Capture,
    CapacityFinalCalleeLargeFrameFirstFieldTestOutcome Outcome)
{
    public bool EntryEDXCaptured { get; init; }
    public bool FreshEAXReadReached { get; init; }
    public uint? FreshEDXPlus14Address { get; init; }
    public bool FreshEAXCaptured { get; init; }
    public uint? FreshEAX { get; init; }
    public bool LEAECXReached { get; init; }
    public uint? LEAECXValue { get; init; }
    public bool StoreReached { get; init; }
    public bool EntryEBPCaptured { get; init; }
    public uint? StoreAddress { get; init; }
    public uint? StoreValue { get; init; }
    public bool TestEAXReached { get; init; }
    public CapacityFinalCalleeLargeFrameFirstFieldTestFlags? TestEAXFlags { get; init; }
    public bool NonzeroBranchReached { get; init; }
    public bool? NonzeroBranchTaken { get; init; }
    public uint? NonzeroBranchSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeFrameFirstFieldTestRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeFrameFirstFieldTestRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeFrameFirstFieldTestWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeFrameFirstFieldTestWrite>.Empty;
    public bool UnreachedEntryEBPCaptureProvided { get; init; }
    public bool UnreachedFreshEAXCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the ordered fresh-field load, address-only LEA, EBP-relative store,
/// and TEST/JNZ window at raw 91D6 through 91E3.
/// </summary>
public static class CapacityFinalCalleeLargeFrameFirstFieldTestProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C91D6;
    public const uint EntryVirtualAddress = 0x016C9DD6;
    public const uint FreshReadRawFileOffset = 0x012C91D6;
    public const uint LeaEcxRawFileOffset = 0x012C91D9;
    public const uint StoreRawFileOffset = 0x012C91DC;
    public const uint TestEaxRawFileOffset = 0x012C91DF;
    public const uint NonzeroBranchRawFileOffset = 0x012C91E1;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C91E3;

    public const uint NonzeroDestinationRawFileOffset = 0x012C91F0;
    public const uint FreshReadDisplacement = 0x14;
    public const uint StoreDisplacement = 4;

    public const string CapturedWindowBytes =
        "8B 42 14 8D 4A 14 89 4D FC 85 C0 75 0D";

    public static ImmutableArray<CapacityFinalCalleeLargeFrameFirstFieldTestInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load a fresh DWORD from entry EDX+14 into EAX",
                FreshReadRawFileOffset, "8B 42 14"),
            Instruction("Form EDX+14 in ECX without reading the field",
                LeaEcxRawFileOffset, "8D 4A 14"),
            Instruction("Store ECX at entry EBP-4",
                StoreRawFileOffset, "89 4D FC"),
            Instruction("Test fresh EAX against zero",
                TestEaxRawFileOffset, "85 C0"),
            Instruction("Jump to 91F0 when fresh EAX is nonzero",
                NonzeroBranchRawFileOffset, "75 0D",
                branchTargetRawFileOffset: NonzeroDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshEDXPlus14Address(uint entryEDX) =>
        unchecked(entryEDX + FreshReadDisplacement);

    public static uint DeriveStoreAddress(uint entryEBP) =>
        unchecked(entryEBP - StoreDisplacement);

    public static CapacityFinalCalleeLargeFrameFirstFieldTestPlan Project(
        CapacityFinalCalleeLargeFrameFirstFieldTestCapture capture)
    {
        var reached = ImmutableArray.CreateBuilder<uint>();
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeFrameFirstFieldTestRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeFrameFirstFieldTestWrite>();
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
        CapacityFinalCalleeLargeFrameFirstFieldTestFlags? testFlags = null;
        bool branchReached = false;
        bool? branchTaken = null;
        uint? branchDestination = null;

        CapacityFinalCalleeLargeFrameFirstFieldTestPlan Finish(
            CapacityFinalCalleeLargeFrameFirstFieldTestOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEDXCaptured = entryEDXCaptured,
                FreshEAXReadReached = freshReadReached,
                FreshEDXPlus14Address = freshAddress,
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
                NonzeroBranchReached = branchReached,
                NonzeroBranchTaken = branchTaken,
                NonzeroBranchSelectedDestinationRawFileOffset = branchDestination,
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
                CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.MissingEntryEDXForFreshRead,
                FreshReadRawFileOffset);
        }

        entryEDXCaptured = true;
        freshAddress = DeriveFreshEDXPlus14Address(entryEDX);
        reads.Add(new(
            FreshReadRawFileOffset,
            freshAddress,
            capture.FreshEAX.HasValue,
            capture.FreshEAX));
        if (capture.FreshEAX is not uint freshEAX)
        {
            return Finish(
                CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.MissingFreshEAX,
                FreshReadRawFileOffset);
        }

        freshCaptured = true;
        freshValue = freshEAX;

        leaReached = true;
        reached.Add(LeaEcxRawFileOffset);
        leaValue = DeriveFreshEDXPlus14Address(entryEDX);

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
                CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.MissingEntryEBPForStore,
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
        reached.Add(NonzeroBranchRawFileOffset);
        branchTaken = freshEAX != 0;
        branchDestination = branchTaken.Value
            ? NonzeroDestinationRawFileOffset
            : BoundaryRawFileOffsetExclusive;
        return Finish(
            branchTaken.Value
                ? CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.FreshEAXNonzeroToTarget
                : CapacityFinalCalleeLargeFrameFirstFieldTestOutcome.FreshEAXZeroToFallthrough,
            branchDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeLargeFrameFirstFieldTestFlags CalculateTestFlags(
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

    private static CapacityFinalCalleeLargeFrameFirstFieldTestInstructionEvidence Instruction(
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
