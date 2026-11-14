# Login-owned matrix connection (partial)

This trace continues auto-login state 5 after selected-character preparation.
Addresses are raw file offsets unless marked VA. The `0xBB0`-byte
Login-owned object at Login `+0x1C` is called a matrix connection object by
observed logs and operations; its original class name is not established.
It is distinct from the global `uiManager` event endpoint at VA `0x1E002B8`
(raw `.data` offset `0x19FE6B8`). The singleton wrapper at VA `0x1E002A8`
has RTTI `.?AV?$slSingleton@VuiManager@@@@`; its instance pointer is `+0x10`,
and its on-disk value is zero. The raw singleton seed at `0x3EE8C0` links the
wrapper into list head `0x1E4B0A4`, installs wrapper vtable VA `0x1C62FF0`,
and zeroes that pointer.

`ClientApplicationBootstrap` (`0x2EAE70`) calls singleton walker `0x12C3A80`,
which walks the `+8` links and invokes vtable slot `+4` on uninitialized
entries. The `slSingleton<uiManager>` slot `+4` method `0x3E44F0` allocates
`0xA10` bytes, constructs `uiManager` with `0xBF7A80`, stores the instance at
wrapper `+0x10` (VA `0x1E002B8`), links the wrapper into `0x1E4B0A8`, and
sets its initialized flag. `uiManager` RTTI is `.?AVuiManager@@`, and its
vtable VA is `0x1C73EBC`.

The separate Login-mode routine at raw `0x63C1E2` (after the `ret` at
`0x63C0A7` in `Login_HandleLoginResponse`) loads the resulting pointer, pushes
message ID `0x145`, and invokes `uiManager` slot `+0xAC` at raw `0x63C1F5`.
That route enters `0xBF9D80`, looks up an event-ID bucket in the
registry at `uiManager+0x6F0`, and walks listeners through `0xBF9C40`;
listener matching/callback work continues through `0xC38400`. Dispatch is
gated by `uiManager+0x672`, which the constructor initializes to zero.
Who enables that flag is not established. This identifies the endpoint and
routing layer, but the subscriber for message `0x145` remains unresolved. A
separate installed-UI join path now traces through the native Matrix
connection entry to its oracle-ticket callsite. `JoinWorld` can pass a nil
`dev_matrix_url` in the PvE matchmaking branch; native code converts it to
empty shared text, then prefers a nonempty Login-selected URL or
`Development/MatrixOverride`. With no fallback, an empty destination is
permitted, and a reused connection can retain its prior preferred matrix text
at `+0xBA0`. The new-object constructor initializes `+0xBA0` to zero at raw
`0x48F0F9`. This does not prove a completed handshake or a `tfWorld` request. See
[`LoginMode.md`](LoginMode.md#ui-preload-route-into-the-matrix-connection).

Subscription names map to numeric IDs through `uiManager+0x6E8`.
`0xBF9AB0` returns an existing ID or assigns the current DWORD count of the
name array at `+0x71C..+0x720`, then appends the new string. Component XML
event parser `0xC35740` handles the `<Events>` section of a root
`<Component>` document. For each `<Event>` child, it reads the `name`
attribute and immediately calls `0xBF9AB0`; it then reads `bind`, builds the
listener record, and registers it through `0xBF9B40` (raw callsite
`0xC35AD8`). The name-map call is at `0xC358AF`, before the `bind` lookup at
`0xC358FE`. The `Component.BindEvent(event, function)` API is a separate path:
its wrapper `0xC32460` calls named-subscription helper `0xC35BB0` at
`0xC32643`; that helper also calls `0xBF9AB0` and `0xBF9B40`. Both paths add
first-seen names to the same dynamic map.

The `uiManager` constructor initializes the name-array begin/end pointers at
`+0x71C/+0x720` to zero (raw stores `0xBF7FF6` and `0xBF8000`), so the first
new name receives ID 0. ID `0x145` is therefore the 326th distinct
first-time insertion; repeated names reuse their existing ID.

The internal allocator mechanics were also verified directly. On a name-map
hit, `0xBF9AB0` returns the event ID at entry `+0x14` (`mov eax,[eax+0x14]`
at raw `0xBF9ADA`). On a miss, it computes
`(name_array_end - name_array_begin) / 4` from `+0x71C/+0x720` at
`0xBF9AE6`, passes that current count into insertion helper `0xC079A0`
(`0xBF9AFE`), and appends the name pointer to the 4-byte-entry array through
`0xC07D10` (`0xBF9B2B`). This is first-seen insertion order, not a compiled
numeric-name table.

The ID data flow is distinct from the shared-text cleanup that follows insertion.
`0xBF9AB0` prepares the computed count in a stack local and passes its address to
`0xC079A0` (`0xBF9AFE`). The helper forwards that pointer at `0xC079F4` to pair
construction `0xC14660` (`0xC079FE`), which dereferences it and writes the ID
to the pair's `+4` field at `0xC14694`. Node construction `0xC16CB0`
(`0xC07A11`) copies the pair's second dword into node `+0x14` at
`0xC16D2D` (the node base is adjusted by `+0x10` at `0xC16CEC`), then the
map insertion call reaches `0xC16D60` at `0xC07A23`. Only afterward does the
call at `0xC07A34` reach `0xC08B40` to release the temporary shared-text
reference; that cleanup is not an ID store.

Component XML parser `0xC35740` reads an `<Event>`'s `name` attribute and
calls `0xBF9AB0` at `0xC358AF` before reading `bind` at `0xC358FE`; it
registers the listener through `0xBF9B40` at `0xC35AD8`. The separate
`Component.BindEvent(event, function)` route calls named-subscription helper
`0xC35BB0` from wrapper `0xC32460` at `0xC32643`; that helper also uses
`0xBF9AB0` and `0xBF9B40` (call sites `0xC35BE4` and `0xC35CF0`). Therefore
both XML registrations and runtime script calls contribute to the same ID
allocation order.

The native binary and tracked repository do not contain a complete static
first-insertion order. UI-set roots are traversed in declared order, but the
folder loader enumerates files using `_wfindfirst64i32`/`_wfindnext64i32`
without sorting, and script `Component.BindEvent` calls add names at runtime.
No static table assigning these application codes to names was found; codes
`228`, `229`, and `357` remain unbound rather than inferred from dialog class
names or payload keys.

The UI-set loader's folder path uses `0x30AB90`, which calls directory
enumerator `0x12B6A00` with search pattern `%s\*`. The enumerator uses CRT
`_wfindfirst64i32`, `_wfindnext64i32`, and `_findclose`; it splits entries by
the directory attribute, appends each class in returned order, and the caller
processes the directory list before its file list. No sort is observed in
this path. Therefore the folder roots listed in the UI-set XML do not alone
establish the component-file event-registration order.

The local installed UI sources provide a candidate name for the Login
notification. `UISets\LoginUI.xml` sets `glue_mode=true` and lists folder
roots `LoginUI`, `MainUI/Panels/Options`, `MainUI/Backend/IME`, and
`MainUI/Panels/Generic` in that order. The component
`components\LoginUI\AccountLogin\AccountLogin.xml:17` binds
`ON_LOGIN_RESPONSE` to `OnLoginResponse`; the Lua handler is defined at
`AccountLogin.lua:876`. This makes `0x145 -> ON_LOGIN_RESPONSE` plausible, but not proven: the native
callsite is in a separate Login-mode routine, not in the response callback
`0x63B7C0`. `0x145` is array index 325, the 326th distinct first-time
insertion; repeated event names reuse their existing ID.
Because both XML `<Events>` parsing and `Component.BindEvent` calls insert
names, the complete first-insertion order across folders, components, and
script activity is not reconstructed. The actual subscriber remains
unresolved; the independent UI-to-Matrix route does not establish that
message `0x145` triggers it.

These read-only text samples were hashed before inspection. They are under
`D:\SteamLibrary\steamapps\common\Firefall\system\gui\` and are not repository
files:

| Relative sample path | Size | SHA-256 | Observation |
|---|---:|---|---|
| `UISets\LoginUI.xml` | 521 | `F7FE3C61489C51E0262E288EE5DBF7996CE90029A2D6CBC179AEEBC898A71D66` | `glue_mode=true`; lists `LoginUI` then three `MainUI` folder roots. |
| `UISets\MainUI.xml` | 409 | `1441F9FE7633F857FFA1B49B2E1297A1C19BFE22C0FBC60DAAE3601F97E75EDD` | Root UI-set comparison; no event IDs. |
| `UISets\LookingGlass.xml` | 386 | `9A0BE16C2A2175FDD842420DAC9DEE982C31965E9781C6C303D774435FE34B25` | Root UI-set comparison; `glue_mode=false`. |
| `UISets\Broadcast.xml` | 991 | `7F60A9E8370E1871239CF7AB82C4485A8F7E19A3BECA2AD05B63B2B02FB88E63` | Root UI-set comparison; `glue_mode=false`. |
| `components\LoginUI\AccountLogin\AccountLogin.xml` | 24,728 | `67CBD909E54E1354E291728F66EBAB9E7F1410DB2600CA75E50EFF77CB5FA05A` | Declares `ON_LOGIN_RESPONSE` at line17. |
| `components\LoginUI\AccountLogin\AccountLogin.lua` | 69,488 | `9EA014EA94C803B82DC696CF0790E12DD414154993B11F5997DD3901B244D462` | Defines `OnLoginResponse` at line876. |
| `components\LoginUI\AccountLogin\Legal.xml` | 3,122 | `14199356EEDE06A28594CA4ED573A7BAECFF2AF9E1B5698C0BC1099838881865` | Declares `ON_COMPONENT_LOAD`. |
| `components\LoginUI\CharacterCreation\CharacterCreation.xml` | 36,506 | `7814E0E16ADF21B6EF196A009C8BE0518DE8045636B48D5639E6750B505EA64D` | Declares component-load, stream-progress, mouseover, and `MY_CONNECT` events. |
| `components\LoginUI\Credits\Credits.xml` | 7,065 | `551F35946F670DE3E52F85CA3AE44FE2269180DA17C993149E3903D3E8C75891` | Declares `ON_COMPONENT_LOAD` and `MY_SHOW_CREDITS`. |
| `components\LoginUI\LoginBackground\LoginBackground.xml` | 9,788 | `B479DF7498D9540C62A0283C444928BF05F9D995FC44F01110DB08450D1A285C` | Declares `ON_COMPONENT_LOAD` and `ON_LIST_CHARACTERS_RESPONSE`. |
| `components\LoginUI\LoginPreloadScreen\LoginFullscreenPanel.xml` | 2,334 | `BAA660AEEEC18A28AD4AE1F7FA97C1270B3722DEC5EC9A544C89F40D2E956EA9` | Declares `ON_COMPONENT_LOAD` and `MY_FULLPANEL_EVENT`. |
| `components\LoginUI\ZoneSelection\ZoneSelection.xml` | 33,076 | `F49545D3F3B9757D7E360CB607341386AD794BFCD15618ED8758844CBD380AD4` | Binds `MY_CONNECT` to `OnConnect` at line25. |
| `components\LoginUI\ZoneSelection\ZoneSelection.lua` | 75,192 | `F136FC2FF26EFC892D9AB24E5464F98D49CB3890830E391971C26C5C2148BD97` | `JoinWorld` emits `MY_PRELOAD_EVENT` with `zone.matrix_url` and character fields at lines2147-2225; the URL is nil in the PvE matchmaking branch. |
| `components\LoginUI\LoginPreloadScreen\LoadingCinematic\LoadingCinematic.xml` | 2,771 | `84C8FE76044C58157FAAB9C061EB43F46831CA29AFAFF6D62382926CEBFB5FC2` | Declares `ON_COMPONENT_LOAD`, `ON_FIRSTLOGIN`, and `MY_PLAYINTRO`. |
| `components\LoginUI\LoginPreloadScreen\LoginPreloadScreen.xml` | 3,147 | `A59E0A3F7D93FD8866AFF2FB3BF1E1AAF8A2D24BFC7439076F2B53F9FDEB3E31` | Binds `MY_PRELOAD_EVENT` to `OnPreload` at line19. |
| `components\LoginUI\LoginPreloadScreen\LoginPreloadScreen.lua` | 4,614 | `2D1A1801F331101D035F9A0BAE3A12EB1EF75ACDD4DE91B7B433AD77EF0D45F4` | After the duration/delay timer, the credentials gate calls `System.SetupCharacter` then `System.StartInstanceConnection` at lines89-98. |
| `components\LoginUI\LoginPreloadScreen\WaitingScreen\WaitingScreen.xml` | 1,005 | `E236C85F37631D4BB36BBB753103DA92FEBFD0E635D6298607D1DE664C63CB0D` | Declares `MY_WAITNG_SCREEN`. |
| `components\LoginUI\Migration\Migration.xml` | 7,198 | `564DD33CB6E680F5C85B7155540FA2A62A27ECDA5CB9F4F878004D1DD35D71E0` | Declares `ON_COMPONENT_LOAD` and `ON_LIST_CHARACTERS_RESPONSE`. |
| `components\LoginUI\Web\Webframe\Webframe.xml` | 1,301 | `620525806B925ECE6E896F4DFDDE4228CB12666767C10C13B8BB813732A72F25` | Declares operator, web-UI, and match-list events. |

A static discriminator must reconstruct the first-time event-name insertions
across loaded component XML `<Events>` sections and calls to
`Component.BindEvent`, including native folder enumeration, then evaluate
the ID assigned to `ON_LOGIN_RESPONSE`. Matching array index `0x145` would
confirm the hypothesis; the order across folders, components, and script API
calls is not established.


## Topic files

- [Transport, oracle ticket, UDP setup, and queueing](MatrixConnectionTransport.md#oracle-ticket-request)
- [Handshake, packet admission, IOCP, and shutdown](MatrixConnectionHandshake.md#outer-packet-admission)
- [Server dispatch, selector receivers, and parser results](MatrixServerDispatch.md#decoded-server-message-dispatch-and-msg-35)
- [Message bodies, zone transitions, and challenge routes](MatrixServerMessages.md#message35-application-body)
- [Synchronization, collection messages, and metadata transforms](MatrixCollectionAndSync.md#forceunqueue-remaining-buffer-and-collection-messages56-61)
- [Profiler aggregation and final server routes](MatrixProfiler.md#profiler-frame61-missing-name-collection-and-refresh-gates)
- [Chat payloads and file logging](MatrixChat.md#initial-bodies3436-and-catalog-wide-wire-coverage)
- [World, renderer, and engine lifecycle](MatrixWorldLifecycle.md#tflogin-update-0x638b89-guard-and-20-queue-consumer)
