using System.Security.Cryptography;
using Firefall.Reconstruction.Startup;

internal static class MatrixSynchronization57Checks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] envelope = "abcd"u8.ToArray().Concat(Convert.FromHexString("E2FC714C4727EE9395F324CD2E7F331F")).ToArray();
        var verified = MatrixSynchronizationRequest57NativeLayout.VerifyPostTransformBuffer(envelope);
        check("SynchronizationRequest57 verifies MD5 and retains raw prefix without treating it as the inflated routing key",
            verified is { Outcome: MatrixSynchronizationEnvelopeOutcome.EnvelopeVerified,
                Envelope: { RawPayloadPrefixWord: 0x6261 } }
            && verified.Envelope.PayloadBytes.Span.SequenceEqual("abcd"u8)
            && verified.Envelope.DigestBytes.Span.SequenceEqual(envelope.AsSpan(4)));
        check("SynchronizationRequest57 minimum is20, not only a sixteen-byte digest or a two-byte leading word",
            Enumerable.Range(0, 20).All(length =>
                MatrixSynchronizationRequest57NativeLayout.VerifyPostTransformBuffer(envelope.AsSpan(0, length))
                    is { Outcome: MatrixSynchronizationEnvelopeOutcome.TooShort, Envelope: null }));
        bool mutationsRejected = true;
        for (int i = 0; i < envelope.Length; i++)
        {
            byte[] changed = envelope.ToArray();
            changed[i] ^= 1;
            mutationsRejected &= MatrixSynchronizationRequest57NativeLayout.VerifyPostTransformBuffer(changed)
                is { Outcome: MatrixSynchronizationEnvelopeOutcome.DigestMismatch, Envelope: null };
        }
        check("SynchronizationRequest57 rejects changed payload/digest and nonzero owner gate before parsing",
            mutationsRejected
            && MatrixSynchronizationRequest57NativeLayout.VerifyPostTransformBuffer(envelope, 0x80)
                is { Outcome: MatrixSynchronizationEnvelopeOutcome.GateBlocked, Envelope: null });
        check("SynchronizationRequest57 external transform selection uses mode, gate and minimum length without invoking hook",
            MatrixSynchronizationRequest57NativeLayout.SelectExternalTransform(0, 1, 20)
            && MatrixSynchronizationRequest57NativeLayout.SelectExternalTransform(0, 0x80, 20)
            && !MatrixSynchronizationRequest57NativeLayout.SelectExternalTransform(0, 0, 20)
            && !MatrixSynchronizationRequest57NativeLayout.SelectExternalTransform(1, 1, 20)
            && !MatrixSynchronizationRequest57NativeLayout.SelectExternalTransform(0, 1, 19));
        var provider = NativeGeneratedInitialModeZeroImage.FromSeed(0, 0x12340000);
        byte[] transformedEnvelope = NativeGeneratedInitialModeZeroByteTransform.Forward(envelope, provider);
        var decoded = MatrixSynchronizationRequest57NativeLayout.VerifyWithReconstructedInitialProvider(
            transformedEnvelope, 0, 1, provider);
        check("SynchronizationRequest57 opt-in reconstructed provider uses inverse bytes before MD5 without calling generated code",
            decoded is { Outcome: MatrixSynchronizationEnvelopeOutcome.EnvelopeVerified }
            && decoded.Envelope!.PayloadBytes.Span.SequenceEqual("abcd"u8)
            && !transformedEnvelope.SequenceEqual(envelope));
        check("SynchronizationRequest57 opt-in provider preserves gate/minimum/mode bypasses without requiring a provider on skipped paths",
            MatrixSynchronizationRequest57NativeLayout.VerifyWithReconstructedInitialProvider(
                transformedEnvelope, 1, 1, null).Outcome == MatrixSynchronizationEnvelopeOutcome.GateBlocked
            && MatrixSynchronizationRequest57NativeLayout.VerifyWithReconstructedInitialProvider(
                transformedEnvelope.AsSpan(0, 19), 0, 1, null).Outcome == MatrixSynchronizationEnvelopeOutcome.TooShort
            && MatrixSynchronizationRequest57NativeLayout.VerifyWithReconstructedInitialProvider(
                envelope, 0, 0, null).Outcome == MatrixSynchronizationEnvelopeOutcome.EnvelopeVerified);
        bool providerRequired = false;
        try
        {
            MatrixSynchronizationRequest57NativeLayout.VerifyWithReconstructedInitialProvider(
                transformedEnvelope, 0, 1, null);
        }
        catch (ArgumentNullException) { providerRequired = true; }
        check("SynchronizationRequest57 selected reconstructed-provider path rejects missing provider instead of verifying untransformed bytes", providerRequired);
        check("SynchronizationRequest57 selects registered-handler/direct dispatch only after envelope verification",
            MatrixSynchronizationRequest57NativeLayout.SelectDispatchAfterVerification(verified.Outcome, 1)
                == MatrixSynchronizationDispatchSelection.RegisteredHandlerByLeadingWord
            && MatrixSynchronizationRequest57NativeLayout.SelectDispatchAfterVerification(verified.Outcome, 0)
                == MatrixSynchronizationDispatchSelection.DirectByLeadingWord
            && MatrixSynchronizationRequest57NativeLayout.SelectDispatchAfterVerification(
                MatrixSynchronizationEnvelopeOutcome.DigestMismatch, 1) == MatrixSynchronizationDispatchSelection.None);
        byte[] copiedPayload = verified.Envelope!.PayloadBytes.ToArray();
        byte[] copiedDigest = verified.Envelope.DigestBytes.ToArray();
        envelope[0] ^= 0xFF;
        envelope[^1] ^= 0xFF;
        check("SynchronizationRequest57 verified payload and footer are independent copies",
            verified.Envelope.PayloadBytes.Span.SequenceEqual(copiedPayload)
            && verified.Envelope.DigestBytes.Span.SequenceEqual(copiedDigest));
        foreach (int length in new[] { 4, 55, 56, 63, 64, 65, 127, 128, 129 })
        {
            byte[] payload = Enumerable.Range(0, length).Select(i => (byte)i).ToArray();
            byte[] packet = new byte[] { 57 }.Concat(payload).Concat(MD5.HashData(payload)).ToArray();
            var wire = MatrixServerMessages56To61Wire.Decode(packet);
            check($"SynchronizationRequest57 keeps whole-remainder boundary and verifies MD5 padding length{length}",
                wire is { BytesConsumed: var used, Payload: MatrixRemainingBufferPayload raw } && used == packet.Length
                && MatrixSynchronizationRequest57NativeLayout.VerifyPostTransformBuffer(raw.Bytes.Span)
                    is { Outcome: MatrixSynchronizationEnvelopeOutcome.EnvelopeVerified });
        }
    }
}
