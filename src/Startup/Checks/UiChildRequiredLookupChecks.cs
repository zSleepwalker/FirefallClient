using Firefall.Reconstruction.Startup;

internal static class UiChildRequiredLookupChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI child required lookups record both raw helper and dependency virtual-slot anchors",
            UiChildRequiredLookupProjectionRules.FirstLookupHelperRawFileOffset == 0xCA7090
            && UiChildRequiredLookupProjectionRules.FirstLookupDependencyVirtualSlotOffset == 0x16C
            && UiChildRequiredLookupProjectionRules.SecondLookupHelperRawFileOffset == 0xCA71F0
            && UiChildRequiredLookupProjectionRules.SecondLookupDependencyVirtualSlotOffset == 0x1A8);

        foreach (int status in new[] { int.MinValue, -1, 0, 1, int.MaxValue })
        foreach (uint outputPointer in new[] { 0u, 1u, 0x80000000u, 0xFFFFFFFFu })
        {
            bool expected = status >= 0 && outputPointer != 0;
            check($"UI child required lookup status={status}, captured output={outputPointer:X8}",
                UiChildRequiredLookupProjectionRules.IsRequiredLookupSatisfied(
                    status, outputPointer) == expected);
        }
    }
}
