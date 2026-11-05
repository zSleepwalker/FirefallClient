using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateSuccessorGateCapture(
    uint? EntryEBP,
    uint? FreshFrameMinusC,
    uint? EntryEAX,
    uint? EntryEBX,
    uint? FreshFieldAtEaxPlus8,
    uint? EntryEDX);

public enum CapacityFinalCalleeAlternateSuccessorGateOutcome
{
    MissingEntryEBP,
    MissingFreshFrameMinusC,
    MissingEntryEAX,
    EqualFrameValueToEntryEAX,
    MissingEntryEBX,
    BelowToOpaqueCallBoundary,
    MissingFreshFieldAtEaxPlus8,
    MissingEntryEDX,
    EqualFreshFieldToEntryEDX,
    UnequalFreshFieldToOpaqueCallBoundary
}

public readonly record struct CapacityFinalCalleeAlternateSuccessorGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeAlternateSuccessorGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeAlternateSuccessorGateWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeAlternateSuccessorGatePlan(
    CapacityFinalCalleeAlternateSuccessorGateCapture Capture,
    CapacityFinalCalleeAlternateSuccessorGateOutcome Outcome)
{
    public bool EntryEBPCaptured { get; init; }
    public bool FrameMinusCReadReached { get; init; }
    public uint? FrameMinusCAddress { get; init; }
    public bool FrameMinusCCaptured { get; init; }
    public uint? FrameMinusC { get; init; }
    public bool EntryEAXCaptured { get; init; }
    public bool FrameCompareReached { get; init; }
    public uint? FrameCompareLeft { get; init; }
    public uint? FrameCompareRight { get; init; }
    public uint? FrameCompareResult { get; init; }
    public CapacityFinalCalleeAlternateSuccessorGateFlags? FrameCompareFlags { get; init; }
    public bool FrameJzReached { get; init; }
    public bool? FrameJzTaken { get; init; }
    public uint? FrameJzSelectedDestinationRawFileOffset { get; init; }
    public bool EntryEBXCaptured { get; init; }
    public bool RegisterCompareReached { get; init; }
    public uint? RegisterCompareLeft { get; init; }
    public uint? RegisterCompareRight { get; init; }
    public uint? RegisterCompareResult { get; init; }
    public CapacityFinalCalleeAlternateSuccessorGateFlags? RegisterCompareFlags { get; init; }
    public bool JbReached { get; init; }
    public bool? JbTaken { get; init; }
    public uint? JbSelectedDestinationRawFileOffset { get; init; }
    public bool FreshFieldReadReached { get; init; }
    public uint? FreshFieldAddress { get; init; }
    public bool FreshFieldCaptured { get; init; }
    public uint? FreshFieldValue { get; init; }
    public bool EntryEDXCaptured { get; init; }
    public bool FieldCompareReached { get; init; }
    public uint? FieldCompareLeft { get; init; }
    public uint? FieldCompareRight { get; init; }
    public uint? FieldCompareResult { get; init; }
    public CapacityFinalCalleeAlternateSuccessorGateFlags? FieldCompareFlags { get; init; }
    public bool FieldJzReached { get; init; }
    public bool? FieldJzTaken { get; init; }
    public uint? FieldJzSelectedDestinationRawFileOffset { get; init; }
    public bool UnreachedFreshFrameMinusCCaptureProvided { get; init; }
    public bool UnreachedEntryEAXCaptureProvided { get; init; }
    public bool LateEntryEAXCaptureProvided { get; init; }
    public bool UnreachedEntryEBXCaptureProvided { get; init; }
    public bool LateEntryEBXCaptureProvided { get; init; }
    public bool UnreachedFreshFieldCaptureProvided { get; init; }
    public bool LateFreshFieldCaptureProvided { get; init; }
    public bool UnreachedEntryEDXCaptureProvided { get; init; }
    public bool LateEntryEDXCaptureProvided { get; init; }
    public bool UnusedEntryEBXCaptureProvided { get; init; }
    public bool UnusedFreshFieldCaptureProvided { get; init; }
    public bool UnusedEntryEDXCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool ExactCleanBypassAdmitted { get; init; }
    public ImmutableArray<CapacityFinalCalleeAlternateSuccessorGateWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeAlternateSuccessorGateWrite>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
    public bool OpaqueCallReached => false;
}

/// <summary>
/// Projects the fresh EBP-C comparison, unsigned-below gate, and fresh
/// EAX+8 comparison. It stops at either the opaque-call boundary or 9190.
/// </summary>
public static class CapacityFinalCalleeAlternateSuccessorGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C917C;
    public const uint EntryVirtualAddress = 0x016C9D7C;
    public const uint FrameCompareRawFileOffset = 0x012C917C;
    public const uint FrameJzRawFileOffset = 0x012C917F;
    public const uint RegisterCompareRawFileOffset = 0x012C9181;
    public const uint JbRawFileOffset = 0x012C9183;
    public const uint FieldCompareRawFileOffset = 0x012C9185;
    public const uint FieldJzRawFileOffset = 0x012C9188;
    public const uint OpaqueCallRawFileOffset = 0x012C918A;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C9190;
    public const uint OpaqueCallCellVirtualAddress = 0x01AC4A24;
    public const uint OpaqueCallBoundaryRawFileOffset = 0x012C918A;
    public const uint FrameEqualDestinationRawFileOffset = 0x012C9190;
    public const uint BelowDestinationRawFileOffset = 0x012C918A;
    public const uint FieldEqualDestinationRawFileOffset = 0x012C9190;
    public const uint FieldUnequalDestinationRawFileOffset = 0x012C918A;

    public const uint EbpMinusCDisplacement = 0x0C;
    public const uint EaxPlus8Displacement = 0x08;
    public const string CapturedWindowBytes =
        "3B45F4740F3BC372053950087406FF15244AAC01";

    public static ImmutableArray<CapacityFinalCalleeAlternateSuccessorGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry EAX with fresh DWORD at entry EBP-C",
                FrameCompareRawFileOffset, "3B 45 F4"),
            Instruction("Jump equal to the clean 9190 destination",
                FrameJzRawFileOffset, "74 0F",
                branchTargetRawFileOffset: FrameEqualDestinationRawFileOffset),
            Instruction("Compare entry EAX with entry EBX",
                RegisterCompareRawFileOffset, "3B C3"),
            Instruction("Jump unsigned-below to the opaque-call boundary",
                JbRawFileOffset, "72 05",
                branchTargetRawFileOffset: BelowDestinationRawFileOffset),
            Instruction("Compare fresh DWORD at entry EAX+8 with entry EDX",
                FieldCompareRawFileOffset, "39 50 08"),
            Instruction("Jump equal to the clean 9190 destination",
                FieldJzRawFileOffset, "74 06",
                branchTargetRawFileOffset: FieldEqualDestinationRawFileOffset),
            Instruction("Opaque indirect call, not entered by this projection",
                OpaqueCallRawFileOffset, "FF 15 24 4A AC 01"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEbpMinusCAddress(uint entryEbp) =>
        unchecked(entryEbp - EbpMinusCDisplacement);

    public static uint DeriveEaxPlus8Address(uint entryEax) =>
        unchecked(entryEax + EaxPlus8Displacement);

    public static CapacityFinalCalleeAlternateSuccessorGatePlan Project(
        CapacityFinalCalleeAlternateSuccessorGateCapture capture)
    {
        bool ebpCaptured = false;
        bool frameReadReached = false;
        uint? frameAddress = null;
        bool frameCaptured = false;
        uint? frameValue = null;
        bool eaxCaptured = false;
        bool frameCompareReached = false;
        uint? frameCompareLeft = null;
        uint? frameCompareRight = null;
        uint? frameCompareResult = null;
        CapacityFinalCalleeAlternateSuccessorGateFlags? frameCompareFlags = null;
        bool frameJzReached = false;
        bool? frameJzTaken = null;
        uint? frameJzDestination = null;
        bool ebxCaptured = false;
        bool registerCompareReached = false;
        uint? registerCompareLeft = null;
        uint? registerCompareRight = null;
        uint? registerCompareResult = null;
        CapacityFinalCalleeAlternateSuccessorGateFlags? registerCompareFlags = null;
        bool jbReached = false;
        bool? jbTaken = null;
        uint? jbDestination = null;
        bool freshReadReached = false;
        uint? freshAddress = null;
        bool freshCaptured = false;
        uint? freshValue = null;
        bool edxCaptured = false;
        bool fieldCompareReached = false;
        uint? fieldCompareLeft = null;
        uint? fieldCompareRight = null;
        uint? fieldCompareResult = null;
        CapacityFinalCalleeAlternateSuccessorGateFlags? fieldCompareFlags = null;
        bool fieldJzReached = false;
        bool? fieldJzTaken = null;
        uint? fieldJzDestination = null;
        bool unreachedFrame = false;
        bool unreachedEax = false;
        bool lateEax = false;
        bool unreachedEbx = false;
        bool lateEbx = false;
        bool unreachedFresh = false;
        bool lateFresh = false;
        bool unreachedEdx = false;
        bool lateEdx = false;
        bool unusedEbx = false;
        bool unusedFresh = false;
        bool unusedEdx = false;

        CapacityFinalCalleeAlternateSuccessorGatePlan Finish(
            CapacityFinalCalleeAlternateSuccessorGateOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false,
            bool exactCleanBypassAdmitted = false) =>
            new(capture, outcome)
            {
                EntryEBPCaptured = ebpCaptured,
                FrameMinusCReadReached = frameReadReached,
                FrameMinusCAddress = frameAddress,
                FrameMinusCCaptured = frameCaptured,
                FrameMinusC = frameValue,
                EntryEAXCaptured = eaxCaptured,
                FrameCompareReached = frameCompareReached,
                FrameCompareLeft = frameCompareLeft,
                FrameCompareRight = frameCompareRight,
                FrameCompareResult = frameCompareResult,
                FrameCompareFlags = frameCompareFlags,
                FrameJzReached = frameJzReached,
                FrameJzTaken = frameJzTaken,
                FrameJzSelectedDestinationRawFileOffset = frameJzDestination,
                EntryEBXCaptured = ebxCaptured,
                RegisterCompareReached = registerCompareReached,
                RegisterCompareLeft = registerCompareLeft,
                RegisterCompareRight = registerCompareRight,
                RegisterCompareResult = registerCompareResult,
                RegisterCompareFlags = registerCompareFlags,
                JbReached = jbReached,
                JbTaken = jbTaken,
                JbSelectedDestinationRawFileOffset = jbDestination,
                FreshFieldReadReached = freshReadReached,
                FreshFieldAddress = freshAddress,
                FreshFieldCaptured = freshCaptured,
                FreshFieldValue = freshValue,
                EntryEDXCaptured = edxCaptured,
                FieldCompareReached = fieldCompareReached,
                FieldCompareLeft = fieldCompareLeft,
                FieldCompareRight = fieldCompareRight,
                FieldCompareResult = fieldCompareResult,
                FieldCompareFlags = fieldCompareFlags,
                FieldJzReached = fieldJzReached,
                FieldJzTaken = fieldJzTaken,
                FieldJzSelectedDestinationRawFileOffset = fieldJzDestination,
                UnreachedFreshFrameMinusCCaptureProvided = unreachedFrame,
                UnreachedEntryEAXCaptureProvided = unreachedEax,
                LateEntryEAXCaptureProvided = lateEax,
                UnreachedEntryEBXCaptureProvided = unreachedEbx,
                LateEntryEBXCaptureProvided = lateEbx,
                UnreachedFreshFieldCaptureProvided = unreachedFresh,
                LateFreshFieldCaptureProvided = lateFresh,
                UnreachedEntryEDXCaptureProvided = unreachedEdx,
                LateEntryEDXCaptureProvided = lateEdx,
                UnusedEntryEBXCaptureProvided = unusedEbx,
                UnusedFreshFieldCaptureProvided = unusedFresh,
                UnusedEntryEDXCaptureProvided = unusedEdx,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                ExactCleanBypassAdmitted = exactCleanBypassAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryEBP is not uint entryEbp)
        {
            unreachedFrame = capture.FreshFrameMinusC.HasValue;
            unreachedEax = capture.EntryEAX.HasValue;
            lateEax = unreachedEax;
            unreachedEbx = capture.EntryEBX.HasValue;
            lateEbx = unreachedEbx;
            unreachedFresh = capture.FreshFieldAtEaxPlus8.HasValue;
            lateFresh = unreachedFresh;
            unreachedEdx = capture.EntryEDX.HasValue;
            lateEdx = unreachedEdx;
            return Finish(
                CapacityFinalCalleeAlternateSuccessorGateOutcome.MissingEntryEBP,
                FrameCompareRawFileOffset);
        }

        ebpCaptured = true;
        frameReadReached = true;
        frameAddress = DeriveEbpMinusCAddress(entryEbp);
        if (capture.FreshFrameMinusC is not uint freshFrameMinusC)
        {
            lateEax = capture.EntryEAX.HasValue;
            unreachedEax = lateEax;
            lateEbx = capture.EntryEBX.HasValue;
            unreachedEbx = lateEbx;
            lateFresh = capture.FreshFieldAtEaxPlus8.HasValue;
            unreachedFresh = lateFresh;
            lateEdx = capture.EntryEDX.HasValue;
            unreachedEdx = lateEdx;
            return Finish(
                CapacityFinalCalleeAlternateSuccessorGateOutcome.MissingFreshFrameMinusC,
                FrameCompareRawFileOffset);
        }

        frameCaptured = true;
        frameValue = freshFrameMinusC;
        if (capture.EntryEAX is not uint entryEax)
        {
            unreachedEbx = capture.EntryEBX.HasValue;
            lateEbx = unreachedEbx;
            unreachedFresh = capture.FreshFieldAtEaxPlus8.HasValue;
            lateFresh = unreachedFresh;
            unreachedEdx = capture.EntryEDX.HasValue;
            lateEdx = unreachedEdx;
            return Finish(
                CapacityFinalCalleeAlternateSuccessorGateOutcome.MissingEntryEAX,
                FrameCompareRawFileOffset);
        }

        eaxCaptured = true;
        frameCompareReached = true;
        frameCompareLeft = entryEax;
        frameCompareRight = freshFrameMinusC;
        frameCompareResult = unchecked(entryEax - freshFrameMinusC);
        frameCompareFlags = CalculateCompareFlags(
            entryEax, freshFrameMinusC, frameCompareResult.Value);
        frameJzReached = true;
        frameJzTaken = frameCompareFlags.Value.Zero;
        frameJzDestination = frameJzTaken.Value
            ? FrameEqualDestinationRawFileOffset
            : RegisterCompareRawFileOffset;

        if (frameJzTaken.Value)
        {
            unusedEbx = capture.EntryEBX.HasValue;
            unusedFresh = capture.FreshFieldAtEaxPlus8.HasValue;
            unusedEdx = capture.EntryEDX.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateSuccessorGateOutcome.EqualFrameValueToEntryEAX,
                FrameEqualDestinationRawFileOffset,
                exactCleanExitAdmitted: !unusedEbx && !unusedFresh && !unusedEdx,
                exactCleanBypassAdmitted: !unusedEbx && !unusedFresh && !unusedEdx);
        }

        if (capture.EntryEBX is not uint entryEbx)
        {
            lateFresh = capture.FreshFieldAtEaxPlus8.HasValue;
            unreachedFresh = lateFresh;
            lateEdx = capture.EntryEDX.HasValue;
            unreachedEdx = lateEdx;
            return Finish(
                CapacityFinalCalleeAlternateSuccessorGateOutcome.MissingEntryEBX,
                RegisterCompareRawFileOffset);
        }

        ebxCaptured = true;
        registerCompareReached = true;
        registerCompareLeft = entryEax;
        registerCompareRight = entryEbx;
        registerCompareResult = unchecked(entryEax - entryEbx);
        registerCompareFlags = CalculateCompareFlags(
            entryEax, entryEbx, registerCompareResult.Value);
        jbReached = true;
        jbTaken = registerCompareFlags.Value.Carry;
        jbDestination = jbTaken.Value
            ? BelowDestinationRawFileOffset
            : FieldCompareRawFileOffset;

        if (jbTaken.Value)
        {
            unusedFresh = capture.FreshFieldAtEaxPlus8.HasValue;
            unusedEdx = capture.EntryEDX.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateSuccessorGateOutcome.BelowToOpaqueCallBoundary,
                BelowDestinationRawFileOffset,
                exactCleanExitAdmitted: !unusedFresh && !unusedEdx);
        }

        freshReadReached = true;
        freshAddress = DeriveEaxPlus8Address(entryEax);
        if (capture.FreshFieldAtEaxPlus8 is uint freshField)
        {
            freshCaptured = true;
            freshValue = freshField;
        }

        if (capture.FreshFieldAtEaxPlus8 is not uint freshFieldAtAddress)
        {
            lateEdx = capture.EntryEDX.HasValue;
            unreachedEdx = lateEdx;
            return Finish(
                CapacityFinalCalleeAlternateSuccessorGateOutcome.MissingFreshFieldAtEaxPlus8,
                FieldCompareRawFileOffset);
        }

        if (capture.EntryEDX is not uint entryEdx)
        {
            return Finish(
                CapacityFinalCalleeAlternateSuccessorGateOutcome.MissingEntryEDX,
                FieldCompareRawFileOffset);
        }

        edxCaptured = true;
        fieldCompareReached = true;
        fieldCompareLeft = freshFieldAtAddress;
        fieldCompareRight = entryEdx;
        fieldCompareResult = unchecked(freshFieldAtAddress - entryEdx);
        fieldCompareFlags = CalculateCompareFlags(
            freshFieldAtAddress, entryEdx, fieldCompareResult.Value);
        fieldJzReached = true;
        fieldJzTaken = fieldCompareFlags.Value.Zero;
        fieldJzDestination = fieldJzTaken.Value
            ? FieldEqualDestinationRawFileOffset
            : FieldUnequalDestinationRawFileOffset;

        return Finish(
            fieldJzTaken.Value
                ? CapacityFinalCalleeAlternateSuccessorGateOutcome.EqualFreshFieldToEntryEDX
                : CapacityFinalCalleeAlternateSuccessorGateOutcome.UnequalFreshFieldToOpaqueCallBoundary,
            fieldJzDestination.Value,
            exactCleanExitAdmitted: true,
            exactCleanBypassAdmitted: fieldJzTaken.Value);
    }

    private static CapacityFinalCalleeAlternateSuccessorGateFlags CalculateCompareFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;

    private static CapacityFinalCalleeAlternateSuccessorGateInstructionEvidence Instruction(
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
