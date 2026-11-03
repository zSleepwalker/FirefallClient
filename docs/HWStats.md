# HWStats.exe

Static IDA analysis only. SHA256 `0AE559DA740DE2C51A77A652906F021833A17E2A634757C15A6ADE5BEB9D679D`, 594,648 bytes, x86 native console, PE stamp 2016-05-05. Reconstruction: `src\HWStats`.

## Observed
- `main` (0x404480) takes optional argv[1] output path (opened `"w"`), else stdout; calls report function 0x404340.
- Report: `CoInitialize`, prints `macaddr = xx:xx:...`, enumerates DirectSound playback (`soundplayback`) and capture (`soundrecording`) devices, then runs sections for registry `HARDWARE\DESCRIPTION\System\CentralProcessor`, `SYSTEM\CurrentControlSet\Enum\DISPLAY`, DriveSpace (`total = %.3f GB/MB`), D3D9 caps (`Direct3DCreate9`, `GetAvailableVidMem*`, `D3DVersion`, D3DFMT names), CPU features (MMX/SSE/SSE2/3DNow), and `%s = %s` key/value dumps.
- Imports: KERNEL32, USER32, ADVAPI32 (registry), SHELL32, ole32, NETAPI32, DSOUND, WINMM. NETAPI32's `Netbios` supplies the MAC survey; [MAC-address evidence](HWStatsMacAddress.md) records its enumeration/reset/status requests and unchecked failure-path output.
- DumpTruck launches it with a stdout pipe and compresses the output into the crash report.

## Interpreted
- Local hardware survey; output is plain text `key = value`, uploaded only via DumpTruck.

## Section decode (IDA decompile; VAs are image VAs)
- `sub_4035A0` (0x4035A0, thiscall on the report object): `GlobalMemoryStatusEx`; selects `ram = %.2f GB` when the low DWORD of whole physical MB is >= 1024, else passes original physical-byte DWORDs to `ram = %ull MB` (format string as stored). Both memory and affinity API results are unchecked. Then counts set bits of the *system* affinity mask (`GetProcessAffinityMask`) and emits `nprocessors = N` (via `sub_406AE0`). Finally calls `sub_403350`, which emits a `CPUID` block: `processor`, `model` (brand string, also tries registry `ProcessorNameString`), `family`, `model`, `stepping`, `feature`, `os_support`, `checks`. [RAM and processor evidence](HWStatsRamAndProcessors.md) records exact argument and arithmetic boundaries.
- `sub_404020` (0x404020): emits a `System` section header (`sub_406CB0`). `sub_403A20` builds an OS description via `GetVersionExW`, `GetNativeSystemInfo`/`GetSystemInfo`, `GetProductInfo` (NT 6.x), `GetSystemMetrics(SM_SERVERR2)` (NT 5.2) into a wide buffer. [OS text fragments](HWStatsOsDescriptionText.md) capture name/edition selection, literal spacing, service-pack/build order and architecture suffixes without emulating the CRT stream. Reads `HKLM\SOFTWARE\Microsoft\DirectX` value `Version`; prints `GetVersionEx = <OS string>, DirectX Version <ver>`. Then emits registry values (via `sub_4030F0`, REG_SZ/REG_MULTI_SZ only) `Identifier`, `SystemBiosDate`, `SystemBiosVersion`, `VideoBiosDate`, `VideoBiosVersion`.
- `sub_406880` (0x406880) is not a report section: report-object destructor/cleanup that frees 20 heap pointers at offsets +4..+0x53 and `fclose`s the FILE* at +0.
- Report object: FILE* at +0, indent level at +0x54 (printed as `%<4*n>s` padding before each line).

## Interpreted
- The OS/DirectX line combines the wide description from `sub_403A20` with the DirectX registry value. Native argument inspection recovered the buffer link hidden by the decompiler's prototype. [BIOS registry evidence](HWStatsBiosRegistry.md) traces `sub_406F30`: HKLM `HARDWARE\DESCRIPTION\SYSTEM\`, KEY_ALL_ACCESS, ANSI queries and REG_SZ/REG_MULTI_SZ-only print admission.

## Unresolved
- Complete CRT stream/outer-line formatting and failure-path text; OS name and edition fragments are captured.

[OS-description guards](HWStatsOsDescriptionGuards.md) now preserve version
and platform admission, native-system-info fallback, major-six export
requirements, unchecked `GetProductInfo` results and the caller's unchecked
destination use. The exact text renderer remains unresolved.

## DriveSpace follow-up

[DriveSpace output](HWStatsDriveSpace.md) corrects the former memory
label for `0x4037F0`: it surveys C-Z fixed-drive available-to-caller
space. Saved pure code preserves strict above-one thresholds, accumulation
before line admission, API-failure zeros, and the actual signed/extra-high-
part float arithmetic rather than substituting a generic unit converter.
No actual drives are queried.