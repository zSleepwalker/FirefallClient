# Matrix server dispatch and parsed receivers

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

The detailed dispatch tables and selector-reader analyses are maintained in [MatrixSecondarySelectors.md](MatrixSecondarySelectors.md).

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
manager-side delivery structure at `+0x5EC`. The November 4 active-sample
trace resolves later consumption: manager `0xC02730` snapshots/clears scheduled
listeners and calls `0xC38CA0` at `0xC0357B`. Drain reads the borrowed
registration record and invokes `0xC3B3C0`, then releases queued payload
ownership and removes the entry. Invocation enters the protected VM path;
the concrete runtime native target at `0xCD728C` remains unresolved.
This is deferred delivery, not inline execution by `0xC38400`.

The registration record is `{+0 event text, +4 event ID, +8 callback text}`
(`0xC53E50`). Queue entries retain payload ownership, not a callback-text
snapshot; replacement updates record `+8` in place, so queued deliveries
read replacement text if it changes before draining. Script
`Component.UnbindEvent` reaches immediate record removal through
`0xC4DC80 -> 0xC32680 -> 0xC35D10`. That inspected removal path does
not invalidate pending queue entries; concrete script/queue reachability
and runtime consequences remain unresolved, not established vulnerabilities.

Null payload selects the zero pair at PE VA `0x1E68660`, in the virtual-only
tail of `.data`, not bytes at that raw offset. Conversion makes a fresh VM
table per callback and inserts `"event"` with registration event text
(`0xC25E89 -> 0xCD4550`, `0xC25F43..0xC25F95`); this default is
neither nil nor a shared mutable payload object. Exact callsites and ownership
boundaries are recorded in `Handoff.md`. Runtime listeners and names for
these numeric event codes, including logout event 83, remain unresolved.

Nonnull payload conversion `0xC25E30` reads begin/end pointers and
eight-byte `{name,value}` entries, rather than a virtual interface.
Named fields use shared-text lengths; unnamed fields receive consecutive
integer keys starting at one. Value tags 0 through 8 select strings,
raw DWORD VM values, widened floats, booleans, object-registry wrappers,
two QWORD userdata subtypes, nested payloads, and a permissive JSON-like
text parser (`0xC25790`). The latter parses one value without requiring
trailing-input exhaustion, accepts single quotes and comma skipping, and
has optional extensions. Failure returns zero and the caller pushes nil;
some malformed/extension inputs instead successfully produce nil, so these
outcomes must not be conflated. Exact grammar, numeric threshold, offsets,
and internal VM tags are tabulated in `Handoff.md`;
these are in-memory layouts, not reconstructed wire framing.
Payload entries precede the reserved `"event"` insertion, so nonnull
event text overrides a payload field with that key.
Extension mode recognizes `0x` handles and `sdb` table/ID lookups;
`sdb` scans `%127[^:]:%u` from its next lexeme. The `mat` extension
pushes its next lexeme as text, not an established material wrapper.
These format strings require PE-VA mapping before raw inspection.
`LocalizedString` SDB references instead reach `tfGlobalizationManager`,
whose unsigned-ID lookup selects one of six text pointers and falls back
to English for missing/empty selected text. Missing ID returns false/null;
existing records return true even if fallback text is empty. The parser
ultimately pushes NUL-terminated text rather than userdata; its encoding
is not established. Exact service/provider and language-field mapping is
in `Handoff.md`.

Parser failure does not locally restore VM top. A traced malformed Unicode
escape inside a partial array leaves extra slots and can redirect the next
relative-index insertion into that partial array. The actual event path
then writes reserved event text into the partial array and can select the
stranded key as callable rather than the intended handler. Returned protected
calls restore entry top, so this is not evidence of unbounded live-stack
growth. Exact static stack ledgers and guards are in `Handoff.md`; runtime
occurrence and concrete eventual callable remain unobserved.

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
