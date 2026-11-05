using Firefall.Reconstruction.Startup;
using System.Numerics;

internal static class GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .CoordinatorProvidedBytes);
        check("First-branch bytes anchor the four POPs and exclusive F1 boundary",
            GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .BytesIndependentlyVerified
            && suppliedBytes.SequenceEqual(
                GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection.WindowBytes)
            && GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .PopECXRawFileOffset == 0x00BD34ED
            && GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .PopECXEncoding == "59"
            && GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .PopEDIRawFileOffset == 0x00BD34EE
            && GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .PopEDIEncoding == "5F"
            && GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .PopESIRawFileOffset == 0x00BD34EF
            && GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .PopESIEncoding == "5E"
            && GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .PopEBXRawFileOffset == 0x00BD34F0
            && GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .PopEBXEncoding == "5B"
            && GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .StopBeforeMoveEspEbpRawFileOffset == 0x00BD34F1
            && GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .StopBeforeMoveEspEbpEncoding == "8B E5"
            && GameSocketParserCountConsumptionStoresProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BD34ED) == 0x00FD40ED
            && GameSocketParserCountConsumptionStoresProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BD34F1) == 0x00FD40F1
            && !GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .ExecutesNativeInstructions
            && !GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .ExecutesNativeReads
            && !GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection
                .ExecutesContinuationInstruction);

        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan successful =
            Project(2, 0xFFFFFFFC, 0x12345678, 0, uint.MaxValue, 0, uint.MaxValue);
        check("Fresh POP reads commit in order, wrap ESP, and preserve inherited ADD flags",
            successful.ParentPlan.ExactCleanCompletion
            && successful.ExactCleanCompletion
            && successful.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreOutcome
                    .CompletedBeforePopAt34ED
            && successful.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                    .CompletedBeforeMoveESPFromEBPAt34F1
            && successful.Pops.Select(pop => pop.InstructionRawFileOffset).SequenceEqual(
                new uint[] { 0x00BD34ED, 0x00BD34EE, 0x00BD34EF, 0x00BD34F0 })
            && successful.Reads.Select(read => read.Address)
                .SequenceEqual(new uint?[] { 0xFFFFFFFC, 0, 4, 8 })
            && successful.Reads.Select(read => read.Value)
                .SequenceEqual(new uint?[] { 0, uint.MaxValue, 0, uint.MaxValue })
            && successful.Pops.All(pop =>
                pop.Attempted
                && pop.Reached
                && pop.Committed
                && pop.FlagsBefore == successful.ParentPlan.FlagsAtStop
                && pop.FlagsAfter == successful.ParentPlan.FlagsAtStop)
            && successful.ECXAfterPop == 0
            && successful.EDIAfterPop == uint.MaxValue
            && successful.ESIAfterPop == 0
            && successful.EBXAfterPop == uint.MaxValue
            && successful.InitialESP == 0xFFFFFFFC
            && successful.Pops.Select(pop => pop.StackPointerAfter)
                .SequenceEqual(new uint?[] { 0, 4, 8, 12 })
            && successful.ResultESP == 12
            && successful.ESPAdvancedForCommittedPops
            && successful.StopBeforeRawFileOffset == 0x00BD34F1
            && successful.LastReachedRawFileOffset == 0x00BD34F0
            && successful.ContinuationBoundary is {
                InstructionRawFileOffset: 0x00BD34F1,
                NextInstructionEncoding: "8B E5",
                Reached: true,
                NextInstructionExecuted: false
            }
            && successful.FlagsAtStop == successful.ParentPlan.FlagsAtStop
            && successful.FlagsAtStop == ExpectedAddFlags(2, 1)
            && successful.FirstXaddAddFlagsPreserved
            && successful.EAXAfterPop is null
            && successful.EBPAfterPop is null
            && !successful.EAXCaptured
            && !successful.EBPCaptured
            && successful.FreshPopECXCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState.Captured
            && successful.FreshPopEDICaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState.Captured
            && successful.FreshPopESICaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState.Captured
            && successful.FreshPopEBXCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState.Captured);

        RunMissingPopChecks(check);
        RunRejectedParentChecks(check);
        RunReprojectionCheck(check, successful);
    }

    private static void RunMissingPopChecks(Action<string, bool> check)
    {
        uint[] offsets = { 0x00BD34ED, 0x00BD34EE, 0x00BD34EF, 0x00BD34F0 };
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome[] outcomes =
        {
            GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                .MissingFreshPopECXAt34ED,
            GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                .MissingFreshPopEDIAt34EE,
            GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                .MissingFreshPopESIAt34EF,
            GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                .MissingFreshPopEBXAt34F0
        };
        for (int missingIndex = 0; missingIndex < offsets.Length; missingIndex++)
        {
            uint?[] values = { 0x10, 0x20, 0x30, 0x40 };
            values[missingIndex] = null;
            GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan plan =
                Project(2, 0x1000, 0x12345678, values[0], values[1], values[2], values[3]);
            bool statesInOrder = true;
            bool committedRegistersPreserved = true;
            for (int index = 0; index < values.Length; index++)
            {
                GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState expected =
                    index < missingIndex
                        ? GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                            .Captured
                        : index == missingIndex
                            ? GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                                .Missing
                            : values[index].HasValue
                                ? GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                                    .SuppliedButUnused
                                : GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                                    .NotReached;
                statesInOrder &= CaptureState(plan, index) == expected;
                uint? expectedRegister = index < missingIndex ? values[index] : null;
                committedRegistersPreserved &=
                    RegisterValue(plan, index) == expectedRegister;
            }

            check($"Missing fresh POP read at {offsets[missingIndex]:X8} preserves only earlier commits",
                plan.ParentPlan.ExactCleanCompletion
                && plan.SuffixAdmitted
                && plan.Outcome == outcomes[missingIndex]
                && plan.Reads.Length == missingIndex + 1
                && plan.Pops.Length == missingIndex + 1
                && plan.Pops[^1] is {
                    Attempted: true,
                    Reached: true,
                    Committed: false,
                    StackPointerBefore: var before,
                    StackPointerAfter: var after
                }
                && before == unchecked(0x1000u + (uint)(missingIndex * 4))
                && after == before
                && plan.Reads[^1] is {
                    Attempted: true,
                    Captured: false,
                    NativeReadExecuted: false,
                    Value: null
                }
                && plan.Pops.Take(missingIndex).All(pop => pop.Committed)
                && plan.LastReachedRawFileOffset == offsets[missingIndex]
                && plan.StopBeforeRawFileOffset == offsets[missingIndex]
                && plan.ResultESP == unchecked(0x1000u + (uint)(missingIndex * 4))
                && plan.ContinuationBoundary is null
                && !plan.ChildBoundedCompletionBoundaryReached
                && plan.ExactBoundedCompletion
                && !plan.ExactCleanCompletion
                && plan.ESPAdvancedForCommittedPops
                && plan.FlagsAtStop == plan.ParentPlan.FlagsAtStop
                && plan.FirstXaddAddFlagsPreserved
                && committedRegistersPreserved
                && statesInOrder);
        }

        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan wrappedPartial =
            Project(2, 0xFFFFFFFC, 0x12345678, 0, null, 0x30, 0x40);
        check("A missing second POP preserves wrapped ECX commit and ESP zero",
            wrappedPartial.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsOutcome
                    .MissingFreshPopEDIAt34EE
            && wrappedPartial.Reads.Select(read => read.Address)
                .SequenceEqual(new uint?[] { 0xFFFFFFFC, 0 })
            && wrappedPartial.Pops[0].Committed
            && wrappedPartial.Pops[1] is {
                StackPointerBefore: 0,
                StackPointerAfter: 0,
                Committed: false
            }
            && wrappedPartial.ECXAfterPop == 0
            && wrappedPartial.EDIAfterPop is null
            && wrappedPartial.ResultESP == 0
            && wrappedPartial.ESPAdvancedForCommittedPops
            && wrappedPartial.FlagsAtStop == wrappedPartial.ParentPlan.FlagsAtStop
            && wrappedPartial.FirstXaddAddFlagsPreserved
            && wrappedPartial.StopBeforeRawFileOffset == 0x00BD34EE
            && wrappedPartial.FreshPopESICaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && wrappedPartial.FreshPopEBXCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused);
    }

    private static void RunRejectedParentChecks(Action<string, bool> check)
    {
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan fallthrough =
            Project(1, 0x7400, 0x12345678, 1, 2, 3, 4);
        check("A not-taken first-XADD branch leaves every POP unused at CB",
            fallthrough.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreOutcome
                    .BranchNotTakenTo34CB
            && !fallthrough.ParentContinuationReached
            && !fallthrough.SuffixAdmitted
            && fallthrough.Pops.IsEmpty
            && fallthrough.Reads.IsEmpty
            && fallthrough.FreshPopECXCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && fallthrough.FreshPopEBXCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && fallthrough.FlagsAtStop == fallthrough.ParentPlan.FlagsAtStop
            && fallthrough.LastReachedRawFileOffset
                == fallthrough.ParentPlan.LastReachedRawFileOffset
            && fallthrough.StopBeforeRawFileOffset
                == fallthrough.ParentPlan.StopBeforeRawFileOffset
            && fallthrough.ExactBoundedCompletion
                == fallthrough.ParentPlan.ExactBoundedCompletion
            && !fallthrough.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan missingESP =
            Project(2, null, 0x12345678, 1, 2, 3, 4);
        check("Missing first-branch ESP preserves the parent's E2 stop and skips POP reads",
            !missingESP.SuffixAdmitted
            && missingESP.Pops.IsEmpty
            && missingESP.Reads.IsEmpty
            && missingESP.InitialESP is null
            && missingESP.FlagsAtStop == missingESP.ParentPlan.FlagsAtStop
            && missingESP.LastReachedRawFileOffset
                == missingESP.ParentPlan.LastReachedRawFileOffset
            && missingESP.StopBeforeRawFileOffset == 0x00BD34E2
            && missingESP.StopBeforeRawFileOffset
                == missingESP.ParentPlan.StopBeforeRawFileOffset
            && missingESP.FreshPopECXCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused);

        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan missingDWORD =
            Project(2, 0x7400, null, 1, 2, 3, 4);
        check("Missing FS restoration DWORD preserves the parent's E2 stop and rejects POPs",
            missingDWORD.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreOutcome
                    .MissingFreshDWORDAt34E2
            && !missingDWORD.SuffixAdmitted
            && missingDWORD.Pops.IsEmpty
            && missingDWORD.Reads.IsEmpty
            && missingDWORD.FlagsAtStop == missingDWORD.ParentPlan.FlagsAtStop
            && missingDWORD.LastReachedRawFileOffset
                == missingDWORD.ParentPlan.LastReachedRawFileOffset
            && missingDWORD.StopBeforeRawFileOffset
                == missingDWORD.ParentPlan.StopBeforeRawFileOffset
            && missingDWORD.ExactBoundedCompletion
                == missingDWORD.ParentPlan.ExactBoundedCompletion
            && missingDWORD.FreshPopEBXCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused);

        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan rejectedParent =
            Project(null, 0x7500, 0x11223344, 10, 20, 30, 40);
        check("An incomplete XADD ancestor retains its own boundary and rejects supplied POPs",
            !rejectedParent.ParentContinuationReached
            && !rejectedParent.SuffixAdmitted
            && rejectedParent.Pops.IsEmpty
            && rejectedParent.Reads.IsEmpty
            && rejectedParent.FreshPopECXCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && rejectedParent.FreshPopEDICaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && rejectedParent.FreshPopESICaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && rejectedParent.FreshPopEBXCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && rejectedParent.FlagsAtStop == rejectedParent.ParentPlan.FlagsAtStop
            && rejectedParent.LastReachedRawFileOffset
                == rejectedParent.ParentPlan.LastReachedRawFileOffset
            && rejectedParent.StopBeforeRawFileOffset
                == rejectedParent.ParentPlan.StopBeforeRawFileOffset
            && rejectedParent.ExactBoundedCompletion
                == rejectedParent.ParentPlan.ExactBoundedCompletion
            && !rejectedParent.ExactCleanCompletion);
    }

    private static void RunReprojectionCheck(
        Action<string, bool> check,
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan accepted)
    {
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan replay =
            GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection.Project(
                accepted.Capture);
        check("The ordered child reads, POPs, ADD flags, and ancestry reproject identically",
            replay.Capture == accepted.Capture
            && replay.Outcome == accepted.Outcome
            && replay.ParentPlan.Outcome == accepted.ParentPlan.Outcome
            && replay.ParentPlan.StopBeforeRawFileOffset
                == accepted.ParentPlan.StopBeforeRawFileOffset
            && replay.ParentPlan.LastReachedRawFileOffset
                == accepted.ParentPlan.LastReachedRawFileOffset
            && replay.Reads.SequenceEqual(accepted.Reads)
            && replay.Pops.SequenceEqual(accepted.Pops)
            && replay.InitialESP == accepted.InitialESP
            && replay.ResultESP == accepted.ResultESP
            && replay.FlagsAtStop == accepted.FlagsAtStop
            && replay.LastReachedRawFileOffset == accepted.LastReachedRawFileOffset
            && replay.StopBeforeRawFileOffset == accepted.StopBeforeRawFileOffset
            && replay.OrderedSemanticPrefixPreserved
            && replay.FirstXaddAddFlagsPreserved
            && replay.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCaptureState
        CaptureState(
            GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan plan,
            int index) =>
        index switch
        {
            0 => plan.FreshPopECXCaptureState,
            1 => plan.FreshPopEDICaptureState,
            2 => plan.FreshPopESICaptureState,
            _ => plan.FreshPopEBXCaptureState
        };

    private static uint? RegisterValue(
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan plan,
        int index) =>
        index switch
        {
            0 => plan.ECXAfterPop,
            1 => plan.EDIAfterPop,
            2 => plan.ESIAfterPop,
            _ => plan.EBXAfterPop
        };

    private static GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsPlan Project(
        uint? firstRmwDWORD,
        uint? freshESP,
        uint? freshDWORD,
        uint? popECX,
        uint? popEDI,
        uint? popESI,
        uint? popEBX) =>
        GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsProjection.Project(
            new(
                new(Capture(firstRmwDWORD), freshESP, freshDWORD),
                popECX,
                popEDI,
                popESI,
                popEBX));

    private static GameSocketParserDiagnosticCleanupXaddBranchCapture Capture(
        uint? firstRmwDWORD)
    {
        GameSocketParserDiagnosticCleanupStackPrefixCapture stackCapture =
            CreateStackPrefixCapture();
        GameSocketParserDiagnosticCleanupZeroBranchCapture zeroBranchCapture =
            new(stackCapture);
        GameSocketParserDiagnosticCleanupRegisterPreparationCapture registerCapture =
            new(zeroBranchCapture);
        GameSocketParserDiagnosticCleanupXaddCapture xaddCapture =
            new(registerCapture, firstRmwDWORD);
        return new(xaddCapture);
    }

    private static GameSocketParserDiagnosticCleanupStackPrefixCapture CreateStackPrefixCapture()
    {
        GameSocketParserSelectorDiagnosticPreparationCapture preparationCapture =
            new(
                new GameSocketParserStoredByteSelectorGateCapture(
                    new GameSocketParserCountConsumptionStoresCapture(
                        new GameSocketParserCallerCountGateCapture(
                            new GameSocketParserSecondByteScalarGateCapture(
                                new GameSocketParserFirstByteFieldsCapture(
                                    new GameSocketParserPostResizePointerCapture(
                                        new GameSocketParserLocalSizeDispatchCapture(
                                            true,
                                            0x1000,
                                            0x5000,
                                            0x6000,
                                            true,
                                            0x1234,
                                            0x2000,
                                            new GameSocketParserLocalSizeDispatchFlags(
                                                Carry: true,
                                                Parity: false,
                                                AuxiliaryCarry: true,
                                                Zero: false,
                                                Sign: true,
                                                Overflow: false)),
                                        true,
                                        0x3000,
                                        0x5000,
                                        0x6000),
                                    0),
                                FreshSecondHeaderByte: 2),
                            0x7000,
                            100),
                        FreshStackDwordAtESPPlus14: 0x1000,
                        FreshStackByteAtESPPlus13: 1)),
                0x7000,
                0xCAFEBABE);
        GameSocketParserSelectorDiagnosticCounterContinuationCapture counterCapture =
            new(
                new(
                    preparationCapture,
                    NormalReturnObserved: true,
                    FreshPostCallESP: 0x8FEC),
                FreshPostCallESI: 0x7000,
                FreshCounterDWORD: 4);
        return new(
            new(
                new(
                    counterCapture,
                    FreshPostCallEBP: 0x7000,
                    FreshDWORDAtEBPPlusC: 0,
                    FreshDWORDAtESPPlus14: 0xCAFEBABE)),
            0xCAFEBABE);
    }

    private static GameSocketParserStoredByteSelectorGateFlags ExpectedAddFlags(
        uint oldValue,
        uint result)
    {
        bool parity = (BitOperations.PopCount((byte)result) & 1) == 0;
        bool auxiliaryCarry = (oldValue & 0xF) != 0;
        bool overflow = (oldValue & 0x80000000) != 0
            && ((oldValue ^ result) & 0x80000000) != 0;
        return new(
            Carry: oldValue != 0,
            Parity: parity,
            AuxiliaryCarry: auxiliaryCarry,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: overflow);
    }

    private static byte[] ParseBytes(string text) =>
        text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
