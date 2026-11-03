namespace Firefall.Reconstruction.Awesomium;

public enum SeverityThreeDiagnosticRoute
{
    ConsumedByInterceptor,
    CustomHandler,
    DefaultDisplayHelper
}

public enum DiagnosticDisplayAction
{
    ReturnWithoutDisplay,
    AttemptHelperLaunch
}

public enum DiagnosticLaunchAction
{
    WaitIndefinitelyThenCloseHandles,
    ShowMessageBox
}

/// <summary>Observed diagnostic configuration and routing; never logs, displays UI or launches a helper.</summary>
public static class ConversionDiagnosticNativeLayout
{
    public const uint DestructorVa = 0x10289EC0;
    public const uint DefaultDisplayHelperVa = 0x10288EA0;
    public const uint InterceptorPointerVa = 0x11596B1C;
    public const uint InterceptorSetterVa = 0x10288E50;
    public const uint MinimumSeverityGetterVa = 0x10288E40;
    public const uint MinimumSeveritySetterVa = 0x10288E70;
    public const uint MinimumSeverityStorageVa = 0x11596B04;
    public const uint MinimumSeverityFallbackVa = 0x1127B090;
    public const uint GpuInitializationVa = 0x1019CAA0;
    public const uint GpuInterceptorVa = 0x1019C9C0;
    public const uint GpuTeardownVa = 0x1019C7F0;
    public const bool GpuInterceptorConsumesDiagnostic = false;
    public const uint SeverityThreeHandlerPointerVa = 0x11596B18;
    public const uint DisplayEnableByteVa = 0x11596B13;
    public const int ConverterDiagnosticSeverity = 3;
    public const string HelperFileName = "debug_message.exe";
    public const string MessageBoxTitle = "Fatal error";
    public const uint MessageBoxFlags = 0x40010;
    public const uint WaitMilliseconds = uint.MaxValue;

    public static int CaptureMinimumSeverityAssignment(int requestedSeverity) =>
        requestedSeverity >= 3 ? 3 : requestedSeverity;

    public static bool CaptureGpuInterceptorInstallation(
        bool initializationFailed, bool singleProcessSwitchPresent, bool inProcessGpuSwitchPresent) =>
        !initializationFailed && !singleProcessSwitchPresent && !inProcessGpuSwitchPresent;

    public static SeverityThreeDiagnosticRoute CaptureRoute(
        bool interceptorPresent, bool observedInterceptorConsumed, bool customHandlerPresent) =>
        interceptorPresent && observedInterceptorConsumed ? SeverityThreeDiagnosticRoute.ConsumedByInterceptor
        : customHandlerPresent ? SeverityThreeDiagnosticRoute.CustomHandler
        : SeverityThreeDiagnosticRoute.DefaultDisplayHelper;

    public static DiagnosticDisplayAction CaptureDefaultDisplayAdmission(
        bool messageNonempty, bool displayEnabled, bool convertedMessageNonempty) =>
        messageNonempty && displayEnabled && convertedMessageNonempty
            ? DiagnosticDisplayAction.AttemptHelperLaunch : DiagnosticDisplayAction.ReturnWithoutDisplay;

    public static DiagnosticLaunchAction CaptureLaunchResult(bool createProcessSucceeded) =>
        createProcessSucceeded ? DiagnosticLaunchAction.WaitIndefinitelyThenCloseHandles
        : DiagnosticLaunchAction.ShowMessageBox;
}
