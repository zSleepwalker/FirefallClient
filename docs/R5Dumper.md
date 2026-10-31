# r5dumper.exe

Static IDA analysis only. SHA256 `63F5E802548809C1327A4569CD66953BC3EDBFEFB6177D753A403022C3194729`, 29,400 bytes, x86 console, PE stamp 2012-10-25, VS2008 CRT. Reconstruction: `src\R5Dumper` (includes a compile-checked user-info line parser).

## Observed
- Usage: `r5dumper <pid> <dmpflags> <inhandle> <dmpfile>`; main at 0x401D50.
- Loads `dbghelp_red5.dll`, resolves `MiniDumpWriteDump`.
- Reads user info from the inherited handle (0x200-byte ReadFile loop): lines `<key>|<i32|i64|str>|<value>` (type case-insensitive; 2 fields = empty value; else "Malformed user information string"; unknown type error). Key is a numeric stream type.
- Creates the dump file (CREATE_ALWAYS), `OpenProcess(0x450, pid)`, writes minidump with the user streams; deletes the file on failure.
- Exit codes: 0 ok, -1 dbghelp load failure, -2 usage, -3 user-info read failure, -4 dump failure.
- Imports only KERNEL32/MSVCP90/MSVCR90; no network or registry.

## Interpreted
- Out-of-process dump writer invoked by RedHanded (`sub_407040`) so the dump is taken from a healthy process.

## Unresolved
- Exact meaning of the "Failed to write the minidump" path (not xref-checked); mapping of stream types to semantic names is defined by RedHanded.
