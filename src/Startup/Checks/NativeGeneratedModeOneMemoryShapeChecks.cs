using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedModeOneMemoryShapeChecks
{
    public static void Run(Action<string, bool> check)
    {
        int[] codes = [0, .. Enumerable.Range(37, 32)];
        int accepted = 0;
        bool shapesMatch = true;
        foreach (int baseCode in codes)
        foreach (int indexCode in codes)
        for (byte scale = 1; scale <= 8; scale++)
        {
            bool base32 = baseCode is >= 37 and <= 52;
            bool index32 = indexCode is >= 37 and <= 52;
            bool stackIndex = indexCode is 41 or 57;
            bool mixed = baseCode != 0 && indexCode != 0 && base32 != index32;
            bool allowedScale = scale is 1 or 2 or 4 or 8;
            bool expected = !stackIndex && !mixed && allowedScale && (scale == 1 || indexCode != 0);
            var result = NativeGeneratedModeOneMemoryShape.Project(baseCode, indexCode, scale);
            shapesMatch &= result.IsAccepted == expected;
            if (!expected)
                continue;
            accepted++;
            int baseIndex = baseCode == 0 ? 0 : baseCode - (base32 ? 37 : 53);
            shapesMatch &= result.NeedsAddressSizeOverride == (base32 || index32)
                && result.UsesNativeNoSibFastPath == (scale == 1 && indexCode == 0 && baseIndex != 4)
                && result.NativeDiagnosticCode is null;
        }
        check("Mode1 address validation covers8712 base/index/scale shapes,2073 accepted combinations and32-bit address override selection",
            shapesMatch && accepted == 2073);
        var esp = NativeGeneratedModeOneMemoryShape.Project(53, 41, 1);
        var r12 = NativeGeneratedModeOneMemoryShape.Project(53, 65, 8);
        var missingIndex = NativeGeneratedModeOneMemoryShape.Project(53, 0, 2);
        check("Mode1 rejects ESP/RSP indexes, permits extended index12 and reports scaled-without-index separately",
            esp.Issue == NativeGeneratedModeOneMemoryShapeIssue.StackPointerIndex && esp.NativeDiagnosticCode == 1041234517
            && r12.IsAccepted && !r12.NeedsAddressSizeOverride
            && missingIndex.Issue == NativeGeneratedModeOneMemoryShapeIssue.ScaledWithoutIndex
            && missingIndex.NativeDiagnosticCode == 0x3E183111);
        check("Mode1 invalid kind, width, mixed widths, unknown code and scale retain explicit native diagnostics",
            NativeGeneratedModeOneMemoryShape.Project(69, 0, 1).Issue == NativeGeneratedModeOneMemoryShapeIssue.NotAnAddressRegister
            && NativeGeneratedModeOneMemoryShape.Project(21, 0, 1).Issue == NativeGeneratedModeOneMemoryShapeIssue.UnsupportedAddressWidth
            && NativeGeneratedModeOneMemoryShape.Project(37, 53, 1).Issue == NativeGeneratedModeOneMemoryShapeIssue.MixedAddressWidths
            && NativeGeneratedModeOneMemoryShape.Project(139, 0, 1).NativeDiagnosticCode == 1040197660
            && NativeGeneratedModeOneMemoryShape.Project(53, 0, 0).NativeDiagnosticCode == 1040187409);
        check("Mode1 fast-path predicate compares full base index with4, not index modulo8; no canonical R12/R13 addressing is invented",
            !NativeGeneratedModeOneMemoryShape.Project(57, 0, 1).UsesNativeNoSibFastPath
            && NativeGeneratedModeOneMemoryShape.Project(65, 0, 1).UsesNativeNoSibFastPath
            && NativeGeneratedModeOneMemoryShape.Project(66, 0, 1).UsesNativeNoSibFastPath
            && NativeGeneratedModeOneMemoryShape.Project(0, 0, 1).UsesNativeNoSibFastPath);
    }
}
