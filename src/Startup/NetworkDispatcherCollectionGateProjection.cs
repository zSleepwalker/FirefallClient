using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum NetworkDispatcherCollectionGateOutcome
{
    ParentGateIncomplete,
    ParentGateDidNotSelectCollectionEntry,
    ParentGateExitNotClean,
    MissingThresholdDWORD,
    MissingEntryESPBeforePush,
    MissingEntryECXBeforePush,
    MissingOpaqueCallReturnStatus,
    OpaqueCallDidNotReturnNormally,
    MissingPostLoggingESI,
    MissingParentEntryESI,
    MissingFreshEndDWORD,
    MissingFreshStartDWORD,
    MissingFreshReloadDWORD,
    MissingFreshCollectionEntry,
    EmptyShiftedDifference,
    ZeroCollectionEntry,
    NonzeroCollectionEntry
}

public enum NetworkDispatcherCollectionGateReadKind
{
    ThresholdDWORD,
    FreshEndDWORD,
    FreshStartDWORD,
    FreshReloadDWORD,
    FreshCollectionEntryDWORD
}

public enum NetworkDispatcherCollectionGateCaptureState
{
    Missing,
    Captured,
    Unreached,
    SuppliedUnused
}

public enum NetworkDispatcherCollectionGatePushKind
{
    InheritedECX,
    Zero,
    AddressImmediate,
    One
}

public enum NetworkDispatcherCollectionGateOrderedEffectKind
{
    ParentLockPush,
    ParentOpaqueImportCall,
    ParentFreshCounterRead,
    ParentCounterIncrement,
    ParentFreshArgumentByteRead,
    ParentEntryFrameClear,
    ParentReceiverByteClear,
    ThresholdDWORDRead,
    InheritedECXPush,
    ZeroPush,
    AddressImmediatePush,
    OnePush,
    OpaqueCall,
    FreshEndDWORDRead,
    FreshStartDWORDRead,
    FreshReloadDWORDRead,
    FreshCollectionEntryDWORDRead
}

public readonly record struct NetworkDispatcherCollectionGateFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct NetworkDispatcherCollectionGateCaptureStates(
    NetworkDispatcherCollectionGateCaptureState ThresholdDWORD,
    NetworkDispatcherCollectionGateCaptureState EntryESP,
    NetworkDispatcherCollectionGateCaptureState EntryECX,
    NetworkDispatcherCollectionGateCaptureState OpaqueCallReturnStatus,
    NetworkDispatcherCollectionGateCaptureState FreshPostLoggingESI,
    NetworkDispatcherCollectionGateCaptureState FreshEndDWORD,
    NetworkDispatcherCollectionGateCaptureState FreshStartDWORD,
    NetworkDispatcherCollectionGateCaptureState FreshReloadDWORD,
    NetworkDispatcherCollectionGateCaptureState FreshCollectionEntryDWORD);

public readonly record struct NetworkDispatcherCollectionGateCapture(
    NetworkDispatcherLockGateCapture ParentGate,
    uint? EntryESP = null,
    uint? EntryECX = null,
    uint? FreshThresholdDWORD = null,
    bool? OpaqueCallReturnedNormally = null,
    uint? FreshPostLoggingESI = null,
    uint? FreshEndDWORD = null,
    uint? FreshStartDWORD = null,
    uint? FreshReloadDWORD = null,
    uint? FreshCollectionEntryDWORD = null);

public readonly record struct NetworkDispatcherCollectionGateRead(
    uint RawFileOffset,
    NetworkDispatcherCollectionGateReadKind Kind,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public readonly record struct NetworkDispatcherCollectionGatePush(
    uint RawFileOffset,
    NetworkDispatcherCollectionGatePushKind Kind,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    uint? Value,
    bool Attempted,
    bool Committed);

public readonly record struct NetworkDispatcherCollectionGateOpaqueCall(
    uint RawFileOffset,
    uint TargetRawFileOffset,
    uint TargetVirtualAddress,
    bool Attempted,
    bool? ReturnedNormally);

public readonly record struct NetworkDispatcherCollectionGateOrderedEffect(
    uint RawFileOffset,
    NetworkDispatcherCollectionGateOrderedEffectKind Kind,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Committed);

public sealed record NetworkDispatcherCollectionGatePlan(
    NetworkDispatcherLockGatePlan ParentGatePlan,
    NetworkDispatcherCollectionGateCapture Capture,
    NetworkDispatcherCollectionGateOutcome Outcome,
    ImmutableArray<NetworkDispatcherCollectionGateRead> Reads,
    ImmutableArray<NetworkDispatcherCollectionGatePush> Pushes,
    ImmutableArray<NetworkDispatcherCollectionGateOpaqueCall> OpaqueCalls,
    ImmutableArray<NetworkDispatcherCollectionGateOrderedEffect> OrderedEffects,
    bool ChildGateAdmitted,
    bool ThresholdCompareReached,
    uint? ThresholdValue,
    NetworkDispatcherCollectionGateFlags? ThresholdFlags,
    bool ThresholdBranchReached,
    bool? ThresholdBranchTaken,
    uint? ThresholdSelectedDestinationRawFileOffset,
    bool? OpaqueCallReturnInvalidatedFlags,
    uint? CollectionESI,
    uint? FreshEndAddress,
    uint? FreshStartAddress,
    uint? DifferenceValue,
    NetworkDispatcherCollectionGateFlags? SubtractFlags,
    NetworkDispatcherCollectionGateFlags? FlagsAfterZeroEBX,
    uint? ShiftedDifference,
    NetworkDispatcherCollectionGateFlags? ShiftFlags,
    bool CollectionTestReached,
    NetworkDispatcherCollectionGateFlags? CollectionTestFlags,
    bool? EmptyBranchTaken,
    uint? ReloadAddress,
    uint? CollectionEntryAddress,
    uint? CollectionEntryValue,
    bool CollectionEntryTestReached,
    NetworkDispatcherCollectionGateFlags? CollectionEntryTestFlags,
    bool? NonzeroEntryBranchTaken,
    uint? SelectedDestinationRawFileOffset,
    NetworkDispatcherCollectionGateFlags? FlagsAtStop,
    bool BoundedCompletionBoundaryReached,
    NetworkDispatcherCollectionGateCaptureStates CaptureStates,
    bool UnreachedEntryESPCaptureProvided,
    bool UnreachedEntryECXCaptureProvided,
    bool UnreachedOpaqueCallStatusProvided,
    bool UnreachedPostLoggingESICaptureProvided,
    bool UnreachedThresholdCaptureProvided,
    bool UnreachedFreshEndCaptureProvided,
    bool UnreachedFreshStartCaptureProvided,
    bool UnreachedFreshReloadCaptureProvided,
    bool UnreachedCollectionEntryCaptureProvided,
    uint StopAtRawFileOffsetExclusive)
{
    public bool BoundedContinuationEntered => false;

    public bool ParentBoundedCompletionBoundaryReached =>
        ParentGatePlan.BoundedCompletionBoundaryReached;

    public bool ExactBoundedCompletion =>
        BoundedCompletionBoundaryReached
        || (!ChildGateAdmitted && ParentBoundedCompletionBoundaryReached);

    public bool ExactCleanExitAdmitted =>
        ChildGateAdmitted
        && BoundedCompletionBoundaryReached
        && ParentGatePlan.ExactCleanExitAdmitted
        && !UnreachedEntryESPCaptureProvided
        && !UnreachedEntryECXCaptureProvided
        && !UnreachedOpaqueCallStatusProvided
        && !UnreachedPostLoggingESICaptureProvided
        && !UnreachedThresholdCaptureProvided
        && !UnreachedFreshEndCaptureProvided
        && !UnreachedFreshStartCaptureProvided
        && !UnreachedFreshReloadCaptureProvided
        && !UnreachedCollectionEntryCaptureProvided;
}

/// <summary>
/// Value-only projection of the collection gate reached from the parent's
/// nonzero-argument boundary. The opaque call and all selected continuations
/// remain unexecuted.
/// </summary>
public static class NetworkDispatcherCollectionGateProjection
{
    public const uint EntryVirtualAddress = 0x00FBEA0E;
    public const uint EntryRawFileOffset = 0x00BBDE0E;
    public const uint RawToVirtualAddressDelta = 0x00400C00;

    public const uint ThresholdCompareRawFileOffset = 0x00BBDE0E;
    public const uint ThresholdBranchRawFileOffset = 0x00BBDE15;
    public const uint FirstPushRawFileOffset = 0x00BBDE17;
    public const uint ZeroPushRawFileOffset = 0x00BBDE18;
    public const uint AddressImmediatePushRawFileOffset = 0x00BBDE1A;
    public const uint OnePushRawFileOffset = 0x00BBDE1F;
    public const uint SetECXImmediateRawFileOffset = 0x00BBDE21;
    public const uint OpaqueCallRawFileOffset = 0x00BBDE26;
    public const uint CollectionStartRawFileOffset = 0x00BBDE2B;
    public const uint ZeroEBXRawFileOffset = 0x00BBDE31;
    public const uint SubtractStartRawFileOffset = 0x00BBDE33;
    public const uint ShiftDifferenceRawFileOffset = 0x00BBDE39;
    public const uint CollectionTestRawFileOffset = 0x00BBDE3C;
    public const uint EmptyBranchRawFileOffset = 0x00BBDE3E;
    public const uint ReloadStartRawFileOffset = 0x00BBDE40;
    public const uint ReadCollectionEntryRawFileOffset = 0x00BBDE46;
    public const uint CollectionEntryTestRawFileOffset = 0x00BBDE49;
    public const uint NonzeroEntryBranchRawFileOffset = 0x00BBDE4B;
    public const uint ZeroEntryContinuationRawFileOffset = 0x00BBDE4D;
    public const uint NonzeroEntryContinuationRawFileOffset = 0x00BBDE66;
    public const uint EmptyContinuationRawFileOffset = 0x00BBDE83;

    public const uint ThresholdDWORDVirtualAddress = 0x01E68590;
    public const uint ECXImmediateValue = 0x01E68580;
    public const uint PushedAddressImmediate = 0x01BB4F3C;
    public const uint OpaqueCallTargetRawFileOffset = 0x012C2030;
    public const uint OpaqueCallTargetVirtualAddress = 0x016C2C30;
    public const uint EndOffsetFromESI = 0xA8;
    public const uint StartOffsetFromESI = 0xA4;
    public const uint ShiftCount = 2;
    public const uint BytesPerEntry = 4;

    public const string CapturedWindowBytes =
        "833D9085E601017F14516A00683C4FBB016A01B98085E601E8054270008B86A800000033DB2B86A4000000C1F80285C074438B86A40000008B3C9885FF751957683B020000687F71B501687E71B501FF15A05EC10183C4108B078BCF";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherCollectionGatePlan Project(
        NetworkDispatcherCollectionGateCapture capture)
    {
        NetworkDispatcherLockGatePlan parent =
            NetworkDispatcherLockGateProjection.Project(capture.ParentGate);
        var reads = ImmutableArray.CreateBuilder<NetworkDispatcherCollectionGateRead>();
        var pushes = ImmutableArray.CreateBuilder<NetworkDispatcherCollectionGatePush>();
        var calls = ImmutableArray.CreateBuilder<NetworkDispatcherCollectionGateOpaqueCall>();
        NetworkDispatcherCollectionGateFlags? noFlags = null;

        if (!parent.BoundedCompletionBoundaryReached)
        {
            return Stop(NetworkDispatcherCollectionGateOutcome.ParentGateIncomplete,
                parent.StopAtRawFileOffsetExclusive, parent, reads, pushes, calls,
                capture, thresholdReached: false, thresholdValue: null,
                thresholdFlags: noFlags, thresholdBranchTaken: null,
                thresholdDestination: null, opaqueReturnInvalidatedFlags: null,
                collectionESI: null, freshEndAddress: null, freshStartAddress: null,
                difference: null, subtractFlags: noFlags, shiftedDifference: null,
                shiftFlags: noFlags, collectionTestReached: false,
                collectionTestFlags: noFlags, emptyBranchTaken: null,
                reloadAddress: null, collectionEntryAddress: null,
                collectionEntryValue: null, collectionEntryTestReached: false,
                collectionEntryTestFlags: noFlags, nonzeroEntryBranchTaken: null,
                destination: parent.SelectedDestinationRawFileOffset,
                flags: ConvertParentFlags(parent.FlagsAtStop), boundaryReached: false,
                thresholdReachedForUnused: false, entryESPReached: false,
                entryECXReached: false, callStatusReached: false,
                postLoggingESIReached: false, endReached: false,
                startReached: false, reloadReached: false, entryReached: false);
        }

        if (parent.Outcome
            != NetworkDispatcherLockGateOutcome.NonzeroArgumentFallthroughSelected)
        {
            return Stop(NetworkDispatcherCollectionGateOutcome.ParentGateDidNotSelectCollectionEntry,
                parent.StopAtRawFileOffsetExclusive, parent, reads, pushes, calls,
                capture, thresholdReached: false, thresholdValue: null,
                thresholdFlags: noFlags, thresholdBranchTaken: null,
                thresholdDestination: null, opaqueReturnInvalidatedFlags: null,
                collectionESI: null, freshEndAddress: null, freshStartAddress: null,
                difference: null, subtractFlags: noFlags, shiftedDifference: null,
                shiftFlags: noFlags, collectionTestReached: false,
                collectionTestFlags: noFlags, emptyBranchTaken: null,
                reloadAddress: null, collectionEntryAddress: null,
                collectionEntryValue: null, collectionEntryTestReached: false,
                collectionEntryTestFlags: noFlags, nonzeroEntryBranchTaken: null,
                destination: parent.SelectedDestinationRawFileOffset,
                flags: ConvertParentFlags(parent.FlagsAtStop), boundaryReached: false,
                thresholdReachedForUnused: false, entryESPReached: false,
                entryECXReached: false, callStatusReached: false,
                postLoggingESIReached: false, endReached: false,
                startReached: false, reloadReached: false, entryReached: false);
        }

        if (!parent.ExactCleanExitAdmitted)
        {
            return Stop(NetworkDispatcherCollectionGateOutcome.ParentGateExitNotClean,
                parent.StopAtRawFileOffsetExclusive, parent, reads, pushes, calls,
                capture, thresholdReached: false, thresholdValue: null,
                thresholdFlags: noFlags, thresholdBranchTaken: null,
                thresholdDestination: parent.SelectedDestinationRawFileOffset,
                opaqueReturnInvalidatedFlags: null,
                collectionESI: null, freshEndAddress: null, freshStartAddress: null,
                difference: null, subtractFlags: noFlags, shiftedDifference: null,
                shiftFlags: noFlags, collectionTestReached: false,
                collectionTestFlags: noFlags, emptyBranchTaken: null,
                reloadAddress: null, collectionEntryAddress: null,
                collectionEntryValue: null, collectionEntryTestReached: false,
                collectionEntryTestFlags: noFlags, nonzeroEntryBranchTaken: null,
                destination: parent.SelectedDestinationRawFileOffset,
                flags: ConvertParentFlags(parent.FlagsAtStop), boundaryReached: false,
                thresholdReachedForUnused: false, entryESPReached: false,
                entryECXReached: false, callStatusReached: false,
                postLoggingESIReached: false, endReached: false,
                startReached: false, reloadReached: false, entryReached: false);
        }

        NetworkDispatcherCollectionGateFlags thresholdFlags =
            ThresholdCompareFlags(capture.FreshThresholdDWORD ?? 0);
        if (capture.FreshThresholdDWORD is not uint threshold)
        {
            reads.Add(new NetworkDispatcherCollectionGateRead(
                ThresholdCompareRawFileOffset,
                NetworkDispatcherCollectionGateReadKind.ThresholdDWORD,
                ThresholdDWORDVirtualAddress,
                Width: 4,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false));
            return Stop(NetworkDispatcherCollectionGateOutcome.MissingThresholdDWORD,
                ThresholdCompareRawFileOffset, parent, reads, pushes, calls, capture,
                thresholdReached: false, thresholdValue: null, thresholdFlags: noFlags,
                thresholdBranchTaken: null, thresholdDestination: null,
                opaqueReturnInvalidatedFlags: null, collectionESI: null,
                freshEndAddress: null, freshStartAddress: null, difference: null,
                subtractFlags: noFlags, shiftedDifference: null, shiftFlags: noFlags,
                collectionTestReached: false, collectionTestFlags: noFlags,
                emptyBranchTaken: null, reloadAddress: null,
                collectionEntryAddress: null, collectionEntryValue: null,
                collectionEntryTestReached: false, collectionEntryTestFlags: noFlags,
                nonzeroEntryBranchTaken: null, destination: null,
                flags: ConvertParentFlags(parent.FlagsAtStop),
                boundaryReached: false, thresholdReachedForUnused: true,
                entryESPReached: false, entryECXReached: false,
                callStatusReached: false, postLoggingESIReached: false,
                endReached: false, startReached: false, reloadReached: false,
                entryReached: false);
        }

        reads.Add(new NetworkDispatcherCollectionGateRead(
            ThresholdCompareRawFileOffset,
            NetworkDispatcherCollectionGateReadKind.ThresholdDWORD,
            ThresholdDWORDVirtualAddress,
            Width: 4,
            Attempted: true,
            Captured: true,
            Value: threshold,
            Committed: false));
        thresholdFlags = ThresholdCompareFlags(threshold);
        bool signedGreaterThanOne =
            thresholdFlags.Sign == thresholdFlags.Overflow
            && thresholdFlags.Zero == false;
        uint thresholdDestination = signedGreaterThanOne
            ? CollectionStartRawFileOffset
            : FirstPushRawFileOffset;

        uint? currentESI = null;
        bool? callReturnInvalidatedFlags = null;
        NetworkDispatcherCollectionGateFlags? flags = thresholdFlags;
        if (!signedGreaterThanOne)
        {
            if (capture.EntryESP is not uint esp)
            {
                AddUnresolvedPush(pushes, FirstPushRawFileOffset,
                    NetworkDispatcherCollectionGatePushKind.InheritedECX,
                    null, null, null, attempted: true, committed: false);
                return Stop(NetworkDispatcherCollectionGateOutcome.MissingEntryESPBeforePush,
                    FirstPushRawFileOffset, parent, reads, pushes, calls, capture,
                    thresholdReached: true, thresholdValue: threshold,
                    thresholdFlags, thresholdBranchTaken: false,
                    thresholdDestination, callReturnInvalidatedFlags,
                    collectionESI: null, freshEndAddress: null, freshStartAddress: null,
                    difference: null, subtractFlags: noFlags,
                    shiftedDifference: null, shiftFlags: noFlags,
                    collectionTestReached: false, collectionTestFlags: noFlags,
                    emptyBranchTaken: null, reloadAddress: null,
                    collectionEntryAddress: null, collectionEntryValue: null,
                    collectionEntryTestReached: false, collectionEntryTestFlags: noFlags,
                    nonzeroEntryBranchTaken: null, destination: null, flags,
                    boundaryReached: false, thresholdReachedForUnused: true,
                    entryESPReached: true, entryECXReached: false,
                    callStatusReached: false, postLoggingESIReached: false,
                    endReached: false, startReached: false, reloadReached: false,
                    entryReached: false);
            }

            if (capture.EntryECX is not uint ecx)
            {
                AddUnresolvedPush(pushes, FirstPushRawFileOffset,
                    NetworkDispatcherCollectionGatePushKind.InheritedECX,
                    esp, unchecked(esp - 4), null, attempted: true, committed: false);
                return Stop(NetworkDispatcherCollectionGateOutcome.MissingEntryECXBeforePush,
                    FirstPushRawFileOffset, parent, reads, pushes, calls, capture,
                    thresholdReached: true, thresholdValue: threshold,
                    thresholdFlags, thresholdBranchTaken: false,
                    thresholdDestination, callReturnInvalidatedFlags,
                    collectionESI: null, freshEndAddress: null, freshStartAddress: null,
                    difference: null, subtractFlags: noFlags,
                    shiftedDifference: null, shiftFlags: noFlags,
                    collectionTestReached: false, collectionTestFlags: noFlags,
                    emptyBranchTaken: null, reloadAddress: null,
                    collectionEntryAddress: null, collectionEntryValue: null,
                    collectionEntryTestReached: false, collectionEntryTestFlags: noFlags,
                    nonzeroEntryBranchTaken: null, destination: null, flags,
                    boundaryReached: false, thresholdReachedForUnused: true,
                    entryESPReached: true, entryECXReached: true,
                    callStatusReached: false, postLoggingESIReached: false,
                    endReached: false, startReached: false, reloadReached: false,
                    entryReached: false);
            }

            uint afterECXPush = AddPush(pushes, FirstPushRawFileOffset,
                NetworkDispatcherCollectionGatePushKind.InheritedECX,
                esp, ecx, committed: true);
            uint afterZeroPush = AddPush(pushes, ZeroPushRawFileOffset,
                NetworkDispatcherCollectionGatePushKind.Zero,
                afterECXPush, 0, committed: true);
            uint afterAddressPush = AddPush(pushes, AddressImmediatePushRawFileOffset,
                NetworkDispatcherCollectionGatePushKind.AddressImmediate,
                afterZeroPush, PushedAddressImmediate, committed: true);
            AddPush(pushes, OnePushRawFileOffset,
                NetworkDispatcherCollectionGatePushKind.One,
                afterAddressPush, 1, committed: true);
            if (capture.OpaqueCallReturnedNormally is not bool returnedNormally)
            {
                calls.Add(new NetworkDispatcherCollectionGateOpaqueCall(
                    OpaqueCallRawFileOffset,
                    OpaqueCallTargetRawFileOffset,
                    OpaqueCallTargetVirtualAddress,
                    Attempted: true,
                    ReturnedNormally: null));
                return Stop(NetworkDispatcherCollectionGateOutcome.MissingOpaqueCallReturnStatus,
                    OpaqueCallRawFileOffset, parent, reads, pushes, calls, capture,
                    thresholdReached: true, thresholdValue: threshold,
                    thresholdFlags, thresholdBranchTaken: false,
                    thresholdDestination, callReturnInvalidatedFlags,
                    collectionESI: null, freshEndAddress: null, freshStartAddress: null,
                    difference: null, subtractFlags: noFlags,
                    shiftedDifference: null, shiftFlags: noFlags,
                    collectionTestReached: false, collectionTestFlags: noFlags,
                    emptyBranchTaken: null, reloadAddress: null,
                    collectionEntryAddress: null, collectionEntryValue: null,
                    collectionEntryTestReached: false, collectionEntryTestFlags: noFlags,
                    nonzeroEntryBranchTaken: null, destination: null, flags,
                    boundaryReached: false, thresholdReachedForUnused: true,
                    entryESPReached: true, entryECXReached: true,
                    callStatusReached: true, postLoggingESIReached: false,
                    endReached: false, startReached: false, reloadReached: false,
                    entryReached: false);
            }

            callReturnInvalidatedFlags = returnedNormally;
            calls.Add(new NetworkDispatcherCollectionGateOpaqueCall(
                OpaqueCallRawFileOffset,
                OpaqueCallTargetRawFileOffset,
                OpaqueCallTargetVirtualAddress,
                Attempted: true,
                ReturnedNormally: returnedNormally));
            if (!returnedNormally)
            {
                return Stop(NetworkDispatcherCollectionGateOutcome.OpaqueCallDidNotReturnNormally,
                    OpaqueCallRawFileOffset, parent, reads, pushes, calls, capture,
                    thresholdReached: true, thresholdValue: threshold,
                    thresholdFlags, thresholdBranchTaken: false,
                    thresholdDestination, callReturnInvalidatedFlags,
                    collectionESI: null, freshEndAddress: null, freshStartAddress: null,
                    difference: null, subtractFlags: noFlags,
                    shiftedDifference: null, shiftFlags: noFlags,
                    collectionTestReached: false, collectionTestFlags: noFlags,
                    emptyBranchTaken: null, reloadAddress: null,
                    collectionEntryAddress: null, collectionEntryValue: null,
                    collectionEntryTestReached: false, collectionEntryTestFlags: noFlags,
                    nonzeroEntryBranchTaken: null, destination: null, flags,
                    boundaryReached: false, thresholdReachedForUnused: true,
                    entryESPReached: true, entryECXReached: true,
                    callStatusReached: true, postLoggingESIReached: false,
                    endReached: false, startReached: false, reloadReached: false,
                    entryReached: false);
            }

            flags = UnknownFlags();
            if (capture.FreshPostLoggingESI is not uint postLoggingESI)
            {
                return Stop(NetworkDispatcherCollectionGateOutcome.MissingPostLoggingESI,
                    CollectionStartRawFileOffset, parent, reads, pushes, calls, capture,
                    thresholdReached: true, thresholdValue: threshold,
                    thresholdFlags, thresholdBranchTaken: false,
                    thresholdDestination, callReturnInvalidatedFlags,
                    collectionESI: null, freshEndAddress: null, freshStartAddress: null,
                    difference: null, subtractFlags: noFlags,
                    shiftedDifference: null, shiftFlags: noFlags,
                    collectionTestReached: false, collectionTestFlags: noFlags,
                    emptyBranchTaken: null, reloadAddress: null,
                    collectionEntryAddress: null, collectionEntryValue: null,
                    collectionEntryTestReached: false, collectionEntryTestFlags: noFlags,
                    nonzeroEntryBranchTaken: null, destination: null, flags,
                    boundaryReached: false, thresholdReachedForUnused: true,
                    entryESPReached: true, entryECXReached: true,
                    callStatusReached: true, postLoggingESIReached: true,
                    endReached: false, startReached: false, reloadReached: false,
                    entryReached: false);
            }

            currentESI = postLoggingESI;
        }
        else
        {
            currentESI = capture.ParentGate.EntryESIReceiver;
            if (currentESI is null)
            {
                return Stop(NetworkDispatcherCollectionGateOutcome.MissingParentEntryESI,
                    CollectionStartRawFileOffset, parent, reads, pushes, calls, capture,
                    thresholdReached: true, thresholdValue: threshold,
                    thresholdFlags, thresholdBranchTaken: true,
                    thresholdDestination, callReturnInvalidatedFlags,
                    collectionESI: null, freshEndAddress: null, freshStartAddress: null,
                    difference: null, subtractFlags: noFlags,
                    shiftedDifference: null, shiftFlags: noFlags,
                    collectionTestReached: false, collectionTestFlags: noFlags,
                    emptyBranchTaken: null, reloadAddress: null,
                    collectionEntryAddress: null, collectionEntryValue: null,
                    collectionEntryTestReached: false, collectionEntryTestFlags: noFlags,
                    nonzeroEntryBranchTaken: null, destination: null, flags,
                    boundaryReached: false, thresholdReachedForUnused: true,
                    entryESPReached: false, entryECXReached: false,
                    callStatusReached: false, postLoggingESIReached: false,
                    endReached: false, startReached: false, reloadReached: false,
                    entryReached: false);
            }
        }

        uint esi = currentESI!.Value;
        uint endAddress = unchecked(esi + EndOffsetFromESI);
        if (capture.FreshEndDWORD is not uint end)
        {
            reads.Add(new NetworkDispatcherCollectionGateRead(
                CollectionStartRawFileOffset,
                NetworkDispatcherCollectionGateReadKind.FreshEndDWORD,
                endAddress,
                Width: 4,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false));
            return Stop(NetworkDispatcherCollectionGateOutcome.MissingFreshEndDWORD,
                CollectionStartRawFileOffset, parent, reads, pushes, calls, capture,
                thresholdReached: true, thresholdValue: threshold,
                thresholdFlags, thresholdBranchTaken: signedGreaterThanOne,
                thresholdDestination, callReturnInvalidatedFlags, esi,
                endAddress, null, null, noFlags, null, noFlags,
                collectionTestReached: false, collectionTestFlags: noFlags,
                emptyBranchTaken: null, reloadAddress: null,
                collectionEntryAddress: null, collectionEntryValue: null,
                collectionEntryTestReached: false, collectionEntryTestFlags: noFlags,
                nonzeroEntryBranchTaken: null, destination: null, flags,
                boundaryReached: false, thresholdReachedForUnused: true,
                entryESPReached: !signedGreaterThanOne,
                entryECXReached: !signedGreaterThanOne,
                callStatusReached: !signedGreaterThanOne,
                postLoggingESIReached: !signedGreaterThanOne,
                endReached: true, startReached: false, reloadReached: false,
                entryReached: false);
        }

        reads.Add(new NetworkDispatcherCollectionGateRead(
            CollectionStartRawFileOffset,
            NetworkDispatcherCollectionGateReadKind.FreshEndDWORD,
            endAddress,
            Width: 4,
            Attempted: true,
            Captured: true,
            Value: end,
            Committed: false));
        NetworkDispatcherCollectionGateFlags zeroEBXFlags = XorZeroFlags();
        uint startAddress = unchecked(esi + StartOffsetFromESI);
        if (capture.FreshStartDWORD is not uint start)
        {
            reads.Add(new NetworkDispatcherCollectionGateRead(
                SubtractStartRawFileOffset,
                NetworkDispatcherCollectionGateReadKind.FreshStartDWORD,
                startAddress,
                Width: 4,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false));
            return Stop(NetworkDispatcherCollectionGateOutcome.MissingFreshStartDWORD,
                SubtractStartRawFileOffset, parent, reads, pushes, calls, capture,
                thresholdReached: true, thresholdValue: threshold,
                thresholdFlags, thresholdBranchTaken: signedGreaterThanOne,
                thresholdDestination, callReturnInvalidatedFlags, esi,
                endAddress, startAddress, null, noFlags, null, noFlags,
                collectionTestReached: false, collectionTestFlags: noFlags,
                emptyBranchTaken: null, reloadAddress: null,
                collectionEntryAddress: null, collectionEntryValue: null,
                collectionEntryTestReached: false, collectionEntryTestFlags: noFlags,
                nonzeroEntryBranchTaken: null, destination: null, flags: zeroEBXFlags,
                boundaryReached: false, thresholdReachedForUnused: true,
                entryESPReached: !signedGreaterThanOne,
                entryECXReached: !signedGreaterThanOne,
                callStatusReached: !signedGreaterThanOne,
                postLoggingESIReached: !signedGreaterThanOne,
                endReached: true, startReached: true, reloadReached: false,
                entryReached: false);
        }

        reads.Add(new NetworkDispatcherCollectionGateRead(
            SubtractStartRawFileOffset,
            NetworkDispatcherCollectionGateReadKind.FreshStartDWORD,
            startAddress,
            Width: 4,
            Attempted: true,
            Captured: true,
            Value: start,
            Committed: false));
        uint difference = unchecked(end - start);
        NetworkDispatcherCollectionGateFlags subtractFlags =
            SubtractFlags(end, start, difference);
        int signedDifference = unchecked((int)difference);
        uint shiftedDifference = unchecked((uint)(signedDifference >> (int)ShiftCount));
        NetworkDispatcherCollectionGateFlags shiftFlags =
            ArithmeticShiftRightFlags(difference, shiftedDifference, ShiftCount);
        NetworkDispatcherCollectionGateFlags collectionTestFlags =
            TestFlags(shiftedDifference);
        bool emptyBranchTaken = shiftedDifference == 0;
        if (emptyBranchTaken)
        {
            return Stop(NetworkDispatcherCollectionGateOutcome.EmptyShiftedDifference,
                EmptyContinuationRawFileOffset, parent, reads, pushes, calls, capture,
                thresholdReached: true, thresholdValue: threshold,
                thresholdFlags, thresholdBranchTaken: signedGreaterThanOne,
                thresholdDestination, callReturnInvalidatedFlags, esi,
                endAddress, startAddress, difference, subtractFlags,
                shiftedDifference, shiftFlags, collectionTestReached: true,
                collectionTestFlags, emptyBranchTaken, reloadAddress: null,
                collectionEntryAddress: null, collectionEntryValue: null,
                collectionEntryTestReached: false, collectionEntryTestFlags: noFlags,
                nonzeroEntryBranchTaken: null,
                destination: EmptyContinuationRawFileOffset,
                flags: collectionTestFlags, boundaryReached: true,
                thresholdReachedForUnused: true,
                entryESPReached: !signedGreaterThanOne,
                entryECXReached: !signedGreaterThanOne,
                callStatusReached: !signedGreaterThanOne,
                postLoggingESIReached: !signedGreaterThanOne,
                endReached: true, startReached: true, reloadReached: false,
                entryReached: false);
        }

        uint? reloadAddress = startAddress;
        if (capture.FreshReloadDWORD is not uint reload)
        {
            reads.Add(new NetworkDispatcherCollectionGateRead(
                ReloadStartRawFileOffset,
                NetworkDispatcherCollectionGateReadKind.FreshReloadDWORD,
                reloadAddress,
                Width: 4,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false));
            return Stop(NetworkDispatcherCollectionGateOutcome.MissingFreshReloadDWORD,
                ReloadStartRawFileOffset, parent, reads, pushes, calls, capture,
                thresholdReached: true, thresholdValue: threshold,
                thresholdFlags, thresholdBranchTaken: signedGreaterThanOne,
                thresholdDestination, callReturnInvalidatedFlags, esi,
                endAddress, startAddress, difference, subtractFlags,
                shiftedDifference, shiftFlags, collectionTestReached: true,
                collectionTestFlags, emptyBranchTaken, reloadAddress,
                collectionEntryAddress: null, collectionEntryValue: null,
                collectionEntryTestReached: false, collectionEntryTestFlags: noFlags,
                nonzeroEntryBranchTaken: null, destination: null,
                flags: collectionTestFlags, boundaryReached: false,
                thresholdReachedForUnused: true,
                entryESPReached: !signedGreaterThanOne,
                entryECXReached: !signedGreaterThanOne,
                callStatusReached: !signedGreaterThanOne,
                postLoggingESIReached: !signedGreaterThanOne,
                endReached: true, startReached: true, reloadReached: true,
                entryReached: false);
        }

        reads.Add(new NetworkDispatcherCollectionGateRead(
            ReloadStartRawFileOffset,
            NetworkDispatcherCollectionGateReadKind.FreshReloadDWORD,
            reloadAddress,
            Width: 4,
            Attempted: true,
            Captured: true,
            Value: reload,
            Committed: false));
        uint entryAddress = unchecked(reload + 0u * BytesPerEntry);
        if (capture.FreshCollectionEntryDWORD is not uint collectionEntry)
        {
            reads.Add(new NetworkDispatcherCollectionGateRead(
                ReadCollectionEntryRawFileOffset,
                NetworkDispatcherCollectionGateReadKind.FreshCollectionEntryDWORD,
                entryAddress,
                Width: 4,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false));
            return Stop(NetworkDispatcherCollectionGateOutcome.MissingFreshCollectionEntry,
                ReadCollectionEntryRawFileOffset, parent, reads, pushes, calls, capture,
                thresholdReached: true, thresholdValue: threshold,
                thresholdFlags, thresholdBranchTaken: signedGreaterThanOne,
                thresholdDestination, callReturnInvalidatedFlags, esi,
                endAddress, startAddress, difference, subtractFlags,
                shiftedDifference, shiftFlags, collectionTestReached: true,
                collectionTestFlags, emptyBranchTaken, reloadAddress,
                entryAddress, null, collectionEntryTestReached: false,
                collectionEntryTestFlags: noFlags, nonzeroEntryBranchTaken: null,
                destination: null, flags: collectionTestFlags,
                boundaryReached: false, thresholdReachedForUnused: true,
                entryESPReached: !signedGreaterThanOne,
                entryECXReached: !signedGreaterThanOne,
                callStatusReached: !signedGreaterThanOne,
                postLoggingESIReached: !signedGreaterThanOne,
                endReached: true, startReached: true, reloadReached: true,
                entryReached: true);
        }

        reads.Add(new NetworkDispatcherCollectionGateRead(
            ReadCollectionEntryRawFileOffset,
            NetworkDispatcherCollectionGateReadKind.FreshCollectionEntryDWORD,
            entryAddress,
            Width: 4,
            Attempted: true,
            Captured: true,
            Value: collectionEntry,
            Committed: false));
        NetworkDispatcherCollectionGateFlags entryTestFlags =
            TestFlags(collectionEntry);
        bool nonzeroEntryBranchTaken = collectionEntry != 0;
        uint destination = nonzeroEntryBranchTaken
            ? NonzeroEntryContinuationRawFileOffset
            : ZeroEntryContinuationRawFileOffset;
        return Stop(nonzeroEntryBranchTaken
                ? NetworkDispatcherCollectionGateOutcome.NonzeroCollectionEntry
                : NetworkDispatcherCollectionGateOutcome.ZeroCollectionEntry,
            destination, parent, reads, pushes, calls, capture,
            thresholdReached: true, thresholdValue: threshold,
            thresholdFlags, thresholdBranchTaken: signedGreaterThanOne,
            thresholdDestination, callReturnInvalidatedFlags, esi,
            endAddress, startAddress, difference, subtractFlags,
            shiftedDifference, shiftFlags, collectionTestReached: true,
            collectionTestFlags, emptyBranchTaken, reloadAddress,
            entryAddress, collectionEntry, collectionEntryTestReached: true,
            entryTestFlags, nonzeroEntryBranchTaken, destination,
            entryTestFlags, boundaryReached: true,
            thresholdReachedForUnused: true,
            entryESPReached: !signedGreaterThanOne,
            entryECXReached: !signedGreaterThanOne,
            callStatusReached: !signedGreaterThanOne,
            postLoggingESIReached: !signedGreaterThanOne,
            endReached: true, startReached: true, reloadReached: true,
            entryReached: true);
    }

    private static NetworkDispatcherCollectionGatePlan Stop(
        NetworkDispatcherCollectionGateOutcome outcome,
        uint stopAt,
        NetworkDispatcherLockGatePlan parent,
        ImmutableArray<NetworkDispatcherCollectionGateRead>.Builder reads,
        ImmutableArray<NetworkDispatcherCollectionGatePush>.Builder pushes,
        ImmutableArray<NetworkDispatcherCollectionGateOpaqueCall>.Builder calls,
        NetworkDispatcherCollectionGateCapture capture,
        bool thresholdReached,
        uint? thresholdValue,
        NetworkDispatcherCollectionGateFlags? thresholdFlags,
        bool? thresholdBranchTaken,
        uint? thresholdDestination,
        bool? opaqueReturnInvalidatedFlags,
        uint? collectionESI,
        uint? freshEndAddress,
        uint? freshStartAddress,
        uint? difference,
        NetworkDispatcherCollectionGateFlags? subtractFlags,
        uint? shiftedDifference,
        NetworkDispatcherCollectionGateFlags? shiftFlags,
        bool collectionTestReached,
        NetworkDispatcherCollectionGateFlags? collectionTestFlags,
        bool? emptyBranchTaken,
        uint? reloadAddress,
        uint? collectionEntryAddress,
        uint? collectionEntryValue,
        bool collectionEntryTestReached,
        NetworkDispatcherCollectionGateFlags? collectionEntryTestFlags,
        bool? nonzeroEntryBranchTaken,
        uint? destination,
        NetworkDispatcherCollectionGateFlags? flags,
        bool boundaryReached,
        bool thresholdReachedForUnused,
        bool entryESPReached,
        bool entryECXReached,
        bool callStatusReached,
        bool postLoggingESIReached,
        bool endReached,
        bool startReached,
        bool reloadReached,
        bool entryReached)
    {
        ImmutableArray<NetworkDispatcherCollectionGateRead> readArray = reads.ToImmutable();
        ImmutableArray<NetworkDispatcherCollectionGatePush> pushArray = pushes.ToImmutable();
        ImmutableArray<NetworkDispatcherCollectionGateOpaqueCall> callArray = calls.ToImmutable();
        bool childGateAdmitted =
            parent.ExactCleanExitAdmitted
            && parent.Outcome
                == NetworkDispatcherLockGateOutcome.NonzeroArgumentFallthroughSelected;
        var captureStates = new NetworkDispatcherCollectionGateCaptureStates(
            CaptureState(thresholdReachedForUnused, capture.FreshThresholdDWORD.HasValue),
            CaptureState(entryESPReached, capture.EntryESP.HasValue),
            CaptureState(entryECXReached, capture.EntryECX.HasValue),
            CaptureState(callStatusReached, capture.OpaqueCallReturnedNormally.HasValue),
            CaptureState(postLoggingESIReached, capture.FreshPostLoggingESI.HasValue),
            CaptureState(endReached, capture.FreshEndDWORD.HasValue),
            CaptureState(startReached, capture.FreshStartDWORD.HasValue),
            CaptureState(reloadReached, capture.FreshReloadDWORD.HasValue),
            CaptureState(entryReached, capture.FreshCollectionEntryDWORD.HasValue));
        return new NetworkDispatcherCollectionGatePlan(
            parent,
            capture,
            outcome,
            readArray,
            pushArray,
            callArray,
            BuildOrderedEffects(parent, readArray, pushArray, callArray),
            childGateAdmitted,
            thresholdReached,
            thresholdValue,
            thresholdFlags,
            thresholdReached,
            thresholdBranchTaken,
            thresholdDestination,
            opaqueReturnInvalidatedFlags,
            collectionESI,
            freshEndAddress,
            freshStartAddress,
            difference,
            subtractFlags,
            readArray.Any(read =>
                read.Kind == NetworkDispatcherCollectionGateReadKind.FreshEndDWORD
                && read.Captured)
                ? XorZeroFlags()
                : null,
            shiftedDifference,
            shiftFlags,
            collectionTestReached,
            collectionTestFlags,
            emptyBranchTaken,
            reloadAddress,
            collectionEntryAddress,
            collectionEntryValue,
            collectionEntryTestReached,
            collectionEntryTestFlags,
            nonzeroEntryBranchTaken,
            destination,
            flags,
            boundaryReached,
            captureStates,
            !entryESPReached && capture.EntryESP.HasValue,
            !entryECXReached && capture.EntryECX.HasValue,
            !callStatusReached && capture.OpaqueCallReturnedNormally.HasValue,
            !postLoggingESIReached && capture.FreshPostLoggingESI.HasValue,
            !thresholdReachedForUnused && capture.FreshThresholdDWORD.HasValue,
            !endReached && capture.FreshEndDWORD.HasValue,
            !startReached && capture.FreshStartDWORD.HasValue,
            !reloadReached && capture.FreshReloadDWORD.HasValue,
            !entryReached && capture.FreshCollectionEntryDWORD.HasValue,
            stopAt);
    }

    private static NetworkDispatcherCollectionGateCaptureState CaptureState(
        bool reached,
        bool supplied) =>
        reached
            ? supplied
                ? NetworkDispatcherCollectionGateCaptureState.Captured
                : NetworkDispatcherCollectionGateCaptureState.Missing
            : supplied
                ? NetworkDispatcherCollectionGateCaptureState.SuppliedUnused
                : NetworkDispatcherCollectionGateCaptureState.Unreached;

    private static uint AddPush(
        ImmutableArray<NetworkDispatcherCollectionGatePush>.Builder pushes,
        uint rawFileOffset,
        NetworkDispatcherCollectionGatePushKind kind,
        uint stackPointerBefore,
        uint value,
        bool committed)
    {
        uint after = unchecked(stackPointerBefore - 4);
        pushes.Add(new NetworkDispatcherCollectionGatePush(
            rawFileOffset, kind, stackPointerBefore, after, value,
            Attempted: true, Committed: committed));
        return after;
    }

    private static void AddUnresolvedPush(
        ImmutableArray<NetworkDispatcherCollectionGatePush>.Builder pushes,
        uint rawFileOffset,
        NetworkDispatcherCollectionGatePushKind kind,
        uint? stackPointerBefore,
        uint? stackPointerAfter,
        uint? value,
        bool attempted,
        bool committed) =>
        pushes.Add(new NetworkDispatcherCollectionGatePush(
            rawFileOffset, kind, stackPointerBefore, stackPointerAfter, value,
            attempted, committed));

    private static ImmutableArray<NetworkDispatcherCollectionGateOrderedEffect>
        BuildOrderedEffects(
            NetworkDispatcherLockGatePlan parent,
            ImmutableArray<NetworkDispatcherCollectionGateRead> reads,
            ImmutableArray<NetworkDispatcherCollectionGatePush> pushes,
            ImmutableArray<NetworkDispatcherCollectionGateOpaqueCall> calls)
    {
        var effects =
            new List<(uint Offset, int Order, NetworkDispatcherCollectionGateOrderedEffect Effect)>();
        foreach (NetworkDispatcherLockGateOrderedEffect effect in parent.OrderedEffects)
        {
            NetworkDispatcherCollectionGateOrderedEffectKind kind = effect.Kind switch
            {
                NetworkDispatcherLockGateOrderedEffectKind.LockPush =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.ParentLockPush,
                NetworkDispatcherLockGateOrderedEffectKind.OpaqueImportCall =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.ParentOpaqueImportCall,
                NetworkDispatcherLockGateOrderedEffectKind.FreshCounterRead =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.ParentFreshCounterRead,
                NetworkDispatcherLockGateOrderedEffectKind.CounterIncrement =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.ParentCounterIncrement,
                NetworkDispatcherLockGateOrderedEffectKind.FreshArgumentByteRead =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.ParentFreshArgumentByteRead,
                NetworkDispatcherLockGateOrderedEffectKind.EntryFrameClear =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.ParentEntryFrameClear,
                _ => NetworkDispatcherCollectionGateOrderedEffectKind.ParentReceiverByteClear
            };
            int order = effect.Kind switch
            {
                NetworkDispatcherLockGateOrderedEffectKind.FreshCounterRead => 0,
                NetworkDispatcherLockGateOrderedEffectKind.CounterIncrement => 1,
                _ => 0
            };
            effects.Add((effect.RawFileOffset, order,
                new NetworkDispatcherCollectionGateOrderedEffect(
                    effect.RawFileOffset, kind, effect.Address, effect.Width,
                    effect.Value, effect.Attempted, effect.Committed)));
        }

        foreach (NetworkDispatcherCollectionGateRead read in reads)
        {
            NetworkDispatcherCollectionGateOrderedEffectKind kind = read.Kind switch
            {
                NetworkDispatcherCollectionGateReadKind.ThresholdDWORD =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.ThresholdDWORDRead,
                NetworkDispatcherCollectionGateReadKind.FreshEndDWORD =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.FreshEndDWORDRead,
                NetworkDispatcherCollectionGateReadKind.FreshStartDWORD =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.FreshStartDWORDRead,
                NetworkDispatcherCollectionGateReadKind.FreshReloadDWORD =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.FreshReloadDWORDRead,
                _ => NetworkDispatcherCollectionGateOrderedEffectKind.FreshCollectionEntryDWORDRead
            };
            effects.Add((read.RawFileOffset, 2,
                new NetworkDispatcherCollectionGateOrderedEffect(
                    read.RawFileOffset, kind, read.Address, read.Width, read.Value,
                    read.Attempted, read.Committed)));
        }

        foreach (NetworkDispatcherCollectionGatePush push in pushes)
        {
            NetworkDispatcherCollectionGateOrderedEffectKind kind = push.Kind switch
            {
                NetworkDispatcherCollectionGatePushKind.InheritedECX =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.InheritedECXPush,
                NetworkDispatcherCollectionGatePushKind.Zero =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.ZeroPush,
                NetworkDispatcherCollectionGatePushKind.AddressImmediate =>
                    NetworkDispatcherCollectionGateOrderedEffectKind.AddressImmediatePush,
                _ => NetworkDispatcherCollectionGateOrderedEffectKind.OnePush
            };
            effects.Add((push.RawFileOffset, 3,
                new NetworkDispatcherCollectionGateOrderedEffect(
                    push.RawFileOffset, kind, push.StackPointerAfter, 4,
                    push.Value, push.Attempted, push.Committed)));
        }

        foreach (NetworkDispatcherCollectionGateOpaqueCall call in calls)
        {
            effects.Add((call.RawFileOffset, 4,
                new NetworkDispatcherCollectionGateOrderedEffect(
                    call.RawFileOffset, NetworkDispatcherCollectionGateOrderedEffectKind.OpaqueCall,
                    call.TargetVirtualAddress, 4, null, call.Attempted, Committed: false)));
        }

        effects.Sort(static (left, right) =>
        {
            int offsetOrder = left.Offset.CompareTo(right.Offset);
            return offsetOrder != 0 ? offsetOrder : left.Order.CompareTo(right.Order);
        });
        return effects.Select(item => item.Effect).ToImmutableArray();
    }

    private static NetworkDispatcherCollectionGateFlags ThresholdCompareFlags(uint value)
    {
        uint result = unchecked(value - 1);
        return new(
            Carry: value < 1,
            Parity: HasEvenParity((byte)result),
            Auxiliary: ((value ^ 1 ^ result) & 0x10) != 0,
            Zero: value == 1,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((value ^ 1) & (value ^ result) & 0x80000000) != 0);
    }

    private static NetworkDispatcherCollectionGateFlags XorZeroFlags() =>
        new(Carry: false, Parity: true, Auxiliary: null, Zero: true,
            Sign: false, Overflow: false);

    private static NetworkDispatcherCollectionGateFlags SubtractFlags(
        uint left,
        uint right,
        uint result) =>
        new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            Auxiliary: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static NetworkDispatcherCollectionGateFlags ArithmeticShiftRightFlags(
        uint before,
        uint after,
        uint count) =>
        new(
            Carry: count == 0 ? null : ((before >> ((int)count - 1)) & 1) != 0,
            Parity: HasEvenParity((byte)after),
            Auxiliary: null,
            Zero: after == 0,
            Sign: (after & 0x80000000) != 0,
            Overflow: count == 1 ? false : null);

    private static NetworkDispatcherCollectionGateFlags TestFlags(uint value) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)value),
            Auxiliary: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static NetworkDispatcherCollectionGateFlags UnknownFlags() =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null,
            Sign: null, Overflow: null);

    private static NetworkDispatcherCollectionGateFlags? ConvertParentFlags(
        NetworkDispatcherLockGateFlags? flags)
    {
        if (flags is not NetworkDispatcherLockGateFlags parentFlags)
        {
            return null;
        }

        return new NetworkDispatcherCollectionGateFlags(
            parentFlags.Carry,
            parentFlags.Parity,
            parentFlags.Auxiliary,
            parentFlags.Zero,
            parentFlags.Sign,
            parentFlags.Overflow);
    }

    private static bool HasEvenParity(byte value)
    {
        int setBits = 0;
        for (int bit = 0; bit < 8; bit++)
        {
            setBits += (value >> bit) & 1;
        }

        return (setBits & 1) == 0;
    }
}
