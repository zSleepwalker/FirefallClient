using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum FrameHandoffCapacityCallEntryRoute
{
    DirectCallB5,
    GuardIndirectBC4
}

public readonly record struct FrameHandoffCapacityCallCapture(
    FrameHandoffCapacityCallEntryRoute? EntryRoute,
    uint FrameEbp,
    bool? DirectCallReturnedNormally,
    uint? DirectCallReturnEax,
    bool? GuardIndirectCallReturnedNormally);

public enum FrameHandoffCapacityCallOutcome
{
    MissingEntryRoute,
    InvalidEntryRoute,
    MissingDirectCallReturnCapture,
    DirectCallDidNotReturnNormally,
    MissingDirectCallReturnEaxCapture,
    MissingGuardIndirectCallReturnCapture,
    GuardIndirectCallDidNotReturnNormally,
    DirectNonzeroReturnedNormally,
    DirectZeroGuardIndirectReturnedNormally,
    GuardIndirectReturnedNormally
}

public readonly record struct FrameHandoffCapacityCallPlan(
    FrameHandoffCapacityCallCapture Capture,
    FrameHandoffCapacityCallOutcome Outcome,
    bool DirectCallReached,
    bool? DirectCallReturnedNormally,
    bool Cleanup4Reached,
    bool FrameWriteReached,
    bool FrameWriteProjected,
    uint? FrameWriteAddress,
    uint? FrameWriteValue,
    bool EaxTestReached,
    bool? JnzTaken,
    bool GuardIndirectCallReached,
    bool? GuardIndirectCallReturnedNormally,
    bool UnreachedDirectCallReturnCaptureProvided,
    bool UnreachedDirectCallReturnEaxCaptureProvided,
    bool UnreachedGuardIndirectCallReturnCaptureProvided,
    bool NormalBoundaryReached,
    bool SuccessAdmission,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct FrameHandoffCapacityCallInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null,
    uint? DirectCallTargetRawFileOffset = null,
    uint? DirectCallTargetVirtualAddress = null,
    uint? IndirectCallCellVirtualAddress = null);

/// <summary>
/// Projects the direct and guard-indirect capacity-call routes through their shared return boundary.
/// </summary>
public static class FrameHandoffCapacityCallProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6BB5;
    public const uint EntryVirtualAddress = 0x00FE77B5;
    public const uint DirectCallRawFileOffset = 0x00BE6BB5;
    public const uint DirectCallNormalReturnRawFileOffset = 0x00BE6BBA;
    public const uint Cleanup4RawFileOffset = 0x00BE6BBA;
    public const uint FrameWriteRawFileOffset = 0x00BE6BBD;
    public const uint EaxTestRawFileOffset = 0x00BE6BC0;
    public const uint JnzRawFileOffset = 0x00BE6BC2;
    public const uint GuardIndirectCallRawFileOffset = 0x00BE6BC4;
    public const uint GuardBranchEntryRawFileOffset = GuardIndirectCallRawFileOffset;
    public const uint NormalBoundaryRawFileOffsetExclusive = 0x00BE6BCA;
    public const uint DirectCallTargetRawFileOffset = 0x012B7FB0;
    public const uint DirectCallTargetVirtualAddress =
        DirectCallTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint GuardIndirectCallCellVirtualAddress = 0x01AC4424;
    public const uint FrameWriteOffset = 4;
    public const uint CapturedWindowByteCount =
        NormalBoundaryRawFileOffsetExclusive - EntryRawFileOffset;

    public const string CapturedWindowBytes =
        "E8F6136D0083C4048945FC85C07506FF152444AC01";

    public static ImmutableArray<FrameHandoffCapacityCallInstructionEvidence> Evidence { get; } =
        ImmutableArray.Create(
            new FrameHandoffCapacityCallInstructionEvidence(
                "Call raw 0x012B7FB0; normal return reaches raw 0xBE6BBA",
                DirectCallRawFileOffset,
                DirectCallRawFileOffset,
                RawFileOffsetToVirtualAddress(DirectCallRawFileOffset),
                "E8 F6 13 6D 00",
                DirectCallTargetRawFileOffset: DirectCallTargetRawFileOffset,
                DirectCallTargetVirtualAddress: DirectCallTargetVirtualAddress),
            new FrameHandoffCapacityCallInstructionEvidence(
                "Clean up four bytes before the frame store",
                Cleanup4RawFileOffset,
                Cleanup4RawFileOffset,
                RawFileOffsetToVirtualAddress(Cleanup4RawFileOffset),
                "83 C4 04"),
            new FrameHandoffCapacityCallInstructionEvidence(
                "Store the full EAX DWORD at [EBP-4]",
                FrameWriteRawFileOffset,
                FrameWriteRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameWriteRawFileOffset),
                "89 45 FC"),
            new FrameHandoffCapacityCallInstructionEvidence(
                "Test EAX against itself",
                EaxTestRawFileOffset,
                EaxTestRawFileOffset,
                RawFileOffsetToVirtualAddress(EaxTestRawFileOffset),
                "85 C0"),
            new FrameHandoffCapacityCallInstructionEvidence(
                "A nonzero EAX skips the indirect call and reaches the normal boundary",
                JnzRawFileOffset,
                JnzRawFileOffset,
                RawFileOffsetToVirtualAddress(JnzRawFileOffset),
                "75 06",
                NormalBoundaryRawFileOffsetExclusive,
                RawFileOffsetToVirtualAddress(NormalBoundaryRawFileOffsetExclusive)),
            new FrameHandoffCapacityCallInstructionEvidence(
                "For zero EAX or the explicit guard entry, call through cell VA 0x01AC4424",
                GuardIndirectCallRawFileOffset,
                GuardIndirectCallRawFileOffset,
                RawFileOffsetToVirtualAddress(GuardIndirectCallRawFileOffset),
                "FF 15 24 44 AC 01",
                IndirectCallCellVirtualAddress: GuardIndirectCallCellVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint RelativeCallTarget(uint instructionRawFileOffset, int displacement) =>
        unchecked(instructionRawFileOffset + 5u + (uint)displacement);

    public static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2u + (uint)(sbyte)displacement);

    public static uint FrameWriteAddress(uint frameEbp) =>
        unchecked(frameEbp - FrameWriteOffset);

    public static FrameHandoffCapacityCallPlan Project(
        FrameHandoffCapacityCallCapture capture)
    {
        if (capture.EntryRoute is not FrameHandoffCapacityCallEntryRoute route
            || route is not (FrameHandoffCapacityCallEntryRoute.DirectCallB5
                or FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4))
        {
            return Stop(
                capture,
                capture.EntryRoute.HasValue
                    ? FrameHandoffCapacityCallOutcome.InvalidEntryRoute
                    : FrameHandoffCapacityCallOutcome.MissingEntryRoute,
                unreachedDirectCallReturnCaptureProvided:
                    capture.DirectCallReturnedNormally.HasValue,
                unreachedDirectCallReturnEaxCaptureProvided:
                    capture.DirectCallReturnEax.HasValue,
                unreachedGuardIndirectCallReturnCaptureProvided:
                    capture.GuardIndirectCallReturnedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        if (route == FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4)
        {
            bool unreachedDirectReturn = capture.DirectCallReturnedNormally.HasValue;
            bool unreachedDirectEax = capture.DirectCallReturnEax.HasValue;
            if (capture.GuardIndirectCallReturnedNormally is not bool guardReturnedNormally)
            {
                return Stop(
                    capture,
                    FrameHandoffCapacityCallOutcome.MissingGuardIndirectCallReturnCapture,
                    guardIndirectCallReached: true,
                    unreachedDirectCallReturnCaptureProvided: unreachedDirectReturn,
                    unreachedDirectCallReturnEaxCaptureProvided: unreachedDirectEax,
                    stopAt: GuardIndirectCallRawFileOffset);
            }

            if (!guardReturnedNormally)
            {
                return Stop(
                    capture,
                    FrameHandoffCapacityCallOutcome.GuardIndirectCallDidNotReturnNormally,
                    guardIndirectCallReached: true,
                    guardIndirectCallReturnedNormally: false,
                    unreachedDirectCallReturnCaptureProvided: unreachedDirectReturn,
                    unreachedDirectCallReturnEaxCaptureProvided: unreachedDirectEax,
                    stopAt: GuardIndirectCallRawFileOffset);
            }

            bool successAdmission = !unreachedDirectReturn && !unreachedDirectEax;
            return Stop(
                capture,
                FrameHandoffCapacityCallOutcome.GuardIndirectReturnedNormally,
                guardIndirectCallReached: true,
                guardIndirectCallReturnedNormally: true,
                unreachedDirectCallReturnCaptureProvided: unreachedDirectReturn,
                unreachedDirectCallReturnEaxCaptureProvided: unreachedDirectEax,
                normalBoundaryReached: true,
                successAdmission: successAdmission,
                stopAt: NormalBoundaryRawFileOffsetExclusive);
        }

        if (capture.DirectCallReturnedNormally is not bool directReturnedNormally)
        {
            return Stop(
                capture,
                FrameHandoffCapacityCallOutcome.MissingDirectCallReturnCapture,
                directCallReached: true,
                unreachedDirectCallReturnEaxCaptureProvided:
                    capture.DirectCallReturnEax.HasValue,
                unreachedGuardIndirectCallReturnCaptureProvided:
                    capture.GuardIndirectCallReturnedNormally.HasValue,
                stopAt: DirectCallRawFileOffset);
        }

        if (!directReturnedNormally)
        {
            return Stop(
                capture,
                FrameHandoffCapacityCallOutcome.DirectCallDidNotReturnNormally,
                directCallReached: true,
                directCallReturnedNormally: false,
                unreachedDirectCallReturnEaxCaptureProvided:
                    capture.DirectCallReturnEax.HasValue,
                unreachedGuardIndirectCallReturnCaptureProvided:
                    capture.GuardIndirectCallReturnedNormally.HasValue,
                stopAt: DirectCallRawFileOffset);
        }

        if (capture.DirectCallReturnEax is not uint returnEax)
        {
            return Stop(
                capture,
                FrameHandoffCapacityCallOutcome.MissingDirectCallReturnEaxCapture,
                directCallReached: true,
                directCallReturnedNormally: true,
                cleanup4Reached: true,
                unreachedGuardIndirectCallReturnCaptureProvided:
                    capture.GuardIndirectCallReturnedNormally.HasValue,
                stopAt: FrameWriteRawFileOffset);
        }

        bool jnzTaken = returnEax != 0;
        bool unreachedGuardReturn = jnzTaken
            && capture.GuardIndirectCallReturnedNormally.HasValue;
        if (jnzTaken)
        {
            bool successAdmission = !unreachedGuardReturn;
            return Stop(
                capture,
                FrameHandoffCapacityCallOutcome.DirectNonzeroReturnedNormally,
                directCallReached: true,
                directCallReturnedNormally: true,
                cleanup4Reached: true,
                frameWriteReached: true,
                frameWriteProjected: true,
                frameWriteAddress: FrameWriteAddress(capture.FrameEbp),
                frameWriteValue: returnEax,
                eaxTestReached: true,
                jnzTaken: true,
                unreachedGuardIndirectCallReturnCaptureProvided: unreachedGuardReturn,
                normalBoundaryReached: true,
                successAdmission: successAdmission,
                stopAt: NormalBoundaryRawFileOffsetExclusive);
        }

        if (capture.GuardIndirectCallReturnedNormally is not bool indirectReturnedNormally)
        {
            return Stop(
                capture,
                FrameHandoffCapacityCallOutcome.MissingGuardIndirectCallReturnCapture,
                directCallReached: true,
                directCallReturnedNormally: true,
                cleanup4Reached: true,
                frameWriteReached: true,
                frameWriteProjected: true,
                frameWriteAddress: FrameWriteAddress(capture.FrameEbp),
                frameWriteValue: returnEax,
                eaxTestReached: true,
                jnzTaken: false,
                guardIndirectCallReached: true,
                stopAt: GuardIndirectCallRawFileOffset);
        }

        if (!indirectReturnedNormally)
        {
            return Stop(
                capture,
                FrameHandoffCapacityCallOutcome.GuardIndirectCallDidNotReturnNormally,
                directCallReached: true,
                directCallReturnedNormally: true,
                cleanup4Reached: true,
                frameWriteReached: true,
                frameWriteProjected: true,
                frameWriteAddress: FrameWriteAddress(capture.FrameEbp),
                frameWriteValue: returnEax,
                eaxTestReached: true,
                jnzTaken: false,
                guardIndirectCallReached: true,
                guardIndirectCallReturnedNormally: false,
                stopAt: GuardIndirectCallRawFileOffset);
        }

        return Stop(
            capture,
            FrameHandoffCapacityCallOutcome.DirectZeroGuardIndirectReturnedNormally,
            directCallReached: true,
            directCallReturnedNormally: true,
            cleanup4Reached: true,
            frameWriteReached: true,
            frameWriteProjected: true,
            frameWriteAddress: FrameWriteAddress(capture.FrameEbp),
            frameWriteValue: returnEax,
            eaxTestReached: true,
            jnzTaken: false,
            guardIndirectCallReached: true,
            guardIndirectCallReturnedNormally: true,
            normalBoundaryReached: true,
            successAdmission: true,
            stopAt: NormalBoundaryRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityCallPlan Stop(
        FrameHandoffCapacityCallCapture capture,
        FrameHandoffCapacityCallOutcome outcome,
        bool directCallReached = false,
        bool? directCallReturnedNormally = null,
        bool cleanup4Reached = false,
        bool frameWriteReached = false,
        bool frameWriteProjected = false,
        uint? frameWriteAddress = null,
        uint? frameWriteValue = null,
        bool eaxTestReached = false,
        bool? jnzTaken = null,
        bool guardIndirectCallReached = false,
        bool? guardIndirectCallReturnedNormally = null,
        bool unreachedDirectCallReturnCaptureProvided = false,
        bool unreachedDirectCallReturnEaxCaptureProvided = false,
        bool unreachedGuardIndirectCallReturnCaptureProvided = false,
        bool normalBoundaryReached = false,
        bool successAdmission = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            directCallReached,
            directCallReturnedNormally,
            cleanup4Reached,
            frameWriteReached,
            frameWriteProjected,
            frameWriteAddress,
            frameWriteValue,
            eaxTestReached,
            jnzTaken,
            guardIndirectCallReached,
            guardIndirectCallReturnedNormally,
            unreachedDirectCallReturnCaptureProvided,
            unreachedDirectCallReturnEaxCaptureProvided,
            unreachedGuardIndirectCallReturnCaptureProvided,
            normalBoundaryReached,
            successAdmission,
            stopAt);
}
