using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolResetAlignmentChecks
{
    public static void Run(Action<string, bool> check)
    {
        string[] capturedByteValues =
            UiShaderSourcePoolResetAlignmentProjectionRules.CapturedBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        check("Reset alignment anchors match the pinned sample and raw-offset-to-VA mapping",
            UiShaderSourcePoolResetAlignmentProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolResetAlignmentProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolResetAlignmentProjectionRules.ResetFunctionRawFileOffset
                == 0x12CDC50
            && UiShaderSourcePoolResetAlignmentProjectionRules.ResetFunctionVirtualAddress
                == 0x16CE850
            && UiShaderSourcePoolResetAlignmentProjectionRules.EntryRawFileOffset == 0x12CDD68
            && UiShaderSourcePoolResetAlignmentProjectionRules.EntryVirtualAddress == 0x16CE968
            && UiShaderSourcePoolResetAlignmentProjectionRules.EndRawFileOffsetExclusive == 0x12CDDA8
            && UiShaderSourcePoolResetAlignmentProjectionRules.EndVirtualAddressExclusive == 0x16CE9A8
            && UiShaderSourcePoolResetAlignmentProjectionRules.RawFileOffsetToCodeVirtualAddress(
                UiShaderSourcePoolResetAlignmentProjectionRules.EntryRawFileOffset) == 0x16CE968
            && UiShaderSourcePoolResetAlignmentProjectionRules.RawFileOffsetToCodeVirtualAddress(
                UiShaderSourcePoolResetAlignmentProjectionRules.DiagnosticCallRawFileOffset)
                == 0x16CE99F
            && UiShaderSourcePoolResetAlignmentProjectionRules.RawFileOffsetToCodeVirtualAddress(
                UiShaderSourcePoolResetAlignmentProjectionRules.EndRawFileOffsetExclusive)
                == 0x16CE9A8
            && UiShaderSourcePoolResetAlignmentProjectionRules.EndRawFileOffsetExclusive
                - UiShaderSourcePoolResetAlignmentProjectionRules.EntryRawFileOffset == 0x40
            && UiShaderSourcePoolResetAlignmentProjectionRules.PoolAlignmentDivisorFieldOffset == 0x18
            && UiShaderSourcePoolResetAlignmentProjectionRules.SavedCandidateStackOffset == -0x1C
            && UiShaderSourcePoolResetAlignmentProjectionRules.DiagnosticCode == 0x227
            && UiShaderSourcePoolResetAlignmentProjectionRules.DiagnosticPointerCellVirtualAddress
                == 0x01C15EA0);

        check("Captured bytes and ordered instruction slices match the reset alignment range",
            capturedByteValues.Length == 0x40
            && UiShaderSourcePoolResetAlignmentProjectionRules.CapturedBytes
                == "8B 4B 18 8D 78 04 33 D2 89 7D E4 8B C7 F7 F1 85 D2 74 09 "
                    + "8B C1 2B C2 03 F8 89 7D E4 33 D2 8B C7 F7 F1 85 D2 74 1A "
                    + "6A 00 68 27 02 00 00 68 D7 41 B7 01 68 87 43 B7 01 FF 15 "
                    + "A0 5E C1 01 83 C4 10"
            && UiShaderSourcePoolResetAlignmentProjectionRules.Instructions
                .Count(instruction =>
                    instruction.RawFileOffset
                        == UiShaderSourcePoolResetAlignmentProjectionRules.DivisorLoadRawFileOffset
                    && instruction.FieldOffset
                        == UiShaderSourcePoolResetAlignmentProjectionRules.PoolAlignmentDivisorFieldOffset)
                == 1
            && UiShaderSourcePoolResetAlignmentProjectionRules.Instructions
                .All(instruction => HasExactSlice(instruction, capturedByteValues))
            && UiShaderSourcePoolResetAlignmentProjectionRules.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.BranchTargetRawFileOffset,
                        instruction.FieldOffset, instruction.Immediate,
                        instruction.PointerCellVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    uint? BranchTargetRawFileOffset,
                    int? FieldOffset,
                    uint? Immediate,
                    uint? PointerCellVirtualAddress)[]
                {
                    (0x12CDD68, null, 0x18, null, null),
                    (0x12CDD6B, null, null, 4, null),
                    (0x12CDD6E, null, null, null, null),
                    (0x12CDD70, null, -0x1C, null, null),
                    (0x12CDD73, null, null, null, null),
                    (0x12CDD75, null, null, null, null),
                    (0x12CDD77, null, null, null, null),
                    (0x12CDD79, 0x12CDD84, null, null, null),
                    (0x12CDD7B, null, null, null, null),
                    (0x12CDD7D, null, null, null, null),
                    (0x12CDD7F, null, null, null, null),
                    (0x12CDD81, null, -0x1C, null, null),
                    (0x12CDD84, null, null, null, null),
                    (0x12CDD86, null, null, null, null),
                    (0x12CDD88, null, null, null, null),
                    (0x12CDD8A, null, null, null, null),
                    (0x12CDD8C, 0x12CDDA8, null, null, null),
                    (0x12CDD8E, null, null, 0, null),
                    (0x12CDD90, null, null, 0x227, null),
                    (0x12CDD95, null, null, 0x01B741D7, null),
                    (0x12CDD9A, null, null, 0x01B74387, null),
                    (0x12CDD9F, null, null, null, 0x01C15EA0),
                    (0x12CDDA5, null, null, null, null)
                })
            && UiShaderSourcePoolResetAlignmentProjectionRules.Instructions[^1]
                .RawFileOffset + 3
                == UiShaderSourcePoolResetAlignmentProjectionRules.EndRawFileOffsetExclusive);

        UiShaderSourcePoolResetAlignmentProjection noCorrection =
            UiShaderSourcePoolResetAlignmentProjectionRules.Project(
                new UiShaderSourcePoolResetAlignmentCapture(
                    IncomingNodeValue: 0x1000,
                    PoolAlignmentDivisor: 4,
                    DiagnosticCallReturned: false));
        check("A zero first remainder skips correction and branches out when the second remainder is zero",
            noCorrection.InitialCandidate == 0x1004
            && noCorrection.FirstRemainder == 0
            && noCorrection.AlignmentCorrection is null
            && noCorrection.CandidateForSecondDivision == 0x1004
            && noCorrection.SecondRemainder == 0
            && !noCorrection.DiagnosticCallReached
            && !noCorrection.DiagnosticReturnedNormally
            && !noCorrection.DiagnosticCleanupReached
            && noCorrection.ContinuationBoundaryReached
            && noCorrection.StopRawFileOffset == 0x12CDDA8
            && noCorrection.Outcome
                == UiShaderSourcePoolResetAlignmentOutcome.ReturnedToBoundaryWithoutDiagnostic
            && noCorrection.Actions.Select(action => action.RawFileOffset).SequenceEqual(new uint[]
            {
                0x12CDD68, 0x12CDD6B, 0x12CDD6E, 0x12CDD70, 0x12CDD73, 0x12CDD75,
                0x12CDD77, 0x12CDD79, 0x12CDD84, 0x12CDD86, 0x12CDD88, 0x12CDD8A,
                0x12CDD8C
            })
            && noCorrection.Actions[7].BranchTaken == true
            && noCorrection.Actions[7].BranchTargetRawFileOffset == 0x12CDD84
            && noCorrection.Actions[^1].BranchTaken == true
            && noCorrection.Actions[^1].BranchTargetRawFileOffset == 0x12CDDA8);

        UiShaderSourcePoolResetAlignmentProjection diagnosticReturned =
            UiShaderSourcePoolResetAlignmentProjectionRules.Project(
                new UiShaderSourcePoolResetAlignmentCapture(
                    IncomingNodeValue: 0xFFFFFFFB,
                    PoolAlignmentDivisor: 7,
                    DiagnosticCallReturned: true));
        check("Wrapped incoming-plus-four correction reaches diagnostic 0x227 then reaches the boundary only after return",
            diagnosticReturned.InitialCandidate == uint.MaxValue
            && diagnosticReturned.FirstRemainder == 3
            && diagnosticReturned.AlignmentCorrection == 4
            && diagnosticReturned.CandidateForSecondDivision == 3
            && diagnosticReturned.SecondRemainder == 3
            && diagnosticReturned.DiagnosticCallReached
            && diagnosticReturned.DiagnosticReturnedNormally
            && diagnosticReturned.DiagnosticCleanupReached
            && diagnosticReturned.ContinuationBoundaryReached
            && diagnosticReturned.StopRawFileOffset == 0x12CDDA8
            && diagnosticReturned.Outcome
                == UiShaderSourcePoolResetAlignmentOutcome.DiagnosticReturnedToBoundary
            && diagnosticReturned.Actions.Select(action => action.RawFileOffset).SequenceEqual(new uint[]
            {
                0x12CDD68, 0x12CDD6B, 0x12CDD6E, 0x12CDD70, 0x12CDD73, 0x12CDD75,
                0x12CDD77, 0x12CDD79, 0x12CDD7B, 0x12CDD7D, 0x12CDD7F, 0x12CDD81,
                0x12CDD84, 0x12CDD86, 0x12CDD88, 0x12CDD8A, 0x12CDD8C, 0x12CDD8E,
                0x12CDD90, 0x12CDD95, 0x12CDD9A, 0x12CDD9F, 0x12CDDA5
            })
            && diagnosticReturned.Actions[7].BranchTaken == false
            && diagnosticReturned.Actions[16].BranchTaken == false
            && diagnosticReturned.Actions[^2].Kind
                == UiShaderSourcePoolResetAlignmentActionKind.CallDiagnosticIndirectly
            && diagnosticReturned.Actions[^1].Kind
                == UiShaderSourcePoolResetAlignmentActionKind.CleanUpDiagnosticArguments);

        UiShaderSourcePoolResetAlignmentProjection diagnosticNotReturned =
            UiShaderSourcePoolResetAlignmentProjectionRules.Project(
                new UiShaderSourcePoolResetAlignmentCapture(
                    IncomingNodeValue: 0xFFFFFFFB,
                    PoolAlignmentDivisor: 7,
                    DiagnosticCallReturned: false));
        check("A nonreturning diagnostic capture stops at the indirect call without projecting cleanup or the boundary",
            diagnosticNotReturned.DiagnosticCallReached
            && !diagnosticNotReturned.DiagnosticReturnedNormally
            && !diagnosticNotReturned.DiagnosticCleanupReached
            && !diagnosticNotReturned.ContinuationBoundaryReached
            && diagnosticNotReturned.StopRawFileOffset == 0x12CDD9F
            && diagnosticNotReturned.Outcome
                == UiShaderSourcePoolResetAlignmentOutcome.DiagnosticCallDidNotReturn
            && diagnosticNotReturned.Actions[^1].Kind
                == UiShaderSourcePoolResetAlignmentActionKind.CallDiagnosticIndirectly
            && diagnosticNotReturned.Actions.All(action =>
                action.Kind != UiShaderSourcePoolResetAlignmentActionKind.CleanUpDiagnosticArguments));

        UiShaderSourcePoolResetAlignmentProjection zeroDivisor =
            UiShaderSourcePoolResetAlignmentProjectionRules.Project(
                new UiShaderSourcePoolResetAlignmentCapture(
                    IncomingNodeValue: 0xFFFFFFFC,
                    PoolAlignmentDivisor: 0,
                    DiagnosticCallReturned: false));
        check("A zero divisor stops at the first DIV fault without fabricating a remainder",
            zeroDivisor.InitialCandidate == 0
            && zeroDivisor.FirstRemainder is null
            && zeroDivisor.AlignmentCorrection is null
            && zeroDivisor.CandidateForSecondDivision is null
            && zeroDivisor.SecondRemainder is null
            && !zeroDivisor.DiagnosticCallReached
            && !zeroDivisor.DiagnosticReturnedNormally
            && !zeroDivisor.DiagnosticCleanupReached
            && !zeroDivisor.ContinuationBoundaryReached
            && zeroDivisor.StopRawFileOffset == 0x12CDD75
            && zeroDivisor.Outcome
                == UiShaderSourcePoolResetAlignmentOutcome.DivideFaultAtFirstDivision
            && zeroDivisor.Actions[^1].Kind
                == UiShaderSourcePoolResetAlignmentActionKind.FirstUnsignedDivision
            && zeroDivisor.Actions.All(action =>
                action.Kind != UiShaderSourcePoolResetAlignmentActionKind.TestFirstRemainder));

        bool rejectedUnreachableDiagnosticReturn = false;
        try
        {
            UiShaderSourcePoolResetAlignmentProjectionRules.Project(
                new UiShaderSourcePoolResetAlignmentCapture(
                    IncomingNodeValue: 0x1000,
                    PoolAlignmentDivisor: 4,
                    DiagnosticCallReturned: true));
        }
        catch (ArgumentException)
        {
            rejectedUnreachableDiagnosticReturn = true;
        }

        check("A diagnostic-return capture is rejected when a zero second remainder skips the diagnostic",
            rejectedUnreachableDiagnosticReturn);
    }

    private static bool HasExactSlice(
        UiShaderSourcePoolResetAlignmentInstructionEvidence instruction,
        string[] capturedBytes)
    {
        uint startOffset = UiShaderSourcePoolResetAlignmentProjectionRules.EntryRawFileOffset;
        if (instruction.RawFileOffset < startOffset)
        {
            return false;
        }

        int start = checked((int)(instruction.RawFileOffset - startOffset));
        int length = instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        return start + length <= capturedBytes.Length
            && string.Join(" ", capturedBytes.Skip(start).Take(length)) == instruction.Bytes;
    }
}
