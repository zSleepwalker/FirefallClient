using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRightPairedStoresCapture(
    uint? EntryEAX,
    uint? EntryECX,
    uint? IncomingEFlags = null);

public enum CapacityFinalCalleeLargeRightPairedStoresOutcome
{
    MissingBothFirstStoreOperands,
    MissingFirstStoreAddress,
    MissingFirstStoreValue,
    Completed
}

public readonly record struct CapacityFinalCalleeLargeRightPairedStoresWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeLargeRightPairedStoresInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public sealed record CapacityFinalCalleeLargeRightPairedStoresPlan(
    CapacityFinalCalleeLargeRightPairedStoresCapture Capture,
    CapacityFinalCalleeLargeRightPairedStoresOutcome Outcome)
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
    public bool SecondStoreAddressAttempted { get; init; }
    public bool SecondStoreValueAttempted { get; init; }
    public uint? SecondStoreAddress { get; init; }
    public uint? SecondStoreValue { get; init; }
    public bool SecondStoreCommitted { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeRightPairedStoresWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeRightPairedStoresWrite>.Empty;
    public bool IncomingEFlagsCaptured { get; init; }
    public uint? IncomingEFlags { get; init; }
    public uint? EFlagsAtStop { get; init; }
    public bool FlagsUnaffected => true;
    public bool ExactCleanCompletion { get; init; }
    public bool BoundaryInstructionExecuted => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>Projects the two ordered DWORD MOV stores at raw 92B7..92BD.</summary>
public static class CapacityFinalCalleeLargeRightPairedStoresProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C92B7;
    public const uint EntryVirtualAddress = 0x016C9EB7;
    public const uint FirstStoreRawFileOffset = 0x012C92B7;
    public const uint SecondStoreRawFileOffset = 0x012C92BA;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C92BD;
    public const uint FirstStoreDisplacement = 0x14;
    public const uint SecondStoreDisplacement = 0x18;

    public const string CapturedWindowBytes = "89 48 14 89 41 18";

    public static ImmutableArray<CapacityFinalCalleeLargeRightPairedStoresInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Store entry ECX at entry EAX+14",
                FirstStoreRawFileOffset,
                "89 48 14"),
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

    public static CapacityFinalCalleeLargeRightPairedStoresPlan Project(
        CapacityFinalCalleeLargeRightPairedStoresCapture capture)
    {
        bool entryEaxCaptured = capture.EntryEAX.HasValue;
        bool entryEcxCaptured = capture.EntryECX.HasValue;
        uint? firstStoreAddress = entryEaxCaptured
            ? DeriveFirstStoreAddress(capture.EntryEAX!.Value)
            : null;
        uint? firstStoreValue = capture.EntryECX;

        CapacityFinalCalleeLargeRightPairedStoresPlan Finish(
            CapacityFinalCalleeLargeRightPairedStoresOutcome outcome,
            bool firstStoreCommitted = false,
            bool secondStoreReached = false,
            bool secondStoreAddressAttempted = false,
            bool secondStoreValueAttempted = false,
            uint? secondStoreAddress = null,
            uint? secondStoreValue = null,
            bool secondStoreCommitted = false,
            ImmutableArray<CapacityFinalCalleeLargeRightPairedStoresWrite> writes = default,
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
                SecondStoreAddressAttempted = secondStoreAddressAttempted,
                SecondStoreValueAttempted = secondStoreValueAttempted,
                SecondStoreAddress = secondStoreAddress,
                SecondStoreValue = secondStoreValue,
                SecondStoreCommitted = secondStoreCommitted,
                Writes = writes.IsDefault
                    ? ImmutableArray<CapacityFinalCalleeLargeRightPairedStoresWrite>.Empty
                    : writes,
                IncomingEFlagsCaptured = capture.IncomingEFlags.HasValue,
                IncomingEFlags = capture.IncomingEFlags,
                EFlagsAtStop = capture.IncomingEFlags,
                ExactCleanCompletion = exactCleanCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (firstStoreAddress is null && firstStoreValue is null)
        {
            return Finish(
                CapacityFinalCalleeLargeRightPairedStoresOutcome.MissingBothFirstStoreOperands);
        }

        if (firstStoreAddress is null)
        {
            return Finish(
                CapacityFinalCalleeLargeRightPairedStoresOutcome.MissingFirstStoreAddress);
        }

        if (firstStoreValue is null)
        {
            return Finish(
                CapacityFinalCalleeLargeRightPairedStoresOutcome.MissingFirstStoreValue);
        }

        var firstWrite = new CapacityFinalCalleeLargeRightPairedStoresWrite(
            FirstStoreRawFileOffset,
            firstStoreAddress.Value,
            firstStoreValue.Value);
        uint secondStoreAddress = DeriveSecondStoreAddress(capture.EntryECX!.Value);
        uint secondStoreValue = capture.EntryEAX!.Value;
        var secondWrite = new CapacityFinalCalleeLargeRightPairedStoresWrite(
            SecondStoreRawFileOffset,
            secondStoreAddress,
            secondStoreValue);

        return Finish(
            CapacityFinalCalleeLargeRightPairedStoresOutcome.Completed,
            firstStoreCommitted: true,
            secondStoreReached: true,
            secondStoreAddressAttempted: true,
            secondStoreValueAttempted: true,
            secondStoreAddress: secondStoreAddress,
            secondStoreValue: secondStoreValue,
            secondStoreCommitted: true,
            writes: ImmutableArray.Create(firstWrite, secondWrite),
            exactCleanCompletion: true,
            stopAt: BoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeLargeRightPairedStoresInstructionEvidence Instruction(
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
