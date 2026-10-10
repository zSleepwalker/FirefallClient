using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedModeOneMemoryEncodingChecks
{
    public static void Run(Action<string, bool> check)
    {
        bool Matches(int baseCode, int indexCode, byte scale, int displacement, byte modRm,
            string bytes, byte rex = 0, bool addressOverride = false, bool alreadyPresent = false)
        {
            var projection = NativeGeneratedModeOneMemoryEncoding.Project(
                baseCode, indexCode, scale, displacement, modRm, 0xFF, alreadyPresent);
            return projection.Shape.IsAccepted && projection.Fields is { } fields
                && Convert.ToHexString(fields.ToAddressBytes()) == bytes
                && fields.RexAddressBitsToOr == rex && fields.EmitAddressSizeOverride == addressOverride;
        }
        check("Mode1 memory projection reproduces default-header stack-load address bytes with native E4 SIB",
            Matches(57, 0, 1, 32, 0, "44E420")
            && Matches(57, 0, 1, 24, 0x10, "54E418"));
        check("Mode1 memory projection preserves signed displacement boundaries and mandatory zero byte for RBP",
            Matches(53, 0, 1, -128, 0xFF, "7880")
            && Matches(53, 0, 1, 127, 0xFF, "787F")
            && Matches(53, 0, 1, -129, 0xFF, "B87FFFFFFF")
            && Matches(53, 0, 1, 128, 0xFF, "B880000000")
            && Matches(58, 0, 1, 0, 0, "4500"));
        check("Mode1 absent base requires full displacement and indexed forms preserve scale/index/base fields",
            Matches(0, 0, 1, 0x12345678, 0x18, "1D78563412")
            && Matches(0, 53, 4, unchecked((int)0xDEADBEEF), 0, "0485EFBEADDE")
            && Matches(60, 59, 8, 0, 0x18, "1CF7")
            && Matches(66, 65, 2, 0, 0, "446500", 3));
        check("Mode1 captured32-bit address registers request67 once while preserving extended REX X/B bits",
            Matches(45, 52, 8, 0, 0x18, "1CF8", 3, true)
            && Matches(45, 52, 8, 0, 0x18, "1CF8", 3, false, true)
            && Matches(41, 0, 1, 0, 0, "04E4", 0, true));
        check("Mode1 R12/R13 no-index fast-path quirks remain raw fields rather than repaired canonical instructions",
            Matches(65, 0, 1, 0, 0, "04", 1)
            && Matches(66, 0, 1, 0, 0, "05", 1));
        var invalid = NativeGeneratedModeOneMemoryEncoding.Project(53, 57, 1, 0, 0);
        check("Mode1 invalid memory shape exposes its diagnostic and no encoded fields",
            !invalid.Shape.IsAccepted && invalid.Shape.NativeDiagnosticCode == 1041234517 && invalid.Fields is null);
        bool exhaustive = true;
        int combinations = 0;
        int[] codes = [0, .. Enumerable.Range(37, 32)];
        foreach (int baseCode in codes)
        foreach (int indexCode in codes)
        foreach (byte scale in new byte[] { 1, 2, 4, 8 })
        foreach (int displacement in new[] { int.MinValue, -129, -128, -1, 0, 1, 127, 128, int.MaxValue })
        {
            var result = NativeGeneratedModeOneMemoryEncoding.Project(baseCode, indexCode, scale, displacement, 0xFF, 0xFF);
            if (!result.Shape.IsAccepted)
            {
                exhaustive &= result.Fields is null;
                continue;
            }
            combinations++;
            var fields = result.Fields!;
            exhaustive &= (fields.ModRm & 0x38) == 0x38
                && fields.DisplacementBytes.Length is 0 or 1 or 4
                && fields.ToAddressBytes().Length == 1 + (fields.Sib.HasValue ? 1 : 0) + fields.DisplacementBytes.Length
                && (fields.RexAddressBitsToOr & ~3) == 0;
        }
        check("Mode1 memory fields preserve captured ModRM register bits across18657 accepted boundary combinations",
            exhaustive && combinations == 18657);
    }
}
