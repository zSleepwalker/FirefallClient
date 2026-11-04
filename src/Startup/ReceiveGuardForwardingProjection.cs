using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct ReceiveGuardForwardingCapture(
    uint FramePointer,
    uint Argument0,
    uint Argument1,
    uint? ThresholdDword,
    uint? Argument2,
    bool? OpaqueCallReturnedNormally);

public enum ReceiveGuardForwardingOutcome
{
    UnreachedThresholdCapture,
    MissingThresholdCapture,
    UnreachedHandoffCapture,
    MissingArgument2Capture,
    MissingOpaqueCallReturnCapture,
    OpaqueCallDidNotReturn,
    EarlySignedLessReturn,
    NormalEpilogue
}

public readonly record struct ReceiveGuardForwardingPlan(
    ReceiveGuardForwardingCapture Capture,
    ReceiveGuardForwardingOutcome Outcome,
    bool ThresholdRead,
    uint? ThresholdAddress,
    bool? SignedLessComparison,
    bool HandoffSelected,
    bool TailPointerPushed,
    uint? TailPointerValue,
    bool Argument2Pushed,
    uint? ForwardedArgument2,
    uint StackArgumentBytesPushed,
    uint StackArgumentBytesCleaned,
    bool? EbpPopped,
    bool? FunctionReturned,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReceiveGuardForwardingInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public static class ReceiveGuardForwardingProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint GateStartRawFileOffset = 0x00BE2F50;
    public const uint GateStartVirtualAddress = 0x00FE3B50;
    public const uint GateEndRawFileOffsetExclusive = 0x00BE2F73;
    public const uint Argument0StackOffset = 0x08;
    public const uint Argument1StackOffset = 0x0C;
    public const uint Argument2StackOffset = 0x10;
    public const uint Argument3TailStackOffset = 0x14;

    public const uint Argument0LoadRawFileOffset = 0x00BE2F53;
    public const uint Argument1LoadRawFileOffset = 0x00BE2F56;
    public const uint NullTestRawFileOffset = 0x00BE2F59;
    public const uint NullBranchRawFileOffset = 0x00BE2F5B;
    public const uint NullBranchDestinationRawFileOffset = 0x00BE2F62;
    public const uint ThresholdCompareRawFileOffset = 0x00BE2F5D;
    public const uint SignedLessBranchRawFileOffset = 0x00BE2F60;
    public const uint EarlyReturnRawFileOffset = 0x00BE2F71;
    public const uint HandoffRawFileOffset = 0x00BE2F62;
    public const uint TailPointerPushRawFileOffset = 0x00BE2F65;
    public const uint Argument2PushRawFileOffset = 0x00BE2F66;
    public const uint OpaqueCallRawFileOffset = 0x00BE2F69;
    public const uint OpaqueCallReturnRawFileOffset = 0x00BE2F6E;
    public const uint StackCleanupRawFileOffset = 0x00BE2F6E;
    public const uint EpilogueReturnRawFileOffset = 0x00BE2F72;
    public const uint OpaqueCallTargetRawFileOffset = 0x00BE2F80;
    public const uint StackArgumentBytes = 8;
    public const uint RetPopBytes = 0;

    public const string CapturedGateBytes =
        "55 8B EC 8B 4D 08 8B 55 0C 85 C9 74 05 3B 51 04 "
        + "7C 0F 8D 45 14 50 FF 75 10 E8 12 00 00 00 83 C4 08 5D C3";

    public static ImmutableArray<ReceiveGuardForwardingInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new ReceiveGuardForwardingInstructionEvidence(
                "Set up the frame and load argument 0 and argument 1",
                GateStartRawFileOffset,
                RawFileOffsetToVirtualAddress(GateStartRawFileOffset),
                "55 8B EC 8B 4D 08 8B 55 0C"),
            new ReceiveGuardForwardingInstructionEvidence(
                "Test argument 0; JZ skips the compare and selects the handoff",
                NullTestRawFileOffset,
                RawFileOffsetToVirtualAddress(NullTestRawFileOffset),
                "85 C9 74 05",
                NullBranchDestinationRawFileOffset,
                RawFileOffsetToVirtualAddress(NullBranchDestinationRawFileOffset)),
            new ReceiveGuardForwardingInstructionEvidence(
                "Compare argument 1 with the DWORD at argument0+4, then signed JL returns early",
                ThresholdCompareRawFileOffset,
                RawFileOffsetToVirtualAddress(ThresholdCompareRawFileOffset),
                "3B 51 04 7C 0F",
                EarlyReturnRawFileOffset,
                RawFileOffsetToVirtualAddress(EarlyReturnRawFileOffset)),
            new ReceiveGuardForwardingInstructionEvidence(
                "Form and push the incoming argument-3 tail pointer, then push argument 2",
                HandoffRawFileOffset,
                RawFileOffsetToVirtualAddress(HandoffRawFileOffset),
                "8D 45 14 50 FF 75 10"),
            new ReceiveGuardForwardingInstructionEvidence(
                "Call raw 0xBE2F80; normal return continues through caller cleanup and RET",
                OpaqueCallRawFileOffset,
                RawFileOffsetToVirtualAddress(OpaqueCallRawFileOffset),
                "E8 12 00 00 00 83 C4 08 5D C3",
                OpaqueCallTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(OpaqueCallTargetRawFileOffset)),
            new ReceiveGuardForwardingInstructionEvidence(
                "The signed-less early path only pops EBP and executes plain RET",
                EarlyReturnRawFileOffset,
                RawFileOffsetToVirtualAddress(EarlyReturnRawFileOffset),
                "5D C3"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ThresholdAddress(uint argument0) =>
        unchecked(argument0 + sizeof(uint));

    public static uint Argument3TailPointer(uint framePointer) =>
        unchecked(framePointer + Argument3TailStackOffset);

    public static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);

    public static uint RelativeCallTarget(uint instructionRawFileOffset, int displacement) =>
        unchecked(instructionRawFileOffset + 5 + (uint)displacement);

    public static ReceiveGuardForwardingPlan Project(ReceiveGuardForwardingCapture capture)
    {
        if (capture.Argument0 == 0)
        {
            if (capture.ThresholdDword.HasValue)
                return Stop(
                    capture,
                    ReceiveGuardForwardingOutcome.UnreachedThresholdCapture,
                    ThresholdCompareRawFileOffset);

            return ProjectHandoff(capture);
        }

        if (capture.ThresholdDword is not uint threshold)
            return Stop(
                capture,
                HasHandoffCapture(capture)
                    ? ReceiveGuardForwardingOutcome.UnreachedHandoffCapture
                    : ReceiveGuardForwardingOutcome.MissingThresholdCapture,
                ThresholdCompareRawFileOffset,
                thresholdAddress: ThresholdAddress(capture.Argument0));

        bool signedLess = unchecked((int)capture.Argument1) < unchecked((int)threshold);
        if (signedLess)
        {
            if (HasHandoffCapture(capture))
                return Stop(
                    capture,
                    ReceiveGuardForwardingOutcome.UnreachedHandoffCapture,
                    EarlyReturnRawFileOffset,
                    thresholdRead: true,
                    thresholdAddress: ThresholdAddress(capture.Argument0),
                    signedLess: true);

            return new ReceiveGuardForwardingPlan(
                capture,
                ReceiveGuardForwardingOutcome.EarlySignedLessReturn,
                ThresholdRead: true,
                ThresholdAddress: ThresholdAddress(capture.Argument0),
                SignedLessComparison: true,
                HandoffSelected: false,
                TailPointerPushed: false,
                TailPointerValue: null,
                Argument2Pushed: false,
                ForwardedArgument2: null,
                StackArgumentBytesPushed: 0,
                StackArgumentBytesCleaned: 0,
                EbpPopped: true,
                FunctionReturned: true,
                StopAtRawFileOffsetExclusive: GateEndRawFileOffsetExclusive);
        }

        return ProjectHandoff(
            capture,
            thresholdRead: true,
            thresholdAddress: ThresholdAddress(capture.Argument0),
            signedLess: false);
    }

    private static ReceiveGuardForwardingPlan ProjectHandoff(
        ReceiveGuardForwardingCapture capture,
        bool thresholdRead = false,
        uint? thresholdAddress = null,
        bool? signedLess = null)
    {
        uint tailPointer = Argument3TailPointer(capture.FramePointer);
        if (capture.Argument2 is not uint argument2)
        {
            bool laterCaptureProvided = capture.OpaqueCallReturnedNormally.HasValue;
            return new ReceiveGuardForwardingPlan(
                capture,
                laterCaptureProvided
                    ? ReceiveGuardForwardingOutcome.UnreachedHandoffCapture
                    : ReceiveGuardForwardingOutcome.MissingArgument2Capture,
                thresholdRead,
                thresholdAddress,
                signedLess,
                HandoffSelected: true,
                TailPointerPushed: true,
                TailPointerValue: tailPointer,
                Argument2Pushed: false,
                ForwardedArgument2: null,
                StackArgumentBytesPushed: sizeof(uint),
                StackArgumentBytesCleaned: 0,
                EbpPopped: null,
                FunctionReturned: null,
                StopAtRawFileOffsetExclusive: Argument2PushRawFileOffset);
        }

        if (!capture.OpaqueCallReturnedNormally.HasValue)
            return new ReceiveGuardForwardingPlan(
                capture,
                ReceiveGuardForwardingOutcome.MissingOpaqueCallReturnCapture,
                thresholdRead,
                thresholdAddress,
                signedLess,
                HandoffSelected: true,
                TailPointerPushed: true,
                TailPointerValue: tailPointer,
                Argument2Pushed: true,
                ForwardedArgument2: argument2,
                StackArgumentBytesPushed: StackArgumentBytes,
                StackArgumentBytesCleaned: 0,
                EbpPopped: null,
                FunctionReturned: null,
                StopAtRawFileOffsetExclusive: OpaqueCallReturnRawFileOffset);

        if (capture.OpaqueCallReturnedNormally is false)
            return new ReceiveGuardForwardingPlan(
                capture,
                ReceiveGuardForwardingOutcome.OpaqueCallDidNotReturn,
                thresholdRead,
                thresholdAddress,
                signedLess,
                HandoffSelected: true,
                TailPointerPushed: true,
                TailPointerValue: tailPointer,
                Argument2Pushed: true,
                ForwardedArgument2: argument2,
                StackArgumentBytesPushed: StackArgumentBytes,
                StackArgumentBytesCleaned: 0,
                EbpPopped: null,
                FunctionReturned: null,
                StopAtRawFileOffsetExclusive: OpaqueCallReturnRawFileOffset);

        return new ReceiveGuardForwardingPlan(
            capture,
            ReceiveGuardForwardingOutcome.NormalEpilogue,
            thresholdRead,
            thresholdAddress,
            signedLess,
            HandoffSelected: true,
            TailPointerPushed: true,
            TailPointerValue: tailPointer,
            Argument2Pushed: true,
            ForwardedArgument2: argument2,
            StackArgumentBytesPushed: StackArgumentBytes,
            StackArgumentBytesCleaned: StackArgumentBytes,
            EbpPopped: true,
            FunctionReturned: true,
            StopAtRawFileOffsetExclusive: GateEndRawFileOffsetExclusive);
    }

    private static ReceiveGuardForwardingPlan Stop(
        ReceiveGuardForwardingCapture capture,
        ReceiveGuardForwardingOutcome outcome,
        uint stopAtRawFileOffsetExclusive,
        bool thresholdRead = false,
        uint? thresholdAddress = null,
        bool? signedLess = null) =>
        new(
            capture,
            outcome,
            thresholdRead,
            thresholdAddress,
            signedLess,
            HandoffSelected: false,
            TailPointerPushed: false,
            TailPointerValue: null,
            Argument2Pushed: false,
            ForwardedArgument2: null,
            StackArgumentBytesPushed: 0,
            StackArgumentBytesCleaned: 0,
            EbpPopped: null,
            FunctionReturned: null,
            StopAtRawFileOffsetExclusive: stopAtRawFileOffsetExclusive);

    private static bool HasHandoffCapture(ReceiveGuardForwardingCapture capture) =>
        capture.Argument2.HasValue
        || capture.OpaqueCallReturnedNormally.HasValue;
}
