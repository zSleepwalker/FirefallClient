using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeNextPairedStoresCapture(
    uint? EntryEAX,
    uint? EntryECX);

public enum CapacityFinalCalleeLargeNextPairedStoresOutcome
{
    MissingBothFirstStoreOperands,
    MissingFirstStoreAddress,
    MissingFirstStoreValue,
    Completed
}

public readonly record struct CapacityFinalCalleeLargeNextPairedStoresWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeLargeNextPairedStoresInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public sealed record CapacityFinalCalleeLargeNextPairedStoresPlan(
    CapacityFinalCalleeLargeNextPairedStoresCapture Capture,
    CapacityFinalCalleeLargeNextPairedStoresOutcome Outcome)
{
    public bool EntryEAXCaptured { get; init; }
    public bool EntryECXCaptured { get; init; }
    public bool FirstStoreReached { get; init; }
    public bool FirstStoreAddressAttempted { get; init; }
    public bool FirstStoreValueAttempted { get; init; }
    public uint? FirstStoreAddress { get; init; }
    public uint? FirstStoreValue { get; init; }
    public bool FirstStoreCommitted { get; init; }
    public bool SecondStoreReached { get; init; }
    public uint? SecondStoreAddress { get; init; }
    public uint? SecondStoreValue { get; init; }
    public bool SecondStoreCommitted { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeNextPairedStoresWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeNextPairedStoresWrite>.Empty;
    public bool FlagsUnaffected => true;
    public bool ExactCleanCompletion { get; init; }
    public bool BoundaryInstructionExecuted => false;
    public bool FreshReadReached => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the two ordered MOV stores at raw 929E..92A4 without executing the following read.
/// </summary>
public static class CapacityFinalCalleeLargeNextPairedStoresProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C929E;
    public const uint EntryVirtualAddress = 0x016C9E9E;
    public const uint FirstStoreRawFileOffset = 0x012C929E;
    public const uint SecondStoreRawFileOffset = 0x012C92A1;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C92A4;
    public const uint FreshReadRawFileOffset = BoundaryRawFileOffsetExclusive;
    public const uint FirstStoreDisplacement = 0x10;
    public const uint SecondStoreDisplacement = 0x18;

    public const string CapturedWindowBytes = "89 48 10 89 41 18";

    public static ImmutableArray<CapacityFinalCalleeLargeNextPairedStoresInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Store entry ECX at entry EAX+10",
                FirstStoreRawFileOffset,
                "89 48 10"),
            Instruction(
                "Store entry EAX at entry ECX+18",
                SecondStoreRawFileOffset,
                "89 41 18"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFirstStoreAddress(uint entryEax) =>
        unchecked(entryEax + FirstStoreDisplacement);

    public static uint DeriveSecondStoreAddress(uint entryEcx) =>
        unchecked(entryEcx + SecondStoreDisplacement);

    public static CapacityFinalCalleeLargeNextPairedStoresPlan Project(
        CapacityFinalCalleeLargeNextPairedStoresCapture capture)
    {
        bool entryEaxCaptured = capture.EntryEAX.HasValue;
        bool entryEcxCaptured = capture.EntryECX.HasValue;
        uint? firstStoreAddress = entryEaxCaptured
            ? DeriveFirstStoreAddress(capture.EntryEAX!.Value)
            : null;
        uint? firstStoreValue = capture.EntryECX;

        CapacityFinalCalleeLargeNextPairedStoresPlan Finish(
            CapacityFinalCalleeLargeNextPairedStoresOutcome outcome,
            bool firstStoreCommitted = false,
            bool secondStoreReached = false,
            uint? secondStoreAddress = null,
            uint? secondStoreValue = null,
            bool secondStoreCommitted = false,
            ImmutableArray<CapacityFinalCalleeLargeNextPairedStoresWrite> writes = default,
            bool exactCleanCompletion = false,
            uint stopAt = EntryRawFileOffset) =>
            new(capture, outcome)
            {
                EntryEAXCaptured = entryEaxCaptured,
                EntryECXCaptured = entryEcxCaptured,
                FirstStoreReached = true,
                FirstStoreAddressAttempted = true,
                FirstStoreValueAttempted = true,
                FirstStoreAddress = firstStoreAddress,
                FirstStoreValue = firstStoreValue,
                FirstStoreCommitted = firstStoreCommitted,
                SecondStoreReached = secondStoreReached,
                SecondStoreAddress = secondStoreAddress,
                SecondStoreValue = secondStoreValue,
                SecondStoreCommitted = secondStoreCommitted,
                Writes = writes.IsDefault
                    ? ImmutableArray<CapacityFinalCalleeLargeNextPairedStoresWrite>.Empty
                    : writes,
                ExactCleanCompletion = exactCleanCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (firstStoreAddress is null && firstStoreValue is null)
        {
            return Finish(
                CapacityFinalCalleeLargeNextPairedStoresOutcome.MissingBothFirstStoreOperands);
        }

        if (firstStoreAddress is null)
        {
            return Finish(
                CapacityFinalCalleeLargeNextPairedStoresOutcome.MissingFirstStoreAddress);
        }

        if (firstStoreValue is null)
        {
            return Finish(
                CapacityFinalCalleeLargeNextPairedStoresOutcome.MissingFirstStoreValue);
        }

        var firstWrite = new CapacityFinalCalleeLargeNextPairedStoresWrite(
            FirstStoreRawFileOffset,
            firstStoreAddress.Value,
            firstStoreValue.Value);
        uint secondStoreAddress = DeriveSecondStoreAddress(capture.EntryECX!.Value);
        uint secondStoreValue = capture.EntryEAX!.Value;
        var secondWrite = new CapacityFinalCalleeLargeNextPairedStoresWrite(
            SecondStoreRawFileOffset,
            secondStoreAddress,
            secondStoreValue);

        return Finish(
            CapacityFinalCalleeLargeNextPairedStoresOutcome.Completed,
            firstStoreCommitted: true,
            secondStoreReached: true,
            secondStoreAddress: secondStoreAddress,
            secondStoreValue: secondStoreValue,
            secondStoreCommitted: true,
            writes: ImmutableArray.Create(firstWrite, secondWrite),
            exactCleanCompletion: true,
            stopAt: BoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeLargeNextPairedStoresInstructionEvidence Instruction(
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
