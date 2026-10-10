# Platform and linked components

This is a 32-bit Windows PE using the Visual C++ 2013 runtime (`MSVCR120.dll`, `MSVCP120.dll`) and OpenMP runtime (`VCOMP120.DLL`). The IDA raw-file database reports no imports because it did not load the image as PE; the actual import directory at RVA `0x19BCDCC` contains **31 imported DLLs**. Parsing PE import descriptors and thunk counts directly gives:

| Role | Imported modules (imported entries) |
| --- | --- |
| Graphics/input | `d3d9.dll` (3), `d3dx9_43.dll` (6), `D3DCOMPILER_43.dll` (3), `DINPUT8.dll` (1), `XINPUT1_3.dll` (4), `IMM32.dll` (16), `GDI32.dll` (7) |
| UI/integrations | `awesomium.dll` (86), `steam_api.dll` (10), `twitchsdk_32_release.dll` (30) |
| Networking/security | `WSOCK32.dll` (29), `WS2_32.dll` (7), `WINHTTP.dll` (18), `WINTRUST.dll` (7), `CRYPT32.dll` (9), `IPHLPAPI.DLL` (4) |
| Runtime/system | `MSVCR120.dll` (295), `MSVCP120.dll` (157), `VCOMP120.DLL` (12), `KERNEL32.dll` (164), `USER32.dll` (88), `WINMM.dll` (3), `ADVAPI32.dll` (31), `ole32.dll` (11), `OLEAUT32.dll` (3), `VERSION.dll` (3), `SHELL32.dll` (2), `SHLWAPI.dll` (1), `PSAPI.DLL` (5), `pdh.dll` (6), `SETUPAPI.dll` (3) |

These are **static PE imports**, not a full dependency list: e.g. the NVAPI bootstrap in `src\Graphics\Nvapi\` loads `nvapi.dll` dynamically. The binary embeds a Lua 5.3.1 version string near raw offset `0x17B9970`; this identifies the included Lua implementation, not a separate DLL import. Library presence alone does not identify the game's execution path for a given API.
