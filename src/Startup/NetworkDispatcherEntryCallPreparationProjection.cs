using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryCallPreparationInputs(
    NetworkDispatcherCollectionGateCapture ParentCollectionGate,
    uint? FreshEntryESP,
    uint? FreshVptrDWORD);

public enum NetworkDispatcherEntryCallPreparationOutcome
{
    ParentGateIncomplete,
    ParentGateDidNotSelectCollectionEntry,
    ParentGateExitNotClean,
    MissingFreshEntryESP,
    MissingFreshVptrDWORD,
    ZeroEntryOpaqueImportBoundary,
    NonzeroEntryVtableCallBoundary
}

public enum NetworkDispatcherEntryCallPreparationCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedButUnused
}

public readonly record struct NetworkDispatcherEntryCallPreparationCaptureStates(
    NetworkDispatcherEntryCallPreparationCaptureState FreshEntryESP,
    NetworkDispatcherEntryCallPreparationCaptureState FreshVptrDWORD);

public enum NetworkDispatcherEntryCallPreparationReadKind
{
    FreshVptrDWORD
}

public enum NetworkDispatcherEntryCallPreparationPushKind
{
    EntryEDI,
    ArgumentCount,
    AddressImmediate,
    One
}

public enum NetworkDispatcherEntryCallPreparationInstructionKind
{
    ReadVptr,
    MoveEntryEDIToECX,
    Push,
    OpaqueImportCallBoundary,
    VtableCallBoundary
}

public enum NetworkDispatcherEntryCallPreparationTargetKind
{
    OpaqueImportCell,
    VtableSlot
}

public enum NetworkDispatcherEntryCallPreparationOrderedEffectKind
{
    FreshVptrDWORDRead,
    Push
}

public readonly record struct NetworkDispatcherEntryCallPreparationRead(
    uint RawFileOffset,
    NetworkDispatcherEntryCallPreparationReadKind Kind,
    uint? Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value,
    bool Committed);

public readonly record struct NetworkDispatcherEntryCallPreparationPush(
    uint RawFileOffset,
    NetworkDispatcherEntryCallPreparationPushKind Kind,
    uint? StackPointerBefore,
    uint? StackPointerAfter,
    uint? Value,
    bool Attempted,
    bool Committed);

public readonly record struct NetworkDispatcherEntryCallPreparationInstruction(
    NetworkDispatcherEntryCallPreparationInstructionKind Kind,
    uint RawFileOffset,
    uint? Address = null,
    uint? Value = null);

public readonly record struct NetworkDispatcherEntryCallPreparationCallBoundary(
    uint RawFileOffset,
    NetworkDispatcherEntryCallPreparationTargetKind TargetKind,
    uint? ImportCellVirtualAddress,
    uint? VtableSlotAddress,
    bool CallSiteReached,
    bool TargetReadAttempted,
    bool TargetCaptured,
    bool CallAttempted);

public readonly record struct NetworkDispatcherEntryCallPreparationOrderedEffect(
    int Sequence,
    NetworkDispatcherCollectionGateOrderedEffect? ParentCollectionGateEffect,
    NetworkDispatcherEntryCallPreparationRead? Read,
    NetworkDispatcherEntryCallPreparationPush? Push);

public sealed record NetworkDispatcherEntryCallPreparationPlan(
    NetworkDispatcherCollectionGatePlan ParentCollectionGatePlan,
    NetworkDispatcherEntryCallPreparationInputs Capture,
    NetworkDispatcherEntryCallPreparationOutcome Outcome,
    ImmutableArray<NetworkDispatcherEntryCallPreparationRead> Reads,
    ImmutableArray<NetworkDispatcherEntryCallPreparationPush> Pushes,
    ImmutableArray<NetworkDispatcherEntryCallPreparationInstruction> InstructionOrder,
    ImmutableArray<NetworkDispatcherEntryCallPreparationCallBoundary> CallBoundaries,
    ImmutableArray<NetworkDispatcherEntryCallPreparationOrderedEffect> OrderedEffects,
    NetworkDispatcherEntryCallPreparationCaptureStates CaptureStates,
    uint? EntryEDI,
    uint? EntryESI,
    uint? EntryEBX,
    uint? EntryECX,
    uint? EntryEAX,
    uint? StackPointerAtStop,
    uint? VtableSlotAddress,
    NetworkDispatcherCollectionGateFlags? FlagsAtStop,
    bool ChildGateAdmitted,
    bool BoundedCompletionBoundaryReached,
    bool ExactBoundedCompletion,
    bool ExactCleanCompletion,
    uint StopAtRawFileOffsetExclusive)
{
    public bool OpaqueCallsExecuted => false;

    public bool VtableCallExecuted => false;

    public bool CollectionIterationModeled => false;

    public bool UnlockExecuted => false;

    public bool ReallocationModeled => false;
}

/// <summary>
/// Reprojects the exact collection-entry branches without reading either
/// indirect call target or executing native instructions.
/// </summary>
public static class NetworkDispatcherEntryCallPreparationProjection
{
    public const uint EntryRawFileOffset = 0x00BBDE4D;
    public const uint RawToVirtualAddressDelta = 0x00400C00;

    public const uint ZeroEntryPushEDIRawFileOffset = 0x00BBDE4D;
    public const uint ZeroEntryPushArgumentCountRawFileOffset = 0x00BBDE4E;
    public const uint ZeroEntryPushFirstAddressRawFileOffset = 0x00BBDE53;
    public const uint ZeroEntryPushSecondAddressRawFileOffset = 0x00BBDE58;
    public const uint ZeroEntryOpaqueImportCallRawFileOffset = 0x00BBDE5D;
    public const uint ZeroEntryOpaqueImportCellVirtualAddress = 0x01C15EA0;

    public const uint NonzeroEntryReadVptrRawFileOffset = 0x00BBDE66;
    public const uint NonzeroEntryMoveEDIToECXRawFileOffset = 0x00BBDE68;
    public const uint NonzeroEntryPushOneRawFileOffset = 0x00BBDE6A;
    public const uint NonzeroEntryVtableCallRawFileOffset = 0x00BBDE6C;
    public const uint VtableSlotOffset = 0x10;

    public const uint ArgumentCount = 0x23B;
    public const uint FirstAddressImmediate = 0x01B5717F;
    public const uint SecondAddressImmediate = 0x01B5717E;
    public const uint PushWidth = 4;

    public const string CapturedWindowBytes =
        "57683B020000687F71B501687E71B501FF15A05EC10183C4108B078BCF6A01FF5010";

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryCallPreparationPlan Project(
        NetworkDispatcherEntryCallPreparationInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        NetworkDispatcherCollectionGatePlan parent =
            NetworkDispatcherCollectionGateProjection.Project(inputs.ParentCollectionGate);
        var reads =
            ImmutableArray.CreateBuilder<NetworkDispatcherEntryCallPreparationRead>();
        var pushes =
            ImmutableArray.CreateBuilder<NetworkDispatcherEntryCallPreparationPush>();
        var instructions =
            ImmutableArray.CreateBuilder<NetworkDispatcherEntryCallPreparationInstruction>();
        var calls =
            ImmutableArray.CreateBuilder<NetworkDispatcherEntryCallPreparationCallBoundary>();
        var ownEffects =
            ImmutableArray.CreateBuilder<NetworkDispatcherEntryCallPreparationOrderedEffect>();

        NetworkDispatcherEntryCallPreparationOutcome? parentRejection = null;
        if (!parent.ParentBoundedCompletionBoundaryReached)
        {
            parentRejection = NetworkDispatcherEntryCallPreparationOutcome.ParentGateIncomplete;
        }
        else if (parent.Outcome
            == NetworkDispatcherCollectionGateOutcome.ParentGateExitNotClean)
        {
            parentRejection =
                NetworkDispatcherEntryCallPreparationOutcome.ParentGateExitNotClean;
        }
        else if (parent.Outcome is not (
            NetworkDispatcherCollectionGateOutcome.ZeroCollectionEntry
            or NetworkDispatcherCollectionGateOutcome.NonzeroCollectionEntry))
        {
            parentRejection =
                NetworkDispatcherEntryCallPreparationOutcome.ParentGateDidNotSelectCollectionEntry;
        }
        else if (!parent.ExactCleanExitAdmitted)
        {
            parentRejection =
                NetworkDispatcherEntryCallPreparationOutcome.ParentGateExitNotClean;
        }

        bool zeroRoute =
            parent.Outcome == NetworkDispatcherCollectionGateOutcome.ZeroCollectionEntry;
        bool nonzeroRoute =
            parent.Outcome == NetworkDispatcherCollectionGateOutcome.NonzeroCollectionEntry;
        bool childGateAdmitted = parentRejection is null;
        uint? edi = childGateAdmitted ? parent.CollectionEntryValue : null;
        uint? esi = childGateAdmitted ? parent.CollectionESI : null;
        uint? ebx = childGateAdmitted ? 0u : null;
        NetworkDispatcherCollectionGateFlags? flags = parent.FlagsAtStop;
        uint? ecx = null;
        uint? eax = childGateAdmitted ? parent.Capture.FreshReloadDWORD : null;
        uint? stackPointer = null;
        uint? vtableSlotAddress = null;
        bool boundedBoundary = false;
        uint stopAt = parent.StopAtRawFileOffsetExclusive;
        NetworkDispatcherEntryCallPreparationOutcome outcome;

        NetworkDispatcherEntryCallPreparationCaptureState espState =
            UnreachedOrUnused(inputs.FreshEntryESP.HasValue);
        NetworkDispatcherEntryCallPreparationCaptureState vptrState =
            UnreachedOrUnused(inputs.FreshVptrDWORD.HasValue);

        if (parentRejection is NetworkDispatcherEntryCallPreparationOutcome rejection)
        {
            outcome = rejection;
        }
        else if (zeroRoute)
        {
            espState = CaptureState(inputs.FreshEntryESP);
            if (inputs.FreshEntryESP is not uint esp)
            {
                AddPush(ZeroEntryPushEDIRawFileOffset,
                    NetworkDispatcherEntryCallPreparationPushKind.EntryEDI,
                    stackPointerBefore: null, value: edi, committed: false);
                outcome = NetworkDispatcherEntryCallPreparationOutcome.MissingFreshEntryESP;
                stopAt = ZeroEntryPushEDIRawFileOffset;
            }
            else
            {
                uint after = AddPush(ZeroEntryPushEDIRawFileOffset,
                    NetworkDispatcherEntryCallPreparationPushKind.EntryEDI,
                    esp, edi, committed: true)!.Value;
                after = AddPush(ZeroEntryPushArgumentCountRawFileOffset,
                    NetworkDispatcherEntryCallPreparationPushKind.ArgumentCount,
                    after, ArgumentCount, committed: true)!.Value;
                after = AddPush(ZeroEntryPushFirstAddressRawFileOffset,
                    NetworkDispatcherEntryCallPreparationPushKind.AddressImmediate,
                    after, FirstAddressImmediate, committed: true)!.Value;
                after = AddPush(ZeroEntryPushSecondAddressRawFileOffset,
                    NetworkDispatcherEntryCallPreparationPushKind.AddressImmediate,
                    after, SecondAddressImmediate, committed: true)!.Value;
                stackPointer = after;
                calls.Add(new(
                    ZeroEntryOpaqueImportCallRawFileOffset,
                    NetworkDispatcherEntryCallPreparationTargetKind.OpaqueImportCell,
                    ZeroEntryOpaqueImportCellVirtualAddress,
                    null,
                    CallSiteReached: true,
                    TargetReadAttempted: false,
                    TargetCaptured: false,
                    CallAttempted: false));
                instructions.Add(new(
                    NetworkDispatcherEntryCallPreparationInstructionKind.OpaqueImportCallBoundary,
                    ZeroEntryOpaqueImportCallRawFileOffset,
                    ZeroEntryOpaqueImportCellVirtualAddress));
                outcome =
                    NetworkDispatcherEntryCallPreparationOutcome.ZeroEntryOpaqueImportBoundary;
                boundedBoundary = true;
                stopAt = ZeroEntryOpaqueImportCallRawFileOffset;
            }
        }
        else
        {
            vptrState = CaptureState(inputs.FreshVptrDWORD);
            reads.Add(new(
                NonzeroEntryReadVptrRawFileOffset,
                NetworkDispatcherEntryCallPreparationReadKind.FreshVptrDWORD,
                edi,
                Width: 4,
                Attempted: true,
                Captured: inputs.FreshVptrDWORD.HasValue,
                Value: inputs.FreshVptrDWORD,
                Committed: false));
            ownEffects.Add(new(
                ownEffects.Count,
                null,
                reads[^1],
                null));
            instructions.Add(new(
                NetworkDispatcherEntryCallPreparationInstructionKind.ReadVptr,
                NonzeroEntryReadVptrRawFileOffset,
                edi,
                inputs.FreshVptrDWORD));

            if (inputs.FreshVptrDWORD is not uint vptr)
            {
                outcome = NetworkDispatcherEntryCallPreparationOutcome.MissingFreshVptrDWORD;
                stopAt = NonzeroEntryReadVptrRawFileOffset;
            }
            else
            {
                eax = vptr;
                vtableSlotAddress = unchecked(vptr + VtableSlotOffset);
                ecx = edi;
                instructions.Add(new(
                    NetworkDispatcherEntryCallPreparationInstructionKind.MoveEntryEDIToECX,
                    NonzeroEntryMoveEDIToECXRawFileOffset,
                    Value: edi));
                espState = CaptureState(inputs.FreshEntryESP);
                if (inputs.FreshEntryESP is not uint esp)
                {
                    AddPush(NonzeroEntryPushOneRawFileOffset,
                        NetworkDispatcherEntryCallPreparationPushKind.One,
                        stackPointerBefore: null, value: 1, committed: false);
                    outcome =
                        NetworkDispatcherEntryCallPreparationOutcome.MissingFreshEntryESP;
                    stopAt = NonzeroEntryPushOneRawFileOffset;
                }
                else
                {
                    stackPointer = AddPush(NonzeroEntryPushOneRawFileOffset,
                        NetworkDispatcherEntryCallPreparationPushKind.One,
                        esp, 1, committed: true);
                    calls.Add(new(
                        NonzeroEntryVtableCallRawFileOffset,
                        NetworkDispatcherEntryCallPreparationTargetKind.VtableSlot,
                        null,
                        vtableSlotAddress,
                        CallSiteReached: true,
                        TargetReadAttempted: false,
                        TargetCaptured: false,
                        CallAttempted: false));
                    instructions.Add(new(
                        NetworkDispatcherEntryCallPreparationInstructionKind.VtableCallBoundary,
                        NonzeroEntryVtableCallRawFileOffset,
                        vtableSlotAddress));
                    outcome =
                        NetworkDispatcherEntryCallPreparationOutcome.NonzeroEntryVtableCallBoundary;
                    boundedBoundary = true;
                    stopAt = NonzeroEntryVtableCallRawFileOffset;
                }
            }
        }

        uint? AddPush(
            uint rawFileOffset,
            NetworkDispatcherEntryCallPreparationPushKind kind,
            uint? stackPointerBefore,
            uint? value,
            bool committed)
        {
            uint? after = stackPointerBefore is uint before
                ? unchecked(before - PushWidth)
                : null;
            var push = new NetworkDispatcherEntryCallPreparationPush(
                rawFileOffset,
                kind,
                stackPointerBefore,
                after,
                value,
                Attempted: true,
                Committed: committed);
            pushes.Add(push);
            ownEffects.Add(new(
                ownEffects.Count,
                null,
                null,
                push));
            instructions.Add(new(
                NetworkDispatcherEntryCallPreparationInstructionKind.Push,
                rawFileOffset,
                after,
                value));
            return after;
        }

        ImmutableArray<NetworkDispatcherEntryCallPreparationOrderedEffect> ordered =
            parent.OrderedEffects
                .Select(effect => new NetworkDispatcherEntryCallPreparationOrderedEffect(
                    Sequence: 0,
                    ParentCollectionGateEffect: effect,
                    Read: null,
                    Push: null))
                .Concat(ownEffects)
                .Select((effect, index) => effect with { Sequence = index })
                .ToImmutableArray();

        var captureStates = new NetworkDispatcherEntryCallPreparationCaptureStates(
            espState,
            vptrState);
        bool exactCleanCompletion =
            boundedBoundary
            && parent.ExactCleanExitAdmitted
            && espState == NetworkDispatcherEntryCallPreparationCaptureState.Captured
            && (zeroRoute
                ? vptrState != NetworkDispatcherEntryCallPreparationCaptureState.SuppliedButUnused
                : vptrState == NetworkDispatcherEntryCallPreparationCaptureState.Captured);
        return new(
            parent,
            inputs,
            outcome,
            reads.ToImmutable(),
            pushes.ToImmutable(),
            instructions.ToImmutable(),
            calls.ToImmutable(),
            ordered,
            captureStates,
            edi,
            esi,
            ebx,
            ecx,
            eax,
            stackPointer,
            vtableSlotAddress,
            flags,
            childGateAdmitted,
            boundedBoundary,
            boundedBoundary
                || (!childGateAdmitted && parent.ExactBoundedCompletion),
            exactCleanCompletion,
            stopAt);
    }

    private static NetworkDispatcherEntryCallPreparationCaptureState CaptureState(
        uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryCallPreparationCaptureState.Captured
            : NetworkDispatcherEntryCallPreparationCaptureState.Missing;

    private static NetworkDispatcherEntryCallPreparationCaptureState UnreachedOrUnused(
        bool supplied) =>
        supplied
            ? NetworkDispatcherEntryCallPreparationCaptureState.SuppliedButUnused
            : NetworkDispatcherEntryCallPreparationCaptureState.Unreached;
}
