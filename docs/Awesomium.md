# awesomium.dll (third party)

Static triage via IDA; image VAs, base 0x10000000. Concise: only Firefall-relevant parts.

## Observed
- x86 DLL, image size 0x16A5000, md5 `60602fe6daf7bb25336c9a3bc515394d`, sha256 `de27851602cb1387ef50086761b417d3d39a2a2648b5490939f843a711ff7f6e`.
- 855 exports: 530 `_Awe_*@N` C wrappers, 317 C++ `?...@Awesomium@@` names. 36 imported modules (incl. WS2_32, WINHTTP, d3d9, avcodec-53, MF).
- Embedded Chromium 18 (UA string `Chrome/18.0.1003.1 ... Awesomium/`, source path `C:\dev\chromium2\chromium\src`).
- `Awesomium::IsChildProcess` (VA 0x1003A630) checks the command line for switch `type`. `Awesomium::ChildProcessMain` (VA 0x1003A710) returns 0 without it, else runs Chromium content main (`sub_10064B90`). C wrappers: VA 0x1004C820 / 0x1004C840.
- WebCore init (VA 0x1000CF80) resolves the child exe path (`sub_1000CE50`, from `WebConfig::child_process_path`; setter wrapper VA 0x1004B2E0) and sets switch `browser-subprocess-path`; sets `enable-logging` when logging is on.
- Child launcher (VA 0x10189DD0) reads `--type` (renderer, plugin, worker, nacl-loader, gpu-process, ppapi, utility), appends switches (`no-sandbox`, `disable-gpu-sandbox`, `in-process-plugins`, `disable-3d-apis`, `disable-webgl`, ...), ` --type=renderer `, `/prefetch:%d`, and spawns via `StartProcessWithAccess::LAUNCHPROCESS`.
- Awesomium-specific switches/strings: `awesomium-asset-protocol`, `awesomium-user-script`, `awesomium-log-path`, `./awesomium.log`.
- IPC: UTF-16 string `\\.\pipe\chrome.` (string data VA 0x11267238).

## Interpreted
- The helper child is FirefallProcess.exe (stock `awesomium_process`, see FirefallProcess.md), launched via `browser-subprocess-path`; IPC is Chromium named pipes `\\.\pipe\chrome.*`.

## Unresolved
- Exact pipe name suffix format. See client-side section for the remaining client-side unknowns.

## FirefallClient.exe client side (static, IDA addresses = raw file offsets; VA = raw+0x400C00 in .text, raw+0x401400 in .rdata)
### Observed
- awesomium.dll is a plain static import (no LoadLibrary/GetProcAddress loop, no runtime table). Import descriptor RVA table: OFT RVA 0x19BDE4C, IAT FirstThunk RVA 0x16C4E00 (VA 0x1AC4E00..0x1AC4F54, stride 4), exactly 86 by-name entries; slot 0 `?ToBoolean@JSValue`, slot 85 `?ToString@JSValue`. The ~85 mangled strings at 0x19BD41C.. are that table's hint/name entries. IDA's own import list omits this descriptor, which caused the earlier "no static import" claim.
- Key slots: 0x1AC4E8C `WebConfig::WebConfig`, 0x1AC4E94 `WebCore::Initialize`, 0x1AC4E88 `WebCore::Shutdown`, 0x1AC4E90 `WebString::operator=`, 0x1AC4EA8 `WebConfig::~WebConfig`, 0x1AC4E10 `WebPreferences::WebPreferences`, 0x1AC4EE0 `SetCustomMethod` (imported, absent from the older used-API list).
- Sole init function `sub_CBCA50` (thiscall; only caller `helper_multicall_CBC410` at 0xCBC4B6): calls WebConfig ctor at 0xCBCA96, `WebCore::Initialize(&config)` at 0xCBCF0E, stores WebCore* at this+8, then vtable+0x10 on it, logs `Initialized awesomium: %s`.
- Wide literals (.rdata raw): `awesomium` 0x17C4BDC, `system\bin` 0x17C4C28, `system\bin\Plugins` 0x17C4C40, `awesomium.log` 0x17C4C68, ASCII `firefall/%s (%s)` 0x17C4C84, `FirefallProcess.exe` 0x17C4CB0, `Plugins` 0x17C4CD8.
- Logic: `<base>\FirefallProcess.exe` existence is tested with GetFileAttributesW; if present the plugin dir is `<base>\Plugins` else `<base>\system\bin[\Plugins]`. WebConfig (stack base var_E0): +0x00 int = 2 (log_level), +0x04/+0x08/+0x0C/+0x10/+0x14 WebStrings assigned via `WebString::operator=`, +0x20 byte = 1. +0x0C = path joined with `awesomium.log`; +0x14 = user agent `firefall/%s (%s)` formatted with two build strings (`VIBE_FormatSharedText`).
### Interpreted
- The WebString at +0x08 is built from the FirefallProcess.exe path and is the `child_process_path` (so the client does set it, stock `awesomium_process` is not used); +0x04 is plugin_path, +0x0C log_path, +0x14 user_agent. Field names follow Awesomium 1.7 SDK order and are inferred, not proven by symbol.
### Unresolved
- Which two strings fill `%s (%s)` and the meaning of +0x10 and +0x20 (+0x20=1 possibly a boolean flag); exact path branch semantics; the `WebPreferences` and `SetCustomMethod` call sites.