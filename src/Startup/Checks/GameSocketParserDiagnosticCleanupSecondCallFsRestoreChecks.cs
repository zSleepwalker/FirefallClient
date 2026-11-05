using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupSecondCallFsRestoreChecks
{
    public static void Run(Action<string, bool> check)
    {
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan normalParentPlan =
            Project(freshOperationVptr: 0x7400, targetDWORD: 0x12345678, returnedNormally: true);
        RunFsRestoreChecks(check, normalParentPlan);
    }

    private static void RunFsRestoreChecks(
        Action<string, bool> check,
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan normalParentPlan)
    {
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture normalParentCapture =
            normalParentPlan.Capture;
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .CoordinatorProvidedBytes);
        check("Fresh IDA bytes anchor MOV ECX,[ESP+48], FS:[0] write, and stop before POP ECX",
            GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .BytesIndependentlyVerified
            && GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .MoveRawFileOffset == 0x00BD34E2
            && GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .MoveEncoding == "8B 4C 24 48"
            && GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .FsStoreRawFileOffset == 0x00BD34E6
            && GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .FsStoreEncoding == "64 89 0D 00 00 00 00"
            && GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .StopBeforePopRawFileOffset == 0x00BD34ED
            && GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .NextInstructionEncoding == "59"
            && suppliedBytes.Length == 22
            && suppliedBytes.SequenceEqual(
                GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.WindowBytes)
            && suppliedBytes.Take(4).SequenceEqual(ParseBytes("8B 4C 24 48"))
            && suppliedBytes.Skip(4).Take(7).SequenceEqual(
                ParseBytes("64 89 0D 00 00 00 00"))
            && suppliedBytes.Skip(11).Take(1).SequenceEqual(ParseBytes("59"))
            && !GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .ExecutesNativeInstructions
            && !GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .ExecutesNativeReads
            && !GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .ExecutesNativeWrites
            && !GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection
                .ExecutesContinuationInstruction);

        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan zeroAddressZeroValue =
            ProjectFsRestore(normalParentPlan, 0xFFFFFFB8, 0);
        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan maxAddressMaxValue =
            ProjectFsRestore(normalParentPlan, 0xFFFFFFB7, uint.MaxValue);
        check("Wrapped ESP-plus-48 addresses and zero/maximum DWORD values remain valid observations",
            zeroAddressZeroValue.MemoryRead is {
                Address: 0,
                Value: 0,
                Attempted: true,
                Captured: true,
                NativeReadExecuted: false
            }
            && zeroAddressZeroValue.RegisterMove is {
                DestinationRegister: "ECX",
                SourceAddress: 0,
                Value: 0,
                Committed: true
            }
            && zeroAddressZeroValue.MemoryWrite is {
                SegmentRegister: "FS",
                SegmentOffset: 0,
                Width: 4,
                Value: 0,
                SegmentRelative: true,
                LinearAddress: null,
                Committed: true,
                NativeWriteExecuted: false
            }
            && zeroAddressZeroValue.ExactCleanCompletion
            && maxAddressMaxValue.MemoryRead is {
                Address: uint.MaxValue,
                Value: uint.MaxValue,
                Captured: true
            }
            && maxAddressMaxValue.MemoryWrite is {
                Value: uint.MaxValue,
                LinearAddress: null,
                SegmentRelative: true,
                Committed: true
            }
            && maxAddressMaxValue.ExactCleanCompletion
            && !zeroAddressZeroValue.LinearFSAddressModeled
            && !maxAddressMaxValue.LinearFSAddressModeled
            && !zeroAddressZeroValue.PointerValidityModeled
            && !maxAddressMaxValue.PointerValidityModeled
            && !zeroAddressZeroValue.NativeABIOrProtocolSemanticsModeled
            && !maxAddressMaxValue.NativeABIOrProtocolSemanticsModeled);

        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan missingESP =
            ProjectFsRestore(normalParentPlan, null, 0x12345678);
        check("Missing fresh post-call ESP stops at E2 without a read or FS write and leaves the supplied DWORD unused",
            missingESP.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome
                    .MissingFreshPostCallESPAt34E2
            && missingESP.ParentPlan.ExactCleanCompletion
            && missingESP.FreshPostCallESPCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState.Missing
            && missingESP.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState.SuppliedButUnused
            && missingESP.MemoryRead is null
            && missingESP.RegisterMove is null
            && missingESP.MemoryWrite is null
            && missingESP.FreshPostCallESP is null
            && missingESP.FlagsAtStop is null
            && missingESP.LastReachedRawFileOffset == 0x00BD34DF
            && missingESP.StopBeforeRawFileOffset == 0x00BD34E2
            && !missingESP.ChildBoundedCompletionBoundaryReached
            && !missingESP.ExactBoundedCompletion
            && !missingESP.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan missingDWORD =
            ProjectFsRestore(normalParentPlan, 0x7000, null);
        check("A missing fresh stack DWORD records the E2 read attempt without committing MOV or writing FS",
            missingDWORD.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome
                    .MissingFreshDWORDAt34E2
            && missingDWORD.FreshPostCallESP == 0x7000
            && missingDWORD.MemoryRead is {
                InstructionRawFileOffset: 0x00BD34E2,
                Address: 0x7048,
                Width: 4,
                Value: null,
                Attempted: true,
                Captured: false,
                NativeReadExecuted: false
            }
            && missingDWORD.RegisterMove is {
                InstructionRawFileOffset: 0x00BD34E2,
                Attempted: true,
                Reached: true,
                Committed: false
            }
            && missingDWORD.MemoryWrite is null
            && missingDWORD.FlagsAtStop is null
            && missingDWORD.LastReachedRawFileOffset == 0x00BD34E2
            && missingDWORD.StopBeforeRawFileOffset == 0x00BD34E2
            && !missingDWORD.ChildBoundedCompletionBoundaryReached
            && !missingDWORD.ExactBoundedCompletion
            && !missingDWORD.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan parentWithPriorStore =
            normalParentPlan.ParentPlan;
        GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect priorStore =
            parentWithPriorStore.MemoryEffects.Single(effect =>
                effect.Kind
                    == GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
                        .StoreFFFFFFFFAtESPPlus50
                && effect.Attempted
                && effect.Captured
                && effect.Committed);
        uint aliasedESP = unchecked(priorStore.Address!.Value - 0x48u);
        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan aliasedRead =
            ProjectFsRestore(normalParentPlan, aliasedESP, 0x0BADC0DE);
        check("The fresh post-call ESP read may alias a prior stack store without reusing its value",
            aliasedRead.FreshPostCallESP == aliasedESP
            && aliasedRead.MemoryRead is {
                Address: var freshAddress,
                Value: 0x0BADC0DE,
                Captured: true,
                NativeReadExecuted: false
            }
            && freshAddress == priorStore.Address
            && priorStore.Value == uint.MaxValue
            && aliasedRead.FreshDWORDAtWrappedESPPlus48 != priorStore.Value
            && aliasedRead.RegisterMove?.Value == aliasedRead.MemoryRead.Value.Value
            && aliasedRead.MemoryWrite?.Value == aliasedRead.RegisterMove.Value.Value
            && aliasedRead.InstructionOrder.Length == 3
            && aliasedRead.InstructionOrder[0].RawFileOffset == 0x00BD34E2
            && aliasedRead.InstructionOrder[1].RawFileOffset == 0x00BD34E2
            && aliasedRead.InstructionOrder[2].RawFileOffset == 0x00BD34E6
            && aliasedRead.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture pendingCapture =
            CreateSecondCallReturnCapture(
                freshOperationVptr: 0x7200,
                targetDWORD: 0x12345678,
                returnedNormally: null);
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture abnormalCapture =
            CreateSecondCallReturnCapture(
                freshOperationVptr: 0x7300,
                targetDWORD: 0x87654321,
                returnedNormally: false);
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture missingVptrCapture =
            CreateSecondCallReturnCapture(
                freshOperationVptr: null,
                targetDWORD: 0x22222222,
                returnedNormally: true);
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture branchToE2Capture =
            CreateSecondCallReturnCapture(
                freshOperationVptr: 0x11111111,
                targetDWORD: 0x33333333,
                returnedNormally: true,
                secondRmwDWORD: 2);
        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan[] rejectedParents =
        {
            ProjectFsRestore(pendingCapture, 0x8000, 1),
            ProjectFsRestore(abnormalCapture, 0x8100, 2),
            ProjectFsRestore(missingVptrCapture, 0x8200, 3),
            ProjectFsRestore(branchToE2Capture, 0x8300, 4)
        };
        check("Pending/false call returns, missing vptr, and the branch to E2 preserve actual ancestor completion without suffix admission",
            rejectedParents.All(plan =>
                !plan.ParentContinuationReached
                && !plan.SuffixAdmitted
                && plan.MemoryRead is null
                && plan.RegisterMove is null
                && plan.MemoryWrite is null
                && plan.FreshPostCallESPCaptureState
                    == GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState
                        .SuppliedButUnused
                && plan.FreshDWORDCaptureState
                    == GameSocketParserDiagnosticCleanupSecondCallFsRestoreCaptureState
                        .SuppliedButUnused
                && plan.FlagsAtStop == plan.ParentPlan.FlagsAtStop
                && plan.LastReachedRawFileOffset
                    == plan.ParentPlan.LastReachedRawFileOffset
                && plan.StopBeforeRawFileOffset
                    == plan.ParentPlan.StopBeforeRawFileOffset
                && plan.ExactBoundedCompletion
                    == plan.ParentPlan.ExactBoundedCompletion)
            && rejectedParents[0].StopBeforeRawFileOffset == 0x00BD34DF
            && rejectedParents[1].StopBeforeRawFileOffset == 0x00BD34DF
            && rejectedParents[2].ParentPlan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34DB
            && rejectedParents[2].StopBeforeRawFileOffset == 0x00BD34DB
            && rejectedParents[3].ParentPlan.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome.JnzTakenTo34E2
            && rejectedParents[3].ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && rejectedParents[3].StopBeforeRawFileOffset == 0x00BD34E2
            && rejectedParents[3].ExactBoundedCompletion);

        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan successful =
            ProjectFsRestore(normalParentPlan, 0x7400, 0x12345678);
        check("FS:[0] follows the fresh read and ECX load, with all unrelated post-call registers unknown",
            successful.Outcome
                == GameSocketParserDiagnosticCleanupSecondCallFsRestoreOutcome.CompletedBeforePopAt34ED
            && successful.ParentContinuationReached
            && successful.SuffixAdmitted
            && successful.InstructionOrder.Select(step => step.RawFileOffset).SequenceEqual(
                new uint[] { 0x00BD34E2, 0x00BD34E2, 0x00BD34E6 })
            && successful.MemoryWrite?.Value == successful.ECXAfterMove
            && successful.FlagsAtStop is null
            && successful.EAXAfterCall is null
            && successful.EBXAfterCall is null
            && successful.ESIAfterCall is null
            && successful.EBPAfterCall is null
            && !successful.PostCallRegistersCaptured
            && !successful.ESPUnchangedClaimed
            && successful.ContinuationBoundary is {
                InstructionRawFileOffset: 0x00BD34ED,
                NextInstructionEncoding: "59",
                Reached: true,
                NextInstructionExecuted: false
            }
            && successful.StopBeforeRawFileOffset == 0x00BD34ED
            && successful.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan reprojection =
            GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.Project(
                successful.Capture);
        check("The post-call ESP capture, read, MOV, and FS write reproject semantically",
            reprojection.Capture == successful.Capture
            && reprojection.Outcome == successful.Outcome
            && reprojection.MemoryRead == successful.MemoryRead
            && reprojection.RegisterMove == successful.RegisterMove
            && reprojection.MemoryWrite == successful.MemoryWrite
            && reprojection.InstructionOrder.SequenceEqual(successful.InstructionOrder)
            && reprojection.FreshPostCallESP == successful.FreshPostCallESP
            && reprojection.FreshDWORDAtWrappedESPPlus48
                == successful.FreshDWORDAtWrappedESPPlus48
            && reprojection.FlagsAtStop == successful.FlagsAtStop
            && reprojection.LastReachedRawFileOffset == successful.LastReachedRawFileOffset
            && reprojection.StopBeforeRawFileOffset == successful.StopBeforeRawFileOffset
            && reprojection.OrderedSemanticPrefixPreserved
            && reprojection.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan ProjectFsRestore(
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture parentCapture,
        uint? freshPostCallESP,
        uint? freshDWORD) =>
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.Project(
            new(parentCapture, freshPostCallESP, freshDWORD));

    private static GameSocketParserDiagnosticCleanupSecondCallFsRestorePlan ProjectFsRestore(
        GameSocketParserDiagnosticCleanupSecondVirtualCallReturnPlan parentPlan,
        uint? freshPostCallESP,
        uint? freshDWORD) =>
        GameSocketParserDiagnosticCleanupSecondCallFsRestoreProjection.Project(
            new(parentPlan.Capture, freshPostCallESP, freshDWORD));

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

    private static GameSocketParserDiagnosticCleanupSecondVirtualCallReturnCapture
        CreateSecondCallReturnCapture(
            uint? freshOperationVptr,
            uint? targetDWORD,
            bool? returnedNormally,
            uint? freshPostCallEBX = 0x1234,
            uint? freshPostCallESI = 0x9000,
            uint secondRmwDWORD = 1)
    {
        GameSocketParserDiagnosticCleanupSecondXaddBranchCapture parentCapture =
            CreateParentCapture(freshPostCallEBX, freshPostCallESI, secondRmwDWORD);
        return new(
            new(parentCapture, freshOperationVptr),
            targetDWORD,
            returnedNormally);
    }

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
