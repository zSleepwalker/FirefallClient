using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeIndexedRootMaskCapture(
    bool CaptureValidatedEntryRegisters,
    uint? EntryEBP,
    uint? EntryESI,
    uint? EntryEAX,
    uint? FreshFrameMinus14,
    uint? FreshFirstIndex,
    uint? FirstIndexedTableValue,
    uint? FreshFrameMinusC,
    uint? FreshSecondIndex,
    uint? FreshFrameMinus8,
    uint? FreshThirdIndex,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleeIndexedRootMaskOutcome
{
    EntryNotValidated,
    MissingEntryEBP,
    MissingFreshFrameMinus14,
    FreshFrameMinus14ZeroExit,
    MissingEntryESI,
    MissingFreshFirstIndex,
    MissingFirstIndexedTableValue,
    MissingFreshFrameMinusC,
    FirstComparisonUnequal,
    MissingFreshSecondIndex,
    MissingEntryEAX,
    StoredEntryEAXNonzeroExit,
    MissingFreshFrameMinus8,
    MissingFreshThirdIndex,
    MissingFreshGlobalValue,
    GlobalValueUpdated
}

public readonly record struct CapacityFinalCalleeIndexedRootMaskFlags(
    bool Carry,
    bool Parity,
    bool AuxiliaryCarry,
    bool Zero,
    bool Sign,
    bool Overflow);

public readonly record struct CapacityFinalCalleeIndexedRootMaskInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeIndexedRootMaskPlan(
    CapacityFinalCalleeIndexedRootMaskCapture Capture,
    CapacityFinalCalleeIndexedRootMaskOutcome Outcome,
    uint? FreshFrameMinus14Address,
    bool FreshFrameMinus14ReadReached,
    bool FreshFrameMinus14Captured,
    uint? FreshFrameMinus14,
    bool ZeroTestReached,
    bool? ZeroTestResult,
    bool ZeroBranchReached,
    bool? ZeroBranchTaken,
    uint? ZeroBranchDestinationRawFileOffset,
    uint? FirstIndexAddress,
    bool FirstIndexReadReached,
    bool FirstIndexCaptured,
    uint? FreshFirstIndex,
    uint? FirstIndexedTableAddress,
    bool FirstIndexedTableReadReached,
    bool FirstIndexedTableValueCaptured,
    uint? FirstIndexedTableValue,
    bool FirstCompareReached,
    uint? FirstCompareResult,
    CapacityFinalCalleeIndexedRootMaskFlags? FirstCompareFlags,
    bool? FirstCompareEqual,
    uint? FreshFrameMinusCAddress,
    bool FreshFrameMinusCReadReached,
    bool FreshFrameMinusCCaptured,
    uint? FreshFrameMinusC,
    bool UnequalBranchReached,
    bool? UnequalBranchTaken,
    uint? UnequalBranchDestinationRawFileOffset,
    uint? SecondIndexAddress,
    bool SecondIndexReadReached,
    bool SecondIndexCaptured,
    uint? FreshSecondIndex,
    uint? TableWriteAddress,
    bool TableWriteReached,
    uint? TableWriteValue,
    bool StoredValueTestReached,
    bool? StoredValueIsZero,
    bool NonzeroBranchReached,
    bool? NonzeroBranchTaken,
    uint? NonzeroBranchDestinationRawFileOffset,
    uint? FreshFrameMinus8Address,
    bool FreshFrameMinus8ReadReached,
    bool FreshFrameMinus8Captured,
    uint? FreshFrameMinus8,
    uint? ThirdIndexAddress,
    bool ThirdIndexReadReached,
    bool ThirdIndexCaptured,
    uint? FreshThirdIndex,
    uint? ShiftCount,
    uint? ShiftedValue,
    uint? NotMask,
    uint? GlobalAddress,
    bool GlobalReadReached,
    bool GlobalValueCaptured,
    uint? GlobalValueBefore,
    bool GlobalWriteReached,
    uint? GlobalValueAfter,
    bool UnreachedEntryRegisterCaptureProvided,
    bool UnreachedFreshFrameMinus14CaptureProvided,
    bool UnreachedFreshFirstIndexCaptureProvided,
    bool UnreachedFirstIndexedTableValueCaptureProvided,
    bool UnreachedFreshFrameMinusCCaptureProvided,
    bool UnreachedFreshSecondIndexCaptureProvided,
    bool UnreachedFreshFrameMinus8CaptureProvided,
    bool UnreachedFreshThirdIndexCaptureProvided,
    bool UnreachedFreshGlobalValueCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the indexed root-table store and its independent optional mask update.
/// MOV reloads do not replace the first CMP flags used by the following JNZ.
/// </summary>
public static class CapacityFinalCalleeIndexedRootMaskProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8FFE;
    public const uint EntryVirtualAddress = 0x016C9BFE;
    public const uint ZeroTestRawFileOffset = 0x012C9001;
    public const uint ZeroBranchRawFileOffset = 0x012C9003;
    public const uint FirstIndexLoadRawFileOffset = 0x012C9009;
    public const uint FirstTableCompareRawFileOffset = 0x012C900C;
    public const uint FreshFrameMinusCLoadRawFileOffset = 0x012C9013;
    public const uint UnequalBranchRawFileOffset = 0x012C9016;
    public const uint SecondIndexLoadRawFileOffset = 0x012C9018;
    public const uint TableStoreRawFileOffset = 0x012C901B;
    public const uint StoredValueTestRawFileOffset = 0x012C9022;
    public const uint NonzeroBranchRawFileOffset = 0x012C9024;
    public const uint FrameMinus8LoadRawFileOffset = 0x012C9026;
    public const uint ThirdIndexLoadRawFileOffset = 0x012C9029;
    public const uint ShiftRawFileOffset = 0x012C902C;
    public const uint NotRawFileOffset = 0x012C902E;
    public const uint GlobalAndRawFileOffset = 0x012C9030;
    public const uint JumpRawFileOffset = 0x012C9036;
    public const uint WindowBoundaryRawFileOffsetExclusive = 0x012C903B;

    public const uint ZeroExitRawFileOffset = 0x012C8EF8;
    public const uint UnequalExitRawFileOffset = 0x012C903B;
    public const uint NonzeroExitRawFileOffset = 0x012C9058;
    public const uint MaskContinuationRawFileOffset = 0x012C8EF2;
    public const uint ZeroExitVirtualAddress = 0x016C9AF8;
    public const uint UnequalExitVirtualAddress = 0x016C9C3B;
    public const uint NonzeroExitVirtualAddress = 0x016C9C58;
    public const uint MaskContinuationVirtualAddress = 0x016C9AF2;

    public const uint FrameMinus14Displacement = 0x14;
    public const uint FrameMinusCDisplacement = 0xC;
    public const uint FrameMinus8Displacement = 8;
    public const uint EsiIndexDisplacement = 0x1C;
    public const uint IndexedTablePeVirtualAddress = 0x01EDAEF8;
    public const uint GlobalMaskVirtualAddress = 0x01EDADCC;
    public const int IndexedTableScaleShift = 2;
    public const int ShiftCountMask = 0x1F;

    public const string CapturedWindowBytes =
        "8B 4D EC 85 C9 0F 84 EF FE FF FF 8B 56 1C 3B 34 95 F8 AE ED 01 "
        + "8B 55 F4 75 23 8B 5E 1C 89 04 9D F8 AE ED 01 85 C0 75 32 "
        + "8B 45 F8 8B 4E 1C D3 E0 F7 D0 21 05 CC AD ED 01 E9 B7 FE FF FF";

    public static ImmutableArray<CapacityFinalCalleeIndexedRootMaskInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load a fresh ECX DWORD from entry EBP-14",
                EntryRawFileOffset, "8B 4D EC"),
            Instruction("TEST ECX,ECX",
                ZeroTestRawFileOffset, "85 C9"),
            Instruction("JZ to the zero exit",
                ZeroBranchRawFileOffset, "0F 84 EF FE FF FF",
                branchTargetRawFileOffset: ZeroExitRawFileOffset,
                branchTargetVirtualAddress: ZeroExitVirtualAddress),
            Instruction("Load an independent first DWORD index from entry ESI+1C into EDX",
                FirstIndexLoadRawFileOffset, "8B 56 1C"),
            Instruction("Compare entry ESI against table[1EDAEF8+EDX*4]",
                FirstTableCompareRawFileOffset, "3B 34 95 F8 AE ED 01",
                peVirtualAddressOperand: IndexedTablePeVirtualAddress),
            Instruction("Reload a fresh DWORD from entry EBP-0C into EDX without changing flags",
                FreshFrameMinusCLoadRawFileOffset, "8B 55 F4"),
            Instruction("JNZ to the unequal exit using the retained table-comparison flags",
                UnequalBranchRawFileOffset, "75 23",
                branchTargetRawFileOffset: UnequalExitRawFileOffset,
                branchTargetVirtualAddress: UnequalExitVirtualAddress),
            Instruction("Load a second independent DWORD index from entry ESI+1C into EBX",
                SecondIndexLoadRawFileOffset, "8B 5E 1C"),
            Instruction("Store entry EAX to table[1EDAEF8+EBX*4]",
                TableStoreRawFileOffset, "89 04 9D F8 AE ED 01",
                peVirtualAddressOperand: IndexedTablePeVirtualAddress),
            Instruction("TEST EAX,EAX after the indexed table write",
                StoredValueTestRawFileOffset, "85 C0"),
            Instruction("JNZ to the nonzero exit",
                NonzeroBranchRawFileOffset, "75 32",
                branchTargetRawFileOffset: NonzeroExitRawFileOffset,
                branchTargetVirtualAddress: NonzeroExitVirtualAddress),
            Instruction("Load a fresh DWORD from entry EBP-8 into EAX",
                FrameMinus8LoadRawFileOffset, "8B 45 F8"),
            Instruction("Load a third independent DWORD index from entry ESI+1C into ECX",
                ThirdIndexLoadRawFileOffset, "8B 4E 1C"),
            Instruction("Shift EAX left by CL with the x86 five-bit count mask",
                ShiftRawFileOffset, "D3 E0"),
            Instruction("Invert the shifted DWORD",
                NotRawFileOffset, "F7 D0"),
            Instruction("AND the inverted DWORD into the fresh global DWORD",
                GlobalAndRawFileOffset, "21 05 CC AD ED 01",
                peVirtualAddressOperand: GlobalMaskVirtualAddress),
            Instruction("Jump to the mask continuation",
                JumpRawFileOffset, "E9 B7 FE FF FF",
                branchTargetRawFileOffset: MaskContinuationRawFileOffset,
                branchTargetVirtualAddress: MaskContinuationVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFrameMinus14Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus14Displacement);

    public static uint DeriveFrameMinusCAddress(uint entryEbp) =>
        unchecked(entryEbp - FrameMinusCDisplacement);

    public static uint DeriveFrameMinus8Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus8Displacement);

    public static uint DeriveEsiIndexAddress(uint entryEsi) =>
        unchecked(entryEsi + EsiIndexDisplacement);

    public static uint DeriveIndexedTableAddress(uint index) =>
        unchecked(IndexedTablePeVirtualAddress + (index << IndexedTableScaleShift));

    public static CapacityFinalCalleeIndexedRootMaskPlan Project(
        CapacityFinalCalleeIndexedRootMaskCapture capture)
    {
        if (!capture.CaptureValidatedEntryRegisters)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.EntryNotValidated,
                unreachedEntryRegisterCaptureProvided: HasEntryRegisterCapture(capture),
                stopAt: EntryRawFileOffset);
        }

        if (capture.EntryEBP is not uint entryEbp)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.MissingEntryEBP,
                freshFrameMinus14ReadReached: false,
                stopAt: EntryRawFileOffset);
        }

        uint frameMinus14Address = DeriveFrameMinus14Address(entryEbp);
        if (capture.FreshFrameMinus14 is not uint freshFrameMinus14)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshFrameMinus14,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                stopAt: EntryRawFileOffset);
        }

        bool zeroTest = freshFrameMinus14 == 0;
        if (zeroTest)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.FreshFrameMinus14ZeroExit,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                zeroTestReached: true,
                zeroTestResult: true,
                zeroBranchReached: true,
                zeroBranchTaken: true,
                zeroBranchDestinationRawFileOffset: ZeroExitRawFileOffset,
                stopAt: ZeroExitRawFileOffset);
        }

        if (capture.EntryESI is not uint entryEsi)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.MissingEntryESI,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                zeroTestReached: true,
                zeroTestResult: false,
                zeroBranchReached: true,
                zeroBranchTaken: false,
                firstIndexReadReached: false,
                stopAt: FirstIndexLoadRawFileOffset);
        }

        uint firstIndexAddress = DeriveEsiIndexAddress(entryEsi);
        if (capture.FreshFirstIndex is not uint firstIndex)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshFirstIndex,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                zeroTestReached: true,
                zeroTestResult: false,
                zeroBranchReached: true,
                zeroBranchTaken: false,
                firstIndexAddress: firstIndexAddress,
                firstIndexReadReached: true,
                stopAt: FirstIndexLoadRawFileOffset);
        }

        uint firstTableAddress = DeriveIndexedTableAddress(firstIndex);
        if (capture.FirstIndexedTableValue is not uint firstTableValue)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.MissingFirstIndexedTableValue,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                zeroTestReached: true,
                zeroTestResult: false,
                zeroBranchReached: true,
                zeroBranchTaken: false,
                firstIndexAddress: firstIndexAddress,
                firstIndexReadReached: true,
                firstIndexCaptured: true,
                freshFirstIndex: firstIndex,
                firstIndexedTableAddress: firstTableAddress,
                firstIndexedTableReadReached: true,
                stopAt: FirstTableCompareRawFileOffset);
        }

        uint compareResult = unchecked(entryEsi - firstTableValue);
        CapacityFinalCalleeIndexedRootMaskFlags compareFlags =
            SubtractFlags(entryEsi, firstTableValue, compareResult);
        bool equal = compareFlags.Zero;

        uint frameMinusCAddress = DeriveFrameMinusCAddress(entryEbp);
        if (capture.FreshFrameMinusC is not uint freshFrameMinusC)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshFrameMinusC,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                zeroTestReached: true,
                zeroTestResult: false,
                zeroBranchReached: true,
                zeroBranchTaken: false,
                firstIndexAddress: firstIndexAddress,
                firstIndexReadReached: true,
                firstIndexCaptured: true,
                freshFirstIndex: firstIndex,
                firstIndexedTableAddress: firstTableAddress,
                firstIndexedTableReadReached: true,
                firstIndexedTableValueCaptured: true,
                firstIndexedTableValue: firstTableValue,
                firstCompareReached: true,
                firstCompareResult: compareResult,
                firstCompareFlags: compareFlags,
                firstCompareEqual: equal,
                freshFrameMinusCAddress: frameMinusCAddress,
                freshFrameMinusCReadReached: true,
                stopAt: FreshFrameMinusCLoadRawFileOffset);
        }

        if (!equal)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.FirstComparisonUnequal,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                zeroTestReached: true,
                zeroTestResult: false,
                zeroBranchReached: true,
                zeroBranchTaken: false,
                firstIndexAddress: firstIndexAddress,
                firstIndexReadReached: true,
                firstIndexCaptured: true,
                freshFirstIndex: firstIndex,
                firstIndexedTableAddress: firstTableAddress,
                firstIndexedTableReadReached: true,
                firstIndexedTableValueCaptured: true,
                firstIndexedTableValue: firstTableValue,
                firstCompareReached: true,
                firstCompareResult: compareResult,
                firstCompareFlags: compareFlags,
                firstCompareEqual: false,
                freshFrameMinusCAddress: frameMinusCAddress,
                freshFrameMinusCReadReached: true,
                freshFrameMinusCCaptured: true,
                freshFrameMinusC: freshFrameMinusC,
                unequalBranchReached: true,
                unequalBranchTaken: true,
                unequalBranchDestinationRawFileOffset: UnequalExitRawFileOffset,
                stopAt: UnequalExitRawFileOffset);
        }

        uint secondIndexAddress = DeriveEsiIndexAddress(entryEsi);
        if (capture.FreshSecondIndex is not uint secondIndex)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshSecondIndex,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                zeroTestReached: true,
                zeroTestResult: false,
                zeroBranchReached: true,
                zeroBranchTaken: false,
                firstIndexAddress: firstIndexAddress,
                firstIndexReadReached: true,
                firstIndexCaptured: true,
                freshFirstIndex: firstIndex,
                firstIndexedTableAddress: firstTableAddress,
                firstIndexedTableReadReached: true,
                firstIndexedTableValueCaptured: true,
                firstIndexedTableValue: firstTableValue,
                firstCompareReached: true,
                firstCompareResult: compareResult,
                firstCompareFlags: compareFlags,
                firstCompareEqual: true,
                freshFrameMinusCAddress: frameMinusCAddress,
                freshFrameMinusCReadReached: true,
                freshFrameMinusCCaptured: true,
                freshFrameMinusC: freshFrameMinusC,
                unequalBranchReached: true,
                unequalBranchTaken: false,
                secondIndexAddress: secondIndexAddress,
                secondIndexReadReached: true,
                stopAt: SecondIndexLoadRawFileOffset);
        }

        if (capture.EntryEAX is not uint entryEax)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.MissingEntryEAX,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                zeroTestReached: true,
                zeroTestResult: false,
                zeroBranchReached: true,
                zeroBranchTaken: false,
                firstIndexAddress: firstIndexAddress,
                firstIndexReadReached: true,
                firstIndexCaptured: true,
                freshFirstIndex: firstIndex,
                firstIndexedTableAddress: firstTableAddress,
                firstIndexedTableReadReached: true,
                firstIndexedTableValueCaptured: true,
                firstIndexedTableValue: firstTableValue,
                firstCompareReached: true,
                firstCompareResult: compareResult,
                firstCompareFlags: compareFlags,
                firstCompareEqual: true,
                freshFrameMinusCAddress: frameMinusCAddress,
                freshFrameMinusCReadReached: true,
                freshFrameMinusCCaptured: true,
                freshFrameMinusC: freshFrameMinusC,
                unequalBranchReached: true,
                unequalBranchTaken: false,
                secondIndexAddress: secondIndexAddress,
                secondIndexReadReached: true,
                secondIndexCaptured: true,
                freshSecondIndex: secondIndex,
                stopAt: TableStoreRawFileOffset);
        }

        uint tableWriteAddress = DeriveIndexedTableAddress(secondIndex);
        if (entryEax != 0)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.StoredEntryEAXNonzeroExit,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                zeroTestReached: true,
                zeroTestResult: false,
                zeroBranchReached: true,
                zeroBranchTaken: false,
                firstIndexAddress: firstIndexAddress,
                firstIndexReadReached: true,
                firstIndexCaptured: true,
                freshFirstIndex: firstIndex,
                firstIndexedTableAddress: firstTableAddress,
                firstIndexedTableReadReached: true,
                firstIndexedTableValueCaptured: true,
                firstIndexedTableValue: firstTableValue,
                firstCompareReached: true,
                firstCompareResult: compareResult,
                firstCompareFlags: compareFlags,
                firstCompareEqual: true,
                freshFrameMinusCAddress: frameMinusCAddress,
                freshFrameMinusCReadReached: true,
                freshFrameMinusCCaptured: true,
                freshFrameMinusC: freshFrameMinusC,
                unequalBranchReached: true,
                unequalBranchTaken: false,
                secondIndexAddress: secondIndexAddress,
                secondIndexReadReached: true,
                secondIndexCaptured: true,
                freshSecondIndex: secondIndex,
                tableWriteAddress: tableWriteAddress,
                tableWriteReached: true,
                tableWriteValue: entryEax,
                storedValueTestReached: true,
                storedValueIsZero: false,
                nonzeroBranchReached: true,
                nonzeroBranchTaken: true,
                nonzeroBranchDestinationRawFileOffset: NonzeroExitRawFileOffset,
                stopAt: NonzeroExitRawFileOffset);
        }

        if (capture.FreshFrameMinus8 is not uint freshFrameMinus8)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshFrameMinus8,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                zeroTestReached: true,
                zeroTestResult: false,
                zeroBranchReached: true,
                zeroBranchTaken: false,
                firstIndexAddress: firstIndexAddress,
                firstIndexReadReached: true,
                firstIndexCaptured: true,
                freshFirstIndex: firstIndex,
                firstIndexedTableAddress: firstTableAddress,
                firstIndexedTableReadReached: true,
                firstIndexedTableValueCaptured: true,
                firstIndexedTableValue: firstTableValue,
                firstCompareReached: true,
                firstCompareResult: compareResult,
                firstCompareFlags: compareFlags,
                firstCompareEqual: true,
                freshFrameMinusCAddress: frameMinusCAddress,
                freshFrameMinusCReadReached: true,
                freshFrameMinusCCaptured: true,
                freshFrameMinusC: freshFrameMinusC,
                unequalBranchReached: true,
                unequalBranchTaken: false,
                secondIndexAddress: secondIndexAddress,
                secondIndexReadReached: true,
                secondIndexCaptured: true,
                freshSecondIndex: secondIndex,
                tableWriteAddress: tableWriteAddress,
                tableWriteReached: true,
                tableWriteValue: entryEax,
                storedValueTestReached: true,
                storedValueIsZero: true,
                nonzeroBranchReached: true,
                nonzeroBranchTaken: false,
                freshFrameMinus8Address: DeriveFrameMinus8Address(entryEbp),
                freshFrameMinus8ReadReached: true,
                stopAt: FrameMinus8LoadRawFileOffset);
        }

        uint thirdIndexAddress = DeriveEsiIndexAddress(entryEsi);
        if (capture.FreshThirdIndex is not uint thirdIndex)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshThirdIndex,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                zeroTestReached: true,
                zeroTestResult: false,
                zeroBranchReached: true,
                zeroBranchTaken: false,
                firstIndexAddress: firstIndexAddress,
                firstIndexReadReached: true,
                firstIndexCaptured: true,
                freshFirstIndex: firstIndex,
                firstIndexedTableAddress: firstTableAddress,
                firstIndexedTableReadReached: true,
                firstIndexedTableValueCaptured: true,
                firstIndexedTableValue: firstTableValue,
                firstCompareReached: true,
                firstCompareResult: compareResult,
                firstCompareFlags: compareFlags,
                firstCompareEqual: true,
                freshFrameMinusCAddress: frameMinusCAddress,
                freshFrameMinusCReadReached: true,
                freshFrameMinusCCaptured: true,
                freshFrameMinusC: freshFrameMinusC,
                unequalBranchReached: true,
                unequalBranchTaken: false,
                secondIndexAddress: secondIndexAddress,
                secondIndexReadReached: true,
                secondIndexCaptured: true,
                freshSecondIndex: secondIndex,
                tableWriteAddress: tableWriteAddress,
                tableWriteReached: true,
                tableWriteValue: entryEax,
                storedValueTestReached: true,
                storedValueIsZero: true,
                nonzeroBranchReached: true,
                nonzeroBranchTaken: false,
                freshFrameMinus8Address: DeriveFrameMinus8Address(entryEbp),
                freshFrameMinus8ReadReached: true,
                freshFrameMinus8Captured: true,
                freshFrameMinus8: freshFrameMinus8,
                thirdIndexAddress: thirdIndexAddress,
                thirdIndexReadReached: true,
                stopAt: ThirdIndexLoadRawFileOffset);
        }

        uint shiftCount = thirdIndex & ShiftCountMask;
        uint shiftedValue = unchecked(freshFrameMinus8 << (int)shiftCount);
        uint notMask = ~shiftedValue;
        if (capture.FreshGlobalValue is not uint freshGlobalValue)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleeIndexedRootMaskOutcome.MissingFreshGlobalValue,
                freshFrameMinus14Address: frameMinus14Address,
                freshFrameMinus14ReadReached: true,
                freshFrameMinus14Captured: true,
                freshFrameMinus14: freshFrameMinus14,
                zeroTestReached: true,
                zeroTestResult: false,
                zeroBranchReached: true,
                zeroBranchTaken: false,
                firstIndexAddress: firstIndexAddress,
                firstIndexReadReached: true,
                firstIndexCaptured: true,
                freshFirstIndex: firstIndex,
                firstIndexedTableAddress: firstTableAddress,
                firstIndexedTableReadReached: true,
                firstIndexedTableValueCaptured: true,
                firstIndexedTableValue: firstTableValue,
                firstCompareReached: true,
                firstCompareResult: compareResult,
                firstCompareFlags: compareFlags,
                firstCompareEqual: true,
                freshFrameMinusCAddress: frameMinusCAddress,
                freshFrameMinusCReadReached: true,
                freshFrameMinusCCaptured: true,
                freshFrameMinusC: freshFrameMinusC,
                unequalBranchReached: true,
                unequalBranchTaken: false,
                secondIndexAddress: secondIndexAddress,
                secondIndexReadReached: true,
                secondIndexCaptured: true,
                freshSecondIndex: secondIndex,
                tableWriteAddress: tableWriteAddress,
                tableWriteReached: true,
                tableWriteValue: entryEax,
                storedValueTestReached: true,
                storedValueIsZero: true,
                nonzeroBranchReached: true,
                nonzeroBranchTaken: false,
                freshFrameMinus8Address: DeriveFrameMinus8Address(entryEbp),
                freshFrameMinus8ReadReached: true,
                freshFrameMinus8Captured: true,
                freshFrameMinus8: freshFrameMinus8,
                thirdIndexAddress: thirdIndexAddress,
                thirdIndexReadReached: true,
                thirdIndexCaptured: true,
                freshThirdIndex: thirdIndex,
                shiftCount: shiftCount,
                shiftedValue: shiftedValue,
                notMask: notMask,
                globalAddress: GlobalMaskVirtualAddress,
                globalReadReached: true,
                stopAt: GlobalAndRawFileOffset);
        }

        return BuildPlan(
            capture,
            CapacityFinalCalleeIndexedRootMaskOutcome.GlobalValueUpdated,
            freshFrameMinus14Address: frameMinus14Address,
            freshFrameMinus14ReadReached: true,
            freshFrameMinus14Captured: true,
            freshFrameMinus14: freshFrameMinus14,
            zeroTestReached: true,
            zeroTestResult: false,
            zeroBranchReached: true,
            zeroBranchTaken: false,
            firstIndexAddress: firstIndexAddress,
            firstIndexReadReached: true,
            firstIndexCaptured: true,
            freshFirstIndex: firstIndex,
            firstIndexedTableAddress: firstTableAddress,
            firstIndexedTableReadReached: true,
            firstIndexedTableValueCaptured: true,
            firstIndexedTableValue: firstTableValue,
            firstCompareReached: true,
            firstCompareResult: compareResult,
            firstCompareFlags: compareFlags,
            firstCompareEqual: true,
            freshFrameMinusCAddress: frameMinusCAddress,
            freshFrameMinusCReadReached: true,
            freshFrameMinusCCaptured: true,
            freshFrameMinusC: freshFrameMinusC,
            unequalBranchReached: true,
            unequalBranchTaken: false,
            secondIndexAddress: secondIndexAddress,
            secondIndexReadReached: true,
            secondIndexCaptured: true,
            freshSecondIndex: secondIndex,
            tableWriteAddress: tableWriteAddress,
            tableWriteReached: true,
            tableWriteValue: entryEax,
            storedValueTestReached: true,
            storedValueIsZero: true,
            nonzeroBranchReached: true,
            nonzeroBranchTaken: false,
            freshFrameMinus8Address: DeriveFrameMinus8Address(entryEbp),
            freshFrameMinus8ReadReached: true,
            freshFrameMinus8Captured: true,
            freshFrameMinus8: freshFrameMinus8,
            thirdIndexAddress: thirdIndexAddress,
            thirdIndexReadReached: true,
            thirdIndexCaptured: true,
            freshThirdIndex: thirdIndex,
            shiftCount: shiftCount,
            shiftedValue: shiftedValue,
            notMask: notMask,
            globalAddress: GlobalMaskVirtualAddress,
            globalReadReached: true,
            globalValueCaptured: true,
            globalValueBefore: freshGlobalValue,
            globalWriteReached: true,
            globalValueAfter: freshGlobalValue & notMask,
            stopAt: MaskContinuationRawFileOffset);
    }

    private static CapacityFinalCalleeIndexedRootMaskFlags SubtractFlags(
        uint left,
        uint right,
        uint result)
    {
        uint lowByte = result & 0xFF;
        lowByte ^= lowByte >> 4;
        lowByte &= 0xF;
        bool evenParity = ((0x6996 >> (int)lowByte) & 1) == 0;
        return new(
            Carry: left < right,
            Parity: evenParity,
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static bool HasEntryRegisterCapture(
        CapacityFinalCalleeIndexedRootMaskCapture capture) =>
        capture.EntryEBP.HasValue
        || capture.EntryESI.HasValue
        || capture.EntryEAX.HasValue;

    private static CapacityFinalCalleeIndexedRootMaskPlan BuildPlan(
        CapacityFinalCalleeIndexedRootMaskCapture capture,
        CapacityFinalCalleeIndexedRootMaskOutcome outcome,
        uint? freshFrameMinus14Address = null,
        bool freshFrameMinus14ReadReached = false,
        bool freshFrameMinus14Captured = false,
        uint? freshFrameMinus14 = null,
        bool zeroTestReached = false,
        bool? zeroTestResult = null,
        bool zeroBranchReached = false,
        bool? zeroBranchTaken = null,
        uint? zeroBranchDestinationRawFileOffset = null,
        uint? firstIndexAddress = null,
        bool firstIndexReadReached = false,
        bool firstIndexCaptured = false,
        uint? freshFirstIndex = null,
        uint? firstIndexedTableAddress = null,
        bool firstIndexedTableReadReached = false,
        bool firstIndexedTableValueCaptured = false,
        uint? firstIndexedTableValue = null,
        bool firstCompareReached = false,
        uint? firstCompareResult = null,
        CapacityFinalCalleeIndexedRootMaskFlags? firstCompareFlags = null,
        bool? firstCompareEqual = null,
        uint? freshFrameMinusCAddress = null,
        bool freshFrameMinusCReadReached = false,
        bool freshFrameMinusCCaptured = false,
        uint? freshFrameMinusC = null,
        bool unequalBranchReached = false,
        bool? unequalBranchTaken = null,
        uint? unequalBranchDestinationRawFileOffset = null,
        uint? secondIndexAddress = null,
        bool secondIndexReadReached = false,
        bool secondIndexCaptured = false,
        uint? freshSecondIndex = null,
        uint? tableWriteAddress = null,
        bool tableWriteReached = false,
        uint? tableWriteValue = null,
        bool storedValueTestReached = false,
        bool? storedValueIsZero = null,
        bool nonzeroBranchReached = false,
        bool? nonzeroBranchTaken = null,
        uint? nonzeroBranchDestinationRawFileOffset = null,
        uint? freshFrameMinus8Address = null,
        bool freshFrameMinus8ReadReached = false,
        bool freshFrameMinus8Captured = false,
        uint? freshFrameMinus8 = null,
        uint? thirdIndexAddress = null,
        bool thirdIndexReadReached = false,
        bool thirdIndexCaptured = false,
        uint? freshThirdIndex = null,
        uint? shiftCount = null,
        uint? shiftedValue = null,
        uint? notMask = null,
        uint? globalAddress = null,
        bool globalReadReached = false,
        bool globalValueCaptured = false,
        uint? globalValueBefore = null,
        bool globalWriteReached = false,
        uint? globalValueAfter = null,
        bool? unreachedEntryRegisterCaptureProvided = null,
        uint? stopAt = null) =>
        new(
            capture,
            outcome,
            freshFrameMinus14ReadReached ? freshFrameMinus14Address : null,
            freshFrameMinus14ReadReached,
            freshFrameMinus14Captured,
            freshFrameMinus14Captured ? freshFrameMinus14 : null,
            zeroTestReached,
            zeroTestResult,
            zeroBranchReached,
            zeroBranchTaken,
            zeroBranchDestinationRawFileOffset,
            firstIndexReadReached ? firstIndexAddress : null,
            firstIndexReadReached,
            firstIndexCaptured,
            firstIndexCaptured ? freshFirstIndex : null,
            firstIndexedTableReadReached ? firstIndexedTableAddress : null,
            firstIndexedTableReadReached,
            firstIndexedTableValueCaptured,
            firstIndexedTableValueCaptured ? firstIndexedTableValue : null,
            firstCompareReached,
            firstCompareResult,
            firstCompareFlags,
            firstCompareEqual,
            freshFrameMinusCReadReached ? freshFrameMinusCAddress : null,
            freshFrameMinusCReadReached,
            freshFrameMinusCCaptured,
            freshFrameMinusCCaptured ? freshFrameMinusC : null,
            unequalBranchReached,
            unequalBranchTaken,
            unequalBranchDestinationRawFileOffset,
            secondIndexReadReached ? secondIndexAddress : null,
            secondIndexReadReached,
            secondIndexCaptured,
            secondIndexCaptured ? freshSecondIndex : null,
            tableWriteReached ? tableWriteAddress : null,
            tableWriteReached,
            tableWriteReached ? tableWriteValue : null,
            storedValueTestReached,
            storedValueIsZero,
            nonzeroBranchReached,
            nonzeroBranchTaken,
            nonzeroBranchDestinationRawFileOffset,
            freshFrameMinus8ReadReached ? freshFrameMinus8Address : null,
            freshFrameMinus8ReadReached,
            freshFrameMinus8Captured,
            freshFrameMinus8Captured ? freshFrameMinus8 : null,
            thirdIndexReadReached ? thirdIndexAddress : null,
            thirdIndexReadReached,
            thirdIndexCaptured,
            thirdIndexCaptured ? freshThirdIndex : null,
            shiftCount,
            shiftedValue,
            notMask,
            globalReadReached ? globalAddress : null,
            globalReadReached,
            globalValueCaptured,
            globalValueCaptured ? globalValueBefore : null,
            globalWriteReached,
            globalWriteReached ? globalValueAfter : null,
            unreachedEntryRegisterCaptureProvided
                ?? (!capture.CaptureValidatedEntryRegisters && HasEntryRegisterCapture(capture)),
            !freshFrameMinus14ReadReached && capture.FreshFrameMinus14.HasValue,
            !firstIndexReadReached && capture.FreshFirstIndex.HasValue,
            !firstIndexedTableReadReached && capture.FirstIndexedTableValue.HasValue,
            !freshFrameMinusCReadReached && capture.FreshFrameMinusC.HasValue,
            !secondIndexReadReached && capture.FreshSecondIndex.HasValue,
            !freshFrameMinus8ReadReached && capture.FreshFrameMinus8.HasValue,
            !thirdIndexReadReached && capture.FreshThirdIndex.HasValue,
            !globalReadReached && capture.FreshGlobalValue.HasValue,
            stopAt ?? EntryRawFileOffset);

    private static CapacityFinalCalleeIndexedRootMaskInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null,
        uint? branchTargetRawFileOffset = null,
        uint? branchTargetVirtualAddress = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            peVirtualAddressOperand,
            branchTargetRawFileOffset,
            branchTargetVirtualAddress);
}
