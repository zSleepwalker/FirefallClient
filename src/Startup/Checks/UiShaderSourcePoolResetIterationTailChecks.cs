using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolResetIterationTailChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Iteration-tail sample, exact raw bytes, entry points, branch targets, and code VAs are pinned",
            UiShaderSourcePoolResetIterationTailProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolResetIterationTailProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolResetIterationTailProjectionRules.FunctionRawFileOffset == 0x12CDC50
            && UiShaderSourcePoolResetIterationTailProjectionRules.CallbackReturnedEntryRawFileOffset
                == 0x12CDDD7
            && UiShaderSourcePoolResetIterationTailProjectionRules.RemovalCompletedEntryRawFileOffset
                == 0x12CDDE0
            && UiShaderSourcePoolResetIterationTailProjectionRules.TailStartRawFileOffset == 0x12CDDD7
            && UiShaderSourcePoolResetIterationTailProjectionRules.TailEndRawFileOffsetExclusive
                == 0x12CDDE6
            && UiShaderSourcePoolResetIterationTailProjectionRules.TailStartVirtualAddress == 0x016CE9D7
            && UiShaderSourcePoolResetIterationTailProjectionRules.TailEndVirtualAddressExclusive
                == 0x016CE9E6
            && UiShaderSourcePoolResetIterationTailProjectionRules.NextIterationRawFileOffset == 0x12CDDB2
            && UiShaderSourcePoolResetIterationTailProjectionRules.NextIterationVirtualAddress == 0x16CE9B2
            && UiShaderSourcePoolResetIterationTailProjectionRules.ReleaseBoundaryRawFileOffset == 0x12CDDE6
            && UiShaderSourcePoolResetIterationTailProjectionRules.ReleaseBoundaryVirtualAddress == 0x16CE9E6
            && UiShaderSourcePoolResetIterationTailProjectionRules.TailStartRawFileOffset
                + UiShaderSourcePoolResetIterationTailProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolResetIterationTailProjectionRules.TailStartVirtualAddress
            && UiShaderSourcePoolResetIterationTailProjectionRules.TailEndRawFileOffsetExclusive
                + UiShaderSourcePoolResetIterationTailProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolResetIterationTailProjectionRules.TailEndVirtualAddressExclusive
            && UiShaderSourcePoolResetIterationTailProjectionRules.NextIterationRawFileOffset
                + UiShaderSourcePoolResetIterationTailProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolResetIterationTailProjectionRules.NextIterationVirtualAddress
            && UiShaderSourcePoolResetIterationTailProjectionRules.ReleaseBoundaryRawFileOffset
                + UiShaderSourcePoolResetIterationTailProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolResetIterationTailProjectionRules.ReleaseBoundaryVirtualAddress
            && UiShaderSourcePoolResetIterationTailProjectionRules.TailEndRawFileOffsetExclusive
                - UiShaderSourcePoolResetIterationTailProjectionRules.TailStartRawFileOffset
                == 0x0F
            && UiShaderSourcePoolResetIterationTailProjectionRules.CapturedTailBytes
                == "8B 55 CC 83 C4 04 FF 45 E8 47 3B 7B 20 72 CC"
            && UiShaderSourcePoolResetIterationTailProjectionRules.CapturedTailBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x0F
            && UiShaderSourcePoolResetIterationTailProjectionRules.TailStartRawFileOffset
                + (uint)UiShaderSourcePoolResetIterationTailProjectionRules.CapturedTailBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                == UiShaderSourcePoolResetIterationTailProjectionRules.TailEndRawFileOffsetExclusive);

        check("Callback-return tail instruction bytes, stack locals, and unsigned JB target match the capture",
            UiShaderSourcePoolResetIterationTailProjectionRules.BeginReloadRawFileOffset == 0x12CDDD7
            && UiShaderSourcePoolResetIterationTailProjectionRules.StackCleanupRawFileOffset == 0x12CDDDA
            && UiShaderSourcePoolResetIterationTailProjectionRules.CallbackCountIncrementRawFileOffset
                == 0x12CDDDD
            && UiShaderSourcePoolResetIterationTailProjectionRules.IndexIncrementRawFileOffset
                == 0x12CDDE0
            && UiShaderSourcePoolResetIterationTailProjectionRules.BoundCompareRawFileOffset == 0x12CDDE1
            && UiShaderSourcePoolResetIterationTailProjectionRules.UnsignedBelowBranchRawFileOffset
                == 0x12CDDE4
            && UiShaderSourcePoolResetIterationTailProjectionRules.ReloadedBeginStackOffset == -0x34
            && UiShaderSourcePoolResetIterationTailProjectionRules.CallbackCountStackOffset == -0x18
            && UiShaderSourcePoolResetIterationTailProjectionRules.LoopBoundThisOffset == 0x20
            && UiShaderSourcePoolResetIterationTailProjectionRules.StackCleanupBytes == 4
            && HasByteSlice(
                UiShaderSourcePoolResetIterationTailProjectionRules.BeginReloadRawFileOffset,
                UiShaderSourcePoolResetIterationTailProjectionRules.BeginReloadRawFileOffset + 3u,
                "8B 55 CC")
            && HasByteSlice(
                UiShaderSourcePoolResetIterationTailProjectionRules.StackCleanupRawFileOffset,
                UiShaderSourcePoolResetIterationTailProjectionRules.StackCleanupRawFileOffset + 3u,
                "83 C4 04")
            && HasByteSlice(
                UiShaderSourcePoolResetIterationTailProjectionRules.CallbackCountIncrementRawFileOffset,
                UiShaderSourcePoolResetIterationTailProjectionRules.CallbackCountIncrementRawFileOffset + 3u,
                "FF 45 E8")
            && HasByteSlice(
                UiShaderSourcePoolResetIterationTailProjectionRules.IndexIncrementRawFileOffset,
                UiShaderSourcePoolResetIterationTailProjectionRules.IndexIncrementRawFileOffset + 1u,
                "47")
            && HasByteSlice(
                UiShaderSourcePoolResetIterationTailProjectionRules.BoundCompareRawFileOffset,
                UiShaderSourcePoolResetIterationTailProjectionRules.BoundCompareRawFileOffset + 3u,
                "3B 7B 20")
            && HasByteSlice(
                UiShaderSourcePoolResetIterationTailProjectionRules.UnsignedBelowBranchRawFileOffset,
                UiShaderSourcePoolResetIterationTailProjectionRules.UnsignedBelowBranchRawFileOffset + 2u,
                "72 CC")
            && unchecked(
                    UiShaderSourcePoolResetIterationTailProjectionRules.UnsignedBelowBranchRawFileOffset
                    + 2u
                    + 0xFFFFFFCCu)
                == UiShaderSourcePoolResetIterationTailProjectionRules.NextIterationRawFileOffset
            && UiShaderSourcePoolResetIterationTailProjectionRules.UnsignedBelowBranchRawFileOffset
                + 2
                == UiShaderSourcePoolResetIterationTailProjectionRules.ReleaseBoundaryRawFileOffset);

        var callbackReturned = UiShaderSourcePoolResetIterationTailProjectionRules.Project(
            new UiShaderSourcePoolResetIterationTailCapture(
                UiShaderSourcePoolResetIterationTailEntryMode.CallbackReturned,
                CallbackCountBeforeIncrement: uint.MaxValue,
                IndexBeforeIncrement: 0x7FFFFFFF,
                LoopBound: uint.MaxValue,
                ReloadedBeginPointer: 0x11223344));
        check("Callback return independently captures begin/count/index/bound and wraps DWORD count and index",
            callbackReturned.BeginReloadInstructionReached
            && callbackReturned.ReloadedBeginPointer == 0x11223344
            && callbackReturned.CallbackCountIncrementInstructionReached
            && callbackReturned.CallbackCountAfterIncrement == 0
            && callbackReturned.IndexAfterIncrement == 0x80000000
            && callbackReturned.UnsignedBelowBound
            && callbackReturned.Outcome == UiShaderSourcePoolResetIterationTailOutcome.NextIteration
            && callbackReturned.BranchTargetRawFileOffset == 0x12CDDB2
            && callbackReturned.BranchTargetVirtualAddress == 0x16CE9B2);

        var removalCompleted = UiShaderSourcePoolResetIterationTailProjectionRules.Project(
            new UiShaderSourcePoolResetIterationTailCapture(
                UiShaderSourcePoolResetIterationTailEntryMode.RemovalCompleted,
                CallbackCountBeforeIncrement: null,
                IndexBeforeIncrement: 0xFFFFFFFE,
                LoopBound: uint.MaxValue,
                ReloadedBeginPointer: null));
        check("Removal entry skips begin reload and callback-count increment but still wraps, compares, and reaches release boundary",
            !removalCompleted.BeginReloadInstructionReached
            && removalCompleted.ReloadedBeginPointer is null
            && !removalCompleted.CallbackCountIncrementInstructionReached
            && removalCompleted.CallbackCountAfterIncrement is null
            && removalCompleted.IndexAfterIncrement == uint.MaxValue
            && !removalCompleted.UnsignedBelowBound
            && removalCompleted.Outcome == UiShaderSourcePoolResetIterationTailOutcome.ReleaseBoundary
            && removalCompleted.BranchTargetRawFileOffset
                == UiShaderSourcePoolResetIterationTailProjectionRules.ReleaseBoundaryRawFileOffset
            && removalCompleted.BranchTargetVirtualAddress == 0x16CE9E6);

        check("Only reached callback-return captures are accepted for either entry mode",
            ThrowsArgumentException(() =>
                UiShaderSourcePoolResetIterationTailProjectionRules.Project(
                    new UiShaderSourcePoolResetIterationTailCapture(
                        UiShaderSourcePoolResetIterationTailEntryMode.CallbackReturned,
                        CallbackCountBeforeIncrement: null,
                        IndexBeforeIncrement: 0,
                        LoopBound: 1,
                        ReloadedBeginPointer: 0x1000)))
            && ThrowsArgumentException(() =>
                UiShaderSourcePoolResetIterationTailProjectionRules.Project(
                    new UiShaderSourcePoolResetIterationTailCapture(
                        UiShaderSourcePoolResetIterationTailEntryMode.CallbackReturned,
                        CallbackCountBeforeIncrement: 4,
                        IndexBeforeIncrement: 0,
                        LoopBound: 1,
                        ReloadedBeginPointer: null)))
            && ThrowsArgumentException(() =>
                UiShaderSourcePoolResetIterationTailProjectionRules.Project(
                    new UiShaderSourcePoolResetIterationTailCapture(
                        UiShaderSourcePoolResetIterationTailEntryMode.RemovalCompleted,
                        CallbackCountBeforeIncrement: 4,
                        IndexBeforeIncrement: 0,
                        LoopBound: 1,
                        ReloadedBeginPointer: null)))
            && ThrowsArgumentException(() =>
                UiShaderSourcePoolResetIterationTailProjectionRules.Project(
                    new UiShaderSourcePoolResetIterationTailCapture(
                        UiShaderSourcePoolResetIterationTailEntryMode.RemovalCompleted,
                        CallbackCountBeforeIncrement: null,
                        IndexBeforeIncrement: 0,
                        LoopBound: 1,
                        ReloadedBeginPointer: 0x1000))));
    }

    private static bool HasByteSlice(uint rawStart, uint rawEndExclusive, string expected)
    {
        string[] bytes =
            UiShaderSourcePoolResetIterationTailProjectionRules.CapturedTailBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        uint captureStart = UiShaderSourcePoolResetIterationTailProjectionRules.TailStartRawFileOffset;
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
