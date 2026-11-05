using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeNodeReloadCapture(
    uint? EntryECX,
    uint? FreshNodeDword);

public enum CapacityFinalCalleeLargeNodeReloadOutcome
{
    MissingEntryECX,
    MissingFreshNodeDword,
    Completed
}

public readonly record struct CapacityFinalCalleeLargeNodeReloadInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeLargeNodeReloadRead(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value);

public sealed record CapacityFinalCalleeLargeNodeReloadPlan(
    CapacityFinalCalleeLargeNodeReloadCapture Capture,
    CapacityFinalCalleeLargeNodeReloadOutcome Outcome)
{
    public bool EntryECXCaptured { get; init; }
    public bool FreshNodeReadReached { get; init; }
    public uint? FreshNodeAddress { get; init; }
    public bool FreshNodeCaptured { get; init; }
    public uint? FreshNodeDword { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeNodeReloadRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeNodeReloadRead>.Empty;
    public bool EAXMoveReached { get; init; }
    public bool EAXCaptured { get; init; }
    public uint? EAX { get; init; }
    public bool EDXMoveReached { get; init; }
    public bool EDXCaptured { get; init; }
    public uint? EDX { get; init; }
    public bool JumpReached { get; init; }
    public uint? JumpTargetRawFileOffset { get; init; }
    public uint? JumpTargetVirtualAddress { get; init; }
    public uint? BoundedStopRawFileOffset =>
        JumpReached ? JumpTargetRawFileOffset : null;
    public bool MemoryWriteReached => false;
    public bool IncomingFlagsPreserved { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool UnreachedFreshNodeCaptureProvided { get; init; }
    public uint StopAtRawFileOffsetExclusive =>
        JumpReached ? CapacityFinalCalleeLargeNodeReloadProjection.JumpTargetRawFileOffset
            : CapacityFinalCalleeLargeNodeReloadProjection.EaxReadRawFileOffset;
}

/// <summary>
/// Projects the bounded fresh-node reload, ECX copy, and short jump.
/// </summary>
public static class CapacityFinalCalleeLargeNodeReloadProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9205;
    public const uint EntryVirtualAddress = 0x016C9E05;
    public const uint EaxReadRawFileOffset = 0x012C9205;
    public const uint EdxMoveRawFileOffset = 0x012C9207;
    public const uint JumpRawFileOffset = 0x012C9209;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C920B;
    public const uint JumpTargetRawFileOffset = 0x012C91F3;
    public const uint JumpTargetVirtualAddress = 0x016C9DF3;

    public const string CapturedWindowBytes = "8B 01 8B D1 EB E8";

    public static ImmutableArray<CapacityFinalCalleeLargeNodeReloadInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Load the fresh DWORD at entry ECX into EAX",
                EaxReadRawFileOffset,
                "8B 01"),
            Instruction(
                "Copy the original entry ECX address into EDX",
                EdxMoveRawFileOffset,
                "8B D1"),
            Instruction(
                "Jump to the bounded stop at 91F3",
                JumpRawFileOffset,
                "EB E8",
                JumpTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeLargeNodeReloadPlan Project(
        CapacityFinalCalleeLargeNodeReloadCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeNodeReloadRead>();
        bool entryECXCaptured = false;
        bool freshNodeReadReached = false;
        uint? freshNodeAddress = null;
        bool freshNodeCaptured = false;
        uint? freshNodeDword = null;
        bool eaxMoveReached = false;
        bool eaxCaptured = false;
        uint? eax = null;
        bool edxMoveReached = false;
        bool edxCaptured = false;
        uint? edx = null;
        bool jumpReached = false;

        CapacityFinalCalleeLargeNodeReloadPlan Finish(
            CapacityFinalCalleeLargeNodeReloadOutcome outcome,
            bool exactCleanCompletion = false) =>
            new(capture, outcome)
            {
                EntryECXCaptured = entryECXCaptured,
                FreshNodeReadReached = freshNodeReadReached,
                FreshNodeAddress = freshNodeAddress,
                FreshNodeCaptured = freshNodeCaptured,
                FreshNodeDword = freshNodeDword,
                Reads = reads.ToImmutable(),
                EAXMoveReached = eaxMoveReached,
                EAXCaptured = eaxCaptured,
                EAX = eax,
                EDXMoveReached = edxMoveReached,
                EDXCaptured = edxCaptured,
                EDX = edx,
                JumpReached = jumpReached,
                JumpTargetRawFileOffset = jumpReached
                    ? JumpTargetRawFileOffset
                    : null,
                JumpTargetVirtualAddress = jumpReached
                    ? JumpTargetVirtualAddress
                    : null,
                IncomingFlagsPreserved = eaxMoveReached,
                ExactCleanCompletion = exactCleanCompletion,
                UnreachedFreshNodeCaptureProvided =
                    !freshNodeReadReached && capture.FreshNodeDword.HasValue
            };

        if (capture.EntryECX is not uint entryECX)
        {
            return Finish(CapacityFinalCalleeLargeNodeReloadOutcome.MissingEntryECX);
        }

        entryECXCaptured = true;
        freshNodeReadReached = true;
        freshNodeAddress = entryECX;
        reads.Add(new(
            EaxReadRawFileOffset,
            freshNodeAddress,
            AddressCaptured: true,
            ValueCaptured: capture.FreshNodeDword.HasValue,
            Value: capture.FreshNodeDword));
        if (capture.FreshNodeDword is not uint freshNode)
        {
            return Finish(CapacityFinalCalleeLargeNodeReloadOutcome.MissingFreshNodeDword);
        }

        freshNodeCaptured = true;
        freshNodeDword = freshNode;
        eaxMoveReached = true;
        eaxCaptured = true;
        eax = freshNode;
        edxMoveReached = true;
        edxCaptured = true;
        edx = entryECX;
        jumpReached = true;

        return Finish(
            CapacityFinalCalleeLargeNodeReloadOutcome.Completed,
            exactCleanCompletion: true);
    }

    private static CapacityFinalCalleeLargeNodeReloadInstructionEvidence Instruction(
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
