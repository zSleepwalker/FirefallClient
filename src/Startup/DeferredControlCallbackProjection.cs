using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct DeferredControlCallbackCapture(
    uint PayloadEdi,
    uint? VtablePointerAtPayload8,
    uint? StackArgumentValueAtPayload4,
    uint? CallResultEax,
    byte? ReturnedPointerByte);

public enum DeferredControlCallbackOutcome
{
    MissingVtablePointer,
    UnreachedCaptureProvided,
    MissingStackArgument,
    MissingCallResult,
    MissingReturnedPointerByte,
    MaskBitClearBranchesToZeroTarget,
    MaskBitSetFallsThrough
}

public readonly record struct DeferredControlCallbackPlan(
    DeferredControlCallbackCapture Capture,
    DeferredControlCallbackOutcome Outcome,
    bool IndirectCallReached,
    uint VtablePointerAddress,
    uint EcxArgumentAddress,
    uint StackArgumentAddress,
    uint? IndirectSlotAddress,
    uint? ReturnedPointerByteAddress,
    byte? TestedBits,
    uint? RouteRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct DeferredControlCallbackInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null);

/// <summary>
/// Projects only the observed deferred-control callback slice at raw BD37D8
/// through its first test-bit branch. Captured values remain independent; this
/// does not dereference memory or identify the indirect-call target.
/// </summary>
public static class DeferredControlCallbackProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x00BD37D8;
    public const uint VtablePointerLoadRawFileOffset = 0x00BD37D8;
    public const uint EcxArgumentAddressRawFileOffset = 0x00BD37DB;
    public const uint StackArgumentLoadRawFileOffset = 0x00BD37DE;
    public const uint IndirectCallRawFileOffset = 0x00BD37E1;
    public const uint CallReturnRawFileOffset = 0x00BD37E4;
    public const uint ReturnedPointerTestRawFileOffset = 0x00BD37E6;
    public const uint BranchRawFileOffset = 0x00BD37E9;
    public const uint EndRawFileOffsetExclusive = 0x00BD37EB;
    public const uint ZeroBitBranchTargetRawFileOffset = 0x00BD3805;

    public const uint PayloadStackArgumentOffset = 4;
    public const uint PayloadVtablePointerOffset = 8;
    public const uint IndirectSlotOffset = 4;
    public const byte ReturnedPointerTestMask = 8;
    public const byte ZeroBitBranchDisplacement = 0x1A;

    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint CallReturnVirtualAddress =
        CallReturnRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint ZeroBitBranchTargetVirtualAddress =
        ZeroBitBranchTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public static ReadOnlyCollection<DeferredControlCallbackInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new DeferredControlCallbackInstructionEvidence(
                "Load the independent vtable pointer from [EDI+8]",
                VtablePointerLoadRawFileOffset,
                "8B 47 08"),
            new DeferredControlCallbackInstructionEvidence(
                "Set ECX to the payload address EDI+8, not the loaded vtable pointer",
                EcxArgumentAddressRawFileOffset,
                "8D 4F 08"),
            new DeferredControlCallbackInstructionEvidence(
                "Push the stack argument value loaded from [EDI+4]",
                StackArgumentLoadRawFileOffset,
                "FF 77 04"),
            new DeferredControlCallbackInstructionEvidence(
                "Call through the opaque indirect slot at [vtablePointer+4]",
                IndirectCallRawFileOffset,
                "FF 50 04"),
            new DeferredControlCallbackInstructionEvidence(
                "On normal return, copy EAX into EBX",
                CallReturnRawFileOffset,
                "8B D8"),
            new DeferredControlCallbackInstructionEvidence(
                "Test bit 3 of the byte at the returned EAX pointer",
                ReturnedPointerTestRawFileOffset,
                "F6 03 08"),
            new DeferredControlCallbackInstructionEvidence(
                "A clear bit branches outside this exact slice; a set bit falls through to its end",
                BranchRawFileOffset,
                "74 1A",
                ZeroBitBranchTargetRawFileOffset)
        });

    /// <summary>
    /// Projects only captures reached in instruction order. A zero returned
    /// pointer still produces byte-read address zero and is not treated as an
    /// error or as evidence that the read succeeded.
    /// </summary>
    public static DeferredControlCallbackPlan Project(
        DeferredControlCallbackCapture capture)
    {
        uint vtablePointerAddress = AddPayloadOffset(
            capture.PayloadEdi,
            PayloadVtablePointerOffset);
        uint ecxArgumentAddress = vtablePointerAddress;
        uint stackArgumentAddress = AddPayloadOffset(
            capture.PayloadEdi,
            PayloadStackArgumentOffset);

        if (capture.VtablePointerAtPayload8 is not uint vtablePointer)
        {
            return HasAnyAfterVtableLoad(capture)
                ? Stop(
                    capture,
                    DeferredControlCallbackOutcome.UnreachedCaptureProvided,
                    vtablePointerAddress,
                    ecxArgumentAddress,
                    stackArgumentAddress,
                    stopAt: VtablePointerLoadRawFileOffset)
                : Stop(
                    capture,
                    DeferredControlCallbackOutcome.MissingVtablePointer,
                    vtablePointerAddress,
                    ecxArgumentAddress,
                    stackArgumentAddress,
                    stopAt: VtablePointerLoadRawFileOffset);
        }

        if (capture.StackArgumentValueAtPayload4 is not uint)
        {
            return HasAnyAfterStackArgument(capture)
                ? Stop(
                    capture,
                    DeferredControlCallbackOutcome.UnreachedCaptureProvided,
                    vtablePointerAddress,
                    ecxArgumentAddress,
                    stackArgumentAddress,
                    stopAt: StackArgumentLoadRawFileOffset)
                : Stop(
                    capture,
                    DeferredControlCallbackOutcome.MissingStackArgument,
                    vtablePointerAddress,
                    ecxArgumentAddress,
                    stackArgumentAddress,
                    stopAt: StackArgumentLoadRawFileOffset);
        }

        uint indirectSlotAddress = AddPayloadOffset(
            vtablePointer,
            IndirectSlotOffset);
        if (capture.CallResultEax is not uint returnedPointer)
        {
            return capture.ReturnedPointerByte is not null
                ? Stop(
                    capture,
                    DeferredControlCallbackOutcome.UnreachedCaptureProvided,
                    vtablePointerAddress,
                    ecxArgumentAddress,
                    stackArgumentAddress,
                    indirectCallReached: true,
                    indirectSlotAddress: indirectSlotAddress,
                    stopAt: CallReturnRawFileOffset)
                : Stop(
                    capture,
                    DeferredControlCallbackOutcome.MissingCallResult,
                    vtablePointerAddress,
                    ecxArgumentAddress,
                    stackArgumentAddress,
                    indirectCallReached: true,
                    indirectSlotAddress: indirectSlotAddress,
                    stopAt: CallReturnRawFileOffset);
        }

        uint returnedPointerByteAddress = returnedPointer;
        if (capture.ReturnedPointerByte is not byte returnedPointerByte)
        {
            return Stop(
                capture,
                DeferredControlCallbackOutcome.MissingReturnedPointerByte,
                vtablePointerAddress,
                ecxArgumentAddress,
                stackArgumentAddress,
                indirectCallReached: true,
                indirectSlotAddress: indirectSlotAddress,
                returnedPointerByteAddress: returnedPointerByteAddress,
                stopAt: ReturnedPointerTestRawFileOffset);
        }

        bool testBitSet = (returnedPointerByte & ReturnedPointerTestMask) != 0;
        uint routeRawFileOffset = testBitSet
            ? EndRawFileOffsetExclusive
            : ZeroBitBranchTargetRawFileOffset;
        return Stop(
            capture,
            testBitSet
                ? DeferredControlCallbackOutcome.MaskBitSetFallsThrough
                : DeferredControlCallbackOutcome.MaskBitClearBranchesToZeroTarget,
            vtablePointerAddress,
            ecxArgumentAddress,
            stackArgumentAddress,
            indirectCallReached: true,
            indirectSlotAddress: indirectSlotAddress,
            returnedPointerByteAddress: returnedPointerByteAddress,
            testedBits: (byte)(returnedPointerByte & ReturnedPointerTestMask),
            routeRawFileOffset: routeRawFileOffset,
            stopAt: routeRawFileOffset);
    }

    public static uint AddPayloadOffset(uint payloadPointer, uint offset) =>
        unchecked(payloadPointer + offset);

    private static bool HasAnyAfterVtableLoad(
        DeferredControlCallbackCapture capture) =>
        capture.StackArgumentValueAtPayload4 is not null
        || capture.CallResultEax is not null
        || capture.ReturnedPointerByte is not null;

    private static bool HasAnyAfterStackArgument(
        DeferredControlCallbackCapture capture) =>
        capture.CallResultEax is not null
        || capture.ReturnedPointerByte is not null;

    private static DeferredControlCallbackPlan Stop(
        DeferredControlCallbackCapture capture,
        DeferredControlCallbackOutcome outcome,
        uint vtablePointerAddress,
        uint ecxArgumentAddress,
        uint stackArgumentAddress,
        bool indirectCallReached = false,
        uint? indirectSlotAddress = null,
        uint? returnedPointerByteAddress = null,
        byte? testedBits = null,
        uint? routeRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            indirectCallReached,
            vtablePointerAddress,
            ecxArgumentAddress,
            stackArgumentAddress,
            indirectSlotAddress,
            returnedPointerByteAddress,
            testedBits,
            routeRawFileOffset,
            stopAt);
}
