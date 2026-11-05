using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupStackPrefixChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupStackPrefixProjection.CoordinatorProvidedBytes);
        check("The supplied cleanup-stack bytes cover the store, fresh load, and TEST only",
            !GameSocketParserDiagnosticCleanupStackPrefixProjection.BytesIndependentlyVerified
            && suppliedBytes.Length == 14
            && string.Join(" ", suppliedBytes.Select(value => value.ToString("X2")))
                == "C7 44 24 50 FF FF FF FF 8B 74 24 3C 85 F6"
            && GameSocketParserDiagnosticCleanupStackPrefixProjection.StoreRawFileOffset
                == 0x00BD34AD
            && GameSocketParserDiagnosticCleanupStackPrefixProjection.MoveESIRawFileOffset
                == 0x00BD34B5
            && GameSocketParserDiagnosticCleanupStackPrefixProjection.TestESIRawFileOffset
                == 0x00BD34B9
            && GameSocketParserDiagnosticCleanupStackPrefixProjection.JzRawFileOffset
                == 0x00BD34BB
            && GameSocketParserDiagnosticCleanupStackPrefixProjection.StoreEncoding
                == "C7 44 24 50 FF FF FF FF"
            && GameSocketParserDiagnosticCleanupStackPrefixProjection.MoveESIEncoding
                == "8B 74 24 3C"
            && GameSocketParserDiagnosticCleanupStackPrefixProjection.TestESIEncoding
                == "85 F6");

        uint[] values =
        {
            0,
            1,
            2,
            0x7F,
            0x80,
            0x7FFFFFFF,
            0x80000000,
            uint.MaxValue
        };
        bool casesExact = true;
        bool sawParitySet = false;
        bool sawParityClear = false;
        bool sawZeroSet = false;
        bool sawZeroClear = false;
        bool sawSignSet = false;
        bool sawSignClear = false;
        foreach (uint value in values)
        {
            GameSocketParserDiagnosticCleanupStackPrefixPlan plan =
                Project(freshDWORDAtESPPlus3C: value);
            GameSocketParserStoredByteSelectorGateFlags expected =
                ExpectedTestFlags(value);
            casesExact &= plan.ParentBranchAdmitted
                && plan.ParentPlan.IsCleanCompletion
                && plan.ParentPlan.Outcome
                    == GameSocketParserDiagnosticDestinationBranchGateOutcome
                        .JaeFallsThroughJmpTo34AD
                && plan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34AD
                && plan.ExactBoundedCompletion
                && plan.IsCleanCompletion
                && plan.ResultESP == 0x9000
                && plan.StoreAddress == 0x9050
                && plan.ReadAddress == 0x903C
                && plan.ESIValue == value
                && plan.ParentFlagsAtStop is { Carry: true }
                && plan.FlagsAtStop == expected
                && plan.Instructions.Length == 3
                && plan.Instructions[0] is {
                    Kind: GameSocketParserDiagnosticCleanupStackPrefixInstructionKind
                        .StoreFFFFFFFFAt34AD,
                    InstructionRawFileOffset: 0x00BD34AD,
                    MemoryAddress: 0x9050,
                    MemoryValue: uint.MaxValue,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: var storeFlagsBefore,
                    FlagsAfter: var storeFlagsAfter
                }
                && storeFlagsBefore == plan.ParentFlagsAtStop
                && storeFlagsAfter == plan.ParentFlagsAtStop
                && plan.Instructions[1] is {
                    Kind: GameSocketParserDiagnosticCleanupStackPrefixInstructionKind
                        .MoveESIFromESPPlus3CAt34B5,
                    InstructionRawFileOffset: 0x00BD34B5,
                    MemoryAddress: 0x903C,
                    MemoryValue: var movedValue,
                    DestinationRegister: "ESI",
                    DestinationValue: var esiValue,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: var moveFlagsBefore,
                    FlagsAfter: var moveFlagsAfter
                }
                && movedValue == value
                && esiValue == value
                && moveFlagsBefore == plan.ParentFlagsAtStop
                && moveFlagsAfter == plan.ParentFlagsAtStop
                && plan.Instructions[2] is {
                    Kind: GameSocketParserDiagnosticCleanupStackPrefixInstructionKind.TestESIAt34B9,
                    InstructionRawFileOffset: 0x00BD34B9,
                    Encoding: "85 F6",
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: var testFlagsBefore,
                    FlagsAfter: var testFlagsAfter
                }
                && testFlagsBefore == plan.ParentFlagsAtStop
                && testFlagsAfter == expected
                && plan.FreshDWORDReadAttempted
                && plan.FreshDWORDCaptured
                && plan.StoreCommitted
                && plan.ESICommitted
                && plan.TestCommitted
                && plan.ESPUnchanged
                && plan.StoreWidthBytes == 4
                && plan.FreshReadWidthBytes == 4
                && plan.Reads is [{
                    InstructionRawFileOffset: 0x00BD34B5,
                    Address: 0x903C,
                    Value: var readValue,
                    Attempted: true,
                    Captured: true,
                    Committed: false
                }]
                && readValue == value
                && plan.OrderedSemanticPrefixPreserved
                && !plan.BranchTargetInstructionExecuted
                && !plan.JzAttempted
                && !plan.JzReached
                && !plan.JzCommitted
                && !plan.CleanupReleaseIdentityModeled
                && !plan.NativeAtomicSemanticsModeled
                && plan.StopBeforeRawFileOffset == 0x00BD34BB;
            sawParitySet |= expected.Parity == true;
            sawParityClear |= expected.Parity == false;
            sawZeroSet |= expected.Zero == true;
            sawZeroClear |= expected.Zero == false;
            sawSignSet |= expected.Sign == true;
            sawSignClear |= expected.Sign == false;
        }

        check("Fresh ESI values drive TEST flags; the committed store and MOV preserve CMP flags",
            casesExact
            && sawParitySet
            && sawParityClear
            && sawZeroSet
            && sawZeroClear
            && sawSignSet
            && sawSignClear
            && values.All(value => ExpectedTestFlags(value).Carry == false
                && ExpectedTestFlags(value).Overflow == false
                && ExpectedTestFlags(value).AuxiliaryCarry is null));

        GameSocketParserDiagnosticCleanupStackPrefixPlan missingRead =
            Project(freshDWORDAtESPPlus3C: null);
        check("Missing fresh stack input retains the prior store and CMP flags, then stops at 34B5",
            missingRead.ParentBranchAdmitted
            && missingRead.Outcome
                == GameSocketParserDiagnosticCleanupStackPrefixOutcome.MissingFreshDWORDAtESPPlus3C
            && missingRead.FreshDWORDAtESPPlus3CCaptureState
                == GameSocketParserDiagnosticCleanupStackPrefixCaptureState.Missing
            && missingRead.StoreCommitted
            && missingRead.FreshDWORDReadAttempted
            && !missingRead.FreshDWORDCaptured
            && !missingRead.ESICommitted
            && !missingRead.TestAttempted
            && missingRead.ESIValue is null
            && missingRead.FlagsAtStop == missingRead.ParentFlagsAtStop
            && missingRead.Instructions.Length == 2
            && missingRead.Instructions[0].Committed
            && missingRead.Instructions[1] is {
                Kind: GameSocketParserDiagnosticCleanupStackPrefixInstructionKind
                    .MoveESIFromESPPlus3CAt34B5,
                Reached: true,
                Committed: false,
                MemoryAddress: 0x903C,
                MemoryValue: null
            }
            && missingRead.Reads is [{
                Address: 0x903C,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }]
            && missingRead.StopBeforeRawFileOffset == 0x00BD34B5
            && !missingRead.ExactBoundedCompletion
            && !missingRead.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupStackPrefixPlan aliasingValues =
            Project(
                comparisonValue: 1,
                freshEBP: 0x9030,
                freshDWORDAtESPPlus3C: 0xDEADBEEF);
        check("A fresh read remains independent when its address aliases an earlier captured DWORD",
            aliasingValues.ParentBranchAdmitted
            && aliasingValues.ParentPlan.ParentPlan.ComparisonAddress
                == aliasingValues.ReadAddress
            && aliasingValues.ParentPlan.ParentPlan.Capture.FreshDWORDAtEBPPlusC == 1
            && aliasingValues.ReadAddress == 0x903C
            && aliasingValues.ESIValue == 0xDEADBEEF
            && aliasingValues.ParentPlan.ParentPlan.EDIValue == 0x56E
            && !aliasingValues.AliasedReadsReconciled
            && aliasingValues.TestCommitted
            && aliasingValues.FlagsAtStop == ExpectedTestFlags(0xDEADBEEF));

        GameSocketParserDiagnosticCleanupStackPrefixPlan rejectedParent =
            Project(freshEBP: null, freshDWORDAtESPPlus3C: 0x12345678);
        GameSocketParserDiagnosticCleanupStackPrefixPlan otherBranch =
            Project(comparisonValue: 2, freshDWORDAtESPPlus3C: 0);
        check("The alternate selected target is not admitted as a cleanup prefix",
            !otherBranch.ParentBranchAdmitted
            && otherBranch.StopBeforeRawFileOffset == 0x00BD31F8
            && otherBranch.ExactBoundedCompletion
            && otherBranch.Instructions.IsEmpty
            && otherBranch.FreshDWORDAtESPPlus3CCaptureState
                == GameSocketParserDiagnosticCleanupStackPrefixCaptureState.SuppliedButUnused);
        check("Rejected parent branches do not attempt this suffix or consume its supplied capture",
            !rejectedParent.ParentBranchAdmitted
            && rejectedParent.Outcome
                == GameSocketParserDiagnosticCleanupStackPrefixOutcome.ParentBranchNotAdmitted
            && rejectedParent.FreshDWORDAtESPPlus3CCaptureState
                == GameSocketParserDiagnosticCleanupStackPrefixCaptureState.SuppliedButUnused
            && rejectedParent.Instructions.IsEmpty
            && rejectedParent.Reads.IsEmpty
            && !rejectedParent.StoreAttempted
            && !rejectedParent.FreshDWORDReadAttempted
            && rejectedParent.BranchTargetRawFileOffset is null
            && rejectedParent.StopBeforeRawFileOffset
                == rejectedParent.ParentPlan.StopBeforeRawFileOffset
            && rejectedParent.ExactBoundedCompletion
                == rejectedParent.ParentPlan.ExactBoundedCompletion
            && !rejectedParent.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupStackPrefixPlan original =
            Project(
                comparisonValue: 0,
                postCallESP: 0xFFFFFFF0,
                freshDWORDAtESPPlus3C: 0x80000000);
        GameSocketParserDiagnosticCleanupStackPrefixPlan replay =
            GameSocketParserDiagnosticCleanupStackPrefixProjection.Project(
                original.Capture);
        check("Immutable capture reprojection preserves semantics and wrapping stack addresses",
            original.Capture == replay.Capture
            && original.Outcome == replay.Outcome
            && original.ResultESP == replay.ResultESP
            && original.StoreAddress == 0x54
            && original.ReadAddress == 0x40
            && original.ESIValue == replay.ESIValue
            && original.FlagsAtStop == replay.FlagsAtStop
            && original.Instructions.SequenceEqual(replay.Instructions)
            && original.Reads.SequenceEqual(replay.Reads)
            && original.MemoryEffects.SequenceEqual(replay.MemoryEffects)
            && original.OrderedSemanticPrefixPreserved
            && replay.OrderedSemanticPrefixPreserved
            && original.IsCleanCompletion
            && replay.IsCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupStackPrefixPlan Project(
        uint? freshDWORDAtESPPlus3C = null,
        uint comparisonValue = 0,
        uint postCallESP = 0x8FEC,
        uint? freshEBP = 0x7000) =>
        GameSocketParserDiagnosticCleanupStackPrefixProjection.Project(
            new(
                CreateParentCapture(
                    comparisonValue,
                    postCallESP,
                    freshEBP),
                freshDWORDAtESPPlus3C));

    private static GameSocketParserDiagnosticDestinationBranchGateCapture CreateParentCapture(
        uint comparisonValue,
        uint postCallESP,
        uint? freshEBP)
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
        var prefixCapture = new GameSocketParserDiagnosticDestinationPrefixCapture(
            counterCapture,
            freshEBP,
            comparisonValue,
            FreshDWORDAtESPPlus14: 0xCAFEBABE);
        return new(prefixCapture);
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
