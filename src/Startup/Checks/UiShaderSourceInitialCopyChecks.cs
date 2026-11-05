using Firefall.Reconstruction.Startup;

internal static class UiShaderSourceInitialCopyChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Shader-source initial-copy anchors match the pinned executable and raw-offset mapping",
            UiShaderSourceInitialCopyProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourceInitialCopyProjectionRules.ImageBase == 0x00400000
            && UiShaderSourceInitialCopyProjectionRules.TextSectionRva == 0x00001000
            && UiShaderSourceInitialCopyProjectionRules.TextSectionRawFileOffset == 0x00000400
            && UiShaderSourceInitialCopyProjectionRules.CodeVirtualAddressOffsetFromRawFileOffset == 0x00400C00
            && UiShaderSourceInitialCopyProjectionRules.ConstructorEntryRawFileOffset == 0x12CB830
            && UiShaderSourceInitialCopyProjectionRules.ResizeHelperRawFileOffset == 0x12CBCB0
            && UiShaderSourceInitialCopyProjectionRules.ToCodeVirtualAddress(0x12CB830) == 0x16CC430
            && UiShaderSourceInitialCopyProjectionRules.ToCodeVirtualAddress(0x12CBCB0) == 0x16CC8B0
            && UiShaderSourceInitialCopyProjectionRules.ToCodeVirtualAddress(0x159D8F4) == 0x199E4F4
            && UiShaderSourceInitialCopyProjectionRules.WrapperVtableVirtualAddress == 0x1C80CC4
            && UiShaderSourceInitialCopyProjectionRules.BackingPointerOffset == 4
            && UiShaderSourceInitialCopyProjectionRules.ControlPointerOffset == 8
            && UiShaderSourceInitialCopyProjectionRules.TrailingDwordOffset == 0x0C
            && UiShaderSourceInitialCopyProjectionRules.BackingDataPointerOffset == 4
            && UiShaderSourceInitialCopyProjectionRules.ResizePreserveFlag == 0);

        check("Initial-copy evidence retains exact stores, resize arguments, and the backing-only copy gate",
            UiShaderSourceInitialCopyProjectionRules.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.BytesHex))
                .SequenceEqual(new (uint RawFileOffset, string BytesHex)[]
                {
                    (0x12CB860, "C706C40CC801"),
                    (0x12CB866, "C7460400000000"),
                    (0x12CB86D, "C7460800000000"),
                    (0x12CB874, "6A00"),
                    (0x12CB876, "FF750C"),
                    (0x12CB87D, "C7460C00000000"),
                    (0x12CB884, "E827040000"),
                    (0x12CB889, "8B4604"),
                    (0x12CB88C, "85C0"),
                    (0x12CB88E, "7411"),
                    (0x12CB890, "FF750C"),
                    (0x12CB893, "FF7508"),
                    (0x12CB896, "FF7004"),
                    (0x12CB899, "E856202D00")
                })
            && UiShaderSourceInitialCopyProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CB884)
                .TargetVirtualAddress == 0x16CC8B0
            && UiShaderSourceInitialCopyProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CB88E)
                .BranchTargetRawFileOffset == 0x12CB8A1
            && UiShaderSourceInitialCopyProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CB899)
                .TargetVirtualAddress == 0x199E4F4);

        UiShaderSourceInitialCopyProjection beforeBackingRead =
            UiShaderSourceInitialCopyProjectionRules.Project(
                new UiShaderSourceInitialCopyCapture(
                    DestinationPointer: 0x10000000,
                    SourcePointer: 0,
                    RequestedSize: 0,
                    PostResizeBackingPointer: null,
                    PostResizeBackingDataPointer: null));
        check("Unreached post-resize reads remain absent without inferring allocation outcome or a copy call",
            beforeBackingRead.Actions.Select(action => action.Kind).SequenceEqual(new[]
            {
                UiShaderSourceInitialCopyActionKind.WriteDestinationVtable,
                UiShaderSourceInitialCopyActionKind.ZeroDestinationBackingPointer,
                UiShaderSourceInitialCopyActionKind.ZeroDestinationControlPointer,
                UiShaderSourceInitialCopyActionKind.ZeroDestinationTrailingDword,
                UiShaderSourceInitialCopyActionKind.CallResizeWithOptionalPreserve
            })
            && beforeBackingRead.Actions[^1].Argument0 == 0
            && beforeBackingRead.Actions[^1].Argument1 == 0
            && !beforeBackingRead.Actions.Any(action =>
                action.Kind == UiShaderSourceInitialCopyActionKind.CallCopyThunk));

        bool nullBackingSkipsCopyForIndependentSourceAndSize =
            new uint[] { 0, 7 }.All(size =>
                new uint[] { 0, 0x50000000 }.All(source =>
                {
                    UiShaderSourceInitialCopyProjection projection =
                        UiShaderSourceInitialCopyProjectionRules.Project(
                            new UiShaderSourceInitialCopyCapture(
                                DestinationPointer: 0x10000000,
                                SourcePointer: source,
                                RequestedSize: size,
                                PostResizeBackingPointer: 0,
                                PostResizeBackingDataPointer: null));
                    return projection.Actions.Any(action =>
                            action.Kind == UiShaderSourceInitialCopyActionKind.SkipCopyForNullBackingPointer)
                        && !projection.Actions.Any(action =>
                            action.Kind == UiShaderSourceInitialCopyActionKind.CallCopyThunk);
                }));
        check("A captured null backing pointer skips copying regardless of source pointer or requested size",
            nullBackingSkipsCopyForIndependentSourceAndSize);

        bool nonNullBackingAlwaysReachesCopyCall =
            new uint[] { 0, 1 }.All(size =>
                new uint[] { 0, 0x50000000 }.All(source =>
                {
                    UiShaderSourceInitialCopyProjection projection =
                        UiShaderSourceInitialCopyProjectionRules.Project(
                            new UiShaderSourceInitialCopyCapture(
                                DestinationPointer: 0x10000000,
                                SourcePointer: source,
                                RequestedSize: size,
                                PostResizeBackingPointer: 0x20000000,
                                PostResizeBackingDataPointer: 0x30000000));
                    UiShaderSourceInitialCopyAction copyCall = projection.Actions.Single(action =>
                        action.Kind == UiShaderSourceInitialCopyActionKind.CallCopyThunk);
                    return copyCall.Argument0 == 0x30000000
                        && copyCall.Argument1 == source
                        && copyCall.Argument2 == size;
                }));
        check("A non-null backing reaches the native copy call even for size zero or source zero",
            nonNullBackingAlwaysReachesCopyCall);

        UiShaderSourceInitialCopyProjection orderedCopyPath =
            UiShaderSourceInitialCopyProjectionRules.Project(
                new UiShaderSourceInitialCopyCapture(
                    DestinationPointer: 0x10000000,
                    SourcePointer: 0x50000000,
                    RequestedSize: 4,
                    PostResizeBackingPointer: 0x20000000,
                    PostResizeBackingDataPointer: 0x30000000));
        check("The full constructor plan keeps copy pushes and the tied 0x896 data read/push in native instruction order",
            orderedCopyPath.Actions
                .Select(action => (action.Kind, action.InstructionRawFileOffset))
                .SequenceEqual(new[]
                {
                    (UiShaderSourceInitialCopyActionKind.WriteDestinationVtable, 0x12CB860u),
                    (UiShaderSourceInitialCopyActionKind.ZeroDestinationBackingPointer, 0x12CB866u),
                    (UiShaderSourceInitialCopyActionKind.ZeroDestinationControlPointer, 0x12CB86Du),
                    (UiShaderSourceInitialCopyActionKind.ZeroDestinationTrailingDword, 0x12CB87Du),
                    (UiShaderSourceInitialCopyActionKind.CallResizeWithOptionalPreserve, 0x12CB884u),
                    (UiShaderSourceInitialCopyActionKind.CapturePostResizeBackingPointer, 0x12CB889u),
                    (UiShaderSourceInitialCopyActionKind.PushRequestedSizeToCopyThunk, 0x12CB890u),
                    (UiShaderSourceInitialCopyActionKind.PushSourcePointerToCopyThunk, 0x12CB893u),
                    (UiShaderSourceInitialCopyActionKind.CaptureBackingDataPointer, 0x12CB896u),
                    (UiShaderSourceInitialCopyActionKind.PushBackingDataPointerToCopyThunk, 0x12CB896u),
                    (UiShaderSourceInitialCopyActionKind.CallCopyThunk, 0x12CB899u)
                }));

        UiShaderSourceInitialCopyProjection nullDataPointer =
            UiShaderSourceInitialCopyProjectionRules.Project(
                new UiShaderSourceInitialCopyCapture(
                    DestinationPointer: 0x10000000,
                    SourcePointer: 0x50000000,
                    RequestedSize: 4,
                    PostResizeBackingPointer: 0x20000000,
                    PostResizeBackingDataPointer: 0));
        check("A reached null backing-data value is passed through without inventing a data-pointer gate",
            nullDataPointer.Actions.Single(action =>
                action.Kind == UiShaderSourceInitialCopyActionKind.CallCopyThunk).Argument0 == 0);

        bool unreachedBackingRejectsDataCapture = false;
        bool nullBackingRejectsDataCapture = false;
        bool nonNullBackingRequiresDataCapture = false;
        try
        {
            UiShaderSourceInitialCopyProjectionRules.Project(
                new UiShaderSourceInitialCopyCapture(1, 2, 3, null, 0));
        }
        catch (ArgumentException)
        {
            unreachedBackingRejectsDataCapture = true;
        }

        try
        {
            UiShaderSourceInitialCopyProjectionRules.Project(
                new UiShaderSourceInitialCopyCapture(1, 2, 3, 0, 4));
        }
        catch (ArgumentException)
        {
            nullBackingRejectsDataCapture = true;
        }

        try
        {
            UiShaderSourceInitialCopyProjectionRules.Project(
                new UiShaderSourceInitialCopyCapture(1, 2, 3, 4, null));
        }
        catch (ArgumentException)
        {
            nonNullBackingRequiresDataCapture = true;
        }

        check("Optional backing/data captures preserve which native reads were actually reached",
            unreachedBackingRejectsDataCapture
            && nullBackingRejectsDataCapture
            && nonNullBackingRequiresDataCapture);
    }
}
