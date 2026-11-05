using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticDestinationPrefixChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticDestinationPrefixProjection.CoordinatorProvidedBytes);
        check("The supplied diagnostic prefix ends immediately before JAE at 3480",
            !GameSocketParserDiagnosticDestinationPrefixProjection.BytesIndependentlyVerified
            && suppliedBytes.Length == 21
            && string.Join(" ", suppliedBytes.Select(value => value.ToString("X2")))
                == "83 7D 0C 02 BF 6E 05 00 00 8B 54 24 14 0F 83 72 FD FF FF EB 25"
            && GameSocketParserDiagnosticDestinationPrefixProjection.CompareRawFileOffset
                == 0x00BD3473
            && GameSocketParserDiagnosticDestinationPrefixProjection.MoveEDIRawFileOffset
                == 0x00BD3477
            && GameSocketParserDiagnosticDestinationPrefixProjection.MoveEDXRawFileOffset
                == 0x00BD347C
            && GameSocketParserDiagnosticDestinationPrefixProjection.JaeRawFileOffset
                == 0x00BD3480
            && GameSocketParserDiagnosticDestinationPrefixProjection.CompareEncoding
                == "83 7D 0C 02"
            && GameSocketParserDiagnosticDestinationPrefixProjection.MoveEDIEncoding
                == "BF 6E 05 00 00"
            && GameSocketParserDiagnosticDestinationPrefixProjection.MoveEDXEncoding
                == "8B 54 24 14"
            && GameSocketParserDiagnosticDestinationPrefixProjection.EBPDisplacement == 0x0C
            && GameSocketParserDiagnosticDestinationPrefixProjection.ESPDisplacement == 0x14
            && GameSocketParserDiagnosticDestinationPrefixProjection.CompareImmediate == 2
            && GameSocketParserDiagnosticDestinationPrefixProjection.EDIImmediate == 0x56E);

        uint[] comparisonValues =
        {
            0,
            1,
            2,
            3,
            0x7FFFFFFF,
            0x80000000,
            uint.MaxValue
        };
        bool sawCarrySet = false;
        bool sawCarryClear = false;
        bool sawParitySet = false;
        bool sawParityClear = false;
        bool sawAuxiliaryCarrySet = false;
        bool sawAuxiliaryCarryClear = false;
        bool sawZeroSet = false;
        bool sawZeroClear = false;
        bool sawSignSet = false;
        bool sawSignClear = false;
        bool sawOverflowSet = false;
        bool sawOverflowClear = false;
        bool comparisonsExact = true;
        foreach (uint value in comparisonValues)
        {
            GameSocketParserDiagnosticDestinationPrefixPlan plan =
                Project(value, postCallESP: 0x9000, freshEBP: 0x7000,
                    comparisonDWORD: value, stackDWORD: 0xAABBCCDD);
            GameSocketParserStoredByteSelectorGateFlags expected =
                ExpectedCompareFlags(value, 2);
            comparisonsExact &= plan.ParentContinuationAdmitted
                && plan.FreshPostCallEBP == 0x7000
                && plan.ComparisonAddress == 0x700C
                && plan.StackDWORDAddress == unchecked(plan.ResultESP!.Value + 0x14)
                && plan.EDIValue == 0x56E
                && plan.EDXValue == 0xAABBCCDD
                && plan.FlagsAtStop == expected
                && plan.Instructions is [
                    {
                        Kind: GameSocketParserDiagnosticDestinationPrefixInstructionKind
                            .CompareDWORDAtEBPPlusCWith2At3473,
                        InstructionRawFileOffset: 0x00BD3473,
                        CodeVirtualAddress: var cmpVA,
                        MemoryAddress: 0x700C,
                        MemoryValue: var cmpValue,
                        Committed: true,
                        FlagsBefore: var cmpFlagsBefore,
                        FlagsAfter: var cmpFlagsAfter
                    },
                    {
                        Kind: GameSocketParserDiagnosticDestinationPrefixInstructionKind
                            .MoveEDIImmediateAt3477,
                        InstructionRawFileOffset: 0x00BD3477,
                        DestinationRegister: "EDI",
                        DestinationValue: 0x56E,
                        Committed: true,
                        FlagsBefore: var ediFlagsBefore,
                        FlagsAfter: var ediFlagsAfter
                    },
                    {
                        Kind: GameSocketParserDiagnosticDestinationPrefixInstructionKind
                            .MoveEDXFromESPPlus14At347C,
                        InstructionRawFileOffset: 0x00BD347C,
                        DestinationRegister: "EDX",
                        DestinationValue: 0xAABBCCDD,
                        Committed: true,
                        FlagsBefore: var edxFlagsBefore,
                        FlagsAfter: var edxFlagsAfter
                    }
                ]
                && cmpVA == 0x00FD4073
                && cmpValue == value
                && cmpFlagsBefore == plan.ParentPlan.FlagsAtStop
                && cmpFlagsAfter == expected
                && ediFlagsBefore == expected
                && ediFlagsAfter == expected
                && edxFlagsBefore == expected
                && edxFlagsAfter == expected
                && plan.StopBeforeRawFileOffset == 0x00BD3480
                && plan.ExactBoundedCompletion
                && plan.IsCleanCompletion
                && !plan.JaeAttempted
                && !plan.JaeReached
                && !plan.JaeCommitted
                && !plan.JaeDestinationModeled
                && !plan.BranchTargetInstructionExecuted
                && plan.OrderedMemoryPrefixPreserved
                && plan.LogicalMemoryEffectsOnly
                && !plan.EBPIdentitySemanticsModeled
                && !plan.AliasedReadsReconciled
                && !plan.NativeMemoryEffectsModeled
                && !plan.ParserExecutionModeled
                && !plan.RuntimeSampleExecuted;
            sawCarrySet |= expected.Carry == true;
            sawCarryClear |= expected.Carry == false;
            sawParitySet |= expected.Parity == true;
            sawParityClear |= expected.Parity == false;
            sawAuxiliaryCarrySet |= expected.AuxiliaryCarry == true;
            sawAuxiliaryCarryClear |= expected.AuxiliaryCarry == false;
            sawZeroSet |= expected.Zero == true;
            sawZeroClear |= expected.Zero == false;
            sawSignSet |= expected.Sign == true;
            sawSignClear |= expected.Sign == false;
            sawOverflowSet |= expected.Overflow == true;
            sawOverflowClear |= expected.Overflow == false;
        }

        check("CMP replaces all six flags and MOV instructions preserve them",
            comparisonsExact
            && sawCarrySet && sawCarryClear
            && sawParitySet && sawParityClear
            && sawAuxiliaryCarrySet && sawAuxiliaryCarryClear
            && sawZeroSet && sawZeroClear
            && sawSignSet && sawSignClear
            && sawOverflowSet && sawOverflowClear
            && ExpectedCompareFlags(0x80000000, 2).Overflow == true
            && ExpectedCompareFlags(uint.MaxValue, 2).Sign == true);

        const uint aliasAddress = 0x2FFC;
        GameSocketParserDiagnosticDestinationPrefixPlan aliasedReads =
            Project(
                comparisonValue: 4,
                postCallESP: 0x2FD4,
                freshEBP: aliasAddress - 0x0C,
                comparisonDWORD: 4,
                stackDWORD: 0xDEADBEEF);
        int parentEffectCount = aliasedReads.ParentPlan.MemoryEffects.Length;
        check("Fresh comparison and stack reads stay independent when both alias a prior store",
            aliasedReads.ParentPlan.ParentPlan.ParentPlan.Stores[0].Address == aliasAddress
            && aliasedReads.ComparisonAddress == aliasAddress
            && aliasedReads.StackDWORDAddress == aliasAddress
            && aliasedReads.Reads is [
                { InstructionRawFileOffset: 0x00BD3473, Address: aliasAddress, Value: 4 },
                {
                    InstructionRawFileOffset: 0x00BD347C,
                    Address: aliasAddress,
                    Value: 0xDEADBEEF
                }
            ]
            && aliasedReads.MemoryEffects[parentEffectCount] is {
                Kind: GameSocketParserDiagnosticDestinationPrefixMemoryEffectKind
                    .FreshDWORDAtEBPPlusCRead,
                Address: aliasAddress,
                Value: 4,
                Committed: false
            }
            && aliasedReads.MemoryEffects[parentEffectCount + 1] is {
                Kind: GameSocketParserDiagnosticDestinationPrefixMemoryEffectKind
                    .FreshDWORDAtESPPlus14Read,
                Address: aliasAddress,
                Value: 0xDEADBEEF,
                Committed: false
            }
            && aliasedReads.IsCleanCompletion);

        GameSocketParserDiagnosticDestinationPrefixPlan wrappedAddresses =
            Project(
                comparisonValue: 0,
                postCallESP: 0xFFFFFFDC,
                freshEBP: 0xFFFFFFF4,
                comparisonDWORD: 0,
                stackDWORD: 0);
        check("Zero and wrapped EBP/ESP displacements remain valid captured addresses",
            wrappedAddresses.ResultESP == 0xFFFFFFF0
            && wrappedAddresses.ComparisonAddress == 0
            && wrappedAddresses.StackDWORDAddress == 4
            && wrappedAddresses.Reads is [
                { Address: 0, Value: 0, Captured: true },
                { Address: 4, Value: 0, Captured: true }
            ]
            && wrappedAddresses.IsCleanCompletion);

        GameSocketParserDiagnosticDestinationPrefixPlan missingEBP =
            Project(4, postCallESP: 0x9000, freshEBP: null,
                comparisonDWORD: 0, stackDWORD: 1);
        GameSocketParserDiagnosticDestinationPrefixPlan missingCompareDWORD =
            Project(4, postCallESP: 0x9000, freshEBP: 0x7000,
                comparisonDWORD: null, stackDWORD: 1);
        GameSocketParserDiagnosticDestinationPrefixPlan missingStackDWORD =
            Project(4, postCallESP: 0x9000, freshEBP: 0x7000,
                comparisonDWORD: 3, stackDWORD: null);
        check("Missing captures stop at their first use without inventing registers or flags",
            missingEBP.ParentContinuationAdmitted
            && missingEBP.FreshPostCallEBPCaptureState
                == GameSocketParserDiagnosticDestinationPrefixCaptureState.Missing
            && missingEBP.Reads.IsEmpty
            && missingEBP.ComparisonAddress is null
            && missingEBP.EDIValue is null
            && missingEBP.EDXValue is null
            && missingEBP.FlagsAtStop == missingEBP.ParentPlan.FlagsAtStop
            && missingEBP.Instructions is [{
                Kind: GameSocketParserDiagnosticDestinationPrefixInstructionKind
                    .CompareDWORDAtEBPPlusCWith2At3473,
                MemoryAddress: null,
                Attempted: true,
                Reached: true,
                Committed: false
            }]
            && missingEBP.StopBeforeRawFileOffset == 0x00BD3473
            && missingEBP.DWORDAtEBPPlusCCaptureState
                == GameSocketParserDiagnosticDestinationPrefixCaptureState.SuppliedButUnused
            && missingEBP.DWORDAtESPPlus14CaptureState
                == GameSocketParserDiagnosticDestinationPrefixCaptureState.SuppliedButUnused
            && missingCompareDWORD.ParentContinuationAdmitted
            && missingCompareDWORD.Reads is [{
                InstructionRawFileOffset: 0x00BD3473,
                Address: 0x700C,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }]
            && missingCompareDWORD.EDIValue is null
            && missingCompareDWORD.EDXValue is null
            && missingCompareDWORD.FlagsAtStop == missingCompareDWORD.ParentPlan.FlagsAtStop
            && missingCompareDWORD.StopBeforeRawFileOffset == 0x00BD3473
            && missingCompareDWORD.DWORDAtESPPlus14CaptureState
                == GameSocketParserDiagnosticDestinationPrefixCaptureState.SuppliedButUnused
            && missingStackDWORD.ParentContinuationAdmitted
            && missingStackDWORD.EDIValue == 0x56E
            && missingStackDWORD.EDXValue is null
            && missingStackDWORD.FlagsAtStop == ExpectedCompareFlags(3, 2)
            && missingStackDWORD.Reads is [
                { InstructionRawFileOffset: 0x00BD3473, Captured: true },
                {
                    InstructionRawFileOffset: 0x00BD347C,
                    Address: var missingStackAddress,
                    Value: null,
                    Attempted: true,
                    Captured: false,
                    Committed: false
                }
            ]
            && missingStackAddress == missingStackDWORD.StackDWORDAddress
            && missingStackDWORD.Instructions is [
                { Kind: GameSocketParserDiagnosticDestinationPrefixInstructionKind
                    .CompareDWORDAtEBPPlusCWith2At3473, Committed: true },
                { Kind: GameSocketParserDiagnosticDestinationPrefixInstructionKind
                    .MoveEDIImmediateAt3477, Committed: true },
                {
                    Kind: GameSocketParserDiagnosticDestinationPrefixInstructionKind
                        .MoveEDXFromESPPlus14At347C,
                    DestinationRegister: "EDX",
                    DestinationValue: null,
                    Committed: false,
                    FlagsAfter: var missingStackFlags
                }
            ]
            && missingStackFlags == ExpectedCompareFlags(3, 2)
            && missingStackDWORD.StopBeforeRawFileOffset == 0x00BD347C
            && !missingEBP.IsCleanCompletion
            && !missingCompareDWORD.IsCleanCompletion
            && !missingStackDWORD.IsCleanCompletion);

        GameSocketParserDiagnosticDestinationPrefixPlan rejectedParent =
            Project(4, normalReturnObserved: true, postCallESP: 0x9000,
                freshESI: null, freshEBP: 0x7000, comparisonDWORD: 2, stackDWORD: 7);
        check("A missing parent ESI keeps the parent's actual bound and leaves suffix captures unused",
            !rejectedParent.ParentContinuationAdmitted
            && rejectedParent.ParentPlan.Outcome
                == GameSocketParserSelectorDiagnosticCounterContinuationOutcome
                    .MissingFreshPostCallESI
            && rejectedParent.Instructions.IsEmpty
            && rejectedParent.Reads.IsEmpty
            && rejectedParent.StopBeforeRawFileOffset
                == rejectedParent.ParentPlan.StopBeforeRawFileOffset
            && rejectedParent.StopBeforeRawFileOffset == 0x00BD3289
            && rejectedParent.UnusedCaptures.Length == 3
            && rejectedParent.FreshPostCallEBPCaptureState
                == GameSocketParserDiagnosticDestinationPrefixCaptureState.SuppliedButUnused
            && rejectedParent.DWORDAtEBPPlusCCaptureState
                == GameSocketParserDiagnosticDestinationPrefixCaptureState.SuppliedButUnused
            && rejectedParent.DWORDAtESPPlus14CaptureState
                == GameSocketParserDiagnosticDestinationPrefixCaptureState.SuppliedButUnused
            && !rejectedParent.IsCleanCompletion);

        GameSocketParserDiagnosticDestinationPrefixPlan clean =
            Project(3, postCallESP: 0xBEEF, freshEBP: 0x1234,
                comparisonDWORD: 2, stackDWORD: 0x12345678, selector: 3);
        GameSocketParserDiagnosticDestinationPrefixPlan reprojected =
            GameSocketParserDiagnosticDestinationPrefixProjection.Project(clean.Capture);
        check("Immutable capture reprojection preserves semantic register, flag, and read effects",
            reprojected.Outcome == clean.Outcome
            && reprojected.ParentContinuationAdmitted == clean.ParentContinuationAdmitted
            && reprojected.NormalReturnObserved == clean.NormalReturnObserved
            && reprojected.ResultNormalReturnObserved == clean.ResultNormalReturnObserved
            && reprojected.ParentESP == clean.ParentESP
            && reprojected.FreshPostCallESP == clean.FreshPostCallESP
            && reprojected.ResultESP == clean.ResultESP
            && reprojected.FreshPostCallEBP == clean.FreshPostCallEBP
            && reprojected.FreshPostCallEBPCaptureState == clean.FreshPostCallEBPCaptureState
            && reprojected.DWORDAtEBPPlusCCaptureState == clean.DWORDAtEBPPlusCCaptureState
            && reprojected.DWORDAtESPPlus14CaptureState == clean.DWORDAtESPPlus14CaptureState
            && reprojected.ComparisonAddress == clean.ComparisonAddress
            && reprojected.StackDWORDAddress == clean.StackDWORDAddress
            && reprojected.EDIValue == clean.EDIValue
            && reprojected.EDXValue == clean.EDXValue
            && reprojected.FlagsAtStop == clean.FlagsAtStop
            && reprojected.StopBeforeRawFileOffset == clean.StopBeforeRawFileOffset
            && reprojected.Instructions.SequenceEqual(clean.Instructions)
            && reprojected.Reads.SequenceEqual(clean.Reads)
            && reprojected.MemoryEffects.SequenceEqual(clean.MemoryEffects)
            && reprojected.UnusedCaptures.SequenceEqual(clean.UnusedCaptures)
            && reprojected.IsCleanCompletion);
    }

    private static GameSocketParserDiagnosticDestinationPrefixPlan Project(
        uint comparisonValue,
        bool? normalReturnObserved = true,
        uint? postCallESP = 0x9000,
        uint? freshESI = 0x7000,
        uint? counterValue = 4,
        uint? freshEBP = 0x7000,
        uint? comparisonDWORD = 2,
        uint? stackDWORD = 0xCAFEBABE,
        uint callerCount = 100,
        byte freshByte = 2,
        uint? preparationESI = 0x7000,
        uint? receiverValue = 0xCAFEBABE,
        uint postResizeESP = 0x3000,
        byte selector = 0)
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
        GameSocketParserSelectorDiagnosticCounterContinuationCapture parentCapture =
            new(
                new(preparationCapture, normalReturnObserved, postCallESP),
                freshESI,
                counterValue);
        return GameSocketParserDiagnosticDestinationPrefixProjection.Project(
            new(parentCapture, freshEBP, comparisonDWORD, stackDWORD));
    }

    private static GameSocketParserStoredByteSelectorGateFlags ExpectedCompareFlags(
        uint left,
        uint right)
    {
        uint result = unchecked(left - right);
        return new(
            Carry: left < right,
            Parity: (System.Numerics.BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static byte[] ParseBytes(string hex) =>
        hex.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
