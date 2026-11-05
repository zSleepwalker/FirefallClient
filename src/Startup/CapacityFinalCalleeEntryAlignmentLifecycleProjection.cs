namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeEntryAlignmentLifecycleCapture(
    CapacityFinalCalleeEntryValidationCapture EntryValidation,
    uint? AlignmentEntryEBP,
    uint? AlignmentEntryESP,
    uint? AlignmentEntryEsiDword,
    uint? AlignmentGlobalDispatchDword,
    uint? AlignmentCallCellTargetVirtualAddress,
    bool? AlignmentCallReturnedNormally);

public enum CapacityFinalCalleeEntryAlignmentLifecycleOutcome
{
    EntryValidationBlocked,
    ZeroEntryReturned,
    ValidationTerminalDestination,
    UnreachedAlignmentCaptureProvided,
    MissingIndependentAlignmentEntryRegisterCapture,
    AlignmentDispatchBlocked,
    AlignmentBranchDestination,
    ProjectionComplete
}

public readonly record struct CapacityFinalCalleeEntryAlignmentLifecyclePlan(
    CapacityFinalCalleeEntryAlignmentLifecycleCapture Capture,
    CapacityFinalCalleeEntryAlignmentLifecycleOutcome Outcome,
    CapacityFinalCalleeEntryValidationPlan EntryValidationPlan,
    CapacityFinalCalleeAlignmentDispatchPlan? AlignmentDispatchPlan,
    bool EntryValidationOutcomeClean,
    bool ZeroEntryReturned,
    bool ValidationTerminalDestinationSelected,
    uint? ValidationTerminalDestinationRawFileOffset,
    bool AlignmentDispatchReached,
    uint? DerivedAlignmentEntryEAX,
    uint? DerivedAlignmentEntryESI,
    uint? DerivedAlignmentEntryECX,
    bool AlignmentEntryEBPCaptureProvided,
    bool AlignmentEntryESPCaptureProvided,
    bool AlignmentBranchDestinationSelected,
    uint? AlignmentBranchDestinationRawFileOffset,
    bool UnreachedAlignmentEntryEsiDwordCaptureProvided,
    bool UnreachedAlignmentGlobalDispatchDwordCaptureProvided,
    bool UnreachedAlignmentCallCellTargetCaptureProvided,
    bool UnreachedAlignmentCallReturnStatusProvided,
    bool UnreachedAlignmentCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Follows only the exact clean validation fallthrough at raw 12C8DA7 into the alignment
/// window. EAX, ESI, and masked ECX come from validation; EBP and ESP remain independent
/// captures because the opaque prefix call does not establish their continuity.
/// </summary>
public static class CapacityFinalCalleeEntryAlignmentLifecycleProjection
{
    public const uint LifecycleStartRawFileOffset =
        CapacityFinalCalleeEntryValidationProjection.LifecycleStartRawFileOffset;
    public const uint AlignmentEntryRawFileOffset =
        CapacityFinalCalleeAlignmentDispatchProjection.EntryRawFileOffset;
    public const uint LifecycleEndRawFileOffsetExclusive =
        CapacityFinalCalleeAlignmentDispatchProjection.BoundaryRawFileOffsetExclusive;
    public const uint ZeroReturnBoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeEntryValidationProjection.ZeroReturnBoundaryRawFileOffsetExclusive;

    public static CapacityFinalCalleeEntryAlignmentLifecyclePlan Project(
        CapacityFinalCalleeEntryAlignmentLifecycleCapture capture)
    {
        CapacityFinalCalleeEntryValidationPlan entryValidationPlan =
            CapacityFinalCalleeEntryValidationProjection.Project(capture.EntryValidation);
        bool unreachedAlignmentEsiDword = capture.AlignmentEntryEsiDword.HasValue;
        bool unreachedAlignmentGlobal = capture.AlignmentGlobalDispatchDword.HasValue;
        bool unreachedAlignmentCallCell =
            capture.AlignmentCallCellTargetVirtualAddress.HasValue;
        bool unreachedAlignmentCallReturn = capture.AlignmentCallReturnedNormally.HasValue;
        bool anyAlignmentCapture =
            capture.AlignmentEntryEBP.HasValue
            || capture.AlignmentEntryESP.HasValue
            || unreachedAlignmentEsiDword
            || unreachedAlignmentGlobal
            || unreachedAlignmentCallCell
            || unreachedAlignmentCallReturn;

        if (IsCleanZeroEntryReturn(entryValidationPlan))
        {
            return BuildPlan(
                capture,
                anyAlignmentCapture
                    ? CapacityFinalCalleeEntryAlignmentLifecycleOutcome
                        .UnreachedAlignmentCaptureProvided
                    : CapacityFinalCalleeEntryAlignmentLifecycleOutcome.ZeroEntryReturned,
                entryValidationPlan,
                alignmentDispatchPlan: null,
                entryValidationOutcomeClean: true,
                zeroEntryReturned: true,
                validationTerminalDestinationSelected: false,
                validationTerminalDestinationRawFileOffset: null,
                alignmentDispatchReached: false,
                derivedAlignmentEntryEAX: null,
                derivedAlignmentEntryESI: null,
                derivedAlignmentEntryECX: null,
                unreachedAlignmentEntryEsiDword: unreachedAlignmentEsiDword,
                unreachedAlignmentGlobal: unreachedAlignmentGlobal,
                unreachedAlignmentCallCell: unreachedAlignmentCallCell,
                unreachedAlignmentCallReturn: unreachedAlignmentCallReturn,
                unreachedAlignmentCaptureProvided: anyAlignmentCapture,
                stopAt: ZeroReturnBoundaryRawFileOffsetExclusive);
        }

        if (!IsCleanEntryValidation(entryValidationPlan))
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeEntryAlignmentLifecycleOutcome.EntryValidationBlocked,
                entryValidationPlan,
                alignmentDispatchPlan: null,
                entryValidationOutcomeClean: false,
                zeroEntryReturned: false,
                validationTerminalDestinationSelected: false,
                validationTerminalDestinationRawFileOffset: null,
                alignmentDispatchReached: false,
                derivedAlignmentEntryEAX: null,
                derivedAlignmentEntryESI: null,
                derivedAlignmentEntryECX: null,
                unreachedAlignmentEntryEsiDword: unreachedAlignmentEsiDword,
                unreachedAlignmentGlobal: unreachedAlignmentGlobal,
                unreachedAlignmentCallCell: unreachedAlignmentCallCell,
                unreachedAlignmentCallReturn: unreachedAlignmentCallReturn,
                unreachedAlignmentCaptureProvided: anyAlignmentCapture,
                stopAt: entryValidationPlan.StopAtRawFileOffsetExclusive);
        }

        CapacityFinalCalleeFirstValidationPlan validationPlan =
            entryValidationPlan.ValidationLifecyclePlan!.Value.ValidationPlan;
        if (validationPlan.Outcome
            != CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne)
        {
            uint? terminalDestination = GetValidationTerminalDestination(validationPlan);
            bool terminalDestinationSelected = terminalDestination.HasValue;
            CapacityFinalCalleeEntryAlignmentLifecycleOutcome terminalOutcome =
                terminalDestinationSelected
                    ? CapacityFinalCalleeEntryAlignmentLifecycleOutcome
                        .ValidationTerminalDestination
                    : CapacityFinalCalleeEntryAlignmentLifecycleOutcome
                        .EntryValidationBlocked;
            if (anyAlignmentCapture && terminalDestinationSelected)
            {
                terminalOutcome = CapacityFinalCalleeEntryAlignmentLifecycleOutcome
                    .UnreachedAlignmentCaptureProvided;
            }

            return BuildPlan(
                capture,
                terminalOutcome,
                entryValidationPlan,
                alignmentDispatchPlan: null,
                entryValidationOutcomeClean: true,
                zeroEntryReturned: false,
                validationTerminalDestinationSelected: terminalDestinationSelected,
                validationTerminalDestinationRawFileOffset: terminalDestination,
                alignmentDispatchReached: false,
                derivedAlignmentEntryEAX: null,
                derivedAlignmentEntryESI: null,
                derivedAlignmentEntryECX: null,
                unreachedAlignmentEntryEsiDword: unreachedAlignmentEsiDword,
                unreachedAlignmentGlobal: unreachedAlignmentGlobal,
                unreachedAlignmentCallCell: unreachedAlignmentCallCell,
                unreachedAlignmentCallReturn: unreachedAlignmentCallReturn,
                unreachedAlignmentCaptureProvided: anyAlignmentCapture,
                stopAt: terminalDestination ?? validationPlan.StopAtRawFileOffsetExclusive);
        }

        if (!HasExactCleanValidationFallthrough(entryValidationPlan, validationPlan))
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeEntryAlignmentLifecycleOutcome.EntryValidationBlocked,
                entryValidationPlan,
                alignmentDispatchPlan: null,
                entryValidationOutcomeClean: false,
                zeroEntryReturned: false,
                validationTerminalDestinationSelected: false,
                validationTerminalDestinationRawFileOffset: null,
                alignmentDispatchReached: false,
                derivedAlignmentEntryEAX: null,
                derivedAlignmentEntryESI: null,
                derivedAlignmentEntryECX: null,
                unreachedAlignmentEntryEsiDword: unreachedAlignmentEsiDword,
                unreachedAlignmentGlobal: unreachedAlignmentGlobal,
                unreachedAlignmentCallCell: unreachedAlignmentCallCell,
                unreachedAlignmentCallReturn: unreachedAlignmentCallReturn,
                unreachedAlignmentCaptureProvided: anyAlignmentCapture,
                stopAt: validationPlan.StopAtRawFileOffsetExclusive);
        }

        if (capture.AlignmentEntryEBP is not uint entryEbp
            || capture.AlignmentEntryESP is not uint entryEsp)
        {
            bool laterAlignmentCaptureProvided =
                unreachedAlignmentEsiDword
                || unreachedAlignmentGlobal
                || unreachedAlignmentCallCell
                || unreachedAlignmentCallReturn;
            return BuildPlan(
                capture,
                CapacityFinalCalleeEntryAlignmentLifecycleOutcome
                    .MissingIndependentAlignmentEntryRegisterCapture,
                entryValidationPlan,
                alignmentDispatchPlan: null,
                entryValidationOutcomeClean: true,
                zeroEntryReturned: false,
                validationTerminalDestinationSelected: false,
                validationTerminalDestinationRawFileOffset: null,
                alignmentDispatchReached: false,
                derivedAlignmentEntryEAX: validationPlan.EaxAfterLoad,
                derivedAlignmentEntryESI: validationPlan.Capture.EntryESI,
                derivedAlignmentEntryECX: validationPlan.EcxAfterMask,
                unreachedAlignmentEntryEsiDword: laterAlignmentCaptureProvided
                    && unreachedAlignmentEsiDword,
                unreachedAlignmentGlobal: laterAlignmentCaptureProvided
                    && unreachedAlignmentGlobal,
                unreachedAlignmentCallCell: laterAlignmentCaptureProvided
                    && unreachedAlignmentCallCell,
                unreachedAlignmentCallReturn: laterAlignmentCaptureProvided
                    && unreachedAlignmentCallReturn,
                unreachedAlignmentCaptureProvided: laterAlignmentCaptureProvided,
                stopAt: AlignmentEntryRawFileOffset);
        }

        CapacityFinalCalleeAlignmentDispatchCapture alignmentCapture = new(
            CaptureValidatedEntry: true,
            EntryEAX: validationPlan.EaxAfterLoad!.Value,
            EntryESI: validationPlan.Capture.EntryESI,
            EntryECX: validationPlan.EcxAfterMask!.Value,
            EntryEBP: entryEbp,
            EntryESP: entryEsp,
            EntryEsiDword: capture.AlignmentEntryEsiDword,
            GlobalDispatchDword: capture.AlignmentGlobalDispatchDword,
            CallCellTargetVirtualAddress: capture.AlignmentCallCellTargetVirtualAddress,
            CallReturnedNormally: capture.AlignmentCallReturnedNormally);
        CapacityFinalCalleeAlignmentDispatchPlan alignmentDispatchPlan =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(alignmentCapture);
        bool dispatchComplete =
            alignmentDispatchPlan.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.CallReturnedNormally
            && alignmentDispatchPlan.StopAtRawFileOffsetExclusive
                == LifecycleEndRawFileOffsetExclusive
            && !alignmentDispatchPlan.UnreachedEntryEsiDwordCaptureProvided
            && !alignmentDispatchPlan.UnreachedGlobalDispatchDwordCaptureProvided
            && !alignmentDispatchPlan.UnreachedCallCellTargetCaptureProvided
            && !alignmentDispatchPlan.UnreachedCallReturnStatusProvided;
        uint? alignmentBranchDestination =
            GetCleanAlignmentBranchDestination(alignmentDispatchPlan);
        bool alignmentBranchReached = alignmentBranchDestination.HasValue;
        bool anyUnreachedAlignmentCapture =
            alignmentDispatchPlan.UnreachedEntryEsiDwordCaptureProvided
            || alignmentDispatchPlan.UnreachedGlobalDispatchDwordCaptureProvided
            || alignmentDispatchPlan.UnreachedCallCellTargetCaptureProvided
            || alignmentDispatchPlan.UnreachedCallReturnStatusProvided;

        return BuildPlan(
            capture,
            dispatchComplete
                ? CapacityFinalCalleeEntryAlignmentLifecycleOutcome.ProjectionComplete
                : alignmentBranchReached
                    ? CapacityFinalCalleeEntryAlignmentLifecycleOutcome
                        .AlignmentBranchDestination
                    : CapacityFinalCalleeEntryAlignmentLifecycleOutcome
                        .AlignmentDispatchBlocked,
            entryValidationPlan,
            alignmentDispatchPlan,
            entryValidationOutcomeClean: true,
            zeroEntryReturned: false,
            validationTerminalDestinationSelected: false,
            validationTerminalDestinationRawFileOffset: null,
            alignmentDispatchReached: true,
            derivedAlignmentEntryEAX: alignmentCapture.EntryEAX,
            derivedAlignmentEntryESI: alignmentCapture.EntryESI,
            derivedAlignmentEntryECX: alignmentCapture.EntryECX,
            unreachedAlignmentEntryEsiDword:
                alignmentDispatchPlan.UnreachedEntryEsiDwordCaptureProvided,
            unreachedAlignmentGlobal:
                alignmentDispatchPlan.UnreachedGlobalDispatchDwordCaptureProvided,
            unreachedAlignmentCallCell:
                alignmentDispatchPlan.UnreachedCallCellTargetCaptureProvided,
            unreachedAlignmentCallReturn:
                alignmentDispatchPlan.UnreachedCallReturnStatusProvided,
            unreachedAlignmentCaptureProvided: anyUnreachedAlignmentCapture,
            stopAt: alignmentDispatchPlan.StopAtRawFileOffsetExclusive,
            alignmentBranchDestinationSelected: alignmentBranchReached,
            alignmentBranchDestinationRawFileOffset: alignmentBranchDestination);
    }

    private static bool IsCleanZeroEntryReturn(
        CapacityFinalCalleeEntryValidationPlan plan) =>
        plan.Outcome == CapacityFinalCalleeEntryValidationOutcome.ZeroPathReturned
        && plan.ZeroEntryReturned
        && plan.PrefixLifecycleOutcomeClean
        && !plan.ValidationLifecycleReached
        && plan.ValidationLifecyclePlan is null
        && !plan.UnreachedValidationEntryAssertionProvided
        && !plan.UnreachedGlobalDwordCaptureProvided
        && !plan.UnreachedEsi4DwordCaptureProvided
        && !plan.UnreachedExpectedValidationEntryESIProvided
        && plan.StopAtRawFileOffsetExclusive
            == ZeroReturnBoundaryRawFileOffsetExclusive;

    private static bool IsCleanEntryValidation(
        CapacityFinalCalleeEntryValidationPlan plan)
    {
        if (plan.Outcome != CapacityFinalCalleeEntryValidationOutcome.ProjectionComplete
            || plan.ZeroEntryReturned
            || !plan.PrefixLifecycleOutcomeClean
            || !plan.ValidationLifecycleReached
            || !plan.DeriveValidationLifecyclePrefix
            || !plan.ValidationOutcomeClean
            || plan.UnreachedValidationEntryAssertionProvided
            || plan.UnreachedGlobalDwordCaptureProvided
            || plan.UnreachedEsi4DwordCaptureProvided
            || plan.UnreachedExpectedValidationEntryESIProvided
            || plan.ValidationLifecyclePlan is not
                CapacityFinalCalleeValidationLifecyclePlan lifecycle
            || lifecycle.Outcome
                != CapacityFinalCalleeValidationLifecycleOutcome.ProjectionComplete
            || !lifecycle.PrefixOutcomeClean
            || !lifecycle.DeriveValidatedEntry
            || lifecycle.UnreachedGlobalDwordCaptureProvided
            || lifecycle.UnreachedEsi4DwordCaptureProvided
            || lifecycle.ValidationPlan.UnreachedGlobalDwordCaptureProvided
            || lifecycle.ValidationPlan.UnreachedEsi4DwordCaptureProvided
            || lifecycle.ValidationPlan.Capture.CaptureValidatedEntry != true)
        {
            return false;
        }

        return (lifecycle.ValidationPlan.Outcome is
                CapacityFinalCalleeFirstValidationOutcome.EntryEsiUnsignedBelowGlobal
                or CapacityFinalCalleeFirstValidationOutcome.MaskEqualsOne
                or CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne)
            && lifecycle.ValidationPlan.StopAtRawFileOffsetExclusive
                == plan.StopAtRawFileOffsetExclusive;
    }

    private static bool HasExactCleanValidationFallthrough(
        CapacityFinalCalleeEntryValidationPlan entryValidationPlan,
        CapacityFinalCalleeFirstValidationPlan validationPlan) =>
        IsCleanEntryValidation(entryValidationPlan)
        && entryValidationPlan.ValidationLifecyclePlan is
            CapacityFinalCalleeValidationLifecyclePlan lifecycle
        && validationPlan.Outcome
            == CapacityFinalCalleeFirstValidationOutcome.MaskNotEqualOne
        && validationPlan.EqualOneBranchReached
        && validationPlan.EqualOneBranchTaken == false
        && validationPlan.FinalSelectedDestinationRawFileOffset
            == CapacityFinalCalleeFirstValidationProjection
                .MaskNotEqualOneFallthroughRawFileOffset
        && validationPlan.StopAtRawFileOffsetExclusive
            == AlignmentEntryRawFileOffset
        && validationPlan.Esi4ReadReached
        && validationPlan.Esi4DwordCaptured
        && validationPlan.Esi4Dword == validationPlan.EaxAfterLoad
        && validationPlan.EaxAfterLoad.HasValue
        && validationPlan.EcxAfterMove == validationPlan.EaxAfterLoad
        && validationPlan.EcxAfterMask == (validationPlan.EaxAfterLoad!.Value & 3)
        && validationPlan.Reads.Length == 2
        && validationPlan.Reads[0].RawFileOffset
            == CapacityFinalCalleeFirstValidationProjection.EntryRawFileOffset
        && validationPlan.Reads[0].Kind
            == CapacityFinalCalleeFirstValidationReadKind.GlobalDword
        && validationPlan.Reads[0].ValueCaptured
        && validationPlan.Reads[0].Value == validationPlan.GlobalDword
        && validationPlan.Reads[1].RawFileOffset
            == CapacityFinalCalleeFirstValidationProjection.MovEaxFromEsiPlusFourRawFileOffset
        && validationPlan.Reads[1].Kind
            == CapacityFinalCalleeFirstValidationReadKind.EsiPlusFourDword
        && validationPlan.Reads[1].Address
            == CapacityFinalCalleeFirstValidationProjection.EsiPlusFourAddress(
                validationPlan.Capture.EntryESI)
        && validationPlan.Reads[1].ValueCaptured
        && validationPlan.Reads[1].Value == validationPlan.EaxAfterLoad
        && validationPlan.Capture.EntryESI
            == lifecycle.ValidationPlan.Capture.EntryESI
        && !entryValidationPlan.UnreachedGlobalDwordCaptureProvided
        && !entryValidationPlan.UnreachedEsi4DwordCaptureProvided
        && !lifecycle.UnreachedGlobalDwordCaptureProvided
        && !lifecycle.UnreachedEsi4DwordCaptureProvided;

    private static uint? GetValidationTerminalDestination(
        CapacityFinalCalleeFirstValidationPlan validationPlan) =>
        validationPlan.Outcome switch
        {
            CapacityFinalCalleeFirstValidationOutcome.EntryEsiUnsignedBelowGlobal =>
                validationPlan.FirstSelectedDestinationRawFileOffset,
            CapacityFinalCalleeFirstValidationOutcome.MaskEqualsOne =>
                validationPlan.FinalSelectedDestinationRawFileOffset,
            _ => null
        };

    private static uint? GetCleanAlignmentBranchDestination(
        CapacityFinalCalleeAlignmentDispatchPlan plan)
    {
        if (plan.UnreachedEntryEsiDwordCaptureProvided
            || plan.UnreachedGlobalDispatchDwordCaptureProvided
            || plan.UnreachedCallCellTargetCaptureProvided
            || plan.UnreachedCallReturnStatusProvided)
        {
            return null;
        }

        return plan.Outcome switch
        {
            CapacityFinalCalleeAlignmentDispatchOutcome.EntryAlLowBitSet
                when plan.TestAlBranchReached
                    && plan.TestAlBranchTaken == true
                    && plan.TestAlSelectedDestinationRawFileOffset
                        == CapacityFinalCalleeAlignmentDispatchProjection
                            .EntryAlBranchDestinationRawFileOffset
                    && plan.TestAlSelectedDestinationVirtualAddress
                        == CapacityFinalCalleeAlignmentDispatchProjection
                            .EntryAlBranchDestinationVirtualAddress
                    && plan.StopAtRawFileOffsetExclusive
                        == CapacityFinalCalleeAlignmentDispatchProjection
                            .EntryAlBranchDestinationRawFileOffset =>
                CapacityFinalCalleeAlignmentDispatchProjection
                    .EntryAlBranchDestinationRawFileOffset,
            CapacityFinalCalleeAlignmentDispatchOutcome.EntryEcxNonzero
                when plan.TestEntryEcxBranchReached
                    && plan.TestEntryEcxBranchTaken == true
                    && plan.TestEntryEcxSelectedDestinationRawFileOffset
                        == CapacityFinalCalleeAlignmentDispatchProjection
                            .EntryEcxBranchDestinationRawFileOffset
                    && plan.TestEntryEcxSelectedDestinationVirtualAddress
                        == CapacityFinalCalleeAlignmentDispatchProjection
                            .EntryEcxBranchDestinationVirtualAddress
                    && plan.StopAtRawFileOffsetExclusive
                        == CapacityFinalCalleeAlignmentDispatchProjection
                            .EntryEcxBranchDestinationRawFileOffset =>
                CapacityFinalCalleeAlignmentDispatchProjection
                    .EntryEcxBranchDestinationRawFileOffset,
            CapacityFinalCalleeAlignmentDispatchOutcome.ComputedEcxZero
                when plan.TestComputedEcxBranchReached
                    && plan.TestComputedEcxBranchTaken == true
                    && plan.TestComputedEcxSelectedDestinationRawFileOffset
                        == CapacityFinalCalleeAlignmentDispatchProjection
                            .ComputedEcxZeroBranchDestinationRawFileOffset
                    && plan.TestComputedEcxSelectedDestinationVirtualAddress
                        == CapacityFinalCalleeAlignmentDispatchProjection
                            .ComputedEcxZeroBranchDestinationVirtualAddress
                    && plan.StopAtRawFileOffsetExclusive
                        == CapacityFinalCalleeAlignmentDispatchProjection
                            .ComputedEcxZeroBranchDestinationRawFileOffset =>
                CapacityFinalCalleeAlignmentDispatchProjection
                    .ComputedEcxZeroBranchDestinationRawFileOffset,
            _ => null
        };
    }

    private static CapacityFinalCalleeEntryAlignmentLifecyclePlan BuildPlan(
        CapacityFinalCalleeEntryAlignmentLifecycleCapture capture,
        CapacityFinalCalleeEntryAlignmentLifecycleOutcome outcome,
        CapacityFinalCalleeEntryValidationPlan entryValidationPlan,
        CapacityFinalCalleeAlignmentDispatchPlan? alignmentDispatchPlan,
        bool entryValidationOutcomeClean,
        bool zeroEntryReturned,
        bool validationTerminalDestinationSelected,
        uint? validationTerminalDestinationRawFileOffset,
        bool alignmentDispatchReached,
        uint? derivedAlignmentEntryEAX,
        uint? derivedAlignmentEntryESI,
        uint? derivedAlignmentEntryECX,
        bool unreachedAlignmentEntryEsiDword,
        bool unreachedAlignmentGlobal,
        bool unreachedAlignmentCallCell,
        bool unreachedAlignmentCallReturn,
        bool unreachedAlignmentCaptureProvided,
        uint stopAt,
        bool alignmentBranchDestinationSelected = false,
        uint? alignmentBranchDestinationRawFileOffset = null) =>
        new(
            capture,
            outcome,
            entryValidationPlan,
            alignmentDispatchPlan,
            entryValidationOutcomeClean,
            zeroEntryReturned,
            validationTerminalDestinationSelected,
            validationTerminalDestinationRawFileOffset,
            alignmentDispatchReached,
            derivedAlignmentEntryEAX,
            derivedAlignmentEntryESI,
            derivedAlignmentEntryECX,
            capture.AlignmentEntryEBP.HasValue,
            capture.AlignmentEntryESP.HasValue,
            alignmentBranchDestinationSelected,
            alignmentBranchDestinationRawFileOffset,
            unreachedAlignmentEntryEsiDword,
            unreachedAlignmentGlobal,
            unreachedAlignmentCallCell,
            unreachedAlignmentCallReturn,
            unreachedAlignmentCaptureProvided,
            stopAt);
}
