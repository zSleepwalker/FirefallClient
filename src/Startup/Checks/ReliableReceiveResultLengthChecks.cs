using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveResultLengthChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The zero-route cleanup and length entry have the exact mapped instruction evidence",
            ReliableReceiveResultLengthProjection.SamplePath
                == ReliableReceiveCallbackResultProjection.SamplePath
            && ReliableReceiveResultLengthProjection.SampleSha256
                == ReliableReceiveCallbackResultProjection.SampleSha256
            && ReliableReceiveResultLengthProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReliableReceiveResultLengthProjection.DiagnosticCleanupRawFileOffset
                == 0x00BE53BC
            && ReliableReceiveResultLengthProjection.DiagnosticCleanupStackBytes == 0x10
            && ReliableReceiveResultLengthProjection.LengthEntryRawFileOffset == 0x00BE53BF
            && ReliableReceiveResultLengthProjection.LengthEntryVirtualAddress == 0x00FE5FBF
            && ReliableReceiveResultLengthProjection.EndRawFileOffsetExclusive == 0x00BE53DD
            && ReliableReceiveResultLengthProjection.DiagnosticCleanupBytes == "83 C4 10"
            && ReliableReceiveResultLengthProjection.Instructions.SequenceEqual(
                new[]
                {
                    new ReliableReceiveResultLengthInstructionEvidence(
                        "After a normal zero-result diagnostic return, add 0x10 to ESP before the length entry",
                        0x00BE53BC,
                        0x00FE5FBC,
                        "83 C4 10")
                }));

        ReliableReceiveLengthCallbackCapture completeLength =
            new(true, 0x10203040, 7, 0x11223344, 0x55667788, 0x99AABBCC, true);
        ReliableReceiveResultLengthPlan diagnosticZero =
            ReliableReceiveResultLengthProjection.Project(
                new(true, 0, true),
                completeLength);
        check("A normal zero result completes the diagnostic handoff and applies cleanup before length",
            diagnosticZero.ResultPlan.Outcome
                == ReliableReceiveCallbackResultOutcome.ZeroOpaqueHandoffReturnedNormally
            && diagnosticZero.ResultAdmittedLengthBoundary
            && diagnosticZero.DiagnosticCleanupProjected
            && diagnosticZero.DiagnosticCleanupStackBytes == 0x10
            && diagnosticZero.LengthEntryRawFileOffset == 0x00BE53BF
            && diagnosticZero.LengthPlan?.Outcome
                == ReliableReceiveLengthCallbackOutcome.IndirectCallReturnedNormally
            && diagnosticZero.LengthPlan?.ComputedLength == 3
            && diagnosticZero.Outcome
                == ReliableReceiveResultLengthOutcome.LengthCallbackReturnedNormally
            && diagnosticZero.SelectedDestinationRawFileOffset == 0x00BE53DD
            && diagnosticZero.StopAtRawFileOffsetExclusive == 0x00BE53DD);

        ReliableReceiveResultLengthPlan nonzeroResult =
            ReliableReceiveResultLengthProjection.Project(
                new(true, 0xA5A5A5A5, null),
                completeLength with { PayloadHeader = 2 });
        check("A nonzero result skips diagnostic cleanup and uses the independently captured WORD for length",
            nonzeroResult.ResultPlan.Outcome
                == ReliableReceiveCallbackResultOutcome.NonzeroEaxBranch
            && nonzeroResult.ResultAdmittedLengthBoundary
            && !nonzeroResult.DiagnosticCleanupProjected
            && nonzeroResult.DiagnosticCleanupStackBytes is null
            && nonzeroResult.LengthEntryRawFileOffset == 0x00BE53BF
            && nonzeroResult.LengthPlan?.Outcome
                == ReliableReceiveLengthCallbackOutcome.IndirectCallReturnedNormally
            && nonzeroResult.LengthPlan?.ComputedLength == 0xFFFFFFFE
            && nonzeroResult.Outcome
                == ReliableReceiveResultLengthOutcome.LengthCallbackReturnedNormally);

        ReliableReceiveResultLengthPlan missingDiagnosticReturn =
            ReliableReceiveResultLengthProjection.Project(
                new(true, 0, null),
                completeLength);
        ReliableReceiveResultLengthPlan unreachedResultAtBranchBoundary =
            ReliableReceiveResultLengthProjection.Project(
                new(true, 1, true),
                completeLength);
        check("Only completed result outcomes admit length, even when an unreached result stops at its branch destination",
            missingDiagnosticReturn.ResultPlan.Outcome
                == ReliableReceiveCallbackResultOutcome.MissingDiagnosticCallReturn
            && missingDiagnosticReturn.ResultPlan.StopAtRawFileOffsetExclusive
                == 0x00BE53BC
            && !missingDiagnosticReturn.ResultAdmittedLengthBoundary
            && !missingDiagnosticReturn.DiagnosticCleanupProjected
            && missingDiagnosticReturn.LengthPlan is null
            && missingDiagnosticReturn.StopAtRawFileOffsetExclusive == 0x00BE53BC
            && unreachedResultAtBranchBoundary.ResultPlan.Outcome
                == ReliableReceiveCallbackResultOutcome.UnreachedCaptureProvided
            && unreachedResultAtBranchBoundary.ResultPlan.StopAtRawFileOffsetExclusive
                == 0x00BE53BF
            && !unreachedResultAtBranchBoundary.ResultAdmittedLengthBoundary
            && unreachedResultAtBranchBoundary.LengthPlan is null
            && unreachedResultAtBranchBoundary.StopAtRawFileOffsetExclusive == 0x00BE53BF);

        ReliableReceiveResultLengthPlan missingLength =
            ReliableReceiveResultLengthProjection.Project(new(true, 0, true), null);
        check("A missing length capture preserves completed zero-route cleanup",
            missingLength.Outcome == ReliableReceiveResultLengthOutcome.MissingLengthCapture
            && missingLength.ResultAdmittedLengthBoundary
            && missingLength.DiagnosticCleanupProjected
            && missingLength.DiagnosticCleanupStackBytes == 0x10
            && missingLength.LengthPlan is null
            && missingLength.SelectedDestinationRawFileOffset is null
            && missingLength.StopAtRawFileOffsetExclusive == 0x00BE53BF);

        ReliableReceiveLengthCallbackCapture invalidBoundary =
            completeLength with { EntryBoundaryValidated = false };
        ReliableReceiveResultLengthPlan invalidLengthBoundary =
            ReliableReceiveResultLengthProjection.Project(
                new(true, 0, true),
                invalidBoundary);
        ReliableReceiveResultLengthPlan invalidBoundaryAfterNonzero =
            ReliableReceiveResultLengthProjection.Project(
                new(true, 1, null),
                invalidBoundary);
        check("Length requires its validated entry boundary and rejects every later capture when that gate is false",
            invalidLengthBoundary.Outcome
                == ReliableReceiveResultLengthOutcome.LengthEntryBoundaryNotValidated
            && invalidLengthBoundary.DiagnosticCleanupProjected
            && invalidLengthBoundary.DiagnosticCleanupStackBytes == 0x10
            && invalidLengthBoundary.LengthPlan?.Outcome
                == ReliableReceiveLengthCallbackOutcome.EntryGateNotMet
            && invalidLengthBoundary.LengthPlan?.UnreachedPayloadHeaderCaptureProvided == true
            && invalidLengthBoundary.LengthPlan?.UnreachedPayloadDwordAt8CaptureProvided == true
            && invalidLengthBoundary.LengthPlan?.UnreachedPayloadDwordAt4CaptureProvided == true
            && invalidLengthBoundary.LengthPlan?.UnreachedIndirectTargetCaptureProvided == true
            && invalidLengthBoundary.LengthPlan?.UnreachedNormalReturnCaptureProvided == true
            && invalidLengthBoundary.LengthPlan?.PayloadHeaderRead == false
            && invalidLengthBoundary.LengthPlan?.StopAtRawFileOffsetExclusive == 0x00BE53BF
            && invalidLengthBoundary.SelectedDestinationRawFileOffset is null
            && invalidBoundaryAfterNonzero.LengthPlan?.Outcome
                == ReliableReceiveLengthCallbackOutcome.EntryGateNotMet
            && !invalidBoundaryAfterNonzero.DiagnosticCleanupProjected
            && invalidBoundaryAfterNonzero.DiagnosticCleanupStackBytes is null);

        ReliableReceiveResultLengthPlan underflowAndPartialPush =
            ReliableReceiveResultLengthProjection.Project(
                new(true, 0, true),
                new(true, 0x10203040, 0, 0x55667788, null, null, null));
        check("Header zero underflows to FFFFFFFC, with only the first length push projected before the missing second push",
            underflowAndPartialPush.Outcome
                == ReliableReceiveResultLengthOutcome.LengthCallbackStopped
            && underflowAndPartialPush.DiagnosticCleanupProjected
            && underflowAndPartialPush.LengthPlan?.Outcome
                == ReliableReceiveLengthCallbackOutcome.MissingPayloadDwordAt4
            && underflowAndPartialPush.LengthPlan?.ComputedLength == uint.MaxValue - 3
            && underflowAndPartialPush.LengthPlan?.PayloadDwordAt8Read == true
            && underflowAndPartialPush.LengthPlan?.StackPushes.SequenceEqual(
                new uint[] { uint.MaxValue - 3 }) == true
            && underflowAndPartialPush.LengthPlan?.CallStackArguments.IsEmpty == true
            && underflowAndPartialPush.LengthPlan?.StopAtRawFileOffsetExclusive == 0x00BE53D7
            && underflowAndPartialPush.SelectedDestinationRawFileOffset is null
            && underflowAndPartialPush.StopAtRawFileOffsetExclusive == 0x00BE53D7);
    }
}
