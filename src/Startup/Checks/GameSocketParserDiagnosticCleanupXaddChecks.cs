using Firefall.Reconstruction.Startup;
using System.Numerics;

internal static class GameSocketParserDiagnosticCleanupXaddChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupXaddProjection.CoordinatorProvidedBytes);
        check("The supplied bytes anchor LOCK XADD and stop before the following JNZ",
            !GameSocketParserDiagnosticCleanupXaddProjection.BytesIndependentlyVerified
            && suppliedBytes.Length == 16
            && string.Join(" ", suppliedBytes.Select(value => value.ToString("X2")))
                == GameSocketParserDiagnosticCleanupXaddProjection.CoordinatorProvidedBytes
            && GameSocketParserDiagnosticCleanupXaddProjection.LockedXaddRawFileOffset
                == 0x00BD34C5
            && GameSocketParserDiagnosticCleanupXaddProjection.LockedXaddEncoding
                == "F0 0F C1 08"
            && GameSocketParserDiagnosticCleanupXaddProjection.JnzRawFileOffset
                == 0x00BD34C9
            && suppliedBytes.Take(4).SequenceEqual(ParseBytes("F0 0F C1 08"))
            && suppliedBytes.Skip(4).Take(2).SequenceEqual(ParseBytes("75 17")));

        uint[] values =
        {
            0,
            1,
            2,
            0x10,
            0x11,
            0x7FFFFFFF,
            0x80000000,
            uint.MaxValue
        };
        bool allValuesExact = true;
        bool sawOneToZero = false;
        bool sawZeroToMax = false;
        bool sawSignedOverflow = false;
        foreach (uint oldValue in values)
        {
            GameSocketParserDiagnosticCleanupXaddPlan plan = Project(
                esi: 0x7000,
                freshDWORDAtLockedRmw: oldValue);
            uint expectedNewValue = unchecked(oldValue + uint.MaxValue);
            GameSocketParserStoredByteSelectorGateFlags expectedFlags =
                FlagsForAdd(oldValue, expectedNewValue);
            allValuesExact &= plan.ParentBranchAdmitted
                && plan.ParentExactBoundedCompletion
                && plan.ParentPlan.IsCleanCompletion
                && plan.Outcome
                    == GameSocketParserDiagnosticCleanupXaddOutcome.BoundedBeforeJnzAt34C9
                && plan.StopBeforeRawFileOffset == 0x00BD34C9
                && plan.LockedXaddRawFileOffset == 0x00BD34C5
                && plan.ExactBoundedCompletion
                && plan.IsCleanCompletion
                && plan.FreshDWORDCaptureState
                    == GameSocketParserDiagnosticCleanupXaddCaptureState.Captured
                && plan.FreshDWORDReadAttempted
                && plan.FreshDWORDCaptured
                && plan.MemoryRead is {
                    InstructionRawFileOffset: 0x00BD34C5,
                    Value: var capturedValue,
                    Attempted: true,
                    Captured: true,
                    ObservedAtLockedRmw: true
                }
                && capturedValue == oldValue
                && plan.MemoryWrite is {
                    InstructionRawFileOffset: 0x00BD34C5,
                    OldValue: var writeOldValue,
                    NewValue: var writeNewValue,
                    Attempted: true,
                    Committed: true
                }
                && writeOldValue == oldValue
                && writeNewValue == expectedNewValue
                && plan.EAXValue == 0x7004
                && plan.EBXValue == uint.MaxValue
                && plan.ECXValue == oldValue
                && plan.ESIValue == 0x7000
                && plan.ResultESP == plan.ParentPlan.ResultESP
                && plan.EAXUnchanged
                && plan.EBXUnchanged
                && plan.ESIUnchanged
                && plan.ESPUnchanged
                && plan.ECXReceivedOldMemoryValue
                && plan.FlagsAtStop == expectedFlags
                && plan.FlagsPropagated
                && plan.Instructions.Length == 1
                && plan.Instructions[0] is {
                    InstructionRawFileOffset: 0x00BD34C5,
                    Encoding: "F0 0F C1 08",
                    LockPrefixPresent: true,
                    MemoryAddress: 0x7004,
                    EAXBefore: 0x7004,
                    EAXAfter: 0x7004,
                    EBXBefore: uint.MaxValue,
                    EBXAfter: uint.MaxValue,
                    ECXBefore: uint.MaxValue,
                    ECXAfter: var ecxAfter,
                    ESIBefore: 0x7000,
                    ESIAfter: 0x7000,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsAfter: var flagsAfter
                }
                && ecxAfter == oldValue
                && flagsAfter == expectedFlags
                && plan.LockPrefixRecorded
                && !plan.NativeAtomicSemanticsModeled
                && !plan.ConcurrencyModeled
                && !plan.LinearizabilityModeled
                && !plan.CleanupReleaseIdentityModeled
                && !plan.NativeMemoryEffectsModeled
                && !plan.NativeABIOrProtocolSemanticsModeled
                && !plan.ParserExecutionModeled
                && !plan.RuntimeSampleExecuted
                && !plan.ParentInstructionsReplayed
                && plan.OrderedSemanticPrefixPreserved;
            sawOneToZero |= oldValue == 1 && expectedNewValue == 0 && expectedFlags.Zero == true;
            sawZeroToMax |= oldValue == 0
                && expectedNewValue == uint.MaxValue
                && !expectedFlags.Carry.GetValueOrDefault();
            sawSignedOverflow |= oldValue == 0x80000000
                && expectedNewValue == 0x7FFFFFFF
                && expectedFlags.Overflow.GetValueOrDefault();
        }

        check("LOCK XADD decrements with wraparound and ADD32 flags, including zero and overflow",
            allValuesExact && sawOneToZero && sawZeroToMax && sawSignedOverflow);

        GameSocketParserDiagnosticCleanupXaddPlan missing =
            Project(esi: 0x7000, freshDWORDAtLockedRmw: null);
        check("A missing operation-time DWORD read stops at 34C5 without an RMW write or ECX result",
            missing.ParentBranchAdmitted
            && missing.ParentExactBoundedCompletion
            && missing.ParentPlan.IsCleanCompletion
            && missing.Outcome
                == GameSocketParserDiagnosticCleanupXaddOutcome.MissingFreshDWORDAt34C5
            && missing.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupXaddCaptureState.Missing
            && missing.StopBeforeRawFileOffset == 0x00BD34C5
            && !missing.ExactBoundedCompletion
            && !missing.IsCleanCompletion
            && missing.FreshDWORDReadAttempted
            && !missing.FreshDWORDCaptured
            && missing.MemoryRead is {
                Address: 0x7004,
                Value: null,
                Attempted: true,
                Captured: false,
                ObservedAtLockedRmw: true
            }
            && !missing.RmwWriteAttempted
            && !missing.RmwWriteCommitted
            && missing.MemoryWrite is {
                Address: 0x7004,
                OldValue: null,
                NewValue: null,
                Attempted: false,
                Committed: false
            }
            && missing.ECXValue == uint.MaxValue
            && missing.Instructions.Length == 1
            && missing.Instructions[0].Reached
            && !missing.Instructions[0].Attempted
            && !missing.Instructions[0].Committed
            && missing.FlagsAtStop == missing.ParentPlan.FlagsAtStop);

        GameSocketParserDiagnosticCleanupXaddPlan differentCaptureA =
            Project(esi: 0x7000, freshDWORDAtLockedRmw: 0x12345678);
        GameSocketParserDiagnosticCleanupXaddPlan differentCaptureB =
            Project(esi: 0x7000, freshDWORDAtLockedRmw: 0x87654321);
        check("The XADD read uses its own immutable capture rather than an ancestor read value",
            differentCaptureA.ParentPlan.Reads.SequenceEqual(differentCaptureB.ParentPlan.Reads)
            && differentCaptureA.ParentPlan.MemoryEffects.SequenceEqual(
                differentCaptureB.ParentPlan.MemoryEffects)
            && differentCaptureA.MemoryRead?.Value == 0x12345678
            && differentCaptureB.MemoryRead?.Value == 0x87654321
            && differentCaptureA.MemoryWrite?.OldValue == 0x12345678
            && differentCaptureB.MemoryWrite?.OldValue == 0x87654321
            && differentCaptureA.MemoryWrite?.NewValue
                == unchecked(0x12345678u + uint.MaxValue)
            && differentCaptureB.MemoryWrite?.NewValue
                == unchecked(0x87654321u + uint.MaxValue));

        GameSocketParserDiagnosticCleanupXaddPlan ancestorStoreProbe =
            Project(esi: 0x7000, freshDWORDAtLockedRmw: 9);
        GameSocketParserDiagnosticCleanupStackPrefixMemoryEffect ancestorStore =
            ancestorStoreProbe.ParentPlan.MemoryEffects.Single(effect =>
                effect.Kind
                    == GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
                        .StoreFFFFFFFFAtESPPlus50
                && effect.Committed
                && effect.Address.HasValue);
        uint aliasingESI = unchecked(ancestorStore.Address!.Value - 4);
        GameSocketParserDiagnosticCleanupXaddPlan aliasingStore =
            Project(esi: aliasingESI, freshDWORDAtLockedRmw: 9);
        check("An operation-time XADD capture remains independent when its address aliases an ancestor store",
            aliasingStore.EAXValue == ancestorStore.Address
            && aliasingStore.ParentPlan.MemoryEffects.Any(effect =>
                effect.Kind
                    == GameSocketParserDiagnosticCleanupStackPrefixMemoryEffectKind
                        .StoreFFFFFFFFAtESPPlus50
                && effect.Address == aliasingStore.EAXValue
                && effect.Value == uint.MaxValue
                && effect.Committed)
            && aliasingStore.MemoryRead is {
                Address: var readAddress,
                Value: 9,
                Captured: true,
                ObservedAtLockedRmw: true
            }
            && readAddress == ancestorStore.Address
            && aliasingStore.MemoryWrite is {
                OldValue: 9,
                NewValue: 8,
                Committed: true
            }
            && aliasingStore.ECXValue == 9
            && aliasingStore.OrderedSemanticPrefixPreserved);

        GameSocketParserDiagnosticCleanupXaddPlan replay =
            GameSocketParserDiagnosticCleanupXaddProjection.Project(
                differentCaptureA.Capture);
        check("The immutable XADD and parent captures reproject with identical ordered scalar evidence",
            differentCaptureA.Capture.FreshDWORDObservedAtLockedRmw
                == replay.Capture.FreshDWORDObservedAtLockedRmw
            && differentCaptureA.ParentPlan.Outcome == replay.ParentPlan.Outcome
            && differentCaptureA.ParentPlan.StopBeforeRawFileOffset
                == replay.ParentPlan.StopBeforeRawFileOffset
            && differentCaptureA.ParentPlan.ESIValue == replay.ParentPlan.ESIValue
            && differentCaptureA.ParentPlan.EAXValue == replay.ParentPlan.EAXValue
            && differentCaptureA.ParentPlan.EBXValue == replay.ParentPlan.EBXValue
            && differentCaptureA.ParentPlan.ECXValue == replay.ParentPlan.ECXValue
            && differentCaptureA.ParentPlan.ResultESP == replay.ParentPlan.ResultESP
            && differentCaptureA.ParentPlan.FlagsAtStop == replay.ParentPlan.FlagsAtStop
            && differentCaptureA.ParentPlan.Instructions.SequenceEqual(
                replay.ParentPlan.Instructions)
            && differentCaptureA.ParentPlan.Reads.SequenceEqual(replay.ParentPlan.Reads)
            && differentCaptureA.ParentPlan.MemoryEffects.SequenceEqual(
                replay.ParentPlan.MemoryEffects)
            && differentCaptureA.Outcome == replay.Outcome
            && differentCaptureA.FreshDWORDCaptureState == replay.FreshDWORDCaptureState
            && differentCaptureA.ResultESP == replay.ResultESP
            && differentCaptureA.ESIValue == replay.ESIValue
            && differentCaptureA.EAXValue == replay.EAXValue
            && differentCaptureA.EBXValue == replay.EBXValue
            && differentCaptureA.ECXValue == replay.ECXValue
            && differentCaptureA.FlagsAtStop == replay.FlagsAtStop
            && differentCaptureA.StopBeforeRawFileOffset == replay.StopBeforeRawFileOffset
            && differentCaptureA.Instructions.SequenceEqual(replay.Instructions)
            && differentCaptureA.MemoryRead == replay.MemoryRead
            && differentCaptureA.MemoryWrite == replay.MemoryWrite
            && differentCaptureA.ParentExactBoundedCompletion
                == replay.ParentExactBoundedCompletion
            && differentCaptureA.ExactBoundedCompletion == replay.ExactBoundedCompletion
            && differentCaptureA.IsCleanCompletion == replay.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupXaddPlan missingParentRead =
            Project(esi: null, freshDWORDAtLockedRmw: 7);
        check("A supplied XADD DWORD remains unused when the parent stack read is missing",
            !missingParentRead.ParentBranchAdmitted
            && missingParentRead.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupXaddCaptureState.SuppliedButUnused
            && missingParentRead.Outcome
                == GameSocketParserDiagnosticCleanupXaddOutcome.ParentBranchNotAdmitted
            && missingParentRead.ParentPlan.StopBeforeRawFileOffset == 0x00BD34B5
            && missingParentRead.Instructions.IsEmpty
            && missingParentRead.MemoryRead is null
            && missingParentRead.MemoryWrite is null
            && missingParentRead.StopBeforeRawFileOffset
                == missingParentRead.ParentPlan.StopBeforeRawFileOffset
            && !missingParentRead.ExactBoundedCompletion
            && !missingParentRead.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupXaddPlan rejectedZeroBranch =
            Project(esi: 0, freshDWORDAtLockedRmw: 7);
        check("The rejected zero branch preserves its 34E2 bound and leaves XADD evidence unused",
            !rejectedZeroBranch.ParentBranchAdmitted
            && rejectedZeroBranch.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupZeroBranchOutcome.JzTakenTo34E2
            && rejectedZeroBranch.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && rejectedZeroBranch.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupXaddCaptureState.SuppliedButUnused
            && rejectedZeroBranch.Instructions.IsEmpty
            && rejectedZeroBranch.MemoryRead is null
            && rejectedZeroBranch.MemoryWrite is null
            && rejectedZeroBranch.StopBeforeRawFileOffset == 0x00BD34E2
            && rejectedZeroBranch.ExactBoundedCompletion
            && !rejectedZeroBranch.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupXaddPlan otherAncestor =
            Project(esi: 1, freshDWORDAtLockedRmw: 7, comparisonValue: 2);
        check("A different ancestor forwards its 31F8 bound without entering the XADD suffix",
            !otherAncestor.ParentBranchAdmitted
            && otherAncestor.ParentPlan.ParentPlan.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticDestinationBranchGateOutcome.JaeTakenTo31F8
            && otherAncestor.ParentPlan.StopBeforeRawFileOffset == 0x00BD31F8
            && otherAncestor.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupXaddCaptureState.SuppliedButUnused
            && otherAncestor.Instructions.IsEmpty
            && otherAncestor.MemoryRead is null
            && otherAncestor.MemoryWrite is null
            && otherAncestor.StopBeforeRawFileOffset == 0x00BD31F8
            && otherAncestor.ExactBoundedCompletion
            && !otherAncestor.IsCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupXaddPlan Project(
        uint? esi,
        uint? freshDWORDAtLockedRmw,
        uint comparisonValue = 0,
        uint postCallESP = 0x8FEC,
        uint? freshEBP = 0x7000) =>
        GameSocketParserDiagnosticCleanupXaddProjection.Project(
            new(
                new(
                    new(
                        CreateParentCapture(
                            comparisonValue,
                            postCallESP,
                            freshEBP,
                            esi))),
                freshDWORDAtLockedRmw));

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

    private static GameSocketParserStoredByteSelectorGateFlags FlagsForAdd(
        uint oldValue,
        uint newValue)
    {
        bool parity = (BitOperations.PopCount((byte)newValue) & 1) == 0;
        bool auxiliaryCarry = (oldValue & 0xF) != 0;
        bool overflow = (oldValue & 0x80000000) != 0
            && ((oldValue ^ newValue) & 0x80000000) != 0;
        return new(
            Carry: oldValue != 0,
            Parity: parity,
            AuxiliaryCarry: auxiliaryCarry,
            Zero: newValue == 0,
            Sign: (newValue & 0x80000000) != 0,
            Overflow: overflow);
    }

    private static byte[] ParseBytes(string encoding) =>
        encoding.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
