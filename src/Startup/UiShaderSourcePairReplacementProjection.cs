namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePairReplacementCapture(
    uint DestinationPointer,
    uint SourcePointer,
    uint SourceControlPointer,
    uint SourceBackingPointer,
    uint OldDestinationControlPointer,
    uint? OldDestinationControlCountReturnedByXadd,
    uint? OldDestinationSecondaryCountReturnedByXadd);

public enum UiShaderSourcePairReplacementActionKind
{
    CaptureSourceControlPointer,
    CaptureSourceBackingPointer,
    IncrementIncomingControlReferenceCount,
    CaptureOldDestinationControlPointer,
    DecrementOldDestinationControlReferenceCount,
    BranchOnOldDestinationControlCounterResult,
    CallOldDestinationCallbackSlot0,
    DecrementOldDestinationSecondaryReferenceCount,
    BranchOnOldDestinationSecondaryCounterResult,
    CallOldDestinationCallbackSlot4,
    StoreIncomingControlPointer,
    StoreIncomingBackingPointer
}

public readonly record struct UiShaderSourcePairReplacementAction(
    UiShaderSourcePairReplacementActionKind Kind,
    uint InstructionRawFileOffset,
    uint? TargetAddress = null,
    int? RelativeOffset = null,
    uint? Value = null,
    uint? CounterValueReturnedByXadd = null,
    uint? CounterValueAfterXadd = null,
    bool? BranchTaken = null,
    uint? BranchTargetRawFileOffset = null,
    int? CallbackVtableSlotOffset = null);

public readonly record struct UiShaderSourcePairReplacementProjection(
    UiShaderSourcePairReplacementCapture Capture,
    IReadOnlyList<UiShaderSourcePairReplacementAction> Actions,
    bool? OldDestinationControlCounterReachedZero,
    bool? OldDestinationSecondaryCounterReachedZero);

public readonly record struct UiShaderSourcePairReplacementInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string BytesHex,
    uint? BranchTargetRawFileOffset = null);

/// <summary>
/// Projects the observed helper's captured pair values, reference-count gates, callback
/// order, and final stores. Captures are distinct reads, not a snapshot or alias guarantee;
/// this projection neither rereads source fields nor models callback effects or freeing.
/// The pinned IDA database uses raw file offsets as EAs (flat base-zero mapping).
/// </summary>
public static class UiShaderSourcePairReplacementProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressOffsetFromRawFileOffset = 0x00400C00;

    public const uint HelperEntryRawFileOffset = 0x12CC430;
    public const uint SourceControlLoadRawFileOffset = 0x12CC439;
    public const uint SourceBackingLoadRawFileOffset = 0x12CC43E;
    public const uint SourceBackingSaveRawFileOffset = 0x12CC440;
    public const uint SourceControlNullTestRawFileOffset = 0x12CC443;
    public const uint SourceControlNullBranchRawFileOffset = 0x12CC445;
    public const uint IncomingControlIncrementRawFileOffset = 0x12CC44F;
    public const uint OldDestinationControlLoadRawFileOffset = 0x12CC456;
    public const uint OldDestinationControlNullTestRawFileOffset = 0x12CC459;
    public const uint OldDestinationControlNullBranchRawFileOffset = 0x12CC45B;
    public const uint OldDestinationControlCounterXaddRawFileOffset = 0x12CC463;
    public const uint OldDestinationControlCounterBranchRawFileOffset = 0x12CC467;
    public const uint OldDestinationCallbackSlot0CallRawFileOffset = 0x12CC46D;
    public const uint OldDestinationSecondaryCounterXaddRawFileOffset = 0x12CC475;
    public const uint OldDestinationSecondaryCounterBranchRawFileOffset = 0x12CC479;
    public const uint OldDestinationCallbackSlot4CallRawFileOffset = 0x12CC47F;
    public const uint DestinationControlStoreAfterCallbacksRawFileOffset = 0x12CC485;
    public const uint DestinationBackingStoreAfterCallbacksRawFileOffset = 0x12CC489;
    public const uint DestinationControlStoreWithoutCallbacksRawFileOffset = 0x12CC491;
    public const uint DestinationBackingStoreWithoutCallbacksRawFileOffset = 0x12CC495;

    public const uint OldDestinationControlCounterNonzeroBranchTargetRawFileOffset = 0x12CC491;
    public const uint OldDestinationSecondaryCounterNonzeroBranchTargetRawFileOffset = 0x12CC482;

    public const int SourceBackingPointerOffset = 0;
    public const int SourceControlPointerOffset = 4;
    public const int DestinationBackingPointerOffset = 0;
    public const int DestinationControlPointerOffset = 4;
    public const int ReferenceCountOffset = 4;
    public const int SecondaryReferenceCountOffset = 8;
    public const int CallbackSlot0Offset = 0;
    public const int CallbackSlot4Offset = 4;

    public static IReadOnlyList<UiShaderSourcePairReplacementInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Capture source control pointer from source +4",
                SourceControlLoadRawFileOffset,
                "8B7804"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Capture source backing pointer from source +0",
                SourceBackingLoadRawFileOffset,
                "8B10"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Save the captured source backing pointer before conditional reference-count work",
                SourceBackingSaveRawFileOffset,
                "895508"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Test the captured source control pointer for null",
                SourceControlNullTestRawFileOffset,
                "85FF"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Skip incoming control increment when captured source control is null",
                SourceControlNullBranchRawFileOffset,
                "740F",
                OldDestinationControlLoadRawFileOffset),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Increment the captured incoming control counter at +4",
                IncomingControlIncrementRawFileOffset,
                "F00FC110"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Load old destination control pointer from destination +4",
                OldDestinationControlLoadRawFileOffset,
                "8B5E04"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Test the old destination control pointer for null",
                OldDestinationControlNullTestRawFileOffset,
                "85DB"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Skip both old destination counter xadds and callbacks when old control is null",
                OldDestinationControlNullBranchRawFileOffset,
                "7434",
                DestinationControlStoreWithoutCallbacksRawFileOffset),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Address old destination control counter at +4",
                0x12CC45D,
                "8D4B04"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Set the old destination control decrement operand to -1",
                0x12CC460,
                "83C8FF"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Atomically decrement old destination control counter at +4",
                OldDestinationControlCounterXaddRawFileOffset,
                "F00FC101"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Branch directly to no-callback stores when the control decrement result is nonzero",
                OldDestinationControlCounterBranchRawFileOffset,
                "7528",
                OldDestinationControlCounterNonzeroBranchTargetRawFileOffset),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Load old destination callback vtable for slot 0",
                0x12CC469,
                "8B03"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Call old destination callback vtable slot 0",
                OldDestinationCallbackSlot0CallRawFileOffset,
                "FF10"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Address old destination secondary counter at +8",
                0x12CC46F,
                "8D4308"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Set the secondary decrement operand to -1",
                0x12CC472,
                "83C9FF"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Atomically decrement old destination secondary counter at +8",
                OldDestinationSecondaryCounterXaddRawFileOffset,
                "F00FC108"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Branch to post-callback stores when the secondary decrement result is nonzero",
                OldDestinationSecondaryCounterBranchRawFileOffset,
                "7507",
                OldDestinationSecondaryCounterNonzeroBranchTargetRawFileOffset),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Load old destination callback vtable for slot 4",
                0x12CC47B,
                "8B03"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Call old destination callback vtable slot 4",
                OldDestinationCallbackSlot4CallRawFileOffset,
                "FF5004"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Load the saved incoming backing pointer before callback-path stores",
                0x12CC482,
                "8B4508"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Store captured incoming control pointer at destination +4 after callbacks",
                DestinationControlStoreAfterCallbacksRawFileOffset,
                "897E04"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Store captured incoming backing pointer at destination +0 after callbacks",
                DestinationBackingStoreAfterCallbacksRawFileOffset,
                "8906"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Store captured incoming control pointer at destination +4 on the direct path",
                DestinationControlStoreWithoutCallbacksRawFileOffset,
                "897E04"),
            new UiShaderSourcePairReplacementInstructionEvidence(
                "Store captured incoming backing pointer at destination +0 on the direct path",
                DestinationBackingStoreWithoutCallbacksRawFileOffset,
                "8916")
        });

    public static uint ToCodeVirtualAddress(uint rawFileOffset) =>
        checked(rawFileOffset + CodeVirtualAddressOffsetFromRawFileOffset);

    public static UiShaderSourcePairReplacementProjection Project(
        UiShaderSourcePairReplacementCapture capture)
    {
        ValidateReachedCounterCaptures(capture);

        var actions = new List<UiShaderSourcePairReplacementAction>
        {
            new(
                UiShaderSourcePairReplacementActionKind.CaptureSourceControlPointer,
                SourceControlLoadRawFileOffset,
                unchecked(capture.SourcePointer + SourceControlPointerOffset),
                SourceControlPointerOffset,
                capture.SourceControlPointer),
            new(
                UiShaderSourcePairReplacementActionKind.CaptureSourceBackingPointer,
                SourceBackingLoadRawFileOffset,
                capture.SourcePointer,
                SourceBackingPointerOffset,
                capture.SourceBackingPointer)
        };

        if (capture.SourceControlPointer != 0)
        {
            actions.Add(new UiShaderSourcePairReplacementAction(
                UiShaderSourcePairReplacementActionKind.IncrementIncomingControlReferenceCount,
                IncomingControlIncrementRawFileOffset,
                unchecked(capture.SourceControlPointer + ReferenceCountOffset),
                ReferenceCountOffset,
                Value: 1));
        }

        actions.Add(new UiShaderSourcePairReplacementAction(
            UiShaderSourcePairReplacementActionKind.CaptureOldDestinationControlPointer,
            OldDestinationControlLoadRawFileOffset,
            unchecked(capture.DestinationPointer + DestinationControlPointerOffset),
            DestinationControlPointerOffset,
            capture.OldDestinationControlPointer));

        bool? oldControlReachedZero = null;
        bool? oldSecondaryReachedZero = null;
        uint finalControlStoreOffset = DestinationControlStoreWithoutCallbacksRawFileOffset;
        uint finalBackingStoreOffset = DestinationBackingStoreWithoutCallbacksRawFileOffset;

        if (capture.OldDestinationControlPointer != 0)
        {
            uint oldControlCount = capture.OldDestinationControlCountReturnedByXadd!.Value;
            uint controlCountAfter = unchecked(oldControlCount - 1);
            oldControlReachedZero = controlCountAfter == 0;

            actions.Add(new UiShaderSourcePairReplacementAction(
                UiShaderSourcePairReplacementActionKind.DecrementOldDestinationControlReferenceCount,
                OldDestinationControlCounterXaddRawFileOffset,
                unchecked(capture.OldDestinationControlPointer + ReferenceCountOffset),
                ReferenceCountOffset,
                CounterValueReturnedByXadd: oldControlCount,
                CounterValueAfterXadd: controlCountAfter));
            actions.Add(new UiShaderSourcePairReplacementAction(
                UiShaderSourcePairReplacementActionKind.BranchOnOldDestinationControlCounterResult,
                OldDestinationControlCounterBranchRawFileOffset,
                BranchTaken: controlCountAfter != 0,
                BranchTargetRawFileOffset: OldDestinationControlCounterNonzeroBranchTargetRawFileOffset));

            if (controlCountAfter == 0)
            {
                finalControlStoreOffset = DestinationControlStoreAfterCallbacksRawFileOffset;
                finalBackingStoreOffset = DestinationBackingStoreAfterCallbacksRawFileOffset;
                actions.Add(new UiShaderSourcePairReplacementAction(
                    UiShaderSourcePairReplacementActionKind.CallOldDestinationCallbackSlot0,
                    OldDestinationCallbackSlot0CallRawFileOffset,
                    capture.OldDestinationControlPointer,
                    CallbackVtableSlotOffset: CallbackSlot0Offset));

                uint secondaryCount = capture.OldDestinationSecondaryCountReturnedByXadd!.Value;
                uint secondaryCountAfter = unchecked(secondaryCount - 1);
                oldSecondaryReachedZero = secondaryCountAfter == 0;
                actions.Add(new UiShaderSourcePairReplacementAction(
                    UiShaderSourcePairReplacementActionKind.DecrementOldDestinationSecondaryReferenceCount,
                    OldDestinationSecondaryCounterXaddRawFileOffset,
                    unchecked(capture.OldDestinationControlPointer + SecondaryReferenceCountOffset),
                    SecondaryReferenceCountOffset,
                    CounterValueReturnedByXadd: secondaryCount,
                    CounterValueAfterXadd: secondaryCountAfter));
                actions.Add(new UiShaderSourcePairReplacementAction(
                    UiShaderSourcePairReplacementActionKind.BranchOnOldDestinationSecondaryCounterResult,
                    OldDestinationSecondaryCounterBranchRawFileOffset,
                    BranchTaken: secondaryCountAfter != 0,
                    BranchTargetRawFileOffset: OldDestinationSecondaryCounterNonzeroBranchTargetRawFileOffset));

                if (secondaryCountAfter == 0)
                {
                    actions.Add(new UiShaderSourcePairReplacementAction(
                        UiShaderSourcePairReplacementActionKind.CallOldDestinationCallbackSlot4,
                        OldDestinationCallbackSlot4CallRawFileOffset,
                        capture.OldDestinationControlPointer,
                        CallbackVtableSlotOffset: CallbackSlot4Offset));
                }
            }
        }

        actions.Add(new UiShaderSourcePairReplacementAction(
            UiShaderSourcePairReplacementActionKind.StoreIncomingControlPointer,
            finalControlStoreOffset,
            unchecked(capture.DestinationPointer + DestinationControlPointerOffset),
            DestinationControlPointerOffset,
            capture.SourceControlPointer));
        actions.Add(new UiShaderSourcePairReplacementAction(
            UiShaderSourcePairReplacementActionKind.StoreIncomingBackingPointer,
            finalBackingStoreOffset,
            unchecked(capture.DestinationPointer + DestinationBackingPointerOffset),
            DestinationBackingPointerOffset,
            capture.SourceBackingPointer));

        return new UiShaderSourcePairReplacementProjection(
            capture,
            Array.AsReadOnly(actions.ToArray()),
            oldControlReachedZero,
            oldSecondaryReachedZero);
    }

    private static void ValidateReachedCounterCaptures(UiShaderSourcePairReplacementCapture capture)
    {
        if (capture.OldDestinationControlPointer == 0)
        {
            if (capture.OldDestinationControlCountReturnedByXadd is not null
                || capture.OldDestinationSecondaryCountReturnedByXadd is not null)
            {
                throw new ArgumentException(
                    "Old destination counter values are supplied only when their native xadd is reached.",
                    nameof(capture));
            }

            return;
        }

        if (capture.OldDestinationControlCountReturnedByXadd is not uint oldControlCount)
        {
            throw new ArgumentException(
                "The old destination control counter xadd is reached for a non-null old destination.",
                nameof(capture));
        }

        if (oldControlCount == 1)
        {
            if (capture.OldDestinationSecondaryCountReturnedByXadd is null)
            {
                throw new ArgumentException(
                    "The secondary counter is captured when the first decrement reaches zero.",
                    nameof(capture));
            }
        }
        else if (capture.OldDestinationSecondaryCountReturnedByXadd is not null)
        {
            throw new ArgumentException(
                "The secondary counter xadd is not reached when the first decrement remains nonzero.",
                nameof(capture));
        }
    }
}
