using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum NetworkDispatcherEntryRegisterSaveOutcome
{
    ParentPlanMismatch,
    ParentPrefixNotAccepted,
    MissingFreshEntryEBX,
    MissingFreshEntryESI,
    MissingFreshEntryEDI,
    RegisterSavesProjected
}

public enum NetworkDispatcherEntryRegisterSaveCaptureState
{
    Missing,
    Captured,
    Unreached,
    SuppliedUnused
}

public readonly record struct NetworkDispatcherEntryRegisterSavePush(
    uint RawFileOffset,
    uint? StackAddress,
    uint Value,
    bool StackAddressKnown,
    bool Attempted,
    bool Committed);

public sealed record NetworkDispatcherEntryRegisterSaveCapture(
    NetworkDispatcherEntryFramePrefixCapture ParentCapture,
    NetworkDispatcherEntryFramePrefixPlan ParentPlan,
    uint? FreshEntryEBX,
    uint? FreshEntryESI,
    uint? FreshEntryEDI);

public sealed record NetworkDispatcherEntryRegisterSavePlan(
    NetworkDispatcherEntryRegisterSaveOutcome Outcome,
    NetworkDispatcherEntryFramePrefixCapture ParentCapture,
    NetworkDispatcherEntryFramePrefixPlan ParentPlan,
    bool ParentPlanMatchesCapture,
    ImmutableArray<uint> CompletedInstructionOffsets,
    ImmutableArray<NetworkDispatcherEntryRegisterSavePush> Pushes,
    NetworkDispatcherEntryRegisterSaveCaptureState FreshEntryEBXCaptureState,
    NetworkDispatcherEntryRegisterSaveCaptureState FreshEntryESICaptureState,
    NetworkDispatcherEntryRegisterSaveCaptureState FreshEntryEDICaptureState,
    uint? FreshEntryEBX,
    uint? FreshEntryESI,
    uint? FreshEntryEDI,
    uint? EntryEBP,
    uint? CurrentESPAtStop,
    NetworkIoCompletionClockGateFlags? FlagsAtStop,
    uint StopAtRawFileOffsetExclusive)
{
    public bool ExactRegisterSavesProjected =>
        Outcome == NetworkDispatcherEntryRegisterSaveOutcome.RegisterSavesProjected
        && StopAtRawFileOffsetExclusive
            == NetworkDispatcherEntryRegisterSaveProjection.PrefixEndRawFileOffsetExclusive;

    public bool CookieGlobalReadReached => false;
}

/// <summary>
/// Value-only projection of the three dispatcher entry register saves after the
/// accepted entry-frame prefix. Captures are coordinator-provided and not verified
/// against the binary by this projection.
/// </summary>
public static class NetworkDispatcherEntryRegisterSaveProjection
{
    public const uint EntryRawFileOffset = 0x00BBDDC4;
    public const uint SaveEBXRawFileOffset = 0x00BBDDC4;
    public const uint SaveESIRawFileOffset = 0x00BBDDC5;
    public const uint SaveEDIRawFileOffset = 0x00BBDDC6;
    public const uint CookieGlobalReadRawFileOffset = 0x00BBDDC7;
    public const uint PrefixEndRawFileOffsetExclusive = 0x00BBDDC7;
    public const uint PushSize = sizeof(uint);

    public const string CapturedWindowBytes =
        "55 8B EC 6A FF 68 18 3C A4 01 64 A1 00 00 00 00 50 83 EC 0C 53 56 57 A1 00 50 DC 01";

    public static bool ExecutesNativeInstructions => false;

    public static uint StackAddressAfterPush(uint stackPointer) =>
        unchecked(stackPointer - PushSize);

    public static NetworkDispatcherEntryRegisterSavePlan Project(
        NetworkDispatcherEntryRegisterSaveCapture capture)
    {
        NetworkDispatcherEntryFramePrefixPlan reprojectedParent =
            NetworkDispatcherEntryFramePrefixProjection.Project(capture.ParentCapture);
        return Project(capture, reprojectedParent);
    }

    internal static NetworkDispatcherEntryRegisterSavePlan Project(
        NetworkDispatcherEntryRegisterSaveCapture capture,
        NetworkDispatcherEntryFramePrefixPlan reprojectedParent)
    {
        bool parentPlanMatches = ParentPlansEqual(capture.ParentPlan, reprojectedParent);

        if (!parentPlanMatches)
            return Stop(
                NetworkDispatcherEntryRegisterSaveOutcome.ParentPlanMismatch,
                capture,
                reprojectedParent,
                parentPlanMatches: false,
                stopAt: reprojectedParent.StopAtRawFileOffsetExclusive);

        if (!reprojectedParent.ExactPrefixProjected
            || reprojectedParent.EntryEBPAfterMov is not uint entryEBP
            || reprojectedParent.CurrentESPAtStop is not uint currentESP)
            return Stop(
                NetworkDispatcherEntryRegisterSaveOutcome.ParentPrefixNotAccepted,
                capture,
                reprojectedParent,
                parentPlanMatches,
                reprojectedParent.StopAtRawFileOffsetExclusive);

        var completed = ImmutableArray.CreateBuilder<uint>(3);
        var pushes = ImmutableArray.CreateBuilder<NetworkDispatcherEntryRegisterSavePush>(3);

        if (capture.FreshEntryEBX is not uint entryEBX)
            return Stop(
                NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryEBX,
                capture,
                reprojectedParent,
                parentPlanMatches,
                SaveEBXRawFileOffset,
                completed.ToImmutable(),
                pushes.ToImmutable(),
                entryEBP,
                currentESP);

        pushes.Add(Push(SaveEBXRawFileOffset, ref currentESP, entryEBX));
        completed.Add(SaveEBXRawFileOffset);

        if (capture.FreshEntryESI is not uint entryESI)
            return Stop(
                NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryESI,
                capture,
                reprojectedParent,
                parentPlanMatches,
                SaveESIRawFileOffset,
                completed.ToImmutable(),
                pushes.ToImmutable(),
                entryEBP,
                currentESP);

        pushes.Add(Push(SaveESIRawFileOffset, ref currentESP, entryESI));
        completed.Add(SaveESIRawFileOffset);

        if (capture.FreshEntryEDI is not uint entryEDI)
            return Stop(
                NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryEDI,
                capture,
                reprojectedParent,
                parentPlanMatches,
                SaveEDIRawFileOffset,
                completed.ToImmutable(),
                pushes.ToImmutable(),
                entryEBP,
                currentESP);

        pushes.Add(Push(SaveEDIRawFileOffset, ref currentESP, entryEDI));
        completed.Add(SaveEDIRawFileOffset);

        return new(
            NetworkDispatcherEntryRegisterSaveOutcome.RegisterSavesProjected,
            capture.ParentCapture,
            reprojectedParent,
            parentPlanMatches,
            completed.ToImmutable(),
            pushes.ToImmutable(),
            NetworkDispatcherEntryRegisterSaveCaptureState.Captured,
            NetworkDispatcherEntryRegisterSaveCaptureState.Captured,
            NetworkDispatcherEntryRegisterSaveCaptureState.Captured,
            capture.FreshEntryEBX,
            capture.FreshEntryESI,
            capture.FreshEntryEDI,
            entryEBP,
            currentESP,
            reprojectedParent.FlagsAtStop,
            PrefixEndRawFileOffsetExclusive);
    }

    private static NetworkDispatcherEntryRegisterSavePush Push(
        uint rawFileOffset,
        ref uint stackPointer,
        uint value)
    {
        stackPointer = StackAddressAfterPush(stackPointer);
        return new(
            rawFileOffset,
            stackPointer,
            value,
            StackAddressKnown: true,
            Attempted: true,
            Committed: true);
    }

    private static NetworkDispatcherEntryRegisterSavePlan Stop(
        NetworkDispatcherEntryRegisterSaveOutcome outcome,
        NetworkDispatcherEntryRegisterSaveCapture capture,
        NetworkDispatcherEntryFramePrefixPlan parentPlan,
        bool parentPlanMatches,
        uint stopAt,
        ImmutableArray<uint> completed = default,
        ImmutableArray<NetworkDispatcherEntryRegisterSavePush> pushes = default,
        uint? entryEBP = null,
        uint? currentESP = null) =>
        new(
            outcome,
            capture.ParentCapture,
            parentPlan,
            parentPlanMatches,
            completed.IsDefault ? ImmutableArray<uint>.Empty : completed,
            pushes.IsDefault
                ? ImmutableArray<NetworkDispatcherEntryRegisterSavePush>.Empty
                : pushes,
            CaptureStateFor(outcome, capture.FreshEntryEBX, Register.EBX),
            CaptureStateFor(outcome, capture.FreshEntryESI, Register.ESI),
            CaptureStateFor(outcome, capture.FreshEntryEDI, Register.EDI),
            capture.FreshEntryEBX,
            capture.FreshEntryESI,
            capture.FreshEntryEDI,
            entryEBP ?? parentPlan.EntryEBPAfterMov,
            currentESP ?? parentPlan.CurrentESPAtStop,
            parentPlan.FlagsAtStop,
            stopAt);

    private enum Register
    {
        EBX,
        ESI,
        EDI
    }

    private static NetworkDispatcherEntryRegisterSaveCaptureState CaptureStateFor(
        NetworkDispatcherEntryRegisterSaveOutcome outcome,
        uint? value,
        Register register)
    {
        bool supplied = value.HasValue;
        if (outcome is NetworkDispatcherEntryRegisterSaveOutcome.ParentPlanMismatch
            or NetworkDispatcherEntryRegisterSaveOutcome.ParentPrefixNotAccepted)
            return supplied
                ? NetworkDispatcherEntryRegisterSaveCaptureState.SuppliedUnused
                : NetworkDispatcherEntryRegisterSaveCaptureState.Unreached;

        bool observed = outcome switch
        {
            NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryEBX =>
                register == Register.EBX,
            NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryESI =>
                register is Register.EBX or Register.ESI,
            NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryEDI => true,
            NetworkDispatcherEntryRegisterSaveOutcome.RegisterSavesProjected => true,
            _ => false
        };
        if (!observed)
            return supplied
                ? NetworkDispatcherEntryRegisterSaveCaptureState.SuppliedUnused
                : NetworkDispatcherEntryRegisterSaveCaptureState.Unreached;

        bool missing = outcome switch
        {
            NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryEBX =>
                register == Register.EBX,
            NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryESI =>
                register == Register.ESI,
            NetworkDispatcherEntryRegisterSaveOutcome.MissingFreshEntryEDI =>
                register == Register.EDI,
            _ => false
        };
        return missing
            ? NetworkDispatcherEntryRegisterSaveCaptureState.Missing
            : NetworkDispatcherEntryRegisterSaveCaptureState.Captured;
    }

    private static bool ParentPlansEqual(
        NetworkDispatcherEntryFramePrefixPlan left,
        NetworkDispatcherEntryFramePrefixPlan right) =>
        left.Outcome == right.Outcome
        && left.CallerClockGateCapture == right.CallerClockGateCapture
        && ClockPlansEqual(left.CallerClockGatePlan, right.CallerClockGatePlan)
        && left.CallerDispatcherTargetVirtualAddress == right.CallerDispatcherTargetVirtualAddress
        && left.CallerDispatcherCallReached == right.CallerDispatcherCallReached
        && left.CallerDispatcherReturnPending == right.CallerDispatcherReturnPending
        && left.CompletedInstructionOffsets.SequenceEqual(right.CompletedInstructionOffsets)
        && left.Pushes.SequenceEqual(right.Pushes)
        && left.FSOffset0Read == right.FSOffset0Read
        && left.FreshEntryESPCaptureState == right.FreshEntryESPCaptureState
        && left.FreshEntryEBPCaptureState == right.FreshEntryEBPCaptureState
        && left.FreshEntryFlagsCaptureState == right.FreshEntryFlagsCaptureState
        && left.FreshFSOffset0DWORDCaptureState == right.FreshFSOffset0DWORDCaptureState
        && left.FreshEntryESP == right.FreshEntryESP
        && left.FreshEntryEBP == right.FreshEntryEBP
        && left.EntryEBPAfterMov == right.EntryEBPAfterMov
        && left.CurrentESPAtStop == right.CurrentESPAtStop
        && left.FreshEntryFlags == right.FreshEntryFlags
        && left.FreshFSOffset0DWORD == right.FreshFSOffset0DWORD
        && left.SubtractionReached == right.SubtractionReached
        && left.SubtractionLeft == right.SubtractionLeft
        && left.SubtractionRight == right.SubtractionRight
        && left.SubtractionResult == right.SubtractionResult
        && left.SubtractionFlags == right.SubtractionFlags
        && left.FlagsAtStop == right.FlagsAtStop
        && left.StopAtRawFileOffsetExclusive == right.StopAtRawFileOffsetExclusive;

    private static bool ClockPlansEqual(
        NetworkIoCompletionClockGatePlan left,
        NetworkIoCompletionClockGatePlan right) =>
        left.Outcome == right.Outcome
        && left.Reads.SequenceEqual(right.Reads)
        && left.Writes.SequenceEqual(right.Writes)
        && left.FencesRecorded.SequenceEqual(right.FencesRecorded)
        && left.Pushes.SequenceEqual(right.Pushes)
        && left.ThresholdLoaded == right.ThresholdLoaded
        && left.Threshold == right.Threshold
        && left.SavedPointerAddress == right.SavedPointerAddress
        && left.SavedPointerValue == right.SavedPointerValue
        && left.FreshClockFieldAddress == right.FreshClockFieldAddress
        && left.ClockCallReached == right.ClockCallReached
        && left.CachedClockTargetVirtualAddress == right.CachedClockTargetVirtualAddress
        && left.ClockReturnedNormally == right.ClockReturnedNormally
        && left.ClockReturnEAX == right.ClockReturnEAX
        && left.ClockCallInvalidatedArithmeticFlags == right.ClockCallInvalidatedArithmeticFlags
        && left.SubtractionReached == right.SubtractionReached
        && left.SubtractionLeft == right.SubtractionLeft
        && left.SubtractionRight == right.SubtractionRight
        && left.SubtractionResult == right.SubtractionResult
        && left.SubtractionFlags == right.SubtractionFlags
        && left.FreshSavedPointerReadReached == right.FreshSavedPointerReadReached
        && left.FreshSavedPointerAddress == right.FreshSavedPointerAddress
        && left.RestoredPointer == right.RestoredPointer
        && left.CompareReached == right.CompareReached
        && left.CompareLeft == right.CompareLeft
        && left.CompareRight == right.CompareRight
        && left.CompareResult == right.CompareResult
        && left.CompareFlags == right.CompareFlags
        && left.BelowBranchReached == right.BelowBranchReached
        && left.BelowBranchTaken == right.BelowBranchTaken
        && left.SelectedDestinationRawFileOffset == right.SelectedDestinationRawFileOffset
        && left.DispatcherArgumentReached == right.DispatcherArgumentReached
        && left.DispatcherArgumentECX == right.DispatcherArgumentECX
        && left.DispatcherCallReached == right.DispatcherCallReached
        && left.DispatcherTargetVirtualAddress == right.DispatcherTargetVirtualAddress
        && left.DispatcherReturnedNormally == right.DispatcherReturnedNormally
        && left.FlagsAtStop == right.FlagsAtStop
        && left.UnreachedEntryEBPCaptureProvided == right.UnreachedEntryEBPCaptureProvided
        && left.UnreachedEntryFlagsCaptureProvided == right.UnreachedEntryFlagsCaptureProvided
        && left.UnreachedFreshClockFieldCaptureProvided == right.UnreachedFreshClockFieldCaptureProvided
        && left.UnreachedCachedClockTargetCaptureProvided == right.UnreachedCachedClockTargetCaptureProvided
        && left.UnreachedClockReturnStatusProvided == right.UnreachedClockReturnStatusProvided
        && left.UnreachedClockReturnEAXCaptureProvided == right.UnreachedClockReturnEAXCaptureProvided
        && left.UnreachedFreshSavedPointerCaptureProvided == right.UnreachedFreshSavedPointerCaptureProvided
        && left.UnreachedDispatcherReturnStatusProvided == right.UnreachedDispatcherReturnStatusProvided
        && left.DispatcherReturnStatusProvidedButUnused == right.DispatcherReturnStatusProvidedButUnused
        && left.StopAtRawFileOffsetExclusive == right.StopAtRawFileOffsetExclusive;
}
