namespace Firefall.Reconstruction.Startup;

public enum NativeGeneratedModeOneMemoryShapeIssue
{
    None,
    UnknownDescriptorCode,
    NotAnAddressRegister,
    UnsupportedAddressWidth,
    StackPointerIndex,
    MixedAddressWidths,
    InvalidScale,
    ScaledWithoutIndex
}

public readonly record struct NativeGeneratedModeOneMemoryShapeResult(
    NativeGeneratedModeOneMemoryShapeIssue Issue,
    bool NeedsAddressSizeOverride = false,
    bool UsesNativeNoSibFastPath = false)
{
    public bool IsAccepted => Issue == NativeGeneratedModeOneMemoryShapeIssue.None;
    public uint? NativeDiagnosticCode => Issue switch
    {
        NativeGeneratedModeOneMemoryShapeIssue.None => null,
        NativeGeneratedModeOneMemoryShapeIssue.UnknownDescriptorCode => 1040197660,
        NativeGeneratedModeOneMemoryShapeIssue.StackPointerIndex => 1041234517,
        NativeGeneratedModeOneMemoryShapeIssue.InvalidScale => 1040187409,
        NativeGeneratedModeOneMemoryShapeIssue.ScaledWithoutIndex => 0x3E183111,
        _ => 1040484283
    };
}

/// <summary>Mode1 address-shape validation and native fast-path predicate, not canonical x64 encoding or execution.</summary>
public static class NativeGeneratedModeOneMemoryShape
{
    public const uint ValidatorFileOffset = 0x129A6E0;
    public const uint BaseRegisterValidatorFileOffset = 0x129A960;
    public const uint IndexRegisterValidatorFileOffset = 0x129A8F0;
    public const uint NoSibFastPathPredicateFileOffset = 0x129A7F0;

    public static NativeGeneratedModeOneMemoryShapeResult Project(
        int baseDescriptorCode, int indexDescriptorCode, byte scale)
    {
        var baseResult = NativeGeneratedDescriptorFactories.ProjectCode(baseDescriptorCode);
        var indexResult = NativeGeneratedDescriptorFactories.ProjectCode(indexDescriptorCode);
        if (baseResult.Fields is not { } baseFields || indexResult.Fields is not { } indexFields)
            return new(NativeGeneratedModeOneMemoryShapeIssue.UnknownDescriptorCode);
        var issue = RegisterIssue(baseFields);
        if (issue != NativeGeneratedModeOneMemoryShapeIssue.None)
            return new(issue);
        issue = RegisterIssue(indexFields);
        if (issue != NativeGeneratedModeOneMemoryShapeIssue.None)
            return new(issue);
        if (indexFields.Kind != 0 && indexFields.Index == 4)
            return new(NativeGeneratedModeOneMemoryShapeIssue.StackPointerIndex);
        if (baseFields.Kind != 0 && indexFields.Kind != 0 && baseFields.Width != indexFields.Width)
            return new(NativeGeneratedModeOneMemoryShapeIssue.MixedAddressWidths);
        if (scale is not (1 or 2 or 4 or 8))
            return new(NativeGeneratedModeOneMemoryShapeIssue.InvalidScale);
        if (scale != 1 && indexFields.Kind == 0)
            return new(NativeGeneratedModeOneMemoryShapeIssue.ScaledWithoutIndex);
        return new(NativeGeneratedModeOneMemoryShapeIssue.None,
            baseFields.Width == 32 || indexFields.Width == 32,
            scale == 1 && indexFields.Kind == 0 && baseFields.Index != 4);
    }

    private static NativeGeneratedModeOneMemoryShapeIssue RegisterIssue(NativeGeneratedCodeFields fields)
    {
        if (fields.Kind == 0)
            return NativeGeneratedModeOneMemoryShapeIssue.None;
        if (fields.Kind != 4)
            return NativeGeneratedModeOneMemoryShapeIssue.NotAnAddressRegister;
        return fields.Width is 32 or 64
            ? NativeGeneratedModeOneMemoryShapeIssue.None
            : NativeGeneratedModeOneMemoryShapeIssue.UnsupportedAddressWidth;
    }
}
