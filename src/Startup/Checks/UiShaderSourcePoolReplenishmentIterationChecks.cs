using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolReplenishmentIterationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Iteration entry, exact sample, and raw-file-offset to code-VA mapping are exact",
            UiShaderSourcePoolReplenishmentIterationProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.EntryRawFileOffset
                == 0x12CE1B4
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.EntryVirtualAddress
                == 0x16CEDB4
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.EntryRawFileOffset
                + UiShaderSourcePoolReplenishmentIterationProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolReplenishmentIterationProjectionRules.EntryVirtualAddress
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.IterationEndRawFileOffsetExclusive
                == 0x12CE1F5
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.PoolPushFunctionRawFileOffset
                == 0x12CDA60
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.DiagnosticReportId == 704);

        check("Initial zero-index gate bytes and unsigned exit target are exact",
            UiShaderSourcePoolReplenishmentIterationProjectionRules.CapturedInitialGateBytes
                == "33 DB 39 5F 20 76 41"
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.InitialIndexInitializeRawFileOffset
                == 0x12CE1AD
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.InitialBoundCompareRawFileOffset
                == 0x12CE1AF
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.InitialGateBranchRawFileOffset
                + 2 + 0x41
                == UiShaderSourcePoolReplenishmentIterationProjectionRules.IterationEndRawFileOffsetExclusive
            && !UiShaderSourcePoolReplenishmentIterationProjectionRules.ShouldEnterIteration(0)
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.ShouldEnterIteration(1)
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.ShouldEnterIteration(uint.MaxValue));

        check("Captured iteration bytes preserve every read, unsigned divide, diagnostic, push, and tail branch",
            UiShaderSourcePoolReplenishmentIterationProjectionRules.CapturedEntryBytes
                == "8B 77 14 33 D2 03 77 1C 0F AF F3 03 F1 8B C6 F7 77 18 "
                    + "85 D2 74 1A 6A 00 68 C0 02 00 00 68 DB 43 B7 01 "
                    + "68 B7 43 B7 01 FF 15 A0 5E C1 01 83 C4 10 "
                    + "56 8B CF E8 74 F8 FF FF 8B 4D D0 43 3B 5F 20 72 BF"
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.CapturedEntryBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x41
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.EntryRawFileOffset
                + 0x41
                == UiShaderSourcePoolReplenishmentIterationProjectionRules.IterationEndRawFileOffsetExclusive
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.FieldOffset,
                        instruction.TargetRawFileOffset, instruction.AbsoluteCellVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    int? FieldOffset,
                    uint? TargetRawFileOffset,
                    uint? AbsoluteCellVirtualAddress)[]
                {
                    (0x12CE1AD, "33 DB", null, null, null),
                    (0x12CE1AF, "39 5F 20", 0x20, null, null),
                    (0x12CE1B2, "76 41", null, 0x12CE1F5, null),
                    (0x12CE1B4, "8B 77 14", 0x14, null, null),
                    (0x12CE1B7, "33 D2", null, null, null),
                    (0x12CE1B9, "03 77 1C", 0x1C, null, null),
                    (0x12CE1BC, "0F AF F3", null, null, null),
                    (0x12CE1BF, "03 F1", null, null, null),
                    (0x12CE1C1, "8B C6", null, null, null),
                    (0x12CE1C3, "F7 77 18", 0x18, null, null),
                    (0x12CE1C6, "85 D2", null, null, null),
                    (0x12CE1C8, "74 1A", null, 0x12CE1E4, null),
                    (0x12CE1CA, "6A 00", null, null, null),
                    (0x12CE1CC, "68 C0 02 00 00", null, null, null),
                    (0x12CE1D1, "68 DB 43 B7 01", null, null, null),
                    (0x12CE1D6, "68 B7 43 B7 01", null, null, null),
                    (0x12CE1DB, "FF 15 A0 5E C1 01", null, null, 0x01C15EA0),
                    (0x12CE1E1, "83 C4 10", null, null, null),
                    (0x12CE1E4, "56", null, null, null),
                    (0x12CE1E5, "8B CF", null, null, null),
                    (0x12CE1E7, "E8 74 F8 FF FF", null, 0x12CDA60, null),
                    (0x12CE1EC, "8B 4D D0", null, null, null),
                    (0x12CE1EF, "43", null, null, null),
                    (0x12CE1F0, "3B 5F 20", 0x20, null, null),
                    (0x12CE1F3, "72 BF", null, 0x12CE1B4, null)
                })
            && HasByteSlice(
                0x12CE1B4,
                0x12CE1C6,
                "8B 77 14 33 D2 03 77 1C 0F AF F3 03 F1 8B C6 F7 77 18")
            && HasByteSlice(
                0x12CE1C6,
                0x12CE1E4,
                "85 D2 74 1A 6A 00 68 C0 02 00 00 68 DB 43 B7 01 "
                    + "68 B7 43 B7 01 FF 15 A0 5E C1 01 83 C4 10")
            && HasByteSlice(
                0x12CE1E4,
                0x12CE1F5,
                "56 8B CF E8 74 F8 FF FF 8B 4D D0 43 3B 5F 20 72 BF")
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.PoolPushCallRawFileOffset
                + 5 - 0x78C
                == UiShaderSourcePoolReplenishmentIterationProjectionRules.PoolPushFunctionRawFileOffset
            && UiShaderSourcePoolReplenishmentIterationProjectionRules.RepeatBranchRawFileOffset
                + 2 - 0x41
                == UiShaderSourcePoolReplenishmentIterationProjectionRules.RepeatBranchTargetRawFileOffset);

        var remainderPath = UiShaderSourcePoolReplenishmentIterationProjectionRules.Project(
            new UiShaderSourcePoolReplenishmentIterationCapture(
                Index: 2,
                CurrentBase: 0xFFFFFFF0,
                Field14Value: 3,
                Field1CValue: 2,
                PoolDivisor: 7,
                LaterBaseCapture: 0xABCDEF01,
                FreshLoopBound20: 10));
        check("Nonzero unsigned remainder diagnoses, then still pushes the same wrapped entry",
            remainderPath.Field14PlusField1C == 5
            && remainderPath.ScaledTerm == 10
            && remainderPath.EntryValue == 0xFFFFFFFA
            && remainderPath.Remainder == 5
            && remainderPath.ReachedDiagnostic
            && remainderPath.ReachedPush
            && remainderPath.PushEntryValue == remainderPath.EntryValue
            && remainderPath.ReachedLoopDecision
            && remainderPath.ReloadedCurrentBase == 0xABCDEF01
            && remainderPath.NextIndex == 3
            && remainderPath.FreshLoopBound20 == 10
            && remainderPath.TakesUnsignedRepeatBranch == true
            && remainderPath.Capture.CurrentBase == 0xFFFFFFF0
            && remainderPath.Capture.LaterBaseCapture == 0xABCDEF01
            && remainderPath.Capture.Field14Value == 3
            && remainderPath.Capture.Field1CValue == 2);

        var exactMultiple = UiShaderSourcePoolReplenishmentIterationProjectionRules.Project(
            new UiShaderSourcePoolReplenishmentIterationCapture(
                Index: 2,
                CurrentBase: 20,
                Field14Value: 2,
                Field1CValue: 3,
                PoolDivisor: 5,
                LaterBaseCapture: 20,
                FreshLoopBound20: 3));
        check("Zero remainder skips diagnostics and the fresh post-push bound controls only the next branch",
            exactMultiple.EntryValue == 30
            && exactMultiple.Remainder == 0
            && !exactMultiple.ReachedDiagnostic
            && exactMultiple.ReachedPush
            && exactMultiple.PushEntryValue == 30
            && exactMultiple.NextIndex == 3
            && exactMultiple.TakesUnsignedRepeatBranch == false);

        var multiplyWrap = UiShaderSourcePoolReplenishmentIterationProjectionRules.Project(
            new UiShaderSourcePoolReplenishmentIterationCapture(
                Index: uint.MaxValue,
                CurrentBase: 5,
                Field14Value: 1,
                Field1CValue: 1,
                PoolDivisor: 2,
                LaterBaseCapture: 5,
                FreshLoopBound20: 1));
        check("Field addition, index multiplication, base addition, and index increment wrap as DWORDs",
            multiplyWrap.Field14PlusField1C == 2
            && multiplyWrap.ScaledTerm == 0xFFFFFFFE
            && multiplyWrap.EntryValue == 3
            && multiplyWrap.Remainder == 1
            && multiplyWrap.NextIndex == 0
            && multiplyWrap.TakesUnsignedRepeatBranch == true);

        var fieldSumWrap = UiShaderSourcePoolReplenishmentIterationProjectionRules.Project(
            new UiShaderSourcePoolReplenishmentIterationCapture(
                Index: 2,
                CurrentBase: 0,
                Field14Value: uint.MaxValue,
                Field1CValue: 2,
                PoolDivisor: 3,
                LaterBaseCapture: 0,
                FreshLoopBound20: 4));
        check("The separately captured field values sum with 32-bit wrap before index multiplication",
            fieldSumWrap.Field14PlusField1C == 1
            && fieldSumWrap.ScaledTerm == 2
            && fieldSumWrap.EntryValue == 2
            && fieldSumWrap.Remainder == 2);

        bool missingTailCapturesRejected = ThrowsArgumentException(() =>
            UiShaderSourcePoolReplenishmentIterationProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentIterationCapture(
                    Index: 1,
                    CurrentBase: 1,
                    Field14Value: 1,
                    Field1CValue: 1,
                    PoolDivisor: 3)));
        bool unreachedTailCapturesRejected = ThrowsArgumentException(() =>
            UiShaderSourcePoolReplenishmentIterationProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentIterationCapture(
                    Index: 1,
                    CurrentBase: 1,
                    Field14Value: 1,
                    Field1CValue: 1,
                    PoolDivisor: 0,
                    LaterBaseCapture: 1,
                    FreshLoopBound20: 2)));
        check("Loop-tail captures are required on normal continuation and rejected on an unreached path",
            missingTailCapturesRejected && unreachedTailCapturesRejected);

        var zeroDivisor = UiShaderSourcePoolReplenishmentIterationProjectionRules.Project(
            new UiShaderSourcePoolReplenishmentIterationCapture(
                Index: 4,
                CurrentBase: 5,
                Field14Value: 6,
                Field1CValue: 7,
                PoolDivisor: 0));
        check("A zero divisor is a distinct fault before remainder diagnostics or push",
            zeroDivisor.Outcome
                == UiShaderSourcePoolReplenishmentIterationOutcome.DivideByZeroFault
            && zeroDivisor.EntryValue == 57
            && zeroDivisor.Remainder is null
            && !zeroDivisor.ReachedDiagnostic
            && !zeroDivisor.ReachedPush
            && zeroDivisor.PushEntryValue is null
            && !zeroDivisor.ReachedLoopDecision);

        var diagnosticDoesNotReturn =
            UiShaderSourcePoolReplenishmentIterationProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentIterationCapture(
                    Index: 1,
                    CurrentBase: 1,
                    Field14Value: 1,
                    Field1CValue: 1,
                    PoolDivisor: 2,
                    DiagnosticCallReturnedNormally: false));
        check("A nonreturning reached diagnostic preserves iteration captures and prevents push",
            diagnosticDoesNotReturn.Outcome
                == UiShaderSourcePoolReplenishmentIterationOutcome.DiagnosticDidNotReturnNormally
            && diagnosticDoesNotReturn.ReachedDiagnostic
            && !diagnosticDoesNotReturn.ReachedPush
            && diagnosticDoesNotReturn.EntryValue == 3
            && diagnosticDoesNotReturn.Remainder == 1
            && diagnosticDoesNotReturn.Capture.Index == 1
            && diagnosticDoesNotReturn.Capture.CurrentBase == 1);

        var pushDoesNotReturn = UiShaderSourcePoolReplenishmentIterationProjectionRules.Project(
            new UiShaderSourcePoolReplenishmentIterationCapture(
                Index: 1,
                CurrentBase: 1,
                Field14Value: 1,
                Field1CValue: 1,
                PoolDivisor: 4,
                PushCallReturnedNormally: false));
        check("A nonreturning push preserves its entry argument and does not reach the loop-tail read",
            pushDoesNotReturn.ReachedPush
            && pushDoesNotReturn.PushEntryValue == 3
            && !pushDoesNotReturn.ReachedLoopDecision
            && pushDoesNotReturn.ReloadedCurrentBase is null
            && pushDoesNotReturn.NextIndex is null
            && pushDoesNotReturn.FreshLoopBound20 is null
            && pushDoesNotReturn.TakesUnsignedRepeatBranch is null);
    }

    private static bool HasByteSlice(uint rawStart, uint rawEndExclusive, string expected)
    {
        uint entryRawFileOffset =
            UiShaderSourcePoolReplenishmentIterationProjectionRules.EntryRawFileOffset;
        if (rawStart < entryRawFileOffset || rawEndExclusive < rawStart)
        {
            return false;
        }

        string[] bytes =
            UiShaderSourcePoolReplenishmentIterationProjectionRules.CapturedEntryBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int start = checked((int)(rawStart - entryRawFileOffset));
        int length = checked((int)(rawEndExclusive - rawStart));
        return start >= 0
            && length >= 0
            && start + length <= bytes.Length
            && string.Join(" ", bytes.Skip(start).Take(length)) == expected;
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
