using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadGrowthBranchCopyCapture(
    uint RetainedEdi,
    uint GrowthDeltaEsi,
    uint ReceiverDword4ReloadEcx,
    uint SavedPointerEdx,
    uint FramePointer,
    uint? CallTargetDword,
    uint? FrameDwordMinus10Reload,
    bool? CallReturnedNormally);

public enum PayloadGrowthBranchCopyOutcome
{
    MissingCallTargetCapture,
    MissingFrameDwordMinus10Capture,
    UnreachedCaptureProvided,
    CallReturnStatusNotCaptured,
    CallDidNotReturn,
    ReturnedNormally
}

public readonly record struct PayloadGrowthBranchCopyPlan
{
    public PayloadGrowthBranchCopyCapture Capture { get; init; }
    public PayloadGrowthBranchCopyOutcome Outcome { get; init; }
    public bool TakenBranch { get; init; }
    public bool UnsignedGreaterThan { get; init; }
    public uint RouteRawFileOffset { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }

    public uint? FallScaledEax { get; init; }
    public int? FallSignedArithmeticShiftResult { get; init; }
    public uint? FallSignedArithmeticShiftResultBits { get; init; }
    public uint? FallEdxAfterSubtract { get; init; }
    public uint? FrameDwordMinusCWriteAddress { get; init; }
    public uint? FrameDwordMinusCWriteValue { get; init; }
    public uint? FallScaledEbx { get; init; }

    public uint? TakenScaledEdi { get; init; }
    public uint? TakenMaskedEax { get; init; }

    public uint? CallTargetReadRawFileOffset { get; init; }
    public bool CallTargetRead { get; init; }
    public uint? CallTargetDword { get; init; }

    public uint? FrameDwordMinus10ReadAddress { get; init; }
    public bool FrameDwordMinus10Read { get; init; }
    public uint? FrameDwordMinus10ReadValue { get; init; }

    public bool CountPushed { get; init; }
    public bool SourcePushed { get; init; }
    public bool DestinationPushed { get; init; }
    public uint? CountStackArgument { get; init; }
    public uint? SourceStackArgument { get; init; }
    public uint? DestinationStackArgument { get; init; }
    public uint? CallEntryEsp4 { get; init; }
    public uint? CallEntryEsp8 { get; init; }
    public uint? CallEntryEspC { get; init; }

    public uint CallRawFileOffset { get; init; }
    public bool CallReached { get; init; }
    public bool? CallReturnedNormally { get; init; }
    public uint? NormalBoundaryRawFileOffsetExclusive { get; init; }
}

public enum PayloadGrowthBranchCopyPath
{
    Fall,
    Taken
}

public readonly record struct PayloadGrowthBranchCopyInstructionEvidence(
    PayloadGrowthBranchCopyPath Path,
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the two branch-local argument setups through their first opaque
/// calls. The call-target capture records only the DWORD loaded from the
/// absolute slot; it does not identify or validate the pointed-to target.
/// </summary>
public static class PayloadGrowthBranchCopyProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint CallTargetDwordAddress = 0x01AC489C;
    public const uint FrameDwordMinusCOffset = 0x0C;
    public const uint FrameDwordMinus10Offset = 0x10;
    public const uint TakenCountMask = 0xFFFFFFFC;

    public const uint TakenBranchRawFileOffset = 0x00BE69A7;
    public const uint FallBranchRawFileOffset = 0x00BE6960;
    public const uint FallCallTargetReadRawFileOffset = 0x00BE6978;
    public const uint FallFrameDwordMinus10ReadRawFileOffset = 0x00BE697F;
    public const uint FallCallRawFileOffset = 0x00BE6982;
    public const uint FallNormalBoundaryRawFileOffsetExclusive = 0x00BE6984;
    public const uint TakenCallTargetReadRawFileOffset = 0x00BE69B4;
    public const uint TakenCallRawFileOffset = 0x00BE69BB;
    public const uint TakenNormalBoundaryRawFileOffsetExclusive = 0x00BE69BD;

    public const string FallCapturedCodeBytes =
        "8D 04 BD 00 00 00 00 8B D6 C1 F8 02 2B D7 89 55 F4 "
        + "8D 1C 85 00 00 00 00 A1 9C 48 AC 01 53 51 FF 75 F0 FF D0";
    public const string TakenCapturedCodeBytes =
        "8D 3C B5 00 00 00 00 8B C7 83 E0 FC 50 A1 9C 48 AC 01 51 52 FF D0";

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FrameDwordAddress(uint framePointer, uint offset) =>
        unchecked(framePointer - offset);

    public static uint ScaleByFour(uint value) =>
        unchecked(value * 4);

    public static ReadOnlyCollection<PayloadGrowthBranchCopyInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Fall,
                "LEA EAX,[EDI*4] with DWORD wrap",
                0x00BE6960,
                0x00FE7560,
                "8D 04 BD 00 00 00 00"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Fall,
                "Copy ESI to EDX, then arithmetic-shift EAX right by two",
                0x00BE6967,
                0x00FE7567,
                "8B D6 C1 F8 02"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Fall,
                "Subtract retained EDI from EDX with DWORD wrap",
                0x00BE696C,
                0x00FE756C,
                "2B D7"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Fall,
                "Write EDX to framePointer-0x0C",
                0x00BE696E,
                0x00FE756E,
                "89 55 F4"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Fall,
                "LEA EBX,[EAX*4] with DWORD wrap",
                0x00BE6971,
                0x00FE7571,
                "8D 1C 85 00 00 00 00"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Fall,
                "Read the independent call-target DWORD from the absolute slot",
                FallCallTargetReadRawFileOffset,
                RawFileOffsetToVirtualAddress(FallCallTargetReadRawFileOffset),
                "A1 9C 48 AC 01"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Fall,
                "Push count, then source",
                0x00BE697D,
                0x00FE757D,
                "53 51"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Fall,
                "Independently reload framePointer-0x10 as the destination and push it",
                FallFrameDwordMinus10ReadRawFileOffset,
                RawFileOffsetToVirtualAddress(FallFrameDwordMinus10ReadRawFileOffset),
                "FF 75 F0"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Fall,
                "Call through the captured EAX; normal continuation begins at raw 0xBE6984",
                FallCallRawFileOffset,
                RawFileOffsetToVirtualAddress(FallCallRawFileOffset),
                "FF D0"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Taken,
                "LEA EDI,[ESI*4] with DWORD wrap",
                TakenBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(TakenBranchRawFileOffset),
                "8D 3C B5 00 00 00 00"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Taken,
                "Copy EDI to EAX and clear its low two bits",
                0x00BE69AE,
                0x00FE75AE,
                "8B C7 83 E0 FC"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Taken,
                "Push count, then independently read the call-target DWORD",
                0x00BE69B3,
                0x00FE75B3,
                "50 A1 9C 48 AC 01"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Taken,
                "Push ECX source, then retained EDX destination",
                0x00BE69B9,
                0x00FE75B9,
                "51 52"),
            new PayloadGrowthBranchCopyInstructionEvidence(
                PayloadGrowthBranchCopyPath.Taken,
                "Call through the captured EAX; normal continuation begins at raw 0xBE69BD",
                TakenCallRawFileOffset,
                RawFileOffsetToVirtualAddress(TakenCallRawFileOffset),
                "FF D0")
        });

    public static PayloadGrowthBranchCopyPlan Project(
        PayloadGrowthBranchCopyCapture capture)
    {
        bool takenBranch = capture.RetainedEdi > capture.GrowthDeltaEsi;
        uint routeRawFileOffset = takenBranch
            ? TakenBranchRawFileOffset
            : FallBranchRawFileOffset;
        PayloadGrowthBranchCopyPlan plan = new()
        {
            Capture = capture,
            TakenBranch = takenBranch,
            UnsignedGreaterThan = takenBranch,
            RouteRawFileOffset = routeRawFileOffset,
            StopAtRawFileOffsetExclusive = routeRawFileOffset
        };

        return takenBranch
            ? ProjectTaken(capture, plan)
            : ProjectFall(capture, plan);
    }

    private static PayloadGrowthBranchCopyPlan ProjectFall(
        PayloadGrowthBranchCopyCapture capture,
        PayloadGrowthBranchCopyPlan plan)
    {
        uint scaledEax = ScaleByFour(capture.RetainedEdi);
        int signedShift = unchecked((int)scaledEax) >> 2;
        uint signedShiftBits = unchecked((uint)signedShift);
        uint edxAfterSubtract = unchecked(capture.GrowthDeltaEsi - capture.RetainedEdi);
        uint frameMinusCAddress = FrameDwordAddress(
            capture.FramePointer,
            FrameDwordMinusCOffset);
        uint scaledEbx = ScaleByFour(signedShiftBits);

        plan = plan with
        {
            FallScaledEax = scaledEax,
            FallSignedArithmeticShiftResult = signedShift,
            FallSignedArithmeticShiftResultBits = signedShiftBits,
            FallEdxAfterSubtract = edxAfterSubtract,
            FrameDwordMinusCWriteAddress = frameMinusCAddress,
            FrameDwordMinusCWriteValue = edxAfterSubtract,
            FallScaledEbx = scaledEbx
        };

        if (capture.CallTargetDword is not uint callTargetDword)
        {
            return plan with
            {
                Outcome = capture.FrameDwordMinus10Reload is not null
                    || capture.CallReturnedNormally is not null
                    ? PayloadGrowthBranchCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthBranchCopyOutcome.MissingCallTargetCapture,
                StopAtRawFileOffsetExclusive = FallCallTargetReadRawFileOffset
            };
        }

        uint frameMinus10ReadAddress = FrameDwordAddress(
            capture.FramePointer,
            FrameDwordMinus10Offset);
        plan = plan with
        {
            CallTargetReadRawFileOffset = FallCallTargetReadRawFileOffset,
            CallTargetRead = true,
            CallTargetDword = callTargetDword,
            FrameDwordMinus10ReadAddress = frameMinus10ReadAddress,
            CountPushed = true,
            SourcePushed = true,
            CountStackArgument = scaledEbx,
            SourceStackArgument = capture.ReceiverDword4ReloadEcx
        };

        if (capture.FrameDwordMinus10Reload is not uint frameMinus10Reload)
        {
            return plan with
            {
                Outcome = capture.CallReturnedNormally is not null
                    ? PayloadGrowthBranchCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthBranchCopyOutcome.MissingFrameDwordMinus10Capture,
                StopAtRawFileOffsetExclusive = FallFrameDwordMinus10ReadRawFileOffset
            };
        }

        plan = plan with
        {
            FrameDwordMinus10Read = true,
            FrameDwordMinus10ReadValue = frameMinus10Reload,
            DestinationPushed = true,
            DestinationStackArgument = frameMinus10Reload,
            CallEntryEsp4 = frameMinus10Reload,
            CallEntryEsp8 = capture.ReceiverDword4ReloadEcx,
            CallEntryEspC = scaledEbx
        };

        return FinishCall(
            plan,
            capture.CallReturnedNormally,
            FallCallRawFileOffset,
            FallNormalBoundaryRawFileOffsetExclusive,
            hasUnreachedCapture: false);
    }

    private static PayloadGrowthBranchCopyPlan ProjectTaken(
        PayloadGrowthBranchCopyCapture capture,
        PayloadGrowthBranchCopyPlan plan)
    {
        uint scaledEdi = ScaleByFour(capture.GrowthDeltaEsi);
        uint maskedEax = scaledEdi & TakenCountMask;
        plan = plan with
        {
            TakenScaledEdi = scaledEdi,
            TakenMaskedEax = maskedEax,
            CountPushed = true,
            CountStackArgument = maskedEax
        };

        if (capture.CallTargetDword is not uint callTargetDword)
        {
            return plan with
            {
                Outcome = capture.FrameDwordMinus10Reload is not null
                    || capture.CallReturnedNormally is not null
                    ? PayloadGrowthBranchCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthBranchCopyOutcome.MissingCallTargetCapture,
                StopAtRawFileOffsetExclusive = TakenCallTargetReadRawFileOffset
            };
        }

        plan = plan with
        {
            CallTargetReadRawFileOffset = TakenCallTargetReadRawFileOffset,
            CallTargetRead = true,
            CallTargetDword = callTargetDword,
            SourcePushed = true,
            DestinationPushed = true,
            SourceStackArgument = capture.ReceiverDword4ReloadEcx,
            DestinationStackArgument = capture.SavedPointerEdx,
            CallEntryEsp4 = capture.SavedPointerEdx,
            CallEntryEsp8 = capture.ReceiverDword4ReloadEcx,
            CallEntryEspC = maskedEax
        };

        return FinishCall(
            plan,
            capture.CallReturnedNormally,
            TakenCallRawFileOffset,
            TakenNormalBoundaryRawFileOffsetExclusive,
            hasUnreachedCapture: capture.FrameDwordMinus10Reload is not null);
    }

    private static PayloadGrowthBranchCopyPlan FinishCall(
        PayloadGrowthBranchCopyPlan plan,
        bool? callReturnedNormally,
        uint callRawFileOffset,
        uint normalBoundaryRawFileOffsetExclusive,
        bool hasUnreachedCapture)
    {
        if (callReturnedNormally is not bool returnedNormally)
        {
            return plan with
            {
                Outcome = hasUnreachedCapture
                    ? PayloadGrowthBranchCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthBranchCopyOutcome.CallReturnStatusNotCaptured,
                CallRawFileOffset = callRawFileOffset,
                CallReached = true,
                StopAtRawFileOffsetExclusive = callRawFileOffset
            };
        }

        if (!returnedNormally)
        {
            return plan with
            {
                Outcome = hasUnreachedCapture
                    ? PayloadGrowthBranchCopyOutcome.UnreachedCaptureProvided
                    : PayloadGrowthBranchCopyOutcome.CallDidNotReturn,
                CallRawFileOffset = callRawFileOffset,
                CallReached = true,
                CallReturnedNormally = false,
                StopAtRawFileOffsetExclusive = callRawFileOffset
            };
        }

        return plan with
        {
            Outcome = hasUnreachedCapture
                ? PayloadGrowthBranchCopyOutcome.UnreachedCaptureProvided
                : PayloadGrowthBranchCopyOutcome.ReturnedNormally,
            CallRawFileOffset = callRawFileOffset,
            CallReached = true,
            CallReturnedNormally = true,
            NormalBoundaryRawFileOffsetExclusive = hasUnreachedCapture
                ? null
                : normalBoundaryRawFileOffsetExclusive,
            StopAtRawFileOffsetExclusive = hasUnreachedCapture
                ? callRawFileOffset
                : normalBoundaryRawFileOffsetExclusive
        };
    }
}
