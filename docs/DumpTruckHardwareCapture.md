# DumpTruck HWStats stdout capture

Static, read-only follow-up for the pipe lifetime and control flow in
[`CompanionBinaryConnections.md`](CompanionBinaryConnections.md#dumptruck-to-hwstats-is-a-different-pipe).
The prior ledger already records chunk forwarding and the blob/report handoff;
this note captures the endpoint inheritance and the less-obvious API-result
branches.

Sample: `D:\SteamLibrary\steamapps\common\Firefall\system\bin\DumpTruck.exe`,
16,675,032 bytes, SHA-256
`53796D440D586A306C36ADAE86744C580B9F9ECD3987D2C0D3CAB0EA77ADD6E4`.
The preferred image base is `0x400000`: function VA `0x42AB90` is RVA
`0x2AB90` and raw file offset `0x29F90`. All `0x42...` addresses below are
preferred VAs (not raw offsets).

## Pipe ownership and inheritance

At `0x42AC27..0x42AC50`, `sub_42AB90` initializes a 12-byte
`SECURITY_ATTRIBUTES`, sets `bInheritHandle=1` at `0x42AC32`, leaves its
security descriptor null, initializes both output slots to `INVALID_HANDLE_VALUE`,
and calls `CreatePipe` with requested capacity `0x800`. The output slots are
the local read handle and `STARTUPINFOA.hStdOutput`, i.e. the parent's read
endpoint and the write endpoint installed as the child's stdout.
`STARTF_USESTDHANDLES` (`0x100`) is set at `0x42ABE2`; `CreateProcessA` is
called with `bInheritHandles=TRUE` at `0x42AC93/0x42ACA0`.

Consequently, on the ordinary Win32 inheritance semantics used here, **both**
pipe endpoints are inheritable when the child is created. This routine does
not clear the read handle's inherit flag, close either endpoint before launch,
or use an explicit handle list. The child is assigned the write endpoint as
stdout, but the same inheritance flag also makes the read endpoint eligible
for inheritance. The parent retains both local handles until common cleanup.
Afterward it calls `CloseHandle` on the read endpoint (`0x42ADE8`), write
endpoint (`0x42AE00`), process thread (`0x42AE18`), and process
(`0x42AE30`); return values from these closes are not tested.

This is an ownership/inheritance observation, not evidence that the program
deadlocks on EOF: it does not wait for pipe closure. It uses child exit-code
polling instead. On the `CreatePipe`-failure branch, it returns `-1` directly
before the common cleanup block. The `CreatePipe` out-parameter state on that
failure is not analyzed, so no handle leak claim is made.

The `PROCESS_INFORMATION` local is not explicitly initialized before
`CreateProcessA`. Even on its false-return path the common cleanup compares
its thread/process fields with `INVALID_HANDLE_VALUE` and calls `CloseHandle`
for values that differ. The fields' failure-path contents and any resulting
effect are not established here; this is a literal control-flow observation,
not a claim that invalid handles are closed at runtime.

## Poll, read, and API-result decisions

The function first sets its exit-code local to `STILL_ACTIVE` (`259`) and
calls `GetExitCodeProcess` at `0x42ACEC`. The inner loop peeks at
`0x42AD1B/0x42AD78`; only a successful peek reporting nonzero available bytes
reaches `ReadFile`. It requests exactly `0x800` bytes at `0x42AD45` (it does
not clamp the request to the peek count), checks the `ReadFile` BOOL, and on
success forwards the count written by `ReadFile` to sink vtable slot `+0x18`
at `0x42AD60`. The next peek follows immediately.

If peek fails or reports no bytes, the loop compares the most recently
retrieved exit code against `259`. A non-`259` value is returned unchanged
after appending a single NUL byte to the sink. If it is still active, the
function calls `Sleep(1)` (`0x42AD8D`), queries the exit code again
(`0x42ADA0`), and goes back to peeking. There is no `WaitForSingleObject`,
pipe-EOF wait, timeout, or cancellation path in this function. A failed peek
is not separately classified; if the child is still active it follows the
same sleep/re-query route as an empty pipe.

Failure branches are asymmetric:

| API outcome | Native path/result |
|---|---|
| `CreatePipe` false | Immediate `-1` return (`0x42AC56..0x42AC6D`); bypasses shared cleanup and appends no NUL. |
| `CreateProcessA` false | `-1`, then common cleanup of the parent pipe handles; no NUL. |
| Initial `GetExitCodeProcess` false | Append NUL, return `-1`, common cleanup. |
| `PeekNamedPipe` false | Not itself an error return; test current exit code, then either append NUL/return it or sleep and retry the exit query. |
| `ReadFile` false | Immediate `-1` through common cleanup; does not append NUL or wait for child exit. |
| Follow-up `GetExitCodeProcess` false | Append NUL, return `-1`, common cleanup. |
| Child exit code other than `259` | Append NUL, return that exact code, common cleanup. |

The final NUL therefore appears on normal child completion and either
`GetExitCodeProcess` failure, but not on `CreateProcessA` or `ReadFile`
failure. `PeekNamedPipe` and `CloseHandle` return values are not interpreted
as distinct errors beyond the branches shown. This records the instructions;
it does not predict whether any particular API failure occurs at runtime.

## Admission to the report

`sub_40A3F0` calls the generator at `0x40AC3E` with its `slBlob` sink. The
existing ledger and `HardwareReportNativeLayout` already cover that boundary:
each successful read count, then the final NUL count, is appended before
compression. A nonzero generator return (including `-1`) logs generator
failure and omits the hardware layer while continuing report packaging.
Only return code zero admits type-3 hardware-layer compression; compression
failure is a separate submit-packaging failure.

The inert branch model is `src\DumpTruck\HardwareSurveyCaptureNativeLayout.cs`.
It does not launch a child, touch a pipe, interpret HWStats text, compress
bytes, or upload a report.
