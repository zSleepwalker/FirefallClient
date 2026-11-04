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

public static class UiCursorCacheProjectionRules
{
    public const uint CachePayloadOffset = 0x58C;
    public const uint CachedArgumentByteOffset = 0x590;
    public const uint CacheVersionOffset = 0x594;
    public const uint CursorTreeOffset = 0x6C8;
    public const uint CurrentVersionSourceOffset = 0x990;
    public const uint VersionUpdateMethodFileOffset = 0xC02730;
    public const uint VersionIncrementFileOffset = 0xC02816;
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

    public static UiCursorCacheState RefreshAfterLookup(
        uint requestedVersion,
        byte requestedArgumentByte,
        uint? lookupPayload) =>
        new(lookupPayload ?? 0, requestedArgumentByte, requestedVersion);
}
