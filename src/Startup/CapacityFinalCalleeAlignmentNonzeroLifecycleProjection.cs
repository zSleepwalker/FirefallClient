namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlignmentNonzeroLifecycleCapture(
    CapacityFinalCalleeAlignmentDispatchCapture AlignmentDispatch,
    uint? NonzeroEntryEBX,
    uint? NonzeroGlobalDword,
    uint? NonzeroEsiPlus8Dword,
    uint? NonzeroEsiPlusCDword);

public enum CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
{
    AlignmentDispatchBlocked,
    AlignmentDispatchContaminated,
    UnreachedNonzeroCaptureProvided,
    AlignmentEntryAlLowBitSetDestination,
    AlignmentComputedEcxZeroDestination,
    AlignmentCallReturnedNormally,
    MissingIndependentNonzeroEntryEBX,
    NonzeroValidationBlocked,
    NonzeroEntryEsiUnsignedBelowEntryEbxDestination,
    NonzeroEntryEsiEqualsGlobalDwordDestination,
    NonzeroShiftedEaxUnsignedAboveOrEqual32Destination,
    NonzeroEntryEsiNotEqualEsiPlusCDwordDestination,
    NonzeroEntryEsiEqualsEsiPlusCDwordDestination
}

public readonly record struct CapacityFinalCalleeAlignmentNonzeroLifecyclePlan(
    CapacityFinalCalleeAlignmentNonzeroLifecycleCapture Capture,
    CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome Outcome,
    CapacityFinalCalleeAlignmentDispatchPlan AlignmentDispatchPlan,
    CapacityFinalCalleeNonzeroValidationPlan? NonzeroValidationPlan,
    bool AlignmentDispatchOutcomeClean,
    bool ExactEntryEcxNonzeroBranch,
    bool NonzeroValidationReached,
    uint? DerivedNonzeroEntryEAX,
    uint? DerivedNonzeroEntryEDI,
    uint? DerivedNonzeroEntryESI,
    uint? DerivedNonzeroEntryEBP,
    bool NonzeroEntryEBXCaptureProvided,
    uint? NonzeroEntryEBX,
    bool AlignmentDestinationSelected,
    uint? AlignmentDestinationRawFileOffset,
    bool NonzeroDestinationSelected,
    uint? NonzeroDestinationRawFileOffset,
    bool UnreachedAlignmentEntryEsiDwordCaptureProvided,
    bool UnreachedAlignmentGlobalDispatchDwordCaptureProvided,
    bool UnreachedAlignmentCallCellTargetCaptureProvided,
    bool UnreachedAlignmentCallReturnStatusProvided,
    bool UnreachedAlignmentCaptureProvided,
    bool UnreachedNonzeroEntryEBXCaptureProvided,
    bool UnreachedNonzeroGlobalDwordCaptureProvided,
    bool UnreachedNonzeroEsiPlus8DwordCaptureProvided,
    bool UnreachedNonzeroEsiPlusCDwordCaptureProvided,
    bool UnreachedNonzeroCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Follows only the clean alignment dispatch's exact nonzero-ECX branch into the
/// nonzero validation window. The validation registers are derived from the completed
/// alignment load/subtraction; EBX and its three memory reads remain independent captures.
/// </summary>
public static class CapacityFinalCalleeAlignmentNonzeroLifecycleProjection
{
    public const uint NonzeroEntryRawFileOffset =
        CapacityFinalCalleeNonzeroValidationProjection.EntryRawFileOffset;

    public static CapacityFinalCalleeAlignmentNonzeroLifecyclePlan Project(
        CapacityFinalCalleeAlignmentNonzeroLifecycleCapture capture)
    {
        CapacityFinalCalleeAlignmentDispatchPlan alignmentPlan =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(
                capture.AlignmentDispatch);
        bool unreachedAlignmentEntryEsi =
            alignmentPlan.UnreachedEntryEsiDwordCaptureProvided;
        bool unreachedAlignmentGlobal =
            alignmentPlan.UnreachedGlobalDispatchDwordCaptureProvided;
        bool unreachedAlignmentCallCell =
            alignmentPlan.UnreachedCallCellTargetCaptureProvided;
        bool unreachedAlignmentCallReturn =
            alignmentPlan.UnreachedCallReturnStatusProvided;
        bool unreachedAlignment =
            unreachedAlignmentEntryEsi
            || unreachedAlignmentGlobal
            || unreachedAlignmentCallCell
            || unreachedAlignmentCallReturn;

        AlignmentRoute route = GetCleanAlignmentRoute(alignmentPlan);
        bool alignmentRouteWasSelected =
            route != AlignmentRoute.None
            && HasSelectedAlignmentDestination(alignmentPlan, route);
        uint? alignmentDestination = alignmentRouteWasSelected
            ? GetAlignmentDestination(alignmentPlan, route)
            : null;

        if (!alignmentRouteWasSelected)
        {
            return BuildPlan(
                capture,
                alignmentPlan.Outcome
                        is CapacityFinalCalleeAlignmentDispatchOutcome.EntryAlLowBitSet
                            or CapacityFinalCalleeAlignmentDispatchOutcome.EntryEcxNonzero
                            or CapacityFinalCalleeAlignmentDispatchOutcome.ComputedEcxZero
                            or CapacityFinalCalleeAlignmentDispatchOutcome.CallReturnedNormally
                    ? CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                        .AlignmentDispatchContaminated
                    : CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                        .AlignmentDispatchBlocked,
                alignmentPlan,
                nonzeroValidationPlan: null,
                alignmentDispatchOutcomeClean: false,
                exactEntryEcxNonzeroBranch: false,
                nonzeroValidationReached: false,
                derivedNonzeroEntryEAX: null,
                derivedNonzeroEntryEDI: null,
                derivedNonzeroEntryESI: null,
                derivedNonzeroEntryEBP: null,
                alignmentDestinationSelected: false,
                alignmentDestinationRawFileOffset: null,
                nonzeroDestinationSelected: false,
                nonzeroDestinationRawFileOffset: null,
                unreachedAlignmentEntryEsi,
                unreachedAlignmentGlobal,
                unreachedAlignmentCallCell,
                unreachedAlignmentCallReturn,
                unreachedAlignment,
                unreachedNonzeroEntryEBX:
                    capture.NonzeroEntryEBX.HasValue,
                unreachedNonzeroGlobal:
                    capture.NonzeroGlobalDword.HasValue,
                unreachedNonzeroEsiPlus8:
                    capture.NonzeroEsiPlus8Dword.HasValue,
                unreachedNonzeroEsiPlusC:
                    capture.NonzeroEsiPlusCDword.HasValue,
                stopAt: alignmentPlan.StopAtRawFileOffsetExclusive);
        }

        bool exactEntryEcxNonzero =
            route == AlignmentRoute.EntryEcxNonzero;
        if (!exactEntryEcxNonzero)
        {
            bool nonzeroCaptureProvided = AnyNonzeroCaptureProvided(capture);
            return BuildPlan(
                capture,
                nonzeroCaptureProvided
                    ? CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                        .UnreachedNonzeroCaptureProvided
                    : route switch
                    {
                        AlignmentRoute.EntryAlLowBitSet =>
                            CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                                .AlignmentEntryAlLowBitSetDestination,
                        AlignmentRoute.ComputedEcxZero =>
                            CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                                .AlignmentComputedEcxZeroDestination,
                        AlignmentRoute.CallReturnedNormally =>
                            CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                                .AlignmentCallReturnedNormally,
                        _ => CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                            .AlignmentDispatchBlocked
                    },
                alignmentPlan,
                nonzeroValidationPlan: null,
                alignmentDispatchOutcomeClean: true,
                exactEntryEcxNonzeroBranch: false,
                nonzeroValidationReached: false,
                derivedNonzeroEntryEAX: null,
                derivedNonzeroEntryEDI: null,
                derivedNonzeroEntryESI: null,
                derivedNonzeroEntryEBP: null,
                alignmentDestinationSelected: true,
                alignmentDestinationRawFileOffset: alignmentDestination,
                nonzeroDestinationSelected: false,
                nonzeroDestinationRawFileOffset: null,
                unreachedAlignmentEntryEsi,
                unreachedAlignmentGlobal,
                unreachedAlignmentCallCell,
                unreachedAlignmentCallReturn,
                unreachedAlignment,
                unreachedNonzeroEntryEBX:
                    capture.NonzeroEntryEBX.HasValue,
                unreachedNonzeroGlobal:
                    capture.NonzeroGlobalDword.HasValue,
                unreachedNonzeroEsiPlus8:
                    capture.NonzeroEsiPlus8Dword.HasValue,
                unreachedNonzeroEsiPlusC:
                    capture.NonzeroEsiPlusCDword.HasValue,
                stopAt: alignmentPlan.StopAtRawFileOffsetExclusive);
        }

        uint derivedEax = alignmentPlan.EAX!.Value;
        uint derivedEdi = alignmentPlan.EDI!.Value;
        uint derivedEsi = alignmentPlan.ESI!.Value;
        uint derivedEbp = capture.AlignmentDispatch.EntryEBP;

        if (capture.NonzeroEntryEBX is not uint entryEbx)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .MissingIndependentNonzeroEntryEBX,
                alignmentPlan,
                nonzeroValidationPlan: null,
                alignmentDispatchOutcomeClean: true,
                exactEntryEcxNonzeroBranch: true,
                nonzeroValidationReached: false,
                derivedNonzeroEntryEAX: derivedEax,
                derivedNonzeroEntryEDI: derivedEdi,
                derivedNonzeroEntryESI: derivedEsi,
                derivedNonzeroEntryEBP: derivedEbp,
                alignmentDestinationSelected: true,
                alignmentDestinationRawFileOffset: alignmentDestination,
                nonzeroDestinationSelected: false,
                nonzeroDestinationRawFileOffset: null,
                unreachedAlignmentEntryEsi,
                unreachedAlignmentGlobal,
                unreachedAlignmentCallCell,
                unreachedAlignmentCallReturn,
                unreachedAlignment,
                unreachedNonzeroEntryEBX: false,
                unreachedNonzeroGlobal:
                    capture.NonzeroGlobalDword.HasValue,
                unreachedNonzeroEsiPlus8:
                    capture.NonzeroEsiPlus8Dword.HasValue,
                unreachedNonzeroEsiPlusC:
                    capture.NonzeroEsiPlusCDword.HasValue,
                stopAt: NonzeroEntryRawFileOffset);
        }

        CapacityFinalCalleeNonzeroValidationCapture nonzeroCapture = new(
            CaptureValidatedEntry: true,
            EntryEAX: derivedEax,
            EntryEDI: derivedEdi,
            EntryESI: derivedEsi,
            EntryEBX: entryEbx,
            EntryEBP: derivedEbp,
            GlobalDword: capture.NonzeroGlobalDword,
            EsiPlus8Dword: capture.NonzeroEsiPlus8Dword,
            EsiPlusCDword: capture.NonzeroEsiPlusCDword);
        CapacityFinalCalleeNonzeroValidationPlan nonzeroPlan =
            CapacityFinalCalleeNonzeroValidationProjection.Project(nonzeroCapture);
        bool nonzeroDestinationSelected =
            TryGetCleanNonzeroDestination(nonzeroPlan, out uint destination);

        return BuildPlan(
            capture,
            nonzeroDestinationSelected
                ? GetNonzeroSuccessOutcome(nonzeroPlan.Outcome)
                : CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroValidationBlocked,
            alignmentPlan,
            nonzeroPlan,
            alignmentDispatchOutcomeClean: true,
            exactEntryEcxNonzeroBranch: true,
            nonzeroValidationReached: true,
            derivedNonzeroEntryEAX: derivedEax,
            derivedNonzeroEntryEDI: derivedEdi,
            derivedNonzeroEntryESI: derivedEsi,
            derivedNonzeroEntryEBP: derivedEbp,
            alignmentDestinationSelected: true,
            alignmentDestinationRawFileOffset: alignmentDestination,
            nonzeroDestinationSelected,
            nonzeroDestinationRawFileOffset:
                nonzeroDestinationSelected ? destination : null,
            unreachedAlignmentEntryEsi,
            unreachedAlignmentGlobal,
            unreachedAlignmentCallCell,
            unreachedAlignmentCallReturn,
            unreachedAlignment,
            unreachedNonzeroEntryEBX: false,
            unreachedNonzeroGlobal:
                nonzeroPlan.UnreachedGlobalDwordCaptureProvided,
            unreachedNonzeroEsiPlus8:
                nonzeroPlan.UnreachedEsiPlus8DwordCaptureProvided,
            unreachedNonzeroEsiPlusC:
                nonzeroPlan.UnreachedEsiPlusCDwordCaptureProvided,
            stopAt: nonzeroPlan.StopAtRawFileOffsetExclusive);
    }

    private static AlignmentRoute GetCleanAlignmentRoute(
        CapacityFinalCalleeAlignmentDispatchPlan plan)
    {
        if (!HasCleanAlignmentPrelude(plan))
        {
            return AlignmentRoute.None;
        }

        return plan.Outcome switch
        {
            CapacityFinalCalleeAlignmentDispatchOutcome.EntryAlLowBitSet
                when IsCleanEntryAlBranch(plan) =>
                AlignmentRoute.EntryAlLowBitSet,
            CapacityFinalCalleeAlignmentDispatchOutcome.EntryEcxNonzero
                when IsCleanEntryEcxNonzeroBranch(plan) =>
                AlignmentRoute.EntryEcxNonzero,
            CapacityFinalCalleeAlignmentDispatchOutcome.ComputedEcxZero
                when IsCleanComputedEcxZeroBranch(plan) =>
                AlignmentRoute.ComputedEcxZero,
            CapacityFinalCalleeAlignmentDispatchOutcome.CallReturnedNormally
                when IsCleanCallReturn(plan) =>
                AlignmentRoute.CallReturnedNormally,
            _ => AlignmentRoute.None
        };
    }

    private static bool HasCleanAlignmentPrelude(
        CapacityFinalCalleeAlignmentDispatchPlan plan)
    {
        CapacityFinalCalleeAlignmentDispatchCapture capture = plan.Capture;
        uint alignedEax = capture.EntryEAX & 0xFFFFFFF8;
        return capture.CaptureValidatedEntry
            && plan.TestAlReached
            && plan.TestAlFlags is CapacityFinalCalleeAlignmentDispatchFlags
            {
                Carry: false,
                AuxiliaryCarry: null,
                Overflow: false
            } testAlFlags
            && testAlFlags.Zero == ((capture.EntryEAX & 1) == 0)
            && testAlFlags.Parity == EvenParity((byte)(capture.EntryEAX & 1))
            && !testAlFlags.Sign
            && plan.FrameWrites.Length >= 2
            && plan.FrameWrites[0]
                == new CapacityFinalCalleeAlignmentDispatchWrite(
                    0x012C8DA9,
                    CapacityFinalCalleeAlignmentDispatchProjection.FrameLocalAddress(
                        capture.EntryEBP,
                        CapacityFinalCalleeAlignmentDispatchProjection
                            .FrameLocalMinusEightDisplacement),
                    1)
            && plan.FrameWrites[1]
                == new CapacityFinalCalleeAlignmentDispatchWrite(
                    0x012C8DB6,
                    CapacityFinalCalleeAlignmentDispatchProjection.FrameLocalAddress(
                        capture.EntryEBP,
                        CapacityFinalCalleeAlignmentDispatchProjection
                            .FrameLocalMinusTwelveDisplacement),
                    unchecked(alignedEax + capture.EntryESI))
            && plan.EDX == unchecked(alignedEax + capture.EntryESI);
    }

    private static bool IsCleanEntryAlBranch(
        CapacityFinalCalleeAlignmentDispatchPlan plan) =>
        !plan.UnreachedEntryEsiDwordCaptureProvided
        && !plan.UnreachedGlobalDispatchDwordCaptureProvided
        && !plan.UnreachedCallCellTargetCaptureProvided
        && !plan.UnreachedCallReturnStatusProvided
        && (plan.Capture.EntryEAX & 1) != 0
        && plan.EDI == (plan.Capture.EntryEAX & 0xFFFFFFF8)
        && plan.FrameWrites.Length == 2
        && plan.TestAlBranchReached
        && plan.TestAlBranchTaken == true
        && plan.TestAlSelectedDestinationRawFileOffset
            == CapacityFinalCalleeAlignmentDispatchProjection
                .EntryAlBranchDestinationRawFileOffset
        && plan.TestAlSelectedDestinationVirtualAddress
            == CapacityFinalCalleeAlignmentDispatchProjection
                .EntryAlBranchDestinationVirtualAddress
        && plan.Reads.IsEmpty
        && !plan.TestEntryEcxReached
        && !plan.TestComputedEcxReached
        && !plan.GlobalDispatchReadReached
        && !plan.CallCellReadReached
        && !plan.CallReached
        && plan.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeAlignmentDispatchProjection
                .EntryAlBranchDestinationRawFileOffset;

    private static bool IsCleanEntryEcxNonzeroBranch(
        CapacityFinalCalleeAlignmentDispatchPlan plan)
    {
        CapacityFinalCalleeAlignmentDispatchCapture capture = plan.Capture;
        if (capture.EntryEsiDword is not uint loadedWord)
        {
            return false;
        }

        return !plan.UnreachedEntryEsiDwordCaptureProvided
            && !plan.UnreachedGlobalDispatchDwordCaptureProvided
            && !plan.UnreachedCallCellTargetCaptureProvided
            && !plan.UnreachedCallReturnStatusProvided
            && (plan.Capture.EntryEAX & 1) == 0
            && plan.TestAlFlags?.Zero == true
            && plan.TestAlBranchReached
            && plan.TestAlBranchTaken == false
            && plan.Reads.Length == 1
            && plan.Reads[0]
                == new CapacityFinalCalleeAlignmentDispatchRead(
                    CapacityFinalCalleeAlignmentDispatchProjection
                        .EntryEsiReadRawFileOffset,
                    CapacityFinalCalleeAlignmentDispatchReadKind.EntryEsiDword,
                    CapacityFinalCalleeAlignmentDispatchProjection.EntryEsiAddress(
                        capture.EntryESI),
                    true,
                    loadedWord)
            && plan.EntryEsiReadReached
            && plan.EntryEsiDwordCaptured
            && plan.EntryEsiDword == loadedWord
            && plan.EAX == loadedWord
            && plan.ESI == unchecked(capture.EntryESI - loadedWord)
            && plan.EDI == (capture.EntryEAX & 0xFFFFFFF8)
            && plan.TestEntryEcxReached
            && plan.TestEntryEcxFlags is
                CapacityFinalCalleeAlignmentDispatchFlags
                {
                    Carry: false,
                    AuxiliaryCarry: null,
                    Zero: false,
                    Overflow: false
                } testFlags
            && testFlags.Parity == EvenParity((byte)capture.EntryECX)
            && testFlags.Sign
                == ((capture.EntryECX & 0x80000000) != 0)
            && plan.TestEntryEcxBranchReached
            && plan.TestEntryEcxBranchTaken == true
            && plan.TestEntryEcxSelectedDestinationRawFileOffset
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .EntryEcxBranchDestinationRawFileOffset
            && plan.TestEntryEcxSelectedDestinationVirtualAddress
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .EntryEcxBranchDestinationVirtualAddress
            && plan.ECX == capture.EntryECX
            && plan.FrameWrites.Length == 2
            && !plan.TestComputedEcxReached
            && !plan.GlobalDispatchReadReached
            && !plan.CallCellReadReached
            && !plan.CallReached
            && plan.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .EntryEcxBranchDestinationRawFileOffset;
    }

    private static bool IsCleanComputedEcxZeroBranch(
        CapacityFinalCalleeAlignmentDispatchPlan plan)
    {
        CapacityFinalCalleeAlignmentDispatchCapture capture = plan.Capture;
        if (capture.EntryEsiDword is not uint loadedWord)
        {
            return false;
        }

        uint computedEcx = unchecked(
            unchecked(loadedWord
                + CapacityFinalCalleeAlignmentDispatchProjection.ComputedEcxOffset)
            + (capture.EntryEAX & 0xFFFFFFF8));
        return !plan.UnreachedEntryEsiDwordCaptureProvided
            && !plan.UnreachedGlobalDispatchDwordCaptureProvided
            && !plan.UnreachedCallCellTargetCaptureProvided
            && !plan.UnreachedCallReturnStatusProvided
            && (capture.EntryEAX & 1) == 0
            && plan.TestAlBranchReached
            && plan.TestAlBranchTaken == false
            && plan.Reads.Length == 1
            && plan.Reads[0].RawFileOffset
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .EntryEsiReadRawFileOffset
            && plan.Reads[0].Kind
                == CapacityFinalCalleeAlignmentDispatchReadKind.EntryEsiDword
            && plan.Reads[0].Address
                == CapacityFinalCalleeAlignmentDispatchProjection.EntryEsiAddress(
                    capture.EntryESI)
            && plan.Reads[0].ValueCaptured
            && plan.Reads[0].Value == loadedWord
            && plan.EntryEsiReadReached
            && plan.EntryEsiDwordCaptured
            && plan.EntryEsiDword == loadedWord
            && plan.EAX == loadedWord
            && plan.ESI == unchecked(capture.EntryESI - loadedWord)
            && plan.TestEntryEcxReached
            && plan.TestEntryEcxFlags is
                CapacityFinalCalleeAlignmentDispatchFlags
                {
                    Carry: false,
                    AuxiliaryCarry: null,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                } entryEcxFlags
            && entryEcxFlags.Parity
                == EvenParity((byte)capture.EntryECX)
            && plan.TestEntryEcxBranchReached
            && plan.TestEntryEcxBranchTaken == false
            && plan.TestComputedEcxReached
            && plan.TestComputedEcxFlags is
                CapacityFinalCalleeAlignmentDispatchFlags
                {
                    Carry: false,
                    AuxiliaryCarry: null,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                } testFlags
            && testFlags.Parity == EvenParity((byte)computedEcx)
            && computedEcx == 0
            && plan.TestComputedEcxBranchReached
            && plan.TestComputedEcxBranchTaken == true
            && plan.TestComputedEcxSelectedDestinationRawFileOffset
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .ComputedEcxZeroBranchDestinationRawFileOffset
            && plan.TestComputedEcxSelectedDestinationVirtualAddress
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .ComputedEcxZeroBranchDestinationVirtualAddress
            && plan.ECX == computedEcx
            && plan.EDI == computedEcx
            && plan.FrameWrites.Length == 3
            && plan.FrameWrites[2]
                == new CapacityFinalCalleeAlignmentDispatchWrite(
                    0x012C8DCE,
                    CapacityFinalCalleeAlignmentDispatchProjection.FrameLocalAddress(
                        capture.EntryEBP,
                        CapacityFinalCalleeAlignmentDispatchProjection
                            .FrameLocalMinusSixteenDisplacement),
                    computedEcx)
            && !plan.GlobalDispatchReadReached
            && !plan.CallCellReadReached
            && !plan.CallReached
            && plan.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .ComputedEcxZeroBranchDestinationRawFileOffset;
    }

    private static bool IsCleanCallReturn(
        CapacityFinalCalleeAlignmentDispatchPlan plan)
    {
        CapacityFinalCalleeAlignmentDispatchCapture capture = plan.Capture;
        if (capture.EntryEsiDword is not uint loadedWord
            || capture.GlobalDispatchDword is not uint globalDword
            || capture.CallCellTargetVirtualAddress is not uint callCellTarget)
        {
            return false;
        }

        uint computedEcx = unchecked(
            unchecked(loadedWord
                + CapacityFinalCalleeAlignmentDispatchProjection.ComputedEcxOffset)
            + (capture.EntryEAX & 0xFFFFFFF8));
        uint computedEsi = unchecked(capture.EntryESI - loadedWord);
        uint callArgument = CapacityFinalCalleeAlignmentDispatchProjection
            .FrameLocalAddress(
                capture.EntryEBP,
                CapacityFinalCalleeAlignmentDispatchProjection
                    .FrameLocalMinusFortyEightDisplacement);
        if (computedEcx == 0)
        {
            return false;
        }

        return !plan.UnreachedEntryEsiDwordCaptureProvided
            && !plan.UnreachedGlobalDispatchDwordCaptureProvided
            && !plan.UnreachedCallCellTargetCaptureProvided
            && !plan.UnreachedCallReturnStatusProvided
            && plan.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.CallReturnedNormally
            && capture.EntryECX == 0
            && plan.TestAlBranchReached
            && plan.TestAlBranchTaken == false
            && plan.Reads.Length == 3
            && plan.Reads[0]
                == new CapacityFinalCalleeAlignmentDispatchRead(
                    CapacityFinalCalleeAlignmentDispatchProjection
                        .EntryEsiReadRawFileOffset,
                    CapacityFinalCalleeAlignmentDispatchReadKind.EntryEsiDword,
                    CapacityFinalCalleeAlignmentDispatchProjection.EntryEsiAddress(
                        capture.EntryESI),
                    true,
                    loadedWord)
            && plan.EntryEsiReadReached
            && plan.EntryEsiDwordCaptured
            && plan.EntryEsiDword == loadedWord
            && plan.EAX == callArgument
            && plan.ESI == computedEsi
            && plan.TestEntryEcxReached
            && plan.TestEntryEcxFlags is
                CapacityFinalCalleeAlignmentDispatchFlags
                {
                    Carry: false,
                    AuxiliaryCarry: null,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                } entryEcxFlags
            && entryEcxFlags.Parity
            && plan.TestEntryEcxBranchReached
            && plan.TestEntryEcxBranchTaken == false
            && plan.TestComputedEcxReached
            && plan.TestComputedEcxFlags is
                CapacityFinalCalleeAlignmentDispatchFlags
                {
                    Carry: false,
                    AuxiliaryCarry: null,
                    Zero: false,
                    Overflow: false
                } computedFlags
            && computedFlags.Sign == ((computedEcx & 0x80000000) != 0)
            && computedFlags.Parity == EvenParity((byte)computedEcx)
            && plan.TestComputedEcxBranchReached
            && plan.TestComputedEcxBranchTaken == false
            && plan.TestComputedEcxSelectedDestinationRawFileOffset is null
            && plan.TestComputedEcxSelectedDestinationVirtualAddress is null
            && plan.ECX == computedEcx
            && plan.EDI == computedEcx
            && plan.EBX == globalDword
            && plan.FrameWrites.Length == 3
            && plan.FrameWrites[2]
                == new CapacityFinalCalleeAlignmentDispatchWrite(
                    0x012C8DCE,
                    CapacityFinalCalleeAlignmentDispatchProjection.FrameLocalAddress(
                        capture.EntryEBP,
                        CapacityFinalCalleeAlignmentDispatchProjection
                            .FrameLocalMinusSixteenDisplacement),
                    computedEcx)
            && plan.Pushes.Length == 3
            && plan.Pushes[0]
                == new CapacityFinalCalleeAlignmentDispatchPush(
                    CapacityFinalCalleeAlignmentDispatchProjection.FirstPushRawFileOffset,
                    unchecked(capture.EntryESP - 4u),
                    CapacityFinalCalleeAlignmentDispatchProjection.StackArgumentSize)
            && plan.Pushes[1]
                == new CapacityFinalCalleeAlignmentDispatchPush(
                    CapacityFinalCalleeAlignmentDispatchProjection.SecondPushRawFileOffset,
                    unchecked(capture.EntryESP - 8u),
                    callArgument)
            && plan.Pushes[2]
                == new CapacityFinalCalleeAlignmentDispatchPush(
                    CapacityFinalCalleeAlignmentDispatchProjection.ThirdPushRawFileOffset,
                    unchecked(capture.EntryESP - 12u),
                    computedEsi)
            && plan.ESP == unchecked(capture.EntryESP - 12u)
            && plan.GlobalDispatchReadReached
            && plan.GlobalDispatchDwordCaptured
            && plan.GlobalDispatchDword == globalDword
            && plan.Reads[1]
                == new CapacityFinalCalleeAlignmentDispatchRead(
                    CapacityFinalCalleeAlignmentDispatchProjection
                        .GlobalDispatchReadRawFileOffset,
                    CapacityFinalCalleeAlignmentDispatchReadKind.GlobalDispatchDword,
                    CapacityFinalCalleeAlignmentDispatchProjection
                        .GlobalDispatchCellVirtualAddress,
                    true,
                    globalDword)
            && plan.CallCellReadReached
            && plan.CallCellTargetCaptured
            && plan.CallCellTargetVirtualAddress == callCellTarget
            && plan.Reads[2]
                == new CapacityFinalCalleeAlignmentDispatchRead(
                    CapacityFinalCalleeAlignmentDispatchProjection.CallRawFileOffset,
                    CapacityFinalCalleeAlignmentDispatchReadKind
                        .CallCellTargetVirtualAddress,
                    CapacityFinalCalleeAlignmentDispatchProjection
                        .CallCellVirtualAddress,
                    true,
                    callCellTarget)
            && plan.CallReached
            && plan.CallReturnedNormally == true
            && plan.CallRegisterOutputsUnknown
            && plan.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeAlignmentDispatchProjection
                    .BoundaryRawFileOffsetExclusive;
    }

    private static bool HasSelectedAlignmentDestination(
        CapacityFinalCalleeAlignmentDispatchPlan plan,
        AlignmentRoute route) =>
        route switch
        {
            AlignmentRoute.EntryAlLowBitSet =>
                plan.TestAlSelectedDestinationRawFileOffset.HasValue,
            AlignmentRoute.EntryEcxNonzero =>
                plan.TestEntryEcxSelectedDestinationRawFileOffset.HasValue,
            AlignmentRoute.ComputedEcxZero =>
                plan.TestComputedEcxSelectedDestinationRawFileOffset.HasValue,
            AlignmentRoute.CallReturnedNormally =>
                plan.StopAtRawFileOffsetExclusive
                    == CapacityFinalCalleeAlignmentDispatchProjection
                        .BoundaryRawFileOffsetExclusive,
            _ => false
        };

    private static uint? GetAlignmentDestination(
        CapacityFinalCalleeAlignmentDispatchPlan plan,
        AlignmentRoute route) =>
        route switch
        {
            AlignmentRoute.EntryAlLowBitSet =>
                plan.TestAlSelectedDestinationRawFileOffset,
            AlignmentRoute.EntryEcxNonzero =>
                plan.TestEntryEcxSelectedDestinationRawFileOffset,
            AlignmentRoute.ComputedEcxZero =>
                plan.TestComputedEcxSelectedDestinationRawFileOffset,
            AlignmentRoute.CallReturnedNormally =>
                CapacityFinalCalleeAlignmentDispatchProjection
                    .BoundaryRawFileOffsetExclusive,
            _ => null
        };

    private static bool TryGetCleanNonzeroDestination(
        CapacityFinalCalleeNonzeroValidationPlan plan,
        out uint destination)
    {
        destination = plan.StopAtRawFileOffsetExclusive;
        if (plan.Capture.CaptureValidatedEntry != true
            || plan.UnreachedGlobalDwordCaptureProvided
            || plan.UnreachedEsiPlus8DwordCaptureProvided
            || plan.UnreachedEsiPlusCDwordCaptureProvided)
        {
            return false;
        }

        return plan.Outcome switch
        {
            CapacityFinalCalleeNonzeroValidationOutcome
                .EntryEsiUnsignedBelowEntryEbx =>
                plan.CompareEsiEbxReached
                && plan.UnsignedBelowEbxBranchReached
                && plan.EntryEsiUnsignedBelowEntryEbx == true
                && plan.CompareEsiEbxFlags?.Carry == true
                && !plan.GlobalReadReached
                && plan.Reads.IsEmpty
                && plan.Writes.IsEmpty
                && DestinationIs(
                    plan,
                    CapacityFinalCalleeNonzeroValidationProjection
                        .EntryEsiUnsignedBelowEbxDestinationRawFileOffset),
            CapacityFinalCalleeNonzeroValidationOutcome
                .EntryEsiEqualsGlobalDword =>
                plan.GlobalDwordCaptured
                && plan.CompareEsiGlobalReached
                && plan.GlobalEqualsBranchReached
                && plan.EntryEsiEqualsGlobalDword == true
                && plan.CompareEsiGlobalFlags?.Zero == true
                && plan.Reads.Length == 1
                && plan.Reads[0].ValueCaptured
                && plan.Reads[0].Value == plan.Capture.GlobalDword
                && plan.Writes.IsEmpty
                && DestinationIs(
                    plan,
                    CapacityFinalCalleeNonzeroValidationProjection
                        .EntryEsiEqualsGlobalDestinationRawFileOffset),
            CapacityFinalCalleeNonzeroValidationOutcome
                .ShiftedEaxUnsignedAboveOrEqual32 =>
                plan.GlobalDwordCaptured
                && plan.CapacityJnbReached
                && plan.ShiftedEaxUnsignedAboveOrEqual32 == true
                && plan.CompareShiftedEaxFlags?.Carry == false
                && plan.Writes.Length == 1
                && plan.Writes[0].RawFileOffset
                    == CapacityFinalCalleeNonzeroValidationProjection
                        .StoreShiftedEaxRawFileOffset
                && plan.EsiPlus8ReadReached == false
                && DestinationIs(
                    plan,
                    CapacityFinalCalleeNonzeroValidationProjection
                        .CapacityJnbDestinationRawFileOffset),
            CapacityFinalCalleeNonzeroValidationOutcome
                .EntryEsiNotEqualEsiPlusCDword =>
                IsCleanFinalReadAndCompare(plan, expectedNotEqual: true)
                && DestinationIs(
                    plan,
                    CapacityFinalCalleeNonzeroValidationProjection
                        .EntryEsiNotEqualDestinationRawFileOffset),
            CapacityFinalCalleeNonzeroValidationOutcome
                .EntryEsiEqualsEsiPlusCDword =>
                IsCleanFinalReadAndCompare(plan, expectedNotEqual: false)
                && DestinationIs(
                    plan,
                    CapacityFinalCalleeNonzeroValidationProjection
                        .BoundaryRawFileOffsetExclusive),
            _ => false
        };
    }

    private static bool IsCleanFinalReadAndCompare(
        CapacityFinalCalleeNonzeroValidationPlan plan,
        bool expectedNotEqual) =>
        plan.EsiPlus8ReadReached
        && plan.EsiPlus8DwordCaptured
        && plan.StoreEsiPlus8Reached
        && plan.EsiPlusCReadReached
        && plan.EsiPlusCDwordCaptured
        && plan.StoreEsiPlusCReached
        && plan.CompareEsiEsiPlusCReached
        && plan.EsiNotEqualBranchReached
        && plan.EntryEsiNotEqualEsiPlusCDword == expectedNotEqual
        && plan.CompareEsiEsiPlusCFlags?.Zero == !expectedNotEqual
        && plan.Reads.Length == 3
        && plan.Reads.All(read => read.ValueCaptured)
        && plan.Reads[0].Kind
            == CapacityFinalCalleeNonzeroValidationReadKind.GlobalDword
        && plan.Reads[1].Kind
            == CapacityFinalCalleeNonzeroValidationReadKind.EsiPlus8Dword
        && plan.Reads[2].Kind
            == CapacityFinalCalleeNonzeroValidationReadKind.EsiPlusCDword
        && plan.Writes.Select(write => write.RawFileOffset).SequenceEqual(
            new uint[]
            {
                CapacityFinalCalleeNonzeroValidationProjection
                    .StoreShiftedEaxRawFileOffset,
                CapacityFinalCalleeNonzeroValidationProjection
                    .StoreEsiPlus8RawFileOffset,
                CapacityFinalCalleeNonzeroValidationProjection
                    .StoreEsiPlusCStackRawFileOffset
            });

    private static bool DestinationIs(
        CapacityFinalCalleeNonzeroValidationPlan plan,
        uint expectedDestination) =>
        plan.StopAtRawFileOffsetExclusive == expectedDestination;

    private static CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
        GetNonzeroSuccessOutcome(
            CapacityFinalCalleeNonzeroValidationOutcome outcome) =>
        outcome switch
        {
            CapacityFinalCalleeNonzeroValidationOutcome
                .EntryEsiUnsignedBelowEntryEbx =>
                CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroEntryEsiUnsignedBelowEntryEbxDestination,
            CapacityFinalCalleeNonzeroValidationOutcome
                .EntryEsiEqualsGlobalDword =>
                CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroEntryEsiEqualsGlobalDwordDestination,
            CapacityFinalCalleeNonzeroValidationOutcome
                .ShiftedEaxUnsignedAboveOrEqual32 =>
                CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroShiftedEaxUnsignedAboveOrEqual32Destination,
            CapacityFinalCalleeNonzeroValidationOutcome
                .EntryEsiNotEqualEsiPlusCDword =>
                CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroEntryEsiNotEqualEsiPlusCDwordDestination,
            CapacityFinalCalleeNonzeroValidationOutcome
                .EntryEsiEqualsEsiPlusCDword =>
                CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                    .NonzeroEntryEsiEqualsEsiPlusCDwordDestination,
            _ => CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome
                .NonzeroValidationBlocked
        };

    private static bool AnyNonzeroCaptureProvided(
        CapacityFinalCalleeAlignmentNonzeroLifecycleCapture capture) =>
        capture.NonzeroEntryEBX.HasValue
        || capture.NonzeroGlobalDword.HasValue
        || capture.NonzeroEsiPlus8Dword.HasValue
        || capture.NonzeroEsiPlusCDword.HasValue;

    private static bool EvenParity(byte value) =>
        System.Numerics.BitOperations.PopCount((uint)value) % 2 == 0;

    private static CapacityFinalCalleeAlignmentNonzeroLifecyclePlan BuildPlan(
        CapacityFinalCalleeAlignmentNonzeroLifecycleCapture capture,
        CapacityFinalCalleeAlignmentNonzeroLifecycleOutcome outcome,
        CapacityFinalCalleeAlignmentDispatchPlan alignmentPlan,
        CapacityFinalCalleeNonzeroValidationPlan? nonzeroValidationPlan,
        bool alignmentDispatchOutcomeClean,
        bool exactEntryEcxNonzeroBranch,
        bool nonzeroValidationReached,
        uint? derivedNonzeroEntryEAX,
        uint? derivedNonzeroEntryEDI,
        uint? derivedNonzeroEntryESI,
        uint? derivedNonzeroEntryEBP,
        bool alignmentDestinationSelected,
        uint? alignmentDestinationRawFileOffset,
        bool nonzeroDestinationSelected,
        uint? nonzeroDestinationRawFileOffset,
        bool unreachedAlignmentEntryEsi,
        bool unreachedAlignmentGlobal,
        bool unreachedAlignmentCallCell,
        bool unreachedAlignmentCallReturn,
        bool unreachedAlignment,
        bool unreachedNonzeroEntryEBX,
        bool unreachedNonzeroGlobal,
        bool unreachedNonzeroEsiPlus8,
        bool unreachedNonzeroEsiPlusC,
        uint stopAt)
    {
        bool unreachedNonzero = unreachedNonzeroEntryEBX
            || unreachedNonzeroGlobal
            || unreachedNonzeroEsiPlus8
            || unreachedNonzeroEsiPlusC;
        return new(
            capture,
            outcome,
            alignmentPlan,
            nonzeroValidationPlan,
            alignmentDispatchOutcomeClean,
            exactEntryEcxNonzeroBranch,
            nonzeroValidationReached,
            derivedNonzeroEntryEAX,
            derivedNonzeroEntryEDI,
            derivedNonzeroEntryESI,
            derivedNonzeroEntryEBP,
            capture.NonzeroEntryEBX.HasValue,
            capture.NonzeroEntryEBX,
            alignmentDestinationSelected,
            alignmentDestinationSelected ? alignmentDestinationRawFileOffset : null,
            nonzeroDestinationSelected,
            nonzeroDestinationSelected ? nonzeroDestinationRawFileOffset : null,
            unreachedAlignmentEntryEsi,
            unreachedAlignmentGlobal,
            unreachedAlignmentCallCell,
            unreachedAlignmentCallReturn,
            unreachedAlignment,
            unreachedNonzeroEntryEBX,
            unreachedNonzeroGlobal,
            unreachedNonzeroEsiPlus8,
            unreachedNonzeroEsiPlusC,
            unreachedNonzero,
            stopAt);
    }

    private enum AlignmentRoute
    {
        None,
        EntryAlLowBitSet,
        EntryEcxNonzero,
        ComputedEcxZero,
        CallReturnedNormally
    }
}
