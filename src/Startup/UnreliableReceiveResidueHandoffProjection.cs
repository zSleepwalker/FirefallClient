using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveResidueHandoffEntry
{
    private UnreliableReceiveResidueHandoffEntry(
        ushort receiverWord,
        ushort payloadWord,
        uint entryDifference32,
        ushort entryResidue)
    {
        ReceiverWord = receiverWord;
        PayloadWord = payloadWord;
        EntryDifference32 = entryDifference32;
        EntryResidue = entryResidue;
    }

    public ushort ReceiverWord { get; }
    public ushort PayloadWord { get; }
    public uint EntryDifference32 { get; }
    public ushort EntryResidue { get; }

    public bool IsValid =>
        EntryResidue >= UnreliableReceiveResidueHandoffProjection.EntryResidueMinimum
        && EntryResidue == unchecked((ushort)EntryDifference32)
        && EntryDifference32 == unchecked((uint)ReceiverWord - PayloadWord);

    public static bool TryCreate(
        ushort receiverWord,
        ushort payloadWord,
        out UnreliableReceiveResidueHandoffEntry entry)
    {
        uint difference32 = unchecked((uint)receiverWord - payloadWord);
        ushort residue = unchecked((ushort)difference32);
        if (residue < UnreliableReceiveResidueHandoffProjection.EntryResidueMinimum)
        {
            entry = default;
            return false;
        }

        entry = new UnreliableReceiveResidueHandoffEntry(
            receiverWord,
            payloadWord,
            difference32,
            residue);
        return true;
    }
}

public readonly record struct UnreliableReceiveResidueHandoffCapture(
    uint ReceiverBaseAddress,
    uint? ReceiverDwordAtOffset04,
    uint? ReceiverDwordAtOffset18,
    bool? HelperReturnedNormally);

public enum UnreliableReceiveResidueHandoffOutcome
{
    EarlyJbeTaken,
    MissingReceiverOffset04Capture,
    MissingReceiverOffset04CaptureWithUnreachedLaterCaptures,
    MissingReceiverOffset18Capture,
    MissingReceiverOffset18CaptureWithUnreachedHelperReturnCapture,
    MissingHelperReturnCapture,
    HelperDidNotReturnNormally,
    HelperReturnedToExclusiveBoundary
}

public enum UnreliableReceiveResidueHandoffPushKind
{
    ReceiverDwordAtOffset04,
    ResidueMinusOne,
    LiteralArgument,
    ZeroArgument,
    ReceiverDwordAtOffset18
}

public readonly record struct UnreliableReceiveResidueHandoffPlan(
    UnreliableReceiveResidueHandoffEntry Entry,
    UnreliableReceiveResidueHandoffCapture Capture,
    UnreliableReceiveResidueHandoffOutcome Outcome,
    uint ReceiverDwordAtOffset04Address,
    uint ReceiverDwordAtOffset18Address,
    uint HandoffDifference32,
    ushort HandoffResidue,
    bool? EarlyJbeTaken,
    uint? ResidueMinusOne,
    bool ReceiverDwordAtOffset04Read,
    bool ReceiverDwordAtOffset18Read,
    bool UnreachedReceiverDwordAtOffset04CaptureProvided,
    bool UnreachedReceiverDwordAtOffset18CaptureProvided,
    bool UnreachedHelperReturnCaptureProvided,
    ImmutableArray<uint> ArgumentsPushedInInstructionOrder,
    ImmutableArray<uint> CalleeStackArguments,
    bool HelperCallReached,
    bool? HelperReturnedNormally,
    bool? HelperReturnBoundaryReached,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveResidueHandoffInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the residue handoff through the helper-call return boundary only.
/// </summary>
public static class UnreliableReceiveResidueHandoffProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint HandoffStartRawFileOffset = 0x00BE5AC2;
    public const uint HandoffStartVirtualAddress = 0x00FE66C2;
    public const uint HandoffEndRawFileOffsetExclusive = 0x00BE5AE4;
    public const uint HandoffEndVirtualAddressExclusive = 0x00FE66E4;

    public const uint DifferenceSubtractRawFileOffset = 0x00BE5AC2;
    public const uint ResidueZeroExtendRawFileOffset = 0x00BE5AC4;
    public const uint EarlyCompareRawFileOffset = 0x00BE5AC7;
    public const uint EarlyBranchRawFileOffset = 0x00BE5ACA;
    public const uint EarlyBranchTargetRawFileOffset = 0x00BE5AF0;
    public const uint EarlyBranchTargetVirtualAddress = 0x00FE66F0;
    public const uint EntryResidueMinimum = 0x8000;
    public const uint EarlyBranchCompareImmediate = 1;

    public const uint ReceiverDwordAtOffset04 = 0x04;
    public const uint ReceiverDwordAtOffset18 = 0x18;
    public const uint ReceiverDwordAtOffset04PushRawFileOffset = 0x00BE5ACC;
    public const uint ResidueMinusOnePushRawFileOffset = 0x00BE5AD4;
    public const uint LiteralPushRawFileOffset = 0x00BE5AD5;
    public const uint ZeroPushRawFileOffset = 0x00BE5ADA;
    public const uint ReceiverDwordAtOffset18PushRawFileOffset = 0x00BE5ADC;
    public const uint HelperCallRawFileOffset = 0x00BE5ADF;
    public const uint HelperCallReturnRawFileOffset = 0x00BE5AE4;
    public const uint HelperCallTargetRawFileOffset = 0x00BE2F50;
    public const uint HelperCallTargetVirtualAddress = 0x00FE3B50;
    public const uint LiteralArgument = 0x01BB8D04;
    public const uint ZeroArgument = 0;
    public const uint ArgumentCount = 5;

    public const string CapturedHandoffBytes =
        "2B CA 0F B7 C1 83 F8 01 76 24 FF 77 04 8B F0 8D 46 FF "
        + "50 68 04 8D BB 01 6A 00 FF 77 18 E8 6C D4 FF FF";

    public static ImmutableArray<UnreliableReceiveResidueHandoffPushKind> PushKindsInInstructionOrder { get; } =
        ImmutableArray.Create(
            UnreliableReceiveResidueHandoffPushKind.ReceiverDwordAtOffset04,
            UnreliableReceiveResidueHandoffPushKind.ResidueMinusOne,
            UnreliableReceiveResidueHandoffPushKind.LiteralArgument,
            UnreliableReceiveResidueHandoffPushKind.ZeroArgument,
            UnreliableReceiveResidueHandoffPushKind.ReceiverDwordAtOffset18);

    public static ImmutableArray<UnreliableReceiveResidueHandoffInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveResidueHandoffInstructionEvidence(
                "Subtract EDX from ECX, then zero-extend the low WORD residue into EAX",
                DifferenceSubtractRawFileOffset,
                RawFileOffsetToVirtualAddress(DifferenceSubtractRawFileOffset),
                "2B CA 0F B7 C1"),
            new UnreliableReceiveResidueHandoffInstructionEvidence(
                "Compare the zero-extended residue with one; unsigned JBE reaches raw 0xBE5AF0",
                EarlyCompareRawFileOffset,
                RawFileOffsetToVirtualAddress(EarlyCompareRawFileOffset),
                "83 F8 01 76 24",
                EarlyBranchTargetRawFileOffset,
                EarlyBranchTargetVirtualAddress),
            new UnreliableReceiveResidueHandoffInstructionEvidence(
                "Push the DWORD at receiver+4 before computing and pushing residue minus one",
                ReceiverDwordAtOffset04PushRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDwordAtOffset04PushRawFileOffset),
                "FF 77 04 8B F0 8D 46 FF 50"),
            new UnreliableReceiveResidueHandoffInstructionEvidence(
                "Push the literal 0x1BB8D04, zero, then the DWORD at receiver+0x18",
                LiteralPushRawFileOffset,
                RawFileOffsetToVirtualAddress(LiteralPushRawFileOffset),
                "68 04 8D BB 01 6A 00 FF 77 18"),
            new UnreliableReceiveResidueHandoffInstructionEvidence(
                "Call raw 0xBE2F50; normal return reaches the exclusive raw 0xBE5AE4 boundary",
                HelperCallRawFileOffset,
                RawFileOffsetToVirtualAddress(HelperCallRawFileOffset),
                "E8 6C D4 FF FF",
                HelperCallTargetRawFileOffset,
                HelperCallTargetVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAtOffset04Address(uint receiverBaseAddress) =>
        unchecked(receiverBaseAddress + ReceiverDwordAtOffset04);

    public static uint ReceiverDwordAtOffset18Address(uint receiverBaseAddress) =>
        unchecked(receiverBaseAddress + ReceiverDwordAtOffset18);

    public static uint RelativeCallTarget(uint instructionRawFileOffset, int displacement) =>
        unchecked(instructionRawFileOffset + 5u + (uint)displacement);

    public static UnreliableReceiveResidueHandoffPlan Project(
        UnreliableReceiveResidueHandoffEntry entry,
        UnreliableReceiveResidueHandoffCapture capture)
    {
        if (!entry.IsValid)
        {
            throw new ArgumentException(
                "The entry must be created with TryCreate and satisfy the masked-delta gate.",
                nameof(entry));
        }

        uint difference32 = unchecked((uint)entry.PayloadWord - entry.ReceiverWord);
        ushort residue = unchecked((ushort)difference32);
        uint receiverDwordAtOffset04Address =
            ReceiverDwordAtOffset04Address(capture.ReceiverBaseAddress);
        uint receiverDwordAtOffset18Address =
            ReceiverDwordAtOffset18Address(capture.ReceiverBaseAddress);
        bool earlyJbeTaken = residue <= EarlyBranchCompareImmediate;

        if (earlyJbeTaken)
        {
            return new UnreliableReceiveResidueHandoffPlan(
                entry,
                capture,
                UnreliableReceiveResidueHandoffOutcome.EarlyJbeTaken,
                receiverDwordAtOffset04Address,
                receiverDwordAtOffset18Address,
                difference32,
                residue,
                EarlyJbeTaken: true,
                ResidueMinusOne: null,
                ReceiverDwordAtOffset04Read: false,
                ReceiverDwordAtOffset18Read: false,
                UnreachedReceiverDwordAtOffset04CaptureProvided: capture.ReceiverDwordAtOffset04.HasValue,
                UnreachedReceiverDwordAtOffset18CaptureProvided: capture.ReceiverDwordAtOffset18.HasValue,
                UnreachedHelperReturnCaptureProvided: capture.HelperReturnedNormally.HasValue,
                ImmutableArray<uint>.Empty,
                ImmutableArray<uint>.Empty,
                HelperCallReached: false,
                HelperReturnedNormally: null,
                HelperReturnBoundaryReached: null,
                SelectedDestinationRawFileOffset: EarlyBranchTargetRawFileOffset,
                StopAtRawFileOffsetExclusive: EarlyBranchTargetRawFileOffset);
        }

        if (capture.ReceiverDwordAtOffset04 is not uint receiverDwordAtOffset04)
        {
            bool laterCaptureProvided =
                capture.ReceiverDwordAtOffset18.HasValue || capture.HelperReturnedNormally.HasValue;
            return new UnreliableReceiveResidueHandoffPlan(
                entry,
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveResidueHandoffOutcome.MissingReceiverOffset04CaptureWithUnreachedLaterCaptures
                    : UnreliableReceiveResidueHandoffOutcome.MissingReceiverOffset04Capture,
                receiverDwordAtOffset04Address,
                receiverDwordAtOffset18Address,
                difference32,
                residue,
                EarlyJbeTaken: false,
                ResidueMinusOne: null,
                ReceiverDwordAtOffset04Read: false,
                ReceiverDwordAtOffset18Read: false,
                UnreachedReceiverDwordAtOffset04CaptureProvided: false,
                UnreachedReceiverDwordAtOffset18CaptureProvided: capture.ReceiverDwordAtOffset18.HasValue,
                UnreachedHelperReturnCaptureProvided: capture.HelperReturnedNormally.HasValue,
                ImmutableArray<uint>.Empty,
                ImmutableArray<uint>.Empty,
                HelperCallReached: false,
                HelperReturnedNormally: null,
                HelperReturnBoundaryReached: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: ReceiverDwordAtOffset04PushRawFileOffset);
        }

        uint residueMinusOne = unchecked((uint)residue - 1);
        var pushedArguments = ImmutableArray.CreateBuilder<uint>((int)ArgumentCount);
        pushedArguments.Add(receiverDwordAtOffset04);
        pushedArguments.Add(residueMinusOne);
        pushedArguments.Add(LiteralArgument);
        pushedArguments.Add(ZeroArgument);

        if (capture.ReceiverDwordAtOffset18 is not uint receiverDwordAtOffset18)
        {
            bool laterCaptureProvided = capture.HelperReturnedNormally.HasValue;
            return new UnreliableReceiveResidueHandoffPlan(
                entry,
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveResidueHandoffOutcome.MissingReceiverOffset18CaptureWithUnreachedHelperReturnCapture
                    : UnreliableReceiveResidueHandoffOutcome.MissingReceiverOffset18Capture,
                receiverDwordAtOffset04Address,
                receiverDwordAtOffset18Address,
                difference32,
                residue,
                EarlyJbeTaken: false,
                residueMinusOne,
                ReceiverDwordAtOffset04Read: true,
                ReceiverDwordAtOffset18Read: false,
                UnreachedReceiverDwordAtOffset04CaptureProvided: false,
                UnreachedReceiverDwordAtOffset18CaptureProvided: false,
                UnreachedHelperReturnCaptureProvided: laterCaptureProvided,
                pushedArguments.ToImmutable(),
                ImmutableArray<uint>.Empty,
                HelperCallReached: false,
                HelperReturnedNormally: null,
                HelperReturnBoundaryReached: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: ReceiverDwordAtOffset18PushRawFileOffset);
        }

        pushedArguments.Add(receiverDwordAtOffset18);
        ImmutableArray<uint> pushed = pushedArguments.ToImmutable();
        ImmutableArray<uint> calleeArguments = pushed.Reverse().ToImmutableArray();
        if (capture.HelperReturnedNormally is not bool helperReturnedNormally)
        {
            return new UnreliableReceiveResidueHandoffPlan(
                entry,
                capture,
                UnreliableReceiveResidueHandoffOutcome.MissingHelperReturnCapture,
                receiverDwordAtOffset04Address,
                receiverDwordAtOffset18Address,
                difference32,
                residue,
                EarlyJbeTaken: false,
                residueMinusOne,
                ReceiverDwordAtOffset04Read: true,
                ReceiverDwordAtOffset18Read: true,
                UnreachedReceiverDwordAtOffset04CaptureProvided: false,
                UnreachedReceiverDwordAtOffset18CaptureProvided: false,
                UnreachedHelperReturnCaptureProvided: false,
                pushed,
                calleeArguments,
                HelperCallReached: true,
                HelperReturnedNormally: null,
                HelperReturnBoundaryReached: null,
                SelectedDestinationRawFileOffset: null,
                StopAtRawFileOffsetExclusive: HelperCallReturnRawFileOffset);
        }

        return new UnreliableReceiveResidueHandoffPlan(
            entry,
            capture,
            helperReturnedNormally
                ? UnreliableReceiveResidueHandoffOutcome.HelperReturnedToExclusiveBoundary
                : UnreliableReceiveResidueHandoffOutcome.HelperDidNotReturnNormally,
            receiverDwordAtOffset04Address,
            receiverDwordAtOffset18Address,
            difference32,
            residue,
            EarlyJbeTaken: false,
            residueMinusOne,
            ReceiverDwordAtOffset04Read: true,
            ReceiverDwordAtOffset18Read: true,
            UnreachedReceiverDwordAtOffset04CaptureProvided: false,
            UnreachedReceiverDwordAtOffset18CaptureProvided: false,
            UnreachedHelperReturnCaptureProvided: false,
            pushed,
            calleeArguments,
            HelperCallReached: true,
            helperReturnedNormally,
            HelperReturnBoundaryReached: helperReturnedNormally,
            SelectedDestinationRawFileOffset: null,
            StopAtRawFileOffsetExclusive: HelperCallReturnRawFileOffset);
    }
}
