using Firefall.Reconstruction.Startup;

internal static class SocketStatePredicateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Socket-state predicate pins the sample, 80 raw bytes, and scripted raw-to-VA mapping",
            SocketStatePredicateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && SocketStatePredicateProjection.EntryRawFileOffset == 0x00BCCA60
            && SocketStatePredicateProjection.FunctionEndRawFileOffsetExclusive == 0x00BCCAB0
            && SocketStatePredicateProjection.FunctionEndRawFileOffsetExclusive
                - SocketStatePredicateProjection.EntryRawFileOffset == 0x50
            && SocketStatePredicateProjection.EntryVirtualAddress == 0x00FCD660
            && SocketStatePredicateProjection.FunctionEndVirtualAddressExclusive == 0x00FCD6B0
            && SocketStatePredicateProjection.EntryRawFileOffset
                + SocketStatePredicateProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketStatePredicateProjection.EntryVirtualAddress
            && SocketStatePredicateProjection.FunctionEndRawFileOffsetExclusive
                + SocketStatePredicateProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketStatePredicateProjection.FunctionEndVirtualAddressExclusive
            && SocketStatePredicateProjection.CapturedFunctionBytes
                == "56 8B F1 8A 06 84 C0 75 02 5E C3 A8 01 74 0A "
                    + "33 C0 39 46 04 5E 0F 95 C0 C3 A8 02 75 1A "
                    + "6A 00 68 3C 02 00 00 68 61 78 B5 01 68 62 78 B5 01 "
                    + "FF 15 A0 5E C1 01 83 C4 10 8B 46 08 0B 46 0C "
                    + "75 0A 8B 46 10 0B 46 14 75 02 5E C3 "
                    + "B8 01 00 00 00 5E C3"
            && SocketStatePredicateProjection.CapturedFunctionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x50);

        check("Instruction anchors preserve byte-load, both status tests, diagnostic, and short-circuit branch targets",
            SocketStatePredicateProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.BranchTargetRawFileOffset, instruction.TargetVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    uint? BranchTargetRawFileOffset,
                    uint? TargetVirtualAddress)[]
                {
                    (0x00BCCA63, "8A 06", null, null),
                    (0x00BCCA65, "84 C0 75 02", 0x00BCCA6B, null),
                    (0x00BCCA6B, "A8 01", null, null),
                    (0x00BCCA71, "39 46 04", null, null),
                    (0x00BCCA75, "0F 95 C0 C3", null, null),
                    (0x00BCCA79, "A8 02", null, null),
                    (0x00BCCA7B, "75 1A", 0x00BCCA97, 0x00FCD697),
                    (0x00BCCA7D,
                        "6A 00 68 3C 02 00 00 68 61 78 B5 01 68 62 78 B5 01",
                        null,
                        null),
                    (0x00BCCA8E, "FF 15 A0 5E C1 01", null, 0x01C15EA0),
                    (0x00BCCA97, "8B 46 08 0B 46 0C", null, null),
                    (0x00BCCA9D, "75 0A", 0x00BCCAA9, 0x00FCD6A9),
                    (0x00BCCA9F, "8B 46 10 0B 46 14", null, null),
                    (0x00BCCAA5, "75 02", 0x00BCCAA9, 0x00FCD6A9),
                    (0x00BCCAA7, "5E C3", null, null),
                    (0x00BCCAA9, "B8 01 00 00 00 5E C3", null, null)
                })
            && InstructionSlicesMatchCapturedRange()
            && ShortBranchTarget(0x00BCCA67, 0x02) == 0x00BCCA6B
            && ShortBranchTarget(0x00BCCA6D, 0x0A) == 0x00BCCA79
            && ShortBranchTarget(0x00BCCA7B, 0x1A) == 0x00BCCA97
            && ShortBranchTarget(0x00BCCA9D, 0x0A) == 0x00BCCAA9
            && ShortBranchTarget(0x00BCCAA5, 0x02) == 0x00BCCAA9);

        check("Diagnostic report 0x23C is distinct from the fixed indirect-call cell",
            SocketStatePredicateProjection.DiagnosticReportId == 0x23C
            && SocketStatePredicateProjection.DiagnosticCallRawFileOffset == 0x00BCCA8E
            && SocketStatePredicateProjection.DiagnosticFunctionPointerCellVirtualAddress
                == 0x01C15EA0
            && SocketStatePredicateProjection.DiagnosticCallVirtualAddress == 0x00FCD68E);

        check("This predicate's loaded status byte is independent of the caller's socket+0x108 state DWORD",
            SocketStatePredicateProjection.StatusByteOffset == 0
            && SocketStatePredicateProjection.ReceiverDword4Offset == 4
            && SocketSmallStatusProjection.ReceiverDword108Offset == 0x108
            && SocketSmallStatusProjection.PredicateReceiverOffset == 0x230
            && SocketStatePredicateProjection.StatusByteOffset
                != SocketSmallStatusProjection.ReceiverDword108Offset);

        SocketStatePredicatePlan missingStatus =
            SocketStatePredicateProjection.Project(Captures());
        check("The status byte is a required single capture, and later captures cannot precede it",
            missingStatus.Outcome == SocketStatePredicateOutcome.MissingStatusByte
            && missingStatus.ReturnedEax is null
            && missingStatus.Actions.IsEmpty
            && ThrowsArgumentException(() =>
                SocketStatePredicateProjection.Project(
                    Captures(receiverDword8: 1))));

        bool allStatusBytesCovered = Enumerable.Range(0, 256).All(value =>
        {
            byte status = (byte)value;
            SocketStatePredicatePlan plan;
            if (status == 0)
            {
                plan = SocketStatePredicateProjection.Project(
                    Captures(status, incomingEax: 0xA1B2C3D4));
                return plan.Outcome
                        == SocketStatePredicateOutcome.ZeroStatusReturnsWithPreservedUpperEax
                    && plan.ReturnedEax == 0xA1B2C300
                    && plan.ReturnedAl == 0
                    && plan.Actions.Count(action =>
                        action.Kind == SocketStatePredicateActionKind.CaptureStatusByte) == 1;
            }

            if ((status & 1) != 0)
            {
                plan = SocketStatePredicateProjection.Project(
                    Captures(status, receiverDword4: 0xFFFFFFFF));
                return plan.Outcome
                        == SocketStatePredicateOutcome.ReceiverDword4ReturnsNormalizedComparison
                    && plan.ReturnedEax == 1
                    && plan.ReturnedAl == 1;
            }

            plan = SocketStatePredicateProjection.Project(
                Captures(status, diagnosticReturnedNormally: (status & 2) == 0 ? true : null,
                    receiverDword8: 1, receiverDwordC: 0));
            return plan.Outcome == SocketStatePredicateOutcome.FirstPairNonzeroReturnsOne
                && plan.ReturnedEax == 1
                && plan.ReturnedAl == 1
                && plan.Actions.Count(action =>
                    action.Kind == SocketStatePredicateActionKind.CaptureStatusByte) == 1
                && plan.Actions.All(action =>
                    action.Kind != SocketStatePredicateActionKind.CaptureReceiverDword10
                    && action.Kind != SocketStatePredicateActionKind.CaptureReceiverDword14);
        });
        check("Every one of the 256 byte patterns follows bit tests on the one loaded status capture",
            allStatusBytesCovered);

        var zeroStatusPlans = new[]
        {
            SocketStatePredicateProjection.Project(Captures(0, incomingEax: 0xFFFFFFFF)),
            SocketStatePredicateProjection.Project(Captures(0, incomingEax: 0x12345678)),
            SocketStatePredicateProjection.Project(Captures(0, incomingEax: 0x000000FF))
        };
        check("Zero status returns AL zero while preserving exactly the incoming EAX upper 24 bits",
            zeroStatusPlans.Select(plan => plan.ReturnedEax)
                .SequenceEqual(new uint?[] { 0xFFFFFF00, 0x12345600, 0 })
            && zeroStatusPlans.All(plan =>
                plan.Outcome == SocketStatePredicateOutcome.ZeroStatusReturnsWithPreservedUpperEax
                && plan.ReturnedAl == 0
                && plan.Actions[^1].Kind
                    == SocketStatePredicateActionKind.ReturnZeroStatusWithPreservedUpperEax)
            && ThrowsArgumentException(() =>
                SocketStatePredicateProjection.Project(
                    Captures(0, receiverDword4: 0))));

        bool allSetBitZeroComparisonsNormalize = Enumerable.Range(1, 128).All(index =>
        {
            byte status = (byte)(index * 2 - 1);
            uint dword = (uint)index % 2 == 0 ? 0u : uint.MaxValue;
            SocketStatePredicatePlan plan = SocketStatePredicateProjection.Project(
                Captures(status, receiverDword4: dword));
            return plan.Outcome
                    == SocketStatePredicateOutcome.ReceiverDword4ReturnsNormalizedComparison
                && plan.ReturnedEax == (dword == 0 ? 0u : 1u)
                && plan.ReturnedAl == (dword == 0 ? (byte)0 : (byte)1)
                && plan.StopAtRawFileOffsetExclusive
                    == SocketStatePredicateProjection.ReceiverDword4ReturnRawFileOffsetExclusive;
        });
        SocketStatePredicatePlan missingDword4 =
            SocketStatePredicateProjection.Project(Captures(1));
        check("Every set-bit-0 byte reads this+4 and returns normalized zero or one",
            allSetBitZeroComparisonsNormalize
            && missingDword4.Outcome == SocketStatePredicateOutcome.MissingReceiverDword4
            && missingDword4.ReturnedEax is null
            && ThrowsArgumentException(() =>
                SocketStatePredicateProjection.Project(
                    Captures(1, receiverDword4: 0, receiverDword8: 1))));

        SocketStatePredicatePlan diagnosticUnavailable =
            SocketStatePredicateProjection.Project(Captures(4));
        SocketStatePredicatePlan diagnosticDidNotReturn =
            SocketStatePredicateProjection.Project(
                Captures(4, diagnosticReturnedNormally: false));
        SocketStatePredicatePlan diagnosticReturned =
            SocketStatePredicateProjection.Project(
                Captures(4, diagnosticReturnedNormally: true,
                    receiverDword8: 0, receiverDwordC: 0,
                    receiverDword10: 0, receiverDword14: 0));
        SocketStatePredicatePlan bit1SkipsDiagnostic =
            SocketStatePredicateProjection.Project(
                Captures(2, receiverDword8: 0, receiverDwordC: 0,
                    receiverDword10: 0, receiverDword14: 1));
        check("Bit0-clear/bit1-clear diagnostic return gates later fields; bit1-set skips the call",
            diagnosticUnavailable.Outcome == SocketStatePredicateOutcome.DiagnosticReturnUnavailable
            && diagnosticUnavailable.StopAtRawFileOffsetExclusive
                == SocketStatePredicateProjection.DiagnosticCallReturnRawFileOffsetExclusive
            && diagnosticDidNotReturn.Outcome
                == SocketStatePredicateOutcome.DiagnosticDidNotReturnNormally
            && diagnosticDidNotReturn.ReturnedEax is null
            && diagnosticReturned.Outcome == SocketStatePredicateOutcome.LastPairZeroReturnsZero
            && diagnosticReturned.ReturnedEax == 0
            && diagnosticReturned.Actions.Any(action =>
                action.Kind == SocketStatePredicateActionKind.CallDiagnostic
                && action.CapturedValue == 0x23C
                && action.CallReturnedNormally == true)
            && bit1SkipsDiagnostic.Outcome
                == SocketStatePredicateOutcome.LastPairNonzeroReturnsOne
            && bit1SkipsDiagnostic.ReturnedEax == 1
            && bit1SkipsDiagnostic.Actions.All(action =>
                action.Kind != SocketStatePredicateActionKind.CallDiagnostic)
            && ThrowsArgumentException(() =>
                SocketStatePredicateProjection.Project(
                    Captures(4, diagnosticReturnedNormally: false, receiverDword8: 0)))
            && ThrowsArgumentException(() =>
                SocketStatePredicateProjection.Project(
                    Captures(2, diagnosticReturnedNormally: true))));

        SocketStatePredicatePlan firstPairDword8Nonzero =
            SocketStatePredicateProjection.Project(
                Captures(2, receiverDword8: 0x80000000, receiverDwordC: 0));
        SocketStatePredicatePlan firstPairDwordCNonzero =
            SocketStatePredicateProjection.Project(
                Captures(2, receiverDword8: 0, receiverDwordC: 1));
        SocketStatePredicatePlan firstPairMissing8 =
            SocketStatePredicateProjection.Project(Captures(2));
        SocketStatePredicatePlan firstPairMissingC =
            SocketStatePredicateProjection.Project(
                Captures(2, receiverDword8: 0));
        SocketStatePredicatePlan firstPairBothZeroLastMissing =
            SocketStatePredicateProjection.Project(
                Captures(2, receiverDword8: 0, receiverDwordC: 0));
        check("Both first-pair DWORDs are independent reads; a nonzero OR skips both later reads",
            firstPairDword8Nonzero.Outcome
                == SocketStatePredicateOutcome.FirstPairNonzeroReturnsOne
            && firstPairDword8Nonzero.ReturnedEax == 1
            && firstPairDwordCNonzero.Outcome
                == SocketStatePredicateOutcome.FirstPairNonzeroReturnsOne
            && firstPairDwordCNonzero.ReturnedEax == 1
            && firstPairDword8Nonzero.Actions.Any(action =>
                action.Kind == SocketStatePredicateActionKind.CaptureReceiverDwordC
                && action.CapturedValue == 0)
            && firstPairDword8Nonzero.Actions.All(action =>
                action.Kind != SocketStatePredicateActionKind.CaptureReceiverDword10
                && action.Kind != SocketStatePredicateActionKind.CaptureReceiverDword14)
            && firstPairMissing8.Outcome
                == SocketStatePredicateOutcome.MissingFirstPairCapture
            && firstPairMissing8.StopAtRawFileOffsetExclusive
                == SocketStatePredicateProjection.FirstPairDword8CaptureRawFileOffset
            && firstPairMissing8.Actions[^1].Kind
                == SocketStatePredicateActionKind.CaptureReceiverDword8
            && firstPairMissing8.Actions.All(action =>
                action.Kind != SocketStatePredicateActionKind.CaptureReceiverDwordC)
            && ThrowsArgumentException(() =>
                SocketStatePredicateProjection.Project(
                    Captures(2, receiverDwordC: 0)))
            && firstPairMissingC.Outcome
                == SocketStatePredicateOutcome.MissingFirstPairCapture
            && firstPairMissingC.StopAtRawFileOffsetExclusive
                == SocketStatePredicateProjection.FirstPairDwordCCaptureRawFileOffset
            && firstPairMissingC.Actions[^1].Kind
                == SocketStatePredicateActionKind.CaptureReceiverDwordC
            && firstPairBothZeroLastMissing.Outcome
                == SocketStatePredicateOutcome.MissingLastPairCapture
            && firstPairBothZeroLastMissing.StopAtRawFileOffsetExclusive
                == SocketStatePredicateProjection.LastPairDword10CaptureRawFileOffset
            && firstPairBothZeroLastMissing.Actions[^1].Kind
                == SocketStatePredicateActionKind.CaptureReceiverDword10
            && firstPairBothZeroLastMissing.Actions.All(action =>
                action.Kind != SocketStatePredicateActionKind.CaptureReceiverDword14)
            && ThrowsArgumentException(() =>
                SocketStatePredicateProjection.Project(
                    Captures(2, receiverDword8: 1, receiverDwordC: 0,
                        receiverDword10: 0))));

        bool allLastPairCombinations = new (uint First, uint Second, uint Expected)[]
            {
                (0, 0, 0),
                (1, 0, 1),
                (0, uint.MaxValue, 1),
                (0x80000000, 0x7FFFFFFF, 1)
            }
            .All(values =>
            {
                SocketStatePredicatePlan plan = SocketStatePredicateProjection.Project(
                    Captures(6, receiverDword8: 0, receiverDwordC: 0,
                        receiverDword10: values.First, receiverDword14: values.Second));
                return plan.ReturnedEax == values.Expected
                    && plan.ReturnedAl == values.Expected
                    && plan.Outcome == (values.Expected == 0
                        ? SocketStatePredicateOutcome.LastPairZeroReturnsZero
                        : SocketStatePredicateOutcome.LastPairNonzeroReturnsOne);
            });
        check("Only a zero first pair reaches the independently captured last pair, which returns normalized OR truth",
            allLastPairCombinations
            && firstPairBothZeroLastMissing.ReturnedEax is null
            && ThrowsArgumentException(() =>
                SocketStatePredicateProjection.Project(
                    Captures(6, receiverDword8: 0, receiverDwordC: 0,
                        receiverDword14: 1)))
            && SocketStatePredicateProjection.Project(
                Captures(6, receiverDword8: 0, receiverDwordC: 0,
                    receiverDword10: 0)).StopAtRawFileOffsetExclusive
                == SocketStatePredicateProjection.LastPairDword14CaptureRawFileOffset
            && SocketStatePredicateProjection.Project(
                Captures(6, receiverDword8: 0, receiverDwordC: 0,
                    receiverDword10: 0)).Actions[^1].Kind
                == SocketStatePredicateActionKind.CaptureReceiverDword14);

        check("Every receiver-field address uses wrapping 32-bit this-plus-offset arithmetic",
            SocketStatePredicateProjection.StatusByteAddress(uint.MaxValue) == uint.MaxValue
            && SocketStatePredicateProjection.ReceiverDword4Address(uint.MaxValue - 3) == 0
            && SocketStatePredicateProjection.ReceiverDword8Address(uint.MaxValue - 7) == 0
            && SocketStatePredicateProjection.ReceiverDwordCAddress(uint.MaxValue - 11) == 0
            && SocketStatePredicateProjection.ReceiverDword10Address(uint.MaxValue - 15) == 0
            && SocketStatePredicateProjection.ReceiverDword14Address(uint.MaxValue - 19) == 0);
    }

    private static SocketStatePredicateCapture Captures(
        byte? status = null,
        uint incomingEax = 0xA1B2C3D4,
        uint? receiverDword4 = null,
        bool? diagnosticReturnedNormally = null,
        uint? receiverDword8 = null,
        uint? receiverDwordC = null,
        uint? receiverDword10 = null,
        uint? receiverDword14 = null) =>
        new(
            ReceiverPointer: 0x10203040,
            IncomingEaxAtEntry: incomingEax,
            LoadedStatusByte: status,
            ReceiverDword4: receiverDword4,
            DiagnosticReturnedNormally: diagnosticReturnedNormally,
            ReceiverDword8: receiverDword8,
            ReceiverDwordC: receiverDwordC,
            ReceiverDword10: receiverDword10,
            ReceiverDword14: receiverDword14);

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked((uint)((int)instructionRawFileOffset + 2 + (sbyte)displacement));

    private static bool InstructionSlicesMatchCapturedRange()
    {
        byte[] captured = Convert.FromHexString(
            SocketStatePredicateProjection.CapturedFunctionBytes.Replace(" ", string.Empty));
        return SocketStatePredicateProjection.Instructions.All(instruction =>
        {
            if (instruction.RawFileOffset < SocketStatePredicateProjection.EntryRawFileOffset)
                return false;

            uint offset = instruction.RawFileOffset
                - SocketStatePredicateProjection.EntryRawFileOffset;
            byte[] expected = Convert.FromHexString(
                instruction.Bytes.Replace(" ", string.Empty));
            return offset <= (uint)captured.Length
                && (uint)expected.Length <= (uint)captured.Length - offset
                && captured.AsSpan((int)offset, expected.Length).SequenceEqual(expected);
        });
    }

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
