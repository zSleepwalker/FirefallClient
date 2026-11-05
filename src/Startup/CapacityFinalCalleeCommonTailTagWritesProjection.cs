using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonTailTagWritesCapture(
    uint? EntryEDI,
    uint? EntryESI);

public enum CapacityFinalCalleeCommonTailTagWritesOutcome
{
    MissingEntryEDI,
    MissingEntryESI,
    Completed
}

public readonly record struct CapacityFinalCalleeCommonTailTagWritesFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeCommonTailTagWritesWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeCommonTailTagWritesInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public sealed record CapacityFinalCalleeCommonTailTagWritesPlan(
    CapacityFinalCalleeCommonTailTagWritesCapture Capture,
    CapacityFinalCalleeCommonTailTagWritesOutcome Outcome)
{
    public bool EntryEDICaptured { get; init; }
    public bool EAXLoadedFromEDI { get; init; }
    public uint? EAXRetained { get; init; }
    public bool OrInstructionReached { get; init; }
    public uint? EAXAfterOr { get; init; }
    public CapacityFinalCalleeCommonTailTagWritesFlags? OrFlags { get; init; }
    public bool EntryESICaptured { get; init; }
    public bool FirstStoreReached { get; init; }
    public uint? FirstStoreAddress { get; init; }
    public uint? FirstStoreValue { get; init; }
    public bool SecondStoreReached { get; init; }
    public uint? SecondStoreAddress { get; init; }
    public uint? SecondStoreValue { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonTailTagWritesWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonTailTagWritesWrite>.Empty;
    public CapacityFinalCalleeCommonTailTagWritesFlags? FlagsAtStop { get; init; }
    public bool UnreachedEntryESICaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the bounded MOV/OR/two-store tail without entering the following code.
/// </summary>
public static class CapacityFinalCalleeCommonTailTagWritesProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C92BD;
    public const uint EntryVirtualAddress = 0x016C9EBD;
    public const uint LoadEAXRawFileOffset = 0x012C92BD;
    public const uint OrEAXRawFileOffset = 0x012C92BF;
    public const uint FirstStoreRawFileOffset = 0x012C92C2;
    public const uint SecondStoreRawFileOffset = 0x012C92C5;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C92C8;

    public const uint EsiPlus4Displacement = 4;
    public const uint OrMask = 1;

    public const string CapturedWindowBytes =
        "8B C7 83 C8 01 89 46 04 89 3C 37";

    public static ImmutableArray<CapacityFinalCalleeCommonTailTagWritesInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load entry EDI into EAX", LoadEAXRawFileOffset, "8B C7"),
            Instruction("OR EAX with 1", OrEAXRawFileOffset, "83 C8 01"),
            Instruction("Store EAX at entry ESI+4", FirstStoreRawFileOffset, "89 46 04"),
            Instruction("Store entry EDI at entry EDI+ESI",
                SecondStoreRawFileOffset, "89 3C 37"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEsiPlus4Address(uint entryEsi) =>
        unchecked(entryEsi + EsiPlus4Displacement);

    public static uint DeriveEdiPlusEsiAddress(uint entryEdi, uint entryEsi) =>
        unchecked(entryEdi + entryEsi);

    public static uint ApplyOrMask(uint value) => value | OrMask;

    public static CapacityFinalCalleeCommonTailTagWritesPlan Project(
        CapacityFinalCalleeCommonTailTagWritesCapture capture)
    {
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonTailTagWritesWrite>();
        bool ediCaptured = false;
        bool eaxLoaded = false;
        uint? eaxRetained = null;
        bool orReached = false;
        uint? eaxAfterOr = null;
        CapacityFinalCalleeCommonTailTagWritesFlags? orFlags = null;
        bool esiCaptured = false;
        bool firstStoreReached = false;
        uint? firstStoreAddress = null;
        uint? firstStoreValue = null;
        bool secondStoreReached = false;
        uint? secondStoreAddress = null;
        uint? secondStoreValue = null;

        CapacityFinalCalleeCommonTailTagWritesPlan Finish(
            CapacityFinalCalleeCommonTailTagWritesOutcome outcome,
            uint stopAt,
            bool unreachedEsi = false,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEDICaptured = ediCaptured,
                EAXLoadedFromEDI = eaxLoaded,
                EAXRetained = eaxRetained,
                OrInstructionReached = orReached,
                EAXAfterOr = eaxAfterOr,
                OrFlags = orFlags,
                EntryESICaptured = esiCaptured,
                FirstStoreReached = firstStoreReached,
                FirstStoreAddress = firstStoreAddress,
                FirstStoreValue = firstStoreValue,
                SecondStoreReached = secondStoreReached,
                SecondStoreAddress = secondStoreAddress,
                SecondStoreValue = secondStoreValue,
                Writes = writes.ToImmutable(),
                FlagsAtStop = orFlags,
                UnreachedEntryESICaptureProvided = unreachedEsi,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEDI is not uint entryEdi)
        {
            return Finish(
                CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryEDI,
                LoadEAXRawFileOffset,
                unreachedEsi: capture.EntryESI.HasValue);
        }

        ediCaptured = true;
        eaxLoaded = true;
        eaxRetained = entryEdi;
        orReached = true;
        eaxAfterOr = ApplyOrMask(entryEdi);
        orFlags = CalculateOrFlags(eaxAfterOr.Value);

        if (capture.EntryESI is not uint entryEsi)
        {
            return Finish(
                CapacityFinalCalleeCommonTailTagWritesOutcome.MissingEntryESI,
                FirstStoreRawFileOffset);
        }

        esiCaptured = true;
        firstStoreAddress = DeriveEsiPlus4Address(entryEsi);
        firstStoreValue = eaxAfterOr.Value;
        firstStoreReached = true;
        writes.Add(new(
            FirstStoreRawFileOffset,
            firstStoreAddress.Value,
            firstStoreValue.Value));

        secondStoreAddress = DeriveEdiPlusEsiAddress(entryEdi, entryEsi);
        secondStoreValue = entryEdi;
        secondStoreReached = true;
        writes.Add(new(
            SecondStoreRawFileOffset,
            secondStoreAddress.Value,
            secondStoreValue.Value));

        return Finish(
            CapacityFinalCalleeCommonTailTagWritesOutcome.Completed,
            BoundaryRawFileOffsetExclusive,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeCommonTailTagWritesFlags CalculateOrFlags(
        uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeCommonTailTagWritesInstructionEvidence Instruction(
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
