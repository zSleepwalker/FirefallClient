# FirefallProcess.exe

Static, read-only triage (IDA MCP; no execution/patching).

- Path: `D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallProcess.exe`
- Size 39,128 bytes; SHA256 `A4055F0E08268150625772967E31D575D0E98D234970EC7F1ECEE87BDB6109BC`; MD5 `8ae322a83ab5d12b35ff77514bf89767`
- PE32 x86, base `0x400000`, image size `0xC000`, PE timestamp `0x5335BBA1` (2014-03-28 18:12:49 UTC)
- PDB string `C:\dev\chromium2\chromium\src\build\Release\awesomium_process.pdb` (`0x407880`)

## Observed
- Sections: `.text 0x401000-0x406000`, `.idata 0x406000`, `.rdata 0x4060E4`, `.data 0x408000`. 136 functions, 123 flagged as CRT library code; 97 strings.
- Entry `start` `0x4013A5` (MSVC CRT startup) -> `_wWinMain@16` `0x401000` (115 bytes).
- wWinMain body: installs an SEH4 frame (`__except_handler4`, scope table `0x4078F0`), then calls
  `ds:?ChildProcessMain@Awesomium@@YAHPAUHINSTANCE__@@@Z` (IAT `0x4060DC`) with `hInstance` as the only argument and returns its result.
  The SEH handler path (`0x401068`) calls `ExitProcess(1)`. `lpCmdLine`, `hPrevInstance`, `nShowCmd` are unused.
- Imports: KERNEL32 (CRT support only: heap, TLS/FLS, critical sections, `GetCommandLineW`, `LoadLibraryW`, `GetProcAddress`, `IsDebuggerPresent`, etc.), `awesomium.dll` (single import: `Awesomium::ChildProcessMain(HINSTANCE__*)`), USER32 `MessageBoxW` family (CRT runtime-error dialog only). No network, crypto, registry or file APIs beyond CRT.
- No application strings: only CRT locale/date strings, the PDB path and import names. No command-line literals/switch strings in this binary.

## Interpreted
- This is the stock Awesomium (Chromium-based embedded browser) helper process executable. It contains no Firefall-specific logic; all behaviour (argument parsing, IPC channel setup, renderer/plugin role) lives in `awesomium.dll`'s `ChildProcessMain`, which reads the process command line itself (it is not passed to wWinMain's arguments by this stub).
- Relationship to FirefallClient.exe: FirefallClient hosts awesomium.dll (the UI/web view layer) and presumably spawns this exe as the Awesomium child (renderer) process. The IPC is Chromium's internal IPC owned by awesomium.dll, not a Firefall protocol.

## Unresolved
- Exact child command-line switches and IPC channel naming (need `awesomium.dll` analysis; its IDB is separate and was not inspected).
- Where FirefallClient sets Awesomium's `child_process_path` (not examined in this pass).
- Key classes: none present in this binary.

Layout constants: `src\FirefallProcess\FirefallProcessLayout.cs`.
