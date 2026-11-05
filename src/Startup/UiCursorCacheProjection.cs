namespace Firefall.Reconstruction.Startup;

public enum UiCursorCacheDecisionKind
{
    CacheHit,
    LookupRequired
}

public readonly record struct UiCursorCacheState(
    uint Payload,
    byte CachedArgumentByte,
    uint Version);

public readonly record struct UiCursorCacheDecision(
    UiCursorCacheDecisionKind Kind,
    uint? Payload);

public enum UiCursorInvalidationDecisionKind
{
    SkippedNotReady,
    Updated
}

public readonly record struct UiCursorInvalidationState(
    uint Version,
    uint Word988,
    uint Word98C);

public readonly record struct UiCursorInvalidationDecision(
    UiCursorInvalidationDecisionKind Kind,
    UiCursorInvalidationState State);

public static class UiCursorCacheProjectionRules
{
    public const uint CachePayloadOffset = 0x58C;
    public const uint CachedArgumentByteOffset = 0x590;
    public const uint CacheVersionOffset = 0x594;
    public const uint CursorTreeOffset = 0x6C8;
    public const uint CurrentVersionSourceOffset = 0x990;
    public const uint InvalidationReadyByteOffset = 0x672;
    public const uint InvalidationWord988Offset = 0x988;
    public const uint InvalidationWord98COffset = 0x98C;
    public const uint InvalidationComponentOffset = 0x974;
    public const uint InvalidationComponentSlot40Offset = 0x40;
    public const uint InvalidationComponentSlot44Offset = 0x44;
    public const uint VersionUpdateVtableSlotRawOffset = 0x1872AFC;
    public const uint VersionUpdateVtableSlotValue = 0x01003330;
    public const uint VersionUpdateMethodFileOffset = 0xC02730;
    public const uint VersionUpdateGlobalCounterIncrementFileOffset = 0xC02765;
    public const uint InvalidationReadyCheckFileOffset = 0xC0276B;
    public const uint InvalidationNotReadyEpilogueFileOffset = 0xC041EC;
    public const uint InvalidationCallbackFileOffset = 0xC06670;
    public const uint VersionIncrementFileOffset = 0xC02816;
    public const uint InvalidationWord988StoreFileOffset = 0xC0281C;
    public const uint InvalidationWord98CStoreFileOffset = 0xC02825;
    public const uint InvalidationComponentSlot40CallFileOffset = 0xC02831;
    public const uint InvalidationComponentSlot44CallFileOffset = 0xC0284E;
    public const uint CacheCallerFileOffset = 0xBFF930;
    public const uint CacheVersionLoadFileOffset = 0xBFF958;
    public const uint CacheHitCompareFileOffset = 0xBFF95E;
    public const uint CachePayloadStoreFileOffset = 0xBFFA14;
    public const uint CachedArgumentByteStoreFileOffset = 0xBFFA2B;
    public const uint CacheVersionStoreFileOffset = 0xBFFA37;
    public const uint StateSyncCallFileOffset = 0xBFFA3F;
    public const uint CacheHitReturnFileOffset = 0xBFFA45;

    public static UiCursorCacheDecision Decide(UiCursorCacheState cache, uint requestedVersion) =>
        cache.Version == requestedVersion
            ? new(UiCursorCacheDecisionKind.CacheHit, cache.Payload)
            : new(UiCursorCacheDecisionKind.LookupRequired, null);

    public static UiCursorInvalidationDecision ProjectInvalidation(
        UiCursorInvalidationState capturedState,
        byte readyByte,
        uint argumentOne,
        uint argumentTwo) =>
        readyByte == 0
            ? new(UiCursorInvalidationDecisionKind.SkippedNotReady, capturedState)
            : new(
                UiCursorInvalidationDecisionKind.Updated,
                new UiCursorInvalidationState(
                    unchecked(capturedState.Version + 1),
                    argumentOne,
                    argumentTwo));

    public static UiCursorCacheState RefreshAfterLookup(
        uint requestedVersion,
        byte requestedArgumentByte,
        uint? lookupPayload) =>
        new(lookupPayload ?? 0, requestedArgumentByte, requestedVersion);
}
