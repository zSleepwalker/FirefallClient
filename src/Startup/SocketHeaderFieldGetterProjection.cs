using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum SocketHeaderFieldGetterEntry
{
    First,
    Second
}

public readonly record struct SocketHeaderFieldGetterCapture(
    SocketHeaderFieldGetterEntry Entry,
    uint ReceiverPointer,
    uint? InitialDword,
    bool? DiagnosticReturnedNormally,
    uint? FinalDword);

public enum SocketHeaderFieldGetterOutcome
{
    MissingInitialDword,
    UnreachedCaptureProvided,
    MissingDiagnosticReturnCapture,
    DiagnosticDidNotReturnNormally,
    MissingFinalDword,
    Completed
}

public readonly record struct SocketHeaderFieldGetterPlan(
    SocketHeaderFieldGetterCapture Capture,
    SocketHeaderFieldGetterOutcome Outcome,
    uint InitialFieldAddress,
    uint? FinalFieldAddress,
    bool DiagnosticInvoked,
    uint? DiagnosticCallRawFileOffset,
    uint? DiagnosticFunctionPointerCellVirtualAddress,
    uint? FinalFieldDword,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct SocketHeaderFieldGetterInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects the identical 35-byte code bodies at raw BD5BA0 and BD5C00.
/// The 36-byte captured windows include the following CC padding byte. The
/// receiver DWORD is loaded independently before and after the opaque call;
/// the second value is not tested, and the getter has no stack argument.
/// </summary>
public static class SocketHeaderFieldGetterProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint FirstEntryRawFileOffset = 0x00BD5BA0;
    public const uint SecondEntryRawFileOffset = 0x00BD5C00;
    public const uint FunctionLength = 0x23;
    public const uint CapturedWindowLength = 0x24;

    public const uint InitialFieldCompareOffset = 0x03;
    public const uint DiagnosticBranchOffset = 0x06;
    public const uint DiagnosticBlockOffset = 0x08;
    public const uint DiagnosticCallOffset = 0x16;
    public const uint DiagnosticCallReturnOffset = 0x1C;
    public const uint FinalFieldReloadOffset = 0x1F;
    public const uint RestoreEsiOffset = 0x21;
    public const uint PlainReturnOffset = 0x22;

    public const uint DiagnosticArgumentZero = 0;
    public const uint DiagnosticCode = 0x5B;
    public const uint DiagnosticStringPointer = 0x01C5EA58;
    public const uint DiagnosticFunctionPointerCellVirtualAddress = 0x01C15EA0;

    public const string CapturedFunctionBytes =
        "56 8B F1 83 3E 00 75 17 6A 00 6A 5B 68 58 EA C5 01 "
            + "68 58 EA C5 01 FF 15 A0 5E C1 01 83 C4 10 8B 06 5E C3";

    public const string CapturedWindowBytes = CapturedFunctionBytes + " CC";

    public static uint EntryRawFileOffset(SocketHeaderFieldGetterEntry entry) =>
        entry switch
        {
            SocketHeaderFieldGetterEntry.First => FirstEntryRawFileOffset,
            SocketHeaderFieldGetterEntry.Second => SecondEntryRawFileOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry, "Unsupported getter entry.")
        };

    public static uint EntryVirtualAddress(SocketHeaderFieldGetterEntry entry) =>
        unchecked(EntryRawFileOffset(entry) + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FunctionEndRawFileOffsetExclusive(SocketHeaderFieldGetterEntry entry) =>
        unchecked(EntryRawFileOffset(entry) + FunctionLength);

    public static uint FunctionEndVirtualAddressExclusive(SocketHeaderFieldGetterEntry entry) =>
        unchecked(FunctionEndRawFileOffsetExclusive(entry) + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint CapturedWindowEndRawFileOffsetExclusive(SocketHeaderFieldGetterEntry entry) =>
        unchecked(EntryRawFileOffset(entry) + CapturedWindowLength);

    public static ImmutableArray<SocketHeaderFieldGetterInstructionEvidence> Instructions(
        SocketHeaderFieldGetterEntry entry)
    {
        uint entryRaw = EntryRawFileOffset(entry);
        uint diagnosticBranchTarget = Offset(entryRaw, 0x1F);

        return ImmutableArray.Create(
            new SocketHeaderFieldGetterInstructionEvidence(
                "Save ESI",
                entryRaw,
                "56"),
            new SocketHeaderFieldGetterInstructionEvidence(
                "Capture the receiver from ECX",
                Offset(entryRaw, 0x01),
                "8B F1"),
            new SocketHeaderFieldGetterInstructionEvidence(
                "Compare the initial receiver DWORD with zero",
                Offset(entryRaw, InitialFieldCompareOffset),
                "83 3E 00"),
            new SocketHeaderFieldGetterInstructionEvidence(
                "A nonzero initial DWORD skips the diagnostic block",
                Offset(entryRaw, DiagnosticBranchOffset),
                "75 17",
                diagnosticBranchTarget,
                unchecked(diagnosticBranchTarget + CodeVirtualAddressDeltaFromRawFileOffset)),
            new SocketHeaderFieldGetterInstructionEvidence(
                "Push diagnostic argument zero",
                Offset(entryRaw, DiagnosticBlockOffset),
                "6A 00"),
            new SocketHeaderFieldGetterInstructionEvidence(
                "Push diagnostic code 0x5B",
                Offset(entryRaw, 0x0A),
                "6A 5B"),
            new SocketHeaderFieldGetterInstructionEvidence(
                "Push first 0x01C5EA58 argument",
                Offset(entryRaw, 0x0C),
                "68 58 EA C5 01",
                TargetVirtualAddress: DiagnosticStringPointer),
            new SocketHeaderFieldGetterInstructionEvidence(
                "Push second 0x01C5EA58 argument",
                Offset(entryRaw, 0x11),
                "68 58 EA C5 01",
                TargetVirtualAddress: DiagnosticStringPointer),
            new SocketHeaderFieldGetterInstructionEvidence(
                "Call through the indirect function-pointer cell",
                Offset(entryRaw, DiagnosticCallOffset),
                "FF 15 A0 5E C1 01",
                TargetVirtualAddress: DiagnosticFunctionPointerCellVirtualAddress),
            new SocketHeaderFieldGetterInstructionEvidence(
                "Clean up the four diagnostic arguments after normal return",
                Offset(entryRaw, DiagnosticCallReturnOffset),
                "83 C4 10"),
            new SocketHeaderFieldGetterInstructionEvidence(
                "Reload the receiver DWORD on both paths",
                Offset(entryRaw, FinalFieldReloadOffset),
                "8B 06"),
            new SocketHeaderFieldGetterInstructionEvidence(
                "Restore ESI",
                Offset(entryRaw, RestoreEsiOffset),
                "5E"),
            new SocketHeaderFieldGetterInstructionEvidence(
                "Return with plain RET",
                Offset(entryRaw, PlainReturnOffset),
                "C3"));
    }

    public static SocketHeaderFieldGetterPlan Project(SocketHeaderFieldGetterCapture capture)
    {
        uint entryRaw = EntryRawFileOffset(capture.Entry);
        uint initialFieldAddress = FieldAddress(capture.ReceiverPointer);
        uint initialCompare = Offset(entryRaw, InitialFieldCompareOffset);
        uint diagnosticBlock = Offset(entryRaw, DiagnosticBlockOffset);
        uint diagnosticCall = Offset(entryRaw, DiagnosticCallOffset);
        uint diagnosticCallReturn = Offset(entryRaw, DiagnosticCallReturnOffset);
        uint finalFieldReload = Offset(entryRaw, FinalFieldReloadOffset);
        uint functionEnd = Offset(entryRaw, FunctionLength);

        if (capture.InitialDword is null)
        {
            return Plan(
                capture,
                capture.DiagnosticReturnedNormally is not null || capture.FinalDword is not null
                    ? SocketHeaderFieldGetterOutcome.UnreachedCaptureProvided
                    : SocketHeaderFieldGetterOutcome.MissingInitialDword,
                initialFieldAddress,
                finalFieldAddress: null,
                diagnosticInvoked: false,
                diagnosticCallRawFileOffset: null,
                finalFieldDword: null,
                stopAtRawFileOffsetExclusive: initialCompare);
        }

        if (capture.InitialDword.Value != 0)
        {
            if (capture.DiagnosticReturnedNormally is not null)
            {
                return Plan(
                    capture,
                    SocketHeaderFieldGetterOutcome.UnreachedCaptureProvided,
                    initialFieldAddress,
                    finalFieldAddress: null,
                    diagnosticInvoked: false,
                    diagnosticCallRawFileOffset: null,
                    finalFieldDword: null,
                    stopAtRawFileOffsetExclusive: diagnosticCall);
            }

            return FinalFieldPlan(
                capture,
                initialFieldAddress,
                finalFieldReload,
                functionEnd);
        }

        if (capture.DiagnosticReturnedNormally is null)
        {
            return Plan(
                capture,
                capture.FinalDword is null
                    ? SocketHeaderFieldGetterOutcome.MissingDiagnosticReturnCapture
                    : SocketHeaderFieldGetterOutcome.UnreachedCaptureProvided,
                initialFieldAddress,
                finalFieldAddress: null,
                diagnosticInvoked: true,
                diagnosticCallRawFileOffset: diagnosticCall,
                finalFieldDword: null,
                stopAtRawFileOffsetExclusive: diagnosticCall);
        }

        if (!capture.DiagnosticReturnedNormally.Value)
        {
            return Plan(
                capture,
                capture.FinalDword is null
                    ? SocketHeaderFieldGetterOutcome.DiagnosticDidNotReturnNormally
                    : SocketHeaderFieldGetterOutcome.UnreachedCaptureProvided,
                initialFieldAddress,
                finalFieldAddress: null,
                diagnosticInvoked: true,
                diagnosticCallRawFileOffset: diagnosticCall,
                finalFieldDword: null,
                stopAtRawFileOffsetExclusive: diagnosticCallReturn);
        }

        return FinalFieldPlan(
            capture,
            initialFieldAddress,
            finalFieldReload,
            functionEnd);
    }

    private static SocketHeaderFieldGetterPlan FinalFieldPlan(
        SocketHeaderFieldGetterCapture capture,
        uint initialFieldAddress,
        uint finalFieldReload,
        uint functionEnd)
    {
        uint finalFieldAddress = FieldAddress(capture.ReceiverPointer);
        return Plan(
            capture,
            capture.FinalDword is null
                ? SocketHeaderFieldGetterOutcome.MissingFinalDword
                : SocketHeaderFieldGetterOutcome.Completed,
            initialFieldAddress,
            finalFieldAddress,
            diagnosticInvoked: capture.InitialDword == 0,
            diagnosticCallRawFileOffset: capture.InitialDword == 0
                ? Offset(EntryRawFileOffset(capture.Entry), DiagnosticCallOffset)
                : null,
            finalFieldDword: capture.FinalDword,
            stopAtRawFileOffsetExclusive: capture.FinalDword is null
                ? finalFieldReload
                : functionEnd);
    }

    private static SocketHeaderFieldGetterPlan Plan(
        SocketHeaderFieldGetterCapture capture,
        SocketHeaderFieldGetterOutcome outcome,
        uint initialFieldAddress,
        uint? finalFieldAddress,
        bool diagnosticInvoked,
        uint? diagnosticCallRawFileOffset,
        uint? finalFieldDword,
        uint stopAtRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            initialFieldAddress,
            finalFieldAddress,
            diagnosticInvoked,
            diagnosticCallRawFileOffset,
            diagnosticInvoked ? DiagnosticFunctionPointerCellVirtualAddress : null,
            finalFieldDword,
            stopAtRawFileOffsetExclusive);

    private static uint FieldAddress(uint receiverPointer) =>
        unchecked(receiverPointer + 0);

    private static uint Offset(uint entryRawFileOffset, uint offset) =>
        unchecked(entryRawFileOffset + offset);
}
