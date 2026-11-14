# Matrix connection: handshake and receive dispatch

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
