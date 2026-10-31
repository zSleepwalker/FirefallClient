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
- FirefallClient.exe does not statically import awesomium.dll in the IDB import table; it holds ~85 mangled export names as strings (string-data VAs 0x19BD41C–0x19BE2CE, followed by `awesomium.dll` at 0x19BE2F8), i.e. it appears to bind exports by name at runtime. Used API: WebString, WebURL, JSValue/JSArray/JSObject (Invoke/InvokeAsync/SetProperty/GetProperty), WebConfig, WebPreferences, WebCore::Initialize/Shutdown, BitmapSurface (buffer/width/height/row_span/is_dirty), ResourceInterceptor::OnWillDownload, WebViewListener::{View,Load,Process}, WebKeyboardEvent. Client strings: `awesomium.log_callbacks`, `awesomium.async_jsinvoke`, `uiWebFrame: Awesomium process was terminated on url ...`.

## Interpreted
- The helper child is FirefallProcess.exe (stock `awesomium_process`, see FirefallProcess.md), launched via `browser-subprocess-path`; IPC is Chromium named pipes `\\.\pipe\chrome.*`.
- Contradicts earlier "86 imports" note in other docs: not reproducible from the IDB import table; the 85 mangled strings are the evidence found.

## Unresolved
- Where FirefallClient assigns `child_process_path` (not located); exact pipe name suffix format.
