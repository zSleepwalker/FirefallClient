using Firefall.Reconstruction.Startup;

internal static class MatrixSynchronizationDispatchChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] payload = [1, 0, 0x78, 0x56, 0x34, 0x12, 8, 0, 0, 0];
        check("Synchronization mode0 reads LE16 key and two LE32 fields, not an eight-byte payload length",
            MatrixSynchronizationDispatchNativeLayout.DecodeModeZeroFields(payload)
                is { Outcome: MatrixSynchronizationDirectOutcome.ActivationPathSelected,
                    Fields: { Key: 1, GeneratorSeed: 0x12345678, SecondDwordAt6: 8 },
                    BytesConsumed: 10, NativeDiagnosticCode: null });
        check("Synchronization mode0 preserves partial read progress and rejects every strict prefix",
            Enumerable.Range(0, 10).All(length =>
            {
                var result = MatrixSynchronizationDispatchNativeLayout.DecodeModeZeroFields(payload.AsSpan(0, length));
                return result.Fields is null && result.NativeDiagnosticCode is null
                    && result.Outcome == (length < 2 ? MatrixSynchronizationDirectOutcome.TruncatedLeadingWord
                        : MatrixSynchronizationDirectOutcome.TruncatedDwordPair)
                    && result.BytesConsumed == (length < 2 ? 0 : length < 6 ? 2 : 6);
            }));
        byte[] invalidKey = payload.ToArray();
        invalidKey[0] = 2;
        byte[] invalidValue = payload.ToArray();
        invalidValue[6] = 7;
        check("Synchronization mode0 diagnostic gates occur after both DWORD reads",
            MatrixSynchronizationDispatchNativeLayout.DecodeModeZeroFields(invalidKey)
                is { Outcome: MatrixSynchronizationDirectOutcome.UnsupportedKey, NativeDiagnosticCode: 0x0FFACB37 }
            && MatrixSynchronizationDispatchNativeLayout.DecodeModeZeroFields(invalidKey.AsSpan(0, 9))
                is { Outcome: MatrixSynchronizationDirectOutcome.TruncatedDwordPair, NativeDiagnosticCode: null }
            && MatrixSynchronizationDispatchNativeLayout.DecodeModeZeroFields(invalidValue)
                is { Outcome: MatrixSynchronizationDirectOutcome.UnsupportedSecondDword, NativeDiagnosticCode: 0x0FA98FC0 });
        check("Synchronization mode0 accepts ignored trailing bytes and the complete unsigned generator seed",
            MatrixSynchronizationDispatchNativeLayout.DecodeModeZeroFields(
                new byte[] { 1, 0, 0xFF, 0xFF, 0xFF, 0xFF, 8, 0, 0, 0, 0xAA })
                is { Outcome: MatrixSynchronizationDirectOutcome.ActivationPathSelected,
                    Fields.GeneratorSeed: uint.MaxValue, BytesConsumed: 10 });
        check("Synchronization registered numeric-key hashing is plain FNV1a over four LE bytes, without final mix",
            MatrixSynchronizationDispatchNativeLayout.ComputeRegisteredKeyHash(0) == 0x4B95F515
            && MatrixSynchronizationDispatchNativeLayout.ComputeRegisteredKeyHash(1) == 0xFB69B604
            && MatrixSynchronizationDispatchNativeLayout.ComputeRegisteredKeyHash(0x1234) == 0x83CE9A1B
            && MatrixSynchronizationDispatchNativeLayout.ComputeRegisteredKeyHash(0xFFFF) == 0x16C70CDB
            && MatrixSynchronizationDispatchNativeLayout.SelectRegisteredBucket(1, 7, 4) == 0
            && MatrixSynchronizationDispatchNativeLayout.SelectRegisteredBucket(1, 7, 5) == 4);
    }
}
