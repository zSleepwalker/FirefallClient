using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryFsPublicationCapture(
    NetworkDispatcherEntryCookiePreparationCapture ParentCookiePreparationCapture);

public enum NetworkDispatcherEntryFsPublicationOutcome
{
    ParentCookiePreparationNotAccepted,
    FsPublicationProjected
}

public readonly record struct NetworkDispatcherEntryFsPublicationAddressCalculation(
    uint RawFileOffset,
    uint? BaseEBP,
    uint Displacement,
    uint? ResultEAX,
    bool Attempted,
    bool Completed);

public readonly record struct NetworkDispatcherEntryFsPublicationWrite(
    uint RawFileOffset,
    NetworkDispatcherEntryFramePrefixSegmentRegister SegmentRegister,
    uint SegmentOffset,
    uint? Value,
    bool Attempted,
    bool Committed);

public sealed record NetworkDispatcherEntryFsPublicationPlan(
    NetworkDispatcherEntryFsPublicationCapture Capture,
    NetworkDispatcherEntryCookiePreparationPlan ParentCookiePreparationPlan,
    NetworkDispatcherEntryFsPublicationOutcome Outcome,
    ImmutableArray<uint> CompletedInstructionOffsets,
    NetworkDispatcherEntryFramePrefixSegmentRead? PreviousFSOffset0Read,
    NetworkDispatcherEntryFsPublicationAddressCalculation? AddressCalculation,
    NetworkDispatcherEntryFsPublicationWrite? PublicationWrite,
    uint? EntryEBP,
    uint? EntryEAX,
    uint? ESPBefore,
    uint? CurrentESPAtStop,
    NetworkIoCompletionClockGateFlags? FlagsBefore,
    NetworkIoCompletionClockGateFlags? FlagsAtStop,
    uint StopAtRawFileOffsetExclusive)
{
    public bool ExactFsPublicationProjected =>
        Outcome == NetworkDispatcherEntryFsPublicationOutcome.FsPublicationProjected
        && StopAtRawFileOffsetExclusive
            == NetworkDispatcherEntryFsPublicationProjection.PrefixEndRawFileOffsetExclusive;

    public bool StackPointerUnchanged =>
        ExactFsPublicationProjected && ESPBefore == CurrentESPAtStop;

    public bool FlagsUnchanged =>
        ExactFsPublicationProjected && FlagsBefore == FlagsAtStop;

    public bool NativeInstructionsExecuted => false;

    public bool ExceptionABIInferred => false;

    public bool LinearAddressInterpretationMade => false;
}

/// <summary>
/// Reprojects LEA EAX,[EBP-0Ch] and MOV FS:[0],EAX after the accepted cookie
/// preparation, without executing native instructions or treating FS:[0] as a
/// linear address.
/// </summary>
public static class NetworkDispatcherEntryFsPublicationProjection
{
    public const uint EntryRawFileOffset = 0x00BBDDCF;
    public const uint AddressCalculationRawFileOffset = 0x00BBDDCF;
    public const uint PublishFSOffset0RawFileOffset = 0x00BBDDD2;
    public const uint PrefixEndRawFileOffsetExclusive = 0x00BBDDD8;
    public const uint LocalAllocationDisplacement = 0x0C;
    public const uint FSChainHeadSegmentOffset = 0;

    public const string CapturedWindowBytes =
        "8D 45 F4 64 A3 00 00 00 00 8B F1";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFsPublicationPlan Project(
        NetworkDispatcherEntryFsPublicationCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentCookiePreparationCapture);

        NetworkDispatcherEntryCookiePreparationPlan parent =
            NetworkDispatcherEntryCookiePreparationProjection.Project(
                capture.ParentCookiePreparationCapture);
        NetworkDispatcherEntryFramePrefixSegmentRead? previousFSRead =
            parent.ParentRegisterSavePlan.ParentPlan.FSOffset0Read;

        if (!parent.ExactCookiePreparationProjected
            || parent.EntryEBP is not uint entryEBP
            || parent.CurrentESPAtStop is not uint currentESP)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFsPublicationOutcome.ParentCookiePreparationNotAccepted,
                ImmutableArray<uint>.Empty,
                previousFSRead,
                null,
                null,
                parent.EntryEBP,
                null,
                parent.CurrentESPAtStop,
                parent.CurrentESPAtStop,
                parent.FlagsAtStop,
                parent.FlagsAtStop,
                parent.StopAtRawFileOffsetExclusive);
        }

        uint address = unchecked(entryEBP - LocalAllocationDisplacement);
        var addressCalculation = new NetworkDispatcherEntryFsPublicationAddressCalculation(
            AddressCalculationRawFileOffset,
            entryEBP,
            LocalAllocationDisplacement,
            address,
            Attempted: true,
            Completed: true);
        var publicationWrite = new NetworkDispatcherEntryFsPublicationWrite(
            PublishFSOffset0RawFileOffset,
            NetworkDispatcherEntryFramePrefixSegmentRegister.FS,
            FSChainHeadSegmentOffset,
            address,
            Attempted: true,
            Committed: true);

        return new(
            capture,
            parent,
            NetworkDispatcherEntryFsPublicationOutcome.FsPublicationProjected,
            ImmutableArray.Create(
                AddressCalculationRawFileOffset,
                PublishFSOffset0RawFileOffset),
            previousFSRead,
            addressCalculation,
            publicationWrite,
            entryEBP,
            address,
            currentESP,
            currentESP,
            parent.FlagsAtStop,
            parent.FlagsAtStop,
            PrefixEndRawFileOffsetExclusive);
    }
}
