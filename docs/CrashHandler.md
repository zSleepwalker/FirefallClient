# FirefallClient.exe crash handler (static, read-only IDA + byte scans)

Addresses: file offsets (IDB addresses) unless marked VA. .text VA=raw+0x400C00, .rdata VA=raw+0x401400. Constants: `src/Startup/CrashHandlerNativeLayout.cs`; byte check in `StartupBinaryEvidence.cs`.

## Observed
- Install function `0x12C37D0` (`__fastcall(char enable, int filter)`): if enable and `GetModuleHandleA(0)==0x400000`, stores filter and calls `SetUnhandledExceptionFilter(filter)`; then `signal(SIGABRT=22, 0x12C3710)`, `_set_purecall_handler(0x12C3790)`, `_set_invalid_parameter_handler(0x12C37A0)`, `_set_error_mode(1)`. Unconditionally `set_terminate(0x12C3770)`, `set_unexpected(0x12C3780)`, `_set_abort_behavior(0,1)`. Disable path zeroes the same slots. Called from WinMain-like `0x2EAE70` (call at 0x2EAF0C) with (1, 0), i.e. filter NULL. A second installer `0x12BD5A0` (caller 0x304553) also calls SetConsoleCtrlHandler and sets filter 0.
- Handler stubs (tail-jump to fatal `0x12C3550` with name/message): TERMINATE/"std::terminate()", UNEXPECTED/"std::unexpected()", PUREVIRT/"pure virtual function call" (VA 0x1C16000/0x1C1600C), INVALIDP/"invalid parameter" (VA 0x1C16028/0x1C16034). `0x12C3710` SIGABRT handler: `sprintf_s("SIGABRT - signo %d")`, name "ASSERT".
- Fatal `0x12C3550` (noreturn): thread-id compare-exchange re-entry guard; optional hook `dword_1E4B09C(msg)`; if debugger present raises via `0x12C3470`; logs "%s: %s" with "DEATH" (VA 0x1C16070) at level 5; `_flushall`; `_exit(123)`.
- Assert paths: `0x12C3650` (non-terminating, RaiseException-style, codes 0x012ED500/0x012ED502), `0x12C38E0` formats `assert(%s) failed at: %s:%d\n` / `assertmsg(%s, %s) failed at: %s:%d\n`.
- Dump writers (no direct callers found, no pointer scan hits): ANSI `0x12C2F90(name, msg, EXCEPTION_POINTERS*)`, wide `0x12C3250`. Flow: `SetUnhandledExceptionFilter(0)` then restore; `LoadLibraryA("dbghelp_red5.dll")` (VA 0x1C16048 / wide copy 0x1C160AC); `GetProcAddress("MiniDumpWriteDump")`; `GetTempPath` + `_snprintf("%s%s.dmp", temp, name)`; `CreateFileW(GENERIC_WRITE, CREATE_ALWAYS)`; `MiniDumpWriteDump(process, pid, file, DumpType=2 MiniDumpWithFullMemory, exc{tid, ptrs, 0} or NULL, 5 user streams, NULL)`. Streams: 0x1000B=name, 0x10002="prod"(5), 0x10005=1962, 0x10004=117123, 0x10007=message. Returns the path buffer or 0.
- RedHanded launcher `0x2EA990`: picks `64\RedHanded.exe` (AMD64) else `RedHanded.exe` via `0x2EA6B0`, builds command line `"%s"%s--rundir="%s" --app="%s" --sourcerev="%u" --buildnumber="%u" --envmnemonic="%s" --envname="%s" --hotfix-exe="%s" -- %s` (VA 0x1B3F2B8) with optional `--steam-overlay`, `--dumptruck`, `--bigdump`, `--bigdump --debug`, `--dumptruck-autosubmit` strings, app "Firefall", then `CreateProcessW` (0x2EADB2). Failure text: "Unable to execute redhanded wrapper. FirefallClient may not be run without it." (message box). Strings "Firefall (v1.5.1962)", "prod-1962" nearby.

## Interpreted
- The client is meant to run under RedHanded (crash wrapper); in-process, crashes through terminate/purecall/invalid-parameter/SIGABRT end in `_exit(123)` with a "DEATH" log. Dump writers are the Red 5 full-memory dump path (dbghelp_red5.dll bundled, see DbgHelpRed5.md), written to the temp folder as `<name>.dmp`; likely reached indirectly or from the hook at `dword_1E4B09C`.
- The 1962 stream value is the build number (matches "prod-1962"); 117123 is probably the source revision.
- No DumpTruck/r5dumper exe strings exist; `--dumptruck*` are RedHanded arguments (DumpTruck is likely inside RedHanded).

## Unresolved
- Callers/registration of the two dump writers and of hook `dword_1E4B09C`.
- Meaning of stream types 0x10002/4/5/7/0x1000B beyond inference; import slot 0x1AC41B0 assumed RaiseException.
- Why WinMain installs with filter NULL; other SetUnhandledExceptionFilter sites (0x2FAA55, 0xE54955.., 0xF05AD2, 0xF66347) not examined.
- Which condition adds each RedHanded argument; `last_crash.log` consumer (0xE5D04F) not analyzed.

## Hook setter, wrapper and RedHanded trigger (static pass)

Observed: `dword_1E4B09C` is read only by fatal 0x12C3550 (0x12C35AA). Its only writer is the unnamed setter at raw 0x12C38D0 (`89 0D 9C B0 E4 01 C3`). Wrapper 0x12C38B0 loads cells 1E4B098/1E4B094 and jumps to installer 0x12C37D0. Neither has an E8/E9 rel32 caller or absolute-VA reference. Dump writers 0x12C2F90/0x12C3250 likewise have no rel32 caller and no absolute-VA reference (checked in the .original image). RedHanded launcher 0x2EA990 has one rel32 caller: `ClientApplicationBootstrap` 0x2EAE70 at 0x2EAEBF.

Interpreted: the hook is unset on the observed paths, so fatal calls nothing through it. Writers are probably reached by data or run-time paths not found.

Unresolved: any caller of the dump writers, setter or wrapper (scoped to the scans above; computed or register-based calls were not searched).
