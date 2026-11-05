using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacitySeedCapture(
    uint EntryFrameEbp,
    uint ReceiverEcx,
    uint? ReceiverPlus08);

public enum FrameHandoffCapacitySeedOutcome
{
    MissingReceiverPlus08Capture,
    ZeroReadKeepsInitialSeed,
    NonzeroReadReplacesSeed
}

public readonly record struct FrameHandoffCapacitySeedPlan(
    FrameHandoffCapacitySeedCapture Capture,
    FrameHandoffCapacitySeedOutcome Outcome,
    uint EbxReceiverFromEntryEcx,
    bool ReceiverProvenancePreserved,
    bool ReceiverFrameStoreProjected,
    uint ReceiverFrameStoreAddress,
    uint ReceiverFrameStoreValue,
    bool EsiSeedEstablished,
    uint EsiSeedBeforeReceiverRead,
    uint ReceiverPlus08ReadAddress,
    bool ReceiverPlus08ReadReached,
    uint? ReceiverPlus08ReadValue,
    bool ReceiverPlus08Captured,
    bool TestEdxEdxReached,
    bool? ZeroFlag,
    bool ConditionalMoveReached,
    bool? ConditionalMoveCopiedEdxToEsi,
    uint? EsiAfterConditionalMove,
    bool JumpToJoinReached,
    uint? ReachedJoinRawFileOffset,
    uint? EsiAtJoin,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct FrameHandoffCapacitySeedInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the helper entry through its first join and maps raw offsets through the
/// sample's file-backed PE code section.
/// </summary>
public static class FrameHandoffCapacitySeedProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6B50;
    public const uint EntryVirtualAddress = 0x00FE7750;
    public const uint ReceiverPlus08ReadRawFileOffset = 0x00BE6B63;
    public const uint ReceiverPlus08ReadVirtualAddress = 0x00FE7763;
    public const uint JoinRawFileOffset = 0x00BE6B70;
    public const uint JoinVirtualAddress = 0x00FE7770;
    public const uint EndRawFileOffsetExclusive = JoinRawFileOffset;
    public const uint ReceiverPlus08Offset = 0x08;
    public const uint ReceiverFrameStoreOffset = 0x14;
    public const uint InitialEsiSeed = 1;

    public const string CapturedWindowBytes =
        "55 8B EC 83 EC 14 53 8B D9 56 BE 01 00 00 00 89 5D EC "
        + "57 8B 53 08 85 D2 0F 45 F2 EB 03 8D 49 00";

    public static ImmutableArray<FrameHandoffCapacitySeedInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Save EBP",
                EntryRawFileOffset,
                RawFileOffsetToVirtualAddress(EntryRawFileOffset),
                "55"),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Establish the EBP frame",
                0x00BE6B51,
                RawFileOffsetToVirtualAddress(0x00BE6B51),
                "8B EC"),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Reserve 0x14 stack bytes",
                0x00BE6B53,
                RawFileOffsetToVirtualAddress(0x00BE6B53),
                "83 EC 14"),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Save EBX",
                0x00BE6B56,
                RawFileOffsetToVirtualAddress(0x00BE6B56),
                "53"),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Copy entry ECX to EBX",
                0x00BE6B57,
                RawFileOffsetToVirtualAddress(0x00BE6B57),
                "8B D9"),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Save ESI",
                0x00BE6B59,
                RawFileOffsetToVirtualAddress(0x00BE6B59),
                "56"),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Establish the independent ESI seed of one",
                0x00BE6B5A,
                RawFileOffsetToVirtualAddress(0x00BE6B5A),
                "BE 01 00 00 00"),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Store the receiver provenance in the EBP frame",
                0x00BE6B5F,
                RawFileOffsetToVirtualAddress(0x00BE6B5F),
                "89 5D EC"),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Save EDI",
                0x00BE6B62,
                RawFileOffsetToVirtualAddress(0x00BE6B62),
                "57"),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Independently read DWORD [EBX+8] into EDX",
                ReceiverPlus08ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverPlus08ReadRawFileOffset),
                "8B 53 08"),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Test EDX against zero",
                0x00BE6B66,
                RawFileOffsetToVirtualAddress(0x00BE6B66),
                "85 D2"),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Copy EDX to ESI only when EDX is nonzero",
                0x00BE6B68,
                RawFileOffsetToVirtualAddress(0x00BE6B68),
                "0F 45 F2"),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Jump to the join, skipping the following three raw bytes",
                0x00BE6B6B,
                RawFileOffsetToVirtualAddress(0x00BE6B6B),
                "EB 03",
                JoinRawFileOffset,
                JoinVirtualAddress),
            new FrameHandoffCapacitySeedInstructionEvidence(
                "Flat-IDA decodes the skipped bytes as LEA ECX,[ECX+0]",
                0x00BE6B6D,
                RawFileOffsetToVirtualAddress(0x00BE6B6D),
                "8D 49 00"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverAddress(uint receiverBaseAddress, uint fieldOffset) =>
        unchecked(receiverBaseAddress + fieldOffset);

    public static FrameHandoffCapacitySeedPlan Project(
        FrameHandoffCapacitySeedCapture capture)
    {
        uint receiverPlus08Address =
            ReceiverAddress(capture.ReceiverEcx, ReceiverPlus08Offset);

        if (capture.ReceiverPlus08 is not uint receiverPlus08)
        {
            return new(
                capture,
                FrameHandoffCapacitySeedOutcome.MissingReceiverPlus08Capture,
                capture.ReceiverEcx,
                true,
                true,
                unchecked(capture.EntryFrameEbp - ReceiverFrameStoreOffset),
                capture.ReceiverEcx,
                true,
                InitialEsiSeed,
                receiverPlus08Address,
                true,
                null,
                false,
                false,
                null,
                false,
                null,
                null,
                false,
                null,
                null,
                ReceiverPlus08ReadRawFileOffset);
        }

        bool isZero = receiverPlus08 == 0;
        uint esiAtJoin = isZero ? InitialEsiSeed : receiverPlus08;

        return new(
            capture,
            isZero
                ? FrameHandoffCapacitySeedOutcome.ZeroReadKeepsInitialSeed
                : FrameHandoffCapacitySeedOutcome.NonzeroReadReplacesSeed,
            capture.ReceiverEcx,
            true,
            true,
            unchecked(capture.EntryFrameEbp - ReceiverFrameStoreOffset),
            capture.ReceiverEcx,
            true,
            InitialEsiSeed,
            receiverPlus08Address,
            true,
            receiverPlus08,
            true,
            true,
            isZero,
            true,
            !isZero,
            esiAtJoin,
            true,
            JoinRawFileOffset,
            esiAtJoin,
            EndRawFileOffsetExclusive);
    }
}
