using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct ReliableReceiveEntryCapture(
    byte? FlagAtOffset0C,
    byte? FlagAtOffset1C,
    bool? DiagnosticCallReturnedNormally);

public enum ReliableReceiveEntryOutcome
{
    MissingFlagAtOffset0CCapture,
    UnreachedDiagnosticReturnCapture,
    UnreachedFlagAtOffset1CCapture,
    MissingDiagnosticReturnCapture,
    DiagnosticCallDidNotReturnNormally,
    MissingFlagAtOffset1CCapture,
    EarlyReturn,
    ArgumentLoadBoundarySelected
}

public readonly record struct ReliableReceiveEntryPlan(
    ReliableReceiveEntryCapture Capture,
    ReliableReceiveEntryOutcome Outcome,
    bool FirstFlagRead,
    bool? FirstFlagIsZero,
    bool FirstConditionalBranchTaken,
    bool DiagnosticCallReached,
    bool? DiagnosticCallReturnedNormally,
    bool UnreachedDiagnosticReturnCaptureProvided,
    bool SecondFlagRead,
    byte? FlagAtOffset1C,
    bool? SecondConditionalBranchTaken,
    bool UnreachedFlagAtOffset1CCaptureProvided,
    byte? ReturnAl,
    ushort? ReturnStackArgumentBytes,
    bool ArgumentLoadBoundarySelected,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveEntryInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the initial two byte-flag gates and optional diagnostic call,
/// ending at the early RET 4 or immediately before the argument-load block.
/// </summary>
public static class ReliableReceiveEntryProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryStartRawFileOffset = 0x00BE51F0;
    public const uint EntryStartVirtualAddress = 0x00FE5DF0;
    public const uint FirstFlagOffset = 0x0C;
    public const uint FirstFlagTestRawFileOffset = 0x00BE51F8;
    public const uint FirstFlagBranchRawFileOffset = 0x00BE51FC;
    public const uint FirstFlagBranchVirtualAddress = 0x00FE5DFC;
    public const uint FirstFlagBranchTargetRawFileOffset = 0x00BE5218;
    public const uint FirstFlagBranchTargetVirtualAddress = 0x00FE5E18;
    public const uint DiagnosticPushesRawFileOffset = 0x00BE51FE;
    public const uint DiagnosticCallRawFileOffset = 0x00BE520F;
    public const uint DiagnosticCallReturnRawFileOffset = 0x00BE5215;
    public const uint DiagnosticCallPointerSlotAddress = 0x01C15EA0;
    public const uint DiagnosticLiteralPointerA = 0x01B58B7A;
    public const uint DiagnosticLiteralPointerB = 0x01B58B7B;
    public const uint DiagnosticCode = 0x1DA;
    public const uint DiagnosticZeroArgument = 0;
    public const uint SecondFlagOffset = 0x1C;
    public const uint SecondFlagTestRawFileOffset = 0x00BE5218;
    public const uint SecondFlagBranchRawFileOffset = 0x00BE521C;
    public const uint SecondFlagBranchVirtualAddress = 0x00FE5E1C;
    public const uint SecondFlagBranchTargetRawFileOffset = 0x00BE5227;
    public const uint SecondFlagBranchTargetVirtualAddress = 0x00FE5E27;
    public const uint EarlyReturnXorRawFileOffset = 0x00BE521E;
    public const uint EarlyReturnEpilogueRawFileOffset = 0x00BE5220;
    public const uint EarlyReturnRawFileOffset = 0x00BE5224;
    public const uint EarlyReturnVirtualAddress = 0x00FE5E24;
    public const ushort EarlyReturnStackArgumentBytes = 4;
    public const uint ArgumentLoadBoundaryRawFileOffset = 0x00BE5227;
    public const uint ArgumentLoadBoundaryVirtualAddress = 0x00FE5E27;
    public const uint PrefixEndRawFileOffsetExclusive = ArgumentLoadBoundaryRawFileOffset;

    public const string CapturedEntryBytes =
        "55 8B EC 53 56 8B F1 57 80 7E 0C 00 75 1A 6A 00 68 DA 01 00 00 "
        + "68 7A 8B B5 01 68 7B 8B B5 01 FF 15 A0 5E C1 01 83 C4 10 "
        + "80 7E 1C 00 74 09 32 C0 5F 5E 5B 5D C2 04 00";

    public static ImmutableArray<uint> DiagnosticPushValuesInInstructionOrder { get; } =
        ImmutableArray.Create(
            DiagnosticZeroArgument,
            DiagnosticCode,
            DiagnosticLiteralPointerA,
            DiagnosticLiteralPointerB);

    public static ImmutableArray<ReliableReceiveEntryInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveEntryInstructionEvidence(
                "Save the incoming ECX object pointer in ESI before either byte-flag read",
                0x00BE51F0,
                "55 8B EC 53 56 8B F1 57"),
            new ReliableReceiveEntryInstructionEvidence(
                "Read the first byte flag at ESI+0x0C; no null guard precedes this access",
                FirstFlagTestRawFileOffset,
                "80 7E 0C 00"),
            new ReliableReceiveEntryInstructionEvidence(
                "JNZ skips the diagnostic setup and selects the independent ESI+0x1C test",
                FirstFlagBranchRawFileOffset,
                "75 1A",
                FirstFlagBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstFlagBranchTargetRawFileOffset)),
            new ReliableReceiveEntryInstructionEvidence(
                "Push diagnostic arguments in instruction order: 0, 0x1DA, literal pointers 0x01B58B7A and 0x01B58B7B",
                DiagnosticPushesRawFileOffset,
                "6A 00 68 DA 01 00 00 68 7A 8B B5 01 68 7B 8B B5 01"),
            new ReliableReceiveEntryInstructionEvidence(
                "Call through the indirect diagnostic function-pointer slot",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01"),
            new ReliableReceiveEntryInstructionEvidence(
                "Restore the four pushed diagnostic arguments after normal call return",
                DiagnosticCallReturnRawFileOffset,
                "83 C4 10"),
            new ReliableReceiveEntryInstructionEvidence(
                "Read the independent byte flag at ESI+0x1C",
                SecondFlagTestRawFileOffset,
                "80 7E 1C 00"),
            new ReliableReceiveEntryInstructionEvidence(
                "JZ selects the next-block boundary when the second byte flag is zero",
                SecondFlagBranchRawFileOffset,
                "74 09",
                SecondFlagBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(SecondFlagBranchTargetRawFileOffset)),
            new ReliableReceiveEntryInstructionEvidence(
                "A nonzero second byte flag falls through to XOR AL,AL",
                EarlyReturnXorRawFileOffset,
                "32 C0"),
            new ReliableReceiveEntryInstructionEvidence(
                "Restore EDI, ESI, EBX, and EBP, then return while popping one 4-byte argument",
                EarlyReturnEpilogueRawFileOffset,
                "5F 5E 5B 5D C2 04 00"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static ReliableReceiveEntryPlan Project(ReliableReceiveEntryCapture capture)
    {
        if (capture.FlagAtOffset0C is not byte firstFlag)
        {
            ReliableReceiveEntryOutcome outcome =
                capture.DiagnosticCallReturnedNormally.HasValue
                    ? ReliableReceiveEntryOutcome.UnreachedDiagnosticReturnCapture
                    : capture.FlagAtOffset1C.HasValue
                        ? ReliableReceiveEntryOutcome.UnreachedFlagAtOffset1CCapture
                        : ReliableReceiveEntryOutcome.MissingFlagAtOffset0CCapture;
            return Plan(
                capture,
                outcome,
                firstFlagRead: false,
                firstFlagIsZero: null,
                firstConditionalBranchTaken: false,
                diagnosticCallReached: false,
                diagnosticCallReturnedNormally: null,
                secondFlagRead: false,
                flagAtOffset1C: null,
                secondConditionalBranchTaken: null,
                returnAl: null,
                returnStackArgumentBytes: null,
                argumentLoadBoundarySelected: false,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: FirstFlagTestRawFileOffset);
        }

        bool firstFlagIsZero = firstFlag == 0;
        bool diagnosticCallReached = firstFlagIsZero;
        bool? diagnosticReturnedNormally = diagnosticCallReached
            ? capture.DiagnosticCallReturnedNormally
            : null;

        if (!diagnosticCallReached && capture.DiagnosticCallReturnedNormally.HasValue)
        {
            return Plan(
                capture,
                ReliableReceiveEntryOutcome.UnreachedDiagnosticReturnCapture,
                firstFlagRead: true,
                firstFlagIsZero,
                firstConditionalBranchTaken: true,
                diagnosticCallReached: false,
                diagnosticCallReturnedNormally: null,
                secondFlagRead: false,
                flagAtOffset1C: null,
                secondConditionalBranchTaken: null,
                returnAl: null,
                returnStackArgumentBytes: null,
                argumentLoadBoundarySelected: false,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: SecondFlagTestRawFileOffset);
        }

        if (diagnosticCallReached && diagnosticReturnedNormally is null)
        {
            ReliableReceiveEntryOutcome outcome = capture.FlagAtOffset1C.HasValue
                ? ReliableReceiveEntryOutcome.UnreachedFlagAtOffset1CCapture
                : ReliableReceiveEntryOutcome.MissingDiagnosticReturnCapture;
            return Plan(
                capture,
                outcome,
                firstFlagRead: true,
                firstFlagIsZero,
                firstConditionalBranchTaken: false,
                diagnosticCallReached: true,
                diagnosticCallReturnedNormally: null,
                secondFlagRead: false,
                flagAtOffset1C: null,
                secondConditionalBranchTaken: null,
                returnAl: null,
                returnStackArgumentBytes: null,
                argumentLoadBoundarySelected: false,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: DiagnosticCallReturnRawFileOffset);
        }

        if (diagnosticCallReached && diagnosticReturnedNormally == false)
        {
            ReliableReceiveEntryOutcome outcome = capture.FlagAtOffset1C.HasValue
                ? ReliableReceiveEntryOutcome.UnreachedFlagAtOffset1CCapture
                : ReliableReceiveEntryOutcome.DiagnosticCallDidNotReturnNormally;
            return Plan(
                capture,
                outcome,
                firstFlagRead: true,
                firstFlagIsZero,
                firstConditionalBranchTaken: false,
                diagnosticCallReached: true,
                diagnosticCallReturnedNormally: false,
                secondFlagRead: false,
                flagAtOffset1C: null,
                secondConditionalBranchTaken: null,
                returnAl: null,
                returnStackArgumentBytes: null,
                argumentLoadBoundarySelected: false,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: DiagnosticCallReturnRawFileOffset);
        }

        if (capture.FlagAtOffset1C is not byte secondFlag)
        {
            return Plan(
                capture,
                ReliableReceiveEntryOutcome.MissingFlagAtOffset1CCapture,
                firstFlagRead: true,
                firstFlagIsZero,
                firstConditionalBranchTaken: !firstFlagIsZero,
                diagnosticCallReached,
                diagnosticReturnedNormally,
                secondFlagRead: false,
                flagAtOffset1C: null,
                secondConditionalBranchTaken: null,
                returnAl: null,
                returnStackArgumentBytes: null,
                argumentLoadBoundarySelected: false,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: SecondFlagTestRawFileOffset);
        }

        bool secondFlagIsZero = secondFlag == 0;
        if (!secondFlagIsZero)
        {
            return Plan(
                capture,
                ReliableReceiveEntryOutcome.EarlyReturn,
                firstFlagRead: true,
                firstFlagIsZero,
                firstConditionalBranchTaken: !firstFlagIsZero,
                diagnosticCallReached,
                diagnosticReturnedNormally,
                secondFlagRead: true,
                secondFlag,
                secondConditionalBranchTaken: false,
                returnAl: 0,
                returnStackArgumentBytes: EarlyReturnStackArgumentBytes,
                argumentLoadBoundarySelected: false,
                selectedDestinationRawFileOffset: EarlyReturnXorRawFileOffset,
                stopAtRawFileOffsetExclusive: PrefixEndRawFileOffsetExclusive);
        }

        return Plan(
            capture,
            ReliableReceiveEntryOutcome.ArgumentLoadBoundarySelected,
            firstFlagRead: true,
            firstFlagIsZero,
            firstConditionalBranchTaken: !firstFlagIsZero,
            diagnosticCallReached,
            diagnosticReturnedNormally,
            secondFlagRead: true,
            secondFlag,
            secondConditionalBranchTaken: true,
            returnAl: null,
            returnStackArgumentBytes: null,
            argumentLoadBoundarySelected: true,
            selectedDestinationRawFileOffset: ArgumentLoadBoundaryRawFileOffset,
            stopAtRawFileOffsetExclusive: ArgumentLoadBoundaryRawFileOffset);
    }

    private static ReliableReceiveEntryPlan Plan(
        ReliableReceiveEntryCapture capture,
        ReliableReceiveEntryOutcome outcome,
        bool firstFlagRead,
        bool? firstFlagIsZero,
        bool firstConditionalBranchTaken,
        bool diagnosticCallReached,
        bool? diagnosticCallReturnedNormally,
        bool secondFlagRead,
        byte? flagAtOffset1C,
        bool? secondConditionalBranchTaken,
        byte? returnAl,
        ushort? returnStackArgumentBytes,
        bool argumentLoadBoundarySelected,
        uint? selectedDestinationRawFileOffset,
        uint stopAtRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            firstFlagRead,
            firstFlagIsZero,
            firstConditionalBranchTaken,
            diagnosticCallReached,
            diagnosticCallReturnedNormally,
            capture.DiagnosticCallReturnedNormally.HasValue && !diagnosticCallReached,
            secondFlagRead,
            flagAtOffset1C,
            secondConditionalBranchTaken,
            capture.FlagAtOffset1C.HasValue && !secondFlagRead,
            returnAl,
            returnStackArgumentBytes,
            argumentLoadBoundarySelected,
            selectedDestinationRawFileOffset,
            stopAtRawFileOffsetExclusive);
}
