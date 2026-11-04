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
    public const uint FirstCallbackListOffset = 0x6B0;
    public const uint SecondCallbackListOffset = 0x6D8;
    public const uint CallbackListSlotOffset = 0x14;
    public const uint PayloadCallbackTreeOffset = 0x5E4;
    public const uint PayloadCallbackMethodFileOffset = 0xC376A0;

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
