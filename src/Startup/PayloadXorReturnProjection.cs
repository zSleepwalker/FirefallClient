using System.Collections.ObjectModel;

namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadXorReturnCapture(
    bool EntryBoundaryValidated,
    uint? EaxAtEntry);

public enum PayloadXorReturnOutcome
{
    EntryBoundaryNotValidated,
    NativeReturnReached
}

public enum PayloadXorReturnActionKind
{
    PopEsi,
    PopEbx,
    ResetEspFromEbp,
    PopEbp,
    Return
}

public readonly record struct PayloadXorReturnAction(
    PayloadXorReturnActionKind Kind,
    uint RawFileOffset);

public readonly record struct PayloadXorReturnPlan(
    PayloadXorReturnCapture Capture,
    PayloadXorReturnOutcome Outcome,
    bool UnreachedEaxAtEntryCaptureProvided,
    bool EsiPopped,
    bool EbxPopped,
    bool FrameStackPointerReset,
    bool EbpPopped,
    uint? EaxAtReturn,
    bool EaxPreservedThroughEpilogue,
    bool ReturnRegisterWriteProjected,
    bool ReturnReached,
    bool ReturnImmediatePresent,
    uint StopAtRawFileOffsetExclusive,
    PayloadXorReturnAction[] Actions);

public readonly record struct PayloadXorReturnInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the native epilogue at raw 0xBD594E..0xBD5954. It records the
/// ESP-from-EBP reset without inferring numeric ESP changes.
/// </summary>
public static class PayloadXorReturnProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryStartRawFileOffset = 0x00BD594E;
    public const uint EntryEndRawFileOffsetExclusive = 0x00BD5954;
    public const uint CapturedCodeLength =
        EntryEndRawFileOffsetExclusive - EntryStartRawFileOffset;

    public const uint PopEsiRawFileOffset = 0x00BD594E;
    public const uint PopEbxRawFileOffset = 0x00BD594F;
    public const uint FrameStackPointerResetRawFileOffset = 0x00BD5950;
    public const uint PopEbpRawFileOffset = 0x00BD5952;
    public const uint ReturnRawFileOffset = 0x00BD5953;

    public const string CapturedCodeBytes = "5E 5B 8B E5 5D C3";

    public static uint EntryStartVirtualAddress =>
        RawFileOffsetToVirtualAddress(EntryStartRawFileOffset);

    public static uint EntryEndVirtualAddressExclusive =>
        RawFileOffsetToVirtualAddress(EntryEndRawFileOffsetExclusive);

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static ReadOnlyCollection<PayloadXorReturnInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new PayloadXorReturnInstructionEvidence(
                "Pop ESI",
                PopEsiRawFileOffset,
                RawFileOffsetToVirtualAddress(PopEsiRawFileOffset),
                "5E"),
            new PayloadXorReturnInstructionEvidence(
                "Pop EBX",
                PopEbxRawFileOffset,
                RawFileOffsetToVirtualAddress(PopEbxRawFileOffset),
                "5B"),
            new PayloadXorReturnInstructionEvidence(
                "Reset ESP from EBP",
                FrameStackPointerResetRawFileOffset,
                RawFileOffsetToVirtualAddress(FrameStackPointerResetRawFileOffset),
                "8B E5"),
            new PayloadXorReturnInstructionEvidence(
                "Pop EBP",
                PopEbpRawFileOffset,
                RawFileOffsetToVirtualAddress(PopEbpRawFileOffset),
                "5D"),
            new PayloadXorReturnInstructionEvidence(
                "Return without an immediate",
                ReturnRawFileOffset,
                RawFileOffsetToVirtualAddress(ReturnRawFileOffset),
                "C3")
        });

    public static PayloadXorReturnPlan Project(PayloadXorReturnCapture capture)
    {
        if (!capture.EntryBoundaryValidated)
        {
            return new PayloadXorReturnPlan(
                capture,
                PayloadXorReturnOutcome.EntryBoundaryNotValidated,
                UnreachedEaxAtEntryCaptureProvided: capture.EaxAtEntry.HasValue,
                EsiPopped: false,
                EbxPopped: false,
                FrameStackPointerReset: false,
                EbpPopped: false,
                EaxAtReturn: null,
                EaxPreservedThroughEpilogue: false,
                ReturnRegisterWriteProjected: false,
                ReturnReached: false,
                ReturnImmediatePresent: false,
                StopAtRawFileOffsetExclusive: EntryStartRawFileOffset,
                Actions: Array.Empty<PayloadXorReturnAction>());
        }

        return new PayloadXorReturnPlan(
            capture,
            PayloadXorReturnOutcome.NativeReturnReached,
            UnreachedEaxAtEntryCaptureProvided: false,
            EsiPopped: true,
            EbxPopped: true,
            FrameStackPointerReset: true,
            EbpPopped: true,
            EaxAtReturn: capture.EaxAtEntry,
            EaxPreservedThroughEpilogue: true,
            ReturnRegisterWriteProjected: false,
            ReturnReached: true,
            ReturnImmediatePresent: false,
            StopAtRawFileOffsetExclusive: EntryEndRawFileOffsetExclusive,
            Actions: new[]
            {
                Action(PayloadXorReturnActionKind.PopEsi, PopEsiRawFileOffset),
                Action(PayloadXorReturnActionKind.PopEbx, PopEbxRawFileOffset),
                Action(
                    PayloadXorReturnActionKind.ResetEspFromEbp,
                    FrameStackPointerResetRawFileOffset),
                Action(PayloadXorReturnActionKind.PopEbp, PopEbpRawFileOffset),
                Action(PayloadXorReturnActionKind.Return, ReturnRawFileOffset)
            });
    }

    private static PayloadXorReturnAction Action(
        PayloadXorReturnActionKind kind,
        uint rawFileOffset) =>
        new(kind, rawFileOffset);
}
