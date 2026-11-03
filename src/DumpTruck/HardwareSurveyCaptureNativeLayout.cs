namespace Firefall.Reconstruction.DumpTruck;

public enum HardwareSurveyCaptureAction
{
    ReturnFailure,
    PollPipe,
    ReadPipe,
    SleepThenQueryExitCode,
    ForwardBytesThenPoll,
    AppendNulAndReturn
}

public readonly record struct HardwareSurveyCaptureDecision(
    HardwareSurveyCaptureAction Action,
    uint ReadRequestByteCount,
    uint BytesToForward,
    uint? ReturnCode,
    bool AppendTerminatingNul,
    bool EntersCommonCleanup);

/// <summary>Pipe capture facts and branch decisions from DumpTruck PE VAs.</summary>
public static class HardwareSurveyCaptureNativeLayout
{
    public const uint PipeAttributesInheritableVa = 0x42AC32;
    public const uint CreatePipeCallVa = 0x42AC50;
    public const uint CreateProcessCallVa = 0x42ACA0;
    public const uint InitialExitCodeQueryVa = 0x42ACEC;
    public const uint FirstPeekNamedPipeCallVa = 0x42AD1B;
    public const uint ReadFileCallVa = 0x42AD45;
    public const uint SecondPeekNamedPipeCallVa = 0x42AD78;
    public const uint PollSleepCallVa = 0x42AD8D;
    public const uint FollowupExitCodeQueryVa = 0x42ADA0;
    public const uint AppendNulCallVa = 0x42ADCE;
    public const uint CloseReadPipeCallVa = 0x42ADE8;
    public const uint CloseWritePipeCallVa = 0x42AE00;
    public const uint CloseThreadHandleCallVa = 0x42AE18;
    public const uint CloseProcessHandleCallVa = 0x42AE30;

    public const uint PipeRequestedByteCount = 0x800;
    public const uint ReadRequestByteCount = 0x800;
    public const uint StillActiveExitCode = 259;
    public const uint PollSleepMilliseconds = 1;
    public const uint SinkVtableSlotOffset = 0x18;
    public const uint FailureReturnCode = uint.MaxValue;

    public const bool PipeSecurityAttributeInheritHandle = true;
    public const bool CreateProcessInheritHandles = true;
    public const uint StartupUseStandardHandlesFlag = 0x100;

    public static HardwareSurveyCaptureDecision CaptureStartup(
        bool createPipeSucceeded,
        bool createProcessSucceeded,
        bool initialExitCodeQuerySucceeded)
    {
        if (!createPipeSucceeded)
            return new(HardwareSurveyCaptureAction.ReturnFailure, 0, 0, FailureReturnCode, false, false);

        if (!createProcessSucceeded)
            return new(HardwareSurveyCaptureAction.ReturnFailure, 0, 0, FailureReturnCode, false, true);

        if (!initialExitCodeQuerySucceeded)
            return new(HardwareSurveyCaptureAction.AppendNulAndReturn, 0, 0, FailureReturnCode, true, true);

        return new(HardwareSurveyCaptureAction.PollPipe, 0, 0, null, false, false);
    }

    public static HardwareSurveyCaptureDecision CapturePipePoll(
        bool peekNamedPipeSucceeded,
        uint bytesAvailable,
        uint childExitCode)
    {
        if (peekNamedPipeSucceeded && bytesAvailable != 0)
            return new(HardwareSurveyCaptureAction.ReadPipe, ReadRequestByteCount, 0, null, false, false);

        if (childExitCode != StillActiveExitCode)
            return new(HardwareSurveyCaptureAction.AppendNulAndReturn, 0, 0, childExitCode, true, true);

        return new(HardwareSurveyCaptureAction.SleepThenQueryExitCode, 0, 0, null, false, false);
    }

    public static HardwareSurveyCaptureDecision CaptureReadResult(bool readFileSucceeded, uint bytesRead) =>
        readFileSucceeded
            ? new(HardwareSurveyCaptureAction.ForwardBytesThenPoll, 0, bytesRead, null, false, false)
            : new(HardwareSurveyCaptureAction.ReturnFailure, 0, 0, FailureReturnCode, false, true);

    public static HardwareSurveyCaptureDecision CaptureFollowupExitCodeQuery(bool querySucceeded) =>
        querySucceeded
            ? new(HardwareSurveyCaptureAction.PollPipe, 0, 0, null, false, false)
            : new(HardwareSurveyCaptureAction.AppendNulAndReturn, 0, 0, FailureReturnCode, true, true);
}
