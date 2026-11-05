using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeLeafClearStoreCapture(
    uint? EntryEBP,
    uint? FreshFrameMinus4);

public enum CapacityFinalCalleeLargeLeafClearStoreOutcome
{
    MissingEntryEBP,
    MissingFreshFrameMinus4,
    StoredZero
}

public enum CapacityFinalCalleeLargeLeafClearStoreEventKind
{
    Read,
    Write
}

public readonly record struct CapacityFinalCalleeLargeLeafClearStoreInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct CapacityFinalCalleeLargeLeafClearStoreRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeLeafClearStoreWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public readonly record struct CapacityFinalCalleeLargeLeafClearStoreEvent(
    CapacityFinalCalleeLargeLeafClearStoreEventKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public sealed record CapacityFinalCalleeLargeLeafClearStorePlan(
    CapacityFinalCalleeLargeLeafClearStoreCapture Capture,
    CapacityFinalCalleeLargeLeafClearStoreOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeLargeLeafClearStoreRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeLeafClearStoreRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeLeafClearStoreWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeLeafClearStoreWrite>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeLeafClearStoreEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeLeafClearStoreEvent>.Empty;
    public bool EntryEBPCaptured { get; init; }
    public bool FreshFrameReadReached { get; init; }
    public uint? FreshFrameAddress { get; init; }
    public bool FreshFrameCaptured { get; init; }
    public uint? FreshFrameValue { get; init; }
    public bool StoreReached { get; init; }
    public uint? StoreAddress { get; init; }
    public uint? StoreValue { get; init; }
    public bool UnreachedFreshFrameCaptureProvided { get; init; }
    public bool FlagsUnaffected { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool DestinationEntered { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the fresh EBP-4 load and following zero store without reading or
/// modifying process memory.
/// </summary>
public static class CapacityFinalCalleeLargeLeafClearStoreProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C921D;
    public const uint EntryFlatIdaAddress = 0x012C921D;
    public const uint EntryVirtualAddress = 0x016C9E1D;
    public const uint FreshFrameReadRawFileOffset = 0x012C921D;
    public const uint StoreRawFileOffset = 0x012C9220;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9226;
    public const uint EbpMinus4Displacement = 4;

    public const string CapturedWindowBytes =
        "8B 4D FC C7 01 00 00 00 00";

    public static ImmutableArray<CapacityFinalCalleeLargeLeafClearStoreInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the fresh DWORD at entry EBP-4 into ECX",
                FreshFrameReadRawFileOffset, "8B 4D FC"),
            Instruction("Store zero to the address freshly loaded into ECX",
                StoreRawFileOffset, "C7 01 00 00 00 00"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEbpMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - EbpMinus4Displacement);

    public static CapacityFinalCalleeLargeLeafClearStorePlan Project(
        CapacityFinalCalleeLargeLeafClearStoreCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeLeafClearStoreRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeLeafClearStoreWrite>();
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeLeafClearStoreEvent>();
        bool readReached = false;
        uint? readAddress = null;
        bool readCaptured = false;
        uint? readValue = null;
        bool storeReached = false;
        uint? storeAddress = null;

        CapacityFinalCalleeLargeLeafClearStorePlan Finish(
            CapacityFinalCalleeLargeLeafClearStoreOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                Events = events.ToImmutable(),
                EntryEBPCaptured = capture.EntryEBP.HasValue,
                FreshFrameReadReached = readReached,
                FreshFrameAddress = readAddress,
                FreshFrameCaptured = readCaptured,
                FreshFrameValue = readValue,
                StoreReached = storeReached,
                StoreAddress = storeAddress,
                StoreValue = storeReached ? 0u : null,
                UnreachedFreshFrameCaptureProvided =
                    !readReached && capture.FreshFrameMinus4.HasValue,
                FlagsUnaffected = true,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                DestinationEntered = false,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEBP is not uint entryEbp)
        {
            return Finish(
                CapacityFinalCalleeLargeLeafClearStoreOutcome.MissingEntryEBP,
                EntryRawFileOffset);
        }

        readReached = true;
        readAddress = DeriveEbpMinus4Address(entryEbp);
        readCaptured = capture.FreshFrameMinus4.HasValue;
        readValue = capture.FreshFrameMinus4;
        reads.Add(new(
            FreshFrameReadRawFileOffset,
            readAddress,
            AddressCaptured: true,
            ValueCaptured: readCaptured,
            Value: readValue));
        events.Add(new(
            CapacityFinalCalleeLargeLeafClearStoreEventKind.Read,
            FreshFrameReadRawFileOffset,
            readAddress,
            AddressCaptured: true,
            ValueCaptured: readCaptured,
            Value: readValue));

        if (capture.FreshFrameMinus4 is not uint freshFrame)
        {
            return Finish(
                CapacityFinalCalleeLargeLeafClearStoreOutcome.MissingFreshFrameMinus4,
                FreshFrameReadRawFileOffset);
        }

        storeReached = true;
        storeAddress = freshFrame;
        writes.Add(new(
            StoreRawFileOffset,
            storeAddress,
            AddressCaptured: true,
            ValueCaptured: true,
            Value: 0));
        events.Add(new(
            CapacityFinalCalleeLargeLeafClearStoreEventKind.Write,
            StoreRawFileOffset,
            storeAddress,
            AddressCaptured: true,
            ValueCaptured: true,
            Value: 0));

        return Finish(
            CapacityFinalCalleeLargeLeafClearStoreOutcome.StoredZero,
            BoundaryRawFileOffsetExclusive,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeLargeLeafClearStoreInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes);
}
