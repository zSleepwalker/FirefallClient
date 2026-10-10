using System.Text;
using Firefall.Reconstruction.Startup;

internal static class NativeAdapterAddressFingerprintChecks
{
    public static void Run(Action<string, bool> check)
    {
        NativeAdapterRegistrySnapshot Registry(string text, bool available = true,
            uint probe = 0, uint type = 1, uint read = 0) =>
            new(available, probe, type, read, Encoding.ASCII.GetBytes(text));
        check("Native adapter registry filter allows a missing/unopened Connection key rather than rejecting the adapter",
            NativeAdapterAddressFingerprint.EvaluateRegistryAscii(Registry("", false, 5, 0, 5)) == true);
        check("Native adapter registry filter rejects an available key's failed size query, wrong type or failed data read",
            NativeAdapterAddressFingerprint.EvaluateRegistryAscii(Registry(@"PCI\DEVICE", probe: 5)) == false
            && NativeAdapterAddressFingerprint.EvaluateRegistryAscii(Registry(@"PCI\DEVICE", type: 3)) == false
            && NativeAdapterAddressFingerprint.EvaluateRegistryAscii(Registry(@"PCI\DEVICE", read: 234)) == false);
        check("Native adapter PnP filter permits only case-insensitive PCI/USB/BTH prefixes before the first backslash",
            new[] { @"PCI\A", @"uSb\B", @"BTH\C" }.All(text =>
                NativeAdapterAddressFingerprint.EvaluateRegistryAscii(Registry(text)) == true)
            && new[] { "PCI", @"ROOT\A", @"VMBUS\B", @"PCIE\C", @"\PCI" }.All(text =>
                NativeAdapterAddressFingerprint.EvaluateRegistryAscii(Registry(text)) == false));
        var unsupported = new NativeAdapterRegistrySnapshot(true, 0, 1, 0, new byte[] { 0xFF, 1, 2, (byte)'\\' });
        check("Native adapter registry ASCII projection explicitly leaves non-ASCII locale comparisons unresolved",
            NativeAdapterAddressFingerprint.EvaluateRegistryAscii(unsupported) is null);

        byte[] low = [1, 2, 3, 4, 5, 6, 0xEE, 0xFF];
        byte[] high = [7, 8, 9, 10, 11, 12];
        NativeAdapterAddressCandidate[] candidates =
        [
            new(71, 6, high, true), new(6, 6, low, true),
            new(24, 6, high, null), new(6, 5, low, null),
            new(6, 999, ReadOnlyMemory<byte>.Empty, false)
        ];
        var accepted = NativeAdapterAddressFingerprint.Compute(111, 0, candidates);
        check("Native adapter fingerprint gates type6/71 and length>=6 before captured filter, sorts and hashes exact address lengths",
            accepted.NativeReturnValue == true
            && accepted.Digest is { } digest && Convert.ToHexString(digest.Span) == "D2BC225F9724EA69812867FC45794A2E"
            && accepted.SortedAddresses.Count == 2 && accepted.SortedAddresses[0].Length == 6);
        low[0] = 0xFF;
        check("Native adapter fingerprint retains independent copies rather than caller-owned address aliases",
            accepted.SortedAddresses[0].Span.SequenceEqual(new byte[] { 1, 2, 3, 4, 5, 6 }));
        byte[] repeated = [1, 2, 3, 4, 5, 6];
        var duplicate = NativeAdapterAddressFingerprint.Compute(111, 0,
            new[] { new NativeAdapterAddressCandidate(6, 6, repeated, true), new(6, 6, repeated, true) });
        check("Native adapter fingerprint retains duplicate address records instead of deduplicating",
            duplicate.SortedAddresses.Count == 2 && duplicate.Digest is { } duplicateDigest
            && Convert.ToHexString(duplicateDigest.Span) == "243FD6A1000F6A337C58C99C6C3112AD");
        var empty = NativeAdapterAddressFingerprint.Compute(111, 0, Array.Empty<NativeAdapterAddressCandidate>());
        check("Native successful empty adapter selection writes standard MD5 of empty input, not a failure",
            empty.NativeReturnValue == true && empty.Digest is { } emptyDigest
            && Convert.ToHexString(emptyDigest.Span) == "D41D8CD98F00B204E9800998ECF8427E");
        var probe = NativeAdapterAddressFingerprint.Compute(0, 0, candidates);
        var enumeration = NativeAdapterAddressFingerprint.Compute(111, 5, candidates);
        var missing = NativeAdapterAddressFingerprint.Compute(111, 0,
            new[] { new NativeAdapterAddressCandidate(6, 6, high, null) });
        check("Native adapter fingerprint requires probe111 and second-call zero; unresolved filter never yields a success digest",
            probe is { NativeReturnValue: false, Digest: null, ApiStatus: 0 }
            && enumeration is { NativeReturnValue: false, Digest: null, ApiStatus: 5 }
            && missing is { NativeReturnValue: null, Digest: null, UnresolvedRecordIndex: 0 });
        bool invalidRejected = false;
        try { NativeAdapterAddressFingerprint.Compute(111, 0, new[] { new NativeAdapterAddressCandidate(6, 6, high[..5], true) }); }
        catch (ArgumentException) { invalidRejected = true; }
        check("Native accepted address outside captured storage is explicitly rejected rather than shortened", invalidRejected);
    }
}
