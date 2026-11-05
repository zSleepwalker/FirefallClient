using Firefall.Reconstruction.Startup;
using System.Numerics;

internal static class GameSocketParserDiagnosticCleanupFirstBranchFsRestoreChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection
                .CoordinatorProvidedBytes);
        check("The first taken-branch suffix maps MOV, FS:[0], and the stop before POP",
            GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection
                .BytesIndependentlyVerified
            && GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection
                .MoveRawFileOffset == 0x00BD34E2
            && GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection
                .MoveEncoding == "8B 4C 24 48"
            && GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection
                .FsStoreRawFileOffset == 0x00BD34E6
            && GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection
                .FsStoreEncoding == "64 89 0D 00 00 00 00"
            && GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection
                .StopBeforePopRawFileOffset == 0x00BD34ED
            && GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection
                .NextInstructionEncoding == "59"
            && suppliedBytes.Length == 12
            && suppliedBytes.SequenceEqual(
                GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection.WindowBytes)
            && suppliedBytes.Take(4).SequenceEqual(ParseBytes("8B 4C 24 48"))
            && suppliedBytes.Skip(4).Take(7).SequenceEqual(
                ParseBytes("64 89 0D 00 00 00 00"))
            && suppliedBytes.Skip(11).SequenceEqual(ParseBytes("59"))
            && CodeVA(0x00BD34E2) == 0x00FD40E2
            && CodeVA(0x00BD34E6) == 0x00FD40E6
            && CodeVA(0x00BD34ED) == 0x00FD40ED
            && !GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection
                .ExecutesNativeInstructions
            && !GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection
                .ExecutesNativeReads
            && !GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection
                .ExecutesNativeWrites
            && !GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection
                .ExecutesContinuationInstruction);

        uint[] oldValues =
        {
            0,
            1,
            2,
            uint.MaxValue,
            0x80000000,
            0x7FFFFFFF
        };
        bool branchCasesExact = true;
        bool sawTaken = false;
        bool sawZeroFallThrough = false;
        foreach (uint oldValue in oldValues)
        {
            uint decremented = unchecked(oldValue - 1);
            GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan plan =
                Project(oldValue, 0x7400, 0x12345678);
            GameSocketParserStoredByteSelectorGateFlags expectedFlags =
                ExpectedAddFlags(oldValue, decremented);
            bool taken = decremented != 0;
            if (taken)
            {
                branchCasesExact &= plan.ParentContinuationReached
                    && plan.SuffixAdmitted
                    && plan.ParentPlan.Outcome
                        == GameSocketParserDiagnosticCleanupXaddBranchOutcome
                            .JnzTakenTo34E2
                    && plan.ParentPlan.BranchTargetRawFileOffset == 0x00BD34E2
                    && plan.Outcome
                        == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreOutcome
                            .CompletedBeforePopAt34ED
                    && plan.ParentPlan.FlagsAtStop == expectedFlags
                    && plan.ParentPlan.BranchFlagsPreserved
                    && plan.FlagsAtStop == expectedFlags
                    && plan.FirstXaddAddFlagsPreserved
                    && !plan.CallAfterFirstBranchObserved
                    && !plan.ParentInstructionsReplayed
                    && plan.MemoryRead is {
                        InstructionRawFileOffset: 0x00BD34E2,
                        Address: 0x7448,
                        Width: 4,
                        Value: 0x12345678,
                        Attempted: true,
                        Captured: true,
                        NativeReadExecuted: false
                    }
                    && plan.RegisterMove is {
                        InstructionRawFileOffset: 0x00BD34E2,
                        DestinationRegister: "ECX",
                        SourceAddress: 0x7448,
                        Value: 0x12345678,
                        Attempted: true,
                        Reached: true,
                        Committed: true
                    }
                    && plan.MemoryWrite is {
                        InstructionRawFileOffset: 0x00BD34E6,
                        SegmentRegister: "FS",
                        SegmentOffset: 0,
                        Width: 4,
                        Value: 0x12345678,
                        SegmentRelative: true,
                        LinearAddress: null,
                        Attempted: true,
                        Committed: true,
                        NativeWriteExecuted: false
                    }
                    && plan.MemoryWrite.Value.Value == plan.ECXAfterMove
                    && plan.InstructionOrder.Select(step => step.RawFileOffset).SequenceEqual(
                        new uint[] { 0x00BD34E2, 0x00BD34E2, 0x00BD34E6 })
                    && plan.InstructionOrder.All(step =>
                        step.FlagsBefore == expectedFlags
                        && step.FlagsAfter == expectedFlags)
                    && plan.ContinuationBoundary is {
                        InstructionRawFileOffset: 0x00BD34ED,
                        NextInstructionEncoding: "59",
                        Reached: true,
                        NextInstructionExecuted: false
                    }
                    && plan.LastReachedRawFileOffset == 0x00BD34E6
                    && plan.StopBeforeRawFileOffset == 0x00BD34ED
                    && plan.StopBeforeCodeVirtualAddress == 0x00FD40ED
                    && plan.ExactBoundedCompletion
                    && plan.ExactCleanCompletion;
                sawTaken = true;
            }
            else
            {
                branchCasesExact &= plan.ParentPlan.FlagsAtStop == expectedFlags
                    && plan.ParentPlan.FlagsAtStop?.Zero == true
                    && plan.ParentPlan.Outcome
                        == GameSocketParserDiagnosticCleanupXaddBranchOutcome
                            .JnzNotTakenTo34CB
                    && plan.ParentPlan.StopBeforeRawFileOffset == 0x00BD34CB
                    && plan.Outcome
                        == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreOutcome
                            .BranchNotTakenTo34CB
                    && !plan.ParentContinuationReached
                    && !plan.SuffixAdmitted
                    && plan.MemoryRead is null
                    && plan.RegisterMove is null
                    && plan.MemoryWrite is null
                    && plan.FreshESPCaptureState
                        == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreCaptureState
                            .SuppliedButUnused
                    && plan.FreshDWORDCaptureState
                        == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreCaptureState
                            .SuppliedButUnused
                    && plan.StopBeforeRawFileOffset == 0x00BD34CB
                    && !plan.ExactCleanCompletion;
                sawZeroFallThrough = true;
            }
        }

        check("Only nonzero first-XADD results enter the clean E2 suffix; ADD flags survive",
            branchCasesExact && sawTaken && sawZeroFallThrough);

        GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan wrappedAddress =
            Project(2, 0xFFFFFFB8, 0);
        check("The fresh ESP is independent and its wrapped plus-48 read supplies the FS DWORD",
            wrappedAddress.Capture.FreshESPAt34E2 == 0xFFFFFFB8
            && wrappedAddress.Capture.FreshDWORDAtWrappedESPPlus48 == 0
            && wrappedAddress.ParentPlan.ParentPlan.MemoryRead?.Value == 2
            && wrappedAddress.MemoryRead is {
                Address: 0,
                Value: 0,
                Attempted: true,
                Captured: true,
                NativeReadExecuted: false
            }
            && wrappedAddress.RegisterMove is {
                SourceAddress: 0,
                Value: 0,
                Committed: true
            }
            && wrappedAddress.MemoryWrite is {
                SegmentRegister: "FS",
                SegmentOffset: 0,
                Width: 4,
                Value: 0,
                SegmentRelative: true,
                LinearAddress: null,
                Committed: true
            }
            && wrappedAddress.ExactCleanCompletion
            && !wrappedAddress.LinearFSAddressModeled
            && !wrappedAddress.PointerValidityModeled
            && !wrappedAddress.NativeABIOrProtocolSemanticsModeled);

        GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan missingESP =
            Project(2, null, 0x12345678);
        check("Missing fresh ESP stops at E2 without a stack read or FS write",
            missingESP.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreOutcome
                    .MissingFreshESPAt34E2
            && missingESP.ParentContinuationReached
            && missingESP.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && missingESP.FreshESPCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreCaptureState.Missing
            && missingESP.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreCaptureState
                    .SuppliedButUnused
            && missingESP.MemoryRead is null
            && missingESP.RegisterMove is null
            && missingESP.MemoryWrite is null
            && missingESP.FlagsAtStop == missingESP.ParentPlan.FlagsAtStop
            && missingESP.FirstXaddAddFlagsPreserved
            && missingESP.LastReachedRawFileOffset == 0x00BD34C9
            && missingESP.StopBeforeRawFileOffset == 0x00BD34E2
            && !missingESP.ChildBoundedCompletionBoundaryReached
            && !missingESP.ExactBoundedCompletion
            && !missingESP.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan missingDWORD =
            Project(2, 0x7000, null);
        check("Missing fresh stack DWORD reaches but cannot complete the E2 load or write FS",
            missingDWORD.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreOutcome
                    .MissingFreshDWORDAt34E2
            && missingDWORD.FreshESPAt34E2 == 0x7000
            && missingDWORD.MemoryRead is {
                InstructionRawFileOffset: 0x00BD34E2,
                Address: 0x7048,
                Width: 4,
                Value: null,
                Attempted: true,
                Captured: false,
                NativeReadExecuted: false
            }
            && missingDWORD.RegisterMove is {
                InstructionRawFileOffset: 0x00BD34E2,
                DestinationRegister: "ECX",
                Attempted: true,
                Reached: true,
                Committed: false
            }
            && missingDWORD.MemoryWrite is null
            && missingDWORD.FlagsAtStop == missingDWORD.ParentPlan.FlagsAtStop
            && missingDWORD.FirstXaddAddFlagsPreserved
            && missingDWORD.LastReachedRawFileOffset == 0x00BD34E2
            && missingDWORD.StopBeforeRawFileOffset == 0x00BD34E2
            && !missingDWORD.ChildBoundedCompletionBoundaryReached
            && !missingDWORD.ExactBoundedCompletion
            && !missingDWORD.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan zeroResult =
            Project(1, 0x7100, 0xAABBCCDD);
        check("A zero-result first XADD branches to CB and leaves both fresh captures unused",
            zeroResult.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupXaddBranchOutcome.JnzNotTakenTo34CB
            && zeroResult.ParentPlan.FlagsAtStop?.Zero == true
            && zeroResult.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreOutcome
                    .BranchNotTakenTo34CB
            && !zeroResult.ParentContinuationReached
            && !zeroResult.SuffixAdmitted
            && zeroResult.MemoryRead is null
            && zeroResult.RegisterMove is null
            && zeroResult.MemoryWrite is null
            && zeroResult.FreshESPCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreCaptureState
                    .SuppliedButUnused
            && zeroResult.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreCaptureState
                    .SuppliedButUnused
            && zeroResult.StopBeforeRawFileOffset == 0x00BD34CB
            && zeroResult.ExactBoundedCompletion
                == zeroResult.ParentPlan.ExactBoundedCompletion
            && !zeroResult.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan incompleteParent =
            Project(null, 0x7200, 0x11223344);
        check("An incomplete XADD parent preserves its own stop and rejects the FS suffix",
            incompleteParent.Outcome
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreOutcome
                    .ParentContinuationNotAdmitted
            && !incompleteParent.ParentContinuationReached
            && !incompleteParent.SuffixAdmitted
            && incompleteParent.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupXaddBranchOutcome.ParentBranchNotAdmitted
            && incompleteParent.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupXaddOutcome.MissingFreshDWORDAt34C5
            && incompleteParent.ParentPlan.StopBeforeRawFileOffset == 0x00BD34C5
            && incompleteParent.FreshESPCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreCaptureState
                    .SuppliedButUnused
            && incompleteParent.FreshDWORDCaptureState
                == GameSocketParserDiagnosticCleanupFirstBranchFsRestoreCaptureState
                    .SuppliedButUnused
            && incompleteParent.MemoryRead is null
            && incompleteParent.RegisterMove is null
            && incompleteParent.MemoryWrite is null
            && incompleteParent.StopBeforeRawFileOffset == 0x00BD34C5
            && !incompleteParent.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan successful =
            Project(2, 0x7400, 0x12345678);
        GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan reprojection =
            GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection.Project(
                successful.Capture);
        check("The clean first-branch restoration reprojects from immutable independent captures",
            reprojection.Capture == successful.Capture
            && reprojection.Outcome == successful.Outcome
            && reprojection.ParentPlan.Capture == successful.ParentPlan.Capture
            && reprojection.ParentPlan.Instructions.SequenceEqual(
                successful.ParentPlan.Instructions)
            && reprojection.MemoryRead == successful.MemoryRead
            && reprojection.RegisterMove == successful.RegisterMove
            && reprojection.MemoryWrite == successful.MemoryWrite
            && reprojection.InstructionOrder.SequenceEqual(successful.InstructionOrder)
            && reprojection.FreshESPAt34E2 == successful.FreshESPAt34E2
            && reprojection.FreshDWORDAtWrappedESPPlus48
                == successful.FreshDWORDAtWrappedESPPlus48
            && reprojection.FlagsAtStop == successful.FlagsAtStop
            && reprojection.LastReachedRawFileOffset == successful.LastReachedRawFileOffset
            && reprojection.StopBeforeRawFileOffset == successful.StopBeforeRawFileOffset
            && reprojection.OrderedSemanticPrefixPreserved
            && reprojection.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupFirstBranchFsRestorePlan Project(
        uint? firstRmwDWORD,
        uint? freshESP,
        uint? freshDWORD) =>
        GameSocketParserDiagnosticCleanupFirstBranchFsRestoreProjection.Project(
            new(Capture(firstRmwDWORD), freshESP, freshDWORD));

    private static GameSocketParserDiagnosticCleanupXaddBranchCapture Capture(
        uint? firstRmwDWORD)
    {
        GameSocketParserDiagnosticCleanupStackPrefixCapture stackCapture =
            CreateParentCapture();
        GameSocketParserDiagnosticCleanupZeroBranchCapture zeroBranchCapture =
            new(stackCapture);
        GameSocketParserDiagnosticCleanupRegisterPreparationCapture registerCapture =
            new(zeroBranchCapture);
        GameSocketParserDiagnosticCleanupXaddCapture xaddCapture =
            new(registerCapture, firstRmwDWORD);
        return new(xaddCapture);
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

    private static uint CodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection
            .RawFileOffsetToCodeVirtualAddress(rawFileOffset);

    private static byte[] ParseBytes(string encoding) =>
        encoding.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
