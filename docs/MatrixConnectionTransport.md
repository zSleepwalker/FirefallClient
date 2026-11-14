# Matrix connection: transport and oracle ticket

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
draining; the outer loop ends when thread byte `+4` is set. The November 4
active-sample trace bounds each inner pass to one 500-ms wait and 100
dequeue attempts before another stop read. The join hook is a bare return,
not a stop post; quiet-stop observation requires no external completion,
but callbacks/maintenance can still delay progress. Successful
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
IOCP handle at `+0xDC` was not established by that destructor trace.
The later active-sample final-release trace resolves a separate path:
`0xBBD570` destroys HTTP manager first, pumps its independently owned
socket vector for the supplied timeout, joins the worker with INFINITE
timeout, deletes residual socket entries, closes IOCP at `0xBBD6CA`,
then calls WSACleanup at `0xBBD6DA`. It runs only for the last network
reference; nonfinal release merely decrements the owner-thread-affine count.

For concrete ManagedUDP, vtable VA `0x1C73D78` slot `+0x1C`
resolves to `0xBED590`, which queues sentinel 1; `0xBEDAA0` consumes
it to finish state 4 to 5 and permit vector-owned deletion. The close key
is a borrowed socket pointer, not an added socket reference. Network close's
post BOOL is ignored by `0xBED0F0`; no local failed-post retry/rollback
was found, and close-request byte `+0x81` suppresses another attempt.
Inspected maintenance either skips work for argument one or processes only
states 1/2 for argument zero; empty-queue state 4 remains retained. Sentinel
2 calls the already-suppressed close helper, while sentinel 1 completes
4 to 5. Other sentinel producers remain under investigation. Exact state,
ownership, and shutdown callsites are in `Handoff.md`.

ManagedUDP's receive path is distinct from GameSocket's. It owns a persistent
`0xA0`-byte operation at socket `+0x98` and receive buffer `+0x50`
(capacity `+0x4C`, default `0x4000`). Only successful result retrieval
reposts at `0xBED8F5`; nonempty results first enqueue a separate packet.
At the IOCP layer, error 995 skips callbacks, while other nonnull-operation
failures reach slot `+0x20`. Inside ManagedUDP result retrieval, error 996
instead suppresses callback/repost and 995/10038 become zero. Error slot
`0xBED5F0` always queues close token 2; zero suppresses logging only.
Persistent receive storage is freed by destruction, not those error paths.
Queued `0x38`-byte packets contain independent blob byte snapshots before
receive-buffer reuse. States 1/2 synchronously lend packet blob/endpoint and
timestamp through owner slot `+4`; the SimpleSocketReplayReader interface
target is `0xBE1450`, forwarding matching endpoints to `0xBE0110`.
Packet/blob ownership is released immediately afterward or without delivery
in closing states. Downstream retention is not implied by that borrowed
pointer. Owner presence is assertion-checked, not a null-safe delivery guard.
The discriminator-1 branch inside `0xBE0110` appends remaining bytes into
reader-owned blob storage with an explicit copy, not retained packet backing.
That branch's lifetime is independent of the immediate packet release;
other discriminators remain separate scope.
Outer framing at `0xBE0110` recognizes one-byte discriminators 0 and 1.
Case 0 copies 24 bytes, then reads an LE16 length and supplies that many
bytes to its embedded destination; it consumes `27+L` bytes and ignores
trailing input here. Case 1 appends remaining input. Unknown discriminators
log after consuming one byte. Shortages throw through the borrowed cursor
handler; case 0's fixed-field write precedes its later length checks and
is not transactional. Zero length leaves the embedded destination unchanged.
The embedded destination at reader `+0xC8` is slBlob initialized by
`0xBDFBC0`; its slot `+0x10` replaces bytes, not appends. Request entry
`0xBDFD80` arms propagation flag `+0xD8`, cleared by successful case 0.
The fixed record remains opaque. The concrete forwarded slot `+8` target
`0xBE1600` is diagnostic-only and receives no request arguments/bytes;
the arming entry is not established as a wire request operation. Owner
record/ready-byte consumers remain follow-on scope.
Endpoint matching at `0xBE1450` uses common family bits rather than full
flag equality, preferring IPv4 over IPv6 when both are common; address bytes
and port are compared directly without swapping there. No common family
succeeds only for zero packet flags, still subject to the port comparison.
Mismatch logs without forwarding to `0xBE0110`; initialization of the fixed
configured endpoint remains separate from the known endpoint-list insertion.
The fixed fields are ManagedUDP's remote peer, copied by `0xBECF50`
after successful connect. Bind with port zero instead obtains its assigned
local port through getsockname and stores it at state `+0x58`, not peer
port `+0x78`. Constructor zero inputs therefore do not establish persistent
zero-peer matching. Exact setup calls and family-mode mapping are in
`Handoff.md`.

The concrete SimpleSocketReplayReader constructor returns allocation even
when connect fails; its peer-based readiness slot is separate. Its wrapping
`nl::SocketReplayReader` exposes inverted readiness at slot `+0x14`,
raw `0xBE00E0`. Worker `0xBD62F0` consumes it with another guard byte
to update status/timing fields `+0x54/+0x55`, not a demonstrated error
return. Constructor-pointer checks alone do not prove connection success.
One status consumer sets app `+0xB78` under additional worker/count
conditions. The run loop clears that byte and zeroes one iteration's timing
scale; it is not the loop-exit flag. Worker EOF/error meanings remain
unestablished. The timing callback is
the app `+0xB98` object's primary-vtable `+8` target: vtable VA
`0x1C6781C`, entry VA `0xA5CDF0` (raw `0x65C1F0`).
App dispatch raw `0x30A009` invokes it when `+0xB98` is nonnull
and app `+0xB94` is clear. The later handoff closes its invocation chain:
PE startup -> wrapper raw `0x2E9630` -> bootstrap `0x2EAE70` ->
runloop `0x3098C0` -> renderer service vtable index 8 (byte offset
`+0x20`, raw `0xE55990`) -> retained callback raw `0x302A30`.
The callback runs synchronously on that startup thread. Renderer service
byte `+0x12C` is a distinct shutdown-request latch; app `+0xB78`
only clears the iteration-local multiplier, not persistent app `+0xB70`.

The requested helpers `0xBC3970` and `0xBC3990` have no direct `E8`/`E9`
caller or `.rdata` VA/vtable pointer in the original-file scans. Their code calls the imported
`WinHttpCloseHandle` at VA `0x1AC4CF8`, then clears `this+0x20` and `this`,
respectively; no evidence ties those WinHTTP handles to Network or GameSocket.
The standalone drain `0xBBDA20` likewise has no direct `E8`/`E9` caller or
actual VA pointer/vtable slot found. Its raw value occurs at unaligned `.rdata`
offset `0x183CCFD` among `+0x20`-stride values, and its RVA occurs at
`0x183CE7D` in another such sequence; neither sequence is established as a
vtable, so its owner remains unresolved.
