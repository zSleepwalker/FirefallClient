using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeMaskAndStoreCapture(
    uint? EntryEAX,
    uint? FreshGlobalDword);

public enum CapacityFinalCalleeLargeMaskAndStoreOutcome
{
    MissingBothOperands,
    MissingEntryEAX,
    MissingFreshGlobalDword,
    Completed
}

public enum CapacityFinalCalleeLargeMaskAndStoreEventKind
{
    Read,
    Write
}

public readonly record struct CapacityFinalCalleeLargeMaskAndStoreFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeMaskAndStoreInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct CapacityFinalCalleeLargeMaskAndStoreRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeMaskAndStoreWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public readonly record struct CapacityFinalCalleeLargeMaskAndStoreEvent(
    CapacityFinalCalleeLargeMaskAndStoreEventKind Kind,
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public sealed record CapacityFinalCalleeLargeMaskAndStorePlan(
    CapacityFinalCalleeLargeMaskAndStoreCapture Capture,
    CapacityFinalCalleeLargeMaskAndStoreOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeLargeMaskAndStoreRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeMaskAndStoreRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeMaskAndStoreWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeMaskAndStoreWrite>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeMaskAndStoreEvent> Events { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeMaskAndStoreEvent>.Empty;
    public bool EntryEAXCaptured { get; init; }
    public bool FreshGlobalDwordCaptured { get; init; }
    public bool GlobalReadReached { get; init; }
    public uint? GlobalReadAddress { get; init; }
    public uint? GlobalReadValue { get; init; }
    public bool AndExecuted { get; init; }
    public uint? AndEAX { get; init; }
    public uint? AndFreshGlobalDword { get; init; }
    public uint? AndResult { get; init; }
    public CapacityFinalCalleeLargeMaskAndStoreFlags? AndFlags { get; init; }
    public bool GlobalWriteReached { get; init; }
    public uint? GlobalWriteAddress { get; init; }
    public uint? GlobalWriteValue { get; init; }
    public bool JumpReached { get; init; }
    public uint? JumpTargetRawFileOffset { get; init; }
    public CapacityFinalCalleeLargeMaskAndStoreFlags? FlagsAtStop { get; init; }
    public bool ExactStopAtJumpTarget { get; init; }
    public bool CommonTailEntered { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the large-mask absolute DWORD AND and its following short jump.
/// </summary>
public static class CapacityFinalCalleeLargeMaskAndStoreProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C925B;
    public const uint EntryFlatIdaAddress = 0x012C925B;
    public const uint EntryVirtualAddress = 0x016C9E5B;
    public const uint AndRawFileOffset = 0x012C925B;
    public const uint JumpRawFileOffset = 0x012C9261;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9263;
    public const uint JumpTargetRawFileOffset = 0x012C92BD;
    public const uint GlobalVirtualAddress = 0x01EDADCC;
    public const byte JumpDisplacement = 0x5A;

    public const string CapturedWindowBytes =
        "21 05 CC AD ED 01 EB 5A";

    public static ImmutableArray<CapacityFinalCalleeLargeMaskAndStoreInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("AND the absolute global DWORD with entry EAX",
                AndRawFileOffset, "21 05 CC AD ED 01"),
            Instruction("Jump from the next instruction to raw 92BD",
                JumpRawFileOffset, "EB 5A"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint CalculateJumpTargetRawFileOffset() =>
        unchecked(BoundaryRawFileOffsetExclusive + JumpDisplacement);

    public static CapacityFinalCalleeLargeMaskAndStorePlan Project(
        CapacityFinalCalleeLargeMaskAndStoreCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeMaskAndStoreRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeMaskAndStoreWrite>();
        var events =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeMaskAndStoreEvent>();

        CapacityFinalCalleeLargeMaskAndStorePlan Finish(
            CapacityFinalCalleeLargeMaskAndStoreOutcome outcome,
            bool andExecuted = false,
            uint? andEax = null,
            uint? andFreshGlobalDword = null,
            uint? andResult = null,
            CapacityFinalCalleeLargeMaskAndStoreFlags? andFlags = null,
            bool globalWriteReached = false,
            uint? globalWriteValue = null,
            bool jumpReached = false,
            uint? jumpTarget = null,
            uint stopAt = AndRawFileOffset) =>
            new(capture, outcome)
            {
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                Events = events.ToImmutable(),
                EntryEAXCaptured = capture.EntryEAX.HasValue,
                FreshGlobalDwordCaptured = capture.FreshGlobalDword.HasValue,
                GlobalReadReached = true,
                GlobalReadAddress = GlobalVirtualAddress,
                GlobalReadValue = capture.FreshGlobalDword,
                AndExecuted = andExecuted,
                AndEAX = andEax,
                AndFreshGlobalDword = andFreshGlobalDword,
                AndResult = andResult,
                AndFlags = andFlags,
                GlobalWriteReached = globalWriteReached,
                GlobalWriteAddress = globalWriteReached ? GlobalVirtualAddress : null,
                GlobalWriteValue = globalWriteReached ? globalWriteValue : null,
                JumpReached = jumpReached,
                JumpTargetRawFileOffset = jumpTarget,
                FlagsAtStop = andFlags,
                ExactStopAtJumpTarget = jumpReached
                    && jumpTarget == JumpTargetRawFileOffset,
                CommonTailEntered = false,
                StopAtRawFileOffsetExclusive = stopAt
            };

        bool freshGlobalCaptured = capture.FreshGlobalDword.HasValue;
        reads.Add(new(
            AndRawFileOffset,
            GlobalVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: freshGlobalCaptured,
            Value: capture.FreshGlobalDword));
        events.Add(new(
            CapacityFinalCalleeLargeMaskAndStoreEventKind.Read,
            AndRawFileOffset,
            GlobalVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: freshGlobalCaptured,
            Value: capture.FreshGlobalDword));

        if (!capture.EntryEAX.HasValue && !freshGlobalCaptured)
        {
            return Finish(
                CapacityFinalCalleeLargeMaskAndStoreOutcome.MissingBothOperands);
        }

        if (!capture.EntryEAX.HasValue)
        {
            return Finish(
                CapacityFinalCalleeLargeMaskAndStoreOutcome.MissingEntryEAX);
        }

        if (!freshGlobalCaptured)
        {
            return Finish(
                CapacityFinalCalleeLargeMaskAndStoreOutcome.MissingFreshGlobalDword);
        }

        uint entryEax = capture.EntryEAX!.Value;
        uint freshGlobalDword = capture.FreshGlobalDword!.Value;
        uint result = freshGlobalDword & entryEax;
        CapacityFinalCalleeLargeMaskAndStoreFlags flags = CalculateAndFlags(result);
        writes.Add(new(
            AndRawFileOffset,
            GlobalVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: true,
            Value: result));
        events.Add(new(
            CapacityFinalCalleeLargeMaskAndStoreEventKind.Write,
            AndRawFileOffset,
            GlobalVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: true,
            Value: result));

        uint jumpTarget = CalculateJumpTargetRawFileOffset();
        return Finish(
            CapacityFinalCalleeLargeMaskAndStoreOutcome.Completed,
            andExecuted: true,
            andEax: entryEax,
            andFreshGlobalDword: freshGlobalDword,
            andResult: result,
            andFlags: flags,
            globalWriteReached: true,
            globalWriteValue: result,
            jumpReached: true,
            jumpTarget: jumpTarget,
            stopAt: jumpTarget);
    }

    private static CapacityFinalCalleeLargeMaskAndStoreFlags CalculateAndFlags(
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

    private static CapacityFinalCalleeLargeMaskAndStoreInstructionEvidence Instruction(
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
