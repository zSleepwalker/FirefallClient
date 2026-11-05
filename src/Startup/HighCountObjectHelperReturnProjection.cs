using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Entry EAX is the full result of the helper at raw 0xBE6B30.
/// Entry ESI is zero or an independent reload at raw 0xBE6456; neither register is null-guarded.
/// </summary>
public readonly record struct HighCountObjectHelperReturnCapture(
    uint? EntryEax,
    uint? EntryEsi,
    uint? DwordAtEntryEaxPlus8,
    uint? DwordAtEntryEsiPlus8,
    uint? DwordAtEntryEsiPlus4,
    uint? DwordAtComputedSlot);

public enum HighCountObjectHelperReturnOutcome
{
    MissingEntryEax,
    MissingDwordAtEntryEaxPlus8,
    MissingEntryEsi,
    MissingDwordAtEntryEsiPlus8,
    MissingDwordAtEntryEsiPlus4,
    MissingDwordAtComputedSlot,
    Returned
}

public readonly record struct HighCountObjectHelperReturnReadProjection(
    bool ReadSiteReached,
    uint? Address,
    bool CaptureProvided,
    bool MissingCapture,
    bool UnreachedCaptureProvided,
    uint? Value);

public readonly record struct HighCountObjectHelperReturnPlan(
    HighCountObjectHelperReturnCapture Capture,
    HighCountObjectHelperReturnOutcome Outcome,
    HighCountObjectHelperReturnReadProjection DwordAtEntryEaxPlus8,
    HighCountObjectHelperReturnReadProjection DwordAtEntryEsiPlus8,
    HighCountObjectHelperReturnReadProjection DwordAtEntryEsiPlus4,
    uint? EntryEcxAfterMove,
    uint? DecrementedEax,
    uint? ShiftedEcx,
    uint? MaskedEcx,
    uint? MaskedEdx,
    uint? ComputedSlotAddress,
    HighCountObjectHelperReturnReadProjection DwordAtComputedSlot,
    uint? ReturnEax,
    uint StopAtRawFileOffsetExclusive,
    bool UnreachedEntryEsiCaptureProvided)
{
    public bool OwnershipInferred => false;
}

public readonly record struct HighCountObjectHelperReturnInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the independent reads and DWORD arithmetic in the shared helper return tail.
/// The returned EAX is a wrapped value; no object ownership is inferred.
/// </summary>
public static class HighCountObjectHelperReturnProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6458;
    public const uint EntryVirtualAddress = 0x00FE7058;
    public const uint EndRawFileOffsetExclusive = 0x00BE6477;
    public const uint EndVirtualAddressExclusive = 0x00FE7077;
    public const uint CapturedWindowLength =
        EndRawFileOffsetExclusive - EntryRawFileOffset;

    public const uint DwordAtEntryEaxPlus8ReadRawFileOffset = 0x00BE6458;
    public const uint DwordAtEntryEsiPlus8ReadRawFileOffset = 0x00BE645D;
    public const uint DwordAtEntryEsiPlus4ReadRawFileOffset = 0x00BE6466;
    public const uint DwordAtComputedSlotReadRawFileOffset = 0x00BE646D;
    public const uint ReturnRawFileOffset = 0x00BE6476;

    public const string CapturedWindowBytes =
        "8B 50 08 8B CA 8B 46 08 48 C1 E9 02 23 C8 8B 46 04 "
        + "83 E2 03 5E 8B 04 88 8D 04 90 8B E5 5D C3";

    public static ImmutableArray<HighCountObjectHelperReturnInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new HighCountObjectHelperReturnInstructionEvidence(
                "Load the DWORD at entry EAX+8 into EDX",
                DwordAtEntryEaxPlus8ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(DwordAtEntryEaxPlus8ReadRawFileOffset),
                "8B 50 08"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Copy EDX into ECX",
                0x00BE645B,
                RawFileOffsetToVirtualAddress(0x00BE645B),
                "8B CA"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Independently load the DWORD at entry ESI+8 into EAX",
                DwordAtEntryEsiPlus8ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(DwordAtEntryEsiPlus8ReadRawFileOffset),
                "8B 46 08"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Decrement EAX with DWORD wrap",
                0x00BE6460,
                RawFileOffsetToVirtualAddress(0x00BE6460),
                "48"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Shift ECX right by two",
                0x00BE6461,
                RawFileOffsetToVirtualAddress(0x00BE6461),
                "C1 E9 02"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Mask shifted ECX by decremented EAX",
                0x00BE6464,
                RawFileOffsetToVirtualAddress(0x00BE6464),
                "23 C8"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Independently load the DWORD at entry ESI+4 into EAX",
                DwordAtEntryEsiPlus4ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(DwordAtEntryEsiPlus4ReadRawFileOffset),
                "8B 46 04"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Keep the low two bits of EDX",
                0x00BE6469,
                RawFileOffsetToVirtualAddress(0x00BE6469),
                "83 E2 03"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Restore ESI",
                0x00BE646C,
                RawFileOffsetToVirtualAddress(0x00BE646C),
                "5E"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Independently load the DWORD at EAX+ECX*4",
                DwordAtComputedSlotReadRawFileOffset,
                RawFileOffsetToVirtualAddress(DwordAtComputedSlotReadRawFileOffset),
                "8B 04 88"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Add EDX*4 to EAX with DWORD wrap",
                0x00BE6470,
                RawFileOffsetToVirtualAddress(0x00BE6470),
                "8D 04 90"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Restore ESP from EBP",
                0x00BE6473,
                RawFileOffsetToVirtualAddress(0x00BE6473),
                "8B E5"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Restore EBP",
                0x00BE6475,
                RawFileOffsetToVirtualAddress(0x00BE6475),
                "5D"),
            new HighCountObjectHelperReturnInstructionEvidence(
                "Return the full EAX value",
                ReturnRawFileOffset,
                RawFileOffsetToVirtualAddress(ReturnRawFileOffset),
                "C3"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static HighCountObjectHelperReturnPlan Project(
        HighCountObjectHelperReturnCapture capture)
    {
        HighCountObjectHelperReturnReadProjection dwordAtEntryEaxPlus8 =
            ReachedRead(
                capture.EntryEax is uint entryEax
                    ? unchecked(entryEax + 8)
                    : null,
                capture.DwordAtEntryEaxPlus8,
                capture.EntryEax.HasValue);

        if (capture.EntryEax is not uint eax)
        {
            return Stop(
                capture,
                HighCountObjectHelperReturnOutcome.MissingEntryEax,
                dwordAtEntryEaxPlus8,
                UnreachedRead(capture.DwordAtEntryEsiPlus8),
                UnreachedRead(capture.DwordAtEntryEsiPlus4),
                UnreachedRead(capture.DwordAtComputedSlot),
                DwordAtEntryEaxPlus8ReadRawFileOffset,
                unreachedEntryEsiCaptureProvided: capture.EntryEsi.HasValue);
        }

        if (capture.DwordAtEntryEaxPlus8 is not uint edx)
        {
            return Stop(
                capture,
                HighCountObjectHelperReturnOutcome.MissingDwordAtEntryEaxPlus8,
                dwordAtEntryEaxPlus8,
                UnreachedRead(capture.DwordAtEntryEsiPlus8),
                UnreachedRead(capture.DwordAtEntryEsiPlus4),
                UnreachedRead(capture.DwordAtComputedSlot),
                DwordAtEntryEaxPlus8ReadRawFileOffset,
                unreachedEntryEsiCaptureProvided: capture.EntryEsi.HasValue);
        }

        uint ecxAfterMove = edx;
        HighCountObjectHelperReturnReadProjection dwordAtEntryEsiPlus8 =
            ReachedRead(
                capture.EntryEsi is uint entryEsiForPlus8
                    ? unchecked(entryEsiForPlus8 + 8)
                    : null,
                capture.DwordAtEntryEsiPlus8,
                capture.EntryEsi.HasValue);
        if (capture.EntryEsi is not uint esi)
        {
            return Stop(
                capture,
                HighCountObjectHelperReturnOutcome.MissingEntryEsi,
                dwordAtEntryEaxPlus8,
                dwordAtEntryEsiPlus8,
                UnreachedRead(capture.DwordAtEntryEsiPlus4),
                UnreachedRead(capture.DwordAtComputedSlot),
                DwordAtEntryEsiPlus8ReadRawFileOffset,
                entryEcxAfterMove: ecxAfterMove);
        }

        if (capture.DwordAtEntryEsiPlus8 is not uint eaxFromEsiPlus8)
        {
            return Stop(
                capture,
                HighCountObjectHelperReturnOutcome.MissingDwordAtEntryEsiPlus8,
                dwordAtEntryEaxPlus8,
                dwordAtEntryEsiPlus8,
                UnreachedRead(capture.DwordAtEntryEsiPlus4),
                UnreachedRead(capture.DwordAtComputedSlot),
                DwordAtEntryEsiPlus8ReadRawFileOffset,
                entryEcxAfterMove: ecxAfterMove);
        }

        uint decrementedEax = unchecked(eaxFromEsiPlus8 - 1);
        uint shiftedEcx = ecxAfterMove >> 2;
        uint maskedEcx = shiftedEcx & decrementedEax;
        HighCountObjectHelperReturnReadProjection dwordAtEntryEsiPlus4 =
            ReachedRead(
                unchecked(esi + 4),
                capture.DwordAtEntryEsiPlus4);
        if (capture.DwordAtEntryEsiPlus4 is not uint eaxFromEsiPlus4)
        {
            return Stop(
                capture,
                HighCountObjectHelperReturnOutcome.MissingDwordAtEntryEsiPlus4,
                dwordAtEntryEaxPlus8,
                dwordAtEntryEsiPlus8,
                dwordAtEntryEsiPlus4,
                UnreachedRead(capture.DwordAtComputedSlot),
                DwordAtEntryEsiPlus4ReadRawFileOffset,
                entryEcxAfterMove: ecxAfterMove,
                decrementedEax: decrementedEax,
                shiftedEcx: shiftedEcx,
                maskedEcx: maskedEcx);
        }

        uint maskedEdx = edx & 3;
        uint computedSlotAddress = unchecked(eaxFromEsiPlus4 + (maskedEcx * 4));
        HighCountObjectHelperReturnReadProjection dwordAtComputedSlot =
            ReachedRead(computedSlotAddress, capture.DwordAtComputedSlot);
        if (capture.DwordAtComputedSlot is not uint dwordAtSlot)
        {
            return Stop(
                capture,
                HighCountObjectHelperReturnOutcome.MissingDwordAtComputedSlot,
                dwordAtEntryEaxPlus8,
                dwordAtEntryEsiPlus8,
                dwordAtEntryEsiPlus4,
                dwordAtComputedSlot,
                DwordAtComputedSlotReadRawFileOffset,
                entryEcxAfterMove: ecxAfterMove,
                decrementedEax: decrementedEax,
                shiftedEcx: shiftedEcx,
                maskedEcx: maskedEcx,
                maskedEdx: maskedEdx,
                computedSlotAddress: computedSlotAddress);
        }

        uint returnEax = unchecked(dwordAtSlot + (maskedEdx * 4));
        return Stop(
            capture,
            HighCountObjectHelperReturnOutcome.Returned,
            dwordAtEntryEaxPlus8,
            dwordAtEntryEsiPlus8,
            dwordAtEntryEsiPlus4,
            dwordAtComputedSlot,
            EndRawFileOffsetExclusive,
            entryEcxAfterMove: ecxAfterMove,
            decrementedEax: decrementedEax,
            shiftedEcx: shiftedEcx,
            maskedEcx: maskedEcx,
            maskedEdx: maskedEdx,
            computedSlotAddress: computedSlotAddress,
            returnEax: returnEax);
    }

    private static HighCountObjectHelperReturnReadProjection ReachedRead(
        uint? address,
        uint? capture,
        bool prerequisitesKnown = true) =>
        new(
            ReadSiteReached: true,
            Address: address,
            CaptureProvided: capture.HasValue,
            MissingCapture: prerequisitesKnown && !capture.HasValue,
            UnreachedCaptureProvided: !prerequisitesKnown && capture.HasValue,
            Value: prerequisitesKnown ? capture : null);

    private static HighCountObjectHelperReturnReadProjection UnreachedRead(
        uint? capture) =>
        new(
            ReadSiteReached: false,
            Address: null,
            CaptureProvided: capture.HasValue,
            MissingCapture: false,
            UnreachedCaptureProvided: capture.HasValue,
            Value: null);

    private static HighCountObjectHelperReturnPlan Stop(
        HighCountObjectHelperReturnCapture capture,
        HighCountObjectHelperReturnOutcome outcome,
        HighCountObjectHelperReturnReadProjection dwordAtEntryEaxPlus8,
        HighCountObjectHelperReturnReadProjection dwordAtEntryEsiPlus8,
        HighCountObjectHelperReturnReadProjection dwordAtEntryEsiPlus4,
        HighCountObjectHelperReturnReadProjection dwordAtComputedSlot,
        uint stopAtRawFileOffsetExclusive,
        uint? entryEcxAfterMove = null,
        uint? decrementedEax = null,
        uint? shiftedEcx = null,
        uint? maskedEcx = null,
        uint? maskedEdx = null,
        uint? computedSlotAddress = null,
        uint? returnEax = null,
        bool unreachedEntryEsiCaptureProvided = false) =>
        new(
            capture,
            outcome,
            dwordAtEntryEaxPlus8,
            dwordAtEntryEsiPlus8,
            dwordAtEntryEsiPlus4,
            entryEcxAfterMove,
            decrementedEax,
            shiftedEcx,
            maskedEcx,
            maskedEdx,
            computedSlotAddress,
            dwordAtComputedSlot,
            returnEax,
            stopAtRawFileOffsetExclusive,
            unreachedEntryEsiCaptureProvided);
}
