using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateAlignedSizeCapture(
    bool EntryValidated,
    uint? EntryECX,
    uint? EntryEDI,
    uint? EntryEBP);

public enum CapacityFinalCalleeAlternateAlignedSizeOutcome
{
    EntryNotValidated,
    MissingEntryECX,
    MissingEntryEDI,
    MissingEntryEBP,
    BelowThresholdFallthrough,
    AtOrAboveThresholdBranchTarget
}

public readonly record struct CapacityFinalCalleeAlternateAlignedSizeFlags(
    bool? Carry,
    bool? Parity,
    bool? AuxiliaryCarry,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct CapacityFinalCalleeAlternateAlignedSizeInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeAlternateAlignedSizeStore(
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeAlternateAlignedSizePlan(
    CapacityFinalCalleeAlternateAlignedSizeCapture Capture,
    CapacityFinalCalleeAlternateAlignedSizeOutcome Outcome)
{
    public bool AndReached { get; init; }
    public bool EntryECXCaptured { get; init; }
    public uint? AlignedECX { get; init; }
    public CapacityFinalCalleeAlternateAlignedSizeFlags? AndFlags { get; init; }
    public bool MoveEAXReached { get; init; }
    public uint? EAXAfterMove { get; init; }
    public CapacityFinalCalleeAlternateAlignedSizeFlags? FlagsAfterMove { get; init; }
    public bool AddReached { get; init; }
    public bool EntryEDICaptured { get; init; }
    public uint? EDIAfterAdd { get; init; }
    public CapacityFinalCalleeAlternateAlignedSizeFlags? AddFlags { get; init; }
    public bool ShiftReached { get; init; }
    public uint? AlignedSize { get; init; }
    public CapacityFinalCalleeAlternateAlignedSizeFlags? ShiftFlags { get; init; }
    public bool StoreReached { get; init; }
    public bool EntryEBPCaptured { get; init; }
    public CapacityFinalCalleeAlternateAlignedSizeStore? Store { get; init; }
    public CapacityFinalCalleeAlternateAlignedSizeFlags? FlagsAfterStore { get; init; }
    public bool CompareReached { get; init; }
    public uint? CompareResult { get; init; }
    public CapacityFinalCalleeAlternateAlignedSizeFlags? CompareFlags { get; init; }
    public bool JnbReached { get; init; }
    public bool? JnbTaken { get; init; }
    public uint? SelectedDestinationRawFileOffset { get; init; }
    public bool ExactCleanAdmission { get; init; }
    public bool UnreachedEntryECXCaptureProvided { get; init; }
    public bool UnreachedEntryEDICaptureProvided { get; init; }
    public bool UnreachedEntryEBPCaptureProvided { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the aligned-size arithmetic and its unsigned threshold branch only.
/// </summary>
public static class CapacityFinalCalleeAlternateAlignedSizeProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C90F6;
    public const uint EntryVirtualAddress = 0x016C9CF6;
    public const uint AndRawFileOffset = 0x012C90F6;
    public const uint MoveEAXRawFileOffset = 0x012C90F9;
    public const uint AddRawFileOffset = 0x012C90FB;
    public const uint ShiftRawFileOffset = 0x012C90FD;
    public const uint StoreRawFileOffset = 0x012C9100;
    public const uint CompareRawFileOffset = 0x012C9103;
    public const uint JnbRawFileOffset = 0x012C9106;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C910C;
    public const uint BranchTargetRawFileOffset = 0x012C919B;
    public const uint FallthroughRawFileOffset = 0x012C910C;

    public const uint AlignmentMask = 0xFFFFFFF8;
    public const int ShiftCount = 3;
    public const uint Threshold = 0x20;
    public const uint LocalStoreDisplacement = 4;

    public const string CapturedWindowBytes =
        "83 E1 F8 8B C1 03 F9 C1 E8 03 89 45 FC 83 F8 20 0F 83 8F 00 00 00";

    public static ImmutableArray<CapacityFinalCalleeAlternateAlignedSizeInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Align entry ECX down to an eight-byte boundary",
                AndRawFileOffset, "83 E1 F8"),
            Instruction("Copy aligned ECX to EAX without changing flags",
                MoveEAXRawFileOffset, "8B C1"),
            Instruction("Add aligned ECX to entry EDI",
                AddRawFileOffset, "03 F9"),
            Instruction("Shift the aligned-size copy in EAX right by three",
                ShiftRawFileOffset, "C1 E8 03"),
            Instruction("Store the shifted size at entry EBP-4 without changing flags",
                StoreRawFileOffset, "89 45 FC"),
            Instruction("Compare the shifted size with 0x20",
                CompareRawFileOffset, "83 F8 20"),
            Instruction("JNB to 919B when the shifted size is unsigned-above-or-equal",
                JnbRawFileOffset, "0F 83 8F 00 00 00",
                branchTargetRawFileOffset: BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint AlignECX(uint entryECX) => entryECX & AlignmentMask;

    public static uint AddAlignedECXToEDI(uint entryEDI, uint alignedECX) =>
        unchecked(entryEDI + alignedECX);

    public static uint ShiftAlignedSize(uint alignedECX) =>
        alignedECX >> ShiftCount;

    public static uint LocalStoreAddress(uint entryEBP) =>
        unchecked(entryEBP - LocalStoreDisplacement);

    public static CapacityFinalCalleeAlternateAlignedSizePlan Project(
        CapacityFinalCalleeAlternateAlignedSizeCapture capture)
    {
        bool andReached = false;
        uint? alignedECX = null;
        CapacityFinalCalleeAlternateAlignedSizeFlags? andFlags = null;
        bool moveEAXReached = false;
        uint? eaxAfterMove = null;
        bool addReached = false;
        uint? ediAfterAdd = null;
        CapacityFinalCalleeAlternateAlignedSizeFlags? addFlags = null;
        bool shiftReached = false;
        uint? alignedSize = null;
        CapacityFinalCalleeAlternateAlignedSizeFlags? shiftFlags = null;
        bool storeReached = false;
        CapacityFinalCalleeAlternateAlignedSizeStore? store = null;
        bool compareReached = false;
        uint? compareResult = null;
        CapacityFinalCalleeAlternateAlignedSizeFlags? compareFlags = null;
        bool jnbReached = false;
        bool? jnbTaken = null;
        uint? destination = null;
        bool exactCleanAdmission = false;
        uint stopAt = EntryRawFileOffset;

        CapacityFinalCalleeAlternateAlignedSizePlan Finish(
            CapacityFinalCalleeAlternateAlignedSizeOutcome outcome) =>
            new(capture, outcome)
            {
                AndReached = andReached,
                EntryECXCaptured = andReached && capture.EntryECX.HasValue,
                AlignedECX = andReached ? alignedECX : null,
                AndFlags = andFlags,
                MoveEAXReached = moveEAXReached,
                EAXAfterMove = moveEAXReached ? eaxAfterMove : null,
                FlagsAfterMove = moveEAXReached ? andFlags : null,
                AddReached = addReached,
                EntryEDICaptured = addReached && capture.EntryEDI.HasValue,
                EDIAfterAdd = addReached ? ediAfterAdd : null,
                AddFlags = addFlags,
                ShiftReached = shiftReached,
                AlignedSize = shiftReached ? alignedSize : null,
                ShiftFlags = shiftFlags,
                StoreReached = storeReached,
                EntryEBPCaptured = storeReached && capture.EntryEBP.HasValue,
                Store = storeReached ? store : null,
                FlagsAfterStore = storeReached ? shiftFlags : null,
                CompareReached = compareReached,
                CompareResult = compareReached ? compareResult : null,
                CompareFlags = compareReached ? compareFlags : null,
                JnbReached = jnbReached,
                JnbTaken = jnbReached ? jnbTaken : null,
                SelectedDestinationRawFileOffset = jnbReached ? destination : null,
                ExactCleanAdmission = exactCleanAdmission,
                UnreachedEntryECXCaptureProvided = !andReached && capture.EntryECX.HasValue,
                UnreachedEntryEDICaptureProvided = !addReached && capture.EntryEDI.HasValue,
                UnreachedEntryEBPCaptureProvided = !storeReached && capture.EntryEBP.HasValue,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (!capture.EntryValidated)
        {
            return Finish(CapacityFinalCalleeAlternateAlignedSizeOutcome.EntryNotValidated);
        }

        if (capture.EntryECX is not uint entryECX)
        {
            return Finish(CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryECX);
        }

        andReached = true;
        alignedECX = AlignECX(entryECX);
        andFlags = CalculateAndFlags(alignedECX.Value);
        moveEAXReached = true;
        eaxAfterMove = alignedECX;

        if (capture.EntryEDI is not uint entryEDI)
        {
            stopAt = AddRawFileOffset;
            return Finish(CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryEDI);
        }

        addReached = true;
        ediAfterAdd = AddAlignedECXToEDI(entryEDI, alignedECX.Value);
        addFlags = CalculateAddFlags(entryEDI, alignedECX.Value, ediAfterAdd.Value);
        shiftReached = true;
        alignedSize = ShiftAlignedSize(eaxAfterMove.Value);
        shiftFlags = CalculateShiftFlags(eaxAfterMove.Value, alignedSize.Value);

        if (capture.EntryEBP is not uint entryEBP)
        {
            stopAt = StoreRawFileOffset;
            return Finish(CapacityFinalCalleeAlternateAlignedSizeOutcome.MissingEntryEBP);
        }

        storeReached = true;
        store = new(
            StoreRawFileOffset,
            LocalStoreAddress(entryEBP),
            alignedSize.Value);
        compareReached = true;
        compareResult = unchecked(alignedSize.Value - Threshold);
        compareFlags = CalculateSubFlags(alignedSize.Value, Threshold, compareResult.Value);
        jnbReached = true;
        jnbTaken = compareFlags.Value.Carry == false;
        destination = jnbTaken.Value
            ? BranchTargetRawFileOffset
            : FallthroughRawFileOffset;
        exactCleanAdmission = true;
        stopAt = destination.Value;

        return Finish(
            jnbTaken.Value
                ? CapacityFinalCalleeAlternateAlignedSizeOutcome.AtOrAboveThresholdBranchTarget
                : CapacityFinalCalleeAlternateAlignedSizeOutcome.BelowThresholdFallthrough);
    }

    private static CapacityFinalCalleeAlternateAlignedSizeFlags CalculateAndFlags(
        uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: false);

    private static CapacityFinalCalleeAlternateAlignedSizeFlags CalculateAddFlags(
        uint left,
        uint right,
        uint result)
    {
        ulong sum = (ulong)left + right;
        return new(
            Carry: sum > uint.MaxValue,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: (~(left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static CapacityFinalCalleeAlternateAlignedSizeFlags CalculateShiftFlags(
        uint value,
        uint result) =>
        new(
            Carry: (value & (1u << (ShiftCount - 1))) != 0,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: null);

    private static CapacityFinalCalleeAlternateAlignedSizeFlags CalculateSubFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeAlternateAlignedSizeInstructionEvidence Instruction(
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
