using System.Buffers.Binary;
using System.IO.Compression;
using Firefall.Reconstruction.Startup;

internal static class MatrixModeZeroDescriptorChecks
{
    public static void Run(Action<string, bool> check)
    {
        uint[] packedVersions = [0x50000, 0x50004, 0x50100, 0x50101, 0x50102, 0x50103,
            0x60000, 0x60001, 0x60002, 0x60100, 0x60101, 0x60200];
        check("Native Windows version projection preserves all twelve exact OS/service-pack codes",
            packedVersions.Select((packed, index) => NativeWindowsVersionCode.Project(
                new(true, packed >> 16, (packed >> 8) & 255, (ushort)(packed & 255))) == (uint)(index + 1)).All(value => value));
        check("Native Windows version failure zero, unknown capture, later-than60200 and unrecognized older versions differ",
            NativeWindowsVersionCode.Project(new(false, 10, 0, 0)) == 0
            && NativeWindowsVersionCode.Project(new(null, 10, 0, 0)) is null
            && NativeWindowsVersionCode.Project(new(true, 10, 0, 0)) == 13
            && NativeWindowsVersionCode.Project(new(true, 5, 0, 1)) == 0
            && NativeWindowsVersionCode.Project(new(true, 6, 1, 2)) == 0
            && NativeWindowsVersionCode.Project(new(true, 6, 2, 1)) == 13);
        check("Native Windows version service-pack suppression and wrapping packed OR arithmetic are preserved",
            NativeWindowsVersionCode.Project(new(true, 6, 1, 1), true) == 10
            && NativeWindowsVersionCode.Project(new(true, 0x10005, 1, 0)) == 3
            && NativeWindowsVersionCode.Project(new(true, 5, 0, 0x100)) == 3);

        byte[] drive = Enumerable.Range(0, 16).Select(value => (byte)value).ToArray();
        byte[] adapter = Enumerable.Range(16, 16).Select(value => (byte)value).ToArray();
        byte[] firstText = [0x41, 0, 0xFF];
        byte[] secondText = [0x80, 0x42];
        var descriptor = new MatrixModeZeroDescriptorSnapshot(10, drive, adapter,
            new(0x11223344, firstText, 0x55667788, secondText));
        byte[] serialized = MatrixModeZeroDescriptorCodec.Serialize(descriptor);
        byte[] golden = Convert.FromHexString(
            "02000A000000000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F4433221103004100FF8877665502008042");
        check("Mode-zero operation2 descriptor preserves field order, raw digests, opaque words and ushort-length narrow bytes",
            serialized.SequenceEqual(golden));
        drive[0] ^= 0xFF;
        firstText[0] ^= 0xFF;
        check("Mode-zero serialized descriptor is independently owned and has no implicit CString terminator/UTF8 conversion",
            serialized.SequenceEqual(golden) && serialized.Length == 55 && serialized[45] == 0 && serialized[46] == 0xFF);
        var emptyMetadata = new MatrixModeZeroMetadataSnapshot(0, ReadOnlyMemory<byte>.Empty, 0, ReadOnlyMemory<byte>.Empty);
        var rejectedAdapter = NativeAdapterAddressFingerprint.Compute(0, 0, Array.Empty<NativeAdapterAddressCandidate>());
        var zeroSlots = MatrixSynchronizationModeZeroDigestSlots.Project(new(false, default), rejectedAdapter);
        var zeroDescriptor = MatrixModeZeroDescriptorCodec.FromCapturedDigestSlots(0, zeroSlots, emptyMetadata);
        byte[] zeros = MatrixModeZeroDescriptorCodec.Serialize(zeroDescriptor);
        check("Mode-zero failed optional fingerprints serialize initialized zeros without inventing activation failure",
            zeros.Length == 50 && zeros[0] == 2 && zeros.AsSpan(1).IndexOfAnyExcept((byte)0) < 0);
        bool rejectsUnknown = false, rejectsDigest = false, rejectsOversize = false;
        try
        {
            MatrixModeZeroDescriptorCodec.FromCapturedDigestSlots(0,
                MatrixSynchronizationModeZeroDigestSlots.Project(new(null, default), rejectedAdapter), emptyMetadata);
        }
        catch (ArgumentException) { rejectsUnknown = true; }
        try { MatrixModeZeroDescriptorCodec.Serialize(descriptor with { AdapterAddressDigest = new byte[15] }); }
        catch (ArgumentException) { rejectsDigest = true; }
        try
        {
            MatrixModeZeroDescriptorCodec.Serialize(descriptor with
            {
                Metadata = emptyMetadata with { NarrowBytesAt44 = new byte[65536] }
            });
        }
        catch (ArgumentException) { rejectsOversize = true; }
        var longest = MatrixModeZeroDescriptorCodec.Serialize(zeroDescriptor with
        {
            Metadata = emptyMetadata with { NarrowBytesAt28 = new byte[65535], NarrowBytesAt44 = new byte[65535] }
        });
        check("Mode-zero serializer rejects unknown slots, incorrect digest widths and length65536 but accepts both FFFF strings",
            rejectsUnknown && rejectsDigest && rejectsOversize && longest.Length == 131120
            && BinaryPrimitives.ReadUInt16LittleEndian(longest.AsSpan(42)) == 65535
            && BinaryPrimitives.ReadUInt16LittleEndian(longest.AsSpan(65583)) == 65535);

        byte[] compressed = Convert.FromHexString(
            "789C6362E0620002462666165636760E4E2E6E1E5E3E7E014121611151317109492969195939791763254166064786FF1DE569A14C0C0D4E005C580668");
        using var source = new MemoryStream(compressed);
        using var zlib = new ZLibStream(source, CompressionMode.Decompress);
        using var inflatedFixture = new MemoryStream();
        zlib.CopyTo(inflatedFixture);
        check("Independent synthetic zlib fixture inflates to the exact descriptor golden",
            inflatedFixture.ToArray().SequenceEqual(golden));
        byte[] frame = NativeZlibBufferFraming.AppendLengthAndMd5(compressed, (uint)golden.Length);
        check("Native compressed framing appends LE32 original size then MD5 over compression plus size, not over plaintext",
            Convert.ToHexString(frame) ==
            "789C6362E0620002462666165636760E4E2E6E1E5E3E7E014121611151317109492969195939791763254166064786FF1DE569A14C0C0D4E005C58066837000000159D67CA6724CF3C834F26204E4B5780");
        var envelope = MatrixSynchronizationRequest57NativeLayout.VerifyPostTransformBuffer(frame);
        var inflation = NativeZlibBufferFraming.ProjectCapturedInflation(envelope.Envelope!.PayloadBytes.Span, new(1, 0, golden));
        check("Verified compressed prefix is not routing word2; captured successful inflation exposes the actual serialized descriptor",
            envelope.Envelope.RawPayloadPrefixWord == 0x9C78
            && inflation.InflatedBytes is { } plaintext && plaintext.Span.SequenceEqual(golden)
            && BinaryPrimitives.ReadUInt16LittleEndian(plaintext.Span) == 2);

        byte[] requestFrame = Convert.FromHexString(
            "789C6364D0626060E0006200018400340A0000006582AD2905DC936FE37E78C5F10F88CD");
        var requestEnvelope = MatrixSynchronizationRequest57NativeLayout.VerifyPostTransformBuffer(requestFrame);
        byte[] request = Convert.FromHexString("01002A00000008000000");
        var requestInflation = NativeZlibBufferFraming.ProjectCapturedInflation(
            requestEnvelope.Envelope!.PayloadBytes.Span, new(1, 0, request));
        check("Mode-zero reader consumes inflated WORD1/seed42/value8, never the compressed prefix or length footer",
            requestInflation.InflatedBytes is { } requestBytes
            && MatrixSynchronizationDispatchNativeLayout.DecodeModeZeroFields(requestBytes.Span)
                is { Outcome: MatrixSynchronizationDirectOutcome.ActivationPathSelected, Fields.GeneratorSeed: 42 });
        byte[] padded = requestEnvelope.Envelope.PayloadBytes.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(padded.AsSpan(padded.Length - 4), 12);
        var shortOutput = NativeZlibBufferFraming.ProjectCapturedInflation(padded, new(1, 0, request));
        request[0] = 99;
        check("Native inflater accepts shorter completed output and preserves independent FF-prefilled suffix, not a fabricated exact-size gate",
            shortOutput.InflatedBytes is { } paddedBytes && paddedBytes.Length == 12
            && paddedBytes.Span[0] == 1 && paddedBytes.Span[10] == 0xFF && paddedBytes.Span[11] == 0xFF);
        check("Native inflater stream-end and zero-unused-input gates distinguish rejection from unobserved capture",
            NativeZlibBufferFraming.ProjectCapturedInflation(padded, new(-3, 0, default))
                .Outcome == NativeZlibInflationOutcome.NativeInflateRejected
            && NativeZlibBufferFraming.ProjectCapturedInflation(padded, new(1, 1, default))
                .Outcome == NativeZlibInflationOutcome.NativeInflateRejected
            && NativeZlibBufferFraming.ProjectCapturedInflation(padded, new(-3, null, default))
                .Outcome == NativeZlibInflationOutcome.NativeInflateRejected
            && NativeZlibBufferFraming.ProjectCapturedInflation(padded, new(null, 1, default))
                .Outcome == NativeZlibInflationOutcome.NativeInflateRejected
            && NativeZlibBufferFraming.ProjectCapturedInflation(padded, new(null, null, default))
                .Outcome == NativeZlibInflationOutcome.UnresolvedCapture
            && NativeZlibBufferFraming.ProjectCapturedInflation(new byte[3], new(1, 0, default))
                .Outcome == NativeZlibInflationOutcome.LengthFooterMissing);
        bool rejectsImpossibleCapture = false;
        try { NativeZlibBufferFraming.ProjectCapturedInflation(padded, new(1, 0, new byte[13])); }
        catch (ArgumentException) { rejectsImpossibleCapture = true; }
        check("Native successful inflation capture cannot claim bytes beyond its declared allocation", rejectsImpossibleCapture);
        BinaryPrimitives.WriteUInt32LittleEndian(padded.AsSpan(padded.Length - 4), uint.MaxValue);
        check("Native uint-sized allocation beyond managed array limits is unsupported, not silently narrowed or classified as native failure",
            NativeZlibBufferFraming.ProjectCapturedInflation(padded, new(1, 0, default))
                .Outcome == NativeZlibInflationOutcome.UnsupportedManagedAllocation);
    }
}
