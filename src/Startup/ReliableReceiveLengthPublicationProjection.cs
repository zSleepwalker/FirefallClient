using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct ReliableReceiveLengthPublicationCapture(
    ReliableReceiveLengthCallbackCapture? LengthCallback,
    ReliableReceiveCallbackScalarHandoffCapture? ScalarHandoff,
    uint? ScalarPayloadEdi,
    bool? PublicationEntryValidated,
    uint? PublicationFrameEbp,
    uint? PublicationPayloadEdi,
    ReliableReceiveSlotPublicationCapture? SlotPublication);

public enum ReliableReceiveLengthPublicationOutcome
{
    MissingLengthCallbackCapture,
    UnreachedLaterCaptureProvided,
    LengthCallbackNotAdmitted,
    MissingScalarHandoffCapture,
    ScalarPayloadMismatch,
    ScalarHandoffNotAdmitted,
    MissingPublicationEntryValidation,
    PublicationEntryNotValidated,
    MissingPublicationFrameEbp,
    MissingPublicationPayloadEdi,
    PublicationPayloadMismatch,
    SlotPublicationNotAdmitted,
    PublicationSequenceProjected
}

public readonly record struct ReliableReceiveLengthPublicationPlan(
    ReliableReceiveLengthPublicationCapture Capture,
    ReliableReceiveLengthPublicationOutcome Outcome,
    ReliableReceiveLengthCallbackPlan? LengthCallbackPlan,
    ReliableReceiveCallbackScalarHandoffPlan? ScalarHandoffPlan,
    ReliableReceiveSlotPublicationPlan? SlotPublicationPlan,
    bool UnreachedScalarHandoffCaptureProvided,
    bool UnreachedScalarPayloadEdiCaptureProvided,
    bool UnreachedPublicationEntryValidationCaptureProvided,
    bool UnreachedPublicationFrameEbpCaptureProvided,
    bool UnreachedPublicationPayloadEdiCaptureProvided,
    bool UnreachedSlotPublicationCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct ReliableReceiveLengthPublicationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Strictly composes the length callback, scalar handoff, and slot publication
/// projections, admitting each later stage only at its validated predecessor boundary.
/// </summary>
public static class ReliableReceiveLengthPublicationProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE53BF;
    public const uint EntryVirtualAddress = 0x00FE5FBF;
    public const uint EndRawFileOffsetExclusive = 0x00BE53F7;
    public const uint EndVirtualAddressExclusive = 0x00FE5FF7;

    public const string CapturedWindowBytes =
        "66 8B 07 B9 FF 07 00 00 66 23 C1 8D 4F 08 0F B7 C0 "
        + "83 E8 04 50 8B 47 08 FF 77 04 FF 50 04 "
        + "8B D0 8A CB E8 FA 04 FF FF 83 C4 04 "
        + "8B 45 08 89 38 B0 01 5F 5E 5B 5D C2 04 00";

    public static ImmutableArray<ReliableReceiveLengthPublicationInstructionEvidence>
        Instructions { get; } =
        ReliableReceiveLengthCallbackProjection.Instructions
            .Select(instruction =>
                new ReliableReceiveLengthPublicationInstructionEvidence(
                    instruction.Observation,
                    instruction.RawFileOffset,
                    instruction.VirtualAddress,
                    instruction.Bytes))
            .Concat(
                ReliableReceiveCallbackScalarHandoffProjection.Instructions.Select(
                    instruction =>
                        new ReliableReceiveLengthPublicationInstructionEvidence(
                            instruction.Observation,
                            instruction.RawFileOffset,
                            instruction.VirtualAddress,
                            instruction.Bytes)))
            .Concat(
                ReliableReceiveSlotPublicationProjection.Instructions.Select(
                    instruction =>
                        new ReliableReceiveLengthPublicationInstructionEvidence(
                            instruction.Observation,
                            instruction.RawFileOffset,
                            instruction.VirtualAddress,
                            instruction.Bytes)))
            .ToImmutableArray();

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static ReliableReceiveLengthPublicationPlan Project(
        ReliableReceiveLengthPublicationCapture capture)
    {
        if (capture.LengthCallback is not { } lengthCapture)
        {
            return Stop(
                capture,
                HasAnyLaterCapture(capture)
                    ? ReliableReceiveLengthPublicationOutcome.UnreachedLaterCaptureProvided
                    : ReliableReceiveLengthPublicationOutcome.MissingLengthCallbackCapture,
                unreachedScalarHandoff: capture.ScalarHandoff.HasValue,
                unreachedScalarPayloadEdi: capture.ScalarPayloadEdi.HasValue,
                unreachedPublicationEntryValidation: capture.PublicationEntryValidated.HasValue,
                unreachedPublicationFrameEbp: capture.PublicationFrameEbp.HasValue,
                unreachedPublicationPayloadEdi: capture.PublicationPayloadEdi.HasValue,
                unreachedSlotPublication: HasSlotPublicationCapture(capture),
                stopAt: EntryRawFileOffset);
        }

        ReliableReceiveLengthCallbackPlan lengthPlan =
            ReliableReceiveLengthCallbackProjection.Project(lengthCapture);
        if (!IsLengthBoundaryAdmitted(lengthPlan))
        {
            return Stop(
                capture,
                ReliableReceiveLengthPublicationOutcome.LengthCallbackNotAdmitted,
                lengthCallbackPlan: lengthPlan,
                unreachedScalarHandoff: capture.ScalarHandoff.HasValue,
                unreachedScalarPayloadEdi: capture.ScalarPayloadEdi.HasValue,
                unreachedPublicationEntryValidation: capture.PublicationEntryValidated.HasValue,
                unreachedPublicationFrameEbp: capture.PublicationFrameEbp.HasValue,
                unreachedPublicationPayloadEdi: capture.PublicationPayloadEdi.HasValue,
                unreachedSlotPublication: HasSlotPublicationCapture(capture),
                stopAt: lengthPlan.StopAtRawFileOffsetExclusive);
        }

        if (capture.ScalarHandoff is not { } scalarCapture)
        {
            return Stop(
                capture,
                ReliableReceiveLengthPublicationOutcome.MissingScalarHandoffCapture,
                lengthCallbackPlan: lengthPlan,
                unreachedPublicationEntryValidation: capture.PublicationEntryValidated.HasValue,
                unreachedPublicationFrameEbp: capture.PublicationFrameEbp.HasValue,
                unreachedPublicationPayloadEdi: capture.PublicationPayloadEdi.HasValue,
                unreachedSlotPublication: HasSlotPublicationCapture(capture),
                stopAt: ReliableReceiveCallbackScalarHandoffProjection.EntryRawFileOffset);
        }

        if (scalarCapture.PriorCallbackReturnedNormally is true
            && capture.ScalarPayloadEdi is uint scalarPayloadEdi
            && scalarPayloadEdi != lengthCapture.PayloadEdi)
        {
            return Stop(
                capture,
                ReliableReceiveLengthPublicationOutcome.ScalarPayloadMismatch,
                lengthCallbackPlan: lengthPlan,
                unreachedPublicationEntryValidation: capture.PublicationEntryValidated.HasValue,
                unreachedPublicationFrameEbp: capture.PublicationFrameEbp.HasValue,
                unreachedPublicationPayloadEdi: capture.PublicationPayloadEdi.HasValue,
                unreachedSlotPublication: HasSlotPublicationCapture(capture),
                stopAt: ReliableReceiveCallbackScalarHandoffProjection.EntryRawFileOffset);
        }

        ReliableReceiveCallbackScalarHandoffPlan scalarPlan =
            ReliableReceiveCallbackScalarHandoffProjection.Project(scalarCapture);
        if (!IsScalarBoundaryAdmitted(scalarPlan))
        {
            return Stop(
                capture,
                ReliableReceiveLengthPublicationOutcome.ScalarHandoffNotAdmitted,
                lengthCallbackPlan: lengthPlan,
                scalarHandoffPlan: scalarPlan,
                unreachedPublicationEntryValidation: capture.PublicationEntryValidated.HasValue,
                unreachedPublicationFrameEbp: capture.PublicationFrameEbp.HasValue,
                unreachedPublicationPayloadEdi: capture.PublicationPayloadEdi.HasValue,
                unreachedSlotPublication: HasSlotPublicationCapture(capture),
                stopAt: scalarPlan.StopAtRawFileOffsetExclusive);
        }

        if (capture.PublicationEntryValidated is not bool entryValidated)
        {
            return Stop(
                capture,
                ReliableReceiveLengthPublicationOutcome.MissingPublicationEntryValidation,
                lengthCallbackPlan: lengthPlan,
                scalarHandoffPlan: scalarPlan,
                unreachedPublicationFrameEbp: capture.PublicationFrameEbp.HasValue,
                unreachedPublicationPayloadEdi: capture.PublicationPayloadEdi.HasValue,
                unreachedSlotPublication: HasSlotPublicationCapture(capture),
                stopAt: ReliableReceiveSlotPublicationProjection.PublicationStartRawFileOffset);
        }

        if (!entryValidated)
        {
            return Stop(
                capture,
                ReliableReceiveLengthPublicationOutcome.PublicationEntryNotValidated,
                lengthCallbackPlan: lengthPlan,
                scalarHandoffPlan: scalarPlan,
                unreachedPublicationFrameEbp: capture.PublicationFrameEbp.HasValue,
                unreachedPublicationPayloadEdi: capture.PublicationPayloadEdi.HasValue,
                unreachedSlotPublication: HasSlotPublicationCapture(capture),
                stopAt: ReliableReceiveSlotPublicationProjection.PublicationStartRawFileOffset);
        }

        if (capture.PublicationFrameEbp is not uint frameEbp)
        {
            return Stop(
                capture,
                ReliableReceiveLengthPublicationOutcome.MissingPublicationFrameEbp,
                lengthCallbackPlan: lengthPlan,
                scalarHandoffPlan: scalarPlan,
                unreachedPublicationPayloadEdi: capture.PublicationPayloadEdi.HasValue,
                unreachedSlotPublication: HasSlotPublicationCapture(capture),
                stopAt: ReliableReceiveSlotPublicationProjection.FrameArgument8ReadRawFileOffset);
        }

        if (capture.PublicationPayloadEdi is not uint publicationPayloadEdi)
        {
            return Stop(
                capture,
                ReliableReceiveLengthPublicationOutcome.MissingPublicationPayloadEdi,
                lengthCallbackPlan: lengthPlan,
                scalarHandoffPlan: scalarPlan,
                unreachedSlotPublication: HasSlotPublicationCapture(capture),
                stopAt: ReliableReceiveSlotPublicationProjection.PayloadDwordWriteRawFileOffset);
        }

        if (publicationPayloadEdi != lengthCapture.PayloadEdi)
        {
            return Stop(
                capture,
                ReliableReceiveLengthPublicationOutcome.PublicationPayloadMismatch,
                lengthCallbackPlan: lengthPlan,
                scalarHandoffPlan: scalarPlan,
                unreachedSlotPublication: HasSlotPublicationCapture(capture),
                stopAt: ReliableReceiveSlotPublicationProjection.PayloadDwordWriteRawFileOffset);
        }

        ReliableReceiveSlotPublicationPlan publicationPlan =
            ReliableReceiveSlotPublicationProjection.Project(
                frameEbp,
                publicationPayloadEdi,
                capture.SlotPublication
                    ?? new ReliableReceiveSlotPublicationCapture(null));
        if (!IsPublicationSequenceProjected(publicationPlan))
        {
            return Stop(
                capture,
                ReliableReceiveLengthPublicationOutcome.SlotPublicationNotAdmitted,
                lengthCallbackPlan: lengthPlan,
                scalarHandoffPlan: scalarPlan,
                slotPublicationPlan: publicationPlan,
                stopAt: publicationPlan.StopAtRawFileOffsetExclusive);
        }

        return Stop(
            capture,
            ReliableReceiveLengthPublicationOutcome.PublicationSequenceProjected,
            lengthCallbackPlan: lengthPlan,
            scalarHandoffPlan: scalarPlan,
            slotPublicationPlan: publicationPlan,
            stopAt: publicationPlan.StopAtRawFileOffsetExclusive);
    }

    private static bool IsLengthBoundaryAdmitted(
        ReliableReceiveLengthCallbackPlan plan) =>
        plan.Outcome == ReliableReceiveLengthCallbackOutcome.IndirectCallReturnedNormally
        && plan.Capture.EntryBoundaryValidated
        && plan.IndirectCallReturnedNormally == true
        && plan.NormalCallBoundaryReached
        && plan.SelectedDestinationRawFileOffset
            == ReliableReceiveCallbackScalarHandoffProjection.EntryRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == ReliableReceiveCallbackScalarHandoffProjection.EntryRawFileOffset;

    private static bool IsScalarBoundaryAdmitted(
        ReliableReceiveCallbackScalarHandoffPlan plan) =>
        plan.Outcome == ReliableReceiveCallbackScalarHandoffOutcome.HelperReturnedNormally
        && plan.PriorCallbackReturnedNormally == true
        && plan.HelperCallReached
        && plan.HelperReturnedNormally == true
        && plan.StackCleanupBytes
            == ReliableReceiveCallbackScalarHandoffProjection.StackCleanupAmount
        && plan.SelectedDestinationRawFileOffset
            == ReliableReceiveSlotPublicationProjection.PublicationStartRawFileOffset
        && plan.StopAtRawFileOffsetExclusive
            == ReliableReceiveSlotPublicationProjection.PublicationStartRawFileOffset;

    private static bool IsPublicationSequenceProjected(
        ReliableReceiveSlotPublicationPlan plan) =>
        plan.Outcome == ReliableReceiveSlotPublicationOutcome.FrameArgument8CaptureAvailable
        && plan.FrameArgument8Read
        && plan.CapturedPointer.HasValue
        && plan.PayloadDwordWriteProjected
        && plan.PayloadDwordWriteAddress == plan.CapturedPointer.Value
        && plan.PayloadDwordWriteValue == plan.PayloadEdi
        && plan.AlOneEffectProjected
        && plan.ProjectedEaxAfterAlOne.HasValue
        && plan.ProjectedEaxAfterAlOne.Value
            == ((plan.CapturedPointer.Value & 0xFFFFFF00u) | 0x01u)
        && plan.StackPopRegisters.SequenceEqual(new[] { "EDI", "ESI", "EBX", "EBP" })
        && plan.ReturnStackBytes
            == ReliableReceiveSlotPublicationProjection.ReturnStackBytes
        && plan.StopAtRawFileOffsetExclusive
            == ReliableReceiveSlotPublicationProjection.PublicationEndRawFileOffsetExclusive;

    private static bool HasAnyLaterCapture(
        ReliableReceiveLengthPublicationCapture capture) =>
        capture.ScalarHandoff.HasValue
        || capture.ScalarPayloadEdi.HasValue
        || capture.PublicationEntryValidated.HasValue
        || capture.PublicationFrameEbp.HasValue
        || capture.PublicationPayloadEdi.HasValue
        || HasSlotPublicationCapture(capture);

    private static bool HasSlotPublicationCapture(
        ReliableReceiveLengthPublicationCapture capture) =>
        capture.SlotPublication?.FrameArgument8Dword.HasValue == true;

    private static ReliableReceiveLengthPublicationPlan Stop(
        ReliableReceiveLengthPublicationCapture capture,
        ReliableReceiveLengthPublicationOutcome outcome,
        ReliableReceiveLengthCallbackPlan? lengthCallbackPlan = null,
        ReliableReceiveCallbackScalarHandoffPlan? scalarHandoffPlan = null,
        ReliableReceiveSlotPublicationPlan? slotPublicationPlan = null,
        bool unreachedScalarHandoff = false,
        bool unreachedScalarPayloadEdi = false,
        bool unreachedPublicationEntryValidation = false,
        bool unreachedPublicationFrameEbp = false,
        bool unreachedPublicationPayloadEdi = false,
        bool unreachedSlotPublication = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            lengthCallbackPlan,
            scalarHandoffPlan,
            slotPublicationPlan,
            unreachedScalarHandoff,
            unreachedScalarPayloadEdi,
            unreachedPublicationEntryValidation,
            unreachedPublicationFrameEbp,
            unreachedPublicationPayloadEdi,
            unreachedSlotPublication,
            stopAt);
}
