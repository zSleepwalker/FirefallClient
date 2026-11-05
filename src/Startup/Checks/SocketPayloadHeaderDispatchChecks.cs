using Firefall.Reconstruction.Startup;

internal static class SocketPayloadHeaderDispatchChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Header dispatch anchors pin the PE sample, raw-offset mapping, jump table, and call targets",
            SocketPayloadHeaderDispatchProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && SocketPayloadHeaderDispatchProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x00400C00
            && SocketPayloadHeaderDispatchProjection.HeaderDispatchRawFileOffset == 0x00BD412B
            && SocketPayloadHeaderDispatchProjection.HeaderDispatchVirtualAddress == 0x00FD4D2B
            && SocketPayloadHeaderDispatchProjection.JumpTableVirtualAddress == 0x00FD4E2C
            && SocketPayloadHeaderDispatchProjection.JumpTableRawFileOffset == 0x00BD422C
            && SocketPayloadHeaderDispatchProjection.JumpTableRawVirtualAddress == 0x00FD4E2C
            && new[]
            {
                SocketPayloadHeaderDispatchProjection.Case0RawFileOffset,
                SocketPayloadHeaderDispatchProjection.Case1RawFileOffset,
                SocketPayloadHeaderDispatchProjection.Case2RawFileOffset,
                SocketPayloadHeaderDispatchProjection.Case3RawFileOffset
            }.SequenceEqual(new uint[] { 0x00BD4141, 0x00BD415A, 0x00BD418D, 0x00BD41C0 })
            && new[]
            {
                SocketPayloadHeaderDispatchProjection.Case0CallTargetRawFileOffset,
                SocketPayloadHeaderDispatchProjection.Case1CallTargetRawFileOffset,
                SocketPayloadHeaderDispatchProjection.Case2CallTargetRawFileOffset,
                SocketPayloadHeaderDispatchProjection.Case3CallTargetRawFileOffset
            }.SequenceEqual(new uint[] { 0x00BD3760, 0x00BD5BA0, 0x00BD5BA0, 0x00BD5C00 }));

        check("Instruction evidence pins the header load, unsigned switch, table slots, and first opaque calls",
            SocketPayloadHeaderDispatchProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.BranchTargetRawFileOffset))
                .SequenceEqual(new (uint RawFileOffset, string Bytes, uint? BranchTargetRawFileOffset)[]
                {
                    (0x00BD412B, "0F B7 07", null),
                    (0x00BD412E, "C1 E8 0E", null),
                    (0x00BD4131, "83 F8 03", null),
                    (0x00BD4134, "0F 87 C0 00 00 00", 0x00BD41FA),
                    (0x00BD413A, "FF 24 85 2C 4E FD 00", null),
                    (0x00BD422C, "41 4D FD 00 5A 4D FD 00 8D 4D FD 00 C0 4D FD 00", null),
                    (0x00BD4141, "57 8B CE E8 17 F6 FF FF", 0x00BD3760),
                    (0x00BD415A, "57 8D 8E 6C 0D 00 00 E8 3A 1A 00 00", 0x00BD5BA0),
                    (0x00BD418D, "57 8D 8E 70 0D 00 00 E8 07 1A 00 00", 0x00BD5BA0),
                    (0x00BD41C0, "57 8D 8E 74 0D 00 00 E8 34 1A 00 00", 0x00BD5C00)
                })
            && unchecked((int)0x00BD413A + 0xC0) == 0x00BD41FA
            && unchecked((int)0x00BD4149 + (int)0xFFFFF617) == 0x00BD3760
            && unchecked((int)0x00BD4166 + 0x1A3A) == 0x00BD5BA0
            && unchecked((int)0x00BD4199 + 0x1A07) == 0x00BD5BA0
            && unchecked((int)0x00BD41CC + 0x1A34) == 0x00BD5C00);

        SocketPayloadHeaderDispatchPlan missingHeader =
            SocketPayloadHeaderDispatchProjection.Project(
                new SocketPayloadHeaderDispatchCapture(
                    HeaderPointer: 0x12345678,
                    ContextPointer: 0x87654321,
                    HeaderWord: null));
        check("A missing header word remains visible at the [EDI] read boundary",
            missingHeader.Outcome == SocketPayloadHeaderDispatchOutcome.MissingHeaderWord
            && missingHeader.HeaderWordAddress == 0x12345678
            && missingHeader.Route is null
            && missingHeader.EcxValueAtOpaqueCall is null
            && missingHeader.OpaqueCallInstructionRawFileOffset is null
            && missingHeader.StopAtRawFileOffsetExclusive == 0x00BD412B);

        bool allWordsRouteToTheirObservedFirstOpaqueCall = true;
        for (uint word = 0; word <= ushort.MaxValue; word++)
        {
            SocketPayloadHeaderDispatchRoute route =
                (SocketPayloadHeaderDispatchRoute)(word >> 14);
            (uint expectedDisplacement, uint expectedCase, uint expectedCall, uint expectedTarget) =
                route switch
                {
                    SocketPayloadHeaderDispatchRoute.Case0 =>
                        (0u, 0x00BD4141u, 0x00BD4144u, 0x00BD3760u),
                    SocketPayloadHeaderDispatchRoute.Case1 =>
                        (0xD6Cu, 0x00BD415Au, 0x00BD4161u, 0x00BD5BA0u),
                    SocketPayloadHeaderDispatchRoute.Case2 =>
                        (0xD70u, 0x00BD418Du, 0x00BD4194u, 0x00BD5BA0u),
                    _ =>
                        (0xD74u, 0x00BD41C0u, 0x00BD41C7u, 0x00BD5C00u)
                };
            SocketPayloadHeaderDispatchPlan plan =
                SocketPayloadHeaderDispatchProjection.Project(
                    new SocketPayloadHeaderDispatchCapture(
                        HeaderPointer: 0x10203040,
                        ContextPointer: 0x20304050,
                        HeaderWord: (ushort)word));

            allWordsRouteToTheirObservedFirstOpaqueCall &=
                plan.Route == route
                && plan.CaseRawFileOffset == expectedCase
                && plan.EdiValuePushed == 0x10203040
                && plan.EcxValueAtOpaqueCall
                    == unchecked(0x20304050u + expectedDisplacement)
                && plan.OpaqueCallInstructionRawFileOffset == expectedCall
                && plan.OpaqueCallTargetRawFileOffset == expectedTarget
                && plan.OpaqueCallTargetVirtualAddress
                    == unchecked(expectedTarget + 0x00400C00)
                && plan.StopAtRawFileOffsetExclusive == expectedCall;
        }

        check("All 65,536 header words select the correct route and stop before its first opaque call",
            allWordsRouteToTheirObservedFirstOpaqueCall);

        SocketPayloadHeaderDispatchPlan wrapped =
            SocketPayloadHeaderDispatchProjection.Project(
                new SocketPayloadHeaderDispatchCapture(
                    HeaderPointer: uint.MaxValue,
                    ContextPointer: uint.MaxValue - 0x10,
                    HeaderWord: 0xC000));
        check("Header and case context addresses retain DWORD wrapping",
            wrapped.HeaderWordAddress == uint.MaxValue
            && wrapped.EdiValuePushed == uint.MaxValue
            && wrapped.EcxValueAtOpaqueCall == 0xD63
            && wrapped.OpaqueCallTargetVirtualAddress == 0x00FD6800);
    }
}
