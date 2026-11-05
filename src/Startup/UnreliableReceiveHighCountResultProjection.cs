using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveHighCountResultCapture(
    UnreliableReceiveResultStoresCapture ResultStores,
    UnreliableReceiveHighCountForwardCapture HighCountForward);

public enum UnreliableReceiveHighCountResultOutcome
{
    ResultStoresDidNotReachHighCountBoundary,
    HighCountBoundaryNotValidated,
    ReceiverIdentityMismatch,
    ForwardIncomplete,
    ForwardCompletedNormally
}

public readonly record struct UnreliableReceiveHighCountResultPlan(
    UnreliableReceiveHighCountResultCapture Capture,
    UnreliableReceiveResultStoresPlan ResultStoresPlan,
    UnreliableReceiveHighCountForwardPlan HighCountForwardPlan,
    UnreliableReceiveHighCountResultOutcome Outcome,
    bool CounterAtOrAboveThreshold,
    bool ReceiverIdentityMatches,
    bool HighCountBoundaryAdmitted,
    bool Complete,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveHighCountResultInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Composes the result stores with the validated high-count forwarding path.
/// </summary>
public static class UnreliableReceiveHighCountResultProjection
{
    public const string SamplePath = UnreliableReceiveResultStoresProjection.SamplePath;
    public const string SampleSha256 = UnreliableReceiveResultStoresProjection.SampleSha256;
    public const uint CodeVirtualAddressDeltaFromRawFileOffset =
        UnreliableReceiveResultStoresProjection.CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EntryRawFileOffset = UnreliableReceiveResultStoresProjection.EntryRawFileOffset;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint HighCountForwardEntryRawFileOffset =
        UnreliableReceiveHighCountForwardProjection.EntryRawFileOffset;
    public const uint EndRawFileOffsetExclusive =
        UnreliableReceiveHighCountForwardProjection.EndRawFileOffsetExclusive;
    public const uint EndVirtualAddressExclusive =
        EndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedWindowBytes =
        UnreliableReceiveResultStoresProjection.CapturedWindowBytes
        + " "
        + UnreliableReceiveHighCountForwardProjection.CapturedWindowBytes;

    public static ImmutableArray<UnreliableReceiveHighCountResultInstructionEvidence>
        Instructions { get; } =
        UnreliableReceiveResultStoresProjection.Instructions
            .Select(instruction =>
                new UnreliableReceiveHighCountResultInstructionEvidence(
                    instruction.Observation,
                    instruction.RawFileOffset,
                    instruction.VirtualAddress,
                    instruction.Bytes))
            .Concat(
                UnreliableReceiveHighCountForwardProjection.Evidence.Select(instruction =>
                    new UnreliableReceiveHighCountResultInstructionEvidence(
                        instruction.Observation,
                        instruction.RawFileOffset,
                        instruction.VirtualAddress,
                        instruction.Bytes)))
            .ToImmutableArray();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static UnreliableReceiveHighCountResultPlan Project(
        UnreliableReceiveHighCountResultCapture capture)
    {
        UnreliableReceiveResultStoresPlan resultStoresPlan =
            UnreliableReceiveResultStoresProjection.Project(capture.ResultStores);
        bool counterAtOrAboveThreshold =
            resultStoresPlan.Outcome
                == UnreliableReceiveResultStoresOutcome.CounterAtOrAboveThreshold;
        bool receiverIdentityMatches =
            counterAtOrAboveThreshold
            && capture.HighCountForward.ReceiverEdi == capture.ResultStores.ReceiverEdi;
        bool highCountBoundaryAdmitted =
            counterAtOrAboveThreshold
            && capture.HighCountForward.ValidatedHighCountBoundary
            && receiverIdentityMatches;

        UnreliableReceiveHighCountForwardPlan highCountForwardPlan =
            UnreliableReceiveHighCountForwardProjection.Project(
                capture.HighCountForward with
                {
                    ValidatedHighCountBoundary = highCountBoundaryAdmitted
                });

        UnreliableReceiveHighCountResultOutcome outcome =
            !counterAtOrAboveThreshold
                ? UnreliableReceiveHighCountResultOutcome.ResultStoresDidNotReachHighCountBoundary
                : !capture.HighCountForward.ValidatedHighCountBoundary
                    ? UnreliableReceiveHighCountResultOutcome.HighCountBoundaryNotValidated
                    : !receiverIdentityMatches
                        ? UnreliableReceiveHighCountResultOutcome.ReceiverIdentityMismatch
                        : highCountForwardPlan.NormalCallBoundaryReached
                            ? UnreliableReceiveHighCountResultOutcome.ForwardCompletedNormally
                            : UnreliableReceiveHighCountResultOutcome.ForwardIncomplete;

        bool complete =
            highCountBoundaryAdmitted
            && highCountForwardPlan.Outcome
                == UnreliableReceiveHighCountForwardOutcome.SecondHelperReturnedNormally
            && highCountForwardPlan.NormalCallBoundaryReached;

        uint stopAtRawFileOffsetExclusive =
            !counterAtOrAboveThreshold
                ? resultStoresPlan.StopAtRawFileOffsetExclusive
                : highCountBoundaryAdmitted
                    ? highCountForwardPlan.StopAtRawFileOffsetExclusive
                    : HighCountForwardEntryRawFileOffset;

        return new UnreliableReceiveHighCountResultPlan(
            capture,
            resultStoresPlan,
            highCountForwardPlan,
            outcome,
            counterAtOrAboveThreshold,
            receiverIdentityMatches,
            highCountBoundaryAdmitted,
            complete,
            complete ? EndRawFileOffsetExclusive : null,
            stopAtRawFileOffsetExclusive);
    }
}
