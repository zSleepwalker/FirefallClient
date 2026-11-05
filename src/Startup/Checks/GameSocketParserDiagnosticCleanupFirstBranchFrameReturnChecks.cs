using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupFirstBranchFrameReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .CoordinatorProvidedBytes);
        check("First-branch F1 bytes anchor MOV ESP,EBP, POP EBP, and RETN 8",
            GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .BytesIndependentlyVerified
            && suppliedBytes.SequenceEqual(
                GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection.WindowBytes)
            && suppliedBytes.SequenceEqual(ParseBytes("8B E5 5D C2 08 00"))
            && GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .MoveEspEbpRawFileOffset == 0x00BD34F1
            && GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .MoveEspEbpEncoding == "8B E5"
            && GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .PopEBPRawFileOffset == 0x00BD34F3
            && GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .PopEBPEncoding == "5D"
            && GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .Ret8RawFileOffset == 0x00BD34F4
            && GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .Ret8Encoding == "C2 08 00"
            && GameSocketParserCountConsumptionStoresProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BD34F1) == 0x00FD40F1
            && GameSocketParserCountConsumptionStoresProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BD34F3) == 0x00FD40F3
            && GameSocketParserCountConsumptionStoresProjection
                .RawFileOffsetToCodeVirtualAddress(0x00BD34F4) == 0x00FD40F4
            && GameSocketParserCountConsumptionStoresProjection
                .RawFileOffsetToCodeVirtualAddress(
                    GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                        .Ret8EndRawFileOffset) == 0x00FD40F7
            && GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .Ret8EndRawFileOffset == 0x00BD34F7
            && !GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .ExecutesNativeInstructions
            && !GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .ExecutesNativeReads
            && !GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .ExecutesReturnTarget
            && !GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection
                .IdentifiesCallerContinuation);

        GameSocketParserDiagnosticCleanupFirstBranchFrameReturnPlan wrapped =
            Project(
                freshFrameResetEBP: 0xFFFFFFF8,
                freshPopEBPStackDWORD: 0x12345678,
                freshReturnTargetDWORD: 0);
        check("Fresh frame, saved EBP, and logical RETN 8 preserve first-ADD flags and wrap ESP",
            wrapped.ParentPlan.ExactCleanCompletion
            && wrapped.ParentPlan.FirstXaddAddFlagsPreserved
            && wrapped.ExactCleanCompletion
            && wrapped.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnOutcome
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
            && wrapped.FirstXaddAddFlagsPreserved
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
        GameSocketParserDiagnosticCleanupFirstBranchFrameReturnPlan missingFrameEBP =
            Project(
                freshFrameResetEBP: null,
                freshPopEBPStackDWORD: 0x11111111,
                freshReturnTargetDWORD: 0x22222222);
        check("Missing frame-reset EBP stops at F1 and leaves later captures unused",
            missingFrameEBP.ParentPlan.ExactCleanCompletion
            && missingFrameEBP.SuffixAdmitted
            && missingFrameEBP.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnOutcome
                    .MissingFreshFrameResetEBPAt34F1
            && missingFrameEBP.FreshFrameResetEBPCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState.Missing
            && missingFrameEBP.FreshPopEBPStackCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && missingFrameEBP.FreshReturnTargetCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState
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
            && missingFrameEBP.FirstXaddAddFlagsPreserved);

        GameSocketParserDiagnosticCleanupFirstBranchFrameReturnPlan unreachedCaptures =
            Project(
                freshFrameResetEBP: null,
                freshPopEBPStackDWORD: null,
                freshReturnTargetDWORD: null);
        check("Captures beyond a missing F1 frame reset remain not reached when unsupplied",
            unreachedCaptures.FreshFrameResetEBPCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState.Missing
            && unreachedCaptures.FreshPopEBPStackCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState.NotReached
            && unreachedCaptures.FreshReturnTargetCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState.NotReached
            && unreachedCaptures.SavedEBPRead is null
            && unreachedCaptures.SavedEBPPop is null
            && unreachedCaptures.ReturnTargetRead is null
            && unreachedCaptures.ReturnInstruction is null
            && unreachedCaptures.StopBeforeRawFileOffset == 0x00BD34F1);

        GameSocketParserDiagnosticCleanupFirstBranchFrameReturnPlan missingSavedEBP =
            Project(
                freshFrameResetEBP: 0x7100,
                freshPopEBPStackDWORD: null,
                freshReturnTargetDWORD: 0x33333333);
        check("Missing saved EBP preserves the F1 reset and stops at the F3 read",
            missingSavedEBP.ParentPlan.ExactCleanCompletion
            && missingSavedEBP.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnOutcome
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
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && missingSavedEBP.ReturnTargetRead is null
            && missingSavedEBP.ReturnInstruction is null
            && missingSavedEBP.ResultESP == 0x7100
            && missingSavedEBP.LastReachedRawFileOffset == 0x00BD34F3
            && missingSavedEBP.StopBeforeRawFileOffset == 0x00BD34F3
            && missingSavedEBP.ExactBoundedCompletion
            && !missingSavedEBP.ExactCleanCompletion
            && missingSavedEBP.FirstXaddAddFlagsPreserved);

        GameSocketParserDiagnosticCleanupFirstBranchFrameReturnPlan missingTarget =
            Project(
                freshFrameResetEBP: 0x7200,
                freshPopEBPStackDWORD: 0x44444444,
                freshReturnTargetDWORD: null);
        check("Missing RET target retains prior commits and stops at F4",
            missingTarget.ParentPlan.ExactCleanCompletion
            && missingTarget.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnOutcome
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
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState.Missing
            && missingTarget.ReturnTargetVirtualAddress is null
            && missingTarget.ResultESP == 0x7204
            && missingTarget.LastReachedRawFileOffset == 0x00BD34F4
            && missingTarget.StopBeforeRawFileOffset == 0x00BD34F4
            && !missingTarget.ChildBoundedCompletionBoundaryReached
            && missingTarget.ExactBoundedCompletion
            && !missingTarget.ExactCleanCompletion
            && missingTarget.FirstXaddAddFlagsPreserved
            && missingTarget.RegistersPreservedByRet);
    }

    private static void RunRejectedParentChecks(Action<string, bool> check)
    {
        GameSocketParserDiagnosticCleanupFirstBranchFrameReturnPlan notTaken =
            Project(
                freshFrameResetEBP: 0x9000,
                freshPopEBPStackDWORD: 1,
                freshReturnTargetDWORD: 2,
                firstRmwDWORD: 1);
        check("A not-taken first-XADD branch leaves every frame capture unused at CB",
            notTaken.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreOutcome
                    .BranchNotTakenTo34CB
            && !notTaken.ParentContinuationReached
            && !notTaken.SuffixAdmitted
            && notTaken.FrameReset is null
            && notTaken.SavedEBPRead is null
            && notTaken.SavedEBPPop is null
            && notTaken.ReturnTargetRead is null
            && notTaken.ReturnInstruction is null
            && notTaken.FreshFrameResetEBPCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && notTaken.FreshPopEBPStackCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && notTaken.FreshReturnTargetCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && notTaken.FlagsAtStop == notTaken.ParentPlan.FlagsAtStop
            && notTaken.LastReachedRawFileOffset
                == notTaken.ParentPlan.LastReachedRawFileOffset
            && notTaken.StopBeforeRawFileOffset
                == notTaken.ParentPlan.StopBeforeRawFileOffset
            && notTaken.ExactBoundedCompletion
                == notTaken.ParentPlan.ExactBoundedCompletion
            && !notTaken.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupFirstBranchFrameReturnPlan partialParent =
            Project(
                freshFrameResetEBP: 0x9100,
                freshPopEBPStackDWORD: 1,
                freshReturnTargetDWORD: 2,
                freshPopECX: 0x10,
                freshPopEDI: null,
                freshPopESI: 0x30,
                freshPopEBX: 0x40);
        check("A partial first-branch POP parent keeps its EE endpoint and rejects the frame",
            !partialParent.ParentContinuationReached
            && !partialParent.SuffixAdmitted
            && partialParent.FrameReset is null
            && partialParent.SavedEBPRead is null
            && partialParent.SavedEBPPop is null
            && partialParent.ReturnTargetRead is null
            && partialParent.ReturnInstruction is null
            && partialParent.FreshFrameResetEBPCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && partialParent.FreshPopEBPStackCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && partialParent.FreshReturnTargetCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFrameReturnCaptureState
                    .SuppliedButUnused
            && partialParent.FlagsAtStop == partialParent.ParentPlan.FlagsAtStop
            && partialParent.LastReachedRawFileOffset
                == partialParent.ParentPlan.LastReachedRawFileOffset
            && partialParent.StopBeforeRawFileOffset == 0x00BD34EE
            && partialParent.StopBeforeRawFileOffset
                == partialParent.ParentPlan.StopBeforeRawFileOffset
            && partialParent.ExactBoundedCompletion
                == partialParent.ParentPlan.ExactBoundedCompletion
            && !partialParent.ExactCleanCompletion);
    }

    private static void RunReprojectionCheck(
        Action<string, bool> check,
        GameSocketParserDiagnosticCleanupFirstBranchFrameReturnPlan accepted)
    {
        GameSocketParserDiagnosticCleanupFirstBranchFrameReturnPlan replay =
            GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection.Project(
                accepted.Capture);
        check("Reprojection preserves first-branch POPs, ADD flags, and ordered return effects",
            replay.Capture == accepted.Capture
            && replay.Outcome == accepted.Outcome
            && replay.ParentPlan.Outcome == accepted.ParentPlan.Outcome
            && replay.ParentPlan.FirstXaddAddFlagsPreserved
                == accepted.ParentPlan.FirstXaddAddFlagsPreserved
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
            && replay.FirstXaddAddFlagsPreserved
            && replay.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupFirstBranchFrameReturnPlan Project(
        uint? freshFrameResetEBP,
        uint? freshPopEBPStackDWORD,
        uint? freshReturnTargetDWORD,
        uint? firstRmwDWORD = 2,
        uint? freshPopECX = 0x10,
        uint? freshPopEDI = 0x20,
        uint? freshPopESI = 0x30,
        uint? freshPopEBX = 0x40) =>
        GameSocketParserDiagnosticCleanupFirstBranchFrameReturnProjection.Project(
            new(
                CreateParentCapture(
                    firstRmwDWORD,
                    freshPopECX,
                    freshPopEDI,
                    freshPopESI,
                    freshPopEBX),
                freshFrameResetEBP,
                freshPopEBPStackDWORD,
                freshReturnTargetDWORD));

    private static GameSocketParserDiagnosticCleanupFirstBranchRegisterPopsCapture
        CreateParentCapture(
            uint? firstRmwDWORD,
            uint? freshPopECX,
            uint? freshPopEDI,
            uint? freshPopESI,
            uint? freshPopEBX)
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
        GameSocketParserDiagnosticCleanupFirstBranchFsRestoreCapture fsRestoreCapture =
            new(xaddBranchCapture, 0x7400, 0xCAFEBABE);
        return new(
            fsRestoreCapture,
            freshPopECX,
            freshPopEDI,
            freshPopESI,
            freshPopEBX);
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
