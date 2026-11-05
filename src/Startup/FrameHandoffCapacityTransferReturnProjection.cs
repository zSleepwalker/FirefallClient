using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityTransferReturnCapture(
    bool ValidatedNormalEntry,
    uint EntryEbx,
    uint EntryEbp,
    uint EntryEdi,
    uint EntryEsi,
    uint? HelperReturnEax,
    uint? Receiver4,
    uint? FrameMinus8);

public enum FrameHandoffCapacityTransferReturnOutcome
{
    EntryNotValidated,
    MissingReceiver4Capture,
    MissingHelperReturnEaxCapture,
    MissingFrameMinus8Capture,
    FrameMinus8UnsignedGreaterThanEntryEsi,
    FrameMinus8UnsignedLessThanOrEqualToEntryEsi
}

public enum FrameHandoffCapacityTransferReturnMemoryAccessKind
{
    Read,
    Write
}

public readonly record struct FrameHandoffCapacityTransferReturnRead(
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct FrameHandoffCapacityTransferReturnMemoryAccess(
    FrameHandoffCapacityTransferReturnMemoryAccessKind Kind,
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct FrameHandoffCapacityTransferReturnInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct FrameHandoffCapacityTransferReturnPlan(
    FrameHandoffCapacityTransferReturnCapture Capture,
    FrameHandoffCapacityTransferReturnOutcome Outcome,
    ImmutableArray<FrameHandoffCapacityTransferReturnRead> Reads,
    ImmutableArray<FrameHandoffCapacityTransferReturnMemoryAccess> MemoryAccesses,
    bool Receiver4ReadReached,
    bool Receiver4Captured,
    bool StackCleanupReached,
    bool EdxLeaReached,
    uint? EdxAfterLea,
    bool FrameMinus8ReadReached,
    bool FrameMinus8Captured,
    bool FrameMinus10StoreReached,
    uint? FrameMinus10StoreAddress,
    uint? FrameMinus10StoreValue,
    bool CompareReached,
    bool? FrameMinus8UnsignedGreaterThanEntryEsi,
    bool? BranchTaken,
    uint? SelectedDestinationRawFileOffset,
    bool UnreachedReceiver4CaptureProvided,
    bool UnreachedHelperReturnEaxCaptureProvided,
    bool UnreachedFrameMinus8CaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the bounded capacity-transfer helper return join through its unsigned branch.
/// </summary>
public static class FrameHandoffCapacityTransferReturnProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6BFD;
    public const uint EntryVirtualAddress = 0x00FE77FD;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE6C10;
    public const uint NormalBoundaryVirtualAddressExclusive = 0x00FE7810;
    public const uint AboveCapacityDestinationRawFileOffset = 0x00BE6C57;
    public const uint AboveCapacityDestinationVirtualAddress = 0x00FE7857;
    public const uint Receiver4Offset = 4;
    public const uint FrameMinus8Offset = 8;
    public const uint FrameMinus10Offset = 0x10;
    public const uint StackCleanupBytes = 0x0C;

    public const uint Receiver4ReadRawFileOffset = 0x00BE6BFD;
    public const uint StackCleanupRawFileOffset = 0x00BE6C00;
    public const uint EdxLeaRawFileOffset = 0x00BE6C03;
    public const uint FrameMinus8ReadRawFileOffset = 0x00BE6C06;
    public const uint FrameMinus10StoreRawFileOffset = 0x00BE6C09;
    public const uint CompareRawFileOffset = 0x00BE6C0C;
    public const uint AboveCapacityBranchRawFileOffset = 0x00BE6C0E;

    public const string CapturedWindowBytes =
        "8B 4B 04 83 C4 0C 8D 14 07 8B 7D F8 89 55 F0 3B FE 77 47";

    public static ImmutableArray<FrameHandoffCapacityTransferReturnInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Freshly read [EBX+4] into ECX before stack cleanup",
                Receiver4ReadRawFileOffset, "8B 4B 04"),
            Instruction("Clean up 12 stack bytes",
                StackCleanupRawFileOffset, "83 C4 0C"),
            Instruction("Form EDX as entry EDI plus helper-return EAX with DWORD wraparound",
                EdxLeaRawFileOffset, "8D 14 07"),
            Instruction("Independently read [EBP-8] into EDI",
                FrameMinus8ReadRawFileOffset, "8B 7D F8"),
            Instruction("Store EDX to [EBP-0x10]",
                FrameMinus10StoreRawFileOffset, "89 55 F0"),
            Instruction("Compare the loaded EDI value against independently captured entry ESI",
                CompareRawFileOffset, "3B FE"),
            Instruction("Unsigned JA to BE6C57; otherwise continue at BE6C10",
                AboveCapacityBranchRawFileOffset, "77 47"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint Receiver4ReadAddress(uint entryEbx) =>
        unchecked(entryEbx + Receiver4Offset);

    public static uint FrameMinus8Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus8Offset);

    public static uint FrameMinus10Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus10Offset);

    public static FrameHandoffCapacityTransferReturnPlan Project(
        FrameHandoffCapacityTransferReturnCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<FrameHandoffCapacityTransferReturnRead>();
        var accesses =
            ImmutableArray.CreateBuilder<FrameHandoffCapacityTransferReturnMemoryAccess>();

        if (!capture.ValidatedNormalEntry)
        {
            return Stop(capture, FrameHandoffCapacityTransferReturnOutcome.EntryNotValidated,
                reads, accesses, EntryRawFileOffset,
                unreachedReceiver4CaptureProvided: capture.Receiver4.HasValue,
                unreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue,
                unreachedFrameMinus8CaptureProvided: capture.FrameMinus8.HasValue);
        }

        uint receiver4Address = Receiver4ReadAddress(capture.EntryEbx);
        reads.Add(new(Receiver4ReadRawFileOffset, receiver4Address, capture.Receiver4));
        accesses.Add(new(FrameHandoffCapacityTransferReturnMemoryAccessKind.Read,
            Receiver4ReadRawFileOffset, receiver4Address, capture.Receiver4));
        if (capture.Receiver4 is not uint)
        {
            return Stop(capture, FrameHandoffCapacityTransferReturnOutcome.MissingReceiver4Capture,
                reads, accesses, Receiver4ReadRawFileOffset,
                receiver4ReadReached: true,
                unreachedHelperReturnEaxCaptureProvided: capture.HelperReturnEax.HasValue,
                unreachedFrameMinus8CaptureProvided: capture.FrameMinus8.HasValue);
        }

        if (capture.HelperReturnEax is not uint helperReturnEax)
        {
            return Stop(capture,
                FrameHandoffCapacityTransferReturnOutcome.MissingHelperReturnEaxCapture,
                reads, accesses, EdxLeaRawFileOffset,
                receiver4ReadReached: true,
                receiver4Captured: true,
                stackCleanupReached: true,
                unreachedFrameMinus8CaptureProvided: capture.FrameMinus8.HasValue);
        }

        uint edxAfterLea = unchecked(capture.EntryEdi + helperReturnEax);
        uint frameMinus8Address = FrameMinus8Address(capture.EntryEbp);
        reads.Add(new(FrameMinus8ReadRawFileOffset, frameMinus8Address, capture.FrameMinus8));
        accesses.Add(new(FrameHandoffCapacityTransferReturnMemoryAccessKind.Read,
            FrameMinus8ReadRawFileOffset, frameMinus8Address, capture.FrameMinus8));
        if (capture.FrameMinus8 is not uint frameMinus8)
        {
            return Stop(capture,
                FrameHandoffCapacityTransferReturnOutcome.MissingFrameMinus8Capture,
                reads, accesses, FrameMinus8ReadRawFileOffset,
                receiver4ReadReached: true,
                receiver4Captured: true,
                stackCleanupReached: true,
                edxLeaReached: true,
                edxAfterLea: edxAfterLea,
                frameMinus8ReadReached: true);
        }

        uint frameMinus10Address = FrameMinus10Address(capture.EntryEbp);
        accesses.Add(new(FrameHandoffCapacityTransferReturnMemoryAccessKind.Write,
            FrameMinus10StoreRawFileOffset, frameMinus10Address, edxAfterLea));
        bool aboveCapacity = frameMinus8 > capture.EntryEsi;
        uint destination = aboveCapacity
            ? AboveCapacityDestinationRawFileOffset
            : NormalBoundaryRawFileOffsetExclusive;
        return BuildPlan(
            capture,
            aboveCapacity
                ? FrameHandoffCapacityTransferReturnOutcome
                    .FrameMinus8UnsignedGreaterThanEntryEsi
                : FrameHandoffCapacityTransferReturnOutcome
                    .FrameMinus8UnsignedLessThanOrEqualToEntryEsi,
            reads, accesses,
            receiver4ReadReached: true,
            receiver4Captured: true,
            stackCleanupReached: true,
            edxLeaReached: true,
            edxAfterLea: edxAfterLea,
            frameMinus8ReadReached: true,
            frameMinus8Captured: true,
            frameMinus10StoreReached: true,
            frameMinus10StoreAddress: frameMinus10Address,
            frameMinus10StoreValue: edxAfterLea,
            compareReached: true,
            frameMinus8UnsignedGreaterThanEntryEsi: aboveCapacity,
            branchTaken: aboveCapacity,
            selectedDestinationRawFileOffset: destination,
            stopAt: destination);
    }

    private static FrameHandoffCapacityTransferReturnInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(observation, rawFileOffset, rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset), bytes);

    private static FrameHandoffCapacityTransferReturnPlan Stop(
        FrameHandoffCapacityTransferReturnCapture capture,
        FrameHandoffCapacityTransferReturnOutcome outcome,
        ImmutableArray<FrameHandoffCapacityTransferReturnRead>.Builder reads,
        ImmutableArray<FrameHandoffCapacityTransferReturnMemoryAccess>.Builder accesses,
        uint stopAt,
        bool receiver4ReadReached = false,
        bool receiver4Captured = false,
        bool stackCleanupReached = false,
        bool edxLeaReached = false,
        uint? edxAfterLea = null,
        bool frameMinus8ReadReached = false,
        bool frameMinus8Captured = false,
        bool unreachedReceiver4CaptureProvided = false,
        bool unreachedHelperReturnEaxCaptureProvided = false,
        bool unreachedFrameMinus8CaptureProvided = false) =>
        BuildPlan(capture, outcome, reads, accesses,
            receiver4ReadReached, receiver4Captured, stackCleanupReached,
            edxLeaReached, edxAfterLea, frameMinus8ReadReached, frameMinus8Captured,
            frameMinus10StoreReached: false,
            frameMinus10StoreAddress: null,
            frameMinus10StoreValue: null,
            compareReached: false,
            frameMinus8UnsignedGreaterThanEntryEsi: null,
            branchTaken: null,
            selectedDestinationRawFileOffset: null,
            unreachedReceiver4CaptureProvided: unreachedReceiver4CaptureProvided,
            unreachedHelperReturnEaxCaptureProvided: unreachedHelperReturnEaxCaptureProvided,
            unreachedFrameMinus8CaptureProvided: unreachedFrameMinus8CaptureProvided,
            stopAt: stopAt);

    private static FrameHandoffCapacityTransferReturnPlan BuildPlan(
        FrameHandoffCapacityTransferReturnCapture capture,
        FrameHandoffCapacityTransferReturnOutcome outcome,
        ImmutableArray<FrameHandoffCapacityTransferReturnRead>.Builder reads,
        ImmutableArray<FrameHandoffCapacityTransferReturnMemoryAccess>.Builder accesses,
        bool receiver4ReadReached,
        bool receiver4Captured,
        bool stackCleanupReached,
        bool edxLeaReached,
        uint? edxAfterLea,
        bool frameMinus8ReadReached,
        bool frameMinus8Captured,
        bool frameMinus10StoreReached,
        uint? frameMinus10StoreAddress,
        uint? frameMinus10StoreValue,
        bool compareReached,
        bool? frameMinus8UnsignedGreaterThanEntryEsi,
        bool? branchTaken,
        uint? selectedDestinationRawFileOffset,
        bool unreachedReceiver4CaptureProvided = false,
        bool unreachedHelperReturnEaxCaptureProvided = false,
        bool unreachedFrameMinus8CaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            reads.ToImmutable(),
            accesses.ToImmutable(),
            receiver4ReadReached,
            receiver4Captured,
            stackCleanupReached,
            edxLeaReached,
            edxAfterLea,
            frameMinus8ReadReached,
            frameMinus8Captured,
            frameMinus10StoreReached,
            frameMinus10StoreAddress,
            frameMinus10StoreValue,
            compareReached,
            frameMinus8UnsignedGreaterThanEntryEsi,
            branchTaken,
            selectedDestinationRawFileOffset,
            unreachedReceiver4CaptureProvided,
            unreachedHelperReturnEaxCaptureProvided,
            unreachedFrameMinus8CaptureProvided,
            stopAt);
}
