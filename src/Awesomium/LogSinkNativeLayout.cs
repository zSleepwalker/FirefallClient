namespace Firefall.Reconstruction.Awesomium;

public readonly record struct LogSinkAttempts(
    bool OutputDebugString,
    bool StandardErrorAndFlush,
    bool EnterFileOutputBlock);

/// <summary>Observed sink admission only, not successful delivery or severity-handler behavior.</summary>
public static class LogSinkNativeLayout
{
    public const uint MessageDestructorVa = 0x10289EC0;
    public const uint SynchronizationInitializerVa = 0x10289AE0;
    public const uint WriteFileCallVa = 0x1028A018;
    public const uint WriteFileImportVa = 0x112402F8;
    public const uint StandardStreamsVa = 0x11553420;
    public const uint StandardErrorOffset = 0x40;

    public static LogSinkAttempts CaptureSinkAdmission(
        bool interceptedAndConsumed, int destination, int severity)
    {
        if (interceptedAndConsumed)
            return new(false, false, false);

        bool debugger = destination is 2 or 3;
        return new(debugger, debugger || severity >= 2, destination != 0 && destination != 2);
    }

    public static bool CaptureFileWriteAdmission(bool enteredFileOutputBlock, bool openHelperReturnedTrue) =>
        enteredFileOutputBlock && openHelperReturnedTrue;
}
