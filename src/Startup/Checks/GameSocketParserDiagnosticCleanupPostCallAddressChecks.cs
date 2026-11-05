using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticCleanupPostCallAddressChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] suppliedBytes = ParseBytes(
            GameSocketParserDiagnosticCleanupPostCallAddressProjection.CoordinatorProvidedBytes);
        check("The supplied window anchors only LEA EAX,[ESI+8] and its exclusive D4 stop",
            !GameSocketParserDiagnosticCleanupPostCallAddressProjection
                .BytesIndependentlyVerified
            && suppliedBytes.Length == 24
            && GameSocketParserDiagnosticCleanupPostCallAddressProjection.WindowBytes
                .SequenceEqual(suppliedBytes)
            && GameSocketParserDiagnosticCleanupPostCallAddressProjection
                .LeaRawFileOffset == 0x00BD34D1
            && GameSocketParserDiagnosticCleanupPostCallAddressProjection
                .LeaEncoding == "8D 46 08"
            && suppliedBytes.Take(3).SequenceEqual(ParseBytes("8D 46 08"))
            && GameSocketParserDiagnosticCleanupPostCallAddressProjection
                .ExclusiveStopRawFileOffset == 0x00BD34D4
            && GameSocketParserDiagnosticCleanupPostCallAddressProjection
                .FollowingLockedRmwRawFileOffset == 0x00BD34D4
            && GameSocketParserDiagnosticCleanupPostCallAddressProjection
                .FollowingLockedRmwEncoding == "F0 0F C1 18"
            && suppliedBytes.Skip(3).Take(4).SequenceEqual(ParseBytes("F0 0F C1 18")));

        GameSocketParserDiagnosticCleanupPostCallAddressPlan missingESI =
            Project(freshPostCallESI: null);
        GameSocketParserDiagnosticCleanupPostCallAddressPlan zeroESI =
            Project(freshPostCallESI: 0);
        GameSocketParserDiagnosticCleanupPostCallAddressPlan maximumESI =
            Project(freshPostCallESI: uint.MaxValue);
        check("Fresh missing, zero, and maximum ESI captures are distinct and DWORD addition wraps",
            missingESI.ParentExactCompletion
            && missingESI.ParentExactBoundedCompletion
            && missingESI.ParentExactCleanCompletion
            && missingESI.ParentPlan.FlagsAtStop is null
            && missingESI.ParentAdmitted
            && missingESI.Outcome
                == GameSocketParserDiagnosticCleanupPostCallAddressOutcome
                    .MissingFreshPostCallESIAt34D1
            && missingESI.FreshPostCallESICaptureState
                == GameSocketParserDiagnosticCleanupPostCallAddressCaptureState.Missing
            && missingESI.Instructions.Length == 1
            && missingESI.Instructions[0] is {
                InstructionRawFileOffset: 0x00BD34D1,
                Encoding: "8D 46 08",
                SourceRegister: "ESI",
                SourceValue: null,
                DestinationRegister: "EAX",
                DestinationValue: null,
                Attempted: false,
                Reached: true,
                Committed: false
            }
            && missingESI.LastReachedRawFileOffset == 0x00BD34D1
            && missingESI.StopBeforeRawFileOffset == 0x00BD34D1
            && !missingESI.ChildBoundedCompletionBoundaryReached
            && !missingESI.ExactBoundedCompletion
            && !missingESI.ExactCleanCompletion
            && zeroESI.Capture.FreshPostCallESI == 0
            && zeroESI.ESIValue == 0
            && zeroESI.EAXValue == 8
            && zeroESI.ExactCleanCompletion
            && maximumESI.Capture.FreshPostCallESI == uint.MaxValue
            && maximumESI.ESIValue == uint.MaxValue
            && maximumESI.EAXValue == 7
            && maximumESI.Instructions[0].DestinationValue == 7
            && maximumESI.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupPostCallAddressPlan independentESI =
            Project(freshPostCallESI: 0x1234, preCallESI: 0x7000);
        check("The LEA uses an independently captured post-call ESI, not pre-call ESI",
            independentESI.ParentPlan.PreCallESI == 0x7000
            && independentESI.Capture.FreshPostCallESI == 0x1234
            && independentESI.Instructions[0] is {
                SourceValue: 0x1234,
                DestinationValue: 0x123C,
                FlagsBefore: null,
                FlagsAfter: null
            }
            && independentESI.ESIValue == 0x1234
            && independentESI.EAXValue == 0x123C
            && independentESI.PostCallEBX is null
            && independentESI.PostCallECX is null
            && independentESI.PostCallESP is null
            && !independentESI.PostCallEBXCaptured
            && !independentESI.PostCallECXCaptured
            && !independentESI.PostCallESPCaptured
            && independentESI.ExactCleanCompletion);

        GameSocketParserDiagnosticCleanupPostCallAddressPlan unusedAfterOtherParent =
            Project(
                freshPostCallESI: 0xA100,
                freshDWORDAtLockedRmw: 0,
                freshVtableSlot: 0x22222222,
                returnedNormally: true);
        GameSocketParserDiagnosticCleanupPostCallAddressPlan pendingReturn =
            Project(freshPostCallESI: 0xA200, returnedNormally: null);
        GameSocketParserDiagnosticCleanupPostCallAddressPlan falseReturn =
            Project(freshPostCallESI: 0xA300, returnedNormally: false);
        GameSocketParserDiagnosticCleanupPostCallAddressPlan missingVptr =
            Project(freshPostCallESI: 0xA400, freshVptr: null);
        check("Rejected normal, pending, false, and missing-vptr parents leave fresh ESI unused",
            unusedAfterOtherParent.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .ParentBranchNotAdmitted
            && unusedAfterOtherParent.ParentPlan.StopBeforeRawFileOffset == 0x00BD34E2
            && unusedAfterOtherParent.ParentPlan.ExactBoundedCompletion
            && !unusedAfterOtherParent.ParentPlan.ExactChildCompletion
            && unusedAfterOtherParent.FreshPostCallESICaptureState
                == GameSocketParserDiagnosticCleanupPostCallAddressCaptureState
                    .SuppliedButUnused
            && unusedAfterOtherParent.Capture.FreshPostCallESI == 0xA100
            && unusedAfterOtherParent.Instructions.IsEmpty
            && unusedAfterOtherParent.EAXValue is null
            && unusedAfterOtherParent.ESIValue is null
            && !unusedAfterOtherParent.ParentAdmitted
            && unusedAfterOtherParent.StopBeforeRawFileOffset == 0x00BD34E2
            && unusedAfterOtherParent.ParentExactBoundedCompletion
            && !unusedAfterOtherParent.ChildBoundedCompletionBoundaryReached
            && pendingReturn.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .MissingOpaqueCallReturnObservation
            && pendingReturn.ParentPlan.StopBeforeRawFileOffset == 0x00BD34CF
            && pendingReturn.ParentPlan.FlagsAtStop
                == pendingReturn.ParentPlan.ParentPlan.FlagsAtStop
            && pendingReturn.FreshPostCallESICaptureState
                == GameSocketParserDiagnosticCleanupPostCallAddressCaptureState
                    .SuppliedButUnused
            && pendingReturn.Instructions.IsEmpty
            && pendingReturn.StopBeforeRawFileOffset == 0x00BD34CF
            && !pendingReturn.ParentExactBoundedCompletion
            && falseReturn.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .OpaqueCallDidNotReturnNormally
            && falseReturn.ParentPlan.StopBeforeRawFileOffset == 0x00BD34CF
            && falseReturn.FreshPostCallESICaptureState
                == GameSocketParserDiagnosticCleanupPostCallAddressCaptureState
                    .SuppliedButUnused
            && falseReturn.Instructions.IsEmpty
            && falseReturn.StopBeforeRawFileOffset == 0x00BD34CF
            && missingVptr.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallReturnOutcome
                    .ParentBranchNotAdmitted
            && missingVptr.ParentPlan.ParentPlan.Outcome
                == GameSocketParserDiagnosticCleanupVirtualCallPreparationOutcome
                    .MissingFreshVptrDWORDAt34CB
            && missingVptr.ParentPlan.StopBeforeRawFileOffset == 0x00BD34CB
            && missingVptr.FreshPostCallESICaptureState
                == GameSocketParserDiagnosticCleanupPostCallAddressCaptureState
                    .SuppliedButUnused
            && missingVptr.Instructions.IsEmpty
            && missingVptr.StopBeforeRawFileOffset == 0x00BD34CB
            && !missingVptr.ParentAdmitted);

        GameSocketParserDiagnosticCleanupPostCallAddressPlan clean =
            Project(freshPostCallESI: 0x8123);
        GameSocketParserDiagnosticCleanupPostCallAddressPlan replay =
            GameSocketParserDiagnosticCleanupPostCallAddressProjection.Project(clean.Capture);
        check("Immutable input reprojects the same LEA without changing parent evidence",
            replay.Capture == clean.Capture
            && replay.Outcome == clean.Outcome
            && replay.FreshPostCallESICaptureState == clean.FreshPostCallESICaptureState
            && replay.Instructions.SequenceEqual(clean.Instructions)
            && replay.EAXValue == clean.EAXValue
            && replay.ESIValue == clean.ESIValue
            && replay.FlagsAtStop == clean.FlagsAtStop
            && replay.LastReachedRawFileOffset == clean.LastReachedRawFileOffset
            && replay.StopBeforeRawFileOffset == clean.StopBeforeRawFileOffset
            && replay.ParentPlan.InstructionOrder.SequenceEqual(
                clean.ParentPlan.InstructionOrder)
            && replay.ParentPlan.Reads.SequenceEqual(clean.ParentPlan.Reads)
            && replay.ParentPlan.MemoryEffects.SequenceEqual(
                clean.ParentPlan.MemoryEffects)
            && replay.Reads.SequenceEqual(clean.Reads)
            && replay.MemoryEffects.SequenceEqual(clean.MemoryEffects)
            && replay.ParentExactCompletion == clean.ParentExactCompletion
            && replay.ParentExactBoundedCompletion == clean.ParentExactBoundedCompletion
            && replay.ChildBoundedCompletionBoundaryReached
                == clean.ChildBoundedCompletionBoundaryReached
            && replay.OrderedSemanticPrefixPreserved
            && replay.ExactCleanCompletion);
    }

    private static GameSocketParserDiagnosticCleanupPostCallAddressPlan Project(
        uint? freshPostCallESI,
        uint? freshVptr = 0x7000,
        uint? freshVtableSlot = 0x12345678,
        bool? returnedNormally = true,
        uint? freshDWORDAtLockedRmw = 1,
        uint preCallESI = 0x7000)
    {
        GameSocketParserDiagnosticCleanupStackPrefixCapture stackCapture =
            CreateParentCapture(preCallESI);
        GameSocketParserDiagnosticCleanupZeroBranchCapture zeroBranchCapture =
            new(stackCapture);
        GameSocketParserDiagnosticCleanupRegisterPreparationCapture registerCapture =
            new(zeroBranchCapture);
        GameSocketParserDiagnosticCleanupXaddCapture xaddCapture =
            new(registerCapture, freshDWORDAtLockedRmw);
        GameSocketParserDiagnosticCleanupXaddBranchCapture branchCapture =
            new(xaddCapture);
        GameSocketParserDiagnosticCleanupVirtualCallPreparationCapture preparationCapture =
            new(branchCapture, freshVptr);
        GameSocketParserDiagnosticCleanupVirtualCallReturnCapture returnCapture =
            new(preparationCapture, freshVtableSlot, returnedNormally);
        return GameSocketParserDiagnosticCleanupPostCallAddressProjection.Project(
            new(returnCapture, freshPostCallESI));
    }

    private static GameSocketParserDiagnosticCleanupStackPrefixCapture CreateParentCapture(
        uint preCallESI)
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
            preCallESI);
    }

    private static byte[] ParseBytes(string encoding) =>
        encoding.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
