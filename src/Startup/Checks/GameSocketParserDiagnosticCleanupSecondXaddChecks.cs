using Firefall.Reconstruction.Startup;
using System.Numerics;

internal static class GameSocketParserDiagnosticCleanupSecondXaddChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupSecondXaddProjection.CoordinatorProvidedBytes);
        check("The supplied bytes anchor the second LOCK XADD and stop before DEC EBX",
            !GameSocketParserDiagnosticCleanupSecondXaddProjection.BytesIndependentlyVerified
            && suppliedBytes.Length == 24
            && GameSocketParserDiagnosticCleanupSecondXaddProjection.LockedXaddRawFileOffset
                == 0x00BD34D4
            && GameSocketParserDiagnosticCleanupSecondXaddProjection.LockedXaddLength == 4
            && GameSocketParserDiagnosticCleanupSecondXaddProjection.ExclusiveStopRawFileOffset
                == 0x00BD34D8
            && GameSocketParserDiagnosticCleanupSecondXaddProjection.FollowingDecRawFileOffset
                == 0x00BD34D8
            && GameSocketParserDiagnosticCleanupSecondXaddProjection.LockedXaddEncoding
                == "F0 0F C1 18"
            && GameSocketParserDiagnosticCleanupSecondXaddProjection.FollowingDecEncoding == "4B"
            && GameSocketParserDiagnosticCleanupSecondXaddProjection.FollowingDecRawFileOffset
                == GameSocketParserDiagnosticCleanupSecondXaddProjection
                    .LockedXaddRawFileOffset
                    + GameSocketParserDiagnosticCleanupSecondXaddProjection.LockedXaddLength
            && suppliedBytes.Take(3).SequenceEqual(ParseBytes("8D 46 08"))
            && suppliedBytes.Skip(3).Take(4).SequenceEqual(ParseBytes("F0 0F C1 18"))
            && suppliedBytes.Skip(7).Take(1).SequenceEqual(ParseBytes("4B")));

        (uint Memory, uint Operand)[] cases =
        {
            (0, 0),
            (uint.MaxValue, 1),
            (0x7FFFFFFF, 1),
            (0x80000000, 0x80000000),
            (0x10, 0x20),
            (1, 2),
            (0, 0x80000000),
            (uint.MaxValue, uint.MaxValue)
        };
        bool allValuesExact = true;
        bool[] sawFlagValues = new bool[12];
        foreach ((uint oldValue, uint freshEBX) in cases)
        {
            GameSocketParserDiagnosticCleanupSecondXaddPlan plan =
                Project(freshEBX, oldValue);
            uint result = unchecked(oldValue + freshEBX);
            GameSocketParserStoredByteSelectorGateFlags expectedFlags =
                IndependentFlagsForAdd(oldValue, freshEBX);
            allValuesExact &= plan.ParentAdmitted
                && plan.ParentExactCompletion
                && plan.ParentExactBoundedCompletion
                && plan.ParentExactCleanCompletion
                && plan.ParentPlan.FlagsAtStop is null
                && plan.Outcome
                    == GameSocketParserDiagnosticCleanupSecondXaddOutcome
                        .BoundedBeforeDecAt34D8
                && plan.StopBeforeRawFileOffset == 0x00BD34D8
                && plan.LastReachedRawFileOffset == 0x00BD34D4
                && plan.ExactBoundedCompletion
                && plan.ExactCleanCompletion
                && plan.FreshPostCallEBXCaptureState
                    == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured
                && plan.FreshDWORDCaptureState
                    == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured
                && plan.FreshDWORDReadAttempted
                && plan.FreshDWORDCaptured
                && plan.MemoryRead is {
                    InstructionRawFileOffset: 0x00BD34D4,
                    Address: var readAddress,
                    Value: var capturedValue,
                    Attempted: true,
                    Captured: true,
                    ObservedAtLockedRmw: true
                }
                && readAddress == plan.ParentPlan.EAXValue
                && capturedValue == oldValue
                && plan.MemoryWrite is {
                    InstructionRawFileOffset: 0x00BD34D4,
                    Address: var writeAddress,
                    OldValue: var writeOldValue,
                    NewValue: var writeNewValue,
                    Attempted: true,
                    Committed: true
                }
                && writeAddress == plan.ParentPlan.EAXValue
                && writeOldValue == oldValue
                && writeNewValue == result
                && plan.EAXValue == plan.ParentPlan.EAXValue
                && plan.ESIValue == plan.ParentPlan.ESIValue
                && plan.EBXValue == oldValue
                && plan.ECXValue is null
                && plan.ESPValue is null
                && plan.EAXUnchanged
                && plan.ESIUnchanged
                && plan.EBXReceivedOldMemoryValue
                && plan.WriteIsOldMemoryPlusFreshEBX
                && plan.FlagsAtStop == expectedFlags
                && plan.FlagsPropagated
                && plan.Instructions.Length == 1
                && plan.Instructions[0] is {
                    InstructionRawFileOffset: 0x00BD34D4,
                    Encoding: "F0 0F C1 18",
                    LockPrefixPresent: true,
                    MemoryAddress: var instructionAddress,
                    StackPointer: null,
                    EAXBefore: var eaxBefore,
                    EAXAfter: var eaxAfter,
                    EBXBefore: var ebxBefore,
                    EBXAfter: var ebxAfter,
                    ECXBefore: null,
                    ECXAfter: null,
                    ESIBefore: var esiBefore,
                    ESIAfter: var esiAfter,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: null,
                    FlagsAfter: var flagsAfter
                }
                && instructionAddress == plan.ParentPlan.EAXValue
                && eaxBefore == plan.ParentPlan.EAXValue
                && eaxAfter == plan.ParentPlan.EAXValue
                && ebxBefore == freshEBX
                && ebxAfter == oldValue
                && esiBefore == plan.ParentPlan.ESIValue
                && esiAfter == plan.ParentPlan.ESIValue
                && flagsAfter == expectedFlags
                && plan.LockPrefixRecorded
                && !plan.ESPLineageCaptured
                && !plan.ECXLineageCaptured
                && !plan.NativeAtomicSemanticsModeled
                && !plan.ConcurrencyModeled
                && !plan.LinearizabilityModeled
                && !plan.NativeMemoryEffectsModeled
                && !plan.NativeABIOrProtocolSemanticsModeled
                && !plan.RuntimeSampleExecuted
                && !plan.ParentInstructionsReplayed
                && plan.OrderedSemanticPrefixPreserved;
            Mark(sawFlagValues, expectedFlags);
        }

        check("The second XADD uses old memory plus fresh EBX and computes all six ADD32 flags",
            allValuesExact && sawFlagValues.All(value => value));

        GameSocketParserDiagnosticCleanupSecondXaddPlan missingEBX =
            Project(freshPostCallEBX: null, freshDWORD: 7);
        GameSocketParserDiagnosticCleanupSecondXaddPlan missingEBXAndMemory =
            Project(freshPostCallEBX: null, freshDWORD: null);
        check("Missing post-call EBX prevents an RMW and leaves a supplied DWORD unused",
            missingEBX.ParentAdmitted
            && missingEBX.ParentPlan.FlagsAtStop is null
            && missingEBX.FreshPostCallEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Missing
            && missingEBX.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
            && missingEBX.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddOutcome
                    .MissingFreshPostCallEBXAt34D4
            && missingEBX.StopBeforeRawFileOffset == 0x00BD34D4
            && !missingEBX.ExactBoundedCompletion
            && !missingEBX.ExactCleanCompletion
            && missingEBX.MemoryRead is null
            && !missingEBX.FreshDWORDReadAttempted
            && !missingEBX.RmwWriteAttempted
            && !missingEBX.RmwWriteCommitted
            && missingEBX.MemoryWrite is {
                Address: 0x9008,
                OldValue: null,
                NewValue: null,
                Attempted: false,
                Committed: false
            }
            && missingEBX.EBXValue is null
            && missingEBX.Instructions.Length == 1
            && missingEBX.Instructions[0] is {
                Reached: true,
                Attempted: false,
                Committed: false,
                EBXBefore: null,
                EBXAfter: null,
                FlagsBefore: null,
                FlagsAfter: null
            }
            && missingEBX.FlagsAtStop is null
            && missingEBXAndMemory.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.NotReached
            && missingEBXAndMemory.StopBeforeRawFileOffset == 0x00BD34D4);

        GameSocketParserDiagnosticCleanupSecondXaddPlan missingDWORD =
            Project(freshPostCallEBX: 3, freshDWORD: null);
        check("A missing operation-time DWORD is read-attempted without write, exchange, or known flags",
            missingDWORD.ParentAdmitted
            && missingDWORD.FreshPostCallEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Captured
            && missingDWORD.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Missing
            && missingDWORD.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddOutcome.MissingFreshDWORDAt34D4
            && missingDWORD.StopBeforeRawFileOffset == 0x00BD34D4
            && missingDWORD.MemoryRead is {
                Address: 0x9008,
                Value: null,
                Attempted: true,
                Captured: false,
                ObservedAtLockedRmw: true
            }
            && missingDWORD.FreshDWORDReadAttempted
            && !missingDWORD.FreshDWORDCaptured
            && !missingDWORD.RmwWriteAttempted
            && !missingDWORD.RmwWriteCommitted
            && missingDWORD.MemoryWrite is {
                Address: 0x9008,
                OldValue: null,
                NewValue: null,
                Attempted: false,
                Committed: false
            }
            && missingDWORD.EBXValue == 3
            && missingDWORD.Instructions.Length == 1
            && missingDWORD.Instructions[0] is {
                Reached: true,
                Attempted: true,
                Committed: false,
                EBXBefore: 3,
                EBXAfter: 3,
                FlagsBefore: null,
                FlagsAfter: null
            }
            && missingDWORD.FlagsAtStop is null
            && !missingDWORD.ExactBoundedCompletion);

        GameSocketParserDiagnosticCleanupSecondXaddPlan freshEBXZero =
            Project(freshPostCallEBX: 0, freshDWORD: 9);
        GameSocketParserDiagnosticCleanupSecondXaddPlan freshEBXMaximum =
            Project(freshPostCallEBX: uint.MaxValue, freshDWORD: 9);
        uint? preCallEBX = PreCallEBX(freshEBXZero);
        check("Fresh post-call EBX is not inherited from the ancestor FFFFFFFF value",
            preCallEBX == uint.MaxValue
            && freshEBXZero.Capture.FreshPostCallEBX == 0
            && freshEBXZero.Instructions[0].EBXBefore == 0
            && freshEBXZero.EBXValue == 9
            && freshEBXMaximum.Capture.FreshPostCallEBX == uint.MaxValue
            && freshEBXMaximum.Instructions[0].EBXBefore == uint.MaxValue
            && freshEBXMaximum.EBXValue == 9
            && missingEBX.EBXValue is null);

        GameSocketParserDiagnosticCleanupSecondXaddPlan parentStoreProbe =
            Project(freshPostCallEBX: 5, freshDWORD: 9);
        GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect priorStore =
            parentStoreProbe.ParentPlan.MemoryEffects.Single(effect =>
                effect.Kind
                    == GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
                        .StoreFFFFFFFFAtESPPlus50
                && effect.Committed
                && effect.Address.HasValue);
        GameSocketParserDiagnosticCleanupSecondXaddPlan aliasedPriorStore =
            Project(
                freshPostCallEBX: 5,
                freshDWORD: 0x12345678,
                freshPostCallESI: unchecked(priorStore.Address!.Value - 8));
        check("The operation-time DWORD remains independent when its address aliases a prior store",
            aliasedPriorStore.ParentPlan.EAXValue == priorStore.Address
            && aliasedPriorStore.MemoryRead is {
                Address: var storeReadAddress,
                Value: 0x12345678,
                Captured: true,
                ObservedAtLockedRmw: true
            }
            && storeReadAddress == priorStore.Address
            && priorStore.Value == uint.MaxValue
            && aliasedPriorStore.MemoryWrite is {
                OldValue: 0x12345678,
                NewValue: 0x1234567D,
                Committed: true
            }
            && aliasedPriorStore.EBXValue == 0x12345678
            && aliasedPriorStore.OrderedSemanticPrefixPreserved);

        GameSocketParserDiagnosticCleanupSecondXaddPlan priorRmwProbe =
            Project(freshPostCallEBX: 5, freshDWORD: 9);
        GameSocketParserDiagnosticCleanupXaddPlan priorXadd =
            priorRmwProbe.ParentPlan.ParentPlan.ParentPlan.ParentPlan.ParentPlan;
        uint priorRmwAddress = priorXadd.MemoryWrite!.Value.Address!.Value;
        GameSocketParserDiagnosticCleanupSecondXaddPlan aliasedPriorRmw =
            Project(
                freshPostCallEBX: 7,
                freshDWORD: 0xABCDEF01,
                freshPostCallESI: unchecked(priorRmwAddress - 8));
        check("The operation-time DWORD remains independent when its address aliases the prior XADD",
            aliasedPriorRmw.ParentPlan.EAXValue == priorRmwAddress
            && aliasedPriorRmw.MemoryRead is {
                Address: var rmwReadAddress,
                Value: 0xABCDEF01,
                Captured: true,
                ObservedAtLockedRmw: true
            }
            && rmwReadAddress == priorRmwAddress
            && aliasedPriorRmw.MemoryWrite is {
                OldValue: 0xABCDEF01,
                NewValue: 0xABCDEF08,
                Committed: true
            }
            && aliasedPriorRmw.MemoryWrite.Value.OldValue
                != priorXadd.MemoryWrite.Value.NewValue
            && aliasedPriorRmw.OrderedSemanticPrefixPreserved);

        GameSocketParserDiagnosticCleanupSecondXaddPlan replaySource =
            Project(freshPostCallEBX: 0x1234, freshDWORD: 0x5678);
        GameSocketParserDiagnosticCleanupSecondXaddPlan replay =
            GameSocketParserDiagnosticCleanupSecondXaddProjection.Project(
                replaySource.Capture);
        check("Immutable input reprojects the complete LEA parent and second XADD evidence",
            replay.Capture == replaySource.Capture
            && replay.Outcome == replaySource.Outcome
            && replay.ParentPlan.Capture == replay.Capture.ParentCapture
            && replay.ParentPlan.ParentPlan.Capture
                == replay.Capture.ParentCapture.ParentCapture
            && replay.FreshPostCallEBXCaptureState
                == replaySource.FreshPostCallEBXCaptureState
            && replay.FreshDWORDCaptureState == replaySource.FreshDWORDCaptureState
            && replay.ParentPlan.Outcome == replaySource.ParentPlan.Outcome
            && replay.ParentPlan.Instructions.SequenceEqual(
                replaySource.ParentPlan.Instructions)
            && replay.ParentPlan.Reads.SequenceEqual(replaySource.ParentPlan.Reads)
            && replay.ParentPlan.MemoryEffects.SequenceEqual(
                replaySource.ParentPlan.MemoryEffects)
            && replay.ParentPlan.EAXValue == replaySource.ParentPlan.EAXValue
            && replay.ParentPlan.ESIValue == replaySource.ParentPlan.ESIValue
            && replay.ParentPlan.FlagsAtStop == replaySource.ParentPlan.FlagsAtStop
            && replay.Instructions.SequenceEqual(replaySource.Instructions)
            && replay.MemoryRead == replaySource.MemoryRead
            && replay.MemoryWrite == replaySource.MemoryWrite
            && replay.EAXValue == replaySource.EAXValue
            && replay.EBXValue == replaySource.EBXValue
            && replay.ESIValue == replaySource.ESIValue
            && replay.FlagsAtStop == replaySource.FlagsAtStop
            && replay.LastReachedRawFileOffset == replaySource.LastReachedRawFileOffset
            && replay.StopBeforeRawFileOffset == replaySource.StopBeforeRawFileOffset
            && replay.ParentExactCompletion == replaySource.ParentExactCompletion
            && replay.ParentExactBoundedCompletion
                == replaySource.ParentExactBoundedCompletion
            && replay.ChildBoundedCompletionBoundaryReached
                == replaySource.ChildBoundedCompletionBoundaryReached
            && replay.OrderedSemanticPrefixPreserved
            && replay.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupSecondXaddPlan pendingParent =
            Project(
                freshPostCallEBX: 0xA100,
                freshDWORD: 0xA200,
                returnedNormally: null);
        GameSocketParserDiagnosticCleanupSecondXaddPlan missingPostCallESI =
            Project(freshPostCallEBX: 0xB100, freshDWORD: 0xB200, freshPostCallESI: null);
        GameSocketParserDiagnosticCleanupSecondXaddPlan rejectedBranch =
            Project(freshPostCallEBX: 0xC100, freshDWORD: 0xC200, priorXaddDWORD: 2);
        check("Rejected pending, missing-ESI, and branch parents preserve their own bounds and unused inputs",
            pendingParent.ParentPlan.StopBeforeRawFileOffset == 0x00BD34CF
            && pendingParent.FreshPostCallEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
            && pendingParent.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
            && pendingParent.Instructions.IsEmpty
            && pendingParent.StopBeforeRawFileOffset
                == pendingParent.ParentPlan.StopBeforeRawFileOffset
            && missingPostCallESI.ParentPlan.StopBeforeRawFileOffset == 0x00BD34D1
            && missingPostCallESI.FreshPostCallEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
            && missingPostCallESI.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
            && missingPostCallESI.Instructions.IsEmpty
            && missingPostCallESI.StopBeforeRawFileOffset
                == missingPostCallESI.ParentPlan.StopBeforeRawFileOffset
            && rejectedBranch.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && rejectedBranch.FreshPostCallEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
            && rejectedBranch.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
            && rejectedBranch.Instructions.IsEmpty
            && rejectedBranch.StopBeforeRawFileOffset
                == rejectedBranch.ParentPlan.StopBeforeRawFileOffset
            && rejectedBranch.ExactBoundedCompletion
                == rejectedBranch.ParentPlan.ExactBoundedCompletion
            && rejectedBranch.ParentExactBoundedCompletion
                == rejectedBranch.ParentPlan.ExactBoundedCompletion
            && !rejectedBranch.ChildBoundedCompletionBoundaryReached
            && !rejectedBranch.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupSecondXaddPlan Project(
        uint? freshPostCallEBX,
        uint? freshDWORD,
        uint? freshPostCallESI = 0x9000,
        bool? returnedNormally = true,
        uint? freshVptr = 0x7000,
        uint? priorXaddDWORD = 1)
    {
        GameSocketParserDiagnosticCleanupStackPrefixCapture stackCapture =
            CreateParentCapture();
        GameSocketParserDiagnosticCleanupZeroBranchCapture zeroBranchCapture =
            new(stackCapture);
        GameSocketParserDiagnosticCleanupRegisterPreparationCapture registerCapture =
            new(zeroBranchCapture);
        GameSocketParserDiagnosticCleanupXaddCapture xaddCapture =
            new(registerCapture, priorXaddDWORD);
        GameSocketParserDiagnosticCleanupXaddBranchCapture branchCapture =
            new(xaddCapture);
        GameSocketParserDiagnosticCleanupVirtualCallPreparationCapture preparationCapture =
            new(branchCapture, freshVptr);
        GameSocketParserDiagnosticCleanupVirtualCallReturnCapture returnCapture =
            new(preparationCapture, 0x12345678, returnedNormally);
        GameSocketParserDiagnosticCleanupPostCallAddressCapture addressCapture =
            new(returnCapture, freshPostCallESI);
        return GameSocketParserDiagnosticCleanupSecondXaddProjection.Project(
            new(addressCapture, freshPostCallEBX, freshDWORD));
    }

    private static uint? PreCallEBX(
        GameSocketParserDiagnosticCleanupSecondXaddPlan plan) =>
        plan.ParentPlan.ParentPlan.ParentPlan.ParentPlan.ParentPlan.ParentPlan.EBXValue;

    private static GameSocketParserDiagnosticCleanupStackPrefixCapture CreateParentCapture()
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

    private static GameSocketParserStoredByteSelectorGateFlags IndependentFlagsForAdd(
        uint left,
        uint right)
    {
        uint result = unchecked(left + right);
        long signedResult = (long)unchecked((int)left) + unchecked((int)right);
        return new(
            Carry: (ulong)left + right > uint.MaxValue,
            Parity: (BitOperations.PopCount((byte)result) & 1) == 0,
            AuxiliaryCarry: (left & 0xF) + (right & 0xF) > 0xF,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: signedResult < int.MinValue || signedResult > int.MaxValue);
    }

    private static void Mark(
        bool[] observed,
        GameSocketParserStoredByteSelectorGateFlags flags)
    {
        observed[0] |= flags.Carry == true;
        observed[1] |= flags.Carry == false;
        observed[2] |= flags.Parity == true;
        observed[3] |= flags.Parity == false;
        observed[4] |= flags.AuxiliaryCarry == true;
        observed[5] |= flags.AuxiliaryCarry == false;
        observed[6] |= flags.Zero == true;
        observed[7] |= flags.Zero == false;
        observed[8] |= flags.Sign == true;
        observed[9] |= flags.Sign == false;
        observed[10] |= flags.Overflow == true;
        observed[11] |= flags.Overflow == false;
    }

    private static byte[] ParseBytes(string encoding) =>
        encoding.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
