# Matrix server message bodies and state transitions

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
