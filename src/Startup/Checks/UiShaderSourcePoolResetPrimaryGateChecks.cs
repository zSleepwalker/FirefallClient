using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolResetPrimaryGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Primary reset-gate sample identity, raw bounds, stack offsets, and exact byte captures match",
            UiShaderSourcePoolResetPrimaryGateProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.ResetFunctionRawFileOffset
                == 0x12CDC50
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.InitialGateStartRawFileOffset
                == 0x12CDCC2
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.InitialGateEndRawFileOffsetExclusive
                == 0x12CDCE3
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.RepeatGateStartRawFileOffset
                == 0x12CDD39
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.RepeatGateEndRawFileOffsetExclusive
                == 0x12CDD4D
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.DescriptorBeginStackOffset == -0x34
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.DescriptorEndStackOffset == -0x30
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.DescriptorStorageEndStackOffset
                == -0x2C
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.CapturedInitialGateBytes
                == "33 F6 C7 45 CC 00 00 00 00 33 DB 89 75 D0 89 5D D4 "
                    + "8B CF 89 5D FC E8 03 FE FF FF 8B F8 85 FF 74 6A"
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.CapturedRepeatGateBytes
                == "8B 4D E4 83 C6 04 89 75 D0 E8 99 FD FF FF 8B F8 "
                    + "85 FF 75 96"
            && ByteCount(UiShaderSourcePoolResetPrimaryGateProjectionRules.CapturedInitialGateBytes)
                == 0x21
            && ByteCount(UiShaderSourcePoolResetPrimaryGateProjectionRules.CapturedRepeatGateBytes)
                == 0x14);

        check("Initial and repeat pop calls resolve to the same helper and exact branch destinations",
            UiShaderSourcePoolResetPrimaryGateProjectionRules.PopCallTargetRawFileOffset
                == 0x12CDAE0
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.PopCallTargetVirtualAddress
                == 0x16CE6E0
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.ResolveRelativeCallTargetRawFileOffset(
                UiShaderSourcePoolResetPrimaryGateProjectionRules.InitialPopCallRawFileOffset,
                -0x1FD) == UiShaderSourcePoolResetPrimaryGateProjectionRules.PopCallTargetRawFileOffset
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.ResolveRelativeCallTargetRawFileOffset(
                UiShaderSourcePoolResetPrimaryGateProjectionRules.RepeatPopCallRawFileOffset,
                -0x267) == UiShaderSourcePoolResetPrimaryGateProjectionRules.PopCallTargetRawFileOffset
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.ResolveShortBranchTargetRawFileOffset(
                UiShaderSourcePoolResetPrimaryGateProjectionRules.InitialZeroResultBranchRawFileOffset,
                0x6A) == UiShaderSourcePoolResetPrimaryGateProjectionRules.SecondaryGateContinuationRawFileOffset
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.ResolveShortBranchTargetRawFileOffset(
                UiShaderSourcePoolResetPrimaryGateProjectionRules.RepeatNonzeroResultBranchRawFileOffset,
                unchecked((sbyte)0x96))
                == UiShaderSourcePoolResetPrimaryGateProjectionRules.AppendContinuationRawFileOffset
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.SecondaryGateContinuationRawFileOffset
                == 0x12CDD4D
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.AppendContinuationRawFileOffset
                == 0x12CDCE3);

        check("Instruction evidence anchors are byte-exact and repeat advancement remains append-owned",
            UiShaderSourcePoolResetPrimaryGateProjectionRules.Instructions.Count == 13
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.Instructions
                .Where(instruction =>
                    instruction.RawFileOffset >= UiShaderSourcePoolResetPrimaryGateProjectionRules.InitialGateStartRawFileOffset
                    && instruction.RawFileOffset < UiShaderSourcePoolResetPrimaryGateProjectionRules.InitialGateEndRawFileOffsetExclusive)
                .All(instruction => HasByteSlice(
                    UiShaderSourcePoolResetPrimaryGateProjectionRules.CapturedInitialGateBytes,
                    UiShaderSourcePoolResetPrimaryGateProjectionRules.InitialGateStartRawFileOffset,
                    instruction.RawFileOffset,
                    instruction.Bytes))
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.Instructions
                .Where(instruction =>
                    instruction.RawFileOffset >= UiShaderSourcePoolResetPrimaryGateProjectionRules.RepeatGateStartRawFileOffset
                    && instruction.RawFileOffset < UiShaderSourcePoolResetPrimaryGateProjectionRules.RepeatGateEndRawFileOffsetExclusive)
                .All(instruction => HasByteSlice(
                    UiShaderSourcePoolResetPrimaryGateProjectionRules.CapturedRepeatGateBytes,
                    UiShaderSourcePoolResetPrimaryGateProjectionRules.RepeatGateStartRawFileOffset,
                    instruction.RawFileOffset,
                    instruction.Bytes))
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.AppendOwnedRepeatEndAdvanceRawFileOffset
                == UiShaderSourcePoolResetAppendProjectionRules.EndPointerAdvanceRawFileOffset
            && UiShaderSourcePoolResetPrimaryGateProjectionRules.AppendOwnedRepeatEndStoreRawFileOffset
                == UiShaderSourcePoolResetAppendProjectionRules.EndPointerStoreRawFileOffset);

        var initialZero = UiShaderSourcePoolResetPrimaryGateProjectionRules.Project(
            new UiShaderSourcePoolResetPrimaryGateCapture(
                UiShaderSourcePoolResetPrimaryGateMode.Initial,
                ReceiverPointer: 0x12345678,
                PopResultPointer: 0));
        check("Initial zero result preserves its receiver and reaches only the secondary gate after zeroing descriptor locals",
            initialZero.Capture.ReceiverPointer == 0x12345678
            && initialZero.Capture.PopResultPointer == 0
            && initialZero.DescriptorZeroingReached
            && initialZero.DescriptorBeginPointer == 0
            && initialZero.DescriptorEndPointer == 0
            && initialZero.DescriptorStorageEndPointer == 0
            && initialZero.Boundary == UiShaderSourcePoolResetPrimaryGateBoundary.SecondaryGate
            && initialZero.ContinuationRawFileOffset
                == UiShaderSourcePoolResetPrimaryGateProjectionRules.SecondaryGateContinuationRawFileOffset);

        var initialNonzero = UiShaderSourcePoolResetPrimaryGateProjectionRules.Project(
            new UiShaderSourcePoolResetPrimaryGateCapture(
                UiShaderSourcePoolResetPrimaryGateMode.Initial,
                ReceiverPointer: 0,
                PopResultPointer: 0xAABBCCDD));
        check("Initial nonzero result uses the captured result and selects append without receiver fallback",
            initialNonzero.Capture.ReceiverPointer == 0
            && initialNonzero.Capture.PopResultPointer == 0xAABBCCDD
            && initialNonzero.DescriptorZeroingReached
            && initialNonzero.Boundary == UiShaderSourcePoolResetPrimaryGateBoundary.AppendBranch
            && initialNonzero.ContinuationRawFileOffset
                == UiShaderSourcePoolResetPrimaryGateProjectionRules.AppendContinuationRawFileOffset);

        var repeatZero = UiShaderSourcePoolResetPrimaryGateProjectionRules.Project(
            new UiShaderSourcePoolResetPrimaryGateCapture(
                UiShaderSourcePoolResetPrimaryGateMode.Repeat,
                ReceiverPointer: 0x87654321,
                PopResultPointer: 0));
        var repeatNonzero = UiShaderSourcePoolResetPrimaryGateProjectionRules.Project(
            new UiShaderSourcePoolResetPrimaryGateCapture(
                UiShaderSourcePoolResetPrimaryGateMode.Repeat,
                ReceiverPointer: 0x87654321,
                PopResultPointer: 0x11223344));
        check("Repeat zero reaches the secondary gate and repeat nonzero returns to append without re-zeroing locals",
            !repeatZero.DescriptorZeroingReached
            && repeatZero.DescriptorBeginPointer is null
            && repeatZero.DescriptorEndPointer is null
            && repeatZero.DescriptorStorageEndPointer is null
            && repeatZero.Boundary == UiShaderSourcePoolResetPrimaryGateBoundary.SecondaryGate
            && repeatZero.ContinuationRawFileOffset
                == UiShaderSourcePoolResetPrimaryGateProjectionRules.SecondaryGateContinuationRawFileOffset
            && !repeatNonzero.DescriptorZeroingReached
            && repeatNonzero.DescriptorBeginPointer is null
            && repeatNonzero.DescriptorEndPointer is null
            && repeatNonzero.DescriptorStorageEndPointer is null
            && repeatNonzero.Capture.ReceiverPointer == 0x87654321
            && repeatNonzero.Capture.PopResultPointer == 0x11223344
            && repeatNonzero.Boundary == UiShaderSourcePoolResetPrimaryGateBoundary.AppendBranch
            && repeatNonzero.ContinuationRawFileOffset
                == UiShaderSourcePoolResetPrimaryGateProjectionRules.AppendContinuationRawFileOffset);

        var initialCallBoundary = UiShaderSourcePoolResetPrimaryGateProjectionRules.Project(
            new UiShaderSourcePoolResetPrimaryGateCapture(
                UiShaderSourcePoolResetPrimaryGateMode.Initial,
                ReceiverPointer: 0x12345678,
                PopResultPointer: null));
        var repeatCallBoundary = UiShaderSourcePoolResetPrimaryGateProjectionRules.Project(
            new UiShaderSourcePoolResetPrimaryGateCapture(
                UiShaderSourcePoolResetPrimaryGateMode.Repeat,
                ReceiverPointer: 0x87654321,
                PopResultPointer: null));
        check("Missing reached pop-result captures stop at the call boundary without fallback or traversal",
            initialCallBoundary.Boundary == UiShaderSourcePoolResetPrimaryGateBoundary.PopCallBoundary
            && initialCallBoundary.ContinuationRawFileOffset is null
            && initialCallBoundary.DescriptorZeroingReached
            && initialCallBoundary.DescriptorBeginPointer == 0
            && repeatCallBoundary.Boundary == UiShaderSourcePoolResetPrimaryGateBoundary.PopCallBoundary
            && repeatCallBoundary.ContinuationRawFileOffset is null
            && !repeatCallBoundary.DescriptorZeroingReached
            && repeatCallBoundary.DescriptorBeginPointer is null);

        check("Unknown gate modes are rejected",
            ThrowsArgumentOutOfRangeException(() =>
                UiShaderSourcePoolResetPrimaryGateProjectionRules.Project(
                    new UiShaderSourcePoolResetPrimaryGateCapture(
                        (UiShaderSourcePoolResetPrimaryGateMode)99,
                        ReceiverPointer: 0,
                        PopResultPointer: null))));
    }

    private static int ByteCount(string bytes) =>
        bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

    private static bool HasByteSlice(
        string capture,
        uint captureStartRawFileOffset,
        uint instructionRawFileOffset,
        string expectedBytes)
    {
        string captured = string.Concat(capture.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        string expected = string.Concat(expectedBytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        int start = checked((int)(instructionRawFileOffset - captureStartRawFileOffset) * 2);
        return start >= 0
            && start + expected.Length <= captured.Length
            && captured.Substring(start, expected.Length) == expected;
    }

    private static bool ThrowsArgumentOutOfRangeException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }
}
