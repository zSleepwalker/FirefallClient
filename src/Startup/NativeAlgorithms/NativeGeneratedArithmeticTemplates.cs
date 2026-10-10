namespace Firefall.Reconstruction.Startup;

/// <summary>ADD/SUB/XOR candidates for default-header mode0 BL/EBX or explicit mode1 BL/RBX body forms.</summary>
public static class NativeGeneratedArithmeticTemplates
{
    public static IReadOnlyList<ReadOnlyMemory<byte>> ImmediateFromConsumedParitySample(
        NativeGeneratedOperationCode operation, uint paritySample, uint value)
    {
        byte group = BaseOpcode(operation);
        var scratch = NativeGeneratedBodyDecisionRules.ScratchFromConsumedParitySample(paritySample);
        ReadOnlyMemory<byte> immediate = NativeGeneratedImmediateBytes.Project(value, scratch.Width).Bytes
            ?? throw new InvalidOperationException("Supported scratch width must resolve immediate bytes.");
        byte[] candidate = new byte[2 + immediate.Length];
        candidate[0] = scratch.Width == 8 ? (byte)0x80 : (byte)0x81;
        candidate[1] = (byte)(0xC3 | group);
        immediate.Span.CopyTo(candidate.AsSpan(2));
        return Array.AsReadOnly<ReadOnlyMemory<byte>>([candidate]);
    }

    public static IReadOnlyList<ReadOnlyMemory<byte>> RegisterSource(
        NativeGeneratedOperationCode operation, NativeGeneratedRegisterSourceChoice source)
    {
        byte baseOpcode = BaseOpcode(operation);
        bool narrow = source.Scratch == new NativeGeneratedScratchChoice(4, 8);
        bool wide = source.Scratch == new NativeGeneratedScratchChoice(40, 32);
        if ((!narrow && !wide)
            || (narrow && source.SourceDescriptorCode is not (2 or 3))
            || (wide && source.SourceDescriptorCode is not (38 or 39)))
            throw new ArgumentOutOfRangeException(nameof(source), "Only recovered BL/EBX with CL/ECX or DL/EDX sources are supported.");

        uint index = source.SourceDescriptorCode - (narrow ? 1u : 37u);
        byte firstOpcode = (byte)(baseOpcode + (wide ? 1 : 0));
        byte secondOpcode = (byte)(baseOpcode + (wide ? 3 : 2));
        return Array.AsReadOnly<ReadOnlyMemory<byte>>(
            [new byte[] { firstOpcode, (byte)(0xC3 | (index << 3)) },
             new byte[] { secondOpcode, (byte)(0xD8 | index) }]);
    }

    public static IReadOnlyList<ReadOnlyMemory<byte>> TableSource(
        NativeGeneratedOperationCode operation, NativeGeneratedTableSourceChoice source)
    {
        byte baseOpcode = BaseOpcode(operation);
        if (source.Width is not (8 or 32) || source.BaseDescriptorCode != 44
            || source.Scale is not (1 or 2 or 4 or 8) || source.BaseOffset >= 256
            || source.IndexValue >= 256 / source.Scale
            || source.EffectiveTableOffset >= 256
            || (source.Scale == 1 && (source.IndexDescriptorCode != 0 || source.IndexValue != 0))
            || (source.Scale != 1 && source.IndexDescriptorCode != 43))
            throw new ArgumentOutOfRangeException(nameof(source), "Only recovered bounded EDI/(ESI*scale+EDI) table operands are supported.");

        byte opcode = (byte)(baseOpcode + (source.Width == 8 ? 2 : 3));
        byte[] candidate;
        if (source.Scale == 1)
            candidate = [opcode, 0x1F];
        else
        {
            byte scaleBits = source.Scale switch { 2 => 1, 4 => 2, 8 => 3, _ => throw new InvalidOperationException() };
            candidate = [opcode, 0x1C, (byte)((scaleBits << 6) | 0x37)];
        }
        return Array.AsReadOnly<ReadOnlyMemory<byte>>([candidate]);
    }

    public static IReadOnlyList<ReadOnlyMemory<byte>> ImmediateForModeOneFromConsumedParitySample(
        NativeGeneratedOperationCode operation, uint paritySample, uint value)
    {
        var candidates = ImmediateFromConsumedParitySample(operation, paritySample, value);
        return (paritySample & 1) != 0 ? candidates : WithRexW(candidates);
    }

    public static IReadOnlyList<ReadOnlyMemory<byte>> RegisterSourceForModeOne(
        NativeGeneratedOperationCode operation, NativeGeneratedRegisterSourceChoice source)
    {
        if (source.Scratch == new NativeGeneratedScratchChoice(4, 8))
            return RegisterSource(operation, source);
        if (source.Scratch != new NativeGeneratedScratchChoice(56, 64) || source.SourceDescriptorCode is not (54 or 55))
            throw new ArgumentOutOfRangeException(nameof(source), "Only recovered BL/RBX with CL/RCX or DL/RDX sources are supported.");
        return WithRexW(RegisterSource(operation, source with
        {
            Scratch = new(40, 32),
            SourceDescriptorCode = source.SourceDescriptorCode - 16
        }));
    }

    public static IReadOnlyList<ReadOnlyMemory<byte>> TableSourceForModeOne(
        NativeGeneratedOperationCode operation, NativeGeneratedTableSourceChoice source)
    {
        if (source.Width is not (8 or 64) || source.BaseDescriptorCode != 60
            || (source.IndexDescriptorCode != 0 && source.IndexDescriptorCode != 59))
            throw new ArgumentOutOfRangeException(nameof(source), "Only recovered RDI/(RSI*scale+RDI) table sources are supported.");
        var candidates = TableSource(operation, source with
        {
            Width = source.Width == 8 ? (byte)8 : (byte)32,
            BaseDescriptorCode = 44,
            IndexDescriptorCode = source.IndexDescriptorCode == 0 ? 0u : 43u
        });
        return source.Width == 8 ? candidates : WithRexW(candidates);
    }

    private static IReadOnlyList<ReadOnlyMemory<byte>> WithRexW(IReadOnlyList<ReadOnlyMemory<byte>> candidates)
    {
        var prefixed = new List<ReadOnlyMemory<byte>>(candidates.Count);
        foreach (var candidate in candidates)
        {
            byte[] bytes = new byte[1 + candidate.Length];
            bytes[0] = 0x48;
            candidate.Span.CopyTo(bytes.AsSpan(1));
            prefixed.Add(bytes);
        }
        return prefixed.AsReadOnly();
    }

    private static byte BaseOpcode(NativeGeneratedOperationCode operation) => operation switch
    {
        NativeGeneratedOperationCode.Add => 0x00,
        NativeGeneratedOperationCode.Subtract => 0x28,
        NativeGeneratedOperationCode.Xor => 0x30,
        _ => throw new ArgumentOutOfRangeException(nameof(operation), "Only recovered ADD/SUB/XOR templates are supported.")
    };
}
