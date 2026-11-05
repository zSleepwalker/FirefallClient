using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolResetEntryChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Reset-entry sample, raw interval, and code-VA mapping are exact",
            UiShaderSourcePoolResetEntryProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolResetEntryProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolResetEntryProjectionRules.EntryRawFileOffset == 0x12CDC50
            && UiShaderSourcePoolResetEntryProjectionRules.EntryVirtualAddress == 0x16CE850
            && UiShaderSourcePoolResetEntryProjectionRules.FenceRawFileOffset == 0x12CDC9D
            && UiShaderSourcePoolResetEntryProjectionRules.PoolCounterCompareRawFileOffset == 0x12CDCA0
            && UiShaderSourcePoolResetEntryProjectionRules.NonzeroCounterBranchRawFileOffset == 0x12CDCAA
            && UiShaderSourcePoolResetEntryProjectionRules.NonzeroCounterContinuationRawFileOffset
                == 0x12CDCC2
            && UiShaderSourcePoolResetEntryProjectionRules.ZeroCounterEaxClearRawFileOffset == 0x12CDCAC
            && UiShaderSourcePoolResetEntryProjectionRules.ZeroCounterReturnRawFileOffset == 0x12CDCBF
            && UiShaderSourcePoolResetEntryProjectionRules.EntryEndRawFileOffsetExclusive == 0x12CDCC2
            && UiShaderSourcePoolResetEntryProjectionRules.EntryEndVirtualAddressExclusive == 0x16CE8C2
            && UiShaderSourcePoolResetEntryProjectionRules.EntryRawFileOffset
                + UiShaderSourcePoolResetEntryProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolResetEntryProjectionRules.EntryVirtualAddress);

        check("Captured entry bytes equal the exact 0x72-byte interval through the zero-counter return",
            UiShaderSourcePoolResetEntryProjectionRules.CapturedEntryBytes
                == "55 8B EC 6A FF 68 48 5A A9 01 64 A1 00 00 00 00 "
                    + "50 83 EC 28 53 56 57 A1 00 50 DC 01 33 C5 50 8D "
                    + "45 F4 64 A3 00 00 00 00 8B F9 89 7D E4 8B 45 08 "
                    + "85 C0 75 19 50 68 15 02 00 00 68 BF 41 B7 01 68 "
                    + "E3 41 B7 01 FF 15 A0 5E C1 01 83 C4 10 0F AE E8 "
                    + "83 7F 10 00 8D 47 10 89 45 D8 75 16 33 C0 8B 4D "
                    + "F4 64 89 0D 00 00 00 00 59 5F 5E 5B 8B E5 5D C2 04 00"
            && UiShaderSourcePoolResetEntryProjectionRules.CapturedEntryBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x72
            && UiShaderSourcePoolResetEntryProjectionRules.EntryRawFileOffset
                + (uint)UiShaderSourcePoolResetEntryProjectionRules.CapturedEntryBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                == UiShaderSourcePoolResetEntryProjectionRules.EntryEndRawFileOffsetExclusive);

        check("Callback-null diagnostic, post-report fence, and later counter branches match exact bytes",
            UiShaderSourcePoolResetEntryProjectionRules.CallbackNullDiagnosticId == 0x215
            && UiShaderSourcePoolResetEntryProjectionRules.CallbackNullDiagnosticId == 533
            && UiShaderSourcePoolResetEntryProjectionRules.ThisPoolCounterOffset == 0x10
            && UiShaderSourcePoolResetEntryProjectionRules.DiagnosticReporterCellVirtualAddress
                == 0x01C15EA0
            && UiShaderSourcePoolResetEntryProjectionRules.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.ThisRelativeOffset,
                        instruction.DiagnosticId, instruction.BranchTargetRawFileOffset,
                        instruction.TargetVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    int? ThisRelativeOffset,
                    uint? DiagnosticId,
                    uint? BranchTargetRawFileOffset,
                    uint? TargetVirtualAddress)[]
                {
                    (0x12CDC7D, "8B 45 08", null, null, null, null),
                    (0x12CDC80, "85 C0", null, null, null, null),
                    (0x12CDC82, "75 19", null, null, 0x12CDC9D, null),
                    (0x12CDC84, "50", null, null, null, null),
                    (0x12CDC85, "68 15 02 00 00", null, 0x215, null, null),
                    (0x12CDC94, "FF 15 A0 5E C1 01", null, null, null, 0x01C15EA0),
                    (0x12CDC9A, "83 C4 10", null, null, null, null),
                    (0x12CDC9D, "0F AE E8", null, null, null, null),
                    (0x12CDCA0, "83 7F 10 00", 0x10, null, null, null),
                    (0x12CDCAA, "75 16", null, null, 0x12CDCC2, null),
                    (0x12CDCAC, "33 C0", null, null, null, null),
                    (0x12CDCBF, "C2 04 00", null, null, null, null)
                })
            && UiShaderSourcePoolResetEntryProjectionRules.EntryRawFileOffset
                + 0x72
                == UiShaderSourcePoolResetEntryProjectionRules.EntryEndRawFileOffsetExclusive
            && UiShaderSourcePoolResetEntryProjectionRules.NonzeroCounterBranchRawFileOffset
                + 2 + 0x16
                == UiShaderSourcePoolResetEntryProjectionRules.NonzeroCounterContinuationRawFileOffset
            && UiShaderSourcePoolResetEntryProjectionRules.EntryRawFileOffset
                + UiShaderSourcePoolResetEntryProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolResetEntryProjectionRules.EntryVirtualAddress);

        string capturedBytes = string.Concat(
            UiShaderSourcePoolResetEntryProjectionRules.CapturedEntryBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        check("Every instruction anchor matches its exact slice in the captured interval",
            UiShaderSourcePoolResetEntryProjectionRules.Instructions.All(instruction =>
            {
                if (instruction.RawFileOffset
                    < UiShaderSourcePoolResetEntryProjectionRules.EntryRawFileOffset)
                {
                    return false;
                }

                string expectedSlice = string.Concat(
                    instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                int start = checked((int)(instruction.RawFileOffset
                    - UiShaderSourcePoolResetEntryProjectionRules.EntryRawFileOffset) * 2);
                return start + expectedSlice.Length <= capturedBytes.Length
                    && capturedBytes.Substring(start, expectedSlice.Length) == expectedSlice;
            }));

        UiShaderSourcePoolResetEntryProjection zeroCounter =
            UiShaderSourcePoolResetEntryProjectionRules.Project(
                new UiShaderSourcePoolResetEntryCapture(
                    CallbackArgument: 0,
                    LaterPoolCounter: 0));
        check("A null callback selects 0x215 and a later zero counter returns EAX=0",
            zeroCounter.SelectedDiagnosticId == 0x215
            && zeroCounter.Boundary
                == UiShaderSourcePoolResetEntryBoundary.ZeroCounterEarlyReturn
            && zeroCounter.ReturnEax == 0
            && zeroCounter.Capture.CallbackArgument == 0
            && zeroCounter.Capture.LaterPoolCounter == 0);

        UiShaderSourcePoolResetEntryProjection nonzeroCounter =
            UiShaderSourcePoolResetEntryProjectionRules.Project(
                new UiShaderSourcePoolResetEntryCapture(
                    CallbackArgument: 0x1234,
                    LaterPoolCounter: uint.MaxValue));
        check("A non-null callback skips diagnostic selection and nonzero counter stops at unmodeled reset work",
            nonzeroCounter.SelectedDiagnosticId is null
            && nonzeroCounter.Boundary
                == UiShaderSourcePoolResetEntryBoundary.NonzeroCounterResetWorkUnmodeled
            && nonzeroCounter.ReturnEax is null
            && nonzeroCounter.Capture.CallbackArgument == 0x1234
            && nonzeroCounter.Capture.LaterPoolCounter == uint.MaxValue);

        UiShaderSourcePoolResetEntryProjection diagnosticDoesNotReturn =
            UiShaderSourcePoolResetEntryProjectionRules.Project(
                new UiShaderSourcePoolResetEntryCapture(
                    CallbackArgument: 0,
                    LaterPoolCounter: null,
                    DiagnosticCallReturnedNormally: false));
        check("A nonreturning selected diagnostic stops before the fence and counter read",
            diagnosticDoesNotReturn.SelectedDiagnosticId == 0x215
            && diagnosticDoesNotReturn.Boundary
                == UiShaderSourcePoolResetEntryBoundary.DiagnosticDidNotReturnNormally
            && diagnosticDoesNotReturn.ReturnEax is null);

        bool missingReachedCounterRejected = ThrowsArgumentException(() =>
            UiShaderSourcePoolResetEntryProjectionRules.Project(
                new UiShaderSourcePoolResetEntryCapture(
                    CallbackArgument: 1,
                    LaterPoolCounter: null)));
        bool counterAfterNonreturningDiagnosticRejected = ThrowsArgumentException(() =>
            UiShaderSourcePoolResetEntryProjectionRules.Project(
                new UiShaderSourcePoolResetEntryCapture(
                    CallbackArgument: 0,
                    LaterPoolCounter: 1,
                    DiagnosticCallReturnedNormally: false)));
        check("Counter captures are required only after the reporter-return/fence gate",
            missingReachedCounterRejected && counterAfterNonreturningDiagnosticRejected);
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
