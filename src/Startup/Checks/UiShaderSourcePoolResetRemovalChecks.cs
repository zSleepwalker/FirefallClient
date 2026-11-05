using System.Globalization;
using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolResetRemovalChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Reset removal uses the pinned sample, raw range, mapping, and exact captured bytes",
            UiShaderSourcePoolResetRemovalProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolResetRemovalProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolResetRemovalProjectionRules.ResetRemovalStartRawFileOffset
                == 0x12CDE44
            && UiShaderSourcePoolResetRemovalProjectionRules.ResetRemovalEndRawFileOffsetExclusive
                == 0x12CDE55
            && UiShaderSourcePoolResetRemovalProjectionRules.ResetRemovalEndRawFileOffsetExclusive
                - UiShaderSourcePoolResetRemovalProjectionRules.ResetRemovalStartRawFileOffset
                == 0x11
            && UiShaderSourcePoolResetRemovalProjectionRules.CapturedRemovalBytes
                == "3B C6 74 8B 8B 4E FC 83 C6 FC 89 08 89 75 D0 EB 8B"
            && UiShaderSourcePoolResetRemovalProjectionRules.CapturedRemovalBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x11);

        check("Equal cursor and end branch to the callback boundary before any tail access",
            UiShaderSourcePoolResetRemovalProjectionRules.CursorEndCompareRawFileOffset
                == 0x12CDE44
            && UiShaderSourcePoolResetRemovalProjectionRules.EqualBranchRawFileOffset
                == 0x12CDE46
            && UiShaderSourcePoolResetRemovalProjectionRules.CallbackBoundaryRawFileOffset
                == 0x12CDDD3
            && HasByteSlice(0x12CDE44, 0x12CDE48, "3B C6 74 8B")
            && BranchTarget(0x12CDE46, 0x12CDE47) == 0x12CDDD3);

        check("Unequal path reads the tail DWORD, subtracts four with wrap, stores at cursor, and saves ESI",
            UiShaderSourcePoolResetRemovalProjectionRules.TailDwordLoadRawFileOffset
                == 0x12CDE48
            && UiShaderSourcePoolResetRemovalProjectionRules.EndSubtractFourRawFileOffset
                == 0x12CDE4B
            && UiShaderSourcePoolResetRemovalProjectionRules.CursorStoreRawFileOffset
                == 0x12CDE4E
            && UiShaderSourcePoolResetRemovalProjectionRules.SaveEndToEbp30RawFileOffset
                == 0x12CDE50
            && UiShaderSourcePoolResetRemovalProjectionRules.Ebp30StackDisplacement == -0x30
            && HasByteSlice(0x12CDE48, 0x12CDE4B, "8B 4E FC")
            && HasByteSlice(0x12CDE4B, 0x12CDE4E, "83 C6 FC")
            && HasByteSlice(0x12CDE4E, 0x12CDE50, "89 08")
            && HasByteSlice(0x12CDE50, 0x12CDE53, "89 75 D0"));

        check("Unequal path branches to the index-increment boundary using the signed short displacement",
            UiShaderSourcePoolResetRemovalProjectionRules.IndexIncrementBranchRawFileOffset
                == 0x12CDE53
            && UiShaderSourcePoolResetRemovalProjectionRules.IndexIncrementBoundaryRawFileOffset
                == 0x12CDDE0
            && HasByteSlice(0x12CDE53, 0x12CDE55, "EB 8B")
            && BranchTarget(0x12CDE53, 0x12CDE54) == 0x12CDDE0);

        var equal = UiShaderSourcePoolResetRemovalProjectionRules.Project(
            new UiShaderSourcePoolResetRemovalCapture(
                CursorEax: 0x1000,
                EndEsi: 0x1000,
                TailDword: null));
        check("Equal cursors mark tail capture as unreached and perform no removal-path attempts",
            equal.Boundary == UiShaderSourcePoolResetRemovalBoundary.CallbackBoundary
            && equal.TailCaptureState
                == UiShaderSourcePoolResetRemovalTailCaptureState.Unreached
            && equal.TailDwordReadAddress is null
            && equal.EndAfterRemoval is null
            && !equal.TailDwordLoadAttempted
            && equal.TailDwordValue is null
            && !equal.StoreInstructionReachedByControlFlow
            && equal.StoreDestinationCursor is null
            && equal.StoreValue is null
            && !equal.EndSavedToEbp30
            && equal.BranchTargetRawFileOffset
                == UiShaderSourcePoolResetRemovalProjectionRules.CallbackBoundaryRawFileOffset);

        var wrappedEnd = UiShaderSourcePoolResetRemovalProjectionRules.Project(
            new UiShaderSourcePoolResetRemovalCapture(
                CursorEax: 0x2000,
                EndEsi: 3,
                TailDword: 0x11223344));
        check("Unequal cursors with a reached tail capture wrap end minus four",
            wrappedEnd.Boundary == UiShaderSourcePoolResetRemovalBoundary.IndexIncrementBoundary
            && wrappedEnd.TailCaptureState
                == UiShaderSourcePoolResetRemovalTailCaptureState.Captured
            && wrappedEnd.TailDwordReadAddress == uint.MaxValue
            && wrappedEnd.EndAfterRemoval == uint.MaxValue
            && wrappedEnd.TailDwordLoadAttempted
            && wrappedEnd.TailDwordValue == 0x11223344
            && wrappedEnd.StoreInstructionReachedByControlFlow
            && wrappedEnd.StoreDestinationCursor == 0x2000
            && wrappedEnd.StoreValue == 0x11223344
            && wrappedEnd.EndSavedToEbp30
            && wrappedEnd.BranchTargetRawFileOffset
                == UiShaderSourcePoolResetRemovalProjectionRules.IndexIncrementBoundaryRawFileOffset);

        var capturedTail = UiShaderSourcePoolResetRemovalProjectionRules.Project(
            new UiShaderSourcePoolResetRemovalCapture(
                CursorEax: 0x3000,
                EndEsi: 0x5008,
                TailDword: 0xAABBCCDD));
        check("Captured tail DWORD is projected independently as the attempted cursor-store value",
            capturedTail.TailCaptureState
                == UiShaderSourcePoolResetRemovalTailCaptureState.Captured
            && capturedTail.TailDwordReadAddress == 0x5004
            && capturedTail.EndAfterRemoval == 0x5004
            && capturedTail.TailDwordLoadAttempted
            && capturedTail.TailDwordValue == 0xAABBCCDD
            && capturedTail.StoreInstructionReachedByControlFlow
            && capturedTail.StoreDestinationCursor == 0x3000
            && capturedTail.StoreValue == 0xAABBCCDD
            && capturedTail.EndSavedToEbp30);

        check("A missing tail capture stops at the reached read instead of projecting later instructions",
            ThrowsArgumentException(() =>
                UiShaderSourcePoolResetRemovalProjectionRules.Project(
                    new UiShaderSourcePoolResetRemovalCapture(
                        CursorEax: 0x2000,
                        EndEsi: 3,
                        TailDword: null))));

        check("An equal-cursor input cannot claim a tail capture from the unreached path",
            ThrowsArgumentException(() =>
                UiShaderSourcePoolResetRemovalProjectionRules.Project(
                    new UiShaderSourcePoolResetRemovalCapture(
                        CursorEax: 4,
                        EndEsi: 4,
                        TailDword: 0x11223344))));
    }

    private static uint BranchTarget(uint instructionRawOffset, uint displacementRawOffset)
    {
        string[] bytes =
            UiShaderSourcePoolResetRemovalProjectionRules.CapturedRemovalBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        uint captureStart =
            UiShaderSourcePoolResetRemovalProjectionRules.ResetRemovalStartRawFileOffset;
        int displacementIndex = checked((int)(displacementRawOffset - captureStart));
        int displacement = byte.Parse(
            bytes[displacementIndex],
            NumberStyles.HexNumber,
            CultureInfo.InvariantCulture);
        if (displacement >= 0x80)
        {
            displacement -= 0x100;
        }

        return unchecked((uint)((int)instructionRawOffset + 2 + displacement));
    }

    private static bool HasByteSlice(
        uint rawStart,
        uint rawEndExclusive,
        string expected)
    {
        string[] bytes =
            UiShaderSourcePoolResetRemovalProjectionRules.CapturedRemovalBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        uint captureStart =
            UiShaderSourcePoolResetRemovalProjectionRules.ResetRemovalStartRawFileOffset;
        if (rawStart < captureStart || rawEndExclusive < rawStart)
        {
            return false;
        }

        int start = checked((int)(rawStart - captureStart));
        int length = checked((int)(rawEndExclusive - rawStart));
        return start + length <= bytes.Length
            && string.Join(" ", bytes.Skip(start).Take(length)) == expected;
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
