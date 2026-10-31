# Client main loop (App object `dword_1DFFD98`)

Static, read-only IDA analysis of `FirefallClient.exe`. Constants: `src\Startup\ClientMainLoopNativeLayout.cs`; fingerprints: `StartupBinaryEvidence.cs` ("Binary client main loop"). Related: `MatrixConnection.md` (App/login/oEngineSystem). Offsets below are IDA/file offsets (.text VA = +0x400C00).

## Observed
- Bootstrap `0x2EAE70`: `ClientApplicationInitialize(App,"",&v)` at `0x2EB18F`; on success `ClientApplicationRunLoop(App)`; otherwise a severity-5 log. `ClientApplicationShutdown` call at `0x2EB1F9`, followed by teardown helpers.
- `RunLoop 0x3098C0` (`__thiscall(App*)`): `while(!byte_1E4B0B0)` (quit flag, checked at `0x3098F1` and `0x309F79`). All per-frame work is inline; there is no separate per-frame function. Per-iteration order:
  1. `0x323E00(App+0xA88)` frame timer (reads `dword_1AC43B8`, 11-sample ring).
  2. `0xBBDB60(dword_1DFFF70)` pump: calls slot +0x14 on each object in a vector, removes finished ones via slot +0xC, then `ClientApi_PumpRoutes (0xBC4550)` on `[obj+0xA0]`.
  3. Step delta from App+0xB48 (u64), +0xB70 (float, must be >=0, assert line 2112 otherwise), +0xB74 (float), +0xB78 (one-shot byte). Then slot `+0x20` of `**dword_1E60EA4` is called with an int delta (`0x3099CD..0x3099DE`). A true return logs two strings and skips mode apply; otherwise `Client_ApplyPendingAppModes (0x30D850)` is called.
  4. Mode timeout: if `App+0xA30 != 0` and `GetTick - App+0xA2C > App+0xA30`, it logs.
  5. If `App+0xB80 == 1`, `0x30BFE0(App,str)`. Else if the byte at `App+0xC` is set, it logs and calls `0x30BFE0`. Otherwise it sets `App+0xC=1`, copies the shared state at `App+0xA28` into `App+0x10/0x14` and calls `0x30D790(App,..,0,0)`.
  6. `0x3289D0()`, then slot +8 of `dword_1E46774` (`0x309B0D`), then `0x30E1D0(App)` (status/log text builder).
  7. `0x386430(.., App+0xC64, ..)` on the non-null load-state object `[App+0xC64]` (assert line 91).
  8. Profiler-sampled section (`dword_1E0004C`, `0x481430` samples). It calls slot `+0x24` of `[App+0xB98]` at `0x309C9B` (guarded by `test ecx,ecx`), then virtual calls on the returned context and the `0x35E700` stack.
- `[App+0xB98]` is the current mode object. `Client_ApplyPendingAppModes` builds it: `AppMode_Login_Ctor 0x6365B0` (size 0xA58, default case, selector 1), `AppMode_World_Ctor 0x64AE40` (0x2904, selector 2, arg `App+0x9E8`), `AppMode_LookingGlass_Ctor 0x65A190` (0x1368, selector 3, args `App+0x9F0`, `[App+0x9E8]`). `App+0x9E8` is cleared after world/looking-glass construction (`0x30DB8A`). Any other selector hits an assert (line 3045).
- At `0x30976B` (`ClientApplicationShutdown`): `mov eax,[ecx]; push 1; call [eax]` is **vtable slot 0, the deleting destructor** with delete flag 1. It follows the slot +0x14 call (leave/exit hook) at `0x30975A`, and `App+0xB98` is zeroed immediately after.
- Shutdown then calls the `dword_1DFC82C` slot +0x64 with 0, clears `App+0xB80`, runs `0xBC4620`/`0xBBD570(0xBB8)` against `dword_1DFFF70`, and tears down `[App+0xC64]` (`0x386290`, `0x12C8D60`).
- Initialize (`0x3063D0`, large) stores `App+0xBDC = (v == 183)` (`0x306441`), and calls `0xBBD110(.., dword_1DFFF70, ..)` near its end.

## Proven App fields
`+0xC` byte first-frame flag; `+0x10/+0x14` copied shared-state words; `+0x9E8` pending-mode node; `+0xA28` shared state ptr; `+0xA2C/+0xA30` timeout start/limit; `+0xA88` frame-timer object; `+0xB48` u64 step ticks; `+0xB70/+0xB74` floats; `+0xB78` byte one-shot; `+0xB80` mode selector; `+0xB98` current mode object; `+0xC64` load-state object. Types and roles come from access patterns only.

## Interpreted
- `dword_1DFFF70` is the network/service manager (shares Initialize, pump and shutdown calls); `dword_1E60EA4` slot `+0x20` is likely the engine update/step taking milliseconds.
- `App+0xB80` is the active tf mode (1 = tfLogin, 2 = tfWorld, 3 = tfLookingGlass), and `App+0xB84` is a pending-mode ring queue. Transitions are queued by mode objects via `0x30DEB0` (e.g. `{1,0}` after a failed mode init) and applied at step 3 each frame, so mode changes occur between frames inside RunLoop.
- Slot `+0x24` returns a per-mode context used by the profiler section.

## Unresolved
- A message pump (PeekMessage), render submission and sleep/frame limiter are not in the RunLoop body. They are presumed to live in the slot `+0x20` call, `dword_1E46774` slot 2, `0x3289D0` (its decompile fails; it switches on `dword_1E6819C`) or the mode object's virtual calls. Not confirmed; this negative is scoped to the RunLoop body only.
- Class identities of `dword_1E60EA4`, `dword_1E46774` and `dword_1E002B8`.
- Producers of the `App+0x9E8` node (already indirect per `MatrixConnection.md`); the tfLogin selector cases 0/2/3/4 were not re-derived here.
- Full Initialize field map.


## Round 2: Initialize, engine step, helpers

### Observed
- `dword_1E60EA4` is a copy cell of the oEngineSystem instance slot (existing MatrixConnection.md notes). The vtable at file offset `0x187682C` has slot 8 (+0x20) = VA `0x1256590` = raw `0xE55990`, so the RunLoop engine-step call at `0x3099CD..0x3099DE` (arg = int ms delta) lands in `0xE55990` (`__thiscall(this, ms)`; vtable word checked in Startup checks).
- `0xE55990` per call: returns 1 immediately if `[this+0x12C]` is set; accumulates `ms` into the u64 at `+0x18`, increments frame counter `+0x20`, derives float `+0x24` (seconds, from `u64/50e9` times a float constant), splits ms into `+0x0C` (sec) / `+0x10` (ms remainder). If byte `+0xBC` == 0 it runs a message pump: `while (PeekMessageW(msg,0,0,0,0) && ...)` using imports `PeekMessageW` (`dword_1AC4BA4`), `GetMessageW` (`dword_1AC4BA0`), `TranslateMessage` (`dword_1AC4C38`), `DispatchMessageW` (`dword_1AC4B9C`); when GetMessageW returns 0 (WM_QUIT) it calls `dword_1E69348` vtable +0xD4 and breaks out. Then calls render-system object (`dword_1DFE970` +0x3C getter) virtuals +0x20 and +0x160, `dword_1E6A4F0` slot +8, `dword_1E69348` slot +0xF8, an optional callback at `this+0x100(ms)`, `0xE4FE10(&unk_1E6A450)`, render virtuals +0xE8(-1), +0x14C when +0x134 returns 0, per-index +0x94 loop when byte `+0xD9`, then `0xE55C80(this)`, `0xE55750(this+0x28, +0x30, +0x2C)` and an import `dword_1DF975C()` when byte `+0xDA`. Returns 0.
- So the message pump and render-side per-frame calls are in this slot, not inline in RunLoop. No `Sleep`/`timeBeginPeriod` call was seen in `0xE55990` itself (scope: that function body only, not its callees).
- `0x3289D0`: gated by `byte_1E4677A`; handles `dword_1E68204` (1 -> call imported function pointer `dword_1AC5054`, which resolves to `twitchsdk_32_release.dll`; 2 -> lookup + `dword_1E69348` +0x98 call, clear), then switches on `dword_1E6819C-1` (cases 0..3, jump table VA `0x729CF4`). Decompile fails (alignment-stack prologue `53 8B DC 83 EC 08 83 E4 F0`). Also called from bootstrap shutdown when the gate byte is set.
- `0x30E1D0`: periodic status/log text builder (float compares at `dword_1E7354C+44` / `dword_1E734E4+44`, throttle fields App+0xBD4..0xBE0, `dword_1E60EA4`, `0x2FCC90`, `0xE546A0`).
- `0x386430` (this = `[App+0xC64]`): deferred load-state step. Fields: +8 pending object, +0x10 pending id byte, +0x14 current object (swap with +8), +0x18 object deleted via virtual dtor, +0x1C state (4 -> 5, 6 on failure), +0x60/+0x61 requested id / request flag. Calls `0x3891B0`, `0x3865C0`, `0x386900` (starts a load), asserts at lines 91 and 124 ("id mismatch"), then `dword_1E002B8` vtable +0xAC(136) after the swap.
- Initialize `0x3063D0` ordered calls (from decompile): `App+0xBDC=(v==183)` (0x306441); `0x2FCC90`; `0x304170`; `0x30BE50`; `0x2EA410(0)`; `0xDD89B0`; `0x325350` (0x30695B); `0xBF2760(dword_1DFFDD8, App+0x2E8)`; `0xBEF040(dword_1DFFEF4)`; `0xBF1E70(dword_1DFFE2C)`; `0x31B2F0/0x31B370`; `0x311410(dword_1E00024+8)`; `0x3255E0`, `0x3047E0`, `0x304BB0`; `0xD713E0`, `0xDA3D40`; `0x384D90(dword_1E000D8)`; `0x3860A0`; `0x310B20(App+0xC64)` (constructs the load-state object); `0x38B310(dword_1DFFFD4,0,0,0)`; `0x3248F0/0x324850/0x3247A0`; `0xEB38B0(dword_1E00178)`; `0x38FFD0/0x38FDA0/0x3905D0(dword_1E00100)/0x390550`; `0x39A970(dword_1E00074+4)`; `0xBBD110(.., dword_1DFFF70, .., 1)` (network init); tail jump to `0x307224` (0x7F0 bytes; engine slot +0x24 and `[[1DFC82C]]+0x164`, slot +0x64 at 0x3072B2).

### Interpreted
- `0xE55990` is the oEngineSystem per-frame update (pump + render + callbacks). Name "EngineSystem_Step" is an interpretation.
- `0x386430` is a world/zone load-state manager step (interpretation only).
- `0x3289D0` is a platform/service state pump (interpretation only; Twitch import evidence is for the `dword_1E68204==1` branch only).

### Unresolved
- Writers of `dword_1E60EA4`, `dword_1E002B8`, `dword_1DFFF70` (no direct `mov ds:cell, reg` found by py_eval scan of xrefs; scoped to IDA xrefs).
- Config/cvar loading steps in Initialize are not attributed: helpers `0x304170/0x304BB0/0x3047E0/0x325350` were not decompiled.
- `0x3289D0` switch case bodies and frame limiter (no Sleep/limiter located; callees of the slot not searched).


## Round 3: Initialize helpers, tail, engine-step frame limiter

### Observed
- Frame limiter: engine step `0xE55990` calls `0xE55C80(this)` (call at `0xE55C3E`) then `0xE55750(this+0x28, this+0x30, this+0x2C)` (call at `0xE55C4D`). `0xE55C80` (`__thiscall`) reads `QueryPerformanceCounter` (import `dword_1AC43B8`) against a static baseline `qword_1EDFE08`, converts to microseconds, picks a target FPS: `this[89]` (+0x164), overridden by `[dword_1E6A7BC+60]` when > 0 (0 when negative), by `[dword_1E6A824+60]` when render virtual +0x12C returns 0 and `[dword_1E6AB64+64]==0`, clamped to 20 when render virtual +0x15C returns nonzero, and by cap `this[90]` (+0x168) when nonzero and smaller. Frame budget = `1000000 / fps` us. If the budget exceeds elapsed time (and a flag local is 0) it calls `Sleep((budget-elapsed)/1000)` (import `dword_1AC4374`, call site `0xE55DB3..0xE55DD4`), then busy-spins on QPC with `pause` until the budget is reached; it stores the new baseline. If fps is 0 there is no wait.
- `0xE55750` is an FPS counter: QPC delta, min/max frame time and frame count accumulation; once more than 200000 us (`0x30D40`) elapsed it writes average FPS to `*a1` (via a float constant), and min/max-derived FPS to the two out floats, then resets.
- Scope of Sleep search: direct-call BFS from `0xE55990` to depth 3 (21 functions, 73 indirect/virtual calls not followed) found imports `PeekMessageW` (0xE55990), `QueryPerformanceCounter` (0xE55C80, 0xE55750), `Sleep` (0xE55C80), `GetTickCount` (0x12C6B1A at depth 3). No `timeBeginPeriod`/`timeEndPeriod`/`SwitchToThread`/`WaitFor*` hit within that scope (they are imported by the binary, callers elsewhere).
- `0x304170(App)`: text-registry/option setup: runs `0x13178B0(dword_1DFFF5C)`, sets `dword_1DF96F8`, assigns shared-text options into the container at `App+0xA6C` (`this+667` dwords), tries parsing text-registry entries (`FindTextRegistryEntryByExactName`) with `0x12BB660`, merges via `0x12DB5D0`, logs severity 2/5 messages, sets `dword_1E4B0B8=1` and calls `0x12BD5A0` when one named entry exists, then `0x303E50(App)`. Returns 1. All names in the entries are obfuscated/encrypted (no readable string at the referenced addresses).
- `0x3047E0(App, outText)`: path option normalizer: looks up a value in the container at `App+0xA6C` (`0x12DC080`, `this+667`), copies into `outText` (`0x2FCC90`), expands `%...%` environment variables using wide text and an imported expander (`dword_1AC48C4`, likely ExpandEnvironmentStrings; inferred), converts `/` to `\`, validates the path (`0x12B7DE0`, `0x12BA130`), composes a subpath (`0x12BBEC0`) and checks existence (`0x12BA030`, then closes the handle via `dword_1AC49A8`); on failure appends a formatted message to the App error list (`0x30BE50`). Returns bool.
- `0x304BB0(App, outText*, outFlag*)`: if the registry-text entry named by `byte_1B40034` is absent it reads/writes a registry key (imports `dword_1AC4074/1AC4070/1AC4068/1AC4064/1AC4060`; HKEY_CURRENT_USER constant `-2147483647` = 0x80000001 appears as the root argument), compares a stored value with a constant string (`strcmp`), writes values on mismatch (install/registration record), and logs; if the entry is present it hands off to `0x304F30`. Key and value names are obfuscated (`0xDD89B0`). Role "registry install/registration record" is Interpreted from the registry-style import calls, not from names.
- `0x325350(map, result*, key*, ...)`: red-black-tree style find-or-insert for a string-keyed map (`strcmp` descent, `0x325180` insert, `0x324A70` iterator step). Container-internal, not config loading by itself.
- Tail `0x307224` (decompile header: "if the pending mode queue is empty ... enqueue Login request {1,0}, then apply pending modes"): `0x2FB6A0`, `0x12DCAD0(App+0xA6C, ...)` stores byte `App+0xBDD` (=3037), `byte_1DDA9E4 = 0x324750(App)`, `0x3BDE00(dword_1E00114)`, `[[1DFC82C]]` slot +0x64 (call `0x3072AE`, arg `dword_1E00128`), a run of subsystem refresh calls on `dword_1E00268/1E0027C/1E00164/1E00290/1E001C8/1E002A4/1E001DC/1E0018C/1DFFFC0/1DFFF1C`, `0x3090F0`, render virtuals +0x80/+0x84, `[[dword_1E60EA4]]` slot +0x54 (`0x307403`), a renderer/graphics-config build (`0x366920(dword_1DFFFE8, ...)`, `0xBF0B80(dword_1DFFE2C, ...)` with a loop over display-mode candidates), and finally the Login-mode enqueue / `Client_ApplyPendingAppModes`.

### Interpreted
- `0xE55C80` = engine frame limiter (QPC + Sleep + spin) and `0xE55750` = FPS statistics. The RunLoop therefore has its pump, render calls and limiter inside engine step slot +0x20.
- `0x3047E0` = install/data path resolver with env expansion; `0x304170` = options/command-text parse stage; `0x325350` = string-map insert helper.

### Unresolved
- Command-line parsing and cvar/config file loading: no helper in the above was identified as a file or command-line reader; option text appears pre-populated in the `App+0xA6C` container before these helpers run (not traced to its filler). Filesystem/asset mounts: not found in these helpers (scope: only 0x304170, 0x3047E0, 0x304BB0, 0x325350 and the tail 0x307224 decompiles).
- Strings are obfuscated, so key/value names (registry, options) are not recovered statically.
- Virtual (indirect) callees of the engine step (73 not followed) may contain further waits.
