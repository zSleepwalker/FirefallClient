using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .CoordinatorProvidedBytes);
        check("IDA raw-offset bytes anchor the four POPs and exclusive F1 boundary",
            GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .BytesIndependentlyVerified
            && suppliedBytes.SequenceEqual(
                GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection.WindowBytes)
            && GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .PopECXRawFileOffset == 0x00BD34ED
            && GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .PopECXEncoding == "59"
            && GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .PopEDIRawFileOffset == 0x00BD34EE
            && GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .PopEDIEncoding == "5F"
            && GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .PopESIRawFileOffset == 0x00BD34EF
            && GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .PopESIEncoding == "5E"
            && GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .PopEBXRawFileOffset == 0x00BD34F0
            && GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .PopEBXEncoding == "5B"
            && GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .StopBeforeMoveEspEbpRawFileOffset == 0x00BD34F1
            && GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .StopBeforeMoveEspEbpEncoding == "8B E5"
            && GameSocketParserCountConsumptionStoresProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BD34ED) == 0x00FD40ED
            && !GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .ExecutesNativeInstructions
            && !GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .ExecutesNativeReads
            && !GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection
                .ExecutesContinuationInstruction);

        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan zeroAndMaximum =
            Project(2, 0xFFFFFFFC, 0, uint.MaxValue, 0, uint.MaxValue);
        check("Each POP freshly reads its own DWORD, advances wrapped ESP, and preserves DEC flags",
            zeroAndMaximum.ParentPlan.ExactCleanCompletion
            && zeroAndMaximum.ExactCleanCompletion
            && zeroAndMaximum.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsOutcome
                    .CompletedBeforeMoveESPFromEBPAt34F1
            && zeroAndMaximum.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome.JnzTakenTo34E2
            && zeroAndMaximum.ParentPlan.ParentPlan.Instructions[0].FlagsAfter
                == zeroAndMaximum.ParentPlan.FlagsAtStop
            && zeroAndMaximum.ParentPlan.ParentPlan.Instructions[1].Taken == true
            && !zeroAndMaximum.ParentPlan.OpaqueCallAfterBranchObserved
            && zeroAndMaximum.Pops.Select(pop => pop.InstructionRawFileOffset)
                .SequenceEqual(new uint[] { 0x00BD34ED, 0x00BD34EE, 0x00BD34EF, 0x00BD34F0 })
            && zeroAndMaximum.Reads.Select(read => read.Address)
                .SequenceEqual(new uint?[] { 0xFFFFFFFC, 0, 4, 8 })
            && zeroAndMaximum.Reads.Select(read => read.Value)
                .SequenceEqual(new uint?[] { 0, uint.MaxValue, 0, uint.MaxValue })
            && zeroAndMaximum.Pops.All(pop =>
                pop.Attempted
                && pop.Reached
                && pop.Committed
                && pop.FlagsBefore == zeroAndMaximum.ParentPlan.FlagsAtStop
                && pop.FlagsAfter == zeroAndMaximum.ParentPlan.FlagsAtStop)
            && zeroAndMaximum.ECXAfterPop == 0
            && zeroAndMaximum.EDIAfterPop == uint.MaxValue
            && zeroAndMaximum.ESIAfterPop == 0
            && zeroAndMaximum.EBXAfterPop == uint.MaxValue
            && zeroAndMaximum.InitialESP == 0xFFFFFFFC
            && zeroAndMaximum.Pops.Select(pop => pop.StackPointerAfter)
                .SequenceEqual(new uint?[] { 0, 4, 8, 12 })
            && zeroAndMaximum.ResultESP == 12
            && zeroAndMaximum.ESPAdvancedForCommittedPops
            && zeroAndMaximum.StopBeforeRawFileOffset == 0x00BD34F1
            && zeroAndMaximum.LastReachedRawFileOffset == 0x00BD34F0
            && zeroAndMaximum.ContinuationBoundary is {
                InstructionRawFileOffset: 0x00BD34F1,
                NextInstructionEncoding: "8B E5",
                Reached: true,
                NextInstructionExecuted: false
            }
            && zeroAndMaximum.FlagsAtStop == zeroAndMaximum.ParentPlan.FlagsAtStop
            && zeroAndMaximum.FlagsAtStop is {
                Carry: not null,
                Parity: not null,
                AuxiliaryCarry: not null,
                Zero: false,
                Sign: not null,
                Overflow: not null
            }
            && zeroAndMaximum.DECFlagsPreserved
            && zeroAndMaximum.EAXAfterPop is null
            && zeroAndMaximum.EBPAfterPop is null
            && !zeroAndMaximum.EAXCaptured
            && !zeroAndMaximum.EBPCaptured
            && zeroAndMaximum.FreshPopECXCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState.Captured
            && zeroAndMaximum.FreshPopEDICaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState.Captured
            && zeroAndMaximum.FreshPopESICaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState.Captured
            && zeroAndMaximum.FreshPopEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState.Captured);

        RunMissingPopChecks(check);
        RunRejectedParentChecks(check);
        RunReprojectionCheck(check, zeroAndMaximum);
    }

    private static void RunMissingPopChecks(Action<string, bool> check)
    {
        uint[] offsets = { 0x00BD34ED, 0x00BD34EE, 0x00BD34EF, 0x00BD34F0 };
        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsOutcome[] outcomes =
        {
            GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsOutcome
                .MissingFreshPopECXAt34ED,
            GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsOutcome
                .MissingFreshPopEDIAt34EE,
            GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsOutcome
                .MissingFreshPopESIAt34EF,
            GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsOutcome
                .MissingFreshPopEBXAt34F0
        };
        for (int missingIndex = 0; missingIndex < offsets.Length; missingIndex++)
        {
            uint?[] values = { 0x10, 0x20, 0x30, 0x40 };
            values[missingIndex] = null;
            GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan plan =
                Project(2, 0x1000, values[0], values[1], values[2], values[3]);
            bool statesInOrder = true;
            bool committedRegistersPreserved = true;
            for (int index = 0; index < values.Length; index++)
            {
                GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState expected =
                    index < missingIndex
                        ? GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
                            .Captured
                        : index == missingIndex
                            ? GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
                                .Missing
                            : values[index].HasValue
                                ? GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
                                    .SuppliedButUnused
                                : GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
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
                && plan.DECFlagsPreserved
                && committedRegistersPreserved
                && statesInOrder);
        }

        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan wrappedPartial =
            Project(2, 0xFFFFFFFC, 0, null, 0x30, 0x40);
        check("A missing second POP after wrapped ECX preserves its commit and ESP zero",
            wrappedPartial.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsOutcome
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
            && wrappedPartial.DECFlagsPreserved
            && wrappedPartial.StopBeforeRawFileOffset == 0x00BD34EE
            && wrappedPartial.FreshPopESICaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && wrappedPartial.FreshPopEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
                    .SuppliedButUnused);
    }

    private static void RunRejectedParentChecks(Action<string, bool> check)
    {
        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan fallthrough =
            Project(1, 0x7400, 1, 2, 3, 4);
        check("The not-taken JNZ leaves all four supplied POP captures unused at DB",
            fallthrough.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                    .BranchNotTakenTo34DB
            && !fallthrough.ParentContinuationReached
            && !fallthrough.SuffixAdmitted
            && fallthrough.Pops.IsEmpty
            && fallthrough.Reads.IsEmpty
            && fallthrough.FreshPopECXCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && fallthrough.FreshPopEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && fallthrough.FlagsAtStop == fallthrough.ParentPlan.FlagsAtStop
            && fallthrough.LastReachedRawFileOffset
                == fallthrough.ParentPlan.LastReachedRawFileOffset
            && fallthrough.StopBeforeRawFileOffset == 0x00BD34DB
            && fallthrough.ExactBoundedCompletion
                == fallthrough.ParentPlan.ExactBoundedCompletion
            && !fallthrough.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan missingESP =
            Project(2, null, 1, 2, 3, 4);
        check("Missing branch ESP retains the E2 bound without attempting a POP read",
            missingESP.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                    .MissingFreshESPAt34E2
            && missingESP.ParentPlan.FreshESPCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState.Missing
            && missingESP.ParentPlan.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
                    .SuppliedButUnused
            && !missingESP.SuffixAdmitted
            && missingESP.Pops.IsEmpty
            && missingESP.Reads.IsEmpty
            && missingESP.InitialESP is null
            && missingESP.FlagsAtStop == missingESP.ParentPlan.FlagsAtStop
            && missingESP.LastReachedRawFileOffset
                == missingESP.ParentPlan.LastReachedRawFileOffset
            && missingESP.StopBeforeRawFileOffset == 0x00BD34E2
            && missingESP.ExactBoundedCompletion
                == missingESP.ParentPlan.ExactBoundedCompletion
            && !missingESP.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan missingDWORD =
            Project(Capture(2, 0x7400, null), 1, 2, 3, 4);
        check("Missing E2 stack DWORD preserves the FS parent's E2 bound and skips all POPs",
            missingDWORD.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                    .MissingFreshDWORDAt34E2
            && missingDWORD.ParentPlan.LastReachedRawFileOffset == 0x00BD34E2
            && missingDWORD.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && !missingDWORD.SuffixAdmitted
            && missingDWORD.Pops.IsEmpty
            && missingDWORD.Reads.IsEmpty
            && missingDWORD.FlagsAtStop == missingDWORD.ParentPlan.FlagsAtStop
            && missingDWORD.LastReachedRawFileOffset
                == missingDWORD.ParentPlan.LastReachedRawFileOffset
            && missingDWORD.StopBeforeRawFileOffset == 0x00BD34E2
            && missingDWORD.ExactBoundedCompletion
                == missingDWORD.ParentPlan.ExactBoundedCompletion
            && !missingDWORD.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan rejectedParent =
            Project(
                Capture(
                    2,
                    0x7500,
                    0x11223344,
                    freshOperationVptr: null),
                10,
                20,
                30,
                40);
        check("A rejected ancestor retains its actual boundary and leaves every supplied POP unused",
            !rejectedParent.ParentContinuationReached
            && !rejectedParent.SuffixAdmitted
            && rejectedParent.Pops.IsEmpty
            && rejectedParent.Reads.IsEmpty
            && rejectedParent.FreshPopECXCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && rejectedParent.FreshPopEDICaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && rejectedParent.FreshPopESICaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
                    .SuppliedButUnused
            && rejectedParent.FreshPopEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
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
        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan accepted)
    {
        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan replay =
            GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection.Project(
                accepted.Capture);
        check("The ordered child reads, POPs, DEC flags, and ancestry reproject identically",
            replay.Capture == accepted.Capture
            && replay.Outcome == accepted.Outcome
            && replay.ParentPlan.Capture == replay.Capture.ParentCapture
            && replay.ParentPlan.ParentPlan.Capture
                == replay.Capture.ParentCapture.ParentCapture
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
            && replay.DECFlagsPreserved
            && replay.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCaptureState
        CaptureState(
            GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan plan,
            int index) =>
        index switch
        {
            0 => plan.FreshPopECXCaptureState,
            1 => plan.FreshPopEDICaptureState,
            2 => plan.FreshPopESICaptureState,
            _ => plan.FreshPopEBXCaptureState
        };

    private static uint? RegisterValue(
        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan plan,
        int index) =>
        index switch
        {
            0 => plan.ECXAfterPop,
            1 => plan.EDIAfterPop,
            2 => plan.ESIAfterPop,
            _ => plan.EBXAfterPop
        };

    private static GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan Project(
        uint? secondRmwDWORD,
        uint? freshESP,
        uint? popECX,
        uint? popEDI,
        uint? popESI,
        uint? popEBX) =>
        Project(
            Capture(secondRmwDWORD, freshESP, 0xCAFEBABE),
            popECX,
            popEDI,
            popESI,
            popEBX);

    private static GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsPlan Project(
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCapture parentCapture,
        uint? popECX,
        uint? popEDI,
        uint? popESI,
        uint? popEBX) =>
        GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsProjection.Project(
            new(parentCapture, popECX, popEDI, popESI, popEBX));

    private static GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCapture Capture(
        uint? secondRmwDWORD,
        uint? freshESP,
        uint? freshDWORD,
        uint? freshPostCallEBX = 0x1234,
        uint? freshPostCallESI = 0x9000,
        bool? returnedNormally = true,
        uint? freshOperationVptr = 0x7000,
        uint? targetDWORD = 0x12345678,
        uint? firstRmwDWORD = 1) =>
        new(
            CreateParentCapture(
                secondRmwDWORD,
                freshPostCallEBX,
                freshPostCallESI,
                returnedNormally,
                freshOperationVptr,
                targetDWORD,
                firstRmwDWORD),
            freshESP,
            freshDWORD);

    private static GameSocketParserDiagnosticCleanupSecondXaddBranchCapture CreateParentCapture(
        uint? secondRmwDWORD,
        uint? freshPostCallEBX,
        uint? freshPostCallESI,
        bool? returnedNormally,
        uint? freshOperationVptr,
        uint? targetDWORD,
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
        GameSocketParserDiagnosticCleanupXaddBranchCapture xaddBranchCapture =
            new(xaddCapture);
        GameSocketParserDiagnosticCleanupVirtualCallPreparationCapture firstCallPreparation =
            new(xaddBranchCapture, freshOperationVptr);
        GameSocketParserDiagnosticCleanupVirtualCallReturnCapture firstCallReturn =
            new(firstCallPreparation, targetDWORD, returnedNormally);
        GameSocketParserDiagnosticCleanupPostCallAddressCapture addressCapture =
            new(firstCallReturn, freshPostCallESI);
        return new(new(addressCapture, freshPostCallEBX, secondRmwDWORD));
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

    private static byte[] ParseBytes(string encoding) =>
        encoding.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
