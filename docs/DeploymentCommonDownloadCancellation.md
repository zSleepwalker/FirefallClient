# DeploymentCommon worker cancellation and cleanup

## Sample and method evidence

Sample: `D:\SteamLibrary\steamapps\common\Firefall\system\bin\DeploymentCommon.dll`,
76,800 bytes, SHA-256
`33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0`.
The managed image was inspected through PE/CLI metadata and IL only; it was
not loaded or executed. MethodDef tokens and body/code RVAs are:

| MethodDef | Body RVA | IL code RVA | Method |
|---|---:|---:|---|
| `0x06000067` | `0x3634` | `0x3640` | `ThreadedFileDownloader::.ctor` |
| `0x0600007C` | `0x38B0` | `0x38BC` | `ThreadedFileDownloader.DoFileDownload` |
| `0x0600007D` | `0x3E10` | `0x3E1C` | `ThreadedFileDownloader.ThreadRunner` |
| `0x06000082` | `0x38A5` | `0x38A6` | `ThreadedFileDownloader.<DoFileDownload>b__1` |
| `0x0600009E` | `0x4550` | `0x455C` | `DownloadObject.DoDownload` |

`DoFileDownload`, `ThreadRunner`, and the constructor use 12-byte fat headers.
The short `<DoFileDownload>b__1` method has a one-byte tiny header. The PE
image base is `0x00400000`; `.text` RVA `0x2000` maps to file offset `0x200`.
Thus these IL code RVAs map to file-backed VAs by adding the IL offset and
`0x00400000`. These VAs address IL bytes, not JIT/native instructions.

## Fatal-worker cancellation boundary

`ThreadedFileDownloader::.ctor` initializes `mIsRunning` to `true`
(MethodDef `0x06000067`, IL `0x0000–0x0006`). In the first-fatal branch of
`ThreadRunner`, after the existing exception-slot check, the worker clears
`mIsRunning` at IL `0x019B`, stores the caught exception, and enumerates
`AllThreads` (MethodDef `0x0600007D`, IL `0x018E–0x01B6`). The abort loop
compares each thread with `Thread.get_CurrentThread()` and branches past the
abort call for that thread; `Thread.Abort()` is called only on the other
enumerated thread objects (IL `0x01C0–0x01CE`). This is an abort request, not
an observed join or proof that those threads have exited. The `AllThreads`
enumerator is disposed in its own finally (try `0x01B6/0x0023`, handler
`0x01D9/0x000E`).

The fatal handler then drains `DownloadQueue`. For each remaining pair it
checks whether the value is a `ManualResetEvent` and calls `Set()` on that
event (IL `0x01E7–0x0214`), before releasing the downloader lock through the
enclosing finally (try `0x0062/0x01C2`, handler `0x0224/0x0008`). This only
signals queued pairs; it does not dispose their events. An already-dequeued
pair is not part of this drain.

`ThreadRunner`'s catch covers only its `DownloadObject.DoDownload` call
(try `0x003B/0x0018`, handler `0x0053/0x02E0`, catch type
`System.Exception`). In that handler, a caught `ThreadAbortException` is
identified at IL `0x022C` and branches around the ordinary exception
diagnostic block to the loop's `mIsRunning` test at IL `0x0333`. The loop
returns when that flag is false. This is not a separate `ThreadAbortException`
catch and does not establish handling for an abort delivered outside the
protected `DoDownload` call.
Nor does that IL branch prove suppression of a CLR thread abort: the
framework's automatic ThreadAbortException rethrow behavior is distinct
from ordinary control-flow branches. No ResetAbort or guaranteed graceful
return is established by this trace.

`DoDownload` has a whole-method finally (MethodDef `0x0600009E`, try
`0x0000/0x04B6`, handler `0x04B6/0x001C`) that marks the object complete and
sets its supplied event in the handler. That is distinct from the fatal
worker's queue-drain `Set()`. The IL proves the protected region and handler
instructions, but does not by itself prove that an abort request has completed
or that every waiter has observed a signal.

## Outer waits and cleanup

`DoFileDownload` creates one `ManualResetEvent` per queued part, stores each in
the list of wait handles, and starts the workers. It creates a
`System.Threading.Timer` with a 2,000 ms due time and period, assigns it to
`mUpdateTimer`, then enters the event/thread wait region (IL `0x0264–0x028D`).
That region waits on each queued event with `WaitOne()` and then repeats while
`AllThreads.FindAll(thread => thread.IsAlive).Count > 0` (IL `0x028F–0x02F4`;
predicate MethodDef `0x06000082`, body RVA `0x38A5`, whose IL calls
`Thread.get_IsAlive`).

Only after both wait phases finish does `DoFileDownload` read the shared
exception slot and log/throw the stored exception (IL `0x0302–0x032A`). Thus
the outer throw is not an immediate response to the worker's abort request:
it follows the event waits and the live-thread scan.

The timer is disposed by the finally around this wait region (try
`0x028D/0x0069`, handler `0x02F6/0x000C`). The handler null-checks its local
timer and calls `IDisposable.Dispose()`. It runs when the wait region exits
normally or by exception, before the subsequent exception-slot check. The
body does not call `Timer.Change()`/`Stop()` or clear `mUpdateTimer`; `Dispose`
is the observed cleanup operation, and its IL alone does not establish that
an already-running callback has finished.

The wait-list enumerator is disposed in its own finally (try
`0x0298/0x001E`, handler `0x02B6/0x000E`), but the individual
`ManualResetEvent`/`WaitHandle` instances are not disposed in this method.
There is no whole-method finally around `DoFileDownload`: the other finally
regions release the worker-start lock (try `0x0194/0x00C8`, handler
`0x025C/0x0008`), dispose the assembly enumerator (try `0x0405/0x00A8`,
handler `0x04AD/0x000E`), and dispose the output stream (try
`0x03F8/0x00C5`, handler `0x04BD/0x000C`). The timer-finally only applies
after the timer/wait region has been reached; it is not a global cleanup
handler for earlier failures.

## Instruction-aligned guards

The following checks use the existing `SampleImage deployment` instance.
They are parent-integration suggestions; this task did not edit the test
program. The current `SampleImage` API can directly guard the instructions and
the `System.Exception` catch. Finally-region offsets below were read from the
method body's PE exception-region metadata and are now checked using
`SampleImage.ManagedFinallyMatches`.

```csharp
Check("DeploymentCommon constructor starts worker loop as running",
    deployment.Matches(0x00403640, "02177D28000004"));

Check("DeploymentCommon fatal worker clears mIsRunning",
    deployment.Matches(0x00403FB7, "02167D280000040207287B000006"));

Check("DeploymentCommon abort loop excludes CurrentThread",
    deployment.Matches(0x00403FDC, "288F00000A092E06096FAE00000A"));

Check("DeploymentCommon queued ManualResetEvent is signaled during drain",
    deployment.Matches(0x0040401E,
        "120428A900000A75710000016FB000000A26"));

Check("DeploymentCommon ThreadAbortException skips ordinary worker diagnostics",
    deployment.Matches(0x00404048, "07757B0000013AFA000000"));

Check("DeploymentCommon wait-list loop waits on each handle",
    deployment.Matches(0x00403B4B, "11056F9900000A"));

Check("DeploymentCommon outer liveness scan uses the IsAlive predicate",
    deployment.Matches(0x00403B8E, "FE0682000006739D00000A803A000004")
    && deployment.Matches(0x004038A6, "026F7B00000A2A"));

Check("DeploymentCommon outer timer is created, stored, and disposed in finally",
    deployment.Matches(0x00403B20,
        "0202FE067E000006739700000A1420D007000020D0070000739800000A2513117D2A000004")
    && deployment.Matches(0x00403BB2, "11122C0711126F4000000ADC"));

Check("DeploymentCommon worker catch and abort diagnostic string",
    deployment.ManagedCatchMatches(
        DownloadWorkerNativeLayout.ThreadRunnerMethodRow,
        0x3B, 0x18, 0x53, 0x2E0, "System.Exception")
    && deployment.ManagedUserStringsMatch(
        (0x1F19, "Caught an exception while downloading part number {0}: {1}")));
```

The exact finally regions relevant to this behavior are:

```text
ThreadRunner 0x0600007D:
  Finally try 0x01B6/0x0023 -> handler 0x01D9/0x000E (AllThreads enumerator)
  Finally try 0x0062/0x01C2 -> handler 0x0224/0x0008 (downloader lock)
  Catch   try 0x003B/0x0018 -> handler 0x0053/0x02E0 (System.Exception)

DoFileDownload 0x0600007C:
  Finally try 0x0194/0x00C8 -> handler 0x025C/0x0008 (worker-start lock)
  Finally try 0x0298/0x001E -> handler 0x02B6/0x000E (wait-list enumerator)
  Finally try 0x028D/0x0069 -> handler 0x02F6/0x000C (outer timer)
  Finally try 0x0405/0x00A8 -> handler 0x04AD/0x000E (assembly enumerator)
  Finally try 0x03F8/0x00C5 -> handler 0x04BD/0x000C (output stream)

DoDownload 0x0600009E:
  Finally try 0x0000/0x04B6 -> handler 0x04B6/0x001C (completion state/event)
```

## Pure disposition checks

These model only decisions/descriptors. They do not create or abort threads,
signal/dispose handles, instantiate timers, perform I/O, or construct
exceptions.

```csharp
Check("DeploymentCommon abort disposition skips only the current thread",
    DownloadCancellationNativeLayout.GetAbortDisposition(isCurrentThread: true)
        == DownloadWorkerAbortDisposition.SkipCurrentThread
    && DownloadCancellationNativeLayout.GetAbortDisposition(isCurrentThread: false)
        == DownloadWorkerAbortDisposition.RequestAbortOtherThread);

Check("DeploymentCommon queue drain signals only ManualResetEvent values",
    DownloadCancellationNativeLayout.ShouldSignalDrainedQueueEvent(
        valueIsManualResetEvent: true)
    && !DownloadCancellationNativeLayout.ShouldSignalDrainedQueueEvent(
        valueIsManualResetEvent: false));

Check("DeploymentCommon outer wait continues while any worker is alive",
    DownloadCancellationNativeLayout.ShouldContinueWaitingForWorkers(
        anyWorkerIsAlive: true)
    && !DownloadCancellationNativeLayout.ShouldContinueWaitingForWorkers(
        anyWorkerIsAlive: false));

Check("DeploymentCommon timer cleanup is conditional on a non-null local",
    DownloadCancellationNativeLayout.ShouldDisposeTimerOnWaitExit(
        timerLocalIsNonNull: true)
    && !DownloadCancellationNativeLayout.ShouldDisposeTimerOnWaitExit(
        timerLocalIsNonNull: false));
```
