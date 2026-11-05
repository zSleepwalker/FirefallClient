using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupRegisterPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupRegisterPreparationProjection.CoordinatorProvidedBytes);
        check("The supplied cleanup bytes anchor OR, LEA, MOV, and the exclusive XADD stop",
            !GameSocketParserDiagnosticCleanupRegisterPreparationProjection
                .BytesIndependentlyVerified
            && suppliedBytes.Length == 32
            && string.Join(" ", suppliedBytes.Select(value => value.ToString("X2")))
                == GameSocketParserDiagnosticCleanupRegisterPreparationProjection
                    .CoordinatorProvidedBytes
            && GameSocketParserDiagnosticCleanupRegisterPreparationProjection.OrRawFileOffset
                == 0x00BD34BD
            && GameSocketParserDiagnosticCleanupRegisterPreparationProjection.OrEncoding
                == "83 CB FF"
            && GameSocketParserDiagnosticCleanupRegisterPreparationProjection.LeaRawFileOffset
                == 0x00BD34C0
            && GameSocketParserDiagnosticCleanupRegisterPreparationProjection.LeaEncoding
                == "8D 46 04"
            && GameSocketParserDiagnosticCleanupRegisterPreparationProjection.MoveRawFileOffset
                == 0x00BD34C3
            && GameSocketParserDiagnosticCleanupRegisterPreparationProjection.MoveEncoding
                == "8B CB"
            && GameSocketParserDiagnosticCleanupRegisterPreparationProjection.LockXaddRawFileOffset
                == 0x00BD34C5);

        uint[] esiValues = { 1, 0xFFFFFFFC, uint.MaxValue, 0x7FFFFFFF, 0x80000000 };
        bool allValuesExact = true;
        bool sawWrapToZero = false;
        bool sawWrapToThree = false;
        foreach (uint esi in esiValues)
        {
            GameSocketParserDiagnosticCleanupRegisterPreparationPlan plan = Project(esi);
            uint expectedEax = unchecked(esi + 4);
            GameSocketParserStoredByteSelectorGateFlags expectedFlags = OrFlags();
            allValuesExact &= plan.ParentBranchAdmitted
                && plan.ParentPlan.IsCleanCompletion
                && plan.ParentPlan.Outcome
                    == GameSocketParserDiagnosticCleanupZeroBranchOutcome.JzNotTakenTo34BD
                && plan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34BD
                && plan.Outcome
                    == GameSocketParserDiagnosticCleanupRegisterPreparationOutcome
                        .BoundedBeforeLockXaddAt34C5
                && plan.ExactBoundedCompletion
                && plan.IsCleanCompletion
                && plan.StopBeforeRawFileOffset == 0x00BD34C5
                && plan.LockXaddRawFileOffset == 0x00BD34C5
                && plan.ESIValue == esi
                && plan.EAXValue == expectedEax
                && plan.EBXValue == uint.MaxValue
                && plan.ECXValue == uint.MaxValue
                && plan.ResultESP == plan.ParentPlan.ResultESP
                && plan.ESPUnchanged
                && plan.ESIInherited
                && plan.FlagsAtStop == expectedFlags
                && plan.FlagsPropagated
                && plan.Instructions.Length == 3
                && plan.Instructions[0] is {
                    Kind: GameSocketParserDiagnosticCleanupRegisterPreparationInstructionKind
                        .OrEBXWithMinusOneAt34BD,
                    InstructionRawFileOffset: 0x00BD34BD,
                    Encoding: "83 CB FF",
                    DestinationRegister: "EBX",
                    DestinationValue: uint.MaxValue,
                    SourceRegister: "EBX",
                    SourceValue: null,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsAfter: var orFlags
                }
                && orFlags == expectedFlags
                && plan.Instructions[1] is {
                    Kind: GameSocketParserDiagnosticCleanupRegisterPreparationInstructionKind
                        .LeaEAXFromESIPlusFourAt34C0,
                    InstructionRawFileOffset: 0x00BD34C0,
                    Encoding: "8D 46 04",
                    DestinationRegister: "EAX",
                    DestinationValue: var leaValue,
                    SourceRegister: "ESI + 4",
                    SourceValue: var leaSource,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: var leaFlagsBefore,
                    FlagsAfter: var leaFlagsAfter
                }
                && leaValue == expectedEax
                && leaSource == esi
                && leaFlagsBefore == expectedFlags
                && leaFlagsAfter == expectedFlags
                && plan.Instructions[2] is {
                    Kind: GameSocketParserDiagnosticCleanupRegisterPreparationInstructionKind
                        .MoveECXFromEBXAt34C3,
                    InstructionRawFileOffset: 0x00BD34C3,
                    Encoding: "8B CB",
                    DestinationRegister: "ECX",
                    DestinationValue: uint.MaxValue,
                    SourceRegister: "EBX",
                    SourceValue: uint.MaxValue,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: var moveFlagsBefore,
                    FlagsAfter: var moveFlagsAfter
                }
                && moveFlagsBefore == expectedFlags
                && moveFlagsAfter == expectedFlags
                && plan.PreviousEBXValueCaptured == false
                && !plan.PreviousEBXCaptureRequired
                && !plan.LEAAddressDereferenced
                && plan.Reads.SequenceEqual(plan.ParentPlan.Reads)
                && plan.MemoryEffects.SequenceEqual(plan.ParentPlan.MemoryEffects)
                && plan.ParentPlan.Instructions.Length == 1
                && !plan.LockXaddAttempted
                && !plan.LockXaddReached
                && !plan.LockXaddCommitted
                && !plan.CleanupReleaseIdentityModeled
                && !plan.NativeAtomicSemanticsModeled
                && !plan.NativeMemoryEffectsModeled
                && !plan.NativeABIOrProtocolSemanticsModeled
                && !plan.ParserExecutionModeled
                && !plan.RuntimeSampleExecuted
                && !plan.ParentInstructionsReplayed
                && plan.OrderedSemanticPrefixPreserved;
            sawWrapToZero |= esi == 0xFFFFFFFC && expectedEax == 0;
            sawWrapToThree |= esi == uint.MaxValue && expectedEax == 3;
        }

        check("OR fixes EBX/ECX, LEA wraps ESI+4, and flags and ESP match the instruction semantics",
            allValuesExact && sawWrapToZero && sawWrapToThree);

        GameSocketParserDiagnosticCleanupRegisterPreparationPlan accepted =
            Project(4298);
        GameSocketParserDiagnosticCleanupRegisterPreparationPlan replay =
            GameSocketParserDiagnosticCleanupRegisterPreparationProjection.Project(
                accepted.Capture);
        check("The immutable clean-34BD capture reprojects with identical scalar and ordered evidence",
            accepted.Capture.ParentCapture.ParentCapture.FreshDWORDAtESPPlus3C
                == 4298
            && accepted.ParentPlan.Outcome == replay.ParentPlan.Outcome
            && accepted.ParentPlan.ParentPrefixAdmitted
                == replay.ParentPlan.ParentPrefixAdmitted
            && accepted.ParentPlan.StopBeforeRawFileOffset
                == replay.ParentPlan.StopBeforeRawFileOffset
            && accepted.ParentPlan.ESIValue == replay.ParentPlan.ESIValue
            && accepted.ParentPlan.ResultESP == replay.ParentPlan.ResultESP
            && accepted.ParentPlan.FlagsAtStop == replay.ParentPlan.FlagsAtStop
            && accepted.ParentPlan.Instructions.SequenceEqual(
                replay.ParentPlan.Instructions)
            && accepted.ParentPlan.Reads.SequenceEqual(replay.ParentPlan.Reads)
            && accepted.ParentPlan.MemoryEffects.SequenceEqual(
                replay.ParentPlan.MemoryEffects)
            && accepted.ParentPlan.IsCleanCompletion
                == replay.ParentPlan.IsCleanCompletion
            && accepted.Outcome == replay.Outcome
            && accepted.ESIValue == replay.ESIValue
            && accepted.EAXValue == replay.EAXValue
            && accepted.EBXValue == replay.EBXValue
            && accepted.ECXValue == replay.ECXValue
            && accepted.ResultESP == replay.ResultESP
            && accepted.FlagsAtStop == replay.FlagsAtStop
            && accepted.StopBeforeRawFileOffset == replay.StopBeforeRawFileOffset
            && accepted.Instructions.SequenceEqual(replay.Instructions)
            && accepted.Reads.SequenceEqual(replay.Reads)
            && accepted.MemoryEffects.SequenceEqual(replay.MemoryEffects)
            && accepted.IsCleanCompletion == replay.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupRegisterPreparationPlan missingParentRead =
            Project(esi: null);
        check("A missing parent stack read forwards its actual stop without register effects",
            !missingParentRead.ParentBranchAdmitted
            && missingParentRead.Outcome
                == GameSocketParserDiagnosticCleanupRegisterPreparationOutcome
                    .ParentBranchNotAdmitted
            && missingParentRead.ParentPlan.StopBeforeRawFileOffset == 0x00BD34B5
            && missingParentRead.Instructions.IsEmpty
            && missingParentRead.EAXValue is null
            && missingParentRead.EBXValue is null
            && missingParentRead.ECXValue is null
            && missingParentRead.LockXaddRawFileOffset is null
            && missingParentRead.StopBeforeRawFileOffset
                == missingParentRead.ParentPlan.StopBeforeRawFileOffset
            && missingParentRead.ExactBoundedCompletion
                == missingParentRead.ParentPlan.ExactBoundedCompletion
            && !missingParentRead.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupRegisterPreparationPlan rejectedZeroBranch =
            Project(esi: 0);
        check("The rejected zero branch keeps its 34E2 bound and executes no register preparation",
            !rejectedZeroBranch.ParentBranchAdmitted
            && rejectedZeroBranch.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupZeroBranchOutcome.JzTakenTo34E2
            && rejectedZeroBranch.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && rejectedZeroBranch.Instructions.IsEmpty
            && rejectedZeroBranch.StopBeforeRawFileOffset == 0x00BD34E2
            && rejectedZeroBranch.LockXaddRawFileOffset is null
            && !rejectedZeroBranch.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupRegisterPreparationPlan otherAncestor =
            Project(esi: 1, comparisonValue: 2);
        check("A different ancestor forwards its 31F8 bound without entering this suffix",
            !otherAncestor.ParentBranchAdmitted
            && otherAncestor.ParentPlan.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticDestinationBranchGateOutcome.JaeTakenTo31F8
            && otherAncestor.ParentPlan.StopBeforeRawFileOffset == 0x00BD31F8
            && otherAncestor.Instructions.IsEmpty
            && otherAncestor.StopBeforeRawFileOffset == 0x00BD31F8
            && otherAncestor.LockXaddRawFileOffset is null
            && !otherAncestor.IsCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupRegisterPreparationPlan Project(
        uint? esi,
        uint comparisonValue = 0,
        uint postCallESP = 0x8FEC,
        uint? freshEBP = 0x7000) =>
        GameSocketParserDiagnosticCleanupRegisterPreparationProjection.Project(
            new(
                new(
                    CreateParentCapture(
                        comparisonValue,
                        postCallESP,
                        freshEBP,
                        esi))));

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

    private static GameSocketParserStoredByteSelectorGateFlags OrFlags() =>
        new(
            Carry: false,
            Parity: true,
            AuxiliaryCarry: null,
            Zero: false,
            Sign: true,
            Overflow: false);

    private static byte[] ParseBytes(string encoding) =>
        encoding.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
