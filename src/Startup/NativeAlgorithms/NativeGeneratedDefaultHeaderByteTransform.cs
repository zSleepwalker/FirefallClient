namespace Firefall.Reconstruction.Startup;

/// <summary>Managed low-byte algorithm for positive-length buffers. Never invokes or interprets generated machine-code arrays.</summary>
public static class NativeGeneratedDefaultHeaderByteTransform
{
    public static byte[] Forward(ReadOnlySpan<byte> input, NativeGeneratedDefaultHeaderSnapshot snapshot) =>
        Project(input, snapshot, false);

    public static byte[] Inverse(ReadOnlySpan<byte> input, NativeGeneratedDefaultHeaderSnapshot snapshot) =>
        Project(input, snapshot, true);

    private static byte[] Project(ReadOnlySpan<byte> input, NativeGeneratedDefaultHeaderSnapshot snapshot, bool inverse)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.ModeWordAt4DuringGeneration is not (0 or 1))
            throw new ArgumentException("Only recovered default-header mode0/1 byte effects are supported.", nameof(snapshot));
        return ProjectRecoveredSteps(input, snapshot.Prefix, inverse ? snapshot.InverseByteSteps : snapshot.ForwardByteSteps, nameof(snapshot));
    }

    internal static byte[] ProjectRecoveredSteps(ReadOnlySpan<byte> input,
        NativeGeneratedProviderPrefixSnapshot prefix, IReadOnlyList<NativeGeneratedByteStep> steps, string snapshotParameterName)
    {
        if (input.IsEmpty)
            throw new ArgumentException("Only positive-length buffer effects are modeled; zero-count branch quirks require separate control-flow analysis.", nameof(input));
        if (steps.Count != prefix.IterationCount || prefix.TableBytes.Length != NativeGeneratedTransformProviderPrefix.TableLength)
            throw new ArgumentException("Snapshot must contain complete recovered body steps and264-byte table.", snapshotParameterName);
        byte[] output = new byte[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            byte value = input[i];
            uint remaining = (uint)(input.Length - i);
            foreach (var step in steps)
            {
                switch (step.Operation)
                {
                    case NativeGeneratedOperationCode.Increment when step.SourceKind == NativeGeneratedByteSourceKind.None:
                        value = unchecked((byte)(value + 1));
                        break;
                    case NativeGeneratedOperationCode.Decrement when step.SourceKind == NativeGeneratedByteSourceKind.None:
                        value = unchecked((byte)(value - 1));
                        break;
                    case NativeGeneratedOperationCode.RotateLeft when step.SourceKind == NativeGeneratedByteSourceKind.ImmediateParameter:
                        int left = (int)(step.Parameter & 7);
                        value = unchecked((byte)((value << left) | (value >> (8 - left))));
                        break;
                    case NativeGeneratedOperationCode.RotateRight when step.SourceKind == NativeGeneratedByteSourceKind.ImmediateParameter:
                        int right = (int)(step.Parameter & 7);
                        value = unchecked((byte)((value >> right) | (value << (8 - right))));
                        break;
                    case NativeGeneratedOperationCode.Add:
                    case NativeGeneratedOperationCode.Subtract:
                    case NativeGeneratedOperationCode.Xor:
                        byte operand = Operand(step, remaining, prefix.TableBytes.Span);
                        value = step.Operation switch
                        {
                            NativeGeneratedOperationCode.Add => unchecked((byte)(value + operand)),
                            NativeGeneratedOperationCode.Subtract => unchecked((byte)(value - operand)),
                            _ => (byte)(value ^ operand)
                        };
                        break;
                    default:
                        throw new ArgumentException("Snapshot contains an unsupported low-byte operation/source combination.", snapshotParameterName);
                }
            }
            output[i] = value;
        }
        return output;
    }

    private static byte Operand(NativeGeneratedByteStep step, uint remaining, ReadOnlySpan<byte> table) => step.SourceKind switch
    {
        NativeGeneratedByteSourceKind.ImmediateParameter => unchecked((byte)step.Parameter),
        NativeGeneratedByteSourceKind.RemainingByteCount => unchecked((byte)remaining),
        NativeGeneratedByteSourceKind.RelativeTableByte when step.TableOffset is uint offset && offset < 256 => table[(int)offset],
        _ => throw new ArgumentException("Snapshot arithmetic operand is outside the recovered source shapes.", nameof(step))
    };
}
