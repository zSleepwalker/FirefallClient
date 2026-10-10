# `tfLogin` app mode (partial)

`Client_ApplyPendingAppModes` maps mode 1 to a `0xA58`-byte `tfLogin` object.
Its primary vtable is VA `0x1C676C0` (raw `0x18662C0`), and the constructor is
raw `0x6365B0`. The RTTI name is `.?AVtfLogin@@`; the class derives from
`tfAppMode`. Addresses are raw file offsets unless marked VA; see
`BinaryLayout.md`.

## Lifecycle and object layout

The dispatcher invokes virtual slot `+4` as Initialize, slot `+20` as
Shutdown, and slot `0` as the deleting destructor. The concrete methods are
raw `0x636FF0`, `0x63B360`, and `0x636AD0`, respectively. Failed initialization
logs `Failed to initialize AppMode_Login` (raw `0x173FF34`) before shutdown
and deletion. On success, the application records an initialized flag and
`timeGetTime()` timestamp.

The constructor installs a base `tfAppMode` vtable first (VA `0x1C67710`,
raw `0x1866310`), initializes the callbackable subobject, and then installs
the `tfLogin` vtable. Selected verified members:

| Offset | Observed value or role |
| ---: | --- |
| `+0x10` | Allocated zone/scene object (size `0x338`) |
| `+0x14/+0x18` | Initially zero; later used as a reference-counted pair |
| `+0x20` | Owner/context argument; constructor asserts non-null and `*owner == 0` |
| `+0x24` | Another owner/context argument; constructor asserts non-null |
| `+0x50` | Input context object, constructed with ID 9 |
| `+0x9A0` | Allocated object (size `0x2E8`), associated with login shader parameters |
| `+0x9A8/+0x9AC` | `KernelSize` and `ScaleTex` parameter names |
| `+0x9B0` | Shader chain `shaders/loginblur.oShdChain` |
| `+0x9BC` | Cvar name `rd.loadingScreenOpacity` |

Other constructor arguments and fields are preserved only where their roles
were established; this is not a complete native struct definition.

## Initialization behavior

Initialize loads `LoginUI.xml` through raw `0x30AF50`; failure logs
`Couldn't load Login GUI` and returns false. It registers 15 named input
actions on the context at `+0x50`: `LeftClick`, `RightClick`, `ArrowUp`,
`ArrowDown`, `ArrowLeft`, `ArrowRight`, `ScrollUp`, `ScrollDown`, `ToStart`,
`ToEnd`, `Delete`, `Tab`, `Submit`, `Escape`, and `RunConsoleMacro`. It
registers that context on the global layer stack and shutdown unregisters it.

The initializer reads `Development/ConsoleMacro` and may set it through a
configuration callback. In the demo path, if both `Demo/DemoMode` and
`Demo/DefaultSettings` are true, it deletes `<base>/ui_savedsettings`
recursively if that directory exists and removes `<base>/keybinds.json`.
These are observed client behaviors only; the reconstruction performs no file
deletion.

The login zone object is loaded from `this+0x10`; inability to load the login
screen zone logs `Unable to load Login screen Zone` but is not fatal. A later
scene-picker call at raw `0x639840` is fatal if it cannot choose a background.
The picker gathers candidates using `Development/InstallDir` plus fallbacks,
seeds `rand()` once, selects a background entry from a 0x68-stride table at
VA `0x1DFAEF0`, and references
`gui/components/LoginUI/LoginBackground/title%02i.dds` and `fade`. The table
entry semantics and exact candidate-key behavior remain unresolved.

If a global flag is set, initialization assigns 30 to a global `+0x168` value;
shutdown clears it. The initializer also ensures a cvar at VA `0x1E742C0` is
100. The exact cvar names and full semantics of these globals are not known.

## Response handlers and automatic login

Three server-response routines have been identified by their parsed fields and
behavior:

| Raw function | Observed purpose | JSON keys / errors |
| ---: | --- | --- |
| `0x63B7C0` | Login response | Keys: `can_login`, `is_dev`, `steam_auth_prompt`, `skip_precursor`, `cais_status`, `state`, `duration`, `expires_at`, `character_limit`, `is_vip`, `vip_expiration`. Errors: `login_failed`, `ERR_INVALID_LOGIN`. |
| `0x63C5F0` | Character refresh response | Keys: `characters`, `character_guid`, `name`, `visuals`. Error: `characterlist_failed`. |
| `0x63D270` | Zone-list response | Logs `AutoLogin: failed to pick zone by "%s"` on the observed failure path. |

The constructor creates three `0x20`-byte callback wrapper objects. Each stores
the Login owner at wrapper `+4` and a target function pointer at wrapper
`+0x10`. Those pointers exactly match the three handlers above after
translating their PE VAs through `BinaryLayout.md`:

| Login member | Target VA | Target raw offset | Observed handler |
| ---: | ---: | ---: | --- |
| `+0x9D8` | `0xA3C3C0` | `0x63B7C0` | Login response |
| `+0x9E8` | `0xA3D1F0` | `0x63C5F0` | Character refresh response |
| `+0xA00` | `0xA3DE70` | `0x63D270` | Zone-list response |

The target immediates are initialized at constructor raw offsets `0x636743`,
`0x6367C2`, and `0x63684C`; each matching object is then retained in the Login
member through the same reference-counted wrapper helper. This directly ties
the callback objects to their handlers. `Login_QueueLoginRequest` at raw
`0x63B550` copies the first callback/member pair at `+0x9D8/+0x9DC` into
offsets `+0x50/+0x54` of a newly allocated `0x78`-byte request object, sets
that object's type field at `+8` to 2, and submits it through the global
request manager. The Login object uses `+0x9E0` as an in-flight guard and
records the caller's option byte at `+0x9E2` after successful submission.
Thus the login request carries the Login-response callback. The exact runtime
handler dispatch is now traced through the shared request worker below.
The other two wrappers' request attachment sites are also identified.

All three request paths allocate `0x78` bytes, initialize the object through
raw `0x3843F0`, attach the callback pair at request `+0x50/+0x54`, prepare
through `0x636550`, and submit through `0x6499F0` to manager `0xBC43C0`.
The request-type DWORD at `+8` selects the WinHTTP verb in worker `0xBC2B30`:

| Purpose | Builder raw | Callback attachment raw | Login pair | Type / verb | Endpoint format |
| --- | ---: | ---: | ---: | --- | --- |
| Login | `0x63B550` | `0x63B653 / 0x63B659` | `+0x9D8/+0x9DC` | 2 / POST | `%s/api/v2/accounts/login` |
| Character refresh | `0x63ECE0` | `0x63EDBC / 0x63EDC2` | `+0x9E8/+0x9EC` | 1 / GET | `%s/api/v2/characters/list` |
| Zone list | `0x63E390` state-2 branch | `0x63E56C` (pair-copy helper) | `+0xA00/+0xA04` | 2 / POST | `%s/api/v1/server/list` |

Character refresh returns without submitting when Login `+0x9F0` is
nonzero. It sets that byte to 1 only when submission succeeds. Failure
destroys the request and logs `Unable to queue refresh characters request
to %s`. The pair copy explicitly retains the source control block and
releases the previous destination control block.

The zone-list construction lies in the state dispatcher at `0x63E390`,
which reads the DWORD at Login `+0xA48`. Its indirect jump table VA
`0xA3F574` translates to raw `0x63E974` in **.text**, not .rdata. State 2
enters raw `0x63E454`; if the text at Login `+0xA1C` is nonempty, it calls
the character-refresh builder, then conditionally constructs the zone-list
request based on additional text/parse checks. This is not an unconditional
request on every state-2 visit. The zone request copies the `+0xA00` callback
pair via helper `0x6448E0`, stores `application/json` at request `+0x28`,
and adds a `build` field through a serializer. The exact field value and
complete request-body representation are not reconstructed. No separate
zone-request in-flight byte has been established; the null flag in the C#
catalog means **unidentified**, not absence of native gating.

The callback wrapper's vtable is at VA `0x1C674A8` (raw `0x18660A8`). Its
slot `+4` points to VA `0xA43F80` (raw `0x643380`). That trampoline loads the
target from wrapper `+0x10` and tail-calls it with `this` set to the owner at
wrapper `+4` plus the adjustment at `+0x14`. The constructor zeroes this
adjustment, so invoking this slot forwards the Login object as `this` to the
stored response-handler address. The response pointer already on the stack
is preserved across the tail call.

The login-response handler has a bounded retry branch: after clearing the
response-in-progress byte at `+0x9E1`, it retries only when the request's
option byte at `+0x9E2` is nonzero and the response contains nonempty text.
It decrements the counter at `+0x9E4`, initialized to 3 by the constructor.
If the decremented value remains positive, it waits 500 ms and calls
`Login_StartAutoLogin` again; otherwise it routes the response text through
the shared error/display path. The interactive request path at `0x6411A0`
passes option 0, while the auto-login path passes option 1. This describes the
observed retry gate, not the meaning or completeness of the server error
classification.

The shared submission routine at raw `0xBC43C0` validates a request, extracts
its route text, looks up or creates a per-route manager entry, and links the
request to that entry. Its creation helper copies the manager pointer, a
shared-text field from request `+0x10`, and a DWORD from request `+0x14` into
a `0x38`-byte tracker. This submission path does not inspect callback pair
`+0x50/+0x54`; delivery happens in the worker pump, not inline during
submission.

## Shared request worker and callback delivery

The manager pump `0xBC4550` visits its per-route entries and calls route
pump `0xBC3F80`. That pump visits workers and calls `0xBC2B30`. The worker
is WinHTTP-backed: the PE import descriptors identify IAT VA `0x1AC4CFC`
as `WINHTTP.dll!WinHttpOpenRequest`, called at raw `0xBC2DB7`, and
`0x1AC4D24` as `WINMM.dll!timeGetTime`.

At `0xBC2C81`, the worker reads request `+8`, subtracts 2, and uses an
unsigned bound check against 3. Its four-case table VA `0xFC44C0`
(raw `.text` `0xBC38C0`) selects UTF-16 verb literals POST, PUT, DELETE,
HEAD for types 2, 3, 4, 5. All other values take GET, including type 1.
The chosen literal is pushed as `WinHttpOpenRequest`'s verb argument.
`ClientApiRequestNativeLayout.ResolveHttpMethod` reconstructs this mapping,
including the native default, without sending any request.

The bounded completion path is directly visible in instructions:

1. Worker `+0x1C` must hold a completion pointer. At `0xBC32B5` its
   request-handle DWORD at `+0` must be zero; otherwise the worker returns without delivery.
   The completed pointer is detached from worker `+0x1C`.
2. Completion `+0x18` holds a `0x20`-byte response object; response `+4`
   holds the associated request. The response reset at `0xBC11D0` clears
   `+4`, initializes result kind `+8` to 1 and status word `+0x0C` to zero,
   clears its metadata tree at `+0x10/+0x14`, and resets text fields
   `+0x18/+0x1C`. Their complete semantic meanings remain unresolved.
3. After status handling, raw `0xBC37DF` adds `0x50` to the request pointer.
   `0xBC37E2` calls `0xBC4A80` to acquire a strong callback pair from the
   stored weak pair. Helper `0x327730` refuses a zero strong count at
   control block `+4` and uses compare-exchange to increment a live count.
   The request pair's control block retention at `+8` is therefore weak
   ownership, not a strong guarantee that the callback still exists.
4. If the acquired callback object is non-null, raw `0xBC37FC` invokes
   its virtual slot `+4`, pushing the response pointer as the argument.
   For Login's three wrappers, that slot is the already-verified
   `0x643380` owner/target trampoline. If acquisition fails, invocation is
   skipped; result/request cleanup still follows.
5. After invocation, the response's request is destroyed, response `+4`
   is zeroed, the response is returned to a worker-owned collection,
   the completion allocation is freed, and the acquired strong callback
   reference is released.

This establishes manager-to-wrapper-to-handler delivery for the attached
callbacks, with a live-owner gate. Completion signaling is now established
below; all retry/error branches, thread affinity, and full wire representations
remain incomplete.

### WinHTTP completion signaling

The completion record is `0x1C` bytes. Its `+0` is a **live WinHTTP request
handle**, not a boolean state; `+0x14` is an internal status-dispatch result,
and `+0x18` is the response pointer. The worker allocates it at `0xBC2D10`,
associates the response at `0xBC2D47`, stores the `WinHttpOpenRequest`
handle at `0xBC2DEC`, and stores the completion in worker `+0x1C` at
`0xBC3280`.

Raw `0xBC31E9` calls imported `WinHttpSetStatusCallback` (IAT VA
`0x1AC4D0C`), passing callback VA `0xFC32E0`, notification mask
`0xFFFFFFFF`, and reserved zero. The actual OS callback is raw
`0xBC26E0`, a five-argument stdcall ending in `ret 0x14`; `0xBC1DA0`
is its internal status dispatcher, **not** the registered callback.
The completion pointer itself is pushed at `0xBC31F8` as the final
`dwContext` argument to `WinHttpSendRequest` (`0xBC3213`).

The callback loads that context and compares context `+0` to the incoming
handle. Status `0x800` (`WINHTTP_CALLBACK_STATUS_HANDLE_CLOSING`) sets
response result kind `+8` to 3 only when it was zero, then unconditionally
clears context `+0` at `0xBC2728`. This is the asynchronous zero writer
that makes the completion eligible for the previously traced worker gate.
When context byte `+0x11` is zero, the response/context remain for the worker;
when it is nonzero, the callback destroys the response and frees the context.
The flag transfers cleanup ownership during worker teardown as described below.

Other notifications are passed to dispatcher `0xBC1DA0` only while
context `+0x14` is zero. A zero dispatcher return leaves the handle open.
A nonzero return is retained at `+0x14`, assigns result kind 4 only if no
result was already assigned, and calls `WinHttpCloseHandle` at `0xBC2785`.
For `READ_COMPLETE` (`0x80000`) with zero data length, the dispatcher
assigns result kind 1 if still zero and returns 1. A `REQUEST_ERROR`
(`0x200000`) validates an eight-byte status-info record and passes its
second DWORD through the error formatter returning a negative HRESULT.
Both terminal paths therefore reach handle close, then HANDLE_CLOSING,
then worker delivery. The numeric result kinds are not labeled as a full
success/error enum without tracing every producer.

A separate synchronous failure path closes an existing request handle and
explicitly clears context `+0` at `0xBC322E`; an open failure leaves the
initial zero untouched. Do not attribute every zero handle to an OS callback.

### Worker teardown and detached cleanup

Worker teardown `0xBC2890` loads its active completion at `+0x1C` and,
when nonnull, sets completion byte `+0x11` to 1 at `0xBC28CB`. Route
destructor `0xBC3B80` calls this teardown for each worker before freeing it.
The flag therefore hands active response/context destruction to the
HANDLE_CLOSING callback rather than a surviving worker delivery owner.

The route pump's idle-removal path also tears down workers, but it first
checks worker `+0x1C` and bypasses pruning when a completion is active.
Thus that observed pruning path does not detach an active context.

Teardown closes the separate handle at worker `+0x20` via
`WinHttpCloseHandle` at `0xBC28D7`, clears it, drains queued requests, and
destroys pooled responses. It does **not** visibly close the request handle
at completion `+0`. The native connection/session relationship of the
separate handle and OS child-handle closure behavior remain unproven here.
This establishes deferred memory cleanup ownership, not a complete
cancellation guarantee or proof that every detached request will close.

The optional binary-backed Startup checks compare the saved findings against
the executable's actual instructions, code-section jump tables, and translated
UTF-16 verb literals:

```powershell
dotnet run --project src\Startup\Checks\Startup.Checks.csproj -- --binary "D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
```

The executable is opened read-only and is not included in the reconstruction.
These checks target this analyzed build, not arbitrary client versions.

## Character refresh and outgoing message boundary

The character-refresh handler at `0x63C5F0` walks the record range described
by begin/end pointers at `this+0x9F4/+0x9F8`, using `0xD0`-byte records. It
compares the selected text against record `+8`; on a match it copies the first
two DWORDs to `this+0xA40/+0xA44`. This is a selected 64-bit pair; calling it
a character GUID is plausible from context but not proven by the copy alone.
The handler then makes an indirect call through an object vtable at `+0xAC`,
passing message ID `0x89` and a local message buffer, before callback cleanup.
At raw `0x63CEF2`, the receiver is loaded from global VA `0x1E002B8`
(translated .data raw `0x19FE6B8`, initially zero in the file). The call
pushes zero, the local buffer address, and `0x89`, then dispatches through
receiver vtable `+0xAC`. Other callers submit different message IDs through
the same global/slot. This identifies a shared runtime message endpoint,
not its concrete class. The call is an asynchronous boundary; it does not
directly enqueue an app mode in the traced body.

## Zone-list decoding and selection

The handler `0x63D270` returns immediately when signed Login `+0xA48` is
less than 3. Otherwise it parses response text `+0x1C`, selects JSON member
`zone_list`, and decodes a vector of 24-byte records via `0x646500` and
record decoder `0x635BE0`. Correctly translated literals establish:

| Record offset | JSON field | Native value |
| ---: | --- | --- |
| `+0` | `zone_id` | Low 32 bits of `strtoull` conversion |
| `+4` | `zone_name` | Shared text |
| `+8` | `protocol_version` | Low 16 bits of `strtoull` conversion |
| `+0x0C` | `build` | Shared text |
| `+0x10` | `matrix_url` | Shared text |
| `+0x14` | `players` | Low 16 bits of `strtoull` conversion |

The numeric decoder passes base zero and maps tokens starting with `-` to
zero. This records native behavior, not a recommended JSON validation policy;
the C# selector accepts already-decoded values and does not reproduce parsing.

Only records with `protocol_version == 0x4C5F` and `players != 0` survive.
Rejected records are replaced by the last record before shrinking the vector,
so surviving order is **not** a stable filter. This order matters because
selection uses `rand() % candidateCount`. The count's JSON name is proven,
but the filter does not establish whether it counts current players, capacity,
or something else.

Login text member `+0xA20` serves as the selection filter before the handler
and is overwritten with the selected record's **matrix_url** (`+0x10`),
not the zone name or zone ID. If `_stricmp(filter, "random") == 0`, selection
uses all surviving records; otherwise it randomly selects among records
whose zone ID passes `0x63D160`. That helper performs a zone-ID lookup,
checks a table-derived name, a localized lookup, then a category-derived
text fallback. Exact table types and category meanings remain unresolved.

The string matcher searches case-insensitively through native helper
`0x12B9D60`, using CRT `tolower`, then requires the **first** occurrence to
start at the filter's beginning or after `;`, and end at string end or
before `;`. It does not retry a later occurrence if the first occurrence
fails these boundaries. A generic `Split(';').Contains(...)` reconstruction
would therefore change native behavior.

If no candidate matches, the handler logs the failure, clears Login
`+0xA20`, and still increments `+0xA48`. Selection also increments that
state. `LoginZoneListSelection.cs` reconstructs signed-state deferral,
swap-removal, candidate sampling, destination assignment, and explicit
failure outcomes. It accepts a caller-supplied zone matcher and random sample
because SDB/localization lookup, CRT locale, and global PRNG state are not
reconstructed. Its matcher should be a pure decision; it does not reproduce
the native helper's two-pass invocation count or reference-counted ownership.

## Selected-character connection preparation

Auto-login state 5 in `0x63E390` targets raw `0x63E890`. It validates the
selected pair at Login `+0xA40/+0xA44`, searches the `0xD0`-stride character
records for a matching first two DWORDs, then calls `0x63EB50` on the record
and `0x63EE30` on its property map at `record+0xC8`. This is a concrete
post-selection path, not proof of a completed World-mode transition.

The preparation routine `0x63EE30` reads property keys `name`, `is_dev`,
`race`, and `gender`. An empty name logs `SetupCharacter : empty name` and
returns false. The observed `is_dev` conversion tests only the first
character for `1`, `t`, or `T`. Race/gender go through separate converters
and logging/default paths; their enum meanings remain untraced. The method
resets the existing Login `+0x1C` connection object or allocates a
`0xBB0`-byte object through constructor `0x48ED50`, then copies a packed
configuration containing context, shared name, race/gender codes, and the
development byte into it via `0x490390`.

When preparation succeeds, the state-5 branch calls `0x63F230`. That routine
uses `Development/MatrixOverride` and the selected `matrix_url` at Login
`+0xA20` as observed override sources for the connection destination.
It validates nonempty destinations with shared host/port helpers and refuses
when connection-object unsigned state DWORD `+0x0C` is at least 2, logging
`Matrix is already connected / connecting`. On the permitted path it calls
`0x494CA0` on the Login-owned connection object with the retained destination
text and returns success.

Although `0x494CA0` has a preexisting message-24-oriented name in the IDB,
this call must not be described as an immediate message-24 send:
the preceding state gate allows only state <2, and on that branch
`0x494CA0` resets connection state, sets it to 2, retains the supplied text,
and calls `0x494E30`. Its message-24 descriptor branch is instead for states
6/7. The oracle-ticket request and its callback/transport-open boundary are now
documented in [`MatrixConnection.md`](MatrixConnection.md). The app-mode
transition remains unresolved.

## Pending app-mode queue

The application queue helper at raw `0x30DEB0` (`AppMode_EnqueuePendingModeRequest`)
copies a two-DWORD request: mode ID at pair offset `+0` and payload at `+4`.
Its queue storage is at application object `+0xB84`, with queue head/index at
`+0xB90` and count at `+0xB94`. The dispatcher at `0x30D850` confirms this
mapping. A special branch handles mode 1 when an object flag is set instead
of following the ordinary pending-queue insertion path:

| Mode ID | Constructed mode |
|---:|---|
| `1` | `tfLogin` (`0x6365B0`) |
| `2` | `tfWorld` (`0x64AE40`) |
| `3` | `tfLookingGlass` (`0x65A190`) |

The helper at raw `0x30D590` (`AppMode_QueueLookingGlassMode`) enqueues `{3,0}`
and copies the caller's `0x34`-byte request-context region to application
`+0x9F0`. The `tfLookingGlass` constructor copies that region to mode `+0x18`,
preserving shared-text references at context `+0x14` and `+0x1C`; the other
context fields are opaque. Function `0x6385C0` has a conditional path that queues mode 3
through that helper or queues `{2,0}` directly; a separate branch calls
`Login_StartAutoLogin`. No static callers of `0x6385C0` were found in the
analyzed IDB.

The mode-3 helper itself has six static callers, including replay-reader
request paths at `0x65D400`, `0x6AE3E0`, and `0x6AF120`, plus the text-and-pair
request utility at `0x306050`. Some callers invoke the reader setup directly
when LookingGlass is already active and queue mode 3 otherwise. This confirms
that the disabled `GoLookingGlass` UI binding is not the only native route
that can request LookingGlass; it does not identify all external entry
conditions or establish a Login-response-to-queue transition.

Neither `Login_HandleLoginResponse` nor `Login_HandleCharacterRefreshResponse`
has a direct call edge to the queue helper or dispatcher. Their response
callback registration/dispatch is known, but the branch that actually completes
character selection remains unresolved, so the observed mode-2/mode-3 queue paths must
not be presented as proven consequences of successful login.

Auto-login settings are read from `autologin-*` command-line entries, with
`AutoLogin/*` configuration fallbacks. Observed setting names are `MaxTimes`,
`Account`, `SavedPassword`, `CharacterName`, `DevMatrixAddress`,
`SpectatorMode`, `SpectatorDirectorControl`, `SpectatorTargetGuid`, and
`RandomReplay`. A queued request builds `%s/api/v2/accounts/login` using
`clientapi_host`; it reports an already-pending request or queue failure.
Steam, Youxi, and 369 ticket branches are present, but their wire formats are
not reconstructed.

Shutdown unregisters input, releases the owner/reference objects, destroys
the zone object, restores two cvars through a virtual setter, and performs
application cleanup calls. The C# snapshot in
`src\Startup\LoginModeNativeLayout.cs` records addresses, object offsets,
action names, response-handler keys, and auto-login setting names; it does not
send login requests or parse network responses.

## Unknowns

- The behavior of the other 16 `tfLogin` vtable slots.
- All worker retry/error branches, thread affinity, guarantees that detached
  request handles close on teardown, and the ticket protocols.
- The background-zone table layout and the `Development/InstallDir` key
  semantics.
- The exact names of cvars at VA `0x1E742C0` and `0x1E7E9D0`.
- Whether raw `0x159D3C8` is `_purecall` and raw `0x159DC30` is `_allmul`.
- The full transition from Login to World mode; the callback-to-queue connection
  and the meaning of the optional mode-3 text are not established.
