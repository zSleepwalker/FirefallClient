using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadXorTableSetupCapture(
    bool EntryBoundaryValidated,
    uint FramePointer,
    uint RetainedEbx,
    byte AlByte,
    uint? StackLength,
    uint? FirstTableDword,
    uint? SecondTableDword);

public enum PayloadXorTableSetupOutcome
{
    EntryBoundaryNotValidated,
    MissingStackLength,
    MissingFirstTableDword,
    MissingSecondTableDword,
    UnreachedCaptureProvided,
    UnsignedStartAtOrBeyondEnd,
    UnalignedStart,
    EightByteSpanExceedsEnd,
    EightByteSpanWithinEnd
}

public enum PayloadXorTableSetupActionKind
{
    ReadStackLengthFromFramePlus8,
    ZeroExtendAlToEax,
    AddRetainedEbxToStackLength,
    ReadFirstTableDword,
    ReadSecondTableDword,
    CompareRetainedEbxWithEnd,
    BranchUnsignedAboveOrEqualToEnd,
    ComputeRetainedEbxPlus8,
    Nop,
    TestRetainedEbxLowThreeBits,
    BranchIfUnaligned,
    ComparePlus8WithEnd,
    BranchIfPlus8AboveEnd,
    StopBeforeXorBody,
    StopBeforeOutOfWindowBranchTarget
}

public readonly record struct PayloadXorTableSetupAction(
    PayloadXorTableSetupActionKind Kind,
    uint RawFileOffset);

public readonly record struct PayloadXorTableSetupPlan(
    PayloadXorTableSetupCapture Capture,
    PayloadXorTableSetupOutcome Outcome,
    bool UnreachedStackLengthCaptureProvided,
    bool UnreachedFirstTableDwordCaptureProvided,
    bool UnreachedSecondTableDwordCaptureProvided,
    bool StackLengthRead,
    uint? StackLengthAddress,
    uint? ZeroExtendedAl,
    uint? ComputedEnd,
    bool FirstTableDwordRead,
    uint? FirstTableDwordAddress,
    uint? FirstTableDword,
    bool SecondTableDwordRead,
    uint? SecondTableDwordAddress,
    uint? SecondTableDword,
    uint? PlusEightAddress,
    bool? RetainedEbxIsEightByteAligned,
    uint? SelectedBranchTargetRawFileOffset,
    uint StopAtRawFileOffsetExclusive,
    PayloadXorTableSetupAction[] Actions);

public readonly record struct PayloadXorTableSetupInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null,
    uint? AbsoluteVirtualAddress = null);

/// <summary>
/// Projects the table reads and unsigned/alignment guards at raw 0xBD5912..0xBD5939.
/// The projection does not model any instruction at or beyond the XOR body.
/// </summary>
public static class PayloadXorTableSetupProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryStartRawFileOffset = 0x00BD5912;
    public const uint EntryEndRawFileOffsetExclusive = 0x00BD5939;
    public const uint CapturedCodeLength =
        EntryEndRawFileOffsetExclusive - EntryStartRawFileOffset;

    public const uint StackLengthReadRawFileOffset = 0x00BD5912;
    public const uint AlZeroExtendRawFileOffset = 0x00BD5915;
    public const uint ComputeEndRawFileOffset = 0x00BD5918;
    public const uint FirstTableReadRawFileOffset = 0x00BD591A;
    public const uint SecondTableReadRawFileOffset = 0x00BD5921;
    public const uint CompareStartWithEndRawFileOffset = 0x00BD5928;
    public const uint UnsignedStartBranchRawFileOffset = 0x00BD592A;
    public const uint PlusEightRawFileOffset = 0x00BD592C;
    public const uint AlignmentTestRawFileOffset = 0x00BD5930;
    public const uint UnalignedBranchRawFileOffset = 0x00BD5933;
    public const uint PlusEightCompareRawFileOffset = 0x00BD5935;
    public const uint PlusEightAboveBranchRawFileOffset = 0x00BD5937;

    public const uint StackLengthOffsetFromFramePointer = 8;
    public const uint FirstTableVirtualAddress = 0x01BB7490;
    public const uint SecondTableVirtualAddress = 0x01BB7494;
    public const uint TableEntryStride = 8;
    public const uint TableValueOffset = 4;
    public const uint EightByteSpan = 8;

    public const uint UnsignedStartBranchTargetRawFileOffset = 0x00BD594E;
    public const uint AlternateBranchTargetRawFileOffset = 0x00BD5946;

    public const string CapturedCodeBytes =
        "8B 4D 08 0F B6 C0 03 CB 8B 14 C5 90 74 BB 01 "
        + "8B 34 C5 94 74 BB 01 3B D9 73 22 8D 43 08 90 "
        + "F6 C3 07 75 11 3B C1 77 0D";

    public static uint EntryStartVirtualAddress =>
        RawFileOffsetToVirtualAddress(EntryStartRawFileOffset);

    public static uint EntryEndVirtualAddressExclusive =>
        RawFileOffsetToVirtualAddress(EntryEndRawFileOffsetExclusive);

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint StackLengthAddress(uint framePointer) =>
        unchecked(framePointer + StackLengthOffsetFromFramePointer);

    public static uint TableDwordAddress(uint tableVirtualAddress, byte alIndex) =>
        unchecked(tableVirtualAddress + ((uint)alIndex * TableEntryStride));

    public static uint FirstTableDwordAddress(byte alIndex) =>
        TableDwordAddress(FirstTableVirtualAddress, alIndex);

    public static uint SecondTableDwordAddress(byte alIndex) =>
        TableDwordAddress(SecondTableVirtualAddress, alIndex);

    public static uint ComputeEnd(uint retainedEbx, uint stackLength) =>
        unchecked(retainedEbx + stackLength);

    public static uint ComputePlusEight(uint retainedEbx) =>
        unchecked(retainedEbx + EightByteSpan);

    public static ReadOnlyCollection<PayloadXorTableSetupInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadXorTableSetupInstructionEvidence(
                "Read the DWORD at the physical [EBP+8] address",
                StackLengthReadRawFileOffset,
                RawFileOffsetToVirtualAddress(StackLengthReadRawFileOffset),
                "8B 4D 08"),
            new PayloadXorTableSetupInstructionEvidence(
                "Zero-extend AL into EAX",
                AlZeroExtendRawFileOffset,
                RawFileOffsetToVirtualAddress(AlZeroExtendRawFileOffset),
                "0F B6 C0"),
            new PayloadXorTableSetupInstructionEvidence(
                "Add retained EBX to ECX with DWORD wrap",
                ComputeEndRawFileOffset,
                RawFileOffsetToVirtualAddress(ComputeEndRawFileOffset),
                "03 CB"),
            new PayloadXorTableSetupInstructionEvidence(
                "Read the first DWORD at [EAX*8+0x01BB7490]",
                FirstTableReadRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstTableReadRawFileOffset),
                "8B 14 C5 90 74 BB 01",
                AbsoluteVirtualAddress: FirstTableVirtualAddress),
            new PayloadXorTableSetupInstructionEvidence(
                "Read the second DWORD at [EAX*8+0x01BB7494] before the length branch",
                SecondTableReadRawFileOffset,
                RawFileOffsetToVirtualAddress(SecondTableReadRawFileOffset),
                "8B 34 C5 94 74 BB 01",
                AbsoluteVirtualAddress: SecondTableVirtualAddress),
            new PayloadXorTableSetupInstructionEvidence(
                "Compare retained EBX against the wrapped end",
                CompareStartWithEndRawFileOffset,
                RawFileOffsetToVirtualAddress(CompareStartWithEndRawFileOffset),
                "3B D9"),
            new PayloadXorTableSetupInstructionEvidence(
                "JNB exits on unsigned EBX >= end",
                UnsignedStartBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(UnsignedStartBranchRawFileOffset),
                "73 22",
                UnsignedStartBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(UnsignedStartBranchTargetRawFileOffset)),
            new PayloadXorTableSetupInstructionEvidence(
                "Compute retained EBX+8 with DWORD wrap",
                PlusEightRawFileOffset,
                RawFileOffsetToVirtualAddress(PlusEightRawFileOffset),
                "8D 43 08"),
            new PayloadXorTableSetupInstructionEvidence(
                "NOP",
                0x00BD592F,
                RawFileOffsetToVirtualAddress(0x00BD592F),
                "90"),
            new PayloadXorTableSetupInstructionEvidence(
                "Test the low three bits of retained EBX",
                AlignmentTestRawFileOffset,
                RawFileOffsetToVirtualAddress(AlignmentTestRawFileOffset),
                "F6 C3 07"),
            new PayloadXorTableSetupInstructionEvidence(
                "JNZ selects the alternate path when retained EBX is unaligned",
                UnalignedBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(UnalignedBranchRawFileOffset),
                "75 11",
                AlternateBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(AlternateBranchTargetRawFileOffset)),
            new PayloadXorTableSetupInstructionEvidence(
                "Compare EBX+8 against the wrapped end",
                PlusEightCompareRawFileOffset,
                RawFileOffsetToVirtualAddress(PlusEightCompareRawFileOffset),
                "3B C1"),
            new PayloadXorTableSetupInstructionEvidence(
                "JA selects the alternate path on unsigned EBX+8 > end",
                PlusEightAboveBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(PlusEightAboveBranchRawFileOffset),
                "77 0D",
                AlternateBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(AlternateBranchTargetRawFileOffset))
        });

    public static PayloadXorTableSetupPlan Project(
        PayloadXorTableSetupCapture capture)
    {
        var actions = new List<PayloadXorTableSetupAction>();
        if (!capture.EntryBoundaryValidated)
        {
            return Stop(
                capture,
                PayloadXorTableSetupOutcome.EntryBoundaryNotValidated,
                unreachedStackLength: capture.StackLength.HasValue,
                unreachedFirstTable: capture.FirstTableDword.HasValue,
                unreachedSecondTable: capture.SecondTableDword.HasValue,
                actions: actions,
                stopAt: EntryStartRawFileOffset);
        }

        uint stackAddress = StackLengthAddress(capture.FramePointer);
        actions.Add(Action(
            PayloadXorTableSetupActionKind.ReadStackLengthFromFramePlus8,
            StackLengthReadRawFileOffset));
        if (capture.StackLength is not uint stackLength)
        {
            return Stop(
                capture,
                HasAnyCaptureAfterStackLength(capture)
                    ? PayloadXorTableSetupOutcome.UnreachedCaptureProvided
                    : PayloadXorTableSetupOutcome.MissingStackLength,
                unreachedFirstTable: capture.FirstTableDword.HasValue,
                unreachedSecondTable: capture.SecondTableDword.HasValue,
                stackLengthAddress: stackAddress,
                actions: actions,
                stopAt: StackLengthReadRawFileOffset);
        }

        actions.Add(Action(
            PayloadXorTableSetupActionKind.ZeroExtendAlToEax,
            AlZeroExtendRawFileOffset));
        uint end = ComputeEnd(capture.RetainedEbx, stackLength);
        actions.Add(Action(
            PayloadXorTableSetupActionKind.AddRetainedEbxToStackLength,
            ComputeEndRawFileOffset));

        uint firstAddress = FirstTableDwordAddress(capture.AlByte);
        actions.Add(Action(
            PayloadXorTableSetupActionKind.ReadFirstTableDword,
            FirstTableReadRawFileOffset));
        if (capture.FirstTableDword is not uint firstTableDword)
        {
            return Stop(
                capture,
                capture.SecondTableDword.HasValue
                    ? PayloadXorTableSetupOutcome.UnreachedCaptureProvided
                    : PayloadXorTableSetupOutcome.MissingFirstTableDword,
                unreachedSecondTable: capture.SecondTableDword.HasValue,
                stackLengthRead: true,
                stackLengthAddress: stackAddress,
                zeroExtendedAl: capture.AlByte,
                computedEnd: end,
                firstTableDwordAddress: firstAddress,
                actions: actions,
                stopAt: FirstTableReadRawFileOffset);
        }

        uint secondAddress = SecondTableDwordAddress(capture.AlByte);
        actions.Add(Action(
            PayloadXorTableSetupActionKind.ReadSecondTableDword,
            SecondTableReadRawFileOffset));
        if (capture.SecondTableDword is not uint secondTableDword)
        {
            return Stop(
                capture,
                PayloadXorTableSetupOutcome.MissingSecondTableDword,
                stackLengthRead: true,
                stackLengthAddress: stackAddress,
                zeroExtendedAl: capture.AlByte,
                computedEnd: end,
                firstTableDwordRead: true,
                firstTableDwordAddress: firstAddress,
                firstTableDword: firstTableDword,
                secondTableDwordAddress: secondAddress,
                actions: actions,
                stopAt: SecondTableReadRawFileOffset);
        }

        actions.Add(Action(
            PayloadXorTableSetupActionKind.CompareRetainedEbxWithEnd,
            CompareStartWithEndRawFileOffset));
        actions.Add(Action(
            PayloadXorTableSetupActionKind.BranchUnsignedAboveOrEqualToEnd,
            UnsignedStartBranchRawFileOffset));
        if (capture.RetainedEbx >= end)
        {
            actions.Add(Action(
                PayloadXorTableSetupActionKind.StopBeforeOutOfWindowBranchTarget,
                UnsignedStartBranchRawFileOffset));
            return Complete(
                capture,
                PayloadXorTableSetupOutcome.UnsignedStartAtOrBeyondEnd,
                stackAddress,
                stackLength,
                end,
                firstAddress,
                firstTableDword,
                secondAddress,
                secondTableDword,
                plusEightAddress: null,
                aligned: null,
                selectedBranchTarget: UnsignedStartBranchTargetRawFileOffset,
                stopAt: UnsignedStartBranchRawFileOffset,
                actions: actions);
        }

        uint plusEight = ComputePlusEight(capture.RetainedEbx);
        actions.Add(Action(
            PayloadXorTableSetupActionKind.ComputeRetainedEbxPlus8,
            PlusEightRawFileOffset));
        actions.Add(Action(PayloadXorTableSetupActionKind.Nop, 0x00BD592F));
        actions.Add(Action(
            PayloadXorTableSetupActionKind.TestRetainedEbxLowThreeBits,
            AlignmentTestRawFileOffset));
        bool aligned = (capture.RetainedEbx & 7) == 0;
        actions.Add(Action(
            PayloadXorTableSetupActionKind.BranchIfUnaligned,
            UnalignedBranchRawFileOffset));
        if (!aligned)
        {
            actions.Add(Action(
                PayloadXorTableSetupActionKind.StopBeforeOutOfWindowBranchTarget,
                UnalignedBranchRawFileOffset));
            return Complete(
                capture,
                PayloadXorTableSetupOutcome.UnalignedStart,
                stackAddress,
                stackLength,
                end,
                firstAddress,
                firstTableDword,
                secondAddress,
                secondTableDword,
                plusEight,
                aligned,
                AlternateBranchTargetRawFileOffset,
                UnalignedBranchRawFileOffset,
                actions: actions);
        }

        actions.Add(Action(
            PayloadXorTableSetupActionKind.ComparePlus8WithEnd,
            PlusEightCompareRawFileOffset));
        actions.Add(Action(
            PayloadXorTableSetupActionKind.BranchIfPlus8AboveEnd,
            PlusEightAboveBranchRawFileOffset));
        if (plusEight > end)
        {
            actions.Add(Action(
                PayloadXorTableSetupActionKind.StopBeforeOutOfWindowBranchTarget,
                PlusEightAboveBranchRawFileOffset));
            return Complete(
                capture,
                PayloadXorTableSetupOutcome.EightByteSpanExceedsEnd,
                stackAddress,
                stackLength,
                end,
                firstAddress,
                firstTableDword,
                secondAddress,
                secondTableDword,
                plusEight,
                aligned,
                AlternateBranchTargetRawFileOffset,
                PlusEightAboveBranchRawFileOffset,
                actions: actions);
        }

        actions.Add(Action(
            PayloadXorTableSetupActionKind.StopBeforeXorBody,
            EntryEndRawFileOffsetExclusive));
        return Complete(
            capture,
            PayloadXorTableSetupOutcome.EightByteSpanWithinEnd,
            stackAddress,
            stackLength,
            end,
            firstAddress,
            firstTableDword,
            secondAddress,
            secondTableDword,
            plusEight,
            aligned,
            selectedBranchTarget: null,
            stopAt: EntryEndRawFileOffsetExclusive,
            actions: actions);
    }

    private static bool HasAnyCaptureAfterStackLength(
        PayloadXorTableSetupCapture capture) =>
        capture.FirstTableDword.HasValue || capture.SecondTableDword.HasValue;

    private static PayloadXorTableSetupAction Action(
        PayloadXorTableSetupActionKind kind,
        uint rawFileOffset) =>
        new(kind, rawFileOffset);

    private static PayloadXorTableSetupPlan Stop(
        PayloadXorTableSetupCapture capture,
        PayloadXorTableSetupOutcome outcome,
        bool unreachedStackLength = false,
        bool unreachedFirstTable = false,
        bool unreachedSecondTable = false,
        bool stackLengthRead = false,
        uint? stackLengthAddress = null,
        uint? zeroExtendedAl = null,
        uint? computedEnd = null,
        bool firstTableDwordRead = false,
        uint? firstTableDwordAddress = null,
        uint? firstTableDword = null,
        bool secondTableDwordRead = false,
        uint? secondTableDwordAddress = null,
        uint? secondTableDword = null,
        List<PayloadXorTableSetupAction>? actions = null,
        uint stopAt = EntryStartRawFileOffset) =>
        new(
            capture,
            outcome,
            unreachedStackLength,
            unreachedFirstTable,
            unreachedSecondTable,
            stackLengthRead,
            stackLengthAddress,
            zeroExtendedAl,
            computedEnd,
            firstTableDwordRead,
            firstTableDwordAddress,
            firstTableDword,
            secondTableDwordRead,
            secondTableDwordAddress,
            secondTableDword,
            null,
            null,
            null,
            stopAt,
            actions?.ToArray() ?? Array.Empty<PayloadXorTableSetupAction>());

    private static PayloadXorTableSetupPlan Complete(
        PayloadXorTableSetupCapture capture,
        PayloadXorTableSetupOutcome outcome,
        uint stackAddress,
        uint stackLength,
        uint end,
        uint firstAddress,
        uint firstTableDword,
        uint secondAddress,
        uint secondTableDword,
        uint? plusEightAddress,
        bool? aligned,
        uint? selectedBranchTarget,
        uint stopAt,
        List<PayloadXorTableSetupAction> actions) =>
        new(
            capture,
            outcome,
            false,
            false,
            false,
            true,
            stackAddress,
            capture.AlByte,
            end,
            true,
            firstAddress,
            firstTableDword,
            true,
            secondAddress,
            secondTableDword,
            plusEightAddress,
            aligned,
            selectedBranchTarget,
            stopAt,
            actions.ToArray());
}
