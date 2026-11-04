using Firefall.Reconstruction.Startup;

internal static class UiShaderSourceCopyChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Shader-source copy constructor anchors match the pinned executable",
            UiShaderSourceCopyProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourceCopyProjectionRules.ImageBase == 0x00400000
            && UiShaderSourceCopyProjectionRules.TextSectionRva == 0x00001000
            && UiShaderSourceCopyProjectionRules.TextSectionRawFileOffset == 0x00000400
            && UiShaderSourceCopyProjectionRules.CodeVirtualAddressOffsetFromRawFileOffset
                == 0x00400C00
            && UiShaderSourceCopyProjectionRules.ConstructorEntryRawFileOffset == 0x12CB7F0
            && UiShaderSourceCopyProjectionRules.PairAssignmentHelperRawFileOffset == 0x12CC430
            && UiShaderSourceCopyProjectionRules.ToCodeVirtualAddress(0x12CB7F0) == 0x16CC3F0
            && UiShaderSourceCopyProjectionRules.ToCodeVirtualAddress(0x12CC430) == 0x16CD030
            && UiShaderSourceCopyProjectionRules.DestinationVtableStoreRawFileOffset == 0x12CB801
            && UiShaderSourceCopyProjectionRules.DestinationBackingZeroRawFileOffset == 0x12CB807
            && UiShaderSourceCopyProjectionRules.DestinationControlZeroRawFileOffset == 0x12CB80D
            && UiShaderSourceCopyProjectionRules.PairAssignmentCallRawFileOffset == 0x12CB814
            && UiShaderSourceCopyProjectionRules.DestinationControlStoreRawFileOffset == 0x12CC491
            && UiShaderSourceCopyProjectionRules.DestinationBackingStoreRawFileOffset == 0x12CC495
            && UiShaderSourceCopyProjectionRules.SourceTrailingDwordLoadRawFileOffset == 0x12CB819
            && UiShaderSourceCopyProjectionRules.DestinationTrailingDwordStoreRawFileOffset == 0x12CB81C
            && UiShaderSourceCopyProjectionRules.BackingPointerOffset == 4
            && UiShaderSourceCopyProjectionRules.ControlPointerOffset == 8
            && UiShaderSourceCopyProjectionRules.TrailingDwordOffset == 0x0C
            && UiShaderSourceCopyProjectionRules.ReferenceCountOffset == 4
            && UiShaderSourceCopyProjectionRules.WrapperVtableVirtualAddress == 0x1C80CC4);

        check("Shader-source copy evidence records the exact constructor and helper instruction bytes",
            UiShaderSourceCopyProjectionRules.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.BytesHex))
                .SequenceEqual(new (uint RawFileOffset, string BytesHex)[]
                {
                    (0x12CB801, "C707C40CC801"),
                    (0x12CB807, "C70100000000"),
                    (0x12CB80D, "C7410400000000"),
                    (0x12CB814, "E8170C0000"),
                    (0x12CC439, "8B7804"),
                    (0x12CC43E, "8B10"),
                    (0x12CC443, "85FF"),
                    (0x12CC445, "740F"),
                    (0x12CC447, "8D4704"),
                    (0x12CC44A, "BA01000000"),
                    (0x12CC44F, "F00FC110"),
                    (0x12CC456, "8B5E04"),
                    (0x12CC45B, "7434"),
                    (0x12CC491, "897E04"),
                    (0x12CC495, "8916"),
                    (0x12CB819, "8B460C"),
                    (0x12CB81C, "89470C")
                })
            && UiShaderSourceCopyProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CB814)
                .TargetVirtualAddress == 0x16CD030
            && UiShaderSourceCopyProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CC445)
                .TargetVirtualAddress == 0x16CD056
            && UiShaderSourceCopyProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CC45B)
                .TargetVirtualAddress == 0x16CD091);

        UiShaderSourceCopyProjection ordinary =
            UiShaderSourceCopyProjectionRules.Project(
                new UiShaderSourceCopyCapture(
                    DestinationPointer: 0x10000000,
                    SourcePointer: 0x20000000,
                    SourceControlPointer: 0x30000000,
                    SourceBackingPointer: 0x40000000,
                    SourceTrailingDword: 0x50000000));
        check("Non-null control increments its +4 counter before ordered destination pair and trailing-field writes",
            ordinary.Actions.Select(action => action.Kind).SequenceEqual(new[]
            {
                UiShaderSourceCopyActionKind.WriteDestinationVtable,
                UiShaderSourceCopyActionKind.ZeroDestinationBackingPointer,
                UiShaderSourceCopyActionKind.ZeroDestinationControlPointer,
                UiShaderSourceCopyActionKind.CallPairAssignmentHelper,
                UiShaderSourceCopyActionKind.CaptureSourceControlPointer,
                UiShaderSourceCopyActionKind.CaptureSourceBackingPointer,
                UiShaderSourceCopyActionKind.IncrementCapturedControlReferenceCount,
                UiShaderSourceCopyActionKind.ReadFreshDestinationControlPointer,
                UiShaderSourceCopyActionKind.SkipDestinationControlRelease,
                UiShaderSourceCopyActionKind.WriteDestinationControlPointer,
                UiShaderSourceCopyActionKind.WriteDestinationBackingPointer,
                UiShaderSourceCopyActionKind.CaptureSourceTrailingDword,
                UiShaderSourceCopyActionKind.WriteDestinationTrailingDword
            })
            && ordinary.Actions[6].TargetVirtualAddress == 0x30000004
            && ordinary.Actions[6].Value == 1
            && ordinary.Actions[9].TargetVirtualAddress == 0x10000008
            && ordinary.Actions[9].Value == 0x30000000
            && ordinary.Actions[10].TargetVirtualAddress == 0x10000004
            && ordinary.Actions[10].Value == 0x40000000
            && ordinary.Actions[11].TargetVirtualAddress == 0x2000000C
            && ordinary.Actions[11].Value == 0x50000000
            && ordinary.Actions[12].TargetVirtualAddress == 0x1000000C
            && ordinary.Actions[12].Value == 0x50000000);

        bool allNullCombinationsCross = true;
        foreach (uint controlPointer in new uint[] { 0, 0x30000000 })
        foreach (uint backingPointer in new uint[] { 0, 0x40000000 })
        {
            UiShaderSourceCopyProjection projection =
                UiShaderSourceCopyProjectionRules.Project(
                    new UiShaderSourceCopyCapture(
                        DestinationPointer: 0x10000000,
                        SourcePointer: 0x20000000,
                        SourceControlPointer: controlPointer,
                        SourceBackingPointer: backingPointer,
                        SourceTrailingDword: 0x50000000));
            bool incrementPresent = projection.Actions.Any(action =>
                action.Kind == UiShaderSourceCopyActionKind.IncrementCapturedControlReferenceCount);
            UiShaderSourceCopyAction destinationControlWrite = projection.Actions.Single(action =>
                action.Kind == UiShaderSourceCopyActionKind.WriteDestinationControlPointer);
            UiShaderSourceCopyAction destinationBackingWrite = projection.Actions.Single(action =>
                action.Kind == UiShaderSourceCopyActionKind.WriteDestinationBackingPointer);
            UiShaderSourceCopyAction trailingWrite = projection.Actions.Single(action =>
                action.Kind == UiShaderSourceCopyActionKind.WriteDestinationTrailingDword);
            allNullCombinationsCross &=
                incrementPresent == (controlPointer != 0)
                && destinationControlWrite.Value == controlPointer
                && destinationBackingWrite.Value == backingPointer
                && trailingWrite.Value == 0x50000000;
        }
        check("Null and non-null control/backing combinations preserve the pair and later dword independently",
            allNullCombinationsCross);

        UiShaderSourceCopyProjection wrappedPointers =
            UiShaderSourceCopyProjectionRules.Project(
                new UiShaderSourceCopyCapture(
                    DestinationPointer: uint.MaxValue,
                    SourcePointer: uint.MaxValue,
                    SourceControlPointer: uint.MaxValue,
                    SourceBackingPointer: 0,
                    SourceTrailingDword: 0));
        check("Captured 32-bit destination and control-relative addresses wrap without inventing a counter value",
            wrappedPointers.Actions[1].TargetVirtualAddress == 3
            && wrappedPointers.Actions[2].TargetVirtualAddress == 7
            && wrappedPointers.Actions[6].TargetVirtualAddress == 3
            && wrappedPointers.Actions[6].Value == 1
            && wrappedPointers.Actions[9].TargetVirtualAddress == 7
            && wrappedPointers.Actions[10].TargetVirtualAddress == 3);
    }
}
