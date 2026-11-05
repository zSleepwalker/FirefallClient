using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct ReliableReceiveRecordWordCapture(
    uint CapturedSlotRecordPointer,
    uint PayloadPointer,
    ushort? RecordWordAtPointerPlusTwo,
    ushort? PayloadWordAtPointerPlusTwo,
    bool? DiagnosticReturnedNormally);

public enum ReliableReceiveRecordWordOutcome
{
    InvalidRecordEntry,
    MissingRecordWordCapture,
    UnreachedPayloadWordCapture,
    MissingPayloadWordCapture,
    UnreachedDiagnosticReturnCapture,
    DiagnosticSkippedForEqualWords,
    MissingDiagnosticReturnCapture,
    DiagnosticDidNotReturnNormally,
    DiagnosticReturnedNormally
}

public readonly record struct ReliableReceiveRecordWordPlan(
    ReliableReceiveRecordWordCapture Capture,
    ReliableReceiveRecordWordOutcome Outcome,
    bool UnreachedRecordWordCaptureProvided,
    bool RecordWordRead,
    uint? RecordWordAddress,
    bool UnreachedPayloadWordCaptureProvided,
    bool UnreachedDiagnosticReturnCaptureProvided,
    bool PayloadWordRead,
    uint? PayloadWordAddress,
    bool? WordsEqual,
    bool? EqualityBranchTaken,
    bool DiagnosticCallReached,
    ImmutableArray<uint> DiagnosticStackPushes,
    uint? DiagnosticCallPointerVirtualAddress,
    bool? DiagnosticReturnedNormally,
    bool CleanupReached,
    ushort? CleanupStackBytes,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveRecordWordInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the record/payload WORD comparison and optional diagnostic call,
/// stopping at the boundary immediately following its normal-return cleanup.
/// </summary>
public static class ReliableReceiveRecordWordProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint ComparisonStartRawFileOffset = 0x00BE532F;
    public const uint ComparisonStartVirtualAddress = 0x00FE5F2F;
    public const uint ComparisonEndRawFileOffsetExclusive = 0x00BE5353;
    public const uint RecordWordOffset = 2;
    public const uint PayloadWordOffset = 2;

    public const uint RecordWordReadRawFileOffset = 0x00BE532F;
    public const uint PayloadWordReadRawFileOffset = 0x00BE5333;
    public const uint EqualityBranchRawFileOffset = 0x00BE5337;
    public const uint EqualityBranchVirtualAddress = 0x00FE5F37;
    public const uint EqualityBranchDestinationRawFileOffset = 0x00BE5353;
    public const uint DiagnosticSetupRawFileOffset = 0x00BE5339;
    public const uint DiagnosticCallRawFileOffset = 0x00BE534A;
    public const uint DiagnosticCallReturnRawFileOffset = 0x00BE5350;
    public const uint DiagnosticCallPointerVirtualAddress = 0x01C15EA0;
    public const ushort DiagnosticCleanupStackBytes = 0x10;

    public const string CapturedComparisonBytes =
        "66 8B 40 02 66 3B 47 02 74 1A 6A 00 68 FA 01 00 00 "
        + "68 3E 8B B5 01 68 1B 8B B5 01 FF 15 A0 5E C1 01 83 C4 10";

    public static ImmutableArray<ReliableReceiveRecordWordInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveRecordWordInstructionEvidence(
                "Read the WORD at the captured slot-record pointer plus two",
                RecordWordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(RecordWordReadRawFileOffset),
                "66 8B 40 02"),
            new ReliableReceiveRecordWordInstructionEvidence(
                "Compare it with the independently captured WORD at payload pointer plus two",
                PayloadWordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(PayloadWordReadRawFileOffset),
                "66 3B 47 02"),
            new ReliableReceiveRecordWordInstructionEvidence(
                "Equal WORDs jump over the diagnostic call to raw 0xBE5353",
                EqualityBranchRawFileOffset,
                EqualityBranchVirtualAddress,
                "74 1A",
                EqualityBranchDestinationRawFileOffset,
                RawFileOffsetToVirtualAddress(EqualityBranchDestinationRawFileOffset)),
            new ReliableReceiveRecordWordInstructionEvidence(
                "Push diagnostic arguments in observed machine order: 0, 0x1FA, 0x1B58B3E, 0x1B58B1B",
                DiagnosticSetupRawFileOffset,
                RawFileOffsetToVirtualAddress(DiagnosticSetupRawFileOffset),
                "6A 00 68 FA 01 00 00 68 3E 8B B5 01 68 1B 8B B5 01"),
            new ReliableReceiveRecordWordInstructionEvidence(
                "Call through the absolute pointer stored at VA 0x1C15EA0",
                DiagnosticCallRawFileOffset,
                RawFileOffsetToVirtualAddress(DiagnosticCallRawFileOffset),
                "FF 15 A0 5E C1 01"),
            new ReliableReceiveRecordWordInstructionEvidence(
                "On normal return only, discard the four pushed DWORD arguments",
                DiagnosticCallReturnRawFileOffset,
                RawFileOffsetToVirtualAddress(DiagnosticCallReturnRawFileOffset),
                "83 C4 10"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint RecordWordAddress(uint capturedSlotRecordPointer) =>
        unchecked(capturedSlotRecordPointer + RecordWordOffset);

    public static uint PayloadWordAddress(uint payloadPointer) =>
        unchecked(payloadPointer + PayloadWordOffset);

    public static ReliableReceiveRecordWordPlan Project(
        ReliableReceiveRecordWordCapture capture)
    {
        if (capture.CapturedSlotRecordPointer == 0)
        {
            return MakePlan(
                capture,
                ReliableReceiveRecordWordOutcome.InvalidRecordEntry,
                unreachedRecordCapture: capture.RecordWordAtPointerPlusTwo.HasValue,
                recordRead: false,
                recordAddress: null,
                unreachedPayloadCapture: capture.PayloadWordAtPointerPlusTwo.HasValue,
                unreachedDiagnosticCapture: capture.DiagnosticReturnedNormally.HasValue,
                payloadRead: false,
                payloadAddress: null,
                wordsEqual: null,
                equalityBranchTaken: null,
                callReached: false,
                returnedNormally: null,
                cleanupReached: false,
                selectedDestination: null,
                stopAt: RecordWordReadRawFileOffset);
        }

        uint recordAddress = RecordWordAddress(capture.CapturedSlotRecordPointer);
        if (capture.RecordWordAtPointerPlusTwo is not ushort recordWord)
        {
            return MakePlan(
                capture,
                capture.PayloadWordAtPointerPlusTwo.HasValue
                    ? ReliableReceiveRecordWordOutcome.UnreachedPayloadWordCapture
                    : capture.DiagnosticReturnedNormally.HasValue
                        ? ReliableReceiveRecordWordOutcome.UnreachedDiagnosticReturnCapture
                        : ReliableReceiveRecordWordOutcome.MissingRecordWordCapture,
                unreachedRecordCapture: false,
                recordRead: false,
                recordAddress,
                unreachedPayloadCapture: capture.PayloadWordAtPointerPlusTwo.HasValue,
                unreachedDiagnosticCapture: capture.DiagnosticReturnedNormally.HasValue,
                payloadRead: false,
                payloadAddress: null,
                wordsEqual: null,
                equalityBranchTaken: null,
                callReached: false,
                returnedNormally: null,
                cleanupReached: false,
                selectedDestination: null,
                stopAt: RecordWordReadRawFileOffset);
        }

        uint payloadAddress = PayloadWordAddress(capture.PayloadPointer);
        if (capture.PayloadWordAtPointerPlusTwo is not ushort payloadWord)
        {
            return MakePlan(
                capture,
                ReliableReceiveRecordWordOutcome.MissingPayloadWordCapture,
                unreachedRecordCapture: false,
                recordRead: true,
                recordAddress,
                unreachedPayloadCapture: false,
                unreachedDiagnosticCapture: capture.DiagnosticReturnedNormally.HasValue,
                payloadRead: false,
                payloadAddress,
                wordsEqual: null,
                equalityBranchTaken: null,
                callReached: false,
                returnedNormally: null,
                cleanupReached: false,
                selectedDestination: null,
                stopAt: PayloadWordReadRawFileOffset);
        }

        bool wordsEqual = recordWord == payloadWord;
        if (wordsEqual)
        {
            return MakePlan(
                capture,
                capture.DiagnosticReturnedNormally.HasValue
                    ? ReliableReceiveRecordWordOutcome.UnreachedDiagnosticReturnCapture
                    : ReliableReceiveRecordWordOutcome.DiagnosticSkippedForEqualWords,
                unreachedRecordCapture: false,
                recordRead: true,
                recordAddress,
                unreachedPayloadCapture: false,
                unreachedDiagnosticCapture: capture.DiagnosticReturnedNormally.HasValue,
                payloadRead: true,
                payloadAddress,
                wordsEqual,
                equalityBranchTaken: true,
                callReached: false,
                returnedNormally: null,
                cleanupReached: false,
                selectedDestination: EqualityBranchDestinationRawFileOffset,
                stopAt: ComparisonEndRawFileOffsetExclusive);
        }

        if (capture.DiagnosticReturnedNormally is not bool diagnosticReturnedNormally)
        {
            return MakePlan(
                capture,
                ReliableReceiveRecordWordOutcome.MissingDiagnosticReturnCapture,
                unreachedRecordCapture: false,
                recordRead: true,
                recordAddress,
                unreachedPayloadCapture: false,
                unreachedDiagnosticCapture: false,
                payloadRead: true,
                payloadAddress,
                wordsEqual,
                equalityBranchTaken: false,
                callReached: true,
                returnedNormally: null,
                cleanupReached: false,
                selectedDestination: null,
                stopAt: DiagnosticCallReturnRawFileOffset);
        }

        if (!diagnosticReturnedNormally)
        {
            return MakePlan(
                capture,
                ReliableReceiveRecordWordOutcome.DiagnosticDidNotReturnNormally,
                unreachedRecordCapture: false,
                recordRead: true,
                recordAddress,
                unreachedPayloadCapture: false,
                unreachedDiagnosticCapture: false,
                payloadRead: true,
                payloadAddress,
                wordsEqual,
                equalityBranchTaken: false,
                callReached: true,
                returnedNormally: false,
                cleanupReached: false,
                selectedDestination: null,
                stopAt: DiagnosticCallReturnRawFileOffset);
        }

        return MakePlan(
            capture,
            ReliableReceiveRecordWordOutcome.DiagnosticReturnedNormally,
            unreachedRecordCapture: false,
            recordRead: true,
            recordAddress,
            unreachedPayloadCapture: false,
            unreachedDiagnosticCapture: false,
            payloadRead: true,
            payloadAddress,
            wordsEqual,
            equalityBranchTaken: false,
            callReached: true,
            returnedNormally: true,
            cleanupReached: true,
            selectedDestination: ComparisonEndRawFileOffsetExclusive,
            stopAt: ComparisonEndRawFileOffsetExclusive);
    }

    private static ReliableReceiveRecordWordPlan MakePlan(
        ReliableReceiveRecordWordCapture capture,
        ReliableReceiveRecordWordOutcome outcome,
        bool unreachedRecordCapture,
        bool recordRead,
        uint? recordAddress,
        bool unreachedPayloadCapture,
        bool unreachedDiagnosticCapture,
        bool payloadRead,
        uint? payloadAddress,
        bool? wordsEqual,
        bool? equalityBranchTaken,
        bool callReached,
        bool? returnedNormally,
        bool cleanupReached,
        uint? selectedDestination,
        uint stopAt) =>
        new(
            capture,
            outcome,
            unreachedRecordCapture,
            recordRead,
            recordAddress,
            unreachedPayloadCapture,
            unreachedDiagnosticCapture,
            payloadRead,
            payloadAddress,
            wordsEqual,
            equalityBranchTaken,
            callReached,
            callReached
                ? ImmutableArray.Create(0u, 0x1FAu, 0x1B58B3Eu, 0x1B58B1Bu)
                : ImmutableArray<uint>.Empty,
            callReached ? DiagnosticCallPointerVirtualAddress : null,
            returnedNormally,
            cleanupReached,
            cleanupReached ? DiagnosticCleanupStackBytes : null,
            selectedDestination,
            stopAt);
}
