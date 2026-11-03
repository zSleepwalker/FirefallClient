# Awesomium log sink admission and unchecked writes

Static inspection of `awesomium.dll`, 23,470,048 bytes, SHA-256
`DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E`.
Addresses are preferred PE VAs (image base `0x10000000`).
`LogSinkNativeLayout.cs` records attempted-output decisions, not delivery,
and does not call any sink, synchronization primitive or severity handler.

## Early callback and ordinary sinks

Destructor `0x10289EC0` finalizes its stream and extracts a narrow string.
The [early interceptor](AwesomiumConversionDiagnostic.md) can consume it
before all ordinary sinks below. When consumed it skips these outputs and
severity dispatch, proceeding to cleanup/LastError restoration.

For an unconsumed diagnostic, instructions `0x10289F30-0x10289F4A` select:

| Destination DWORD interpreted as Int32 | OutputDebugStringA | stderr + flush | File-output block |
|---|---|---|---|
| 0 | No | Only signed severity >=2 | No |
| 1 | No | Only signed severity >=2 | Yes |
| 2 | Yes | Yes, even negative severity | No |
| 3 | Yes | Yes, even negative severity | Yes |
| Other values | No | Only signed severity >=2 | Yes |

These are local branch decisions, not a claim that arbitrary destination
values occur normally. In particular `disable-logging` selects destination0
in the observed [setup caller](AwesomiumLoggingSetup.md), but does not suppress
stderr for severity2 or higher, nor separate severity3/4 handling.

OutputDebugStringA is called at `0x10289F5B` with the narrow string pointer.
The stderr path calls static CRT `fprintf` at `0x10289F7E` with literal
`%s`, then `fflush` at `0x10289F8C`. Both use the stream array returned by
`0x1004E6A8`, plus byte offset `0x40`. The sample's three32-byte FILE slots
at `0x11553420` have descriptor values0/1/2 at offset `0x10`, supporting
identification of this third slot as stderr. No caller branch checks the
fprintf or fflush result.

The debugger and `%s` paths use null-terminated text. They do not prove
preservation of suffix bytes after an embedded NUL.

## File block, seek and exact byte count

Before destination-specific file admission, `0x10289F98` calls
`0x10289AE0(0,0)` to initialize synchronization if needed. Inside the file
block, the code chooses a critical-section helper or
WaitForSingleObject(mutex,INFINITE), then calls
[open helper `0x10289C70`](AwesomiumLogFile.md) at `0x10289FDA`.
Neither mutex wait result nor initialization failure is checked here.
Blocking, allocation failures and shared-state races remain unresolved;
an admitted block is not guaranteed to reach the write.

If the open helper returns AL0, the code skips seeking/writing and proceeds
to synchronization release. Otherwise it calls
SetFilePointer(handle,0,NULL,FILE_END) at `0x10289FF0`, independently of the
open helper's own seek, and ignores that result.

WriteFile at `0x1028A018` receives global handle `0x11596B0C`, the selected
inline/heap narrow string pointer, the **stored string byte length** as its
DWORD count, a local bytes-written output pointer, and null OVERLAPPED.
No terminator byte is added by this call. Unlike debugger/CRT text output,
this count is not recomputed through strlen. Embedded NUL is therefore not
a delimiter for the requested write, without implying successful output.

The BOOL result and bytes-written local are never tested in this destructor.
There is no partial-write retry, result-dependent error branch, handle
clear/close, or FlushFileBuffers call on this inspected write path. Both
WriteFile outcomes reach the same synchronization release then severity
dispatch. A pure capture must not report successful logging from admission.

For synthetic destinations other than0/1/2/3, the destructor's file predicate
and open helper's predicate differ: the destructor enters the file block,
but a zero-handle open helper can return true without opening (it opens
only for destination1/3). The caller can consequently attempt seek/write on
zero in that synthetic state. Do not silently normalize unknown destination
values to the supported enum or fabricate a valid handle.

Severity3/4 handlers run after the ordinary sinks and remain separate from
these booleans. Hash-guarded instruction checks bind sink gates, CRT calls,
WriteFile argument/count handling, ignored results and release continuation.
No target, log, debugger output, file API or synchronization API was executed.
