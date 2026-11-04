using Firefall.Reconstruction.Startup;

internal static class UiShaderSourceNonPoolReleaseChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Nonpool header-release vtable and entry anchors match the pinned image and address mappings",
            UiShaderSourceNonPoolReleaseProjection.PinnedBinarySha256
                == "97362D668FF64778193A64759265E4AE90842043C4A15B627DF9902FD9135445"
            && UiShaderSourceNonPoolReleaseProjection.NonPoolControlVtableVirtualAddress == 0x1C80CB0
            && UiShaderSourceNonPoolReleaseProjection.NonPoolControlVtableRawFileOffset == 0x187F8B0
            && UiShaderSourceNonPoolReleaseProjection.NonPoolControlVtableVirtualAddress
                - UiShaderSourceNonPoolReleaseProjection.DataVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourceNonPoolReleaseProjection.NonPoolControlVtableRawFileOffset
            && UiShaderSourceNonPoolReleaseProjection.NonPoolControlVtableSlotPointerBytes
                == "80 D5 6C 01 A0 D5 6C 01 D0 D5 6C 01"
            && UiShaderSourceNonPoolReleaseProjection.HeaderReleaseSlotPointerBytes == "80 D5 6C 01"
            && UiShaderSourceNonPoolReleaseProjection.HeaderReleaseSlotVirtualAddress == 0x16CD580
            && UiShaderSourceNonPoolReleaseProjection.HeaderReleaseSlotRawFileOffset == 0x12CC980
            && UiShaderSourceNonPoolReleaseProjection.HeaderReleaseSlotVirtualAddress
                - UiShaderSourceNonPoolReleaseProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourceNonPoolReleaseProjection.HeaderReleaseSlotRawFileOffset
            && UiShaderSourceNonPoolReleaseProjection.HeaderReleaseEntryEndRawFileOffsetExclusive
                - UiShaderSourceNonPoolReleaseProjection.HeaderReleaseSlotRawFileOffset
                == 29
            && UiShaderSourceNonPoolReleaseProjection.HeaderReleaseEntryBytes
                == "56 8B 71 0C 85 F6 74 13 8B 4E 04 2B 4E 08 E8 CD C3 FF FF 8B CE 5E E9 C5 C3 FF FF 5E C3");

        check("Captured slot-0 bytes retain the null branch, two reads, call, and tail jump as distinct anchors",
            UiShaderSourceNonPoolReleaseProjection.HeaderPointerOffset == 0x0C
            && UiShaderSourceNonPoolReleaseProjection.HeaderWordAtOffset4 == 4
            && UiShaderSourceNonPoolReleaseProjection.HeaderWordAtOffset8 == 8
            && UiShaderSourceNonPoolReleaseProjection.NullHeaderBranchRawFileOffset == 0x12CC986
            && UiShaderSourceNonPoolReleaseProjection.NullHeaderReturnRawFileOffset == 0x12CC99B
            && UiShaderSourceNonPoolReleaseProjection.NullHeaderBranchRawFileOffset + 2 + 0x13
                == UiShaderSourceNonPoolReleaseProjection.NullHeaderReturnRawFileOffset
            && UiShaderSourceNonPoolReleaseProjection.NullHeaderReturnBytes == "5E C3"
            && UiShaderSourceNonPoolReleaseProjection.DifferenceCallRawFileOffset == 0x12CC98E
            && UiShaderSourceNonPoolReleaseProjection.DifferenceCallBytes == "E8 CD C3 FF FF"
            && UiShaderSourceNonPoolReleaseProjection.HeaderTailJumpRawFileOffset == 0x12CC996
            && UiShaderSourceNonPoolReleaseProjection.HeaderTailJumpBytes == "E9 C5 C3 FF FF"
            && UiShaderSourceNonPoolReleaseProjection.OffsetReleaseHelperRawFileOffset == 0x12C8D60
            && unchecked(UiShaderSourceNonPoolReleaseProjection.DifferenceCallRawFileOffset + 5 - 0x3C33)
                == UiShaderSourceNonPoolReleaseProjection.OffsetReleaseHelperRawFileOffset
            && unchecked(UiShaderSourceNonPoolReleaseProjection.HeaderTailJumpRawFileOffset + 5 - 0x3C3B)
                == UiShaderSourceNonPoolReleaseProjection.OffsetReleaseHelperRawFileOffset
            && UiShaderSourceNonPoolReleaseProjection.DirectEntryInstructions
                .Where(instruction => instruction.TargetRawFileOffset.HasValue)
                .Select(instruction => instruction.TargetRawFileOffset!.Value)
                .SequenceEqual(new uint[]
                {
                    0x12CC99B,
                    0x12C8D60,
                    0x12C8D60
                }));

        UiShaderSourceNonPoolReleasePlan nullHeader =
            UiShaderSourceNonPoolReleaseProjection.Project(
                new UiShaderSourceNonPoolReleaseCapture(0, null, null));
        check("Null captured header returns without requiring +4/+8 captures or projecting helper transfers",
            nullHeader.Capture.HeaderPointer == 0
            && nullHeader.WrappedHeaderDifference is null
            && nullHeader.Actions.SequenceEqual(new[]
            {
                new UiShaderSourceNonPoolReleaseAction(
                    UiShaderSourceNonPoolReleaseActionKind.Return,
                    0x12CC99B)
            }));

        UiShaderSourceNonPoolReleasePlan zeroDifference =
            UiShaderSourceNonPoolReleaseProjection.Project(
                new UiShaderSourceNonPoolReleaseCapture(0x12345678, 0x40, 0x40));
        check("Zero wrapped difference preserves both the helper call and distinct tail jump",
            zeroDifference.WrappedHeaderDifference == 0
            && zeroDifference.Actions.SequenceEqual(new[]
            {
                new UiShaderSourceNonPoolReleaseAction(
                    UiShaderSourceNonPoolReleaseActionKind.CallOffsetReleaseHelper,
                    0x12CC98E,
                    HelperArgument: 0,
                    TargetRawFileOffset: 0x12C8D60),
                new UiShaderSourceNonPoolReleaseAction(
                    UiShaderSourceNonPoolReleaseActionKind.TailJumpOffsetReleaseHelper,
                    0x12CC996,
                    HelperArgument: 0x12345678,
                    TargetRawFileOffset: 0x12C8D60)
            }));

        UiShaderSourceNonPoolReleasePlan nonzeroDifference =
            UiShaderSourceNonPoolReleaseProjection.Project(
                new UiShaderSourceNonPoolReleaseCapture(0x87654321, 0xA0, 0x18));
        check("Nonzero difference is passed first, followed by a tail jump carrying the original header pointer",
            nonzeroDifference.WrappedHeaderDifference == 0x88
            && nonzeroDifference.Actions.Length == 2
            && nonzeroDifference.Actions[0].Kind
                == UiShaderSourceNonPoolReleaseActionKind.CallOffsetReleaseHelper
            && nonzeroDifference.Actions[0].HelperArgument == 0x88
            && nonzeroDifference.Actions[0].TargetRawFileOffset == 0x12C8D60
            && nonzeroDifference.Actions[1].Kind
                == UiShaderSourceNonPoolReleaseActionKind.TailJumpOffsetReleaseHelper
            && nonzeroDifference.Actions[1].HelperArgument == 0x87654321
            && nonzeroDifference.Actions[1].TargetRawFileOffset == 0x12C8D60);

        UiShaderSourceNonPoolReleasePlan wrappingDifference =
            UiShaderSourceNonPoolReleaseProjection.Project(
                new UiShaderSourceNonPoolReleaseCapture(0xABCDEF01, 0, 1));
        check("Captured header-word subtraction preserves unsigned 32-bit wraparound",
            wrappingDifference.WrappedHeaderDifference == uint.MaxValue
            && wrappingDifference.Actions[0].HelperArgument == uint.MaxValue
            && wrappingDifference.Actions[1].HelperArgument == 0xABCDEF01);

        bool missingOffset4Rejected = false;
        try
        {
            UiShaderSourceNonPoolReleaseProjection.Project(
                new UiShaderSourceNonPoolReleaseCapture(1, null, 0));
        }
        catch (ArgumentException exception)
        {
            missingOffset4Rejected = exception.ParamName == "capture";
        }

        bool missingOffset8Rejected = false;
        try
        {
            UiShaderSourceNonPoolReleaseProjection.Project(
                new UiShaderSourceNonPoolReleaseCapture(1, 0, null));
        }
        catch (ArgumentException exception)
        {
            missingOffset8Rejected = exception.ParamName == "capture";
        }

        check("Nonnull captured header rejects either missing word capture",
            missingOffset4Rejected && missingOffset8Rejected);
    }
}
