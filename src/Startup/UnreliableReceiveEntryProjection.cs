using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveEntryCapture(
    byte? FlagAtOffset0C,
    bool? DiagnosticCallReturnedNormally);

public enum UnreliableReceiveEntryOutcome
{
    MissingFlagCapture,
    MissingFlagCaptureWithUnreachedDiagnosticReturnCapture,
    UnreachedDiagnosticReturnCapture,
    MissingDiagnosticReturnCapture,
    DiagnosticCallDidNotReturnNormally,
    DiagnosticSkippedToBoundary,
    DiagnosticReturnedToBoundary
}

public readonly record struct UnreliableReceiveEntryPlan(
    uint EntryReceiverAddress,
    uint FlagAddress,
    UnreliableReceiveEntryCapture Capture,
    UnreliableReceiveEntryOutcome Outcome,
    bool? FlagIsZero,
    bool? DiagnosticBranchTaken,
    bool? DiagnosticCallReached,
    bool? DiagnosticCallReturnedNormally,
    bool UnreachedDiagnosticReturnCaptureProvided,
    bool? GuardBoundaryReached,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveEntryInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects only the entry diagnostic guard, stopping before raw offset 0xBE5A6B.
/// </summary>
public static class UnreliableReceiveEntryProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryStartRawFileOffset = 0x00BE5A40;
    public const uint EntryStartVirtualAddress = 0x00FE6640;
    public const uint ReceiverFlagOffset = 0x0C;
    public const uint ReceiverToFlagAddressOffset = ReceiverFlagOffset;
    public const uint ReceiverLoadRawFileOffset = 0x00BE5A49;
    public const uint FlagCompareRawFileOffset = 0x00BE5A4B;
    public const uint DiagnosticBranchRawFileOffset = 0x00BE5A4F;
    public const uint DiagnosticBranchTargetRawFileOffset = 0x00BE5A6B;
    public const uint DiagnosticPushesRawFileOffset = 0x00BE5A51;
    public const uint DiagnosticCallRawFileOffset = 0x00BE5A62;
    public const uint DiagnosticCallReturnRawFileOffset = 0x00BE5A68;
    public const uint DiagnosticReporterPointerSlotAddress = 0x01C15EA0;
    public const uint DiagnosticReportCode = 0x2AF;
    public const uint DiagnosticZeroArgument = 0;
    public const uint DiagnosticLiteralAtAddress1B58C2F = 0x01B58C2F;
    public const uint DiagnosticLiteralAtAddress1B58C3D = 0x01B58C3D;
    public const uint GuardBoundaryRawFileOffsetExclusive = 0x00BE5A6B;
    public const uint PrefixEndRawFileOffsetExclusive = GuardBoundaryRawFileOffsetExclusive;

    public const string CapturedGuardBytes =
        "55 8B EC 83 EC 08 53 56 57 8B F9 80 7F 0C 00 75 1A 6A 00 "
        + "68 AF 02 00 00 68 2F 8C B5 01 68 3D 8C B5 01 FF 15 A0 5E "
        + "C1 01 83 C4 10";

    public static ImmutableArray<uint> DiagnosticPushValuesInInstructionOrder { get; } =
        ImmutableArray.Create(
            DiagnosticZeroArgument,
            DiagnosticReportCode,
            DiagnosticLiteralAtAddress1B58C2F,
            DiagnosticLiteralAtAddress1B58C3D);

    public static ImmutableArray<UnreliableReceiveEntryInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveEntryInstructionEvidence(
                "The entry prologue saves registers, then transfers ECX to EDI",
                EntryStartRawFileOffset,
                "55 8B EC 83 EC 08 53 56 57 8B F9"),
            new UnreliableReceiveEntryInstructionEvidence(
                "Compare BYTE [EDI+0x0C] with zero; no receiver-null guard precedes this dereference",
                FlagCompareRawFileOffset,
                "80 7F 0C 00"),
            new UnreliableReceiveEntryInstructionEvidence(
                "JNZ skips diagnostic setup and targets the exclusive guard boundary",
                DiagnosticBranchRawFileOffset,
                "75 1A",
                DiagnosticBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(DiagnosticBranchTargetRawFileOffset)),
            new UnreliableReceiveEntryInstructionEvidence(
                "Push diagnostic arguments in instruction order: 0, 0x2AF, 0x1B58C2F, 0x1B58C3D",
                DiagnosticPushesRawFileOffset,
                "6A 00 68 AF 02 00 00 68 2F 8C B5 01 68 3D 8C B5 01"),
            new UnreliableReceiveEntryInstructionEvidence(
                "Call through the indirect diagnostic reporter pointer slot",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01"),
            new UnreliableReceiveEntryInstructionEvidence(
                "After a normal diagnostic return, discard the four pushed arguments",
                DiagnosticCallReturnRawFileOffset,
                "83 C4 10"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static UnreliableReceiveEntryPlan Project(
        uint entryReceiverAddress,
        byte? flagAtOffset0C,
        bool? diagnosticCallReturnedNormally)
    {
        uint flagAddress = unchecked(entryReceiverAddress + ReceiverToFlagAddressOffset);
        UnreliableReceiveEntryCapture capture =
            new(flagAtOffset0C, diagnosticCallReturnedNormally);

        if (flagAtOffset0C is not byte flag)
        {
            bool laterCaptureProvided = diagnosticCallReturnedNormally.HasValue;
            return Plan(
                entryReceiverAddress,
                flagAddress,
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveEntryOutcome.MissingFlagCaptureWithUnreachedDiagnosticReturnCapture
                    : UnreliableReceiveEntryOutcome.MissingFlagCapture,
                flagIsZero: null,
                diagnosticBranchTaken: null,
                diagnosticCallReached: null,
                diagnosticCallReturnedNormally: null,
                unreachedDiagnosticReturnCaptureProvided: laterCaptureProvided,
                guardBoundaryReached: null,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: FlagCompareRawFileOffset);
        }

        bool flagIsZero = flag == 0;
        if (!flagIsZero)
        {
            bool laterCaptureProvided = diagnosticCallReturnedNormally.HasValue;
            return Plan(
                entryReceiverAddress,
                flagAddress,
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveEntryOutcome.UnreachedDiagnosticReturnCapture
                    : UnreliableReceiveEntryOutcome.DiagnosticSkippedToBoundary,
                flagIsZero: false,
                diagnosticBranchTaken: true,
                diagnosticCallReached: false,
                diagnosticCallReturnedNormally: null,
                unreachedDiagnosticReturnCaptureProvided: laterCaptureProvided,
                guardBoundaryReached: true,
                selectedDestinationRawFileOffset: GuardBoundaryRawFileOffsetExclusive,
                stopAtRawFileOffsetExclusive: GuardBoundaryRawFileOffsetExclusive);
        }

        if (diagnosticCallReturnedNormally is not bool returnedNormally)
        {
            return Plan(
                entryReceiverAddress,
                flagAddress,
                capture,
                UnreliableReceiveEntryOutcome.MissingDiagnosticReturnCapture,
                flagIsZero: true,
                diagnosticBranchTaken: false,
                diagnosticCallReached: true,
                diagnosticCallReturnedNormally: null,
                unreachedDiagnosticReturnCaptureProvided: false,
                guardBoundaryReached: false,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: DiagnosticCallReturnRawFileOffset);
        }

        return Plan(
            entryReceiverAddress,
            flagAddress,
            capture,
            returnedNormally
                ? UnreliableReceiveEntryOutcome.DiagnosticReturnedToBoundary
                : UnreliableReceiveEntryOutcome.DiagnosticCallDidNotReturnNormally,
            flagIsZero: true,
            diagnosticBranchTaken: false,
            diagnosticCallReached: true,
            diagnosticCallReturnedNormally: returnedNormally,
            unreachedDiagnosticReturnCaptureProvided: false,
            guardBoundaryReached: returnedNormally,
            selectedDestinationRawFileOffset: returnedNormally
                ? GuardBoundaryRawFileOffsetExclusive
                : null,
            stopAtRawFileOffsetExclusive: returnedNormally
                ? GuardBoundaryRawFileOffsetExclusive
                : DiagnosticCallReturnRawFileOffset);
    }

    private static UnreliableReceiveEntryPlan Plan(
        uint entryReceiverAddress,
        uint flagAddress,
        UnreliableReceiveEntryCapture capture,
        UnreliableReceiveEntryOutcome outcome,
        bool? flagIsZero,
        bool? diagnosticBranchTaken,
        bool? diagnosticCallReached,
        bool? diagnosticCallReturnedNormally,
        bool unreachedDiagnosticReturnCaptureProvided,
        bool? guardBoundaryReached,
        uint? selectedDestinationRawFileOffset,
        uint stopAtRawFileOffsetExclusive) =>
        new(
            entryReceiverAddress,
            flagAddress,
            capture,
            outcome,
            flagIsZero,
            diagnosticBranchTaken,
            diagnosticCallReached,
            diagnosticCallReturnedNormally,
            unreachedDiagnosticReturnCaptureProvided,
            guardBoundaryReached,
            selectedDestinationRawFileOffset,
            stopAtRawFileOffsetExclusive);
}
