namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolReplenishmentAlignmentCapture(
    uint AllocatedBlockValue,
    uint PoolAlignmentDivisor);

public enum UiShaderSourcePoolReplenishmentAlignmentOutcome
{
    DivideFaultAtFirstDivision,
    NoDiagnosticBranchToBoundary,
    DiagnosticCallReached
}

public readonly record struct UiShaderSourcePoolReplenishmentAlignmentProjection(
    UiShaderSourcePoolReplenishmentAlignmentCapture Capture,
    uint InitialCandidate,
    uint? FirstRemainder,
    uint? AlignmentAdjustment,
    uint? Candidate,
    uint? SecondRemainder,
    UiShaderSourcePoolReplenishmentAlignmentOutcome Outcome,
    uint? DiagnosticCode,
    uint? DiagnosticCallRawFileOffset);

public readonly record struct UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    int? FieldOffset = null,
    uint? Immediate = null);

/// <summary>
/// Projects the captured post-push 32-bit alignment arithmetic and second
/// remainder diagnostic branch. A zero divisor faults at the first DIV.
/// AllocatedBlockValue is arithmetic input, not evidence of valid memory or a
/// successful, safe allocation. Projection stops before the XOR/loop at 0x1AD.
/// </summary>
public static class UiShaderSourcePoolReplenishmentAlignmentProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint FirstPoolPushRawFileOffset = 0x12CE161;
    public const uint FirstPoolPushCallRawFileOffset = 0x12CE165;
    public const uint EntryRawFileOffset = 0x12CE16A;
    public const uint EntryVirtualAddress = 0x16CED6A;
    public const uint EntryEndRawFileOffsetExclusive = 0x12CE1AD;
    public const uint EntryEndVirtualAddressExclusive = 0x16CEDAD;

    public const uint PoolAlignmentDivisorLoadRawFileOffset = 0x12CE16A;
    public const uint CandidatePlusFourRawFileOffset = 0x12CE16D;
    public const uint FirstDivisionRawFileOffset = 0x12CE177;
    public const uint FirstRemainderBranchRawFileOffset = 0x12CE17B;
    public const uint AlignmentAdjustmentRawFileOffset = 0x12CE17D;
    public const uint SecondDivisionRawFileOffset = 0x12CE18A;
    public const uint SecondRemainderBranchRawFileOffset = 0x12CE18E;
    public const uint DiagnosticCodePushRawFileOffset = 0x12CE192;
    public const uint DiagnosticCallRawFileOffset = 0x12CE1A1;
    public const uint CandidateReloadRawFileOffset = 0x12CE1A7;
    public const uint StopBeforeRawFileOffset = 0x12CE1AD;

    public const int PoolAlignmentDivisorFieldOffset = 0x18;
    public const uint DiagnosticCode = 0x2BA;

    public const string CapturedAlignmentBytes =
        "8B 77 18 8D 4B 04 33 D2 89 4D D0 8B C1 F7 F6 85 D2 74 09 " +
        "8B C6 2B C2 03 C8 89 4D D0 33 D2 8B C1 F7 F6 85 D2 74 1D " +
        "6A 00 68 BA 02 00 00 68 CF 43 B7 01 68 E7 43 B7 01 FF 15 " +
        "A0 5E C1 01 8B 4D D0 83 C4 10";

    private static readonly string[] CapturedAlignmentByteValues =
        CapturedAlignmentBytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private static string CapturedBytesAt(uint rawFileOffset, int length) =>
        string.Join(
            " ",
            CapturedAlignmentByteValues,
            (int)(rawFileOffset - EntryRawFileOffset),
            length);

    public static IReadOnlyList<UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Push the allocated block for the first pool push",
                FirstPoolPushRawFileOffset,
                "53"),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Call the pool push helper; the captured alignment range begins after it returns",
                FirstPoolPushCallRawFileOffset,
                "E8 F6 F8 FF FF"),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Load the pool alignment divisor once from this+0x18",
                PoolAlignmentDivisorLoadRawFileOffset,
                CapturedBytesAt(PoolAlignmentDivisorLoadRawFileOffset, 3),
                FieldOffset: PoolAlignmentDivisorFieldOffset),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Set the initial candidate to the allocated block value plus four",
                CandidatePlusFourRawFileOffset,
                CapturedBytesAt(CandidatePlusFourRawFileOffset, 3),
                Immediate: 4),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Clear EDX before the first unsigned divide",
                0x12CE170,
                CapturedBytesAt(0x12CE170, 2)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Save the initial candidate",
                0x12CE172,
                CapturedBytesAt(0x12CE172, 3)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Move the initial candidate to EAX",
                0x12CE175,
                CapturedBytesAt(0x12CE175, 2)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Divide EDX:EAX by the loaded pool alignment divisor",
                FirstDivisionRawFileOffset,
                CapturedBytesAt(FirstDivisionRawFileOffset, 2)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Test the first remainder",
                0x12CE179,
                CapturedBytesAt(0x12CE179, 2)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Keep an already-divisible candidate; otherwise continue to the adjustment",
                FirstRemainderBranchRawFileOffset,
                CapturedBytesAt(FirstRemainderBranchRawFileOffset, 2),
                0x12CE186),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Load the divisor for the correction",
                AlignmentAdjustmentRawFileOffset,
                CapturedBytesAt(AlignmentAdjustmentRawFileOffset, 2)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Subtract the first remainder to form divisor-minus-remainder",
                0x12CE17F,
                CapturedBytesAt(0x12CE17F, 2)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Add the correction to the candidate with 32-bit wrap",
                0x12CE181,
                CapturedBytesAt(0x12CE181, 2)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Save the adjusted candidate",
                0x12CE183,
                CapturedBytesAt(0x12CE183, 3)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Clear EDX before the second unsigned divide",
                0x12CE186,
                CapturedBytesAt(0x12CE186, 2)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Move the candidate to EAX",
                0x12CE188,
                CapturedBytesAt(0x12CE188, 2)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Divide EDX:EAX by the same loaded pool alignment divisor",
                SecondDivisionRawFileOffset,
                CapturedBytesAt(SecondDivisionRawFileOffset, 2)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Test the second remainder",
                0x12CE18C,
                CapturedBytesAt(0x12CE18C, 2)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Branch to the captured boundary when the second remainder is zero",
                SecondRemainderBranchRawFileOffset,
                CapturedBytesAt(SecondRemainderBranchRawFileOffset, 2),
                StopBeforeRawFileOffset),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Push the zero diagnostic argument",
                0x12CE190,
                CapturedBytesAt(0x12CE190, 2),
                Immediate: 0),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Push diagnostic code 0x2BA",
                DiagnosticCodePushRawFileOffset,
                CapturedBytesAt(DiagnosticCodePushRawFileOffset, 5),
                Immediate: DiagnosticCode),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Push the first diagnostic address argument",
                0x12CE197,
                CapturedBytesAt(0x12CE197, 5)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Push the second diagnostic address argument",
                0x12CE19C,
                CapturedBytesAt(0x12CE19C, 5)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Call the diagnostic indirectly",
                DiagnosticCallRawFileOffset,
                CapturedBytesAt(DiagnosticCallRawFileOffset, 6)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Reload the candidate after the diagnostic call",
                CandidateReloadRawFileOffset,
                CapturedBytesAt(CandidateReloadRawFileOffset, 3)),
            new UiShaderSourcePoolReplenishmentAlignmentInstructionEvidence(
                "Clean up the diagnostic arguments; the projection ends before the next instruction",
                0x12CE1AA,
                CapturedBytesAt(0x12CE1AA, 3))
        });

    public static UiShaderSourcePoolReplenishmentAlignmentProjection Project(
        UiShaderSourcePoolReplenishmentAlignmentCapture capture)
    {
        uint initialCandidate = unchecked(capture.AllocatedBlockValue + 4);
        if (capture.PoolAlignmentDivisor == 0)
        {
            return new UiShaderSourcePoolReplenishmentAlignmentProjection(
                capture,
                initialCandidate,
                FirstRemainder: null,
                AlignmentAdjustment: null,
                Candidate: null,
                SecondRemainder: null,
                Outcome:
                    UiShaderSourcePoolReplenishmentAlignmentOutcome.DivideFaultAtFirstDivision,
                DiagnosticCode: null,
                DiagnosticCallRawFileOffset: null);
        }

        uint firstRemainder = initialCandidate % capture.PoolAlignmentDivisor;
        uint adjustment = firstRemainder == 0
            ? 0
            : unchecked(capture.PoolAlignmentDivisor - firstRemainder);
        uint candidate = unchecked(initialCandidate + adjustment);
        uint secondRemainder = candidate % capture.PoolAlignmentDivisor;
        bool diagnosticReached = secondRemainder != 0;

        return new UiShaderSourcePoolReplenishmentAlignmentProjection(
            capture,
            initialCandidate,
            firstRemainder,
            adjustment,
            candidate,
            secondRemainder,
            diagnosticReached
                ? UiShaderSourcePoolReplenishmentAlignmentOutcome.DiagnosticCallReached
                : UiShaderSourcePoolReplenishmentAlignmentOutcome.NoDiagnosticBranchToBoundary,
            diagnosticReached ? DiagnosticCode : null,
            diagnosticReached ? DiagnosticCallRawFileOffset : null);
    }
}
