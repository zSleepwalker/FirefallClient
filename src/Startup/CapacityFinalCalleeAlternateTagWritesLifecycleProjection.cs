using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateTagWritesLifecycleCapture(
    CapacityFinalCalleeAlternateTagGateCapture TagGateCapture,
    uint? DownstreamEntryEDI,
    uint? DownstreamFreshEdxPlus4,
    uint? DownstreamEntryESI);

public enum CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
{
    TagGateIncomplete,
    TagGateIncompleteWithDownstreamCaptures,
    MaskedTagNotThreeTo8EF8,
    MaskedTagNotThreeWithDownstreamCaptures,
    AlternateFieldWritesIncomplete,
    CompletedAt9441
}

public sealed record CapacityFinalCalleeAlternateTagWritesLifecyclePlan(
    CapacityFinalCalleeAlternateTagWritesLifecycleCapture Capture,
    CapacityFinalCalleeAlternateTagGatePlan TagGate,
    CapacityFinalCalleeAlternateFieldWritesPlan? AlternateFieldWrites,
    bool AlternateFieldWritesReached,
    bool ExactTagThreeHandoffAdmitted,
    bool DownstreamEntryEDXDerived,
    uint? DownstreamEntryEDX,
    bool UnreachedDownstreamEntryEDICaptureProvided,
    bool UnreachedDownstreamFreshEdxPlus4CaptureProvided,
    bool UnreachedDownstreamEntryESICaptureProvided,
    CapacityFinalCalleeAlternateTagWritesLifecycleOutcome Outcome,
    bool Admitted,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Joins the clean tag-three gate at 90B0 to the independent alternate-field
/// captures. Other tags remain bounded at 8EF8.
/// </summary>
public static class CapacityFinalCalleeAlternateTagWritesLifecycleProjection
{
    public const uint EntryRawFileOffset =
        CapacityFinalCalleeAlternateTagGateProjection.EntryRawFileOffset;
    public const uint AlternateFieldWritesEntryRawFileOffset =
        CapacityFinalCalleeAlternateFieldWritesProjection.EntryRawFileOffset;
    public const uint AlternateDestinationRawFileOffset =
        CapacityFinalCalleeAlternateTagGateProjection.AlternateDestinationRawFileOffset;
    public const uint FinalTargetRawFileOffset =
        CapacityFinalCalleeAlternateFieldWritesProjection.JumpTargetRawFileOffset;
    public const uint TagThreeHandoffRawFileOffset =
        CapacityFinalCalleeAlternateTagGateProjection.BoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeAlternateTagWritesLifecyclePlan Project(
        CapacityFinalCalleeAlternateTagWritesLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateTagGatePlan gate =
            CapacityFinalCalleeAlternateTagGateProjection.Project(
                capture.TagGateCapture);
        bool downstreamEntryEdiProvided = capture.DownstreamEntryEDI.HasValue;
        bool downstreamFreshProvided = capture.DownstreamFreshEdxPlus4.HasValue;
        bool downstreamEntryEsiProvided = capture.DownstreamEntryESI.HasValue;
        bool anyDownstreamCaptureProvided =
            downstreamEntryEdiProvided
            || downstreamFreshProvided
            || downstreamEntryEsiProvided;

        if (IsExactCleanTagThree(gate))
        {
            CapacityFinalCalleeAlternateFieldWritesCapture downstreamCapture = new(
                EntryEDI: capture.DownstreamEntryEDI,
                EntryEDX: downstreamEntryEdiProvided
                    ? capture.TagGateCapture.EntryEDX
                    : null,
                FreshEdxPlus4: capture.DownstreamFreshEdxPlus4,
                EntryESI: capture.DownstreamEntryESI);
            CapacityFinalCalleeAlternateFieldWritesPlan fieldWrites =
                CapacityFinalCalleeAlternateFieldWritesProjection.Project(
                    downstreamCapture);
            bool entryEdxDerived = fieldWrites.EntryEDXCaptured;
            uint? derivedEntryEdx = entryEdxDerived
                ? downstreamCapture.EntryEDX
                : null;
            bool completed = IsExactCompletedFieldWrites(
                fieldWrites,
                capture.TagGateCapture.EntryEDX,
                capture.DownstreamFreshEdxPlus4);

            return new(
                Capture: capture,
                TagGate: gate,
                AlternateFieldWrites: fieldWrites,
                AlternateFieldWritesReached: true,
                ExactTagThreeHandoffAdmitted: true,
                DownstreamEntryEDXDerived: entryEdxDerived,
                DownstreamEntryEDX: derivedEntryEdx,
                UnreachedDownstreamEntryEDICaptureProvided: false,
                UnreachedDownstreamFreshEdxPlus4CaptureProvided:
                    fieldWrites.UnreachedFreshEdxPlus4CaptureProvided,
                UnreachedDownstreamEntryESICaptureProvided:
                    fieldWrites.UnreachedEntryESICaptureProvided,
                Outcome: completed
                    ? CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                        .CompletedAt9441
                    : CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                        .AlternateFieldWritesIncomplete,
                Admitted: completed,
                StopAtRawFileOffsetExclusive:
                    fieldWrites.StopAtRawFileOffsetExclusive);
        }

        bool cleanNotThree = IsExactCleanNotThree(gate);
        bool gateIncomplete = !cleanNotThree;
        bool contaminated = anyDownstreamCaptureProvided;
        bool blockedByCaptures = gateIncomplete || cleanNotThree;
        return new(
            capture,
            gate,
            AlternateFieldWrites: null,
            AlternateFieldWritesReached: false,
            ExactTagThreeHandoffAdmitted: false,
            DownstreamEntryEDXDerived: false,
            DownstreamEntryEDX: null,
            UnreachedDownstreamEntryEDICaptureProvided:
                blockedByCaptures && downstreamEntryEdiProvided,
            UnreachedDownstreamFreshEdxPlus4CaptureProvided:
                blockedByCaptures && downstreamFreshProvided,
            UnreachedDownstreamEntryESICaptureProvided:
                blockedByCaptures && downstreamEntryEsiProvided,
            Outcome: cleanNotThree
                ? contaminated
                    ? CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                        .MaskedTagNotThreeWithDownstreamCaptures
                    : CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                        .MaskedTagNotThreeTo8EF8
                : contaminated
                    ? CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                        .TagGateIncompleteWithDownstreamCaptures
                    : CapacityFinalCalleeAlternateTagWritesLifecycleOutcome
                        .TagGateIncomplete,
            Admitted: cleanNotThree && !contaminated,
            StopAtRawFileOffsetExclusive: gate.StopAtRawFileOffsetExclusive);
    }

    private static bool IsExactCleanTagThree(
        CapacityFinalCalleeAlternateTagGatePlan gate) =>
        gate.Outcome
            == CapacityFinalCalleeAlternateTagGateOutcome
                .MaskedTagThreeToBoundary
        && gate.Capture.EntryEDX.HasValue
        && gate.Capture.FreshDWORD.HasValue
        && gate.EntryEDXCaptured
        && gate.FreshDWORDReadReached
        && gate.FreshDWORDAddress
            == CapacityFinalCalleeAlternateTagGateProjection.DeriveFreshEdxPlus4Address(
                gate.Capture.EntryEDX.Value)
        && gate.FreshDWORDCaptured
        && gate.FreshDWORD == gate.Capture.FreshDWORD
        && gate.AndReached
        && gate.AndResult
            == (gate.Capture.FreshDWORD.Value
                & CapacityFinalCalleeAlternateTagGateProjection.TagMask)
        && gate.AndResult
            == CapacityFinalCalleeAlternateTagGateProjection.ComparedTag
        && gate.ByteCompareReached
        && gate.ByteCompareResult == 0
        && gate.ByteCompareFlags is
        {
            Carry: false,
            Parity: true,
            AuxiliaryCarry: false,
            Zero: true,
            Sign: false,
            Overflow: false
        }
        && gate.JnzReached
        && gate.JnzTaken == false
        && gate.JnzSelectedDestinationRawFileOffset
            == TagThreeHandoffRawFileOffset
        && gate.Reads.Length == 1
        && gate.Reads[0].RawFileOffset
            == CapacityFinalCalleeAlternateTagGateProjection.FreshReadRawFileOffset
        && gate.Reads[0].Address == gate.FreshDWORDAddress
        && gate.Reads[0].ValueCaptured
        && gate.Reads[0].Value == gate.Capture.FreshDWORD
        && !gate.UnreachedFreshDWORDCaptureProvided
        && gate.ExactCleanExitAdmitted
        && gate.StopAtRawFileOffsetExclusive == TagThreeHandoffRawFileOffset;

    private static bool IsExactCleanNotThree(
        CapacityFinalCalleeAlternateTagGatePlan gate) =>
        gate.Outcome
            == CapacityFinalCalleeAlternateTagGateOutcome
                .MaskedTagNotThreeToAlternateTarget
        && gate.Capture.EntryEDX.HasValue
        && gate.Capture.FreshDWORD.HasValue
        && gate.EntryEDXCaptured
        && gate.FreshDWORDReadReached
        && gate.FreshDWORDAddress
            == CapacityFinalCalleeAlternateTagGateProjection.DeriveFreshEdxPlus4Address(
                gate.Capture.EntryEDX.Value)
        && gate.FreshDWORDCaptured
        && gate.FreshDWORD == gate.Capture.FreshDWORD
        && gate.AndReached
        && gate.AndResult
            == (gate.Capture.FreshDWORD.Value
                & CapacityFinalCalleeAlternateTagGateProjection.TagMask)
        && gate.ByteCompareReached
        && gate.ByteCompareResult is byte result
        && result == CompareResult(
            (byte)(gate.Capture.FreshDWORD.Value
                & CapacityFinalCalleeAlternateTagGateProjection.TagMask),
            CapacityFinalCalleeAlternateTagGateProjection.ComparedTag)
        && result != 0
        && gate.ByteCompareFlags == CompareFlags(
            (byte)(gate.Capture.FreshDWORD.Value
                & CapacityFinalCalleeAlternateTagGateProjection.TagMask),
            CapacityFinalCalleeAlternateTagGateProjection.ComparedTag)
        && gate.JnzReached
        && gate.JnzTaken == true
        && gate.JnzSelectedDestinationRawFileOffset
            == AlternateDestinationRawFileOffset
        && gate.Reads.Length == 1
        && gate.Reads[0].RawFileOffset
            == CapacityFinalCalleeAlternateTagGateProjection.FreshReadRawFileOffset
        && gate.Reads[0].Address == gate.FreshDWORDAddress
        && gate.Reads[0].ValueCaptured
        && gate.Reads[0].Value == gate.Capture.FreshDWORD
        && !gate.UnreachedFreshDWORDCaptureProvided
        && gate.ExactCleanExitAdmitted
        && gate.StopAtRawFileOffsetExclusive == AlternateDestinationRawFileOffset;

    private static byte CompareResult(byte left, byte right) =>
        unchecked((byte)(left - right));

    private static CapacityFinalCalleeAlternateTagGateCompareFlags CompareFlags(
        byte left,
        byte right)
    {
        byte result = CompareResult(left, right);
        return new(
            left < right,
            (BitOperations.PopCount((uint)result) & 1) == 0,
            ((left ^ right ^ result) & 0x10) != 0,
            result == 0,
            (result & 0x80) != 0,
            ((left ^ right) & (left ^ result) & 0x80) != 0);
    }

    private static bool IsExactCompletedFieldWrites(
        CapacityFinalCalleeAlternateFieldWritesPlan fieldWrites,
        uint? gateEntryEdx,
        uint? downstreamFreshEdxPlus4) =>
        fieldWrites.Outcome
            == CapacityFinalCalleeAlternateFieldWritesOutcome.Completed
        && fieldWrites.Capture.EntryEDX == gateEntryEdx
        && fieldWrites.Capture.EntryEDI.HasValue
        && fieldWrites.EntryEDICaptured
        && fieldWrites.GlobalWriteReached
        && fieldWrites.EAXLoadedFromEDI
        && fieldWrites.EntryEDXCaptured
        && fieldWrites.FreshEdxPlus4Address
            == CapacityFinalCalleeAlternateFieldWritesProjection
                .DeriveEdxPlus4Address(gateEntryEdx!.Value)
        && fieldWrites.FreshEdxPlus4ReadReached
        && fieldWrites.FreshEdxPlus4Captured
        && fieldWrites.FreshEdxPlus4 == downstreamFreshEdxPlus4
        && fieldWrites.AndInstructionReached
        && fieldWrites.AndWriteReached
        && fieldWrites.OrInstructionReached
        && fieldWrites.EntryESICaptured
        && fieldWrites.EsiPlus4WriteReached
        && fieldWrites.EsiPlusEdiWriteReached
        && fieldWrites.JumpReached
        && fieldWrites.JumpTargetRawFileOffset
            == FinalTargetRawFileOffset
        && fieldWrites.JumpTargetVirtualAddress
            == CapacityFinalCalleeAlternateFieldWritesProjection
                .JumpTargetVirtualAddress
        && fieldWrites.Writes.Length == 4
        && !fieldWrites.UnreachedEntryEDXCaptureProvided
        && !fieldWrites.UnreachedFreshEdxPlus4CaptureProvided
        && !fieldWrites.UnreachedEntryESICaptureProvided
        && fieldWrites.ExactCleanExitAdmitted
        && fieldWrites.StopAtRawFileOffsetExclusive == FinalTargetRawFileOffset;
}
