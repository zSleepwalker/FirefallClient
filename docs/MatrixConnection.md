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

## Oracle-ticket request

Connection entry `0x494CA0` sets connection state `+0x0C` to 2 on the
Login-reachable path and calls `0x494E30` at raw callsite `0x494D0B`. The
ticket-queue routine `0x494E30` requires state 2 and returns without another
request when byte `+0x1C` is already set.
It checks a locked account/service context before constructing a request;
the complete meaning of that service's flags is untraced.

For an empty input, `0x494CA0` calls empty-text predicate `0x2FCD00` at
`0x494CF4` and skips replacing the preferred destination at `+0xBA0`.
`0x494E30` calls the same predicate at `0x495257` and appends JSON key
`matrix` only when that stored destination is nonempty. The `SetupCharacter`
reuse path resets the existing connection through `0x490090`, which destroys
the socket and resets connection state but has no observed direct store to
`+0xBA0`; its following metadata setter `0x490390` writes other fields. A nil
`StartInstanceConnection` argument therefore becomes empty text, but can leave
and send an older preferred URL on a reused object. The initial preference on
a freshly allocated connection is zeroed by constructor `0x48ED50` at raw
`0x48F0F9`.

The request is `0x78` bytes, initialized through `0x3233B0`, and formats
`%s/api/v1/oracle/ticket` from `clientapi_host`. It sets type 2 (POST),
attaches the connection's weak callback pair at `+0xB98/+0xB9C` to request
`+0x50/+0x54` through `0x498680`, sets content type `application/json`,
and serializes `ver`, `build`, and optional `matrix` from the retained
preferred destination at connection `+0xBA0`. Other platform-dependent
body fields and exact escaping/serialization are not reconstructed.
Headers include `Connection: Keep-Alive` and `Accept-Language`; further
header values remain opaque.

It prepares through shared `0x636550` and submits through `0x49AEF0`
to the same manager `0xBC43C0` used by Login requests. Success sets
connection byte `+0x1C` to 1. Parse/scheme failures store an error text at
`+0x10`, set state 1, and destroy the request rather than pretending success.

## A different callback-wrapper layout

The connection constructor `0x48ED50` allocates a **16-byte** callback
wrapper, not Login's 32-byte wrapper:

| Offset | Observed role |
| ---: | --- |
| `+0` | Vtable VA `0x1C641D8` (raw `0x1862DD8`) |
| `+4` | Connection owner pointer |
| `+8` | Target VA `0x896060` (raw response handler `0x495460`) |
| `+0x0C` | Zero owner adjustment |

Its vtable slot `+4` points to VA `0x8979F0` (raw `0x496DF0`), which
tail-calls target `+8` using owner `+4` plus adjustment `+0x0C`. The common
request worker's weak-to-strong callback acquisition and virtual slot `+4`
delivery therefore reach this handler too, without merging the two wrapper
layouts.

## Response and retry behavior

Handler `0x495460` clears the in-flight byte at `+0x1C` and calls parser
`0x4956C0`. That parser's accepted branch requires response result kind
`+8 == 1` and signed response status DWORD `+0x0C / 100 == 2`. It reads
response text `+0x1C` as JSON; literal-backed keys are `matrix_url`,
`ticket`, `datacenter`, `operator_override`, `session_id`, and `hostname`.
Some are optional. Complete validation and every failure branch are not
reconstructed; the raw-loaded decompiler fails on this parser.

The parsed matrix destination and ticket lead to `0x495F20`, which validates
host/port, allocates a `0xDF8` transport object with constructor `0xBD00B0`,
registers it with a service, and calls transport open `0xBD0960`. On failure
it cleans up the transport and records an error; on success it sets connection
state to 3 and invokes packet-building/sending helpers. This is a transport
opening boundary, not proof of a server handshake or World-mode transition.

When the response parser fails while connection state is still 2, the handler
updates the retry-delay DWORD at `+0x18` using
`next = min(300, current + 2 + (current + 1) / 2)`, logs
`Failed response from oracle. Will try again in %d sec`, emits message 383
through the shared runtime message endpoint, and requests another oracle
ticket. The request builder uses that positive delay with `_time64` to set
request due time `+0x48/+0x4C`; this is a scheduled retry, not an immediate
sleep. The reconstructed helper covers the observed nonnegative 0..300
range and explicitly rejects other inputs.

If parsing succeeds, the handler clears the preferred matrix text and resets
the retry-delay DWORD to zero. The full retry termination policy, ticket
handshake serialization, transport ownership, and complete mode transition
remain unresolved. `src\Startup\MatrixConnectionNativeLayout.cs` preserves
the known layout and bounded retry calculation without sending network traffic.

## Transport opening is UDP, not a TCP connection

`0xBD0960` opens a game socket. Its factory `0xBE3510` calls the named
`WS2_32.dll!WSASocketA` import at VA `0x1AC4D68` with socket type 2
(`SOCK_DGRAM`), protocol 17 (`IPPROTO_UDP`), and flags 1
(`WSA_FLAG_OVERLAPPED`). Global family mode 1 selects family 2 (`AF_INET`);
mode 2 selects family 23 (`AF_INET6`). Other modes assert. The following
`WSAIoctl` command is `0x9800000C` with a zero DWORD input; the broader
configuration/dual-stack policy is not reconstructed.

The destination must be a specific, nonzero address and have a nonzero port.
The routine retains the remote address/port at socket `+0x230..+0x248`,
creates a handle at `+0x118`, and calls `ioctlsocket` before **binding locally**.
It does not call Winsock `connect` on this path. Names such as “connecting”
in its logs refer to higher-level game-socket operations.

Local binding at `0xBD0D05` tries seven times when a preferred local port
is nonzero: up to five successive ports, then two port-zero attempts. With
no preferred port it tries port zero twice. Execution stops immediately
on success. Port increment is observed through the low 16 bits; if it wraps
to zero it remains zero, rather than moving on to port one.
`getsockname` at `0xBD0E6E` retrieves the actual OS-assigned address;
sockaddr decoder `0xBE3A00` stores its port at socket `+0x28`.
Address encoder `0xBE3B50` writes family 2/length16 or family23/length28 and
converts the port to network byte order. Complete mapped-address handling
is not reconstructed.

The game socket uses its own state DWORD `+0x108`: opening is 1,
the later failure cleanup path assigns 7. These are **not** connection
owner state `+0x0C` values. A pump object at socket `+8` receives a virtual
slot `+4` registration call. The first overlapped receive at `0xBD1211`
uses named `WS2_32.dll!WSARecvFrom` (IAT VA `0x1AC4D54`), a 1400-byte
buffer at socket `+0x278`, and a retained asynchronous-operation record.
An immediate zero result or error 997 (`WSA_IO_PENDING`) is accepted;
other errors log, close the socket, and fail the open routine.
Completion-pump routing, retransmission, and the game protocol remain
untraced at this boundary. A successful open does not establish a completed
server handshake.

The legacy `WSOCK32.dll` imports are ordinal-based: 2=`bind`,
6=`getsockname`, 12=`ioctlsocket`, 111=`WSAGetLastError`,
22=`shutdown`, and 3=`closesocket`. These are symbol translations of
the observed import ordinals, not names provided by the raw IDA database.
`MatrixTransportNativeLayout.cs` saves the offsets and pure local-port
candidate calculation; it creates no sockets.

## Initial application packet

The success tail of `0x495F20` constructs an object through `0x338250`,
emits it through `0x4980A0`, then destroys it through **`0x338120`**.
The latter is a destructor, not an encoder. The encoder is `0x349E60`,
called by serialization helper `0x498A00` at `0x498A7D`. The application
header is supplied before that encoder. Its observed buffer is:

| Order | Native object field | Encoded value |
| ---: | --- | --- |
| 1 | Base `+0x10` / emitted header | One byte `0x11` |
| 2 | `+0x28` from connection `+0x83C` | One mode byte (observed selection 0/1/0x11) |
| 3 | `+0x2C` | DWORD literal 1962 (`AA 07 00 00`); semantic identity unproven |
| 4 | `+0x30` | NUL-terminated byte string, empty on this callsite |
| 5 | `+0x38` from connection `+0x848` | Eight raw bytes, opaque |
| 6 | `+0x40` from connection `+0x968` | DWORD, opaque |
| 7 | `+0x44` from connection `+0x96C` | DWORD, opaque |
| 8 | `+0x48` from connection `+0x970` | WORD, opaque |
| 9 | `+0x4A` from connection `+0x972` | Byte, opaque |
| 10 | `+0x4C` | Zero byte |
| 11 | `+0x4D` | Byte from runtime global VA `0x1E00100` pointee `+4`, opaque |
| 12 | `+0x50` | NUL-terminated byte string returned through `0x30F660`, contents unknown |
| 13 | `+0x58` | Optional-present byte; zero on this callsite |
| 14 | `+0x70` wrapper | Raw backing blob, no length prefix |

The numeric primitives write native x86 little-endian values without byte
swaps: byte `0x3273D0`, DWORD `0x327310`, WORD `0x327210`, eight-byte pair
`0x325BE0`. String writes reserve stored length+1 and copy the terminator;
reservation helpers do **not** encode a length. The trailing wrapper's
size virtual `+0x0C` resolves through vtable VA `0x1C80CC4` to raw
`0x12CB9C0`. It determines how many backing bytes to copy but emits no
size itself. Its contents and semantic association with response fields
remain opaque.

The constructor copies 12 bytes from connection `+0x968..+0x973`;
the final byte at object `+0x4B` is not encoded. The general encoder can
write an optional-present marker of 1 followed by an eight-byte pair and
another NUL-terminated string through `0x349CE0`; that variant is not
used by the traced success callsite and is not implemented in the bounded
C# encoder.

For this success variant, total application size is **30 + generated-text
byte length + trailing-blob byte length**, excluding the generated text's
terminator from that length. Native `0x1D` reservation is a capacity hint,
not a fixed wire length. `MatrixInitialPacket.cs` encodes this bounded
variant with injected raw text/blob bytes so no encoding or token meaning
is guessed.

`0x4980A0` submits the encoded buffer to transport method `0xBD3B20`
with explicit first argument 1 and the buffer pointer. Remaining reserved
stack bytes and the flag's meaning are unresolved. The returned encoded
byte count is ignored by the connection-open success tail. No final UDP
framing, reliability behavior, ticket interpretation, or completed handshake
is inferred from this application packet alone.

## Transport submission and queued fragmentation

`0xBD3B20` asserts socket state `+0x108 != 0` and handler `+0x10C != 0`,
then skips suppressed submissions (`+0xD95`) and empty buffers. Its four-way
switch table is VA `0xFD4A3C`, translated to raw `0xBD3E3C`. The raw-loaded
decompiler cannot follow it; the actual targets are raw `0xBD3C19`,
`0xBD3C3C`, `0xBD3C3C`, and `0xBD3DC6` for channels 0..3.

Channel 0 asserts “User code may not send to this channel.” Channels 1 and
2 share the split path but choose separate queue pointers at socket
`+0xD6C` and `+0xD70`. Channel 3 uses a separate queue at `+0xD74`.
If a channel-3 message exceeds the socket's maximum segment size WORD
`+0x104`, native code logs “Message too long for unreliable channel,
forcing reliable fragmentation” and changes the channel to 2. This
establishes that channel 3 is an unreliable path and channel 2 supplies
the promoted reliable-fragment path; channel 1's complete delivery policy
is still unknown. Native code asserts channel-3 payloads over `0x56C`
(1388 bytes).

For channels 1/2, each fragment retains a reference to the **original**
application buffer at fragment `+8` and stores its payload slice offset
at fragment `+4`; splitting does not duplicate the entire buffer.
Each nonfinal fragment covers exactly `maximumSegmentSize` bytes and the
last covers the remainder, including a full final fragment for exact
multiples. Initial sequence WORD `+2` is zero.

The initial header WORD `+0` packs `(channel << 14)`, size including four
header bytes in bits0..10, and continuation bit `0x0800` for nonfinal
fragments. Bits12..13 are zero in this submission path. General header
constructor `0xBCFE30` supports those extra two bits, but their semantics
are not established. Queue insertion helper `0xBE47C0` asserts the header's
size does not exceed `0x570` (1392 bytes). This yields a bounded reconstructed
maximum segment size of 1388 bytes; zero or larger sizes are explicitly
rejected by the offline planner.

`MatrixTransportFragmentPlan.cs` returns only channel-1/channel-2 queued
fragment descriptors, not final datagrams or a complete reliability engine.
Later sequence assignment, acknowledgments, batching, outer headers,
compression/encryption, and socket send completion remain outside this
model. The Login open-success packet enters channel 1. Its initial queued
header for an unsplit empty-input 30-byte application packet is `0x4022`;
that is **not** a claim that the final wire datagram starts with those bytes.

## Queue promotion and retry preparation

The socket constructor creates two `0x5A8` objects through `0xBE3CE0`,
with channel IDs 1 and 2, and stores them at `+0xD6C/+0xD70`. Both use
the same pending-queue, outstanding-tree, sequence-window and retry logic.
It initializes maximum segment size to `0x56C` (1388) through the packed
DWORD store at raw `0xBD012C`; the 1400-byte receive buffer is a separate
limit, not the fragment queue's 1392-byte bound.

Queue method `0xBE49E0` requires active byte `+0x0C`, rejects an already
failed queue (`+0x1C`), and samples `timeGetTime` in milliseconds. While the
outstanding-tree count `+0x54` is below 512, it pops pending fragments from
queue `+0x38`, assigns fragment sequence WORD `+2` from queue WORD
`+0x28`, increments that WORD (wrapping at 16 bits), and inserts records
into tree `+0x50`. This is queue promotion, not itself a Winsock send.

It considers records only while unsigned 16-bit
`fragmentSequence - peerWindowSequence(+0x2C) < 256`. Records whose
pending-selection field is zero and elapsed time reaches retry interval
`+0x18` can be prepared again through `0xBE4DE0`. That helper increases
the interval to `clamp(current + (current >> 1), 250, 2500)` milliseconds,
increments the record's byte-sized attempt counter, and rewrites header
bits12..13 using `attempt % 3 + 1`, preserving channel, length, and
continuation. It updates statistics and marks the record selected. Exact
send/clear semantics of this selected field remain incomplete.

If a due record's attempt counter is at least 16, and socket flag `+0x100`
is clear, the queue marks failure with code1. A full 256-sequence window
persisting at least 15000 milliseconds marks code2 under the same flag
condition. The flag's formal purpose is unproven; it suppresses these two
failure transitions in the observed code. These are not oracle-ticket
retry delays, which are measured in seconds and capped at 300.

Acknowledgment handler `0xBE43B0` reads network-byte-order WORDs, advances
modular sequence frontiers, and removes acknowledged outstanding entries.
It also feeds elapsed time back into the capped retry interval. Its
peer frontiers, optional exact-sequence list, and matching outbound builder
are described below; the remaining receive-window and reliability behavior
is partial.
`GameSocketReliableQueueNativeLayout.cs`
preserves bounded retry growth and the exact modular send-window test,
without implementing a timer or reliability engine.

## Datagram assembly and byte order

Data assembler `0xBD4340` runs its main loop only in **game-socket states5
or8**, not the opening state1. Thus the initial application packet is first
queued; connection-open success does not mean that this data packet has
already gone to Winsock. The context-driven handshake progression is
described below; the complete transport state machine remains partial.

The working buffer is socket `+0x7F1`. The assembler writes a four-byte
network-order prefix from socket DWORD `+0x114` (semantic identity unknown),
then offers acknowledgment/control blocks from queues1/2, followed by
selected data fragments (queue1 before queue2, then queue3). Capacity begins
as WORD `maximumSegmentSize + 12`, default1400 bytes. Other optional control
objects can append data; the complete datagram layout is not reconstructed.

Fragment copy `0xBD4240` refuses a fragment that cannot fit. It writes the
header WORD and sequence WORD in **network byte order**, then copies the
application slice selected by the retained buffer reference and payload
offset. This differs from the application's little-endian numeric fields.
For a nonzero header marker (bits12..13), `0xBD58E0` transforms the copied
payload **only**, using a uniform XOR byte: marker1=`FF`, marker2=`AA`,
marker3=`CC`. The translated literal table is VA `0x1BB7490`, raw
`0x17B6090`; each eight-byte optimized mask repeats that same byte, so
alignment does not change the result. Marker0 skips the transform. This
is simple per-byte obfuscation, not a demonstrated cryptographic protocol.

`0xBD4780` is not a general encryption/compression pass. It can append a
time-gated extra block containing a network-order size, type byte6, and
repeated `rand()`-derived filler while adjusting a second size-bound WORD
at socket `+0x106`. Its role is consistent with probing a larger packet
size, but the full probing/feedback algorithm is not proven.

Socket send `0xBD2980` uses `sendto` (legacy ordinal20) or `send`
(ordinal19) according to byte `+0x259`, with the handle at `+0x118`.
It returns false for invalid handles, negative results (with an explicit
Winsock error log), or zero bytes; a positive short write asserts rather
than being silently accepted. It updates byte/packet statistics on success.
Connected tick helper `0xBD4A20` sets the connected-send flag after its
peer-socket connection helper succeeds; failure returns false to the tick.

`MatrixDatagramFragmentEncoder.cs` encodes only the known **fragment**
portion and its payload transform, with injected sequence and original
application buffer. It does not claim a complete datagram or omit unknown
prefix/ACK/padding fields behind a success-shaped full-wire API.

## IOCP receive dispatch

Original-file RTTI establishes `nl::GameSocket` (vtable VA `0x1C73A5C`,
type descriptor VA `0x1E2C770`), `nl::Network` (vtable `0x1C734B4`,
descriptor `0x1E2C45C`), and `nl::Network::PumpThread` (vtable
`0x1C734D4`, descriptor `0x1E2C438`). These names are not inferred from
logs alone.

Connection setup obtains the Network service from global VA `0x1DFFF70`
and registers the socket through `0xBBD730`. It appends to service
container `+0xA4` and invokes GameSocket slot `+8` (`0xBCFDF0`), which
stores Network at socket `+8`. The connection handler is separately at
socket `+0x10C`.

Network initialization `0xBBD110` creates a completion port at call
`0xBBD2E1`, stores its handle at Network `+0xDC`, and starts the embedded
PumpThread at `+0x48` through `_beginthreadex`. Network slot `+4`,
`0xBBD820`, associates the socket handle with that port through
`CreateIoCompletionPort`, using **GameSocket* as the completion key**.
It does not require a socket backpointer in the operation record.

PumpThread method `0xBBC860` calls `GetQueuedCompletionStatus` at
`0xBBC911` and drains up to100 completions per inner pass. Timeout begins
at zero, uses500ms after an empty dequeue, then returns to zero while
draining; the outer loop ends when thread byte `+4` is set. Successful
completions invoke key-object slot `+0x18` for nonnull OVERLAPPED and
slot `+0x1C` for null OVERLAPPED (close notification). On failed dequeue
with a nonnull operation, error995 skips callback; other errors use slot
`+0x20`. Detailed shutdown/lifetime guarantees remain untraced. A second
standalone drain at `0xBBDA20` has equivalent dispatch, but its caller
is unresolved.

GameSocket's slots map to raw receive `0xBD2E30`, close `0xBD2D10`, and
error `0xBD2D70`. Receive handling locks socket `+0xD98` and skips
suppressed work (`+0xD95`). It obtains byte count through
`WSAGetOverlappedResult` at `0xBD2ED1`, **not** by a byte-count callback
argument from the pump. The retained `0xA0`-byte operation record at
socket `+0xDF4` contains:

| Offset | Observation |
| ---: | --- |
| `+0..+0x13` | Zero-initialized 32-bit OVERLAPPED |
| `+0x14` | Unresolved |
| `+0x18` | Sender sockaddr storage |
| `+0x98` | Sender sockaddr length DWORD, reset to128 for each post |
| `+0x9C` | Unresolved |

On normal success, it decodes sender sockaddr through `0xBE3A00` and
calls packet entry `0xBD3030` at `0xBD2F25`, passing the count and sender
address while using socket buffer `+0x278`. After that returns, it reposts
`WSARecvFrom` at `0xBD2FE2` with the same retained OVERLAPPED, sender
storage, and 1400-byte buffer; the completion-routine argument is null.
Immediate success or error997 is accepted; other repost failures invoke
the error slot. Failed `WSAGetOverlappedResult` exits without repost:
error996 suppresses the error callback, others invoke it. Consequently
reposting is established for normal success, not every failure/suppression
branch.

`NetworkIoCompletionNativeLayout.cs` preserves the native classes, routing
slots, and operation layout. This establishes a concrete OS-to-socket
receive chain, separate from WinHTTP's callback mechanism. Queued delivery
to the connection handler is described below; application packet dispatch
and the complete World transition remain partial.

## Shutdown and handle ownership

The original PE's `nl::Network` vtable (VA `0x1C734B4`) has slot `+8` at
VA `0xFBE540`, raw `0xBBD940`. That method receives a pointer to a socket
handle, calls the optional callback at Network `+0xD8`, then calls
`WSOCK32.dll` ordinals 22 and 3 with the handle (shutdown mode `2`, then
close) and writes `-1` through the handle pointer. If Network `+0xDC` is
valid, it posts a zero-byte/null-overlapped completion using the first
argument as completion key. This is the observed Network socket-close and
pump-wakeup path.

GameSocket vtable slot `+0x1C` is raw `0xBD2D10`; it marks byte `+0xD94`
and drains the queue, but does not close the socket. The GameSocket deleting
destructor (`0xBD03D0` -> `0xBD0410`) checks socket handle `+0x118` and, when
valid, calls `0xBE36B0` with its address. `0xBE36B0` performs the same
shutdown-mode-2/close sequence and stores `-1`. Thus the GameSocket owns the
socket field; Network's close path also closes a socket supplied by pointer.
The Network destructor vtable slot `0` (`0xBBCD00` -> `0xBBCD30`) tears down
its embedded pump thread and owned buffers, but the eventual close of its
IOCP handle at `+0xDC` is not established by this trace.

The requested helpers `0xBC3970` and `0xBC3990` have no direct `E8`/`E9`
caller or `.rdata` VA/vtable pointer in the original-file scans. Their code calls the imported
`WinHttpCloseHandle` at VA `0x1AC4CF8`, then clears `this+0x20` and `this`,
respectively; no evidence ties those WinHTTP handles to Network or GameSocket.
The standalone drain `0xBBDA20` likewise has no direct `E8`/`E9` caller or
actual VA pointer/vtable slot found. Its raw value occurs at unaligned `.rdata`
offset `0x183CCFD` among `+0x20`-stride values, and its RVA occurs at
`0x183CE7D` in another such sequence; neither sequence is established as a
vtable, so its owner remains unresolved.

## Outer packet admission

Packet entry `0xBD3030` accepts exactly5..1400 bytes, as an unsigned
`count - 5 <= 0x573` check. It reads the first four bytes in network order
and requires equality with the opaque socket DWORD `+0x114`. The sender
must be specific/nonzero under native address helper `0xBCCA60` and have
a nonzero port. Failed outer checks log and increment socket `+0xEC`.

An optional sender-binding flag at socket `+0x25A` causes additional
address-copy/connection-helper/event-queue side effects, then is cleared;
these are not implemented in the pure classifier. The remaining body is
`count-4` bytes starting at receive buffer `+0x27C`. A zero expected context
routes to parser `0xBD39D0`; a nonzero value routes to `0xBD3140`.
These are observed parser choices, not proof of authenticated or completed
connection state. Subsequent format checks may still reject an admitted
packet.

`GameSocketPacketAdmission.cs` returns explicit admission/rejection outcomes,
preserves the exact size and byte-order gates, and injects sender specificity
without guessing native IPv4/IPv6 representation or binding side effects.

## Zero-context handshake requests and replies

Sender `0xBD2580` requires game-socket state1 and constructs12 bytes:
`[zero DWORD][ASCII POKE][BE WORD 4][BE WORD from socket+0x30]`.
Sender `0xBD26D0` requires state2 and constructs14 bytes:
`[zero DWORD][ASCII KISS][BE DWORD from socket+0x224][BE WORD from +0x32]`.
Both use `0xBD2980`, record a `timeGetTime` sample at `+0x250`, increment
attempt byte `+0x258`, and calculate a retry delay at `+0x254` from
configuration DWORD `+0xDEC`. The first multiplies by the new attempt
count; the second doubles that product. They compare attempts against
socket byte `+0xDF0` (constructor default8) and take a failure helper path
when exhausted. The enclosing tick driver `0xBD4BC0` and its context-ready
transitions are described below; timers and restart paths remain only partly modeled.

Zero-context parser `0xBD39D0` recognizes byte-exact, unterminated
four-character magic values:

| Required socket state | Minimum body bytes | Magic | Observed action |
| ---: | ---: | --- | --- |
| Any | 5 | `ABRT` | Store reason byte at `+0x11C`, log; for reason0..5 invoke error helper using mapping `[0,12,12,10,11,13]` |
| 1 | 8 | `HEHE` | Decode BE DWORD at body+4 into socket context candidate `+0x224` |
| 2 | 8 | `HUGG` | Decode BE WORDs at body+4/+6; require second nonzero, store first at `+0x228`, second as remote port `+0x248`, and copy candidate `+0x224` to expected context `+0x114` |

Unknown ABRT reasons still log/store the byte but do not index the
six-entry map. Accepted replies may contain trailing bytes, which these
branches do not inspect. Other signatures/state combinations and a zero
HUGG port log an invalid-message path and increment the rejection counter.
These branch effects alone do not change state `+0x108` or prove a
completed handshake.

`GameSocketHandshake.cs` reconstructs the two request byte arrays and
typed reply decoding. It keeps input words opaque, does not guess a
security meaning for the context candidate, and does not send or apply
state changes. Nonzero-context parser `0xBD3140` separately walks
size-prefixed chunks and dispatches channel0 control bodies; that control
state machine is the next unresolved handshake boundary.

## Chunk headers, control routing and Connected state

The native log-name table at VA `0x1C73A10` (raw `0x1872610`) establishes
all ten socket states, in ID order:
`NotOpened`, `Initializing`, `Transferring`, `Connecting`,
`WaitingContact`, `Connected`, `DisconnectedNotify`, `Disconnected`,
`Destroyed`, and `Closed`. These are the socket's states, not the Login
connection object's distinct state DWORD. `GameSocketStateAndControlCatalog.cs`
preserves those names and supported control opcodes.

Parser `0xBD3140` reads two-byte network-order chunk headers: channel in
bits14..15, payload marker in bits12..13, continuation in bit11, and
declared total length in bits0..10. Length must be2..1392 and fit the
remaining body. Continuation is allowed only for channels1/2; channel0
requires marker0. The parser advances by the declared total length. It
unconditionally reads a network-order sequence WORD at chunk offset2 for
channels1..3 and uses body-slice offset4; channel0 writes sequence0 and uses
body-slice offset2. Channel0 with marker0 first calls immediate control
`0xBD3510` with the bytes after the two-byte header, and queues only an
unhandled result. A channel0 nonzero marker is logged and increments socket
`+0x4C`; malformed length/continuation paths increment `+0xEC`.

The queued fragment record stores its normalized header at `+0`, sequence
WORD at `+2`, body-slice offset DWORD at `+4`, an embedded shared-body
`slBlob` at `+8`, and a `timeGetTime` sample at `+0x18`; it is appended to
the socket queue at `+0x260`. The `+4` value is an offset, not a payload
length. No `length >= 4` check precedes the nonzero-channel sequence read:
declared lengths2 and3 pass the header gate but leave only zero or one
sequence bytes inside that chunk. The read can cross into the following
chunk; this establishes a declared-chunk-boundary overread, not by itself
an access beyond the backing allocation. The later data-channel handlers
derive payload length as `declared length - 4` and reverse the marker XOR.
`GameSocketChunkHeader.cs` consequently models only the header gate, not the
complete native parser. Iteration stops once fewer than two body bytes
remain; a lone trailing byte after processed chunks is not parsed as another
header. `GameSocketQueuedReceiveNativeLayout.cs` records the queue and
fragment-field offsets without implementing a full parser.

Immediate channel0 handler `0xBD3510` rejects an empty body or a first
opcode byte with mask `0x08` set. Its translated table (raw `0xBD3738`)
maps opcode0 to close/error handling, 1 and6 to handled no-ops
(keepalive/probe padding), 4 to clock-query response, and5 to preliminary
clock-reply processing. Opcodes2/3 and otherwise-unrecognized values
return unhandled so the body can enter the deferred queue. Empty bodies and
the masked-opcode case log, increment socket `+0x4C`, and return handled, so
they do not enter that queue. No wider control code is inferred.

Deferred handler `0xBD3760` uses a separate translated table at raw
`0xBD39B4`. Opcode2 selects the channel1 queue at socket `+0xD6C`; opcode3
selects channel2 at `+0xD70`. After the control opcode, both require at least
two network-order WORDs and an even byte count. The first word advances queue
field `+0x2C` (`PeerWindowSequence`); the second advances `+0x2E` and
cumulatively retires outstanding sequence entries at or before that frontier.
Any remaining WORDs are selective acknowledgments: each is looked up by
sequence in the outstanding tree and an exact match is retired. Comparisons
use 16-bit modular ordering. `GameSocketAcknowledgmentBody.cs` decodes this
bounded body without mutating queue state.

Outbound ACK construction is `0xBE4F20`, called twice by datagram assembler
`0xBD4340`: queue `+0xD6C` with opcode2, then queue `+0xD70` with opcode3.
The chunk bytes are `[00][length][opcode][BE16 next-receive][BE16 cumulative]
[BE16 selective-sequence...]`. The two frontier fields come from queue
`+0x2A` (next receive sequence) and `+0x30` (last contiguous received
sequence); those correspond to the peer-facing `+0x2C/+0x2E` fields updated
by the incoming ACK handler. The builder emits only when its pending-sequence
count at `+0x48C` or pending flag at `+0x32` is set.

The builder consumes the received-sequence tracker at `+0x47C`. A sequence
exactly one past `+0x30` advances the cumulative frontier and the ring head;
other forward sequences are appended as individual selective WORDs and
marked in a 256-entry bitmap at `+0x490`, with head index at `+0x590`.
After writing the current `+0x30` frontier to the chunk, it collapses
contiguous set bits for a later ACK. The one-byte total-length field starts
at7 and allows at most124 selective entries (chunk length255). The assembler
places these control chunks before queued data fragments. This documents the
outbound ACK builder's observed projection only; it does not construct or send
datagrams in C#.

The pending flag at queue `+0x32` is initialized to zero by activation
`0xBE3F60`, set by `0xBE5440` after it advances the next-receive sequence and
releases a contiguous fragment, then cleared by the ACK builder. `0xBE4F20`
returns without a chunk only when both the tracker count `+0x48C` and this
flag are zero. Thus the flag can request a frontier-only ACK when there are
no selective sequence entries; origins beyond these observed stores remain
unresolved.

For acknowledged fragments with a valid elapsed-time sample, `0xBE43B0`
updates the retry interval to
`clamp((old + ceil(sample/2) + 2*sample) / 2, 250, 2500)` milliseconds.
The associated smoothed-RTT statistic is averaged only when the fragment's
attempt byte is1. After processing the reported frontiers and selective
sequences, the handler scans outstanding entries in the observed 256-slot
range; entries at least250 milliseconds old are passed to `0xBE4DE0` for
retry preparation. This records the observed gate, not a complete resend
or selective-ACK protocol implementation.

Opcode5 requires a17-byte body (19-byte chunk including header), calls clock-sample
processor `0xBE82E0`, and then, **if socket state is3/Connecting and
clock sample count at clock object+0x10 is at least2**, invokes state
setter `0xBD1B70` with5/Connected. Otherwise, while Connecting, it resets
the attempt byte and sends another clock query through `0xBD2840`.
Sample processing increments that counter only until it reaches2.

Clock queries encode a64-bit `timeGetTime()*1000` value and use control
opcode4; retry delay grows by powers of two times the configured base.
Immediate query handling echoes eight request bytes and appends a local
clock value under opcode5. Complete clock estimation and callback units
remain partial, so no full clock-synchronization implementation is saved.

The Connected setter case validates its prior state and destination,
logs/renames the socket description, and records timing samples before
committing state5. The transition driver is described below; no direct
Connected owner callback has been established. This proves a concrete native
**transport Connected-state gate**, not a World-mode transition or
acceptance of the initial application packet by the server.

## Context-driven handshake tick

The locked GameSocket virtual `+0x10` is raw `0xBD4BC0`. It consumes the
received queue at socket `+0x260` through `0xBD3E50`, then dispatches
states1..9 using the translated table at raw `0xBD505C`.

Initializing/state1 with a nonzero HEHE candidate `+0x224` clears the
expected context word, records a tick, decrements the shared attempt byte
if nonzero, enters Transferring/state2, and sends KISS immediately.
Without a candidate it instead checks abort/timer state and can send POKE.

Transferring/state2 with a nonzero HUGG-established context `+0x114`
enters Connecting/state3 when a clock object exists. It activates all
three channel queues using HUGG's first WORD at `+0x228`. The shared
reliable-queue activator `0xBE3F60` establishes that this WORD is a
**sequence seed**: it sets next-send/receive WORDs to the seed, frontiers
to seed-1, and initial retry interval to1000 milliseconds. If there is
no clock object, the tick calls the state setter to enter Connected/state5
directly. The observed matrix open normally allocates a clock object.

A timed-out Transferring path does **not** simply resend KISS: it increments
a separate restart byte `+0x11D`, clears the candidate, and resets to
Initializing/state1 while its configured restart limit permits. Connecting
checks its timer and sends clock queries; deferred clock replies supply
the two-sample Connected gate described above. Complete abort/failure,
WaitingContact, teardown and owner-notification logic remains partial.

`GameSocketTickNativeLayout.cs` reconstructs only the two immediate
context-ready transitions, returning no transition when their prerequisites
are absent. It deliberately does not hide timers, aborts or restart counters
behind a full state-machine API.

## Queued receives and owner callbacks

`0xBD3E50` consumes a mixed queue: a zero result is skipped, nonzero values
below `0x10` are internal event tokens, and values at or above `0x10` take
the fragment-pointer path. Token1 is produced by close completion
`0xBD2D10`; it enters disconnect-notification state6 unless already
Disconnected/state7 or Closed/state9, and enters Closed/state9 from
Destroyed/state8. Token2 is produced by completion error `0xBD2D70` and
failed sender binding in `0xBD3030`; the consumer calls
`GameSocket_RequestPumpClose` (`0xBD2430`). Token3 is produced by `0xBD3030`
after sender binding succeeds. It acts only in WaitingContact/state4, checks
the retained address/port at `+0x230/+0x248`, then calls the state setter for
Connected/state5; diagnostics do not suppress that setter call. Other
nonzero values below `0x10` log and take the same disconnect/close fallback;
their producers and exact meaning are unresolved.

Fragment processing asserts state3,5 or8. With suppression byte `+0xD95`
set it releases a fragment without delivery. While Connecting/state3,
only channel0 deferred control is processed; early data-channel fragments
are released without normal application delivery.

In states5/8 the translated channel table at raw `0xBD422C` routes:
channel0 to deferred control `0xBD3760`; channels1/2 to reliable receive
`0xBE51F0` through their separate queues; channel3 to `0xBE5A40`.
Successful data-channel handlers take responsibility for ownership:
the caller does **not** unconditionally return every fragment to the
allocator. Failed data handling invokes failure/close logic and releases
the fragment. Full reassembly and receive-window algorithms remain partial.

GameSocket virtual `+0x14`, raw `0xBD5170`, drains assembled data toward
the handler at socket `+0x10C`. Channel1 uses handler slot `+4`.
Channel2 uses slot `+8` with argument1; channel3 uses that same slot with
argument0. Channel3 draining requires current socket state5 and releases
the socket's second lock around the callback. Channel1/2 drains stop when
the handler pointer becomes null and are not gated by state5 in this
method. These are delivery observations, not a reconstructed lifetime
guarantee.

For DisconnectedNotify/state6, the delivery method transitions to
Disconnected/state7, closes the handle where necessary, releases its
lock and invokes handler slot `+0xC`. It returns false when the socket is
Closed/state9. No explicit Connected callback was found in either this
delivery method or the Connected setter case.

The oracle connection open stores the **MatrixConnection object itself**
as socket handler (`0x496121`). Constructor-installed primary vtable
VA `0x1C642D0`, raw `0x1862ED0`, maps:

| Slot | Native entry | Observed route |
| ---: | ---: | --- |
| `+4` | `0x4904A0` | Passes the retained channel1 descriptor to `0x33CE40` with `this = connection+4` |
| `+8` | `0x4904C0` | Forwards buffer and flag through connection `+0x910` handler slot `+8` |
| `+0xC` | `0x490540` | Disconnect/error handling; also invokes `+0x910` handler slot `+0xC` |

`GameSocketQueuedReceiveNativeLayout.cs` exposes only these routing and
delivery choices. Channel1 descriptor dispatch is described below; socket
state5 alone is not an assumed Login-to-World transition.

## Decoded server-message dispatch and msg-35

Channel1 wrapper `0x4904A0` adjusts `this` by4 before calling `0x33CE40`.
This is a **secondary receiver subobject**, not the primary transport
handler interface and not a byte-stream decoder. The dispatcher reads
the supplied object's DWORD at `+0x10` as an ID, asserts outside34..68,
and dispatches through the translated table at raw `0x33D424`. All35
cases exist. They use receiver virtual slots `+4..+0x8C`, in ID order.
Optional logs supply only names `msg-34` through `msg-68`.
The constructor installs receiver vtable VA `0x1C6423C`, raw `0x1862E3C`.
`MatrixServerMessages\MatrixServerMessageCatalog.cs` saves the complete
ID/slot/handler mapping without assigning unsupported semantic names or
claiming descriptor dispatch itself decodes bytes. The wire-header connection
is established separately below.

`MatrixServerMessageNativeTypes.cs` separately records all35 **original**
`matrix_fury` RTTI identities with factory-case, constructor, vtable-store,
COL, type-descriptor and name addresses. The factory at raw`0x33D4B0`
indexes raw`0x33E250` by ID minus34. Each case calls its recorded constructor;
the constructor passes that ID to its base initializer and installs the
recorded vtable. Vtable[-1] leads through COL+0C to the type descriptor and
its exact decorated name at+8. These are independently recovered paths,
not a mapping inferred from neighboring RTTI entries. Numeric log names
are retained unchanged; `MatrixServerMessageCatalog.Get(id).OriginalType`
exposes the separate identity. See the [body index](MatrixServerMessageBodies.md)
for all names. Class names alone do not prove scalar field meanings.

Message35 routes to `0x490AD0`. Its `this` is connection+4, so the
native `this+8` state accesses are **connection+0xC**, not socket state.
It asserts connection state3, then:

| Descriptor source | Connection destination | Observation |
| ---: | ---: | --- |
| `+0x28` DWORD | `+0x860` | Low half of an opaque decoded QWORD |
| `+0x2C` DWORD | `+0x864` | High half of that QWORD |
| `+0x34` field | `+0x86C` | Reference-backed copy; full representation unresolved |
| Injected clock sample | `+0x818` QWORD | Socket clock helper `0xBE8410`, or `timeGetTime()*1000` fallback |
| Runtime configuration | `+0x83D` byte | Clears then combines independent masks1,2,4 |

It writes **connection state4** at raw `0x490BB4`. This is a concrete
server-descriptor-driven advance beyond successful transport opening.
The handler logs the original text **"Connected to matrix"**; state4's
formal enum name and relationship to World construction remain unresolved.
`MatrixMessage35NativeLayout.cs` saves only the scalar
update with an injected clock sample; it explicitly does not replace the
reference-backed copy or a wire decoder.

## EnterZone descriptor, preview overrides and state6

Message37 routes to `0x490C10`, whose original log is
`EnterZone "%s": zone %u, instance %016llx, match %016llx`.
The argument pushes establish descriptor fields:
instance ID `+0x28` QWORD, zone ID `+0x30` DWORD, match ID `+0x58`
QWORD, and zone-name shared text `+0x68`. This establishes their logged
meaning; their packed wire encodings are described below. The handler asserts connection
state4, then writes **connection state6** at raw `0x490CAA`.

Its instance guard rejects zero and every value with all-FF upper56 bits,
not merely the single all-FF QWORD. Copy helper `0x491070` transfers the
zone-field region from descriptor+28 to connection+878, including
reference-counted text and a nested field. A separate copy transfers64
opaque bytes from descriptor+80 to connection+8D0. Neither that block's
contents nor the complete nested/reference-backed representation is
modeled as recovered source.

Preview byte descriptor+40 goes to connection+890. The literal
`Preview` / `Disabled` option clears its low-three flags first;
`ForceZonePreview`, `ForceAssetPreview`, and `ForceDataPreview` then
independently **set**, not toggle, masks1,4,2 respectively. Other bits are
preserved. The resulting low-three flags select15 or30 for a DWORD at
global-object pointer `0x1E46770` +34; its metric/units remain unknown.
Descriptor byte+C0 causes the log **"Set to connect as spectator"** and
sets connection byte+83D mask2; false does not clear an existing mask.

The handler also initializes downstream objects, can emit runtime
message110 with a `paused` value, and can submit outgoing message24.
Those selections and the response body are now traced below; a World
mode queue request is still not established. `MatrixEnterZoneNativeLayout.cs` reconstructs only the identified
scalar fields, instance guard, state4→6, preview choices and spectator
mask. It does not silently substitute for those remaining side effects.

## Reliable receive window and descriptor creation boundary

Reliable receive `0xBE51F0` requires an active queue and returns false
when its failure flag is set. It computes WORD-wrapped distance between
fragment sequence and queue's next expected sequence `+0x2A`:
0..255 is in-window;256..32767 is beyond the window;32768..65535 is
treated as before-window. The exact native gate is
`(ushort)(distance - 256) > 0x7EFF`. Before-window and duplicate-slot
fragments are released as successfully handled drops; beyond-window
fragments also drop but skip the sequence-tracker call at queue+47C.

New in-window fragments occupy pointer ring `+0x78`, slot
`(headIndex(+0x478) + distance) % 256`. Duplicate occupancy must match
the same sequence. Only a new fragment applies the attempt-marker XOR
in place before storage, over payload length `declaredLength - 4`, using the
body-slice offset at fragment `+4` and the embedded body object at `+8`.
Receive-side XOR reverses the previously saved send transform; it is not a
second protocol or cryptographic layer.

`0xBE56D0` consumes only contiguous available head slots through
`0xBE5440`. Continuation fragments append to queue's reassembly buffer
`+0x594` without producing a decoded object. A final fragment calls the
queue's **plain function pointer at+0x14**, not an owner virtual method:
directly on a single fragment slice, or on the accumulated payload at offset0.
The receive queue's `+0x04` channel DWORD is written to decoded-object `+0x20`,
and payload size to `+0x24`; the object is enqueued at queue `+0x60` for the
previously traced owner delivery. The consumed ring slot is cleared, head
advances modulo256, expected sequence advances modulo65536, the native
fragment is released, and ACK-pending byte `+0x32` is set.

A nonzero DWORD limit at socket `+0x120` is checked only on the continuation
path, using `accumulatedSize + currentPayloadLength >= limit`; zero skips
that check. No equivalent comparison is visible when the final fragment is
appended, so this is not evidence of a complete final-message size cap.
Continuation-limit failure sets queue failure flag `+0x1C` and code `3` if
no code was already set. A null decoder result likewise sets the failure
flag and code `4` if unset. Neither path produces a decoded object.

`0xBD5080` drains reliable queues `+0xD6C` and `+0xD70` in order. If either
drain fails, it atomically stores that queue's failure code at socket `+0x124`
if no earlier error is present, snapshots socket state `+0x108` to `+0x128`,
and requests pump close with the failure code.
Decoder addresses and the header reader are established below; the complete
accumulator ABI and per-message field decoding remain partial.

`GameSocketReliableReceiveWindow.cs` saves the exact classification and
ring-slot math, exhaustively checked over all65536 sequence distances.
It does not implement acknowledgment tracking, buffer ownership, or a
success-shaped replacement for the unresolved decoder.

## Installed channel codecs

The MatrixConnection constructor explicitly installs:

| Channel | Opaque tag WORD | Connection tag/callback fields | Decoder VA → raw |
| ---: | ---: | --- | --- |
| 1 | `0xB968` | `+0x824` / `+0x828` | `0x73EEE0` → `0x33E2E0` |
| 2,3 | `0x4C5F` | `+0x82C` / `+0x830` | `0xC227A0` → `0x821BA0` |

`MatrixConnection_OpenTransport` passes `this+0x824` at raw callsite
`0x4961AD` to GameSocket open `0xBD0960`. Open copies the first pair from
configuration offsets `+0/+4` into queue `+0xD6C` fields `+0x10/+0x14`;
it copies the second pair from `+8/+0xC` into queues `+0xD70` and `+0xD74`.
Thus channel3 shares channel2's configuration. Reliable-queue constructor
`0xBE3CE0` zeros its tag/decoder slots, and activation `0xBE3F60` logs when
both the tag WORD and decoder pointer remain zero. The low tag WORDs also
become socket `+0x30/+0x32`, explaining the default values of POKE/KISS's
previously opaque WORDs. The second tag numerically matches Login's zone-list
protocol filter; its derivation/checksum meaning is **not** established.

Channel1 decoder `0x33E2E0` builds a bounded read view from the supplied
buffer, or an optional alternate buffer, reads a compact message code through
`0x34A020`, then calls descriptor factory `0x33D4B0`. IDA flags a bad stack value in this
function, so its inflated virtual-call argument lists are not reliable
prototypes. The callback address and its call edge are established, but
per-message wire fields and complete error handling still require instruction
verification. `MatrixChannelCodecNativeLayout.cs` records the connection
fields and queue-copy offsets, without pretending this is a usable replacement
decoder.

## Compact message-code header

Instruction inspection corrects the initial factory assumption:
`0x34A020` is the compact positive16-bit header reader; `0x33D4B0` is
the descriptor factory. The decoder's call at raw `0x33E3F1` passes the
decoded code to that factory. Its translated table at raw `0x33E250`
covers34..68, matching the secondary receiver's descriptor IDs.

The compact format uses a nonzero byte for1..255. A zero byte escapes to
a **little-endian WORD**, which must exceed255. The native reader throws
on noncanonical extended values0..255; this is not a three-level varint
and cannot encode zero. Matching writer `0x320900` asserts positive
values no wider than16 bits and emits the same form. Thus the known
server IDs34..68 normally have single-byte headers, but header acceptance
alone does not validate their bodies.

`CompactPositiveUInt16Codec.cs` saves both directions, explicitly reports
truncation/noncanonical input, and rejects invalid encoder values.
All65535 supported values are round-trip checked. It reads only a header,
does not discard trailing payload, and does not simulate cursor mutation
or C++ exception unwinding.

## Shared channel-2/3 compact-code dispatcher

Decoder, case-block, factory, serializer and secondary-dispatch function
addresses below are raw file offsets; lookup arrays and vtables are labeled
by VA, with raw offsets where available.
Decoder `0x821BA0` compares primary-view size with `offset+length` and calls
the native diagnostic if the slice is short; no early return for that mismatch
is visible. It then reads a raw QWORD and a compact positive16 code. The
QWORD is copied as two native DWORDs (little-endian on this x86 client); its
semantic meaning is unknown. A nonnull alternate buffer is read from its own
start for these fields, although the primary-slice diagnostic still runs.
If both the primary length and alternate buffer are empty, this decoder
returns0.

Codes1..9 select a route by indexing byte map VA `0xC229C8` (raw `0x821DC8`)
with `code-1`, then indexing the three-entry DWORD table VA `0xC229BC`
(raw `0x821DBC`):

| Compact code | Route-table index | Native case (raw file offset) | Observed result path |
| ---: | ---: | ---: | --- |
| 1-6, 9 | 0 | `0x821CF7` | Builds an RTTI `nl::RoutedMessage` via `0xBC9B80` |
| 7 | 2 | `0x821D7C` | Enters the secondary dispatcher `0x81CC50` |
| 8 | 1 | `0x821D41` | Builds an RTTI `nl::RoutedMultipleMessage` via `0xBC9DB0` |

Codes outside1..9 bypass the table and branch to shared secondary-dispatch
block `0x821D7C`, which calls `0x81CC50`; code7 reaches the same block
through route index2. `0x81CC50` is a large separate dispatcher; its own
byte/argument tables are not mapped here. The two factories produce
different result layouts. RTTI identifies the first vtable at VA `0x1C7394C`
as `nl::RoutedMessage`; its embedded blob is at `+0x28`, with the source
extent copied to `+0x34`, and a DWORD blob-slice offset/length pair at
`+0x38/+0x3C`. Serializer `0xBCB070` writes the slice length then copies that
slice from the embedded blob; its deserializer sets the offset to zero and
reads a DWORD length.
The second vtable at VA `0x1C7395C` is
`nl::RoutedMultipleMessage`; its embedded blob is at `+0x28`, with source
extent at `+0x34` and a WORD slice offset/length pair at `+0x38/+0x3A`.
Serializer `0xBCB120` writes the WORD length and copies that slice; its
deserializer resets the offset and reads a WORD length. RTTI for the base
vtables identifies `nl::MessageBaseT<nl::RoutedMessage>` and
`nl::MessageBaseT<nl::RoutedMultipleMessage>`. These names and slice shapes do
not establish message semantics. The raw QWORD's identity and the complete
factory/callback ABI remain unresolved.
`MatrixOtherChannelDecoderNativeLayout.cs` records dispatch only, not a
replacement decoder.

## Secondary command dispatcher and verified selector cases

The secondary-dispatch target `0x81CC50` is a large raw function
(`0x4B9E` bytes) with two separate selector tables. It reads a leading byte
from its input structure. Byte0 and values `0xF0..0xFF` use the secondary
selector path. For other values, it decrements the leading byte, compares the
result with `0x35`, then uses that decremented value as the index into the
54-byte map at VA `0xC2240C` (raw `0x82180C`). Thus nonzero leading bytes
`0x01..0x36` map to indices `0x00..0x35`; the map result selects a seven-entry
table at VA `0xC223F0` (raw `0x8217F0`). The observed leading-byte ranges are:

| Leading byte | Route index | Raw target | Observation |
| ---: | ---: | ---: | --- |
| `0x01..0x0E` | 0 | `0x81CCAD` | Nested selector table |
| `0x0F..0x12`, `0x20..0x21`, `0x29..0x33`, `0x36` | 1 | `0x8217D9` | Returns zero |
| `0x13..0x19` | 2 | `0x81EE63` | Three dispatcher values (`0x53..0x55`); value `0x53` documented below |
| `0x1A..0x1F` | 3 | `0x81EF6D` | Nine switch entries; entry 0 documented below |
| `0x22..0x25` | 4 | `0x81F206` | Three dispatcher values (`0x53..0x55`); value `0x53` documented below |
| `0x26..0x28` | 5 | `0x81F2EF` | Value `0x53` documented below |
| `0x34..0x35` | 6 | `0x81F34A` | Value `0x53` documented below |

Leading byte0 or `0xF0..0xFF` uses a second table at VA `0xC22630`
(raw `0x821A30`) when its selector is `0x20..0x7A`; values outside that
selector range return zero. Leading bytes `0x37..0xEF` also reach the
zero-return path. These comparisons establish dispatch boundaries only;
RTTI-backed secondary-selector identities are documented below.

The five case-bearing primary routes (indices 2..6) have 17 directly selected
subcases: three at route2, nine at route3, three at route4, and one each at
routes5 and6. All 17 subcases now have native layouts.
Route0's nested table has a separate, complete set of 40 documented selectors
(`0x53..0x7A`); route1 is the zero-return route. The secondary selector table
records 91 dispatch targets. Selectors `0x20..0x5F` now have 64 RTTI-backed
class and body-reader layouts; selectors `0x60..0x67`, `0x68..0x6F`, and
`0x70..0x77` each add eight verified layouts; selectors `0x78..0x7A` add the
final three. All 91 targets now have distinct, RTTI-backed message identities
and reconstructed reader layouts.

### Secondary-selector entries `0x20..0x7A`

This is the separate second table used by the leading-byte-zero and
`0xF0..0xFF` path in `0x81CC50`. Its index is `selector - 0x20`, with accepted
selectors `0x20..0x7A`, and the DWORD table is at VA `0xC22630` (raw file
offset `0x821A30`). It is distinct from the keyed-receiver factory's selector
range `0x02..0x35` and route0's nested Character selectors `0x53..0x7A`.
Secondary selectors `0x53..0x7A` overlap route0's Character selector values
numerically, but belong to separate tables and dispatch paths. In particular,
secondary selectors `0x78..0x7A` below are `VendorProductsResponse`,
`VendorPurchaseResponse`, and `ConductorGlobalAnnouncement`; they are not
route0's `OpenMovieDialog`, `PrivateDialog`, and `PublicDialog` cases.
The original PE confirms `.text` raw offsets map to VA with `+0x400C00`;
`.rdata` vtable/COL locations map with `+0x401400`, while `.data` TypeDescriptor
locations map with `+0x401C00`. Vtable names below were resolved through their
MSVC complete-object locators, not by treating VAs as raw offsets.
Selectors outside `0x20..0x7A` reach the shared zero-return block at raw
`0x8217D9`.

| Selector | Table index | Target VA / raw | Alloc size | Constructor (raw) | Vtable VA / RTTI | Body reader (raw) |
| ---: | ---: | ---: | ---: | ---: | --- | ---: |
| `0x20` | 0 | `0xC1FFBB` / `0x81F3BB` | `0x38` | `0x813400` | `0x1C6E258` / `EncounterToUIMessage` | `0x853B10` |
| `0x21` | 1 | `0xC20023` / `0x81F423` | `0x50` | `0x813630` | `0x1C6E288` / `VotekickInitiated` | `0x853A20` |
| `0x22` | 2 | `0xC2008B` / `0x81F48B` | `0x40` | `0x8138A0` | `0x1C6E278` / `VotekickResponded` | `0x8538D0` |
| `0x23` | 3 | `0xC200F3` / `0x81F4F3` | `0x30` | `0x813AF0` | `0x1C6E228` / `ScoreBoardEnable` | `0x356CF0` |
| `0x24` | 4 | `0xC2015D` / `0x81F55D` | `0x40` | `0x813BA0` | `0x1C6E248` / `ScoreBoardInit` | `0x853720` |
| `0x25` | 5 | `0xC201C5` / `0x81F5C5` | `0x30` | `0x813EA0` | `0x1C6E238` / `ScoreBoardSetWinner` | `0x337180` |
| `0x26` | 6 | `0xC2022F` / `0x81F62F` | `0x28` | `0x813F50` | `0x1C6E1F8` / `ScoreBoardClear` | none |
| `0x27` | 7 | `0xC20283` / `0x81F683` | `0x58` | `0x813FC0` | `0x1C6E218` / `ScoreBoardAddPlayer` | `0x851490` |
| `0x28` | 8 | `0xC202EB` / `0x81F6EB` | `0x30` | `0x814170` | `0x1C6E208` / `ScoreBoardRemovePlayer` | `0x356F80` |
| `0x29` | 9 | `0xC20356` / `0x81F756` | `0x48` | `0x814230` | `0x1C6E1D8` / `ScoreBoardUpdatePlayerStats` | `0x851410` |
| `0x2A` | 10 | `0xC203BE` / `0x81F7BE` | `0x40` | `0x814390` | `0x1C6E1E8` / `ScoreBoardUpdatePlayerStatsFromStats` | `0x851370` |
| `0x2B` | 11 | `0xC20426` / `0x81F826` | `0x48` | `0x814500` | `0x1C6E1B8` / `ScoreBoardUpdatePlayerStatus` | `0x8506B0` |
| `0x2C` | 12 | `0xC2048E` / `0x81F88E` | `0x38` | `0x8145F0` | `0x1C6E1A8` / `MatchLoadingState` | `0x356CF0`, `0x8503E0` |
| `0x2D` | 13 | `0xC20502` / `0x81F902` | `0x30` | `0x814780` | `0x1C6E1C8` / `MatchEndAck` | `0x356F80` (shared continuation) |
| `0x2E` | 14 | `0xC2054A` / `0x81F94A` | `0x40` | `0x814840` | `0x1C6E188` / `ServerProfiler_SendNames` | `0x34E700` twice |
| `0x2F` | 15 | `0xC205BE` / `0x81F9BE` | `0x38` | `0x8149D0` | `0x1C6E178` / `ServerProfiler_SendFrame` | `0x34D6D0` |
| `0x30` | 16 | `0xC20628` / `0x81FA28` | `0x48` | `0x814B50` | `0x1C6E198` / `TempConsoleMessage` | `0x84F7F0` |
| `0x31` | 17 | `0xC20690` / `0x81FA90` | `0x28` | `0x814EF0` | `0x1C6E148` / `ReloadStaticData` | none |
| `0x32` | 18 | `0xC206E4` / `0x81FAE4` | `0x30` | `0x814F60` | `0x1C6E138` / `EncDebugChatMessage` | `0x12EE010` |
| `0x33` | 19 | `0xC2074E` / `0x81FB4E` | `0x30` | `0x8150F0` | `0x1C6E168` / `SendRadioMessage` | `0x84F650` |
| `0x34` | 20 | `0xC207B6` / `0x81FBB6` | `0x30` | `0x8151B0` | `0x1C6E158` / `NpcBehaviorInfo` | `0x12EE010` (shared with `0x32`) |
| `0x35` | 21 | `0xC207FE` / `0x81FBFE` | `0x38` | `0x815340` | `0x1C6E108` / `NpcMonitoringLog` | `0x84F550` |
| `0x36` | 22 | `0xC20866` / `0x81FC66` | `0x48` | `0x815560` | `0x1C6E128` / `NpcNavigationInfo` | `0x84F490` |
| `0x37` | 23 | `0xC208CE` / `0x81FCCE` | `0x40` | `0x815730` | `0x1C6E118` / `NpcHostilityDebugInfo` | `0x84E200` |
| `0x38` | 24 | `0xC20936` / `0x81FD36` | `0x48` | `0x815890` | `0x1C6E0D8` / `NpcPositionalDebugInfo` | `0x84D830` |
| `0x39` | 25 | `0xC2099E` / `0x81FD9E` | `0x40` | `0x815A80` | `0x1C6E0F8` / `NpcShapeDebugInfo` | `0x84C960` |
| `0x3A` | 26 | `0xC20A06` / `0x81FE06` | `0x50` | `0x815C70` | `0x1C6E0E8` / `NpcVoxelInfo` | `0x84BC10` |
| `0x3B` | 27 | `0xC20A6E` / `0x81FE6E` | `0x38` | `0x815E40` | `0x1C6E0B8` / `DebugDrawInfo` | `0x609E50` |
| `0x3C` | 28 | `0xC20AD8` / `0x81FED8` | `0x30` | `0x815F90` | `0x1C6E0A8` / `NpcDevCmdResponse` | `0x12EE010` twice |
| `0x3D` | 29 | `0xC20B4C` / `0x81FF4C` | `0x60` | `0x8161A0` | `0x1C6E0C8` / `DevRequestObjectPositions` | `0x84BA20` |
| `0x3E` | 30 | `0xC20BB4` / `0x81FFB4` | `0x38` | `0x8163A0` | `0x1C6E088` / `DevRequestSpawnTables` | `0x84B470` |
| `0x3F` | 31 | `0xC20C1E` / `0x82001E` | `0x38` | `0x8164E0` | `0x1C6E078` / `DevRequestResourceNodeDebug` | `0x84E230` |
| `0x40` | 32 | `0xC20C88` / `0x820088` | `0x48` | `0x816630` | `0x1C6E098` / `MissionObjectiveUpdated` | `0x84A7F0` |
| `0x41` | 33 | `0xC20CF0` / `0x8200F0` | `0x38` | `0x816880` | `0x1C6E048` / `MissionStatusChanged` | `0x84A640` |
| `0x42` | 34 | `0xC20D58` / `0x820158` | `0x30` | `0x816960` | `0x1C6E038` / `MissionReturnToChanged` | `0x84A520` |
| `0x43` | 35 | `0xC20DC0` / `0x8201C0` | `0x40` | `0x816A20` | `0x1C6E068` / `MissionsAvailable` | `0x84A480` |
| `0x44` | 36 | `0xC20E28` / `0x820228` | `0x30` | `0x816B80` | `0x1C6E058` / `MissionActivationAck` | `0x849AB0` |
| `0x45` | 37 | `0xC20E90` / `0x820290` | `0x30` | `0x816C40` | `0x1C6E008` / `BountyStatusChanged` | `0x8499F0` |
| `0x46` | 38 | `0xC20EF8` / `0x8202F8` | `0x30` | `0x816D00` | `0x1C6E028` / `BountyAbortAck` | `0x8498D0` |
| `0x47` | 39 | `0xC20F60` / `0x820360` | `0x50` | `0x816DC0` | `0x1C6E018` / `BountyActivationAck` | `0x849790` |
| `0x48` | 40 | `0xC20FC8` / `0x8203C8` | `0x40` | `0x816FC0` | `0x1C6DFD8` / `BountyListActiveAck` | `0x849760` |
| `0x49` | 41 | `0xC21030` / `0x820430` | `0x40` | `0x817120` | `0x1C6DFF8` / `BountyListActiveDetailsAck` | `0x8496A0` |
| `0x4A` | 42 | `0xC21098` / `0x820498` | `0x40` | `0x817260` | `0x1C6DFE8` / `BountyListAvailableAck` | `0x849600` |
| `0x4B` | 43 | `0xC21100` / `0x820500` | `0x30` | `0x8173A0` | `0x1C6DFB8` / `BountyClearAck` | `0x849560` |
| `0x4C` | 44 | `0xC21168` / `0x820568` | `0x30` | `0x817460` | `0x1C6DFA8` / `BountyClearPreviousAck` | `0x8494C0` |
| `0x4D` | 45 | `0xC211D0` / `0x8205D0` | `0x40` | `0x817520` | `0x1C6DFC8` / `BountyListPreviousAck` | `0x849420` |
| `0x4E` | 46 | `0xC21238` / `0x820638` | `0x38` | `0x817660` | `0x1C6DF88` / `BountyRerollResponse` | `0x849380` |
| `0x4F` | 47 | `0xC212A0` / `0x8206A0` | `0x30` | `0x817800` | `0x1C6DF78` / `DisplayUiTrackBounty` | `0x3370A0` (direct) |
| `0x50` | 48 | `0xC2130B` / `0x82070B` | `0x38` | `0x8178C0` | `0x1C6DF98` / `Achievements` | `0x848FC0` |
| `0x51` | 49 | `0xC21375` / `0x820775` | `0x30` | `0x817A10` | `0x1C6DF48` / `AchievementUnlocked` | `0x356F80` (shared continuation) |
| `0x52` | 50 | `0xC213BD` / `0x8207BD` | `0x30` | `0x817AD0` | `0x1C6DF38` / `TotalAchievementPoints` | `0x3370A0` (shared continuation) |
| `0x53` | 51 | `0xC21405` / `0x820805` | `0x38` | `0x817B90` | `0x1C6DF68` / `MissionCompletionCounts` | `0x848610`, `0x12EE010` |
| `0x54` | 52 | `0xC21479` / `0x820879` | `0x38` | `0x817D60` | `0x1C6DF58` / `ContentUnlocked` | `0x847DC0` |
| `0x55` | 53 | `0xC214E1` / `0x8208E1` | `0x30` | `0x817F20` | `0x1C6DF08` / `ClientUIAction` | `0x12EE010` (shared continuation) |
| `0x56` | 54 | `0xC21529` / `0x820929` | `0x30` | `0x8180B0` | `0x1C6DF28` / `ArcCompletionHistoryUpdate` | `0x847A70` |
| `0x57` | 55 | `0xC21593` / `0x820993` | `0x38` | `0x818220` | `0x1C6DF18` / `JobLedgerEntriesUpdate` | `0x6095C0` |
| `0x58` | 56 | `0xC215FD` / `0x8209FD` | `0x30` | `0x818370` | `0x1C6DED8` / `TrackRecipe` | `0x3370A0` (shared continuation) |
| `0x59` | 57 | `0xC21645` / `0x820A45` | `0x28` | `0x818430` | `0x1C6DEF8` / `ClearTrackedRecipe` | none |
| `0x5A` | 58 | `0xC21699` / `0x820A99` | `0x30` | `0x8184A0` | `0x1C6DEE8` / `SlotTech` | `0x3370A0` (shared continuation) |
| `0x5B` | 59 | `0xC216E1` / `0x820AE1` | `0x40` | `0x818560` | `0x1C6DEB8` / `InteractableStatusChanged` | `0x846A50` |
| `0x5C` | 60 | `0xC21749` / `0x820B49` | `0x38` | `0x818640` | `0x1C6DEA8` / `SendTipMessage` | `0x846960` |
| `0x5D` | 61 | `0xC217B1` / `0x820BB1` | `0x30` | `0x818730` | `0x1C6DEC8` / `DebugEventSample` | `0x846850` |
| `0x5E` | 62 | `0xC21819` / `0x820C19` | `0x38` | `0x818800` | `0x1C6DE88` / `DebugLagPlayerSample` | `0x846760` |
| `0x5F` | 63 | `0xC21881` / `0x820C81` | `0x48` | `0x8188E0` | `0x1C6DE78` / `DebugLagSimulationSample` | `0x8465C0` |
| `0x60` | 64 | `0xC218E9` / `0x820CE9` | `0x30` | `0x818A10` | `0x1C6DE98` / `DebugLagRaiaSample` | `0x846580` |
| `0x61` | 65 | `0xC21951` / `0x820D51` | `0x58` | `0x818AE0` | `0x1C6DE48` / `DebugEncounterVolumes` | `0x846480` |
| `0x62` | 66 | `0xC219B9` / `0x820DB9` | `0x40` | `0x818CE0` | `0x1C6DE38` / `Trail` | `0x844AD0` |
| `0x63` | 67 | `0xC21A21` / `0x820E21` | `0x38` | `0x818E40` | `0x1C6DE68` / `EncounterDebugNotification` | `0x844A10` |
| `0x64` | 68 | `0xC21A89` / `0x820E89` | `0x60` | `0x819070` | `0x1C6DE58` / `EncounterUIScopeIn` | `0x8448B0` |
| `0x65` | 69 | `0xC21AF1` / `0x820EF1` | `0x50` | `0x819310` | `0x1C6DE08` / `EncounterUIUpdate` | `0x842940` |
| `0x66` | 70 | `0xC21B59` / `0x820F59` | `0x30` | `0x8194D0` | `0x1C6DE28` / `EncounterUIScopeOut` | `0x356F80` (shared continuation) |
| `0x67` | 71 | `0xC21BA1` / `0x820FA1` | `0x90` | `0x819590` | `0x1C6DE18` / `DisplayUiNotification` | `0x8428B0` |
| `0x68` | 72 | `0xC21C0C` / `0x82100C` | `0x38` | `0x819750` | `0x1C6DDF8` / `DisplayMoneyBombBanner` | `0x8420B0` |
| `0x69` | 73 | `0xC21C74` / `0x821074` | `0x68` | `0x819900` | `0x1C6DDC8` / `SetPreloadPosition` | `0x6087E0` |
| `0x6A` | 74 | `0xC21CDE` / `0x8210DE` | `0x38` | `0x8199B0` | `0x1C6DDE8` / `PlaySoundId` | `0x841F80` |
| `0x6B` | 75 | `0xC21D46` / `0x821146` | `0x40` | `0x819A90` | `0x1C6DDD8` / `PlaySoundIdAtLocation` | `0x841ED0` |
| `0x6C` | 76 | `0xC21DAE` / `0x8211AE` | `0x38` | `0x819B60` | `0x1C6DD98` / `PlayDialogScriptMessage` | `0x841DE0` |
| `0x6D` | 77 | `0xC21E16` / `0x821216` | `0x40` | `0x819CC0` | `0x1C6DDB8` / `StopDialogScriptMessage` | `0x841D40` |
| `0x6E` | 78 | `0xC21E7E` / `0x82127E` | `0x38` | `0x819E20` | `0x1C6DDA8` / `PingMap` | `0x841C60` |
| `0x6F` | 79 | `0xC21EE6` / `0x8212E6` | `0x38` | `0x819EF0` | `0x1C6DD78` / `PingMapMarker` | `0x841B70` |
| `0x70` | 80 | `0xC21F4E` / `0x82134E` | `0x40` | `0x819FC0` | `0x1C6DD68` / `EncounterPublicInfo` | `0x841AA0` |
| `0x71` | 81 | `0xC21FB6` / `0x8213B6` | `0x38` | `0x81A120` | `0x1C6DD88` / `RequestActiveEncounters_Response` | `0x841950` |
| `0x72` | 82 | `0xC2201E` / `0x82141E` | `0x30` | `0x81A260` | `0x1C6DD48` / `ShoppingListInit` | `0x5B9670` |
| `0x73` | 83 | `0xC22088` / `0x821488` | `0x50` | `0x81A3D0` | `0x1C6DD38` / `SetClientDailyInfo` | `0x8417D0` |
| `0x74` | 84 | `0xC220F0` / `0x8214F0` | `0x38` | `0x81A570` | `0x1C6DD58` / `GlobalCounterUpdate` | `0x841730` |
| `0x75` | 85 | `0xC22158` / `0x821558` | `0x38` | `0x81A720` | `0x1C6DD08` / `GlobalCounterMilestoneInfo` | `0x841430`, `0x355E50` |
| `0x76` | 86 | `0xC221CC` / `0x8215CC` | `0x38` | `0x81A930` | `0x1C6DCF8` / `ChatMessageList` | `0x83ECF0` |
| `0x77` | 87 | `0xC22236` / `0x821636` | `0x50` | `0x81AA70` | `0x1C6DD28` / `CurrentLoadoutResponse` | `0x83E000` |
| `0x78` | 88 | `0xC2229E` / `0x82169E` | `0x60` | `0x81AC60` | `0x1C6DD18` / `VendorProductsResponse` | `0x83DF60` |
| `0x79` | 89 | `0xC22306` / `0x821706` | `0x48` | `0x81AE20` | `0x1C6DCE8` / `VendorPurchaseResponse` | `0x83B9F0` |
| `0x7A` | 90 | `0xC2236E` / `0x82176E` | `0x88` | `0x81B000` | `0x1C6DCD8` / `ConductorGlobalAnnouncement` | `0x83B8F0` |

All 91 entries have 91 distinct, nonzero table destinations. No shared case or
zero-valued table slot occurs. Selector `0x26`
does have a distinct allocation-failure branch to the common zero-return block;
that is a case-path result, not a zero table entry. Selectors `0x28` and `0x2D`
share a body-reader continuation at raw `0x81F72E`, but their selector-table
destinations and RTTI-identified types remain distinct. Selectors `0x32` and
`0x34` likewise have separate table targets, constructors, and RTTI, while
sharing the NUL-terminated text reader at raw `0x12EE010` through continuation
`0x81FB27`.

All 91 constructors call a selector-specific base-metadata initializer
before storing the derived vtable. For selectors `0x20..0x27` these helpers
are `0x823020`, `0x823090`, `0x823100`, `0x823170`, `0x8231E0`, `0x823250`,
`0x8232C0`, and `0x823330`; for `0x28..0x2F` they are `0x8233C0`, `0x823430`,
`0x8234A0`, `0x823510`, `0x823580`, `0x8236F0`, `0x823760`, and `0x8237D0`;
for `0x30..0x37` they are `0x823840`, `0x8238B0`, `0x823920`, `0x823990`,
`0x823A00`, `0x823A70`, `0x823AE0`, and `0x823C50`; selectors `0x38..0x3F`
use `0x823DB0`, `0x823F20`, `0x824090`, `0x824100`, `0x824170`, `0x8241E0`,
`0x824250`, and `0x8243C0`; selectors `0x40..0x47` use `0x824430`,
`0x8244A0`, `0x824510`, `0x824580`, `0x8246E0`, `0x824750`, `0x8247C0`,
and `0x824830`; selectors `0x48..0x4F` use `0x8248A0`, `0x824910`,
`0x824980`, `0x8249F0`, `0x824A60`, `0x824AD0`, `0x824B40`, and `0x824BB0`;
selectors `0x50..0x57` use `0x824C20`, `0x824D80`, `0x824DF0`, `0x824E60`,
`0x824FD0`, `0x825040`, `0x8250B0`, and `0x825120`. The
`0x8250B0` initializer also installs a base vtable before initializing the
common metadata fields. Selectors `0x58..0x5F` use `0x825190`, `0x825200`,
`0x825270`, `0x8252E0`, `0x825350`, `0x8253C0`, `0x825430`, and `0x8254A0`;
selectors `0x60..0x67` use `0x825510`, `0x825580`, `0x825700`, `0x825770`,
`0x8257E0`, `0x8258C0`, `0x825930`, and `0x8259A0`; selectors `0x68..0x6F`
use `0x825AA0`, `0x825B10`, `0x825B80`, `0x825BF0`, `0x825C60`, `0x825CD0`,
`0x825D40`, and `0x825DB0`; selectors `0x70..0x77` use `0x825E20`, `0x825E90`,
`0x825F00`, `0x825F70`, `0x825FE0`, `0x826050`, `0x826130`, and `0x8262A0`;
selectors `0x78..0x7A` use `0x826310`, `0x826380`, and `0x826400`.
Each helper sets a base flag DWORD at `+0x08` to 1, stores its
descriptor-anchor pointer at `+0x0C`, stores the constructor-supplied type
DWORD at `+0x10`, copies the parser input's first two DWORDs to `+0x18/+0x1C`,
and zeros DWORDs at `+0x20/+0x24`. Thus the message body begins at `+0x28`.
The descriptor anchors are opaque metadata pointers; class identities in the
table come from the section-aware vtable RTTI names. The cases for selectors
`0x28..0x2F` write parser-local DWORD literals `0x08..0x0F` into a separate
stack slot. The cases for selectors `0x30..0x37` write `0x10..0x17` there.
Their roles are unknown; numerical coincidence with table indexes does not
establish that they are selector indexes or message discriminators.
Selectors `0x38..0x3F` likewise store parser-local DWORD literals `0x18..0x1F`;
these are recorded separately from selector/table indexes and their roles are
unknown. Selectors `0x40..0x4F` store parser-local DWORD literals `0x20..0x2F`
in the same separate slot; their roles are also unresolved and they are not
the constructor type IDs or table indexes.
Selectors `0x58..0x5F` store parser-local DWORD literals `0x38..0x3F` in that
separate slot. These remain distinct observations from the selector, native
constructor type ID, and descriptor anchor; no role is assigned to the values.
Selectors `0x60..0x67` similarly store parser-local DWORD literals `0x40..0x47`
in that separate slot; their roles are unknown and distinct from selector,
constructor type ID, and descriptor anchor.
Selectors `0x68..0x6F` similarly store parser-local DWORD literals `0x48..0x4F`;
their roles remain unknown and distinct from selector, type ID, and descriptor
anchor. Selectors `0x70..0x77` similarly store parser-local DWORD literals
`0x50..0x57`; selectors `0x78..0x7A` store `0x58..0x5A` in the same separate
slot. These values remain unknown and distinct from selector, type ID, and
descriptor anchor.

Body reads, with field labels deliberately limited to offsets and widths:

- `0x20` `EncounterToUIMessage`: raw QWORD at `+0x28`, then
  NUL-terminated shared-text reads at `+0x30` and `+0x34`.
- `0x21` `VotekickInitiated`: raw QWORD at `+0x28`; shared text at `+0x30`
  and `+0x34`; DWORD at `+0x38`; raw QWORD at `+0x40`; byte at `+0x48`.
- `0x22` `VotekickResponded`: raw QWORD at `+0x28`; shared text at `+0x30`
  and `+0x34`; byte at `+0x38`; WORDs at `+0x3A` and `+0x3C`.
- `0x23` `ScoreBoardEnable` and `0x25` `ScoreBoardSetWinner`: one byte at
  `+0x28` via their respective readers. Neither has a body gate.
- `0x24` `ScoreBoardInit`: a three-pointer record vector at
  `+0x28/+0x2C/+0x30`, followed by bytes at `+0x34`, `+0x35`, `+0x38`,
  `+0x39`, then shared text at `+0x3C`. The collection reader first clears
  existing `0xA8`-stride records. It then reads a byte run count, parses that
  many records when nonzero, and reads another run count only when the prior
  count was `0xFF`; zero ends the sequence. Each record reader at `0x852B70`
  reads QWORDs at `+0x00/+0x08`, shared text at `+0x10`, DWORD at `+0x14`,
  byte at `+0x18`, DWORD at `+0x1C`, bytes at `+0x20/+0x24/+0x29/+0x28`,
  then nested readers at `+0x2C`, `+0x94`, `+0x88` in that order. The
  `+0x2C` reader consumes 18 DWORDs, a byte-run collection of four-DWORD
  items, a DWORD at `+0x80`, and a byte at `+0x84`. The `+0x94` reader
  inserts byte-run entries of three DWORDs (first DWORD used as the tree key);
  it is followed by a DWORD at `+0x9C` and byte at `+0xA0`. The final `+0x88`
  collection contains `0x5C`-stride records. Names and semantics for these
  nested values remain unknown.
- `0x26` `ScoreBoardClear`: no body-reader call; its allocation is exactly the
  `0x28`-byte base object.
- `0x27` `ScoreBoardAddPlayer`: raw QWORDs at `+0x28/+0x30`, shared text at
  `+0x38`, DWORD at `+0x3C`, byte at `+0x40`, DWORD at `+0x44`, byte at
  `+0x48`, then byte at `+0x4C`, byte at `+0x51`, and byte at `+0x50`.
- `0x28` `ScoreBoardRemovePlayer`: raw QWORD at `+0x28`.
- `0x29` `ScoreBoardUpdatePlayerStats`: raw QWORD at `+0x28`, then a
  four-byte wire gate. Gate value 1 reads a WORD-length-prefixed blob into
  `+0x34`; other values reset/default that blob destination. The gate is not
  retained by this reader. The constructor initializes `+0x34` to the
  static value `0x1C80CC4`; neither the gate's application meaning nor the
  blob's semantic type is inferred.
- `0x2A` `ScoreBoardUpdatePlayerStatsFromStats`: raw QWORD at `+0x28`, then a
  byte-counted collection at `+0x30` (begin/end/capacity pointers at
  `+0x30/+0x34/+0x38`). Count `0xFF` continues with another count byte; zero
  ends the collection. Each collection item reads a DWORD, then a nested
  structure whose DWORD count frames repeated NUL-terminated shared-text
  entries followed by DWORD tags. The nested tag reader dispatches values
  1..16; the exact per-tag payload layouts remain unresolved (one branch is
  observed to read NUL-terminated shared text). Native top-level entries
  have `0x10`-byte stride. These count/tag values are framing observations; no
  application meanings or field names are assigned.
- `0x2B` `ScoreBoardUpdatePlayerStatus`: raw QWORD at `+0x28`, then byte at
  `+0x30`, DWORD at `+0x34`, byte at `+0x38`, byte at `+0x3C`, byte at
  `+0x41`, and byte at `+0x40`, in that read order. The constructor
  initializes WORD `+0x40` to 1; meanings of the scalar fields remain open.
- `0x2C` `MatchLoadingState`: byte at `+0x28`, then a byte-counted vector at
  `+0x2C/+0x30/+0x34`. Count `0xFF` continues with another count byte; zero
  ends the vector. Each `0x10`-stride item reads raw QWORD, NUL-terminated
  shared text, then one byte.
- `0x2D` `MatchEndAck`: raw QWORD at `+0x28`; it reuses selector `0x28`'s
  `0x356F80` body-reader continuation. On allocation failure, its case
  branches to selector `0x28`'s zero-object block at `0x81F72C`, then reaches
  the same reader at `0x81F72E`.
- `0x2E` `ServerProfiler_SendNames`: two vectors at
  `+0x28/+0x2C/+0x30` and `+0x34/+0x38/+0x3C`, each read by `0x34E700`.
  Each vector uses a byte count with `0xFF` continuation and zero termination;
  each item reads a WORD followed by NUL-terminated shared text and has
  `0x08`-byte native stride.
- `0x2F` `ServerProfiler_SendFrame`: root vector at
  `+0x28/+0x2C/+0x30`, with byte counts and `0xFF` continuation. Each root
  entry is a WORD followed by a recursively counted child vector. Root entries
  have `0x10`-byte native stride (WORD at `+0`, child vector at `+4`). Each
  nested entry reads nine WORDs before its child vector, uses `0x20`-byte
  native stride (first WORD at `+0`, child vector at `+4`, eight trailing
  WORDs at `+0x10..+0x1E`), and recursively repeats the same nested shape.
  These WORDs remain offset-only fields.
- `0x30` `TempConsoleMessage`: four NUL-terminated shared-text reads at
  `+0x28`, `+0x2C`, `+0x30`, and `+0x34`, then a WORD-length-prefixed byte
  blob at `+0x38`. The prefix is a 16-bit little-endian length; zero skips the
  blob read. Its constructor writes `0x1C80CC4` to `+0x38`; that initializer
  value is recorded without assigning it a semantic name.
- `0x31` `ReloadStaticData`: allocation is `0x28` bytes, exactly the common
  message header. No body-reader call follows its constructor.
- `0x32` `EncDebugChatMessage`: one NUL-terminated shared-text field at
  `+0x28` via `0x12EE010`.
- `0x33` `SendRadioMessage`: DWORDs at `+0x28` and `+0x2C`, in that order,
  through the four-byte stream readers `0x3370A0` and `0x353310`.
- `0x34` `NpcBehaviorInfo`: one NUL-terminated shared-text field at `+0x28`
  via the same reader and continuation as selector `0x32`.
- `0x35` `NpcMonitoringLog`: NUL-terminated shared text at `+0x28`, one byte
  at `+0x2C`, then NUL-terminated shared text at `+0x30`.
- `0x36` `NpcNavigationInfo`: raw QWORD at `+0x28`, followed by two distinct
  byte-counted vectors. The first vector at `+0x30/+0x34/+0x38` uses count
  bytes (`0xFF` continues with another count; zero ends); each item is three
  DWORDs (12 wire bytes) and is appended with 12-byte native stride. The
  reader zero-extends each count byte into its item-loop counter and
  decrements it at raw `0x84E390`; the `jnz` at `0x84E391` returns to the
  item reader at `0x84E294`. The second vector at `+0x3C/+0x40/+0x44` uses
  the same count framing and has `0x70`-byte native entries. Each entry begins
  with seven 32-bit values. Its byte counter is incremented and compared with
  the current chunk count at `0x84F374`; `jb` at `0x84F376` returns to raw
  `0x84EC90` while the count is not reached. The instruction-aligned gate
  producer starts at raw `0x84EF2E` (raw `0x84EF30` is inside its `mulss`
  instruction): it squares and sums staged scalar values, subtracts a
  referenced DWORD, ANDs with a referenced 128-bit value, then uses `comiss`
  against a referenced scalar. The following `jb` at `0x84EF54` targets
  cleanup/exit at raw `0x84F3A4`; because `comiss` sets CF for below and
  unordered comparisons, the branch can cover either case. The referenced
  constants and the predicate's application meaning remain unresolved. When
  the branch is not taken, the reader consumes three more DWORDs and one byte.
- `0x37` `NpcHostilityDebugInfo`: raw QWORD at `+0x28`, then a byte-counted
  vector at `+0x30/+0x34/+0x38`. Each wire item is a QWORD followed by a DWORD
  (12 wire bytes), stored at `0x10`-byte native stride. Count `0xFF` continues
  with another count; zero ends the vector.
- `0x38` `NpcPositionalDebugInfo`: raw QWORD at `+0x28`, shared text at
  `+0x30`, then a byte-counted record vector at `+0x34/+0x38/+0x3C`. Each
  record reads a QWORD followed by eight DWORDs (0x28 wire bytes and native
  stride), then the message has a byte at `+0x40`. Count `0xFF` continues
  with another count; zero ends the vector.
- `0x39` `NpcShapeDebugInfo`: raw QWORD at `+0x28`, shared text at `+0x30`,
  then a byte-counted record vector at `+0x34/+0x38/+0x3C`. Each record reads
  a leading byte, sixteen DWORDs, four more DWORDs, and a trailing byte
  (0x52 fixed wire bytes); the native stride is `0x58`. The sixteen-DWORD
  reader processes four groups of four DWORDs. Count `0xFF` continues with
  another count; zero ends the vector.
- `0x3A` `NpcVoxelInfo`: raw QWORD at `+0x28`; a byte-counted three-DWORD
  vector at `+0x30/+0x34/+0x38` (12 wire bytes and native stride); a
  byte-counted DWORD vector at `+0x3C/+0x40/+0x44` (four wire bytes and
  native stride); then a byte at `+0x48`. Both vectors use byte counts with
  `0xFF` continuation and zero termination.
- `0x3B` `DebugDrawInfo`: a byte-counted byte vector at
  `+0x28/+0x2C/+0x30`. Each item consumes one byte and has one-byte native
  stride. Count `0xFF` continues with another count; zero ends the vector.
  The payload remains opaque.
- `0x3C` `NpcDevCmdResponse`: two NUL-terminated shared-text reads at `+0x28`
  and `+0x2C`, using the same text reader in sequence.
- `0x3D` `DevRequestObjectPositions`: bytes at `+0x28` and `+0x29`; a
  byte-counted three-DWORD vector at `+0x2C/+0x30/+0x34`; then byte-counted
  DWORD vectors at `+0x38/+0x3C/+0x40`, `+0x44/+0x48/+0x4C`, and
  `+0x50/+0x54/+0x58`, in that order. Items are 12 or four wire bytes,
  respectively, with matching native strides. All four vectors use byte
  counts with `0xFF` continuation and zero termination.
- `0x3E` `DevRequestSpawnTables`: a byte-counted record vector at
  `+0x28/+0x2C/+0x30`. Each item reads NUL-terminated shared text, five DWORDs,
  one byte, then another NUL-terminated shared text; its native stride is
  `0x20`. Count `0xFF` continues with another count; zero ends the vector.
- `0x3F` `DevRequestResourceNodeDebug`: a byte-counted three-DWORD vector at
  `+0x28/+0x2C/+0x30`, with 12 wire bytes and `0x0C` native stride. Count
  `0xFF` continues with another count; zero ends the vector.
- `0x40` `MissionObjectiveUpdated`: raw QWORD at `+0x28`, DWORD at `+0x30`,
  NUL-terminated shared text at `+0x34/+0x38`, byte at `+0x3C`, then DWORD
  at `+0x40`.
- `0x41` `MissionStatusChanged`: raw QWORD at `+0x28`, then one byte at
  `+0x30`.
- `0x42` `MissionReturnToChanged`: DWORD at `+0x28`, then one byte at
  `+0x2C`.
- `0x43` `MissionsAvailable`: a byte-counted vector at
  `+0x28/+0x2C/+0x30`, followed by raw QWORD at `+0x38`. Each wire item is a
  DWORD then a byte (five bytes); entries are stored at eight-byte native
  stride. The count helper loops over each chunk and reads another count byte
  only when the current count is `0xFF`; zero ends the vector.
- `0x44` `MissionActivationAck` and `0x46` `BountyAbortAck`: DWORD at `+0x28`,
  then bytes at `+0x2C` and `+0x2D`.
- `0x45` `BountyStatusChanged`: DWORD at `+0x28`, then one byte at `+0x2C`.
- `0x47` `BountyActivationAck`: DWORD at `+0x28`, bytes at `+0x2C/+0x2D/+0x2E`,
  DWORDs at `+0x30/+0x34`, byte at `+0x38`, raw QWORD at `+0x40`, then
  NUL-terminated shared text at `+0x48`. This reader is called with the object
  in ECX and the stream as its stack argument; selectors `0x40..0x46` use the
  opposite register/stack ordering. Its constructor writes zero at
  `+0x28/+0x2C/+0x2E/+0x30/+0x34/+0x38/+0x40/+0x44/+0x48`. The zeroing
  WORD store at `+0x2C` also initializes byte `+0x2D`; no allocator zero-fill
  behavior is inferred.
- `0x48` `BountyListActiveAck`: byte at `+0x28`, then one wire byte at `+0x2C`
  stored zero-extended as a DWORD, then a WORD-length-prefixed byte span passed
  to the embedded object at `+0x30`. Its constructor initializes byte `+0x28`,
  DWORD `+0x2C`, and the four-DWORD embedded-object region at `+0x30..+0x3C`;
  the first embedded DWORD is `0x1C80CC4`, the remaining three are zero.
- `0x49` `BountyListActiveDetailsAck` and `0x4A` `BountyListAvailableAck`:
  byte at `+0x28`, then a WORD-length-prefixed byte span passed to the embedded
  object at `+0x2C`. Each constructor initializes byte `+0x28`, stores
  `0x1C80CC4` at `+0x2C`, and zeros DWORDs `+0x30/+0x34/+0x38`.
- `0x4B` `BountyClearAck` and `0x4C` `BountyClearPreviousAck`: byte at `+0x28`,
  then one wire byte at `+0x2C` stored zero-extended as a DWORD. Each
  constructor initializes byte `+0x28` and DWORD `+0x2C`; the second reader
  consumes one byte, not the entire initialized DWORD.
- `0x4D` `BountyListPreviousAck`: byte at `+0x28`, then a WORD-length-prefixed
  byte span passed to the embedded object at `+0x2C`. Its constructor
  initializes byte `+0x28`, stores `0x1C80CC4` at `+0x2C`, and zeros DWORDs
  `+0x30/+0x34/+0x38`.
- `0x4E` `BountyRerollResponse`: DWORD at `+0x28`, byte at `+0x2C`, then
  NUL-terminated shared text at `+0x30`. The constructor initializes DWORD
  `+0x28`, byte `+0x2C`, and DWORD `+0x30`; it does not write the intervening
  bytes `+0x2D..+0x2F`, and this reader does not consume them.
- `0x4F` `DisplayUiTrackBounty`: one DWORD at `+0x28`, read directly by the
  case through `0x3370A0`; unlike `0x48..0x4E`, it has no separate body-reader
  function.
- `0x50` `Achievements`: a byte-counted record vector at
  `+0x28/+0x2C/+0x30`. Each wire record is a DWORD followed by one byte (five
  bytes); native records have eight-byte stride. Count `0xFF` continues with
  another count byte; zero ends the sequence.
- `0x51` `AchievementUnlocked`: one raw QWORD at `+0x28`, via the shared
  `0x356F80` reader continuation (`0x81F72E`, call at `0x81F73C`).
- `0x52` `TotalAchievementPoints`: one DWORD at `+0x28`, via the shared
  `0x3370A0` reader continuation (`0x8206E3`, call at `0x8206F1`).
- `0x53` `MissionCompletionCounts`: a byte-counted record vector at
  `+0x28/+0x2C/+0x30`, followed by NUL-terminated shared text at `+0x34`.
  Each record is a DWORD, QWORD, then DWORD (16 wire bytes) stored at
  24-byte native stride. Count `0xFF` continues with another count byte; zero
  ends the vector. The reader clears the vector before processing chunks.
- `0x54` `ContentUnlocked`: NUL-terminated shared text at `+0x28`, then a
  DWORD at `+0x2C` and raw QWORD at `+0x30`, in that order.
- `0x55` `ClientUIAction`: one NUL-terminated shared-text field at `+0x28`
  through the shared `0x12EE010` continuation (`0x81FB27`, call at `0x81FB34`).
- `0x56` `ArcCompletionHistoryUpdate`: a byte-counted sequence of 16-byte
  records inserted into a tree rooted at `+0x28`; each record is a key DWORD,
  QWORD, then trailing DWORD. Count `0xFF` continues with another count byte;
  zero ends the sequence. Its constructor allocates a `0x20` sentinel and
  initializes its three pointer links to itself. It writes WORD `0x0101` at
  sentinel `+0x0C`; this is one word store spanning two adjacent bytes, not
  evidence for two independent byte fields. The reader resets the tree before
  reading and inserts records through `0x8479F0`; no field meanings are
  inferred.
- `0x57` `JobLedgerEntriesUpdate`: a byte-counted DWORD vector at
  `+0x28/+0x2C/+0x30`. Each item is four wire bytes and four-byte native
  stride. Count `0xFF` continues with another count byte; zero ends the vector.
- `0x58` `TrackRecipe`: one DWORD at `+0x28`, via the same reader and
  continuation as selector `0x52`.
- `0x59` `ClearTrackedRecipe`: no body-reader call; its allocation is exactly
  the `0x28`-byte base object.
- `0x5A` `SlotTech`: one DWORD at `+0x28`, via the same reader and
  continuation as selectors `0x52` and `0x58`.
- `0x5B` `InteractableStatusChanged`: byte at `+0x28`, DWORD at `+0x2C`, raw
  QWORD at `+0x30`, then byte at `+0x38`.
- `0x5C` `SendTipMessage`: byte at `+0x28`, DWORDs at `+0x2C/+0x30`, WORD at
  `+0x34`, then byte at `+0x36`.
- `0x5D` `DebugEventSample`: WORDs at `+0x28/+0x2A`, then bytes at
  `+0x2C/+0x2D`.
- `0x5E` `DebugLagPlayerSample`: five WORDs at `+0x28`, `+0x2A`, `+0x2C`,
  `+0x2E`, and `+0x30`.
- `0x5F` `DebugLagSimulationSample`: thirteen WORDs at two-byte intervals
  from `+0x28` through `+0x40`.
- `0x60` `DebugLagRaiaSample`: four WORDs at `+0x28`, `+0x2A`, `+0x2C`,
  and `+0x2E`.
- `0x61` `DebugEncounterVolumes`: four byte-counted vectors. The first
  contains four-float (`0x10`-wire-byte) records at `+0x28`; the next two
  contain `0x40`-wire-byte records at `+0x34` and `+0x40`; the last starts at
  `+0x4C` and contains NUL-terminated shared text followed by `0x40` bytes.
  Their native strides are respectively `0x10`, `0x40`, `0x40`, and `0x44`.
  Each count is a byte; `0xFF` continues with another count and zero ends that
  vector. The record payloads remain offset-only.
- `0x62` `Trail`: DWORD at `+0x28`, byte at `+0x2C`, then a byte-counted
  vector at `+0x30` of three-float records (12 wire bytes and 12-byte native
  stride). Count `0xFF` continues; zero ends the vector.
- `0x63` `EncounterDebugNotification`: raw QWORD at `+0x28`, then
  NUL-terminated shared-text reads at `+0x30` and `+0x34`.
- `0x64` `EncounterUIScopeIn`: raw QWORD at `+0x28`; a WORD-length-prefixed
  byte blob at `+0x30`; WORD at `+0x40`; a tree at `+0x44` with its root
  pointer and count at `+0x44/+0x48`; then the embedded blob at `+0x4C`
  receives all remaining bytes in the current input slice, if any. Tree
  entries read a QWORD key, QWORD, DWORD, then a byte-counted vector of tagged
  entries. Both count levels use `0xFF` continuation and zero termination.
  Tagged entries have `0x18`-byte native stride. Tag `0` reads a DWORD to
  `+8` and clears DWORD `+0x0C`; tag `1` reads a DWORD to `+0x10`; tags `2`/`3`
  read a QWORD to `+8`; tag `4` reads one byte to
  WORD `+0x14`; tag `6` reads a WORD to `+0x14`; tag `7` reads a byte to WORD
  `+0x14`, then reads a DWORD to `+0x10` when bit `0x02` is set, otherwise a
  QWORD to `+8`; tag `8` writes WORD 1 without a payload. Special tag `0xFE`
  writes WORD 0, and `0xFF` takes a no-payload path. Tag `5` and values
  `0x09..0xFD` reach the same invalid/unsupported path. These tag values and
  fields have no assigned application semantics.
- `0x65` `EncounterUIUpdate`: raw QWORD at `+0x28`; a WORD-length-prefixed
  byte blob at `+0x30`; then all remaining bytes in the current input slice
  are copied into the embedded blob at `+0x40`, if any remain.
- `0x66` `EncounterUIScopeOut`: raw QWORD at `+0x28` through the shared
  `0x356F80` reader continuation at raw `0x81F72E`. The allocation-failure
  branch targets the shared zero-object block at raw `0x81F72C`; this is
  static control flow only and does not assert runtime consequences.
- `0x67` `DisplayUiNotification`: DWORDs at `+0x28/+0x2C`; a byte-counted
  vector at `+0x30`; then byte at `+0x8C`. Each vector record reads a byte,
  NUL-terminated shared text, DWORD, and float. Records have `0x10`-byte
  native stride; count `0xFF` continues with another count and zero ends the
  vector.
- `0x68` `DisplayMoneyBombBanner`: DWORD at `+0x28`, NUL-terminated shared
  text at `+0x2C`, then DWORD at `+0x30`.
- `0x69` `SetPreloadPosition`: sixteen DWORDs at four-byte intervals from
  `+0x28` through `+0x64` (four groups of four in the reader loops).
- `0x6A` `PlaySoundId`: raw QWORDs at `+0x28` and `+0x30`.
- `0x6B` `PlaySoundIdAtLocation`: raw QWORD at `+0x28`, then DWORDs at
  `+0x30`, `+0x34`, and `+0x38`.
- `0x6C` `PlayDialogScriptMessage`: DWORD at `+0x28`, then a vector at
  `+0x2C/+0x30/+0x34` of opaque eight-byte wire records stored at eight-byte
  native stride.
- `0x6D` `StopDialogScriptMessage`: DWORD at `+0x28`, the same eight-byte
  record vector at `+0x2C/+0x30/+0x34`, then one byte at `+0x38`.
- `0x6E` `PingMap`: DWORDs at `+0x28`, `+0x2C`, `+0x30`, and `+0x34`.
- `0x6F` `PingMapMarker`: raw QWORD at `+0x28`, then DWORD at `+0x30`.
- `0x70` `EncounterPublicInfo`: raw QWORD at `+0x28`, then the remaining
  input bytes are passed to the embedded object at `+0x30` through virtual
  method `+0x10`. The reader advances and decrements the stream by that
  remaining-byte count.
- `0x71` `RequestActiveEncounters_Response`: the remaining input bytes are
  passed to the embedded object at `+0x28` through virtual method `+0x10`;
  there are no preceding scalar reads.
- `0x72` `ShoppingListInit`: a byte-counted tree at `+0x28`. Each entry
  consumes two DWORDs and is inserted through the tree helper. Count `0xFF`
  continues with another count byte; zero terminates. The two DWORD roles and
  tree-key semantics remain unknown.
- `0x73` `SetClientDailyInfo`: DWORD at `+0x28`, then a byte-counted sequence
  at `+0x2C`. Each entry reads one DWORD and passes it to helper `0x38EC20`.
  Count `0xFF` continues; zero terminates. The helper's application semantics
  are unresolved.
- `0x74` `GlobalCounterUpdate`: NUL-terminated shared text at `+0x28`, then a
  raw QWORD at `+0x30`.
- `0x75` `GlobalCounterMilestoneInfo`: two separately byte-counted
  collections at `+0x28` and `+0x30`. Each first-collection entry reads
  NUL-terminated text followed by a nested byte-counted collection; each
  nested item reads two DWORDs, NUL-terminated text, and a DWORD. Each
  second-collection entry reads NUL-terminated text and a DWORD. The outer and
  nested counts use `0xFF` continuation and zero termination.
- `0x76` `ChatMessageList`: byte-counted vector at `+0x28`, with
  `0x40`-byte native stride. Each item reads a QWORD at record `+0`, shared
  text at `+8/+0xC`, and bytes at `+0x10/+0x11/+0x12`; then readers at
  `+0x14`, `+0x28`, and `+0x38` process a gated WORD-length-prefixed blob, a
  gated optional QWORD, and a nested value whose field layout is not yet
  resolved. The first helper always consumes its gate byte; when set it reads a
  WORD-length-prefixed byte sequence, and when clear it clears an existing
  referenced blob. The QWORD helper consumes an independent gate byte and
  reads/stores eight bytes only when set. Count `0xFF` continues; zero
  terminates.
- `0x77` `CurrentLoadoutResponse`: QWORD at `+0x28`, DWORDs at `+0x30/+0x34`,
  shared text at `+0x38/+0x3C`, DWORD at `+0x40`, then a byte-counted vector
  at `+0x44`. Vector records have `0x2E0`-byte native stride; each begins
  with DWORD at record `+0`, text at `+4`, nested readers at `+8/+0x14/+0x20`,
  DWORDs at `+0x2C/+0x30`, and a byte gate at `+0x34`. Before testing the
  gate, a DWORD is read unconditionally at `+0x2D4`. When the gate is
  nonzero, readers then fill `+0x38`, `+0xF8`, `+0x1D8`, `+0x1E4`,
  `+0x1F0`, `+0x2D0`, `+0x2D4`, `+0x2D8`, and finally `+0xF4`; thus
  `+0x2D4` is read both before the gate and on its nonzero path. Count `0xFF`
  continues; zero terminates.
- `0x78` `VendorProductsResponse`: DWORD at `+0x28`, QWORD at `+0x30`, DWORD
  at `+0x38`, shared NUL-terminated text at `+0x3C`, DWORD at `+0x40`, then
  byte-counted vectors at `+0x44` and `+0x50`. Each `+0x44` item is a DWORD
  followed by a four-byte float payload; its native stride is eight bytes.
  The `+0x50` vector has `0x30`-byte native records. Each record reads a QWORD
  at `+0`, DWORDs at `+8/+0xC/+0x10`, then two nested byte-counted vectors
  at `+0x14/+0x20`, and a trailing byte at `+0x2C` normalized to a boolean.
  The first nested vector has `0x18`-byte native records carrying QWORD, text,
  and two DWORDs; the second has eight-byte records carrying two NUL-terminated
  text fields. All three vector levels use `0xFF` continuation and zero
  termination.
- `0x79` `VendorPurchaseResponse`: byte at `+0x28`, QWORDs at `+0x30/+0x38`,
  DWORD at `+0x40`, then shared NUL-terminated text at `+0x44`. The reader is
  straight-line and has no field gate or count loop.
- `0x7A` `ConductorGlobalAnnouncement`: the reader first processes a nested
  block at `+0x28`: DWORDs at `+0x28/+0x2C/+0x30`, NUL-terminated text at
  `+0x34/+0x38`, gated values at `+0x3C/+0x4C`, text at `+0x80`, then gated
  values at `+0x5C/+0x70`; a final byte is read at `+0x84`. Each `+0x3C/+0x4C`
  gate consumes one byte, then a DWORD and a separately gated optional DWORD.
  The `+0x5C` gate consumes one byte, then NUL-terminated text and an optional
  DWORD with its own byte gate. The `+0x70` gate consumes one byte, then two
  DWORDs and NUL-terminated text. These are presence gates, not count loops.
  Field semantics and text encoding remain unknown.

These are positional wire layouts only; RTTI class names do not establish the
meaning of any field. None of the `0x58..0x5F` body readers has a count/vector
loop or a field gate. `TrackRecipe` and `SlotTech` reuse the DWORD reader
continuation at `0x8206E3`; `ClearTrackedRecipe` has no body reader. The other
five readers are straight-line scalar reads in the field orders above. Their
primitive helper calls were checked against the one-byte, WORD, DWORD, and
QWORD load/store widths and cursor increments in `FirefallClient.exe.original`.
The `0x60..0x7A` records above likewise remain offset/width observations;
their count framing and tag dispatch are described separately from field
semantics. All nineteen `0x68..0x7A` cases have body-reader calls; none has a
no-reader success epilog.

The shared NUL-terminated text reader used by selectors `0x53..0x55`, `0x68`,
and `0x74..0x7A` reads bytewise through the terminator in repeating 256-byte
chunks; 256 is a chunk size, not a total-string limit. These text fields have
no assigned application semantics or encoding.

Each case in `0x68..0x7A` pushes a descriptor-anchor immediate, a
selector-specific pointer VA, and `0x1E60904` before calling raw `0x3571B0`;
the target bytes are a single `ret`, so no effect for those pushed values is
established. Each derived constructor instead calls its type-specific
base-metadata helper, which stores the descriptor anchor at `+0x0C`, type ID
at `+0x10`, common flag at `+0x08`, parser-context DWORDs at `+0x18/+0x1C`,
and zeroes `+0x20/+0x24` before the derived constructor installs its vtable.
The anchors are opaque metadata pointers, not wire fields.

All eight cases in `0x70..0x77` test the allocation result, branch on null,
and still reach their body reader after either the constructor path or the
zero-object path; static flow alone does not establish the runtime consequence
of the allocation-failure path. Their reader-argument setup differs by case
and is byte-checked; no no-reader success epilog occurs in this batch.
The final cases `0x78..0x7A` likewise test the allocation result and reach their
body readers on both the constructor and zero-object branches. All three have
reader calls and no no-reader success epilog; the allocation-failure
consequence remains a static-flow observation only.

The vectors in selectors `0x6C` and `0x6D` use one-byte chunk counts. Each
count gives the number of eight-byte records in that chunk; `0xFF` means read
another count, and zero terminates the vector. The parser grows the native
eight-byte-stride vector and no overall record limit is established. The
counted collections in `0x72`, `0x73`, `0x75`, `0x76`, and `0x77` also use
one-byte counts with `0xFF` continuation and zero termination as directly
observed; their per-entry widths and nested layouts are listed above. Selectors
`0x70` and `0x71` consume the entire remaining input as embedded blob data.
Selectors `0x76` and `0x77` include the explicit conditional gates described
above.
The `0x70` and `0x71` constructors initialize their embedded members at
`+0x30` and `+0x28` with `slBlob` vtable VA `0x1C80CC4`. The same vtable
initializes the embedded members for selectors `0x74`–`0x76` at `+0x30`,
`+0x30`, and `+0x28`, respectively. Its virtual `+0x10` target is VA
`0x16CC5D0` / raw `0x12CB9D0`: the method calls resize helper `0x12CBCB0`
with the remaining-byte count and a false preserve flag, then, when the blob
backing handle is non-null, copies that many bytes from the input pointer into
the blob's storage via the `memcpy` thunk. Readers `0x5EF1F0`, `0x5EF090`, and
`0x5EEE60` advance the input pointer and decrement remaining length by that
same count after the virtual call. This establishes raw-byte storage, not the
higher-level meaning or schema of those bytes.

The `0x48..0x4E` body-reader functions are called with the stream in ECX and
the message-object pointer on the stack. Selector `0x4F` instead calls the
DWORD helper with the stream in ECX and the destination pointer
`object+0x28` on the stack. The scalar readers consume 1 or 4 wire bytes as
listed; the `0x5BCFA0` helper consumes one byte and stores its zero-extended
value as a DWORD at `+0x2C`.

The shared `0x12EE170` blob reader consumes a 16-bit little-endian length.
Length zero skips the embedded object's virtual `+0x10` copy method. A
nonzero length is checked against available stream bytes, passed with the
source pointer to that method, and then advances the stream by the payload
length. The embedded object's contents and semantics remain unresolved.
`0x12EE010` reads bytewise through the NUL terminator, using an internal
256-byte temporary chunk and repeating until termination; this is a chunk
size, not a total-string limit. Encoding and malformed-input behavior remain
unresolved. These selectors contain no count-framed item loops; no field
semantics are inferred from the RTTI names.

The cases for `0x40..0x47` push a descriptor-anchor VA, a distinct setup
argument, and `0x1E60904` to `0x3571B0`; that target is a one-byte `ret`
(`nullsub`), so no effect is attributed to the call or its arguments. Each
then allocates the size in the table, writes a separate parser-local DWORD
`0x20..0x27` (role unknown), and calls a constructor whose base helper receives
native type ID `0x40..0x47`. The descriptor anchor is copied to header `+0x0C`;
the common header writes `+0x08=1`, type ID at `+0x10`, parser-input DWORDs at
`+0x18/+0x1C`, and zeros at `+0x20/+0x24`. These parser-local values, constructor
type IDs, and table selectors (`0x40..0x47`) are separate observed values;
their numerical relationship does not establish semantic equivalence. The
derived vtables resolve through section-aware COL/TypeDescriptor reads in the
original image. RTTI class names do not establish the meaning of the wire
fields.

Selectors `0x48..0x4F` follow the same setup-call and allocation sequence, with
distinct per-case setup arguments and descriptor anchors recorded in their
layout classes. They store parser-local DWORDs `0x28..0x2F` separately, call
constructors whose base helpers receive type IDs `0x48..0x4F`, and install
distinct RTTI-resolved vtables. These selectors, parser-local values, metadata
anchors, and constructor type IDs remain separate observed values; their
numerical relationships do not establish semantic meaning. The `0x4F`
allocation-failure path zeroes ESI and then forms the direct DWORD-reader
destination as `[ESI+0x28]`; this records the static path only and does not
assert a runtime consequence.

Selectors `0x50..0x57` also call the one-byte-return setup target, allocate the
sizes in the table, store parser-local DWORD literals `0x30..0x37`, and invoke
constructors whose metadata helpers receive type IDs `0x50..0x57`. The literals,
selectors, constructor type IDs, and descriptor anchors remain separate
observations; their numeric relationships do not establish semantic meaning.
The eight derived vtables resolve through section-aware RTTI and have zero
complete-object offsets. Selector `0x56` uses an extended constructor that
initializes its base metadata, installs the derived vtable, and allocates its
tree sentinel. The allocation-failure routes either enter a local zero-object
block or a shared zero-object continuation before the reader calls; this is
static control flow only and does not assert runtime consequences.

Selectors `0x58..0x5F` use the same setup-call/allocation pattern. Their
parser-local DWORD literals are `0x38..0x3F`; the constructors pass native
type IDs `0x58..0x5F` to the selector-specific metadata helpers and install
distinct RTTI-resolved vtables with zero complete-object offsets.
`TrackRecipe` and `SlotTech` share the `0x8206E3` DWORD-reader continuation;
`ClearTrackedRecipe` has no body reader. The remaining readers consume the
ordered byte/DWORD/QWORD/WORD scalar fields recorded above. The checks verify
each reader's destination address, helper call, primitive width, and cursor
increment. No count loop or field gate occurs in these readers. Allocation
failure for `TrackRecipe` and `SlotTech` joins the shared zero-object
continuation at `0x8206E1`; `ClearTrackedRecipe` returns through the common
zero-return block at `0x8217D9`. Selectors `0x5B..0x5F` use local
zero-object blocks immediately before their readers. These are static paths
only; no runtime consequence is asserted.

Selectors `0x60..0x67` use the same setup-call/allocation pattern. Their
parser-local DWORD literals are `0x40..0x47`; the constructors pass native
type IDs `0x60..0x67` to the selector-specific metadata helpers and install
distinct RTTI-resolved vtables with zero complete-object offsets. Checks pin
the case setup arguments, allocator sizes, constructor/helper calls, metadata
anchors, derived-vtable stores, and reader argument order. Selectors
`0x60..0x65` and `0x67` continue through their readers after a local
zero-object block if allocation fails. Selector `0x66` instead branches to
the shared zero-object block at `0x81F72C` on failure and jumps to the shared
QWORD-reader continuation at `0x81F72E` after successful construction. These
are static control-flow observations only; no runtime consequence is
asserted.

Binary fingerprints include table indexes and targets, case setup arguments,
allocation sizes and failure blocks, constructor type/metadata/vtable order,
reader field arguments, primitive-reader calls, byte-count consumption,
native vector increments, and the count-loop continuation and termination
branches. They cover selector `0x36`'s conditional tail and its `0x70` native
stride as well as selector `0x43`'s byte count, DWORD-plus-byte items, and
eight-byte stride, plus the `0x48..0x4F` scalar/embedded-object/text-reader
argument order, byte/DWORD/WORD widths, zero-length blob gate, and shared-text
NUL/chunk loop. The `0x50..0x57` checks pin the table targets and RTTI,
allocation-failure branches, constructor/header stores, all reader field
arguments, the five-/16-/four-byte counted record shapes, native strides, and
the `0x56` tree-sentinel word initializer and count loop. The `0x58..0x5F`
checks pin the new targets/RTTI, allocation and failure paths, metadata
initializers, scalar field order, and primitive load/store widths. These checks
assert wire framing only; unresolved field, gate, count, predicate, and tag
meanings remain unassigned. The `0x60..0x67` fingerprints additionally cover
the four-vector and variable-record readers, the `0xFF` count continuations,
the shared `0x66` reader path, embedded-blob remainder copies, and the `0x64`
tag-dispatch table and conditional payload paths.

On allocation failure, selectors `0x20..0x25`, `0x27..0x2C`, and `0x2E..0x2F`
set the object register to zero but still fall through to their body-reader
call. Selector `0x2D` branches through selector `0x28`'s zero-object block and
shared reader continuation. Selector `0x26` instead branches directly to the
shared zero-return block. Selector `0x31` also branches directly to the shared
zero-return block; selectors through `0x4E` continue through their readers with
the object register set to zero on allocation failure. Selector `0x4F` instead
continues to its direct DWORD helper after forming the destination from the
zero object register. Selectors `0x50`, `0x53`, `0x54`, `0x56`, and `0x57`
enter local zero-object blocks; `0x51`, `0x52`, and `0x55` reuse the zero-object
continuations at `0x81F72C`, `0x8206E1`, and `0x81FB25`, respectively, before
their readers. These are static control-flow observations only; their
runtime consequence is not asserted.
Selectors `0x58` and `0x5A` also branch to the shared zero-object continuation
at `0x8206E1` before the DWORD reader; `0x59` branches to the common zero-return
block at `0x8217D9`. Selectors `0x5B..0x5F` each branch to a local
zero-object block before their scalar-reader call. These are static
control-flow observations only; their runtime consequence is not asserted.
Selectors `0x60..0x65` and `0x67` likewise branch to local zero-object blocks
before their readers; selector `0x66` branches to `0x81F72C`, then reaches the
shared reader at `0x81F72E`. These are static control-flow observations only;
their runtime consequence is not asserted.
Selectors `0x32` and `0x34` share the zero-object continuation at raw
`0x81FB25` and the text-reader continuation at `0x81FB27`. These are static
control-flow observations only; their runtime consequence is not asserted.

### Route index 2, dispatcher value `0x53`: `AreaVisualData::LootObjectCollected`

The route2 block `0x81EE63` subtracts `0x53` from its dispatcher value; equality
branches to case `0x81EF1C`. That case requests `0x40` bytes from allocator
`0x12B7FB0`, stores local DWORD `0xCD` (role unknown), calls constructor
`0xADBAA0`, then body reader `0x83AD90`. The constructor passes `0x53` to helper
`0xB07660` and installs vtable VA `0x1C723C0`, whose RTTI is
`firefall::AreaVisualData::LootObjectCollected`.

The body reader writes a DWORD at object+`0x28`, followed by raw QWORDs at
`+0x30` and `+0x38`, using readers `0x3370A0` and `0x356F80`. Field meanings
remain unknown.

#### Conditional post-parse consumer for route 2

The route selector in `0x81CC50` maps leading bytes `0x13..0x19` to route index
2 and parser block `0x81EE63`; its `0x53` comparison selects the case at
`0x81EF1C`. This is a parser-family choice, not the later receiver selector.
The constructor's base helper writes the metadata pointer at object+`0x0C`
(`0x1BB0224`, whose string is `msg-83`), discriminator `0x53` at `+0x10`, and
the two envelope-key DWORDs at `+0x18/+0x1C`. The parser's local `0xCD` is at
`[esp+0x20]`, not the object discriminator. After the body reader, `0x81EF58`
restores EAX from ESI, preserving the constructed object pointer (or null on
the allocation-failure path).

That result is preserved by both direct parser callers: `0x821BA0` calls
`0x81CC50` at `0x821D87`, and its epilog does not overwrite EAX; wrapper
`0x821DE0` similarly calls at `0x821E1E` and returns EAX unchanged. In the
registered ReplayReader path, the indirect parser call at `0xBD6D3C` moves its
EAX result into EDI at `0xBD6D41`, stores that same value in the queued
12-byte result record at `0xBD6D63`, then calls the queue helper at `0xBD6D66`.
The queue-consumer and callback-context gates described in “Parser result path
and dialog-message primary vftables” remain conditional; this trace does not
assert that every parsed route-2 object reaches application dispatch.

On the callback-context `+8` branch, `0x7FDC60` first requires the low byte of
message `+0x18` to be zero. It then diverts a message whose `+0x10` equals the
nested-message sentinel; otherwise it requires a non-null context `+8` and
calls `0x81BB90` with the original message pointer. `0x81BB90` reads only
message `+0x10`, subtracts `0x20`, and selects the table entry at VA `0xC1D6DC`
(raw `0x81CADC`). Discriminator `0x53` reaches VA `0xC1D001` (raw `0x81C401`),
which makes the virtual call at `0x81C420` through receiver slot `+0xD0`,
passing the same message pointer. The concrete receiver was established from
its constructor/vtable bytes and RTTI chain, not inferred from message RTTI:
the `tfGameProtocolHandler` vtable at VA `0x1C71EC0` has VA `0xECB180`
(`0xACA580` raw `.text`) in slot `+0xD0`. The receiver method therefore is
`0xACA580`; the `0x81BB90` table does not branch on vptr or `+0x0C`.

The handler uses message `+0x34` as a candidate C-string pointer, falling back
to the empty string at VA `0x1C5EA68` when null, and compares it bytewise with
`"missions"` at VA `0x1BAE9E0`. On the equal branch it gets owner state through
VA `0x89E8C0` (raw `0x49DCC0`), resets the end pointer of the vector at
`[owner+0x43B8]+0x18` to its begin pointer, and calls VA `0xCF0D80` (raw
`0x8F0180`) with message `+0x28/+0x2C` as the source range. That helper's copy
routine VA `0xCF26D0` (raw `0x8F1AD0`) copies 24-byte records and stores the
new end pointer at vector `+4`. This is an observed conditional state update;
the record semantics and runtime range validity are not established. The
non-equal comparison branch instead calls VA `0xD38A40` (raw `0x937E40`) with
the address of message `+0x28`.

Boundary remains unresolved: the route-2 reader writes the DWORD at `+0x28`
and QWORDs at `+0x30/+0x38`; the derived constructor zeros `+0x30..+0x3C`,
but neither writes `+0x2C`. The receiver uses `+0x2C` as a range endpoint.
Also, `+0x34` is part of the parsed QWORD at `+0x30`, but the handler treats
that DWORD as a possible C-string pointer. No semantic names are assigned to
these fields. Distinct parser families with discriminator `0x53` retain
different `+0x0C` metadata pointers and vptrs, while this callback selector
collapses them onto the same `+0xD0` receiver method. Read-only IDA
disassembly/byte reads and the recorded binary-evidence checks agree with the
requested original executable at the relevant code sites; the IDB and sample
were not modified or executed.

Value `0x54` selects case `0x81EECB` (`AreaVisualData::AudioEmitterSpawned`).
It allocates `0x40` bytes, stores local DWORD `0xCE`, calls constructor
`0xADBB40`, then reader `0x83ACA0`. The constructor passes `0x54` to base
helper `0xB076D0` and installs vtable VA `0x1C72358` (RTTI
`firefall::AreaVisualData::AudioEmitterSpawned`). Its reader writes a QWORD at
`+0x28`, DWORDs at `+0x30/+0x34/+0x38`, and a WORD at `+0x3C`; helper readers
are `0x356850`, `0x3568E0`, and `0x336AC0`.

Value `0x55` selects case `0x81EE79` (`AreaVisualData::ParticleEffectSpawned`).
It allocates `0x70` bytes, stores local DWORD `0xCF`, calls constructor
`0xADBBC0`, then reader `0x83AAA0`. The constructor passes `0x55` to base
helper `0xB07740`, installs vtable VA `0x1C72368` (RTTI
`firefall::AreaVisualData::ParticleEffectSpawned`), and directly zeroes
DWORDs at `+0x28/+0x2C/+0x30`, bytes at `+0x40/+0x58/+0x64`, a DWORD at
`+0x5C`, and a WORD at `+0x60`. The reader processes, in order, a QWORD at
`+0x28`, DWORDs at `+0x30/+0x34/+0x38/+0x3C`, a conditional record at `+0x40`
(one input presence byte and, when nonzero, three DWORDs at `+0x44/+0x48/+0x4C`),
four WORDs at `+0x50..+0x56`, a byte at `+0x58`, a DWORD at `+0x5C`, a WORD
at `+0x60`, and a second conditional record at `+0x64` (one input presence
byte and, when nonzero, DWORDs at `+0x68/+0x6C`). Helpers `0x5C0270` and
`0x83A9C0` implement the conditional records; their present values are copied
into native fields, while an absent input clears only the presence byte.
No field meanings are inferred from the RTTI names.

### Route index 3, switch entry 0: `Vehicle::AbilityActivated`

The route3 block `0x81EF6D` computes `arg_0 - 0x53`: instruction bytes
`83 C0 AD` sign-extend the encoded immediate `0xAD` to `-0x53`. It bounds the
result to indices `0..8` before indexing the nine-entry table at VA `0xC2260C` (raw
`0x821A0C`). Entry0 reaches case `0x81EF83`, which allocates `0x30` bytes
and writes stack local `[esp+0x20]=0xD0` at raw `0x81EF91` (role unknown; not
the message discriminator). It calls constructor `0xB093C0`, then reader
`0x83A840`. The constructor passes `0x53` to helper `0xB474B0` and
installs vtable VA `0x1C72998` (RTTI `firefall::Vehicle::AbilityActivated`).

The reader writes DWORDs at object+`0x28` and `+0x2C` through `0x3370A0`.
Their meanings remain unknown.

#### Conditional post-parse consumer for route 3

The base constructor stores discriminator `0x53` at message+`0x10`, just as
route 2 does. The callback path is conditional: message+`0x18` must have a
zero low byte, message+`0x10` must not be the nested-message sentinel, and the
callback context's `+8` receiver must be non-null before `0x7FDC60` calls
`0x81BB90` with this object. The receiver dispatcher selects solely on
message+`0x10`; it does not check the message vptr or metadata pointer.
Consequently, discriminator `0x53` reaches the same receiver slot `+0xD0` and
method `0xACA580` described for route 2.

That method reads message+`0x34` as a candidate C-string pointer and compares
it to `"missions"`. On equality, it uses message+`0x28/+0x2C` as a source
range for the vector at owner+`0x43B8`. On inequality, it passes the address
of message+`0x28` to the map helper at `0x937E40`, which walks records in
24-byte steps until the supplied end pointer. The two DWORDs parsed by
`Vehicle::AbilityActivated` therefore serve as that helper's range endpoints
on this path, but their semantics are not otherwise named.

There is a static extent mismatch: this parser requests `0x30` bytes and reads
only through `+0x2C`, while the shared method loads a pointer from `+0x34`.
That address lies beyond the recorded allocation extent. This establishes a
potential out-of-extent read only if a route-3 object reaches this conditional
handler path; the IDA evidence does not establish that it happens at runtime,
the validity of the loaded pointer, or the meaning/range validity of
`+0x28/+0x2C`. Read-only disassembly and original-file fingerprints support
the parser, callback gate, discriminator dispatch, and handler offsets; no
runtime behavior is inferred.

Switch entry1 (dispatcher value `0x54`) selects case `0x81EFD4`,
`Vehicle::AbilityFailed`. It allocates `0x30` bytes, stores local DWORD
`0xD1`, calls constructor `0xB09440`, then reader `0x83A7A0`. The constructor
passes `0x54` to base helper `0xB47520` and installs vtable VA `0x1C72978`
(RTTI `firefall::Vehicle::AbilityFailed`). The reader writes DWORDs at
`+0x28/+0x2C` through `0x3370A0`.

#### Switch entry2 (`0x55`): `Vehicle::PublicCombatLog`

Entry2 selects case `0x81F025`, allocates `0x40` bytes, stores local DWORD
`0xD2`, and calls constructor `0xB094C0`. Its success path joins the shared
reader tail at `0x81CF5B`, which calls body reader `0x5F3350` at `0x81CF67`.
The constructor passes `0x55` to base helper `0xB47590`, installs vtable VA
`0x1C72988` (RTTI `firefall::Vehicle::PublicCombatLog`), zeroes the byte at
`+0x28`, seeds DWORD `+0x2C` with `0x1C80CC4`, and zeroes DWORDs `+0x30`,
`+0x34`, and `+0x38`.

The body reader consumes a DWORD at `+0x28`. Only value `1` continues to
`0x12EE170`, which reads a little-endian WORD length and passes that span to
the embedded blob object at `+0x2C` through its virtual `+0x10` method,
rather than copying payload bytes inline at that offset. A zero length
performs no copy. Other values take the reader's
error path. The gate's application meaning and blob contents remain unknown.

#### Switch entry3 (`0x56`): `Vehicle::CurrentPoseUpdate`

Entry3 selects `0x81F056`, allocates `0x58` bytes, stores local DWORD `0xD3`,
and calls constructor `0xB095B0`; the body reader `0x83A530` is called at
`0x81F08E`. The constructor passes `0x56` to `0xB47600` and installs vtable VA
`0x1C72958` (RTTI `firefall::Vehicle::CurrentPoseUpdate`).

The reader begins at object `+0x28` with a flag byte:

1. Bit `0x01` selects a byte at `+0x29`; otherwise it reads a WORD at `+0x2A`.
2. It always reads a byte at `+0x2C`. If bit `0x02` is clear, it reads a WORD
   at `+0x2E`.
3. If any bit in `0x0C` is set, it zeroes DWORD `+0x30` and copies three raw
   bytes there; otherwise it reads DWORDs `+0x34`, `+0x38`, and `+0x3C`.
4. For `flags & 0x30 == 0`, it reads four WORDs at `+0x44..+0x4A`. The exact
   value `0x10` selects one byte at `+0x40`; `0x20` selects bytes `+0x41..+0x43`;
   `0x30` reads none of these union fields.
5. For `flags & 0xC0 == 0`, it reads WORDs at `+0x50`, `+0x52`, and `+0x54`.
   If the masked value is `0x40` or `0x80`, it zeroes DWORD `+0x4C` and copies
   three bytes there; `0xC0` reads no final payload.

These are observed layout and branch conditions only; the flag and field
meanings are unresolved.

#### Switch entry4 (`0x57`): `Vehicle::TookDebugWeaponHitPublic`

Entry4 selects `0x81F0A8`, allocates `0xA8` bytes, stores local DWORD `0xD4`,
and calls constructor `0xB09630`. It joins the shared body-reader tail at
`0x81CD94` (`0x81CDA0` calls `0x5F60F0`). The constructor passes `0x57` to
`0xB47680`, installs vtable VA `0x1C72968` (RTTI
`firefall::Vehicle::TookDebugWeaponHitPublic`), and zeroes bytes `+0x58` and
`+0x88`.

The reader consumes a byte at `+0x28`, DWORDs at `+0x2C/+0x30`, a WORD at
`+0x34`, six DWORDs at `+0x38..+0x4C`, and another DWORD at `+0x50`.
`0x5F4FD0` then reads a presence byte for state at `+0x58`; when present,
`0x5F4D60` reads a QWORD, three DWORDs, four values through the shared
four-value reader, and two bytes into a temporary record before `0x5F4F60`
updates the object. When absent, it clears the existing presence byte.

`0x5F5FB0` reads a second presence byte at `+0x88`. When set, it reads a raw
QWORD, then `0x5F5420` consumes a sequence of counted, 64-byte records using
`0x6087E0`; `0x5F5EB0` applies that opaque state. The absent path clears any
existing state. The embedded record and field semantics are unresolved.

#### Switch entry5 (`0x58`): `Vehicle::ForcedMovement`

Entry5 selects `0x81F0DC`, allocates `0x1E0` bytes, stores local DWORD `0xD5`,
and calls constructor `0xB09710`. The constructor passes `0x58` to `0xB476F0`,
installs vtable VA `0x1C72938` (RTTI
`firefall::Vehicle::ForcedMovement`), then initializes the embedded `0x1B8`
state at `+0x28` through `0x92B8A0`. Relative to that embedded state, the
initializer sets DWORDs at `+0/+0x08/+0xEC/+0xF0/+0x110/+0x114/+0x190..+0x19C`
and WORDs at `+0x0C/+0xE8/+0x1A4` to zero, sets `+0x6C/+0xD8/+0x174` WORDs
to `0x0101`, and sets `+0x04` to its zero argument.

The body reader `0x92F800` is called at `0x81D62F`. It reads a selector byte
into embedded `+0x08`, a DWORD into `+0x04`, then calls `0x5FDA00` for an
optional QWORD. The QWORD halves, when present, are copied to embedded
`+0x110/+0x114`; otherwise those slots are initialized from data at VA
`0x1E68530`. The selector is bounds-checked to `0..13`, then indexes the
14-entry table at VA `0xD308BC` (raw `0x92FCBC`). The reader also clears
embedded byte `+0x0D` on its common return. No case names or state semantics
are inferred; each case below records only its observed reads:

| State case | Reader target (raw) | Additional reads, relative to embedded state, in order |
|---:|---:|---|
| 0 | `0x92FC83` | None |
| 1 | `0x92F8E8` | Three DWORDs at `+0x118`, `+0x184`, `+0x178`; DWORD `+0x190`; derives `+0x194 = +0x190 + 1` |
| 2 | `0x92F92E` | Three DWORDs `+0x118`, three DWORDs `+0x178`, DWORDs `+0x190/+0x194/+0x19C`, bytes `+0x1A4/+0x1A5` |
| 3 | `0x92F990` | DWORDs `+0x190/+0x194`; three DWORDs `+0x10`; 16 bytes at `+0x1C` then constant WORD `1` at `+0x2C`; three DWORDs `+0x118`; 16 bytes at `+0x124` then constant WORD `1` at `+0x134`; DWORDs `+0x198/+0x19C`; bytes `+0x1A4/+0x1A5` |
| 4 | `0x92FA16` | Three DWORDs `+0x118`; DWORDs `+0x190/+0x194/+0x19C`; three DWORDs `+0x184` |
| 5 | `0x92FA5E` | Three DWORDs `+0x178`; DWORDs `+0x190/+0x194`; byte `+0x0C` |
| 6 | `0x92FA94` | Three DWORDs `+0x178`; DWORDs `+0x190/+0x194/+0x19C` |
| 7 | `0x92FBCF` | DWORDs `+0x190/+0x194` |
| 8 | `0x92FAC6` | Three DWORDs `+0x118`; DWORDs `+0x190/+0x194/+0x198` |
| 9 | `0x92FAF8` | DWORDs `+0x190/+0x194/+0x198`; three DWORDs `+0x178` |
| 10 | `0x92FB32` | DWORDs `+0x190/+0x194`; three DWORDs `+0x178`; DWORDs `+0x198/+0x19C`; byte `+0x1A5` |
| 11 | `0x92FB87` | DWORDs `+0x190/+0x194`; three DWORDs `+0x118`; 16 bytes at `+0x124` then constant WORD `1` at `+0x134` |
| 12 | `0x92FBC1` | Three DWORDs `+0x118`; DWORDs `+0x190/+0x194` |
| 13 | `0x92FBED` | DWORDs `+0x190/+0x194`; QWORD `+0x110`; three DWORDs `+0x118`; DWORDs `+0x1A8/+0x1AC/+0x19C/+0x198`; byte `+0x1A4`; DWORDs `+0x1A0/+0x1B0` |

`0x384C80` reads three DWORDs; `0x384C20` reads four DWORDs and writes its
following WORD marker. Its shared `0x384B40` reader treats the four values as
floats and applies a numeric range check. The discriminator variants remain
opaque despite this structural mapping.

#### Switch entry6 (`0x59`): `Vehicle::ForcedMovementCancelled`

Entry6 selects case `0x81F110`, `Vehicle::ForcedMovementCancelled`. It
allocates `0x30` bytes, stores local DWORD `0xD6`, calls constructor
`0xB097A0`, then reader `0x83A440`. The constructor passes `0x59` to base
helper `0xB47760` and installs vtable VA `0x1C72928` (RTTI
`firefall::Vehicle::ForcedMovementCancelled`). The reader writes a DWORD at
`+0x28` and a WORD at `+0x2C`, through `0x3370A0` and `0x336AC0`.

#### Switch entry7 (`0x5A`): `Vehicle::FlipPunch`

Entry7 selects `0x81F161`, allocates `0x40` bytes, stores local DWORD `0xD7`,
and calls constructor `0xB09820`; body reader `0x83A370` is called at
`0x81F198`. The constructor passes `0x5A` to `0xB477D0` and installs vtable
VA `0x1C72948` (RTTI `firefall::Vehicle::FlipPunch`). The reader consumes six
DWORDs, in order, at `+0x28/+0x2C/+0x30/+0x34/+0x38/+0x3C`. Their meanings
remain unknown.

#### Switch entry8 (`0x5B`): `Vehicle::DebugMovementUpdate`

Entry8 selects `0x81F1B2`, allocates `0xA0` bytes, stores local DWORD `0xD8`,
and calls constructor `0xB098A0`. The constructor passes `0x5B` to
`0xB47840`, installs vtable VA `0x1C728F8` (RTTI
`firefall::Vehicle::DebugMovementUpdate`), and seeds WORD `+0x88` with
`0x0101`.

Reader `0x83A230` reads a byte at `+0x28`, then delegates at `+0x2C` to
`0x608370`. That helper reads DWORDs `+0x2C/+0x30/+0x34`, reads four values
through `0x384B40` and copies their 16 bytes from temporary storage to
`+0x78..+0x87`, writes WORD `1` at `+0x88`, and reads DWORDs
`+0x8C/+0x90/+0x94`. The outer reader then reads WORD `+0x98` and DWORD
`+0x9C`. Field meanings remain unknown.

### Route index 4, dispatcher value `0x53`: `Deployable::TookHit`

The route4 block `0x81F206` subtracts `0x53` and branches to case `0x81F29E`
on equality. The case allocates `0x60` bytes, stores local DWORD `0xD9` (role
unknown), calls constructor `0xB27E80`, then reader `0x83A120`. The constructor
passes `0x53` to helper `0xB47C40`, installs vtable VA `0x1C727A8` (RTTI
`firefall::Deployable::TookHit`), and zeroes byte `+0x28` and WORD `+0x50`.

The body reader calls conditional-record reader `0x5F0CF0` for object+`0x28`,
reads bytes at `+0x50` and `+0x51`, a WORD at `+0x58`, then a byte at `+0x5A`.
The conditional record's internal field meanings and all outer field meanings
remain unknown.

#### Conditional post-parse consumer for route 4

The constructor stores discriminator `0x53` at message+`0x10`, so this
message shares route 2 and route 3's callback selector. Subject to the same
callback gates described above, `0x81BB90` sends it through receiver slot
`+0xD0` to `0xACA580`, without checking message RTTI or vptr. That method
reads message+`0x34` as a candidate C-string pointer. For this message,
`+0x34` is also the high DWORD of the QWORD read into the conditional record
at `+0x30/+0x34` when the record's presence byte at `+0x28` is nonzero. The
record helper also conditionally stores a nested QWORD at `+0x40/+0x44`,
then a DWORD at `+0x48` and byte at `+0x4C`. If the outer record is absent,
the parser clears its presence byte (and clears the nested-presence byte only
when already set); it does not clear the QWORD at `+0x30/+0x34`. The
constructor likewise initializes `+0x28` and `+0x50`, not the QWORD.

The same handler uses message `+0x28/+0x2C` as source-range endpoints for
either the `"missions"` vector branch or the map-update branch. Here `+0x28`
is the conditional record's presence byte, and the parser does not initialize
`+0x2C`; neither is identified as a range by the route-4 body reader. These
are static field overlaps between different parser/consumer views, not proof
that route 4 reaches the handler or that the candidate pointer/range is valid.
Unlike route 3, the `+0x34` read is within this class's `0x60`-byte allocation,
but its value is conditionally populated as part of a raw QWORD. Keep the
field's semantic interpretation unresolved.

Value `0x54` selects case `0x81F24D`, `Deployable::AbilityProjectileFired`.
It allocates `0x68` bytes, stores local DWORD `0xDA`, calls constructor
`0xB27F40`, then reader `0x839F50`. The constructor passes `0x54` to base
helper `0xB47CB0`, installs vtable VA `0x1C72764` (RTTI
`firefall::Deployable::AbilityProjectileFired`), and directly zeroes byte
`+0x58`. The reader writes eight WORDs at `+0x28..+0x36`, DWORDs at
`+0x38/+0x3C`, byte `+0x40`, DWORD `+0x44`, byte `+0x48`, DWORDs at
`+0x4C/+0x50`, then an optional QWORD record at `+0x58`: a presence byte
followed, when nonzero, by QWORD `+0x60`. Reader helpers are `0x336AC0`,
`0x34B740`, `0x6097B0`, `0x3568E0`, `0x353310`, `0x337180`,
`0x3370A0`, and `0x5FDA00` (which uses `0x356F80` for the QWORD).

Value `0x55` selects case `0x81F21C`, `Deployable::PublicCombatLog`. It
allocates `0x40` bytes, stores local DWORD `0xDB`, calls constructor
`0xB27FE0`, then joins the shared reader tail at `0x81CF5B`, which calls
`0x5F3350` with `this = object+0x28`. The constructor passes `0x55` to base
helper `0xB47D20`, installs vtable VA `0x1C72754` (RTTI
`firefall::Deployable::PublicCombatLog`), zeroes byte `+0x28`, and initializes
an embedded blob object at `+0x2C` with vtable VA `0x1C80CC4` and three zero
DWORDs at `+0x30/+0x34/+0x38`. The reader first consumes a DWORD and requires
it to equal `1`; the nonmatching path reaches an error stub. On the matching
path, `0x12EE170` reads a WORD byte-count followed by that many bytes and
passes the span to the embedded blob's indirect vtable slot `+0x10`, then
advances the input cursor. Blob semantics and the indirect destination remain
unresolved; RTTI alone does not establish combat-log meaning.

### Route index 5, dispatcher value `0x53`: `Turret::WeaponProjectileFired`

The route5 block `0x81F2EF` accepts dispatcher value `0x53` and enters case
`0x81F2F9`. It allocates `0x48` bytes, stores local DWORD `0xDC` (role
unknown), calls constructor `0xB36520`, then body reader `0x839EB0`. The
constructor passes `0x53` to helper `0xB47D90`, installs vtable VA `0x1C726E8`
(RTTI `firefall::Turret::WeaponProjectileFired`), and zeroes byte `+0x38`.

The body reader uses `0x3370A0` at `+0x28`, `0x3568E0` at `+0x2C`, `+0x30`,
and `+0x34`, then helper `0x5C0270` at `+0x38`. Field meanings and the role of
the final helper remain unresolved.

#### Conditional post-parse consumer for route 5

The constructor stores discriminator `0x53` at message+`0x10`, so under the
same callback gates route 5 also reaches the shared slot `+0xD0` method
`0xACA580`. It loads message+`0x34` as a candidate C-string pointer. In this
message layout that field is populated by the third `0x3568E0` DWORD reader,
not by a text reader; the constructor only zeroes the byte at `+0x38`.

The shared method then interprets message `+0x28/+0x2C` as a source range for
its vector or map update. Route 5 parses a DWORD at `+0x28` and another DWORD
at `+0x2C`; the parser does not establish that these values are valid range
endpoints or that the DWORD at `+0x34` is a valid C-string pointer. All three
fields are within the `0x48`-byte allocation. This is a static collision
between parser and consumer interpretations, conditional on the callback
gates; it does not establish route-5 runtime delivery or field semantics.

### Route index 6, dispatcher value `0x53`: `LootStoreExtension::LootObjectCollected`

The route6 block `0x81F34A` accepts dispatcher value `0x53` and enters case
`0x81F354`. It allocates `0x40` bytes, stores local DWORD `0xDD` (role
unknown), calls constructor `0xB43A70`, then body reader `0x839DB0`. The
constructor passes `0x53` to helper `0xB48040`, installs vtable VA `0x1C72608`
(RTTI `firefall::LootStoreExtension::LootObjectCollected`), and initializes
DWORDs at `+0x30`, `+0x34`, `+0x38`, and `+0x3C` to zero.

The body reader writes a DWORD at object+`0x28`, then raw QWORDs at `+0x30` and
`+0x38`, through `0x3370A0` and `0x356F80`. Field meanings remain unknown.

#### Conditional post-parse consumer for route 6

The constructor stores discriminator `0x53` at message+`0x10`, so the same
callback gates and receiver dispatch can route this message through slot
`+0xD0` to `0xACA580`. That method treats message+`0x34` as a candidate
C-string pointer; here it is the high DWORD of the first QWORD read at
`+0x30/+0x34`. The constructor initially zeros that QWORD, but the body reader
overwrites it from the stream.

The handler also consumes message `+0x28/+0x2C` as source-range endpoints.
Route 6 reads a DWORD at `+0x28` and leaves `+0x2C` untouched in the traced
constructor and body reader; these are not established as valid range
endpoints. Both the candidate field and range operands are within the
`0x40`-byte allocation. The consumer path remains conditional, and neither
runtime reachability nor field semantics are established.

Nested-table entry0 maps selector `0x53`: route0 block `0x81CCAD` dispatches it
through the table at VA `0xC22444` (raw `0x821844`) to raw case
`0x81CCC3`. That case allocates `0x48` bytes, calls constructor `0x524C30`,
then body reader `0x5F6B10`. Constructor RTTI identifies the object as
`firefall::Character::MarketRequestComplete`. The reader stores, in order,
a byte at object+`0x28`, raw QWORD at+`0x30`, byte at+`0x38`, and three
NUL-terminated shared-text values at+`0x3C/+0x40/+0x44`. Field meanings
remain unknown; this case's RTTI name does not identify the remaining table cases.
`MatrixSecondaryCommandDispatcherNativeLayout.cs` and
`CharacterMarketRequestCompleteNativeLayout` preserve the route bounds and
this bounded object layout without implementing a parser.

#### Conditional post-parse consumer for nested selector 0x53

The route0 constructor calls base helper `0x5A0400` with `0x53`, which stores
that discriminator at message+`0x10`. It therefore shares the same conditional
callback gates and receiver selection as the other `0x53` message families:
when message+`0x18` has a zero low byte, `+0x10` is not the nested sentinel,
and callback context `+8` is non-null, `0x81BB90` dispatches to receiver slot
`+0xD0` / method `0xACA580`. This table is selected by the stored discriminator,
not the route0 nested selector or the message RTTI.

The shared method treats message+`0x34` as a candidate C-string pointer. For
this type, it is the high DWORD of the raw QWORD parsed at `+0x30/+0x34`; the
reader does not parse that DWORD as text. The method also uses `+0x28/+0x2C`
as source-range endpoints, whereas the route0 reader writes a single byte at
`+0x28` and does not initialize `+0x2C` in the traced constructor/reader.
The discriminator match alone does not establish that this message reaches
the handler or that either candidate pointer/range is valid. These are
conditional static field overlaps; do not infer runtime behavior or assign
semantic names to the raw fields.

The same table's entry1 maps selector `0x54` to VA `0xC1D914` / raw case
`0x81CD14`. It allocates `0x40` bytes, calls constructor `0x524DF0`, then
body reader `0x5F68D0`. The constructor installs vtable VA `0x1C67268`
(RTTI `firefall::Character::ReceiveWeaponTweaks`) and initializes an embedded
blob object at `+0x2C` with vtable VA `0x1C80CC4`. The reader processes two
raw bytes into object `+0x28` and `+0x29`, then passes the current stream
cursor and all remaining bytes to the blob object's vtable slot `+0x10`.
That slot resolves to raw `0x12CB9D0`, which resizes the blob to the supplied
length with preservation disabled and copies those bytes; the reader advances
the cursor and subtracts that length from the remaining count. The body
therefore has two observed byte fields followed by a tail blob, without a
separate blob-length prefix. The case also stores local DWORD `0x5C`, whose
role is unknown. The two byte meanings and blob contents remain unresolved;
RTTI alone does not establish weapon semantics. Its body reader attempts a
stream refill when a byte read sees zero remaining, but the exact malformed
short-input behavior is not established.
`CharacterReceiveWeaponTweaksNativeLayout` records the offsets and call
targets without implementing a parser.

#### Conditional post-parse consumer for nested selector 0x54

The shared discriminator table maps `0x54` to switch target VA `0xC1D02C` /
raw `0x81C42C`, which calls receiver slot `+0xD4` at raw `0x81C44B`.
The `tfGameProtocolHandler` vtable entry points to VA `0xECB3E0` / raw method
`0xACA7E0`. Under that conditional routing, the method builds an event payload
with labels `contentType`, `contentId`, and `duration`, then invokes the event
sink's vtable slot `+0xAC` with event code `130`.

The `contentType` helper treats message `+0x28` as a shared-text pointer.
The other two helpers record DWORD values from `+0x2C` and `+0x30`; the payload
builder reserves eight 8-byte entries. These source interpretations overlap
the parser layout: `+0x28/+0x29` are two bytes, `+0x2C` starts the embedded
blob object, and `+0x30` is the first DWORD in that object's state. This is a
static parser/consumer interpretation conflict, not proof the message reaches
this handler or that the event is emitted at runtime. The labels do not resolve
the underlying field meanings. The handler method's target and label strings
are checked against the original executable by `Startup.Checks`.

### Selector 0x55: `TookDebugWeaponHitPublic`

Entry2 of the same table maps selector `0x55` to VA `0xC1D965` / raw case
`0x81CD65`. It allocates `0xA8` bytes, stores local DWORD `0x5D` (role
unknown), then calls constructor `0x524EE0` and reader `0x5F60F0`. The
constructor passes `0x55` to `0x5A04E0`, installs vtable VA `0x1C67258`
(RTTI `firefall::Character::TookDebugWeaponHitPublic`), and zeros object
bytes `+0x58` and `+0x88`. The RTTI label does not establish field semantics.

The reader's fixed scalar prefix is listed in **input order** below. Input
positions are nominal offsets from the reader's initial cursor, assuming the
individual helpers obtain the requested bytes:

| Input offset | Width | Object offset | Observation |
| ---: | ---: | ---: | --- |
| `0x00` | 1 byte | `+0x00` | Raw byte |
| `0x01` | DWORD | `+0x04` | Raw DWORD |
| `0x05` | DWORD | `+0x08` | Raw DWORD |
| `0x09` | WORD | `+0x0C` | Raw WORD |
| `0x0B`, `0x0F`, `0x13`, `0x17`, `0x1B`, `0x1F` | DWORD each | `+0x10..+0x24` | Six raw DWORDs |
| `0x23` | DWORD | `+0x28` | Raw DWORD |

This is nominally `0x27` input bytes before the first conditional tail.
Tail `+0x30` first consumes a gate byte. If nonzero, `0x5F4D60` reads
`0x26` payload bytes into a temporary record: QWORD at temporary `+0`,
three DWORDs at `+8/+0xC/+0x10`, four DWORD bit patterns at `+0x14..+0x20`
that `0x384B40` subsequently uses as floats, then two bytes at `+0x24/+0x25`.
The record is copied into object `+0x38..+0x5D`; the object marker at `+0x30`
is set to one when its prior value is zero. If the gate is zero, a nonzero
object marker is cleared without an observed clear of the rest of the record.
Thus, on this branch the tail consumes nominally `1 + 0x26` bytes. The
constructor's zero at object `+0x58` lies inside the four-float byte range
`+0x4C..+0x5B`, which this reader can overwrite.

Tail `+0x60` then consumes another gate byte. On its nonzero branch, it reads
a QWORD and enters `0x5F5420`. That routine reads a count byte and parses
that many records, each exactly `0x40` bytes (`0x6087E0` reads sixteen
DWORDs per record). A count of `0xFF` causes another count byte and record
block to be read; any other count ends the sequence, with zero representing
no records. The temporary vector is copied into the object beginning at
`+0x74` (begin/end/capacity at `+0x74/+0x78/+0x7C`); the associated marker is
at `+0x60`. A zero gate releases the current vector when the marker is
nonzero, clears those three DWORDs, and clears the marker.

The nonzero `+0x60` path supplies a 12-byte local prefix to `0x5F5EB0`, but its
QWORD reader writes only the first eight bytes. At raw `0x5F6032..0x5F6042`,
the caller zeroes a separate three-DWORD vector temporary at `[esp+0x20]`;
the QWORD scratch begins at `[esp+0x14]` (`0x5F604A`) and is not included in
those stores. `0x356F80` writes two DWORDs and advances the stream by eight
bytes (`0x356FA5..0x356FB5`). The same scratch address is passed to
`0x5F5EB0` at `0x5F6069..0x5F6070`. That helper copies source DWORDs at
`+0/+4/+8` to object `+0x68/+0x6C/+0x70` (`0x5F5EE9/0x5F5EEF/0x5F5EF5`);
therefore its third input DWORD, the local at `[esp+0x1C]`, is not explicitly
initialized or populated by the QWORD read before being copied to object
`+0x70`. It is not a third wire DWORD. The reader's constructor only zeros
object `+0x58` and `+0x88`; it does not initialize `+0x70`. This establishes
the local data-flow discrepancy, not its runtime consequence. The receiver
uses only message `+0x28`, so no traced consumer explains the copied value.
Refill and short-input behavior of the scalar helpers is also not established.
`CharacterTookDebugWeaponHitPublicNativeLayout` preserves the observed
offsets, widths, branch boundaries, and record size without implementing a
parser or naming semantic fields.

#### Conditional post-parse consumer for nested selector 0x55

The shared discriminator table maps `0x55` to switch target VA `0xC1D057` /
raw `0x81C457`, which calls receiver slot `+0xD8` at raw `0x81C476`.
The `tfGameProtocolHandler` vtable entry points to VA `0xECB4E0` / raw method
`0xACA8E0`. The method loads message `+0x28` and treats it as a SharedText
pointer, retaining the referenced value before calling helper `0x8EDBD0`.
That helper builds an event payload under key `"action"` from the text (using
a fallback when null), passes its string data and length through `0xC240F0`,
and invokes the event sink's vtable slot `+0xAC` with code `197`.

The parsed object instead records `+0x28` as one of its raw DWORDs via the
generic four-byte reader `0x3370A0`. This is a conditional static
representation conflict, not proof that this message reaches the handler or
that the value is a valid SharedText pointer. No semantic meaning is assigned
to the field or event.

### Selector 0x56: `TookDebugWeaponHit`

Entry3 maps selector `0x56` to VA `0xC1D9BA` / raw case `0x81CDBA`. It
allocates `0xA8` bytes, stores local DWORD `0x5E` (role unknown), then calls
constructor `0x524FC0`. The constructor passes `0x56` to `0x5A0550`, installs
vtable VA `0x1C67248` (RTTI `firefall::Character::TookDebugWeaponHit`), and
zeros object bytes `+0x58` and `+0x88`. After construction, the case branches
to shared body-reader setup `0x81CD94`; its call at `0x81CDA0` invokes the
same reader `0x5F60F0` used by selector `0x55`.

This proves the two selectors share the same observed field-reading routine,
not that their distinct RTTI classes have identical semantics. The reader's
offset/width ledger and conditional tails are recorded under selector
`0x55`; no semantic field names are inferred for either class.

#### Conditional post-parse consumer for nested selector 0x56

The shared discriminator table maps `0x56` to switch target VA `0xC1D082` /
raw `0x81C482`, which calls receiver slot `+0xDC` at raw `0x81C4A1`. The
`tfGameProtocolHandler` vtable entry contains VA `0xECEA30`, which maps through
the PE `.text` section to raw file offset `0xACDE30`: image base `0x400000`,
section RVA `0x1000`, and raw pointer `0x400` give
`0x400 + (0xECEA30 - 0x400000 - 0x1000)`. This is a function entry, not an
interior instruction target. The earlier `0xACEA30`/`0xACE430` interpretations
were arithmetic errors and do not describe this vtable slot.

At raw `0xACDE30`, the method obtains the current context and checks its state
pointer at `+0x43D0`; a zero pointer enters a diagnostic call with code `1653`.
It then passes the parsed message's `+0x28` address to helper `0x927F60`, using
the state pointer as `this`. The helper checks the DWORD at input `+4`, treats
input `+0` as a tree-container pointer, and merges each source node into the
context tree at `+0x30`. It looks up nodes using the four-byte key at source
node `+0x10`; on a missing key it inserts a node, then copies source DWORDs
`+0x14/+0x18/+0x1C` into destination `+0x14/+0x18/+0x10`. After processing a
nonzero-count collection it calls the event sink vtable slot `+0xAC` with code
`222` and null payload arguments.

There is a notable parser/consumer representation mismatch. The selector
`0x56` case passes message `+0x28` as the body-reader destination; the shared
reader writes a byte at `+0x28` and a DWORD at `+0x2C`. The consumer instead
interprets those positions as a tree-container pointer and its count. The
constructor's observed explicit stores do not initialize the `+0x28` field.
This is a static layout conflict only; it does not establish that the selector
reaches the method, that the collection is malformed at runtime, or that a
runtime failure occurs.

### Selector 0x57: `DebugWeaponStats`

Entry4 maps selector `0x57` to VA `0xC1D9E7` / raw case `0x81CDE7`. It
allocates `0x38` bytes, stores local DWORD `0x5F` (role unknown), then calls
constructor `0x5250A0` and body reader `0x5F4A10`. The constructor passes
`0x57` to `0x5A05C0` and installs vtable VA `0x1C67238` (RTTI
`firefall::Character::DebugWeaponStats`). The reader consumes eight WORDs
in order into object offsets `+0x28,+0x2A,+0x2C,+0x2E,+0x30,+0x32,+0x34,+0x36`.
These occupy the final 16 bytes of the allocation. Their meanings remain
unknown; the RTTI name does not establish their semantics.

#### Conditional post-parse consumer for nested selector 0x57

The outer handler switch uses table VA `0xC1D6DC`; selector `0x57` indexes
entry `0x37`, whose target VA `0xC1D0AD` maps to raw case `0x81C4AD`. That
case calls `tfGameProtocolHandler` slot `+0xE0` at raw `0x81C4CC`. Its vtable
entry contains VA `0xECEF10`, mapped to raw `0xACE310`.

At raw `0xACE310`, the consumer obtains the current context and reads its
state pointer at `+0x43D0`. If that pointer is zero, it enters a diagnostic
call with code `1653`; the observed path then continues to use the state
pointer. The method resets the end pointer of a vector at state `+0x90` to its
begin pointer, then calls `0x929920` with the DWORDs at message `+0x28` and
`+0x2C` as the input range's begin and end addresses. The helper treats the
range as DWORD elements, computes its length as `(end - begin) / 4`, grows
the destination vector when its capacity is insufficient, and copies the
range. The consumer then calls the event sink vtable slot `+0xAC` with code
`227` and null payload arguments, including when the helper sees an empty
range.

The parser/consumer representations do not line up directly: the parser
reader writes eight WORDs from `+0x28` through `+0x36`, while the consumer
interprets the first two DWORDs (`+0x28/+0x2C`) as a pointer range. The
constructor's observed stores initialize metadata through `+0x24` and install
the vtable; no explicit initialization of `+0x28..+0x36` was observed.
These are static access patterns only and do not establish that the selector
reaches this consumer or that the range is invalid at runtime.

### Selector 0x58: `RewardInfo`

Entry5 maps selector `0x58` to VA `0xC1DA38` / raw case `0x81CE38`. It
allocates `0x48` bytes, stores local DWORD `0x60` (role unknown), then calls
constructor `0x525120` and reader `0x5F47C0`. The constructor passes `0x58`
to `0x5A0630`, installs vtable VA `0x1C67218` (RTTI
`firefall::Character::RewardInfo`), and zeros DWORDs at object `+0x30/+0x34`.
The reader then fills a byte at `+0x28`, a byte at `+0x29`, a raw QWORD at
`+0x30`, and three raw DWORDs at `+0x38/+0x3C/+0x40`. These reads nominally
consume `0x16` input bytes and write through object `+0x43`; the constructor's
zeroes at `+0x30/+0x34` are overwritten by the QWORD read. Gaps at
`+0x2A..+0x2F` and the final allocation bytes `+0x44..+0x47` are not written
by this reader. Field meanings remain unknown; the RTTI name does not prove
reward-related semantics.

#### Conditional post-parse consumer for nested selector 0x58

The outer handler switch uses table VA `0xC1D6DC`; selector `0x58` indexes
entry `0x38`, whose target VA `0xC1D0D8` maps to raw case `0x81C4D8`. That case
calls `tfGameProtocolHandler` slot `+0xE4` at raw `0x81C4F7`. The vtable entry
contains VA `0xECB580`, mapped to raw `0xACA980`.

At raw `0xACA980`, the consumer reads the context state pointer at `+0x43C8`.
If it is zero, the method emits diagnostic code `1651` and continues to the
state-update helper `0x909120`. It passes that helper the DWORD at message
`+0x28` and literal `1`; the remaining `RewardInfo` fields are not read by
this consumer. The helper searches its ordered tree using the message DWORD
as the key (node comparison value at `+0x14`). A matching key returns without
the follow-up. If the key is absent and the DWORD is zero, it returns false;
if absent and nonzero, it clears/resets the container and calls
`0x9092A0` with the pair `{1, messageDWORD}` and a zero flag.

The `0x9092A0` follow-up has conditional side effects. One branch calls
`SendFindServiceProviderRequest(1)` when its local selection path sets the
request flag; a successful control-routine result also sets the state byte at
`+8` and calls the event sink slot `+0xAC` with code `219` and null payload
arguments. This is a transitive, branch-dependent path, not an unconditional
effect of receiving selector `0x58`. The consumer itself returns the helper's
result and does not directly emit event `219`.

There is a static parser/consumer representation mismatch: the reader writes
bytes at message `+0x28/+0x29`, while the consumer reads a DWORD at `+0x28`,
which also covers the unwritten `+0x2A/+0x2B` gap. The constructor's observed
zeroes begin at `+0x30`, so they do not establish initialization of those four
bytes. This is an observed layout conflict only; it does not prove that the
selector reaches the consumer or establish a runtime outcome.

### Selector 0x59: `ProgressionXpRefresh`

Entry6 maps selector `0x59` to VA `0xC1DA89` / raw case `0x81CE89`. It
allocates `0x38` bytes, stores local DWORD `0x61` (role unknown), then calls
constructor `0x5251B0` and reader `0x5F4170`. The constructor passes `0x59`
to `0x5A06A0`, installs vtable VA `0x1C67228` (RTTI
`firefall::Character::ProgressionXpRefresh`), and zeros the vector header at
object `+0x28/+0x2C/+0x30`.

The reader treats that header as begin/end/capacity pointers, resets end to
begin, then reads count bytes. For each count it reads that many records of
five raw DWORDs (`0x14` bytes each) and inserts them into the vector.
`0x5200F0` copies the record as 16 bytes plus a DWORD and advances vector end
by `0x14`. A count of `0xFF` reads 255 records and then causes another count
byte; any count below `0xFF` terminates the sequence, with zero meaning no
records in that block. Nominal input consumption is one count byte plus
`count * 0x14` bytes per block. No per-reader cap on repeated `0xFF` blocks is
established. The five DWORD meanings and the RTTI's semantic relation to
progression remain unknown. `CharacterProgressionXpRefreshNativeLayout`
records the vector header, record width, and count continuation marker
without assigning field names or implementing a parser.

#### Conditional post-parse consumer for nested selector 0x59

The outer handler switch uses table VA `0xC1D6DC`; selector `0x59` indexes
entry `0x39`, whose target VA `0xC1D103` maps to raw case `0x81C503`. That case
calls `tfGameProtocolHandler` slot `+0xE8` at raw `0x81C522`. The vtable entry
contains VA `0xECB5D0`, mapped to raw `0xACA9D0`.

The vtable target begins with a context lookup and reads the state pointer at
context `+0x43C8`. If it is zero, the method emits diagnostic code `1651` and
continues to use it. It then treats the pointed-to object as a tree container:
`[state]` is a sentinel pointer and the DWORD at sentinel `+4` is passed to
tree-clear helper `0x90A7B0`. After that call, it resets sentinel fields
`+0/+4/+8` to the sentinel, clears the size DWORD at state `+4`, and sets byte
state `+8` to `1`.

The method does not load the dispatched message argument or any
`ProgressionXpRefresh` payload fields in its observed body. Consequently the
parser's vector header and five-DWORD records are not consumed by this receiver
path. This is a static observation about the receiver instructions; it does
not establish why the sender includes those fields, whether another route
consumes them, or what semantic effect the state-tree reset has.

### Selector 0x5A: `ReceivedDeferredXP`

Entry7 maps selector `0x5A` to VA `0xC1DADC` / raw case `0x81CEDC`. It
allocates `0x38` bytes, stores local DWORD `0x62` (role unknown), then calls
constructor `0x5252A0` and reader `0x5F3A80`. The constructor passes `0x5A`
to `0x5A0710`, installs vtable VA `0x1C67208` (RTTI
`firefall::Character::ReceivedDeferredXP`), and zeros the vector header at
object `+0x28/+0x2C/+0x30`.

The reader resets vector end to begin, reads one count byte, then parses that
many records of four raw DWORDs (`0x10` bytes each) into the vector. A count
of `0xFF` parses 255 records and causes another count byte; any count below
`0xFF` terminates the sequence, with zero meaning no records in that block.
The reader stores each 16-byte record and advances vector end by `0x10`;
`0x5F3880` is the vector-growth helper. Nominal input consumption is one
count byte plus `count * 0x10` bytes per block. No per-reader cap on repeated
`0xFF` blocks is established. The four DWORD meanings and the RTTI's
semantic relation to XP remain unknown. `CharacterReceivedDeferredXPNativeLayout`
records the vector header, record width, and count continuation marker without
assigning field names or implementing a parser.

#### Conditional post-parse consumer for nested selector 0x5A

The outer handler switch uses table VA `0xC1D6DC`; selector `0x5A` indexes
entry `0x3A`, whose target VA `0xC1D12E` maps to raw case `0x81C52E`. That case
calls `tfGameProtocolHandler` slot `+0xEC` at raw `0x81C54D`. The vtable entry
contains VA `0xECB630`, mapped to raw `0xACAA30`.

The consumer reads the DWORD at message `+0x28`, gets the current context
through `0x49DCC0`, then calls `0x4BCBD0` with arguments
`(messageDWORD, 9, 0, 0)`. It does not read the vector end/capacity pointers
or iterate the 16-byte records. Since the parser stores a vector begin pointer
at `+0x28`, this receiver passes that pointer value as the control routine's
first DWORD; the target's intended interpretation is not established.

`0x4BCBD0` performs additional global/configuration lookups and has early
false-return branches. One branch dispatches event code `50` through the
event sink at vtable slot `+0xAC` with null payload arguments, then may call
`0x94E4E0` when its context field at `+0x4140` is nonzero before returning
true. These are conditional helper effects, not guaranteed consequences of
the selector. The `ReceivedDeferredXP` record meanings and the relation of
this control call to the parsed records remain unresolved.

### Selector 0x5B: `PublicCombatLog`

Entry8 maps selector `0x5B` to VA `0xC1DB2F` / raw case `0x81CF2F`. It
allocates `0x40` bytes, stores local DWORD `0x63` (role unknown), then calls
constructor `0x525390` and reader `0x5F3350`. The constructor passes `0x5B`
to `0x5A07A0`, installs vtable VA `0x1C671F8` (RTTI
`firefall::Character::PublicCombatLog`), zeros byte `+0x28`, and initializes
an embedded blob at `+0x2C` with vtable VA `0x1C80CC4`.

At this case's call site, the reader's `this` points to object `+0x28`, and
the input stream is passed separately. The reader consumes a raw DWORD and
continues only when its value is `1`; on that branch it calls
`0x12EE170` with destination `this+4`, which maps to the embedded blob at
object `+0x2C`. The helper reads a WORD length followed by that many bytes.
Thus the nominal successful body is `[raw DWORD 1][WORD length][blob bytes]`,
with minimum six bytes for a zero-length blob. Other DWORD values reach a
diagnostic path whose exact failure/return behavior is not established.
The reader does not store the raw DWORD into object `+0x28`; it is only the
observed gate. Blob contents and field semantics remain unknown.

#### Conditional post-parse consumer for nested selector 0x5B

The outer handler switch uses table VA `0xC1D6DC`; selector `0x5B` indexes
entry `0x3B`, whose target VA `0xC1D159` maps to raw case `0x81C559`. That case
calls `tfGameProtocolHandler` slot `+0xF0` at raw `0x81C578`. The vtable entry
contains VA `0xECB260`, mapped to raw `0xACA660`.

The receiver reads a QWORD at message `+0x30/+0x34`, checks byte `+0x38`, and
uses byte `+0x28` and DWORD `+0x2C` in selected branches. It chooses the
empty/sentinel route when the QWORD is zero or when its high DWORD is
`0xFFFFFFFF` and its low DWORD masked with `0xFFFFFF00` equals
`0xFFFFFF00` (the value range `0xFFFFFFFFFFFFFF00..0xFFFFFFFFFFFFFFFF`).
Otherwise it takes the ordinary QWORD route. In either route, byte `+0x38`
selects a flagged or unflagged helper.

The consumer gets a state pointer from current context `+0x43BC`. A zero
pointer causes diagnostic `1647`, after which the code continues into that
state. For a zero/sentinel QWORD it calls `0xB901F0` when byte `+0x38` is set,
or `0xB903B0` when it is clear. For an ordinary QWORD with the byte set, it
calls `0xB90A30` on the map subobject at state `+8`; with the byte clear, it
searches that map through `0xB90810` and passes the returned range to
`0x36F480`. These helpers perform ordered-tree lookup/update work, but the
records' field meanings and the control meaning of the byte remain unknown.

The parser's successful branch reads a raw DWORD equal to `1`, then delegates
its embedded body at `+0x2C` to a WORD-length-prefixed byte reader. The
consumer's QWORD and byte accesses overlap that embedded object, but the
captured bytes do not establish semantic field names or a higher-level record
schema.

### Selector 0x5C: `PrivateCombatLog`

Entry9 maps selector `0x5C` to VA `0xC1DB81` / raw case `0x81CF81`. It
allocates `0x40` bytes, stores local DWORD `0x64` (role unknown), then calls
constructor `0x525480`. The constructor passes `0x5C` to `0x5A0810`, installs
vtable VA `0x1C671E8` (RTTI `firefall::Character::PrivateCombatLog`), zeros
byte `+0x28`, and initializes the blob subobject at `+0x2C`.

After construction, the case branches to the shared reader setup at raw
`0x81CF5B`; it passes `this = object+0x28` and the input stream to the same
reader `0x5F3350` used by selector `0x5B`. The reader's `this+4` destination
therefore maps to object `+0x2C`, and its successful body format is the same
observed DWORD-1 gate followed by a WORD-length-prefixed blob. Shared parsing
does not prove that `PublicCombatLog` and `PrivateCombatLog` have identical
semantics; the non-1 diagnostic path remains unresolved.

#### Selector 0x5C post-parse consumer

The outer handler switch maps selector `0x5C` through VA `0xC1D184` / raw case
`0x81C584`, which invokes slot `+0xF4` at raw `0x81C5A3`. The slot at raw
`0x1870BB4` contains VA `0xECC460`, mapping to receiver raw `0xACB860`.
The earlier tentative target at VA `0x859010` was based on the wrong vtable
offset and is not this receiver.

The receiver reads the parsed message's byte at `+0x28`, embedded blob object
at `+0x2C`, blob data pointer at `+0x30`, WORD at `+0x34`, and byte at `+0x36`.
It passes the `+0x2C` value to `0x391470`, which maps a numeric key through a
global table and produces shared text; empty lookup uses a separate fallback
path. A nonzero `+0x30` value is passed to the first method of global interface
`0x1E00100`, then appended through `0xC23FE0`. The `+0x28` byte is used as a
key for a 12-byte-stride global table; a matching entry conditionally
contributes two more text values. The WORD at `+0x34` indexes a global table
of floats and is passed to `0xC242F0`; the byte at `+0x36` is zero-extended
and passed to `0xC243E0`.

Finally, the receiver dispatches application event `122` through sink slot
`+0xAC`. This connects the parsed `PrivateCombatLog` object to an application
event path, but the encoded field labels and the higher-level meaning of the
values remain unresolved. These observations are from the receiver's static
data flow; the shared parser body does not establish semantic equivalence with
selector `0x5B`.

### Selector 0x5D: `AnimationUpdated`

Entry10 maps selector `0x5D` to VA `0xC1DBAB` / raw case `0x81CFAB`. It
allocates `0x30` bytes, stores local DWORD `0x65` (role unknown), then calls
constructor `0x525570` and body reader `0x5F30F0`. The constructor passes
`0x5D` to `0x5A0880` and installs vtable VA `0x1C671D8` (RTTI
`firefall::Character::AnimationUpdated`). The reader stores a WORD at object
`+0x28` followed by a byte at `+0x2A`, nominally consuming three input bytes.
The field meanings remain unknown; the RTTI name does not establish
animation-related semantics.

#### Selector 0x5D post-parse consumer

The outer handler switch maps selector `0x5D` through VA `0xC1D1AF` / raw case
`0x81C5AF`, which calls slot `+0xF8` at raw `0x81C5CE`. The corresponding
vtable entry at raw `0x1870BB8` contains VA `0xECCB40`, mapped to receiver raw
`0xACBF40`.

The receiver returns when handler context `+8` is null. Otherwise it reads a
DWORD at context `+8` as the reference counter, then combines that value's
high 16 bits with the two message WORDs at `+0x28` and `+0x2A`. Each candidate
is moved by one `0x10000` period when the distance exceeds `0x8000`, producing
two extended values near the reference counter. These are arithmetic
observations; the code does not establish whether they represent ticks,
sequence numbers, or another domain.

The receiver forwards the extended pair, message byte `+0x2C`, and a mode
derived from byte `+0x2D` to relay helper `0x3343B0`. The relay accepts indices
through 10, modes through 3, and only when its state byte `+6` is zero and
byte `+5` is nonzero. It selects a 16-byte record at `this + 16*(mode+5)` and
passes it to queue helper `0x334460`. That helper stores a 16-byte record
containing the extended pair, the message index byte, and a selected endpoint
of the pair; the endpoint choice is controlled by relay-state byte `+0x40`.
The observed write cursor wraps modulo `0xBB8` (3000 records).

The RTTI label and WORD/byte locations give no stronger semantic names for
the transmitted values. Downstream record consumers and the precise domain
of the extended pair remain unresolved.

### Selector 0x5E: `RaiaNPCDebugging`

Entry11 maps selector `0x5E` to VA `0xC1DBFC` / raw case `0x81CFFC`. It
allocates `0x30` bytes, stores local DWORD `0x66` (role unknown), then calls
constructor `0x5255F0`. The constructor passes `0x5E` to `0x5A08F0`, installs
vtable VA `0x1C671B8` (RTTI `firefall::Character::RaiaNPCDebugging`), and
zeros object field `+0x28`.

The case branches to shared reader setup `0x81FB27`. The caller passes the
input stream and the address of object `+0x28` to helper `0x12EE010`, also
documented as the SDB block-name reader in `src/Serialization/README.md`.
That helper consumes raw bytes through and including the first NUL, copies
the preceding bytes into the shared-text field, and reads in chunks of at
most `0x100` bytes using a `0x104`-byte temporary buffer. The chunk size is
not an established total-string limit; no overall maximum or encoding is
proven. The field’s text meaning remains unknown, and the RTTI name does not
establish NPC-debugging semantics.

#### Selector 0x5E post-parse consumer

The outer handler switch maps selector `0x5E` through VA `0xC1D1DA` / raw case
`0x81C5DA`, then invokes slot `+0xFC` at raw `0x81C5F9`. The vtable entry at
raw `0x1870BBC` contains VA `0xECCBE0`, mapped to receiver raw `0xACBFE0`.
That receiver checks handler-context pointer `+8`, reads a counter through
context `+8`, and consumes five WORDs at message offsets `+0x28`, `+0x2A`,
`+0x2C`, `+0x2E`, and `+0x30`. Each value is extended around the same
reference counter using the same `0x10000` wrap period and `0x8000` half-range
seen in selector `0x5D`. Under gates on global state `0x1E0004C`, the receiver
converts five signed differences to floats and passes them to `0x481430` with
five distinct state/output locations.

This is a static parser/consumer mismatch: the selector-`0x5E` parser allocates
`0x30` bytes and reads one NUL-terminated shared-text field into `+0x28`,
whereas this consumer treats the same message as at least five WORDs through
`+0x30` (read end `+0x32`). The receiver's final WORD therefore extends two
bytes past the declared allocation. The parser supplies a text field rather
than five explicit WORD values; whether that field's storage overlaps the
receiver's other WORD reads is unresolved. The constructor initializes base
fields only through `+0x24`; whether the allocator initializes the remaining
portion is not established. Do not assign semantics to these conflicting
fields or infer runtime impact from the static mismatch alone.

### Selector 0x5F: `WeaponProjectileFired`

Entry12 maps selector `0x5F` to VA `0xC1DC2D` / raw case `0x81D02D`. It
allocates `0x38` bytes, stores local DWORD `0x67` (role unknown), then calls
constructor `0x525700`. The constructor passes `0x5F` to helper `0x5A0960`,
installs vtable VA `0x1C671C8` (RTTI
`firefall::Character::WeaponProjectileFired`), and zeros object field
`+0x30`.

Body reader `0x5F2990` reads four WORDs into object fields `+0x28`, `+0x2A`,
`+0x2C`, and `+0x2E`. It then calls `0x5F27E0` with `this = object+0x30`.
That helper consumes one gate byte; a nonzero byte causes three more WORDs
to be read and stored at `+0x32`, `+0x34`, and `+0x36`, with marker byte
`+0x30` set to one. A zero gate clears the marker when it is set. The WORD
reader checks for two available input bytes before reading and advances the
stream by two. Thus the observed nominal body size is 9 bytes without the
optional tail or 15 bytes with it. Field meanings remain unknown; the RTTI
name alone does not establish projectile semantics.

#### Selector 0x5F post-parse consumer

The outer handler switch maps selector `0x5F` through VA `0xC1D205` / raw case
`0x81C605`, then invokes slot `+0x100` at raw `0x81C624`. The vtable entry at
raw `0x1870BC0` contains VA `0xECCF30`, mapped to receiver raw `0xACC330`.
This routine reads 13 WORDs, in observed order at offsets `+0x28`, `+0x2A`,
`+0x2C`, `+0x2E`, `+0x32`, `+0x30`, `+0x36`, `+0x34`, `+0x38`, `+0x3A`,
`+0x3C`, `+0x3E`, and `+0x40`. Each is zero-extended, converted to a float,
and conditionally passed to `0x481430` with a separate label and output
location when global state `0x1E0004C` has byte `+4` set and byte `+5` clear.

The parsed layout does not provide 13 WORD values: its fixed prefix is four
WORDs, followed by a gate byte and an optional three-WORD tail. Its maximum
body length is 15 bytes, and the allocated object is `0x38` bytes. The
receiver's last WORD at `+0x40` reads through `+0x41`, ten bytes beyond that
allocation; its `+0x38..+0x40` WORD reads are beyond the declared object too.
Whether allocator padding or adjacent storage changes runtime consequences is
not established. Preserve this as a static parser/consumer extent mismatch,
not a runtime conclusion or an inferred projectile schema.

### Selector 0x60: `AbilityProjectileFired`

Entry13 maps selector `0x60` to VA `0xC1DC7E` / raw case `0x81D07E`. It
allocates `0x68` bytes, stores local DWORD `0x68` (role unknown), then calls
constructor `0x5257A0`. That constructor passes `0x60` to base helper
`0x5A09F0`, installs vtable VA `0x1C671A8` (RTTI
`firefall::Character::AbilityProjectileFired`), and zeros the presence byte
at object `+0x58`. The role of the base-helper argument remains unknown.

Body reader `0x5F22A0` reads eight WORD fields at `+0x28`, `+0x2A`,
`+0x2C`, `+0x2E`, `+0x30`, `+0x32`, `+0x34`, and `+0x36`; five DWORD
fields at `+0x38`, `+0x3C`, `+0x44`, `+0x4C`, and `+0x50`; and byte fields
at `+0x40` and `+0x48`. It then calls `0x5FDA00` with `this = object+0x58`.
That helper consumes a gate byte at input offset `0x26`; when nonzero, it
reads an eight-byte QWORD through `0x356F80`, stores it at `+0x60/+0x64`,
and sets the presence byte at `+0x58` to one. A zero gate clears a set
presence byte. The nominal body length is `0x27` bytes without the QWORD
and `0x2F` bytes with it. The reader does not write object `+0x54..+0x57`;
that gap and all field meanings remain unresolved. RTTI alone does not
establish ability-projectile semantics.

#### Selector 0x60 post-parse consumer

The outer handler switch maps selector `0x60` through VA `0xC1D230` / raw case
`0x81C630`, then invokes slot `+0x104` at raw `0x81C64F`. The vtable entry at
raw `0x1870BC4` contains VA `0xECD580`, mapped to receiver raw `0xACC980`.
The receiver reads four WORDs from message offsets `+0x28`, `+0x2A`, `+0x2C`,
and `+0x2E`, the parser's first four WORD fields. Each read is zero-extended
and converted to a float. When global state `0x1E0004C` has byte `+4` set and
byte `+5` clear, it passes the value to `0x481430` with a distinct descriptor
and output-state offset; those identifiers are recorded in
`CharacterAbilityProjectileFiredConsumerNativeLayout`.

The receiver does not consume the parser's remaining four WORDs, its later
DWORD/byte fields, or its optional QWORD as metrics. The input field meanings
and metric descriptor semantics remain unknown; neither the RTTI name nor
these downstream uses establish ability-projectile semantics.

### Selector 0x61: `ProjectileHitReported`

Entry14 maps selector `0x61` to VA `0xC1DCCF` / raw case `0x81D0CF`. It
allocates `0x30` bytes, stores local DWORD `0x69` (role unknown), then calls
constructor `0x525840`. The constructor passes `0x61` to helper `0x5A0A60`
and installs vtable VA `0x1C67198` (RTTI
`firefall::Character::ProjectileHitReported`).

Body reader `0x5F20E0` reads WORDs into object fields `+0x28` and `+0x2A`,
then bytes into `+0x2C` and `+0x2D`. The observed nominal body length is
six bytes. Field meanings remain unknown; the RTTI name does not establish
projectile-hit semantics.

#### Selector 0x61 post-parse receiver

The outer handler switch maps selector `0x61` through VA `0xC1D25B` / raw case
`0x81C65B`, then calls slot `+0x108` at raw `0x81C67A` with the current
message pointer. The vtable entry at raw `0x1870BC8` contains VA `0xECA340`,
mapped to receiver raw `0xAC9740`. This receiver loads its pointer argument
into `ESI`, sets `ECX` to `0x16` (22), and executes `REP LODSD`; it performs
no writes and leaves the final loaded DWORD (source offset `+0x54`) in `EAX`.
With the normal x86 direction-flag-clear entry state, this reads through
message offset `+0x57` (end-exclusive `+0x58`).

The parser declared a `0x30`-byte object and writes only the six-byte body at
`+0x28..+0x2D`. The receiver's sequential 88-byte read therefore extends
`0x28` bytes beyond the declared allocation. The reason for this bulk read,
the caller's use of the return value, and any runtime consequence are
unresolved; do not infer a 22-DWORD message schema from the receiver alone.

### Selector 0x62: `Stumble`

Entry15 maps selector `0x62` to VA `0xC1DD20` / raw case `0x81D120`. It
allocates `0x30` bytes, stores local DWORD `0x6A` (role unknown), then calls
constructor `0x5258C0`. The constructor passes `0x62` to helper `0x5A0AD0`
and installs vtable VA `0x1C67188` (RTTI
`firefall::Character::Stumble`).

Body reader `0x5F1ED0` reads WORDs into object fields `+0x28` and `+0x2A`,
then reads a byte into `+0x2C`. The observed nominal body length is five
bytes. Field meanings remain unknown; the RTTI name does not establish
stumble-event semantics.

#### Selector 0x62 post-parse consumer

The outer handler switch maps selector `0x62` through VA `0xC1D286` / raw case
`0x81C686`, then invokes slot `+0x10C` at raw `0x81C6A5`. The vtable entry at
raw `0x1870BCC` contains VA `0xECC250`, mapped to receiver raw `0xACB650`.
When global pointer `0x1E68354` refers to state with byte `+0x40` set, the
receiver combines the two parser WORDs at message `+0x28` into a DWORD key
and uses it with context map state at `this+0x4C`. It then reads a begin/end
pointer pair at message `+0x30/+0x34`, computes `(end - begin) / 12`, and
copies/processes the resulting 12-byte entries when the count is nonzero.

The parser allocates `0x30` bytes and writes only through `+0x2C`; the
receiver's pointer pair at `+0x30/+0x34` lies eight bytes beyond the declared
object extent. A second path calls `0x60FB60`; when it returns a nonzero
context, the receiver passes the key, the address of the same pointer pair,
and the status byte at `+0x2C` to `0x60FBD0`. That byte is mapped as `0 -> 0`,
`1 -> 1`, and any other value to `2`. The pointer pair's provenance and
runtime validity are unresolved; the static extent discrepancy alone does
not establish runtime impact or the meaning of the event.

### Selector 0x63: `QuickChat`

Entry16 maps selector `0x63` to VA `0xC1DD71` / raw case `0x81D171`. It
allocates `0x30` bytes, stores local DWORD `0x6B` (role unknown), then calls
constructor `0x525940`. The constructor passes `0x63` to helper `0x5A0B40`
and installs vtable VA `0x1C67178` (RTTI
`firefall::Character::QuickChat`).

On the successful allocation path, the case jumps to shared reader
continuation `0x8206E3`, which reads a DWORD into object field `+0x28`
through helper `0x3370A0`; nominal body length is four bytes. The allocation
failure branch at `0x81D189` instead enters `0x8206E1`, zeroes ESI, and
continues into the same field reader, which forms destination address
`0x28`. The runtime consequence of that abnormal path is unresolved.
Field meaning remains unknown; RTTI alone does not establish chat semantics.

#### Selector 0x63 post-parse receiver

The outer handler switch maps selector `0x63` through VA `0xC1D2B1` / raw case
`0x81C6B1`, then invokes slot `+0x110` at raw `0x81C6D0`. The vtable entry at
raw `0x1870BD0` contains VA `0xECD7E0`, mapped to receiver raw `0xACCBE0`.
The receiver reads a type-string pointer from message `+0x34` and a text
pointer from `+0x30`; it compares the type string against two static byte
sequences. For either match, it obtains the current chat through `0x6F4540`
and posts a notification through `0x6E3460`, using the text pointer or an
empty-string fallback. If neither comparison matches, it logs values that
include the DWORD at `+0x28`. The comparison bytes and field semantics remain
unresolved.

The parser allocates `0x30` bytes and writes only the DWORD at `+0x28`. The
receiver's pointer fields at `+0x30` and `+0x34` therefore extend eight bytes
past the declared allocation (`+0x34..+0x37` is the furthest pointer read).
This static parser/receiver mismatch does not establish whether the pointers
are valid or what runtime effect follows.

### Selector 0x64: `ProximityTextChat`

Entry17 maps selector `0x64` to VA `0xC1DDA2` / raw case `0x81D1A2`. It
allocates `0x48` bytes, stores local DWORD `0x6C` (role unknown), then calls
constructor `0x5259C0`. The constructor passes `0x64` to helper `0x5A0BB0`,
installs vtable VA `0x1C67158` (RTTI
`firefall::Character::ProximityTextChat`), zeros the DWORD at object `+0x28`,
and clears the optional-blob presence byte at `+0x30`.

Body reader `0x5F1BC0` reads a NUL-terminated shared-text field at `+0x28`
through `0x12EE010`, then bytes at `+0x2C`, `+0x2D`, and `+0x2E`. It calls
`0x5F19A0` with `this = object+0x30`; that helper always consumes a gate
byte. When nonzero, it reads a little-endian WORD length and that many bytes
through `0x12EE170`, then updates the reference-backed blob field using
`0x5F1920`. A zero gate clears an existing blob through `0x49AC30` when its
presence marker is set.

#### Selector 0x64 post-parse receiver

The outer handler switch maps selector `0x64` through VA `0xC1D2DC` / raw
`0x81C6DC`, then calls slot `+0x114` at raw `0x81C6FB`. The protocol-handler
vtable entry at raw `0x1870BD4` contains VA `0xECD900`, mapped to the receiver
at raw `0xACCD00`. It reads a WORD at message `+0x40`; values other than `2`
are sent to the unhandled-message logger `0x2FC4F0`. For value `2`, it checks
the transport-context pointer at handler `+0x10`. A null context logs diagnostic
code `91`, but execution then continues to `0x977150` either way, passing
addresses of message fields `+0x28`, `+0x30`, `+0x4C`, and `+0x44`.

The `0x977150` sender consumes the `+0x28` and `+0x30` arguments through
collection helpers and traverses the structure rooted at `+0x44`. The
`+0x4C` address is first passed to `0x975660`, whose decompilation remains
unresolved; its exact meaning and whether that helper dereferences it are
unknown. The parser's allocation is `0x48` bytes, so `message+0x4C` is four
bytes beyond the declared extent. The receiver forms and forwards that address,
but does not directly load from it. This static extent mismatch does not by
itself establish runtime validity or impact.

The nominal input length is the number of text bytes through and including
the terminating NUL, plus three bytes and the gate byte, plus
`2 + blobLength` when the gate is nonzero. Thus an empty text with no blob
consumes five bytes; an empty text with an empty blob consumes seven. The
text reader processes at most `0x100` bytes per chunk using a `0x104`-byte
temporary buffer; no overall text limit or encoding is established. The WORD
blob length permits at most `0xFFFF` payload bytes. Field meanings remain
unknown; RTTI alone does not establish proximity-chat semantics.

### Selector 0x65: `JumpActioned`

Entry18 maps selector `0x65` to VA `0xC1DDF3` / raw case `0x81D1F3`. It
allocates `0x30` bytes, stores local DWORD `0x6D` (role unknown), then calls
constructor `0x525AF0`. The constructor passes `0x65` to helper `0x5A0C20`
and installs vtable VA `0x1C67168` (RTTI
`firefall::Character::JumpActioned`).

On the successful allocation path, the case jumps to shared reader
continuation `0x81D21F`, which reads a WORD into object field `+0x28`
through helper `0x336AC0`; nominal body length is two bytes. The allocation
failure branch at `0x81D20B` instead enters `0x81D21D`, zeroes ESI, and
continues into the same field reader, which forms destination address
`0x28`. The runtime consequence of that abnormal path is unresolved.
Field meaning remains unknown; RTTI alone does not establish jump-action
semantics.

#### Selector 0x65 post-parse receiver

The outer handler switch maps selector `0x65` through VA `0xC1D307` / raw
`0x81C707`, then calls slot `+0x118` at raw `0x81C726`. The protocol-handler
vtable entry at raw `0x1870BD8` contains VA `0xECD970`, mapped to the receiver
at raw `0xACCD70`. It checks the handler context at `+0x10`; a null value logs
diagnostic `91`, then execution still calls `0x977750`. The call passes the
address of message `+0x28`, then addresses `+0x30` and `+0x40`.

The `0x977750` helper performs a keyed lookup using the DWORD pair at
message `+0x28/+0x2C`. It treats the addresses at message `+0x30` and `+0x40`
as polymorphic objects: it reads each address's first DWORD as a vtable and
calls virtual slot `+0x0C`. The `+0x40` object's returned range is iterated to
build event `360` records. When the keyed entry has a nonzero-length
associated field, a separate branch can dispatch event `359` using the key.
The `+0x30` object's returned byte sequence is also traversed, including
eight-byte reads and special `0xFF` handling; the exact record meaning remains
unresolved.

The parser requests `0x30` bytes and writes only the WORD at `+0x28`. The
receiver's `+0x30` object starts at the end of that declared allocation, while
its `+0x40` object starts `0x10` bytes beyond it; the helper's virtual calls
read vtable pointers there. These are static parser/receiver extent conflicts.
The requested allocation size does not establish actual runtime capacity or
the cause or effect of the observed accesses.

### Selector 0x66: `JumpRolled`

Entry19 maps selector `0x66` to VA `0xC1DE47` / raw case `0x81D247`. It
allocates `0x30` bytes, stores local DWORD `0x6E` (role unknown), then calls
constructor `0x525B70`. The constructor passes `0x66` to helper `0x5A0C90`
and installs vtable VA `0x1C67148` (RTTI
`firefall::Character::JumpRolled`).

On the successful allocation path, the case jumps to the same reader
continuation `0x81D21F` used by selector `0x65`; it reads a WORD into object
field `+0x28` through helper `0x336AC0`, for a nominal two-byte body.
Selector `0x66` has its own RTTI and constructor, so the shared parser does
not establish semantic equivalence with `JumpActioned`. The allocation
failure branch at `0x81D25F` enters `0x81D21D`, zeroes ESI, and continues
into the same reader, which forms destination address `0x28`; the runtime
consequence remains unresolved. Field meaning is unknown.

#### Selector 0x66 post-parse receiver

The outer handler switch maps selector `0x66` through VA `0xC1D332` / raw
`0x81C732`, then calls slot `+0x11C` at raw `0x81C751`. The protocol-handler
vtable entry at raw `0x1870BDC` contains VA `0xECD9B0`, mapped to the receiver
at raw `0xACCDB0`. It checks handler context `+0x10`; a null value logs
diagnostic `91`, but execution continues and calls `0x977F20` with the address
of message `+0x28`.

The helper looks up the eight-byte key at message `+0x28/+0x2C`. On a hit, it
removes the associated entry, then synchronously drains its `+4` count by
calling `0x977630` for each linked item before finalizing with `0x975020` and
releasing through `0x12C8D60`. The key's interpretation, entry type, and
per-item effects remain unknown. Both key DWORDs are within the parser's
requested `0x30`-byte allocation; the parser itself reads only the WORD at
`+0x28`.

### Selector 0x67: `Respawned`

Entry20 maps selector `0x67` to VA `0xC1DE71` / raw case `0x81D271`. It
allocates `0x30` bytes, stores local DWORD `0x6F` (role unknown), then calls
constructor `0x525BF0`. The constructor passes `0x67` to helper `0x5A0D00`
and installs vtable VA `0x1C67138` (RTTI
`firefall::Character::Respawned`).

On the successful allocation path, the case calls body reader `0x5F1550`.
That reader reads a WORD into object `+0x28` through `0x336AC0`, then bytes
at `+0x2A` and `+0x2B` through `0x356CF0` and `0x337180`; the nominal body
length is four bytes. Allocation failure branches at `0x81D289` to
`0x81D29B`, zeroes ESI, then joins the body-reader setup at `0x81D29D`;
the consequence of that abnormal path remains unresolved. The field meanings
are unknown, and the RTTI name does not establish respawn semantics.

#### Selector 0x67 post-parse receiver

The outer handler switch maps selector `0x67` through VA `0xC1D35D` / raw
`0x81C75D`, then calls slot `+0x120` at raw `0x81C77C`. The protocol-handler
vtable entry at raw `0x1870BE0` contains VA `0xECD9F0`, mapped to receiver raw
`0xACCDF0`.

The receiver compares message `+0x30` and `+0x34` as begin/end pointers and,
when nonempty, computes a count using a `0x10`-byte stride. It processes each
record by the byte at entry `+0`: kind `0` uses the pointer at `+4` as shared
text, kind `1` formats the DWORD at `+8`, and kind `2` formats the float at
`+0xC`; other kinds are skipped. The exact format strings and meanings are
unknown. For an empty range it instead builds shared text from message
`+0x28`. After aggregating, it conditionally appends shared text from `+0x2C`
and a nonzero byte at `+0x8C`, then dispatches event `167`; event semantics are
unresolved.

The parser requests only `0x30` bytes and writes the WORD at `+0x28` and bytes
at `+0x2A/+0x2B`. Thus the receiver's vector begin/end header starts at and
extends beyond the declared allocation, and its optional byte at `+0x8C` is
also beyond it. These are static extent mismatches, not proof of actual runtime
capacity or impact.

### Selector 0x68: `CalledForHelp`

Entry21 maps selector `0x68` to VA `0xC1DEC2` / raw case `0x81D2C2`. It
allocates `0x30` bytes, stores local DWORD `0x70` (role unknown), then calls
constructor `0x525C70`. The constructor passes `0x68` to helper `0x5A0D70`
and installs vtable VA `0x1C67118` (RTTI
`firefall::Character::CalledForHelp`).

On the successful allocation path, the case jumps to reader continuation
`0x81D21F`, shared by selectors `0x65` and `0x66`; it reads a WORD into
object field `+0x28` through helper `0x336AC0`, for a nominal two-byte body.
The allocation-failure branch at `0x81D2DA` enters `0x81D21D`, zeroes ESI,
and continues into the same reader, which forms destination address `0x28`.
The runtime consequence of that abnormal path is unresolved. Field meaning
remains unknown; shared parsing does not establish semantic equivalence with
the other selectors.

#### Selector 0x68 post-parse receiver

The outer handler switch maps selector `0x68` through VA `0xC1D388` / raw
`0x81C788`, then calls slot `+0x124` at raw `0x81C7A7`. The protocol-handler
vtable entry at raw `0x1870BE4` contains VA `0xECE040`, mapped to receiver raw
`0xACD440`. It reads the DWORD at message `+0x28` and looks it up in registry
`0x1E5A830` through `0x327890`. If the lookup has no usable entry, it logs the
key through `0x2FC4F0`. On a hit, it builds event `176` from several converted
registry-entry values and message fields `+0x2C` and `+0x30`; the associated
labels and field meanings remain unknown.

The parser requests `0x30` bytes and writes a WORD at `+0x28` plus bytes
`+0x2A/+0x2B`. The receiver reads a DWORD key at `+0x28` and accesses a DWORD
at `+0x30`; that latter field starts exactly at the declared allocation end,
so its four-byte read is outside the requested object extent. This static
extent mismatch does not establish actual runtime capacity or impact.

### Selector 0x69: `TookHit`

Entry22 maps selector `0x69` to VA `0xC1DEF3` / raw case `0x81D2F3`. It
allocates `0x60` bytes, stores local DWORD `0x71` (role unknown), then calls
constructor `0x525CF0`. The constructor passes `0x69` to helper `0x5A0DE0`,
installs vtable VA `0x1C67128` (RTTI
`firefall::Character::TookHit`), clears byte `+0x28`, and clears WORD
`+0x50`.

The case calls body reader `0x5F0FC0`. Its first sub-reader `0x5F0CF0`
consumes a gate byte for the record at `+0x28`; when set, it reads a QWORD,
then `0x5FDA00` consumes another gate byte and an optional QWORD, followed by
a DWORD and byte. The temporary values are routed through `0x5F0B20` or
`0x5F0C30`, which copy QWORDs at `+0x30` and optionally `+0x40`, a DWORD at
`+0x48`, and a byte at `+0x4C`; the higher-level meaning of these state
helpers remains unresolved. The body reader then reads bytes at `+0x50` and
`+0x51`, a WORD at `+0x58` via `0x336AC0`, and a byte at `+0x5A`.
With the outer gate clear the body consumes six bytes; when set, its length
depends on the nested gate. Field meanings remain unknown.

#### Selector 0x69 post-parse receiver

The outer handler switch maps selector `0x69` through VA `0xC1D3B3` / raw
`0x81C7B3`, then calls slot `+0x128` at raw `0x81C7D2`. The protocol-handler
vtable entry at raw `0x1870BE8` contains VA `0xECE210`, mapped to receiver raw
`0xACD610`. The receiver copies `0x40` bytes from message `+0x28` using
`0x37F810`; that helper performs four 16-byte reads and writes, including the
block at source `+0x30`.

The receiver then adds the global float at VA `0x1C81CA8` to the copied float
whose message source is `+0x60`. It obtains an owner through `0x49DDD0`; when
the owner's byte at `+0x18C` is set and its pointer at `+0x14C8` is nonzero,
the receiver sets byte `+0x108` on that pointed-to object and copies the
modified `0x40`-byte block to its `+0xC8` field. The meanings of this copied
record, the global adjustment, and the state fields are unknown.

The parser requests `0x60` bytes. The receiver's copy from `+0x28` spans
through `+0x67`, so its final eight source bytes lie beyond the declared
allocation; the adjusted float at `+0x60` is itself wholly outside it. This
is a static extent mismatch and does not establish actual runtime capacity or
impact.

### Selector 0x6A: `AlmostHit`

Entry23 maps selector `0x6A` to VA `0xC1DF44` / raw case `0x81D344`. It
allocates `0x30` bytes, stores local DWORD `0x72` (role unknown), then calls
constructor `0x525DD0`. The constructor passes `0x6A` to helper `0x5A0E50`,
installs vtable VA `0x1C67108` (RTTI
`firefall::Character::AlmostHit`), and clears the QWORD at `+0x28`.

The successful path joins the shared continuation at `0x81F72E`, which forms
the destination `object+0x28` and calls `0x356F80` to read one QWORD (nominal
body length eight bytes). Allocation failure branches at `0x81D35C` to
`0x81F72C`, zeroes ESI, then joins the same reader setup, which forms address
`0x28`; the consequence of that abnormal path is unresolved. The field
meaning remains unknown; the RTTI name does not prove message semantics.

#### Selector 0x6A post-parse receiver

The outer handler switch maps selector `0x6A` through VA `0xC1D3DE` / raw
`0x81C7DE`, then calls slot `+0x12C` at raw `0x81C7FD`. The protocol-handler
vtable entry at raw `0x1870BEC` contains VA `0xECEAD0`, mapped to receiver raw
`0xACDED0`.

The receiver first requires the global object at VA `0x1DFFFE8` to have a
nonzero pointer at `+0x28`. It treats the QWORD at message `+0x30` as a lookup
key, rejecting zero and values from `0xFFFFFFFFFFFFFF00` through
`0xFFFFFFFFFFFFFFFF`. For other values, it clears the low byte of the key's
low DWORD and searches the table obtained through `0x35E700` using `0x4093E0`.
If the found entry has a nonzero pointer at `+0x10`, it calls that object's
virtual slot `+0x70`; the return value is then passed to `0x369080` along with
the QWORD at message `+0x28` and constant `12`. That helper branches on the
signed QWORD and routes through distinct manager-side helpers; their meanings
remain unknown.

The parser requests `0x30` bytes and reads a QWORD at `+0x28`. The receiver's
lookup key is a QWORD at `+0x30`, entirely beyond the requested extent. This
is a static extent mismatch and does not establish actual runtime capacity or
impact.

### Selector 0x6B: `DealtHit`

Entry24 maps selector `0x6B` to VA `0xC1DF75` / raw case `0x81D375`. It
allocates `0x58` bytes, stores local DWORD `0x73` (role unknown), then calls
constructor `0x525E60`. The constructor passes `0x6B` to helper `0x5A0EC0`,
installs vtable VA `0x1C670F8` (RTTI
`firefall::Character::DealtHit`), clears byte `+0x28`, and clears WORD
`+0x50`.

The successful path joins the reader setup at `0x81D3A1` and calls
`0x5F0800`. Its first sub-reader `0x5F0CF0` consumes a gate byte for the
record at `+0x28`; when set it reads a QWORD, a nested gate byte and optional
QWORD, then a DWORD and byte. The temporary values are routed through
`0x5F0B20` or `0x5F0C30` into offsets `+0x30`, `+0x38`, `+0x40`, `+0x48`,
and `+0x4C`. The reader then consumes bytes at `+0x50` and `+0x51`. With the
outer gate clear, the nominal minimum body length is three bytes; a set gate
adds the record fields. Allocation failure branches at `0x81D38D` to
`0x81D39F`, zeroes ESI, and continues into the same reader; its abnormal
runtime consequence is unresolved. Field meanings and message semantics
remain unknown.

#### Selector 0x6B post-parse receiver

The outer handler switch maps selector `0x6B` through VA `0xC1D409` / raw
`0x81C809`, then calls slot `+0x130` at raw `0x81C828`. The protocol-handler
vtable entry at raw `0x1870BF0` contains VA `0xECEB70`, mapped to receiver raw
`0xACDF70`. That wrapper calls `0x368EA0` with the address of message `+0x30`,
the QWORD at `+0x28`, constant `12`, and duration value `0x3C`.

The shared helper treats the bytes at `+0x30/+0x34/+0x38` as three floats,
updates position-related state when manager byte `+0xE1` is set, and can
compute their Euclidean distance from the global vector at VAs
`0x1E62C44/0x1E62C48/0x1E62C4C` before a conditional callback through
`0x100AE90`. It also sends the QWORD at `+0x28` through `0x369080` and
registers a follow-up timer for `0x3C * 1000` milliseconds. The meanings of
the QWORD, callback, and timer action remain unresolved.

The parser's conditional record uses byte `+0x38` as a nested gate, while the
receiver's helper reads `+0x38..+0x3B` as its third float. This is an observed
overlay between parser and consumer views; the relationship between those
representations is unresolved. All these reads remain within the parser's
requested `0x58`-byte allocation.

### Selector 0x6C: `Killed`

Entry25 maps selector `0x6C` to VA `0xC1DFC6` / raw case `0x81D3C6`. It
allocates `0x40` bytes, stores local DWORD `0x74` (role unknown), then calls
constructor `0x525F20`. The constructor passes `0x6C` to helper `0x5A0F30`,
installs vtable VA `0x1C670E8` (RTTI
`firefall::Character::Killed`), and clears the QWORD at `+0x30`.

The success path joins reader setup at `0x81D3F2` and calls `0x5F0560`. In
reader order it reads a WORD into `+0x28`, a QWORD into `+0x30`, then bytes
into `+0x38`, `+0x39`, and `+0x3A` (13 nominal input bytes). Allocation
failure branches to `0x81D3F0`, zeroes ESI, and reaches the same reader setup;
the abnormal-path consequence is unresolved. The RTTI label does not prove
message semantics, and all field meanings remain unknown.

#### Selector 0x6C post-parse receiver

The outer handler switch maps selector `0x6C` through VA `0xC1D434` / raw
`0x81C834`, then calls slot `+0x134` at raw `0x81C853`. The protocol-handler
vtable entry at raw `0x1870BF4` contains VA `0xECEBA0`, mapped to receiver raw
`0xACDFA0`. This wrapper passes global `0x1DFFFFC`, the DWORD at message
`+0x28`, constant `12`, zero, and the address of message `+0x2C` to helper
`0x466790`.

The helper looks up an object using the DWORD through `0x421640`; depending on
the returned object's fields and a pair of DWORDs beginning at message
`+0x2C`, it either updates or searches manager-owned contexts. Several paths
allocate a `0xB0`-byte context and call update routine `0x465A10`; other paths
log or return without that update. The object's role, message fields, and
context semantics are unresolved.

The parser requests `0x40` bytes. The receiver's `+0x28` DWORD and the
helper's access to the pair at `+0x2C/+0x30` remain within that extent; no
allocation-boundary mismatch is apparent in this path.

### Selector 0x6D: `WarnLockTargeted`

Entry26 maps selector `0x6D` to VA `0xC1E017` / raw case `0x81D417`. It
allocates `0x28` bytes, stores local DWORD `0x75` (role unknown), then calls
constructor `0x525FB0`. The constructor passes `0x6D` to helper `0x5A0FA0`
and installs vtable VA `0x1C670C8` (RTTI
`firefall::Character::WarnLockTargeted`). This case path has no explicit
body-reader call. At raw `0x81D435` it loads the two DWORDs at `[esi+4]` and
`[esi]`, passes them to constructor `0x525FB0` at `0x81D43C`, then reaches the
common return epilogue at `0x81D441` without another parser/read call.
Allocation failure branches to `0x8217D9`, the switch's zero-return epilogue.
The common decoder `0x821BA0` has already read a raw QWORD and compact
positive-UInt16 selector before calling `0x81CC50`. When the QWORD's first byte
selects the secondary table, the compact selector is passed as the parser's
selector argument; the `0x6D` case uses the two DWORDs at the QWORD address as
constructor arguments. The base initializer stores those words at object
`+0x18/+0x1C`, zeros `+0x20/+0x24`, and finishes at the requested allocation
end `+0x28`. It invokes no additional selector-specific body reader. This
narrows the no-reader observation to bytes beyond the shared QWORD/selector
preamble; whether such trailing bytes are valid or accounted for elsewhere
remains unresolved, as do all field meanings.

#### Selector 0x6D post-parse receiver

The outer handler switch maps selector `0x6D` through VA `0xC1D45F` / raw
`0x81C85F`, then calls slot `+0x138` at raw `0x81C87E`. The protocol-handler
vtable entry at raw `0x1870BF8` contains VA `0xECEBC0`, mapped to receiver raw
`0xACDFC0`.

The receiver calls `0x465D40` only if the global object at VA `0x1DFFFFC`
contains a nonzero manager pointer. It forwards the DWORD at message `+0x28`,
constant `12`, the manager pointer, and a normalized Boolean from byte
`+0x38`. The helper maintains manager-owned entries keyed by the DWORD,
removes matching entries, and can update manager state; when its Boolean and
stored-key conditions hold it may also query a selected object and dispatch
additional state. The behavior of that state and the message's semantics
remain unknown.

The parser allocates `0x28` bytes and has no explicit body-reader call in this
case. The receiver nevertheless reads a DWORD at `+0x28` (four bytes beyond
the allocation) and a Boolean byte at `+0x38` (17 bytes beyond its end). These
are static extent mismatches, not proof of actual runtime capacity or impact.

### Selector 0x6E: `CurrentPoseUpdate`

Entry27 maps selector `0x6E` to VA `0xC1E054` / raw case `0x81D454`. It
allocates `0x58` bytes, stores local DWORD `0x76` (role unknown), and calls
constructor `0x526030`. The constructor passes `0x6E` to helper `0x5A1010`
and installs vtable VA `0x1C670D8` (RTTI
`firefall::Character::CurrentPoseUpdate`).

Reader setup at `0x81D480` calls `0x5EFE30`. The reader uses the raw control
byte at `+0x28`: bit `0x01` selects a byte at `+0x29` instead of a WORD at
`+0x2A`; it always reads a byte at `+0x2C`; when bit `0x02` is clear it reads
a WORD at `+0x2E`. If `(control & 0x0C) == 0`, it reads DWORDs at
`+0x34/+0x38/+0x3C`; otherwise it copies three bytes into the field starting
at `+0x30`. The `control & 0x30` cases select four WORDs at `+0x44..+0x4A`,
one byte at `+0x40`, three bytes at `+0x41..+0x43`, or no mid-group read
when the mask is `0x30`. Finally, `control & 0xC0` selects three WORDs at
`+0x50/+0x52/+0x54` when clear, a three-byte copy at `+0x4C` for `0x40` or
`0x80`, or no tail-group read for `0xC0`. Allocation failure zeroes ESI and
uses the same reader setup. These are observed branch masks and field widths;
their meanings and message semantics remain unknown.

#### Selector 0x6E post-parse receiver

The outer handler switch maps selector `0x6E` through VA `0xC1D48A` / raw
`0x81C88A`, then calls slot `+0x13C` at raw `0x81C8A9`. The protocol-handler
vtable entry at raw `0x1870BFC` contains VA `0xECE2A0`, mapped to receiver raw
`0xACD6A0`.

The receiver creates an event payload, formats message fields `+0x28/+0x2C/
+0x30` as three floats using distinct label strings, and optionally looks up
the DWORD at `+0x34` in registry `0x1E5A4F0`. A usable entry is converted to
text through `0x13122D0` and appended. It dispatches event `239` through the
object at global `0x1E002B8`, vtable slot `+0xAC`, then cleans up the payload.
The relationship between this receiver's float/key view and the parser's
control-byte and conditional-field layout is unresolved.

The parser requests `0x58` bytes; all receiver field accesses described above
are within that requested extent. The event's meaning and the fields' semantic
roles remain unknown.

### Selector 0x6F: `ConfirmedPoseUpdate`

Entry28 maps selector `0x6F` to VA `0xC1E0A6` / raw case `0x81D4A6`. It
allocates `0xC0` bytes, stores local DWORD `0x77` (role unknown), and calls
constructor `0x5260B0`. The constructor passes `0x6F` to helper `0x5A1090`,
installs vtable VA `0x1C670B8` (RTTI
`firefall::Character::ConfirmedPoseUpdate`), initializes WORD `+0x88` to
`0x0101`, and clears DWORD `+0xB0`.

Reader setup at `0x81D4D5` calls `0x5EFAB0`, which reads a record through
`0x5EF8D0` at `object+0x28` and then a WORD at `+0xB8`. The record reader
first reads a WORD at `+0xB4` and gate bytes at `+0xA6/+0xA7`. If bit `0x01`
of `+0xA6` is set, it reads DWORDs at `+0x2C/+0x30/+0x34`, a four-DWORD
block copied to `+0x78..+0x87`, and a WORD at `+0xA4`. It always reads
DWORDs at `+0x98/+0x9C/+0xA0`; bit `0x02` of `+0xA6` adds DWORDs at
`+0x8C/+0x90/+0x94`. It then reads WORDs at `+0xA8/+0xAA/+0xAC` and
processes the field at `+0xB0` through byte-gated nested reader `0x5FFB00`.
Allocation failure zeroes ESI before the same reader setup. Nested contents, field
meanings, and message semantics remain unresolved.

#### Selector 0x6F post-parse receiver

The outer handler switch maps selector `0x6F` through VA `0xC1D4B5` / raw
`0x81C8B5`, then calls slot `+0x140` at raw `0x81C8D4`. The protocol-handler
vtable entry at raw `0x1870C00` contains VA `0xECE400`, mapped to receiver raw
`0xACD800`.

The receiver creates an event payload and appends a typed value of kind `5`
using message DWORDs at `+0x28/+0x2C`. If the DWORD at `+0x30` is nonzero, it
looks it up in registry `0x1E5A4F0`; a usable entry is converted to text by
`0x13122D0` and appended. It dispatches event `239` through the object at
global `0x1E002B8`, vtable slot `+0xAC`, then releases the payload.

The parser requests `0xC0` bytes; all receiver reads described here are within
that allocation. The relationship between the receiver's three DWORD view
and the parser's conditional record layout, plus the event's meaning, remains
unresolved.

### Selector 0x70: `PublicDebugMovementUpdate`

Entry29 maps selector `0x70` to VA `0xC1E0FA` / raw case `0x81D4FA`. The case
allocates `0x5A0` bytes, stores local DWORD `0x78` (role unknown), and calls
constructor `0x526180`. The constructor passes `0x70` to helper `0x5A1100`,
installs vtable VA `0x1C670A8` (RTTI
`firefall::Character::PublicDebugMovementUpdate`), then runs initializer
`0x60E6A0`.

The allocation is exactly a `0x28`-byte object prefix followed by seven
`0xC8`-byte records. In each record, the reader consumes a WORD at `+0x00`;
uses the record at `+0x04` for three DWORDs at record `+0x04/+0x08/+0x0C`,
a 16-byte block at `+0x50`, a WORD set to 1 at `+0x60`, and three DWORDs at
`+0x64/+0x68/+0x6C`; consumes another WORD at `+0x70`; and reads a gate byte
at `+0x74` before a conditional helper-managed nested value. The constructor
initializes each record's WORD `+0x60` to `0x0101` and gate byte `+0x74` to 0;
the reader overwrites the WORD with 1. Record starts are object offsets
`+0x28 + i*0xC8`, for `i=0..6`. The gate's variable payload and field meanings
remain unresolved. Allocation failure zeroes ESI before entering the shared
reader setup; the consequences of that path are not inferred.

The route-0 receiver dispatches through slot `+0x144` (table VA
`0xC1D6DC`, raw table `0x81CADC`) to VA `0xECEBF0` / raw function
`0xACDFF0`. Its switch case is VA `0xC1D4E0` / raw `0x81C8E0`, with the
indirect call through slot `+0x144` at raw `0x81C8FF`. The receiver appends a
kind-5 typed value from the DWORDs at message `+0x28/+0x2C` through helper
`0xC245A0`. It then treats the DWORD at message `+0x30` as a nested object's
vtable pointer, calls slot `+0x0C`, and, for a nonzero result, calls slot
`+0x18` with a pointer to `unk_1B53F77` and value 1 before passing that object
to helper `0xC24780`. Finally it dispatches event 363 through
`uiManager` global VA `0x1E002B8`, slot `+0xAC`. The message `+0x30` location
falls inside the parser's first record; the relationship between that record
and the receiver's object view is not established, nor are the nested object's
semantics or event payload meaning.

### Selector 0x71: `ForcedMovement`

Entry30 maps selector `0x71` to VA `0xC1E14F` / raw case `0x81D54F`. It
allocates `0x1E8` bytes and writes stack local `[esp+0x20]=0x79` at raw
`0x81D560` (role unknown; not the message discriminator), then calls
constructor `0x526230`. The constructor passes `0x71` to helper `0x5A1170`,
installs vtable VA `0x1C67098` (RTTI
`firefall::Character::ForcedMovement`), and initializes the nested state at
object `+0x28` through `0x92B8A0`.

The initialized nested state spans `+0x28..+0x1DF`. Constructor writes are:
zero DWORDs at `+0x28/+0x2C/+0x30`, `+0x114/+0x118`, `+0x138/+0x13C`,
and `+0x1B8/+0x1BC/+0x1C0/+0x1C4`; WORDs `+0x34=0`, `+0x94=0x0101`,
`+0x100=0x0101`, `+0x110=0`, `+0x19C=0x0101`, and `+0x1CC=0`.
Reader `0x5EF690` invokes `0x92F800` on the nested state, then reads a
separate WORD at object `+0x1E0`. The nested reader reads a DWORD at `+0x2C`,
a discriminator byte at `+0x30`, a conditional pair at `+0x138/+0x13C`,
and dispatches 14 discriminator cases. The case-specific tail layouts and
field meanings remain unresolved. Allocation failure zeroes ESI before the
reader setup.

The route-0 receiver dispatches through slot `+0x148` (table VA `0xC1D6DC`,
raw table `0x81CADC`) to VA `0xECED30` / raw function `0xACE130`. Its switch
case is VA `0xC1D50B` / raw `0x81C90B`, with the indirect call through slot
`+0x148` at raw `0x81C92A`. The receiver passes a pointer to the nested state
at message `+0x28` to helper `0xC24780` with label `byte_1BAEDDC` and value 1,
then dispatches event 365 through `uiManager` global VA `0x1E002B8`, slot
`+0xAC`. The receiver does not directly inspect individual state fields;
interpretation of the state and event remains unresolved.

### Selector 0x72: `ForcedMovementCancelled`

Entry31 maps selector `0x72` to VA `0xC1E1A3` / raw case `0x81D5A3`. It
allocates `0x30` bytes, stores local DWORD `0x7A` (role unknown), and calls
constructor `0x5262C0`. The constructor passes `0x72` to helper `0x5A11E0`
and installs vtable VA `0x1C67078` (RTTI
`firefall::Character::ForcedMovementCancelled`). Reader `0x5EF490` reads a
DWORD at object `+0x28` with `0x3370A0`, then a WORD at `+0x2C` with
`0x336AC0`. The derived constructor does not initialize those two fields.
Allocation failure zeroes ESI before the reader setup. Field meanings remain
unknown.

### Selector 0x73: `GrappleClimbPermission`

Entry32 maps selector `0x73` to VA `0xC1E1F4` / raw case `0x81D5F4`. It
allocates `0x1E0` bytes, stores local DWORD `0x7B` (role unknown), and calls
constructor `0x526340`. The constructor passes `0x73` to helper `0x5A1250`,
installs vtable VA `0x1C67088` (RTTI
`firefall::Character::GrappleClimbPermission`), and initializes a `0x1B8`-byte
nested state at object `+0x28` through `0x92B8A0`. Its explicit initialized
DWORD and WORD offsets/values match selector `0x71`; the state reaches the end
of this allocation, with no trailing WORD.

The body reader is `0x92F800`, shared with selector `0x71`: it reads the
DWORD at nested `+0x04`, discriminator byte at `+0x08`, and conditional pair
at `+0x110/+0x114`, then dispatches 14 discriminator cases. Those nested
offsets correspond to object `+0x2C`, `+0x30`, and `+0x138/+0x13C`.
Case-specific tail layouts and field meanings remain unresolved. Allocation
failure zeroes ESI before the reader setup.

### Conditional receiver application for Character selectors 0x70–0x73

When callback routing selects the protocol handler, dispatcher raw `0x81BB90`
subtracts `0x20` from the message discriminator and indexes its selector table
at VA `0xC1D6DC` / raw
`0x81CADC`. For selectors `0x70`–`0x73`, the table targets dispatcher cases
at VA/raw pairs `0xC1D4E0/0x81C8E0`, `0xC1D50B/0x81C90B`,
`0xC1D536/0x81C936`, and `0xC1D561/0x81C961`. Each case calls
`tfGameProtocolHandler` at `4 + 4*(selector - 0x20)`, respectively slots
`+0x144`, `+0x148`, `+0x14C`, and `+0x150`. The vtable is VA `0x1C71EC0`,
mapped through the PE `.rdata` section to file offset `0x1870AC0`; the slot
entries resolve to handler VAs `0xECEBF0`, `0xECED30`, `0xECB670`, and
`0xECB6C0` (raw `.text` offsets `0xACDFF0`, `0xACE130`, `0xACAA70`, and
`0xACAAC0`, using VA = raw + `0x400C00`). RTTI is
`.?AVtfGameProtocolHandler@@`. These are receiver methods reached from the
dispatcher; selector RTTI alone is not used to infer their behavior.

| Selector | Receiver slot / raw handler | Observed application path |
|---|---|---|
| `0x70` | `+0x144` / `0xACDFF0` | Passes message DWORD reads at `+0x28/+0x2C` to `0xC245A0`. It then treats the DWORD at `+0x30` as a virtual object, probes slot `+0x0C`, and conditionally invokes slot `+0x18` and helper `0xC24780`. Dispatches numeric application/UI code `363` through global `0x1E002B8`, vtable slot `+0xAC`. |
| `0x71` | `+0x148` / `0xACE130` | Passes the address of message `+0x28` to `0xC24780`, whose first operation calls the vtable at the first DWORD's slot `+0x0C`; if active, it copies the following state. Dispatches numeric application/UI code `365` through the same manager slot. |
| `0x72` | `+0x14C` / `0xACAA70` | Gets an owner-state pointer at singleton `0x49DCC0` offset `+0x43C8` (diagnostic `1651` if null), then passes the address of message `+0x28` to list walker `0x9090A0`. It visits nodes, passes each node's DWORDs at `+0x10/+0x14` to updater `0x909120`, and has no numeric event dispatch in this receiver method. |
| `0x73` | `+0x150` / `0xACAAC0` | Gets an owner-state pointer at singleton `0x49DCC0` offset `+0x43B8` (diagnostic `1646` if null), then calls `0x8EDD10` with the DWORD at message `+0x28` and the address of `+0x2C`. That helper updates its `+0x28/+0x2C` state, merges list entries against existing 20-byte records, and dispatches numeric application/UI code `213` through manager slot `+0xAC`. Missing records may be materialized as 84-byte records from lookups; their semantics remain unknown. |

These receiver paths have verified producer-consumer layout discrepancies; no
conversion or repair step was found in the traced dispatch path:

* Selector `0x70` parses seven `0xC8`-stride records beginning at `+0x28`.
  The receiver consumes only the first record's `+0x28/+0x2C` locations as
  DWORDs (the parser writes a WORD at `+0x28`) and treats the serialized DWORD
  at `+0x30` as a virtual-object vtable pointer. The parser's reader writes
  raw record data there, not a vtable. Its optional gate is byte `+0x74` in
  each record; that gate controls the parser's nested-value reader and is
  distinct from the receiver's virtual-call result test.
* Selector `0x71` constructs a `0x1B8`-byte inline state at `+0x28` and
  initializes its first DWORD to zero. The reader consumes the discriminator
  byte at `+0x30` first, then the DWORD at `+0x2C`, and a 14-case tail. The receiver
  passes the address of `+0x28` to a helper that treats the DWORD stored there
  as a vtable pointer. It does not read the trailing WORD at `+0x1E0`.
* Selector `0x72` parses a DWORD at `+0x28` and WORD at `+0x2C`; the receiver
  treats the DWORD at `+0x28` as a pointer to a linked-list sentinel and
  ignores the WORD. No pointer conversion was observed.
* Selector `0x73` likewise initializes `+0x28` to zero and parses its nested
  state beginning with the discriminator byte at `+0x30`, then the DWORD at
  `+0x2C`; the receiver passes the value at
  `+0x28` as an application-state value and the address of `+0x2C` where the
  helper dereferences a linked-list head. No conversion was observed.

The byte-level mappings and call sites are checked against the original
executable. The pointer interpretations and downstream runtime consequences
remain unresolved; this record does not infer a corrected wire format.

The nested table at raw `0x821844` has 40 four-byte entries for selectors
`0x53` through `0x7A` inclusive; entry `0x7A` is the last one and points to
VA `0xC1E3EF` / raw `0x81D7EF`. The decoder's secondary dispatch subtracts
`0x20` and accepts values through `0x5A`, giving an inclusive upper bound of
`0x7A`; values above it take the default path.

### Selector 0x74: `AbilityActivated`

Entry33 maps selector `0x74` to VA `0xC1E249` / raw case `0x81D649`. The case
allocates `0x40` bytes, stores local DWORD `0x7C` (role unknown), and calls
constructor `0x5263D0`. The constructor passes selector `0x74` to helper
`0x5A12C0` and installs vtable VA `0x1C67068` (RTTI
`firefall::Character::AbilityActivated`). It initializes a 16-byte state at
object `+0x30`: DWORD `0x1C80CC4` followed by three zero DWORDs.

Reader `0x5EF1F0` reads DWORDs at `+0x28` and `+0x2C` through `0x3370A0`.
It then invokes virtual slot `+0x10` on the state at `+0x30`, passing the
remaining stream length and input pointer. The shared `slBlob` method resizes
the blob to that length without preserving prior contents and copies the
remaining bytes verbatim. Their semantic interpretation remains unknown. On
allocation failure, ESI is zeroed before the reader setup; runtime consequences
are not inferred.

### Selector 0x75: `AbilityFailed`

Entry34 maps selector `0x75` to VA `0xC1E29A` / raw case `0x81D69A`. It
allocates `0x40` bytes, stores local DWORD `0x7D` (role unknown), and calls
constructor `0x5264C0`. The constructor passes selector `0x75` to helper
`0x5A1330` and installs vtable VA `0x1C67058` (RTTI
`firefall::Character::AbilityFailed`). Its initialized 16-byte state at
`+0x30` matches selector `0x74`: DWORD `0x1C80CC4` followed by three zero
DWORDs.

Reader `0x5EF090` has the same observed field-access pattern as `0x5EF1F0`:
DWORDs at `+0x28` and `+0x2C` through `0x3370A0`, then a virtual slot `+0x10`
call on `+0x30` using the remaining stream length. That slot resolves to the
shared `slBlob` reader, which resizes and copies the remaining bytes verbatim.
Shared layout and reader shape do not establish matching event semantics. The
blob's semantic meaning and allocation-failure consequences remain unresolved.

### Selector 0x76: `AbilityCooldowns`

Entry35 maps selector `0x76` to VA `0xC1E2EB` / raw case `0x81D6EB`. It
allocates `0x38` bytes, stores local DWORD `0x7E` (role unknown), and calls
constructor `0x5265B0`. The constructor passes selector `0x76` to helper
`0x5A13C0`, installs vtable VA `0x1C67038` (RTTI
`firefall::Character::AbilityCooldowns`), and initializes a 16-byte state at
object `+0x28` with DWORD `0x1C80CC4` followed by three zero DWORDs.

Reader `0x5EEE60` passes the `slBlob` state at `+0x28` to its virtual slot
`+0x10` with the remaining stream length and input pointer. The slot resizes
and copies the remaining bytes verbatim, after which the reader advances past
those bytes. In the shared `slBlob` reader at raw `0x12CB9D0`, the first DWORD
of the blob object is used as its vtable and the second DWORD (`+4`) is loaded
as a storage-object pointer before copying from the storage object's `+4`
data field. The `AbilityCooldowns` receiver instead computes
`(message[+0x2C] - message[+0x28]) >> 6` and walks records beginning at
`message+0x28`; those positions are respectively the initialized `slBlob`
vtable and its storage-object pointer in the parser's layout. The parsed
pointer reaches the receiver unchanged on the traced callback route, and no
intervening conversion is established. This is a static layout/consumer
mismatch; runtime consequences are not established. The blob contents'
semantics and allocation failure consequences remain unresolved.

### Selector 0x77: `NPCInteraction`

Entry36 maps selector `0x77` to VA `0xC1E33C` / raw case `0x81D73C`. It
requests `0x30` bytes and writes stack local `[esp+0x20]=0x7F` at raw
`0x81D74A` (role unknown; not the message discriminator), then calls
constructor `0x5266A0` at raw `0x81D761`. The constructor passes selector
`0x77` to base helper `0x5A1430`, installs vtable VA `0x1C67048` (RTTI
`firefall::Character::NPCInteraction`), and zero-initializes DWORDs at
`+0x28` and `+0x2C`. The base helper's last object stores are at `+0x20` and
`+0x24`; together, base plus derived construction writes through exclusive
`+0x30`. This describes observed writes, not allocator capacity.

The success path jumps to shared reader setup at raw `0x81F72E`, which reads
one raw QWORD into `+0x28` via `0x356F80`; it copies eight bytes and advances
the input stream by eight, ending at exclusive `+0x30`. The parser carries
the constructor's EAX pointer in ESI (`0x81D766`), passes `ESI+0x28` to the
reader, then restores EAX from ESI at `0x81F741`; no conversion or write past
the QWORD is visible. The object's vtable writer (`0x5EED50`) and virtual
reader (`0x5EECE0`) likewise serialize/deserialize only the raw QWORD at
`+0x28`; neither populates `+0x30`. The allocation-failure branch reaches
`0x81F72C`, zeroes ESI, and joins the same setup. The resulting failure-path
address calculation is recorded as observed; its runtime consequence and the
QWORD's field meanings remain unknown.

### Conditional `tfGameProtocolHandler` application for selectors 0x74–0x77

For messages that take the `0x81BB90` callback branch, the recipient is the
concrete `tfGameProtocolHandler` constructed at raw
`0xAC8CA0`; its constructor writes vtable VA `0x1C71EC0` at raw `.rdata`
`0x1870AC0`. The RTTI-backed table bytes and all four selected method pointers
were checked in the requested original. `.text` code addresses map to raw file
offsets by subtracting `0x400C00`; the receiver vtable resides in `.rdata` and
is translated by the parsed PE section table.

Dispatcher `0x81BB90` subtracts `0x20` and bounds the discriminator at `0x7A`.
Its jump table at VA `0xC1D6DC` / raw `0x81CADC` reaches these methods and
issues the recorded virtual calls:

| Selector | Switch target VA / raw | Receiver slot | Method VA / raw |
| --- | --- | --- | --- |
| `0x74` | `0xC1D58C` / `0x81C98C` | `+0x154` | `0xECE940` / `0xACDD40` |
| `0x75` | `0xC1D5B7` / `0x81C9B7` | `+0x158` | `0xECE9E0` / `0xACDDE0` |
| `0x76` | `0xC1D5E2` / `0x81C9E2` | `+0x15C` | `0xECEE10` / `0xACE210` |
| `0x77` | `0xC1D60D` / `0x81CA0D` | `+0x160` | `0xECEEA0` / `0xACE2A0` |

The slots agree with `4 + 4*(selector - 0x20)` and with the neighbouring
dialog slots already verified at `0x78`–`0x7A`.

* **`0x74 AbilityActivated` → `0xACDD40`.** The method calls accessor
  `0x49DCC0`, then uses owner field `+0x43C8`; a missing field reaches an
  assertion call (code `0x673`) but is not followed by a null-return branch.
  It hashes/compares the key at message `+0x28` in the map at owner-state
  `+0x2C`, inserting a record when absent. The first two DWORDs at message
  `+0x30/+0x34` are copied to the selected record `+0x18/+0x1C`; these remain
  opaque state words. Helper `0x909B40` builds a tag-`1` application payload
  using the record value and calls the global `uiManager` (`0x1E002B8`)
  dispatch slot `+0xAC` with numeric code `230` and null dispatch arguments.
  No event name is assigned to `230`.
* **`0x75 AbilityFailed` → `0xACDDE0`.** It uses the same accessor and asserted
  owner field `+0x43C8`, then passes the *addresses* of message fields `+0x28`
  and `+0x30` to `0x909C60`. That helper resets/initializes a per-owner tree,
  passes `**(+0x28)` and `*(+0x28)` as begin/end to `0x90C580`. The latter
  pointer is also used as the tree-header sentinel; each source node's pair at
  `+0x10/+0x14` is copied by `0x82C2E0` and inserted into the fresh owner tree
  through `0x8410B0`. Separately, the helper dereferences message `+0x30` and
  treats that value as a tree-header pointer: it loads the header's first
  pointer and compares it with the header sentinel before traversing nodes.
  This conflicts with the parser, which initializes message `+0x30` as an
  embedded `slBlob` whose first DWORD is vtable VA `0x1C80CC4`; no intervening
  conversion is established. Source/destination field meanings and runtime
  consequence remain unknown. No `uiManager` event dispatch was observed in
  this helper.
* **`0x76 AbilityCooldowns` → `0xACE210`.** The receiver treats state
  `+0x28/+0x2C` as range endpoints, computes `(end-begin)/0x40`, and walks
  `0x40`-byte records. It forwards each record's QWORD `+0`, nullable strings
  `+0x8/+0xC` (empty-string fallback), bytes `+0x10/+0x11/+0x12`, and address
  `+0x14` to `0x6E2590`. For a zero byte at `+0x14`, `0x6E2590` selects a
  native channel label from byte `+0x10`, lowercases it, and passes the
  `+0x0C` string as notification text, `+0x08` as author, the QWORD at `+0`
  as author ID, and byte `+0x11` as display flags to `0x6E3460`. That helper
  applies the channel gates documented in the chat-display section, emits
  runtime event `335` through `uiManager`, and writes the chat file log. Its
  byte-keyed payload properties are independently identified there; byte
  `+0x11` contributes the five documented low-byte flags. The `+0x12` byte is
  not used by this zero-byte branch.

  A nonzero byte at `+0x14` instead requires a current chat and calls
  `0x6E3670` with the record, `+0x08` string, byte `+0x10`, byte `+0x12`, and
  the object at `+0x18`. The helper obtains a data pointer and length through
  that object's storage object and virtual `+0x0C` method, then switches on
  byte `+0x12`; the 19 case targets and selected outputs are documented below.
  These traces establish consumer behavior, not record semantics or cooldown
  timing. Separately, the parser/receiver state mismatch above remains
  unresolved.
* **`0x77 NPCInteraction` → `0xACE2A0`.** It returns without dispatch if the
  `0x49DDD0` root accessor is null or root `+0x4D20` is null; otherwise
  `0x49DCC0` supplies the owner state. The QWORD at message `+0x28` is passed
  as two DWORD key components to helper `0x4C9250`, which looks up/inserts a
  record in owner-state map `+0x4338`. The helper swaps `0x20` bytes between
  that record `+0x14` and the address message `+0x30`, stores the opaque
  two-DWORD result of indirect helper `0x1AC48D8` at record `+0x38/+0x3C`, then
  builds a tag-`5` payload containing the key and dispatches numeric code `356`
  through `uiManager` slot `+0xAC`. Neither the QWORD's identity nor code
  `356`'s event name is established.

The nested parser returns its ESI object pointer in EAX; its known direct
callers (`0x821BA0` and `0x821DE0`) return that result, and the registered
callback path queues the returned pointer value. When message `+0x18`'s low
byte is zero, `+0x10` does not match the nested sentinel, and callback-context
`+8` is non-null, the message pointer is not replaced on the receiving path:
callback dispatch loads it from `[EDI]` at raw `0x7FDC6E`, pushes that value
into `0x81BB90` at `0x7FDD4F`, and
`0x81BB90` loads its argument into EDI at `0x81BB95`. The selector-`0x77` case
pushes that same EDI value at `0x81CA2B` before the virtual call through
receiver slot `+0x160` (`0x81CA2C`). The receiver's vtable pointer at VA
`0x1C71EC0` / raw `.rdata` `0x1870AC0` has
`0xECEEA0` in that slot, mapping to handler raw `0xACE2A0`. The handler loads
its message argument from `[EBP+8]`, pushes its DWORDs at `+0x2C` and `+0x28`,
and passes the same message address plus `0x30` to `0x4C9250`.

**Conditional extent caveat (unresolved):** if selector `0x77` takes the
`0x81BB90` callback branch, it requests `0x30` bytes and its
constructor plus parser reader write only through exclusive `+0x30`. The
receiver passes the one-past-requested pointer `message+0x30` to
`0x4C9250`; at `0x4C927E` it loads that first argument into EBX and at
`0x4C92B9` pushes the same EBX while setting ECX to map-record `+0x14`, then
calls `0x51C9D0`. That function swaps `0x20` bytes with map-record state,
covering message offsets `[+0x30,+0x50)`. Those bytes are not populated by
either the selector body reader or the class's virtual stream reader. A
direct-call scan of the requested original's `.text` finds one constructor
call (raw `0x81D761`), one call to `0x4C9250` (raw `0xACE2CC`), and one call to
`0x51C9D0` (raw `0x4C92BD`); IDA xrefs also connect the latter two only through
this handler/helper pair. The generic `NPCInteraction` writer and reader
touch only `+0x28`. No alternative producer or field-transforming parser path
was found in the analyzed route; this does not exclude an untraced indirect entry.
These facts establish a producer/consumer extent contradiction, not the
runtime allocation extent: the allocator may return slack, but actual block
capacity/ownership and the resulting runtime consequence remain unknown. Do
not equate the `0x30` request with actual block capacity.

### Selector 0x78: `OpenMovieDialog`

Entry37 maps selector `0x78` to VA `0xC1E36D` / raw case `0x81D76D`. It
allocates `0x30` bytes, stores local DWORD `0x80` (role unknown), and calls
constructor `0x526730`. The constructor passes selector `0x78` to helper
`0x5A14A0`, installs vtable VA `0x1C67028` (RTTI
`firefall::Character::OpenMovieDialog`), and zero-initializes DWORD `+0x28`.

The success path joins shared reader setup at raw `0x81FB27`; allocation
failure first zeroes ESI at `0x81FB25` before joining it. The setup passes
`+0x28` to reader `0x12EE010`, which consumes a NUL-terminated byte sequence
into the shared text state. The meaning of the field and the allocation-failure
consequence remain unresolved.

### Selector 0x79: `PrivateDialog`

Entry38 maps selector `0x79` to VA `0xC1E39E` / raw case `0x81D79E`. It
allocates `0x40` bytes and writes stack local `[esp+0x20]=0x81` at raw
`0x81D7AC` (role unknown; not the message discriminator), then calls
constructor `0x526840`. The constructor passes selector `0x79` to helper
`0x5A1510`, installs vtable VA `0x1C67018` (RTTI
`firefall::Character::PrivateDialog`), and zero-initializes DWORDs at `+0x30`
and `+0x34`.

Reader `0x5EE900` reads a DWORD at `+0x28`, a raw QWORD at `+0x30`, and a
DWORD at `+0x38`, using readers `0x3370A0`, `0x356F80`, and `0x3370A0`
respectively; the last read ends at `+0x3C` (exclusive). The allocation-failure
branch zeroes ESI before reaching this reader; its runtime consequence and the
fields' meanings remain unresolved.

The conditional application-receiver route indicates a native extent
disagreement rather than a pointer conversion. The selector case at raw
`0x81D79E` requests
`0x40` bytes from `0x12B7FB0`, constructs `PrivateDialog` with `0x526840`, and
reads the body through `0x5EE900`. The registered parser returns its object
pointer in EAX; the caller at `0x821BA0` returns that pointer unchanged, and
the ReplayReader result queue stores the same pointer. Whether a queued object
takes the protocol-handler branch depends on the envelope-key and nested
sentinel tests described below. On that branch, `0x7FDC60` loads the queued
pointer at `0x7FDC6E`, pushes it at `0x7FDD4E`, and calls `0x81BB90` at
`0x7FDD4F`. The `0x79` case at raw
`0x81CA63` pushes the same parser argument and calls receiver slot `+0x168`
at `0x81CA82`. That `tfGameProtocolHandler` vtable slot contains VA
`0xECEF00` (raw `.text` `0xACE300`), whose first instruction tail-jumps to
`0x991510` (VA `0xD92110`). No message-object copy, field expansion, or
adjustment appears on this route. The discriminator switch assigns distinct
slots with `4 + 4*(id - 0x20)`; an exact-pointer scan of the requested original
found the `0xECEF00` vtable target only once, at raw `.rdata` `0x1870C28`
(slot `+0x168`). There is no evidence this thunk is shared by another
discriminator in that receiver route.

At `0x991510`, the handler reads `success` from `+0x28`, `product_id` from
`+0x30/+0x34`, `price_id` from `+0x38/+0x3C`, `vendor_id` from `+0x40`, and a
four-byte code-string pointer from `+0x44`; the pointed-to C string is external
to the message object. Its fixed fields therefore extend through `+0x48`
(exclusive), eight bytes beyond the parser's requested `0x40`. The parser's
reader ends at `+0x3C`; it does not read the second price DWORD or either
receiver field at `+0x40/+0x44`. The concrete constructor zeroes `+0x30` and
`+0x34`; its base helper writes through `+0x24`. No observed constructor or
reader store initializes `+0x3C`, `+0x40`, or `+0x44`. On a successful
response with a null or empty code string, the handler passes the `+0x44` slot
by reference to its fallback-string helper.

The allocation helper forwards the requested size to backend `0x12C8A20`.
For a `0x40` request, that allocator's small-block calculation uses an
eight-byte header and a rounded `0x48` total chunk. When a selected free block
has less than `0x10` bytes remaining, it keeps the whole block rather than
splitting it; a returned payload can consequently have eight bytes of slack.
The specific free block selected is runtime heap state, so this does not prove
that this message has a `0x48`-byte payload area. Even if it does, the
`+0x3C/+0x40/+0x44` contents are not serialized or initialized by the traced
parser path. The parser/receiver mismatch remains unresolved; no larger
`PrivateDialog` layout is inferred.

### Selector 0x7A: `PublicDialog`

Entry39 maps selector `0x7A` to VA `0xC1E3EF` / raw case `0x81D7EF`. It
allocates `0x30` bytes, stores local DWORD `0x82` (role unknown), and calls
constructor `0x5268D0`. The constructor passes selector `0x7A` to helper
`0x5A1580` and installs vtable VA `0x1C66FF8` (RTTI
`firefall::Character::PublicDialog`).

Reader `0x5EE730` reads DWORDs at `+0x28` and `+0x2C` through `0x3370A0`.
The allocation-failure branch zeroes ESI before reaching this reader; its
runtime consequence and the fields' meanings remain unresolved.

### Message discriminator provenance cross-check

The four examples below trace constructor-written message `+0x10` values and
the conditional `0x81BB90` slot mapping only; they do not establish a global
selector namespace. In each parser case, the seemingly discriminator-like
DWORD is a stack local at `[esp+0x20]`, not a field in the allocated message:
selector `0x71` writes `0x79` at raw `0x81D560`, selector
`0x77` writes `0x7F` at `0x81D74A`, selector `0x79` writes `0x81` at
`0x81D7AC`, and route-3 `Vehicle::AbilityActivated` writes `0xD0` at
`0x81EF91`. Those local values remain semantically unassigned.

The constructors instead place their parser/message selector in the first
stack argument to the base helper, then that helper stores the argument at
message `+0x10`. For `Character::ForcedMovement` (`0x71`), `NPCInteraction`
(`0x77`), `PrivateDialog` (`0x79`), and `Vehicle::AbilityActivated` (`0x53`),
the constructor's stack immediate is respectively `0x71`, `0x77`, `0x79`,
and `0x53`; helper store instructions at `0x5A1191`, `0x5A1451`, `0x5A1531`,
and `0xB474D1` write that first helper argument to `[this+0x10]`. The helper
also writes a class-specific pointer to `+0x0C` (`0x1B5BDBC`, `0x1B5BD84`,
`0x1B5BD7C`, `0x1BB04E0`, respectively), copies the two constructor arguments
to `+0x18/+0x1C`, and zeros `+0x20/+0x24`; these other fields are not the
discriminator. The parser call sites pass source DWORDs `[ESI]` and
`[ESI+4]` as the constructor arguments. The helper temporarily installs its
common vptr `0x1C61B84` at `+0`, then its class-specific base vptr
(`0x1C65AF0`, `0x1C65AD0`, `0x1C65A70`, `0x1C72554`, respectively); each
derived constructor replaces that same primary vptr at `+0`. The four
concrete message vtables have RTTI Complete Object Locator offsets of zero.
For selector `0x71`, the separate state initializer
at `0x92B8A0` begins with a zero DWORD at message `+0x28`, not a secondary
vptr.

The receiver chain preserves the allocated message pointer: callback helper
`0x7FDC60` loads it from `[record]` into EBX at `0x7FDC6E`, loads the receiver
from callback-context `+8` at `0x7FDD47`, pushes the unchanged EBX at
`0x7FDD4E`, and calls `0x81BB90` at `0x7FDD4F`. The dispatcher loads its
message argument into EDI at `0x81BB95`, reads the DWORD at `[EDI+0x10]` at
`0x81BBB7`, subtracts `0x20`, bounds it through `0x7A`, and indexes the
receiver switch table. Each representative case sets ECX to the receiver and
pushes the unchanged EDI message pointer before its virtual call:

| Construction selector / stored discriminator | Parser stack local | Switch target / slot-call raw | Receiver slot | Vtable method VA / raw |
| --- | --- | --- | --- | --- |
| `0x71` Character `ForcedMovement` | `0x79` at `0x81D560` | `0x81C90B` / `0x81C92A` | `+0x148` | `0xECED30` / `0xACE130` |
| `0x77` Character `NPCInteraction` | `0x7F` at `0x81D74A` | `0x81CA0D` / `0x81CA2C` | `+0x160` | `0xECEEA0` / `0xACE2A0` |
| `0x79` Character `PrivateDialog` | `0x81` at `0x81D7AC` | `0x81CA63` / `0x81CA82` | `+0x168` | `0xECEF00` / `0xACE300` |
| `0x53` Vehicle `AbilityActivated` | `0xD0` at `0x81EF91` | `0x81C401` / `0x81C420` | `+0xD0` | `0xECB180` / `0xACA580` |

For messages that actually reach `0x81BB90`, the slot formula is
`4 + 4*(id-0x20)`. In particular, the `0xD0` parser stack local for Vehicle
`AbilityActivated` is not its discriminator; `0xD0` is that selector's
receiver-vtable byte offset, while the message field is `0x53`. The unchanged
message pointer and primary-vtable calls are proven only for this conditional
dispatcher path, not for every parser family.

### Parser-family collisions and callback route gate

The route-specific parser families below all construct different RTTI types
with the same value, `message+0x10 = 0x53`. The route-local stack DWORDs shown
are separate parser temporaries, not namespace IDs. Character route 0 and
Vehicle route 3 use selector tables; AreaVisualData route 2, Deployable route
4, Turret route 5, and LootStoreExtension route 6 branch directly in their
route dispatchers.

| Parser route | Route dispatcher / selector table | Case raw offset | Message class | Allocation | Parser stack local | Constructor → base helper | `+0x0C` metadata pointer | Primary vtable / RTTI |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Character nested route 0 | `0x81CCAD` / VA `0xC22444` | `0x81CCC3` | `Character::MarketRequestComplete` | `0x48` | `0x5B` at `0x81CCD1` | `0x524C30` → `0x5A0400` | `0x1B5BEAC` (`msg-83…`) | `0x1C67278` / `.?AVMarketRequestComplete@Character@firefall@@` |
| AreaVisualData route 2 | `0x81EE63` / direct case | `0x81EF1C` | `AreaVisualData::LootObjectCollected` | `0x40` | `0xCD` at `0x81EF2A` | `0xADBAA0` → `0xB07660` | `0x1BB0224` (`msg-83…`) | `0x1C723C0` / `.?AVLootObjectCollected@AreaVisualData@firefall@@` |
| Vehicle route 3 | `0x81EF6D` / VA `0xC2260C` | `0x81EF83` | `Vehicle::AbilityActivated` | `0x30` | `0xD0` at `0x81EF91` | `0xB093C0` → `0xB474B0` | `0x1BB04E0` (`msg-83…`) | `0x1C72998` / `.?AVAbilityActivated@Vehicle@firefall@@` |
| Deployable route 4 | `0x81F206` / direct case | `0x81F29E` | `Deployable::TookHit` | `0x60` | `0xD9` at `0x81F2AC` | `0xB27E80` → `0xB47C40` | `0x1BB03C4` (`msg-83…`) | `0x1C727A8` / `.?AVTookHit@Deployable@firefall@@` |
| Turret route 5 | `0x81F2EF` / direct case | `0x81F2F9` | `Turret::WeaponProjectileFired` | `0x48` | `0xDC` at `0x81F307` | `0xB36520` → `0xB47D90` | `0x1BB033C` (`msg-83…`) | `0x1C726E8` / `.?AVWeaponProjectileFired@Turret@firefall@@` |
| LootStoreExtension route 6 | `0x81F34A` / direct case | `0x81F354` | `LootStoreExtension::LootObjectCollected` | `0x40` | `0xDD` at `0x81F362` | `0xB43A70` → `0xB48040` | `0x1BB0280` (`msg-83…`) | `0x1C72608` / `.?AVLootObjectCollected@LootStoreExtension@firefall@@` |

The base helpers temporarily install a common vptr and then a class-specific
base vptr at `+0x00`; the derived constructors replace that with each
route-specific primary vptr. The helpers initialize `+0x08` to 1, store the
route-specific metadata pointer at `+0x0C`, store the compact parser selector
at `+0x10`, copy the stream QWORD to `+0x18/+0x1C`, and zero `+0x20/+0x24`.
They do not write `+0x14` in the observed common initialization sequence; its
meaning is unresolved. The metadata pointers differ, but their strings all
begin `msg-83`; neither that text nor the pointer is proven to be the runtime
namespace selector. The distinct derived RTTI/vptrs and parser-route cases
establish the parser family. A C# snapshot and byte checks for these
constructors and fields live in
`MatrixMessageNamespaceRoutingNativeLayout.cs` and
`StartupBinaryEvidence.cs`.

The stream parser at raw `0x821BA0` reads the envelope QWORD before the compact
selector and passes the QWORD address into `0x81CC50`; each route-specific
constructor passes its two source DWORDs to the base helper, which copies them
to `+0x18/+0x1C`. `tfGameSocketHandler` callback `0x7FDC60` first tests only
the low byte at `message+0x18`. If nonzero, it calls `0x800420`, which consumes
the full `+0x18/+0x1C` key through lookup helper `0x800070` and the socket
handler registries at `+0x24` and `+0x2C`. If the low byte is zero, it compares
`message+0x10` with the nested-message sentinel at `0x1DFC030`; a match takes
the nested path. Otherwise, a non-null callback-context `+8` receiver is
passed the unchanged message pointer at `0x81BB90`. The actual QWORD values
are runtime input and no sample stream was supplied, so none of the six
classes can be assigned to one of these callback branches statically.

The nonzero-key branch is backed by two ordered, QWORD-keyed trees embedded in
the `tfGameSocketHandler` object. At raw `0x7FD670`, its constructor calls
`0x805130` twice and stores the resulting tree headers at `+0x24` and `+0x2C`;
the same constructor stores the factory pointer at `+0x14` (VA `0xC1BDB0` /
raw `0x81B1B0`). `0x800420` copies the message selector and both key DWORDs,
then passes the full key to `0x800070`. That helper searches `this+0x24` and
`this+0x2C` through `0x8013B0`; the tree search compares the key with node
`+0x10/+0x14`, and the mapped pointer is at node `+0x18`. On a first-tree
miss, after the `0x800BF0` creation checks allow it, the callback at context
`+0x14` is called with the QWORD; its dispatcher `0x81B1B0` accepts low-byte
values `0x02..0x35`; the full 52-entry selector mapping is resolved below.
Entry creation uses `0x806910` and tree insertion uses `0x8064C0`; a separate
guarded path at `0x800300` inserts a QWORD-keyed entry into the `+0x2C` tree.
These observations establish the keyed lookup and registration mechanism, not
the meaning of an individual factory selector. The mapped construction cases
are recorded below using their constructors, installed primary vtables, RTTI,
and subsequent receiver call path.

### QWORD-keyed receiver factory: selectors 0x02..0x35

Factory `0x81B1B0` receives the key used by the `0x800070` registry-miss path.
It reads only the low byte of that argument for table selection:
`movzx eax, al; add eax, -2; cmp eax, 0x33; ja 0x81BA93; jmp
[0xC1C6A8+eax*4]`. Thus the table has 52 entries and accepts low-byte values
`0x02..0x35`; table VA `0xC1C6A8` maps to raw `0x81BAA8`. The low byte is a
factory selector, not evidence by itself of a message namespace. The full
QWORD remains the registry key at message `+0x18/+0x1C`. `0x800420` obtains the
registered object through `0x800070`, then uses the returned pointer as the
receiver in its compact-selector dispatch.

The table index is exactly `selector - 2`: selector `0x02` maps to index 0 and
`0x35` to index `0x33`; selector `0x01` underflows the unsigned bound and
`0x36` maps to `0x34`, so both take the out-of-range return. Within the table,
selectors `0x0F` and `0x11` also point to that same zero-return target
`0x81BA93` / VA `0xC1C693`; an in-range selector therefore does not imply
construction.

Each constructing case allocates the listed size through `0x12B7FB0`, calls
its constructor, and the constructor installs the listed RTTI-backed primary
vtable. Text targets use the verified raw-to-VA delta `+0x400C00`; vtable file
offsets use the original PE section map (`.rdata`), not that `.text` delta.
The `.rdata` mapping yields each vtable file offset, and the preceding pointer
resolves its Complete Object Locator (COL) and RTTI name.

| Key low byte | Case raw / VA | Allocation | Constructor raw | Primary vtable VA / raw | Primary slots `+04 / +08 / +0C / +18` (raw) | RTTI |
| --- | --- | ---: | --- | --- | --- | --- |
| `02` | `81B1F2 / C1BDF2` | `B80` | `53F980` | `1C66080 / 1864C80` | `540380 / 541220 / 5441B0 / 54DE00` | `.?AVBaseController@Character@firefall@@` |
| `03` | `81B230 / C1BE30` | `D80` | `54E7B0` | `1C65F1C / 1864B1C` | `54F4A0 / 5503D0 / 5536C0 / 55E160` | `.?AVNPCController@Character@firefall@@` |
| `04` | `81B26E / C1BE6E` | `11D8` | `55E830` | `1C65EF8 / 1864AF8` | `55EAA0 / 55EB10 / 561CF0 / 56B310` | `.?AVMissionAndMarkerController@Character@firefall@@` |
| `05` | `81B2AC / C1BEAC` | `12C0` | `56B970` | `1C65ED4 / 1864AD4` | `56BC20 / 56C0D0 / 56F3A0 / 576C80` | `.?AVCombatController@Character@firefall@@` |
| `06` | `81B2EA / C1BEEA` | `538` | `577320` | `1C65E8C / 1864A8C` | `577410 / 577480 / 5795A0 / 57FBD0` | `.?AVLocalEffectsController@Character@firefall@@` |
| `07` | `81B328 / C1BF28` | `A0` | `57FF80` | `1C65EB0 / 1864AB0` | `5800E0 / 580150 / 580470 / 580F40` | `.?AVSpectatorController@Character@firefall@@` |
| `08` | `81B366 / C1BF66` | `4F8` | `581070` | `1C65E2C / 1864A2C` | `581480 / 5817D0 / 5835C0 / 588500` | `.?AVObserverView@Character@firefall@@` |
| `09` | `81B3A4 / C1BFA4` | `420` | `588830` | `1C65DF4 / 18649F4` | `5889F0 / 588A90 / 589040 / 58A440` | `.?AVEquipmentView@Character@firefall@@` |
| `0A` | `81B3E2 / C1BFE2` | `58` | `58A530` | `1C65DAC / 18649AC` | `58A620 / 58A6E0 / 58AA90 / 58B4C0` | `.?AVAIObserverView@Character@firefall@@` |
| `0B` | `81B41D / C1C01D` | `1258` | `58B5F0` | `1C65D88 / 1864988` | `58B850 / 58BA50 / 58EA40 / 595090` | `.?AVCombatView@Character@firefall@@` |
| `0C` | `81B45B / C1C05B` | `A8` | `595680` | `1C65D58 / 1864958` | `595760 / 595820 / 595E30 / 596250` | `.?AVMovementView@Character@firefall@@` |
| `0D` | `81B499 / C1C099` | `438` | `5962A0` | `1C65D10 / 1864910` | `5963D0 / 596470 / 597590 / 59ACA0` | `.?AVTinyObjectView@Character@firefall@@` |
| `0E` | `81B4D7 / C1C0D7` | `1F8` | `59AEB0` | `1C65D34 / 1864934` | `59AFC0 / 59B060 / 59C600 / 59FF40` | `.?AVDynamicProjectileView@Character@firefall@@` |
| `10` | `81B515 / C1C115` | `88` | `AD9580` | `1C723D8 / 1870FD8` | `AD9790 / AD9830 / AD9BD0 / ADA370` | `.?AVObserverView@Melding@firefall@@` |
| `12` | `81B553 / C1C153` | `58` | `ADA400` | `1C7239C / 1870F9C` | `ADA4D0 / ADA570 / ADAB00 / ADBA10` | `.?AVObserverView@MeldingBubble@firefall@@` |
| `14` | `81B58E / C1C18E` | `158` | `ADBCE0` | `1C72378 / 1870F78` | `ADBE80 / ADBF40 / ADD790 / AE0EF0` | `.?AVObserverView@AreaVisualData@firefall@@` |
| `15` | `81B5CC / C1C1CC` | `628` | `AE10D0` | `1C72334 / 1870F34` | `AE1280 / AE1340 / AE1E80 / AE4170` | `.?AVParticleEffectsView@AreaVisualData@firefall@@` |
| `16` | `81B60A / C1C20A` | `760` | `AE42E0` | `1C722EC / 1870EEC` | `AE44A0 / AE4540 / AE5970 / AE9410` | `.?AVMapMarkerView@AreaVisualData@firefall@@` |
| `17` | `81B648 / C1C248` | `438` | `AE9640` | `1C72310 / 1870F10` | `AE9770 / AE9810 / AEA930 / AEE040` | `.?AVTinyObjectView@AreaVisualData@firefall@@` |
| `18` | `81B686 / C1C286` | `758` | `AEE240` | `1C722C8 / 1870EC8` | `AEE3F0 / AEE4B0 / AEF200 / AF1A60` | `.?AVLootObjectView@AreaVisualData@firefall@@` |
| `19` | `81B6C4 / C1C2C4` | `5C8` | `AF1C00` | `1C722A4 / 1870EA4` | `AF1D30 / AF1DD0 / AF3170 / AF68C0` | `.?AVForceShieldView@AreaVisualData@firefall@@` |
| `1B` | `81B702 / C1C302` | `570` | `B0A990` | `1C72868 / 1871468` | `B0AD20 / B0ADB0 / B0CE40 / B120B0` | `.?AVBaseController@Vehicle@firefall@@` |
| `1C` | `81B740 / C1C340` | `1120` | `B12490` | `1C72824 / 1871424` | `B12650 / B127E0 / B14E20 / B18EB0` | `.?AVCombatController@Vehicle@firefall@@` |
| `1D` | `81B77E / C1C37E` | `478` | `B19380` | `1C727E8 / 18713E8` | `B196E0 / B197A0 / B1B630 / B20100` | `.?AVObserverView@Vehicle@firefall@@` |
| `1E` | `81B7BC / C1C3BC` | `10F8` | `B204A0` | `1C727C4 / 18713C4` | `B20660 / B20720 / B22A80 / B26990` | `.?AVCombatView@Vehicle@firefall@@` |
| `1F` | `81B7FA / C1C3FA` | `B8` | `B26D40` | `1C72784 / 1871384` | `B26E50 / B26F20 / B27670 / B27E20` | `.?AVMovementView@Vehicle@firefall@@` |
| `21` | `81B838 / C1C438` | `78` | `AF6AE0` | `1C72254 / 1870E54` | `AF6C40 / AF6CE0 / AF6E50 / AF7260` | `.?AVAIObserverView@Anchor@firefall@@` |
| `23` | `81B873 / C1C473` | `1710` | `B281A0` | `1C72730 / 1871330` | `B285C0 / B286B0 / B2BCF0 / B34910` | `.?AVObserverView@Deployable@firefall@@` |
| `24` | `81B8B1 / C1C4B1` | `48` | `B34FC0` | `1C72700 / 1871300` | `B35130 / B35200 / B354B0 / B35C40` | `.?AVNPCObserverView@Deployable@firefall@@` |
| `25` | `81B8EC / C1C4EC` | `48` | `B35CC0` | `1C726C4 / 18712C4` | `B35DB0 / B35E50 / B36010 / B364C0` | `.?AVHardpointView@Deployable@firefall@@` |
| `27` | `81B927 / C1C527` | `128` | `B37030` | `1C72648 / 1871248` | `B37220 / B37290 / B37AA0 / B397C0` | `.?AVBaseController@Turret@firefall@@` |
| `28` | `81B965 / C1C565` | `D8` | `B39900` | `1C72618 / 1871218` | `B39AA0 / B39B60 / B3A450 / B3BFA0` | `.?AVObserverView@Turret@firefall@@` |

Each constructor calls the shared helper at raw `0xBCAD70`. It stores the
key's low/high DWORDs at receiver offsets `+0x18/+0x1C`, initializes fields
at `+0x08`, `+0x10`, and `+0x24` to zero, copies its first argument to `+0x20`,
and first writes the helper's unnamed vtable pointer at VA `0x1C660B4` to
offset `+0`. The derived constructor then replaces that pointer with the
listed RTTI-backed vtable. The six added COL offsets for selectors
`0x1B..0x21` are all zero, and no secondary vptr stores were observed in
those constructor bodies. The five constructors at selectors `0x23`, `0x24`,
`0x25`, `0x27`, and `0x28` also call this helper; their vtables map through
`.rdata`, their RTTI TypeDescriptors resolve through the PE section table, and
all five COL subobject offsets are zero.

These are the eight dispatch entries for selectors `0x0A..0x11`; the default
rows have no allocator or constructor:

| Selector | Table index | Entry raw | Target VA / raw | Result |
| --- | ---: | --- | --- | --- |
| `0A` | 8 | `81BAC8` | `C1BFE2 / 81B3E2` | `AIObserverView@Character` |
| `0B` | 9 | `81BACC` | `C1C01D / 81B41D` | `CombatView@Character` |
| `0C` | 10 | `81BAD0` | `C1C05B / 81B45B` | `MovementView@Character` |
| `0D` | 11 | `81BAD4` | `C1C099 / 81B499` | `TinyObjectView@Character` |
| `0E` | 12 | `81BAD8` | `C1C0D7 / 81B4D7` | `DynamicProjectileView@Character` |
| `0F` | 13 | `81BADC` | `C1C693 / 81BA93` | shared zero-return path |
| `10` | 14 | `81BAE0` | `C1C115 / 81B515` | `ObserverView@Melding` |
| `11` | 15 | `81BAE4` | `C1C693 / 81BA93` | shared zero-return path |

The bounded continuation for selectors `0x12..0x19` is:

| Selector | Table index | Entry raw | Target VA / raw | Result |
| --- | ---: | --- | --- | --- |
| `12` | 16 | `81BAE8` | `C1C153 / 81B553` | `MeldingBubble::ObserverView` |
| `13` | 17 | `81BAEC` | `C1C693 / 81BA93` | shared zero-return path |
| `14` | 18 | `81BAF0` | `C1C18E / 81B58E` | `AreaVisualData::ObserverView` |
| `15` | 19 | `81BAF4` | `C1C1CC / 81B5CC` | `AreaVisualData::ParticleEffectsView` |
| `16` | 20 | `81BAF8` | `C1C20A / 81B60A` | `AreaVisualData::MapMarkerView` |
| `17` | 21 | `81BAFC` | `C1C248 / 81B648` | `AreaVisualData::TinyObjectView` |
| `18` | 22 | `81BB00` | `C1C286 / 81B686` | `AreaVisualData::LootObjectView` |
| `19` | 23 | `81BB04` | `C1C2C4 / 81B6C4` | `AreaVisualData::ForceShieldView` |

The eight entries for selectors `0x1A..0x21` are:

| Selector | Table index | Entry raw | Target VA / raw | Result |
| --- | ---: | --- | --- | --- |
| `1A` | 24 | `81BB08` | `C1C693 / 81BA93` | shared zero-return path |
| `1B` | 25 | `81BB0C` | `C1C302 / 81B702` | `Vehicle::BaseController` |
| `1C` | 26 | `81BB10` | `C1C340 / 81B740` | `Vehicle::CombatController` |
| `1D` | 27 | `81BB14` | `C1C37E / 81B77E` | `Vehicle::ObserverView` |
| `1E` | 28 | `81BB18` | `C1C3BC / 81B7BC` | `Vehicle::CombatView` |
| `1F` | 29 | `81BB1C` | `C1C3FA / 81B7FA` | `Vehicle::MovementView` |
| `20` | 30 | `81BB20` | `C1C693 / 81BA93` | shared zero-return path |
| `21` | 31 | `81BB24` | `C1C438 / 81B838` | `Anchor::AIObserverView` |

The eight entries for selectors `0x22..0x29` are:

| Selector | Table index | Entry raw | Target VA / raw | Result |
| --- | ---: | --- | --- | --- |
| `22` | 32 | `81BB28` | `C1C693 / 81BA93` | shared zero-return path |
| `23` | 33 | `81BB2C` | `C1C473 / 81B873` | `Deployable::ObserverView` |
| `24` | 34 | `81BB30` | `C1C4B1 / 81B8B1` | `Deployable::NPCObserverView` |
| `25` | 35 | `81BB34` | `C1C4EC / 81B8EC` | `Deployable::HardpointView` |
| `26` | 36 | `81BB38` | `C1C693 / 81BA93` | shared zero-return path |
| `27` | 37 | `81BB3C` | `C1C527 / 81B927` | `Turret::BaseController` |
| `28` | 38 | `81BB40` | `C1C565 / 81B965` | `Turret::ObserverView` |
| `29` | 39 | `81BB44` | `C1C693 / 81BA93` | shared zero-return path |

Selectors `0x1B..0x1F` install five RTTI-named `Vehicle` receiver types;
selector `0x21` installs `Anchor::AIObserverView`. Each constructor calls
`0xBCAD70` and writes the two key DWORDs at receiver `+0x18/+0x1C`; the
constructor's initial helper vptr at VA `0x1C660B4` is replaced by the
primary vtable listed above. Section-aware `.rdata` mapping resolves each
vtable's Complete Object Locator and TypeDescriptor; all six COL subobject
offsets are zero. This bounded snapshot records the installed primary vptr
and does not claim a hierarchy-wide inventory of secondary subobjects.

This family assignment comes from the concrete constructor-installed vtables
and PE-resolved RTTI names, not from treating the low byte as a namespace ID.
The earlier six additions include five RTTI-named `Character` views and one
`Melding::ObserverView`; the current seven include `MeldingBubble::ObserverView`
and six `AreaVisualData` views; the selector `0x1A..0x21` entries add five
`Vehicle` classes and one `Anchor` class. The latest five add three
`Deployable` classes and two `Turret` classes. These are receiver types, not
parsed-message types. This evidence also corrects the provisional “message factory”
interpretation: these objects are registered against QWORD keys, not created
as per-message parser objects.

The receiver's primary slots are reached through the separate compact selector
at message `+0x10`: selector 1 calls `+0x08`; 2 calls `+0x18`; 3 and 4 call
`+0x0C`; 7 and 8 call `+0x04`; and 9 calls `+0x18`. Selectors 5 and 6 share
the direct helper at `0x800300` and do not call one of these primary slots.
The `+0x04` methods for `BaseController`, `NPCController`, `CombatController`,
and `ObserverView` dispatch message IDs from `message+0x10` over respective
observed ranges `0x53..0xC4`, `0x53..0xC4`, `0x56..0xA8`, and `0x5B..0xAB`,
subject to a class-specific condition. For BaseController and NPCController,
the condition is the non-null virtual-receiver pointer at `+0x2C`, not a
Boolean state flag; the corresponding fields in the other two classes are not
classified here. They first forward to an optional nested object at receiver
`+0x10`; the dispatcher tables themselves are not decoded here. The `+0x04`
methods for `MissionAndMarkerController`,
`LocalEffectsController`, `SpectatorController`, and `EquipmentView` forward
to that optional nested object and conditionally emit the fixed
"this view doesn't have any valid events" message, without a message-ID
argument. These forwarding/logging defaults are not empty stubs and differ
from the dispatchers' invalid-message-ID reporting.

For the newly mapped slots, `Vehicle::BaseController +0x04` handles exactly
message ID `0x5B` when a receiver at `+0x2C` is present; it first forwards to
an optional nested receiver at `+0x10`, and its invalid-ID path calls
`VIBE_LogUnhandledMessageId` with `message+0x10`. The `+0x08` compact-selector
body handler consumes byte data and a length, dispatches byte values through
`0xC1`, and logs values above that bound; individual payload cases remain
unmapped. Its `+0x0C` body handler accepts data and length and may forward to a
nested receiver at `+0x10`, but its large body is not exhaustively mapped. The
`Vehicle::CombatController +0x04` method first forwards to an optional nested
receiver at `+0x10`, then handles message IDs `0x53..0x5A` when a receiver at
`+0x2C` is present; its invalid-ID path calls `VIBE_LogUnhandledMessageId`
with `message+0x10`. `Vehicle::ObserverView`, `CombatView`, and `MovementView`
handle only IDs `0x55`, `0x57`, and `0x56`, respectively. Their invalid IDs
also use the ID-bearing logger; each valid path separately emits
`"received %s"` with `"msg-85"`, `"msg-87"`, or `"msg-86"` and may forward to
a receiver at `+0x2C`. By contrast, `Anchor::AIObserverView` forwards to an
optional nested receiver and may emit the fixed
`"this view doesn't have any valid events"` text through
`VIBE_LogEmitWithArgList`; that log call carries no message ID. The fixed
diagnostic is distinct from an invalid-ID report. These observations describe
the slot code, not message semantics inferred from the class labels.

For the newly mapped `Deployable` and `Turret` receivers, the vtable `+0x04`
targets were read from the installed vtables and traced as actual method bodies.
`Deployable::ObserverView` dispatches only IDs `0x54` and `0x55`, logs
`"received %s"` with `"msg-84"` / `"msg-85"` on those valid paths, and calls
`VIBE_LogUnhandledMessageId` with the supplied ID otherwise.
`Deployable::NPCObserverView` and `Turret::ObserverView` each handle only
`0x53`, using `"msg-83"` on the valid path and the same ID-bearing invalid
logger otherwise. These three methods first forward through an optional
receiver at `+0x10`; when the event receiver at `+0x2C` is present, valid IDs
are forwarded to it. `Deployable::HardpointView` and `Turret::BaseController`
instead forward to the optional `+0x10` receiver and may emit the fixed
`"this view doesn't have any valid events"` diagnostic when `+0x2C` is set;
their method bodies do not inspect a message ID. All five slot `+0x18` methods
store the `dword_1AC4D24` result at receiver `+0x28` and tail-call
`0xBCADF0`. The compact dispatcher routes selectors 1 and 3/4 through slots
`+0x08` and `+0x0C`, respectively; the five receivers' methods at those slots
are mapped by address, while their payload semantics remain outside this
bounded pass.

For selectors `0x0A..0x0C`, the `+0x04` implementations are message-ID
dispatchers: `AIObserverView` handles `0x69`, `CombatView` has a table for
`0x54..0x6C`, and `MovementView` handles `0x6E`; their invalid-ID branches
call `VIBE_LogUnhandledMessageId` with `message+0x10`. The `0x69` and `0x6E`
valid branches separately log `"received %s"` with fixed `"msg-105"` /
`"msg-110"` text, then may forward to an event receiver at `+0x2C`; each also
forwards to a separate optional receiver at `+0x10` before the ID gate.
The new `AreaVisualData::ObserverView`, `ParticleEffectsView`, and
`LootObjectView` `+0x04` methods dispatch only `0x54`, `0x55`, and `0x53`,
respectively; their invalid-ID branches also report `message+0x10` through
`VIBE_LogUnhandledMessageId`, while their valid paths may forward to the
receiver at `+0x2C`. By contrast, the `+0x04` methods for `TinyObjectView`,
`DynamicProjectileView`, `Melding::ObserverView`,
`MeldingBubble::ObserverView`, `AreaVisualData::MapMarkerView`,
`AreaVisualData::TinyObjectView`, and `AreaVisualData::ForceShieldView`
forward to optional nested receivers and conditionally call
`VIBE_LogEmitWithArgList` with the fixed text
`"this view doesn't have any valid events"` without a message-ID argument.
Across the 36 constructing receivers mapped through selector `0x35`, `+0x04`
has nineteen message-ID dispatchers and seventeen forwarding/logging defaults.
The fixed no-events
diagnostics are not invalid-ID reports or empty stubs.

All thirty-six mapped `+0x18` targets are short state-refresh wrappers: they call
the indirect helper at `dword_1AC4D24`, store its result at receiver `+0x28`,
and call `orchestrator_BCADF0`. They ignore payload arguments at this entry
point but are not no-op stubs. The `+0x08` and `+0x0C` targets are compact-selector
body handlers reached with data pointers and lengths; their individual payload
fields remain outside this bounded pass. The
`AIObserverView +0x08` target at `0x58A6E0` has a recovered disassembly
boundary (size `0x394`, ending in `retn 8` at `0x58AA71`); decompilation and
payload semantics remain unresolved. The earlier targets at `0x5536C0` and
`0x5795A0` have a function-boundary limitation, so none are classified
as empty stubs.

The exact table, constructor/vtable/RTTI snapshots, compact-selector call
sites, and original-image fingerprints are kept separately in
`MatrixKeyedReceiverFactoryNativeLayout.cs`,
`MatrixMessageNamespaceRoutingNativeLayout.cs`, and
`StartupBinaryEvidence.cs`. All selectors `0x02..0x35` are now bounded:
36 entries construct RTTI-backed receivers and sixteen table entries share the
zero-return target `0x81BA93` / VA `0xC1C693`.

Each case calls allocator raw `0x12B7FB0` / VA `0x16B8BB0`; its listed
constructor then calls the shared base-key helper raw `0xBCAD70` / VA
`0xFCB970`, which stores the full key at `+0x18/+0x1C`. The final four
constructing cases are:

| Selector | Table entry raw | Case raw / VA | Allocation | Constructor raw / VA | Primary vtable VA / raw | COL VA / raw | TypeDescriptor VA / raw | RTTI |
| --- | --- | --- | ---: | --- | --- | --- | --- | --- |
| `2D` | `81BB54` | `81B9A3 / C1C5A3` | `100` | `AF7300 / EF7F00` | `1C72278 / 1870E78` | `1CCA910 / 18C9510` | `1E29EC0 / 1A282C0` | `.?AVObserverView@Outpost@firefall@@` |
| `30` | `81BB60` | `81B9E1 / C1C5E1` | `11A0` | `B3C0F0 / F3CCF0` | `1C725E4 / 18711E4` | `1CCBA18 / 18CA618` | `1E2A3F0 / 1A287F0` | `.?AVObserverView@ResourceNode@firefall@@` |
| `33` | `81BB6C` | `81BA1F / C1C61F` | `1690` | `AFC100 / EFCD00` | `1C721E0 / 1870DE0` | `1CCA8A0 / 18C94A0` | `1E29EEC / 1A282EC` | `.?AVObserverView@CarryableObject@firefall@@` |
| `35` | `81BB74` | `81BA59 / C1C659` | `758` | `B43B10 / F44710` | `1C72574 / 1871174` | `1CCBA04 / 18CA604` | `1E2A3B4 / 1A287B4` | `.?AVLootObjectView@LootStoreExtension@firefall@@` |

Each derived constructor calls the shared base-key helper `0xBCAD70` before
installing its primary vtable. Text methods use raw+`0x400C00` for VA
conversion; vtable and RTTI addresses use the original PE `.rdata` section map.
The COL subobject offset is zero for each. The four `+0x04` methods are at raw
`AF7420`, `B3C320`, `AFC430`, and `B43CC0`: the first three forward through an
optional receiver at `+0x10` and conditionally emit the fixed
`"this view doesn't have any valid events"` diagnostic, without consuming a
message ID. The `0x35` method accepts exactly message ID `0x53`; invalid IDs
call `VIBE_LogUnhandledMessageId` with the value at `message+0x10`, while the
valid route may log `"received %s"` with `"msg-83"` and forward to a nested
receiver at `+0x2C`.

For these four receivers the `+0x08/+0x0C/+0x18` raw targets are, respectively,
`AF74C0/AF8DC0/AFBBF0`, `B3C3C0/B3EAB0/B43640`,
`AFC4D0/AFF670/B070E0`, and `B43D80/B44AD0/B47330`. The `+0x08/+0x0C`
targets receive compact-body data pointers and lengths; their payload fields
and consumer tails remain unresolved. Each `+0x18` target stores the
`dword_1AC4D24` result at receiver `+0x28` and calls `0xBCADF0`.

The following matrix is the complete 52-entry table summary. A zero-return
entry targets `0x81BA93`; every other selector constructs the RTTI type shown.

| Selector | Factory result | Selector | Factory result | Selector | Factory result | Selector | Factory result |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `02` | `Character::BaseController` | `03` | `Character::NPCController` | `04` | `Character::MissionAndMarkerController` | `05` | `Character::CombatController` |
| `06` | `Character::LocalEffectsController` | `07` | `Character::SpectatorController` | `08` | `Character::ObserverView` | `09` | `Character::EquipmentView` |
| `0A` | `Character::AIObserverView` | `0B` | `Character::CombatView` | `0C` | `Character::MovementView` | `0D` | `Character::TinyObjectView` |
| `0E` | `Character::DynamicProjectileView` | `0F` | zero | `10` | `Melding::ObserverView` | `11` | zero |
| `12` | `MeldingBubble::ObserverView` | `13` | zero | `14` | `AreaVisualData::ObserverView` | `15` | `AreaVisualData::ParticleEffectsView` |
| `16` | `AreaVisualData::MapMarkerView` | `17` | `AreaVisualData::TinyObjectView` | `18` | `AreaVisualData::LootObjectView` | `19` | `AreaVisualData::ForceShieldView` |
| `1A` | zero | `1B` | `Vehicle::BaseController` | `1C` | `Vehicle::CombatController` | `1D` | `Vehicle::ObserverView` |
| `1E` | `Vehicle::CombatView` | `1F` | `Vehicle::MovementView` | `20` | zero | `21` | `Anchor::AIObserverView` |
| `22` | zero | `23` | `Deployable::ObserverView` | `24` | `Deployable::NPCObserverView` | `25` | `Deployable::HardpointView` |
| `26` | zero | `27` | `Turret::BaseController` | `28` | `Turret::ObserverView` | `29` | zero |
| `2A` | zero | `2B` | zero | `2C` | zero | `2D` | `Outpost::ObserverView` |
| `2E` | zero | `2F` | zero | `30` | `ResourceNode::ObserverView` | `31` | zero |
| `32` | zero | `33` | `CarryableObject::ObserverView` | `34` | zero | `35` | `LootStoreExtension::LootObjectView` |

The `0xBD7060` fan-out is a separate conditional side path: `0x7FDC60` reaches
it when the message predicate at `message+0x18` returns zero and context
`+0xB0` is non-null; its selector is `3` for a zero callback argument and `2`
otherwise. `0xBD7060` serializes the message QWORD and message data into an
output stream in this order: raw eight bytes from message `+0x18/+0x1C`, a
compact positive UInt16 encoding of message `+0x10`, then the message vtable's
slot-`+0` serialization. It then passes the selector separately to
`0xBD7460`. The latter stores that selector in the recipient callback's local
header at `0xBD7482`; it is not serialized into the stream. That function
iterates 32-bit recipient pointers in its owner's vector `[+0x2C,+0x30)`
(capacity at `+0x34`), stepping four bytes at a time. For each item it
performs an indirect `__thiscall` through vtable slot `+0x0C`, passing the
output-buffer pointer separately from an eight-byte local header: DWORD
`owner[+0x20]` at header `+0`, WORD output length at `+4`, selector byte at
`+6`, and zero byte at `+7`. The length is narrowed to WORD when the header
is populated. The exact concrete type of every vector entry is not established.

A representative registration path is visible at raw `0x7FE1C0`: it lazily
allocates a `0x48`-byte state object, calls `0xBD6E10`, appends a recipient
pointer to the state's `+0x2C` vector with `0xBD8C90`, then calls the
recipient's vtable slot `+4`. The constructor copies the caller's pair at
`this+0x0C` to state `+0x18/+0x1C`, stores its QWORD argument at `+0x08/+0x0C`,
and zeroes vector fields `+0x2C/+0x30/+0x34`. A concrete recipient on this
observed insertion path is `nl::DefaultReplayWriter`: `0x7FE360` constructs it
at `0xBD7EA0`, whose final vtable VA `0x1C73ADC` has matching RTTI; its slot
`+0x0C` resolves to VA `0xFD9010` / raw `0xBD8410`. That method checks the
record/input lengths through `0x12A0660` and returns a Boolean. This is a
representative recipient of the observed `0x7FE1C0` insertion path, not proof
that every `0xBD7460` vector or the `0x7FDC60` context's `+0xB0` state uses
that class: no direct call from `0x7FDC60` to `0x7FE1C0` was found, and the
shared `+0xB0` offset alone does not establish object identity. The
`context+8` route to `0x81BB90` remains separate; its concrete
`tfGameProtocolHandler` recipient and selector-`0x53` slot mapping below do
not identify the fan-out recipient type or message namespace.

The concrete `tfGameProtocolHandler` has primary vtable VA `0x1C71EC0`, with
COL VA `0x1CCA404` and object offset zero. Its RTTI hierarchy contains
`tfGameProtocolHandler` and `MessageHandler` base descriptors, both at
subobject offset zero; no secondary vtable group was observed for this class.
On the conditional `0x81BB90` path, selector `0x53` indexes slot `+0xD0`,
target VA `0xECB180` / raw `0xACA580`. That method reads `message+0x34`.
Consequently, a `Vehicle::AbilityActivated` object of allocated size `0x30`
would have an apparent extent mismatch if it reached this slot; the unknown
envelope-key branch means that runtime path has not been established. This is
an unresolved conditional observation, not a claim that the parser routes
Vehicle messages to that method.

The original executable (`FirefallClient.exe.original`, 40,197,848 bytes,
SHA-256 `E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235`)
was read directly; `.text` VAs are raw offsets plus `0x400C00`, and `.rdata`
references are translated using its PE section table. No native execution,
IDB edits, or sample edits were performed. This resolves the static routing
conditions and parser-family collisions; it does not repair or reinterpret
the already-recorded field/extent conflicts.

## Keyed Character receiver dispatch for IDs 0x77..0x7A

The QWORD-keyed factory table distinguishes the two Character receiver
classes: selector `0x02` installs `Character::BaseController`, whose primary
vtable `+0x04` target is raw `0x540380` (VA `0x940F80`); selector `0x03`
installs `Character::NPCController`, whose `+0x04` target is raw `0x54F4A0`
(VA `0x94F0A0`). Thus `0x54F4A0` is the NPC controller method, not the
selector-`0x02` BaseController method. These are factory selector identities;
this mapping does not establish which selector is used by a particular runtime
key.

For selector `0x02`, the factory case at raw `0x81B1F2` allocates `0xB80`
bytes and calls the constructor at `0x53F980` (`0x81B21B`). Immediately before
that call, it loads the new object into `ECX` and passes the two factory
arguments from `[ebp+0x08]` and `[ebp+0x0C]`; the registry-side indirect
factory call at `0x800151` forwards its two words from `[ebp+0x18]` and
`[ebp+0x1C]`. The selector-`0x03` factory case similarly allocates and
constructs the NPCController at `0x54E7B0`. These arguments establish
factory/key provenance, not a callback pointer.

The keyed envelope dispatcher at raw `0x800420` first obtains the registry
receiver from the QWORD at envelope `+0x18/+0x1C`. It uses envelope `+0x10`
for its compact selector; after decrementing the value, the `> 8` branch at
`0x800697` takes the default receiver `+0x04` call at `0x8008D3`. Therefore
the values `0x77..0x7A` reach the Character method through that default, with
the original envelope pointer unchanged. The receiver method then reads the
message ID at `message+0x10`. This is distinct from the direct compact
selector table's explicit `0x07/0x08` entries, which also call `+0x04`.

The per-class lookup bytes and targets are:

| Message ID | BaseController map / target | BaseController follow-up | NPCController map / target | NPCController follow-up |
| --- | --- | --- | --- | --- |
| `0x77` | map raw `0x5411C0`=`08`; target raw `0x540446` / VA `0x941046` | optional log, then callback object's vtable `+0x10` | map raw `0x550371`=`0A`; target raw `0x54F58F` / VA `0x95018F` | optional log, then callback object's vtable `+0x14` |
| `0x78` | map raw `0x5411C1`=`09`; target raw `0x54046F` / VA `0x94106F` | optional log, then callback object's vtable `+0x14` | map raw `0x550372`=`0B`; target raw `0x54F5B8` / VA `0x9501B8` | optional log, then callback object's vtable `+0x18` |
| `0x79` | map raw `0x5411C2`=`0A`; target raw `0x540498` / VA `0x941098` | optional log, then callback object's vtable `+0x18` | map raw `0x550373`=`4E`; default raw `0x5501F6` / VA `0x950DF6` | unhandled-ID logger |
| `0x7A` | map raw `0x5411C3`=`4B`; default raw `0x541052` / VA `0x941C52` | unhandled-ID logger | map raw `0x550374`=`4E`; default raw `0x5501F6` / VA `0x950DF6` | unhandled-ID logger |

Both `+0x04` methods first call the indirect helper at `dword_1AC4D24` with
the receiver and store its result at receiver `+0x28`. If receiver `+0x10` is
non-null, they call that nested object's vtable `+0x04` with the receiver,
the original message pointer, and the DWORD at `message+0x24`. Only when
the pointer at receiver `+0x2C` is non-null do they dispatch `message+0x10`; the accepted
range is `0x53..0xC4`. An ID outside that range or a table sentinel (`0x4B`
for BaseController, `0x4E` for NPCController) reaches
`VIBE_LogUnhandledMessageId` with the supplied ID. The constructors at
`0x53F980` and `0x54E7B0` initialize receiver `+0x28/+0x2C/+0x30` to zero;
at the instruction level, both explicitly write zero to `+0x2C` (`0x53F9D2`
and `0x54E802`). Dispatch treats that field as a nullable object pointer, not
as a Boolean state flag: each specific case pushes the original message
pointer, loads `ECX` from `[receiver+0x2C]`, reads that object's vptr, then
calls the listed slot. The selector-`0x02` BaseController binding is resolved
below; the selector-`0x03` NPC runtime binding is not. The separate nested
object at `+0x10` remains optional, with its type and later non-null writer
unresolved.

The five concrete BaseController/NPCController dispatch stubs do not parse
`message+0x28` or other dialog payload fields, update message-driven
application state, or directly dispatch a numeric UI event. They optionally
call `VIBE_LogIfSeverityLe2` using receiver
`+0x30`, with the format string `received %s` and a fixed diagnostic label:
BaseController IDs `0x77/0x78/0x79` log `msg-119`/`msg-120`/`msg-121`;
NPCController IDs `0x77/0x78` log `msg-120`/`msg-121`. These strings are
logger arguments, not event names. The cases then pass the unchanged message
pointer to the object at receiver `+0x2C` through the class/ID-specific
virtual slot shown above.

### Verified selector-`0x02` BaseController binding

The actual object stored in BaseController `+0x2C` on the selector-`0x02`
state-setup path is a separate `tfLocalCharacter::BaseNetController` adapter,
not the `tfGenericCharacter` state object. The receiver pointer returned by
the keyed registry lookup is carried in `EBX` and passed to state creation at
raw `0x800927`; the state factory then passes that same receiver to the
state's `+0x20` setup method at `0x800A7C`. The `tfGenericCharacter` setup at
`0x426840` passes the keyed receiver and its private context to `0x4A17C0`
(`0x4268C2`).

For selector `0x02`, `0x4A17C0` allocates `0x228` bytes at raw `0x4A182C`,
retains the allocation in `EBX`, and calls constructor `0x4C66D0` at
`0x4A1853` with that object in `ECX`, the private context, and the keyed
BaseController receiver as its first two stack arguments; the call also
pushes the pre-call `ECX` value as a third stack argument whose role is not
needed for the traced field stores. The constructor installs vtable VA `0x1C64648`
(raw `.rdata` offset `0x1863248`, RTTI
`.?AVBaseNetController@tfLocalCharacter@@`) at object offset `0`; it stores
the private context at adapter `+0x170` and the keyed receiver at `+0x174`.
The context retains this adapter at `+0x18`.

The setup update at `0x4A18D6` and subsequent context update at `0x4A2EE5`
call `0x4C7360`. That method loads the keyed receiver from adapter `+0x174`
and writes the adapter's unadjusted `this` pointer to
`[BaseController+0x2C]` at `0x4C7393`. It also refreshes BaseController
`+0x24` from the pointer at private-context `+0x170/+0x04`; this is a
separate field from the `+0x2C` callback object.

The installed BaseNetController vtable entries resolve directly to
`+0x10 -> 0x4C82E0`, `+0x14 -> 0x4C8300`, and `+0x18 -> 0x4C9B80`
(runtime VAs `0x8C8EE0`, `0x8C8F00`, and `0x8CA780`). These are the exact
targets used for BaseController IDs `0x77/0x78/0x79`. The case stubs pass the
original message pointer as one stack argument and use the loaded object as
`ECX`; all three target methods return with `ret 4`. The vtable entries point
directly at these methods and the constructor installs the vptr at object
offset zero, so no secondary-subobject adjustment is observed in this
binding. This matches the dispatch call contract.

### BaseNetController callback implementations for BaseController IDs 0x77..0x79

The selector-`0x02` setup constructs a `0x228`-byte adapter with RTTI
`.?AVBaseNetController@tfLocalCharacter@@` and vtable VA `0x1C64648` (raw
`.rdata` offset `0x1863248`). Its constructor at raw `0x4C66D0` stores the
private context at adapter `+0x170`, installs the vptr at `+0`, clears the
DWORD at `+0x16C`, and stores the keyed BaseController receiver at `+0x174`.
The setup path writes the adapter pointer into private-context `+0x18` through
`0x4DB1E0`; its initial update at `0x4A18D6` and later update at `0x4A2EE5`
call `0x4C7360`. That update writes the unadjusted adapter pointer to
`[BaseController+0x2C]` and copies `[private-context+0x04]` to
`[BaseController+0x24]`. These are separate fields.

The previously recorded dispatch table fixes the message-to-slot mapping:

| BaseController message ID | Case stub (raw) | Adapter vtable slot | Direct callback target (raw) |
| --- | --- | --- | --- |
| `0x77` | `0x540446` | `+0x10` | `0x4C82E0` |
| `0x78` | `0x54046F` | `+0x14` | `0x4C8300` |
| `0x79` | `0x540498` | `+0x18` | `0x4C9B80` |

This mapping follows the BaseController's current selector-map and jump-table
case snapshots, not message RTTI. The vtable points directly to these code
addresses; all consume one stack argument and return with `ret 4`. The raw
address-to-VA mapping for `.text` is `+0x400C00`, giving callback VAs
`0x8C8EE0`, `0x8C8F00`, and `0x8CA780`. The `0x4C82E0` target is a short
direct logger stub not recognized as a function by IDA; its bytes contain a direct call to
`VIBE_LogUnhandledMessageId` (`0x2FC4F0`) and `ret 4`, with no thunk jump or
adjustment.

The `0x4C8300` callback uses the message argument's shared-text reference at
`message+0x28` in diagnostic-object construction, then calls the vtable slot
`+0xAC` of the object pointer loaded from global VA `0x1E002B8` with numeric
event ID `285`,
that local object, and zero. The numeric ID is the observed effect; its event
name or meaning is unresolved. The callback does not directly write a field
in the adapter or private context.

The `0x4C9B80` callback reads the message DWORDs at `+0x30/+0x34` to form a
key, clearing the low byte of the first DWORD (`& 0xFFFFFF00`). It searches
the keyed registry returned by `0x35E700` at `+0x54`, using the QWORD-key
lookup at `0x4093E0`. A found entry's `+0x10` pointer is the next recipient;
byte `+0x08` on that pointed-to recipient must equal `1` to enter the update
path; other values skip it. On the update path the callback calls
`0x466660` with the normalized key, the value at `message+0x38`, flag `1`,
and the address of the key pair at `message+0x30`. If that helper returns
false, it calls `0x439A50` with `ECX=recipient`, first stack argument
`message+0x38`, and second stack argument (opaque) `message+0x28`. The callback
body directly proves these offsets and argument order, but not the meaning
of either message value.

#### Bounded fallback state update

The raw `0x439A50` fallback first passes `message+0x38` to `0x421640`, which
searches a separate table and returns either a record pointer or no match. A
no-match returns from the fallback without these recipient updates. For a
match, byte `record+0x24` gates the first update; when nonzero, the fallback
assembles a six-DWORD input snapshot and calls `0x457D30` with destination
`recipient+0x4CA8`. The input DWORD order is: the two DWORDs returned through
`0x49DD40`; three raw 32-bit values at offsets `+0x04/+0x08/+0x0C` of the
object returned by a virtual call at slot `+0x44`; then the DWORD loaded from
`[0x1E4E2AC+0x2C]`. The middle three and final value are moved as scalar
floats in the native instructions, but `0x457D30` copies their exact 32-bit
patterns. The destination presence byte is at `recipient+0x4CA8`, followed
by six DWORD stores at `+0x4CB0`, `+0x4CB4`, `+0x4CB8`, `+0x4CBC`,
`+0x4CC0`, and `+0x4CC4`; the intervening bytes are not written. If the
destination byte was zero, `0x457D30` sets it to `1` after the copy. A
pre-existing nonzero byte is preserved. If `record+0x24` is zero, this
presence byte and all six DWORDs are left unchanged; this path does not clear
the record.

Next, if the matched record's DWORD at `+0x14` is nonzero, `0x439A50` passes
its low WORD and the opaque `message+0x28` value to `0x439380`. That helper
uses the low WORD to find a descriptor-table entry and calls `0x439410` even
when the lookup misses (with its fallback inputs). On a hit, two descriptor
strings and the descriptor's state/data fields feed `0x439410`. It assigns
the selected text through `0x2FCC90` at `recipient+0x4CFC`, writes the state
DWORD at `+0x4D00`, and three DWORDs at `+0x4D04/+0x4D08/+0x4D0C`. The three DWORDs
can be replaced together by the native default vector when any input matches
its masked-float sentinel check; the C# model below takes that vector after
this choice. The source byte for `+0x4D10` (named `SourceByteAt4D10` in the
model input to distinguish it from the stored recipient byte) is selected
from two context bytes based on the opaque argument's comparison with a
context interval. The helper compares bit `0x02` in the old and incoming
`+0x4D00` values. On clear-to-set, it stores the source byte only if that byte
is nonzero or differs from the current `+0x4D10` value; if both are zero, the
existing zero remains. Set-to-clear and unchanged-bit paths preserve the
existing `+0x4D10` byte. A transition calls `0x438D10`: set-to-clear passes
the source byte unless it is zero, in which case it passes the prior
`+0x4D10`; clear-to-set passes zero. With no transition, that call is skipped.
Separately, state bit `0x08` clears recipient byte
`+0x4C70`; otherwise bit `0x10` sets it to `1`; if neither bit is set that
byte is preserved (when both are set, the clear branch wins). These are
offset-level effects only; no semantic names are assigned to the fields or
descriptor ID.

Finally, if the matched record's DWORD at `+0x10` is nonzero, its DWORD at
`+0x08` gates an auxiliary path: when zero, `0x369660` receives the value
loaded from `recipient+0x58A0`, the matched record, literal `7`, and two
zeroes. Regardless of that gate, the fallback then loads an object pointer
from `recipient+0x46A4` and calls `0x111BB20` with the original
`message+0x38` value. These downstream calls can have other effects and are
not simulated by the bounded state model. The keyed lookup/update path has
no removal operation in the direct `0x4C9B80` body, and the callback does
not write adapter `+0x170/+0x174`.

The model in `CharacterKeyedRecipientFallbackModel.cs` applies only these
verified field copies and flag-byte rules from already-resolved input
snapshots; it does not imitate the native heap, discover the registry entry,
recreate descriptor-string ownership, perform the descriptor lookup or
sentinel-vector selection, or execute downstream calls. `Startup.Checks`
tests its no-op, copy, presence, state-bit transition, and clear/set behavior,
while binary checks fingerprint the gates, call sites, and field stores in
the original sample.
The keyed map is heterogeneous and lookup-key-dependent: the `tfGenericCharacter`
producer traced below proves one concrete entry path, not the type of every
recipient. This BaseController callback ID `0x79` is a separate dispatch
namespace from Matrix nested selector `0x79`; it does not establish
`PrivateDialog` message semantics for a keyed object. The selector-`0x03`
MandMController code remains conditional, and the `tfGameProtocolHandler`
findings elsewhere in this document concern a different ReplayReader path.

### Concrete keyed-registry recipient

The Character-state producer at raw `0x943360` calls factory `0x425C90`
(`0x94377C`) and receives its returned pointer in EAX, copied unchanged to
EDI at `0x943781`. The factory requests `0x5AE0` bytes through allocator
`0x12B7FB0` (size at `0x425DD7`, call at `0x425DE2`), calls
`Engine_GenerateCounterId` at `0x425DF7`, and passes its `EDX:EAX` pair to
constructor `0x422E80` at `0x425E08`. The first DWORD is changed to
`(EAX & 0xFFFFFF01) | 1`; the constructor stores it at object `+0x08`, stores
EDX at `+0x0C`, and therefore sets the gate byte `+0x08` to `1`.

The constructor's final primary vptr write is at raw `0x422FEA`, object
offset `0`, with vtable VA `0x1C63998` (raw `.rdata` offset `0x1862598`).
The original binary's RTTI name is `.?AVtfGenericCharacter@@`; its Complete
Object Locator offset is zero. At raw `0x94378B..0x943793`, the producer
pushes the unadjusted EDI pointer, calls the same getter `0x35E700` used by
the callback, moves that exact EAX to ECX, and calls insertion setter
`0x407F00`. The setter uses its `this+0x54` map, reads the object's key at
`+0x08/+0x0C`, and clears the low byte of the first DWORD. Thus this
constructor path inserts a complete `tfGenericCharacter` pointer into the
exact getter-returned registry used by `0x4C9B80`, not merely a same-shaped
map or a type inferred from the fallback offsets.

The insertion path indexes this object by
`(EAX & 0xFFFFFF00, EDX)`. The callback independently looks up
`(message+0x30 & 0xFFFFFF00, message+0x34)` in that same map. The trace proves
the key comparison and both normalizations; it does not prove that the
message bytes are directly copied from `Engine_GenerateCounterId`.

Setter `0x407F00` reaches node allocator helper `0x40C470`; `0x40BBC0`
requests an 0x18-byte node, and `0x40E6C0` stores its key pair and recipient
pointer at node `+0x08/+0x0C/+0x10`. The callback reads that same value slot
at `+0x10`; no pointer adjustment appears between allocation, construction,
insertion, and lookup. Removal helper `0x4088F0` unlinks and passes the node
to `0x12C8D60`, but does not visibly release the recipient pointer stored at
node `+0x10`; pointee ownership/lifetime remains unresolved.

The allocation covers the callback's highest observed recipient offset,
`+0x58A0` (a DWORD extent through `+0x58A4`), and the factory itself writes a
value at `+0x58A0`. This is layout corroboration only; `+0x4CA8`,
`+0x4CB0..+0x4CC4`, `+0x4CFC`, `+0x4D00..+0x4D10`, `+0x46A4`, and `+0x58A0`
remain semantically opaque.

The registry is key-driven and can contain other insertions: a separate
direct path at `0x96D3E2` also calls `0x35E700` and inserts through
`0x407660`; its caller allocates `0x460` bytes at `0x9711D8`. Consequently,
the traced `tfGenericCharacter` producer establishes the concrete recipient
for entries under its generated key, but does not imply that every possible
message key in the shared registry resolves to that class. The
`tfGenericCharacter` state remains separate from the BaseNetController
callback adapter at receiver `+0x2C`; its primary vtable also has setup slot
`+0x20 -> 0x4263C0`. The old `0x420720/0x420780` `ret 8` callback-candidate
assignment remains withdrawn: the actual BaseController `+0x18` callback is
`0x4C9B80`, which returns with `ret 4`.

### Conditional selector-`0x03` adapter code, not an NPC runtime binding

`0x4A17C0` also contains selector-`0x03` code that allocates `0xB0` bytes at
`0x4A190F` and constructs a `tfLocalCharacter::MandMController` at
`0x4C6360`. Its vtable VA is `0x1C64640` (raw `.rdata` `0x1863240`, RTTI
`.?AVMandMController@tfLocalCharacter@@`); its constructor stores context at
adapter `+0x10` and the keyed receiver at `+0x14`. When the private context
later updates `+0x1C`, it calls `0x4C6490`, which writes that adapter's
unadjusted `this` pointer to the receiver's `+0x2C` at `0x4C64A1`. If
installed, the adapter's vtable slots `+0x14/+0x18` point to
`0x4C7C50/0x4C82E0`.

This is conditional code, not evidence that an NPCController receives this
adapter through the traced registration path. `0x4A17C0` has only the direct
caller `0x426840`; the keyed state factory at `0xAC9350` creates
`tfGenericCharacter` for selector `0x02` and takes selector `0x03` to its
unsupported-ID path. Thus the available path establishes the BaseController
binding but does not establish an NPCController `+0x2C` writer or runtime
adapter identity.

The separate indirect store at raw `0x4A2F07` is still not a controller
binding: it loads a pointer from private-context `+0x20`, follows its `+0x9C`
pointer, and writes the original pointer to that child object's `+0x2C`.
Private-context constructor `0x49EB30` initializes `+0x20` to null at
`0x49EBE9`; no alias from that child destination to either keyed controller
is established.

The optional nested receiver at controller `+0x10` is independent of the
callback adapter at `+0x2C`. Shared constructor helper `0xBCAD70` initializes
nested `+0x10` to null at `0xBCAD91`. Both dispatchers load that field and,
when non-null, call its vtable `+0x04` with `ECX` set to the nested object and
stack arguments `(controller, original message, DWORD at message+0x24)`.
The nested receiver's type and any later non-null writer remain unresolved.
The keyed Character cases themselves do not parse `message+0x28` or other
dialog payload fields, update message-driven application state, or directly
dispatch a numeric UI event.

This keyed path is separate from the conditional zero-low-byte branch in
`0x7FDC60`, which can call the `tfGameProtocolHandler` via callback-context
`+0x08`. The present trace does not show the keyed callback object at receiver
`+0x2C` to be that protocol handler. Parser-object consumers and numeric UI
codes documented elsewhere likewise do not establish reuse by these keyed
receiver cases.

## Parser result path and dialog-message primary vftables

The large dispatcher/parser at raw file offset `0x81CC50` has two direct
callers in the current IDA database. `0x821BA0` calls it at `0x821D87` on its
secondary-dispatch path and returns EAX through its own epilog. The separate
wrapper at `0x821DE0` calls it at `0x821E1E` and likewise returns EAX without
post-call transformation. These are parser-result pass-throughs, not observed
application handlers.

The raw function `0x821BA0` has runtime VA `0xC227A0` under the documented
`.text` mapping (`raw + 0x400C00`). The ReplayReader setup now distinguishes
allocation from callback registration:

- `ReplayReader_Initialize` (`0x65D880`) writes the callback VA into a local
  record at `0x65DAEA` (record `+4`; the selector is `0x4C5F`). The preceding
  `push 0xCC` and call at `0x65DAF2` pass only the allocation size to
  `0x12B7FB0`. That helper delegates to allocator backend `0x12C8A20` and
  returns the allocation or logs failure; it is not the callback registrar.
- The initializer constructs the allocation with `0x7FD670` at `0x65DB3F`.
  The constructor stores `0xC227A0` at object `+0x10` (`0x7FD820`) and
  `0xC1BDB0` at `+0x14`.
- The actual static registration call is `0x65DBD3` to `0xBD6260`, with the
  local record and the object at `[edi+0x54]`. `0xBD6260` copies the record's
  first DWORD to registration-state `+0x18`, its parser pointer from record
  `+4` to state `+0x1C`, and the object pointer to state `+0x20`.
- `0x7FE9F0` separately copies `0xC227A0` into `qword_1EDD1EC` and calls
  `0x7FEB50`; this is another observed state update, not proof of a parser
  invocation.

The registered callback is invoked indirectly in `0xBD6B40`. At raw offset
`0xBD6D29`, the function loads `[this+0x1C]`; the call at `0xBD6D3C` is made
only when that pointer is non-null and the compact type read earlier in the
function is outside the built-in values `1..6` and `9`. The pushes establish
the cdecl arguments `(a4, 0, *(uint16_t *)(a3+4), 0)`: `a4` is the caller's
stream/reader object, the second argument is offset zero, the third is the
message length from the header, and the fourth is a null secondary stream.
This matches `0x821BA0`'s four-argument stream parser shape. The separately
registered context at `this+0x20` is not passed to this parser call.

On a non-null EAX return, `0xBD6B40` constructs a 12-byte record from the
first two DWORDs at `a3` and the returned pointer, then appends it to the
queue at `this+0xB4` via `0xBD8B80`. The queue helper copies the three DWORDs;
no retain/add-reference operation is visible in that insertion. The native
consumer at `0xBD62F0` drains that same `this+0xB4` queue and stages records
in a second queue at `this+0xC8`. It reads the stored context from `this+0x20`
and dispatches through its vtable slots `+4` or `+8`, selected by the record's
byte at `+6`. The observed numeric routing is: byte `1` calls slot `+4`;
byte `2` calls slot `+8` with argument `1`; byte `3` calls slot `+8` with
argument `0`; other values skip these virtual calls. The message-pointer slot
is passed by reference, and the consumer reloads the possibly updated pointer
after dispatch. It atomically decrements that pointer's DWORD at `+8`; when
the old count is one, it calls that object's vtable slot `+8` with argument
`1`. These are numeric routing facts only; no message semantics are inferred
from the tag byte.

The ReplayReader context type and primary vtable are now identified from the
registration path. At `0x65DB3F`, `ReplayReader_Initialize` calls constructor
`0x7FD670`; the return in EAX is copied to ESI and stored directly at
`[edi+0x54]` (`0x65DB44`, `0x65DB75`). The later registration call at
`0x65DBD3` passes that same field, and `0xBD6260` copies its third argument
to registration-state `+0x20`. The context therefore points to the
constructor-returned object start, not an adjusted secondary-interface
pointer: its vptr is written at object `+0`, and the vtable's RTTI Complete
Object Locator has object offset `0`. The constructor initializes the mode
byte at `+0x18` to zero and stores the parser callback address at `+0x10`; `ReplayReader_Initialize`
sets that mode byte to `1` at `0x65DBC5`, before registering the context.

The vptr is VA `0x1C6D624` (file offset `0x186C224` via the `.rdata` section
mapping, raw-to-VA delta `0x401400`). Its Complete Object Locator is VA
`0x1CBF794` (raw `0x18BE394`); its type descriptor pointer at `+0x0C` is VA
`0x1E239BC` (raw `0x1A21DBC`) and resolves to `.?AVtfGameSocketHandler@@`.
The primary table's slot `+4` is VA `0xBFE7E0`
(raw `0x7FDBE0` under the `.text` delta `0x400C00`): this method logs through
`dword_1C15EA0` and returns with `ret 4`; it does not read or replace the
message-pointer argument. Slot `+8` is VA `0xBFE800` (raw `0x7FDC00`).
That method tests context byte `+0x18`: while zero, it passes the parsed
message's byte at `+0x18` to `0x809D20` and logs the resulting label along
with message fields `+0x0C` and `+0x10`; when nonzero, it calls
`0x7FDC60` with the message-pointer reference and selector argument.

For registered mode, raw `0x7FDC60` first tests message byte `+0x18`. If it is
nonzero, it calls `0x800420`; if zero, it compares message DWORD `+0x10` with
`dword_1DFC030`. Equality takes the existing nested-message path: it iterates
the 16-bit range at message `+0x38` with count at `+0x3A`, parses children
through `0xBCA850`, recurses through `0x7FDC60`, and releases a returned child
through its vtable slot `+8` if the reference count reaches zero. For a
nonmatching discriminator, `0x81BB90` is called only when callback-context
field `+8` is non-null; this remains conditional on the earlier `+0x18`
low-byte test being zero.

That `+8` value is not a fixed global target. In `ReplayReader_Initialize`,
the object in EDI invokes its vtable slot `+0x24` at raw `0x65DB10`; the
returned pointer's field `+0x78` is loaded at `0x65DB38` and passed as the
constructor's second stack argument to `0x7FD670`. The constructor stores that
argument at callback-context `+8` (`0x7FD6B2`). Thus the `0x81BB90` receiver
is the value read from `[getter-result+0x78]`. Its construction path and RTTI
are now resolved below.

At `0x81BB90`, ECX is that receiver and the parsed-message pointer is the
stack argument. The function loads the message discriminator from `+0x10`,
subtracts `0x20`, rejects values above `0x5A`, and indexes a 91-entry table at
VA `0xC1D6DC` (raw `0x81CADC`). Each case calls a receiver vtable slot with the
message pointer: discriminator `0x20` reaches raw `0x81BBCD` and slot `+4`,
`0x21` reaches `0x81BBF5` and slot `+8`, and `0x40` reaches `0x81C0D0` and
slot `+0x84`. Static decoding of all 91 cases gives slot offsets
`4 + 4 * (discriminator - 0x20)` through `+0x16C`. The known dialog IDs
`0x78..0x7A` select slots `+0x164..+0x16C`; the concrete type and those three
handlers are documented below. This does not assign names to the remaining
numeric cases.

The other direct call, `0xBD7060`, is separately gated by a zero return from
`0x80A030(message[+0x18])` and non-null context field `+0xB0`. Its selector
argument is `3` when the callback-supplied argument is zero, otherwise `2`.
`0xBD7460` passes an eight-byte local header to each recipient's vtable slot
`+0x0C`, separately from the output-buffer pointer. The header contains
`owner[+0x20]` as a DWORD, output length as a WORD, the selector byte, and a
zero byte; the length is narrowed to WORD. The analyzed PE's `.text` header
maps raw offsets to VAs with `+0x400C00` (`ImageBase=0x400000`, section RVA
`0x1000`, raw pointer `0x400`), so raw `0x7FDC60` maps to VA `0xBFE860`; the
binary check validates this mapping and the selector-table entries against the
requested executable. Message-field meanings and producer-side reference
policy/ownership transfer remain unproven.

The parser's returned pointer is still passed through EAX by both `0x821BA0`
and the wrapper `0x821DE0`. The registration field is therefore distinct
from the readiness probe at `0xBD68F4`, which calls a vtable slot through an
object at `this+0x24`; its AL result sets or clears `this+0xB1`. The separate
store of `0xC227A0` into `qword_1EDD1EC` at `0x7FE9F0` remains an independent
state update. No code or data xrefs to raw `0x821DE0` or mapped VA `0xC229E0`
were found in the current IDA database; the wrapper's application caller and
role therefore remain unresolved.

The three dialog-message RTTI vtables have a primary three-slot prefix. VAs
are runtime addresses; the mapped `.rdata` file offsets below follow the PE
section table. The function targets are raw file offsets:

| Message RTTI | Vtable VA / `.rdata` raw offset | `+0` | `+4` | `+8` | Stream fields |
| --- | --- | --- | --- | --- | --- |
| `OpenMovieDialog` | `0x1C67028` / `0x1865C28` | `0x5EEC40` write | `0x5EEAE0` read | `0x526770` deleting destructor | `+0x28` |
| `PrivateDialog` | `0x1C67018` / `0x1865C18` | `0x5EEA40` write | `0x5EE930` read | `0x526880` deleting destructor | `+0x28`, `+0x30`, `+0x38` |
| `PublicDialog` | `0x1C66FF8` / `0x1865BF8` | `0x5EE830` write | `0x5EE760` read | `0x526900` deleting destructor | `+0x28`, `+0x2C` |

For `OpenMovieDialog`, slot `+0` obtains the NUL-terminated shared-text
length from `this+0x28`, reserves output space, and copies the field including
its terminator; slot `+4` calls
`Stream_ReadNullTerminatedSharedTextBytes(stream, this+0x28)`. The
`PrivateDialog` writer and reader visit `+0x28`, `+0x30`, and `+0x38` in that
order; the middle field is passed to the raw-QWORD writer/reader. The
`PublicDialog` pair visits DWORD fields `+0x28` and `+0x2C`. Each `+8` method
has destructor/deallocation behavior (including a conditional delete flag),
not message-specific application handling. Thus these vtable entries are
stream read/write and destruction methods; they should not be treated as
application event handlers.

### Concrete ReplayReader message receiver

The virtual getter called at raw `0x65DB10` is `tfLookingGlass` slot `+0x24`:
the owner vtable is VA `0x1C6781C` / raw `0x186641C`, its RTTI name is
`.?AVtfLookingGlass@@`, and the target method is VA `0xA5A370` / raw
`0x659770`. That getter reads the context pointer at `this+0x50`, then returns
the first DWORD of that context. `ReplayReader_Initialize` loads the returned
context's `+0x78` field and passes it to the callback-context constructor,
which stores it at callback-context `+8`.

The `tfLookingGlass` owner context is initialized through raw `0x9B2D00`
(called from `AppMode_LookingGlass_Initialize` at `0x65A903`). That helper
calls factory `0x9B73F0` and stores its result at context `+0`. The factory
constructs an opaque `0xA0`-byte state block through `0x9B82F0`; its
constructor initializes state field `+0x78` to null at `0x9B8411`. The
factory then constructs a `0xD8`-byte receiver through constructor
`0xAC8CA0` and stores the result into state `+0x78` at `0x9B7B56`, replacing
and releasing a prior non-null value through that value's vtable slot `+0`.
The state block's own type is not inferred.

Constructor `0xAC8CA0` installs the receiver's vptr at object offset zero.
Its vtable is VA `0x1C71EC0` / raw `0x1870AC0`; the Complete Object Locator
is VA `0x1CCA404` / raw `0x18C9004`, and its type descriptor is VA `0x1E29DF8`
/ raw `0x1A281F8`, containing RTTI name
`.?AVtfGameProtocolHandler@@`. These addresses and the RTTI chain are mapped
from the requested PE's section table; the binary checks verify the VA/raw
relationships as well as constructor bytes.

The receiver slots below are separate from the dialog-message serialization
vftables above. Case targets are the switch-table destinations, and the slot
call sites follow the dispatcher's `4 + 4 * (ID - 0x20)` formula:

| Dialog message | ID | Case target / slot-call raw offset | Receiver slot | Vtable method VA / raw | Target form |
| --- | --- | --- | --- | --- | --- |
| `OpenMovieDialog` | `0x78` | `0x81CA38` / `0x81CA57` | `+0x164` | `0xECEEE0` / `0xACE2E0` | Direct |
| `PrivateDialog` | `0x79` | `0x81CA63` / `0x81CA82` | `+0x168` | `0xECEF00` / `0xACE300` | Tail-jump thunk to `0xD92110` / `0x991510` |
| `PublicDialog` | `0x7A` | `0x81CA8E` / `0x81CAAD` | `+0x16C` | `0xECEF90` / `0xACE390` | Direct |

One selected path is traced farther: `OpenMovieDialog` handler `0xACE2E0`
calls `0x991320`, which prepares an application-dispatch record and calls
`dword_1E002B8` through vtable slot `+0xAC` with numeric event code `228`
(`0xE4`; immediate at raw `0x99147C`). The `PrivateDialog` thunk reaches
`0x991510`, which similarly dispatches numeric code `229` (`0xE5`; raw
`0x99165B`). `PublicDialog` handler `0xACE390` calls `0x49DCC0`, then
`0x4D6C10` with the returned value, the message's `+0x28` address, and a
boolean derived from byte `+0x84`; that helper dispatches numeric code `357`
(`0x165`; raw `0x4D6D43`). These are observed dispatch codes and arguments,
not inferred UI or dialog semantics.

### uiManager dispatch target, registry, and dialog payloads

The `.text` mapping in the requested PE is `VA = raw file offset + 0x400C00`
(`ImageBase=0x400000`, section RVA `0x1000`, raw pointer `0x400`). The receiver
is the process `uiManager` instance at VA `0x1E002B8` (raw `.data` offset
`0x19FE6B8`, initially zero in the image). Its constructor `0xBF7A80` writes
the `uiManager` vtable VA `0x1C73EBC`; the vtable is at raw `.rdata` offset
`0x1872ABC`. Its slot `+0xAC` is at raw `0x1872B68` and contains method VA
`0xFFA980`, which maps back to raw `.text` function `0xBF9D80`. The configured
singleton wrapper at VA `0x1E002A8` and the allocation/initialization path are
described at the start of this document. The slot target is established from
the vtable entry and PE mapping, not by guessing from the dialog RTTI.

`0xBF9D80` gates dispatch on `uiManager+0x672`, then indexes the event registry
at `+0x6F0` by the numeric event ID and visits that bucket through
`0xBF9C40`. This does not resolve an ID to a name. The separate name-to-ID map
is at `+0x6E8`; `0xBF9AB0` returns an existing ID or assigns the current count
of the event-name array at `+0x71C..+0x720`. The component XML parser
`0xC35740` registers each event name before its listener (`0xC358AF` then
`0xC35AD8`); `Component.BindEvent` uses the named-subscription helper
`0xC35BB0`. The first-insertion order also depends on component load order and
script registration, so the static binary does not bind numeric IDs `228`,
`229`, or `357` to exact event-name strings.

One downstream route is statically visible but not a specific listener
instance: `0xBF9C40` visits the dynamically registered listener bucket and
optionally compares a listener name at `+0x70`; it calls `0xC38400` for a
match. That routine looks up the listener's per-event callback map at `+0x168`,
updates its pending-dispatch structure at `+0x4C4`, and adds a record to the
manager-side delivery structure at `+0x5EC`. The later consumer/function invocation and
which runtime listener receives these three codes remain unresolved.

The application-dispatch records built immediately before the virtual call
have these verified fields (offsets are from the receiver method's input
pointer):

| Receiver route | Numeric code | Prepared record |
| --- | ---: | --- |
| `OpenMovieDialog` (`0x991320`) | `228` | `vendor_id` = DWORD at `+0x28`; the separate text pointer at `+0x30` also enters a helper/cache path through the object referenced by global `dword_1E0031C`, but is not placed in the dispatched record. The virtual call receives the record and trailing zero. |
| `PrivateDialog` (`0x991510`) | `229` | `success` = byte at `+0x28`; `product_id` = DWORD pair at `+0x30/+0x34`; `price_id` = DWORD pair at `+0x38/+0x3C`; `vendor_id` = DWORD at `+0x40`; `code` = NUL-terminated text at `+0x44`. If the success byte is nonzero and the text is absent/empty, the code field is filled with the literal `PURCHASE_SUCCESSFUL`. The virtual call receives this record and trailing zero. |
| `PublicDialog` (`0x4D6C10`) | `357` | Prepares `action_id` = DWORD at `+0x30` and `activated` = byte at `+0x84`, but the actual `uiManager+0xAC` call passes null for both payload arguments. Do not treat those prepared values as dispatched payload fields. |

These are dictionary keys and a fallback **value**, not event subscription
names. The `PrivateDialog` handler reads through `+0x44`, beyond the currently
fingerprinted serialized field set (`+0x28`, `+0x30`, `+0x38`); how the extra
fields are populated remains unresolved. The local UI assets contain
`ON_VENDOR_PURCHASE_RESPONSE` bound to `OnVendorPurchaseResponse`
(`system/gui/components/MainUI/Panels/Vendor/Vendor.xml:24`); its Lua handler
uses `product_id`, `success`, and `code` (`Vendor.lua:258-275`). This is a
plausible content-level candidate for the private record only. Because the
name-to-ID insertion order is not reconstructed, it is **not verified** as
subscriber to numeric code `229`, and no name is assigned to any of the three
numeric codes.

A focused read-only verification of the installed UI source at
`D:\SteamLibrary\steamapps\common\Firefall\system\gui\` confirms
`components\MainUI\Panels\Vendor\Vendor.xml:24` binds
`ON_VENDOR_PURCHASE_RESPONSE` to `OnVendorPurchaseResponse`. Its handler at
`components\MainUI\Panels\Vendor\Vendor.lua:258-282` finds the product by
`product_id`; success creates a loot event, plays the purchase-confirmation
sound, and refreshes product highlights (`270-273`), while failure clears the
pending product and chooses an error voice line (`275-280`). This is a
verified handler effect for that named subscription, but not proof that the
subscription's runtime ID is `229`.

The analyzed executable also contains native event-like string literals
`ON_VENDOR_PURCHASE_RESPONSE` at raw file offset `0x1749050` (VA
`0x1B4A450`) and `ON_PLAY_MOVIE` at raw `0x1749578` (VA `0x1B4A978`). IDA
reports no xrefs to either literal. The latter was not found in the focused
installed `system\gui` source search. These literals are candidates, not
registration evidence; no subscriber callback was proven for codes `228`,
`229`, or `357`. The public-dialog helper prepares `action_id` and `activated`
but passes null payload arguments, so those fields cannot identify a callback
or its effect.

None of these three selected entries is a null or empty stub: two target
direct functions and the `PrivateDialog` entry is a five-byte no-adjustment
tail-jump thunk. The 91 receiver-table entries have distinct function-pointer
values in the analyzed database, but the other slots have not been semantically
classified as inherited, default, or message-specific methods.

`MatrixMessageParserFlowNativeLayout.cs` records the parser and concrete
recipient path; `DialogApplicationDispatchNativeLayout.cs` records this
separate uiManager dispatch target, dynamic registry path, and observed dialog
payload fields. Startup checks fingerprint the vtable/slot mapping, registry
edges and first-seen ID allocator, payload construction, string keys, native
event-like literals, numeric call arguments, and PE section translations from
the requested binary. The model stores null name/callback bindings for
`228/229/357` rather than guessing. These models do not emulate parsing,
assign event-name semantics to numeric IDs, identify a concrete runtime
listener, or claim a complete ownership policy.

The nested table at VA `0xC22444` (raw `0x821844`) is now traced from selector
`0x53` through `0x7A`, inclusive:

| Selector | RTTI | Selector | RTTI |
| --- | --- | --- | --- |
| `0x53` | `MarketRequestComplete` | `0x54` | `ReceiveWeaponTweaks` |
| `0x55` | `TookDebugWeaponHitPublic` | `0x56` | `TookDebugWeaponHit` |
| `0x57` | `DebugWeaponStats` | `0x58` | `RewardInfo` |
| `0x59` | `ProgressionXpRefresh` | `0x5A` | `ReceivedDeferredXP` |
| `0x5B` | `PublicCombatLog` | `0x5C` | `PrivateCombatLog` |
| `0x5D` | `AnimationUpdated` | `0x5E` | `RaiaNPCDebugging` |
| `0x5F` | `WeaponProjectileFired` | `0x60` | `AbilityProjectileFired` |
| `0x61` | `ProjectileHitReported` | `0x62` | `Stumble` |
| `0x63` | `QuickChat` | `0x64` | `ProximityTextChat` |
| `0x65` | `JumpActioned` | `0x66` | `JumpRolled` |
| `0x67` | `Respawned` | `0x68` | `CalledForHelp` |
| `0x69` | `TookHit` | `0x6A` | `AlmostHit` |
| `0x6B` | `DealtHit` | `0x6C` | `Killed` |
| `0x6D` | `WarnLockTargeted` | `0x6E` | `CurrentPoseUpdate` |
| `0x6F` | `ConfirmedPoseUpdate` | `0x70` | `PublicDebugMovementUpdate` |
| `0x71` | `ForcedMovement` | `0x72` | `ForcedMovementCancelled` |
| `0x73` | `GrappleClimbPermission` | `0x74` | `AbilityActivated` |
| `0x75` | `AbilityFailed` | `0x76` | `AbilityCooldowns` |
| `0x77` | `NPCInteraction` | `0x78` | `OpenMovieDialog` |
| `0x79` | `PrivateDialog` | `0x7A` | `PublicDialog` |

This closes the nested selector mapping, not the overall Matrix analysis.
All 17 primary-route subcases have native layouts. Parser-level field
semantics and allocation-failure consequences remain open for many; the
receiver paths for `0x74`–`0x77` are now traced above, but `0x75`/`0x76`
embedded-state meaning and `0x77`'s out-of-extent transfer remain unresolved.
Selector `0x55`'s 12-byte temporary prefix and selector `0x6D` body
consumption also remain open. The immediate `0x81CC50` pass-throughs, dialog
primary-vftable roles, ReplayReader registration, and conditional recipient
dispatch through `0x81BB90` are recorded above. The indirect parser invocation
and parsed-object ownership details are still open; numeric application codes
are not assigned event names.

## Message35 application body

Factory case35 at `0x33D557` allocates a fresh0x50-byte descriptor through
constructor `0x339AB0`, then reads its body with `0x3570B0`.
Reader `0x356F80` copies exactly eight raw bytes into descriptor+28.
Reader `0x12EE170` then fills reference-backed blob objects at descriptor+30
and+40. Each blob uses a raw little-endian WORD length followed by that
many bytes; zero leaves the freshly initialized blob empty.

The normal-buffer application packet is therefore:
`[compact code35] [QWORD LE] [WORD LE length0] [blob0] [WORD LE length1] [blob1]`.
Minimum size is13 bytes; each blob can independently have65535 bytes.
The two words copied to connection+860/+864 are halves of this one QWORD,
not evidence of two independent semantic identifiers. Their meaning and
both blob meanings remain unresolved.

`MatrixServerMessages\MatrixMessage35Wire.cs` decodes this bounded packet
and reports malformed/truncated input explicitly. It exposes consumed
length instead of pretending trailing bytes are validated, makes managed
copies of blob data, and does not implement transport framing or the
alternate-header-buffer path. No server traffic is generated.

## EnterZone wire body and optional nested metadata

Factory case37 at raw `0x33D629` allocates0xC8 bytes through constructor
`0x339EA0`, then reads body `0x356E60`. Zone-field reader `0x356770`
consumes the following **in wire order**, without native structure padding:
QWORD instance; DWORD zone; opaque QWORD at descriptor+38; preview byte;
NUL-terminated raw text for+44; WORD+48; DWORD+4C; byte+50; QWORD match;
byte+60; DWORD+64; NUL-terminated zone-name bytes; optional metadata.

Optional metadata reader `0x3565C0` begins with a byte. Zero means absent;
nonzero parses two temporary maps through `0x355140/0x355E50` before
copy/cleanup. Each map uses **chunked byte counts**: read a count followed
by that many entries; read another chunk only when the previous count was255.
Empty maps use a single0 count; exact multiples of255 require a final0.
An entry is a NUL-terminated raw byte key followed by a raw LE WORD in the
first map or raw LE DWORD in the second. Value signedness and semantic
types are unresolved; the DWORD is not assumed to be a pointer or float.

Lookups `0x3550A0/0x355DB0` compare keys lexicographically as unsigned
bytes and return existing value slots, so later duplicate keys overwrite
earlier values. The maps stay separate even when keys match across them.
Their adjacent temporary head/size pairs are at stack+1C and+24.
Copy helper `0x3563E0` copies the first at input+0 and the second at input+8;
the decompiler's split locals do **not** mean the second map is discarded.
Presence is normalized to true after copying, including when both maps are empty.

The suffix fills descriptor+80 with raw8 bytes, +88/+8C with raw4-byte
values, then +90 with five raw QWORDs and a **normalized boolean** at+B8.
The final byte fills+C0 (spectator). Consequently the suffix serializes58
bytes, not the64-byte memory copy described earlier: seven padding bytes
after+B8 are not wire fields. The first16 raw bytes remain opaque; the
following five QWORDs and boolean are now linked to clock parameters below.
No transform or spatial-coordinate meanings are assigned.

`MatrixServerMessages\MatrixEnterZoneWire.cs` now implements both variants
through `Decode`, with metadata parsing in `MatrixEnterZoneMetadataWire.cs`.
The earlier `DecodeWithoutNestedMetadata` entry point retains its explicit
`UnsupportedNestedMetadata` rejection. Minimum packet size is103 bytes absent,
105 bytes with present empty maps. A null metadata object means absent;
a nonnull object with empty entry lists means present-but-empty. Decoded
map entries are sorted, unique and backed by managed copies of key bytes.
Text remains bytes with no guessed encoding, the suffix boolean normalizes
any nonzero input, the spectator byte remains raw, and trailing bytes are
exposed by consumed length rather than silently treated as validated.
Shared bounds/byte-copy helpers also serve the message35 decoder.

## EnterZone clock and downstream initialization

The copied block is followed by an embedded clock at connection+918.
Handler `0x490C10` clears its existing initialized byte, calls initializer
`0x8FA620` with descriptor+90, samples connection time through `0x493650`,
then advances the clock through `0x8FA700`. The sample comes from socket
clock+D78 via `0xBE8410`, or `timeGetTime()*1000`, or cached connection+818
when no socket exists.

| Descriptor field | Clock member relative to connection+918 | Proven use |
| --- | --- | --- |
| +90 QWORD | +18 current tick (combined with +B0) | Initial cursor seed |
| +98 QWORD | +8 | Reference tick for rate scaling |
| +A0 QWORD | +10 double | Rate scale, preserved as raw bits in C# |
| +A8 QWORD | +38 | Upper tick limit |
| +B0 QWORD | +28 | Additive offset bits |
| +B8 normalized bool | +30 / connection+948 | Enables upper limit |

Initialization seeds current=initial+offset with wrapping64-bit addition,
then applies the upper limit when enabled. Projector `0x8FA780` skips rate
conversion when abs(scale-1)<=0.001. Otherwise it converts the unsigned,
wrapping source-minus-reference difference to double, multiplies by scale,
converts back to unsigned64 bits, then adds reference and offset. The
upper limit and monotonic cursor comparisons use the **sign of wrapping
subtraction**, not ordinary unsigned ordering. Advance stores only the
low DWORD of the cursor difference at clock+20.

`MatrixClockNativeLayout.cs` saves parameter extraction, initialization,
and projection/advance with finite rates. Native helpers `0x159E17E` and
`0x159E060` establish unsigned64-to-double rounding and truncating
double-to-unsigned64 bit conversion. Negative in-range results retain
signed-to-unsigned two's-complement bits; arithmetic after conversion wraps.
The accepted conversion range is [-2^63,2^64). Native out-of-range/nonfinite
conversion yields integer-indefinite bits and signals floating-point status;
the model instead throws explicit `NotSupportedException` at that boundary.
Floating-point exception flags are not simulated, and raw suffix bytes remain
available. The earlier `AdvanceWithoutRateConversion` entry point remains restricted.

After advancing, clock's limit flag at connection+948 selects runtime
message110 with property `"paused"` set to1. Builder `0xC243E0` stores a
byte value with native variant tag3; the handler invokes receiver global
VA1E002B8 virtual+AC. False does not emit a corresponding paused=false here.
This condition is **independent** of the spectator byte.

The handler next applies the preview options, calls `0x386900` on the global
owner's +C64 subsystem with connection byte+8A0 (copied from descriptor+50),
and requires connection+910. That downstream object's DWORD+28 must be zero;
then its byte+18 is set to1. Subsystem `0x386900` can also emit runtime136
on its asynchronous path; its complete resource behavior is not modeled.
`MatrixEnterZoneDownstreamNativeLayout.cs` preserves these prerequisites and
selections without executing external interfaces.

## Outgoing message24 after EnterZone

A nonzero connection byte+B94 selects descriptor construction and gate
`0x498350`. Its exact gate is a nonnull socket at connection+820 and
**signed connection state>4**. It records `timeGetTime()*1000` at+E0,
optionally logs when diagnostic byte+14 has mask2, and overwrites the
descriptor's +18/+1C pair with current instance context from+878/+87C.
The constructor's provisional global context is therefore not the sent value.

Sender `0x499840` supplies fixed code24 to encoder `0x49A520`, which writes
the raw8-byte context first and the compact positive16-bit code second.
There are **no additional body fields**: code24 is one byte, making the
application buffer exactly9 bytes. Submission `0xBD3B20` uses channel2.
This prefix order is not the incoming msg35/msg37 header order.

`MatrixClientMessages\MatrixMessage24.cs` saves the encoder and pure gate
selection. A permitted gate returns1 after invoking its sender, irrespective
of eventual delivery; the model calls this `ReadyForSubmission`, not delivered.
The raw wire writer imposes no entity-ID validity guard of its own. The
earlier EnterZone handler guard remains a separate boundary.

Finally `0x6F4540` looks up an object in the **already current app mode**:
Login1→mode+14, World2→mode+28E8, LookingGlass3→mode+68, otherwise null.
When nonnull, the handler changes object DWORD+1E0 from0 to1, and does the
same to+1E4 only when byte+1E8 is nonzero. Existing nonzero words are preserved.
This is not a queue request or evidence of a Login→World transition.

## Socket synchronized clock sampling

`0xBE8410` returns a full EDX:EAX64-bit sample from socket+D78. It locks the
clock's critical section at+28, samples `timeGetTime()*1000`, and subtracts
local reference QWORD+18 with wrap. The pipeline is deliberately **unsigned64
to double, then double to single, then a single-precision multiplication**
by float+14. Helper `0x159DE7F` truncates the resulting single into unsigned64
bits; QWORD+20 is then added as the synchronized base.

Candidate minus cached QWORD+60 is tested by its sign bit. Negative keeps the
cached value; otherwise the candidate replaces it. This is a different clock
from the EnterZone double-rate clock at connection+918. For example, local
time1000001000 rounds to1000001024 in this single-precision stage; replacing
the computation with double multiplication would change the result.

`GameSocketSynchronizedClock.cs` preserves finite in-range projection, separate
local/reference/base values, float rounding and monotonic wrap behavior. It
shares bounded numeric/bit arithmetic with the EnterZone clock. Locking and
peer calibration are outside this value model; unsupported numeric exception
cases remain explicit. IDA return/argument types now preserve64-bit results
and compiler-helper XMM0/ECX/EDX ABI instead of dropping high words.

## Adjacent scalar-update messages38 and39

These handlers do not establish a zone-exit or app-mode transition.
Message38 handler `0x491380` simply copies16 descriptor bytes at+28 into
connection+8D0, replacing the **first16 bytes** of the EnterZone block.
There is no state check or clock reinitialization in this handler.
Factory case `0x33D68F` allocates38h bytes; constructor `0x33A340` initializes
the four payload DWORDs to0,0,3F800000,0. Body reader `0x3545D0` reads raw
LE8 bytes, raw4 bytes, raw4 bytes through `0x356850/0x3568E0`. The default
3F800000 is consistent with float1.0, but the payload is retained as bits
until its consumer semantics are established. Normal packet size is17 bytes.

Message39 handler `0x4913A0` requires global owner+C64, invokes the same
`0x386900` subsystem with descriptor byte+28, then stores that raw byte at
connection+8A0. Factory case `0x33D6F2` allocates30h, constructor `0x33A420`
defaults the byte to0, and primitive `0x337180` reads one byte into+28.
Normal packet size is2 bytes. Its byte is not normalized to a boolean.

`MatrixServerMessages\MatrixScalarUpdateWire.cs` preserves both normal-buffer
formats, explicit framing/truncation errors, consumed length, and the
msg39 subsystem prerequisite. The raw fields remain unnamed beyond offsets
and widths; no scale, transform, or asset-quality semantics are assumed.

## Queue status/message62 and ExitZone/message40

The tentative msg62 lifecycle association was disproved: handler `0x491140`
builds runtime message295, properties `"queue_position"` and `"queue_length"`
from descriptor DWORD+28/+2C. Numeric builder `0xC24210` stores their32-bit
values with variant tag1; no guessed signedness or sentinel policy is added.
It calls the runtime receiver's virtual+AC and cleans the property container;
no connection-state write is present. Factory `0x33DF77` allocates30h through
`0x33C310`, reader `0x34B610` reads raw LE4+4 bytes. Normal packet size9.
`MatrixQueueStatusNativeLayout.cs` saves these labels and notification values;
the shared scalar-update wire decoder now includes code62.

Actual ExitZone is **message40**, handler `0x491230`, with original logs
`"Received ExitZone"`, `"Disconnecting from Zone"`, `"Sending ExitZoneAck"`.
Factory `0x33D757` allocates28h through `0x33A4E0` and returns immediately:
there is no body reader. Normal packet is the one-byte compact code40.

The handler first calls `0x494B00` on the primary connection. Cleanup requires
connection+910, acts on its nested object+B4 and releases/zeros that object,
asserts connection state6 or7, sets state5, and requires the current mode
pointer global+B98 before calling `0x64E210`. That current-mode cleanup
performs numerous resource resets and emits runtime97 with null body. Full
resource/lifetime effects remain unmodeled; no direct app-mode queue request
was established at this boundary.

After cleanup returns, nonzero connection byte+B94 selects disconnect:
optional global hook when VA1E4AC84 byte is nonzero, transport destroy request through
`0xBD2090` when present, pointer+820 zeroed, connection state1. Otherwise the
handler constructs the acknowledgement and permits submission only with a
nonnull transport and signed post-cleanup connection state>3; diagnostic
mask1 at+14 selects optional logging. State becomes4 regardless of submission.

The logged **ExitZoneAck is outgoing message19**, sender `0x498D70`, encoder
`0x499970`. It writes only compact code19 and submits on channel1: exactly1
application byte, no routing context or additional body. This is not msg24's
9-byte context-first format. Saved `MatrixExitZoneNativeLayout.cs` separates
known cleanup-state prerequisites from post-cleanup selection, without
assuming external calls cannot reenter. `MatrixClientMessages\MatrixExitZoneAck.cs`
encodes the proven acknowledgement bytes without socket I/O.

## Socket destroy request and deeper zone cleanup

The ExitZone disconnect branch's `0xBD2090` is a **destroy request**, not
unconditional socket-object deallocation. Its original instruction switch uses
the ten-byte index table at raw `0xBD2418` and four target VAs at `0xBD2408`:
indices `0,1,1,2,1,2,0,0,3,3`. These give the following socket-state branches,
separate from MatrixConnection state:

| Input socket state | Written socket state | Selected effects |
|---|---|---|
| 1 Initializing, 2 Transferring, 4 WaitingContact | 8 Destroyed | Call `0xBD2430` to request pump close. |
| 3 Connecting, 5 Connected | 8 Destroyed | No immediate pump-close or channel-cleanup call in this branch. Deferred close is traced below. |
| 0 NotOpened | 9 Closed | Direct handle close `0xBE36B0`, require invalid handle, channel cleanup `0xBD06B0`. |
| 6 DisconnectedNotify, 7 Disconnected | 9 Closed | Require invalid handle, channel cleanup `0xBD06B0`. |
| 8 Destroyed, 9 Closed, outside 0..9 | Assertion path | Do not treat a repeated destroy request as an established no-op. |

Before the switch, the method clears socket handler+10C, atomically changes
marker+124 from0 to6, and only on success captures state+108 into+128,
an imported timestamp result into+12C, and zero into byte+130. **Marker6
does not itself write socket state6.** The method brackets its work with locks
at+D98/+DA8 and increments/decrements nesting count+DD8. After the switch,
nonnull owner+110 selects `0xBD0880(socket,0)`, which clears that owner and
the associated channel owner pointers. This method does not directly free
the socket allocation.

Pump-close helper `0xBD2430` is guarded by byte+D95. If not already requested,
it builds/sends a control buffer, sets that byte, requires pump+8, calls the
pump's virtual+8 with the socket and handle-address+118, and requires the
handle to be invalid afterward. Packet details and deferred lifetime are not
reconstructed here. Direct helper `0xBE36B0` requires a valid handle, calls
two imports (arguments include2 for the first), then writes invalid handle.
Channel cleanup `0xBD06B0` drains queued references and resets the three
channel objects; it is not a whole-socket destructor.

`GameSocketDestroyNativeLayout.cs` saves the branch selections and first-marker
gate without performing locks, callbacks, handle operations, or allocation
release. External calls may change the socket during native execution, so the
model is not a complete atomic transition simulator.

Current-mode method `0x64E210` has tfWorld-shaped offsets, including observer
pointer+28E8. It conditionally calls `0x6E9C50(observer,0)` when observer
DWORDs+120/+124 equal+8/+C. It calls `0x494A40` through mode+10 at raw call
offset `0x64E27D`. That helper compares connection+0x0C with5; mismatch
reaches an assertion and rejoins the cleanup path, so the state is not a
proven execution gate. A null connection+0x910 likewise reaches an assertion
and rejoins. The nested action pointer at connection+0x910+8 controls whether
`0x7FEB50` is called; after that call, the helper clears the nested pointer
if it remains nonzero. It does not visibly clear connection+0x910 itself.

The world method next posts runtime97 with two zero arguments at virtual+AC.
Three virtual+24 getter calls are separate observations, not one assumed
stable pointer: result+74 supplies an object whose word+C is saved, temporarily
zeroed around `0x406310(object,1)`, then restored; result+78 requires pointer+10
for `0x976430`; result+50 gets byte+1AB=0 and+1A9=1. Later it zeros global
object+770, invokes additional subsystem/local-client resets, detaches pointer
`localClient.member4+5590`, and if nonnull invokes `0x861020` and the general
free routine. Finally mode byte+2F1 becomes0. Formal identities of these raw
members and deeper indirect effects remain unresolved.

The `0x976430` helper first walks a linked structure rooted at `this+0x20`,
following node+0 and reading node+0x10. It calls `0x977E20` for each node,
even when that value is null; the null path reaches an assertion and continues
toward a dereference. For non-null values it next calls `0x975020` and the
general free routine. After this value pass, it self-links the head's+0/+4
words and clears `this+0x24`; a second pass saves each node's next pointer
before releasing the node. Value processing therefore precedes head reset,
which precedes node release.

For each non-null value, `0x975020` walks two more sentinel-based structures:
one rooted at value+0x08 with per-node pointer+0x14, and another rooted at
value+0 with per-node pointer+0x18. It calls `0x9751F0` or `0x88AD60` on those
values respectively, then sends each value to the general free routine.
`0x9751F0` releases refcounted pointers at its+0x08/+0x04 and processes the
24-byte span range at+0x10 through `0x979A20`; `0x975020` also releases its
own refcounted pointer at value+0x18. Tag `0x17` follows the previously
described shared-text allocator path. These field accesses are proven, but the
entry identities and `0x88AD60` callback effects remain unresolved.

Two other direct callers pass an outer node's+0x10 value to `0x975020`.
`0x976200` first passes the field to `0x977E20`, reloads it, skips cleanup if
null, then calls `0x975020` and the general free routine. `0x977F20` selects a
node through `0x979C80`, passes its+0x10 value to `0x977E20`, then waits for
value+4 to clear before calling the same cleanup/free pair. The wait is a
synchronous drain: `0x977630` calls `0x978440`, which decrements value+4 if
nonzero, and the caller immediately rechecks it. This is not an observed
callback wait. `0x977E20` has no direct write to the outer node+0x10 slot or
value+4 in its visible body, but it does dispatch through virtual+0xAC with
event0x169, so indirect effects remain possible; `0x978B20` is also untraced.
The callers support a compatible cleanup-value shape, not a common semantic
type.

The two `0x975020` container-clear helpers (`0x97AE50`, `0x97AA70`) call node
removal helpers (`0x97BCC0`, `0x978440`) before the outer routine releases the
container heads. Those helpers manipulate node pointers at+0/+4/+8, bytes at
+0x0C/+0x0D, rebalance links and release a removed node. This is tree-like
structure evidence, not a recovered standard-library container identity.

It then walks a second linked structure rooted at `this+0`, following node+0.
For each node it calls `0x976D30` with node+0x10; that helper reads two DWORDs
there and dispatches through a virtual slot at+0xAC with event367. The callback
meaning is unresolved. Helper `0x9795B0` then resets the second head links and
clears `this+0x04`; its node pass saves next, calls `0x3F7680` with the address
of node+0x20, then rereads and releases the current slot value before releasing
the node. `0x3F7680` has a whole-range fast path to `0x3F7300`, which also
handles an empty tree. In this caller, start is the head's first node and end
is the head sentinel, so the full-range branch is taken. `0x3F7300` recursively
clears node+8 child subtrees through `0x3F72C0`, advances through node+0 links,
and frees each node. It then self-links header+0/+4/+8 and zeroes the adjacent
owner count at the node's+0x24. `0x3F7430` is the partial-range node-erasure
path; it rebalances
the tree-like links, frees the removed node and decrements that same count, but
is not reached for this full-range caller. The wrapper shows no release or slot
transfer: the caller frees the retained node+0x20 value after it returns, then
frees the outer node. This does not prove exclusive ownership or formal
container types.

Two adjacent ranges are also reset through `0x97CBB0` and `0x97C690`: the
first uses `this+0x28` / `this+0x2C`, then stores7/8 at+0x34/+0x38; the second
uses `this+0x08` / `this+0x0C`, then stores7/8 at+0x14/+0x18. Each helper
receives count0x10 (16 four-byte entries); its range is a three-pointer group
with begin/end/capacity-end at relative offsets+0/+4/+8. The caller first sets
end=begin, but the helpers may reallocate and replace all three pointers; they
are not simple pointer resets. On reallocation, the old begin pointer is sent
to the general free routine when non-null. The values and purpose of the
entries are unresolved.

The local-client branch additionally calls `0x4A2FB0` and
`0x431C90`; the latter clears member fields before unresolved virtual
callbacks. The released pointer slot at `localClient.member4+0x5590` is
cleared before the non-null check. For a saved non-null object, `0x861020`
first handles a separate record pointer at object+0x328. It reads a global at
PE VA `0x1DFFD98`, gets a record owner through virtual+0x24 on global+0xB98,
then calls `0xD74A20` with owner+0x0C and the record. `0xD74A20` checks record
leading DWORD `0x3C8`; when record+0x39C matches owner+4, it locks owner+0x38,
removes the DWORD token at record+0x398 from the owner range+0xB8..+0xBC,
calls `0xD935F0` and `0xD78900`, clears the token and decrements owner+0x68.
It then calls the general free routine on the record whether or not that
matching-state branch ran. This is bounded resource cleanup, not evidence of
an application-mode transition. The separate object+0x32C field is zeroed in
`0x860230` and later populated from a virtual+0x40 result in `0x860810`.
`0x861020` invokes the resulting object's vtable slot0 with argument1, but its
target is not a static address and the deleting-destructor interpretation is
unproven. In the producer `0x860810`, raw bytes at `0x860F3C` are
`8B 8E 98 0B 00 00 8B 01 FF 50 24`: it loads the object at global VA
`0x1DFFD98`+`0xB98` and dispatches byte offset `+0x24`. At `0x860F63`,
`8B 4E 10 8D 95 10 FF FF FF 52 8B 01 FF 50 40` loads the getter result's
member `+0x10`, passes a local address, and dispatches byte offset `+0x40`;
the return is stored at `+0x32C` by `89 87 2C 03 00 00` at `0x860F76`.
These establish the indirect calls and offsets, not their target functions.
No concrete vtable or static candidate target was resolved from the inspected
references; consequently slot0 cannot be classified as a deleting destructor.
The cleanup invocation at `0x8610C4` is `8B 01 6A 01 FF 10` (load vptr,
push 1, call slot0), which is compatible with but does not prove that
interpretation. It then calls `0x424790` with object+4; that helper reads the pointer
at object+0x320, whose allocation header is eight bytes
before the payload and has its reference count four bytes before the payload.
On final release, tag `0x17` calls the shared-text allocator singleton getter
`0x12B9F70`, then `0x12CDEC0` releases the allocator node.
That helper decrements allocator counter+0x10 and pushes the header node
through `0x12CDA60`'s interlocked lock-free stack update. Other nonzero tags go
to the general free routine with the header; tag0 frees the payload minus16.
The owner field is cleared after release. This identifies the tag-0x17
return-to-shared-text-allocator path, but does not establish the semantics of
other tags or all values ever stored at object+0x320.

IDA's current code-xref database lists one direct caller of `0x64E210`:
`0x494B00` sets connection state5, calls it at `0x494BDD`, then returns.
Inside cleanup, a guarded lookup of index0x6A can call `0x369A30` at raw
`0x64E275`; that helper forwards a stack-local record to `0x101D710` only when
a global flag is set. Its purpose is unresolved and it is not established as
an app-mode request. The direct stores in `0x64E210` do not target the
documented pending-mode queue fields, and this function does not call the
pending-mode inserter `0x30DEB0`; indirect virtual effects remain outside this
trace. This bounds the finding to the analyzed cleanup path, not the full
client transition graph.

`WorldZoneCleanupNativeLayout.cs` records these offsets/call boundaries and the
two-word observer gate. No complete world cleanup, lifetime management, or
app-mode transition is claimed. Spurious decompiler `st0` parameters are not
published as verified calling conventions.

### Deferred close after Destroyed

The tick's switch table raw `0xBD505C` maps socket state8 to `0xBD4FCD`.
After processing the receive/event queue, that branch skips when close-request
byte+D95 is already nonzero. Otherwise it tests keepalive helper `0xBD2BB0`,
then reliable-retry helper `0xBD2AD0`. Failure of either requests pump close.
Success calls data assembler `0xBD4340`; **the assembler's tail itself requests
pump close if the socket is still state8** (`0xBD475A..0xBD4765`). This explains
why destroy requests from Connecting/Connected can initially omit close
without leaving the normal tick path indefinitely waiting for queue drain.
It does not guarantee delivery/acknowledgement of every pending fragment.

Close callback `0xBD2D10` requires invalid socket handle+118 and zero completion
byte+D94. It sets+D94=1 and enqueues small event token1 into queue+260.
The callback does not directly write socket state. Queue dispatcher `0xBD3E50`
consumes this event: state8 invokes channel cleanup `0xBD06B0`, then becomes
Closed9; active states1..5 become DisconnectedNotify6. Already Disconnected7
or Closed9 ignore the event. State6 and other unsupported states take assertion
paths. These transitions are still distinct from MatrixConnection state.

`GameSocketCloseCompletionNativeLayout.cs` saves tick/callback/queued-event
selections without pretending the asynchronous phases are one atomic action.
Allocation ownership and the eventual socket-object free remain unresolved.

## Server messages41-43 and PvP queue records

The three adjacent factory cases are raw `0x33D7A6`, `0x33D809`,
`0x33D86C`; their constructors `0x33A560`, `0x33A7E0`, `0x33A930`
set codes41/42/43 and allocation sizes58h/38h/30h. Their normal-buffer
formats are saved in `MatrixServerMessages41To43Wire.cs`:

| Code | Wire body after compact code | Minimum packet size |
|---|---|---|
| 41 | LE32(+28), LE32(+2C), byte(+30), byte(+31), LE16(+32), byte(+34), optional list(+38), LE16-length raw blob(+48) | 17 absent;18 present-empty |
| 42 | Chunked record list(+28), LE32(+34) | 6 |
| 43 | LE32(+28), LE16(+2C) | 7 |

Message41 reader `0x3543F0` uses raw DWORD reader `0x353310`, byte/WORD
primitives, optional reader `0x354140`, and the existing word-length blob
reader `0x12EE170`. Optional marker0 is absent; any nonzero is present.
List reader `0x3537D0` writes native16-byte entries but reads only **12 wire
bytes** each: raw LE64 followed by four raw bytes. The last four native bytes
are not serialized; do not invent wire padding from SSE copies. The list has
byte-chunk counts, repeating only at255, so exact multiples255 require a
terminal0. Order and duplicates survive, unlike the EnterZone metadata maps.

Handler41 `0x4914A0` uses secondary receiver `this=connection+4`: its scalar
stores correspond to primary connection+A48 through+A54, optional destination
+A58. It copies the optional list, derives `descriptor.byte34 & 1` for
socket+100, and logs `"Everlasting Gobsocket %s"` when enabling that byte.
Keepalive helper `0xBD2BB0` separately demonstrates that this byte bypasses
the receive-age failure gate. The handler can also increase packed transport
WORD settings using descriptor+32 and a registry-provided ceiling, feeds
numeric observations into an existing subsystem, and clears the copied
optional list before returning. These latter algorithms, formal scalar/entry
meanings, and the trailing blob's consumer are not reconstructed by the
saved wire model.

Message42 is a **PvP queue update**, based on its downstream consumer
`0x85D560` and original `"PvP Queue succeeded.%s"` / `"PvP Queue cancelled"`
logs. Reader `0x3532D0` invokes vector reader `0x352EC0`, then raw DWORD+34.
Each native16-byte record has two raw LE32 words and two shared-text pointers;
on wire, the text fields are independent NUL-terminated raw byte sequences.
Text encoding and identities of these four fields are unresolved. Count
framing is the same255-chunk protocol and preserves record order/duplicates.

Handler42 `0x4918F0` requires connection+910, then calls the consumer only
when downstream+8 and that object's+C pointer are nonnull. For a nonempty
input list, the consumer copies it when descriptor word+34 is9 or equals
consumer word+78; otherwise it clears its own records. Empty input also
clears. **Runtime289's `"queued"` boolean comes from whether the original
input list is nonempty, even when a word mismatch causes the stored list to
be cleared.** It does not simply report whether records were accepted.
Shared-text aggregation and additional callbacks remain outside the bounded
selection model in `MatrixPvpQueueNativeLayout.cs`.

Message43 reader `0x352290` consumes a DWORD and WORD. Handler `0x491960`
checks the required connection+910 and reads its+8 member, but has no observed
payload consumption, runtime notification, or state mutation. Do not
discard the body merely because this handler currently does not use it.

## FoundMatchUpdate44 and challenge messages45-47

Factory cases raw `0x33D8CF`, `0x33D932`, `0x33D995`, `0x33D9F8`
allocate48h/48h/40h/40h descriptors through constructors `0x33AA10`,
`0x33AB30`, `0x33ACD0`, `0x33AEB0`. Saved normal-buffer decoding in
`MatrixServerMessages44To47Wire.cs` preserves these bodies:

| Code | Wire body | Minimum packet size |
|---|---|---|
| 44 | LE32 matchmaker ID(+28), LE64 match ID(+30), raw state byte(+38), four LE16 words(+3A/+3C/+3E/+40) | 22 |
| 45 | LE64 challenge ID(+28), three raw bytes(+30/+31/+32), LE32(+34), chunked record vector(+38), LE32(+44) | 21 |
| 46 /47 | LE64(+28), LE64(+30), NUL-terminated raw name bytes(+38) | 18 |

Message44 handler `0x4919B0` uses the same downstream+8/member+C gate as42
and invokes `0x85DB10`. Original log identifies it as `"FoundMatchUpdate"`
with `"matchmaker %u, matchId, 0x%016llx, state '%s'"`; these two IDs are not
guessed names. The four WORD meanings and state-name mapping remain opaque.
The consumer stores them at+62/+64/+66/+68 and state at+60. In its ordinary
update branch, states1/2/3 refresh deadline+6C only when the state changes or
the existing deadline is0; deadline is current millisecond tick plus
1000 times incoming WORD+40, with native DWORD wrapping. Other states clear
deadline. Repeated state2 or3 suppresses later notification work even if
other fields changed. An incoming matchmaker0 while an existing matchmaker
is nonzero takes a separate reset branch. Further callbacks, state4's
post-callback reset, and runtime289's null-body cancellation are documented
native effects but are not a complete match lifecycle simulator.

Message45 reader `0x352130` reads its three flags through `0x356CF0`;
**that reader copies raw bytes, not normalized bools**. Handler `0x491A40`
normalizes them via nonzero tests before calling `0x8FD6B0` at existing
current-mode-specific object+80. Native logs identify the response as joining
a challenge. Raw +34 is read on wire but no use is established in this
handler. Vector reader `0x351C10` preserves order/duplicates and255-count
continuation. Each native16-byte entry has a LE32 word, two NUL raw texts,
and three raw bytes at+C/+D/+E; the final padding byte is not serialized.
Entry semantic names and text encodings remain unresolved.

Join consumer `0x8FD6B0` requires component active pointer+64. For a valid
stored64-bit challenge ID, incoming ID must match. Stored0 or an ID whose
upper56 bits are all ones is accepted only with the second flag (+31)
nonzero. Otherwise it logs an unexpected response and returns. Accepted
first flag(+30)=0 releases/zeros the active object. Accepted success applies
the list/final DWORD through further helpers; second flag selects runtime269
`"successful":true` and writes the incoming challenge ID. Runtime270
`"reason"` is `"Squad"` when third flag(+32) is nonzero, otherwise `"Created"`
when the second flag is nonzero, otherwise `"Accepted"`. Runtime277 follows
with null body. `MatrixChallengeNativeLayout.cs` preserves the gates and
notification selection without performing ownership or runtime callbacks.

Messages46/47 have identical body readers `0x350F70`/`0x350EA0`. Handlers
`0x491A90`/`0x491BA0` build a temporary name/QWORD pair and call shared
invitation consumer `0x8FD960` on the same mode-specific component+80,
with final argument **0 for46,1 for47**. The consumer removes an existing
same-name invitation and can post runtime271 `"name"`/`"active"` changes,
send Character challenge-invitation responses, or construct squad-info
messages. Its complete invitation/party algorithm and exact identities of
the two incoming QWORD fields are still unresolved; the saved model names
only the confirmed variant flag and raw wire fields.

The shared `MatrixChunkedListWire.cs` retains list semantics for41/42/45;
it is intentionally not reused for EnterZone's sorted, duplicate-overwriting
metadata maps.

## Challenge invitation cancellation48, response49 and kicked50

Saved `MatrixServerMessages48To50Wire.cs` continues the challenge family:

| Code | Normal body after compact code | Minimum packet size |
|---|---|---|
| 48 | Raw LE64 challenge ID(+28) | 9 |
| 49 | Raw LE64 challenge ID(+28), NUL name bytes(+30), raw status byte(+34) | 11 |
| 50 | Raw LE64 challenge ID(+28), NUL kicker name(+30), raw LE64 kickee ID(+38), NUL kickee name(+40) | 19 |

Factory cases `0x33DA5B`, `0x33DAC1`, `0x33DB24` allocate30h/38h/48h via
constructors `0x33B090`, `0x33B170`, `0x33B330`. Case48 directly calls QWORD
reader `0x356F80`;49 uses `0x350D80`;50 uses `0x350CA0`. No native padding
is added to these bodies and text encodings are not established.

Handler48 `0x491C60` requires only a nonnull existing mode-specific object,
then calls cancellation consumer `0x8FE1A0` on component+80. Original log is
`"Received invitation cancel for challenge id 0x%016llx"`. It scans native
16-byte invitations and removes **all** exact matches of ID words+8/+C,
including adjacent duplicates; each removal posts runtime271 with `"name"`
and `"active":false`. It adjusts the index after removal rather than skipping
the next shifted entry. The saved snapshot selector returns matching indices;
it deliberately does not simulate mutations during callback reentrancy.

Handler49 `0x491C80` logs `"ChallengeInvitationResponse"` with incoming ID,
name and status, and calls consumer `0x8FE3E0` only when the mode-specific
object is nonnull. The ID is logged but no filtering by active challenge ID
is established at this handler/consumer boundary. Runtime272 gets `"name"`,
`"accepted"` **only when raw status is0**, and `"reason"` from table
VA1C6FCC0/raw186E8C0. All8 mappings and the explicit out-of-range fallback
are recovered:

| Status | Reason |
|---|---|
| 0 | `Accepted` |
| 1 | `Declined` |
| 2 | `DoesNotExist` |
| 3 | `Offline` |
| 4 | `TeamFull` |
| 5 | `MatchStarting` |
| 6 | `TargetNotChallengeable` |
| 7 | `SquadTeamFull` |
| 8..255 | `UNKNOWN` |

Handler50 `0x491CF0` requires mode-specific object+E4 and exact equality
of that object's first two DWORDs to descriptor challenge ID+28/+2C.
This +E4 is the same active pointer as component+80's+64, not a second
independent challenge field. It calls `0x8FABA0`, whose original
`"ChallengeKicked"` log and runtime274 properties establish `"kicker"`/
`"kickee"` name roles. After runtime274, kickee QWORD+38 is compared with
local-client+8. The native consumer **explicitly substitutes0 if the
local-client getter returned null**; equality then triggers another current-mode
lookup, active-object teardown/zero at+E4 and null-body runtime275.
The snapshot selection preserves that zero comparison without claiming
that zero is a valid identity or simulating pointer changes during callbacks.

`MatrixChallengeFollowupNativeLayout.cs` saves cancellation matches, all256
status-byte selections, the kicked handler gate and the local comparison.
It does not implement callback/property ownership or native object release.

## Challenge messages51-55: separate identity policies

`MatrixServerMessages51To55Wire.cs` saves three QWORD-only bodies (51/53/55,
packet9), message52's QWORD plus chunked record list (minimum packet10),
and message54's QWORD plus raw LE32 (packet13). Factory51/53/55 branches
share the earlier raw QWORD reader path at `0x33DA99`. Message52 reader
`0x350B60` calls `0x350820`;54 reader `0x34FB90` calls the QWORD/DWORD
primitives.

Message52's **native24-byte** record is raw LE64+0, NUL text+8, raw byte+C,
raw LE32+10, raw byte+14. The three padding bytes after+C and after+14
are not serialized. Empty text gives a15-byte wire element. Chunk255 requires
another count byte; order/duplicates survive. This is a different collection
from message45's native16-byte records and must not be merged with it.

All five handlers get the existing mode-specific object and its active
challenge pointer+E4. Their ID gates differ:

| Code / handler | Accepted active ID | Selected effect |
|---|---|---|
| 51 / `0x491D30` | Exact incoming match **or** stored0/upper56-all-FF | Consumer `0x8FE560`: log `"Left challenge"`, release/zero active object, runtime275 null body. |
| 52 / `0x491D90` | Exact match **or** same invalid stored-ID cases | Copy descriptor+30 vector to active+8 via `0x901C70`, runtime273 null body. |
| 53 / `0x491E10` | Exact match only | Runtime276 null body. |
| 54 / `0x491E60` | Exact match **or** same invalid stored-ID cases | `0x8FAD90(active,&descriptor.word30,1)`, runtime277. |
| 55 / `0x491EC0` | Exact match only | Runtime278 null body. |

The invalid stored-ID allowances do not imply incoming ID validation or a
general wildcard policy for every challenge message. Missing mode/active
pointer skips these effects.

Selector `0x8FAD90` writes incoming DWORD to active+14, initializes active+24
to all-FF, then scans the separate16-byte vector at active+18 for **first**
element whose DWORD+0 equals the incoming value. On match it stores the
zero-based index at+24. Its third argument controls runtime277 emission:
message54 supplies1, whereas the already-traced join-response consumer
supplies0 and emits its own follow-up. Formal meaning of the selected DWORD
is unresolved; it is not named a team/map/slot identifier without evidence.

`MatrixChallengeUpdateNativeLayout.cs` captures these gate/action/runtime
selections and the first-match index rule. Collection ownership, callback
reentrancy and meanings of runtime276/278 remain outside this bounded model.

## ForceUnqueue, remaining-buffer and collection messages56-61

`MatrixServerMessages56To61Wire.cs` saves these normal-buffer bodies:

| Code | Factory / reader raw offsets | Body, excluding compact header | Minimum packet |
|---|---|---|---|
| 56 | `0x33DD16` / `0x34FAA0` | byte28 reason, chunked QWORD list2C, DWORD38 | 7 |
| 57 | `0x33DD79` / `0x34F380` | **Entire remaining buffer**, copied through descriptor+28 virtual+10 | 1 |
| 58 | `0x33DDDC` / shared `0x356B70` | Five QWORDs followed by one nonzero-normalized byte | 42 |
| 59 | `0x33DE40` / `0x34F0D0` | Chunked QWORD list28, DWORD34 | 6 |
| 60 | `0x33DEA3` / two calls to `0x34E700` | Two independent chunked lists at28/34; each element WORD0 + NUL raw text4 | 3 |
| 61 | `0x33DF12` / `0x34D6D0` | Chunked roots: WORD0 then recursive node list4 | 2 |

Collections preserve order and duplicates. Every count255 requires a further
chunk; exact multiples terminate with0. Message57 has **no** WORD-length
prefix, chunk count or assumed text encoding. Its supplied span must end at
the native buffer boundary: bytes that would be trailing data for another
decoder are part of57's payload. Handler `0x491F10` obtains descriptor-buffer
pointer/length through virtual methods and, if global byte VA`0x1E4AC84` is
set, passes them to `0x124B3F0`.
RTTI now proves57 is `matrix_fury::SynchronizationRequest`; the bounded
integrity/dispatch envelope of that consumer is reconstructed below.

Original handler56 logs identify **ForceUnqueue**. Handler `0x496460`
forwards reason/DWORD/list to consumer `0x85DCC0` through the same gated
downstream pointer used by42/44. The consumer clears recorded queue state
and its record vector, then emits runtime291 properties `"reason"`,
`"local_player"`, `"text"`. Original20-entry reason table
VA`0x1C6FD10` is saved verbatim in `MatrixForceUnqueueNativeLayout.cs`:
code0 is mixed-case `"Unknown"`; codes20..255 use uppercase `"UNKNOWN"`.
The local-player comparison ignores the low8 ID bits, requires the remaining
56 bits to be nonzero, and substitutes0 when the local-client getter is null.
This is **not** the exact challenge-ID comparison policy. The model records
the raw DWORD that supplies `"text"` formatting, not an invented formatted
string; log-only entity searches and native property ownership are omitted.

Message59 is identified by original `"SuperPing timing names out of date?"`
diagnostics, but its **original descriptor class is `matrix_fury::SuperPong`**.
Existing SuperPing-labelled C# payload names are compatibility labels from
that diagnostic, not claims about the RTTI name.
Handler `0x496690` ignores an empty input vector. Otherwise it
samples the connection clock and appends one raw QWORD via `0x4849A0`.
DWORD34 equal1 forwards to `0x484280`; other values assert **six total**
QWORDs after append and log timing deltas. The C# decoder preserves the
received list, not the later clock sample; diagnostics and downstream timing
aggregation are not simulated.

Message60 handler `0x4967B0` only operates if global VA`0x1E6A470` is nonnull:
it passes both lists to `0xE64A00(global+190,...)`, then writes a sampled
DWORD tick minus5000 at global+18C, wrapping uint32.
`MatrixServerProfilerNames60NativeLayout.cs` now saves the consumer's logical
two-map updates. The first map is global+190 and the second global+1B0;
both use **complete unsigned16-bit keys**, despite the lookup's inferred
byte-pointer argument type. Lookup`0xE671C0` hashes both little-endian
key bytes with plain FNV1a and compares a WORD at node+8.
Initializer`0xE6AF80` stores the complete WORD and zeroes text pointer+0C
in a0x10-byte node. Each source vector uses8-byte entries, WORD0/text4.

Updates preserve unmentioned keys, find-or-insert new keys, then replace
the shared-text reference. Repeated keys are therefore last-value-wins,
including empty text. Empty lists do not clear either map, but the enabled
handler still writes the tick marker. Missing global skips everything.
The C# projection independently copies raw text bytes into two logical
dictionaries and reports missing-consumer explicitly; native reference
counts, pointer-equality fast paths, allocation limits and iteration order
are not simulated. The two tables' distinct **name categories** remain
unresolved; do not label them as threads/functions solely from profiler RTTI.

Message61 `0x4967F0` checks that same global and tail-calls
`0xE65290` with descriptor+28. Its missing-name/refresh gates are now
reconstructed below; no formal event identity or metric units are assigned.

Message61's native root stride is16: WORD0, padding, child-vector4.
Its recursive native node stride is32: WORD0, padding, child-vector4,
eight WORDs at10..1E. **Wire order is WORD0, those eight WORDs, then the
chunked child list**, not native memory order; a leaf takes19 wire bytes.
The node reader `0x34BE80` calls itself after the nine WORDs; the root reader
calls that same node reader after its single WORD. The saved decoder uses
an explicit stack so depth does not impose an arbitrary CLR recursion limit.
No field labels or relationship semantics are guessed.

### Message57 SynchronizationRequest integrity and routing envelope

`MatrixSynchronizationRequest57NativeLayout.cs` models a **post-transform**
buffer, excluding the compact57 header. Native consumer raw`0x124B3F0`
enters the critical section at owner+58. A nonzero byte at owner+70
rejects entry; the byte's formal state name remains unknown. Lengths below20
return false. With owner mode byte+1C nonzero, a function pointer at
`[owner+28]+14` is called with mutable buffer and total length **before**
digest verification. Its transformation and inverse remain unknown and are
not executed by the reconstruction.

The last16 bytes are a standard MD5 digest of all preceding bytes. Evidence:
the four standard initial state words in the consumer, all64 MD5 additive
constants and exact ordered rotations in transform`0x1283FE0`, standard
Boolean rounds, update`0x12847D0`, and finalizer`0x12848A0`. The finalizer
uses VA`0x1C0E4A0` padding (`80` then63 zero bytes), appends the original
64-bit bit count, and emits the four state words little-endian. Native
comparison tests all four trailing DWORDs; there is no additional hash
fold or secret key. This is legacy integrity checking, **not authenticated
origin verification**.

On a match, `0x124AC10` passes the bytes preceding the digest to
`0x120C250`: their final DWORD is an uncompressed allocation length, and
the earlier bytes are a zlib stream. The destination is prefilled with FF;
`0x1256A60` inflates with window15/finish4 and requires stream-end1 and
zero unused compressed input. It does **not** require total output to equal
the allocation length, so a shorter successful output leaves an FF suffix.
Only then does `0x11FC1C0` read the **inflated buffer's** first little-endian
WORD and advance its cursor2.
Nonzero sampled mode+1C selects registered-handler lookup at owner+38 through
`0x124C010`; zero selects direct dispatcher`0x124B7C0`. **This is not proven
queue insertion**: the lookup finds a hash-map entry and invokes its callback.
The key's formal meaning remains unresolved. Native returns
the downstream result, not merely digest validity; C# therefore reports
`EnvelopeVerified`, **not complete handling success**. Its dispatch and
external-transform selections describe sampled boundaries only, not native
locking, pointer ownership or downstream execution.

The minimum20 means at least4 preceding payload bytes for the length footer;
it does not establish a valid compressed stream or a readable inflated key.
Empty whole-remainder decoding for57
remains valid at the wire-reader layer and is rejected at this consumer
layer. Explicit C# outcomes distinguish gate rejection, short buffers and
digest mismatch; verified payload/footer are independent byte copies.
Integrity-only golden: ASCII`abcd` followed by
`E2FC714C4727EE9395F324CD2E7F331F`, raw prefix WORD`0x6261`.
This is not a valid native reader payload. `RawPayloadPrefixWord` deliberately
does not label that prefix a routing key. `NativeZlibBufferFraming` projects
captured inflater results; compression/inflation engines are not executed by
these offline projections, nor is a different compressor claimed byte-identical.

### Synchronization mode-zero fields and registered callbacks

`MatrixSynchronizationDispatchNativeLayout.cs` continues the bounded trace.
In mode0, `0x124E6E0` calls DWORD reader`0x1206B20` **twice**, consuming8
bytes after the already consumed leading WORD in the inflated reader buffer. Each read checks cursor+4,
reads a little-endian DWORD and advances4; short input throws natively.
The pair is **decoded DWORD values, not a pointer/length pair**. Only after
both reads does dispatcher`0x124B7C0` check:

| Gate | Native failure |
|---|---|
| Leading key must equal1 | Diagnostic callback receives raw`0x0FFACB37`, returns0 |
| Second decoded DWORD must equal8 | Diagnostic callback receives raw`0x0FA98FC0`, returns0 |

The second value8 is **not the remaining buffer length**. Trailing bytes
after the pair are not checked by this path. Thus a verified4-byte payload
can still fail reader construction or the mode0 readers; at least10 **inflated**
bytes are needed for these fields, not a fixed26-byte wire minimum.
Short data is detected before unsupported
key/value diagnostics. C# preserves completed read progress and reports an
explicit truncated-field outcome instead of attempting native exceptions.

The first DWORD is passed as the seed to`0x127FFD0`: it initializes624
words with `state[0]=seed` and wrapping
`state[i]=i+1812433253*(state[i-1]^(state[i-1]>>30))`, the MT19937 seeding
recurrence. The later activation calls remain unmodeled, while the bounded
optional digest slots are now saved below. If native activation reaches its tail at`0x124B9D2`, owner
mode+1C is set1. C# says `ActivationPathSelected`, not that initialization
or native side effects have run.

`MatrixSynchronizationModeZeroDigestSlots` projects the descriptor's two
16-byte slots at`+4` and`+14`: both are explicitly initialized to zero.
Helper`0x127C730` is called first into a reusable temporary; only a true
return copies it into the first slot. Adapter-address MD5`0x127D0C0`,
called at`0x124B93A`, then uses the same temporary and only copies to the
second slot on success. A failure must **not copy the previous helper's
stale bytes**. Either optional helper's false return leaves its corresponding
slot zero without rejecting the already-selected branch.
The adapter algorithm is saved in
[Startup](Startup.md#sorted-adapter-address-md5-offline-projection).
The first helper queries a Windows-directory-derived drive and a storage
descriptor. Its serial-number/CString MD5 projection is now saved in
[Startup](Startup.md#windows-drive-storage-serial-md5-offline-projection);
`ProjectCapturedFingerprints` wires the two offline result types together.

The C# projection returns independent copied flags and owned16-byte slots.
An unknown native result is not treated as false: its slot stays unresolved,
while the other slot can retain a known snapshot. Successful captures must
contain exactly16 bytes; failed attempts need no readable output. These
results describe captured slot contents, not successful native activation,
allocation or transformation-provider binding. Outgoing serialization is now
saved separately below.

In nonzero mode, `0x124C010` instead finds a key in the **hash map** at
owner+38 using`0x124C9B0`. Missing entry returns0; found entry's callback
pointer at node+0C is invoked through virtual slot0 with the key and the
reader reference, whose cursor is still2. Its returned byte is normalized
to bool. This corrects the earlier tentative **queued path** description:
no queue insertion has been established.

Bucket helper`0x1248050` hashes all4 little-endian bytes of the zero-extended
key using **plain FNV1a32**, without the engine's final mix. It computes
`bucket=hash&mask` (container+14), then uses the bucket unchanged if active
count (container+18) is greater; otherwise subtracts `1+(mask>>1)`, wrapping
uint32. Shared `EngineByteHash32.ComputeFnv1a` now supplies that first stage,
while existing `Compute` retains the complete chat/SDB final mix unchanged.
Golden plain hashes: key0→`4B95F515`,1→`FB69B604`,1234hex→`83CE9A1B`,
FFFFhex→`16C70CDB`. Callback registration, implementations, and external
transform function selection remain open.

### Mode-zero operation2 descriptor and compressed framing

`MatrixModeZeroDescriptorCodec` saves the serialized fields passed by
`0x124B7C0` into `0x124E010`/`0x124E720`. A WORD2 precedes:

| Native descriptor | Serialized value |
|---|---|
| +0 | Little-endian DWORD Windows version code |
| +4 | Raw16-byte Windows-drive serial digest, or initialized zeros |
| +14 | Raw16-byte sorted adapter-address digest, or initialized zeros |
| +24 / +28 | Opaque DWORD and ushort byte count followed by narrow bytes |
| +40 / +44 | Second opaque DWORD and ushort byte count followed by narrow bytes |

Offsets here are hex. Serializer`0x124FFE0` writes the DWORDs as raw4-byte
ranges; `0x12504E0` writes each digest's16 bytes; `0x120BEF0` rejects a
stored string length aboveFFFF and writes exactly that count, without a
terminator. Embedded zero/high-bit bytes are preserved. Metadata copier
`0x124AA60` takes two DWORD/wide-string groups from the singleton's+F8
member and runs each string through`0x1273530`. Its tail is now recovered:
folder/path normalization is attempted, then per-code-unit locale
`towlower`, middle shortening and strict UTF8 conversion. The provider and
metadata identities remain unresolved. The descriptor codec accepts captured
**converted narrow bytes**; the bounded tail below can produce those bytes
from already captured normalized/lowercased text.

`NativeWindowsVersionCode` projects`0x12800F0`: captured GetVersionExW
OSVERSIONINFOEXW284, failure→0, otherwise wrapping
`((major<<8)|minor)<<8 | servicePackMajor`. Its argument can suppress the
service-pack bits; mode0 passes zero. Exact packed codes50000,50004,
50100..50103,60000..60002,60100..60101,60200 map respectively to1..12;
other packed values greater than60200 map13, other values map0.
This legacy API result can be compatibility-dependent; it is not a claim
about the actual host OS, which was not queried.

`0x11E8910` replaces the serialized stream with zlib output (native literal
version1.2.8, default level-1→6, deflate8, window15, memory8, strategy0,
finish4), then appends the **original uncompressed byte length** as LE32.
`0x124BC10` appends MD5 of the compressed bytes **and that length footer**.
`NativeZlibBufferFraming.AppendLengthAndMd5` reconstructs the framing from
captured zlib bytes; it does not assume managed compression is byte-identical
to the native1.2.8 engine. Synthetic independent zlib fixtures are labeled
as such, not captured game traffic.

`0x124E010` calls either the explicit provider's+10 function pointer or
the owner's+28 provider's+10 pointer with the mutable framed buffer and
its length, then queues its owning buffer under the owner+4 lock.
The selector supplies the generated provider from`0x1285060`/`0x1286F00`.
That callback and later send/ownership behavior are not executed or
identified as a particular cipher. The saved bytes are **before callback
transformation**, not a complete outgoing packet or proof of activation.

### Metadata text shortening and strict UTF8 tail

`NativeMetadataTextTail` reconstructs`0x1283720`, `0x1258C70` and
`0x1258E90` after the earlier normalization and locale case-conversion
boundary. Text with at most64 UTF16 code units is unchanged. Longer text
is first reduced to its first32 and last32 code units; native replaces
three units starting at31 with UTF16 literal`...` (VA1C7EC5C).
Equivalently, the final64 units are **first31 + dots + last30**.
These are code-unit offsets, not Unicode scalar or grapheme counts.

Encoding walks the full stored range, preserving embedded zero. High
surrogates require a following low surrogate; an orphan low surrogate,
missing low surrogate or mismatched next unit triggers native invalid-UTF16
handling, not replacement characters. For a mismatched pair the first
native diagnostic carries the following unit. Shortening can itself split
an otherwise valid pair at either retained boundary; this failure is
preserved. Scalar encoder accepts0..10FFFF excluding D800..DFFF and emits
standard one-to-four-byte UTF8, without BOM or appended terminator.
The checks compare every valid Unicode scalar against independent standard
UTF8 and exercise exact64/65 boundaries and both surrogate-splitting edges.

`NativeFolderPrefixAlias` now saves preceding helper`0x126DAC0`. It first
trims **one** trailing slash/backslash through`0x126D750` and removes the
case-sensitive prefix `\\?\` when present. The four-unit global prefix is
proven by static initializer raw2E33D0 and UTF16 literal VA1C7EA48; it is
not inferred from current host state.

Folder IDs are tried in the fixed order254,255,37,41,36,38,42,5,28,26,
35,0,25,40,7. ID254 uses module-directory provider126CBD0(selector3); ID255
uses GetTempPathW(buffer260), allowing only return1..260; others use
SHGetFolderPathW and require HRESULT0. Captured available roots lose one
trailing separator. `0x1270D40` compares the prefix by applying locale
`towupper` to each UTF16 code unit. A matching root must consume the whole
candidate or be followed by **backslash**, not forward slash. The first
match wins, not the longest root. It replaces the prefix **and one following
backslash** with `<decimalID>`; root+`\Foo` becomes `<ID>Foo`, without
reinserting a separator. No match makes1273530 fall back to the **original**
input, discarding partial trimming/prefix removal.

C# uses captured root availability/paths and explicit captured case mapping.
An unknown earlier provider cannot be silently skipped; non-ASCII mapping
is not guessed by its explicit ASCII/C-locale helper. `0x1273530` then
invokes locale `towlower` before the saved shortening/encoding tail.
No host folders, temporary paths or locale state are queried. Metadata
ownership/meaning and full captured lowercase integration remain
open; the representation algorithms do not identify either opaque
DWORD/string pair.

`NativeModulePathProvider` projects provider126CBD0 from captured
GetModuleFileNameW(NULL) attempts and `_wsplitpath_s` outputs. Its initial
buffer length is260 UTF16 units. Every sampled GetLastError122 retries with
`260+4*previous` (1300,5460,22100...), **irrespective of the return count**.
Native does not clear LastError before querying: even a nonzero return can
retry on stale122. A terminal non122 error permits any nonzero return count;
zero returns false. Query success is followed by split-helper126D9E0, which
requires nonempty input and CRT result0, using drive capacity3 and directory,
filename and extension capacities256. The C# model accepts captured
components rather than substituting platform-dependent `Path` parsing.

The selector is an enum, not flags:0 full module path,1 filename without
extension,2 filename+extension,3 drive+directory. Unsupported selectors
return false **after** query/split gates. Selector3 supplies alias254, whose
trailing directory separator is removed by the alias routine. Missing
attempt/error/split/text captures remain unresolved. Original PE switch
table VA166DAE0 maps to raw126CEE0; correcting IDA's mistaken raw/VA table
mapping and four cases recovered decompilation without changing any
executable bytes.

### Generated transformation provider: saved initialization prefix

`NativeGeneratedTransformProviderPrefix` saves only the initial deterministic
stage of constructor1285060/generator1286F00. Constructor stores RNG at+8,
zeros DWORDs+4/+C/+10/+14, sets byte+18=1 and invokes generation.
Generation draws an inclusive modulo-reduced count60..100 **first**, sets
WORD+0=0200 and byte+2=3, allocates264 bytes at+C, then fills each byte from
one virtual UInt32 draw's low byte. Thus the saved prefix consumes exactly
265 UInt32 calls. `FromSeed` uses the existing MT19937 reconstruction; its
lazy initial twist preserves the native immediately-twisted stream.

Further generation selects candidate spans by raw sample modulo count,
does the selected number of descriptor/template iterations, and assembles
two outputs stored at+10/+14. Bounded helper evidence identifies12867D0
selection,12866C0 descriptor-dependent dispatch and12868D0 span concatenation.
Its allocation requests total span bytes+16 through1250F90 with size-class
paths. This does not establish executable page protection or a cipher.
The later caller invokes+10/+14 as mutable-buffer callbacks, but **no
generated byte buffer is executed or converted into a delegate** by C#.

`NativeGeneratedProviderBoundaryPlanner` now saves the abstract selector
schedule in builder helpers1285290/1285970 for provider DWORD+4==0.
The opening ten operations are29,26, six29s,26,26; descriptor codes are
42;42/41;37,38,39,40,43,44; headerByte0+37 with code42/offset8;
headerByte1+37 with code42/offset12. Closing uses six28s with reversed
codes44,43,40,39,38,37, then25 and33 with a16-bit value8.
Operation codes now have evidence-backed names:25 LEAVE (C9),
26 MOV (89/8B),28 POP (58+index),29 PUSH (50+index,6A/68),
33 near RET (C2 with16-bit immediate or C3 without operands).
Code37..44 width32/index0..7 therefore names EAX,ECX,EDX,EBX,ESP,EBP,ESI,EDI.
This is a **call plan**, not an emitted instruction encoding. Mode1's
different schedule and operand/template encoders remain unmodeled.

`NativeGeneratedBoundaryEncodingCatalog.InitialModeZero` separately saves
the proven boundary candidates for initial header0200/mode0. Opening bytes
are `55 {89 E5 | 8B EC} 50 51 52 53 56 57 8B 45 08 8B 55 0C`;
closing bytes are `5F 5E 5B 5A 59 58 C9 C2 08 00`.
The two MOV EBP,ESP alternatives retain static table order (variants1/3
of operation26); other initial boundary steps each match one template.
This preserves registers, loads EAX/EDX from stack offsets8/12, and returns
while removing8 argument bytes. Argument semantic types remain unproven.
Candidate construction precedes the selector's single UInt32 draw, even
when only one candidate exists. Wrapper condition/tail construction and
their selector draws occur between opening and closing; transformation
body fragments are already constructed before either wrapper runs.
The catalog consumes no RNG and is not a full routine.

`NativeGeneratedDescriptorFactories` projects the numeric code classifier
128A840 and the sparse constructors12506F0/1250780/1284F90/1250850.
Code0 produces kind/width/index0; bands1..20,21..36,37..52,53..68
produce kind4 with widths8/16/32/64 and indices relative to the band start.
Bands69..74,75..82,83..90,91..106,107..122,123..138 produce
kinds5/6/7/8/9/10 and widths16/64/64/128/32/32 respectively.
Out-of-range signed codes store the supplied DWORD before diagnostic
1040197660; no succeeding wrapper writes are modeled on that path.
Names of register families are not assumed.

Operation records have three64-byte operand slots at8/72/136.
Unary/binary constructors copy the entire supplied slots; absent slots
receive only the observed sparse field clears, **not**64 zero bytes.
Header+4 and unassigned operand bytes retain captured preexisting storage.
The C# projection requires nonoverlapping captured buffers; alias-dependent
SIMD copy behavior remains outside its contract.
Immediate operand factory1284EF0 writes kind2, value DWORDs at40/44,
width DWORD48 from the input's **low byte**, and byte56=1.
Its final explicit argument is unused; holes remain captured storage.

`NativeGeneratedImmediateBytes` saves1299D00's immediate-width dispatch:
it uses `bitWidth >> 3`, accepting byte counts1/2/4/8, not just exact
bit widths8/16/32/64. Other counts diagnose1040336665.
Bytes are the corresponding low1/2/4/8 little-endian bytes of the supplied
64-bit value; fixed native16-byte builder capacity remains a separate gate.
Its switch metadata was repaired from unchanged original PE targets at
raw1299DD8; no binary bytes were changed.
Both this switch and the module-path switch are now user-defined so IDA
automatic analysis cannot replace the corrected raw targets with PE VAs.

`NativeGeneratedCandidateRngAdvance` saves1286780: when byte+18 is zero,
sum the last selected candidate's unsigned bytes with UInt32 wrapping,
then discard sum%5 raw UInt32 draws. It emits no extra bytes.
Any nonzero byte+18 skips the candidate read and the RNG calls entirely.
Constructor's initial byte+18=1 therefore disables these advances.
No writes to byte+18 were observed in1286F00 or12868D0.

`NativeGeneratedBodyDecisionRules` saves initial mode0 decision prefixes
and the seven static forward/reverse pairs at PE VA1C0E464:
ADD/SUB, SUB/ADD, INC/DEC, DEC/INC, ROL/ROR, ROR/ROL, XOR/XOR,
selected by a raw UInt32 modulo7. Numeric IDs are1/41,41/1,6/5,5/6,
34/35,35/34,43/43; identities follow their native emitted x86 opcodes.
Unary/arithmetic parity selects BL(width8) or EBX(width32).
The rotation helper's first range0..2 draw chooses a count-register path
only on1 with initial header0,2,3; its next parity selects CL or ECX
for the preparatory immediate MOV, while the rotation itself uses CL.
Register arithmetic uses DL/EDX when the parameter is odd; otherwise
it first loads the parameter into CL/ECX.

Table-source arithmetic uses width8 for odd parameters, otherwise32;
scale is1/2/4/8 from bits1..2. For scale>1 it derives an ESI index and
shrinks the base-offset window before shifting the remaining parameter
by8. EDI receives the captured table address plus that bounded offset;
effective table offset is always below256. Original table allocation264
bytes accommodates the wider reads. Pointer addition wraps as UInt32;
the model does not dereference or obtain a host address.
Full template lowering and output routine behavior remain incomplete.

`NativeGeneratedConditionalBranches` saves JNZ18/JZ22 shaped candidates.
Short predicate is exactly `(offset & FFFFFF00)==0` excluding128/129:
it accepts0..127 and130..255, **not negative offsets**.
Accepted130..255 values truncate to negative signed x86 rel8 bytes.
This is a verified native quirk, not corrected to conventional rel8 bounds.
Near predicate excludes Int32 minimum through minimum+5. Negative near
offsets subtract6 before LE32 encoding; nonnegative offsets stay unchanged.
Candidate order is short then near; no matches diagnose1040188017.

`NativeGeneratedByteLoopWrapper.AssembleCapturedBody` saves12868D0's
initial mode0 wrapper around externally captured body bytes. It selects
opening, OR EDX,EDX, store BL->[EAX], INC EAX, DEC EDX, JNZ back,
JZ skip, closing—in that **construction/RNG order**—then concatenates
opening, OR/JZ, body, store/INC/DEC/JNZ, closing.
It consumes exactly24 raw UInt32 draws, including single-candidate steps.
OR has09D2/0BD2 alternatives; INC EAX has40/FFC0; DEC EDX has4A/FFCA.
Backward offset is minus(body length+store/INC/DEC length), with the JNZ
encoder subtracting its own6 bytes. Skip offset includes body and full tail.
Both output bodies are built before either wrapper; inverse records are
reversed in12-byte vector units, never reversing bytes within a candidate.
C# projects logical byte concatenation only: native executable protection,
allocator size classes/pointers and full body lowering remain unresolved.
Managed array/length overflow is explicit rather than emulated native wrap.

`NativeGeneratedScratchTemplates` lowers initial scratch INC/DEC and
byte ROL/ROR alternatives. BL unary has one FE candidate; EBX has compact
43/4B followed by FFC3/FFCB. Immediate byte rotations use C0C3/C0CB
plus the captured count byte. Count1 adds D0C3/D0CB **after** that candidate;
CL rotations use D2C3/D2CB. Counts are not normalized modulo8 in the
emitted data. `NativeGeneratedImmediateMoveTemplates` saves ordinary
mode0 register MOV preparation: B0+index/imm8 then C6/ModRM/imm8;
B8+index/imm32 then C7/ModRM/imm32, preserving native table order.
The captured table pointer is encoded as a DWORD, never obtained from the
host. `NativeGeneratedArithmeticTemplates` now lowers the initial BL/EBX
ADD/SUB/XOR forms. Direct immediates have exactly one80/81 group candidate;
there is **no83 sign-shortened candidate** in the eight-entry native tables.
Register CL/ECX or DL/EDX sources have r/m-first then register-first
alternatives in table order. Table sources have one register-first candidate:
ModRM1F for `[EDI]`, or ModRM1C/SIB77,B7,F7 for `[EDI+ESI*2/4/8]`.
The pointer/base-offset and index are separate preparatory MOV fragments;
the memory operand has zero displacement and no address-size prefix.
These projections do not cover arbitrary native descriptors or mode1.
`NativeGeneratedInitialModeZeroImage` now combines these recovered slices
into both logical byte images for a fresh provider. The caller supplies a
**captured numeric table address** because native allocation is outside the
model. All UInt32 samples are retained for replay; no host address is queried
and the byte arrays are never executed or made into delegates.

RNG ordering is265 prefix draws, one forward `MOV BL,[EAX]` selection,
all iteration draws, one inverse load selection, then24 selections for each
wrapper. Each iteration first draws the seven-pair selector. Unary pairs
consume five total draws; rotations consume one shared low-byte count and
two independent operand-form selections, six/eight/ten draws in total.
Arithmetic draws one shared modulo3 form and one shared UInt32 parameter,
then builds forward and inverse independently: seven draws for immediate,
odd-parameter register, or unindexed table forms; nine for even-parameter
register or indexed table forms. Forward fragments retain construction
order. Multi-fragment inverse groups are reversed locally, then the complete
inverse list is reversed after appending its load; thus preparatory MOVs
remain before their associated inverse operation.

All-zero samples produce60 iterations,735 calls and two405-byte images.
Constant2 produces62 unary iterations and625 calls; constant4 produces64
CL-path rotation iterations and955 calls. Scripted register/table streams
check independent parity/template selection and both reversals. This is a
complete **logical initial-mode0 byte-image projection**, not a reconstructed
native executable provider. Allocation and ownership are saved separately as
captured numerical plans; no native allocation, execution or complete mode1
image is supplied. Existing branch-displacement quirks are retained.

`GenerateWithCapturedControlByte` additionally accepts an explicit captured
byte+18 override. Any nonzero value disables post-candidate RNG discards;
zero enables byte-sum%5 raw draws at the exact native helper call sites.
Unary selections and rotation selections (including preparatory CL/ECX MOV)
advance; register-source arithmetic advances after its optional MOV and the
arithmetic selection; table-source arithmetic advances after optional ESI MOV
and the arithmetic selection. **Direct immediate arithmetic, table-base EDI
MOV, initial scratch loads and wrapper selections do not advance.** Each
discard happens before the next selection and before inverse-group reversal.
The full replay ledger includes discarded samples and reports their count.

The constructor still writes1: no native setter/entry proving a control0
provider was identified. The explicit override does not change the prefix's
constructor fact or the default `Generate`/`FromSeed` behavior. Captured-control
regressions include constant streams and a1395-draw register transcript whose
discard values would select different forms if accidentally reused.

`NativeGeneratedArenaPolicy` saves numerical output-allocation policy without
performing allocation. Arena1250F90 adds16 bytes with DWORD wrap, searches
one preferred list for total block sizes<=64/128/256 (arena offsets4/36/68),
then the large list at100. Best-fit1251050 scans under the list's critical
section, selects the smallest sufficient block, and retains the first entry
among equal sizes. A remainder>=32 is split and returned to its size-class
list; a shorter tail stays with the allocation. Returned payload is header+16.
Growth12510F0 rounds the required size to64KiB with wrapping DWORD arithmetic,
then uses the original PE import `KERNEL32!VirtualAlloc` with address0,
allocation flags0x3000 (`MEM_RESERVE|MEM_COMMIT`) and protection0x40
(`PAGE_EXECUTE_READWRITE`). It uses the same32-byte split threshold.
The projection exposes wrapping sizes, including zero; it does not pretend
an allocation succeeds. Critical-section import identities are independently
checked against original import slots. Free blocks have captured address
tokens, not dereferenceable managed pointers; mutated lists and native locks
are not simulated.

Provider destructors12850F0/1285140 release nonnull RNG+8 via virtual slot4
with deleting argument1, free nonnull table+C, then unconditionally call
output-return helper1257690 on+10 and+14. That helper lazily obtains the arena,
then returns nonnull payload-16 through1251160; it does not free the region
throughVirtualFree on this path. `NativeGeneratedProviderLifetimePlan` saves
this release order as data, including null output calls. Provider replacement
124B1A0 performs old-provider cleanup/free under the owner lock, assigns
owner+28, clears byte+24 and signals the owner's event.

Thunks1285120/1285130 are **tail jumps** through+10/+14, not no-argument
function calls as the decompiler suggests: caller stack words are retained.
The generated mode0 opening loads buffer/count from EBP+8/+C and closes
withRET8. These are two32-bit stack arguments with callee stack cleanup;
no static xrefs were found to these thunk entries. The two observed direct
provider consumers do not use a callback return value: incoming124B3F0 calls
provider+14 at124B48D before MD5; outgoing124E010 calls provider+10 at124E085
before the queue lock. This does not establish a meaningful generated return
value or all possible callers. Saved ABI constants are facts about the emitted
mode0 image, not callable C# delegates.

`NativeGeneratedInitialModeZeroByteTransform` additionally saves the
**managed low-byte algorithm** for positive-length buffers. It evaluates
recorded logical steps, not the emitted machine-code arrays. INC/DEC and
ADD/SUB/XOR reduce to wrapping byte effects even when native templates use
EBX; higher scratch bits do not feed any later low-byte operation. ROL/ROR
affectBL only. The operand is the captured parameter's low byte, the
remaining buffer count's low byte for DL/EDX sources, or the table byte at
the recovered relative offset. Count starts at input length and decreases
once per byte. Rotate effects use count modulo8; emitted count bytes remain
unchanged in the image model.

Forward/inverse recorded steps round-trip all256 input values across
513-byte inputs and32 MT seeds, including the255/256 remaining-count
boundary. An independent Python MT/body-semantics reference for seed0
gives61 iterations,684 pre-wrapper draws,732 total draws and forward
SHA256 `42ff381c351aa6610e533d4d4716c49e7dac67a595c5db92f193e839096a6cf7`
for input byte`i&255` over513 bytes; this is retained as a managed regression.
This does not assign a named cipher or simulate full CPU flags,
pointer validity, register return values or side effects. Empty input is
explicitly unsupported: the native zero-count JZ can have the previously
recovered wrapped rel8 displacement, so returning an empty success would
invent control-flow safety.

Outgoing descriptor consumer124E010 invokes provider+10 on the complete
compressed/length/MD5 frame before queueing it. Incoming Request57 uses
provider+14 before MD5 verification. The opt-in
`VerifyWithReconstructedInitialProvider` method supplies the managed inverse
only when the existing gate/minimum-length/mode test selects transformation
and the caller identifies the provider as this recovered fresh mode0 type.
Skipped paths retain existing behavior and need no provider. Selected paths
require a complete snapshot; unknown providers are not silently substituted.
The existing post-transform verifier and captured-data APIs are unchanged.

The **mode1 boundary branch** is separately saved, without claiming a
mode1 constructor or whole body. `ForModeOne` records PUSH
RAX,RCX,RDX,RBX,RSI,RDI, then MOV header-derived64-bit registers from
`[RSP+32]` and`[RSP+24]`. These are the saved incoming RCX/RDX values after
six pushes. Closing POPs reverse that order and emits plainRET, with no
LEAVE or immediate stack-pop value. With default header0200,
`ModeOneDefaultHeader` emits:
`505152535657488B44E420488B54E418` and`5F5E5B5A5958C3`.
There are eight opening and seven closing selector calls.

The SIB byte is **E4, not conventional24**: original129A320 sets
scale bits to3 when no index is present. Index4 still means no index,
so those scale bits do not affect the address, but they are retained in
the bytes. MOV hasREX.W; PUSH/POP ordinary64-bit registers need noREX.
The original constructor writes mode0 unconditionally. No established
caller sets mode1. The shared captured-mode projection below exposes mode1
explicitly; the original mode0 convenience API does not silently select it.

`NativeGeneratedModeOneMemoryShape` saves the separate address validator:
present base/index operands must be kind4 with width32 or64; simultaneous
operands must have equal widths; an index with full register index4 (ESP/RSP)
is rejected, while index12 is allowed. Scale must be1/2/4/8, and scaled
forms require an index. Accepted shapes return whether a67 address-size
override is required by a32-bit address register, **not a success boolean**.
The managed result distinguishes acceptance from this returned flag and
preserves explicit diagnostic codes. Checks cover8712 shapes/2073 accepted
combinations.

The separate129A7F0 fast-path predicate is scale1/no index/base index!=4.
It compares the full index, not index modulo8; do not replace this with
canonical R12/R13 addressing assumptions. No complete mode1 encoder is
claimed. Two mode1 switch tables were PE-mapped and restored withSWI_USER:
129A4C1 -> raw129A584 and129A734 -> raw129A7D8 plus byte-index129A7E4.
Original validator return ends at129A76E; throw tail ends at129A7D8 before
its tables. The separate predicate is129A7F0..129A809, previously incorrectly
absorbed into the validator. Calculated targets match original bytes;
cached decompiler block recognition can still be incomplete.

`NativeGeneratedModeOneMemoryEncoding` additionally projects captured
ModRM/SIB/displacement writes, address-size-prefix selection and REX X/B bits
to OR into the caller's prefix. It preserves the captured ModRM register
field, signed-byte displacement boundaries, mandatory zero displacement for
RBP/indexed R13, and DWORD displacement for an absent base. The default
mode1 boundary catalog now uses this shared address projection and retains
its original byte goldens.

Native no-index R12/R13 fast paths emit rm4/rm5 without the canonical extra
SIB/mandatory DWORD displacement; the managed projection deliberately retains
these raw fields and REX.B rather than repairing them into ordinary x64
instructions. Goldens and18657 accepted boundary combinations cover the
recovered writes. This is **not a complete instruction encoder**: caller
opcode/REX.W/REX.R, prefix ordering and buffer-slot/capacity effects remain
separate. The default-header body assembler supplies its recovered instruction
forms explicitly; this memory-field API is not a general x64 assembler.

`ForModeOneOrdinaryRegister` saves immediate MOV candidates for ordinary
byte and64-bit registers. For a64-bit destination and a32-bit source descriptor,
only `48 C7 /0 imm32` matches; RCX/RSI preparatory MOVs do not acquire an
imm64 compact alternative. For a64-bit source descriptor, `48 B8+index imm64`
comes first, followed by C7 only when the full value is the sign extension
of its low DWORD. Original table-address construction hasCDQ at128624F:
wrapped32-bit pointer arithmetic is sign-extended, not zero-extended.
The byte form reuses the unchanged ordinary byte candidates. This catalog
does not claim extended-register destinations.

`UnaryForModeOneFromConsumedParitySample` saves initial scratch selection
BL/RBX and its one matching INC/DEC candidate: `FE C3`/`FE CB` or
`48 FF C3`/`48 FF CB`. The explicit ordinary-register variant also covers
the corresponding byte and64-bit register indexes0..7. Compact40..4F
alternatives are mode0-only, not mode1 REX-prefix substitutes. Byte rotation
candidates remain the existing BL immediate/CL forms.

Explicit mode1 body decisions and ADD/SUB/XOR candidates are also saved.
Parity chooses BL/RBX and CL/RCX or DL/RDX; the wide preparatory immediate
descriptor remains32-bit. Wide immediate arithmetic has one REX.W81 imm32
candidate (no83/imm64), register forms retain their two ordered alternatives,
and table forms use RDI/RSI with one register-first alternative. The264-byte
table bounds include64-bit reads through offset255. These components feed the
captured default-header assembler; they do not establish a native mode1 entry.

`AssembleCapturedBodyForModeOne` saves the enclosing64-bit byte loop: RDX
OR-test, BL store throughRAX, RAX increment, RDX decrement, the original
JZ/JNZ displacement choices, and the saved opening/closing sequence.
It consumes21 selector draws per wrapper versus24 in mode0, including draws
for singleton candidates. Branch128/129 exclusions and wrapped130..255
short displacements are retained, not normalized into canonical branches.

`NativeGeneratedDefaultHeaderImage.GenerateCaptured` now composes complete
logical forward/inverse images for explicit mode0/1 and captured byte+18.
Both architectures share the same body draw-order implementation and local
then global inverse reversals. The constructor's prefix facts stay0/1;
`ModeWordAt4DuringGeneration` and`ControlByteAt18DuringGeneration` separately
record the supplied overrides. A known table-address DWORD is supplied by
the caller; no host pointer is discovered.

Mode1 all-zero stream:60 iterations,687 pre-wrapper draws,729 total and
two468-byte images. Constant2:619 draws; constant4:949 draws. Control0
constant2 adds124 discarded draws; constant4 adds576. Seed0:61 iterations,
684 pre-wrapper draws and726 total. `NativeGeneratedDefaultHeaderByteTransform`
evaluates recorded low-byte operations directly, not emitted machine code.
Default-control mode0/mode1 semantic outputs agree across32 seeds and retain
the independent513-byte seed0 digest; both captured-mode control0/1 variants
have positive-length round-trip and full transcript-replay checks.

The original `NativeGeneratedInitialModeZeroImage` and byte-transform APIs
remain compatible entry points over this shared implementation, including the
Request57 opt-in consumer. Arbitrary headers, other modes, empty-buffer safety,
full CPU/ABI effects, native allocations and arena region teardown remain
unreconstructed. No native mode1/control0 entry has been established.

Previously undefined helpers were recovered from verified original bytes
with explicit ends128596C/1285E27 and verified ECX-this/one-stack-argument
ABI. Original failure was call analysis; corrected signatures expose the
mode0 bodies. Unsupported mode diagnostics carry3E377377. Exact logical
buffers and generation RNG consumption are saved for the default-header
mode0 and explicitly captured mode1 cases above; other headers remain incomplete.
Both observed RNG
calls at12866EE and1286700 use virtual slot0; a decompiler's apparent
64-bit temporary is not proof of a separate UInt64 RNG method or two-word
consumption. The C# prefix does not make that inference, emulate native
pointer ownership/allocation, or claim that construction has completed.

### Native MT19937 seed, twist and UInt32 output

`NativeAlgorithms\NativeMt19937.cs` saves the generator itself. Native
allocation is0x9C8: vtable at0, index DWORD4,624 state DWORDs at8.
Vtable VA`0x1C7EBAC` slot0 points to UInt32 output raw`0x127FEA0`;
slot4 is a deleting destructor, **not the output method**.
Seeding`0x127FFD0` completes with index624. Twist`0x127FF20` uses the
standard397-word lag, upper/lower masks`80000000`/`7FFFFFFF`,
and table`{0,9908B0DF}` at VA`0x1C7EBA4`. Its227-word /396-word /
last-word split performs the standard **in-place** update.

Output twists when index equals624, reads/increments the index, then
tempers with right11, left7/mask`9D2C5680`, left15/mask`EFC60000`,
right18. Native expresses the two left steps as masks`FF3A58AD` and
`FFFFDF8C` **before** shifts; their shifted uint32 masks are the same.
C# uses the conventional equivalent form and an in-place modulo-index
twist, verified against a separate native split-loop projection across
multiple624-word boundaries. Seed5489 begins
`3499211612,581869302,3890346734,3586334585,545404204`.
Seeds0,1,5489,FFFFFFFF and reseeding retain unsigned wrapping behavior.

The model requires an explicit seed and performs no native activation.
It is mutable, unsynchronized and **not cryptographically secure**.
The inclusive unsigned wrapper is now saved below. Floating-point
wrappers and other surrounding consumers remain outside this implementation.

### MT19937 array initialization and inclusive unsigned range

Array seeder`0x1280010` is standard `init_by_array`: begin with scalar
seed19650218 (the operand shown as address`loc_12BD6A7+3` is a **numeric
constant**, not dereferenced memory), mix `max(624,keyCount)` iterations
using1664525 and cyclic seed words/index, then623 iterations using
1566083941 and subtracting the state index. At every state-index wrap it
copies state623 to state0 and restarts at1; it finishes with state0
`0x80000000`. `NativeMt19937` saves nonempty-word constructor/reseed
overloads with uint32 wrapping and no retained caller-array dependency.
An empty array is explicitly rejected: native code would read word0
despite count0, not choose a safe default.

Native constructors`0x127FE20` (scalar) and`0x127FE70` (array) twist
immediately after seeding. C# defers that identical twist to first output;
the exposed sequence is preserved without claiming allocation/init timing.
Canonical seed words `[0x123,0x234,0x345,0x456]` begin
`1067595299,955945823,477289528,4107218783,4228976476`.
Independent standard-library goldens cover key counts1/623/624/625/1000
and outputs around624/1248-word boundaries.

Generic wrapper`0x127FD70` calls virtual slot0 **before** checking bounds.
It rejects unsigned lower>upper through an exception object carrying raw
code`0x1C271921`; valid input uses
`lower + sample % uint32(upper-lower+1)`, with inclusive endpoints.
There is no rejection sampling or modulo-bias correction, and even a
single-value interval consumes a sample. The full interval
`[0,0xFFFFFFFF]` wraps the divisor to0 and reaches native unsigned division
by zero; it does **not** just return the original sample.

`NativeRandomInclusive32` reports accepted/invalid-bounds/wrapped-divisor
outcomes with the consumed sample. `NativeMt19937.NextUInt32Inclusive`
advances the generator before these outcomes, matching native draw order.
No native exception object, hardware fault, callback or activation is
executed. `Draw` accepts an injected raw generator, since the wrapper is
not specific to MT19937; `ProjectConsumedSample` selects the pure arithmetic
stage only. Float wrappers remain untraced.

### Owning byte-buffer comparison and heap algorithms

The routines immediately before the generic random wrapper are **not**
random/float transforms over three scalar values. Their 12-byte records
are owning byte-buffer headers: begin/end/capacity pointers. Copy constructor
`0x3C60C0` zeroes these pointers, allocates the byte length and copies content;
assignment`0x3C7AF0` handles self-assignment, empty sources and capacity reuse.
`NativeByteVectorHeap.cs` saves a content-only projection, not native pointers,
allocation timing/capacity, destructor execution or allocation failures.
Writes copy bytes; saved heap values are independent snapshots.

Comparator`0x127C8F0` uses `min(leftByteLength,rightByteLength)`, skips equal
four-byte groups, and compares the first differing **unsigned byte**.
At the end of the common prefix it returns false, with **no length
tie-break**. Empty buffers compare less than nothing, and `[1]` / `[1,2]`
compare false in both directions. This is not ordinary lexicographic order.
For arbitrary unequal lengths its induced equivalence is nontransitive:
`[1]` is equivalent to `[1,2]` and `[1,3]`, but `[1,2] < [1,3]`.
Do not use it as a general ordered-set equality/comparer, or silently repair it.
Equal-length records have ordinary unsigned lexicographic order; an external
equal-length invariant is **not established**.

The saved algorithms preserve these comparator decisions:

- `0x127F550`: make-heap, processing parents from `count/2-1` down to zero.
- `0x127FA80`: adjust a hole downward, comparing right child against left
  and choosing **right on equivalence**; handle a lone left child, then
  push the saved value back up toward the original hole.
- `0x127FBD0`: push a saved value upward only while parent is less, bounded
  by the supplied top index; equivalent values do not rise.
- `0x127FB10` / `0x127FC50`: duplicate pop-heap bodies, saving last content,
  assigning root content to last, and adjusting the shortened prefix.
- `0x127F610`: repeatedly pop until the remaining prefix has one element;
  it does **not** first make an arbitrary range into a heap.

`MakeHeap`, `SortHeap`, `PopHeap`, `AdjustHole` and `PushSavedValue` operate
on spans of byte memories. Empty make/sort are no-ops; singleton pop retains
its contents. Empty pop and invalid hole/top accesses are rejected explicitly
instead of performing native invalid reads. Tests cover prefix/empty/high-byte
behavior, four-byte boundaries, right-child ties, bounded upward motion,
copied ownership, duplicate multiplicities and fixed-width heap/sort results.
No global ordering guarantee is claimed for arbitrary prefix-equivalent lengths.

### Byte-buffer introsort, pivot partition and insertion

`NativeByteVectorSort.cs` saves the full bounded sorter`0x127E8A0`,
partition`0x127EB60`, pivot helper`0x127F3D0`, order-three`0x127F8F0`
and insertion`0x127F6F0`. Wrapper`0x127E4F0` initializes its ideal budget
to the **element count**, not a logarithmic depth;`0x127E6D0` does the same
while forwarding an opaque token. Selected helpers never use that token to
change the comparison. `Sort` models the count-initialized wrapper;
`SortUnchecked` exposes the explicit signed budget, including zero/negative.

Count at most32 is handled first: zero/one are no-ops and2..32 use insertion
even with an exhausted budget. Larger ranges with positive ideal partition,
then reduce ideal to `ideal/2 + (ideal/2)/2`. They recursively process the
smaller outside range and iterate on the larger; on a tie, recurse right and
iterate left. Nonpositive ideal on a larger range triggers make-heap plus
sort-heap. Managed trace counters expose these decisions for inspection;
they are **not native state or serialized fields**.

The pivot is the middle index `count/2`. Counts up to41 order the
first/middle/last buffers by three pairwise less checks. Starting at42,
the helper uses a ninther: `step=count/8`, order triples around the first,
middle and last positions, then order their middle members. The native
threshold compares `(last-first)/12` to40, hence the inclusive41-record
median-of-three boundary.

The partition expands adjacent comparator-equivalent buffers around the
pivot, scans forward/backward, absorbs further equivalent buffers and rotates
the pivot block when one scan exhausts. It returns a half-open index pair for
that block. **The pivot representative is not captured once:** comparison
reads the current first pivot buffer. Absorbing a shorter matching prefix,
especially an empty buffer, can therefore change which other buffers are
classified equivalent. Tests explicitly preserve this behavior.

Insertion saves each next buffer, checks it against the **first** buffer,
and if less shifts the entire prefix backward. Only the other branch shifts
individual greater predecessors. With `[02,empty,01]`, native yields
`[01,02,empty]`; a conventional guarded insertion implementation could leave
the input unchanged. The saved model does not replace this special branch.
Buffer swaps/assignments retain content-copy ownership, not pointer moves.

Partition's original`0x650`-byte neighborhood was verified unchanged against
the original executable before adding a bounded IDA function definition.
Instruction analysis establishes hidden result pointer in ECX, first pointer
in EDX, last/token on the stack, caller cleanup, and two pointer stores for
the returned boundaries. Correcting helper calling conventions removes the
spurious stack warnings/arguments in the old decompilation. One instruction
remains rendered as data/JUMPOUT by IDA; its exact original bytes are decoded
with Capstone, not inferred from that jump. No executable bytes were patched.

Fixed-width randomized/sorted/reversed/all-equal/duplicate-heavy checks cover
32/42 thresholds, forced heap fallbacks, all partition rotation directions
and records through2048 entries. No general sorting guarantee is claimed
for arbitrary unequal-length prefixes. A concrete caller`0x127D0C0` sorts
enumerated adapter address buffers before concatenated MD5 hashing; its
verified import binding, registry filter and offline projection are documented
in [Startup's adapter-address section](Startup.md#sorted-adapter-address-md5-offline-projection).

### Profiler frame61 missing-name collection and refresh gates

`MatrixServerProfilerFrame61NativeLayout.cs` saves the bounded portion of
consumer`0xE65290`. Root collector`0xE669F0` looks up each root WORD in the
map at owner+190; recursive node collector`0xE66B90` looks up each node
WORD in owner+1B0. This establishes the maps' **root-name / node-name
roles**, not formal thread/function categories. State exposes `RootNames`
and `NodeNames` aliases. Missing entries append IDs to two WORD vectors.
Root order and node **depth-first preorder** are retained; repeated missing
IDs are **not deduplicated**, and a present key with empty text is known.
The C# collector walks iteratively without imposing a recursion limit.
Native temporary subtree copies/refcounts are not reproduced.

If either missing-ID vector is nonempty, native samples its DWORD clock.
Only unsigned wrapping `now-owner[18C] > 5000` enters the request boundary;
equality does **not**. It stores `now` at18C **before** resolving an
endpoint through global VA`0x1DFC82C` object's virtual60. A nonnull endpoint
receives virtual8 with opaque owner DWORD1D0 and both missing-ID vectors.
An absent endpoint therefore still advances the throttle marker. Actual
request encoding/dispatch is unresolved; C# reports selection, not success.
Message60's `tick-5000` store means same-tick elapsed5000 remains throttled,
and the next tick can select this boundary.

Separately, byte VA`0x1E49A5A` equal0 selects opaque refresh calls
`0xE8D440` and`0xE8DB00` on owner+4. A nonzero byte skips those calls,
**not** missing-name traversal or request throttling. Later owner+0C
null-handle construction and frame-data/rendering semantics remain outside
the saved projection. The eight WORD metrics per node remain unnamed.

IDA initially misclassified raw`0xE65300..0xE653DE` as switch-table data.
The original entry/ret4/padding and all instructions were verified;
only that222-byte analysis region was redefined as code inside function
`0xE65290..0xE653DE`. Every executable byte was preserved.

### Profiler61 metric expansion, numeric aggregation and80-frame history

`NativeAlgorithms\NativeProfilerSamples.cs` now reconstructs the numeric
stage, without native tree widgets or rendering. Decoder`0x1303020`
uses the static65536-entry float table at VA`0x1C165B8`; **every finite
entry** was checked against IEEE binary16-to-float32 conversion. The
nonfinite entries classify as NaN/infinity. Selected values are multiplied
by float32 **65536** (bits`47800000`, VA`0x1C821D4`), passed to the shared
signed64 conversion entry`0x159DE70`, and only low32 retained.
This establishes scaled numeric representation, **not metric units**.

| Source node offset | Saved sample offset | Conversion |
|---|---|---|
| 10 | WORD0 | Raw WORD, later used as exclusive divisor |
| 12 | DWORD4 | binary16 → float32 ×65536 → truncated signed64 low32 |
| 14 | DWORD8 | Same scaled conversion |
| 16 | DWORD0C | Same scaled conversion |
| 18 | WORD10 | Raw WORD |
| 1A | WORD12 | Raw WORD |
| 1C | DWORD14 | Same scaled conversion |
| 1E | DWORD18 | Same scaled conversion |

Native sample stride is28; padding at2 is not modeled or serialized.
Finite negatives are not sanitized: encoded BC00(-1) becomes FFFF0000,
7BFF becomes FFE00000, and FBFF becomes00200000 after low32 wrapping.
Tiny finite values truncate toward0. The C# decoder explicitly declines
NaN/infinity instead of claiming to emulate native CRT exceptional-FP
handling; it reports the failing source WORD index and no partial sample.

Numeric updater`0xE8D760` uses a dedicated kind3 child with the same ID for
an **exclusive** sample. Starting from decoded DWORD4, it subtracts each
processed child DWORD4, saturating at0 **per child**. Exclusive DWORD8/0C
both become that remainder divided by WORD0, or the undivided remainder
when WORD0 is0. Exclusive late fields keep the decoded values. The regular
inclusive sample retains its decoded first fields while adding children's
WORD10/12 and DWORD14/18, with16/32-bit wrapping respectively.

Root updater`0xE8DB00` sums direct child DWORD4, sets all three DWORD4/8/0C
to that total, sets WORD0=0, and sums the four late fields with native
wrapping. The pure projections require already processed/matched child
samples: UI node construction, missing-node assertions, selection recovery
and sibling matching remain outside the API.

History initializer`0xE8E090` fills **all80 slots** with a supplied28-byte
template and sets next-slot0. Samples start at object+14, index at+8D4.
Writes replace the current slot then advance modulo80; latest is
`(next+79)%80`, including the template before the first append.
`NativeProfilerHistory80` preserves this filled-history behavior, rather
than treating unused slots as absent. Native object kinds, keys and
shared-text pointers around the history are not recreated.

### Profiler persistent sibling-tree merge

`NativeAlgorithms\NativeProfilerTree.cs` saves the structural stage of
root merger`0xE8D440` and child merger`0xE8D010`.
Native kind1 is a root, kind2 a regular node, and kind3 its dedicated
exclusive child. Kind is DWORD+8D8 and identity a full WORD+8DC.
Parent/next-sibling/first-child links are+4/+8/+0C. The managed projection
uses ordered lists and stable object references; its top-level parent is
null instead of reproducing the hidden native root-holder object.

Each incoming level searches **only forward** from the current sibling
cursor for a matching ID. A match retains that object; a miss inserts a
new object immediately after the previous matched/inserted sibling (or at
the head), **before** any remaining stale siblings. It never globally
indexes by ID, moves matched objects, or deletes absent siblings.
For example, existing roots `[1old,2]` followed by frame `[2,1]` become
`[1old,2,1new]`, not a reordered or deduplicated map. Duplicate source IDs
likewise remain separate objects. Empty frames/child lists preserve stale
structure.

A kind2 node ensures its first child is kind3 with the **same ID**, even
when the incoming child list is empty. Its real incoming children are
matched after that special child; roots do not get an exclusive child.
The native regular-child level first ensures all siblings, then descends
in source order. C# uses an explicit task stack and preserves these final
structural effects without a recursion-depth cap.

`MergeStructure` deliberately updates **structure only**. It does not
append numeric histories, resolve names, emulate allocation order/failures,
change expansion flags, or recover selection. The optional
`MergeWithSelection` stage below adds only the proven selection/expansion
rules, still without executing the UI.

### Profiler selection and visible-row traversal

Identity search`0xE8D3F0` compares DWORD kind and full WORD ID, then searches
children recursively in sibling-order preorder. It ignores expansion
flags and returns the **first** match, including stale objects and kind3
exclusive children. Native `0xE72480` is instead a generic tree
**visible-descendant index lookup**, not a constructor/handle allocator.
It only descends if parent byte+10 is nonzero, counts each child before
recursing, and excludes the starting container from row numbering.
`FindVisibleRow` saves the zero-initialized cursor projection; the hidden
container's expansion state is an explicit input. New profiler nodes
start collapsed (`0xE8E090` writes WORD+10=`0x0100`).

Root merger`0xE8D440` remembers the previous selection's kind/ID. An exact
object visited while merging retains selection without changing expansion
flags. Visit tracking includes the exclusive first child of every matched
regular node, even with an empty incoming child list. Otherwise it searches
the persistent tree by remembered identity. A successful recovery expands
**ancestors only**, including the hidden container, and need not choose a
currently reported node: the first stale duplicate can win. If no prior
identity matches (or there was no selection), it selects visible row0;
that can be null for a collapsed/empty container.

`MergeWithSelection` saves these outcomes with stable object references.
`IsExpanded` is offline metadata, not a GUI operation. The native hidden
container and arbitrary non-profiler widget nodes are not instantiated by
this model; search/recovery covers kinds1/2/3 in the saved profiler tree.
After the optional refresh, frame consumer`0xE65290` independently fills a
null selected-node pointer at owner+0C via the same row0 lookup
(`0xE65392`). This fallback still runs when refresh is suppressed. Owner+8
is the hidden container pointer; the former tentative "lazy handle
creation" interpretation is superseded. Callers modeling suppressed
refresh can use `FindVisibleRow` without performing a structural merge.

### Profiler graph sample refresh and positional root-child mismatch

`NativeAlgorithms\NativeProfilerFrameRefresh.cs` couples persistent tree
objects to the saved numeric projections and80-slot histories. Normally
call `MergeStructure` (or `MergeWithSelection`) first, then `Apply`.
Structural merges alone leave histories unchanged; new nodes'80 slots
contain zero templates. The sample stage updates only currently traversed
nodes; untouched stale nodes retain their previous samples and next-slot.

Root updater`0xE8DB00` finds each root **forward by WORD ID**, but pairs its
incoming direct children with stored direct siblings **by position, with
no ID check/search**. This differs from structural matching and must not
be silently normalized. Existing children `[20old,40]`, incoming `[40]`
leave that structure intact; the numeric updater then writes40's sample
into20old's history, leaves40's history unchanged, and aggregates the
updated20old sample for the root. This observed mismatch is preserved and
counted in `PositionalIdentityMismatches`; it is not a claim about intended
behavior.

Regular-node updater`0xE8D760` validates its same-ID exclusive first child,
decodes its own metric words, then searches real children **forward by
WORD ID**. A missing child is skipped without recursion, metric decoding,
subtraction, or late-field aggregation; after exhausting that cursor,
remaining source children are also skipped. Source node ID is not checked
against the receiving regular node's ID. Only successfully processed
children contribute to the existing inclusive/exclusive formulas.
Exclusive history is appended before regular history; parents consume
the child's newly appended inclusive sample. An explicit work stack
preserves this bottom-up order.

Root totals include only direct children processed for this frame, not
all stored siblings or every descendant. Empty source children append a
zero root sample; absent source roots do not advance. If the stored root
has no children, native code takes its zero-root branch without inspecting
any incoming children (saved as `SkippedRootChildren`).

The result reports updated roots/regular nodes, skipped child occurrences,
and positional identity mismatches. Nonfinite half-floats remain an
**unsupported native CRT boundary**, not zero/success: the exact rejected
source and metric-word index are returned, with earlier completed history
writes retained. Invalid metric counts are explicit too. Missing roots,
exhausted positional sibling lists, and invalid exclusive-child structure
raise managed invariant failures rather than emulating native assertions
or invalid dereferences. This is offline sample state, not rendering,
name resolution, native memory ownership, or exceptional FP emulation.

### Profiler display-name selection and snapshot override

Profiler vtable VA`0x1C78680` slot+10 targets`0xE8CD70`, a previously
undefined144-byte routine now defined from verified original instructions.
No executable bytes changed. `NativeProfilerDisplayNames.Resolve`
saves its raw logical text selection; it does not decode character
encoding, render strings, or manage native shared-text lifetimes.

A nonempty cached shared text at node+8E4 overrides **every** kind/table
branch. Empty or absent cached text falls through. Kind1 looks up the
full WORD ID in the root-name table via`0xE648E0`; kind2 uses the separate
node-name table via`0xE64920`. A missing key returns the exact literal
`<Waiting For Server>`, but a present empty name returns empty text, not
that placeholder. Kind3 returns `[Exclusive]`. Other kinds return
`<Error>` when there is no nonempty override.

The getter does **not** populate the cache or clear it on name-table
updates. Snapshot writer`0xE8CE00` first resolves the getter, copies its
C-string into+8E4 with`0x2FCC90` (`strlen`, byte copy and terminal zero),
then writes base state and cached text. Reader`0xE8CE70` reads base state
and null-terminated shared text into+8E4. Thus snapshots can leave a
nonempty override that takes precedence over later server name updates.
The saved getter API accepts an explicit sampled cache; it does not
invent invalidation or implement the whole snapshot codec. Logical byte
selection remains separate from C-string prefix/termination rules.

### Profiler derived-node subtree snapshots

`NativeProfilerNodeSnapshotCodec` saves the bounded derived-node format
from writers`0xE8CE00`/`0xE8DD00` and readers`0xE8CE70`/`0xE8DDA0`.
This is **not** ServerProfiler_SendFrame61's network body, nor a claim
about whole-file framing or the hidden container's dynamic type.

One derived node writes, in order: LE signed32 child count; every child
body in sibling order; raw expansion byte+10;80 physical history samples;
raw LE DWORD next-slot+8D4; cached name bytes plus terminal NUL.
Each sample writes WORD0, DWORD4/8/0C, WORD10/12, DWORD14/18:
**26 bytes**, omitting the native28-byte structure's padding at+2.
History is physical slot order, not chronological order. A nameless leaf
is2090 bytes; no sample count or half-float conversion is present.

The generic tree base writer`0xE726E0` counts siblings and invokes each
child's virtual writer before expansion; reader`0xE72740` recursively
creates children using virtual+0C and reads the byte after them.
Its count test is signed: negative counts mean no children. Decode keeps
that raw count for inspection; encode re-counts actual children as native
writer does. The next-slot DWORD and nonzero expansion byte are copied
without normalization/validation by native code and remain raw in the DTO.

Kind/ID, parent/sibling pointers, name-map bindings, and cached text's
allocation metadata are **not** in this body. The derived empty-peer
factory`0xE8CEA0` initializes kind0/IDFFFF before reading; the traced body
does not restore those live identities. Snapshot DTOs therefore remain
separate from the live kind1/2/3 tree and are not silently activated.

Encoder input represents **already captured** content: it does not invoke
the live getter or alter a tree's name cache. It writes the first C-string
prefix plus NUL, whereas decode owns name bytes up to the terminator.
The iterative codec leaves trailing framing bytes unconsumed, reports
truncation explicitly, rejects invalid80-slot/cyclic managed input, and
does not execute native refill hooks, constructors, destructors or
partial object mutations. Full file/container/type-dispatch framing is
still unresolved.

### Outgoing profiler-name requests: concrete dual-family endpoint

RTTI proves the concrete endpoint is `tfServerProfiler`, vtable
VA`0x1C61B50`. Slot+8 targets`0x6AAB20`, which builds either
`matrix_fury::ServerProfiler_RequestNames` or
`firefall::ServerProfiler_RequestNames`. `NativeProfilerNameRequests`
saves this method's sampled selection rules; `NativeProfilerNameRequestWire`
saves its normal application buffers. It performs no logging, timestamp
writes, socket submission, or other native execution.

The concrete method first requires manager+0xB80==2 (the verified World
mode). Other modes ignore even an invalid selector. In mode2, selector1
constructs matrix message30; selector0 constructs firefall message18;
other values take the native assertion path. Both first get a selected
observer and require its DWORD+820 to be nonzero. Matrix gate`0x6AADC0`
requires signed DWORD+0C **>3** and optional log mask`0x01`. Firefall
gate`0x6AAD20` instead requires **>4**, uses mask`0x02`, stores
`1000 * uint32(tick)` at observer+0xB0, and copies observer+878/+87C
to descriptor+18/+1C. These copied words are one raw64-bit header prefix;
no type-byte forcing or established character-ID interpretation is added.
The matrix rejection's EAX is not defined; modeled outcomes do not claim
a native Boolean return or delivered request.

Both descriptors copy first/root and second/node WORD vectors into+28
and+34, preserving order and duplicates. Writer`0x349030` emits
chunked-u8 counts and LE WORDs, with a terminal zero for exact multiples
of255. Whole normal buffers are:

- Matrix30 / channel1: compact code30, root-ID list, node-ID list.
- Firefall18 / channel2: raw LE64 routing pair, compact code18, root-ID
  list, node-ID list.

The emitters are`0x6AB080` and`0x6AB190`; send wrappers are`0x6AAE30`
and`0x6AAF50`. No socket envelopes, encryption or live traffic is modeled.
Original class RTTI and constructor codes are verified independently;
the two similar names are not collapsed into one protocol.

Frame consumer`0xE65290` invokes endpoint virtual+8 with owner+1D0 and
its collected root/node vectors. The `tfServerProfiler` method above
matches that slot and argument shape. **The global owner's virtual+60
provider has not yet been proved to return this concrete class**:
the static global VA`0x1DFC82C` points to pointer cell VA`0x1E00470`,
whose runtime installer/provider remains unresolved. Consequently the
new codec and concrete selector are established, but a continuous
frame-consumer-to-provider call path and formal meaning of owner+1D0
are not asserted.

### Message58 pause/unpause update

Handler `0x496680` applies the shared clock parameter schema via `0x4936B0`.
The five QWORDs correspond to EnterZone's initial tick, reference tick,
double-rate bits, upper-limit tick and offset bits. The final byte is
normalized exactly as in EnterZone; descriptor/native padding is not wire
data. The stored copy begins at primary connection+8E0.

`MatrixClockUpdateNativeLayout.cs` saves the distinct selections:
stored upper-limit flag change emits runtime110 `"paused"`; the full incoming
parameter object is then copied. Incoming paused/running-unpaused sets the
running bound at connection+950 and flag+948. Repeated paused updates **do
not overwrite an already-active running bound**. Incoming unpaused with a
running bound rebases running reference+920 to the incoming **initial** tick,
rate bits+928 to the incoming rate, and offset+940 to wrapping
`old bound - incoming initial tick`, then clears+948. Otherwise these running
values stay unchanged. Native assertions require initialized byte+918 and
running offset equal incoming offset after the branch. The model reports
those assertion conditions, rather than silently treating mismatches as
successful initialization; it does not invoke callbacks or emulate copy padding.

## Final server routes63-68: samples, LFG and shared maps

`MatrixServerMessages63To68Wire.cs` saves the final six descriptor bodies:

| Code | Factory / reader raw offsets | Wire body | Minimum packet |
|---|---|---|---|
| 63 | `0x33DFDA` / `0x34B570` | Two raw LE16 at descriptor28/2A | 5 |
| 64 | `0x33E03D` / `0x34B470` | Seven raw LE16 at28..34 | 15 |
| 65 | `0x33E0A0` / `0x34B420` | QWORD28 match ID, DWORD30 LFG category, QWORD38, NUL leader name40, chunked NUL text list44 | 23 |
| 66 | `0x33E103` / `0x34AF70` | QWORD28 match ID, QWORD30, NUL leader name38 | 18 |
| 67 | `0x33E166` / `0x34AE90` | QWORD28 author ID, NUL author/text30/34, DWORD38 flags | 15 |
| 68 | `0x33E1C9` / shared `0x355140` then `0x355E50` | Two mandatory byte-keyed maps of WORD16 and WORD32 | 3 |

Original65/66 logs explicitly name **LFGMatchFound**, **LFGLeaderChange**,
match ID and leader name. Category ID is named by the65 log. The other
QWORD and the list of names/texts are kept positional: sender/field type
identities are not assigned solely from their use. Lists retain duplicate
texts and ordering; count255 requires continuation including terminal0 for
exact multiples. Message68 uses the same unsigned-byte-key ordering and
last-value-wins maps as EnterZone, but **without an optional presence marker**.
Handler `0x4925F0` copies both maps via `0x3563E0` into primary connection+8BC,
setting the destination optional's presence byte true even for two empty maps.

### LFG is separate from challenge state

The existing mode-specific object's LFG presence byte is+48, match ID+50.
These are not the active-challenge pointer+E4 or its ID fields.
Handler65 `0x492400` gets the current mode object, chooses stored+50 when
presence is true, otherwise global pair VA`0x1E68530`, and only dispatches
to `0x6F4A50` when the selected ID is0 or upper56-all-FF. A valid selected
ID takes the `"already in match"` log path; no incoming-ID match is needed.
Handler66 `0x4924D0` selects the same pair but requires **exact two-word
equality with incoming match ID**, then calls `0x6F4C70`.
`MatrixLfgNativeLayout.cs` saves these different gates, taking the inactive
pair explicitly rather than inventing its runtime value.

The match-found consumer requires local client and its+4D20 member, asserts
no existing LFG presence, prepares a30000ms interval record, and initializes
the mode optional through `0x6F5CC0`. It compares descriptor QWORD38 to
mode+8/C, then either processes list entries or delegates leader-name lookup
to `0x6E8BB0`. Leader-change consumer similarly requires the local-client
gate, asserts presence, stores leader text at mode+5C and the second incoming
QWORD at mode+120/124, then chooses `0x6E81E0` versus `0x6E85D0` by equality
against mode+8/C. Full leader/party invitation actions are not modeled.

Earlier heuristic IDA names called `0x6F4A50`/`0x6E8BB0` quick-chat handlers.
The direct65 path and original LFG logs contradict that classification;
their names/comments are corrected to the bounded LFG/leader-name roles.
This does **not** change the separate RTTI-backed Character command144
`PerformQuickChatCommand` model or its proven constructor at `0x532AC0`.

### WORD sample receiver and zone chat67

63/64 handlers `0x491F90`/`0x4920B0` convert unsigned WORDs to float and
call sample recorder `0x481430` on global VA`0x1E0004C`, separately checking
receiver byte4 nonzero and byte5 zero before each call and taking its lock.
`MatrixWordSampleNativeLayout.cs` records exact descriptor order and
receiver destinations:63 ->2DC/2EC;64 ->34C/35C/36C/37C/2FC/30C/33C.
Native tag object VAs are retained, but metric meanings and recorder
aggregation are unresolved. The model does not pretend a snapshot gate
reproduces changing globals or locking between calls.

67 handler `0x492550` requires a mode-specific object and passes author ID,
author/text values (null shared refs substituted by empty strings), constant1,
DWORD38 and constant0 through `0x6E2590`. Its initially-absent temporary optional
selects the helper's generated-label branch: category1 via `0x80CC90` is
original `"ZONE"`, copied and lowercased to `"zone"` before `0x6E3460`.
Following that consumer proves the field names and status masks below.
`MatrixZoneChatPayload` retains raw text bytes and full DWORD; it does not
assign a text encoding.

## Initial bodies34/36 and catalog-wide wire coverage

`MatrixServerMessages34And36Wire.cs` closes the two remaining initial body
gaps. Code34 factory `0x33D4F4`, ctor `0x339950` allocates40h; reader
`0x357180` reads a WORD-length-prefixed **opaque blob** at descriptor+28,
then one raw byte+38. Minimum packet4; the full unsigned WORD length65535
is supported. Blob contents may include NULs and are not interpreted as
text or nested messages.

Handler `0x490AA0` normalizes the byte by `!=0`, then calls manager consumer
`0x30EE20`. This obtains the incoming blob's pointer and length through its
interface and always invokes the **embedded slBlob at manager+C44**
virtual+18 operation, which appends pointer/length bytes.
If normalized flag is nonzero, it allocates an8-byte pointer/length pair at
manager+C3C, copies the **whole accumulated manager buffer**, invokes its
virtual+14 **no-argument clear**, and sets pending byte+C38=1.
Flag0 does not clear an existing pending byte or snapshot pointer.
`MatrixBufferSnapshotNativeLayout.cs` saves selection, offsets and logical
append/snapshot/clear projection. **Snapshot bytes include earlier chunks**,
not necessarily only the newest incoming blob. IDA's inferred memcpy signature drops
the length in pseudocode; original instructions explicitly push the sampled
length before source/destination, so the zero-length-looking decompilation
is not evidence for an empty copy.

The application constructor writes vtable VA`0x1C80CC4` at+C44; RTTI
identifies **slBlob**. Table slots+4/+C/+14/+18 resolve respectively to
`0x12CB960` writable data at offset, `0x12CB9C0` logical length,
`0x12CBA00` clear length, and `0x12CBA10` append. Append resizes to
old length+incoming length with preserve flag1, then copies incoming bytes
to old end. Clear calls the shared resizer`0x12CBCB0` with length0/preserve0.
Resize can retain uniquely owned allocation capacity or detach/release shared
storage; the C# projection models **logical bytes**, not that allocation
policy, copy-on-write/refcounts, native pointer validity or out-of-memory behavior.
The earlier inferred "consume sampled length" call was a decompiler
argument mistake: original instructions push no argument at virtual+14.

Snapshot helper`0x30EDA0` returns null for a null pair, otherwise allocates
an independent8-byte pair and a separate data allocation and copies all
bytes. The returned snapshot is not a borrowed pointer to cached storage;
caller-side freeing of that returned pair was not traced here.
Request helper`0x30EED0`, after its startup-ready wait and native lock,
reuses the cached pair only if key+C40 equals the requested DWORD and
pair+C3C exists; this test does not use pending byte+C38. Cache misses clear
slBlob and explicitly free old cached data and pair, zero key/pointer,
prepare outgoing message32, clear pending, then wait for the message34
completion. On completion it clones the pair and records the key.
It polls in100ms sleeps; shutdown or app mode1 aborts the response wait.
Missing send prerequisites do not produce an invented immediate success.
The startup wait/callback synchronization and
returned-snapshot caller lifetime remain outside the pure projection.

### Outgoing32 RequestSigscan and incoming34 SigscanData

RTTI names request32 `matrix_fury::RequestSigscan` (vtable VA`0x1C62384`)
and response34 `matrix_fury::SigscanData` (vtable VA`0x1C62354`).
These names identify the native traffic; response bytes still remain opaque,
and no actual signature scanning or live requests are performed by the C#.

Encoder`0x31E7B0` emits exactly **six normal application-buffer bytes**:
compact code32, LE32 descriptor+28 cache-key word, raw byte+2C.
It has **no eight-byte entity/context prefix**. Body reader`0x345D20`
mirrors DWORD then byte. Sender`0x31D350` submits that serialized buffer
on application channel1. Outer socket framing remains the separate saved
application-buffer pipeline.
`Client_RequestOrCloneBufferSnapshot` supplies its second argument as the
key and sets byte+2C to1 iff its first argument equals0, otherwise0.
Neither flag's original semantic name nor cache key's original identity
is established; they are not called a nonce, address or scan mode.
`MatrixRequestSigscan32Wire.cs` saves exact raw encode/decode and this
argument mapping with explicit truncated/noncanonical/wrong-code outcomes.

Code36 factory `0x33D5BA`, ctor `0x339C80` allocates30h; the factory directly
reads NUL raw texts+28/+2C through `0x12EE010`. Minimum packet3.
Original handler `0x491400` logs `"Announcement type='%s' data='%s'"`.
Only exact case-sensitive type `"sysmsg"` gets the current mode-specific
object and, if present, calls `0x6E3460` with the data text and fixed label
`"broadcast"`; other types are log-only in this handler. Native null shared
refs substitute empty strings. `MatrixAnnouncementNativeLayout.cs` saves
the byte-exact filter, not text decoding or the full downstream display.

Every one of the35 dispatcher IDs **34..68** now has a saved normal-buffer
body decoder. The cross-catalog check exercises the exact minimum packet
and rejects its one-byte-short prefix for each route, in addition to
individual nonempty/chunk-boundary/malformed-input cases. This is body-layout
coverage, **not** proof of all field meanings, live interoperability,
alternate-header-buffer support, complete native ownership or full consumers.
The [body index](MatrixServerMessageBodies.md) maps each range to its C# API.

## Shared chat display: announcement36 and zone67

Consumer `0x6E3460` posts runtime**335**, then calls `0x6F46F0` after the
runtime callback. Its original property names prove:

| Property | Observed source / condition |
|---|---|
| `"channel"` | Label argument; omitted only when null/empty |
| `"author"` | Author-name argument; always added, empty for native null |
| `"text"` | Message-text argument; always added, empty for native null |
| `"author_id"` | QWORD author argument; omitted only for0, no upper56-ID validity filter |
| `"dev"` | Flags bit0 |
| `"mentor"` | Flags bit1 |
| `"ranger"` | Flags bit2 |
| `"publisher"` | Flags bit3 |
| `"use_encounter_icon"` | Flags bit6 |

The native flag argument is a byte. Message67 reads a DWORD on wire but only
the low-byte masks above contribute to this notification; bits4/5/7 and all
higher DWORD bits are not interpreted as extra role flags by this consumer.
All nonzero author IDs, including upper56-all-FF values, reach `"author_id"`;
do not reuse the challenge/LFG stored-ID validity tests here.

Channel parsing through `0x80CD70` only suppresses three recognized categories:
2 `"COMMUNITY"` requires global VA`0x1E5E42C` object byte+40;
4 `"ZONE_LANG"` requires mode byte+14;
11 `"DEBUG"` requires global VA`0x1E5E4BC` object byte+40.
Other recognized categories and unrecognized labels pass this selection.
Native globals on those filtered paths are dereferenced, not optional values.
The C# gate takes the parser's result and already-sampled enable bytes; it
does not substitute for native pointer validity or reconstruct the imported
text comparator. PE imports now confirm comparator VA`0x1AC48A8` is
MSVCR120.dll `_stricmp`; native locale-dependent comparison is not replaced
by an assumed managed ordinal comparison.

`MatrixChatDisplayNativeLayout.cs` saves the property/flag projection and
gates. Zone67 selects `"zone"` with decoded author/text/ID/flags. Selected
`"sysmsg"` announcement36 selects `"broadcast"`, empty author, omitted ID
and zero flags. An unselected announcement is rejected by the capture API,
not fabricated as a broadcast. These captures do not invoke runtime callbacks,
simulate their reentrancy, or model the post-notification `0x6F46F0` consumer.

## Structured chat consumer switch (raw 0x6E3670)

Evidence: read-only disassembly of
`D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe.original`.
Code addresses below are raw file offsets; `.text` VA = raw + `0x400C00`.
The PE section table translates embedded pointers and literals independently.
The active zero-based IDA database labels this routine
`mid_helper_multicall_6E3670`, but its function ends at raw `0x6E3B12` and its
switch operand is an unadjusted VA. Consequently its decompiler omits most
cases and reports an invalid table. The original table is valid after PE
translation: VA `0xAE5254` maps to raw `0x6E4654`.

The optional-data path of `0x6E2590` supplies the record's `+0x12` byte to this
consumer. At `0x6E36F2` it zero-extends the byte, subtracts one, and rejects an
unsigned result greater than `18` (`0x6E3700/0x6E3703`). Thus selectors
`1..19` reach the table; `0` and `20..255` return false at `0x6E3AF2`.
Fourteen case bodies call distinct or shared payload readers. The other
bodies use already-supplied identity/text/category or owner state; a missing
reader below does not mean the outer message has no payload.

| Selector | Case raw | Payload reader raw | Observed downstream behavior |
| --- | --- | --- | --- |
| 1 | `0x6E3710` | `0x6FDBB0` | Localizes literal `CHAT_DUEL_RESULT`, then calls shared chat display |
| 2 | `0x6E38AE` | `0x6FDC30` | Resolves a row through `0x321EA0`; if present, dispatches numeric `353` |
| 3 | `0x6E3998` | `0x6FDCC0` | Checks current identity, then conditionally calls `0x937A70` |
| 4 | `0x6E3A3C` | `0x6FDD50` | Two owner/identity gates, then dispatches numeric `266` |
| 5 | `0x6E3B12` | `0x6FDDC0` | Category `14` can dispatch `347`; categories `12/13` update matching state and dispatch `262`, conditionally also `224` |
| 6 | `0x6E3D58` | none observed | Categories `12/13` set owner DWORD `+0x1E0`; category `14` sets `+0x1E4` |
| 7 | `0x6E3DAB` | none observed | Category `14`: found entry can dispatch `347` before removal |
| 8 | `0x6E3DAB` | none observed | Same body as `7`, but skips that `347` dispatch before removal |
| 9 | `0x6E3E79` | `0x6FDE30` | Updates owner `+0x188` through `0x6F6230`, then dispatches `261` |
| 10 | `0x6E3EFD` | `0x6FDEA0` | Updates owner `+0x1A0` through `0x6F7C90`, then dispatches `263` |
| 11 | `0x6E3F6C` | `0x6FDF10` | Gated lookup/validation path can call `0x85B850`; failure paths log and return false |
| 12 | `0x6E40D0` | `0x6FDFA0` | Owner `+0x1FC` lookup/compare/copy; selected updates dispatch `268` |
| 13 | `0x6E44A2` | none observed | Gated owner `+0x170` lookup; missing entry dispatches `265` and is inserted |
| 14 | `0x6E4213` | `0x6FE070` | Identity check, decode and gated call `0x95BCD0` |
| 15 | `0x6E42B9` | `0x6FE120` | Identity check, decode and gated call `0x95BF50` |
| 16 | `0x6E4347` | `0x6FE120` | Identity check, decode and call `0x488830` |
| 17 | `0x6E43BB` | `0x6FE190` | Identity check, decode and call `0x96E2F0` |
| 18 | `0x6E4423` | none observed | Gated localization through `0x390B00`, then shared chat display |
| 19 | `0x6E458A` | `0x6FE240` | Decoded flag selects pending-state removal and numeric `345` or `346` |

These are conditional consumer effects, not event names or a guarantee of
dispatch. Case `13`, for example, returns true even when either initial gate
prevents the event. The caller `0x6E2590` does not use this routine's Boolean
return. No managed capture here executes the native callbacks.

Selected literal-backed outputs:

- Case `1` at `0x6E380F` supplies VA `0x1B6E374` (`CHAT_DUEL_RESULT`) to
  localization helper `0x390F20`. The decoded pair is added as two formatting
  arguments. It then uses helper `0x6E9E30` for the channel and calls
  `0x6E3460` at `0x6E3855` with empty author, zero author ID, and zero flags.
  The translated message wording and which participant won are not inferred.
- Case `2` builds `player_name` from the decoded first field, `item_type_id`
  from resolved row `+0x10`, and `quality` from row byte `+0x33`, then calls
  `uiManager+0xAC` with code `353` at `0x6E395C`.
- Case `4` builds `inviter` from the supplied author text and `player` from
  the decoded field, then calls the same slot with `266` at `0x6E3ABE`.
- Case `19` always builds `player_name` from the supplied text and
  `unique_name` through `0x45DD50`. A nonzero decoded byte adds
  `status_type = "PENDING_IN"`, selects owner `+0x34`, and dispatches `345`;
  zero selects owner `+0x28` and `346` without that status property.
  Both call `0x6F5BF0` before dispatch at `0x6E462B`.

`StructuredChatMessageNativeLayout.cs` preserves all 19 case offsets and
payload-reader targets. Startup checks verify the complete PE-translated
table, the 14 reader call sites, selected output calls, and original literals.
This does not reconcile selector `0x76`'s parser-side `slBlob` with the
receiver's record range or establish the semantics of the conflicting
Character RTTI name.

### Structured-chat simple payload field ledger

Same immutable sample: 40,197,848 bytes, SHA-256
`E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235`.
These layouts describe the byte stream passed to the consumer's payload
readers, not the outer Matrix message or the parser-side blob's meaning.
No packet capture or runtime invocation was used. Wire offsets are relative
to the start of that payload; native offsets are relative to the destination
temporary, not the `0x40`-byte chat record.

`L`, `A`, and `B` below count text bytes excluding their NUL terminators.
The shared-text helper `0x12EE010` copies raw bytes through the terminator,
with no length prefix and no character decoding established. Raw multi-byte
integers use x86 little-endian loads. No wire alignment bytes are introduced
by these readers.

| Selector / reader raw | Wire offset and width, in read order | Native destination offsets | Consumed bytes |
| --- | --- | --- | --- |
| 1 / `0x6FDBB0` | `0`: text A + NUL; `A+1`: text B + NUL | `+0`, `+4`: 32-bit shared-text pointers | `A+B+2` |
| 2 / `0x6FDC30` | `0`: text + NUL; `L+1`: byte; `L+2`: DWORD; `L+6`: DWORD | `+0`: pointer; `+4`: zero-extended DWORD; `+8/+0xC`: DWORDs | `L+10` |
| 3 / `0x6FDCC0` | `0`: DWORD; `4`: text + NUL; `L+5`: byte | `+0`: DWORD; `+4`: pointer; `+8`: zero-extended DWORD | `L+6` |
| 4 / `0x6FDD50` | `0`: text + NUL | `+0`: shared-text pointer | `L+1` |
| 11 / `0x6FDF10` | `0`: QWORD; `8`: text A + NUL; `A+9`: text B + NUL | `+0`: QWORD; `+8/+0xC`: shared-text pointers | `A+B+10` |
| 15, 16 / `0x6FE120` | `0`: DWORD | `+0`: DWORD | `4` |
| 19 / `0x6FE240` | `0`: byte | `+0`: byte | `1` |

Important width evidence: raw `0x3849C0` bounds/refills **one** byte at
`0x3849D1`, reads it at `0x3849EB`, decrements the stream's remaining count
by one at `0x384A14`, then zero-extends and stores a DWORD at `0x384A17`
and `0x384A1B`. Its native four-byte output is neither four wire bytes nor a
float. In contrast, `0x356CF0` stores a byte (`0x356D12..0x356D16`).
`0x3370A0` advances four bytes (`0x3370C5..0x3370C7`), and `0x356F80`
copies two DWORDs and advances eight (`0x356FA3..0x356FAD`).

For selector2, the first text becomes `player_name`; native `+8` is the row
lookup input, whose resolved row supplies `item_type_id` and `quality`.
The byte stored at native `+4` and DWORD at `+0xC` are decoded but their
meanings are not established by this consumer. For selector19, the reader
preserves the full byte; the later `cmp byte,0` selects the two branches
for every nonzero value, not just `1`. `CapturePendingNotification` saves
only that already-decoded decision and does not remove entries or emit
notifications. The return value of removal helper `0x6F5BF0` is ignored;
failure to find an entry does not add a dispatch gate.

`StructuredChatMessageNativeLayout.PayloadFields` records the seven unique
simple-reader layouts, primitive call sites, native widths, and wire widths.
Checks cover all 15 field calls, their destination setup, the primitive
width instructions, minimum sizes (`2/10/6/1/10/4/1`), and all 256 pending
flag values. Those minimums are consumption arithmetic, not evidence that
the native wrapper rejects shorter/trailing inputs gracefully. Remaining
readers include nested optional/collection structures and remain outside
this simple ledger.

### Structured-chat optional values (cases5 and10)

The original executable's case5 wrapper calls `0x709750` at raw `0x6FDDF6`.
That reader reaches `0x709650` at `0x7097BE`, passing its destination's
`+0x28` optional member. Case10 wrapper calls `0x703910` at `0x6FDED5`.
Both optional readers first consume one raw presence byte; zero skips the
value reads, while **any nonzero byte** reads a value. Both store the native
value beginning at optional `+8`, with a flag at optional `+0`.

| Reader / case | Payload-relative wire offsets when present | Temporary native offsets | Total consumed, including presence |
| --- | --- | --- | --- |
| `0x709650` / nested case5 optional | `0`: presence; `1`: QWORD; `9`: DWORD; `13/14`: bytes | `+0`: QWORD; `+8`: DWORD; `+0xC/+0xD`: bytes | zero: `1`; nonzero: `15` |
| `0x703910` / case10 | `0`: presence; `1`: DWORD; `5`: QWORD; `13/17/21/25`: four DWORDs | `+0`: DWORD; `+8`: QWORD; `+0x10/+0x14/+0x18/+0x1C`: DWORDs | zero: `1`; nonzero: `29` |

No four-byte alignment gap precedes the case10 wire QWORD: the gap at
temporary native `+4..+7` is a destination-layout feature. Helper
`0x6F7D20`, called at `0x7039CB`, assigns fields individually to optional
`+8/+0x10/+0x14/+0x18/+0x1C/+0x20/+0x24`; it does not copy the temporary's
padding. The final three DWORDs are read through `0x3568E0`; its observed
load/store and four-byte cursor advance establish their width but not their
meaning, so the ledger preserves raw words rather than naming coordinates.

For case5's optional value, the temporary holds 14 initialized/read bytes
but both assignment paths copy **16** bytes. Initialization at
`0x7096B0..0x7096C3` writes a QWORD, DWORD, and WORD; the following reads
replace those same 14 bytes. The two-byte tail is not written on the traced
local path. Instructions at `0x7096F1/0x7096F6` load/store a full 16 bytes,
as do `0x709709/0x70970E` for first assignment. This is a static
initialization/copy-extent observation: it does not establish a visible
runtime effect or imply two additional wire bytes.

On absent input, `0x7039D7..0x7039DC` and
`0x70971C..0x709721` clear an existing presence flag without clearing the
stored value bytes. Present paths normalize the native flag to `1`.
`OptionalValueFields` records the ten primitive read calls relative to each
temporary, and the two consumed-byte-count helpers describe only branch
arithmetic; they neither decode a packet nor synthesize absent values.
Checks cover every presence byte, all field calls/destinations, the wrapper
chain, flag writes, and copy extents against the immutable original sample.

### Structured-chat case5 full read order

Reader `0x709750` processes the following fields in order. Native offsets
are relative to its decoded destination; `ztext` means raw NUL-terminated
bytes. These are storage/width descriptions, not semantic field names.

| Native destination | Wire field | Primitive call raw |
| --- | --- | --- |
| `+0` (DWORD) | one byte, zero-extended | `0x70975F` |
| `+4` | DWORD | `0x70976A` |
| `+8`, `+9` | two bytes | `0x709774`, `0x70977E` |
| `+0xC` | DWORD | `0x709789` |
| `+0x10` | QWORD | `0x709794` |
| `+0x18` | DWORD | `0x70979F` |
| `+0x1C` | ztext | `0x7097A9` |
| `+0x20` | DWORD | `0x7097B4` |
| `+0x28` | presence byte and conditional14-byte value, as above | `0x7097BE` |
| `+0x40`, `+0x41` | two bytes | `0x7097C8`, `0x7097D2` |
| `+0x44` | DWORD | `0x7097DD` |
| `+0x48` | WORD | `0x7097E8` |

With text length `L` excluding NUL, consumption is `L+37` for an absent
optional value or `L+51` when present. In particular, native gaps at
`+0xA/+0xB` or `+0x24..+0x27` are not serialized padding.
`Case5FixedFields` preserves the 13 primitive calls separately from the
optional member and records the last WORD's two-byte wire width.

### Structured-chat count-byte collections (cases9,14,17)

The three readers below reset their destination collection before decoding.
Each group starts with an unsigned count byte and reads exactly that many
entries. Count `255` requires another count group; every smaller count
ends the collection. Zero therefore represents an empty collection, and a
final nonzero count below255 does **not** require a trailing zero. Full
255-entry final groups need an additional count (possibly zero).

| Case / wrapper | Prefix | Collection destination / reader raw | Entry wire layout | Suffix |
| --- | --- | --- | --- | --- |
| 9 / `0x709560` | six bytes, WORD, byte (`9` bytes) | native `+0xC` / `0x7093F0` | QWORD, `8` bytes | none observed |
| 14 / `0x6FE070` | QWORD, DWORD, three four-byte words (`24` bytes) | native `+0x18` / `0x5DE550` | DWORD, WORD, four-byte raw value (`10` bytes) | none observed |
| 17 / `0x6FE190` | DWORD, ztext, three DWORDs | native `+0x14` / `0x6002C0` | DWORD, `4` bytes | byte at native `+0x48` |

Case9's reader appends QWORD-valued linked nodes through `0x709FD0`.
Case14 builds a temporary with DWORD at native `+0`, WORD at `+4`, and
four-byte value at `+8`. Its last value uses `movss` at raw `0x5DE675`,
but its semantic role is unknown. The append helper `0x5DE4C0` advances
the native vector end by **12** (`0x5DE513` and `0x5DE53F`); only **10**
bytes came from wire. Native padding at entry `+6/+7` is not a wire field.
Case17's helper appends four-byte values; its trailing byte is read only
after the complete chunk sequence.

With `N` entries and `G` count groups, the case9 total is `9 + G + 8*N`,
case14 is `24 + G + 10*N`, and case17 is `L + 18 + G + 4*N`.
`G` is at least one, so empty-list minima are `10`, `25`, and `L+19`.
These formulas count transmitted entries, not necessarily unique entries.
They establish no meanings for the opaque words or byte fields.

`StructuredChatCollectionNativeLayout.cs` records the collection offsets,
entry wire/native value sizes, continuation comparison sites, and the 19
surrounding primitive reads. Binary checks cover those calls, the parent
collection calls, per-entry cursor changes, 12-byte native append stride,
and count255 comparisons at raw `0x70950A`, `0x5DE6BE`, and `0x60046B`.
No new wire parser is introduced.

### Structured-chat case12 nested map and duplicate rejection

The case12 wrapper at raw `0x6FDFA0` calls `0x705570` at `0x6FDFD5`.
That reader resets an outer DWORD-keyed map, then reads count-byte groups.
Each transmitted outer entry is:

1. A four-byte little-endian key, loaded at `0x70571B` and consumed by
   `0x705720..0x705745`.
2. A count-byte chunked DWORD vector through `0x7035C0`, called at
   `0x70574E`.
3. A count-byte chunked DWORD **key set** through `0x70A340`, called at
   `0x705758`. No value bytes follow individual keys in this inner set.

All three levels continue only after count255 (outer `0x7058F4`, vector
`0x70376B`, set `0x70A423`). Every shorter count terminates its level.
An empty outer map therefore consumes one byte. One outer entry with two
empty inner collections consumes seven bytes: outer count, DWORD key, zero
vector count, zero set count. In general consumption is the number of outer
count bytes plus the sum of each entry's `4 + vector-count-bytes +
4*vector-entry-count + set-count-bytes + 4*set-entry-count`.

The destination outer node's value begins at node `+0xC`
(`0x705830`). Its vector is assigned through `0x47A770`; the inner key set
at value `+0x20` is assigned through `0x706C20`. The reader selects or inserts
the outer node before these assignments. Do not apply the inner set's
duplicate rejection policy to outer keys: their lookup/assignment path is
different. The assignment-helper follow-up below establishes replacement on
their normal successful copy paths.

The decompiler's `JUMPOUT(0x70A4B1)` is explained by an exception path:

- The inner reader clears its destination once through `0x70A2E0` at
  `0x70A35D`, before the first count. A continued group jumps to
  `0x70A362`, so earlier keys remain in the set across groups.
- Each key is passed to lookup/insert helper `0x38EC20`. The comparison
  at `0x38EC59` checks the full DWORD key. Existing-key return
  `0x38EC92` stores insertion flag0; new-key return `0x38ED21` stores1.
- At `0x70A416`, a zero insertion flag branches to `0x70A43E`, while a new
  key proceeds to the next entry. This rejects repeated inner keys even
  when the repetition is in a later count group of the same inner set.
- The error path allocates a one-byte object, stores an object pointer in a
  stack local, and passes the address of that pointer plus ThrowInfo VA
  `0x1DB8108` to thunk raw `0x159E27A` at `0x70A4AC`. The thunk jumps
  through IAT VA `0x1AC4770`, which the original PE import table identifies
  as MSVCR120.dll `_CxxThrowException`; `0x70A4B1` is an `int3`, not an
  omitted normal decoding continuation.
- The original ThrowInfo's catchable array is VA `0x1DB80FC`, containing
  two types. Its first CatchableType VA `0x1DB80E0` refers to descriptor
  VA `0x1E125C4`, whose literal name is
  `.PAVunpack_exception@serialize@sl@@`. This names a pointer to
  `sl::serialize::unpack_exception`, not a by-value exception object.
  The second descriptor is `.PAX` (`void*`).

This is a statically established error route, not proof of an uncaught
exception, a crash, or a particular caller-visible failure result.
`StructuredChatCollectionNativeLayout` preserves the reader/assignment
layout and exception metadata. Checks verify wire order, all three
continuation branches, insert-result polarity, imported throw target, and
the PE-translated ThrowInfo/type-descriptor chain. No error is replaced by
silent duplicate collapse in the saved model.

The 19 case targets and all payload shapes now have direct traces, including
these nested structures. Open work is semantic: owner state update/compare
helpers, callback names for numeric event codes, the localized text inputs,
and the still-unresolved Character selector0x76 parser/receiver mismatch.

### Case12 stored-map equality and notification gate

The original instructions at raw `0x6E40D0..0x6E4213` establish case12's
owner update route. A failed `0x48C2B0` prerequisite or failed payload reader
returns false. After those gates, the owner collection is at `+0x1FC`
(`0x6E410B`); it is an ordered tree keyed by an unsigned QWORD. Lookup/create
helper `0x6F6420` compares the key at node `+0x10`, then returns its map at
node `+0x18` (`0x6F6475`, `0x6F64A6`).

| Existing owner key | Decoded map condition | Assignment | Event268 |
|---|---|---|---|
| Present | Equal to stored map | No | No |
| Present | Different, including replacement with an empty map | Yes | Yes |
| Absent | Nonempty | Create owner entry and assign | Yes |
| Absent | Empty | Create owner entry and assign empty map | No |

`0x6E4131` calls outer equality helper `0x6FE010`; a true result branches
directly to cleanup at `0x6E41B8`. A changed map goes through `0x6F9840` at
`0x6E414F` before notification. The absent-key branch tests decoded outer
count at `0x6E41CA`; zero takes the separate assignment at `0x6E41EC`, then
returns true without passing the notification code. Thus a successful case
return is not evidence that an event was emitted.

The equality chain is structural, not a bytewise comparison of native
allocations or hash bucket order:

- `0x6FE010` compares outer entry counts, looks up each DWORD key through
  `0x7001E0`, and compares the paired values through `0x6E2060`.
  Outer entry iteration order does not determine equality.
- `0x6E2060` compares vector element counts and then each DWORD in order
  (`0x6E2090..0x6E209C`), so reordered vector elements can change the map.
  Allocator/capacity state and native padding are not compared. The native
  length check masks off the low two bits of byte-length differences;
  element-count equality here assumes the valid DWORD ranges established
  by decoding, not arbitrary malformed native pointers.
- For the key set at value `+0x20`, `0x6FD3E0` compares counts and looks up
  each DWORD key through `0x6F6B00`. Set insertion order is irrelevant;
  membership is what is checked. These are valid decoded unique-key sets,
  given the duplicate rejection established above.

The event stores the same caller-provided QWORD under literal keys
`chatId` (VA `0x1B6E454`) and `entityId` (VA `0x1B6E448`), then dispatches
numeric268 through the application manager's slot `+0xAC`
(`0x6E419C..0x6E41A3`). No named event/subscriber or gameplay meaning is
inferred from those two field names.

Replacement helpers also explain repeated outer payload keys:
`0x7057C3..0x7057DA` selects an existing keyed node when found, then both
new and existing nodes reach the same value assignment. Vector assignment
`0x47A770` copies source elements and sets the destination end; an empty
source resets that end to begin at `0x47A7B0..0x47A7B2`. Set assignment
`0x706C20` clears destination nodes/count at `0x706C65..0x706C8B` before
copying source keys through `0x707540`. Thus, on successful normal copies,
later entries for the same outer key replace both nested collections,
rather than merging them. The owner-map assignment `0x6F9840` similarly
clears the old map via `0x6FB070`, then copies the new entries through
`0x6FF6A0`. Allocation/error behavior is not replaced with managed fallback
values in the saved capture.

One important decompiler limitation: `0x6F6600` initially appears to return
constant0. Its instructions instead initialize an output local at
`0x6F6612`, pass its address in ECX to range-count helper `0x701630`, and
reload it at `0x6F662A`. The helper writes the count through ECX at
`0x701691`. Treating the decompiler's constant as authoritative would
incorrectly eliminate the entire existing-key equality branch.

`StructuredChatMessageNativeLayout.CaptureCase12MapUpdate` captures the
decision only after the owner prerequisite and decode succeed. It does not
parse bytes, perform native lookups/copies, or dispatch an event. Binary
checks anchor the nested equality, output-local count, assignment paths,
two literal field names, and notification suppression branches.

## Post-notification chat file log and folded hash

`0x6F46F0` is a **chat file-log writer**, gated by the current object's
FILE pointer+18. PE imports prove `_time64`, `_localtime64`, `strftime`,
`strncpy`, `sprintf` and `fputs`. It formats current local time with `%X`
for its false detail flag (the runtime335 caller supplies0), otherwise
`%c %Z`. The three prefix templates, including final space, are:
`"%s %s%s"` for no channel/author pointers, `"%s %s: %s%s"` for one,
`"%s %s %s: %s%s"` for both. Pointer presence matters even for an empty
CString. `MatrixChatLogNativeLayout.cs` takes already-formatted timestamp
bytes rather than guessing native locale/time-zone behavior.

The writer hashes a temporary binary suffix **not emitted as log text**:

| Region | Exact bytes |
|---|---|
| Prefix | Formatted timestamp/labels/text, including trailing space |
| Separator | One NUL |
| Author ID | Raw LE64 with low8bits cleared |
| Static footer | 25 bytes from VA`0x1C67C34`: `73616C74696E652C637261636B657273000000000C2AFFFF00` |
| Mode text+10 | First min(native stored byte length,32) bytes via `strncpy`, zero-padded after an embedded NUL |

The static footer begins `"saltine,crackers"` but its full25-byte value
includes zeros/other values; its purpose beyond this hash input is not
established. The mode+10 text is retained positionally, not claimed as an
account/character name. The byte limit may split a multibyte character.

Raw-span hash `0x12F1780` is FNV-1a32 (offset2166136261, prime16777619)
followed by wrapping final mixing: multiply8193, XOR unsigned shift7,
multiply9, XOR unsigned shift17, multiply33. This is **not plain FNV**
or CRC. It processes embedded NULs because an explicit length is supplied.
The same per-byte/final-mix implementation was already saved for SDB name
seeds; `src\Shared\EngineByteHash32.cs` now shares it between Startup and
Serialization without changing SDB string encoding or cipher behavior.

The writer XORs the hash's low/high16bits and overwrites the original
prefix NUL with `"%04X\n"` before `fputs`. The hidden ID/footer/name bytes
are not written. The C# model builds the pre-CRT line bytes and checksum;
it does not open files, emulate text-mode newline translation, format native
time, or handle file lifetime/write errors. The prefix
`"12:34 zone Alice: Hi! "` with author`0x0102030405060708` and mode text
`"Bob"` yields raw hash`0xC62BB938`, suffix`7F13`, and a single LF passed
to the CRT. Prefixes passed directly to the checksum API must be logical
CString bytes without their terminator; invalid embedded NUL is explicit.

## tfChat identity and file lifecycle

RTTI at type descriptor VA`0x1E12EF8` identifies the class with vtable
VA`0x1C67CDC` as **tfChat**. Constructor`0x6E26D0` and destructor`0x6E2C80`
install that table. The prior "current mode-specific object" returned by
`0x6F4540` is this chat component, not the entire app-mode allocation:
Login object+14, World+28E8, LookingGlass+68. It is a getter, not a transition.
Existing positional `Mode*` names are retained for compatibility; chat text+10
still has no proven original field name; its World source is recovered below.
Outgoing command echoes use it as their own author label. LFG fields+48/+50 likewise belong
to tfChat. The full class behavior is not reconstructed.

`MatrixChatFileNativeLayout.cs` saves file-operation **selections without IO**:

| Native path | Proven behavior |
|---|---|
| `0x6F4610` | Compose `"chat.log"` onto the existing cached VIBE base path; base identity/resolution remains outside this model |
| Constructor | Global byte VA`0x1EDB0A4` zero selects `"wt"`; nonzero selects `"at"` |
| Open wrapper`0x12BA030` | Convert UTF8 path/mode to UTF16 with `MultiByteToWideChar(65001,0,...,-1,...)`; `_wfopen`; store returned FILE at+18 |
| Open failure | Log exact `"Failed to open chat log file for writing"`; writer's null-FILE gate prevents a start line |
| Start marker | `"Chat session started"`, detailed timestamp flag1, no author/channel labels; author-ID pointer is global pair VA`0x1E68530`, not an invented zero |
| Attempt flag | Set global byte1 **even after open failure**; it denotes a prior attempt, not success |
| Destructor | Try shutdown line `"Chat session cleanly shut down"`, detail flag0; if FILE exists call `fclose`, then clear+18 |
| Flush helper`0x6F46E0` | `fflush` only for a nonnull FILE; no static callers found in the current IDB |
| OpenChatLog`0x6ACA60` | Absent chat logs `"OpenChatLog: Chat not initialized"`; otherwise flush nonnull FILE, compose path and `ShellExecuteA(0,"open",path,0,0,1)` even if FILE is null |

The modes explicitly select **CRT text mode**: on Windows, the log line's LF
is translated to CRLF during writing. The checksum remains over the pre-CRT
prefix/suffix input. UTF8 conversion above applies to the filename/mode, not
to the emitted `fputs` byte string. The same open wrapper already appears in
the saved Lua bytecode-cache code with `"wb"`; it is not chat-specific.
Native conversion/CRT/shell return handling, filesystem lifetime and actual
viewer launching are not simulated. Shutdown selection covers the FILE
prefix only, not the many other tfChat containers freed by its destructor.

### World setup supplies tfChat's local identity/text

The successful tail of World initializer`0x64B460`, raw`0x64B59C..0x64B5F4`,
loads the owner/context pointer from World+10 and tfChat pointer from
World+28E8. It copies owner DWORDs+848/+84C into chat+8/+C, forcing the
resulting 64-bit value's low byte to **1**. Upper56bits are unchanged;
there is no added zero/invalid-ID filter. It copies the CString at
owner+850, or an empty CString for a null pointer, into chat shared text+10
through `0x2FCC90`. This proves the source of the checksum's positional
text during World setup, rather than inferring it from the hash routine.

`MatrixChatIdentityNativeLayout.cs` projects this successful copy using
raw bytes and an opaque owner pair. The owner+848 pair also appears in the
previously reconstructed authentication request; its formal identifier type
and the original name of owner+850 are not established here. Do not label
these as account GUID/name or character GUID/name on that evidence alone.
Outgoing chat command echoes use chat+10 as their own author label and
chat+8/+C is compared with other identity pairs, so **local chat identity**
is a bounded functional name. Native shared-text allocation/refcounting
remain outside this projection.

Login has a **different source**, not a copy of World's owner fields.
`0x63B250` samples byte+34 of the object at application+A28 while holding
its native lock. Zero returns false before creating chat; nonzero constructs
tfChat at Login+14, calls`0xBC9A20`, forces returned pair's low byte1 and
copies the routine's explicit CString argument into chat+10. No static
callers were found, so the original meaning of that supplied text is not
established. `ProjectSuccessfulLoginInitialization` models only this
successful copy, not locks, allocation failures or callback registration.

Original instructions prove that`0xBC9A20` returns a full **EDX:EAX pair**;
IDA's DWORD-only inferred prototype hides the high return register.
`lock xadd` increments global DWORD VA`0x1DFC588`, using its old unsigned
value. It ORs that value shifted8 into the base pair at VA`0x1E487F8`,
then clears low8bits. The next counter wraps at32bits.
`ProjectGeneratedId` accepts sampled old counter/base, returning projected
pair/next counter without changing shared state or promising uniqueness.
The native base-bit-containment assertion is satisfied by the observed OR
for stable sampled values. This is **not** evidence that Login's ID is a
server-supplied account or character GUID.

LookingGlass initializer`0x65A880` constructs fresh tfChat at+68.
The constructor clears chat+8/+C/+10, so initial identity is zero/empty;
this does not prove those fields remain unchanged by replay callbacks.
No downstream LookingGlass identity producer was followed in this bounded
initialization trace.

### tfLogin update `0x638B89` guard and `{2,0}` queue consumer

Observation (raw offsets, bytes checked against `.original`): at `0x638AB5`
`C7461C00000000 A1A4CAE501 8A4040 84C0 0F84BD000000` clears `[esi+1Ch]`,
loads the pointer cell at VA `0x1E5CAA4`, tests byte `+0x40` of its pointee,
and jumps to `0x638B89` when zero. When nonzero it tests
`[0x1E50C8C+0x40]` and `[0x1E6075C+0x40]` (conditionally
sending message strings via `0x1E69348` vtable `+0x88`), then queues mode 3
through `0x30D590`. `0x638B89` builds `{2,0}` and calls `0x30DEB0` on
`[0x1DFFD98]`. `0x30DEB0` checks `[this+0xC]` and request mode 1 (dedupe vs
current-mode object `[this+0xA28]`), else pushes the pair into the queue at
`this+0xB84` (`0x315060`). `0x30D850` drains it; mode 2 -> `AppMode_World_Ctor`.

Not found: any writer of the pointer cell at VA `0x1E5CAA4`, or a link from
message `0x145`.
Interpretation: `{2,0}` requests World. The pointer cell's semantic role,
writer, and relationship to successful login remain unresolved.

#### Switch selector, jump table and `0x1E5CAA4` writer search

Observation (bytes checked against `.original`): `0x6385C0` is a per-frame
update (`this=ecx->esi`, state `[esi+0x9C4]`), not a message handler. At
`0x6388D8` `8B500C 83FA04 0F87CF010000 FF24951C99A300` loads
`edx=[eax+0xC]` where `eax=[esi+0x1C]`, the first node of the list at
`[esi+0x20]` (`mov ecx,[esi+0x20]; mov eax,[ecx]; mov [esi+0x1C],eax`), then
`cmp edx,4; ja default; jmp [edx*4+0xA3991C]`. Table raw `0x638D1C` (5 DWORDs):
cases 0,2,3,4 -> VA `0xA3983A` (raw `0x638C3A`); case 1 -> VA `0xA395F2` (raw
`0x6389F2`). Values >4 take the `ja` target raw `0x638AB3` (`89 01` store, then
`0x638AB5` `mov [esi+1Ch],0` and the `0x1E5CAA4` test).
Case 1 builds a string from `[node+0x10]` (fallback `0x1C5EA68`), sends message
`0x146` via `[0x1E002B8]` vtable `+0xAC` (`6846010000 8B01 FF90AC000000` at
`0x638A3B`), destroys the node (`vtable[0](1)`), clears `[esi+1Ch]`, and if
`[esi+0xA48]==6` sets it to 5 with a deadline of `GetTickCount+0xBB8`.
Cases 0/2/3/4 share `0x638C3A` (tests `[esi+0x14]`, calls `0xAE3C60`, updates
`[esi+0x9D0]`), no distinguishing code.
The `0x63C1E2` site (message `0x145`) lies in a different function (after the
`ret` at `0x63C0A7`); it switches on its own object's `[+8]-1` (0..3, table VA
`0xA3D1D4`) to pick log strings, and I found no store of a `[node+0xC]` value
or call into `0x6385C0` in it. So `0x145` provably emits no case of this
switch directly; the producer of the `+0x20` list nodes was not located.
`0x1E5CAA4`: the byte pattern `A4CAE501` occurs once in the file (raw
`0x638ABD`), and `.reloc` has exactly one HIGHLOW entry for it (`0xA396BD`).
So no direct `mov [0x1E5CAA4],...`, `lea`, `push`, or data pointer exists. The
initial image holds `05050505`. Neighbouring `0x1E5CAAC..0x1E5CAD0`
belong to a table wrapper whose initializer is at `0xB637F0`; sibling slots
`0x1E5C8A4`/`0x1E5C9A4` pattern likewise have readers only. Interpretation:
the slot is written indirectly (computed base/loop or struct copy), not found.
Whether the case-1 node is an error/notification event is inferred from the
`0x146` send only.

### World scene-factory root and owned renderer interface

The static sample is
`D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe.original`
(40,197,848 bytes; SHA-256
`E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235`).
The following raw code ranges and data bytes were re-read from that file;
the database's flat/raw code addresses are not treated as PE VAs.

**Observations**

- Factory raw `0x9B73F0` allocates `0xA0` bytes and calls constructor
  `0x9B82F0`. The constructor initializes most root child slots; scene
  pointers at `+0/+4/+8` are populated later, and not every root byte is
  initialized. No root vfptr store or root RTTI descriptor was observed. The
  returned pointer is `EDI`, moved to `EAX` at `0x9B82B4`; `RET` is at
  `0x9B82C6`.
- Through indirect calls (global VA `0x1DFE970`, getter slot `+0x3C`, then
  returned object's slot `+0x30`), it looks up the literal scene keys and
  stores returned pointers at root `+0`, `+4`, and `+8`:
  `RenderScene` (raw string `0x179BD28`), `WorldMapScene` (`0x179BE38`),
  `SinvironmentScene` (`0x179BE48`). Each pointer is checked non-null before
  setup continues. The actual virtual targets and resource-provider type are
  unresolved.
- Factory allocations are recorded as `(root field, byte count, conditional)`
  in `SceneFactoryNativeLayout.KnownChildAllocations`; those records preserve
  observed allocation sizes and field destinations, not semantic class names.
  In the `TfWorld_Initialize` call, ECX is zero; the optional allocations at
  root `+0x2C`, `+0x50`, and `+0x88` are therefore on the taken path.
- Root `+0x14` receives a `0x188`-byte child. Constructor `0x9B5440` receives
  root `[0]` and installs vtable VA `0x1C70968`; following the original PE
  section/RTTI records identifies this child's type as
  `.?AVtfPhysicsDrawInterface@@`. Its constructor stores the scene pointer at
  child `+0x44`, clears `+0x48/+0x4C`, copies four floats from
  `xmmword_1DFF438` to `+0x50..+0x5C`, clears byte `+0x60`, and initializes
  bounds at `+0x16C..+0x180` to `FLT_MAX`/`-FLT_MAX`.
- Root `+0x10` gets the result of a helper called with global VA `0x1E001A0`,
  a per-call object, and static descriptor VA `0x1DFC4FC`. The destructor
  `0x9B8490` invokes the handle's vtable slot 0 with argument 1 and clears the
  field. It also destroys/releases child objects in their root slots and
  releases the three scene references; this supports ownership, but does not
  identify the registry's higher-level meaning. The factory also copies the
  pair at root `+0x44` into an 8-byte temporary and passes it to an insertion
  helper on the container at `[root+0x3C]+0x3C`; the registered object's type
  and exact semantics remain unresolved.
- `TfWorld_Initialize` calls the factory at `0x64B500`, stores its result in
  `tfWorld+0x14` at `0x64B508`, and on success publishes root `[0]` to global
  VA `0x1E00060+0xF8`. The other static callsite is `0x9B2D1A`; that caller
  stores the result at its object base.

**Fingerprint checks**

`StartupBinaryEvidence` checks the allocation, constructor call, three
lookup/store instruction sequences, exact NUL-terminated scene keys, return
sequence, `tfWorld+0x14` store, child allocation/constructor and vtable write,
RTTI chain/name, registration-handle sequence, and destructor clearing of
root `+0x14`. The vtable VA is `0x1C70968` (file offset `0x186F568`); its
complete-object-locator VA is `0x1CC7700`, type-descriptor VA `0x1E28A34`,
and RTTI name file offset `0x1A26E3C`. These are checked using PE section
mapping, not by subtracting a universal VA delta. No runtime execution or IDB
write was performed.

### tfPhysicsDrawInterface vtable, constructor and global scene publish

**Observations** (original PE, raw offsets re-read; VA = raw + `0x400C00` in
`.text`)

- Vtable VA `0x1C70968` has 14 slots, all pointing into `.text` (VAs
  `0xDB7530, 0xDB6440, 0xDB6530, 0xDB65C0, 0xDB66A0, 0xDB6960, 0xDB6A60,
  0xDB6EC0, 0xDB7160, 0xDB7330, 0xDB62F0, 0xDB7340, 0xDB7460, 0xDB74F0`; raw
  `0x9B6930, 0x9B5840, 0x9B5930, 0x9B59C0, 0x9B5AA0, 0x9B5D60, 0x9B5E60,
  0x9B62C0, 0x9B6560, 0x9B6730, 0x9B56F0, 0x9B6740, 0x9B6860, 0x9B68F0`).
  The next dword (`0x1CC76EC`) is not a `.text` address.
- Constructor `0x9B5440` first stores base vtable VA `0x1C709B8`, calls
  `0xDBC170` on `this+8`, zeroes `+0x28/+0x2C/+0x30/+0x34/+0x38`, stores the
  scene argument at `+0x44`, then overwrites `[this]` with `0x1C70968`. It
  clears `+0x48/+0x4C`, copies four floats from `0x1DFF438..444` to
  `+0x50..+0x5C`, clears byte `+0x60`, and writes `0x7F7FFFFF` to
  `+0x16C/+0x170/+0x174` and `0xFF7FFFFF` to `+0x178/+0x17C/+0x180`. If
  `+0x44` is null it calls an indirect assertion-like function pointer
  (`[0x1C15EA0]`) with a `0x7F` argument; the callee is unresolved.
- Slot 0 stores the vtable back, pushes 1, calls a helper, and conditionally
  tests the delete flag (`[esp+8] & 1`) before a second call.
- Slot 9 (`0x9B6730`) is a thunk: `jmp [vtable+0x20]`, i.e. slot 8.
- Slot 10 returns false immediately when argument 2 has bit `0x2000` set.
- Slot 13 (`0x9B68F0`) sign-extends a 24-bit argument, logs the string
  `DebugCollision:ReleaseModelObject(model %i)` (VA `0x1C70834`), then calls
  `[global 0x1DFE970 vtable+0x48]` and tail-jumps to returned object's
  slot `+0x34`. Slot 11 uses the same `+0x48` getter with flag `0x200`;
  slot 12 uses another global (`0x1E732C0`, slot `+0x64`).
- Slots 1-8 use the scene pointer/float math (SSE, `+0x30/+0x34` float
  reads from an argument) with float stack arguments.
- Publish at `0x64B528`: `mov ecx,[eax]` (root `[0]`), `mov eax,[0x1E00060]`,
  `mov [eax+0xF8],ecx`. `0x1E00060` holds a pointer; the store is to the
  pointed-to object's `+0xF8`, not to the global's own address plus `0xF8`.
  A null factory result logs string `Unable to create global scene`
  (VA `0x1B635CC`).
- The three scene lookup targets remain indirect through runtime global
  `0x1DFE970`; static bytes do not name their classes.

**Interpretation** (not proven)

- Slots 1-8 are likely debug-draw primitives and 13 a model-release
  callback, from float-heavy bodies and the log string; names unverified.
  Root `[0]` (RenderScene result) is plausibly the "global scene".

### Scene factory named lookup: oRenderSystem getter -> oD3DRenderAPI create-scene slot

**Observations** (original PE, raw offsets; IDA static read-only, bytes re-read):

- Global VA `0x1DFE970` (raw `0x19FCD70`) is **statically initialized** in
  `.data` with vtable VA `0x1C7870C` (raw `0x187730C`); no code store of a
  constructed pointer to it was found (all ~2,880 xrefs are `mov eax,[g]` /
  `mov ecx,offset g` virtual-call pairs). RTTI via COL `0x1CD59A4` ->
  type descriptor `0x1E3103C`: `.?AVoRenderSystem@@`. Its only code writer is
  an exit-time reset stub at raw `0xE93F70`
  (`C70570E9DF0184 87C701 B870E9DF01 C3`) that stores the following
  vtable `0x1C78784` (all slots `0x199DFC8`).
- Factory raw `0x9B7D11`..: `mov eax,[g]; mov ecx,g; call [eax+0x3C]` then
  `push <name>; mov edx,[eax]; call [edx+0x30]`. Slot `+0x3C` of the
  `oRenderSystem` vtable is raw `0xE96380`: `A1 FC05DE01 C3`, i.e. it only
  returns the pointer stored at global VA `0x1DE05FC`.
- `0x1DE05FC` is written at raw `0xED17DC` (`A3FC05DE01`, in the function at
  `0xED1650`, which is itself `oRenderSystem` vtable slot `+0x34`) after
  `new 0x49B8` (`68B8490000E8FC673E00`, raw `0xED17AA`) and the constructor at
  raw `0xF05FE0`, which stores vtable VA `0x1C79ED4` (`C707D49EC701` at
  `+0x3C`). RTTI: type descriptor `0x1E311F0`, `.?AVoD3DRenderAPI@@`. A second
  store at `0xED196B` assigns static `0x1E72DF8` (alternate path, not traced).
- `oD3DRenderAPI` vtable slot `+0x30` is raw `0xF255A0`: allocates `0x18F48`
  bytes and calls `0xF0FCD0(obj, this, name)`. That routine passes the name
  to base ctor `0xE11E40`, stores vtable `0x1C7A334` (`+0x46`), and inserts
  the object into a 32-entry static table at VA `0x1EDACD8` at the first free
  index (`893C85D8ACED01 89877C020000` at raw `0xF104C2`: table entry and
  `this+0x27C`).

**Follow-up observations: scene class, name, ctor fields, `0xED196B`**
(IDA static read-only; bytes checked against `.original`)

- Vtable `0x1C7A334`: COL `0x1CD683C`, RTTI `.?AVoD3DScene@@`, hierarchy
  base `.?AVoScene@@`. Base ctor `0xE11E40` stores `oScene` vtable `0x1C77A94`
  (COL `0x1CD419C`, `.?AVoScene@@`), so `0xF0FCD0` is the `oD3DScene` ctor
  (`C70734A3C701` at `+0x46` overwrites the base vtable).
- Name: `0xF255E1` pushes name then `this`; `0xF0FCFE` forwards it
  (`mov eax,[ebp+0xC]; push eax`) to `0xE11E40`, which calls
  `VIBE_AssignSharedTextFromCString(obj+8, name)` (`E8F2B34DFF` at base `+0x19`)
  and zeroes `+0xC/+0x10`. No compare or hash of the name appears in the
  `0xF0FCD0` body or the base ctor (the only use of the name argument in
  `0xF0FCD0` is the forward at `0xF0FD00`). Readers of `obj+8` were not traced.
- Ctor fields: `+0x27C` registry index (`0xF104C9`); `+0x284`=0 byte;
  `+0x290`=8; `+0x1758` array (6 x `0x188`); `+0x2090` = result of a
  `[[0x1DFE970]+0x40]` object's slot `+0x60` call given
  `"engine/textures/default_cube.dds"` (VA `0x1BEE1E4`; string verified);
  `+0x17C28` = `0x12CE400()-1` if positive, else `lock xadd [obj+0x18F00]`;
  `+0x18F38/+0x18F3C(-1)/+0x18F40`. On a full table the ctor logs
  `"Too many scenes were allocated.  Max is %i\n"` (VA `0x1BEE208`) with `0x20`
  via `[[0x1E60EA4]]` slot `+0x24` and still returns the object (`+0x27C`=32).
- `0xED196B` is `mov [0x1DE05FC], 0x1E72DF8` (`C705FC05DE01F82DE701`) after
  `0xED194C` deletes the old `0x1DE0600` object (`8B01 6A01 FF10` = slot 0
  with arg 1) and reloads `0x1DE05FC`. File-image initial values:
  `0x1DE05FC=0x1E72DF8`, `0x1DE0600=0x1E730E0`, `0x1DE0604=0x1E73230`.
  Stores to `0x1DE05FC` by raw byte pattern: `0xED17DC`, `0xED196B`, and
  `0xED1AC4` (inside a no-function byte region; not decoded).

**Interpretation** (not proven)

- `0xF255A0` is a by-name scene factory returning a new `oD3DScene`; the name
  is only retained as `oScene` text, not used as a registry key. `0xED196B`
  looks like the render API's shutdown path restoring the static placeholder
  after virtual deletion of the previous object; that it is a teardown (vs
  re-init) is inferred from `push 1; call [vtable+0]`, not proven.

**Interpretation** (not proven)

- The "lookup" is plausibly a create-by-name scene factory (allocation +
  construct on each call), not a name->existing-object map. The scenes are
  registered by index, not by name, in the `0x1EDACD8` table; the name is only
  stored by the base constructor (field not traced). No name-keyed registry was
  found within this budget. `oRenderSystem` slot `+0x3C` looks like a
  "get active render API" accessor.

## tfLogin pending-message cell (static, read-only; raw offsets)

**Observation** (bytes checked against `.original`):

- Ctor `0x6365B0` stores stack arg1 at `this+0x20` and arg2 at `this+0x24`
  (`0x63664E`), and zeroes `this+0x1C`. Vtable `0x1C676C0` (base `0x1C67710`).
- The only static ctor caller is the App mode-apply function (`0x30DC1F`),
  which passes `&App+0x9E8` as arg1 (`lea eax,[esi+9E8h]` at `0x30DC18`).
  The App ctor zeroes `App+0x9E8/+0x9EC` (`0x303104`); the mode-apply code
  clears `App+0x9E8` after other mode ctors (`0x30DB8A`).
- Update `0x6385C0` loads `ecx=[this+0x20]`, `eax=[ecx]` and, if it differs
  from `[this+0x1C]`, stores it in `+0x1C` (`0x638897`/`0x6388C5`). It then
  switches on `[node+0xC]` (`<=4`, table VA `0xA3991C`, raw `0x638D1C`):
  entries 0,2,3,4 -> raw `0x638C3A`, entry 1 -> raw `0x6389F2`.
- Case 1 builds a message with format `0x1B61A78` and text `node+0x10`
  (default `0x1C5EA68` when null), calls vtable `+0xAC` on the receiver
  `[0x1E002B8]` with id `0x146`, then virtual-deletes the node
  (`push 1; call [vtable+0]`) and zeroes `this+0x1C`; if `+0xA48==6` it sets
  `+0xA48=5` and `+0xA50=tick+0xBB8`.
- IDA has no function/xrefs for the case-1 block (`0x6389F2`).

**Interpretation / unresolved**

- `+0x20` is a pointer to a single-slot cell holding one polymorphic node
  pointer, not a node list; the "list" reading is not supported. Selector
  case 1 is the only provable name: "send text via notification 0x146".
- **Producer search pass 2 (observation, bytes checked against `.original`):**
  a displacement-`0x9E8` byte scan of `.text` found only these App-class
  sites: ctor zeroing `0x303104` (`C787E8090000 00000000`, `+0x9EC` at
  `0x30310E`), mode-apply clear `0x30DB8A`, reads `push [esi+9E8h]` at
  `0x30DAF7`/`0x30DB6F` (value passed as arg to two other mode ctors
  `0xA5AD90`/`0xA4BA40`), the `lea` at `0x30DC18` (tfLogin ctor), and the
  App dtor funclet `0x16000E9` (`add ecx,9E8h; jmp 0xA37730`). Other
  `+0x9E8` hits (`0x543488`, `0x54B1C6`, `0x636802`, `0x8E1296`,
  `0xB5C457`, `0xB619C7`, `0xBFADA1`, `0xD4032D`, ...) belong to other
  classes (different surrounding fields), not App.
- `0xA37730` (raw `0x636B30`) is a ref-counted release: `ctrl=[ecx+4]`;
  `lock xadd [ctrl+4],-1` -> `call [ctrl_vtbl+0]`; `lock xadd [ctrl+8],-1`
  -> `jmp [ctrl_vtbl+4]`. Interpretation: App+0x9E8/+0x9EC is a
  shared_ptr-like `{ptr, ctrl}` pair, so `[cell]` is the managed pointer.
- tfLogin methods only read `[this+0x20]` (`0x638897`, `0x6388C5`; the
  init path at `0x63694D..0x636970` asserts the cell pointer and `[cell]`
  are non-null/empty, lines `0x117/0x118/0x119`); none store through it.
  Node class/vtable/size, selector values and the triggering event remain
  **unproven**: the producer must reach the cell by an indirect path
  (SIB/computed address or a shared_ptr assignment helper) that the
  displacement scan does not cover. Next step: xrefs to `0xA371B0`'s
  arg3 (`App+0xBE4`) and shared_ptr assign helpers with `App` as target.
- No producer of nodes was found: no writer of `App+0x9E8` besides the
  zeroing sites above, and `Login_HandleLoginResponse` was not traced to a
  node store. Selectors 0/2/3/4 and per-producer values remain unproven.
  The `0x9E8` scans were displacement-byte matches and are not exhaustive.

### Store at raw 0xED1AC4 and name-keyed registry search (static, read-only)

**Observed** (bytes checked against `.original`; disassembled from the file image):

- `0xED1AC4` = `C705FC05DE01F82DE701` = `mov [0x1DE05FC], 0x1E72DF8`, inside the
  function starting at raw `0xED1A10` (`cmp byte [0x1E49B7F],0; push edi; mov
  edi,ecx; je end`; `ret`). That function is `oRenderSystem` vtable slot `+0x38`
  (vtable `0x1C7870C`: slot `+0x34`=`0xED1650`, `+0x38`=`0xED1A10`, `+0x3C`=
  `0xE96380`). No direct call xrefs exist.
- Body: thread check (`[0x1AC43B0]` vs `[0x1E49B24]`, assert-like call); virtual
  call `[[0x1DE05FC]+0xC](0)`; `[[0x1DFE9C8]+4]()`; then for `0x1DE0608`,
  `0x1DE0604`, `0x1DE0600`, `0x1DE05FC` in order: load old pointer, store the
  placeholder (`0x1E61E0C`, `0x1E73230`, `0x1E730E0`, `0x1E72DF8`), and if the old
  pointer was non-null call virtual slot 0 with arg 1. Then `esi=[edi+4]`,
  the `0xED1AC4` store, `[0x1E49B7F]=0`, and if `esi` non-null `call 0x11F3650`,
  `call 0x16C9960`; finally `[edi+4]=0`. Condition: runs only if the byte
  `0x1E49B7F` (initialized flag) is non-zero.
- `0xED196B` is in `0xED1650` (slot `+0x34`, the init that publishes the new API
  at `0xED17DC`), in a block with the same four-object delete/placeholder
  sequence, followed by a log with `"Failed to initialize \"%s\" rendering API\n"`
  (VA `0x1BE934C`). Both stores write the same value `0x1E72DF8`.
- Raw search for the VAs of `RenderScene`/`WorldMapScene`/`SinvironmentScene`
  finds only the factory at `0x9B7D23/0x9B7D42/0x9B7D62`; no other code
  references them.

**Interpreted** (not proven): `0xED1AC4` is the render-system shutdown that
resets the API pointer to the placeholder; this corrects the earlier guess that
`0xED196B` is a shutdown: it is the init-failure rollback. The name-keyed
"lookup" remains a create-by-name call (`0xF255A0`) with index registration;
no name-keyed registry was found.

**Unresolved**: callers of slot `+0x38`/`+0x34` (indirect); what the
`0x1DE0600/04/08` objects are; readers of `oScene+8`; a binary-wide negative
for name-keyed registries (only direct string refs were searched).
### oRenderSystem init/shutdown callers and manager globals (static, read-only)
- **Observed.** A scan of every function referencing `0x1DFE970` (537) for
  `call [reg+0x34]`/`[reg+0x38]` whose register is the vtable loaded from, or
  `ecx` set to, that global finds exactly one real init call: raw `0xE53C72`
  (`mov eax,[0x1DFE970]; mov ecx,0x1DFE970; ... call [eax+0x34]` with six pushes;
  `0xED1650` ends `retn 0x18`). It is inside `0xE53AC0`, whose only direct
  caller is `0xE550D5` (in `0xE54BF0`, after `0xE546F0` returns true). Its next
  step calls getter `+0x3C` and slot `+0x13C` on the result. Other `+0x34/+0x38`
  hits are on objects returned by getter `+0x48`/`+0x3C`/`+0x44` or on
  unrelated objects, not on oRenderSystem.
- **Observed.** No direct `call [reg+0x38]` on oRenderSystem exists in any
  function referencing the global; `0xED1A10` (no xrefs; undefined in the IDB,
  decoded from file bytes) takes no stack args (`ret`). It is not called
  directly by name either, so its caller is **unresolved** (likely via a copy of
  the pointer or a path not tied to the global).
- **Observed.** Init `0xED1650` allocates and publishes (raw offsets of stores):
  `0x49B8` bytes via ctor `0xF05FE0` -> `0x1DE05FC` (vtable `0x1C79ED4`,
  RTTI `oD3DRenderAPI`); `0x190` via `0xF4F7D0` -> `0x1DE0600` (vtable
  `0x1C7A520`, `oD3DTextureManager`); `0x268` via `0xF5F880` -> `0x1DE0604`
  (vtable `0x1C7A618`, `oD3DShaderManager`); `0x9C` via `0xF800C0` ->
  `0x1DE0608` (vtable `0x1C7A874`, `oD3DModelManager`). Shutdown calls the render
  API's slot `+0xC(0)`, `[0x1DFE9C8]` slot `+4`, deletes (slot 0, arg 1) the
  model, shader, texture managers then the render API, restoring placeholders
  `0x1E61E0C`, `0x1E73230`, `0x1E730E0`, `0x1E72DF8` and clearing `0x1E49B7F`.
- **Interpreted.** `0xE53AC0` is an application/window start routine passing
  window-size/fullscreen-like fields (`edi+0x138/0x13C/0x140`, `edi+0x130==1`).
- **Unresolved.** Shutdown's caller, the meaning of the six init arguments, and
  the `0x1DFE9C8` object type.

### Render init arguments, oEngineSystem, and the oShaderBuilderSystem global

- **Observed.** `0xE53AC0` (non-virtual, `this`=edi) pushes the six `0xED1650`
  args at `0xE53C42..0xE53C6F`: arg1 = `(this+0x130 == 1)`, arg2 = `this+0x138`,
  arg3 = `this+0x13C`, arg4 = `this+0x140`, arg5/arg6 = window client
  width/height from `GetClientRect` (clamped >= 1). `0xED1650` forwards all six
  to oD3DRenderAPI vtable (`0x1C79ED4`) slot +8 = `0xF08680`, which stores them
  at api `+0x4928` (byte), `+0x492C`, `+0x4930` (clamped >= 1), `+0x4934` (raw),
  `+0x493C`, `+0x4940` (clamped >= 1). `this+0x130` is a window mode (1 and 3
  are fullscreen-style: popup style, mode 3 sizes to `GetSystemMetrics(0/1)`;
  mode 1 is promoted to 3 when `this+0xC0 > 1`). After init the code reads the
  display values back via api slot +0xA4 and compares them with cvars whose
  descriptor names are `rd.fullscreenWidth`, `rd.fullscreenHeight`,
  `rd.fullscreenRefresh`, `rd.windowWidth`, `rd.windowHeight`, `rd.videoMode`
  (descriptors at `0x1E6A830..0x1E6AA38`, constructed near `0xE53CF3..0xE53DC5`).
- **Observed (class).** `0xE54BF0` is vtable slot 0 of `0x1C77C2C`; its COL names
  `.?AVoEngineSystem@@` (bases: `oEngineSystem`, `oEngineInterface`). `0xE53AC0`
  is a non-virtual helper (sole caller `0xE550D5` in `0xE54BF0`, after
  `0xE546F0`); neighbouring slots 20/21 are `0xE53A40`/`0xE53FA0`. `0xE54BF0` logs
  "Initializing Main Window", "Operating System", memory and
  "--- Initialization ---"/"--- Finished Initialization (%.2f seconds) ---".
  It has no direct callers (virtual); the vtable is stored by a ctor at `0xE4DFE0`.
- **Observed (`0x1DFE9C8`).** Static by-value global whose initial vtable
  `0x1C79E00` has COL -> `.?AVoShaderBuilderSystem@@`; `0xED1650` calls its
  slot 0 with the render API pointer, `0xED1A10` calls slot +4.
- **Interpreted.** arg1 = fullscreen flag; arg2/arg3 = back-buffer/display
  width/height (`rd.fullscreenWidth/Height` or window values depending on mode);
  arg4 = `rd.fullscreenRefresh`-style third display value (stored unclamped, so
  not a dimension); arg5/arg6 = window client size. Pairing of each field with a
  specific cvar is inferred from comparison order, not proven.
- **Unresolved.** Exact cvar-to-field mapping; caller of ctor `0xE4DFE0` and the
  path up to the app main loop; whether `oEngineSystem` slot 0 is `Init`.

### oEngineSystem construction and vtable (static pass)

- **Observed (addressing).** The IDB maps code at raw offsets; the doc's "ctor `0xE4DFE0`" is
  imprecise: the constructor function starts at raw `0xE4DFA0` (size `0x15E`) and stores the
  vtable VA `0x1C77C2C` at raw `0xE4DFDE` (`C7 07 2C 7C C7 01`). It zero/initialises
  fields through `+0x168` (`+0x164 = 0x8F`, `+0x134 = 1`, `+0x158/0x15C = -1`).
- **Observed (constructor caller).** Sole direct caller is `0xE4E660` (call at `0xE4E6E3`):
  allocates `0x170` bytes via `0x12B7FB0`, runs the ctor, stores the object at `factory+0x10`,
  copies old list head `dword_1E4B0A8` to `factory+0xC`, makes `factory` the new head, sets
  `factory+4 = 1`. It has no direct callers: it is slot 1 of a factory vtable
  (VA `0x1C77C1C`, raw `0x187681C` = `{0x124F410, 0x124F260(=0xE4E660), 0x124F320}`).
- **Observed (factory).** The factory is a static `.data` object at VA `0x1E00460` (raw
  `0x19FE860`, holds `0x1C77C1C`). Its static initializer at raw `0xE4F630` links it into
  list head `dword_1E4B0A4` (`+8` = next) and sets vptr/flag. No `0x1E00470`
  (instance slot) code reference exists except accessor `0xE4E630` (`mov eax,1E00470; ret`),
  which has no direct call/jump xref.
- **Observed (who runs the factories).** `0x12C3A80` (called from `ClientApplicationBootstrap`
  `0x2EAE70` at `0x2EAF3B`) walks `dword_1E4B0A4` and calls vtable slot 1 (`+4`) of every
  factory whose `+4` byte is 0, i.e. it creates `oEngineSystem` (and its siblings).
  Upward chain: `0x2E9630` (wrapper) <- `0x136AEDA` in `CrtInitializeAndInvokeClient`
  (`0x136ADBA`, the CRT start / WinMain-equivalent). `ClientApplicationBootstrap` then
  runs `ClientApplicationInitialize(dword_1DFFD98)` (`0x3063D0`), `ClientApplicationRunLoop`
  (`0x3098C0`), `ClientApplicationShutdown` (`0x3095D0`); `dword_1DFFD98` is the App object.
- **Observed (vtable).** Vtable raw `0x187682C`, COL `0x1CD439C`, 26 slots (raw targets):
  0 `E54BF0` (1412 B), 1 `E553F0`, 2 `E55580`, 3 `E4E1E0`, 4 `E4E1F0`, 5 `E4E200`,
  6 `E55370`, 7 `E55380` (104), 8 `E55990` (746), 9 `E55E80` (601), 10 `E4E470` (34),
  11 `E4E4A0` (15), 12 `E55E60` (25), 13 `E6EF70`, 14 `E6F0C0` (158), 15 `E4FB80`,
  16 `E4FB60` (28), 17 `E4FB50`, 18 `E4FB40`, 19 `E4FB30` (16), 20 `E53A40` (120),
  21 `E53FA0` (1717), 22 `E4FB20`, 23 `E54660` (52), 24 `E4FB10`, 25 `E4FB00` (16).
  Slots without a size are not IDA-defined function starts. No slot name is proven by a string
  beyond slot 0's log text.
- **Interpreted.** Engine-system objects are created lazily by the bootstrap factory walk, not by
  a direct call; AppMode objects (Login/World) are not on this path - they come from
  `dword_1DFFD98` (App) during `ClientApplicationInitialize`/run loop.
- **Unresolved.** The caller that invokes slot 0 (`E54BF0`) and the others (no direct call or
  vtable-literal reference found; likely through `factory+0x10` / `0x1E4B0A8` list consumers
  such as `0x110D060`, `0x2F1540`, `0x311090`; not traced); role names of slots 1-25.

### Factory list consumers and engine-system init (static pass 2)

- **Observed.** `0x12C3A80` (create walk) iterates `dword_1E4B0A4` via `+8` and calls factory vtable
  `+4` (create) on entries whose `+4` byte is 0; `byte_1E4B095` = "walking", `byte_1E4B096` =
  "walked". `0x12C3AF0` (teardown walk, raw bytes `85C9740F8B01FF5008...` at `+0x34`) loops on
  `dword_1E4B0A8` and calls factory vtable `+8` (destroy) until the head is empty. Create slots
  (e.g. `0xE4E660`, `0x2EC510`) link the factory into the instance list (`+0xC` = old `A8` head,
  `A8` = factory, `+4 = 1`, object at `+0x10`); destroy slots (e.g. `0x2F1540`) pop the `A8` head.
  `dword_1E4B0A4` has exactly one code reference (the create walker); `dword_1E4B0A8` is referenced
  by 80 functions, each with exactly two references (create/destroy slots) plus the teardown walker.
  No code reads factory `+0x10` by a static address: raw scans find the immediates `0x1E00460`/
  `0x1E00470` only in the static init (`0xE4F630`) and accessor `0xE4E630` (no callers, no jump).
- **Observed (class list).** 43 create slots with a resolvable constructor store these vtables
  (RTTI): `slUsesSingleton<nl::Network>`, `slUsesSingleton<slSafeQueue::NodePool>`, `nl::SocketPumpI`,
  `smSoundThread`, `ilHiddenInputWindowThread`, `fsAssetDB`, `tsLodManager`, `tfGUIInterface`,
  a `tfAudioManager` bound functor, `tfDebugEvent`, `tfDebugLag`, `oParticleInterface`,
  `EnglishRules`, `tfGlobalizationManager`, `smSoundActionManager`, `vcs::StaticDBDataStore`,
  `tfToggleFireModeCommand`, `uiManager`, `oEngineSystem` (rest have no ctor-stored RTTI vtable).
- **Interpreted.** The list is a generic static singleton/service registry (network, sound, asset DB,
  GUI, UI, debug, rules, engine), not an engine-system-only list. Instance order is creation order in
  `A4` order (A4 head = last registered static init); teardown is the reverse (LIFO via `A8`). The
  registration order across translation units (CRT initialiser table) was not recovered.
- **Observed (negative).** No loop over either list calls a slot on `factory+0x10`; the only list
  consumers are the create and teardown walkers. Hence `oEngineSystem` slot 0 (`0xE54BF0`) is NOT
  invoked by a factory-list init loop.
- **Unresolved.** The real caller of `0xE54BF0` (likely a virtual call through an interface pointer
  obtained elsewhere, e.g. a ctor-time global store or `oEngineInterface`); `Init` naming remains
  inferred from its log strings; init order of the other classes; classes without RTTI-resolved ctors.

### Engine-system slot callers and crash-hook pass (static, FirefallClient.exe.original)

Observed:
- `dword_1DFC82C` (raw 0x19FAC2C, bytes `70 04 E0 01`) holds 0x1E00470 = `factory+0x10`, the oEngineSystem instance slot. Accessor 0xE4E630 is `mov eax,1E00470; ret`. 0x2D2D60/0x2D2D70 copy it into aliases `1E60E3C` and `1E60EA4` (478 raw immediate hits).
- Consumers load `eax=[cell]; ecx=[eax]; eax=[ecx]; call [eax+disp]`. Slot calls found through `1DFC82C`: +0x44 (0xF5FF6B), +0x54 (0x46CC7A, 0x470F1B), +0x58 (0x46CA6B), +0x5C (0x3D32B6), +0x60 (0xE651D6, 0xE65233, 0xE65339, 0xE66D5C), +0x64 (0x3072B2 with arg 1 after init, 0x30979A with arg 0 in shutdown). Through `1E60EA4` a chain-tracking scan also found +4 (0x30C168), +0x1C, +0x24, +0x2C, +0x30, +0x38, +0x3C, +0x40, +0x44, +0x48, +0x4C, +0x54, +0x58.
- Direct fields through the same object: +0x160 (byte), +0x164, +0x168, +0x24 (float), +0x78/+0x80 sub-objects; these are fields, not slots.
- Slot 0 (0xE54BF0) appears only as the vtable literal at raw 0x187682C. No call with disp 0 through either cell was found by the chain scan (cells `1DFC82C` and `1E60EA4` only); the F54D82-area disp-0 hits are loads, not slot calls.

Interpreted: slot +0x64 is a boolean-style toggle (1 at init, 0 at shutdown). +0x60 returns an object pointer that callers null-test. Roles of other slots are unproven.

Unresolved: the caller of slot 0 (indirect calls through other copies of the instance pointer, or through registers not traced, were not searched); the writer that fills `1DFC82C`.

### oEngineSystem vtable slot table (static, FirefallClient.exe.original)

Slot index = disp/4; raw targets as in the vtable at raw `0x187682C`. Consumer pattern `eax=[1DFC82C]; ecx=[eax]; eax=[ecx]; call [eax+disp]` passes the object as `this`.

| Slot | Disp | Raw target | Role | Evidence |
|---|---|---|---|---|
| 17 | +0x44 | E4FB50 | `lea eax,[this+0xB8]; ret` - returns address of an embedded block (caller 0xF5FF6B reads `+0x1C` of it as a text/path part) | Observed bytes; block meaning unresolved |
| 21 | +0x54 | E53FA0 (1717 B) | ApplyDisplayMode(mode, width, height, refresh): writes `this[76]` (+0x130) = mode, caches size in `this[78..82]`, drives window-style/resize through the render-system (`dword_1DFE970`) | Observed decompile; name Interpreted. Caller 0x46CC7A pushes 4 args (0,edx,ebx,0) |
| 22 | +0x58 | E4FB20 | GetDisplayMode: `mov eax,[this+0x130]` (the field slot 21 writes) | Observed; caller 0x46CA6B stores the result to `[esi+0x7C]` |
| 23 | +0x5C | E54660 | Out-param getter of three dwords `+0x14C/+0x150/+0x154` (each pointer null-checked, `ret 0xC`) | Observed; semantics of the triple unresolved; caller 0x3D32B6 |
| 24 | +0x60 | E4FB10 | GetHandle: `mov eax,[this+0xB4]` (callers null-test; 0xE651D6 feeds it to an import thunk) | Observed bytes; "window handle" is Interpreted only |
| 25 | +0x64 | E4FB00 | SetHandle: `[this+0xB4]=arg; ret 4` | Observed. **Correction:** init call 0x3072AE pushes global `dword_1E00128` (not literal 1); shutdown call 0x30979A pushes 0 |

Other slots, bytes only (Observed, no consumer found, role unproven): 3 `E4E1E0` = `mov al,[this+0xBC]`; 4 `E4E1F0` = `mov al,[this+0xBD]`; 5 `E4E200` = `mov al,[this+0xDB]`; 6 `E55370` = returns constant 0x1E81D00; 12 `E55E60` = calls `[this+0x10C]` callback if set; 15 `E4FB80` = `mov eax,[this+0xAC]`; 16 `E4FB60` = bounds-checked (`idx<2`) getter of `[this+0x158+idx*4]`; 18 `E4FB40` getter `[this+0xB0]`; 19 `E4FB30` setter `[this+0xB0]`; 0,1,2,7-11,13,14,20 not characterised here (slot 0 = Init by log strings, earlier pass).

Slot 0 callers (scoped negative): I scanned `ClientApplicationBootstrap` 0x2EAE70, `Initialize` 0x3063D0, `RunLoop` 0x3098C0, `Shutdown` 0x3095D0 and wrapper 0x2E9630 for `call [reg]` (disp 0) and references to cells `1DFC82C`/`1E60EA4`/`1E00470`. No slot-0 call through the engine-system cell exists in them. The only disp-0 call is 0x30976B (deleting-dtor style, `push 1; call [eax]`) on the App member `[esi+0xB98]`, not the engine object. `Initialize` has a direct field write `[[1DFC82C]]+0x164` at 0x3071B9 (field, not slot). Callers elsewhere (other copies of the pointer, other functions) were not searched: slot 0 caller remains Unresolved.

Checks: `StartupBinaryEvidence` verifies slots 17/21-25 vtable entries, accessor bytes, and the +0x64 init/shutdown call-site bytes. Constants are in `SceneFactoryNativeLayout` (`EngineSystemSlot*`).
