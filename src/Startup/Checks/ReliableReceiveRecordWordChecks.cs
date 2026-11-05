using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveRecordWordChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample identity, exact raw window, VA mapping, bytes, and branch are recorded",
            ReliableReceiveRecordWordProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ReliableReceiveRecordWordProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveRecordWordProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReliableReceiveRecordWordProjection.ComparisonStartRawFileOffset == 0x00BE532F
            && ReliableReceiveRecordWordProjection.ComparisonStartVirtualAddress == 0x00FE5F2F
            && ReliableReceiveRecordWordProjection.RawFileOffsetToVirtualAddress(0x00BE532F)
                == 0x00FE5F2F
            && ReliableReceiveRecordWordProjection.ComparisonEndRawFileOffsetExclusive == 0x00BE5353
            && ReliableReceiveRecordWordProjection.RawFileOffsetToVirtualAddress(0x00BE5353)
                == 0x00FE5F53
            && ReliableReceiveRecordWordProjection.CapturedComparisonBytes
                == "66 8B 40 02 66 3B 47 02 74 1A 6A 00 68 FA 01 00 00 "
                    + "68 3E 8B B5 01 68 1B 8B B5 01 FF 15 A0 5E C1 01 83 C4 10"
            && ReliableReceiveRecordWordProjection.CapturedComparisonBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x24
            && InstructionEvidenceMatches());

        check("Both WORD reads use independent wrapped addresses and occur record first, payload second",
            ReliableReceiveRecordWordProjection.RecordWordOffset == 2
            && ReliableReceiveRecordWordProjection.PayloadWordOffset == 2
            && ReliableReceiveRecordWordProjection.RecordWordAddress(uint.MaxValue - 1) == 0
            && ReliableReceiveRecordWordProjection.PayloadWordAddress(uint.MaxValue - 1) == 0
            && ReliableReceiveRecordWordProjection.Instructions[0].RawFileOffset
                == ReliableReceiveRecordWordProjection.RecordWordReadRawFileOffset
            && ReliableReceiveRecordWordProjection.Instructions[1].RawFileOffset
                == ReliableReceiveRecordWordProjection.PayloadWordReadRawFileOffset
            && ReliableReceiveRecordWordProjection.EqualityBranchRawFileOffset == 0x00BE5337
            && ReliableReceiveRecordWordProjection.EqualityBranchDestinationRawFileOffset == 0x00BE5353
            && ShortBranchTarget(
                ReliableReceiveRecordWordProjection.EqualityBranchRawFileOffset,
                0x1A) == ReliableReceiveRecordWordProjection.EqualityBranchDestinationRawFileOffset
            && ReliableReceiveRecordWordProjection.DiagnosticCallRawFileOffset == 0x00BE534A
            && ReliableReceiveRecordWordProjection.DiagnosticCallPointerVirtualAddress == 0x01C15EA0
            && ReliableReceiveRecordWordProjection.DiagnosticCallReturnRawFileOffset == 0x00BE5350
            && ReliableReceiveRecordWordProjection.DiagnosticCleanupStackBytes == 0x10);

        ReliableReceiveRecordWordPlan invalidRecordEntry =
            ReliableReceiveRecordWordProjection.Project(new(0, 0, 1, 2, true));
        ReliableReceiveRecordWordPlan missingRecord =
            ReliableReceiveRecordWordProjection.Project(new(0x1000, 0, null, null, null));
        ReliableReceiveRecordWordPlan missingRecordWithLaterCapture =
            ReliableReceiveRecordWordProjection.Project(new(0x1000, 0, null, null, true));
        ReliableReceiveRecordWordPlan unreachedPayload =
            ReliableReceiveRecordWordProjection.Project(new(0x1000, 0, null, 2, null));
        ReliableReceiveRecordWordPlan missingPayload =
            ReliableReceiveRecordWordProjection.Project(new(0x1000, 0, 1, null, null));
        check("A zero record pointer is invalid and missing captures stop in read order",
            invalidRecordEntry.Outcome == ReliableReceiveRecordWordOutcome.InvalidRecordEntry
            && invalidRecordEntry.UnreachedRecordWordCaptureProvided
            && !invalidRecordEntry.RecordWordRead
            && !invalidRecordEntry.PayloadWordRead
            && invalidRecordEntry.UnreachedPayloadWordCaptureProvided
            && invalidRecordEntry.UnreachedDiagnosticReturnCaptureProvided
            && !invalidRecordEntry.DiagnosticCallReached
            && invalidRecordEntry.StopAtRawFileOffsetExclusive
                == ReliableReceiveRecordWordProjection.RecordWordReadRawFileOffset
            && missingRecord.Outcome == ReliableReceiveRecordWordOutcome.MissingRecordWordCapture
            && missingRecord.RecordWordAddress == 0x1002
            && !missingRecord.RecordWordRead
            && !missingRecord.PayloadWordRead
            && missingRecordWithLaterCapture.Outcome
                == ReliableReceiveRecordWordOutcome.UnreachedDiagnosticReturnCapture
            && missingRecordWithLaterCapture.UnreachedDiagnosticReturnCaptureProvided
            && unreachedPayload.Outcome == ReliableReceiveRecordWordOutcome.UnreachedPayloadWordCapture
            && unreachedPayload.UnreachedPayloadWordCaptureProvided
            && !unreachedPayload.PayloadWordRead
            && missingPayload.Outcome == ReliableReceiveRecordWordOutcome.MissingPayloadWordCapture
            && missingPayload.RecordWordRead
            && missingPayload.RecordWordAddress == 0x1002
            && !missingPayload.PayloadWordRead
            && missingPayload.PayloadWordAddress == 2
            && ReliableReceiveRecordWordProjection.Project(new(0x1000, 0, null, null, true))
                .UnreachedDiagnosticReturnCaptureProvided
            && ReliableReceiveRecordWordProjection.Project(new(0x1000, 0, 1, null, true))
                .UnreachedDiagnosticReturnCaptureProvided
            && missingPayload.StopAtRawFileOffsetExclusive
                == ReliableReceiveRecordWordProjection.PayloadWordReadRawFileOffset);

        ReliableReceiveRecordWordPlan zeroWords =
            ReliableReceiveRecordWordProjection.Project(new(0x1000, 0, 0, 0, null));
        ReliableReceiveRecordWordPlan maxWords =
            ReliableReceiveRecordWordProjection.Project(new(0x1000, 0xFFFF, 0xFFFF, 0xFFFF, null));
        ReliableReceiveRecordWordPlan unequalWords =
            ReliableReceiveRecordWordProjection.Project(new(0x1000, 0x2000, 0, 0xFFFF, true));
        ReliableReceiveRecordWordPlan equalWithUnreachedReturn =
            ReliableReceiveRecordWordProjection.Project(new(0x1000, 0x2000, 0xFFFF, 0xFFFF, false));
        check("WORD zero, WORD 0xFFFF, equality short-circuit, and unequal diagnostic branching are exact",
            zeroWords.Outcome == ReliableReceiveRecordWordOutcome.DiagnosticSkippedForEqualWords
            && zeroWords.WordsEqual == true
            && zeroWords.EqualityBranchTaken == true
            && zeroWords.SelectedDestinationRawFileOffset == 0x00BE5353
            && !zeroWords.DiagnosticCallReached
            && maxWords.Outcome == ReliableReceiveRecordWordOutcome.DiagnosticSkippedForEqualWords
            && maxWords.WordsEqual == true
            && unequalWords.Outcome == ReliableReceiveRecordWordOutcome.DiagnosticReturnedNormally
            && unequalWords.WordsEqual == false
            && unequalWords.EqualityBranchTaken == false
            && unequalWords.DiagnosticCallReached
            && unequalWords.DiagnosticStackPushes.SequenceEqual(new uint[] { 0, 0x1FA, 0x1B58B3E, 0x1B58B1B })
            && unequalWords.DiagnosticCallPointerVirtualAddress == 0x01C15EA0
            && unequalWords.CleanupReached
            && unequalWords.CleanupStackBytes == 0x10
            && unequalWords.SelectedDestinationRawFileOffset == 0x00BE5353
            && equalWithUnreachedReturn.Outcome
                == ReliableReceiveRecordWordOutcome.UnreachedDiagnosticReturnCapture
            && equalWithUnreachedReturn.EqualityBranchTaken == true
            && !equalWithUnreachedReturn.DiagnosticCallReached
            && !equalWithUnreachedReturn.CleanupReached);

        ReliableReceiveRecordWordPlan missingReturn =
            ReliableReceiveRecordWordProjection.Project(new(0x1000, 0x2000, 0, 1, null));
        ReliableReceiveRecordWordPlan nonReturning =
            ReliableReceiveRecordWordProjection.Project(new(0x1000, 0x2000, 0, 1, false));
        check("Missing or non-returning diagnostics never reach or claim the cleanup",
            missingReturn.Outcome == ReliableReceiveRecordWordOutcome.MissingDiagnosticReturnCapture
            && missingReturn.DiagnosticCallReached
            && missingReturn.DiagnosticReturnedNormally is null
            && !missingReturn.CleanupReached
            && missingReturn.CleanupStackBytes is null
            && missingReturn.SelectedDestinationRawFileOffset is null
            && missingReturn.StopAtRawFileOffsetExclusive
                == ReliableReceiveRecordWordProjection.DiagnosticCallReturnRawFileOffset
            && nonReturning.Outcome == ReliableReceiveRecordWordOutcome.DiagnosticDidNotReturnNormally
            && nonReturning.DiagnosticCallReached
            && nonReturning.DiagnosticReturnedNormally == false
            && !nonReturning.CleanupReached
            && nonReturning.CleanupStackBytes is null
            && nonReturning.SelectedDestinationRawFileOffset is null
            && nonReturning.StopAtRawFileOffsetExclusive
                == ReliableReceiveRecordWordProjection.DiagnosticCallReturnRawFileOffset);

        ReliableReceiveRecordWordPlan zeroPayloadPointer =
            ReliableReceiveRecordWordProjection.Project(new(0x1000, 0, 1, 2, null));
        check("A zero payload pointer remains a valid native dereference attempt",
            zeroPayloadPointer.PayloadWordRead
            && zeroPayloadPointer.PayloadWordAddress == 2
            && zeroPayloadPointer.WordsEqual == false
            && zeroPayloadPointer.DiagnosticCallReached);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE532F, 0x00FE5F2F, "66 8B 40 02", null, null),
            (0x00BE5333, 0x00FE5F33, "66 3B 47 02", null, null),
            (0x00BE5337, 0x00FE5F37, "74 1A", 0x00BE5353, 0x00FE5F53),
            (0x00BE5339, 0x00FE5F39, "6A 00 68 FA 01 00 00 68 3E 8B B5 01 68 1B 8B B5 01", null, null),
            (0x00BE534A, 0x00FE5F4A, "FF 15 A0 5E C1 01", null, null),
            (0x00BE5350, 0x00FE5F50, "83 C4 10", null, null)
        };
        return ReliableReceiveRecordWordProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);
}
