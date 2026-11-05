using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateFrameLoadsCapture(
    uint? EntryEDX,
    uint? EntryEBP,
    uint? FreshEdxPlus8,
    uint? FreshEdxPlusC);

public enum CapacityFinalCalleeAlternateFrameLoadsOutcome
{
    MissingEntryEDX,
    MissingFreshEdxPlus8,
    MissingEntryEBPForFirstWrite,
    MissingFreshEdxPlusC,
    CompareEqualToOpaqueCallBoundary,
    CompareUnequalToBranchTarget
}

public readonly record struct CapacityFinalCalleeAlternateFrameLoadsFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeAlternateFrameLoadsRead(
    uint RawFileOffset,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeAlternateFrameLoadsWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeAlternateFrameLoadsInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeAlternateFrameLoadsPlan(
    CapacityFinalCalleeAlternateFrameLoadsCapture Capture,
    CapacityFinalCalleeAlternateFrameLoadsOutcome Outcome)
{
    public bool EntryEDXCaptured { get; init; }
    public bool FirstReadReached { get; init; }
    public uint? FreshEdxPlus8Address { get; init; }
    public bool FreshEdxPlus8Captured { get; init; }
    public uint? FreshEdxPlus8 { get; init; }
    public bool EntryEBPCaptured { get; init; }
    public bool FirstWriteReached { get; init; }
    public uint? FirstWriteAddress { get; init; }
    public uint? FirstWriteValue { get; init; }
    public bool SecondReadReached { get; init; }
    public uint? FreshEdxPlusCAddress { get; init; }
    public bool FreshEdxPlusCCaptured { get; init; }
    public uint? FreshEdxPlusC { get; init; }
    public bool SecondWriteReached { get; init; }
    public uint? SecondWriteAddress { get; init; }
    public uint? SecondWriteValue { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeAlternateFrameLoadsFlags? CompareFlags { get; init; }
    public bool BranchReached { get; init; }
    public bool? BranchTaken { get; init; }
    public uint? BranchSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<CapacityFinalCalleeAlternateFrameLoadsRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeAlternateFrameLoadsRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeAlternateFrameLoadsWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeAlternateFrameLoadsWrite>.Empty;
    public bool UnreachedEntryEBPCaptureProvided { get; init; }
    public bool UnreachedFreshEdxPlus8CaptureProvided { get; init; }
    public bool UnreachedFreshEdxPlusCCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool OpaqueCallReached => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the paired fresh EDX loads, frame stores, and CMP/JNZ only.
/// The second fresh load is not inferred from the first store when their
/// addresses alias; the opaque call at 911C is never entered.
/// </summary>
public static class CapacityFinalCalleeAlternateFrameLoadsProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C910C;
    public const uint EntryVirtualAddress = 0x016C9D0C;
    public const uint FirstReadRawFileOffset = 0x012C910C;
    public const uint FirstWriteRawFileOffset = 0x012C910F;
    public const uint SecondReadRawFileOffset = 0x012C9112;
    public const uint SecondWriteRawFileOffset = 0x012C9115;
    public const uint CompareRawFileOffset = 0x012C9118;
    public const uint BranchRawFileOffset = 0x012C911A;
    public const uint OpaqueCallRawFileOffset = 0x012C911C;
    public const uint BoundaryRawFileOffsetExclusive = OpaqueCallRawFileOffset;
    public const uint UnequalDestinationRawFileOffset = 0x012C9122;

    public const uint EdxPlus8Displacement = 8;
    public const uint EdxPlusCDisplacement = 0xC;
    public const uint EbpMinus10Displacement = 0x10;
    public const uint EbpMinus14Displacement = 0x14;

    public const string CapturedWindowBytes =
        "8B 42 08 89 45 F0 8B 42 0C 89 45 EC 3B D0 75 06";

    public static ImmutableArray<CapacityFinalCalleeAlternateFrameLoadsInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the fresh DWORD at entry EDX+8 into EAX",
                FirstReadRawFileOffset, "8B 42 08"),
            Instruction("Write EAX to entry EBP-10",
                FirstWriteRawFileOffset, "89 45 F0"),
            Instruction("Load a separate fresh DWORD at entry EDX+C into EAX",
                SecondReadRawFileOffset, "8B 42 0C"),
            Instruction("Write EAX to entry EBP-14",
                SecondWriteRawFileOffset, "89 45 EC"),
            Instruction("Compare entry EDX with the second fresh EAX",
                CompareRawFileOffset, "3B D0"),
            Instruction("Jump to 9122 when EDX and EAX differ",
                BranchRawFileOffset, "75 06",
                branchTargetRawFileOffset: UnequalDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEdxPlus8Address(uint entryEdx) =>
        unchecked(entryEdx + EdxPlus8Displacement);

    public static uint DeriveEdxPlusCAddress(uint entryEdx) =>
        unchecked(entryEdx + EdxPlusCDisplacement);

    public static uint DeriveEbpMinus10Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus10Displacement);

    public static uint DeriveEbpMinus14Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus14Displacement);

    public static CapacityFinalCalleeAlternateFrameLoadsPlan Project(
        CapacityFinalCalleeAlternateFrameLoadsCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeAlternateFrameLoadsRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeAlternateFrameLoadsWrite>();
        bool edxCaptured = false;
        bool firstReadReached = false;
        uint? firstAddress = null;
        bool firstCaptured = false;
        uint? firstValue = null;
        bool ebpCaptured = false;
        bool firstWriteReached = false;
        uint? firstWriteAddress = null;
        uint? firstWriteValue = null;
        bool secondReadReached = false;
        uint? secondAddress = null;
        bool secondCaptured = false;
        uint? secondValue = null;
        bool secondWriteReached = false;
        uint? secondWriteAddress = null;
        uint? secondWriteValue = null;
        bool compareReached = false;
        uint? compareLeft = null;
        uint? compareRight = null;
        uint? compareResult = null;
        CapacityFinalCalleeAlternateFrameLoadsFlags? compareFlags = null;
        bool branchReached = false;
        bool? branchTaken = null;
        uint? branchDestination = null;
        bool unreachedEbp = false;
        bool unreachedFirst = false;
        bool unreachedSecond = false;

        CapacityFinalCalleeAlternateFrameLoadsPlan Finish(
            CapacityFinalCalleeAlternateFrameLoadsOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEDXCaptured = edxCaptured,
                FirstReadReached = firstReadReached,
                FreshEdxPlus8Address = firstAddress,
                FreshEdxPlus8Captured = firstCaptured,
                FreshEdxPlus8 = firstValue,
                EntryEBPCaptured = ebpCaptured,
                FirstWriteReached = firstWriteReached,
                FirstWriteAddress = firstWriteAddress,
                FirstWriteValue = firstWriteValue,
                SecondReadReached = secondReadReached,
                FreshEdxPlusCAddress = secondAddress,
                FreshEdxPlusCCaptured = secondCaptured,
                FreshEdxPlusC = secondValue,
                SecondWriteReached = secondWriteReached,
                SecondWriteAddress = secondWriteAddress,
                SecondWriteValue = secondWriteValue,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareRight,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                BranchReached = branchReached,
                BranchTaken = branchTaken,
                BranchSelectedDestinationRawFileOffset = branchDestination,
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                UnreachedEntryEBPCaptureProvided = unreachedEbp,
                UnreachedFreshEdxPlus8CaptureProvided = unreachedFirst,
                UnreachedFreshEdxPlusCCaptureProvided = unreachedSecond,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        firstReadReached = true;
        if (capture.EntryEDX is not uint entryEdx)
        {
            unreachedFirst = capture.FreshEdxPlus8.HasValue;
            unreachedEbp = capture.EntryEBP.HasValue;
            unreachedSecond = capture.FreshEdxPlusC.HasValue;
            reads.Add(new(FirstReadRawFileOffset, null, false, null));
            return Finish(
                CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingEntryEDX,
                FirstReadRawFileOffset);
        }

        edxCaptured = true;
        firstAddress = DeriveEdxPlus8Address(entryEdx);
        reads.Add(new(
            FirstReadRawFileOffset,
            firstAddress,
            capture.FreshEdxPlus8.HasValue,
            capture.FreshEdxPlus8));
        if (capture.FreshEdxPlus8 is not uint freshPlus8)
        {
            unreachedEbp = capture.EntryEBP.HasValue;
            unreachedSecond = capture.FreshEdxPlusC.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingFreshEdxPlus8,
                FirstReadRawFileOffset);
        }

        firstCaptured = true;
        firstValue = freshPlus8;
        if (capture.EntryEBP is not uint entryEbp)
        {
            unreachedSecond = capture.FreshEdxPlusC.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingEntryEBPForFirstWrite,
                FirstWriteRawFileOffset);
        }

        ebpCaptured = true;
        firstWriteReached = true;
        firstWriteAddress = DeriveEbpMinus10Address(entryEbp);
        firstWriteValue = firstValue;
        writes.Add(new(FirstWriteRawFileOffset, firstWriteAddress.Value, firstValue.Value));

        secondReadReached = true;
        secondAddress = DeriveEdxPlusCAddress(entryEdx);
        reads.Add(new(
            SecondReadRawFileOffset,
            secondAddress,
            capture.FreshEdxPlusC.HasValue,
            capture.FreshEdxPlusC));
        if (capture.FreshEdxPlusC is not uint freshPlusC)
        {
            return Finish(
                CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingFreshEdxPlusC,
                SecondReadRawFileOffset);
        }

        secondCaptured = true;
        secondValue = freshPlusC;
        secondWriteReached = true;
        secondWriteAddress = DeriveEbpMinus14Address(entryEbp);
        secondWriteValue = secondValue;
        writes.Add(new(
            SecondWriteRawFileOffset,
            secondWriteAddress.Value,
            secondValue.Value));

        compareReached = true;
        compareLeft = entryEdx;
        compareRight = freshPlusC;
        compareResult = unchecked(entryEdx - freshPlusC);
        compareFlags = CalculateCompareFlags(entryEdx, freshPlusC, compareResult.Value);
        branchReached = true;
        branchTaken = !compareFlags.Value.Zero;
        branchDestination = branchTaken.Value
            ? UnequalDestinationRawFileOffset
            : OpaqueCallRawFileOffset;
        return Finish(
            branchTaken.Value
                ? CapacityFinalCalleeAlternateFrameLoadsOutcome.CompareUnequalToBranchTarget
                : CapacityFinalCalleeAlternateFrameLoadsOutcome.CompareEqualToOpaqueCallBoundary,
            branchDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeAlternateFrameLoadsFlags CalculateCompareFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeAlternateFrameLoadsInstructionEvidence Instruction(
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
