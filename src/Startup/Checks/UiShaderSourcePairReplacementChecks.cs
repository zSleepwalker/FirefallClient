using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePairReplacementChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Shader-source pair-replacement evidence is pinned to the flat raw-offset IDB",
            UiShaderSourcePairReplacementProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePairReplacementProjectionRules.HelperEntryRawFileOffset == 0x12CC430
            && UiShaderSourcePairReplacementProjectionRules.ToCodeVirtualAddress(0x12CC430) == 0x16CD030
            && UiShaderSourcePairReplacementProjectionRules.CodeVirtualAddressOffsetFromRawFileOffset == 0x00400C00
            && UiShaderSourcePairReplacementProjectionRules.SourceControlPointerOffset == 4
            && UiShaderSourcePairReplacementProjectionRules.SourceBackingPointerOffset == 0
            && UiShaderSourcePairReplacementProjectionRules.DestinationControlPointerOffset == 4
            && UiShaderSourcePairReplacementProjectionRules.DestinationBackingPointerOffset == 0
            && UiShaderSourcePairReplacementProjectionRules.ReferenceCountOffset == 4
            && UiShaderSourcePairReplacementProjectionRules.SecondaryReferenceCountOffset == 8
            && UiShaderSourcePairReplacementProjectionRules.CallbackSlot0Offset == 0
            && UiShaderSourcePairReplacementProjectionRules.CallbackSlot4Offset == 4);

        check("Shader-source pair helper anchors retain the fresh captured instruction bytes",
            UiShaderSourcePairReplacementProjectionRules.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.BytesHex))
                .SequenceEqual(new (uint RawFileOffset, string BytesHex)[]
                {
                    (0x12CC439, "8B7804"),
                    (0x12CC43E, "8B10"),
                    (0x12CC440, "895508"),
                    (0x12CC443, "85FF"),
                    (0x12CC445, "740F"),
                    (0x12CC44F, "F00FC110"),
                    (0x12CC456, "8B5E04"),
                    (0x12CC459, "85DB"),
                    (0x12CC45B, "7434"),
                    (0x12CC45D, "8D4B04"),
                    (0x12CC460, "83C8FF"),
                    (0x12CC463, "F00FC101"),
                    (0x12CC467, "7528"),
                    (0x12CC469, "8B03"),
                    (0x12CC46D, "FF10"),
                    (0x12CC46F, "8D4308"),
                    (0x12CC472, "83C9FF"),
                    (0x12CC475, "F00FC108"),
                    (0x12CC479, "7507"),
                    (0x12CC47B, "8B03"),
                    (0x12CC47F, "FF5004"),
                    (0x12CC482, "8B4508"),
                    (0x12CC485, "897E04"),
                    (0x12CC489, "8906"),
                    (0x12CC491, "897E04"),
                    (0x12CC495, "8916")
                })
            && UiShaderSourcePairReplacementProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CC467)
                .BranchTargetRawFileOffset == 0x12CC491
            && UiShaderSourcePairReplacementProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CC445)
                .BranchTargetRawFileOffset == 0x12CC456
            && UiShaderSourcePairReplacementProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CC45B)
                .BranchTargetRawFileOffset == 0x12CC491
            && UiShaderSourcePairReplacementProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CC479)
                .BranchTargetRawFileOffset == 0x12CC482);

        bool sourceNullCrossesAreIndependent = true;
        foreach (uint sourceControl in new uint[] { 0, 0x30000000 })
        foreach (uint sourceBacking in new uint[] { 0, 0x40000000 })
        {
            UiShaderSourcePairReplacementProjection projection =
                UiShaderSourcePairReplacementProjectionRules.Project(
                    new UiShaderSourcePairReplacementCapture(
                        DestinationPointer: 0x10000000,
                        SourcePointer: 0x20000000,
                        SourceControlPointer: sourceControl,
                        SourceBackingPointer: sourceBacking,
                        OldDestinationControlPointer: 0,
                        OldDestinationControlCountReturnedByXadd: null,
                        OldDestinationSecondaryCountReturnedByXadd: null));
            sourceNullCrossesAreIndependent &=
                projection.Actions.Any(action =>
                    action.Kind == UiShaderSourcePairReplacementActionKind.IncrementIncomingControlReferenceCount)
                    == (sourceControl != 0)
                && projection.Actions.Single(action =>
                    action.Kind == UiShaderSourcePairReplacementActionKind.CaptureSourceControlPointer).Value
                    == sourceControl
                && projection.Actions.Single(action =>
                    action.Kind == UiShaderSourcePairReplacementActionKind.CaptureSourceBackingPointer).Value
                    == sourceBacking
                && projection.Actions.Single(action =>
                    action.Kind == UiShaderSourcePairReplacementActionKind.StoreIncomingControlPointer).Value
                    == sourceControl
                && projection.Actions.Single(action =>
                    action.Kind == UiShaderSourcePairReplacementActionKind.StoreIncomingBackingPointer).Value
                    == sourceBacking;
        }
        check("Null and non-null captured source control/backing pointers cross independently",
            sourceNullCrossesAreIndependent);

        UiShaderSourcePairReplacementProjection incomingIncrementAndDirectStore =
            UiShaderSourcePairReplacementProjectionRules.Project(
                new UiShaderSourcePairReplacementCapture(
                    DestinationPointer: 0x10000000,
                    SourcePointer: 0x20000000,
                    SourceControlPointer: 0x30000000,
                    SourceBackingPointer: 0x40000000,
                    OldDestinationControlPointer: 0,
                    OldDestinationControlCountReturnedByXadd: null,
                    OldDestinationSecondaryCountReturnedByXadd: null));
        check("Source fields are captured before incoming increment and destination control then backing stores",
            incomingIncrementAndDirectStore.Actions.Select(action => action.Kind).SequenceEqual(new[]
            {
                UiShaderSourcePairReplacementActionKind.CaptureSourceControlPointer,
                UiShaderSourcePairReplacementActionKind.CaptureSourceBackingPointer,
                UiShaderSourcePairReplacementActionKind.IncrementIncomingControlReferenceCount,
                UiShaderSourcePairReplacementActionKind.CaptureOldDestinationControlPointer,
                UiShaderSourcePairReplacementActionKind.StoreIncomingControlPointer,
                UiShaderSourcePairReplacementActionKind.StoreIncomingBackingPointer
            })
            && incomingIncrementAndDirectStore.Actions[^2].InstructionRawFileOffset == 0x12CC491
            && incomingIncrementAndDirectStore.Actions[^2].TargetAddress == 0x10000004
            && incomingIncrementAndDirectStore.Actions[^2].Value == 0x30000000
            && incomingIncrementAndDirectStore.Actions[^1].InstructionRawFileOffset == 0x12CC495
            && incomingIncrementAndDirectStore.Actions[^1].TargetAddress == 0x10000000
            && incomingIncrementAndDirectStore.Actions[^1].Value == 0x40000000
            && incomingIncrementAndDirectStore.OldDestinationControlCounterReachedZero is null
            && incomingIncrementAndDirectStore.OldDestinationSecondaryCounterReachedZero is null);

        uint[] oldControlCountCases = { 0, 1, 2, uint.MaxValue };
        bool controlCounterGatesMatchXaddResult = true;
        foreach (uint oldCount in oldControlCountCases)
        {
            UiShaderSourcePairReplacementCapture capture =
                new(
                    DestinationPointer: 0x10000000,
                    SourcePointer: 0x20000000,
                    SourceControlPointer: 0x30000000,
                    SourceBackingPointer: 0x40000000,
                    OldDestinationControlPointer: 0x50000000,
                    OldDestinationControlCountReturnedByXadd: oldCount,
                    OldDestinationSecondaryCountReturnedByXadd: oldCount == 1 ? 2 : null);
            UiShaderSourcePairReplacementProjection projection =
                UiShaderSourcePairReplacementProjectionRules.Project(capture);
            uint countAfter = unchecked(oldCount - 1);
            bool callbackReached = oldCount == 1;
            controlCounterGatesMatchXaddResult &=
                projection.OldDestinationControlCounterReachedZero == callbackReached
                && projection.Actions.Single(action =>
                    action.Kind == UiShaderSourcePairReplacementActionKind.DecrementOldDestinationControlReferenceCount)
                    is { CounterValueReturnedByXadd: var before, CounterValueAfterXadd: var after }
                    && before == oldCount
                    && after == countAfter
                && projection.Actions.Any(action =>
                    action.Kind == UiShaderSourcePairReplacementActionKind.CallOldDestinationCallbackSlot0)
                    == callbackReached
                && projection.Actions.Any(action =>
                    action.Kind == UiShaderSourcePairReplacementActionKind.DecrementOldDestinationSecondaryReferenceCount)
                    == callbackReached
                && projection.Actions[^2].InstructionRawFileOffset
                    == (callbackReached ? 0x12CC485u : 0x12CC491u)
                && projection.Actions[^1].InstructionRawFileOffset
                    == (callbackReached ? 0x12CC489u : 0x12CC495u);
        }
        check("Control xadd count gates cover 0, 1, 2, and uint max with unsigned decrement wrap",
            controlCounterGatesMatchXaddResult);

        bool secondaryCounterGatesMatchXaddResult = true;
        foreach (uint oldCount in new uint[] { 0, 1, 2, uint.MaxValue })
        {
            UiShaderSourcePairReplacementProjection projection =
                UiShaderSourcePairReplacementProjectionRules.Project(
                    new UiShaderSourcePairReplacementCapture(
                        DestinationPointer: 0x10000000,
                        SourcePointer: 0x20000000,
                        SourceControlPointer: 0,
                        SourceBackingPointer: 0x40000000,
                        OldDestinationControlPointer: 0x50000000,
                        OldDestinationControlCountReturnedByXadd: 1,
                        OldDestinationSecondaryCountReturnedByXadd: oldCount));
            secondaryCounterGatesMatchXaddResult &=
                projection.OldDestinationSecondaryCounterReachedZero == (oldCount == 1)
                && projection.Actions.Single(action =>
                    action.Kind == UiShaderSourcePairReplacementActionKind.DecrementOldDestinationSecondaryReferenceCount)
                    .CounterValueAfterXadd == unchecked(oldCount - 1)
                && projection.Actions.Any(action =>
                    action.Kind == UiShaderSourcePairReplacementActionKind.CallOldDestinationCallbackSlot4)
                    == (oldCount == 1)
                && projection.Actions[^2].InstructionRawFileOffset == 0x12CC485
                && projection.Actions[^1].InstructionRawFileOffset == 0x12CC489;
        }
        check("Secondary xadd count gates cover 0, 1, 2, and uint max without adding a separate decrement",
            secondaryCounterGatesMatchXaddResult);

        UiShaderSourcePairReplacementProjection bothCallbacks =
            UiShaderSourcePairReplacementProjectionRules.Project(
                new UiShaderSourcePairReplacementCapture(
                    DestinationPointer: 0x10000000,
                    SourcePointer: 0x20000000,
                    SourceControlPointer: 0x30000000,
                    SourceBackingPointer: 0x40000000,
                    OldDestinationControlPointer: 0x50000000,
                    OldDestinationControlCountReturnedByXadd: 1,
                    OldDestinationSecondaryCountReturnedByXadd: 1));
        check("Callback slot 0 precedes secondary xadd and callback slot 4, followed by ordered final stores",
            bothCallbacks.Actions.Select(action => action.Kind).SequenceEqual(new[]
            {
                UiShaderSourcePairReplacementActionKind.CaptureSourceControlPointer,
                UiShaderSourcePairReplacementActionKind.CaptureSourceBackingPointer,
                UiShaderSourcePairReplacementActionKind.IncrementIncomingControlReferenceCount,
                UiShaderSourcePairReplacementActionKind.CaptureOldDestinationControlPointer,
                UiShaderSourcePairReplacementActionKind.DecrementOldDestinationControlReferenceCount,
                UiShaderSourcePairReplacementActionKind.BranchOnOldDestinationControlCounterResult,
                UiShaderSourcePairReplacementActionKind.CallOldDestinationCallbackSlot0,
                UiShaderSourcePairReplacementActionKind.DecrementOldDestinationSecondaryReferenceCount,
                UiShaderSourcePairReplacementActionKind.BranchOnOldDestinationSecondaryCounterResult,
                UiShaderSourcePairReplacementActionKind.CallOldDestinationCallbackSlot4,
                UiShaderSourcePairReplacementActionKind.StoreIncomingControlPointer,
                UiShaderSourcePairReplacementActionKind.StoreIncomingBackingPointer
            })
            && bothCallbacks.Actions[6].CallbackVtableSlotOffset == 0
            && bothCallbacks.Actions[9].CallbackVtableSlotOffset == 4
            && bothCallbacks.Actions[^2].InstructionRawFileOffset == 0x12CC485
            && bothCallbacks.Actions[^1].InstructionRawFileOffset == 0x12CC489);

        bool invalidReachedCountsRejected = false;
        try
        {
            UiShaderSourcePairReplacementProjectionRules.Project(
                new UiShaderSourcePairReplacementCapture(
                    1, 2, 0, 0, 3, 1, null));
        }
        catch (ArgumentException)
        {
            invalidReachedCountsRejected = true;
        }
        check("The secondary count is supplied exactly when the first xadd reaches its secondary counter",
            invalidReachedCountsRejected);
    }
}
