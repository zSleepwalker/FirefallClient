using Firefall.Reconstruction.Startup;

internal static class QueueAppendAttemptChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Queue-append CAS anchors use the pinned sample, raw interval, and raw-to-VA mapping",
            QueueAppendAttemptProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && QueueAppendAttemptProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && QueueAppendAttemptProjection.IdaAddressMapping
                == "Base-zero IDA EAs are raw file offsets; code VA is raw + 0x400C00, not RVA."
            && QueueAppendAttemptProjection.AttemptStartRawFileOffset == 0x12CE364
            && QueueAppendAttemptProjection.AttemptEndRawFileOffsetExclusive == 0x12CE3C3
            && QueueAppendAttemptProjection.AttemptStartVirtualAddress == 0x16CEF64
            && QueueAppendAttemptProjection.AttemptEndVirtualAddressExclusive == 0x16CEFC3
            && QueueAppendAttemptProjection.AttemptStartRawFileOffset
                + QueueAppendAttemptProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueAppendAttemptProjection.AttemptStartVirtualAddress
            && QueueAppendAttemptProjection.AttemptEndRawFileOffsetExclusive
                + QueueAppendAttemptProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueAppendAttemptProjection.AttemptEndVirtualAddressExclusive
            && QueueAppendAttemptProjection.CapturedAttemptBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x5F
            && QueueAppendAttemptProjection.RetryTargetRawFileOffset
                == QueueAppendAttemptProjection.AttemptStartRawFileOffset
            && QueueAppendAttemptProjection.SuccessBoundaryRawFileOffset
                == QueueAppendAttemptProjection.AttemptEndRawFileOffsetExclusive);

        check("Raw bytes and instructions preserve the captured inputs, diagnostic, CAS, and ordered compares",
            QueueAppendAttemptProjection.CapturedAttemptBytes
                == "8B 5B 0C 8B 31 89 74 24 18 85 F6 75 1D 56 68 EC 02 00 00 "
                    + "68 DF 42 B7 01 68 03 43 B7 01 FF 15 A0 5E C1 01 8B 44 24 1C "
                    + "83 C4 10 8B D3 8D 4B 01 33 FF 89 54 24 1C 33 DB 0B FE 0B D8 "
                    + "8B C7 8B 74 24 10 F0 0F C7 0E 8B 74 24 18 3B C7 8B 44 24 0C "
                    + "8B 5C 24 14 8B 4C 24 10 75 A7 3B 54 24 1C 75 A1"
            && QueueAppendAttemptProjection.Instructions.All(HasCapturedSlice)
            && QueueAppendAttemptProjection.Instructions.Select(instruction =>
                (instruction.RawFileOffset, instruction.Bytes, instruction.BranchTargetRawFileOffset,
                    instruction.TargetVirtualAddress))
                .SequenceEqual(new
                    (uint RawFileOffset, string Bytes, uint? BranchTargetRawFileOffset,
                        uint? TargetVirtualAddress)[]
                {
                    (0x12CE364, "8B 5B 0C", null, null),
                    (0x12CE367, "8B 31", null, null),
                    (0x12CE369, "89 74 24 18", null, null),
                    (0x12CE36D, "85 F6", null, null),
                    (0x12CE36F, "75 1D", 0x12CE38E, null),
                    (0x12CE381, "FF 15 A0 5E C1 01", null, 0x1C15EA0),
                    (0x12CE387, "8B 44 24 1C", null, null),
                    (0x12CE38E, "8B D3", null, null),
                    (0x12CE390, "8D 4B 01", null, null),
                    (0x12CE395, "89 54 24 1C", null, null),
                    (0x12CE39B, "0B FE", null, null),
                    (0x12CE39D, "0B D8", null, null),
                    (0x12CE39F, "8B C7", null, null),
                    (0x12CE3A1, "8B 74 24 10", null, null),
                    (0x12CE3A5, "F0 0F C7 0E", null, null),
                    (0x12CE3A9, "8B 74 24 18", null, null),
                    (0x12CE3AD, "3B C7", null, null),
                    (0x12CE3AF, "8B 44 24 0C", null, null),
                    (0x12CE3B3, "8B 5C 24 14", null, null),
                    (0x12CE3B7, "8B 4C 24 10", null, null),
                    (0x12CE3BB, "75 A7", 0x12CE364, null),
                    (0x12CE3BD, "3B 54 24 1C", null, null),
                    (0x12CE3C1, "75 A1", 0x12CE364, null)
                })
            && QueueAppendAttemptProjection.Instructions.All(instruction =>
                instruction.RawFileOffset < QueueAppendAttemptProjection.AttemptEndRawFileOffsetExclusive)
            && QueueAppendAttemptProjection.DiagnosticId == 0x2EC
            && QueueAppendAttemptProjection.DiagnosticCallCellVirtualAddress == 0x1C15EA0
            && QueueAppendAttemptProjection.TagOffset == 0x0C
            && QueueAppendAttemptProjection.TailOffset == 8
            && QueueAppendAttemptProjection.PairTailOffset == 0
            && QueueAppendAttemptProjection.PairTagOffset == 4);

        QueueAppendAttemptPlan nullTailReturningDiagnostic =
            QueueAppendAttemptProjection.Project(Captures(
                tail: 0,
                tag: uint.MaxValue,
                diagnosticReturned: QueueAppendAttemptCapture<bool>.Captured(true),
                returnedEax: QueueAppendAttemptCapture<uint>.Captured(0),
                returnedEdx: QueueAppendAttemptCapture<uint>.Captured(uint.MaxValue)));
        check("A null captured tail reports 0x2EC and a returning reporter still reaches the CAS",
            nullTailReturningDiagnostic.Outcome == QueueAppendAttemptOutcome.SuccessBoundary
            && nullTailReturningDiagnostic.Actions.Any(action =>
                action.Kind == QueueAppendAttemptActionKind.ReportNullTailDiagnostic
                && action.DiagnosticId == 0x2EC)
            && nullTailReturningDiagnostic.ExpectedPair
                == new QueueAppendAttemptPair(0, uint.MaxValue)
            && nullTailReturningDiagnostic.ReplacementPair
                == new QueueAppendAttemptPair(0x55667788, 0)
            && nullTailReturningDiagnostic.Actions.Any(action =>
                action.Kind == QueueAppendAttemptActionKind.AttemptLockedCompareExchange)
            && nullTailReturningDiagnostic.Actions[^1]
                == new QueueAppendAttemptAction(
                    QueueAppendAttemptActionKind.StopAtSuccessBoundary,
                    0x12CE3C3));

        QueueAppendAttemptPlan noDiagnosticReturn =
            QueueAppendAttemptProjection.Project(Captures(
                tail: 0,
                diagnosticReturned: QueueAppendAttemptCapture<bool>.Missing()));
        check("A missing null-tail diagnostic return stops before the CAS",
            noDiagnosticReturn.Outcome
                == QueueAppendAttemptOutcome.MissingDiagnosticReturnBoundary
            && noDiagnosticReturn.Actions[^1]
                == new QueueAppendAttemptAction(
                    QueueAppendAttemptActionKind.StopAtMissingDiagnosticReturnBoundary,
                    0x12CE387)
            && !noDiagnosticReturn.Actions.Any(action =>
                action.Kind == QueueAppendAttemptActionKind.AttemptLockedCompareExchange));

        QueueAppendAttemptPlan diagnosticDidNotReturn =
            QueueAppendAttemptProjection.Project(Captures(
                tail: 0,
                diagnosticReturned: QueueAppendAttemptCapture<bool>.Captured(false)));
        check("A nonreturning null-tail diagnostic cannot reach the CAS",
            diagnosticDidNotReturn.Outcome == QueueAppendAttemptOutcome.DiagnosticDidNotReturnBoundary
            && !diagnosticDidNotReturn.Actions.Any(action =>
                action.Kind == QueueAppendAttemptActionKind.AttemptLockedCompareExchange));

        QueueAppendAttemptPlan matchingPair =
            QueueAppendAttemptProjection.Project(Captures(
                tail: 0x12345678,
                tag: 0xAABBCCDD,
                returnedEax: QueueAppendAttemptCapture<uint>.Captured(0x12345678),
                returnedEdx: QueueAppendAttemptCapture<uint>.Captured(0xAABBCCDD)));
        check("One CAS attempt expects (captured tail, tag) and desires (captured new node, wrapped tag+1)",
            matchingPair.Outcome == QueueAppendAttemptOutcome.SuccessBoundary
            && matchingPair.ReceiverPairPointer == 0x1008
            && matchingPair.ExpectedPair == new QueueAppendAttemptPair(0x12345678, 0xAABBCCDD)
            && matchingPair.ReplacementPair == new QueueAppendAttemptPair(0x55667788, 0xAABBCCDE)
            && matchingPair.CompareExchangeReturnedPair
                == new QueueAppendAttemptPair(0x12345678, 0xAABBCCDD)
            && matchingPair.Actions.Select(action => action.InstructionRawFileOffset)
                .Contains(0x12CE3A5u));

        QueueAppendAttemptPlan eaxMismatch =
            QueueAppendAttemptProjection.Project(Captures(
                tail: 1,
                returnedEax: QueueAppendAttemptCapture<uint>.Captured(2),
                returnedEdx: QueueAppendAttemptCapture<uint>.Unreached()));
        QueueAppendAttemptPlan edxMismatch =
            QueueAppendAttemptProjection.Project(Captures(
                tail: 1,
                tag: 5,
                returnedEax: QueueAppendAttemptCapture<uint>.Captured(1),
                returnedEdx: QueueAppendAttemptCapture<uint>.Captured(6)));
        check("Returned EAX is checked first; either mismatch stops at its retry branch without looping",
            eaxMismatch.Outcome == QueueAppendAttemptOutcome.RetryBoundary
            && eaxMismatch.Actions[^1]
                == new QueueAppendAttemptAction(
                    QueueAppendAttemptActionKind.StopAtRetryBoundary,
                    0x12CE3BB,
                    BranchTargetRawFileOffset: 0x12CE364)
            && !eaxMismatch.Actions.Any(action =>
                action.Kind == QueueAppendAttemptActionKind.CompareReturnedEdx)
            && edxMismatch.Outcome == QueueAppendAttemptOutcome.RetryBoundary
            && edxMismatch.Actions[^1]
                == new QueueAppendAttemptAction(
                    QueueAppendAttemptActionKind.StopAtRetryBoundary,
                    0x12CE3C1,
                    BranchTargetRawFileOffset: 0x12CE364));

        QueueAppendAttemptPlan missingEax =
            QueueAppendAttemptProjection.Project(Captures(
                returnedEax: QueueAppendAttemptCapture<uint>.Missing(),
                returnedEdx: QueueAppendAttemptCapture<uint>.Unreached()));
        QueueAppendAttemptPlan missingEdx =
            QueueAppendAttemptProjection.Project(Captures(
                tail: 1,
                returnedEax: QueueAppendAttemptCapture<uint>.Captured(1),
                returnedEdx: QueueAppendAttemptCapture<uint>.Missing()));
        check("Missing CAS-result halves stop at their separate comparison boundaries",
            missingEax.Outcome == QueueAppendAttemptOutcome.MissingCompareExchangeEaxBoundary
            && missingEax.Actions[^1].InstructionRawFileOffset == 0x12CE3AD
            && missingEdx.Outcome == QueueAppendAttemptOutcome.MissingCompareExchangeEdxBoundary
            && missingEdx.Actions[^1].InstructionRawFileOffset == 0x12CE3BD);

        bool rejectsUnreachedCaptures =
            ThrowsArgumentException(() => QueueAppendAttemptProjection.Project(Captures(
                tail: 0,
                diagnosticReturned: QueueAppendAttemptCapture<bool>.Missing(),
                returnedEax: QueueAppendAttemptCapture<uint>.Captured(0))))
            && ThrowsArgumentException(() => QueueAppendAttemptProjection.Project(Captures(
                tail: 4,
                diagnosticReturned: QueueAppendAttemptCapture<bool>.Captured(true))))
            && ThrowsArgumentException(() => QueueAppendAttemptProjection.Project(Captures(
                tail: 1,
                returnedEax: QueueAppendAttemptCapture<uint>.Captured(2),
                returnedEdx: QueueAppendAttemptCapture<uint>.Captured(3))))
            && ThrowsArgumentException(() => QueueAppendAttemptProjection.Project(Captures(
                tail: 1,
                returnedEax: QueueAppendAttemptCapture<uint>.Captured(1),
                returnedEdx: QueueAppendAttemptCapture<uint>.Unreached())));
        check("Projection rejects diagnostic and CAS-result captures from unreached branches",
            rejectsUnreachedCaptures);
    }

    private static QueueAppendAttemptCaptures Captures(
        uint tail = 0x12345678,
        uint tag = 7,
        QueueAppendAttemptCapture<bool>? diagnosticReturned = null,
        QueueAppendAttemptCapture<uint>? returnedEax = null,
        QueueAppendAttemptCapture<uint>? returnedEdx = null) =>
        new(
            ReceiverAddress: 0x1000,
            CapturedNewNode: 0x55667788,
            CapturedTag: QueueAppendAttemptCapture<uint>.Captured(tag),
            CapturedTail: QueueAppendAttemptCapture<uint>.Captured(tail),
            DiagnosticReturned:
                diagnosticReturned ?? QueueAppendAttemptCapture<bool>.Unreached(),
            CompareExchangeReturnedEax:
                returnedEax ?? QueueAppendAttemptCapture<uint>.Unreached(),
            CompareExchangeReturnedEdx:
                returnedEdx ?? QueueAppendAttemptCapture<uint>.Unreached());

    private static bool HasCapturedSlice(QueueAppendAttemptInstructionEvidence instruction)
    {
        string[] body =
            QueueAppendAttemptProjection.CapturedAttemptBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] expected = instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        uint rawOffset = instruction.RawFileOffset
            - QueueAppendAttemptProjection.AttemptStartRawFileOffset;
        int start = checked((int)rawOffset);
        return start + expected.Length <= body.Length
            && body.Skip(start).Take(expected.Length).SequenceEqual(expected);
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
