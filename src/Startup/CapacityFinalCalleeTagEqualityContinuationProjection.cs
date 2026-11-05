using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeTagEqualityContinuationCapture(
    bool CaptureValidatedEntry,
    uint EntryESI,
    uint EntryEDI,
    uint EntryESP,
    uint? InitialGlobalDword,
    uint? FreshTagDword,
    uint? CapacityDword,
    bool? CallReturnedNormally);

public enum CapacityFinalCalleeTagEqualityContinuationOutcome
{
    EntryNotValidated,
    MissingInitialGlobalDword,
    MissingFreshTagDword,
    MissingCapacityDword,
    CapacityUnsignedBelowOrEqual,
    MissingCallReturnStatus,
    CallDidNotReturnNormally,
    CallReturnedNormally
}

public enum CapacityFinalCalleeTagEqualityContinuationReadKind
{
    InitialGlobalDword,
    FreshTagDword,
    CapacityDword
}

public readonly record struct CapacityFinalCalleeTagEqualityContinuationReadDescriptor(
    uint RawFileOffset,
    CapacityFinalCalleeTagEqualityContinuationReadKind Kind,
    uint PeVirtualAddress,
    string? BaseRegister,
    uint Displacement);

public readonly record struct CapacityFinalCalleeTagEqualityContinuationRead(
    uint RawFileOffset,
    CapacityFinalCalleeTagEqualityContinuationReadKind Kind,
    uint Address,
    bool ValueCaptured,
    uint? Value);

public readonly record struct CapacityFinalCalleeTagEqualityContinuationWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeTagEqualityContinuationFlags(
    bool Carry,
    bool Parity,
    bool? AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeTagEqualityContinuationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    byte? ImmediateValue = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeTagEqualityContinuationPlan(
    CapacityFinalCalleeTagEqualityContinuationCapture Capture,
    CapacityFinalCalleeTagEqualityContinuationOutcome Outcome,
    ImmutableArray<CapacityFinalCalleeTagEqualityContinuationRead> Reads,
    ImmutableArray<CapacityFinalCalleeTagEqualityContinuationWrite> Writes,
    uint? EAX,
    uint? ESI,
    uint? EDI,
    uint? ECX,
    uint? EDX,
    uint? ESP,
    bool InitialGlobalReadReached,
    bool InitialGlobalDwordCaptured,
    uint? InitialGlobalDword,
    bool ComputedEcxReached,
    uint? ComputedEcx,
    bool TagComparisonReadReached,
    bool FreshTagDwordCaptured,
    uint? FreshTagDword,
    bool TagComparisonReached,
    CapacityFinalCalleeTagEqualityContinuationFlags? TagComparisonFlags,
    bool TagNotEqualBranchReached,
    bool? EntryEsiEqualsFreshTagDword,
    bool? TagNotEqualBranchTaken,
    bool TagClearWritesReached,
    bool CapacityReadReached,
    bool CapacityDwordCaptured,
    uint? CapacityDword,
    bool CapacityComparisonReached,
    CapacityFinalCalleeTagEqualityContinuationFlags? CapacityComparisonFlags,
    bool CapacityJbeReached,
    bool? CapacityJbeTaken,
    uint? CapacityBranchDestinationRawFileOffset,
    uint? CapacityBranchDestinationVirtualAddress,
    bool CallReached,
    bool? CallReturnedNormally,
    bool CallRegisterOutputsUnknown,
    uint? PreCallEAX,
    uint? PreCallESI,
    uint? PreCallEDI,
    uint? PreCallECX,
    uint? PreCallEDX,
    uint? PreCallESP,
    bool UnreachedInitialGlobalDwordCaptureProvided,
    bool UnreachedFreshTagDwordCaptureProvided,
    bool UnreachedCapacityDwordCaptureProvided,
    bool UnreachedCallReturnStatusProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the bounded final-callee tag-equality continuation from independently
/// validated ESI/EDI/ESP values and each reached memory-read capture. The direct call
/// is opaque; its register values are reported only as pre-call values.
/// </summary>
public static class CapacityFinalCalleeTagEqualityContinuationProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8F21;
    public const uint EntryVirtualAddress = 0x016C9B21;
    public const uint InitialGlobalReadRawFileOffset = 0x012C8F21;
    public const uint AddEdiRawFileOffset = 0x012C8F27;
    public const uint StoreEntryEsiRawFileOffset = 0x012C8F29;
    public const uint MoveEaxEcxRawFileOffset = 0x012C8F2F;
    public const uint StoreComputedEcxRawFileOffset = 0x012C8F31;
    public const uint OrEaxOneRawFileOffset = 0x012C8F37;
    public const uint StoreEsiPlusFourRawFileOffset = 0x012C8F3A;
    public const uint TagComparisonReadRawFileOffset = 0x012C8F3D;
    public const uint TagNotEqualBranchRawFileOffset = 0x012C8F43;
    public const uint ClearTagRawFileOffset = 0x012C8F45;
    public const uint ClearSecondaryTagRawFileOffset = 0x012C8F4F;
    public const uint CapacityReadRawFileOffset = 0x012C8F59;
    public const uint CapacityJbeRawFileOffset = 0x012C8F5F;
    public const uint ClearEdxRawFileOffset = 0x012C8F65;
    public const uint LoadCalleeAddressRawFileOffset = 0x012C8F67;
    public const uint CallRawFileOffset = 0x012C8F6C;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8F71;
    public const uint BoundaryVirtualAddressExclusive = 0x016C9B71;

    public const uint InitialGlobalDwordVirtualAddress = 0x01EDADD4;
    public const uint EntryEsiPublicationVirtualAddress = 0x01EDADE0;
    public const uint FreshTagDwordVirtualAddress = 0x01EDADDC;
    public const uint SecondaryTagVirtualAddress = 0x01EDADD0;
    public const uint CapacityDwordVirtualAddress = 0x01EDADE4;
    public const uint CalleeVirtualAddress = 0x01EDADC8;
    public const uint EntryEsiDwordDisplacement = 4;
    public const uint CapacityBranchDestinationRawFileOffset = 0x012C9441;
    public const uint CapacityBranchDestinationVirtualAddress = 0x016CA041;
    public const uint DirectCallTargetRawFileOffset = 0x012C7BF0;
    public const uint DirectCallTargetVirtualAddress = 0x016C87F0;

    public const string CapturedWindowBytes =
        "8B 0D D4 AD ED 01 03 CF 89 35 E0 AD ED 01 8B C1 89 0D D4 AD ED 01 " +
        "83 C8 01 89 46 04 3B 35 DC AD ED 01 75 14 C7 05 DC AD ED 01 00 00 00 00 " +
        "C7 05 D0 AD ED 01 00 00 00 00 3B 0D E4 AD ED 01 0F 86 DC 04 00 00 " +
        "33 D2 B9 C8 AD ED 01 E8 7F EC FF FF";

    public static ImmutableArray<CapacityFinalCalleeTagEqualityContinuationReadDescriptor>
        OrderedReadDescriptors { get; } =
        ImmutableArray.Create(
            new CapacityFinalCalleeTagEqualityContinuationReadDescriptor(
                InitialGlobalReadRawFileOffset,
                CapacityFinalCalleeTagEqualityContinuationReadKind.InitialGlobalDword,
                InitialGlobalDwordVirtualAddress,
                null,
                0),
            new CapacityFinalCalleeTagEqualityContinuationReadDescriptor(
                TagComparisonReadRawFileOffset,
                CapacityFinalCalleeTagEqualityContinuationReadKind.FreshTagDword,
                FreshTagDwordVirtualAddress,
                null,
                0),
            new CapacityFinalCalleeTagEqualityContinuationReadDescriptor(
                CapacityReadRawFileOffset,
                CapacityFinalCalleeTagEqualityContinuationReadKind.CapacityDword,
                CapacityDwordVirtualAddress,
                null,
                0));

    public static ImmutableArray<CapacityFinalCalleeTagEqualityContinuationInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load the initial global DWORD into ECX",
                InitialGlobalReadRawFileOffset, "8B 0D D4 AD ED 01",
                peVirtualAddressOperand: InitialGlobalDwordVirtualAddress),
            Instruction("Add entry EDI to ECX with DWORD wrap",
                AddEdiRawFileOffset, "03 CF"),
            Instruction("Publish entry ESI to the global DWORD",
                StoreEntryEsiRawFileOffset, "89 35 E0 AD ED 01",
                peVirtualAddressOperand: EntryEsiPublicationVirtualAddress),
            Instruction("Copy the computed ECX value to EAX",
                MoveEaxEcxRawFileOffset, "8B C1"),
            Instruction("Store computed ECX to the initial global DWORD",
                StoreComputedEcxRawFileOffset, "89 0D D4 AD ED 01",
                peVirtualAddressOperand: InitialGlobalDwordVirtualAddress),
            Instruction("Set EAX bit zero",
                OrEaxOneRawFileOffset, "83 C8 01", immediateValue: 1),
            Instruction("Store EAX at entry ESI plus four with DWORD wrap",
                StoreEsiPlusFourRawFileOffset, "89 46 04"),
            Instruction("Compare entry ESI with the freshly captured global DWORD",
                TagComparisonReadRawFileOffset, "3B 35 DC AD ED 01",
                peVirtualAddressOperand: FreshTagDwordVirtualAddress),
            Instruction("Jump to 12C8F59 when the compared DWORDs differ",
                TagNotEqualBranchRawFileOffset, "75 14",
                branchTargetRawFileOffset: CapacityReadRawFileOffset,
                branchTargetVirtualAddress: RawFileOffsetToVirtualAddress(
                    CapacityReadRawFileOffset)),
            Instruction("Clear the tag global when the DWORDs are equal",
                ClearTagRawFileOffset, "C7 05 DC AD ED 01 00 00 00 00",
                peVirtualAddressOperand: FreshTagDwordVirtualAddress),
            Instruction("Clear the secondary tag global",
                ClearSecondaryTagRawFileOffset, "C7 05 D0 AD ED 01 00 00 00 00",
                peVirtualAddressOperand: SecondaryTagVirtualAddress),
            Instruction("Compare computed ECX with the fresh capacity DWORD",
                CapacityReadRawFileOffset, "3B 0D E4 AD ED 01",
                peVirtualAddressOperand: CapacityDwordVirtualAddress),
            Instruction("JBE to 12C9441 when computed ECX is unsigned below or equal",
                CapacityJbeRawFileOffset, "0F 86 DC 04 00 00",
                branchTargetRawFileOffset: CapacityBranchDestinationRawFileOffset,
                branchTargetVirtualAddress: CapacityBranchDestinationVirtualAddress),
            Instruction("Clear EDX before the direct call",
                ClearEdxRawFileOffset, "33 D2"),
            Instruction("Load the callee pointer PE VA 1EDADC8 into ECX",
                LoadCalleeAddressRawFileOffset, "B9 C8 AD ED 01",
                peVirtualAddressOperand: CalleeVirtualAddress),
            Instruction("Direct call to raw target 12C7BF0",
                CallRawFileOffset, "E8 7F EC FF FF",
                branchTargetRawFileOffset: DirectCallTargetRawFileOffset,
                branchTargetVirtualAddress: DirectCallTargetVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint EntryEsiAddress(uint entryEsi) =>
        unchecked(entryEsi + EntryEsiDwordDisplacement);

    public static CapacityFinalCalleeTagEqualityContinuationPlan Project(
        CapacityFinalCalleeTagEqualityContinuationCapture capture)
    {
        var state = new ProjectionState(capture);
        if (!capture.CaptureValidatedEntry)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeTagEqualityContinuationOutcome.EntryNotValidated,
                state,
                unreachedInitialGlobalDwordCaptureProvided:
                    capture.InitialGlobalDword.HasValue,
                unreachedFreshTagDwordCaptureProvided: capture.FreshTagDword.HasValue,
                unreachedCapacityDwordCaptureProvided: capture.CapacityDword.HasValue,
                unreachedCallReturnStatusProvided: capture.CallReturnedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        state.Reads.Add(new(
            InitialGlobalReadRawFileOffset,
            CapacityFinalCalleeTagEqualityContinuationReadKind.InitialGlobalDword,
            InitialGlobalDwordVirtualAddress,
            capture.InitialGlobalDword.HasValue,
            capture.InitialGlobalDword));
        if (capture.InitialGlobalDword is not uint initialGlobalDword)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeTagEqualityContinuationOutcome.MissingInitialGlobalDword,
                state,
                initialGlobalReadReached: true,
                unreachedFreshTagDwordCaptureProvided: capture.FreshTagDword.HasValue,
                unreachedCapacityDwordCaptureProvided: capture.CapacityDword.HasValue,
                unreachedCallReturnStatusProvided: capture.CallReturnedNormally.HasValue,
                stopAt: InitialGlobalReadRawFileOffset);
        }

        state.ECX = unchecked(initialGlobalDword + capture.EntryEDI);
        state.ComputedEcx = state.ECX;
        state.Writes.Add(new(
            StoreEntryEsiRawFileOffset,
            EntryEsiPublicationVirtualAddress,
            capture.EntryESI));
        state.Writes.Add(new(
            StoreComputedEcxRawFileOffset,
            InitialGlobalDwordVirtualAddress,
            state.ECX.Value));
        state.EAX = state.ECX;
        state.EAX = state.EAX.Value | 1;
        state.Writes.Add(new(
            StoreEsiPlusFourRawFileOffset,
            EntryEsiAddress(capture.EntryESI),
            state.EAX.Value));

        state.Reads.Add(new(
            TagComparisonReadRawFileOffset,
            CapacityFinalCalleeTagEqualityContinuationReadKind.FreshTagDword,
            FreshTagDwordVirtualAddress,
            capture.FreshTagDword.HasValue,
            capture.FreshTagDword));
        if (capture.FreshTagDword is not uint freshTagDword)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeTagEqualityContinuationOutcome.MissingFreshTagDword,
                state,
                initialGlobalReadReached: true,
                initialGlobalDwordCaptured: true,
                initialGlobalDword: initialGlobalDword,
                computedEcxReached: true,
                tagComparisonReadReached: true,
                unreachedCapacityDwordCaptureProvided: capture.CapacityDword.HasValue,
                unreachedCallReturnStatusProvided: capture.CallReturnedNormally.HasValue,
                stopAt: TagComparisonReadRawFileOffset);
        }

        state.TagComparisonReached = true;
        state.TagComparisonFlags = CompareFlags(capture.EntryESI, freshTagDword);
        bool entryEsiEqualsFreshTagDword = state.TagComparisonFlags.Value.Zero;
        bool tagNotEqualBranchTaken = !entryEsiEqualsFreshTagDword;
        if (entryEsiEqualsFreshTagDword)
        {
            state.Writes.Add(new(
                ClearTagRawFileOffset,
                FreshTagDwordVirtualAddress,
                0));
            state.Writes.Add(new(
                ClearSecondaryTagRawFileOffset,
                SecondaryTagVirtualAddress,
                0));
            state.TagClearWritesReached = true;
        }

        state.Reads.Add(new(
            CapacityReadRawFileOffset,
            CapacityFinalCalleeTagEqualityContinuationReadKind.CapacityDword,
            CapacityDwordVirtualAddress,
            capture.CapacityDword.HasValue,
            capture.CapacityDword));
        if (capture.CapacityDword is not uint capacityDword)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeTagEqualityContinuationOutcome.MissingCapacityDword,
                state,
                initialGlobalReadReached: true,
                initialGlobalDwordCaptured: true,
                initialGlobalDword: initialGlobalDword,
                computedEcxReached: true,
                tagComparisonReadReached: true,
                freshTagDwordCaptured: true,
                freshTagDword: freshTagDword,
                tagNotEqualBranchReached: true,
                entryEsiEqualsFreshTagDword: entryEsiEqualsFreshTagDword,
                tagNotEqualBranchTaken: tagNotEqualBranchTaken,
                capacityReadReached: true,
                unreachedCallReturnStatusProvided: capture.CallReturnedNormally.HasValue,
                stopAt: CapacityReadRawFileOffset);
        }

        state.CapacityComparisonReached = true;
        state.CapacityComparisonFlags = CompareFlags(state.ECX!.Value, capacityDword);
        bool capacityJbeTaken =
            state.CapacityComparisonFlags.Value.Carry
            || state.CapacityComparisonFlags.Value.Zero;
        if (capacityJbeTaken)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeTagEqualityContinuationOutcome.CapacityUnsignedBelowOrEqual,
                state,
                initialGlobalReadReached: true,
                initialGlobalDwordCaptured: true,
                initialGlobalDword: initialGlobalDword,
                computedEcxReached: true,
                tagComparisonReadReached: true,
                freshTagDwordCaptured: true,
                freshTagDword: freshTagDword,
                tagNotEqualBranchReached: true,
                entryEsiEqualsFreshTagDword: entryEsiEqualsFreshTagDword,
                tagNotEqualBranchTaken: tagNotEqualBranchTaken,
                capacityReadReached: true,
                capacityDwordCaptured: true,
                capacityDword: capacityDword,
                capacityJbeTaken: true,
                stopAt: CapacityBranchDestinationRawFileOffset);
        }

        state.EDX = 0;
        state.ECX = CalleeVirtualAddress;
        if (capture.CallReturnedNormally is not bool callReturnedNormally)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeTagEqualityContinuationOutcome.MissingCallReturnStatus,
                state,
                initialGlobalReadReached: true,
                initialGlobalDwordCaptured: true,
                initialGlobalDword: initialGlobalDword,
                computedEcxReached: true,
                tagComparisonReadReached: true,
                freshTagDwordCaptured: true,
                freshTagDword: freshTagDword,
                tagNotEqualBranchReached: true,
                entryEsiEqualsFreshTagDword: entryEsiEqualsFreshTagDword,
                tagNotEqualBranchTaken: tagNotEqualBranchTaken,
                capacityReadReached: true,
                capacityDwordCaptured: true,
                capacityDword: capacityDword,
                capacityJbeTaken: false,
                callReached: true,
                stopAt: CallRawFileOffset);
        }

        return BuildPlan(
            capture,
            callReturnedNormally
                ? CapacityFinalCalleeTagEqualityContinuationOutcome.CallReturnedNormally
                : CapacityFinalCalleeTagEqualityContinuationOutcome.CallDidNotReturnNormally,
            state,
            initialGlobalReadReached: true,
            initialGlobalDwordCaptured: true,
            initialGlobalDword: initialGlobalDword,
            computedEcxReached: true,
            tagComparisonReadReached: true,
            freshTagDwordCaptured: true,
            freshTagDword: freshTagDword,
            tagNotEqualBranchReached: true,
            entryEsiEqualsFreshTagDword: entryEsiEqualsFreshTagDword,
            tagNotEqualBranchTaken: tagNotEqualBranchTaken,
            capacityReadReached: true,
            capacityDwordCaptured: true,
            capacityDword: capacityDword,
            capacityJbeTaken: false,
            callReached: true,
            callReturnedNormally: callReturnedNormally,
            stopAt: callReturnedNormally
                ? BoundaryRawFileOffsetExclusive
                : CallRawFileOffset);
    }

    private static CapacityFinalCalleeTagEqualityContinuationFlags CompareFlags(
        uint left,
        uint right)
    {
        uint result = unchecked(left - right);
        return new(
            left < right,
            BitOperations.PopCount((uint)(byte)result) % 2 == 0,
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80000000) != 0,
            ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static CapacityFinalCalleeTagEqualityContinuationInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null,
        byte? immediateValue = null,
        uint? branchTargetRawFileOffset = null,
        uint? branchTargetVirtualAddress = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            peVirtualAddressOperand,
            immediateValue,
            branchTargetRawFileOffset,
            branchTargetVirtualAddress);

    private static CapacityFinalCalleeTagEqualityContinuationPlan BuildPlan(
        CapacityFinalCalleeTagEqualityContinuationCapture capture,
        CapacityFinalCalleeTagEqualityContinuationOutcome outcome,
        ProjectionState state,
        bool initialGlobalReadReached = false,
        bool initialGlobalDwordCaptured = false,
        uint? initialGlobalDword = null,
        bool computedEcxReached = false,
        bool tagComparisonReadReached = false,
        bool freshTagDwordCaptured = false,
        uint? freshTagDword = null,
        bool tagNotEqualBranchReached = false,
        bool? entryEsiEqualsFreshTagDword = null,
        bool? tagNotEqualBranchTaken = null,
        bool capacityReadReached = false,
        bool capacityDwordCaptured = false,
        uint? capacityDword = null,
        bool? capacityJbeTaken = null,
        bool callReached = false,
        bool? callReturnedNormally = null,
        bool unreachedInitialGlobalDwordCaptureProvided = false,
        bool unreachedFreshTagDwordCaptureProvided = false,
        bool unreachedCapacityDwordCaptureProvided = false,
        bool unreachedCallReturnStatusProvided = false,
        uint stopAt = EntryRawFileOffset)
    {
        bool capacityJbeReached = state.CapacityComparisonReached;
        bool exposePreCallRegisters = callReached;
        return new(
            capture,
            outcome,
            state.Reads.ToImmutableArray(),
            state.Writes.ToImmutableArray(),
            state.EAX,
            state.ESI,
            state.EDI,
            state.ECX,
            state.EDX,
            state.ESP,
            initialGlobalReadReached,
            initialGlobalReadReached && initialGlobalDwordCaptured,
            initialGlobalReadReached && initialGlobalDwordCaptured
                ? initialGlobalDword
                : null,
            computedEcxReached,
            computedEcxReached ? state.ComputedEcx : null,
            tagComparisonReadReached,
            tagComparisonReadReached && freshTagDwordCaptured,
            tagComparisonReadReached && freshTagDwordCaptured ? freshTagDword : null,
            state.TagComparisonReached,
            state.TagComparisonReached ? state.TagComparisonFlags : null,
            tagNotEqualBranchReached,
            tagNotEqualBranchReached ? entryEsiEqualsFreshTagDword : null,
            tagNotEqualBranchReached ? tagNotEqualBranchTaken : null,
            state.TagClearWritesReached,
            capacityReadReached,
            capacityReadReached && capacityDwordCaptured,
            capacityReadReached && capacityDwordCaptured ? capacityDword : null,
            state.CapacityComparisonReached,
            state.CapacityComparisonReached ? state.CapacityComparisonFlags : null,
            capacityJbeReached,
            capacityJbeReached ? capacityJbeTaken : null,
            capacityJbeReached && capacityJbeTaken == true
                ? CapacityBranchDestinationRawFileOffset
                : null,
            capacityJbeReached && capacityJbeTaken == true
                ? CapacityBranchDestinationVirtualAddress
                : null,
            callReached,
            callReached ? callReturnedNormally : null,
            callReached,
            exposePreCallRegisters ? state.EAX : null,
            exposePreCallRegisters ? state.ESI : null,
            exposePreCallRegisters ? state.EDI : null,
            exposePreCallRegisters ? state.ECX : null,
            exposePreCallRegisters ? state.EDX : null,
            exposePreCallRegisters ? state.ESP : null,
            (!initialGlobalReadReached && capture.InitialGlobalDword.HasValue)
                || unreachedInitialGlobalDwordCaptureProvided,
            (!tagComparisonReadReached && capture.FreshTagDword.HasValue)
                || unreachedFreshTagDwordCaptureProvided,
            (!capacityReadReached && capture.CapacityDword.HasValue)
                || unreachedCapacityDwordCaptureProvided,
            (!callReached && capture.CallReturnedNormally.HasValue)
                || unreachedCallReturnStatusProvided,
            stopAt);
    }

    private sealed class ProjectionState
    {
        public ProjectionState(CapacityFinalCalleeTagEqualityContinuationCapture capture)
        {
            ESI = capture.EntryESI;
            EDI = capture.EntryEDI;
            ESP = capture.EntryESP;
        }

        public List<CapacityFinalCalleeTagEqualityContinuationRead> Reads { get; } = new();
        public List<CapacityFinalCalleeTagEqualityContinuationWrite> Writes { get; } = new();
        public uint? EAX { get; set; }
        public uint? ESI { get; }
        public uint? EDI { get; }
        public uint? ECX { get; set; }
        public uint? EDX { get; set; }
        public uint? ESP { get; }
        public uint? ComputedEcx { get; set; }
        public bool TagComparisonReached { get; set; }
        public CapacityFinalCalleeTagEqualityContinuationFlags? TagComparisonFlags { get; set; }
        public bool TagClearWritesReached { get; set; }
        public bool CapacityComparisonReached { get; set; }
        public CapacityFinalCalleeTagEqualityContinuationFlags? CapacityComparisonFlags { get; set; }
    }
}
