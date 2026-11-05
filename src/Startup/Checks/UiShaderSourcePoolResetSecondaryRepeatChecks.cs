using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolResetSecondaryRepeatChecks
{
    public static void Run(Action<string, bool> check)
    {
        string[] capturedBytes =
            UiShaderSourcePoolResetSecondaryRepeatProjectionRules.CapturedRepeatBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        check("Secondary repeat sample, raw interval, and code virtual-address mapping are pinned",
            UiShaderSourcePoolResetSecondaryRepeatProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.ResetFunctionRawFileOffset
                == 0x12CDC50
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.RepeatStartRawFileOffset
                == 0x12CDDE6
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.RepeatEndRawFileOffsetExclusive
                == 0x12CDE01
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.RepeatStartCodeVirtualAddress
                == 0x16CE9E6
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.RepeatEndCodeVirtualAddressExclusive
                == 0x16CEA01
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.RawFileOffsetToCodeVirtualAddress(
                UiShaderSourcePoolResetSecondaryRepeatProjectionRules.RepeatStartRawFileOffset)
                == 0x16CE9E6
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.RawFileOffsetToCodeVirtualAddress(
                UiShaderSourcePoolResetSecondaryRepeatProjectionRules.RepeatEndRawFileOffsetExclusive)
                == 0x16CEA01
            && capturedBytes.Length == 0x1B
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.CapturedRepeatBytes
                == "8B 4D E0 E8 72 AF FF FF 8D 4B 08 E8 EA FC FF FF "
                    + "89 45 E0 85 C0 0F 85 67 FF FF FF"
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.Instructions
                .All(instruction => HasExactSlice(instruction, capturedBytes)));

        check("Both calls, the wrapped receiver argument, and nonzero branch resolve from their encodings",
            UiShaderSourcePoolResetSecondaryRepeatProjectionRules.SavedNodeStackOffset == -0x20
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.SavedPopResultStackOffset == -0x20
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.ReleaseBackendCallRawFileOffset
                == 0x12CDDE9
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.ReleaseBackendCallTargetRawFileOffset
                == 0x12C8D60
            && unchecked(
                    UiShaderSourcePoolResetSecondaryRepeatProjectionRules.ReleaseBackendCallRawFileOffset
                    + 5u
                    + 0xFFFFAF72u)
                == UiShaderSourcePoolResetSecondaryRepeatProjectionRules
                    .ReleaseBackendCallTargetRawFileOffset
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.ReceiverArgumentFormationRawFileOffset
                == 0x12CDDEE
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.ReceiverArgumentOffset == 8
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.PopCallRawFileOffset == 0x12CDDF1
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.PopCallTargetRawFileOffset
                == 0x12CDAE0
            && unchecked(
                    UiShaderSourcePoolResetSecondaryRepeatProjectionRules.PopCallRawFileOffset
                    + 5u
                    + 0xFFFFFCEAu)
                == UiShaderSourcePoolResetSecondaryRepeatProjectionRules.PopCallTargetRawFileOffset
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.PopResultSaveRawFileOffset
                == 0x12CDDF6
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.PopResultTestRawFileOffset
                == 0x12CDDF9
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.PopResultNonzeroBranchRawFileOffset
                == 0x12CDDFB
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.AlignmentEntryRawFileOffset
                == 0x12CDD68
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.CountLoadRawFileOffset == 0x12CDE01
            && UiShaderSourcePoolResetSecondaryRepeatProjectionRules.InitialSecondaryZeroTargetRawFileOffset
                == 0x12CDE04
            && unchecked(
                    UiShaderSourcePoolResetSecondaryRepeatProjectionRules.PopResultNonzeroBranchRawFileOffset
                    + 6u
                    + 0xFFFFFF67u)
                == UiShaderSourcePoolResetSecondaryRepeatProjectionRules.AlignmentEntryRawFileOffset);

        var missingReleaseReturn = UiShaderSourcePoolResetSecondaryRepeatProjectionRules.Project(
            new UiShaderSourcePoolResetSecondaryRepeatCapture(
                SavedNodePointer: 0x11223344,
                ReleaseBackendReturnedNormally: null,
                ReceiverEbx: null,
                PopResultEax: null));
        check("A missing release return stops at its call and does not infer a next stage",
            missingReleaseReturn.Capture.SavedNodePointer == 0x11223344
            && missingReleaseReturn.ReleaseBackendCallReached
            && missingReleaseReturn.ReleaseBackendArgumentPointer == 0x11223344
            && !missingReleaseReturn.ReleaseBackendReturnCaptured
            && missingReleaseReturn.ReleaseBackendReturnedNormally is null
            && missingReleaseReturn.ReceiverArgumentPointer is null
            && !missingReleaseReturn.PopCallReached
            && !missingReleaseReturn.PopResultSaved
            && !missingReleaseReturn.PopResultTestReached
            && missingReleaseReturn.ContinuationRawFileOffset is null
            && missingReleaseReturn.StopRawFileOffset == 0x12CDDE9
            && missingReleaseReturn.Boundary
                == UiShaderSourcePoolResetSecondaryRepeatBoundary
                    .ReleaseBackendReturnUnavailableAtCallBoundary);

        var nonreturningRelease = UiShaderSourcePoolResetSecondaryRepeatProjectionRules.Project(
            new UiShaderSourcePoolResetSecondaryRepeatCapture(
                SavedNodePointer: 0x55667788,
                ReleaseBackendReturnedNormally: false,
                ReceiverEbx: null,
                PopResultEax: null));
        check("A captured nonreturning release also stops before receiver formation and pop",
            nonreturningRelease.ReleaseBackendReturnCaptured
            && nonreturningRelease.ReleaseBackendReturnedNormally == false
            && nonreturningRelease.ReceiverArgumentPointer is null
            && !nonreturningRelease.PopCallReached
            && nonreturningRelease.StopRawFileOffset == 0x12CDDE9
            && nonreturningRelease.Boundary
                == UiShaderSourcePoolResetSecondaryRepeatBoundary.ReleaseBackendDidNotReturn);

        var missingPopResult = UiShaderSourcePoolResetSecondaryRepeatProjectionRules.Project(
            new UiShaderSourcePoolResetSecondaryRepeatCapture(
                SavedNodePointer: 0x1000,
                ReleaseBackendReturnedNormally: true,
                ReceiverEbx: uint.MaxValue,
                PopResultEax: null));
        check("A missing pop result stops at the pop call after wrapped receiver-plus-eight formation",
            missingPopResult.ReleaseBackendReturnCaptured
            && missingPopResult.ReleaseBackendReturnedNormally == true
            && missingPopResult.ReceiverArgumentPointer == 7
            && missingPopResult.PopCallReached
            && !missingPopResult.PopResultSaved
            && missingPopResult.PopResultStackValue is null
            && !missingPopResult.PopResultTestReached
            && missingPopResult.PopResultWasZero is null
            && missingPopResult.BranchTargetRawFileOffset is null
            && missingPopResult.ContinuationRawFileOffset is null
            && missingPopResult.StopRawFileOffset == 0x12CDDF1
            && !missingPopResult.AlignmentEntryReached
            && !missingPopResult.CountLoadInstructionReached
            && missingPopResult.Boundary
                == UiShaderSourcePoolResetSecondaryRepeatBoundary.PopResultUnavailableAtCallBoundary);

        var zero = UiShaderSourcePoolResetSecondaryRepeatProjectionRules.Project(
            new UiShaderSourcePoolResetSecondaryRepeatCapture(
                SavedNodePointer: 0x2000,
                ReleaseBackendReturnedNormally: true,
                ReceiverEbx: 0x3000,
                PopResultEax: 0));
        check("A zero pop result falls through to the count-load instruction without importing its value",
            zero.ReleaseBackendArgumentPointer == 0x2000
            && zero.ReceiverArgumentPointer == 0x3008
            && zero.PopCallReached
            && zero.PopResultSaved
            && zero.PopResultStackValue == 0
            && zero.PopResultTestReached
            && zero.PopResultWasZero == true
            && zero.BranchTargetRawFileOffset is null
            && zero.ContinuationRawFileOffset == 0x12CDE01
            && zero.StopRawFileOffset == 0x12CDE01
            && zero.CountLoadInstructionReached
            && !zero.AlignmentEntryReached
            && zero.Boundary
                == UiShaderSourcePoolResetSecondaryRepeatBoundary.ZeroPopResultCountLoad
            && zero.ContinuationRawFileOffset
                != UiShaderSourcePoolResetSecondaryRepeatProjectionRules
                    .InitialSecondaryZeroTargetRawFileOffset);

        var nonzero = UiShaderSourcePoolResetSecondaryRepeatProjectionRules.Project(
            new UiShaderSourcePoolResetSecondaryRepeatCapture(
                SavedNodePointer: 0x4000,
                ReleaseBackendReturnedNormally: true,
                ReceiverEbx: 0x5000,
                PopResultEax: 0xA1B2C3D4));
        check("A nonzero pop result follows JNZ to the alignment entry only",
            nonzero.PopResultSaved
            && nonzero.PopResultStackValue == 0xA1B2C3D4
            && nonzero.PopResultTestReached
            && nonzero.PopResultWasZero == false
            && nonzero.BranchTargetRawFileOffset == 0x12CDD68
            && nonzero.ContinuationRawFileOffset == 0x12CDD68
            && nonzero.StopRawFileOffset == 0x12CDD68
            && nonzero.AlignmentEntryReached
            && !nonzero.CountLoadInstructionReached
            && nonzero.Boundary
                == UiShaderSourcePoolResetSecondaryRepeatBoundary.NonzeroPopResultAlignment);

        check("Unreached receiver and pop captures are rejected",
            ThrowsArgumentException(() =>
                UiShaderSourcePoolResetSecondaryRepeatProjectionRules.Project(
                    new UiShaderSourcePoolResetSecondaryRepeatCapture(
                        SavedNodePointer: 1,
                        ReleaseBackendReturnedNormally: null,
                        ReceiverEbx: 2,
                        PopResultEax: null)))
            && ThrowsArgumentException(() =>
                UiShaderSourcePoolResetSecondaryRepeatProjectionRules.Project(
                    new UiShaderSourcePoolResetSecondaryRepeatCapture(
                        SavedNodePointer: 1,
                        ReleaseBackendReturnedNormally: false,
                        ReceiverEbx: null,
                        PopResultEax: 3)))
            && ThrowsArgumentException(() =>
                UiShaderSourcePoolResetSecondaryRepeatProjectionRules.Project(
                    new UiShaderSourcePoolResetSecondaryRepeatCapture(
                        SavedNodePointer: 1,
                        ReleaseBackendReturnedNormally: true,
                        ReceiverEbx: null,
                        PopResultEax: null)))
            && ThrowsArgumentException(() =>
                UiShaderSourcePoolResetSecondaryRepeatProjectionRules.Project(
                    new UiShaderSourcePoolResetSecondaryRepeatCapture(
                        SavedNodePointer: 1,
                        ReleaseBackendReturnedNormally: true,
                        ReceiverEbx: null,
                        PopResultEax: 4))));
    }

    private static bool HasExactSlice(
        UiShaderSourcePoolResetSecondaryRepeatInstructionEvidence instruction,
        string[] capturedBytes)
    {
        uint captureStart =
            UiShaderSourcePoolResetSecondaryRepeatProjectionRules.RepeatStartRawFileOffset;
        if (instruction.RawFileOffset < captureStart)
        {
            return false;
        }

        int start = checked((int)(instruction.RawFileOffset - captureStart));
        string[] expected =
            instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return start + expected.Length <= capturedBytes.Length
            && capturedBytes.Skip(start).Take(expected.Length).SequenceEqual(expected);
    }

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
