using Firefall.Reconstruction.Startup;

internal static class UiShaderSourceDestructorChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI shader-source destructor slot 0 maps from the vtable pointer to the raw code address",
            UiShaderSourceDestructorNativeLayout.WrapperVtableVirtualAddress == 0x1C80CC4
            && UiShaderSourceDestructorNativeLayout.WrapperVtableRawFileOffset == 0x187F8C4
            && UiShaderSourceDestructorNativeLayout.WrapperVtableSlot0Bytes == "A0 C3 6C 01"
            && UiShaderSourceDestructorNativeLayout.DestructorEntryVirtualAddress == 0x16CC3A0
            && UiShaderSourceDestructorNativeLayout.DestructorEntryRawFileOffset == 0x12CB7A0
            && UiShaderSourceDestructorNativeLayout.DestructorEntryVirtualAddress
                - UiShaderSourceDestructorNativeLayout.CodeVirtualAddressDelta
                == UiShaderSourceDestructorNativeLayout.DestructorEntryRawFileOffset
            && UiShaderSourceDestructorNativeLayout.DestructorLength == 0x50);

        check("First counter JNZ immediately follows XADD and therefore tests its post-decrement result",
            UiShaderSourceDestructorNativeLayout.ControlReleaseInstructions.Take(5).SequenceEqual(new[]
            {
                new UiShaderSourceDestructorInstructionEvidence(
                    "Read wrapper control pointer", 0x12CB7A7, "8B 7E 08", WrapperRelativeOffset: 0x08),
                new UiShaderSourceDestructorInstructionEvidence(
                    "Test control pointer and branch if null", 0x12CB7AA, "85 FF 74 27",
                    WrapperRelativeOffset: 0x08),
                new UiShaderSourceDestructorInstructionEvidence(
                    "Atomic decrement at control +4; XADD sets flags from the post-decrement sum",
                    0x12CB7B7, "F0 0F C1 02", ControlRelativeOffset: 0x04),
                new UiShaderSourceDestructorInstructionEvidence(
                    "Immediately following JNZ skips slot 0 when the post-decrement sum is nonzero",
                    0x12CB7BB, "75 17", ControlRelativeOffset: 0x04),
                new UiShaderSourceDestructorInstructionEvidence(
                    "Call control vtable slot 0 when that JNZ is not taken", 0x12CB7C1, "FF 10")
            })
            && UiShaderSourceDestructorNativeLayout.FirstCounterXaddRawFileOffset + 4
                == UiShaderSourceDestructorNativeLayout.FirstCounterJnzRawFileOffset);

        check("Second counter DEC overwrites XADD flags before its following JNZ",
            UiShaderSourceDestructorNativeLayout.ControlReleaseInstructions.Skip(5).SequenceEqual(new[]
            {
                new UiShaderSourceDestructorInstructionEvidence(
                    "Atomic decrement at control +8; XADD returns the prior value in EBX",
                    0x12CB7C6, "F0 0F C1 18", ControlRelativeOffset: 0x08),
                new UiShaderSourceDestructorInstructionEvidence(
                    "DEC EBX overwrites XADD flags using the returned prior value minus one",
                    0x12CB7CA, "4B", ControlRelativeOffset: 0x08),
                new UiShaderSourceDestructorInstructionEvidence(
                    "Following JNZ skips slot +4 when the returned prior value was not one",
                    0x12CB7CB, "75 07", ControlRelativeOffset: 0x08),
                new UiShaderSourceDestructorInstructionEvidence(
                    "Call control vtable slot +4 when DEC produced zero", 0x12CB7D1, "FF 50 04")
            })
            && UiShaderSourceDestructorNativeLayout.SecondCounterXaddRawFileOffset + 4
                == UiShaderSourceDestructorNativeLayout.SecondCounterDecRawFileOffset
            && UiShaderSourceDestructorNativeLayout.SecondCounterDecRawFileOffset + 1
                == UiShaderSourceDestructorNativeLayout.SecondCounterJnzRawFileOffset);

        UiShaderSourceDestructorProjection firstCounterOldZero =
            UiShaderSourceDestructorNativeLayout.Project(
                hasControlPointer: true,
                controlFirstCounterBeforeXadd: 0,
                controlSecondCounterBeforeXadd: null,
                deletingFlags: 0);
        UiShaderSourceDestructorProjection firstCounterDecrementsToZero =
            UiShaderSourceDestructorNativeLayout.Project(
                hasControlPointer: true,
                controlFirstCounterBeforeXadd: 1,
                controlSecondCounterBeforeXadd: 1,
                deletingFlags: 0);
        check("First-counter projection follows XADD result flags rather than its returned old value",
            firstCounterOldZero.ControlFirstCounterAfterXadd == uint.MaxValue
            && !firstCounterOldZero.CallsControlVtableSlot0
            && firstCounterDecrementsToZero.ControlFirstCounterAfterXadd == 0
            && firstCounterDecrementsToZero.CallsControlVtableSlot0
            && firstCounterDecrementsToZero.CallsControlVtableSlot4);

        UiShaderSourceDestructorProjection secondCounterOldZero =
            UiShaderSourceDestructorNativeLayout.Project(
                hasControlPointer: true,
                controlFirstCounterBeforeXadd: 1,
                controlSecondCounterBeforeXadd: 0,
                deletingFlags: 0);
        UiShaderSourceDestructorProjection secondCounterOldOne =
            UiShaderSourceDestructorNativeLayout.Project(
                hasControlPointer: true,
                controlFirstCounterBeforeXadd: 1,
                controlSecondCounterBeforeXadd: 1,
                deletingFlags: 0);
        check("Second-counter projection follows DEC of the XADD-returned value",
            !secondCounterOldZero.CallsControlVtableSlot4
            && secondCounterOldOne.CallsControlVtableSlot4);

        check("Wrapper disposition tests the deleting flag, replaces the vptr, then conditionally calls release",
            UiShaderSourceDestructorNativeLayout.WrapperDispositionInstructions.SequenceEqual(new[]
            {
                new UiShaderSourceDestructorInstructionEvidence(
                    "Test deleting-flag bit 0", 0x12CB7D5, "F6 45 08 01"),
                new UiShaderSourceDestructorInstructionEvidence(
                    "Replace wrapper vptr", 0x12CB7D9, "C7 06 14 FC C5 01",
                    WrapperRelativeOffset: 0x00),
                new UiShaderSourceDestructorInstructionEvidence(
                    "Branch on the deleting-flag test", 0x12CB7DF, "74 07"),
                new UiShaderSourceDestructorInstructionEvidence(
                    "Set ECX to the wrapper before the conditional call", 0x12CB7E1, "8B CE"),
                new UiShaderSourceDestructorInstructionEvidence(
                    "Call wrapper storage-release helper when deleting-flag bit 0 is set",
                    0x12CB7E3, "E8 78 D5 FF FF"),
                new UiShaderSourceDestructorInstructionEvidence(
                    "Return and pop the deleting-flag stack argument", 0x12CB7ED, "C2 04 00")
            })
            && UiShaderSourceDestructorNativeLayout.DeletingFlagStackArgumentOffset == 0x08
            && UiShaderSourceDestructorNativeLayout.DeletingFlagMask == 1
            && UiShaderSourceDestructorNativeLayout.ReplacementVptrVirtualAddress == 0x1C5FC14
            && UiShaderSourceDestructorNativeLayout.WrapperStorageReleaseHelperRawFileOffset == 0x12C8D60);

        UiShaderSourceDestructorProjection nonDeleting =
            UiShaderSourceDestructorNativeLayout.Project(false, null, null, 0);
        UiShaderSourceDestructorProjection deleting =
            UiShaderSourceDestructorNativeLayout.Project(false, null, null, 1);
        UiShaderSourceDestructorProjection otherFlagOnly =
            UiShaderSourceDestructorNativeLayout.Project(false, null, null, 0xFFFFFFFE);
        check("Deleting-flag projection gates only the conditional wrapper-release call",
            nonDeleting.ReplacesWrapperVptr
            && !nonDeleting.CallsWrapperStorageReleaseHelper
            && deleting.CallsWrapperStorageReleaseHelper
            && !otherFlagOnly.CallsWrapperStorageReleaseHelper
            && !nonDeleting.DirectlyClearsWrapperBackingPointer
            && !nonDeleting.DirectlyClearsWrapperControlPointer);

        bool missingFirstCounterRejected = false;
        try
        {
            UiShaderSourceDestructorNativeLayout.Project(true, null, null, 0);
        }
        catch (ArgumentException exception)
        {
            missingFirstCounterRejected = exception.ParamName == "controlFirstCounterBeforeXadd";
        }

        bool unexpectedSecondCounterRejected = false;
        try
        {
            UiShaderSourceDestructorNativeLayout.Project(true, 2, 1, 0);
        }
        catch (ArgumentException exception)
        {
            unexpectedSecondCounterRejected = exception.ParamName == "controlSecondCounterBeforeXadd";
        }

        check("Counter projection rejects captures that are unreachable on the observed branch",
            missingFirstCounterRejected && unexpectedSecondCounterRejected);
    }
}
