using Firefall.Reconstruction.Startup;
using System.Numerics;

internal static class GameSocketParserDiagnosticCleanupXaddBranchChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupXaddBranchProjection.CoordinatorProvidedBytes);
        check("The supplied bytes anchor JNZ at 34C9 and map its fall-through and target",
            !GameSocketParserDiagnosticCleanupXaddBranchProjection.BytesIndependentlyVerified
            && suppliedBytes.Length == 16
            && string.Join(" ", suppliedBytes.Select(value => value.ToString("X2")))
                == GameSocketParserDiagnosticCleanupXaddBranchProjection.CoordinatorProvidedBytes
            && GameSocketParserDiagnosticCleanupXaddBranchProjection.JnzRawFileOffset
                == 0x00BD34C9
            && GameSocketParserDiagnosticCleanupXaddBranchProjection.JnzEncoding == "75 17"
            && GameSocketParserDiagnosticCleanupXaddBranchProjection.FallThroughRawFileOffset
                == 0x00BD34CB
            && GameSocketParserDiagnosticCleanupXaddBranchProjection.JnzTargetRawFileOffset
                == 0x00BD34E2
            && 0x00BD34CB + 0x17
                == GameSocketParserDiagnosticCleanupXaddBranchProjection.JnzTargetRawFileOffset
            && suppliedBytes.Skip(4).Take(2).SequenceEqual(ParseBytes("75 17")));

        uint[] oldValues =
        {
            0,
            1,
            2,
            uint.MaxValue,
            0x80000000,
            0x7FFFFFFF
        };
        bool casesExact = true;
        bool sawJnzTaken = false;
        bool sawJnzNotTaken = false;
        foreach (uint oldValue in oldValues)
        {
            GameSocketParserDiagnosticCleanupXaddBranchPlan plan = Project(
                freshDWORDAtLockedRmw: oldValue);
            uint decremented = unchecked(oldValue - 1);
            GameSocketParserStoredByteSelectorGateFlags expectedFlags =
                ExpectedAddFlags(oldValue, decremented);
            bool taken = oldValue != 1;
            uint expectedTarget = taken ? 0x00BD34E2u : 0x00BD34CBu;
            casesExact &= plan.ParentBranchAdmitted
                && plan.ParentPlan.IsCleanCompletion
                && plan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34C9
                && plan.ParentPlan.ECXValue == oldValue
                && plan.Outcome == (taken
                    ? GameSocketParserDiagnosticCleanupXaddBranchOutcome.JnzTakenTo34E2
                    : GameSocketParserDiagnosticCleanupXaddBranchOutcome.JnzNotTakenTo34CB)
                && plan.ExactBoundedCompletion
                && plan.IsCleanCompletion
                && plan.StopBeforeRawFileOffset == expectedTarget
                && plan.BranchTargetRawFileOffset == expectedTarget
                && plan.BranchTargetCodeVirtualAddress
                    == GameSocketParserCountConsumptionStoresProjection
                        .RawFileOffsetToCodeVirtualAddress(expectedTarget)
                && plan.FallThroughRawFileOffset == 0x00BD34CB
                && plan.EAXValue == 0x7004
                && plan.EBXValue == uint.MaxValue
                && plan.ECXValue == oldValue
                && plan.ESIValue == 0x7000
                && plan.ResultESP == plan.ParentPlan.ResultESP
                && plan.FlagsAtStop == expectedFlags
                && plan.BranchFlagsPreserved
                && plan.RegistersPreserved
                && plan.JnzAttempted
                && plan.JnzReached
                && plan.JnzCommitted
                && plan.JnzTaken == taken
                && plan.Instructions.Length == 1
                && plan.Instructions[0] is {
                    InstructionRawFileOffset: 0x00BD34C9,
                    CodeVirtualAddress: var jnzCodeVa,
                    Encoding: "75 17",
                    StackPointer: var stackPointer,
                    EAX: 0x7004,
                    EBX: uint.MaxValue,
                    ECX: var ecx,
                    ESI: 0x7000,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    Taken: var branchTaken,
                    TargetRawFileOffset: var target,
                    FlagsBefore: var flagsBefore,
                    FlagsAfter: var flagsAfter
                }
                && jnzCodeVa
                    == GameSocketParserCountConsumptionStoresProjection
                        .RawFileOffsetToCodeVirtualAddress(0x00BD34C9)
                && stackPointer == plan.ResultESP
                && ecx == oldValue
                && branchTaken == taken
                && target == expectedTarget
                && flagsBefore == expectedFlags
                && flagsAfter == expectedFlags
                && flagsBefore?.Carry == expectedFlags.Carry
                && flagsBefore?.Parity == expectedFlags.Parity
                && flagsBefore?.AuxiliaryCarry == expectedFlags.AuxiliaryCarry
                && flagsBefore?.Zero == expectedFlags.Zero
                && flagsBefore?.Sign == expectedFlags.Sign
                && flagsBefore?.Overflow == expectedFlags.Overflow
                && plan.Reads.SequenceEqual(plan.ParentPlan.ParentPlan.Reads)
                && plan.MemoryEffects.SequenceEqual(plan.ParentPlan.ParentPlan.MemoryEffects)
                && plan.ParentPlan.MemoryRead is {
                    Value: var captured,
                    Attempted: true,
                    Captured: true,
                    ObservedAtLockedRmw: true
                }
                && captured == oldValue
                && plan.ParentPlan.MemoryWrite is {
                    OldValue: var writeOld,
                    NewValue: var writeNew,
                    Attempted: true,
                    Committed: true
                }
                && writeOld == oldValue
                && writeNew == decremented
                && plan.OrderedSemanticPrefixPreserved
                && !plan.BranchTargetInstructionExecuted
                && !plan.ParentInstructionsReplayed
                && !plan.NativeAtomicSemanticsModeled
                && !plan.NativeMemoryEffectsModeled
                && !plan.NativeABIOrProtocolSemanticsModeled
                && !plan.ParserExecutionModeled
                && !plan.RuntimeSampleExecuted
                && !plan.CleanupReleaseIdentityModeled;
            sawJnzTaken |= taken && plan.StopBeforeRawFileOffset == 0x00BD34E2;
            sawJnzNotTaken |= !taken && plan.StopBeforeRawFileOffset == 0x00BD34CB;
        }

        check("JNZ uses only the XADD ADD32 ZF result; all flags and registers otherwise persist",
            casesExact && sawJnzTaken && sawJnzNotTaken);

        GameSocketParserDiagnosticCleanupXaddBranchPlan accepted =
            Project(freshDWORDAtLockedRmw: 4308);
        GameSocketParserDiagnosticCleanupXaddBranchPlan replay =
            GameSocketParserDiagnosticCleanupXaddBranchProjection.Project(
                accepted.Capture);
        check("The immutable clean-34C9 capture reprojects without replaying or replacing its parent evidence",
            accepted.Capture.ParentCapture.FreshDWORDObservedAtLockedRmw == 4308
            && accepted.ParentExactBoundedCompletion
            && accepted.Capture == replay.Capture
            && accepted.ParentPlan.Capture == replay.ParentPlan.Capture
            && accepted.ParentPlan.Instructions.SequenceEqual(
                replay.ParentPlan.Instructions)
            && accepted.ParentPlan.MemoryRead == replay.ParentPlan.MemoryRead
            && accepted.ParentPlan.MemoryWrite == replay.ParentPlan.MemoryWrite
            && accepted.ParentPlan.ParentPlan.Reads.SequenceEqual(replay.ParentPlan.ParentPlan.Reads)
            && accepted.ParentPlan.ParentPlan.MemoryEffects.SequenceEqual(
                replay.ParentPlan.ParentPlan.MemoryEffects)
            && accepted.Instructions.SequenceEqual(replay.Instructions)
            && accepted.Outcome == replay.Outcome
            && accepted.BranchTargetRawFileOffset == replay.BranchTargetRawFileOffset
            && accepted.StopBeforeRawFileOffset == replay.StopBeforeRawFileOffset
            && accepted.StopBeforeRawFileOffset == 0x00BD34E2
            && accepted.JnzTaken
            && accepted.FlagsAtStop?.Zero == false
            && accepted.FlagsAtStop == replay.FlagsAtStop
            && accepted.OrderedSemanticPrefixPreserved
            && replay.OrderedSemanticPrefixPreserved
            && accepted.IsCleanCompletion
            && replay.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupXaddBranchPlan missingXaddRead =
            Project(freshDWORDAtLockedRmw: null);
        check("A missing XADD operand retains the parent's 34C5 stop and creates no branch",
            !missingXaddRead.ParentBranchAdmitted
            && missingXaddRead.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupXaddOutcome.MissingFreshDWORDAt34C5
            && missingXaddRead.ParentPlan.StopBeforeRawFileOffset == 0x00BD34C5
            && missingXaddRead.Instructions.IsEmpty
            && missingXaddRead.BranchTargetRawFileOffset is null
            && missingXaddRead.StopBeforeRawFileOffset == 0x00BD34C5
            && !missingXaddRead.ExactBoundedCompletion
            && !missingXaddRead.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupXaddBranchPlan zeroPointerAncestor =
            Project(freshDWORDAtLockedRmw: 7, esi: 0);
        check("The rejected zero-pointer ancestor preserves its 34E2 stop and leaves the XADD capture unused",
            !zeroPointerAncestor.ParentBranchAdmitted
            && zeroPointerAncestor.ParentPlan.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupZeroBranchOutcome.JzTakenTo34E2
            && zeroPointerAncestor.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && zeroPointerAncestor.ParentPlan.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupXaddCaptureState.SuppliedButUnused
            && zeroPointerAncestor.Instructions.IsEmpty
            && zeroPointerAncestor.BranchTargetRawFileOffset is null
            && zeroPointerAncestor.StopBeforeRawFileOffset == 0x00BD34E2
            && zeroPointerAncestor.ExactBoundedCompletion
            && !zeroPointerAncestor.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupXaddBranchPlan alternateAncestor =
            Project(freshDWORDAtLockedRmw: 7, comparisonValue: 2);
        check("The alternate 31F8 ancestor stays selected without entering the XADD branch",
            !alternateAncestor.ParentBranchAdmitted
            && alternateAncestor.ParentPlan.ParentPlan.ParentPlan.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticDestinationBranchGateOutcome.JaeTakenTo31F8
            && alternateAncestor.ParentPlan.StopBeforeRawFileOffset == 0x00BD31F8
            && alternateAncestor.ParentPlan.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupXaddCaptureState.SuppliedButUnused
            && alternateAncestor.Instructions.IsEmpty
            && alternateAncestor.BranchTargetRawFileOffset is null
            && alternateAncestor.StopBeforeRawFileOffset == 0x00BD31F8
            && alternateAncestor.ExactBoundedCompletion
            && !alternateAncestor.IsCleanCompletion);

        GameSocketParserDiagnosticCleanupXaddBranchPlan nonNormalAncestor =
            ProjectWithReturnObservation(normalReturnObserved: false);
        GameSocketParserDiagnosticCleanupXaddBranchPlan pendingAncestor =
            ProjectWithReturnObservation(normalReturnObserved: null);
        check("Non-normal and pending ancestors retain their own stops and do not fabricate JNZ",
            !nonNormalAncestor.ParentBranchAdmitted
            && !pendingAncestor.ParentBranchAdmitted
            && nonNormalAncestor.ParentPlan.ParentPlan.ParentPlan.ParentPlan
                .ParentPlan.ParentPlan.NormalReturnObserved == false
            && pendingAncestor.ParentPlan.ParentPlan.ParentPlan.ParentPlan
                .ParentPlan.ParentPlan.NormalReturnObserved is null
            && nonNormalAncestor.Instructions.IsEmpty
            && pendingAncestor.Instructions.IsEmpty
            && nonNormalAncestor.StopBeforeRawFileOffset
                == nonNormalAncestor.ParentPlan.StopBeforeRawFileOffset
            && pendingAncestor.StopBeforeRawFileOffset
                == pendingAncestor.ParentPlan.StopBeforeRawFileOffset
            && !nonNormalAncestor.IsCleanCompletion
            && !pendingAncestor.IsCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupXaddBranchPlan Project(
        uint? freshDWORDAtLockedRmw,
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
        return GameSocketParserDiagnosticCleanupXaddBranchProjection.Project(
            new(xaddCapture));
    }

    private static GameSocketParserDiagnosticCleanupXaddBranchPlan ProjectWithReturnObservation(
        bool? normalReturnObserved)
    {
        GameSocketParserDiagnosticCleanupStackPrefixCapture stackCapture =
            CreateParentCapture(0, 0x8FEC, 0x7000, 0x7000);
        GameSocketParserDiagnosticDestinationBranchGateCapture branchCapture =
            stackCapture.ParentCapture;
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
        stackCapture = stackCapture with { ParentCapture = branchCapture };
        GameSocketParserDiagnosticCleanupXaddBranchCapture capture =
            new(new(new(new(stackCapture)), 9));
        return GameSocketParserDiagnosticCleanupXaddBranchProjection.Project(capture);
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

    private static GameSocketParserStoredByteSelectorGateFlags ExpectedAddFlags(
        uint oldValue,
        uint result)
    {
        bool parity = (BitOperations.PopCount((byte)result) & 1) == 0;
        bool auxiliaryCarry = (oldValue & 0xF) != 0;
        bool overflow = (oldValue & 0x80000000) != 0
            && ((oldValue ^ result) & 0x80000000) != 0;
        return new(
            Carry: oldValue != 0,
            Parity: parity,
            AuxiliaryCarry: auxiliaryCarry,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: overflow);
    }

    private static byte[] ParseBytes(string encoding) =>
        encoding.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
