namespace Firefall.Reconstruction.Startup;

public readonly record struct UiOldPairCallbackGateState(
    uint Word988,
    uint Word98C,
    uint Field20);

public readonly record struct UiOldPairCallbackGateProjection(
    uint QuotientLow32,
    uint Elapsed,
    bool ShouldInvokeCallbacks,
    uint ResultField20);

public readonly record struct UiOldPairCallbackTraversalEvidence(
    uint TreeOffset,
    uint DispatchSequence,
    bool IsInOrderTraversal,
    uint DispatchCodeStartFileOffset,
    uint DispatchCodeEndFileOffsetInclusive,
    uint CallbackReceiverOffset,
    uint CallbackSlotByteOffset,
    uint CallbackArgumentCount,
    bool ChecksNodeByteAfterCallback,
    uint NodeByteOffset,
    bool ReadsSuccessorLinksAfterCallback,
    uint FirstSuccessorLinkOffset,
    uint SecondSuccessorLinkOffset,
    uint ThirdSuccessorLinkOffset,
    bool RereadsHeadAndComparesAfterSuccessorWalk,
    bool SuccessorCapturedBeforeCallback);

public readonly record struct UiOldPairDirectCallbackTraversalEvidence(
    uint TreeOffset,
    uint DispatchSequence,
    uint DispatchCodeStartFileOffset,
    uint DispatchCodeEndFileOffsetInclusive,
    uint CallbackReceiverOffset,
    uint CallbackMethodFileOffset);

public enum UiOldPairNestedCallbackContainerKind
{
    Tree,
    Container
}

public readonly record struct UiOldPairNestedCallbackTraversalEvidence(
    UiOldPairNestedCallbackContainerKind ContainerKind,
    uint ContainerOffset,
    uint ReceiverOffset,
    uint CallbackSlotByteOffset);

public static class UiOldPairCallbackGateProjectionRules
{
    public const uint OldPairLowWordOffset = 0x988;
    public const uint OldPairHighWordOffset = 0x98C;
    public const uint Field20Offset = 0x20;
    public const uint Divisor = 1000;
    public const uint MinimumElapsed = 5000;

    public const uint CallerAfterReadyCheckFileOffset = 0xC02778;
    public const uint CallbackGateFileOffset = 0xC06670;
    public const uint DivisionCallFileOffset = 0xC06686;
    public const uint UnsignedDivideHelperFileOffset = 0x159DD10;
    public const uint FirstStoreAfterCallbackGateFileOffset = 0xC0281C;
    public const uint SecondStoreAfterCallbackGateFileOffset = 0xC02825;
    public const uint Field20StoreBeforeCallbackWalksFileOffset = 0xC0669C;
    public const uint FirstCallbackListOffset = 0x6B0;
    public const uint SecondCallbackListOffset = 0x6D8;
    public const uint CallbackListSlotOffset = 0x14;
    public const uint PayloadCallbackTreeOffset = 0x5E4;
    public const uint PayloadCallbackMethodFileOffset = 0xC376A0;
    public const uint CallbackNodeByteOffset = 0x0D;

    public static IReadOnlyList<UiOldPairCallbackTraversalEvidence> PrimaryCallbackTraversals { get; } =
        Array.AsReadOnly(
        [
            new UiOldPairCallbackTraversalEvidence(
                FirstCallbackListOffset,
                0,
                true,
                0xC066B0,
                0xC066B5,
                0x14,
                CallbackListSlotOffset,
                0,
                true,
                CallbackNodeByteOffset,
                true,
                0x08,
                0x00,
                0x04,
                true,
                false),
            new UiOldPairCallbackTraversalEvidence(
                SecondCallbackListOffset,
                1,
                true,
                0xC06710,
                0xC06715,
                0x14,
                CallbackListSlotOffset,
                0,
                true,
                CallbackNodeByteOffset,
                true,
                0x08,
                0x00,
                0x04,
                true,
                false)
        ]);

    public static IReadOnlyList<UiOldPairDirectCallbackTraversalEvidence> DirectCallbackTraversals { get; } =
        Array.AsReadOnly(
        [
            new UiOldPairDirectCallbackTraversalEvidence(
                PayloadCallbackTreeOffset,
                2,
                0xC06770,
                0xC06773,
                0x14,
                PayloadCallbackMethodFileOffset)
        ]);

    public static IReadOnlyList<UiOldPairNestedCallbackTraversalEvidence> NestedCallbackTraversals { get; } =
        Array.AsReadOnly(
        [
            new UiOldPairNestedCallbackTraversalEvidence(
                UiOldPairNestedCallbackContainerKind.Tree,
                0x0E8,
                0x14,
                0x14),
            new UiOldPairNestedCallbackTraversalEvidence(
                UiOldPairNestedCallbackContainerKind.Tree,
                0x118,
                0x14,
                0x14),
            new UiOldPairNestedCallbackTraversalEvidence(
                UiOldPairNestedCallbackContainerKind.Container,
                0x120,
                0x08,
                0x14)
        ]);

    public static UiOldPairCallbackGateProjection Project(UiOldPairCallbackGateState capturedState)
    {
        ulong oldPair = ((ulong)capturedState.Word98C << 32) | capturedState.Word988;
        uint quotientLow32 = unchecked((uint)(oldPair / Divisor));
        uint elapsed = unchecked(quotientLow32 - capturedState.Field20);
        bool shouldInvokeCallbacks = elapsed >= MinimumElapsed;

        return new UiOldPairCallbackGateProjection(
            quotientLow32,
            elapsed,
            shouldInvokeCallbacks,
            shouldInvokeCallbacks ? quotientLow32 : capturedState.Field20);
    }
}
