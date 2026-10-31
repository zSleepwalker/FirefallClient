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
