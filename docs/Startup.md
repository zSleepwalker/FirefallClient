# Process startup (partial)

The PE entry RVA `0x136B9B0` translates to **raw file offset `0x136ADB0`** using the `.text` section mapping. IDA's raw-file loader did not automatically mark this actual entry as a function; its preexisting `nullsub_112` at raw `0x136B9B0` is **not** the PE entry. A new function was defined at the translated offset.

The entry thunk at `0x136ADB0` calls `0x136B4A4` to initialize the MSVC security cookie (with a corresponding inverted cookie), then jumps to `0x136ADBA`. The latter acquires the CRT startup lock via thread identity and interlocked compare/exchange, runs initializer ranges, optionally invokes the dynamic TLS callback, skips the executable name and whitespace in the command line (respecting quotes), calls the application's entry through `0x2E9630`, then follows the CRT exit/cleanup path. The raw-image decompilation misleadingly labels the passed `0x400000` **image base** as a function; it is not a call into the `MZ` header.

The CRT adapter's `0x2E9630` thunk calls application bootstrap `0x2EAE70`. The latter sets up module/base paths (nearby literal strings include `bin`, `system`, `../..`), logging and platform hooks; it initializes the application object via `0x3063D0`, logs `tfApplication::Initialize() failed` on a false return, calls the main run loop at `0x3098C0` on success, and performs application shutdown at `0x3095D0`. The run loop repeatedly updates services and computes a frame/timing value until its quit flag is set.

The per-frame call at `0x30D850` (`Client_ApplyPendingAppModes`) drains a pending app-mode queue, shuts down the previous mode, constructs and initializes the next, then records the active mode and transition timestamp. Its dispatch maps three observed values: mode `1` allocates `0xA58` bytes and calls `AppMode_Login_Ctor` (`0x6365B0`); mode `2` allocates `0x2904` bytes and calls `AppMode_World_Ctor` (`0x64AE40`); mode `3` allocates `0x1368` bytes and calls `AppMode_LookingGlass_Ctor` (`0x65A190`). Mode 1's identity is independently supported by the failure text at raw `0x173FF34`, `Failed to initialize AppMode_Login`.

Pending requests are inserted by raw `0x30DEB0` as `{modeId, payload}` DWORD pairs in the application queue at `+0xB84` (head/index `+0xB90`, count `+0xB94`). The request layout and constructor mapping are represented by `AppModeCatalog.KnownPendingRequests`. During application initialization, helper `0x307224` checks the pending queue count; if it is zero in the guarded initialization path, it calls `AppMode_EnqueueModeIdWithZeroPayload` (`0x324080`) to enqueue mode ID 1 with zero payload, then calls `Client_ApplyPendingAppModes` immediately. Thus the observed default initial mode is Login when initialization has no previously queued mode request; this is distinct from, and does not prove, the unresolved Login-response-to-World transition. See [`LoginMode.md`](LoginMode.md) for that callback boundary.

A second request path at raw `0x30D790` first invokes a state-update helper, then checks the selected remote-observer object. If no selected object exists, its `+0x820` field is null, or its signed `+0x0C` value is at most 3, the helper enqueues `{1,0}`; otherwise it calls a separate routine on that object. The selected object's meaning and the reason this path falls back to Login remain unknown. It has multiple callers across application and mode code, so this is a separate request source from the startup default.

**RTTI confirmation for all three names:** each constructor installs a vtable pointer (`0x1C676C0`, `0x1C677BC`, or `0x1C6781C`). The preceding vtable COL slots map to raw `0x18662BC`, `0x18663B8`, and `0x1866418`; their stored COL VAs are `0x1CA69F0`, `0x1CA6C4C`, and `0x1CA6FB4`. Applying the PE section map in `BinaryLayout.md` yields type descriptors at raw `0x1A10CC4`, `0x1A10E9C`, and `0x1A11048`, whose decorated names are respectively `.?AVtfLogin@@`, `.?AVtfWorld@@`, and `.?AVtfLookingGlass@@`. The matching class-hierarchy descriptors expose base names: `tfLogin` and `tfWorld` each include `tfAppMode`; `tfLookingGlass` includes `tfAppMode` and `nl::ReplayReader::GameInterface`. Their additional `sl::callbackable` bases are documented in the hierarchy data but do not establish further user-facing mode behavior. These names follow the RTTI attached to the vtables actually installed by the three dispatch constructors, not guesses from class size.

The `tfLookingGlass` object contains a second interface subobject at `+4`: its vtable is VA `0x1C6786C` (raw `0x186646C`), and its RTTI type descriptor is VA `0x1E12AB4` (raw `0x1A10EB4`), naming `nl::ReplayReader::GameInterface`. That interface's vtable entries 0–3 resolve to raw `0x661473`, `0x65E870`, `0x65E940`, and `0x65EB80`; entry 0 is an adjustor thunk (`sub ecx, 4; jmp 0x65A440`). The interface subobject offset and vtable addresses are represented in `AppModeCatalog.KnownInterfaces`.

Modes outside 1–3 take an error path rather than selecting another constructor in this dispatcher. The evidence supports these three mappings only; it does not establish the full set of app-mode values used in other builds or code paths. `src\Startup\AppModeCatalog.cs` stores the observed IDs, native type names, allocation sizes, constructor offsets, and RTTI addresses.

After construction, the dispatcher calls the new object's virtual slot `+4` as its initializer. On success it stores the active-mode pointer and accepts the transition; on failure it invokes the object's virtual shutdown slot `+20`, releases it, and returns through the pending-transition cleanup path. Mode `1` additionally calls a platform/service virtual slot `+72` after the login object is created. These observations identify the dispatch and lifecycle boundary, but not every transition-state semantic.

Mode-specific traces are documented separately: [`LoginMode.md`](LoginMode.md) covers `tfLogin`, and [`WorldMode.md`](WorldMode.md) covers the mode-2 constructor, scene setup, and layer-switching virtuals. LookingGlass replay-reader and end-of-replay behavior are described above; its file format remains unmodeled.

The LookingGlass initializer at raw `0x65A880` references `LookingGlass.xml` and proceeds through raw `0x65D520` to build a replay reader at object offset `+0x1334` (4916), represented by `LookingGlassNativeLayout.ReplayReaderPointerOffset`. It passes the `GameInterface` subobject at `this+4` to raw `0x381A10`. Reader initialization at `0x65D880` logs either `Replay loaded, recorded from` or an error (`Error initializing replay reader` / protocol error); the caller at `0x65E870` reports `Unable to open replay file(s) at: %s` and cleans up on failure. The available evidence describes reader setup, not the replay file format.

When mode 3 is queued, `AppMode_QueueLookingGlassMode` first copies a `0x34`-byte request-context region to application `+0x9F0`, then inserts `{3,0}`. The dispatcher passes that region as the LookingGlass constructor's second argument; `AppMode_LookingGlass_Ctor` (`0x65A190`) copies it to mode object `+0x18`, preserving shared-text references at context offsets `+0x14` and `+0x1C`. The dispatcher also passes the DWORD at application `+0x9E8` as a separate constructor argument, which the constructor stores at mode `+0x58`. The meaning of the context's opaque fields and that extra argument is not established. Constants for the observed transfer are in `LookingGlassNativeLayout`.

Replay completion is handled by raw `0x65C360`. When bit `8` at object offset `+64` is set, it logs `Auto-replay has finished; requesting quit.` (string raw `0x1762E90`) and calls mode cleanup/transition code at `0x65E4B0`. The UI binding string `GoLookingGlass` (raw `0x1761D5C`, reference at `0x642F8B`) reaches raw `0x641E40` through thunk `0x645EC0`; that handler logs `Looking glass from UI has been disabled.` and returns `0` (with a separate usage/argument branch). Separately, the helper at `0x30D590` queues mode 3 from six static call sites: `0x306050`, `0x3D60A0`, `0x6385C0`, `0x65D400`, `0x6AE3E0`, and `0x6AF120`. Several observed paths directly rebuild replay-reader state if mode 3 is already active; otherwise they enqueue a LookingGlass request. Thus only the named UI binding is disabled; other native routes remain present, though their user-facing entry conditions are not all resolved.

`AppMode_TryQueueLookingGlassRequest` (`0x306050`) accepts a text value and two DWORDs, performs registry-dependent text handling, then builds a request for the shared queue helper. The call-site offsets and cautious descriptions are recorded in `AppModeCatalog.KnownLookingGlassQueueCallSites`; they are native call-site evidence, not proof of a complete user-facing workflow.

IDA names assigned during this investigation: `ClientProcessEntryThunk` (`0x136ADB0`), `CrtInitializeSecurityCookie` (`0x136B4A4`), `CrtInitializeAndInvokeClient` (`0x136ADBA`), `ClientApplicationBootstrap` (`0x2EAE70`), `ClientApplicationInitialize` (`0x3063D0`), `ClientApplicationRunLoop` (`0x3098C0`), `Client_ApplyPendingAppModes` (`0x30D850`), `AppMode_EnqueuePendingModeRequest` (`0x30DEB0`), `AppMode_EnqueueModeIdWithZeroPayload` (`0x324080`), `AppMode_QueueLookingGlassMode` (`0x30D590`), `AppMode_TryQueueLookingGlassRequest` (`0x306050`), `AppMode_Login_Ctor` (`0x6365B0`), `AppMode_World_Ctor` (`0x64AE40`), `AppMode_LookingGlass_Ctor` (`0x65A190`), and `ClientApplicationShutdown` (`0x3095D0`). Follow-up names/comments include `AppMode_LookingGlass_Initialize` (`0x65A880`), `LookingGlass_CreateReplayReader` (`0x65D520`), `ReplayReader_Initialize` (`0x65D880`), `LookingGlass_LoadReplayReader` (`0x65E870`), `LookingGlass_UiEntryDisabled` (`0x641E40`), `TfWorld_Initialize` (`0x64B460`), and `TfWorld_SetLayerMode` (`0x64C020`). These names describe observed behavior, not exact original symbols.

Login-specific IDA names/comments include `AppMode_Login_Initialize` (`0x636FF0`), `AppMode_Login_Shutdown` (`0x63B360`), `Login_HandleLoginResponse` (`0x63B7C0`), `Login_HandleCharacterRefreshResponse` (`0x63C5F0`), `Login_HandleZoneListResponse` (`0x63D270`), `Login_PickBackgroundZone` (`0x639840`), `Login_LoadAutoLoginSettings` (`0x63D760`), `Login_StartAutoLogin` (`0x63DE80`), and `Login_QueueLoginRequest` (`0x63B550`). The `tfWorld` slot-11 record handler is named `TfWorld_ApplyRecordInput` (`0x64DCF0`); its offset-only member getters are `TfWorld_GetMember_2E8` (`0x64A0C0`) and `TfWorld_GetMember_2EC` (`0x64A0D0`).

### Scripting-object teardown and payload callbacks

The startup scripting-object cleanup at raw file offset `0xC33060` closes its VM through the call at `0xC33291` to `0xCD5510` before iterating the object's payload containers. The payload passes visit container offsets, in order, `+0xF0`, `+0xE8`, `+0xE0`, `+0xD8`, `+0x100`, `+0x110`, `+0x118`, and `+0x140`; cleanup then resets those same containers in that order. The `+0x118` reset uses `0xC0BE90` in the block at `0xC33973..0xC33993`, which also resets its head/count state.

Two observed payload dispatches are recorded narrowly. The `+0xE8` loop at `0xC33326` dispatches at `0xC3335D` through virtual slot byte offset `0` with explicit argument value `1`. The `+0x118` loop at `0xC336DA` obtains its payload from node offset `+0x14`, then dispatches in `0xC33706..0xC33711` through virtual slot byte offset `0`, again with explicit argument value `1`. The node payload offset, virtual slot offset, and explicit argument are separate quantities. This cleanup dispatch is not the callback walker at `0xC376A0`, whose observed virtual slot byte offset is `+0x14`; neither observation identifies an unresolved indirect target as a concrete destructor.

After the ordered container resets, the path calls helper `0xC3FC10` for container `+0x120` at `0xC33C31` (receiver LEA at `0xC33C2B`), then uses `0xC0BD90` for container `+0x118` and performs a generic stored-object release in `0xC33C3A..0xC33C58`. The later `0xC3FC10` body frees a conditional backing allocation at container `+8`, then list nodes and sentinel; it never reads per-node `+8` callback objects. Its direct allocator transfers are raw `0xC3FC1A`, `0xC3FC52`, and `0xC3FC61`. This is not proof of callback-object destruction. The other later helpers/releases remain unverified here.

The nested `+0x120` container is initialized by `0xC41670` from the caller at `0xC32A79` (receiver setup at `0xC32A73`). The observed fields are head `+0`, count `+4`, backing begin `+8`, backing end `+0xC`, and capacity `+0x10`; these are zeroed before initialization. A separate 12-byte sentinel allocation at `0xC52A10` has self-links at sentinel-relative `+0` and `+4`. The separate backing helper `0xC53290` receives count `16` and a source pointing to the local sentinel pointer; the capacity-times-four calculation is at `0xC52D10..0xC52D23`, yielding 16 four-byte slots (0x40 bytes), which are filled at `0xC52E50` with the captured sentinel runtime pointer. This is a backing array plus circular list; the evidence does not establish a hash-container interpretation.

The additional container fields at `+0x14`, `+0x18`, and `+0x1C` hold observed values `7`, `8`, and bit pattern `0x3F800000`; they are kept as neutral structural values, not labeled as mask, bucket count, or load factor. Container-relative `+8` is the backing-begin field, distinct from node-relative `+8`; independent ownership and release of the latter's callback object remain unresolved. `ProjectContainer120InitialBackingSlots` is only a pure, read-only snapshot projection from a captured sentinel pointer to sixteen repeated slot values. It does not perform native allocation or emulate native state, and does not infer a value for the node-relative `+8` field.

The manager's `+0x6B0` destructor path is bounded to `0xBF8AC0 -> 0xC0BD90 -> 0xC0EB00`: the observed node cleanup decrements the key reference at node offset `+0x10` and frees the node. This evidence does not establish destruction of the node's payload at `+0x14`. On the insertion-failure path, anchors `0xC3C9B0` and `0xC64420` lead to payload virtual slot byte offset `0` with explicit argument value `1`; successful insertion ownership remains unresolved. In particular, the static evidence does not justify calling successful payloads leaked or assigning a global ownership rule.

`StartupObjectLifecycleNativeLayout` holds the read-only ordered offsets and dispatch facts. `Startup.Checks` validates the VM-close and later-helper relative calls when given a binary path, along with in-memory order and slot-versus-argument checks. Run the quiet checks with `dotnet run --project src\Startup\Checks\Startup.Checks.csproj -v:q`.

The `tfLogin` constructor builds three callback wrappers whose stored target
PE VAs map to the Login, character-refresh, and zone-list handlers above. The
three request paths attach their matching wrappers to the shared request
layout. The WinHTTP worker acquires a strong reference from the stored weak
callback pair after completion and invokes slot `+4`; the trampoline tail-calls
the stored target with the Login instance as `this`. WinHTTP's HANDLE_CLOSING
callback clears the live request handle to enable delivery; thread affinity
and detailed cancellation semantics remain unresolved. See
[`LoginMode.md`](LoginMode.md) for request verbs, delivery gates,
zone-list selection, and selected-character connection preparation.

The selected-character path subsequently requests an oracle ticket and opens
an overlapped UDP game socket. Its callback wrapper, scheduled retry policy,
local-port candidates, and initial receive boundary are documented in
[`MatrixConnection.md`](MatrixConnection.md); a successful socket open does
not establish a World-mode transition.

`TfWorld_Shutdown` (`0x64B950`) was identified as the app-mode shutdown/deactivation slot. Evidence comments at `0x64BC40` preserve the observer handler's bounded behavior; `TfWorld_ApplyRecordInput` (`0x64DCF0`) names the record handler by its observed action without guessing the record's semantic type.

## UI manager initialization and payload teardown

The UI manager constructor at raw file offset `0xBF7A80` sets the word at `+0x670` to `1`, with the adjacent initialization bytes at `+0x671` and `+0x672` initially zero. Its initializer at `0xBFB310` first calls vtable slot `+4`, whose target `0xBFA0C0` returns the byte at `+0x671`. A nonzero result rejects the already-initialized object; zero allows initialization to proceed. This is an already-initialized guard, not an initialization failure or parent destruction. The continuing path reaches `0xBFB350`, which sets `+0x671` to `1`; a later failure leaves that partial-initialization byte set. Success reaches `0xBFB8DC` and sets `+0x672` to `1`. The managed snapshot records only the observed post-guard outcomes, not a general initializer simulation.

On the successful UI-manager initialization path, raw `0xBFB310` receives a descriptor pointer as its eighth explicit stack argument (`[EBP+0x24]`). The call at `0xBFB8C6` invokes virtual slot `+0x30` on the object pointer at UI-manager `+0x974`, with argument `1`; its return value is ignored. The initializer then stores the original descriptor pointer verbatim at UI-manager offset `+0xA08` (`0xBFB8CE`) before setting the ready byte at `+0x672` (`0xBFB8DC`). A separate caller at `0x30B1B4` obtains a pointer through an indirect getter and adds `0x48` (an interior address, not a dereferenced field); the concrete source object remains unresolved.

### UI state-sync projection

Raw `0xBFFA90` first calls the UI-manager vtable slot `+4`, which returns byte `+0x671`. It returns `0` unless that getter result and ready byte `+0x672` are both nonzero; once both gates pass, it reaches a return of `1`. The reached path calls slot `+0x1C` on the object at UI-manager `+0x974`; that component callback's return is ignored. This is a callee-side gate, not evidence that every caller reaches it only after readiness.

The caller at raw `0xBFF930` writes fields `+0x58C`, `+0x590`, and `+0x594`, then calls UI-manager slot `+0xB0` at `0xBFFA3F`. In the callee, the selected value starts at `+0x58C`; it is retained only when byte `+0x590` and that value are both nonzero. Otherwise it falls back to `+0x588`, and a zero result falls back to `+0x584`. The callee calls the imported `USER32.dll!GetCursorInfo` through IAT VA `0x1AC4C24` (raw file offset `0x16C3824`). Its local `CURSORINFO` record is 20 bytes: `cbSize` at `+0` is initialized to 20, and the remaining 16 bytes are zeroed before the call; `flags` is at `+4`, `hCursor` at `+8`, and the point coordinates at `+12` and `+16`. The API's `BOOL` return is ignored. The component call uses arguments `(1, selected)` when byte `+0x670` is zero, or when byte `+0x598` is nonzero and captured `CursorFlags` equals exactly `0` or captured `CursorHandle` differs from `selected`. It uses `(0, 0)` when `+0x670` is nonzero, `+0x598` is zero, and captured `CursorFlags` equals exactly `1`; all other cases make no component call. These are exact comparisons, not bit tests.

`UiManagerNativeLayout.ProjectStateSync` is a pure projection of captured bytes, fields, cursor flags, and a 32-bit cursor handle. It consumes snapshots only and does not call Win32. Because the API return is ignored, a false return does not imply that the output record remains zero; the captured output fields are still the projection inputs. Other UI-manager fields retain neutral offset-based names where their meanings are not established.

The cursor-cache caller at raw `0xBFF930` loads the current version from `+0x990` at `0xBFF958` and compares it with cached `+0x594` at `0xBFF95E`. Equality returns that loaded version at `0xBFFA45`, not the cached payload, and preserves `+0x58C`, `+0x590`, and `+0x594` without lookup or synchronization. On mismatch it performs one key lookup in tree `+0x6C8`; the payload is node `+0x14` or zero for a missing key. It stores that payload at `+0x58C` even on a miss, stores its second explicit argument byte at `+0x590`, records the current version at `+0x594`, then calls state sync through slot `+0xB0` and passes through its result. Separately, virtual slot `+0x40` reaches `0xC02730`, which increments the 32-bit value at `+0x990` at `0xC02816`.

The manager routine at raw `0xC02730` is the target of vtable slot `+0x40` (raw table entry `0x1872AFC`, bytes `30 33 00 01`, VA `0x1003330`). It first increments a separate global counter at `0xC02765`; that global is not part of this projection. The ready-byte check reads object byte `+0x672` at `0xC0276B..0xC02772`. A zero byte branches to epilogue `0xC041EC`, bypassing the object's version increment and argument stores. For every nonzero byte, the routine calls `0xC06670` before those writes; this helper reads the old adjacent QWORD at `+0x988/+0x98C`. The one-time global-initialization branch reconverges on the same normal path. That path increments the object's DWORD `+0x990` once at `0xC02816` (modulo 2^32), then stores explicit argument one to DWORD `+0x988` at `0xC0281C` and explicit argument two to DWORD `+0x98C` at `0xC02825`. These full-width words are not the cursor lookup key or cached argument byte. Calls at `0xC02831` and `0xC0284E` use slots `+0x40` and `+0x44` of component pointer `+0x974`; the arguments there are temporary-buffer pointers, not the two stored words.

`UiCursorCacheProjectionRules` compares only captured 32-bit versions and projects cache-hit versus lookup-required state; its hit payload describes available cached state, not the native EAX return. `RefreshAfterLookup` projects a completed lookup, mapping no payload to zero and storing the supplied byte and version. `ProjectInvalidation` is a separate pure projection of captured `(+0x990, +0x988, +0x98C)`, the ready byte, and the two full-width arguments: ready zero returns a typed skipped decision and preserves all captured fields; any nonzero byte projects one wrapping version increment and argument-one/argument-two stores in order. It does not execute `0xC06670`, the component calls, or the one-time global initialization, nor model the independent global counter. Its updated decision describes the normal enabled write path if preceding callbacks return; it does not assert that a callback completes or that the native routine returns normally. The cache-hit/refresh methods do not perform key lookup or state sync, infer key-based invalidation or frame cadence, or model a version increment; the separate invalidation projection models only the captured write path. On a cache hit the new argument byte remains irrelevant and the remembered byte is unchanged; native EAX is still the loaded version. Version comparison is uint equality, so wrapped zero differs from prior `uint.MaxValue`.

### UI old-pair callback gate

The enabled invalidation path calls raw `0xC06670` from `0xC02778` after the ready-byte `+0x672` check and before storing the new arguments at `0xC0281C` and `0xC02825`. The helper combines the pre-store DWORDs at `+0x988` (low) and `+0x98C` (high) into an unsigned QWORD, divides by `1000` through local helper `0x159DD10` (call at `0xC06686`, bytes `E8 85 76 99 00`), then uses the quotient's low DWORD. It subtracts captured field `+0x20` with uint wraparound and proceeds when the unsigned elapsed value is at least `5000`; otherwise field `+0x20` remains unchanged. On the proceeding path it stores the quotient low DWORD to `+0x20` at raw `0xC0669C` before callback dispatch. It then walks the trees at `+0x6B0` and `+0x6D8` in order, followed by tree `+0x5E4` through raw helper `0xC376A0`.

The first two in-order tree walks dispatch indirectly at raw code ranges `0xC066B0..0xC066B5` and `0xC06710..0xC06715`: each uses the receiver at node `+0x14`, virtual slot byte offset `+0x14`, and zero explicit callback arguments. These ranges are dispatch-site addresses, not fixed callback targets. After each callback returns, the walk checks the node byte at offset `+0x0D`, then reads successor links at offsets `+0x8`, `+0x0`, and `+0x4` and continues the successor walk; it rereads the head and compares it afterward. The inspected sequence does not capture the successor before calling back.

The third dispatch site, raw `0xC06770..0xC06773`, calls the direct method target at raw `0xC376A0` for tree `+0x5E4`; the call-site range and method target are distinct addresses. That method's nested dispatch order is trees `+0xE8` and `+0x118` (receiver at each node `+0x14`, virtual slot `+0x14`), then container `+0x120` (entry at `+0x8`, virtual slot `+0x14`).

`UiOldPairCallbackGateProjectionRules.Project` models only the captured arithmetic, threshold decision, and conditional `+0x20` value. Its `ulong` division gives the exact unsigned quotient; it neither executes callbacks nor infers clock units or cadence. The traversal evidence is a read-only static record sequence and does not simulate callback execution or tree mutation. These observations do not establish a snapshot, reference-counting discipline, or thread-safety guarantee. The ready-byte check is a caller-side precondition and is not added as an input or a second gate in this projection.

### UI angle-correction projection

Derived-state helper raw `0xCCA190` calls through raw stub `0x159DD00` (`FF 25 20 48 AC 01`) to `MSVCR120.dll!_CIfmod`, IAT VA `0x1AC4820` / raw `0x16C3420`. The native code forms the original sum from input float `+0x8` and parent float `+0x14`, then obtains a captured remainder through that helper. This projection accepts the original sum and remainder as inputs; it deliberately does not emulate `_CIfmod` or replace its x87 behavior with `%` or another remainder shortcut.

The correction is selected only when the original sum is ordered-negative and the captured remainder is not exactly zero. In that case the code adds the captured single-precision `2π`; the constant is VA `0x1C5E1C4` / raw `0x185CDC4`, bytes `DB 0F C9 40`, float bits `0x40C90FDB`. Zero is VA `0x1C817A4` / raw `0x18803A4`, and one is VA `0x1C81BE8` / raw `0x18807E8`. Returning either signed zero unchanged preserves its sign, including a negative exact-multiple remainder. A NaN original sum does not select correction; a NaN remainder under a negative sum passes the nonzero test and remains NaN after addition, without any promise about payload bits.

The float result is stored at `+0x14`. A separate output byte at `+0x24` is true exactly when input byte `+1` and parent byte `+0x24` are both nonzero. `UiManagerAngleCorrectionProjectionRules.Project` models only these captured-remainder correction and boolean predicates; it does not claim to model the native remainder call.

### UI payload producer gate projection

The derived-state method at raw file offset `0xCCA190` provides a captured output region with nine float slots beginning at object offset `+0x2DC` (ending at `+0x2FF`), followed by byte `+0x300`. Producer raw file offset `0xC95400` (code VA `0x1096000`) calls vtable slot `+0x34` at raw `0xC9544D` before evaluating its early gates. It returns zero for byte `+0x300 == 0` at raw `0xC95456..0xC95458`. Otherwise, the `COMISS` sequence at raw `0xC9545E..0xC9546D` compares the global threshold with the captured float at `+0x2EC`; the following `JA` returns only when the ordered comparison says global threshold is greater than the field. If that comparison completes unordered, `JA` is not taken; this projection does not emulate floating-point exceptions.

Either early rejection skips the later virtual calls at slots `+0x20` and `+0x28`, the conditional virtual call at `+0x74`, and descriptor append. Passing those gates is not itself append eligibility: the later path still requires the input object's byte `+0x72C` to be nonzero and the UI-manager descriptor pointer at `+0xA08` to be nonnull. The UI-manager global at VA `0x1E002B8` is dereferenced without a null guard. `UiPayloadProducerGateProjectionRules.Project` accepts captured values only, reports early-gate and append-gate decisions separately, and does not execute the derived-state helper, virtual calls, or append. It neither models an overall native return value nor treats a zero result as successful fallback; append-gate passage does not assert that a record was actually appended. Tests cover every byte value for the two byte gates, ordered finite comparisons, NaN/unordered operands, and infinities.

The append helper at raw `0xC97380` receives the descriptor pointer as `ECX`. Its observed header has begin, end, and capacity pointers at offsets `+0`, `+4`, and `+8`. It copies a local four-DWORD record sourced from fields `+0x338`, `+0x33C`, `+0x340`, and `+0x344` to the current end and advances the end by `0x10`; local record order is `+0x33C`, `+0x338`, `+0x340`, `+0x344`, as established by stores at raw `0xC9553A..0xC95549` to the record passed from `EBP-0x34`. The full-capacity path reaches raw `0xC978F0`. These offsets support only a positional 16-byte native-record projection (the four source fields use bit-preserving MOVSS transfers), not semantic field names, rendering meaning, ownership rules, or a wire format.

The bounded `NativeRecordVectorCapacityProjectionRules` projects count arithmetic only. For validated counts no greater than `0x0FFFFFFF`, it reports the maximum-used-count guard before increment; otherwise it computes `required = used + 1`, then chooses `max(required, oldCapacity + oldCapacity/2)` when that addition stays within the maximum, and `required` when it would exceed the maximum. Its byte count is the projected count multiplied by 16, which fits in `uint` throughout the validated range. Counts above the maximum are rejected by this managed projection with `ArgumentOutOfRangeException`; that is input validation for the projection, not a claim about a native exception. `C97380` separately rebases an alias source offset after growth; this projection does not model pointers or aliasing.

The native failure evidence is separate from that arithmetic projection. At raw `0xC978F0`, the full-capacity path for `used == 0x0FFFFFFF` pushes `"vector<T> too long"` (VA `0x1C5EA14`) and calls `MSVCP120.dll!std::_Xlength_error(char const*)` through IAT VA `0x1AC45C0` at raw `0xC9791C`. This is consistent with the normal CRT contract of throwing `std::length_error`, but the EXE proves the call site, import, and message—not the external CRT implementation or its behavior at runtime. A separate count guard at raw `0xC97FA0` rejects `count > 0x0FFFFFFF`; the growth allocation helper at raw `0x12B7FB0` calls the allocator at raw `0x12C8A20`. The caller's null-result branch calls `MSVCP120.dll!std::_Xbad_alloc(void)` through IAT VA `0x1AC4424` at raw `0xC97FFB`. The normal CRT contract is to throw `std::bad_alloc`; if that imported call hypothetically returned, native control would continue at raw `0xC98001`. That fallthrough is not evidence that the old allocation remains preserved. The projection neither simulates these exceptions nor models hypothetical writes or allocation-failure behavior. Its `MaximumUsedCountGuardInvoked` decision name describes a projection result only; it is not a report of native runtime execution or a guard side effect. The projected capacity is an arithmetic request only and does not assert allocation success or failure.

Separately, one concrete cursor registration at raw `0xBFD6D0` calls `LoadCursorA(NULL, 0x7F8A)` through IAT VA `0x1AC4C2C`, then inserts into the tree at UI-manager `+0x6C8` through raw `0xC06D40`. The observed node constructor at raw `0xC1675D` stores the key at node `+0x10` and payload at `+0x14`. This records one concrete registration only, not all cursor-tree payloads or registration behavior.

The startup path calls `0x30D150` from `0x30B204` only when `0x30B200` returns zero; a nonzero result branches to `0x30B232` and skips that call. `0x30D150` tail-dispatches UI vtable slot `+0x34` to `0xBFA0D0`. This is the gated bulk-shutdown method. It tests only byte `+0x671` (`0xBFA102`) and, when nonzero, clears the two-byte state beginning there (`0xBFA112`); when nonzero, it drains the payload tree at `+0x5E4`. For a node value at `+0x14`, the observed calls are internal close (`0xC33060`), native object free (`0x12C8D60`), identity removal (`0xBFCF30`), then key release (`0xC09D10`), called from `0xBFA278`, `0xBFA27F`, `0xBFA28B`, and `0xBFA29C`, respectively. Later tree clearing and pointer-set handling are separate operations. Within this same gated shutdown, a nonnull child pointer at manager `+0x24` is passed to reset method `0xCA7330` (call site `0xBFAD3B`); the caller then reloads and null-gates helper `0xBFAF40` (call site `0xBFAD48`) before clearing the manager field at `0xBFAD4D`. Thus this child path is within `0xBFA0D0`, not a separate `0xBF8AC0` destructor path.

The static `0xCA7330` reset layout records an eight-pair loop over child DWORD-pointer fields `+0x24, +0x28, ... +0x60` (16 ordered offsets). For each nonzero field, the observed path carries its value in ECX and on the stack, dispatches indirectly through virtual slot `+8`, then zeros that field. The two early call/store anchors are `0xCA734A`/`0xCA734D` and `0xCA735D`/`0xCA7360`. The later ordered fields are `+8`, `+0x14`, `+0x0C`, `+0x10`, and conditional `+0x74`; these also use slot `+8` and clear their fields. The child `+8` store at `0xCA7379` (`MOV [EDI+8], EBX`) uses the exhausted eight-pair loop counter, which is zero. Child `+0x13C` is checked for a conditional diagnostic, not cleared. The method loads child `+0x64` into `ECX` and calls `0x12C8D60` at `0xCA73F2`, then zeros child `+0x68` at `0xCA73F7` and returns at `0xCA7401`; it does not show a clear of `+0x64`. No meaning is assigned to `EDX` based on decompiler fastcall labeling.

The ordered `Child24ShutdownSequence` is a static trace that flattens the helper call and its internal steps, not an unconditional execution plan. Helper `0xBFAF40` uses ECX as the child receiver; the caller's pushed copy is unread, and the helper returns with `ret 4` at `0xBFAF90`. Its child `+0x150` cleanup call (`0xBFAF50 -> 0xCA6070`) and native free (`0xBFAF57 -> 0x12C8D60`) are conditional on that field being nonnull. It dispatches child `+4` through virtual slot `+8` at `0xBFAF82`, then frees the child itself at `0xBFAF87`. The child `+4` diagnostic does not establish an abort; its indirect virtual target is unresolved. The separate free calls do not prove that the two pointer values cannot alias. Child `+4` and `+0x150` are accessed by this helper, not directly by `0xCA7330`; called-target effects are not generalized. These observations do not establish reference-count semantics. Separately, slot `0xBF82A0` reaches the `0xBF8AC0` destructor, which releases copied-text fields at `+0x5C8` and `+0x34` via `0x30A9D0`; ownership of fields `+0x974` and `+0xA08` remains unresolved.

The nested cleanup at raw `0xCA6070` receives its object through outer-child `+0x150`. It conditionally calls the nonzero nested `+0x1C` pointer through virtual slot `+8`, then clears `+0x1C`; for nonzero `+0x10`, it calls `0x12C8D60` at `0xCA60BB`, then clears `+0x10`, `+0x14`, and `+0x18`; for nonzero `+4`, it calls `0x12C8D60` at `0xCA60DC`, then clears `+4`, `+8`, and `+0x0C`. Each group's clears are skipped when its initial pointer is zero. The nested object's own storage is freed by caller `0xBFAF57`, not by `0xCA6070`; outer-child storage is separately freed at `0xBFAF87`. This static trace establishes no ownership or pointer-alias contract.

The child constructor at raw `0xCA6370` zeros fields `+0x64` and `+0x68` (`0xCA63D9`, `0xCA63E0`). Its initializer at `0xCA66F0` passes allocation size `0x800` at `0xCA6748` to allocator `0x12C8A20` (`0xCA674D`), stores the returned pointer at `+0x64` (`0xCA67A0`), writes `0x20` to `+0x68` (`0xCA67A3`), and zeros `+0x6C` (`0xCA67AA`). If `+0x64` was already nonzero, the observed diagnostic path continues to the allocation and pointer store if the diagnostic returns; that observation does not prove a failure or ownership contract. The `+0x68` and `+0x6C` names remain offset-only facts: the `0x800` request does not establish a record size or semantic capacity/count.

The backing allocation continuation is projected separately by `UiChildBackingAllocationProjectionRules.ProjectAtContinuation`. A zero captured allocator result reaches the join at raw `0xCA6799` only if its diagnostic returns; without that captured return, the projection is absent. At the join, child `+0x64` receives the captured result (including zero), `+0x68` receives `0x20`, and `+0x6C` receives zero. The later diagnostic branch at `0xCA67B1` uses the comparison of child `+0x74`, not another allocation-success test. This is a captured continuation, not a native OOM implementation or a successful-initialization result.

The child initializer's result is a separate captured two-helper gate in `UiChildInitializationResultProjectionRules`. A zero result from `0xCA6C20` (called at raw `0xCA6A61`) directly clears child `+0x6C`, returns zero, and skips the second helper. Otherwise it calls `0xCA6C80` at `0xCA6AA3` and normalizes its captured byte result to zero or one. Missing or impossible second-helper captures are rejected. The UI caller at `0xBFB542` tests this result: zero reaches the zero-result epilogue at `0xBFB668`, while nonzero continues initialization without yet setting ready `+0x672`. The projection does not infer indirect-helper side effects or treat continuation as full UI success.

The final tests in required-lookup helpers `0xCA7090` and `0xCA71F0` are captured by `UiChildRequiredLookupProjectionRules`: the signed dispatcher status must be nonnegative and the captured output DWORD nonzero. This predicate applies only after reaching that terminal stage; it does not replace earlier query gates. The helpers dispatch through dependency virtual slots `+0x16C` and `+0x1A8`, respectively. Output writes occur inside unresolved targets that receive the caller's output address; these helper bodies do not themselves initialize or clear it. No output-retention or refcount contract is inferred.

The success replacement loop in `0xCA6C80` writes eight captured scratch pairs to child offsets `+0x24/+0x28` through `+0x5C/+0x60`. `UiChildPairReplacementProjectionRules` copies exactly sixteen captured DWORDs into an immutable ordered destination-write list; it neither broadcasts a pair nor models old fields. Native callout order remains a separate boundary: the old second field is read after the first old-field virtual `+8` call, but before the first new-value store at `0xCA6F0A`; its own virtual call precedes the second new-value store at `0xCA6F22`. Thus the projection is not an atomic old-state snapshot or a native cleanup emulator.

The eleven required lookup gates in `0xCA6C80` share a failure target at `0xCA6F50`, before the destination replacement loop. This path conditionally dispatches each nonzero pointer in eight scratch pairs through virtual slot `+8`, then clears that scratch field. The initial direct-call failure bypasses this scratch cleanup. Failure sets BL to zero at `0xCA6FCF`, success sets BL to one at `0xCA6F49`, and the common tail transfers BL to AL at `0xCA7061`. `UiChildScratchCleanupNativeLayout` records these static boundaries without assigning virtual-call ownership semantics or ruling out called-target side effects.

The earlier calls through IAT VA `0x1AC40AC` are `D3DCOMPILER_43.dll!D3DCompile`, not generic status adapters. `UiChildShaderCompileProjectionRules` records the eleven captured arguments for raw calls `0xCA70FA` and `0xCA725A`: source pointer from virtual `+4`, source size from virtual `+0xC`, null source-name/defines/include pointers, entry/profile `general_drawing_vs`/`vs_3_0` or `general_drawing_ps`/`ps_3_0`, zero flags, and captured local code/error blob output addresses. The pure projection does not invoke compilation, validate pointers, identify the virtual implementations, or infer blob ownership.

The compiler blob cleanup paths remain distinct in `UiChildShaderBlobCleanupNativeLayout`. Negative compile status selects the error blob for diagnostic use and conditional virtual `+8` cleanup; this branch does not read the code-output local. Nonnegative status conditionally cleans and clears the error output before using the code blob through virtual `+0xC`. The code blob is conditionally cleaned after either terminal predicate outcome. Shared failure cleanup therefore handles the path-selected pointer, not both outputs unconditionally. These observations neither prove what an external compiler may output on failure nor assign cleanup to the source object.

The local source wrapper uses vtable VA `0x1C80CC4` (raw `0x187F8C4`). Slot `+0xC` resolves to raw `0x12CB9C0`: it returns zero when wrapper backing `+4` is null, otherwise the DWORD stored at backing `+0xC`. `UiShaderSourceWrapperSizeProjectionRules` preserves that captured DWORD and rejects inconsistent backing/field captures. Slot `+4` is raw `0x12CB960`; the compiler callers pass offset zero, but this path still calls the preserve helper before returning a data pointer or null. Resize-helper effects are not projected.

The loader's chunk call at raw `0x12BB40E` is PE-bound to `MSVCR120.dll!fread` through IAT VA `0x1AC490C` (raw `0x16C350C`). `UiShaderSourceReadProjectionRules` selects the unsigned minimum of captured size and caller cap (`0x12BB3D0/0x12BB3D3`, cap at `[EBP+0xC]`). Its captured read plan distinguishes two zero paths: initially zero size bypasses setup at `0x12BB3C8`, whereas positive size capped to zero still reaches receiver slot `+0x1C` at `0x12BB3DA` before the zero-length test at `0x12BB3E3`. The read loop is reached only for nonzero clamped length. The projection separately captures the `0x01000000`-byte chunk cap and accepts only returned element count one for `fread(buffer, chunk, 1, handle)`. It does not confuse byte count with element count, execute reads, advance receiver state, or emulate failure cleanup.

The wrapper layout also records branch-specific slow-preserve instruction order: old/new backing data-pointer reads and the copy call precede logical-length and wrapper-pointer writes. `UiShaderSourceClearNativeLayout` separately records slow-zero capture of the old control, clearing wrapper `+8` then `+4`, and entry to its release path. The in-place zero path only writes backing logical length and is not an unconditional detach. These anchors are static access evidence only. A captured null allocation-output header does not acquire a clean failure result in this projection, and external allocator/reporting behavior is not executed or inferred.

`UiShaderSourceAllocationProjectionRules` selects the observed pool branch only for unsigned requested size at most `0x800` and zero alignment. Its output pair records entry-time zero initialization, not the allocator's final result. Caller mapping keeps the first DWORD (header, installed at wrapper `+4`) separate from the second (control, installed at wrapper `+8`); pool selection does not imply allocation success.

The captured chunk continuation advances the cumulative DWORD offset and subtracts the chunk only after a complete `fread` element. Offset addition wraps at 32 bits. A non-one result reaches slot `+0x14` cleanup without advancing either count; its return value is ignored. The projected return byte is zero on this failure, one on completion or a zero-length bypass, and absent while another iteration is required, conditional on cleanup and the stack-cookie check returning normally. It does not emulate cleanup effects.

`UiShaderSourceReuseProjectionRules` selects only the direct, non-assertion length-store edge: nonnull control, count exactly one, two separate unsigned capacity comparisons, and zero alignment or zero data-pointer remainder. The comparisons use backing capacity `+0`, not logical length `+0xC`. Rejection does not imply overall resize failure; an assertion-return path can also reach the length store.

`UiShaderSourceReadOnlyOffsetProjectionRules` models slot `+8` without the writable getter's preserve call. It compares offset inclusively with the captured virtual-size result, then uses a separately captured later backing pointer. Null backing returns zero; otherwise data-plus-offset wraps as a DWORD. Out-of-range pointer arithmetic is conditional on the diagnostic returning. These captures do not imply a stable or atomic backing snapshot.

`UiShaderSourcePreserveCopyProjectionRules` models only the replacement branch: zero request bypasses the accessor, while positive request captures the virtual-size result even when preservation is disabled. The selected count is the unsigned minimum of that result and requested size; only nonzero count and nonzero preserve byte reach the copy call. The byte is an EBP-relative argument, not an object field.

`UiShaderSourcePoolControlProjectionRules` records conditional initialization of captured nonnull control storage (`+4/+8` set to one, then vtable and header), followed by unconditional output-control and output-header writes. Null captured pointers are preserved. Primary/secondary count names describe observed fields, not an established ownership abstraction.

`UiShaderSourceBackingAlignmentProjectionRules` preserves header capacity and projects unchecked DWORD allocation-size and aligned-pointer arithmetic. Nonzero alignment advances by `alignment - pointer % alignment`, including a full unit for an already-aligned pointer; zero alignment leaves the pointer unchanged. Zero capacity continuation is conditional on its diagnostic returning. A captured zero allocator pointer remains an arithmetic input, not a modeled allocation success.

The absolute resize adapter at slot `+0x1C` forwards the requested DWORD and preservation argument one to the helper, then leaves EAX unchanged and pops four argument bytes on return. Its captured contract does not define a boolean status.

The captured control writer supports both observed variants: pooled controls request `0x14` bytes with vtable VA `0x1C80C8C`, while nonpooled controls request `0x10` with vtable VA `0x1C80CB0`. Conditional count/header initialization and output-control-before-header order remain shared; neither variant requires both captured pair pointers to be nonnull.

`UiShaderSourceDestructorNativeLayout` records wrapper slot-zero release gates and disposition. The first XADD/JNZ tests the post-decrement sum; only its zero edge reaches slot-zero callback and the separate second-counter XADD/DEC/JNZ sequence. Captured callback effects are not modeled. The destructor replaces its vptr, conditionally releases wrapper storage on deleting-flag bit zero, and does not directly clear wrapper backing/control fields.

`UiShaderSourceControlReleaseNativeLayout` separates pooled header-release dispatch from control-storage deleting dispatch. Slot zero checks header `+0xC`; slot four guards a null receiver before invoking slot eight with deleting argument one. Slot eight replaces the control vptr and gates its allocator-release call on argument bit zero, without directly releasing the header.

`UiShaderSourcePoolReturnNativeLayout` records conditional diagnostics for header `+8` and capacity, then the continuation assuming reached calls return. Initial and later `+8` captures remain separate. The later value is negated for the release-helper argument; header `+4` is cleared before that call, then pool `+0x10` is atomically decremented before the header handoff. This does not substitute a presumed data pointer or emulate the handoff helper.

`UiShaderSourceNonPoolReleaseProjection` keeps the distinct nonpool path: null header returns directly; otherwise the first helper call receives wrapped DWORD subtraction of header `+4 - +8`, followed by a tail jump carrying the original header pointer. A zero difference does not erase the first call from the projection. No field clears or callee effects are inferred.

`UiShaderSourcePoolCandidateProjectionRules` records the caller's captured acquisition result. Nonnull candidates receive data, displacement, capacity, and logical-length writes in instruction order; data-pointer addition wraps as a DWORD. The explicit null-candidate branch still calls the pair wrapper without header stores. This caller projection neither emulates acquisition retries nor guarantees that acquisition returns.

`UiShaderSourcePoolInitializationProjectionRules` records only the post-allocation continuation: nonnull allocator capture selects configuration with `(2064, 32, 1)`, then the allocator pointer and separately captured prior list head are published before setting byte `+4`. Null allocation still reaches those writes if preceding calls return; the flag does not imply pool readiness. With receiver VA `0x1E00500`, the `+0x10` store targets pool global VA `0x1E00510`.

Its separate entry-gate projection reports ID `0x68` for zero captured global readiness byte and ID `0x69` for nonzero receiver flag, in that order. The receiver flag is captured at its later native read, after any first diagnostic call. Returning reporters allow allocation to continue; these gates do not implement idempotency, locking, or readiness guarantees.

`UiShaderSourceCopyProjectionRules` records fresh-destination constructor order: install vptr, zero the pair, capture source control before backing, conditionally increment control `+4`, publish control before backing, then copy the independently captured trailing DWORD. Its stores use the null-old-control branch, not the callback branch.

`UiShaderSourcePairReplacementProjectionRules` separately handles captured old destination control. Incoming control increment precedes old-control capture; reached counter decrements gate indirect callbacks before the captured new pair is published. Both decrements in this helper use immediate XADD/JNZ flags. Neither projection emulates callback effects, backing-byte copies, an atomic source snapshot, or alias safety.

## Sorted adapter-address MD5 (offline projection)

`src\Startup\NativeAlgorithms\NativeAdapterAddressFingerprint.cs` saves
raw`0x127D0C0`. Original PE import-table mapping proves slot VA`0x1AC412C`
is `IPHLPAPI.DLL!GetAdaptersInfo`; reading that VA as a raw IDA offset
would yield unrelated bytes. The first call has null buffer/zero size and
must return111 (`ERROR_BUFFER_OVERFLOW`); the allocated-buffer call must
return zero. Either API failure returns false **without writing the
destination's16 bytes**.

Each `IP_ADAPTER_INFO` record is gated by type6 or71 and unsigned
address length at least6, then by the registry filter. Observed fields are
AdapterName at`+8`, AddressLength`+190`, Address bytes`+194`, Type`+1A0`.
Accepted records copy the complete declared address length, not an
invented fixed six bytes. The owning buffers are sorted with the reconstructed
[native prefix-only introsort](MatrixConnection.md#byte-buffer-introsort-pivot-partition-and-insertion),
then appended directly to MD5 without count, length, separators or deduplication.
No selected buffers yields successful standard MD5(empty);
the utility does not impose a separate no-address failure.

Filter`0x127CCD0` enumerates HKLM subkeys under
`System\CurrentControlSet\Control\Network\{4d36e972-e325-11ce-bfc1-08002be10318}`,
matches AdapterName using `_stricmp`, and opens the matching `\Connection`
key. **No matched/open key allows the adapter.** An opened key requires
successful `PnpInstanceID` size/type query (`REG_SZ=1`) and data read.
Its prefix before the first backslash must match `pci`, `usb` or `bth`
case-insensitively; no slash, other prefixes or query errors reject it.
Thus this must not be described as unconditional virtual-adapter exclusion.
The root/suffix/value/prefix literals were decoded from their distinct byte
masking routines and checked against both executable files.

The C# kernel consumes captured API statuses and per-record filter decisions.
`EvaluateRegistryAscii` separately projects captured registry results and
ASCII/C-locale prefix comparison; non-ASCII locale comparisons remain
explicitly unresolved. Native resized-string bytes are an input snapshot,
not inferred from a high-level registry string. Missing filter evidence
never produces a digest or a fabricated native return value.
No real host adapters, registry values or network services are accessed.
Managed traces do not emulate heap allocation failure/cleanup timing.

The former function at raw`0x127CD79` was a false split in the filter's
existing stack frame. Original bytes were verified before merging the IDA
function through`0x127D08E`; no executable patch was made.
One direct caller is `Synchronization_SelectModeZeroActivation` at callsite
`0x124B93A`. It zeroes two16-byte slots and conditionally copies each helper's
output only on a true return. Adapter API failure leaves its slot zero but
does not reject the already-selected branch. See the saved
`MatrixSynchronizationModeZeroDigestSlots` projection; the first helper's
`0x127C730` algorithm is saved below. Downstream activation semantics remain separate.

## Windows-drive storage serial MD5 (offline projection)

`NativeWindowsDriveSerialFingerprint.cs` saves helper`0x127C730` and
query`0x127C550`, without executing their Win32 APIs. The first call is
`GetWindowsDirectoryW(buffer,260)`, as verified through IAT slot
VA`0x1AC41C4`. Native rejects only a zero return, then overwrites UTF16
code unit2 with NUL. It uses the first **two code units**, not a general
path-root parser. For a captured `C:\Windows` buffer, the device path is
`\\.\C:`; unusual/non-drive prefixes are not repaired by the projection.

The query creates that device with `CreateFileW`: access`80000000`,
sharing3, disposition3 (`OPEN_EXISTING`), null security/template and
zero flags. Only handle`FFFFFFFF` is rejected by this branch. It sends
`IOCTL_STORAGE_QUERY_PROPERTY=2D1400` with **12 zero input bytes**:
device property0, standard query0, additional parameter0.
First output is an8-byte descriptor header and bytes-returned **must equal8**.
Header DWORD`+4` supplies the allocation/full-query size; the second query
uses that exact size and succeeds only if bytes-returned equals it.
Version is not validated. API failures close/free existing resources and
return false without writing the caller's16-byte digest.

On success, native reads the descriptor DWORD at`+24` as serial-number
offset, copies the NUL-ended bytes there into an owning narrow string, and
MD5-hashes its raw length/content. There is **no case conversion, whitespace
trim, hex interpretation or UTF8 conversion**. For captured `"  SN123  "`,
the digest is `9B7A7550A9DA9CC47A7755638C4ECF6E`.
Zero offset is **not treated as no serial**: it points to the beginning of
the descriptor and may hash header bytes as a CString. A valid empty string
produces MD5(empty) and returns true.

The snapshot projection distinguishes known native API/length rejections
from incomplete captures and unsupported native buffer accesses. Size
below28, out-of-allocation serial offsets and unterminated serial bytes
are not silently made empty/failed; the native code lacks these checks and
could read beyond its allocation. The bounded C# result therefore leaves
the native return value unknown for those cases, without performing the read.
`ProjectCapturedFingerprints` wires this result to the mode-zero Windows-drive
slot at`+4` and the independently captured adapter-address slot at`+14`.
False helpers retain initialized zero slots; unknown helpers remain unknown.
No real directory, drive handle, storage serial or device-control request
is queried or opened.
