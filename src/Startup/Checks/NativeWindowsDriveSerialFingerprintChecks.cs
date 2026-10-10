using System.Buffers.Binary;
using System.Text;
using Firefall.Reconstruction.Startup;

internal static class NativeWindowsDriveSerialFingerprintChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] headerBytes = new byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(headerBytes, 36);
        BinaryPrimitives.WriteUInt32LittleEndian(headerBytes.AsSpan(4), 64);
        byte[] descriptorBytes = new byte[64];
        BinaryPrimitives.WriteUInt32LittleEndian(descriptorBytes, 36);
        BinaryPrimitives.WriteUInt32LittleEndian(descriptorBytes.AsSpan(24), 32);
        Encoding.ASCII.GetBytes("  SN123  \0ignored").CopyTo(descriptorBytes, 32);
        var header = new NativeStorageIoctlCapture(true, 8, headerBytes);
        var descriptor = new NativeStorageIoctlCapture(true, 64, descriptorBytes);
        var capture = new NativeWindowsDriveSerialCapture(10, @"C:\Windows".ToCharArray(), 123, header, descriptor);
        var result = NativeWindowsDriveSerialFingerprint.Compute(capture);
        check("Native storage property query emits twelve zero bytes: device property, standard query and zero additional parameter",
            NativeWindowsDriveSerialFingerprint.CreateQueryInput().SequenceEqual(new byte[12])
            && NativeWindowsDriveSerialFingerprint.IoctlStorageQueryProperty == 0x2D1400);
        check("Native Windows-drive serial fingerprint truncates directory to two UTF16 units and hashes untrimmed serial CString bytes",
            result is { NativeReturnValue: true, DevicePath: @"\\.\C:" }
            && result.Digest is { } digest && Convert.ToHexString(digest.Span) == "9B7A7550A9DA9CC47A7755638C4ECF6E"
            && result.SerialBytes is { } serial && serial.Span.SequenceEqual("  SN123  "u8));
        descriptorBytes[32] = 0xFF;
        check("Native Windows-drive serial capture retains independent serial bytes after caller storage changes",
            result.SerialBytes is { } copied && copied.Span[0] == (byte)' ');
        var adapter = NativeAdapterAddressFingerprint.Compute(111, 0, Array.Empty<NativeAdapterAddressCandidate>());
        var slots = MatrixSynchronizationModeZeroDigestSlots.ProjectCapturedFingerprints(result, adapter);
        check("Native Windows-drive and adapter offline captures populate their separate mode-zero digest slots",
            slots is { WindowsDriveSerialCopied: true, AdapterCopied: true }
            && slots.WindowsDriveSerialSlot is { } driveSlot
            && Convert.ToHexString(driveSlot.Span) == "9B7A7550A9DA9CC47A7755638C4ECF6E");
        descriptorBytes[32] = (byte)' ';
        BinaryPrimitives.WriteUInt32LittleEndian(descriptorBytes.AsSpan(24), 0);
        var zeroOffset = NativeWindowsDriveSerialFingerprint.Compute(capture);
        check("Native storage serial offset zero reads descriptor header bytes instead of being treated as missing serial",
            zeroOffset.NativeReturnValue == true && zeroOffset.Digest is { } headerDigest
            && Convert.ToHexString(headerDigest.Span) == "C3E97DD6E97FB5125688C97F36720CBE");
        BinaryPrimitives.WriteUInt32LittleEndian(descriptorBytes.AsSpan(24), 32);
        descriptorBytes[32] = 0;
        var emptySerial = NativeWindowsDriveSerialFingerprint.Compute(capture);
        check("Native empty serial CString succeeds with MD5(empty)", emptySerial.NativeReturnValue == true
            && emptySerial.Digest is { } emptyDigest && Convert.ToHexString(emptyDigest.Span) == "D41D8CD98F00B204E9800998ECF8427E");
        check("Native Windows-drive API/handle failure and exact query-return lengths are separate rejection gates",
            NativeWindowsDriveSerialFingerprint.Compute(capture with { WindowsDirectoryReturnValue = 0 })
                .Outcome == NativeWindowsDriveFingerprintOutcome.DirectoryApiFailed
            && NativeWindowsDriveSerialFingerprint.Compute(capture with { FileHandle = uint.MaxValue })
                .Outcome == NativeWindowsDriveFingerprintOutcome.InvalidFileHandle
            && NativeWindowsDriveSerialFingerprint.Compute(capture with { HeaderQuery = header with { NativeSucceeded = false } })
                .Outcome == NativeWindowsDriveFingerprintOutcome.HeaderQueryFailed
            && NativeWindowsDriveSerialFingerprint.Compute(capture with { HeaderQuery = header with { BytesReturned = 7 } })
                .Outcome == NativeWindowsDriveFingerprintOutcome.HeaderLengthMismatch
            && NativeWindowsDriveSerialFingerprint.Compute(capture with { DescriptorQuery = descriptor with { NativeSucceeded = false } })
                .Outcome == NativeWindowsDriveFingerprintOutcome.DescriptorQueryFailed
            && NativeWindowsDriveSerialFingerprint.Compute(capture with { DescriptorQuery = descriptor with { BytesReturned = 63 } })
                .Outcome == NativeWindowsDriveFingerprintOutcome.DescriptorLengthMismatch);
        check("Native directory return value is tested only for zero and handle only against FFFFFFFF",
            NativeWindowsDriveSerialFingerprint.Compute(capture with { WindowsDirectoryReturnValue = 1000, FileHandle = 0 })
                .NativeReturnValue == true);
        BinaryPrimitives.WriteUInt32LittleEndian(descriptorBytes.AsSpan(24), 64);
        var pastEnd = NativeWindowsDriveSerialFingerprint.Compute(capture);
        BinaryPrimitives.WriteUInt32LittleEndian(descriptorBytes.AsSpan(24), 32);
        descriptorBytes.AsSpan(32).Fill(0x41);
        var unterminated = NativeWindowsDriveSerialFingerprint.Compute(capture);
        check("Native storage offset outside allocation or missing CString terminator is unsupported, not invented native failure/zero digest",
            pastEnd is { Outcome: NativeWindowsDriveFingerprintOutcome.UnsupportedNativeBufferAccess, NativeReturnValue: null, Digest: null }
            && unterminated is { Outcome: NativeWindowsDriveFingerprintOutcome.UnsupportedNativeBufferAccess, NativeReturnValue: null, Digest: null });
        var uncaptured = NativeWindowsDriveSerialFingerprint.Compute(capture with { DescriptorQuery = null });
        check("Native unobserved storage query remains unresolved", uncaptured is { NativeReturnValue: null, Digest: null });
        check("Native incomplete header and descriptor snapshots remain unresolved despite successful captured API results",
            NativeWindowsDriveSerialFingerprint.Compute(capture with
            {
                HeaderQuery = header with { Bytes = headerBytes.AsMemory(0, 7) }
            }) is { Outcome: NativeWindowsDriveFingerprintOutcome.UnresolvedCapture, NativeReturnValue: null }
            && NativeWindowsDriveSerialFingerprint.Compute(capture with
            {
                DescriptorQuery = descriptor with { Bytes = descriptorBytes.AsMemory(0, 63) }
            }) is { Outcome: NativeWindowsDriveFingerprintOutcome.UnresolvedCapture, NativeReturnValue: null });
        byte[] shortHeaderBytes = headerBytes.ToArray();
        BinaryPrimitives.WriteUInt32LittleEndian(shortHeaderBytes.AsSpan(4), 24);
        check("Native successful short descriptor would read beyond allocation for offset field and is explicitly unsupported",
            NativeWindowsDriveSerialFingerprint.Compute(capture with
            {
                HeaderQuery = header with { Bytes = shortHeaderBytes },
                DescriptorQuery = new(true, 24, new byte[24])
            }) is { Outcome: NativeWindowsDriveFingerprintOutcome.UnsupportedNativeBufferAccess, NativeReturnValue: null, Digest: null });
        var incompleteSlots = MatrixSynchronizationModeZeroDigestSlots.ProjectCapturedFingerprints(uncaptured, adapter);
        check("Native missing drive snapshot does not prevent retaining a separately known adapter digest",
            incompleteSlots is { FirstSlot: null, AdapterCopied: true }
            && incompleteSlots.AdapterSlot is { } adapterSlot
            && Convert.ToHexString(adapterSlot.Span) == "D41D8CD98F00B204E9800998ECF8427E");
    }
}
