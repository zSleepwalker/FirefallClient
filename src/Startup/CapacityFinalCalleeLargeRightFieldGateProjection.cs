using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeRightFieldGateCapture(
    uint? EntryEDX,
    uint? FreshEDXPlus14);

public enum CapacityFinalCalleeLargeRightFieldGateOutcome
{
    MissingEntryEDXForFreshRead,
    MissingFreshEDXPlus14,
    ZeroTo92BD,
    NonzeroTo92AB
}

public readonly record struct CapacityFinalCalleeLargeRightFieldGateFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeRightFieldGateReadAttempt(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    uint? Value,
    bool ValueCaptured,
    bool Attempted);

public readonly record struct CapacityFinalCalleeLargeRightFieldGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeRightFieldGatePlan(
    CapacityFinalCalleeLargeRightFieldGateCapture Capture,
    CapacityFinalCalleeLargeRightFieldGateOutcome Outcome)
{
    public bool FreshReadInstructionReached { get; init; }
    public bool FreshReadAttempted { get; init; }
    public uint? FreshReadAddress { get; init; }
    public bool FreshReadAddressCaptured { get; init; }
    public bool FreshReadValueCaptured { get; init; }
    public uint? FreshECX { get; init; }
    public bool TestReached { get; init; }
    public uint? TestResult { get; init; }
    public CapacityFinalCalleeLargeRightFieldGateFlags? TestFlags { get; init; }
    public CapacityFinalCalleeLargeRightFieldGateFlags? FlagsAtStop => TestFlags;
    public bool JzReached { get; init; }
    public bool? JzTaken { get; init; }
    public uint? JzSelectedDestinationRawFileOffset { get; init; }
    public bool JzTargetEntered => false;
    public bool JzFallthroughEntered => false;
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeRightFieldGateReadAttempt> ReadAttempts
        { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeRightFieldGateReadAttempt>.Empty;
    public bool UnreachedFreshEDXPlus14CaptureProvided { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the independent fresh right-field read and its bounded TEST/JZ gate.
/// </summary>
public static class CapacityFinalCalleeLargeRightFieldGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C92A4;
    public const uint EntryFlatIdaAddress = 0x012C92A4;
    public const uint EntryVirtualAddress = 0x016C9EA4;
    public const uint FreshReadRawFileOffset = 0x012C92A4;
    public const uint TestRawFileOffset = 0x012C92A7;
    public const uint JzRawFileOffset = 0x012C92A9;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C92AB;
    public const uint JzTargetRawFileOffset = 0x012C92BD;
    public const uint FreshReadDisplacement = 0x14;

    public const string CapturedWindowBytes = "8B 4A 14 85 C9 74 12";

    public static ImmutableArray<CapacityFinalCalleeLargeRightFieldGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Load a fresh DWORD from entry EDX+14 into ECX",
                FreshReadRawFileOffset,
                "8B 4A 14"),
            Instruction(
                "TEST the fresh ECX value",
                TestRawFileOffset,
                "85 C9"),
            Instruction(
                "Branch to 92BD when the fresh ECX value is zero",
                JzRawFileOffset,
                "74 12",
                JzTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshReadAddress(uint entryEDX) =>
        unchecked(entryEDX + FreshReadDisplacement);

    public static CapacityFinalCalleeLargeRightFieldGatePlan Project(
        CapacityFinalCalleeLargeRightFieldGateCapture capture)
    {
        var reached = ImmutableArray.CreateBuilder<uint>();
        var readAttempts =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeRightFieldGateReadAttempt>();

        reached.Add(FreshReadRawFileOffset);
        bool freshReadAddressCaptured = capture.EntryEDX.HasValue;
        uint? freshReadAddress = capture.EntryEDX is uint entryEDX
            ? DeriveFreshReadAddress(entryEDX)
            : null;

        CapacityFinalCalleeLargeRightFieldGatePlan Finish(
            CapacityFinalCalleeLargeRightFieldGateOutcome outcome,
            uint stopAt,
            bool freshReadAttempted = false,
            bool freshReadValueCaptured = false,
            uint? freshECX = null,
            bool testReached = false,
            uint? testResult = null,
            CapacityFinalCalleeLargeRightFieldGateFlags? testFlags = null,
            bool jzReached = false,
            bool? jzTaken = null,
            uint? selectedDestination = null,
            bool exactCleanCompletion = false) =>
            new(capture, outcome)
            {
                FreshReadInstructionReached = true,
                FreshReadAttempted = freshReadAttempted,
                FreshReadAddress = freshReadAddress,
                FreshReadAddressCaptured = freshReadAddressCaptured,
                FreshReadValueCaptured = freshReadValueCaptured,
                FreshECX = freshECX,
                TestReached = testReached,
                TestResult = testResult,
                TestFlags = testFlags,
                JzReached = jzReached,
                JzTaken = jzTaken,
                JzSelectedDestinationRawFileOffset = selectedDestination,
                ReachedInstructions = reached.ToImmutable(),
                ReadAttempts = readAttempts.ToImmutable(),
                UnreachedFreshEDXPlus14CaptureProvided =
                    !freshReadAddressCaptured && capture.FreshEDXPlus14.HasValue,
                ExactCleanCompletion = exactCleanCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (!freshReadAddressCaptured)
        {
            return Finish(
                CapacityFinalCalleeLargeRightFieldGateOutcome.MissingEntryEDXForFreshRead,
                FreshReadRawFileOffset);
        }

        bool freshReadValueCaptured = capture.FreshEDXPlus14.HasValue;
        uint? freshECX = freshReadValueCaptured ? capture.FreshEDXPlus14 : null;
        readAttempts.Add(new(
            FreshReadRawFileOffset,
            freshReadAddress,
            AddressCaptured: true,
            freshECX,
            freshReadValueCaptured,
            Attempted: true));

        if (!freshReadValueCaptured)
        {
            return Finish(
                CapacityFinalCalleeLargeRightFieldGateOutcome.MissingFreshEDXPlus14,
                FreshReadRawFileOffset,
                freshReadAttempted: true);
        }

        reached.Add(TestRawFileOffset);
        uint value = freshECX!.Value;
        CapacityFinalCalleeLargeRightFieldGateFlags flags = TestFlagsFor(value);
        reached.Add(JzRawFileOffset);
        bool jzTaken = flags.Zero;
        uint destination = jzTaken ? JzTargetRawFileOffset : BoundaryRawFileOffsetExclusive;

        return Finish(
            jzTaken
                ? CapacityFinalCalleeLargeRightFieldGateOutcome.ZeroTo92BD
                : CapacityFinalCalleeLargeRightFieldGateOutcome.NonzeroTo92AB,
            destination,
            freshReadAttempted: true,
            freshReadValueCaptured: true,
            freshECX: freshECX,
            testReached: true,
            testResult: value,
            testFlags: flags,
            jzReached: true,
            jzTaken: jzTaken,
            selectedDestination: destination,
            exactCleanCompletion: true);
    }

    private static CapacityFinalCalleeLargeRightFieldGateFlags TestFlagsFor(uint value) =>
        new(
            Carry: false,
            Parity: (BitOperations.PopCount((uint)(byte)value) & 1) == 0,
            AuxiliaryCarry: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static CapacityFinalCalleeLargeRightFieldGateInstructionEvidence Instruction(
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
