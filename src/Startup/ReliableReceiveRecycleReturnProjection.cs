using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum ReliableReceiveRecycleReturnRoute
{
    HighDelta,
    ExistingRecord
}

public readonly record struct ReliableReceiveRecycleReturnCapture(
    ReliableReceiveRecycleReturnRoute Route,
    uint ReceiverPointerInEax,
    uint PayloadPointerInEdi,
    uint? OldCounterDword,
    uint? GlobalDword,
    bool? HelperReturnedNormally,
    uint? HelperReturnEax);

public enum ReliableReceiveRecycleReturnOutcome
{
    MissingOldCounterCapture,
    UnreachedLaterCapture,
    MissingGlobalCapture,
    MissingHelperReturnCapture,
    HelperDidNotReturnNormally,
    UnreachedHelperReturnEaxCapture,
    ReturnCompleted
}

public readonly record struct ReliableReceiveRecycleReturnPlan(
    ReliableReceiveRecycleReturnCapture Capture,
    ReliableReceiveRecycleReturnOutcome Outcome,
    bool OldCounterRead,
    uint? CounterAddress,
    uint? OldCounterValue,
    bool CounterIncrementReached,
    uint? IncrementedCounterValue,
    bool GlobalRead,
    uint? GlobalAddress,
    uint? GlobalValue,
    uint? EcX,
    bool PayloadPushed,
    uint? PushedPayloadPointer,
    bool HelperCallReached,
    uint? HelperTargetRawFileOffset,
    bool? HelperReturnedNormally,
    bool AlWriteReached,
    byte? AlValue,
    bool HelperReturnEaxCaptureUnreached,
    uint? EaxAfterAlWrite,
    bool EpilogueReached,
    bool RegistersPopped,
    bool Ret4Reached,
    ushort? RetStackBytes,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveRecycleReturnInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the observed counter increment, global load, helper call, and normal return
/// boundary for the two reliable-receive recycle paths.
/// </summary>
public static class ReliableReceiveRecycleReturnProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint GlobalPointerAddress = 0x01DFFF84;
    public const uint HelperTargetRawFileOffset = 0x00BD5C60;
    public const uint ReceiverCounterOffset = 0x20;
    public const ushort RetStackBytes = 4;

    public const uint HighDeltaStartRawFileOffset = 0x00BE52C7;
    public const uint HighDeltaStartVirtualAddress = 0x00FE5EC7;
    public const uint HighDeltaEndRawFileOffsetExclusive = 0x00BE52DF;
    public const uint ExistingRecordStartRawFileOffset = 0x00BE5375;
    public const uint ExistingRecordStartVirtualAddress = 0x00FE5F75;
    public const uint ExistingRecordEndRawFileOffsetExclusive = 0x00BE538D;

    public const uint HighDeltaCounterIncrementRawFileOffset = 0x00BE52C7;
    public const uint HighDeltaGlobalLoadRawFileOffset = 0x00BE52CA;
    public const uint HighDeltaPayloadPushRawFileOffset = 0x00BE52D0;
    public const uint HighDeltaHelperCallRawFileOffset = 0x00BE52D1;
    public const uint HighDeltaHelperReturnRawFileOffset = 0x00BE52D6;
    public const uint HighDeltaEpilogueRawFileOffset = 0x00BE52D8;
    public const uint HighDeltaRet4RawFileOffset = 0x00BE52DC;

    public const uint ExistingRecordCounterIncrementRawFileOffset = 0x00BE5375;
    public const uint ExistingRecordGlobalLoadRawFileOffset = 0x00BE5378;
    public const uint ExistingRecordPayloadPushRawFileOffset = 0x00BE537E;
    public const uint ExistingRecordHelperCallRawFileOffset = 0x00BE537F;
    public const uint ExistingRecordHelperReturnRawFileOffset = 0x00BE5384;
    public const uint ExistingRecordEpilogueRawFileOffset = 0x00BE5386;
    public const uint ExistingRecordRet4RawFileOffset = 0x00BE538A;

    public const string HighDeltaWindowBytes =
        "FF 40 20 8B 0D 84 FF DF 01 57 E8 8A 09 FF FF B0 01 "
        + "5F 5E 5B 5D C2 04 00";
    public const string ExistingRecordWindowBytes =
        "FF 40 20 8B 0D 84 FF DF 01 57 E8 DC 08 FF FF B0 01 "
        + "5F 5E 5B 5D C2 04 00";

    public static ImmutableArray<ReliableReceiveRecycleReturnInstructionEvidence> HighDeltaInstructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "Increment the DWORD at receiver EAX+0x20 with 32-bit wrap",
                HighDeltaCounterIncrementRawFileOffset,
                RawFileOffsetToVirtualAddress(HighDeltaCounterIncrementRawFileOffset),
                "FF 40 20"),
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "Load ECX from the absolute global DWORD at VA 0x01DFFF84",
                HighDeltaGlobalLoadRawFileOffset,
                RawFileOffsetToVirtualAddress(HighDeltaGlobalLoadRawFileOffset),
                "8B 0D 84 FF DF 01"),
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "Push the captured payload pointer from EDI",
                HighDeltaPayloadPushRawFileOffset,
                RawFileOffsetToVirtualAddress(HighDeltaPayloadPushRawFileOffset),
                "57"),
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "Call the observed helper at raw 0xBD5C60",
                HighDeltaHelperCallRawFileOffset,
                RawFileOffsetToVirtualAddress(HighDeltaHelperCallRawFileOffset),
                "E8 8A 09 FF FF"),
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "On normal helper return, write AL=1 without replacing EAX upper bits",
                HighDeltaHelperReturnRawFileOffset,
                RawFileOffsetToVirtualAddress(HighDeltaHelperReturnRawFileOffset),
                "B0 01"),
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "Restore EDI, ESI, EBX, and EBP",
                HighDeltaEpilogueRawFileOffset,
                RawFileOffsetToVirtualAddress(HighDeltaEpilogueRawFileOffset),
                "5F 5E 5B 5D"),
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "Return and pop four argument bytes",
                HighDeltaRet4RawFileOffset,
                RawFileOffsetToVirtualAddress(HighDeltaRet4RawFileOffset),
                "C2 04 00"));

    public static ImmutableArray<ReliableReceiveRecycleReturnInstructionEvidence> ExistingRecordInstructions { get; } =
        ImmutableArray.Create(
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "Increment the DWORD at receiver EAX+0x20 with 32-bit wrap",
                ExistingRecordCounterIncrementRawFileOffset,
                RawFileOffsetToVirtualAddress(ExistingRecordCounterIncrementRawFileOffset),
                "FF 40 20"),
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "Load ECX from the absolute global DWORD at VA 0x01DFFF84",
                ExistingRecordGlobalLoadRawFileOffset,
                RawFileOffsetToVirtualAddress(ExistingRecordGlobalLoadRawFileOffset),
                "8B 0D 84 FF DF 01"),
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "Push the captured payload pointer from EDI",
                ExistingRecordPayloadPushRawFileOffset,
                RawFileOffsetToVirtualAddress(ExistingRecordPayloadPushRawFileOffset),
                "57"),
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "Call the observed helper at raw 0xBD5C60",
                ExistingRecordHelperCallRawFileOffset,
                RawFileOffsetToVirtualAddress(ExistingRecordHelperCallRawFileOffset),
                "E8 DC 08 FF FF"),
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "On normal helper return, write AL=1 without replacing EAX upper bits",
                ExistingRecordHelperReturnRawFileOffset,
                RawFileOffsetToVirtualAddress(ExistingRecordHelperReturnRawFileOffset),
                "B0 01"),
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "Restore EDI, ESI, EBX, and EBP",
                ExistingRecordEpilogueRawFileOffset,
                RawFileOffsetToVirtualAddress(ExistingRecordEpilogueRawFileOffset),
                "5F 5E 5B 5D"),
            new ReliableReceiveRecycleReturnInstructionEvidence(
                "Return and pop four argument bytes",
                ExistingRecordRet4RawFileOffset,
                RawFileOffsetToVirtualAddress(ExistingRecordRet4RawFileOffset),
                "C2 04 00"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint CounterAddress(uint receiverPointerInEax) =>
        unchecked(receiverPointerInEax + ReceiverCounterOffset);

    public static uint StartRawFileOffset(ReliableReceiveRecycleReturnRoute route) =>
        route switch
        {
            ReliableReceiveRecycleReturnRoute.HighDelta => HighDeltaStartRawFileOffset,
            ReliableReceiveRecycleReturnRoute.ExistingRecord => ExistingRecordStartRawFileOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };

    public static uint EndRawFileOffsetExclusive(ReliableReceiveRecycleReturnRoute route) =>
        route switch
        {
            ReliableReceiveRecycleReturnRoute.HighDelta => HighDeltaEndRawFileOffsetExclusive,
            ReliableReceiveRecycleReturnRoute.ExistingRecord => ExistingRecordEndRawFileOffsetExclusive,
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };

    public static uint CounterIncrementRawFileOffset(ReliableReceiveRecycleReturnRoute route) =>
        route switch
        {
            ReliableReceiveRecycleReturnRoute.HighDelta => HighDeltaCounterIncrementRawFileOffset,
            ReliableReceiveRecycleReturnRoute.ExistingRecord => ExistingRecordCounterIncrementRawFileOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };

    public static uint GlobalLoadRawFileOffset(ReliableReceiveRecycleReturnRoute route) =>
        route switch
        {
            ReliableReceiveRecycleReturnRoute.HighDelta => HighDeltaGlobalLoadRawFileOffset,
            ReliableReceiveRecycleReturnRoute.ExistingRecord => ExistingRecordGlobalLoadRawFileOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };

    public static uint PayloadPushRawFileOffset(ReliableReceiveRecycleReturnRoute route) =>
        route switch
        {
            ReliableReceiveRecycleReturnRoute.HighDelta => HighDeltaPayloadPushRawFileOffset,
            ReliableReceiveRecycleReturnRoute.ExistingRecord => ExistingRecordPayloadPushRawFileOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };

    public static uint HelperCallRawFileOffset(ReliableReceiveRecycleReturnRoute route) =>
        route switch
        {
            ReliableReceiveRecycleReturnRoute.HighDelta => HighDeltaHelperCallRawFileOffset,
            ReliableReceiveRecycleReturnRoute.ExistingRecord => ExistingRecordHelperCallRawFileOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };

    public static uint HelperReturnRawFileOffset(ReliableReceiveRecycleReturnRoute route) =>
        route switch
        {
            ReliableReceiveRecycleReturnRoute.HighDelta => HighDeltaHelperReturnRawFileOffset,
            ReliableReceiveRecycleReturnRoute.ExistingRecord => ExistingRecordHelperReturnRawFileOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };

    public static uint EpilogueRawFileOffset(ReliableReceiveRecycleReturnRoute route) =>
        route switch
        {
            ReliableReceiveRecycleReturnRoute.HighDelta => HighDeltaEpilogueRawFileOffset,
            ReliableReceiveRecycleReturnRoute.ExistingRecord => ExistingRecordEpilogueRawFileOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };

    public static uint Ret4RawFileOffset(ReliableReceiveRecycleReturnRoute route) =>
        route switch
        {
            ReliableReceiveRecycleReturnRoute.HighDelta => HighDeltaRet4RawFileOffset,
            ReliableReceiveRecycleReturnRoute.ExistingRecord => ExistingRecordRet4RawFileOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };

    public static string CapturedWindowBytes(ReliableReceiveRecycleReturnRoute route) =>
        route switch
        {
            ReliableReceiveRecycleReturnRoute.HighDelta => HighDeltaWindowBytes,
            ReliableReceiveRecycleReturnRoute.ExistingRecord => ExistingRecordWindowBytes,
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };

    public static ImmutableArray<ReliableReceiveRecycleReturnInstructionEvidence> Instructions(
        ReliableReceiveRecycleReturnRoute route) =>
        route switch
        {
            ReliableReceiveRecycleReturnRoute.HighDelta => HighDeltaInstructions,
            ReliableReceiveRecycleReturnRoute.ExistingRecord => ExistingRecordInstructions,
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };

    public static uint ComputedHelperTargetRawFileOffset(ReliableReceiveRecycleReturnRoute route) =>
        RelativeCallTarget(
            HelperCallRawFileOffset(route),
            route switch
            {
                ReliableReceiveRecycleReturnRoute.HighDelta => unchecked((int)0xFFFF098A),
                ReliableReceiveRecycleReturnRoute.ExistingRecord => unchecked((int)0xFFFF08DC),
                _ => throw new ArgumentOutOfRangeException(nameof(route))
            });

    public static uint RelativeCallTarget(uint instructionRawFileOffset, int displacement) =>
        unchecked(instructionRawFileOffset + 5u + (uint)displacement);

    public static ReliableReceiveRecycleReturnPlan Project(
        ReliableReceiveRecycleReturnCapture capture)
    {
        uint counterAddress = CounterAddress(capture.ReceiverPointerInEax);
        uint globalAddress = GlobalPointerAddress;
        uint counterReadRaw = CounterIncrementRawFileOffset(capture.Route);
        uint globalReadRaw = GlobalLoadRawFileOffset(capture.Route);
        uint helperReturnRaw = HelperReturnRawFileOffset(capture.Route);
        uint epilogueRaw = EpilogueRawFileOffset(capture.Route);
        uint ret4Raw = Ret4RawFileOffset(capture.Route);
        uint helperTargetRaw = ComputedHelperTargetRawFileOffset(capture.Route);

        if (capture.OldCounterDword is not uint oldCounter)
        {
            bool laterCaptureProvided = capture.GlobalDword.HasValue
                || capture.HelperReturnedNormally.HasValue
                || capture.HelperReturnEax.HasValue;
            return new ReliableReceiveRecycleReturnPlan(
                capture,
                laterCaptureProvided
                    ? ReliableReceiveRecycleReturnOutcome.UnreachedLaterCapture
                    : ReliableReceiveRecycleReturnOutcome.MissingOldCounterCapture,
                OldCounterRead: false,
                counterAddress,
                OldCounterValue: null,
                CounterIncrementReached: false,
                IncrementedCounterValue: null,
                GlobalRead: false,
                globalAddress,
                GlobalValue: null,
                EcX: null,
                PayloadPushed: false,
                PushedPayloadPointer: null,
                HelperCallReached: false,
                HelperTargetRawFileOffset: null,
                HelperReturnedNormally: null,
                AlWriteReached: false,
                AlValue: null,
                HelperReturnEaxCaptureUnreached: capture.HelperReturnEax.HasValue,
                EaxAfterAlWrite: null,
                EpilogueReached: false,
                RegistersPopped: false,
                Ret4Reached: false,
                RetStackBytes: null,
                StopAtRawFileOffsetExclusive: counterReadRaw);
        }

        uint incrementedCounter = unchecked(oldCounter + 1);
        if (capture.GlobalDword is not uint globalValue)
        {
            bool laterCaptureProvided = capture.HelperReturnedNormally.HasValue
                || capture.HelperReturnEax.HasValue;
            return new ReliableReceiveRecycleReturnPlan(
                capture,
                laterCaptureProvided
                    ? ReliableReceiveRecycleReturnOutcome.UnreachedLaterCapture
                    : ReliableReceiveRecycleReturnOutcome.MissingGlobalCapture,
                OldCounterRead: true,
                counterAddress,
                oldCounter,
                CounterIncrementReached: true,
                incrementedCounter,
                GlobalRead: false,
                globalAddress,
                GlobalValue: null,
                EcX: null,
                PayloadPushed: false,
                PushedPayloadPointer: null,
                HelperCallReached: false,
                HelperTargetRawFileOffset: null,
                HelperReturnedNormally: null,
                AlWriteReached: false,
                AlValue: null,
                HelperReturnEaxCaptureUnreached: capture.HelperReturnEax.HasValue,
                EaxAfterAlWrite: null,
                EpilogueReached: false,
                RegistersPopped: false,
                Ret4Reached: false,
                RetStackBytes: null,
                StopAtRawFileOffsetExclusive: globalReadRaw);
        }

        if (capture.HelperReturnedNormally is not bool helperReturnedNormally)
        {
            return new ReliableReceiveRecycleReturnPlan(
                capture,
                capture.HelperReturnEax.HasValue
                    ? ReliableReceiveRecycleReturnOutcome.UnreachedHelperReturnEaxCapture
                    : ReliableReceiveRecycleReturnOutcome.MissingHelperReturnCapture,
                OldCounterRead: true,
                counterAddress,
                oldCounter,
                CounterIncrementReached: true,
                incrementedCounter,
                GlobalRead: true,
                globalAddress,
                globalValue,
                globalValue,
                PayloadPushed: true,
                capture.PayloadPointerInEdi,
                HelperCallReached: true,
                helperTargetRaw,
                HelperReturnedNormally: null,
                AlWriteReached: false,
                AlValue: null,
                HelperReturnEaxCaptureUnreached: capture.HelperReturnEax.HasValue,
                EaxAfterAlWrite: null,
                EpilogueReached: false,
                RegistersPopped: false,
                Ret4Reached: false,
                RetStackBytes: null,
                StopAtRawFileOffsetExclusive: helperReturnRaw);
        }

        if (!helperReturnedNormally)
        {
            return new ReliableReceiveRecycleReturnPlan(
                capture,
                capture.HelperReturnEax.HasValue
                    ? ReliableReceiveRecycleReturnOutcome.UnreachedHelperReturnEaxCapture
                    : ReliableReceiveRecycleReturnOutcome.HelperDidNotReturnNormally,
                OldCounterRead: true,
                counterAddress,
                oldCounter,
                CounterIncrementReached: true,
                incrementedCounter,
                GlobalRead: true,
                globalAddress,
                globalValue,
                globalValue,
                PayloadPushed: true,
                capture.PayloadPointerInEdi,
                HelperCallReached: true,
                helperTargetRaw,
                HelperReturnedNormally: false,
                AlWriteReached: false,
                AlValue: null,
                HelperReturnEaxCaptureUnreached: capture.HelperReturnEax.HasValue,
                EaxAfterAlWrite: null,
                EpilogueReached: false,
                RegistersPopped: false,
                Ret4Reached: false,
                RetStackBytes: null,
                StopAtRawFileOffsetExclusive: helperReturnRaw);
        }

        uint? eaxAfterAlWrite = capture.HelperReturnEax is uint helperReturnEax
            ? (helperReturnEax & 0xFFFFFF00) | 1
            : null;
        return new ReliableReceiveRecycleReturnPlan(
            capture,
            ReliableReceiveRecycleReturnOutcome.ReturnCompleted,
            OldCounterRead: true,
            counterAddress,
            oldCounter,
            CounterIncrementReached: true,
            incrementedCounter,
            GlobalRead: true,
            globalAddress,
            globalValue,
            globalValue,
            PayloadPushed: true,
            capture.PayloadPointerInEdi,
            HelperCallReached: true,
            helperTargetRaw,
            HelperReturnedNormally: true,
            AlWriteReached: true,
            AlValue: 1,
            HelperReturnEaxCaptureUnreached: false,
            eaxAfterAlWrite,
            EpilogueReached: true,
            RegistersPopped: true,
            Ret4Reached: true,
            RetStackBytes,
            StopAtRawFileOffsetExclusive: EndRawFileOffsetExclusive(capture.Route));
    }
}
