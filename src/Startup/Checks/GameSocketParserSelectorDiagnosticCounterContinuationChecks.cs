using Firefall.Reconstruction.Startup;

internal static class GameSocketParserSelectorDiagnosticCounterContinuationChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suffixBytes = ParseBytes(
            GameSocketParserSelectorDiagnosticCounterContinuationProjection
                .CoordinatorProvidedBytes);
        check("The supplied counter suffix ends at the recorded unconditional branch target",
            GameSocketParserSelectorDiagnosticCounterContinuationProjection.BytesIndependentlyVerified
                == false
            && suffixBytes.Length == 11
            && string.Join(" ", suffixBytes.Select(value => value.ToString("X2")))
                == "FF 86 EC 00 00 00 E9 DF 01 00 00"
            && GameSocketParserSelectorDiagnosticCounterContinuationProjection
                .IncrementRawFileOffset == 0x00BD3289
            && GameSocketParserSelectorDiagnosticCounterContinuationProjection
                .BranchRawFileOffset == 0x00BD328F
            && GameSocketParserSelectorDiagnosticCounterContinuationProjection
                .BranchTargetRawFileOffset == 0x00BD3473
            && GameSocketParserSelectorDiagnosticCounterContinuationProjection
                .BranchTargetCodeVirtualAddress == 0x00FD4073
            && GameSocketParserSelectorDiagnosticCounterContinuationProjection
                .CounterDisplacement == 0xEC
            && GameSocketParserSelectorDiagnosticCounterContinuationProjection
                .BranchDisplacement == 0x1DF
            && GameSocketParserSelectorDiagnosticCounterContinuationProjection
                .IncrementEncoding == "FF 86 EC 00 00 00"
            && GameSocketParserSelectorDiagnosticCounterContinuationProjection
                .BranchEncoding == "E9 DF 01 00 00"
            && GameSocketParserSelectorDiagnosticCounterContinuationProjection
                    .IncrementRawFileOffset + 6
                == GameSocketParserSelectorDiagnosticCounterContinuationProjection
                    .BranchRawFileOffset
            && GameSocketParserSelectorDiagnosticCounterContinuationProjection
                    .BranchRawFileOffset + 5
                    + GameSocketParserSelectorDiagnosticCounterContinuationProjection
                        .BranchDisplacement
                == GameSocketParserSelectorDiagnosticCounterContinuationProjection
                    .BranchTargetRawFileOffset);

        uint[] counterValues =
        {
            0,
            1,
            0x0000000E,
            0x0000000F,
            0x7FFFFFFE,
            0x7FFFFFFF,
            0x80000000,
            0xFFFFFFFE,
            uint.MaxValue
        };
        uint[] postCallESPs = { 0x00001000, 0xFFFFFFF0 };
        uint[] postCallESIs = { 0, 0x7000, 0xFFFFFF14, uint.MaxValue };
        bool cleanContinuationsExact = true;
        bool preservedClearCarry = false;
        bool preservedSetCarry = false;
        bool sawParityEven = false;
        bool sawParityOdd = false;
        bool sawAuxiliaryCarry = false;
        bool sawZero = false;
        bool sawSign = false;
        bool sawOverflow = false;
        foreach (byte selector in new byte[] { 0, 3 })
        {
            foreach (uint postCallESP in postCallESPs)
            {
                foreach (uint postCallESI in postCallESIs)
                {
                    foreach (uint counterValue in counterValues)
                    {
                        GameSocketParserSelectorDiagnosticCounterContinuationPlan plan =
                            Project(
                                selector,
                                normalReturnObserved: true,
                                postCallESP,
                                postCallESI,
                                counterValue);
                        GameSocketParserStoredByteSelectorGateFlags parentFlags =
                            ExpectedAddFlags32(postCallESP, 0x14);
                        uint parentResultESP = unchecked(postCallESP + 0x14);
                        uint expectedAddress = unchecked(postCallESI + 0xEC);
                        uint incrementedValue = unchecked(counterValue + 1);
                        GameSocketParserStoredByteSelectorGateFlags expectedFlags =
                            ExpectedIncrementFlags(counterValue, parentFlags);
                        cleanContinuationsExact &= plan.ParentContinuationAdmitted
                            && plan.ParentPreparationCompleted
                            && plan.ParentPlan.ParentPlan.ParentPlan.InheritedEBX == selector
                            && plan.ParentPlan.ParentPlan.ParentPlan.FirstJZState
                                == GameSocketParserStoredByteSelectorGateBranchState.NotTaken
                            && plan.ParentPlan.ParentPlan.ParentPlan.Selector2JEState
                                == GameSocketParserStoredByteSelectorGateBranchState.NotTaken
                            && plan.ParentPlan.ParentPlan.ParentPlan.Selector1JEState
                                == GameSocketParserStoredByteSelectorGateBranchState.NotTaken
                            && plan.ParentESP == 0x2FEC
                            && plan.FreshPostCallESP == postCallESP
                            && plan.ResultESP == parentResultESP
                            && plan.FreshPostCallESI == postCallESI
                            && plan.FreshESICaptureState
                                == GameSocketParserSelectorDiagnosticCounterContinuationCaptureState
                                    .Captured
                            && plan.CounterDWORDCaptureState
                                == GameSocketParserSelectorDiagnosticCounterContinuationCaptureState
                                    .Captured
                            && plan.CounterAddress == expectedAddress
                            && plan.CounterValueBefore == counterValue
                            && plan.CounterValueAfter == incrementedValue
                            && plan.Reads is [{
                                InstructionRawFileOffset: 0x00BD3289,
                                Address: var readAddress,
                                Value: var readValue,
                                Attempted: true,
                                Captured: true,
                                Committed: false
                            }]
                            && readAddress == expectedAddress
                            && readValue == counterValue
                            && plan.Instructions is [
                                {
                                    Kind: GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind
                                        .IncrementDWORDAt3289,
                                    InstructionRawFileOffset: 0x00BD3289,
                                    CodeVirtualAddress: 0x00FD3E89,
                                    Encoding: "FF 86 EC 00 00 00",
                                    StackPointerBefore: var incrementESPBefore,
                                    StackPointerAfter: var incrementESPAafter,
                                    MemoryAddress: var incrementAddress,
                                    ValueBefore: var incrementBefore,
                                    ValueAfter: var incrementAfter,
                                    Attempted: true,
                                    Reached: true,
                                    Committed: true,
                                    FlagsBefore: var incrementFlagsBefore,
                                    FlagsAfter: var incrementFlagsAfter
                                },
                                {
                                    Kind: GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind
                                        .JumpTo3473At328F,
                                    InstructionRawFileOffset: 0x00BD328F,
                                    CodeVirtualAddress: 0x00FD3E8F,
                                    Encoding: "E9 DF 01 00 00",
                                    StackPointerBefore: var branchESPBefore,
                                    StackPointerAfter: var branchESPAafter,
                                    TargetRawFileOffset: 0x00BD3473,
                                    TargetCodeVirtualAddress: 0x00FD4073,
                                    Attempted: true,
                                    Reached: true,
                                    Committed: true,
                                    FlagsBefore: var branchFlagsBefore,
                                    FlagsAfter: var branchFlagsAfter
                                }
                            ]
                            && incrementESPBefore == parentResultESP
                            && incrementESPAafter == parentResultESP
                            && branchESPBefore == parentResultESP
                            && branchESPAafter == parentResultESP
                            && incrementAddress == expectedAddress
                            && incrementBefore == counterValue
                            && incrementAfter == incrementedValue
                            && incrementFlagsBefore == parentFlags
                            && incrementFlagsAfter == expectedFlags
                            && branchFlagsBefore == expectedFlags
                            && branchFlagsAfter == expectedFlags
                            && plan.FlagsAtStop == expectedFlags
                            && plan.BranchAttempted
                            && plan.BranchReached
                            && plan.BranchCommitted
                            && !plan.BranchTargetInstructionExecuted
                            && plan.Outcome
                                == GameSocketParserSelectorDiagnosticCounterContinuationOutcome
                                    .BoundedBeforeBranchTarget
                            && plan.StopBeforeRawFileOffset == 0x00BD3473
                            && plan.StopBeforeCodeVirtualAddress == 0x00FD4073
                            && plan.ReachedBoundedEndpoint
                            && plan.ExactBoundedCompletion
                            && plan.IsCleanCompletion
                            && plan.OrderedMemoryPrefixPreserved
                            && plan.LogicalMemoryEffectsOnly
                            && !plan.OpaqueCallEffectsModeled
                            && !plan.NativeMemoryEffectsModeled
                            && !plan.CounterIdentitySemanticsModeled
                            && !plan.ParserExecutionModeled
                            && !plan.RuntimeSampleExecuted
                            && plan.MemoryEffects.Length
                                == plan.ParentPlan.MemoryEffects.Length + 2
                            && plan.MemoryEffects[^2] is {
                                Kind: GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffectKind
                                    .FreshCounterDWORDRead,
                                Address: var readEffectAddress,
                                Value: var readEffectValue,
                                Attempted: true,
                                Captured: true,
                                Committed: false
                            }
                            && readEffectAddress == expectedAddress
                            && readEffectValue == counterValue
                            && plan.MemoryEffects[^1] is {
                                Kind: GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffectKind
                                    .CounterDWORDIncrement,
                                Address: var writeAddress,
                                Value: var writeValue,
                                Attempted: true,
                                Captured: true,
                                Committed: true
                            }
                            && writeAddress == expectedAddress
                            && writeValue == incrementedValue
                            && plan.UnusedCaptures.IsEmpty;
                        preservedClearCarry |= parentFlags.Carry == false
                            && expectedFlags.Carry == false;
                        preservedSetCarry |= parentFlags.Carry == true
                            && expectedFlags.Carry == true;
                        sawParityEven |= expectedFlags.Parity == true;
                        sawParityOdd |= expectedFlags.Parity == false;
                        sawAuxiliaryCarry |= expectedFlags.AuxiliaryCarry == true;
                        sawZero |= expectedFlags.Zero == true;
                        sawSign |= expectedFlags.Sign == true;
                        sawOverflow |= expectedFlags.Overflow == true;
                    }
                }
            }
        }

        check("Selectors 0/3 increment the fresh DWORD and preserve ESP, CF, and exact INC flags",
            cleanContinuationsExact
            && preservedClearCarry
            && preservedSetCarry
            && sawParityEven
            && sawParityOdd
            && sawAuxiliaryCarry
            && sawZero
            && sawSign
            && sawOverflow
            && ExpectedIncrementFlags(0x7FFFFFFF, ExpectedAddFlags32(0, 0x14)).Overflow == true
            && ExpectedIncrementFlags(uint.MaxValue, ExpectedAddFlags32(0, 0x14)).Carry == false
            && ExpectedIncrementFlags(0xFFFFFFFF, ExpectedAddFlags32(0xFFFFFFF0, 0x14))
                .Carry == true);

        const uint aliasPostResizeESP = 0x3000;
        const uint aliasCounterAddress = 0x2FFC;
        const uint aliasPostCallESI = aliasCounterAddress - 0xEC;
        GameSocketParserSelectorDiagnosticCounterContinuationPlan aliasedCounter =
            Project(
                selector: 3,
                normalReturnObserved: true,
                postCallESP: 0x9000,
                postCallESI: aliasPostCallESI,
                counterValue: 0xDEADBEEF,
                postResizeESP: aliasPostResizeESP);
        int parentEffectCount = aliasedCounter.ParentPlan.MemoryEffects.Length;
        check("The fresh post-call counter read remains independent of an aliased earlier push",
            aliasedCounter.ParentPlan.ParentPlan.Stores[0].Address == aliasCounterAddress
            && aliasedCounter.ParentPlan.ParentPlan.Stores[0].Value
                == GameSocketParserSelectorDiagnosticPreparationProjection.FirstOpaqueImmediate
            && aliasedCounter.ParentPlan.ParentPlan.FreshESIAt327B == 0x7000
            && aliasedCounter.FreshPostCallESI == 0x2F10
            && aliasedCounter.CounterAddress == aliasCounterAddress
            && aliasedCounter.Reads is [{ Address: aliasCounterAddress, Value: 0xDEADBEEF }]
            && aliasedCounter.MemoryEffects[parentEffectCount] is {
                Kind: GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffectKind
                    .FreshCounterDWORDRead,
                Address: aliasCounterAddress,
                Value: 0xDEADBEEF,
                Committed: false
            }
            && aliasedCounter.MemoryEffects[parentEffectCount + 1] is {
                Kind: GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffectKind
                    .CounterDWORDIncrement,
                Address: aliasCounterAddress,
                Value: 0xDEADBEF0,
                Committed: true
            }
            && aliasedCounter.IsCleanCompletion);

        GameSocketParserSelectorDiagnosticCounterContinuationPlan missingESI =
            Project(
                selector: 0,
                normalReturnObserved: true,
                postCallESP: 0x9000,
                postCallESI: null,
                counterValue: null);
        GameSocketParserSelectorDiagnosticCounterContinuationPlan missingESIWithUnusedDWORD =
            Project(
                selector: 3,
                normalReturnObserved: true,
                postCallESP: 0x9000,
                postCallESI: null,
                counterValue: 0);
        GameSocketParserSelectorDiagnosticCounterContinuationPlan missingDWORD =
            Project(
                selector: 0,
                normalReturnObserved: true,
                postCallESP: 0x9000,
                postCallESI: 0x7000,
                counterValue: null);
        check("Missing ESI invents no address or write; missing DWORD reads but cannot increment",
            missingESI.ParentContinuationAdmitted
            && missingESI.FreshESICaptureState
                == GameSocketParserSelectorDiagnosticCounterContinuationCaptureState.Missing
            && missingESI.CounterDWORDCaptureState
                == GameSocketParserSelectorDiagnosticCounterContinuationCaptureState.NotReached
            && missingESI.CounterAddress is null
            && missingESI.Reads.IsEmpty
            && missingESI.MemoryEffects.Length
                == missingESI.ParentPlan.MemoryEffects.Length
            && missingESI.Instructions is [{
                Kind: GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind
                    .IncrementDWORDAt3289,
                MemoryAddress: null,
                ValueBefore: null,
                ValueAfter: null,
                Attempted: true,
                Reached: true,
                Committed: false
            }]
            && missingESI.StopBeforeRawFileOffset == 0x00BD3289
            && missingESI.FlagsAtStop == missingESI.ParentPlan.FlagsAtStop
            && !missingESI.BranchAttempted
            && !missingESI.IsCleanCompletion
            && missingESIWithUnusedDWORD.CounterDWORDCaptureState
                == GameSocketParserSelectorDiagnosticCounterContinuationCaptureState
                    .SuppliedButUnused
            && missingESIWithUnusedDWORD.UnusedCaptures is [
                GameSocketParserSelectorDiagnosticCounterContinuationCaptureKind
                    .FreshCounterDWORD]
            && missingDWORD.ParentContinuationAdmitted
            && missingDWORD.FreshESICaptureState
                == GameSocketParserSelectorDiagnosticCounterContinuationCaptureState.Captured
            && missingDWORD.CounterDWORDCaptureState
                == GameSocketParserSelectorDiagnosticCounterContinuationCaptureState.Missing
            && missingDWORD.CounterAddress == 0x70EC
            && missingDWORD.Reads is [{
                Address: 0x70EC,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }]
            && missingDWORD.MemoryEffects[^1] is {
                Kind: GameSocketParserSelectorDiagnosticCounterContinuationMemoryEffectKind
                    .FreshCounterDWORDRead,
                Address: 0x70EC,
                Attempted: true,
                Captured: false,
                Committed: false
            }
            && missingDWORD.Instructions is [{
                Kind: GameSocketParserSelectorDiagnosticCounterContinuationInstructionKind
                    .IncrementDWORDAt3289,
                MemoryAddress: 0x70EC,
                ValueBefore: null,
                ValueAfter: null,
                Attempted: true,
                Reached: true,
                Committed: false
            }]
            && missingDWORD.CounterValueAfter is null
            && missingDWORD.StopBeforeRawFileOffset == 0x00BD3289
            && !missingDWORD.BranchAttempted
            && !missingDWORD.IsCleanCompletion);

        GameSocketParserSelectorDiagnosticCounterContinuationPlan pending =
            Project(0, normalReturnObserved: null, postCallESP: null, postCallESI: 0x7000,
                counterValue: 4);
        GameSocketParserSelectorDiagnosticCounterContinuationPlan nonNormal =
            Project(3, normalReturnObserved: false, postCallESP: null, postCallESI: 0x7000,
                counterValue: 4);
        GameSocketParserSelectorDiagnosticCounterContinuationPlan missingESP =
            Project(0, normalReturnObserved: true, postCallESP: null, postCallESI: 0x7000,
                counterValue: 4);
        GameSocketParserSelectorDiagnosticCounterContinuationPlan rejectedPreparation =
            Project(0, normalReturnObserved: true, postCallESP: 0x9000, postCallESI: 0x7000,
                counterValue: 4, freshByte: 0);
        GameSocketParserSelectorDiagnosticCounterContinuationPlan missingPreparationESI =
            Project(3, normalReturnObserved: true, postCallESP: 0x9000, postCallESI: 0x7000,
                counterValue: 4, preparationESI: null);
        GameSocketParserSelectorDiagnosticCounterContinuationPlan missingPreparationDWORD =
            Project(0, normalReturnObserved: true, postCallESP: 0x9000, postCallESI: 0x7000,
                counterValue: 4, receiverValue: null);
        check("Pending, non-normal, missing-ESP, and rejected ancestors retain their own bounds",
            !pending.ParentContinuationAdmitted
            && pending.ParentPlan.Outcome
                == GameSocketParserSelectorDiagnosticReturnCleanupOutcome.CallPending
            && pending.Outcome
                == GameSocketParserSelectorDiagnosticCounterContinuationOutcome
                    .ParentContinuationNotAdmitted
            && pending.Instructions.IsEmpty
            && pending.Reads.IsEmpty
            && pending.StopBeforeRawFileOffset == 0x00BD3281
            && pending.FreshESICaptureState
                == GameSocketParserSelectorDiagnosticCounterContinuationCaptureState
                    .SuppliedButUnused
            && pending.CounterDWORDCaptureState
                == GameSocketParserSelectorDiagnosticCounterContinuationCaptureState
                    .SuppliedButUnused
            && pending.UnusedCaptures.Length == 2
            && !nonNormal.ParentContinuationAdmitted
            && nonNormal.ParentPlan.Outcome
                == GameSocketParserSelectorDiagnosticReturnCleanupOutcome.NonNormalReturn
            && nonNormal.Instructions.IsEmpty
            && nonNormal.StopBeforeRawFileOffset == 0x00BD3281
            && nonNormal.UnusedCaptures.Length == 2
            && !missingESP.ParentContinuationAdmitted
            && missingESP.ParentPlan.Outcome
                == GameSocketParserSelectorDiagnosticReturnCleanupOutcome.MissingFreshPostCallESP
            && missingESP.Instructions.IsEmpty
            && missingESP.StopBeforeRawFileOffset == 0x00BD3286
            && missingESP.UnusedCaptures.Length == 2
            && !rejectedPreparation.ParentContinuationAdmitted
            && !rejectedPreparation.ParentPlan.ParentHandoffAdmitted
            && rejectedPreparation.Instructions.IsEmpty
            && rejectedPreparation.StopBeforeRawFileOffset
                == rejectedPreparation.ParentPlan.StopBeforeRawFileOffset
            && rejectedPreparation.UnusedCaptures.Length == 2
            && !missingPreparationESI.ParentContinuationAdmitted
            && !missingPreparationESI.ParentPlan.ParentHandoffAdmitted
            && missingPreparationESI.ParentPlan.ParentPlan.ParentHandoffAdmitted
            && missingPreparationESI.ParentPlan.ParentPlan.Outcome
                == GameSocketParserSelectorDiagnosticPreparationOutcome.MissingFreshESIAt327B
            && missingPreparationESI.Instructions.IsEmpty
            && missingPreparationESI.StopBeforeRawFileOffset
                == missingPreparationESI.ParentPlan.StopBeforeRawFileOffset
            && missingPreparationESI.UnusedCaptures.Length == 2
            && !missingPreparationDWORD.ParentContinuationAdmitted
            && !missingPreparationDWORD.ParentPlan.ParentHandoffAdmitted
            && missingPreparationDWORD.ParentPlan.ParentPlan.Outcome
                == GameSocketParserSelectorDiagnosticPreparationOutcome
                    .MissingFreshReceiverFieldDWORD
            && missingPreparationDWORD.Instructions.IsEmpty
            && missingPreparationDWORD.StopBeforeRawFileOffset
                == missingPreparationDWORD.ParentPlan.StopBeforeRawFileOffset
            && missingPreparationDWORD.UnusedCaptures.Length == 2);

        GameSocketParserSelectorDiagnosticCounterContinuationPlan clean =
            Project(3, normalReturnObserved: true, postCallESP: 0xBEEF,
                postCallESI: 0x1234, counterValue: 0x12345678);
        GameSocketParserSelectorDiagnosticCounterContinuationPlan reprojected =
            GameSocketParserSelectorDiagnosticCounterContinuationProjection.Project(
                clean.Capture);
        check("Immutable capture reprojection preserves the counter and branch effects",
            reprojected.Outcome == clean.Outcome
            && reprojected.ParentContinuationAdmitted == clean.ParentContinuationAdmitted
            && reprojected.NormalReturnObserved == clean.NormalReturnObserved
            && reprojected.ResultNormalReturnObserved == clean.ResultNormalReturnObserved
            && reprojected.ParentESP == clean.ParentESP
            && reprojected.FreshPostCallESP == clean.FreshPostCallESP
            && reprojected.ResultESP == clean.ResultESP
            && reprojected.FreshPostCallESI == clean.FreshPostCallESI
            && reprojected.FreshESICaptureState == clean.FreshESICaptureState
            && reprojected.CounterDWORDCaptureState == clean.CounterDWORDCaptureState
            && reprojected.CounterAddress == clean.CounterAddress
            && reprojected.CounterValueBefore == clean.CounterValueBefore
            && reprojected.CounterValueAfter == clean.CounterValueAfter
            && reprojected.FlagsAtStop == clean.FlagsAtStop
            && reprojected.StopBeforeRawFileOffset == clean.StopBeforeRawFileOffset
            && reprojected.Instructions.SequenceEqual(clean.Instructions)
            && reprojected.Reads.SequenceEqual(clean.Reads)
            && reprojected.MemoryEffects.SequenceEqual(clean.MemoryEffects)
            && reprojected.UnusedCaptures.SequenceEqual(clean.UnusedCaptures)
            && reprojected.OrderedMemoryPrefixPreserved);
    }

    private static GameSocketParserSelectorDiagnosticCounterContinuationPlan Project(
        byte selector,
        bool? normalReturnObserved,
        uint? postCallESP,
        uint? postCallESI,
        uint? counterValue,
        uint callerCount = 100,
        byte freshByte = 2,
        uint? preparationESI = 0x7000,
        uint? receiverValue = 0xCAFEBABE,
        uint postResizeESP = 0x3000)
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
                                        postResizeESP,
                                        0x5000,
                                        0x6000),
                                    (byte)(selector << 6)),
                                FreshSecondHeaderByte: freshByte),
                            0x7000,
                            callerCount),
                        FreshStackDwordAtESPPlus14: 0x1000,
                        FreshStackByteAtESPPlus13: 1)),
                preparationESI,
                receiverValue);
        return GameSocketParserSelectorDiagnosticCounterContinuationProjection.Project(
            new(
                new(preparationCapture, normalReturnObserved, postCallESP),
                postCallESI,
                counterValue));
    }

    private static GameSocketParserStoredByteSelectorGateFlags ExpectedAddFlags32(
        uint left,
        uint right)
    {
        uint result = unchecked(left + right);
        return new(
            Carry: (ulong)left + right > uint.MaxValue,
            Parity: (System.Numerics.BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((~(left ^ right)) & (left ^ result) & 0x80000000) != 0);
    }

    private static GameSocketParserStoredByteSelectorGateFlags ExpectedIncrementFlags(
        uint oldValue,
        GameSocketParserStoredByteSelectorGateFlags flagsBefore)
    {
        uint result = unchecked(oldValue + 1);
        return new(
            Carry: flagsBefore.Carry,
            Parity: (System.Numerics.BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            AuxiliaryCarry: (oldValue & 0x0F) == 0x0F,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: oldValue == 0x7FFFFFFF);
    }

    private static byte[] ParseBytes(string hex) =>
        hex.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
