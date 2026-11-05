using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacitySecondTransferCapture(
    uint EntryEdi,
    uint EntryEsi,
    uint EntryEcx,
    uint EntryEbp,
    uint? CallCellDword,
    uint? FrameMinus10Dword,
    bool? CallReturnedNormally);

public enum FrameHandoffCapacitySecondTransferOutcome
{
    MissingCallCellCapture,
    MissingFrameMinus10Capture,
    MissingCallReturnCapture,
    CallDidNotReturnNormally,
    ReturnedNormally
}

public enum FrameHandoffCapacitySecondTransferMemoryAccessKind
{
    Read,
    Write
}

public readonly record struct FrameHandoffCapacitySecondTransferRead(
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct FrameHandoffCapacitySecondTransferMemoryAccess(
    FrameHandoffCapacitySecondTransferMemoryAccessKind Kind,
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct FrameHandoffCapacitySecondTransferFrameWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct FrameHandoffCapacitySecondTransferPush(
    uint RawFileOffset,
    string Source,
    uint Value);

public readonly record struct FrameHandoffCapacitySecondTransferInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct FrameHandoffCapacitySecondTransferPlan(
    FrameHandoffCapacitySecondTransferCapture Capture,
    FrameHandoffCapacitySecondTransferOutcome Outcome,
    ImmutableArray<FrameHandoffCapacitySecondTransferRead> Reads,
    ImmutableArray<FrameHandoffCapacitySecondTransferMemoryAccess> MemoryAccesses,
    ImmutableArray<FrameHandoffCapacitySecondTransferFrameWrite> FrameWrites,
    ImmutableArray<FrameHandoffCapacitySecondTransferPush> Pushes,
    bool EaxLeaReached,
    uint? EaxAfterLea,
    bool EdxMoveReached,
    uint? EdxAfterMove,
    bool ArithmeticShiftRightReached,
    uint? EaxAfterArithmeticShiftRight,
    bool EdxSubtractReached,
    uint? EdxAfterSubtract,
    bool FrameMinusCWriteReached,
    uint? FrameMinusCWriteAddress,
    uint? FrameMinusCWriteValue,
    bool EbxLeaReached,
    uint? EbxAfterLea,
    bool CallCellReadReached,
    bool CallCellCaptured,
    uint CallCellAddress,
    uint? CallTargetAddress,
    bool EbxPushReached,
    bool EcxPushReached,
    bool FrameMinus10ReadReached,
    bool FrameMinus10Captured,
    bool FrameMinus10PushReached,
    bool CallReached,
    bool? CallReturnedNormally,
    bool NormalBoundaryReached,
    bool UnreachedFrameMinus10CaptureProvided,
    bool UnreachedCallReturnCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the bounded second capacity-transfer through its indirect call return.
/// </summary>
public static class FrameHandoffCapacitySecondTransferProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6C10;
    public const uint EntryVirtualAddress = 0x00FE7810;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE6C34;
    public const uint NormalBoundaryVirtualAddressExclusive = 0x00FE7834;
    public const uint CallCellAddress = 0x01AC489C;
    public const uint FrameMinusCOffset = 0x0C;
    public const uint FrameMinus10Offset = 0x10;

    public const uint EaxLeaRawFileOffset = 0x00BE6C10;
    public const uint EdxMoveRawFileOffset = 0x00BE6C17;
    public const uint ArithmeticShiftRightRawFileOffset = 0x00BE6C19;
    public const uint EdxSubtractRawFileOffset = 0x00BE6C1C;
    public const uint FrameMinusCWriteRawFileOffset = 0x00BE6C1E;
    public const uint EbxLeaRawFileOffset = 0x00BE6C21;
    public const uint CallCellReadRawFileOffset = 0x00BE6C28;
    public const uint PushEbxRawFileOffset = 0x00BE6C2D;
    public const uint PushEcxRawFileOffset = 0x00BE6C2E;
    public const uint FrameMinus10PushRawFileOffset = 0x00BE6C2F;
    public const uint CallRawFileOffset = 0x00BE6C32;

    public const string CapturedWindowBytes =
        "8D 04 BD 00 00 00 00 8B D6 C1 F8 02 2B D7 89 55 F4 "
        + "8D 1C 85 00 00 00 00 A1 9C 48 AC 01 53 51 FF 75 F0 FF D0";

    public static ImmutableArray<FrameHandoffCapacitySecondTransferInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Form EAX as entry EDI times four with DWORD wraparound",
                EaxLeaRawFileOffset, "8D 04 BD 00 00 00 00"),
            Instruction("Copy entry ESI into EDX",
                EdxMoveRawFileOffset, "8B D6"),
            Instruction("Arithmetic-shift EAX right by two",
                ArithmeticShiftRightRawFileOffset, "C1 F8 02"),
            Instruction("Subtract entry EDI from EDX with DWORD wraparound",
                EdxSubtractRawFileOffset, "2B D7"),
            Instruction("Write EDX to [EBP-0xC]",
                FrameMinusCWriteRawFileOffset, "89 55 F4"),
            Instruction("Form EBX as EAX times four with DWORD wraparound",
                EbxLeaRawFileOffset, "8D 1C 85 00 00 00 00"),
            Instruction("Independently read the call-target DWORD from 0x01AC489C into EAX",
                CallCellReadRawFileOffset, "A1 9C 48 AC 01"),
            Instruction("Push EBX", PushEbxRawFileOffset, "53"),
            Instruction("Push entry ECX", PushEcxRawFileOffset, "51"),
            Instruction("Independently read and push [EBP-0x10]",
                FrameMinus10PushRawFileOffset, "FF 75 F0"),
            Instruction("Call EAX; target behavior is opaque",
                CallRawFileOffset, "FF D0"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FrameMinusCAddress(uint entryEbp) =>
        unchecked(entryEbp - FrameMinusCOffset);

    public static uint FrameMinus10Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus10Offset);

    public static uint ArithmeticShiftRightByTwo(uint value) =>
        unchecked((uint)((int)value >> 2));

    public static FrameHandoffCapacitySecondTransferPlan Project(
        FrameHandoffCapacitySecondTransferCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<FrameHandoffCapacitySecondTransferRead>();
        var accesses =
            ImmutableArray.CreateBuilder<FrameHandoffCapacitySecondTransferMemoryAccess>();
        var writes = ImmutableArray.CreateBuilder<FrameHandoffCapacitySecondTransferFrameWrite>();
        var pushes = ImmutableArray.CreateBuilder<FrameHandoffCapacitySecondTransferPush>();

        uint eaxAfterLea = unchecked(capture.EntryEdi * 4);
        uint edxAfterMove = capture.EntryEsi;
        uint eaxAfterArithmeticShiftRight = ArithmeticShiftRightByTwo(eaxAfterLea);
        uint edxAfterSubtract = unchecked(edxAfterMove - capture.EntryEdi);
        uint frameMinusCAddress = FrameMinusCAddress(capture.EntryEbp);
        writes.Add(new(FrameMinusCWriteRawFileOffset, frameMinusCAddress, edxAfterSubtract));
        accesses.Add(new(FrameHandoffCapacitySecondTransferMemoryAccessKind.Write,
            FrameMinusCWriteRawFileOffset, frameMinusCAddress, edxAfterSubtract));
        uint ebxAfterLea = unchecked(eaxAfterArithmeticShiftRight * 4);

        reads.Add(new(CallCellReadRawFileOffset, CallCellAddress, capture.CallCellDword));
        accesses.Add(new(FrameHandoffCapacitySecondTransferMemoryAccessKind.Read,
            CallCellReadRawFileOffset, CallCellAddress, capture.CallCellDword));
        if (capture.CallCellDword is not uint)
        {
            return Stop(capture, FrameHandoffCapacitySecondTransferOutcome.MissingCallCellCapture,
                reads, accesses, writes, pushes, CallCellReadRawFileOffset,
                eaxAfterLea, edxAfterMove, eaxAfterArithmeticShiftRight, edxAfterSubtract,
                frameMinusCAddress, ebxAfterLea,
                callCellReadReached: true,
                unreachedFrameMinus10CaptureProvided: capture.FrameMinus10Dword.HasValue,
                unreachedCallReturnCaptureProvided: capture.CallReturnedNormally.HasValue);
        }

        pushes.Add(new(PushEbxRawFileOffset, "EBX", ebxAfterLea));
        pushes.Add(new(PushEcxRawFileOffset, "ECX", capture.EntryEcx));

        uint frameMinus10Address = FrameMinus10Address(capture.EntryEbp);
        reads.Add(new(FrameMinus10PushRawFileOffset, frameMinus10Address,
            capture.FrameMinus10Dword));
        accesses.Add(new(FrameHandoffCapacitySecondTransferMemoryAccessKind.Read,
            FrameMinus10PushRawFileOffset, frameMinus10Address, capture.FrameMinus10Dword));
        if (capture.FrameMinus10Dword is not uint frameMinus10Dword)
        {
            return Stop(capture,
                FrameHandoffCapacitySecondTransferOutcome.MissingFrameMinus10Capture,
                reads, accesses, writes, pushes, FrameMinus10PushRawFileOffset,
                eaxAfterLea, edxAfterMove, eaxAfterArithmeticShiftRight, edxAfterSubtract,
                frameMinusCAddress, ebxAfterLea,
                callCellReadReached: true,
                callCellCaptured: true,
                ebxPushReached: true,
                ecxPushReached: true,
                frameMinus10ReadReached: true,
                unreachedCallReturnCaptureProvided: capture.CallReturnedNormally.HasValue);
        }

        pushes.Add(new(FrameMinus10PushRawFileOffset, "[EBP-0x10]", frameMinus10Dword));
        if (capture.CallReturnedNormally is not bool callReturnedNormally)
        {
            return Stop(capture,
                FrameHandoffCapacitySecondTransferOutcome.MissingCallReturnCapture,
                reads, accesses, writes, pushes, CallRawFileOffset,
                eaxAfterLea, edxAfterMove, eaxAfterArithmeticShiftRight, edxAfterSubtract,
                frameMinusCAddress, ebxAfterLea,
                callCellReadReached: true,
                callCellCaptured: true,
                ebxPushReached: true,
                ecxPushReached: true,
                frameMinus10ReadReached: true,
                frameMinus10Captured: true,
                frameMinus10PushReached: true,
                callReached: true);
        }

        return BuildPlan(
            capture,
            callReturnedNormally
                ? FrameHandoffCapacitySecondTransferOutcome.ReturnedNormally
                : FrameHandoffCapacitySecondTransferOutcome.CallDidNotReturnNormally,
            reads, accesses, writes, pushes,
            eaxAfterLea, edxAfterMove, eaxAfterArithmeticShiftRight, edxAfterSubtract,
            frameMinusCAddress, ebxAfterLea,
            callCellReadReached: true,
            callCellCaptured: true,
            ebxPushReached: true,
            ecxPushReached: true,
            frameMinus10ReadReached: true,
            frameMinus10Captured: true,
            frameMinus10PushReached: true,
            callReached: true,
            callReturnedNormally: callReturnedNormally,
            normalBoundaryReached: callReturnedNormally,
            stopAt: callReturnedNormally
                ? NormalBoundaryRawFileOffsetExclusive
                : CallRawFileOffset);
    }

    private static FrameHandoffCapacitySecondTransferInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(observation, rawFileOffset, rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset), bytes);

    private static FrameHandoffCapacitySecondTransferPlan Stop(
        FrameHandoffCapacitySecondTransferCapture capture,
        FrameHandoffCapacitySecondTransferOutcome outcome,
        ImmutableArray<FrameHandoffCapacitySecondTransferRead>.Builder reads,
        ImmutableArray<FrameHandoffCapacitySecondTransferMemoryAccess>.Builder accesses,
        ImmutableArray<FrameHandoffCapacitySecondTransferFrameWrite>.Builder writes,
        ImmutableArray<FrameHandoffCapacitySecondTransferPush>.Builder pushes,
        uint stopAt,
        uint eaxAfterLea,
        uint edxAfterMove,
        uint eaxAfterArithmeticShiftRight,
        uint edxAfterSubtract,
        uint frameMinusCAddress,
        uint ebxAfterLea,
        bool callCellReadReached = false,
        bool callCellCaptured = false,
        bool ebxPushReached = false,
        bool ecxPushReached = false,
        bool frameMinus10ReadReached = false,
        bool frameMinus10Captured = false,
        bool frameMinus10PushReached = false,
        bool callReached = false,
        bool unreachedFrameMinus10CaptureProvided = false,
        bool unreachedCallReturnCaptureProvided = false) =>
        BuildPlan(capture, outcome, reads, accesses, writes, pushes,
            eaxAfterLea, edxAfterMove, eaxAfterArithmeticShiftRight, edxAfterSubtract,
            frameMinusCAddress, ebxAfterLea,
            callCellReadReached, callCellCaptured, ebxPushReached, ecxPushReached,
            frameMinus10ReadReached, frameMinus10Captured, frameMinus10PushReached,
            callReached, callReached ? capture.CallReturnedNormally : null,
            normalBoundaryReached: false,
            unreachedFrameMinus10CaptureProvided: unreachedFrameMinus10CaptureProvided,
            unreachedCallReturnCaptureProvided: unreachedCallReturnCaptureProvided,
            stopAt: stopAt);

    private static FrameHandoffCapacitySecondTransferPlan BuildPlan(
        FrameHandoffCapacitySecondTransferCapture capture,
        FrameHandoffCapacitySecondTransferOutcome outcome,
        ImmutableArray<FrameHandoffCapacitySecondTransferRead>.Builder reads,
        ImmutableArray<FrameHandoffCapacitySecondTransferMemoryAccess>.Builder accesses,
        ImmutableArray<FrameHandoffCapacitySecondTransferFrameWrite>.Builder writes,
        ImmutableArray<FrameHandoffCapacitySecondTransferPush>.Builder pushes,
        uint eaxAfterLea,
        uint edxAfterMove,
        uint eaxAfterArithmeticShiftRight,
        uint edxAfterSubtract,
        uint frameMinusCAddress,
        uint ebxAfterLea,
        bool callCellReadReached,
        bool callCellCaptured,
        bool ebxPushReached,
        bool ecxPushReached,
        bool frameMinus10ReadReached,
        bool frameMinus10Captured,
        bool frameMinus10PushReached,
        bool callReached,
        bool? callReturnedNormally = null,
        bool normalBoundaryReached = false,
        bool unreachedFrameMinus10CaptureProvided = false,
        bool unreachedCallReturnCaptureProvided = false,
        uint stopAt = CallRawFileOffset) =>
        new(
            capture,
            outcome,
            reads.ToImmutable(),
            accesses.ToImmutable(),
            writes.ToImmutable(),
            pushes.ToImmutable(),
            EaxLeaReached: true,
            EaxAfterLea: eaxAfterLea,
            EdxMoveReached: true,
            EdxAfterMove: edxAfterMove,
            ArithmeticShiftRightReached: true,
            EaxAfterArithmeticShiftRight: eaxAfterArithmeticShiftRight,
            EdxSubtractReached: true,
            EdxAfterSubtract: edxAfterSubtract,
            FrameMinusCWriteReached: true,
            FrameMinusCWriteAddress: frameMinusCAddress,
            FrameMinusCWriteValue: edxAfterSubtract,
            EbxLeaReached: true,
            EbxAfterLea: ebxAfterLea,
            CallCellReadReached: callCellReadReached,
            CallCellCaptured: callCellCaptured,
            CallCellAddress: CallCellAddress,
            CallTargetAddress: callCellCaptured ? capture.CallCellDword : null,
            EbxPushReached: ebxPushReached,
            EcxPushReached: ecxPushReached,
            FrameMinus10ReadReached: frameMinus10ReadReached,
            FrameMinus10Captured: frameMinus10Captured,
            FrameMinus10PushReached: frameMinus10PushReached,
            CallReached: callReached,
            CallReturnedNormally: callReturnedNormally,
            NormalBoundaryReached: normalBoundaryReached,
            UnreachedFrameMinus10CaptureProvided: unreachedFrameMinus10CaptureProvided,
            UnreachedCallReturnCaptureProvided: unreachedCallReturnCaptureProvided,
            StopAtRawFileOffsetExclusive: stopAt);
}
