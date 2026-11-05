using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveRecycleReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Both pinned raw windows, byte sequences, sample identity, and raw-to-VA mapping are exact",
            ReliableReceiveRecycleReturnProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ReliableReceiveRecycleReturnProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveRecycleReturnProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReliableReceiveRecycleReturnProjection.StartRawFileOffset(
                ReliableReceiveRecycleReturnRoute.HighDelta) == 0x00BE52C7
            && ReliableReceiveRecycleReturnProjection.EndRawFileOffsetExclusive(
                ReliableReceiveRecycleReturnRoute.HighDelta) == 0x00BE52DF
            && ReliableReceiveRecycleReturnProjection.RawFileOffsetToVirtualAddress(0x00BE52C7)
                == 0x00FE5EC7
            && ReliableReceiveRecycleReturnProjection.StartRawFileOffset(
                ReliableReceiveRecycleReturnRoute.ExistingRecord) == 0x00BE5375
            && ReliableReceiveRecycleReturnProjection.EndRawFileOffsetExclusive(
                ReliableReceiveRecycleReturnRoute.ExistingRecord) == 0x00BE538D
            && ReliableReceiveRecycleReturnProjection.RawFileOffsetToVirtualAddress(0x00BE5375)
                == 0x00FE5F75
            && WindowMatches(ReliableReceiveRecycleReturnRoute.HighDelta,
                "FF 40 20 8B 0D 84 FF DF 01 57 E8 8A 09 FF FF B0 01 "
                + "5F 5E 5B 5D C2 04 00")
            && WindowMatches(ReliableReceiveRecycleReturnRoute.ExistingRecord,
                "FF 40 20 8B 0D 84 FF DF 01 57 E8 DC 08 FF FF B0 01 "
                + "5F 5E 5B 5D C2 04 00")
            && ReliableReceiveRecycleReturnProjection.GlobalPointerAddress == 0x01DFFF84
            && ReliableReceiveRecycleReturnProjection.HelperTargetRawFileOffset == 0x00BD5C60
            && ReliableReceiveRecycleReturnProjection.ComputedHelperTargetRawFileOffset(
                ReliableReceiveRecycleReturnRoute.HighDelta) == 0x00BD5C60
            && ReliableReceiveRecycleReturnProjection.ComputedHelperTargetRawFileOffset(
                ReliableReceiveRecycleReturnRoute.ExistingRecord) == 0x00BD5C60
            && EvidenceMatches(ReliableReceiveRecycleReturnRoute.HighDelta)
            && EvidenceMatches(ReliableReceiveRecycleReturnRoute.ExistingRecord));

        check("The counter address and increment use DWORD wrap, and zero global/payload values remain valid",
            ReliableReceiveRecycleReturnProjection.CounterAddress(0xFFFFFFDF) == 0xFFFFFFFF
            && ReliableReceiveRecycleReturnProjection.CounterAddress(0xFFFFFFE0) == 0
            && VerifyNormalReturn(
                ReliableReceiveRecycleReturnRoute.HighDelta,
                receiver: 0xFFFFFFE0,
                payload: 0,
                oldCounter: uint.MaxValue,
                global: 0,
                helperReturnEax: 0xAABBCCDD,
                expectedCounterAddress: 0,
                expectedIncrementedCounter: 0,
                expectedEax: 0xAABBCC01)
            && VerifyNormalReturn(
                ReliableReceiveRecycleReturnRoute.ExistingRecord,
                receiver: 0x1000,
                payload: 0xFEDCBA98,
                oldCounter: 0,
                global: 0x12345678,
                helperReturnEax: 0x010203FE,
                expectedCounterAddress: 0x1020,
                expectedIncrementedCounter: 1,
                expectedEax: 0x01020301));

        ReliableReceiveRecycleReturnPlan missingCounter = Project(
            ReliableReceiveRecycleReturnRoute.HighDelta,
            oldCounter: null,
            global: 0,
            returnedNormally: true,
            returnEax: 0xAABBCCDD);
        ReliableReceiveRecycleReturnPlan unreachedGlobal = Project(
            ReliableReceiveRecycleReturnRoute.ExistingRecord,
            oldCounter: null,
            global: 0);
        ReliableReceiveRecycleReturnPlan missingGlobal = Project(
            ReliableReceiveRecycleReturnRoute.HighDelta,
            oldCounter: 7,
            global: null,
            returnedNormally: true,
            returnEax: 0xAABBCCDD);
        ReliableReceiveRecycleReturnPlan missingReturn =
            Project(ReliableReceiveRecycleReturnRoute.ExistingRecord,
                oldCounter: 7, global: 0x11223344, returnedNormally: null, returnEax: null);
        check("Missing DWORD captures gate subsequent reads, call, and return effects in order",
            missingCounter.Outcome == ReliableReceiveRecycleReturnOutcome.UnreachedLaterCapture
            && !missingCounter.OldCounterRead
            && missingCounter.CounterAddress == ReliableReceiveRecycleReturnProjection.CounterAddress(0x1000)
            && !missingCounter.CounterIncrementReached
            && !missingCounter.GlobalRead
            && missingCounter.HelperReturnEaxCaptureUnreached
            && !missingCounter.HelperCallReached
            && !missingCounter.Ret4Reached
            && missingCounter.StopAtRawFileOffsetExclusive
                == ReliableReceiveRecycleReturnProjection.HighDeltaCounterIncrementRawFileOffset
            && unreachedGlobal.Outcome == ReliableReceiveRecycleReturnOutcome.UnreachedLaterCapture
            && !unreachedGlobal.GlobalRead
            && !unreachedGlobal.HelperCallReached
            && unreachedGlobal.StopAtRawFileOffsetExclusive
                == ReliableReceiveRecycleReturnProjection.ExistingRecordCounterIncrementRawFileOffset
            && missingGlobal.Outcome == ReliableReceiveRecycleReturnOutcome.UnreachedLaterCapture
            && missingGlobal.OldCounterRead
            && missingGlobal.IncrementedCounterValue == 8
            && !missingGlobal.GlobalRead
            && !missingGlobal.PayloadPushed
            && !missingGlobal.HelperCallReached
            && missingGlobal.HelperReturnEaxCaptureUnreached
            && missingGlobal.StopAtRawFileOffsetExclusive
                == ReliableReceiveRecycleReturnProjection.HighDeltaGlobalLoadRawFileOffset
            && missingReturn.Outcome == ReliableReceiveRecycleReturnOutcome.MissingHelperReturnCapture
            && missingReturn.GlobalRead
            && missingReturn.EcX == 0x11223344
            && missingReturn.PayloadPushed
            && missingReturn.HelperCallReached
            && !missingReturn.HelperReturnEaxCaptureUnreached
            && !missingReturn.AlWriteReached
            && !missingReturn.Ret4Reached
            && missingReturn.StopAtRawFileOffsetExclusive
                == ReliableReceiveRecycleReturnProjection.ExistingRecordHelperReturnRawFileOffset);

        ReliableReceiveRecycleReturnPlan normalWithoutEax = Project(
            ReliableReceiveRecycleReturnRoute.HighDelta,
            oldCounter: 0xFFFFFFFF,
            global: 0,
            returnedNormally: true,
            returnEax: null);
        ReliableReceiveRecycleReturnPlan missingNormal = Project(
            ReliableReceiveRecycleReturnRoute.HighDelta,
            oldCounter: 1,
            global: 0,
            returnedNormally: null,
            returnEax: null);
        ReliableReceiveRecycleReturnPlan nonreturning = Project(
            ReliableReceiveRecycleReturnRoute.ExistingRecord,
            oldCounter: 1,
            global: 0,
            returnedNormally: false,
            returnEax: null);
        ReliableReceiveRecycleReturnPlan nonreturningWithEax = Project(
            ReliableReceiveRecycleReturnRoute.ExistingRecord,
            oldCounter: 1,
            global: 0,
            returnedNormally: false,
            returnEax: 0xAABBCCDD);
        check("Normal return gates AL=1, register restoration, and RET 4; EAX capture is optional",
            normalWithoutEax.Outcome == ReliableReceiveRecycleReturnOutcome.ReturnCompleted
            && normalWithoutEax.HelperReturnedNormally == true
            && normalWithoutEax.AlWriteReached
            && normalWithoutEax.AlValue == 1
            && normalWithoutEax.EaxAfterAlWrite is null
            && normalWithoutEax.EpilogueReached
            && normalWithoutEax.RegistersPopped
            && normalWithoutEax.Ret4Reached
            && normalWithoutEax.RetStackBytes == 4
            && normalWithoutEax.StopAtRawFileOffsetExclusive
                == ReliableReceiveRecycleReturnProjection.HighDeltaEndRawFileOffsetExclusive
            && missingNormal.Outcome == ReliableReceiveRecycleReturnOutcome.MissingHelperReturnCapture
            && missingNormal.HelperReturnedNormally is null
            && !missingNormal.AlWriteReached
            && !missingNormal.EpilogueReached
            && !missingNormal.RegistersPopped
            && !missingNormal.Ret4Reached
            && missingNormal.RetStackBytes is null
            && missingNormal.StopAtRawFileOffsetExclusive
                == ReliableReceiveRecycleReturnProjection.HighDeltaHelperReturnRawFileOffset
            && nonreturning.Outcome == ReliableReceiveRecycleReturnOutcome.HelperDidNotReturnNormally
            && nonreturning.HelperReturnedNormally == false
            && !nonreturning.AlWriteReached
            && !nonreturning.Ret4Reached
            && nonreturning.StopAtRawFileOffsetExclusive
                == ReliableReceiveRecycleReturnProjection.ExistingRecordHelperReturnRawFileOffset
            && nonreturningWithEax.Outcome
                == ReliableReceiveRecycleReturnOutcome.UnreachedHelperReturnEaxCapture
            && nonreturningWithEax.HelperReturnEaxCaptureUnreached
            && nonreturningWithEax.EaxAfterAlWrite is null
            && !nonreturningWithEax.Ret4Reached);
    }

    private static bool VerifyNormalReturn(
        ReliableReceiveRecycleReturnRoute route,
        uint receiver,
        uint payload,
        uint oldCounter,
        uint global,
        uint helperReturnEax,
        uint expectedCounterAddress,
        uint expectedIncrementedCounter,
        uint expectedEax)
    {
        ReliableReceiveRecycleReturnPlan plan =
            ReliableReceiveRecycleReturnProjection.Project(new(
                route, receiver, payload, oldCounter, global, true, helperReturnEax));
        return plan.Outcome == ReliableReceiveRecycleReturnOutcome.ReturnCompleted
            && plan.OldCounterRead
            && plan.CounterAddress == expectedCounterAddress
            && plan.OldCounterValue == oldCounter
            && plan.CounterIncrementReached
            && plan.IncrementedCounterValue == expectedIncrementedCounter
            && plan.GlobalRead
            && plan.GlobalAddress == ReliableReceiveRecycleReturnProjection.GlobalPointerAddress
            && plan.GlobalValue == global
            && plan.EcX == global
            && plan.PayloadPushed
            && plan.PushedPayloadPointer == payload
            && plan.HelperCallReached
            && plan.HelperTargetRawFileOffset == 0x00BD5C60
            && plan.AlValue == 1
            && plan.EaxAfterAlWrite == expectedEax
            && plan.EpilogueReached
            && plan.RegistersPopped
            && plan.Ret4Reached
            && plan.RetStackBytes == 4;
    }

    private static ReliableReceiveRecycleReturnPlan Project(
        ReliableReceiveRecycleReturnRoute route,
        uint? oldCounter,
        uint? global,
        bool? returnedNormally = null,
        uint? returnEax = null) =>
        ReliableReceiveRecycleReturnProjection.Project(new(
            route,
            ReceiverPointerInEax: 0x1000,
            PayloadPointerInEdi: 0xCAFEBABE,
            OldCounterDword: oldCounter,
            GlobalDword: global,
            HelperReturnedNormally: returnedNormally,
            HelperReturnEax: returnEax));

    private static bool WindowMatches(
        ReliableReceiveRecycleReturnRoute route,
        string expected) =>
        ReliableReceiveRecycleReturnProjection.CapturedWindowBytes(route) == expected
        && expected.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x18;

    private static bool EvidenceMatches(ReliableReceiveRecycleReturnRoute route)
    {
        var (baseRaw, baseVirtual, expectedBytes) = route switch
        {
            ReliableReceiveRecycleReturnRoute.HighDelta =>
                (0x00BE52C7u, 0x00FE5EC7u, HighDeltaExpectedEvidence),
            ReliableReceiveRecycleReturnRoute.ExistingRecord =>
                (0x00BE5375u, 0x00FE5F75u, ExistingRecordExpectedEvidence),
            _ => throw new ArgumentOutOfRangeException(nameof(route))
        };
        ImmutableArray<ReliableReceiveRecycleReturnInstructionEvidence> evidence =
            ReliableReceiveRecycleReturnProjection.Instructions(route);
        return evidence.All(instruction =>
                instruction.VirtualAddress
                    == ReliableReceiveRecycleReturnProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            && evidence.Select(instruction => instruction.Bytes)
                .SequenceEqual(expectedBytes)
            && evidence[0].RawFileOffset == baseRaw
            && evidence[0].VirtualAddress == baseVirtual
            && ReliableReceiveRecycleReturnProjection.RawFileOffsetToVirtualAddress(
                ReliableReceiveRecycleReturnProjection.EndRawFileOffsetExclusive(route))
                == ReliableReceiveRecycleReturnProjection.EndRawFileOffsetExclusive(route) + 0x00400C00;
    }

    private static readonly string[] HighDeltaExpectedEvidence =
    {
        "FF 40 20", "8B 0D 84 FF DF 01", "57", "E8 8A 09 FF FF",
        "B0 01", "5F 5E 5B 5D", "C2 04 00"
    };

    private static readonly string[] ExistingRecordExpectedEvidence =
    {
        "FF 40 20", "8B 0D 84 FF DF 01", "57", "E8 DC 08 FF FF",
        "B0 01", "5F 5E 5B 5D", "C2 04 00"
    };
}
