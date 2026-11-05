using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherRegisterRestoreCapture(
    NetworkDispatcherFinalReturnSehStoreCapture ParentCapture,
    uint? FreshPostCallESP,
    uint? FreshPopECXDWORD,
    uint? FreshPopEDIDWORD,
    uint? FreshPopESIDWORD,
    uint? FreshPopEBXDWORD);

public enum NetworkDispatcherRegisterRestoreCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public readonly record struct NetworkDispatcherRegisterRestoreCaptureStates(
    NetworkDispatcherRegisterRestoreCaptureState FreshPostCallESP,
    NetworkDispatcherRegisterRestoreCaptureState FreshPopECXDWORD,
    NetworkDispatcherRegisterRestoreCaptureState FreshPopEDIDWORD,
    NetworkDispatcherRegisterRestoreCaptureState FreshPopESIDWORD,
    NetworkDispatcherRegisterRestoreCaptureState FreshPopEBXDWORD);

public enum NetworkDispatcherRegisterRestoreOutcome
{
    ParentHandoffNotAdmitted,
    MissingFreshPostCallESP,
    MissingFreshPopECXDWORD,
    MissingFreshPopEDIDWORD,
    MissingFreshPopESIDWORD,
    MissingFreshPopEBXDWORD,
    BoundedBeforeMoveESPToEBP
}

public enum NetworkDispatcherRegisterRestoreRegister
{
    ECX,
    EDI,
    ESI,
    EBX
}

public enum NetworkDispatcherRegisterRestoreOrderedEffectKind
{
    ParentFinalReturnSehStoreEffect,
    FreshPostCallESPObservation,
    PopStackDWORDRead,
    LogicalPopRegisterAndESPProjection
}

public readonly record struct NetworkDispatcherRegisterRestoreRegisters(
    uint? ECX,
    uint? EDI,
    uint? ESI,
    uint? EBX);

public readonly record struct NetworkDispatcherRegisterRestoreESPObservation(
    uint RawFileOffset,
    uint? Value,
    bool Captured);

public readonly record struct NetworkDispatcherRegisterRestoreRead(
    uint RawFileOffset,
    uint Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public readonly record struct NetworkDispatcherRegisterRestorePop(
    uint RawFileOffset,
    NetworkDispatcherRegisterRestoreRegister Register,
    uint StackAddress,
    uint? StackValue,
    uint StackPointerBefore,
    uint StackPointerAfter,
    uint? RegisterValueAfter,
    bool Attempted,
    bool StackReadCaptured,
    bool RegisterValueDerivedFromRead,
    bool ESPAdvancedInProjection);

public readonly record struct NetworkDispatcherRegisterRestoreOrderedEffect(
    int Sequence,
    NetworkDispatcherFinalReturnSehStoreOrderedEffect? ParentEffect,
    uint RawFileOffset,
    NetworkDispatcherRegisterRestoreOrderedEffectKind Kind,
    NetworkDispatcherRegisterRestoreESPObservation? ESPObservation,
    NetworkDispatcherRegisterRestoreRead? Read,
    NetworkDispatcherRegisterRestorePop? Pop);

public sealed record NetworkDispatcherRegisterRestorePlan(
    NetworkDispatcherFinalReturnSehStorePlan ParentPlan,
    NetworkDispatcherRegisterRestoreCapture Capture,
    NetworkDispatcherRegisterRestoreOutcome Outcome,
    ImmutableArray<NetworkDispatcherRegisterRestoreRead> Reads,
    ImmutableArray<NetworkDispatcherRegisterRestorePop> Pops,
    ImmutableArray<NetworkDispatcherRegisterRestoreOrderedEffect> OrderedEffects,
    ImmutableArray<uint> ReachedInstructionRawFileOffsets,
    NetworkDispatcherRegisterRestoreCaptureStates CaptureStates,
    bool ChildGateAdmitted,
    uint EntryRawFileOffset,
    uint? InitialESP,
    uint? CurrentESPAtStop,
    NetworkDispatcherRegisterRestoreRegisters RegistersAtStop,
    NetworkDispatcherFencedPublicationFlags? FlagsAtStop,
    bool BoundedCompletionBoundaryReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    uint StopAtRawFileOffsetExclusive)
{
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryWritesCommitted => false;
    public bool PopStackReadsAreMemoryWrites => false;
    public bool MoveESPToEBPInstructionEntered => false;
}

/// <summary>
/// Reprojects the four logical POP register restores after the final FS store.
/// </summary>
public static class NetworkDispatcherRegisterRestoreProjection
{
    public const uint EntryRawFileOffset = 0x00BBDEC0;
    public const uint PopECXRawFileOffset = 0x00BBDEC0;
    public const uint PopEDIRawFileOffset = 0x00BBDEC1;
    public const uint PopESIRawFileOffset = 0x00BBDEC2;
    public const uint PopEBXRawFileOffset = 0x00BBDEC3;
    public const uint MoveESPToEBPRawFileOffset = 0x00BBDEC4;
    public const byte DwordWidth = 4;
    public const uint ESPIncrement = 4;
    public const string CapturedWindowBytes = "595F5E5B8BE5";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherRegisterRestorePlan Project(
        NetworkDispatcherRegisterRestoreCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);

        NetworkDispatcherFinalReturnSehStorePlan parent =
            NetworkDispatcherFinalReturnSehStoreProjection.Project(capture.ParentCapture);
        var effects =
            ImmutableArray.CreateBuilder<NetworkDispatcherRegisterRestoreOrderedEffect>();
        foreach (NetworkDispatcherFinalReturnSehStoreOrderedEffect parentEffect
            in parent.OrderedEffects)
        {
            effects.Add(new(
                effects.Count,
                parentEffect,
                parentEffect.RawFileOffset,
                NetworkDispatcherRegisterRestoreOrderedEffectKind
                    .ParentFinalReturnSehStoreEffect,
                null,
                null,
                null));
        }

        bool admitted =
            parent.ExactCleanCompletion
            && parent.BoundedCompletionBoundaryReached
            && parent.Outcome
                == NetworkDispatcherFinalReturnSehStoreOutcome.BoundedBeforePop
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.FlagsAtStop is {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && !parent.NativeInstructionsExecuted
            && !parent.PopInstructionEntered;

        var reads = ImmutableArray.CreateBuilder<NetworkDispatcherRegisterRestoreRead>();
        var pops = ImmutableArray.CreateBuilder<NetworkDispatcherRegisterRestorePop>();
        var reached = ImmutableArray.CreateBuilder<uint>();
        var states = new NetworkDispatcherRegisterRestoreCaptureState[5];
        states[0] = InitialState(capture.FreshPostCallESP, admitted);
        states[1] = InitialState(capture.FreshPopECXDWORD, admitted);
        states[2] = InitialState(capture.FreshPopEDIDWORD, admitted);
        states[3] = InitialState(capture.FreshPopESIDWORD, admitted);
        states[4] = InitialState(capture.FreshPopEBXDWORD, admitted);

        uint entry = admitted ? EntryRawFileOffset : parent.StopAtRawFileOffsetExclusive;
        uint stopAt = parent.StopAtRawFileOffsetExclusive;
        uint? initialEsp = null;
        uint? currentEsp = null;
        var registers = new NetworkDispatcherRegisterRestoreRegisters(null, null, null, null);
        NetworkDispatcherRegisterRestoreOutcome outcome;
        bool boundaryReached = false;

        if (!admitted)
        {
            outcome = NetworkDispatcherRegisterRestoreOutcome.ParentHandoffNotAdmitted;
        }
        else
        {
            states[0] = State(capture.FreshPostCallESP);
            var espObservation = new NetworkDispatcherRegisterRestoreESPObservation(
                EntryRawFileOffset,
                capture.FreshPostCallESP,
                capture.FreshPostCallESP.HasValue);
            effects.Add(new(
                effects.Count,
                null,
                EntryRawFileOffset,
                NetworkDispatcherRegisterRestoreOrderedEffectKind
                    .FreshPostCallESPObservation,
                espObservation,
                null,
                null));

            if (capture.FreshPostCallESP is not uint capturedEsp)
            {
                SetUnreachedOrUnused(states, 1, capture.FreshPopECXDWORD);
                SetUnreachedOrUnused(states, 2, capture.FreshPopEDIDWORD);
                SetUnreachedOrUnused(states, 3, capture.FreshPopESIDWORD);
                SetUnreachedOrUnused(states, 4, capture.FreshPopEBXDWORD);
                outcome = NetworkDispatcherRegisterRestoreOutcome.MissingFreshPostCallESP;
                stopAt = EntryRawFileOffset;
            }
            else
            {
                initialEsp = capturedEsp;
                currentEsp = capturedEsp;
                uint?[] stackValues =
                [
                    capture.FreshPopECXDWORD,
                    capture.FreshPopEDIDWORD,
                    capture.FreshPopESIDWORD,
                    capture.FreshPopEBXDWORD
                ];
                NetworkDispatcherRegisterRestoreRegister[] targetRegisters =
                [
                    NetworkDispatcherRegisterRestoreRegister.ECX,
                    NetworkDispatcherRegisterRestoreRegister.EDI,
                    NetworkDispatcherRegisterRestoreRegister.ESI,
                    NetworkDispatcherRegisterRestoreRegister.EBX
                ];
                uint[] rawOffsets =
                [
                    PopECXRawFileOffset,
                    PopEDIRawFileOffset,
                    PopESIRawFileOffset,
                    PopEBXRawFileOffset
                ];
                NetworkDispatcherRegisterRestoreOutcome[] missingOutcomes =
                [
                    NetworkDispatcherRegisterRestoreOutcome.MissingFreshPopECXDWORD,
                    NetworkDispatcherRegisterRestoreOutcome.MissingFreshPopEDIDWORD,
                    NetworkDispatcherRegisterRestoreOutcome.MissingFreshPopESIDWORD,
                    NetworkDispatcherRegisterRestoreOutcome.MissingFreshPopEBXDWORD
                ];
                outcome = NetworkDispatcherRegisterRestoreOutcome
                    .BoundedBeforeMoveESPToEBP;

                for (int index = 0; index < stackValues.Length; index++)
                {
                    uint rawOffset = rawOffsets[index];
                    uint stackAddress = currentEsp.Value;
                    uint? stackValue = stackValues[index];
                    states[index + 1] = State(stackValue);
                    reached.Add(rawOffset);

                    var read = new NetworkDispatcherRegisterRestoreRead(
                        rawOffset,
                        stackAddress,
                        DwordWidth,
                        Attempted: true,
                        Captured: stackValue.HasValue,
                        Value: stackValue,
                        Committed: false);
                    reads.Add(read);
                    effects.Add(new(
                        effects.Count,
                        null,
                        rawOffset,
                        NetworkDispatcherRegisterRestoreOrderedEffectKind.PopStackDWORDRead,
                        null,
                        read,
                        null));

                    if (stackValue is not uint value)
                    {
                        uint? registerValue = GetRegister(registers, targetRegisters[index]);
                        var missingPop = new NetworkDispatcherRegisterRestorePop(
                            rawOffset,
                            targetRegisters[index],
                            stackAddress,
                            null,
                            currentEsp.Value,
                            currentEsp.Value,
                            registerValue,
                            Attempted: true,
                            StackReadCaptured: false,
                            RegisterValueDerivedFromRead: false,
                            ESPAdvancedInProjection: false);
                        pops.Add(missingPop);
                        effects.Add(new(
                            effects.Count,
                            null,
                            rawOffset,
                            NetworkDispatcherRegisterRestoreOrderedEffectKind
                                .LogicalPopRegisterAndESPProjection,
                            null,
                            null,
                            missingPop));
                        SetUnreachedOrUnused(states, index + 2, stackValues[(index + 1)..]);
                        outcome = missingOutcomes[index];
                        stopAt = rawOffset;
                        break;
                    }

                    registers = SetRegister(registers, targetRegisters[index], value);
                    uint nextEsp = unchecked(currentEsp.Value + ESPIncrement);
                    var pop = new NetworkDispatcherRegisterRestorePop(
                        rawOffset,
                        targetRegisters[index],
                        stackAddress,
                        value,
                        currentEsp.Value,
                        nextEsp,
                        value,
                        Attempted: true,
                        StackReadCaptured: true,
                        RegisterValueDerivedFromRead: true,
                        ESPAdvancedInProjection: true);
                    pops.Add(pop);
                    effects.Add(new(
                        effects.Count,
                        null,
                        rawOffset,
                        NetworkDispatcherRegisterRestoreOrderedEffectKind
                            .LogicalPopRegisterAndESPProjection,
                        null,
                        null,
                        pop));
                    currentEsp = nextEsp;
                }

                if (pops.Count == 4 && pops.All(pop =>
                    pop.StackReadCaptured
                        && pop.RegisterValueDerivedFromRead
                        && pop.ESPAdvancedInProjection))
                {
                    boundaryReached = true;
                    stopAt = MoveESPToEBPRawFileOffset;
                }
            }
        }

        ImmutableArray<NetworkDispatcherRegisterRestoreOrderedEffect> ordered =
            effects.Select((effect, index) => effect with { Sequence = index })
                .ToImmutableArray();
        var captureStates = new NetworkDispatcherRegisterRestoreCaptureStates(
            states[0],
            states[1],
            states[2],
            states[3],
            states[4]);
        bool exactCleanCompletion =
            boundaryReached
            && parent.ExactCleanCompletion
            && captureStates.FreshPostCallESP
                == NetworkDispatcherRegisterRestoreCaptureState.Captured
            && captureStates.FreshPopECXDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.Captured
            && captureStates.FreshPopEDIDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.Captured
            && captureStates.FreshPopESIDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.Captured
            && captureStates.FreshPopEBXDWORD
                == NetworkDispatcherRegisterRestoreCaptureState.Captured;
        bool exactBoundedCompletion =
            boundaryReached
            || (!admitted
                && parent.ExactBoundedCompletion
                && stopAt == parent.StopAtRawFileOffsetExclusive);

        return new(
            parent,
            capture,
            outcome,
            reads.ToImmutable(),
            pops.ToImmutable(),
            ordered,
            reached.ToImmutable(),
            captureStates,
            admitted,
            entry,
            initialEsp,
            currentEsp,
            registers,
            parent.FlagsAtStop,
            boundaryReached,
            exactBoundedCompletion,
            exactCleanCompletion,
            stopAt);
    }

    private static NetworkDispatcherRegisterRestoreCaptureState State(uint? value) =>
        value.HasValue
            ? NetworkDispatcherRegisterRestoreCaptureState.Captured
            : NetworkDispatcherRegisterRestoreCaptureState.Missing;

    private static NetworkDispatcherRegisterRestoreCaptureState InitialState(
        uint? value,
        bool admitted) =>
        admitted
            ? State(value)
            : value.HasValue
                ? NetworkDispatcherRegisterRestoreCaptureState.SuppliedUnused
                : NetworkDispatcherRegisterRestoreCaptureState.Unreached;

    private static void SetUnreachedOrUnused(
        NetworkDispatcherRegisterRestoreCaptureState[] states,
        int index,
        uint? value) =>
        states[index] = value.HasValue
            ? NetworkDispatcherRegisterRestoreCaptureState.SuppliedUnused
            : NetworkDispatcherRegisterRestoreCaptureState.Unreached;

    private static void SetUnreachedOrUnused(
        NetworkDispatcherRegisterRestoreCaptureState[] states,
        int startIndex,
        uint?[] remainingValues)
    {
        for (int index = 0; index < remainingValues.Length; index++)
        {
            SetUnreachedOrUnused(states, startIndex + index, remainingValues[index]);
        }
    }

    private static uint? GetRegister(
        NetworkDispatcherRegisterRestoreRegisters registers,
        NetworkDispatcherRegisterRestoreRegister target) =>
        target switch
        {
            NetworkDispatcherRegisterRestoreRegister.ECX => registers.ECX,
            NetworkDispatcherRegisterRestoreRegister.EDI => registers.EDI,
            NetworkDispatcherRegisterRestoreRegister.ESI => registers.ESI,
            NetworkDispatcherRegisterRestoreRegister.EBX => registers.EBX,
            _ => throw new ArgumentOutOfRangeException(nameof(target))
        };

    private static NetworkDispatcherRegisterRestoreRegisters SetRegister(
        NetworkDispatcherRegisterRestoreRegisters registers,
        NetworkDispatcherRegisterRestoreRegister target,
        uint value) =>
        target switch
        {
            NetworkDispatcherRegisterRestoreRegister.ECX => registers with { ECX = value },
            NetworkDispatcherRegisterRestoreRegister.EDI => registers with { EDI = value },
            NetworkDispatcherRegisterRestoreRegister.ESI => registers with { ESI = value },
            NetworkDispatcherRegisterRestoreRegister.EBX => registers with { EBX = value },
            _ => throw new ArgumentOutOfRangeException(nameof(target))
        };
}
