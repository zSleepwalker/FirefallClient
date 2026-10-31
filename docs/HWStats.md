# HWStats.exe

Static IDA analysis only. SHA256 `0AE559DA740DE2C51A77A652906F021833A17E2A634757C15A6ADE5BEB9D679D`, 594,648 bytes, x86 native console, PE stamp 2016-05-05. Reconstruction: `src\HWStats`.

## Observed
- `main` (0x404480) takes optional argv[1] output path (opened `"w"`), else stdout; calls report function 0x404340.
- Report: `CoInitialize`, prints `macaddr = xx:xx:...`, enumerates DirectSound playback (`soundplayback`) and capture (`soundrecording`) devices, then runs sections for registry `HARDWARE\DESCRIPTION\System\CentralProcessor`, `SYSTEM\CurrentControlSet\Enum\DISPLAY`, memory (`total = %.3f GB/MB`), D3D9 caps (`Direct3DCreate9`, `GetAvailableVidMem*`, `D3DVersion`, D3DFMT names), CPU features (MMX/SSE/SSE2/3DNow), and `%s = %s` key/value dumps.
- Imports: KERNEL32, USER32, ADVAPI32 (registry), SHELL32, ole32, NETAPI32, DSOUND, WINMM. No network imports.
- DumpTruck launches it with a stdout pipe and compresses the output into the crash report.

## Interpreted
- Local hardware survey; output is plain text `key = value`, uploaded only via DumpTruck.

## Unresolved
- Exact content of sections 0x4035A0, 0x404020, 0x406880 and the OS/NETAPI usage were not decompiled.
