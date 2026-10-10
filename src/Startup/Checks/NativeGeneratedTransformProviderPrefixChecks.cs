using System.Security.Cryptography;
using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedTransformProviderPrefixChecks
{
    public static void Run(Action<string, bool> check)
    {
        int draws = 0;
        var prefix = NativeGeneratedTransformProviderPrefix.Initialize(() =>
        {
            int index = draws++;
            return index == 0 ? 40u : 0xABCDEF00u | (uint)index;
        });
        check("Native generated provider draws range60..100 before264 raw low-byte table samples",
            prefix is { IterationCount: 100, FirstRawSample: 40, UInt32CallsConsumed: 265 }
            && draws == 265 && prefix.TableBytes.Length == 264
            && prefix.TableBytes.Span.SequenceEqual(Enumerable.Range(1, 264)
                .Select(index => unchecked((byte)(0xABCDEF00u | (uint)index))).ToArray()));
        check("Native generated provider prefix preserves fixed header0200/byte3, initial word0 and byte-at18 value1",
            prefix is { HeaderWordAt0: 0x200, HeaderByteAt2: 3, InitialWordAt4: 0, InitialByteAt18: 1 });
        var zeroSamples = NativeGeneratedTransformProviderPrefix.Initialize(() => 0);
        check("Native generated provider range minimum and zero low-byte table do not invent permutation or key-scheduling semantics",
            zeroSamples.IterationCount == 60 && zeroSamples.TableBytes.Span.IndexOfAnyExcept((byte)0) < 0);
        var seeded = NativeGeneratedTransformProviderPrefix.FromSeed(5489);
        var independentPosition = new NativeMt19937(5489);
        uint first = independentPosition.NextUInt32();
        byte[] expected = Enumerable.Range(0, 264).Select(_ => (byte)independentPosition.NextUInt32()).ToArray();
        check("Native mode-zero seed uses existing MT generator without skipping the initial range draw",
            seeded.FirstRawSample == 3499211612
            && seeded.IterationCount == 60 + first % 41
            && seeded.TableBytes.Span.SequenceEqual(expected));
        check("Native generated provider seeded table has stable independent SHA256 golden",
            Convert.ToHexString(SHA256.HashData(NativeGeneratedTransformProviderPrefix.FromSeed(0).TableBytes.Span))
                == "8A33AACAE22C723A39957683C3674FF7923E33B29F6DA050416C2D92826FCEC6");
        var boundary = NativeGeneratedProviderBoundaryPlanner.ForModeZero();
        check("Native mode-zero generated boundary plan preserves ten opening and eight closing selector calls",
            boundary.Opening.ToArray().Select(step => step.OperationCode).SequenceEqual<uint>([29u, 26, 29, 29, 29, 29, 29, 29, 26, 26])
            && boundary.Closing.ToArray().Select(step => step.OperationCode).SequenceEqual<uint>([28u, 28, 28, 28, 28, 28, 25, 33]));
        check("Native generated boundary plan retains raw descriptor codes, header-derived choices, offsets8/12 and final16-bit value8",
            boundary.Opening.Span[0] == new NativeGeneratedBoundaryStep(29, 42)
            && boundary.Opening.Span[1] == new NativeGeneratedBoundaryStep(26, 42, 41)
            && boundary.Opening.Span[8] == new NativeGeneratedBoundaryStep(26, 37, 42, 8)
            && boundary.Opening.Span[9] == new NativeGeneratedBoundaryStep(26, 39, 42, 12)
            && boundary.Closing.Span[7] == new NativeGeneratedBoundaryStep(33, ImmediateValue: 8, ImmediateWidth: 16)
            && NativeGeneratedProviderBoundaryPlanner.ForModeZero(1, 3).Opening.Span[8].FirstDescriptorCode == 38
            && NativeGeneratedProviderBoundaryPlanner.ForModeZero(1, 3).Opening.Span[9].FirstDescriptorCode == 40);
        var encodings = NativeGeneratedBoundaryEncodingCatalog.InitialModeZero();
        check("Native initial mode-zero boundary byte candidates preserve MOV template order and exact x86 opening/closing bytes",
            encodings.Opening.Count == 10 && encodings.Closing.Count == 8
            && encodings.Opening.Where((_, index) => index != 1).All(entry => entry.Candidates.Count == 1)
            && encodings.Opening[1].Candidates.Count == 2
            && Convert.ToHexString(encodings.Opening[1].Candidates[0].Span) == "89E5"
            && Convert.ToHexString(encodings.Opening[1].Candidates[1].Span) == "8BEC"
            && Convert.ToHexString(encodings.Opening.SelectMany(entry => entry.Candidates[0].ToArray()).ToArray())
                == "5589E55051525356578B45088B550C"
            && encodings.Closing.All(entry => entry.Candidates.Count == 1)
            && Convert.ToHexString(encodings.Closing.SelectMany(entry => entry.Candidates[0].ToArray()).ToArray())
                == "5F5E5B5A5958C9C20800");
        int discarded = 0;
        uint discardedCount = NativeGeneratedCandidateRngAdvance.Consume(0, [254, 255], () => { discarded++; return 0; });
        check("Native post-candidate helper advances only RNG by wrapped unsigned byte sum modulo5 when control18 is zero",
            discardedCount == 4 && discarded == 4
            && NativeGeneratedCandidateRngAdvance.Count(0, [255, 1]) == 1
            && NativeGeneratedCandidateRngAdvance.Count(0, []) == 0
            && NativeGeneratedCandidateRngAdvance.Consume(1, [254, 255], () => throw new InvalidOperationException()) == 0
            && NativeGeneratedCandidateRngAdvance.Count(255, [254, 255]) == 0);
        byte[] overflowBytes = new byte[16843010];
        overflowBytes.AsSpan().Fill(255);
        check("Native candidate byte sum wraps at UInt32 before modulo5 rather than accumulating in64 bits",
            NativeGeneratedCandidateRngAdvance.Count(0, overflowBytes) == 4);
    }
}
