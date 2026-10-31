# HWStats.exe

Static IDA analysis only. SHA256 `0AE559DA740DE2C51A77A652906F021833A17E2A634757C15A6ADE5BEB9D679D`, 594,648 bytes, x86 native console, PE stamp 2016-05-05. Reconstruction: `src\HWStats`.

## Observed
- `main` (0x404480) takes optional argv[1] output path (opened `"w"`), else stdout; calls report function 0x404340.
- Report: `CoInitialize`, prints `macaddr = xx:xx:...`, enumerates DirectSound playback (`soundplayback`) and capture (`soundrecording`) devices, then runs sections for registry `HARDWARE\DESCRIPTION\System\CentralProcessor`, `SYSTEM\CurrentControlSet\Enum\DISPLAY`, memory (`total = %.3f GB/MB`), D3D9 caps (`Direct3DCreate9`, `GetAvailableVidMem*`, `D3DVersion`, D3DFMT names), CPU features (MMX/SSE/SSE2/3DNow), and `%s = %s` key/value dumps.
- Imports: KERNEL32, USER32, ADVAPI32 (registry), SHELL32, ole32, NETAPI32, DSOUND, WINMM. No network imports.
- DumpTruck launches it with a stdout pipe and compresses the output into the crash report.

## Interpreted
- Local hardware survey; output is plain text `key = value`, uploaded only via DumpTruck.

## Section decode (IDA decompile; VAs are image VAs)
- `sub_4035A0` (0x4035A0, thiscall on the report object): `GlobalMemoryStatusEx`; prints `ram = %.2f GB` when total physical >= 1024 MB, else `ram = %ull MB` (format string as stored). Then counts set bits of the *system* affinity mask (`GetProcessAffinityMask`) and emits `nprocessors = N` (via `sub_406AE0`). Finally calls `sub_403350`, which emits a `CPUID` block: `processor`, `model` (brand string, also tries registry `ProcessorNameString`), `family`, `model`, `stepping`, `feature`, `os_support`, `checks`.
- `sub_404020` (0x404020): emits a `System` section header (`sub_406CB0`). `sub_403A20` builds an OS description via `GetVersionExW`, `GetNativeSystemInfo`/`GetSystemInfo`, `GetProductInfo` (NT 6.x), `GetSystemMetrics(SM_SERVERR2)` (NT 5.2) into a wide buffer (wstream-based; text pieces not recovered). Reads `HKLM\SOFTWARE\Microsoft\DirectX` value `Version`; prints `GetVersionEx = <OS string>, DirectX Version <ver>`. Then emits registry values (via `sub_4030F0`, REG_SZ/REG_MULTI_SZ only) `Identifier`, `SystemBiosDate`, `SystemBiosVersion`, `VideoBiosDate`, `VideoBiosVersion`.
- `sub_406880` (0x406880) is not a report section: report-object destructor/cleanup that frees 20 heap pointers at offsets +4..+0x53 and `fclose`s the FILE* at +0.
- Report object: FILE* at +0, indent level at +0x54 (printed as `%<4*n>s` padding before each line).

## Interpreted
- The OS/DirectX line uses the unrecovered wstream text of `sub_403A20`; the decompiler lost the buffer link into `sub_404020`'s `%s`. Exact registry path for BIOS values (`sub_406F30`) not traced.

## Unresolved
- Exact OS string text, `sub_406F30` registry key, and NETAPI usage (likely the `macaddr` code).