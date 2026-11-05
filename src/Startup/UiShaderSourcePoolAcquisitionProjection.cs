using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolAcquisitionCapture(
    uint OriginalThisPointer,
    uint AcquiredPointer,
    uint CounterValueAtLockedXadd,
    uint? CapturedDivisor);

public enum UiShaderSourcePoolAcquisitionOutcome
{
    CounterIncrementedAwaitingDivisorCapture,
    DivisionByZeroBoundary,
    ReturnedAcquiredPointer,
    DiagnosticThenReturnIfCallReturns
}

public enum UiShaderSourcePoolAcquisitionActionKind
{
    IncrementPoolCounter,
    ReadPoolDivisorForDivision,
    DivisionByZeroDoesNotReachNormalContinuation,
    DivideAcquiredPointerByDivisor,
    CallDiagnosticThroughIndirectCell,
    ReturnAcquiredPointer,
    ReturnAcquiredPointerIfDiagnosticReturns
}

public readonly record struct UiShaderSourcePoolAcquisitionAction(
    UiShaderSourcePoolAcquisitionActionKind Kind,
    uint RawFileOffset,
    uint? Value = null,
    uint? TargetAddress = null);

public readonly record struct UiShaderSourcePoolAcquisitionPlan(
    UiShaderSourcePoolAcquisitionCapture Capture,
    UiShaderSourcePoolAcquisitionOutcome Outcome,
    ImmutableArray<UiShaderSourcePoolAcquisitionAction> Actions,
    uint CounterValueAfterLockedXadd,
    uint? Remainder,
    uint? ReturnValue,
    bool ReturnRequiresDiagnosticCallReturn);

public readonly record struct UiShaderSourcePoolAcquisitionInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null);

/// <summary>
/// Projects the continuation after a captured nonzero pop. It does not model
/// the preceding retry/acquisition loop, allocation, or ownership.
/// </summary>
public static class UiShaderSourcePoolAcquisitionProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x12CDE60;
    public const uint EntryVirtualAddress = 0x16CEA60;
    public const uint EntryEndRawFileOffsetExclusive = 0x12CDEBB;

    public const uint InitialPopCallRawFileOffset = 0x12CDE65;
    public const uint NonzeroPopBranchRawFileOffset = 0x12CDE6E;
    public const uint RetryDispatcherCallRawFileOffset = 0x12CDE72;
    public const uint RetryPopCallRawFileOffset = 0x12CDE79;
    public const uint RetryBranchRawFileOffset = 0x12CDE82;
    public const uint RetryDispatcherRawFileOffset = 0x12CE0F0;
    public const uint PopHelperRawFileOffset = 0x12CDAE0;

    public const uint CounterIncrementSetupRawFileOffset = 0x12CDE84;
    public const uint CounterAddressSetupRawFileOffset = 0x12CDE89;
    public const uint LockedCounterXaddRawFileOffset = 0x12CDE8C;
    public const uint DividendSetupRawFileOffset = 0x12CDE92;
    public const uint DivisorReadAndUnsignedDivideRawFileOffset = 0x12CDE94;
    public const uint RemainderTestRawFileOffset = 0x12CDE97;
    public const uint ZeroRemainderBranchRawFileOffset = 0x12CDE99;
    public const uint DiagnosticTrailingZeroArgumentPushRawFileOffset = 0x12CDE9B;
    public const uint DiagnosticIdPushRawFileOffset = 0x12CDE9D;
    public const uint DiagnosticSecondArgumentAddressPushRawFileOffset = 0x12CDEA2;
    public const uint DiagnosticFirstArgumentAddressPushRawFileOffset = 0x12CDEA7;
    public const uint DiagnosticIndirectCallRawFileOffset = 0x12CDEAC;
    public const uint DiagnosticStackCleanupRawFileOffset = 0x12CDEB2;
    public const uint ReturnAcquiredPointerRawFileOffset = 0x12CDEB6;

    public const uint DiagnosticIndirectCellVirtualAddress = 0x1C15EA0;
    public const uint DiagnosticFirstArgumentVirtualAddress = 0x1B7439F;
    public const uint DiagnosticSecondArgumentVirtualAddress = 0x1B7437B;
    public const uint AlignmentDiagnosticId = 0x254;

    public const int PoolCounterOffset = 0x10;
    public const int PoolDivisorOffset = 0x18;
    public const int PopPairHeadOffset = 0;
    public const int PopPairHighTagOffset = 4;

    public const string CapturedFunctionBytes =
        "51 56 57 8B F9 E8 76 FC FF FF 8B F0 85 F6 75 14 " +
        "8B CF E8 79 02 00 00 8B CF E8 62 FC FF FF 8B F0 " +
        "85 F6 74 EC B9 01 00 00 00 8D 47 10 F0 0F C1 08 " +
        "33 D2 8B C6 F7 77 18 85 D2 74 1A 6A 00 68 54 02 " +
        "00 00 68 7B 43 B7 01 68 9F 43 B7 01 FF 15 A0 5E " +
        "C1 01 83 C4 10 5F 8B C6 5E 59 C3";

    public static IReadOnlyList<UiShaderSourcePoolAcquisitionInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolAcquisitionInstructionEvidence(
                "Save the original this pointer and make the initial pop with it",
                EntryRawFileOffset,
                "51 56 57 8B F9 E8 76 FC FF FF"),
            new UiShaderSourcePoolAcquisitionInstructionEvidence(
                "Continue into the successful-pop block only for a nonzero pop result",
                NonzeroPopBranchRawFileOffset,
                "75 14",
                0x12CDE84),
            new UiShaderSourcePoolAcquisitionInstructionEvidence(
                "Retry dispatch and the next pop both receive the saved original this pointer",
                RetryDispatcherCallRawFileOffset,
                "E8 79 02 00 00 8B CF E8 62 FC FF FF"),
            new UiShaderSourcePoolAcquisitionInstructionEvidence(
                "Increment the pool counter at this+0x10 with locked XADD",
                CounterAddressSetupRawFileOffset,
                "8D 47 10 F0 0F C1 08"),
            new UiShaderSourcePoolAcquisitionInstructionEvidence(
                "The old XADD value is not consumed; prepare the acquired pointer as unsigned dividend",
                LockedCounterXaddRawFileOffset,
                "F0 0F C1 08 33 D2 8B C6"),
            new UiShaderSourcePoolAcquisitionInstructionEvidence(
                "Read the later divisor from this+0x18 and perform unsigned division",
                DivisorReadAndUnsignedDivideRawFileOffset,
                "F7 77 18"),
            new UiShaderSourcePoolAcquisitionInstructionEvidence(
                "Skip diagnostic reporting when the remainder is zero",
                ZeroRemainderBranchRawFileOffset,
                "74 1A",
                0x12CDEB5),
            new UiShaderSourcePoolAcquisitionInstructionEvidence(
                "Push diagnostic ID 0x254 (596 decimal) and its two captured data pointers",
                DiagnosticIdPushRawFileOffset,
                "68 54 02 00 00 68 7B 43 B7 01 68 9F 43 B7 01"),
            new UiShaderSourcePoolAcquisitionInstructionEvidence(
                "Call through the unresolved diagnostic function-pointer cell",
                DiagnosticIndirectCallRawFileOffset,
                "FF 15 A0 5E C1 01"),
            new UiShaderSourcePoolAcquisitionInstructionEvidence(
                "After a normal diagnostic-call return, return the original acquired pointer",
                ReturnAcquiredPointerRawFileOffset,
                "8B C6")
        });

    /// <summary>
    /// Starts strictly after a nonzero pop. A null divisor means its later load
    /// has not yet been captured; zero marks the x86 divide-by-zero boundary.
    /// </summary>
    public static UiShaderSourcePoolAcquisitionPlan Project(
        UiShaderSourcePoolAcquisitionCapture capture)
    {
        if (capture.AcquiredPointer == 0)
        {
            throw new ArgumentException(
                "A zero pop result belongs to the pre-continuation retry path.",
                nameof(capture));
        }

        uint counterAfter = unchecked(capture.CounterValueAtLockedXadd + 1);
        var actions = ImmutableArray.CreateBuilder<UiShaderSourcePoolAcquisitionAction>(5);
        actions.Add(new UiShaderSourcePoolAcquisitionAction(
            UiShaderSourcePoolAcquisitionActionKind.IncrementPoolCounter,
            LockedCounterXaddRawFileOffset,
            counterAfter,
            unchecked(capture.OriginalThisPointer + (uint)PoolCounterOffset)));

        if (!capture.CapturedDivisor.HasValue)
        {
            return new UiShaderSourcePoolAcquisitionPlan(
                capture,
                UiShaderSourcePoolAcquisitionOutcome.CounterIncrementedAwaitingDivisorCapture,
                actions.ToImmutable(),
                counterAfter,
                Remainder: null,
                ReturnValue: null,
                ReturnRequiresDiagnosticCallReturn: false);
        }

        uint divisor = capture.CapturedDivisor.Value;
        actions.Add(new UiShaderSourcePoolAcquisitionAction(
            UiShaderSourcePoolAcquisitionActionKind.ReadPoolDivisorForDivision,
            DivisorReadAndUnsignedDivideRawFileOffset,
            divisor,
            unchecked(capture.OriginalThisPointer + (uint)PoolDivisorOffset)));

        if (divisor == 0)
        {
            actions.Add(new UiShaderSourcePoolAcquisitionAction(
                UiShaderSourcePoolAcquisitionActionKind.DivisionByZeroDoesNotReachNormalContinuation,
                DivisorReadAndUnsignedDivideRawFileOffset));

            return new UiShaderSourcePoolAcquisitionPlan(
                capture,
                UiShaderSourcePoolAcquisitionOutcome.DivisionByZeroBoundary,
                actions.ToImmutable(),
                counterAfter,
                Remainder: null,
                ReturnValue: null,
                ReturnRequiresDiagnosticCallReturn: false);
        }

        uint remainder = capture.AcquiredPointer % divisor;
        actions.Add(new UiShaderSourcePoolAcquisitionAction(
            UiShaderSourcePoolAcquisitionActionKind.DivideAcquiredPointerByDivisor,
            DivisorReadAndUnsignedDivideRawFileOffset,
            remainder));

        if (remainder == 0)
        {
            actions.Add(new UiShaderSourcePoolAcquisitionAction(
                UiShaderSourcePoolAcquisitionActionKind.ReturnAcquiredPointer,
                ReturnAcquiredPointerRawFileOffset,
                capture.AcquiredPointer));

            return new UiShaderSourcePoolAcquisitionPlan(
                capture,
                UiShaderSourcePoolAcquisitionOutcome.ReturnedAcquiredPointer,
                actions.ToImmutable(),
                counterAfter,
                remainder,
                capture.AcquiredPointer,
                ReturnRequiresDiagnosticCallReturn: false);
        }

        actions.Add(new UiShaderSourcePoolAcquisitionAction(
            UiShaderSourcePoolAcquisitionActionKind.CallDiagnosticThroughIndirectCell,
            DiagnosticIndirectCallRawFileOffset,
            AlignmentDiagnosticId,
            DiagnosticIndirectCellVirtualAddress));
        actions.Add(new UiShaderSourcePoolAcquisitionAction(
            UiShaderSourcePoolAcquisitionActionKind.ReturnAcquiredPointerIfDiagnosticReturns,
            ReturnAcquiredPointerRawFileOffset,
            capture.AcquiredPointer));

        return new UiShaderSourcePoolAcquisitionPlan(
            capture,
            UiShaderSourcePoolAcquisitionOutcome.DiagnosticThenReturnIfCallReturns,
            actions.ToImmutable(),
            counterAfter,
            remainder,
            capture.AcquiredPointer,
            ReturnRequiresDiagnosticCallReturn: true);
    }
}
