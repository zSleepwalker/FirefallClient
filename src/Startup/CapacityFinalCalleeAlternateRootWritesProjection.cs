using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateRootWritesCapture(
    uint? FreshGlobal,
    uint? EntryEDI,
    uint? EntryESI);

public enum CapacityFinalCalleeAlternateRootWritesOutcome
{
    MissingFreshGlobal,
    MissingEntryEDI,
    MissingEntryESI,
    Completed
}

public readonly record struct CapacityFinalCalleeAlternateRootWritesAddFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeAlternateRootWritesOrFlags(
    bool Carry,
    bool Parity,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeAlternateRootWritesWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeAlternateRootWritesInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeAlternateRootWritesPlan(
    CapacityFinalCalleeAlternateRootWritesCapture Capture,
    CapacityFinalCalleeAlternateRootWritesOutcome Outcome)
{
    public bool FreshGlobalReadReached { get; init; }
    public bool FreshGlobalCaptured { get; init; }
    public uint? FreshGlobal { get; init; }
    public bool EntryEDICaptured { get; init; }
    public bool AddInstructionReached { get; init; }
    public uint? AddResult { get; init; }
    public CapacityFinalCalleeAlternateRootWritesAddFlags? AddFlags { get; init; }
    public bool EntryESICaptured { get; init; }
    public bool FirstStoreReached { get; init; }
    public uint? FirstStoreAddress { get; init; }
    public uint? FirstStoreValue { get; init; }
    public bool EAXLoadedFromECX { get; init; }
    public uint? EAXRetained { get; init; }
    public bool GlobalStoreReached { get; init; }
    public uint? GlobalStoreAddress { get; init; }
    public uint? GlobalStoreValue { get; init; }
    public bool OrInstructionReached { get; init; }
    public uint? EAXAfterOr { get; init; }
    public CapacityFinalCalleeAlternateRootWritesOrFlags? OrFlags { get; init; }
    public bool EsiPlus4StoreReached { get; init; }
    public uint? EsiPlus4StoreAddress { get; init; }
    public uint? EsiPlus4StoreValue { get; init; }
    public bool EsiPlusEcxStoreReached { get; init; }
    public uint? EsiPlusEcxStoreAddress { get; init; }
    public uint? EsiPlusEcxStoreValue { get; init; }
    public ImmutableArray<CapacityFinalCalleeAlternateRootWritesWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeAlternateRootWritesWrite>.Empty;
    public bool JumpReached { get; init; }
    public uint? JumpTargetRawFileOffset { get; init; }
    public uint? JumpTargetVirtualAddress { get; init; }
    public bool UnreachedEntryEDICaptureProvided { get; init; }
    public bool UnreachedEntryESICaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the bounded scalar root-write sequence without assigning allocator semantics.
/// The alternate-root gate is independent of this native instruction window.
/// </summary>
public static class CapacityFinalCalleeAlternateRootWritesProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C90D2;
    public const uint EntryVirtualAddress = 0x016C9CD2;
    public const uint FreshGlobalReadRawFileOffset = 0x012C90D2;
    public const uint AddRawFileOffset = 0x012C90D8;
    public const uint FirstStoreRawFileOffset = 0x012C90DA;
    public const uint LoadEAXRawFileOffset = 0x012C90E0;
    public const uint GlobalStoreRawFileOffset = 0x012C90E2;
    public const uint OrEAXRawFileOffset = 0x012C90E8;
    public const uint EsiPlus4StoreRawFileOffset = 0x012C90EB;
    public const uint EsiPlusEcxStoreRawFileOffset = 0x012C90EE;
    public const uint JumpRawFileOffset = 0x012C90F1;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C90F6;
    public const uint JumpTargetRawFileOffset = 0x012C9441;
    public const uint JumpTargetVirtualAddress = 0x016CA041;

    public const uint FreshGlobalPeVirtualAddress = 0x01EDADD0;
    public const uint FirstStorePeVirtualAddress = 0x01EDADDC;
    public const uint EsiPlus4Displacement = 4;
    public const uint OrMask = 1;

    public const string CapturedWindowBytes =
        "8B 0D D0 AD ED 01 03 CF 89 35 DC AD ED 01 8B C1 "
        + "89 0D D0 AD ED 01 83 C8 01 89 46 04 89 0C 31 "
        + "E9 4B 03 00 00";

    public static ImmutableArray<CapacityFinalCalleeAlternateRootWritesInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the fresh DWORD at absolute PE VA 1EDADD0 into ECX",
                FreshGlobalReadRawFileOffset, "8B 0D D0 AD ED 01",
                peVirtualAddressOperand: FreshGlobalPeVirtualAddress),
            Instruction("Add entry EDI to ECX",
                AddRawFileOffset, "03 CF"),
            Instruction("Write entry ESI to absolute PE VA 1EDADDC",
                FirstStoreRawFileOffset, "89 35 DC AD ED 01",
                peVirtualAddressOperand: FirstStorePeVirtualAddress),
            Instruction("Load ECX into EAX",
                LoadEAXRawFileOffset, "8B C1"),
            Instruction("Write ECX to absolute PE VA 1EDADD0",
                GlobalStoreRawFileOffset, "89 0D D0 AD ED 01",
                peVirtualAddressOperand: FreshGlobalPeVirtualAddress),
            Instruction("OR EAX with 1",
                OrEAXRawFileOffset, "83 C8 01"),
            Instruction("Write EAX to entry ESI+4",
                EsiPlus4StoreRawFileOffset, "89 46 04"),
            Instruction("Write ECX to entry ESI+ECX",
                EsiPlusEcxStoreRawFileOffset, "89 0C 31"),
            Instruction("Jump to raw offset 12C9441",
                JumpRawFileOffset, "E9 4B 03 00 00",
                branchTargetRawFileOffset: JumpTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ApplyAdd(uint freshGlobal, uint entryEdi) =>
        unchecked(freshGlobal + entryEdi);

    public static uint ApplyOr(uint value) => value | OrMask;

    public static uint DeriveEsiPlus4Address(uint entryEsi) =>
        unchecked(entryEsi + EsiPlus4Displacement);

    public static uint DeriveEsiPlusEcxAddress(uint entryEsi, uint ecx) =>
        unchecked(entryEsi + ecx);

    public static CapacityFinalCalleeAlternateRootWritesPlan Project(
        CapacityFinalCalleeAlternateRootWritesCapture capture)
    {
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeAlternateRootWritesWrite>();
        bool freshReadReached = true;
        bool freshCaptured = false;
        bool entryEdiCaptured = false;
        bool addReached = false;
        uint? addResult = null;
        CapacityFinalCalleeAlternateRootWritesAddFlags? addFlags = null;
        bool entryEsiCaptured = false;
        bool firstStoreReached = false;
        uint? firstStoreAddress = null;
        uint? firstStoreValue = null;
        bool eaxLoaded = false;
        uint? eaxRetained = null;
        bool globalStoreReached = false;
        uint? globalStoreAddress = null;
        uint? globalStoreValue = null;
        bool orReached = false;
        uint? eaxAfterOr = null;
        CapacityFinalCalleeAlternateRootWritesOrFlags? orFlags = null;
        bool esiPlus4StoreReached = false;
        uint? esiPlus4StoreAddress = null;
        uint? esiPlus4StoreValue = null;
        bool esiPlusEcxStoreReached = false;
        uint? esiPlusEcxStoreAddress = null;
        uint? esiPlusEcxStoreValue = null;
        bool jumpReached = false;
        bool unreachedEdi = false;
        bool unreachedEsi = false;

        CapacityFinalCalleeAlternateRootWritesPlan Finish(
            CapacityFinalCalleeAlternateRootWritesOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                FreshGlobalReadReached = freshReadReached,
                FreshGlobalCaptured = freshCaptured,
                FreshGlobal = freshCaptured ? capture.FreshGlobal : null,
                EntryEDICaptured = entryEdiCaptured,
                AddInstructionReached = addReached,
                AddResult = addResult,
                AddFlags = addFlags,
                EntryESICaptured = entryEsiCaptured,
                FirstStoreReached = firstStoreReached,
                FirstStoreAddress = firstStoreAddress,
                FirstStoreValue = firstStoreValue,
                EAXLoadedFromECX = eaxLoaded,
                EAXRetained = eaxRetained,
                GlobalStoreReached = globalStoreReached,
                GlobalStoreAddress = globalStoreAddress,
                GlobalStoreValue = globalStoreValue,
                OrInstructionReached = orReached,
                EAXAfterOr = eaxAfterOr,
                OrFlags = orFlags,
                EsiPlus4StoreReached = esiPlus4StoreReached,
                EsiPlus4StoreAddress = esiPlus4StoreAddress,
                EsiPlus4StoreValue = esiPlus4StoreValue,
                EsiPlusEcxStoreReached = esiPlusEcxStoreReached,
                EsiPlusEcxStoreAddress = esiPlusEcxStoreAddress,
                EsiPlusEcxStoreValue = esiPlusEcxStoreValue,
                Writes = writes.ToImmutable(),
                JumpReached = jumpReached,
                JumpTargetRawFileOffset = jumpReached ? JumpTargetRawFileOffset : null,
                JumpTargetVirtualAddress = jumpReached ? JumpTargetVirtualAddress : null,
                UnreachedEntryEDICaptureProvided = unreachedEdi,
                UnreachedEntryESICaptureProvided = unreachedEsi,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.FreshGlobal is not uint freshGlobal)
        {
            unreachedEdi = capture.EntryEDI.HasValue;
            unreachedEsi = capture.EntryESI.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateRootWritesOutcome.MissingFreshGlobal,
                FreshGlobalReadRawFileOffset);
        }

        freshCaptured = true;
        if (capture.EntryEDI is not uint entryEdi)
        {
            unreachedEsi = capture.EntryESI.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateRootWritesOutcome.MissingEntryEDI,
                AddRawFileOffset);
        }

        entryEdiCaptured = true;
        addReached = true;
        uint ecx = ApplyAdd(freshGlobal, entryEdi);
        addResult = ecx;
        addFlags = CalculateAddFlags(freshGlobal, entryEdi, ecx);

        if (capture.EntryESI is not uint entryEsi)
        {
            return Finish(
                CapacityFinalCalleeAlternateRootWritesOutcome.MissingEntryESI,
                FirstStoreRawFileOffset);
        }

        entryEsiCaptured = true;
        firstStoreReached = true;
        firstStoreAddress = FirstStorePeVirtualAddress;
        firstStoreValue = entryEsi;
        writes.Add(new(FirstStoreRawFileOffset, firstStoreAddress.Value, entryEsi));

        eaxLoaded = true;
        eaxRetained = ecx;
        globalStoreReached = true;
        globalStoreAddress = FreshGlobalPeVirtualAddress;
        globalStoreValue = ecx;
        writes.Add(new(GlobalStoreRawFileOffset, globalStoreAddress.Value, ecx));

        orReached = true;
        uint eaxAfterOrValue = ApplyOr(ecx);
        eaxAfterOr = eaxAfterOrValue;
        orFlags = CalculateOrFlags(eaxAfterOrValue);

        esiPlus4StoreReached = true;
        esiPlus4StoreAddress = DeriveEsiPlus4Address(entryEsi);
        esiPlus4StoreValue = eaxAfterOrValue;
        writes.Add(new(
            EsiPlus4StoreRawFileOffset,
            esiPlus4StoreAddress.Value,
            eaxAfterOrValue));

        esiPlusEcxStoreReached = true;
        esiPlusEcxStoreAddress = DeriveEsiPlusEcxAddress(entryEsi, ecx);
        esiPlusEcxStoreValue = ecx;
        writes.Add(new(
            EsiPlusEcxStoreRawFileOffset,
            esiPlusEcxStoreAddress.Value,
            ecx));

        jumpReached = true;
        return Finish(
            CapacityFinalCalleeAlternateRootWritesOutcome.Completed,
            JumpTargetRawFileOffset,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeAlternateRootWritesAddFlags CalculateAddFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: (ulong)left + right > uint.MaxValue,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((~(left ^ right) & (left ^ result)) & 0x80000000) != 0);

    private static CapacityFinalCalleeAlternateRootWritesOrFlags CalculateOrFlags(
        uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeAlternateRootWritesInstructionEvidence Instruction(
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
