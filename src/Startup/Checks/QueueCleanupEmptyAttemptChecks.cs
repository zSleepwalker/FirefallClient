using System.Buffers.Binary;
using Firefall.Reconstruction.Startup;

internal static class QueueCleanupEmptyAttemptChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Queue-cleanup attempt anchors pin the sample, raw loop bytes, VA mapping, and queue-pop target",
            QueueCleanupEmptyAttemptProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && QueueCleanupEmptyAttemptProjection.LoopHeaderRawFileOffset == 0x12C0348
            && QueueCleanupEmptyAttemptProjection.LoopEndRawFileOffsetExclusive == 0x12C0382
            && QueueCleanupEmptyAttemptProjection.LoopEndRawFileOffsetExclusive
                - QueueCleanupEmptyAttemptProjection.LoopHeaderRawFileOffset == 0x3A
            && QueueCleanupEmptyAttemptProjection.CapturedLoopBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x3A
            && QueueCleanupEmptyAttemptProjection.LoopHeaderVirtualAddress == 0x16C0F48
            && QueueCleanupEmptyAttemptProjection.LoopEndVirtualAddressExclusive == 0x16C0F82
            && QueueCleanupEmptyAttemptProjection.QueuePopTargetRawFileOffset == 0x12CE400
            && QueueCleanupEmptyAttemptProjection.QueuePopTargetVirtualAddress == 0x16CF000
            && QueueCleanupEmptyAttemptProjection.NullHeadReporterFunctionPointerSlotVirtualAddress
                == 0x1C15EA0
            && QueueCleanupEmptyAttemptProjection.NullHeadDiagnosticId == 0x324
            && QueueCleanupEmptyAttemptProjection.CapturedLoopBytes
                == "8B 5E 04 8B 3E 85 FF 75 19 57 68 24 03 00 00 "
                    + "68 57 43 B7 01 68 33 43 B7 01 FF 15 A0 5E C1 01 "
                    + "83 C4 10 83 3F 00 0F 94 C0 3B 5E 04 75 D3 84 C0 "
                    + "75 09 8B CE E8 80 E0 00 00 EB C6");

        check("Instruction evidence preserves independent captures, both branch targets, and direct queue-pop call target",
            QueueCleanupEmptyAttemptProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.TargetRawFileOffset, instruction.TargetVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    uint? TargetRawFileOffset,
                    uint? TargetVirtualAddress)[]
                {
                    (0x12C0348, "8B 5E 04", null, null),
                    (0x12C034B, "8B 3E", null, null),
                    (0x12C034D, "85 FF", null, null),
                    (0x12C034F, "75 19", 0x12C036A, 0x16C0F6A),
                    (0x12C0352, "68 24 03 00 00", null, null),
                    (0x12C0361, "FF 15 A0 5E C1 01", null, 0x1C15EA0),
                    (0x12C036A, "83 3F 00", null, null),
                    (0x12C036D, "0F 94 C0", null, null),
                    (0x12C0370, "3B 5E 04", null, null),
                    (0x12C0373, "75 D3", 0x12C0348, 0x16C0F48),
                    (0x12C0375, "84 C0", null, null),
                    (0x12C0377, "75 09", 0x12C0382, 0x16C0F82),
                    (0x12C0379, "8B CE", null, null),
                    (0x12C037B, "E8 80 E0 00 00", 0x12CE400, 0x16CF000),
                    (0x12C0380, "EB C6", 0x12C0348, 0x16C0F48)
                })
            && QueueCleanupEmptyAttemptProjection.Instructions.All(instruction =>
                HasLoopSlice(instruction.RawFileOffset, instruction.Bytes))
            && ShortBranchTargets(0x12C034F, "75 19", 0x12C036A)
            && ShortBranchTargets(0x12C0373, "75 D3", 0x12C0348)
            && ShortBranchTargets(0x12C0377, "75 09", 0x12C0382)
            && ShortBranchTargets(0x12C0380, "EB C6", 0x12C0348)
            && RelativeCallTargets(
                QueueCleanupEmptyAttemptProjection.QueuePopCallRawFileOffset,
                "E8 80 E0 00 00",
                QueueCleanupEmptyAttemptProjection.QueuePopTargetRawFileOffset)
            && QueueCleanupEmptyAttemptProjection.Instructions.All(instruction =>
                instruction.TargetRawFileOffset is null
                || instruction.TargetVirtualAddress
                    == instruction.TargetRawFileOffset
                        + QueueCleanupEmptyAttemptProjection.CodeVirtualAddressDeltaFromRawFileOffset
                || instruction.RawFileOffset
                    == QueueCleanupEmptyAttemptProjection.NullHeadReporterCallRawFileOffset));

        QueueCleanupEmptyAttemptPlan mismatchWithEmptyHeadNext =
            QueueCleanupEmptyAttemptProjection.Project(new QueueCleanupEmptyAttemptCapture(
                ReceiverPointer: 0x1000,
                CapturedInitialTagFromReceiverPlus4: 0x11223344,
                CapturedInitialHeadFromReceiver: 0x2000,
                NullHeadReporterReturned: null,
                CapturedHeadNextValue: 0,
                CapturedLaterTagFromReceiverPlus4: 0x55667788,
                QueuePopReturnedNormally: null));
        check("A later tag mismatch retries before using the independently captured empty-head result",
            mismatchWithEmptyHeadNext.Outcome
                == QueueCleanupEmptyAttemptOutcome.TagMismatchRetriesLoop
            && mismatchWithEmptyHeadNext.HeadNextWasZero == true
            && mismatchWithEmptyHeadNext.LaterTagMatchedInitialTag == false
            && mismatchWithEmptyHeadNext.Actions[^1]
                == new QueueCleanupEmptyAttemptAction(
                    QueueCleanupEmptyAttemptActionKind.RetryAtLoopHeader,
                    0x12C0373,
                    ConditionMet: true,
                    TargetRawFileOffset: 0x12C0348,
                    TargetVirtualAddress: 0x16C0F48)
            && mismatchWithEmptyHeadNext.Actions.All(action =>
                action.Kind != QueueCleanupEmptyAttemptActionKind.EnterSecondaryDrainBoundary
                && action.Kind != QueueCleanupEmptyAttemptActionKind.CallQueuePop));

        QueueCleanupEmptyAttemptPlan emptyBoundary =
            QueueCleanupEmptyAttemptProjection.Project(new QueueCleanupEmptyAttemptCapture(
                ReceiverPointer: 0x1000,
                CapturedInitialTagFromReceiverPlus4: 0xAABBCCDD,
                CapturedInitialHeadFromReceiver: 0x2000,
                NullHeadReporterReturned: null,
                CapturedHeadNextValue: 0,
                CapturedLaterTagFromReceiverPlus4: 0xAABBCCDD,
                QueuePopReturnedNormally: null));
        check("Matching tags and a zero head-next reach only the secondary-drain boundary",
            emptyBoundary.Outcome
                == QueueCleanupEmptyAttemptOutcome.EmptyHeadNextEntersSecondaryDrainBoundary
            && emptyBoundary.Actions[^1].RawFileOffset == 0x12C0382
            && emptyBoundary.Actions.All(action =>
                action.Kind != QueueCleanupEmptyAttemptActionKind.CallQueuePop));

        QueueCleanupEmptyAttemptPlan popCallBoundary =
            QueueCleanupEmptyAttemptProjection.Project(new QueueCleanupEmptyAttemptCapture(
                ReceiverPointer: 0x3000,
                CapturedInitialTagFromReceiverPlus4: 7,
                CapturedInitialHeadFromReceiver: 0x4000,
                NullHeadReporterReturned: null,
                CapturedHeadNextValue: 0x5000,
                CapturedLaterTagFromReceiverPlus4: 7,
                QueuePopReturnedNormally: null));
        check("Matching tags and a nonzero head-next stop at the queue-pop call when normal return is uncaptured",
            popCallBoundary.Outcome
                == QueueCleanupEmptyAttemptOutcome.QueuePopReturnUnavailableAtCallBoundary
            && popCallBoundary.Actions[^1]
                == new QueueCleanupEmptyAttemptAction(
                    QueueCleanupEmptyAttemptActionKind.CallQueuePop,
                    0x12C037B,
                    CallReturnedNormally: null,
                    TargetRawFileOffset: 0x12CE400,
                    TargetVirtualAddress: 0x16CF000));

        QueueCleanupEmptyAttemptPlan popDidNotReturn =
            QueueCleanupEmptyAttemptProjection.Project(
                popCallBoundary.Capture with { QueuePopReturnedNormally = false });
        QueueCleanupEmptyAttemptPlan popReturned =
            QueueCleanupEmptyAttemptProjection.Project(
                popCallBoundary.Capture with { QueuePopReturnedNormally = true });
        check("A nonreturning pop stops at the call; a normal return reaches the explicit jump back",
            popDidNotReturn.Outcome
                == QueueCleanupEmptyAttemptOutcome.QueuePopDidNotReturnAtCallBoundary
            && popDidNotReturn.Actions[^1].RawFileOffset == 0x12C037B
            && popReturned.Outcome
                == QueueCleanupEmptyAttemptOutcome.QueuePopReturnReachesExplicitLoopRepeat
            && popReturned.Actions[^1]
                == new QueueCleanupEmptyAttemptAction(
                    QueueCleanupEmptyAttemptActionKind.RepeatAtLoopHeader,
                    0x12C0380,
                    CallReturnedNormally: true,
                    TargetRawFileOffset: 0x12C0348,
                    TargetVirtualAddress: 0x16C0F48));

        QueueCleanupEmptyAttemptPlan nullHeadWithoutReportReturn =
            QueueCleanupEmptyAttemptProjection.Project(new QueueCleanupEmptyAttemptCapture(
                ReceiverPointer: 0x6000,
                CapturedInitialTagFromReceiverPlus4: 9,
                CapturedInitialHeadFromReceiver: 0,
                NullHeadReporterReturned: null,
                CapturedHeadNextValue: null,
                CapturedLaterTagFromReceiverPlus4: null,
                QueuePopReturnedNormally: null));
        QueueCleanupEmptyAttemptPlan nullHeadAfterReport =
            QueueCleanupEmptyAttemptProjection.Project(new QueueCleanupEmptyAttemptCapture(
                ReceiverPointer: 0x6000,
                CapturedInitialTagFromReceiverPlus4: 9,
                CapturedInitialHeadFromReceiver: 0,
                NullHeadReporterReturned: true,
                CapturedHeadNextValue: 0,
                CapturedLaterTagFromReceiverPlus4: 9,
                QueuePopReturnedNormally: null));
        check("Null-head reporting gates the head-next comparison, and a returning reporter permits later captures",
            nullHeadWithoutReportReturn.Outcome
                == QueueCleanupEmptyAttemptOutcome.NullHeadReporterReturnUnavailable
            && nullHeadWithoutReportReturn.Actions[^1].CapturedValue == 0x324
            && nullHeadAfterReport.Outcome
                == QueueCleanupEmptyAttemptOutcome.EmptyHeadNextEntersSecondaryDrainBoundary
            && nullHeadAfterReport.Actions[2].CallReturnedNormally == true);

        check("A null-head reporter that does not return stops before the head-next comparison",
            QueueCleanupEmptyAttemptProjection.Project(new QueueCleanupEmptyAttemptCapture(
                ReceiverPointer: 0x6000,
                CapturedInitialTagFromReceiverPlus4: 9,
                CapturedInitialHeadFromReceiver: 0,
                NullHeadReporterReturned: false,
                CapturedHeadNextValue: null,
                CapturedLaterTagFromReceiverPlus4: null,
                QueuePopReturnedNormally: null)).Outcome
                == QueueCleanupEmptyAttemptOutcome.NullHeadReporterDidNotReturn);

        bool rejectsUnreachedCaptures =
            ThrowsArgumentException(() =>
                QueueCleanupEmptyAttemptProjection.Project(
                    popCallBoundary.Capture with { NullHeadReporterReturned = true }))
            && ThrowsArgumentException(() =>
                QueueCleanupEmptyAttemptProjection.Project(
                    nullHeadWithoutReportReturn.Capture with { CapturedHeadNextValue = 0 }))
            && ThrowsArgumentException(() =>
                QueueCleanupEmptyAttemptProjection.Project(
                    mismatchWithEmptyHeadNext.Capture with { QueuePopReturnedNormally = true }))
            && ThrowsArgumentException(() =>
                QueueCleanupEmptyAttemptProjection.Project(
                    emptyBoundary.Capture with { QueuePopReturnedNormally = false }))
            && ThrowsArgumentException(() =>
                QueueCleanupEmptyAttemptProjection.Project(
                    popCallBoundary.Capture with
                    {
                        CapturedLaterTagFromReceiverPlus4 = null,
                        QueuePopReturnedNormally = true
                    }));
        check("Captures from branches or instructions not reached by this pass are rejected",
            rejectsUnreachedCaptures);
    }

    private static bool HasLoopSlice(uint rawFileOffset, string expected)
    {
        string[] body = QueueCleanupEmptyAttemptProjection.CapturedLoopBytes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (rawFileOffset < QueueCleanupEmptyAttemptProjection.LoopHeaderRawFileOffset)
        {
            return false;
        }

        string[] instruction = expected.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int start = checked((int)(
            rawFileOffset - QueueCleanupEmptyAttemptProjection.LoopHeaderRawFileOffset));
        return start + instruction.Length <= body.Length
            && body.Skip(start).Take(instruction.Length).SequenceEqual(instruction);
    }

    private static bool ShortBranchTargets(
        uint rawFileOffset,
        string instructionHex,
        uint expectedTarget)
    {
        byte[] instruction = Convert.FromHexString(
            instructionHex.Replace(" ", string.Empty, StringComparison.Ordinal));
        return instruction.Length == 2
            && instruction[0] is 0x70 or 0x75 or 0xEB
            && rawFileOffset + 2 + (sbyte)instruction[1] == expectedTarget;
    }

    private static bool RelativeCallTargets(
        uint rawFileOffset,
        string instructionHex,
        uint expectedTarget)
    {
        byte[] instruction = Convert.FromHexString(
            instructionHex.Replace(" ", string.Empty, StringComparison.Ordinal));
        return instruction.Length == 5
            && instruction[0] == 0xE8
            && (long)rawFileOffset + instruction.Length
                + BinaryPrimitives.ReadInt32LittleEndian(instruction.AsSpan(1))
                == expectedTarget;
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
