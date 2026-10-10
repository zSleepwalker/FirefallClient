using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedInitialModeZeroImageChecks
{
    public static void Run(Action<string, bool> check)
    {
        var zero = NativeGeneratedInitialModeZeroImage.Generate(() => 0, 0x12345678);
        bool BodyMatches(IReadOnlyList<ReadOnlyMemory<byte>> fragments, int count, params string[] repeatedGroup)
        {
            if (fragments.Count != 1 + count * repeatedGroup.Length || Convert.ToHexString(fragments[0].Span) != "8A18")
                return false;
            for (int i = 1; i < fragments.Count; i++)
                if (Convert.ToHexString(fragments[i].Span) != repeatedGroup[(i - 1) % repeatedGroup.Length])
                    return false;
            return true;
        }
        check("Initial mode0 all-zero stream produces60 ordered ADD/SUB immediate fragments with735 total UInt32 calls",
            zero.Prefix.IterationCount == 60 && zero.ConsumedUInt32Samples.Count == 735
            && zero.UInt32CallsBeforeWrappers == 687
            && zero.Forward.Bytes.Length == 405 && zero.Inverse.Bytes.Length == 405
            && BodyMatches(zero.ForwardBodyFragments, 60, "81C300000000")
            && BodyMatches(zero.InverseBodyFragments, 60, "81EB00000000"));
        var unary = NativeGeneratedInitialModeZeroImage.Generate(() => 2, 0);
        check("Initial mode0 unary stream consumes five draws per iteration and keeps compact EBX candidates",
            unary.Prefix.IterationCount == 62 && unary.ConsumedUInt32Samples.Count == 625
            && BodyMatches(unary.ForwardBodyFragments, 62, "43")
            && BodyMatches(unary.InverseBodyFragments, 62, "4B"));
        var rotate = NativeGeneratedInitialModeZeroImage.Generate(() => 4, 0);
        check("Initial mode0 CL-path rotation stream consumes ten draws per iteration and restores preparatory MOV before inverse rotation",
            rotate.Prefix.IterationCount == 64 && rotate.ConsumedUInt32Samples.Count == 955
            && BodyMatches(rotate.ForwardBodyFragments, 64, "B904000000", "D2C3")
            && BodyMatches(rotate.InverseBodyFragments, 64, "B904000000", "D2CB"));

        var transcript = new List<uint> { 0 };
        transcript.AddRange(Enumerable.Repeat(0xAAu, 264));
        transcript.Add(123);
        for (int i = 0; i < 60; i++)
            transcript.AddRange(new uint[] { 0, 1, 0, 0, 0, 1, 1, 1, 0 });
        transcript.Add(321);
        transcript.AddRange(Enumerable.Range(0, 48).Select(i => (uint)i));
        int cursor = 0;
        var register = NativeGeneratedInitialModeZeroImage.Generate(() => transcript[cursor++], 0xFFFFFFF0);
        check("Initial mode0 scripted register stream preserves separate forward/inverse parity, candidate selection and all pre-wrapper draws",
            cursor == transcript.Count && cursor == 855 && register.UInt32CallsBeforeWrappers == 807
            && register.ConsumedUInt32Samples.SequenceEqual(transcript)
            && register.Forward.ConsumedUInt32Samples.SequenceEqual(Enumerable.Range(0, 24).Select(i => (uint)i))
            && register.Inverse.ConsumedUInt32Samples.SequenceEqual(Enumerable.Range(24, 24).Select(i => (uint)i))
            && BodyMatches(register.ForwardBodyFragments, 60, "B900000000", "03D9")
            && BodyMatches(register.InverseBodyFragments, 60, "C6C100", "28CB"));

        const uint tableParameter = 0xFEDCBA96;
        transcript.Clear();
        transcript.Add(0);
        transcript.AddRange(Enumerable.Repeat(0u, 264));
        transcript.Add(0);
        for (int i = 0; i < 60; i++)
            transcript.AddRange(new uint[] { 0, 2, tableParameter, 0, 1, 0, 1, 0, 0 });
        transcript.Add(0);
        transcript.AddRange(Enumerable.Repeat(0u, 48));
        cursor = 0;
        var table = NativeGeneratedInitialModeZeroImage.Generate(() => transcript[cursor++], 0xFFFFFFF0);
        var source = NativeGeneratedBodyDecisionRules.ArithmeticTableSource(tableParameter, 0xFFFFFFF0);
        string address = Convert.ToHexString(NativeGeneratedImmediateBytes.Project(source.BaseAddress, 32).Bytes!.Value.Span);
        string index = Convert.ToHexString(NativeGeneratedImmediateBytes.Project(source.IndexValue, 32).Bytes!.Value.Span);
        check("Initial mode0 scripted indexed table stream preserves ESI/EDI preparation, pointer wrap and inverse fragment order",
            cursor == transcript.Count && cursor == 855 && source.Scale == 8
            && BodyMatches(table.ForwardBodyFragments, 60, "BE" + index, "C7C7" + address, "031CF7")
            && BodyMatches(table.InverseBodyFragments, 60, "C7C6" + index, "BF" + address, "2B1CF7"));
        var seeded = NativeGeneratedInitialModeZeroImage.FromSeed(0, 0x80001000);
        int replayCursor = 0;
        var replay = NativeGeneratedInitialModeZeroImage.Generate(() => seeded.ConsumedUInt32Samples[replayCursor++], 0x80001000);
        check("Initial mode0 MT seed image can be reproduced exactly from its complete captured UInt32 transcript",
            replayCursor == seeded.ConsumedUInt32Samples.Count
            && replay.Forward.Bytes.Span.SequenceEqual(seeded.Forward.Bytes.Span)
            && replay.Inverse.Bytes.Span.SequenceEqual(seeded.Inverse.Bytes.Span)
            && replay.UInt32CallsBeforeWrappers + 48 == replayCursor
            && replay.Prefix.TableBytes.Span.SequenceEqual(seeded.Prefix.TableBytes.Span));
        byte[] input = Enumerable.Range(0, 513).Select(i => (byte)(i & 255)).ToArray();
        byte[] seedZeroForward = NativeGeneratedInitialModeZeroByteTransform.Forward(input, seeded);
        check("Independent MT/body semantic reference seed0 golden preserves61 iterations,732 draws and exact513-byte transform digest",
            seeded.Prefix.IterationCount == 61 && seeded.UInt32CallsBeforeWrappers == 684
            && seeded.ConsumedUInt32Samples.Count == 732
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(seedZeroForward))
                == "42FF381C351AA6610E533D4D4716C49E7DAC67A595C5DB92F193E839096A6CF7"
            && Convert.ToHexString(seedZeroForward.AsSpan(0, 16)) == "410976568668F6B8064875554825F5B7");
        var controlZeroImmediate = NativeGeneratedInitialModeZeroImage.GenerateWithCapturedControlByte(() => 0, 0x12345678, 0);
        var controlZeroUnary = NativeGeneratedInitialModeZeroImage.GenerateWithCapturedControlByte(() => 2, 0, 0);
        var controlZeroRotate = NativeGeneratedInitialModeZeroImage.GenerateWithCapturedControlByte(() => 4, 0, 0);
        check("Captured control0 does not advance after direct immediates, initial loads or wrappers, but does after unary/rotation selections",
            controlZeroImmediate.ControlByteAt18DuringGeneration == 0
            && controlZeroImmediate.DiscardedUInt32Calls == 0
            && controlZeroImmediate.ConsumedUInt32Samples.Count == 735
            && controlZeroImmediate.Forward.Bytes.Span.SequenceEqual(zero.Forward.Bytes.Span)
            && controlZeroUnary.DiscardedUInt32Calls == 124 && controlZeroUnary.ConsumedUInt32Samples.Count == 749
            && controlZeroUnary.Forward.Bytes.Span.SequenceEqual(unary.Forward.Bytes.Span)
            && controlZeroRotate.DiscardedUInt32Calls == 704 && controlZeroRotate.ConsumedUInt32Samples.Count == 1659
            && controlZeroRotate.Inverse.Bytes.Span.SequenceEqual(rotate.Inverse.Bytes.Span));
        var controlZeroTable = NativeGeneratedInitialModeZeroImage.GenerateWithCapturedControlByte(() => 8, 0x12345678, 0);
        var controlZeroIndexedTable = NativeGeneratedInitialModeZeroImage.GenerateWithCapturedControlByte(() => 35, 0x12345678, 0);
        check("Captured control0 table form advances after index MOV and arithmetic, never after table-base MOV",
            controlZeroTable.Prefix.IterationCount == 68
            && controlZeroTable.DiscardedUInt32Calls == 544 && controlZeroTable.ConsumedUInt32Samples.Count == 1335
            && controlZeroIndexedTable.Prefix.IterationCount == 95
            && controlZeroIndexedTable.DiscardedUInt32Calls == 1520 && controlZeroIndexedTable.ConsumedUInt32Samples.Count == 2690
            && BodyMatches(controlZeroIndexedTable.ForwardBodyFragments, 95, "C7C602000000", "C7C778563412", "021C77")
            && BodyMatches(controlZeroIndexedTable.InverseBodyFragments, 95, "C7C602000000", "C7C778563412", "2A1C77"));
        transcript.Clear();
        transcript.Add(0);
        transcript.AddRange(Enumerable.Repeat(0u, 264));
        transcript.Add(0);
        for (int i = 0; i < 60; i++)
        {
            transcript.AddRange(new uint[] { 0, 1, 0, 0, 0, 0 });
            transcript.AddRange(Enumerable.Repeat(uint.MaxValue, 4));
            transcript.AddRange(new uint[] { 1, 0 });
            transcript.AddRange(Enumerable.Repeat(uint.MaxValue, 2));
            transcript.Add(0);
            transcript.AddRange(Enumerable.Repeat(uint.MaxValue, 3));
        }
        transcript.Add(0);
        transcript.AddRange(Enumerable.Range(0, 48).Select(i => (uint)i));
        cursor = 0;
        var controlZeroRegister = NativeGeneratedInitialModeZeroImage.GenerateWithCapturedControlByte(
            () => transcript[cursor++], 0, 0);
        check("Captured control0 register transcript discards before the next side's parity draw and preserves local/global inverse reversals",
            cursor == transcript.Count && cursor == 1395
            && controlZeroRegister.DiscardedUInt32Calls == 540 && controlZeroRegister.UInt32CallsBeforeWrappers == 1347
            && BodyMatches(controlZeroRegister.ForwardBodyFragments, 60, "B900000000", "01CB")
            && BodyMatches(controlZeroRegister.InverseBodyFragments, 60, "B100", "28CB"));
        var nonzeroControl = NativeGeneratedInitialModeZeroImage.GenerateWithCapturedControlByte(() => 4, 0, byte.MaxValue);
        check("Every nonzero captured control disables discards; constructor-control1 remains separate from an explicit captured override",
            nonzeroControl.DiscardedUInt32Calls == 0
            && nonzeroControl.ConsumedUInt32Samples.Count == rotate.ConsumedUInt32Samples.Count
            && nonzeroControl.Forward.Bytes.Span.SequenceEqual(rotate.Forward.Bytes.Span)
            && controlZeroRegister.Prefix.InitialByteAt18 == 1);
        var controlZeroRng = new NativeMt19937(0);
        var controlZeroSeed = NativeGeneratedInitialModeZeroImage.GenerateWithCapturedControlByte(controlZeroRng.NextUInt32, 0x80001000, 0);
        replayCursor = 0;
        var controlZeroReplay = NativeGeneratedInitialModeZeroImage.GenerateWithCapturedControlByte(
            () => controlZeroSeed.ConsumedUInt32Samples[replayCursor++], 0x80001000, 0);
        byte[] controlZeroForward = NativeGeneratedInitialModeZeroByteTransform.Forward(input, controlZeroSeed);
        check("Captured control0 seeded projection replays every consumed/discarded draw and retains managed forward/inverse byte effects",
            replayCursor == controlZeroSeed.ConsumedUInt32Samples.Count
            && controlZeroSeed.DiscardedUInt32Calls > 0
            && controlZeroReplay.Forward.Bytes.Span.SequenceEqual(controlZeroSeed.Forward.Bytes.Span)
            && controlZeroReplay.Inverse.Bytes.Span.SequenceEqual(controlZeroSeed.Inverse.Bytes.Span)
            && NativeGeneratedInitialModeZeroByteTransform.Inverse(controlZeroForward, controlZeroSeed).SequenceEqual(input));
        var counter = NativeGeneratedInitialModeZeroImage.Generate(() => 1, 0);
        byte[] counterForward = NativeGeneratedInitialModeZeroByteTransform.Forward(input, counter);
        check("Managed mode0 counter-source low-byte effect uses remaining byte count, including255/256 wrap",
            counterForward.SequenceEqual(input.Select((value, i) => unchecked((byte)(value - 61 * (input.Length - i)))))
            && NativeGeneratedInitialModeZeroByteTransform.Inverse(counterForward, counter).SequenceEqual(input));
        check("Managed mode0 low-byte projection retains wide-unary effects without depending on scratch upper bits",
            NativeGeneratedInitialModeZeroByteTransform.Forward(input, unary)
                .SequenceEqual(input.Select(value => unchecked((byte)(value + 62))))
            && NativeGeneratedInitialModeZeroByteTransform.Forward(input, zero).SequenceEqual(input));
        bool roundTrips = true;
        for (uint seed = 0; seed < 32; seed++)
        {
            var image = NativeGeneratedInitialModeZeroImage.FromSeed(seed, 0x80001000);
            byte[] forwardBytes = NativeGeneratedInitialModeZeroByteTransform.Forward(input, image);
            roundTrips &= NativeGeneratedInitialModeZeroByteTransform.Inverse(forwardBytes, image).SequenceEqual(input)
                && image.ForwardByteSteps.Count == image.Prefix.IterationCount
                && image.InverseByteSteps.Count == image.Prefix.IterationCount;
        }
        check("Managed mode0 forward/inverse effects round-trip all byte values and remaining-count boundaries across32 MT seeds", roundTrips);
        bool emptyRejected = false;
        try { NativeGeneratedInitialModeZeroByteTransform.Forward([], seeded); }
        catch (ArgumentException) { emptyRejected = true; }
        check("Managed mode0 byte projection does not invent safe zero-count behavior for quirky emitted branches", emptyRejected);
    }
}
