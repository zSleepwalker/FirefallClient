using Firefall.Reconstruction.Startup;

internal static class GameSocketParserSelectorDiagnosticReturnCleanupChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The diagnostic call and stack cleanup retain their exact instruction boundary",
            GameSocketParserSelectorDiagnosticReturnCleanupProjection.CallRawFileOffset
                == 0x00BD3281
            && GameSocketParserSelectorDiagnosticReturnCleanupProjection.CallReturnRawFileOffset
                == 0x00BD3286
            && GameSocketParserSelectorDiagnosticReturnCleanupProjection.AddESP14RawFileOffset
                == 0x00BD3286
            && GameSocketParserSelectorDiagnosticReturnCleanupProjection.AfterAddRawFileOffset
                == 0x00BD3289
            && GameSocketParserSelectorDiagnosticReturnCleanupProjection.CallTargetRawFileOffset
                == 0x00BE2F50
            && GameSocketParserSelectorDiagnosticReturnCleanupProjection.CallTargetCodeVirtualAddress
                == 0x00FE3B50
            && GameSocketParserSelectorDiagnosticReturnCleanupProjection.CallReturnRawFileOffset
                + GameSocketParserSelectorDiagnosticReturnCleanupProjection.CallDisplacement
                == GameSocketParserSelectorDiagnosticReturnCleanupProjection.CallTargetRawFileOffset
            && GameSocketParserSelectorDiagnosticReturnCleanupProjection.CallEncoding
                == "E8 CA FC 00 00"
            && GameSocketParserSelectorDiagnosticReturnCleanupProjection.AddEncoding
                == "83 C4 14");

        uint[] postCallESPs = {
            0,
            1,
            0xFFFFFFEB,
            0xFFFFFFEC,
            0xFFFFFFED,
            uint.MaxValue,
            0x7FFFFFEB,
            0x7FFFFFEC,
            0x7FFFFFED,
            0x80000000,
            0x80000001,
            0x0000000B,
            0x0000000C
        };
        bool normalReturnsAndFlagsExact = true;
        bool sawCarry = false;
        bool sawOverflow = false;
        bool sawAuxiliaryCarry = false;
        bool sawZero = false;
        bool sawSign = false;
        bool sawEvenParity = false;
        bool sawOddParity = false;
        foreach (byte selector in new byte[] { 0, 3 })
        {
            foreach (uint postCallESP in postCallESPs)
            {
                GameSocketParserSelectorDiagnosticReturnCleanupPlan plan =
                    Project(
                        selector,
                        normalReturnObserved: true,
                        postCallESP: postCallESP);
                uint expectedESP = unchecked(postCallESP + 0x14);
                GameSocketParserStoredByteSelectorGateFlags expectedFlags =
                    ExpectedAddFlags32(postCallESP, 0x14);
                normalReturnsAndFlagsExact &= plan.ParentHandoffAdmitted
                    && plan.ParentPreparationCompleted
                    && plan.ParentPlan.ParentPlan.InheritedEBX == selector
                    && plan.ParentESP == 0x2FEC
                    && plan.FreshPostCallESP == postCallESP
                    && plan.PostCallESPCaptureState
                        == GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.Captured
                    && plan.NormalReturnObserved == true
                    && plan.ResultNormalReturnObserved == true
                    && plan.ResultESP == expectedESP
                    && plan.FlagsAtStop == expectedFlags
                    && plan.Instructions.Length == 2
                    && plan.Instructions[0] is {
                        Kind: GameSocketParserSelectorDiagnosticReturnCleanupInstructionKind
                            .OpaqueDirectCallAt3281,
                        InstructionRawFileOffset: 0x00BD3281,
                        Attempted: true,
                        Reached: true,
                        Committed: true,
                        NormalReturnObserved: true,
                        StackPointerAfter: null,
                        FlagsAfter: null
                    }
                    && plan.Instructions[1] is {
                        Kind: GameSocketParserSelectorDiagnosticReturnCleanupInstructionKind
                            .AddESP14At3286,
                        InstructionRawFileOffset: 0x00BD3286,
                        Encoding: "83 C4 14",
                        Attempted: true,
                        Reached: true,
                        Committed: true,
                        NormalReturnObserved: true,
                        StackPointerBefore: var before,
                        StackPointerAfter: var after,
                        FlagsBefore: null,
                        FlagsAfter: var afterFlags
                    }
                    && before == postCallESP
                    && after == expectedESP
                    && afterFlags == expectedFlags
                    && plan.Outcome
                        == GameSocketParserSelectorDiagnosticReturnCleanupOutcome
                            .BoundedBeforeNextInstruction
                    && plan.StopBeforeRawFileOffset == 0x00BD3289
                    && plan.ReachedBoundedEndpoint
                    && plan.ExactBoundedCompletion
                    && plan.IsCleanCompletion
                    && plan.OrderedMemoryPrefixPreserved
                    && !plan.CallTargetEntryObserved
                    && !plan.OpaqueCallEffectsModeled
                    && !plan.NativeCallStackWriteModeled
                    && !plan.NativeMemoryEffectsModeled
                    && !plan.NativeABIOrProtocolSemanticsModeled
                    && !plan.RuntimeSampleExecuted;
                sawCarry |= expectedFlags.Carry == true;
                sawOverflow |= expectedFlags.Overflow == true;
                sawAuxiliaryCarry |= expectedFlags.AuxiliaryCarry == true;
                sawZero |= expectedFlags.Zero == true;
                sawSign |= expectedFlags.Sign == true;
                sawEvenParity |= expectedFlags.Parity == true;
                sawOddParity |= expectedFlags.Parity == false;
            }
        }

        check("Selectors 0/3 use a fresh post-call ESP and exact 32-bit ADD flags",
            normalReturnsAndFlagsExact
            && sawCarry
            && sawOverflow
            && sawAuxiliaryCarry
            && sawZero
            && sawSign
            && sawEvenParity
            && sawOddParity
            && ExpectedAddFlags32(0, 0x14).Parity == true
            && ExpectedAddFlags32(1, 0x14).Parity == false);

        GameSocketParserSelectorDiagnosticReturnCleanupPlan distinctESP =
            Project(selector: 3, normalReturnObserved: true, postCallESP: 0xA000);
        check("A fresh returned ESP is not derived from the pre-call five-push ESP",
            distinctESP.ParentESP == 0x2FEC
            && distinctESP.Instructions[0].StackPointerBefore == 0x2FEC
            && distinctESP.Instructions[1].StackPointerBefore == 0xA000
            && distinctESP.ResultESP == 0xA014);

        GameSocketParserSelectorDiagnosticReturnCleanupPlan pending =
            Project(selector: 0, normalReturnObserved: null, postCallESP: null);
        GameSocketParserSelectorDiagnosticReturnCleanupPlan pendingWithUnusedESP =
            Project(selector: 3, normalReturnObserved: null, postCallESP: 0);
        GameSocketParserSelectorDiagnosticReturnCleanupPlan nonNormal =
            Project(selector: 0, normalReturnObserved: false, postCallESP: null);
        GameSocketParserSelectorDiagnosticReturnCleanupPlan nonNormalWithUnusedESP =
            Project(selector: 3, normalReturnObserved: false, postCallESP: uint.MaxValue);
        check("Pending and non-normal call observations stop at 3281 without fabricating cleanup",
            pending.ParentHandoffAdmitted
            && pending.Outcome
                == GameSocketParserSelectorDiagnosticReturnCleanupOutcome.CallPending
            && pending.NormalReturnObserved is null
            && pending.Instructions is [{ Attempted: true, NormalReturnObserved: null }]
            && pending.ResultESP is null
            && pending.FlagsAtStop == pending.ParentPlan.FlagsAtStop
            && pending.Instructions[0].FlagsAfter == pending.ParentPlan.FlagsAtStop
            && pending.StopBeforeRawFileOffset == 0x00BD3281
            && pending.PostCallESPCaptureState
                == GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.NotReached
            && pending.IsCleanCompletion == false
            && pendingWithUnusedESP.PostCallESPCaptureState
                == GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.SuppliedButUnused
            && pendingWithUnusedESP.UnusedCaptures is [
                GameSocketParserSelectorDiagnosticReturnCleanupCaptureKind.FreshPostCallESP]
            && nonNormal.Outcome
                == GameSocketParserSelectorDiagnosticReturnCleanupOutcome.NonNormalReturn
            && nonNormal.NormalReturnObserved == false
            && nonNormal.Instructions is [{ Attempted: true, NormalReturnObserved: false }]
            && nonNormal.ResultESP is null
            && nonNormal.FlagsAtStop == nonNormal.ParentPlan.FlagsAtStop
            && nonNormal.Instructions[0].FlagsAfter == nonNormal.ParentPlan.FlagsAtStop
            && nonNormal.StopBeforeRawFileOffset == 0x00BD3281
            && nonNormal.IsCleanCompletion == false
            && nonNormalWithUnusedESP.PostCallESPCaptureState
                == GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.SuppliedButUnused
            && nonNormalWithUnusedESP.UnusedCaptures.Length == 1
            && nonNormalWithUnusedESP.ResultESP is null);

        GameSocketParserSelectorDiagnosticReturnCleanupPlan missingFreshESP =
            Project(selector: 0, normalReturnObserved: true, postCallESP: null);
        check("Normal return without fresh ESP is parent-complete but stops before ADD at 3286",
            missingFreshESP.ParentHandoffAdmitted
            && missingFreshESP.ParentPreparationCompleted
            && missingFreshESP.ParentExactBoundedCompletion
            && missingFreshESP.Outcome
                == GameSocketParserSelectorDiagnosticReturnCleanupOutcome.MissingFreshPostCallESP
            && missingFreshESP.NormalReturnObserved == true
            && missingFreshESP.PostCallESPCaptureState
                == GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.Missing
            && missingFreshESP.Instructions.Length == 2
            && missingFreshESP.Instructions[1] is {
                InstructionRawFileOffset: 0x00BD3286,
                Attempted: true,
                Reached: true,
                Committed: false,
                StackPointerBefore: null,
                StackPointerAfter: null,
                FlagsBefore: null,
                FlagsAfter: null
            }
            && missingFreshESP.ResultESP is null
            && missingFreshESP.FlagsAtStop is null
            && missingFreshESP.StopBeforeRawFileOffset == 0x00BD3286
            && !missingFreshESP.ExactBoundedCompletion
            && !missingFreshESP.IsCleanCompletion);

        GameSocketParserSelectorDiagnosticReturnCleanupPlan wrapped =
            Project(selector: 3, normalReturnObserved: true, postCallESP: uint.MaxValue - 7);
        check("Captured ESP zero, maximum, and wrapping values use 32-bit ADD semantics",
            Project(selector: 0, normalReturnObserved: true, postCallESP: 0).ResultESP == 0x14
            && Project(
                selector: 0,
                normalReturnObserved: true,
                postCallESP: uint.MaxValue).ResultESP == 0x13
            && wrapped.FreshPostCallESP == 0xFFFFFFF8
            && wrapped.ResultESP == 0x0000000C
            && wrapped.FlagsAtStop == ExpectedAddFlags32(0xFFFFFFF8, 0x14));

        GameSocketParserSelectorDiagnosticReturnCleanupPlan missingESI =
            Project(
                selector: 0,
                normalReturnObserved: true,
                postCallESP: 0x9000,
                freshESI: null,
                receiverValue: 0x12345678);
        GameSocketParserSelectorDiagnosticReturnCleanupPlan rejectedAncestor =
            Project(
                selector: 0,
                normalReturnObserved: true,
                postCallESP: 0x9000,
                callerCount: 1);
        check("Parent rejection and missing fresh ESI forward the actual parent bound",
            !missingESI.ParentHandoffAdmitted
            && missingESI.ParentPlan.ParentHandoffAdmitted
            && missingESI.ParentPlan.Outcome
                == GameSocketParserSelectorDiagnosticPreparationOutcome.MissingFreshESIAt327B
            && missingESI.StopBeforeRawFileOffset
                == missingESI.ParentPlan.StopBeforeRawFileOffset
            && missingESI.PostCallESPCaptureState
                == GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.SuppliedButUnused
            && missingESI.ResultNormalReturnObserved is null
            && !missingESI.CallReturnedNormally
            && missingESI.NormalReturnObservationSuppliedButUnused
            && missingESI.Instructions.IsEmpty
            && missingESI.UnusedCaptures.Length == 1
            && !missingESI.ParentPreparationCompleted
            && !missingESI.IsCleanCompletion
            && !rejectedAncestor.ParentHandoffAdmitted
            && rejectedAncestor.ParentPlan.StopBeforeRawFileOffset
                == GameSocketParserCallerCountGateProjection.BranchTargetRawFileOffset
            && rejectedAncestor.StopBeforeRawFileOffset
                == rejectedAncestor.ParentPlan.StopBeforeRawFileOffset
            && rejectedAncestor.Instructions.IsEmpty
            && rejectedAncestor.PostCallESPCaptureState
                == GameSocketParserSelectorDiagnosticReturnCleanupCaptureState.SuppliedButUnused
            && rejectedAncestor.ParentExactBoundedCompletion
                == rejectedAncestor.ParentPlan.ExactBoundedCompletion
            && !rejectedAncestor.IsCleanCompletion);

        GameSocketParserSelectorDiagnosticReturnCleanupPlan clean =
            Project(selector: 3, normalReturnObserved: true, postCallESP: 0xBEEF);
        GameSocketParserSelectorDiagnosticReturnCleanupPlan reprojected =
            GameSocketParserSelectorDiagnosticReturnCleanupProjection.Project(clean.Capture);
        check("Immutable capture reprojection matches scalar, instruction, and ordered-effect values",
            reprojected.Outcome == clean.Outcome
            && reprojected.ParentHandoffAdmitted == clean.ParentHandoffAdmitted
            && reprojected.NormalReturnObserved == clean.NormalReturnObserved
            && reprojected.ResultNormalReturnObserved == clean.ResultNormalReturnObserved
            && reprojected.ParentESP == clean.ParentESP
            && reprojected.FreshPostCallESP == clean.FreshPostCallESP
            && reprojected.ResultESP == clean.ResultESP
            && reprojected.FlagsAtStop == clean.FlagsAtStop
            && reprojected.StopBeforeRawFileOffset == clean.StopBeforeRawFileOffset
            && reprojected.PostCallESPCaptureState == clean.PostCallESPCaptureState
            && reprojected.Instructions.SequenceEqual(clean.Instructions)
            && reprojected.MemoryEffects.SequenceEqual(clean.MemoryEffects)
            && reprojected.UnusedCaptures.SequenceEqual(clean.UnusedCaptures)
            && reprojected.OrderedMemoryPrefixPreserved);
    }

    private static GameSocketParserSelectorDiagnosticReturnCleanupPlan Project(
        byte selector,
        bool? normalReturnObserved,
        uint? postCallESP,
        uint? freshESI = 0x7000,
        uint? receiverValue = 0xCAFEBABE,
        uint callerCount = 100)
    {
        GameSocketParserSelectorDiagnosticPreparationCapture parentCapture =
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
                                    (byte)(selector << 6)),
                                FreshSecondHeaderByte: 2),
                            0x7000,
                            callerCount),
                        FreshStackDwordAtESPPlus14: 0x1000,
                        FreshStackByteAtESPPlus13: 1)),
                freshESI,
                receiverValue);
        return GameSocketParserSelectorDiagnosticReturnCleanupProjection.Project(
            new(
                parentCapture,
                normalReturnObserved,
                postCallESP));
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
}
