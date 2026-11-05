using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolDrainCapture(
    uint CapturedPoolPointer,
    uint? CapturedPopResult);

public enum UiShaderSourcePoolDrainActionKind
{
    ReturnNormallyWithoutPoolCall,
    CallPopForCapturedResult,
    CallHelperForCapturedNode,
    CallPopForUnmodeledContinuation,
    ConditionalLoopBackUnmodeled,
    TailTransferCapturedPoolToHelper
}

public readonly record struct UiShaderSourcePoolDrainAction(
    UiShaderSourcePoolDrainActionKind Kind,
    uint InstructionRawFileOffset,
    uint? Argument = null,
    uint? TargetRawFileOffset = null,
    uint? BranchTargetRawFileOffset = null);

public readonly record struct UiShaderSourcePoolDrainPlan(
    UiShaderSourcePoolDrainCapture Capture,
    ImmutableArray<UiShaderSourcePoolDrainAction> Actions,
    bool EndsAtCapturedPoolTailTransfer);

public readonly record struct UiShaderSourcePoolDrainInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? TargetRawFileOffset = null);

/// <summary>
/// Projects the singleton teardown's captured-pool drain continuation for one
/// pop result only. It does not simulate later iterations or infer helper effects.
/// </summary>
public static class UiShaderSourcePoolDrainProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x12CC1D0;
    public const uint PoolTestRawFileOffset = 0x12CC24F;
    public const uint NullPoolBranchRawFileOffset = 0x12CC251;
    public const uint NullPoolReturnRawFileOffset = 0x12CC27C;
    public const uint InitialPopArgumentSetupRawFileOffset = 0x12CC253;
    public const uint InitialPopCallRawFileOffset = 0x12CC256;
    public const uint CapturedPopTestRawFileOffset = 0x12CC25B;
    public const uint EmptyPopBranchRawFileOffset = 0x12CC25D;
    public const uint CapturedNodeArgumentSetupRawFileOffset = 0x12CC260;
    public const uint CapturedNodeHelperCallRawFileOffset = 0x12CC262;
    public const uint NextPopArgumentSetupRawFileOffset = 0x12CC267;
    public const uint NextPopCallRawFileOffset = 0x12CC26A;
    public const uint NextPopTestRawFileOffset = 0x12CC26F;
    public const uint LoopBackBranchRawFileOffset = 0x12CC271;
    public const uint LoopBackTargetRawFileOffset = 0x12CC260;
    public const uint PoolTailArgumentSetupRawFileOffset = 0x12CC273;
    public const uint PoolTailTransferRawFileOffset = 0x12CC277;
    public const uint EntryReturnRawFileOffset = 0x12CC27E;
    public const uint EntryEndRawFileOffsetExclusive = 0x12CC27F;

    public const uint PoolPopHelperRawFileOffset = 0x12CDAE0;
    public const uint NodeAndPoolHelperRawFileOffset = 0x12C8D60;
    public const int PoolHeadOffset = 8;

    public const string DrainContinuationBytes =
        "85 FF 74 29 8D 4F 08 E8 85 18 00 00 85 C0 74 14 " +
        "90 8B C8 E8 F9 CA FF FF 8D 4F 08 E8 71 18 00 00 " +
        "85 C0 75 ED 8B CF 5F 5E E9 E4 CA FF FF 5F 5E C3";

    public static IReadOnlyList<UiShaderSourcePoolDrainInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Test the captured pool pointer in EDI",
                PoolTestRawFileOffset,
                "85 FF"),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Skip all pool calls when the captured pointer is null",
                NullPoolBranchRawFileOffset,
                "74 29",
                NullPoolReturnRawFileOffset),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Set ECX to captured pool + 8 for the first pop",
                InitialPopArgumentSetupRawFileOffset,
                "8D 4F 08"),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Call the pool-pop helper with ECX = captured pool + 8",
                InitialPopCallRawFileOffset,
                "E8 85 18 00 00",
                PoolPopHelperRawFileOffset),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Test the captured pop result in EAX",
                CapturedPopTestRawFileOffset,
                "85 C0"),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Select the pool-tail path for an empty captured pop",
                EmptyPopBranchRawFileOffset,
                "74 14",
                PoolTailArgumentSetupRawFileOffset),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Move the captured nonnull pop result from EAX to ECX",
                CapturedNodeArgumentSetupRawFileOffset,
                "8B C8"),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Call the node helper with ECX = captured nonnull pop result",
                CapturedNodeHelperCallRawFileOffset,
                "E8 F9 CA FF FF",
                NodeAndPoolHelperRawFileOffset),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Set ECX to captured pool + 8 for the next, unmodeled pop",
                NextPopArgumentSetupRawFileOffset,
                "8D 4F 08"),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Pop again with ECX = captured pool + 8 after the node helper call",
                NextPopCallRawFileOffset,
                "E8 71 18 00 00",
                PoolPopHelperRawFileOffset),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Test the next, unmodeled pop result",
                NextPopTestRawFileOffset,
                "85 C0"),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Conditionally loop to the captured-node helper path",
                LoopBackBranchRawFileOffset,
                "75 ED",
                LoopBackTargetRawFileOffset),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Move the captured pool pointer from EDI to ECX on the empty-pop path",
                PoolTailArgumentSetupRawFileOffset,
                "8B CF"),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Tail-transfer the captured pool pointer to the same helper after an empty pop",
                PoolTailTransferRawFileOffset,
                "E9 E4 CA FF FF",
                NodeAndPoolHelperRawFileOffset),
            new UiShaderSourcePoolDrainInstructionEvidence(
                "Return normally from the null-pool path",
                NullPoolReturnRawFileOffset,
                "5F 5E C3")
        });

    /// <summary>
    /// Projects the null-pool return or one captured pop result. For a nonempty
    /// result it records the node-helper call and following pop, but not that
    /// following pop's value or any later drain iteration.
    /// </summary>
    public static UiShaderSourcePoolDrainPlan Project(
        UiShaderSourcePoolDrainCapture capture)
    {
        if (capture.CapturedPoolPointer == 0)
        {
            if (capture.CapturedPopResult.HasValue)
            {
                throw new ArgumentException(
                    "A null captured pool cannot have a captured pop result.",
                    nameof(capture));
            }

            return new UiShaderSourcePoolDrainPlan(
                capture,
                ImmutableArray.Create(
                    new UiShaderSourcePoolDrainAction(
                        UiShaderSourcePoolDrainActionKind.ReturnNormallyWithoutPoolCall,
                        NullPoolBranchRawFileOffset,
                        BranchTargetRawFileOffset: NullPoolReturnRawFileOffset)),
                EndsAtCapturedPoolTailTransfer: false);
        }

        uint popResult = capture.CapturedPopResult
            ?? throw new ArgumentException(
                "A nonnull captured pool requires its pop result.",
                nameof(capture));
        uint poolHead = unchecked(capture.CapturedPoolPointer + PoolHeadOffset);

        if (popResult == 0)
        {
            return new UiShaderSourcePoolDrainPlan(
                capture,
                ImmutableArray.Create(
                    new UiShaderSourcePoolDrainAction(
                        UiShaderSourcePoolDrainActionKind.CallPopForCapturedResult,
                        InitialPopCallRawFileOffset,
                        poolHead,
                        PoolPopHelperRawFileOffset),
                    new UiShaderSourcePoolDrainAction(
                        UiShaderSourcePoolDrainActionKind.TailTransferCapturedPoolToHelper,
                        PoolTailTransferRawFileOffset,
                        capture.CapturedPoolPointer,
                        NodeAndPoolHelperRawFileOffset)),
                EndsAtCapturedPoolTailTransfer: true);
        }

        return new UiShaderSourcePoolDrainPlan(
            capture,
            ImmutableArray.Create(
                new UiShaderSourcePoolDrainAction(
                    UiShaderSourcePoolDrainActionKind.CallPopForCapturedResult,
                    InitialPopCallRawFileOffset,
                    poolHead,
                    PoolPopHelperRawFileOffset),
                new UiShaderSourcePoolDrainAction(
                    UiShaderSourcePoolDrainActionKind.CallHelperForCapturedNode,
                    CapturedNodeHelperCallRawFileOffset,
                    popResult,
                    NodeAndPoolHelperRawFileOffset),
                new UiShaderSourcePoolDrainAction(
                    UiShaderSourcePoolDrainActionKind.CallPopForUnmodeledContinuation,
                    NextPopCallRawFileOffset,
                    poolHead,
                    PoolPopHelperRawFileOffset),
                new UiShaderSourcePoolDrainAction(
                    UiShaderSourcePoolDrainActionKind.ConditionalLoopBackUnmodeled,
                    LoopBackBranchRawFileOffset,
                    BranchTargetRawFileOffset: LoopBackTargetRawFileOffset)),
            EndsAtCapturedPoolTailTransfer: false);
    }
}
