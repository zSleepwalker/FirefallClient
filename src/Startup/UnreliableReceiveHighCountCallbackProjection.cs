using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UnreliableReceiveHighCountCallbackCapture(
    bool? PriorBE6440Normal,
    uint? HelperReturnEax,
    uint? FirstLoadedDword,
    uint? SecondLoadedDword,
    uint? IndirectTargetDword,
    bool? CallbackNormal);

public enum UnreliableReceiveHighCountCallbackOutcome
{
    MissingPriorBE6440Normal,
    UnreachedCaptureProvided,
    PriorBE6440DidNotReturnNormally,
    MissingHelperReturnEax,
    MissingFirstLoadedDword,
    ZeroObjectBranch,
    MissingSecondLoadedDword,
    MissingIndirectTargetDword,
    MissingCallbackNormal,
    CallbackDidNotReturnNormally,
    CallbackReturnedNormally
}

public readonly record struct UnreliableReceiveHighCountCallbackPlan(
    UnreliableReceiveHighCountCallbackCapture Capture,
    UnreliableReceiveHighCountCallbackOutcome Outcome,
    bool PriorBoundaryValidated,
    bool FirstReadAddressCaptured,
    uint? FirstReadAddress,
    bool FirstLoadedDwordRead,
    uint? CapturedFirstLoadedDword,
    bool ZeroObjectBranchTaken,
    bool SecondReadReached,
    uint? SecondReadAddress,
    bool SecondLoadedDwordRead,
    uint? CapturedSecondLoadedDword,
    bool PushOneReached,
    ImmutableArray<uint> StackPushesInInstructionOrder,
    bool IndirectTargetRead,
    uint? IndirectTargetAddress,
    uint? CapturedIndirectTargetDword,
    bool IndirectCallReached,
    bool? CallbackReturnedNormally,
    bool NormalBoundaryAdmitted,
    bool UnreachedHelperReturnEaxCaptureProvided,
    bool UnreachedFirstLoadedDwordCaptureProvided,
    bool UnreachedSecondLoadedDwordCaptureProvided,
    bool UnreachedIndirectTargetDwordCaptureProvided,
    bool UnreachedCallbackNormalCaptureProvided,
    uint? SelectedDestinationRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct UnreliableReceiveHighCountCallbackInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the ordered object reads, conditional callback, and normal boundary only.
/// </summary>
public static class UnreliableReceiveHighCountCallbackProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint PriorBE6440NormalBoundaryRawFileOffset = 0x00BE6440;
    public const uint EntryRawFileOffset = 0x00BE5BF1;
    public const uint FirstReadRawFileOffset = EntryRawFileOffset;
    public const uint ZeroObjectBranchRawFileOffset = 0x00BE5BF5;
    public const uint SecondReadRawFileOffset = 0x00BE5BF7;
    public const uint PushOneRawFileOffset = 0x00BE5BF9;
    public const uint IndirectCallRawFileOffset = 0x00BE5BFB;
    public const uint NormalBoundaryRawFileOffset = 0x00BE5BFE;
    public const uint EndRawFileOffsetExclusive = NormalBoundaryRawFileOffset;

    public const uint EntryVirtualAddress = 0x00FE67F1;
    public const uint NormalBoundaryVirtualAddress = 0x00FE67FE;
    public const uint IndirectTargetOffset = 8;

    public const string CapturedWindowBytes =
        "8B 08 85 C9 74 07 8B 01 6A 01 FF 50 08";

    public static ImmutableArray<UnreliableReceiveHighCountCallbackInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            new UnreliableReceiveHighCountCallbackInstructionEvidence(
                "Load the DWORD at the captured helper-return EAX address into ECX",
                FirstReadRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstReadRawFileOffset),
                "8B 08"),
            new UnreliableReceiveHighCountCallbackInstructionEvidence(
                "Test the first loaded DWORD",
                0x00BE5BF3,
                RawFileOffsetToVirtualAddress(0x00BE5BF3),
                "85 C9"),
            new UnreliableReceiveHighCountCallbackInstructionEvidence(
                "A zero first loaded DWORD branches to the normal boundary at raw 0xBE5BFE",
                ZeroObjectBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroObjectBranchRawFileOffset),
                "74 07"),
            new UnreliableReceiveHighCountCallbackInstructionEvidence(
                "Load the DWORD at the nonzero first loaded DWORD address into EAX",
                SecondReadRawFileOffset,
                RawFileOffsetToVirtualAddress(SecondReadRawFileOffset),
                "8B 01"),
            new UnreliableReceiveHighCountCallbackInstructionEvidence(
                "Push the immediate value 1 before the indirect target read and call",
                PushOneRawFileOffset,
                RawFileOffsetToVirtualAddress(PushOneRawFileOffset),
                "6A 01"),
            new UnreliableReceiveHighCountCallbackInstructionEvidence(
                "Call through the DWORD at the second loaded DWORD address plus 8",
                IndirectCallRawFileOffset,
                RawFileOffsetToVirtualAddress(IndirectCallRawFileOffset),
                "FF 50 08"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint IndirectTargetAddress(uint secondLoadedDword) =>
        unchecked(secondLoadedDword + IndirectTargetOffset);

    public static UnreliableReceiveHighCountCallbackPlan Project(
        UnreliableReceiveHighCountCallbackCapture capture)
    {
        if (capture.PriorBE6440Normal is not bool priorBoundaryNormal)
        {
            return Stop(
                capture,
                HasAnyAfterPriorBoundary(capture)
                    ? UnreliableReceiveHighCountCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountCallbackOutcome.MissingPriorBE6440Normal,
                unreachedHelperReturnEaxCaptureProvided:
                    capture.HelperReturnEax.HasValue,
                unreachedFirstLoadedDwordCaptureProvided:
                    capture.FirstLoadedDword.HasValue,
                unreachedSecondLoadedDwordCaptureProvided:
                    capture.SecondLoadedDword.HasValue,
                unreachedIndirectTargetDwordCaptureProvided:
                    capture.IndirectTargetDword.HasValue,
                unreachedCallbackNormalCaptureProvided:
                    capture.CallbackNormal.HasValue,
                stopAt: EntryRawFileOffset);
        }

        if (!priorBoundaryNormal)
        {
            return Stop(
                capture,
                HasAnyAfterPriorBoundary(capture)
                    ? UnreliableReceiveHighCountCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountCallbackOutcome.PriorBE6440DidNotReturnNormally,
                unreachedHelperReturnEaxCaptureProvided:
                    capture.HelperReturnEax.HasValue,
                unreachedFirstLoadedDwordCaptureProvided:
                    capture.FirstLoadedDword.HasValue,
                unreachedSecondLoadedDwordCaptureProvided:
                    capture.SecondLoadedDword.HasValue,
                unreachedIndirectTargetDwordCaptureProvided:
                    capture.IndirectTargetDword.HasValue,
                unreachedCallbackNormalCaptureProvided:
                    capture.CallbackNormal.HasValue,
                stopAt: EntryRawFileOffset);
        }

        if (capture.HelperReturnEax is not uint firstReadAddress)
        {
            return Stop(
                capture,
                HasAnyAfterFirstReadAddress(capture)
                    ? UnreliableReceiveHighCountCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountCallbackOutcome.MissingHelperReturnEax,
                priorBoundaryValidated: true,
                unreachedFirstLoadedDwordCaptureProvided:
                    capture.FirstLoadedDword.HasValue,
                unreachedSecondLoadedDwordCaptureProvided:
                    capture.SecondLoadedDword.HasValue,
                unreachedIndirectTargetDwordCaptureProvided:
                    capture.IndirectTargetDword.HasValue,
                unreachedCallbackNormalCaptureProvided:
                    capture.CallbackNormal.HasValue,
                stopAt: FirstReadRawFileOffset);
        }

        if (capture.FirstLoadedDword is not uint firstLoadedDword)
        {
            return Stop(
                capture,
                HasAnyAfterFirstLoadedDword(capture)
                    ? UnreliableReceiveHighCountCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountCallbackOutcome.MissingFirstLoadedDword,
                priorBoundaryValidated: true,
                firstReadAddressCaptured: true,
                firstReadAddress: firstReadAddress,
                unreachedSecondLoadedDwordCaptureProvided:
                    capture.SecondLoadedDword.HasValue,
                unreachedIndirectTargetDwordCaptureProvided:
                    capture.IndirectTargetDword.HasValue,
                unreachedCallbackNormalCaptureProvided:
                    capture.CallbackNormal.HasValue,
                stopAt: FirstReadRawFileOffset);
        }

        if (firstLoadedDword == 0)
        {
            bool laterCaptureProvided =
                capture.SecondLoadedDword.HasValue
                || capture.IndirectTargetDword.HasValue
                || capture.CallbackNormal.HasValue;
            return Stop(
                capture,
                laterCaptureProvided
                    ? UnreliableReceiveHighCountCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountCallbackOutcome.ZeroObjectBranch,
                priorBoundaryValidated: true,
                firstReadAddressCaptured: true,
                firstReadAddress: firstReadAddress,
                firstLoadedDwordRead: true,
                capturedFirstLoadedDword: firstLoadedDword,
                zeroObjectBranchTaken: true,
                normalBoundaryAdmitted: !laterCaptureProvided,
                unreachedSecondLoadedDwordCaptureProvided:
                    capture.SecondLoadedDword.HasValue,
                unreachedIndirectTargetDwordCaptureProvided:
                    capture.IndirectTargetDword.HasValue,
                unreachedCallbackNormalCaptureProvided:
                    capture.CallbackNormal.HasValue,
                selectedDestinationRawFileOffset:
                    laterCaptureProvided ? null : NormalBoundaryRawFileOffset,
                stopAt: NormalBoundaryRawFileOffset);
        }

        if (capture.SecondLoadedDword is not uint secondLoadedDword)
        {
            return Stop(
                capture,
                HasAnyAfterSecondRead(capture)
                    ? UnreliableReceiveHighCountCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountCallbackOutcome.MissingSecondLoadedDword,
                priorBoundaryValidated: true,
                firstReadAddressCaptured: true,
                firstReadAddress: firstReadAddress,
                firstLoadedDwordRead: true,
                capturedFirstLoadedDword: firstLoadedDword,
                secondReadReached: true,
                secondReadAddress: firstLoadedDword,
                unreachedIndirectTargetDwordCaptureProvided:
                    capture.IndirectTargetDword.HasValue,
                unreachedCallbackNormalCaptureProvided:
                    capture.CallbackNormal.HasValue,
                stopAt: SecondReadRawFileOffset);
        }

        ImmutableArray<uint> pushes = ImmutableArray.Create(1u);
        uint indirectTargetAddress = IndirectTargetAddress(secondLoadedDword);
        if (capture.IndirectTargetDword is not uint indirectTargetDword)
        {
            return Stop(
                capture,
                capture.CallbackNormal.HasValue
                    ? UnreliableReceiveHighCountCallbackOutcome.UnreachedCaptureProvided
                    : UnreliableReceiveHighCountCallbackOutcome.MissingIndirectTargetDword,
                priorBoundaryValidated: true,
                firstReadAddressCaptured: true,
                firstReadAddress: firstReadAddress,
                firstLoadedDwordRead: true,
                capturedFirstLoadedDword: firstLoadedDword,
                secondReadReached: true,
                secondReadAddress: firstLoadedDword,
                secondLoadedDwordRead: true,
                capturedSecondLoadedDword: secondLoadedDword,
                pushOneReached: true,
                stackPushesInInstructionOrder: pushes,
                indirectTargetAddress: indirectTargetAddress,
                unreachedCallbackNormalCaptureProvided:
                    capture.CallbackNormal.HasValue,
                stopAt: IndirectCallRawFileOffset);
        }

        if (capture.CallbackNormal is not bool callbackNormal)
        {
            return Stop(
                capture,
                UnreliableReceiveHighCountCallbackOutcome.MissingCallbackNormal,
                priorBoundaryValidated: true,
                firstReadAddressCaptured: true,
                firstReadAddress: firstReadAddress,
                firstLoadedDwordRead: true,
                capturedFirstLoadedDword: firstLoadedDword,
                secondReadReached: true,
                secondReadAddress: firstLoadedDword,
                secondLoadedDwordRead: true,
                capturedSecondLoadedDword: secondLoadedDword,
                pushOneReached: true,
                stackPushesInInstructionOrder: pushes,
                indirectTargetRead: true,
                indirectTargetAddress: indirectTargetAddress,
                capturedIndirectTargetDword: indirectTargetDword,
                indirectCallReached: true,
                stopAt: IndirectCallRawFileOffset);
        }

        if (!callbackNormal)
        {
            return Stop(
                capture,
                UnreliableReceiveHighCountCallbackOutcome.CallbackDidNotReturnNormally,
                priorBoundaryValidated: true,
                firstReadAddressCaptured: true,
                firstReadAddress: firstReadAddress,
                firstLoadedDwordRead: true,
                capturedFirstLoadedDword: firstLoadedDword,
                secondReadReached: true,
                secondReadAddress: firstLoadedDword,
                secondLoadedDwordRead: true,
                capturedSecondLoadedDword: secondLoadedDword,
                pushOneReached: true,
                stackPushesInInstructionOrder: pushes,
                indirectTargetRead: true,
                indirectTargetAddress: indirectTargetAddress,
                capturedIndirectTargetDword: indirectTargetDword,
                indirectCallReached: true,
                callbackReturnedNormally: false,
                stopAt: IndirectCallRawFileOffset);
        }

        return Stop(
            capture,
            UnreliableReceiveHighCountCallbackOutcome.CallbackReturnedNormally,
            priorBoundaryValidated: true,
            firstReadAddressCaptured: true,
            firstReadAddress: firstReadAddress,
            firstLoadedDwordRead: true,
            capturedFirstLoadedDword: firstLoadedDword,
            secondReadReached: true,
            secondReadAddress: firstLoadedDword,
            secondLoadedDwordRead: true,
            capturedSecondLoadedDword: secondLoadedDword,
            pushOneReached: true,
            stackPushesInInstructionOrder: pushes,
            indirectTargetRead: true,
            indirectTargetAddress: indirectTargetAddress,
            capturedIndirectTargetDword: indirectTargetDword,
            indirectCallReached: true,
            callbackReturnedNormally: true,
            normalBoundaryAdmitted: true,
            selectedDestinationRawFileOffset: NormalBoundaryRawFileOffset,
            stopAt: NormalBoundaryRawFileOffset);
    }

    private static bool HasAnyAfterPriorBoundary(
        UnreliableReceiveHighCountCallbackCapture capture) =>
        capture.HelperReturnEax.HasValue
        || capture.FirstLoadedDword.HasValue
        || capture.SecondLoadedDword.HasValue
        || capture.IndirectTargetDword.HasValue
        || capture.CallbackNormal.HasValue;

    private static bool HasAnyAfterFirstReadAddress(
        UnreliableReceiveHighCountCallbackCapture capture) =>
        capture.FirstLoadedDword.HasValue
        || capture.SecondLoadedDword.HasValue
        || capture.IndirectTargetDword.HasValue
        || capture.CallbackNormal.HasValue;

    private static bool HasAnyAfterFirstLoadedDword(
        UnreliableReceiveHighCountCallbackCapture capture) =>
        capture.SecondLoadedDword.HasValue
        || capture.IndirectTargetDword.HasValue
        || capture.CallbackNormal.HasValue;

    private static bool HasAnyAfterSecondRead(
        UnreliableReceiveHighCountCallbackCapture capture) =>
        capture.IndirectTargetDword.HasValue
        || capture.CallbackNormal.HasValue;

    private static UnreliableReceiveHighCountCallbackPlan Stop(
        UnreliableReceiveHighCountCallbackCapture capture,
        UnreliableReceiveHighCountCallbackOutcome outcome,
        bool priorBoundaryValidated = false,
        bool firstReadAddressCaptured = false,
        uint? firstReadAddress = null,
        bool firstLoadedDwordRead = false,
        uint? capturedFirstLoadedDword = null,
        bool zeroObjectBranchTaken = false,
        bool secondReadReached = false,
        uint? secondReadAddress = null,
        bool secondLoadedDwordRead = false,
        uint? capturedSecondLoadedDword = null,
        bool pushOneReached = false,
        ImmutableArray<uint> stackPushesInInstructionOrder = default,
        bool indirectTargetRead = false,
        uint? indirectTargetAddress = null,
        uint? capturedIndirectTargetDword = null,
        bool indirectCallReached = false,
        bool? callbackReturnedNormally = null,
        bool normalBoundaryAdmitted = false,
        bool unreachedHelperReturnEaxCaptureProvided = false,
        bool unreachedFirstLoadedDwordCaptureProvided = false,
        bool unreachedSecondLoadedDwordCaptureProvided = false,
        bool unreachedIndirectTargetDwordCaptureProvided = false,
        bool unreachedCallbackNormalCaptureProvided = false,
        uint? selectedDestinationRawFileOffset = null,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            priorBoundaryValidated,
            firstReadAddressCaptured,
            firstReadAddress,
            firstLoadedDwordRead,
            capturedFirstLoadedDword,
            zeroObjectBranchTaken,
            secondReadReached,
            secondReadAddress,
            secondLoadedDwordRead,
            capturedSecondLoadedDword,
            pushOneReached,
            stackPushesInInstructionOrder.IsDefault
                ? ImmutableArray<uint>.Empty
                : stackPushesInInstructionOrder,
            indirectTargetRead,
            indirectTargetAddress,
            capturedIndirectTargetDword,
            indirectCallReached,
            callbackReturnedNormally,
            normalBoundaryAdmitted,
            unreachedHelperReturnEaxCaptureProvided,
            unreachedFirstLoadedDwordCaptureProvided,
            unreachedSecondLoadedDwordCaptureProvided,
            unreachedIndirectTargetDwordCaptureProvided,
            unreachedCallbackNormalCaptureProvided,
            selectedDestinationRawFileOffset,
            stopAt);
}
