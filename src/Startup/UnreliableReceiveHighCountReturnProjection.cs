using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveHighCountReturnCapture(
    UnreliableReceiveHighCountSuffixCapture Suffix,
    uint? FrameEbp,
    bool? HelperReturnedNormally,
    uint? HelperReturnEax);

public enum UnreliableReceiveHighCountReturnOutcome
{
    SuffixDidNotComplete,
    MissingFrameEbpCapture,
    HandoffDidNotReturnNormally,
    ReturnReached
}

public readonly record struct UnreliableReceiveHighCountReturnPlan(
    UnreliableReceiveHighCountReturnCapture Capture,
    UnreliableReceiveHighCountSuffixPlan SuffixPlan,
    UnreliableReceiveHighCountReturnOutcome Outcome,
    bool HandoffEntryAdmitted,
    bool FrameEbpCaptureProvided,
    UnreliableReceiveFrameHandoffCapture? HandoffCapture,
    UnreliableReceiveFrameHandoffPlan? HandoffPlan,
    bool UnreachedFrameEbpCaptureProvided,
    bool UnreachedHelperReturnedNormallyCaptureProvided,
    bool UnreachedHelperReturnEaxCaptureProvided,
    bool Complete,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveHighCountReturnInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Composes the completed high-count suffix with the validated frame handoff and return.
/// </summary>
public static class UnreliableReceiveHighCountReturnProjection
{
    public const string SamplePath = UnreliableReceiveHighCountSuffixProjection.SamplePath;
    public const string SampleSha256 = UnreliableReceiveHighCountSuffixProjection.SampleSha256;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        UnreliableReceiveHighCountSuffixProjection.CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EntryRawFileOffset =
        UnreliableReceiveHighCountSuffixProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        UnreliableReceiveHighCountSuffixProjection.EntryVirtualAddress;
    public const uint FrameHandoffEntryRawFileOffset =
        UnreliableReceiveFrameHandoffProjection.EntryRawFileOffset;
    public const uint EndRawFileOffsetExclusive =
        UnreliableReceiveFrameHandoffProjection.EndRawFileOffsetExclusive;
    public const uint EndVirtualAddressExclusive =
        EndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedWindowBytes =
        UnreliableReceiveHighCountSuffixProjection.CapturedWindowBytes
        + " "
        + UnreliableReceiveFrameHandoffProjection.CapturedWindowBytes;

    public static ImmutableArray<UnreliableReceiveHighCountReturnInstructionEvidence>
        Instructions { get; } =
        UnreliableReceiveHighCountSuffixProjection.Instructions
            .Select(instruction =>
                new UnreliableReceiveHighCountReturnInstructionEvidence(
                    instruction.Observation,
                    instruction.RawFileOffset,
                    instruction.VirtualAddress,
                    instruction.Bytes))
            .Concat(
                UnreliableReceiveFrameHandoffProjection.Evidence.Select(instruction =>
                    new UnreliableReceiveHighCountReturnInstructionEvidence(
                        instruction.Observation,
                        instruction.RawFileOffset,
                        instruction.VirtualAddress,
                        instruction.Bytes)))
            .ToImmutableArray();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static UnreliableReceiveHighCountReturnPlan Project(
        UnreliableReceiveHighCountReturnCapture capture)
    {
        UnreliableReceiveHighCountSuffixPlan suffixPlan =
            UnreliableReceiveHighCountSuffixProjection.Project(capture.Suffix);

        if (!SuffixAdmittedHandoff(suffixPlan))
        {
            return new UnreliableReceiveHighCountReturnPlan(
                capture,
                suffixPlan,
                UnreliableReceiveHighCountReturnOutcome.SuffixDidNotComplete,
                HandoffEntryAdmitted: false,
                FrameEbpCaptureProvided: capture.FrameEbp.HasValue,
                HandoffCapture: null,
                HandoffPlan: null,
                UnreachedFrameEbpCaptureProvided: capture.FrameEbp.HasValue,
                UnreachedHelperReturnedNormallyCaptureProvided:
                    capture.HelperReturnedNormally.HasValue,
                UnreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue,
                Complete: false,
                suffixPlan.StopAtRawFileOffsetExclusive);
        }

        if (capture.FrameEbp is not uint frameEbp)
        {
            return new UnreliableReceiveHighCountReturnPlan(
                capture,
                suffixPlan,
                UnreliableReceiveHighCountReturnOutcome.MissingFrameEbpCapture,
                HandoffEntryAdmitted: true,
                FrameEbpCaptureProvided: false,
                HandoffCapture: null,
                HandoffPlan: null,
                UnreachedFrameEbpCaptureProvided: false,
                UnreachedHelperReturnedNormallyCaptureProvided:
                    capture.HelperReturnedNormally.HasValue,
                UnreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue,
                Complete: false,
                FrameHandoffEntryRawFileOffset);
        }

        UnreliableReceiveHighCountCounterCapture counterCapture =
            suffixPlan.CounterPlan!.Value.Capture;
        var handoffCapture = new UnreliableReceiveFrameHandoffCapture(
            ValidatedEntry: suffixPlan.Complete && suffixPlan.CounterJoinAdmitted,
            FrameEbp: frameEbp,
            ReceiverEdi: counterCapture.ReceiverEdi,
            HelperReturnedNormally: capture.HelperReturnedNormally,
            HelperReturnEax: capture.HelperReturnEax);
        UnreliableReceiveFrameHandoffPlan handoffPlan =
            UnreliableReceiveFrameHandoffProjection.Project(handoffCapture);
        bool complete =
            handoffPlan.Outcome == UnreliableReceiveFrameHandoffOutcome.ReturnReached;

        return new UnreliableReceiveHighCountReturnPlan(
            capture,
            suffixPlan,
            complete
                ? UnreliableReceiveHighCountReturnOutcome.ReturnReached
                : UnreliableReceiveHighCountReturnOutcome.HandoffDidNotReturnNormally,
            HandoffEntryAdmitted: true,
            FrameEbpCaptureProvided: true,
            handoffCapture,
            handoffPlan,
            UnreachedFrameEbpCaptureProvided: false,
            UnreachedHelperReturnedNormallyCaptureProvided: false,
            UnreachedHelperReturnEaxCaptureProvided:
                handoffPlan.UnreachedHelperReturnEaxCaptureProvided,
            complete,
            handoffPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool SuffixAdmittedHandoff(UnreliableReceiveHighCountSuffixPlan suffixPlan)
    {
        if (!suffixPlan.Complete
            || !suffixPlan.CounterJoinAdmitted
            || suffixPlan.UnreachedCounterEntryValidationProvided
            || suffixPlan.UnreachedIndependentCounter4CReloadCaptureProvided
            || suffixPlan.UnreachedCounter48CaptureProvided
            || suffixPlan.CounterPlan is not UnreliableReceiveHighCountCounterPlan counterPlan
            || counterPlan.UnreachedIndependentCounter4CReloadCaptureProvided
            || counterPlan.UnreachedCounter48CaptureProvided
            || suffixPlan.CallbackPlan.UnreachedHelperReturnEaxCaptureProvided
            || suffixPlan.CallbackPlan.UnreachedFirstLoadedDwordCaptureProvided
            || suffixPlan.CallbackPlan.UnreachedSecondLoadedDwordCaptureProvided
            || suffixPlan.CallbackPlan.UnreachedIndirectTargetDwordCaptureProvided
            || suffixPlan.CallbackPlan.UnreachedCallbackNormalCaptureProvided)
        {
            return false;
        }

        return suffixPlan.Outcome
                is UnreliableReceiveHighCountSuffixOutcome
                    .Counter4CDecrementedAndCounter48Zeroed
                    or UnreliableReceiveHighCountSuffixOutcome
                    .Counter4CDecrementedAndCounter48Incremented
            && counterPlan.Outcome
                is UnreliableReceiveHighCountCounterOutcome
                    .Counter4CDecrementedAndCounter48Zeroed
                    or UnreliableReceiveHighCountCounterOutcome
                    .Counter4CDecrementedAndCounter48Incremented
            && counterPlan.StopAtRawFileOffsetExclusive
                == UnreliableReceiveHighCountSuffixProjection.CounterJoinRawFileOffset;
    }
}
