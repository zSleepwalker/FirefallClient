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

## Secondary command dispatcher and eight verified class cases

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
| `0x13..0x19` | 2 | `0x81EE63` | Case block not mapped |
| `0x1A..0x1F` | 3 | `0x81EF6D` | Case block not mapped |
| `0x22..0x25` | 4 | `0x81F206` | Case block not mapped |
| `0x26..0x28` | 5 | `0x81F2EF` | Case block not mapped |
| `0x34..0x35` | 6 | `0x81F34A` | Case block not mapped |

Leading byte0 or `0xF0..0xFF` uses a second table at VA `0xC22630`
(raw `0x821A30`) when its selector is `0x20..0x7A`; values outside that
selector range return zero. Leading bytes `0x37..0xEF` also reach the
zero-return path. These are dispatch boundaries only; the inputs’ semantic
names and the 91 secondary targets are unresolved.

Eight nested selectors are mapped: route0 block `0x81CCAD` dispatches selector
`0x53` through the table at VA `0xC22444` (raw `0x821844`) to raw case
`0x81CCC3`. That case allocates `0x48` bytes, calls constructor `0x524C30`,
then body reader `0x5F6B10`. Constructor RTTI identifies the object as
`firefall::Character::MarketRequestComplete`. The reader stores, in order,
a byte at object+`0x28`, raw QWORD at+`0x30`, byte at+`0x38`, and three
NUL-terminated shared-text values at+`0x3C/+0x40/+0x44`. Field meanings
remain unknown; these eight selectors do not name the remaining table cases.
`MatrixSecondaryCommandDispatcherNativeLayout.cs` and
`CharacterMarketRequestCompleteNativeLayout` preserve the route bounds and
this bounded object layout without implementing a parser.

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

One data-flow discrepancy remains in the nonzero `+0x60` path. The QWORD
reader writes eight bytes to a 12-byte local prefix at `0x5F6059`, while
`0x5F5EB0` copies three DWORDs from that local prefix to object
`+0x68/+0x6C/+0x70` at `0x5F5EE9/0x5F5EEF/0x5F5EF5`. The disassembled caller
sequence `0x5F6032..0x5F6070` initializes the separate vector temporary and
passes the QWORD temporary first to `0x356F80`, then to `0x5F5EB0`; it shows
no write to the QWORD temporary's third DWORD. The `0x356F80` fingerprint
confirms two DWORD stores and an eight-byte cursor advance, and the
`0x5F5EB0` fingerprint confirms the three destination offsets. Accordingly,
object `+0x70` is recorded as unresolved rather than assigned a wire-field
meaning. Refill and short-input behavior of the scalar helpers is also not
established. `CharacterTookDebugWeaponHitPublicNativeLayout` preserves the
observed offsets, widths, branch boundaries, and record size without
implementing a parser or naming semantic fields.

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

### Selector 0x57: `DebugWeaponStats`

Entry4 maps selector `0x57` to VA `0xC1D9E7` / raw case `0x81CDE7`. It
allocates `0x38` bytes, stores local DWORD `0x5F` (role unknown), then calls
constructor `0x5250A0` and body reader `0x5F4A10`. The constructor passes
`0x57` to `0x5A05C0` and installs vtable VA `0x1C67238` (RTTI
`firefall::Character::DebugWeaponStats`). The reader consumes eight WORDs
in order into object offsets `+0x28,+0x2A,+0x2C,+0x2E,+0x30,+0x32,+0x34,+0x36`.
These occupy the final 16 bytes of the allocation. Their meanings remain
unknown; the RTTI name does not establish their semantics.

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
