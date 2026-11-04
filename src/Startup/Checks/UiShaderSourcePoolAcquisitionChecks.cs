using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolAcquisitionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Pool-acquisition anchors and captured bytes match the pinned sample and raw-offset mapping",
            UiShaderSourcePoolAcquisitionProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolAcquisitionProjection.EntryRawFileOffset == 0x12CDE60
            && UiShaderSourcePoolAcquisitionProjection.EntryVirtualAddress == 0x16CEA60
            && UiShaderSourcePoolAcquisitionProjection.EntryRawFileOffset
                + UiShaderSourcePoolAcquisitionProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolAcquisitionProjection.EntryVirtualAddress
            && UiShaderSourcePoolAcquisitionProjection.EntryEndRawFileOffsetExclusive == 0x12CDEBB
            && UiShaderSourcePoolAcquisitionProjection.CapturedFunctionBytes
                == "51 56 57 8B F9 E8 76 FC FF FF 8B F0 85 F6 75 14 "
                    + "8B CF E8 79 02 00 00 8B CF E8 62 FC FF FF 8B F0 "
                    + "85 F6 74 EC B9 01 00 00 00 8D 47 10 F0 0F C1 08 "
                    + "33 D2 8B C6 F7 77 18 85 D2 74 1A 6A 00 68 54 02 "
                    + "00 00 68 7B 43 B7 01 68 9F 43 B7 01 FF 15 A0 5E "
                    + "C1 01 83 C4 10 5F 8B C6 5E 59 C3"
            && UiShaderSourcePoolAcquisitionProjection.PoolCounterOffset == 0x10
            && UiShaderSourcePoolAcquisitionProjection.PoolDivisorOffset == 0x18
            && UiShaderSourcePoolAcquisitionProjection.PopPairHeadOffset == 0
            && UiShaderSourcePoolAcquisitionProjection.PopPairHighTagOffset == 4);

        check("Evidence keeps initial/retry pop on original this and records XADD, divisor, diagnostic, and return bytes",
            UiShaderSourcePoolAcquisitionProjection.PopHelperRawFileOffset == 0x12CDAE0
            && UiShaderSourcePoolAcquisitionProjection.InitialPopCallRawFileOffset == 0x12CDE65
            && UiShaderSourcePoolAcquisitionProjection.RetryDispatcherRawFileOffset == 0x12CE0F0
            && UiShaderSourcePoolAcquisitionProjection.RetryPopCallRawFileOffset == 0x12CDE79
            && UiShaderSourcePoolAcquisitionProjection.CounterAddressSetupRawFileOffset == 0x12CDE89
            && UiShaderSourcePoolAcquisitionProjection.LockedCounterXaddRawFileOffset == 0x12CDE8C
            && UiShaderSourcePoolAcquisitionProjection.DivisorReadAndUnsignedDivideRawFileOffset == 0x12CDE94
            && UiShaderSourcePoolAcquisitionProjection.ZeroRemainderBranchRawFileOffset == 0x12CDE99
            && UiShaderSourcePoolAcquisitionProjection.DiagnosticIndirectCallRawFileOffset == 0x12CDEAC
            && UiShaderSourcePoolAcquisitionProjection.DiagnosticIndirectCellVirtualAddress == 0x1C15EA0
            && UiShaderSourcePoolAcquisitionProjection.AlignmentDiagnosticId == 0x254
            && UiShaderSourcePoolAcquisitionProjection.AlignmentDiagnosticId == 596
            && UiShaderSourcePoolAcquisitionProjection.ReturnAcquiredPointerRawFileOffset == 0x12CDEB6
            && UiShaderSourcePoolAcquisitionProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.BranchTargetRawFileOffset))
                .SequenceEqual(new (uint RawFileOffset, string Bytes, uint? BranchTargetRawFileOffset)[]
                {
                    (0x12CDE60, "51 56 57 8B F9 E8 76 FC FF FF", null),
                    (0x12CDE6E, "75 14", 0x12CDE84),
                    (0x12CDE72, "E8 79 02 00 00 8B CF E8 62 FC FF FF", null),
                    (0x12CDE89, "8D 47 10 F0 0F C1 08", null),
                    (0x12CDE8C, "F0 0F C1 08 33 D2 8B C6", null),
                    (0x12CDE94, "F7 77 18", null),
                    (0x12CDE99, "74 1A", 0x12CDEB5),
                    (0x12CDE9D, "68 54 02 00 00 68 7B 43 B7 01 68 9F 43 B7 01", null),
                    (0x12CDEAC, "FF 15 A0 5E C1 01", null),
                    (0x12CDEB6, "8B C6", null)
                }));

        bool zeroPopRejectedAsOutsideContinuation = false;
        try
        {
            UiShaderSourcePoolAcquisitionProjection.Project(
                new UiShaderSourcePoolAcquisitionCapture(
                    OriginalThisPointer: 0x10203040,
                    AcquiredPointer: 0,
                    CounterValueAtLockedXadd: 0,
                    CapturedDivisor: 1));
        }
        catch (ArgumentException exception)
        {
            zeroPopRejectedAsOutsideContinuation =
                exception.ParamName == "capture"
                && exception.Message.Contains("pre-continuation retry path", StringComparison.Ordinal);
        }

        check("Projection explicitly rejects a zero pop result as outside its entered continuation",
            zeroPopRejectedAsOutsideContinuation);

        UiShaderSourcePoolAcquisitionPlan awaitingDivisor =
            UiShaderSourcePoolAcquisitionProjection.Project(
                new UiShaderSourcePoolAcquisitionCapture(
                    OriginalThisPointer: 0x10203040,
                    AcquiredPointer: 0x1000,
                    CounterValueAtLockedXadd: uint.MaxValue,
                    CapturedDivisor: null));
        check("Locked XADD wraps its counter while an uncaptured later divisor remains a separate boundary",
            awaitingDivisor.Outcome
                == UiShaderSourcePoolAcquisitionOutcome.CounterIncrementedAwaitingDivisorCapture
            && awaitingDivisor.CounterValueAfterLockedXadd == 0
            && awaitingDivisor.Remainder is null
            && awaitingDivisor.ReturnValue is null
            && awaitingDivisor.Actions.SequenceEqual(new[]
            {
                new UiShaderSourcePoolAcquisitionAction(
                    UiShaderSourcePoolAcquisitionActionKind.IncrementPoolCounter,
                    0x12CDE8C,
                    0,
                    0x10203050)
            }));

        UiShaderSourcePoolAcquisitionPlan zeroDivisor =
            UiShaderSourcePoolAcquisitionProjection.Project(
                new UiShaderSourcePoolAcquisitionCapture(
                    OriginalThisPointer: 0x55667788,
                    AcquiredPointer: 0x1001,
                    CounterValueAtLockedXadd: 7,
                    CapturedDivisor: 0));
        check("A captured zero divisor is a distinct divide-by-zero boundary, not a zero remainder or return",
            zeroDivisor.Outcome == UiShaderSourcePoolAcquisitionOutcome.DivisionByZeroBoundary
            && zeroDivisor.CounterValueAfterLockedXadd == 8
            && zeroDivisor.Remainder is null
            && zeroDivisor.ReturnValue is null
            && zeroDivisor.Actions.SequenceEqual(new[]
            {
                new UiShaderSourcePoolAcquisitionAction(
                    UiShaderSourcePoolAcquisitionActionKind.IncrementPoolCounter,
                    0x12CDE8C,
                    8,
                    0x55667798),
                new UiShaderSourcePoolAcquisitionAction(
                    UiShaderSourcePoolAcquisitionActionKind.ReadPoolDivisorForDivision,
                    0x12CDE94,
                    0,
                    0x556677A0),
                new UiShaderSourcePoolAcquisitionAction(
                    UiShaderSourcePoolAcquisitionActionKind.DivisionByZeroDoesNotReachNormalContinuation,
                    0x12CDE94)
            }));

        UiShaderSourcePoolAcquisitionPlan aligned =
            UiShaderSourcePoolAcquisitionProjection.Project(
                new UiShaderSourcePoolAcquisitionCapture(
                    OriginalThisPointer: 0x55667788,
                    AcquiredPointer: 0x1000,
                    CounterValueAtLockedXadd: 41,
                    CapturedDivisor: 0x100));
        check("A zero unsigned remainder returns the acquired pointer unchanged without diagnostic reporting",
            aligned.Outcome == UiShaderSourcePoolAcquisitionOutcome.ReturnedAcquiredPointer
            && aligned.Remainder == 0
            && aligned.ReturnValue == 0x1000
            && !aligned.ReturnRequiresDiagnosticCallReturn
            && aligned.Actions[^1]
                == new UiShaderSourcePoolAcquisitionAction(
                    UiShaderSourcePoolAcquisitionActionKind.ReturnAcquiredPointer,
                    0x12CDEB6,
                    0x1000));

        UiShaderSourcePoolAcquisitionPlan unaligned =
            UiShaderSourcePoolAcquisitionProjection.Project(
                new UiShaderSourcePoolAcquisitionCapture(
                    OriginalThisPointer: 0x55667788,
                    AcquiredPointer: uint.MaxValue,
                    CounterValueAtLockedXadd: 0xFFFFFFFF,
                    CapturedDivisor: 2));
        check("Unsigned nonzero remainder reports ID 0x254 (596 decimal) through the indirect cell and conditionally returns the same pointer",
            unaligned.Outcome
                == UiShaderSourcePoolAcquisitionOutcome.DiagnosticThenReturnIfCallReturns
            && unaligned.Capture.OriginalThisPointer == 0x55667788
            && unaligned.CounterValueAfterLockedXadd == 0
            && unaligned.Remainder == 1
            && unaligned.ReturnValue == uint.MaxValue
            && unaligned.ReturnRequiresDiagnosticCallReturn
            && unaligned.Actions[^2]
                == new UiShaderSourcePoolAcquisitionAction(
                    UiShaderSourcePoolAcquisitionActionKind.CallDiagnosticThroughIndirectCell,
                    0x12CDEAC,
                    0x254,
                    0x1C15EA0)
            && unaligned.Actions[^1]
                == new UiShaderSourcePoolAcquisitionAction(
                    UiShaderSourcePoolAcquisitionActionKind.ReturnAcquiredPointerIfDiagnosticReturns,
                    0x12CDEB6,
                    uint.MaxValue));

        UiShaderSourcePoolAcquisitionPlan wrappedAddresses =
            UiShaderSourcePoolAcquisitionProjection.Project(
                new UiShaderSourcePoolAcquisitionCapture(
                    OriginalThisPointer: 0xFFFFFFF8,
                    AcquiredPointer: 0x1001,
                    CounterValueAtLockedXadd: 5,
                    CapturedDivisor: 3));
        check("Counter and later divisor addresses independently wrap from the same original this pointer",
            wrappedAddresses.Capture.OriginalThisPointer == 0xFFFFFFF8
            && wrappedAddresses.Capture.CapturedDivisor == 3
            && wrappedAddresses.Actions[0]
                == new UiShaderSourcePoolAcquisitionAction(
                    UiShaderSourcePoolAcquisitionActionKind.IncrementPoolCounter,
                    0x12CDE8C,
                    6,
                    8)
            && wrappedAddresses.Actions[1]
                == new UiShaderSourcePoolAcquisitionAction(
                    UiShaderSourcePoolAcquisitionActionKind.ReadPoolDivisorForDivision,
                    0x12CDE94,
                    3,
                    0x10)
            && wrappedAddresses.Remainder == 2);
    }
}
