# dbghelp_red5.dll triage

Static, read-only. No execution, no IDB writes. Exports/imports/strings were read with `pefile` and a raw string scan of the files (the IDA MCP instance routing is shared with other agents and a `select_instance` to the dbghelp instance returned a different binary, so IDA was not used for this pass).

## Files (observed)

| Path (under `Firefall\system\bin`) | Machine | Size | SHA256 |
|---|---|---|---|
| `dbghelp_red5.dll` | x86 | 1,213,200 | `C06430B8CB025BE506BE50A756488E1BCC3827C4F45158D93E4E3EEB98CE1E4F` |
| `64\dbghelp_red5.dll` | x64 | 1,558,912 | `63A9550E4084A68D4BE812E670197F710349E5F6C718BF19E695759AF980D54A` |

x86: PE timestamp 1265054906 (2010-02-01 20:08:26 UTC), ImageBase `0x3000000`, entry RVA `0x6C811`, sections `.text .data .rsrc .reloc`, PDB name `dbghelp.pdb`. x64 timestamp 1265055344, sections add `.pdata`.

Version resource: InternalName/OriginalFilename `DBGHELP.DLL`, FileDescription "Windows Image Helper", Product "Debugging Tools for Windows(R)", FileVersion `6.12.0002.633 (debuggers(dbg).100201-1203)`, PrivateBuild flag set (as in the Microsoft redistributable).

## Verdict

**Stock Microsoft dbghelp 6.12.0002.633 (Debugging Tools for Windows, Feb 2010), merely renamed.**
- Authenticode signature status `Valid`, signer `CN=Microsoft Corporation, OU=MOPR` (x86 file). A valid hash-checked signature means the bytes are unmodified.
- Export directory name is still `dbghelp.dll`; 205 named exports (x86) / 203 (x64), the standard dbghelp 6.12 set (`MiniDumpWriteDump`, `StackWalk64`, `Sym*`, `SymSrv*`, `Image*`, WinDbg extension exports `WinDbgExtensionDllInit`, `ExtensionApiVersion`, `block chksym dbghelp dh fptr homedir itoldyouso lmi lminfo omap srcfiles stack_force_ebp stackdbg sym symsrv vc7fpo`). The x86 build additionally exports `MapDebugInformation`, `UnmapDebugInformation` (x64 omits them) and `SymLoadModuleExW` appears only in the x86 list.
- Imports only `msvcrt.dll` (83) and `KERNEL32.dll` (81) in x86 (x64: 88/99). No Red 5 modules.
- URL strings are only Microsoft CRL/cert URLs plus `HttpQueryInfoA` (symsrv). No "red5", "firefall", crash-upload or other custom URL strings found. No custom code observed.
- Why renamed: the Windows system `dbghelp.dll` is typically older/newer and loaded from System32; shipping a private copy under a unique name guarantees `MiniDumpWriteDump` and a known version. This reason is an interpretation.

## Consumers (observed strings)

`dbghelp_red5.dll` is named in `FirefallClient.exe`, `DumpTruck.exe`, `HWStats.exe`, `r5dumper.exe`, `RedHanded.exe` (all in `system\bin`).

FirefallClient.exe:
- File `0x1814C48` / `0x1814CAC` (VA `0x1C16048` / `0x1C160AC`): two `dbghelp_red5.dll` literals next to `assertmsg(%s, %s) failed at: %s:%d`, `ASSERT`, `PUREVIRT` (`pure virtual function call`), `INVALIDP` (`invalid parameter`), `DEATH`, `%s%s.dmp`, `MiniDumpWriteDump` (twice, probably two `GetProcAddress` names). Interpretation: the crash/assert handler loads the DLL by name and resolves `MiniDumpWriteDump` dynamically to write `<dir><name>.dmp` minidumps. Not a static import: no dbghelp entry in the import table.
- File `0x16E7770` (VA `0x1AE8B70`): a separate `dbghelp.dll` literal (Havok 2010.1.0 area) with `RtlCaptureStackBackTrace`, `RtlCaptureContext`, `SymCleanup`, `SymGetLineFromAddr64`, `SymGetModuleBase64`, `SymFunctionTableAccess64`, `StackWalk64`, `SymGetSymFromAddr64`, `SymSetOptions`, plus "dbghelp.dll, version too old" messages. This is the Havok stack-walker, which loads the system `dbghelp.dll`, not the Red 5 copy.

## Unresolved

- Code xrefs to the two `dbghelp_red5.dll` literals (callers, dump path/flags, which struct fields are filled) were not traced.
- Whether `DumpTruck.exe`/`r5dumper.exe` call the same exports (e.g. `MiniDumpWriteDump` flags) is unchecked.
- Byte-level diff against a pristine 6.12.0002.633 was not done (no reference copy); the valid signature is the evidence for integrity.

No source reconstruction is warranted: the DLL is third-party Microsoft code with no Red 5 modifications.

