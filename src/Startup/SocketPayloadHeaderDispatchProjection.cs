using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct SocketPayloadHeaderDispatchCapture(
    uint HeaderPointer,
    uint ContextPointer,
    ushort? HeaderWord);

public enum SocketPayloadHeaderDispatchOutcome
{
    MissingHeaderWord,
    Case0StopsBeforeOpaqueCall,
    Case1StopsBeforeOpaqueCall,
    Case2StopsBeforeOpaqueCall,
    Case3StopsBeforeOpaqueCall
}

public enum SocketPayloadHeaderDispatchRoute : byte
{
    Case0,
    Case1,
    Case2,
    Case3
}

public readonly record struct SocketPayloadHeaderDispatchPlan(
    SocketPayloadHeaderDispatchCapture Capture,
    SocketPayloadHeaderDispatchOutcome Outcome,
    uint HeaderWordAddress,
    SocketPayloadHeaderDispatchRoute? Route,
    uint? CaseRawFileOffset,
    uint? EdiValuePushed,
    uint? EcxValueAtOpaqueCall,
    uint? OpaqueCallInstructionRawFileOffset,
    uint? OpaqueCallTargetRawFileOffset,
    uint? OpaqueCallTargetVirtualAddress,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct SocketPayloadHeaderDispatchInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null);

/// <summary>
/// Projects the unsigned top-two-bit header switch and stops immediately
/// before its first opaque case call. The header capture is independent of
/// any preceding socket gate; no call ABI or post-call behavior is modeled.
/// </summary>
public static class SocketPayloadHeaderDispatchProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint HeaderDispatchRawFileOffset = 0x00BD412B;
    public const uint HeaderWordReadRawFileOffset = HeaderDispatchRawFileOffset;
    public const uint HeaderShiftRawFileOffset = 0x00BD412E;
    public const uint RouteCompareRawFileOffset = 0x00BD4131;
    public const uint DefaultBranchRawFileOffset = 0x00BD4134;
    public const uint IndirectJumpRawFileOffset = 0x00BD413A;
    public const uint DefaultRouteRawFileOffset = 0x00BD41FA;

    public const uint JumpTableVirtualAddress = 0x00FD4E2C;
    public const uint JumpTableRawFileOffset = 0x00BD422C;

    public const uint Case0RawFileOffset = 0x00BD4141;
    public const uint Case1RawFileOffset = 0x00BD415A;
    public const uint Case2RawFileOffset = 0x00BD418D;
    public const uint Case3RawFileOffset = 0x00BD41C0;

    public const uint Case0CallRawFileOffset = 0x00BD4144;
    public const uint Case1CallRawFileOffset = 0x00BD4161;
    public const uint Case2CallRawFileOffset = 0x00BD4194;
    public const uint Case3CallRawFileOffset = 0x00BD41C7;

    public const uint Case0CallTargetRawFileOffset = 0x00BD3760;
    public const uint Case1CallTargetRawFileOffset = 0x00BD5BA0;
    public const uint Case2CallTargetRawFileOffset = 0x00BD5BA0;
    public const uint Case3CallTargetRawFileOffset = 0x00BD5C00;

    public const uint Case1ContextDisplacement = 0xD6C;
    public const uint Case2ContextDisplacement = 0xD70;
    public const uint Case3ContextDisplacement = 0xD74;

    public const uint HeaderDispatchVirtualAddress =
        HeaderDispatchRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint JumpTableRawVirtualAddress =
        JumpTableRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public static ReadOnlyCollection<SocketPayloadHeaderDispatchInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new SocketPayloadHeaderDispatchInstructionEvidence(
                "Zero-extend the word at EDI into EAX",
                HeaderWordReadRawFileOffset,
                "0F B7 07"),
            new SocketPayloadHeaderDispatchInstructionEvidence(
                "Shift the unsigned header word right by 14 bits",
                HeaderShiftRawFileOffset,
                "C1 E8 0E"),
            new SocketPayloadHeaderDispatchInstructionEvidence(
                "Compare the route index with 3 and branch above it to the default",
                RouteCompareRawFileOffset,
                "83 F8 03"),
            new SocketPayloadHeaderDispatchInstructionEvidence(
                "Unsigned-above route indices branch to the default handler",
                DefaultBranchRawFileOffset,
                "0F 87 C0 00 00 00",
                DefaultRouteRawFileOffset),
            new SocketPayloadHeaderDispatchInstructionEvidence(
                "Dispatch through the four-entry absolute-address table",
                IndirectJumpRawFileOffset,
                "FF 24 85 2C 4E FD 00"),
            new SocketPayloadHeaderDispatchInstructionEvidence(
                "The four table slots contain the case virtual addresses",
                JumpTableRawFileOffset,
                "41 4D FD 00 5A 4D FD 00 8D 4D FD 00 C0 4D FD 00"),
            new SocketPayloadHeaderDispatchInstructionEvidence(
                "Case 0 pushes EDI, loads ECX from ESI, then calls its opaque target",
                Case0RawFileOffset,
                "57 8B CE E8 17 F6 FF FF",
                Case0CallTargetRawFileOffset),
            new SocketPayloadHeaderDispatchInstructionEvidence(
                "Case 1 pushes EDI, computes ESI plus 0xD6C into ECX, then calls its opaque target",
                Case1RawFileOffset,
                "57 8D 8E 6C 0D 00 00 E8 3A 1A 00 00",
                Case1CallTargetRawFileOffset),
            new SocketPayloadHeaderDispatchInstructionEvidence(
                "Case 2 pushes EDI, computes ESI plus 0xD70 into ECX, then calls its opaque target",
                Case2RawFileOffset,
                "57 8D 8E 70 0D 00 00 E8 07 1A 00 00",
                Case2CallTargetRawFileOffset),
            new SocketPayloadHeaderDispatchInstructionEvidence(
                "Case 3 pushes EDI, computes ESI plus 0xD74 into ECX, then calls its opaque target",
                Case3RawFileOffset,
                "57 8D 8E 74 0D 00 00 E8 34 1A 00 00",
                Case3CallTargetRawFileOffset)
        });

    /// <summary>
    /// A missing header word stops at the read boundary. Otherwise the route
    /// is (word &gt;&gt; 14), and the plan records registers and the call boundary
    /// without executing or assigning semantics to the opaque call.
    /// </summary>
    public static SocketPayloadHeaderDispatchPlan Project(
        SocketPayloadHeaderDispatchCapture capture)
    {
        if (capture.HeaderWord is not ushort headerWord)
        {
            return new(
                capture,
                SocketPayloadHeaderDispatchOutcome.MissingHeaderWord,
                capture.HeaderPointer,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                HeaderWordReadRawFileOffset);
        }

        SocketPayloadHeaderDispatchRoute route =
            (SocketPayloadHeaderDispatchRoute)(headerWord >> 14);
        (uint caseOffset, uint contextDisplacement, uint callOffset, uint targetOffset) =
            route switch
            {
                SocketPayloadHeaderDispatchRoute.Case0 =>
                    (Case0RawFileOffset, 0u, Case0CallRawFileOffset, Case0CallTargetRawFileOffset),
                SocketPayloadHeaderDispatchRoute.Case1 =>
                    (Case1RawFileOffset, Case1ContextDisplacement, Case1CallRawFileOffset, Case1CallTargetRawFileOffset),
                SocketPayloadHeaderDispatchRoute.Case2 =>
                    (Case2RawFileOffset, Case2ContextDisplacement, Case2CallRawFileOffset, Case2CallTargetRawFileOffset),
                SocketPayloadHeaderDispatchRoute.Case3 =>
                    (Case3RawFileOffset, Case3ContextDisplacement, Case3CallRawFileOffset, Case3CallTargetRawFileOffset),
                _ => throw new InvalidOperationException("The 2-bit route is outside the dispatch table.")
            };

        return new(
            capture,
            OutcomeFor(route),
            capture.HeaderPointer,
            route,
            caseOffset,
            capture.HeaderPointer,
            AddDword(capture.ContextPointer, contextDisplacement),
            callOffset,
            targetOffset,
            AddDword(targetOffset, CodeVirtualAddressDeltaFromRawFileOffset),
            callOffset);
    }

    public static uint AddDword(uint value, uint displacement) =>
        unchecked(value + displacement);

    private static SocketPayloadHeaderDispatchOutcome OutcomeFor(
        SocketPayloadHeaderDispatchRoute route) =>
        route switch
        {
            SocketPayloadHeaderDispatchRoute.Case0 =>
                SocketPayloadHeaderDispatchOutcome.Case0StopsBeforeOpaqueCall,
            SocketPayloadHeaderDispatchRoute.Case1 =>
                SocketPayloadHeaderDispatchOutcome.Case1StopsBeforeOpaqueCall,
            SocketPayloadHeaderDispatchRoute.Case2 =>
                SocketPayloadHeaderDispatchOutcome.Case2StopsBeforeOpaqueCall,
            SocketPayloadHeaderDispatchRoute.Case3 =>
                SocketPayloadHeaderDispatchOutcome.Case3StopsBeforeOpaqueCall,
            _ => throw new InvalidOperationException("The 2-bit route is outside the dispatch table.")
        };
}
