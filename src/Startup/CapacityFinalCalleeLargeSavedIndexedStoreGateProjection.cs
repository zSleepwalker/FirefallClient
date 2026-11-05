using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSavedIndexedStoreGateCapture(
    uint? EntryEDX,
    uint? FreshEDXPlus1C,
    uint? EntryEAX);

public enum CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome
{
    MissingEntryEDXForFreshRead,
    MissingFreshEDXPlus1C,
    MissingEntryEAXForStore,
    ZeroToFallthrough9251,
    NonzeroTo927C
}

public readonly record struct CapacityFinalCalleeLargeSavedIndexedStoreGateFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeSavedIndexedStoreGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeLargeSavedIndexedStoreGateRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeSavedIndexedStoreGateWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public sealed record CapacityFinalCalleeLargeSavedIndexedStoreGatePlan(
    CapacityFinalCalleeLargeSavedIndexedStoreGateCapture Capture,
    CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome Outcome)
{
    public bool EntryEDXCaptured { get; init; }
    public bool FreshIndexReadReached { get; init; }
    public uint? FreshIndexReadAddress { get; init; }
    public bool FreshEDXPlus1CCaptured { get; init; }
    public uint? FreshIndex { get; init; }
    public bool StoreInstructionReached { get; init; }
    public uint? StoreAddress { get; init; }
    public bool StoreValueCaptured { get; init; }
    public uint? StoreValue { get; init; }
    public bool TestReached { get; init; }
    public uint? TestResult { get; init; }
    public CapacityFinalCalleeLargeSavedIndexedStoreGateFlags? TestFlags { get; init; }
    public CapacityFinalCalleeLargeSavedIndexedStoreGateFlags? FlagsAtStop => TestFlags;
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? JnzSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedIndexedStoreGateRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeSavedIndexedStoreGateRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedIndexedStoreGateWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeSavedIndexedStoreGateWrite>.Empty;
    public bool UnreachedFreshEDXPlus1CCaptureProvided { get; init; }
    public bool UnreachedEntryEAXCaptureProvided { get; init; }
    public bool DestinationEntered => false;
    public bool FollowingZeroPathReached => false;
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the fresh indexed DWORD store and its EAX TEST/JNZ gate only.
/// </summary>
public static class CapacityFinalCalleeLargeSavedIndexedStoreGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9243;
    public const uint EntryFlatIdaAddress = 0x012C9243;
    public const uint EntryVirtualAddress = 0x016C9E43;
    public const uint FreshIndexReadRawFileOffset = 0x012C9243;
    public const uint StoreRawFileOffset = 0x012C9246;
    public const uint TestRawFileOffset = 0x012C924D;
    public const uint JnzRawFileOffset = 0x012C924F;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9251;
    public const uint JnzDestinationRawFileOffset = 0x012C927C;

    public const uint FreshEDXPlus1CDisplacement = 0x1C;
    public const uint IndexedGlobalBaseVirtualAddress = 0x01EDAEF8;

    public const string CapturedWindowBytes =
        "8B 5A 1C 89 04 9D F8 AE ED 01 85 C0 75 2B";

    public static ImmutableArray<CapacityFinalCalleeLargeSavedIndexedStoreGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Load a fresh DWORD from entry EDX+1C into EBX",
                FreshIndexReadRawFileOffset,
                "8B 5A 1C"),
            Instruction(
                "Store entry EAX at the DWORD address indexed by fresh EBX",
                StoreRawFileOffset,
                "89 04 9D F8 AE ED 01"),
            Instruction(
                "Test entry EAX against itself",
                TestRawFileOffset,
                "85 C0"),
            Instruction(
                "Jump to 927C when TEST found entry EAX nonzero",
                JnzRawFileOffset,
                "75 2B",
                JnzDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshEDXPlus1CAddress(uint entryEDX) =>
        unchecked(entryEDX + FreshEDXPlus1CDisplacement);

    public static uint DeriveFreshIndexedStoreAddress(uint freshIndex) =>
        unchecked(freshIndex * 4 + IndexedGlobalBaseVirtualAddress);

    public static CapacityFinalCalleeLargeSavedIndexedStoreGatePlan Project(
        CapacityFinalCalleeLargeSavedIndexedStoreGateCapture capture)
    {
        var reached = ImmutableArray.CreateBuilder<uint>();
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeSavedIndexedStoreGateRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeSavedIndexedStoreGateWrite>();
        bool freshIndexReadReached = false;
        uint? freshIndexReadAddress = null;
        bool freshIndexCaptured = false;
        uint? freshIndex = null;
        bool storeInstructionReached = false;
        uint? storeAddress = null;
        bool storeValueCaptured = false;
        uint? storeValue = null;
        bool testReached = false;
        uint? testResult = null;
        CapacityFinalCalleeLargeSavedIndexedStoreGateFlags? testFlags = null;
        bool jnzReached = false;
        bool? jnzTaken = null;
        uint? jnzDestination = null;

        CapacityFinalCalleeLargeSavedIndexedStoreGatePlan Finish(
            CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome outcome,
            uint stopAt,
            bool exactCleanCompletion = false) =>
            new(capture, outcome)
            {
                EntryEDXCaptured = capture.EntryEDX.HasValue,
                FreshIndexReadReached = freshIndexReadReached,
                FreshIndexReadAddress = freshIndexReadAddress,
                FreshEDXPlus1CCaptured = freshIndexCaptured,
                FreshIndex = freshIndex,
                StoreInstructionReached = storeInstructionReached,
                StoreAddress = storeAddress,
                StoreValueCaptured = storeValueCaptured,
                StoreValue = storeValue,
                TestReached = testReached,
                TestResult = testResult,
                TestFlags = testFlags,
                JnzReached = jnzReached,
                JnzTaken = jnzTaken,
                JnzSelectedDestinationRawFileOffset = jnzDestination,
                ReachedInstructions = reached.ToImmutable(),
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                UnreachedFreshEDXPlus1CCaptureProvided =
                    !freshIndexCaptured && capture.FreshEDXPlus1C.HasValue,
                UnreachedEntryEAXCaptureProvided =
                    !storeInstructionReached && capture.EntryEAX.HasValue,
                ExactCleanCompletion = exactCleanCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        freshIndexReadReached = true;
        reached.Add(FreshIndexReadRawFileOffset);
        if (capture.EntryEDX is not uint entryEDX)
        {
            reads.Add(new(
                FreshIndexReadRawFileOffset,
                Address: null,
                AddressCaptured: false,
                ValueCaptured: false,
                Value: null));
            return Finish(
                CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome
                    .MissingEntryEDXForFreshRead,
                FreshIndexReadRawFileOffset);
        }

        freshIndexReadAddress = DeriveFreshEDXPlus1CAddress(entryEDX);
        reads.Add(new(
            FreshIndexReadRawFileOffset,
            freshIndexReadAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshEDXPlus1C.HasValue,
            Value: capture.FreshEDXPlus1C));
        if (capture.FreshEDXPlus1C is not uint capturedFreshIndex)
        {
            return Finish(
                CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome
                    .MissingFreshEDXPlus1C,
                FreshIndexReadRawFileOffset);
        }

        freshIndexCaptured = true;
        freshIndex = capturedFreshIndex;

        storeInstructionReached = true;
        reached.Add(StoreRawFileOffset);
        storeAddress = DeriveFreshIndexedStoreAddress(capturedFreshIndex);
        storeValueCaptured = capture.EntryEAX.HasValue;
        storeValue = capture.EntryEAX;
        writes.Add(new(
            StoreRawFileOffset,
            storeAddress,
            AddressCaptured: true,
            ValueCaptured: storeValueCaptured,
            Value: storeValue));
        if (capture.EntryEAX is not uint entryEAX)
        {
            return Finish(
                CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome
                    .MissingEntryEAXForStore,
                StoreRawFileOffset);
        }

        testReached = true;
        reached.Add(TestRawFileOffset);
        testResult = entryEAX;
        testFlags = TestFlagsFor(entryEAX);

        jnzReached = true;
        reached.Add(JnzRawFileOffset);
        jnzTaken = !testFlags.Value.Zero;
        jnzDestination = jnzTaken.Value
            ? JnzDestinationRawFileOffset
            : BoundaryRawFileOffsetExclusive;

        return Finish(
            jnzTaken.Value
                ? CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome.NonzeroTo927C
                : CapacityFinalCalleeLargeSavedIndexedStoreGateOutcome.ZeroToFallthrough9251,
            jnzDestination.Value,
            exactCleanCompletion: true);
    }

    private static CapacityFinalCalleeLargeSavedIndexedStoreGateFlags TestFlagsFor(
        uint value) =>
        new(
            Carry: false,
            Parity: (BitOperations.PopCount((uint)(byte)value) & 1) == 0,
            AuxiliaryCarry: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static CapacityFinalCalleeLargeSavedIndexedStoreGateInstructionEvidence Instruction(
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
