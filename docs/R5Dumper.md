# r5dumper.exe

Static IDA analysis only. SHA256 `63F5E802548809C1327A4569CD66953BC3EDBFEFB6177D753A403022C3194729`, 29,400 bytes, x86 console, PE stamp 2012-10-25, VS2008 CRT. Reconstruction: `src\R5Dumper` (includes a compile-checked user-info line parser).

## Observed
- Usage: `r5dumper <pid> <dmpflags> <inhandle> <dmpfile>`; main at 0x401D50.
- Loads `dbghelp_red5.dll`, resolves `MiniDumpWriteDump`.
- Reads user info from the inherited handle (0x200-byte ReadFile loop): CRLF lines `<key>|<i32|i64|str>|<value>` terminated by an empty CRLF line. Native tokenization skips repeated separators and preserves the third-field remainder, including later pipes. The saved C# parser is a convenience subset, not a byte-accurate native reader.
- Creates the dump file (CREATE_ALWAYS), `OpenProcess(0x450, pid)`, writes minidump with the user streams. A failed dump write deletes the file; the separate OpenProcess-failure branch closes but does not delete it.
- Exit codes: 0 ok, -1 dbghelp load failure, -2 usage, -3 user-info read failure, -4 dump failure.
- Imports only KERNEL32/MSVCP90/MSVCR90; no network or registry.

## Interpreted
- Out-of-process dump writer invoked by RedHanded (`sub_407040`) so the dump is taken from a healthy process.

## Cross-binary follow-up

[Companion binary connections](CompanionBinaryConnections.md) traces both ends of
the inherited pipe and the bundled dbghelp export. It records all eleven actual
producer stream IDs (`0x10000`, `0x10002..0x1000B`), native descriptor sizes,
blank-line completion, and parent exact-write/child-exit checks. The minidump
call passes null exception/callback information; metadata is carried as user
streams. Some producer owner-field names still require initialization tracing.
