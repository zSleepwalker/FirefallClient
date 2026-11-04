using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolPopPair(
    uint Head,
    uint HighTag);

public readonly record struct UiShaderSourcePoolPopCapture(
    uint ReceiverPairPointer,
    uint CapturedHighTag,
    uint CapturedHead,
    uint? CapturedNextHead,
    uint? CompareExchangeReturnedHead,
    uint? CompareExchangeReturnedHighTag);

public enum UiShaderSourcePoolPopOutcome
{
    EmptyCapturedHead,
    ReturnedCapturedHead,
    RetryBoundary
}

public enum UiShaderSourcePoolPopActionKind
{
    ReturnZeroWithoutExchange,
    ReadCapturedHeadLink,
    AttemptLockedCompareExchange,
    ReturnCapturedHead,
    StopAtUnmodeledRetryBoundary
}

public readonly record struct UiShaderSourcePoolPopAction(
    UiShaderSourcePoolPopActionKind Kind,
    uint InstructionRawFileOffset,
    uint? Pointer = null,
    UiShaderSourcePoolPopPair? ExpectedPair = null,
    UiShaderSourcePoolPopPair? ReplacementPair = null,
    uint? BranchTargetRawFileOffset = null);

public readonly record struct UiShaderSourcePoolPopPlan(
    UiShaderSourcePoolPopCapture Capture,
    UiShaderSourcePoolPopOutcome Outcome,
    ImmutableArray<UiShaderSourcePoolPopAction> Actions,
    UiShaderSourcePoolPopPair? ExpectedPair,
    UiShaderSourcePoolPopPair? ReplacementPair,
    UiShaderSourcePoolPopPair? CompareExchangeReturnedPair,
    uint? ReturnValue);

public readonly record struct UiShaderSourcePoolPopInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null);

/// <summary>
/// Projects the empty path or one captured cmpxchg8b attempt. It models neither
/// memory mutation nor a subsequent retry, and treats the high DWORD only as the
/// observed tag value in the compared pair.
/// </summary>
public static class UiShaderSourcePoolPopProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x12CDAE0;
    public const uint EmptyHeadTestRawFileOffset = 0x12CDAFB;
    public const uint EmptyHeadBranchRawFileOffset = 0x12CDAFD;
    public const uint CapturedLinkLoadRawFileOffset = 0x12CDAFF;
    public const uint CapturedHighIncrementRawFileOffset = 0x12CDB0F;
    public const uint CompareExchangeRawFileOffset = 0x12CDB16;
    public const uint CompareReturnedHeadCompareRawFileOffset = 0x12CDB1E;
    public const uint CompareReturnedHeadBranchRawFileOffset = 0x12CDB24;
    public const uint CompareReturnedHighRawFileOffset = 0x12CDB26;
    public const uint CompareReturnedHighBranchRawFileOffset = 0x12CDB2A;
    public const uint ReturnCapturedHeadRawFileOffset = 0x12CDB2C;
    public const uint EmptyHeadReturnRawFileOffset = 0x12CDB35;
    public const uint EntryEndRawFileOffsetExclusive = 0x12CDB3E;

    public const int PairHeadOffset = 0;
    public const int PairHighTagOffset = 4;

    public const string CapturedFunctionBytes =
        "55 8B EC 83 E4 F8 83 EC 0C 53 56 8B C1 57 89 44 24 0C " +
        "8B 48 04 8B 30 89 74 24 10 85 F6 74 36 8B 06 33 FF " +
        "8B D1 33 DB 0B FE 89 54 24 14 0B D8 41 8B C7 8B 74 " +
        "24 0C F0 0F C7 0E 8B 74 24 10 3B C7 8B 44 24 0C 75 " +
        "CC 3B 54 24 14 75 C6 8B C6 5F 5E 5B 8B E5 5D C3 " +
        "5F 5E 33 C0 5B 8B E5 5D C3";

    public static IReadOnlyList<UiShaderSourcePoolPopInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolPopInstructionEvidence(
                "Capture the receiver pair's high DWORD in ECX",
                0x12CDAF2,
                "8B 48 04"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Capture the receiver pair's head DWORD in ESI",
                0x12CDAF5,
                "8B 30"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Test the captured head",
                EmptyHeadTestRawFileOffset,
                "85 F6"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Return zero when the captured head is null",
                EmptyHeadBranchRawFileOffset,
                "74 36",
                EmptyHeadReturnRawFileOffset),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Capture the next pointer from the captured head",
                CapturedLinkLoadRawFileOffset,
                "8B 06"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Place the captured high DWORD in EDX as the expected high half",
                0x12CDB03,
                "8B D1"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Place the captured head in EDI as the expected low half",
                0x12CDB07,
                "0B FE"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Place the captured next pointer in EBX as the replacement low half",
                0x12CDB0D,
                "0B D8"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Increment the captured high DWORD in ECX for the replacement high half",
                CapturedHighIncrementRawFileOffset,
                "41"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Place the captured head in EAX as the expected low half",
                0x12CDB10,
                "8B C7"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Load the receiver pair pointer in ESI for the exchange",
                0x12CDB12,
                "8B 74 24 0C"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Compare EDX:EAX and replace with ECX:EBX in one locked 64-bit exchange",
                CompareExchangeRawFileOffset,
                "F0 0F C7 0E"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Compare the returned low DWORD against the captured head",
                CompareReturnedHeadCompareRawFileOffset,
                "3B C7"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Restore the receiver pointer to EAX without replacing the prior compare flags",
                0x12CDB20,
                "8B 44 24 0C"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Branch to recapture when the returned low DWORD differs",
                CompareReturnedHeadBranchRawFileOffset,
                "75 CC",
                0x12CDAF2),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Compare the returned high DWORD against the captured high DWORD",
                CompareReturnedHighRawFileOffset,
                "3B 54 24 14"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Branch to recapture when the returned high DWORD differs",
                CompareReturnedHighBranchRawFileOffset,
                "75 C6",
                0x12CDAF2),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Return the separately saved captured head when both halves compare equal",
                ReturnCapturedHeadRawFileOffset,
                "8B C6"),
            new UiShaderSourcePoolPopInstructionEvidence(
                "Return zero on the empty-head path",
                EmptyHeadReturnRawFileOffset,
                "5F 5E 33 C0 5B 8B E5 5D C3")
        });

    /// <summary>
    /// Uses the captured compare-exchange result registers, not the instruction's
    /// ZF, to select the observed return or stop at the retry boundary.
    /// </summary>
    public static UiShaderSourcePoolPopPlan Project(
        UiShaderSourcePoolPopCapture capture)
    {
        if (capture.CapturedHead == 0)
        {
            if (capture.CapturedNextHead.HasValue
                || capture.CompareExchangeReturnedHead.HasValue
                || capture.CompareExchangeReturnedHighTag.HasValue)
            {
                throw new ArgumentException(
                    "An empty captured head has no link-load or compare-exchange result.",
                    nameof(capture));
            }

            return new UiShaderSourcePoolPopPlan(
                capture,
                UiShaderSourcePoolPopOutcome.EmptyCapturedHead,
                ImmutableArray.Create(
                    new UiShaderSourcePoolPopAction(
                        UiShaderSourcePoolPopActionKind.ReturnZeroWithoutExchange,
                        EmptyHeadBranchRawFileOffset,
                        BranchTargetRawFileOffset: EmptyHeadReturnRawFileOffset)),
                ExpectedPair: null,
                ReplacementPair: null,
                CompareExchangeReturnedPair: null,
                ReturnValue: 0);
        }

        uint capturedNextHead = capture.CapturedNextHead
            ?? throw new ArgumentException(
                "A nonempty captured head requires its separately captured link value.",
                nameof(capture));
        uint returnedHead = capture.CompareExchangeReturnedHead
            ?? throw new ArgumentException(
                "A nonempty captured head requires the compare-exchange returned low DWORD.",
                nameof(capture));
        uint returnedHighTag = capture.CompareExchangeReturnedHighTag
            ?? throw new ArgumentException(
                "A nonempty captured head requires the compare-exchange returned high DWORD.",
                nameof(capture));

        var expectedPair = new UiShaderSourcePoolPopPair(
            capture.CapturedHead,
            capture.CapturedHighTag);
        var replacementPair = new UiShaderSourcePoolPopPair(
            capturedNextHead,
            unchecked(capture.CapturedHighTag + 1));
        var returnedPair = new UiShaderSourcePoolPopPair(
            returnedHead,
            returnedHighTag);

        bool returnedPairMatchesCapture = returnedPair == expectedPair;
        var actions = ImmutableArray.CreateBuilder<UiShaderSourcePoolPopAction>(4);
        actions.Add(new UiShaderSourcePoolPopAction(
            UiShaderSourcePoolPopActionKind.ReadCapturedHeadLink,
            CapturedLinkLoadRawFileOffset,
            capture.CapturedHead));
        actions.Add(new UiShaderSourcePoolPopAction(
            UiShaderSourcePoolPopActionKind.AttemptLockedCompareExchange,
            CompareExchangeRawFileOffset,
            capture.ReceiverPairPointer,
            expectedPair,
            replacementPair));

        if (returnedPairMatchesCapture)
        {
            actions.Add(new UiShaderSourcePoolPopAction(
                UiShaderSourcePoolPopActionKind.ReturnCapturedHead,
                ReturnCapturedHeadRawFileOffset,
                capture.CapturedHead));

            return new UiShaderSourcePoolPopPlan(
                capture,
                UiShaderSourcePoolPopOutcome.ReturnedCapturedHead,
                actions.ToImmutable(),
                expectedPair,
                replacementPair,
                returnedPair,
                capture.CapturedHead);
        }

        actions.Add(new UiShaderSourcePoolPopAction(
            UiShaderSourcePoolPopActionKind.StopAtUnmodeledRetryBoundary,
            returnedHead != capture.CapturedHead
                ? CompareReturnedHeadBranchRawFileOffset
                : CompareReturnedHighBranchRawFileOffset,
            BranchTargetRawFileOffset: 0x12CDAF2));

        return new UiShaderSourcePoolPopPlan(
            capture,
            UiShaderSourcePoolPopOutcome.RetryBoundary,
            actions.ToImmutable(),
            expectedPair,
            replacementPair,
            returnedPair,
            ReturnValue: null);
    }
}
