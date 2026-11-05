using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public enum NetworkDispatcherEntryFramePrefixOutcome
{
    CallerDispatcherCallNotReached,
    CallerDispatcherReturnAlreadyObserved,
    CallerDispatcherCallNotCleanPending,
    MissingFreshEntryESP,
    MissingFreshEntryEBP,
    MissingFreshFSOffset0DWORD,
    PrefixProjected
}

public enum NetworkDispatcherEntryFramePrefixCaptureState
{
    Missing,
    Captured,
    Unreached,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFramePrefixSegmentRegister
{
    FS
}

public readonly record struct NetworkDispatcherEntryFramePrefixPush(
    uint RawFileOffset,
    uint? StackAddress,
    uint Value,
    bool StackAddressKnown,
    bool Attempted,
    bool Committed);

public readonly record struct NetworkDispatcherEntryFramePrefixSegmentRead(
    uint RawFileOffset,
    NetworkDispatcherEntryFramePrefixSegmentRegister SegmentRegister,
    uint SegmentOffset,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public sealed record NetworkDispatcherEntryFramePrefixCapture(
    NetworkIoCompletionClockGateCapture CallerClockGateCapture,
    uint? FreshEntryESP,
    uint? FreshEntryEBP,
    NetworkIoCompletionClockGateFlags? FreshEntryFlags,
    uint? FreshFSOffset0DWORD);

public sealed record NetworkDispatcherEntryFramePrefixPlan(
    NetworkDispatcherEntryFramePrefixOutcome Outcome,
    NetworkIoCompletionClockGateCapture CallerClockGateCapture,
    NetworkIoCompletionClockGatePlan CallerClockGatePlan,
    uint? CallerDispatcherTargetVirtualAddress,
    bool CallerDispatcherCallReached,
    bool CallerDispatcherReturnPending,
    ImmutableArray<uint> CompletedInstructionOffsets,
    ImmutableArray<NetworkDispatcherEntryFramePrefixPush> Pushes,
    NetworkDispatcherEntryFramePrefixSegmentRead? FSOffset0Read,
    NetworkDispatcherEntryFramePrefixCaptureState FreshEntryESPCaptureState,
    NetworkDispatcherEntryFramePrefixCaptureState FreshEntryEBPCaptureState,
    NetworkDispatcherEntryFramePrefixCaptureState FreshEntryFlagsCaptureState,
    NetworkDispatcherEntryFramePrefixCaptureState FreshFSOffset0DWORDCaptureState,
    uint? FreshEntryESP,
    uint? FreshEntryEBP,
    uint? EntryEBPAfterMov,
    uint? CurrentESPAtStop,
    NetworkIoCompletionClockGateFlags? FreshEntryFlags,
    uint? FreshFSOffset0DWORD,
    bool SubtractionReached,
    uint? SubtractionLeft,
    uint? SubtractionRight,
    uint? SubtractionResult,
    NetworkIoCompletionClockGateFlags? SubtractionFlags,
    NetworkIoCompletionClockGateFlags? FlagsAtStop,
    uint StopAtRawFileOffsetExclusive)
{
    public bool ExactCallerDispatcherTarget =>
        CallerDispatcherCallReached
        && CallerDispatcherTargetVirtualAddress
            == NetworkIoCompletionClockGateProjection.DispatcherVirtualAddress;

    public bool ExactPrefixProjected =>
        Outcome == NetworkDispatcherEntryFramePrefixOutcome.PrefixProjected
        && StopAtRawFileOffsetExclusive
            == NetworkDispatcherEntryFramePrefixProjection.PrefixEndRawFileOffsetExclusive;
}

/// <summary>
/// Value-only projection of the dispatcher entry frame prefix through SUB ESP, 0Ch.
/// The supplied byte window is coordinator-provided evidence and is not independently
/// verified against the binary by this projection.
/// </summary>
public static class NetworkDispatcherEntryFramePrefixProjection
{
    public const uint EntryRawFileOffset = 0x00BBDDB0;
    public const uint SaveEntryEBPRawFileOffset = 0x00BBDDB0;
    public const uint EstablishFramePointerRawFileOffset = 0x00BBDDB1;
    public const uint PushExceptionSentinelRawFileOffset = 0x00BBDDB3;
    public const uint PushExceptionHandlerRawFileOffset = 0x00BBDDB5;
    public const uint ReadFSOffset0RawFileOffset = 0x00BBDDBA;
    public const uint PushFSOffset0RawFileOffset = 0x00BBDDC0;
    public const uint AllocateLocalsRawFileOffset = 0x00BBDDC1;
    public const uint PrefixEndRawFileOffsetExclusive = 0x00BBDDC4;

    public const uint ExceptionHandlerVirtualAddress = 0x01A43C18;
    public const uint ExceptionSentinel = uint.MaxValue;
    public const uint FSChainHeadSegmentOffset = 0;
    public const uint LocalAllocationSize = 0x0C;
    public const uint PushSize = sizeof(uint);

    public const string CapturedWindowBytes =
        "55 8B EC 6A FF 68 18 3C A4 01 64 A1 00 00 00 00 50 83 EC 0C";

    public static bool ExecutesNativeInstructions => false;

    public static uint StackAddressAfterPush(uint stackPointer) =>
        unchecked(stackPointer - PushSize);

    public static uint EntryEBPAfterMov(uint entryESP) =>
        StackAddressAfterPush(entryESP);

    public static uint SubtractLocalAllocation(uint stackPointer) =>
        unchecked(stackPointer - LocalAllocationSize);

    public static NetworkDispatcherEntryFramePrefixPlan Project(
        NetworkDispatcherEntryFramePrefixCapture capture)
    {
        var callerCapture = capture.CallerClockGateCapture;
        var callerPlan = NetworkIoCompletionClockGateProjection.Project(callerCapture);
        var callerReached = callerPlan.DispatcherCallReached;
        bool callerHasNoUnreachedCaptures =
            !callerPlan.UnreachedEntryEBPCaptureProvided
            && !callerPlan.UnreachedEntryFlagsCaptureProvided
            && !callerPlan.UnreachedFreshClockFieldCaptureProvided
            && !callerPlan.UnreachedCachedClockTargetCaptureProvided
            && !callerPlan.UnreachedClockReturnStatusProvided
            && !callerPlan.UnreachedClockReturnEAXCaptureProvided
            && !callerPlan.UnreachedFreshSavedPointerCaptureProvided
            && !callerPlan.UnreachedDispatcherReturnStatusProvided
            && !callerPlan.DispatcherReturnStatusProvidedButUnused;
        var callerPending = callerReached
            && callerCapture.DispatcherReturnedNormally is null
            && callerPlan.DispatcherReturnedNormally is null
            && callerPlan.Outcome
                == NetworkIoCompletionClockGateOutcome.DispatcherReturnStatusUnknown
            && callerHasNoUnreachedCaptures;

        if (!callerReached)
            return Stop(
                NetworkDispatcherEntryFramePrefixOutcome.CallerDispatcherCallNotReached,
                callerCapture,
                callerPlan,
                callerReached,
                callerPending,
                callerPlan.StopAtRawFileOffsetExclusive,
                capture);

        if (callerCapture.DispatcherReturnedNormally.HasValue
            || callerPlan.DispatcherReturnedNormally.HasValue)
            return Stop(
                NetworkDispatcherEntryFramePrefixOutcome.CallerDispatcherReturnAlreadyObserved,
                callerCapture,
                callerPlan,
                callerReached,
                callerPending,
                callerPlan.StopAtRawFileOffsetExclusive,
                capture);

        if (!callerPending
            || callerPlan.DispatcherTargetVirtualAddress
                != NetworkIoCompletionClockGateProjection.DispatcherVirtualAddress)
            return Stop(
                NetworkDispatcherEntryFramePrefixOutcome.CallerDispatcherCallNotCleanPending,
                callerCapture,
                callerPlan,
                callerReached,
                callerPending,
                callerPlan.StopAtRawFileOffsetExclusive,
                capture);

        if (capture.FreshEntryESP is not uint entryESP)
            return Stop(
                NetworkDispatcherEntryFramePrefixOutcome.MissingFreshEntryESP,
                callerCapture,
                callerPlan,
                callerReached,
                callerPending,
                EntryRawFileOffset,
                capture);

        if (capture.FreshEntryEBP is not uint entryEBP)
            return Stop(
                NetworkDispatcherEntryFramePrefixOutcome.MissingFreshEntryEBP,
                callerCapture,
                callerPlan,
                callerReached,
                callerPending,
                EntryRawFileOffset,
                capture,
                currentESP: capture.FreshEntryESP);

        var pushes = ImmutableArray.CreateBuilder<NetworkDispatcherEntryFramePrefixPush>(4);
        var completed = ImmutableArray.CreateBuilder<uint>(7);
        uint currentESP = entryESP;
        var entryFramePointer = EntryEBPAfterMov(entryESP);

        pushes.Add(Push(
            SaveEntryEBPRawFileOffset,
            ref currentESP,
            entryEBP));
        completed.Add(SaveEntryEBPRawFileOffset);
        completed.Add(EstablishFramePointerRawFileOffset);

        pushes.Add(Push(
            PushExceptionSentinelRawFileOffset,
            ref currentESP,
            ExceptionSentinel));
        completed.Add(PushExceptionSentinelRawFileOffset);

        pushes.Add(Push(
            PushExceptionHandlerRawFileOffset,
            ref currentESP,
            ExceptionHandlerVirtualAddress));
        completed.Add(PushExceptionHandlerRawFileOffset);

        var fsRead = new NetworkDispatcherEntryFramePrefixSegmentRead(
            ReadFSOffset0RawFileOffset,
            NetworkDispatcherEntryFramePrefixSegmentRegister.FS,
            FSChainHeadSegmentOffset,
            Attempted: true,
            Captured: capture.FreshFSOffset0DWORD.HasValue,
            Value: capture.FreshFSOffset0DWORD,
            Committed: false);

        if (capture.FreshFSOffset0DWORD is not uint fsChainHead)
            return Stop(
                NetworkDispatcherEntryFramePrefixOutcome.MissingFreshFSOffset0DWORD,
                callerCapture,
                callerPlan,
                callerReached,
                callerPending,
                ReadFSOffset0RawFileOffset,
                capture,
                completed.ToImmutable(),
                pushes.ToImmutable(),
                fsRead,
                entryFramePointer,
                currentESP);

        completed.Add(ReadFSOffset0RawFileOffset);
        pushes.Add(Push(
            PushFSOffset0RawFileOffset,
            ref currentESP,
            fsChainHead));
        completed.Add(PushFSOffset0RawFileOffset);

        uint subtractionLeft = currentESP;
        uint subtractionResult = SubtractLocalAllocation(subtractionLeft);
        currentESP = subtractionResult;
        var subtractionFlags = SubtractionFlags(
            subtractionLeft,
            LocalAllocationSize,
            subtractionResult);
        completed.Add(AllocateLocalsRawFileOffset);

        return new(
            NetworkDispatcherEntryFramePrefixOutcome.PrefixProjected,
            callerCapture,
            callerPlan,
            callerPlan.DispatcherTargetVirtualAddress,
            callerReached,
            callerPending,
            completed.ToImmutable(),
            pushes.ToImmutable(),
            fsRead,
            NetworkDispatcherEntryFramePrefixCaptureState.Captured,
            NetworkDispatcherEntryFramePrefixCaptureState.Captured,
            capture.FreshEntryFlags.HasValue
                ? NetworkDispatcherEntryFramePrefixCaptureState.Captured
                : NetworkDispatcherEntryFramePrefixCaptureState.Missing,
            NetworkDispatcherEntryFramePrefixCaptureState.Captured,
            entryESP,
            entryEBP,
            entryFramePointer,
            currentESP,
            capture.FreshEntryFlags,
            capture.FreshFSOffset0DWORD,
            SubtractionReached: true,
            SubtractionLeft: subtractionLeft,
            SubtractionRight: LocalAllocationSize,
            SubtractionResult: subtractionResult,
            SubtractionFlags: subtractionFlags,
            FlagsAtStop: subtractionFlags,
            StopAtRawFileOffsetExclusive: PrefixEndRawFileOffsetExclusive);
    }

    private static NetworkDispatcherEntryFramePrefixPush Push(
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

    private static NetworkIoCompletionClockGateFlags SubtractionFlags(
        uint left,
        uint right,
        uint result)
    {
        uint lowByte = result & 0xFF;
        bool evenParity = (BitOperations.PopCount(lowByte) & 1) == 0;
        return new(
            Carry: left < right,
            Parity: evenParity,
            Auxiliary: (left & 0xF) < (right & 0xF),
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static NetworkDispatcherEntryFramePrefixPlan Stop(
        NetworkDispatcherEntryFramePrefixOutcome outcome,
        NetworkIoCompletionClockGateCapture callerCapture,
        NetworkIoCompletionClockGatePlan callerPlan,
        bool callerReached,
        bool callerPending,
        uint stopAt,
        NetworkDispatcherEntryFramePrefixCapture? capture = null,
        ImmutableArray<uint> completed = default,
        ImmutableArray<NetworkDispatcherEntryFramePrefixPush> pushes = default,
        NetworkDispatcherEntryFramePrefixSegmentRead? fsRead = null,
        uint? entryEBPAfterMov = null,
        uint? currentESP = null) =>
        new(
            outcome,
            callerCapture,
            callerPlan,
            callerReached ? callerPlan.DispatcherTargetVirtualAddress : null,
            callerReached,
            callerPending,
            completed.IsDefault ? ImmutableArray<uint>.Empty : completed,
            pushes.IsDefault
                ? ImmutableArray<NetworkDispatcherEntryFramePrefixPush>.Empty
                : pushes,
            fsRead,
            CaptureStateFor(outcome, capture, FreshEntryRegister.ESP),
            CaptureStateFor(outcome, capture, FreshEntryRegister.EBP),
            CaptureStateFor(outcome, capture, FreshEntryRegister.Flags),
            CaptureStateFor(outcome, capture, FreshEntryRegister.FSOffset0),
            capture?.FreshEntryESP,
            capture?.FreshEntryEBP,
            entryEBPAfterMov,
            currentESP,
            capture?.FreshEntryFlags,
            capture?.FreshFSOffset0DWORD,
            SubtractionReached: false,
            SubtractionLeft: null,
            SubtractionRight: null,
            SubtractionResult: null,
            SubtractionFlags: null,
            FlagsAtStop: IsEntryReached(outcome)
                ? capture?.FreshEntryFlags
                : callerPlan.FlagsAtStop,
            StopAtRawFileOffsetExclusive: stopAt);

    private enum FreshEntryRegister
    {
        ESP,
        EBP,
        Flags,
        FSOffset0
    }

    private static NetworkDispatcherEntryFramePrefixCaptureState CaptureStateFor(
        NetworkDispatcherEntryFramePrefixOutcome outcome,
        NetworkDispatcherEntryFramePrefixCapture? capture,
        FreshEntryRegister register)
    {
        bool supplied = register switch
        {
            FreshEntryRegister.ESP => capture?.FreshEntryESP.HasValue == true,
            FreshEntryRegister.EBP => capture?.FreshEntryEBP.HasValue == true,
            FreshEntryRegister.Flags => capture?.FreshEntryFlags.HasValue == true,
            FreshEntryRegister.FSOffset0 => capture?.FreshFSOffset0DWORD.HasValue == true,
            _ => false
        };
        bool entryReached = IsEntryReached(outcome);

        if (!entryReached)
            return supplied
                ? NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
                : NetworkDispatcherEntryFramePrefixCaptureState.Unreached;

        bool observationReached = outcome switch
        {
            NetworkDispatcherEntryFramePrefixOutcome.MissingFreshEntryESP =>
                register == FreshEntryRegister.ESP,
            NetworkDispatcherEntryFramePrefixOutcome.MissingFreshEntryEBP =>
                register is FreshEntryRegister.ESP or FreshEntryRegister.EBP,
            NetworkDispatcherEntryFramePrefixOutcome.MissingFreshFSOffset0DWORD =>
                true,
            NetworkDispatcherEntryFramePrefixOutcome.PrefixProjected => true,
            _ => false
        };

        if (!observationReached)
            return supplied
                ? NetworkDispatcherEntryFramePrefixCaptureState.SuppliedUnused
                : NetworkDispatcherEntryFramePrefixCaptureState.Unreached;

        if (outcome == NetworkDispatcherEntryFramePrefixOutcome.MissingFreshEntryESP
            && register == FreshEntryRegister.ESP
            || outcome == NetworkDispatcherEntryFramePrefixOutcome.MissingFreshEntryEBP
                && register == FreshEntryRegister.EBP
            || outcome == NetworkDispatcherEntryFramePrefixOutcome.MissingFreshFSOffset0DWORD
                && register == FreshEntryRegister.FSOffset0)
            return NetworkDispatcherEntryFramePrefixCaptureState.Missing;

        return supplied
            ? NetworkDispatcherEntryFramePrefixCaptureState.Captured
            : NetworkDispatcherEntryFramePrefixCaptureState.Missing;
    }

    private static bool IsEntryReached(NetworkDispatcherEntryFramePrefixOutcome outcome) =>
        outcome is NetworkDispatcherEntryFramePrefixOutcome.MissingFreshEntryESP
            or NetworkDispatcherEntryFramePrefixOutcome.MissingFreshEntryEBP
            or NetworkDispatcherEntryFramePrefixOutcome.MissingFreshFSOffset0DWORD
            or NetworkDispatcherEntryFramePrefixOutcome.PrefixProjected;
}
