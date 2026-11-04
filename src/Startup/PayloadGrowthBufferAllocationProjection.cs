using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadGrowthBufferAllocationCapture(
    uint ReceiverPointer,
    uint FramePointer,
    uint RetainedInitialCount,
    uint RetainedFinalDoublingResult,
    uint? ReceiverDword0C,
    bool? AllocatorCallReturnedNormally,
    uint? AllocatorEax,
    bool? IndirectCallReturnedNormally);

public enum PayloadGrowthBufferAllocationOutcome
{
    InvalidEntryCondition,
    MissingReceiverDword0CCapture,
    UnreachedCaptureProvided,
    MissingAllocatorCallReturnCapture,
    AllocatorCallDidNotReturnNormally,
    MissingAllocatorEaxCapture,
    MissingIndirectCallReturnCapture,
    IndirectCallDidNotReturnNormally,
    IndirectCallReturnedNormally,
    AllocatorReturnedNonzero
}

public readonly record struct PayloadGrowthBufferAllocationPlan(
    PayloadGrowthBufferAllocationCapture Capture,
    PayloadGrowthBufferAllocationOutcome Outcome,
    bool EntryConditionValid,
    uint EntryGrowthDelta,
    uint? ReceiverDword0CAddress,
    bool ReceiverDword0CRead,
    uint? EaxBeforeZeroTest,
    bool? ZeroTestBranchTaken,
    bool? OversizeBranchTaken,
    uint? FrameMinus8Address,
    bool FrameMinus8Written,
    uint? FrameMinus8Value,
    uint? FrameMinus4Address,
    bool FrameMinus4ZeroWritten,
    bool FrameMinus4EaxWritePerformed,
    uint? FrameMinus4LastWrittenValue,
    bool AllocatorCallReached,
    uint? AllocatorCallArgument,
    bool? AllocatorCallReturnedNormally,
    uint? AllocatorStackCleanupBytes,
    uint? AllocatorEax,
    bool IndirectCallReached,
    uint? IndirectCallCellVirtualAddress,
    uint? RouteRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct PayloadGrowthBufferAllocationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null,
    uint? CallTargetRawFileOffset = null,
    uint? CallTargetVirtualAddress = null,
    uint? NormalReturnContinuationRawFileOffset = null,
    uint? AbsoluteVirtualAddress = null);

/// <summary>
/// Projects the bounded buffer-allocation prefix at raw 0xBE68E1..0xBE691A.
/// Entry is accepted only when the preceding unsigned JNB guards hold. Opaque
/// calls are modeled only through separately captured normal-return observations.
/// </summary>
public static class PayloadGrowthBufferAllocationProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint GateStartRawFileOffset = 0x00BE68E1;
    public const uint GateStartVirtualAddress = 0x00FE74E1;
    public const uint GateEndRawFileOffsetExclusive = 0x00BE691A;
    public const uint GateEndVirtualAddressExclusive = 0x00FE751A;

    public const uint GateStartVirtualAddressFromPeSection =
        0x00400000 + 0x00001000 + (GateStartRawFileOffset - 0x00000400);

    public const uint EntryGrowthDeltaMinimum = 1;
    public const uint EntryFinalDoublingResultMinimum = 8;
    public const uint ReceiverDword0COffset = 0x0C;
    public const uint FrameMinus8Offset = 0x08;
    public const uint FrameMinus4Offset = 0x04;
    public const uint AllocationSizeLimit = 0x3FFFFFFF;
    public const uint AllocationElementSize = 4;

    public const uint ReceiverDword0CReadRawFileOffset = 0x00BE68E1;
    public const uint RetainedCountDifferenceRawFileOffset = 0x00BE68E4;
    public const uint ReceiverDword0CShiftRawFileOffset = 0x00BE68E6;
    public const uint FrameMinus8WriteRawFileOffset = 0x00BE68E9;
    public const uint FrameMinus4ZeroWriteRawFileOffset = 0x00BE68EC;
    public const uint EaxFormationRawFileOffset = 0x00BE68F3;
    public const uint ZeroTestRawFileOffset = 0x00BE68F6;
    public const uint ZeroBranchRawFileOffset = 0x00BE68F8;
    public const uint ZeroBranchTargetRawFileOffset = 0x00BE691A;
    public const uint AllocationSizeCompareRawFileOffset = 0x00BE68FA;
    public const uint OversizeBranchRawFileOffset = 0x00BE68FF;
    public const uint OversizeBranchTargetRawFileOffset = 0x00BE6914;
    public const uint AllocationSizeShiftRawFileOffset = 0x00BE6901;
    public const uint AllocatorArgumentPushRawFileOffset = 0x00BE6904;
    public const uint AllocatorCallRawFileOffset = 0x00BE6905;
    public const uint AllocatorCallNormalReturnRawFileOffset = 0x00BE690A;
    public const uint AllocatorTargetRawFileOffset = 0x012B7FB0;
    public const uint AllocatorStackCleanupRawFileOffset = 0x00BE690A;
    public const uint AllocatorStackCleanupBytes = 4;
    public const uint FrameMinus4EaxWriteRawFileOffset = 0x00BE690D;
    public const uint AllocatorResultTestRawFileOffset = 0x00BE6910;
    public const uint NonzeroAllocatorResultBranchRawFileOffset = 0x00BE6912;
    public const uint NonzeroAllocatorResultBranchTargetRawFileOffset = 0x00BE691A;
    public const uint IndirectCallRawFileOffset = 0x00BE6914;
    public const uint IndirectCallNormalReturnRawFileOffset = 0x00BE691A;
    public const uint IndirectCallCellVirtualAddress = 0x01AC4424;

    public const uint AllocatorTargetVirtualAddress =
        AllocatorTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const string CapturedGateBytes =
        "8B 7B 0C 2B F2 C1 EF 03 89 7D F8 C7 45 FC 00 00 00 00 "
        + "8D 04 32 85 C0 74 20 3D FF FF FF 3F 77 13 C1 E0 02 50 "
        + "E8 A6 16 6D 00 83 C4 04 89 45 FC 85 C0 75 06 "
        + "FF 15 24 44 AC 01";

    public static uint GateStartVirtualAddressFromRawOffset =>
        RawFileOffsetToVirtualAddress(GateStartRawFileOffset);

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAddress(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    public static uint FrameAddress(uint framePointer, uint offset) =>
        unchecked(framePointer - offset);

    public static ReadOnlyCollection<PayloadGrowthBufferAllocationInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Capture receiver DWORD +0x0C into EDI",
                ReceiverDword0CReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword0CReadRawFileOffset),
                "8B 7B 0C"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Subtract retained initial EDX from final ESI with DWORD wraparound",
                RetainedCountDifferenceRawFileOffset,
                RawFileOffsetToVirtualAddress(RetainedCountDifferenceRawFileOffset),
                "2B F2"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Shift captured receiver DWORD right by three",
                ReceiverDword0CShiftRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDword0CShiftRawFileOffset),
                "C1 EF 03"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Write the shifted value to frame-8",
                FrameMinus8WriteRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameMinus8WriteRawFileOffset),
                "89 7D F8"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Initialize frame-4 to zero",
                FrameMinus4ZeroWriteRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameMinus4ZeroWriteRawFileOffset),
                "C7 45 FC 00 00 00 00"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Form EAX as EDX plus the wrapped ESI difference, reproducing entry ESI",
                EaxFormationRawFileOffset,
                RawFileOffsetToVirtualAddress(EaxFormationRawFileOffset),
                "8D 04 32"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Test EAX; the zero branch is unreachable after a valid entry guard",
                ZeroTestRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroTestRawFileOffset),
                "85 C0"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "JZ to raw 0xBE691A; valid entry requires the tested EAX to be at least eight",
                ZeroBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroBranchRawFileOffset),
                "74 20",
                ZeroBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroBranchTargetRawFileOffset)),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Compare EAX unsigned against 0x3FFFFFFF",
                AllocationSizeCompareRawFileOffset,
                RawFileOffsetToVirtualAddress(AllocationSizeCompareRawFileOffset),
                "3D FF FF FF 3F"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Unsigned JA sends oversized counts directly to the indirect call",
                OversizeBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(OversizeBranchRawFileOffset),
                "77 13",
                OversizeBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(OversizeBranchTargetRawFileOffset)),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Scale the allocator size by four with DWORD wraparound",
                AllocationSizeShiftRawFileOffset,
                RawFileOffsetToVirtualAddress(AllocationSizeShiftRawFileOffset),
                "C1 E0 02"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Push the scaled allocator argument",
                AllocatorArgumentPushRawFileOffset,
                RawFileOffsetToVirtualAddress(AllocatorArgumentPushRawFileOffset),
                "50"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Call allocator raw 0x12B7FB0; normal return resumes at raw 0xBE690A",
                AllocatorCallRawFileOffset,
                RawFileOffsetToVirtualAddress(AllocatorCallRawFileOffset),
                "E8 A6 16 6D 00",
                CallTargetRawFileOffset: AllocatorTargetRawFileOffset,
                CallTargetVirtualAddress: AllocatorTargetVirtualAddress,
                NormalReturnContinuationRawFileOffset:
                    AllocatorCallNormalReturnRawFileOffset),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Clean up the allocator argument after normal return",
                AllocatorStackCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(AllocatorStackCleanupRawFileOffset),
                "83 C4 04"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "After cleanup, store captured allocator EAX into frame-4",
                FrameMinus4EaxWriteRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameMinus4EaxWriteRawFileOffset),
                "89 45 FC"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Test captured allocator EAX",
                AllocatorResultTestRawFileOffset,
                RawFileOffsetToVirtualAddress(AllocatorResultTestRawFileOffset),
                "85 C0"),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "A nonzero allocator result skips the indirect call",
                NonzeroAllocatorResultBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(NonzeroAllocatorResultBranchRawFileOffset),
                "75 06",
                NonzeroAllocatorResultBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(
                    NonzeroAllocatorResultBranchTargetRawFileOffset)),
            new PayloadGrowthBufferAllocationInstructionEvidence(
                "Call through absolute cell VA 0x01AC4424; normal return resumes at raw 0xBE691A",
                IndirectCallRawFileOffset,
                RawFileOffsetToVirtualAddress(IndirectCallRawFileOffset),
                "FF 15 24 44 AC 01",
                NormalReturnContinuationRawFileOffset:
                    IndirectCallNormalReturnRawFileOffset,
                AbsoluteVirtualAddress: IndirectCallCellVirtualAddress)
        });

    public static PayloadGrowthBufferAllocationPlan Project(
        PayloadGrowthBufferAllocationCapture capture)
    {
        uint entryGrowthDelta = unchecked(
            capture.RetainedFinalDoublingResult - capture.RetainedInitialCount);
        bool entryConditionValid =
            entryGrowthDelta >= EntryGrowthDeltaMinimum
            && capture.RetainedFinalDoublingResult
                >= EntryFinalDoublingResultMinimum;

        if (!entryConditionValid)
        {
            return Stop(
                capture,
                HasAnyPostEntryCapture(capture)
                    ? PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided
                    : PayloadGrowthBufferAllocationOutcome.InvalidEntryCondition,
                entryConditionValid: false,
                entryGrowthDelta: entryGrowthDelta);
        }

        uint eaxBeforeZeroTest = unchecked(
            capture.RetainedInitialCount + entryGrowthDelta);
        uint receiverDword0CAddress = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword0COffset);
        if (capture.ReceiverDword0C is not uint receiverDword0C)
        {
            return Stop(
                capture,
                HasCaptureAfterReceiverRead(capture)
                    ? PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided
                    : PayloadGrowthBufferAllocationOutcome.MissingReceiverDword0CCapture,
                entryConditionValid: true,
                entryGrowthDelta: entryGrowthDelta,
                receiverDword0CAddress: receiverDword0CAddress,
                stopAt: ReceiverDword0CReadRawFileOffset);
        }

        uint frameMinus8Address = FrameAddress(
            capture.FramePointer,
            FrameMinus8Offset);
        uint frameMinus4Address = FrameAddress(
            capture.FramePointer,
            FrameMinus4Offset);
        uint frameMinus8Value = receiverDword0C >> 3;
        bool oversizeBranchTaken = eaxBeforeZeroTest > AllocationSizeLimit;

        if (oversizeBranchTaken)
        {
            if (capture.AllocatorCallReturnedNormally is not null
                || capture.AllocatorEax is not null)
            {
                return Stop(
                    capture,
                    PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided,
                    entryConditionValid: true,
                    entryGrowthDelta: entryGrowthDelta,
                    receiverDword0CAddress: receiverDword0CAddress,
                    receiverDword0CRead: true,
                    eaxBeforeZeroTest: eaxBeforeZeroTest,
                    zeroTestBranchTaken: false,
                    oversizeBranchTaken: true,
                    frameMinus8Address: frameMinus8Address,
                    frameMinus8Written: true,
                    frameMinus8Value: frameMinus8Value,
                    frameMinus4Address: frameMinus4Address,
                    frameMinus4ZeroWritten: true,
                    frameMinus4LastWrittenValue: 0);
            }

            return ProjectIndirectCall(
                capture,
                entryGrowthDelta,
                receiverDword0CAddress,
                eaxBeforeZeroTest,
                oversizeBranchTaken: true,
                frameMinus8Address,
                frameMinus8Value,
                frameMinus4Address);
        }

        if (capture.AllocatorCallReturnedNormally
            is not bool allocatorReturnedNormally)
        {
            return Stop(
                capture,
                capture.AllocatorEax is not null
                    || capture.IndirectCallReturnedNormally is not null
                    ? PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided
                    : PayloadGrowthBufferAllocationOutcome
                        .MissingAllocatorCallReturnCapture,
                entryConditionValid: true,
                entryGrowthDelta: entryGrowthDelta,
                receiverDword0CAddress: receiverDword0CAddress,
                receiverDword0CRead: true,
                eaxBeforeZeroTest: eaxBeforeZeroTest,
                zeroTestBranchTaken: false,
                oversizeBranchTaken: false,
                frameMinus8Address: frameMinus8Address,
                frameMinus8Written: true,
                frameMinus8Value: frameMinus8Value,
                frameMinus4Address: frameMinus4Address,
                frameMinus4ZeroWritten: true,
                frameMinus4LastWrittenValue: 0,
                allocatorCallReached: true,
                allocatorCallArgument: unchecked(
                    eaxBeforeZeroTest * AllocationElementSize),
                stopAt: AllocatorCallNormalReturnRawFileOffset);
        }

        if (!allocatorReturnedNormally)
        {
            return Stop(
                capture,
                capture.AllocatorEax is not null
                    || capture.IndirectCallReturnedNormally is not null
                    ? PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided
                    : PayloadGrowthBufferAllocationOutcome
                        .AllocatorCallDidNotReturnNormally,
                entryConditionValid: true,
                entryGrowthDelta: entryGrowthDelta,
                receiverDword0CAddress: receiverDword0CAddress,
                receiverDword0CRead: true,
                eaxBeforeZeroTest: eaxBeforeZeroTest,
                zeroTestBranchTaken: false,
                oversizeBranchTaken: false,
                frameMinus8Address: frameMinus8Address,
                frameMinus8Written: true,
                frameMinus8Value: frameMinus8Value,
                frameMinus4Address: frameMinus4Address,
                frameMinus4ZeroWritten: true,
                frameMinus4LastWrittenValue: 0,
                allocatorCallReached: true,
                allocatorCallArgument: unchecked(
                    eaxBeforeZeroTest * AllocationElementSize),
                allocatorCallReturnedNormally: false,
                stopAt: AllocatorCallNormalReturnRawFileOffset);
        }

        if (capture.AllocatorEax is not uint allocatorEax)
        {
            return Stop(
                capture,
                capture.IndirectCallReturnedNormally is not null
                    ? PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided
                    : PayloadGrowthBufferAllocationOutcome.MissingAllocatorEaxCapture,
                entryConditionValid: true,
                entryGrowthDelta: entryGrowthDelta,
                receiverDword0CAddress: receiverDword0CAddress,
                receiverDword0CRead: true,
                eaxBeforeZeroTest: eaxBeforeZeroTest,
                zeroTestBranchTaken: false,
                oversizeBranchTaken: false,
                frameMinus8Address: frameMinus8Address,
                frameMinus8Written: true,
                frameMinus8Value: frameMinus8Value,
                frameMinus4Address: frameMinus4Address,
                frameMinus4ZeroWritten: true,
                frameMinus4LastWrittenValue: 0,
                allocatorCallReached: true,
                allocatorCallArgument: unchecked(
                    eaxBeforeZeroTest * AllocationElementSize),
                allocatorCallReturnedNormally: true,
                allocatorStackCleanupBytes: AllocatorStackCleanupBytes,
                stopAt: FrameMinus4EaxWriteRawFileOffset);
        }

        if (allocatorEax != 0)
        {
            return Stop(
                capture,
                capture.IndirectCallReturnedNormally is not null
                    ? PayloadGrowthBufferAllocationOutcome.UnreachedCaptureProvided
                    : PayloadGrowthBufferAllocationOutcome.AllocatorReturnedNonzero,
                entryConditionValid: true,
                entryGrowthDelta: entryGrowthDelta,
                receiverDword0CAddress: receiverDword0CAddress,
                receiverDword0CRead: true,
                eaxBeforeZeroTest: eaxBeforeZeroTest,
                zeroTestBranchTaken: false,
                oversizeBranchTaken: false,
                frameMinus8Address: frameMinus8Address,
                frameMinus8Written: true,
                frameMinus8Value: frameMinus8Value,
                frameMinus4Address: frameMinus4Address,
                frameMinus4ZeroWritten: true,
                frameMinus4EaxWritePerformed: true,
                frameMinus4LastWrittenValue: allocatorEax,
                allocatorCallReached: true,
                allocatorCallArgument: unchecked(
                    eaxBeforeZeroTest * AllocationElementSize),
                allocatorCallReturnedNormally: true,
                allocatorStackCleanupBytes: AllocatorStackCleanupBytes,
                allocatorEax: allocatorEax,
                route: GateEndRawFileOffsetExclusive,
                stopAt: GateEndRawFileOffsetExclusive);
        }

        return ProjectIndirectCall(
            capture,
            entryGrowthDelta,
            receiverDword0CAddress,
            eaxBeforeZeroTest,
            oversizeBranchTaken: false,
            frameMinus8Address,
            frameMinus8Value,
            frameMinus4Address,
            allocatorCallReached: true,
            allocatorCallArgument: unchecked(
                eaxBeforeZeroTest * AllocationElementSize),
            allocatorCallReturnedNormally: true,
            allocatorStackCleanupBytes: AllocatorStackCleanupBytes,
            allocatorEax: allocatorEax,
            frameMinus4EaxWritePerformed: true,
            frameMinus4LastWrittenValue: allocatorEax);
    }

    private static PayloadGrowthBufferAllocationPlan ProjectIndirectCall(
        PayloadGrowthBufferAllocationCapture capture,
        uint entryGrowthDelta,
        uint receiverDword0CAddress,
        uint eaxBeforeZeroTest,
        bool oversizeBranchTaken,
        uint frameMinus8Address,
        uint frameMinus8Value,
        uint frameMinus4Address,
        bool allocatorCallReached = false,
        uint? allocatorCallArgument = null,
        bool? allocatorCallReturnedNormally = null,
        uint? allocatorStackCleanupBytes = null,
        uint? allocatorEax = null,
        bool frameMinus4EaxWritePerformed = false,
        uint? frameMinus4LastWrittenValue = 0)
    {
        if (capture.IndirectCallReturnedNormally
            is not bool indirectCallReturnedNormally)
        {
            return Stop(
                capture,
                PayloadGrowthBufferAllocationOutcome.MissingIndirectCallReturnCapture,
                entryConditionValid: true,
                entryGrowthDelta: entryGrowthDelta,
                receiverDword0CAddress: receiverDword0CAddress,
                receiverDword0CRead: true,
                eaxBeforeZeroTest: eaxBeforeZeroTest,
                zeroTestBranchTaken: false,
                oversizeBranchTaken: oversizeBranchTaken,
                frameMinus8Address: frameMinus8Address,
                frameMinus8Written: true,
                frameMinus8Value: frameMinus8Value,
                frameMinus4Address: frameMinus4Address,
                frameMinus4ZeroWritten: true,
                frameMinus4EaxWritePerformed: frameMinus4EaxWritePerformed,
                frameMinus4LastWrittenValue: frameMinus4LastWrittenValue,
                allocatorCallReached: allocatorCallReached,
                allocatorCallArgument: allocatorCallArgument,
                allocatorCallReturnedNormally: allocatorCallReturnedNormally,
                allocatorStackCleanupBytes: allocatorStackCleanupBytes,
                allocatorEax: allocatorEax,
                indirectCallReached: true,
                indirectCallCellVirtualAddress: IndirectCallCellVirtualAddress,
                route: IndirectCallRawFileOffset,
                stopAt: IndirectCallNormalReturnRawFileOffset);
        }

        return Stop(
            capture,
            indirectCallReturnedNormally
                ? PayloadGrowthBufferAllocationOutcome.IndirectCallReturnedNormally
                : PayloadGrowthBufferAllocationOutcome.IndirectCallDidNotReturnNormally,
            entryConditionValid: true,
            entryGrowthDelta: entryGrowthDelta,
            receiverDword0CAddress: receiverDword0CAddress,
            receiverDword0CRead: true,
            eaxBeforeZeroTest: eaxBeforeZeroTest,
            zeroTestBranchTaken: false,
            oversizeBranchTaken: oversizeBranchTaken,
            frameMinus8Address: frameMinus8Address,
            frameMinus8Written: true,
            frameMinus8Value: frameMinus8Value,
            frameMinus4Address: frameMinus4Address,
            frameMinus4ZeroWritten: true,
            frameMinus4EaxWritePerformed: frameMinus4EaxWritePerformed,
            frameMinus4LastWrittenValue: frameMinus4LastWrittenValue,
            allocatorCallReached: allocatorCallReached,
            allocatorCallArgument: allocatorCallArgument,
            allocatorCallReturnedNormally: allocatorCallReturnedNormally,
            allocatorStackCleanupBytes: allocatorStackCleanupBytes,
            allocatorEax: allocatorEax,
            indirectCallReached: true,
            indirectCallCellVirtualAddress: IndirectCallCellVirtualAddress,
            route: indirectCallReturnedNormally
                ? GateEndRawFileOffsetExclusive
                : null,
            stopAt: IndirectCallNormalReturnRawFileOffset);
    }

    private static bool HasAnyPostEntryCapture(
        PayloadGrowthBufferAllocationCapture capture) =>
        capture.ReceiverDword0C is not null
        || capture.AllocatorCallReturnedNormally is not null
        || capture.AllocatorEax is not null
        || capture.IndirectCallReturnedNormally is not null;

    private static bool HasCaptureAfterReceiverRead(
        PayloadGrowthBufferAllocationCapture capture) =>
        capture.AllocatorCallReturnedNormally is not null
        || capture.AllocatorEax is not null
        || capture.IndirectCallReturnedNormally is not null;

    private static PayloadGrowthBufferAllocationPlan Stop(
        PayloadGrowthBufferAllocationCapture capture,
        PayloadGrowthBufferAllocationOutcome outcome,
        bool entryConditionValid = false,
        uint entryGrowthDelta = 0,
        uint? receiverDword0CAddress = null,
        bool receiverDword0CRead = false,
        uint? eaxBeforeZeroTest = null,
        bool? zeroTestBranchTaken = null,
        bool? oversizeBranchTaken = null,
        uint? frameMinus8Address = null,
        bool frameMinus8Written = false,
        uint? frameMinus8Value = null,
        uint? frameMinus4Address = null,
        bool frameMinus4ZeroWritten = false,
        bool frameMinus4EaxWritePerformed = false,
        uint? frameMinus4LastWrittenValue = null,
        bool allocatorCallReached = false,
        uint? allocatorCallArgument = null,
        bool? allocatorCallReturnedNormally = null,
        uint? allocatorStackCleanupBytes = null,
        uint? allocatorEax = null,
        bool indirectCallReached = false,
        uint? indirectCallCellVirtualAddress = null,
        uint? route = null,
        uint stopAt = GateStartRawFileOffset) =>
        new(
            capture,
            outcome,
            entryConditionValid,
            entryGrowthDelta,
            receiverDword0CAddress,
            receiverDword0CRead,
            eaxBeforeZeroTest,
            zeroTestBranchTaken,
            oversizeBranchTaken,
            frameMinus8Address,
            frameMinus8Written,
            frameMinus8Value,
            frameMinus4Address,
            frameMinus4ZeroWritten,
            frameMinus4EaxWritePerformed,
            frameMinus4LastWrittenValue,
            allocatorCallReached,
            allocatorCallArgument,
            allocatorCallReturnedNormally,
            allocatorStackCleanupBytes,
            allocatorEax,
            indirectCallReached,
            indirectCallCellVirtualAddress,
            route,
            stopAt);
}
