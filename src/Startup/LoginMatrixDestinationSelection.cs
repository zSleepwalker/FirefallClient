namespace Firefall.Reconstruction.Startup;

public enum LoginMatrixDestinationSource
{
    None,
    LuaArgument,
    DevelopmentMatrixOverride,
    LoginSelectedMatrixUrl
}

public readonly record struct LoginMatrixDestinationSelectionResult(
    string DestinationText,
    LoginMatrixDestinationSource Source);

/// <summary>
/// Reconstructs destination-source precedence only. Native host/port validation,
/// connection state, and retained MatrixConnection preference are separate.
/// </summary>
public static class LoginMatrixDestinationSelection
{
    public static LoginMatrixDestinationSelectionResult Select(
        string? luaArgument,
        string? developmentMatrixOverride,
        string? loginSelectedMatrixUrl)
    {
        if (!string.IsNullOrEmpty(loginSelectedMatrixUrl))
            return new(loginSelectedMatrixUrl, LoginMatrixDestinationSource.LoginSelectedMatrixUrl);

        if (!string.IsNullOrEmpty(developmentMatrixOverride))
            return new(developmentMatrixOverride, LoginMatrixDestinationSource.DevelopmentMatrixOverride);

        if (!string.IsNullOrEmpty(luaArgument))
            return new(luaArgument, LoginMatrixDestinationSource.LuaArgument);

        return new("", LoginMatrixDestinationSource.None);
    }
}
