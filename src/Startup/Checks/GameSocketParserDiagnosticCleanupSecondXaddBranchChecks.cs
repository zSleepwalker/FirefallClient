using Firefall.Reconstruction.Startup;
using System.Numerics;

internal static class GameSocketParserDiagnosticCleanupSecondXaddBranchChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.CoordinatorProvidedBytes);
        check("The supplied bytes anchor DEC EBX and the following JNZ without entering either target",
            !GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.BytesIndependentlyVerified
            && suppliedBytes.Length == 17
            && GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.DecRawFileOffset
                == 0x00BD34D8
            && GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.DecEncoding == "4B"
            && GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.JnzRawFileOffset
                == 0x00BD34D9
            && GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.JnzEncoding == "75 07"
            && GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.FallThroughRawFileOffset
                == 0x00BD34DB
            && GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.JnzTargetRawFileOffset
                == 0x00BD34E2
            && 0x00BD34DB + 7
                == GameSocketParserDiagnosticCleanupSecondXaddBranchProjection
                    .JnzTargetRawFileOffset
            && suppliedBytes.Take(1).SequenceEqual(ParseBytes("4B"))
            && suppliedBytes.Skip(1).Take(2).SequenceEqual(ParseBytes("75 07"))
            && suppliedBytes.Skip(3).SequenceEqual(ParseBytes(
                "8B 06 8B CE FF 50 04 8B 4C 24 48 64 89 0D")));

        (uint OldMemory, uint FreshPostCallEBX)[] cases =
        {
            (0, 0x1234),
            (1, uint.MaxValue),
            (uint.MaxValue, 1),
            (0x80000000, 0)
        };
        bool casesExact = true;
        bool representativeClean = false;
        foreach ((uint oldMemory, uint freshEBX) in cases)
        {
            GameSocketParserDiagnosticCleanupSecondXaddBranchPlan plan =
                Project(freshEBX, oldMemory);
            uint result = unchecked(oldMemory - 1);
            bool priorAddCarry = (ulong)oldMemory + freshEBX > uint.MaxValue;
            GameSocketParserStoredByteSelectorGateFlags expectedFlags =
                ExpectedDecrementFlags(oldMemory, priorAddCarry);
            bool taken = result != 0;
            uint expectedTarget = taken ? 0x00BD34E2u : 0x00BD34DBu;
            if (oldMemory == 0)
            {
                representativeClean = plan.ExactCleanCompletion;
            }

            casesExact &= plan.ParentAdmitted
                && plan.ParentPlan.Outcome
                    == GameSocketParserDiagnosticCleanupSecondXaddOutcome
                        .BoundedBeforeDecAt34D8
                && plan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34D8
                && plan.ParentPlan.EBXValue == oldMemory
                && plan.Outcome == (taken
                    ? GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome
                        .JnzTakenTo34E2
                    : GameSocketParserDiagnosticCleanupSecondXaddBranchOutcome
                        .JnzNotTakenTo34DB)
                && plan.ExactBoundedCompletion
                && plan.StopBeforeRawFileOffset == expectedTarget
                && plan.LastReachedRawFileOffset == 0x00BD34D9
                && plan.BranchTargetRawFileOffset == expectedTarget
                && plan.BranchTargetCodeVirtualAddress
                    == GameSocketParserCountConsumptionStoresProjection
                        .RawFileOffsetToCodeVirtualAddress(expectedTarget)
                && plan.FallThroughRawFileOffset == 0x00BD34DB
                && plan.EAXValue == plan.ParentPlan.EAXValue
                && plan.ESIValue == plan.ParentPlan.ESIValue
                && plan.EBXValue == result
                && plan.ECXValue is null
                && plan.ESPValue is null
                && plan.FlagsAtStop == expectedFlags
                && plan.FlagsPropagated
                && plan.CarryPreservedByDecrement
                && plan.JnzFlagsPreserved
                && plan.RegistersPreserved
                && plan.DecrementAttempted
                && plan.DecrementReached
                && plan.DecrementCommitted
                && plan.JnzAttempted
                && plan.JnzReached
                && plan.JnzCommitted
                && plan.JnzTaken == taken
                && plan.Instructions.Length == 2
                && plan.Instructions[0] is {
                    Kind: GameSocketParserDiagnosticCleanupSecondXaddBranchInstructionKind
                        .DecrementEBXAt34D8,
                    InstructionRawFileOffset: 0x00BD34D8,
                    Encoding: "4B",
                    StackPointer: null,
                    EAX: var decEAX,
                    EBXBefore: var ebxBefore,
                    EBXAfter: var ebxAfter,
                    ECX: null,
                    ESI: var decESI,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    Taken: null,
                    TargetRawFileOffset: null,
                    FlagsBefore: var flagsBefore,
                    FlagsAfter: var flagsAfter
                }
                && decEAX == plan.ParentPlan.EAXValue
                && decESI == plan.ParentPlan.ESIValue
                && ebxBefore == oldMemory
                && ebxAfter == result
                && flagsBefore == plan.ParentPlan.FlagsAtStop
                && flagsAfter == expectedFlags
                && plan.Instructions[1] is {
                    Kind: GameSocketParserDiagnosticCleanupSecondXaddBranchInstructionKind
                        .JnzAt34D9,
                    InstructionRawFileOffset: 0x00BD34D9,
                    Encoding: "75 07",
                    StackPointer: null,
                    EAX: var jnzEAX,
                    EBXBefore: var jnzEBXBefore,
                    EBXAfter: var jnzEBXAfter,
                    ECX: null,
                    ESI: var jnzESI,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    Taken: var jnzTaken,
                    TargetRawFileOffset: var target,
                    FlagsBefore: var jnzFlagsBefore,
                    FlagsAfter: var jnzFlagsAfter
                }
                && jnzEAX == plan.ParentPlan.EAXValue
                && jnzESI == plan.ParentPlan.ESIValue
                && jnzEBXBefore == result
                && jnzEBXAfter == result
                && jnzTaken == taken
                && target == expectedTarget
                && jnzFlagsBefore == expectedFlags
                && jnzFlagsAfter == expectedFlags
                && plan.ParentPlan.MemoryRead is {
                    Address: var readAddress,
                    Value: var capturedOld,
                    Attempted: true,
                    Captured: true,
                    ObservedAtLockedRmw: true
                }
                && readAddress == plan.ParentPlan.EAXValue
                && capturedOld == oldMemory
                && plan.ParentPlan.MemoryWrite is {
                    OldValue: var writeOld,
                    NewValue: var writeNew,
                    Attempted: true,
                    Committed: true
                }
                && writeOld == oldMemory
                && writeNew == unchecked(oldMemory + freshEBX)
                && expectedFlags.Carry == priorAddCarry
                && !plan.BranchTargetInstructionExecuted
                && !plan.FollowingTargetInstructionExecuted
                && !plan.ParentInstructionsReplayed
                && !plan.NativeAtomicSemanticsModeled
                && !plan.NativeMemoryEffectsModeled
                && !plan.NativeABIOrProtocolSemanticsModeled
                && !plan.RuntimeSampleExecuted;
        }

        check("DEC wraps EBX, preserves the second XADD carry, computes the other flags, and JNZ stops at an unentered target",
            casesExact
            && representativeClean
            && ExpectedDecrementFlags(0, false) is {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && ExpectedDecrementFlags(1, true) is {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && ExpectedDecrementFlags(uint.MaxValue, true) is {
                Carry: true,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && ExpectedDecrementFlags(0x80000000, false) is {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: true,
                Zero: false,
                Sign: false,
                Overflow: true
            });

        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan source =
            Project(freshPostCallEBX: 0x1234, freshDWORD: 0);
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan replay =
            GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.Project(
                source.Capture);
        bool sourceClean = source.ExactCleanCompletion;
        bool replayClean = replay.ExactCleanCompletion;
        check("The immutable input capture reprojects the same branch without forging or replacing parent evidence",
            sourceClean
            && replayClean
            && source.Capture == replay.Capture
            && source.Outcome == replay.Outcome
            && replay.ParentPlan.Capture == replay.Capture.ParentCapture
            && replay.ParentPlan.ParentPlan.Capture
                == replay.Capture.ParentCapture.ParentCapture
            && source.ParentPlan.Capture == replay.ParentPlan.Capture
            && source.ParentPlan.Instructions.SequenceEqual(
                replay.ParentPlan.Instructions)
            && source.ParentPlan.MemoryRead == replay.ParentPlan.MemoryRead
            && source.ParentPlan.MemoryWrite == replay.ParentPlan.MemoryWrite
            && source.Instructions.SequenceEqual(replay.Instructions)
            && source.EAXValue == replay.EAXValue
            && source.EBXValue == replay.EBXValue
            && source.ESIValue == replay.ESIValue
            && source.FlagsAtStop == replay.FlagsAtStop
            && source.LastReachedRawFileOffset == replay.LastReachedRawFileOffset
            && source.StopBeforeRawFileOffset == replay.StopBeforeRawFileOffset
            && source.BranchTargetRawFileOffset == replay.BranchTargetRawFileOffset);

        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan missingEBX =
            Project(freshPostCallEBX: null, freshDWORD: 7);
        bool missingEBXClean = missingEBX.ExactCleanCompletion;
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan missingDWORD =
            Project(freshPostCallEBX: 3, freshDWORD: null);
        bool missingDWORDClean = missingDWORD.ExactCleanCompletion;
        check("Missing second-XADD inputs retain the actual D4 stop and do not fabricate DEC or JNZ",
            !missingEBXClean
            && !missingDWORDClean
            && !missingEBX.ParentAdmitted
            && !missingDWORD.ParentAdmitted
            && missingEBX.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddOutcome
                    .MissingFreshPostCallEBXAt34D4
            && missingDWORD.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupSecondXaddOutcome.MissingFreshDWORDAt34D4
            && missingEBX.ParentPlan.StopBeforeRawFileOffset == 0x00BD34D4
            && missingDWORD.ParentPlan.StopBeforeRawFileOffset == 0x00BD34D4
            && missingEBX.StopBeforeRawFileOffset == missingEBX.ParentPlan.StopBeforeRawFileOffset
            && missingDWORD.StopBeforeRawFileOffset
                == missingDWORD.ParentPlan.StopBeforeRawFileOffset
            && missingEBX.ParentPlan.FlagsAtStop is null
            && missingDWORD.ParentPlan.FlagsAtStop?.Carry is null
            && missingEBX.ParentPlan.FreshPostCallEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Missing
            && missingEBX.ParentPlan.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
            && missingDWORD.ParentPlan.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.Missing
            && missingEBX.Instructions.IsEmpty
            && missingDWORD.Instructions.IsEmpty
            && missingEBX.BranchTargetRawFileOffset is null
            && missingDWORD.BranchTargetRawFileOffset is null
            && missingEBX.ParentExactBoundedCompletion
                == missingEBX.ParentPlan.ExactBoundedCompletion
            && missingDWORD.ParentExactBoundedCompletion
                == missingDWORD.ParentPlan.ExactBoundedCompletion
            && missingEBX.ExactBoundedCompletion
                == missingEBX.ParentExactBoundedCompletion
            && missingDWORD.ExactBoundedCompletion
                == missingDWORD.ParentExactBoundedCompletion
            && !missingEBX.ChildBoundedCompletionBoundaryReached
            && !missingDWORD.ChildBoundedCompletionBoundaryReached);

        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan missingESI =
            Project(freshPostCallEBX: 0xB100, freshDWORD: 0xB200, freshPostCallESI: null);
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan pendingReturn =
            Project(freshPostCallEBX: 0xA100, freshDWORD: 0xA200, returnedNormally: null);
        GameSocketParserDiagnosticCleanupSecondXaddBranchPlan selected34E2 =
            Project(
                freshPostCallEBX: 0xC100,
                freshDWORD: 0xC200,
                priorXaddDWORD: 2);
        bool missingESIClean = missingESI.ExactCleanCompletion;
        bool pendingReturnClean = pendingReturn.ExactCleanCompletion;
        bool selected34E2Clean = selected34E2.ExactCleanCompletion;
        check("Rejected ESI, pending-CF, pending-return, and prior-branch captures keep their own parent completion",
            !missingESIClean
            && !missingDWORDClean
            && !pendingReturnClean
            && !selected34E2Clean
            && missingESI.ParentPlan.StopBeforeRawFileOffset == 0x00BD34D1
            && missingESI.ParentPlan.FreshPostCallEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
            && missingESI.ParentPlan.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
            && missingESI.Instructions.IsEmpty
            && missingESI.StopBeforeRawFileOffset == missingESI.ParentPlan.StopBeforeRawFileOffset
            && missingESI.ExactBoundedCompletion
                == missingESI.ParentExactBoundedCompletion
            && missingDWORD.ParentPlan.StopBeforeRawFileOffset == 0x00BD34D4
            && missingDWORD.ParentPlan.FlagsAtStop?.Carry is null
            && missingDWORD.Instructions.IsEmpty
            && missingDWORD.StopBeforeRawFileOffset
                == missingDWORD.ParentPlan.StopBeforeRawFileOffset
            && missingDWORD.ExactBoundedCompletion
                == missingDWORD.ParentExactBoundedCompletion
            && pendingReturn.ParentPlan.StopBeforeRawFileOffset == 0x00BD34CF
            && pendingReturn.Instructions.IsEmpty
            && pendingReturn.StopBeforeRawFileOffset
                == pendingReturn.ParentPlan.StopBeforeRawFileOffset
            && pendingReturn.ExactBoundedCompletion
                == pendingReturn.ParentExactBoundedCompletion
            && selected34E2.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && selected34E2.ParentPlan.FreshPostCallEBXCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
            && selected34E2.ParentPlan.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupSecondXaddCaptureState.SuppliedButUnused
            && selected34E2.Instructions.IsEmpty
            && selected34E2.StopBeforeRawFileOffset == 0x00BD34E2
            && selected34E2.ParentExactBoundedCompletion
                == selected34E2.ParentPlan.ExactBoundedCompletion
            && selected34E2.ExactBoundedCompletion
                == selected34E2.ParentExactBoundedCompletion
            && !missingESI.ChildBoundedCompletionBoundaryReached
            && !missingDWORD.ChildBoundedCompletionBoundaryReached
            && !pendingReturn.ChildBoundedCompletionBoundaryReached
            && !selected34E2.ChildBoundedCompletionBoundaryReached);
    }

    private static GameSocketParserDiagnosticCleanupSecondXaddBranchPlan Project(
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
        return GameSocketParserDiagnosticCleanupSecondXaddBranchProjection.Project(
            new(new(addressCapture, freshPostCallEBX, freshDWORD)));
    }

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

    private static GameSocketParserStoredByteSelectorGateFlags ExpectedDecrementFlags(
        uint oldValue,
        bool carry)
    {
        uint result = unchecked(oldValue - 1);
        bool evenParity = (BitOperations.PopCount((byte)result) & 1) == 0;
        return new(
            Carry: carry,
            Parity: evenParity,
            AuxiliaryCarry: (oldValue & 0xF) == 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: oldValue == 0x80000000);
    }

    private static byte[] ParseBytes(string encoding) =>
        encoding.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
