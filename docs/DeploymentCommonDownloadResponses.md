# DeploymentCommon response and retry boundary

## Sample and method evidence

Sample: `D:\SteamLibrary\steamapps\common\Firefall\system\bin\DeploymentCommon.dll`,
76,800 bytes, SHA-256
`33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0`.
The managed image was read as PE/CLI metadata and IL; it was not loaded or
executed. MethodDef token `0x0600009E` is
`DownloadObject.DoDownload`, body RVA `0x4550`. Its 12-byte fat header puts
the IL code at RVA `0x455C`; the code is `0x4D3` bytes, through IL offset
`0x04D2`. With image base `0x00400000` and `.text` RVA `0x2000` at raw
offset `0x200`, the body begins at file offset `0x2750` and IL at `0x275C`.
The VA guards below therefore address IL bytes in the file-backed PE image,
not JIT/native instruction addresses.

The method identity can be checked directly through `PEReader` metadata:

```csharp
var responseMethod = metadata.GetMethodDefinition(MetadataTokens.MethodDefinitionHandle(0x9E));
var responseOwner = metadata.GetTypeDefinition(responseMethod.GetDeclaringType());
Check("DeploymentCommon DoDownload MethodDef identity and body RVA",
    metadata.GetString(responseOwner.Name) == "DownloadObject"
        && metadata.GetString(responseMethod.Name) == "DoDownload"
        && responseMethod.RelativeVirtualAddress == 0x4550);
```

## Observed boundary

The retry counter is local `V_6`, initialized to zero at IL `0x0121–0x0122`.
After `GetResponse`, `StatusCode` is compared with 206 at IL `0x0245–0x0255`.
Equality branches directly to IL `0x0314`, skips the unexpected-status log,
and reads the response stream.

Every non-206 status first reaches the unexpected-response log at IL
`0x0256–0x02AB`. It is then compared with 200 at IL `0x02AC–0x02B9`.
Statuses other than 200 branch to the same IL `0x0314` stream path. Thus 201,
416, and 500 are not rejected by this comparison if a response with that
status actually reaches it: they are logged and their response streams are
read. This does not establish that `HttpWebRequest.GetResponse` returns
normally for those statuses; it can throw before this boundary. No exception
response recovery or real error-status reachability is inferred.
Status 200 alone takes the special retry path:

* IL `0x02BA–0x02C3` calls `Thread.Sleep(1500)`; this reconstruction does not
  sleep.
* IL `0x02C4–0x02CF` increments `V_6` and uses signed `bgt.s` against 10.
* A resulting count of 1 through 10 logs “Trying download again” and leaves
  back to IL `0x0124`, restarting the request loop. Count 11 takes IL
  `0x02DF–0x0313` and throws
  `ExceededRangeRequestRetryLimitException`. Since the increment precedes a
  strict `> 10` test, this is ten permitted retries after the initial attempt.

The final MD5-mismatch branch increments that same local `V_6` at IL
`0x0428–0x0433`, applies the same strict `> 10` test, and branches back to
IL `0x0124` at IL `0x043E` when the count is at most 10. Its terminal path at
IL `0x0443–0x0477` throws the same exception type. This comparison is reached
only when `PartMd5` is nonempty; an absent digest bypasses the MD5 test and
does not increment the counter. Consequently response-200 retries and final
MD5 mismatches consume one shared per-call counter; the retry loop does not
reset it. IL `add` is unchecked, so the pure model preserves wraparound for
synthetic integer inputs.

`DownloadResponseNativeLayout` records those two decisions only. It issues no
requests, reads or writes files, sleeps, invokes callbacks, computes hashes,
or throws the native exception. `RetryLimitExceeded` represents the decision
to throw; the observed IL throws the exception described above. The method's
exception regions are `finally` clauses, not catches around these branches;
whether an outer caller handles a propagated exception was not traced.

## Instruction-aligned binary guards

These are narrow, instruction-aligned `SampleImage.Matches` guards, shown in
the existing test program's `Check(name, bool)` form. They are intended for
parent integration, not as a change to the test program:

```csharp
Check("DeploymentCommon DoDownload response and shared-counter IL guards",
    deployment.Matches(0x004047A1, "110D6F6500000A20CE0000003BBE000000") // IL 0245: status == 206 -> 0314
    && deployment.Matches(0x00404808, "110D6F6500000A20C8000000335A") // IL 02AC: status != 200 -> 0314
    && deployment.Matches(0x00404820, "11061758130611061F0A300F") // IL 02C4: V_6++, signed > 10
    && deployment.Matches(0x00404836, "DD45FEFFFF") // IL 02DA: retry leaves to 0124
    && deployment.Matches(0x00404984, "11061758130611061F0A300F") // IL 0428: same V_6++, same limit
    && deployment.Matches(0x0040499A, "38E1FCFFFF")); // IL 043E: MD5 retry branches to 0124
```

The corresponding user-string heap checks use the offsets from the `ldstr`
tokens in this method:

```csharp
Check("DeploymentCommon DoDownload retry and MD5 diagnostic strings",
    deployment.ManagedUserStringsMatch(
    (0x2497, "Got an unexpected response code from range request with offset of {0}-{3} for chunk {2}: {1}"),
    (0x16B8, "Trying download again"),
    (0x2552, "Exceeded max number of retries (10), starting download over again for chunk {0}"),
    (0x25F3, "Exceeded the maximum number of retry attempts for range request on chunk {0}."),
    (0x272A, "Downloaded file part {0}, but MD5 didn't match.  Expected {1} but got {2}."),
    (0x27C1, "Trying again"),
    (0x27DB, "Exceeded max number of retries (10), starting chunk download over again for chunk {0}"),
    (0x2888, "Exceeded the maximum number of retry attempts (MD5 mismatch) for chunk {0}.")));
```

The following pure checks cover the observable decision boundary and the
shared counter. They make no requests or hashes:

```csharp
var partialContent = DownloadResponseNativeLayout.CaptureStatusCode(206, 3);
Check("DeploymentCommon 206 reads response stream without retry",
    partialContent == new DownloadResponseDecision(DownloadResponseAction.ReadResponseStream, 3));

var ignoredRange = DownloadResponseNativeLayout.CaptureStatusCode(200, 9);
Check("DeploymentCommon 200 consumes retry ten and retries",
    ignoredRange == new DownloadResponseDecision(DownloadResponseAction.RetryRequest, 10));

var otherStatus = DownloadResponseNativeLayout.CaptureStatusCode(500, 3);
Check("DeploymentCommon non-200 non-206 logs then reads response stream",
    otherStatus == new DownloadResponseDecision(DownloadResponseAction.ReadUnexpectedStatusResponseStream, 3));

var terminalRangeRetry = DownloadResponseNativeLayout.CaptureStatusCode(200, 10);
Check("DeploymentCommon 200 retry eleven exceeds the strict greater-than-ten limit",
    terminalRangeRetry == new DownloadResponseDecision(DownloadResponseAction.RetryLimitExceeded, 11));

var md5MismatchAfterRangeRetries =
    DownloadResponseNativeLayout.CaptureMd5Mismatch(ignoredRange.RetryCount);
Check("DeploymentCommon final MD5 mismatch shares the range-retry counter",
    md5MismatchAfterRangeRetries
        == new DownloadResponseDecision(DownloadResponseAction.RetryLimitExceeded, 11));
```

## Inert decision table

| Input status | Decision | Counter |
|---|---|---|
| 206 | Read response stream without the unexpected-status log | unchanged |
| 200 | Retry request, or report retry-limit exceeded when incremented count is greater than 10 | incremented |
| Any other status | Log unexpected status, then read response stream | unchanged |
| Nonempty expected MD5 and final mismatch | Retry request, or report retry-limit exceeded when incremented count is greater than 10 | incremented shared counter |
| Empty expected MD5 | Skip final MD5 comparison | unchanged |

No behavior of the outer caller, its error reporting, or its retry scheduling is
inferred here.
