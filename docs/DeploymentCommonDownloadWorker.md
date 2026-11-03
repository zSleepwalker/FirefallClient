# DeploymentCommon download worker error handoff

## Sample and method evidence

Sample: `D:\SteamLibrary\steamapps\common\Firefall\system\bin\DeploymentCommon.dll`,
76,800 bytes, SHA-256
`33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0`.
The managed image was inspected through PE/CLI metadata and IL; it was not
loaded or executed. MethodDefs are tokens in this image:

| MethodDef | Body RVA | IL code RVA | Method |
|---|---:|---:|---|
| `0x0600009E` | `0x4550` | `0x455C` | `DownloadObject.DoDownload` |
| `0x0600007D` | `0x3E10` | `0x3E1C` | `ThreadedFileDownloader.ThreadRunner` |
| `0x0600007C` | `0x38B0` | `0x38BC` | `ThreadedFileDownloader.DoFileDownload` |

These method bodies use 12-byte fat headers. The PE image base is `0x00400000`;
`.text` RVA `0x2000` maps to file offset `0x200`, so a code RVA plus an IL
offset gives the VA used by `SampleImage.Matches`, and the corresponding file
offset is `code RVA + IL offset - 0x1E00`. These VAs address file-backed IL
bytes, not JIT/native instructions.

## Observed exception path

`ThreadedFileDownloader.DoFileDownload` constructs each
`DownloadObject` with its part number, offsets, path, digest, progress
callback, and a `ManualResetEvent`; it enqueues the object/event pair and
adds the event to its wait list (IL `0x00E4–0x015E`). It starts background
threads with a `ParameterizedThreadStart` bound to `ThreadRunner`
(IL `0x01E1–0x0249`).

`ThreadRunner` dequeues a pair under the downloader lock, obtains its key
(`DownloadObject`) and value (the completion event), then calls
`DownloadObject.DoDownload(event)` (IL `0x003B–0x004D`). That call is covered
by the `System.Exception` catch at try `0x003B/0x0018`, handler
`0x0053/0x02E0`. `DoDownload` has no catch clause: its exception regions are
four `finally` clauses, including a whole-method finally at try
`0x0000/0x04B6`, handler `0x04B6/0x001C`.

The terminal range-retry branch in `DoDownload` logs the retry-limit message,
constructs `ExceededRangeRequestRetryLimitException` using MethodDef
`0x06000065`, and throws it (IL `0x02DF–0x0313`). The exception type extends
`System.Exception`; its constructor calls `System.Exception::.ctor`. Before
that exception escapes `DoDownload`, its whole-method finally sets
`DownloadComplete` and sets the supplied `ManualResetEvent` when the argument
is one (IL `0x04B6–0x04D1`).

In `ThreadRunner`, the caught exception is first tested as
`System.Net.WebException`. Only a WebException with the global
`mRecoverableErrorCount < 10` and either status value `14` or message exactly
`"test"` takes the retry path. That path stores the exception in the
`DownloadObject`'s `LastRecoverableException`, increments its
`RecoverableErrorCount`, logs the part count and current global count,
increments the downloader-wide `mRecoverableErrorCount`, resets the completion
event, and re-enqueues the pair (IL `0x0053–0x0189`). These are
error-accounting/worker decisions, not a replay of the HTTP or MD5 retry
behavior.

The terminal range-retry exception is not a WebException, so it bypasses that
retry classification. Under `mLockObj`, the worker checks the shared
`Exception` property (backed by `mException`): if already occupied, it leaves
without replacing the existing exception. Otherwise it clears
`mIsRunning`, stores this exception in the slot, aborts other worker threads,
and drains queued pairs, setting their completion events (IL `0x018E–0x022B`).
The generic handler logs `Caught an exception while downloading part number
{0}: {1}`; additional response/status diagnostics are conditional on the
exception also being a WebException and therefore do not run for this custom
retry-limit exception (IL `0x022C–0x0332`).

`DoFileDownload` waits on the queued completion events and then waits while
any worker thread is alive (IL `0x028F–0x0302`). It reads the shared
`Exception` slot. If set, it logs the prefix
`An error occurred while using the multi-part downloader: ` plus the stored
message and executes a `throw` instruction on the retrieved exception object
(not an IL `rethrow`); this is the terminal exception observable by
its caller. If the slot is empty, the method continues through progress
reporting and file assembly and can return normally (`void`) if later reporting/assembly operations succeed.
The outer method's
exception regions are `finally` clauses, not catches. In a concurrent fatal
failure, the first exception stored in the shared slot remains the one
reported by this outer path.

`DownloadWorkerNativeLayout` models only these worker disposition and outer
throw decisions. It performs no threading, I/O, callbacks, downloads, or
exception construction.

## Instruction-aligned binary guards

These standalone checks use the existing `SampleImage deployment` instance.
They are suggestions for parent integration, not edits to the test program:

```csharp
Check("DeploymentCommon worker dequeues a pair and invokes DoDownload",
    deployment.Matches(0x00403E57, "120028A800000A120028A900000A6F9E000006DDE0020000"));

Check("DeploymentCommon ThreadRunner catches System.Exception around the worker call",
    deployment.ManagedCatchMatches(DownloadWorkerNativeLayout.ThreadRunnerMethodRow,
        0x3B, 0x18, 0x53, 0x2E0, "System.Exception"));

Check("DeploymentCommon worker retry classification and per-part error slots",
    deployment.Matches(0x00403E6F,
        "0B027B29000004251308288700000A07757800000139210100000775780000010C027B270000041F0A3C0D010000086FAA00000A1F0E2E15086F2D00000A72F41D0070281C00000A39EE000000")
    && deployment.Matches(0x00403EBC,
        "120028A800000A086F9C000006120028A800000A256F9900000617586F9A000006"));

Check("DeploymentCommon worker aggregates retry counts and requeues the reset event pair",
    deployment.Matches(0x00403F53,
        "02257B2700000417587D27000004120028A900000A75710000012C12120028A900000A75710000016FAB00000A26027B2900000425130A288700000A02286E000006066F8400000ADE08110A289600000ADCDDA5010000"));

Check("DeploymentCommon worker stores only the first fatal exception",
    deployment.Matches(0x00403FAA,
        "02287A0000062C05DDA301000002167D280000040207287B000006")
    && deployment.Matches(0x00404053,
        "72191F0070120028A800000A6F870000068C6D000001076F2D00000A281500000A28F9000006"));

Check("DeploymentCommon terminal retry failure throws through DoDownload finally",
    deployment.Matches(0x0040483B,
        "72522500700228870000068C6D000001281B00000A28F900000672F32500700228870000068C6D000001281B00000A73650000067A")
    && deployment.Matches(0x00404A12,
        "021728980000060375710000012C0C0375710000016FB000000A26DC"));

Check("DeploymentCommon DoFileDownload reports and throws its captured exception",
    deployment.Matches(0x00403BBE,
        "02287A0000062C21725A1B007002287A0000066F2D00000A281E00000A28F900000602287A0000067A"));

Check("DeploymentCommon worker and outer failure diagnostic strings",
    deployment.ManagedUserStringsMatch(
        (0x1DF4, "test"),
        (0x1DFE, "Part {0} has encountered {1} total potentially recoverable errors.  {2}.  Status {3}.  Global recoverable error count {4}/{5}  Trying again."),
        (0x1F19, "Caught an exception while downloading part number {0}: {1}"),
        (0x1F8F, "A WebException occurred: "),
        (0x1B5A, "An error occurred while using the multi-part downloader: "),
        (0x25F3, "Exceeded the maximum number of retry attempts for range request on chunk {0}.")));
```

## Pure disposition checks

These checks do not create exceptions or exercise threads:

```csharp
var terminalRangeFailure = DownloadWorkerNativeLayout.CaptureWorkerException(
    isWebException: false, webExceptionStatus: 0, message: null,
    globalRecoverableErrorCount: 0, exceptionSlotOccupied: false);
Check("DeploymentCommon terminal retry exception occupies the first fatal slot",
    terminalRangeFailure == DownloadWorkerExceptionAction.RecordFirstFatalException
    && DownloadWorkerNativeLayout.ShouldThrowFromDoFileDownload(exceptionSlotOccupied: true));

Check("DeploymentCommon qualifying WebException retries below the global limit",
    DownloadWorkerNativeLayout.CaptureWorkerException(
        isWebException: true, webExceptionStatus: 14, message: null,
        globalRecoverableErrorCount: 9, exceptionSlotOccupied: false)
    == DownloadWorkerExceptionAction.RetryPart
    && DownloadWorkerNativeLayout.CaptureWorkerException(
        isWebException: true, webExceptionStatus: 0, message: "test",
        globalRecoverableErrorCount: 9, exceptionSlotOccupied: false)
    == DownloadWorkerExceptionAction.RetryPart);

Check("DeploymentCommon exhausted worker error budget records a fatal exception",
    DownloadWorkerNativeLayout.CaptureWorkerException(
        isWebException: true, webExceptionStatus: 14, message: null,
        globalRecoverableErrorCount: 10, exceptionSlotOccupied: false)
    == DownloadWorkerExceptionAction.RecordFirstFatalException);

Check("DeploymentCommon later fatal worker exception preserves the occupied slot",
    DownloadWorkerNativeLayout.CaptureWorkerException(
        isWebException: false, webExceptionStatus: 0, message: null,
        globalRecoverableErrorCount: 1, exceptionSlotOccupied: true)
    == DownloadWorkerExceptionAction.KeepFirstFatalException
    && !DownloadWorkerNativeLayout.ShouldThrowFromDoFileDownload(exceptionSlotOccupied: false));
```
