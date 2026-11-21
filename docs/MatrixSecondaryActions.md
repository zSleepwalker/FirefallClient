# Matrix secondary selectors: character actions

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
