namespace Firefall.Reconstruction.Startup;

public enum NativeGeneratedByteSourceKind
{
    None,
    ImmediateParameter,
    RemainingByteCount,
    RelativeTableByte
}

/// <summary>Logical low-byte effect of a recovered body operation, not an instruction pointer or executable descriptor.</summary>
public readonly record struct NativeGeneratedByteStep(
    NativeGeneratedOperationCode Operation, NativeGeneratedByteSourceKind SourceKind,
    uint Parameter = 0, uint? TableOffset = null);
