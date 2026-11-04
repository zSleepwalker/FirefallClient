namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolResetAlignmentCapture(
    uint IncomingNodeValue,
    uint PoolAlignmentDivisor,
    bool DiagnosticCallReturned);

public enum UiShaderSourcePoolResetAlignmentOutcome
{
    DivideFaultAtFirstDivision,
    ReturnedToBoundaryWithoutDiagnostic,
    DiagnosticCallDidNotReturn,
    DiagnosticReturnedToBoundary
}

public enum UiShaderSourcePoolResetAlignmentActionKind
{
    LoadDivisor,
    FormCandidate,
    ClearEdxBeforeFirstDivision,
    SaveCandidate,
    LoadCandidateForFirstDivision,
    FirstUnsignedDivision,
    TestFirstRemainder,
    BranchOnFirstRemainder,
    CopyDivisorForCorrection,
    SubtractFirstRemainder,
    AddCorrectionToCandidate,
    SaveAdjustedCandidate,
    ClearEdxBeforeSecondDivision,
    LoadCandidateForSecondDivision,
    SecondUnsignedDivision,
    TestSecondRemainder,
    BranchOnSecondRemainder,
    PushDiagnosticZeroArgument,
    PushDiagnosticCode,
    PushDiagnosticAddressArgumentOne,
    PushDiagnosticAddressArgumentTwo,
    CallDiagnosticIndirectly,
    CleanUpDiagnosticArguments
}

public readonly record struct UiShaderSourcePoolResetAlignmentAction(
    UiShaderSourcePoolResetAlignmentActionKind Kind,
    uint RawFileOffset,
    bool? BranchTaken = null,
    uint? BranchTargetRawFileOffset = null);

public readonly record struct UiShaderSourcePoolResetAlignmentProjection(
    UiShaderSourcePoolResetAlignmentCapture Capture,
    uint InitialCandidate,
    uint? FirstRemainder,
    uint? AlignmentCorrection,
    uint? CandidateForSecondDivision,
    uint? SecondRemainder,
    bool DiagnosticCallReached,
    bool DiagnosticReturnedNormally,
    bool DiagnosticCleanupReached,
    bool ContinuationBoundaryReached,
    uint StopRawFileOffset,
    UiShaderSourcePoolResetAlignmentOutcome Outcome,
    IReadOnlyList<UiShaderSourcePoolResetAlignmentAction> Actions);

public readonly record struct UiShaderSourcePoolResetAlignmentInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    int? FieldOffset = null,
    uint? Immediate = null,
    uint? PointerCellVirtualAddress = null);

/// <summary>
/// Projects the captured 32-bit alignment arithmetic and diagnostic path
/// inside the reset routine. IncomingNodeValue is only a captured EAX value;
/// it conveys no ownership claim. This projection performs no native reads,
/// writes, calls, or execution.
/// </summary>
public static class UiShaderSourcePoolResetAlignmentProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint ResetFunctionRawFileOffset = 0x12CDC50;
    public const uint ResetFunctionVirtualAddress =
        ResetFunctionRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EntryRawFileOffset = 0x12CDD68;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EndRawFileOffsetExclusive = 0x12CDDA8;
    public const uint EndVirtualAddressExclusive =
        EndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint DivisorLoadRawFileOffset = 0x12CDD68;
    public const uint CandidatePlusFourRawFileOffset = 0x12CDD6B;
    public const uint FirstDivisionRawFileOffset = 0x12CDD75;
    public const uint FirstRemainderBranchRawFileOffset = 0x12CDD79;
    public const uint FirstRemainderBranchTargetRawFileOffset = 0x12CDD84;
    public const uint CandidateSaveRawFileOffset = 0x12CDD70;
    public const uint AlignmentCorrectionRawFileOffset = 0x12CDD7B;
    public const uint AdjustedCandidateSaveRawFileOffset = 0x12CDD81;
    public const uint SecondDivisionRawFileOffset = 0x12CDD88;
    public const uint SecondRemainderBranchRawFileOffset = 0x12CDD8C;
    public const uint DiagnosticCodePushRawFileOffset = 0x12CDD90;
    public const uint DiagnosticCallRawFileOffset = 0x12CDD9F;
    public const uint DiagnosticCleanupRawFileOffset = 0x12CDDA5;
    public const uint DiagnosticPointerCellVirtualAddress = 0x01C15EA0;

    public const int PoolAlignmentDivisorFieldOffset = 0x18;
    public const int SavedCandidateStackOffset = -0x1C;
    public const uint DiagnosticCode = 0x227;
    public const uint DiagnosticCallReturnBoundaryRawFileOffset = EndRawFileOffsetExclusive;
    public const uint DiagnosticCallNonReturnStopRawFileOffset = DiagnosticCallRawFileOffset;

    public const string CapturedBytes =
        "8B 4B 18 8D 78 04 33 D2 89 7D E4 8B C7 F7 F1 85 D2 74 09 " +
        "8B C1 2B C2 03 F8 89 7D E4 33 D2 8B C7 F7 F1 85 D2 74 1A " +
        "6A 00 68 27 02 00 00 68 D7 41 B7 01 68 87 43 B7 01 FF 15 " +
        "A0 5E C1 01 83 C4 10";

    public static IReadOnlyList<UiShaderSourcePoolResetAlignmentInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Load the pool alignment divisor once from [EBX+0x18]",
                DivisorLoadRawFileOffset,
                "8B 4B 18",
                FieldOffset: PoolAlignmentDivisorFieldOffset),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Form the candidate from the captured incoming EAX value plus four",
                CandidatePlusFourRawFileOffset,
                "8D 78 04",
                Immediate: 4),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Clear EDX before the first unsigned divide",
                0x12CDD6E,
                "33 D2"),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Save the candidate at EBP-0x1C",
                CandidateSaveRawFileOffset,
                "89 7D E4",
                FieldOffset: SavedCandidateStackOffset),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Move the candidate to EAX",
                0x12CDD73,
                "8B C7"),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Divide EDX:EAX by the captured divisor",
                FirstDivisionRawFileOffset,
                "F7 F1"),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Test the first remainder",
                0x12CDD77,
                "85 D2"),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Skip correction when the first remainder is zero",
                FirstRemainderBranchRawFileOffset,
                "74 09",
                BranchTargetRawFileOffset: FirstRemainderBranchTargetRawFileOffset),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Copy the single captured divisor from ECX to EAX for divisor-minus-remainder",
                AlignmentCorrectionRawFileOffset,
                "8B C1"),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Subtract the first remainder from the divisor",
                0x12CDD7D,
                "2B C2"),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Add the correction to the candidate with 32-bit wrap",
                0x12CDD7F,
                "03 F8"),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Save the adjusted candidate at EBP-0x1C",
                AdjustedCandidateSaveRawFileOffset,
                "89 7D E4",
                FieldOffset: SavedCandidateStackOffset),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Clear EDX before the second unsigned divide",
                0x12CDD84,
                "33 D2"),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Move the candidate to EAX",
                0x12CDD86,
                "8B C7"),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Divide EDX:EAX by the same captured divisor",
                SecondDivisionRawFileOffset,
                "F7 F1"),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Test the second remainder",
                0x12CDD8A,
                "85 D2"),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Skip diagnostic arguments and call when the second remainder is zero",
                SecondRemainderBranchRawFileOffset,
                "74 1A",
                BranchTargetRawFileOffset: EndRawFileOffsetExclusive),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Push the zero diagnostic argument",
                0x12CDD8E,
                "6A 00",
                Immediate: 0),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Push diagnostic code 0x227",
                DiagnosticCodePushRawFileOffset,
                "68 27 02 00 00",
                Immediate: DiagnosticCode),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Push the first diagnostic address argument",
                0x12CDD95,
                "68 D7 41 B7 01",
                Immediate: 0x01B741D7),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Push the second diagnostic address argument",
                0x12CDD9A,
                "68 87 43 B7 01",
                Immediate: 0x01B74387),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Call the diagnostic through the pointer cell at VA 0x1C15EA0",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                PointerCellVirtualAddress: DiagnosticPointerCellVirtualAddress),
            new UiShaderSourcePoolResetAlignmentInstructionEvidence(
                "Clean up four diagnostic arguments after the call returns",
                DiagnosticCleanupRawFileOffset,
                "83 C4 10")
        });

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static UiShaderSourcePoolResetAlignmentProjection Project(
        UiShaderSourcePoolResetAlignmentCapture capture)
    {
        uint initialCandidate = unchecked(capture.IncomingNodeValue + 4);
        if (capture.PoolAlignmentDivisor == 0 && capture.DiagnosticCallReturned)
        {
            throw new ArgumentException(
                "The diagnostic return capture is invalid when the first division faults.",
                nameof(capture));
        }

        List<UiShaderSourcePoolResetAlignmentAction> actions = new();
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.LoadDivisor,
            DivisorLoadRawFileOffset));
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.FormCandidate,
            CandidatePlusFourRawFileOffset));
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.ClearEdxBeforeFirstDivision,
            0x12CDD6E));
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.SaveCandidate,
            CandidateSaveRawFileOffset));
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.LoadCandidateForFirstDivision,
            0x12CDD73));
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.FirstUnsignedDivision,
            FirstDivisionRawFileOffset));

        if (capture.PoolAlignmentDivisor == 0)
        {
            return Result(
                capture,
                initialCandidate,
                firstRemainder: null,
                alignmentCorrection: null,
                candidateForSecondDivision: null,
                secondRemainder: null,
                diagnosticCallReached: false,
                diagnosticReturnedNormally: false,
                diagnosticCleanupReached: false,
                continuationBoundaryReached: false,
                stopRawFileOffset: FirstDivisionRawFileOffset,
                UiShaderSourcePoolResetAlignmentOutcome.DivideFaultAtFirstDivision,
                actions);
        }

        uint firstRemainder = initialCandidate % capture.PoolAlignmentDivisor;
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.TestFirstRemainder,
            0x12CDD77));
        bool correctionNeeded = firstRemainder != 0;
        actions.Add(BranchAction(
            UiShaderSourcePoolResetAlignmentActionKind.BranchOnFirstRemainder,
            FirstRemainderBranchRawFileOffset,
            branchTaken: !correctionNeeded,
            branchTarget: correctionNeeded ? null : FirstRemainderBranchTargetRawFileOffset));

        uint? alignmentCorrection = null;
        uint candidate = initialCandidate;
        if (correctionNeeded)
        {
            actions.Add(Action(
                UiShaderSourcePoolResetAlignmentActionKind.CopyDivisorForCorrection,
                AlignmentCorrectionRawFileOffset));
            actions.Add(Action(
                UiShaderSourcePoolResetAlignmentActionKind.SubtractFirstRemainder,
                0x12CDD7D));
            actions.Add(Action(
                UiShaderSourcePoolResetAlignmentActionKind.AddCorrectionToCandidate,
                0x12CDD7F));
            alignmentCorrection = unchecked(capture.PoolAlignmentDivisor - firstRemainder);
            candidate = unchecked(candidate + alignmentCorrection.Value);
            actions.Add(Action(
                UiShaderSourcePoolResetAlignmentActionKind.SaveAdjustedCandidate,
                AdjustedCandidateSaveRawFileOffset));
        }

        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.ClearEdxBeforeSecondDivision,
            0x12CDD84));
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.LoadCandidateForSecondDivision,
            0x12CDD86));
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.SecondUnsignedDivision,
            SecondDivisionRawFileOffset));

        uint secondRemainder = candidate % capture.PoolAlignmentDivisor;
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.TestSecondRemainder,
            0x12CDD8A));
        bool diagnosticReached = secondRemainder != 0;
        actions.Add(BranchAction(
            UiShaderSourcePoolResetAlignmentActionKind.BranchOnSecondRemainder,
            SecondRemainderBranchRawFileOffset,
            branchTaken: !diagnosticReached,
            branchTarget: diagnosticReached ? null : EndRawFileOffsetExclusive));

        if (!diagnosticReached)
        {
            if (capture.DiagnosticCallReturned)
            {
                throw new ArgumentException(
                    "The diagnostic return capture is invalid when the remainder-zero branch skips the call.",
                    nameof(capture));
            }

            return Result(
                capture,
                initialCandidate,
                firstRemainder,
                alignmentCorrection,
                candidate,
                secondRemainder,
                diagnosticCallReached: false,
                diagnosticReturnedNormally: false,
                diagnosticCleanupReached: false,
                continuationBoundaryReached: true,
                stopRawFileOffset: EndRawFileOffsetExclusive,
                UiShaderSourcePoolResetAlignmentOutcome.ReturnedToBoundaryWithoutDiagnostic,
                actions);
        }

        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.PushDiagnosticZeroArgument,
            0x12CDD8E));
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.PushDiagnosticCode,
            DiagnosticCodePushRawFileOffset));
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.PushDiagnosticAddressArgumentOne,
            0x12CDD95));
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.PushDiagnosticAddressArgumentTwo,
            0x12CDD9A));
        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.CallDiagnosticIndirectly,
            DiagnosticCallRawFileOffset));

        if (!capture.DiagnosticCallReturned)
        {
            return Result(
                capture,
                initialCandidate,
                firstRemainder,
                alignmentCorrection,
                candidate,
                secondRemainder,
                diagnosticCallReached: true,
                diagnosticReturnedNormally: false,
                diagnosticCleanupReached: false,
                continuationBoundaryReached: false,
                stopRawFileOffset: DiagnosticCallNonReturnStopRawFileOffset,
                UiShaderSourcePoolResetAlignmentOutcome.DiagnosticCallDidNotReturn,
                actions);
        }

        actions.Add(Action(
            UiShaderSourcePoolResetAlignmentActionKind.CleanUpDiagnosticArguments,
            DiagnosticCleanupRawFileOffset));
        return Result(
            capture,
            initialCandidate,
            firstRemainder,
            alignmentCorrection,
            candidate,
            secondRemainder,
            diagnosticCallReached: true,
            diagnosticReturnedNormally: true,
            diagnosticCleanupReached: true,
            continuationBoundaryReached: true,
            stopRawFileOffset: EndRawFileOffsetExclusive,
            UiShaderSourcePoolResetAlignmentOutcome.DiagnosticReturnedToBoundary,
            actions);
    }

    private static UiShaderSourcePoolResetAlignmentAction Action(
        UiShaderSourcePoolResetAlignmentActionKind kind,
        uint rawFileOffset) =>
        new(kind, rawFileOffset);

    private static UiShaderSourcePoolResetAlignmentAction BranchAction(
        UiShaderSourcePoolResetAlignmentActionKind kind,
        uint rawFileOffset,
        bool branchTaken,
        uint? branchTarget) =>
        new(kind, rawFileOffset, branchTaken, branchTarget);

    private static UiShaderSourcePoolResetAlignmentProjection Result(
        UiShaderSourcePoolResetAlignmentCapture capture,
        uint initialCandidate,
        uint? firstRemainder,
        uint? alignmentCorrection,
        uint? candidateForSecondDivision,
        uint? secondRemainder,
        bool diagnosticCallReached,
        bool diagnosticReturnedNormally,
        bool diagnosticCleanupReached,
        bool continuationBoundaryReached,
        uint stopRawFileOffset,
        UiShaderSourcePoolResetAlignmentOutcome outcome,
        List<UiShaderSourcePoolResetAlignmentAction> actions) =>
        new(
            capture,
            initialCandidate,
            firstRemainder,
            alignmentCorrection,
            candidateForSecondDivision,
            secondRemainder,
            diagnosticCallReached,
            diagnosticReturnedNormally,
            diagnosticCleanupReached,
            continuationBoundaryReached,
            stopRawFileOffset,
            outcome,
            Array.AsReadOnly(actions.ToArray()));
}
