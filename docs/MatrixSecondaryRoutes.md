# Matrix secondary selectors: routes 2-6

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
