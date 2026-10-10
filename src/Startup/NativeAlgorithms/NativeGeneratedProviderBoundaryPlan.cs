namespace Firefall.Reconstruction.Startup;

public enum NativeGeneratedOperationCode : uint
{
    Add = 1,
    Decrement = 5,
    Increment = 6,
    JumpNotZero = 18,
    JumpZero = 22,
    Leave = 25,
    Move = 26,
    Or = 27,
    Pop = 28,
    Push = 29,
    Return = 33,
    RotateLeft = 34,
    RotateRight = 35,
    Subtract = 41,
    Xor = 43
}

public enum NativeGeneratedRegisterCode32 : uint
{
    Eax = 37, Ecx = 38, Edx = 39, Ebx = 40, Esp = 41, Ebp = 42, Esi = 43, Edi = 44
}

public enum NativeGeneratedRegisterCode64 : uint
{
    Rax = 53, Rcx = 54, Rdx = 55, Rbx = 56, Rsp = 57, Rbp = 58, Rsi = 59, Rdi = 60
}

public readonly record struct NativeGeneratedBoundaryStep(
    uint OperationCode, uint? FirstDescriptorCode = null, uint? SecondDescriptorCode = null,
    uint? SecondOperandOffset = null, uint? ImmediateValue = null, uint? ImmediateWidth = null);

public sealed record NativeGeneratedBoundaryPlan(
    ReadOnlyMemory<NativeGeneratedBoundaryStep> Opening, ReadOnlyMemory<NativeGeneratedBoundaryStep> Closing);

/// <summary>Abstract selector-call schedules for provider DWORD+4 modes0/1; not an instruction encoder or executable buffer.</summary>
public static class NativeGeneratedProviderBoundaryPlanner
{
    public const uint OpeningMethodFileOffset = 0x1285290;
    public const uint ClosingMethodFileOffset = 0x1285970;
    public const uint UnsupportedModeDiagnostic = 0x3E377377;

    public static NativeGeneratedBoundaryPlan ForModeZero(byte headerByte0 = 0, byte headerByte1 = 2)
    {
        NativeGeneratedBoundaryStep[] opening =
        [
            new(29, 42),
            new(26, 42, 41),
            new(29, 37),
            new(29, 38),
            new(29, 39),
            new(29, 40),
            new(29, 43),
            new(29, 44),
            new(26, 37u + headerByte0, 42, 8),
            new(26, 37u + headerByte1, 42, 12)
        ];
        NativeGeneratedBoundaryStep[] closing =
        [
            new(28, 44),
            new(28, 43),
            new(28, 40),
            new(28, 39),
            new(28, 38),
            new(28, 37),
            new(25),
            new(33, ImmediateValue: 8, ImmediateWidth: 16)
        ];
        return new(opening, closing);
    }

    public static NativeGeneratedBoundaryPlan ForModeOne(byte headerByte0 = 0, byte headerByte1 = 2)
    {
        NativeGeneratedBoundaryStep[] opening =
        [
            new(29, 53),
            new(29, 54),
            new(29, 55),
            new(29, 56),
            new(29, 59),
            new(29, 60),
            new(26, 53u + headerByte0, 57, 32),
            new(26, 53u + headerByte1, 57, 24)
        ];
        NativeGeneratedBoundaryStep[] closing =
        [
            new(28, 60),
            new(28, 59),
            new(28, 56),
            new(28, 55),
            new(28, 54),
            new(28, 53),
            new(33)
        ];
        return new(opening, closing);
    }
}
