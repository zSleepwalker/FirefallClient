using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveHighCountLifecycleCapture(
    UnreliableReceiveHighCountResultCapture Result,
    UnreliableReceiveHighCountReturnCapture Return);

public enum UnreliableReceiveHighCountLifecycleOutcome
{
    ResultDidNotCompleteNormally,
    CounterReceiverIdentityMismatch,
    ReturnDidNotComplete,
    ReturnCompletedNormally
}

public readonly record struct UnreliableReceiveHighCountLifecyclePlan(
    UnreliableReceiveHighCountLifecycleCapture Capture,
    UnreliableReceiveHighCountResultPlan ResultPlan,
    UnreliableReceiveHighCountReturnPlan ReturnPlan,
    UnreliableReceiveHighCountLifecycleOutcome Outcome,
    bool ResultForwardCompletedNormally,
    bool CallbackEntryAdmitted,
    bool CounterReceiverIdentityMatches,
    bool CounterIdentityMismatchBlockedEntry,
    bool CounterEntryAdmitted,
    bool Complete,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveHighCountLifecycleInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Composes the result forwarding boundary with the callback, counter update, and normal return.
/// </summary>
public static class UnreliableReceiveHighCountLifecycleProjection
{
    public const string SamplePath = UnreliableReceiveHighCountResultProjection.SamplePath;
    public const string SampleSha256 = UnreliableReceiveHighCountResultProjection.SampleSha256;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        UnreliableReceiveHighCountResultProjection.CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EntryRawFileOffset =
        UnreliableReceiveHighCountResultProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EndRawFileOffsetExclusive =
        UnreliableReceiveHighCountReturnProjection.EndRawFileOffsetExclusive;
    public const uint EndVirtualAddressExclusive =
        EndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedWindowBytes =
        UnreliableReceiveHighCountResultProjection.CapturedWindowBytes
        + " "
        + UnreliableReceiveHighCountReturnProjection.CapturedWindowBytes;

    public static ImmutableArray<UnreliableReceiveHighCountLifecycleInstructionEvidence>
        Instructions { get; } =
        UnreliableReceiveHighCountResultProjection.Instructions
            .Select(instruction =>
                new UnreliableReceiveHighCountLifecycleInstructionEvidence(
                    instruction.Observation,
                    instruction.RawFileOffset,
                    instruction.VirtualAddress,
                    instruction.Bytes))
            .Concat(
                UnreliableReceiveHighCountReturnProjection.Instructions.Select(instruction =>
                    new UnreliableReceiveHighCountLifecycleInstructionEvidence(
                        instruction.Observation,
                        instruction.RawFileOffset,
                        instruction.VirtualAddress,
                        instruction.Bytes)))
            .ToImmutableArray();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static UnreliableReceiveHighCountLifecyclePlan Project(
        UnreliableReceiveHighCountLifecycleCapture capture)
    {
        UnreliableReceiveHighCountResultPlan resultPlan =
            UnreliableReceiveHighCountResultProjection.Project(capture.Result);
        bool resultForwardCompletedNormally =
            resultPlan.Outcome == UnreliableReceiveHighCountResultOutcome.ForwardCompletedNormally
            && resultPlan.Complete
            && resultPlan.HighCountBoundaryAdmitted
            && resultPlan.HighCountForwardPlan.Outcome
                == UnreliableReceiveHighCountForwardOutcome.SecondHelperReturnedNormally
            && resultPlan.HighCountForwardPlan.SecondHelperReturnedNormally == true
            && resultPlan.HighCountForwardPlan.NormalCallBoundaryReached;

        UnreliableReceiveHighCountCallbackCapture callbackCapture =
            capture.Return.Suffix.Callback with
            {
                PriorBE6440Normal = resultForwardCompletedNormally
            };
        UnreliableReceiveHighCountCallbackPlan callbackPlan =
            UnreliableReceiveHighCountCallbackProjection.Project(callbackCapture);
        bool callbackEntryAdmitted = CallbackAdmitsCounter(callbackPlan);

        uint resultReceiverEdi = capture.Result.HighCountForward.ReceiverEdi;
        uint counterReceiverEdi = capture.Return.Suffix.Counter.ReceiverEdi;
        bool counterReceiverIdentityMatches = resultReceiverEdi == counterReceiverEdi;
        bool counterIdentityMismatchBlockedEntry =
            resultForwardCompletedNormally
            && callbackEntryAdmitted
            && !counterReceiverIdentityMatches;

        UnreliableReceiveHighCountReturnCapture returnCapture =
            capture.Return with
            {
                Suffix = capture.Return.Suffix with
                {
                    Callback = callbackCapture,
                    Counter = capture.Return.Suffix.Counter with
                    {
                        ValidatedEntry =
                            capture.Return.Suffix.Counter.ValidatedEntry
                            && (!callbackEntryAdmitted || counterReceiverIdentityMatches)
                    }
                }
            };
        UnreliableReceiveHighCountReturnPlan returnPlan =
            UnreliableReceiveHighCountReturnProjection.Project(returnCapture);

        bool counterEntryAdmitted =
            callbackEntryAdmitted
            && counterReceiverIdentityMatches
            && returnPlan.SuffixPlan.CounterEntryReached
            && returnPlan.SuffixPlan.CounterPlan is UnreliableReceiveHighCountCounterPlan counterPlan
            && counterPlan.Capture.ValidatedEntry;
        bool complete =
            resultForwardCompletedNormally
            && returnPlan.Outcome == UnreliableReceiveHighCountReturnOutcome.ReturnReached
            && returnPlan.Complete;
        UnreliableReceiveHighCountLifecycleOutcome outcome =
            !resultForwardCompletedNormally
                ? UnreliableReceiveHighCountLifecycleOutcome.ResultDidNotCompleteNormally
                : counterIdentityMismatchBlockedEntry
                    ? UnreliableReceiveHighCountLifecycleOutcome.CounterReceiverIdentityMismatch
                    : complete
                        ? UnreliableReceiveHighCountLifecycleOutcome.ReturnCompletedNormally
                        : UnreliableReceiveHighCountLifecycleOutcome.ReturnDidNotComplete;

        uint stopAtRawFileOffsetExclusive =
            !resultForwardCompletedNormally
                ? resultPlan.StopAtRawFileOffsetExclusive
                : returnPlan.StopAtRawFileOffsetExclusive;

        return new UnreliableReceiveHighCountLifecyclePlan(
            capture,
            resultPlan,
            returnPlan,
            outcome,
            resultForwardCompletedNormally,
            callbackEntryAdmitted,
            counterReceiverIdentityMatches,
            counterIdentityMismatchBlockedEntry,
            counterEntryAdmitted,
            complete,
            complete ? EndRawFileOffsetExclusive : null,
            stopAtRawFileOffsetExclusive);
    }

    private static bool CallbackAdmitsCounter(UnreliableReceiveHighCountCallbackPlan callbackPlan)
    {
        bool cleanZeroObjectBranch =
            callbackPlan.Outcome == UnreliableReceiveHighCountCallbackOutcome.ZeroObjectBranch
            && callbackPlan.ZeroObjectBranchTaken
            && callbackPlan.NormalBoundaryAdmitted
            && !callbackPlan.SecondReadReached
            && !callbackPlan.PushOneReached
            && !callbackPlan.IndirectCallReached;
        bool normalIndirectCallbackReturn =
            callbackPlan.Outcome
                == UnreliableReceiveHighCountCallbackOutcome.CallbackReturnedNormally
            && callbackPlan.IndirectCallReached
            && callbackPlan.CallbackReturnedNormally == true
            && callbackPlan.NormalBoundaryAdmitted;

        return (cleanZeroObjectBranch || normalIndirectCallbackReturn)
            && !callbackPlan.UnreachedHelperReturnEaxCaptureProvided
            && !callbackPlan.UnreachedFirstLoadedDwordCaptureProvided
            && !callbackPlan.UnreachedSecondLoadedDwordCaptureProvided
            && !callbackPlan.UnreachedIndirectTargetDwordCaptureProvided
            && !callbackPlan.UnreachedCallbackNormalCaptureProvided
            && callbackPlan.SelectedDestinationRawFileOffset
                == UnreliableReceiveHighCountSuffixProjection.CallbackBoundaryRawFileOffset
            && callbackPlan.StopAtRawFileOffsetExclusive
                == UnreliableReceiveHighCountSuffixProjection.CallbackBoundaryRawFileOffset;
    }
}
