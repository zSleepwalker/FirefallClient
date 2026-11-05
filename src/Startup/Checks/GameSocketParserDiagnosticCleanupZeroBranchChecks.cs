using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupZeroBranchChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupZeroBranchProjection.CoordinatorProvidedBytes);
        check("The supplied cleanup bytes identify only the JZ branch and its bounded target",
            !GameSocketParserDiagnosticCleanupZeroBranchProjection.BytesIndependentlyVerified
            && suppliedBytes.Length == 32
            && string.Join(" ", suppliedBytes.Select(value => value.ToString("X2")))
                == GameSocketParserDiagnosticCleanupZeroBranchProjection.CoordinatorProvidedBytes
            && GameSocketParserDiagnosticCleanupZeroBranchProjection.JzRawFileOffset
                == 0x00BD34BB
            && GameSocketParserDiagnosticCleanupZeroBranchProjection.JzEncoding == "74 25"
            && GameSocketParserDiagnosticCleanupZeroBranchProjection.FallThroughRawFileOffset
                == 0x00BD34BD
            && GameSocketParserDiagnosticCleanupZeroBranchProjection.JzTargetRawFileOffset
                == 0x00BD34E2);

        uint[] values = { 0, 1, uint.MaxValue, 0x80000000, 0x7FFFFFFF };
        bool casesExact = true;
        bool sawTaken = false;
        bool sawNotTaken = false;
        bool sawAuxiliaryCarryUnknown = false;
        foreach (uint value in values)
        {
            GameSocketParserDiagnosticCleanupZeroBranchPlan plan =
                Project(freshDWORDAtESPPlus3C: value);
            GameSocketParserStoredByteSelectorGateFlags expected =
                ExpectedTestFlags(value);
            uint expectedStop = value == 0
                ? GameSocketParserDiagnosticCleanupZeroBranchProjection.JzTargetRawFileOffset
                : GameSocketParserDiagnosticCleanupZeroBranchProjection.FallThroughRawFileOffset;
            GameSocketParserDiagnosticCleanupZeroBranchOutcome expectedOutcome = value == 0
                ? GameSocketParserDiagnosticCleanupZeroBranchOutcome.JzTakenTo34E2
                : GameSocketParserDiagnosticCleanupZeroBranchOutcome.JzNotTakenTo34BD;
            casesExact &= plan.ParentPrefixAdmitted
                && plan.ParentPlan.IsCleanCompletion
                && plan.ParentPlan.StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticCleanupZeroBranchProjection.JzRawFileOffset
                && plan.ParentPlan.ESIValue == value
                && plan.Outcome == expectedOutcome
                && plan.ExactBoundedCompletion
                && plan.IsCleanCompletion
                && plan.StopBeforeRawFileOffset == expectedStop
                && plan.BranchTargetRawFileOffset == expectedStop
                && plan.ESIValue == value
                && plan.ResultESP == plan.ParentPlan.ResultESP
                && plan.FlagsAtStop == expected
                && plan.Instructions.Length == 1
                && plan.Instructions[0] is {
                    Kind: GameSocketParserDiagnosticCleanupZeroBranchInstructionKind.JzAt34BB,
                    InstructionRawFileOffset: 0x00BD34BB,
                    Encoding: "74 25",
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    BranchTargetRawFileOffset: var target,
                    FlagsBefore: var flagsBefore,
                    FlagsAfter: var flagsAfter
                }
                && target == expectedStop
                && flagsBefore == expected
                && flagsAfter == expected
                && plan.JzAttempted
                && plan.JzReached
                && plan.JzCommitted
                && plan.JzTaken == (value == 0)
                && plan.FlagsPreserved
                && plan.ESIAndESPInherited
                && plan.OrderedSemanticPrefixPreserved
                && !plan.BranchTargetInstructionExecuted
                && !plan.CleanupReleaseIdentityModeled
                && !plan.NativeAtomicSemanticsModeled
                && !plan.NativeMemoryEffectsModeled
                && !plan.NativeABIOrProtocolSemanticsModeled
                && !plan.ParserExecutionModeled
                && !plan.RuntimeSampleExecuted
                && !plan.ParentInstructionsReplayed
                && plan.Reads.SequenceEqual(plan.ParentPlan.Reads)
                && plan.MemoryEffects.SequenceEqual(plan.ParentPlan.MemoryEffects)
                && plan.ParentPlan.Instructions.Length == 3
                && plan.ParentPlan.Reads.Length == 1
                && plan.ParentPlan.MemoryEffects.Length
                    == plan.ParentPlan.ParentPlan.MemoryEffects.Length + 2;
            sawTaken |= value == 0 && plan.StopBeforeRawFileOffset == 0x00BD34E2;
            sawNotTaken |= value != 0 && plan.StopBeforeRawFileOffset == 0x00BD34BD;
            sawAuxiliaryCarryUnknown |= expected.AuxiliaryCarry is null
                && plan.FlagsAtStop?.AuxiliaryCarry is null;
        }

        check("TEST's zero result selects one branch; flags, ESI, ESP, and the ordered parent prefix persist",
            casesExact && sawTaken && sawNotTaken && sawAuxiliaryCarryUnknown);

        GameSocketParserDiagnosticCleanupZeroBranchPlan acceptedCapture =
            Project(freshDWORDAtESPPlus3C: 4296);
        GameSocketParserDiagnosticCleanupZeroBranchPlan replay =
            GameSocketParserDiagnosticCleanupZeroBranchProjection.Project(
                acceptedCapture.Capture);
        check("The immutable clean-34BB capture reprojects without changing evidence or branch effects",
            acceptedCapture.Capture == replay.Capture
            && acceptedCapture.ParentPlan.Capture == replay.ParentPlan.Capture
            && acceptedCapture.ParentPlan.Instructions.SequenceEqual(
                replay.ParentPlan.Instructions)
            && acceptedCapture.ParentPlan.Reads.SequenceEqual(replay.ParentPlan.Reads)
            && acceptedCapture.ParentPlan.MemoryEffects.SequenceEqual(
                replay.ParentPlan.MemoryEffects)
            && acceptedCapture.Instructions.SequenceEqual(replay.Instructions)
            && acceptedCapture.Outcome == replay.Outcome
            && acceptedCapture.StopBeforeRawFileOffset == 0x00BD34BD
            && acceptedCapture.StopBeforeRawFileOffset == replay.StopBeforeRawFileOffset
            && acceptedCapture.IsCleanCompletion
            && replay.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupZeroBranchPlan missingRead =
            Project(freshDWORDAtESPPlus3C: null);
        check("A missing parent stack read keeps its prior stop and does not fabricate a JZ",
            !missingRead.ParentPrefixAdmitted
            && missingRead.Outcome
                == GameSocketParserDiagnosticCleanupZeroBranchOutcome.ParentPrefixNotAdmitted
            && missingRead.ParentPlan.StoreCommitted
            && !missingRead.ParentPlan.FreshDWORDCaptured
            && missingRead.ParentPlan.StopBeforeRawFileOffset == 0x00BD34B5
            && missingRead.Instructions.IsEmpty
            && !missingRead.JzAttempted
            && missingRead.BranchTargetRawFileOffset is null
            && missingRead.StopBeforeRawFileOffset
                == missingRead.ParentPlan.StopBeforeRawFileOffset
            && missingRead.ExactBoundedCompletion
                == missingRead.ParentPlan.ExactBoundedCompletion
            && !missingRead.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupZeroBranchPlan alternateAncestor =
            Project(comparisonValue: 2, freshDWORDAtESPPlus3C: 0);
        check("The alternate 31F8 ancestor remains selected without entering the cleanup branch",
            !alternateAncestor.ParentPrefixAdmitted
            && alternateAncestor.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticDestinationBranchGateOutcome.JaeTakenTo31F8
            && alternateAncestor.ParentPlan.StopBeforeRawFileOffset == 0x00BD31F8
            && alternateAncestor.Instructions.IsEmpty
            && alternateAncestor.StopBeforeRawFileOffset == 0x00BD31F8
            && alternateAncestor.BranchTargetRawFileOffset is null
            && alternateAncestor.ParentPlan.FreshDWORDAtESPPlus3CCaptureState
                == GameSocketParserDiagnosticCleanupStackPrefixCaptureState.SuppliedButUnused);

        GameSocketParserDiagnosticCleanupZeroBranchPlan rejectedAncestor =
            Project(freshEBP: null, freshDWORDAtESPPlus3C: 0);
        check("A rejected parent prefix preserves its stop and leaves the supplied read unused",
            !rejectedAncestor.ParentPrefixAdmitted
            && rejectedAncestor.Instructions.IsEmpty
            && rejectedAncestor.StopBeforeRawFileOffset
                == rejectedAncestor.ParentPlan.StopBeforeRawFileOffset
            && rejectedAncestor.ParentPlan.FreshDWORDAtESPPlus3CCaptureState
                == GameSocketParserDiagnosticCleanupStackPrefixCaptureState.SuppliedButUnused
            && !rejectedAncestor.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupZeroBranchPlan nonNormalParent =
            ProjectWithReturnObservation(normalReturnObserved: false);
        GameSocketParserDiagnosticCleanupZeroBranchPlan pendingParent =
            ProjectWithReturnObservation(normalReturnObserved: null);
        check("Non-normal and pending ancestors remain bounded at their own parent stops",
            !nonNormalParent.ParentPrefixAdmitted
            && !pendingParent.ParentPrefixAdmitted
            && nonNormalParent.ParentPlan.NormalReturnObserved == false
            && pendingParent.ParentPlan.NormalReturnObserved is null
            && nonNormalParent.Instructions.IsEmpty
            && pendingParent.Instructions.IsEmpty
            && nonNormalParent.StopBeforeRawFileOffset
                == nonNormalParent.ParentPlan.StopBeforeRawFileOffset
            && pendingParent.StopBeforeRawFileOffset
                == pendingParent.ParentPlan.StopBeforeRawFileOffset
            && !nonNormalParent.IsCleanCompletion
            && !pendingParent.IsCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupZeroBranchPlan Project(
        uint? freshDWORDAtESPPlus3C,
        uint comparisonValue = 0,
        uint postCallESP = 0x8FEC,
        uint? freshEBP = 0x7000) =>
        GameSocketParserDiagnosticCleanupZeroBranchProjection.Project(
            new(
                CreateParentCapture(
                    comparisonValue,
                    postCallESP,
                    freshEBP,
                    freshDWORDAtESPPlus3C)));

    private static GameSocketParserDiagnosticCleanupZeroBranchPlan ProjectWithReturnObservation(
        bool? normalReturnObserved)
    {
        GameSocketParserDiagnosticCleanupStackPrefixCapture capture =
            CreateParentCapture(0, 0x8FEC, 0x7000, 4296);
        GameSocketParserDiagnosticDestinationBranchGateCapture branchCapture =
            capture.ParentCapture;
        GameSocketParserDiagnosticDestinationPrefixCapture prefixCapture =
            branchCapture.ParentCapture;
        GameSocketParserSelectorDiagnosticCounterContinuationCapture counterCapture =
            prefixCapture.ParentCapture;
        GameSocketParserSelectorDiagnosticReturnCleanupCapture returnCapture =
            counterCapture.ParentCapture with
            {
                NormalReturnObserved = normalReturnObserved
            };
        counterCapture = counterCapture with { ParentCapture = returnCapture };
        prefixCapture = prefixCapture with { ParentCapture = counterCapture };
        branchCapture = branchCapture with { ParentCapture = prefixCapture };
        capture = capture with { ParentCapture = branchCapture };
        return GameSocketParserDiagnosticCleanupZeroBranchProjection.Project(
            new(capture));
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

    private static GameSocketParserStoredByteSelectorGateFlags ExpectedTestFlags(uint value) =>
        new(
            Carry: false,
            Parity: (System.Numerics.BitOperations.PopCount((uint)(byte)value) & 1) == 0,
            AuxiliaryCarry: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static byte[] ParseBytes(string encoding) =>
        encoding.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
