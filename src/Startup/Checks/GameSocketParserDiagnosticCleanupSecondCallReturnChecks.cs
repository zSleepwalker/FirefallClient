using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupSecondCallReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupSecondCallReturnProjection
                .CoordinatorProvidedBytes);
        check("IDA confirms the RETN 8 bytes at BD34F4 and the projection stops at that return",
            GameSocketParserDiagnosticCleanupSecondCallReturnProjection
                .BytesIndependentlyVerified
            && suppliedBytes.SequenceEqual(
                GameSocketParserDiagnosticCleanupSecondCallReturnProjection.WindowBytes)
            && GameSocketParserDiagnosticCleanupSecondCallReturnProjection
                .Ret8RawFileOffset == 0x00BD34F4
            && GameSocketParserDiagnosticCleanupSecondCallReturnProjection
                .Ret8Encoding == "C2 08 00"
            && suppliedBytes.SequenceEqual(ParseBytes("C2 08 00"))
            && GameSocketParserDiagnosticCleanupSecondCallReturnProjection
                .Ret8StackAdvance == 12
            && !GameSocketParserDiagnosticCleanupSecondCallReturnProjection
                .ExecutesNativeInstructions
            && !GameSocketParserDiagnosticCleanupSecondCallReturnProjection
                .ExecutesNativeReads
            && !GameSocketParserDiagnosticCleanupSecondCallReturnProjection
                .ExecutesReturnTarget
            && !GameSocketParserDiagnosticCleanupSecondCallReturnProjection
                .IdentifiesCallerContinuation);

        GameSocketParserDiagnosticCleanupSecondCallReturnPlan normal =
            Project(returnTargetDWORD: 0x12345678);
        check("RET reads its target at the parent's current ESP and applies wrapping +12",
            normal.ParentPlan.ExactCleanCompletion
            && normal.ExactCleanCompletion
            && normal.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallReturnOutcome
                    .LogicalRet8CompletedAt34F4
            && normal.TargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState.Captured
            && normal.TargetRead is {
                InstructionRawFileOffset: 0x00BD34F4,
                Address: var address,
                Width: 4,
                Value: 0x12345678,
                Attempted: true,
                Captured: true,
                NativeReadExecuted: false
            }
            && address == normal.ParentPlan.ResultESP
            && normal.ReturnInstruction is {
                InstructionRawFileOffset: 0x00BD34F4,
                Encoding: "C2 08 00",
                StackPointerBefore: var before,
                StackPointerAfter: var after,
                TargetVirtualAddress: 0x12345678,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsWritten: false,
                TargetExecuted: false,
                CallerContinuationIdentified: false
            }
            && before == normal.ParentPlan.ResultESP
            && after == unchecked(before!.Value + 12)
            && normal.InitialESP == normal.ParentPlan.ResultESP
            && normal.ResultESP == after
            && normal.FreshReturnTargetDWORD == 0x12345678
            && normal.ReturnTargetVirtualAddress == 0x12345678
            && normal.LastReachedRawFileOffset == 0x00BD34F4
            && normal.StopBeforeRawFileOffset is null
            && normal.ChildBoundedCompletionBoundaryReached
            && normal.FlagsPreserved
            && normal.RegistersPreserved
            && normal.RestoredRegisters is {
                EBP: 0x12345678,
                ECX: 0x10,
                EDI: 0x20,
                ESI: 0x30,
                EBX: 0x40
            }
            && normal.ReturnInstruction.Value.RegistersBefore
                == normal.ReturnInstruction.Value.RegistersAfter
            && !normal.NativeInstructionsExecuted
            && !normal.NativeReadsExecuted
            && !normal.RuntimeSampleExecuted
            && !normal.ParentInstructionsReplayed
            && !normal.ReturnTargetExecuted
            && !normal.CallerContinuationIdentified
            && !normal.NativeABIOrProtocolSemanticsModeled);

        GameSocketParserDiagnosticCleanupSecondCallReturnPlan wrappedESP =
            Project(
                returnTargetDWORD: 0,
                freshFrameResetEBP: 0xFFFFFFF8,
                freshPopEBPStackDWORD: 0xAABBCCDD);
        check("RET accepts a captured zero target as a scalar and wraps FFFFFFFC plus twelve",
            wrappedESP.ExactCleanCompletion
            && wrappedESP.InitialESP == 0xFFFFFFFC
            && wrappedESP.TargetRead is { Address: 0xFFFFFFFC, Value: 0 }
            && wrappedESP.ReturnTargetVirtualAddress == 0
            && wrappedESP.ReturnInstruction is {
                StackPointerBefore: 0xFFFFFFFC,
                StackPointerAfter: 8,
                TargetVirtualAddress: 0,
                Committed: true,
                TargetExecuted: false
            }
            && wrappedESP.ResultESP == 8
            && wrappedESP.FlagsPreserved
            && wrappedESP.RegistersPreserved);

        RunMissingTargetCheck(check);
        RunRejectedParentChecks(check);
        RunReprojectionCheck(check, normal);
    }

    private static void RunMissingTargetCheck(Action<string, bool> check)
    {
        GameSocketParserDiagnosticCleanupSecondCallReturnPlan missing =
            Project(returnTargetDWORD: null);
        check("A missing fresh return target leaves ESP and restored state unchanged at RET",
            missing.ParentPlan.ExactCleanCompletion
            && missing.SuffixAdmitted
            && missing.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallReturnOutcome
                    .MissingFreshReturnTargetDWORDAt34F4
            && missing.TargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState.Missing
            && missing.TargetRead is {
                InstructionRawFileOffset: 0x00BD34F4,
                Address: var address,
                Width: 4,
                Value: null,
                Attempted: true,
                Captured: false,
                NativeReadExecuted: false
            }
            && address == missing.ParentPlan.ResultESP
            && missing.ReturnInstruction is {
                StackPointerBefore: var before,
                StackPointerAfter: var after,
                TargetVirtualAddress: null,
                Attempted: true,
                Reached: true,
                Committed: false,
                FlagsWritten: false,
                TargetExecuted: false
            }
            && before == missing.InitialESP
            && after == before
            && missing.ResultESP == missing.InitialESP
            && missing.LastReachedRawFileOffset == 0x00BD34F4
            && missing.StopBeforeRawFileOffset == 0x00BD34F4
            && missing.ExactBoundedCompletion
            && !missing.ExactCleanCompletion
            && missing.FlagsPreserved
            && missing.RegistersPreserved
            && !missing.ChildBoundedCompletionBoundaryReached);
    }

    private static void RunRejectedParentChecks(Action<string, bool> check)
    {
        GameSocketParserDiagnosticCleanupSecondCallReturnPlan pendingParent =
            Project(returnTargetDWORD: 0x11111111, secondCallReturnedNormally: null);
        GameSocketParserDiagnosticCleanupSecondCallReturnPlan rejectedCall =
            Project(returnTargetDWORD: 0x12121212, secondCallReturnedNormally: false);
        GameSocketParserDiagnosticCleanupSecondCallReturnPlan missingParentFrameEBP =
            Project(returnTargetDWORD: 0x22222222, freshFrameResetEBP: null);
        GameSocketParserDiagnosticCleanupSecondCallReturnPlan missingParentPopDWORD =
            Project(returnTargetDWORD: 0x33333333, freshPopEBPStackDWORD: null);
        GameSocketParserDiagnosticCleanupSecondCallReturnPlan rejectedParentRead =
            Project(returnTargetDWORD: 0x44444444, freshParentRead: null);
        GameSocketParserDiagnosticCleanupSecondCallReturnPlan[] rejected =
        [
            pendingParent,
            rejectedCall,
            missingParentFrameEBP,
            missingParentPopDWORD,
            rejectedParentRead
        ];
        check("Pending and incomplete parents retain their bounds and leave RET captures unused",
            rejected.All(plan =>
                !plan.SuffixAdmitted
                && plan.TargetRead is null
                && plan.ReturnInstruction is null
                && plan.ChildBoundedCompletionBoundaryReached == false
                && plan.TargetCaptureState
                    == GameSocketParserDiagnosticCleanupSecondCallReturnCaptureState
                        .SuppliedButUnused
                && plan.FlagsAtStop == plan.ParentPlan.FlagsAtStop
                && plan.LastReachedRawFileOffset == plan.ParentPlan.LastReachedRawFileOffset
                && plan.StopBeforeRawFileOffset == plan.ParentPlan.StopBeforeRawFileOffset
                && plan.ExactBoundedCompletion == plan.ParentPlan.ExactBoundedCompletion
                && !plan.ExactCleanCompletion)
            && pendingParent.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallFrameResetOutcome
                    .ParentContinuationNotAdmitted
            && pendingParent.ParentPlan.ParentPlan.ParentPlan.ParentPlan.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState.Missing
            && rejectedCall.ParentPlan.ParentPlan.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                    .OpaqueCallDidNotReturnNormally
            && missingParentFrameEBP.ParentPlan.StopBeforeRawFileOffset == 0x00BD34F1
            && missingParentPopDWORD.ParentPlan.StopBeforeRawFileOffset == 0x00BD34F3
            && rejectedParentRead.ParentPlan.StopBeforeRawFileOffset
                == rejectedParentRead.ParentPlan.ParentPlan.StopBeforeRawFileOffset);
    }

    private static void RunReprojectionCheck(
        Action<string, bool> check,
        GameSocketParserDiagnosticCleanupSecondCallReturnPlan accepted)
    {
        GameSocketParserDiagnosticCleanupSecondCallReturnPlan replay =
            GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Project(
                accepted.Capture);
        check("Reprojection preserves the ordered return effect and semantic parent evidence",
            replay.Outcome == accepted.Outcome
            && replay.ParentPlan.Outcome == accepted.ParentPlan.Outcome
            && replay.ParentPlan.ExactBoundedCompletion
                == accepted.ParentPlan.ExactBoundedCompletion
            && replay.ParentPlan.ExactCleanCompletion
                == accepted.ParentPlan.ExactCleanCompletion
            && replay.ParentPlan.FrameReset == accepted.ParentPlan.FrameReset
            && replay.ParentPlan.StackRead == accepted.ParentPlan.StackRead
            && replay.ParentPlan.Pop == accepted.ParentPlan.Pop
            && replay.ParentPlan.ContinuationBoundary
                == accepted.ParentPlan.ContinuationBoundary
            && replay.ParentPlan.InitialESP == accepted.ParentPlan.InitialESP
            && replay.ParentPlan.ResultESP == accepted.ParentPlan.ResultESP
            && replay.ParentPlan.FlagsAtStop == accepted.ParentPlan.FlagsAtStop
            && replay.ParentPlan.LastReachedRawFileOffset
                == accepted.ParentPlan.LastReachedRawFileOffset
            && replay.ParentPlan.StopBeforeRawFileOffset
                == accepted.ParentPlan.StopBeforeRawFileOffset
            && replay.ParentPlan.ParentPlan.Reads.SequenceEqual(
                accepted.ParentPlan.ParentPlan.Reads)
            && replay.ParentPlan.ParentPlan.Pops.SequenceEqual(
                accepted.ParentPlan.ParentPlan.Pops)
            && replay.ParentPlan.ParentPlan.ParentPlan.MemoryEffects.SequenceEqual(
                accepted.ParentPlan.ParentPlan.ParentPlan.MemoryEffects)
            && replay.TargetRead == accepted.TargetRead
            && replay.ReturnInstruction == accepted.ReturnInstruction
            && replay.InitialESP == accepted.InitialESP
            && replay.ResultESP == accepted.ResultESP
            && replay.FlagsAtStop == accepted.FlagsAtStop
            && replay.LastReachedRawFileOffset == accepted.LastReachedRawFileOffset
            && replay.StopBeforeRawFileOffset == accepted.StopBeforeRawFileOffset
            && replay.OrderedSemanticPrefixPreserved
            && replay.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupSecondCallReturnPlan Project(
        uint? returnTargetDWORD,
        uint? freshFrameResetEBP = 0x2000,
        uint? freshPopEBPStackDWORD = 0x12345678,
        bool? secondCallReturnedNormally = true,
        uint? freshOperationVptr = 0x7400,
        uint? freshParentRead = 0xCAFEBABE)
    {
        GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCapture registerPopsCapture =
            CreateRegisterPopsCapture(
                secondCallReturnedNormally,
                freshOperationVptr,
                freshParentRead);
        GameSocketParserDiagnosticCleanupSecondCallFrameResetCapture frameResetCapture =
            new(registerPopsCapture, freshFrameResetEBP, freshPopEBPStackDWORD);
        return GameSocketParserDiagnosticCleanupSecondCallReturnProjection.Project(
            new(frameResetCapture, returnTargetDWORD));
    }

    private static GameSocketParserDiagnosticCleanupSecondCallRegisterPopsCapture
        CreateRegisterPopsCapture(
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
