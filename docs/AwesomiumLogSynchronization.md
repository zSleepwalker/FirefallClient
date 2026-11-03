# Awesomium logger synchronization initialization

Static inspection of `awesomium.dll`, 23,470,048 bytes, SHA-256
`DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E`.
Addresses are preferred PE VAs (image base `0x10000000`).
`LogSynchronizationNativeLayout.cs` saves initialization decisions and a
wide-string name transform only. It creates no kernel objects or locks.

## Entry flag and mode

Initializer `0x10289AE0(mode,path)` checks byte `0x11596B20` at
`0x10289AEE`. Nonzero returns without applying either new argument.
When zero it stores the supplied mode DWORD to `0x11596B28`, before any
resource allocation succeeds.

Any nonzero mode takes the critical-section branch, not just mode1.
It allocates24 bytes and calls constructor `0x10288CF0` when allocation
returns nonzero. A null allocation stores zero to critical-section pointer
`0x11596B24`, then still sets initialized byte1 at `0x10289C4D`.
This is not a guaranteed usable synchronization object or a mutex fallback.
Constructor exception/failure semantics remain outside this capture.

Mode0 checks stored mutex `0x11596B2C`. A nonzero value bypasses name
construction/creation and sets initialized1; it is not otherwise validated.
With zero storage it builds a name and attempts CreateMutexW.

This is an observed byte flag, not proof of thread-safe once-only
initialization. No interlocked operation in the inspected entry path
establishes ordering for concurrent callers.

## Mutex name

Nonnull path argument, including a pointer to an empty string, is copied
using its null-terminated WCHAR length. A null pointer instead invokes
[default log-path builder](AwesomiumLogFile.md) at `0x10289B58`.
The original path pointer and an empty native string are not equivalent.
The same default-path API failure caveats apply here.

The loop `0x10289BA0-0x10289BB3` walks the copied string's stored WCHAR
length, changes every backslash to a forward slash, and preserves all other
code units. It then prepends the exact wide literal `Global\`.
It does not lowercase, canonicalize, resolve dot segments, replace colons,
hash or otherwise sanitize the path.

| Observed copied path | Constructed name |
|---|---|
| Empty | `Global\` |
| `C:\Game\debug.log` | `Global\C:/Game/debug.log` |
| `.\debug.log` | `Global\./debug.log` |
| `\\server\share\debug.log` | `Global\//server/share/debug.log` |
| `C:/Game/../debug.log` | `Global\C:/Game/../debug.log` |

`CaptureMutexName` takes an already observed copied string, not an original
pointer or input filename. It does not emulate native pointer termination,
path acquisition failures or Win32 object-name acceptance.

## Creation and retry

CreateMutexW at `0x10289BE9` receives null security attributes, initial-owner
FALSE, and the constructed name. Its returned handle is immediately stored
at `0x10289BEF`. Only zero is failure here, not `0xFFFFFFFF`.

On zero, the code destroys local strings and returns at `0x10289C17`
**before** setting initialized1. Mode remains0 and mutex remains0, so a
later call can attempt initialization again. No failure status is returned
by this void initializer. No LastError/GetLastError or existing-object
decision is made at this creation boundary.

On nonzero, string cleanup reaches the initialized1 assignment. Both a new
and an existing named mutex returned normally follow that same branch.
No actual object creation, access privilege, ownership or cross-process
coordination was observed during analysis.

The [sink caller](AwesomiumLogSinks.md) calls this initializer with `(0,0)`
before file output and does not check whether initialization succeeded.
Its subsequent wait result is also unchecked.

## Reconfiguration and resource lifetime

The initialized byte has only two direct xrefs in this image: the test at
`0x10289AEE` and the store of1 at `0x10289C4D`. The same bounded direct-xref
query found the critical-section pointer read/writes only in initializer,
lock/unlock calls and the logger configuration branch; mutex-handle reads/
write are initializer, WaitForSingleObject and ReleaseMutex. No direct
zero/reset writer for the initialized byte or synchronization handles was
found outside initialization. In particular, reconfiguration through
`0x1028A2A0` calls the initializer, but an already-set byte makes that call
return without replacing mode or synchronization objects.

This is an image-local direct-reference result, not a proof against
undetected indirect writes or code outside the sample. No logger shutdown/
destructor that closes the mutex or destroys the critical section was found
in these xrefs. Treat those resources as having no observed teardown path,
not as guaranteed process-lifetime objects.

The configuration helper's cleanup concerns a different global:
file handle `0x11596B0C`. Before applying a new file destination,
`0x1028A2A0` reads that global and conditionally calls CloseHandle at
`0x1028A684`, then stores0 at `0x1028A68A`. It does **not** close mutex
`0x11596B2C` or free critical-section pointer `0x11596B24` there.
Reconfiguration to destination0 or2 returns after this file-handle cleanup
and does not replace stored path object `0x11596B08`. Other destinations
copy the new path and proceed to optional DeleteFile/open logic.

Consequently closing a previous log file is not evidence that the global
lock/mutex is reset. Mutex wait/release calls and critical-section
enter/leave calls protect configuration/output according to stored mode;
their synchronization outcomes remain unchecked as recorded in
[log sinks](AwesomiumLogSinks.md).

Hash-guarded instruction checks bind flag/mode gates, path selection,
normalization loop, prefix, CreateMutexW arguments, failure return, and
critical-section null-allocation/initialized continuation. Separate
configuration guards bind CloseHandle to the file-handle global and its
zeroing store. Pure cases record those local decisions; they do not promise
successful synchronization or an observed reset in another code path.
