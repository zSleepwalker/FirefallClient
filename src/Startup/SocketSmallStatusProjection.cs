using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct SocketSmallStatusCapture(
    uint ReceiverPointer,
    uint? Edi,
    bool? Case2NormalReturn,
    uint? ReceiverDword108,
    byte? PredicateAl,
    ushort? ReceiverWord248,
    bool? DiagnosticNormalReturn,
    bool? SetStateNormalReturn);

public enum SocketSmallStatusOutcome
{
    MissingEntryEdi,
    EntryEdiZeroRejected,
    EntryEdiAtLeast16Rejected,
    UnreachedCaptureProvided,
    MissingCase2ReturnCapture,
    Case2DidNotReturnNormally,
    Case2NormalReturnJoins,
    OtherSmallStatusBranchesToNextDispatch,
    MissingReceiverDword108,
    Case3StateNotFourJoins,
    MissingPredicateAl,
    MissingReceiverWord248,
    MissingDiagnosticReturnCapture,
    DiagnosticDidNotReturnNormally,
    MissingSetStateReturnCapture,
    SetStateDidNotReturnNormally,
    SetStateNormalReturnJoins
}

public enum SocketSmallStatusDiagnosticPath
{
    NotReached,
    PredicateAlZero,
    ReceiverWord248Zero,
    ReceiverWord248Nonzero
}

public readonly record struct SocketSmallStatusPlan(
    SocketSmallStatusCapture Capture,
    SocketSmallStatusOutcome Outcome,
    SocketSmallStatusDiagnosticPath DiagnosticPath,
    uint? CallRawFileOffset,
    uint? CallEcx,
    uint? CallStackArgument,
    uint? BranchTargetRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct SocketSmallStatusInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null);

/// <summary>
/// Projects the verified nonzero EDI values below 0x10 that fall through to
/// raw BD3EBF, covering only status 2, status 3, and their other-status edge.
/// Calls are modeled only through captured normal returns; no event-loop,
/// helper, or semantic state behavior is inferred.
/// </summary>
public static class SocketSmallStatusProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x00BD3EBF;
    public const uint EntryEndRawFileOffsetExclusive = 0x00BD3F1C;
    public const uint JoinRawFileOffset = 0x00BD3E60;
    public const uint OtherStatusTargetRawFileOffset = 0x00BD3F1C;
    public const uint PriorZeroBranchTargetRawFileOffset = 0x00BD3E60;
    public const uint PriorUnsignedAtLeast16BranchTargetRawFileOffset = 0x00BD40BF;

    public const uint Case2CallRawFileOffset = 0x00BD3EC6;
    public const uint Case2CallReturnRawFileOffset = 0x00BD3ECB;
    public const uint Case2JoinBranchEndRawFileOffset = 0x00BD3ECD;
    public const uint OtherStatusBranchEndRawFileOffset = 0x00BD3ED2;
    public const uint Case3StateCompareRawFileOffset = 0x00BD3ED2;
    public const uint Case3JoinBranchEndRawFileOffset = 0x00BD3EDB;
    public const uint PredicateCallRawFileOffset = 0x00BD3EE1;
    public const uint PredicateCallReturnRawFileOffset = 0x00BD3EE6;
    public const uint PredicateAlZeroBranchRawFileOffset = 0x00BD3EE8;
    public const uint PredicateZeroBranchEndRawFileOffset = 0x00BD3EEA;
    public const uint Word248CompareRawFileOffset = 0x00BD3EEA;
    public const uint DiagnosticBlockRawFileOffset = 0x00BD3EF4;
    public const uint DiagnosticCallRawFileOffset = 0x00BD3F05;
    public const uint DiagnosticCallReturnRawFileOffset = 0x00BD3F0B;
    public const uint SetStateBlockRawFileOffset = 0x00BD3F0E;
    public const uint SetStateCallRawFileOffset = 0x00BD3F12;
    public const uint SetStateCallReturnRawFileOffset = 0x00BD3F17;
    public const uint SetStateJoinBranchEndRawFileOffset = 0x00BD3F1C;

    public const uint ReceiverDword108Offset = 0x108;
    public const uint PredicateReceiverOffset = 0x230;
    public const uint ReceiverWord248Offset = 0x248;
    public const uint SetStateArgument = 5;
    public const uint DiagnosticCode = 0x4EB;

    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EntryEndVirtualAddressExclusive =
        EntryEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint Case2CallVirtualAddress =
        Case2CallRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint PredicateCallVirtualAddress =
        PredicateCallRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint SetStateCallVirtualAddress =
        SetStateCallRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedFunctionBytes =
        "83 FF 02 75 09 8B CE E8 65 E5 FF FF EB 93 "
            + "83 FF 03 75 4A 83 BE 08 01 00 00 04 75 85 "
            + "8D 8E 30 02 00 00 E8 7A 8B FF FF 84 C0 74 0A "
            + "66 83 BE 48 02 00 00 00 75 1A 6A 00 68 EB 04 00 00 "
            + "68 AF 83 B5 01 68 B2 83 B5 01 FF 15 A0 5E C1 01 "
            + "83 C4 10 6A 05 8B CE E8 59 DC FF FF E9 44 FF FF FF";

    public const uint Case2CallTargetRawFileOffset = 0x00BD2430;
    public const uint PredicateCallTargetRawFileOffset = 0x00BCCA60;
    public const uint SetStateCallTargetRawFileOffset = 0x00BD1B70;
    public const uint Case2CallTargetVirtualAddress =
        Case2CallTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint PredicateCallTargetVirtualAddress =
        PredicateCallTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint SetStateCallTargetVirtualAddress =
        SetStateCallTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const bool Case2SecondArgumentSetupObserved = false;
    public const string DiagnosticPredicate =
        "AL == 0 OR (AL != 0 AND receiver[+0x248] == 0)";

    public static ReadOnlyCollection<SocketSmallStatusInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new SocketSmallStatusInstructionEvidence(
                "Status 2 calls BD2430 with ECX equal to the receiver",
                0x00BD3EC4,
                "8B CE E8 65 E5 FF FF"),
            new SocketSmallStatusInstructionEvidence(
                "Status 2's normal call return unconditionally joins BD3E60",
                Case2CallReturnRawFileOffset,
                "EB 93",
                JoinRawFileOffset),
            new SocketSmallStatusInstructionEvidence(
                "Only status 3 falls through the next status comparison",
                0x00BD3ECD,
                "83 FF 03 75 4A",
                OtherStatusTargetRawFileOffset),
            new SocketSmallStatusInstructionEvidence(
                "Status 3 requires the independently captured receiver DWORD at +0x108 to equal 4",
                Case3StateCompareRawFileOffset,
                "83 BE 08 01 00 00 04 75 85",
                JoinRawFileOffset),
            new SocketSmallStatusInstructionEvidence(
                "The predicate helper receives receiver+0x230 in ECX and returns into TEST AL,AL",
                0x00BD3EDB,
                "8D 8E 30 02 00 00 E8 7A 8B FF FF 84 C0",
                PredicateCallTargetRawFileOffset),
            new SocketSmallStatusInstructionEvidence(
                "AL zero branches directly to the diagnostic block before the +0x248 word read",
                PredicateAlZeroBranchRawFileOffset,
                "74 0A",
                DiagnosticBlockRawFileOffset),
            new SocketSmallStatusInstructionEvidence(
                "Only nonzero AL reaches the independent receiver word comparison",
                Word248CompareRawFileOffset,
                "66 83 BE 48 02 00 00 00 75 1A",
                SetStateBlockRawFileOffset),
            new SocketSmallStatusInstructionEvidence(
                "The diagnostic block pushes four stack arguments including code 0x4EB",
                DiagnosticBlockRawFileOffset,
                "6A 00 68 EB 04 00 00 68 AF 83 B5 01 68 B2 83 B5 01"),
            new SocketSmallStatusInstructionEvidence(
                "The diagnostic block calls through its observed indirect target",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01"),
            new SocketSmallStatusInstructionEvidence(
                "After a normal diagnostic return and stack cleanup, status 5 is passed to BD1B70",
                SetStateBlockRawFileOffset,
                "6A 05 8B CE E8 59 DC FF FF",
                SetStateCallTargetRawFileOffset),
            new SocketSmallStatusInstructionEvidence(
                "Status-setting normal return joins BD3E60",
                SetStateCallReturnRawFileOffset,
                "E9 44 FF FF FF",
                JoinRawFileOffset)
        });

    public static SocketSmallStatusPlan Project(SocketSmallStatusCapture capture)
    {
        if (capture.Edi is not uint edi)
        {
            if (HasAnyStatusCaptures(capture))
            {
                return Stop(
                    capture,
                    SocketSmallStatusOutcome.UnreachedCaptureProvided,
                    EntryRawFileOffset);
            }

            return Stop(capture, SocketSmallStatusOutcome.MissingEntryEdi, EntryRawFileOffset);
        }

        if (edi == 0)
        {
            return Stop(capture, SocketSmallStatusOutcome.EntryEdiZeroRejected, EntryRawFileOffset);
        }

        if (edi >= 0x10)
        {
            return Stop(capture, SocketSmallStatusOutcome.EntryEdiAtLeast16Rejected, EntryRawFileOffset);
        }

        if (edi == 2)
        {
            if (HasCase3Captures(capture))
            {
                return Stop(
                    capture,
                    SocketSmallStatusOutcome.UnreachedCaptureProvided,
                    Case2CallRawFileOffset);
            }

            if (capture.Case2NormalReturn is not bool returnedNormally)
            {
                return Stop(
                    capture,
                    SocketSmallStatusOutcome.MissingCase2ReturnCapture,
                    Case2CallReturnRawFileOffset,
                    callRawFileOffset: Case2CallRawFileOffset,
                    callEcx: capture.ReceiverPointer);
            }

            return returnedNormally
                ? Join(
                    capture,
                    SocketSmallStatusOutcome.Case2NormalReturnJoins,
                    Case2JoinBranchEndRawFileOffset,
                    callRawFileOffset: Case2CallRawFileOffset,
                    callEcx: capture.ReceiverPointer)
                : Stop(
                    capture,
                    SocketSmallStatusOutcome.Case2DidNotReturnNormally,
                    Case2CallReturnRawFileOffset,
                    callRawFileOffset: Case2CallRawFileOffset,
                    callEcx: capture.ReceiverPointer);
        }

        if (edi != 3)
        {
            if (HasCase2Captures(capture) || HasCase3Captures(capture))
            {
                return Stop(
                    capture,
                    SocketSmallStatusOutcome.UnreachedCaptureProvided,
                    EntryRawFileOffset);
            }

            return Stop(
                capture,
                SocketSmallStatusOutcome.OtherSmallStatusBranchesToNextDispatch,
                OtherStatusBranchEndRawFileOffset,
                branchTarget: OtherStatusTargetRawFileOffset);
        }

        if (capture.Case2NormalReturn is not null)
        {
            return Stop(
                capture,
                SocketSmallStatusOutcome.UnreachedCaptureProvided,
                Case3StateCompareRawFileOffset);
        }

        if (capture.ReceiverDword108 is not uint state108)
        {
            if (HasPredicateAndLaterCaptures(capture))
            {
                return Stop(
                    capture,
                    SocketSmallStatusOutcome.UnreachedCaptureProvided,
                    Case3StateCompareRawFileOffset);
            }

            return Stop(
                capture,
                SocketSmallStatusOutcome.MissingReceiverDword108,
                Case3StateCompareRawFileOffset);
        }

        if (state108 != 4)
        {
            if (HasPredicateAndLaterCaptures(capture))
            {
                return Stop(
                    capture,
                    SocketSmallStatusOutcome.UnreachedCaptureProvided,
                    Case3StateCompareRawFileOffset);
            }

            return Join(
                capture,
                SocketSmallStatusOutcome.Case3StateNotFourJoins,
                Case3JoinBranchEndRawFileOffset,
                branchTarget: JoinRawFileOffset);
        }

        if (capture.PredicateAl is not byte al)
        {
            if (HasPredicateAndLaterCaptures(capture))
            {
                return Stop(
                    capture,
                    SocketSmallStatusOutcome.UnreachedCaptureProvided,
                    PredicateCallReturnRawFileOffset,
                    callRawFileOffset: PredicateCallRawFileOffset,
                    callEcx: PredicateReceiverAddress(capture.ReceiverPointer));
            }

            return Stop(
                capture,
                SocketSmallStatusOutcome.MissingPredicateAl,
                PredicateCallReturnRawFileOffset,
                callRawFileOffset: PredicateCallRawFileOffset,
                callEcx: PredicateReceiverAddress(capture.ReceiverPointer));
        }

        SocketSmallStatusDiagnosticPath diagnosticPath;
        if (al == 0)
        {
            if (capture.ReceiverWord248 is not null)
            {
                return Stop(
                    capture,
                    SocketSmallStatusOutcome.UnreachedCaptureProvided,
                    PredicateZeroBranchEndRawFileOffset,
                    branchTarget: DiagnosticBlockRawFileOffset,
                    diagnosticPath: SocketSmallStatusDiagnosticPath.PredicateAlZero);
            }

            diagnosticPath = SocketSmallStatusDiagnosticPath.PredicateAlZero;
        }
        else
        {
            if (capture.ReceiverWord248 is not ushort word248)
            {
                if (capture.DiagnosticNormalReturn is not null
                    || capture.SetStateNormalReturn is not null)
                {
                    return Stop(
                        capture,
                        SocketSmallStatusOutcome.UnreachedCaptureProvided,
                        Word248CompareRawFileOffset);
                }

                return Stop(
                    capture,
                    SocketSmallStatusOutcome.MissingReceiverWord248,
                    Word248CompareRawFileOffset);
            }

            if (word248 == 0)
            {
                diagnosticPath = SocketSmallStatusDiagnosticPath.ReceiverWord248Zero;
            }
            else
            {
                if (capture.DiagnosticNormalReturn is not null)
                {
                    return Stop(
                        capture,
                        SocketSmallStatusOutcome.UnreachedCaptureProvided,
                        SetStateBlockRawFileOffset,
                        branchTarget: SetStateBlockRawFileOffset);
                }

                diagnosticPath = SocketSmallStatusDiagnosticPath.ReceiverWord248Nonzero;
            }
        }

        if (diagnosticPath == SocketSmallStatusDiagnosticPath.ReceiverWord248Nonzero)
        {
            return ProjectSetState(capture, diagnosticPath);
        }

        if (capture.SetStateNormalReturn is not null
            && capture.DiagnosticNormalReturn is not true)
        {
            return Stop(
                capture,
                SocketSmallStatusOutcome.UnreachedCaptureProvided,
                DiagnosticCallReturnRawFileOffset,
                diagnosticPath: diagnosticPath);
        }

        if (capture.DiagnosticNormalReturn is not bool diagnosticReturnedNormally)
        {
            return Stop(
                capture,
                SocketSmallStatusOutcome.MissingDiagnosticReturnCapture,
                DiagnosticCallReturnRawFileOffset,
                callRawFileOffset: DiagnosticCallRawFileOffset,
                branchTarget: DiagnosticBlockRawFileOffset,
                diagnosticPath: diagnosticPath);
        }

        if (!diagnosticReturnedNormally)
        {
            return Stop(
                capture,
                SocketSmallStatusOutcome.DiagnosticDidNotReturnNormally,
                DiagnosticCallReturnRawFileOffset,
                callRawFileOffset: DiagnosticCallRawFileOffset,
                branchTarget: DiagnosticBlockRawFileOffset,
                diagnosticPath: diagnosticPath);
        }

        return ProjectSetState(capture, diagnosticPath);
    }

    public static uint AddReceiverOffset(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    public static uint ReceiverDword108Address(uint receiverPointer) =>
        AddReceiverOffset(receiverPointer, ReceiverDword108Offset);

    public static uint PredicateReceiverAddress(uint receiverPointer) =>
        AddReceiverOffset(receiverPointer, PredicateReceiverOffset);

    public static uint ReceiverWord248Address(uint receiverPointer) =>
        AddReceiverOffset(receiverPointer, ReceiverWord248Offset);

    private static SocketSmallStatusPlan ProjectSetState(
        SocketSmallStatusCapture capture,
        SocketSmallStatusDiagnosticPath diagnosticPath)
    {
        if (capture.SetStateNormalReturn is not bool setStateReturnedNormally)
        {
            return Stop(
                capture,
                SocketSmallStatusOutcome.MissingSetStateReturnCapture,
                SetStateCallReturnRawFileOffset,
                callRawFileOffset: SetStateCallRawFileOffset,
                callEcx: capture.ReceiverPointer,
                callStackArgument: SetStateArgument,
                diagnosticPath: diagnosticPath);
        }

        return setStateReturnedNormally
            ? Join(
                capture,
                SocketSmallStatusOutcome.SetStateNormalReturnJoins,
                SetStateJoinBranchEndRawFileOffset,
                callRawFileOffset: SetStateCallRawFileOffset,
                callEcx: capture.ReceiverPointer,
                callStackArgument: SetStateArgument,
                diagnosticPath: diagnosticPath)
            : Stop(
                capture,
                SocketSmallStatusOutcome.SetStateDidNotReturnNormally,
                SetStateCallReturnRawFileOffset,
                callRawFileOffset: SetStateCallRawFileOffset,
                callEcx: capture.ReceiverPointer,
                callStackArgument: SetStateArgument,
                diagnosticPath: diagnosticPath);
    }

    private static bool HasCase2Captures(SocketSmallStatusCapture capture) =>
        capture.Case2NormalReturn is not null;

    private static bool HasCase3Captures(SocketSmallStatusCapture capture) =>
        capture.ReceiverDword108 is not null
        || capture.PredicateAl is not null
        || capture.ReceiverWord248 is not null
        || capture.DiagnosticNormalReturn is not null
        || capture.SetStateNormalReturn is not null;

    private static bool HasAnyStatusCaptures(SocketSmallStatusCapture capture) =>
        HasCase2Captures(capture) || HasCase3Captures(capture);

    private static bool HasPredicateAndLaterCaptures(SocketSmallStatusCapture capture) =>
        capture.PredicateAl is not null
        || capture.ReceiverWord248 is not null
        || capture.DiagnosticNormalReturn is not null
        || capture.SetStateNormalReturn is not null;

    private static SocketSmallStatusPlan Join(
        SocketSmallStatusCapture capture,
        SocketSmallStatusOutcome outcome,
        uint stopAt,
        uint? callRawFileOffset = null,
        uint? callEcx = null,
        uint? callStackArgument = null,
        uint? branchTarget = null,
        SocketSmallStatusDiagnosticPath diagnosticPath = SocketSmallStatusDiagnosticPath.NotReached) =>
        new(capture, outcome, diagnosticPath, callRawFileOffset, callEcx, callStackArgument,
            branchTarget ?? JoinRawFileOffset, stopAt);

    private static SocketSmallStatusPlan Stop(
        SocketSmallStatusCapture capture,
        SocketSmallStatusOutcome outcome,
        uint stopAt,
        uint? callRawFileOffset = null,
        uint? callEcx = null,
        uint? callStackArgument = null,
        uint? branchTarget = null,
        SocketSmallStatusDiagnosticPath diagnosticPath = SocketSmallStatusDiagnosticPath.NotReached) =>
        new(capture, outcome, diagnosticPath, callRawFileOffset, callEcx, callStackArgument,
            branchTarget, stopAt);
}
