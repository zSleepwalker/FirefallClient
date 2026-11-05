namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffPublicationReturnCapture(
    FrameHandoffSlotPublicationCapture Publication,
    FrameHandoffFinalWriteReturnCapture Final);

public enum FrameHandoffPublicationReturnOutcome
{
    PublicationProjectionBlocked,
    PublicationHasUnreachedCaptures,
    FinalEntryEcxMismatch,
    FinalEntryEsiMismatch,
    FinalProjectionBlocked,
    ProjectionComplete
}

public readonly record struct FrameHandoffPublicationReturnPlan(
    FrameHandoffPublicationReturnCapture Capture,
    FrameHandoffPublicationReturnOutcome Outcome,
    FrameHandoffSlotPublicationPlan PublicationPlan,
    FrameHandoffFinalWriteReturnPlan? FinalPlan,
    bool FinalEntryEcxMatchesPublication,
    bool FinalEntryEsiMatchesPublication,
    bool UnreachedPublicationStoreBaseCaptureProvided,
    bool UnreachedPublicationEcxCaptureProvided,
    bool UnreachedPublicationFreshBaseCaptureProvided,
    bool UnreachedPublicationSlotDwordCaptureProvided,
    bool UnreachedFinalFrame8PointerCaptureProvided,
    bool UnreachedFinalPointeeDwordCaptureProvided,
    bool UnreachedFinalReceiver10OldValueCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Composes the slot publication through its ECX test with the final write and return.
/// The final entry EBP is a fresh independent capture; no earlier frame identity is inferred.
/// </summary>
public static class FrameHandoffPublicationReturnProjection
{
    public const uint StopAtRawFileOffsetExclusive = 0x00BE652F;

    public static FrameHandoffPublicationReturnPlan Project(
        FrameHandoffPublicationReturnCapture capture)
    {
        FrameHandoffSlotPublicationPlan publicationPlan =
            FrameHandoffSlotPublicationProjection.Project(capture.Publication);
        bool unreachedStoreBase = publicationPlan.UnreachedStoreBaseCaptureProvided;
        bool unreachedPublicationEcx = publicationPlan.UnreachedEcxCaptureProvided;
        bool unreachedFreshBase = publicationPlan.UnreachedFreshBaseCaptureProvided;
        bool unreachedSlotDword = publicationPlan.UnreachedSlotDwordCaptureProvided;

        if (publicationPlan.Outcome != FrameHandoffSlotPublicationOutcome.ProjectionComplete
            || publicationPlan.ComputedEcxBeforeTest is not uint publicationEcx)
        {
            return Stop(
                capture,
                FrameHandoffPublicationReturnOutcome.PublicationProjectionBlocked,
                publicationPlan,
                unreachedStoreBase,
                unreachedPublicationEcx,
                unreachedFreshBase,
                unreachedSlotDword,
                unreachedFinalFrame8Pointer:
                    capture.Final.Frame8Pointer.HasValue,
                unreachedFinalPointeeDword:
                    capture.Final.PointeeDword.HasValue,
                unreachedFinalReceiver10OldValue:
                    capture.Final.Receiver10OldValue.HasValue,
                stopAt: publicationPlan.StopAtRawFileOffsetExclusive);
        }

        if (unreachedStoreBase
            || unreachedPublicationEcx
            || unreachedFreshBase
            || unreachedSlotDword)
        {
            return Stop(
                capture,
                FrameHandoffPublicationReturnOutcome.PublicationHasUnreachedCaptures,
                publicationPlan,
                unreachedStoreBase,
                unreachedPublicationEcx,
                unreachedFreshBase,
                unreachedSlotDword,
                unreachedFinalFrame8Pointer:
                    capture.Final.Frame8Pointer.HasValue,
                unreachedFinalPointeeDword:
                    capture.Final.PointeeDword.HasValue,
                unreachedFinalReceiver10OldValue:
                    capture.Final.Receiver10OldValue.HasValue,
                stopAt: publicationPlan.StopAtRawFileOffsetExclusive);
        }

        bool ecxMatches = capture.Final.EntryEcx == publicationEcx;
        bool esiMatches =
            capture.Final.EntryEsi == publicationPlan.Capture.ReceiverBaseEsi;
        if (!ecxMatches)
        {
            return Stop(
                capture,
                FrameHandoffPublicationReturnOutcome.FinalEntryEcxMismatch,
                publicationPlan,
                finalEntryEcxMatchesPublication: false,
                finalEntryEsiMatchesPublication: esiMatches,
                unreachedFinalFrame8Pointer:
                    capture.Final.Frame8Pointer.HasValue,
                unreachedFinalPointeeDword:
                    capture.Final.PointeeDword.HasValue,
                unreachedFinalReceiver10OldValue:
                    capture.Final.Receiver10OldValue.HasValue,
                stopAt: FrameHandoffFinalWriteReturnProjection.EntryRawFileOffset);
        }

        if (!esiMatches)
        {
            return Stop(
                capture,
                FrameHandoffPublicationReturnOutcome.FinalEntryEsiMismatch,
                publicationPlan,
                finalEntryEcxMatchesPublication: true,
                finalEntryEsiMatchesPublication: false,
                unreachedFinalFrame8Pointer:
                    capture.Final.Frame8Pointer.HasValue,
                unreachedFinalPointeeDword:
                    capture.Final.PointeeDword.HasValue,
                unreachedFinalReceiver10OldValue:
                    capture.Final.Receiver10OldValue.HasValue,
                stopAt: FrameHandoffFinalWriteReturnProjection.EntryRawFileOffset);
        }

        FrameHandoffFinalWriteReturnPlan finalPlan =
            FrameHandoffFinalWriteReturnProjection.Project(capture.Final);
        return Stop(
            capture,
            finalPlan.Outcome == FrameHandoffFinalWriteReturnOutcome.ReturnedNormally
                ? FrameHandoffPublicationReturnOutcome.ProjectionComplete
                : FrameHandoffPublicationReturnOutcome.FinalProjectionBlocked,
            publicationPlan,
            finalPlan: finalPlan,
            finalEntryEcxMatchesPublication: true,
            finalEntryEsiMatchesPublication: true,
            unreachedFinalFrame8Pointer:
                finalPlan.UnreachedFrame8PointerCaptureProvided,
            unreachedFinalPointeeDword:
                finalPlan.UnreachedPointeeDwordCaptureProvided,
            unreachedFinalReceiver10OldValue:
                finalPlan.UnreachedReceiver10OldValueCaptureProvided,
            stopAt: finalPlan.StopAtRawFileOffsetExclusive);
    }

    private static FrameHandoffPublicationReturnPlan Stop(
        FrameHandoffPublicationReturnCapture capture,
        FrameHandoffPublicationReturnOutcome outcome,
        FrameHandoffSlotPublicationPlan publicationPlan,
        bool unreachedStoreBase = false,
        bool unreachedPublicationEcx = false,
        bool unreachedFreshBase = false,
        bool unreachedSlotDword = false,
        FrameHandoffFinalWriteReturnPlan? finalPlan = null,
        bool finalEntryEcxMatchesPublication = false,
        bool finalEntryEsiMatchesPublication = false,
        bool unreachedFinalFrame8Pointer = false,
        bool unreachedFinalPointeeDword = false,
        bool unreachedFinalReceiver10OldValue = false,
        uint stopAt = StopAtRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            publicationPlan,
            finalPlan,
            finalEntryEcxMatchesPublication,
            finalEntryEsiMatchesPublication,
            unreachedStoreBase,
            unreachedPublicationEcx,
            unreachedFreshBase,
            unreachedSlotDword,
            unreachedFinalFrame8Pointer,
            unreachedFinalPointeeDword,
            unreachedFinalReceiver10OldValue,
            stopAt);
}
