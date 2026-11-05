using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public enum PayloadGrowthZeroFillEntry
{
    Fall,
    Taken
}

public readonly record struct PayloadGrowthZeroFillCapture(
    PayloadGrowthZeroFillEntry Entry,
    bool? PriorCallReturnedNormally,
    uint FramePointer,
    uint? RetainedEbx,
    uint? CallEax,
    uint? IncomingEdi,
    uint? RetainedEsi,
    uint? FrameDwordMinusC,
    uint? FrameDwordMinus8,
    uint? FrameDwordMinus4,
    bool? DirectionFlag,
    bool? FirstRepNormalCompletion,
    bool? FinalRepNormalCompletion);

public enum PayloadGrowthZeroFillOutcome
{
    InvalidEntry,
    PriorCallReturnStatusNotCaptured,
    PriorCallDidNotReturn,
    MissingFrameDwordMinusCCapture,
    MissingRetainedEbxCapture,
    MissingCallEaxCapture,
    MissingIncomingEdiCapture,
    MissingRetainedEsiCapture,
    MissingFrameDwordMinus8Capture,
    MissingFrameDwordMinus4Capture,
    DirectionFlagNotCaptured,
    FirstRepCompletionNotCaptured,
    FirstRepDidNotCompleteNormally,
    FinalRepCompletionNotCaptured,
    FinalRepDidNotCompleteNormally,
    UnreachedCaptureProvided,
    Completed
}

public readonly record struct PayloadGrowthZeroFillOperation(
    uint RawFileOffset,
    uint DwordCount,
    uint Destination,
    bool? DirectionFlag,
    bool? NormalCompletion)
{
    public const uint ZeroDwordValue = 0;
    public uint Value => ZeroDwordValue;

    public uint? EcxAfter => NormalCompletion == true ? 0u : null;

    public uint? FinalEdi => NormalCompletion == true
        && DirectionFlag is bool directionFlag
            ? Advance(Destination, DwordCount, directionFlag)
            : null;

    public uint AddressAt(uint index)
    {
        if (index >= DwordCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (DirectionFlag is not bool directionFlag)
        {
            throw new InvalidOperationException(
                "The REP direction flag was not captured.");
        }

        return Advance(Destination, index, directionFlag);
    }

    private static uint Advance(uint address, uint dwordCount, bool directionFlag)
    {
        uint byteCount = unchecked(dwordCount * 4);
        return directionFlag
            ? unchecked(address - byteCount)
            : unchecked(address + byteCount);
    }
}

public readonly record struct PayloadGrowthZeroFillPlan
{
    public PayloadGrowthZeroFillCapture Capture { get; init; }
    public PayloadGrowthZeroFillOutcome Outcome { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    public uint? StackCleanupBytes { get; init; }
    public uint? FrameDwordMinusCReadAddress { get; init; }
    public bool FrameDwordMinusCRead { get; init; }
    public uint? FrameDwordMinusCValue { get; init; }
    public uint? CallEaxUsed { get; init; }
    public uint? FirstRepDestination { get; init; }
    public uint? FirstRepCount { get; init; }
    public PayloadGrowthZeroFillOperation? FirstRep { get; init; }
    public uint? FrameDwordMinus8ReadAddress { get; init; }
    public bool FrameDwordMinus8Read { get; init; }
    public uint? FrameDwordMinus8Value { get; init; }
    public uint? EdiBeforeFinalRep { get; init; }

    public uint? TakenEdiAfterLea { get; init; }
    public uint? FrameDwordMinus4ReadAddress { get; init; }
    public bool FrameDwordMinus4Read { get; init; }
    public uint? FrameDwordMinus4Value { get; init; }
    public uint? EbxAfterFrameDwordMinus4Reload { get; init; }
    public uint? FinalRepDestination { get; init; }
    public uint? FinalRepCount { get; init; }
    public PayloadGrowthZeroFillOperation? FinalRep { get; init; }
    public uint? NormalBoundaryRawFileOffsetExclusive { get; init; }
}

public enum PayloadGrowthZeroFillInstructionPath
{
    Fall,
    Taken
}

public readonly record struct PayloadGrowthZeroFillInstructionEvidence(
    PayloadGrowthZeroFillInstructionPath Path,
    string Observation,
    uint RawFileOffset,
    string Bytes);

/// <summary>
/// Projects the normal fall and taken continuations through their conditional
/// DWORD zero-fill operations. REP plans describe address progression only;
/// they make no claim that any memory address is valid or was written.
/// </summary>
public static class PayloadGrowthZeroFillProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint FallStartRawFileOffset = 0x00BE6984;
    public const uint FallEndRawFileOffsetExclusive = 0x00BE69A7;
    public const uint TakenStartRawFileOffset = 0x00BE69DF;
    public const uint TakenEndRawFileOffsetExclusive = 0x00BE69F2;
    public const uint SharedStartRawFileOffset = 0x00BE69EE;
    public const uint SharedEndRawFileOffsetExclusive = 0x00BE69F2;

    public const uint FallFrameDwordMinusCReadRawFileOffset = 0x00BE6984;
    public const uint FallStackCleanupRawFileOffset = 0x00BE6987;
    public const uint FallCallEaxUseRawFileOffset = 0x00BE698A;
    public const uint FallFirstRepRawFileOffset = 0x00BE6995;
    public const uint FallFrameDwordMinus8ReadRawFileOffset = 0x00BE6997;
    public const uint FallFrameDwordMinus4ReadRawFileOffset = 0x00BE699A;
    public const uint FallTestEdiRawFileOffset = 0x00BE699D;
    public const uint FallFinalRepRawFileOffset = 0x00BE69F0;
    public const uint FallNormalBoundaryRawFileOffsetExclusive = 0x00BE69F2;

    public const uint TakenStackCleanupRawFileOffset = 0x00BE69DF;
    public const uint TakenCallEaxUseRawFileOffset = 0x00BE69E2;
    public const uint TakenFrameDwordMinus4ReadRawFileOffset = 0x00BE69E5;
    public const uint TakenFinalRepRawFileOffset = 0x00BE69F0;
    public const uint TakenNormalBoundaryRawFileOffsetExclusive = 0x00BE69F2;

    public const uint FrameDwordMinusCOffset = 0x0C;
    public const uint FrameDwordMinus8Offset = 0x08;
    public const uint FrameDwordMinus4Offset = 0x04;
    public const uint FallStackCleanupBytes = 0x0C;
    public const uint TakenStackCleanupBytes = 0x18;

    public const string FallCapturedCodeBytes =
        "8B 4D F4 83 C4 0C 8D 14 03 85 C9 74 09 33 C0 8B FA "
        + "F3 AB 8B 7D F8 8B 5D FC 85 FF 74 51 8B CF 8B FB EB 47";
    public const string TakenCapturedCodeBytes =
        "83 C4 18 8D 3C 03 8B 5D FC 85 F6 74 06 8B CE 33 C0 F3 AB";
    public const string SharedCodeBytes = "33 C0 F3 AB";

    public static ReadOnlyCollection<PayloadGrowthZeroFillInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadGrowthZeroFillInstructionEvidence(
                PayloadGrowthZeroFillInstructionPath.Fall,
                "Read framePointer-0x0C before stack cleanup",
                FallFrameDwordMinusCReadRawFileOffset,
                "8B 4D F4"),
            new PayloadGrowthZeroFillInstructionEvidence(
                PayloadGrowthZeroFillInstructionPath.Fall,
                "Clean up three prior call arguments",
                FallStackCleanupRawFileOffset,
                "83 C4 0C"),
            new PayloadGrowthZeroFillInstructionEvidence(
                PayloadGrowthZeroFillInstructionPath.Fall,
                "Form EDX from retained EBX plus prior-call EAX",
                FallCallEaxUseRawFileOffset,
                "8D 14 03"),
            new PayloadGrowthZeroFillInstructionEvidence(
                PayloadGrowthZeroFillInstructionPath.Fall,
                "Test frame-C count; a nonzero count zeros DWORDs at EDX",
                0x00BE698D,
                "85 C9 74 09 33 C0 8B FA F3 AB"),
            new PayloadGrowthZeroFillInstructionEvidence(
                PayloadGrowthZeroFillInstructionPath.Fall,
                "Reload frame-8 into EDI only after a nonzero first REP",
                FallFrameDwordMinus8ReadRawFileOffset,
                "8B 7D F8"),
            new PayloadGrowthZeroFillInstructionEvidence(
                PayloadGrowthZeroFillInstructionPath.Fall,
                "Reload frame-4 into EBX, then test EDI and select the final REP",
                FallFrameDwordMinus4ReadRawFileOffset,
                "8B 5D FC 85 FF 74 51 8B CF 8B FB EB 47"),
            new PayloadGrowthZeroFillInstructionEvidence(
                PayloadGrowthZeroFillInstructionPath.Taken,
                "Clean up six prior call arguments",
                TakenStackCleanupRawFileOffset,
                "83 C4 18"),
            new PayloadGrowthZeroFillInstructionEvidence(
                PayloadGrowthZeroFillInstructionPath.Taken,
                "Form EDI from retained EBX plus prior-call EAX",
                TakenCallEaxUseRawFileOffset,
                "8D 3C 03"),
            new PayloadGrowthZeroFillInstructionEvidence(
                PayloadGrowthZeroFillInstructionPath.Taken,
                "Reload frame-4 into EBX before testing retained ESI",
                TakenFrameDwordMinus4ReadRawFileOffset,
                "8B 5D FC 85 F6 74 06 8B CE"),
            new PayloadGrowthZeroFillInstructionEvidence(
                PayloadGrowthZeroFillInstructionPath.Taken,
                "Set EAX to zero and REP STOSD at shared raw 0xBE69EE",
                SharedStartRawFileOffset,
                SharedCodeBytes)
        });

    public static uint FrameDwordAddress(uint framePointer, uint offset) =>
        unchecked(framePointer - offset);

    public static PayloadGrowthZeroFillPlan Project(
        PayloadGrowthZeroFillCapture capture)
    {
        if (capture.Entry is not PayloadGrowthZeroFillEntry.Fall
            and not PayloadGrowthZeroFillEntry.Taken)
        {
            return Stop(capture, PayloadGrowthZeroFillOutcome.InvalidEntry,
                capture.Entry == PayloadGrowthZeroFillEntry.Fall
                    ? FallStartRawFileOffset
                    : TakenStartRawFileOffset);
        }

        uint start = capture.Entry == PayloadGrowthZeroFillEntry.Fall
            ? FallStartRawFileOffset
            : TakenStartRawFileOffset;
        if (capture.PriorCallReturnedNormally is not bool returnedNormally)
        {
            return Stop(
                capture,
                HasPostCallCapture(capture)
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.PriorCallReturnStatusNotCaptured,
                start);
        }

        if (!returnedNormally)
        {
            return Stop(
                capture,
                HasPostCallCapture(capture)
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.PriorCallDidNotReturn,
                start);
        }

        return capture.Entry == PayloadGrowthZeroFillEntry.Fall
            ? ProjectFall(capture)
            : ProjectTaken(capture);
    }

    private static PayloadGrowthZeroFillPlan ProjectFall(
        PayloadGrowthZeroFillCapture capture)
    {
        uint frameMinusCAddress = FrameDwordAddress(
            capture.FramePointer,
            FrameDwordMinusCOffset);
        if (capture.FrameDwordMinusC is not uint frameMinusC)
        {
            return Stop(
                capture,
                capture.RetainedEbx is not null
                    || capture.CallEax is not null
                    || HasPostCallCapture(capture)
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.MissingFrameDwordMinusCCapture,
                FallFrameDwordMinusCReadRawFileOffset) with
            {
                FrameDwordMinusCReadAddress = frameMinusCAddress
            };
        }

        PayloadGrowthZeroFillPlan plan = new()
        {
            Capture = capture,
            FrameDwordMinusCReadAddress = frameMinusCAddress,
            FrameDwordMinusCRead = true,
            FrameDwordMinusCValue = frameMinusC,
            StackCleanupBytes = FallStackCleanupBytes
        };
        if (capture.RetainedEbx is not uint retainedEbx)
        {
            return StopFall(plan,
                HasFallCaptureAfterCallEax(capture)
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.MissingRetainedEbxCapture,
                FallCallEaxUseRawFileOffset);
        }

        if (capture.CallEax is not uint callEax)
        {
            return StopFall(plan,
                HasFallCaptureAfterCallEax(capture)
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.MissingCallEaxCapture,
                FallCallEaxUseRawFileOffset);
        }

        uint firstRepDestination = unchecked(retainedEbx + callEax);
        plan = plan with
        {
            CallEaxUsed = callEax,
            FirstRepDestination = frameMinusC == 0 ? null : firstRepDestination,
            FirstRepCount = frameMinusC == 0 ? null : frameMinusC
        };

        if (frameMinusC != 0)
        {
            if (HasFallCaptureBeforeFirstRep(capture))
            {
                return StopFall(plan,
                    PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided,
                    FallFirstRepRawFileOffset);
            }

            PayloadGrowthZeroFillOperation firstRep = new(
                FallFirstRepRawFileOffset,
                frameMinusC,
                firstRepDestination,
                capture.DirectionFlag,
                capture.FirstRepNormalCompletion);
            plan = plan with { FirstRep = firstRep };

            if (capture.DirectionFlag is null)
            {
                return StopFall(plan,
                    capture.FrameDwordMinus8 is not null
                        || capture.FrameDwordMinus4 is not null
                        || capture.FinalRepNormalCompletion is not null
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.DirectionFlagNotCaptured,
                    FallFirstRepRawFileOffset);
            }

            if (capture.FirstRepNormalCompletion is not bool firstRepCompleted)
            {
                return StopFall(plan,
                    capture.FinalRepNormalCompletion is not null
                        || capture.FrameDwordMinus8 is not null
                        || capture.FrameDwordMinus4 is not null
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.FirstRepCompletionNotCaptured,
                    FallFirstRepRawFileOffset);
            }

            if (!firstRepCompleted)
            {
                return StopFall(plan,
                    capture.FrameDwordMinus8 is not null
                        || capture.FrameDwordMinus4 is not null
                        || capture.FinalRepNormalCompletion is not null
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.FirstRepDidNotCompleteNormally,
                    FallFirstRepRawFileOffset);
            }

            firstRep = firstRep with { NormalCompletion = true };
            plan = plan with { FirstRep = firstRep };

            uint frameMinus8Address = FrameDwordAddress(
                capture.FramePointer,
                FrameDwordMinus8Offset);
            plan = plan with { FrameDwordMinus8ReadAddress = frameMinus8Address };
            if (capture.FrameDwordMinus8 is not uint frameMinus8)
            {
                return StopFall(plan,
                    capture.FrameDwordMinus4 is not null
                        || capture.FinalRepNormalCompletion is not null
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.MissingFrameDwordMinus8Capture,
                    FallFrameDwordMinus8ReadRawFileOffset);
            }

            plan = plan with
            {
                FrameDwordMinus8Read = true,
                FrameDwordMinus8Value = frameMinus8,
                EdiBeforeFinalRep = frameMinus8
            };
        }
        else
        {
            if (HasFallCaptureAfterSkippedFirstRep(capture))
            {
                return StopFall(plan,
                    PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided,
                    FallFrameDwordMinus4ReadRawFileOffset);
            }
        }

        uint frameMinus4Address = FrameDwordAddress(
            capture.FramePointer,
            FrameDwordMinus4Offset);
        plan = plan with { FrameDwordMinus4ReadAddress = frameMinus4Address };
        if (capture.FrameDwordMinus4 is not uint frameMinus4)
        {
            return StopFall(plan,
                capture.FinalRepNormalCompletion is not null
                    || capture.DirectionFlag is not null && frameMinusC == 0
                ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                : PayloadGrowthZeroFillOutcome.MissingFrameDwordMinus4Capture,
                FallFrameDwordMinus4ReadRawFileOffset);
        }

        plan = plan with
        {
            FrameDwordMinus4ReadAddress = frameMinus4Address,
            FrameDwordMinus4Read = true,
            FrameDwordMinus4Value = frameMinus4,
            EbxAfterFrameDwordMinus4Reload = frameMinus4
        };

        if (frameMinusC == 0)
        {
            if (capture.IncomingEdi is not uint incomingEdi)
            {
                return StopFall(plan,
                    capture.DirectionFlag is not null
                        || capture.FinalRepNormalCompletion is not null
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.MissingIncomingEdiCapture,
                    FallTestEdiRawFileOffset);
            }

            plan = plan with { EdiBeforeFinalRep = incomingEdi };
        }

        uint ediForFinalRep = plan.EdiBeforeFinalRep!.Value;
        if (ediForFinalRep == 0)
        {
            if (frameMinusC == 0 && capture.DirectionFlag is not null
                || capture.FinalRepNormalCompletion is not null)
            {
                return StopFall(plan,
                    PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided,
                    FallNormalBoundaryRawFileOffsetExclusive);
            }

            return Complete(plan, FallNormalBoundaryRawFileOffsetExclusive);
        }

        PayloadGrowthZeroFillOperation finalRep = new(
            FallFinalRepRawFileOffset,
            ediForFinalRep,
            frameMinus4,
            capture.DirectionFlag,
            capture.FinalRepNormalCompletion);
        plan = plan with
        {
            FinalRepDestination = frameMinus4,
            FinalRepCount = ediForFinalRep,
            FinalRep = finalRep
        };
        if (capture.DirectionFlag is null)
        {
            return StopFall(plan,
                capture.FinalRepNormalCompletion is not null
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.DirectionFlagNotCaptured,
                FallFinalRepRawFileOffset);
        }

        if (capture.FinalRepNormalCompletion is not bool finalRepCompleted)
        {
            return StopFall(plan,
                PayloadGrowthZeroFillOutcome.FinalRepCompletionNotCaptured,
                FallFinalRepRawFileOffset);
        }

        if (!finalRepCompleted)
        {
            return StopFall(plan,
                PayloadGrowthZeroFillOutcome.FinalRepDidNotCompleteNormally,
                FallFinalRepRawFileOffset);
        }

        finalRep = finalRep with { NormalCompletion = true };
        return Complete(
            plan with { FinalRep = finalRep },
            FallNormalBoundaryRawFileOffsetExclusive);
    }

    private static PayloadGrowthZeroFillPlan ProjectTaken(
        PayloadGrowthZeroFillCapture capture)
    {
        PayloadGrowthZeroFillPlan plan = new()
        {
            Capture = capture,
            StackCleanupBytes = TakenStackCleanupBytes
        };
        if (HasTakenCaptureBeforeEbxOrEax(capture))
        {
            return StopTaken(plan,
                PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided,
                TakenCallEaxUseRawFileOffset);
        }

        if (capture.RetainedEbx is not uint retainedEbx)
        {
            return StopTaken(plan,
                HasTakenCaptureAfterLea(capture)
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.MissingRetainedEbxCapture,
                TakenCallEaxUseRawFileOffset);
        }

        if (capture.CallEax is not uint callEax)
        {
            return StopTaken(plan,
                HasTakenCaptureAfterLea(capture)
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.MissingCallEaxCapture,
                TakenCallEaxUseRawFileOffset);
        }

        uint takenEdi = unchecked(retainedEbx + callEax);
        uint frameMinus4Address = FrameDwordAddress(
            capture.FramePointer,
            FrameDwordMinus4Offset);
        plan = plan with
        {
            CallEaxUsed = callEax,
            TakenEdiAfterLea = takenEdi,
            FrameDwordMinus4ReadAddress = frameMinus4Address
        };

        if (capture.FrameDwordMinus4 is not uint frameMinus4)
        {
            return StopTaken(plan,
                HasTakenCaptureAfterFrameMinus4(capture)
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.MissingFrameDwordMinus4Capture,
                TakenFrameDwordMinus4ReadRawFileOffset);
        }

        plan = plan with
        {
            FrameDwordMinus4Read = true,
            FrameDwordMinus4Value = frameMinus4,
            EbxAfterFrameDwordMinus4Reload = frameMinus4
        };

        if (capture.RetainedEsi is not uint retainedEsi)
        {
            return StopTaken(plan,
                capture.DirectionFlag is not null
                    || capture.FinalRepNormalCompletion is not null
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.MissingRetainedEsiCapture,
                0x00BE69E8);
        }

        if (retainedEsi == 0)
        {
            if (capture.DirectionFlag is not null
                || capture.FinalRepNormalCompletion is not null)
            {
                return StopTaken(plan,
                    PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided,
                    TakenNormalBoundaryRawFileOffsetExclusive);
            }

            return Complete(plan, TakenNormalBoundaryRawFileOffsetExclusive);
        }

        PayloadGrowthZeroFillOperation finalRep = new(
            TakenFinalRepRawFileOffset,
            retainedEsi,
            takenEdi,
            capture.DirectionFlag,
            capture.FinalRepNormalCompletion);
        plan = plan with
        {
            FinalRepDestination = takenEdi,
            FinalRepCount = retainedEsi,
            FinalRep = finalRep
        };
        if (capture.DirectionFlag is null)
        {
            return StopTaken(plan,
                capture.FinalRepNormalCompletion is not null
                    ? PayloadGrowthZeroFillOutcome.UnreachedCaptureProvided
                    : PayloadGrowthZeroFillOutcome.DirectionFlagNotCaptured,
                TakenFinalRepRawFileOffset);
        }

        if (capture.FinalRepNormalCompletion is not bool finalRepCompleted)
        {
            return StopTaken(plan,
                PayloadGrowthZeroFillOutcome.FinalRepCompletionNotCaptured,
                TakenFinalRepRawFileOffset);
        }

        if (!finalRepCompleted)
        {
            return StopTaken(plan,
                PayloadGrowthZeroFillOutcome.FinalRepDidNotCompleteNormally,
                TakenFinalRepRawFileOffset);
        }

        finalRep = finalRep with { NormalCompletion = true };
        return Complete(
            plan with { FinalRep = finalRep },
            TakenNormalBoundaryRawFileOffsetExclusive);
    }

    private static PayloadGrowthZeroFillPlan Stop(
        PayloadGrowthZeroFillCapture capture,
        PayloadGrowthZeroFillOutcome outcome,
        uint stopAt) =>
        new()
        {
            Capture = capture,
            Outcome = outcome,
            StopAtRawFileOffsetExclusive = stopAt
        };

    private static PayloadGrowthZeroFillPlan StopFall(
        PayloadGrowthZeroFillPlan plan,
        PayloadGrowthZeroFillOutcome outcome,
        uint stopAt) =>
        plan with
        {
            Outcome = outcome,
            StopAtRawFileOffsetExclusive = stopAt
        };

    private static PayloadGrowthZeroFillPlan StopTaken(
        PayloadGrowthZeroFillPlan plan,
        PayloadGrowthZeroFillOutcome outcome,
        uint stopAt) =>
        plan with
        {
            Outcome = outcome,
            StopAtRawFileOffsetExclusive = stopAt
        };

    private static PayloadGrowthZeroFillPlan Complete(
        PayloadGrowthZeroFillPlan plan,
        uint boundary) =>
        plan with
        {
            Outcome = PayloadGrowthZeroFillOutcome.Completed,
            NormalBoundaryRawFileOffsetExclusive = boundary,
            StopAtRawFileOffsetExclusive = boundary
        };

    private static bool HasPostCallCapture(PayloadGrowthZeroFillCapture capture) =>
        capture.RetainedEbx is not null
        || capture.CallEax is not null
        || capture.IncomingEdi is not null
        || capture.RetainedEsi is not null
        || capture.FrameDwordMinusC is not null
        || capture.FrameDwordMinus8 is not null
        || capture.FrameDwordMinus4 is not null
        || capture.DirectionFlag is not null
        || capture.FirstRepNormalCompletion is not null
        || capture.FinalRepNormalCompletion is not null;

    private static bool HasFallCaptureAfterCallEax(
        PayloadGrowthZeroFillCapture capture) =>
        capture.IncomingEdi is not null
        || capture.RetainedEsi is not null
        || capture.FrameDwordMinus8 is not null
        || capture.FrameDwordMinus4 is not null
        || capture.DirectionFlag is not null
        || capture.FirstRepNormalCompletion is not null
        || capture.FinalRepNormalCompletion is not null;

    private static bool HasFallCaptureBeforeFirstRep(
        PayloadGrowthZeroFillCapture capture) =>
        capture.IncomingEdi is not null
        || capture.RetainedEsi is not null;

    private static bool HasFallCaptureAfterSkippedFirstRep(
        PayloadGrowthZeroFillCapture capture) =>
        capture.FrameDwordMinus8 is not null
        || capture.FirstRepNormalCompletion is not null
        || capture.RetainedEsi is not null;

    private static bool HasTakenCaptureBeforeEbxOrEax(
        PayloadGrowthZeroFillCapture capture) =>
        capture.IncomingEdi is not null
        || capture.FrameDwordMinusC is not null
        || capture.FrameDwordMinus8 is not null
        || capture.FirstRepNormalCompletion is not null;

    private static bool HasTakenCaptureAfterLea(
        PayloadGrowthZeroFillCapture capture) =>
        capture.FrameDwordMinus4 is not null
        || capture.RetainedEsi is not null
        || capture.DirectionFlag is not null
        || capture.FinalRepNormalCompletion is not null;

    private static bool HasTakenCaptureAfterFrameMinus4(
        PayloadGrowthZeroFillCapture capture) =>
        capture.IncomingEdi is not null
        || capture.RetainedEsi is not null
        || capture.FrameDwordMinusC is not null
        || capture.FrameDwordMinus8 is not null
        || capture.FirstRepNormalCompletion is not null
        || capture.DirectionFlag is not null
        || capture.FinalRepNormalCompletion is not null;
}
