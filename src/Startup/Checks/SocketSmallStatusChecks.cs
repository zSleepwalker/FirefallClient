using Firefall.Reconstruction.Startup;

internal static class SocketSmallStatusChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Small-status projection pins the requested sample, raw interval, and arithmetic VA mapping",
            SocketSmallStatusProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && SocketSmallStatusProjection.EntryRawFileOffset == 0x00BD3EBF
            && SocketSmallStatusProjection.EntryEndRawFileOffsetExclusive == 0x00BD3F1C
            && SocketSmallStatusProjection.EntryEndRawFileOffsetExclusive
                - SocketSmallStatusProjection.EntryRawFileOffset == 0x5D
            && SocketSmallStatusProjection.EntryVirtualAddress == 0x00FD4ABF
            && SocketSmallStatusProjection.EntryEndVirtualAddressExclusive == 0x00FD4B1C
            && SocketSmallStatusProjection.EntryRawFileOffset
                + SocketSmallStatusProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketSmallStatusProjection.EntryVirtualAddress
            && SocketSmallStatusProjection.EntryEndRawFileOffsetExclusive
                + SocketSmallStatusProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketSmallStatusProjection.EntryEndVirtualAddressExclusive
            && SocketSmallStatusProjection.CapturedFunctionBytes
                == "83 FF 02 75 09 8B CE E8 65 E5 FF FF EB 93 "
                    + "83 FF 03 75 4A 83 BE 08 01 00 00 04 75 85 "
                    + "8D 8E 30 02 00 00 E8 7A 8B FF FF 84 C0 74 0A "
                    + "66 83 BE 48 02 00 00 00 75 1A 6A 00 68 EB 04 00 00 "
                    + "68 AF 83 B5 01 68 B2 83 B5 01 FF 15 A0 5E C1 01 "
                    + "83 C4 10 6A 05 8B CE E8 59 DC FF FF E9 44 FF FF FF"
            && SocketSmallStatusProjection.CapturedFunctionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x5D);

        check("The entry is exactly the prior verified nonzero unsigned-below-16 fall-through",
            SocketReceivedQueuePopProjection.ZeroBranchTargetRawFileOffset
                == SocketSmallStatusProjection.PriorZeroBranchTargetRawFileOffset
            && SocketReceivedQueuePopProjection.UnsignedThresholdBranchTargetRawFileOffset
                == SocketSmallStatusProjection.PriorUnsignedAtLeast16BranchTargetRawFileOffset
            && SocketReceivedQueuePopProjection.UnsignedThresholdFallThroughRawFileOffset
                == SocketSmallStatusProjection.EntryRawFileOffset
            && SocketReceivedQueuePopProjection.UnsignedThresholdBranchMnemonic == "JNB"
            && SocketReceivedQueuePopProjection.UnsignedThresholdCondition == "EDI >= 0x10 (unsigned)");

        check("Raw caller evidence pins the branch targets, calls, offsets, and no invented case-2 second argument",
            SocketSmallStatusProjection.Case2CallTargetRawFileOffset == 0x00BD2430
            && SocketSmallStatusProjection.PredicateCallTargetRawFileOffset == 0x00BCCA60
            && SocketSmallStatusProjection.SetStateCallTargetRawFileOffset == 0x00BD1B70
            && SocketSmallStatusProjection.Case2SecondArgumentSetupObserved == false
            && SocketSmallStatusProjection.Case2CallVirtualAddress == 0x00FD4AC6
            && SocketSmallStatusProjection.PredicateCallVirtualAddress == 0x00FD4AE1
            && SocketSmallStatusProjection.SetStateCallVirtualAddress == 0x00FD4B12
            && SocketSmallStatusProjection.Case2CallTargetVirtualAddress == 0x00FD3030
            && SocketSmallStatusProjection.PredicateCallTargetVirtualAddress == 0x00FCD660
            && SocketSmallStatusProjection.SetStateCallTargetVirtualAddress == 0x00FD2770
            && SocketSmallStatusProjection.Case2CallTargetRawFileOffset
                + SocketSmallStatusProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketSmallStatusProjection.Case2CallTargetVirtualAddress
            && SocketSmallStatusProjection.PredicateCallTargetRawFileOffset
                + SocketSmallStatusProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketSmallStatusProjection.PredicateCallTargetVirtualAddress
            && SocketSmallStatusProjection.SetStateCallTargetRawFileOffset
                + SocketSmallStatusProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketSmallStatusProjection.SetStateCallTargetVirtualAddress
            && SocketSmallStatusProjection.Case2CallRawFileOffset + 5 - 0x1A9B
                == SocketSmallStatusProjection.Case2CallTargetRawFileOffset
            && SocketSmallStatusProjection.PredicateCallRawFileOffset + 5 - 0x7486
                == SocketSmallStatusProjection.PredicateCallTargetRawFileOffset
            && SocketSmallStatusProjection.SetStateCallRawFileOffset + 5 - 0x23A7
                == SocketSmallStatusProjection.SetStateCallTargetRawFileOffset
            && SocketSmallStatusProjection.Case2JoinBranchEndRawFileOffset == 0x00BD3ECD
            && SocketSmallStatusProjection.OtherStatusBranchEndRawFileOffset == 0x00BD3ED2
            && SocketSmallStatusProjection.Case3JoinBranchEndRawFileOffset == 0x00BD3EDB
            && SocketSmallStatusProjection.PredicateCallReturnRawFileOffset == 0x00BD3EE6
            && SocketSmallStatusProjection.PredicateAlZeroBranchRawFileOffset == 0x00BD3EE8
            && SocketSmallStatusProjection.PredicateZeroBranchEndRawFileOffset == 0x00BD3EEA
            && SocketSmallStatusProjection.SetStateJoinBranchEndRawFileOffset == 0x00BD3F1C
            && ShortBranchTarget(0x00BD3EC2, 0x09) == 0x00BD3ECD
            && ShortBranchTarget(0x00BD3ECB, unchecked((byte)0x93)) == 0x00BD3E60
            && ShortBranchTarget(0x00BD3ED0, 0x4A) == 0x00BD3F1C
            && ShortBranchTarget(0x00BD3ED9, unchecked((byte)0x85)) == 0x00BD3E60
            && ShortBranchTarget(0x00BD3EE8, 0x0A) == 0x00BD3EF4
            && ShortBranchTarget(0x00BD3EF2, 0x1A) == 0x00BD3F0E
            && RelativeBranchTarget(0x00BD3F17, 0xFFFFFF44) == 0x00BD3E60
            && SocketSmallStatusProjection.DiagnosticCode == 0x4EB
            && SocketSmallStatusProjection.ReceiverDword108Offset == 0x108
            && SocketSmallStatusProjection.PredicateReceiverOffset == 0x230
            && SocketSmallStatusProjection.ReceiverWord248Offset == 0x248
            && SocketSmallStatusProjection.SetStateArgument == 5);

        check("The supplied instruction evidence starts at the correct raw sites and preserves the AL-before-word order",
            SocketSmallStatusProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.BranchTargetRawFileOffset))
                .SequenceEqual(new (uint RawFileOffset, string Bytes, uint? BranchTargetRawFileOffset)[]
                {
                    (0x00BD3EC4, "8B CE E8 65 E5 FF FF", null),
                    (0x00BD3ECB, "EB 93", 0x00BD3E60),
                    (0x00BD3ECD, "83 FF 03 75 4A", 0x00BD3F1C),
                    (0x00BD3ED2, "83 BE 08 01 00 00 04 75 85", 0x00BD3E60),
                    (0x00BD3EDB, "8D 8E 30 02 00 00 E8 7A 8B FF FF 84 C0", 0x00BCCA60),
                    (0x00BD3EE8, "74 0A", 0x00BD3EF4),
                    (0x00BD3EEA, "66 83 BE 48 02 00 00 00 75 1A", 0x00BD3F0E),
                    (0x00BD3EF4, "6A 00 68 EB 04 00 00 68 AF 83 B5 01 68 B2 83 B5 01", null),
                    (0x00BD3F05, "FF 15 A0 5E C1 01", null),
                    (0x00BD3F0E, "6A 05 8B CE E8 59 DC FF FF", 0x00BD1B70),
                    (0x00BD3F17, "E9 44 FF FF FF", 0x00BD3E60)
                })
            && InstructionSlicesMatchCapturedRange()
            && SocketSmallStatusProjection.DiagnosticPredicate
                == "AL == 0 OR (AL != 0 AND receiver[+0x248] == 0)");

        SocketSmallStatusCapture MissingEntry(uint? edi = null) =>
            new(0x10203040, edi, null, null, null, null, null, null);

        SocketSmallStatusPlan missingEntry =
            SocketSmallStatusProjection.Project(MissingEntry());
        SocketSmallStatusPlan zeroEntry =
            SocketSmallStatusProjection.Project(MissingEntry(0));
        SocketSmallStatusPlan thresholdEntry =
            SocketSmallStatusProjection.Project(MissingEntry(0x10));
        SocketSmallStatusPlan highBitEntry =
            SocketSmallStatusProjection.Project(MissingEntry(0x80000000));
        SocketSmallStatusPlan missingEntryWithLaterCapture = SocketSmallStatusProjection.Project(
            new(0x10203040, null, null, null, null, null, null, true));
        check("Missing, zero, and unsigned EDI values at or above 0x10 are rejected before this entry",
            missingEntry.Outcome == SocketSmallStatusOutcome.MissingEntryEdi
            && zeroEntry.Outcome == SocketSmallStatusOutcome.EntryEdiZeroRejected
            && thresholdEntry.Outcome == SocketSmallStatusOutcome.EntryEdiAtLeast16Rejected
            && highBitEntry.Outcome == SocketSmallStatusOutcome.EntryEdiAtLeast16Rejected
            && missingEntryWithLaterCapture.Outcome
                == SocketSmallStatusOutcome.UnreachedCaptureProvided
            && new[] { missingEntry, zeroEntry, thresholdEntry, highBitEntry }.All(plan =>
                plan.BranchTargetRawFileOffset is null
                && plan.StopAtRawFileOffsetExclusive == SocketSmallStatusProjection.EntryRawFileOffset));

        SocketSmallStatusPlan case2Missing = SocketSmallStatusProjection.Project(
            new(0x10203040, 2, null, null, null, null, null, null));
        SocketSmallStatusPlan case2NoReturn = SocketSmallStatusProjection.Project(
            new(0x10203040, 2, false, null, null, null, null, null));
        SocketSmallStatusPlan case2Returned = SocketSmallStatusProjection.Project(
            new(0x10203040, 2, true, null, null, null, null, null));
        SocketSmallStatusPlan case2WithUnreached = SocketSmallStatusProjection.Project(
            new(0x10203040, 2, true, 4, null, null, null, null));
        check("Status 2 models only ECX=receiver, a nullable opaque normal return, and its join",
            case2Missing.Outcome == SocketSmallStatusOutcome.MissingCase2ReturnCapture
            && case2Missing.CallRawFileOffset == SocketSmallStatusProjection.Case2CallRawFileOffset
            && case2Missing.CallEcx == 0x10203040
            && case2Missing.CallStackArgument is null
            && case2Missing.StopAtRawFileOffsetExclusive
                == SocketSmallStatusProjection.Case2CallReturnRawFileOffset
            && case2NoReturn.Outcome == SocketSmallStatusOutcome.Case2DidNotReturnNormally
            && case2Returned.Outcome == SocketSmallStatusOutcome.Case2NormalReturnJoins
            && case2Returned.BranchTargetRawFileOffset == SocketSmallStatusProjection.JoinRawFileOffset
            && case2WithUnreached.Outcome == SocketSmallStatusOutcome.UnreachedCaptureProvided);

        var otherStatusPlans = new[] { 1u, 4u, 15u }
            .Select(edi => SocketSmallStatusProjection.Project(
                new(0x10203040, edi, null, null, null, null, null, null)))
            .ToArray();
        SocketSmallStatusPlan otherStatusWithCaseCapture = SocketSmallStatusProjection.Project(
            new(0x10203040, 1, null, null, null, null, null, true));
        bool allNonzeroBelow16EntriesRouteCorrectly = Enumerable.Range(1, 15).All(value =>
        {
            SocketSmallStatusOutcome expected = value switch
            {
                2 => SocketSmallStatusOutcome.MissingCase2ReturnCapture,
                3 => SocketSmallStatusOutcome.MissingReceiverDword108,
                _ => SocketSmallStatusOutcome.OtherSmallStatusBranchesToNextDispatch
            };
            return SocketSmallStatusProjection.Project(
                new(0x10203040, (uint)value, null, null, null, null, null, null)).Outcome
                == expected;
        });
        check("Only status 2 and status 3 are handled here; other valid small statuses branch to BD3F1C",
            otherStatusPlans.All(plan =>
                plan.Outcome == SocketSmallStatusOutcome.OtherSmallStatusBranchesToNextDispatch
                && plan.BranchTargetRawFileOffset
                    == SocketSmallStatusProjection.OtherStatusTargetRawFileOffset
                && plan.StopAtRawFileOffsetExclusive
                        == SocketSmallStatusProjection.OtherStatusBranchEndRawFileOffset)
            && otherStatusWithCaseCapture.Outcome == SocketSmallStatusOutcome.UnreachedCaptureProvided
            && allNonzeroBelow16EntriesRouteCorrectly);

        SocketSmallStatusPlan case3MissingState = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, null, null, null, null, null));
        SocketSmallStatusPlan case3MissingStateWithLaterCapture = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, null, 1, null, null, null));
        SocketSmallStatusPlan case3StateNotFour = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 3, null, null, null, null));
        SocketSmallStatusPlan case3StateWithUnreached = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 3, 0, null, null, null));
        SocketSmallStatusPlan case3MissingAl = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 4, null, null, null, null));
        check("Status 3 independently gates on DWORD receiver+0x108==4 before its ECX=receiver+0x230 call",
            case3MissingState.Outcome == SocketSmallStatusOutcome.MissingReceiverDword108
            && case3MissingStateWithLaterCapture.Outcome
                == SocketSmallStatusOutcome.UnreachedCaptureProvided
            && case3StateNotFour.Outcome == SocketSmallStatusOutcome.Case3StateNotFourJoins
            && case3StateNotFour.BranchTargetRawFileOffset
                == SocketSmallStatusProjection.JoinRawFileOffset
            && case3StateNotFour.StopAtRawFileOffsetExclusive
                == SocketSmallStatusProjection.Case3JoinBranchEndRawFileOffset
            && case3StateWithUnreached.Outcome
                == SocketSmallStatusOutcome.UnreachedCaptureProvided
            && case3MissingAl.Outcome == SocketSmallStatusOutcome.MissingPredicateAl
            && case3MissingAl.CallRawFileOffset == SocketSmallStatusProjection.PredicateCallRawFileOffset
            && case3MissingAl.CallEcx
                == SocketSmallStatusProjection.AddReceiverOffset(0x10203040, 0x230));

        SocketSmallStatusPlan wrappedPredicateReceiver = SocketSmallStatusProjection.Project(
            new(uint.MaxValue - 0x100, 3, null, 4, null, null, null, null));
        check("The predicate receiver offset uses wrapping 32-bit receiver+0x230 arithmetic",
            wrappedPredicateReceiver.CallEcx == 0x12F
            && wrappedPredicateReceiver.StopAtRawFileOffsetExclusive
                == SocketSmallStatusProjection.PredicateCallReturnRawFileOffset);
        check("All captured receiver field and subobject addresses use wrapping 32-bit offset arithmetic",
            SocketSmallStatusProjection.ReceiverDword108Address(uint.MaxValue - 0x50) == 0xB7
            && SocketSmallStatusProjection.PredicateReceiverAddress(uint.MaxValue - 0x100) == 0x12F
            && SocketSmallStatusProjection.ReceiverWord248Address(uint.MaxValue - 0x100) == 0x147);

        SocketSmallStatusPlan alZeroMissingDiagnosticReturn = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 4, 0, null, null, null));
        SocketSmallStatusPlan alZeroDiagnosticDidNotReturn = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 4, 0, null, false, null));
        SocketSmallStatusPlan alZeroDiagnosticReturned = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 4, 0, null, true, null));
        SocketSmallStatusPlan alZeroWordUnreached = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 4, 0, 1, null, null));
        SocketSmallStatusPlan alZeroSetStateUnreached = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 4, 0, null, false, true));
        check("AL==0 branches directly to diagnostic 0x4EB and never captures receiver+0x248",
            alZeroMissingDiagnosticReturn.Outcome
                == SocketSmallStatusOutcome.MissingDiagnosticReturnCapture
            && alZeroMissingDiagnosticReturn.DiagnosticPath
                == SocketSmallStatusDiagnosticPath.PredicateAlZero
            && alZeroMissingDiagnosticReturn.CallRawFileOffset
                == SocketSmallStatusProjection.DiagnosticCallRawFileOffset
            && alZeroMissingDiagnosticReturn.StopAtRawFileOffsetExclusive
                == SocketSmallStatusProjection.DiagnosticCallReturnRawFileOffset
            && alZeroDiagnosticDidNotReturn.Outcome
                == SocketSmallStatusOutcome.DiagnosticDidNotReturnNormally
            && alZeroDiagnosticReturned.Outcome
                == SocketSmallStatusOutcome.MissingSetStateReturnCapture
            && alZeroDiagnosticReturned.CallEcx == 0x10203040
            && alZeroDiagnosticReturned.CallStackArgument == 5
            && alZeroWordUnreached.Outcome == SocketSmallStatusOutcome.UnreachedCaptureProvided
            && alZeroSetStateUnreached.Outcome == SocketSmallStatusOutcome.UnreachedCaptureProvided);

        SocketSmallStatusPlan nonzeroAlMissingWord = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 4, 1, null, null, null));
        SocketSmallStatusPlan nonzeroAlWithUnreached = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 4, 1, null, true, null));
        check("Only nonzero AL reaches the independent WORD receiver+0x248 comparison",
            nonzeroAlMissingWord.Outcome == SocketSmallStatusOutcome.MissingReceiverWord248
            && nonzeroAlMissingWord.StopAtRawFileOffsetExclusive
                == SocketSmallStatusProjection.Word248CompareRawFileOffset
            && nonzeroAlWithUnreached.Outcome
                == SocketSmallStatusOutcome.UnreachedCaptureProvided);

        bool allDiagnosticReturnCombinations = new[]
            {
                (Al: (byte)0, Word: (ushort?)null,
                    Path: SocketSmallStatusDiagnosticPath.PredicateAlZero),
                (Al: (byte)1, Word: (ushort?)0,
                    Path: SocketSmallStatusDiagnosticPath.ReceiverWord248Zero)
            }
            .All(gate => new bool?[] { null, false, true }
            .All(diagnosticReturn => new bool?[] { null, false, true }.All(setStateReturn =>
            {
                var plan = SocketSmallStatusProjection.Project(
                    new(0x10203040, 3, null, 4, gate.Al, gate.Word, diagnosticReturn, setStateReturn));
                SocketSmallStatusOutcome expected = (diagnosticReturn, setStateReturn) switch
                {
                    (null, null) => SocketSmallStatusOutcome.MissingDiagnosticReturnCapture,
                    (null, _) => SocketSmallStatusOutcome.UnreachedCaptureProvided,
                    (false, null) => SocketSmallStatusOutcome.DiagnosticDidNotReturnNormally,
                    (false, _) => SocketSmallStatusOutcome.UnreachedCaptureProvided,
                    (true, null) => SocketSmallStatusOutcome.MissingSetStateReturnCapture,
                    (true, false) => SocketSmallStatusOutcome.SetStateDidNotReturnNormally,
                    (true, true) => SocketSmallStatusOutcome.SetStateNormalReturnJoins
                };
                return plan.Outcome == expected
                    && plan.DiagnosticPath == gate.Path;
            })));
        check("Diagnostic and state-setting return captures are gated strictly across every nullable combination",
            allDiagnosticReturnCombinations);

        bool allWordGateCombinations = new ushort?[] { 0, 1, ushort.MaxValue }
            .All(word248 => new bool?[] { null, false, true }.All(setStateReturn =>
            {
                bool skipsDiagnostic = word248 != 0;
                var plan = SocketSmallStatusProjection.Project(
                    new(0x10203040, 3, null, 4, 1, word248,
                        skipsDiagnostic ? null : true,
                        setStateReturn));
                SocketSmallStatusOutcome expected = (skipsDiagnostic, setStateReturn) switch
                {
                    (true, null) => SocketSmallStatusOutcome.MissingSetStateReturnCapture,
                    (true, false) => SocketSmallStatusOutcome.SetStateDidNotReturnNormally,
                    (true, true) => SocketSmallStatusOutcome.SetStateNormalReturnJoins,
                    (false, null) => SocketSmallStatusOutcome.MissingSetStateReturnCapture,
                    (false, false) => SocketSmallStatusOutcome.SetStateDidNotReturnNormally,
                    (false, true) => SocketSmallStatusOutcome.SetStateNormalReturnJoins
                };
                return plan.Outcome == expected
                    && plan.DiagnosticPath == (word248 == 0
                        ? SocketSmallStatusDiagnosticPath.ReceiverWord248Zero
                        : SocketSmallStatusDiagnosticPath.ReceiverWord248Nonzero);
            }));
        SocketSmallStatusPlan nonzeroWordWithUnreachedDiagnostic = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 4, 1, 1, false, null));
        check("WORD==0 reports after nonzero AL; nonzero WORD skips that diagnostic, and both call the helper with argument 5",
            allWordGateCombinations
            && nonzeroWordWithUnreachedDiagnostic.Outcome
                == SocketSmallStatusOutcome.UnreachedCaptureProvided);

        SocketSmallStatusPlan diagnosticAndStateReturned = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 4, 1, 0, true, true));
        SocketSmallStatusPlan skippedDiagnosticAndStateReturned = SocketSmallStatusProjection.Project(
            new(0x10203040, 3, null, 4, 1, ushort.MaxValue, null, true));
        check("Every case-3 diagnostic decision calls the helper with argument 5 and joins only after normal return",
            diagnosticAndStateReturned.Outcome
                == SocketSmallStatusOutcome.SetStateNormalReturnJoins
            && diagnosticAndStateReturned.DiagnosticPath
                == SocketSmallStatusDiagnosticPath.ReceiverWord248Zero
            && diagnosticAndStateReturned.CallRawFileOffset
                == SocketSmallStatusProjection.SetStateCallRawFileOffset
            && diagnosticAndStateReturned.CallEcx == 0x10203040
            && diagnosticAndStateReturned.CallStackArgument == 5
            && diagnosticAndStateReturned.BranchTargetRawFileOffset
                == SocketSmallStatusProjection.JoinRawFileOffset
            && diagnosticAndStateReturned.StopAtRawFileOffsetExclusive
                == SocketSmallStatusProjection.SetStateJoinBranchEndRawFileOffset
            && skippedDiagnosticAndStateReturned.Outcome
                == SocketSmallStatusOutcome.SetStateNormalReturnJoins
            && skippedDiagnosticAndStateReturned.DiagnosticPath
                == SocketSmallStatusDiagnosticPath.ReceiverWord248Nonzero);
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked((uint)((int)instructionRawFileOffset + 2 + (sbyte)displacement));

    private static uint RelativeBranchTarget(uint instructionRawFileOffset, uint displacement) =>
        unchecked(instructionRawFileOffset + 5 + (uint)(int)displacement);

    private static bool InstructionSlicesMatchCapturedRange()
    {
        byte[] captured = Convert.FromHexString(
            SocketSmallStatusProjection.CapturedFunctionBytes.Replace(" ", string.Empty));
        return SocketSmallStatusProjection.Instructions.All(instruction =>
        {
            if (instruction.RawFileOffset < SocketSmallStatusProjection.EntryRawFileOffset)
                return false;

            uint offset = instruction.RawFileOffset - SocketSmallStatusProjection.EntryRawFileOffset;
            byte[] expected = Convert.FromHexString(instruction.Bytes.Replace(" ", string.Empty));
            return offset <= (uint)captured.Length
                && (uint)expected.Length <= (uint)captured.Length - offset
                && captured.AsSpan((int)offset, expected.Length).SequenceEqual(expected);
        });
    }
}
