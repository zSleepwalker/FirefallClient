namespace Firefall.Reconstruction.DumpTruck;

public enum AuthInitializationAction
{
    KeepProvidedAuth,
    LeaveEmpty,
    TryStoredCredentials
}

public enum AuthSubmitFallbackAction
{
    KeepProvidedAuth,
    UseMarkedRegistryLoginName,
    LeaveEmptyAfterRegistryLookup
}

/// <summary>Static auth handoff decisions only; no registry, credentials, or network access.</summary>
public static class AuthHandoffNativeLayout
{
    public const int OptionsAuthOffset = 0x08;
    public const int OptionsForgeOffset = 0x14;
    public const int RequestContextAuthOffset = 0x08;
    public const uint RegistryLoginReaderVa = 0x4024E0;
    public const uint StoredCredentialFallbackVa = 0x404850;
    public const uint XmlTextEscapeVa = 0x409460;
    public const uint RequestContextConstructorVa = 0x409830;
    public const uint RequestAuthFormatterVa = 0x44B820;
    public const string RegistryLoginMarkerFormat = "!%s";
    public const string NativeReportAuthFormat = "<report-crash><auth>%s</auth></report-crash>";

    public static AuthInitializationAction CaptureInitialization(bool forge, bool suppliedAuthEmpty) =>
        !suppliedAuthEmpty ? AuthInitializationAction.KeepProvidedAuth
        : forge ? AuthInitializationAction.TryStoredCredentials
        : AuthInitializationAction.LeaveEmpty;

    public static AuthSubmitFallbackAction CaptureSubmitFallback(bool suppliedAuthEmpty, bool registryLoginEmpty) =>
        !suppliedAuthEmpty ? AuthSubmitFallbackAction.KeepProvidedAuth
        : registryLoginEmpty ? AuthSubmitFallbackAction.LeaveEmptyAfterRegistryLookup
        : AuthSubmitFallbackAction.UseMarkedRegistryLoginName;

    public static string CaptureXmlText(string? sourceCString)
    {
        string visible = sourceCString ?? string.Empty;
        int nul = visible.IndexOf('\0');
        if (nul >= 0)
            visible = visible[..nul];
        return visible.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);
    }
}
