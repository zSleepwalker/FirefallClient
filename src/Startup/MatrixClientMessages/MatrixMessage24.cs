using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Native msg-24 application buffer: instance context first, compact message code
/// second, no additional body. This is not the incoming server-message header order.
/// </summary>
public sealed record MatrixMessage24(ulong InstanceContext)
{
    public const ushort MessageCode = 24;
    public const int EncodedSize = 9;
    public const int TransportChannel = 2;
    public const uint ConstructorFileOffset = 0x822CA0;
    public const uint SenderFileOffset = 0x499840;
    public const uint EncoderFileOffset = 0x49A520;

    public byte[] Encode()
    {
        var bytes = new byte[EncodedSize];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, InstanceContext);
        CompactPositiveUInt16Codec.Encode(MessageCode).CopyTo(bytes.AsSpan(8));
        return bytes;
    }
}

public enum MatrixMessage24DispatchOutcome
{
    MissingTransport,
    ConnectionStateNotPermitted,
    ReadyForSubmission
}

public sealed record MatrixMessage24DispatchDecision(
    MatrixMessage24DispatchOutcome Outcome, ulong? TimestampAtE0,
    bool SelectsDiagnosticLog, MatrixMessage24? Message);

/// <summary>Gate selection and descriptor stamping only; no socket writes or delivery claim.</summary>
public static class MatrixMessage24NativeLayout
{
    public const uint GateFileOffset = 0x498350;
    public const int TransportPointerOffset = 0x820;
    public const int ConnectionStateOffset = 0xC;
    public const int TimestampOffset = 0xE0;
    public const int InstanceContextOffset = 0x878;
    public const int DescriptorContextOffset = 0x18;
    public const int DiagnosticFlagsOffset = 0x14;
    public const byte DiagnosticMask = 2;

    public static MatrixMessage24DispatchDecision SelectDispatch(int connectionState,
        bool hasTransport, ulong currentInstanceContext, uint tickMilliseconds, byte diagnosticFlags)
    {
        if (!hasTransport)
            return new(MatrixMessage24DispatchOutcome.MissingTransport, null, false, null);
        if (connectionState <= 4)
            return new(MatrixMessage24DispatchOutcome.ConnectionStateNotPermitted, null, false, null);
        return new(MatrixMessage24DispatchOutcome.ReadyForSubmission, 1000UL * tickMilliseconds,
            (diagnosticFlags & DiagnosticMask) != 0, new(currentInstanceContext));
    }
}
