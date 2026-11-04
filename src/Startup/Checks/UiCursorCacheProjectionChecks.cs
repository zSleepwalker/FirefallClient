using Firefall.Reconstruction.Startup;

internal static class UiCursorCacheProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI cursor-cache evidence records the compared fields and hit/refresh sites",
            UiCursorCacheProjectionRules.CachePayloadOffset == 0x58C
            && UiCursorCacheProjectionRules.CachedArgumentByteOffset == 0x590
            && UiCursorCacheProjectionRules.CacheVersionOffset == 0x594
            && UiCursorCacheProjectionRules.CursorTreeOffset == 0x6C8
            && UiCursorCacheProjectionRules.CurrentVersionSourceOffset == 0x990
            && UiCursorCacheProjectionRules.VersionUpdateMethodFileOffset == 0xC02730
            && UiCursorCacheProjectionRules.VersionIncrementFileOffset == 0xC02816
            && UiCursorCacheProjectionRules.CacheCallerFileOffset == 0xBFF930
            && UiCursorCacheProjectionRules.CacheVersionLoadFileOffset == 0xBFF958
            && UiCursorCacheProjectionRules.CacheHitCompareFileOffset == 0xBFF95E
            && UiCursorCacheProjectionRules.CachePayloadStoreFileOffset == 0xBFFA14
            && UiCursorCacheProjectionRules.CachedArgumentByteStoreFileOffset == 0xBFFA2B
            && UiCursorCacheProjectionRules.CacheVersionStoreFileOffset == 0xBFFA37
            && UiCursorCacheProjectionRules.StateSyncCallFileOffset == 0xBFFA3F
            && UiCursorCacheProjectionRules.CacheHitReturnFileOffset == 0xBFFA45);

        var cached = new UiCursorCacheState(0x1234, 1, 7);
        var equalVersion = UiCursorCacheProjectionRules.Decide(cached, 7);
        check("UI cursor-cache equal version hits with the cached payload without consulting the argument byte",
            equalVersion == new UiCursorCacheDecision(UiCursorCacheDecisionKind.CacheHit, 0x1234)
            && cached == new UiCursorCacheState(0x1234, 1, 7));

        var differentVersion = UiCursorCacheProjectionRules.Decide(cached, 8);
        check("UI cursor-cache different version requires a lookup without projecting a payload",
            differentVersion == new UiCursorCacheDecision(UiCursorCacheDecisionKind.LookupRequired, null));

        var cleared = UiCursorCacheProjectionRules.RefreshAfterLookup(8, 0, null);
        check("UI cursor-cache lookup miss refreshes the payload to zero and stores a zero argument byte",
            cleared == new UiCursorCacheState(0, 0, 8));

        var nonzeroByte = UiCursorCacheProjectionRules.RefreshAfterLookup(9, 0xA5, 0x5678);
        check("UI cursor-cache lookup result refresh stores its payload, nonzero argument byte, and version",
            nonzeroByte == new UiCursorCacheState(0x5678, 0xA5, 9));

        var wrappedVersion = UiCursorCacheProjectionRules.Decide(
            new UiCursorCacheState(0x1234, 1, uint.MaxValue), 0);
        var maxVersion = UiCursorCacheProjectionRules.Decide(
            new UiCursorCacheState(0x1234, 1, uint.MaxValue), uint.MaxValue);
        check("UI cursor-cache compares wrapped uint versions by equality without modeling the increment",
            wrappedVersion == new UiCursorCacheDecision(UiCursorCacheDecisionKind.LookupRequired, null)
            && maxVersion == new UiCursorCacheDecision(UiCursorCacheDecisionKind.CacheHit, 0x1234));
    }
}
