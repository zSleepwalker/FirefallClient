using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityAboveSecondTransferCapture(
    bool ValidatedNormalEntry,
    uint EntryEbx,
    uint EntryEdi,
    uint EntryEbp,
    uint? Receiver4Dword,
    uint? FrameMinusCDword,
    uint? FrameMinus4Dword,
    bool? CallReturnedNormally);

public enum FrameHandoffCapacityAboveSecondTransferOutcome
{
    EntryNotValidated,
    MissingReceiver4Capture,
    MissingFrameMinusCCapture,
    MissingFrameMinus4Capture,
    MissingCallReturnCapture,
    CallDidNotReturnNormally,
    ReturnedNormally
}

public readonly record struct FrameHandoffCapacityAboveSecondTransferRead(
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct FrameHandoffCapacityAboveSecondTransferPush(
    uint RawFileOffset,
    string Source,
    uint Value);

public readonly record struct FrameHandoffCapacityAboveSecondTransferScalar(
    uint RawFileOffset,
    string Operation,
    string Source,
    uint Value);

public readonly record struct FrameHandoffCapacityAboveSecondTransferInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct FrameHandoffCapacityAboveSecondTransferPlan(
    FrameHandoffCapacityAboveSecondTransferCapture Capture,
    FrameHandoffCapacityAboveSecondTransferOutcome Outcome,
    ImmutableArray<FrameHandoffCapacityAboveSecondTransferRead> Reads,
    ImmutableArray<FrameHandoffCapacityAboveSecondTransferPush> Pushes,
    ImmutableArray<FrameHandoffCapacityAboveSecondTransferScalar> Scalars,
    bool Receiver4ReadReached,
    bool Receiver4Captured,
    uint Receiver4ReadAddress,
    bool FrameMinusCReadReached,
    bool FrameMinusCCaptured,
    uint FrameMinusCReadAddress,
    bool EcxLeaReached,
    uint? EcxAfterLea,
    bool EdxSubtractReached,
    uint? EdxAfterSubtract,
    bool EdxAddReached,
    uint? EdxAfterAdd,
    bool EdxArithmeticShiftRightReached,
    uint? EdxAfterArithmeticShiftRight,
    bool EbxLeaReached,
    uint? EbxAfterLea,
    bool EbxPushReached,
    bool EcxPushReached,
    bool FrameMinus4ReadReached,
    bool FrameMinus4Captured,
    uint FrameMinus4ReadAddress,
    bool FrameMinus4PushReached,
    uint CallCellAddress,
    bool CallReached,
    bool? CallReturnedNormally,
    bool NormalBoundaryReached,
    bool UnreachedReceiver4CaptureProvided,
    bool UnreachedFrameMinusCCaptureProvided,
    bool UnreachedFrameMinus4CaptureProvided,
    bool UnreachedCallReturnCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the validated normal-entry path through its indirect call return.
/// </summary>
public static class FrameHandoffCapacityAboveSecondTransferProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6C6D;
    public const uint EntryVirtualAddress = 0x00FE786D;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE6C8F;
    public const uint NormalBoundaryVirtualAddressExclusive = 0x00FE788F;
    public const uint CallCellAddress = 0x01AC489C;
    public const uint Receiver4Offset = 4;
    public const uint FrameMinusCOffset = 0x0C;
    public const uint FrameMinus4Offset = 4;

    public const uint Receiver4ReadRawFileOffset = 0x00BE6C6D;
    public const uint FrameMinusCReadRawFileOffset = 0x00BE6C70;
    public const uint EcxLeaRawFileOffset = 0x00BE6C73;
    public const uint EdxSubtractRawFileOffset = 0x00BE6C76;
    public const uint EdxAddRawFileOffset = 0x00BE6C78;
    public const uint ArithmeticShiftRightRawFileOffset = 0x00BE6C7A;
    public const uint EbxLeaRawFileOffset = 0x00BE6C7D;
    public const uint PushEbxRawFileOffset = 0x00BE6C84;
    public const uint PushEcxRawFileOffset = 0x00BE6C85;
    public const uint FrameMinus4PushRawFileOffset = 0x00BE6C86;
    public const uint CallRawFileOffset = 0x00BE6C89;

    public const string CapturedWindowBytes =
        "8B 43 04 8B 55 F4 8D 0C 07 2B D1 03 D0 C1 FA 02 "
        + "8D 1C 95 00 00 00 00 53 51 FF 75 FC FF 15 9C 48 AC 01";

    public static ImmutableArray<FrameHandoffCapacityAboveSecondTransferInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Independently read [entry EBX+4] into EAX",
                Receiver4ReadRawFileOffset, "8B 43 04"),
            Instruction("Independently read [entry EBP-0xC] into EDX",
                FrameMinusCReadRawFileOffset, "8B 55 F4"),
            Instruction("Form ECX as entry EDI plus receiver DWORD with DWORD wraparound",
                EcxLeaRawFileOffset, "8D 0C 07"),
            Instruction("Subtract ECX from EDX with DWORD wraparound",
                EdxSubtractRawFileOffset, "2B D1"),
            Instruction("Add the original receiver DWORD to EDX with DWORD wraparound",
                EdxAddRawFileOffset, "03 D0"),
            Instruction("Arithmetic-shift EDX right by two",
                ArithmeticShiftRightRawFileOffset, "C1 FA 02"),
            Instruction("Form EBX as EDX times four with DWORD wraparound",
                EbxLeaRawFileOffset, "8D 1C 95 00 00 00 00"),
            Instruction("Push EBX", PushEbxRawFileOffset, "53"),
            Instruction("Push ECX", PushEcxRawFileOffset, "51"),
            Instruction("Independently read and push [entry EBP-4]",
                FrameMinus4PushRawFileOffset, "FF 75 FC"),
            Instruction("Call through the absolute cell at 0x01AC489C; target behavior is opaque",
                CallRawFileOffset, "FF 15 9C 48 AC 01"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint Receiver4Address(uint entryEbx) =>
        unchecked(entryEbx + Receiver4Offset);

    public static uint FrameMinusCAddress(uint entryEbp) =>
        unchecked(entryEbp - FrameMinusCOffset);

    public static uint FrameMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus4Offset);

    public static uint ArithmeticShiftRightByTwo(uint value) =>
        unchecked((uint)((int)value >> 2));

    public static FrameHandoffCapacityAboveSecondTransferPlan Project(
        FrameHandoffCapacityAboveSecondTransferCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<FrameHandoffCapacityAboveSecondTransferRead>();
        var pushes = ImmutableArray.CreateBuilder<FrameHandoffCapacityAboveSecondTransferPush>();
        var scalars = ImmutableArray.CreateBuilder<FrameHandoffCapacityAboveSecondTransferScalar>();

        if (!capture.ValidatedNormalEntry)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityAboveSecondTransferOutcome.EntryNotValidated,
                reads, pushes, scalars,
                stopAt: EntryRawFileOffset,
                unreachedReceiver4CaptureProvided: capture.Receiver4Dword.HasValue,
                unreachedFrameMinusCCaptureProvided: capture.FrameMinusCDword.HasValue,
                unreachedFrameMinus4CaptureProvided: capture.FrameMinus4Dword.HasValue,
                unreachedCallReturnCaptureProvided: capture.CallReturnedNormally.HasValue);
        }

        uint receiver4Address = Receiver4Address(capture.EntryEbx);
        reads.Add(new(Receiver4ReadRawFileOffset, receiver4Address, capture.Receiver4Dword));
        if (capture.Receiver4Dword is not uint receiver4)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityAboveSecondTransferOutcome.MissingReceiver4Capture,
                reads, pushes, scalars,
                receiver4ReadReached: true,
                receiver4ReadAddress: receiver4Address,
                unreachedFrameMinusCCaptureProvided: capture.FrameMinusCDword.HasValue,
                unreachedFrameMinus4CaptureProvided: capture.FrameMinus4Dword.HasValue,
                unreachedCallReturnCaptureProvided: capture.CallReturnedNormally.HasValue,
                stopAt: Receiver4ReadRawFileOffset);
        }

        uint frameMinusCAddress = FrameMinusCAddress(capture.EntryEbp);
        reads.Add(new(FrameMinusCReadRawFileOffset, frameMinusCAddress, capture.FrameMinusCDword));
        if (capture.FrameMinusCDword is not uint frameMinusC)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityAboveSecondTransferOutcome.MissingFrameMinusCCapture,
                reads, pushes, scalars,
                receiver4ReadReached: true,
                receiver4Captured: true,
                receiver4ReadAddress: receiver4Address,
                frameMinusCReadReached: true,
                frameMinusCReadAddress: frameMinusCAddress,
                unreachedFrameMinus4CaptureProvided: capture.FrameMinus4Dword.HasValue,
                unreachedCallReturnCaptureProvided: capture.CallReturnedNormally.HasValue,
                stopAt: FrameMinusCReadRawFileOffset);
        }

        uint ecxAfterLea = unchecked(capture.EntryEdi + receiver4);
        scalars.Add(new(EcxLeaRawFileOffset, "LEA", "entry EDI + receiver4", ecxAfterLea));
        uint edxAfterSubtract = unchecked(frameMinusC - ecxAfterLea);
        scalars.Add(new(EdxSubtractRawFileOffset, "SUB", "EDX - ECX", edxAfterSubtract));
        uint edxAfterAdd = unchecked(edxAfterSubtract + receiver4);
        scalars.Add(new(EdxAddRawFileOffset, "ADD", "EDX + original receiver4", edxAfterAdd));
        uint edxAfterArithmeticShiftRight = ArithmeticShiftRightByTwo(edxAfterAdd);
        scalars.Add(new(ArithmeticShiftRightRawFileOffset, "SAR", "EDX >> 2", edxAfterArithmeticShiftRight));
        uint ebxAfterLea = unchecked(edxAfterArithmeticShiftRight * 4);
        scalars.Add(new(EbxLeaRawFileOffset, "LEA", "EDX * 4", ebxAfterLea));

        pushes.Add(new(PushEbxRawFileOffset, "EBX", ebxAfterLea));
        pushes.Add(new(PushEcxRawFileOffset, "ECX", ecxAfterLea));

        uint frameMinus4Address = FrameMinus4Address(capture.EntryEbp);
        reads.Add(new(FrameMinus4PushRawFileOffset, frameMinus4Address, capture.FrameMinus4Dword));
        if (capture.FrameMinus4Dword is not uint frameMinus4)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityAboveSecondTransferOutcome.MissingFrameMinus4Capture,
                reads, pushes, scalars,
                receiver4ReadReached: true,
                receiver4Captured: true,
                receiver4ReadAddress: receiver4Address,
                frameMinusCReadReached: true,
                frameMinusCCaptured: true,
                frameMinusCReadAddress: frameMinusCAddress,
                ecxAfterLea: ecxAfterLea,
                edxAfterSubtract: edxAfterSubtract,
                edxAfterAdd: edxAfterAdd,
                edxAfterArithmeticShiftRight: edxAfterArithmeticShiftRight,
                ebxAfterLea: ebxAfterLea,
                ebxPushReached: true,
                ecxPushReached: true,
                frameMinus4ReadReached: true,
                frameMinus4ReadAddress: frameMinus4Address,
                unreachedCallReturnCaptureProvided: capture.CallReturnedNormally.HasValue,
                stopAt: FrameMinus4PushRawFileOffset);
        }

        pushes.Add(new(FrameMinus4PushRawFileOffset, "[EBP-0x4]", frameMinus4));
        if (capture.CallReturnedNormally is not bool callReturnedNormally)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityAboveSecondTransferOutcome.MissingCallReturnCapture,
                reads, pushes, scalars,
                receiver4ReadReached: true,
                receiver4Captured: true,
                receiver4ReadAddress: receiver4Address,
                frameMinusCReadReached: true,
                frameMinusCCaptured: true,
                frameMinusCReadAddress: frameMinusCAddress,
                ecxAfterLea: ecxAfterLea,
                edxAfterSubtract: edxAfterSubtract,
                edxAfterAdd: edxAfterAdd,
                edxAfterArithmeticShiftRight: edxAfterArithmeticShiftRight,
                ebxAfterLea: ebxAfterLea,
                ebxPushReached: true,
                ecxPushReached: true,
                frameMinus4ReadReached: true,
                frameMinus4Captured: true,
                frameMinus4ReadAddress: frameMinus4Address,
                frameMinus4PushReached: true,
                callReached: true,
                stopAt: CallRawFileOffset);
        }

        return BuildPlan(
            capture,
            callReturnedNormally
                ? FrameHandoffCapacityAboveSecondTransferOutcome.ReturnedNormally
                : FrameHandoffCapacityAboveSecondTransferOutcome.CallDidNotReturnNormally,
            reads, pushes, scalars,
            receiver4ReadReached: true,
            receiver4Captured: true,
            receiver4ReadAddress: receiver4Address,
            frameMinusCReadReached: true,
            frameMinusCCaptured: true,
            frameMinusCReadAddress: frameMinusCAddress,
            ecxAfterLea: ecxAfterLea,
            edxAfterSubtract: edxAfterSubtract,
            edxAfterAdd: edxAfterAdd,
            edxAfterArithmeticShiftRight: edxAfterArithmeticShiftRight,
            ebxAfterLea: ebxAfterLea,
            ebxPushReached: true,
            ecxPushReached: true,
            frameMinus4ReadReached: true,
            frameMinus4Captured: true,
            frameMinus4ReadAddress: frameMinus4Address,
            frameMinus4PushReached: true,
            callReached: true,
            callReturnedNormally: callReturnedNormally,
            normalBoundaryReached: callReturnedNormally,
            stopAt: callReturnedNormally
                ? NormalBoundaryRawFileOffsetExclusive
                : CallRawFileOffset);
    }

    private static FrameHandoffCapacityAboveSecondTransferInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(observation, rawFileOffset, rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset), bytes);

    private static FrameHandoffCapacityAboveSecondTransferPlan BuildPlan(
        FrameHandoffCapacityAboveSecondTransferCapture capture,
        FrameHandoffCapacityAboveSecondTransferOutcome outcome,
        ImmutableArray<FrameHandoffCapacityAboveSecondTransferRead>.Builder reads,
        ImmutableArray<FrameHandoffCapacityAboveSecondTransferPush>.Builder pushes,
        ImmutableArray<FrameHandoffCapacityAboveSecondTransferScalar>.Builder scalars,
        bool receiver4ReadReached = false,
        bool receiver4Captured = false,
        uint? receiver4ReadAddress = null,
        bool frameMinusCReadReached = false,
        bool frameMinusCCaptured = false,
        uint? frameMinusCReadAddress = null,
        uint? ecxAfterLea = null,
        uint? edxAfterSubtract = null,
        uint? edxAfterAdd = null,
        uint? edxAfterArithmeticShiftRight = null,
        uint? ebxAfterLea = null,
        bool ebxPushReached = false,
        bool ecxPushReached = false,
        bool frameMinus4ReadReached = false,
        bool frameMinus4Captured = false,
        uint? frameMinus4ReadAddress = null,
        bool frameMinus4PushReached = false,
        bool callReached = false,
        bool? callReturnedNormally = null,
        bool normalBoundaryReached = false,
        bool unreachedReceiver4CaptureProvided = false,
        bool unreachedFrameMinusCCaptureProvided = false,
        bool unreachedFrameMinus4CaptureProvided = false,
        bool unreachedCallReturnCaptureProvided = false,
        uint stopAt = CallRawFileOffset) =>
        new(
            capture,
            outcome,
            reads.ToImmutable(),
            pushes.ToImmutable(),
            scalars.ToImmutable(),
            receiver4ReadReached,
            receiver4Captured,
            receiver4ReadAddress ?? Receiver4Address(capture.EntryEbx),
            frameMinusCReadReached,
            frameMinusCCaptured,
            frameMinusCReadAddress ?? FrameMinusCAddress(capture.EntryEbp),
            ecxAfterLea.HasValue,
            ecxAfterLea,
            edxAfterSubtract.HasValue,
            edxAfterSubtract,
            edxAfterAdd.HasValue,
            edxAfterAdd,
            edxAfterArithmeticShiftRight.HasValue,
            edxAfterArithmeticShiftRight,
            ebxAfterLea.HasValue,
            ebxAfterLea,
            ebxPushReached,
            ecxPushReached,
            frameMinus4ReadReached,
            frameMinus4Captured,
            frameMinus4ReadAddress ?? FrameMinus4Address(capture.EntryEbp),
            frameMinus4PushReached,
            CallCellAddress,
            callReached,
            callReached ? callReturnedNormally : null,
            normalBoundaryReached,
            unreachedReceiver4CaptureProvided,
            unreachedFrameMinusCCaptureProvided,
            unreachedFrameMinus4CaptureProvided,
            unreachedCallReturnCaptureProvided,
            stopAt);
}
