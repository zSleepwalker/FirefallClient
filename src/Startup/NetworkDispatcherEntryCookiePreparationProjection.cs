using System.Collections.Immutable;
using System.Numerics;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryCookiePreparationCapture(
    NetworkDispatcherEntryRegisterSaveCapture ParentRegisterSaveCapture,
    uint? FreshCookieGlobalDWORD);

public enum NetworkDispatcherEntryCookiePreparationOutcome
{
    ParentRegisterSavesNotAccepted,
    MissingFreshCookieGlobalDWORD,
    CookiePreparationProjected
}

public enum NetworkDispatcherEntryCookiePreparationCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public readonly record struct NetworkDispatcherEntryCookiePreparationRead(
    uint RawFileOffset,
    uint Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Completed);

public readonly record struct NetworkDispatcherEntryCookiePreparationXor(
    uint RawFileOffset,
    uint? LeftEAX,
    uint EntryFrameEBP,
    uint? ResultEAX,
    bool Attempted,
    bool Completed,
    NetworkIoCompletionClockGateFlags? FlagsBefore,
    NetworkIoCompletionClockGateFlags? FlagsAfter);

public readonly record struct NetworkDispatcherEntryCookiePreparationPush(
    uint RawFileOffset,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    uint? Value,
    bool Attempted,
    bool Committed);

public sealed record NetworkDispatcherEntryCookiePreparationPlan(
    NetworkDispatcherEntryCookiePreparationCapture Capture,
    NetworkDispatcherEntryRegisterSavePlan ParentRegisterSavePlan,
    NetworkDispatcherEntryCookiePreparationOutcome Outcome,
    ImmutableArray<uint> CompletedInstructionOffsets,
    NetworkDispatcherEntryCookiePreparationRead? FreshCookieRead,
    NetworkDispatcherEntryCookiePreparationXor? CookieXor,
    NetworkDispatcherEntryCookiePreparationPush? CookiePush,
    NetworkDispatcherEntryCookiePreparationCaptureState FreshCookieGlobalDWORDCaptureState,
    uint? EntryEBP,
    uint? EntryEAX,
    uint? CurrentESPAtStop,
    NetworkIoCompletionClockGateFlags? FlagsAtStop,
    uint StopAtRawFileOffsetExclusive)
{
    public bool ExactCookiePreparationProjected =>
        Outcome == NetworkDispatcherEntryCookiePreparationOutcome.CookiePreparationProjected
        && StopAtRawFileOffsetExclusive
            == NetworkDispatcherEntryCookiePreparationProjection.PrefixEndRawFileOffsetExclusive;

    public bool NativeCookieValidityChecked => false;

    public bool CookieABIInferred => false;

    public bool LinearFSInterpretationMade => false;
}

/// <summary>
/// Reprojects the dispatcher cookie load, XOR, and push without executing
/// native instructions or treating the cookie as a validity or ABI value.
/// </summary>
public static class NetworkDispatcherEntryCookiePreparationProjection
{
    public const uint EntryRawFileOffset = 0x00BBDDC7;
    public const uint FreshCookieGlobalReadRawFileOffset = 0x00BBDDC7;
    public const uint FreshCookieGlobalVirtualAddress = 0x01DC5000;
    public const uint CookieXorEntryEBPRawFileOffset = 0x00BBDDCC;
    public const uint PushCookieRawFileOffset = 0x00BBDDCE;
    public const uint PrefixEndRawFileOffsetExclusive = 0x00BBDDCF;
    public const uint PushWidth = sizeof(uint);

    public const string CapturedWindowBytes =
        "55 8B EC 6A FF 68 18 3C A4 01 64 A1 00 00 00 00 50 83 EC 0C 53 56 57 A1 00 50 DC 01 33 C5 50 8D 45 F4 64 A3 00 00 00 00 8B F1 8D 5E 6C C6 45 EC 01 53 89 5D E8 FF 15 84 42 AC 01";

    public static bool ExecutesNativeInstructions => false;

    public static uint StackAddressAfterPush(uint stackPointer) =>
        unchecked(stackPointer - PushWidth);

    public static NetworkDispatcherEntryCookiePreparationPlan Project(
        NetworkDispatcherEntryCookiePreparationCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentRegisterSaveCapture);

        NetworkDispatcherEntryRegisterSaveCapture suppliedParent =
            capture.ParentRegisterSaveCapture;
        NetworkDispatcherEntryFramePrefixPlan reprojectedFramePrefix =
            NetworkDispatcherEntryFramePrefixProjection.Project(suppliedParent.ParentCapture);
        var immutableParentCapture = new NetworkDispatcherEntryRegisterSaveCapture(
            suppliedParent.ParentCapture,
            reprojectedFramePrefix,
            suppliedParent.FreshEntryEBX,
            suppliedParent.FreshEntryESI,
            suppliedParent.FreshEntryEDI);
        NetworkDispatcherEntryRegisterSavePlan parent =
            NetworkDispatcherEntryRegisterSaveProjection.Project(
                immutableParentCapture,
                reprojectedFramePrefix);

        if (!parent.ExactRegisterSavesProjected
            || parent.EntryEBP is not uint entryEBP
            || parent.CurrentESPAtStop is not uint currentESP)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryCookiePreparationOutcome.ParentRegisterSavesNotAccepted,
                ImmutableArray<uint>.Empty,
                null,
                null,
                null,
                capture.FreshCookieGlobalDWORD.HasValue
                    ? NetworkDispatcherEntryCookiePreparationCaptureState.SuppliedUnused
                    : NetworkDispatcherEntryCookiePreparationCaptureState.Unreached,
                parent.EntryEBP,
                null,
                parent.CurrentESPAtStop,
                parent.FlagsAtStop,
                parent.StopAtRawFileOffsetExclusive);
        }

        bool cookieCaptured = capture.FreshCookieGlobalDWORD.HasValue;
        var read = new NetworkDispatcherEntryCookiePreparationRead(
            FreshCookieGlobalReadRawFileOffset,
            FreshCookieGlobalVirtualAddress,
            Width: 4,
            Attempted: true,
            Captured: cookieCaptured,
            Value: capture.FreshCookieGlobalDWORD,
            Completed: cookieCaptured);
        var completed = ImmutableArray.CreateBuilder<uint>(3);
        if (!cookieCaptured)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryCookiePreparationOutcome.MissingFreshCookieGlobalDWORD,
                completed.ToImmutable(),
                read,
                null,
                null,
                NetworkDispatcherEntryCookiePreparationCaptureState.Missing,
                entryEBP,
                null,
                currentESP,
                parent.FlagsAtStop,
                FreshCookieGlobalReadRawFileOffset);
        }

        uint cookieDWORD = capture.FreshCookieGlobalDWORD!.Value;
        completed.Add(FreshCookieGlobalReadRawFileOffset);
        uint result = cookieDWORD ^ entryEBP;
        var xorFlags = new NetworkIoCompletionClockGateFlags(
            Carry: false,
            Parity: (BitOperations.PopCount(result & 0xFF) & 1) == 0,
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x8000_0000) != 0,
            Overflow: false);
        var xor = new NetworkDispatcherEntryCookiePreparationXor(
            CookieXorEntryEBPRawFileOffset,
            cookieDWORD,
            entryEBP,
            result,
            Attempted: true,
            Completed: true,
            FlagsBefore: parent.FlagsAtStop,
            FlagsAfter: xorFlags);
        completed.Add(CookieXorEntryEBPRawFileOffset);

        uint stackPointerAfterPush = StackAddressAfterPush(currentESP);
        var push = new NetworkDispatcherEntryCookiePreparationPush(
            PushCookieRawFileOffset,
            currentESP,
            stackPointerAfterPush,
            result,
            Attempted: true,
            Committed: true);
        completed.Add(PushCookieRawFileOffset);

        return new(
            capture,
            parent,
            NetworkDispatcherEntryCookiePreparationOutcome.CookiePreparationProjected,
            completed.ToImmutable(),
            read,
            xor,
            push,
            NetworkDispatcherEntryCookiePreparationCaptureState.Captured,
            entryEBP,
            result,
            stackPointerAfterPush,
            xorFlags,
            PrefixEndRawFileOffsetExclusive);
    }
}
