namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffSlotCallPublicationCapture(
    FrameHandoffHelperSlotCapture Slot,
    FrameHandoffEmptySlotCallCapture Call,
    FrameHandoffSlotPublicationCapture Publication);

public enum FrameHandoffSlotCallPublicationOutcome
{
    SlotProjectionBlocked,
    NonzeroSlotRouteRejectedUnreachedCaptures,
    CallProjectionBlocked,
    CallHasUnreachedCaptures,
    MissingPublicationBranchEntry,
    InvalidPublicationBranchEntry,
    PublicationReceiverIdentityMismatch,
    PublicationEbxIdentityMismatch,
    PublicationEdiIdentityMismatch,
    PublicationEcxMismatch,
    PublicationProjectionBlocked,
    ProjectionComplete
}

public readonly record struct FrameHandoffSlotCallPublicationPlan(
    FrameHandoffSlotCallPublicationCapture Capture,
    FrameHandoffSlotCallPublicationOutcome Outcome,
    FrameHandoffHelperSlotPlan SlotPlan,
    FrameHandoffEmptySlotCallPlan? CallPlan,
    FrameHandoffSlotPublicationPlan? PublicationPlan,
    uint? NativeEsiReceiver,
    uint? NativeEbxSlotByteOffset,
    uint? NativeEdiWrappingReceiverPlus0C,
    uint? KnownEcxAfterDirectCall,
    uint? CapturedZeroRoutePostCallEcx,
    uint? PublicationEcx,
    bool PublicationReceiverIdentityMatches,
    bool PublicationEbxIdentityMatches,
    bool PublicationEdiIdentityMatches,
    bool UnreachedCallEntryValidationProvided,
    bool UnreachedDirectCallReturnCaptureProvided,
    bool UnreachedDirectCallReturnEaxCaptureProvided,
    bool UnreachedZeroRouteIndirectCallReturnCaptureProvided,
    bool UnreachedZeroRoutePostCallEcxCaptureProvided,
    bool UnreachedPublicationIncomingEcxCaptureProvided,
    bool UnreachedPublicationStoreBase6508CaptureProvided,
    bool UnreachedPublicationFreshBase650EProvided,
    bool UnreachedPublicationSlotDwordProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the first-slot branch, empty-slot call, and later publication reloads.
/// </summary>
public static class FrameHandoffSlotCallPublicationProjection
{
    public const uint StopAtRawFileOffsetExclusive = 0x00BE651C;

    public static FrameHandoffSlotCallPublicationPlan Project(
        FrameHandoffSlotCallPublicationCapture capture)
    {
        FrameHandoffHelperSlotPlan slotPlan =
            FrameHandoffHelperSlotProjection.Project(capture.Slot);

        if (slotPlan.Outcome is not (
            FrameHandoffHelperSlotOutcome.FirstSlotZeroFallsThrough
            or FrameHandoffHelperSlotOutcome.FirstSlotNonzeroBranches))
        {
            return Stop(
                capture,
                FrameHandoffSlotCallPublicationOutcome.SlotProjectionBlocked,
                slotPlan,
                callPlan: null,
                publicationPlan: null,
                unreachedCallEntryValidationProvided: capture.Call.CaptureValidatedEntry,
                unreachedDirectCallReturnCaptureProvided:
                    capture.Call.DirectCallReturnedNormally.HasValue,
                unreachedDirectCallReturnEaxCaptureProvided:
                    capture.Call.DirectCallReturnEax.HasValue,
                unreachedZeroRouteIndirectCallReturnCaptureProvided:
                    capture.Call.ZeroRouteIndirectCallReturnedNormally.HasValue,
                unreachedZeroRoutePostCallEcxCaptureProvided:
                    capture.Call.ZeroRoutePostCallEcx.HasValue,
                unreachedPublicationIncomingEcxCaptureProvided:
                    capture.Publication.IncomingEcx.HasValue,
                unreachedPublicationStoreBase6508CaptureProvided:
                    capture.Publication.ReceiverPlus04At6508.HasValue,
                unreachedPublicationFreshBase650EProvided:
                    capture.Publication.ReceiverPlus04At650E.HasValue,
                unreachedPublicationSlotDwordProvided:
                    capture.Publication.SlotDwordAtEbxPlusFreshBase.HasValue,
                stopAt: slotPlan.StopAtRawFileOffsetExclusive);
        }

        uint nativeEsi = capture.Slot.ReceiverBaseEsi;
        uint nativeEbx = slotPlan.FirstSlotByteOffset!.Value;
        uint nativeEdi = slotPlan.WrappingEdiPlusReceiverPlus0CFresh!.Value;
        bool zeroSlotRoute =
            slotPlan.Outcome == FrameHandoffHelperSlotOutcome.FirstSlotZeroFallsThrough;

        if (!zeroSlotRoute)
        {
            bool callCapturesProvided =
                capture.Call.CaptureValidatedEntry
                || capture.Call.DirectCallReturnedNormally.HasValue
                || capture.Call.DirectCallReturnEax.HasValue
                || capture.Call.ZeroRouteIndirectCallReturnedNormally.HasValue
                || capture.Call.ZeroRoutePostCallEcx.HasValue;
            bool storeCapturesProvided =
                capture.Publication.IncomingEcx.HasValue
                || capture.Publication.ReceiverPlus04At6508.HasValue;
            if (callCapturesProvided || storeCapturesProvided)
            {
                return Stop(
                    capture,
                    FrameHandoffSlotCallPublicationOutcome
                        .NonzeroSlotRouteRejectedUnreachedCaptures,
                    slotPlan,
                    callPlan: null,
                    publicationPlan: null,
                    nativeEsiReceiver: nativeEsi,
                    nativeEbxSlotByteOffset: nativeEbx,
                    nativeEdiWrappingReceiverPlus0C: nativeEdi,
                    unreachedCallEntryValidationProvided:
                        capture.Call.CaptureValidatedEntry,
                    unreachedDirectCallReturnCaptureProvided:
                        capture.Call.DirectCallReturnedNormally.HasValue,
                    unreachedDirectCallReturnEaxCaptureProvided:
                        capture.Call.DirectCallReturnEax.HasValue,
                    unreachedZeroRouteIndirectCallReturnCaptureProvided:
                        capture.Call.ZeroRouteIndirectCallReturnedNormally.HasValue,
                    unreachedZeroRoutePostCallEcxCaptureProvided:
                        capture.Call.ZeroRoutePostCallEcx.HasValue,
                    unreachedPublicationIncomingEcxCaptureProvided:
                        capture.Publication.IncomingEcx.HasValue,
                    unreachedPublicationStoreBase6508CaptureProvided:
                        capture.Publication.ReceiverPlus04At6508.HasValue,
                    unreachedPublicationFreshBase650EProvided:
                        capture.Publication.ReceiverPlus04At650E.HasValue,
                    unreachedPublicationSlotDwordProvided:
                        capture.Publication.SlotDwordAtEbxPlusFreshBase.HasValue,
                    stopAt: FrameHandoffHelperSlotProjection
                        .FirstSlotNonzeroBranchDestinationRawFileOffset);
            }

            return ProjectPublication(
                capture,
                slotPlan,
                null,
                nativeEsi,
                nativeEbx,
                nativeEdi,
                publicationEcx: null,
                expectedEntry: FrameHandoffSlotPublicationEntry
                    .ExistingNonzeroSlotBranchAt650E,
                stopAt: FrameHandoffSlotPublicationProjection
                    .ExistingNonzeroSlotBranchEntryRawFileOffset);
        }

        FrameHandoffEmptySlotCallPlan callPlan =
            FrameHandoffEmptySlotCallProjection.Project(capture.Call);
        bool callOutcomeAllowed =
            callPlan.Outcome is
                FrameHandoffEmptySlotCallOutcome.DirectNonzeroRouteReturnedNormally
                or FrameHandoffEmptySlotCallOutcome.ZeroRouteReturnedNormally;
        if (!callOutcomeAllowed)
        {
            return Stop(
                capture,
                FrameHandoffSlotCallPublicationOutcome.CallProjectionBlocked,
                slotPlan,
                callPlan,
                publicationPlan: null,
                nativeEsiReceiver: nativeEsi,
                nativeEbxSlotByteOffset: nativeEbx,
                nativeEdiWrappingReceiverPlus0C: nativeEdi,
                unreachedDirectCallReturnCaptureProvided:
                    callPlan.UnreachedDirectCallReturnCaptureProvided,
                unreachedDirectCallReturnEaxCaptureProvided:
                    callPlan.UnreachedDirectCallReturnEaxCaptureProvided,
                unreachedZeroRouteIndirectCallReturnCaptureProvided:
                    callPlan.UnreachedZeroRouteIndirectCallReturnCaptureProvided,
                unreachedZeroRoutePostCallEcxCaptureProvided:
                    callPlan.UnreachedZeroRoutePostCallEcxCaptureProvided,
                unreachedPublicationIncomingEcxCaptureProvided:
                    capture.Publication.IncomingEcx.HasValue,
                unreachedPublicationStoreBase6508CaptureProvided:
                    capture.Publication.ReceiverPlus04At6508.HasValue,
                unreachedPublicationFreshBase650EProvided:
                    capture.Publication.ReceiverPlus04At650E.HasValue,
                unreachedPublicationSlotDwordProvided:
                    capture.Publication.SlotDwordAtEbxPlusFreshBase.HasValue,
                stopAt: callPlan.StopAtRawFileOffsetExclusive);
        }

        if (callPlan.UnreachedDirectCallReturnCaptureProvided
            || callPlan.UnreachedDirectCallReturnEaxCaptureProvided
            || callPlan.UnreachedZeroRouteIndirectCallReturnCaptureProvided
            || callPlan.UnreachedZeroRoutePostCallEcxCaptureProvided)
        {
            return Stop(
                capture,
                FrameHandoffSlotCallPublicationOutcome.CallHasUnreachedCaptures,
                slotPlan,
                callPlan,
                publicationPlan: null,
                nativeEsiReceiver: nativeEsi,
                nativeEbxSlotByteOffset: nativeEbx,
                nativeEdiWrappingReceiverPlus0C: nativeEdi,
                unreachedDirectCallReturnCaptureProvided:
                    callPlan.UnreachedDirectCallReturnCaptureProvided,
                unreachedDirectCallReturnEaxCaptureProvided:
                    callPlan.UnreachedDirectCallReturnEaxCaptureProvided,
                unreachedZeroRouteIndirectCallReturnCaptureProvided:
                    callPlan.UnreachedZeroRouteIndirectCallReturnCaptureProvided,
                unreachedZeroRoutePostCallEcxCaptureProvided:
                    callPlan.UnreachedZeroRoutePostCallEcxCaptureProvided,
                unreachedPublicationIncomingEcxCaptureProvided:
                    capture.Publication.IncomingEcx.HasValue,
                unreachedPublicationStoreBase6508CaptureProvided:
                    capture.Publication.ReceiverPlus04At6508.HasValue,
                unreachedPublicationFreshBase650EProvided:
                    capture.Publication.ReceiverPlus04At650E.HasValue,
                unreachedPublicationSlotDwordProvided:
                    capture.Publication.SlotDwordAtEbxPlusFreshBase.HasValue,
                stopAt: callPlan.StopAtRawFileOffsetExclusive);
        }

        uint publicationEcx = callPlan.Outcome switch
        {
            FrameHandoffEmptySlotCallOutcome.DirectNonzeroRouteReturnedNormally
                when callPlan.EcxAfterDirectCall is uint directEcx => directEcx,
            FrameHandoffEmptySlotCallOutcome.ZeroRouteReturnedNormally
                when callPlan.CapturedZeroRoutePostCallEcx is uint postCallEcx => postCallEcx,
            _ => throw new InvalidOperationException("The successful call plan has no ECX value.")
        };

        if (capture.Publication.IncomingEcx is uint suppliedEcx
            && suppliedEcx != publicationEcx)
        {
            return Stop(
                capture,
                FrameHandoffSlotCallPublicationOutcome.PublicationEcxMismatch,
                slotPlan,
                callPlan,
                publicationPlan: null,
                nativeEsiReceiver: nativeEsi,
                nativeEbxSlotByteOffset: nativeEbx,
                nativeEdiWrappingReceiverPlus0C: nativeEdi,
                knownEcxAfterDirectCall: callPlan.EcxAfterDirectCall,
                capturedZeroRoutePostCallEcx: callPlan.CapturedZeroRoutePostCallEcx,
                unreachedPublicationIncomingEcxCaptureProvided:
                    capture.Publication.IncomingEcx.HasValue,
                unreachedPublicationStoreBase6508CaptureProvided:
                    capture.Publication.ReceiverPlus04At6508.HasValue,
                unreachedPublicationFreshBase650EProvided:
                    capture.Publication.ReceiverPlus04At650E.HasValue,
                unreachedPublicationSlotDwordProvided:
                    capture.Publication.SlotDwordAtEbxPlusFreshBase.HasValue,
                stopAt: FrameHandoffSlotPublicationProjection.StoreRawFileOffset);
        }

        return ProjectPublication(
            capture,
            slotPlan,
            callPlan,
            nativeEsi,
            nativeEbx,
            nativeEdi,
            publicationEcx,
            FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
            callPlan.StopAtRawFileOffsetExclusive);
    }

    private static FrameHandoffSlotCallPublicationPlan ProjectPublication(
        FrameHandoffSlotCallPublicationCapture capture,
        FrameHandoffHelperSlotPlan slotPlan,
        FrameHandoffEmptySlotCallPlan? callPlan,
        uint nativeEsi,
        uint nativeEbx,
        uint nativeEdi,
        uint? publicationEcx,
        FrameHandoffSlotPublicationEntry expectedEntry,
        uint stopAt)
    {
        if (!capture.Publication.Entry.HasValue)
        {
            return Stop(
                capture,
                FrameHandoffSlotCallPublicationOutcome.MissingPublicationBranchEntry,
                slotPlan,
                callPlan,
                publicationPlan: null,
                nativeEsiReceiver: nativeEsi,
                nativeEbxSlotByteOffset: nativeEbx,
                nativeEdiWrappingReceiverPlus0C: nativeEdi,
                knownEcxAfterDirectCall: callPlan?.EcxAfterDirectCall,
                capturedZeroRoutePostCallEcx: callPlan?.CapturedZeroRoutePostCallEcx,
                unreachedPublicationIncomingEcxCaptureProvided:
                    capture.Publication.IncomingEcx.HasValue,
                unreachedPublicationStoreBase6508CaptureProvided:
                    capture.Publication.ReceiverPlus04At6508.HasValue,
                unreachedPublicationFreshBase650EProvided:
                    capture.Publication.ReceiverPlus04At650E.HasValue,
                unreachedPublicationSlotDwordProvided:
                    capture.Publication.SlotDwordAtEbxPlusFreshBase.HasValue,
                stopAt: stopAt);
        }

        if (capture.Publication.Entry.Value != expectedEntry)
        {
            return Stop(
                capture,
                FrameHandoffSlotCallPublicationOutcome.InvalidPublicationBranchEntry,
                slotPlan,
                callPlan,
                publicationPlan: null,
                nativeEsiReceiver: nativeEsi,
                nativeEbxSlotByteOffset: nativeEbx,
                nativeEdiWrappingReceiverPlus0C: nativeEdi,
                knownEcxAfterDirectCall: callPlan?.EcxAfterDirectCall,
                capturedZeroRoutePostCallEcx: callPlan?.CapturedZeroRoutePostCallEcx,
                unreachedPublicationIncomingEcxCaptureProvided:
                    capture.Publication.IncomingEcx.HasValue,
                unreachedPublicationStoreBase6508CaptureProvided:
                    capture.Publication.ReceiverPlus04At6508.HasValue,
                unreachedPublicationFreshBase650EProvided:
                    capture.Publication.ReceiverPlus04At650E.HasValue,
                unreachedPublicationSlotDwordProvided:
                    capture.Publication.SlotDwordAtEbxPlusFreshBase.HasValue,
                stopAt: stopAt);
        }

        bool receiverMatches = capture.Publication.ReceiverBaseEsi == nativeEsi;
        bool ebxMatches = capture.Publication.SlotArrayBaseEbx == nativeEbx;
        bool ediMatches = capture.Publication.IncomingEdi == nativeEdi;
        if (!receiverMatches)
        {
            return IdentityMismatch(
                FrameHandoffSlotCallPublicationOutcome.PublicationReceiverIdentityMismatch);
        }

        if (!ebxMatches)
        {
            return IdentityMismatch(
                FrameHandoffSlotCallPublicationOutcome.PublicationEbxIdentityMismatch);
        }

        if (!ediMatches)
        {
            return IdentityMismatch(
                FrameHandoffSlotCallPublicationOutcome.PublicationEdiIdentityMismatch);
        }

        FrameHandoffSlotPublicationCapture effectivePublication =
            capture.Publication with { IncomingEcx = publicationEcx };
        FrameHandoffSlotPublicationPlan publicationPlan =
            FrameHandoffSlotPublicationProjection.Project(effectivePublication);
        return Stop(
            capture,
            publicationPlan.Outcome == FrameHandoffSlotPublicationOutcome.ProjectionComplete
                ? FrameHandoffSlotCallPublicationOutcome.ProjectionComplete
                : FrameHandoffSlotCallPublicationOutcome.PublicationProjectionBlocked,
            slotPlan,
            callPlan,
            publicationPlan,
            nativeEsi,
            nativeEbx,
            nativeEdi,
            publicationEcx,
            publicationReceiverIdentityMatches: true,
            publicationEbxIdentityMatches: true,
            publicationEdiIdentityMatches: true,
            knownEcxAfterDirectCall: callPlan?.EcxAfterDirectCall,
            capturedZeroRoutePostCallEcx: callPlan?.CapturedZeroRoutePostCallEcx,
            unreachedPublicationFreshBase650EProvided:
                publicationPlan.UnreachedFreshBaseCaptureProvided,
            unreachedPublicationSlotDwordProvided:
                publicationPlan.UnreachedSlotDwordCaptureProvided,
            stopAt: publicationPlan.StopAtRawFileOffsetExclusive);

        FrameHandoffSlotCallPublicationPlan IdentityMismatch(
            FrameHandoffSlotCallPublicationOutcome outcome) =>
            Stop(
                capture,
                outcome,
                slotPlan,
                callPlan,
                publicationPlan: null,
                nativeEsiReceiver: nativeEsi,
                nativeEbxSlotByteOffset: nativeEbx,
                nativeEdiWrappingReceiverPlus0C: nativeEdi,
                knownEcxAfterDirectCall: callPlan?.EcxAfterDirectCall,
                capturedZeroRoutePostCallEcx: callPlan?.CapturedZeroRoutePostCallEcx,
                publicationReceiverIdentityMatches: receiverMatches,
                publicationEbxIdentityMatches: ebxMatches,
                publicationEdiIdentityMatches: ediMatches,
                unreachedPublicationIncomingEcxCaptureProvided:
                    capture.Publication.IncomingEcx.HasValue,
                unreachedPublicationStoreBase6508CaptureProvided:
                    capture.Publication.ReceiverPlus04At6508.HasValue,
                unreachedPublicationFreshBase650EProvided:
                    capture.Publication.ReceiverPlus04At650E.HasValue,
                unreachedPublicationSlotDwordProvided:
                    capture.Publication.SlotDwordAtEbxPlusFreshBase.HasValue,
                stopAt: expectedEntry == FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508
                    ? FrameHandoffSlotPublicationProjection.PublicationStoreEntryRawFileOffset
                    : FrameHandoffSlotPublicationProjection.ExistingNonzeroSlotBranchEntryRawFileOffset);
    }

    private static FrameHandoffSlotCallPublicationPlan Stop(
        FrameHandoffSlotCallPublicationCapture capture,
        FrameHandoffSlotCallPublicationOutcome outcome,
        FrameHandoffHelperSlotPlan slotPlan,
        FrameHandoffEmptySlotCallPlan? callPlan,
        FrameHandoffSlotPublicationPlan? publicationPlan,
        uint? nativeEsiReceiver = null,
        uint? nativeEbxSlotByteOffset = null,
        uint? nativeEdiWrappingReceiverPlus0C = null,
        uint? publicationEcx = null,
        bool publicationReceiverIdentityMatches = false,
        bool publicationEbxIdentityMatches = false,
        bool publicationEdiIdentityMatches = false,
        uint? knownEcxAfterDirectCall = null,
        uint? capturedZeroRoutePostCallEcx = null,
        bool unreachedCallEntryValidationProvided = false,
        bool unreachedDirectCallReturnCaptureProvided = false,
        bool unreachedDirectCallReturnEaxCaptureProvided = false,
        bool unreachedZeroRouteIndirectCallReturnCaptureProvided = false,
        bool unreachedZeroRoutePostCallEcxCaptureProvided = false,
        bool unreachedPublicationIncomingEcxCaptureProvided = false,
        bool unreachedPublicationStoreBase6508CaptureProvided = false,
        bool unreachedPublicationFreshBase650EProvided = false,
        bool unreachedPublicationSlotDwordProvided = false,
        uint stopAt = StopAtRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            slotPlan,
            callPlan,
            publicationPlan,
            nativeEsiReceiver,
            nativeEbxSlotByteOffset,
            nativeEdiWrappingReceiverPlus0C,
            knownEcxAfterDirectCall,
            capturedZeroRoutePostCallEcx,
            publicationEcx,
            publicationReceiverIdentityMatches,
            publicationEbxIdentityMatches,
            publicationEdiIdentityMatches,
            unreachedCallEntryValidationProvided,
            unreachedDirectCallReturnCaptureProvided,
            unreachedDirectCallReturnEaxCaptureProvided,
            unreachedZeroRouteIndirectCallReturnCaptureProvided,
            unreachedZeroRoutePostCallEcxCaptureProvided,
            unreachedPublicationIncomingEcxCaptureProvided,
            unreachedPublicationStoreBase6508CaptureProvided,
            unreachedPublicationFreshBase650EProvided,
            unreachedPublicationSlotDwordProvided,
            stopAt);
}
