using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct SocketStateEntryCapture(
    uint? EntryEcx,
    uint? StackArgumentEbp8,
    uint? ReceiverDword108,
    bool? DiagnosticReturnedNormally);

public enum SocketStateEntryOutcome
{
    MissingEntryEcx,
    MissingStackArgumentEbp8,
    MissingReceiverDword108,
    UnequalStateSkipsDiagnostic,
    DiagnosticReturnUnavailable,
    DiagnosticDidNotReturnNormally,
    EqualStateDiagnosticReturnedNormally
}

public enum SocketStateEntryActionKind
{
    SaveEcxInEsi,
    LoadStackArgumentIntoEdi,
    CaptureReceiverDword108,
    CompareRequestedState,
    BranchOnUnequalState,
    PushDiagnosticArgument,
    CallDiagnostic,
    CleanupDiagnosticArguments
}

public readonly record struct SocketStateEntryAction(
    SocketStateEntryActionKind Kind,
    uint RawFileOffset,
    uint? CapturedValue = null,
    bool? ConditionMet = null,
    bool? CallReturnedNormally = null,
    uint? BranchTargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

public readonly record struct SocketStateEntryPlan(
    SocketStateEntryCapture Capture,
    SocketStateEntryOutcome Outcome,
    ImmutableArray<SocketStateEntryAction> Actions,
    uint? EsiValueAtCompare,
    uint? EdiValueAtCompare,
    uint? ReceiverDword108Address,
    bool? StatesEqual,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct SocketStateEntryInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects the bounded state comparison and diagnostic call at raw BD1B98.
/// Equality reaches the diagnostic; this slice does not model a function
/// return or any state write beyond its exclusive BD1BC2 boundary.
/// </summary>
public static class SocketStateEntryProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x00BD1B98;
    public const uint StackArgumentLoadRawFileOffset = 0x00BD1B9A;
    public const uint ReceiverDword108CompareRawFileOffset = 0x00BD1B9D;
    public const uint UnequalBranchRawFileOffset = 0x00BD1BA3;
    public const uint DiagnosticArgument0PushRawFileOffset = 0x00BD1BA5;
    public const uint DiagnosticArgument1PushRawFileOffset = 0x00BD1BAA;
    public const uint DiagnosticArgument2PushRawFileOffset = 0x00BD1BAF;
    public const uint DiagnosticArgument3PushRawFileOffset = 0x00BD1BB4;
    public const uint DiagnosticCallRawFileOffset = 0x00BD1BB9;
    public const uint DiagnosticCallReturnRawFileOffsetExclusive = 0x00BD1BBF;
    public const uint StackCleanupRawFileOffset = 0x00BD1BBF;
    public const uint EntryBoundaryRawFileOffsetExclusive = 0x00BD1BC2;

    public const uint ReceiverDword108Offset = 0x108;
    public const uint DiagnosticFunctionPointerCellVirtualAddress = 0x01C15EA0;

    public const uint DiagnosticArgument0VirtualAddress = 0x01BB66B0;
    public const uint DiagnosticArgument1 = 0x17F;
    public const uint DiagnosticArgument2VirtualAddress = 0x1B57D77;
    public const uint DiagnosticArgument3VirtualAddress = 0x1B57D93;

    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EntryBoundaryVirtualAddressExclusive =
        EntryBoundaryRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint UnequalBranchTargetRawFileOffset =
        EntryBoundaryRawFileOffsetExclusive;
    public const uint UnequalBranchTargetVirtualAddress =
        UnequalBranchTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint DiagnosticCallVirtualAddress =
        DiagnosticCallRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedEntryBytes =
        "8B F1 8B 7D 08 3B BE 08 01 00 00 75 1D "
            + "68 B0 66 BB 01 68 7F 01 00 00 68 77 7D B5 01 "
            + "68 93 7D B5 01 FF 15 A0 5E C1 01 83 C4 10";

    public static ImmutableArray<uint> DiagnosticArguments { get; } =
        ImmutableArray.Create(
            DiagnosticArgument0VirtualAddress,
            DiagnosticArgument1,
            DiagnosticArgument2VirtualAddress,
            DiagnosticArgument3VirtualAddress);

    public static IReadOnlyList<SocketStateEntryInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new SocketStateEntryInstructionEvidence(
                "Save incoming ECX in ESI",
                EntryRawFileOffset,
                "8B F1"),
            new SocketStateEntryInstructionEvidence(
                "Load the requested state from [EBP+8] into EDI",
                StackArgumentLoadRawFileOffset,
                "8B 7D 08"),
            new SocketStateEntryInstructionEvidence(
                "Compare EDI with the independently captured DWORD at [ESI+0x108]",
                ReceiverDword108CompareRawFileOffset,
                "3B BE 08 01 00 00"),
            new SocketStateEntryInstructionEvidence(
                "A non-equal comparison branches over the diagnostic to the exclusive boundary",
                UnequalBranchRawFileOffset,
                "75 1D",
                UnequalBranchTargetRawFileOffset,
                UnequalBranchTargetVirtualAddress),
            new SocketStateEntryInstructionEvidence(
                "Push diagnostic argument 0",
                DiagnosticArgument0PushRawFileOffset,
                "68 B0 66 BB 01"),
            new SocketStateEntryInstructionEvidence(
                "Push diagnostic argument 1",
                DiagnosticArgument1PushRawFileOffset,
                "68 7F 01 00 00"),
            new SocketStateEntryInstructionEvidence(
                "Push diagnostic argument 2",
                DiagnosticArgument2PushRawFileOffset,
                "68 77 7D B5 01"),
            new SocketStateEntryInstructionEvidence(
                "Push diagnostic argument 3",
                DiagnosticArgument3PushRawFileOffset,
                "68 93 7D B5 01"),
            new SocketStateEntryInstructionEvidence(
                "Call through diagnostic function-pointer cell 0x1C15EA0",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                TargetVirtualAddress: DiagnosticFunctionPointerCellVirtualAddress),
            new SocketStateEntryInstructionEvidence(
                "After a normal diagnostic return, discard the four pushed arguments",
                StackCleanupRawFileOffset,
                "83 C4 10")
        });

    public static SocketStateEntryPlan Project(SocketStateEntryCapture capture)
    {
        var actions = ImmutableArray.CreateBuilder<SocketStateEntryAction>();

        if (capture.EntryEcx is not uint esi)
        {
            if (capture.StackArgumentEbp8 is not null
                || capture.ReceiverDword108 is not null
                || capture.DiagnosticReturnedNormally is not null)
            {
                throw new ArgumentException(
                    "Later captures are unreachable without the entry ECX capture.",
                    nameof(capture));
            }

            return Plan(SocketStateEntryOutcome.MissingEntryEcx);
        }

        actions.Add(new(
            SocketStateEntryActionKind.SaveEcxInEsi,
            EntryRawFileOffset,
            CapturedValue: esi));

        if (capture.StackArgumentEbp8 is not uint edi)
        {
            if (capture.ReceiverDword108 is not null
                || capture.DiagnosticReturnedNormally is not null)
            {
                throw new ArgumentException(
                    "Later captures are unreachable without the [EBP+8] argument capture.",
                    nameof(capture));
            }

            return Plan(
                SocketStateEntryOutcome.MissingStackArgumentEbp8,
                esi,
                stopAt: StackArgumentLoadRawFileOffset);
        }

        actions.Add(new(
            SocketStateEntryActionKind.LoadStackArgumentIntoEdi,
            StackArgumentLoadRawFileOffset,
            CapturedValue: edi));

        if (capture.ReceiverDword108 is not uint receiverDword)
        {
            if (capture.DiagnosticReturnedNormally is not null)
            {
                throw new ArgumentException(
                    "The diagnostic return capture is unreachable without the comparison DWORD.",
                    nameof(capture));
            }

            return Plan(
                SocketStateEntryOutcome.MissingReceiverDword108,
                esi,
                edi,
                ReceiverDword108Address: AddDword(esi, ReceiverDword108Offset),
                stopAt: ReceiverDword108CompareRawFileOffset);
        }

        uint receiverDwordAddress = AddDword(esi, ReceiverDword108Offset);
        bool statesEqual = edi == receiverDword;
        actions.Add(new(
            SocketStateEntryActionKind.CaptureReceiverDword108,
            ReceiverDword108CompareRawFileOffset,
            CapturedValue: receiverDword));
        actions.Add(new(
            SocketStateEntryActionKind.CompareRequestedState,
            ReceiverDword108CompareRawFileOffset,
            CapturedValue: edi,
            ConditionMet: statesEqual));
        actions.Add(new(
            SocketStateEntryActionKind.BranchOnUnequalState,
            UnequalBranchRawFileOffset,
            ConditionMet: !statesEqual,
            BranchTargetRawFileOffset: UnequalBranchTargetRawFileOffset,
            TargetVirtualAddress: UnequalBranchTargetVirtualAddress));

        if (!statesEqual)
        {
            if (capture.DiagnosticReturnedNormally is not null)
            {
                throw new ArgumentException(
                    "The diagnostic return capture is unreachable when the state values differ.",
                    nameof(capture));
            }

            return Plan(
                SocketStateEntryOutcome.UnequalStateSkipsDiagnostic,
                esi,
                edi,
                receiverDwordAddress,
                statesEqual,
                EntryBoundaryRawFileOffsetExclusive);
        }

        for (int i = 0; i < DiagnosticArguments.Length; i++)
        {
            actions.Add(new(
                SocketStateEntryActionKind.PushDiagnosticArgument,
                DiagnosticArgumentPushRawFileOffset(i),
                CapturedValue: DiagnosticArguments[i]));
        }

        actions.Add(new(
            SocketStateEntryActionKind.CallDiagnostic,
            DiagnosticCallRawFileOffset,
            CallReturnedNormally: capture.DiagnosticReturnedNormally,
            TargetVirtualAddress: DiagnosticFunctionPointerCellVirtualAddress));

        if (capture.DiagnosticReturnedNormally is null)
        {
            return Plan(
                SocketStateEntryOutcome.DiagnosticReturnUnavailable,
                esi,
                edi,
                receiverDwordAddress,
                statesEqual,
                DiagnosticCallReturnRawFileOffsetExclusive);
        }

        if (!capture.DiagnosticReturnedNormally.Value)
        {
            return Plan(
                SocketStateEntryOutcome.DiagnosticDidNotReturnNormally,
                esi,
                edi,
                receiverDwordAddress,
                statesEqual,
                DiagnosticCallReturnRawFileOffsetExclusive);
        }

        actions.Add(new(
            SocketStateEntryActionKind.CleanupDiagnosticArguments,
            StackCleanupRawFileOffset,
            CapturedValue: 0x10));
        return Plan(
            SocketStateEntryOutcome.EqualStateDiagnosticReturnedNormally,
            esi,
            edi,
            receiverDwordAddress,
            statesEqual,
            EntryBoundaryRawFileOffsetExclusive);

        SocketStateEntryPlan Plan(
            SocketStateEntryOutcome outcome,
            uint? esiValue = null,
            uint? ediValue = null,
            uint? ReceiverDword108Address = null,
            bool? statesEqualValue = null,
            uint stopAt = EntryRawFileOffset) =>
            new(
                capture,
                outcome,
                actions.ToImmutable(),
                esiValue,
                ediValue,
                ReceiverDword108Address,
                statesEqualValue,
                stopAt);
    }

    public static uint ReceiverDword108Address(uint receiverPointer) =>
        AddDword(receiverPointer, ReceiverDword108Offset);

    public static uint AddDword(uint value, uint displacement) =>
        unchecked(value + displacement);

    private static uint DiagnosticArgumentPushRawFileOffset(int index) =>
        index switch
        {
            0 => DiagnosticArgument0PushRawFileOffset,
            1 => DiagnosticArgument1PushRawFileOffset,
            2 => DiagnosticArgument2PushRawFileOffset,
            3 => DiagnosticArgument3PushRawFileOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
}
