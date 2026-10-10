using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedModeOneBoundaryChecks
{
    public static void Run(Action<string, bool> check)
    {
        var plan = NativeGeneratedProviderBoundaryPlanner.ForModeOne();
        check("Native mode1 boundary schedule saves six64-bit registers then loads saved RCX/RDX values from RSP+32/+24",
            plan.Opening.Length == 8 && plan.Closing.Length == 7
            && plan.Opening.Span[..6].ToArray().Select(step => step.FirstDescriptorCode)
                .SequenceEqual(new uint?[] { 53, 54, 55, 56, 59, 60 })
            && plan.Opening.Span[6] == new NativeGeneratedBoundaryStep(26, 53, 57, 32)
            && plan.Opening.Span[7] == new NativeGeneratedBoundaryStep(26, 55, 57, 24)
            && plan.Closing.Span[^1] == new NativeGeneratedBoundaryStep(33));
        var catalog = NativeGeneratedBoundaryEncodingCatalog.ModeOneDefaultHeader();
        byte[] opening = catalog.Opening.SelectMany(entry => entry.Candidates[0].ToArray()).ToArray();
        byte[] closing = catalog.Closing.SelectMany(entry => entry.Candidates[0].ToArray()).ToArray();
        check("Native mode1 boundary byte goldens preserve unusual no-index SIB E4, REX.W MOVs and plain RET",
            catalog.Opening.All(entry => entry.Candidates.Count == 1)
            && catalog.Closing.All(entry => entry.Candidates.Count == 1)
            && Convert.ToHexString(opening) == "505152535657488B44E420488B54E418"
            && Convert.ToHexString(closing) == "5F5E5B5A5958C3");
        var alternateHeader = NativeGeneratedProviderBoundaryPlanner.ForModeOne(1, 3);
        check("Native mode1 abstract plan retains header-derived register codes without claiming arbitrary-header encodings",
            alternateHeader.Opening.Span[6].FirstDescriptorCode == 54
            && alternateHeader.Opening.Span[7].FirstDescriptorCode == 56
            && (uint)NativeGeneratedRegisterCode64.Rsp == 57);
    }
}
