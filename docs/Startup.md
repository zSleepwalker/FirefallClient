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

`UiShaderSourceInitialCopyProjectionRules` records the separate source-byte constructor: install the wrapper vptr, zero backing/control/alignment fields, and request resizing with preserve zero. After that call returns, the later backing capture alone gates the copy-thunk path. A nonnull backing reaches the call even for captured size zero, source zero, or data zero; those observations are not memory-safety or allocation-success guarantees. An absent backing capture represents a projection ending before that read.

`UiShaderSourcePoolTeardownNativeLayout` starts after the entry diagnostics return. It captures pool and next-link fields separately, publishes the captured next link to the singleton-list head, then clears receiver `+0xC` before `+0x10`. It does not directly clear receiver flag `+4`. Only the captured pool pointer selects the later drain; the layout stops at that boundary rather than modeling allocator effects or shutdown completion.

Its separate entry projection reports `0x76` when the captured global readiness byte is zero, `0x77` when receiver flag `+4` is zero or its conditionally reached pool read is zero, and `0x78` when the later list head differs from the receiver. The pool read is absent on the zero-flag branch; the diagnostic capture does not substitute for the later detach capture. Reporting through the non-import cell at VA `0x1C15EA0` does not itself establish abort behavior.

`UiShaderSourcePoolDrainProjection` distinguishes a null captured pool (no helper calls) from a nonnull pool whose first pop returns zero (tail transfer of the pool to the allocator backend). A nonzero captured pop is passed to the same backend before another pop with receiver `pool+8`; that next result and subsequent loop iterations remain unmodeled. The projection neither guarantees termination nor treats the backend as an operating-system free routine.

`UiShaderSourcePoolPopProjection` records one attempt against a receiver-relative head/tag pair. A null captured head returns zero without an exchange. Otherwise, the proposed pair contains the captured node link and wrapping tag increment; the explicit comparisons of returned low and high DWORDs select return versus an unmodeled recapture boundary. It does not mutate memory or simulate contention. These offsets are relative to the pair receiver, which the teardown supplies as `pool+8`, not to every containing pool object.

`UiShaderSourcePoolPushProjection` keeps the link-store attempt separate from the later node-link reread used as the expected low compare-exchange half. It proposes the node pointer and wrapping tag increment, then follows explicit returned-half comparisons to return the receiver or stop at retry. A null-node capture describes only the continuation after its diagnostic returned; projected stores do not establish valid memory or successful mutation.

`UiShaderSourcePoolAcquisitionProjection` begins only after a nonzero pop from the original receiver. It increments receiver `+0x10`, then independently captures the divisor at `+0x18`. An absent divisor ends the projection before division; zero marks the divide-error boundary. A nonzero remainder reaches diagnostic `0x254` (596 decimal), with the original acquired pointer returned if the reporter returns. The earlier replenish/retry loop is not simulated.

`UiShaderSourcePoolSingletonDestructor` records singleton vtable slot zero at raw `0x12CC280`, distinct from slot-eight detach. The bounded body tests deleting bit zero, replaces the vptr, clears receiver `+8` and `+0xC`, conditionally calls the allocator backend with the receiver, and returns that receiver if the call returns. It does not invoke the adjacent drain helper or directly access the singleton pool/list-head fields.

`UiShaderSourcePoolReplenishmentProjectionRules` stops at the allocator call in raw `0x12CE0F0`. Separate captures at receiver `+0x14`, `+0x1C`, `+0x20`, and `+0x18` produce `((field14 + field1C) * field20) + field18 + 3`, retaining only 32 bits at each arithmetic step. This models neither overflow rejection nor allocation success; post-allocation publication and later alignment remain separate boundaries.

`UiShaderSourcePoolReplenishmentResultProjectionRules` keeps a null allocator result on the diagnostic path. Only captured normal returns from that path allow the first tagged-push call with receiver `pool+8`, preserving the allocator result, including zero, as its argument. Nonnull results bypass those diagnostics; neither case establishes push success.

`UiShaderSourcePoolReplenishmentAlignmentProjectionRules` starts after that push returns. It captures the divisor once from `+0x18`, rounds wrapping `block+4` upward when its remainder is nonzero, and checks the resulting candidate with a second unsigned division. Zero divisor stops at the first divide-error boundary. Wrapping with a non-power-of-two divisor can still reach diagnostic `0x2BA`; the projection does not repair that candidate or infer valid allocation storage.

`UiShaderSourcePoolReplenishmentIterationProjectionRules` records one already-entered loop iteration, computing a wrapping entry address from the captured index, base, size, and padding. It distinguishes divide error, nonreturning diagnostic `0x2C0`, and nonreturning primary-push boundaries. Only normal continuation requires the separately captured later base and fresh `+0x20` bound; unsigned comparison of the incremented index selects the next-iteration boundary. The entry predicate is separate, and no whole-loop execution or helper effects are simulated.

`UiShaderSourcePoolConfigurationProjectionRules` records initial zero/argument stores and the later padding store. Entry size/count/alignment arguments remain separate from subsequent field reads across returning diagnostics. Those reads select ordered IDs `0x1F5`, `0x1F6`, `0x1F7`, and `0x1FD`, then compute wrapping padding `((size - 1 + alignment) & ~(alignment - 1)) - size`. The final arithmetic uses its own later size/alignment captures, not a presumed unchanged argument snapshot. The singleton's `(0x810, 0x20, 1)` configuration yields zero padding when those values remain unchanged.

`UiShaderSourcePoolResetEntryProjectionRules` separates reset's null-callback diagnostic `0x215` from the later counter read after `LFENCE`. A returning diagnostic does not reject the call: captured counter zero returns EAX zero, while nonzero selects reset work beyond this projection. Missing reached counter captures and supplied unreached captures are rejected; callback traversal and reset effects are not inferred from passing this gate.

`UiShaderSourcePoolResetCallbackProjectionRules` projects one captured iteration-entry predicate, wrapping candidate `alignedBase + index * (padding + size)`, and first equality in an immutable captured DWORD range. Equality stops at the branch to raw `0x12CDE44`; it does not simulate that path's cursor mutation. No match reaches the cdecl callback at raw `0x12CDDD4` with one candidate argument, including a zero callback target if the earlier diagnostic returned. The callback result is unused. The later bound read and subsequent iterations remain outside this projection.

`UiShaderSourcePoolResetFinalizationProjectionRules` distinguishes secondary-drain completion from the initial empty-secondary edge, which skips the count load and requires retained zero EDI. It initializes a local zero, compares its address with the captured counter-cell address, and on inequality executes SFENCE then LFENCE before the zero store. Both alias outcomes perform the separately captured readback. A nonnull temporary begin reaches the opaque release helper; only its normal-return path restores SEH and returns the captured count with RET 4. No allocator or OS-free semantics are inferred.

`UiShaderSourcePoolResetAlignmentProjectionRules` captures the distinct reset prefix at raw `0x12CDD68`: wrap incoming EAX plus four, divide by the single captured field-18 divisor, conditionally add divisor minus remainder, then divide again. Zero divisor stops at the first fault boundary. A nonzero second remainder reaches diagnostic `0x227`; only an explicitly captured return reaches the candidate-loop boundary. Wrapped correction can leave a nonzero second remainder, so this is not an unconditional alignment guarantee.

`UiShaderSourcePoolResetAppendProjectionRules` distinguishes a saved local source address inside the captured descriptor range from an external saved pop value. Full capacity stops at the growth call unless separate return captures are supplied; the alias path reloads begin and selects its source by the saved element index. A nonzero destination end permits the DWORD store, while both null-gate outcomes advance the saved end by four. Missing reached source captures and supplied unreached captures are rejected.

`UiShaderSourcePoolResetGrowthProjectionRules` preserves arithmetic right shifts of wrapping byte differences and subsequent unsigned comparisons in raw `0x6A5500`. Spare capacity returns directly. Insufficient capacity reads begin independently for used-count and capacity calculations, stops at the selected indirect reporter, or chooses the unsigned maximum of wrapped required capacity and guarded capacity-plus-half before tail-transferring to raw `0x6A5900`. It does not simulate allocation or reporter return.

`UiShaderSourcePoolResetRemovalProjectionRules` projects raw `0x12CDE44` separately from the membership scan. Equal cursor/end reaches the callback boundary without a tail read. Otherwise a captured last DWORD is stored at the cursor and the wrapping end-minus-four is saved, then control reaches the item-index increment without the callback-count increment. This tail replacement does not preserve element order or prove memory validity; reached tail captures are mandatory.

`UiShaderSourcePoolResetStorageProjectionRules` projects raw `0x6A5900` with separate allocation-result, initial copy-descriptor, and post-copy descriptor captures. Oversized counts and null allocation results stop at the opaque report cell. Zero count skips allocation but still reaches the copy boundary with new base zero. Normal copy/release continuations publish begin, storage end, and retained end in that order; wrapping masked copy length and later arithmetic-shifted retained count need not describe the same descriptor snapshot.

PE import names identify cell VA `0x1AC4424` as `std::_Xbad_alloc`, `0x1AC45C0` as `std::_Xlength_error`, `0x1AC489C` as `memmove`, and `0x1AC4870` as `_snprintf`. These bindings identify the called imports, not observed runtime outcomes; the captured projections retain their explicit call boundaries.

`UiShaderSourcePoolResetSecondaryGateProjectionRules` initializes EDI and the callback count to zero, then captures one pop of receiver plus eight. A missing result stops at the call. Nonzero enters reset alignment; zero jumps directly to raw `0x12CDE04`, skipping the final count load at `0x12CDE01` and retaining the initialized zero EDI. These finalization entry edges are distinct.

`UiShaderSourcePoolResetPrimaryGateProjectionRules` separates initial descriptor zeroing from repeated pop-result gates. Each captured zero result selects the secondary gate and each nonzero result selects append. Missing results stop at the pop call; the repeat gate does not duplicate append's end advancement.

`PoolResetVectorCleanupProjectionRules` binds the concrete reset callback at raw `0x12C0920` to caller raw `0x12C0340`, receiver this plus `0x20`. The cdecl callback's null first DWORD skips every zero store. A nonnull first DWORD reaches the opaque release helper; only its captured normal return reaches zero stores at offsets 0, 4, and 8. This does not establish ownership or native memory effects.

`UiShaderSourcePoolResetIterationTailProjectionRules` distinguishes callback return from removal completion: only callback return reloads begin, cleans the argument, and increments callback count. Both increment the item index with DWORD wrap and compare it unsigned against a fresh captured bound. `UiShaderSourcePoolResetSecondaryRepeatProjectionRules` separately captures the release helper's return and next secondary pop; a zero next pop falls through to count load raw `0x12CDE01`, while nonzero returns to alignment.

`StackAllocationResultProjectionRules` records the stack-byte-count wrapper's captured allocator result and individual normal-return gates for the null-result thunk, formatter, reporter, and cookie check. Returning OOM diagnostics restore the saved zero result before the cookie call. No result or helper effects beyond that call are inferred; normal-return claims for unreached calls are rejected.

`QueuePopCompletionProjection` starts only after successful sentinel-head CAS in raw `0x12CE400`. It projects separate receiver and global-pool counter decrements, a separately reloaded old head, and its payload read before the old head is pushed back to the captured pool. Only the push helper's normal-return continuation returns the captured payload. The two XADD operations are not a single atomic lifecycle operation, and neither counter's returned prior value is used.

`QueuePopAttemptProjection` models a single sentinel-head attempt with separate head/tag reads. A null head reaches diagnostic `0x300`, not a clean empty result; a returning diagnostic still reaches the captured link read. Only null next plus an unchanged later tag returns zero. Otherwise the projection records expected/replacement pairs and explicit returned-half comparisons, stopping at retry or successful-CAS continuation without simulating the operation or retry loop.

`QueueCleanupEmptyAttemptProjection` records one surrounding destructor-loop pass: independently capture tag/head, then head-next and a later tag. Tag mismatch retries before consuming the saved empty predicate. Agreement plus empty next selects the embedded-pool drain; nonempty next calls queue pop, ignores its result, and only a captured normal return reaches the explicit repeat.

`QueueAppendAttemptProjection` records a single tail-pair CAS using separate receiver+`0x0C` tag and receiver+8 tail captures. A returning null-tail diagnostic `0x2EC` may still reach the CAS. Expected `(tail,tag)` and replacement `(newNode,tag+1)` precede explicit returned-half comparisons and register restoration. Retry and publication boundaries are distinct; this does not model allocation, publication, or whole-append atomicity.

`QueueAppendPublicationProjection` covers the successful-CAS continuation at raw `0x12CE3C3`: nonzero old-tail next reports `0x2F0`, and only a captured normal diagnostic return reaches publication. Payload is stored at old-tail+4 before the independently captured new node is linked at old-tail+0 and returned with `RET 4`.

`QueueSentinelReturnProjectionRules` preserves the receiver+8 versus receiver+0 diagnostic `0x2D6`, LFENCE and independent receiver+`0x10` diagnostic `0x2D7`, and later independent node/global-pool reads. A returning null-node diagnostic `0x25A` may continue with zero. Independently captured pool counter arithmetic wraps on decrement; its old XADD result is unused. Plain return requires the opaque pool-push helper's normal-return capture.

`QueueEmbeddedPoolDrainProjection` models one destructor drain cycle at raw `[0x12C0382,0x12C03AF)`. The embedded-pool pointer saved before reset is distinct from the queue pointer formed after reset. Initial zero skips release; otherwise a captured normal release return gates the next pop. Nonzero next pop stops at the repeat edge; zero reaches queue cleanup. Cleanup's captured normal return alone selects the destructor continuation.

`QueueSentinelInitializationProjection` clears receiver+`0x10`, optionally initializes the shared global pool, checks receiver alignment (diagnostic `0x2CD`), and independently reloads that global for acquisition. Nonnull acquisition zeros eight bytes; null skips that store but still publishes null to tail and head. Tail precedes head, followed by clearing both tags and returning the receiver. The constructor uses `this+8` for this queue and separately configures the embedded pool at `this+0x20` with `(12,8,1)`.

`QueueAppendAcquisitionProjection` increments the receiver counter before independently loading the global pool and calling acquisition. Unlike initialization, null acquisition does not skip the following unconditional DWORD zero-store attempts through addresses zero and four; no clean failure or successful write is inferred. The prefix stops before the first tail-tag read and preserves missing versus unreached capture boundaries.

`QueueDestructorFinalizationProjection` covers the ordinary tail at raw `[0x12C03AF,0x12C03CA)`: reload the saved receiver, store immediate `0x1C734CC` to its first DWORD, independently reload and restore the prior FS exception chain, then restore registers and return. This tail has no inline cookie-check call; the separate exception handler is not modeled.

`QueueGlobalPoolInitializationProjectionRules` captures guard diagnostic `104` and independent receiver-flag diagnostic `105`, then the `0x28` allocation/configuration boundary. Null allocation skips `(8,32,8)` configuration but still publishes null at receiver+`0x10`, old global head at receiver+`0x0C`, receiver as global head, and initialized byte last. The slice stops at that flag store, not function return or an atomic initialization guarantee.

`SocketReceivedQueuePopProjection` preserves the caller at raw `0xBD3EA5`: receiver+`0x260` selects the queue; zero pop result branches to `0xBD3E60`; nonzero values are compared **unsigned** with `0x10`, with values at least that threshold branching to `0xBD40BF`. Small nonzero values fall through without being treated as dereferenced pointer payloads.

`SocketFragmentEnqueueProjectionRules` masks the captured first payload word with `0x7FF`; only values above unsigned threshold `0x570` reach diagnostic `0xB2`. A nullable diagnostic return distinguishes skipped, missing, nonreturn, and normal continuation. Append receives the original payload and receiver+`0x38`; its captured EAX is forwarded unchanged, with returned zero distinct from a missing return capture.

`ContextQueueAppendProjection` covers the producer's final handoff at raw `0xBD3456`: wrapping cursor-minus-initial-pointer subtraction is truncated to 16 bits and zero-extended into payload+4, so this field is a modulo-65536 offset rather than a length. An independently captured stack value goes to payload+`0x18`; the unchanged payload pointer is passed to append with receiver+`0x260`. Projection stops at the captured append-return boundary, without processing the parser repeat edge.

`OwnerCallbackQueueDeliveryProjection` preserves the first owner queue's saved-pop local and address argument to callback slot+4. Only a captured normal callback return permits the independent local reload; the callback may replace the payload, including with null. A nonnull reloaded payload supplies the counter address at +8; wrapping decrement uses a separately captured old counter, and only old value one reaches opaque payload slot+8 with argument one. No ownership-strength or deallocation semantics are inferred.

`SocketSmallStatusProjection` splits small nonzero queue values: case 2 calls the close helper without inventing an EDX argument; case 3 requires the captured +`0x108` DWORD to equal four, then tests predicate AL. Zero AL reaches diagnostic `0x4EB` without reading +`0x248`; nonzero AL reads that word and diagnoses only if zero. Captured normal returns gate the state-helper argument five and join edge.

`SocketLargeQueueGateProjection` keeps initial and later +`0x108` DWORD reads independent. Initial states 3/5/8 skip diagnostic `0x4FA`; otherwise its captured normal return gates later reads. Nonzero +`0xD95` selects handoff. With a zero byte, later state three tests whether either top payload-word bit is set (mask `0xC000`), not equality with `0xC000`; other later states select header dispatch. Downstream routes remain boundaries.

`DeferredControlCallbackProjection` records the embedded callback slice at raw `0xBD37D8`: independently load the vtable pointer at payload+8 and argument value at payload+4, pass **payload+8** in ECX, and call opaque slot+4. Only a captured EAX permits the independent returned-pointer byte read. Clear bit `0x08` branches to `0xBD3805`; set bit falls through to `0xBD37EB`. A zero returned pointer is not normalized into a clean failure.

`SocketPayloadHeaderDispatchProjection` independently captures WORD[payload] at raw `0xBD412B` and selects one of four table entries using `word >> 14`. The table operand is VA `0xFD4E2C`, mapped to raw `0xBD422C`, not a raw address itself. Each route records the pushed payload and ECX, stopping before its first opaque call: receiver / `BD3760`, receiver+`D6C` / `BD5BA0`, receiver+`D70` / `BD5BA0`, or receiver+`D74` / `BD5C00`. The unsigned-above-three default is unreachable for a zero-extended WORD.

`SocketStatePredicateProjection` captures the status byte once at raw `0xBCCA63`. Zero status returns AL zero while preserving incoming EAX's upper 24 bits. Bit zero set returns a normalized comparison of DWORD[this+4] with zero. Otherwise bit one clear reaches diagnostic `0x23C`, whose normal return gates later reads. The first pair, DWORD[this+8] OR DWORD[this+C], short-circuits on nonzero; only zero reaches the independent +`10` and +`14` captures. Missing sequential reads do not authorize later captures.

`SocketStateEntryProjection` covers only raw `[0xBD1B98,0xBD1BC2)`. Requested state unequal to the independently captured DWORD[this+108] skips the diagnostic; equality reaches it and requires a captured normal return before stack cleanup. Equality is not an early return. The later state write at raw `0xBD2035` remains outside this projection; calling the helper with argument five is not itself evidence that the write was reached.

`SocketPayloadRecycleTailProjection` starts at raw `0xBD5C9C`, after the preceding opaque reference-count calls. It writes the fixed DWORD through node+8 before its late null diagnostic, so the diagnostic does not protect earlier accesses. Only normal diagnostic return reaches the independently captured saved receiver reload. The pool counter at receiver+`10` is decremented modulo 32 bits; its old value is not tested before passing the node to raw `0x12CDA60`. Only captured normal helper return reaches the wrapper's `RET 4`. Missing node, receiver, or counter values stop before later actions and reject supplied later captures.

`SocketPayloadRecyclePrefixProjection` captures the associated pointer at node+`10` once. Null skips to raw `0xBD5C9C`. Otherwise LOCK XADD decrements associated+4; only an updated zero reaches vtable slot zero. Its captured normal return permits the independent associated+8 decrement; only updated zero reaches the independently reloaded vtable's slot+4. The original associated pointer is retained across calls. No explicit stack arguments are pushed at either call, and no ownership or freeing semantics are inferred.

`SocketHeaderFieldGetterProjection` shares the identical 35-byte getters at raw `0xBD5BA0` and `0xBD5C00`. An initial zero DWORD[receiver] reaches diagnostic `0x5B`; its normal return gates the final reload. Both paths independently reload DWORD[receiver] into EAX, so even an initially nonzero field can return zero. Both use plain `RET`, do not read a caller stack argument, and do not consume the earlier pushed payload.

`SocketHeaderCaseContinuationProjection` copies the independently captured getter EAX into ECX, including zero, before calling raw `0xBE51F0` for cases one/two or `0xBE5A40` for case three. The earlier pushed EDI remains before that call; later stack cleanup is not inferred. Captured return AL nonzero selects the common raw `0xBD3E60` destination; zero selects the case-specific fallthrough. Missing return captures select no branch.

`ReliableReceiveEntryProjection` covers the raw `0xBE51F0` two-flag prefix. Receiver+`C` zero reaches diagnostic `0x1DA`, whose normal return gates the independent receiver+`1C` byte read. A nonzero second flag clears AL only and returns with `RET 4`; zero stops before the raw `0xBE5227` argument-load block. Upper EAX bits are not projected.

`UnreliableReceiveEntryProjection` covers only raw `[0xBE5A40,0xBE5A6B)`: receiver+`C` nonzero skips diagnostic `0x2AF`; zero requires a captured normal diagnostic return before reaching the same boundary. Neither entry has a null guard before its first flag dereference. The case-three sequence comparison remains outside this projection.

`ReliableReceiveDeltaGateProjection` covers raw `[0xBE5227,0xBE524F)`. It reads payload WORD+2 before receiver WORD+`2A`, subtracts in DWORD registers, truncates to 16 bits, and compares the low WORD of `difference-0x100` unsigned against `0x7EFF`. Fallthrough requires modular difference `0x100..0x7FFF`; other residues branch to `0xBE528F`. The argument stack slot is overwritten with DWORD `0x7EFF`. No parser or downstream call behavior is projected.

`UnreliableReceiveDeltaGateProjection` covers raw `[0xBE5A6B,0xBE5A86)`, reading receiver WORD+`1E` before payload WORD+2. DWORD subtraction is masked to `0xFFFF`, followed by **DWORD** CMP EAX,`0x8000` and signed JGE. Because masking leaves EAX in `0..65535`, the branch selects raw `0xBE5AC2` exactly for residues at least `0x8000`; other residues fall through to `0xBE5A86`. This is not an AX comparison and no downstream parsing is projected.

`ReceiveGuardForwardingProjection` covers raw `[0xBE2F50,0xBE2F73)`. Null argument zero bypasses the threshold read but still forwards. Otherwise signed argument one less than independently captured DWORD[arg0+4] returns without setting EAX. Forwarding pushes the frame+`14` tail address and captured argument two, calls raw `0xBE2F80`, and cleans its own eight pushed bytes only after normal return. Its plain `RET` does not purge incoming arguments. A missing DWORD is distinct from captured native zero; no format interpretation is inferred.

`ReliableReceiveAlternateHandoffProjection` covers raw `[0xBE528F,0xBE52A5)` for prior-gate residues below `0x100` or above `0x7FFF`. It pushes the previously formed payload-word address and passes receiver+`47C` in ECX to raw `0xBE6300`, without rereading either WORD. Only normal helper return permits comparing the retained delta in BX against AX=`8000`: unsigned JB selects `0xBE52DF`, otherwise `0xBE52A5`. Middle-interval entry captures are rejected.

`PayloadWordGrowthGateProjection` covers raw `[0xBE6306,0xBE6325)`. Independent DWORD reads at receiver+`C` and +`10` are added with wraparound; nonzero low three bits skip the opaque call. Otherwise DWORD[receiver+8] is compared unsigned against `(captured DWORD[receiver+10]+8) >> 3`, with addition wrapping before the shift. Greater skips; less-or-equal pushes the captured +`10` value and calls raw `0xBE68A0` with the original receiver. Only normal call return reaches `0xBE6325` on that path. No capacity, allocation, or growth success is inferred.

`PayloadWordSlotIndexProjection` covers raw `[0xBE6325,0xBE6352)`. It reloads receiver DWORDs +8 and +`10`, forms wrapping masks `count*8-1` and `count-1`, and AND-writes DWORD[receiver+`C`]. An independent +`C` reload is added to the retained +`10` value; its logical right shift by three is ANDed with `count-1` to choose a DWORD slot at the independently captured +4 base. A zero slot falls through to `0xBE6352`; nonzero branches to `0xBE636E`. No power-of-two count, access validity, allocation, or ownership is assumed.

`PayloadWordSlotAllocationProjection` covers the zero-slot path `[0xBE6352,0xBE636E)`: push `10`, call raw `0x12B7FB0`, and clean four argument bytes after capturing its normal return and EAX. Nonzero EAX is retained in ECX. Zero calls through cell VA `0x1AC4424`; normal return and post-call ECX are independent captures, not a presumed zero. An independent receiver+4 reload supplies the base for the DWORD slot write. No allocation success or indirect-target identity is inferred.

`PayloadWordCopyTailProjection` covers `[0xBE636E,0xBE6391)`. It independently reloads receiver+4 and the DWORD slot at base+retained EBX. Candidate address is the slot pointer plus twice the low three bits of retained EDI, with DWORD wraparound. Only candidate zero skips reading the stack-passed pointer and copying its WORD; a zero slot pointer can still produce a nonzero candidate. Both paths increment independently captured DWORD[receiver+`10`] before `RET 4` at `0xBE638E`. No full EAX return value or access validity is claimed.

`PayloadWordGrowthDoublingProjection` covers `[0xBE68B3,0xBE68E1)`: capture DWORD[receiver+8], initialize ESI to that value unless zero (then one), and preserve each wrapping loop iteration. Unsigned `(ESI-count)>=1` followed by `ESI>=8` reaches `0xBE68E1`; otherwise unsigned `(0x0FFFFFFF-ESI)<ESI` branches to `0xBE6A10`, with subtraction wrapping. Other iterations double ESI. Repeated ESI is an explicit arithmetic-cycle outcome, not a capped success: count `0x80000000` doubles to zero and repeats. This does not establish that such a count is reachable in native operation.

`PayloadGrowthBufferAllocationProjection` covers `[0xBE68E1,0xBE691A)` after the preceding unsigned loop-exit conditions. It independently reads receiver+`C`, writes its logical shift by three to frame-8, and initializes frame-4 to zero. The retained final count above `0x3FFFFFFF` bypasses allocation to the opaque indirect call through VA `0x1AC4424`; otherwise count times four is pushed to raw `0x12B7FB0`. Normal allocator return permits four-byte cleanup before the separate EAX capture and frame-4 write. Nonzero EAX skips the indirect call; zero requires its independently captured normal return. Frame writes are observations, not allocation or readiness guarantees.

`PayloadWordAppendProjection` composes the growth gate, slot index, optional zero-slot allocation, and copy tail using the existing projections. Both gate skips continue to slot indexing, as does a normal opaque handoff return. Receiver pointers and retained EDI/EBX must agree across stages; independent memory reloads are not forced to equal earlier writes. Nonzero slots reject allocation-stage captures, and incomplete or invalid stages cannot produce append completion.

`PayloadGrowthFirstCopyProjection` covers `[0xBE691A,0xBE694D)`. It independently reads receiver+4/+8 and frame-4/-`C`, preserving the frame-`C` write and later reload separately. Wrapped subtraction/addition and signed SAR by two form the scaled count. The opaque call through VA `0x1AC489C` receives destination at entry ESP+4, source at ESP+8, and count at ESP+`C`. Only a captured normal return reaches `0xBE694D`; optional returned EAX is not needed for this boundary. No copy-target identity, buffer validity, or cleanup beyond the window is inferred.

`PayloadGrowthPostCopyGateProjection` covers `[0xBE694D,0xBE6960)` after an explicitly captured normal prior-call return. It independently reloads receiver+4 before cleaning twelve argument bytes. Retained EDI plus prior-call EAX wraps to the saved pointer; an independent frame-8 reload precedes writing that pointer to frame-`10`. Unsigned frame-8 greater than retained ESI branches to `0xBE69A7`; equality and below fall through to `0xBE6960`. Missing earlier reads suppress dependent cleanup or stores.

`PayloadGrowthBranchCopyProjection` covers the first branch-local opaque calls at `[0xBE6960,0xBE6984)` and `[0xBE69A7,0xBE69BD)`. Unsigned retained EDI greater than ESI selects the latter. Fallthrough writes wrapped ESI-EDI to frame-`C`, preserves the signed SAR intermediate, loads the call target independently from VA `0x1AC489C`, and reloads destination from frame-`10`. The taken path uses retained EDX directly and never reads that frame slot. Both expose destination/source/count at call-entry ESP+4/+8/+`C`; captured target zero is distinct from a missing target. No call identity or successful copy is inferred.

`PayloadGrowthTakenSecondCopyProjection` covers `[0xBE69BD,0xBE69DF)` after an explicitly captured prior normal return. Receiver+4 overwrites prior-call EAX; independent frame-`C` and frame-4 captures form the signed-SAR count and destination for another opaque call through VA `0x1AC489C`. Twelve prior argument bytes remain pending while twelve more are pushed; the window stops before the later twenty-four-byte cleanup.

`PayloadGrowthPublicationProjection` covers `[0xBE69F2,0xBE6A10)`. An independent frame-`14` reload supplies the receiver, including captured native zero. Receiver+4 zero skips only the opaque call; nonzero passes that captured value in ECX to raw `0x12C8D60`. Only the appropriate skip or normal return permits independently reading and adding retained ESI to receiver+8, then publishing retained EBX to receiver+4 before `RET 4`. No ownership, freeing, or full EAX result is inferred.

`ReliableReceiveSlotGateProjection` covers `[0xBE5303,0xBE532F)` with validated alternate-path delta below `0x100`. It adds that delta to independently captured DWORD[receiver+`478`], masks with `0x800000FF`, and preserves the exact negative DEC/OR/INC sequence. Zero-extended CX plus `1E`, scaled by four, selects the wrapping slot address. That address is written to frame+8 before its DWORD is read. Captured zero routes to `0xBE538D`; nonzero falls through to `0xBE532F`. No slot validity or record semantics are inferred.

`ReliableReceiveForwardingCallerProjection` covers both six-push calls to raw `0xBE2F50`, `[0xBE52A5,0xBE52C7)` and `[0xBE5353,0xBE5375)`. Receiver+4 is read before receiver+24; on normal helper return, receiver+8 is independently reloaded before the 24-byte caller cleanup. The route-specific tags and literal pointers are preserved without assigning format semantics.

`PayloadGrowthZeroFillProjection` covers fall `[0xBE6984,0xBE69A7)`, taken `[0xBE69DF,0xBE69F2)`, and their shared REP tail. Each reached REP has an explicit direction flag and independent normal-completion capture. Address progression is symbolic wrapping DWORD arithmetic, not a count-sized array or proof of valid writes. Fall's frame-8 reload occurs only after its nonzero first REP completes; a skipped first REP retains incoming EDI. Frame-4 reloads remain independent of earlier writes.

`ReliableReceiveRecordWordProjection` covers `[0xBE532F,0xBE5353)` after a captured nonzero slot record. It reads WORD[record+2] before WORD[payload+2]. Equality skips the diagnostic; mismatch pushes `0`, `1FA`, `1B58B3E`, `1B58B1B` and calls through absolute cell VA `1C15EA0`. Only a captured normal diagnostic return reaches the 16-byte caller cleanup. The later forwarding stage is not inferred.

`SharedTextPoolSingletonProjectionRules` captures the ordinary low-bit guard branch at raw `0x12B9F70`. A clear bit is set and stored before configuring receiver VA `0x1F113D8` with `(32,128,16)`, followed by the separate helper call. Both normally returning paths return that receiver. Guard VA `0x1F113D0` and the receiver lie in zero-filled virtual data, not file-backed bytes; this is not an atomic-once or readiness guarantee.

`SingletonTeardownWalkerProjectionRules` records the surrounding list walker at raw `0x12C3AF0`: after any returning entry diagnostic, capture the initial global head, clear byte VA `0x1E4B096`, and set guard byte VA `0x1E4B095`. A nonnull head receives one slot-eight dispatch, followed by an independent global-head capture. A null initial or later head selects the final guard clear; a nonnull later head stops at the repeat boundary. The walker itself reads no next link and supplies no explicit dispatch arguments. This is not a virtual-callback or complete-shutdown simulation.

`ReliableReceiveRecycleReturnProjection` covers `[0xBE52C7,0xBE52DF)` and `[0xBE5375,0xBE538D)`. Counter DWORD[EAX+20] is incremented before the independent global read at VA `1DFFF84`. That global supplies ECX for the payload-push call to raw `BD5C60`. Only normal helper return permits AL=1, register restoration, and RET 4. Full EAX remains unknown unless independently captured at helper return; only its low byte is replaced.

`ReliableReceiveEmptySlotCallbackProjection` covers `[0xBE538D,0xBE53A2)`. MOV BX/SHR BX/AND BL select header bits 12/13; zero branches to `BE53E9`. Otherwise DWORD[payload+8] supplies the base of an indirect target read at +C, while ECX is the address payload+8. No stack arguments are explicitly pushed. Target/address zero is a captured scalar, not a null guard or validity guarantee.

`ReliableReceiveForwardingReturnProjection` composes the forwarding caller and recycle-return projections. Only completed caller cleanup admits a tail capture. Route and the independently reloaded receiver+8/EAX pointer must match before projecting any tail effects. A mismatched route/pointer or incomplete return cannot be mistaken for completion from its raw boundary alone.

`ReliableReceiveCallbackResultProjection` covers the callback-return TEST/JNZ at raw `BE53A2` and zero-result diagnostic handoff through `BE53BC`. Nonzero EAX branches to `BE53BF`; zero pushes `0`, `205`, `1B58B27`, `1B58BDA` through absolute cell `1C15EA0`. Its separate cleanup is outside this projection.

`ReliableReceiveLengthCallbackProjection` covers `[BE53BF,BE53DD)` after the nonzero-result edge. Independently reloaded header WORD is masked with `7FF`, zero-extended, then subtracts four with DWORD wrap. Length is pushed before payload+8 is read; payload+4 is read/pushed next, then the indirect target at the loaded pointer+4. ECX is payload+8's address, not contents.

`ReliableReceiveSlotPublicationProjection` covers `[BE53E9,BE53F7)`. An independent frame+8 DWORD reload supplies the address for the payload pointer write, then AL=1 preserves the pointer's upper 24 EAX bits before pops and RET 4. The captured pointer need not equal an earlier frame write; zero is not guarded. All effects remain projections, not access-success guarantees.

`ReliableReceiveCallbackScalarHandoffProjection` covers `[BE53DD,BE53E9)`. EDX receives captured callback EAX; CL receives retained BL without inventing ECX's upper 24 bits. The opaque call to raw `BD58E0` has no explicit pushes in this window; its normal return gates the four-byte cleanup.

`UnreliableReceiveResidueHandoffProjection` covers `[BE5AC2,BE5AE4)` under the validated earlier masked receiver-minus-payload residue gate. The reversed difference is truncated to a WORD; residues at most one bypass the helper to `BE5AF0`. Otherwise it pushes receiver+4, residue-minus-one, literal `1BB8D04`, zero, receiver+18 to raw `BE2F50`, preserving independent reads and normal-return capture.

`UnreliableReceiveResidueCounterProjection` covers `[BE5AE4,BE5AF0)` only after normal helper return. Receiver+8 is independently read, EAX becomes retained ESI minus one with DWORD wrap, 20 stack bytes are cleaned up, then that value is added to the independently captured counter at loaded-pointer+1C. A missing counter capture retains the earlier cleanup but projects no counter write.

`UnreliableReceiveJoinedHeaderProjection` covers `[BE5AF0,BE5B16)`. WORD[payload+2] is written to receiver+1E before an independent header WORD reload. Header length is masked to `7FF`, subtracts four as a DWORD, then is truncated through zero-extended AX into ESI and frame-4. The selector from header bits 12/13 is stored independently at frame+8 before its zero branch to `BE5B32`; nonzero falls through to `BE5B16`.

`ReliableReceiveOccupiedSlotProjection` composes slot selection, record WORD comparison, forwarding, and the recycle-return tail. It validates retained receiver, record pointer, and payload correspondence before later effects. Only the exact successful WORD outcomes admit forwarding; an invalid capture with a branch destination does not suffice. Independent memory reloads remain independent, including receiver+8.

`UnreliableReceiveMarkerCallbackProjection` covers `[BE5B16,BE5B25)` under a nonzero selector in `1..3`. Retained SI is directly zero-extended and pushed before reading DWORD[payload+8]. The payload+4 DWORD is then read/pushed before the indirect target at loaded-pointer+4. ECX is payload+8's address. Missing later captures retain the length push; only normal call return reaches `BE5B25`.

`UnreliableReceiveCallbackScalarProjection` covers `[BE5B25,BE5B32)`. The independent BYTE[frame+8] reload replaces CL without fixing unknown ECX upper bits, then EDX receives captured prior callback EAX. Only normal return from opaque raw `BD58E0` permits the four-byte cleanup. A missing EAX capture preserves the earlier frame-byte read and known CL.

`UnreliableReceiveResidueJoinProjection` composes residue handoff, optional counter continuation, and joined-header preparation. A clean early JBE bypasses the counter; a normally returning helper requires it, including matching retained ESI and receiver. Reloaded payload WORDs remain independent of earlier captured words.

`ReliableReceiveEmptyPublicationProjection` composes only the zero-slot, clean zero-header branch through independent frame+8 publication. A normal nonzero-header callback stops at its result boundary instead; an otherwise matching branch destination cannot bypass forbidden captures or missing prerequisites.

`UnreliableReceiveCallbackPresenceProjection` covers `[BE5B32,BE5B52)` from a validated boundary reached either by the zero-selector branch or normally returning nonzero-marker scalar cleanup. A nonzero receiver+14 DWORD skips the diagnostic. Zero pushes `0`, `2CA`, `1B58C06`, `1B58C07` through absolute cell `1C15EA0`; normal return gates cleanup 16. This first target check does not fix a later target reload.

`UnreliableReceiveDirectCallbackProjection` covers `[BE5B52,BE5B64)`. It pushes zero, zero-extended SI, independent DWORD[payload+4], and the payload+8 address, then independently reloads receiver+14 and calls EAX. Missing later captures preserve preceding pushes; target zero is not guarded in this window.

`UnreliableReceiveMarkerScalarProjection` admits the scalar handoff only after a normal marker-callback return; frame+8 BYTE and callback-return EAX remain independent captures. `UnreliableReceivePresenceCallbackProjection` admits only successful presence outcomes and requires matching receiver identity before projecting the independent callback-target reload. Invalid or unreached captures cannot advance either composition.

The reliable masked-length callback's validated entry also admits the zero-result diagnostic after its normal cleanup, not only the nonzero-result branch.

`ReliableReceiveResultLengthProjection` composes both admitted result routes with the length callback. Only a normally returned zero-result diagnostic projects cleanup 16 at `BE53BC`; that completed cleanup remains visible if the later length capture is missing or invalid.

`UnreliableReceiveCallbackRecycleProjection` covers `[BE5B64,BE5B78)`: global DWORD read precedes cleanup 16, full callback EAX copy to ESI/frame+8, payload push, and opaque helper call. Missing EAX preserves the preceding read and cleanup. `UnreliableReceiveZeroResultProjection` follows normal helper return: nonzero ESI branches away; zero reads receiver+0, pushes receiver+4, reads the first loaded value+4, selects that nonzero value or literal `1C5EA68`, and completes five ordered forwarding arguments.

`UnreliableReceiveZeroReturnProjection` covers `[BE5B9E,BE5BAC)` after normal forwarding return. Cleanup 20 and RET 4 are distinct; XOR AL clears only the low byte, with upper EAX known only from an optional helper-return capture. The epilogue restores EDI/ESI/EBX, resets ESP from EBP, and restores EBP.

`UnreliableReceiveResultStoresProjection` covers the nonzero-result edge `[BE5BAC,BE5BC4)`. It independently reads receiver+4 and frame-4 before either result+20/+24 store, zero-extends only the reloaded frame low WORD for the latter, then compares a fresh receiver+4C DWORD unsigned with `10000`. Receiver base zero is not an entry guard; missing counter capture retains both completed stores.

`PayloadXorDiagnosticEntryProjection` covers `[BD58E0,BD5912)`. Markers 1..3 skip diagnosis; 0 and 4..255 diagnose but are not rejected. A normal diagnostic return independently reloads BYTE[EBP-1] before cleanup 16. The retained EDX-to-EBX scalar and potentially different reloaded AL reach the later table setup; no XOR body or buffer safety is inferred.

`PayloadXorTableSetupProjection` reads the physical frame+8 length and two independently captured table DWORDs selected by all eight AL bits before testing the unsigned wrapped end. Empty/wrapped ranges still require both table reads. Alignment and unsigned start+8/end guards select the first body boundary without projecting XOR writes.

`UnreliableReceiveSelectorCallbackProjection` joins the header's zero-selector route or normal nonzero marker/scalar route with the presence/direct callback. Payload, receiver, selector, and retained length identities are checked before later-stage effects; the scalar frame BYTE and callback targets remain independent. `UnreliableReceiveCallbackRecycleJoinProjection` then requires a normal direct return and matching payload identity before any recycle continuation effect.

`ReliableReceiveLengthPublicationProjection` composes normal length callback, scalar helper cleanup, and publication with payload identity validation and an independently captured frame+8 pointer. `UnreliableReceiveFrameHandoffProjection` covers shared `[BE5C0F,BE5C26)` address-only handoff to `BE64A0` and its normal AL=1 epilogue; unknown helper EAX does not block the known low-byte effect.

`PayloadXorReturnProjection` covers `[BD594E,BD5954)`: restores ESI/EBX, resets ESP from EBP, restores EBP, and returns without immediate cleanup or an EAX write. Optional captured EAX is preserved, not interpreted as the input pointer. `UnreliableReceiveZeroForwardReturnProjection` admits the zero-AL return only after the exact normally returned forwarding outcome, never the nonzero-ESI branch at the same end offset.

`UnreliableReceiveHighCountForwardProjection` preserves six forwarding pushes, normal-call gates, an independent receiver+8 reload, cleanup 24, and wrapped object+1C increment before `BE6440`. `UnreliableReceiveHighCountCallbackProjection` dereferences that helper's independent EAX, skips the callback only for a zero first loaded DWORD, otherwise reads a second DWORD, pushes 1, and reads/calls its +8 target.

`UnreliableReceiveHighCountCounterProjection` uses a fresh receiver+4C reload after those calls. DEC wraps; post-DEC zero writes receiver+48 without reading it, while nonzero independently reads/increments receiver+48. Missing later captures retain the completed DEC. An unreached receiver+48 capture on the zero path is flagged even though both visible writes are projected.

`PayloadXorEntrySetupProjection` requires admitted diagnostic-entry outcomes plus matching frame, retained EBX, and boundary AL before any stack/table reads. `UnreliableReceiveBelowThresholdReturnProjection` constructs the frame handoff from the stored receiver/frame identities only on the exact below-threshold outcome; high-count paths stop before their unmodeled cleanup rather than jumping to the shared return.

`UnreliableReceiveHighCountSuffixProjection` admits counter updates only after a clean zero-object branch or normal indirect callback return. It preserves partial DEC effects but rejects unreached zero-path counter+48 captures as complete joins.

`UnreliableReceiveHighCountResultProjection` admits forwarding only after the exact at-or-above-threshold result and matching receiver identity. Receiver+4 is independently reloaded for forwarding; its value need not equal the prior result-store input. Partial forwarding retains completed result stores and its own earlier pushes/cleanup.

`UnreliableReceiveHighCountReturnProjection` enters the shared frame handoff only after an unflagged completed suffix, carrying the counter receiver and an explicit frame capture. `HighCountObjectHelperEntryProjection` passes a local frame-0C address to opaque `BE6B30`, then independently captures its returned EAX address and loaded DWORD. Zero branches to `BE6458`; nonzero stops before the additional read at `BE6456`.

`FrameHandoffHelperEntryProjection` preserves independent receiver+0C/+10 reads, wrapping sum and low-bit gate, then compares receiver+8 unsigned against `(receiver10+4)>>2` with DWORD wrap before the shift. Both bypass branches join `BE64C5`; otherwise receiver10 is pushed and opaque `BE6B50` called. Missing reads preserve earlier captures and stop at their instruction, while missing/nonreturn call evidence stops at `BE64C0`. No receiver writes occur in this window.

`FrameHandoffHelperSlotProjection` models the `BE64C5` continuation's mask write to receiver+0C separately from its fresh reload. Independent receiver+8/+10/+4 and indexed-slot reads determine the zero fallthrough at `BE64F2` or nonzero branch to `BE650E`. `UnreliableReceiveHighCountLifecycleProjection` composes successful result forwarding through callback/counter/shared return; counter receiver mismatch and unreached callback captures block counter entry before writes.

`HighCountObjectHelperReturnProjection` models `BE6458..BE6477`: independent reads at helper-EAX+8 and retained-ESI+8/+4, wrapping decrement and masked index, indexed DWORD reload, then full-EAX address arithmetic and plain RET. Missing register prerequisites mark supplied dependent reads unreached; zero register bases remain captured values, not absence.

`FrameHandoffHelperEntrySlotProjection` admits the three exact unflagged `BE64C5` entry routes and checks receiver identity before slot reads/writes, without equating independent reload values. `FrameHandoffCapacitySeedProjection` records the receiver store at frame-14 before the receiver+8 read, seeds ESI with one, and replaces that seed only for a nonzero read before the `BE6B70` join.

`HighCountObjectHelperLifecycleProjection` joins the helper entry and indexed return, independently loading `[first loaded DWORD]` at `BE6456` only on the nonzero route. The zero route supplies retained ESI zero and rejects any nonzero-only bridge capture before tail effects. Optional return register identity captures are checked only after entry/bridge prerequisites; the composed EAX/ESI values come from the entry and bridge.

`FrameHandoffEmptySlotCallProjection` preserves the push-0x10/direct-call path and zero-result indirect call through cell `1AC4424`, whose returned ECX is independently captured rather than assumed zero. Missing direct EAX stops at the copy instruction before cleanup/test. `FrameHandoffSlotPublicationProjection` separates the `6508` indexed store from the `650E` fresh-base read, then masks EDI, forms ECX and pops EDI/EBX before the test at `651C`; its EBX input is the byte offset computed by the preceding slot projection.

`FrameHandoffFinalWriteReturnProjection` tests ECX, conditionally reads a pointer at frame+8 and then its pointee DWORD before writing to ECX, and independently increments receiver+10 with DWORD wrap before RET4. `FrameHandoffCapacityLoopProjection` projects one iteration only: CMP of wrapped ESI-EDX against one governs the zero-difference branch, followed by the unsigned ESI/eight exit or unsigned `(0FFFFFFF-ESI)<ESI` failure gate; otherwise ESI doubles and selects the loop entry.

`FrameHandoffCapacitySeedLoopProjection` derives a single iteration's ESI/EDX from the seed and validates optional identities before projecting it. `FrameHandoffSlotCallPublicationProjection` carries native receiver ESI, slot byte offset EBX and wrapping sum EDI into publication; nonzero-slot routes skip calls/stores, while zero-slot routes require unflagged call results. `FrameHandoffPublicationReturnProjection` validates formed ECX and receiver ESI before final reads, writes and increment.

`FrameHandoffContinuationLifecycleProjection` uses the effective call-derived publication capture to compose the complete `BE64C5..BE652F` continuation. `FrameHandoffCapacityPrefixProjection` preserves the receiver+C read, ordered frame-8/frame-4 stores and unsigned scaling gate before the call. `FrameHandoffCapacityCallProjection` models cleanup4 before the returned EAX frame store, the nonzero return gate and independent indirect-call return; invalid entry routes and unreached captures cannot authorize successful admission.

`FrameHandoffCapacityTransferProjection` models `BE6BCA..BE6BFD`: signed SAR, independent frame reads, ordered scalar pushes and an opaque indirect-call normal boundary. Its admission rejects unreached later captures. `FrameHandoffCapacityTransferReturnProjection` models the fresh receiver read before cleanup12, wrapping addition of independently captured return EAX and entry EDI, independent frame-8 reload, frame-10 store and unsigned branch to `BE6C10` or `BE6C57`; it makes no callee-preservation claim.

`FrameHandoffCapacityPrefixCallProjection` derives zero bypass, guarded indirect entry or direct scaled-push entry from the prefix, rejects conflicting route captures and validates the frame identity before call effects. Completed prefix stores and pushes remain visible when a later call is blocked.

`FrameHandoffHelperLifecycleProjection` composes the three admitted entry routes through the full frame continuation, validating receiver identity before slot effects. `FrameHandoffCapacityTransferLifecycleProjection` validates receiver/frame identities before the first transfer return reads, but retains independent post-call EDI/ESI and opaque EAX captures. `FrameHandoffCapacitySecondTransferProjection` models `BE6C10..BE6C34`, including signed scaling, the frame-C store, independent call-cell/frame-10 reads and ordered pushes before the opaque call.

`FrameHandoffCapacitySecondReturnGateProjection` preserves the fresh frame-C read before cleanup12 and the independently captured post-call EBX/EAX addition. Its lifecycle composition validates EBP without assuming EBX preservation. `FrameHandoffCapacityPrefixTransferProjection` requires clean prefix-call admission, retaining independent post-call EDI on call routes and deriving it on the zero bypass. `FrameHandoffCapacityAboveTransferProjection` preserves the first push before the call-cell load at `BE6C64`.

`FrameHandoffCapacityRepeatStoreProjection` represents `BE6C41..BE6C4A` with one compressed zero-store descriptor, explicitly captured DF and normal REP completion. It does not assume forward direction or enumerate writes; a missing fresh frame-8 reload retains completed REP effects.

`FrameHandoffCapacityLoopPrefixProjection` admits only the exact unsigned exit of one loop iteration, deriving prefix ESI/EDX without running additional iterations. `FrameHandoffCapacityFillJoinProjection` independently reloads frame-4 before selecting the final-repeat or direct-tail route. `FrameHandoffCapacityGateRepeatProjection` derives nonzero REP inputs from the return gate while rejecting repeat-only captures on the zero bypass.

`FrameHandoffCapacityAboveSecondTransferProjection` preserves independent receiver/frame reads, signed scaling and pushes through the second above-branch call. `FrameHandoffCapacityAboveReturnGateProjection` preserves cleanup24 before the returned EAX addition and fresh frame-4 reload, without assuming post-call EBX/ESI preservation.

`FrameHandoffCapacitySharedRepeatProjection` models the separate `BE6C9E..BE6CA2` compressed directional zero store without a subsequent frame-8 reload. `FrameHandoffCapacityFinalCallGateProjection` independently reloads frame-14 and receiver4 before the optional direct call, exposing outgoing EDI only on the no-call bypass. `FrameHandoffCapacityFinalReturnProjection` preserves the ordered receiver8 ADD, receiver4 overwrite and RET4 without claiming a full EAX result.

`FrameHandoffCapacityFinalLifecycleProjection` admits the return tail only
after the exact clean final-gate outcome. It validates the shared frame and
the no-call EDI identity before the receiver8 read; opaque-call register
captures remain independent. `FrameHandoffCapacityAboveLifecycleProjection`
preserves both calls and the return gate's cleanup-before-LEA ordering,
requiring normal call boundaries and a matching frame for the final reload.
`FrameHandoffCapacityFillSharedRepeatProjection` derives repeat count and
start from the fill join's moves, retaining zero bypass, unresolved DF,
explicit normal completion, and partial plans without claiming stores.

The final direct callee at raw `12C8D60` does not establish unconditional
register preservation. Its exit window `12C9441..12C945C` tests mask `0x02` of the
byte at encoded PE VA `1EDAF84`, restores EDI/ESI/EBX, then branches over an
indirect call through encoded PE VA `1AC427C` when that bit is clear.
The call at raw `12C9452` occurs after the restores when the bit is set;
its register outputs remain unresolved. This is not a whole-callee contract.

`FrameHandoffCapacityRepeatFillJoinProjection` links the first completed
REP/fresh-frame8 reload or exact zero-count bypass to the fresh frame4
fill join. The REP route derives EDI from the independent reload; the
bypass requires an independent EDI capture. Both validate frame identity
before the fill read and retain completed store descriptors when a later
reload is missing.

`FrameHandoffCapacityAboveRepeatProjection` admits shared REP only from the
exact nonzero above-return gate, deriving count/start and treating optional
identity captures as cross-checks rather than missing prerequisites. Its
zero branch rejects repeat-only captures; missing DF and incomplete REP
retain partial effects and cannot admit the final join.

`FrameHandoffCapacityFillFinalLifecycleProjection` derives final-gate
admission only after the clean fill/shared-REP route and validates its
frame before the fresh frame14 read. On the final no-call bypass, EBX
must agree with the earlier fill frame4 reload before any final writes;
the called route retains independent register captures.
`CapacityFinalCalleeExitProjection` models only raw `12C9441..12C945C`,
with ordered TEST/pop/gated-call/epilogue captures. Encoded data operands
are PE VAs, separate from raw instruction offsets; restored values are
not promised as outputs of the subsequent opaque call.

`FrameHandoffCapacityFirstFillLifecycleProjection` derives its return-gate
plan from the admitted second-transfer lifecycle, so supplied later plans
cannot bypass a failed call. `FrameHandoffCapacityAboveFinalLifecycleProjection`
composes the above return/REP route through RET4, validating EBX and ESI
continuity only on the final no-call bypass and propagating unreached gate
captures. The final callee's raw `12C8D60..12C8D78` entry anchor confirms
zero ECX jumps directly to the epilogue after frame allocation, bypassing
the global mask read and EBX/ESI/EDI saves; nonzero behavior remains bounded.

`FrameHandoffCapacityFirstFinalLifecycleProjection` preserves all eight
first-gate/fill-branch/final-call combinations. It derives the downstream
fill capture from the admitted first fill plan, retains partial first REP
effects, and does not invent a missing ESI continuity contract.
`CapacityFinalCalleeEntryProjection` records frame allocation and ordered
stack stores with DWORD-wrapped addresses. Its zero path requires an
independent saved-EBP reload before RET; its nonzero path stops after the
three register saves, with no claim about the subsequent call.

`FrameHandoffCapacityOverflowTailProjection` keeps loop overflow distinct
from the unsigned size guard: raw `BE6CC0` pushes encoded VA `1C607A4`,
then calls through encoded VA `1AC45C0`. A captured normal return ends at
`BE6CCB`; neither nonreturn nor an epilogue is inferred from the call.
The separate callee-prefix and first-validation anchors pin the mask-gated
call at `12C8D82` and the fresh global/ESI4 reads and unsigned/tag branches
at `12C8D88..12C8DA7`.

`FrameHandoffCapacityLoopOverflowProjection` admits the overflow call from
only an exact taken JB in one bounded loop iteration, distinguishing a
reached-but-not-taken guard from an earlier exit.
`CapacityFinalCalleePrefixCallProjection` derives pre-call ESI with wrapping
subtraction, uses the prior mask-test flags without a new global read,
and exposes outgoing ESI only when the opaque call is bypassed.

`CapacityFinalCalleeFirstValidationProjection` pins the ordered global
and `[ESI+4]` reads, unsigned comparison, and low-two-bit comparison,
including defined CMP/AND flags without defining AND's auxiliary carry.
`CapacityFinalCalleePrefixLifecycleProjection` derives the prefix mask
from the entry TEST; `CapacityFinalCalleeValidationLifecycleProjection`
derives bypass ESI and requires independent ESI after the opaque call.
Terminal validation branches with supplied unreached reads are retained
as partial evidence, not admitted as a clean continuation.
`CapacityFinalCalleeEntryValidationProjection` connects the entry/prefix
lifecycle to this validation without admitting validation captures on the
zero-entry RET path. Nonzero validation receives the exact derived prefix
capture, while post-call ESI remains independently captured.
`CapacityFinalCalleeAlignmentDispatchProjection` retains the two initial
frame writes, the AL-bit route, the fresh `[ESI]` subtraction, and the
independent entry-ECX/computed-ECX gates. Its opaque indirect call retains
pre-call register evidence only; captured normal return does not establish
post-call register preservation.
`CapacityFinalCalleeTagValidationProjection` distinguishes the unsigned
ESI/EDX guard, the two independently tested bits of the fresh `[EDX+4]`
DWORD, and the subsequent comparison against fresh global `1EDADE0`.
Branch destinations remain separate evidence routes; equality stops at
raw `12C8F21` without inferring later global updates.
`CapacityFinalCalleeAlignmentTagLifecycleProjection` admits only the clean
AL-bit route and derives tag ESI/EDX from its local register evidence.
`CapacityFinalCalleeZeroExitLifecycleProjection` retains the native global
SUB read/write even when the admitted ECX is zero, then forwards independent
exit captures. Successful return requires the exact RET boundary and clear
unreached exit-capture flags.
`CapacityFinalCalleeTagEqualityContinuationProjection` preserves the ordered
global and `[ESI+4]` writes before the fresh tag and unsigned-limit reads.
Optional tag clearing does not replace later reloads with inferred values.
The direct-call route records only pre-call registers and a captured return
gate, while the unsigned-limit branch stops at the exit entry.

`FrameHandoffCapacityDispatchLifecycleProjection` connects the admitted
first-transfer return to only its selected above or at/below branch.
Successor entry registers come from fresh return reads and local arithmetic;
opaque downstream call captures remain independent. Supplied unselected
branch evidence is retained as unreached rather than executed.
`CapacityFinalCalleeEntryAlignmentLifecycleProjection` derives the original
loaded EAX, masked ECX, and admitted ESI for the alignment window while
requiring independent EBP/ESP captures. Clean alignment branches remain
successful bounded destinations rather than failed calls.
`CapacityFinalCalleeNonzeroValidationProjection` preserves the initial EDI
addition, logical EAX shift, ordered frame stores and fresh memory reads.
The common `12C8E76` destination is an opaque indirect call, not a proven
nonreturning terminal.
`CapacityFinalCalleeFrameComparisonCallProjection` keeps the subsequent
`[EBP-4]` comparison as a fresh reload and models its optional indirect call
separately from the call at `12C8E76`.
`CapacityFinalCalleeTagEqualityLifecycleProjection` derives the equality
continuation registers from the admitted AL-bit path. Clean other tag
destinations remain admitted bounded routes only when no continuation-only
captures were supplied.
`CapacityFinalCalleeSizeComparisonCallProjection` masks a fresh `[ESI+4]`
DWORD and shifts an independently captured EAX before comparison. No prior
shift result is reused across unresolved calls.
`CapacityFinalCalleeAlignmentNonzeroLifecycleProjection` derives the
nonzero-route EAX/EDI/ESI/EBP locally but requires independent EBX evidence.
Each route validates its own final writes and registers, rather than imposing
the two-write prelude's register values on later three-write branches.
`CapacityFinalCalleeTagEqualityExitLifecycleProjection` admits the exit only
from the clean capacity branch at `12C9441`, retaining independent exit reads
and opaque-call outputs. Any supplied optional exit capture on another route
is marked unreached, including an otherwise empty capture.
`CapacityFinalCalleeDescriptorReloadProjection` distinguishes the scaled
LEA address and frame write from subsequent independent frame reloads.
`CapacityFinalCalleeFrameMaskUpdateProjection` captures the variable-count
shift with the x86 low-five-bit count, 32-bit complement, and fresh global
AND read/write without reusing earlier frame or register values.
`CapacityFinalCalleeFrameSizeLifecycleProjection` carries ESI/ESP only across
the frame-call bypass and requires independent post-call captures otherwise;
EAX is always independently captured. A reached but contaminated size
boundary is not admitted as a successful lifecycle.
`CapacityFinalCalleeDescriptorComparisonCallProjection` distinguishes the
two equality bypasses from the unsigned-below and memory-mismatch call
routes, preserving the fresh reads and exact comparison operand order.
`CapacityFinalCalleePairedFieldWritesProjection` records the two ordered
stores at raw `12C8EE9..12C8EF8`, followed by an independent global reload.
It retains both stores when that reload is missing, including aliased
destinations; it does not infer pointer ownership or list semantics.
`CapacityFinalCalleeFrameMaskTagLifecycleProjection` admits the tag join
only after a clean global-mask write and requires independent ESI/EDX.
A clean frame mismatch remains bounded at raw `12C8ED5`; supplying tag
captures on that route rejects admission without erasing earlier effects.
`CapacityFinalCalleeSizeDescriptorLifecycleProjection` joins only the clean
size boundary. It carries ESI across the size-call bypass, requires a fresh
ESI capture after an opaque call, and independently captures EBP/EBX.
Contaminated descriptor destinations retain their native effects but do not
count as admitted successful routes.
`CapacityFinalCalleePairedTagLifecycleProjection` preserves both ordered
stores through a blocked tag continuation and independently captures tag
registers and memory. `CapacityFinalCalleeDescriptorPairedLifecycleProjection`
carries EAX/EBP only across comparison bypasses; opaque call returns require
independent registers before either store is admitted.
`CapacityFinalCalleeDescriptorMaskLifecycleProjection` carries the initial
frame-minus-10 value as ECX across the no-call equality route, not the scaled
descriptor address or the later comparison reload. Its mask reads remain fresh.
`CapacityFinalCalleeLargeSizeFieldsProjection` reconstructs the bounded
raw `12C8F76..12C8FA8` gates, frame write, and paired stores without inferring
ownership. It preserves earlier effects on each missing-read stop.
`CapacityFinalCalleeAlternateFieldSelectionProjection` writes each candidate
address before testing the separately loaded DWORD. It stops at `12C8FC2`
or `12C8FFE`, retaining the first write when the second read is missing.
`CapacityFinalCalleeFieldDescentStepProjection` performs one bounded descent
step only. The selected pointer is freshly reloaded after its comparison,
and the loopback to `12C8FC5` is recorded without executing another iteration.
`CapacityFinalCalleeFieldClearTailProjection` retains the original CMP flags
across the frame store and EDX reload. Both the later frame read and the global
reload after the zero store remain independent, even at aliased addresses.
`CapacityFinalCalleeDescriptorFallbackMaskLifecycleProjection` models the
distinct raw `12C8EB8` opaque call before the mask window. It requires captured
post-call registers, rejects call-only captures on bypasses, and admits only
clean mask destinations.
`CapacityFinalCalleeConditionalFieldStoreProjection` uses an unsigned gate
before its fresh field comparison, writes exactly one selected field, and
tests the retained EAX value to select raw `12C8EF2` or `12C9058`.
`CapacityFinalCalleeLargeSizeAlternateLifecycleProjection` joins only the
clean equality route at raw `12C8FA8`. Other large-size destinations remain
bounded; the alternate reads are fresh even when the prior frame write
aliases their addresses.
`CapacityFinalCalleeIndexedRootMaskProjection` retains the fresh frame-C
read before the unequal branch, three independent index loads, and the
indexed write before its optional mask update. Missing reads stop at their
own instruction; zero, unequal, nonzero-store, and mask routes remain
bounded at raw `12C8EF8`, `12C903B`, `12C9058`, and `12C8EF2`.
`CapacityFinalCalleeDescentClearLifecycleProjection` joins only a clean FDD
terminal, carrying the original frame-derived EDX into the clear tail.
FC5 loopbacks stay single-step; contaminated captures preserve effects
without admitting the continuation.
`CapacityFinalCalleeSelectionDescentLifecycleProjection` carries either
clean FC2 selection's EAX into one descent step, with a separately captured
frame-minus-four reload. The FFE selection bypass cannot consume descent
captures, and FC5 is a bounded loopback rather than an iterative model.
`CapacityFinalCalleeGuardedFieldStoreProjection` covers raw `12C9058..12C906E`:
the unsigned global comparison precedes the ECX store, then a fresh
ESI-plus-10 read selects `12C9080` or `12C906E`.
`CapacityFinalCalleeSuccessorPairedStoresProjection` covers only the
`12C906E` entry, retaining its independent global comparison and ordered
stores before a fresh ESI-plus-14 read selects `12C8EF2` or `12C908B`.
The direct `12C9080` bypass does not execute the preceding paired stores.
`CapacityFinalCalleeIndexedConditionalLifecycleProjection` joins only the
clean unequal root comparison at `12C903B`, retaining frame-derived ECX.
The conditional EBX and EAX remain independent captures; derived ESI is
supplied only when its comparison consumer is reached. Other root exits
stay bounded and reject conditional captures.
`CapacityFinalCalleeSecondFieldTestProjection` models the direct `12C9080`
entry without the preceding stores. `CapacityFinalCalleeClearIndexedLifecycleProjection`
joins only a clean field-clear completion at FFE to indexed reads; fresh
frame and global captures remain independent from the clear-tail writes.
`CapacityFinalCalleeSelectionClearLifecycleProjection` admits clear-tail
execution only after a clean terminal selection/descent at FDD. FFE
bypasses and FC5 loopbacks remain bounded and cannot consume clear-tail
captures; missing reads preserve only effects reached before the stop.
`CapacityFinalCalleeFinalPairedTailProjection` guards the final ECX value
against a fresh global at `12C908B`, then performs the two ordered stores
and jumps to `12C8EF2`. `CapacityFinalCalleeConditionalGuardedLifecycleProjection`
joins only clean nonzero conditional-store routes to `12C9058`; retained
registers are supplied only at reached consumers, with independent later reads.
`CapacityFinalCalleeSecondTestTailLifecycleProjection` joins the fresh
nonzero second-field value at `12C908B` to the final guard, with EAX
remaining an independent live-in. `CapacityFinalCalleeAlternateTagGateProjection`
starts separately at `12C90A2`, masks a fresh DWORD, and retains byte-width
CMP flags. It is reached from the `12C8E51` branch, not by fallthrough from
the final paired stores.
`CapacityFinalCalleeGuardedSuccessorLifecycleProjection` selects the
`12C906E` paired path or the `12C9080` bypass test without projecting an
unselected child. `CapacityFinalCalleeAlternateFieldWritesProjection`
preserves the global write before a fresh low-bit clear and ordered
EDI-derived stores, stopping at the `12C9441` epilogue entry.
`CapacityFinalCalleeIndexedGuardedLifecycleProjection` joins only the
conditional nonzero path, leaving the root's direct table-write path bounded.
`CapacityFinalCalleePairedFinalLifecycleProjection` carries the successor's
fresh second-field value into the final guard, preserving all ordered stores.
`CapacityFinalCalleeGuardedFinalLifecycleProjection` extends both selected
second-field routes through that final guard. The alternate tag/write
lifecycle retains independent reads at an aliased address.
The alternate root gate at `12C90CA` and equal-path writes at `12C90D2`
remain separate bounded projections. The gated epilogue at `12C9441`
records independent stack reads, preserves TEST flags across POPs, and
requires fresh post-call EBP rather than assuming opaque-call preservation.
The alternate root lifecycle joins only equality to its ordered writes;
its clean unequal exit remains bounded. The aligned-size projection stores
the shifted value before the unsigned threshold branch. The alternate
epilogue lifecycle preserves preceding writes on missing stack evidence
or a nonreturning opaque call.
The alternate paired frame loads at `12C910C` retain separate fresh values
even when the first frame store aliases the second load. Both stores precede
the comparison; equality stops before the opaque call at `12C911C`.
The root-size lifecycle follows only the unequal root branch, while the
root-epilogue lifecycle follows only completed root writes. The second
frame gate at `12C9122` uses an independently captured frame value and
stops before its own opaque call when equal.
The shifted-frame gate at `12C912D` reloads `[EBP-4]` independently,
shifts with DWORD wrapping, and keeps undefined shift flags absent.
Equality bypasses the opaque call at `12C9137`.
The aligned-frame lifecycle enters the frame loads only through the clean
below-threshold branch. It supplies retained EBP only when the first frame
store consumes it and rejects later captures on the large-size bypass.
The paired-frame gate lifecycle carries retained EDX/EBP only across its
clean unequal edge. The second-shifted lifecycle leaves equality bounded
at the preceding opaque call and preserves independent subsequent reloads.
The alternate backlink gate preserves both comparisons without entering the
opaque call at raw 0x12C915A. Only the not-below path reads ECX+C; a fresh
field comparison equal to EDX selects raw 0x12C9160. The final-frame gate
then independently reloads EBP-14 and compares it with ECX, bounding equal
and unequal paths at raw 0x12C9167 and 0x12C917C respectively.
The preceding indexed-frame gate independently reloads EBP-4 and EBP-10,
computes the wrapping address 0x1EDADF0 plus eight times the first value,
and stores that address at EBP-C before comparing it with the second value.
LEA performs no memory read. Equal selects raw 0x12C9160; unequal selects
raw 0x12C9151, with the local write retained on both paths.
The paired-shifted lifecycle joins only the clean second comparison's
unequal raw 0x12C912D edge. It preserves the two preceding frame stores,
independently reloads EBP-4, and bounds the later comparison at raw
0x12C9137 or 0x12C913D. Supplied shifted captures on either earlier opaque
boundary are reported as unreached and prevent clean admission.
The alternate mask tail reloads EBP-8 and EBP-4 independently, masks CL
to five bits, complements the shifted value, and applies it to a fresh
global DWORD at VA 0x1EDADC8. Zero-count shifts preserve captured flags
(or remain unknown); the final AND defines its own flags. Unused entry
flags on nonzero shifts prevent clean exit admission.

The alternate successor gate bounds frame equality and fresh-field
equality at raw 0x12C9190, while unsigned-below and field inequality stop
before the opaque call at raw 0x12C918A. The separate successor-write
projection preserves both MOV events even when their addresses alias,
then stops at raw 0x12C92BD. The backlink-final lifecycle enters the final
frame comparison only through the earlier clean raw 0x12C9160 bypass.
The shifted-indexed lifecycle preserves two distinct EBP-4 captures on its
clean equality handoff. The indexed-backlink lifecycle carries the fresh
EBP-10 value into ECX while retaining the preceding local store. Both
compositions reject downstream evidence on their bounded bypass routes.
The common-tail tag-write projection retains its OR result and flags if
ESI is missing, and keeps both stores ordered even at aliased addresses.
The indexed-final lifecycle enters the final-frame gate only on indexed
equality, retaining its preceding store. Its ECX handoff is withheld until
the independent EBP-14 reload makes the consuming CMP reachable.
The large-frame threshold projection compares a fresh EDX+8 value with
EBX, stores it at EBP-14, then independently reloads EBP-10. The MOV
instructions preserve the original CMP flags: JB selects raw 0x12C8E76
or 0x12C91B9 from that earlier unsigned comparison, not the reloaded ECX.
The large-frame root gate requires both independent EDX-relative reloads
before its EBP-10 store. Missing prerequisites stop at their own instruction;
the final JZ selects raw 0x12C91D6 on equality and raw 0x12C91A8 otherwise.
The final-mask lifecycle carries the preceding CMP flags only into reached
zero-count shifts and admits a clean bounded unequal exit at raw 0x12C917C.
The large first-field test independently reloads EDX+14, stores the
LEA-derived address at EBP-4, then tests the fresh value. Its zero and
nonzero destinations are bounded at raw 0x12C91E3 and 0x12C91F0.
The successor-write lifecycle admits both clean gate equalities while
keeping contaminated opaque exits bounded. Mask-common and successor-common
lifecycles preserve all earlier writes when later caller registers are
missing; aliasing does not collapse their ordered write events. Mask-common
also retains shift/NOT flags when the fresh global read is missing.
The large backlink gate independently reloads EBP-14 before reading that
value's plus-C field. Missing EDX prevents CMP completion without discarding
the independently reached memory operand. The common root gate likewise
retains its global read when ESI is missing. Both gates compute all CMP
flags and stop at their exact branch destinations without entering them.
The second field test at raw91E3 independently reads EDX+10, overwrites
EBP-4 with that field's address, and tests the fresh value. Zero selects
9226; nonzero selects91F0. The common root store at raw92D0 writes EDI to
VA1EDADD0 and jumps to9441 without entering the epilogue or changing flags.
FinalMaskCommon and IndexedBacklinkFinal preserve independent downstream
captures and exact clean branch handoffs. CommonWritesRoot keeps both tag
stores before a fresh global read, including aliases. CommonRootStore
admits only fully evidenced equal or unequal exits, never missing operands.
LargeRootThreshold derives EBP only at the child store and retains the
latest CMP flags even if the subsequent frame reload is missing. The
common size gate shifts unsigned EDI by three before comparing against20h;
its destinations are92F6 and9354.
LargeRootFirstField and LargePairedFieldTest gate fresh field reads on
their exact predecessor branches. The paired path requires a zero first
field, preserves both writes to EBP-4, and retains first TEST flags when
the second read is missing. LargeThresholdBacklink independently reloads
the frame despite its preceding store. LargeSuccessorWrites retains the
fresh CMP operand when EDX is absent, records a reached first store when
EBX is missing, and preserves two distinct aliased writes on equality.
CommonRootSize preserves the parent CMP when caller EDI is missing and
enters the size gate only on root inequality. FinalMaskCommonRoot retains
three writes before its independent root read; unused root captures on
the bounded917C path prevent clean admission.
The bounded node reload loads fresh EAX through ECX, retains that address
in EDX, and stops at91F3 without iterating. The saved-frame gate independently
loads EBP-10 and tests it: zero selects92BD, nonzero selects9231.
NodeFieldSelection preserves a separate fresh saved EDX while inspecting
EAX+14 and, only after zero, EAX+10. LEAs preserve each CMP's flags.
CommonIndexedAddress forms the address without reading it and stops at
the opaque9304 call instruction or the clean930A branch.
BacklinkSuccessor derives late EBX from the independently reloaded frame
only after child equality reaches the first store. SavedFrameCommon enters
the tag stores only on the zero frame path. LeafThreshold preserves the
independent global CMP operands and its flags across the saved EDX store
and a fresh EBP-C reload; that reload cannot change the earlier JB decision.
NodeSelectionReload derives the node read address from the selected LEA,
not the previously captured field value. Aliased node reads remain fresh,
and the backward jump to91F3 is a bounded exit rather than another iteration.
RootPairedField composes the existing paired helper, retaining both frame
stores and the last completed root CMP or child TEST. LeafClearStore reads
fresh EBP-4 before writing literal zero; missing read data stops at921D,
and captured zero remains a valid write address.
NodeSelectionLeafThreshold uses the saved frame EDX, not the selected LEA
or EAX, for the global threshold comparison. Its EBP handoff is gated by
the completed child CMP. LeafThresholdClear reloads EBP-4 independently
of the preceding saved EDX store and retains the threshold CMP flags.
CommonSizeIndexed keeps the shifted size index separate from original EDI
and stops at the opaque9304 call or bounded930A/9354 exits. SavedIndexGate
uses fresh EDX+1C to address the independent indexed CMP operand, then
reloads global EBX without changing that CMP's JNZ decision.
CommonMaskSelection bounds the shifted mask TEST before either later path,
with zero-count SHL preserving incoming flags. SavedFrameCommonRoot enters
the root CMP only after the common tag stores complete at92C8, not at their
entry92BD; independent global captures never replace prior writes.
SavedFrameIndexed keeps child EDX independent of the saved ECX value.
CommonIndexedMask preserves the parent CMP flags when the child's masked
shift count is zero and its fresh global is missing; a reached TEST replaces
those flags before either bounded mask destination.
CommonMaskOrStore writes only the OR result to VA1EDADC8 without rereading
that global. SavedIndexedStoreGate performs a new EDX+1C read before the
indexed EAX store; missing EAX retains the attempted store address but cannot
reach TEST or JNZ, and both zero and nonzero destinations remain bounded.
CommonSuccessorThreshold compares the freshly loaded EDX+8 DWORD against
an independent VA1EDADD8 capture, exposing unsigned JB and all CMP flags
without entering either rejection or later writes.
CommonRingWrites preserves all four ordered link stores, including aliasing
and independently captured attempted operands, before the bounded9441 jump.
LargeMaskAndStore reads and updates VA1EDADCC independently, retaining known
global data when EAX is missing and stopping before the common tail at92BD.
SavedIndexStoreLifecycle admits only the exact equality9243 edge, retaining
original EDX but independently reloading the child index at the same address.
The child EAX remains an explicit capture and a missing value preserves the
attempted store and prior CMP flags without advancing to TEST.
SavedMaskShift retains the fresh shift count across later missing frame
evidence and computes NOT without changing SHL flags. MaskSelectionOr derives
both OR operands from the existing selection helper. MaskSuccessor instead
keeps child EDX and both successor reads independent of the earlier mask.
OrRing retains the global OR write before any later missing ring operand.
SavedMaskAnd passes the NOT result into the independent global update,
preserving shift flags until AND completes. IndexedReject bounds the register
CMP/JB at9263 without projecting the following field access.
IndexedFieldGate bounds the fresh [ECX+10] comparison at926B and preserves the
independent EDX operand when the memory evidence is missing. EaxGlobalThreshold
bounds927C's comparison against the separately captured absolute global.
Neither projection enters the selected store or rejection destination.
AlternateRing shares the four-store mechanism while retaining9343's distinct
instruction evidence. SavedIndexReject derives EBX only from the later absolute
reload. RejectField retains ECX into the fresh field comparison; IndexedFieldStore
selects +10 or +14 and preserves CMP flags through the bounded9278 handoff.
SavedIndexedZeroMask keeps both reloads independent of the preceding zero
store, restoring the parent TEST flags when the masked SHL count is zero.
SuccessorRing derives EAX from the fresh successor read and retains EDX,
admitting the alternate ring only at the exact not-below9343 edge.
PostStoreZero bounds9278's TEST/JZ, distinguishing a missing EAX from captured
zero and preserving the selected92BD or927C destination without entering it.
BacklinkNextField retains the9288 store attempt or commitment before independently
reading [EDX+10]. Its TEST uses the fresh value even when the address aliases
the preceding write, and stops before9292 or92A4.
HighSizeEntry retains SHR's intermediate flags but uses TEST's replacement
flags for the935B branch. EaxBacklink admits9288 only after an exact not-below
threshold edge, preserving CMP flags until the child's fresh-value TEST.
HighSizeCeiling performs a full unsigned comparison against FFFF, selecting
the936F BSR entry or setting ECX to31 before the9384 store boundary.
NextFieldThreshold keeps ECX and the absolute global operand independently
captured at9292 and bounds the unsigned branch before the paired stores.
IndexedFieldStoreZero derives TEST's EAX from the committed store value,
retaining CMP flags when an earlier store is incomplete and replacing them
only after TEST executes at9278.
BacklinkNextThreshold derives the next CMP's ECX from the independent fresh
read, not the stored backlink value. HighSizeCeilingLifecycle carries the
shifted EAX into the ceiling check. NextPairedStores retains both directional
MOV writes, including aliased destinations, before the92A4 read boundary.
HighSizeBitScan models the936F BSR, committed9374 frame store, masked SHR,
and final index LEA through9384. Zero BSR leaves its destination undefined
and prevents further projection. The actual AND and final LEA begin at
raw012C937E and012C9381; the subsequent store remains outside this helper.
NextThresholdStoresLifecycle admits the exact929E threshold fallthrough
before its two directional stores. Parent CMP flags survive both MOVs,
including a missing store-address stop; the independent global observation
remains ordered before writes. A bypassed late EAX capture is explicitly unused.
IndexedNodeInitialization models the9384 field store, address-only indexed
LEA, EDX initialization, and two ordered zero stores through93A1. Partial first
store operands survive without a committed write. RightFieldGate independently
reloads [EDX+14], tests that fresh value, and stops at92BD or92AB without
entering either destination.
RightFieldThresholdGate compares ECX with an independent absolute-global
DWORD and preserves either known operand when the other is missing. FreshMaskGate
reads its separate global before SHL/TEST/JNZ; known shift operands remain
visible independently during partial stops. Counts are masked to five bits,
and TEST replaces the shift flags before selecting93AC or93C0.
HighSizeIndexLifecycle derives the BSR input from the exact ceiling handoff
and reuses original EDI for the later shift, bounding the composed index to31.
Zero/above-ceiling bypasses leave a supplied late EBP explicitly unused;
missing EBP cannot admit clean completion. SelfRingTail preserves its committed
global OR store during partial local-store stops, then records four separate
ring writes before the unentered942F jump.
RightFieldThresholdLifecycle derives the comparison register from the fresh
field observation, retaining parent TEST flags until the independent CMP
executes. IndexedCountClassification retains its fresh indexed DWORD before
the31 comparison; the equal path clears EAX without assuming its incoming value,
while the alternate path shifts ECX and subtracts it from19h.
CleanupCounterGate records the independent global counter read and decrement
write, preserving incoming CF even when the counter observation is missing.
The nonzero branch stops before9441; the zero path sets ECX to the address
immediate and stops before the943C call, whose callee is never entered.
MaskedTagEqualityGate preserves the original count in ECX and shifted EDI
in EBX while masking an independent fresh tag DWORD for comparison. A zero
effective shift count preserves AND flags. RingInsertion retains five ordered
stores through942F, including incomplete attempts and earlier commits when
the second-store EDX capture is missing.
DualPointerThreshold loads both independent observations before its first
comparison, even when that comparison branches below. FreshMaskSelfRingLifecycle
derives both OR operands from the prior gate and rejects unused late pointers
on bypass. HighSizeNodeLifecycle derives the initialization index across all
three size paths, preserving parent flags and frame-before-node write order.
TraversalIteration models one indexed child selection and masked comparison,
stopping at equality, zero-child, or loop-back edges without another iteration.
The zero-child destination at raw `0x012C945C` is additional CMP/JB code after
the preceding RET, not a return instruction. ZeroSlotThresholdGate captures
both same-instruction operands independently. DualThresholdRingLifecycle
derives the insertion pointers from the two fresh reads and preserves CMP2
flags through five ordered stores, stopping before the cleanup decrement.
IndexedCountTagLifecycle derives the tag-gate count and pointer from the
indexed classification, with independent late tag data and preserved flags.
RightPairedStores models the two owner-field MOVs at raw `0x012C92B7`,
including same-instruction partial operands and distinct ordered alias stores.
ZeroSlotStoreTail keeps EDX as the owner-field source and stops at its direct
cleanup jump. NodeFreshMaskLifecycle carries node-derived index/bit operands
into the fresh global gate, rejecting dirty parent handoffs. TagTraversalLifecycle
derives a single iteration from the unequal tag edge. RingCleanupLifecycle
admits an independent counter decrement only after both non-borrowing thresholds
and all five ring stores; its incoming carry is necessarily clear.
RightFieldStoresLifecycle derives the paired stores' ECX from the fresh field
and preserves the threshold's typed flags. ZeroSlotStoresLifecycle carries the
CMP operands into the four-store tail, separating bounded exits from clean
completion when late pointer captures remain unused.
SelfRingCleanupLifecycle and NodeSelfRingLifecycle compose the exact fresh-mask
zero edge into ordered self-ring stores; cleanup observes its counter independently
after the stores. ZeroSlotCleanupLifecycle similarly preserves the four-store
sequence and full compare flags when the counter read is missing. DEC preserves
the admitted clear carry, and neither cleanup destination nor call is entered.
TraversalZeroSlotLifecycle derives the threshold slot from the single child LEA,
rejects unused child-plus-four captures, and distinguishes internal threshold
entry from its unentered outer destination. RightFieldTagLifecycle admits both
clean right-field routes into the shared MOV/OR/tag stores and stops before
raw `0x012C92C8`, preserving parent flags until OR executes.
RingCleanupEpilogueLifecycle admits only the exact nonzero cleanup edge into
the existing bounded epilogue. Nullable late ESP/EBP and fresh TEST/stack/call
observations remain independent. A captured normal opaque-call return leaves
all arithmetic flags unknown rather than forwarding pre-call TEST flags.
TraversalSlotStoresLifecycle carries the derived child-slot ECX and original
traversal EDX into four ordered pointer stores and their cleanup jump.
RightTagRootLifecycle retains both right-field routes and independently
observes the root DWORD at VA `0x01EDADDC` after tag writes, including aliases.
NodeRingCleanupLifecycle observes the counter after initialized-node and
self-ring writes, preserves OR flags on a missing read, and separates clean
completion from a bounded bypass carrying unused counter data.
IndexedTagTraversalLifecycle carries the classified count and loaded pointer
through one tag comparison and one traversal iteration, without repeating the
fresh tag read at raw `0x012C93D8`. ZeroCleanupEpilogueLifecycle joins the
four-store cleanup's exact nonzero branch to the existing epilogue; missing
entry registers and later observations remain explicit, and a normal opaque
call return invalidates all six arithmetic flags.
NodeCleanupEpilogueLifecycle adds the same bounded epilogue to the exact
initialized-node/self-ring cleanup path. It retains node and counter evidence,
keeps zero-counter and nonzero-mask bypasses distinct, and preserves DEC flags
when the internal TEST read is attempted without a captured byte.
IndexedTraversalZeroSlotLifecycle derives the threshold ECX from the indexed
traversal's child-slot LEA. It rejects dirty zero-child handoffs before the
threshold read, retains fresh child/global observations even when they alias,
and stops before either threshold destination.
TraversalSlotCleanupLifecycle appends an independent counter observation to
the four-store traversal route, distinguishing reached instruction attempts
from execution when operands are missing. RightRootSuccessorLifecycle carries
consumed EDI into the root-equal global store/jump or the root-unequal
SHR-by-three size gate, without entering either helper's selected destination.
IndexedTraversalSlotStoresLifecycle extends the indexed count/tag zero-child
route through the four link stores and its cleanup jump. The child-slot
address supplies ECX, while the original loaded indexed pointer supplies EDX;
neither the indexed-slot address nor the captured zero child is substituted.
NodeMaskIndexedLifecycle connects the nonzero node-mask branch to independent
indexed and tag reads using the initialized node's index and slot. It also
preserves the zero-mask bounded exit and partial write/read observations.
TraversalCleanupEpilogueLifecycle joins the exact nonzero counter handoff to
the gated epilogue, retaining ordered effects and nullable stack captures.
Completion requires the observed RET boundary; a normally returning opaque
call makes all arithmetic flags unknown even if a later capture is missing.
RightRootIndexedMaskLifecycle carries the already-classified small-bin count
into the address-only LEA and mask gate without repeating size classification.
Its independent global read follows the earlier root effects, including
aliases; dirty unused captures affect clean completion, not selected bounded
exits at the opaque-call instruction or mask-branch destinations.
IndexedTraversalSlotCleanupLifecycle appends the independent counter
read/DEC/write/JNZ to the exact indexed four-store handoff. Earlier missing
indexed, compare, child-read, and store operands remain reached attempts,
not executed instructions; the cleanup branch destinations stay unentered.
NodeIndexedTraversalLifecycle extends the exact nonzero indexed-mask/tag
unequal handoff through one traversal iteration. EBX comes from the completed
tag shift, EDX from the fresh indexed pointer, and EDI from the original size.
It does not reread the tag or enter another iteration at the loop boundary.
IndexedCleanupEpilogueLifecycle extends the indexed counter's exact nonzero
handoff to the gated epilogue, preserving ten parent effects before the TEST
and independent stack/call observations. RightRootSmallBinRingLifecycle
extends the mask branches through their OR/ring or successor-threshold/ring
tails, deriving inherited ESI and preserving independent fresh thresholds.
Neither ring tail enters the selected epilogue or abort destination.
NodeIndexedTraversalZeroSlotLifecycle connects only the clean zero-child
branch to a fresh threshold CMP/JB. The LEA child-slot address supplies ECX,
missing threshold data preserves the prior TEST flags, and unused child data
blocks admission without erasing the already selected bounded exit.
NodeIndexedTraversalSlotStoresLifecycle extends the admitted not-below
route with four ordered stores and the cleanup jump, retaining original
indexed EDX rather than the zero-child value. SmallBinEpilogueLifecycle
admits either completed ring tail directly to the gated epilogue without
a cleanup-counter decrement; equal-root bypasses remain distinct.
NodeIndexedTraversalSlotCleanupLifecycle observes the fresh counter
decrement after the four-store jump, preserving carry and bounded
zero/nonzero destinations without entering the cleanup call or epilogue.
NetworkIoCompletionPassProjection observes one bounded dequeue pass:
at most one 500-ms fallback, one hundred counted attempts, independent
error/key/OVERLAPPED reads and opaque return observations. Missing stop
data or unused captures cannot claim clean completion; null dispatch
keys remain bounded rather than invoking an inferred callback.
GameSocketSenderBindingProjection covers the optional post-admission
flag branch through token selection, opaque queue append and flag clear.
It keeps disabled bypasses, missing operands and unused captures distinct,
and separates historical TEST flags from flags invalidated by normal calls.
GameSocketCompletionErrorStateProjection starts at the internal
post-log/release continuation: it observes compare-exchange state ten,
conditionally copies the fresh field and queues token two. It does not
model the outer zero-error bypass, concurrency execution or SEH cleanup.
CapacityFinalCalleeCommonNodeIndexedCleanupEpilogueLifecycleProjection
joins the indexed node cleanup counter to the shared gated epilogue.
The zero-counter path requires a separately observed normal helper return;
the nonzero path enters directly. Pending calls preserve preceding flags,
while normal opaque returns invalidate them. Unused captures spoil clean
admission without erasing an observed bounded child return.
NetworkIoCompletionClockGateProjection models the predequeue unsigned
500-unit clock gate, with independent global, field and saved-stack reads.
Its cached clock target is not reread from the IAT, and dispatcher calls
remain opaque. GameSocketReceiveResultGateProjection models the separate
receive-result BOOL/error-996 branch and dynamic slot-0x20 callback,
stopping before success preparation or common cleanup. The error import
is identified as WSOCK32 ordinal 111, not a named API.
GameSocketDatagramContextDispatchProjection continues the exact clean
sender-binding tail using a fresh context DWORD, not the earlier admission
classification. It records wrapped parser arguments and opaque parser
return observations. The nonzero path independently reads the high-water
DWORD before its unsigned compare and optional original-count store;
all three endpoints stop before native pops or return instructions.
CapacityFinalCalleeCommonNodeIndexedEqualityRingLifecycleProjection
joins only the nonzero child's exact masked-equality fallthrough to the
existing dual-threshold ring tail. It preserves distinct outer equality,
zero-child and loop-back exits, inherits the child-derived EDX, and retains
fresh threshold reads before the five stores. Completion stops before DEC.
NetworkDispatcherLockGateProjection starts after the dispatcher's first
opaque import call. It separates the zero-result fallback call from the
direct path, observes a fresh counter increment, and compares a byte
argument before the frame and receiver-byte clears. INC preserves the
preceding carry flag, including an unknown carry after a normal opaque
return. Both branch destinations remain unentered.
GameSocketCompletionErrorQueueAcquisitionProjection joins the exact
pre-call error-state boundary to the existing queue acquisition prefix.
Captured acquisition results assume a normal opaque return; missing
results stop at its result store. The tail-tag read remains unentered.
GameSocketReceiveSenderPreparationProjection joins a clean nonzero
receive-result branch to the independent operation reload and sender
helper call boundary. A zero operation requires explicit diagnostic-stack
observations, including an independent stack pointer after normal return.
Neither projection executes the imported or direct callees.
CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupLifecycleProjection
joins the exact five committed ring stores to the independent cleanup
counter read and DEC/JNZ gate. The zero path stops before its cleanup
call, while the nonzero destination remains unentered; neither path
claims epilogue completion.
GameSocketReceivePacketCallPreparationProjection observes a normal
sender-helper return before pushing its independent result and freshly
read stack count. It stops before the fixed packet call at raw BD2F25;
packet execution, reposting and return code remain outside the contract.
GameSocketParserLocalSizeDispatchProjection observes the fresh local
vtable and slot-C target after constructor return. It derives the resize
receiver from an independently captured post-getter ESP, not the earlier
local address, and stops before the resize call. Pending or nonnormal
getter observations preserve the incoming flags at the call boundary.
CapacityFinalCalleeCommonNodeIndexedEqualityRingCleanupEpilogueLifecycleProjection
joins the five-store cleanup counter paths to the shared bit-1 TEST
epilogue. Only an observed normal zero-counter cleanup return enters
that epilogue; unused return captures dirty clean admission without
erasing an actual bounded RET.
GameSocketCompletionErrorQueueAttemptLifecycleProjection joins a clean
nonzero acquisition to one generic append attempt. It keeps tag and tail
reads independent, compares returned EAX before EDX, and stops at retry
or pre-publication success. CMPXCHG8B changes only ZF; the preceding OR
provides the preserved non-ZF flags. No retry or publication is executed.
GameSocketReceivePostPacketStackPreparationProjection observes fresh
post-packet registers and an independent operation read before the
three stack stores. The nonzero destination and zero-path diagnostic
call remain unentered; this is preparation evidence, not a full repost.
GameSocketParserPostResizePointerProjection observes independent
post-resize stack and DWORD reads, preserves branch-specific TEST/XOR
flags through two stores, and records EDI as 0x56E. It stops before the
first header-byte read, including when the derived address is zero.
NetworkDispatcherCollectionGateProjection preserves the signed threshold
gate and opaque logging boundary, then uses independent end/start and
first-entry reads. Selected empty, zero-entry, and nonzero-entry
continuations remain unentered.
CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingLifecycleProjection
composes root indexed equality with both fresh threshold reads and the
five ring stores. Unsigned bypasses remain distinct from clean completion;
the decrement at raw 0x12C942F remains unentered.
GameSocketCompletionErrorQueuePublicationLifecycleProjection admits only
a clean single-CAS success. It preserves fresh link, stack cleanup,
payload, and node reload observations and distinct capture boundaries.
Its generic return-value projection does not establish an actual native
RET or the outer queue call's normal return.
CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupLifecycleProjection
admits the fresh counter decrement only after validating the root
equality path's five ordered stores. It preserves carry and stops before
the nonzero shared epilogue or zero-path cleanup call.
GameSocketReceiveOperationReloadGateProjection models the direct
continuation and the earlier diagnostic's explicitly observed normal
return. Fresh post-call registers and an independent operation reload
drive the stack store and TEST; it stops before the next diagnostic
call or the nonzero destination, without claiming a repost.
GameSocketParserFirstByteFieldsProjection records the first byte's four
bit partitions and two byte stores, exhaustively covering all 256 scalar
values. Final ECX contains the low three bits while flags retain the
earlier bit-3 AND result; the second byte remains unread.
NetworkDispatcherEntryCallPreparationProjection preserves first-entry
TEST flags through zero-entry argument pushes or a fresh nonzero vptr
read. It stops before reading either indirect target or entering its call.
CapacityFinalCalleeCommonNodeIndexedOuterEqualityRingCleanupEpilogueLifecycleProjection
joins the root ring/counter path to the shared bit-1 epilogue. A zero
counter requires explicit normal cleanup-call return; pending calls
preserve DEC flags and do not count as completed epilogues.

`FrameHandoffCapacityAboveBranchLifecycleProjection` derives its tail
return-gate capture only from a clean above-capacity lifecycle.
`FrameHandoffCapacitySecondBranchLifecycleProjection` forwards only an
admitted second-transfer return gate into the first/fill/final chain.
Both retain earlier writes and pushes when downstream evidence is missing
or unreached; supplied later captures cannot manufacture successful admission.

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
