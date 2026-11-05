using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupSecondCallFrameResetChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                .CoordinatorProvidedBytes);
        check("The byte window anchors MOV ESP,EBP, POP EBP, and the exclusive RETN 8 boundary",
            GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                .BytesIndependentlyVerified
            && suppliedBytes.SequenceEqual(
                GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection.WindowBytes)
            && GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                .MoveEspEbpRawFileOffset == 0x00BD34F1
            && GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                .MoveEspEbpEncoding == "8B E5"
            && GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                .PopEBPRawFileOffset == 0x00BD34F3
            && GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                .PopEBPEncoding == "5D"
            && GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                .StopBeforeRet8RawFileOffset == 0x00BD34F4
            && GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                .Ret8Encoding == "C2 08 00"
            && suppliedBytes.SequenceEqual(ParseBytes("8B E5 5D C2 08 00"))
            && !GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                .ExecutesNativeInstructions
            && !GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                .ExecutesNativeReads
            && !GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection
                .ExecutesContinuationInstruction);

        GameSocketParserDiagnosticCleanupSecondCallFrameResetCapture normalCapture =
            new(
                CreateParentCapture(
                    returnedNormally: true,
                    freshOperationVptr: 0x7400,
                    freshParentRead: 0xCAFEBABE),
                FreshFrameResetEBP: 0x2000,
                FreshPopEBPStackDWORD: 0x12345678);
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan normal =
            GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection.Project(
                normalCapture);
        check("MOV independently resets ESP from fresh EBP, then POP EBP uses a fresh stack DWORD",
            normal.ParentPlan.ExactCleanCompletion
            && normal.ExactCleanCompletion
            && normal.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
                    .CompletedBeforeRet8At34F4
            && normal.FrameReset is {
                InstructionRawFileOffset: 0x00BD34F1,
                Encoding: "8B E5",
                EBPValue: 0x2000,
                ESPBefore: var espBefore,
                ESPAfter: 0x2000,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsBefore: null,
                FlagsAfter: null,
                FlagsWritten: false
            }
            && espBefore == normal.ParentPlan.ResultESP
            && normal.StackRead is {
                InstructionRawFileOffset: 0x00BD34F3,
                Address: 0x2000,
                Width: 4,
                Value: 0x12345678,
                Attempted: true,
                Captured: true,
                NativeReadExecuted: false
            }
            && normal.Pop is {
                InstructionRawFileOffset: 0x00BD34F3,
                Encoding: "5D",
                StackPointerBefore: 0x2000,
                StackPointerAfter: 0x2004,
                EBPBefore: 0x2000,
                Value: 0x12345678,
                EBPMovedTo: 0x12345678,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsBefore: null,
                FlagsAfter: null,
                FlagsWritten: false
            }
            && normal.InitialESP == normal.ParentPlan.ResultESP
            && normal.ResultESP == 0x2004
            && normal.FreshFrameResetEBP == 0x2000
            && normal.FreshPopEBPStackDWORD == 0x12345678
            && normal.EBPAfterPop == 0x12345678
            && normal.ESPResetFromFreshEBP
            && normal.ESPAdvancedForCommittedPop
            && normal.FlagsPreserved
            && normal.ContinuationBoundary is {
                InstructionRawFileOffset: 0x00BD34F4,
                NextInstructionEncoding: "C2 08 00",
                Reached: true,
                NextInstructionExecuted: false
            }
            && normal.LastReachedRawFileOffset == 0x00BD34F3
            && normal.StopBeforeRawFileOffset == 0x00BD34F4
            && !normal.NativeInstructionsExecuted
            && !normal.NativeReadsExecuted
            && !normal.RuntimeSampleExecuted
            && !normal.ParentInstructionsReplayed
            && !normal.NativeABIOrProtocolSemanticsModeled);

        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan wrappedESP =
            Project(freshFrameResetEBP: 0xFFFFFFFC, freshPopEBPStackDWORD: 0);
        check("POP EBP wraps ESP from FFFFFFFC to zero while preserving the captured DWORD",
            wrappedESP.ExactCleanCompletion
            && wrappedESP.FrameReset is { ESPAfter: 0xFFFFFFFC }
            && wrappedESP.StackRead is { Address: 0xFFFFFFFC, Value: 0 }
            && wrappedESP.Pop is {
                StackPointerBefore: 0xFFFFFFFC,
                StackPointerAfter: 0,
                Value: 0,
                EBPMovedTo: 0,
                Committed: true
            }
            && wrappedESP.ResultESP == 0
            && wrappedESP.EBPAfterPop == 0
            && wrappedESP.ESPResetFromFreshEBP
            && wrappedESP.ESPAdvancedForCommittedPop
            && wrappedESP.FlagsPreserved);

        RunMissingCaptureChecks(check);
        RunRejectedParentChecks(check);
        RunReprojectionCheck(check, normal);
    }

    private static void RunMissingCaptureChecks(Action<string, bool> check)
    {
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan missingFrameEBP =
            Project(freshFrameResetEBP: null, freshPopEBPStackDWORD: 0xABCDEF01);
        check("Missing fresh EBP stops at F1 without resetting ESP or reading the stack",
            missingFrameEBP.ParentPlan.ExactCleanCompletion
            && missingFrameEBP.SuffixAdmitted
            && missingFrameEBP.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
                    .MissingFreshFrameResetEBPAt34F1
            && missingFrameEBP.FreshFrameResetEBPCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState.Missing
            && missingFrameEBP.FreshPopEBPStackCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState
                    .SuppliedButUnused
            && missingFrameEBP.FrameReset is {
                Attempted: true,
                Reached: true,
                Committed: false,
                EBPValue: null,
                ESPBefore: var before,
                ESPAfter: var after
            }
            && before == missingFrameEBP.ParentPlan.ResultESP
            && after == before
            && missingFrameEBP.StackRead is null
            && missingFrameEBP.Pop is null
            && missingFrameEBP.ResultESP == missingFrameEBP.InitialESP
            && missingFrameEBP.LastReachedRawFileOffset == 0x00BD34F1
            && missingFrameEBP.StopBeforeRawFileOffset == 0x00BD34F1
            && missingFrameEBP.ExactBoundedCompletion
            && !missingFrameEBP.ExactCleanCompletion
            && missingFrameEBP.ESPAdvancedForCommittedPop
            && missingFrameEBP.FlagsPreserved);

        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan missingStackDWORD =
            Project(freshFrameResetEBP: 0x7100, freshPopEBPStackDWORD: null);
        check("A missing fresh POP DWORD retains the committed frame reset and stops at F3",
            missingStackDWORD.ParentPlan.ExactCleanCompletion
            && missingStackDWORD.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
                    .MissingFreshPopEBPStackDWORDAt34F3
            && missingStackDWORD.FreshFrameResetEBPCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState.Captured
            && missingStackDWORD.FreshPopEBPStackCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState.Missing
            && missingStackDWORD.FrameReset is {
                EBPValue: 0x7100,
                ESPAfter: 0x7100,
                Committed: true
            }
            && missingStackDWORD.StackRead is {
                InstructionRawFileOffset: 0x00BD34F3,
                Address: 0x7100,
                Width: 4,
                Value: null,
                Attempted: true,
                Captured: false,
                NativeReadExecuted: false
            }
            && missingStackDWORD.Pop is {
                StackPointerBefore: 0x7100,
                StackPointerAfter: 0x7100,
                EBPBefore: 0x7100,
                Value: null,
                EBPMovedTo: null,
                Attempted: true,
                Reached: true,
                Committed: false,
                FlagsBefore: null,
                FlagsAfter: null,
                FlagsWritten: false
            }
            && missingStackDWORD.EBPAfterPop is null
            && missingStackDWORD.ResultESP == 0x7100
            && missingStackDWORD.LastReachedRawFileOffset == 0x00BD34F3
            && missingStackDWORD.StopBeforeRawFileOffset == 0x00BD34F3
            && missingStackDWORD.ContinuationBoundary is null
            && missingStackDWORD.ExactBoundedCompletion
            && !missingStackDWORD.ExactCleanCompletion
            && missingStackDWORD.ESPAdvancedForCommittedPop
            && missingStackDWORD.FlagsPreserved);
    }

    private static void RunRejectedParentChecks(Action<string, bool> check)
    {
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan pendingParent =
            Project(
                freshFrameResetEBP: 0x9000,
                freshPopEBPStackDWORD: 1,
                returnedNormally: false);
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan missingVptrParent =
            Project(
                freshFrameResetEBP: 0x9100,
                freshPopEBPStackDWORD: 2,
                freshOperationVptr: null);
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan missingParentRead =
            Project(
                freshFrameResetEBP: 0x9200,
                freshPopEBPStackDWORD: 3,
                freshParentRead: null);
        var rejected = new[] { pendingParent, missingVptrParent, missingParentRead };
        check("Rejected parents retain their own bound and leave both child captures unused",
            rejected.All(plan =>
                !plan.ParentContinuationReached
                && !plan.SuffixAdmitted
                && plan.FrameReset is null
                && plan.StackRead is null
                && plan.Pop is null
                && plan.ContinuationBoundary is null
                && plan.FreshFrameResetEBPCaptureState
                    == GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState
                        .SuppliedButUnused
                && plan.FreshPopEBPStackCaptureState
                    == GameSocketParserDiagnosticCleanupSecondCallFrameResetCaptureState
                        .SuppliedButUnused
                && plan.FlagsAtStop == plan.ParentPlan.FlagsAtStop
                && plan.LastReachedRawFileOffset == plan.ParentPlan.LastReachedRawFileOffset
                && plan.StopBeforeRawFileOffset == plan.ParentPlan.StopBeforeRawFileOffset
                && plan.ExactBoundedCompletion == plan.ParentPlan.ExactBoundedCompletion
                && !plan.ExactCleanCompletion)
            && pendingParent.StopBeforeRawFileOffset == 0x00BD34DF
            && missingVptrParent.StopBeforeRawFileOffset == 0x00BD34DB
            && missingParentRead.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallRegisterPopsOutcome
                    .ParentContinuationNotAdmitted
            && missingParentRead.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && missingParentRead.StopBeforeRawFileOffset == 0x00BD34E2);
    }

    private static void RunReprojectionCheck(
        Action<string, bool> check,
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan accepted)
    {
        GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan replay =
            GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection.Project(
                accepted.Capture);
        check("Reprojection preserves the ordered child effects and semantic parent evidence",
            replay.Outcome == accepted.Outcome
            && replay.ParentPlan.Outcome == accepted.ParentPlan.Outcome
            && replay.ParentPlan.ExactBoundedCompletion
                == accepted.ParentPlan.ExactBoundedCompletion
            && replay.ParentPlan.ExactCleanCompletion
                == accepted.ParentPlan.ExactCleanCompletion
            && replay.ParentPlan.Reads.SequenceEqual(accepted.ParentPlan.Reads)
            && replay.ParentPlan.Pops.SequenceEqual(accepted.ParentPlan.Pops)
            && replay.ParentPlan.ParentPlan.MemoryEffects.SequenceEqual(
                accepted.ParentPlan.ParentPlan.MemoryEffects)
            && replay.FrameReset == accepted.FrameReset
            && replay.StackRead == accepted.StackRead
            && replay.Pop == accepted.Pop
            && replay.InitialESP == accepted.InitialESP
            && replay.ResultESP == accepted.ResultESP
            && replay.FlagsAtStop == accepted.FlagsAtStop
            && replay.LastReachedRawFileOffset == accepted.LastReachedRawFileOffset
            && replay.StopBeforeRawFileOffset == accepted.StopBeforeRawFileOffset
            && replay.OrderedSemanticPrefixPreserved
            && replay.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupSecondCallFrameResetPlan Project(
        uint? freshFrameResetEBP,
        uint? freshPopEBPStackDWORD,
        bool? returnedNormally = true,
        uint? freshOperationVptr = 0x7400,
        uint? freshParentRead = 0xCAFEBABE) =>
        GameSocketParserDiagnosticCleanupSecondCallFrameResetProjection.Project(
            new(
                CreateParentCapture(returnedNormally, freshOperationVptr, freshParentRead),
                freshFrameResetEBP,
                freshPopEBPStackDWORD));

    private static GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCapture
        CreateParentCapture(
            bool? returnedNormally,
            uint? freshOperationVptr,
            uint? freshParentRead)
    {
        GameSocketParserDiagnosticCleanupStackPrefixCapture stackPrefixCapture =
            CreateStackPrefixCapture();
        GameSocketParserDiagnosticCleanupZeroBranchCapture zeroBranchCapture =
            new(stackPrefixCapture);
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
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture secondCallReturn =
            GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection.Project(
                new(secondCallPreparation, 0x87654321, returnedNormally)).Capture;
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreCapture fsRestoreCapture =
            new(secondCallReturn, 0x8000, freshParentRead);
        return new(fsRestoreCapture, 0x10, 0x20, 0x30, 0x40);
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
