using Firefall.Reconstruction.Startup;

internal static class SocketPayloadRecycleTailChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The recycle tail pins the requested sample, bounded raw/VA anchors, and independent mapping literals",
            SocketPayloadRecycleTailProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && SocketPayloadRecycleTailProjection.WrapperEntryRawFileOffset == 0xBD5C60
            && SocketPayloadRecycleTailProjection.WrapperEntryVirtualAddress == 0xFD6860
            && SocketPayloadRecycleTailProjection.TailStartRawFileOffset == 0xBD5C9C
            && SocketPayloadRecycleTailProjection.TailStartVirtualAddress == 0xFD689C
            && SocketPayloadRecycleTailProjection.TailEndRawFileOffsetExclusive == 0xBD5CD9
            && SocketPayloadRecycleTailProjection.TailEndVirtualAddressExclusive == 0xFD68D9
            && SocketPayloadRecycleTailProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x400C00
            && SocketPayloadRecycleTailProjection.WrapperEntryRawFileOffset
                + SocketPayloadRecycleTailProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketPayloadRecycleTailProjection.WrapperEntryVirtualAddress
            && SocketPayloadRecycleTailProjection.TailStartRawFileOffset
                + SocketPayloadRecycleTailProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketPayloadRecycleTailProjection.TailStartVirtualAddress
            && SocketPayloadRecycleTailProjection.TailEndRawFileOffsetExclusive
                + SocketPayloadRecycleTailProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketPayloadRecycleTailProjection.TailEndVirtualAddressExclusive
            && SocketPayloadRecycleTailProjection.TailEndRawFileOffsetExclusive
                - SocketPayloadRecycleTailProjection.TailStartRawFileOffset == 0x3D);

        check("The capture contains exactly the bounded 0x3D-byte tail and instruction-aligned start",
            SocketPayloadRecycleTailProjection.CapturedTailBytes
                == "C7 43 08 14 FC C5 01 85 DB 75 19 53 68 5A 02 00 00 "
                    + "68 93 43 B7 01 68 6F 43 B7 01 FF 15 A0 5E C1 01 "
                    + "83 C4 10 8B 4D FC 8D 41 10 F0 0F C1 38 53 "
                    + "E8 90 7D 6F 00 5F 5E 5B 8B E5 5D C2 04 00"
            && SocketPayloadRecycleTailProjection.CapturedTailBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x3D
            && SocketPayloadRecycleTailProjection.Instructions.All(InstructionMatchesCapturedSlice)
            && SocketPayloadRecycleTailProjection.Instructions.Single(
                instruction => instruction.RawFileOffset
                    == SocketPayloadRecycleTailProjection.NodeTagWriteRawFileOffset)
                .Bytes == "C7 43 08 14 FC C5 01");

        check("The late tag write precedes the null test and the diagnostic push sequence is exact",
            SocketPayloadRecycleTailProjection.NodeTagValue == 0x01C5FC14
            && SocketPayloadRecycleTailProjection.NodeTagOffset == 8
            && SocketPayloadRecycleTailProjection.NodeNullTestRawFileOffset == 0xBD5CA3
            && SocketPayloadRecycleTailProjection.DiagnosticNodePushRawFileOffset == 0xBD5CA7
            && SocketPayloadRecycleTailProjection.DiagnosticLineValue == 0x25A
            && SocketPayloadRecycleTailProjection.DiagnosticLinePushRawFileOffset == 0xBD5CA8
            && SocketPayloadRecycleTailProjection.DiagnosticArgumentOneVirtualAddress == 0x1B74393
            && SocketPayloadRecycleTailProjection.DiagnosticArgumentOnePushRawFileOffset == 0xBD5CAD
            && SocketPayloadRecycleTailProjection.DiagnosticArgumentTwoVirtualAddress == 0x1B7436F
            && SocketPayloadRecycleTailProjection.DiagnosticArgumentTwoPushRawFileOffset == 0xBD5CB2
            && SocketPayloadRecycleTailProjection.DiagnosticCallRawFileOffset == 0xBD5CB7
            && SocketPayloadRecycleTailProjection.DiagnosticFunctionPointerCellVirtualAddress == 0x1C15EA0
            && SocketPayloadRecycleTailProjection.DiagnosticCallNormalReturnRawFileOffset == 0xBD5CBD);

        check("The saved receiver, wrapping counter XADD, helper target, and normal-return boundaries are pinned",
            SocketPayloadRecycleTailProjection.SavedReceiverReloadRawFileOffset == 0xBD5CC0
            && SocketPayloadRecycleTailProjection.PoolCounterOffset == 0x10
            && SocketPayloadRecycleTailProjection.PoolCounterAddressRawFileOffset == 0xBD5CC3
            && SocketPayloadRecycleTailProjection.PoolCounterXaddRawFileOffset == 0xBD5CC6
            && SocketPayloadRecycleTailProjection.PoolCounterXaddSource == 0xFFFFFFFF
            && SocketPayloadRecycleTailProjection.PoolHelperNodePushRawFileOffset == 0xBD5CCA
            && SocketPayloadRecycleTailProjection.PoolPushCallRawFileOffset == 0xBD5CCB
            && SocketPayloadRecycleTailProjection.PoolPushHelperRawFileOffset == 0x12CDA60
            && SocketPayloadRecycleTailProjection.PoolPushHelperVirtualAddress == 0x16CE660
            && SocketPayloadRecycleTailProjection.PoolPushCallNormalReturnRawFileOffset == 0xBD5CD0
            && SocketPayloadRecycleTailProjection.WrapperReturnRawFileOffset == 0xBD5CD6
            && SocketPayloadRecycleTailProjection.TailEndRawFileOffsetExclusive == 0xBD5CD9
            && SocketPayloadRecycleTailProjection.WrapperReturnStackBytes == 4);

        SocketPayloadRecycleTailPlan nonNullNode =
            SocketPayloadRecycleTailProjection.Project(
                new SocketPayloadRecycleTailCapture(
                    NodeValue: 0xFFFFFFF8,
                    SavedReceiverPointer: 0xFFFFFFF8,
                    DiagnosticReturnedNormally: null,
                    PoolCounterBeforeXadd: 0,
                    PoolPushReturnedNormally: true));
        check("A non-null node skips diagnostics; receiver-plus-counter and node addresses wrap independently",
            nonNullNode.DiagnosticOutcome == SocketPayloadRecycleTailCallOutcome.NotReached
            && nonNullNode.ReceiverOutcome == SocketPayloadRecycleTailReceiverOutcome.Captured
            && nonNullNode.CounterOutcome == SocketPayloadRecycleTailCounterOutcome.Captured
            && nonNullNode.PoolPushOutcome == SocketPayloadRecycleTailCallOutcome.ReturnedNormally
            && nonNullNode.NodeTagAddress == 0
            && nonNullNode.PoolCounterAddress == 8
            && nonNullNode.PoolCounterOldValue == 0
            && nonNullNode.PoolCounterValueAfterSubtract == uint.MaxValue
            && nonNullNode.PoolPushReceiverPointer == 0xFFFFFFF8
            && nonNullNode.PoolPushNodeArgument == 0xFFFFFFF8
            && nonNullNode.WrapperEpilogueReached
            && nonNullNode.StopAtRawFileOffsetExclusive
                == SocketPayloadRecycleTailProjection.TailEndRawFileOffsetExclusive
            && nonNullNode.Actions.Select(action => action.Kind).SequenceEqual(
                new[]
                {
                    SocketPayloadRecycleTailActionKind.WriteNodeTag,
                    SocketPayloadRecycleTailActionKind.TestNodeForNull,
                    SocketPayloadRecycleTailActionKind.BranchAroundDiagnostic,
                    SocketPayloadRecycleTailActionKind.ReloadSavedReceiver,
                    SocketPayloadRecycleTailActionKind.FormPoolCounterAddress,
                    SocketPayloadRecycleTailActionKind.AtomicSubtractPoolCounter,
                    SocketPayloadRecycleTailActionKind.PushNodeForPoolHelper,
                    SocketPayloadRecycleTailActionKind.CallPoolPushHelper,
                    SocketPayloadRecycleTailActionKind.WrapperEpilogue
                }));

        SocketPayloadRecycleTailPlan nullNodeMissingDiagnosticReturn =
            SocketPayloadRecycleTailProjection.Project(
                new SocketPayloadRecycleTailCapture(
                    NodeValue: 0,
                    SavedReceiverPointer: null,
                    DiagnosticReturnedNormally: null,
                    PoolCounterBeforeXadd: null,
                    PoolPushReturnedNormally: null));
        check("A reached diagnostic with missing return capture is distinct from an unreached call",
            nullNodeMissingDiagnosticReturn.DiagnosticOutcome
                == SocketPayloadRecycleTailCallOutcome.MissingReturnCapture
            && nullNodeMissingDiagnosticReturn.ReceiverOutcome
                == SocketPayloadRecycleTailReceiverOutcome.NotReached
            && nullNodeMissingDiagnosticReturn.CounterOutcome
                == SocketPayloadRecycleTailCounterOutcome.NotReached
            && nullNodeMissingDiagnosticReturn.PoolPushOutcome
                == SocketPayloadRecycleTailCallOutcome.NotReached
            && nullNodeMissingDiagnosticReturn.Actions.Any(
                action => action.Kind == SocketPayloadRecycleTailActionKind.CallDiagnostic)
            && !nullNodeMissingDiagnosticReturn.Actions.Any(
                action => action.Kind == SocketPayloadRecycleTailActionKind.AtomicSubtractPoolCounter)
            && !nullNodeMissingDiagnosticReturn.Actions.Any(
                action => action.Kind == SocketPayloadRecycleTailActionKind.ReloadSavedReceiver)
            && nullNodeMissingDiagnosticReturn.StopAtRawFileOffsetExclusive
                == SocketPayloadRecycleTailProjection.DiagnosticStackCleanupRawFileOffset
            && !nullNodeMissingDiagnosticReturn.WrapperEpilogueReached);

        SocketPayloadRecycleTailPlan nullNodeDiagnosticDidNotReturn =
            SocketPayloadRecycleTailProjection.Project(
                new SocketPayloadRecycleTailCapture(
                    NodeValue: 0,
                    SavedReceiverPointer: null,
                    DiagnosticReturnedNormally: false,
                    PoolCounterBeforeXadd: null,
                    PoolPushReturnedNormally: null));
        check("A diagnostic that did not return normally blocks the counter and helper tail",
            nullNodeDiagnosticDidNotReturn.DiagnosticOutcome
                == SocketPayloadRecycleTailCallOutcome.DidNotReturnNormally
            && nullNodeDiagnosticDidNotReturn.ReceiverOutcome
                == SocketPayloadRecycleTailReceiverOutcome.NotReached
            && nullNodeDiagnosticDidNotReturn.CounterOutcome
                == SocketPayloadRecycleTailCounterOutcome.NotReached
            && nullNodeDiagnosticDidNotReturn.PoolPushOutcome
                == SocketPayloadRecycleTailCallOutcome.NotReached
            && nullNodeDiagnosticDidNotReturn.StopAtRawFileOffsetExclusive
                == SocketPayloadRecycleTailProjection.DiagnosticCallNormalReturnRawFileOffset);

        SocketPayloadRecycleTailPlan nullNodeDiagnosticReturns =
            SocketPayloadRecycleTailProjection.Project(
                new SocketPayloadRecycleTailCapture(
                    NodeValue: 0,
                    SavedReceiverPointer: 0xFFFFFFFC,
                    DiagnosticReturnedNormally: true,
                    PoolCounterBeforeXadd: 0x80000000,
                    PoolPushReturnedNormally: true));
        check("A normally returning diagnostic preserves push order, then reaches helper with the saved receiver",
            nullNodeDiagnosticReturns.DiagnosticOutcome
                == SocketPayloadRecycleTailCallOutcome.ReturnedNormally
            && nullNodeDiagnosticReturns.ReceiverOutcome
                == SocketPayloadRecycleTailReceiverOutcome.Captured
            && nullNodeDiagnosticReturns.PoolCounterAddress == 0x0C
            && nullNodeDiagnosticReturns.PoolCounterValueAfterSubtract == 0x7FFFFFFF
            && nullNodeDiagnosticReturns.PoolPushReceiverPointer == 0xFFFFFFFC
            && nullNodeDiagnosticReturns.PoolPushNodeArgument == 0
            && nullNodeDiagnosticReturns.Actions
                .Where(action => action.Kind is
                    SocketPayloadRecycleTailActionKind.PushDiagnosticNode
                    or SocketPayloadRecycleTailActionKind.PushDiagnosticLine
                    or SocketPayloadRecycleTailActionKind.PushDiagnosticArgumentOne
                    or SocketPayloadRecycleTailActionKind.PushDiagnosticArgumentTwo)
                .Select(action => action.CapturedValue)
                .SequenceEqual(new uint?[]
                {
                    0,
                    0x25A,
                    0x1B74393,
                    0x1B7436F
                })
            && nullNodeDiagnosticReturns.Actions.Single(
                action => action.Kind == SocketPayloadRecycleTailActionKind.CallPoolPushHelper)
                .TargetRawFileOffset == 0x12CDA60);

        SocketPayloadRecycleTailPlan missingReceiver =
            SocketPayloadRecycleTailProjection.Project(
                new SocketPayloadRecycleTailCapture(
                    NodeValue: 0x2000,
                    SavedReceiverPointer: null,
                    DiagnosticReturnedNormally: null,
                    PoolCounterBeforeXadd: null,
                    PoolPushReturnedNormally: null));
        check("A missing saved receiver stops at its reload without emitting counter or helper actions",
            missingReceiver.DiagnosticOutcome
                == SocketPayloadRecycleTailCallOutcome.NotReached
            && missingReceiver.ReceiverOutcome
                == SocketPayloadRecycleTailReceiverOutcome.MissingCapture
            && missingReceiver.CounterOutcome == SocketPayloadRecycleTailCounterOutcome.PathUndetermined
            && missingReceiver.PoolPushOutcome
                == SocketPayloadRecycleTailCallOutcome.PathUndetermined
            && missingReceiver.PoolCounterAddress is null
            && missingReceiver.PoolPushReceiverPointer is null
            && missingReceiver.PoolPushNodeArgument is null
            && !missingReceiver.WrapperEpilogueReached
            && missingReceiver.StopAtRawFileOffsetExclusive
                == SocketPayloadRecycleTailProjection.PoolCounterAddressRawFileOffset
            && missingReceiver.Actions.Select(action => action.Kind).SequenceEqual(
                new[]
                {
                    SocketPayloadRecycleTailActionKind.WriteNodeTag,
                    SocketPayloadRecycleTailActionKind.TestNodeForNull,
                    SocketPayloadRecycleTailActionKind.BranchAroundDiagnostic,
                    SocketPayloadRecycleTailActionKind.ReloadSavedReceiver
                }));

        SocketPayloadRecycleTailPlan missingCounter =
            SocketPayloadRecycleTailProjection.Project(
                new SocketPayloadRecycleTailCapture(
                    NodeValue: 0x2000,
                    SavedReceiverPointer: 0x3000,
                    DiagnosticReturnedNormally: null,
                    PoolCounterBeforeXadd: null,
                    PoolPushReturnedNormally: null));
        check("A missing counter capture stops after XADD, before pushing or calling the helper",
            missingCounter.ReceiverOutcome == SocketPayloadRecycleTailReceiverOutcome.Captured
            && missingCounter.CounterOutcome
                == SocketPayloadRecycleTailCounterOutcome.MissingValueCapture
            && missingCounter.PoolCounterAddress == 0x3010
            && missingCounter.PoolPushOutcome == SocketPayloadRecycleTailCallOutcome.PathUndetermined
            && !missingCounter.Actions.Any(
                action => action.Kind == SocketPayloadRecycleTailActionKind.PushNodeForPoolHelper)
            && !missingCounter.Actions.Any(
                action => action.Kind == SocketPayloadRecycleTailActionKind.CallPoolPushHelper)
            && missingCounter.StopAtRawFileOffsetExclusive
                == SocketPayloadRecycleTailProjection.PoolHelperNodePushRawFileOffset);

        SocketPayloadRecycleTailPlan missingHelperReturn =
            SocketPayloadRecycleTailProjection.Project(
                new SocketPayloadRecycleTailCapture(
                    NodeValue: 0x2000,
                    SavedReceiverPointer: 0x3000,
                    DiagnosticReturnedNormally: null,
                    PoolCounterBeforeXadd: 1,
                    PoolPushReturnedNormally: null));
        check("A reached pool-push helper with missing normal-return capture stops before the epilogue",
            missingHelperReturn.CounterOutcome == SocketPayloadRecycleTailCounterOutcome.Captured
            && missingHelperReturn.PoolPushOutcome
                == SocketPayloadRecycleTailCallOutcome.MissingReturnCapture
            && missingHelperReturn.Actions.Any(
                action => action.Kind == SocketPayloadRecycleTailActionKind.CallPoolPushHelper)
            && !missingHelperReturn.Actions.Any(
                action => action.Kind == SocketPayloadRecycleTailActionKind.WrapperEpilogue)
            && !missingHelperReturn.WrapperEpilogueReached
            && missingHelperReturn.StopAtRawFileOffsetExclusive
                == SocketPayloadRecycleTailProjection.PoolPushCallNormalReturnRawFileOffset);

        SocketPayloadRecycleTailPlan helperDidNotReturn =
            SocketPayloadRecycleTailProjection.Project(
                new SocketPayloadRecycleTailCapture(
                    NodeValue: 0x2000,
                    SavedReceiverPointer: 0x3000,
                    DiagnosticReturnedNormally: null,
                    PoolCounterBeforeXadd: 1,
                    PoolPushReturnedNormally: false));
        check("A helper that did not return normally is distinct from a missing return capture",
            helperDidNotReturn.PoolPushOutcome
                == SocketPayloadRecycleTailCallOutcome.DidNotReturnNormally
            && !helperDidNotReturn.WrapperEpilogueReached
            && helperDidNotReturn.StopAtRawFileOffsetExclusive
                == SocketPayloadRecycleTailProjection.PoolPushCallNormalReturnRawFileOffset);

        SocketPayloadRecycleTailPlan missingNode =
            SocketPayloadRecycleTailProjection.Project(
                new SocketPayloadRecycleTailCapture(
                    NodeValue: null,
                    SavedReceiverPointer: null,
                    DiagnosticReturnedNormally: null,
                    PoolCounterBeforeXadd: null,
                    PoolPushReturnedNormally: null));
        check("A missing node capture leaves the branch path undetermined rather than calling it unreached",
            missingNode.DiagnosticOutcome == SocketPayloadRecycleTailCallOutcome.PathUndetermined
            && missingNode.ReceiverOutcome == SocketPayloadRecycleTailReceiverOutcome.PathUndetermined
            && missingNode.CounterOutcome == SocketPayloadRecycleTailCounterOutcome.PathUndetermined
            && missingNode.PoolPushOutcome == SocketPayloadRecycleTailCallOutcome.PathUndetermined
            && missingNode.NodeTagAddress is null
            && !missingNode.WrapperEpilogueReached
            && missingNode.StopAtRawFileOffsetExclusive
                == SocketPayloadRecycleTailProjection.NodeNullTestRawFileOffset
            && missingNode.Actions.Select(action => action.Kind).SequenceEqual(
                new[] { SocketPayloadRecycleTailActionKind.WriteNodeTag }));

        check("Every later capture is rejected after a missing node or non-returning diagnostic",
            Rejects(
                new SocketPayloadRecycleTailCapture(null, 0x2000, null, null, null),
                nameof(SocketPayloadRecycleTailCapture.NodeValue))
            && Rejects(
                new SocketPayloadRecycleTailCapture(null, null, true, null, null),
                nameof(SocketPayloadRecycleTailCapture.NodeValue))
            && Rejects(
                new SocketPayloadRecycleTailCapture(null, null, null, 1, null),
                nameof(SocketPayloadRecycleTailCapture.NodeValue))
            && Rejects(
                new SocketPayloadRecycleTailCapture(null, null, null, null, true),
                nameof(SocketPayloadRecycleTailCapture.NodeValue))
            && Rejects(
                new SocketPayloadRecycleTailCapture(
                    0, 0x2000, null, null, null),
                nameof(SocketPayloadRecycleTailCapture.DiagnosticReturnedNormally))
            && Rejects(
                new SocketPayloadRecycleTailCapture(
                    0, 0x2000, false, null, null),
                nameof(SocketPayloadRecycleTailCapture.DiagnosticReturnedNormally))
            && Rejects(
                new SocketPayloadRecycleTailCapture(
                    0, 0x2000, false, 1, null),
                nameof(SocketPayloadRecycleTailCapture.DiagnosticReturnedNormally))
            && Rejects(
                new SocketPayloadRecycleTailCapture(
                    0, 0x2000, false, null, true),
                nameof(SocketPayloadRecycleTailCapture.DiagnosticReturnedNormally)));

        check("Receiver and counter gaps reject captures from every later stage",
            Rejects(
                new SocketPayloadRecycleTailCapture(0x2000, null, null, 1, null),
                nameof(SocketPayloadRecycleTailCapture.SavedReceiverPointer))
            && Rejects(
                new SocketPayloadRecycleTailCapture(0x2000, null, null, null, true),
                nameof(SocketPayloadRecycleTailCapture.SavedReceiverPointer))
            && Rejects(
                new SocketPayloadRecycleTailCapture(0x2000, 0x3000, null, null, true),
                nameof(SocketPayloadRecycleTailCapture.PoolCounterBeforeXadd)));
    }

    private static bool InstructionMatchesCapturedSlice(
        SocketPayloadRecycleTailInstructionEvidence instruction)
    {
        string[] captured = SocketPayloadRecycleTailProjection.CapturedTailBytes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] evidence = instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (instruction.RawFileOffset < SocketPayloadRecycleTailProjection.TailStartRawFileOffset)
        {
            return false;
        }

        int offset = checked((int)(
            instruction.RawFileOffset - SocketPayloadRecycleTailProjection.TailStartRawFileOffset));
        return offset + evidence.Length <= captured.Length
            && captured.Skip(offset).Take(evidence.Length)
                .SequenceEqual(evidence, StringComparer.OrdinalIgnoreCase);
    }

    private static bool Rejects(
        SocketPayloadRecycleTailCapture capture,
        string expectedParameterName)
    {
        try
        {
            SocketPayloadRecycleTailProjection.Project(capture);
            return false;
        }
        catch (ArgumentException exception)
        {
            return exception.ParamName == expectedParameterName;
        }
    }
}
