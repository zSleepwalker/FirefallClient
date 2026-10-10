using System.Security.Cryptography;

namespace Firefall.Reconstruction.Startup;

public sealed record NativeAdapterAddressCandidate(
    uint InterfaceType, uint AddressLength, ReadOnlyMemory<byte> AddressStorage, bool? CapturedFilterDecision);

public sealed record NativeAdapterRegistrySnapshot(
    bool ConnectionKeyAvailable, uint ProbeStatus, uint ProbeValueType,
    uint ReadStatus, ReadOnlyMemory<byte> ResizedValueBytes);

public enum NativeAdapterFingerprintOutcome
{
    Accepted,
    ProbeRejected,
    EnumerationRejected,
    UnresolvedFilter
}

public sealed record NativeAdapterFingerprintResult(
    NativeAdapterFingerprintOutcome Outcome, ReadOnlyMemory<byte>? Digest,
    IReadOnlyList<ReadOnlyMemory<byte>> SortedAddresses, uint? ApiStatus, int? UnresolvedRecordIndex)
{
    public bool? NativeReturnValue => Outcome switch
    {
        NativeAdapterFingerprintOutcome.Accepted => true,
        NativeAdapterFingerprintOutcome.UnresolvedFilter => null,
        NativeAdapterFingerprintOutcome.ProbeRejected
            or NativeAdapterFingerprintOutcome.EnumerationRejected => false,
        _ => throw new InvalidOperationException("Unknown adapter fingerprint outcome.")
    };
}

/// <summary>Offline GetAdaptersInfo snapshot projection. Does not enumerate host interfaces or read the registry.</summary>
public static class NativeAdapterAddressFingerprint
{
    public const uint MethodFileOffset = 0x127D0C0;
    public const uint RegistryFilterMethodFileOffset = 0x127CCD0;
    public const uint PnpPrefixMethodFileOffset = 0x127CB20;
    public const uint GetAdaptersInfoSlotVirtualAddress = 0x1AC412C;
    public const int AddressLengthOffset = 0x190;
    public const int AddressOffset = 0x194;
    public const int InterfaceTypeOffset = 0x1A0;
    public const string RegistryRoot =
        @"System\CurrentControlSet\Control\Network\{4d36e972-e325-11ce-bfc1-08002be10318}";
    public const string ConnectionSuffix = @"\Connection";
    public const string PnpValueName = "PnpInstanceID";

    public static NativeAdapterFingerprintResult Compute(uint probeStatus, uint enumerationStatus,
        IReadOnlyList<NativeAdapterAddressCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        if (probeStatus != 111)
            return new(NativeAdapterFingerprintOutcome.ProbeRejected, null, Array.Empty<ReadOnlyMemory<byte>>(),
                probeStatus, null);
        if (enumerationStatus != 0)
            return new(NativeAdapterFingerprintOutcome.EnumerationRejected, null, Array.Empty<ReadOnlyMemory<byte>>(),
                enumerationStatus, null);

        var selected = new List<ReadOnlyMemory<byte>>();
        for (int i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            if (candidate.InterfaceType is not (6 or 71) || candidate.AddressLength < 6)
                continue;
            if (candidate.CapturedFilterDecision is null)
                return new(NativeAdapterFingerprintOutcome.UnresolvedFilter, null,
                    Array.Empty<ReadOnlyMemory<byte>>(), null, i);
            if (candidate.CapturedFilterDecision == false)
                continue;
            if (candidate.AddressLength > candidate.AddressStorage.Length)
                throw new ArgumentException("Accepted adapter address exceeds its captured storage.", nameof(candidates));
            selected.Add(candidate.AddressStorage[..checked((int)candidate.AddressLength)].ToArray());
        }
        var addresses = selected.ToArray();
        NativeByteVectorSort.Sort(addresses);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        foreach (var address in addresses)
            hash.AppendData(address.Span);
        return new(NativeAdapterFingerprintOutcome.Accepted, hash.GetHashAndReset(),
            Array.AsReadOnly(addresses), null, null);
    }

    /// <summary>
    /// Projects captured registry status and C-locale ASCII prefix comparison.
    /// Other locale-sensitive prefix bytes are unresolved, not silently rejected.
    /// </summary>
    public static bool? EvaluateRegistryAscii(NativeAdapterRegistrySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!snapshot.ConnectionKeyAvailable)
            return true;
        if (snapshot.ProbeStatus != 0 || snapshot.ProbeValueType != 1 || snapshot.ReadStatus != 0)
            return false;

        var value = snapshot.ResizedValueBytes.Span;
        int slash = value.IndexOf((byte)'\\');
        if (slash < 0)
            return false;
        var prefix = value[..slash];
        int zero = prefix.IndexOf((byte)0);
        if (zero >= 0)
            prefix = prefix[..zero];
        foreach (byte b in prefix)
            if (b >= 0x80)
                return null;
        if (prefix.Length != 3)
            return false;
        Span<byte> folded = stackalloc byte[3];
        for (int i = 0; i < folded.Length; i++)
            folded[i] = prefix[i] is >= (byte)'A' and <= (byte)'Z'
                ? (byte)(prefix[i] + 32) : prefix[i];
        return folded.SequenceEqual("pci"u8) || folded.SequenceEqual("usb"u8) || folded.SequenceEqual("bth"u8);
    }
}
