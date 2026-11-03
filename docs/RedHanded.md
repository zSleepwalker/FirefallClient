# RedHanded.exe

Static IDA analysis only. SHA256 `1FBD21966A9DA297AB46A061ECE94BE8F4822ACC103BF2D17CB5F076476AED9B`, 1,918,168 bytes, x86 GUI, PE stamp 2016-05-05, VS2013 CRT. A 64-bit copy exists at `system\bin\64\RedHanded.exe` (not analysed). Reconstruction: `src\RedHanded`.

## Observed
- Options (wWinMain 0x404540): `rundir dumpurl dumptruck-autosubmit app(Firefall) sourcerev(117123) buildnumber(1962) envmnemonic(prod) envname(Firefall) hotfix-exe dumptruck(DumpTruck.exe) dumper(r5dumper.exe) bigdump debug nofear`; args after `--` are the monitored command line.
- Imports: debugger APIs (WaitForDebugEvent, ReadProcessMemory, CreateProcessW...), pipes, Toolhelp, WinHTTP, WS2_32, WinVerifyTrust/CryptCATAdmin, Crypt*, registry, PDH, SetupAPI, user32 (hooks, clipboard, SendInput).
- Loop `sub_406820(rundir,...)` launches and debugs the target; repeats while status == 75.
- On fatal/error results it starts DumpTruck detached with `--dump= --auth= --log= --app= --msg= [--autosubmit --url= --debug]`.
- Debug-event handler `sub_407630`: writes a dump using r5dumper (`sub_407040`) and a 0x4000-byte inheritable pipe. It sends eleven typed CRLF records, then an empty CRLF line. Failure or a short pipe write kills the dumper with `0xBADC0DE`; an exact write leads to a child wait and exit-code check. The metadata fields are not identical to the separate telemetry JSON fields.
- Debug-string protocol requires `RED5-` before `LOGPATH=` or `DUMPAUTH=`. Bare fields are not recognized by the traced event consumer. Only ANSI strings with reported length above5 reach its remote read.
- Own crash handler writes `%TEMP%\RedHanded.dmp` via dbghelp_red5 (full memory, 5 user streams: 0x1000B app, 0x10002 env, 0x10005 build, 0x10004 rev, 0x10007 msg).
- Telemetry `sub_4092C0`: JSON POST to `<clientapi_host>/api/v1/client_event`; host from `HKCU\Software\Red 5 Studios\<app>\Operator\clientapi_host`. Fields: event, action, data{app_name, game_env, game_build, source_rev, account, message, source}. Event `redhanded_init_failed`.
- No hardcoded host; URL templates only.

## Interpreted
- Watchdog/debugger parent of the game: collects crash dump + log, hands to DumpTruck for upload. Status 75 is probably "restart (hotfix)".
- `rh::subsystem::DebuggerIntegrity` and IpcMessageHeader RTTI suggest a named-pipe link to the client and anti-tamper checks.

## Unresolved
- Full `sub_406820` semantics, broader debugger IPC, use of WinVerifyTrust/Crypt*, remaining `--debug` effects, and the client-side producer of exception-supplied source bytes.

The [companion handoff ledger](CompanionBinaryConnections.md) verifies that
r5dumper's three decimal arguments are PID, dump flags, and inherited read handle.
The optional bigdump call passes flags2. It also distinguishes actual stream
IDs from truncated decompiler arguments and identifies the blank-line terminator.
The follow-up traces metadata owner fields to application name, source revision,
build number, and environment mnemonic/name options. Stream `0x1000A` is the
debug event's thread ID in both exception and RIP-event calls. Numeric option
defaults replace only zero after `atoi`; negative nonzero values survive and
are emitted with unsigned decimal formatting.
Source stream `0x10006` reads at most1024 exception-supplied bytes for private
codes `0x012ED500/501`, otherwise preserving explicit source markers.
Memory stream `0x10008` prefers64-bit PDH Virtual Bytes, falling back only
on zero to a zero-extended DWORD PagefileUsage; it is not uniformly a
working-set measurement. See the ledger for the gates, API-result caveats,
and the independent total-page-file/total-virtual ceiling in stream `0x10009`.

## Client debug-string handoff

The original client emits these through verified KERNEL32!OutputDebugStringA
(IAT VA `0x1AC434C`), with an IsDebuggerPresent gate at each call site:

| Client raw function / emission | Static format | Destination in RedHanded |
|---|---|---|
| `0x12BBC60` / `0x12BBD55` | `RED5-LOGPATH=%s\n` | Owner `+0x0C`, value starts at byte13 |
| `0x63AEF0` / `0x63B15B` | `RED5-DUMPAUTH=!%s\n` | Owner `+0x10`, value starts at byte14 |

The LOGPATH routine normalizes/composes its configured path with `console.log`
before emission; that filename literal is VA `0x1C1556C`. The DUMPAUTH
format at VA `0x1B61C14` includes a literal **`!`** before its opaque input.
RedHanded copies the suffix immediately after `DUMPAUTH=` and does not
strip that marker in this function. No credential values are collected,
saved, or inferred by this analysis.

On OUTPUT_DEBUG_STRING_EVENT (event8), RedHanded reads `fUnicode` as a WORD
at event record `+0x10` and reported length as a WORD at `+0x12`
(`0x409ACB..0x409ADA`). It ignores any nonzero Unicode flag or length <=5.
Otherwise it allocates reported-length+1 and calls ReadProcessMemory for
the reported byte count (`0x409AF5`), with a null bytes-read pointer.
This is not the1024-byte cap used by its exception-text readers:
reported debug-string length is a16-bit field and can request up to65535
bytes here. A failed read skips field processing; no default path/auth
assignment is performed by that branch.

After a successful read, `0x409B07..0x409B18` trims any trailing sequence
of NUL, CR, and LF bytes, writes a local NUL at the resulting length, and
logs the text. Prefix comparisons are case-sensitive `strncmp`, first
`RED5-` (5 bytes), then `LOGPATH=` (8) or `DUMPAUTH=` (9).
The matched suffix is assigned at `0x409B5C` or `0x409B92`.
Interior NUL still truncates subsequent C-string operations; this is not
a binary-safe protocol or a general newline parser.

`DebugStringHandoffNativeLayout.cs` saves the producer/consumer constants
and exact read-admission decision; it does not read target memory or parse
actual credentials. Native checks hash the original client and RedHanded
before comparing literals, import identities, gates, trimming bytes, and
assignment offsets. The downstream interpretation of the `!` marker and
additional debug-string producers remain separate traces.
