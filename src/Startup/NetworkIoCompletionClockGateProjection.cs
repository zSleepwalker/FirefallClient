using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public enum NetworkIoCompletionClockGateOutcome
{
    EntryNotValidated,
    MissingFreshGlobalPointer,
    MissingEntryEBP,
    MissingFreshClockField,
    MissingCachedClockTarget,
    ClockReturnStatusUnknown,
    ClockDidNotReturnNormally,
    MissingClockReturnEAX,
    MissingFreshSavedPointer,
    BelowThreshold,
    DispatcherReturnStatusUnknown,
    DispatcherDidNotReturnNormally,
    DispatcherReturnedNormally
}

public enum NetworkIoCompletionClockGateReadKind
{
    FreshGlobalPointer,
    FreshClockField,
    FreshSavedPointer
}

public enum NetworkIoCompletionClockGateWriteKind
{
    SavedPointerStackStore,
    DispatcherPush
}

public readonly record struct NetworkIoCompletionClockGateFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct NetworkIoCompletionClockGateRead(
    uint RawFileOffset,
    NetworkIoCompletionClockGateReadKind Kind,
    uint Address,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public readonly record struct NetworkIoCompletionClockGateWrite(
    uint RawFileOffset,
    NetworkIoCompletionClockGateWriteKind Kind,
    uint? Address,
    uint Value,
    bool AddressKnown,
    bool Attempted,
    bool Committed);

public readonly record struct NetworkIoCompletionClockGateFence(uint RawFileOffset);

public readonly record struct NetworkIoCompletionClockGatePush(
    uint RawFileOffset,
    uint? StackAddress,
    uint Value,
    bool StackAddressKnown,
    bool Attempted,
    bool Committed);

/// <summary>
/// Entry captures for the gate. CachedClockTargetVirtualAddress comes from an
/// earlier EAX capture; this projection does not reread the clock IAT slot.
/// </summary>
public readonly record struct NetworkIoCompletionClockGateCapture(
    bool EntryValidated,
    uint? EntryEBP,
    NetworkIoCompletionClockGateFlags? EntryFlags,
    uint? FreshGlobalPointer,
    uint? FreshClockField,
    uint? CachedClockTargetVirtualAddress,
    bool? ClockReturnedNormally,
    uint? ClockReturnEAX,
    uint? FreshSavedPointerRead,
    bool? DispatcherReturnedNormally);

public sealed record NetworkIoCompletionClockGatePlan(
    NetworkIoCompletionClockGateOutcome Outcome,
    ImmutableArray<NetworkIoCompletionClockGateRead> Reads,
    ImmutableArray<NetworkIoCompletionClockGateWrite> Writes,
    ImmutableArray<NetworkIoCompletionClockGateFence> FencesRecorded,
    ImmutableArray<NetworkIoCompletionClockGatePush> Pushes,
    bool ThresholdLoaded,
    uint? Threshold,
    uint? SavedPointerAddress,
    uint? SavedPointerValue,
    uint? FreshClockFieldAddress,
    bool ClockCallReached,
    uint? CachedClockTargetVirtualAddress,
    bool? ClockReturnedNormally,
    uint? ClockReturnEAX,
    bool ClockCallInvalidatedArithmeticFlags,
    bool SubtractionReached,
    uint? SubtractionLeft,
    uint? SubtractionRight,
    uint? SubtractionResult,
    NetworkIoCompletionClockGateFlags? SubtractionFlags,
    bool FreshSavedPointerReadReached,
    uint? FreshSavedPointerAddress,
    uint? RestoredPointer,
    bool CompareReached,
    uint? CompareLeft,
    uint? CompareRight,
    uint? CompareResult,
    NetworkIoCompletionClockGateFlags? CompareFlags,
    bool BelowBranchReached,
    bool? BelowBranchTaken,
    uint? SelectedDestinationRawFileOffset,
    bool DispatcherArgumentReached,
    uint? DispatcherArgumentECX,
    bool DispatcherCallReached,
    uint? DispatcherTargetVirtualAddress,
    bool? DispatcherReturnedNormally,
    NetworkIoCompletionClockGateFlags? FlagsAtStop,
    bool UnreachedEntryEBPCaptureProvided,
    bool UnreachedEntryFlagsCaptureProvided,
    bool UnreachedFreshClockFieldCaptureProvided,
    bool UnreachedCachedClockTargetCaptureProvided,
    bool UnreachedClockReturnStatusProvided,
    bool UnreachedClockReturnEAXCaptureProvided,
    bool UnreachedFreshSavedPointerCaptureProvided,
    bool UnreachedDispatcherReturnStatusProvided,
    bool DispatcherReturnStatusProvidedButUnused,
    uint StopAtRawFileOffsetExclusive)
{
    public bool BoundedContinuationEntered => false;

    public bool BoundedBranchSelected =>
        BelowBranchReached && BelowBranchTaken == true;

    public bool BoundedCompletionBoundaryReached =>
        Outcome is NetworkIoCompletionClockGateOutcome.BelowThreshold
            or NetworkIoCompletionClockGateOutcome.DispatcherReturnedNormally;

    public bool ExactCleanExitAdmitted =>
        BoundedCompletionBoundaryReached
        && !UnreachedEntryEBPCaptureProvided
        && !UnreachedEntryFlagsCaptureProvided
        && !UnreachedFreshClockFieldCaptureProvided
        && !UnreachedCachedClockTargetCaptureProvided
        && !UnreachedClockReturnStatusProvided
        && !UnreachedClockReturnEAXCaptureProvided
        && !UnreachedFreshSavedPointerCaptureProvided
        && !UnreachedDispatcherReturnStatusProvided;
}

/// <summary>
/// Value-only projection of the first observed clock gate. It does not execute
/// native instructions, model timer behavior, or project a whole worker pass.
/// </summary>
public static class NetworkIoCompletionClockGateProjection
{
    public const uint EntryRawFileOffset = 0x00BBC8B0;
    public const uint ThresholdLoadRawFileOffset = 0x00BBC8B6;
    public const uint SavePointerRawFileOffset = 0x00BBC8BB;
    public const uint FirstFenceRawFileOffset = 0x00BBC8BE;
    public const uint FreshClockFieldRawFileOffset = 0x00BBC8C1;
    public const uint SecondFenceRawFileOffset = 0x00BBC8C4;
    public const uint ClockCallRawFileOffset = 0x00BBC8C7;
    public const uint SubtractRawFileOffset = 0x00BBC8C9;
    public const uint RestorePointerRawFileOffset = 0x00BBC8CB;
    public const uint CompareRawFileOffset = 0x00BBC8CE;
    public const uint BelowBranchRawFileOffset = 0x00BBC8D0;
    public const uint DispatcherPushRawFileOffset = 0x00BBC8D2;
    public const uint DispatcherArgumentRawFileOffset = 0x00BBC8D4;
    public const uint DispatcherCallRawFileOffset = 0x00BBC8D6;
    public const uint BoundedContinuationRawFileOffset = 0x00BBC8DB;

    public const uint FreshGlobalPointerVirtualAddress = 0x01DFFF70;
    public const uint FreshClockFieldOffset = 0x68;
    public const uint SavedPointerStackOffset = 0xFFFFFFF0;
    public const uint ThresholdMilliseconds = 500;
    public const uint ClockIatVirtualAddress = 0x01AC4D24;
    public const uint ClockIatRawFileOffset = 0x016C3924;
    public const uint DispatcherRawFileOffset = 0x00BBDDB0;
    public const uint DispatcherRva = 0x00BBE9B0;
    public const uint ImageBase = 0x00400000;
    public const uint DispatcherVirtualAddress = 0x00FBE9B0;
    public const uint DispatcherCallRelativeDisplacement = 0x14D5;

    public const string CapturedWindowBytes =
        "8B 35 70 FF DF 01 BB F4 01 00 00 89 75 F0 0F AE E8 "
        + "8B 76 68 0F AE E8 FF D0 2B C6 8B 75 F0 3B C3 72 09 "
        + "6A 01 8B CE E8 D5 14 00 00";

    public static bool ExecutesNativeInstructions => false;

    public static uint FreshClockFieldAddress(uint freshGlobalPointer) =>
        unchecked(freshGlobalPointer + FreshClockFieldOffset);

    public static uint SavedPointerAddress(uint entryEBP) =>
        unchecked(entryEBP + SavedPointerStackOffset);

    public static uint Subtract(uint clockReturnEAX, uint freshClockField) =>
        unchecked(clockReturnEAX - freshClockField);

    public static NetworkIoCompletionClockGatePlan Project(
        NetworkIoCompletionClockGateCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<NetworkIoCompletionClockGateRead>();
        var writes = ImmutableArray.CreateBuilder<NetworkIoCompletionClockGateWrite>();
        var fences = ImmutableArray.CreateBuilder<NetworkIoCompletionClockGateFence>();
        var pushes = ImmutableArray.CreateBuilder<NetworkIoCompletionClockGatePush>();

        if (!capture.EntryValidated)
            return Stop(NetworkIoCompletionClockGateOutcome.EntryNotValidated, EntryRawFileOffset);

        NetworkIoCompletionClockGateFlags? currentFlags = capture.EntryFlags;
        reads.Add(new(
            EntryRawFileOffset,
            NetworkIoCompletionClockGateReadKind.FreshGlobalPointer,
            FreshGlobalPointerVirtualAddress,
            Attempted: true,
            Captured: capture.FreshGlobalPointer.HasValue,
            Value: capture.FreshGlobalPointer,
            Committed: false));
        if (capture.FreshGlobalPointer is not uint originalPointer)
            return Stop(
                NetworkIoCompletionClockGateOutcome.MissingFreshGlobalPointer,
                EntryRawFileOffset,
                flagsAtStop: currentFlags);

        if (capture.EntryEBP is not uint entryEBP)
            return Stop(
                NetworkIoCompletionClockGateOutcome.MissingEntryEBP,
                SavePointerRawFileOffset,
                thresholdLoaded: true,
                flagsAtStop: currentFlags);

        uint savedPointerAddress = SavedPointerAddress(entryEBP);
        writes.Add(new(
            SavePointerRawFileOffset,
            NetworkIoCompletionClockGateWriteKind.SavedPointerStackStore,
            savedPointerAddress,
            originalPointer,
            AddressKnown: true,
            Attempted: true,
            Committed: true));
        fences.Add(new(FirstFenceRawFileOffset));

        uint freshClockFieldAddress = FreshClockFieldAddress(originalPointer);
        reads.Add(new(
            FreshClockFieldRawFileOffset,
            NetworkIoCompletionClockGateReadKind.FreshClockField,
            freshClockFieldAddress,
            Attempted: true,
            Captured: capture.FreshClockField.HasValue,
            Value: capture.FreshClockField,
            Committed: false));
        if (capture.FreshClockField is not uint freshClockField)
            return Stop(
                NetworkIoCompletionClockGateOutcome.MissingFreshClockField,
                FreshClockFieldRawFileOffset,
                thresholdLoaded: true,
                savedPointerAddress: savedPointerAddress,
                savedPointerValue: originalPointer,
                freshClockFieldAddress: freshClockFieldAddress,
                flagsAtStop: currentFlags);

        fences.Add(new(SecondFenceRawFileOffset));
        if (capture.CachedClockTargetVirtualAddress is not uint clockTarget)
            return Stop(
                NetworkIoCompletionClockGateOutcome.MissingCachedClockTarget,
                ClockCallRawFileOffset,
                thresholdLoaded: true,
                savedPointerAddress: savedPointerAddress,
                savedPointerValue: originalPointer,
                freshClockFieldAddress: freshClockFieldAddress,
                flagsAtStop: currentFlags);

        if (capture.ClockReturnedNormally is null)
            return Stop(
                NetworkIoCompletionClockGateOutcome.ClockReturnStatusUnknown,
                ClockCallRawFileOffset,
                thresholdLoaded: true,
                savedPointerAddress: savedPointerAddress,
                savedPointerValue: originalPointer,
                freshClockFieldAddress: freshClockFieldAddress,
                clockCallReached: true,
                clockTarget: clockTarget,
                flagsAtStop: currentFlags);

        if (capture.ClockReturnedNormally == false)
            return Stop(
                NetworkIoCompletionClockGateOutcome.ClockDidNotReturnNormally,
                ClockCallRawFileOffset,
                thresholdLoaded: true,
                savedPointerAddress: savedPointerAddress,
                savedPointerValue: originalPointer,
                freshClockFieldAddress: freshClockFieldAddress,
                clockCallReached: true,
                clockTarget: clockTarget,
                clockReturnedNormally: false,
                flagsAtStop: currentFlags);

        currentFlags = null;
        if (capture.ClockReturnEAX is not uint clockReturnEAX)
            return Stop(
                NetworkIoCompletionClockGateOutcome.MissingClockReturnEAX,
                SubtractRawFileOffset,
                thresholdLoaded: true,
                savedPointerAddress: savedPointerAddress,
                savedPointerValue: originalPointer,
                freshClockFieldAddress: freshClockFieldAddress,
                clockCallReached: true,
                clockTarget: clockTarget,
                clockReturnedNormally: true,
                clockCallInvalidatedFlags: true,
                flagsAtStop: currentFlags);

        uint elapsed = Subtract(clockReturnEAX, freshClockField);
        NetworkIoCompletionClockGateFlags subtractionFlags =
            ArithmeticFlags(clockReturnEAX, freshClockField, elapsed);
        currentFlags = subtractionFlags;

        reads.Add(new(
            RestorePointerRawFileOffset,
            NetworkIoCompletionClockGateReadKind.FreshSavedPointer,
            savedPointerAddress,
            Attempted: true,
            Captured: capture.FreshSavedPointerRead.HasValue,
            Value: capture.FreshSavedPointerRead,
            Committed: false));
        if (capture.FreshSavedPointerRead is not uint restoredPointer)
            return Stop(
                NetworkIoCompletionClockGateOutcome.MissingFreshSavedPointer,
                RestorePointerRawFileOffset,
                thresholdLoaded: true,
                savedPointerAddress: savedPointerAddress,
                savedPointerValue: originalPointer,
                freshClockFieldAddress: freshClockFieldAddress,
                clockCallReached: true,
                clockTarget: clockTarget,
                clockReturnedNormally: true,
                clockReturnEAX: clockReturnEAX,
                clockCallInvalidatedFlags: true,
                subtractionReached: true,
                subtractionLeft: clockReturnEAX,
                subtractionRight: freshClockField,
                subtractionResult: elapsed,
                subtractionFlags: subtractionFlags,
                freshSavedPointerReadReached: true,
                freshSavedPointerAddress: savedPointerAddress,
                flagsAtStop: currentFlags);

        uint compareResult = unchecked(elapsed - ThresholdMilliseconds);
        NetworkIoCompletionClockGateFlags compareFlags =
            ArithmeticFlags(elapsed, ThresholdMilliseconds, compareResult);
        currentFlags = compareFlags;
        bool belowThreshold = elapsed < ThresholdMilliseconds;

        if (belowThreshold)
        {
            return Stop(
                NetworkIoCompletionClockGateOutcome.BelowThreshold,
                BoundedContinuationRawFileOffset,
                thresholdLoaded: true,
                savedPointerAddress: savedPointerAddress,
                savedPointerValue: originalPointer,
                freshClockFieldAddress: freshClockFieldAddress,
                clockCallReached: true,
                clockTarget: clockTarget,
                clockReturnedNormally: true,
                clockReturnEAX: clockReturnEAX,
                clockCallInvalidatedFlags: true,
                subtractionReached: true,
                subtractionLeft: clockReturnEAX,
                subtractionRight: freshClockField,
                subtractionResult: elapsed,
                subtractionFlags: subtractionFlags,
                freshSavedPointerReadReached: true,
                freshSavedPointerAddress: savedPointerAddress,
                restoredPointer: restoredPointer,
                compareReached: true,
                compareLeft: elapsed,
                compareRight: ThresholdMilliseconds,
                compareResult: compareResult,
                compareFlags: compareFlags,
                belowBranchReached: true,
                belowBranchTaken: true,
                selectedDestination: BoundedContinuationRawFileOffset,
                flagsAtStop: currentFlags,
                dispatcherReturnStatusProvidedButUnused:
                    capture.DispatcherReturnedNormally.HasValue);
        }

        pushes.Add(new(
            DispatcherPushRawFileOffset,
            StackAddress: null,
            Value: 1,
            StackAddressKnown: false,
            Attempted: true,
            Committed: true));
        writes.Add(new(
            DispatcherPushRawFileOffset,
            NetworkIoCompletionClockGateWriteKind.DispatcherPush,
            Address: null,
            Value: 1,
            AddressKnown: false,
            Attempted: true,
            Committed: true));

        if (capture.DispatcherReturnedNormally is null)
            return Stop(
                NetworkIoCompletionClockGateOutcome.DispatcherReturnStatusUnknown,
                DispatcherCallRawFileOffset,
                thresholdLoaded: true,
                savedPointerAddress: savedPointerAddress,
                savedPointerValue: originalPointer,
                freshClockFieldAddress: freshClockFieldAddress,
                clockCallReached: true,
                clockTarget: clockTarget,
                clockReturnedNormally: true,
                clockReturnEAX: clockReturnEAX,
                clockCallInvalidatedFlags: true,
                subtractionReached: true,
                subtractionLeft: clockReturnEAX,
                subtractionRight: freshClockField,
                subtractionResult: elapsed,
                subtractionFlags: subtractionFlags,
                freshSavedPointerReadReached: true,
                freshSavedPointerAddress: savedPointerAddress,
                restoredPointer: restoredPointer,
                compareReached: true,
                compareLeft: elapsed,
                compareRight: ThresholdMilliseconds,
                compareResult: compareResult,
                compareFlags: compareFlags,
                belowBranchReached: true,
                belowBranchTaken: false,
                selectedDestination: DispatcherPushRawFileOffset,
                dispatcherArgumentReached: true,
                dispatcherArgumentECX: restoredPointer,
                dispatcherCallReached: true,
                dispatcherReturnedNormally: null,
                flagsAtStop: currentFlags);

        if (capture.DispatcherReturnedNormally == false)
            return Stop(
                NetworkIoCompletionClockGateOutcome.DispatcherDidNotReturnNormally,
                DispatcherCallRawFileOffset,
                thresholdLoaded: true,
                savedPointerAddress: savedPointerAddress,
                savedPointerValue: originalPointer,
                freshClockFieldAddress: freshClockFieldAddress,
                clockCallReached: true,
                clockTarget: clockTarget,
                clockReturnedNormally: true,
                clockReturnEAX: clockReturnEAX,
                clockCallInvalidatedFlags: true,
                subtractionReached: true,
                subtractionLeft: clockReturnEAX,
                subtractionRight: freshClockField,
                subtractionResult: elapsed,
                subtractionFlags: subtractionFlags,
                freshSavedPointerReadReached: true,
                freshSavedPointerAddress: savedPointerAddress,
                restoredPointer: restoredPointer,
                compareReached: true,
                compareLeft: elapsed,
                compareRight: ThresholdMilliseconds,
                compareResult: compareResult,
                compareFlags: compareFlags,
                belowBranchReached: true,
                belowBranchTaken: false,
                selectedDestination: DispatcherPushRawFileOffset,
                dispatcherArgumentReached: true,
                dispatcherArgumentECX: restoredPointer,
                dispatcherCallReached: true,
                dispatcherReturnedNormally: false,
                flagsAtStop: currentFlags);

        return Stop(
            NetworkIoCompletionClockGateOutcome.DispatcherReturnedNormally,
            BoundedContinuationRawFileOffset,
            thresholdLoaded: true,
            savedPointerAddress: savedPointerAddress,
            savedPointerValue: originalPointer,
            freshClockFieldAddress: freshClockFieldAddress,
            clockCallReached: true,
            clockTarget: clockTarget,
            clockReturnedNormally: true,
            clockReturnEAX: clockReturnEAX,
            clockCallInvalidatedFlags: true,
            subtractionReached: true,
            subtractionLeft: clockReturnEAX,
            subtractionRight: freshClockField,
            subtractionResult: elapsed,
            subtractionFlags: subtractionFlags,
            freshSavedPointerReadReached: true,
            freshSavedPointerAddress: savedPointerAddress,
            restoredPointer: restoredPointer,
            compareReached: true,
            compareLeft: elapsed,
            compareRight: ThresholdMilliseconds,
            compareResult: compareResult,
            compareFlags: compareFlags,
            belowBranchReached: true,
            belowBranchTaken: false,
            selectedDestination: DispatcherPushRawFileOffset,
            dispatcherArgumentReached: true,
            dispatcherArgumentECX: restoredPointer,
            dispatcherCallReached: true,
            dispatcherReturnedNormally: true,
            flagsAtStop: null);

        NetworkIoCompletionClockGatePlan Stop(
            NetworkIoCompletionClockGateOutcome outcome,
            uint stop,
            bool thresholdLoaded = false,
            uint? savedPointerAddress = null,
            uint? savedPointerValue = null,
            uint? freshClockFieldAddress = null,
            bool clockCallReached = false,
            uint? clockTarget = null,
            bool? clockReturnedNormally = null,
            uint? clockReturnEAX = null,
            bool clockCallInvalidatedFlags = false,
            bool subtractionReached = false,
            uint? subtractionLeft = null,
            uint? subtractionRight = null,
            uint? subtractionResult = null,
            NetworkIoCompletionClockGateFlags? subtractionFlags = null,
            bool freshSavedPointerReadReached = false,
            uint? freshSavedPointerAddress = null,
            uint? restoredPointer = null,
            bool compareReached = false,
            uint? compareLeft = null,
            uint? compareRight = null,
            uint? compareResult = null,
            NetworkIoCompletionClockGateFlags? compareFlags = null,
            bool belowBranchReached = false,
            bool? belowBranchTaken = null,
            uint? selectedDestination = null,
            bool dispatcherArgumentReached = false,
            uint? dispatcherArgumentECX = null,
            bool dispatcherCallReached = false,
            bool? dispatcherReturnedNormally = null,
            NetworkIoCompletionClockGateFlags? flagsAtStop = null,
            bool dispatcherReturnStatusProvidedButUnused = false)
        {
            bool fieldReadReached = reads.Any(
                read => read.Kind == NetworkIoCompletionClockGateReadKind.FreshClockField);
            bool savedPointerReadReached = reads.Any(
                read => read.Kind == NetworkIoCompletionClockGateReadKind.FreshSavedPointer);
            return new(
                outcome,
                reads.ToImmutable(),
                writes.ToImmutable(),
                fences.ToImmutable(),
                pushes.ToImmutable(),
                thresholdLoaded,
                thresholdLoaded ? ThresholdMilliseconds : null,
                savedPointerAddress,
                savedPointerValue,
                freshClockFieldAddress,
                clockCallReached,
                clockTarget,
                clockReturnedNormally,
                clockReturnEAX,
                clockCallInvalidatedFlags,
                subtractionReached,
                subtractionLeft,
                subtractionRight,
                subtractionResult,
                subtractionFlags,
                freshSavedPointerReadReached,
                freshSavedPointerAddress,
                restoredPointer,
                compareReached,
                compareLeft,
                compareRight,
                compareResult,
                compareFlags,
                belowBranchReached,
                belowBranchTaken,
                selectedDestination,
                dispatcherArgumentReached,
                dispatcherArgumentECX,
                dispatcherCallReached,
                dispatcherCallReached ? DispatcherVirtualAddress : null,
                dispatcherReturnedNormally,
                flagsAtStop,
                capture.EntryEBP.HasValue && !writes.Any(
                    write => write.Kind == NetworkIoCompletionClockGateWriteKind.SavedPointerStackStore),
                capture.EntryFlags.HasValue && !capture.EntryValidated,
                capture.FreshClockField.HasValue && !fieldReadReached,
                capture.CachedClockTargetVirtualAddress.HasValue && !clockCallReached,
                capture.ClockReturnedNormally.HasValue && !clockCallReached,
                capture.ClockReturnEAX.HasValue && clockReturnedNormally != true,
                capture.FreshSavedPointerRead.HasValue && !savedPointerReadReached,
                capture.DispatcherReturnedNormally.HasValue && !dispatcherCallReached,
                dispatcherReturnStatusProvidedButUnused,
                stop);
        }
    }

    private static NetworkIoCompletionClockGateFlags ArithmeticFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: (BitOperations.PopCount(result & 0xFF) & 1) == 0,
            Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);
}
