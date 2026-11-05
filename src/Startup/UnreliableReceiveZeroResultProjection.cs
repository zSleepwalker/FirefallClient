using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveZeroResultCapture(
    uint RetainedEsi,
    uint ReceiverEdi,
    bool? PriorRecycleHelperReturnedNormally,
    uint? ReceiverDwordAtOffset00,
    uint? ReceiverDwordAtOffset04,
    uint? LoadedSlotDwordAtOffset04,
    uint? ReceiverDwordAtOffset18,
    bool? HandoffHelperReturnedNormally);

public enum UnreliableReceiveZeroResultOutcome
{
    MissingPriorRecycleHelperReturnCapture,
    PriorRecycleHelperDidNotReturnNormally,
    NonzeroEsiBranch,
    MissingReceiverOffset00Capture,
    MissingReceiverOffset00CaptureWithUnreachedLaterCaptures,
    MissingReceiverOffset04Capture,
    MissingReceiverOffset04CaptureWithUnreachedLaterCaptures,
    MissingLoadedSlotOffset04Capture,
    MissingLoadedSlotOffset04CaptureWithUnreachedLaterCaptures,
    MissingReceiverOffset18Capture,
    MissingReceiverOffset18CaptureWithUnreachedHandoffReturnCapture,
    MissingHandoffHelperReturnCapture,
    HandoffHelperDidNotReturnNormally,
    HandoffReturnedToExclusiveBoundary
}

public enum UnreliableReceiveZeroResultPushKind
{
    ReceiverDwordAtOffset04,
    SelectedEcx,
    LiteralArgument,
    FourArgument,
    ReceiverDwordAtOffset18
}

public readonly record struct UnreliableReceiveZeroResultPlan(
    UnreliableReceiveZeroResultCapture Capture,
    UnreliableReceiveZeroResultOutcome Outcome,
    bool? PriorRecycleHelperReturnedNormally,
    bool? EsiIsZero,
    bool? NonzeroEsiBranchTaken,
    uint ReceiverDwordAtOffset00Address,
    uint ReceiverDwordAtOffset04Address,
    uint? LoadedSlotDwordAtOffset04Address,
    uint ReceiverDwordAtOffset18Address,
    bool ReceiverDwordAtOffset00Read,
    bool ReceiverDwordAtOffset04Read,
    bool LoadedSlotDwordAtOffset04Read,
    bool ReceiverDwordAtOffset18Read,
    uint? SelectedEcxValue,
    bool UnreachedReceiverDwordAtOffset00CaptureProvided,
    bool UnreachedReceiverDwordAtOffset04CaptureProvided,
    bool UnreachedLoadedSlotDwordAtOffset04CaptureProvided,
    bool UnreachedReceiverDwordAtOffset18CaptureProvided,
    bool UnreachedHandoffReturnCaptureProvided,
    ImmutableArray<uint> ArgumentsPushedInInstructionOrder,
    bool HandoffCallReached,
    bool? HandoffHelperReturnedNormally,
    bool? HandoffReturnBoundaryReached,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive)
{
    public bool CanComposeCompletedHandoff =>
        PriorRecycleHelperReturnedNormally == true
        && NonzeroEsiBranchTaken == false
        && !UnreachedReceiverDwordAtOffset00CaptureProvided
        && !UnreachedReceiverDwordAtOffset04CaptureProvided
        && !UnreachedLoadedSlotDwordAtOffset04CaptureProvided
        && !UnreachedReceiverDwordAtOffset18CaptureProvided
        && !UnreachedHandoffReturnCaptureProvided
        && HandoffCallReached
        && HandoffReturnBoundaryReached == true;
}

public readonly record struct UnreliableReceiveZeroResultInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the zero-result receive handoff through the helper return boundary only.
/// </summary>
public static class UnreliableReceiveZeroResultProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint HandoffStartRawFileOffset = 0x00BE5B78;
    public const uint HandoffStartVirtualAddress = 0x00FE6778;
    public const uint HandoffEndRawFileOffsetExclusive = 0x00BE5B9E;
    public const uint HandoffEndVirtualAddressExclusive = 0x00FE679E;

    public const uint EsiTestRawFileOffset = 0x00BE5B78;
    public const uint NonzeroEsiBranchRawFileOffset = 0x00BE5B7A;
    public const uint NonzeroEsiBranchTargetRawFileOffset = 0x00BE5BAC;
    public const uint ReceiverDwordAtOffset00 = 0x00;
    public const uint ReceiverDwordAtOffset04 = 0x04;
    public const uint ReceiverDwordAtOffset18 = 0x18;
    public const uint ReceiverDwordAtOffset00ReadRawFileOffset = 0x00BE5B7C;
    public const uint ReceiverDwordAtOffset04PushRawFileOffset = 0x00BE5B83;
    public const uint LoadedSlotDwordAtOffset04ReadRawFileOffset = 0x00BE5B86;
    public const uint ReceiverDwordAtOffset18PushRawFileOffset = 0x00BE5B96;
    public const uint HandoffCallRawFileOffset = 0x00BE5B99;
    public const uint HandoffCallReturnRawFileOffset = 0x00BE5B9E;
    public const uint HandoffCallTargetRawFileOffset = 0x00BE2F50;
    public const uint HandoffCallTargetVirtualAddress = 0x00FE3B50;

    public const uint InitialEcxLiteral = 0x01C5EA68;
    public const uint HandoffLiteralArgument = 0x01BB8E7C;
    public const uint FourArgument = 4;
    public const uint ArgumentCount = 5;

    public const string CapturedHandoffBytes =
        "85 F6 75 30 8B 07 B9 68 EA C5 01 FF 77 04 8B 40 04 "
        + "85 C0 0F 45 C8 51 68 7C 8E BB 01 6A 04 FF 77 18 "
        + "E8 B2 D3 FF FF";

    public static ImmutableArray<UnreliableReceiveZeroResultPushKind> PushKindsInInstructionOrder { get; } =
        ImmutableArray.Create(
            UnreliableReceiveZeroResultPushKind.ReceiverDwordAtOffset04,
            UnreliableReceiveZeroResultPushKind.SelectedEcx,
            UnreliableReceiveZeroResultPushKind.LiteralArgument,
            UnreliableReceiveZeroResultPushKind.FourArgument,
            UnreliableReceiveZeroResultPushKind.ReceiverDwordAtOffset18);

    public static ImmutableArray<UnreliableReceiveZeroResultInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveZeroResultInstructionEvidence(
                "TEST ESI,ESI; a nonzero result branches to raw 0xBE5BAC",
                EsiTestRawFileOffset,
                RawFileOffsetToVirtualAddress(EsiTestRawFileOffset),
                "85 F6 75 30",
                NonzeroEsiBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(NonzeroEsiBranchTargetRawFileOffset)),
            new UnreliableReceiveZeroResultInstructionEvidence(
                "Read the DWORD at receiver+0 into EAX without a zero-pointer guard",
                ReceiverDwordAtOffset00ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDwordAtOffset00ReadRawFileOffset),
                "8B 07"),
            new UnreliableReceiveZeroResultInstructionEvidence(
                "Load literal 0x1C5EA68 into ECX, then independently read and push receiver+4",
                0x00BE5B7E,
                RawFileOffsetToVirtualAddress(0x00BE5B7E),
                "B9 68 EA C5 01 FF 77 04"),
            new UnreliableReceiveZeroResultInstructionEvidence(
                "Read the DWORD at the first loaded value+4; CMOVNE replaces ECX only when nonzero",
                LoadedSlotDwordAtOffset04ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(LoadedSlotDwordAtOffset04ReadRawFileOffset),
                "8B 40 04 85 C0 0F 45 C8"),
            new UnreliableReceiveZeroResultInstructionEvidence(
                "Push selected ECX, literal 0x1BB8E7C, four, and receiver+0x18 in instruction order",
                0x00BE5B8E,
                RawFileOffsetToVirtualAddress(0x00BE5B8E),
                "51 68 7C 8E BB 01 6A 04 FF 77 18"),
            new UnreliableReceiveZeroResultInstructionEvidence(
                "Call raw 0xBE2F50; normal return reaches exclusive raw 0xBE5B9E",
                HandoffCallRawFileOffset,
                RawFileOffsetToVirtualAddress(HandoffCallRawFileOffset),
                "E8 B2 D3 FF FF",
                HandoffCallTargetRawFileOffset,
                HandoffCallTargetVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAtOffset00Address(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverDwordAtOffset00);

    public static uint ReceiverDwordAtOffset04Address(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverDwordAtOffset04);

    public static uint ReceiverDwordAtOffset18Address(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverDwordAtOffset18);

    public static uint LoadedSlotDwordAtOffset04Address(uint loadedSlot) =>
        unchecked(loadedSlot + ReceiverDwordAtOffset04);

    public static uint RelativeCallTarget(uint instructionRawFileOffset, int displacement) =>
        unchecked(instructionRawFileOffset + 5u + (uint)displacement);

    public static UnreliableReceiveZeroResultPlan Project(
        UnreliableReceiveZeroResultCapture capture)
    {
        uint receiverDwordAtOffset00Address =
            ReceiverDwordAtOffset00Address(capture.ReceiverEdi);
        uint receiverDwordAtOffset04Address =
            ReceiverDwordAtOffset04Address(capture.ReceiverEdi);
        uint receiverDwordAtOffset18Address =
            ReceiverDwordAtOffset18Address(capture.ReceiverEdi);

        if (capture.PriorRecycleHelperReturnedNormally is not bool priorHelperReturnedNormally)
        {
            return Plan(
                capture,
                UnreliableReceiveZeroResultOutcome.MissingPriorRecycleHelperReturnCapture,
                priorRecycleHelperReturnedNormally: null,
                esiIsZero: null,
                receiverDwordAtOffset00Address: receiverDwordAtOffset00Address,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                loadedSlotDwordAtOffset04Address: null,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset00Read: false,
                receiverDwordAtOffset04Read: false,
                loadedSlotDwordAtOffset04Read: false,
                receiverDwordAtOffset18Read: false,
                selectedEcxValue: null,
                unreachedReceiverDwordAtOffset00CaptureProvided: capture.ReceiverDwordAtOffset00.HasValue,
                unreachedReceiverDwordAtOffset04CaptureProvided: capture.ReceiverDwordAtOffset04.HasValue,
                unreachedLoadedSlotDwordAtOffset04CaptureProvided: capture.LoadedSlotDwordAtOffset04.HasValue,
                unreachedReceiverDwordAtOffset18CaptureProvided: capture.ReceiverDwordAtOffset18.HasValue,
                unreachedHandoffReturnCaptureProvided: capture.HandoffHelperReturnedNormally.HasValue,
                argumentsPushedInInstructionOrder: ImmutableArray<uint>.Empty,
                handoffCallReached: false,
                handoffHelperReturnedNormally: null,
                handoffReturnBoundaryReached: null,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: EsiTestRawFileOffset);
        }

        if (!priorHelperReturnedNormally)
        {
            return Plan(
                capture,
                UnreliableReceiveZeroResultOutcome.PriorRecycleHelperDidNotReturnNormally,
                priorRecycleHelperReturnedNormally: false,
                esiIsZero: null,
                receiverDwordAtOffset00Address: receiverDwordAtOffset00Address,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                loadedSlotDwordAtOffset04Address: null,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset00Read: false,
                receiverDwordAtOffset04Read: false,
                loadedSlotDwordAtOffset04Read: false,
                receiverDwordAtOffset18Read: false,
                selectedEcxValue: null,
                unreachedReceiverDwordAtOffset00CaptureProvided: capture.ReceiverDwordAtOffset00.HasValue,
                unreachedReceiverDwordAtOffset04CaptureProvided: capture.ReceiverDwordAtOffset04.HasValue,
                unreachedLoadedSlotDwordAtOffset04CaptureProvided: capture.LoadedSlotDwordAtOffset04.HasValue,
                unreachedReceiverDwordAtOffset18CaptureProvided: capture.ReceiverDwordAtOffset18.HasValue,
                unreachedHandoffReturnCaptureProvided: capture.HandoffHelperReturnedNormally.HasValue,
                argumentsPushedInInstructionOrder: ImmutableArray<uint>.Empty,
                handoffCallReached: false,
                handoffHelperReturnedNormally: null,
                handoffReturnBoundaryReached: null,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: EsiTestRawFileOffset);
        }

        if (capture.RetainedEsi != 0)
        {
            return Plan(
                capture,
                UnreliableReceiveZeroResultOutcome.NonzeroEsiBranch,
                priorRecycleHelperReturnedNormally: true,
                esiIsZero: false,
                receiverDwordAtOffset00Address: receiverDwordAtOffset00Address,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                loadedSlotDwordAtOffset04Address: null,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset00Read: false,
                receiverDwordAtOffset04Read: false,
                loadedSlotDwordAtOffset04Read: false,
                receiverDwordAtOffset18Read: false,
                selectedEcxValue: null,
                unreachedReceiverDwordAtOffset00CaptureProvided: capture.ReceiverDwordAtOffset00.HasValue,
                unreachedReceiverDwordAtOffset04CaptureProvided: capture.ReceiverDwordAtOffset04.HasValue,
                unreachedLoadedSlotDwordAtOffset04CaptureProvided: capture.LoadedSlotDwordAtOffset04.HasValue,
                unreachedReceiverDwordAtOffset18CaptureProvided: capture.ReceiverDwordAtOffset18.HasValue,
                unreachedHandoffReturnCaptureProvided: capture.HandoffHelperReturnedNormally.HasValue,
                argumentsPushedInInstructionOrder: ImmutableArray<uint>.Empty,
                handoffCallReached: false,
                handoffHelperReturnedNormally: null,
                handoffReturnBoundaryReached: null,
                selectedDestinationRawFileOffset: NonzeroEsiBranchTargetRawFileOffset,
                stopAtRawFileOffsetExclusive: NonzeroEsiBranchTargetRawFileOffset);
        }

        if (capture.ReceiverDwordAtOffset00 is not uint receiverDwordAtOffset00)
        {
            bool laterCaptureProvided =
                capture.ReceiverDwordAtOffset04.HasValue
                || capture.LoadedSlotDwordAtOffset04.HasValue
                || capture.ReceiverDwordAtOffset18.HasValue
                || capture.HandoffHelperReturnedNormally.HasValue;
            return Plan(
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveZeroResultOutcome.MissingReceiverOffset00CaptureWithUnreachedLaterCaptures
                    : UnreliableReceiveZeroResultOutcome.MissingReceiverOffset00Capture,
                priorRecycleHelperReturnedNormally: true,
                esiIsZero: true,
                receiverDwordAtOffset00Address: receiverDwordAtOffset00Address,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                loadedSlotDwordAtOffset04Address: null,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset00Read: false,
                receiverDwordAtOffset04Read: false,
                loadedSlotDwordAtOffset04Read: false,
                receiverDwordAtOffset18Read: false,
                selectedEcxValue: null,
                unreachedReceiverDwordAtOffset00CaptureProvided: false,
                unreachedReceiverDwordAtOffset04CaptureProvided: capture.ReceiverDwordAtOffset04.HasValue,
                unreachedLoadedSlotDwordAtOffset04CaptureProvided: capture.LoadedSlotDwordAtOffset04.HasValue,
                unreachedReceiverDwordAtOffset18CaptureProvided: capture.ReceiverDwordAtOffset18.HasValue,
                unreachedHandoffReturnCaptureProvided: capture.HandoffHelperReturnedNormally.HasValue,
                argumentsPushedInInstructionOrder: ImmutableArray<uint>.Empty,
                handoffCallReached: false,
                handoffHelperReturnedNormally: null,
                handoffReturnBoundaryReached: null,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: ReceiverDwordAtOffset00ReadRawFileOffset);
        }

        uint? loadedSlotDwordAtOffset04Address =
            LoadedSlotDwordAtOffset04Address(receiverDwordAtOffset00);

        if (capture.ReceiverDwordAtOffset04 is not uint receiverDwordAtOffset04)
        {
            bool laterCaptureProvided =
                capture.LoadedSlotDwordAtOffset04.HasValue
                || capture.ReceiverDwordAtOffset18.HasValue
                || capture.HandoffHelperReturnedNormally.HasValue;
            return Plan(
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveZeroResultOutcome.MissingReceiverOffset04CaptureWithUnreachedLaterCaptures
                    : UnreliableReceiveZeroResultOutcome.MissingReceiverOffset04Capture,
                priorRecycleHelperReturnedNormally: true,
                esiIsZero: true,
                receiverDwordAtOffset00Address: receiverDwordAtOffset00Address,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                loadedSlotDwordAtOffset04Address: loadedSlotDwordAtOffset04Address,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset00Read: true,
                receiverDwordAtOffset04Read: false,
                loadedSlotDwordAtOffset04Read: false,
                receiverDwordAtOffset18Read: false,
                selectedEcxValue: null,
                unreachedReceiverDwordAtOffset00CaptureProvided: false,
                unreachedReceiverDwordAtOffset04CaptureProvided: false,
                unreachedLoadedSlotDwordAtOffset04CaptureProvided: capture.LoadedSlotDwordAtOffset04.HasValue,
                unreachedReceiverDwordAtOffset18CaptureProvided: capture.ReceiverDwordAtOffset18.HasValue,
                unreachedHandoffReturnCaptureProvided: capture.HandoffHelperReturnedNormally.HasValue,
                argumentsPushedInInstructionOrder: ImmutableArray<uint>.Empty,
                handoffCallReached: false,
                handoffHelperReturnedNormally: null,
                handoffReturnBoundaryReached: null,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: ReceiverDwordAtOffset04PushRawFileOffset);
        }

        var pushedArguments = ImmutableArray.CreateBuilder<uint>((int)ArgumentCount);
        pushedArguments.Add(receiverDwordAtOffset04);

        if (capture.LoadedSlotDwordAtOffset04 is not uint loadedSlotDwordAtOffset04)
        {
            bool laterCaptureProvided =
                capture.ReceiverDwordAtOffset18.HasValue
                || capture.HandoffHelperReturnedNormally.HasValue;
            return Plan(
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveZeroResultOutcome.MissingLoadedSlotOffset04CaptureWithUnreachedLaterCaptures
                    : UnreliableReceiveZeroResultOutcome.MissingLoadedSlotOffset04Capture,
                priorRecycleHelperReturnedNormally: true,
                esiIsZero: true,
                receiverDwordAtOffset00Address: receiverDwordAtOffset00Address,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                loadedSlotDwordAtOffset04Address: loadedSlotDwordAtOffset04Address,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset00Read: true,
                receiverDwordAtOffset04Read: true,
                loadedSlotDwordAtOffset04Read: false,
                receiverDwordAtOffset18Read: false,
                selectedEcxValue: null,
                unreachedReceiverDwordAtOffset00CaptureProvided: false,
                unreachedReceiverDwordAtOffset04CaptureProvided: false,
                unreachedLoadedSlotDwordAtOffset04CaptureProvided: false,
                unreachedReceiverDwordAtOffset18CaptureProvided: capture.ReceiverDwordAtOffset18.HasValue,
                unreachedHandoffReturnCaptureProvided: capture.HandoffHelperReturnedNormally.HasValue,
                argumentsPushedInInstructionOrder: pushedArguments.ToImmutable(),
                handoffCallReached: false,
                handoffHelperReturnedNormally: null,
                handoffReturnBoundaryReached: null,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: LoadedSlotDwordAtOffset04ReadRawFileOffset);
        }

        uint selectedEcxValue = loadedSlotDwordAtOffset04 == 0
            ? InitialEcxLiteral
            : loadedSlotDwordAtOffset04;
        pushedArguments.Add(selectedEcxValue);
        pushedArguments.Add(HandoffLiteralArgument);
        pushedArguments.Add(FourArgument);

        if (capture.ReceiverDwordAtOffset18 is not uint receiverDwordAtOffset18)
        {
            bool laterCaptureProvided = capture.HandoffHelperReturnedNormally.HasValue;
            return Plan(
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveZeroResultOutcome.MissingReceiverOffset18CaptureWithUnreachedHandoffReturnCapture
                    : UnreliableReceiveZeroResultOutcome.MissingReceiverOffset18Capture,
                priorRecycleHelperReturnedNormally: true,
                esiIsZero: true,
                receiverDwordAtOffset00Address: receiverDwordAtOffset00Address,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                loadedSlotDwordAtOffset04Address: loadedSlotDwordAtOffset04Address,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset00Read: true,
                receiverDwordAtOffset04Read: true,
                loadedSlotDwordAtOffset04Read: true,
                receiverDwordAtOffset18Read: false,
                selectedEcxValue: selectedEcxValue,
                unreachedReceiverDwordAtOffset00CaptureProvided: false,
                unreachedReceiverDwordAtOffset04CaptureProvided: false,
                unreachedLoadedSlotDwordAtOffset04CaptureProvided: false,
                unreachedReceiverDwordAtOffset18CaptureProvided: false,
                unreachedHandoffReturnCaptureProvided: laterCaptureProvided,
                argumentsPushedInInstructionOrder: pushedArguments.ToImmutable(),
                handoffCallReached: false,
                handoffHelperReturnedNormally: null,
                handoffReturnBoundaryReached: null,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: ReceiverDwordAtOffset18PushRawFileOffset);
        }

        pushedArguments.Add(receiverDwordAtOffset18);
        ImmutableArray<uint> pushed = pushedArguments.ToImmutable();

        if (capture.HandoffHelperReturnedNormally is not bool handoffHelperReturnedNormally)
        {
            return Plan(
                capture,
                UnreliableReceiveZeroResultOutcome.MissingHandoffHelperReturnCapture,
                priorRecycleHelperReturnedNormally: true,
                esiIsZero: true,
                receiverDwordAtOffset00Address: receiverDwordAtOffset00Address,
                receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
                loadedSlotDwordAtOffset04Address: loadedSlotDwordAtOffset04Address,
                receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
                receiverDwordAtOffset00Read: true,
                receiverDwordAtOffset04Read: true,
                loadedSlotDwordAtOffset04Read: true,
                receiverDwordAtOffset18Read: true,
                selectedEcxValue: selectedEcxValue,
                unreachedReceiverDwordAtOffset00CaptureProvided: false,
                unreachedReceiverDwordAtOffset04CaptureProvided: false,
                unreachedLoadedSlotDwordAtOffset04CaptureProvided: false,
                unreachedReceiverDwordAtOffset18CaptureProvided: false,
                unreachedHandoffReturnCaptureProvided: false,
                argumentsPushedInInstructionOrder: pushed,
                handoffCallReached: true,
                handoffHelperReturnedNormally: null,
                handoffReturnBoundaryReached: null,
                selectedDestinationRawFileOffset: null,
                stopAtRawFileOffsetExclusive: HandoffCallRawFileOffset);
        }

        return Plan(
            capture,
            handoffHelperReturnedNormally
                ? UnreliableReceiveZeroResultOutcome.HandoffReturnedToExclusiveBoundary
                : UnreliableReceiveZeroResultOutcome.HandoffHelperDidNotReturnNormally,
            priorRecycleHelperReturnedNormally: true,
            esiIsZero: true,
            receiverDwordAtOffset00Address: receiverDwordAtOffset00Address,
            receiverDwordAtOffset04Address: receiverDwordAtOffset04Address,
            loadedSlotDwordAtOffset04Address: loadedSlotDwordAtOffset04Address,
            receiverDwordAtOffset18Address: receiverDwordAtOffset18Address,
            receiverDwordAtOffset00Read: true,
            receiverDwordAtOffset04Read: true,
            loadedSlotDwordAtOffset04Read: true,
            receiverDwordAtOffset18Read: true,
            selectedEcxValue: selectedEcxValue,
            unreachedReceiverDwordAtOffset00CaptureProvided: false,
            unreachedReceiverDwordAtOffset04CaptureProvided: false,
            unreachedLoadedSlotDwordAtOffset04CaptureProvided: false,
            unreachedReceiverDwordAtOffset18CaptureProvided: false,
            unreachedHandoffReturnCaptureProvided: false,
            argumentsPushedInInstructionOrder: pushed,
            handoffCallReached: true,
            handoffHelperReturnedNormally: handoffHelperReturnedNormally,
            handoffReturnBoundaryReached: handoffHelperReturnedNormally,
            selectedDestinationRawFileOffset: null,
            stopAtRawFileOffsetExclusive: handoffHelperReturnedNormally
                ? HandoffCallReturnRawFileOffset
                : HandoffCallRawFileOffset);
    }

    private static UnreliableReceiveZeroResultPlan Plan(
        UnreliableReceiveZeroResultCapture capture,
        UnreliableReceiveZeroResultOutcome outcome,
        bool? priorRecycleHelperReturnedNormally,
        bool? esiIsZero,
        uint receiverDwordAtOffset00Address,
        uint receiverDwordAtOffset04Address,
        uint? loadedSlotDwordAtOffset04Address,
        uint receiverDwordAtOffset18Address,
        bool receiverDwordAtOffset00Read,
        bool receiverDwordAtOffset04Read,
        bool loadedSlotDwordAtOffset04Read,
        bool receiverDwordAtOffset18Read,
        uint? selectedEcxValue,
        bool unreachedReceiverDwordAtOffset00CaptureProvided,
        bool unreachedReceiverDwordAtOffset04CaptureProvided,
        bool unreachedLoadedSlotDwordAtOffset04CaptureProvided,
        bool unreachedReceiverDwordAtOffset18CaptureProvided,
        bool unreachedHandoffReturnCaptureProvided,
        ImmutableArray<uint> argumentsPushedInInstructionOrder,
        bool handoffCallReached,
        bool? handoffHelperReturnedNormally,
        bool? handoffReturnBoundaryReached,
        uint? selectedDestinationRawFileOffset,
        uint stopAtRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            priorRecycleHelperReturnedNormally,
            esiIsZero,
            esiIsZero is bool isZero ? !isZero : null,
            receiverDwordAtOffset00Address,
            receiverDwordAtOffset04Address,
            loadedSlotDwordAtOffset04Address,
            receiverDwordAtOffset18Address,
            receiverDwordAtOffset00Read,
            receiverDwordAtOffset04Read,
            loadedSlotDwordAtOffset04Read,
            receiverDwordAtOffset18Read,
            selectedEcxValue,
            unreachedReceiverDwordAtOffset00CaptureProvided,
            unreachedReceiverDwordAtOffset04CaptureProvided,
            unreachedLoadedSlotDwordAtOffset04CaptureProvided,
            unreachedReceiverDwordAtOffset18CaptureProvided,
            unreachedHandoffReturnCaptureProvided,
            argumentsPushedInInstructionOrder,
            handoffCallReached,
            handoffHelperReturnedNormally,
            handoffReturnBoundaryReached,
            selectedDestinationRawFileOffset,
            stopAtRawFileOffsetExclusive);
}
