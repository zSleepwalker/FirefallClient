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

/// <summary>Observed severity-three diagnostic routing only; never logs, displays UI or launches a helper.</summary>
public static class ConversionDiagnosticNativeLayout
{
    public const uint DestructorVa = 0x10289EC0;
    public const uint DefaultDisplayHelperVa = 0x10288EA0;
    public const uint InterceptorPointerVa = 0x11596B1C;
    public const uint SeverityThreeHandlerPointerVa = 0x11596B18;
    public const uint DisplayEnableByteVa = 0x11596B13;
    public const int ConverterDiagnosticSeverity = 3;
    public const string HelperFileName = "debug_message.exe";
    public const string MessageBoxTitle = "Fatal error";
    public const uint MessageBoxFlags = 0x40010;
    public const uint WaitMilliseconds = uint.MaxValue;

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
