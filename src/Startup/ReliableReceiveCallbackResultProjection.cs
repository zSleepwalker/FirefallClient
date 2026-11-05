using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct ReliableReceiveCallbackResultCapture(
    bool? PriorCallReturnedNormally,
    uint? ReturnEax,
    bool? DiagnosticCallReturnedNormally);

public enum ReliableReceiveCallbackResultOutcome
{
    MissingPriorCallReturn,
    UnreachedCaptureProvided,
    PriorCallDidNotReturnNormally,
    MissingReturnEax,
    NonzeroEaxBranch,
    MissingDiagnosticCallReturn,
    DiagnosticCallDidNotReturnNormally,
    ZeroOpaqueHandoffReturnedNormally
}

public readonly record struct ReliableReceiveCallbackResultPlan(
    ReliableReceiveCallbackResultCapture Capture,
    ReliableReceiveCallbackResultOutcome Outcome,
    bool? PriorCallReturnedNormally,
    uint? ReturnEax,
    bool? BranchTaken,
    ImmutableArray<uint> PushedValuesInInstructionOrder,
    bool DiagnosticCallReached,
    uint? DiagnosticCallTargetSlotAddress,
    uint? DiagnosticCallContinuationRawFileOffset,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveCallbackResultInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the callback-result branch and zero-result handoff, without
/// interpreting the opaque diagnostic call or including its following cleanup.
/// </summary>
public static class ReliableReceiveCallbackResultProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE53A2;
    public const uint TestEaxRawFileOffset = EntryRawFileOffset;
    public const uint BranchRawFileOffset = 0x00BE53A4;
    public const uint FirstPushRawFileOffset = 0x00BE53A6;
    public const uint DiagnosticCallRawFileOffset = 0x00BE53B6;
    public const uint DiagnosticCallContinuationRawFileOffset = 0x00BE53BC;
    public const uint NonzeroEaxBranchTargetRawFileOffset = 0x00BE53BF;
    public const uint EndRawFileOffsetExclusive = NonzeroEaxBranchTargetRawFileOffset;

    public const uint DiagnosticCallTargetSlotAddress = 0x01C15EA0;
    public const uint ZeroPushValue = 0;
    public const uint DiagnosticCode = 0x00000205;
    public const uint FirstDiagnosticLiteralPointer = 0x01B58B27;
    public const uint SecondDiagnosticLiteralPointer = 0x01B58BDA;
    public const byte ExplicitStackPushCount = 4;
    public const byte BranchDisplacement = 0x19;

    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint DiagnosticCallVirtualAddress =
        DiagnosticCallRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint DiagnosticCallContinuationVirtualAddress =
        DiagnosticCallContinuationRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint NonzeroEaxBranchTargetVirtualAddress =
        NonzeroEaxBranchTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedWindowBytes =
        "85 C0 75 19 50 68 05 02 00 00 68 27 8B B5 01 68 DA 8B B5 01 FF 15 A0 5E C1 01";

    public static ImmutableArray<ReliableReceiveCallbackResultInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveCallbackResultInstructionEvidence(
                "Test the prior call's EAX result",
                TestEaxRawFileOffset,
                RawFileOffsetToVirtualAddress(TestEaxRawFileOffset),
                "85 C0"),
            new ReliableReceiveCallbackResultInstructionEvidence(
                "A nonzero result branches directly to raw 0xBE53BF",
                BranchRawFileOffset,
                RawFileOffsetToVirtualAddress(BranchRawFileOffset),
                "75 19",
                NonzeroEaxBranchTargetRawFileOffset,
                NonzeroEaxBranchTargetVirtualAddress),
            new ReliableReceiveCallbackResultInstructionEvidence(
                "On the zero-result path, push EAX, 0x205, and the two literal pointers",
                FirstPushRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstPushRawFileOffset),
                "50 68 05 02 00 00 68 27 8B B5 01 68 DA 8B B5 01"),
            new ReliableReceiveCallbackResultInstructionEvidence(
                "Call through the absolute slot at 0x01C15EA0; normal continuation is raw 0xBE53BC",
                DiagnosticCallRawFileOffset,
                RawFileOffsetToVirtualAddress(DiagnosticCallRawFileOffset),
                "FF 15 A0 5E C1 01"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint RelativeBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2u + displacement);

    public static ReliableReceiveCallbackResultPlan Project(
        ReliableReceiveCallbackResultCapture capture)
    {
        if (capture.PriorCallReturnedNormally is not bool priorCallReturnedNormally)
        {
            return Stop(
                capture,
                HasAnyAfterPriorCall(capture)
                    ? ReliableReceiveCallbackResultOutcome.UnreachedCaptureProvided
                    : ReliableReceiveCallbackResultOutcome.MissingPriorCallReturn,
                priorCallReturnedNormally: null,
                returnEax: null,
                stopAt: EntryRawFileOffset);
        }

        if (!priorCallReturnedNormally)
        {
            return Stop(
                capture,
                HasAnyAfterPriorCall(capture)
                    ? ReliableReceiveCallbackResultOutcome.UnreachedCaptureProvided
                    : ReliableReceiveCallbackResultOutcome.PriorCallDidNotReturnNormally,
                priorCallReturnedNormally: false,
                returnEax: null,
                stopAt: EntryRawFileOffset);
        }

        if (capture.ReturnEax is not uint returnEax)
        {
            return Stop(
                capture,
                capture.DiagnosticCallReturnedNormally is not null
                    ? ReliableReceiveCallbackResultOutcome.UnreachedCaptureProvided
                    : ReliableReceiveCallbackResultOutcome.MissingReturnEax,
                priorCallReturnedNormally: true,
                returnEax: null,
                stopAt: TestEaxRawFileOffset);
        }

        if (returnEax != 0)
        {
            return Stop(
                capture,
                capture.DiagnosticCallReturnedNormally is not null
                    ? ReliableReceiveCallbackResultOutcome.UnreachedCaptureProvided
                    : ReliableReceiveCallbackResultOutcome.NonzeroEaxBranch,
                priorCallReturnedNormally: true,
                returnEax: returnEax,
                branchTaken: true,
                selectedDestinationRawFileOffset: NonzeroEaxBranchTargetRawFileOffset,
                stopAt: NonzeroEaxBranchTargetRawFileOffset);
        }

        if (capture.DiagnosticCallReturnedNormally is not bool diagnosticCallReturnedNormally)
        {
            return Stop(
                capture,
                ReliableReceiveCallbackResultOutcome.MissingDiagnosticCallReturn,
                priorCallReturnedNormally: true,
                returnEax: returnEax,
                branchTaken: false,
                diagnosticCallReached: true,
                stopAt: DiagnosticCallContinuationRawFileOffset);
        }

        if (!diagnosticCallReturnedNormally)
        {
            return Stop(
                capture,
                ReliableReceiveCallbackResultOutcome.DiagnosticCallDidNotReturnNormally,
                priorCallReturnedNormally: true,
                returnEax: returnEax,
                branchTaken: false,
                diagnosticCallReached: true,
                stopAt: DiagnosticCallContinuationRawFileOffset);
        }

        return Stop(
            capture,
            ReliableReceiveCallbackResultOutcome.ZeroOpaqueHandoffReturnedNormally,
            priorCallReturnedNormally: true,
            returnEax: returnEax,
            branchTaken: false,
            diagnosticCallReached: true,
            selectedDestinationRawFileOffset: DiagnosticCallContinuationRawFileOffset,
            stopAt: DiagnosticCallContinuationRawFileOffset);
    }

    private static bool HasAnyAfterPriorCall(
        ReliableReceiveCallbackResultCapture capture) =>
        capture.ReturnEax is not null
        || capture.DiagnosticCallReturnedNormally is not null;

    private static ReliableReceiveCallbackResultPlan Stop(
        ReliableReceiveCallbackResultCapture capture,
        ReliableReceiveCallbackResultOutcome outcome,
        bool? priorCallReturnedNormally,
        uint? returnEax,
        bool? branchTaken = null,
        bool diagnosticCallReached = false,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = EntryRawFileOffset)
    {
        ImmutableArray<uint> pushes = branchTaken == false
            ? ImmutableArray.Create(
                ZeroPushValue,
                DiagnosticCode,
                FirstDiagnosticLiteralPointer,
                SecondDiagnosticLiteralPointer)
            : ImmutableArray<uint>.Empty;

        return new ReliableReceiveCallbackResultPlan(
            capture,
            outcome,
            priorCallReturnedNormally,
            returnEax,
            branchTaken,
            pushes,
            diagnosticCallReached,
            diagnosticCallReached ? DiagnosticCallTargetSlotAddress : null,
            diagnosticCallReached ? DiagnosticCallContinuationRawFileOffset : null,
            selectedDestinationRawFileOffset,
            stopAt);
    }
}
