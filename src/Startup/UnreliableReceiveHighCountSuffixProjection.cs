using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveHighCountSuffixCapture(
    UnreliableReceiveHighCountCallbackCapture Callback,
    UnreliableReceiveHighCountCounterCapture Counter);

public enum UnreliableReceiveHighCountSuffixOutcome
{
    CallbackDidNotAdmitCounter,
    UnreachedCounterCaptureProvided,
    InvalidCounterEntry,
    MissingIndependentCounter4CReload,
    MissingCounter48ForNonzeroPostDecrement,
    Counter4CDecrementedAndCounter48Zeroed,
    Counter4CDecrementedAndCounter48Incremented
}

public readonly record struct UnreliableReceiveHighCountSuffixPlan(
    UnreliableReceiveHighCountSuffixCapture Capture,
    UnreliableReceiveHighCountCallbackPlan CallbackPlan,
    UnreliableReceiveHighCountSuffixOutcome Outcome,
    bool CounterEntryReached,
    UnreliableReceiveHighCountCounterPlan? CounterPlan,
    bool CounterJoinAdmitted,
    bool Complete,
    bool UnreachedCounterEntryValidationProvided,
    bool UnreachedIndependentCounter4CReloadCaptureProvided,
    bool UnreachedCounter48CaptureProvided,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveHighCountSuffixInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Composes the callback's normal boundary with the receiver counter update.
/// </summary>
public static class UnreliableReceiveHighCountSuffixProjection
{
    public const string SamplePath = UnreliableReceiveHighCountCallbackProjection.SamplePath;
    public const string SampleSha256 = UnreliableReceiveHighCountCallbackProjection.SampleSha256;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        UnreliableReceiveHighCountCallbackProjection.CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EntryRawFileOffset =
        UnreliableReceiveHighCountCallbackProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        UnreliableReceiveHighCountCallbackProjection.EntryVirtualAddress;
    public const uint CallbackBoundaryRawFileOffset =
        UnreliableReceiveHighCountCallbackProjection.NormalBoundaryRawFileOffset;
    public const uint CounterJoinRawFileOffset =
        UnreliableReceiveHighCountCounterProjection.JoinRawFileOffset;
    public const uint EndRawFileOffsetExclusive = CounterJoinRawFileOffset;
    public const uint EndVirtualAddressExclusive =
        EndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedWindowBytes =
        UnreliableReceiveHighCountCallbackProjection.CapturedWindowBytes
        + " "
        + UnreliableReceiveHighCountCounterProjection.CapturedWindowBytes;

    public static ImmutableArray<UnreliableReceiveHighCountSuffixInstructionEvidence>
        Instructions { get; } =
        UnreliableReceiveHighCountCallbackProjection.Instructions
            .Select(instruction =>
                new UnreliableReceiveHighCountSuffixInstructionEvidence(
                    instruction.Observation,
                    instruction.RawFileOffset,
                    instruction.VirtualAddress,
                    instruction.Bytes))
            .Concat(
                UnreliableReceiveHighCountCounterProjection.Instructions.Select(instruction =>
                    new UnreliableReceiveHighCountSuffixInstructionEvidence(
                        instruction.Observation,
                        instruction.RawFileOffset,
                        instruction.VirtualAddress,
                        instruction.Bytes)))
            .ToImmutableArray();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static UnreliableReceiveHighCountSuffixPlan Project(
        UnreliableReceiveHighCountSuffixCapture capture)
    {
        UnreliableReceiveHighCountCallbackPlan callbackPlan =
            UnreliableReceiveHighCountCallbackProjection.Project(capture.Callback);

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
        bool callbackAdmittedCounter =
            (cleanZeroObjectBranch || normalIndirectCallbackReturn)
            && callbackPlan.SelectedDestinationRawFileOffset
                == CallbackBoundaryRawFileOffset
            && callbackPlan.StopAtRawFileOffsetExclusive
                == CallbackBoundaryRawFileOffset;

        if (!callbackAdmittedCounter)
        {
            bool unreachedEntryValidation = capture.Counter.ValidatedEntry;
            bool unreachedCounter4CCapture =
                capture.Counter.IndependentCounter4CReload.HasValue;
            bool unreachedCounter48Capture =
                capture.Counter.Counter48IfNonzeroPostDecrement.HasValue;
            bool anyUnreachedCounterCapture =
                unreachedEntryValidation
                || unreachedCounter4CCapture
                || unreachedCounter48Capture;

            return new UnreliableReceiveHighCountSuffixPlan(
                capture,
                callbackPlan,
                anyUnreachedCounterCapture
                    ? UnreliableReceiveHighCountSuffixOutcome.UnreachedCounterCaptureProvided
                    : UnreliableReceiveHighCountSuffixOutcome.CallbackDidNotAdmitCounter,
                CounterEntryReached: false,
                CounterPlan: null,
                CounterJoinAdmitted: false,
                Complete: false,
                unreachedEntryValidation,
                unreachedCounter4CCapture,
                unreachedCounter48Capture,
                callbackPlan.SelectedDestinationRawFileOffset,
                callbackPlan.StopAtRawFileOffsetExclusive);
        }

        UnreliableReceiveHighCountCounterPlan counterPlan =
            UnreliableReceiveHighCountCounterProjection.Project(capture.Counter);
        bool unreachedCounter4C =
            counterPlan.UnreachedIndependentCounter4CReloadCaptureProvided;
        bool unreachedCounter48 = counterPlan.UnreachedCounter48CaptureProvided;
        bool unreachedCounterCapture = unreachedCounter4C || unreachedCounter48;
        bool counterJoinAdmitted =
            !unreachedCounterCapture
            && counterPlan.StopAtRawFileOffsetExclusive == CounterJoinRawFileOffset;
        return new UnreliableReceiveHighCountSuffixPlan(
            capture,
            callbackPlan,
            unreachedCounterCapture
                ? UnreliableReceiveHighCountSuffixOutcome.UnreachedCounterCaptureProvided
                : MapOutcome(counterPlan.Outcome),
            CounterEntryReached: true,
            counterPlan,
            counterJoinAdmitted,
            Complete: counterJoinAdmitted
                && (counterPlan.Outcome
                        == UnreliableReceiveHighCountCounterOutcome
                            .Counter4CDecrementedAndCounter48Zeroed
                    || counterPlan.Outcome
                        == UnreliableReceiveHighCountCounterOutcome
                            .Counter4CDecrementedAndCounter48Incremented),
            UnreachedCounterEntryValidationProvided: false,
            UnreachedIndependentCounter4CReloadCaptureProvided: unreachedCounter4C,
            UnreachedCounter48CaptureProvided: unreachedCounter48,
            counterJoinAdmitted ? CounterJoinRawFileOffset : null,
            counterPlan.StopAtRawFileOffsetExclusive);
    }

    private static UnreliableReceiveHighCountSuffixOutcome MapOutcome(
        UnreliableReceiveHighCountCounterOutcome outcome) =>
        outcome switch
        {
            UnreliableReceiveHighCountCounterOutcome.InvalidEntry =>
                UnreliableReceiveHighCountSuffixOutcome.InvalidCounterEntry,
            UnreliableReceiveHighCountCounterOutcome.MissingIndependentCounter4CReload =>
                UnreliableReceiveHighCountSuffixOutcome.MissingIndependentCounter4CReload,
            UnreliableReceiveHighCountCounterOutcome.MissingCounter48ForNonzeroPostDecrement =>
                UnreliableReceiveHighCountSuffixOutcome.MissingCounter48ForNonzeroPostDecrement,
            UnreliableReceiveHighCountCounterOutcome.Counter4CDecrementedAndCounter48Zeroed =>
                UnreliableReceiveHighCountSuffixOutcome.Counter4CDecrementedAndCounter48Zeroed,
            UnreliableReceiveHighCountCounterOutcome.Counter4CDecrementedAndCounter48Incremented =>
                UnreliableReceiveHighCountSuffixOutcome.Counter4CDecrementedAndCounter48Incremented,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
        };
}
