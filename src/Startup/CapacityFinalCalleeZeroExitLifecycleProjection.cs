using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeZeroExitLifecycleCapture(
    CapacityFinalCalleeAlignmentDispatchPlan AlignmentDispatchPlan,
    uint? NativeGlobalDwordBeforeSubtraction,
    CapacityFinalCalleeExitCapture Exit);

public enum CapacityFinalCalleeZeroExitLifecycleOutcome
{
    DispatchNotAdmitted,
    MissingNativeGlobalDword,
    ExitBlocked,
    ReturnedNormally
}

public readonly record struct CapacityFinalCalleeZeroExitInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeZeroExitGlobalAccess(
    uint RawFileOffset,
    uint Address,
    bool ReadReached,
    bool ValueCaptured,
    uint? ValueBefore,
    bool WriteReached,
    uint? ValueAfter,
    uint? Subtrahend);

public readonly record struct CapacityFinalCalleeZeroExitLifecyclePlan(
    CapacityFinalCalleeZeroExitLifecycleCapture Capture,
    CapacityFinalCalleeZeroExitLifecycleOutcome Outcome,
    bool DispatchComputedZeroAdmitted,
    bool SubtractionEcxDerived,
    uint? SubtractionEcx,
    CapacityFinalCalleeZeroExitGlobalAccess GlobalAccess,
    CapacityFinalCalleeExitPlan ExitPlan,
    bool ExitEntryAdmitted,
    bool ExitReturnedNormally,
    bool UnreachedNativeGlobalDwordCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Follows only a clean computed-ECX-zero dispatch branch through its native global SUB
/// and JMP to the existing callee-exit projection. The global read and write remain
/// explicit even though the admitted ECX value is zero; exit register captures stay
/// independent and the indirect call remains opaque.
/// </summary>
public static class CapacityFinalCalleeZeroExitLifecycleProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset =
        CapacityFinalCalleeAlignmentDispatchProjection.ComputedEcxZeroBranchDestinationRawFileOffset;
    public const uint SubtractRawFileOffset = 0x012C8E3A;
    public const uint JumpRawFileOffset = 0x012C8E40;
    public const uint SubtractWindowBoundaryRawFileOffsetExclusive = 0x012C8E45;
    public const uint ExitEntryRawFileOffset =
        CapacityFinalCalleeExitProjection.EntryRawFileOffset;
    public const uint BoundaryRawFileOffsetExclusive =
        CapacityFinalCalleeExitProjection.ReturnBoundaryRawFileOffsetExclusive;
    public const uint NativeGlobalDwordVirtualAddress = 0x01EDAF78;
    public const uint JumpDestinationRawFileOffset =
        CapacityFinalCalleeExitProjection.EntryRawFileOffset;

    public const string CapturedWindowBytes =
        "29 0D 78 AF ED 01 E9 FC 05 00 00";

    public static ImmutableArray<CapacityFinalCalleeZeroExitInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new CapacityFinalCalleeZeroExitInstructionEvidence(
                "Subtract ECX from the dword at PE VA 1EDAF78",
                SubtractRawFileOffset,
                SubtractRawFileOffset,
                RawFileOffsetToVirtualAddress(SubtractRawFileOffset),
                "29 0D 78 AF ED 01",
                NativeGlobalDwordVirtualAddress),
            new CapacityFinalCalleeZeroExitInstructionEvidence(
                "Jump to the reviewed callee-exit entry at raw 12C9441",
                JumpRawFileOffset,
                JumpRawFileOffset,
                RawFileOffsetToVirtualAddress(JumpRawFileOffset),
                "E9 FC 05 00 00",
                BranchTargetRawFileOffset: JumpDestinationRawFileOffset,
                BranchTargetVirtualAddress:
                    CapacityFinalCalleeExitProjection.EntryVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeZeroExitLifecyclePlan Project(
        CapacityFinalCalleeZeroExitLifecycleCapture capture)
    {
        CapacityFinalCalleeAlignmentDispatchPlan dispatchPlan =
            capture.AlignmentDispatchPlan;
        if (!IsExactCleanComputedZero(dispatchPlan))
        {
            CapacityFinalCalleeExitPlan blockedExit =
                CapacityFinalCalleeExitProjection.Project(
                    capture.Exit with { EntryValidated = false });
            return new(
                capture,
                CapacityFinalCalleeZeroExitLifecycleOutcome.DispatchNotAdmitted,
                DispatchComputedZeroAdmitted: false,
                SubtractionEcxDerived: false,
                SubtractionEcx: null,
                new(
                    SubtractRawFileOffset,
                    NativeGlobalDwordVirtualAddress,
                    ReadReached: false,
                    ValueCaptured: false,
                    ValueBefore: null,
                    WriteReached: false,
                    ValueAfter: null,
                    Subtrahend: null),
                blockedExit,
                ExitEntryAdmitted: false,
                ExitReturnedNormally: false,
                UnreachedNativeGlobalDwordCaptureProvided:
                    capture.NativeGlobalDwordBeforeSubtraction.HasValue,
                StopAtRawFileOffsetExclusive:
                    dispatchPlan.Outcome
                        == CapacityFinalCalleeAlignmentDispatchOutcome.ComputedEcxZero
                        ? EntryRawFileOffset
                        : dispatchPlan.StopAtRawFileOffsetExclusive);
        }

        const uint subtractionEcx = 0;
        if (capture.NativeGlobalDwordBeforeSubtraction is not uint valueBefore)
        {
            CapacityFinalCalleeExitPlan blockedExit =
                CapacityFinalCalleeExitProjection.Project(
                    capture.Exit with { EntryValidated = false });
            return new(
                capture,
                CapacityFinalCalleeZeroExitLifecycleOutcome.MissingNativeGlobalDword,
                DispatchComputedZeroAdmitted: true,
                SubtractionEcxDerived: true,
                SubtractionEcx: subtractionEcx,
                new(
                    SubtractRawFileOffset,
                    NativeGlobalDwordVirtualAddress,
                    ReadReached: true,
                    ValueCaptured: false,
                    ValueBefore: null,
                    WriteReached: false,
                    ValueAfter: null,
                    Subtrahend: subtractionEcx),
                blockedExit,
                ExitEntryAdmitted: false,
                ExitReturnedNormally: false,
                UnreachedNativeGlobalDwordCaptureProvided: false,
                StopAtRawFileOffsetExclusive: SubtractRawFileOffset);
        }

        uint valueAfter = unchecked(valueBefore - subtractionEcx);
        CapacityFinalCalleeExitPlan exitPlan =
            CapacityFinalCalleeExitProjection.Project(
                capture.Exit with { EntryValidated = true });
        bool exitReturnedNormally = IsCleanReturnedExit(exitPlan);
        return new(
            capture,
            exitReturnedNormally
                ? CapacityFinalCalleeZeroExitLifecycleOutcome.ReturnedNormally
                : CapacityFinalCalleeZeroExitLifecycleOutcome.ExitBlocked,
            DispatchComputedZeroAdmitted: true,
            SubtractionEcxDerived: true,
            SubtractionEcx: subtractionEcx,
            new(
                SubtractRawFileOffset,
                NativeGlobalDwordVirtualAddress,
                ReadReached: true,
                ValueCaptured: true,
                ValueBefore: valueBefore,
                WriteReached: true,
                ValueAfter: valueAfter,
                Subtrahend: subtractionEcx),
            exitPlan,
            ExitEntryAdmitted: true,
            ExitReturnedNormally: exitReturnedNormally,
            UnreachedNativeGlobalDwordCaptureProvided: false,
            StopAtRawFileOffsetExclusive:
                exitReturnedNormally
                    ? BoundaryRawFileOffsetExclusive
                    : exitPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsCleanReturnedExit(CapacityFinalCalleeExitPlan plan) =>
        (plan.Outcome is CapacityFinalCalleeExitOutcome.CallSkipped
            or CapacityFinalCalleeExitOutcome.ReturnedNormally)
        && plan.ReturnReached
        && plan.StopAtRawFileOffsetExclusive
            == CapacityFinalCalleeExitProjection.ReturnBoundaryRawFileOffsetExclusive
        && !plan.UnreachedTestedByteCaptureProvided
        && !plan.UnreachedStackPopEdiCaptureProvided
        && !plan.UnreachedStackPopEsiCaptureProvided
        && !plan.UnreachedStackPopEbxCaptureProvided
        && !plan.UnreachedCallCellTargetCaptureProvided
        && !plan.UnreachedCallReturnStatusProvided
        && !plan.UnreachedCalleeOutputEbxCaptureProvided
        && !plan.UnreachedCalleeOutputEdiCaptureProvided
        && !plan.UnreachedCalleeOutputEsiCaptureProvided
        && !plan.UnreachedStackPopEbpCaptureProvided;

    private static bool IsExactCleanComputedZero(
        CapacityFinalCalleeAlignmentDispatchPlan plan)
    {
        CapacityFinalCalleeAlignmentDispatchPlan expected =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(plan.Capture);
        return plan.Capture == expected.Capture
            && plan.Outcome == CapacityFinalCalleeAlignmentDispatchOutcome.ComputedEcxZero
            && plan.Outcome == expected.Outcome
            && plan.Reads.SequenceEqual(expected.Reads)
            && plan.FrameWrites.SequenceEqual(expected.FrameWrites)
            && plan.Pushes.SequenceEqual(expected.Pushes)
            && plan.EAX == expected.EAX
            && plan.ESI == expected.ESI
            && plan.ECX == 0
            && plan.ECX == expected.ECX
            && plan.EDI == expected.EDI
            && plan.EDX == expected.EDX
            && plan.EBX == expected.EBX
            && plan.ESP == expected.ESP
            && plan.TestAlReached == expected.TestAlReached
            && plan.TestAlFlags == expected.TestAlFlags
            && plan.TestAlBranchReached == expected.TestAlBranchReached
            && plan.TestAlBranchTaken == expected.TestAlBranchTaken
            && plan.TestAlSelectedDestinationRawFileOffset
                == expected.TestAlSelectedDestinationRawFileOffset
            && plan.TestAlSelectedDestinationVirtualAddress
                == expected.TestAlSelectedDestinationVirtualAddress
            && plan.EntryEsiReadReached == expected.EntryEsiReadReached
            && plan.EntryEsiDwordCaptured == expected.EntryEsiDwordCaptured
            && plan.EntryEsiDword == expected.EntryEsiDword
            && plan.TestEntryEcxReached == expected.TestEntryEcxReached
            && plan.TestEntryEcxFlags == expected.TestEntryEcxFlags
            && plan.TestEntryEcxBranchReached == expected.TestEntryEcxBranchReached
            && plan.TestEntryEcxBranchTaken == expected.TestEntryEcxBranchTaken
            && plan.TestEntryEcxSelectedDestinationRawFileOffset
                == expected.TestEntryEcxSelectedDestinationRawFileOffset
            && plan.TestEntryEcxSelectedDestinationVirtualAddress
                == expected.TestEntryEcxSelectedDestinationVirtualAddress
            && plan.TestComputedEcxReached == expected.TestComputedEcxReached
            && plan.TestComputedEcxFlags == expected.TestComputedEcxFlags
            && plan.TestComputedEcxBranchReached == expected.TestComputedEcxBranchReached
            && plan.TestComputedEcxBranchTaken == expected.TestComputedEcxBranchTaken
            && plan.TestComputedEcxSelectedDestinationRawFileOffset
                == EntryRawFileOffset
            && plan.TestComputedEcxSelectedDestinationVirtualAddress
                == RawFileOffsetToVirtualAddress(EntryRawFileOffset)
            && plan.TestComputedEcxSelectedDestinationRawFileOffset
                == expected.TestComputedEcxSelectedDestinationRawFileOffset
            && plan.TestComputedEcxSelectedDestinationVirtualAddress
                == expected.TestComputedEcxSelectedDestinationVirtualAddress
            && plan.GlobalDispatchReadReached == expected.GlobalDispatchReadReached
            && plan.GlobalDispatchDwordCaptured == expected.GlobalDispatchDwordCaptured
            && plan.GlobalDispatchDword == expected.GlobalDispatchDword
            && plan.CallCellReadReached == expected.CallCellReadReached
            && plan.CallCellTargetCaptured == expected.CallCellTargetCaptured
            && plan.CallCellTargetVirtualAddress == expected.CallCellTargetVirtualAddress
            && plan.CallReached == expected.CallReached
            && plan.CallReturnedNormally == expected.CallReturnedNormally
            && plan.CallRegisterOutputsUnknown == expected.CallRegisterOutputsUnknown
            && !plan.UnreachedEntryEsiDwordCaptureProvided
            && !plan.UnreachedGlobalDispatchDwordCaptureProvided
            && !plan.UnreachedCallCellTargetCaptureProvided
            && !plan.UnreachedCallReturnStatusProvided
            && plan.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && expected.Capture.CaptureValidatedEntry
            && expected.TestAlFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && expected.TestAlBranchTaken == false
            && expected.TestEntryEcxFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && expected.TestEntryEcxBranchTaken == false
            && expected.TestComputedEcxFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && expected.TestComputedEcxBranchTaken == true
            && expected.EntryEsiReadReached
            && expected.EntryEsiDwordCaptured
            && !expected.GlobalDispatchReadReached
            && !expected.CallCellReadReached
            && !expected.CallReached;
    }
}
