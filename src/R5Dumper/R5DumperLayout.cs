namespace Firefall.Reconstruction.R5Dumper;

// Static facts observed in system\bin\r5dumper.exe (x86, console, VS2008 CRT).
public static class R5DumperLayout
{
    public const string Sha256 = "63F5E802548809C1327A4569CD66953BC3EDBFEFB6177D753A403022C3194729";
    public const string Md5 = "1c3037e686ee347fd26dcb045ed34a85";
    public const long FileSize = 29400;
    public const uint ImageBase = 0x400000;
    public const uint EntryPointVa = 0x4034DE;
    public const uint MainVa = 0x401D50;
    public const uint ReadUserInfoVa = 0x401430;
    public const uint WriteDumpVa = 0x401C10;
    public const string Usage = "Usage: <exe> <pid> <dmpflags> <inhandle> <dmpfile>";
    public const string PdbPath = @"C:\Users\plindblad\Documents\Visual Studio 2008\Projects\dumpmini\Release\r5dumper.pdb";
    public const uint OpenProcessAccess = 0x450;

    public const int ExitOk = 0, ExitDbgHelpLoad = -1, ExitUsage = -2, ExitUserInfo = -3, ExitDumpFailed = -4;
}

public enum UserInfoKind { Str, Int32, Int64 }

public sealed record UserInfoRecord(uint StreamType, UserInfoKind Kind, string Value);

public static class R5DumperUserInfo
{
    // Parses "<key>|<type>|<value>" (type case-insensitive); two fields mean an empty value.
    public static UserInfoRecord Parse(string line)
    {
        var parts = line.Split('|');
        if (parts.Length is not (2 or 3))
            throw new FormatException("Malformed user information string");
        var kind = parts[1].ToLowerInvariant() switch
        {
            "i32" => UserInfoKind.Int32,
            "i64" => UserInfoKind.Int64,
            "str" => UserInfoKind.Str,
            _ => throw new FormatException($"Unknown user information type: \"{parts[1]}\""),
        };
        return new UserInfoRecord(uint.Parse(parts[0]), kind, parts.Length == 3 ? parts[2] : "");
    }
}
