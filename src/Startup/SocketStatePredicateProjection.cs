using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct SocketStatePredicateCapture(
    uint ReceiverPointer,
    uint IncomingEaxAtEntry,
    byte? LoadedStatusByte,
    uint? ReceiverDword4,
    bool? DiagnosticReturnedNormally,
    uint? ReceiverDword8,
    uint? ReceiverDwordC,
    uint? ReceiverDword10,
    uint? ReceiverDword14);

public enum SocketStatePredicateOutcome
{
    MissingStatusByte,
    ZeroStatusReturnsWithPreservedUpperEax,
    MissingReceiverDword4,
    ReceiverDword4ReturnsNormalizedComparison,
    DiagnosticReturnUnavailable,
    DiagnosticDidNotReturnNormally,
    MissingFirstPairCapture,
    FirstPairNonzeroReturnsOne,
    MissingLastPairCapture,
    LastPairZeroReturnsZero,
    LastPairNonzeroReturnsOne
}

public enum SocketStatePredicateActionKind
{
    CaptureStatusByte,
    ReturnZeroStatusWithPreservedUpperEax,
    TestStatusBit0,
    CaptureReceiverDword4,
    ReturnNormalizedReceiverDword4Comparison,
    TestStatusBit1,
    CallDiagnostic,
    CaptureReceiverDword8,
    CaptureReceiverDwordC,
    BranchOnFirstPairOr,
    CaptureReceiverDword10,
    CaptureReceiverDword14,
    BranchOnLastPairOr,
    ReturnZero,
    ReturnOne
}

public readonly record struct SocketStatePredicateAction(
    SocketStatePredicateActionKind Kind,
    uint RawFileOffset,
    uint? CapturedValue = null,
    bool? ConditionMet = null,
    bool? CallReturnedNormally = null,
    uint? BranchTargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

public readonly record struct SocketStatePredicatePlan(
    SocketStatePredicateCapture Capture,
    SocketStatePredicateOutcome Outcome,
    ImmutableArray<SocketStatePredicateAction> Actions,
    uint? ReturnedEax,
    byte? ReturnedAl,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct SocketStatePredicateInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects the captured byte predicate at raw BCCA60. Each receiver DWORD is
/// an independent capture; the diagnostic is opaque and must return normally
/// before its following field reads are considered reached.
/// </summary>
public static class SocketStatePredicateProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x00BCCA60;
    public const uint FunctionEndRawFileOffsetExclusive = 0x00BCCAB0;
    public const uint StatusByteCaptureRawFileOffset = 0x00BCCA63;
    public const uint ZeroStatusReturnRawFileOffsetExclusive = 0x00BCCA6B;
    public const uint StatusBit0TestRawFileOffset = 0x00BCCA6B;
    public const uint StatusBit1TestRawFileOffset = 0x00BCCA79;
    public const uint ReceiverDword4CompareRawFileOffset = 0x00BCCA71;
    public const uint ReceiverDword4CompareEndRawFileOffset = 0x00BCCA74;
    public const uint ReceiverDword4ReturnRawFileOffsetExclusive = 0x00BCCA79;
    public const uint DiagnosticBranchRawFileOffset = 0x00BCCA7B;
    public const uint DiagnosticCallRawFileOffset = 0x00BCCA8E;
    public const uint DiagnosticCallReturnRawFileOffsetExclusive = 0x00BCCA94;
    public const uint FirstPairDword8CaptureRawFileOffset = 0x00BCCA97;
    public const uint FirstPairDwordCCaptureRawFileOffset = 0x00BCCA9A;
    public const uint FirstPairBranchRawFileOffset = 0x00BCCA9D;
    public const uint LastPairDword10CaptureRawFileOffset = 0x00BCCA9F;
    public const uint LastPairDword14CaptureRawFileOffset = 0x00BCCAA2;
    public const uint LastPairBranchRawFileOffset = 0x00BCCAA5;
    public const uint ZeroReturnRawFileOffset = 0x00BCCAA7;
    public const uint OneReturnRawFileOffset = 0x00BCCAA9;

    public const uint StatusByteOffset = 0;
    public const uint ReceiverDword4Offset = 4;
    public const uint ReceiverDword8Offset = 8;
    public const uint ReceiverDwordCOffset = 0x0C;
    public const uint ReceiverDword10Offset = 0x10;
    public const uint ReceiverDword14Offset = 0x14;
    public const uint DiagnosticReportId = 0x23C;
    public const uint DiagnosticFunctionPointerCellVirtualAddress = 0x01C15EA0;
    public const uint IncomingEaxPreservedMask = 0xFFFFFF00;

    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint FunctionEndVirtualAddressExclusive =
        FunctionEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint DiagnosticCallVirtualAddress =
        DiagnosticCallRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedFunctionBytes =
        "56 8B F1 8A 06 84 C0 75 02 5E C3 A8 01 74 0A "
            + "33 C0 39 46 04 5E 0F 95 C0 C3 A8 02 75 1A "
            + "6A 00 68 3C 02 00 00 68 61 78 B5 01 68 62 78 B5 01 "
            + "FF 15 A0 5E C1 01 83 C4 10 8B 46 08 0B 46 0C "
            + "75 0A 8B 46 10 0B 46 14 75 02 5E C3 "
            + "B8 01 00 00 00 5E C3";

    public const string DiagnosticPredicate =
        "status & 1 == 0 AND status & 2 == 0";

    public static IReadOnlyList<SocketStatePredicateInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new SocketStatePredicateInstructionEvidence(
                "Capture the loaded status byte once from this+0 into AL",
                StatusByteCaptureRawFileOffset,
                "8A 06"),
            new SocketStatePredicateInstructionEvidence(
                "Return immediately when the captured status byte is zero",
                0x00BCCA65,
                "84 C0 75 02",
                StatusBit0TestRawFileOffset),
            new SocketStatePredicateInstructionEvidence(
                "Test captured status bit 0",
                StatusBit0TestRawFileOffset,
                "A8 01"),
            new SocketStatePredicateInstructionEvidence(
                "A set bit 0 compares the independently read DWORD this+4 against zero",
                ReceiverDword4CompareRawFileOffset,
                "39 46 04"),
            new SocketStatePredicateInstructionEvidence(
                "Normalize the this+4 comparison to AL 0 or 1 and return",
                0x00BCCA75,
                "0F 95 C0 C3"),
            new SocketStatePredicateInstructionEvidence(
                "When bit 0 is clear, test captured status bit 1",
                StatusBit1TestRawFileOffset,
                "A8 02"),
            new SocketStatePredicateInstructionEvidence(
                "A clear bit 1 falls through to the diagnostic call; a set bit skips it",
                DiagnosticBranchRawFileOffset,
                "75 1A",
                FirstPairDword8CaptureRawFileOffset,
                FirstPairDword8CaptureRawFileOffset
                    + CodeVirtualAddressDeltaFromRawFileOffset),
            new SocketStatePredicateInstructionEvidence(
                "Push diagnostic report ID 0x23C and the diagnostic arguments",
                0x00BCCA7D,
                "6A 00 68 3C 02 00 00 68 61 78 B5 01 68 62 78 B5 01"),
            new SocketStatePredicateInstructionEvidence(
                "Call through diagnostic function-pointer cell 0x1C15EA0",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                TargetVirtualAddress: DiagnosticFunctionPointerCellVirtualAddress),
            new SocketStatePredicateInstructionEvidence(
                "Read this+8 and OR the independent DWORD at this+0xC",
                FirstPairDword8CaptureRawFileOffset,
                "8B 46 08 0B 46 0C"),
            new SocketStatePredicateInstructionEvidence(
                "A nonzero first-pair OR skips the later pair and returns one",
                FirstPairBranchRawFileOffset,
                "75 0A",
                OneReturnRawFileOffset,
                OneReturnRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset),
            new SocketStatePredicateInstructionEvidence(
                "Only a zero first-pair OR reads this+0x10 and ORs this+0x14",
                LastPairDword10CaptureRawFileOffset,
                "8B 46 10 0B 46 14"),
            new SocketStatePredicateInstructionEvidence(
                "A nonzero last-pair OR returns one",
                LastPairBranchRawFileOffset,
                "75 02",
                OneReturnRawFileOffset,
                OneReturnRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset),
            new SocketStatePredicateInstructionEvidence(
                "Return zero when both DWORD OR pairs are zero",
                ZeroReturnRawFileOffset,
                "5E C3"),
            new SocketStatePredicateInstructionEvidence(
                "Return normalized EAX one when either DWORD OR pair is nonzero",
                OneReturnRawFileOffset,
                "B8 01 00 00 00 5E C3")
        });

    public static uint AddReceiverOffset(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    public static uint StatusByteAddress(uint receiverPointer) =>
        AddReceiverOffset(receiverPointer, StatusByteOffset);

    public static uint ReceiverDword4Address(uint receiverPointer) =>
        AddReceiverOffset(receiverPointer, ReceiverDword4Offset);

    public static uint ReceiverDword8Address(uint receiverPointer) =>
        AddReceiverOffset(receiverPointer, ReceiverDword8Offset);

    public static uint ReceiverDwordCAddress(uint receiverPointer) =>
        AddReceiverOffset(receiverPointer, ReceiverDwordCOffset);

    public static uint ReceiverDword10Address(uint receiverPointer) =>
        AddReceiverOffset(receiverPointer, ReceiverDword10Offset);

    public static uint ReceiverDword14Address(uint receiverPointer) =>
        AddReceiverOffset(receiverPointer, ReceiverDword14Offset);

    /// <summary>
    /// Consumes the single captured status byte and independently captured
    /// DWORDs. Null return captures mark an unavailable control-flow boundary;
    /// any supplied capture from a branch not established by prior captures
    /// is rejected.
    /// </summary>
    public static SocketStatePredicatePlan Project(SocketStatePredicateCapture capture)
    {
        var actions = ImmutableArray.CreateBuilder<SocketStatePredicateAction>(12);
        if (capture.LoadedStatusByte is not byte status)
        {
            Reject(!HasPostStatusCaptures(capture),
                nameof(capture),
                "Receiver fields and diagnostic-return captures are unreachable without the status-byte capture.");
            return Stop(capture, SocketStatePredicateOutcome.MissingStatusByte,
                StatusByteCaptureRawFileOffset, actions);
        }

        actions.Add(new SocketStatePredicateAction(
            SocketStatePredicateActionKind.CaptureStatusByte,
            StatusByteCaptureRawFileOffset,
            CapturedValue: status));

        if (status == 0)
        {
            Reject(!HasPostStatusCaptures(capture),
                nameof(capture),
                "No receiver DWORD or diagnostic capture is reached after the zero-status early return.");
            uint returnedEax = capture.IncomingEaxAtEntry & IncomingEaxPreservedMask;
            actions.Add(new SocketStatePredicateAction(
                SocketStatePredicateActionKind.ReturnZeroStatusWithPreservedUpperEax,
                0x00BCCA6A,
                CapturedValue: returnedEax));
            return Stop(capture,
                SocketStatePredicateOutcome.ZeroStatusReturnsWithPreservedUpperEax,
                ZeroStatusReturnRawFileOffsetExclusive,
                actions,
                returnedEax);
        }

        actions.Add(new SocketStatePredicateAction(
            SocketStatePredicateActionKind.TestStatusBit0,
            StatusBit0TestRawFileOffset,
            CapturedValue: (uint)(status & 1)));

        if ((status & 1) != 0)
        {
            Reject(capture.DiagnosticReturnedNormally is null
                && capture.ReceiverDword8 is null
                && capture.ReceiverDwordC is null
                && capture.ReceiverDword10 is null
                && capture.ReceiverDword14 is null,
                nameof(capture),
                "The diagnostic and later DWORD captures are unreachable on the set-bit-0 return path.");
            actions.Add(new SocketStatePredicateAction(
                SocketStatePredicateActionKind.CaptureReceiverDword4,
                ReceiverDword4CompareRawFileOffset,
                capture.ReceiverDword4));
            if (capture.ReceiverDword4 is not uint dword4)
            {
                return Stop(capture,
                    SocketStatePredicateOutcome.MissingReceiverDword4,
                    ReceiverDword4CompareEndRawFileOffset,
                    actions);
            }

            uint returnedEax = dword4 == 0 ? 0u : 1u;
            actions.Add(new SocketStatePredicateAction(
                SocketStatePredicateActionKind.ReturnNormalizedReceiverDword4Comparison,
                0x00BCCA75,
                CapturedValue: dword4,
                ConditionMet: dword4 != 0));
            return Stop(capture,
                SocketStatePredicateOutcome.ReceiverDword4ReturnsNormalizedComparison,
                ReceiverDword4ReturnRawFileOffsetExclusive,
                actions,
                returnedEax);
        }

        actions.Add(new SocketStatePredicateAction(
            SocketStatePredicateActionKind.TestStatusBit1,
            StatusBit1TestRawFileOffset,
            CapturedValue: (uint)(status & 2)));

        if ((status & 2) == 0)
        {
            Reject(capture.ReceiverDword4 is null,
                nameof(capture),
                "The this+4 comparison is unreachable when status bit 0 is clear.");
            actions.Add(new SocketStatePredicateAction(
                SocketStatePredicateActionKind.CallDiagnostic,
                DiagnosticCallRawFileOffset,
                CapturedValue: DiagnosticReportId,
                CallReturnedNormally: capture.DiagnosticReturnedNormally,
                TargetVirtualAddress: DiagnosticFunctionPointerCellVirtualAddress));
            if (capture.DiagnosticReturnedNormally is not true)
            {
                Reject(capture.ReceiverDword8 is null
                    && capture.ReceiverDwordC is null
                    && capture.ReceiverDword10 is null
                    && capture.ReceiverDword14 is null,
                    nameof(capture),
                    "The receiver DWORD captures are unreachable unless the diagnostic call returns normally.");
                return Stop(capture,
                    capture.DiagnosticReturnedNormally is null
                        ? SocketStatePredicateOutcome.DiagnosticReturnUnavailable
                        : SocketStatePredicateOutcome.DiagnosticDidNotReturnNormally,
                    DiagnosticCallReturnRawFileOffsetExclusive,
                    actions);
            }
        }
        else
        {
            Reject(capture.DiagnosticReturnedNormally is null
                && capture.ReceiverDword4 is null,
                nameof(capture),
                "The diagnostic is skipped and this+4 is not read when status bit 1 is set.");
        }

        actions.Add(new SocketStatePredicateAction(
            SocketStatePredicateActionKind.CaptureReceiverDword8,
            FirstPairDword8CaptureRawFileOffset,
            capture.ReceiverDword8));
        if (capture.ReceiverDword8 is not uint dword8)
        {
            Reject(capture.ReceiverDwordC is null
                && capture.ReceiverDword10 is null
                && capture.ReceiverDword14 is null,
                nameof(capture),
                "The this+0xC read and later captures are unreachable without the this+8 capture.");
            return Stop(capture,
                SocketStatePredicateOutcome.MissingFirstPairCapture,
                FirstPairDword8CaptureRawFileOffset,
                actions);
        }

        actions.Add(new SocketStatePredicateAction(
            SocketStatePredicateActionKind.CaptureReceiverDwordC,
            FirstPairDwordCCaptureRawFileOffset,
            capture.ReceiverDwordC));
        if (capture.ReceiverDwordC is not uint dwordC)
        {
            Reject(capture.ReceiverDword10 is null && capture.ReceiverDword14 is null,
                nameof(capture),
                "The last pair is reached only after the this+0xC capture establishes a zero first-pair OR.");
            return Stop(capture,
                SocketStatePredicateOutcome.MissingFirstPairCapture,
                FirstPairDwordCCaptureRawFileOffset,
                actions);
        }

        uint firstPairOr = dword8 | dwordC;
        bool firstPairNonzero = firstPairOr != 0;
        actions.Add(new SocketStatePredicateAction(
            SocketStatePredicateActionKind.BranchOnFirstPairOr,
            FirstPairBranchRawFileOffset,
            CapturedValue: firstPairOr,
            ConditionMet: firstPairNonzero,
            BranchTargetRawFileOffset: firstPairNonzero ? OneReturnRawFileOffset : null,
            TargetVirtualAddress: firstPairNonzero
                ? OneReturnRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset
                : null));
        if (firstPairNonzero)
        {
            Reject(capture.ReceiverDword10 is null && capture.ReceiverDword14 is null,
                nameof(capture),
                "The last DWORD pair is skipped when this+8 OR this+0xC is nonzero.");
            return ReturnOne(capture, actions);
        }

        actions.Add(new SocketStatePredicateAction(
            SocketStatePredicateActionKind.CaptureReceiverDword10,
            LastPairDword10CaptureRawFileOffset,
            capture.ReceiverDword10));
        if (capture.ReceiverDword10 is not uint dword10)
        {
            Reject(capture.ReceiverDword14 is null,
                nameof(capture),
                "The this+0x14 read is unreachable without the this+0x10 capture.");
            return Stop(capture,
                SocketStatePredicateOutcome.MissingLastPairCapture,
                LastPairDword10CaptureRawFileOffset,
                actions);
        }

        actions.Add(new SocketStatePredicateAction(
            SocketStatePredicateActionKind.CaptureReceiverDword14,
            LastPairDword14CaptureRawFileOffset,
            capture.ReceiverDword14));
        if (capture.ReceiverDword14 is not uint dword14)
        {
            return Stop(capture,
                SocketStatePredicateOutcome.MissingLastPairCapture,
                LastPairDword14CaptureRawFileOffset,
                actions);
        }

        uint lastPairOr = dword10 | dword14;
        bool lastPairNonzero = lastPairOr != 0;
        actions.Add(new SocketStatePredicateAction(
            SocketStatePredicateActionKind.BranchOnLastPairOr,
            LastPairBranchRawFileOffset,
            CapturedValue: lastPairOr,
            ConditionMet: lastPairNonzero,
            BranchTargetRawFileOffset: lastPairNonzero ? OneReturnRawFileOffset : null,
            TargetVirtualAddress: lastPairNonzero
                ? OneReturnRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset
                : null));
        if (lastPairNonzero)
        {
            return ReturnOne(capture, actions);
        }

        actions.Add(new SocketStatePredicateAction(
            SocketStatePredicateActionKind.ReturnZero,
            ZeroReturnRawFileOffset));
        return Stop(capture,
            SocketStatePredicateOutcome.LastPairZeroReturnsZero,
            OneReturnRawFileOffset,
            actions,
            0);
    }

    private static SocketStatePredicatePlan ReturnOne(
        SocketStatePredicateCapture capture,
        ImmutableArray<SocketStatePredicateAction>.Builder actions)
    {
        actions.Add(new SocketStatePredicateAction(
            SocketStatePredicateActionKind.ReturnOne,
            OneReturnRawFileOffset,
            CapturedValue: 1));
        return Stop(capture,
            capture.ReceiverDword8.GetValueOrDefault() != 0
                || capture.ReceiverDwordC.GetValueOrDefault() != 0
                ? SocketStatePredicateOutcome.FirstPairNonzeroReturnsOne
                : SocketStatePredicateOutcome.LastPairNonzeroReturnsOne,
            FunctionEndRawFileOffsetExclusive,
            actions,
            1);
    }

    private static SocketStatePredicatePlan Stop(
        SocketStatePredicateCapture capture,
        SocketStatePredicateOutcome outcome,
        uint stopAtRawFileOffsetExclusive,
        ImmutableArray<SocketStatePredicateAction>.Builder actions,
        uint? returnedEax = null) =>
        new(
            capture,
            outcome,
            actions.ToImmutable(),
            returnedEax,
            returnedEax is uint eax ? (byte)eax : null,
            stopAtRawFileOffsetExclusive);

    private static bool HasPostStatusCaptures(SocketStatePredicateCapture capture) =>
        capture.ReceiverDword4 is not null
        || capture.DiagnosticReturnedNormally is not null
        || capture.ReceiverDword8 is not null
        || capture.ReceiverDwordC is not null
        || capture.ReceiverDword10 is not null
        || capture.ReceiverDword14 is not null;

    private static void Reject(bool condition, string parameterName, string message)
    {
        if (!condition)
        {
            throw new ArgumentException(message, parameterName);
        }
    }
}
