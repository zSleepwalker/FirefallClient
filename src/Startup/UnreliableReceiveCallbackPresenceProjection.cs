using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveCallbackPresenceCapture(
    uint ReceiverEdi,
    bool EntryBoundaryValidated,
    uint? ReceiverDwordAt14,
    bool? DiagnosticCallReturnedNormally);

public enum UnreliableReceiveCallbackPresenceOutcome
{
    EntryBoundaryNotValidated,
    MissingReceiverDwordAt14,
    UnreachedCaptureProvided,
    NonzeroReceiverDword,
    MissingDiagnosticCallReturn,
    DiagnosticCallDidNotReturnNormally,
    NormalReturnReached
}

public readonly record struct UnreliableReceiveCallbackPresencePlan(
    UnreliableReceiveCallbackPresenceCapture Capture,
    UnreliableReceiveCallbackPresenceOutcome Outcome,
    bool ReceiverDwordAt14Read,
    uint? ReceiverDwordAt14Address,
    uint? CapturedReceiverDwordAt14,
    bool UnreachedDiagnosticCallReturnProvided,
    ImmutableArray<uint> StackPushesInInstructionOrder,
    bool IndirectCallReached,
    bool? DiagnosticCallReturnedNormally,
    bool NormalCallBoundaryReached,
    bool StackCleanupReached,
    byte? StackCleanupBytes,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveCallbackPresenceInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the receiver-presence gate and its opaque indirect call through stack cleanup.
/// The validated entry boundary admits either the clean zero-selector branch or the
/// normal nonzero-marker callback scalar-helper cleanup path.
/// </summary>
public static class UnreliableReceiveCallbackPresenceProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE5B32;
    public const uint EntryVirtualAddress = 0x00FE6732;
    public const uint ReceiverDwordAt14ReadRawFileOffset = EntryRawFileOffset;
    public const uint NonzeroBranchRawFileOffset = 0x00BE5B36;
    public const uint FirstStackPushRawFileOffset = 0x00BE5B38;
    public const uint IndirectCallRawFileOffset = 0x00BE5B49;
    public const uint StackCleanupRawFileOffset = 0x00BE5B4F;
    public const uint ContinuationRawFileOffset = 0x00BE5B52;
    public const uint ContinuationVirtualAddress = 0x00FE6752;
    public const uint EndRawFileOffsetExclusive = ContinuationRawFileOffset;

    public const uint ReceiverDwordAt14Offset = 0x14;
    public const uint IndirectCallPointerCellVirtualAddress = 0x01C15EA0;
    public const byte StackCleanupAmount = 0x10;

    public const string CapturedWindowBytes =
        "83 7F 14 00 75 1A 6A 00 68 CA 02 00 00 68 06 8C B5 01 "
        + "68 07 8C B5 01 FF 15 A0 5E C1 01 83 C4 10";

    public static ImmutableArray<UnreliableReceiveCallbackPresenceInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveCallbackPresenceInstructionEvidence(
                "Compare the first DWORD read from [EDI+0x14] with zero",
                ReceiverDwordAt14ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDwordAt14ReadRawFileOffset),
                "83 7F 14 00"),
            new UnreliableReceiveCallbackPresenceInstructionEvidence(
                "A nonzero DWORD branches to the continuation at raw 0xBE5B52",
                NonzeroBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(NonzeroBranchRawFileOffset),
                "75 1A"),
            new UnreliableReceiveCallbackPresenceInstructionEvidence(
                "Push zero",
                FirstStackPushRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstStackPushRawFileOffset),
                "6A 00"),
            new UnreliableReceiveCallbackPresenceInstructionEvidence(
                "Push the DWORD immediate 0x2CA",
                0x00BE5B3A,
                RawFileOffsetToVirtualAddress(0x00BE5B3A),
                "68 CA 02 00 00"),
            new UnreliableReceiveCallbackPresenceInstructionEvidence(
                "Push the DWORD immediate 0x01B58C06",
                0x00BE5B3F,
                RawFileOffsetToVirtualAddress(0x00BE5B3F),
                "68 06 8C B5 01"),
            new UnreliableReceiveCallbackPresenceInstructionEvidence(
                "Push the DWORD immediate 0x01B58C07",
                0x00BE5B44,
                RawFileOffsetToVirtualAddress(0x00BE5B44),
                "68 07 8C B5 01"),
            new UnreliableReceiveCallbackPresenceInstructionEvidence(
                "Call indirectly through the absolute pointer cell at VA 0x01C15EA0",
                IndirectCallRawFileOffset,
                RawFileOffsetToVirtualAddress(IndirectCallRawFileOffset),
                "FF 15 A0 5E C1 01"),
            new UnreliableReceiveCallbackPresenceInstructionEvidence(
                "On normal call return, clean up 0x10 stack bytes",
                StackCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(StackCleanupRawFileOffset),
                "83 C4 10"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAt14Address(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverDwordAt14Offset);

    public static uint NonzeroBranchDestination(uint branchRawFileOffset) =>
        unchecked(branchRawFileOffset + 2 + 0x1A);

    public static UnreliableReceiveCallbackPresencePlan Project(
        UnreliableReceiveCallbackPresenceCapture capture)
    {
        if (!capture.EntryBoundaryValidated)
        {
            return Stop(
                capture,
                UnreliableReceiveCallbackPresenceOutcome.EntryBoundaryNotValidated,
                receiverDwordAt14Address: null,
                stopAt: EntryRawFileOffset);
        }

        uint receiverDwordAt14Address = ReceiverDwordAt14Address(capture.ReceiverEdi);
        if (capture.ReceiverDwordAt14 is not uint receiverDwordAt14)
        {
            return Stop(
                capture,
                capture.DiagnosticCallReturnedNormally.HasValue
                    ? UnreliableReceiveCallbackPresenceOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveCallbackPresenceOutcome.MissingReceiverDwordAt14,
                receiverDwordAt14Address,
                unreachedDiagnosticCallReturnProvided:
                    capture.DiagnosticCallReturnedNormally.HasValue,
                stopAt: ReceiverDwordAt14ReadRawFileOffset);
        }

        if (receiverDwordAt14 != 0)
        {
            return Stop(
                capture,
                capture.DiagnosticCallReturnedNormally.HasValue
                    ? UnreliableReceiveCallbackPresenceOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveCallbackPresenceOutcome.NonzeroReceiverDword,
                receiverDwordAt14Address,
                receiverDwordAt14Read: true,
                capturedReceiverDwordAt14: receiverDwordAt14,
                unreachedDiagnosticCallReturnProvided:
                    capture.DiagnosticCallReturnedNormally.HasValue,
                selectedDestinationRawFileOffset: ContinuationRawFileOffset,
                stopAt: ContinuationRawFileOffset);
        }

        ImmutableArray<uint> pushes =
            ImmutableArray.Create(0u, 0x2CAu, 0x01B58C06u, 0x01B58C07u);
        if (capture.DiagnosticCallReturnedNormally is not bool returnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveCallbackPresenceOutcome.MissingDiagnosticCallReturn,
                receiverDwordAt14Address,
                receiverDwordAt14Read: true,
                capturedReceiverDwordAt14: receiverDwordAt14,
                stackPushesInInstructionOrder: pushes,
                indirectCallReached: true,
                stopAt: IndirectCallRawFileOffset);
        }

        if (!returnedNormally)
        {
            return Stop(
                capture,
                UnreliableReceiveCallbackPresenceOutcome.DiagnosticCallDidNotReturnNormally,
                receiverDwordAt14Address,
                receiverDwordAt14Read: true,
                capturedReceiverDwordAt14: receiverDwordAt14,
                stackPushesInInstructionOrder: pushes,
                indirectCallReached: true,
                diagnosticCallReturnedNormally: false,
                stopAt: IndirectCallRawFileOffset);
        }

        return Stop(
            capture,
            UnreliableReceiveCallbackPresenceOutcome.NormalReturnReached,
            receiverDwordAt14Address,
            receiverDwordAt14Read: true,
            capturedReceiverDwordAt14: receiverDwordAt14,
            stackPushesInInstructionOrder: pushes,
            indirectCallReached: true,
            diagnosticCallReturnedNormally: true,
            normalCallBoundaryReached: true,
            stackCleanupReached: true,
            stackCleanupBytes: StackCleanupAmount,
            selectedDestinationRawFileOffset: ContinuationRawFileOffset,
            stopAt: ContinuationRawFileOffset);
    }

    private static UnreliableReceiveCallbackPresencePlan Stop(
        UnreliableReceiveCallbackPresenceCapture capture,
        UnreliableReceiveCallbackPresenceOutcome outcome,
        uint? receiverDwordAt14Address,
        bool receiverDwordAt14Read = false,
        uint? capturedReceiverDwordAt14 = null,
        bool unreachedDiagnosticCallReturnProvided = false,
        ImmutableArray<uint> stackPushesInInstructionOrder = default,
        bool indirectCallReached = false,
        bool? diagnosticCallReturnedNormally = null,
        bool normalCallBoundaryReached = false,
        bool stackCleanupReached = false,
        byte? stackCleanupBytes = null,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            receiverDwordAt14Read,
            receiverDwordAt14Address,
            capturedReceiverDwordAt14,
            unreachedDiagnosticCallReturnProvided,
            stackPushesInInstructionOrder.IsDefault
                ? ImmutableArray<uint>.Empty
                : stackPushesInInstructionOrder,
            indirectCallReached,
            diagnosticCallReturnedNormally,
            normalCallBoundaryReached,
            stackCleanupReached,
            stackCleanupBytes,
            selectedDestinationRawFileOffset,
            stopAt);
}
