using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveEntryChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The entry bytes, sample identity, VA mapping, and guard boundaries are pinned",
            ReliableReceiveEntryProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveEntryProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x00400C00
            && ReliableReceiveEntryProjection.EntryStartRawFileOffset == 0x00BE51F0
            && ReliableReceiveEntryProjection.EntryStartVirtualAddress == 0x00FE5DF0
            && ReliableReceiveEntryProjection.RawFileOffsetToVirtualAddress(0x00BE51F0) == 0x00FE5DF0
            && ReliableReceiveEntryProjection.FirstFlagTestRawFileOffset == 0x00BE51F8
            && ReliableReceiveEntryProjection.FirstFlagBranchRawFileOffset == 0x00BE51FC
            && ReliableReceiveEntryProjection.FirstFlagBranchVirtualAddress == 0x00FE5DFC
            && ReliableReceiveEntryProjection.FirstFlagBranchTargetRawFileOffset == 0x00BE5218
            && ReliableReceiveEntryProjection.FirstFlagBranchTargetVirtualAddress == 0x00FE5E18
            && ReliableReceiveEntryProjection.DiagnosticCallRawFileOffset == 0x00BE520F
            && ReliableReceiveEntryProjection.DiagnosticCallReturnRawFileOffset == 0x00BE5215
            && ReliableReceiveEntryProjection.SecondFlagTestRawFileOffset == 0x00BE5218
            && ReliableReceiveEntryProjection.SecondFlagBranchRawFileOffset == 0x00BE521C
            && ReliableReceiveEntryProjection.SecondFlagBranchVirtualAddress == 0x00FE5E1C
            && ReliableReceiveEntryProjection.SecondFlagBranchTargetRawFileOffset == 0x00BE5227
            && ReliableReceiveEntryProjection.SecondFlagBranchTargetVirtualAddress == 0x00FE5E27
            && ReliableReceiveEntryProjection.EarlyReturnXorRawFileOffset == 0x00BE521E
            && ReliableReceiveEntryProjection.EarlyReturnRawFileOffset == 0x00BE5224
            && ReliableReceiveEntryProjection.EarlyReturnVirtualAddress == 0x00FE5E24
            && ReliableReceiveEntryProjection.EarlyReturnStackArgumentBytes == 4
            && ReliableReceiveEntryProjection.ArgumentLoadBoundaryRawFileOffset == 0x00BE5227
            && ReliableReceiveEntryProjection.ArgumentLoadBoundaryVirtualAddress == 0x00FE5E27
            && ReliableReceiveEntryProjection.PrefixEndRawFileOffsetExclusive == 0x00BE5227
            && ReliableReceiveEntryProjection.CapturedEntryBytes
                == "55 8B EC 53 56 8B F1 57 80 7E 0C 00 75 1A 6A 00 68 DA 01 00 00 "
                    + "68 7A 8B B5 01 68 7B 8B B5 01 FF 15 A0 5E C1 01 83 C4 10 "
                    + "80 7E 1C 00 74 09 32 C0 5F 5E 5B 5D C2 04 00"
            && ReliableReceiveEntryProjection.CapturedEntryBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x37);

        check("The actual JNZ and JZ encodings resolve to the verified raw and virtual destinations",
            ShortBranchTarget(0x00BE51FC, 0x1A) == 0x00BE5218
            && ShortBranchTarget(0x00BE521C, 0x09) == 0x00BE5227
            && ReliableReceiveEntryProjection.RawFileOffsetToVirtualAddress(
                ReliableReceiveEntryProjection.FirstFlagBranchRawFileOffset)
                == ReliableReceiveEntryProjection.FirstFlagBranchVirtualAddress
            && ReliableReceiveEntryProjection.RawFileOffsetToVirtualAddress(
                ReliableReceiveEntryProjection.SecondFlagBranchRawFileOffset)
                == ReliableReceiveEntryProjection.SecondFlagBranchVirtualAddress
            && ReliableReceiveEntryProjection.RawFileOffsetToVirtualAddress(
                ReliableReceiveEntryProjection.EarlyReturnRawFileOffset)
                == ReliableReceiveEntryProjection.EarlyReturnVirtualAddress
            && ReliableReceiveEntryProjection.RawFileOffsetToVirtualAddress(0x00BE5218) == 0x00FE5E18
            && ReliableReceiveEntryProjection.RawFileOffsetToVirtualAddress(0x00BE5227) == 0x00FE5E27
            && InstructionEvidenceMatches());

        check("Diagnostic push values and data pointers match the observed instructions and literals",
            ReliableReceiveEntryProjection.DiagnosticCode == 0x1DA
            && ReliableReceiveEntryProjection.DiagnosticZeroArgument == 0
            && ReliableReceiveEntryProjection.DiagnosticLiteralPointerA == 0x01B58B7A
            && ReliableReceiveEntryProjection.DiagnosticLiteralPointerB == 0x01B58B7B
            && ReliableReceiveEntryProjection.DiagnosticCallPointerSlotAddress == 0x01C15EA0
            && ReliableReceiveEntryProjection.DiagnosticPushValuesInInstructionOrder.SequenceEqual(
                new uint[] { 0, 0x1DA, 0x01B58B7A, 0x01B58B7B }));

        ReliableReceiveEntryPlan missingFirst = ReliableReceiveEntryProjection.Project(new(null, null, null));
        ReliableReceiveEntryPlan missingFirstWithLaterCaptures =
            ReliableReceiveEntryProjection.Project(new(null, 0, true));
        ReliableReceiveEntryPlan missingDiagnosticReturn =
            ReliableReceiveEntryProjection.Project(new(0, null, null));
        ReliableReceiveEntryPlan missingDiagnosticWithUnreachedFlag =
            ReliableReceiveEntryProjection.Project(new(0, 0, null));
        ReliableReceiveEntryPlan nonReturningDiagnostic =
            ReliableReceiveEntryProjection.Project(new(0, 1, false));
        ReliableReceiveEntryPlan skippedDiagnosticWithUnreachedReturn =
            ReliableReceiveEntryProjection.Project(new(1, 0, true));
        ReliableReceiveEntryPlan missingSecondFlag =
            ReliableReceiveEntryProjection.Project(new(1, null, null));
        check("Missing and unreached nullable captures stop at their gates without inventing reads",
            missingFirst.Outcome == ReliableReceiveEntryOutcome.MissingFlagAtOffset0CCapture
            && !missingFirst.FirstFlagRead
            && !missingFirst.DiagnosticCallReached
            && !missingFirst.SecondFlagRead
            && missingFirst.StopAtRawFileOffsetExclusive
                == ReliableReceiveEntryProjection.FirstFlagTestRawFileOffset
            && missingFirstWithLaterCaptures.Outcome
                == ReliableReceiveEntryOutcome.UnreachedDiagnosticReturnCapture
            && missingFirstWithLaterCaptures.UnreachedDiagnosticReturnCaptureProvided
            && missingFirstWithLaterCaptures.UnreachedFlagAtOffset1CCaptureProvided
            && missingDiagnosticReturn.Outcome
                == ReliableReceiveEntryOutcome.MissingDiagnosticReturnCapture
            && missingDiagnosticReturn.DiagnosticCallReached
            && !missingDiagnosticReturn.SecondFlagRead
            && missingDiagnosticWithUnreachedFlag.Outcome
                == ReliableReceiveEntryOutcome.UnreachedFlagAtOffset1CCapture
            && missingDiagnosticWithUnreachedFlag.DiagnosticCallReturnedNormally is null
            && missingDiagnosticWithUnreachedFlag.UnreachedFlagAtOffset1CCaptureProvided
            && !missingDiagnosticWithUnreachedFlag.SecondFlagRead
            && nonReturningDiagnostic.Outcome
                == ReliableReceiveEntryOutcome.UnreachedFlagAtOffset1CCapture
            && !nonReturningDiagnostic.SecondFlagRead
            && skippedDiagnosticWithUnreachedReturn.Outcome
                == ReliableReceiveEntryOutcome.UnreachedDiagnosticReturnCapture
            && !skippedDiagnosticWithUnreachedReturn.DiagnosticCallReached
            && missingSecondFlag.Outcome == ReliableReceiveEntryOutcome.MissingFlagAtOffset1CCapture
            && !missingSecondFlag.SecondFlagRead);

        bool everyFirstByteSelectsItsReadGate = Enumerable.Range(byte.MinValue, byte.MaxValue + 1)
            .All(value =>
            {
                byte first = (byte)value;
                bool zero = first == 0;
                ReliableReceiveEntryPlan plan = ReliableReceiveEntryProjection.Project(
                    new(first, 0, zero ? true : null));
                return plan.FirstFlagRead
                    && plan.FirstFlagIsZero == zero
                    && plan.FirstConditionalBranchTaken == !zero
                    && plan.DiagnosticCallReached == zero
                    && plan.DiagnosticCallReturnedNormally == (zero ? true : null)
                    && !plan.UnreachedDiagnosticReturnCaptureProvided
                    && plan.SecondFlagRead
                    && plan.Outcome == ReliableReceiveEntryOutcome.ArgumentLoadBoundarySelected
                    && plan.ArgumentLoadBoundarySelected
                    && plan.StopAtRawFileOffsetExclusive
                        == ReliableReceiveEntryProjection.ArgumentLoadBoundaryRawFileOffset;
            });
        check("Every byte value at offset 0x0C independently gates the diagnostic call and second read",
            everyFirstByteSelectsItsReadGate);

        bool everySecondByteReturnsOrSelectsOnlyTheBoundary = Enumerable.Range(byte.MinValue, byte.MaxValue + 1)
            .All(value =>
            {
                byte second = (byte)value;
                bool zero = second == 0;
                ReliableReceiveEntryPlan skippedDiagnostic = ReliableReceiveEntryProjection.Project(
                    new(1, second, null));
                ReliableReceiveEntryPlan returnedDiagnostic = ReliableReceiveEntryProjection.Project(
                    new(0, second, true));
                return SecondFlagPlanMatches(skippedDiagnostic, second, zero, diagnosticReached: false)
                    && SecondFlagPlanMatches(returnedDiagnostic, second, zero, diagnosticReached: true);
            });
        check("Every byte value at offset 0x1C selects JZ boundary or AL-zero RET 4 after either gate",
            everySecondByteReturnsOrSelectsOnlyTheBoundary);

        check("Zero second flag stops before argument loading; nonzero returns AL zero with RET 4",
            VerifySecondFlagPlan(
                ReliableReceiveEntryProjection.Project(new(1, 0, null)),
                secondFlag: 0,
                branchTaken: true,
                destination: 0x00BE5227,
                expectedOutcome: ReliableReceiveEntryOutcome.ArgumentLoadBoundarySelected,
                expectedReturnAl: null)
            && VerifySecondFlagPlan(
                ReliableReceiveEntryProjection.Project(new(0, 0xFF, true)),
                secondFlag: 0xFF,
                branchTaken: false,
                destination: 0x00BE521E,
                expectedOutcome: ReliableReceiveEntryOutcome.EarlyReturn,
                expectedReturnAl: 0));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE51F0, "55 8B EC 53 56 8B F1 57", null, null),
            (0x00BE51F8, "80 7E 0C 00", null, null),
            (0x00BE51FC, "75 1A", 0x00BE5218, 0x00FE5E18),
            (0x00BE51FE, "6A 00 68 DA 01 00 00 68 7A 8B B5 01 68 7B 8B B5 01", null, null),
            (0x00BE520F, "FF 15 A0 5E C1 01", null, null),
            (0x00BE5215, "83 C4 10", null, null),
            (0x00BE5218, "80 7E 1C 00", null, null),
            (0x00BE521C, "74 09", 0x00BE5227, 0x00FE5E27),
            (0x00BE521E, "32 C0", null, null),
            (0x00BE5220, "5F 5E 5B 5D C2 04 00", null, null)
        };
        return ReliableReceiveEntryProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);
    }

    private static bool SecondFlagPlanMatches(
        ReliableReceiveEntryPlan plan,
        byte secondFlag,
        bool secondFlagIsZero,
        bool diagnosticReached) =>
        plan.FirstFlagRead
        && plan.FirstFlagIsZero == diagnosticReached
        && plan.FirstConditionalBranchTaken == !diagnosticReached
        && plan.DiagnosticCallReached == diagnosticReached
        && plan.DiagnosticCallReturnedNormally == (diagnosticReached ? true : null)
        && plan.SecondFlagRead
        && plan.FlagAtOffset1C == secondFlag
        && plan.SecondConditionalBranchTaken == secondFlagIsZero
        && plan.UnreachedFlagAtOffset1CCaptureProvided == false
        && VerifySecondFlagPlan(
            plan,
            secondFlag,
            secondFlagIsZero,
            secondFlagIsZero ? 0x00BE5227u : 0x00BE521Eu,
            secondFlagIsZero
                ? ReliableReceiveEntryOutcome.ArgumentLoadBoundarySelected
                : ReliableReceiveEntryOutcome.EarlyReturn,
            secondFlagIsZero ? null : (byte)0);

    private static bool VerifySecondFlagPlan(
        ReliableReceiveEntryPlan plan,
        byte secondFlag,
        bool branchTaken,
        uint destination,
        ReliableReceiveEntryOutcome expectedOutcome,
        byte? expectedReturnAl) =>
        plan.Outcome == expectedOutcome
        && plan.SecondFlagRead
        && plan.FlagAtOffset1C == secondFlag
        && plan.SecondConditionalBranchTaken == branchTaken
        && plan.SelectedDestinationRawFileOffset == destination
        && plan.ReturnAl == expectedReturnAl
        && plan.ReturnStackArgumentBytes
            == (expectedReturnAl.HasValue
                ? ReliableReceiveEntryProjection.EarlyReturnStackArgumentBytes
                : null)
        && plan.ArgumentLoadBoundarySelected == branchTaken
        && plan.StopAtRawFileOffsetExclusive
            == ReliableReceiveEntryProjection.ArgumentLoadBoundaryRawFileOffset;

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);
}
