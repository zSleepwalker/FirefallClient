using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonMaskOrStoreCapture(
    uint? EntryECX,
    uint? EntryEDI);

public enum CapacityFinalCalleeCommonMaskOrStoreOutcome
{
    MissingEntryECX,
    MissingEntryEDI,
    MissingBothEntryOperands,
    Completed
}

public readonly record struct CapacityFinalCalleeCommonMaskOrStoreFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeCommonMaskOrStoreWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeCommonMaskOrStoreInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public sealed record CapacityFinalCalleeCommonMaskOrStorePlan(
    CapacityFinalCalleeCommonMaskOrStoreCapture Capture,
    CapacityFinalCalleeCommonMaskOrStoreOutcome Outcome)
{
    public bool EntryECXCaptured { get; init; }
    public bool EntryEDICaptured { get; init; }
    public bool OrReached { get; init; }
    public uint? OrECX { get; init; }
    public uint? OrEDI { get; init; }
    public uint? OrResult { get; init; }
    public CapacityFinalCalleeCommonMaskOrStoreFlags? OrFlags { get; init; }
    public bool GlobalReadReached { get; init; }
    public bool GlobalWriteReached { get; init; }
    public uint? GlobalWriteAddress { get; init; }
    public uint? GlobalWriteValue { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonMaskOrStoreWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonMaskOrStoreWrite>.Empty;
    public CapacityFinalCalleeCommonMaskOrStoreFlags? FlagsAtStop { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the bounded OR ECX,EDI and absolute global store at raw 931B..9323.
/// </summary>
public static class CapacityFinalCalleeCommonMaskOrStoreProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C931B;
    public const uint EntryVirtualAddress = 0x016C9F1B;
    public const uint OrRawFileOffset = 0x012C931B;
    public const uint GlobalStoreRawFileOffset = 0x012C931D;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9323;
    public const uint GlobalVirtualAddress = 0x01EDADC8;

    public const string CapturedWindowBytes =
        "0B CF 89 0D C8 AD ED 01";

    public static ImmutableArray<CapacityFinalCalleeCommonMaskOrStoreInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("OR entry ECX with independently captured entry EDI",
                OrRawFileOffset, "0B CF"),
            Instruction("Store ECX to the absolute global virtual address",
                GlobalStoreRawFileOffset, "89 0D C8 AD ED 01"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonMaskOrStorePlan Project(
        CapacityFinalCalleeCommonMaskOrStoreCapture capture)
    {
        bool ecxCaptured = capture.EntryECX.HasValue;
        bool ediCaptured = capture.EntryEDI.HasValue;

        CapacityFinalCalleeCommonMaskOrStorePlan Finish(
            CapacityFinalCalleeCommonMaskOrStoreOutcome outcome,
            bool orReached = false,
            uint? orEcx = null,
            uint? orEdi = null,
            uint? orResult = null,
            CapacityFinalCalleeCommonMaskOrStoreFlags? orFlags = null,
            bool globalWriteReached = false,
            uint? globalWriteAddress = null,
            uint? globalWriteValue = null,
            ImmutableArray<CapacityFinalCalleeCommonMaskOrStoreWrite> writes = default,
            bool exactCleanExitAdmitted = false,
            uint stopAt = EntryRawFileOffset) =>
            new(capture, outcome)
            {
                EntryECXCaptured = ecxCaptured,
                EntryEDICaptured = ediCaptured,
                OrReached = orReached,
                OrECX = orEcx,
                OrEDI = orEdi,
                OrResult = orResult,
                OrFlags = orFlags,
                GlobalReadReached = false,
                GlobalWriteReached = globalWriteReached,
                GlobalWriteAddress = globalWriteAddress,
                GlobalWriteValue = globalWriteValue,
                Writes = writes.IsDefault
                    ? ImmutableArray<CapacityFinalCalleeCommonMaskOrStoreWrite>.Empty
                    : writes,
                FlagsAtStop = orFlags,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (!ecxCaptured && !ediCaptured)
        {
            return Finish(
                CapacityFinalCalleeCommonMaskOrStoreOutcome.MissingBothEntryOperands);
        }

        if (!ecxCaptured)
        {
            return Finish(CapacityFinalCalleeCommonMaskOrStoreOutcome.MissingEntryECX);
        }

        if (!ediCaptured)
        {
            return Finish(CapacityFinalCalleeCommonMaskOrStoreOutcome.MissingEntryEDI);
        }

        uint entryEcx = capture.EntryECX!.Value;
        uint entryEdi = capture.EntryEDI!.Value;
        uint result = entryEcx | entryEdi;
        CapacityFinalCalleeCommonMaskOrStoreFlags flags = CalculateOrFlags(result);
        var write = new CapacityFinalCalleeCommonMaskOrStoreWrite(
            GlobalStoreRawFileOffset,
            GlobalVirtualAddress,
            result);

        return Finish(
            CapacityFinalCalleeCommonMaskOrStoreOutcome.Completed,
            orReached: true,
            orEcx: entryEcx,
            orEdi: entryEdi,
            orResult: result,
            orFlags: flags,
            globalWriteReached: true,
            globalWriteAddress: GlobalVirtualAddress,
            globalWriteValue: result,
            writes: ImmutableArray.Create(write),
            exactCleanExitAdmitted: true,
            stopAt: BoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeCommonMaskOrStoreFlags CalculateOrFlags(
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

    private static CapacityFinalCalleeCommonMaskOrStoreInstructionEvidence Instruction(
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
