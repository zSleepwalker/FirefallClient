namespace Firefall.Reconstruction.Awesomium;

public readonly record struct VerboseSourceSlice(int Offset, int Length);
public readonly record struct ObservedVerboseRule(bool Matches, int Level);

/// <summary>Source slicing and observed rule selection only; not a wildcard or rule-text parser.</summary>
public static class VerboseRuleNativeLayout
{
    public const uint LookupVa = 0x102B53A0;
    public const uint ModuleSliceVa = 0x102B5210;
    public const uint WildcardMatcherVa = 0x102B52B0;
    public const uint AppendRuleVa = 0x102B5650;
    public const int RuleStride = 36;

    public static bool CaptureUsesFullSourcePath(ReadOnlySpan<byte> observedPattern) =>
        observedPattern.IndexOfAny((byte)'\\', (byte)'/') >= 0;

    public static VerboseSourceSlice CaptureModuleSlice(ReadOnlySpan<byte> observedSource)
    {
        int start = observedSource.LastIndexOfAny((byte)'\\', (byte)'/') + 1;
        ReadOnlySpan<byte> module = observedSource[start..];
        int dot = module.LastIndexOf((byte)'.');
        if (dot >= 0)
            module = module[..dot];
        if (module.EndsWith("-inl"u8))
            module = module[..^4];
        return new(start, module.Length);
    }

    public static int CaptureLevel(ReadOnlySpan<ObservedVerboseRule> orderedRules, int observedMinimumSeverity)
    {
        foreach (var rule in orderedRules)
            if (rule.Matches)
                return rule.Level;
        return unchecked(-observedMinimumSeverity);
    }
}
