using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeFieldDescentStepCapture(
    uint? ValidatedEntryEBP,
    uint? ValidatedEntryEAX,
    uint? FreshFrameMinus4,
    uint? FreshEaxPlus14,
    uint? FreshEaxPlus10,
    uint? FreshEcxValue);

public enum CapacityFinalCalleeFieldDescentStepOutcome
{
    MissingValidatedEntryEBP,
    MissingFreshFrameMinus4,
    MissingValidatedEntryEAX,
    MissingFreshEaxPlus14,
    MissingFreshEaxPlus10,
    MissingFreshEcxValue,
    ReachedTerminal,
    TerminalHasUnreachedCaptures,
    ReachedLoopback,
    LoopbackHasUnreachedCaptures
}

public readonly record struct CapacityFinalCalleeFieldDescentStepInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct CapacityFinalCalleeFieldDescentStepPlan(
    CapacityFinalCalleeFieldDescentStepCapture Capture,
    CapacityFinalCalleeFieldDescentStepOutcome Outcome,
    bool FreshFrameMinus4ReadReached,
    bool FreshFrameMinus4Captured,
    uint? FreshFrameMinus4Address,
    uint? EDXLoadedFromFrameMinus4,
    bool ValidatedEntryEAXUseReached,
    bool EaxPlus14ReadReached,
    bool EaxPlus14Captured,
    uint? FreshEaxPlus14Address,
    uint? FreshEaxPlus14Value,
    bool EaxPlus14CompareReached,
    bool? EaxPlus14EqualsZero,
    bool FirstLeaReached,
    uint? ECXAfterFirstLea,
    bool? FirstLeaPreservedZeroFlag,
    bool FirstJneReached,
    bool? FirstJneTaken,
    bool EaxPlus10ReadReached,
    bool EaxPlus10Captured,
    uint? FreshEaxPlus10Address,
    uint? FreshEaxPlus10Value,
    bool EaxPlus10CompareReached,
    bool? EaxPlus10EqualsZero,
    bool SecondLeaReached,
    uint? ECXAfterSecondLea,
    bool? SecondLeaPreservedZeroFlag,
    bool SecondJeReached,
    bool? SecondJeTaken,
    bool FreshEcxReadReached,
    bool FreshEcxValueCaptured,
    uint? FreshEcxAddress,
    uint? FreshEcxValue,
    uint? EAXLoadedFromEcx,
    uint? EDXLoadedFromEcx,
    bool LoopbackReached,
    bool TerminalReached,
    bool UnreachedValidatedEntryEAXCaptureProvided,
    bool UnreachedFreshFrameMinus4CaptureProvided,
    bool UnreachedFreshEaxPlus14CaptureProvided,
    bool UnreachedFreshEaxPlus10CaptureProvided,
    bool UnreachedFreshEcxCaptureProvided,
    bool AnyUnreachedCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects one bounded field-descent step. The FC5 loopback is recorded but
/// never iterated; each captured memory value is an independent fresh read.
/// </summary>
public static class CapacityFinalCalleeFieldDescentStepProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8FC2;
    public const uint EntryVirtualAddress = 0x016C9BC2;
    public const uint FirstCompareRawFileOffset = 0x012C8FC5;
    public const uint FirstLeaRawFileOffset = 0x012C8FC9;
    public const uint FirstBranchRawFileOffset = 0x012C8FCC;
    public const uint SecondCompareRawFileOffset = 0x012C8FCE;
    public const uint SecondLeaRawFileOffset = 0x012C8FD2;
    public const uint SecondBranchRawFileOffset = 0x012C8FD5;
    public const uint FreshEcxLoadRawFileOffset = 0x012C8FD7;
    public const uint EDXUpdateRawFileOffset = 0x012C8FD9;
    public const uint LoopbackJumpRawFileOffset = 0x012C8FDB;
    public const uint LoopbackRawFileOffset = 0x012C8FC5;
    public const uint TerminalRawFileOffset = 0x012C8FDD;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8FDD;

    public const uint FrameMinus4Displacement = 4;
    public const uint EaxPlus14Displacement = 0x14;
    public const uint EaxPlus10Displacement = 0x10;

    public const string CapturedWindowBytes =
        "8B 55 FC 83 78 14 00 8D 48 14 75 09 "
        + "83 78 10 00 8D 48 10 74 06 8B 01 8B D1 EB E8";

    public static ImmutableArray<CapacityFinalCalleeFieldDescentStepInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load a fresh DWORD at entry EBP-4 into EDX",
                EntryRawFileOffset, "8B 55 FC"),
            Instruction("Compare a fresh DWORD at entry EAX+14 with zero",
                FirstCompareRawFileOffset, "83 78 14 00"),
            Instruction("Set ECX to entry EAX+14 without changing comparison flags",
                FirstLeaRawFileOffset, "8D 48 14"),
            Instruction("Skip the EAX+10 comparison when the EAX+14 DWORD is nonzero",
                FirstBranchRawFileOffset, "75 09"),
            Instruction("Compare a fresh DWORD at entry EAX+10 with zero",
                SecondCompareRawFileOffset, "83 78 10 00"),
            Instruction("Set ECX to entry EAX+10 without changing comparison flags",
                SecondLeaRawFileOffset, "8D 48 10"),
            Instruction("Stop at FDD when the EAX+10 DWORD is zero",
                SecondBranchRawFileOffset, "74 06"),
            Instruction("Load a fresh DWORD at ECX into EAX",
                FreshEcxLoadRawFileOffset, "8B 01"),
            Instruction("Copy ECX into EDX",
                EDXUpdateRawFileOffset, "8B D1"),
            Instruction("Jump back to FC5; do not iterate this projection",
                LoopbackJumpRawFileOffset, "EB E8"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFrameMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus4Displacement);

    public static uint DeriveEaxPlus14Address(uint entryEax) =>
        unchecked(entryEax + EaxPlus14Displacement);

    public static uint DeriveEaxPlus10Address(uint entryEax) =>
        unchecked(entryEax + EaxPlus10Displacement);

    public static CapacityFinalCalleeFieldDescentStepPlan Project(
        CapacityFinalCalleeFieldDescentStepCapture capture)
    {
        bool frameMinus4ReadReached = false;
        bool frameMinus4Captured = false;
        uint? frameMinus4Address = null;
        uint? edxLoadedFromFrameMinus4 = null;
        bool entryEaxUseReached = false;
        bool eaxPlus14ReadReached = false;
        bool eaxPlus14Captured = false;
        uint? eaxPlus14Address = null;
        uint? eaxPlus14Value = null;
        bool eaxPlus14CompareReached = false;
        bool? eaxPlus14EqualsZero = null;
        bool firstLeaReached = false;
        uint? ecxAfterFirstLea = null;
        bool? firstLeaPreservedZeroFlag = null;
        bool firstJneReached = false;
        bool? firstJneTaken = null;
        bool eaxPlus10ReadReached = false;
        bool eaxPlus10Captured = false;
        uint? eaxPlus10Address = null;
        uint? eaxPlus10Value = null;
        bool eaxPlus10CompareReached = false;
        bool? eaxPlus10EqualsZero = null;
        bool secondLeaReached = false;
        uint? ecxAfterSecondLea = null;
        bool? secondLeaPreservedZeroFlag = null;
        bool secondJeReached = false;
        bool? secondJeTaken = null;
        bool freshEcxReadReached = false;
        bool freshEcxValueCaptured = false;
        uint? freshEcxAddress = null;
        uint? freshEcxValue = null;
        uint? eaxLoadedFromEcx = null;
        uint? edxLoadedFromEcx = null;
        bool loopbackReached = false;
        bool terminalReached = false;

        CapacityFinalCalleeFieldDescentStepPlan BuildPlan(
            CapacityFinalCalleeFieldDescentStepOutcome outcome,
            uint stopAt) =>
            new(
                capture,
                outcome,
                frameMinus4ReadReached,
                frameMinus4Captured,
                frameMinus4Address,
                edxLoadedFromFrameMinus4,
                entryEaxUseReached,
                eaxPlus14ReadReached,
                eaxPlus14Captured,
                eaxPlus14Address,
                eaxPlus14Value,
                eaxPlus14CompareReached,
                eaxPlus14EqualsZero,
                firstLeaReached,
                ecxAfterFirstLea,
                firstLeaPreservedZeroFlag,
                firstJneReached,
                firstJneTaken,
                eaxPlus10ReadReached,
                eaxPlus10Captured,
                eaxPlus10Address,
                eaxPlus10Value,
                eaxPlus10CompareReached,
                eaxPlus10EqualsZero,
                secondLeaReached,
                ecxAfterSecondLea,
                secondLeaPreservedZeroFlag,
                secondJeReached,
                secondJeTaken,
                freshEcxReadReached,
                freshEcxValueCaptured,
                freshEcxAddress,
                freshEcxValue,
                eaxLoadedFromEcx,
                edxLoadedFromEcx,
                loopbackReached,
                terminalReached,
                !entryEaxUseReached && capture.ValidatedEntryEAX.HasValue,
                !frameMinus4ReadReached && capture.FreshFrameMinus4.HasValue,
                !eaxPlus14ReadReached && capture.FreshEaxPlus14.HasValue,
                !eaxPlus10ReadReached && capture.FreshEaxPlus10.HasValue,
                !freshEcxReadReached && capture.FreshEcxValue.HasValue,
                (!entryEaxUseReached && capture.ValidatedEntryEAX.HasValue)
                    || (!frameMinus4ReadReached && capture.FreshFrameMinus4.HasValue)
                    || (!eaxPlus14ReadReached && capture.FreshEaxPlus14.HasValue)
                    || (!eaxPlus10ReadReached && capture.FreshEaxPlus10.HasValue)
                    || (!freshEcxReadReached && capture.FreshEcxValue.HasValue),
                stopAt);

        if (capture.ValidatedEntryEBP is not uint entryEbp)
        {
            return BuildPlan(
                CapacityFinalCalleeFieldDescentStepOutcome.MissingValidatedEntryEBP,
                EntryRawFileOffset);
        }

        frameMinus4ReadReached = true;
        frameMinus4Address = DeriveFrameMinus4Address(entryEbp);
        if (capture.FreshFrameMinus4 is not uint freshFrameMinus4)
        {
            return BuildPlan(
                CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshFrameMinus4,
                EntryRawFileOffset);
        }

        frameMinus4Captured = true;
        edxLoadedFromFrameMinus4 = freshFrameMinus4;
        if (capture.ValidatedEntryEAX is not uint entryEax)
        {
            return BuildPlan(
                CapacityFinalCalleeFieldDescentStepOutcome.MissingValidatedEntryEAX,
                FirstCompareRawFileOffset);
        }

        entryEaxUseReached = true;
        eaxPlus14ReadReached = true;
        eaxPlus14Address = DeriveEaxPlus14Address(entryEax);
        if (capture.FreshEaxPlus14 is not uint freshEaxPlus14)
        {
            return BuildPlan(
                CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshEaxPlus14,
                FirstCompareRawFileOffset);
        }

        eaxPlus14Captured = true;
        eaxPlus14Value = freshEaxPlus14;
        eaxPlus14CompareReached = true;
        eaxPlus14EqualsZero = freshEaxPlus14 == 0;
        firstLeaReached = true;
        ecxAfterFirstLea = eaxPlus14Address;
        firstLeaPreservedZeroFlag = eaxPlus14EqualsZero;
        firstJneReached = true;
        firstJneTaken = !eaxPlus14EqualsZero.Value;

        if (eaxPlus14EqualsZero.Value)
        {
            eaxPlus10ReadReached = true;
            eaxPlus10Address = DeriveEaxPlus10Address(entryEax);
            if (capture.FreshEaxPlus10 is not uint freshEaxPlus10)
            {
                return BuildPlan(
                    CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshEaxPlus10,
                    SecondCompareRawFileOffset);
            }

            eaxPlus10Captured = true;
            eaxPlus10Value = freshEaxPlus10;
            eaxPlus10CompareReached = true;
            eaxPlus10EqualsZero = freshEaxPlus10 == 0;
            secondLeaReached = true;
            ecxAfterSecondLea = eaxPlus10Address;
            secondLeaPreservedZeroFlag = eaxPlus10EqualsZero;
            secondJeReached = true;
            secondJeTaken = eaxPlus10EqualsZero.Value;
            if (secondJeTaken.Value)
            {
                terminalReached = true;
                bool hasUnreachedCaptures =
                    (!freshEcxReadReached && capture.FreshEcxValue.HasValue);
                return BuildPlan(
                    hasUnreachedCaptures
                        ? CapacityFinalCalleeFieldDescentStepOutcome
                            .TerminalHasUnreachedCaptures
                        : CapacityFinalCalleeFieldDescentStepOutcome.ReachedTerminal,
                    TerminalRawFileOffset);
            }
        }

        freshEcxAddress = firstJneTaken.Value
            ? ecxAfterFirstLea
            : ecxAfterSecondLea;
        freshEcxReadReached = true;
        if (capture.FreshEcxValue is not uint capturedEcxValue)
        {
            return BuildPlan(
                CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshEcxValue,
                FreshEcxLoadRawFileOffset);
        }

        freshEcxValueCaptured = true;
        freshEcxValue = capturedEcxValue;
        eaxLoadedFromEcx = capturedEcxValue;
        edxLoadedFromEcx = freshEcxAddress;
        loopbackReached = true;
        bool loopbackHasUnreachedCaptures =
            !eaxPlus10ReadReached && capture.FreshEaxPlus10.HasValue;
        return BuildPlan(
            loopbackHasUnreachedCaptures
                ? CapacityFinalCalleeFieldDescentStepOutcome
                    .LoopbackHasUnreachedCaptures
                : CapacityFinalCalleeFieldDescentStepOutcome.ReachedLoopback,
            LoopbackRawFileOffset);
    }

    private static CapacityFinalCalleeFieldDescentStepInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes);
}
