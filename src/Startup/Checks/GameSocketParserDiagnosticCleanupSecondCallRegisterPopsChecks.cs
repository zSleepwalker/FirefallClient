using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupSecondCallRegisterPopsChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                .CoordinatorProvidedBytes);
        check("The captured window anchors the ordered POP sequence and exclusive F1 boundary",
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                .BytesIndependentlyVerified
            && suppliedBytes.SequenceEqual(
                GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.WindowBytes)
            && GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.PopECXRawFileOffset
                == 0x00BD34ED
            && GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.PopECXEncoding
                == "59"
            && GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.PopEDIRawFileOffset
                == 0x00BD34EE
            && GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.PopEDIEncoding
                == "5F"
            && GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.PopESIRawFileOffset
                == 0x00BD34EF
            && GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.PopESIEncoding
                == "5E"
            && GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.PopEBXRawFileOffset
                == 0x00BD34F0
            && GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.PopEBXEncoding
                == "5B"
            && GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                .StopBeforeMoveEspEbpRawFileOffset == 0x00BD34F1
            && GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                .StopBeforeMoveEspEbpEncoding == "8B E5"
            && !GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                .ExecutesNativeInstructions
            && !GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                .ExecutesNativeReads
            && !GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection
                .ExecutesContinuationInstruction);

        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan zeroAndMaximumRegisters =
            Project(0, 0, uint.MaxValue, 0, uint.MaxValue);
        check("Each POP captures its own DWORD at the current ESP and updates only its register and ESP",
            zeroAndMaximumRegisters.ParentPlan.ExactCleanCompletion
            && zeroAndMaximumRegisters.ExactCleanCompletion
            && zeroAndMaximumRegisters.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
                    .CompletedBeforeMoveESPFromEBPAt34F1
            && zeroAndMaximumRegisters.Pops.Select(pop => pop.InstructionRawFileOffset)
                .SequenceEqual(new uint[] { 0x00BD34ED, 0x00BD34EE, 0x00BD34EF, 0x00BD34F0 })
            && zeroAndMaximumRegisters.Reads.Select(read => read.Address)
                .SequenceEqual(new uint?[] { 0, 4, 8, 12 })
            && zeroAndMaximumRegisters.Reads.Select(read => read.Value)
                .SequenceEqual(new uint?[] { 0, uint.MaxValue, 0, uint.MaxValue })
            && zeroAndMaximumRegisters.Pops.All(pop =>
                pop.Attempted
                && pop.Reached
                && pop.Committed
                && pop.FlagsBefore is null
                && pop.FlagsAfter is null)
            && zeroAndMaximumRegisters.ECXAfterPop == 0
            && zeroAndMaximumRegisters.EDIAfterPop == uint.MaxValue
            && zeroAndMaximumRegisters.ESIAfterPop == 0
            && zeroAndMaximumRegisters.EBXAfterPop == uint.MaxValue
            && zeroAndMaximumRegisters.InitialESP == 0
            && zeroAndMaximumRegisters.ResultESP == 16
            && zeroAndMaximumRegisters.ESPAdvancedForCommittedPops
            && zeroAndMaximumRegisters.StopBeforeRawFileOffset == 0x00BD34F1
            && zeroAndMaximumRegisters.LastReachedRawFileOffset == 0x00BD34F0
            && zeroAndMaximumRegisters.ContinuationBoundary is {
                InstructionRawFileOffset: 0x00BD34F1,
                NextInstructionEncoding: "8B E5",
                Reached: true,
                NextInstructionExecuted: false
            }
            && zeroAndMaximumRegisters.FlagsAtStop is null
            && zeroAndMaximumRegisters.EAXAfterPop is null
            && zeroAndMaximumRegisters.EBPAfterPop is null
            && !zeroAndMaximumRegisters.EAXCaptured
            && !zeroAndMaximumRegisters.EBPCaptured
            && zeroAndMaximumRegisters.FreshPopECXCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState.Captured
            && zeroAndMaximumRegisters.FreshPopEDICaptureState
                == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState.Captured
            && zeroAndMaximumRegisters.FreshPopESICaptureState
                == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState.Captured
            && zeroAndMaximumRegisters.FreshPopEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState.Captured);

        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan wrappedESP =
            Project(0xFFFFFFFC, uint.MaxValue, 0, uint.MaxValue, 0);
        check("ESP wraps after POP ECX from FFFFFFFC and each later POP reads the resulting address",
            wrappedESP.ExactCleanCompletion
            && wrappedESP.InitialESP == 0xFFFFFFFC
            && wrappedESP.Reads.Select(read => read.Address)
                .SequenceEqual(new uint?[] { 0xFFFFFFFC, 0, 4, 8 })
            && wrappedESP.Pops.Select(pop => pop.StackPointerAfter)
                .SequenceEqual(new uint?[] { 0, 4, 8, 12 })
            && wrappedESP.ECXAfterPop == uint.MaxValue
            && wrappedESP.EDIAfterPop == 0
            && wrappedESP.ESIAfterPop == uint.MaxValue
            && wrappedESP.EBXAfterPop == 0
            && wrappedESP.ResultESP == 12
            && wrappedESP.ESPAdvancedForCommittedPops);

        RunMissingPopChecks(check, wrappedESP.ParentPlan);
        RunAliasedPriorWriteCheck(check);
        RunRejectedParentChecks(check);
        RunReprojectionCheck(check, zeroAndMaximumRegisters);
    }

    private static void RunMissingPopChecks(
        Action<string, bool> check,
        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan wrappedParentPlan)
    {
        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan sharedParentPlan =
            GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.Project(
                CreateFsRestoreCapture(0x1000, 0xCAFEBABE));
        var results = new GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan[4];
        uint[] offsets = { 0x00BD34ED, 0x00BD34EE, 0x00BD34EF, 0x00BD34F0 };
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome[] outcomes =
        {
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
                .MissingFreshPopECXAt34ED,
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
                .MissingFreshPopEDIAt34EE,
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
                .MissingFreshPopESIAt34EF,
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
                .MissingFreshPopEBXAt34F0
        };
        for (int missingIndex = 0; missingIndex < results.Length; missingIndex++)
        {
            uint?[] values = { 0x10, 0x20, 0x30, 0x40 };
            values[missingIndex] = null;
            results[missingIndex] = Project(
                sharedParentPlan,
                values[0],
                values[1],
                values[2],
                values[3]);
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan plan =
                results[missingIndex];
            bool statesInOrder = true;
            for (int index = 0; index < 4; index++)
            {
                GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState expected =
                    index < missingIndex
                        ? GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState.Captured
                        : index == missingIndex
                            ? GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
                                .Missing
                            : values[index].HasValue
                                ? GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
                                    .SuppliedButUnused
                                : GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
                                    .NotReached;
                statesInOrder &= CaptureState(plan, index) == expected;
            }

            check($"Missing fresh POP read at {offsets[missingIndex]:X8} preserves only earlier committed pops",
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
                && plan.LastReachedRawFileOffset == offsets[missingIndex]
                && plan.StopBeforeRawFileOffset == offsets[missingIndex]
                && plan.ResultESP == unchecked(0x1000u + (uint)(missingIndex * 4))
                && plan.Pops.Take(missingIndex).All(pop => pop.Committed)
                && plan.Pops.Take(missingIndex).Count()
                    == missingIndex
                && plan.ContinuationBoundary is null
                && !plan.ChildBoundedCompletionBoundaryReached
                && plan.ExactBoundedCompletion
                && !plan.ExactCleanCompletion
                && plan.ESPAdvancedForCommittedPops
                && plan.FlagsAtStop is null
                && statesInOrder);
        }

        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan wrappedPartial =
            Project(wrappedParentPlan, 0, null, 0x30, 0x40);
        check("A missing second POP after a wrapped first POP retains ESP zero and the committed ECX",
            wrappedPartial.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
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
            && wrappedPartial.StopBeforeRawFileOffset == 0x00BD34EE
            && wrappedPartial.FreshPopESICaptureState
                == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
                    .SuppliedButUnused
            && wrappedPartial.FreshPopEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
                    .SuppliedButUnused);
    }

    private static void RunAliasedPriorWriteCheck(Action<string, bool> check)
    {
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture baseParentCapture =
            CreateFsRestoreCapture(freshPostCallESP: 0x8000, freshFsRead: 0x12345678);
        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan baseParentPlan =
            GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.Project(
                baseParentCapture);
        GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect priorStore =
            baseParentPlan.MemoryEffects.Single(effect =>
                effect.Kind
                    == GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
                        .StoreFFFFFFFFAtESPPlus50
                && effect.Attempted
                && effect.Captured
                && effect.Committed);
        uint aliasAddress = priorStore.Address!.Value;
        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan aliasedParentPlan =
            GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.Project(
                new(baseParentPlan.Capture.ParentCapture, aliasAddress, 0xCAFEBABE));
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan aliasedRead =
            Project(
                aliasedParentPlan,
                0x0BADC0DE,
                0x22222222,
                0x33333333,
                0x44444444);
        check("POP ECX performs an independent fresh read at an address aliased by an earlier write",
            priorStore.Value == uint.MaxValue
            && aliasedRead.ParentPlan.ExactCleanCompletion
            && aliasedRead.Reads[0] is {
                Address: var freshAddress,
                Value: 0x0BADC0DE,
                Attempted: true,
                Captured: true,
                NativeReadExecuted: false
            }
            && freshAddress == priorStore.Address
            && aliasedRead.ECXAfterPop == 0x0BADC0DE
            && aliasedRead.ECXAfterPop != priorStore.Value
            && aliasedRead.ExactCleanCompletion);
    }

    private static void RunRejectedParentChecks(Action<string, bool> check)
    {
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture pendingCapture =
            CreateFsRestoreCapture(
                freshPostCallESP: 0x8000,
                freshFsRead: 1,
                returnedNormally: null);
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture abnormalCapture =
            CreateFsRestoreCapture(
                freshPostCallESP: 0x8100,
                freshFsRead: 2,
                returnedNormally: false);
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture missingVptrCapture =
            CreateFsRestoreCapture(
                freshPostCallESP: 0x8200,
                freshFsRead: 3,
                freshOperationVptr: null);
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture missingParentReadCapture =
            CreateFsRestoreCapture(
                freshPostCallESP: 0x8300,
                freshFsRead: null);
        var rejected = new[]
        {
            Project(pendingCapture, 10, 11, 12, 13),
            Project(abnormalCapture, 20, 21, 22, 23),
            Project(missingVptrCapture, 30, 31, 32, 33),
            Project(missingParentReadCapture, 40, 41, 42, 43)
        };
        check("Pending/false calls, missing vptr, and an E2 read failure retain each parent's actual bound",
            rejected.All(plan =>
                !plan.ParentContinuationReached
                && !plan.SuffixAdmitted
                && plan.Pops.IsEmpty
                && plan.Reads.IsEmpty
                && plan.ContinuationBoundary is null
                && plan.FreshPopECXCaptureState
                    == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
                        .SuppliedButUnused
                && plan.FreshPopEDICaptureState
                    == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
                        .SuppliedButUnused
                && plan.FreshPopESICaptureState
                    == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
                        .SuppliedButUnused
                && plan.FreshPopEBXCaptureState
                    == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
                        .SuppliedButUnused
                && plan.FlagsAtStop == plan.ParentPlan.FlagsAtStop
                && plan.LastReachedRawFileOffset == plan.ParentPlan.LastReachedRawFileOffset
                && plan.StopBeforeRawFileOffset == plan.ParentPlan.StopBeforeRawFileOffset
                && plan.ExactBoundedCompletion == plan.ParentPlan.ExactBoundedCompletion)
            && rejected[0].StopBeforeRawFileOffset == 0x00BD34DF
            && rejected[1].StopBeforeRawFileOffset == 0x00BD34DF
            && rejected[2].ParentPlan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34DB
            && rejected[2].StopBeforeRawFileOffset == 0x00BD34DB
            && rejected[3].ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome
                    .MissingFreshDWORDAt34E2
            && rejected[3].ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && rejected[3].StopBeforeRawFileOffset == 0x00BD34E2
            && !rejected[3].ParentExactBoundedCompletion
            && !rejected[3].ExactBoundedCompletion
            && rejected.All(plan => !plan.ExactCleanCompletion));

        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan missingFreshESP =
            Project(
                CreateFsRestoreCapture(
                    freshPostCallESP: null,
                    freshFsRead: 0xABCDEF01),
                1,
                2,
                3,
                4);
        check("A missing fresh ESP at E2 keeps the FS parent's bound and does not infer a POP address",
            missingFreshESP.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome
                    .MissingFreshPostCallESPAt34E2
            && missingFreshESP.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && missingFreshESP.StopBeforeRawFileOffset == 0x00BD34E2
            && missingFreshESP.InitialESP is null
            && missingFreshESP.Pops.IsEmpty
            && missingFreshESP.Reads.IsEmpty
            && missingFreshESP.FreshPopECXCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
                    .SuppliedButUnused
            && !missingFreshESP.ParentExactBoundedCompletion
            && !missingFreshESP.ExactBoundedCompletion
            && !missingFreshESP.ExactCleanCompletion);
    }

    private static void RunReprojectionCheck(
        Action<string, bool> check,
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan accepted)
    {
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan replay =
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.Project(
                accepted.Capture);
        check("The complete ordered pop/read plan and parent evidence reproject identically",
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
            && replay.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCaptureState
        CaptureState(
            GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan plan,
            int index) =>
        index switch
        {
            0 => plan.FreshPopECXCaptureState,
            1 => plan.FreshPopEDICaptureState,
            2 => plan.FreshPopESICaptureState,
            _ => plan.FreshPopEBXCaptureState
        };

    private static GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan Project(
        uint? postCallESP,
        uint? popECX,
        uint? popEDI,
        uint? popESI,
        uint? popEBX) =>
        Project(
            CreateFsRestoreCapture(postCallESP, 0xCAFEBABE),
            popECX,
            popEDI,
            popESI,
            popEBX);

    private static GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan Project(
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture parentCapture,
        uint? popECX,
        uint? popEDI,
        uint? popESI,
        uint? popEBX) =>
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.Project(
            new(parentCapture, popECX, popEDI, popESI, popEBX));

    private static GameSocketParserDiagnosticCleanupSecondCallRegisterPopsPlan Project(
        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan parentPlan,
        uint? popECX,
        uint? popEDI,
        uint? popESI,
        uint? popEBX) =>
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsProjection.Project(
            new(parentPlan.Capture, popECX, popEDI, popESI, popEBX));

    private static GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture
        CreateFsRestoreCapture(
            uint? freshPostCallESP,
            uint? freshFsRead,
            bool? returnedNormally = true,
            uint? freshOperationVptr = 0x7400)
    {
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture returnCapture =
            CreateSecondCallReturnCapture(freshOperationVptr, returnedNormally);
        return new(returnCapture, freshPostCallESP, freshFsRead);
    }

    private static GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture
        CreateSecondCallReturnCapture(uint? freshOperationVptr, bool? returnedNormally)
    {
        GameSocketParserDiagnosticCleanupStackPrefixCapture stackCapture =
            CreateStackPrefixCapture();
        GameSocketParserDiagnosticCleanupZeroBranchCapture zeroBranchCapture =
            new(stackCapture);
        GameSocketParserDiagnosticCleanupRegisterPreparationCapture registerCapture =
            new(zeroBranchCapture);
        GameSocketParserDiagnosticCleanupXaddCapture xaddCapture =
            new(registerCapture, 1);
        GameSocketParserDiagnosticCleanupXaddBranchCapture xaddBranchCapture =
            new(xaddCapture);
        GameSocketParserDiagnosticCleanupVirtualCallPreparationCapture firstCallPreparation =
            new(xaddBranchCapture, 0x7000);
        GameSocketParserDiagnosticCleanupVirtualCallReturnCapture firstCallReturn =
            new(firstCallPreparation, 0x12345678, true);
        GameSocketParserDiagnosticCleanupPostCallAddressCapture addressCapture =
            new(firstCallReturn, 0x7000);
        GameSocketParserDiagnosticCleanupSecondXaddBranchCapture secondXaddBranchCapture =
            new(new(addressCapture, 0x1234, 1));
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCapture secondCallPreparation =
            new(secondXaddBranchCapture, freshOperationVptr);
        return new(secondCallPreparation, 0x87654321, returnedNormally);
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
