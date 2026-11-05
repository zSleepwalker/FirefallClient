using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeLargeSuccessorWritesCapture(
    uint? EntryEAX,
    uint? FreshEaxPlus8,
    uint? EntryEDX,
    uint? LateEntryEBX);

public enum CapacityFinalCalleeLargeSuccessorWritesOutcome
{
    MissingEntryEAX,
    MissingFreshField,
    MissingEntryEDX,
    MissingFreshFieldAndEntryEDX,
    UnequalTo8E76,
    MissingLateEntryEBXForEqualStores,
    EqualTo9229
}

public readonly record struct CapacityFinalCalleeLargeSuccessorWritesCompareFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeLargeSuccessorWritesRead(
    uint RawFileOffset,
    uint Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeLargeSuccessorWritesWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeLargeSuccessorWritesInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeLargeSuccessorWritesPlan(
    CapacityFinalCalleeLargeSuccessorWritesCapture Capture,
    CapacityFinalCalleeLargeSuccessorWritesOutcome Outcome)
{
    public bool EntryEAXCaptured { get; init; }
    public bool FreshFieldReadReached { get; init; }
    public uint? FreshFieldAddress { get; init; }
    public bool FreshFieldCaptured { get; init; }
    public uint? FreshFieldValue { get; init; }
    public bool EntryEDXCaptured { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeLargeSuccessorWritesCompareFlags? CompareFlags { get; init; }
    public CapacityFinalCalleeLargeSuccessorWritesCompareFlags? FlagsAtStop => CompareFlags;
    public bool JnzReached { get; init; }
    public bool? JnzTaken { get; init; }
    public uint? JnzSelectedDestinationRawFileOffset { get; init; }
    public bool FirstStoreReached { get; init; }
    public uint? FirstStoreAddress { get; init; }
    public uint? FirstStoreValue { get; init; }
    public bool SecondStoreReached { get; init; }
    public uint? SecondStoreAddress { get; init; }
    public uint? SecondStoreValue { get; init; }
    public bool JumpReached { get; init; }
    public uint? JumpTargetRawFileOffset { get; init; }
    public uint? JumpTargetVirtualAddress { get; init; }
    public bool DestinationEntered { get; init; }
    public bool FlagsPreservedAcrossStores { get; init; }
    public bool UnreachedFreshFieldCaptureProvided { get; init; }
    public bool UnreachedEntryEDXCaptureProvided { get; init; }
    public bool UnreachedLateEntryEBXCaptureProvided { get; init; }
    public bool UnusedLateEntryEBXCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public ImmutableArray<CapacityFinalCalleeLargeSuccessorWritesRead> Reads { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeSuccessorWritesRead>.Empty;
    public ImmutableArray<CapacityFinalCalleeLargeSuccessorWritesWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeLargeSuccessorWritesWrite>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the fresh-field comparison, its unequal branch, and the two
/// ordered stores on the equal path. It does not read or modify process memory.
/// </summary>
public static class CapacityFinalCalleeLargeSuccessorWritesProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C91C5;
    public const uint EntryVirtualAddress = 0x016C9DC5;
    public const uint CompareRawFileOffset = 0x012C91C5;
    public const uint JnzRawFileOffset = 0x012C91C8;
    public const uint FirstStoreRawFileOffset = 0x012C91CE;
    public const uint SecondStoreRawFileOffset = 0x012C91D1;
    public const uint JumpRawFileOffset = 0x012C91D4;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C91D6;

    public const uint UnequalDestinationRawFileOffset = 0x012C8E76;
    public const uint EqualDestinationRawFileOffset = 0x012C9229;
    public const uint EqualDestinationVirtualAddress = 0x016C9E29;
    public const uint FreshFieldDisplacement = 0x08;
    public const uint FirstStoreDisplacement = 0x0C;
    public const uint SecondStoreDisplacement = 0x08;

    public const string CapturedWindowBytes =
        "39 50 08 0F 85 A8 FC FF FF 89 43 0C 89 58 08 EB 53";

    public static ImmutableArray<CapacityFinalCalleeLargeSuccessorWritesInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare fresh [EAX+8] with entry EDX",
                CompareRawFileOffset, "39 50 08"),
            Instruction("Jump to 8E76 when the comparison is unequal",
                JnzRawFileOffset, "0F 85 A8 FC FF FF",
                branchTargetRawFileOffset: UnequalDestinationRawFileOffset),
            Instruction("Store entry EAX at late entry EBX+0C on equality",
                FirstStoreRawFileOffset, "89 43 0C"),
            Instruction("Store late entry EBX at entry EAX+8",
                SecondStoreRawFileOffset, "89 58 08"),
            Instruction("Jump to the bounded equal destination 9229",
                JumpRawFileOffset, "EB 53",
                branchTargetRawFileOffset: EqualDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFreshFieldAddress(uint entryEax) =>
        unchecked(entryEax + FreshFieldDisplacement);

    public static uint DeriveFirstStoreAddress(uint lateEntryEbx) =>
        unchecked(lateEntryEbx + FirstStoreDisplacement);

    public static uint DeriveSecondStoreAddress(uint entryEax) =>
        unchecked(entryEax + SecondStoreDisplacement);

    public static CapacityFinalCalleeLargeSuccessorWritesPlan Project(
        CapacityFinalCalleeLargeSuccessorWritesCapture capture)
    {
        var reads =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeSuccessorWritesRead>();
        var writes =
            ImmutableArray.CreateBuilder<CapacityFinalCalleeLargeSuccessorWritesWrite>();
        bool entryEaxAvailable = false;
        bool compareReached = false;
        uint? compareResult = null;
        CapacityFinalCalleeLargeSuccessorWritesCompareFlags? compareFlags = null;
        bool jnzReached = false;
        bool? jnzTaken = null;
        uint? jnzDestination = null;
        bool jumpReached = false;
        bool lateEbxUnreached = false;
        bool unusedLateEbx = false;
        bool firstStoreReached = false;
        uint? firstStoreAddress = null;
        uint? firstStoreValue = null;
        uint? secondStoreAddress = null;
        uint? secondStoreValue = null;

        CapacityFinalCalleeLargeSuccessorWritesPlan Finish(
            CapacityFinalCalleeLargeSuccessorWritesOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEAXCaptured = capture.EntryEAX.HasValue,
                FreshFieldReadReached = reads.Count != 0,
                FreshFieldAddress = reads.Count != 0 ? reads[0].Address : null,
                FreshFieldCaptured = reads.Count != 0 && reads[0].ValueCaptured,
                FreshFieldValue = reads.Count != 0 ? reads[0].Value : null,
                EntryEDXCaptured = entryEaxAvailable && capture.EntryEDX.HasValue,
                CompareReached = compareReached,
                CompareResult = compareResult,
                CompareFlags = compareFlags,
                JnzReached = jnzReached,
                JnzTaken = jnzTaken,
                JnzSelectedDestinationRawFileOffset = jnzDestination,
                FirstStoreReached = firstStoreReached,
                FirstStoreAddress = firstStoreAddress,
                FirstStoreValue = firstStoreValue,
                SecondStoreReached = secondStoreAddress.HasValue,
                SecondStoreAddress = secondStoreAddress,
                SecondStoreValue = secondStoreValue,
                JumpReached = jumpReached,
                JumpTargetRawFileOffset = jumpReached
                    ? EqualDestinationRawFileOffset
                    : null,
                JumpTargetVirtualAddress = jumpReached
                    ? EqualDestinationVirtualAddress
                    : null,
                DestinationEntered = false,
                FlagsPreservedAcrossStores =
                    compareReached && firstStoreAddress.HasValue
                    && secondStoreAddress.HasValue && jumpReached,
                UnreachedFreshFieldCaptureProvided =
                    !entryEaxAvailable && capture.FreshEaxPlus8.HasValue,
                UnreachedEntryEDXCaptureProvided =
                    !entryEaxAvailable && capture.EntryEDX.HasValue,
                UnreachedLateEntryEBXCaptureProvided = lateEbxUnreached
                    && capture.LateEntryEBX.HasValue,
                UnusedLateEntryEBXCaptureProvided = unusedLateEbx,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                Reads = reads.ToImmutable(),
                Writes = writes.ToImmutable(),
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEAX is not uint entryEax)
        {
            lateEbxUnreached = true;
            return Finish(
                CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingEntryEAX,
                EntryRawFileOffset);
        }

        entryEaxAvailable = true;
        uint freshFieldAddress = DeriveFreshFieldAddress(entryEax);
        reads.Add(new(
            CompareRawFileOffset,
            freshFieldAddress,
            capture.FreshEaxPlus8.HasValue,
            capture.FreshEaxPlus8));

        if (capture.FreshEaxPlus8 is not uint freshField)
        {
            lateEbxUnreached = true;
            CapacityFinalCalleeLargeSuccessorWritesOutcome outcome =
                capture.EntryEDX.HasValue
                    ? CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingFreshField
                    : CapacityFinalCalleeLargeSuccessorWritesOutcome
                        .MissingFreshFieldAndEntryEDX;
            return Finish(outcome, CompareRawFileOffset);
        }

        if (capture.EntryEDX is not uint entryEdx)
        {
            lateEbxUnreached = true;
            return Finish(
                CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingEntryEDX,
                CompareRawFileOffset);
        }

        uint difference = unchecked(freshField - entryEdx);
        compareResult = difference;
        compareFlags = CalculateCompareFlags(freshField, entryEdx, difference);
        compareReached = true;
        jnzReached = true;
        jnzTaken = !compareFlags.Value.Zero;
        if (jnzTaken.Value)
        {
            jnzDestination = UnequalDestinationRawFileOffset;
            unusedLateEbx = capture.LateEntryEBX.HasValue;
            return Finish(
                CapacityFinalCalleeLargeSuccessorWritesOutcome.UnequalTo8E76,
                UnequalDestinationRawFileOffset,
                exactCleanExitAdmitted: !unusedLateEbx);
        }

        jnzDestination = FirstStoreRawFileOffset;
        firstStoreReached = true;
        if (capture.LateEntryEBX is not uint lateEntryEbx)
        {
            return Finish(
                CapacityFinalCalleeLargeSuccessorWritesOutcome
                    .MissingLateEntryEBXForEqualStores,
                FirstStoreRawFileOffset);
        }

        firstStoreAddress = DeriveFirstStoreAddress(lateEntryEbx);
        firstStoreValue = entryEax;
        writes.Add(new(
            FirstStoreRawFileOffset,
            firstStoreAddress.Value,
            firstStoreValue.Value));

        secondStoreAddress = DeriveSecondStoreAddress(entryEax);
        secondStoreValue = lateEntryEbx;
        writes.Add(new(
            SecondStoreRawFileOffset,
            secondStoreAddress.Value,
            secondStoreValue.Value));

        jumpReached = true;
        return Finish(
            CapacityFinalCalleeLargeSuccessorWritesOutcome.EqualTo9229,
            EqualDestinationRawFileOffset,
            exactCleanExitAdmitted: true);
    }

    private static CapacityFinalCalleeLargeSuccessorWritesCompareFlags
        CalculateCompareFlags(uint left, uint right, uint result) =>
        new(
            left < right,
            HasEvenParity((byte)result),
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeLargeSuccessorWritesInstructionEvidence Instruction(
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
