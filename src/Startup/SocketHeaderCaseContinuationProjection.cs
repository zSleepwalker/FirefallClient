using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum SocketHeaderCaseContinuationCase
{
    Case1 = 1,
    Case2 = 2,
    Case3 = 3
}

public readonly record struct SocketHeaderCaseContinuationCapture(
    SocketHeaderCaseContinuationCase Case,
    uint? GetterEax,
    byte? OpaqueCallReturnAl,
    uint? PushedEdiValue = null);

public enum SocketHeaderCaseContinuationOutcome
{
    MissingGetterEaxCapture,
    UnreachedOpaqueReturnAlCapture,
    MissingOpaqueReturnAlCapture,
    BranchSelected
}

public readonly record struct SocketHeaderCaseContinuationPlan(
    SocketHeaderCaseContinuationCapture Capture,
    SocketHeaderCaseContinuationOutcome Outcome,
    uint? GetterEax,
    bool NextCallReached,
    uint? NextCallEcX,
    uint NextCallRawFileOffset,
    uint NextCallTargetRawFileOffset,
    uint NextCallTargetVirtualAddress,
    byte? OpaqueCallReturnAl,
    bool? OpaqueCallReturnAlNonZero,
    bool? BranchTaken,
    uint BranchRawFileOffset,
    uint BranchTargetRawFileOffset,
    uint BranchFallthroughRawFileOffset,
    uint? SelectedBranchDestinationRawFileOffset,
    uint? SelectedBranchDestinationVirtualAddress,
    uint? PreexistingPushedEdiValue,
    bool PushedEdiStackEntryRemainsBeforeNextCall,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct SocketHeaderCaseContinuationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects the three socket-header case continuations from a normally
/// returning getter through the first AL test and its selected destination.
/// A captured getter EAX implies normal return; no separate return flag is
/// required. The projection stops at that destination and makes no claim
/// about the called target's ABI or later stack cleanup.
/// </summary>
public static class SocketHeaderCaseContinuationProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint Case1PushedEdiRawFileOffset = 0x00BD415A;
    public const uint Case1GetterCallRawFileOffset = 0x00BD4161;
    public const uint Case1GetterRawFileOffset = 0x00BD5BA0;
    public const uint Case1GetterPlainRetRawFileOffset = 0x00BD5BC2;
    public const uint Case1PostGetterRawFileOffset = 0x00BD4166;
    public const uint Case1NextCallRawFileOffset = 0x00BD4168;
    public const uint Case1NextCallTargetRawFileOffset = 0x00BE51F0;
    public const uint Case1AlTestRawFileOffset = 0x00BD416D;
    public const uint Case1BranchRawFileOffset = 0x00BD416F;
    public const uint Case1BranchFallthroughRawFileOffset = 0x00BD4175;

    public const uint Case2PushedEdiRawFileOffset = 0x00BD418D;
    public const uint Case2GetterCallRawFileOffset = 0x00BD4194;
    public const uint Case2GetterRawFileOffset = 0x00BD5BA0;
    public const uint Case2GetterPlainRetRawFileOffset = 0x00BD5BC2;
    public const uint Case2PostGetterRawFileOffset = 0x00BD4199;
    public const uint Case2NextCallRawFileOffset = 0x00BD419B;
    public const uint Case2NextCallTargetRawFileOffset = 0x00BE51F0;
    public const uint Case2AlTestRawFileOffset = 0x00BD41A0;
    public const uint Case2BranchRawFileOffset = 0x00BD41A2;
    public const uint Case2BranchFallthroughRawFileOffset = 0x00BD41A8;

    public const uint Case3PushedEdiRawFileOffset = 0x00BD41C0;
    public const uint Case3GetterCallRawFileOffset = 0x00BD41C7;
    public const uint Case3GetterRawFileOffset = 0x00BD5C00;
    public const uint Case3GetterPlainRetRawFileOffset = 0x00BD5C22;
    public const uint Case3PostGetterRawFileOffset = 0x00BD41CC;
    public const uint Case3NextCallRawFileOffset = 0x00BD41CE;
    public const uint Case3NextCallTargetRawFileOffset = 0x00BE5A40;
    public const uint Case3AlTestRawFileOffset = 0x00BD41D3;
    public const uint Case3BranchRawFileOffset = 0x00BD41D5;
    public const uint Case3BranchFallthroughRawFileOffset = 0x00BD41DB;

    public const uint GetterPlainReturnLength = 1;
    public const uint NextCallLength = 5;
    public const uint AlTestLength = 2;
    public const uint ConditionalBranchLength = 6;
    public const uint BranchTakenDestinationRawFileOffset = 0x00BD3E60;

    public const string Case1GetterCallBytes = "E8 3A 1A 00 00";
    public const string Case2GetterCallBytes = "E8 07 1A 00 00";
    public const string Case3GetterCallBytes = "E8 34 1A 00 00";
    public const string GetterPlainReturnBytes = "C3";
    public const string MoveGetterEaxToEcxBytes = "8B C8";
    public const string Case1NextCallBytes = "E8 83 10 01 00";
    public const string Case2NextCallBytes = "E8 50 10 01 00";
    public const string Case3NextCallBytes = "E8 6D 18 01 00";
    public const string AlTestBytes = "84 C0";
    public const string Case1ConditionalBranchBytes = "0F 85 EB FC FF FF";
    public const string Case2ConditionalBranchBytes = "0F 85 B8 FC FF FF";
    public const string Case3ConditionalBranchBytes = "0F 85 85 FC FF FF";

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint PushedEdiRawFileOffset(SocketHeaderCaseContinuationCase @case) =>
        GetRoute(@case).PushedEdiRawFileOffset;

    public static uint GetterCallRawFileOffset(SocketHeaderCaseContinuationCase @case) =>
        GetRoute(@case).GetterCallRawFileOffset;

    public static uint GetterRawFileOffset(SocketHeaderCaseContinuationCase @case) =>
        GetRoute(@case).GetterRawFileOffset;

    public static uint GetterPlainRetRawFileOffset(SocketHeaderCaseContinuationCase @case) =>
        GetRoute(@case).GetterPlainRetRawFileOffset;

    public static uint PostGetterRawFileOffset(SocketHeaderCaseContinuationCase @case) =>
        GetRoute(@case).PostGetterRawFileOffset;

    public static uint NextCallRawFileOffset(SocketHeaderCaseContinuationCase @case) =>
        GetRoute(@case).NextCallRawFileOffset;

    public static uint NextCallTargetRawFileOffset(SocketHeaderCaseContinuationCase @case) =>
        GetRoute(@case).NextCallTargetRawFileOffset;

    public static uint AlTestRawFileOffset(SocketHeaderCaseContinuationCase @case) =>
        GetRoute(@case).AlTestRawFileOffset;

    public static uint BranchRawFileOffset(SocketHeaderCaseContinuationCase @case) =>
        GetRoute(@case).BranchRawFileOffset;

    public static uint BranchFallthroughRawFileOffset(SocketHeaderCaseContinuationCase @case) =>
        GetRoute(@case).BranchFallthroughRawFileOffset;

    public static ImmutableArray<SocketHeaderCaseContinuationInstructionEvidence> Instructions(
        SocketHeaderCaseContinuationCase @case)
    {
        Route route = GetRoute(@case);
        return ImmutableArray.Create(
            new SocketHeaderCaseContinuationInstructionEvidence(
                "Push EDI before the getter; this preexisting stack entry survives the getter's plain RET",
                route.PushedEdiRawFileOffset,
                "57"),
            new SocketHeaderCaseContinuationInstructionEvidence(
                "Call the getter; its normal return reaches the post-getter MOV",
                route.GetterCallRawFileOffset,
                route.GetterCallBytes,
                TargetRawFileOffset: route.GetterRawFileOffset,
                TargetVirtualAddress: RawFileOffsetToVirtualAddress(route.GetterRawFileOffset)),
            new SocketHeaderCaseContinuationInstructionEvidence(
                "The getter returns with plain RET and does not remove the earlier pushed EDI",
                route.GetterPlainRetRawFileOffset,
                GetterPlainReturnBytes),
            new SocketHeaderCaseContinuationInstructionEvidence(
                "Copy the getter's captured EAX into ECX for the next call",
                route.PostGetterRawFileOffset,
                MoveGetterEaxToEcxBytes),
            new SocketHeaderCaseContinuationInstructionEvidence(
                "Direct call after MOV ECX,EAX",
                route.NextCallRawFileOffset,
                route.NextCallBytes,
                TargetRawFileOffset: route.NextCallTargetRawFileOffset,
                TargetVirtualAddress: RawFileOffsetToVirtualAddress(route.NextCallTargetRawFileOffset)),
            new SocketHeaderCaseContinuationInstructionEvidence(
                "Test the opaque return's low byte as an unsigned byte against zero",
                route.AlTestRawFileOffset,
                AlTestBytes),
            new SocketHeaderCaseContinuationInstructionEvidence(
                "JNZ selects the common destination when AL is nonzero",
                route.BranchRawFileOffset,
                route.ConditionalBranchBytes,
                BranchTargetRawFileOffset: BranchTakenDestinationRawFileOffset,
                TargetVirtualAddress: RawFileOffsetToVirtualAddress(BranchTakenDestinationRawFileOffset)));
    }

    public static SocketHeaderCaseContinuationPlan Project(
        SocketHeaderCaseContinuationCapture capture)
    {
        Route route = GetRoute(capture.Case);
        uint? getterEax = capture.GetterEax;
        if (getterEax is null)
        {
            return Plan(
                capture,
                capture.OpaqueCallReturnAl is null
                    ? SocketHeaderCaseContinuationOutcome.MissingGetterEaxCapture
                    : SocketHeaderCaseContinuationOutcome.UnreachedOpaqueReturnAlCapture,
                route,
                getterEax: null,
                opaqueCallReturnAl: null,
                branchTaken: null,
                selectedBranchDestinationRawFileOffset: null,
                pushedEdiStackEntryRemainsBeforeNextCall: false,
                stopAtRawFileOffsetExclusive: route.GetterCallRawFileOffset);
        }

        if (capture.OpaqueCallReturnAl is not byte returnAl)
        {
            return Plan(
                capture,
                SocketHeaderCaseContinuationOutcome.MissingOpaqueReturnAlCapture,
                route,
                getterEax,
                opaqueCallReturnAl: null,
                branchTaken: null,
                selectedBranchDestinationRawFileOffset: null,
                pushedEdiStackEntryRemainsBeforeNextCall: true,
                stopAtRawFileOffsetExclusive: route.AlTestRawFileOffset);
        }

        bool branchTaken = returnAl != 0;
        uint destination = branchTaken
            ? BranchTakenDestinationRawFileOffset
            : route.BranchFallthroughRawFileOffset;
        return Plan(
            capture,
            SocketHeaderCaseContinuationOutcome.BranchSelected,
            route,
            getterEax,
            returnAl,
            branchTaken,
            destination,
            pushedEdiStackEntryRemainsBeforeNextCall: true,
            stopAtRawFileOffsetExclusive: destination);
    }

    private static SocketHeaderCaseContinuationPlan Plan(
        SocketHeaderCaseContinuationCapture capture,
        SocketHeaderCaseContinuationOutcome outcome,
        Route route,
        uint? getterEax,
        byte? opaqueCallReturnAl,
        bool? branchTaken,
        uint? selectedBranchDestinationRawFileOffset,
        bool pushedEdiStackEntryRemainsBeforeNextCall,
        uint stopAtRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            getterEax,
            NextCallReached: getterEax.HasValue,
            NextCallEcX: getterEax,
            route.NextCallRawFileOffset,
            route.NextCallTargetRawFileOffset,
            RawFileOffsetToVirtualAddress(route.NextCallTargetRawFileOffset),
            opaqueCallReturnAl,
            opaqueCallReturnAl is null ? null : opaqueCallReturnAl.Value != 0,
            branchTaken,
            route.BranchRawFileOffset,
            BranchTakenDestinationRawFileOffset,
            route.BranchFallthroughRawFileOffset,
            selectedBranchDestinationRawFileOffset,
            selectedBranchDestinationRawFileOffset is uint destination
                ? RawFileOffsetToVirtualAddress(destination)
                : null,
            capture.PushedEdiValue,
            pushedEdiStackEntryRemainsBeforeNextCall,
            stopAtRawFileOffsetExclusive);

    private static Route GetRoute(SocketHeaderCaseContinuationCase @case) =>
        @case switch
        {
            SocketHeaderCaseContinuationCase.Case1 => new(
                Case1PushedEdiRawFileOffset,
                Case1GetterCallRawFileOffset,
                Case1GetterRawFileOffset,
                Case1GetterPlainRetRawFileOffset,
                Case1PostGetterRawFileOffset,
                Case1NextCallRawFileOffset,
                Case1NextCallTargetRawFileOffset,
                Case1AlTestRawFileOffset,
                Case1BranchRawFileOffset,
                Case1BranchFallthroughRawFileOffset,
                Case1GetterCallBytes,
                Case1NextCallBytes,
                Case1ConditionalBranchBytes),
            SocketHeaderCaseContinuationCase.Case2 => new(
                Case2PushedEdiRawFileOffset,
                Case2GetterCallRawFileOffset,
                Case2GetterRawFileOffset,
                Case2GetterPlainRetRawFileOffset,
                Case2PostGetterRawFileOffset,
                Case2NextCallRawFileOffset,
                Case2NextCallTargetRawFileOffset,
                Case2AlTestRawFileOffset,
                Case2BranchRawFileOffset,
                Case2BranchFallthroughRawFileOffset,
                Case2GetterCallBytes,
                Case2NextCallBytes,
                Case2ConditionalBranchBytes),
            SocketHeaderCaseContinuationCase.Case3 => new(
                Case3PushedEdiRawFileOffset,
                Case3GetterCallRawFileOffset,
                Case3GetterRawFileOffset,
                Case3GetterPlainRetRawFileOffset,
                Case3PostGetterRawFileOffset,
                Case3NextCallRawFileOffset,
                Case3NextCallTargetRawFileOffset,
                Case3AlTestRawFileOffset,
                Case3BranchRawFileOffset,
                Case3BranchFallthroughRawFileOffset,
                Case3GetterCallBytes,
                Case3NextCallBytes,
                Case3ConditionalBranchBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(@case), @case, "Unsupported socket-header case.")
        };

    private readonly record struct Route(
        uint PushedEdiRawFileOffset,
        uint GetterCallRawFileOffset,
        uint GetterRawFileOffset,
        uint GetterPlainRetRawFileOffset,
        uint PostGetterRawFileOffset,
        uint NextCallRawFileOffset,
        uint NextCallTargetRawFileOffset,
        uint AlTestRawFileOffset,
        uint BranchRawFileOffset,
        uint BranchFallthroughRawFileOffset,
        string GetterCallBytes,
        string NextCallBytes,
        string ConditionalBranchBytes);
}
