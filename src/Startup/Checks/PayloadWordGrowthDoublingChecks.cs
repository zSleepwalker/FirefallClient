using Firefall.Reconstruction.Startup;

internal static class PayloadWordGrowthDoublingChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE sample identity, raw range, VA mapping, bytes, and branch destinations are pinned",
            PayloadWordGrowthDoublingProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadWordGrowthDoublingProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadWordGrowthDoublingProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PayloadWordGrowthDoublingProjection.LoopStartRawFileOffset == 0x00BE68B3
            && PayloadWordGrowthDoublingProjection.LoopEndRawFileOffsetExclusive
                == 0x00BE68E1
            && PayloadWordGrowthDoublingProjection.CapturedCodeLength == 46
            && PayloadWordGrowthDoublingProjection.LoopStartVirtualAddress == 0x00FE74B3
            && PayloadWordGrowthDoublingProjection.LoopStartVirtualAddressFromRawOffset
                == 0x00FE74B3
            && PayloadWordGrowthDoublingProjection.RawFileOffsetToVirtualAddress(0x00BE68E1)
                == 0x00FE74E1
            && PayloadWordGrowthDoublingProjection.ReceiverDword8Offset == 8
            && PayloadWordGrowthDoublingProjection.DoublingGuardSeed == 0x0FFFFFFF
            && PayloadWordGrowthDoublingProjection.ReceiverEightBoundaryRawFileOffset
                == 0x00BE68E1
            && PayloadWordGrowthDoublingProjection.DoublingGuardBranchTargetRawFileOffset
                == 0x00BE6A10
            && PayloadWordGrowthDoublingProjection.LoopBackEdgeTargetRawFileOffset
                == 0x00BE68C0
            && string.Concat(PayloadWordGrowthDoublingProjection.CapturedLoopBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B530885D20F45F2EB038D49008BC62BC283F801720583FE087313B8FFFFFF0F2BC63BC6"
                    + "0F823301000003F6EBDF"
            && PayloadWordGrowthDoublingProjection.Instructions.SequenceEqual(
                ExpectedInstructions()));

        check("The receiver DWORD read uses a wrapping receiver+8 address and missing capture stops before the loop",
            PayloadWordGrowthDoublingProjection.ReceiverDwordAddress(uint.MaxValue, 8)
                == 7
            && VerifyMissingRead());

        (uint value, PayloadWordGrowthDoublingOutcome outcome, uint? route, int stateCount)[] cases =
        [
            (0, PayloadWordGrowthDoublingOutcome.BranchesToReceiverEightBoundary, 0x00BE68E1, 4),
            (1, PayloadWordGrowthDoublingOutcome.BranchesToReceiverEightBoundary, 0x00BE68E1, 4),
            (7, PayloadWordGrowthDoublingOutcome.BranchesToReceiverEightBoundary, 0x00BE68E1, 2),
            (8, PayloadWordGrowthDoublingOutcome.BranchesToReceiverEightBoundary, 0x00BE68E1, 2),
            (0x0FFFFFFF, PayloadWordGrowthDoublingOutcome.BranchesToDoublingGuardTarget, 0x00BE6A10, 1),
            (0x10000000, PayloadWordGrowthDoublingOutcome.BranchesToReceiverEightBoundary, 0x00BE68E1, 2),
            (0x7FFFFFFF, PayloadWordGrowthDoublingOutcome.BranchesToReceiverEightBoundary, 0x00BE68E1, 2),
            (0x80000000, PayloadWordGrowthDoublingOutcome.NonterminatingRepeatedEsi, null, 2),
            (0xFFFFFFFF, PayloadWordGrowthDoublingOutcome.BranchesToDoublingGuardTarget, 0x00BE6A10, 1)
        ];

        check("Captured DWORD boundary cases retain the exact guard, unsigned branches, wrap, and repeated-zero cycle",
            cases.All(testCase =>
            {
                PayloadWordGrowthDoublingPlan plan =
                    PayloadWordGrowthDoublingProjection.Project(
                        new(0x2000, testCase.value));
                return plan.Outcome == testCase.outcome
                    && plan.BranchRouteRawFileOffset == testCase.route
                    && plan.Iterations.Count == testCase.stateCount
                    && MatchesIndependentOracle(testCase.value, plan)
                    && (testCase.value != 0x80000000
                        || plan.Iterations[0].EsiAfterDoubling == 0
                            && plan.Iterations[1].EsiBefore == 0
                            && plan.Iterations[1].EsiAfterDoubling == 0
                            && plan.Iterations[1].CycleDetected
                            && plan.BranchRouteRawFileOffset is null);
            }));

        check("Every 32-bit power of two and its in-range neighbors match the independent bounded oracle",
            VerifyPowersOfTwoAndNeighbors());
    }

    private static bool VerifyMissingRead()
    {
        PayloadWordGrowthDoublingPlan missing =
            PayloadWordGrowthDoublingProjection.Project(new(0xFFFFFFFC, null));
        return missing.Outcome
                == PayloadWordGrowthDoublingOutcome.MissingReceiverDword8Capture
            && missing.ReceiverDword8ReadAddress == 4
            && !missing.ReceiverDword8Read
            && missing.InitialEsi is null
            && missing.Iterations.Count == 0
            && missing.BranchRouteRawFileOffset is null;
    }

    private static bool VerifyPowersOfTwoAndNeighbors()
    {
        var values = new HashSet<uint>();
        for (int bit = 0; bit < 32; bit++)
        {
            uint power = 1u << bit;
            values.Add(power);
            if (power > 0)
            {
                values.Add(power - 1);
            }

            if (power < uint.MaxValue)
            {
                values.Add(power + 1);
            }
        }

        return values.Count == 93
            && values.All(value =>
            {
                PayloadWordGrowthDoublingPlan plan =
                    PayloadWordGrowthDoublingProjection.Project(
                        new(0x3000, value));
                return plan.Iterations.Count <= 34
                    && MatchesIndependentOracle(value, plan);
            });
    }

    private static bool MatchesIndependentOracle(
        uint receiverDword8,
        PayloadWordGrowthDoublingPlan plan)
    {
        OracleResult expected = RunIndependentOracle(receiverDword8);
        return plan.InitialEsi == expected.InitialEsi
            && plan.Outcome == expected.Outcome
            && plan.BranchRouteRawFileOffset == expected.BranchRouteRawFileOffset
            && plan.Iterations.SequenceEqual(expected.Iterations);
    }

    private static OracleResult RunIndependentOracle(uint receiverDword8)
    {
        const ulong DwordModulus = 0x1_0000_0000UL;
        const ulong DwordMask = DwordModulus - 1;
        uint initialEsi = receiverDword8 == 0 ? 1u : receiverDword8;
        uint esi = initialEsi;
        var seen = new HashSet<uint> { esi };
        var states = new List<PayloadWordGrowthDoublingIterationState>();

        for (int iteration = 0; iteration < 34; iteration++)
        {
            ulong difference =
                ((ulong)esi + DwordModulus - receiverDword8) & DwordMask;
            bool differenceBelowOne = difference < 1;
            bool receiverEightCompareEvaluated = !differenceBelowOne;
            bool receiverEightBranchTaken =
                receiverEightCompareEvaluated && esi >= 8;

            if (receiverEightBranchTaken)
            {
                states.Add(new(
                    iteration,
                    esi,
                    (uint)difference,
                    differenceBelowOne,
                    receiverEightCompareEvaluated,
                    true,
                    null,
                    null,
                    null,
                    false,
                    PayloadWordGrowthDoublingProjection.ReceiverEightBoundaryRawFileOffset));
                return new(
                    initialEsi,
                    PayloadWordGrowthDoublingOutcome.BranchesToReceiverEightBoundary,
                    PayloadWordGrowthDoublingProjection.ReceiverEightBoundaryRawFileOffset,
                    states.ToArray());
            }

            ulong guardSubtractResult =
                ((ulong)PayloadWordGrowthDoublingProjection.DoublingGuardSeed
                    + DwordModulus - esi) & DwordMask;
            bool guardBranchTaken = guardSubtractResult < esi;
            if (guardBranchTaken)
            {
                states.Add(new(
                    iteration,
                    esi,
                    (uint)difference,
                    differenceBelowOne,
                    receiverEightCompareEvaluated,
                    false,
                    (uint)guardSubtractResult,
                    true,
                    null,
                    false,
                    PayloadWordGrowthDoublingProjection.DoublingGuardBranchTargetRawFileOffset));
                return new(
                    initialEsi,
                    PayloadWordGrowthDoublingOutcome.BranchesToDoublingGuardTarget,
                    PayloadWordGrowthDoublingProjection.DoublingGuardBranchTargetRawFileOffset,
                    states.ToArray());
            }

            uint doubledEsi = (uint)(((ulong)esi * 2) & DwordMask);
            bool cycleDetected = !seen.Add(doubledEsi);
            states.Add(new(
                iteration,
                esi,
                (uint)difference,
                differenceBelowOne,
                receiverEightCompareEvaluated,
                false,
                (uint)guardSubtractResult,
                false,
                doubledEsi,
                cycleDetected,
                null));
            if (cycleDetected)
            {
                return new(
                    initialEsi,
                    PayloadWordGrowthDoublingOutcome.NonterminatingRepeatedEsi,
                    null,
                    states.ToArray());
            }

            esi = doubledEsi;
        }

        throw new InvalidOperationException(
            "The reference model exceeded its 34-state DWORD-doubling bound.");
    }

    private static PayloadWordGrowthDoublingInstructionEvidence[] ExpectedInstructions() =>
    [
        new("Read the receiver DWORD at +8 into EDX", 0x00BE68B3, 0x00FE74B3, "8B 53 08"),
        new("Test the captured receiver DWORD", 0x00BE68B6, 0x00FE74B6, "85 D2"),
        new("Use the captured nonzero DWORD as ESI; otherwise retain initial ESI=1", 0x00BE68B8, 0x00FE74B8, "0F 45 F2"),
        new("Jump over the three-byte LEA to the loop at raw 0xBE68C0", 0x00BE68BB, 0x00FE74BB, "EB 03", 0x00BE68C0, 0x00FE74C0),
        new("Skipped LEA ECX,[ECX]", 0x00BE68BD, 0x00FE74BD, "8D 49 00"),
        new("Compute wrapping ESI minus the captured receiver DWORD", 0x00BE68C0, 0x00FE74C0, "8B C6 2B C2"),
        new("Unsigned JB branches to the growth guard when the difference is below one", 0x00BE68C4, 0x00FE74C4, "83 F8 01 72 05", 0x00BE68CE, 0x00FE74CE),
        new("Compare ESI with eight and branch to the exclusive boundary when ESI is at least eight", 0x00BE68C9, 0x00FE74C9, "83 FE 08 73 13", 0x00BE68E1, 0x00FE74E1),
        new("Load the 0x0FFFFFFF guard seed", 0x00BE68CE, 0x00FE74CE, "B8 FF FF FF 0F"),
        new("Subtract ESI from the guard seed with wrapping DWORD arithmetic", 0x00BE68D3, 0x00FE74D3, "2B C6"),
        new("Compare the wrapped guard result against ESI", 0x00BE68D5, 0x00FE74D5, "3B C6"),
        new("Unsigned JB branches to raw 0xBE6A10", 0x00BE68D7, 0x00FE74D7, "0F 82 33 01 00 00", 0x00BE6A10, 0x00FE7610),
        new("Double ESI with wrapping DWORD arithmetic", 0x00BE68DD, 0x00FE74DD, "03 F6"),
        new("Repeat the loop from raw 0xBE68C0", 0x00BE68DF, 0x00FE74DF, "EB DF", 0x00BE68C0, 0x00FE74C0)
    ];

    private readonly record struct OracleResult(
        uint InitialEsi,
        PayloadWordGrowthDoublingOutcome Outcome,
        uint? BranchRouteRawFileOffset,
        PayloadWordGrowthDoublingIterationState[] Iterations);
}
