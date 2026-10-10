using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public sealed record NativeGeneratedBranchCandidates(
    IReadOnlyList<ReadOnlyMemory<byte>> Candidates, uint? NativeDiagnosticCode);

/// <summary>JZ/JNZ with kind3 first operand and absent second/third operands. Preserves native range gates, including short-offset quirks.</summary>
public static class NativeGeneratedConditionalBranches
{
    public const uint NoCandidatesDiagnostic = 1040188017;

    public static NativeGeneratedBranchCandidates ProjectShapedBranch(NativeGeneratedOperationCode operation, int offset)
    {
        byte shortOpcode = operation switch
        {
            NativeGeneratedOperationCode.JumpNotZero => 0x75,
            NativeGeneratedOperationCode.JumpZero => 0x74,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), "Only recovered JZ/JNZ templates are supported.")
        };
        var candidates = new List<ReadOnlyMemory<byte>>(2);
        if ((unchecked((uint)offset) & 0xFFFFFF00u) == 0 && offset is not (128 or 129))
        {
            int displacement = offset < 0 ? unchecked(offset - 2) : offset;
            candidates.Add(new byte[] { shortOpcode, unchecked((byte)displacement) });
        }
        if (unchecked((uint)offset + 0x80000000u) > 5)
        {
            byte[] bytes = [0x0F, (byte)(shortOpcode + 0x10), 0, 0, 0, 0];
            int displacement = offset < 0 ? unchecked(offset - 6) : offset;
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(2), displacement);
            candidates.Add(bytes);
        }
        return new(candidates.AsReadOnly(), candidates.Count == 0 ? NoCandidatesDiagnostic : null);
    }
}
