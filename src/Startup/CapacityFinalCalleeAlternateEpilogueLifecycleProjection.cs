using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateEpilogueLifecycleCapture(
    CapacityFinalCalleeAlternateFieldWritesCapture AlternateFieldWritesCapture,
    CapacityFinalCalleeGatedEpilogueCapture GatedEpilogueCapture);

public enum CapacityFinalCalleeAlternateEpilogueLifecycleOutcome
{
    AlternateFieldWritesIncomplete,
    AlternateFieldWritesIncompleteWithEpilogueCaptures,
    EpilogueEntryNotValidated,
    EpilogueIncomplete,
    EpilogueContaminated,
    CompletedAt945C
}

public sealed record CapacityFinalCalleeAlternateEpilogueLifecyclePlan(
    CapacityFinalCalleeAlternateEpilogueLifecycleCapture Capture,
    CapacityFinalCalleeAlternateFieldWritesPlan AlternateFieldWrites,
    CapacityFinalCalleeGatedEpiloguePlan? GatedEpilogue,
    bool GatedEpilogueReached,
    bool ExactJmp9441HandoffAdmitted,
    bool UnreachedEntryValidatedClaimProvided,
    bool UnreachedTestedByteCaptureProvided,
    bool UnreachedStackPopEdiCaptureProvided,
    bool UnreachedStackPopEsiCaptureProvided,
    bool UnreachedStackPopEbxCaptureProvided,
    bool UnreachedCallCellTargetCaptureProvided,
    bool UnreachedCallReturnStatusProvided,
    bool UnreachedPostCallEbpCaptureProvided,
    bool UnreachedStackPopEbpCaptureProvided,
    bool UnreachedReturnTargetCaptureProvided,
    CapacityFinalCalleeAlternateEpilogueLifecycleOutcome Outcome,
    bool ExactCleanExitAdmitted,
    uint StopAtRawFileOffsetExclusive)
{
    public ImmutableArray<CapacityFinalCalleeAlternateFieldWritesWrite> OrderedUpstreamWrites =>
        AlternateFieldWrites.Writes;

    public ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess> EpilogueStackAccesses =>
        GatedEpilogue?.StackAccesses
        ?? ImmutableArray<CapacityFinalCalleeGatedEpilogueStackAccess>.Empty;
}

/// <summary>
/// Joins the alternate-field writes through JMP 9441 to the independently captured
/// gated epilogue. The epilogue's entry ESP and EBP are never derived from those writes.
/// </summary>
public static class CapacityFinalCalleeAlternateEpilogueLifecycleProjection
{
    public const uint AlternateFieldWritesEntryRawFileOffset =
        CapacityFinalCalleeAlternateFieldWritesProjection.EntryRawFileOffset;
    public const uint GatedEpilogueEntryRawFileOffset =
        CapacityFinalCalleeGatedEpilogueProjection.EntryRawFileOffset;
    public const uint FinalReturnBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeGatedEpilogueProjection.ReturnBoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeAlternateEpilogueLifecyclePlan Project(
        CapacityFinalCalleeAlternateEpilogueLifecycleCapture capture)
    {
        CapacityFinalCalleeAlternateFieldWritesPlan alternateFieldWrites =
            CapacityFinalCalleeAlternateFieldWritesProjection.Project(
                capture.AlternateFieldWritesCapture);
        CapacityFinalCalleeGatedEpilogueCapture epilogueCapture =
            capture.GatedEpilogueCapture;

        if (!IsExactJmp9441Handoff(
                alternateFieldWrites,
                capture.AlternateFieldWritesCapture))
        {
            bool anyEpilogueCaptureProvided =
                epilogueCapture.EntryValidated
                || HasEpilogueCaptures(epilogueCapture);
            return new(
                capture,
                alternateFieldWrites,
                GatedEpilogue: null,
                GatedEpilogueReached: false,
                ExactJmp9441HandoffAdmitted: false,
                UnreachedEntryValidatedClaimProvided: epilogueCapture.EntryValidated,
                UnreachedTestedByteCaptureProvided:
                    epilogueCapture.TestedByte.HasValue,
                UnreachedStackPopEdiCaptureProvided:
                    epilogueCapture.StackPopEdiValue.HasValue,
                UnreachedStackPopEsiCaptureProvided:
                    epilogueCapture.StackPopEsiValue.HasValue,
                UnreachedStackPopEbxCaptureProvided:
                    epilogueCapture.StackPopEbxValue.HasValue,
                UnreachedCallCellTargetCaptureProvided:
                    epilogueCapture.CallCellTargetVirtualAddress.HasValue,
                UnreachedCallReturnStatusProvided:
                    epilogueCapture.CallReturnedNormally.HasValue,
                UnreachedPostCallEbpCaptureProvided:
                    epilogueCapture.PostCallEbp.HasValue,
                UnreachedStackPopEbpCaptureProvided:
                    epilogueCapture.StackPopEbpValue.HasValue,
                UnreachedReturnTargetCaptureProvided:
                    epilogueCapture.ReturnTargetVirtualAddress.HasValue,
                Outcome: anyEpilogueCaptureProvided
                    ? CapacityFinalCalleeAlternateEpilogueLifecycleOutcome
                        .AlternateFieldWritesIncompleteWithEpilogueCaptures
                    : CapacityFinalCalleeAlternateEpilogueLifecycleOutcome
                        .AlternateFieldWritesIncomplete,
                ExactCleanExitAdmitted: false,
                StopAtRawFileOffsetExclusive:
                    alternateFieldWrites.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeGatedEpiloguePlan gatedEpilogue =
            CapacityFinalCalleeGatedEpilogueProjection.Project(
                epilogueCapture with
                {
                    EntryValidated =
                        epilogueCapture.EntryValidated
                        && alternateFieldWrites.ExactCleanExitAdmitted
                });
        bool exactCleanExitAdmitted = IsExactCleanExit(gatedEpilogue);
        bool hasUnreachedEpilogueCapture =
            gatedEpilogue.UnreachedTestedByteCaptureProvided
            || gatedEpilogue.UnreachedStackPopEdiCaptureProvided
            || gatedEpilogue.UnreachedStackPopEsiCaptureProvided
            || gatedEpilogue.UnreachedStackPopEbxCaptureProvided
            || gatedEpilogue.UnreachedCallCellTargetCaptureProvided
            || gatedEpilogue.UnreachedCallReturnStatusProvided
            || gatedEpilogue.UnreachedPostCallEbpCaptureProvided
            || gatedEpilogue.UnreachedStackPopEbpCaptureProvided
            || gatedEpilogue.UnreachedReturnTargetCaptureProvided;

        CapacityFinalCalleeAlternateEpilogueLifecycleOutcome outcome =
            exactCleanExitAdmitted
                ? CapacityFinalCalleeAlternateEpilogueLifecycleOutcome.CompletedAt945C
                : gatedEpilogue.Outcome
                    == CapacityFinalCalleeGatedEpilogueOutcome.EntryNotValidated
                    ? CapacityFinalCalleeAlternateEpilogueLifecycleOutcome
                        .EpilogueEntryNotValidated
                    : hasUnreachedEpilogueCapture
                        ? CapacityFinalCalleeAlternateEpilogueLifecycleOutcome
                            .EpilogueContaminated
                        : CapacityFinalCalleeAlternateEpilogueLifecycleOutcome
                            .EpilogueIncomplete;

        return new(
            capture,
            alternateFieldWrites,
            gatedEpilogue,
            GatedEpilogueReached: true,
            ExactJmp9441HandoffAdmitted: true,
            UnreachedEntryValidatedClaimProvided: false,
            UnreachedTestedByteCaptureProvided:
                gatedEpilogue.UnreachedTestedByteCaptureProvided,
            UnreachedStackPopEdiCaptureProvided:
                gatedEpilogue.UnreachedStackPopEdiCaptureProvided,
            UnreachedStackPopEsiCaptureProvided:
                gatedEpilogue.UnreachedStackPopEsiCaptureProvided,
            UnreachedStackPopEbxCaptureProvided:
                gatedEpilogue.UnreachedStackPopEbxCaptureProvided,
            UnreachedCallCellTargetCaptureProvided:
                gatedEpilogue.UnreachedCallCellTargetCaptureProvided,
            UnreachedCallReturnStatusProvided:
                gatedEpilogue.UnreachedCallReturnStatusProvided,
            UnreachedPostCallEbpCaptureProvided:
                gatedEpilogue.UnreachedPostCallEbpCaptureProvided,
            UnreachedStackPopEbpCaptureProvided:
                gatedEpilogue.UnreachedStackPopEbpCaptureProvided,
            UnreachedReturnTargetCaptureProvided:
                gatedEpilogue.UnreachedReturnTargetCaptureProvided,
            outcome,
            exactCleanExitAdmitted,
            gatedEpilogue.StopAtRawFileOffsetExclusive);
    }

    private static bool HasEpilogueCaptures(
        CapacityFinalCalleeGatedEpilogueCapture capture) =>
        capture.TestedByte.HasValue
        || capture.StackPopEdiValue.HasValue
        || capture.StackPopEsiValue.HasValue
        || capture.StackPopEbxValue.HasValue
        || capture.CallCellTargetVirtualAddress.HasValue
        || capture.CallReturnedNormally.HasValue
        || capture.PostCallEbp.HasValue
        || capture.StackPopEbpValue.HasValue
        || capture.ReturnTargetVirtualAddress.HasValue;

    private static bool IsExactJmp9441Handoff(
        CapacityFinalCalleeAlternateFieldWritesPlan plan,
        CapacityFinalCalleeAlternateFieldWritesCapture capture)
    {
        if (plan.Outcome
                != CapacityFinalCalleeAlternateFieldWritesOutcome.Completed
            || !plan.ExactCleanExitAdmitted
            || plan.StopAtRawFileOffsetExclusive
                != GatedEpilogueEntryRawFileOffset
            || !plan.JumpReached
            || plan.JumpTargetRawFileOffset
                != GatedEpilogueEntryRawFileOffset
            || plan.JumpTargetVirtualAddress
                != CapacityFinalCalleeGatedEpilogueProjection.EntryVirtualAddress
            || capture.EntryEDI is not uint entryEdi
            || capture.EntryEDX is not uint entryEdx
            || capture.FreshEdxPlus4 is not uint freshEdxPlus4
            || capture.EntryESI is not uint entryEsi
            || !plan.EntryEDICaptured
            || !plan.GlobalWriteReached
            || plan.GlobalWriteAddress
                != CapacityFinalCalleeAlternateFieldWritesProjection
                    .GlobalWritePeVirtualAddress
            || plan.GlobalWriteValue != entryEdi
            || !plan.EAXLoadedFromEDI
            || plan.EAXRetained != entryEdi
            || !plan.EntryEDXCaptured
            || plan.FreshEdxPlus4Address
                != CapacityFinalCalleeAlternateFieldWritesProjection
                    .DeriveEdxPlus4Address(entryEdx)
            || !plan.FreshEdxPlus4ReadReached
            || !plan.FreshEdxPlus4Captured
            || plan.FreshEdxPlus4 != freshEdxPlus4
            || !plan.AndInstructionReached
            || !plan.AndWriteReached
            || plan.AndWriteAddress != plan.FreshEdxPlus4Address
            || plan.AndWriteValue
                != CapacityFinalCalleeAlternateFieldWritesProjection
                    .ApplyAndMask(freshEdxPlus4)
            || !plan.OrInstructionReached
            || plan.EAXAfterOr
                != CapacityFinalCalleeAlternateFieldWritesProjection
                    .ApplyOrMask(entryEdi)
            || !plan.EntryESICaptured
            || !plan.EsiPlus4WriteReached
            || plan.EsiPlus4WriteAddress
                != CapacityFinalCalleeAlternateFieldWritesProjection
                    .DeriveEsiPlus4Address(entryEsi)
            || plan.EsiPlus4WriteValue != plan.EAXAfterOr
            || !plan.EsiPlusEdiWriteReached
            || plan.EsiPlusEdiWriteAddress
                != CapacityFinalCalleeAlternateFieldWritesProjection
                    .DeriveEsiPlusEdiAddress(entryEsi, entryEdi)
            || plan.EsiPlusEdiWriteValue != entryEdi
            || plan.UnreachedEntryEDXCaptureProvided
            || plan.UnreachedFreshEdxPlus4CaptureProvided
            || plan.UnreachedEntryESICaptureProvided
            || plan.Writes.Length != 4)
        {
            return false;
        }

        return plan.Writes.SequenceEqual(
            ImmutableArray.Create(
                new CapacityFinalCalleeAlternateFieldWritesWrite(
                    CapacityFinalCalleeAlternateFieldWritesProjection
                        .GlobalWriteRawFileOffset,
                    CapacityFinalCalleeAlternateFieldWritesProjection
                        .GlobalWritePeVirtualAddress,
                    entryEdi),
                new CapacityFinalCalleeAlternateFieldWritesWrite(
                    CapacityFinalCalleeAlternateFieldWritesProjection
                        .AndEdxPlus4RawFileOffset,
                    CapacityFinalCalleeAlternateFieldWritesProjection
                        .DeriveEdxPlus4Address(entryEdx),
                    CapacityFinalCalleeAlternateFieldWritesProjection
                        .ApplyAndMask(freshEdxPlus4)),
                new CapacityFinalCalleeAlternateFieldWritesWrite(
                    CapacityFinalCalleeAlternateFieldWritesProjection
                        .EsiPlus4WriteRawFileOffset,
                    CapacityFinalCalleeAlternateFieldWritesProjection
                        .DeriveEsiPlus4Address(entryEsi),
                    CapacityFinalCalleeAlternateFieldWritesProjection
                        .ApplyOrMask(entryEdi)),
                new CapacityFinalCalleeAlternateFieldWritesWrite(
                    CapacityFinalCalleeAlternateFieldWritesProjection
                        .EsiPlusEdiWriteRawFileOffset,
                    CapacityFinalCalleeAlternateFieldWritesProjection
                        .DeriveEsiPlusEdiAddress(entryEsi, entryEdi),
                    entryEdi)));
    }

    private static bool IsExactCleanExit(
        CapacityFinalCalleeGatedEpiloguePlan plan) =>
        plan.ReturnReached
        && plan.ExactCleanExitAdmitted
        && plan.StopAtRawFileOffsetExclusive
            == FinalReturnBoundaryRawFileOffsetExclusive
        && plan.Outcome is
            CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
                or CapacityFinalCalleeGatedEpilogueOutcome.ReturnedNormally;
}
