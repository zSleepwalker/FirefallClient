# Reverse-engineering handoff archive 07

## Localization globals borrow their containing table object's storage

The verified metadata layout is:

| Table | Context VA | Rows VA | Count VA |
| --- | --- | --- | --- |
| Remap | `0x1E55DD4` | `0x1E55DD8` | `0x1E55DDC` |
| Primary | `0x1E55E44` | `0x1E55E48` | `0x1E55E4C` |

These are zero-initialized `.data` virtual-tail globals, **without
file-backed raw offsets**. Mechanical section-delta subtraction would
identify unrelated bytes in another section.
Raw `0x32C187..0x32C1A0` and `0x32C457..0x32C481` store
lookup result rows, incoming `this` (saved as ESI at `0x32C07F`),
and count. Lookup `0x1310670` searches the tree at object `+0x34`;
node `+0x14` points to a holder. Success returns holder `+0xC`
and writes holder `+4` to count. Missing key explicitly returns zero
and count zero at `0x13106B5..0x13106C5`, still storing context.

The global triple stores do not retain/release anything. Alternate setters
`0x7403D0/0x740450` likewise overwrite three DWORDs; inspected
context-global references are assignments only.
Containing-object reset `0x1312030` supplies actual cleanup:
optional catalog-entry callback `+0x24` gets rows/count at
`0x1312091..0x13120A2`; holder `+0x18` provides ownership handle.
Atomic decrement of handle strong `+4` invokes slot zero on resulting
zero, then weak `+8` invokes slot `+4` on resulting zero.
Holder cleanup/free helper is `0x12C8D60` at `0x13120E5`.
Instruction `lock xadd` plus branch tests the updated count; misleading
decompiler old-count comparisons must not substitute for it.
The corresponding retain site remains unverified.

Loading entry `0x130FF0A..0x130FF10` resets an already-loaded object
before replacement; app `0x309331` calls it with context **VA**
`0x1E4BAB0`. Metadata callback `0x32C070` has no discovered direct
call; its actual **VA** `0x72CC70` appears at raw `0x1860980`,
**VA** `0x1C61D80`, reported slot `+4` of that context's table.
The precise indirect trigger and global-rebinding order remain the next
check. Row source format and sorted-order validation remain unestablished.

## Metadata callback is invoked after successful decoded-block parsing

Raw constructor `0x32C028` installs vtable **VA** `0x1C61D7C`
into context **VA** `0x1E4BAB0`. Raw table bytes at `0x186097C`
are `40 CC 72 00 70 CC 72 00 00 0F 73 00`: slot `+4`
is **VA** `0x72CC70`, raw `0x32C070`; slot `+8` is **VA**
`0x730F00`, raw `0x330300`.
Exact success invocation is `0x13104B7: mov eax,[edi]`,
`0x13104B9: mov ecx,edi`, `0x13104BB: FF 50 04`.

On inspected normal success, decoded-block parsing completes, context
loaded byte `+9=1` is stored at `0x13104A7`, temporary text is
released, then slot `+4` binds metadata before success return.
Already-loaded replacement resets at `0x130FF10`; reset invokes slot
`+8` at `0x1312065` **before** clearing loaded byte and destroying
holders/backing. Open failures return false without success callback;
parser-false calls reset at `0x131047A`, then returns false.
Exception unwinding is outside this bounded result.

One separate metadata triple, **VA** `0x1E4ED14/0x1E4ED18/0x1E4ED1C`,
is cleared by reset helper `0x7129A0` and rebound through
`0x712A00` at `0x32C0AE`. This does **not** by itself establish
clear/reset coverage of localization/remap triples `0x1E55DD4...`
and `0x1E55E44...`; those exact slot-`+8` helper paths are the next
check rather than extrapolating from another table.

## Replay propagation arming forwards no request bytes

The prior "header request" label is not established by its implementation.
Raw `0xBDFD80` sets `this+0xD8=1`, loads interface `this+8`,
and tail-jumps slot `+8` without stack arguments or serialized bytes.
SimpleSocketReplayReader constructor `0xBE1040` installs interface
vtable **VA** `0x1C73C88`; its slot `+8` contains **VA**
`0xFE2200`, raw `0xBE1600`.

That target only checks `[interface-4]` (owner `+4`), diagnoses zero
with code `0x3D`, then returns. It is not a request writer.
Separate successful case-0 publication still clears armed `+0xD8`,
copies the opaque 24-byte record to owner `+0x90..+0xA7`, and sets
owner `+0x8E=1`. Bounded xref/direct-call searches found no callers
of the arming entry; no specific request frame or record semantics were
established. Next work follows owner construction and concrete consumption
of the published record/byte.

## Replay reader reciprocal link is not its publication owner

The bounded owner-consumer search did not identify a concrete publication
owner type or a consumer of owner `+0x8E/+0x90..+0xA7`.
Constructor raw `0xBDFBC0` initializes reader `+4` to null, stores its
second argument at reader `+8`, and writes the reader pointer to that
argument's `+4`. Destructor `0xBDFCB0` verifies/clears that reciprocal
link. These are distinct fields; constructor argument two does not by
itself establish reader `+4`'s later owner.

Case-0 publication, when armed, diagnoses null reader `+4` but continues
to dereference it, locks through owner `+0x58`, copies the 24 bytes to
owner `+0x90..+0xA7`, sets `+0x8E=1`, and unlocks.
Parser callers include wrapper `0xBDF820` and callback `0xBE1450`.
Actual nonnull assignment to reader `+4` is the next scope before
guessing owner semantics from offset coincidences.

## Reset clears localization/remap triples before holder destruction

The worker's reported SHA ending `544a` was a transcription error.
Fresh file hashing and IDA server health confirm the active sample ending
`5445`. Its reset-byte observations therefore apply to the identified sample.
Raw `0x330300` directly stores immediate zero to all six globals:

| Triple | Rows clear raw | Context clear raw | Count clear raw |
| --- | --- | --- | --- |
| Remap, VAs `0x1E55DD4/0x1E55DD8/0x1E55DDC` | `0x330346` | `0x330350` | `0x33035A` |
| Primary, VAs `0x1E55E44/0x1E55E48/0x1E55E4C` | `0x330495` | `0x33049F` | `0x3304A9` |

These are straight-line `C7 05 [global] 00 00 00 00` stores, with
no helper between either triple's stores. Separate setters
`0x7403D0/0x740450` are not called here; success callback
`0x32C070` directly rebinds the triples at the previously recorded sites.

The earlier missing-direct-xref reachability caveat is retracted:
raw table `0x186097C` slot `+8` bytes `00 0F 73 00` resolve to
VA `0x730F00`, raw `0x330300`; raw `0x1312065` bytes
`FF 50 08` invoke that slot. Together with the established constructor
table assignment, this supplies the indirect reset path. It clears the
borrowed global triples before containing-holder destruction on inspected
normal reset paths. Exception/concurrent-reader behavior is not established.

## Bounded stops: reader attachment and localization row ordering

The attachment investigation did not resolve a nonnull writer to reader
`+4`. Helper raw `0xBDF9E0` receives reader `+0xC`, so its
`[this+4]=0` initializes reader `+0x10`, not the publication-owner
field. A broad field scan returned tens of thousands of unrelated matches;
it supplies no defensible attach site and was not promoted to evidence.
The worker's separate claim of invalid vtable bytes at operand
`0x1C73CC0` is withheld pending the correct PE-VA-to-raw read.

The row-order investigation likewise found no new producer proof.
Lower-bound `0x6AA040` assumes primary rows sorted by unsigned key
`+0x18`, stride `0x1C`; it neither sorts nor validates them.
Checked catalog lookup `0x1310670` returns holder payload fields but
does not establish an ordering flag/source-order contract. Row construction
and sort validation remain unresolved, not evidence that validation is
globally absent.

Further broad rechecks of those two gaps are paused. New independent
scopes are the ReplayReader endpoint comparison and the globalization
language selector's setter/default, both with concrete entry points.

The vtable-byte concern is corrected: constructor raw `0xBDFBFF`
encodes **VA** `0x1C73CC0`; its raw `.rdata` location is
`0x18728C0` and contains pointer-like entries. The earlier nonpointer
claim read VA as raw. Reported first DWORDs `0xFE0890/0xFE0980`
are stored VA values, not yet-converted raw targets; they must not be used
as raw code offsets. Full method boundary/remaining table entries were not
established by merely reading adjacent DWORDs. This does not resolve the
attachment writer.

The worker confirmed the first two stored values are **VAs**
`0xFE0890/0xFE0980`, mapping to raw `0xBDFC90/0xBDFD80`.
Only those two targets are confirmed; adjacent RTTI/data must not be
classified wholesale as method slots.

## Replay endpoint comparison uses common family bits and exact port

Raw `0xBE1450` builds its configured endpoint from `[this+4]` fields
`+0x60..+0x79` and compares it with the packet endpoint (packet `+8`).

| Field | Packet offset | Configured owner offset |
| --- | --- | --- |
| Family flags | `+8` | `+0x60` |
| IPv4 bytes | `+0xC..+0xF` | `+0x64..+0x67` |
| IPv6 bytes | `+0x10..+0x1F` | `+0x68..+0x77` |
| Port | `+0x20..+0x21` | `+0x78..+0x79` |

Helper `0xBCC440`, called at `0xBE14D6`, compares address bytes as
one DWORD for IPv4 or four DWORDs for IPv6 without swapping them.
It selects by intersection of family bits, not whole-byte equality:
common bit 1 selects IPv4 before common bit 2/IPv6. With no common bits,
it succeeds only when packet flags are zero. Port is separately compared
as 16 bits at `0xBE14DF` in every case.
Mismatch formats/logs endpoints and does not forward the blob; match
calls `0xBE0110` at `0xBE1508`. Connection closure is not established
by the mismatch branch.

Related `0xBE1890` discriminator 2 with nonnull receiver calls
`0xBE0D60(endpoint,10000)`. That helper compares 40-byte endpoint-list
records and appends fields/metadata if no match and capacity allows.
This is not proof that the list initializes fixed owner `+0x60` fields.
Their concrete writer is the next bounded check.

## Globalization selector defaults and availability policy

Raw constructor `0x38FC61` sets manager `+4=0`, English.
Numeric setter `0x390550` accepts only unsigned selector <=5 and
availability predicate `0x390640` before writing. Availability accepts
English/German/French/Chinese, rejects Korean, and requires boolean
`Globalization.RussianEnabled` (default false) for Russian.

String setter `0x3905D0` uses mapper `0x10DB480`, exact
case-sensitive `en/ko/de/fr/zh/ru`; unknown strings, including empty or
null-backed text, map to English before availability validation.
Invalid numeric selectors instead leave state unchanged.
App raw `0x307153` reads `Config.ForceLanguage`: present calls the
string setter at `0x307182`; absent uses `0x390530`, applying config
state `+0x3C` only when unsigned <=5.

After a write, differing config-state `+0x3C` causes indirect slot
`+0x88` call at `0x390597` (string equivalent `0x39062A`), with
arguments `(VA 0x1E4C310,language,0,0)`. Concrete target and persistence
remain unresolved; next work follows this notification.
Language table **VA** `0x1DE0868`, raw `0x19DEC68`, is in `.data`,
not `.rdata`. Codes are four-byte-spaced literals at **VA**
`0x1C03878..0x1C0388C`, raw `0x1802478..0x180248C`.

## Language setter updates SavedLanguage through oConsoleSystem

The indirect receiver is **VA** `0x1E69348`, RTTI `oConsoleSystem`.
Constructor raw `0xE58770` writes vptr **VA** `0x1C77EA8`
at `0xE587A3`. Slot `+0x88` targets **VA** `0x1242360`,
raw `0xE41760`, formats language with `%i`, and forwards through
slots `+0x98` then `+0x80` to raw `0xE41360`
(VA `0x1241F60`).

Argument **VA** `0x1E4C310` is console variable `SavedLanguage`,
description `Current Language`, default -1, bounds -1..5, flags `0x8C0`,
constructed at raw `0x2B3320`. Constructor `0xDD5158` writes
`[variable+0x5C]=variable`, so **VA** `0x1E4C36C` aliases this
variable. The call updates its text/numeric state including `+0x3C`,
not merely a notification. Disk persistence is not established by the
variable name; save/export filtering is the next explicit check.

## Fixed ManagedUDP endpoint writer found; interpretation pending

The fixed fields consumed by `0xBE1450` are written in `0xBEC830`,
called from constructor `0xBE1040` with `(0,0)` after ManagedUDP
state construction and callback-interface attachment.
Reported stores set flags at state `+0x60`, copy **VA**
`0x1E6853C` into IPv4 `+0x64` when bit 1 applies, copy
**VA** `0x1E68540..0x1E6854C` into IPv6 `+0x68..0x74` when
bit 2 applies, and set port `+0x78=0`.
Constructor `0xBEC2E0` also initializes flags and port zero.

The worker corrected its labels: these are raw-zero code offsets.
Raw `0xBECAB2` references absolute operand **VA** `0x1E6853C`.
Constructor `0xBE1074` computes interface `outer+8` and
`0xBE10D3` stores state at `outer+0xC`; callback
`0xBE147E` reads `[callback_this+4]`, the same state slot after
the +8 interface adjustment. This is separate from the publication-owner
field `reader+4` in the other base object.
Runtime values and local-bind versus remote-peer meaning are unresolved.
In particular, zero initial port does not prove a packet
matching failure: later bind/getsockname-related field writes must be traced.
Endpoint-list insertion `0xBE0D60` remains a separate path.

## SavedLanguage qualifies for config export, not unconditional autosave

Raw exporter `0xE426B0` requires variable flags `+0xC & 0x40`
at `0xE42711`; SavedLanguage flags `0x8C0` qualify.
Without flag `0x400`, it reads current string `+0x1C`, omits unchanged
defaults, and emits `SetSave SavedLanguage "<value>"`
(`0xE427C7..0xE42874`).
Routine `0xE8AEB0` invokes exporter through console slot `+0x48`,
builds `%s.con`, and writes through `0x12BB0B0` with verified mode
`wb`. Automatic export checks `eng.autoSaveConfig`, default zero;
config-name default is `settings` at `0xE55416..0xE5543E`.

Thus `settings.con` export capability is established, not unconditional
disk persistence after language changes. Absolute directory, trigger
timing, and flag `0x800` semantics remain unresolved. Next scope follows
one actual automatic-export caller and its guard.

## ManagedUDP peer endpoint is distinct from assigned local bind port

Raw `0xBEC830` builds a bind address through
`0xBE3990 -> 0xBE3B50` from network owner `+0x30`, port zero.
Bind is at `0xBEC9A9`, getsockname at `0xBECA23`, decode through
`0xBE3A00` at `0xBECA75`. The assigned local port is stored at
**state `+0x58`** at `0xBECA7E`, not remote port `+0x78`.

Connect routine `0xBECF50` calls `0xBE35D0`; only successful connect
reaches `0xBED064 -> 0x483640`, copying peer flags/address/port to
**state `+0x60..+0x79`**. Those are the fields compared by
`0xBE1450`, so zero constructor input does not establish a zero peer
comparison after connection.
Network family mode `+0x2C` is selected by `0xBBD110`;
`0xBE3510` maps 1 to AF_INET(2), 2 to AF_INET6(23).
Endpoint bits 1/2 represent IPv4/IPv6 presence.
PE imports/local export verification resolves WSOCK32 ordinals
2/4/6 as bind/connect/getsockname. Relevant PE globals are zero-fill;
raw-IDB `0B` bytes at their VA numbers are not initialized address values.
Next work follows the concrete ReplayReader connect wrapper and state.

## Replay peer connect occurs during reader construction

Raw caller `0x38262B` supplies a new `0x18`-byte reader in ECX and
endpoint pointer on stack to constructor `0xBE1040`.
Endpoint text comes from caller-object `+0xC`, parsed at
`0x382608 -> 0xBCBF90` as `host:port` or `[host]:port`,
with endpoint port at `+0x18`.

Constructor binds at `0xBE1120 -> 0xBEC830(0,0)`, then copies a
`0x20`-byte endpoint by value onto the stack at
`0xBE1129..0xBE1137` and calls `0xBECF50` with socket state
from reader `+0xC`. An extra decompiler EDI parameter is not an
established stack argument.
Connect returns AL zero for invalid address, zero port, state `+0x54`
not one, or connect failure. Success copies peer endpoint at `0xBED064`,
changes state to two, returns AL one.

The constructor nevertheless returns the reader pointer on connection
failure. Primary slot `+4`, raw `0xBE12D0`, checks stored peer address
and nonzero port, not socket state. RTTI confirms
`nl::SimpleSocketReplayReader`; the concrete caller's readiness/fallback
handling is the next question before inferring its success result.

## Autosave is shutdown-conditional, with an initialization enable option

Raw `0xE553F0` logs `Shutting Down`; guard
`0xE55416..0xE5541F` skips export when autosave variable `+0x40`
is zero. Call `0xE5543E` uses console slot `+0xEC`, resolved through
raw entry `0x1876B94` to `0xE8AEB0`. Variable registrations
`0x2D2EB7..0x2D2EBC` and `0x2D2EF7..0x2D2EFC` supply
defaults zero and `settings`.

Initialization raw `0x10D8CCA..0x10D8CDB` executes command
`eng.autoSaveConfig 1` only if its first argument byte `+0x11`
is nonzero. Producer/default of that option remains the next bounded
check. Persistence is conditional on shutdown, not immediate or guaranteed;
final output directory remains unresolved.

## Reader construction stores allocation; wrapper exposes inverted readiness

Raw `0x382634..0x382642` stores the constructed reader at owner
`+0x1A4`; `0x382662..0x382681` creates/stores wrapper `+0x1A0`,
borrowing the reader through wrapper `+8`.
Check `0x382686` tests wrapper existence, not connected readiness.
No connect-failure-based fallback/release was found in that construction
path. Constructor returning a pointer proves allocation, not connection.

Wrapper raw `0xBE00E0..0xBE00F3` loads the borrowed reader, invokes
slot `+4 -> 0xBE12D0`, and returns logical **not ready**.
Actual consumer of that predicate is the next bounded check.
Owner reset `0x382760` clears fields and deletes wrapper first at
`0x382806`, then reader at `0x382822`, each via slot zero with flag one.

## Normal application initialization enables shutdown autosave

`ClientApplicationInitialize`, raw `0x3063D0`, constructs descriptor
at `[ebp-0x12C]` with `0x306D82 -> 0x3248F0`.
Constructor `0x32490C` zeros descriptor word `+0x10`, including
byte `+0x11`. Application initialization then unconditionally sets that
byte one within this block at raw `0x306E1C` (`var_11B`).
Call `0x30705A` passes the descriptor to `0x10D8A40`, whose guard
at `0x10D8CC6` tests that same `+0x11` before the command call
at `0x10D8CCC`.

Thus the normal inspected initialization path explicitly enables
`eng.autoSaveConfig`, despite the console variable's registration default
zero. The descriptor field is left unnamed; no symbolic option name was
established. Export remains conditional on its later shutdown guard, current
variable state, and successful file operations, not immediate persistence
on each language setter. Output path and write-error propagation are the
next bounded questions.

## Wrapper readiness affects worker status/timing, not a proven error return

RTTI identifies wrapper `nl::SocketReplayReader`; vtable **VA**
`0x1C73CC0`, slot `+0x14` at raw `0x18728D4`, contains
**VA** `0xFE0CE0`, raw `0xBE00E0`.
Worker raw `0xBD62F0` holds it at `+0x24` and calls this slot at
`0xBD6756..0xBD675E`.
The guard at `0xBD6760..0xBD676D` requires nonzero inverted
readiness and stack byte `[esp+0x17]==0`. True sets worker `+0x54=1`
at `0xBD6782`, adjusts timing, then stores wrapper virtual `+0x18`
result into `+0x55` at `0xBD67F6`; otherwise both bytes clear at
`0xBD67FE`. This establishes status/timing behavior, not an error return.
The guard byte's producer and status consumers are the next scope;
worker/outer-owner RTTI remains unverified.

## Config export write prefix and nonuniform write failures

Raw `0xE8B02B -> 0x13192D0` selects write-directory field `+4`;
`0x3A5910` extracts the prefixed path.
Initialization `0x10D8BDA -> 0x1317970` uses descriptor `+0x18`,
otherwise default `localassets` (**VA** `0x1C0357C`, raw
`0x180217C`). Relative prefixes resolve against captured process CWD,
`0x13178B0 -> 0x12B6E90`, using GetCurrentDirectoryW.
Thus the default is `<captured-CWD>\localassets\settings.con`;
the normal application descriptor's override is the next explicit check,
and no actual absolute user directory is asserted.

Writer `0x12BB0B0 -> 0x12BA030` uses `_wfopen("wb")`.
Failed open attempts directory creation and retries; there is no mount
lookup on this inspected path. Final failure returns false and exporter
logs `Couldn't open %s.con for writing` at `0xE8B07C`.
Short fwrite invokes a diagnostic callback, then returns true if it returns;
fclose errors are ignored. These are distinct from cleanly propagated
open failure, not proof every export succeeded. No writes were executed.

## Worker readiness guard marks queue work; status has an app consumer

Raw worker `0xBD62F0` initializes stack guard byte zero at
`0xBD64EF`, sets it one at `0xBD656E` after nonzero worker
`+0xC4`, and processes its queue-like `+0xB4` structure.
At `0xBD6768..0xBD676D`, set guard prevents the inverted-readiness
timing path and clears `+0x54/+0x55` at
`0xBD67FB..0xBD6804`, even when virtual return is true.
This is a per-call work/queue-path guard, not established EOF/error.

Concrete caller raw `0x65C1F0` tests worker `+0x54` at `0x65C28E`;
with zero input count and `0x384390(worker)` false, it sets an
application field `+0xB78` at `0x65C295..0x65C2A3`.
Worker also sets `+0x54` on an earlier state-flag path at `0xBD63E6`
that returns false, so the byte must not be interpreted as a simple error
or EOF. No `+0x55` consumer or semantic label was verified.
Next scope follows the application flag and its consumer, not guessing
semantics from readiness inversion alone.

## Normal application leaves write-prefix override null

Raw descriptor constructor `0x324919` initializes `+0x18=null`.
No write to that field was found between normal app descriptor construction
at `0x306D82` and dispatch at `0x30705A`; the inspected null/empty
guard therefore selects `localassets` as write prefix.
At `0x306EE3`, app config `+0xA6C` looks up
`Development -> InstallDir`; `0x306EF6` stores it at
`[EBP-0x110]`, descriptor **`+0x1C`, not +0x18**.
Verified literals are **VA** `0x1B40774/0x1B40780`,
raw `0x173F374/0x173F380`.

Config lookup supports parent fallback and recursion-depth guarding.
The installed-directory field is a distinct option whose consumer remains
the next scope. The static result still names no absolute user path:
relative write-prefix resolution uses runtime captured CWD.

## Application +0xB78 requests a one-iteration timing override

Raw bootstrap `0x2EAE70` loads app global **VA** `0x1DFFD98`
for initialization (`0x2EB184`), run loop (`0x2EB198`), and shutdown
(`0x2EB1F9`), establishing the same app base for this writer/consumer.
Writer `0x65C1F0` sets app byte `+0xB78` at `0x65C2A3`
only after successful `0xBD62F0`, zero argument a2, worker `+0x54`
nonzero, and `0x384390(worker)` false.
That helper is true for `+0x55` or `+0xB1` set, or both
`+0x8C/+0x8D` clear. Its false case therefore requires `+0x55`
and `+0xB1` clear and at least one of `+0x8C/+0x8D` set.
Branch/store bytes were checked at `0x65C28E..0x65C2AA`.

Run loop `0x3098C0` reads app `+0xB78` at `0x309955`,
clears it at `0x30996C`, and sets its iteration-local timing scale zero
at `0x309973`. Adjacent code converts time using that scale and passes
the result to virtual `+0x20`. Continuation uses separate global byte
**VA** `0x1E4B0B0`, not `+0xB78`.
Thus this is a one-iteration timing override, not an exit/stop flag or
proof of worker EOF/error. Invocation binding of `0x65C1F0` remains
the next scope because no direct xrefs were reported.

## InstallDir field has two separately gated initialization consumers

Raw `0x10D8A40` loads descriptor `+0x1C` at `0x10D8D57`.
Nonnull text passes to `0xE9BDA0` at `0x10D8D7F` if descriptor
`+0x24` allows it; another guarded path passes the field to
`0xEBA0E0` at `0x10D8DC9` if `+0x28` allows it.
Null keeps a sentinel fallback. Separate write-prefix `+0x18` handling
remains independent. Null descriptor uses a different initialization path
at `0x10D8DD3`; exit records descriptor presence in `this+5` at
`0x10D8E04..0x10D8E10`.

The bounded pass supports a resource/path-prefix input, not a proven
physical read root. Actual default/app values of gates `+0x24/+0x28`
and concrete consumer roles are the next check; it did not trace those
helpers to file opens or independently reconfirm constructor defaults.

## Replay timing callback is the app-owned object's virtual tick

For the identified active sample, constructor raw `0x65A190`
(VA `0xA5AD90`) stores primary vptr **VA** `0x1C6781C` at
raw `0x65A204`. Vtable entry `+8`, **VA** `0x1C67824`
(raw `0x1866424`), contains **VA** `0xA5CDF0`
(raw `0x65C1F0`). This dispatch has no this-adjustment.
The constructor result is stored in app `+0xB98` at raw `0x30DB84`.

App tick dispatch at raw `0x30A009` invokes this object's virtual `+8`
with `(mode, tickArgument)` when app `+0xB98` is nonnull and
`+0xB94` is zero. Wrapper raw `0x302A30` (VA `0x703630`)
supplies global app **VA** `0x1DFFD98`; startup stores that wrapper
address at raw `0x306E55`.
This resolves concrete indirect invocation despite absent direct function
xrefs. Scheduler registration ownership, invocation ordering, and thread
remain unproven and are the narrower follow-on scope.

## InstallDir consumer gates are pointers populated by normal startup

The active-sample follow-on resolves the earlier gate-value uncertainty.
All code addresses in this section are raw offsets.
App initialization `0x3063D0` calls descriptor constructor `0x3248F0`
at `0x306D82`; construction zeroes `+0x24/+0x28`, but startup
replaces both before `0x30705A` calls `0x10D8A40`.
At `0x306FC6`, descriptor `+0x24` (`var_108`) receives
`&var_138`, a local three-DWORD structure. At `0x307048`,
`+0x28` (`var_104`) receives `&var_14C`, another local structure.
Both are nonnull pointer gates, not booleans. The second consumer reads
fields through at least pointee `+0x10`; complete pointee semantics
remain unresolved.

The `+0x24` test at `0x10D8D50` guards `0xE9BDA0`
at `0x10D8D7F`, which receives selected InstallDir text as a3.
An initial follow-on incorrectly reported forwarding that text to
`0xBBD110` at `0xE9BE03`; the instruction-level correction below
shows a separate networking initialization call with argument 1 instead.
The other consumer `0xEBA0E0` receives the text as a6 and passes
it to `0xEC2D60` at `0xEBA1EE` while building/registering
configuration state. These are concrete initialization routes, not proof
of a physical read root or write destination. Descriptor `+0x18` and
its independent `localassets` write-prefix default are unchanged.
The next bounded scope follows the first sink `0xBBD110`.

## Replay tick registration enters a service through descriptor +0x48

Startup raw `0x306E55` (VA `0x707A55`) stores callback
**VA** `0x703630` at descriptor `+0x48`: stack `[ebp-0xE4]`
relative to descriptor `[ebp-0x12C]`.
Raw `0x30705A` passes that descriptor to `0x10D8A40`.
At raw `0x10D8C58..0x10D8C83`, a nonnull descriptor is
forwarded through service virtual slot zero; false return aborts initialization.
The service is reached through the global at **VA** `0x1E60EA4`.
Raw `0x2D2D60/0x2D2D70` establish aliases involving **VAs**
`0x1DFC82C`, `0x1E60E3C`, and `0x1E60EA4`.
Their exact indirection levels and originating object initialization are the
next narrow check, not grounds for naming a scheduler owner.

This proves descriptor handoff, not retention or invocation of `+0x48`.
Concrete service vptr/slot-zero target, callback ordering, and thread remain
unresolved; no scheduler ownership claim is yet justified.

## InstallDir expands virtual-texture location strings; BBD110 is separate

The active-sample instruction check corrects the earlier claimed InstallDir
forwarding to `0xBBD110`. All instruction addresses here are raw.
At `0xE9BDD6`, EDI preserves InstallDir from `[ebp+8]`, the
third logical argument after ECX/EDX. At `0xE9BDF5`, ECX instead
loads network object `[VA 0x1DFFF70]`; `0xE9BDFB` compares
its DWORD `+8` with zero, and `0xE9BE01` pushes **1** before
`0xE9BE03` calls `0xBBD110`. This is a zero initialization-count
test, not a null field, and InstallDir is not passed.
`0xBBD110` increments existing count at `0xBBD1AE` or initializes
it to 1 at `0xBBD1C2`; literals `NetLib starting up...` and
`WSAStartup failed.` corroborate networking initialization.

Actual InstallDir use occurs at `0xE9BEE9..0xE9BEF4` and
`0xE9BF6F..0xE9BF7A`: `0x12B8E70` receives EDI and
literal `%INSTALLDIR%` (**VA** `0x1BE7B58`, raw `0x17E6758`)
to replace occurrences in local strings `[ebp-0x44]` and `[ebp-0x3C]`.
Replacement helper `0x12B9170` ensures capacity and copies bytes at
`0x12B91E6..0x12B91EF`, rather than retaining the InstallDir pointer.
At `0xE9C504`, the expanded strings pass to `0xEA1330`;
`0xEA13CB..0xEA13DC` copies them into string objects at **VAs**
`0x1E70D68/0x1E70D6C`. Helper `0x2FCC90` confirms allocation
and byte-copy behavior; local string references are released during cleanup.

The literals `Opening virtual texture` and `Installed textures: %s`
support installed/alternate virtual-texture location semantics. The input is
borrowed for expansion and the expanded results are copied; original upstream
InstallDir ownership and exact backing-store opening remain unresolved.
The next scope follows a reader of global string **VA** `0x1E70D68`.

## Descriptor service aliases point to a backed pointer cell

The bounded service-target pass resolves indirection, not the concrete object.
With `.data VA = raw + 0x401C00`, **VA** `0x1DFC82C` is
file-backed at raw `0x19FAC2C`, containing `70 04 E0 01`:
the address of cell **VA** `0x1E00470` (raw `0x19FE870`,
initially zero). These specific cells must not be classified as virtual-only.
Accessor raw `0xE4E630` (VA `0x124F230`) is
`B8 70 04 E0 01; C3`, returning the cell address.

Raw `0x2D2D60/0x2D2D65` loads `[VA 0x1DFC82C]` and stores
it to `[VA 0x1E60E3C]`; `0x2D2D70/0x2D2D75` copies that
to `[VA 0x1E60EA4]`. Thus these aliases carry the cell address,
not the service object. Raw `0x10D8C6D..0x10D8C7F` executes
`eax=[alias]; ecx=[eax]; eax=[ecx]; eax=[eax]; call eax`.
Concrete object publication, constructor/vptr, and slot-zero target remain
unresolved. The final attempted inspection failed with a Python helper-scope
NameError, not evidence that the object lacks a writer.
The next narrow check is the cell/accessor's concrete write references.

## Expanded virtual-texture locations open index files in primary/fallback order

Reader raw `0xE9D960` (VA `0x129E560`) is called at raw
`0xEA156D` after ECX receives **VA** `0x1E6F3C8`.
Its fields `+0x19A0/+0x19A4` are consequently the string globals
**VA** `0x1E70D68/0x1E70D6C`. These globals are virtual-only.
Raw `0xE9D9D7` loads the first string; a null pointer substitutes
empty literal **VA** `0x1C5EA68`.
Formatting via `0xDD89B0` uses `%s_idx` at **VA**
`0x1BE7DDC` (raw `0x17E69DC`), then converts UTF-8 path/mode
with `MultiByteToWideChar(65001,0,...,-1,...)`.

Raw `0xE9DA4D` calls IAT **VA** `0x1AC4704`
(raw `0x16C3304`), verified PE import `MSVCR120!_wfopen`,
with mode `rb`. Nonnull result branches at `0xE9DA60` directly
to `0xE9DB17`, bypassing fallback.
Only a null first result loads the second location at `0xE9DA66`,
formats it with the other `%s_idx` literal **VA** `0x1BE7DB0`
(raw `0x17E69B0`), and opens at `0xE9DAD3`, also `rb`.
This proves primary/fallback **index-file** opening, not texture-payload
or archive opening. Managed strings own copied characters; this reader
borrows their pointers. The next scope starts at successful-open branch
`0xE9DB17` to reconstruct the index's actual reads.

## Service pointer publication path and concrete object vptr

For the same active sample, initializer raw `0xE4F630`
(VA `0x1250230`) installs vptr **VA** `0x1C77C1C` at static
owner **VA** `0x1E00460`, clearing its `+0x10` pointer cell
`0x1E00470`. Owner vtable slot `+4` contains **VA** `0x124F260`.
In that method, raw `0xE4E6EC` (VA `0x124F2EC`) bytes
`89 46 10` publish constructor-returned EAX to `[esi+0x10]`.
Constructor raw `0xE4DFA0` (VA `0x124EBA0`) installs object
vptr **VA** `0x1C77C2C` at raw `0xE4DFDE`.
This supplies a concrete construction/publication path for the service
cell; actual invocation with this static owner remains unverified.
The next scope resolves object slot zero and descriptor `+0x48` handling.

## Virtual-texture index outer layout and read/cleanup boundaries

Successful file opening reaches raw `0xE9DB17` with EBX=`FILE*`.
IAT **VA** `0x1AC490C` (raw `0x16C350C`) is `fread`;
**VA** `0x1AC49A8` (raw `0x16C35A8`) is `fclose`.
Assuming complete preceding reads, native little-endian x86 fields are:

| File offset | Width | Observed use |
|---|---|---|
| `0` | DWORD | Compared with 1 at raw `0xE9DB2E`; preceding fread count unchecked. |
| `4` | DWORD N | Read to object `+0x184C` at `0xE9DB5D..0xE9DB69`; count unchecked. |
| `8` | N records of `0x4C` bytes | Read at `0xE9DB9E..0xE9DBAF`; returned item count compared with N. |
| `8+0x4C*N` onward | Seven DWORD lengths L_i, each followed by L_i bytes | Reads/loop at `0xE9DC45..0xE9DD22`. |

Raw `0xE9DB6B..0xE9DB82` allocates `(N+1)*0x4C` bytes into
object `+0x1848`; helper `0xEB35D0` uses 32-bit multiplication
and zeroes successful allocations. No range/overflow guard was observed.
For i=0..6, destination sizes are `((0x400 >> i)^2) << 4`:
`0x1000000,0x400000,0x100000,0x40000,0x10000,0x4000,0x1000`.
Pointers occupy object `+0x1850+4*i`. Raw `0xE9DCFA` calls
`0x13090D0` (VA `0x1709CD0`) with ECX=payload, EDX=L_i
and stack `(destination,size)`; its return value is ignored.
Byte-copy/back-reference processing is observed, but codec identity is
the next scope. Raw `0xE9DD2E` calls `0xE9DD50`, which walks
`0x4C` records and passes DWORDs `+0x38/+0x3C/+0x40/+0x44`
to `0xE9C990`; semantic field names remain unresolved.

Version mismatch or record-allocation failure logs conditionally and closes
at `0xE9DB55`, then reaches `0xE9DAFF` to attempt a zeroed
one-record allocation and returns AL=1, not false.
Short record/length reads invoke callback **VA** `0x1C15EA0`;
if it returns, processing continues. Payload fread count is unchecked.
Payload allocation failure also invokes that callback, without a dedicated
close branch if it returns. Normal completion closes at `0xE9DD34`
and returns AL=1 at `0xE9DD42`.
No file-length, trailing-data, or N/L_i range validation was observed in
this bounded pass. Callback termination/throwing behavior and cleanup after
non-returning callbacks remain unestablished; no malformed file was executed.

## Concrete service retains descriptor callback at object +0x100

Object vtable **VA** `0x1C77C2C` (raw `0x187682C`) begins
with bytes `F0 57 25 01`, slot-zero target **VA** `0x12557F0`
(raw `0xE54BF0`). Raw `0xE54C33` computes object `+0xB8`
and calls `0xE551F0` to copy the descriptor.
At raw `0xE55317` (VA `0x1255F17`), bytes
`8B 47 48 89 46 48` copy descriptor `+0x48` to that retained
descriptor's `+0x48`, hence service object **+0x100**.
This proves callback retention, not invocation, thread affinity, or actual
initialization through the static-owner publication path.
The next scope follows this specific implementation's `+0x100` reads/calls.

## Index block decoder grammar: literals and extended backreferences

Raw `0x13090D0` takes 32-bit source pointer/length in ECX/EDX
and destination pointer/capacity on stack. EAX is a 32-bit output byte
count, not the pointer type inferred by the decompiler.
All instruction addresses below are raw; code VA adds `0x400C00`.
No standard-codec identity or signature check has been established.

| Item | Observed decoding |
|---|---|
| Initial control | `c=*src++ & 31`; upper three bits discarded, so first item is literal (`0x13090E4..0x1309101`). |
| Literal, c less than 32 | Copy c+1 source bytes, range 1..32 (`0x1309231..0x1309257`). |
| Match, h=c>>5 in 1..6 | Length h+2, range 3..8; q=c&31 (`0x130911C..0x1309175`). |
| Match, h=7 | Length 9 plus sum of extension bytes; read another extension while previous byte is 255, including the terminating non-255 byte. |
| Ordinary distance | Read b; distance=256*q+b+1. |
| Far distance | If q=31 and b=255, read u,v; distance=8192+256*u+v, range 8192..73727. This two-byte extension is big-endian. |

Matches copy from output-distance with forward advancement, supporting overlap;
distance one has a repeated-byte optimization (`0x13091A1..0x130922F`).
Source length zero returns zero before reading. Literal input-span checking
precedes output truncation; insufficient literal input returns zero.
Match extension, offset, and far-distance bytes have no preceding source-end
checks in the inspected code. Unsigned backreference-start comparison against
destination start rejects an earlier source with return zero.
Pointer arithmetic is 32-bit, without explicit overflow checks observed.

After match metadata, source cursor greater than or equal to end finishes
after that match; equality is not required. Otherwise the next control is
prefetched. Literals finish when copied input reaches end; there is no
explicit terminator token (`0x1309181..0x1309192`,
`0x1309259..0x1309268`).
Normal completion returns produced bytes. An item exceeding output capacity
writes only the remaining capacity and immediately returns capacity, without
validating the remaining stream. Exact fill alone does not terminate decoding.
The truncated-match branch calls imported memcpy, so the explicit loop's
overlap semantics must not be assumed for that branch.
No actual compressed block was decoded or sample altered; checking a real
block's first two items remains a possible separate format-validation step.

## Index record rectangle fields reserve a separate occupancy bitmap

The bounded active-sample trace resolves record DWORDs
`+0x38/+0x3C/+0x40/+0x44` as x/y/width/height in the
separate occupancy structure, not decompressed-table indices.
Raw `0xE9DDA9` calls `0xE9C990` (VA `0x129D590`).
That routine tests rectangle bits with base `x+2016*y`, then index
`base+2016*r+c`; byte addressing uses arithmetic shift right by 3
and mask `1 << (index & 7)`. Any occupied bit returns false before
writes; otherwise it sets every tested bit.
Nonpositive dimensions succeed without writes; no coordinate bounds checks
were observed in this routine.

At raw `0xE9ED35`, `0xE9CA60` searches y-major/x-minor for
free space and reserves it. Decompressed tables independently use
`table[i]+16*j`: raw `0xE9E652` reads a QWORD and `0xEA05C6`
writes one. The record-to-table mapping is not yet resolved.
The next scope connects coordinate conversion at `0xE9EE17..0xE9F2D9`
to table index j rather than inferring a direct correspondence.

## Retained replay tick callback is invoked synchronously by service slot +8

Concrete service vtable **index 8 (byte offset +0x20)** targets **VA** `0x1256590`,
raw `0xE55990`, with EBX=this.
Raw `0xE55B5C` bytes `8B 83 00 01 00 00` load `[this+0x100]`.
A test/null branch gates the call; `0xE55B66` pushes unchanged
`[ebp+8]`, `0xE55B69` bytes `FF D0` call it, and `add esp,4`
performs caller cleanup. The callback receives one 32-bit argument,
with no explicit context argument.

Earlier nonzero service `+0x12C` returns before this callback.
Observed order is a global-service virtual `+0xF8` call, retained
callback, then raw `0xE4FE10`.
This is synchronous on the service method's invoking thread; thread
identity remains unproven. No direct parent calls were found in the
bounded pass, so the next scope resolves an external indirect slot-8 caller.

## Texture coordinates map to row-major 16-byte entries by level

The active-sample trace establishes `j=(1024 >> L)*ty+tx`.
All instruction addresses here are raw.
At `0xE9EE17..0xE9EFAD`, placement coordinates become signed
32-bit `X=(placementX<<6) SAR L`, `Y=(placementY<<6) SAR L`;
tile starts truncate X/126 and Y/126 toward zero.
Division uses signed high-product multiplication by `0x82082083`,
addition, arithmetic shift by 6 and sign correction, not unsigned division.
Counts are `trunc((end+125)/126)-start`.
Level progression is L=6,5,...,0, giving widths 16,32,...,1024;
the inner loop increments tx and outer loop ty.

Raw `0xE9F2B6..0xE9F2E1` computes the row-major j and stores
it at a working object's `+0x20`. Alternate call path
`0xE9F333..0xE9F359 -> 0xEA0430` passes `(L,tx,ty)`;
`0xEA0451..0xEA046B` checks signed coordinates against zero and
`1024 SAR L`, branching separately for invalid values.
Table pointer is `[owner+0x1850+4*L]`, entry address table+16*j.
Raw `0xE9E64F..0xE9E65F` reads entry QWORD `+0`;
`0xEA05BD..0xEA05D3` writes it.
Raw `0xEA0352` compares DWORD `+8` with an XOR-derived value;
`0xEA0126` reads DWORD `+0xC` for allocation/read length.
No further semantic field names are established.
Arithmetic wraps at 32 bits and shifts use CL. The next check follows
the writer of entry `+8` to determine its stored-value construction.

## Run loop calls service vtable index 8, not byte offset +8

The preceding service callback section's original "slot +8" wording was
ambiguous: the resolved entry is **index 8, byte offset +0x20**.
Table **VA** `0x1C77C4C` contains **VA** `0x1256590`
(raw `0xE55990`). Run-loop raw `0x3099CD..0x3099DB`
loads alias **VA** `0x1E60EA4`, dereferences the object, loads its
vptr, and selects `[vptr+0x20]`; raw `0x3099DE` calls it.
Raw `0x3099DA` pushes one rounded/scaled timestep, subsequently
forwarded unchanged to the retained callback.
This closes the synchronous runloop -> service -> callback chain,
but does not by itself prove a main-thread identity.
The exact bootstrap callsite and timing-zero destination are undergoing a
narrow operand check; the prior local-only B78 interpretation remains the
authority until that check, not an asserted write to persistent app `+0xB70`.

## Startup-thread chain and precise timing operand corrections

The narrow operand check confirms bootstrap **call opcode** raw `0x2EB19E`
(VA `0x6EBD9E`), bytes `E8 1D E7 01 00`, targeting raw
`0x3098C0` (VA `0x70A4C0`). Earlier raw `0x2EB198`
denotes the preceding `mov ecx,[VA 0x1DFFD98]`, not the call.
At raw `0x309973`, `movss [esp+0x14],xmm2` zeros the
iteration-local stack multiplier. It does **not** modify persistent app
`+0xB70`. Raw `0x30996C` separately clears app byte `+0xB78`.
Service table entry **VA** `0x1C77C4C` is base `0x1C77C2C`
plus byte offset `0x20`, zero-based index 8.

The independently traced PE entry raw `0x136ADB0`
(VA `0x176B9B0`) calls initialization then jumps into CRT startup
raw `0x136ADBA`. CRT pushes four WinMain-shaped arguments and
calls wrapper raw `0x2E9630` (VA `0x6EA230`) at raw
`0x136AEDA`. Wrapper bytes `E8 3B 18 00 00 C2 10 00`
prove `call bootstrap; ret 0x10`, targeting raw `0x2EAE70`.
Together these checks join PE startup -> wrapper -> bootstrap -> runloop
-> service index-8 tick -> retained app callback synchronously on the
PE startup thread. Numeric thread identity is not known.

## Virtual-texture entry +8 stores XOR of three CRC32 results

For the active sample, raw `0xEA0560` receives ECX=request and
stack owner argument; L is sign-extended request byte `+0x24`,
j is request DWORD `+0x20`, and table is `[owner+0x1850+4*L]`.
Raw `0xEA073F` (VA `0x12A133F`) stores ESI to entry
`table+16*j+8`. Its exact construction is:
CRC32(buffer at request `+0x14`, `0x4000` bytes) XOR
CRC32(buffer at request `+0x18`, `0x4000` bytes) XOR
CRC32(buffer at request `+0x1C`, `0x4000` bytes).
The helper's 256 table entries match reflected IEEE CRC32, with
initialization and final XOR `0xFFFFFFFF`; table **VA** `0x1C788D0`
maps to raw `0x18774D0`.

Raw `0xEA0352` (VA `0x12A0F52`) compares the identical
32-bit construction against entry `+8`; equality succeeds.
This identifies a stored content-check value, without assigning further
buffer/channel semantics. The producer-address clarification resolves a typo:
raw `0xEA01E3`, bytes `E8 D8 82 FF FF`, calls raw `0xE984C0`
(VA `0x12990C0`) with `&request[0x14]` for the three buffer pointers;
the earlier `EA984C0` spelling is invalid. Current independent scope follows the
entry's QWORD `+0` and DWORD `+0xC` storage instead.

## Texture entries store data-file byte offsets and encoded byte lengths

The active-sample trace resolves entry QWORD `+0` as an absolute byte
offset into the per-level data file, not a file-pointer position.
Raw `0xEA05A6` (VA `0x12A11A6`) calls GetFileSizeEx on
handle `[owner+0xF58+0x48*L]`; its QWORD result is stored at
entry `+0` at raw `0xEA05CD/0xEA05D3`.
Raw `0xEA05DF` invokes the encoder, obtaining payload pointer/length;
`0xEA075D` passes them to `0xE97C40`. Actual WriteFile at
`0xE97CD9` uses OVERLAPPED low/high offsets both `0xFFFFFFFF`,
requesting append at EOF. Raw `0xEA0793` stores the unmasked DWORD
encoded byte count directly at entry `+0xC`.

The reader loads that count at raw `0xEA0126`, allocates exactly it,
and calls its read wrapper at `0xEA01C9`. ReadFile at `0xE97EC7`
receives entry `+0` as OVERLAPPED low/high byte offsets.
File object is owner `+0xF48+0x48*L`, handle at object `+0x10`.
Setup formats `%s%i` at raw `0xEA15A3` and opens at `0xEA1610`;
maintenance uses owner `+0x19A0` as prefix. Data files are therefore
prefix-plus-decimal-level, distinct from `%s_idx` index files.

Observed publication order is offset, CRC, append, length, size assertion,
then `[owner+0x19B0+8*L]=offset+length`.
EOF sampling precedes the append wrapper's lock. Same-owner concurrent-writer
exclusion and durable index publication remain unproved; the next scope
follows caller serialization and index-save order rather than assuming atomicity.

## Service +0x12C is a byte shutdown-request latch

The active sample's constructor raw `0xE4E058` and initialization
`0xE54C50` clear one byte at service `+0x12C`.
Vtable **index 2, byte offset +8**, target raw `0xE55580`
(VA `0x1256180`), sets it to 1 with
`C6 81 2C 01 00 00 01; C3`.
Tick raw `0xE559C0` tests the same byte; nonzero skips tick work
and returns AL=1. Application caller at `0x3099DE` interprets this
as `Offset renderer requested application shutdown.`
This supports shutdown-request semantics, not an ordinary pause flag.
Initialization clearing is proven; successful in-place resumption is not.
The next scope resolves conditions of callers of this specific object's
byte-offset-8 setter, distinct from its byte-offset-0x20 tick method.

## Texture caller saves the index before outermost data-file flush

Active-sample raw `0xEA95F0` (VA `0x12AA1F0`) calls owner-begin
`0xE9E200`, appends at `0xEA968F`, saves at `0xEA972E`,
then calls owner-end at `0xEA9757`.
Begin increments non-atomic owner DWORD `+0x1994`, acquiring the
filesystem lock at `+0x1998` only on transition 0->1.
Acquisition at raw `0xE9E293` retries `_wrename` to
`%s_tmp_lock_file`. This is not demonstrated same-owner thread exclusion.

The index save orders fflush at raw `0xE9E0A2`, _commit at
`0xE9E0B0`, fclose, destination _wunlink, then _wrename at
`0xE9E15A`. Write/flush/commit/close results are unchecked in the
inspected path; rename failure invokes the assertion callback then returns
success if it returns.
Data-file FlushFileBuffers happens at `0xE9E366`, only on outermost
owner-end and **after index publication**; its results are unchecked.
These observations do not establish crash-atomic publication or an exclusive
same-owner writer. The next scope follows dispatch into `0xEA95F0`
for concrete thread/mutex discipline.

## Renderer shutdown-request caller remains a bounded unresolved stop

The setter entry is confirmed at raw `0x1876834`, containing **VA**
`0x1256180`, vtable index 2 / byte offset 8.
No direct E8/E9 reference was found in raw `[0,0x1700000)`.
A bounded scan of 478 alias-immediate occurrences with limited linear
provenance (at most 150 instructions/512 bytes each) found no confirmed
same-object byte-offset-8 call. Branch/interprocedural coverage is incomplete,
so absence of a caller is not established.
The reported alias chain repeats already-resolved cell-address forwarding
and supplies no new concrete requester. This gap is parked; the next
independent lifecycle scope resolves the known virtual `+0xF8` phase before
the retained app callback in `0xE55990`, not another broad alias scan.

## Texture writer dispatch remains unproven after bounded reference checks

Raw `0xEA95F0..0xEA975F` has no IDA incoming references,
no direct .text E8/E9 calls/jumps into its body, and no exact raw/RVA/VA
pointer found in the inspected file-wide scan. An interior-VA match at
raw `0x2F362E` is another call's rel32 operand, not callback evidence.
This negative coverage does not establish unreachability.
Incoming ECX is saved at `0xEA95F9/0xEA9600` and reused as
owner for begin `0xEA960E`, save `0xEA972E`, end `0xEA9757`.
Counter updates at `0xE9E22A..0xE9E233` and `0xE9E32F`
are ordinary increment/decrement; same-thread or mutex exclusion remains
unproved. The dispatch gap is parked rather than repeating broad scans.
The next independent format scope starts at concrete decoder `0xE984C0`.

## Pre-callback global phase services oConsoleSystem

The global at **VA** `0x1E69348` is RTTI `oConsoleSystem`,
zero-filled virtual-tail .data without a raw file offset.
Constructor raw `0xE587A3`, bytes `C7 05 48 93 E6 01 A8 7E C7 01`,
installs vptr **VA** `0x1C77EA8` (raw `0x1876AA8`).
Vtable byte offset `+0xF8`, raw `0x1876BA0`, contains
**VA** `0x128AD80`, raw `0xE8A180`.
Renderer tick raw `0xE55B56`, bytes `FF 90 F8 00 00 00`,
invokes it before the retained callback at `0xE55B69`.

The method is reentrancy-guarded console servicing. It conditionally uses
timeGetTime and an elapsed interval before virtual `+0xDC`, then calls
raw `0xE89EC0` with buffer **VA** `0x1DFCAC4`.
Queued-console-command processing is an interpretation, not yet a complete
buffer/dispatch proof. The next scope resolves the helper's `+0x98`
dispatch and concrete buffer consumption.

## Texture payload decoder header, modes, and three-buffer ownership

Raw `0xE984C0` (VA `0x12990C0`) receives encoded pointer/byte
count N in ECX/EDX and stack arguments: three-output-pointer array,
optional scratch, scratch capacity. Reader call raw `0xEA01E3`
passes `&request[0x14],NULL,0`. It previously allocates/zeroes three
separate `0x4000` buffers at `0xEA0060..0xEA00C9`.
Decoder does not allocate or replace these output buffers.

| Encoded offset | Width | Observed use |
|---|---|---|
| `0` | byte | LZMA property for compressed modes. |
| `1` | LE DWORD | LZMA dictionary size. |
| `5` | LE 3-byte integer S | Scratch/output-size field. |
| `8` | byte | Outer mode. |
| `9` onward | variable | Payload; mode 16 first has three property bytes here, then payload at 12. |

Raw `0xE984FF..0xE98523` checks unsigned
`S-0x7801 <= 0x107FF`, hence `0x7801..0x18000`.
No explicit N>=9 guard precedes header access/N-9, and no mode-specific
equality with reconstructed output size was observed.
Dispatch table raw `0xE98D40` selects:
mode 0 (`0xE985F2`), LZMA to scratch then scatter/postprocess;
modes 1/15 (`0xE988E7/0xE989B3`), LZMA then three consecutive
`0x4000` copies; mode 13 (`0xE98970`), direct copies from encoded
offsets `9,0x4009,0x8009`; mode 16 (`0xE989D0`), three LZMA
streams to scratch offsets `0,0x3000,0x7800` with capacities
`0x3000,0x4800,0x4800`, then scatter. Other modes fail.

First codec call raw `0xE9862E` targets `0x12ABBD0`
(VA `0x16AC7D0`). Property parser `0x12ABA30` proves LZMA:
property p<225, lc=p%9, lp=(p/9)%5, pb=p/45, dictionary minimum
`0x1000`. This is distinct from index decoder `0x13090D0`.
Compressed modes reuse sufficiently large supplied scratch or allocate
exactly S; owned scratch is released at `0xE988BD..0xE988CB`.
Failure zeroes all three outputs and returns AL=0; success AL=1.
Reader releases encoded input at `0xEA03B3`; outputs remain attached
to the request, with eventual destructor not yet traced.

## Console helper consumes statements and updates the last-value variable

Raw `0xE8A044..0xE8A058` loads the **vptr** from static object
**VA** `0x1E69348`, then sets ECX to that object's address.
It does not load an object pointer from a pointer global.
Virtual byte offset `+0x98` at table **VA** `0x1C77EA8`
targets raw `0xE41660` (VA `0x1242260`), which tail-dispatches
through byte offset `+0x80` to raw `0xE41360`.
This updates a console-variable descriptor, including text/numeric values,
modification counters, and optional descriptor `+0x54` callback.
It is **not an event-queue dispatch**.

Here arguments are `(descriptor VA 0x1E6B670, output-or-empty,0,0)`.
Constructor raw `0x2D4870` identifies variable `_`, described as
`Last value returned from a console statement.` Pointer-identical normalized
output/current value skips the update.
Parser raw `0xE89C50` returns parserEnd-inputStart, retained in EDI.
At `0xE8A075..0xE8A0B7`, if `0<consumed<=buffer length`,
memmove shifts the remaining suffix to the front, cursor `+0x18` is
adjusted, `+0x1C` conditionally cleared, and the buffer resized.
Nonzero wait/deadline state preserves the tail; exhaustion or zero consumed
clears the buffer. Temporary output text is reference-count released at
`0xE8A11F`. The next scope resolves concrete wait/deadline producers.

## Texture mode zero fully writes three 32x32 grids of 16-byte records

For the active sample, mode-zero scatter raw `0xE98690..0xE9871E`
operates on decompressed buffer B and three output buffers O[j].
Each output is 1024 records of 16 bytes, exactly `0x4000` bytes.
No color-channel interpretation is established.

| Record bytes, i=0..1023 | Source/write |
|---|---|
| `0` | Reconstructed fourth residual sequence for output j. |
| `1` | Reconstructed third sequence. |
| `2..7` | Six bytes from `B+0x3000+j*0x1800+i*6`. |
| `8..9` | LE16 reconstructed from second sequence. |
| `0xA..0xB` | LE16 reconstructed from first sequence. |
| `0xC..0xF` | Four bytes from `B+j*0x1000+i*4`. |

Scatter consumes B offsets `0..0x77FF`. Destination advances by 16,
four-byte source by four, six-byte source by six; final record begins
at output `0x3FF0` and its last DWORD ends at `0x3FFF`.
At raw `0xE9873E`, a residual reader starts at B+`0x7800`,
bit position zero, persisting across twelve reads in output order 0,1,2.
Per output it decodes `0xC00` signed words for helper `0xE981F0`
at record `+0xA`, then `0xC00` for `+8`, then `0x400`
for helper `0xE983A0` at `+1`, then `0x400` for `+0`.

Both helpers reconstruct 32x32 grids, record stride `0x10`, row stride
`0x200`: first row predicts from left, later first columns from above.
Interior selects left if `(top-left-left) > (top-left-above)`, else above.
Triplet helper uses signed 16-bit differences and word-wrapping additions;
for signed triplet (a,b,c), it computes
`s=a-(c SAR 1)`, `t=s-(b SAR 1)`, then stores
`LE16(((b+t)<<11)|((c+s)<<5)|t)`, without separate component masks.
Byte helper adds signed residuals to prediction and clamps to 0..255.

Successful mode zero writes every byte of all three outputs; none depends
on omitted-write prezeroing. Residual failures accumulate even though later
helpers may still execute; final failure at raw `0xE98881` clears every
output and returns AL=0. Residual scratch is filled by its reader, not
explicitly initialized here. Bounds use returned decoded length
`[ebp-0x184C]-0x7800`; equality with header S and exact residual
bit grammar remain the next bounded check. Final bit-cursor exhaustion is
not required in the observed path.

## Console wait state is shared countdown/deadline storage; setters unresolved

The bounded active-sample check identifies shared globals, not console-object
fields: **VA** `0x1E49A74` is a signed pump-count countdown, used
at raw `0xE89EF8..0xE89F19`; **VA** `0x1E49A78` is a
deadline, used at raw `0xE89F1C..0xE89F4B`.
Unsigned `low32(clock64/1000) < deadline` blocks continuation;
equality resumes and clears the deadline. The comparison is not wrap-safe;
milliseconds are not yet proven as the clock unit.
Raw `0xE8A0BC..0xE8A0D2` preserves the compacted remaining
buffer if either state is nonzero and arg_4 is zero.

No nonzero setter/address-taking occurrence was found in the bounded pass;
computed/indirect writes were not exhaustively covered. This producer gap
is parked rather than assigning a guessed wait-command binding.
Adjacent verified `ExecSkip` registration at raw `0x2D4992/0x2D49AD`
targets **VA** `0x128A2E0`, raw `0xE896E0`, and writes
different global **VA** `0x1E49A70`, not either wait field.
The next concrete scope follows that handler and its actual consumer.

## Residual symbols use gamma-like codes with signed-16 truncation

Active-sample helper raw `0x1308A80` (VA `0x1709680`) is called
at raw `0xE9877E`, bytes `E8 FD 02 47 00`.
ECX points to `{tail,bitpos}`, EDX is actual decoded length L minus
`0x7800`, and stack arguments are word destination/count.
Physical bits are consumed LSB-first within bytes.
Each symbol reads k zeroes then 1, followed by k payload bits accumulated
most-significant first with initial accumulator 1, yielding positive integer U.
At raw `0x1308B44`, CWDE sign-extends **16-bit U-1** to s;
even s maps to s/2, odd s to `-((s+1)/2)`.
Ordinary logical codes `1,010,011` yield `0,-1,+1`.
This truncation must not be replaced with assumed full-range zigzag semantics.
No encoded-width cap is enforced; accumulation wraps at 32 bits.

Raw `0x1308AB0..0x1308ABE` checks the byte index only before
a complete symbol, with no further unary/payload-loop bounds checks.
Success means the requested word count was produced, not that every
internal bit read was bounded. Updated bit position is always stored.
LZMA wrapper raw `0x12ABCF3` writes actual decoded length L back
through `[ebp-0x184C]`. Header S is capacity: success does not require
L=S. Residual bounds use L-`0x7800`, not S-`0x7800`.
The next check follows the matching writer from the known texture encoder
to establish intended signed mapping and maximum width.

## ExecSkip skips parsed segments, not only executable commands

The concrete active-sample chain is registration raw `0x2D4992/0x2D49AD`
-> handler `0xE896E0` -> shared global **VA** `0x1E49A70`
-> queue consumer raw `0xE89F70`.
Handler requires argument count at least two; missing count prints syntax
without assignment. Helper `0xDD5C00(0)` converts the first argument
using verified MSVCR120!atoi; extra arguments are ignored.
Raw `0xE896FA` replaces the shared count, not adds to it.
Only signed-positive values activate skipping.

Each skip still calls parser raw `0xE89790`, decrements the count at
`0xE89F8B`, bypasses dispatcher `0xE89C50`, and consumes the
parsed prefix. Unquoted semicolon/newline terminates a segment; leading
`//` consumes its line. Parsing/substitution still happens.
Thus "next N buffered parser segments" is more precise than "N commands."
The tail executes normally once the count reaches zero.
Buffer exhaustion clears surplus count at raw `0xE8A0F0`, rather
than carrying it into subsequently supplied text. Wait-tail exit and early
wait returns bypass that clear and preserve state.
The next scope identifies concrete `0xE89EC0` callers and their buffers
to bound shared-state interleaving; no concurrent runtime occurrence is claimed.

## Inverse residual writer exists; predictor pairing remains unproved

Raw `0x1308970` (VA `0x1709570`) is a concrete inverse bit writer.
At `0x13089A0..0x13089B8` it loads/sign-extends a word d,
computes m=2*d for nonnegative d or m=-1-2*d for negative d, then
U=m+1. Raw `0x13089D0..0x1308A53` emits k zeroes, one,
and k MSB-first suffix bits, with physical byte bits advanced LSB-first.
Full signed-16 inputs permit k<=16 and at most 33 bits per symbol;
no destination-capacity checking was observed.

The decoder's DEC/TEST/CWDE/SAR-or-NEG at raw
`0x1308B41..0x1308B4E` means exact arithmetic round-trip only
for d in [-16384,16383], at most 31 bits. Exhaustive local arithmetic
verification reported 32,768 mismatches outside that interval. This is
not proof that texture predictors constrain residuals to that range, nor
proof that this writer is the texture predictor's producer.
No direct calls or literal pointers to the writer were found in the
bounded pass. Known texture call raw `0xEA05DF -> 0xE993B0`
reaches indirect thunk `0x15D2614` at `0xE9942E`.
The next scope resolves that thunk/import or callback before asserting
an encoder-to-predictor linkage.

## Console file buffers share skip state but bypass waits

The active-sample mapped-byte CALL-rel32 scan found three direct callers
of raw `0xE89EC0`; IDA xrefs missed two undefined-code sites.

| Caller / callsite (raw) | Buffer | arg4 |
|---|---|---|
| `0xE8A180 / 0xE8A1EB` | Global **VA** `0x1DFCAC4`. | 0 |
| `0xE8A850 / 0xE8A994` | Invocation-local pre-push ESP+0x20, label `ProcessFile` (corrected below). | 1 |
| `0xE8A9F0 / 0xE8AB3A` | Separate invocation-local pre-push ESP+0x20, label `PrintConfig`. | 1 |

First local caller allocates storage at `0xE8A8A2`, stores it at
buffer `+8`, and calls loader `0x12BB660` at `0xE8A928`.
The loader uses _fileno, _fstat64, fread, fclose. Buffer vtable **VA**
`0x1C77A30` (raw `0x1876630`) accessor `0xDF47F0`
returns backing pointer plus offset. Cleanup releases backing storage at
`0xE8A9AE`. Second caller independently allocates at `0xE8AA43`,
loads at `0xE8AAC9`, releases at `0xE8AB5E`.

All three use absolute-global skip check `0xE89F70` and decrement
**VA** `0x1E49A70`; no caller saves/restores it. Preserved skip
state can therefore affect another buffer on a subsequent invocation.
arg4=1 bypasses wait entry checks and wait-tail suspension, but not skipping.
Normal exhaustion clears skip, potentially clearing another buffer's
preserved remainder count.
This proves shared state, not actual interleaving/concurrency.
Normal caller's non-atomic guard does not establish mutual exclusion with
either local caller. The next scope resolves their registrations/invocation.

## Texture encoder thunk is the ordinary VCOMP120 _vcomp_fork import

In the active sample, raw `0xE9942E` bytes `E8 E1 91 73 00`
call raw `0x15D2614`; following `83 C4 24` cleans 36 stack bytes.
Thunk bytes `FF 25 B0 4C AC 01` jump through IAT **VA**
`0x1AC4CB0`, raw `0x16C38B0`.
The PE descriptor for VCOMP120.DLL has FirstThunk RVA `0x16C4C94`;
entry 7 names `_vcomp_fork`. This is a normal PE import, not dynamic
resolution or raw numeric `0x1AC4CB0` file backing.

Assembly supplies nine 32-bit stack arguments; the decompiler rendered only
eight, so its call signature is not authoritative. Reported leading values
are 1 and 6, followed by an outlined callback and six data arguments.
The callback/data labels require exact raw/VA operand clarification before
assigning their target or roles. Local pointers refer to stack storage;
no ownership transfer is visible, and retention beyond the call is not
established by that observation alone.
The next scope resolves the actual outlined callback and one encoder operation,
not an assumed connection to residual writer `0x1308970`.
