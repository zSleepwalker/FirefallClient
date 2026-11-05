using Firefall.Reconstruction.Startup;

internal static class GameSocketParserDiagnosticDestinationBranchGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] bytes = ParseBytes(
            GameSocketParserDiagnosticDestinationBranchGateProjection.CoordinatorProvidedBytes);
        uint jaeTarget = RelativeTarget(
            GameSocketParserDiagnosticDestinationBranchGateProjection.JaeRawFileOffset + 6,
            unchecked((int)0xFFFFFD72));
        uint jmpTarget = RelativeTarget(
            GameSocketParserDiagnosticDestinationBranchGateProjection.JmpRawFileOffset + 2,
            0x25);
        check("Coordinator branch bytes and signed relative targets are consistent",
            !GameSocketParserDiagnosticDestinationBranchGateProjection.BytesIndependentlyVerified
            && bytes.Length == 8
            && string.Join(" ", bytes.Select(value => value.ToString("X2")))
                == "0F 83 72 FD FF FF EB 25"
            && GameSocketParserDiagnosticDestinationBranchGateProjection.JaeEncoding
                == "0F 83 72 FD FF FF"
            && GameSocketParserDiagnosticDestinationBranchGateProjection.JmpEncoding == "EB 25"
            && GameSocketParserDiagnosticDestinationBranchGateProjection.JaeRawFileOffset
                == 0x00BD3480
            && GameSocketParserDiagnosticDestinationBranchGateProjection.JmpRawFileOffset
                == 0x00BD3486
            && jaeTarget
                == GameSocketParserDiagnosticDestinationBranchGateProjection.JaeTargetRawFileOffset
            && jmpTarget
                == GameSocketParserDiagnosticDestinationBranchGateProjection.JmpTargetRawFileOffset);

        uint[] values =
        {
            0,
            1,
            2,
            3,
            0x7FFFFFFF,
            0x80000000,
            uint.MaxValue
        };
        bool casesExact = true;
        bool unsignedAndSignedDiffer = false;
        foreach (uint value in values)
        {
            GameSocketParserDiagnosticDestinationBranchGatePlan plan =
                Project(value, postCallESP: 0x9000, freshEBP: 0x7000,
                    comparisonDWORD: value, stackDWORD: 0xAABBCCDD);
            GameSocketParserStoredByteSelectorGateFlags expected =
                ExpectedCompareFlags(value, 2);
            bool carrySet = expected.Carry == true;
            uint expectedTarget = carrySet
                ? GameSocketParserDiagnosticDestinationBranchGateProjection.JmpTargetRawFileOffset
                : GameSocketParserDiagnosticDestinationBranchGateProjection.JaeTargetRawFileOffset;
            casesExact &= plan.ParentPrefixAdmitted
                && plan.ParentPlan.IsCleanCompletion
                && plan.ParentPlan.StopBeforeRawFileOffset
                    == GameSocketParserDiagnosticDestinationPrefixProjection.JaeRawFileOffset
                && plan.ParentExactBoundedCompletion
                && plan.ParentPrefixExactBoundedCompletion
                && plan.ExactBoundedCompletion
                && plan.IsCleanCompletion
                && plan.BranchTargetRawFileOffset == expectedTarget
                && plan.StopBeforeRawFileOffset == expectedTarget
                && plan.FlagsAtStop == expected
                && plan.EDIValue == 0x56E
                && plan.EDXValue == 0xAABBCCDD
                && plan.EDIAndEDXPreserved
                && plan.FlagsPreserved
                && plan.Instructions.Length == (carrySet ? 2 : 1)
                && plan.Instructions[0] is {
                    Kind: GameSocketParserDiagnosticDestinationBranchGateInstructionKind.JaeAt3480,
                    InstructionRawFileOffset: 0x00BD3480,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: var jaeBefore,
                    FlagsAfter: var jaeAfter
                }
                && jaeBefore == expected
                && jaeAfter == expected
                && (carrySet
                    ? plan.Instructions[0].BranchTargetRawFileOffset is null
                        && plan.JmpAttempted
                        && plan.JmpReached
                        && plan.JmpCommitted
                        && plan.Instructions[1] is {
                            Kind: GameSocketParserDiagnosticDestinationBranchGateInstructionKind.JmpAt3486,
                            InstructionRawFileOffset: 0x00BD3486,
                            BranchTargetRawFileOffset: 0x00BD34AD,
                            Attempted: true,
                            Reached: true,
                            Committed: true,
                            FlagsBefore: var jmpBefore,
                            FlagsAfter: var jmpAfter
                        }
                        && jmpBefore == expected
                        && jmpAfter == expected
                    : plan.Instructions[0].BranchTargetRawFileOffset == 0x00BD31F8
                        && !plan.JmpAttempted
                        && !plan.JmpReached
                        && !plan.JmpCommitted)
                && plan.JaeAttempted
                && plan.JaeReached
                && plan.JaeCommitted
                && !plan.BranchTargetInstructionExecuted
                && plan.OrderedSemanticPrefixPreserved
                && plan.ParentPlan.Capture
                    == plan.Capture.ParentCapture;
            unsignedAndSignedDiffer |= value == 0x80000000
                && expected.Carry == false
                && expected.Sign == false
                && expected.Overflow == true;
        }

        check("CMP carry alone selects JAE versus fall-through JMP; flags and registers stay unchanged",
            casesExact && unsignedAndSignedDiffer);

        GameSocketParserDiagnosticDestinationBranchGatePlan missingEBP =
            Project(3, postCallESP: 0x9000, freshEBP: null,
                comparisonDWORD: 3, stackDWORD: 0x1234);
        GameSocketParserDiagnosticDestinationBranchGatePlan missingComparison =
            Project(3, postCallESP: 0x9000, freshEBP: 0x7000,
                comparisonDWORD: null, stackDWORD: 0x1234);
        GameSocketParserDiagnosticDestinationBranchGatePlan missingStackDWORD =
            Project(3, postCallESP: 0x9000, freshEBP: 0x7000,
                comparisonDWORD: 3, stackDWORD: null);
        GameSocketParserDiagnosticDestinationBranchGatePlan nonNormalAncestor =
            Project(3, normalReturnObserved: false, postCallESP: 0x9000,
                freshEBP: 0x7000, comparisonDWORD: 3, stackDWORD: 0x1234);
        GameSocketParserDiagnosticDestinationBranchGatePlan pendingAncestor =
            Project(3, normalReturnObserved: null, postCallESP: 0x9000,
                freshEBP: 0x7000, comparisonDWORD: 3, stackDWORD: 0x1234);
        check("Missing prefix captures and pending ancestors reject the gate without changing bounds",
            IsRejectedAtParentBound(missingEBP)
            && IsRejectedAtParentBound(missingComparison)
            && IsRejectedAtParentBound(missingStackDWORD)
            && IsRejectedAtParentBound(pendingAncestor)
            && IsRejectedAtParentBound(nonNormalAncestor)
            && missingEBP.ParentPlan.FreshPostCallEBPCaptureState
                == GameSocketParserDiagnosticDestinationPrefixCaptureState.Missing
            && missingComparison.ParentPlan.DWORDAtEBPPlusCCaptureState
                == GameSocketParserDiagnosticDestinationPrefixCaptureState.Missing
            && missingStackDWORD.ParentPlan.DWORDAtESPPlus14CaptureState
                == GameSocketParserDiagnosticDestinationPrefixCaptureState.Missing
            && pendingAncestor.ParentPlan.ParentContinuationAdmitted == false);

        GameSocketParserDiagnosticDestinationBranchGatePlan original =
            Project(1, postCallESP: 0xBEEF, freshEBP: 0x1234,
                comparisonDWORD: 1, stackDWORD: 0x12345678, selector: 3);
        GameSocketParserDiagnosticDestinationBranchGatePlan replay =
            GameSocketParserDiagnosticDestinationBranchGateProjection.Project(
                original.Capture);
        check("Immutable capture reprojection retains the ordered semantic prefix and branch decision",
            replay.Capture == original.Capture
            && replay.Outcome == original.Outcome
            && replay.ParentPrefixAdmitted == original.ParentPrefixAdmitted
            && replay.ParentExactBoundedCompletion == original.ParentExactBoundedCompletion
            && replay.ParentPrefixExactBoundedCompletion
                == original.ParentPrefixExactBoundedCompletion
            && replay.ResultESP == original.ResultESP
            && replay.FreshPostCallEBP == original.FreshPostCallEBP
            && replay.EDIValue == original.EDIValue
            && replay.EDXValue == original.EDXValue
            && replay.FlagsAtStop == original.FlagsAtStop
            && replay.BranchTargetRawFileOffset == original.BranchTargetRawFileOffset
            && replay.StopBeforeRawFileOffset == original.StopBeforeRawFileOffset
            && replay.Instructions.SequenceEqual(original.Instructions)
            && replay.OrderedSemanticPrefixPreserved
            && replay.IsCleanCompletion);
    }

    private static bool IsRejectedAtParentBound(
        GameSocketParserDiagnosticDestinationBranchGatePlan plan) =>
        !plan.ParentPrefixAdmitted
        && !plan.JaeAttempted
        && !plan.JaeReached
        && !plan.JaeCommitted
        && !plan.JmpAttempted
        && !plan.JmpReached
        && !plan.JmpCommitted
        && plan.Instructions.IsEmpty
        && plan.BranchTargetRawFileOffset is null
        && plan.StopBeforeRawFileOffset == plan.ParentPlan.StopBeforeRawFileOffset
        && plan.ExactBoundedCompletion == plan.ParentPlan.ExactBoundedCompletion
        && !plan.IsCleanCompletion;

    private static GameSocketParserDiagnosticDestinationBranchGatePlan Project(
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
        var prefixCapture = new GameSocketParserDiagnosticDestinationPrefixCapture(
            parentCapture,
            freshEBP,
            comparisonDWORD ?? comparisonValue,
            stackDWORD);
        if (comparisonDWORD is null)
        {
            prefixCapture = prefixCapture with { FreshDWORDAtEBPPlusC = null };
        }

        return GameSocketParserDiagnosticDestinationBranchGateProjection.Project(
            new(prefixCapture));
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

    private static uint RelativeTarget(uint nextInstruction, int displacement) =>
        unchecked(nextInstruction + (uint)displacement);

    private static byte[] ParseBytes(string encoding) =>
        encoding.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
