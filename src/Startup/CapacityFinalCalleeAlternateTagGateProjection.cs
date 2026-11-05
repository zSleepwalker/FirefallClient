using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateTagGateCapture(
    uint? EntryEDX,
    uint? FreshDWORD);

public enum CapacityFinalCalleeAlternateTagGateOutcome
{
    MissingEntryEDXForRead,
    MissingFreshDWORD,
    MaskedTagNotThreeToAlternateTarget,
    MaskedTagThreeToBoundary
}

public readonly record struct CapacityFinalCalleeAlternateTagGateCompareFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeAlternateTagGateRead(
    uint RawFileOffset,
    uint? Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeAlternateTagGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeAlternateTagGatePlan(
    CapacityFinalCalleeAlternateTagGateCapture Capture,
    CapacityFinalCalleeAlternateTagGateOutcome Outcome)
{
    public bool EntryEDXCaptured { get; init; }
    public bool FreshDWORDReadReached { get; init; }
    public uint? FreshDWORDAddress { get; init; }
    public bool FreshDWORDCaptured { get; init; }
    public uint? FreshDWORD { get; init; }
    public bool AndReached { get; init; }
    public uint? AndResult { get; init; }
    public bool ByteCompareReached { get; init; }
    public byte? ByteCompareResult { get; init; }
    public CapacityFinalCalleeAlternateTagGateCompareFlags? ByteCompareFlags { get; init; }
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? JnzSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<CapacityFinalCalleeAlternateTagGateRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeAlternateTagGateRead>.Empty;
    public bool UnreachedFreshDWORDCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the bounded fresh-load and alternate-tag gate at 90A2 only.
/// This small window does not establish ownership or broader object semantics.
/// </summary>
public static class CapacityFinalCalleeAlternateTagGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C90A2;
    public const uint EntryVirtualAddress = 0x016C9CA2;
    public const uint FreshReadRawFileOffset = 0x012C90A2;
    public const uint AndRawFileOffset = 0x012C90A5;
    public const uint ByteCompareRawFileOffset = 0x012C90A8;
    public const uint JnzRawFileOffset = 0x012C90AA;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C90B0;

    public const uint AlternateDestinationRawFileOffset = 0x012C8EF8;
    public const uint FreshReadDisplacement = 4;
    public const uint TagMask = 3;
    public const byte ComparedTag = 3;

    public const uint InboundZeroBranchRawFileOffset = 0x012C8E51;
    public const string InboundZeroBranchBytes = "0F 84 4B 02 00 00";
    public const uint AdjacentNonFallthroughJumpRawFileOffset = 0x012C909D;

    public const string CapturedWindowBytes =
        "8B 42 04 83 E0 03 3C 03 0F 85 48 FE FF FF";

    public static CapacityFinalCalleeAlternateTagGateInstructionEvidence
        InboundZeroBranch { get; } =
        Instruction("Inbound JZ targets this block entry",
            InboundZeroBranchRawFileOffset, InboundZeroBranchBytes,
            branchTargetRawFileOffset: EntryRawFileOffset);

    public static ImmutableArray<CapacityFinalCalleeAlternateTagGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load a fresh DWORD from entry EDX+4 into EAX",
                FreshReadRawFileOffset, "8B 42 04"),
            Instruction("Mask EAX to its low two tag bits",
                AndRawFileOffset, "83 E0 03"),
            Instruction("Compare AL with 3 using byte-width CMP flags",
                ByteCompareRawFileOffset, "3C 03"),
            Instruction("Jump to 8EF8 when masked AL is not 3",
                JnzRawFileOffset, "0F 85 48 FE FF FF",
                branchTargetRawFileOffset: AlternateDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshEdxPlus4Address(uint entryEdx) =>
        unchecked(entryEdx + FreshReadDisplacement);

    public static CapacityFinalCalleeAlternateTagGatePlan Project(
        CapacityFinalCalleeAlternateTagGateCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeAlternateTagGateRead>();
        bool entryEdxCaptured = false;
        bool freshReadReached = true;
        uint? freshAddress = null;
        bool freshCaptured = false;
        uint? freshValue = null;
        bool andReached = false;
        uint? andResult = null;
        bool compareReached = false;
        byte? compareResult = null;
        CapacityFinalCalleeAlternateTagGateCompareFlags? compareFlags = null;
        bool jnzReached = false;
        bool? jnzTaken = null;
        uint? jnzDestination = null;
        bool unreachedFresh = false;

        CapacityFinalCalleeAlternateTagGatePlan Finish(
            CapacityFinalCalleeAlternateTagGateOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEDXCaptured = entryEdxCaptured,
                FreshDWORDReadReached = freshReadReached,
                FreshDWORDAddress = freshAddress,
                FreshDWORDCaptured = freshCaptured,
                FreshDWORD = freshValue,
                AndReached = andReached,
                AndResult = andResult,
                ByteCompareReached = compareReached,
                ByteCompareResult = compareResult,
                ByteCompareFlags = compareFlags,
                JnzReached = jnzReached,
                JnzTaken = jnzTaken,
                JnzSelectedDestinationRawFileOffset = jnzDestination,
                Reads = reads.ToImmutable(),
                UnreachedFreshDWORDCaptureProvided = unreachedFresh,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEDX is not uint entryEdx)
        {
            unreachedFresh = capture.FreshDWORD.HasValue;
            reads.Add(new(
                FreshReadRawFileOffset,
                Address: null,
                ValueCaptured: false,
                Value: null));
            return Finish(
                CapacityFinalCalleeAlternateTagGateOutcome.MissingEntryEDXForRead,
                FreshReadRawFileOffset);
        }

        entryEdxCaptured = true;
        freshAddress = DeriveFreshEdxPlus4Address(entryEdx);
        reads.Add(new(
            FreshReadRawFileOffset,
            freshAddress,
            capture.FreshDWORD.HasValue,
            capture.FreshDWORD));
        if (capture.FreshDWORD is not uint freshDword)
        {
            return Finish(
                CapacityFinalCalleeAlternateTagGateOutcome.MissingFreshDWORD,
                FreshReadRawFileOffset);
        }

        freshCaptured = true;
        freshValue = freshDword;
        andReached = true;
        uint maskedTag = freshDword & TagMask;
        andResult = maskedTag;
        compareReached = true;
        byte left = (byte)maskedTag;
        byte difference = unchecked((byte)(left - ComparedTag));
        compareResult = difference;
        compareFlags = CalculateByteCompareFlags(left, ComparedTag, difference);
        jnzReached = true;
        jnzTaken = difference != 0;
        jnzDestination = jnzTaken.Value
            ? AlternateDestinationRawFileOffset
            : BoundaryRawFileOffsetExclusive;
        return Finish(
            jnzTaken.Value
                ? CapacityFinalCalleeAlternateTagGateOutcome
                    .MaskedTagNotThreeToAlternateTarget
                : CapacityFinalCalleeAlternateTagGateOutcome.MaskedTagThreeToBoundary,
            jnzDestination.Value,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeAlternateTagGateCompareFlags
        CalculateByteCompareFlags(byte left, byte right, byte result) =>
        new(
            left < right,
            HasEvenParity(result),
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80) != 0,
            ((left ^ right) & (left ^ result) & 0x80) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeAlternateTagGateInstructionEvidence Instruction(
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
