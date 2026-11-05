using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityAboveTransferCapture(
    bool ValidatedBranchEntry,
    uint EntryEsi,
    uint EntryEcx,
    uint EntryEdx,
    uint? CallCellDword,
    bool? CallReturnedNormally);

public enum FrameHandoffCapacityAboveTransferOutcome
{
    EntryNotValidated,
    MissingCallCellCapture,
    MissingCallReturnCapture,
    CallDidNotReturnNormally,
    ReturnedNormally
}

public readonly record struct FrameHandoffCapacityAboveTransferRead(
    uint RawFileOffset,
    uint Address,
    uint? Value);

public readonly record struct FrameHandoffCapacityAboveTransferPush(
    uint RawFileOffset,
    string Source,
    uint Value);

public readonly record struct FrameHandoffCapacityAboveTransferScalar(
    uint RawFileOffset,
    string Operation,
    string Source,
    uint Value);

public readonly record struct FrameHandoffCapacityAboveTransferInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct FrameHandoffCapacityAboveTransferPlan(
    FrameHandoffCapacityAboveTransferCapture Capture,
    FrameHandoffCapacityAboveTransferOutcome Outcome,
    ImmutableArray<FrameHandoffCapacityAboveTransferRead> Reads,
    ImmutableArray<FrameHandoffCapacityAboveTransferPush> Pushes,
    ImmutableArray<FrameHandoffCapacityAboveTransferScalar> Scalars,
    bool EdiLeaReached,
    uint? EdiAfterLea,
    bool EaxMoveReached,
    uint? EaxAfterMove,
    bool EaxAndReached,
    uint? EaxAfterAnd,
    bool EaxPushReached,
    bool CallCellReadReached,
    bool CallCellCaptured,
    uint CallCellAddress,
    uint? CallTargetAddress,
    bool EcxPushReached,
    bool EdxPushReached,
    bool CallReached,
    bool? CallReturnedNormally,
    bool NormalBoundaryReached,
    bool UnreachedCallCellCaptureProvided,
    bool UnreachedCallReturnCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the bounded above-transfer through its indirect call. Entry validation is supplied
/// by the upstream branch; this projection does not infer a threshold from ESI.
/// </summary>
public static class FrameHandoffCapacityAboveTransferProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6C57;
    public const uint EntryVirtualAddress = 0x00FE7857;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE6C6D;
    public const uint NormalBoundaryVirtualAddressExclusive = 0x00FE786D;
    public const uint CallCellAddress = 0x01AC489C;

    public const uint EdiLeaRawFileOffset = 0x00BE6C57;
    public const uint EaxMoveRawFileOffset = 0x00BE6C5E;
    public const uint EaxAndRawFileOffset = 0x00BE6C60;
    public const uint PushEaxRawFileOffset = 0x00BE6C63;
    public const uint CallCellReadRawFileOffset = 0x00BE6C64;
    public const uint PushEcxRawFileOffset = 0x00BE6C69;
    public const uint PushEdxRawFileOffset = 0x00BE6C6A;
    public const uint CallRawFileOffset = 0x00BE6C6B;

    public const string CapturedWindowBytes =
        "8D 3C B5 00 00 00 00 8B C7 83 E0 FC 50 A1 9C 48 AC 01 51 52 FF D0";

    public static ImmutableArray<FrameHandoffCapacityAboveTransferInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Form EDI as entry ESI times four with DWORD wraparound",
                EdiLeaRawFileOffset, "8D 3C B5 00 00 00 00"),
            Instruction("Copy EDI into EAX",
                EaxMoveRawFileOffset, "8B C7"),
            Instruction("Clear the low two bits of EAX",
                EaxAndRawFileOffset, "83 E0 FC"),
            Instruction("Push the scalar EAX value",
                PushEaxRawFileOffset, "50"),
            Instruction("Independently read the call-target DWORD from 0x01AC489C into EAX",
                CallCellReadRawFileOffset, "A1 9C 48 AC 01"),
            Instruction("Push entry ECX",
                PushEcxRawFileOffset, "51"),
            Instruction("Push entry EDX",
                PushEdxRawFileOffset, "52"),
            Instruction("Call EAX; target behavior is opaque",
                CallRawFileOffset, "FF D0"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ScaleEntryEsi(uint entryEsi) =>
        unchecked(entryEsi * 4);

    public static uint ClearLowTwoBits(uint value) =>
        value & 0xFFFFFFFC;

    public static FrameHandoffCapacityAboveTransferPlan Project(
        FrameHandoffCapacityAboveTransferCapture capture)
    {
        var reads = ImmutableArray.CreateBuilder<FrameHandoffCapacityAboveTransferRead>();
        var pushes = ImmutableArray.CreateBuilder<FrameHandoffCapacityAboveTransferPush>();
        var scalars = ImmutableArray.CreateBuilder<FrameHandoffCapacityAboveTransferScalar>();

        if (!capture.ValidatedBranchEntry)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityAboveTransferOutcome.EntryNotValidated,
                reads, pushes, scalars,
                stopAt: EntryRawFileOffset,
                unreachedCallCellCaptureProvided: capture.CallCellDword.HasValue,
                unreachedCallReturnCaptureProvided: capture.CallReturnedNormally.HasValue);
        }

        uint ediAfterLea = ScaleEntryEsi(capture.EntryEsi);
        scalars.Add(new(EdiLeaRawFileOffset, "LEA", "entry ESI * 4", ediAfterLea));
        uint eaxAfterMove = ediAfterLea;
        scalars.Add(new(EaxMoveRawFileOffset, "MOV", "EDI", eaxAfterMove));
        uint eaxAfterAnd = ClearLowTwoBits(eaxAfterMove);
        scalars.Add(new(EaxAndRawFileOffset, "AND", "EAX & 0xFFFFFFFC", eaxAfterAnd));
        pushes.Add(new(PushEaxRawFileOffset, "EAX", eaxAfterAnd));

        reads.Add(new(CallCellReadRawFileOffset, CallCellAddress, capture.CallCellDword));
        if (capture.CallCellDword is not uint callTarget)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityAboveTransferOutcome.MissingCallCellCapture,
                reads, pushes, scalars,
                ediLeaReached: true,
                ediAfterLea: ediAfterLea,
                eaxMoveReached: true,
                eaxAfterMove: eaxAfterMove,
                eaxAndReached: true,
                eaxAfterAnd: eaxAfterAnd,
                eaxPushReached: true,
                callCellReadReached: true,
                unreachedCallReturnCaptureProvided: capture.CallReturnedNormally.HasValue,
                stopAt: CallCellReadRawFileOffset);
        }

        pushes.Add(new(PushEcxRawFileOffset, "ECX", capture.EntryEcx));
        pushes.Add(new(PushEdxRawFileOffset, "EDX", capture.EntryEdx));
        if (capture.CallReturnedNormally is not bool callReturnedNormally)
        {
            return BuildPlan(
                capture,
                FrameHandoffCapacityAboveTransferOutcome.MissingCallReturnCapture,
                reads, pushes, scalars,
                ediLeaReached: true,
                ediAfterLea: ediAfterLea,
                eaxMoveReached: true,
                eaxAfterMove: eaxAfterMove,
                eaxAndReached: true,
                eaxAfterAnd: eaxAfterAnd,
                eaxPushReached: true,
                callCellReadReached: true,
                callCellCaptured: true,
                callTargetAddress: callTarget,
                ecxPushReached: true,
                edxPushReached: true,
                callReached: true,
                stopAt: CallRawFileOffset);
        }

        return BuildPlan(
            capture,
            callReturnedNormally
                ? FrameHandoffCapacityAboveTransferOutcome.ReturnedNormally
                : FrameHandoffCapacityAboveTransferOutcome.CallDidNotReturnNormally,
            reads, pushes, scalars,
            ediLeaReached: true,
            ediAfterLea: ediAfterLea,
            eaxMoveReached: true,
            eaxAfterMove: eaxAfterMove,
            eaxAndReached: true,
            eaxAfterAnd: eaxAfterAnd,
            eaxPushReached: true,
            callCellReadReached: true,
            callCellCaptured: true,
            callTargetAddress: callTarget,
            ecxPushReached: true,
            edxPushReached: true,
            callReached: true,
            callReturnedNormally: callReturnedNormally,
            normalBoundaryReached: callReturnedNormally,
            stopAt: callReturnedNormally
                ? NormalBoundaryRawFileOffsetExclusive
                : CallRawFileOffset);
    }

    private static FrameHandoffCapacityAboveTransferInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(observation, rawFileOffset, rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset), bytes);

    private static FrameHandoffCapacityAboveTransferPlan BuildPlan(
        FrameHandoffCapacityAboveTransferCapture capture,
        FrameHandoffCapacityAboveTransferOutcome outcome,
        ImmutableArray<FrameHandoffCapacityAboveTransferRead>.Builder reads,
        ImmutableArray<FrameHandoffCapacityAboveTransferPush>.Builder pushes,
        ImmutableArray<FrameHandoffCapacityAboveTransferScalar>.Builder scalars,
        bool ediLeaReached = false,
        uint? ediAfterLea = null,
        bool eaxMoveReached = false,
        uint? eaxAfterMove = null,
        bool eaxAndReached = false,
        uint? eaxAfterAnd = null,
        bool eaxPushReached = false,
        bool callCellReadReached = false,
        bool callCellCaptured = false,
        uint? callTargetAddress = null,
        bool ecxPushReached = false,
        bool edxPushReached = false,
        bool callReached = false,
        bool? callReturnedNormally = null,
        bool normalBoundaryReached = false,
        bool unreachedCallCellCaptureProvided = false,
        bool unreachedCallReturnCaptureProvided = false,
        uint stopAt = CallRawFileOffset) =>
        new(
            capture,
            outcome,
            reads.ToImmutable(),
            pushes.ToImmutable(),
            scalars.ToImmutable(),
            ediLeaReached,
            ediAfterLea,
            eaxMoveReached,
            eaxAfterMove,
            eaxAndReached,
            eaxAfterAnd,
            eaxPushReached,
            callCellReadReached,
            callCellCaptured,
            CallCellAddress,
            callCellCaptured ? callTargetAddress : null,
            ecxPushReached,
            edxPushReached,
            callReached,
            callReached ? callReturnedNormally : null,
            normalBoundaryReached,
            unreachedCallCellCaptureProvided,
            unreachedCallReturnCaptureProvided,
            stopAt);
}
