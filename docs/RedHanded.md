# RedHanded.exe

Static IDA analysis only. SHA256 `1FBD21966A9DA297AB46A061ECE94BE8F4822ACC103BF2D17CB5F076476AED9B`, 1,918,168 bytes, x86 GUI, PE stamp 2016-05-05, VS2013 CRT. A 64-bit copy exists at `system\bin\64\RedHanded.exe` (not analysed). Reconstruction: `src\RedHanded`.

## Observed
- Options (wWinMain 0x404540): `rundir dumpurl dumptruck-autosubmit app(Firefall) sourcerev(117123) buildnumber(1962) envmnemonic(prod) envname(Firefall) hotfix-exe dumptruck(DumpTruck.exe) dumper(r5dumper.exe) bigdump debug nofear`; args after `--` are the monitored command line.
- Imports: debugger APIs (WaitForDebugEvent, ReadProcessMemory, CreateProcessW...), pipes, Toolhelp, WinHTTP, WS2_32, WinVerifyTrust/CryptCATAdmin, Crypt*, registry, PDH, SetupAPI, user32 (hooks, clipboard, SendInput).
- Loop `sub_406820(rundir,...)` launches and debugs the target; repeats while status == 75.
- On fatal/error results it starts DumpTruck detached with `--dump= --auth= --log= --app= --msg= [--autosubmit --url= --debug]`.
- Debug-event handler `sub_407630`: writes dump (temp `XXXX-XXXX-XXXX-XXXX.tmp`, `bigdump.dmp`) by spawning r5dumper (`sub_407040`) with a 0x4000-byte inheritable pipe; sends userdata lines `<n>|str|%s`, `|i32|%u`, `|i64|%llu` (command_line, mem_used, mem_total, coredump, crash, EXCEPTION, DEBUG_SOURCE/MESSAGE). Kills dumper with 0xBADC0DE on pipe write failure.
- Debug-string protocol: `RED5-LOGPATH=`, `LOGPATH=`, `DUMPAUTH=`.
- Own crash handler writes `%TEMP%\RedHanded.dmp` via dbghelp_red5 (full memory, 5 user streams: 0x1000B app, 0x10002 env, 0x10005 build, 0x10004 rev, 0x10007 msg).
- Telemetry `sub_4092C0`: JSON POST to `<clientapi_host>/api/v1/client_event`; host from `HKCU\Software\Red 5 Studios\<app>\Operator\clientapi_host`. Fields: event, action, data{app_name, game_env, game_build, source_rev, account, message, source}. Event `redhanded_init_failed`.
- No hardcoded host; URL templates only.

## Interpreted
- Watchdog/debugger parent of the game: collects crash dump + log, hands to DumpTruck for upload. Status 75 is probably "restart (hotfix)".
- `rh::subsystem::DebuggerIntegrity` and IpcMessageHeader RTTI suggest a named-pipe link to the client and anti-tamper checks.

## Unresolved
- `sub_406820` semantics, IPC protocol, use of WinVerifyTrust/Crypt*, effect of `--bigdump`/`--debug`, exact three `%u` values for r5dumper.
