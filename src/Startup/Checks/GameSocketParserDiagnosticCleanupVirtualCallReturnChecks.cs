using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupVirtualCallReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The supplied window anchors FF 10 and stops before the following LEA",
            !GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .BytesIndependentlyVerified
            && GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .OpaqueCallRawFileOffset == 0x00BD34CF
            && GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .ContinuationRawFileOffset == 0x00BD34D1
            && GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .OpaqueCallEncoding == "FF 10"
            && GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .ContinuationEncoding == "8D 46 08"
            && GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .WindowBytes.SequenceEqual(Convert.FromHexString(
                    "F00FC10875178B068BCEFF108D4608F0"))
            && !GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .ExecutesNativeInstructions
            && !GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .ExecutesNativeReads
            && !GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .ExecutesMemoryWrites
            && !GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .ExecutesCalls
            && !GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .EntersOpaqueCallTarget
            && !GameSocketParserDiagnosticCleanupVirtualCallReturnProjection
                .ExecutesContinuationInstruction);

        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan zeroValues =
            Project(freshVptr: 0, freshSlot: 0, returnedNormally: true);
        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan maximumValues =
            Project(
                freshVptr: uint.MaxValue,
                freshSlot: uint.MaxValue,
                returnedNormally: true);
        check("Zero and maximum fresh vptr and slot values remain captured scalars",
            zeroValues.ParentPlan.EAXValue == 0
            && zeroValues.MemoryRead is {
                Address: 0,
                Width: 4,
                Value: 0,
                Attempted: true,
                Captured: true,
                NativeReadExecuted: false
            }
            && zeroValues.CallBoundary is {
                VtableSlotAddress: 0,
                TargetDWORD: 0,
                LogicalCallAttempted: true,
                NativeCallExecuted: false
            }
            && zeroValues.ExactCleanCompletion
            && maximumValues.ParentPlan.EAXValue == uint.MaxValue
            && maximumValues.MemoryRead is {
                Address: uint.MaxValue,
                Width: 4,
                Value: uint.MaxValue,
                Attempted: true,
                Captured: true,
                NativeReadExecuted: false
            }
            && maximumValues.CallBoundary is {
                VtableSlotAddress: uint.MaxValue,
                TargetDWORD: uint.MaxValue,
                LogicalCallAttempted: true,
                NativeCallExecuted: false
            }
            && maximumValues.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan missingTarget =
            Project(freshVptr: 0x7100, freshSlot: null, returnedNormally: true);
        check("A missing slot DWORD records the read attempt but creates no logical call",
            missingTarget.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .MissingVtableSlotDWORDAt34CF
            && missingTarget.ParentPlan.IsCleanCompletion
            && missingTarget.TargetCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState.Missing
            && missingTarget.TargetReadAttempted
            && !missingTarget.TargetReadCaptured
            && missingTarget.MemoryRead is {
                InstructionRawFileOffset: 0x00BD34CF,
                Address: 0x7100,
                Width: 4,
                Value: null,
                Attempted: true,
                Captured: false
            }
            && !missingTarget.LogicalCallAttempted
            && missingTarget.CallBoundary is null
            && !missingTarget.ReturnObservationCaptured
            && missingTarget.OpaqueCallReturnedNormally is null
            && missingTarget.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState
                    .SuppliedButUnused
            && missingTarget.FlagsAtStop == missingTarget.ParentPlan.FlagsAtStop
            && missingTarget.StopBeforeRawFileOffset == 0x00BD34CF
            && !missingTarget.ChildBoundedCompletionBoundaryReached
            && !missingTarget.ExactBoundedCompletion
            && !missingTarget.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan pendingReturn =
            Project(freshVptr: 0x7200, freshSlot: 0x12345678, returnedNormally: null);
        check("A pending return retains the parent's ADD flags and does not complete the child",
            pendingReturn.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .MissingOpaqueCallReturnObservation
            && pendingReturn.TargetReadCaptured
            && pendingReturn.LogicalCallAttempted
            && pendingReturn.CallBoundary is {
                LogicalCallAttempted: true,
                NativeCallExecuted: false
            }
            && pendingReturn.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState.Missing
            && !pendingReturn.ReturnObservationCaptured
            && pendingReturn.OpaqueCallReturnedNormally is null
            && pendingReturn.FlagsAtStop == pendingReturn.ParentPlan.FlagsAtStop
            && pendingReturn.StopBeforeRawFileOffset == 0x00BD34CF
            && !pendingReturn.ChildBoundedCompletionBoundaryReached
            && !pendingReturn.ExactBoundedCompletion
            && !pendingReturn.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan abnormalReturn =
            Project(freshVptr: 0x7300, freshSlot: 0x12345678, returnedNormally: false);
        check("An explicit abnormal return retains parent flags and stops at the call",
            abnormalReturn.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .OpaqueCallDidNotReturnNormally
            && abnormalReturn.ReturnObservationCaptured
            && abnormalReturn.OpaqueCallReturnedNormally == false
            && abnormalReturn.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState.Captured
            && abnormalReturn.FlagsAtStop == abnormalReturn.ParentPlan.FlagsAtStop
            && abnormalReturn.ContinuationBoundary is null
            && abnormalReturn.StopBeforeRawFileOffset == 0x00BD34CF
            && !abnormalReturn.ChildBoundedCompletionBoundaryReached
            && !abnormalReturn.ExactBoundedCompletion
            && !abnormalReturn.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan normalReturn =
            Project(freshVptr: 0x7400, freshSlot: 0x12345678, returnedNormally: true);
        check("A normal return stops before LEA, invalidates flags, and forwards no post-call registers",
            normalReturn.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .OpaqueCallReturnedNormally
            && normalReturn.ReturnObservationCaptured
            && normalReturn.OpaqueCallReturnedNormally == true
            && normalReturn.ContinuationBoundary is {
                InstructionRawFileOffset: 0x00BD34D1,
                NextInstructionEncoding: "8D 46 08",
                Reached: true,
                NextInstructionExecuted: false
            }
            && normalReturn.LastReachedRawFileOffset == 0x00BD34CF
            && normalReturn.StopBeforeRawFileOffset == 0x00BD34D1
            && normalReturn.FlagsAtStop is null
            && normalReturn.PostCallRegistersCaptured == false
            && normalReturn.PostCallEAX is null
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

        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan aliasVptrAndStack =
            Project(
                freshVptr: 0x9050,
                freshSlot: 0xDEADBEEF,
                returnedNormally: true,
                esi: 0x9050);
        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan aliasRmw =
            Project(
                freshVptr: 0x7004,
                freshSlot: 0xCAFEBABE,
                returnedNormally: true);
        check("Fresh slot reads remain independent when their addresses alias earlier captures or writes",
            aliasVptrAndStack.MemoryRead is {
                Address: 0x9050,
                Value: 0xDEADBEEF,
                Captured: true
            }
            && aliasVptrAndStack.ParentPlan.MemoryRead is {
                Address: 0x9050,
                Value: 0x9050
            }
            && aliasVptrAndStack.ParentPlan.MemoryEffects.Any(effect =>
                effect.Address == 0x9050 && effect.Committed)
            && aliasVptrAndStack.ExactCleanCompletion
            && aliasRmw.MemoryRead is {
                Address: 0x7004,
                Value: 0xCAFEBABE,
                Captured: true
            }
            && aliasRmw.ParentPlan.ParentPlan.ParentPlan.MemoryRead is {
                Address: 0x7004,
                Value: 1
            }
            && aliasRmw.ParentPlan.ParentPlan.ParentPlan.MemoryWrite is {
                Address: 0x7004,
                OldValue: 1,
                NewValue: 0
            }
            && aliasRmw.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan takenAncestor =
            Project(
                freshVptr: 0x7500,
                freshSlot: 0x11111111,
                returnedNormally: true,
                freshDWORDAtLockedRmw: 0);
        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan missingVptr =
            Project(
                freshVptr: null,
                freshSlot: 0x22222222,
                returnedNormally: true);
        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan missingRmw =
            Project(
                freshVptr: 0x7600,
                freshSlot: 0x33333333,
                returnedNormally: true,
                freshDWORDAtLockedRmw: null);
        check("Rejected and incomplete parents preserve actual bounds and leave child captures unused",
            takenAncestor.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .ParentBranchNotAdmitted
            && takenAncestor.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome
                    .ParentBranchNotAdmitted
            && takenAncestor.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && takenAncestor.TargetCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState
                    .SuppliedButUnused
            && takenAncestor.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState
                    .SuppliedButUnused
            && takenAncestor.MemoryRead is null
            && takenAncestor.ExactBoundedCompletion
                == takenAncestor.ParentPlan.ExactBoundedCompletion
            && takenAncestor.StopBeforeRawFileOffset == 0x00BD34E2
            && missingVptr.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .ParentBranchNotAdmitted
            && missingVptr.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome
                    .MissingFreshVptrDWORDAt34CB
            && missingVptr.ParentPlan.StopBeforeRawFileOffset == 0x00BD34CB
            && missingVptr.TargetCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState
                    .SuppliedButUnused
            && missingVptr.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState
                    .SuppliedButUnused
            && missingVptr.MemoryRead is null
            && !missingVptr.ExactBoundedCompletion
            && missingVptr.StopBeforeRawFileOffset == 0x00BD34CB
            && missingRmw.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .ParentBranchNotAdmitted
            && missingRmw.ParentPlan.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupXaddOutcome.MissingFreshDWORDAt34C5
            && missingRmw.ParentPlan.StopBeforeRawFileOffset == 0x00BD34C5
            && missingRmw.TargetCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState
                    .SuppliedButUnused
            && missingRmw.ReturnCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallReturnCaptureState
                    .SuppliedButUnused
            && missingRmw.MemoryRead is null
            && missingRmw.StopBeforeRawFileOffset == 0x00BD34C5
            && !missingRmw.ExactBoundedCompletion);

        GameSocketParserDiagnosticCleanupVirtualCallReturnPlan reprojection =
            GameSocketParserDiagnosticCleanupVirtualCallReturnProjection.Project(
                normalReturn.Capture);
        check("The immutable parent prefix and fresh target capture reproject semantically",
            reprojection.Capture == normalReturn.Capture
            && reprojection.Outcome == normalReturn.Outcome
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

    private static GameSocketParserDiagnosticCleanupVirtualCallReturnPlan Project(
        uint? freshVptr,
        uint? freshSlot,
        bool? returnedNormally,
        uint? freshDWORDAtLockedRmw = 1,
        uint? esi = 0x7000)
    {
        GameSocketParserDiagnosticCleanupZeroBranchCapture zeroBranchCapture =
            new(CreateParentCapture(esi));
        GameSocketParserDiagnosticCleanupRegisterPreparationCapture registerCapture =
            new(zeroBranchCapture);
        GameSocketParserDiagnosticCleanupXaddCapture xaddCapture =
            new(registerCapture, freshDWORDAtLockedRmw);
        GameSocketParserDiagnosticCleanupXaddBranchCapture branchCapture =
            new(xaddCapture);
        GameSocketParserDiagnosticCleanupVirtualCallPreparationCapture preparationCapture =
            new(branchCapture, freshVptr);
        return GameSocketParserDiagnosticCleanupVirtualCallReturnProjection.Project(
            new(preparationCapture, freshSlot, returnedNormally));
    }

    private static GameSocketParserDiagnosticCleanupStackPrefixCapture CreateParentCapture(
        uint? esi)
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
            esi);
    }
}
