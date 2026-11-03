namespace Firefall.Reconstruction.Awesomium;

public readonly record struct DiagnosticLoggingSetup(
    int OutputDestination,
    bool AppendExistingLog,
    int DiagnosticChecksMode,
    int MinimumSeverityAfterNormalSetup);

/// <summary>Inert caller arguments and threshold assignments, not a logger or numeric parser.</summary>
public static class LoggingSetupNativeLayout
{
    public const uint SetupCallerVa = 0x10033BC0;
    public const uint InitializeLoggerVa = 0x1028A2A0;
    public const uint VerboseConfigurationConstructorVa = 0x102B5720;
    public const uint IntegerConversionWrapperVa = 0x1029CB30;
    public const uint DiagnosticChecksModeStorageVa = 0x11596AF8;

    // The caller uses the converter's output slot even when its success byte is false.
    public static DiagnosticLoggingSetup CaptureNormalSetup(
        bool disableLoggingSwitchPresent, bool asciiTypeValueNonempty, int observedLogLevelOutput) =>
        new(disableLoggingSwitchPresent ? 0 : 1, asciiTypeValueNonempty, 0,
            ConversionDiagnosticNativeLayout.CaptureMinimumSeverityAssignment(observedLogLevelOutput));

    public static int CaptureVerboseThresholdAssignment(
        int previousThreshold, bool verboseValueNonempty, bool conversionSucceeded, int observedVerboseOutput) =>
        verboseValueNonempty && conversionSucceeded ? unchecked(-observedVerboseOutput) : previousThreshold;
}
