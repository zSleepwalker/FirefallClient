namespace Firefall.Reconstruction.Startup;

public enum SingletonTeardownCaptureKind
{
    Missing,
    Unreached,
    Captured
}

public readonly struct SingletonTeardownCapture<T> where T : struct
{
    private SingletonTeardownCapture(SingletonTeardownCaptureKind kind, T? value)
    {
        Kind = kind;
        Value = value;
    }

    public SingletonTeardownCaptureKind Kind { get; }
    public T? Value { get; }

    public static SingletonTeardownCapture<T> Missing() =>
        new(SingletonTeardownCaptureKind.Missing, null);

    public static SingletonTeardownCapture<T> Unreached() =>
        new(SingletonTeardownCaptureKind.Unreached, null);

    public static SingletonTeardownCapture<T> Captured(T value) =>
        new(SingletonTeardownCaptureKind.Captured, value);
}

public readonly record struct SingletonTeardownWalkerCaptures(
    SingletonTeardownCapture<byte> DiagnosticGateByte,
    SingletonTeardownCapture<uint> InitialHead,
    SingletonTeardownCapture<uint> LaterHead);

public enum SingletonTeardownWalkerCaptureError
{
    None,
    MissingDiagnosticGateByte,
    UnreachedDiagnosticGateByte,
    MissingInitialHead,
    UnreachedInitialHead,
    MissingLaterHead,
    UnreachedLaterHead,
    LaterHeadWasReachedWithNullInitialHead
}

public enum SingletonTeardownWalkerBoundary
{
    NullHeadPath,
    RepeatBoundary
}

public readonly record struct SingletonTeardownWalkerWrite(
    uint CodeRawFileOffset,
    uint Address,
    byte Value);

public readonly record struct SingletonTeardownWalkerDispatch(
    uint CodeRawFileOffset,
    uint Receiver,
    uint VirtualSlotOffset,
    uint ExplicitArgumentCount);

public readonly record struct SingletonTeardownWalkerProjection(
    bool DiagnosticReported,
    byte DiagnosticGateByte,
    uint InitialHead,
    uint? LaterHead,
    IReadOnlyList<SingletonTeardownWalkerWrite> OrderedWrites,
    SingletonTeardownWalkerDispatch? Dispatch,
    SingletonTeardownWalkerBoundary Boundary);

public readonly record struct SingletonTeardownWalkerProjectionResult(
    SingletonTeardownWalkerProjection? Projection,
    SingletonTeardownWalkerCaptureError Error)
{
    public bool Succeeded => Projection.HasValue;
}

public static class SingletonTeardownWalkerProjectionRules
{
    public const uint EntryVa = 0x016C46F0;
    public const uint EntryRawFileOffset = 0x012C3AF0;
    public const uint RawCodeStartFileOffset = 0x012C3AF0;
    public const uint RawCodeEndFileOffsetInclusive = 0x012C3B3E;
    public const uint ObservedVaToRawFileOffsetDelta = 0x00400C00;

    public const uint DiagnosticGateAddress = 0x01E4B096;
    public const uint DiagnosticReportId = 0x1C;
    public const uint DiagnosticCallCellAddress = 0x01C15EA0;
    public const uint DiagnosticCallRawFileOffset = 0x012C3B07;
    public const uint InitialHeadLoadRawFileOffset = 0x012C3B10;
    public const uint HeadGlobalAddress = 0x01E4B0A8;
    public const uint DiagnosticGateClearRawFileOffset = 0x012C3B16;
    public const uint DiagnosticGateClearAddress = 0x01E4B096;
    public const uint GuardAddress = 0x01E4B095;
    public const uint GuardSetRawFileOffset = 0x012C3B1D;
    public const uint NullHeadGuardClearRawFileOffset = 0x012C3B37;
    public const uint VirtualPointerLoadRawFileOffset = 0x012C3B28;
    public const uint VirtualDispatchRawFileOffset = 0x012C3B2A;
    public const uint VirtualSlotOffset = 8;
    public const uint MaximumCapturedDispatchCount = 1;
    public const uint NextLinkReadCount = 0;
    public const uint HeadReloadRawFileOffset = 0x012C3B2D;
    public const uint RepeatCheckRawFileOffset = 0x012C3B33;
    public const uint RepeatBranchRawFileOffset = 0x012C3B35;
    public const uint RawCodeReturnRawFileOffset = 0x012C3B3E;

    public static IReadOnlyList<byte> RawWalkerInstructionBytes { get; } =
        Array.AsReadOnly(new byte[]
        {
            0x80, 0x3D, 0x96, 0xB0, 0xE4, 0x01, 0x00, 0x75, 0x17, 0x6A, 0x00, 0x6A, 0x1C,
            0x68, 0xF3, 0x3D, 0xB7, 0x01, 0x68, 0xDB, 0x3D, 0xB7, 0x01, 0xFF, 0x15, 0xA0,
            0x5E, 0xC1, 0x01, 0x83, 0xC4, 0x10, 0x8B, 0x0D, 0xA8, 0xB0, 0xE4, 0x01, 0xC6,
            0x05, 0x96, 0xB0, 0xE4, 0x01, 0x00, 0xC6, 0x05, 0x95, 0xB0, 0xE4, 0x01, 0x01,
            0x85, 0xC9, 0x74, 0x0F, 0x8B, 0x01, 0xFF, 0x50, 0x08, 0x8B, 0x0D, 0xA8, 0xB0,
            0xE4, 0x01, 0x85, 0xC9, 0x75, 0xF1, 0xC6, 0x05, 0x95, 0xB0, 0xE4, 0x01, 0x00,
            0xC3
        });

    public static SingletonTeardownWalkerProjectionResult Project(
        SingletonTeardownWalkerCaptures captures)
    {
        if (captures.DiagnosticGateByte.Kind != SingletonTeardownCaptureKind.Captured)
        {
            return Failure(captures.DiagnosticGateByte.Kind switch
            {
                SingletonTeardownCaptureKind.Missing => SingletonTeardownWalkerCaptureError.MissingDiagnosticGateByte,
                SingletonTeardownCaptureKind.Unreached => SingletonTeardownWalkerCaptureError.UnreachedDiagnosticGateByte,
                _ => SingletonTeardownWalkerCaptureError.MissingDiagnosticGateByte
            });
        }

        if (captures.InitialHead.Kind != SingletonTeardownCaptureKind.Captured)
        {
            return Failure(captures.InitialHead.Kind switch
            {
                SingletonTeardownCaptureKind.Missing => SingletonTeardownWalkerCaptureError.MissingInitialHead,
                SingletonTeardownCaptureKind.Unreached => SingletonTeardownWalkerCaptureError.UnreachedInitialHead,
                _ => SingletonTeardownWalkerCaptureError.MissingInitialHead
            });
        }

        byte gateByte = captures.DiagnosticGateByte.Value!.Value;
        uint initialHead = captures.InitialHead.Value!.Value;
        bool diagnosticReported = gateByte == 0;
        var writes = new List<SingletonTeardownWalkerWrite>
        {
            new(DiagnosticGateClearRawFileOffset, DiagnosticGateClearAddress, 0),
            new(GuardSetRawFileOffset, GuardAddress, 1)
        };

        if (initialHead == 0)
        {
            if (captures.LaterHead.Kind == SingletonTeardownCaptureKind.Missing)
            {
                return Failure(SingletonTeardownWalkerCaptureError.MissingLaterHead);
            }

            if (captures.LaterHead.Kind != SingletonTeardownCaptureKind.Unreached)
            {
                return Failure(SingletonTeardownWalkerCaptureError.LaterHeadWasReachedWithNullInitialHead);
            }

            writes.Add(new SingletonTeardownWalkerWrite(NullHeadGuardClearRawFileOffset, GuardAddress, 0));
            return Success(new SingletonTeardownWalkerProjection(
                diagnosticReported,
                gateByte,
                initialHead,
                null,
                Array.AsReadOnly(writes.ToArray()),
                null,
                SingletonTeardownWalkerBoundary.NullHeadPath));
        }

        if (captures.LaterHead.Kind != SingletonTeardownCaptureKind.Captured)
        {
            return Failure(captures.LaterHead.Kind switch
            {
                SingletonTeardownCaptureKind.Missing => SingletonTeardownWalkerCaptureError.MissingLaterHead,
                SingletonTeardownCaptureKind.Unreached => SingletonTeardownWalkerCaptureError.UnreachedLaterHead,
                _ => SingletonTeardownWalkerCaptureError.MissingLaterHead
            });
        }

        uint laterHead = captures.LaterHead.Value!.Value;
        var dispatch = new SingletonTeardownWalkerDispatch(
            VirtualDispatchRawFileOffset,
            initialHead,
            VirtualSlotOffset,
            0);

        if (laterHead == 0)
        {
            writes.Add(new SingletonTeardownWalkerWrite(NullHeadGuardClearRawFileOffset, GuardAddress, 0));
            return Success(new SingletonTeardownWalkerProjection(
                diagnosticReported,
                gateByte,
                initialHead,
                laterHead,
                Array.AsReadOnly(writes.ToArray()),
                dispatch,
                SingletonTeardownWalkerBoundary.NullHeadPath));
        }

        return Success(new SingletonTeardownWalkerProjection(
            diagnosticReported,
            gateByte,
            initialHead,
            laterHead,
            Array.AsReadOnly(writes.ToArray()),
            dispatch,
            SingletonTeardownWalkerBoundary.RepeatBoundary));
    }

    private static SingletonTeardownWalkerProjectionResult Failure(
        SingletonTeardownWalkerCaptureError error) =>
        new(null, error);

    private static SingletonTeardownWalkerProjectionResult Success(
        SingletonTeardownWalkerProjection projection) =>
        new(projection, SingletonTeardownWalkerCaptureError.None);
}
