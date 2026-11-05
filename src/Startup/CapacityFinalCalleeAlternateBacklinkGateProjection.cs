using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateBacklinkGateCapture(
    uint? EntryECX,
    uint? EntryEBX,
    uint? EntryEDX,
    uint? FreshFieldAtEcxPlusC);

public enum CapacityFinalCalleeAlternateBacklinkGateOutcome
{
    MissingEntryECX,
    MissingEntryEBX,
    BelowToOpaqueCallBoundary,
    MissingEntryEDX,
    MissingFreshFieldAtEcxPlusC,
    EqualToBypassTarget,
    UnequalToOpaqueCallBoundary
}

public readonly record struct CapacityFinalCalleeAlternateBacklinkGateFlags(
    bool Carry,
    bool Parity,
    bool Auxiliary,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeAlternateBacklinkGateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeAlternateBacklinkGateWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public sealed record CapacityFinalCalleeAlternateBacklinkGatePlan(
    CapacityFinalCalleeAlternateBacklinkGateCapture Capture,
    CapacityFinalCalleeAlternateBacklinkGateOutcome Outcome)
{
    public bool EntryECXCaptured { get; init; }
    public bool EntryEBXCaptured { get; init; }
    public bool FirstCompareReached { get; init; }
    public uint? FirstCompareLeft { get; init; }
    public uint? FirstCompareRight { get; init; }
    public uint? FirstCompareResult { get; init; }
    public CapacityFinalCalleeAlternateBacklinkGateFlags? FirstCompareFlags { get; init; }
    public bool JbReached { get; init; }
    public bool? JbTaken { get; init; }
    public uint? JbSelectedDestinationRawFileOffset { get; init; }
    public bool EntryEDXReadReached { get; init; }
    public bool EntryEDXCaptured { get; init; }
    public bool FreshFieldReadReached { get; init; }
    public uint? FreshFieldAddress { get; init; }
    public bool FreshFieldCaptured { get; init; }
    public uint? FreshFieldValue { get; init; }
    public bool SecondCompareReached { get; init; }
    public uint? SecondCompareLeft { get; init; }
    public uint? SecondCompareRight { get; init; }
    public uint? SecondCompareResult { get; init; }
    public CapacityFinalCalleeAlternateBacklinkGateFlags? SecondCompareFlags { get; init; }
    public bool JzReached { get; init; }
    public bool? JzTaken { get; init; }
    public uint? JzSelectedDestinationRawFileOffset { get; init; }
    public bool UnreachedEntryEBXCaptureProvided { get; init; }
    public bool LateEntryEDXCaptureProvided { get; init; }
    public bool LateFreshFieldCaptureProvided { get; init; }
    public bool UnusedEntryEDXCaptureProvided { get; init; }
    public bool UnusedFreshFieldCaptureProvided { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool ExactCleanBypassAdmitted { get; init; }
    public ImmutableArray<CapacityFinalCalleeAlternateBacklinkGateWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeAlternateBacklinkGateWrite>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
    public bool OpaqueCallReached => false;
}

/// <summary>
/// Projects the parent-fresh CMP/JB followed by the independent fresh-field
/// CMP/JZ. Both exit at or bypass the opaque native-call boundary.
/// </summary>
public static class CapacityFinalCalleeAlternateBacklinkGateProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9151;
    public const uint EntryVirtualAddress = 0x016C9D51;
    public const uint FirstCompareRawFileOffset = 0x012C9151;
    public const uint JbRawFileOffset = 0x012C9153;
    public const uint SecondCompareRawFileOffset = 0x012C9155;
    public const uint JzRawFileOffset = 0x012C9158;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C915A;
    public const uint OpaqueCallRawFileOffset = 0x012C915A;
    public const uint OpaqueCallCellVirtualAddress = 0x01AC4A24;
    public const uint BelowDestinationRawFileOffset = 0x012C915A;
    public const uint UnequalDestinationRawFileOffset = 0x012C915A;
    public const uint EqualDestinationRawFileOffset = 0x012C9160;

    public const uint EcxPlusCDisplacement = 0x0C;
    public const string CapturedWindowBytes = "3B CB 72 05 39 51 0C 74 06";

    public static ImmutableArray<CapacityFinalCalleeAlternateBacklinkGateInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Compare entry ECX with entry EBX",
                FirstCompareRawFileOffset, "3B CB"),
            Instruction("Jump below to the opaque-call boundary",
                JbRawFileOffset, "72 05",
                branchTargetRawFileOffset: BelowDestinationRawFileOffset),
            Instruction("Compare the fresh DWORD at entry ECX+C with entry EDX",
                SecondCompareRawFileOffset, "39 51 0C"),
            Instruction("Jump equal to the exact bypass destination",
                JzRawFileOffset, "74 06",
                branchTargetRawFileOffset: EqualDestinationRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEcxPlusCAddress(uint entryEcx) =>
        unchecked(entryEcx + EcxPlusCDisplacement);

    public static CapacityFinalCalleeAlternateBacklinkGatePlan Project(
        CapacityFinalCalleeAlternateBacklinkGateCapture capture)
    {
        bool ecxCaptured = false;
        bool ebxCaptured = false;
        bool firstCompareReached = false;
        uint? firstCompareLeft = null;
        uint? firstCompareRight = null;
        uint? firstCompareResult = null;
        CapacityFinalCalleeAlternateBacklinkGateFlags? firstCompareFlags = null;
        bool jbReached = false;
        bool? jbTaken = null;
        uint? jbDestination = null;
        bool edxReadReached = false;
        bool edxCaptured = false;
        bool freshReadReached = false;
        uint? freshAddress = null;
        bool freshCaptured = false;
        uint? freshValue = null;
        bool secondCompareReached = false;
        uint? secondCompareLeft = null;
        uint? secondCompareRight = null;
        uint? secondCompareResult = null;
        CapacityFinalCalleeAlternateBacklinkGateFlags? secondCompareFlags = null;
        bool jzReached = false;
        bool? jzTaken = null;
        uint? jzDestination = null;
        bool unreachedEbx = false;
        bool lateEdx = false;
        bool lateFresh = false;
        bool unusedEdx = false;
        bool unusedFresh = false;

        CapacityFinalCalleeAlternateBacklinkGatePlan Finish(
            CapacityFinalCalleeAlternateBacklinkGateOutcome outcome,
            uint stopAt,
            bool exactCleanExitAdmitted = false,
            bool exactCleanBypassAdmitted = false) =>
            new(capture, outcome)
            {
                EntryECXCaptured = ecxCaptured,
                EntryEBXCaptured = ebxCaptured,
                FirstCompareReached = firstCompareReached,
                FirstCompareLeft = firstCompareLeft,
                FirstCompareRight = firstCompareRight,
                FirstCompareResult = firstCompareResult,
                FirstCompareFlags = firstCompareFlags,
                JbReached = jbReached,
                JbTaken = jbTaken,
                JbSelectedDestinationRawFileOffset = jbDestination,
                EntryEDXReadReached = edxReadReached,
                EntryEDXCaptured = edxCaptured,
                FreshFieldReadReached = freshReadReached,
                FreshFieldAddress = freshAddress,
                FreshFieldCaptured = freshCaptured,
                FreshFieldValue = freshValue,
                SecondCompareReached = secondCompareReached,
                SecondCompareLeft = secondCompareLeft,
                SecondCompareRight = secondCompareRight,
                SecondCompareResult = secondCompareResult,
                SecondCompareFlags = secondCompareFlags,
                JzReached = jzReached,
                JzTaken = jzTaken,
                JzSelectedDestinationRawFileOffset = jzDestination,
                UnreachedEntryEBXCaptureProvided = unreachedEbx,
                LateEntryEDXCaptureProvided = lateEdx,
                LateFreshFieldCaptureProvided = lateFresh,
                UnusedEntryEDXCaptureProvided = unusedEdx,
                UnusedFreshFieldCaptureProvided = unusedFresh,
                ExactCleanExitAdmitted = exactCleanExitAdmitted,
                ExactCleanBypassAdmitted = exactCleanBypassAdmitted,
                StopAtRawFileOffsetExclusive = stopAt
            };

        if (capture.EntryECX is not uint entryEcx)
        {
            unreachedEbx = capture.EntryEBX.HasValue;
            lateEdx = capture.EntryEDX.HasValue;
            lateFresh = capture.FreshFieldAtEcxPlusC.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingEntryECX,
                FirstCompareRawFileOffset);
        }

        ecxCaptured = true;
        if (capture.EntryEBX is not uint entryEbx)
        {
            lateEdx = capture.EntryEDX.HasValue;
            lateFresh = capture.FreshFieldAtEcxPlusC.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingEntryEBX,
                FirstCompareRawFileOffset);
        }

        ebxCaptured = true;
        firstCompareReached = true;
        firstCompareLeft = entryEcx;
        firstCompareRight = entryEbx;
        firstCompareResult = unchecked(entryEcx - entryEbx);
        firstCompareFlags = CalculateCompareFlags(
            entryEcx, entryEbx, firstCompareResult.Value);
        jbReached = true;
        jbTaken = firstCompareFlags.Value.Carry;
        jbDestination = jbTaken.Value
            ? BelowDestinationRawFileOffset
            : SecondCompareRawFileOffset;

        if (jbTaken.Value)
        {
            unusedEdx = capture.EntryEDX.HasValue;
            unusedFresh = capture.FreshFieldAtEcxPlusC.HasValue;
            return Finish(
                CapacityFinalCalleeAlternateBacklinkGateOutcome.BelowToOpaqueCallBoundary,
                BelowDestinationRawFileOffset,
                exactCleanExitAdmitted: !unusedEdx && !unusedFresh);
        }

        edxReadReached = true;
        edxCaptured = capture.EntryEDX.HasValue;
        freshReadReached = true;
        freshAddress = DeriveEcxPlusCAddress(entryEcx);
        if (capture.FreshFieldAtEcxPlusC is uint freshField)
        {
            freshCaptured = true;
            freshValue = freshField;
        }

        if (capture.EntryEDX is not uint entryEdx)
        {
            return Finish(
                CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingEntryEDX,
                SecondCompareRawFileOffset);
        }

        if (capture.FreshFieldAtEcxPlusC is not uint freshValueAtAddress)
        {
            return Finish(
                CapacityFinalCalleeAlternateBacklinkGateOutcome.MissingFreshFieldAtEcxPlusC,
                SecondCompareRawFileOffset);
        }

        secondCompareReached = true;
        secondCompareLeft = freshValueAtAddress;
        secondCompareRight = entryEdx;
        secondCompareResult = unchecked(freshValueAtAddress - entryEdx);
        secondCompareFlags = CalculateCompareFlags(
            freshValueAtAddress, entryEdx, secondCompareResult.Value);
        jzReached = true;
        jzTaken = secondCompareFlags.Value.Zero;
        jzDestination = jzTaken.Value
            ? EqualDestinationRawFileOffset
            : UnequalDestinationRawFileOffset;

        return Finish(
            jzTaken.Value
                ? CapacityFinalCalleeAlternateBacklinkGateOutcome.EqualToBypassTarget
                : CapacityFinalCalleeAlternateBacklinkGateOutcome.UnequalToOpaqueCallBoundary,
            jzDestination.Value,
            exactCleanExitAdmitted: true,
            exactCleanBypassAdmitted: jzTaken.Value);
    }

    private static CapacityFinalCalleeAlternateBacklinkGateFlags CalculateCompareFlags(
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

    private static CapacityFinalCalleeAlternateBacklinkGateInstructionEvidence Instruction(
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
