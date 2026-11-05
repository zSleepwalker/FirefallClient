using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupVirtualCallPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection
                .CoordinatorProvidedBytes);
        check("The supplied bytes anchor the independent vptr read, ECX move, and exclusive call stop",
            !GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection
                .BytesIndependentlyVerified
            && suppliedBytes.Length == 16
            && string.Join(" ", suppliedBytes.Select(value => value.ToString("X2")))
                == GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection
                    .CoordinatorProvidedBytes
            && GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection
                .VptrReadRawFileOffset == 0x00BD34CB
            && GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection
                .VptrReadEncoding == "8B 06"
            && GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection
                .MoveECXRawFileOffset == 0x00BD34CD
            && GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection
                .MoveECXEncoding == "8B CE"
            && GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection
                .IndirectCallRawFileOffset == 0x00BD34CF
            && suppliedBytes.Skip(6).Take(4).SequenceEqual(ParseBytes("8B 06 8B CE")));

        uint[] freshVptrValues = { 0, uint.MaxValue, 1, 4310 };
        bool valuesExact = true;
        bool sawZero = false;
        bool sawMaximum = false;
        bool sawAliasValue = false;
        bool sawAcceptedValue = false;
        foreach (uint freshVptr in freshVptrValues)
        {
            GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan plan =
                Project(freshVptrDWORDAtESI: freshVptr);
            GameSocketParserStoredByteSelectorGateFlags expectedFlags = DecrementOneFlags();
            valuesExact &= plan.ParentBranchAdmitted
                && plan.ParentPlan.IsCleanCompletion
                && plan.ParentPlan.Outcome
                    == GameSocketParserDiagnosticCleanupXaddBranchOutcome
                        .JnzNotTakenTo34CB
                && plan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34CB
                && plan.ParentPlan.ParentPlan.MemoryRead is {
                    Value: 1,
                    Attempted: true,
                    Captured: true,
                    ObservedAtLockedRmw: true
                }
                && plan.ParentPlan.ParentPlan.MemoryWrite is {
                    OldValue: 1,
                    NewValue: 0,
                    Attempted: true,
                    Committed: true
                }
                && plan.Outcome
                    == GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome
                        .BoundedBeforeIndirectCallAt34CF
                && plan.FreshVptrCaptureState
                    == GameSocketParserDiagnosticCleanupVirtualCallPreparationCaptureState
                        .Captured
                && plan.ExactBoundedCompletion
                && plan.IsCleanCompletion
                && plan.StopBeforeRawFileOffset == 0x00BD34CF
                && plan.IndirectCallRawFileOffset == 0x00BD34CF
                && plan.MemoryRead is {
                    InstructionRawFileOffset: 0x00BD34CB,
                    Address: 0x7000,
                    Value: var capturedVptr,
                    Attempted: true,
                    Captured: true
                }
                && capturedVptr == freshVptr
                && plan.EAXValue == freshVptr
                && plan.ECXValue == 0x7000
                && plan.ESIValue == 0x7000
                && plan.EBXValue == uint.MaxValue
                && plan.ResultESP == plan.ParentPlan.ResultESP
                && plan.ESPUnchanged
                && plan.EBXUnchanged
                && plan.ESIUnchanged
                && plan.FlagsAtStop == expectedFlags
                && plan.FlagsPropagated
                && plan.Instructions.Length == 2
                && plan.Instructions[0] is {
                    Kind: GameSocketParserDiagnosticCleanupVirtualCallPreparationInstructionKind
                        .MoveEAXFromESIAt34CB,
                    InstructionRawFileOffset: 0x00BD34CB,
                    Encoding: "8B 06",
                    DestinationRegister: "EAX",
                    DestinationValue: var eaxValue,
                    SourceRegister: "[ESI]",
                    SourceValue: 0x7000,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: var eaxFlagsBefore,
                    FlagsAfter: var eaxFlagsAfter
                }
                && eaxValue == freshVptr
                && eaxFlagsBefore == expectedFlags
                && eaxFlagsAfter == expectedFlags
                && plan.Instructions[1] is {
                    Kind: GameSocketParserDiagnosticCleanupVirtualCallPreparationInstructionKind
                        .MoveECXFromESIAt34CD,
                    InstructionRawFileOffset: 0x00BD34CD,
                    Encoding: "8B CE",
                    DestinationRegister: "ECX",
                    DestinationValue: 0x7000,
                    SourceRegister: "ESI",
                    SourceValue: 0x7000,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: var ecxFlagsBefore,
                    FlagsAfter: var ecxFlagsAfter
                }
                && ecxFlagsBefore == expectedFlags
                && ecxFlagsAfter == expectedFlags
                && plan.Reads.SequenceEqual(plan.ParentPlan.Reads)
                && plan.MemoryEffects.SequenceEqual(plan.ParentPlan.MemoryEffects)
                && plan.ParentPlan.Instructions.SequenceEqual(
                    GameSocketParserDiagnosticCleanupXaddBranchProjection.Project(
                        plan.Capture.ParentCapture).Instructions)
                && plan.ParentPlan.ParentPlan.MemoryRead?.Value == 1
                && plan.ParentPlan.ParentPlan.MemoryWrite?.NewValue == 0
                && plan.OrderedSemanticPrefixPreserved
                && !plan.IndirectCallAttempted
                && !plan.IndirectCallReached
                && !plan.IndirectCallCommitted
                && !plan.VtableSlotRead
                && !plan.CleanupReleaseIdentityModeled
                && !plan.NativeMemoryEffectsModeled
                && !plan.NativeABIOrProtocolSemanticsModeled
                && !plan.ParserExecutionModeled
                && !plan.RuntimeSampleExecuted
                && !plan.ParentInstructionsReplayed;
            sawZero |= freshVptr == 0 && plan.EAXValue == 0;
            sawMaximum |= freshVptr == uint.MaxValue && plan.EAXValue == uint.MaxValue;
            sawAliasValue |= freshVptr == 1
                && plan.ParentPlan.ParentPlan.MemoryRead?.Value == freshVptr
                && plan.MemoryRead?.Value == freshVptr
                && plan.ParentPlan.ParentPlan.MemoryRead?.Address == 0x7004
                && plan.MemoryRead?.Address == 0x7000;
            sawAcceptedValue |= freshVptr == 4310 && plan.EAXValue == 4310;
        }

        check("A fresh [ESI] read independently supplies EAX; MOV ECX,ESI preserves flags and parent evidence",
            valuesExact && sawZero && sawMaximum && sawAliasValue && sawAcceptedValue);
        GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan aliasedStore =
            Project(freshVptrDWORDAtESI: 0xDEADBEEF, esi: 0x9050);
        check("A fresh vptr capture does not inherit an earlier aliased stack store",
            aliasedStore.MemoryRead is { Address: 0x9050, Value: 0xDEADBEEF }
            && aliasedStore.MemoryEffects.Any(effect =>
                effect.Address == 0x9050 && effect.Value == uint.MaxValue && effect.Committed)
            && aliasedStore.EAXValue == 0xDEADBEEF
            && aliasedStore.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan accepted =
            Project(freshVptrDWORDAtESI: 4310);
        GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan replay =
            GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection.Project(
                accepted.Capture);
        check("The immutable clean-34CB capture reprojects with its RMW and ordered prefix unchanged",
            accepted.Capture.ParentCapture.ParentCapture.FreshDWORDObservedAtLockedRmw == 1
            && accepted.Capture.FreshVptrDWORDAtESI == 4310
            && accepted.ParentPlan.Capture == replay.ParentPlan.Capture
            && accepted.ParentPlan.Instructions.SequenceEqual(replay.ParentPlan.Instructions)
            && accepted.ParentPlan.ParentPlan.MemoryRead == replay.ParentPlan.ParentPlan.MemoryRead
            && accepted.ParentPlan.ParentPlan.MemoryWrite == replay.ParentPlan.ParentPlan.MemoryWrite
            && accepted.ParentPlan.Reads.SequenceEqual(replay.ParentPlan.Reads)
            && accepted.ParentPlan.MemoryEffects.SequenceEqual(replay.ParentPlan.MemoryEffects)
            && accepted.Instructions.SequenceEqual(replay.Instructions)
            && accepted.MemoryRead == replay.MemoryRead
            && accepted.Outcome == replay.Outcome
            && accepted.EAXValue == replay.EAXValue
            && accepted.ECXValue == replay.ECXValue
            && accepted.EBXValue == replay.EBXValue
            && accepted.ESIValue == replay.ESIValue
            && accepted.ResultESP == replay.ResultESP
            && accepted.FlagsAtStop == replay.FlagsAtStop
            && accepted.StopBeforeRawFileOffset == replay.StopBeforeRawFileOffset
            && accepted.OrderedSemanticPrefixPreserved
            && replay.OrderedSemanticPrefixPreserved
            && accepted.IsCleanCompletion
            && replay.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan missingVptrRead =
            Project(freshVptrDWORDAtESI: null);
        check("A missing fresh [ESI] read stops at 34CB without overwriting EAX or moving ECX",
            missingVptrRead.ParentBranchAdmitted
            && missingVptrRead.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome
                    .MissingFreshVptrDWORDAt34CB
            && missingVptrRead.FreshVptrCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallPreparationCaptureState.Missing
            && missingVptrRead.MemoryRead is {
                Address: 0x7000,
                Value: null,
                Attempted: true,
                Captured: false
            }
            && missingVptrRead.Instructions.Length == 1
            && missingVptrRead.Instructions[0] is {
                InstructionRawFileOffset: 0x00BD34CB,
                Attempted: false,
                Reached: true,
                Committed: false
            }
            && missingVptrRead.EAXValue == 0x7004
            && missingVptrRead.ECXValue == 1
            && missingVptrRead.FlagsAtStop == DecrementOneFlags()
            && missingVptrRead.StopBeforeRawFileOffset == 0x00BD34CB
            && missingVptrRead.IndirectCallRawFileOffset == 0x00BD34CF
            && !missingVptrRead.ExactBoundedCompletion
            && !missingVptrRead.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan takenAncestor =
            Project(freshVptrDWORDAtESI: 4310, freshDWORDAtLockedRmw: 0);
        check("A taken parent JNZ preserves its actual 34E2 endpoint and leaves the vptr capture unused",
            !takenAncestor.ParentBranchAdmitted
            && takenAncestor.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupXaddBranchOutcome.JnzTakenTo34E2
            && takenAncestor.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && takenAncestor.FreshVptrCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallPreparationCaptureState
                    .SuppliedButUnused
            && takenAncestor.Instructions.IsEmpty
            && takenAncestor.MemoryRead is null
            && takenAncestor.StopBeforeRawFileOffset == 0x00BD34E2
            && takenAncestor.ExactBoundedCompletion
            && !takenAncestor.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan missingRmw =
            Project(freshVptrDWORDAtESI: 4310, freshDWORDAtLockedRmw: null);
        check("A missing parent RMW read keeps its 34C5 stop and does not consume the vptr capture",
            !missingRmw.ParentBranchAdmitted
            && missingRmw.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupXaddOutcome.MissingFreshDWORDAt34C5
            && missingRmw.ParentPlan.StopBeforeRawFileOffset == 0x00BD34C5
            && missingRmw.FreshVptrCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallPreparationCaptureState
                    .SuppliedButUnused
            && missingRmw.Instructions.IsEmpty
            && missingRmw.MemoryRead is null
            && missingRmw.StopBeforeRawFileOffset == 0x00BD34C5
            && !missingRmw.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan otherAncestor =
            Project(freshVptrDWORDAtESI: 4310, comparisonValue: 2);
        check("A different ancestor forwards its 31F8 stop without fabricating a vptr read",
            !otherAncestor.ParentBranchAdmitted
            && otherAncestor.ParentPlan.ParentPlan.ParentPlan.ParentPlan.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticDestinationBranchGateOutcome.JaeTakenTo31F8
            && otherAncestor.ParentPlan.StopBeforeRawFileOffset == 0x00BD31F8
            && otherAncestor.FreshVptrCaptureState
                == GameSocketParserDiagnosticCleanupVirtualCallPreparationCaptureState
                    .SuppliedButUnused
            && otherAncestor.Instructions.IsEmpty
            && otherAncestor.MemoryRead is null
            && otherAncestor.StopBeforeRawFileOffset == 0x00BD31F8
            && !otherAncestor.IsCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupVirtualCallPreparationPlan Project(
        uint? freshVptrDWORDAtESI,
        uint? freshDWORDAtLockedRmw = 1,
        uint? esi = 0x7000,
        uint comparisonValue = 0,
        uint postCallESP = 0x8FEC,
        uint? freshEBP = 0x7000)
    {
        GameSocketParserDiagnosticCleanupStackPrefixCapture stackCapture =
            CreateParentCapture(
                comparisonValue,
                postCallESP,
                freshEBP,
                esi);
        GameSocketParserDiagnosticCleanupZeroBranchCapture zeroBranchCapture =
            new(stackCapture);
        GameSocketParserDiagnosticCleanupRegisterPreparationCapture registerCapture =
            new(zeroBranchCapture);
        GameSocketParserDiagnosticCleanupXaddCapture xaddCapture =
            new(registerCapture, freshDWORDAtLockedRmw);
        GameSocketParserDiagnosticCleanupXaddBranchCapture branchCapture =
            new(xaddCapture);
        return GameSocketParserDiagnosticCleanupVirtualCallPreparationProjection.Project(
            new(branchCapture, freshVptrDWORDAtESI));
    }

    private static GameSocketParserDiagnosticCleanupStackPrefixCapture CreateParentCapture(
        uint comparisonValue,
        uint postCallESP,
        uint? freshEBP,
        uint? freshDWORDAtESPPlus3C)
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
                    FreshPostCallESP: postCallESP),
                FreshPostCallESI: 0x7000,
                FreshCounterDWORD: 4);
        return new(
            new(
                new(
                    counterCapture,
                    freshEBP,
                    comparisonValue,
                    FreshDWORDAtESPPlus14: 0xCAFEBABE)),
            freshDWORDAtESPPlus3C);
    }

    private static GameSocketParserStoredByteSelectorGateFlags DecrementOneFlags() =>
        new(
            Carry: true,
            Parity: true,
            AuxiliaryCarry: true,
            Zero: true,
            Sign: false,
            Overflow: false);

    private static byte[] ParseBytes(string encoding) =>
        encoding.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
