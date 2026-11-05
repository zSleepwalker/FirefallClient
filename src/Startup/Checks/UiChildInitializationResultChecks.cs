using Firefall.Reconstruction.Startup;

internal static class UiChildInitializationResultChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI child initialization result records the captured helper and caller RAW anchors",
            UiChildInitializationResultProjectionRules.ChildInitializationEntryAddress == 0xCA66F0
            && UiChildInitializationResultProjectionRules.FirstHelperCallAddress == 0xCA6A61
            && UiChildInitializationResultProjectionRules.FirstHelperEntryAddress == 0xCA6C20
            && UiChildInitializationResultProjectionRules.SecondHelperCallAddress == 0xCA6AA3
            && UiChildInitializationResultProjectionRules.SecondHelperEntryAddress == 0xCA6C80
            && UiChildInitializationResultProjectionRules.ChildInitializationReturnAddress == 0xCA6ABD
            && UiChildInitializationResultProjectionRules.CallerCallAddress == 0xBFB542
            && UiChildInitializationResultProjectionRules.CallerZeroResultEpilogueAddress == 0xBFB668
            && UiChildInitializationResultProjectionRules.CallerLaterInitializationAddress == 0xBFB8DC
            && UiChildInitializationResultProjectionRules.ChildField6COffset == 0x6C);

        foreach (byte firstHelperResult in new byte[] { 0x00, 0x01, 0x80, 0xFF })
        {
            if (firstHelperResult == 0)
            {
                var projection = UiChildInitializationResultProjectionRules.Project(
                    firstHelperResult,
                    secondHelperResult: null);
                check("UI child initialization zero first-helper result clears field 6C and skips the second helper",
                    projection == new UiChildInitializationResultProjection(
                        0x00,
                        null,
                        NativeChildResult: 0,
                        SecondHelperCallReached: false,
                        DirectlyClearsChildField6C: true,
                        CallerContinuesUiInitialization: false));
                continue;
            }

            foreach (byte secondHelperResult in new byte[] { 0x00, 0x01, 0x80, 0xFF })
            {
                byte normalizedResult = secondHelperResult == 0 ? (byte)0 : (byte)1;
                var projection = UiChildInitializationResultProjectionRules.Project(
                    firstHelperResult,
                    secondHelperResult);
                check($"UI child initialization helper results {firstHelperResult:X2}/{secondHelperResult:X2} are captured and normalized",
                    projection == new UiChildInitializationResultProjection(
                        firstHelperResult,
                        secondHelperResult,
                        normalizedResult,
                        SecondHelperCallReached: true,
                        DirectlyClearsChildField6C: false,
                        CallerContinuesUiInitialization: normalizedResult != 0));
            }
        }

        check("UI child initialization rejects a second-helper capture when the first helper gates it off",
            ThrowsArgumentException(() =>
                UiChildInitializationResultProjectionRules.Project(0, secondHelperResult: 0)));
        check("UI child initialization requires a second-helper capture when the first helper gates it on",
            ThrowsArgumentException(() =>
                UiChildInitializationResultProjectionRules.Project(1, secondHelperResult: null)));
    }

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
