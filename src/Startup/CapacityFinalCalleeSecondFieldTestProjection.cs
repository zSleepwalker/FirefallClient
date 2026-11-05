using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeSecondFieldTestCapture(
    uint? EntryESI,
    uint? FreshECX);

public enum CapacityFinalCalleeSecondFieldTestOutcome
{
    MissingEntryESIForRead,
    MissingFreshECX,
    FreshECXZeroToTarget,
    FreshECXNonzeroToBoundary
}

public readonly record struct CapacityFinalCalleeSecondFieldTestFlags(
    bool Carry,
    bool Parity,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeSecondFieldTestRead(
    uint RawFileOffset,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeSecondFieldTestInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeSecondFieldTestPlan(
    CapacityFinalCalleeSecondFieldTestCapture Capture,
    CapacityFinalCalleeSecondFieldTestOutcome Outcome)
{
    public bool EntryESICaptured { get; init; }
    public uint? FreshEsiPlus14Address { get; init; }
    public bool FreshECXReadReached { get; init; }
    public bool FreshECXCaptured { get; init; }
    public uint? FreshECX { get; init; }
    public bool TestECXReached { get; init; }
    public CapacityFinalCalleeSecondFieldTestFlags? TestECXFlags { get; init; }
    public bool ZeroBranchReached { get; init; }
    public bool? ZeroBranchTaken { get; init; }
    public uint? ZeroBranchSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<CapacityFinalCalleeSecondFieldTestRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeSecondFieldTestRead>.Empty;
    public bool UnreachedFreshECXCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the standalone fresh ESI+14 load and TEST/JZ path at 9080 only.
/// </summary>
public static class CapacityFinalCalleeSecondFieldTestProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9080;
    public const uint EntryVirtualAddress = 0x016C9C80;
    public const uint FreshReadRawFileOffset = 0x012C9080;
    public const uint TestEcxRawFileOffset = 0x012C9083;
    public const uint ZeroBranchRawFileOffset = 0x012C9085;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C908B;

    public const uint ZeroDestinationRawFileOffset = 0x012C8EF2;
    public const uint FreshReadDisplacement = 0x14;

    public const string CapturedWindowBytes =
        "8B 4E 14 85 C9 0F 84 67 FE FF FF";

    public static ImmutableArray<CapacityFinalCalleeSecondFieldTestInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load a fresh DWORD from entry ESI+14 into ECX",
                FreshReadRawFileOffset, "8B 4E 14"),
            Instruction("Test fresh ECX against zero",
                TestEcxRawFileOffset, "85 C9"),
            Instruction("Jump to 8EF2 when fresh ECX is zero",
                ZeroBranchRawFileOffset, "0F 84 67 FE FF FF",
                branchTargetRawFileOffset: ZeroDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshEsiPlus14Address(uint entryEsi) =>
        unchecked(entryEsi + FreshReadDisplacement);

    public static CapacityFinalCalleeSecondFieldTestPlan Project(
        CapacityFinalCalleeSecondFieldTestCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeSecondFieldTestRead>();
        bool entryEsiCaptured = false;
        uint? freshAddress = null;
        bool freshReadReached = false;
        bool freshCaptured = false;
        uint? freshValue = null;
        bool testReached = false;
        CapacityFinalCalleeSecondFieldTestFlags? testFlags = null;
        bool branchReached = false;
        bool? branchTaken = null;
        uint? branchDestination = null;
        bool unreachedFresh = false;

        CapacityFinalCalleeSecondFieldTestPlan Finish(
            CapacityFinalCalleeSecondFieldTestOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryESICaptured = entryEsiCaptured,
                FreshEsiPlus14Address = freshAddress,
                FreshECXReadReached = freshReadReached,
                FreshECXCaptured = freshCaptured,
                FreshECX = freshValue,
                TestECXReached = testReached,
                TestECXFlags = testFlags,
                ZeroBranchReached = branchReached,
                ZeroBranchTaken = branchTaken,
                ZeroBranchSelectedDestinationRawFileOffset = branchDestination,
                Reads = reads.ToImmutable(),
                UnreachedFreshECXCaptureProvided = unreachedFresh,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        freshReadReached = true;
        if (capture.EntryESI is not uint entryEsi)
        {
            unreachedFresh = capture.FreshECX.HasValue;
            reads.Add(new(
                FreshReadRawFileOffset,
                Address: null,
                ValueCaptured: false,
                Value: null));
            return Finish(
                CapacityFinalCalleeSecondFieldTestOutcome.MissingEntryESIForRead,
                EntryRawFileOffset);
        }

        entryEsiCaptured = true;
        freshAddress = DeriveFreshEsiPlus14Address(entryEsi);
        reads.Add(new(
            FreshReadRawFileOffset,
            freshAddress,
            capture.FreshECX.HasValue,
            capture.FreshECX));
        if (capture.FreshECX is not uint freshEcx)
        {
            return Finish(
                CapacityFinalCalleeSecondFieldTestOutcome.MissingFreshECX,
                FreshReadRawFileOffset);
        }

        freshCaptured = true;
        freshValue = freshEcx;
        testReached = true;
        testFlags = CalculateTestFlags(freshEcx);
        branchReached = true;
        branchTaken = testFlags.Value.Zero;
        branchDestination = branchTaken.Value
            ? ZeroDestinationRawFileOffset
            : BoundaryRawFileOffsetExclusive;
        return Finish(
            branchTaken.Value
                ? CapacityFinalCalleeSecondFieldTestOutcome.FreshECXZeroToTarget
                : CapacityFinalCalleeSecondFieldTestOutcome.FreshECXNonzeroToBoundary,
            branchDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeSecondFieldTestFlags CalculateTestFlags(uint value) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)value),
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeSecondFieldTestInstructionEvidence Instruction(
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
