namespace Firefall.Reconstruction.Startup;

public sealed record NativeGeneratedDefaultHeaderSnapshot(
    NativeGeneratedProviderPrefixSnapshot Prefix,
    uint CapturedTableAddress,
    IReadOnlyList<ReadOnlyMemory<byte>> ForwardBodyFragments,
    IReadOnlyList<ReadOnlyMemory<byte>> InverseBodyFragments,
    IReadOnlyList<NativeGeneratedByteStep> ForwardByteSteps,
    IReadOnlyList<NativeGeneratedByteStep> InverseByteSteps,
    NativeGeneratedWrappedBody Forward,
    NativeGeneratedWrappedBody Inverse,
    int UInt32CallsBeforeWrappers,
    IReadOnlyList<uint> ConsumedUInt32Samples)
{
    public uint ModeWordAt4DuringGeneration { get; init; }
    public byte ControlByteAt18DuringGeneration { get; init; } = 1;
    public uint DiscardedUInt32Calls { get; init; }
}

/// <summary>Offline default-header images for explicitly captured mode0/1 and control byte. Constructor facts stay separate; no native allocation or execution.</summary>
public static class NativeGeneratedDefaultHeaderImage
{
    public const uint GeneratorFileOffset = 0x1286F00;
    public const uint DispatcherFileOffset = 0x12866C0;

    public static NativeGeneratedDefaultHeaderSnapshot FromSeedCaptured(
        uint seed, uint capturedTableAddress, uint capturedModeWordAt4, byte capturedControlByteAt18 = 1)
    {
        var rng = new NativeMt19937(seed);
        return GenerateCaptured(rng.NextUInt32, capturedTableAddress, capturedModeWordAt4, capturedControlByteAt18);
    }

    public static NativeGeneratedDefaultHeaderSnapshot GenerateCaptured(
        Func<uint> nextUInt32, uint capturedTableAddress, uint capturedModeWordAt4, byte capturedControlByteAt18 = 1)
    {
        ArgumentNullException.ThrowIfNull(nextUInt32);
        if (capturedModeWordAt4 is not (0 or 1))
            throw new ArgumentOutOfRangeException(nameof(capturedModeWordAt4), "Only recovered default-header mode0/1 images are supported.");
        bool modeOne = capturedModeWordAt4 == 1;
        var samples = new List<uint>();
        uint discardedCalls = 0;
        uint Draw()
        {
            uint sample = nextUInt32();
            samples.Add(sample);
            return sample;
        }
        ReadOnlyMemory<byte> Select(IReadOnlyList<ReadOnlyMemory<byte>> candidates, bool advanceAfterCandidate = false)
        {
            if (candidates.Count == 0)
                throw new InvalidOperationException($"Native candidate builder would diagnose {NativeGeneratedConditionalBranches.NoCandidatesDiagnostic}.");
            ReadOnlyMemory<byte> candidate = candidates[(int)(Draw() % (uint)candidates.Count)];
            if (advanceAfterCandidate)
                discardedCalls += NativeGeneratedCandidateRngAdvance.Consume(capturedControlByteAt18, candidate.Span, Draw);
            return candidate;
        }
        IReadOnlyList<ReadOnlyMemory<byte>> Move(uint code, ulong value, byte sourceWidth) =>
            modeOne ? NativeGeneratedImmediateMoveTemplates.ForModeOneOrdinaryRegister(code, value, sourceWidth)
                : NativeGeneratedImmediateMoveTemplates.ForRegister(code, unchecked((uint)value));
        void Unary(List<ReadOnlyMemory<byte>> fragments, NativeGeneratedOperationCode operation)
        {
            uint parity = Draw();
            fragments.Add(Select(modeOne
                ? NativeGeneratedScratchTemplates.UnaryForModeOneFromConsumedParitySample(operation, parity)
                : NativeGeneratedScratchTemplates.UnaryFromConsumedParitySample(operation, parity), true));
        }
        void Rotate(List<ReadOnlyMemory<byte>> fragments, NativeGeneratedOperationCode operation, byte count, bool inverse)
        {
            if (!NativeGeneratedBodyDecisionRules.RotateUsesCountRegisterFromConsumedSample(Draw()))
            {
                fragments.Add(Select(NativeGeneratedScratchTemplates.RotateImmediate(operation, count), true));
                return;
            }
            uint parity = Draw();
            var register = modeOne ? NativeGeneratedBodyDecisionRules.RotateCountRegisterForModeOneFromConsumedParitySample(parity)
                : NativeGeneratedBodyDecisionRules.RotateCountRegisterFromConsumedParitySample(parity);
            ReadOnlyMemory<byte> move = Select(Move(register.DescriptorCode, count, register.Width == 8 ? (byte)8 : (byte)32), true);
            ReadOnlyMemory<byte> rotate = Select(NativeGeneratedScratchTemplates.RotateViaCl(operation), true);
            fragments.Add(inverse ? rotate : move);
            fragments.Add(inverse ? move : rotate);
        }
        void Arithmetic(List<ReadOnlyMemory<byte>> fragments, NativeGeneratedOperationCode operation,
            uint form, uint parameter, bool inverse)
        {
            if (form == 0)
            {
                uint parity = Draw();
                fragments.Add(Select(modeOne
                    ? NativeGeneratedArithmeticTemplates.ImmediateForModeOneFromConsumedParitySample(operation, parity, parameter)
                    : NativeGeneratedArithmeticTemplates.ImmediateFromConsumedParitySample(operation, parity, parameter)));
                return;
            }
            var group = new List<ReadOnlyMemory<byte>>(3);
            if (form == 1)
            {
                uint parity = Draw();
                var source = modeOne ? NativeGeneratedBodyDecisionRules.ArithmeticRegisterSourceForModeOne(parameter, parity)
                    : NativeGeneratedBodyDecisionRules.ArithmeticRegisterSource(parameter, parity);
                if (source.RequiresImmediateMove)
                    group.Add(Select(Move(source.SourceDescriptorCode, parameter, source.Scratch.Width == 8 ? (byte)8 : (byte)32), true));
                group.Add(Select(modeOne ? NativeGeneratedArithmeticTemplates.RegisterSourceForModeOne(operation, source)
                    : NativeGeneratedArithmeticTemplates.RegisterSource(operation, source), true));
            }
            else if (form == 2)
            {
                var source = modeOne ? NativeGeneratedBodyDecisionRules.ArithmeticTableSourceForModeOne(parameter, capturedTableAddress)
                    : NativeGeneratedBodyDecisionRules.ArithmeticTableSource(parameter, capturedTableAddress);
                if (source.IndexDescriptorCode != 0)
                    group.Add(Select(Move(source.IndexDescriptorCode, source.IndexValue, 32), true));
                group.Add(Select(Move(source.BaseDescriptorCode,
                    modeOne ? source.SignedDwordBaseAddressBits : source.BaseAddress, modeOne ? (byte)64 : (byte)32)));
                group.Add(Select(modeOne ? NativeGeneratedArithmeticTemplates.TableSourceForModeOne(operation, source)
                    : NativeGeneratedArithmeticTemplates.TableSource(operation, source), true));
            }
            else
                throw new ArgumentOutOfRangeException(nameof(form), "Native arithmetic form is a modulo3 result.");
            if (inverse)
                group.Reverse();
            fragments.AddRange(group);
        }
        static byte[] Flatten(IReadOnlyList<ReadOnlyMemory<byte>> fragments)
        {
            int length = 0;
            foreach (var fragment in fragments)
                length = checked(length + fragment.Length);
            byte[] bytes = new byte[length];
            int offset = 0;
            foreach (var fragment in fragments)
            {
                fragment.Span.CopyTo(bytes.AsSpan(offset));
                offset += fragment.Length;
            }
            return bytes;
        }

        var prefix = NativeGeneratedTransformProviderPrefix.Initialize(Draw);
        var forward = new List<ReadOnlyMemory<byte>>();
        var inverse = new List<ReadOnlyMemory<byte>>();
        var forwardSteps = new List<NativeGeneratedByteStep>();
        var inverseSteps = new List<NativeGeneratedByteStep>();
        IReadOnlyList<ReadOnlyMemory<byte>> loadScratch = [new byte[] { 0x8A, 0x18 }];
        forward.Add(Select(loadScratch));
        for (uint iteration = 0; iteration < prefix.IterationCount; iteration++)
        {
            var pair = NativeGeneratedBodyDecisionRules.PairFromConsumedSample(Draw());
            if (pair.Forward is NativeGeneratedOperationCode.Increment or NativeGeneratedOperationCode.Decrement)
            {
                forwardSteps.Add(new(pair.Forward, NativeGeneratedByteSourceKind.None));
                inverseSteps.Add(new(pair.Reverse, NativeGeneratedByteSourceKind.None));
                Unary(forward, pair.Forward);
                Unary(inverse, pair.Reverse);
            }
            else if (pair.Forward is NativeGeneratedOperationCode.RotateLeft or NativeGeneratedOperationCode.RotateRight)
            {
                byte count = unchecked((byte)Draw());
                forwardSteps.Add(new(pair.Forward, NativeGeneratedByteSourceKind.ImmediateParameter, count));
                inverseSteps.Add(new(pair.Reverse, NativeGeneratedByteSourceKind.ImmediateParameter, count));
                Rotate(forward, pair.Forward, count, false);
                Rotate(inverse, pair.Reverse, count, true);
            }
            else
            {
                uint form = Draw() % 3;
                uint parameter = Draw();
                var sourceKind = form == 2 ? NativeGeneratedByteSourceKind.RelativeTableByte
                    : form == 1 && (parameter & 1) != 0 ? NativeGeneratedByteSourceKind.RemainingByteCount
                    : NativeGeneratedByteSourceKind.ImmediateParameter;
                uint? tableOffset = form == 2
                    ? NativeGeneratedBodyDecisionRules.ArithmeticTableSource(parameter, capturedTableAddress).EffectiveTableOffset
                    : null;
                forwardSteps.Add(new(pair.Forward, sourceKind, parameter, tableOffset));
                inverseSteps.Add(new(pair.Reverse, sourceKind, parameter, tableOffset));
                Arithmetic(forward, pair.Forward, form, parameter, false);
                Arithmetic(inverse, pair.Reverse, form, parameter, true);
            }
        }
        inverse.Add(Select(loadScratch));
        inverse.Reverse();
        inverseSteps.Reverse();
        int beforeWrappers = samples.Count;
        var forwardWrapped = modeOne ? NativeGeneratedByteLoopWrapper.AssembleCapturedBodyForModeOne(Flatten(forward), Draw)
            : NativeGeneratedByteLoopWrapper.AssembleCapturedBody(Flatten(forward), Draw);
        var inverseWrapped = modeOne ? NativeGeneratedByteLoopWrapper.AssembleCapturedBodyForModeOne(Flatten(inverse), Draw)
            : NativeGeneratedByteLoopWrapper.AssembleCapturedBody(Flatten(inverse), Draw);
        return new(prefix, capturedTableAddress, forward.AsReadOnly(), inverse.AsReadOnly(),
            forwardSteps.AsReadOnly(), inverseSteps.AsReadOnly(),
            forwardWrapped, inverseWrapped, beforeWrappers, samples.AsReadOnly())
        {
            ModeWordAt4DuringGeneration = capturedModeWordAt4,
            ControlByteAt18DuringGeneration = capturedControlByteAt18,
            DiscardedUInt32Calls = discardedCalls
        };
    }
}
