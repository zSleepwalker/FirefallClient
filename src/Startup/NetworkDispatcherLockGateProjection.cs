using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum NetworkDispatcherLockGateOutcome
{
    MissingResultEAX,
    MissingReceiverBeforePush,
    MissingOpaqueImportReturnStatus,
    OpaqueImportDidNotReturnNormally,
    MissingReceiverBeforeIncrement,
    MissingFreshCounterValue,
    MissingEntryEBP,
    MissingFreshArgumentByte,
    ZeroArgumentBranchSelected,
    NonzeroArgumentFallthroughSelected
}

public enum NetworkDispatcherLockGateReadKind
{
    FreshCounter,
    FreshArgumentByte
}

public enum NetworkDispatcherLockGateWriteKind
{
    CounterIncrement,
    EntryFrameClear,
    ReceiverByteClear
}

public enum NetworkDispatcherLockGateOrderedEffectKind
{
    LockPush,
    OpaqueImportCall,
    FreshCounterRead,
    CounterIncrement,
    FreshArgumentByteRead,
    EntryFrameClear,
    ReceiverByteClear
}

public readonly record struct NetworkDispatcherLockGateFlags(
    bool? Carry,
    bool? Parity,
    bool? Auxiliary,
    bool? Zero,
    bool? Sign,
    bool? Overflow);

public readonly record struct NetworkDispatcherLockGateCapture(
    uint? ResultEAX,
    uint? EntryESIReceiver,
    uint? FreshCounterValue,
    uint? EntryEBP,
    byte? FreshArgumentByte,
    bool? OpaqueImportReturnedNormally);

public readonly record struct NetworkDispatcherLockGateRead(
    uint RawFileOffset,
    NetworkDispatcherLockGateReadKind Kind,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public readonly record struct NetworkDispatcherLockGateWrite(
    uint RawFileOffset,
    NetworkDispatcherLockGateWriteKind Kind,
    uint Address,
    byte Width,
    uint Value,
    bool Attempted,
    bool Committed);

public readonly record struct NetworkDispatcherLockGatePush(
    uint RawFileOffset,
    uint Value,
    bool Attempted,
    bool Committed);

public readonly record struct NetworkDispatcherLockGateOpaqueCall(
    uint RawFileOffset,
    uint ImportCellVirtualAddress,
    bool Attempted,
    bool? ReturnedNormally);

public readonly record struct NetworkDispatcherLockGateOrderedEffect(
    uint RawFileOffset,
    NetworkDispatcherLockGateOrderedEffectKind Kind,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Committed);

public sealed record NetworkDispatcherLockGatePlan(
    NetworkDispatcherLockGateOutcome Outcome,
    ImmutableArray<NetworkDispatcherLockGateRead> Reads,
    ImmutableArray<NetworkDispatcherLockGateWrite> Writes,
    ImmutableArray<NetworkDispatcherLockGatePush> Pushes,
    ImmutableArray<NetworkDispatcherLockGateOpaqueCall> OpaqueCalls,
    ImmutableArray<NetworkDispatcherLockGateOrderedEffect> OrderedEffects,
    bool TestReached,
    bool? TestBranchTaken,
    NetworkDispatcherLockGateFlags? TestFlags,
    uint? DerivedLockPointer,
    uint? CounterAddress,
    uint? IncrementedCounterValue,
    NetworkDispatcherLockGateFlags? IncrementFlags,
    bool? OpaqueCallReturnInvalidatedFlags,
    bool CompareReached,
    uint? CompareAddress,
    byte? CompareValue,
    NetworkDispatcherLockGateFlags? CompareFlags,
    bool BranchReached,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset,
    NetworkDispatcherLockGateFlags? FlagsAtStop,
    bool UnreachedReceiverCaptureProvided,
    bool UnreachedCounterCaptureProvided,
    bool UnreachedEntryEBPCaptureProvided,
    bool UnreachedArgumentByteCaptureProvided,
    bool UnreachedOpaqueReturnStatusProvided,
    uint StopAtRawFileOffsetExclusive)
{
    public bool BoundedContinuationEntered => false;

    public bool BoundedCompletionBoundaryReached => BranchReached;

    public bool ExactCleanExitAdmitted =>
        BoundedCompletionBoundaryReached
        && !UnreachedReceiverCaptureProvided
        && !UnreachedCounterCaptureProvided
        && !UnreachedEntryEBPCaptureProvided
        && !UnreachedArgumentByteCaptureProvided
        && !UnreachedOpaqueReturnStatusProvided;
}

/// <summary>
/// Value-only projection of the observed lock/counter gate. It does not model
/// the opaque import target or either unentered fallback continuation.
/// </summary>
public static class NetworkDispatcherLockGateProjection
{
    public const uint EntryVirtualAddress = 0x00FBE9EB;
    public const uint EntryRawFileOffset = 0x00BBDDEB;
    public const uint RawToVirtualAddressDelta = 0x00400C00;

    public const uint TestRawFileOffset = 0x00BBDDEB;
    public const uint TestBranchRawFileOffset = 0x00BBDDED;
    public const uint PushLockRawFileOffset = 0x00BBDDEF;
    public const uint OpaqueImportCallRawFileOffset = 0x00BBDDF0;
    public const uint IncrementCounterRawFileOffset = 0x00BBDDF6;
    public const uint CompareArgumentRawFileOffset = 0x00BBDDF9;
    public const uint ClearEntryFrameRawFileOffset = 0x00BBDDFD;
    public const uint ClearReceiverByteRawFileOffset = 0x00BBDE04;
    public const uint ZeroBranchRawFileOffset = 0x00BBDE08;
    public const uint BoundedFallthroughRawFileOffset = 0x00BBDE0E;
    public const uint ZeroContinuationRawFileOffset = 0x00BBDECA;
    public const uint NonzeroContinuationRawFileOffset = 0x00BBDE0E;

    public const uint OpaqueImportCellVirtualAddress = 0x01AC4278;
    public const uint LockPointerOffsetFromReceiver = 0x6C;
    public const uint CounterOffsetFromLock = 0x30;
    public const uint CounterOffsetFromReceiver = 0x9C;
    public const uint ArgumentByteOffsetFromEBP = 0x08;
    public const uint EntryFrameClearOffsetFromEBP = 0xFFFFFFFC;
    public const uint ReceiverByteClearOffset = 0x64;

    public const string CapturedWindowBytes =
        "85C0750753FF157842AC01FF4330807D0800C745FC00000000C64664000F84BC000000";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherLockGatePlan Project(
        NetworkDispatcherLockGateCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<NetworkDispatcherLockGateRead>();
        var writes = ImmutableArray.CreateBuilder<NetworkDispatcherLockGateWrite>();
        var pushes = ImmutableArray.CreateBuilder<NetworkDispatcherLockGatePush>();
        var opaqueCalls = ImmutableArray.CreateBuilder<NetworkDispatcherLockGateOpaqueCall>();
        var noFlags = (NetworkDispatcherLockGateFlags?)null;

        if (capture.ResultEAX is not uint resultEAX)
        {
            return Stop(NetworkDispatcherLockGateOutcome.MissingResultEAX,
                TestRawFileOffset, reads, writes, pushes, opaqueCalls,
                testReached: false, testBranchTaken: null, derivedLock: null,
                counterAddress: null, incrementedCounter: null,
                opaqueReturnInvalidatedFlags: null, compareReached: false,
                compareAddress: null, compareValue: null, compareFlags: noFlags,
                branchReached: false, branchTaken: null, destination: null,
                flags: noFlags, capture, receiverReached: false,
                counterReached: false, ebpReached: false, argumentReached: false,
                opaqueStatusReached: false);
        }

        NetworkDispatcherLockGateFlags flags = TestFlags(resultEAX);
        bool testBranchTaken = resultEAX != 0;
        uint? derivedLock = null;
        uint? counterAddress = null;
        uint? incrementedCounter = null;
        bool? opaqueReturnInvalidatedFlags = null;

        if (!testBranchTaken)
        {
            if (capture.EntryESIReceiver is not uint receiverBeforePush)
            {
                return Stop(NetworkDispatcherLockGateOutcome.MissingReceiverBeforePush,
                    PushLockRawFileOffset, reads, writes, pushes, opaqueCalls,
                    testReached: true, testBranchTaken, derivedLock: null,
                    counterAddress: null, incrementedCounter: null,
                    opaqueReturnInvalidatedFlags: null, compareReached: false,
                    compareAddress: null, compareValue: null, compareFlags: noFlags,
                    branchReached: false, branchTaken: null, destination: null,
                    flags, capture, receiverReached: true, counterReached: false,
                    ebpReached: false, argumentReached: false,
                    opaqueStatusReached: false);
            }

            derivedLock = unchecked(receiverBeforePush + LockPointerOffsetFromReceiver);
            pushes.Add(new NetworkDispatcherLockGatePush(
                PushLockRawFileOffset, derivedLock.Value, Attempted: true, Committed: true));

            opaqueCalls.Add(new NetworkDispatcherLockGateOpaqueCall(
                OpaqueImportCallRawFileOffset,
                OpaqueImportCellVirtualAddress,
                Attempted: true,
                ReturnedNormally: capture.OpaqueImportReturnedNormally));

            if (capture.OpaqueImportReturnedNormally is not bool returnedNormally)
            {
                return Stop(NetworkDispatcherLockGateOutcome.MissingOpaqueImportReturnStatus,
                    OpaqueImportCallRawFileOffset, reads, writes, pushes, opaqueCalls,
                    testReached: true, testBranchTaken, derivedLock,
                    counterAddress: null, incrementedCounter: null,
                    opaqueReturnInvalidatedFlags: null, compareReached: false,
                    compareAddress: null, compareValue: null, compareFlags: noFlags,
                    branchReached: false, branchTaken: null, destination: null,
                    flags, capture, receiverReached: true,
                    counterReached: false, ebpReached: false, argumentReached: false,
                    opaqueStatusReached: true);
            }

            opaqueReturnInvalidatedFlags = returnedNormally ? true : null;
            if (!returnedNormally)
            {
                return Stop(NetworkDispatcherLockGateOutcome.OpaqueImportDidNotReturnNormally,
                    OpaqueImportCallRawFileOffset, reads, writes, pushes, opaqueCalls,
                    testReached: true, testBranchTaken, derivedLock,
                    counterAddress: null, incrementedCounter: null,
                    opaqueReturnInvalidatedFlags, compareReached: false,
                    compareAddress: null, compareValue: null, compareFlags: noFlags,
                    branchReached: false, branchTaken: null, destination: null,
                    flags, capture, receiverReached: true,
                    counterReached: false, ebpReached: false, argumentReached: false,
                    opaqueStatusReached: true);
            }

            flags = noFlags.GetValueOrDefault();
        }

        if (capture.EntryESIReceiver is not uint receiver)
        {
            return Stop(NetworkDispatcherLockGateOutcome.MissingReceiverBeforeIncrement,
                IncrementCounterRawFileOffset, reads, writes, pushes, opaqueCalls,
                testReached: true, testBranchTaken, derivedLock,
                counterAddress: null, incrementedCounter: null,
                opaqueReturnInvalidatedFlags, compareReached: false,
                compareAddress: null, compareValue: null, compareFlags: noFlags,
                branchReached: false, branchTaken: null, destination: null,
                flags, capture, receiverReached: true, counterReached: false,
                ebpReached: false, argumentReached: false,
                opaqueStatusReached: !testBranchTaken);
        }

        derivedLock ??= unchecked(receiver + LockPointerOffsetFromReceiver);
        counterAddress = unchecked(derivedLock.Value + CounterOffsetFromLock);
        if (counterAddress != unchecked(receiver + CounterOffsetFromReceiver))
        {
            throw new InvalidOperationException("The derived lock/counter offsets disagree.");
        }

        if (capture.FreshCounterValue is not uint counterValue)
        {
            reads.Add(new NetworkDispatcherLockGateRead(
                IncrementCounterRawFileOffset,
                NetworkDispatcherLockGateReadKind.FreshCounter,
                counterAddress,
                Width: 4,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false));
            return Stop(NetworkDispatcherLockGateOutcome.MissingFreshCounterValue,
                IncrementCounterRawFileOffset, reads, writes, pushes, opaqueCalls,
                testReached: true, testBranchTaken, derivedLock, counterAddress,
                incrementedCounter: null, opaqueReturnInvalidatedFlags,
                compareReached: false, compareAddress: null, compareValue: null,
                compareFlags: noFlags, branchReached: false, branchTaken: null,
                destination: null, flags, capture, receiverReached: true,
                counterReached: true, ebpReached: false, argumentReached: false,
                opaqueStatusReached: !testBranchTaken);
        }

        reads.Add(new NetworkDispatcherLockGateRead(
            IncrementCounterRawFileOffset,
            NetworkDispatcherLockGateReadKind.FreshCounter,
            counterAddress,
            Width: 4,
            Attempted: true,
            Captured: true,
            Value: counterValue,
            Committed: false));

        incrementedCounter = unchecked(counterValue + 1);
        writes.Add(new NetworkDispatcherLockGateWrite(
            IncrementCounterRawFileOffset,
            NetworkDispatcherLockGateWriteKind.CounterIncrement,
            counterAddress.Value,
            Width: 4,
            Value: incrementedCounter.Value,
            Attempted: true,
            Committed: true));
        flags = IncrementFlags(counterValue, incrementedCounter.Value, flags.Carry);

        if (capture.EntryEBP is not uint ebp)
        {
            return Stop(NetworkDispatcherLockGateOutcome.MissingEntryEBP,
                CompareArgumentRawFileOffset, reads, writes, pushes, opaqueCalls,
                testReached: true, testBranchTaken, derivedLock, counterAddress,
                incrementedCounter, opaqueReturnInvalidatedFlags,
                compareReached: false, compareAddress: null, compareValue: null,
                compareFlags: noFlags, branchReached: false, branchTaken: null,
                destination: null, flags, capture, receiverReached: true,
                counterReached: true, ebpReached: true, argumentReached: false,
                opaqueStatusReached: !testBranchTaken);
        }

        uint argumentAddress = unchecked(ebp + ArgumentByteOffsetFromEBP);
        if (capture.FreshArgumentByte is not byte argument)
        {
            reads.Add(new NetworkDispatcherLockGateRead(
                CompareArgumentRawFileOffset,
                NetworkDispatcherLockGateReadKind.FreshArgumentByte,
                argumentAddress,
                Width: 1,
                Attempted: true,
                Captured: false,
                Value: null,
                Committed: false));
            return Stop(NetworkDispatcherLockGateOutcome.MissingFreshArgumentByte,
                CompareArgumentRawFileOffset, reads, writes, pushes, opaqueCalls,
                testReached: true, testBranchTaken, derivedLock, counterAddress,
                incrementedCounter, opaqueReturnInvalidatedFlags,
                compareReached: false, argumentAddress, compareValue: null,
                compareFlags: noFlags, branchReached: false, branchTaken: null,
                destination: null, flags, capture, receiverReached: true,
                counterReached: true, ebpReached: true, argumentReached: true,
                opaqueStatusReached: !testBranchTaken);
        }

        reads.Add(new NetworkDispatcherLockGateRead(
            CompareArgumentRawFileOffset,
            NetworkDispatcherLockGateReadKind.FreshArgumentByte,
            argumentAddress,
            Width: 1,
            Attempted: true,
            Captured: true,
            Value: argument,
            Committed: false));
        NetworkDispatcherLockGateFlags compareFlags = CompareByteToZeroFlags(argument);

        writes.Add(new NetworkDispatcherLockGateWrite(
            ClearEntryFrameRawFileOffset,
            NetworkDispatcherLockGateWriteKind.EntryFrameClear,
            unchecked(ebp + EntryFrameClearOffsetFromEBP),
            Width: 4,
            Value: 0,
            Attempted: true,
            Committed: true));
        writes.Add(new NetworkDispatcherLockGateWrite(
            ClearReceiverByteRawFileOffset,
            NetworkDispatcherLockGateWriteKind.ReceiverByteClear,
            unchecked(receiver + ReceiverByteClearOffset),
            Width: 1,
            Value: 0,
            Attempted: true,
            Committed: true));

        bool branchTaken = argument == 0;
        uint destination = branchTaken
            ? ZeroContinuationRawFileOffset
            : NonzeroContinuationRawFileOffset;
        return Stop(branchTaken
                ? NetworkDispatcherLockGateOutcome.ZeroArgumentBranchSelected
                : NetworkDispatcherLockGateOutcome.NonzeroArgumentFallthroughSelected,
            destination, reads, writes, pushes, opaqueCalls,
            testReached: true, testBranchTaken, derivedLock, counterAddress,
            incrementedCounter, opaqueReturnInvalidatedFlags,
            compareReached: true, argumentAddress, argument, compareFlags,
            branchReached: true, branchTaken, destination, compareFlags, capture,
            receiverReached: true, counterReached: true, ebpReached: true,
            argumentReached: true, opaqueStatusReached: !testBranchTaken);
    }

    private static NetworkDispatcherLockGatePlan Stop(
        NetworkDispatcherLockGateOutcome outcome,
        uint stopAt,
        ImmutableArray<NetworkDispatcherLockGateRead>.Builder reads,
        ImmutableArray<NetworkDispatcherLockGateWrite>.Builder writes,
        ImmutableArray<NetworkDispatcherLockGatePush>.Builder pushes,
        ImmutableArray<NetworkDispatcherLockGateOpaqueCall>.Builder opaqueCalls,
        bool testReached,
        bool? testBranchTaken,
        uint? derivedLock,
        uint? counterAddress,
        uint? incrementedCounter,
        bool? opaqueReturnInvalidatedFlags,
        bool compareReached,
        uint? compareAddress,
        byte? compareValue,
        NetworkDispatcherLockGateFlags? compareFlags,
        bool branchReached,
        bool? branchTaken,
        uint? destination,
        NetworkDispatcherLockGateFlags? flags,
        NetworkDispatcherLockGateCapture capture,
        bool receiverReached,
        bool counterReached,
        bool ebpReached,
        bool argumentReached,
        bool opaqueStatusReached)
    {
        ImmutableArray<NetworkDispatcherLockGateRead> readArray = reads.ToImmutable();
        ImmutableArray<NetworkDispatcherLockGateWrite> writeArray = writes.ToImmutable();
        ImmutableArray<NetworkDispatcherLockGatePush> pushArray = pushes.ToImmutable();
        ImmutableArray<NetworkDispatcherLockGateOpaqueCall> opaqueCallArray =
            opaqueCalls.ToImmutable();
        NetworkDispatcherLockGateFlags? testFlags =
            capture.ResultEAX is uint resultEAX ? TestFlags(resultEAX) : null;
        NetworkDispatcherLockGateFlags? incrementFlags =
            capture.FreshCounterValue is uint counterBefore
            && incrementedCounter is uint counterAfter
            && writeArray.Any(write =>
                write.Kind == NetworkDispatcherLockGateWriteKind.CounterIncrement)
                ? IncrementFlags(counterBefore, counterAfter,
                    capture.ResultEAX == 0 ? null : false)
                : null;

        return new NetworkDispatcherLockGatePlan(
            outcome,
            readArray,
            writeArray,
            pushArray,
            opaqueCallArray,
            BuildOrderedEffects(readArray, writeArray, pushArray, opaqueCallArray),
            testReached,
            testBranchTaken,
            testFlags,
            derivedLock,
            counterAddress,
            incrementedCounter,
            incrementFlags,
            opaqueReturnInvalidatedFlags,
            compareReached,
            compareAddress,
            compareValue,
            compareFlags,
            branchReached,
            branchTaken,
            destination,
            flags,
            UnreachedReceiverCaptureProvided:
                !receiverReached && capture.EntryESIReceiver.HasValue,
            UnreachedCounterCaptureProvided:
                !counterReached && capture.FreshCounterValue.HasValue,
            UnreachedEntryEBPCaptureProvided:
                !ebpReached && capture.EntryEBP.HasValue,
            UnreachedArgumentByteCaptureProvided:
                !argumentReached && capture.FreshArgumentByte.HasValue,
            UnreachedOpaqueReturnStatusProvided:
                !opaqueStatusReached && capture.OpaqueImportReturnedNormally.HasValue,
            stopAt);
    }

    private static ImmutableArray<NetworkDispatcherLockGateOrderedEffect> BuildOrderedEffects(
        ImmutableArray<NetworkDispatcherLockGateRead> reads,
        ImmutableArray<NetworkDispatcherLockGateWrite> writes,
        ImmutableArray<NetworkDispatcherLockGatePush> pushes,
        ImmutableArray<NetworkDispatcherLockGateOpaqueCall> opaqueCalls)
    {
        var effects =
            new List<(uint RawFileOffset, int Order, NetworkDispatcherLockGateOrderedEffect Effect)>();
        foreach (NetworkDispatcherLockGatePush push in pushes)
        {
            effects.Add((push.RawFileOffset, 0,
                new NetworkDispatcherLockGateOrderedEffect(
                    push.RawFileOffset,
                    NetworkDispatcherLockGateOrderedEffectKind.LockPush,
                    Address: null,
                    Width: 4,
                    Value: push.Value,
                    Attempted: push.Attempted,
                    Committed: push.Committed)));
        }

        foreach (NetworkDispatcherLockGateOpaqueCall call in opaqueCalls)
        {
            effects.Add((call.RawFileOffset, 1,
                new NetworkDispatcherLockGateOrderedEffect(
                    call.RawFileOffset,
                    NetworkDispatcherLockGateOrderedEffectKind.OpaqueImportCall,
                    call.ImportCellVirtualAddress,
                    Width: 4,
                    Value: null,
                    Attempted: call.Attempted,
                    Committed: false)));
        }

        foreach (NetworkDispatcherLockGateRead read in reads)
        {
            var kind = read.Kind == NetworkDispatcherLockGateReadKind.FreshCounter
                ? NetworkDispatcherLockGateOrderedEffectKind.FreshCounterRead
                : NetworkDispatcherLockGateOrderedEffectKind.FreshArgumentByteRead;
            effects.Add((read.RawFileOffset, 2,
                new NetworkDispatcherLockGateOrderedEffect(
                    read.RawFileOffset, kind, read.Address, read.Width, read.Value,
                    read.Attempted, read.Committed)));
        }

        foreach (NetworkDispatcherLockGateWrite write in writes)
        {
            var kind = write.Kind switch
            {
                NetworkDispatcherLockGateWriteKind.CounterIncrement =>
                    NetworkDispatcherLockGateOrderedEffectKind.CounterIncrement,
                NetworkDispatcherLockGateWriteKind.EntryFrameClear =>
                    NetworkDispatcherLockGateOrderedEffectKind.EntryFrameClear,
                _ => NetworkDispatcherLockGateOrderedEffectKind.ReceiverByteClear
            };
            effects.Add((write.RawFileOffset, 3,
                new NetworkDispatcherLockGateOrderedEffect(
                    write.RawFileOffset, kind, write.Address, write.Width, write.Value,
                    write.Attempted, write.Committed)));
        }

        effects.Sort(static (left, right) =>
        {
            int offsetOrder = left.RawFileOffset.CompareTo(right.RawFileOffset);
            return offsetOrder != 0 ? offsetOrder : left.Order.CompareTo(right.Order);
        });
        return effects.Select(item => item.Effect).ToImmutableArray();
    }

    private static NetworkDispatcherLockGateFlags TestFlags(uint value) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)value),
            Auxiliary: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static NetworkDispatcherLockGateFlags IncrementFlags(
        uint before,
        uint after,
        bool? preservedCarry) =>
        new(
            Carry: preservedCarry,
            Parity: HasEvenParity((byte)after),
            Auxiliary: (before & 0x0F) == 0x0F,
            Zero: after == 0,
            Sign: (after & 0x80000000) != 0,
            Overflow: before == 0x7FFFFFFF);

    private static NetworkDispatcherLockGateFlags CompareByteToZeroFlags(byte value) =>
        new(
            Carry: false,
            Parity: HasEvenParity(value),
            Auxiliary: false,
            Zero: value == 0,
            Sign: (value & 0x80) != 0,
            Overflow: false);

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
