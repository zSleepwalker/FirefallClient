# Process startup (partial)

The current dispatcher entry continuation reconstructs the zero-result fallback
as a fresh stack PUSH, a distinct indirect import return, and a counter INC
using independently captured post-call EBX and DWORD values. Its next comparison
is byte-sized (`80 7D 08 00` at raw BBDDF9), with a fresh EBP and argument byte;
earlier counter writes do not supply that read even when addresses alias.
After frame/receiver clears, JZ at raw BBDE08 selects BBDECA for argument zero
or BBDE0E otherwise. The latter compares fresh DWORD[VA1E68590] with one;
JG at BBDE15 uses signed greater-than, selecting BBDE2B or BBDE17.
The non-taken path projects independent fresh ESP/ECX into PUSH ECX and PUSH 0,
stopping at BBDE1A before the next diagnostic argument preparation. From there,
the continuation now includes diagnostic argument setup through the direct CALL
at BBDE26, bounded return at BBDE2B, and call-target arithmetic
(`MOV/XOR/SUB/SAR/TEST`) through BBDE3D with a stop before the JZ at BBDE3E.
Current source-level bounded branch coverage includes:
- JZ not-taken path `BBDE40..BBDE49`, stop before `BBDE4B` JNZ.
- JZ taken path to `BBDE83`, stop before indirect CALL `BBDE86`.
- Nonzero-path call boundaries at `BBDE5D` and `BBDE6C`, each with explicit
  normal-return stop at `BBDE63`/`BBDE6F`.
- Nonzero post-call arithmetic and JB boundary through `BBDE81`, with both
  branch destinations modeled.
- Zero-path post-call continuation `BBDE8C..BBDE96`, stop before `BBDE97` JZ.
- Zero-path `BBDE97` split and fallthrough window through `BBDEA2`.
The focused dispatcher run now rebuilds and executes from the matching focused
output path and reports `Failures: 0`, including the nonzero/JB and zero
fallthrough continuations. Earlier stale-focused-binary reports remain
historical and are not accepted as validation evidence.

The parser diagnostic cleanup continuation now includes both first-branch
(`BD34E2..BD34F6`) and second-branch epilogue families. First-branch FS restore,
register POPs, and frame/RET8 compile, but previously cited focused success
included stale-runner output and is not claimed as current-source runtime
acceptance.
Second-branch FS restore/register POPs/frame-return now complete conclusive
fresh focused runs with `Failures: 0`. All of these remain offline value-only
projections: missing captures stop at precise boundaries, no native target is
executed, and no caller continuation is inferred unless explicitly modeled.

Normal second-call parser suffix checks (`SecondVirtualCallReturn`,
`SecondCallFsRestore`, `SecondCallRegisterPops`, `SecondCallFrameReset`,
`SecondCallReturn`) now complete in focused runs with `Failures: 0`; they are
still intentionally not registered in the main `Program.cs` suite list until
the untracked pair files are explicitly accepted and staged.

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


## Topic files

- [UI manager initialization, callbacks, and teardown](StartupUiManager.md#ui-manager-initialization-and-payload-teardown)
- [Shader-source and pool lifecycle](StartupShaderPools.md)
- [Queue, socket, and reliable/unreliable receive projections](StartupQueuesAndReceives.md)
- [Frame-handoff capacity projection](StartupFrameHandoff.md)
- [Capacity final-callee projections](StartupCapacityFinalCallee.md)

## Offline projections

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
[native prefix-only introsort](MatrixCollectionAndSync.md#byte-buffer-introsort-pivot-partition-and-insertion),
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
