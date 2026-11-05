using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolPushPair(
    uint Head,
    uint HighTag);

public readonly record struct UiShaderSourcePoolPushCapture(
    uint ReceiverPairPointer,
    uint CapturedNodePointer,
    uint CapturedHighTag,
    uint CapturedHead,
    uint CapturedNodeHeadForExpectedPair,
    uint CompareExchangeReturnedHead,
    uint CompareExchangeReturnedHighTag);

public enum UiShaderSourcePoolPushOutcome
{
    ReturnedReceiver,
    RetryBoundary
}

public enum UiShaderSourcePoolPushActionKind
{
    CallNullNodeDiagnostic,
    CaptureReceiverHighTag,
    CaptureReceiverHead,
    AttemptNodeLinkStore,
    CaptureNodeHeadForExpectedPair,
    AttemptLockedCompareExchange,
    CompareReturnedHead,
    LoadReceiverForReturn,
    StopAtUnmodeledRetryBoundary,
    CompareReturnedHighTag,
    ReturnReceiver
}

public readonly record struct UiShaderSourcePoolPushAction(
    UiShaderSourcePoolPushActionKind Kind,
    uint InstructionRawFileOffset,
    uint? Pointer = null,
    uint? CapturedValue = null,
    UiShaderSourcePoolPushPair? ExpectedPair = null,
    UiShaderSourcePoolPushPair? ReplacementPair = null,
    uint? BranchTargetRawFileOffset = null);

public readonly record struct UiShaderSourcePoolPushPlan(
    UiShaderSourcePoolPushCapture Capture,
    UiShaderSourcePoolPushOutcome Outcome,
    ImmutableArray<UiShaderSourcePoolPushAction> Actions,
    UiShaderSourcePoolPushPair ExpectedPair,
    UiShaderSourcePoolPushPair ReplacementPair,
    UiShaderSourcePoolPushPair CompareExchangeReturnedPair,
    uint? ReturnValue);

public readonly record struct UiShaderSourcePoolPushInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? IndirectPointerSlotVirtualAddress = null);

/// <summary>
/// Projects the diagnostic continuation and one captured cmpxchg8b attempt.
/// A null-node capture represents the continuation after the diagnostic call
/// returned. Node stores are attempts only: this projection does not infer
/// store success or cmpxchg mutation, simulates no retry, and models no ownership
/// or termination.
/// </summary>
public static class UiShaderSourcePoolPushProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const string EntryName = "VIBE_LockFreePushStackNode";

    public const uint EntryRawFileOffset = 0x12CDA60;
    public const uint NullNodeTestRawFileOffset = 0x12CDA69;
    public const uint NullNodeBranchRawFileOffset = 0x12CDA76;
    public const uint NullNodeDiagnosticCallRawFileOffset = 0x12CDA89;
    public const uint RetryCaptureRawFileOffset = 0x12CDA96;
    public const uint CapturedHighTagLoadRawFileOffset = 0x12CDA9B;
    public const uint CapturedHeadLoadRawFileOffset = 0x12CDAA0;
    public const uint NodeLinkStoreAttemptRawFileOffset = 0x12CDAAA;
    public const uint CapturedNodeHeadLoadRawFileOffset = 0x12CDAAD;
    public const uint ReplacementTagIncrementRawFileOffset = 0x12CDAAC;
    public const uint CompareExchangeRawFileOffset = 0x12CDAB9;
    public const uint CompareReturnedHeadRawFileOffset = 0x12CDAC1;
    public const uint LowMismatchBranchRawFileOffset = 0x12CDAC7;
    public const uint CompareReturnedHighTagRawFileOffset = 0x12CDAC9;
    public const uint HighMismatchBranchRawFileOffset = 0x12CDACB;
    public const uint LoadReceiverForReturnRawFileOffset = 0x12CDAC3;
    public const uint ReturnRawFileOffset = 0x12CDAD3;
    public const uint EntryEndRawFileOffsetExclusive = 0x12CDAD6;

    public const uint NullNodeDiagnosticPointerSlotVirtualAddress = 0x1C15EA0;

    public const int PairHeadOffset = 0;
    public const int PairHighTagOffset = 4;

    public const string CapturedFunctionBytes =
        "55 8B EC 83 E4 F8 83 EC 0C 83 7D 08 00 8B C1 53 " +
        "56 57 89 44 24 10 75 1E 6A 00 68 B6 01 00 00 68 " +
        "A7 41 B7 01 68 6B 41 B7 01 FF 15 A0 5E C1 01 8B " +
        "44 24 20 83 C4 10 8B 55 08 33 FF 8B 48 04 33 DB " +
        "8B 00 8B F1 0B DA 89 74 24 14 89 02 41 8B 02 0B " +
        "F8 8B C7 8B D6 8B 74 24 10 F0 0F C7 0E 8B 74 24 " +
        "14 3B C7 8B 44 24 10 75 CD 3B D6 75 C9 5F 5E 5B " +
        "8B E5 5D C2 04 00";

    public static IReadOnlyList<UiShaderSourcePoolPushInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolPushInstructionEvidence(
                "Save the frame pointer",
                0x12CDA60,
                "55"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Establish the frame",
                0x12CDA61,
                "8B EC"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Align the stack",
                0x12CDA63,
                "83 E4 F8"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Reserve stack space",
                0x12CDA66,
                "83 EC 0C"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Test the node argument for null",
                NullNodeTestRawFileOffset,
                "83 7D 08 00"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Keep the receiver pair pointer in EAX",
                0x12CDA6D,
                "8B C1"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Save EBX",
                0x12CDA6F,
                "53"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Save ESI",
                0x12CDA70,
                "56"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Save EDI",
                0x12CDA71,
                "57"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Save the receiver pair pointer",
                0x12CDA72,
                "89 44 24 10"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Skip the diagnostic when the node argument is nonnull",
                NullNodeBranchRawFileOffset,
                "75 1E",
                RetryCaptureRawFileOffset),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Push a zero diagnostic argument",
                0x12CDA78,
                "6A 00"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Push the immediate diagnostic argument 0x1B6",
                0x12CDA7A,
                "68 B6 01 00 00"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Push the immediate address argument 0x1B741A7",
                0x12CDA7F,
                "68 A7 41 B7 01"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Push the immediate address argument 0x1B7416B",
                0x12CDA84,
                "68 6B 41 B7 01"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Call indirectly through the diagnostic pointer slot",
                NullNodeDiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                IndirectPointerSlotVirtualAddress: NullNodeDiagnosticPointerSlotVirtualAddress),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Restore the receiver pair pointer after the diagnostic call",
                0x12CDA8F,
                "8B 44 24 20"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Remove the diagnostic arguments",
                0x12CDA93,
                "83 C4 10"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Capture the node argument for this attempt",
                RetryCaptureRawFileOffset,
                "8B 55 08"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Zero the expected-low and replacement-high staging registers",
                0x12CDA99,
                "33 FF"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Sample the receiver pair high DWORD at offset 4",
                CapturedHighTagLoadRawFileOffset,
                "8B 48 04"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Zero the replacement-low staging register",
                0x12CDA9E,
                "33 DB"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Sample the receiver pair head DWORD at offset 0",
                CapturedHeadLoadRawFileOffset,
                "8B 00"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Keep the separately sampled high DWORD",
                0x12CDAA2,
                "8B F1"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Stage the node pointer as the replacement low DWORD",
                0x12CDAA4,
                "0B DA"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Save the captured high DWORD",
                0x12CDAA6,
                "89 74 24 14"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Attempt to store the captured head through the node pointer",
                NodeLinkStoreAttemptRawFileOffset,
                "89 02"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Increment the captured high DWORD for the replacement tag",
                ReplacementTagIncrementRawFileOffset,
                "41"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Load the node's current DWORD as the expected low half",
                CapturedNodeHeadLoadRawFileOffset,
                "8B 02"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Stage the separately loaded node DWORD as the expected low half",
                0x12CDAAF,
                "0B F8"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Stage the expected low half in EAX",
                0x12CDAB1,
                "8B C7"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Stage the captured high DWORD as the expected high half",
                0x12CDAB3,
                "8B D6"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Load the receiver pair pointer for the exchange",
                0x12CDAB5,
                "8B 74 24 10"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Attempt locked cmpxchg8b with EDX:EAX expected and ECX:EBX desired",
                CompareExchangeRawFileOffset,
                "F0 0F C7 0E"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Restore the captured high DWORD for the result comparison",
                0x12CDABD,
                "8B 74 24 14"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Compare the returned low DWORD against the observed node DWORD",
                CompareReturnedHeadRawFileOffset,
                "3B C7"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Load the receiver pointer into EAX without changing the compare flags",
                LoadReceiverForReturnRawFileOffset,
                "8B 44 24 10"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Retry from the separate captures if the returned low DWORD differs",
                LowMismatchBranchRawFileOffset,
                "75 CD",
                RetryCaptureRawFileOffset),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Compare the returned high DWORD against the captured high DWORD",
                CompareReturnedHighTagRawFileOffset,
                "3B D6"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Retry from the separate captures if the returned high DWORD differs",
                HighMismatchBranchRawFileOffset,
                "75 C9",
                RetryCaptureRawFileOffset),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Restore EDI",
                0x12CDACD,
                "5F"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Restore ESI",
                0x12CDACE,
                "5E"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Restore EBX",
                0x12CDACF,
                "5B"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Restore the stack pointer",
                0x12CDAD0,
                "8B E5"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Restore the frame pointer",
                0x12CDAD2,
                "5D"),
            new UiShaderSourcePoolPushInstructionEvidence(
                "Return EAX and pop the four-byte stack argument",
                ReturnRawFileOffset,
                "C2 04 00")
        });

    /// <summary>
    /// Uses the captured cmpxchg8b result halves, not ZF, to select the return
    /// or stop at the retry branch. A node-zero capture includes the diagnostic
    /// path and subsequent instruction captures only if that call returned.
    /// </summary>
    public static UiShaderSourcePoolPushPlan Project(
        UiShaderSourcePoolPushCapture capture)
    {
        var expectedPair = new UiShaderSourcePoolPushPair(
            capture.CapturedNodeHeadForExpectedPair,
            capture.CapturedHighTag);
        var replacementPair = new UiShaderSourcePoolPushPair(
            capture.CapturedNodePointer,
            unchecked(capture.CapturedHighTag + 1));
        var returnedPair = new UiShaderSourcePoolPushPair(
            capture.CompareExchangeReturnedHead,
            capture.CompareExchangeReturnedHighTag);

        var actions = ImmutableArray.CreateBuilder<UiShaderSourcePoolPushAction>(9);
        if (capture.CapturedNodePointer == 0)
        {
            actions.Add(new UiShaderSourcePoolPushAction(
                UiShaderSourcePoolPushActionKind.CallNullNodeDiagnostic,
                NullNodeDiagnosticCallRawFileOffset,
                Pointer: NullNodeDiagnosticPointerSlotVirtualAddress));
        }

        actions.Add(new UiShaderSourcePoolPushAction(
            UiShaderSourcePoolPushActionKind.CaptureReceiverHighTag,
            CapturedHighTagLoadRawFileOffset,
            unchecked(capture.ReceiverPairPointer + (uint)PairHighTagOffset),
            capture.CapturedHighTag));
        actions.Add(new UiShaderSourcePoolPushAction(
            UiShaderSourcePoolPushActionKind.CaptureReceiverHead,
            CapturedHeadLoadRawFileOffset,
            capture.ReceiverPairPointer,
            capture.CapturedHead));
        actions.Add(new UiShaderSourcePoolPushAction(
            UiShaderSourcePoolPushActionKind.AttemptNodeLinkStore,
            NodeLinkStoreAttemptRawFileOffset,
            capture.CapturedNodePointer,
            capture.CapturedHead));
        actions.Add(new UiShaderSourcePoolPushAction(
            UiShaderSourcePoolPushActionKind.CaptureNodeHeadForExpectedPair,
            CapturedNodeHeadLoadRawFileOffset,
            capture.CapturedNodePointer,
            capture.CapturedNodeHeadForExpectedPair));
        actions.Add(new UiShaderSourcePoolPushAction(
            UiShaderSourcePoolPushActionKind.AttemptLockedCompareExchange,
            CompareExchangeRawFileOffset,
            capture.ReceiverPairPointer,
            ExpectedPair: expectedPair,
            ReplacementPair: replacementPair));
        actions.Add(new UiShaderSourcePoolPushAction(
            UiShaderSourcePoolPushActionKind.CompareReturnedHead,
            CompareReturnedHeadRawFileOffset,
            CapturedValue: capture.CompareExchangeReturnedHead));
        actions.Add(new UiShaderSourcePoolPushAction(
            UiShaderSourcePoolPushActionKind.LoadReceiverForReturn,
            LoadReceiverForReturnRawFileOffset,
            CapturedValue: capture.ReceiverPairPointer));

        if (capture.CompareExchangeReturnedHead != expectedPair.Head)
        {
            actions.Add(new UiShaderSourcePoolPushAction(
                UiShaderSourcePoolPushActionKind.StopAtUnmodeledRetryBoundary,
                LowMismatchBranchRawFileOffset,
                BranchTargetRawFileOffset: RetryCaptureRawFileOffset));

            return new UiShaderSourcePoolPushPlan(
                capture,
                UiShaderSourcePoolPushOutcome.RetryBoundary,
                actions.ToImmutable(),
                expectedPair,
                replacementPair,
                returnedPair,
                ReturnValue: null);
        }

        actions.Add(new UiShaderSourcePoolPushAction(
            UiShaderSourcePoolPushActionKind.CompareReturnedHighTag,
            CompareReturnedHighTagRawFileOffset,
            CapturedValue: capture.CompareExchangeReturnedHighTag));

        if (capture.CompareExchangeReturnedHighTag != expectedPair.HighTag)
        {
            actions.Add(new UiShaderSourcePoolPushAction(
                UiShaderSourcePoolPushActionKind.StopAtUnmodeledRetryBoundary,
                HighMismatchBranchRawFileOffset,
                BranchTargetRawFileOffset: RetryCaptureRawFileOffset));

            return new UiShaderSourcePoolPushPlan(
                capture,
                UiShaderSourcePoolPushOutcome.RetryBoundary,
                actions.ToImmutable(),
                expectedPair,
                replacementPair,
                returnedPair,
                ReturnValue: null);
        }

        actions.Add(new UiShaderSourcePoolPushAction(
            UiShaderSourcePoolPushActionKind.ReturnReceiver,
            ReturnRawFileOffset,
            CapturedValue: capture.ReceiverPairPointer));

        return new UiShaderSourcePoolPushPlan(
            capture,
            UiShaderSourcePoolPushOutcome.ReturnedReceiver,
            actions.ToImmutable(),
            expectedPair,
            replacementPair,
            returnedPair,
            capture.ReceiverPairPointer);
    }
}
