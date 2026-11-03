# Companion binary connections

This pass compares the nine authorized local samples without executing them,
opening network connections, or modifying their IDA databases. Samples reside
outside the repository under
`D:\SteamLibrary\steamapps\common\Firefall\system\bin`; paths below are relative
to that directory. No binary copies are committed.

## Sample identities and address conventions

All nine files have DOS/PE signatures and I386 COFF machine `0x14C`.
`DeploymentCommon.dll` additionally has a CLR header at RVA `0x2008`: its
I386 COFF field alone is not proof of a native x86-only implementation.
The other eight have no CLR header.

| Sample | Bytes | SHA-256 |
|---|---:|---|
| `FirefallClient.exe.original` | 40,197,848 | `E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235` |
| `awesomium.dll` | 23,470,048 | `DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E` |
| `dbghelp_red5.dll` | 1,213,200 | `C06430B8CB025BE506BE50A756488E1BCC3827C4F45158D93E4E3EEB98CE1E4F` |
| `DeploymentCommon.dll` | 76,800 | `33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0` |
| `DumpTruck.exe` | 16,675,032 | `53796D440D586A306C36ADAE86744C580B9F9ECD3987D2C0D3CAB0EA77ADD6E4` |
| `FirefallProcess.exe` | 39,128 | `A4055F0E08268150625772967E31D575D0E98D234970EC7F1ECEE87BDB6109BC` |
| `HWStats.exe` | 594,648 | `0AE559DA740DE2C51A77A652906F021833A17E2A634757C15A6ADE5BEB9D679D` |
| `r5dumper.exe` | 29,400 | `63F5E802548809C1327A4569CD66953BC3EDBFEFB6177D753A403022C3194729` |
| `RedHanded.exe` | 1,918,168 | `1FBD21966A9DA297AB46A061ECE94BE8F4822ACC103BF2D17CB5F076476AED9B` |

The currently opened `FirefallClient.exe` is the same size but has SHA-256
`97362D668FF64778193A64759265E4AE90842043C4A15B627DF9902FD9135445`.
That pre-existing difference is not attributed to this analysis. Original-client
checks deliberately use `.original`, not the live IDA input.

The client IDB uses raw file offsets at image base zero. **Companion addresses
in this document are preferred PE VAs**, not that convention. The companion
executables and DeploymentCommon have image base `0x400000`; Awesomium is
`0x10000000`; dbghelp is `0x3000000`. The checks translate through each
sample's actual PE sections rather than subtracting one shared constant.

## Confirmed connections and boundaries

| Producer | Consumer | Evidence and limits |
|---|---|---|
| FirefallClient | RedHanded | Existing bootstrap raw `0x2EA990` constructs the wrapper command and launches it; see [CrashHandler](CrashHandler.md). The wrapper is not a game-network message handler. |
| RedHanded | r5dumper | VA `0x407040` / raw `0x6440` builds positional arguments, inherits a metadata pipe, writes once, then checks child exit. |
| r5dumper | dbghelp_red5 | `LoadLibraryA`/`GetProcAddress` in VA `0x401D50` resolve `MiniDumpWriteDump`. The actual DLL exports it at RVA `0x5E800`, ordinal31, without forwarding. |
| RedHanded | DumpTruck | Existing fatal-result path constructs `--dump/--auth/--log/--app/--msg` options; see [RedHanded](RedHanded.md). Not the metadata pipe described below. |
| DumpTruck | HWStats | VA `0x42AB90` / raw `0x29F90` redirects the child's stdout to a separate anonymous pipe and forwards byte counts to an output sink. |
| FirefallClient | Awesomium | All86 static by-name imports in the **original** client match actual exports in the supplied DLL. This checks the bundled ABI names, not all implementations. |
| FirefallProcess | Awesomium | Its only import from the DLL is `?ChildProcessMain@Awesomium@@YAHPAUHINSTANCE__@@@Z`; VA `0x40103E` calls IAT VA `0x4060DC`. The symbol resolves to DLL VA `0x1003A710`, as recorded in `AwesomiumLayout`. |
| DeploymentCommon | Deployment tooling | CLR header and `mscoree.dll!_CorDllMain`; earlier IL reconstruction is in [DeploymentCommon](DeploymentCommon.md). None of the seven other native executables/DLLs checked here statically imports DeploymentCommon. This does not exclude dynamic/managed loading by other software. |

The companion projects already contain partial reconstructions. New source is
`src\RedHanded\DumperHandoffNativeLayout.cs` and
`src\R5Dumper\UserStreamNativeLayout.cs`; hardware pipe facts extend
`src\DumpTruck\DumpTruckLayout.cs`. No new wire parser, process launcher,
dump writer, or uploader is introduced.

## RedHanded to r5dumper: lifetime and framing

Producer VA `0x407040` constructs an inheritable anonymous pipe with requested
capacity `0x4000`, duplicates the write handle as non-inheritable when that
operation succeeds, and closes the original inheritable write handle.
If duplication fails, it logs the failure and continues with the original
write handle; do not model that path as guaranteed non-inheritance.
`CreateProcessA` uses handle inheritance and `CREATE_NO_WINDOW` (`0x08000000`).

The positional child arguments are decimal PID, dump flags, decimal read-handle
value, and dump-file path. Receiver VA `0x401D50` independently establishes that
order from `argv[1..4]`; its numbers use base10 `strtoul`.
The original producer varargs pass whole DWORDs even where decompiler output
incorrectly renders a `char` value.

The normal debug-handler path initializes flags to `0x40` at VA `0x40787C`;
the special exception path may select2. The optional bigdump call at
`0x40832F` explicitly passes2 (full-memory flag), not the normal default.
No child is launched by these reconstruction checks.

Metadata is a sequence of byte lines:

```text
<decimal stream type>|str|<text>\r\n
<decimal stream type>|i32|<unsigned decimal value>\r\n
<decimal stream type>|i64|<unsigned decimal value>\r\n
\r\n
```

Producer VA `0x408167` / raw `0x7567` appends the final two-byte empty CRLF
line. Each record already ends in CRLF, so a nonempty metadata block ends
with `0D 0A 0D 0A`. The blank line is necessary for the observed handoff:
RedHanded writes the block at `0x40749B`, waits at `0x4074EB`, and closes
its pipe handles after the wait. The receiver cannot require the parent
to close its writer before completing.

`r5dumper` VA `0x4012F0` / raw `0x6F0` first tries to extract an already
buffered line, then reads up to512 bytes and appends the read chunk as a
NUL-terminated C string. Extractor `0x401390` / raw `0x790` searches for
literal `0D 0A`, returns the bytes before it, and removes that line plus
two terminator bytes from its accumulation. The outer loop at `0x4014C1`
recognizes the empty line as successful completion.

Consequences established statically:

- A CRLF may be split across reads; the accumulation supports it. A bare LF
  is not the searched delimiter. Unfinished input followed by `ReadFile`
  failure is a read error, not implicit success.
- This is not a binary-safe pipe reader: an embedded NUL terminates the
  chunk's C-string append. No escaping scheme is observed for CRLF in
  field values; such input can alter framing.
- Parent `WriteFile` success alone is insufficient. The code also requires
  written count equal requested count; a short successful write takes the
  same logged `TerminateProcess(..., 0xBADC0DE)` path as failure. It does
  not retry a partial write in this function.
- An exact successful write leads to an indefinite child wait. Parent success
  then depends on retrieving child exit code0. These are control-flow facts,
  not a claim that arbitrary inputs or handles will complete successfully.

## Actual stream ledger

The table gives the producer's stream-type `push` instruction; each is five
bytes (`68` plus the little-endian DWORD). Values are formatted as **decimal**
on the pipe; hexadecimal is used here for readability. In particular,
decompiler displays of arguments `0,2,...11` lose the upper `0x10000`:
the actual bytes, preserved in checks, are authoritative.

| Stream | Kind | Observed value source | Producer VA / raw |
|---|---|---|---|
| `0x10000` | str | Owner `+0x1C`, command line | `0x407AAF / 0x6EAF` |
| `0x10002` | str | Environment mnemonic, owner `+0x2C` | `0x407B54 / 0x6F54` |
| `0x10003` | str | Environment name, owner `+0x30` | `0x407BFB / 0x6FFB` |
| `0x10004` | i32 | Source revision, owner `+0x24` | `0x407C92 / 0x7092` |
| `0x10005` | i32 | Build number, owner `+0x28` | `0x407D30 / 0x7130` |
| `0x10006` | str | Decoded debug source | `0x407DCF / 0x71CF` |
| `0x10007` | str | Handler message argument | `0x407E68 / 0x7268` |
| `0x10008` | i64 | Process memory measurement, with fallback | `0x407F08 / 0x7308` |
| `0x10009` | i64 | Minimum total-page-file/total-virtual memory, or0 on status failure | `0x407FA3 / 0x73A3` |
| `0x1000A` | i32 | Debug event's thread ID | `0x40803D / 0x743D` |
| `0x1000B` | str | Application name, owner `+0x20` | `0x4080DF / 0x74DF` |

There is no `0x10001` record in this sequence.

### Owner initialization and thread identity

The names above are traced through `RedHanded.exe!wWinMain` VA `0x404540`,
not inferred from ID order. Its `app`, `sourcerev`, `buildnumber`,
`envmnemonic`, and `envname` options populate locals at stack `+0x24`,
`+0x28`, `+0x2C`, `+0x30`, and `+0x34`, respectively (offsets here are at
the constructor's balanced stack). Instructions at `0x404A72`,
`0x404A86`, `0x404A91`, `0x404AAB`, and `0x404ACE` copy those into object
stack slots `+0x80..+0x90`. The object base is stack `+0x60`:
`0x404B36` loads ECX with `[esp+0x68]` after two argument pushes and
`0x404B3B` calls the monitor. Subtraction gives owner offsets `+0x20`,
`+0x24`, `+0x28`, `+0x2C`, and `+0x30`.
Default text is `Firefall`, `prod`, and `Firefall` for app, mnemonic, and
environment name when those options are absent. An explicitly present
empty/null text follows the empty-string path, not the absent-option default.

Numeric options call imported `atoi` at `0x4048C9` and `0x4048F2`.
VA `0x4049BF..0x4049E7` replaces **zero**, not all nonpositive values,
with source revision117123 and build number1962. Nonzero negative values
survive as DWORDs and later format with `%u`; for example a parsed `-1`
becomes `4294967295` on the pipe. `CaptureBuildNumbers` models only this
post-`atoi` transformation, not text parsing or CRT overflow behavior.

The debug loop VA `0x409880` passes the event buffer at `[ebp-0x70]` to
helper `0x458B20` (`0x4098B6..0x4098BD`). Its `DEBUG_EVENT.dwThreadId`
is at `[ebp-0x68]`, offset8 in the Windows structure.
Both EXCEPTION_DEBUG_EVENT (case1) and RIP_EVENT (case9) push that DWORD
at `0x409A00` and `0x409A81` before calling `0x407630`
(`0x409A05` and `0x409A86`). The handler saves its first stack argument
from `[ebp+8]` to `[ebp-0x33C]` at `0x407665..0x407675`, then pushes
that unchanged local at `0x408031` into stream `0x1000A`.
Thus this stream is the **thread ID**, not a process ID, exception code,
or chance flag. For case9 the exception-record argument is null; the
thread ID still comes from the same debug event.

These are static data-flow conclusions with instruction-byte checks.
The exact meaning of decoded-source stream `0x10006` and the process-memory
measurement/fallback stream `0x10008` still needs its own bounded trace.

### Receiver tokenization and native descriptors

Tokenizer VA `0x401120` / raw `0x520` uses `find_first_not_of('|')` and
`find_first_of('|')`. Leading/repeated separators are skipped. Once the first
two fields have been obtained, the rest from the next nonseparator through
end-of-line becomes the value, **including any later pipes**. Two obtained
fields cause an empty third value to be appended by the reader. Types use
case-insensitive `stricmp`; values use `strtoul` or `_strtoui64` for integers.
Those routines' numeric-prefix behavior is not equivalent to managed
`uint.Parse`.

The existing `R5DumperUserInfo.Parse` remains a **convenience subset**: its
`Split('|')` rejects values with pipes and preserves empty split fields;
its managed numeric parsing is stricter. It must not be used as evidence
for native acceptance/rejection. No exact native parser is introduced here.

Descriptor builder VA `0x401A60` / raw `0xE60` creates12-byte x86
`MINIDUMP_USER_STREAM` entries:

| Native offset | Width | Source |
|---|---:|---|
| `+0` | 4 | Parsed stream type, copied unchanged from object `+4` |
| `+4` | 4 | Buffer length from vtable slot `+4` |
| `+8` | 4 | Buffer pointer from vtable slot `+0` |

`i32`/`i64` objects return a pointer to their native value at `+8` and sizes4/8
(`0x402180`, `0x4021A0`); the underlying integer bytes are little-endian x86
memory. Text accessor `0x401270` returns stored length plus1, including its
NUL terminator. No `0x10000` adjustment is added by the descriptor builder.

VA `0x401B50` / raw `0xF50` passes this user-stream array to the dynamically
resolved writer, with null exception-information and callback-information
arguments. This handoff does not supply an `EXCEPTION_POINTERS` object.
The matching dbghelp export is VA `0x305E800` / raw `0x5DC00`.
Its internals are not reimplemented.

File lifecycle is narrower than earlier notes suggested: if `OpenProcess`
fails, the created destination is closed but not deleted in that branch.
If the dump-writing helper is actually called and returns false, the file
is closed and deleted. The path returning failure is not synonymous with
unconditional deletion.

## DumpTruck to HWStats is a different pipe

`DumpTruck!0x42AB90` sets `STARTF_USESTDHANDLES`, redirects stdout through an
inheritable pipe with requested size2048, and launches literal `HWStats.exe`
without an output-file argument. `HWStats!0x404480` therefore selects stdout
in its existing reconstruction. The parent polls `PeekNamedPipe` and
`GetExitCodeProcess`, reads at most2048 bytes, and forwards the **actual**
count to sink vtable slot `+0x18` at `0x42AD60`.

It checks `STILL_ACTIVE`259, sleeps1ms between polls, and appends one NUL
byte to the sink on its normal completion path (also on exit-code-query
failure), at `0x42ADC7..0x42ADCE`. This is neither the CRLF dump-user-info
protocol nor a minidump user-stream array. A native nonzero exit code is
returned as itself; pipe/process/read failures have separate `-1` paths.
No claims are made here about cancellation, timeouts, compression internals,
or whether a later upload succeeds.

## Reproducible checks and open work

```powershell
dotnet build Firefall.Reconstruction.slnx -v:q
dotnet run --no-build --project tests\CompanionTools\CompanionTools.Checks.csproj -- --samples "D:\SteamLibrary\steamapps\common\Firefall\system\bin"
```

Checks hash every sample before reading its PE metadata, verify the11
stream pushes/formats/calls and receiver descriptor widths, and compare
the original client's86 Awesomium import names with the supplied DLL's
exports. They do not execute any target or contact a service. Without
`--samples`, only the pure managed decisions and metadata checks run,
with an explicit notice that binary evidence was not checked.

Next bounded traces: decoded-source and process-memory metadata semantics;
the DumpTruck sink's compression boundary and interpretation of its return codes; and
the managed loader/callers of DeploymentCommon. Static import absence is
not used as a claim that all nine form one direct-load chain.
