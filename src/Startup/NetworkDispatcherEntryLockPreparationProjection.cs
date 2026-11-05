using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryLockPreparationCapture(
    NetworkDispatcherEntryFsPublicationCapture ParentFsPublicationCapture,
    uint? FreshEntryECX);

public enum NetworkDispatcherEntryLockPreparationOutcome
{
    ParentFsPublicationNotAccepted,
    MissingFreshEntryECX,
    LockPreparationProjected
}

public enum NetworkDispatcherEntryLockPreparationCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryLockPreparationInstructionKind
{
    MoveEntryECXToESI,
    CalculateEntryESIPlusDisplacementToEBX,
    WriteByteLocalOne,
    PushEBX,
    WriteDwordLocalEBX
}

public readonly record struct NetworkDispatcherEntryLockPreparationInstruction(
    NetworkDispatcherEntryLockPreparationInstructionKind Kind,
    uint RawFileOffset);

public readonly record struct NetworkDispatcherEntryLockPreparationMove(
    uint RawFileOffset,
    uint? SourceECX,
    uint? ResultESI,
    bool Attempted,
    bool Completed);

public readonly record struct NetworkDispatcherEntryLockPreparationAddressCalculation(
    uint RawFileOffset,
    uint? BaseESI,
    uint Displacement,
    uint? ResultEBX,
    bool Attempted,
    bool Completed);

public readonly record struct NetworkDispatcherEntryLockPreparationWrite(
    uint RawFileOffset,
    uint? BaseEBP,
    uint Displacement,
    uint? Address,
    byte Width,
    uint? Value,
    bool Attempted,
    bool Committed);

public readonly record struct NetworkDispatcherEntryLockPreparationPush(
    uint RawFileOffset,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    uint? Value,
    bool Attempted,
    bool Committed);

public sealed record NetworkDispatcherEntryLockPreparationPlan(
    NetworkDispatcherEntryLockPreparationCapture Capture,
    NetworkDispatcherEntryFsPublicationPlan ParentFsPublicationPlan,
    NetworkDispatcherEntryLockPreparationOutcome Outcome,
    ImmutableArray<uint> CompletedInstructionOffsets,
    ImmutableArray<NetworkDispatcherEntryLockPreparationInstruction> InstructionOrder,
    NetworkDispatcherEntryLockPreparationCaptureState FreshEntryECXCaptureState,
    NetworkDispatcherEntryLockPreparationMove? MoveECXToESI,
    NetworkDispatcherEntryLockPreparationAddressCalculation? AddressCalculation,
    NetworkDispatcherEntryLockPreparationWrite? ByteWrite,
    NetworkDispatcherEntryLockPreparationPush? EBXPush,
    NetworkDispatcherEntryLockPreparationWrite? DwordWrite,
    uint? EntryECX,
    uint? InitialESI,
    uint? CurrentESIAtStop,
    uint? InitialEBX,
    uint? CurrentEBXAtStop,
    uint? EntryEBP,
    uint? ESPBefore,
    uint? CurrentESPAtStop,
    NetworkIoCompletionClockGateFlags? FlagsBefore,
    NetworkIoCompletionClockGateFlags? FlagsAtStop,
    uint StopAtRawFileOffsetExclusive)
{
    public bool ExactLockPreparationProjected =>
        Outcome == NetworkDispatcherEntryLockPreparationOutcome.LockPreparationProjected
        && StopAtRawFileOffsetExclusive
            == NetworkDispatcherEntryLockPreparationProjection.PrefixEndRawFileOffsetExclusive;

    public bool StackPointerChangedByEBXPush =>
        ExactLockPreparationProjected
        && EBXPush?.StackPointerAfter == CurrentESPAtStop
        && EBXPush?.StackPointerBefore == ESPBefore
        && CurrentESPAtStop != ESPBefore;

    public bool FlagsUnchanged =>
        Outcome != NetworkDispatcherEntryLockPreparationOutcome.ParentFsPublicationNotAccepted
        && FlagsBefore == FlagsAtStop;

    public bool StackPointerUnchangedBeforeMissingECX =>
        Outcome == NetworkDispatcherEntryLockPreparationOutcome.MissingFreshEntryECX
        && ESPBefore == CurrentESPAtStop;

    public bool NativeInstructionsExecuted => false;

    public bool NativeMemoryWritesExecuted => false;

    public bool MemoryWriteAliasingReconciled => false;

    public bool OpaqueImportCallReached => false;

    public bool CallerECXForwardedToOpaqueImport => false;

    public bool OpaqueImportSemanticsInferred => false;
}

/// <summary>
/// Reprojects the entry lock-preparation instructions through the final local
/// store, stopping before the opaque import call and without executing native code.
/// </summary>
public static class NetworkDispatcherEntryLockPreparationProjection
{
    public const uint EntryRawFileOffset = 0x00BBDDD8;
    public const uint MoveEntryECXToESIRawFileOffset = 0x00BBDDD8;
    public const uint CalculateEntryESIPlusDisplacementToEBXRawFileOffset = 0x00BBDDDA;
    public const uint WriteByteLocalOneRawFileOffset = 0x00BBDDDD;
    public const uint PushEBXRawFileOffset = 0x00BBDDE1;
    public const uint WriteDwordLocalEBXRawFileOffset = 0x00BBDDE2;
    public const uint OpaqueImportCallRawFileOffset = 0x00BBDDE5;
    public const uint PrefixEndRawFileOffsetExclusive = OpaqueImportCallRawFileOffset;

    public const uint EBXDisplacement = 0x6C;
    public const uint ByteLocalDisplacement = 0x14;
    public const uint DwordLocalDisplacement = 0x18;
    public const uint PushWidth = sizeof(uint);
    public const byte ByteWriteWidth = sizeof(byte);
    public const byte DwordWriteWidth = sizeof(uint);
    public const byte ByteLocalValue = 1;

    public const string CapturedWindowBytes =
        "8B F1 8D 5E 6C C6 45 EC 01 53 89 5D E8 FF 15 84 42 AC 01";

    public static bool ExecutesNativeInstructions => false;

    public static uint AddEBXDisplacement(uint esi) =>
        unchecked(esi + EBXDisplacement);

    public static uint ByteLocalAddress(uint ebp) =>
        unchecked(ebp - ByteLocalDisplacement);

    public static uint DwordLocalAddress(uint ebp) =>
        unchecked(ebp - DwordLocalDisplacement);

    public static uint StackAddressAfterPush(uint stackPointer) =>
        unchecked(stackPointer - PushWidth);

    public static NetworkDispatcherEntryLockPreparationPlan Project(
        NetworkDispatcherEntryLockPreparationCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentFsPublicationCapture);

        NetworkDispatcherEntryFsPublicationPlan parent =
            NetworkDispatcherEntryFsPublicationProjection.Project(
                capture.ParentFsPublicationCapture);
        NetworkDispatcherEntryCookiePreparationPlan cookieParent =
            parent.ParentCookiePreparationPlan;
        NetworkDispatcherEntryRegisterSavePlan registerParent =
            cookieParent.ParentRegisterSavePlan;
        uint? initialESI =
            registerParent.FreshEntryESICaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.Captured
                ? registerParent.FreshEntryESI
                : null;
        uint? initialEBX =
            registerParent.FreshEntryEBXCaptureState
                == NetworkDispatcherEntryRegisterSaveCaptureState.Captured
                ? registerParent.FreshEntryEBX
                : null;

        if (!parent.ExactFsPublicationProjected
            || parent.EntryEBP is not uint entryEBP
            || parent.CurrentESPAtStop is not uint currentESP
            || initialESI is not uint entryESI
            || initialEBX is not uint entryEBX)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryLockPreparationOutcome.ParentFsPublicationNotAccepted,
                ImmutableArray<uint>.Empty,
                ImmutableArray<NetworkDispatcherEntryLockPreparationInstruction>.Empty,
                capture.FreshEntryECX.HasValue
                    ? NetworkDispatcherEntryLockPreparationCaptureState.SuppliedUnused
                    : NetworkDispatcherEntryLockPreparationCaptureState.Unreached,
                null,
                null,
                null,
                null,
                null,
                null,
                initialESI,
                initialESI,
                initialEBX,
                initialEBX,
                parent.EntryEBP,
                parent.CurrentESPAtStop,
                parent.CurrentESPAtStop,
                parent.FlagsAtStop,
                parent.FlagsAtStop,
                parent.StopAtRawFileOffsetExclusive);
        }

        var move = new NetworkDispatcherEntryLockPreparationMove(
            MoveEntryECXToESIRawFileOffset,
            capture.FreshEntryECX,
            capture.FreshEntryECX,
            Attempted: true,
            Completed: capture.FreshEntryECX.HasValue);

        if (capture.FreshEntryECX is not uint entryECX)
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryLockPreparationOutcome.MissingFreshEntryECX,
                ImmutableArray<uint>.Empty,
                ImmutableArray<NetworkDispatcherEntryLockPreparationInstruction>.Empty,
                NetworkDispatcherEntryLockPreparationCaptureState.Missing,
                move,
                null,
                null,
                null,
                null,
                null,
                entryESI,
                entryESI,
                entryEBX,
                entryEBX,
                entryEBP,
                currentESP,
                currentESP,
                parent.FlagsAtStop,
                parent.FlagsAtStop,
                MoveEntryECXToESIRawFileOffset);
        }

        uint resultEBX = AddEBXDisplacement(entryECX);
        var addressCalculation = new NetworkDispatcherEntryLockPreparationAddressCalculation(
            CalculateEntryESIPlusDisplacementToEBXRawFileOffset,
            entryECX,
            EBXDisplacement,
            resultEBX,
            Attempted: true,
            Completed: true);
        var byteWrite = new NetworkDispatcherEntryLockPreparationWrite(
            WriteByteLocalOneRawFileOffset,
            entryEBP,
            ByteLocalDisplacement,
            ByteLocalAddress(entryEBP),
            ByteWriteWidth,
            ByteLocalValue,
            Attempted: true,
            Committed: true);
        uint stackPointerAfterPush = StackAddressAfterPush(currentESP);
        var push = new NetworkDispatcherEntryLockPreparationPush(
            PushEBXRawFileOffset,
            currentESP,
            stackPointerAfterPush,
            resultEBX,
            Attempted: true,
            Committed: true);
        var dwordWrite = new NetworkDispatcherEntryLockPreparationWrite(
            WriteDwordLocalEBXRawFileOffset,
            entryEBP,
            DwordLocalDisplacement,
            DwordLocalAddress(entryEBP),
            DwordWriteWidth,
            resultEBX,
            Attempted: true,
            Committed: true);

        ImmutableArray<NetworkDispatcherEntryLockPreparationInstruction> instructionOrder =
            ImmutableArray.Create(
                new NetworkDispatcherEntryLockPreparationInstruction(
                    NetworkDispatcherEntryLockPreparationInstructionKind.MoveEntryECXToESI,
                    MoveEntryECXToESIRawFileOffset),
                new NetworkDispatcherEntryLockPreparationInstruction(
                    NetworkDispatcherEntryLockPreparationInstructionKind.CalculateEntryESIPlusDisplacementToEBX,
                    CalculateEntryESIPlusDisplacementToEBXRawFileOffset),
                new NetworkDispatcherEntryLockPreparationInstruction(
                    NetworkDispatcherEntryLockPreparationInstructionKind.WriteByteLocalOne,
                    WriteByteLocalOneRawFileOffset),
                new NetworkDispatcherEntryLockPreparationInstruction(
                    NetworkDispatcherEntryLockPreparationInstructionKind.PushEBX,
                    PushEBXRawFileOffset),
                new NetworkDispatcherEntryLockPreparationInstruction(
                    NetworkDispatcherEntryLockPreparationInstructionKind.WriteDwordLocalEBX,
                    WriteDwordLocalEBXRawFileOffset));

        return new(
            capture,
            parent,
            NetworkDispatcherEntryLockPreparationOutcome.LockPreparationProjected,
            instructionOrder.Select(instruction => instruction.RawFileOffset).ToImmutableArray(),
            instructionOrder,
            NetworkDispatcherEntryLockPreparationCaptureState.Captured,
            move,
            addressCalculation,
            byteWrite,
            push,
            dwordWrite,
            entryECX,
            entryESI,
            entryECX,
            entryEBX,
            resultEBX,
            entryEBP,
            currentESP,
            stackPointerAfterPush,
            parent.FlagsAtStop,
            parent.FlagsAtStop,
            PrefixEndRawFileOffsetExclusive);
    }
}
