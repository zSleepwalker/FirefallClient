using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveResidueCounterCapture(
    bool? PriorHelperReturnedNormally,
    uint ReceiverEdi,
    uint RetainedEsi,
    uint? ReceiverDwordAtOffset08,
    uint? CounterDwordAtLoadedPointerOffset1C);

public enum UnreliableReceiveResidueCounterOutcome
{
    MissingPriorHelperReturnCapture,
    PriorHelperDidNotReturnNormally,
    MissingReceiverDwordAtOffset08Capture,
    MissingCounterDwordAtLoadedPointerOffset1C,
    CounterDwordAdded
}

public readonly record struct UnreliableReceiveResidueCounterPlan(
    UnreliableReceiveResidueCounterCapture Capture,
    UnreliableReceiveResidueCounterOutcome Outcome,
    bool? PriorHelperNormalGatePassed,
    uint ReceiverDwordAtOffset08Address,
    bool ReceiverDwordAtOffset08Read,
    uint? LoadedPointerEcx,
    bool EsiMinusOneComputed,
    uint? EaxAfterEsiMinusOne,
    bool StackCleanupProjected,
    ushort? StackCleanupBytes,
    uint? CounterDwordAtLoadedPointerOffset1CAddress,
    bool CounterDwordRead,
    uint? CounterDwordReadValue,
    bool CounterDwordWriteProjected,
    uint? CounterDwordWriteValue,
    bool UnreachedReceiverDwordAtOffset08CaptureProvided,
    bool UnreachedCounterDwordAtLoadedPointerOffset1CCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveResidueCounterInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the normal helper-return continuation through its counter update only.
/// </summary>
public static class UnreliableReceiveResidueCounterProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint CounterUpdateStartRawFileOffset = 0x00BE5AE4;
    public const uint CounterUpdateStartVirtualAddress = 0x00FE66E4;
    public const uint CounterUpdateEndRawFileOffsetExclusive = 0x00BE5AF0;
    public const uint CounterUpdateEndVirtualAddressExclusive = 0x00FE66F0;

    public const uint ReceiverDwordAtOffset08 = 0x08;
    public const uint CounterDwordAtLoadedPointerOffset1C = 0x1C;
    public const ushort StackCleanupBytes = 0x14;

    public const uint ReceiverDwordAtOffset08ReadRawFileOffset = 0x00BE5AE4;
    public const uint EsiMinusOneRawFileOffset = 0x00BE5AE7;
    public const uint StackCleanupRawFileOffset = 0x00BE5AEA;
    public const uint CounterDwordAddRawFileOffset = 0x00BE5AED;

    public const string CapturedCounterUpdateBytes =
        "8B 4F 08 8D 46 FF 83 C4 14 01 41 1C";

    public static ImmutableArray<UnreliableReceiveResidueCounterInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveResidueCounterInstructionEvidence(
                "Load ECX from the independently captured DWORD at EDI+8",
                ReceiverDwordAtOffset08ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverDwordAtOffset08ReadRawFileOffset),
                "8B 4F 08"),
            new UnreliableReceiveResidueCounterInstructionEvidence(
                "Compute EAX as retained ESI minus one with 32-bit wraparound",
                EsiMinusOneRawFileOffset,
                RawFileOffsetToVirtualAddress(EsiMinusOneRawFileOffset),
                "8D 46 FF"),
            new UnreliableReceiveResidueCounterInstructionEvidence(
                "Clean up 0x14 bytes from the stack",
                StackCleanupRawFileOffset,
                RawFileOffsetToVirtualAddress(StackCleanupRawFileOffset),
                "83 C4 14"),
            new UnreliableReceiveResidueCounterInstructionEvidence(
                "Add EAX to the DWORD at ECX+0x1C with 32-bit wraparound",
                CounterDwordAddRawFileOffset,
                RawFileOffsetToVirtualAddress(CounterDwordAddRawFileOffset),
                "01 41 1C"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAtOffset08Address(uint receiverEdi) =>
        unchecked(receiverEdi + ReceiverDwordAtOffset08);

    public static uint CounterDwordAtLoadedPointerOffset1CAddress(uint loadedPointerEcx) =>
        unchecked(loadedPointerEcx + CounterDwordAtLoadedPointerOffset1C);

    public static UnreliableReceiveResidueCounterPlan Project(
        UnreliableReceiveResidueCounterCapture capture)
    {
        uint receiverDwordAtOffset08Address = ReceiverDwordAtOffset08Address(capture.ReceiverEdi);

        if (capture.PriorHelperReturnedNormally is not bool priorHelperReturnedNormally)
        {
            return new UnreliableReceiveResidueCounterPlan(
                capture,
                UnreliableReceiveResidueCounterOutcome.MissingPriorHelperReturnCapture,
                PriorHelperNormalGatePassed: null,
                receiverDwordAtOffset08Address,
                ReceiverDwordAtOffset08Read: false,
                LoadedPointerEcx: null,
                EsiMinusOneComputed: false,
                EaxAfterEsiMinusOne: null,
                StackCleanupProjected: false,
                StackCleanupBytes: null,
                CounterDwordAtLoadedPointerOffset1CAddress: null,
                CounterDwordRead: false,
                CounterDwordReadValue: null,
                CounterDwordWriteProjected: false,
                CounterDwordWriteValue: null,
                UnreachedReceiverDwordAtOffset08CaptureProvided:
                    capture.ReceiverDwordAtOffset08.HasValue,
                UnreachedCounterDwordAtLoadedPointerOffset1CCaptureProvided:
                    capture.CounterDwordAtLoadedPointerOffset1C.HasValue,
                StopAtRawFileOffsetExclusive: CounterUpdateStartRawFileOffset);
        }

        if (!priorHelperReturnedNormally)
        {
            return new UnreliableReceiveResidueCounterPlan(
                capture,
                UnreliableReceiveResidueCounterOutcome.PriorHelperDidNotReturnNormally,
                PriorHelperNormalGatePassed: false,
                receiverDwordAtOffset08Address,
                ReceiverDwordAtOffset08Read: false,
                LoadedPointerEcx: null,
                EsiMinusOneComputed: false,
                EaxAfterEsiMinusOne: null,
                StackCleanupProjected: false,
                StackCleanupBytes: null,
                CounterDwordAtLoadedPointerOffset1CAddress: null,
                CounterDwordRead: false,
                CounterDwordReadValue: null,
                CounterDwordWriteProjected: false,
                CounterDwordWriteValue: null,
                UnreachedReceiverDwordAtOffset08CaptureProvided:
                    capture.ReceiverDwordAtOffset08.HasValue,
                UnreachedCounterDwordAtLoadedPointerOffset1CCaptureProvided:
                    capture.CounterDwordAtLoadedPointerOffset1C.HasValue,
                StopAtRawFileOffsetExclusive: CounterUpdateStartRawFileOffset);
        }

        if (capture.ReceiverDwordAtOffset08 is not uint loadedPointerEcx)
        {
            return new UnreliableReceiveResidueCounterPlan(
                capture,
                UnreliableReceiveResidueCounterOutcome.MissingReceiverDwordAtOffset08Capture,
                PriorHelperNormalGatePassed: true,
                receiverDwordAtOffset08Address,
                ReceiverDwordAtOffset08Read: false,
                LoadedPointerEcx: null,
                EsiMinusOneComputed: false,
                EaxAfterEsiMinusOne: null,
                StackCleanupProjected: false,
                StackCleanupBytes: null,
                CounterDwordAtLoadedPointerOffset1CAddress: null,
                CounterDwordRead: false,
                CounterDwordReadValue: null,
                CounterDwordWriteProjected: false,
                CounterDwordWriteValue: null,
                UnreachedReceiverDwordAtOffset08CaptureProvided: false,
                UnreachedCounterDwordAtLoadedPointerOffset1CCaptureProvided:
                    capture.CounterDwordAtLoadedPointerOffset1C.HasValue,
                StopAtRawFileOffsetExclusive: ReceiverDwordAtOffset08ReadRawFileOffset);
        }

        uint eaxAfterEsiMinusOne = unchecked(capture.RetainedEsi - 1);
        uint counterAddress = CounterDwordAtLoadedPointerOffset1CAddress(loadedPointerEcx);

        if (capture.CounterDwordAtLoadedPointerOffset1C is not uint counterValue)
        {
            return new UnreliableReceiveResidueCounterPlan(
                capture,
                UnreliableReceiveResidueCounterOutcome.MissingCounterDwordAtLoadedPointerOffset1C,
                PriorHelperNormalGatePassed: true,
                receiverDwordAtOffset08Address,
                ReceiverDwordAtOffset08Read: true,
                loadedPointerEcx,
                EsiMinusOneComputed: true,
                eaxAfterEsiMinusOne,
                StackCleanupProjected: true,
                StackCleanupBytes: StackCleanupBytes,
                counterAddress,
                CounterDwordRead: false,
                CounterDwordReadValue: null,
                CounterDwordWriteProjected: false,
                CounterDwordWriteValue: null,
                UnreachedReceiverDwordAtOffset08CaptureProvided: false,
                UnreachedCounterDwordAtLoadedPointerOffset1CCaptureProvided: false,
                StopAtRawFileOffsetExclusive: CounterDwordAddRawFileOffset);
        }

        return new UnreliableReceiveResidueCounterPlan(
            capture,
            UnreliableReceiveResidueCounterOutcome.CounterDwordAdded,
            PriorHelperNormalGatePassed: true,
            receiverDwordAtOffset08Address,
            ReceiverDwordAtOffset08Read: true,
            loadedPointerEcx,
            EsiMinusOneComputed: true,
            eaxAfterEsiMinusOne,
            StackCleanupProjected: true,
            StackCleanupBytes: StackCleanupBytes,
            counterAddress,
            CounterDwordRead: true,
            counterValue,
            CounterDwordWriteProjected: true,
            CounterDwordWriteValue: unchecked(counterValue + eaxAfterEsiMinusOne),
            UnreachedReceiverDwordAtOffset08CaptureProvided: false,
            UnreachedCounterDwordAtLoadedPointerOffset1CCaptureProvided: false,
            StopAtRawFileOffsetExclusive: CounterUpdateEndRawFileOffsetExclusive);
    }
}
