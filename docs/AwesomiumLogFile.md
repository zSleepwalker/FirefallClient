# Awesomium log-file opening boundary

Static inspection of `awesomium.dll`, 23,470,048 bytes, SHA-256
`DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E`.
Addresses are preferred PE VAs (image base `0x10000000`).
`LogFileNativeLayout.cs` records inert decisions and constant API arguments;
it never opens, deletes, seeks or writes files.

## Entry and path preparation

Helper `0x10289C70` first compares global handle storage `0x11596B0C`
with zero. Any nonzero value returns AL1 immediately, including a synthetic
`0xFFFFFFFF`. This is not a validity check of an already stored handle.
No actual invalid entry state is claimed.

With zero storage it checks path-object pointer `0x11596B08`; a null pointer
causes allocation of28 bytes and default-path builder `0x10289A20`, then
stores the object pointer. Only afterward does the helper test destination
`0x11554A18`: exactly1 or3 attempts file opening, any other Int32 returns
true without opening. Path preparation may therefore precede even a
destination that does not request a file.

Allocation failure is not converted into a clean failure on the file-open
path: the observed code subsequently dereferences the path-object pointer.
Saved entry decisions describe control flow, not guaranteed completion,
safe allocation failure, or a valid file when AL1 is returned.

Default-path builder `0x10289A20` calls `GetModuleFileNameW(NULL,buffer,260)`,
ignores its return and then uses `wcslen` on that local buffer. It copies
the result to a wide string, searches backward for a backslash, truncates
after it when found, and appends `debug.log`. With no separator it appends
the name to the untrimmed string; it does not invent a directory separator.
Unchecked API failure/truncation and the resulting buffer contents remain
unresolved; no default path is synthesized for those cases.

## Configured open, fallback, seek

The configured path uses inline wide storage if string capacity<8,
otherwise the pointer in the first DWORD. Both open attempts use:

| CreateFileW argument | Observed value |
|---|---|
| Desired access | `0x40000000`, GENERIC_WRITE |
| Sharing | 3, FILE_SHARE_READ + FILE_SHARE_WRITE |
| Security attributes | null |
| Disposition | 4, OPEN_ALWAYS |
| Flags/attributes | `0x80`, FILE_ATTRIBUTE_NORMAL |
| Template | null |

`0x10289CD7` attempts the configured path and stores the result immediately.
Both zero and `0xFFFFFFFF` select the fallback `.\debug.log` at
`0x10289CFE`, using identical flags. The fallback is relative to the process
current directory, not necessarily the executable directory.

If either attempt returns a value other than zero and `0xFFFFFFFF`, the
helper calls `SetFilePointer(handle,0,NULL,FILE_END)` at `0x10289D15` and
returns true. The seek result is ignored; true is not evidence that the file
pointer reached EOF. If fallback also returns zero or `0xFFFFFFFF`, it
clears global storage at `0x10289D1F` and returns false. There is no
CloseHandle call on this helper's paths. Logger configuration separately
closes a previous file handle before replacing a file destination, as
documented in [synchronization/resource lifetime](AwesomiumLogSynchronization.md).

The [logging setup caller](AwesomiumLoggingSetup.md) may first attempt
DeleteFileW when append is false; OPEN_ALWAYS here does not itself truncate.
That deletion's ignored result and the seek's ignored result must not be
rewritten as guaranteed truncate/append behavior.

## Evidence and bounds

Hash-guarded instruction checks bind the entry shortcut, destination tests,
wide-string storage branch, both CreateFileW argument lists and result
checks, fallback literal, SetFilePointer arguments and final clear/false
path. Pure cases include zero, invalid and nonzero DWORD handles and
destinations outside1/3, without creating real handles.

No sample, filesystem API, logger, callback, UI or service was executed.
Actual handle validity, allocation failure effects, module-path API errors,
concurrent logger state and eventual write results are not resolved here.
