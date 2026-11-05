using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSavedIndexGateCapture(
    uint? EntryEDX,
    uint? FreshEDXPlus1C,
    uint? FreshIndexedDword,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeLargeSavedIndexGateOutcome
{
    MissingEntryEDXForFreshRead,
    MissingFreshEDXPlus1C,
    MissingFreshIndexedDword,
    MissingFreshGlobalValue,
    JnzTo9263,
    EqualToFallthrough9243
}

public readonly record struct CapacityFinalCalleeLargeSavedIndexGateFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeSavedIndexGateRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeSavedIndexGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeSavedIndexGatePlan(
    CapacityFinalCalleeLargeSavedIndexGateCapture Capture,
    CapacityFinalCalleeLargeSavedIndexGateOutcome Outcome)
{
    public bool EntryEDXCaptured { get; init; }
    public bool FreshEDXPlus1CReadReached { get; init; }
    public uint? FreshEDXPlus1CAddress { get; init; }
    public bool FreshEDXPlus1CCaptured { get; init; }
    public uint? EBXAfterFreshEDXPlus1C { get; init; }
    public bool FreshIndexedReadReached { get; init; }
    public uint? FreshIndexedAddress { get; init; }
    public bool FreshIndexedDwordCaptured { get; init; }
    public uint? FreshIndexedDword { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareLeft { get; init; }
    public uint? CompareRight { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeLargeSavedIndexGateFlags? CompareFlags { get; init; }
    public CapacityFinalCalleeLargeSavedIndexGateFlags? FlagsAtStop => CompareFlags;
    public bool FreshGlobalReadReached { get; init; }
    public uint? FreshGlobalAddress { get; init; }
    public bool FreshGlobalValueCaptured { get; init; }
    public uint? EBXAfterFreshGlobalReload { get; init; }
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? JnzSelectedDestinationRawFileOffset { get; init; }
    public ImmutableArray<uint> ReachedInstructions { get; init; } =
        ImmutableArray<uint>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSavedIndexGateRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeSavedIndexGateRead>.Empty;
    public bool UnreachedFreshIndexedDwordCaptureProvided { get; init; }
    public bool UnreachedFreshGlobalValueCaptureProvided { get; init; }
    public bool IncomingFlagsPreservedAfterCompare { get; init; }
    public bool MemoryWriteReached => false;
    public bool ExactCleanCompletion { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the saved-index CMP, independent absolute EBX reload, and JNZ gate.
/// </summary>
public static class CapacityFinalCalleeLargeSavedIndexGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9231;
    public const uint EntryVirtualAddress = 0x016C9E31;
    public const uint FreshEDXPlus1CReadRawFileOffset = 0x012C9231;
    public const uint CompareRawFileOffset = 0x012C9234;
    public const uint FreshGlobalReadRawFileOffset = 0x012C923B;
    public const uint JnzRawFileOffset = 0x012C9241;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9243;
    public const uint JnzDestinationRawFileOffset = 0x012C9263;

    public const uint FreshEDXPlus1CDisplacement = 0x1C;
    public const uint IndexedGlobalBaseVirtualAddress = 0x01EDAEF8;
    public const uint FreshGlobalValueVirtualAddress = 0x01EDADD8;

    public const string CapturedWindowBytes =
        "8B 5A 1C 3B 14 9D F8 AE ED 01 8B 1D D8 AD ED 01 75 20";

    public static ImmutableArray<CapacityFinalCalleeLargeSavedIndexGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Load a fresh DWORD from entry EDX+1C into EBX",
                FreshEDXPlus1CReadRawFileOffset,
                "8B 5A 1C"),
            Instruction(
                "Compare entry EDX with the DWORD indexed by fresh EBX",
                CompareRawFileOffset,
                "3B 14 9D F8 AE ED 01"),
            Instruction(
                "Independently reload EBX from the absolute global address",
                FreshGlobalReadRawFileOffset,
                "8B 1D D8 AD ED 01"),
            Instruction(
                "Jump to 9263 when the earlier CMP was unequal",
                JnzRawFileOffset,
                "75 20",
                JnzDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshEDXPlus1CAddress(uint entryEDX) =>
        unchecked(entryEDX + FreshEDXPlus1CDisplacement);

    public static uint DeriveFreshIndexedAddress(uint freshEDXPlus1C) =>
        unchecked(freshEDXPlus1C * 4 + IndexedGlobalBaseVirtualAddress);

    public static CapacityFinalCalleeLargeSavedIndexGatePlan Project(
        CapacityFinalCalleeLargeSavedIndexGateCapture capture)
    {
        var reached = ImmutableArray.CreateBuilder<uint>();
        var reads = ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeSavedIndexGateRead>();
        bool entryEDXCaptured = false;
        bool freshEDXPlus1CReadReached = false;
        uint? freshEDXPlus1CAddress = null;
        bool freshEDXPlus1CCaptured = false;
        uint? ebxAfterFreshEDXPlus1C = null;
        bool freshIndexedReadReached = false;
        uint? freshIndexedAddress = null;
        bool freshIndexedDwordCaptured = false;
        uint? freshIndexedDword = null;
        bool compareReached = false;
        uint? compareLeft = null;
        uint? compareRight = null;
        uint? compareResult = null;
        CapacityFinalCalleeLargeSavedIndexGateFlags? compareFlags = null;
        bool freshGlobalReadReached = false;
        bool freshGlobalValueCaptured = false;
        uint? ebxAfterFreshGlobalReload = null;
        bool jnzReached = false;
        bool? jnzTaken = null;
        uint? jnzDestination = null;

        CapacityFinalCalleeLargeSavedIndexGatePlan Finish(
            CapacityFinalCalleeLargeSavedIndexGateOutcome outcome,
            uint stopAt,
            bool exactCleanCompletion = false) =>
            new(capture, outcome)
            {
                EntryEDXCaptured = entryEDXCaptured,
                FreshEDXPlus1CReadReached = freshEDXPlus1CReadReached,
                FreshEDXPlus1CAddress = freshEDXPlus1CAddress,
                FreshEDXPlus1CCaptured = freshEDXPlus1CCaptured,
                EBXAfterFreshEDXPlus1C = ebxAfterFreshEDXPlus1C,
                FreshIndexedReadReached = freshIndexedReadReached,
                FreshIndexedAddress = freshIndexedAddress,
                FreshIndexedDwordCaptured = freshIndexedDwordCaptured,
                FreshIndexedDword = freshIndexedDword,
                CompareReached = compareReached,
                CompareLeft = compareLeft,
                CompareRight = compareRight,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                FreshGlobalReadReached = freshGlobalReadReached,
                FreshGlobalAddress = freshGlobalReadReached
                    ? FreshGlobalValueVirtualAddress
                    : null,
                FreshGlobalValueCaptured = freshGlobalValueCaptured,
                EBXAfterFreshGlobalReload = ebxAfterFreshGlobalReload,
                JnzReached = jnzReached,
                JnzTaken = jnzTaken,
                JnzSelectedDestinationRawFileOffset = jnzDestination,
                ReachedInstructions = reached.ToImmutable(),
                Reads = reads.ToImmutable(),
                UnreachedFreshIndexedDwordCaptureProvided =
                    !freshIndexedReadReached && capture.FreshIndexedDword.HasValue,
                UnreachedFreshGlobalValueCaptureProvided =
                    !freshGlobalReadReached && capture.FreshGlobalValue.HasValue,
                IncomingFlagsPreservedAfterCompare = compareReached,
                ExactCleanCompletion = exactCleanCompletion,
                StopAtRawFileOffsetExclusive = stopAt
            };

        freshEDXPlus1CReadReached = true;
        reached.Add(FreshEDXPlus1CReadRawFileOffset);
        if (capture.EntryEDX is not uint entryEDX)
        {
            reads.Add(new(
                FreshEDXPlus1CReadRawFileOffset,
                Address: null,
                AddressCaptured: false,
                ValueCaptured: false,
                Value: null));
            return Finish(
                CapacityFinalCalleeLargeSavedIndexGateOutcome.MissingEntryEDXForFreshRead,
                FreshEDXPlus1CReadRawFileOffset);
        }

        entryEDXCaptured = true;
        freshEDXPlus1CAddress = DeriveFreshEDXPlus1CAddress(entryEDX);
        reads.Add(new(
            FreshEDXPlus1CReadRawFileOffset,
            freshEDXPlus1CAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshEDXPlus1C.HasValue,
            Value: capture.FreshEDXPlus1C));
        if (capture.FreshEDXPlus1C is not uint freshEDXPlus1C)
        {
            return Finish(
                CapacityFinalCalleeLargeSavedIndexGateOutcome.MissingFreshEDXPlus1C,
                FreshEDXPlus1CReadRawFileOffset);
        }

        freshEDXPlus1CCaptured = true;
        ebxAfterFreshEDXPlus1C = freshEDXPlus1C;

        freshIndexedReadReached = true;
        reached.Add(CompareRawFileOffset);
        freshIndexedAddress = DeriveFreshIndexedAddress(freshEDXPlus1C);
        reads.Add(new(
            CompareRawFileOffset,
            freshIndexedAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshIndexedDword.HasValue,
            Value: capture.FreshIndexedDword));
        if (capture.FreshIndexedDword is not uint indexedDword)
        {
            return Finish(
                CapacityFinalCalleeLargeSavedIndexGateOutcome.MissingFreshIndexedDword,
                CompareRawFileOffset);
        }

        freshIndexedDwordCaptured = true;
        freshIndexedDword = indexedDword;
        compareReached = true;
        compareLeft = entryEDX;
        compareRight = indexedDword;
        compareResult = unchecked(entryEDX - indexedDword);
        compareFlags = SubtractFlags(entryEDX, indexedDword, compareResult.Value);

        freshGlobalReadReached = true;
        reached.Add(FreshGlobalReadRawFileOffset);
        reads.Add(new(
            FreshGlobalReadRawFileOffset,
            FreshGlobalValueVirtualAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshGlobalValue.HasValue,
            Value: capture.FreshGlobalValue));
        if (capture.FreshGlobalValue is not uint freshGlobalValue)
        {
            return Finish(
                CapacityFinalCalleeLargeSavedIndexGateOutcome.MissingFreshGlobalValue,
                FreshGlobalReadRawFileOffset);
        }

        freshGlobalValueCaptured = true;
        ebxAfterFreshGlobalReload = freshGlobalValue;

        jnzReached = true;
        reached.Add(JnzRawFileOffset);
        jnzTaken = !compareFlags.Value.Zero;
        jnzDestination = jnzTaken.Value
            ? JnzDestinationRawFileOffset
            : BoundaryRawFileOffsetExclusive;

        return Finish(
            jnzTaken.Value
                ? CapacityFinalCalleeLargeSavedIndexGateOutcome.JnzTo9263
                : CapacityFinalCalleeLargeSavedIndexGateOutcome.EqualToFallthrough9243,
            jnzDestination.Value,
            exactCleanCompletion: true);
    }

    private static CapacityFinalCalleeLargeSavedIndexGateFlags SubtractFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: (BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static CapacityFinalCalleeLargeSavedIndexGateInstructionEvidence Instruction(
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
