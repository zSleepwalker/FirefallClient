using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupSecondVirtualCallReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .CoordinatorProvidedBytes);
        check("Fresh IDA bytes anchor the three-byte slot-four call and stop before the continuation MOV",
            GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .BytesIndependentlyVerified
            && GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .Slot4CallRawFileOffset == 0x00BD34DF
            && GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .Slot4CallEncoding == "FF 50 04"
            && GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .ContinuationRawFileOffset == 0x00BD34E2
            && GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .ContinuationEncoding == "8B 4C 24 48"
            && suppliedBytes.Length == 17
            && suppliedBytes.SequenceEqual(
                GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection.WindowBytes)
            && suppliedBytes.Take(3).SequenceEqual(ParseBytes("4B 75 07"))
            && suppliedBytes.Skip(3).Take(2).SequenceEqual(ParseBytes("8B 06"))
            && suppliedBytes.Skip(5).Take(2).SequenceEqual(ParseBytes("8B CE"))
            && suppliedBytes.Skip(7).Take(3).SequenceEqual(ParseBytes("FF 50 04"))
            && suppliedBytes.Skip(10).Take(4).SequenceEqual(ParseBytes("8B 4C 24 48"))
            && !GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .ExecutesNativeInstructions
            && !GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .ExecutesNativeReads
            && !GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .ExecutesMemoryWrites
            && !GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .ExecutesCalls
            && !GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .EntersOpaqueCallTarget
            && !GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection
                .ExecutesContinuationInstruction);

        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan sharedParentPlan =
            GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.Project(
                CreateParentCapture(0x1234, 0x9000, secondRmwDWORD: 1));
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan zeroValues =
            Project(sharedParentPlan, freshOperationVptr: 0, targetDWORD: 0,
                returnedNormally: true);
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan maximumValues =
            Project(
                sharedParentPlan,
                freshOperationVptr: uint.MaxValue,
                targetDWORD: uint.MaxValue,
                returnedNormally: true);
        check("Wrapped vptr-plus-four addresses and zero/maximum target scalars are captured without pointer validation",
            zeroValues.MemoryRead is {
                Address: 4,
                Value: 0,
                Attempted: true,
                Captured: true,
                NativeReadExecuted: false
            }
            && zeroValues.CallBoundary is {
                Slot4TargetAddress: 4,
                TargetDWORD: 0,
                LogicalCallAttempted: true,
                NativeCallExecuted: false
            }
            && zeroValues.ExactCleanCompletion
            && maximumValues.MemoryRead is {
                Address: 3,
                Value: uint.MaxValue,
                Attempted: true,
                Captured: true,
                NativeReadExecuted: false
            }
            && maximumValues.CallBoundary is {
                Slot4TargetAddress: 3,
                TargetDWORD: uint.MaxValue,
                LogicalCallAttempted: true,
                NativeCallExecuted: false
            }
            && maximumValues.ExactCleanCompletion
            && !zeroValues.PointerValidityModeled
            && !maximumValues.PointerValidityModeled
            && !zeroValues.NativeABIOrProtocolSemanticsModeled
            && !maximumValues.NativeABIOrProtocolSemanticsModeled);

        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan missingTarget =
            Project(
                sharedParentPlan,
                freshOperationVptr: 0x7100,
                targetDWORD: null,
                returnedNormally: true);
        check("A missing target read is recorded without a logical call and leaves the supplied return unused",
            missingTarget.Outcome
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                    .MissingSlot4TargetDWORDAt34DF
            && missingTarget.ParentPlan.ExactCleanCompletion
            && missingTarget.TargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState.Missing
            && missingTarget.TargetReadAttempted
            && !missingTarget.TargetReadCaptured
            && missingTarget.MemoryRead is {
                InstructionRawFileOffset: 0x00BD34DF,
                Address: 0x7104,
                Width: 4,
                Value: null,
                Attempted: true,
                Captured: false,
                NativeReadExecuted: false
            }
            && !missingTarget.LogicalCallAttempted
            && missingTarget.CallBoundary is null
            && !missingTarget.ReturnObservationCaptured
            && missingTarget.OpaqueCallReturnedNormally is null
            && missingTarget.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState
                    .SuppliedButUnused
            && missingTarget.FlagsAtStop == missingTarget.ParentPlan.FlagsAtStop
            && missingTarget.LastReachedRawFileOffset == 0x00BD34DF
            && missingTarget.StopBeforeRawFileOffset == 0x00BD34DF
            && !missingTarget.ChildBoundedCompletionBoundaryReached
            && !missingTarget.ExactBoundedCompletion
            && !missingTarget.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan pendingReturn =
            Project(
                sharedParentPlan,
                freshOperationVptr: 0x7200,
                targetDWORD: 0x12345678,
                returnedNormally: null);
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan abnormalReturn =
            Project(
                sharedParentPlan,
                freshOperationVptr: 0x7300,
                targetDWORD: 0x87654321,
                returnedNormally: false);
        check("Pending and abnormal opaque returns stop at the call with the parent's DEC flags",
            pendingReturn.Outcome
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                    .MissingOpaqueCallReturnObservation
            && pendingReturn.TargetReadCaptured
            && pendingReturn.LogicalCallAttempted
            && pendingReturn.CallBoundary is {
                LogicalCallAttempted: true,
                NativeCallExecuted: false
            }
            && pendingReturn.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState.Missing
            && !pendingReturn.ReturnObservationCaptured
            && pendingReturn.OpaqueCallReturnedNormally is null
            && pendingReturn.FlagsAtStop == pendingReturn.ParentPlan.FlagsAtStop
            && pendingReturn.LastReachedRawFileOffset == 0x00BD34DF
            && pendingReturn.StopBeforeRawFileOffset == 0x00BD34DF
            && !pendingReturn.ChildBoundedCompletionBoundaryReached
            && !pendingReturn.ExactBoundedCompletion
            && !pendingReturn.ExactCleanCompletion
            && abnormalReturn.Outcome
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                    .OpaqueCallDidNotReturnNormally
            && abnormalReturn.ReturnObservationCaptured
            && abnormalReturn.OpaqueCallReturnedNormally == false
            && abnormalReturn.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState.Captured
            && abnormalReturn.FlagsAtStop == abnormalReturn.ParentPlan.FlagsAtStop
            && abnormalReturn.ContinuationBoundary is null
            && abnormalReturn.LastReachedRawFileOffset == 0x00BD34DF
            && abnormalReturn.StopBeforeRawFileOffset == 0x00BD34DF
            && !abnormalReturn.ChildBoundedCompletionBoundaryReached
            && !abnormalReturn.ExactBoundedCompletion
            && !abnormalReturn.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan normalReturn =
            Project(
                sharedParentPlan,
                freshOperationVptr: 0x7400,
                targetDWORD: 0x12345678,
                returnedNormally: true);
        check("A normal opaque return stops before MOV, invalidates flags, and forwards no post-call registers",
            normalReturn.Outcome
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                    .OpaqueCallReturnedNormally
            && normalReturn.CallBoundary is {
                InstructionRawFileOffset: 0x00BD34DF,
                Slot4TargetAddress: 0x7404,
                TargetDWORD: 0x12345678,
                PreCallEAX: 0x7400,
                PreCallEBX: 0,
                PreCallECX: var preCallECX,
                PreCallESI: var preCallESI,
                PreCallESP: null,
                LogicalCallAttempted: true,
                NativeCallExecuted: false
            }
            && preCallECX == normalReturn.ParentPlan.ESIValue
            && preCallESI == normalReturn.ParentPlan.ESIValue
            && normalReturn.ContinuationBoundary is {
                InstructionRawFileOffset: 0x00BD34E2,
                NextInstructionEncoding: "8B 4C 24 48",
                Reached: true,
                NextInstructionExecuted: false
            }
            && normalReturn.LastReachedRawFileOffset == 0x00BD34DF
            && normalReturn.StopBeforeRawFileOffset == 0x00BD34E2
            && normalReturn.FlagsAtStop is null
            && !normalReturn.PostCallRegistersCaptured
            && normalReturn.PostCallEAX is null
            && normalReturn.PostCallEBX is null
            && normalReturn.PostCallECX is null
            && normalReturn.PostCallESI is null
            && normalReturn.PostCallESP is null
            && normalReturn.ExactChildCompletion
            && normalReturn.ExactBoundedCompletion
            && normalReturn.ExactCleanCompletion
            && !normalReturn.NativeInstructionsExecuted
            && !normalReturn.NativeCallExecuted
            && !normalReturn.OpaqueCallTargetEntered
            && !normalReturn.VtableSlotReadExecuted);

        GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect priorStackStore =
            normalReturn.ParentPlan.MemoryEffects.Single(effect =>
                effect.Kind
                    == GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
                        .StoreFFFFFFFFAtESPPlus50
                && effect.Attempted
                && effect.Captured
                && effect.Committed);
        uint aliasedAddress = priorStackStore.Address!.Value;
        uint vptrForAliasedAddress = unchecked(aliasedAddress - 4u);
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan aliasedRead =
            Project(
                sharedParentPlan,
                freshOperationVptr: vptrForAliasedAddress,
                targetDWORD: 0x0BADC0DE,
                returnedNormally: true);
        GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect aliasedPriorStore =
            aliasedRead.ParentPlan.MemoryEffects.Single(effect =>
                effect.Kind
                    == GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
                        .StoreFFFFFFFFAtESPPlus50
                && effect.Attempted
                && effect.Captured
                && effect.Committed);
        check("The fresh slot read derives its wrapped address from EAX and remains independent of an aliased parent store",
            aliasedRead.ParentPlan.EAXValue == vptrForAliasedAddress
            && aliasedRead.MemoryRead is {
                Address: var aliasedReadAddress,
                Value: 0x0BADC0DE,
                Captured: true,
                NativeReadExecuted: false
            }
            && aliasedReadAddress == aliasedAddress
            && aliasedPriorStore.Address == aliasedAddress
            && aliasedPriorStore.Value == uint.MaxValue
            && aliasedRead.MemoryRead.Value.Value != aliasedPriorStore.Value
            && aliasedRead.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan missingParentVptr =
            Project(
                sharedParentPlan,
                freshOperationVptr: null,
                targetDWORD: 0x22222222,
                returnedNormally: true);
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan takenParent =
            Project(
                freshOperationVptr: 0x11111111,
                targetDWORD: 0x33333333,
                returnedNormally: true,
                secondRmwDWORD: 2);
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan unreachedParent =
            Project(
                sharedParentPlan,
                freshOperationVptr: null,
                targetDWORD: null,
                returnedNormally: null);
        check("Rejected parents preserve their own exact stop and leave the child target and return unused",
            missingParentVptr.Outcome
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                    .ParentBranchNotAdmitted
            && missingParentVptr.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationOutcome
                    .MissingFreshOperationVptrDWORDAt34DB
            && missingParentVptr.ParentPlan.StopBeforeRawFileOffset == 0x00BD34DB
            && missingParentVptr.StopBeforeRawFileOffset == 0x00BD34DB
            && !missingParentVptr.ParentCallBoundaryAdmitted
            && missingParentVptr.TargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState
                    .SuppliedButUnused
            && missingParentVptr.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState
                    .SuppliedButUnused
            && missingParentVptr.LastReachedRawFileOffset
                == missingParentVptr.ParentPlan.LastReachedRawFileOffset
            && missingParentVptr.MemoryRead is null
            && !missingParentVptr.ExactBoundedCompletion
            && takenParent.Outcome
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnOutcome
                    .ParentBranchNotAdmitted
            && takenParent.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome.JnzTakenTo34E2
            && takenParent.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && takenParent.StopBeforeRawFileOffset == 0x00BD34E2
            && takenParent.TargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState
                    .SuppliedButUnused
            && takenParent.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState
                    .SuppliedButUnused
            && takenParent.LastReachedRawFileOffset
                == takenParent.ParentPlan.LastReachedRawFileOffset
            && takenParent.MemoryRead is null
            && takenParent.ExactBoundedCompletion
                == takenParent.ParentPlan.ExactBoundedCompletion
            && unreachedParent.ParentPlan.StopBeforeRawFileOffset == 0x00BD34DB
            && unreachedParent.TargetCaptureState
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState.NotReached
            && unreachedParent.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCaptureState.NotReached
            && unreachedParent.MemoryRead is null
            && unreachedParent.StopBeforeRawFileOffset
                == unreachedParent.ParentPlan.StopBeforeRawFileOffset);

        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan reprojection =
            GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection.Project(
                normalReturn.Capture);
        check("The parent prefix and fresh target capture reproject semantically",
            reprojection.Capture == normalReturn.Capture
            && reprojection.Outcome == normalReturn.Outcome
            && reprojection.ParentPlan.Capture == reprojection.Capture.ParentCapture
            && reprojection.ParentPlan.ParentPlan.Capture
                == reprojection.Capture.ParentCapture.ParentCapture
            && reprojection.TargetCaptureState == normalReturn.TargetCaptureState
            && reprojection.ReturnCaptureState == normalReturn.ReturnCaptureState
            && reprojection.TargetReadAttempted == normalReturn.TargetReadAttempted
            && reprojection.TargetReadCaptured == normalReturn.TargetReadCaptured
            && reprojection.LogicalCallAttempted == normalReturn.LogicalCallAttempted
            && reprojection.ReturnObservationCaptured
                == normalReturn.ReturnObservationCaptured
            && reprojection.MemoryRead == normalReturn.MemoryRead
            && reprojection.CallBoundary == normalReturn.CallBoundary
            && reprojection.InstructionOrder.SequenceEqual(normalReturn.InstructionOrder)
            && reprojection.ParentPlan.Instructions.SequenceEqual(
                normalReturn.ParentPlan.Instructions)
            && reprojection.ParentPlan.Reads.SequenceEqual(normalReturn.ParentPlan.Reads)
            && reprojection.ParentPlan.MemoryEffects.SequenceEqual(
                normalReturn.ParentPlan.MemoryEffects)
            && reprojection.Reads.SequenceEqual(normalReturn.Reads)
            && reprojection.MemoryEffects.SequenceEqual(normalReturn.MemoryEffects)
            && reprojection.FlagsAtStop == normalReturn.FlagsAtStop
            && reprojection.LastReachedRawFileOffset
                == normalReturn.LastReachedRawFileOffset
            && reprojection.StopBeforeRawFileOffset
                == normalReturn.StopBeforeRawFileOffset
            && reprojection.OrderedSemanticPrefixPreserved
            && reprojection.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan Project(
        uint? freshOperationVptr,
        uint? targetDWORD,
        bool? returnedNormally,
        uint? freshPostCallEBX = 0x1234,
        uint? freshPostCallESI = 0x9000,
        uint secondRmwDWORD = 1)
    {
        GameSocketParserDiagnosticCleanupSecondXaddBranchCapture parentCapture =
            CreateParentCapture(freshPostCallEBX, freshPostCallESI, secondRmwDWORD);
        return GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection.Project(
            new(
                new(parentCapture, freshOperationVptr),
                targetDWORD,
                returnedNormally));
    }

    private static GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan Project(
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan parentPlan,
        uint? freshOperationVptr,
        uint? targetDWORD,
        bool? returnedNormally)
        => GameSocketParserDiagnosticCleanupSecondVirtualCallReturnProjection.Project(
            new(
                new(parentPlan.Capture, freshOperationVptr),
                targetDWORD,
                returnedNormally));

    private static GameSocketParserDiagnosticCleanupSecondXaddBranchCapture CreateParentCapture(
        uint? freshPostCallEBX,
        uint? freshPostCallESI,
        uint secondRmwDWORD)
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
        GameSocketParserDiagnosticCleanupVirtualCallPreparationCapture preparationCapture =
            new(xaddBranchCapture, 0x7000);
        GameSocketParserDiagnosticCleanupVirtualCallReturnCapture returnCapture =
            new(preparationCapture, 0x12345678, true);
        GameSocketParserDiagnosticCleanupPostCallAddressCapture addressCapture =
            new(returnCapture, freshPostCallESI);
        return new(
            new(addressCapture, freshPostCallEBX, secondRmwDWORD));
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
