using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityTransferCapture(
    uint EntryEbx,
    uint EntryEbp,
    uint EntryEdi,
    uint? Receiver4,
    uint? Receiver8,
    uint? FrameMinus4,
    uint? FrameMinusC,
    bool? HelperReturnedNormally);

public enum FrameHandoffCapacityTransferOutcome
{
    MissingReceiver4Capture,
    MissingReceiver8Capture,
    MissingFrameMinus4Capture,
    MissingFrameMinusCCapture,
    MissingHelperNormalCapture,
    HelperDidNotReturnNormally,
    ReturnedNormally
}

public readonly record struct FrameHandoffCapacityTransferFrameWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public enum FrameHandoffCapacityTransferMemoryAccessKind
{
    Read,
    Write
}

public readonly record struct FrameHandoffCapacityTransferMemoryAccess(
    FrameHandoffCapacityTransferMemoryAccessKind Kind,
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct FrameHandoffCapacityTransferRead(
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct FrameHandoffCapacityTransferScalarPush(
    uint RawFileOffset,
    string Register,
    uint Value);

public readonly record struct FrameHandoffCapacityTransferPlan(
    FrameHandoffCapacityTransferCapture Capture,
    FrameHandoffCapacityTransferOutcome Outcome,
    ImmutableArray<FrameHandoffCapacityTransferRead> Reads,
    ImmutableArray<FrameHandoffCapacityTransferMemoryAccess> MemoryAccesses,
    ImmutableArray<FrameHandoffCapacityTransferFrameWrite> FrameWrites,
    bool EaxLeaReached,
    uint? EaxAfterLea,
    bool EdxLeaReached,
    uint? EdxAfterLea,
    bool Receiver8ShiftReached,
    uint? Receiver8AfterShift,
    bool SubtractReached,
    uint? EaxAfterSubtract,
    bool AddReceiver4Reached,
    uint? EaxAfterAddReceiver4,
    bool ArithmeticShiftRightReached,
    uint? EaxAfterArithmeticShiftRight,
    bool EdiLeaReached,
    uint? EdiAfterLea,
    bool FrameMinusCAddReached,
    uint? EaxAfterFrameMinusCAdd,
    ImmutableArray<FrameHandoffCapacityTransferScalarPush> ScalarPushes,
    bool IndirectCallReached,
    uint IndirectCallCellAddress,
    bool? HelperReturnedNormally,
    bool UnreachedReceiver8CaptureProvided,
    bool UnreachedFrameMinus4CaptureProvided,
    bool UnreachedFrameMinusCCaptureProvided,
    bool UnreachedHelperReturnCaptureProvided,
    bool NormalBoundaryReached,
    uint StopAtRawFileOffsetExclusive)
{
    public bool SuccessAdmission =>
        Outcome == FrameHandoffCapacityTransferOutcome.ReturnedNormally
        && NormalBoundaryReached
        && !UnreachedReceiver8CaptureProvided
        && !UnreachedFrameMinus4CaptureProvided
        && !UnreachedFrameMinusCCaptureProvided
        && !UnreachedHelperReturnCaptureProvided;
}

public readonly record struct FrameHandoffCapacityTransferInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the first opaque frame-handoff capacity transfer through its indirect call.
/// </summary>
public static class FrameHandoffCapacityTransferProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6BCA;
    public const uint EntryVirtualAddress = 0x00FE77CA;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE6BFD;
    public const uint NormalBoundaryVirtualAddressExclusive = 0x00FE77FD;
    public const uint IndirectCallRawFileOffset = 0x00BE6BF7;
    public const uint IndirectCallCellAddress = 0x01AC489C;
    public const uint Receiver4Offset = 4;
    public const uint Receiver8Offset = 8;
    public const uint FrameMinus4Offset = 4;
    public const uint FrameMinusCOffset = 0x0C;

    public const uint Receiver4ReadRawFileOffset = 0x00BE6BCA;
    public const uint EaxLeaRawFileOffset = 0x00BE6BCD;
    public const uint FrameMinusCWriteRawFileOffset = 0x00BE6BD4;
    public const uint EdxLeaRawFileOffset = 0x00BE6BD7;
    public const uint Receiver8ReadRawFileOffset = 0x00BE6BDA;
    public const uint Receiver8ShiftRawFileOffset = 0x00BE6BDD;
    public const uint SubtractRawFileOffset = 0x00BE6BE0;
    public const uint AddReceiver4RawFileOffset = 0x00BE6BE2;
    public const uint ArithmeticShiftRightRawFileOffset = 0x00BE6BE4;
    public const uint EdiLeaRawFileOffset = 0x00BE6BE7;
    public const uint FrameMinus4ReadRawFileOffset = 0x00BE6BEE;
    public const uint FrameMinusCReadAddRawFileOffset = 0x00BE6BF1;
    public const uint PushEdiRawFileOffset = 0x00BE6BF4;
    public const uint PushEdxRawFileOffset = 0x00BE6BF5;
    public const uint PushEaxRawFileOffset = 0x00BE6BF6;

    public const string CapturedWindowBytes =
        "8B 4B 04 8D 04 BD 00 00 00 00 89 45 F4 8D 14 08 8B 43 08 "
        + "C1 E0 02 2B C2 03 C1 C1 F8 02 8D 3C 85 00 00 00 00 "
        + "8B 45 FC 03 45 F4 57 52 50 FF 15 9C 48 AC 01";

    public static ImmutableArray<FrameHandoffCapacityTransferInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Read [EBX+4] into ECX", Receiver4ReadRawFileOffset, "8B 4B 04"),
            Instruction("Form EAX as entry EDI times four with DWORD wraparound",
                EaxLeaRawFileOffset, "8D 04 BD 00 00 00 00"),
            Instruction("Write EAX to [EBP-0xC]", FrameMinusCWriteRawFileOffset, "89 45 F4"),
            Instruction("Form EDX as EAX plus the captured receiver-4 value",
                EdxLeaRawFileOffset, "8D 14 08"),
            Instruction("Independently read [EBX+8] into EAX",
                Receiver8ReadRawFileOffset, "8B 43 08"),
            Instruction("Shift EAX left by two with DWORD wraparound",
                Receiver8ShiftRawFileOffset, "C1 E0 02"),
            Instruction("Subtract EDX from EAX with DWORD wraparound",
                SubtractRawFileOffset, "2B C2"),
            Instruction("Add the original captured receiver-4 value to EAX",
                AddReceiver4RawFileOffset, "03 C1"),
            Instruction("Arithmetic-shift EAX right by two (SAR, not SHR)",
                ArithmeticShiftRightRawFileOffset, "C1 F8 02"),
            Instruction("Form EDI as EAX times four with DWORD wraparound",
                EdiLeaRawFileOffset, "8D 3C 85 00 00 00 00"),
            Instruction("Independently read [EBP-4] into EAX",
                FrameMinus4ReadRawFileOffset, "8B 45 FC"),
            Instruction("Add an independent read of [EBP-0xC] to EAX",
                FrameMinusCReadAddRawFileOffset, "03 45 F4"),
            Instruction("Push the scalar EDI value", PushEdiRawFileOffset, "57"),
            Instruction("Push the scalar EDX value", PushEdxRawFileOffset, "52"),
            Instruction("Push the scalar EAX value", PushEaxRawFileOffset, "50"),
            Instruction("Indirect call through the cell at 0x01AC489C; target behavior is opaque",
                IndirectCallRawFileOffset, "FF 15 9C 48 AC 01"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint Receiver4ReadAddress(uint entryEbx) =>
        unchecked(entryEbx + Receiver4Offset);

    public static uint Receiver8ReadAddress(uint entryEbx) =>
        unchecked(entryEbx + Receiver8Offset);

    public static uint FrameMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus4Offset);

    public static uint FrameMinusCAddress(uint entryEbp) =>
        unchecked(entryEbp - FrameMinusCOffset);

    public static uint ArithmeticShiftRightByTwo(uint value) =>
        unchecked((uint)((int)value >> 2));

    public static FrameHandoffCapacityTransferPlan Project(
        FrameHandoffCapacityTransferCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<FrameHandoffCapacityTransferRead>();
        var memoryAccesses =
            ImmutableArray.CreateBuilder<FrameHandoffCapacityTransferMemoryAccess>();
        var writes = ImmutableArray.CreateBuilder<FrameHandoffCapacityTransferFrameWrite>();
        var pushes = ImmutableArray.CreateBuilder<FrameHandoffCapacityTransferScalarPush>();
        uint receiver4Address = Receiver4ReadAddress(capture.EntryEbx);
        reads.Add(new(Receiver4ReadRawFileOffset, receiver4Address, capture.Receiver4));
        memoryAccesses.Add(new(FrameHandoffCapacityTransferMemoryAccessKind.Read,
            Receiver4ReadRawFileOffset, receiver4Address, capture.Receiver4));
        if (capture.Receiver4 is not uint receiver4)
        {
            return Stop(capture, FrameHandoffCapacityTransferOutcome.MissingReceiver4Capture,
                reads, memoryAccesses, writes, pushes, Receiver4ReadRawFileOffset);
        }

        uint eaxAfterLea = unchecked(capture.EntryEdi * 4);
        uint frameMinusCAddress = FrameMinusCAddress(capture.EntryEbp);
        writes.Add(new(FrameMinusCWriteRawFileOffset, frameMinusCAddress, eaxAfterLea));
        memoryAccesses.Add(new(FrameHandoffCapacityTransferMemoryAccessKind.Write,
            FrameMinusCWriteRawFileOffset, frameMinusCAddress, eaxAfterLea));
        uint edxAfterLea = unchecked(eaxAfterLea + receiver4);

        uint receiver8Address = Receiver8ReadAddress(capture.EntryEbx);
        reads.Add(new(Receiver8ReadRawFileOffset, receiver8Address, capture.Receiver8));
        memoryAccesses.Add(new(FrameHandoffCapacityTransferMemoryAccessKind.Read,
            Receiver8ReadRawFileOffset, receiver8Address, capture.Receiver8));
        if (capture.Receiver8 is not uint receiver8)
        {
            return Stop(capture, FrameHandoffCapacityTransferOutcome.MissingReceiver8Capture,
                reads, memoryAccesses, writes, pushes, Receiver8ReadRawFileOffset,
                eaxLeaReached: true, eaxAfterLea: eaxAfterLea,
                edxLeaReached: true, edxAfterLea: edxAfterLea);
        }

        uint receiver8AfterShift = unchecked(receiver8 << 2);
        uint eaxAfterSubtract = unchecked(receiver8AfterShift - edxAfterLea);
        uint eaxAfterAddReceiver4 = unchecked(eaxAfterSubtract + receiver4);
        uint eaxAfterArithmeticShiftRight = ArithmeticShiftRightByTwo(eaxAfterAddReceiver4);
        uint ediAfterLea = unchecked(eaxAfterArithmeticShiftRight * 4);

        uint frameMinus4Address = FrameMinus4Address(capture.EntryEbp);
        reads.Add(new(FrameMinus4ReadRawFileOffset, frameMinus4Address, capture.FrameMinus4));
        memoryAccesses.Add(new(FrameHandoffCapacityTransferMemoryAccessKind.Read,
            FrameMinus4ReadRawFileOffset, frameMinus4Address, capture.FrameMinus4));
        if (capture.FrameMinus4 is not uint frameMinus4)
        {
            return Stop(capture, FrameHandoffCapacityTransferOutcome.MissingFrameMinus4Capture,
                reads, memoryAccesses, writes, pushes, FrameMinus4ReadRawFileOffset,
                eaxLeaReached: true, eaxAfterLea: eaxAfterLea,
                edxLeaReached: true, edxAfterLea: edxAfterLea,
                receiver8ShiftReached: true, receiver8AfterShift: receiver8AfterShift,
                subtractReached: true, eaxAfterSubtract: eaxAfterSubtract,
                addReceiver4Reached: true, eaxAfterAddReceiver4: eaxAfterAddReceiver4,
                arithmeticShiftRightReached: true,
                eaxAfterArithmeticShiftRight: eaxAfterArithmeticShiftRight,
                ediLeaReached: true, ediAfterLea: ediAfterLea);
        }

        reads.Add(new(FrameMinusCReadAddRawFileOffset, frameMinusCAddress, capture.FrameMinusC));
        memoryAccesses.Add(new(FrameHandoffCapacityTransferMemoryAccessKind.Read,
            FrameMinusCReadAddRawFileOffset, frameMinusCAddress, capture.FrameMinusC));
        if (capture.FrameMinusC is not uint frameMinusC)
        {
            return Stop(capture, FrameHandoffCapacityTransferOutcome.MissingFrameMinusCCapture,
                reads, memoryAccesses, writes, pushes, FrameMinusCReadAddRawFileOffset,
                eaxLeaReached: true, eaxAfterLea: eaxAfterLea,
                edxLeaReached: true, edxAfterLea: edxAfterLea,
                receiver8ShiftReached: true, receiver8AfterShift: receiver8AfterShift,
                subtractReached: true, eaxAfterSubtract: eaxAfterSubtract,
                addReceiver4Reached: true, eaxAfterAddReceiver4: eaxAfterAddReceiver4,
                arithmeticShiftRightReached: true,
                eaxAfterArithmeticShiftRight: eaxAfterArithmeticShiftRight,
                ediLeaReached: true, ediAfterLea: ediAfterLea);
        }

        uint eaxAfterFrameMinusCAdd = unchecked(frameMinus4 + frameMinusC);
        pushes.Add(new(PushEdiRawFileOffset, "EDI", ediAfterLea));
        pushes.Add(new(PushEdxRawFileOffset, "EDX", edxAfterLea));
        pushes.Add(new(PushEaxRawFileOffset, "EAX", eaxAfterFrameMinusCAdd));

        if (capture.HelperReturnedNormally is not bool helperReturnedNormally)
        {
            return Stop(capture, FrameHandoffCapacityTransferOutcome.MissingHelperNormalCapture,
                reads, memoryAccesses, writes, pushes, IndirectCallRawFileOffset,
                eaxLeaReached: true, eaxAfterLea: eaxAfterLea,
                edxLeaReached: true, edxAfterLea: edxAfterLea,
                receiver8ShiftReached: true, receiver8AfterShift: receiver8AfterShift,
                subtractReached: true, eaxAfterSubtract: eaxAfterSubtract,
                addReceiver4Reached: true, eaxAfterAddReceiver4: eaxAfterAddReceiver4,
                arithmeticShiftRightReached: true,
                eaxAfterArithmeticShiftRight: eaxAfterArithmeticShiftRight,
                ediLeaReached: true, ediAfterLea: ediAfterLea,
                frameMinusCAddReached: true, eaxAfterFrameMinusCAdd: eaxAfterFrameMinusCAdd,
                indirectCallReached: true);
        }

        return BuildPlan(
            capture,
            helperReturnedNormally
                ? FrameHandoffCapacityTransferOutcome.ReturnedNormally
                : FrameHandoffCapacityTransferOutcome.HelperDidNotReturnNormally,
                reads, memoryAccesses, writes, pushes,
            eaxLeaReached: true, eaxAfterLea: eaxAfterLea,
            edxLeaReached: true, edxAfterLea: edxAfterLea,
            receiver8ShiftReached: true, receiver8AfterShift: receiver8AfterShift,
            subtractReached: true, eaxAfterSubtract: eaxAfterSubtract,
            addReceiver4Reached: true, eaxAfterAddReceiver4: eaxAfterAddReceiver4,
            arithmeticShiftRightReached: true,
            eaxAfterArithmeticShiftRight: eaxAfterArithmeticShiftRight,
            ediLeaReached: true, ediAfterLea: ediAfterLea,
            frameMinusCAddReached: true, eaxAfterFrameMinusCAdd: eaxAfterFrameMinusCAdd,
            indirectCallReached: true,
            helperReturnedNormally: helperReturnedNormally,
            normalBoundaryReached: helperReturnedNormally,
            stopAt: helperReturnedNormally
                ? NormalBoundaryRawFileOffsetExclusive
                : IndirectCallRawFileOffset);
    }

    private static FrameHandoffCapacityTransferInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(observation, rawFileOffset, rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset), bytes);

    private static FrameHandoffCapacityTransferPlan Stop(
        FrameHandoffCapacityTransferCapture capture,
        FrameHandoffCapacityTransferOutcome outcome,
        ImmutableArray<FrameHandoffCapacityTransferRead>.Builder reads,
        ImmutableArray<FrameHandoffCapacityTransferMemoryAccess>.Builder memoryAccesses,
        ImmutableArray<FrameHandoffCapacityTransferFrameWrite>.Builder writes,
        ImmutableArray<FrameHandoffCapacityTransferScalarPush>.Builder pushes,
        uint stopAt,
        bool eaxLeaReached = false,
        uint? eaxAfterLea = null,
        bool edxLeaReached = false,
        uint? edxAfterLea = null,
        bool receiver8ShiftReached = false,
        uint? receiver8AfterShift = null,
        bool subtractReached = false,
        uint? eaxAfterSubtract = null,
        bool addReceiver4Reached = false,
        uint? eaxAfterAddReceiver4 = null,
        bool arithmeticShiftRightReached = false,
        uint? eaxAfterArithmeticShiftRight = null,
        bool ediLeaReached = false,
        uint? ediAfterLea = null,
        bool frameMinusCAddReached = false,
        uint? eaxAfterFrameMinusCAdd = null,
        bool indirectCallReached = false) =>
        BuildPlan(capture, outcome, reads, memoryAccesses, writes, pushes,
            eaxLeaReached, eaxAfterLea, edxLeaReached, edxAfterLea,
            receiver8ShiftReached, receiver8AfterShift, subtractReached, eaxAfterSubtract,
            addReceiver4Reached, eaxAfterAddReceiver4, arithmeticShiftRightReached,
            eaxAfterArithmeticShiftRight, ediLeaReached, ediAfterLea,
            frameMinusCAddReached, eaxAfterFrameMinusCAdd, indirectCallReached,
            indirectCallReached ? capture.HelperReturnedNormally : null,
            normalBoundaryReached: false,
            stopAt: stopAt);

    private static FrameHandoffCapacityTransferPlan BuildPlan(
        FrameHandoffCapacityTransferCapture capture,
        FrameHandoffCapacityTransferOutcome outcome,
        ImmutableArray<FrameHandoffCapacityTransferRead>.Builder reads,
        ImmutableArray<FrameHandoffCapacityTransferMemoryAccess>.Builder memoryAccesses,
        ImmutableArray<FrameHandoffCapacityTransferFrameWrite>.Builder writes,
        ImmutableArray<FrameHandoffCapacityTransferScalarPush>.Builder pushes,
        bool eaxLeaReached,
        uint? eaxAfterLea,
        bool edxLeaReached,
        uint? edxAfterLea,
        bool receiver8ShiftReached,
        uint? receiver8AfterShift,
        bool subtractReached,
        uint? eaxAfterSubtract,
        bool addReceiver4Reached,
        uint? eaxAfterAddReceiver4,
        bool arithmeticShiftRightReached,
        uint? eaxAfterArithmeticShiftRight,
        bool ediLeaReached,
        uint? ediAfterLea,
        bool frameMinusCAddReached,
        uint? eaxAfterFrameMinusCAdd,
        bool indirectCallReached,
        bool? helperReturnedNormally = null,
        bool normalBoundaryReached = false,
        uint stopAt = IndirectCallRawFileOffset) =>
        new(
            capture,
            outcome,
            reads.ToImmutable(),
            memoryAccesses.ToImmutable(),
            writes.ToImmutable(),
            eaxLeaReached,
            eaxAfterLea,
            edxLeaReached,
            edxAfterLea,
            receiver8ShiftReached,
            receiver8AfterShift,
            subtractReached,
            eaxAfterSubtract,
            addReceiver4Reached,
            eaxAfterAddReceiver4,
            arithmeticShiftRightReached,
            eaxAfterArithmeticShiftRight,
            ediLeaReached,
            ediAfterLea,
            frameMinusCAddReached,
            eaxAfterFrameMinusCAdd,
            pushes.ToImmutable(),
            indirectCallReached,
            IndirectCallCellAddress,
            helperReturnedNormally,
            capture.Receiver8.HasValue
                && !reads.Any(read => read.RawFileOffset == Receiver8ReadRawFileOffset),
            capture.FrameMinus4.HasValue
                && !reads.Any(read => read.RawFileOffset == FrameMinus4ReadRawFileOffset),
            capture.FrameMinusC.HasValue
                && !reads.Any(read => read.RawFileOffset == FrameMinusCReadAddRawFileOffset),
            capture.HelperReturnedNormally.HasValue && !indirectCallReached,
            normalBoundaryReached,
            stopAt);
}
