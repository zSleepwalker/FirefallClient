using Firefall.Reconstruction.Startup;

internal static class SocketStateEntryChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Socket-state entry pins the sample, exact bounded bytes, and raw-to-VA mapping",
            SocketStateEntryProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && SocketStateEntryProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x00400C00
            && SocketStateEntryProjection.EntryRawFileOffset == 0x00BD1B98
            && SocketStateEntryProjection.EntryVirtualAddress == 0x00FD2798
            && SocketStateEntryProjection.EntryBoundaryRawFileOffsetExclusive == 0x00BD1BC2
            && SocketStateEntryProjection.EntryBoundaryVirtualAddressExclusive == 0x00FD27C2
            && SocketStateEntryProjection.EntryBoundaryRawFileOffsetExclusive
                - SocketStateEntryProjection.EntryRawFileOffset == 0x2A
            && SocketStateEntryProjection.CapturedEntryBytes
                == "8B F1 8B 7D 08 3B BE 08 01 00 00 75 1D "
                    + "68 B0 66 BB 01 68 7F 01 00 00 68 77 7D B5 01 "
                    + "68 93 7D B5 01 FF 15 A0 5E C1 01 83 C4 10"
            && Convert.FromHexString(
                SocketStateEntryProjection.CapturedEntryBytes.Replace(" ", string.Empty)).Length
                == 0x2A);

        check("Instruction evidence pins the state comparison, unequal branch, diagnostic arguments, call, and cleanup",
            SocketStateEntryProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.BranchTargetRawFileOffset, instruction.TargetVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    uint? BranchTargetRawFileOffset,
                    uint? TargetVirtualAddress)[]
                {
                    (0x00BD1B98, "8B F1", null, null),
                    (0x00BD1B9A, "8B 7D 08", null, null),
                    (0x00BD1B9D, "3B BE 08 01 00 00", null, null),
                    (0x00BD1BA3, "75 1D", 0x00BD1BC2, 0x00FD27C2),
                    (0x00BD1BA5, "68 B0 66 BB 01", null, null),
                    (0x00BD1BAA, "68 7F 01 00 00", null, null),
                    (0x00BD1BAF, "68 77 7D B5 01", null, null),
                    (0x00BD1BB4, "68 93 7D B5 01", null, null),
                    (0x00BD1BB9, "FF 15 A0 5E C1 01", null, 0x01C15EA0),
                    (0x00BD1BBF, "83 C4 10", null, null)
                })
            && InstructionSlicesMatchCapturedRange()
            && ShortBranchTarget(0x00BD1BA3, 0x1D) == 0x00BD1BC2
            && SocketStateEntryProjection.DiagnosticCallVirtualAddress == 0x00FD27B9
            && SocketStateEntryProjection.DiagnosticCallReturnRawFileOffsetExclusive
                == 0x00BD1BBF);

        check("Diagnostic arguments are the four pushed literal values in stack order",
            SocketStateEntryProjection.DiagnosticArguments.SequenceEqual(new uint[]
            {
                0x01BB66B0,
                0x17F,
                0x1B57D77,
                0x1B57D93
            })
            && SocketStateEntryProjection.DiagnosticFunctionPointerCellVirtualAddress
                == 0x01C15EA0
            && SocketStateEntryProjection.ReceiverDword108Offset == 0x108);

        SocketStateEntryPlan missingEcx =
            SocketStateEntryProjection.Project(
                new SocketStateEntryCapture(null, null, null, null));
        SocketStateEntryPlan missingArgument =
            SocketStateEntryProjection.Project(
                new SocketStateEntryCapture(0x10203040, null, null, null));
        SocketStateEntryPlan missingReceiver =
            SocketStateEntryProjection.Project(
                new SocketStateEntryCapture(0x10203040, 0xAABBCCDD, null, null));
        check("Missing or unreached comparison captures stop at their respective read boundary",
            missingEcx.Outcome == SocketStateEntryOutcome.MissingEntryEcx
            && missingEcx.StopAtRawFileOffsetExclusive
                == SocketStateEntryProjection.EntryRawFileOffset
            && missingEcx.EdiValueAtCompare is null
            && missingArgument.Outcome == SocketStateEntryOutcome.MissingStackArgumentEbp8
            && missingArgument.EsiValueAtCompare == 0x10203040
            && missingArgument.StopAtRawFileOffsetExclusive
                == SocketStateEntryProjection.StackArgumentLoadRawFileOffset
            && missingReceiver.Outcome == SocketStateEntryOutcome.MissingReceiverDword108
            && missingReceiver.EsiValueAtCompare == 0x10203040
            && missingReceiver.EdiValueAtCompare == 0xAABBCCDD
            && missingReceiver.ReceiverDword108Address == 0x10203148
            && missingReceiver.StopAtRawFileOffsetExclusive
                == SocketStateEntryProjection.ReceiverDword108CompareRawFileOffset);

        SocketStateEntryPlan unequal =
            SocketStateEntryProjection.Project(
                Captures(stackArgument: 0x11223344, receiverDword108: 0x11223345));
        check("An unequal state branches over the diagnostic to the exclusive boundary",
            unequal.Outcome == SocketStateEntryOutcome.UnequalStateSkipsDiagnostic
            && unequal.EsiValueAtCompare == 0x10203040
            && unequal.EdiValueAtCompare == 0x11223344
            && unequal.ReceiverDword108Address == 0x10203148
            && unequal.StatesEqual == false
            && unequal.Actions.Any(action =>
                action.Kind == SocketStateEntryActionKind.BranchOnUnequalState
                && action.ConditionMet == true
                && action.BranchTargetRawFileOffset == 0x00BD1BC2
                && action.TargetVirtualAddress == 0x00FD27C2)
            && unequal.Actions.All(action =>
                action.Kind != SocketStateEntryActionKind.PushDiagnosticArgument
                && action.Kind != SocketStateEntryActionKind.CallDiagnostic
                && action.Kind != SocketStateEntryActionKind.CleanupDiagnosticArguments)
            && unequal.StopAtRawFileOffsetExclusive == 0x00BD1BC2
            && ThrowsArgumentException(() =>
                SocketStateEntryProjection.Project(
                    Captures(stackArgument: 1, receiverDword108: 2,
                        diagnosticReturnedNormally: true))));

        SocketStateEntryPlan equalReturnUnavailable =
            SocketStateEntryProjection.Project(
                Captures(stackArgument: 7, receiverDword108: 7));
        SocketStateEntryPlan equalDidNotReturn =
            SocketStateEntryProjection.Project(
                Captures(stackArgument: 7, receiverDword108: 7,
                    diagnosticReturnedNormally: false));
        SocketStateEntryPlan equalReturned =
            SocketStateEntryProjection.Project(
                Captures(stackArgument: 7, receiverDword108: 7,
                    diagnosticReturnedNormally: true));
        check("Equality reaches the diagnostic; only its normal return reaches cleanup and the boundary",
            equalReturnUnavailable.Outcome == SocketStateEntryOutcome.DiagnosticReturnUnavailable
            && equalReturnUnavailable.StatesEqual == true
            && equalReturnUnavailable.Actions.Count(action =>
                action.Kind == SocketStateEntryActionKind.PushDiagnosticArgument) == 4
            && equalReturnUnavailable.Actions.Any(action =>
                action.Kind == SocketStateEntryActionKind.CallDiagnostic
                && action.CallReturnedNormally is null
                && action.TargetVirtualAddress == 0x01C15EA0)
            && equalReturnUnavailable.Actions.All(action =>
                action.Kind != SocketStateEntryActionKind.CleanupDiagnosticArguments)
            && equalReturnUnavailable.StopAtRawFileOffsetExclusive == 0x00BD1BBF
            && equalDidNotReturn.Outcome
                == SocketStateEntryOutcome.DiagnosticDidNotReturnNormally
            && equalDidNotReturn.Actions.All(action =>
                action.Kind != SocketStateEntryActionKind.CleanupDiagnosticArguments)
            && equalDidNotReturn.StopAtRawFileOffsetExclusive == 0x00BD1BBF
            && equalReturned.Outcome
                == SocketStateEntryOutcome.EqualStateDiagnosticReturnedNormally
            && equalReturned.Actions[^1].Kind
                == SocketStateEntryActionKind.CleanupDiagnosticArguments
            && equalReturned.Actions[^1].CapturedValue == 0x10
            && equalReturned.StopAtRawFileOffsetExclusive == 0x00BD1BC2);

        check("Receiver DWORD address arithmetic wraps at 32 bits",
            SocketStateEntryProjection.ReceiverDword108Address(0xFFFFFF00) == 0x8
            && SocketStateEntryProjection.AddDword(uint.MaxValue, 0x108) == 0x107);
    }

    private static SocketStateEntryCapture Captures(
        uint? entryEcx = 0x10203040,
        uint? stackArgument = 0xAABBCCDD,
        uint? receiverDword108 = 0xAABBCCDD,
        bool? diagnosticReturnedNormally = null) =>
        new(
            EntryEcx: entryEcx,
            StackArgumentEbp8: stackArgument,
            ReceiverDword108: receiverDword108,
            DiagnosticReturnedNormally: diagnosticReturnedNormally);

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked((uint)((int)instructionRawFileOffset + 2 + (sbyte)displacement));

    private static bool InstructionSlicesMatchCapturedRange()
    {
        byte[] captured = Convert.FromHexString(
            SocketStateEntryProjection.CapturedEntryBytes.Replace(" ", string.Empty));
        return SocketStateEntryProjection.Instructions.All(instruction =>
        {
            if (instruction.RawFileOffset < SocketStateEntryProjection.EntryRawFileOffset)
                return false;

            uint offset = instruction.RawFileOffset
                - SocketStateEntryProjection.EntryRawFileOffset;
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
