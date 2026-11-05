using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeBacklinkNextFieldGateCapture(
    uint? EntryEAX,
    uint? EntryECX,
    uint? EntryEDX,
    uint? FreshEDXPlus10);

public enum CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome
{
    MissingEntryEAXForFirstStore,
    MissingEntryECXForFirstStore,
    MissingBothFirstStoreOperands,
    MissingEntryEDXForFreshRead,
    MissingFreshEDXPlus10,
    ZeroTo92A4,
    NonzeroTo9292
}

public readonly record struct CapacityFinalCalleeLargeBacklinkNextFieldGateFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public enum CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperationKind
{
    Write,
    Read
}

public readonly record struct CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperation(
    uint RawFileOffset,
    CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperationKind Kind,
    uint? Address,
    bool AddressCaptured,
    uint? Value,
    bool ValueCaptured,
    bool Committed);

public readonly record struct CapacityFinalCalleeLargeBacklinkNextFieldGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeBacklinkNextFieldGatePlan(
    CapacityFinalCalleeLargeBacklinkNextFieldGateCapture Capture,
    CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome Outcome)
{
    public bool FirstStoreAttempted { get; init; }
    public uint? FirstStoreAddress { get; init; }
    public bool FirstStoreAddressCaptured { get; init; }
    public uint? FirstStoreValue { get; init; }
    public bool FirstStoreValueCaptured { get; init; }
    public bool FirstStoreCommitted { get; init; }
    public bool FreshReadReached { get; init; }
    public uint? FreshReadAddress { get; init; }
    public bool FreshReadAddressCaptured { get; init; }
    public bool FreshReadValueCaptured { get; init; }
    public uint? FreshECX { get; init; }
    public bool TestReached { get; init; }
    public uint? TestResult { get; init; }
    public CapacityFinalCalleeLargeBacklinkNextFieldGateFlags? TestFlags { get; init; }
    public CapacityFinalCalleeLargeBacklinkNextFieldGateFlags? FlagsAtStop => TestFlags;
    public bool JzReached { get; init; }
    public bool? JzTaken { get; init; }
    public uint? JzSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperation>
        MemoryOperations { get; init; } =
            ImmutableArray<CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperation>.Empty;
    public bool UnreachedEntryEDXCaptureProvided { get; init; }
    public bool UnreachedFreshEDXPlus10CaptureProvided { get; init; }
    public bool UnusableFreshEDXPlus10CaptureProvided { get; init; }
    public bool FollowingCMPReached => false;
    public bool TargetReadReached => false;
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the initial backlink store and the independent fresh next-field TEST/JZ gate.
/// </summary>
public static class CapacityFinalCalleeLargeBacklinkNextFieldGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9288;
    public const uint EntryFlatIdaAddress = 0x012C9288;
    public const uint EntryVirtualAddress = 0x016C9E88;
    public const uint FirstStoreRawFileOffset = 0x012C9288;
    public const uint FreshReadRawFileOffset = 0x012C928B;
    public const uint TestRawFileOffset = 0x012C928E;
    public const uint JzRawFileOffset = 0x012C9290;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9292;
    public const uint JzDestinationRawFileOffset = 0x012C92A4;
    public const uint FirstStoreDisplacement = 0x18;
    public const uint FreshReadDisplacement = 0x10;

    public const string CapturedWindowBytes =
        "89 48 18 8B 4A 10 85 C9 74 12";

    public static ImmutableArray<CapacityFinalCalleeLargeBacklinkNextFieldGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Attempt to store entry ECX at entry EAX+18",
                FirstStoreRawFileOffset,
                "89 48 18"),
            Instruction(
                "Load a fresh DWORD from entry EDX+10 into ECX",
                FreshReadRawFileOffset,
                "8B 4A 10"),
            Instruction(
                "TEST the fresh ECX value",
                TestRawFileOffset,
                "85 C9"),
            Instruction(
                "Branch to 92A4 when the fresh ECX value is zero",
                JzRawFileOffset,
                "74 12",
                JzDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFirstStoreAddress(uint entryEAX) =>
        unchecked(entryEAX + FirstStoreDisplacement);

    public static uint DeriveFreshReadAddress(uint entryEDX) =>
        unchecked(entryEDX + FreshReadDisplacement);

    public static CapacityFinalCalleeLargeBacklinkNextFieldGatePlan Project(
        CapacityFinalCalleeLargeBacklinkNextFieldGateCapture capture)
    {
        var reached = ImmutableArray.CreateBuilder<uint>();
        var memoryOperations =
            ImmutableArray.CreateBuilder<
                CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperation>();

        reached.Add(FirstStoreRawFileOffset);
        bool storeAddressCaptured = capture.EntryEAX.HasValue;
        uint? storeAddress = capture.EntryEAX is uint entryEAX
            ? DeriveFirstStoreAddress(entryEAX)
            : null;
        bool storeValueCaptured = capture.EntryECX.HasValue;
        uint? storeValue = capture.EntryECX;
        bool storeCommitted = storeAddressCaptured && storeValueCaptured;
        memoryOperations.Add(new(
            FirstStoreRawFileOffset,
            CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperationKind.Write,
            storeAddress,
            storeAddressCaptured,
            storeValue,
            storeValueCaptured,
            storeCommitted));

        CapacityFinalCalleeLargeBacklinkNextFieldGatePlan Finish(
            CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome outcome,
            uint stopAt,
            bool exactCleanCompletion = false,
            bool freshReadReached = false,
            uint? freshReadAddress = null,
            bool freshReadAddressCaptured = false,
            bool freshReadValueCaptured = false,
            uint? freshECX = null,
            bool testReached = false,
            uint? testResult = null,
            CapacityFinalCalleeLargeBacklinkNextFieldGateFlags? testFlags = null,
            bool jzReached = false,
            bool? jzTaken = null,
            uint? selectedDestination = null) =>
            new(capture, outcome)
            {
                FirstStoreAttempted = true,
                FirstStoreAddress = storeAddress,
                FirstStoreAddressCaptured = storeAddressCaptured,
                FirstStoreValue = storeValue,
                FirstStoreValueCaptured = storeValueCaptured,
                FirstStoreCommitted = storeCommitted,
                FreshReadReached = freshReadReached,
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
                MemoryOperations = memoryOperations.ToImmutable(),
                UnreachedEntryEDXCaptureProvided =
                    !freshReadReached && capture.EntryEDX.HasValue,
                UnreachedFreshEDXPlus10CaptureProvided =
                    !freshReadReached && capture.FreshEDXPlus10.HasValue,
                UnusableFreshEDXPlus10CaptureProvided =
                    freshReadReached
                    && !freshReadAddressCaptured
                    && capture.FreshEDXPlus10.HasValue,
                ExactCleanCompletion = exactCleanCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (!storeCommitted)
        {
            CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome outcome =
                !storeAddressCaptured && !storeValueCaptured
                    ? CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome
                        .MissingBothFirstStoreOperands
                    : !storeAddressCaptured
                        ? CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome
                            .MissingEntryEAXForFirstStore
                        : CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome
                            .MissingEntryECXForFirstStore;
            return Finish(outcome, FirstStoreRawFileOffset);
        }

        reached.Add(FreshReadRawFileOffset);
        bool freshReadAddressCaptured = capture.EntryEDX.HasValue;
        uint? freshReadAddress = capture.EntryEDX is uint entryEDX
            ? DeriveFreshReadAddress(entryEDX)
            : null;
        bool freshReadValueCaptured =
            freshReadAddressCaptured && capture.FreshEDXPlus10.HasValue;
        uint? freshECX = freshReadValueCaptured
            ? capture.FreshEDXPlus10
            : null;
        memoryOperations.Add(new(
            FreshReadRawFileOffset,
            CapacityFinalCalleeLargeBacklinkNextFieldGateMemoryOperationKind.Read,
            freshReadAddress,
            freshReadAddressCaptured,
            freshECX,
            freshReadValueCaptured,
            Committed: false));

        if (!freshReadAddressCaptured)
        {
            return Finish(
                CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome
                    .MissingEntryEDXForFreshRead,
                FreshReadRawFileOffset,
                freshReadReached: true,
                freshReadAddress: null,
                freshReadAddressCaptured: false);
        }

        if (!freshReadValueCaptured)
        {
            return Finish(
                CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome
                    .MissingFreshEDXPlus10,
                FreshReadRawFileOffset,
                freshReadReached: true,
                freshReadAddress: freshReadAddress,
                freshReadAddressCaptured: true);
        }

        reached.Add(TestRawFileOffset);
        uint value = freshECX!.Value;
        CapacityFinalCalleeLargeBacklinkNextFieldGateFlags flags = TestFlagsFor(value);
        reached.Add(JzRawFileOffset);
        bool jzTaken = flags.Zero;
        uint destination = jzTaken
            ? JzDestinationRawFileOffset
            : BoundaryRawFileOffsetExclusive;

        return Finish(
            jzTaken
                ? CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.ZeroTo92A4
                : CapacityFinalCalleeLargeBacklinkNextFieldGateOutcome.NonzeroTo9292,
            destination,
            exactCleanCompletion: true,
            freshReadReached: true,
            freshReadAddress: freshReadAddress,
            freshReadAddressCaptured: true,
            freshReadValueCaptured: true,
            freshECX: freshECX,
            testReached: true,
            testResult: value,
            testFlags: flags,
            jzReached: true,
            jzTaken: jzTaken,
            selectedDestination: destination);
    }

    private static CapacityFinalCalleeLargeBacklinkNextFieldGateFlags TestFlagsFor(
        uint value) =>
        new(
            Carry: false,
            Parity: (BitOperations.PopCount((uint)(byte)value) & 1) == 0,
            AuxiliaryCarry: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static CapacityFinalCalleeLargeBacklinkNextFieldGateInstructionEvidence
        Instruction(
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
