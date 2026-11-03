namespace Firefall.Reconstruction.DeploymentCommon;

public enum IniReadAction
{
    ReturnNull,
    RetryWithLargerBuffer,
    CopyReturnedCharacters,
    ThrowValueTooLarge
}

public readonly record struct IniReadDecision(IniReadAction Action, int? RetryCapacity, uint CopyCharacterCount);

/// <summary>Observed wrapper policies only; never reads or writes an INI file.</summary>
public static class IniFileNativeLayout
{
    public const int InitialBufferCapacity = 512;
    public const int MaximumBufferCapacity = 32768;
    public const int AdvertisedMaxKeyValueSize = 32766;
    public const int GetStringMethodRow = 0xDA;
    public const int GetBoolWithDefaultMethodRow = 0xDD;
    public const int KeyExistsMethodRow = 0xE0;
    public const int DeleteSectionKeysMethodRow = 0xE1;
    public const uint GetStringBodyRva = 0x5328;
    public const uint GetBoolWithDefaultBodyRva = 0x53E8;
    public const uint KeyExistsBodyRva = 0x5480;
    public const uint DeleteSectionKeysBodyRva = 0x54BF;
    public const bool DiscardsSectionDeleteResult = true;

    /// <summary>Capacity is the current native loop capacity (512 through32768); returned count is opaque.</summary>
    public static IniReadDecision CaptureReadAttempt(int capacity, uint returnedCharacters)
    {
        if (returnedCharacters == 0)
            return new(IniReadAction.ReturnNull, null, 0);
        if (returnedCharacters != (uint)(capacity - 1))
            return new(IniReadAction.CopyReturnedCharacters, null, returnedCharacters);
        int next = unchecked(capacity * 2);
        return next <= MaximumBufferCapacity
            ? new(IniReadAction.RetryWithLargerBuffer, next, 0)
            : new(IniReadAction.ThrowValueTooLarge, null, 0);
    }

    public static bool CaptureSetStringResult(uint nativeReturn) => nativeReturn != 0;

    public static bool CaptureBooleanValue(string? value, bool defaultValue = false)
    {
        if (value is null)
            throw new NullReferenceException();
        return value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Equals("1", StringComparison.OrdinalIgnoreCase) ? true : defaultValue;
    }

    public static bool CaptureKeyExists(string? requestedKey, IReadOnlyList<string> enumeratedKeys)
    {
        foreach (string key in enumeratedKeys)
        {
            if (requestedKey is null)
                throw new NullReferenceException();
            if (requestedKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
