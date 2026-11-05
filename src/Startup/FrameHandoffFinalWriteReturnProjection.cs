using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffFinalWriteReturnCapture(
    uint EntryEcx,
    uint EntryEsi,
    uint EntryEbp,
    uint? Frame8Pointer,
    uint? PointeeDword,
    uint? Receiver10OldValue);

public enum FrameHandoffFinalWriteReturnOutcome
{
    UnreachedReadCaptureProvided,
    MissingFrame8PointerCapture,
    MissingPointeeDwordCapture,
    MissingReceiver10OldValueCapture,
    ReturnedNormally
}

public readonly record struct FrameHandoffFinalWriteReturnPlan(
    FrameHandoffFinalWriteReturnCapture Capture,
    FrameHandoffFinalWriteReturnOutcome Outcome,
    bool EcxTestReached,
    bool? ZeroBranchTaken,
    uint? Frame8ReadAddress,
    bool Frame8ReadReached,
    bool Frame8PointerCaptured,
    uint? PointeeReadAddress,
    bool PointeeReadReached,
    bool PointeeDwordCaptured,
    bool FinalWriteReached,
    bool FinalWriteProjected,
    uint? FinalWriteAddress,
    uint? FinalWriteValue,
    uint? ReturnEaxValue,
    uint? Receiver10ReadAddress,
    bool CounterIncrementReached,
    bool Receiver10OldValueCaptured,
    uint? CounterValueBeforeIncrement,
    uint? CounterValueAfterIncrement,
    ImmutableArray<string> StackPopRegisters,
    bool ReturnReached,
    ushort? ReturnImmediateBytes,
    bool UnreachedFrame8PointerCaptureProvided,
    bool UnreachedPointeeDwordCaptureProvided,
    bool UnreachedReceiver10OldValueCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct FrameHandoffFinalWriteReturnInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the conditional final write and independently reached receiver counter increment.
/// </summary>
public static class FrameHandoffFinalWriteReturnProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE651C;
    public const uint EcxTestRawFileOffset = EntryRawFileOffset;
    public const uint ZeroBranchRawFileOffset = 0x00BE651E;
    public const uint ZeroBranchTargetRawFileOffset = 0x00BE6527;
    public const uint Frame8ReadRawFileOffset = 0x00BE6520;
    public const uint PointeeReadRawFileOffset = 0x00BE6523;
    public const uint FinalWriteRawFileOffset = 0x00BE6525;
    public const uint CounterIncrementRawFileOffset = 0x00BE6527;
    public const uint PopEsiRawFileOffset = 0x00BE652A;
    public const uint PopEbpRawFileOffset = 0x00BE652B;
    public const uint ReturnRawFileOffset = 0x00BE652C;
    public const uint StopBeforeEcxTestRawFileOffsetExclusive = EcxTestRawFileOffset;
    public const uint ReturnBoundaryRawFileOffsetExclusive = 0x00BE652F;

    public const uint Frame8Offset = 8;
    public const uint Receiver10Offset = 0x10;
    public const ushort ReturnImmediateBytes = 4;

    public const string EcxTestBytes = "85 C9";
    public const string ZeroBranchBytes = "74 07";
    public const string CapturedWindowBytes =
        "85 C9 74 07 8B 45 08 8B 00 89 01 FF 46 10 5E 5D C2 04 00";

    public static ImmutableArray<FrameHandoffFinalWriteReturnInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new FrameHandoffFinalWriteReturnInstructionEvidence(
                "Test entry ECX",
                EcxTestRawFileOffset,
                EcxTestRawFileOffset,
                RawFileOffsetToVirtualAddress(EcxTestRawFileOffset),
                "85 C9"),
            new FrameHandoffFinalWriteReturnInstructionEvidence(
                "Jump directly to the receiver counter increment when ECX is zero",
                ZeroBranchRawFileOffset,
                ZeroBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroBranchRawFileOffset),
                "74 07"),
            new FrameHandoffFinalWriteReturnInstructionEvidence(
                "Load the DWORD pointer from [EBP+8]",
                Frame8ReadRawFileOffset,
                Frame8ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(Frame8ReadRawFileOffset),
                "8B 45 08"),
            new FrameHandoffFinalWriteReturnInstructionEvidence(
                "Load the pointee DWORD into EAX",
                PointeeReadRawFileOffset,
                PointeeReadRawFileOffset,
                RawFileOffsetToVirtualAddress(PointeeReadRawFileOffset),
                "8B 00"),
            new FrameHandoffFinalWriteReturnInstructionEvidence(
                "Store the pointee DWORD at [ECX]",
                FinalWriteRawFileOffset,
                FinalWriteRawFileOffset,
                RawFileOffsetToVirtualAddress(FinalWriteRawFileOffset),
                "89 01"),
            new FrameHandoffFinalWriteReturnInstructionEvidence(
                "Increment the DWORD at [ESI+0x10]",
                CounterIncrementRawFileOffset,
                CounterIncrementRawFileOffset,
                RawFileOffsetToVirtualAddress(CounterIncrementRawFileOffset),
                "FF 46 10"),
            new FrameHandoffFinalWriteReturnInstructionEvidence(
                "Pop ESI",
                PopEsiRawFileOffset,
                PopEsiRawFileOffset,
                RawFileOffsetToVirtualAddress(PopEsiRawFileOffset),
                "5E"),
            new FrameHandoffFinalWriteReturnInstructionEvidence(
                "Pop EBP",
                PopEbpRawFileOffset,
                PopEbpRawFileOffset,
                RawFileOffsetToVirtualAddress(PopEbpRawFileOffset),
                "5D"),
            new FrameHandoffFinalWriteReturnInstructionEvidence(
                "Return and discard four argument bytes",
                ReturnRawFileOffset,
                ReturnRawFileOffset,
                RawFileOffsetToVirtualAddress(ReturnRawFileOffset),
                "C2 04 00"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint Frame8ReadAddress(uint entryEbp) =>
        unchecked(entryEbp + Frame8Offset);

    public static uint Receiver10ReadAddress(uint entryEsi) =>
        unchecked(entryEsi + Receiver10Offset);

    public static FrameHandoffFinalWriteReturnPlan Project(
        FrameHandoffFinalWriteReturnCapture capture)
    {
        bool zeroEcx = capture.EntryEcx == 0;
        if (zeroEcx && (capture.Frame8Pointer.HasValue || capture.PointeeDword.HasValue))
        {
            return Stop(
                capture,
                FrameHandoffFinalWriteReturnOutcome.UnreachedReadCaptureProvided,
                ecxTestReached: true,
                zeroBranchTaken: true,
                unreachedFrame8PointerCaptureProvided: capture.Frame8Pointer.HasValue,
                unreachedPointeeDwordCaptureProvided: capture.PointeeDword.HasValue,
                unreachedReceiver10OldValueCaptureProvided:
                    capture.Receiver10OldValue.HasValue,
                stopAt: ZeroBranchRawFileOffset);
        }

        uint? frame8ReadAddress = zeroEcx ? null : Frame8ReadAddress(capture.EntryEbp);
        if (!zeroEcx && !capture.Frame8Pointer.HasValue)
        {
            return Stop(
                capture,
                FrameHandoffFinalWriteReturnOutcome.MissingFrame8PointerCapture,
                ecxTestReached: true,
                zeroBranchTaken: false,
                frame8ReadAddress: frame8ReadAddress,
                frame8ReadReached: true,
                unreachedPointeeDwordCaptureProvided: capture.PointeeDword.HasValue,
                unreachedReceiver10OldValueCaptureProvided:
                    capture.Receiver10OldValue.HasValue,
                stopAt: Frame8ReadRawFileOffset);
        }

        uint? pointeeReadAddress = zeroEcx ? null : capture.Frame8Pointer;
        if (!zeroEcx && !capture.PointeeDword.HasValue)
        {
            return Stop(
                capture,
                FrameHandoffFinalWriteReturnOutcome.MissingPointeeDwordCapture,
                ecxTestReached: true,
                zeroBranchTaken: false,
                frame8ReadAddress: frame8ReadAddress,
                frame8ReadReached: true,
                frame8PointerCaptured: true,
                pointeeReadAddress: pointeeReadAddress,
                pointeeReadReached: true,
                unreachedReceiver10OldValueCaptureProvided:
                    capture.Receiver10OldValue.HasValue,
                stopAt: PointeeReadRawFileOffset);
        }

        bool finalWriteReached = !zeroEcx;
        bool finalWriteProjected = !zeroEcx;
        uint? finalWriteAddress = finalWriteReached ? capture.EntryEcx : null;
        uint? finalWriteValue = finalWriteReached ? capture.PointeeDword : null;
        if (capture.Receiver10OldValue is not uint counterBefore)
        {
            return Stop(
                capture,
                FrameHandoffFinalWriteReturnOutcome.MissingReceiver10OldValueCapture,
                ecxTestReached: true,
                zeroBranchTaken: zeroEcx,
                frame8ReadAddress: frame8ReadAddress,
                frame8ReadReached: !zeroEcx,
                frame8PointerCaptured: !zeroEcx,
                pointeeReadAddress: pointeeReadAddress,
                pointeeReadReached: !zeroEcx,
                pointeeDwordCaptured: !zeroEcx,
                finalWriteReached: finalWriteReached,
                finalWriteProjected: finalWriteProjected,
                finalWriteAddress: finalWriteAddress,
                finalWriteValue: finalWriteValue,
                returnEaxValue: zeroEcx ? null : capture.PointeeDword,
                receiver10ReadAddress: Receiver10ReadAddress(capture.EntryEsi),
                counterIncrementReached: true,
                stopAt: CounterIncrementRawFileOffset);
        }

        return new FrameHandoffFinalWriteReturnPlan(
            Capture: capture,
            Outcome: FrameHandoffFinalWriteReturnOutcome.ReturnedNormally,
            EcxTestReached: true,
            ZeroBranchTaken: zeroEcx,
            Frame8ReadAddress: frame8ReadAddress,
            Frame8ReadReached: !zeroEcx,
            Frame8PointerCaptured: !zeroEcx,
            PointeeReadAddress: pointeeReadAddress,
            PointeeReadReached: !zeroEcx,
            PointeeDwordCaptured: !zeroEcx,
            FinalWriteReached: finalWriteReached,
            FinalWriteProjected: finalWriteProjected,
            FinalWriteAddress: finalWriteAddress,
            FinalWriteValue: finalWriteValue,
            ReturnEaxValue: zeroEcx ? null : capture.PointeeDword,
            Receiver10ReadAddress: Receiver10ReadAddress(capture.EntryEsi),
            CounterIncrementReached: true,
            Receiver10OldValueCaptured: true,
            CounterValueBeforeIncrement: counterBefore,
            CounterValueAfterIncrement: unchecked(counterBefore + 1),
            StackPopRegisters: ImmutableArray.Create("ESI", "EBP"),
            ReturnReached: true,
            ReturnImmediateBytes: ReturnImmediateBytes,
            UnreachedFrame8PointerCaptureProvided: false,
            UnreachedPointeeDwordCaptureProvided: false,
            UnreachedReceiver10OldValueCaptureProvided: false,
            StopAtRawFileOffsetExclusive: ReturnBoundaryRawFileOffsetExclusive);
    }

    private static FrameHandoffFinalWriteReturnPlan Stop(
        FrameHandoffFinalWriteReturnCapture capture,
        FrameHandoffFinalWriteReturnOutcome outcome,
        bool ecxTestReached = false,
        bool? zeroBranchTaken = null,
        uint? frame8ReadAddress = null,
        bool frame8ReadReached = false,
        bool frame8PointerCaptured = false,
        uint? pointeeReadAddress = null,
        bool pointeeReadReached = false,
        bool pointeeDwordCaptured = false,
        bool finalWriteReached = false,
        bool finalWriteProjected = false,
        uint? finalWriteAddress = null,
        uint? finalWriteValue = null,
        uint? returnEaxValue = null,
        uint? receiver10ReadAddress = null,
        bool counterIncrementReached = false,
        bool receiver10OldValueCaptured = false,
        uint? counterValueBeforeIncrement = null,
        uint? counterValueAfterIncrement = null,
        bool unreachedFrame8PointerCaptureProvided = false,
        bool unreachedPointeeDwordCaptureProvided = false,
        bool unreachedReceiver10OldValueCaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            ecxTestReached,
            zeroBranchTaken,
            frame8ReadAddress,
            frame8ReadReached,
            frame8PointerCaptured,
            pointeeReadAddress,
            pointeeReadReached,
            pointeeDwordCaptured,
            finalWriteReached,
            finalWriteProjected,
            finalWriteAddress,
            finalWriteValue,
            returnEaxValue,
            receiver10ReadAddress,
            counterIncrementReached,
            receiver10OldValueCaptured,
            counterValueBeforeIncrement,
            counterValueAfterIncrement,
            ImmutableArray<string>.Empty,
            ReturnReached: false,
            ReturnImmediateBytes: null,
            UnreachedFrame8PointerCaptureProvided: unreachedFrame8PointerCaptureProvided,
            UnreachedPointeeDwordCaptureProvided: unreachedPointeeDwordCaptureProvided,
            UnreachedReceiver10OldValueCaptureProvided:
                unreachedReceiver10OldValueCaptureProvided,
            StopAtRawFileOffsetExclusive: stopAt);
}
