using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolResetFinalizationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Finalization sample identity, raw interval, code-VA mapping, and exact bytes are pinned",
            UiShaderSourcePoolResetFinalizationProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolResetFinalizationProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolResetFinalizationProjectionRules.FunctionRawFileOffset
                == 0x12CDC50
            && UiShaderSourcePoolResetFinalizationProjectionRules.FinalizationStartRawFileOffset
                == 0x12CDE01
            && UiShaderSourcePoolResetFinalizationProjectionRules.FinalizationEndRawFileOffsetExclusive
                == 0x12CDE44
            && UiShaderSourcePoolResetFinalizationProjectionRules.FinalizationCodeVirtualAddress
                == 0x16CEA01
            && UiShaderSourcePoolResetFinalizationProjectionRules.CapturedFinalizationBytes
                == "8B 7D E8 8B 4D D8 8D 45 DC C7 45 DC 00 00 00 00 "
                    + "3B C1 74 0B 0F AE F8 0F AE E8 8B 45 DC 89 01 8B "
                    + "01 8B 4D CC 85 C9 74 05 E8 32 AF FF FF 8B C7 8B "
                    + "4D F4 64 89 0D 00 00 00 00 59 5F 5E 5B 8B E5 5D "
                    + "C2 04 00"
            && UiShaderSourcePoolResetFinalizationProjectionRules.CapturedFinalizationBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x43
            && UiShaderSourcePoolResetFinalizationProjectionRules.FinalizationStartRawFileOffset
                + (uint)UiShaderSourcePoolResetFinalizationProjectionRules.CapturedFinalizationBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                == UiShaderSourcePoolResetFinalizationProjectionRules.FinalizationEndRawFileOffsetExclusive);

        check("Alias gating, ordered fences, zero-store/readback, tempbegin gate, and RET 4 targets match bytes",
            UiShaderSourcePoolResetFinalizationProjectionRules.ReturnedCountStackOffset == -0x18
            && UiShaderSourcePoolResetFinalizationProjectionRules.PoolCounterPointerStackOffset == -0x28
            && UiShaderSourcePoolResetFinalizationProjectionRules.LocalZeroStackOffset == -0x24
            && UiShaderSourcePoolResetFinalizationProjectionRules.TempBeginStackOffset == -0x34
            && UiShaderSourcePoolResetFinalizationProjectionRules.SavedSehChainStackOffset == -0x0C
            && UiShaderSourcePoolResetFinalizationProjectionRules.AliasBranchRawFileOffset
                + 2 + 0x0B
                == UiShaderSourcePoolResetFinalizationProjectionRules.AliasBranchTargetRawFileOffset
            && UiShaderSourcePoolResetFinalizationProjectionRules.TempBeginNullBranchRawFileOffset
                + 2 + 0x05
                == UiShaderSourcePoolResetFinalizationProjectionRules.TempBeginNullBranchTargetRawFileOffset
            && unchecked(
                    UiShaderSourcePoolResetFinalizationProjectionRules.ReleaseHelperCallRawFileOffset
                    + 5u
                    + 0xFFFFAF32u)
                == UiShaderSourcePoolResetFinalizationProjectionRules.ReleaseHelperTargetRawFileOffset
            && UiShaderSourcePoolResetFinalizationProjectionRules.NormalReturnRawFileOffset
                == 0x12CDE41
            && UiShaderSourcePoolResetFinalizationProjectionRules.NormalReturnStackPopBytes == 4
            && UiShaderSourcePoolResetFinalizationProjectionRules.Instructions.All(HasExactSlice));

        var nonAlias = UiShaderSourcePoolResetFinalizationProjectionRules.Project(
            new UiShaderSourcePoolResetFinalizationCapture(
                ReturnedCountEdi: 7,
                ReceiverPointer: 0x11110000,
                TempBeginPointer: 0x22220000,
                PoolCounterPointer: 0x33330010,
                LocalZeroAddress: 0x44440024,
                PoolCounterReadbackValue: 0,
                ReleaseHelperReturnedNormally: true));
        check("Non-alias finalization records actual addresses, stores zero, captures readback, and returns EDI",
            nonAlias.Capture.ReturnedCountEdi == 7
            && nonAlias.Capture.ReceiverPointer == 0x11110000
            && nonAlias.Capture.TempBeginPointer == 0x22220000
            && nonAlias.Capture.PoolCounterPointer == 0x33330010
            && nonAlias.Capture.LocalZeroAddress == 0x44440024
            && nonAlias.LocalZeroInitialized
            && nonAlias.StoreFenceReached
            && nonAlias.LoadFenceReached
            && nonAlias.PoolCounterZeroStoreReached
            && nonAlias.PoolCounterZeroStoreValue == 0
            && nonAlias.PoolCounterReadbackReached
            && nonAlias.PoolCounterReadbackValue == 0
            && nonAlias.ReleaseHelperCallReached
            && nonAlias.Boundary == UiShaderSourcePoolResetFinalizationBoundary.NormalReturn
            && nonAlias.SehChainRestored
            && nonAlias.ReturnedEax == 7
            && nonAlias.ReturnRawFileOffset
                == UiShaderSourcePoolResetFinalizationProjectionRules.NormalReturnRawFileOffset
            && nonAlias.ReturnStackPopBytes == 4);

        var aliased = UiShaderSourcePoolResetFinalizationProjectionRules.Project(
            new UiShaderSourcePoolResetFinalizationCapture(
                ReturnedCountEdi: 0xA5,
                ReceiverPointer: 0x1000,
                TempBeginPointer: 0,
                PoolCounterPointer: 0x5000,
                LocalZeroAddress: 0x5000,
                PoolCounterReadbackValue: 0xBEEF));
        check("Equal captured addresses skip fences/store without assuming the readback is zero",
            aliased.Capture.ReceiverPointer == 0x1000
            && aliased.Capture.PoolCounterPointer == aliased.Capture.LocalZeroAddress
            && aliased.LocalZeroInitialized
            && !aliased.StoreFenceReached
            && !aliased.LoadFenceReached
            && !aliased.PoolCounterZeroStoreReached
            && aliased.PoolCounterZeroStoreValue is null
            && aliased.PoolCounterReadbackReached
            && aliased.PoolCounterReadbackValue == 0xBEEF
            && !aliased.ReleaseHelperCallReached
            && aliased.Boundary == UiShaderSourcePoolResetFinalizationBoundary.NormalReturn
            && aliased.SehChainRestored
            && aliased.ReturnedEax == 0xA5
            && aliased.ReturnRawFileOffset == 0x12CDE41
            && aliased.ReturnStackPopBytes == 4);

        var nonreturningHelper = UiShaderSourcePoolResetFinalizationProjectionRules.Project(
            new UiShaderSourcePoolResetFinalizationCapture(
                ReturnedCountEdi: 13,
                ReceiverPointer: 0x7100,
                TempBeginPointer: 0x7200,
                PoolCounterPointer: 0x7300,
                LocalZeroAddress: 0x7400,
                PoolCounterReadbackValue: 0,
                ReleaseHelperReturnedNormally: false));
        check("A reached nonreturning helper stops before EAX count, SEH restore, and RET 4",
            nonreturningHelper.PoolCounterZeroStoreReached
            && nonreturningHelper.PoolCounterReadbackReached
            && nonreturningHelper.ReleaseHelperCallReached
            && nonreturningHelper.Boundary
                == UiShaderSourcePoolResetFinalizationBoundary.ReleaseHelperDidNotReturnNormally
            && !nonreturningHelper.SehChainRestored
            && nonreturningHelper.ReturnedEax is null
            && nonreturningHelper.ReturnRawFileOffset is null
            && nonreturningHelper.ReturnStackPopBytes is null);

        var changedReadback = UiShaderSourcePoolResetFinalizationProjectionRules.Project(
            nonAlias.Capture with { PoolCounterReadbackValue = 0x12345678 });
        check("The later readback remains an independent capture even after the projected zero store",
            changedReadback.PoolCounterZeroStoreValue == 0
            && changedReadback.PoolCounterReadbackValue == 0x12345678
            && changedReadback.ReturnedEax == 7);

        var emptyGate = UiShaderSourcePoolResetSecondaryGateProjectionRules.Project(
            new UiShaderSourcePoolResetSecondaryGateCapture(0x1000, 0));
        var emptyFinalization = UiShaderSourcePoolResetFinalizationProjectionRules.Project(
            nonAlias.Capture with
            {
                ReturnedCountEdi = emptyGate.ReturnedCountEdiAtBoundary,
                Entry = UiShaderSourcePoolResetFinalizationEntry.InitialSecondaryPopWasEmpty
            });
        check("The initial empty-secondary finalization edge retains zero EDI instead of loading a later count",
            emptyGate.StopRawFileOffset
                == UiShaderSourcePoolResetFinalizationProjectionRules.PoolCounterPointerLoadRawFileOffset
            && emptyFinalization.ReturnedEax == 0
            && ThrowsArgumentException(() =>
                UiShaderSourcePoolResetFinalizationProjectionRules.Project(
                    nonAlias.Capture with
                    {
                        Entry = UiShaderSourcePoolResetFinalizationEntry.InitialSecondaryPopWasEmpty
                    })));

        bool skippedHelperCannotHaveNonreturnOutcome = ThrowsArgumentException(() =>
            UiShaderSourcePoolResetFinalizationProjectionRules.Project(
                new UiShaderSourcePoolResetFinalizationCapture(
                    ReturnedCountEdi: 1,
                    ReceiverPointer: 2,
                    TempBeginPointer: 0,
                    PoolCounterPointer: 3,
                    LocalZeroAddress: 4,
                    PoolCounterReadbackValue: 5,
                    ReleaseHelperReturnedNormally: false)));
        check("The helper-return capture is only meaningful when the tempbegin null gate reaches its call",
            skippedHelperCannotHaveNonreturnOutcome);
    }

    private static bool HasExactSlice(
        UiShaderSourcePoolResetFinalizationInstructionEvidence instruction)
    {
        string[] bytes =
            UiShaderSourcePoolResetFinalizationProjectionRules.CapturedFinalizationBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        uint startRaw = UiShaderSourcePoolResetFinalizationProjectionRules.FinalizationStartRawFileOffset;
        if (instruction.RawFileOffset < startRaw)
        {
            return false;
        }

        string[] expected = instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int start = checked((int)(instruction.RawFileOffset - startRaw));
        return start + expected.Length <= bytes.Length
            && bytes.Skip(start).Take(expected.Length).SequenceEqual(expected);
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
