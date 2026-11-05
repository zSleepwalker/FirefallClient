using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .CoordinatorProvidedBytes);
        check("The supplied bytes anchor the fresh operation-vptr load and stop before the slot-four call",
            !GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .BytesIndependentlyVerified
            && suppliedBytes.Length == 17
            && GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .VptrReadRawFileOffset == 0x00BD34DB
            && GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .VptrReadEncoding == "8B 06"
            && GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .MoveECXRawFileOffset == 0x00BD34DD
            && GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .MoveECXEncoding == "8B CE"
            && GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection
                .Slot4CallRawFileOffset == 0x00BD34DF
            && suppliedBytes.Take(3).SequenceEqual(ParseBytes("4B 75 07"))
            && suppliedBytes.Skip(3).Take(2).SequenceEqual(ParseBytes("8B 06"))
            && suppliedBytes.Skip(5).Take(2).SequenceEqual(ParseBytes("8B CE"))
            && suppliedBytes.Skip(7).Take(3).SequenceEqual(ParseBytes("FF 50 04")));

        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan zeroVptr =
            Project(freshOperationVptr: 0);
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan maxVptr =
            Project(freshOperationVptr: uint.MaxValue);
        bool zeroClean = zeroVptr.ExactCleanCompletion;
        bool maxClean = maxVptr.ExactCleanCompletion;
        check("Captured zero and maximum fresh vptr DWORDs are valid scalar observations",
            zeroClean
            && maxClean
            && zeroVptr.MemoryRead is {
                Address: var zeroAddress,
                Value: 0,
                Attempted: true,
                Captured: true
            }
            && zeroAddress == zeroVptr.ESIValue
            && zeroVptr.EAXValue == 0
            && maxVptr.MemoryRead is {
                Address: var maxAddress,
                Value: uint.MaxValue,
                Attempted: true,
                Captured: true
            }
            && maxAddress == maxVptr.ESIValue
            && maxVptr.EAXValue == uint.MaxValue
            && !zeroVptr.PointerValidityModeled
            && !maxVptr.PointerValidityModeled
            && !zeroVptr.NativeABIOrProtocolSemanticsModeled
            && !maxVptr.NativeABIOrProtocolSemanticsModeled);

        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan successful =
            Project(freshOperationVptr: 0xDEADBEEF);
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan replay =
            GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection.Project(
                new(
                    successful.Capture.ParentCapture,
                    successful.Capture.FreshOperationVptrDWORDAtESI));
        bool successfulClean = successful.ExactCleanCompletion;
        bool replayClean = replay.ExactCleanCompletion;
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan branch =
            successful.ParentPlan;
        check("The fresh operation-vptr read controls EAX while preserving registers and flags",
            successfulClean
            && replayClean
            && successful.FreshOperationVptrReadAttempted
            && successful.FreshOperationVptrReadCaptured
            && successful.MemoryRead is {
                InstructionRawFileOffset: 0x00BD34DB,
                Address: var readAddress,
                Value: 0xDEADBEEF,
                Attempted: true,
                Captured: true
            }
            && readAddress == successful.ESIValue
            && branch.ParentPlan.MemoryRead is {
                Value: 1,
                Attempted: true,
                Captured: true,
                ObservedAtLockedRmw: true
            }
            && successful.EAXValue == 0xDEADBEEF
            && successful.ECXValue == successful.ESIValue
            && successful.EBXValue == 0
            && successful.ESIUnchanged
            && successful.ESPRemainsUnknown
            && successful.ESPValue is null
            && successful.FlagsAtStop == branch.FlagsAtStop
            && successful.FlagsPropagated
            && successful.RegistersPreserved
            && successful.OrderedSemanticPrefixPreserved
            && successful.LastReachedRawFileOffset == 0x00BD34DD
            && successful.StopBeforeRawFileOffset == 0x00BD34DF
            && successful.Slot4CallRawFileOffset == 0x00BD34DF
            && successful.Instructions.Length == 2
            && successful.Instructions[0] is {
                Kind: GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstructionKind
                    .MoveEAXFromESIAt34DB,
                InstructionRawFileOffset: 0x00BD34DB,
                Encoding: "8B 06",
                StackPointerBefore: null,
                StackPointerAfter: null,
                DestinationRegister: "EAX",
                DestinationValue: 0xDEADBEEF,
                SourceRegister: "[ESI]",
                SourceValue: var loadAddress,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsBefore: var loadFlagsBefore,
                FlagsAfter: var loadFlagsAfter
            }
            && loadAddress == successful.ESIValue
            && loadFlagsBefore == branch.FlagsAtStop
            && loadFlagsAfter == branch.FlagsAtStop
            && successful.Instructions[1] is {
                Kind: GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstructionKind
                    .MoveECXFromESIAt34DD,
                InstructionRawFileOffset: 0x00BD34DD,
                Encoding: "8B CE",
                StackPointerBefore: null,
                StackPointerAfter: null,
                DestinationRegister: "ECX",
                DestinationValue: var moveECXValue,
                SourceRegister: "ESI",
                SourceValue: var moveECXSource,
                Attempted: true,
                Reached: true,
                Committed: true,
                FlagsBefore: var moveFlagsBefore,
                FlagsAfter: var moveFlagsAfter
            }
            && moveECXValue == successful.ESIValue
            && moveECXSource == successful.ESIValue
            && moveFlagsBefore == branch.FlagsAtStop
            && moveFlagsAfter == branch.FlagsAtStop
            && !successful.Slot4CallAttempted
            && !successful.Slot4CallReached
            && !successful.Slot4CallCommitted
            && !successful.Slot4TargetRead
            && !successful.ParentInstructionsReplayed
            && successful.Capture == replay.Capture
            && successful.ParentPlan.Capture == replay.ParentPlan.Capture
            && successful.ParentPlan.Instructions.SequenceEqual(
                replay.ParentPlan.Instructions)
            && successful.ParentPlan.Reads.SequenceEqual(replay.ParentPlan.Reads)
            && successful.ParentPlan.MemoryEffects.SequenceEqual(
                replay.ParentPlan.MemoryEffects)
            && successful.Instructions.SequenceEqual(replay.Instructions)
            && successful.MemoryRead == replay.MemoryRead
            && successful.EAXValue == replay.EAXValue
            && successful.EBXValue == replay.EBXValue
            && successful.ECXValue == replay.ECXValue
            && successful.ESIValue == replay.ESIValue
            && successful.FlagsAtStop == replay.FlagsAtStop
            && successful.LastReachedRawFileOffset == replay.LastReachedRawFileOffset
            && successful.StopBeforeRawFileOffset == replay.StopBeforeRawFileOffset);

        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan baseline =
            Project(freshOperationVptr: 0xA5A5A5A5);
        GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect parentStackStore =
            baseline.ParentPlan.MemoryEffects.Single(effect =>
                effect.Kind
                    == GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
                        .StoreFFFFFFFFAtESPPlus50
                && effect.Attempted
                && effect.Captured
                && effect.Committed);
        uint stackStoreAddress = parentStackStore.Address!.Value;
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan aliasedRead =
            Project(
                freshOperationVptr: 0x0BADC0DE,
                freshPostCallESI: stackStoreAddress);
        GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect aliasedParentStore =
            aliasedRead.ParentPlan.MemoryEffects.Single(effect =>
                effect.Kind
                    == GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
                        .StoreFFFFFFFFAtESPPlus50
                && effect.Attempted
                && effect.Captured
                && effect.Committed);
        check("The fresh vptr capture remains independent when its address aliases a committed parent stack store",
            aliasedRead.ExactCleanCompletion
            && aliasedParentStore.Address == stackStoreAddress
            && aliasedRead.ESIValue == stackStoreAddress
            && aliasedRead.MemoryRead is {
                Address: var aliasedReadAddress,
                Value: 0x0BADC0DE,
                Attempted: true,
                Captured: true
            }
            && aliasedReadAddress == aliasedParentStore.Address
            && aliasedParentStore.Value == uint.MaxValue
            && aliasedRead.MemoryRead.Value.Value != aliasedParentStore.Value
            && aliasedRead.EAXValue == 0x0BADC0DE);

        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan missing =
            Project(freshOperationVptr: null);
        bool missingClean = missing.ExactCleanCompletion;
        check("A missing fresh read is attempted, preserves parent EAX and DEC flags, and stops before MOV ECX",
            !missingClean
            && missing.ParentBranchAdmitted
            && missing.Outcome
                == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationOutcome
                    .MissingFreshOperationVptrDWORDAt34DB
            && missing.FreshOperationVptrCaptureState
                == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCaptureState
                    .Missing
            && missing.MemoryRead is {
                InstructionRawFileOffset: 0x00BD34DB,
                Address: var missingAddress,
                Value: null,
                Attempted: true,
                Captured: false
            }
            && missingAddress == missing.ESIValue
            && missing.EAXValue == missing.ParentPlan.EAXValue
            && missing.ECXValue is null
            && missing.ESIValue == missing.ParentPlan.ESIValue
            && missing.EBXValue == 0
            && missing.FlagsAtStop == missing.ParentPlan.FlagsAtStop
            && missing.FlagsPropagated
            && missing.Instructions.Length == 1
            && missing.Instructions[0] is {
                Kind: GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationInstructionKind
                    .MoveEAXFromESIAt34DB,
                Attempted: true,
                Reached: true,
                Committed: false,
                DestinationRegister: "EAX",
                DestinationValue: var retainedEAX,
                SourceRegister: "[ESI]",
                FlagsBefore: var missingFlagsBefore,
                FlagsAfter: var missingFlagsAfter
            }
            && retainedEAX == missing.ParentPlan.EAXValue
            && missingFlagsBefore == missing.ParentPlan.FlagsAtStop
            && missingFlagsAfter == missing.ParentPlan.FlagsAtStop
            && missing.LastReachedRawFileOffset == 0x00BD34DB
            && missing.StopBeforeRawFileOffset == 0x00BD34DB
            && missing.ParentExactBoundedCompletion
            && missing.ParentPlan.ExactBoundedCompletion
            && !missing.ChildBoundedCompletionBoundaryReached
            && !missing.ExactChildCompletion
            && !missing.ExactBoundedCompletion
            && missing.MoveECXAttempted == false
            && missing.Slot4CallAttempted == false);

        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan takenParent =
            Project(freshOperationVptr: 0x11111111, secondRmwDWORD: 2);
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan missingEBX =
            Project(freshOperationVptr: 0x22222222, freshPostCallEBX: null);
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan missingESI =
            Project(freshOperationVptr: 0x33333333, freshPostCallESI: null);
        GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan pendingReturn =
            Project(freshOperationVptr: 0x44444444, returnedNormally: null);
        check("Taken, incomplete, and pending parents reject the suffix while preserving their actual bounds",
            !takenParent.ExactCleanCompletion
            && !missingEBX.ExactCleanCompletion
            && !missingESI.ExactCleanCompletion
            && !pendingReturn.ExactCleanCompletion
            && takenParent.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome.JnzTakenTo34E2
            && takenParent.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && missingEBX.ParentPlan.StopBeforeRawFileOffset == 0x00BD34D4
            && missingESI.ParentPlan.StopBeforeRawFileOffset == 0x00BD34D1
            && pendingReturn.ParentPlan.StopBeforeRawFileOffset == 0x00BD34CF
            && new[] { takenParent, missingEBX, missingESI, pendingReturn }.All(plan =>
                !plan.ParentBranchAdmitted
                && plan.Instructions.IsEmpty
                && plan.MemoryRead is null
                && plan.FreshOperationVptrCaptureState
                    == GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationCaptureState
                        .SuppliedButUnused
                && plan.StopBeforeRawFileOffset == plan.ParentPlan.StopBeforeRawFileOffset
                && plan.ExactBoundedCompletion == plan.ParentExactBoundedCompletion
                && !plan.ChildBoundedCompletionBoundaryReached
                && plan.Slot4CallRawFileOffset is null));
    }

    private static GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationPlan Project(
        uint? freshOperationVptr,
        uint? freshPostCallEBX = 0x1234,
        uint? freshPostCallESI = 0x9000,
        bool? returnedNormally = true,
        uint secondRmwDWORD = 1)
    {
        GameSocketParserDiagnosticCleanupSecondXaddBranchCapture parentCapture =
            CreateParentCapture(
                freshPostCallEBX,
                freshPostCallESI,
                returnedNormally,
                secondRmwDWORD);
        return GameSocketParserDiagnosticCleanupSecondVirtualCallPreparationProjection.Project(
            new(parentCapture, freshOperationVptr));
    }

    private static GameSocketParserDiagnosticCleanupSecondXaddBranchCapture CreateParentCapture(
        uint? freshPostCallEBX,
        uint? freshPostCallESI,
        bool? returnedNormally,
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
            new(preparationCapture, 0x12345678, returnedNormally);
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
