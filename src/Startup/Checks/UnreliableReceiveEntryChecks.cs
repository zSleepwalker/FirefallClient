using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveEntryChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The sample, raw-to-VA mapping, exclusive boundary, and captured guard bytes are pinned",
            UnreliableReceiveEntryProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveEntryProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x00400C00
            && UnreliableReceiveEntryProjection.EntryStartRawFileOffset == 0x00BE5A40
            && UnreliableReceiveEntryProjection.EntryStartVirtualAddress == 0x00FE6640
            && UnreliableReceiveEntryProjection.RawFileOffsetToVirtualAddress(0x00BE5A40) == 0x00FE6640
            && UnreliableReceiveEntryProjection.RawFileOffsetToVirtualAddress(0x00BE5A6B) == 0x00FE666B
            && UnreliableReceiveEntryProjection.PrefixEndRawFileOffsetExclusive == 0x00BE5A6B
            && UnreliableReceiveEntryProjection.CapturedGuardBytes
                == "55 8B EC 83 EC 08 53 56 57 8B F9 80 7F 0C 00 75 1A 6A 00 "
                    + "68 AF 02 00 00 68 2F 8C B5 01 68 3D 8C B5 01 FF 15 A0 5E "
                    + "C1 01 83 C4 10"
            && UnreliableReceiveEntryProjection.CapturedGuardBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x2B);

        check("ECX becomes EDI before an unguarded byte dereference and JNZ reaches 0xBE5A6B",
            UnreliableReceiveEntryProjection.ReceiverFlagOffset == 0x0C
            && UnreliableReceiveEntryProjection.ReceiverLoadRawFileOffset == 0x00BE5A49
            && UnreliableReceiveEntryProjection.FlagCompareRawFileOffset == 0x00BE5A4B
            && UnreliableReceiveEntryProjection.DiagnosticBranchRawFileOffset == 0x00BE5A4F
            && ShortBranchTarget(0x00BE5A4F, 0x1A) == 0x00BE5A6B
            && InstructionEvidenceMatches());

        check("The diagnostic code is 0x2AF and is distinct from reporter pointer slot 0x1C15EA0",
            UnreliableReceiveEntryProjection.DiagnosticReportCode == 0x2AF
            && UnreliableReceiveEntryProjection.DiagnosticReporterPointerSlotAddress == 0x01C15EA0
            && UnreliableReceiveEntryProjection.DiagnosticLiteralAtAddress1B58C2F == 0x01B58C2F
            && UnreliableReceiveEntryProjection.DiagnosticLiteralAtAddress1B58C3D == 0x01B58C3D
            && UnreliableReceiveEntryProjection.DiagnosticPushValuesInInstructionOrder.SequenceEqual(
                new uint[] { 0, 0x2AF, 0x01B58C2F, 0x01B58C3D }));

        UnreliableReceiveEntryPlan missingFlag =
            UnreliableReceiveEntryProjection.Project(0x1000, null, null);
        UnreliableReceiveEntryPlan missingFlagWithLaterCapture =
            UnreliableReceiveEntryProjection.Project(0x1000, null, true);
        UnreliableReceiveEntryPlan skippedDiagnosticWithLaterCapture =
            UnreliableReceiveEntryProjection.Project(0x1000, 1, true);
        UnreliableReceiveEntryPlan missingDiagnosticReturn =
            UnreliableReceiveEntryProjection.Project(0x1000, 0, null);
        UnreliableReceiveEntryPlan diagnosticDidNotReturn =
            UnreliableReceiveEntryProjection.Project(0x1000, 0, false);
        check("Missing captures stop strictly; supplied captures from unreached instructions are visible",
            missingFlag.Outcome == UnreliableReceiveEntryOutcome.MissingFlagCapture
            && missingFlag.StopAtRawFileOffsetExclusive
                == UnreliableReceiveEntryProjection.FlagCompareRawFileOffset
            && missingFlagWithLaterCapture.Outcome
                == UnreliableReceiveEntryOutcome.MissingFlagCaptureWithUnreachedDiagnosticReturnCapture
            && missingFlagWithLaterCapture.UnreachedDiagnosticReturnCaptureProvided
            && missingFlagWithLaterCapture.DiagnosticCallReached is null
            && skippedDiagnosticWithLaterCapture.Outcome
                == UnreliableReceiveEntryOutcome.UnreachedDiagnosticReturnCapture
            && skippedDiagnosticWithLaterCapture.UnreachedDiagnosticReturnCaptureProvided
            && skippedDiagnosticWithLaterCapture.Capture.DiagnosticCallReturnedNormally == true
            && skippedDiagnosticWithLaterCapture.DiagnosticCallReturnedNormally is null
            && missingDiagnosticReturn.Outcome
                == UnreliableReceiveEntryOutcome.MissingDiagnosticReturnCapture
            && missingDiagnosticReturn.DiagnosticCallReached == true
            && missingDiagnosticReturn.GuardBoundaryReached == false
            && diagnosticDidNotReturn.Outcome
                == UnreliableReceiveEntryOutcome.DiagnosticCallDidNotReturnNormally
            && diagnosticDidNotReturn.GuardBoundaryReached == false
            && diagnosticDidNotReturn.SelectedDestinationRawFileOffset is null);

        bool everyFlagByteSelectsOnlyTheGuardBranch = Enumerable.Range(byte.MinValue, byte.MaxValue + 1)
            .All(value =>
            {
                byte flag = (byte)value;
                bool zero = flag == 0;
                UnreliableReceiveEntryPlan plan =
                    UnreliableReceiveEntryProjection.Project(0x1000, flag, zero ? true : null);
                return plan.FlagIsZero == zero
                    && plan.DiagnosticBranchTaken == !zero
                    && plan.DiagnosticCallReached == zero
                    && plan.DiagnosticCallReturnedNormally == (zero ? true : null)
                    && plan.GuardBoundaryReached == true
                    && plan.SelectedDestinationRawFileOffset
                        == UnreliableReceiveEntryProjection.GuardBoundaryRawFileOffsetExclusive
                    && plan.StopAtRawFileOffsetExclusive
                        == UnreliableReceiveEntryProjection.GuardBoundaryRawFileOffsetExclusive;
            });
        check("All 256 byte patterns select only the skip path or the diagnostic call path",
            everyFlagByteSelectsOnlyTheGuardBranch);

        UnreliableReceiveEntryPlan wrapped =
            UnreliableReceiveEntryProjection.Project(uint.MaxValue, 1, null);
        check("Receiver-plus-offset address arithmetic wraps as an unchecked 32-bit address",
            wrapped.FlagAddress == 0x0000000B
            && UnreliableReceiveEntryProjection.Project(0xFFFFFFF4, 0, true).FlagAddress == 0);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE5A40, "55 8B EC 83 EC 08 53 56 57 8B F9", null, null),
            (0x00BE5A4B, "80 7F 0C 00", null, null),
            (0x00BE5A4F, "75 1A", 0x00BE5A6B, 0x00FE666B),
            (0x00BE5A51, "6A 00 68 AF 02 00 00 68 2F 8C B5 01 68 3D 8C B5 01", null, null),
            (0x00BE5A62, "FF 15 A0 5E C1 01", null, null),
            (0x00BE5A68, "83 C4 10", null, null)
        };
        return UnreliableReceiveEntryProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);
}
