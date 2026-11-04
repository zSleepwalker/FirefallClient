using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadWordGrowthDoublingCapture(
    uint ReceiverPointer,
    uint? ReceiverDword8);

public enum PayloadWordGrowthDoublingOutcome
{
    MissingReceiverDword8Capture,
    BranchesToReceiverEightBoundary,
    BranchesToDoublingGuardTarget,
    NonterminatingRepeatedEsi
}

public readonly record struct PayloadWordGrowthDoublingIterationState(
    int Iteration,
    uint EsiBefore,
    uint EsiMinusReceiver,
    bool EsiMinusReceiverBelowOneBranchTaken,
    bool ReceiverEightCompareEvaluated,
    bool ReceiverEightBranchTaken,
    uint? GuardSubtractResult,
    bool? GuardUnsignedBranchTaken,
    uint? EsiAfterDoubling,
    bool CycleDetected,
    uint? BranchRouteRawFileOffset);

public readonly record struct PayloadWordGrowthDoublingPlan(
    PayloadWordGrowthDoublingCapture Capture,
    PayloadWordGrowthDoublingOutcome Outcome,
    uint ReceiverDword8ReadAddress,
    bool ReceiverDword8Read,
    uint? InitialEsi,
    ReadOnlyCollection<PayloadWordGrowthDoublingIterationState> Iterations,
    uint? BranchRouteRawFileOffset);

public readonly record struct PayloadWordGrowthDoublingInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the receiver DWORD read and the exact wrapping growth loop at
/// raw 0xBE68B3 through, but not including, raw 0xBE68E1.
/// </summary>
public static class PayloadWordGrowthDoublingProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint LoopStartRawFileOffset = 0x00BE68B3;
    public const uint LoopEndRawFileOffsetExclusive = 0x00BE68E1;
    public const uint LoopStartVirtualAddress = 0x00FE74B3;
    public const uint ReceiverDword8Offset = 0x08;
    public const uint InitialEsiValue = 1;
    public const uint ReceiverEightBoundary = 8;
    public const uint DoublingGuardSeed = 0x0FFFFFFF;
    public const uint ReceiverEightBoundaryRawFileOffset = 0x00BE68E1;
    public const uint DoublingGuardBranchTargetRawFileOffset = 0x00BE6A10;
    public const uint LoopBackEdgeRawFileOffset = 0x00BE68DF;
    public const uint LoopBackEdgeTargetRawFileOffset = 0x00BE68C0;
    public const uint CapturedCodeLength =
        LoopEndRawFileOffsetExclusive - LoopStartRawFileOffset;

    public const string CapturedLoopBytes =
        "8B 53 08 85 D2 0F 45 F2 EB 03 8D 49 00 8B C6 2B C2 83 F8 01 "
        + "72 05 83 FE 08 73 13 B8 FF FF FF 0F 2B C6 3B C6 "
        + "0F 82 33 01 00 00 03 F6 EB DF";

    public static uint LoopStartVirtualAddressFromRawOffset =>
        RawFileOffsetToVirtualAddress(LoopStartRawFileOffset);

    public static ReadOnlyCollection<PayloadWordGrowthDoublingInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Read the receiver DWORD at +8 into EDX",
                0x00BE68B3,
                0x00FE74B3,
                "8B 53 08"),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Test the captured receiver DWORD",
                0x00BE68B6,
                0x00FE74B6,
                "85 D2"),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Use the captured nonzero DWORD as ESI; otherwise retain initial ESI=1",
                0x00BE68B8,
                0x00FE74B8,
                "0F 45 F2"),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Jump over the three-byte LEA to the loop at raw 0xBE68C0",
                0x00BE68BB,
                0x00FE74BB,
                "EB 03",
                0x00BE68C0,
                0x00FE74C0),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Skipped LEA ECX,[ECX]",
                0x00BE68BD,
                0x00FE74BD,
                "8D 49 00"),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Compute wrapping ESI minus the captured receiver DWORD",
                0x00BE68C0,
                0x00FE74C0,
                "8B C6 2B C2"),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Unsigned JB branches to the growth guard when the difference is below one",
                0x00BE68C4,
                0x00FE74C4,
                "83 F8 01 72 05",
                0x00BE68CE,
                0x00FE74CE),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Compare ESI with eight and branch to the exclusive boundary when ESI is at least eight",
                0x00BE68C9,
                0x00FE74C9,
                "83 FE 08 73 13",
                ReceiverEightBoundaryRawFileOffset,
                RawFileOffsetToVirtualAddress(ReceiverEightBoundaryRawFileOffset)),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Load the 0x0FFFFFFF guard seed",
                0x00BE68CE,
                0x00FE74CE,
                "B8 FF FF FF 0F"),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Subtract ESI from the guard seed with wrapping DWORD arithmetic",
                0x00BE68D3,
                0x00FE74D3,
                "2B C6"),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Compare the wrapped guard result against ESI",
                0x00BE68D5,
                0x00FE74D5,
                "3B C6"),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Unsigned JB branches to raw 0xBE6A10",
                0x00BE68D7,
                0x00FE74D7,
                "0F 82 33 01 00 00",
                DoublingGuardBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(DoublingGuardBranchTargetRawFileOffset)),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Double ESI with wrapping DWORD arithmetic",
                0x00BE68DD,
                0x00FE74DD,
                "03 F6"),
            new PayloadWordGrowthDoublingInstructionEvidence(
                "Repeat the loop from raw 0xBE68C0",
                LoopBackEdgeRawFileOffset,
                RawFileOffsetToVirtualAddress(LoopBackEdgeRawFileOffset),
                "EB DF",
                LoopBackEdgeTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(LoopBackEdgeTargetRawFileOffset))
        });

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverDwordAddress(uint receiverPointer, uint offset) =>
        unchecked(receiverPointer + offset);

    public static PayloadWordGrowthDoublingPlan Project(
        PayloadWordGrowthDoublingCapture capture)
    {
        uint receiverDword8ReadAddress = ReceiverDwordAddress(
            capture.ReceiverPointer,
            ReceiverDword8Offset);
        if (capture.ReceiverDword8 is not uint receiverDword8)
        {
            return new(
                capture,
                PayloadWordGrowthDoublingOutcome.MissingReceiverDword8Capture,
                receiverDword8ReadAddress,
                false,
                null,
                Array.AsReadOnly(Array.Empty<PayloadWordGrowthDoublingIterationState>()),
                null);
        }

        uint initialEsi = receiverDword8 == 0
            ? InitialEsiValue
            : receiverDword8;
        var iterations = new List<PayloadWordGrowthDoublingIterationState>();
        var seenEsiValues = new HashSet<uint> { initialEsi };
        uint esi = initialEsi;

        while (true)
        {
            uint esiMinusReceiver = unchecked(esi - receiverDword8);
            bool esiMinusReceiverBelowOne = esiMinusReceiver < 1;
            bool receiverEightCompareEvaluated = !esiMinusReceiverBelowOne;
            bool receiverEightBranchTaken =
                receiverEightCompareEvaluated && esi >= ReceiverEightBoundary;

            if (receiverEightBranchTaken)
            {
                iterations.Add(new(
                    iterations.Count,
                    esi,
                    esiMinusReceiver,
                    esiMinusReceiverBelowOne,
                    receiverEightCompareEvaluated,
                    true,
                    null,
                    null,
                    null,
                    false,
                    ReceiverEightBoundaryRawFileOffset));
                return Completed(
                    capture,
                    PayloadWordGrowthDoublingOutcome.BranchesToReceiverEightBoundary,
                    receiverDword8ReadAddress,
                    initialEsi,
                    iterations,
                    ReceiverEightBoundaryRawFileOffset);
            }

            uint guardSubtractResult = unchecked(DoublingGuardSeed - esi);
            bool guardUnsignedBranchTaken = guardSubtractResult < esi;
            if (guardUnsignedBranchTaken)
            {
                iterations.Add(new(
                    iterations.Count,
                    esi,
                    esiMinusReceiver,
                    esiMinusReceiverBelowOne,
                    receiverEightCompareEvaluated,
                    false,
                    guardSubtractResult,
                    true,
                    null,
                    false,
                    DoublingGuardBranchTargetRawFileOffset));
                return Completed(
                    capture,
                    PayloadWordGrowthDoublingOutcome.BranchesToDoublingGuardTarget,
                    receiverDword8ReadAddress,
                    initialEsi,
                    iterations,
                    DoublingGuardBranchTargetRawFileOffset);
            }

            uint doubledEsi = unchecked(esi + esi);
            bool cycleDetected = seenEsiValues.Contains(doubledEsi);
            iterations.Add(new(
                iterations.Count,
                esi,
                esiMinusReceiver,
                esiMinusReceiverBelowOne,
                receiverEightCompareEvaluated,
                false,
                guardSubtractResult,
                false,
                doubledEsi,
                cycleDetected,
                null));

            if (cycleDetected)
            {
                return Completed(
                    capture,
                    PayloadWordGrowthDoublingOutcome.NonterminatingRepeatedEsi,
                    receiverDword8ReadAddress,
                    initialEsi,
                    iterations,
                    null);
            }

            seenEsiValues.Add(doubledEsi);
            esi = doubledEsi;
        }
    }

    private static PayloadWordGrowthDoublingPlan Completed(
        PayloadWordGrowthDoublingCapture capture,
        PayloadWordGrowthDoublingOutcome outcome,
        uint receiverDword8ReadAddress,
        uint initialEsi,
        List<PayloadWordGrowthDoublingIterationState> iterations,
        uint? branchRouteRawFileOffset) =>
        new(
            capture,
            outcome,
            receiverDword8ReadAddress,
            true,
            initialEsi,
            Array.AsReadOnly(iterations.ToArray()),
            branchRouteRawFileOffset);
}
