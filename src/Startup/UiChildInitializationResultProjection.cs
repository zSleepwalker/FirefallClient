namespace Firefall.Reconstruction.Startup;

public readonly record struct UiChildInitializationResultProjection(
    byte FirstHelperResult,
    byte? SecondHelperResult,
    byte NativeChildResult,
    bool SecondHelperCallReached,
    bool DirectlyClearsChildField6C,
    bool CallerContinuesUiInitialization);

public static class UiChildInitializationResultProjectionRules
{
    // Instruction addresses below are raw file offsets, not runtime VAs.
    public const uint ChildInitializationEntryAddress = 0xCA66F0;
    public const uint FirstHelperCallAddress = 0xCA6A61;
    public const uint FirstHelperEntryAddress = 0xCA6C20;
    public const uint SecondHelperCallAddress = 0xCA6AA3;
    public const uint SecondHelperEntryAddress = 0xCA6C80;
    public const uint ChildInitializationReturnAddress = 0xCA6ABD;
    public const uint CallerCallAddress = 0xBFB542;
    public const uint CallerZeroResultEpilogueAddress = 0xBFB668;
    public const uint CallerLaterInitializationAddress = 0xBFB8DC;
    public const int ChildField6COffset = 0x6C;

    public static UiChildInitializationResultProjection Project(
        byte firstHelperResult,
        byte? secondHelperResult)
    {
        if (firstHelperResult == 0)
        {
            if (secondHelperResult.HasValue)
                throw new ArgumentException(
                    "The second helper result must be absent when the first helper returns zero.",
                    nameof(secondHelperResult));

            return new UiChildInitializationResultProjection(
                firstHelperResult,
                null,
                NativeChildResult: 0,
                SecondHelperCallReached: false,
                DirectlyClearsChildField6C: true,
                CallerContinuesUiInitialization: false);
        }

        if (!secondHelperResult.HasValue)
            throw new ArgumentException(
                "The second helper result is required when the first helper returns nonzero.",
                nameof(secondHelperResult));

        byte nativeChildResult = secondHelperResult.Value == 0 ? (byte)0 : (byte)1;
        return new UiChildInitializationResultProjection(
            firstHelperResult,
            secondHelperResult,
            nativeChildResult,
            SecondHelperCallReached: true,
            DirectlyClearsChildField6C: false,
            CallerContinuesUiInitialization: nativeChildResult != 0);
    }
}
