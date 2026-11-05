using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveCallbackResultChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample, raw range, VA mapping, branch target, and exact bounded bytes match",
            ReliableReceiveCallbackResultProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ReliableReceiveCallbackResultProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveCallbackResultProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReliableReceiveCallbackResultProjection.EntryRawFileOffset == 0x00BE53A2
            && ReliableReceiveCallbackResultProjection.EntryVirtualAddress == 0x00FE5FA2
            && ReliableReceiveCallbackResultProjection.DiagnosticCallRawFileOffset == 0x00BE53B6
            && ReliableReceiveCallbackResultProjection.DiagnosticCallVirtualAddress == 0x00FE5FB6
            && ReliableReceiveCallbackResultProjection.DiagnosticCallContinuationRawFileOffset
                == 0x00BE53BC
            && ReliableReceiveCallbackResultProjection.DiagnosticCallContinuationVirtualAddress
                == 0x00FE5FBC
            && ReliableReceiveCallbackResultProjection.NonzeroEaxBranchTargetRawFileOffset
                == 0x00BE53BF
            && ReliableReceiveCallbackResultProjection.NonzeroEaxBranchTargetVirtualAddress
                == 0x00FE5FBF
            && ReliableReceiveCallbackResultProjection.EndRawFileOffsetExclusive == 0x00BE53BF
            && ReliableReceiveCallbackResultProjection.DiagnosticCallTargetSlotAddress
                == 0x01C15EA0
            && ReliableReceiveCallbackResultProjection.ExplicitStackPushCount == 4
            && ReliableReceiveCallbackResultProjection.CapturedWindowBytes
                == "85 C0 75 19 50 68 05 02 00 00 68 27 8B B5 01 68 DA 8B B5 01 FF 15 A0 5E C1 01"
            && ReliableReceiveCallbackResultProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 26
            && InstructionsMatch());

        ReliableReceiveCallbackResultPlan zeroResult =
            ReliableReceiveCallbackResultProjection.Project(new(true, 0, true));
        check("A normally returned zero pushes four exact values and stops at the opaque handoff",
            zeroResult.Outcome
                == ReliableReceiveCallbackResultOutcome.ZeroOpaqueHandoffReturnedNormally
            && zeroResult.PriorCallReturnedNormally == true
            && zeroResult.ReturnEax == 0
            && zeroResult.BranchTaken == false
            && zeroResult.PushedValuesInInstructionOrder.SequenceEqual(
                new uint[] { 0, 0x205, 0x01B58B27, 0x01B58BDA })
            && zeroResult.DiagnosticCallReached
            && zeroResult.DiagnosticCallTargetSlotAddress == 0x01C15EA0
            && zeroResult.DiagnosticCallContinuationRawFileOffset == 0x00BE53BC
            && zeroResult.SelectedDestinationRawFileOffset == 0x00BE53BC
            && zeroResult.StopAtRawFileOffsetExclusive == 0x00BE53BC);

        ReliableReceiveCallbackResultPlan nonzeroResult =
            ReliableReceiveCallbackResultProjection.Project(new(true, uint.MaxValue, null));
        check("A nonzero FFFFFFFF result branches directly to raw 0xBE53BF with no call or pushes",
            nonzeroResult.Outcome == ReliableReceiveCallbackResultOutcome.NonzeroEaxBranch
            && nonzeroResult.PriorCallReturnedNormally == true
            && nonzeroResult.ReturnEax == uint.MaxValue
            && nonzeroResult.BranchTaken == true
            && nonzeroResult.PushedValuesInInstructionOrder.IsEmpty
            && !nonzeroResult.DiagnosticCallReached
            && nonzeroResult.DiagnosticCallTargetSlotAddress is null
            && nonzeroResult.SelectedDestinationRawFileOffset == 0x00BE53BF
            && nonzeroResult.StopAtRawFileOffsetExclusive == 0x00BE53BF);

        ReliableReceiveCallbackResultPlan missingPriorReturn =
            ReliableReceiveCallbackResultProjection.Project(new(null, null, null));
        ReliableReceiveCallbackResultPlan missingEax =
            ReliableReceiveCallbackResultProjection.Project(new(true, null, null));
        ReliableReceiveCallbackResultPlan missingDiagnosticReturn =
            ReliableReceiveCallbackResultProjection.Project(new(true, 0, null));
        check("Each required return capture is strict and stops at its earliest unresolved boundary",
            missingPriorReturn.Outcome
                == ReliableReceiveCallbackResultOutcome.MissingPriorCallReturn
            && missingPriorReturn.StopAtRawFileOffsetExclusive == 0x00BE53A2
            && missingEax.Outcome == ReliableReceiveCallbackResultOutcome.MissingReturnEax
            && missingEax.PriorCallReturnedNormally == true
            && missingEax.ReturnEax is null
            && missingEax.StopAtRawFileOffsetExclusive == 0x00BE53A2
            && missingDiagnosticReturn.Outcome
                == ReliableReceiveCallbackResultOutcome.MissingDiagnosticCallReturn
            && missingDiagnosticReturn.DiagnosticCallReached
            && missingDiagnosticReturn.StopAtRawFileOffsetExclusive == 0x00BE53BC);

        ReliableReceiveCallbackResultPlan priorCallDidNotReturn =
            ReliableReceiveCallbackResultProjection.Project(new(false, null, null));
        ReliableReceiveCallbackResultPlan diagnosticCallDidNotReturn =
            ReliableReceiveCallbackResultProjection.Project(new(true, 0, false));
        check("A prior or diagnostic call that did not return normally cannot advance past its boundary",
            priorCallDidNotReturn.Outcome
                == ReliableReceiveCallbackResultOutcome.PriorCallDidNotReturnNormally
            && priorCallDidNotReturn.PriorCallReturnedNormally == false
            && priorCallDidNotReturn.ReturnEax is null
            && !priorCallDidNotReturn.DiagnosticCallReached
            && priorCallDidNotReturn.StopAtRawFileOffsetExclusive == 0x00BE53A2
            && diagnosticCallDidNotReturn.Outcome
                == ReliableReceiveCallbackResultOutcome.DiagnosticCallDidNotReturnNormally
            && diagnosticCallDidNotReturn.DiagnosticCallReached
            && diagnosticCallDidNotReturn.SelectedDestinationRawFileOffset is null
            && diagnosticCallDidNotReturn.StopAtRawFileOffsetExclusive == 0x00BE53BC);

        ReliableReceiveCallbackResultPlan laterAfterMissingPrior =
            ReliableReceiveCallbackResultProjection.Project(new(null, 0, true));
        ReliableReceiveCallbackResultPlan diagnosticOnNonzero =
            ReliableReceiveCallbackResultProjection.Project(new(true, uint.MaxValue, true));
        check("Captures after an unreached call or nonzero branch are rejected as unreached",
            laterAfterMissingPrior.Outcome
                == ReliableReceiveCallbackResultOutcome.UnreachedCaptureProvided
            && laterAfterMissingPrior.ReturnEax is null
            && laterAfterMissingPrior.PriorCallReturnedNormally is null
            && !laterAfterMissingPrior.DiagnosticCallReached
            && diagnosticOnNonzero.Outcome
                == ReliableReceiveCallbackResultOutcome.UnreachedCaptureProvided
            && diagnosticOnNonzero.BranchTaken == true
            && !diagnosticOnNonzero.DiagnosticCallReached
            && diagnosticOnNonzero.PushedValuesInInstructionOrder.IsEmpty);
    }

    private static bool InstructionsMatch()
    {
        var expected =
            new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
            {
                (0x00BE53A2, 0x00FE5FA2, "85 C0", null, null),
                (0x00BE53A4, 0x00FE5FA4, "75 19", 0x00BE53BF, 0x00FE5FBF),
                (0x00BE53A6, 0x00FE5FA6,
                    "50 68 05 02 00 00 68 27 8B B5 01 68 DA 8B B5 01", null, null),
                (0x00BE53B6, 0x00FE5FB6, "FF 15 A0 5E C1 01", null, null)
            };
        return ReliableReceiveCallbackResultProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                        instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected)
            && ReliableReceiveCallbackResultProjection.RelativeBranchTarget(0x00BE53A4, 0x19)
                == 0x00BE53BF;
    }
}
