namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolReplenishmentIterationCapture(
    uint Index,
    uint CurrentBase,
    uint Field14Value,
    uint Field1CValue,
    uint PoolDivisor,
    uint? LaterBaseCapture = null,
    uint? FreshLoopBound20 = null,
    bool DiagnosticCallReturnedNormally = true,
    bool PushCallReturnedNormally = true);

public enum UiShaderSourcePoolReplenishmentIterationOutcome
{
    DivideByZeroFault,
    DiagnosticDidNotReturnNormally,
    PushDidNotReturnNormally,
    IterationCompleted
}

public readonly record struct UiShaderSourcePoolReplenishmentIterationProjection(
    UiShaderSourcePoolReplenishmentIterationCapture Capture,
    uint Field14PlusField1C,
    uint ScaledTerm,
    uint EntryValue,
    uint? Remainder,
    bool ReachedDiagnostic,
    bool ReachedPush,
    uint? PushEntryValue,
    bool ReachedLoopDecision,
    uint? ReloadedCurrentBase,
    uint? NextIndex,
    uint? FreshLoopBound20,
    bool? TakesUnsignedRepeatBranch,
    UiShaderSourcePoolReplenishmentIterationOutcome Outcome);

public readonly record struct UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? FieldOffset = null,
    uint? TargetRawFileOffset = null,
    uint? AbsoluteCellVirtualAddress = null);

/// <summary>
/// Projects one already-entered replenishment iteration. It records independent,
/// non-atomic captures and normal-return boundaries without modeling helper effects
/// or another loop iteration.
/// </summary>
public static class UiShaderSourcePoolReplenishmentIterationProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint InitialIndexInitializeRawFileOffset = 0x12CE1AD;
    public const uint InitialBoundCompareRawFileOffset = 0x12CE1AF;
    public const uint InitialGateBranchRawFileOffset = 0x12CE1B2;
    public const uint EntryRawFileOffset = 0x12CE1B4;
    public const uint EntryVirtualAddress = 0x16CEDB4;
    public const uint IterationEndRawFileOffsetExclusive = 0x12CE1F5;

    public const uint Field14LoadRawFileOffset = 0x12CE1B4;
    public const uint DividendHighClearRawFileOffset = 0x12CE1B7;
    public const uint Field1CAddRawFileOffset = 0x12CE1B9;
    public const uint MultiplyIndexRawFileOffset = 0x12CE1BC;
    public const uint AddCurrentBaseRawFileOffset = 0x12CE1BF;
    public const uint DividendMoveRawFileOffset = 0x12CE1C1;
    public const uint DivideByPoolDivisorRawFileOffset = 0x12CE1C3;
    public const uint RemainderTestRawFileOffset = 0x12CE1C6;
    public const uint RemainderBranchRawFileOffset = 0x12CE1C8;
    public const uint DiagnosticReportIdPushRawFileOffset = 0x12CE1CC;
    public const uint DiagnosticCallRawFileOffset = 0x12CE1DB;
    public const uint DiagnosticCallCellVirtualAddress = 0x01C15EA0;
    public const uint DiagnosticCleanupRawFileOffset = 0x12CE1E1;
    public const uint PushEntryRawFileOffset = 0x12CE1E4;
    public const uint PushReceiverMoveRawFileOffset = 0x12CE1E5;
    public const uint PoolPushCallRawFileOffset = 0x12CE1E7;
    public const uint PoolPushFunctionRawFileOffset = 0x12CDA60;
    public const uint CurrentBaseReloadRawFileOffset = 0x12CE1EC;
    public const uint IndexIncrementRawFileOffset = 0x12CE1EF;
    public const uint FreshBoundCompareRawFileOffset = 0x12CE1F0;
    public const uint RepeatBranchRawFileOffset = 0x12CE1F3;
    public const uint RepeatBranchTargetRawFileOffset = 0x12CE1B4;

    public const int Field14Offset = 0x14;
    public const int Field1COffset = 0x1C;
    public const int PoolDivisorOffset = 0x18;
    public const int LoopBoundOffset = 0x20;
    public const uint DiagnosticReportId = 0x2C0;

    public const string CapturedEntryBytes =
        "8B 77 14 33 D2 03 77 1C 0F AF F3 03 F1 8B C6 F7 77 18 " +
        "85 D2 74 1A 6A 00 68 C0 02 00 00 68 DB 43 B7 01 " +
        "68 B7 43 B7 01 FF 15 A0 5E C1 01 83 C4 10 " +
        "56 8B CF E8 74 F8 FF FF 8B 4D D0 43 3B 5F 20 72 BF";

    public const string CapturedInitialGateBytes =
        "33 DB 39 5F 20 76 41";

    public static IReadOnlyList<UiShaderSourcePoolReplenishmentIterationInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Initialize the loop index to zero",
                InitialIndexInitializeRawFileOffset,
                "33 DB"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Compare the initial unsigned bound against the zero index",
                InitialBoundCompareRawFileOffset,
                "39 5F 20",
                LoopBoundOffset),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Skip the loop when the initial bound is unsigned-below-or-equal to zero",
                InitialGateBranchRawFileOffset,
                "76 41",
                TargetRawFileOffset: IterationEndRawFileOffsetExclusive),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Read this + 0x14 independently for the current entry calculation",
                Field14LoadRawFileOffset,
                "8B 77 14",
                Field14Offset),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Clear EDX before the 32-bit unsigned division",
                DividendHighClearRawFileOffset,
                "33 D2"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Add a separate read of this + 0x1C",
                Field1CAddRawFileOffset,
                "03 77 1C",
                Field1COffset),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Multiply the wrapped field sum by the captured index, retaining the low DWORD",
                MultiplyIndexRawFileOffset,
                "0F AF F3"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Add the captured current base with 32-bit wraparound",
                AddCurrentBaseRawFileOffset,
                "03 F1"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Move the computed entry into the unsigned dividend",
                DividendMoveRawFileOffset,
                "8B C6"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Divide by the later read of this + 0x18; a zero divisor faults here",
                DivideByPoolDivisorRawFileOffset,
                "F7 77 18",
                PoolDivisorOffset),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Test the unsigned division remainder",
                RemainderTestRawFileOffset,
                "85 D2"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Skip diagnostics for a zero remainder",
                RemainderBranchRawFileOffset,
                "74 1A",
                TargetRawFileOffset: PushEntryRawFileOffset),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Push the diagnostic callback's zero-valued final argument",
                0x12CE1CA,
                "6A 00"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Pass diagnostic report id 0x2C0",
                DiagnosticReportIdPushRawFileOffset,
                "68 C0 02 00 00"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Push the diagnostic callback's second string pointer",
                0x12CE1D1,
                "68 DB 43 B7 01"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Push the diagnostic callback's first string pointer",
                0x12CE1D6,
                "68 B7 43 B7 01"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Call the diagnostic callback through its absolute cell",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                AbsoluteCellVirtualAddress: DiagnosticCallCellVirtualAddress),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Clean up diagnostic arguments after a normal callback return",
                DiagnosticCleanupRawFileOffset,
                "83 C4 10"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Push the computed entry as the helper argument",
                PushEntryRawFileOffset,
                "56"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Load this as ECX for the pool push helper",
                PushReceiverMoveRawFileOffset,
                "8B CF"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Call the pool push helper",
                PoolPushCallRawFileOffset,
                "E8 74 F8 FF FF",
                TargetRawFileOffset: PoolPushFunctionRawFileOffset),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Reload the captured current base after a normal helper return",
                CurrentBaseReloadRawFileOffset,
                "8B 4D D0"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Increment the captured index modulo 2^32",
                IndexIncrementRawFileOffset,
                "43"),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Read a fresh this + 0x20 bound for the loop decision",
                FreshBoundCompareRawFileOffset,
                "3B 5F 20",
                LoopBoundOffset),
            new UiShaderSourcePoolReplenishmentIterationInstructionEvidence(
                "Repeat only when the incremented index is unsigned-below the fresh bound",
                RepeatBranchRawFileOffset,
                "72 BF",
                TargetRawFileOffset: RepeatBranchTargetRawFileOffset)
        });

    public static bool ShouldEnterIteration(uint initialLoopBound20) =>
        initialLoopBound20 != 0;

    public static UiShaderSourcePoolReplenishmentIterationProjection Project(
        UiShaderSourcePoolReplenishmentIterationCapture capture)
    {
        uint fieldSum = unchecked(capture.Field14Value + capture.Field1CValue);
        uint scaledTerm = unchecked(capture.Index * fieldSum);
        uint entryValue = unchecked(capture.CurrentBase + scaledTerm);

        if (capture.PoolDivisor == 0)
        {
            ValidateLoopTailCaptures(capture, reachesLoopDecision: false);
            return new UiShaderSourcePoolReplenishmentIterationProjection(
                capture,
                fieldSum,
                scaledTerm,
                entryValue,
                null,
                false,
                false,
                null,
                false,
                null,
                null,
                null,
                null,
                UiShaderSourcePoolReplenishmentIterationOutcome.DivideByZeroFault);
        }

        uint remainder = entryValue % capture.PoolDivisor;
        bool reachedDiagnostic = remainder != 0;
        if (reachedDiagnostic && !capture.DiagnosticCallReturnedNormally)
        {
            ValidateLoopTailCaptures(capture, reachesLoopDecision: false);
            return new UiShaderSourcePoolReplenishmentIterationProjection(
                capture,
                fieldSum,
                scaledTerm,
                entryValue,
                remainder,
                true,
                false,
                null,
                false,
                null,
                null,
                null,
                null,
                UiShaderSourcePoolReplenishmentIterationOutcome.DiagnosticDidNotReturnNormally);
        }

        if (!capture.PushCallReturnedNormally)
        {
            ValidateLoopTailCaptures(capture, reachesLoopDecision: false);
            return new UiShaderSourcePoolReplenishmentIterationProjection(
                capture,
                fieldSum,
                scaledTerm,
                entryValue,
                remainder,
                reachedDiagnostic,
                true,
                entryValue,
                false,
                null,
                null,
                null,
                null,
                UiShaderSourcePoolReplenishmentIterationOutcome.PushDidNotReturnNormally);
        }

        ValidateLoopTailCaptures(capture, reachesLoopDecision: true);
        uint nextIndex = unchecked(capture.Index + 1);
        return new UiShaderSourcePoolReplenishmentIterationProjection(
            capture,
            fieldSum,
            scaledTerm,
            entryValue,
            remainder,
            reachedDiagnostic,
            true,
            entryValue,
            true,
            capture.LaterBaseCapture!.Value,
            nextIndex,
            capture.FreshLoopBound20,
            nextIndex < capture.FreshLoopBound20!.Value,
            UiShaderSourcePoolReplenishmentIterationOutcome.IterationCompleted);
    }

    private static void ValidateLoopTailCaptures(
        UiShaderSourcePoolReplenishmentIterationCapture capture,
        bool reachesLoopDecision)
    {
        bool hasLaterBase = capture.LaterBaseCapture.HasValue;
        bool hasFreshBound = capture.FreshLoopBound20.HasValue;
        if (hasLaterBase != reachesLoopDecision || hasFreshBound != reachesLoopDecision)
        {
            throw new ArgumentException(
                reachesLoopDecision
                    ? "The reached loop tail requires both later-base and fresh-bound captures."
                    : "Loop-tail captures are invalid when the loop tail is not reached.",
                nameof(capture));
        }
    }
}
