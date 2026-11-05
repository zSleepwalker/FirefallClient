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
            && UiCursorCacheProjectionRules.InvalidationReadyByteOffset == 0x672
            && UiCursorCacheProjectionRules.InvalidationWord988Offset == 0x988
            && UiCursorCacheProjectionRules.InvalidationWord98COffset == 0x98C
            && UiCursorCacheProjectionRules.InvalidationComponentOffset == 0x974
            && UiCursorCacheProjectionRules.InvalidationComponentSlot40Offset == 0x40
            && UiCursorCacheProjectionRules.InvalidationComponentSlot44Offset == 0x44
            && UiCursorCacheProjectionRules.VersionUpdateVtableSlotRawOffset == 0x1872AFC
            && UiCursorCacheProjectionRules.VersionUpdateVtableSlotValue == 0x01003330
            && UiCursorCacheProjectionRules.VersionUpdateMethodFileOffset == 0xC02730
            && UiCursorCacheProjectionRules.VersionUpdateGlobalCounterIncrementFileOffset == 0xC02765
            && UiCursorCacheProjectionRules.InvalidationReadyCheckFileOffset == 0xC0276B
            && UiCursorCacheProjectionRules.InvalidationNotReadyEpilogueFileOffset == 0xC041EC
            && UiCursorCacheProjectionRules.InvalidationCallbackFileOffset == 0xC06670
            && UiCursorCacheProjectionRules.VersionIncrementFileOffset == 0xC02816
            && UiCursorCacheProjectionRules.InvalidationWord988StoreFileOffset == 0xC0281C
            && UiCursorCacheProjectionRules.InvalidationWord98CStoreFileOffset == 0xC02825
            && UiCursorCacheProjectionRules.InvalidationComponentSlot40CallFileOffset == 0xC02831
            && UiCursorCacheProjectionRules.InvalidationComponentSlot44CallFileOffset == 0xC0284E
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
        check("UI cursor-cache hit state payload remains distinct from the native current-version EAX value",
            equalVersion.Payload == cached.Payload
            && cached.Version == 7
            && equalVersion.Payload != cached.Version);

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

        var capturedInvalidationState = new UiCursorInvalidationState(41, 0x10203040, 0x50607080);
        bool allReadyByteValuesProjectTheObservedGate = Enumerable.Range(0, 256).All(value =>
        {
            var decision = UiCursorCacheProjectionRules.ProjectInvalidation(
                capturedInvalidationState,
                (byte)value,
                0xFFFFFFFF,
                0x80000001);
            return value == 0
                ? decision == new UiCursorInvalidationDecision(
                    UiCursorInvalidationDecisionKind.SkippedNotReady,
                    capturedInvalidationState)
                : decision == new UiCursorInvalidationDecision(
                    UiCursorInvalidationDecisionKind.Updated,
                    new UiCursorInvalidationState(42, 0xFFFFFFFF, 0x80000001));
        });
        check("UI invalidation treats only ready byte zero as skipped across all byte values",
            allReadyByteValuesProjectTheObservedGate);

        var notReady = UiCursorCacheProjectionRules.ProjectInvalidation(
            capturedInvalidationState, 0, 0xFFFFFFFF, 0x80000001);
        check("UI invalidation skipped path preserves all captured fields",
            notReady.Kind == UiCursorInvalidationDecisionKind.SkippedNotReady
            && notReady.State == capturedInvalidationState);

        var ready = UiCursorCacheProjectionRules.ProjectInvalidation(
            capturedInvalidationState, 1, 0xFFFFFFFF, 0x80000001);
        check("UI invalidation enabled path increments once and stores both full-width arguments in order",
            ready == new UiCursorInvalidationDecision(
                UiCursorInvalidationDecisionKind.Updated,
                new UiCursorInvalidationState(42, 0xFFFFFFFF, 0x80000001)));

        var wrappedInvalidation = UiCursorCacheProjectionRules.ProjectInvalidation(
            new UiCursorInvalidationState(uint.MaxValue, 0xAAAAAAAA, 0xBBBBBBBB),
            0xFF,
            0x12345678,
            0xFEDCBA98);
        check("UI invalidation version increment wraps uint.MaxValue to zero",
            wrappedInvalidation == new UiCursorInvalidationDecision(
                UiCursorInvalidationDecisionKind.Updated,
                new UiCursorInvalidationState(0, 0x12345678, 0xFEDCBA98)));
    }
}
