using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct SocketPayloadRecyclePrefixCapture(
    uint? AssociatedPointerValue,
    uint? Counter4BeforeXadd,
    uint? Slot0VtableValue,
    bool? Slot0ReturnedNormally,
    uint? Counter8BeforeXadd,
    uint? Slot4VtableValue,
    bool? Slot4ReturnedNormally);

public enum SocketPayloadRecyclePrefixCounterOutcome
{
    PathUndetermined,
    NotReached,
    MissingValueCapture,
    Captured
}

public enum SocketPayloadRecyclePrefixVtableOutcome
{
    PathUndetermined,
    NotReached,
    MissingValueCapture,
    Captured
}

public enum SocketPayloadRecyclePrefixCallOutcome
{
    PathUndetermined,
    NotReached,
    MissingReturnCapture,
    DidNotReturnNormally,
    ReturnedNormally
}

public enum SocketPayloadRecyclePrefixActionKind
{
    LoadAssociatedPointer,
    TestAssociatedPointer,
    BranchNullAssociatedPointer,
    FormCounter4Address,
    AtomicDecrementCounter4,
    BranchOnCounter4UpdatedValue,
    LoadSlot0Vtable,
    SetSlot0ThisArgument,
    CallSlot0,
    FormCounter8Address,
    AtomicDecrementCounter8,
    BranchOnCounter8UpdatedValue,
    LoadSlot4Vtable,
    SetSlot4ThisArgument,
    CallSlot4,
    ReachBoundary
}

public readonly record struct SocketPayloadRecyclePrefixAction(
    SocketPayloadRecyclePrefixActionKind Kind,
    uint RawFileOffset,
    uint? CapturedValue = null,
    uint? SecondaryCapturedValue = null,
    bool? ConditionMet = null,
    bool? CallReturnedNormally = null,
    uint? BranchTargetRawFileOffset = null);

public readonly record struct SocketPayloadRecyclePrefixPlan(
    SocketPayloadRecyclePrefixCapture Capture,
    SocketPayloadRecyclePrefixCounterOutcome Counter4Outcome,
    SocketPayloadRecyclePrefixVtableOutcome Slot0VtableOutcome,
    SocketPayloadRecyclePrefixCallOutcome Slot0CallOutcome,
    SocketPayloadRecyclePrefixCounterOutcome Counter8Outcome,
    SocketPayloadRecyclePrefixVtableOutcome Slot4VtableOutcome,
    SocketPayloadRecyclePrefixCallOutcome Slot4CallOutcome,
    ImmutableArray<SocketPayloadRecyclePrefixAction> Actions,
    uint? Counter4Address,
    uint? Counter4OldValue,
    uint? Counter4UpdatedValue,
    uint? Slot0VtableValue,
    uint? Counter8Address,
    uint? Counter8OldValue,
    uint? Counter8UpdatedValue,
    uint? Slot4VtableValue,
    bool BoundaryReached,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct SocketPayloadRecyclePrefixInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? NormalReturnRawFileOffset = null);

/// <summary>
/// Projects the recycle wrapper from its single associated-pointer load
/// through the boundary at raw 0xBD5C9C. Captures remain independent across
/// both XADDs and both vtable reads; this projection makes no ownership or
/// freeing inference about either indirect call.
/// </summary>
public static class SocketPayloadRecyclePrefixProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint PrefixStartRawFileOffset = 0x00BD5C72;
    public const uint PrefixStartVirtualAddress = 0x00FD6872;
    public const uint BoundaryRawFileOffset = 0x00BD5C9C;
    public const uint BoundaryVirtualAddress = 0x00FD689C;
    public const uint PrefixEndRawFileOffsetExclusive = BoundaryRawFileOffset;
    public const uint PrefixEndVirtualAddressExclusive = BoundaryVirtualAddress;

    public const uint AssociatedPointerLoadRawFileOffset = 0x00BD5C72;
    public const uint AssociatedPointerTestRawFileOffset = 0x00BD5C75;
    public const uint AssociatedPointerBranchRawFileOffset = 0x00BD5C77;
    public const uint AssociatedPointerNullBranchTargetRawFileOffset = 0x00BD5C9C;
    public const uint Counter4AddressRawFileOffset = 0x00BD5C79;
    public const uint Counter4XaddRawFileOffset = 0x00BD5C7E;
    public const uint Counter4BranchRawFileOffset = 0x00BD5C82;
    public const uint Counter4BranchTargetRawFileOffset = 0x00BD5C9C;
    public const uint Slot0VtableLoadRawFileOffset = 0x00BD5C84;
    public const uint Slot0ThisArgumentRawFileOffset = 0x00BD5C86;
    public const uint Slot0CallRawFileOffset = 0x00BD5C88;
    public const uint Slot0CallNormalReturnRawFileOffset = 0x00BD5C8A;
    public const uint Counter8AddressRawFileOffset = 0x00BD5C8A;
    public const uint Counter8XaddRawFileOffset = 0x00BD5C8F;
    public const uint Counter8BranchRawFileOffset = 0x00BD5C93;
    public const uint Counter8BranchTargetRawFileOffset = 0x00BD5C9C;
    public const uint Slot4VtableLoadRawFileOffset = 0x00BD5C95;
    public const uint Slot4ThisArgumentRawFileOffset = 0x00BD5C97;
    public const uint Slot4CallRawFileOffset = 0x00BD5C99;
    public const uint Slot4CallNormalReturnRawFileOffset = 0x00BD5C9C;
    public const uint AssociatedPointerOffset = 0x10;
    public const uint Counter4Offset = 4;
    public const uint Counter8Offset = 8;
    public const uint XaddSource = 0xFFFFFFFF;
    public const uint Slot0VtableOffset = 0;
    public const uint Slot4VtableOffset = 4;
    public const uint ExplicitStackArgumentBytes = 0;

    public const string CapturedPrefixBytes =
        "8B 73 10 85 F6 74 23 8D 46 04 8B CF F0 0F C1 08 "
            + "75 18 8B 06 8B CE FF 10 8D 46 08 8B D7 F0 0F C1 10 "
            + "75 07 8B 06 8B CE FF 50 04";

    public static IReadOnlyList<SocketPayloadRecyclePrefixInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Load the associated pointer from EBX+0x10 once; ESI retains this value across both calls",
                AssociatedPointerLoadRawFileOffset,
                "8B 73 10"),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Test the loaded pointer and branch to the 0x9C boundary when null",
                AssociatedPointerTestRawFileOffset,
                "85 F6"),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "The null-pointer branch reaches the exclusive prefix boundary",
                AssociatedPointerBranchRawFileOffset,
                "74 23",
                AssociatedPointerNullBranchTargetRawFileOffset),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Form ESI+4 for the first independent counter capture",
                Counter4AddressRawFileOffset,
                "8D 46 04"),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Set ECX to -1 and atomically XADD the DWORD at ESI+4",
                Counter4XaddRawFileOffset,
                "F0 0F C1 08"),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Branch to 0x9C when the updated first counter is nonzero",
                Counter4BranchRawFileOffset,
                "75 18",
                Counter4BranchTargetRawFileOffset),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Read the slot-0 vtable value only when the first updated counter is zero",
                Slot0VtableLoadRawFileOffset,
                "8B 06"),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Set ECX to the retained associated pointer for the slot-0 call",
                Slot0ThisArgumentRawFileOffset,
                "8B CE"),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Call the captured slot-0 vtable target with no explicit stack arguments",
                Slot0CallRawFileOffset,
                "FF 10",
                NormalReturnRawFileOffset: Slot0CallNormalReturnRawFileOffset),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Form ESI+8 independently after the slot-0 call returns normally",
                Counter8AddressRawFileOffset,
                "8D 46 08"),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Set EDX to -1 and atomically XADD the DWORD at ESI+8",
                Counter8XaddRawFileOffset,
                "F0 0F C1 10"),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Branch to 0x9C when the updated second counter is nonzero",
                Counter8BranchRawFileOffset,
                "75 07",
                Counter8BranchTargetRawFileOffset),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Independently reload the slot-4 vtable value after slot 0 may have changed it",
                Slot4VtableLoadRawFileOffset,
                "8B 06"),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Set ECX to the retained associated pointer for the slot-4 call",
                Slot4ThisArgumentRawFileOffset,
                "8B CE"),
            new SocketPayloadRecyclePrefixInstructionEvidence(
                "Call the independently loaded slot-4 vtable target with no explicit stack arguments",
                Slot4CallRawFileOffset,
                "FF 50 04",
                NormalReturnRawFileOffset: Slot4CallNormalReturnRawFileOffset)
        });

    public static uint AddPointerOffset(uint pointer, uint offset) =>
        unchecked(pointer + offset);

    public static uint Counter4Address(uint associatedPointer) =>
        AddPointerOffset(associatedPointer, Counter4Offset);

    public static uint Counter8Address(uint associatedPointer) =>
        AddPointerOffset(associatedPointer, Counter8Offset);

    public static SocketPayloadRecyclePrefixPlan Project(
        SocketPayloadRecyclePrefixCapture capture)
    {
        var actions = ImmutableArray.CreateBuilder<SocketPayloadRecyclePrefixAction>(16);
        actions.Add(new SocketPayloadRecyclePrefixAction(
            SocketPayloadRecyclePrefixActionKind.LoadAssociatedPointer,
            AssociatedPointerLoadRawFileOffset,
            CapturedValue: capture.AssociatedPointerValue));

        if (capture.AssociatedPointerValue is not uint associatedPointer)
        {
            RejectNoLaterCaptures(capture,
                "Later captures cannot be classified until the associated-pointer load is known.");
            return Stop(
                capture,
                SocketPayloadRecyclePrefixCounterOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixVtableOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixCallOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixCounterOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixVtableOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixCallOutcome.PathUndetermined,
                actions,
                null, null, null, null, null, null, null, null,
                boundaryReached: false,
                stopAtRawFileOffsetExclusive: AssociatedPointerTestRawFileOffset);
        }

        actions.Add(new SocketPayloadRecyclePrefixAction(
            SocketPayloadRecyclePrefixActionKind.TestAssociatedPointer,
            AssociatedPointerTestRawFileOffset,
            CapturedValue: associatedPointer));
        bool associatedPointerIsNull = associatedPointer == 0;
        actions.Add(new SocketPayloadRecyclePrefixAction(
            SocketPayloadRecyclePrefixActionKind.BranchNullAssociatedPointer,
            AssociatedPointerBranchRawFileOffset,
            ConditionMet: associatedPointerIsNull,
            BranchTargetRawFileOffset: associatedPointerIsNull
                ? AssociatedPointerNullBranchTargetRawFileOffset
                : null));

        if (associatedPointerIsNull)
        {
            RejectNoLaterCaptures(capture,
                "Counter and vtable-call captures are unreachable for a null associated pointer.");
            return Stop(
                capture,
                SocketPayloadRecyclePrefixCounterOutcome.NotReached,
                SocketPayloadRecyclePrefixVtableOutcome.NotReached,
                SocketPayloadRecyclePrefixCallOutcome.NotReached,
                SocketPayloadRecyclePrefixCounterOutcome.NotReached,
                SocketPayloadRecyclePrefixVtableOutcome.NotReached,
                SocketPayloadRecyclePrefixCallOutcome.NotReached,
                actions,
                null, null, null, null, null, null, null, null,
                boundaryReached: true,
                stopAtRawFileOffsetExclusive: BoundaryRawFileOffset);
        }

        uint counter4Address = Counter4Address(associatedPointer);
        actions.Add(new SocketPayloadRecyclePrefixAction(
            SocketPayloadRecyclePrefixActionKind.FormCounter4Address,
            Counter4AddressRawFileOffset,
            CapturedValue: counter4Address));
        uint? counter4Old = capture.Counter4BeforeXadd;
        uint? counter4Updated = counter4Old is uint firstOld
            ? unchecked(firstOld + XaddSource)
            : null;
        actions.Add(new SocketPayloadRecyclePrefixAction(
            SocketPayloadRecyclePrefixActionKind.AtomicDecrementCounter4,
            Counter4XaddRawFileOffset,
            CapturedValue: counter4Old,
            SecondaryCapturedValue: counter4Updated));

        if (counter4Old is null)
        {
            RejectLaterSlot0AndCounter8Captures(capture,
                "Later captures are not classified while the first XADD result is missing.");
            return Stop(
                capture,
                SocketPayloadRecyclePrefixCounterOutcome.MissingValueCapture,
                SocketPayloadRecyclePrefixVtableOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixCallOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixCounterOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixVtableOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixCallOutcome.PathUndetermined,
                actions,
                counter4Address, null, null, null, null, null, null, null,
                boundaryReached: false,
                stopAtRawFileOffsetExclusive: Counter4BranchRawFileOffset);
        }

        bool counter4UpdatedNonzero = counter4Updated != 0;
        actions.Add(new SocketPayloadRecyclePrefixAction(
            SocketPayloadRecyclePrefixActionKind.BranchOnCounter4UpdatedValue,
            Counter4BranchRawFileOffset,
            CapturedValue: counter4Updated,
            ConditionMet: counter4UpdatedNonzero,
            BranchTargetRawFileOffset: counter4UpdatedNonzero
                ? Counter4BranchTargetRawFileOffset
                : null));

        if (counter4UpdatedNonzero)
        {
            RejectLaterSlot0AndCounter8Captures(capture,
                "The nonzero first-counter branch reaches 0x9C and skips all later captures.");
            actions.Add(new SocketPayloadRecyclePrefixAction(
                SocketPayloadRecyclePrefixActionKind.ReachBoundary,
                BoundaryRawFileOffset));
            return Stop(
                capture,
                SocketPayloadRecyclePrefixCounterOutcome.Captured,
                SocketPayloadRecyclePrefixVtableOutcome.NotReached,
                SocketPayloadRecyclePrefixCallOutcome.NotReached,
                SocketPayloadRecyclePrefixCounterOutcome.NotReached,
                SocketPayloadRecyclePrefixVtableOutcome.NotReached,
                SocketPayloadRecyclePrefixCallOutcome.NotReached,
                actions,
                counter4Address, counter4Old, counter4Updated, null,
                null, null, null, null,
                boundaryReached: true,
                stopAtRawFileOffsetExclusive: BoundaryRawFileOffset);
        }

        SocketPayloadRecyclePrefixVtableOutcome slot0VtableOutcome;
        SocketPayloadRecyclePrefixCallOutcome slot0CallOutcome;
        uint? slot0Vtable = null;
        slot0Vtable = capture.Slot0VtableValue;
        slot0VtableOutcome = slot0Vtable is null
            ? SocketPayloadRecyclePrefixVtableOutcome.MissingValueCapture
            : SocketPayloadRecyclePrefixVtableOutcome.Captured;
        actions.Add(new SocketPayloadRecyclePrefixAction(
            SocketPayloadRecyclePrefixActionKind.LoadSlot0Vtable,
            Slot0VtableLoadRawFileOffset,
            CapturedValue: slot0Vtable));
        if (slot0Vtable is null)
        {
            RejectSlot0ReturnAndLaterCaptures(capture,
                "A missing slot-0 vtable value cannot support the following call or later captures.");
            return Stop(
                capture,
                SocketPayloadRecyclePrefixCounterOutcome.Captured,
                slot0VtableOutcome,
                SocketPayloadRecyclePrefixCallOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixCounterOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixVtableOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixCallOutcome.PathUndetermined,
                actions,
                counter4Address, counter4Old, counter4Updated, null,
                null, null, null, null,
                boundaryReached: false,
                stopAtRawFileOffsetExclusive: Slot0ThisArgumentRawFileOffset);
        }

        actions.Add(new SocketPayloadRecyclePrefixAction(
            SocketPayloadRecyclePrefixActionKind.SetSlot0ThisArgument,
            Slot0ThisArgumentRawFileOffset,
            CapturedValue: associatedPointer));
        actions.Add(new SocketPayloadRecyclePrefixAction(
            SocketPayloadRecyclePrefixActionKind.CallSlot0,
            Slot0CallRawFileOffset,
            CapturedValue: slot0Vtable,
            CallReturnedNormally: capture.Slot0ReturnedNormally));
        slot0CallOutcome = CallOutcome(capture.Slot0ReturnedNormally);

        if (capture.Slot0ReturnedNormally is not true)
        {
            RejectCounter8AndSlot4Captures(capture,
                "The second XADD and slot-4 call are unreachable unless slot 0 returns normally.");
            return Stop(
                capture,
                SocketPayloadRecyclePrefixCounterOutcome.Captured,
                slot0VtableOutcome,
                slot0CallOutcome,
                capture.Slot0ReturnedNormally is false
                    ? SocketPayloadRecyclePrefixCounterOutcome.NotReached
                    : SocketPayloadRecyclePrefixCounterOutcome.PathUndetermined,
                capture.Slot0ReturnedNormally is false
                    ? SocketPayloadRecyclePrefixVtableOutcome.NotReached
                    : SocketPayloadRecyclePrefixVtableOutcome.PathUndetermined,
                capture.Slot0ReturnedNormally is false
                    ? SocketPayloadRecyclePrefixCallOutcome.NotReached
                    : SocketPayloadRecyclePrefixCallOutcome.PathUndetermined,
                actions,
                counter4Address, counter4Old, counter4Updated, slot0Vtable,
                null, null, null, null,
                boundaryReached: false,
                stopAtRawFileOffsetExclusive: Slot0CallNormalReturnRawFileOffset);
        }

        uint counter8Address = Counter8Address(associatedPointer);
        actions.Add(new SocketPayloadRecyclePrefixAction(
            SocketPayloadRecyclePrefixActionKind.FormCounter8Address,
            Counter8AddressRawFileOffset,
            CapturedValue: counter8Address));
        uint? counter8Old = capture.Counter8BeforeXadd;
        uint? counter8Updated = counter8Old is uint secondOld
            ? unchecked(secondOld + XaddSource)
            : null;
        actions.Add(new SocketPayloadRecyclePrefixAction(
            SocketPayloadRecyclePrefixActionKind.AtomicDecrementCounter8,
            Counter8XaddRawFileOffset,
            CapturedValue: counter8Old,
            SecondaryCapturedValue: counter8Updated));

        if (counter8Old is null)
        {
            RejectSlot4Captures(capture,
                "Later captures are not classified while the second XADD result is missing.");
            return Stop(
                capture,
                SocketPayloadRecyclePrefixCounterOutcome.Captured,
                slot0VtableOutcome,
                slot0CallOutcome,
                SocketPayloadRecyclePrefixCounterOutcome.MissingValueCapture,
                SocketPayloadRecyclePrefixVtableOutcome.PathUndetermined,
                SocketPayloadRecyclePrefixCallOutcome.PathUndetermined,
                actions,
                counter4Address, counter4Old, counter4Updated, slot0Vtable,
                counter8Address, null, null, null,
                boundaryReached: false,
                stopAtRawFileOffsetExclusive: Counter8BranchRawFileOffset);
        }

        bool counter8UpdatedNonzero = counter8Updated != 0;
        actions.Add(new SocketPayloadRecyclePrefixAction(
            SocketPayloadRecyclePrefixActionKind.BranchOnCounter8UpdatedValue,
            Counter8BranchRawFileOffset,
            CapturedValue: counter8Updated,
            ConditionMet: counter8UpdatedNonzero,
            BranchTargetRawFileOffset: counter8UpdatedNonzero
                ? Counter8BranchTargetRawFileOffset
                : null));

        SocketPayloadRecyclePrefixVtableOutcome slot4VtableOutcome;
        SocketPayloadRecyclePrefixCallOutcome slot4CallOutcome;
        uint? slot4Vtable = null;
        bool boundaryReached;
        uint stopAt;
        if (counter8UpdatedNonzero)
        {
            RejectSlot4Captures(capture,
                "The slot-4 vtable load and call are skipped when the updated second counter is nonzero.");
            slot4VtableOutcome = SocketPayloadRecyclePrefixVtableOutcome.NotReached;
            slot4CallOutcome = SocketPayloadRecyclePrefixCallOutcome.NotReached;
            boundaryReached = true;
            stopAt = BoundaryRawFileOffset;
        }
        else
        {
            slot4Vtable = capture.Slot4VtableValue;
            slot4VtableOutcome = slot4Vtable is null
                ? SocketPayloadRecyclePrefixVtableOutcome.MissingValueCapture
                : SocketPayloadRecyclePrefixVtableOutcome.Captured;
            actions.Add(new SocketPayloadRecyclePrefixAction(
                SocketPayloadRecyclePrefixActionKind.LoadSlot4Vtable,
                Slot4VtableLoadRawFileOffset,
                CapturedValue: slot4Vtable));
            if (slot4Vtable is null)
            {
                Reject(capture.Slot4ReturnedNormally is null,
                    nameof(capture.Slot4ReturnedNormally),
                    "A missing slot-4 vtable value cannot support the following call or return capture.");
                return Stop(
                    capture,
                    SocketPayloadRecyclePrefixCounterOutcome.Captured,
                    slot0VtableOutcome,
                    slot0CallOutcome,
                    SocketPayloadRecyclePrefixCounterOutcome.Captured,
                    slot4VtableOutcome,
                    SocketPayloadRecyclePrefixCallOutcome.PathUndetermined,
                    actions,
                    counter4Address, counter4Old, counter4Updated, slot0Vtable,
                    counter8Address, counter8Old, counter8Updated, null,
                    boundaryReached: false,
                    stopAtRawFileOffsetExclusive: Slot4ThisArgumentRawFileOffset);
            }

            actions.Add(new SocketPayloadRecyclePrefixAction(
                SocketPayloadRecyclePrefixActionKind.SetSlot4ThisArgument,
                Slot4ThisArgumentRawFileOffset,
                CapturedValue: associatedPointer));
            actions.Add(new SocketPayloadRecyclePrefixAction(
                SocketPayloadRecyclePrefixActionKind.CallSlot4,
                Slot4CallRawFileOffset,
                CapturedValue: slot4Vtable,
                CallReturnedNormally: capture.Slot4ReturnedNormally));
            slot4CallOutcome = CallOutcome(capture.Slot4ReturnedNormally);
            boundaryReached = capture.Slot4ReturnedNormally is true;
            stopAt = Slot4CallNormalReturnRawFileOffset;
        }

        if (boundaryReached)
        {
            actions.Add(new SocketPayloadRecyclePrefixAction(
                SocketPayloadRecyclePrefixActionKind.ReachBoundary,
                BoundaryRawFileOffset));
        }

        return Stop(
            capture,
            SocketPayloadRecyclePrefixCounterOutcome.Captured,
            slot0VtableOutcome,
            slot0CallOutcome,
            SocketPayloadRecyclePrefixCounterOutcome.Captured,
            slot4VtableOutcome,
            slot4CallOutcome,
            actions,
            counter4Address, counter4Old, counter4Updated, slot0Vtable,
            counter8Address, counter8Old, counter8Updated, slot4Vtable,
            boundaryReached,
            boundaryReached ? BoundaryRawFileOffset : stopAt);
    }

    private static SocketPayloadRecyclePrefixCallOutcome CallOutcome(bool? returnedNormally) =>
        returnedNormally switch
        {
            null => SocketPayloadRecyclePrefixCallOutcome.MissingReturnCapture,
            false => SocketPayloadRecyclePrefixCallOutcome.DidNotReturnNormally,
            true => SocketPayloadRecyclePrefixCallOutcome.ReturnedNormally
        };

    private static void RejectNoLaterCaptures(
        SocketPayloadRecyclePrefixCapture capture,
        string message)
    {
        Reject(capture.Counter4BeforeXadd is null,
            nameof(capture.Counter4BeforeXadd), message);
        Reject(capture.Slot0VtableValue is null,
            nameof(capture.Slot0VtableValue), message);
        Reject(capture.Slot0ReturnedNormally is null,
            nameof(capture.Slot0ReturnedNormally), message);
        Reject(capture.Counter8BeforeXadd is null,
            nameof(capture.Counter8BeforeXadd), message);
        Reject(capture.Slot4VtableValue is null,
            nameof(capture.Slot4VtableValue), message);
        Reject(capture.Slot4ReturnedNormally is null,
            nameof(capture.Slot4ReturnedNormally), message);
    }

    private static void RejectLaterSlot0AndCounter8Captures(
        SocketPayloadRecyclePrefixCapture capture,
        string message)
    {
        Reject(capture.Slot0VtableValue is null,
            nameof(capture.Slot0VtableValue), message);
        Reject(capture.Slot0ReturnedNormally is null,
            nameof(capture.Slot0ReturnedNormally), message);
        Reject(capture.Counter8BeforeXadd is null,
            nameof(capture.Counter8BeforeXadd), message);
        Reject(capture.Slot4VtableValue is null,
            nameof(capture.Slot4VtableValue), message);
        Reject(capture.Slot4ReturnedNormally is null,
            nameof(capture.Slot4ReturnedNormally), message);
    }

    private static void RejectSlot0ReturnAndLaterCaptures(
        SocketPayloadRecyclePrefixCapture capture,
        string message)
    {
        Reject(capture.Slot0ReturnedNormally is null,
            nameof(capture.Slot0ReturnedNormally), message);
        Reject(capture.Counter8BeforeXadd is null,
            nameof(capture.Counter8BeforeXadd), message);
        Reject(capture.Slot4VtableValue is null,
            nameof(capture.Slot4VtableValue), message);
        Reject(capture.Slot4ReturnedNormally is null,
            nameof(capture.Slot4ReturnedNormally), message);
    }

    private static void RejectCounter8AndSlot4Captures(
        SocketPayloadRecyclePrefixCapture capture,
        string message)
    {
        Reject(capture.Counter8BeforeXadd is null,
            nameof(capture.Counter8BeforeXadd), message);
        Reject(capture.Slot4VtableValue is null,
            nameof(capture.Slot4VtableValue), message);
        Reject(capture.Slot4ReturnedNormally is null,
            nameof(capture.Slot4ReturnedNormally), message);
    }

    private static void RejectSlot4Captures(
        SocketPayloadRecyclePrefixCapture capture,
        string message)
    {
        Reject(capture.Slot4VtableValue is null,
            nameof(capture.Slot4VtableValue), message);
        Reject(capture.Slot4ReturnedNormally is null,
            nameof(capture.Slot4ReturnedNormally), message);
    }

    private static void Reject(bool condition, string parameterName, string message)
    {
        if (!condition)
        {
            throw new ArgumentException(message, parameterName);
        }
    }

    private static SocketPayloadRecyclePrefixPlan Stop(
        SocketPayloadRecyclePrefixCapture capture,
        SocketPayloadRecyclePrefixCounterOutcome counter4Outcome,
        SocketPayloadRecyclePrefixVtableOutcome slot0VtableOutcome,
        SocketPayloadRecyclePrefixCallOutcome slot0CallOutcome,
        SocketPayloadRecyclePrefixCounterOutcome counter8Outcome,
        SocketPayloadRecyclePrefixVtableOutcome slot4VtableOutcome,
        SocketPayloadRecyclePrefixCallOutcome slot4CallOutcome,
        ImmutableArray<SocketPayloadRecyclePrefixAction>.Builder actions,
        uint? counter4Address,
        uint? counter4OldValue,
        uint? counter4UpdatedValue,
        uint? slot0VtableValue,
        uint? counter8Address,
        uint? counter8OldValue,
        uint? counter8UpdatedValue,
        uint? slot4VtableValue,
        bool boundaryReached,
        uint stopAtRawFileOffsetExclusive) =>
        new(
            capture,
            counter4Outcome,
            slot0VtableOutcome,
            slot0CallOutcome,
            counter8Outcome,
            slot4VtableOutcome,
            slot4CallOutcome,
            actions.ToImmutable(),
            counter4Address,
            counter4OldValue,
            counter4UpdatedValue,
            slot0VtableValue,
            counter8Address,
            counter8OldValue,
            counter8UpdatedValue,
            slot4VtableValue,
            boundaryReached,
            stopAtRawFileOffsetExclusive);
}
