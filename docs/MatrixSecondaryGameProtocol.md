# Matrix secondary selectors: game-protocol events

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
