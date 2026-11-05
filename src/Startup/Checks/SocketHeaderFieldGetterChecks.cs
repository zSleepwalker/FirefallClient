using Firefall.Reconstruction.Startup;

internal static class SocketHeaderFieldGetterChecks
{
    public static void Run(Action<string, bool> check)
    {
        SocketHeaderFieldGetterEntry first = SocketHeaderFieldGetterEntry.First;
        SocketHeaderFieldGetterEntry second = SocketHeaderFieldGetterEntry.Second;

        check("Header-field getter anchors pin both entries and the exclusive 35-byte code boundaries",
            SocketHeaderFieldGetterProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && SocketHeaderFieldGetterProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x00400C00
            && SocketHeaderFieldGetterProjection.EntryRawFileOffset(first) == 0x00BD5BA0
            && SocketHeaderFieldGetterProjection.EntryRawFileOffset(second) == 0x00BD5C00
            && SocketHeaderFieldGetterProjection.EntryVirtualAddress(first) == 0x00FD67A0
            && SocketHeaderFieldGetterProjection.EntryVirtualAddress(second) == 0x00FD6800
            && SocketHeaderFieldGetterProjection.EntryRawFileOffset(first)
                + SocketHeaderFieldGetterProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00FD67A0
            && SocketHeaderFieldGetterProjection.EntryRawFileOffset(second)
                + SocketHeaderFieldGetterProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00FD6800
            && SocketHeaderFieldGetterProjection.FunctionLength == 0x23
            && SocketHeaderFieldGetterProjection.CapturedWindowLength == 0x24
            && SocketHeaderFieldGetterProjection.FunctionEndRawFileOffsetExclusive(first) == 0x00BD5BC3
            && SocketHeaderFieldGetterProjection.FunctionEndRawFileOffsetExclusive(second) == 0x00BD5C23
            && SocketHeaderFieldGetterProjection.FunctionEndVirtualAddressExclusive(first) == 0x00FD67C3
            && SocketHeaderFieldGetterProjection.FunctionEndVirtualAddressExclusive(second) == 0x00FD6823
            && SocketHeaderFieldGetterProjection.CapturedWindowEndRawFileOffsetExclusive(first) == 0x00BD5BC4
            && SocketHeaderFieldGetterProjection.CapturedWindowEndRawFileOffsetExclusive(second) == 0x00BD5C24);

        check("Both getter bodies match the exact 35 code bytes and include one captured CC padding byte",
            SocketHeaderFieldGetterProjection.CapturedFunctionBytes
                == "56 8B F1 83 3E 00 75 17 6A 00 6A 5B 68 58 EA C5 01 "
                    + "68 58 EA C5 01 FF 15 A0 5E C1 01 83 C4 10 8B 06 5E C3"
            && SocketHeaderFieldGetterProjection.CapturedWindowBytes
                == SocketHeaderFieldGetterProjection.CapturedFunctionBytes + " CC"
            && SocketHeaderFieldGetterProjection.CapturedFunctionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x23
            && SocketHeaderFieldGetterProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x24);

        check("Both entries preserve the compare, exact JNZ target, diagnostic call, final reload, and plain RET",
            InstructionsMatch(first)
            && InstructionsMatch(second)
            && Offset(first, 0x06) + 2 + 0x17 == 0x00BD5BBF
            && Offset(second, 0x06) + 2 + 0x17 == 0x00BD5C1F
            && unchecked(0x00BD5BA0u + 0x06 + 2 + 0x17 + 0x00400C00) == 0x00FD67BF
            && unchecked(0x00BD5C00u + 0x06 + 2 + 0x17 + 0x00400C00) == 0x00FD681F
            && SocketHeaderFieldGetterProjection.DiagnosticCallOffset == 0x16
            && SocketHeaderFieldGetterProjection.DiagnosticCallReturnOffset == 0x1C
            && SocketHeaderFieldGetterProjection.FinalFieldReloadOffset == 0x1F
            && SocketHeaderFieldGetterProjection.RestoreEsiOffset == 0x21
            && SocketHeaderFieldGetterProjection.PlainReturnOffset == 0x22
            && SocketHeaderFieldGetterProjection.DiagnosticArgumentZero == 0
            && SocketHeaderFieldGetterProjection.DiagnosticCode == 0x5B
            && SocketHeaderFieldGetterProjection.DiagnosticStringPointer == 0x01C5EA58
            && SocketHeaderFieldGetterProjection.DiagnosticFunctionPointerCellVirtualAddress == 0x01C15EA0);

        SocketHeaderFieldGetterPlan nonzeroInitialCanReloadZero =
            SocketHeaderFieldGetterProjection.Project(
                new(
                    first,
                    ReceiverPointer: 0x10203040,
                    InitialDword: 0xA,
                    DiagnosticReturnedNormally: null,
                    FinalDword: 0));
        SocketHeaderFieldGetterPlan zeroInitialCanReloadDifferentValue =
            SocketHeaderFieldGetterProjection.Project(
                new(
                    second,
                    ReceiverPointer: 0x20304050,
                    InitialDword: 0,
                    DiagnosticReturnedNormally: true,
                    FinalDword: 0xDEADBEEF));
        check("The final DWORD is independently captured after either path and is not tested",
            nonzeroInitialCanReloadZero.Outcome == SocketHeaderFieldGetterOutcome.Completed
            && nonzeroInitialCanReloadZero.Capture.InitialDword == 0xA
            && nonzeroInitialCanReloadZero.Capture.FinalDword == 0
            && !nonzeroInitialCanReloadZero.DiagnosticInvoked
            && nonzeroInitialCanReloadZero.FinalFieldAddress == 0x10203040
            && nonzeroInitialCanReloadZero.FinalFieldDword == 0
            && nonzeroInitialCanReloadZero.StopAtRawFileOffsetExclusive == 0x00BD5BC3
            && zeroInitialCanReloadDifferentValue.Outcome == SocketHeaderFieldGetterOutcome.Completed
            && zeroInitialCanReloadDifferentValue.Capture.InitialDword == 0
            && zeroInitialCanReloadDifferentValue.Capture.FinalDword == 0xDEADBEEF
            && zeroInitialCanReloadDifferentValue.DiagnosticInvoked
            && zeroInitialCanReloadDifferentValue.DiagnosticCallRawFileOffset == 0x00BD5C16
            && zeroInitialCanReloadDifferentValue.DiagnosticFunctionPointerCellVirtualAddress == 0x01C15EA0
            && zeroInitialCanReloadDifferentValue.FinalFieldAddress == 0x20304050
            && zeroInitialCanReloadDifferentValue.FinalFieldDword == 0xDEADBEEF
            && zeroInitialCanReloadDifferentValue.StopAtRawFileOffsetExclusive == 0x00BD5C23);

        SocketHeaderFieldGetterPlan missingInitial =
            SocketHeaderFieldGetterProjection.Project(new(first, 0x12345678, null, null, null));
        SocketHeaderFieldGetterPlan missingInitialWithLaterCapture =
            SocketHeaderFieldGetterProjection.Project(new(first, 0x12345678, null, null, 7));
        SocketHeaderFieldGetterPlan missingDiagnosticReturn =
            SocketHeaderFieldGetterProjection.Project(new(first, 0x12345678, 0, null, null));
        SocketHeaderFieldGetterPlan missingDiagnosticWithUnreachedFinal =
            SocketHeaderFieldGetterProjection.Project(new(first, 0x12345678, 0, null, 7));
        SocketHeaderFieldGetterPlan diagnosticDidNotReturn =
            SocketHeaderFieldGetterProjection.Project(new(first, 0x12345678, 0, false, null));
        SocketHeaderFieldGetterPlan diagnosticDidNotReturnWithUnreachedFinal =
            SocketHeaderFieldGetterProjection.Project(new(first, 0x12345678, 0, false, 7));
        SocketHeaderFieldGetterPlan missingFinalAfterDiagnostic =
            SocketHeaderFieldGetterProjection.Project(new(first, 0x12345678, 0, true, null));
        SocketHeaderFieldGetterPlan missingFinalAfterSkippedDiagnostic =
            SocketHeaderFieldGetterProjection.Project(new(first, 0x12345678, 1, null, null));
        SocketHeaderFieldGetterPlan diagnosticOnSkippedPath =
            SocketHeaderFieldGetterProjection.Project(new(first, 0x12345678, 1, true, null));
        check("Missing and unreachable captures stop at the strict initial, call-return, and final-load gates",
            missingInitial.Outcome == SocketHeaderFieldGetterOutcome.MissingInitialDword
            && missingInitial.StopAtRawFileOffsetExclusive == 0x00BD5BA3
            && missingInitialWithLaterCapture.Outcome == SocketHeaderFieldGetterOutcome.UnreachedCaptureProvided
            && missingDiagnosticReturn.Outcome == SocketHeaderFieldGetterOutcome.MissingDiagnosticReturnCapture
            && missingDiagnosticReturn.StopAtRawFileOffsetExclusive == 0x00BD5BB6
            && missingDiagnosticWithUnreachedFinal.Outcome == SocketHeaderFieldGetterOutcome.UnreachedCaptureProvided
            && diagnosticDidNotReturn.Outcome == SocketHeaderFieldGetterOutcome.DiagnosticDidNotReturnNormally
            && diagnosticDidNotReturnWithUnreachedFinal.Outcome
                == SocketHeaderFieldGetterOutcome.UnreachedCaptureProvided
            && missingFinalAfterDiagnostic.Outcome == SocketHeaderFieldGetterOutcome.MissingFinalDword
            && missingFinalAfterDiagnostic.StopAtRawFileOffsetExclusive == 0x00BD5BBF
            && missingFinalAfterSkippedDiagnostic.Outcome == SocketHeaderFieldGetterOutcome.MissingFinalDword
            && missingFinalAfterSkippedDiagnostic.StopAtRawFileOffsetExclusive == 0x00BD5BBF
            && diagnosticOnSkippedPath.Outcome == SocketHeaderFieldGetterOutcome.UnreachedCaptureProvided);

        SocketHeaderFieldGetterPlan wrappedAddress =
            SocketHeaderFieldGetterProjection.Project(
                new(first, uint.MaxValue, 1, null, 0));
        check("The zero-offset receiver field address remains a DWORD for a high-bit pointer",
            wrappedAddress.Outcome == SocketHeaderFieldGetterOutcome.Completed
            && wrappedAddress.InitialFieldAddress == uint.MaxValue
            && wrappedAddress.FinalFieldAddress == uint.MaxValue
            && wrappedAddress.FinalFieldDword == 0);

        check("Entry selection rejects enum values that do not identify one of the two identical functions",
            ThrowsArgumentOutOfRangeException(
                () => SocketHeaderFieldGetterProjection.EntryRawFileOffset((SocketHeaderFieldGetterEntry)2)));
    }

    private static bool InstructionsMatch(SocketHeaderFieldGetterEntry entry)
    {
        uint at(uint offset) => Offset(entry, offset);
        uint branchTarget = at(0x1F);
        return SocketHeaderFieldGetterProjection.Instructions(entry)
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.TargetVirtualAddress))
            .SequenceEqual(new (
                uint RawFileOffset,
                string Bytes,
                uint? BranchTargetRawFileOffset,
                uint? TargetVirtualAddress)[]
            {
                (at(0), "56", null, null),
                (at(0x01), "8B F1", null, null),
                (at(0x03), "83 3E 00", null, null),
                (at(0x06), "75 17", branchTarget, branchTarget + 0x00400C00),
                (at(0x08), "6A 00", null, null),
                (at(0x0A), "6A 5B", null, null),
                (at(0x0C), "68 58 EA C5 01", null, 0x01C5EA58),
                (at(0x11), "68 58 EA C5 01", null, 0x01C5EA58),
                (at(0x16), "FF 15 A0 5E C1 01", null, 0x01C15EA0),
                (at(0x1C), "83 C4 10", null, null),
                (at(0x1F), "8B 06", null, null),
                (at(0x21), "5E", null, null),
                (at(0x22), "C3", null, null)
            });
    }

    private static uint Offset(SocketHeaderFieldGetterEntry entry, uint offset) =>
        unchecked(SocketHeaderFieldGetterProjection.EntryRawFileOffset(entry) + offset);

    private static bool ThrowsArgumentOutOfRangeException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }
}
