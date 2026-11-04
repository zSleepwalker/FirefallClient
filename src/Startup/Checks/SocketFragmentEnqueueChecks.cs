using Firefall.Reconstruction.Startup;

internal static class SocketFragmentEnqueueChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Socket fragment enqueue wrapper pins the sample, full raw range, and code VA mapping",
            SocketFragmentEnqueueProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && SocketFragmentEnqueueProjectionRules.EntryRawFileOffset == 0xBE47C0
            && SocketFragmentEnqueueProjectionRules.EntryVirtualAddress == 0xFE53C0
            && SocketFragmentEnqueueProjectionRules.EntryRawFileOffset
                + SocketFragmentEnqueueProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketFragmentEnqueueProjectionRules.EntryVirtualAddress
            && SocketFragmentEnqueueProjectionRules.WrapperEndRawFileOffsetExclusive == 0xBE480A
            && SocketFragmentEnqueueProjectionRules.WrapperEndVirtualAddressExclusive == 0xFE540A
            && SocketFragmentEnqueueProjectionRules.WrapperEndRawFileOffsetExclusive
                + SocketFragmentEnqueueProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketFragmentEnqueueProjectionRules.WrapperEndVirtualAddressExclusive
            && SocketFragmentEnqueueProjectionRules.WrapperEndRawFileOffsetExclusive
                - SocketFragmentEnqueueProjectionRules.EntryRawFileOffset == 0x4A
            && SocketFragmentEnqueueProjectionRules.TailStartRawFileOffset == 0xBE47FA
            && SocketFragmentEnqueueProjectionRules.TailStartVirtualAddress == 0xFE53FA
            && SocketFragmentEnqueueProjectionRules.TailStartRawFileOffset
                + SocketFragmentEnqueueProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketFragmentEnqueueProjectionRules.TailStartVirtualAddress
            && SocketFragmentEnqueueProjectionRules.TailEndRawFileOffsetExclusive == 0xBE480A
            && SocketFragmentEnqueueProjectionRules.AppendCallRawFileOffset == 0xBE47FE
            && SocketFragmentEnqueueProjectionRules.AppendCallVirtualAddress == 0xFE53FE
            && SocketFragmentEnqueueProjectionRules.AppendCallRawFileOffset
                + SocketFragmentEnqueueProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketFragmentEnqueueProjectionRules.AppendCallVirtualAddress
            && SocketFragmentEnqueueProjectionRules.AppendCallNormalReturnRawFileOffset
                + SocketFragmentEnqueueProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketFragmentEnqueueProjectionRules.AppendCallNormalReturnVirtualAddress
            && SocketFragmentEnqueueProjectionRules.AppendFunctionRawFileOffset == 0x12CE310
            && SocketFragmentEnqueueProjectionRules.AppendFunctionRawFileOffset
                + SocketFragmentEnqueueProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketFragmentEnqueueProjectionRules.AppendFunctionVirtualAddress
            && SocketFragmentEnqueueProjectionRules.AppendFunctionVirtualAddress == 0x16CEF10);

        check("Whole-function bytes include the entry prologue and earlier diagnostic path",
            SocketFragmentEnqueueProjectionRules.CapturedWrapperBytes
                == "55 8B EC 51 56 8B 75 08 57 8B F9 B9 FF 07 00 00 "
                    + "66 8B 06 66 23 C1 B9 70 05 00 00 66 3B C1 76 1A "
                    + "6A 00 68 B2 00 00 00 68 97 89 B5 01 68 05 8A B5 01 "
                    + "FF 15 A0 5E C1 01 83 C4 10 56 8D 4F 38 E8 0D 9B 6E 00 "
                    + "5F 5E 59 5D C2 04 00"
            && SocketFragmentEnqueueProjectionRules.CapturedWrapperBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x4A
            && SocketFragmentEnqueueProjectionRules.CapturedWrapperBytes
                .StartsWith("55 8B EC 51 56 8B 75 08 57 8B F9", StringComparison.Ordinal)
            && SocketFragmentEnqueueProjectionRules.CapturedNormalTailBytes
                == "56 8D 4F 38 E8 0D 9B 6E 00 5F 5E 59 5D C2 04 00"
            && SocketFragmentEnqueueProjectionRules.CapturedWrapperBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip((int)(SocketFragmentEnqueueProjectionRules.TailStartRawFileOffset
                    - SocketFragmentEnqueueProjectionRules.EntryRawFileOffset))
                .SequenceEqual(
                    SocketFragmentEnqueueProjectionRules.CapturedNormalTailBytes
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries),
                    StringComparer.OrdinalIgnoreCase));

        check("Diagnostic and append targets are derived from the captured absolute and relative references",
            SocketFragmentEnqueueProjectionRules.DiagnosticCallRawFileOffset == 0xBE47F1
            && SocketFragmentEnqueueProjectionRules.DiagnosticCallVirtualAddress == 0xFE53F1
            && SocketFragmentEnqueueProjectionRules.DiagnosticCallRawFileOffset
                + SocketFragmentEnqueueProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketFragmentEnqueueProjectionRules.DiagnosticCallVirtualAddress
            && SocketFragmentEnqueueProjectionRules.DiagnosticCallCellVirtualAddress == 0x1C15EA0
            && SocketFragmentEnqueueProjectionRules.DiagnosticCallNormalReturnRawFileOffset == 0xBE47F7
            && SocketFragmentEnqueueProjectionRules.DiagnosticArgumentOneVirtualAddress == 0x1B58A05
            && SocketFragmentEnqueueProjectionRules.DiagnosticArgumentTwoVirtualAddress == 0x1B58997
            && SocketFragmentEnqueueProjectionRules.DiagnosticPushedImmediate == 0xB2
            && SocketFragmentEnqueueProjectionRules.AppendCallNormalReturnRawFileOffset == 0xBE4803
            && SocketFragmentEnqueueProjectionRules.AppendCallNormalReturnVirtualAddress == 0xFE5403
            && SocketFragmentEnqueueProjectionRules.AppendCallNextRawFileOffset == 0xBE4803
            && SocketFragmentEnqueueProjectionRules.AppendCallNextRawFileOffset + 0x006E9B0D
                == SocketFragmentEnqueueProjectionRules.AppendFunctionRawFileOffset
            && SocketFragmentEnqueueProjectionRules.WrapperReturnInstructionRawFileOffset == 0xBE4807
            && SocketFragmentEnqueueProjectionRules.WrapperReturnInstructionVirtualAddress == 0xFE5407
            && SocketFragmentEnqueueProjectionRules.WrapperReturnInstructionRawFileOffset
                + SocketFragmentEnqueueProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketFragmentEnqueueProjectionRules.WrapperReturnInstructionVirtualAddress
            && SocketFragmentEnqueueProjectionRules.WrapperReturnStackBytes == 4);

        check("Instruction evidence is contained in the whole-function capture",
            SocketFragmentEnqueueProjectionRules.Instructions.All(InstructionMatchesCapturedSlice)
            && SocketFragmentEnqueueProjectionRules.Instructions.Single(
                instruction => instruction.RawFileOffset
                    == SocketFragmentEnqueueProjectionRules.PayloadArgumentLoadRawFileOffset)
                .Bytes == "8B 75 08"
            && SocketFragmentEnqueueProjectionRules.Instructions.Single(
                instruction => instruction.RawFileOffset
                    == SocketFragmentEnqueueProjectionRules.DiagnosticCallRawFileOffset)
                .Bytes == "FF 15 A0 5E C1 01"
            && SocketFragmentEnqueueProjectionRules.Instructions.Single(
                instruction => instruction.RawFileOffset
                    == SocketFragmentEnqueueProjectionRules.AppendCallRawFileOffset)
                == new SocketFragmentEnqueueInstructionEvidence(
                    "Call the append target and continue directly into the epilogue",
                    0xBE47FE,
                    "E8 0D 9B 6E 00",
                    0x12CE310,
                    NormalReturnContinuationRawFileOffset: 0xBE4803)
            && SocketFragmentEnqueueProjectionRules.Instructions.Single(
                instruction => instruction.RawFileOffset
                    == SocketFragmentEnqueueProjectionRules.AppendCallNormalReturnRawFileOffset)
                .Bytes == "5F 5E 59 5D C2 04 00");

        SocketFragmentEnqueueProjection noDiagnostic =
            SocketFragmentEnqueueProjectionRules.Project(
                new SocketFragmentEnqueueCapture(
                    ReceiverAddress: 0x1000,
                    PayloadArgument: 0x2000,
                    PayloadFirstWord: 0x0570,
                    DiagnosticCallReturnedNormally: null,
                    AppendCallReturnEax: 0x12345678));
        check("The inclusive diagnostic threshold skips earlier reporting and passes append inputs separately",
            noDiagnostic.MaskedPayloadWord == 0x0570
            && !noDiagnostic.ReachesDiagnosticCall
            && noDiagnostic.DiagnosticCallOutcome
                == SocketFragmentEnqueueDiagnosticCallOutcome.NotReached
            && noDiagnostic.ReachesAppendCall
            && noDiagnostic.AppendCallOutcome
                == SocketFragmentEnqueueAppendCallOutcome.ReturnedNormally
            && noDiagnostic.AppendReceiverAddress == 0x1038
            && noDiagnostic.AppendPayloadArgument == 0x2000
            && noDiagnostic.WrapperReturnEax == 0x12345678);

        SocketFragmentEnqueueProjection diagnostic =
            SocketFragmentEnqueueProjectionRules.Project(
                new SocketFragmentEnqueueCapture(
                    ReceiverAddress: 0xFFFFFFF0,
                    PayloadArgument: 0xABCDEF01,
                    PayloadFirstWord: 0x0D71,
                    DiagnosticCallReturnedNormally: true,
                    AppendCallReturnEax: 0));
        check("A masked value above threshold reaches diagnostics then the append tail with wrapping receiver arithmetic",
            diagnostic.MaskedPayloadWord == 0x0571
            && diagnostic.ReachesDiagnosticCall
            && diagnostic.DiagnosticCallOutcome
                == SocketFragmentEnqueueDiagnosticCallOutcome.ReturnedNormally
            && diagnostic.ReachesAppendCall
            && diagnostic.AppendCallOutcome
                == SocketFragmentEnqueueAppendCallOutcome.ReturnedNormally
            && diagnostic.AppendReceiverAddress == 0x28
            && diagnostic.AppendPayloadArgument == 0xABCDEF01
            && diagnostic.WrapperReturnEax == 0);

        SocketFragmentEnqueueProjection diagnosticReturnMissing =
            SocketFragmentEnqueueProjectionRules.Project(
                new SocketFragmentEnqueueCapture(
                    ReceiverAddress: 1,
                    PayloadArgument: 2,
                    PayloadFirstWord: 0x0571,
                    DiagnosticCallReturnedNormally: null,
                    AppendCallReturnEax: null));
        check("A reached diagnostic with no return capture is distinct from a diagnostic that did not return",
            diagnosticReturnMissing.ReachesDiagnosticCall
            && diagnosticReturnMissing.DiagnosticCallOutcome
                == SocketFragmentEnqueueDiagnosticCallOutcome.MissingReturnCapture
            && !diagnosticReturnMissing.ReachesAppendCall
            && diagnosticReturnMissing.AppendCallOutcome
                == SocketFragmentEnqueueAppendCallOutcome.NotReached
            && diagnosticReturnMissing.WrapperReturnEax is null);

        SocketFragmentEnqueueProjection diagnosticDoesNotReturn =
            SocketFragmentEnqueueProjectionRules.Project(
                new SocketFragmentEnqueueCapture(
                    ReceiverAddress: 1,
                    PayloadArgument: 2,
                    PayloadFirstWord: 0x0571,
                    DiagnosticCallReturnedNormally: false,
                    AppendCallReturnEax: null));
        check("A reached diagnostic that did not return normally is distinct from missing return capture",
            diagnosticDoesNotReturn.ReachesDiagnosticCall
            && diagnosticDoesNotReturn.DiagnosticCallOutcome
                == SocketFragmentEnqueueDiagnosticCallOutcome.DidNotReturnNormally
            && !diagnosticDoesNotReturn.ReachesAppendCall
            && diagnosticDoesNotReturn.AppendCallOutcome
                == SocketFragmentEnqueueAppendCallOutcome.NotReached
            && diagnosticDoesNotReturn.AppendReceiverAddress is null
            && diagnosticDoesNotReturn.AppendPayloadArgument is null
            && diagnosticDoesNotReturn.WrapperReturnEax is null);

        SocketFragmentEnqueueProjection absentAppendResult =
            SocketFragmentEnqueueProjectionRules.Project(
                new SocketFragmentEnqueueCapture(
                    ReceiverAddress: 0xFFFFFFFC,
                    PayloadArgument: 0x3000,
                    PayloadFirstWord: 0,
                    DiagnosticCallReturnedNormally: null,
                    AppendCallReturnEax: null));
        check("A reached append with missing return capture is distinct from a returned zero",
            absentAppendResult.ReachesAppendCall
            && absentAppendResult.AppendCallOutcome
                == SocketFragmentEnqueueAppendCallOutcome.MissingReturnCapture
            && absentAppendResult.AppendReceiverAddress == 0x34
            && absentAppendResult.AppendPayloadArgument == 0x3000
            && absentAppendResult.WrapperReturnEax is null);

        check("Any diagnostic return capture is rejected when the diagnostic call is unreached",
            Rejects(
                new SocketFragmentEnqueueCapture(1, 2, 0x0570, false, null),
                nameof(SocketFragmentEnqueueCapture.DiagnosticCallReturnedNormally))
            && Rejects(
                new SocketFragmentEnqueueCapture(1, 2, 0x0570, true, null),
                nameof(SocketFragmentEnqueueCapture.DiagnosticCallReturnedNormally))
            && Rejects(
                new SocketFragmentEnqueueCapture(1, 2, 0x0571, false, 0x55),
                nameof(SocketFragmentEnqueueCapture.AppendCallReturnEax)));
    }

    private static bool InstructionMatchesCapturedSlice(
        SocketFragmentEnqueueInstructionEvidence instruction)
    {
        string[] captured = SocketFragmentEnqueueProjectionRules.CapturedWrapperBytes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] evidence = instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (instruction.RawFileOffset < SocketFragmentEnqueueProjectionRules.EntryRawFileOffset)
            return false;

        int offset = checked((int)(
            instruction.RawFileOffset - SocketFragmentEnqueueProjectionRules.EntryRawFileOffset));
        return offset + evidence.Length <= captured.Length
            && captured.Skip(offset).Take(evidence.Length)
                .SequenceEqual(evidence, StringComparer.OrdinalIgnoreCase);
    }

    private static bool Rejects(
        SocketFragmentEnqueueCapture capture,
        string expectedParameterName)
    {
        try
        {
            SocketFragmentEnqueueProjectionRules.Project(capture);
            return false;
        }
        catch (ArgumentException exception)
        {
            return exception.ParamName == expectedParameterName;
        }
    }
}
