using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupSecondBranchFrameReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .CoordinatorProvidedBytes);
        check("IDA confirms MOV ESP,EBP / POP EBP / RETN 8 at raw F1 without executing the return",
            GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .BytesIndependentlyVerified
            && suppliedBytes.SequenceEqual(
                GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection.WindowBytes)
            && GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .MoveEspEbpRawFileOffset == 0x00BD34F1
            && GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .MoveEspEbpEncoding == "8B E5"
            && GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .PopEBPRawFileOffset == 0x00BD34F3
            && GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .PopEBPEncoding == "5D"
            && GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .Ret8RawFileOffset == 0x00BD34F4
            && GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .Ret8Encoding == "C2 08 00"
            && suppliedBytes.SequenceEqual(ParseBytes("8B E5 5D C2 08 00"))
            && GameSocketParserCountConsumptionStoresProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BD34F1) == 0x00FD40F1
            && !GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .ExecutesNativeInstructions
            && !GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .ExecutesNativeReads
            && !GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .ExecutesReturnTarget
            && !GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection
                .IdentifiesCallerContinuation);

        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPlan wrapped =
            Project(
                freshFrameResetEBP: 0xFFFFFFF8,
                freshPopEBPStackDWORD: 0x12345678,
                freshReturnTargetDWORD: 0);
        check("Fresh frame, saved EBP, and RET target reads remain distinct with wrapping +16 ESP",
            wrapped.ParentPlan.ExactCleanCompletion
            && wrapped.ParentPlan.DECFlagsPreserved
            && wrapped.ExactCleanCompletion
            && wrapped.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                    .LogicalRet8CompletedAt34F4
            && wrapped.FrameReset is {
                InstructionRawFileOffset: 0x00BD34F1,
                Encoding: "8B E5",
                EBPValue: 0xFFFFFFF8,
                ESPBefore: var espBefore,
                ESPAfter: 0xFFFFFFF8,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsBefore: var resetFlags,
                FlagsAfter: var resetFlagsAfter,
                FlagsWritten: false
            }
            && espBefore == wrapped.ParentPlan.ResultESP
            && resetFlags == wrapped.ParentPlan.FlagsAtStop
            && resetFlagsAfter == resetFlags
            && wrapped.SavedEBPRead is {
                InstructionRawFileOffset: 0x00BD34F3,
                Address: 0xFFFFFFF8,
                Width: 4,
                Value: 0x12345678,
                Attempted: true,
                Captured: true,
                NativeReadExecuted: false
            }
            && wrapped.SavedEBPPop is {
                InstructionRawFileOffset: 0x00BD34F3,
                Encoding: "5D",
                StackPointerBefore: 0xFFFFFFF8,
                StackPointerAfter: 0xFFFFFFFC,
                EBPBefore: 0xFFFFFFF8,
                Value: 0x12345678,
                EBPMovedTo: 0x12345678,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsBefore: var popFlags,
                FlagsAfter: var popFlagsAfter,
                FlagsWritten: false
            }
            && popFlags == wrapped.ParentPlan.FlagsAtStop
            && popFlagsAfter == popFlags
            && wrapped.ReturnTargetRead is {
                InstructionRawFileOffset: 0x00BD34F4,
                Address: 0xFFFFFFFC,
                Width: 4,
                Value: 0,
                Attempted: true,
                Captured: true,
                NativeReadExecuted: false
            }
            && wrapped.ReturnInstruction is {
                InstructionRawFileOffset: 0x00BD34F4,
                Encoding: "C2 08 00",
                StackPointerBefore: 0xFFFFFFFC,
                StackPointerAfter: 8,
                TargetVirtualAddress: 0,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsBefore: var retFlags,
                FlagsAfter: var retFlagsAfter,
                FlagsWritten: false,
                TargetExecuted: false,
                CallerContinuationIdentified: false
            }
            && retFlags == wrapped.ParentPlan.FlagsAtStop
            && retFlagsAfter == retFlags
            && wrapped.InitialESP == wrapped.ParentPlan.ResultESP
            && wrapped.ResultESP == 8
            && wrapped.FinalESPFromResetEBP == 8
            && wrapped.FreshFrameResetEBP == 0xFFFFFFF8
            && wrapped.FreshPopEBPStackDWORD == 0x12345678
            && wrapped.FreshReturnTargetDWORDAtResetESPPlus4 == 0
            && wrapped.ReturnTargetVirtualAddress == 0
            && wrapped.EBPAfterPop == 0x12345678
            && wrapped.RestoredRegisters is {
                EBP: 0x12345678,
                ECX: var ecx,
                EDI: var edi,
                ESI: var esi,
                EBX: var ebx
            }
            && ecx == wrapped.ParentPlan.ECXAfterPop
            && edi == wrapped.ParentPlan.EDIAfterPop
            && esi == wrapped.ParentPlan.ESIAfterPop
            && ebx == wrapped.ParentPlan.EBXAfterPop
            && wrapped.ReturnInstruction.Value.RegistersBefore
                == wrapped.ReturnInstruction.Value.RegistersAfter
            && wrapped.LastReachedRawFileOffset == 0x00BD34F4
            && wrapped.StopBeforeRawFileOffset is null
            && wrapped.ChildBoundedCompletionBoundaryReached
            && wrapped.ESPResetFromFreshEBP
            && wrapped.ESPAdvancedForCommittedEffects
            && wrapped.DECFlagsPreserved
            && wrapped.FlagsAtStop == wrapped.ParentPlan.FlagsAtStop
            && wrapped.RegistersPreservedByRet
            && !wrapped.NativeInstructionsExecuted
            && !wrapped.NativeReadsExecuted
            && !wrapped.RuntimeSampleExecuted
            && !wrapped.ParentInstructionsReplayed
            && !wrapped.ReturnTargetExecuted
            && !wrapped.CallerContinuationIdentified
            && !wrapped.NativeABIOrProtocolSemanticsModeled);

        RunMissingCaptureChecks(check);
        RunRejectedParentChecks(check);
        RunReprojectionCheck(check, wrapped);
    }

    private static void RunMissingCaptureChecks(Action<string, bool> check)
    {
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPlan missingFrameEBP =
            Project(
                freshFrameResetEBP: null,
                freshPopEBPStackDWORD: 0x11111111,
                freshReturnTargetDWORD: 0x22222222);
        check("Missing fresh EBP stops at F1 and leaves later stack captures unused",
            missingFrameEBP.ParentPlan.ExactCleanCompletion
            && missingFrameEBP.SuffixAdmitted
            && missingFrameEBP.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                    .MissingFreshFrameResetEBPAt34F1
            && missingFrameEBP.FreshFrameResetEBPCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState.Missing
            && missingFrameEBP.FreshPopEBPStackCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && missingFrameEBP.FreshReturnTargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && missingFrameEBP.FrameReset is {
                Attempted: true,
                Reached: true,
                Committed: false,
                ESPBefore: var before,
                ESPAfter: var after
            }
            && before == missingFrameEBP.ParentPlan.ResultESP
            && after == before
            && missingFrameEBP.SavedEBPRead is null
            && missingFrameEBP.SavedEBPPop is null
            && missingFrameEBP.ReturnTargetRead is null
            && missingFrameEBP.ReturnInstruction is null
            && missingFrameEBP.ResultESP == missingFrameEBP.InitialESP
            && missingFrameEBP.LastReachedRawFileOffset == 0x00BD34F1
            && missingFrameEBP.StopBeforeRawFileOffset == 0x00BD34F1
            && missingFrameEBP.ExactBoundedCompletion
            && !missingFrameEBP.ExactCleanCompletion
            && missingFrameEBP.DECFlagsPreserved);

        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPlan missingSavedEBP =
            Project(
                freshFrameResetEBP: 0x7100,
                freshPopEBPStackDWORD: null,
                freshReturnTargetDWORD: 0x33333333);
        check("Missing saved EBP preserves the F1 reset and stops at the F3 read",
            missingSavedEBP.ParentPlan.ExactCleanCompletion
            && missingSavedEBP.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                    .MissingFreshPopEBPStackDWORDAt34F3
            && missingSavedEBP.FrameReset is {
                EBPValue: 0x7100,
                ESPAfter: 0x7100,
                Committed: true
            }
            && missingSavedEBP.SavedEBPRead is {
                InstructionRawFileOffset: 0x00BD34F3,
                Address: 0x7100,
                Width: 4,
                Value: null,
                Attempted: true,
                Captured: false,
                NativeReadExecuted: false
            }
            && missingSavedEBP.SavedEBPPop is {
                StackPointerBefore: 0x7100,
                StackPointerAfter: 0x7100,
                EBPBefore: 0x7100,
                Value: null,
                EBPMovedTo: null,
                Attempted: true,
                Reached: true,
                Committed: false,
                FlagsWritten: false
            }
            && missingSavedEBP.FreshReturnTargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && missingSavedEBP.ReturnTargetRead is null
            && missingSavedEBP.ReturnInstruction is null
            && missingSavedEBP.ResultESP == 0x7100
            && missingSavedEBP.LastReachedRawFileOffset == 0x00BD34F3
            && missingSavedEBP.StopBeforeRawFileOffset == 0x00BD34F3
            && missingSavedEBP.ExactBoundedCompletion
            && !missingSavedEBP.ExactCleanCompletion
            && missingSavedEBP.DECFlagsPreserved);

        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPlan missingTarget =
            Project(
                freshFrameResetEBP: 0x7200,
                freshPopEBPStackDWORD: 0x44444444,
                freshReturnTargetDWORD: null);
        check("Missing RET target retains both earlier commits and stops at F4",
            missingTarget.ParentPlan.ExactCleanCompletion
            && missingTarget.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnOutcome
                    .MissingFreshReturnTargetDWORDAt34F4
            && missingTarget.FrameReset is { Committed: true, ESPAfter: 0x7200 }
            && missingTarget.SavedEBPRead is { Address: 0x7200, Captured: true }
            && missingTarget.SavedEBPPop is {
                StackPointerBefore: 0x7200,
                StackPointerAfter: 0x7204,
                Committed: true
            }
            && missingTarget.ReturnTargetRead is {
                InstructionRawFileOffset: 0x00BD34F4,
                Address: 0x7204,
                Width: 4,
                Value: null,
                Attempted: true,
                Captured: false,
                NativeReadExecuted: false
            }
            && missingTarget.ReturnInstruction is {
                StackPointerBefore: 0x7204,
                StackPointerAfter: 0x7204,
                TargetVirtualAddress: null,
                Attempted: true,
                Reached: true,
                Committed: false,
                TargetExecuted: false,
                CallerContinuationIdentified: false
            }
            && missingTarget.FreshReturnTargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState.Missing
            && missingTarget.ReturnTargetVirtualAddress is null
            && missingTarget.ResultESP == 0x7204
            && missingTarget.LastReachedRawFileOffset == 0x00BD34F4
            && missingTarget.StopBeforeRawFileOffset == 0x00BD34F4
            && !missingTarget.ChildBoundedCompletionBoundaryReached
            && missingTarget.ExactBoundedCompletion
            && !missingTarget.ExactCleanCompletion
            && missingTarget.DECFlagsPreserved
            && missingTarget.RegistersPreservedByRet);
    }

    private static void RunRejectedParentChecks(Action<string, bool> check)
    {
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPlan skippedSecondCall =
            Project(
                freshFrameResetEBP: 0x9000,
                freshPopEBPStackDWORD: 1,
                freshReturnTargetDWORD: 2,
                secondXaddValue: 1);
        check("A skipped second-call path keeps DEC flags and leaves all frame captures unused",
            skippedSecondCall.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                    .BranchNotTakenTo34DB
            && !skippedSecondCall.ParentContinuationReached
            && !skippedSecondCall.SuffixAdmitted
            && skippedSecondCall.FrameReset is null
            && skippedSecondCall.SavedEBPRead is null
            && skippedSecondCall.SavedEBPPop is null
            && skippedSecondCall.ReturnTargetRead is null
            && skippedSecondCall.ReturnInstruction is null
            && skippedSecondCall.FreshFrameResetEBPCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && skippedSecondCall.FreshPopEBPStackCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && skippedSecondCall.FreshReturnTargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && skippedSecondCall.FlagsAtStop == skippedSecondCall.ParentPlan.FlagsAtStop
            && skippedSecondCall.DECFlagsPreserved
            && skippedSecondCall.FlagsAtStop
                == skippedSecondCall.ParentPlan.ParentPlan.ParentPlan.FlagsAtStop
            && skippedSecondCall.LastReachedRawFileOffset
                == skippedSecondCall.ParentPlan.LastReachedRawFileOffset
            && skippedSecondCall.StopBeforeRawFileOffset
                == skippedSecondCall.ParentPlan.StopBeforeRawFileOffset
            && skippedSecondCall.ExactBoundedCompletion
                == skippedSecondCall.ParentPlan.ExactBoundedCompletion
            && !skippedSecondCall.ExactCleanCompletion);
    }

    private static void RunReprojectionCheck(
        Action<string, bool> check,
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPlan accepted)
    {
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPlan replay =
            GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection.Project(
                accepted.Capture);
        check("Reprojection preserves immutable captures, parent POPs, DEC flags, and ordered effects",
            replay.Capture == accepted.Capture
            && replay.Outcome == accepted.Outcome
            && replay.ParentPlan.Capture == replay.Capture.ParentCapture
            && replay.ParentPlan.ParentPlan.Capture
                == replay.Capture.ParentCapture.ParentCapture
            && replay.ParentPlan.ParentPlan.ParentPlan.Capture
                == replay.Capture.ParentCapture.ParentCapture.ParentCapture
            && replay.ParentPlan.Outcome == accepted.ParentPlan.Outcome
            && replay.ParentPlan.ExactBoundedCompletion
                == accepted.ParentPlan.ExactBoundedCompletion
            && replay.ParentPlan.ExactCleanCompletion
                == accepted.ParentPlan.ExactCleanCompletion
            && replay.ParentPlan.Reads.SequenceEqual(accepted.ParentPlan.Reads)
            && replay.ParentPlan.Pops.SequenceEqual(accepted.ParentPlan.Pops)
            && replay.ParentPlan.FlagsAtStop == accepted.ParentPlan.FlagsAtStop
            && replay.FrameReset == accepted.FrameReset
            && replay.SavedEBPRead == accepted.SavedEBPRead
            && replay.SavedEBPPop == accepted.SavedEBPPop
            && replay.ReturnTargetRead == accepted.ReturnTargetRead
            && replay.ReturnInstruction == accepted.ReturnInstruction
            && replay.InitialESP == accepted.InitialESP
            && replay.ResultESP == accepted.ResultESP
            && replay.FlagsAtStop == accepted.FlagsAtStop
            && replay.LastReachedRawFileOffset == accepted.LastReachedRawFileOffset
            && replay.StopBeforeRawFileOffset == accepted.StopBeforeRawFileOffset
            && replay.OrderedSemanticPrefixPreserved
            && replay.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupSecondBranchFrameReturnPlan Project(
        uint? freshFrameResetEBP,
        uint? freshPopEBPStackDWORD,
        uint? freshReturnTargetDWORD,
        uint secondXaddValue = 2) =>
        GameSocketParserDiagnosticCleanupSecondBranchFrameReturnProjection.Project(
            new(
                CreateParentCapture(secondXaddValue),
                freshFrameResetEBP,
                freshPopEBPStackDWORD,
                freshReturnTargetDWORD));

    private static GameSocketParserDiagnosticCleanupSecondBranchRegisterPopsCapture
        CreateParentCapture(uint secondXaddValue)
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
            new(new(addressCapture, 0x1234, secondXaddValue));
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCapture branchFsRestore =
            new(
                secondXaddBranchCapture,
                0xFFFFFFFC,
                0xCAFEBABE);
        return new(branchFsRestore, 0x10, 0x20, 0x30, 0x40);
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
