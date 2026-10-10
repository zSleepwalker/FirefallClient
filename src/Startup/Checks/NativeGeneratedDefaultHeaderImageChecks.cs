using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedDefaultHeaderImageChecks
{
    public static void Run(Action<string, bool> check)
    {
        bool BodyMatches(IReadOnlyList<ReadOnlyMemory<byte>> fragments, uint count, params string[] group)
        {
            if (fragments.Count != 1 + count * group.Length || Convert.ToHexString(fragments[0].Span) != "8A18")
                return false;
            for (int i = 1; i < fragments.Count; i++)
                if (Convert.ToHexString(fragments[i].Span) != group[(i - 1) % group.Length])
                    return false;
            return true;
        }
        var zero = NativeGeneratedDefaultHeaderImage.GenerateCaptured(() => 0, 0x12345678, 1);
        check("Captured mode1 all-zero stream saves60 ADD/SUB pairs,729 draws and two468-byte images",
            zero.Prefix.IterationCount == 60 && zero.ModeWordAt4DuringGeneration == 1
            && zero.Prefix.InitialWordAt4 == 0 && zero.Prefix.InitialByteAt18 == 1
            && zero.UInt32CallsBeforeWrappers == 687 && zero.ConsumedUInt32Samples.Count == 729
            && zero.Forward.Bytes.Length == 468 && zero.Inverse.Bytes.Length == 468
            && BodyMatches(zero.ForwardBodyFragments, 60, "4881C300000000")
            && BodyMatches(zero.InverseBodyFragments, 60, "4881EB00000000"));
        var unary = NativeGeneratedDefaultHeaderImage.GenerateCaptured(() => 2, 0, 1);
        var rotation = NativeGeneratedDefaultHeaderImage.GenerateCaptured(() => 4, 0, 1);
        check("Captured mode1 unary/RCX-rotation streams preserve native candidate counts and inverse preparation order",
            unary.ConsumedUInt32Samples.Count == 619
            && BodyMatches(unary.ForwardBodyFragments, 62, "48FFC3")
            && BodyMatches(unary.InverseBodyFragments, 62, "48FFCB")
            && rotation.ConsumedUInt32Samples.Count == 949
            && BodyMatches(rotation.ForwardBodyFragments, 64, "48C7C104000000", "D2C3")
            && BodyMatches(rotation.InverseBodyFragments, 64, "48C7C104000000", "D2CB"));
        var unaryControlZero = NativeGeneratedDefaultHeaderImage.GenerateCaptured(() => 2, 0, 1, 0);
        var rotationControlZero = NativeGeneratedDefaultHeaderImage.GenerateCaptured(() => 4, 0, 1, 0);
        check("Captured mode1 control0 includes REX bytes in post-candidate sums and leaves wrappers unadvanced",
            unaryControlZero.DiscardedUInt32Calls == 124 && unaryControlZero.ConsumedUInt32Samples.Count == 743
            && rotationControlZero.DiscardedUInt32Calls == 576 && rotationControlZero.ConsumedUInt32Samples.Count == 1525
            && unaryControlZero.UInt32CallsBeforeWrappers + 42 == unaryControlZero.ConsumedUInt32Samples.Count
            && rotationControlZero.UInt32CallsBeforeWrappers + 42 == rotationControlZero.ConsumedUInt32Samples.Count);
        byte[] input = Enumerable.Range(0, 513).Select(i => (byte)i).ToArray();
        var seeded = NativeGeneratedDefaultHeaderImage.FromSeedCaptured(0, 0x80001000, 1);
        byte[] seedForward = NativeGeneratedDefaultHeaderByteTransform.Forward(input, seeded);
        check("Captured mode1 seed0 semantic output matches independent513-byte golden while wrappers consume42 draws",
            seeded.UInt32CallsBeforeWrappers == 684 && seeded.ConsumedUInt32Samples.Count == 726
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(seedForward))
                == "42FF381C351AA6610E533D4D4716C49E7DAC67A595C5DB92F193E839096A6CF7");
        bool replays = true;
        bool roundTrips = true;
        bool sharedSemantics = true;
        for (uint seed = 0; seed < 32; seed++)
        {
            var modeZero = NativeGeneratedInitialModeZeroImage.FromSeed(seed, 0x80001000);
            var modeOne = NativeGeneratedDefaultHeaderImage.FromSeedCaptured(seed, 0x80001000, 1);
            var controlZero = NativeGeneratedDefaultHeaderImage.FromSeedCaptured(seed, 0x80001000, 1, 0);
            int cursor = 0;
            var replay = NativeGeneratedDefaultHeaderImage.GenerateCaptured(
                () => controlZero.ConsumedUInt32Samples[cursor++], 0x80001000, 1, 0);
            replays &= cursor == controlZero.ConsumedUInt32Samples.Count
                && replay.DiscardedUInt32Calls == controlZero.DiscardedUInt32Calls
                && replay.Forward.Bytes.Span.SequenceEqual(controlZero.Forward.Bytes.Span)
                && replay.Inverse.Bytes.Span.SequenceEqual(controlZero.Inverse.Bytes.Span);
            byte[] modeOneForward = NativeGeneratedDefaultHeaderByteTransform.Forward(input, modeOne);
            byte[] controlZeroForward = NativeGeneratedDefaultHeaderByteTransform.Forward(input, controlZero);
            roundTrips &= NativeGeneratedDefaultHeaderByteTransform.Inverse(modeOneForward, modeOne).SequenceEqual(input)
                && NativeGeneratedDefaultHeaderByteTransform.Inverse(controlZeroForward, controlZero).SequenceEqual(input);
            sharedSemantics &= modeOne.ForwardByteSteps.SequenceEqual(modeZero.ForwardByteSteps)
                && modeOne.InverseByteSteps.SequenceEqual(modeZero.InverseByteSteps)
                && modeOneForward.SequenceEqual(NativeGeneratedInitialModeZeroByteTransform.Forward(input, modeZero))
                && modeOne.UInt32CallsBeforeWrappers == modeZero.UInt32CallsBeforeWrappers;
        }
        check("Captured mode1 control0 complete images replay every draw and both modes retain positive-length byte round trips across32 seeds",
            replays && roundTrips && sharedSemantics);
        bool modeRejected = false;
        bool emptyRejected = false;
        try { NativeGeneratedDefaultHeaderImage.GenerateCaptured(() => 0, 0, 2); }
        catch (ArgumentOutOfRangeException) { modeRejected = true; }
        try { NativeGeneratedDefaultHeaderByteTransform.Forward([], seeded); }
        catch (ArgumentException) { emptyRejected = true; }
        check("Captured-mode generator rejects unsupported modes and byte effects reject unsafe empty-input claims",
            modeRejected && emptyRejected);
    }
}
