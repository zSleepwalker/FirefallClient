using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityFinalReturnCapture(
    bool ValidatedNormalTail,
    uint EntryEdi,
    uint EntryEsi,
    uint EntryEbx,
    uint EntryEbp,
    uint? Receiver8Old);

public enum FrameHandoffCapacityFinalReturnOutcome
{
    TailNotValidated,
    MissingReceiver8OldCapture,
    ReturnedNormally
}

public enum FrameHandoffCapacityFinalReturnMemoryAccessKind
{
    Read,
    Write
}

public readonly record struct FrameHandoffCapacityFinalReturnMemoryAccess(
    FrameHandoffCapacityFinalReturnMemoryAccessKind Kind,
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct FrameHandoffCapacityFinalReturnInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct FrameHandoffCapacityFinalReturnPlan(
    FrameHandoffCapacityFinalReturnCapture Capture,
    FrameHandoffCapacityFinalReturnOutcome Outcome,
    ImmutableArray<FrameHandoffCapacityFinalReturnMemoryAccess> MemoryAccesses,
    bool Receiver8ReadReached,
    bool Receiver8OldCaptured,
    uint? Receiver8ReadAddress,
    uint? Receiver8ValueBeforeAdd,
    bool Receiver8WriteReached,
    uint? Receiver8WriteAddress,
    uint? Receiver8ValueAfterAdd,
    bool Receiver4StoreReached,
    uint? Receiver4StoreAddress,
    uint? Receiver4StoreValue,
    ImmutableArray<string> StackPopRegisters,
    bool EspSetFromEbpReached,
    bool ReturnReached,
    ushort? ReturnImmediateBytes,
    bool UnreachedReceiver8OldCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the validated capacity tail's final receiver update and frame return.
/// </summary>
public static class FrameHandoffCapacityFinalReturnProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6CB1;
    public const uint EntryVirtualAddress = 0x00FE78B1;
    public const uint ReturnBoundaryRawFileOffsetExclusive = 0x00BE6CC0;
    public const uint ReturnBoundaryVirtualAddressExclusive = 0x00FE78C0;

    public const uint Receiver8Offset = 8;
    public const uint Receiver4Offset = 4;
    public const ushort ReturnImmediateBytes = 4;

    public const uint Receiver8AddRawFileOffset = 0x00BE6CB1;
    public const uint Receiver4StoreRawFileOffset = 0x00BE6CB4;
    public const uint PopEdiRawFileOffset = 0x00BE6CB7;
    public const uint PopEsiRawFileOffset = 0x00BE6CB8;
    public const uint PopEbxRawFileOffset = 0x00BE6CB9;
    public const uint EspFromEbpRawFileOffset = 0x00BE6CBA;
    public const uint PopEbpRawFileOffset = 0x00BE6CBC;
    public const uint ReturnRawFileOffset = 0x00BE6CBD;

    public const string CapturedWindowBytes =
        "01 77 08 89 5F 04 5F 5E 5B 8B E5 5D C2 04 00";

    public static ImmutableArray<FrameHandoffCapacityFinalReturnInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Add entry ESI to the DWORD at [entry EDI+8]",
                Receiver8AddRawFileOffset, "01 77 08"),
            Instruction("Store entry EBX at [entry EDI+4]",
                Receiver4StoreRawFileOffset, "89 5F 04"),
            Instruction("Pop EDI", PopEdiRawFileOffset, "5F"),
            Instruction("Pop ESI", PopEsiRawFileOffset, "5E"),
            Instruction("Pop EBX", PopEbxRawFileOffset, "5B"),
            Instruction("Set ESP from EBP", EspFromEbpRawFileOffset, "8B E5"),
            Instruction("Pop EBP", PopEbpRawFileOffset, "5D"),
            Instruction("Return and discard four argument bytes",
                ReturnRawFileOffset, "C2 04 00"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint Receiver8Address(uint entryEdi) =>
        unchecked(entryEdi + Receiver8Offset);

    public static uint Receiver4Address(uint entryEdi) =>
        unchecked(entryEdi + Receiver4Offset);

    public static FrameHandoffCapacityFinalReturnPlan Project(
        FrameHandoffCapacityFinalReturnCapture capture)
    {
        var accesses =
            ImmutableArray.CreateBuilder<FrameHandoffCapacityFinalReturnMemoryAccess>();

        if (!capture.ValidatedNormalTail)
        {
            return Stop(
                capture,
                FrameHandoffCapacityFinalReturnOutcome.TailNotValidated,
                accesses,
                EntryRawFileOffset,
                unreachedReceiver8OldCaptureProvided: capture.Receiver8Old.HasValue);
        }

        uint receiver8Address = Receiver8Address(capture.EntryEdi);
        accesses.Add(new(
            FrameHandoffCapacityFinalReturnMemoryAccessKind.Read,
            Receiver8AddRawFileOffset,
            receiver8Address,
            capture.Receiver8Old));
        if (capture.Receiver8Old is not uint receiver8Old)
        {
            return Stop(
                capture,
                FrameHandoffCapacityFinalReturnOutcome.MissingReceiver8OldCapture,
                accesses,
                Receiver8AddRawFileOffset,
                receiver8ReadReached: true,
                receiver8ReadAddress: receiver8Address);
        }

        uint receiver8AfterAdd = unchecked(receiver8Old + capture.EntryEsi);
        accesses.Add(new(
            FrameHandoffCapacityFinalReturnMemoryAccessKind.Write,
            Receiver8AddRawFileOffset,
            receiver8Address,
            receiver8AfterAdd));

        uint receiver4Address = Receiver4Address(capture.EntryEdi);
        accesses.Add(new(
            FrameHandoffCapacityFinalReturnMemoryAccessKind.Write,
            Receiver4StoreRawFileOffset,
            receiver4Address,
            capture.EntryEbx));

        return new(
            capture,
            FrameHandoffCapacityFinalReturnOutcome.ReturnedNormally,
            accesses.ToImmutable(),
            Receiver8ReadReached: true,
            Receiver8OldCaptured: true,
            Receiver8ReadAddress: receiver8Address,
            Receiver8ValueBeforeAdd: receiver8Old,
            Receiver8WriteReached: true,
            Receiver8WriteAddress: receiver8Address,
            Receiver8ValueAfterAdd: receiver8AfterAdd,
            Receiver4StoreReached: true,
            Receiver4StoreAddress: receiver4Address,
            Receiver4StoreValue: capture.EntryEbx,
            StackPopRegisters: ImmutableArray.Create("EDI", "ESI", "EBX", "EBP"),
            EspSetFromEbpReached: true,
            ReturnReached: true,
            ReturnImmediateBytes: ReturnImmediateBytes,
            UnreachedReceiver8OldCaptureProvided: false,
            StopAtRawFileOffsetExclusive: ReturnBoundaryRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityFinalReturnInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(observation, rawFileOffset, rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset), bytes);

    private static FrameHandoffCapacityFinalReturnPlan Stop(
        FrameHandoffCapacityFinalReturnCapture capture,
        FrameHandoffCapacityFinalReturnOutcome outcome,
        ImmutableArray<FrameHandoffCapacityFinalReturnMemoryAccess>.Builder accesses,
        uint stopAt,
        bool receiver8ReadReached = false,
        bool receiver8OldCaptured = false,
        uint? receiver8ReadAddress = null,
        uint? receiver8ValueBeforeAdd = null,
        bool unreachedReceiver8OldCaptureProvided = false) =>
        new(
            capture,
            outcome,
            accesses.ToImmutable(),
            receiver8ReadReached,
            receiver8OldCaptured,
            receiver8ReadAddress,
            receiver8ValueBeforeAdd,
            Receiver8WriteReached: false,
            Receiver8WriteAddress: null,
            Receiver8ValueAfterAdd: null,
            Receiver4StoreReached: false,
            Receiver4StoreAddress: null,
            Receiver4StoreValue: null,
            StackPopRegisters: ImmutableArray<string>.Empty,
            EspSetFromEbpReached: false,
            ReturnReached: false,
            ReturnImmediateBytes: null,
            UnreachedReceiver8OldCaptureProvided: unreachedReceiver8OldCaptureProvided,
            StopAtRawFileOffsetExclusive: stopAt);
}
