using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateFieldWritesCapture(
    uint? EntryEDI,
    uint? EntryEDX,
    uint? FreshEdxPlus4,
    uint? EntryESI);

public enum CapacityFinalCalleeAlternateFieldWritesOutcome
{
    MissingEntryEDI,
    MissingEntryEDX,
    MissingFreshEdxPlus4,
    MissingEntryESI,
    Completed
}

public readonly record struct CapacityFinalCalleeAlternateFieldWritesFlags(
    bool Carry,
    bool Parity,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeAlternateFieldWritesWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeAlternateFieldWritesInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeAlternateFieldWritesPlan(
    CapacityFinalCalleeAlternateFieldWritesCapture Capture,
    CapacityFinalCalleeAlternateFieldWritesOutcome Outcome)
{
    public bool EntryEDICaptured { get; init; }
    public bool GlobalWriteReached { get; init; }
    public uint? GlobalWriteAddress { get; init; }
    public uint? GlobalWriteValue { get; init; }
    public bool EAXLoadedFromEDI { get; init; }
    public uint? EAXRetained { get; init; }
    public bool EntryEDXCaptured { get; init; }
    public uint? FreshEdxPlus4Address { get; init; }
    public bool FreshEdxPlus4ReadReached { get; init; }
    public bool FreshEdxPlus4Captured { get; init; }
    public uint? FreshEdxPlus4 { get; init; }
    public bool AndInstructionReached { get; init; }
    public uint? AndResult { get; init; }
    public CapacityFinalCalleeAlternateFieldWritesFlags? AndFlags { get; init; }
    public bool AndWriteReached { get; init; }
    public uint? AndWriteAddress { get; init; }
    public uint? AndWriteValue { get; init; }
    public bool OrInstructionReached { get; init; }
    public uint? EAXAfterOr { get; init; }
    public CapacityFinalCalleeAlternateFieldWritesFlags? OrFlags { get; init; }
    public bool EntryESICaptured { get; init; }
    public bool EsiPlus4WriteReached { get; init; }
    public uint? EsiPlus4WriteAddress { get; init; }
    public uint? EsiPlus4WriteValue { get; init; }
    public bool EsiPlusEdiWriteReached { get; init; }
    public uint? EsiPlusEdiWriteAddress { get; init; }
    public uint? EsiPlusEdiWriteValue { get; init; }
    public ImmutableArray<CapacityFinalCalleeAlternateFieldWritesWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeAlternateFieldWritesWrite>.Empty;
    public bool JumpReached { get; init; }
    public uint? JumpTargetRawFileOffset { get; init; }
    public uint? JumpTargetVirtualAddress { get; init; }
    public bool UnreachedEntryEDXCaptureProvided { get; init; }
    public bool UnreachedFreshEdxPlus4CaptureProvided { get; init; }
    public bool UnreachedEntryESICaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the ordered absolute and indexed DWORD writes through the final jump.
/// The fresh EDX+4 value is independent of the preceding absolute global write.
/// </summary>
public static class CapacityFinalCalleeAlternateFieldWritesProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C90B0;
    public const uint EntryVirtualAddress = 0x016C9CB0;
    public const uint GlobalWriteRawFileOffset = 0x012C90B0;
    public const uint LoadEAXRawFileOffset = 0x012C90B6;
    public const uint AndEdxPlus4RawFileOffset = 0x012C90B8;
    public const uint OrEAXRawFileOffset = 0x012C90BC;
    public const uint EsiPlus4WriteRawFileOffset = 0x012C90BF;
    public const uint EsiPlusEdiWriteRawFileOffset = 0x012C90C2;
    public const uint JumpRawFileOffset = 0x012C90C5;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C90CA;
    public const uint JumpTargetRawFileOffset = 0x012C9441;
    public const uint JumpTargetVirtualAddress = 0x016CA041;

    public const uint GlobalWritePeVirtualAddress = 0x01EDADD0;
    public const uint EdxPlus4Displacement = 4;
    public const uint EsiPlus4Displacement = 4;
    public const uint EsiPlusEdiDisplacement = 0;
    public const uint AndMask = 0xFFFFFFFE;
    public const uint OrMask = 1;

    public const string CapturedWindowBytes =
        "89 3D D0 AD ED 01 8B C7 83 62 04 FE 83 C8 01 "
        + "89 46 04 89 3C 3E E9 77 03 00 00";

    public static ImmutableArray<CapacityFinalCalleeAlternateFieldWritesInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Write entry EDI to the absolute PE VA 1EDADD0",
                GlobalWriteRawFileOffset, "89 3D D0 AD ED 01",
                peVirtualAddressOperand: GlobalWritePeVirtualAddress),
            Instruction("Load entry EDI into EAX",
                LoadEAXRawFileOffset, "8B C7"),
            Instruction("AND the fresh DWORD at entry EDX+4 with FFFFFFFE",
                AndEdxPlus4RawFileOffset, "83 62 04 FE"),
            Instruction("OR retained EAX with 1",
                OrEAXRawFileOffset, "83 C8 01"),
            Instruction("Write EAX to entry ESI+4",
                EsiPlus4WriteRawFileOffset, "89 46 04"),
            Instruction("Write entry EDI to entry ESI+EDI",
                EsiPlusEdiWriteRawFileOffset, "89 3C 3E"),
            Instruction("Jump to raw offset 12C9441",
                JumpRawFileOffset, "E9 77 03 00 00",
                branchTargetRawFileOffset: JumpTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEdxPlus4Address(uint entryEdx) =>
        unchecked(entryEdx + EdxPlus4Displacement);

    public static uint DeriveEsiPlus4Address(uint entryEsi) =>
        unchecked(entryEsi + EsiPlus4Displacement);

    public static uint DeriveEsiPlusEdiAddress(uint entryEsi, uint entryEdi) =>
        unchecked(entryEsi + entryEdi + EsiPlusEdiDisplacement);

    public static uint ApplyAndMask(uint value) => value & AndMask;

    public static uint ApplyOrMask(uint value) => value | OrMask;

    public static CapacityFinalCalleeAlternateFieldWritesPlan Project(
        CapacityFinalCalleeAlternateFieldWritesCapture capture)
    {
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeAlternateFieldWritesWrite>();
        bool entryEdiCaptured = false;
        bool globalWriteReached = false;
        uint? globalWriteAddress = null;
        uint? globalWriteValue = null;
        bool eaxLoaded = false;
        uint? eaxRetained = null;
        bool entryEdxCaptured = false;
        uint? freshAddress = null;
        bool freshReadReached = false;
        bool freshCaptured = false;
        bool andReached = false;
        uint? andResult = null;
        CapacityFinalCalleeAlternateFieldWritesFlags? andFlags = null;
        bool andWriteReached = false;
        uint? andWriteAddress = null;
        uint? andWriteValue = null;
        bool orReached = false;
        uint? eaxAfterOr = null;
        CapacityFinalCalleeAlternateFieldWritesFlags? orFlags = null;
        bool entryEsiCaptured = false;
        bool esiPlus4WriteReached = false;
        uint? esiPlus4WriteAddress = null;
        uint? esiPlus4WriteValue = null;
        bool esiPlusEdiWriteReached = false;
        uint? esiPlusEdiWriteAddress = null;
        uint? esiPlusEdiWriteValue = null;
        bool jumpReached = false;
        bool unreachedEdx = false;
        bool unreachedFresh = false;
        bool unreachedEsi = false;

        CapacityFinalCalleeAlternateFieldWritesPlan Finish(
            CapacityFinalCalleeAlternateFieldWritesOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEDICaptured = entryEdiCaptured,
                GlobalWriteReached = globalWriteReached,
                GlobalWriteAddress = globalWriteAddress,
                GlobalWriteValue = globalWriteValue,
                EAXLoadedFromEDI = eaxLoaded,
                EAXRetained = eaxRetained,
                EntryEDXCaptured = entryEdxCaptured,
                FreshEdxPlus4Address = freshAddress,
                FreshEdxPlus4ReadReached = freshReadReached,
                FreshEdxPlus4Captured = freshCaptured,
                FreshEdxPlus4 = freshCaptured ? capture.FreshEdxPlus4 : null,
                AndInstructionReached = andReached,
                AndResult = andResult,
                AndFlags = andFlags,
                AndWriteReached = andWriteReached,
                AndWriteAddress = andWriteAddress,
                AndWriteValue = andWriteValue,
                OrInstructionReached = orReached,
                EAXAfterOr = eaxAfterOr,
                OrFlags = orFlags,
                EntryESICaptured = entryEsiCaptured,
                EsiPlus4WriteReached = esiPlus4WriteReached,
                EsiPlus4WriteAddress = esiPlus4WriteAddress,
                EsiPlus4WriteValue = esiPlus4WriteValue,
                EsiPlusEdiWriteReached = esiPlusEdiWriteReached,
                EsiPlusEdiWriteAddress = esiPlusEdiWriteAddress,
                EsiPlusEdiWriteValue = esiPlusEdiWriteValue,
                Writes = writes.ToImmutable(),
                JumpReached = jumpReached,
                JumpTargetRawFileOffset = jumpReached ? JumpTargetRawFileOffset : null,
                JumpTargetVirtualAddress = jumpReached ? JumpTargetVirtualAddress : null,
                UnreachedEntryEDXCaptureProvided = unreachedEdx,
                UnreachedFreshEdxPlus4CaptureProvided = unreachedFresh,
                UnreachedEntryESICaptureProvided = unreachedEsi,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEDI is not uint entryEdi)
        {
            unreachedEdx = capture.EntryEDX.HasValue;
            unreachedFresh = capture.FreshEdxPlus4.HasValue;
            unreachedEsi = capture.EntryESI.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateFieldWritesOutcome.MissingEntryEDI,
                EntryRawFileOffset);
        }

        entryEdiCaptured = true;
        globalWriteAddress = GlobalWritePeVirtualAddress;
        globalWriteValue = entryEdi;
        globalWriteReached = true;
        writes.Add(new(GlobalWriteRawFileOffset, globalWriteAddress.Value, entryEdi));
        eaxLoaded = true;
        eaxRetained = entryEdi;

        if (capture.EntryEDX is not uint entryEdx)
        {
            unreachedFresh = capture.FreshEdxPlus4.HasValue;
            unreachedEsi = capture.EntryESI.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateFieldWritesOutcome.MissingEntryEDX,
                AndEdxPlus4RawFileOffset);
        }

        entryEdxCaptured = true;
        freshAddress = DeriveEdxPlus4Address(entryEdx);
        freshReadReached = true;
        andReached = true;
        if (capture.FreshEdxPlus4 is not uint freshEdxPlus4)
        {
            unreachedEsi = capture.EntryESI.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateFieldWritesOutcome.MissingFreshEdxPlus4,
                AndEdxPlus4RawFileOffset);
        }
        freshCaptured = true;
        andResult = ApplyAndMask(freshEdxPlus4);
        andFlags = CalculateLogicalFlags(andResult.Value);
        andWriteReached = true;
        andWriteAddress = freshAddress.Value;
        andWriteValue = andResult.Value;
        writes.Add(new(
            AndEdxPlus4RawFileOffset,
            andWriteAddress.Value,
            andWriteValue.Value));

        orReached = true;
        eaxAfterOr = ApplyOrMask(eaxRetained.Value);
        orFlags = CalculateLogicalFlags(eaxAfterOr.Value);

        if (capture.EntryESI is not uint entryEsi)
        {
            return Finish(
                CapacityFinalCalleeAlternateFieldWritesOutcome.MissingEntryESI,
                EsiPlus4WriteRawFileOffset);
        }

        entryEsiCaptured = true;
        esiPlus4WriteReached = true;
        esiPlus4WriteAddress = DeriveEsiPlus4Address(entryEsi);
        esiPlus4WriteValue = eaxAfterOr.Value;
        writes.Add(new(
            EsiPlus4WriteRawFileOffset,
            esiPlus4WriteAddress.Value,
            esiPlus4WriteValue.Value));

        esiPlusEdiWriteReached = true;
        esiPlusEdiWriteAddress = DeriveEsiPlusEdiAddress(entryEsi, entryEdi);
        esiPlusEdiWriteValue = entryEdi;
        writes.Add(new(
            EsiPlusEdiWriteRawFileOffset,
            esiPlusEdiWriteAddress.Value,
            esiPlusEdiWriteValue.Value));

        jumpReached = true;
        return Finish(
            CapacityFinalCalleeAlternateFieldWritesOutcome.Completed,
            JumpTargetRawFileOffset,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeAlternateFieldWritesFlags CalculateLogicalFlags(
        uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeAlternateFieldWritesInstructionEvidence Instruction(
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
