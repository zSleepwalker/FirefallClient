using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolResetSecondaryGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        string[] capturedBytes =
            UiShaderSourcePoolResetSecondaryGateProjectionRules.CapturedGateBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        check("Secondary gate sample identity, raw range, and code-VA mapping are pinned",
            UiShaderSourcePoolResetSecondaryGateProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.ResetFunctionRawFileOffset
                == 0x12CDC50
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.GateStartRawFileOffset
                == 0x12CDD4D
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.GateEndRawFileOffsetExclusive
                == 0x12CDD68
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.GateStartCodeVirtualAddress
                == 0x16CE94D
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.GateEndCodeVirtualAddressExclusive
                == 0x16CE968
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.RawFileOffsetToCodeVirtualAddress(
                UiShaderSourcePoolResetSecondaryGateProjectionRules.GateStartRawFileOffset)
                == 0x16CE94D
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.RawFileOffsetToCodeVirtualAddress(
                UiShaderSourcePoolResetSecondaryGateProjectionRules.GateEndRawFileOffsetExclusive)
                == 0x16CE968
            && capturedBytes.Length == 0x1B
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.CapturedGateBytes
                == "8B 5D E4 33 FF 89 7D E8 8D 4B 08 E8 83 FD FF FF "
                    + "89 45 E0 85 C0 0F 84 9C 00 00 00"
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.Instructions
                .All(instruction => HasExactSlice(instruction, capturedBytes)));

        check("The direct pop call and conditional zero target match their encoded displacements",
            UiShaderSourcePoolResetSecondaryGateProjectionRules.PopCallRawFileOffset == 0x12CDD58
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.PopCallTargetRawFileOffset
                == 0x12CDAE0
            && unchecked(
                    UiShaderSourcePoolResetSecondaryGateProjectionRules.PopCallRawFileOffset
                    + 5u
                    + 0xFFFFFD83u)
                == UiShaderSourcePoolResetSecondaryGateProjectionRules.PopCallTargetRawFileOffset
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.PopResultZeroBranchRawFileOffset
                == 0x12CDD62
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.PopResultZeroBranchTargetRawFileOffset
                == 0x12CDE04
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.PopResultZeroBranchRawFileOffset
                + 6 + 0x9C
                == UiShaderSourcePoolResetSecondaryGateProjectionRules.PopResultZeroBranchTargetRawFileOffset
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.AlignmentStartRawFileOffset
                == 0x12CDD68
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.AlignmentEntryBytes == "8B 4B 18"
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.FinalizationCountLoadRawFileOffset
                == 0x12CDE01
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.FinalizationCountLoadBytes
                == "8B 7D E8"
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.FinalizationPointerLoadRawFileOffset
                == 0x12CDE04
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.FinalizationPointerLoadBytes
                == "8B 4D D8"
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.PopResultZeroBranchTargetRawFileOffset
                == UiShaderSourcePoolResetSecondaryGateProjectionRules.FinalizationCountLoadRawFileOffset
                    + 3);

        check("A missing pop-result capture stops at the call without taking either success branch",
            UiShaderSourcePoolResetSecondaryGateProjectionRules.SavedReceiverStackOffset == -0x1C
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.LocalCountStackOffset == -0x18
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.SavedPopResultStackOffset == -0x20
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.ReceiverArgumentOffset == 8
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.InitialReturnedCount == 0
            && UiShaderSourcePoolResetSecondaryGateProjectionRules.InitialLocalCount == 0
            && MissingPopResultStopsAtCallBoundary());

        var empty = UiShaderSourcePoolResetSecondaryGateProjectionRules.Project(
            new UiShaderSourcePoolResetSecondaryGateCapture(
                ReceiverPointer: 0x1000,
                PopResultEax: 0));
        check("A zero pop result retains EDI and the local count at zero, then targets the finalization pointer load",
            empty.Capture.ReceiverPointer == 0x1000
            && empty.ReceiverArgumentPointer == 0x1008
            && empty.ReturnedCountEdiAtBoundary == 0
            && empty.LocalCountAtBoundary == 0
            && empty.PopCallReached
            && empty.PopResultSaved
            && empty.PopResultStackValue == 0
            && empty.PopResultTestReached
            && empty.PopResultWasZero == true
            && empty.BranchTargetRawFileOffset == 0x12CDE04
            && empty.StopRawFileOffset == 0x12CDE04
            && empty.FinalizationCountLoadSkipped
            && empty.Boundary
                == UiShaderSourcePoolResetSecondaryGateBoundary.ZeroPopResultFinalizationPointerLoad);

        var nonempty = UiShaderSourcePoolResetSecondaryGateProjectionRules.Project(
            new UiShaderSourcePoolResetSecondaryGateCapture(
                ReceiverPointer: uint.MaxValue,
                PopResultEax: 0x12345678));
        check("A nonzero result preserves the pop capture and stops at the alignment entry",
            nonempty.Capture.ReceiverPointer == uint.MaxValue
            && nonempty.ReceiverArgumentPointer == 7
            && nonempty.ReturnedCountEdiAtBoundary == 0
            && nonempty.LocalCountAtBoundary == 0
            && nonempty.PopCallReached
            && nonempty.PopResultSaved
            && nonempty.PopResultStackValue == 0x12345678
            && nonempty.PopResultTestReached
            && nonempty.PopResultWasZero == false
            && nonempty.BranchTargetRawFileOffset == 0x12CDD68
            && nonempty.StopRawFileOffset == 0x12CDD68
            && !nonempty.FinalizationCountLoadSkipped
            && nonempty.Boundary
                == UiShaderSourcePoolResetSecondaryGateBoundary.NonzeroPopResultAlignment);
    }

    private static bool MissingPopResultStopsAtCallBoundary()
    {
        var missing = UiShaderSourcePoolResetSecondaryGateProjectionRules.Project(
            new UiShaderSourcePoolResetSecondaryGateCapture(
                ReceiverPointer: 0xFFFFFFFC,
                PopResultEax: null));

        return missing.Capture.ReceiverPointer == 0xFFFFFFFC
            && missing.ReceiverArgumentPointer == 4
            && missing.ReturnedCountEdiAtBoundary == 0
            && missing.LocalCountAtBoundary == 0
            && missing.PopCallReached
            && !missing.PopResultSaved
            && missing.PopResultStackValue is null
            && !missing.PopResultTestReached
            && missing.PopResultWasZero is null
            && missing.BranchTargetRawFileOffset is null
            && missing.StopRawFileOffset == 0x12CDD58
            && !missing.FinalizationCountLoadSkipped
            && missing.Boundary
                == UiShaderSourcePoolResetSecondaryGateBoundary.PopResultUnavailableAtCallBoundary;
    }

    private static bool HasExactSlice(
        UiShaderSourcePoolResetSecondaryGateInstructionEvidence instruction,
        string[] capturedBytes)
    {
        uint captureStart =
            UiShaderSourcePoolResetSecondaryGateProjectionRules.GateStartRawFileOffset;
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
}
