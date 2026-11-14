# Matrix secondary selectors: character combat events

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
