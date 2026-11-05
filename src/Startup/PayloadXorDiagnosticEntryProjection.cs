using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

/// <summary>
/// FramePointer is the explicitly captured EBP after this entry's prologue.
/// </summary>
public readonly record struct PayloadXorDiagnosticEntryCapture(
    byte MarkerByte,
    uint EdxDword,
    uint FramePointer,
    bool? DiagnosticReturnedNormally,
    byte? PostDiagnosticFrameMinus1Reload);

public enum PayloadXorDiagnosticEntryOutcome
{
    DiagnosticSkippedAtBoundary,
    DiagnosticReturnStatusNotCaptured,
    DiagnosticDidNotReturn,
    MissingPostDiagnosticFrameMinus1Reload,
    UnreachedCaptureProvided,
    DiagnosticReloadedAndStackCleaned
}

public enum PayloadXorDiagnosticEntryActionKind
{
    CopyClToAl,
    StoreAlAtFrameMinus1,
    SaveEbx,
    CopyEdxToEbx,
    SaveEsi,
    TestAl,
    BranchZeroToDiagnostic,
    CompareAlToFour,
    BranchBelowFourToBoundary,
    MarkerGateFallthrough,
    PushDiagnosticZero,
    PushDiagnosticLine,
    PushDiagnosticMessagePointer,
    PushDiagnosticSourcePointer,
    CallDiagnostic,
    DiagnosticReturnedNormally,
    ReloadAlFromFrameMinus1,
    CleanupDiagnosticArguments,
    StopBeforeStackLengthRead
}

public readonly record struct PayloadXorDiagnosticEntryAction(
    PayloadXorDiagnosticEntryActionKind Kind,
    uint RawFileOffset);

public readonly record struct PayloadXorDiagnosticEntryPlan(
    PayloadXorDiagnosticEntryCapture Capture,
    PayloadXorDiagnosticEntryOutcome Outcome,
    byte AlAfterClCopy,
    byte MarkerStoredAtFrameMinus1,
    uint FrameMinus1Address,
    uint RetainedEbxDword,
    bool DiagnosticCallReached,
    bool PostDiagnosticReloadRead,
    byte? AlAtBoundary,
    uint? DiagnosticArgumentStackCleanupBytes,
    uint? RouteRawFileOffset,
    uint StopAtRawFileOffsetExclusive,
    PayloadXorDiagnosticEntryAction[] Actions);

public readonly record struct PayloadXorDiagnosticEntryInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null,
    uint? AbsoluteVirtualAddress = null);

/// <summary>
/// Projects the marker gate and diagnostic-call prelude at raw 0xBD58E0..0xBD5912.
/// The opaque diagnostic call is modeled only through its normal-return capture and
/// the independent byte reload from the explicit EBP-based frame capture.
/// </summary>
public static class PayloadXorDiagnosticEntryProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryStartRawFileOffset = 0x00BD58E0;
    public const uint EntryEndRawFileOffsetExclusive = 0x00BD5912;
    public const uint CapturedCodeLength =
        EntryEndRawFileOffsetExclusive - EntryStartRawFileOffset;

    public const uint ClToAlRawFileOffset = 0x00BD58E4;
    public const uint StoreAlAtFrameMinus1RawFileOffset = 0x00BD58E6;
    public const uint SaveEbxRawFileOffset = 0x00BD58E9;
    public const uint CopyEdxToEbxRawFileOffset = 0x00BD58EA;
    public const uint TestAlRawFileOffset = 0x00BD58ED;
    public const uint ZeroMarkerBranchRawFileOffset = 0x00BD58EF;
    public const uint CompareAlToFourRawFileOffset = 0x00BD58F1;
    public const uint BelowFourBranchRawFileOffset = 0x00BD58F3;
    public const uint DiagnosticStartRawFileOffset = 0x00BD58F5;
    public const uint DiagnosticCallRawFileOffset = 0x00BD5906;
    public const uint DiagnosticNormalReturnRawFileOffset = 0x00BD590C;
    public const uint FrameMinus1ReloadRawFileOffset = 0x00BD590C;
    public const uint DiagnosticStackCleanupRawFileOffset = 0x00BD590F;
    public const uint DiagnosticBoundaryRawFileOffsetExclusive = 0x00BD5912;
    public const uint DiagnosticCallTargetVirtualAddress = 0x01C15EA0;
    public const uint DiagnosticArgumentZero = 0;
    public const uint DiagnosticArgumentLine = 0x70F;
    public const uint DiagnosticArgumentMessagePointer = 0x1B585FB;
    public const uint DiagnosticArgumentSourcePointer = 0x1B58601;
    public const uint DiagnosticStackCleanupBytes = 0x10;

    public const string CapturedCodeBytes =
        "55 8B EC 51 8A C1 88 45 FF 53 8B DA 56 84 C0 74 04 3C 04 72 1D "
        + "6A 00 68 0F 07 00 00 68 FB 85 B5 01 68 01 86 B5 01 "
        + "FF 15 A0 5E C1 01 8A 45 FF 83 C4 10";

    public static uint EntryStartVirtualAddress =>
        RawFileOffsetToVirtualAddress(EntryStartRawFileOffset);

    public static uint EntryEndVirtualAddressExclusive =>
        RawFileOffsetToVirtualAddress(EntryEndRawFileOffsetExclusive);

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FrameMinus1Address(uint framePointer) =>
        unchecked(framePointer - 1);

    public static ReadOnlyCollection<PayloadXorDiagnosticEntryInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Establish the frame pointer",
                EntryStartRawFileOffset,
                RawFileOffsetToVirtualAddress(EntryStartRawFileOffset),
                "55 8B EC"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Push ECX to reserve four stack bytes",
                0x00BD58E3,
                RawFileOffsetToVirtualAddress(0x00BD58E3),
                "51"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Copy incoming CL into AL",
                ClToAlRawFileOffset,
                RawFileOffsetToVirtualAddress(ClToAlRawFileOffset),
                "8A C1"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Store the incoming marker byte at EBP-1 before the opaque diagnostic call",
                StoreAlAtFrameMinus1RawFileOffset,
                RawFileOffsetToVirtualAddress(StoreAlAtFrameMinus1RawFileOffset),
                "88 45 FF"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Save EBX",
                SaveEbxRawFileOffset,
                RawFileOffsetToVirtualAddress(SaveEbxRawFileOffset),
                "53"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Retain the incoming EDX DWORD in EBX",
                CopyEdxToEbxRawFileOffset,
                RawFileOffsetToVirtualAddress(CopyEdxToEbxRawFileOffset),
                "8B DA"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Save ESI",
                0x00BD58EC,
                RawFileOffsetToVirtualAddress(0x00BD58EC),
                "56"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Test AL; zero branches directly to the diagnostic call setup",
                TestAlRawFileOffset,
                RawFileOffsetToVirtualAddress(TestAlRawFileOffset),
                "84 C0"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "JZ reaches the diagnostic path for marker zero",
                ZeroMarkerBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroMarkerBranchRawFileOffset),
                "74 04",
                DiagnosticStartRawFileOffset,
                RawFileOffsetToVirtualAddress(DiagnosticStartRawFileOffset)),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Compare nonzero AL against four",
                CompareAlToFourRawFileOffset,
                RawFileOffsetToVirtualAddress(CompareAlToFourRawFileOffset),
                "3C 04"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "JB skips the diagnostic path for markers 1..3 and reaches the boundary",
                BelowFourBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(BelowFourBranchRawFileOffset),
                "72 1D",
                DiagnosticBoundaryRawFileOffsetExclusive,
                RawFileOffsetToVirtualAddress(DiagnosticBoundaryRawFileOffsetExclusive)),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Push diagnostic argument zero",
                DiagnosticStartRawFileOffset,
                RawFileOffsetToVirtualAddress(DiagnosticStartRawFileOffset),
                "6A 00"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Push diagnostic line 0x70F",
                0x00BD58F7,
                RawFileOffsetToVirtualAddress(0x00BD58F7),
                "68 0F 07 00 00"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Push diagnostic message pointer",
                0x00BD58FC,
                RawFileOffsetToVirtualAddress(0x00BD58FC),
                "68 FB 85 B5 01"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Push diagnostic source pointer",
                0x00BD5901,
                RawFileOffsetToVirtualAddress(0x00BD5901),
                "68 01 86 B5 01"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Call through absolute pointer cell VA 0x01C15EA0",
                DiagnosticCallRawFileOffset,
                RawFileOffsetToVirtualAddress(DiagnosticCallRawFileOffset),
                "FF 15 A0 5E C1 01",
                AbsoluteVirtualAddress: DiagnosticCallTargetVirtualAddress),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "After normal return, independently reload AL from EBP-1",
                FrameMinus1ReloadRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameMinus1ReloadRawFileOffset),
                "8A 45 FF"),
            new PayloadXorDiagnosticEntryInstructionEvidence(
                "Clean up the four diagnostic arguments",
                DiagnosticStackCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(DiagnosticStackCleanupRawFileOffset),
                "83 C4 10")
        });

    public static PayloadXorDiagnosticEntryPlan Project(
        PayloadXorDiagnosticEntryCapture capture)
    {
        var actions = new List<PayloadXorDiagnosticEntryAction>
        {
            Action(PayloadXorDiagnosticEntryActionKind.CopyClToAl, ClToAlRawFileOffset),
            Action(PayloadXorDiagnosticEntryActionKind.StoreAlAtFrameMinus1, StoreAlAtFrameMinus1RawFileOffset),
            Action(PayloadXorDiagnosticEntryActionKind.SaveEbx, SaveEbxRawFileOffset),
            Action(PayloadXorDiagnosticEntryActionKind.CopyEdxToEbx, CopyEdxToEbxRawFileOffset),
            Action(PayloadXorDiagnosticEntryActionKind.SaveEsi, 0x00BD58EC),
            Action(PayloadXorDiagnosticEntryActionKind.TestAl, TestAlRawFileOffset)
        };
        uint frameMinus1Address = FrameMinus1Address(capture.FramePointer);

        if (capture.MarkerByte == 0)
        {
            actions.Add(Action(
                PayloadXorDiagnosticEntryActionKind.BranchZeroToDiagnostic,
                ZeroMarkerBranchRawFileOffset));
        }
        else
        {
            actions.Add(Action(
                PayloadXorDiagnosticEntryActionKind.CompareAlToFour,
                CompareAlToFourRawFileOffset));
            if (capture.MarkerByte < 4)
            {
                actions.Add(Action(
                    PayloadXorDiagnosticEntryActionKind.BranchBelowFourToBoundary,
                    BelowFourBranchRawFileOffset));
                actions.Add(Action(
                    PayloadXorDiagnosticEntryActionKind.StopBeforeStackLengthRead,
                    DiagnosticBoundaryRawFileOffsetExclusive));
                bool hasUnreachedCapture =
                    capture.DiagnosticReturnedNormally is not null
                    || capture.PostDiagnosticFrameMinus1Reload is not null;
                return Plan(
                    capture,
                    hasUnreachedCapture
                        ? PayloadXorDiagnosticEntryOutcome.UnreachedCaptureProvided
                        : PayloadXorDiagnosticEntryOutcome.DiagnosticSkippedAtBoundary,
                    frameMinus1Address,
                    diagnosticCallReached: false,
                    postDiagnosticReloadRead: false,
                    alAtBoundary: hasUnreachedCapture ? null : capture.MarkerByte,
                    diagnosticArgumentStackCleanupBytes: 0,
                    routeRawFileOffset: DiagnosticBoundaryRawFileOffsetExclusive,
                    stopAtRawFileOffsetExclusive: DiagnosticBoundaryRawFileOffsetExclusive,
                    actions);
            }

            actions.Add(Action(
                PayloadXorDiagnosticEntryActionKind.MarkerGateFallthrough,
                BelowFourBranchRawFileOffset));
        }

        actions.Add(Action(
            PayloadXorDiagnosticEntryActionKind.PushDiagnosticZero,
            DiagnosticStartRawFileOffset));
        actions.Add(Action(
            PayloadXorDiagnosticEntryActionKind.PushDiagnosticLine,
            0x00BD58F7));
        actions.Add(Action(
            PayloadXorDiagnosticEntryActionKind.PushDiagnosticMessagePointer,
            0x00BD58FC));
        actions.Add(Action(
            PayloadXorDiagnosticEntryActionKind.PushDiagnosticSourcePointer,
            0x00BD5901));
        actions.Add(Action(
            PayloadXorDiagnosticEntryActionKind.CallDiagnostic,
            DiagnosticCallRawFileOffset));

        if (capture.DiagnosticReturnedNormally is not bool returnedNormally)
        {
            return Plan(
                capture,
                capture.PostDiagnosticFrameMinus1Reload is null
                    ? PayloadXorDiagnosticEntryOutcome.DiagnosticReturnStatusNotCaptured
                    : PayloadXorDiagnosticEntryOutcome.UnreachedCaptureProvided,
                frameMinus1Address,
                diagnosticCallReached: true,
                postDiagnosticReloadRead: false,
                alAtBoundary: null,
                diagnosticArgumentStackCleanupBytes: null,
                routeRawFileOffset: null,
                stopAtRawFileOffsetExclusive: DiagnosticCallRawFileOffset,
                actions);
        }

        if (!returnedNormally)
        {
            return Plan(
                capture,
                capture.PostDiagnosticFrameMinus1Reload is null
                    ? PayloadXorDiagnosticEntryOutcome.DiagnosticDidNotReturn
                    : PayloadXorDiagnosticEntryOutcome.UnreachedCaptureProvided,
                frameMinus1Address,
                diagnosticCallReached: true,
                postDiagnosticReloadRead: false,
                alAtBoundary: null,
                diagnosticArgumentStackCleanupBytes: null,
                routeRawFileOffset: null,
                stopAtRawFileOffsetExclusive: DiagnosticCallRawFileOffset,
                actions);
        }

        actions.Add(Action(
            PayloadXorDiagnosticEntryActionKind.DiagnosticReturnedNormally,
            DiagnosticNormalReturnRawFileOffset));
        if (capture.PostDiagnosticFrameMinus1Reload is not byte reloadedAl)
        {
            return Plan(
                capture,
                PayloadXorDiagnosticEntryOutcome.MissingPostDiagnosticFrameMinus1Reload,
                frameMinus1Address,
                diagnosticCallReached: true,
                postDiagnosticReloadRead: false,
                alAtBoundary: null,
                diagnosticArgumentStackCleanupBytes: null,
                routeRawFileOffset: null,
                stopAtRawFileOffsetExclusive: FrameMinus1ReloadRawFileOffset,
                actions);
        }

        actions.Add(Action(
            PayloadXorDiagnosticEntryActionKind.ReloadAlFromFrameMinus1,
            FrameMinus1ReloadRawFileOffset));
        actions.Add(Action(
            PayloadXorDiagnosticEntryActionKind.CleanupDiagnosticArguments,
            DiagnosticStackCleanupRawFileOffset));
        actions.Add(Action(
            PayloadXorDiagnosticEntryActionKind.StopBeforeStackLengthRead,
            DiagnosticBoundaryRawFileOffsetExclusive));
        return Plan(
            capture,
            PayloadXorDiagnosticEntryOutcome.DiagnosticReloadedAndStackCleaned,
            frameMinus1Address,
            diagnosticCallReached: true,
            postDiagnosticReloadRead: true,
            alAtBoundary: reloadedAl,
            diagnosticArgumentStackCleanupBytes: DiagnosticStackCleanupBytes,
            routeRawFileOffset: DiagnosticBoundaryRawFileOffsetExclusive,
            stopAtRawFileOffsetExclusive: DiagnosticBoundaryRawFileOffsetExclusive,
            actions);
    }

    private static PayloadXorDiagnosticEntryAction Action(
        PayloadXorDiagnosticEntryActionKind kind,
        uint rawFileOffset) =>
        new(kind, rawFileOffset);

    private static PayloadXorDiagnosticEntryPlan Plan(
        PayloadXorDiagnosticEntryCapture capture,
        PayloadXorDiagnosticEntryOutcome outcome,
        uint frameMinus1Address,
        bool diagnosticCallReached,
        bool postDiagnosticReloadRead,
        byte? alAtBoundary,
        uint? diagnosticArgumentStackCleanupBytes,
        uint? routeRawFileOffset,
        uint stopAtRawFileOffsetExclusive,
        List<PayloadXorDiagnosticEntryAction> actions) =>
        new(
            capture,
            outcome,
            capture.MarkerByte,
            capture.MarkerByte,
            frameMinus1Address,
            capture.EdxDword,
            diagnosticCallReached,
            postDiagnosticReloadRead,
            alAtBoundary,
            diagnosticArgumentStackCleanupBytes,
            routeRawFileOffset,
            stopAtRawFileOffsetExclusive,
            actions.ToArray());
}
