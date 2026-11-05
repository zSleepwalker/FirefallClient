using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupSecondBranchFsRestoreChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .CoordinatorProvidedBytes);
        check("IDA bytes anchor the branch MOV, segment-relative FS store, and stop before POP",
            GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .BytesIndependentlyVerified
            && GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .MoveRawFileOffset == 0x00BD34E2
            && GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .MoveEncoding == "8B 4C 24 48"
            && GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .FsStoreRawFileOffset == 0x00BD34E6
            && GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .FsStoreEncoding == "64 89 0D 00 00 00 00"
            && GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .StopBeforePopRawFileOffset == 0x00BD34ED
            && GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .NextInstructionEncoding == "59"
            && suppliedBytes.Length == 12
            && suppliedBytes.SequenceEqual(
                GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection.WindowBytes)
            && suppliedBytes.Take(4).SequenceEqual(ParseBytes("8B 4C 24 48"))
            && suppliedBytes.Skip(4).Take(7).SequenceEqual(
                ParseBytes("64 89 0D 00 00 00 00"))
            && suppliedBytes.Skip(11).SequenceEqual(ParseBytes("59"))
            && !GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .ExecutesNativeInstructions
            && !GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .ExecutesNativeReads
            && !GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .ExecutesNativeWrites
            && !GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection
                .ExecutesContinuationInstruction);

        GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan zeroAddressZeroValue =
            Project(secondRmwDWORD: 2, freshESP: 0xFFFFFFB8, freshDWORD: 0);
        GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan maxAddressMaxValue =
            Project(secondRmwDWORD: 2, freshESP: 0xFFFFFFB7, freshDWORD: uint.MaxValue);
        check("Fresh ESP-plus-48 reads wrap as DWORDs and FS writes remain segment-relative",
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

        GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan missingESP =
            Project(secondRmwDWORD: 2, freshESP: null, freshDWORD: 0x12345678);
        check("Missing fresh branch ESP stops at E2 without a read or FS write",
            missingESP.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                    .MissingFreshESPAt34E2
            && missingESP.ParentContinuationReached
            && missingESP.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && missingESP.FreshESPCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState.Missing
            && missingESP.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
                    .SuppliedButUnused
            && missingESP.MemoryRead is null
            && missingESP.RegisterMove is null
            && missingESP.MemoryWrite is null
            && missingESP.FlagsAtStop == missingESP.ParentPlan.FlagsAtStop
            && missingESP.DECFlagsPreserved
            && missingESP.LastReachedRawFileOffset == 0x00BD34D9
            && missingESP.StopBeforeRawFileOffset == 0x00BD34E2
            && !missingESP.ChildBoundedCompletionBoundaryReached
            && !missingESP.ExactBoundedCompletion
            && !missingESP.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan missingDWORD =
            Project(secondRmwDWORD: 2, freshESP: 0x7000, freshDWORD: null);
        check("Missing fresh stack DWORD records the E2 load without committing ECX or writing FS",
            missingDWORD.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                    .MissingFreshDWORDAt34E2
            && missingDWORD.FreshESPAt34E2 == 0x7000
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
                DestinationRegister: "ECX",
                Attempted: true,
                Reached: true,
                Committed: false
            }
            && missingDWORD.MemoryWrite is null
            && missingDWORD.FlagsAtStop == missingDWORD.ParentPlan.FlagsAtStop
            && missingDWORD.DECFlagsPreserved
            && missingDWORD.LastReachedRawFileOffset == 0x00BD34E2
            && missingDWORD.StopBeforeRawFileOffset == 0x00BD34E2
            && !missingDWORD.ChildBoundedCompletionBoundaryReached
            && !missingDWORD.ExactBoundedCompletion
            && !missingDWORD.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan fallthrough =
            Project(secondRmwDWORD: 1, freshESP: 0x7100, freshDWORD: 0xAABBCCDD);
        check("Old DWORD one falls through to DB and cannot enter the E2 restoration suffix",
            fallthrough.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                    .BranchNotTakenTo34DB
            && fallthrough.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome
                    .JnzNotTakenTo34DB
            && fallthrough.ParentPlan.ExactCleanCompletion
            && fallthrough.ParentPlan.LastReachedRawFileOffset == 0x00BD34D9
            && fallthrough.ParentPlan.StopBeforeRawFileOffset == 0x00BD34DB
            && !fallthrough.ParentContinuationReached
            && !fallthrough.SuffixAdmitted
            && fallthrough.MemoryRead is null
            && fallthrough.RegisterMove is null
            && fallthrough.MemoryWrite is null
            && fallthrough.FreshESPCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
                    .SuppliedButUnused
            && fallthrough.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
                    .SuppliedButUnused
            && fallthrough.FlagsAtStop == fallthrough.ParentPlan.FlagsAtStop
            && fallthrough.ExactBoundedCompletion
                == fallthrough.ParentPlan.ExactBoundedCompletion
            && fallthrough.StopBeforeRawFileOffset == 0x00BD34DB
            && !fallthrough.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCapture[] incompleteCaptures =
        {
            Capture(secondRmwDWORD: 2, freshPostCallEBX: null),
            Capture(secondRmwDWORD: null),
            Capture(secondRmwDWORD: 2, freshPostCallESI: null),
            Capture(secondRmwDWORD: 2, returnedNormally: null),
            Capture(secondRmwDWORD: 2, returnedNormally: false),
            Capture(secondRmwDWORD: 2, freshOperationVptr: null),
            Capture(secondRmwDWORD: 2, targetDWORD: null),
            Capture(secondRmwDWORD: 2, firstRmwDWORD: null)
        };
        GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan[] incompleteParents =
            incompleteCaptures.Select(capture =>
                GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection.Project(
                    capture with
                    {
                        FreshESPAt34E2 = 0x7200,
                        FreshDWORDAtWrappedESPPlus48 = 0x11223344
                    }))
                .ToArray();
        check("Every incomplete ancestor retains its actual bound and rejects the branch suffix",
            incompleteParents.All(plan =>
                plan.Outcome
                    == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                        .ParentContinuationNotAdmitted
                && !plan.ParentContinuationReached
                && !plan.SuffixAdmitted
                && plan.MemoryRead is null
                && plan.RegisterMove is null
                && plan.MemoryWrite is null
                && plan.FreshESPCaptureState
                    == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
                        .SuppliedButUnused
                && plan.FreshDWORDCaptureState
                    == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCaptureState
                        .SuppliedButUnused
                && plan.FlagsAtStop == plan.ParentPlan.FlagsAtStop
                && plan.LastReachedRawFileOffset
                    == plan.ParentPlan.LastReachedRawFileOffset
                && plan.StopBeforeRawFileOffset
                    == plan.ParentPlan.StopBeforeRawFileOffset
                && plan.ExactBoundedCompletion
                    == plan.ParentPlan.ExactBoundedCompletion
                && !plan.ChildBoundedCompletionBoundaryReached
                && !plan.ExactCleanCompletion)
            && incompleteParents[0].ParentPlan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34D4
            && incompleteParents[1].ParentPlan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34D4
            && incompleteParents[2].ParentPlan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34D1
            && incompleteParents[3].ParentPlan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34CF
            && incompleteParents[4].ParentPlan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34CF
            && incompleteParents[5].ParentPlan.ParentPlan.ParentPlan.ParentPlan.ParentPlan
                .StopBeforeRawFileOffset == 0x00BD34CB
            && incompleteParents[6].ParentPlan.ParentPlan.ParentPlan.ParentPlan
                .StopBeforeRawFileOffset
                == 0x00BD34CF
            && incompleteParents[7].ParentPlan.ParentPlan.ParentPlan.ParentPlan.ParentPlan
                .ParentPlan.ParentPlan
                .StopBeforeRawFileOffset == 0x00BD34C5);

        GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan successful =
            Project(secondRmwDWORD: 2, freshESP: 0x7400, freshDWORD: 0x12345678);
        check("Taken JNZ restores FS from a separate fresh stack read while preserving DEC flags",
            successful.Outcome
                == GameSocketParserDiagnosticCleanupSecondBranchFsRestoreOutcome
                    .CompletedBeforePopAt34ED
            && successful.ParentContinuationReached
            && successful.SuffixAdmitted
            && successful.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome.JnzTakenTo34E2
            && successful.InstructionOrder.Select(step => step.RawFileOffset).SequenceEqual(
                new uint[] { 0x00BD34E2, 0x00BD34E2, 0x00BD34E6 })
            && successful.MemoryRead is {
                Address: 0x7448,
                Value: 0x12345678,
                Attempted: true,
                Captured: true
            }
            && successful.RegisterMove is {
                DestinationRegister: "ECX",
                SourceAddress: 0x7448,
                Value: 0x12345678,
                Committed: true
            }
            && successful.MemoryWrite?.Value == successful.ECXAfterMove
            && successful.FlagsAtStop == successful.ParentPlan.FlagsAtStop
            && successful.FlagsAtStop is {
                Carry: not null,
                Parity: not null,
                AuxiliaryCarry: not null,
                Zero: false,
                Sign: not null,
                Overflow: not null
            }
            && successful.DECFlagsPreserved
            && successful.OpaqueCallAfterBranchObserved == false
            && successful.ContinuationBoundary is {
                InstructionRawFileOffset: 0x00BD34ED,
                NextInstructionEncoding: "59",
                Reached: true,
                NextInstructionExecuted: false
            }
            && successful.LastReachedRawFileOffset == 0x00BD34E6
            && successful.StopBeforeRawFileOffset == 0x00BD34ED
            && successful.ExactBoundedCompletion
            && successful.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan branchWithPriorStores =
            GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.Project(
                Capture(secondRmwDWORD: 2).ParentCapture);
        GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect priorStore =
            branchWithPriorStores.MemoryEffects.Single(effect =>
                effect.Kind
                    == GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
                        .StoreFFFFFFFFAtESPPlus50
                && effect.Attempted
                && effect.Captured
                && effect.Committed);
        uint aliasedESP = unchecked(priorStore.Address!.Value - 0x48u);
        GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan aliasedRead =
            Project(secondRmwDWORD: 2, freshESP: aliasedESP, freshDWORD: 0x0BADC0DE);
        check("The fresh branch stack read is independent even when it aliases an earlier store",
            aliasedRead.FreshESPAt34E2 == aliasedESP
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

        GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan reprojection =
            GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection.Project(
                successful.Capture);
        check("The taken-branch restore reprojects from its immutable capture",
            reprojection.Capture == successful.Capture
            && reprojection.Outcome == successful.Outcome
            && reprojection.ParentPlan.Capture == reprojection.Capture.ParentCapture
            && reprojection.ParentPlan.ParentPlan.Capture
                == reprojection.Capture.ParentCapture.ParentCapture
            && reprojection.ParentPlan.Capture == successful.ParentPlan.Capture
            && reprojection.ParentPlan.Instructions.SequenceEqual(
                successful.ParentPlan.Instructions)
            && reprojection.MemoryRead == successful.MemoryRead
            && reprojection.RegisterMove == successful.RegisterMove
            && reprojection.MemoryWrite == successful.MemoryWrite
            && reprojection.InstructionOrder.SequenceEqual(successful.InstructionOrder)
            && reprojection.FreshESPAt34E2 == successful.FreshESPAt34E2
            && reprojection.FreshDWORDAtWrappedESPPlus48
                == successful.FreshDWORDAtWrappedESPPlus48
            && reprojection.FlagsAtStop == successful.FlagsAtStop
            && reprojection.LastReachedRawFileOffset == successful.LastReachedRawFileOffset
            && reprojection.StopBeforeRawFileOffset == successful.StopBeforeRawFileOffset
            && reprojection.OrderedSemanticPrefixPreserved
            && reprojection.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupSecondBranchFsRestorePlan Project(
        uint? secondRmwDWORD,
        uint? freshESP,
        uint? freshDWORD) =>
        GameSocketParserDiagnosticCleanupSecondBranchFsRestoreProjection.Project(
            new(Capture(secondRmwDWORD).ParentCapture, freshESP, freshDWORD));

    private static GameSocketParserDiagnosticCleanupSecondBranchFsRestoreCapture Capture(
        uint? secondRmwDWORD,
        uint? freshPostCallEBX = 0x1234,
        uint? freshPostCallESI = 0x9000,
        bool? returnedNormally = true,
        uint? freshOperationVptr = 0x7000,
        uint? targetDWORD = 0x12345678,
        uint? firstRmwDWORD = 1) =>
        new(CreateParentCapture(
                secondRmwDWORD,
                freshPostCallEBX,
                freshPostCallESI,
                returnedNormally,
                freshOperationVptr,
                targetDWORD,
                firstRmwDWORD),
            FreshESPAt34E2: null,
            FreshDWORDAtWrappedESPPlus48: null);

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
        GameSocketParserDiagnosticCleanupVirtualCallPreparationCapture preparationCapture =
            new(xaddBranchCapture, freshOperationVptr);
        GameSocketParserDiagnosticCleanupVirtualCallReturnCapture returnCapture =
            new(preparationCapture, targetDWORD, returnedNormally);
        GameSocketParserDiagnosticCleanupPostCallAddressCapture addressCapture =
            new(returnCapture, freshPostCallESI);
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
