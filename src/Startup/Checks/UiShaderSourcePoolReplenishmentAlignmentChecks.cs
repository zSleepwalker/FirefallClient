using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolReplenishmentAlignmentChecks
{
    public static void Run(Action<string, bool> check)
    {
        string[] capturedAlignmentByteValues =
            UiShaderSourcePoolReplenishmentAlignmentProjectionRules.CapturedAlignmentBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        check("Alignment capture anchors match the pinned sample and raw-offset-to-VA mapping",
            UiShaderSourcePoolReplenishmentAlignmentProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.FirstPoolPushRawFileOffset
                == 0x12CE161
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.FirstPoolPushCallRawFileOffset
                == 0x12CE165
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryRawFileOffset
                == 0x12CE16A
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryVirtualAddress
                == 0x16CED6A
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryRawFileOffset
                + UiShaderSourcePoolReplenishmentAlignmentProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryVirtualAddress
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryEndRawFileOffsetExclusive
                == 0x12CE1AD
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryEndVirtualAddressExclusive
                == 0x16CEDAD
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryEndRawFileOffsetExclusive
                + UiShaderSourcePoolReplenishmentAlignmentProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryEndVirtualAddressExclusive
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryEndRawFileOffsetExclusive
                - UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryRawFileOffset
                == 0x43
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.StopBeforeRawFileOffset
                == UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryEndRawFileOffsetExclusive
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.PoolAlignmentDivisorFieldOffset == 0x18
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.DiagnosticCode == 0x2BA);

        check("Captured bytes and instruction anchors stop before XOR EBX at 0x12CE1AD",
            capturedAlignmentByteValues.Length == 0x43
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.CapturedAlignmentBytes
                == "8B 77 18 8D 4B 04 33 D2 89 4D D0 8B C1 F7 F6 85 D2 74 09 "
                    + "8B C6 2B C2 03 C8 89 4D D0 33 D2 8B C1 F7 F6 85 D2 74 1D "
                    + "6A 00 68 BA 02 00 00 68 CF 43 B7 01 68 E7 43 B7 01 FF 15 "
                    + "A0 5E C1 01 8B 4D D0 83 C4 10"
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.Instructions
                .Take(2)
                .Select(instruction => instruction.Bytes)
                .SequenceEqual(new[] { "53", "E8 F6 F8 FF FF" })
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.Instructions
                .Where(instruction =>
                    instruction.RawFileOffset
                        >= UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryRawFileOffset)
                .All(instruction =>
                {
                    int start = (int)(instruction.RawFileOffset
                        - UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryRawFileOffset);
                    int length = instruction.Bytes
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                    return start >= 0
                        && start + length <= capturedAlignmentByteValues.Length
                        && instruction.Bytes
                            == string.Join(" ", capturedAlignmentByteValues, start, length);
                })
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.BranchTargetRawFileOffset,
                        instruction.FieldOffset, instruction.Immediate))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    uint? BranchTargetRawFileOffset,
                    int? FieldOffset,
                    uint? Immediate)[]
                {
                    (0x12CE161, null, null, null),
                    (0x12CE165, null, null, null),
                    (0x12CE16A, null, 0x18, null),
                    (0x12CE16D, null, null, 4),
                    (0x12CE170, null, null, null),
                    (0x12CE172, null, null, null),
                    (0x12CE175, null, null, null),
                    (0x12CE177, null, null, null),
                    (0x12CE179, null, null, null),
                    (0x12CE17B, 0x12CE186, null, null),
                    (0x12CE17D, null, null, null),
                    (0x12CE17F, null, null, null),
                    (0x12CE181, null, null, null),
                    (0x12CE183, null, null, null),
                    (0x12CE186, null, null, null),
                    (0x12CE188, null, null, null),
                    (0x12CE18A, null, null, null),
                    (0x12CE18C, null, null, null),
                    (0x12CE18E, 0x12CE1AD, null, null),
                    (0x12CE190, null, null, 0),
                    (0x12CE192, null, null, 0x2BA),
                    (0x12CE197, null, null, null),
                    (0x12CE19C, null, null, null),
                    (0x12CE1A1, null, null, null),
                    (0x12CE1A7, null, null, null),
                    (0x12CE1AA, null, null, null)
                })
            && UiShaderSourcePoolReplenishmentAlignmentProjectionRules.Instructions[^1]
                .RawFileOffset + 3
                == UiShaderSourcePoolReplenishmentAlignmentProjectionRules.EntryEndRawFileOffsetExclusive);

        UiShaderSourcePoolReplenishmentAlignmentProjection alreadyAligned =
            UiShaderSourcePoolReplenishmentAlignmentProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentAlignmentCapture(
                    AllocatedBlockValue: 0x1000,
                    PoolAlignmentDivisor: 4));
        check("An initially divisible candidate is kept and the second divide branches to the boundary",
            alreadyAligned.InitialCandidate == 0x1004
            && alreadyAligned.FirstRemainder == 0
            && alreadyAligned.AlignmentAdjustment == 0
            && alreadyAligned.Candidate == 0x1004
            && alreadyAligned.SecondRemainder == 0
            && alreadyAligned.Outcome
                == UiShaderSourcePoolReplenishmentAlignmentOutcome.NoDiagnosticBranchToBoundary
            && alreadyAligned.DiagnosticCode is null
            && alreadyAligned.DiagnosticCallRawFileOffset is null);

        UiShaderSourcePoolReplenishmentAlignmentProjection adjusted =
            UiShaderSourcePoolReplenishmentAlignmentProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentAlignmentCapture(
                    AllocatedBlockValue: 0x1001,
                    PoolAlignmentDivisor: 16));
        check("A nonzero first remainder adds divisor-minus-remainder before the second divide",
            adjusted.InitialCandidate == 0x1005
            && adjusted.FirstRemainder == 5
            && adjusted.AlignmentAdjustment == 11
            && adjusted.Candidate == 0x1010
            && adjusted.SecondRemainder == 0
            && adjusted.Outcome
                == UiShaderSourcePoolReplenishmentAlignmentOutcome.NoDiagnosticBranchToBoundary);

        UiShaderSourcePoolReplenishmentAlignmentProjection wrappingNonPowerOfTwo =
            UiShaderSourcePoolReplenishmentAlignmentProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentAlignmentCapture(
                    AllocatedBlockValue: 0xFFFFFFFB,
                    PoolAlignmentDivisor: 7));
        check("32-bit wrap with a non-power-of-two divisor can reach diagnostic 0x2BA",
            wrappingNonPowerOfTwo.InitialCandidate == uint.MaxValue
            && wrappingNonPowerOfTwo.FirstRemainder == 3
            && wrappingNonPowerOfTwo.AlignmentAdjustment == 4
            && wrappingNonPowerOfTwo.Candidate == 3
            && wrappingNonPowerOfTwo.SecondRemainder == 3
            && wrappingNonPowerOfTwo.Outcome
                == UiShaderSourcePoolReplenishmentAlignmentOutcome.DiagnosticCallReached
            && wrappingNonPowerOfTwo.DiagnosticCode == 0x2BA
            && wrappingNonPowerOfTwo.DiagnosticCallRawFileOffset == 0x12CE1A1);

        UiShaderSourcePoolReplenishmentAlignmentProjection zeroDivisor =
            UiShaderSourcePoolReplenishmentAlignmentProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentAlignmentCapture(
                    AllocatedBlockValue: 0xFFFFFFFC,
                    PoolAlignmentDivisor: 0));
        check("A zero divisor stops at the first DIV fault boundary without fabricating a remainder",
            zeroDivisor.InitialCandidate == 0
            && zeroDivisor.FirstRemainder is null
            && zeroDivisor.AlignmentAdjustment is null
            && zeroDivisor.Candidate is null
            && zeroDivisor.SecondRemainder is null
            && zeroDivisor.Outcome
                == UiShaderSourcePoolReplenishmentAlignmentOutcome.DivideFaultAtFirstDivision
            && zeroDivisor.DiagnosticCode is null
            && zeroDivisor.DiagnosticCallRawFileOffset is null);

        UiShaderSourcePoolReplenishmentAlignmentProjection zeroCapturedBlock =
            UiShaderSourcePoolReplenishmentAlignmentProjectionRules.Project(
                new UiShaderSourcePoolReplenishmentAlignmentCapture(
                    AllocatedBlockValue: 0,
                    PoolAlignmentDivisor: 4));
        check("A captured zero block remains arithmetic-only and makes no allocation-safety claim",
            zeroCapturedBlock.InitialCandidate == 4
            && zeroCapturedBlock.Candidate == 4
            && zeroCapturedBlock.SecondRemainder == 0
            && zeroCapturedBlock.Outcome
                == UiShaderSourcePoolReplenishmentAlignmentOutcome.NoDiagnosticBranchToBoundary);
    }
}
