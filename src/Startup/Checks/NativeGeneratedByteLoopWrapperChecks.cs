using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedByteLoopWrapperChecks
{
    public static void Run(Action<string, bool> check)
    {
        int calls = 0;
        var first = NativeGeneratedByteLoopWrapper.AssembleCapturedBody([0x8A, 0x18], () => { calls++; return 0; });
        check("Native captured-body loop wrapper preserves complete byte golden and24 selector draws in construction order",
            calls == 24 && first.ConsumedUInt32Samples.Count == 24
            && Convert.ToHexString(first.Bytes.Span)
                == "5589E55051525356578B45088B550C09D2740C8A188818404A0F85F4FFFFFF5F5E5B5A5958C9C20800");
        var second = NativeGeneratedByteLoopWrapper.AssembleCapturedBody([], () => 1);
        check("Native wrapper selection retains alternate MOV/OR, two-byte INC/DEC and near JZ for odd samples",
            Convert.ToHexString(second.Bytes.Span)
                == "558BEC5051525356578B45088B550C0BD20F840C0000008818FFC0FFCA0F85F4FFFFFF5F5E5B5A5958C9C20800");
        var quirky = NativeGeneratedByteLoopWrapper.AssembleCapturedBody(new byte[160], () => 0);
        check("Native wrapper preserves short-skip displacement wrapping for captured160-byte body",
            quirky.Bytes.Span[17] == 0x74 && quirky.Bytes.Span[18] == 0xAA
            && quirky.Bytes.Length == 199);
        uint draw = 0;
        var ordered = NativeGeneratedByteLoopWrapper.AssembleCapturedBody([0x8A, 0x18], () => draw++);
        check("Native wrapper records exact selector stream ordering across opening, condition, footer, skip and closing",
            ordered.ConsumedUInt32Samples.SequenceEqual(Enumerable.Range(0, 24).Select(i => (uint)i))
            && Convert.ToHexString(ordered.Bytes.Span)
                == "558BEC5051525356578B45088B550C09D20F840D0000008A18881840FFCA0F85F3FFFFFF5F5E5B5A5958C9C20800");
    }
}
