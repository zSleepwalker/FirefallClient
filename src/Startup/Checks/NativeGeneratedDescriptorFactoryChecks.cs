using System.Buffers.Binary;
using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedDescriptorFactoryChecks
{
    public static void Run(Action<string, bool> check)
    {
        (int First, int Last, uint Kind, byte Width)[] bands =
        [
            (1, 20, 4, 8), (21, 36, 4, 16), (37, 52, 4, 32), (53, 68, 4, 64),
            (69, 74, 5, 16), (75, 82, 6, 64), (83, 90, 7, 64), (91, 106, 8, 128),
            (107, 122, 9, 32), (123, 138, 10, 32)
        ];
        bool allCodes = true;
        foreach (var band in bands)
            for (int code = band.First; code <= band.Last; code++)
                allCodes &= NativeGeneratedDescriptorFactories.ProjectCode(code).Fields
                    == new NativeGeneratedCodeFields(band.Kind, band.Width, (byte)(code - band.First));
        check("Native generated operand code factory preserves every classification band and zero sentinel",
            allCodes && NativeGeneratedDescriptorFactories.ProjectCode(0).Fields == new NativeGeneratedCodeFields(0, 0, 0));
        byte[] operand = Enumerable.Repeat((byte)0xA5, 64).ToArray();
        var accepted = NativeGeneratedDescriptorFactories.ApplyCodeOperand(operand, 42);
        check("Native generated code operand writes kind4, code42, width32/index5 and preserves unassigned bytes",
            accepted.NativeDiagnosticCode is null
            && BinaryPrimitives.ReadUInt32LittleEndian(operand) == 4
            && BinaryPrimitives.ReadUInt32LittleEndian(operand.AsSpan(8)) == 4
            && BinaryPrimitives.ReadUInt32LittleEndian(operand.AsSpan(12)) == 42
            && operand[16] == 32 && operand[17] == 5
            && BinaryPrimitives.ReadUInt32LittleEndian(operand.AsSpan(48)) == 32
            && operand[4] == 0xA5 && operand[18] == 0xA5 && operand[30] == 0xA5 && operand[63] == 0xA5);
        byte[] rejected = Enumerable.Repeat((byte)0xA5, 64).ToArray();
        var invalid = NativeGeneratedDescriptorFactories.ApplyCodeOperand(rejected, -1);
        check("Native invalid operand code stores supplied code before diagnostic without completing wrapper writes",
            invalid.NativeDiagnosticCode == 1040197660u
            && BinaryPrimitives.ReadInt32LittleEndian(rejected.AsSpan(12)) == -1
            && rejected.Where((_, index) => index < 12 || index > 15).All(value => value == 0xA5)
            && NativeGeneratedDescriptorFactories.ProjectCode(139).NativeDiagnosticCode == 1040197660u);
        byte[] immediateOperand = Enumerable.Repeat((byte)0xA5, 64).ToArray();
        NativeGeneratedDescriptorFactories.ApplyImmediateOperand(immediateOperand, 0x89ABCDEF, 0x01234567, 0x123410);
        check("Native immediate operand constructor writes kind2/value at40/44, truncates width to low byte and preserves holes",
            BinaryPrimitives.ReadUInt32LittleEndian(immediateOperand) == 2
            && BinaryPrimitives.ReadUInt64LittleEndian(immediateOperand.AsSpan(40)) == 0x0123456789ABCDEF
            && BinaryPrimitives.ReadUInt32LittleEndian(immediateOperand.AsSpan(48)) == 16
            && immediateOperand[56] == 1 && immediateOperand[57] == 0xA5
            && immediateOperand[4] == 0xA5 && immediateOperand[18] == 0xA5 && immediateOperand[32] == 0xA5);

        byte[] operation = Enumerable.Repeat((byte)0xA5, 200).ToArray();
        NativeGeneratedDescriptorFactories.ApplyNoOperands(operation, 25);
        bool sparse = BinaryPrimitives.ReadUInt32LittleEndian(operation) == 25;
        var cleared = new HashSet<int>();
        foreach (int slot in new[] { 8, 72, 136 })
        {
            foreach (int offset in new[] { 0, 8, 12, 20, 24, 48 })
                for (int i = 0; i < 4; i++) cleared.Add(slot + offset + i);
            foreach (int offset in new[] { 16, 28 })
                for (int i = 0; i < 2; i++) cleared.Add(slot + offset + i);
        }
        for (int i = 4; i < operation.Length; i++)
            sparse &= operation[i] == (cleared.Contains(i) ? 0 : 0xA5);
        check("Native no-operand operation performs sparse clears rather than invented full initialization", sparse);
        byte[] first = Enumerable.Range(0, 64).Select(i => (byte)i).ToArray();
        byte[] second = Enumerable.Range(64, 64).Select(i => (byte)i).ToArray();
        NativeGeneratedDescriptorFactories.ApplyOneOperand(operation, 29, first);
        check("Native unary operation copies all64 operand bytes and retains header padding",
            operation.AsSpan(8, 64).SequenceEqual(first) && operation[4] == 0xA5
            && BinaryPrimitives.ReadUInt32LittleEndian(operation) == 29);
        NativeGeneratedDescriptorFactories.ApplyTwoOperands(operation, 26, first, second);
        check("Native binary operation preserves operand order at8/72 and clears only defined third-slot fields",
            operation.AsSpan(8, 64).SequenceEqual(first) && operation.AsSpan(72, 64).SequenceEqual(second)
            && BinaryPrimitives.ReadUInt32LittleEndian(operation) == 26 && operation[140] == 0xA5
            && BinaryPrimitives.ReadUInt32LittleEndian(operation.AsSpan(136)) == 0);
        byte[] immediateGolden = [0xEF, 0xCD, 0xAB, 0x89, 0x67, 0x45, 0x23, 0x01];
        bool widths = true;
        for (uint bits = 0; bits < 80; bits++)
        {
            var result = NativeGeneratedImmediateBytes.Project(0x0123456789ABCDEF, bits);
            uint count = bits / 8;
            if (count is 1 or 2 or 4 or 8)
                widths &= result.NativeDiagnosticCode is null
                    && result.Bytes is { } bytes && bytes.Span.SequenceEqual(immediateGolden.AsSpan(0, (int)count));
            else
                widths &= result.Bytes is null && result.NativeDiagnosticCode == 1040336665u;
        }
        check("Native immediate switch floors widths to byte count, accepting8..15/16..23/32..39/64..71 with little-endian truncation",
            widths && NativeGeneratedImmediateBytes.Project(0, uint.MaxValue).NativeDiagnosticCode == 1040336665u);
    }
}
