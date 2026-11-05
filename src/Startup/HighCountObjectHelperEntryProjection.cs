using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct HighCountObjectHelperEntryCapture(
    uint FrameEbpAfterPrologue,
    uint EntryEcxReceiver,
    bool? HelperReturnedNormally,
    uint? HelperReturnEax,
    uint? DwordAtHelperEax);

public enum HighCountObjectHelperEntryOutcome
{
    MissingHelperReturnStatus,
    HelperDidNotReturnNormally,
    MissingHelperReturnEax,
    MissingDwordAtHelperEax,
    ZeroLoadedDwordBranchesToBoundary,
    NonzeroLoadedDwordFallsThrough,
    UnreachedCaptureProvided
}

public readonly record struct HighCountObjectHelperEntryPlan(
    HighCountObjectHelperEntryCapture Capture,
    HighCountObjectHelperEntryOutcome Outcome,
    uint LocalArgumentAddress,
    bool LocalArgumentPointerPushed,
    bool LocalArgumentReadBeforeHelperCall,
    bool HelperCallReached,
    uint HelperEntryEcxReceiver,
    bool? HelperReturnedNormally,
    bool HelperReturnEaxCaptured,
    uint? HelperReturnEax,
    bool DwordReadReached,
    uint? DwordReadAddress,
    bool DwordAtHelperEaxCaptured,
    uint? DwordAtHelperEax,
    bool ZeroTestProjected,
    bool? ZeroBranchTaken,
    uint? NextRawFileOffset,
    bool UnreachedHelperReturnEaxCaptureProvided,
    bool UnreachedDwordAtHelperEaxCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct HighCountObjectHelperEntryInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null,
    uint? CallTargetRawFileOffset = null,
    uint? CallTargetVirtualAddress = null);

/// <summary>
/// Projects the first helper call and the zero test of its returned object pointer.
/// The helper is opaque; no later body or object ownership behavior is modeled.
/// </summary>
public static class HighCountObjectHelperEntryProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6440;
    public const uint EntryVirtualAddress = 0x00FE7040;
    public const uint EndRawFileOffsetExclusive = 0x00BE6458;
    public const uint CapturedWindowLength =
        EndRawFileOffsetExclusive - EntryRawFileOffset;

    public const uint FramePointerDefinitionRawFileOffset = 0x00BE6441;
    public const uint LocalAllocationRawFileOffset = 0x00BE6443;
    public const uint LocalAddressFormationRawFileOffset = 0x00BE6446;
    public const uint SaveEsiRawFileOffset = 0x00BE6449;
    public const uint PushArgumentRawFileOffset = 0x00BE644A;
    public const uint HelperCallRawFileOffset = 0x00BE644B;
    public const uint HelperCallReturnRawFileOffset = 0x00BE6450;
    public const uint HelperReturnDwordReadRawFileOffset = 0x00BE6450;
    public const uint ZeroTestRawFileOffset = 0x00BE6452;
    public const uint ZeroBranchRawFileOffset = 0x00BE6454;
    public const uint NonzeroFallthroughRawFileOffset = 0x00BE6456;
    public const uint ZeroBranchTargetRawFileOffset = 0x00BE6458;
    public const uint HelperTargetRawFileOffset = 0x00BE6B30;
    public const uint ReservedLocalBytes = 0x0C;
    public const uint LocalArgumentOffsetFromFrameEbp = 0x0C;

    public const uint HelperTargetVirtualAddress = 0x00FE7730;
    public const uint EndVirtualAddressExclusive = 0x00FE7058;

    public const string CapturedWindowBytes =
        "55 8B EC 83 EC 0C 8D 45 F4 56 50 E8 E0 06 00 00 "
        + "8B 30 85 F6 74 02 8B 36";

    public static ImmutableArray<uint> PushInstructionRawFileOffsets { get; } =
        ImmutableArray.Create(
            EntryRawFileOffset,
            SaveEsiRawFileOffset,
            PushArgumentRawFileOffset);

    public static ImmutableArray<HighCountObjectHelperEntryInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new HighCountObjectHelperEntryInstructionEvidence(
                "Save the incoming EBP",
                EntryRawFileOffset,
                RawFileOffsetToVirtualAddress(EntryRawFileOffset),
                "55"),
            new HighCountObjectHelperEntryInstructionEvidence(
                "Define the frame EBP after the prologue push",
                FramePointerDefinitionRawFileOffset,
                RawFileOffsetToVirtualAddress(FramePointerDefinitionRawFileOffset),
                "8B EC"),
            new HighCountObjectHelperEntryInstructionEvidence(
                "Reserve twelve local bytes",
                LocalAllocationRawFileOffset,
                RawFileOffsetToVirtualAddress(LocalAllocationRawFileOffset),
                "83 EC 0C"),
            new HighCountObjectHelperEntryInstructionEvidence(
                "Form the local argument address at EBP-0xC without reading it",
                LocalAddressFormationRawFileOffset,
                RawFileOffsetToVirtualAddress(LocalAddressFormationRawFileOffset),
                "8D 45 F4"),
            new HighCountObjectHelperEntryInstructionEvidence(
                "Save ESI before pushing the helper argument",
                SaveEsiRawFileOffset,
                RawFileOffsetToVirtualAddress(SaveEsiRawFileOffset),
                "56"),
            new HighCountObjectHelperEntryInstructionEvidence(
                "Push the local argument address from EAX",
                PushArgumentRawFileOffset,
                RawFileOffsetToVirtualAddress(PushArgumentRawFileOffset),
                "50"),
            new HighCountObjectHelperEntryInstructionEvidence(
                "Call the opaque helper at raw 0xBE6B30 with the entry ECX still unchanged",
                HelperCallRawFileOffset,
                RawFileOffsetToVirtualAddress(HelperCallRawFileOffset),
                "E8 E0 06 00 00",
                CallTargetRawFileOffset: HelperTargetRawFileOffset,
                CallTargetVirtualAddress: HelperTargetVirtualAddress),
            new HighCountObjectHelperEntryInstructionEvidence(
                "Load the DWORD at the helper-return EAX address into ESI",
                HelperReturnDwordReadRawFileOffset,
                RawFileOffsetToVirtualAddress(HelperReturnDwordReadRawFileOffset),
                "8B 30"),
            new HighCountObjectHelperEntryInstructionEvidence(
                "Test the loaded DWORD",
                ZeroTestRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroTestRawFileOffset),
                "85 F6"),
            new HighCountObjectHelperEntryInstructionEvidence(
                "JZ reaches raw 0xBE6458; nonzero falls through to raw 0xBE6456",
                ZeroBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroBranchRawFileOffset),
                "74 02",
                ZeroBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroBranchTargetRawFileOffset)),
            new HighCountObjectHelperEntryInstructionEvidence(
                "The nonzero path begins with an unmodeled later-body read",
                NonzeroFallthroughRawFileOffset,
                RawFileOffsetToVirtualAddress(NonzeroFallthroughRawFileOffset),
                "8B 36"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint LocalArgumentAddress(uint frameEbpAfterPrologue) =>
        unchecked(frameEbpAfterPrologue - LocalArgumentOffsetFromFrameEbp);

    public static HighCountObjectHelperEntryPlan Project(
        HighCountObjectHelperEntryCapture capture)
    {
        uint localArgumentAddress = LocalArgumentAddress(capture.FrameEbpAfterPrologue);
        bool hasUnreachedEax = capture.HelperReturnEax.HasValue;
        bool hasUnreachedDword = capture.DwordAtHelperEax.HasValue;

        if (capture.HelperReturnedNormally is not bool helperReturnedNormally)
        {
            bool laterCaptureProvided = hasUnreachedEax || hasUnreachedDword;
            return Stop(
                capture,
                laterCaptureProvided
                    ? HighCountObjectHelperEntryOutcome.UnreachedCaptureProvided
                    : HighCountObjectHelperEntryOutcome.MissingHelperReturnStatus,
                localArgumentAddress,
                unreachedHelperReturnEaxCaptureProvided: hasUnreachedEax,
                unreachedDwordAtHelperEaxCaptureProvided: hasUnreachedDword,
                stopAt: HelperCallRawFileOffset);
        }

        if (!helperReturnedNormally)
        {
            bool laterCaptureProvided = hasUnreachedEax || hasUnreachedDword;
            return Stop(
                capture,
                laterCaptureProvided
                    ? HighCountObjectHelperEntryOutcome.UnreachedCaptureProvided
                    : HighCountObjectHelperEntryOutcome.HelperDidNotReturnNormally,
                localArgumentAddress,
                helperReturnedNormally: false,
                unreachedHelperReturnEaxCaptureProvided: hasUnreachedEax,
                unreachedDwordAtHelperEaxCaptureProvided: hasUnreachedDword,
                stopAt: HelperCallRawFileOffset);
        }

        if (capture.HelperReturnEax is not uint helperReturnEax)
        {
            return Stop(
                capture,
                hasUnreachedDword
                    ? HighCountObjectHelperEntryOutcome.UnreachedCaptureProvided
                    : HighCountObjectHelperEntryOutcome.MissingHelperReturnEax,
                localArgumentAddress,
                helperReturnedNormally: true,
                unreachedDwordAtHelperEaxCaptureProvided: hasUnreachedDword,
                stopAt: HelperReturnDwordReadRawFileOffset);
        }

        if (capture.DwordAtHelperEax is not uint dwordAtHelperEax)
        {
            return Stop(
                capture,
                HighCountObjectHelperEntryOutcome.MissingDwordAtHelperEax,
                localArgumentAddress,
                helperReturnedNormally: true,
                helperReturnEaxCaptured: true,
                helperReturnEax: helperReturnEax,
                dwordReadReached: true,
                dwordReadAddress: helperReturnEax,
                stopAt: HelperReturnDwordReadRawFileOffset);
        }

        bool zeroBranchTaken = dwordAtHelperEax == 0;
        uint nextRawFileOffset = zeroBranchTaken
            ? ZeroBranchTargetRawFileOffset
            : NonzeroFallthroughRawFileOffset;
        return Stop(
            capture,
            zeroBranchTaken
                ? HighCountObjectHelperEntryOutcome.ZeroLoadedDwordBranchesToBoundary
                : HighCountObjectHelperEntryOutcome.NonzeroLoadedDwordFallsThrough,
            localArgumentAddress,
            helperReturnedNormally: true,
            helperReturnEaxCaptured: true,
            helperReturnEax: helperReturnEax,
            dwordReadReached: true,
            dwordReadAddress: helperReturnEax,
            dwordAtHelperEaxCaptured: true,
            dwordAtHelperEax: dwordAtHelperEax,
            zeroTestProjected: true,
            zeroBranchTaken: zeroBranchTaken,
            nextRawFileOffset: nextRawFileOffset,
            stopAt: nextRawFileOffset);
    }

    private static HighCountObjectHelperEntryPlan Stop(
        HighCountObjectHelperEntryCapture capture,
        HighCountObjectHelperEntryOutcome outcome,
        uint localArgumentAddress,
        bool? helperReturnedNormally = null,
        bool helperReturnEaxCaptured = false,
        uint? helperReturnEax = null,
        bool dwordReadReached = false,
        uint? dwordReadAddress = null,
        bool dwordAtHelperEaxCaptured = false,
        uint? dwordAtHelperEax = null,
        bool zeroTestProjected = false,
        bool? zeroBranchTaken = null,
        uint? nextRawFileOffset = null,
        bool unreachedHelperReturnEaxCaptureProvided = false,
        bool unreachedDwordAtHelperEaxCaptureProvided = false,
        uint stopAt = HelperCallRawFileOffset) =>
        new(
            capture,
            outcome,
            localArgumentAddress,
            LocalArgumentPointerPushed: true,
            LocalArgumentReadBeforeHelperCall: false,
            HelperCallReached: true,
            HelperEntryEcxReceiver: capture.EntryEcxReceiver,
            helperReturnedNormally,
            helperReturnEaxCaptured,
            helperReturnEax,
            dwordReadReached,
            dwordReadAddress,
            dwordAtHelperEaxCaptured,
            dwordAtHelperEax,
            zeroTestProjected,
            zeroBranchTaken,
            nextRawFileOffset,
            unreachedHelperReturnEaxCaptureProvided,
            unreachedDwordAtHelperEaxCaptureProvided,
            stopAt);
}
