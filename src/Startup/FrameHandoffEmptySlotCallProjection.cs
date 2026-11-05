using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffEmptySlotCallCapture(
    bool CaptureValidatedEntry,
    bool? DirectCallReturnedNormally,
    uint? DirectCallReturnEax,
    bool? ZeroRouteIndirectCallReturnedNormally,
    uint? ZeroRoutePostCallEcx);

public enum FrameHandoffEmptySlotCallOutcome
{
    EntryNotValidated,
    MissingDirectCallReturnCapture,
    DirectCallDidNotReturnNormally,
    MissingDirectCallReturnEaxCapture,
    MissingZeroRouteIndirectCallReturnCapture,
    ZeroRouteIndirectCallDidNotReturnNormally,
    MissingZeroRoutePostCallEcxCapture,
    DirectNonzeroRouteReturnedNormally,
    ZeroRouteReturnedNormally
}

public enum FrameHandoffEmptySlotCallInstruction
{
    PushTen,
    DirectCall,
    CopyEaxToEcx,
    CleanupFourStackBytes,
    TestEcx,
    BranchIfEcxNonzero,
    ZeroRouteIndirectCall
}

public readonly record struct FrameHandoffEmptySlotCallPlan(
    FrameHandoffEmptySlotCallCapture Capture,
    FrameHandoffEmptySlotCallOutcome Outcome,
    bool PushTenReached,
    uint? StackBytesPushed,
    byte? ExplicitPushValue,
    bool DirectCallReached,
    bool? DirectCallReturnedNormally,
    uint? CapturedDirectCallReturnEax,
    bool DirectCallReturnEaxCaptured,
    bool EaxCopiedToEcx,
    uint? EcxAfterDirectCall,
    bool StackCleanupReached,
    uint? StackCleanupBytes,
    bool EcxTestReached,
    bool? JnzTaken,
    bool ZeroRouteIndirectCallReached,
    bool? ZeroRouteIndirectCallReturnedNormally,
    uint? CapturedZeroRoutePostCallEcx,
    bool ZeroRoutePostCallEcxCaptured,
    bool UnreachedDirectCallReturnCaptureProvided,
    bool UnreachedDirectCallReturnEaxCaptureProvided,
    bool UnreachedZeroRouteIndirectCallReturnCaptureProvided,
    bool UnreachedZeroRoutePostCallEcxCaptureProvided,
    bool NormalBoundaryReached,
    ImmutableArray<FrameHandoffEmptySlotCallInstruction> InstructionsExecuted,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct FrameHandoffEmptySlotCallInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null,
    uint? IndirectCallCellVirtualAddress = null);

/// <summary>
/// Projects the validated push, direct call, ECX gate, and empty-slot indirect-call slice.
/// </summary>
public static class FrameHandoffEmptySlotCallProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE64F2;
    public const uint DirectCallRawFileOffset = 0x00BE64F4;
    public const uint DirectCallNormalReturnRawFileOffset = 0x00BE64F9;
    public const uint EaxToEcxRawFileOffset = 0x00BE64F9;
    public const uint StackCleanupRawFileOffset = 0x00BE64FB;
    public const uint EcxTestRawFileOffset = 0x00BE64FE;
    public const uint ZeroRouteBranchRawFileOffset = 0x00BE6500;
    public const uint ZeroRouteIndirectCallRawFileOffset = 0x00BE6502;
    public const uint ZeroRouteIndirectCallNormalReturnRawFileOffset = 0x00BE6508;
    public const uint NormalBoundaryRawFileOffset = ZeroRouteIndirectCallNormalReturnRawFileOffset;
    public const uint DirectCallTargetRawFileOffset = 0x012B7FB0;
    public const uint DirectCallTargetVirtualAddress =
        DirectCallTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint IndirectCallCellVirtualAddress = 0x01AC4424;

    public const uint StackBytesPushed = 4;
    public const byte ExplicitPushValue = 0x10;
    public const uint StackCleanupByteCount = 4;

    public const string CapturedWindowBytes =
        "6A10E8B71A6D008BC883C40485C97506FF152444AC01";

    public static ImmutableArray<FrameHandoffEmptySlotCallInstruction> InstructionsInOrder { get; } =
        ImmutableArray.Create(
            FrameHandoffEmptySlotCallInstruction.PushTen,
            FrameHandoffEmptySlotCallInstruction.DirectCall,
            FrameHandoffEmptySlotCallInstruction.CopyEaxToEcx,
            FrameHandoffEmptySlotCallInstruction.CleanupFourStackBytes,
            FrameHandoffEmptySlotCallInstruction.TestEcx,
            FrameHandoffEmptySlotCallInstruction.BranchIfEcxNonzero,
            FrameHandoffEmptySlotCallInstruction.ZeroRouteIndirectCall);

    public static ImmutableArray<FrameHandoffEmptySlotCallInstructionEvidence> Evidence { get; } =
        ImmutableArray.Create(
            new FrameHandoffEmptySlotCallInstructionEvidence(
                "Push immediate 0x10 (16)",
                EntryRawFileOffset,
                RawFileOffsetToVirtualAddress(EntryRawFileOffset),
                "6A 10"),
            new FrameHandoffEmptySlotCallInstructionEvidence(
                "Call raw 0x012B7FB0; normal return is raw 0xBE64F9",
                DirectCallRawFileOffset,
                RawFileOffsetToVirtualAddress(DirectCallRawFileOffset),
                "E8 B7 1A 6D 00",
                DirectCallTargetRawFileOffset,
                DirectCallTargetVirtualAddress),
            new FrameHandoffEmptySlotCallInstructionEvidence(
                "Copy the complete EAX value to ECX",
                EaxToEcxRawFileOffset,
                RawFileOffsetToVirtualAddress(EaxToEcxRawFileOffset),
                "8B C8"),
            new FrameHandoffEmptySlotCallInstructionEvidence(
                "Clean up four explicit stack bytes",
                StackCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(StackCleanupRawFileOffset),
                "83 C4 04"),
            new FrameHandoffEmptySlotCallInstructionEvidence(
                "Test ECX against itself",
                EcxTestRawFileOffset,
                RawFileOffsetToVirtualAddress(EcxTestRawFileOffset),
                "85 C9"),
            new FrameHandoffEmptySlotCallInstructionEvidence(
                "A nonzero ECX skips the indirect call to raw 0xBE6508",
                ZeroRouteBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroRouteBranchRawFileOffset),
                "75 06",
                NormalBoundaryRawFileOffset,
                RawFileOffsetToVirtualAddress(NormalBoundaryRawFileOffset)),
            new FrameHandoffEmptySlotCallInstructionEvidence(
                "For zero ECX, call through absolute cell VA 0x01AC4424",
                ZeroRouteIndirectCallRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroRouteIndirectCallRawFileOffset),
                "FF 15 24 44 AC 01",
                IndirectCallCellVirtualAddress: IndirectCallCellVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint VirtualAddressToRawFileOffset(uint virtualAddress) =>
        unchecked(virtualAddress - CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint RelativeCallTarget(uint instructionRawFileOffset, int displacement) =>
        unchecked(instructionRawFileOffset + 5u + (uint)displacement);

    public static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2u + (uint)(sbyte)displacement);

    public static FrameHandoffEmptySlotCallPlan Project(
        FrameHandoffEmptySlotCallCapture capture)
    {
        if (!capture.CaptureValidatedEntry)
        {
            return Stop(
                capture,
                FrameHandoffEmptySlotCallOutcome.EntryNotValidated,
                unreachedDirectCallReturnCaptureProvided:
                    capture.DirectCallReturnedNormally.HasValue,
                unreachedDirectCallReturnEaxCaptureProvided: capture.DirectCallReturnEax.HasValue,
                unreachedZeroRouteIndirectCallReturnCaptureProvided:
                    capture.ZeroRouteIndirectCallReturnedNormally.HasValue,
                unreachedZeroRoutePostCallEcxCaptureProvided:
                    capture.ZeroRoutePostCallEcx.HasValue,
                stopAt: EntryRawFileOffset);
        }

        ImmutableArray<FrameHandoffEmptySlotCallInstruction> afterDirectCall =
            InstructionsInOrder.Take(2).ToImmutableArray();

        if (capture.DirectCallReturnedNormally is not bool directCallReturnedNormally)
        {
            return Stop(
                capture,
                FrameHandoffEmptySlotCallOutcome.MissingDirectCallReturnCapture,
                pushTenReached: true,
                directCallReached: true,
                unreachedDirectCallReturnEaxCaptureProvided: capture.DirectCallReturnEax.HasValue,
                unreachedZeroRouteIndirectCallReturnCaptureProvided:
                    capture.ZeroRouteIndirectCallReturnedNormally.HasValue,
                unreachedZeroRoutePostCallEcxCaptureProvided:
                    capture.ZeroRoutePostCallEcx.HasValue,
                instructionsExecuted: afterDirectCall,
                stopAt: DirectCallRawFileOffset);
        }

        if (!directCallReturnedNormally)
        {
            return Stop(
                capture,
                FrameHandoffEmptySlotCallOutcome.DirectCallDidNotReturnNormally,
                pushTenReached: true,
                directCallReached: true,
                directCallReturnedNormally: false,
                unreachedDirectCallReturnEaxCaptureProvided: capture.DirectCallReturnEax.HasValue,
                unreachedZeroRouteIndirectCallReturnCaptureProvided:
                    capture.ZeroRouteIndirectCallReturnedNormally.HasValue,
                unreachedZeroRoutePostCallEcxCaptureProvided:
                    capture.ZeroRoutePostCallEcx.HasValue,
                instructionsExecuted: afterDirectCall,
                stopAt: DirectCallRawFileOffset);
        }

        if (capture.DirectCallReturnEax is not uint directCallReturnEax)
        {
            return Stop(
                capture,
                FrameHandoffEmptySlotCallOutcome.MissingDirectCallReturnEaxCapture,
                pushTenReached: true,
                directCallReached: true,
                directCallReturnedNormally: true,
                unreachedZeroRouteIndirectCallReturnCaptureProvided:
                    capture.ZeroRouteIndirectCallReturnedNormally.HasValue,
                unreachedZeroRoutePostCallEcxCaptureProvided:
                    capture.ZeroRoutePostCallEcx.HasValue,
                instructionsExecuted: afterDirectCall,
                stopAt: EaxToEcxRawFileOffset);
        }

        bool jnzTaken = directCallReturnEax != 0;
        if (jnzTaken)
        {
            return Stop(
                capture,
                FrameHandoffEmptySlotCallOutcome.DirectNonzeroRouteReturnedNormally,
                pushTenReached: true,
                directCallReached: true,
                directCallReturnedNormally: true,
                capturedDirectCallReturnEax: directCallReturnEax,
                directCallReturnEaxCaptured: true,
                eaxCopiedToEcx: true,
                ecxAfterDirectCall: directCallReturnEax,
                stackCleanupReached: true,
                ecxTestReached: true,
                jnzTaken: true,
                unreachedZeroRouteIndirectCallReturnCaptureProvided:
                    capture.ZeroRouteIndirectCallReturnedNormally.HasValue,
                unreachedZeroRoutePostCallEcxCaptureProvided:
                    capture.ZeroRoutePostCallEcx.HasValue,
                normalBoundaryReached: true,
                instructionsExecuted: InstructionsInOrder.Take(6).ToImmutableArray(),
                stopAt: NormalBoundaryRawFileOffset);
        }

        if (capture.ZeroRouteIndirectCallReturnedNormally is not bool indirectCallReturnedNormally)
        {
            return Stop(
                capture,
                FrameHandoffEmptySlotCallOutcome.MissingZeroRouteIndirectCallReturnCapture,
                pushTenReached: true,
                directCallReached: true,
                directCallReturnedNormally: true,
                capturedDirectCallReturnEax: directCallReturnEax,
                directCallReturnEaxCaptured: true,
                eaxCopiedToEcx: true,
                ecxAfterDirectCall: directCallReturnEax,
                stackCleanupReached: true,
                ecxTestReached: true,
                jnzTaken: false,
                zeroRouteIndirectCallReached: true,
                unreachedZeroRoutePostCallEcxCaptureProvided:
                    capture.ZeroRoutePostCallEcx.HasValue,
                instructionsExecuted: InstructionsInOrder,
                stopAt: ZeroRouteIndirectCallRawFileOffset);
        }

        if (!indirectCallReturnedNormally)
        {
            return Stop(
                capture,
                FrameHandoffEmptySlotCallOutcome.ZeroRouteIndirectCallDidNotReturnNormally,
                pushTenReached: true,
                directCallReached: true,
                directCallReturnedNormally: true,
                capturedDirectCallReturnEax: directCallReturnEax,
                directCallReturnEaxCaptured: true,
                eaxCopiedToEcx: true,
                ecxAfterDirectCall: directCallReturnEax,
                stackCleanupReached: true,
                ecxTestReached: true,
                jnzTaken: false,
                zeroRouteIndirectCallReached: true,
                zeroRouteIndirectCallReturnedNormally: false,
                unreachedZeroRoutePostCallEcxCaptureProvided:
                    capture.ZeroRoutePostCallEcx.HasValue,
                instructionsExecuted: InstructionsInOrder,
                stopAt: ZeroRouteIndirectCallRawFileOffset);
        }

        if (capture.ZeroRoutePostCallEcx is not uint postCallEcx)
        {
            return Stop(
                capture,
                FrameHandoffEmptySlotCallOutcome.MissingZeroRoutePostCallEcxCapture,
                pushTenReached: true,
                directCallReached: true,
                directCallReturnedNormally: true,
                capturedDirectCallReturnEax: directCallReturnEax,
                directCallReturnEaxCaptured: true,
                eaxCopiedToEcx: true,
                ecxAfterDirectCall: directCallReturnEax,
                stackCleanupReached: true,
                ecxTestReached: true,
                jnzTaken: false,
                zeroRouteIndirectCallReached: true,
                zeroRouteIndirectCallReturnedNormally: true,
                normalBoundaryReached: true,
                instructionsExecuted: InstructionsInOrder,
                stopAt: NormalBoundaryRawFileOffset);
        }

        return Stop(
            capture,
            FrameHandoffEmptySlotCallOutcome.ZeroRouteReturnedNormally,
            pushTenReached: true,
            directCallReached: true,
            directCallReturnedNormally: true,
            capturedDirectCallReturnEax: directCallReturnEax,
            directCallReturnEaxCaptured: true,
            eaxCopiedToEcx: true,
            ecxAfterDirectCall: directCallReturnEax,
            stackCleanupReached: true,
            ecxTestReached: true,
            jnzTaken: false,
            zeroRouteIndirectCallReached: true,
            zeroRouteIndirectCallReturnedNormally: true,
            capturedZeroRoutePostCallEcx: postCallEcx,
            zeroRoutePostCallEcxCaptured: true,
            normalBoundaryReached: true,
            instructionsExecuted: InstructionsInOrder,
            stopAt: NormalBoundaryRawFileOffset);
    }

    private static FrameHandoffEmptySlotCallPlan Stop(
        FrameHandoffEmptySlotCallCapture capture,
        FrameHandoffEmptySlotCallOutcome outcome,
        bool pushTenReached = false,
        bool directCallReached = false,
        bool? directCallReturnedNormally = null,
        uint? capturedDirectCallReturnEax = null,
        bool directCallReturnEaxCaptured = false,
        bool eaxCopiedToEcx = false,
        uint? ecxAfterDirectCall = null,
        bool stackCleanupReached = false,
        bool ecxTestReached = false,
        bool? jnzTaken = null,
        bool zeroRouteIndirectCallReached = false,
        bool? zeroRouteIndirectCallReturnedNormally = null,
        uint? capturedZeroRoutePostCallEcx = null,
        bool zeroRoutePostCallEcxCaptured = false,
        bool unreachedDirectCallReturnCaptureProvided = false,
        bool unreachedDirectCallReturnEaxCaptureProvided = false,
        bool unreachedZeroRouteIndirectCallReturnCaptureProvided = false,
        bool unreachedZeroRoutePostCallEcxCaptureProvided = false,
        bool normalBoundaryReached = false,
        ImmutableArray<FrameHandoffEmptySlotCallInstruction> instructionsExecuted = default,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            pushTenReached,
            pushTenReached ? StackBytesPushed : null,
            pushTenReached ? ExplicitPushValue : null,
            directCallReached,
            directCallReturnedNormally,
            capturedDirectCallReturnEax,
            directCallReturnEaxCaptured,
            eaxCopiedToEcx,
            ecxAfterDirectCall,
            stackCleanupReached,
            stackCleanupReached ? StackCleanupByteCount : null,
            ecxTestReached,
            jnzTaken,
            zeroRouteIndirectCallReached,
            zeroRouteIndirectCallReturnedNormally,
            capturedZeroRoutePostCallEcx,
            zeroRoutePostCallEcxCaptured,
            unreachedDirectCallReturnCaptureProvided,
            unreachedDirectCallReturnEaxCaptureProvided,
            unreachedZeroRouteIndirectCallReturnCaptureProvided,
            unreachedZeroRoutePostCallEcxCaptureProvided,
            normalBoundaryReached,
            instructionsExecuted.IsDefault
                ? ImmutableArray<FrameHandoffEmptySlotCallInstruction>.Empty
                : instructionsExecuted,
            stopAt);
}
