# Reverse-engineering handoff archive 08

## OpenMP callback maps to raw 15A1BA0 and copies three planes for selector zero

Raw `0xE99425`, bytes `68 A0 27 9A 01`, pushes callback
**VA** `0x19A27A0`, hence raw `0x15A1BA0`, not raw `0x19A27A0`.
The fork's nine arguments are
`(1,6,callback,&var_5C,&var_50,0x1C9DB,&var_38,incomingEDX,stackArg0)`.
The outlined callback consumes six stack parameters; inferred decompiler
signatures are unreliable here.
Selector zero copies three `0x4000`-byte planes to contiguous scratch;
calls at raw `0x15A1CD2/0x15A1CE9/0x15A1D00` resolve to
imported memcpy. Raw `0x15A1D86` then calls `0x12A9E30`.
No direct callback call to residual writer `0x1308970` was found;
transitive pairing and selector-to-outer-mode identity remain unproved.
The next scope follows output encoding through `0x12A9D70 -> 0x12A9BD0`.

## PrintConfig binds to immediate local execution; ProcessFile label corrected

Active-sample console vptr **VA** `0x1C77EA8` has byte-offset
`+0xE8` at raw `0x1876B90`, containing **VA** `0x128B450`
(raw `0xE8A850`), and `+0xF0` at raw `0x1876B98`,
containing **VA** `0x128B5F0` (raw `0xE8A9F0`).
The earlier BufferFile label for `0xE8A850` was incorrect:
raw `0xE8A8E3` references `ProcessFile`, **VA** `0x1BE26C4`.
BufferFile is instead raw `0xE8A6C0`, byte-offset `+0xE4`.

PrintConfig registration raw `0x2D4C92` supplies its name at **VA**
`0x1BE278C`; `0x2D4CAD` stores callback **VA** `0x128BD00`
(raw `0xE8B100`), and `0x2D4D09` links its descriptor into head
**VA** `0x1E48990`.
Callback requires `[args+0x208]>=2`, obtaining filename from base
`[args+0x20C]` plus the second argument offset from `[args+0x204]`.
Raw `0xE8B13C` pushes that pointer; `0xE8B147` invokes console
virtual byte-offset `+0xF0`, reaching `0xE8A9F0`.
It forwards the filename to path helper `0x1318870`, loads through
`0x12BB660`, then calls `0xE89EC0` at `0xE8AB3A` with
local buffer and flag 1. This is the concrete immediate-execution chain.
Filename normalization and thread identity remain unverified.

No concrete registration/caller of ProcessFile was established. That gap
does not justify attributing unrelated `+0xE8` calls to this object.
The next concrete scope follows BufferFile at `0xE8A6C0` instead.

## BufferFile copies loaded text into the global console queue

The active sample supplies a concrete caller at raw `0xE4E2BB`,
bytes `FF 96 E4 00 00 00`, on static console object **VA**
`0x1E69348`. Raw `0xE4E210` formats `!%s.con` (literal raw
`0x17DD31C`) before that call and ignores its result.
BufferFile byte-offset `+0xE4` targets raw `0xE8A6C0`;
at `0xE8A7F8` it invokes console byte-offset `+0xE0`,
target `0xE8A580`, which selects global buffer **VA** `0x1DFCAC4`.

BufferFile owns a temporary loaded by `0x12BB660` in rb mode.
Complete nonempty reads add a trailing NUL; empty input keeps its initial NUL.
Queueing copies strlen(temp) bytes, so an embedded NUL truncates queued text.
The temporary is released at raw `0xE8A813`.
Helper `0xDF3210` writes at the queue's current cursor without resetting it;
ordinary mode appends LF at `0xE8A616`, NUL at `0xE8A62C`,
then backs the cursor onto the terminator. Text mode may add indentation.
Path rejection, open/status failure, or short read returns false without
queueing. After invoking the queue, BufferFile returns true without checking
queue errors; that success does not prove allocation/write success.
The next scope resolves the formatted filename's startup source and ordering,
without guessing the meaning of its leading exclamation mark.

## Encoder helper writes a five-byte property/dictionary prefix

Active-sample raw `0x12A9CD0` (VA `0x16AA8D0`) requires
prefix capacity at least five, sets it to five, writes property
`b0+9*(b1+5*b2)` from object bytes `+0x32598/+0x3259C/+0x325A0`,
then an unaligned DWORD at output+1. The dictionary-like DWORD derives
from object `+0x3BD08`, minimum 4096, rounded to a 2x/3x power-of-two
bucket. Five is the prefix length, not total decoded output size.
Raw `0x12A9E30` initializes a processing object, writes this prefix,
then invokes `0x12A9D70` and releases the object. Prefix failure skips
processing. Its fallback `0x12A9D70 -> 0x12A9BD0` subtracts
consumed input from the input-count pointer; it is not the prefix's output
length publication. The optional processing callback is null here.
Selector zero is proven as a staging branch only, not outer mode zero.
Codec implementation identity and outer publication remain unproved by this
pass; the next scope follows status/count pointers from raw `0x15A1D86`
into candidate selection.

## Startup attempts console configuration twice, with processing inside init

Raw `0xE4E210` takes no parameters. Its filename source is
`[VA 0x1E6A684]+0x1C`, transformed by `0x13192D0 -> 0x3A5910`,
not a caller-supplied filename.
Formatting `!%s.con` occurs at raw `0xE4E2AD`, then BufferFile
at `0xE4E2BB`; raw `0xE4E327` immediately invokes console
virtual `+0xF8`, raw `0xE8A180`, which attempts queue processing.
Execution is therefore not necessarily deferred until the main loop, though
reentrancy/wait/countdown gates can postpone it.

Service initialization raw `0xE54BF0` orders:
callback(0), conditional configuration load/process, `0xE4E350`
command-line --cmd execution, `0xE52650` (Initializing Debug Menus),
conditional configuration load/process again, callback(1).
Both config gates use service byte `+0xC9`, copied from descriptor
`+0x11`; normal startup sets it to 1 at raw `0x306E1C`.
Each pass rereads the global source, so identical filenames are not guaranteed.
Concrete basename and leading-! semantics remain unresolved.
Literal settings at raw `0x17DD9E4` (VA `0x1BDEDE4`) is not
yet bound to this source. The next check follows registration around raw
`0x2D2EF3..0x2D2EF8` to global **VA** `0x1E6A684`.

## Texture selector zero publishes outer mode one and enters minimum-size selection

Raw `0x15A1D2C` initializes candidate count to capacity.
At `0x15A1D86`, `0x12A9E30` receives payload destination buffer+9,
count pointer, staged `0xC000` input, properties, prefix destination
buffer, and prefix-length pointer initially 5.
Output callback raw `0x12A9A50` copies/advances bytes and decreases
remaining capacity; `0x12A9E0D` publishes capacity-minus-remaining.
Overflow sets status 7; allocation failure returns 2.
Raw `0x15A1D8E` reports nonzero status, but if the reporting function
returns, header/count publication still proceeds.

Raw `0x15A1DB2` writes DWORD `[buffer+5]=0x0100C000`,
proving LE24 size `0xC000` and mode byte **1**.
Raw `0x15A1DB9` adds nine header bytes to payload count.
The dispatch table maps selector zero to this branch: **selector 0 produces
outer mode 1, not mode 0**.
Raw `0xE99454` compares three candidate lengths signed, selecting strict
improvements and preserving earlier ties, without separate status filtering
observed. Raw `0xE99756` frees staging buffers; `0xE9976B` frees
losing encoded buffers. `0xE99861..0xE9986B` transfers winning
pointer/count to the incoming destination pair.
Raw `0xEA075D` passes it to the already-traced append wrapper;
`0xEA0793` records count and `0xEA0797` releases the winning buffer.
The next scope resolves selector 1/2 header/mode construction.

## Startup basename is registered as eng.autoSaveConfigName, default settings

Raw `0x2D2ED7`, bytes `B9 28 A6 E6 01`, selects console-variable
object **VA** `0x1E6A628`.
Raw `0x2D2EF2` pushes default settings at **VA** `0x1BDEDE4`;
`0x2D2EF7` pushes name eng.autoSaveConfigName at **VA**
`0x1BDEDF0`. Constructor raw `0xDD5158` stores self to object
`+0x5C`, the alias cell **VA** `0x1E6A684`; `0xDD5166`
copies the default into object `+0x1C`.
This binds settings to the filename source rather than merely locating a literal.
Raw `0xE54E10` pushes the same object and `0xE54E15` invokes
console byte-offset `+0x98`, reaching `0xE41360`; `0xE41555..0xE41558`
copies replacement text into `+0x1C`.
Whether transformation `0x13192D0 -> 0x3A5910` changes that basename
and what leading ! means remain the next concrete path-resolution scope.

## Configuration path transform prepends manager prefix; ! bypasses root search

Active-sample raw `0x1319350` normalizes slash to backslash.
For ordinary input, raw `0x1319488..0x131948E` concatenates
path-manager `+4` prefix and input into result `+4`; result `+0`
retains normalized input. Raw `0x3A5926/0x3A5934` copies result
`+4` without another transformation.
Thus settings is not renamed, but the formatted source is prefix+settings,
not necessarily the bare basename.

In BufferFile path helper, raw `0x131892F` recognizes leading `!`;
`0x1318938` skips exactly that character, retaining the remaining
normalized path at result `+4`. Root-list searching belongs to the non-!
branch. Prefix matching may populate result `+0` but does not replace `+4`.
Raw `0xE8A7C0` copies result `+4` for loading.
This proves bypass semantics, not a particular directory. The next scope
resolves manager `+4` initialization before joining it to earlier write-prefix
evidence. The final import probe in this bounded pass failed; no import
conclusion relies on that failed probe.

## Candidate ordinals one/two use selectors 15/16, with distinct layouts

Worker tables raw `0x15A23B8/0x15A23CC` (VAs
`0x19A2FB8/0x19A2FCC`) map candidate ordinals 1/2 to selectors
15/16, not literal selectors 1/2 (which dispatch to default).
Mode 15 compresses `0xC000` bytes at raw `0x15A1EFC` into B+9;
`0x15A1F28` stores `[B+5]=0x0F00C000`, count=payload+9.

Mode 16 scatter at raw `0x15A2080..0x15A213F` separates each
16-byte block into source fields `+12:4`, `+2:6`, `+8:4`, `+0:2`.
Across three `0x4000` inputs, scratch regions are
`0..0x3000`, `0x3000..0x7800`, `0x7800..0xA800`,
`0xA800..0xC000`. Calls at `0x15A21D2/0x15A2273/0x15A2314`
compress lengths `0x3000/0x4800/0x4800` contiguously from B+12.
Raw `0x15A234F/0x15A2351` copies first-stream five property bytes;
`0x15A235A` stores `[B+5]=0x1000C000`; per-stream first property
bytes go to B+9/+0xA/+0xB at `0x15A2364/0x15A2370/0x15A237A`.
The reported returned count is sum(compressed lengths)+24, while
constructed data ends at B+12+sum. This discrepancy requires the next
producer/consumer operand check, not an assumed padding explanation.

Both alternate candidates conditionally preprocess when the worker's final
argument is less than 3. Helper raw `0xE98D70` applies index-DWORD
replacement via `0xEABE8D` and six-byte index replacement via
`0xEAC612/0xEAC615`, preserving endpoint fields.
Color/alpha-index interpretation was reported; exact transform semantics
and residual-writer linkage remain unproved beyond the observed field writes.

## Ordinary configuration read prefix is captured-CWD\localassets\

The active-sample trace joins the read-path prefix to concrete startup stores.
Raw `0x13178EA` (VA `0x17184EA`), bytes `E8 A1 F5 F9 FF`,
captures CWD through `0x12B6E90`; IAT **VA** `0x1AC431C`
is verified GetCurrentDirectoryW.
Normal setup raw `0x10D8BCE`, bytes `B9 7C 35 C0 01`,
selects localassets. Raw `0x13179FE` concatenates that relative
directory with the captured root; `0x1317A06/0x1317A10`
assign manager `+4`, with normalization adding a trailing backslash.
Raw `0x1319488` supplies that prefix for basename transformation.

Thus the ordinary/default settings input resolves to
`<captured-CWD>\localassets\settings.con`, matching the independently
traced write-prefix behavior. The leading ! bypasses root-list searching,
not this already-prepended physical prefix. Actual launch CWD remains
unknown, and changed basename/descriptor configuration is not asserted
to take the default path. Normal descriptor `+0x18` null is already
documented; the next concrete scope is intervening --cmd execution.

## Mode-16 count overstatement and per-stream budget are instruction-confirmed

The follow-on initially mislabeled raw IDB addresses as VAs. Correct locations:
raw `0x15A237D/0x15A2383/0x15A2386` map to VAs
`0x19A2F7D/0x19A2F83/0x19A2F86`; raw `0xE989DB`
maps to VA `0x12995DB`. Subtracted locations `0x11A177D/0xA97DB`
contain unrelated bytes and must not be used.
At the correct producer locations, `2B 34 FA` subtracts B from the
final cursor, `83 C6 0C` adds 12, and `89 74 FA 04` stores
the result. This confirms count=sum(compressed lengths)+24 while
constructed extent is B+12+sum, a 12-byte overstatement.
No padding-initialization or runtime consequence is established here.

Consumer raw `0xE989DB` adds 12 to skip its header. Each stream
advances the input cursor by the decoder-updated input count.
The available budget is saved as incoming EDX-9 at
`0xE984DA/0xE984F3`; raw `0xE98A97/0xE98AF0/0xE98B4E`
reuse that same budget without subtracting prior consumption.
No final input-end equality check was observed after stream three.
The current scope resolves wrapper/internal termination semantics before
assigning meaning to unused trailing bytes.

## Startup --cmd dispatches the first statement of each matching option directly

Raw `0xE4E350` calls `0x12BE760` to collect exact case-sensitive
cmd entries from argument-pair vector **VA** `0x1E004FC`, in order.
Option parser `0x12BD190` requires `--name=value`, splitting at
the first equals sign; matching outer single/double quotes are removed,
mismatched quotes retained. Bare --cmd does not consume the next argument.
Raw `0xE4E3CD..0xE4E3E9` passes each value with format `%s\n`
to console byte-offset `+0xD4`, raw `0xE8A200 -> 0xE89C50`.

Tokenizer `0xE89790` separates unquoted whitespace; either quote
toggles the same quoting state, backslash escapes quote/backslash, and
unquoted semicolon or any LF terminates parsing. This chain dispatches
only the first statement, not its remainder; repeated --cmd options dispatch
separately. Callbacks run directly at raw `0xE89D97`.
Variable assignments invoke console `+0x98` at `0xE89E1A`
with `(variable,value,1,0)`. Flags can reject or latch writes; shared
**VA** `0x1E49A59` selects logging-only behavior.
Thus changing eng.autoSaveConfigName before pass two is structurally possible,
not yet unconditional. Current scope resolves its concrete flags/lookup and
normal logging state. The offset clarification confirms 28 decimal means
hex `+0x1C`, not `+0x28`: raw `0xE4E237` bytes
`A1 84 A6 E6 01` load the alias, and `0xE4E243` bytes
`8B 40 1C` read its object's current text.

## LZMA wrapper publishes consumed input separately from produced output

The active-sample wrapper raw `0x12ABBD0` starts
`55 8B EC 81 EC A0 00 00 00` (VA `0x16AC7D0`).
It saves output capacity from *a2 and available input from *a4, clears
both counts, rejects input budgets below five, then restores input budget
before decoder dispatch. Decoder updates input consumption while reducing
its local remaining budget. Returned *a2 is output produced; *a4 is
input consumed, not the original available budget.
The bounded report examined a separate caller `0xEB3840/0xEB388C`;
that caller's produced-length equality and missing consumed-length equality
must not be attributed to mode 16. Its exact mode-16 call/status/end-marker
checks remain the next narrower scope.

## Basename flags allow immediate updates; exact registry publication still open

Registration raw `0x2D2EEB`, bytes `6A 00`, supplies flags zero
for eng.autoSaveConfigName. Constructor `0xDD513E` stores them at
descriptor `+0xC`. Canonical `0x200`, variable `0x10`, and latch
`0x400` gates at `0xE413CF/0xE413FD/0xE414C3` are all clear;
assignment at `0xE41555..0xE41558` changes current text `+0x1C`.
Fallback lookup `0xE4098F..0xE409C7` compares the unique suffix
autoSaveConfigName after the first dot; full-name lookup uses registry
**VA** `0x1E692CC`. Publication of this exact descriptor to that
registry remains unproved by this pass.

Logging-only byte **VA** `0x1E49A59` is loader-zeroed virtual-tail
.data, not the unrelated raw-mapped byte 05. Its initial zero selects
normal dispatch at `0xE89CAA`; setter `0xE89EB6` writes it.
PrintConfig brackets its work with 1/0 at `0xE8AB2B/0xE8AB45`.
Once lookup resolves the descriptor, --cmd can therefore change the
basename before pass two; flags do not block it. Exact registry insertion
and startup ordering are the next check, not assumed lookup success.

## Mode-16 accepts zero-error decoder completion without exact produced lengths

The active-sample mode-16 calls to raw `0x12ABBD0` are:
raw `0xE98AD1` bytes `E8 FA 30 41 00`,
`0xE98B2F` bytes `E8 9C 30 41 00`, and
`0xE98B8D` bytes `E8 3E 30 41 00`.
Capacities are `0x3000/0x4800/0x4800`; nonzero EAX goes to
`0xE98881`, clearing all three outputs and returning AL=0.
No exact-produced-length comparison appears between calls.

Core raw `0x12AB350` reports status 1 for terminal marker with zero code,
2 at capacity, 3 for insufficient input, and 4 for capacity with zero pending
length/code. Wrapper `0x12ABCDD..0x12ABCE5` converts otherwise
successful status 3 to error 6; statuses 1/2/4 remain successful.
The branch then performs fixed reconstruction and returns AL=1 through
`0xE988BD..0xE988E6`.
The next scope checks scratch initialization and fixed read extents rather
than assuming success guarantees all expected scratch bytes were produced.

## Mode-16 outer dispatch bounds scratch but does not initialize it

Raw `0xE984C0` reads the low 24 bits of the header DWORD at
`[ECX+5]` as the declared extent and the high byte as the mode. It admits
extents `0x7801..0x18000` and modes `0`, `1`, `13`, `15`, and `16`; other values
take the failure path at `0xE98881`. Mode dispatch is an indirect jump.

For an admitted mode, the second stack argument is treated as an optional
scratch pointer and the third as its available size: the function reuses the
supplied pointer only when nonnull and the declared extent does not exceed
that size. Otherwise it requests a buffer sized to the declared extent via
`0x12C8A20`; a successful temporary allocation is released through
`0x12C8D60` after dispatch. The caller-side mode-16 fixed reads are not
identified by this outer wrapper alone.

No scratch clearing is visible in raw `0xE984C0` before the mode handler.
Its `0xE98881` failure path instead zeroes three separate output pointers,
each for `0x4000` bytes, sets the return byte to zero, and releases a
temporary scratch allocation if one was obtained. On a successful dispatch,
the wrapper sets the return byte to one and frees only its temporary
allocation; it has no produced-length equality check here. Consequently,
this function does not establish whether bytes beyond each LZMA-produced
prefix were preinitialized elsewhere or which bytes the mode-16
reconstruction reads. The indirect mode-16 handler and its allocation
helper remain the next bounded checks; do not infer fresh-allocation
contents or a runtime outcome from this wrapper.

## Basename descriptor enters the pending list and startup console registry

Constructor raw `0xDD51EB` loads pending head **VA** `0x1E48990`,
stores it at descriptor `+0x14`, then replaces the head with this descriptor.
Flag-4 exclusion is clear for eng.autoSaveConfigName.
Service startup raw `0xE54C73/0xE54C79` supplies that pending head
to console virtual `+0x10`, **VA** `0x1242B00`.
The walker follows `+0x14`; this descriptor's type method raw
`0xDD5E70` returns 1, selecting console `+0x18`, **VA** `0x12426B0`.
Raw `0xE41BBC..0xE41BC6` passes name/descriptor to registry
**VA** `0x1E692CC` helper **VA** `0x11F3770`.
Raw `0xDF2BFB` publishes its bucket node and `0xDF2C19`
stores the descriptor payload.

Initializer-table **VA** `0x1AC7874` contains registry initializer
`0x6D2B20`; descriptor initializer `0x6D3AD0` occurs later at
table **VA** `0x1AC78E8`.
An existing non-placeholder same-name descriptor instead takes alias branch
raw `0xE41AF1`, writing descriptor `+0x5C`; publication does not
guarantee that the original object becomes canonical.
The next scope resolves console `+0xBC` at raw `0xE54DBA`
and dispatch-readiness ordering before startup configuration/--cmd processing.

## Console startup +0xBC target: corrected vtable arithmetic

Startup raw `0xE54DBA` bytes `FF 90 BC 00 00 00` calls
console vtable byte offset `+0xBC`, after registry registration and before
the configuration/--cmd passes. Correct table backing is raw `0x1876AA8`;
adding `0xBC` yields raw `0x1876B64`. A subsequent byte check corrects
the initially reported target: bytes `C0 BC 25 01` contain **VA**
`0x125BCC0`, target raw `0xE5B0C0`, not `0x125BDC0/0xE551C0`.
The initial report used raw `0x187C564` by arithmetic error and interpreted
unrelated bytes as an import thunk. That target/import interpretation is
discarded; no readiness behavior follows from it.
The actual body and formatting-only effect are recorded below.

## Mode-16 fixed reconstruction reads scratch through exclusive offset C000

The active-sample post-decode scatter is independent of returned produced
lengths on the successful-call path. Each output contains 1024 sixteen-byte
records; scratch ranges below are inclusive and relative to S.

| Output record field | Width / stride | Output 0 | Output 1 | Output 2 |
|---|---|---|---|---|
| `+0xC` | 4 / 4 | `0000..0FFF` | `1000..1FFF` | `2000..2FFF` |
| `+2` | 4+2 / 6 | `3000..47FF` | `4800..5FFF` | `6000..77FF` |
| `+8` | 4 / 4 | `7800..87FF` | `8800..97FF` | `9800..A7FF` |
| `+0` | 2 / 2 | `A800..AFFF` | `B000..B7FF` | `B800..BFFF` |

Raw `0xE98C1B` bytes `8B 44 38 FC` and `0xE98C26`
bytes `8B 47 FC` load DWORD planes. Six-byte planes use DWORD
loads at `0xE98C78/0xE98C9E/0xE98CBC` and word loads at
`0xE98C85/0xE98CAA/0xE98CD0`; two-byte planes use word
loads at `0xE98CE8/0xE98CFF/0xE98D16`.
Loop raw `0xE98D24..0xE98D32` increments destination offset by
`0x10`, continuing while less than `0x4000`; last record starts
at `0x3FF0`. Combined scratch reads end exclusively at S+`0xC000`.
Raw `0xE98D39` jumps to successful cleanup `0xE988BD`.
This establishes fixed read extents, not a runtime malformed-input outcome.

## Console startup +BC increments formatting depth, not readiness

Correct target raw `0xE5B0C0` (VA `0x125BCC0`) consists of
`FF 05 10 9A E4 01 C3`: increment DWORD **VA** `0x1E49A10`,
then return. It consumes no stack arguments and has no callees.
Startup raw `0xE54DBA` invokes it with the console object's address
in ECX before configuration/command processing.
Raw `0xE5ABA6` multiplies this counter by a configured width; later
memset prefixes output, capped at 32 characters. This is formatting depth,
not a dispatch-readiness gate or callback.
The next scope resolves paired decrement/reset and width binding.

## Tile records own buffers until explicit pending drain cleanup

The active-sample reader raw `0xE9FFF0` receives caller-provided
`0x28`-byte tile-record storage, not a proven refcounted request object.
Wrapper `0xEA0430` invokes it. Cleanup helper raw `0xEA0860`
(VA `0x12A1460`) loops three times from record `+0x14`:
`0xEA0889` calls free `0x12C8D60`, then `0xEA088E` nulls
the field, advancing four bytes. Thus `+0x14/+0x18/+0x1C`
are individually released; the same loop frees `+8/+0xC/+0x10`.
This is explicit buffer cleanup, not a demonstrated C++ destructor.

Concrete producer raw `0xE9F359` invokes the reader wrapper;
`0xE9F360` calls `0xE9FEA0` to prepare the other three buffers.
Raw `0xE9F538` calls `0xEA9D70`; `0xEA9DF0..0xEA9E07`
shallow-copy exactly `0x28` bytes without observed reference increments.
Drain raw `0xE9F739` calls encoder `0xEA0560`, followed by
cleanup `0xEA0860` at `0xE9F740`, then advances by `0x28`.
This supports practical ownership retained by the pending collection until
drain; shallow copying alone does not establish shared ownership.

Reader failure exits retain buffers. Separate direct caller `0xEA95F0`
tests record byte `+0x25`; its false branch frees/nuls three buffers
starting at `0xEA96F0`. Allocation-exception cleanup and exact source
type remain unresolved. The next scope examines enclosing unwind and
pending-collection destruction rather than repeating the normal release loop.

## Console depth decrement/reset are conditional; width binding remains open

Vtable entries following +BC contain **VAs** `0x125BCD0/0x125BCF0`,
raw `0xE5B0D0/0xE5B0F0`.
Decrement checks `[ecx+4]!=0`, then decrements global depth and clamps
negative to zero; otherwise it returns unchanged. The next method clears
depth only with the same object-field condition.
Separate reset raw `0xE5D13E` explicitly stores zero to **VA**
`0x1E49A10`; cleanup branches converge before setting object `+4`
to 1 and invoking an indirect call. A matched startup increment/decrement
pair is not established by this pass.

Formatter raw `0xE5ABA6` multiplies depth by DWORD `+0x3C`
of the object reached through **VA** `0x1E6AC44`, caps prefix
length at 32, and uses configured/fallback character.
Names con.indentAmount / con.indentChar at raw
`0x17DE398/0x17DE3D0` are identified, but adjacent 2/space values
are not yet proven defaults. The next narrow scope binds their registration.
The report's renewed uncertainty about startup +BC does not invalidate the
already byte-verified target above; no new contrary operand was supplied.

## Pending tile-record unwind frees collection backing, not nested buffers

Enclosing function raw `0xE9E6B0` (VA `0x129F2B0`) has pending
collection at `[ebp-0x14A4]`; state becomes 1 at raw `0xE9EDF2`.
EH metadata raw `0x1975F08` (VA `0x1D77308`) has magic
`0x19930522` and two states. State 1->0 action **VA** `0x1A62C29`
is raw `0x1662029`, whose bytes
`8D 8D 5C EB FF FF E9 CC 7C 84 FF` select the collection
and tail-jump to raw `0xEA9D00`.
That cleanup frees collection backing and zeroes its three pointers,
without record traversal or nested-field release.
Normal drain therefore remains the proven nested-buffer reclamation path;
this unwind handler alone does not establish it.
The next scope checks one concrete exception-capable path between enqueue
and drain rather than inferring a throw from allocation names.

## Pending growth can reach std::_Xbad_alloc if the allocation reporter returns

The active-sample enqueue raw `0xE9F538` calls `0xEA9D70`;
loop branch `0xE9F65E -> 0xE9F160` permits another iteration
before drain. A full collection grows at `0xEA9DE4 -> 0xEAA5A8
-> 0xEAADF0`; allocation at `0xEAAE38` is followed by null test
and conditional call through **VA** `0x1AC4424` at
`0xEAAE45..0xEAAE49`. PE import resolves to
MSVCP120!std::_Xbad_alloc, not an inferred allocator name.
EH state remains 1 through this window. Its handler releases collection
backing only; existing records' nested-buffer cleanup is not reached.

This is conditional evidence: allocator null handling first invokes reporter
through **VA** `0x1C15EA0` at raw `0x12B8006`.
Whether it returns to the null/_Xbad_alloc path or terminates remains open.
The next scope resolves that reporter; no unconditional runtime throw or
observed resource consequence is claimed.

## Indentation consumer aliases are known; raw-string xrefs do not bind defaults

Formatter raw `0xE5AB97..0xE5ABBB` consumes amount through alias
**VA** `0x1E6AC44`, DWORD object `+0x3C`, multiplying depth
and capping at 32. Character at `0xE5ABD4..0xE5ABEB` comes
through alias **VA** `0x1E6ABD4`, text object `+0x1C`, taking
its first byte; fallback literal is **VA** `0x1C5EA68`.
The reported byte 0x12 at the numeric VA is not accepted as a literal
value without raw mapping; that address is a previously verified empty string.

No registration binding was found by raw-string xrefs. This is a raw-IDB
limitation, not evidence that constructors are absent.
The pass also miscomputed `0x17DE398+0x401400` as `0x181F798`;
the correct stored string VA is `0x1BDF798` (and con.indentChar
raw `0x17DE3D0` maps to VA `0x1BDF7D0`).
The next narrow check follows self-alias object constructors and stored
VAs, not another raw-string xref scan or adjacent-default inference.

## Indentation defaults and flags are bound by exact constructors

The active-sample CRT initializer raw `0x2D3350` (VA `0x6D3F50`)
selects con.indentAmount object **VA** `0x1E6ABE8`, name **VA**
`0x1BDF798`, default **VA** `0x1BDF7C8` bytes `32 00` (2).
Raw `0x2D336B`, bytes `68 80 00 00 00`, supplies flags `0x80`.
Character initializer raw `0x2D3310` (VA `0x6D3F10`) selects
object **VA** `0x1E6AB78`, name **VA** `0x1BDF7D0`, default
**VA** `0x1BDF7CC` bytes `20 00` (space).
Raw `0x2D332B`, bytes `6A 00`, supplies flags zero.
Both call constructor **VA** `0x11D5CA0`; raw `0xDD5158`
stores self at `+0x5C`, proving the formatter aliases.
CRT pointers raw `0x16C6524/0x16C6528` contain the respective
initializer VAs. Defaults are now constructor-bound, not inferred by adjacency.

## Allocation reporter cell maps to a return-capable assertion wrapper

Correct mapping of reporter **VA** `0x1C15EA0` is raw
`0x1814AA0`, not the initially suggested `0x1804AA0`.
Cell bytes `C0 37 6C 01` contain **VA** `0x16C37C0`,
target raw `0x12C2BC0`. Allocator raw `0x12B8006` bytes
`FF 15 A0 5E C1 01` call this cell.
The wrapper invokes raw `0x12C38E0`; both have explicit returns
at `0x12C2BD8/0x12C39A9`. The callee formats assertions,
flushes, and invokes downstream reporting.
The cell lies in read-only .rdata; the bounded literal-reference scan found
calls, not publication writes.
This proves return-capable structure only, not that downstream reporting
returns in the selected failure scenario. The next scope is raw `0x12C3470`.

## Assertion reporter uses a locally continuable exception on this path

Active-sample raw `0x12C38E0` formats assertion text using sprintf_s
through IAT **VA** `0x1AC48DC`, flushes via _flushall at
**VA** `0x1AC46E0`, then calls `0x12C3470` at `0x12C3972`.
Arguments include literal exception code `0x012ED502`, ASSERT text,
message buffer, and byte flag 1. The numeric code is not an address.

Raw `0x12C34FC` calls RaiseException through IAT **VA**
`0x1AC41B0` (raw `0x16C2DB0`) with code, flags zero and four
arguments containing string lengths/pointers. Local scope metadata at
raw `0x19AD398` selects filter raw `0x12C350B`, bytes
`83 C8 FF C3`, returning -1 / EXCEPTION_CONTINUE_EXECUTION.
On this flag-1 path, code can resume, restore its frame and return at
`0x12C3545`; its parent can log conditionally then return at
`0x12C39A9`. Neither inspected function directly calls terminate/abort/exit
or checks debugger presence.

Flag zero takes a different RaiseException call at `0x12C3524`,
with EXCEPTION_NONCONTINUABLE and no local try; that is not this
allocator reporter's selected call.
Local return-capable control flow does not prove process outcome:
vectored or outer reporting handlers can intervene. The next bounded
scope looks for code `0x012ED502` recognition and actual registration,
not an unconditional recovery claim.

## Assertion-handler search finds only emitters and a continue-search VEH

The bounded active-sample search for code bytes `02 D5 2E 01`
found raw `0x12C36DB/0x12C396E`, both loading the exception code
before calling `0x12C3470`, not recognizers.
Local filter raw `0x12C350B` remains unconditional continue-execution.
VEH registration at raw `0x125371A` supplies callback **VA**
`0x1654570`, raw `0x1253970`; its `33 C0 C2 04 00`
returns continue-search unconditionally.
No code-specific interception policy is established. Computed/external
handlers remain outside this bounded coverage; the gap is parked, not
grounds for asserting either unconditional recovery or termination.

## Startup success pairs depth increment with conditional decrement

Service initialization raw `0xE54DB5..0xE54DBF` supplies the static
console object's address and invokes +BC. Normal success calls the same
object's +C0 at raw `0xE550F1`, targeting `0xE5B0D0`.
Constructor raw `0xE5879A` zeros word object `+4`, while the
decrement requires that gate nonzero. Post-increment failure exit at
`0xE550A5` bypasses +C0; this function does not call +C4 reset.
Thus call pairing is proven on success, not unconditional numeric balance.
The next scope follows gate activation through the known depth reset at
raw `0xE5D13E` and object `+4` assignment.

## Console activation precedes startup's formatting increment/decrement

Console constructor raw `0xE5879A` zeros word **VA** `0x1E6934C`.
Service init raw `0xE54C67` invokes console vtable slot zero,
whose raw `0x1876AA8` bytes `90 D9 25 01` target
**VA** `0x125D990`, raw `0xE5CD90`.
That initializer preserves this in EBX and resets depth at `0xE5D13E`,
then raw `0xE5D148`, bytes `C6 43 04 01`, sets object byte
`+4` to 1 without changing its adjacent byte.
This activation precedes startup increment `0xE54DBA`, success
decrement `0xE550F1`, and intervening failure exits.
Failure exit `0xE550A5` itself neither decrements nor clears the gate;
an earlier branch can reach it before activation. Intervening indirect
callees have not been exhaustively checked for later mutation.

## Texture index save mirrors outer reads and excludes extra allocated slot

Writer raw `0xE9DE10` (VA `0x129EA10`) serializes version 1,
N=`[owner+0x184C]`, then exactly N records of `0x4C` bytes from
`[owner+0x1848]`, without skipping a leading record.
Reader allocates N+1 but reads N, so extra slot index N is not serialized;
calling it a sentinel remains an inference pending its initialization/use.

Seven table blocks follow in ascending level 0..6: LE DWORD compressed
length L_i followed by L_i bytes, next position `position+4+L_i`.
Decoded sizes are `0x1000000,0x400000,0x100000,0x40000,0x10000,0x4000,0x1000`.
Writer allocates `S+floor(5*S/100)+66` for a decoded size S.
Raw `0xE9E04B`, bytes `E8 90 AB 46 00`, calls compressor
`0x1308BE0` (VA `0x17097E0`) with ECX=input, EDX=size,
stack output. EAX becomes L_i directly, without added codec-header length.
Reader calls matching decoder `0x13090D0`.

The compressor's 32-byte literals, extended/far matches, and first-byte
OR `0x20` at raw `0x13090B9` support a FastLZ level-2-style
interpretation; external algorithm identity is not proven by a function name.
Writes of version/count/records/length/payload have no result checks in the
inspected path; flush/commit/close likewise remain unchecked.
Rename is tested and reports failure, then returns success if reporting returns.
Exact import identities here rely on earlier PE evidence, not fresh placeholder
slot enumeration. The next scope follows the extra record's concrete use.

## Extra index record is zeroed append capacity in the traced path

Allocator raw `0xEB35E2..0xEB35E9` multiplies dimensions;
`0xEB3643..0xEB3647` invokes verified MSVCR120!memset over
all `0x4C*(N+1)` bytes, including slot N.
Lookup raw `0xE9E466..0xE9E4ED` scans strictly `[0,N)` and
returns null on miss; it does not use a search sentinel.
Append raw `0xE9F81D..0xE9F833` computes base+N*`0x4C`,
increments owner count, then `0xE9F839..0xE9F8D3` fully populates
that record before save at `0xE9F8D8`.
This establishes slot N as append capacity in this path, not a search
sentinel. Alternate consumers are not exhausted. The next check follows
array resizing across repeated appends rather than assuming permanent spare
capacity after count increments.

## Console initializes a text log sink and tolerates open failure

Console initializer raw `0xE5D2CF` invokes virtual `+0xCC`,
target `0xE5EA00` (VA `0x125F600`).
At `0xE5EA7A`, helper `0x12BA030` converts UTF-8 path to wide
and calls _wfopen through **VA** `0x1AC4704`, mode wt.
Path is object `+0x10`, default console.log; FILE* is stored at
global **VA** `0x1E49A18`.
Writer raw `0xE5AF48` calls fprintf through **VA** `0x1AC48B0`
with format %s; null handle skips output, and `0xE5AF5D` conditionally
flushes. Open failure reports `Could not open %s for logging` through
`0x2FC4F0`, severity 5, then continues without propagated failure.
Shutdown virtual `+0xD0 -> 0xE5EEE0` closes/clears the handle.
The next scope resolves callback linkage into writer `0xE5AC70`,
not an assumed connection from arbitrary log calls.

## Texture misses restore spare capacity by reloading before append

Raw `0xE9EAC2` enters owner transaction `0xE9E200`, then
`0xE9EACD` reloads the index through `0xE9D960`.
Reload frees previous record storage at `0xE9D98C`, clears its pointer
at `0xE9D991`, loads persisted N, and allocates zeroed `(N+1)*0x4C`
at `0xE9DB7A`, publishing it at `0xE9DB82`.
Append consumes the spare and publishes count N+1, then saves.
No subsequent array reallocation was found before this operation returns.
Successful repeated misses therefore reload persisted N, allocate N+1,
append/save N+1, then on the next miss reload and allocate N+2.
This is not permanent spare capacity after every append.
Owner-end `0xE9E300` follows save at `0xE9F904`.
The next scope resolves the known filesystem helpers `0x1309560/0x1309650`;
non-atomic owner nesting does not itself prove cross-thread/process exclusion.

## Category logging reaches the console writer through a callback slot

Console vtable byte-offset `+0xB4`, raw `0x1876B5C`, contains
**VA** `0x125B870`, writer raw `0xE5AC70`.
Raw `0x3041AD` selects callback **VA** `0x14D9440`,
raw `0x10D8840`; raw `0x3041CA` stores EAX to callback slot
**VA** `0x1DF96F8`. The registration helper's exact returned-value
contract remains the next check before equating every stored EAX with
that selected callback.
Category constructor raw `0x12C1A4A` stores this slot's address at
category `+0x14`; generic emitter `0x12C208F` invokes the callback.
Callback raw `0x10D89F6` dispatches console `+0xB4`.

Concrete emission raw `0xE5EA9D` logs `Logging to %s` through
`0x325A90`, severity 2, category **VA** `0x1E6B5D8`.
Its initializer `0x2D3B38..0x2D3B41` invokes category constructor
`0x12C1A20`. This joins emission, generic emitter, callback, writer,
and conditional file output.
The calls are synchronous and pass stack-local text; exclusive thread ownership
is not established. Enable/context gates and nonnull FILE still govern output.
The next scope checks callback-slot initialization/replacement and startup order.

## Texture lock uses rename claim, heartbeat and stale-name reclamation

Owner entry raw `0xE9E22A..0xE9E293` increments ordinary nesting
count; only previous-zero entry acquires object `+0x1998`, ignoring
its return. Lock constructor formats `%s_tmp_lock_file` from the already
traced owner `+0x19A0` virtual-texture location prefix.
Helper `0x12BC5A0` generates a candidate in current directory using
`XXXX-XXXX-XXXX-XXXX.tmp`, seeded by timeGetTime XOR thread ID.
Stat rejection of existing nondirectory candidates is not exclusive creation.
Acquisition raw `0x1309695..0x13096B0` fopen wb creates the
candidate; null fails, fclose result is ignored.
UTF-8/wide conversion helper `0x12B7610` then `_wrename`s candidate
to lockname; exactly zero return succeeds.

On failed rename, the algorithm samples lock mtime, zero on stat failure.
A changed mtime records `{mtime,current_time}` in object `+0x28/+0x30`.
Unchanged nonzero mtime triggers unlink/retry if elapsed high DWORD is
nonzero or low DWORD exceeds 120 seconds. Unlink result is ignored;
one further rename failure deletes the candidate and returns false.
Outer raw `0x1309570..0x13095A4` logs failed attempts and sleeps
5000 ms, retrying indefinitely without overall timeout.

Success sets held byte `+0x38`, clears observation fields and starts a
heartbeat thread. Worker raw `0x1309880` polls stop after Sleep(100)
and roughly every 30,000 accumulated milliseconds opens lockname wb,
closes it, and ignores failures. Thread-create failure does not release
the claimed name; exact wrapper ownership is the next scope.
Final owner release flushes data, stops/joins heartbeat with INFINITE wait,
unlinks lockname unchecked, clears held, destroys the object and nulls `+0x1998`.

PE evidence verifies `_wrename` IAT **VA** `0x1AC4720`,
`_wunlink` `0x1AC471C`, `_wstat64` `0x1AC472C`,
`_time64` `0x1AC48D8`, Sleep `0x1AC4374`.
This is pathname claiming plus heartbeat/stale reclamation, not universal
writer exclusion or a proven same-owner thread mutex.

## Logging callback replacement is a direct store, not a helper return

The active-sample instruction correction at raw `0x3041AD..0x3041CA`
finds no registration-helper call: immediate callback **VA** `0x14D9440`
is selected and directly stored to **VA** `0x1DF96F8`.
The earlier returned-EAX caveat is resolved; that EAX is directly formed.
Slot initial backing raw `0x19F7AF8` contains `B0 21 6C 01`,
default **VA** `0x16C21B0`, raw `0x12C15B0`.
Four direct stores found at raw `0x3041CA`, `0x10D8A34`,
`0x10D8ADE`, `0x12C1A08` install `0x14D9440`; no fallback
reset was established.
Console shutdown `0xE5EEE0..0xE5EF1A` closes/nulls two FILE
cells, not this callback slot. No slot synchronization was observed.
The report did not resolve console-open ordering, but its suggested
initializer/open chain is already documented above and is not rescanned.
The next independent scope identifies the concrete default callback behavior.

## Default logging callback writes and flushes CRT streams synchronously

Initial callback raw `0x12C15B0` reaches fprintf at `0x12C18F9`
through IAT **VA** `0x1AC48B0`, raw `0x16C34B0`; PE
hint/name evidence identifies fprintf.
Format **VA** `0x1C15D20` (raw `0x1814920`) is
`%5s %s %7s (%05u) %s\n`, containing severity label, timestamp,
category, numeric identifier and message.
Severity 5 selects `__iob_func()+0x40`, otherwise `+0x20`,
consistent with CRT stderr/stdout. Both converge on fflush at raw
`0x12C1984`, IAT **VA** `0x1AC48B4`.

Null fourth argument takes direct fprintf; nonnull builds a `0x600`-byte
stack format, forcibly terminates it, then calls vfprintf using its va-list.
Category byte `[arg_4+7]!=0` substitutes BAD MEM rather than discarding
the message. No normal severity-discard path was found; absent console
handle skips coloring only. Formatting/write/flush results are unchecked.
Normal return is raw `0x12C19CF`.
Arguments are borrowed, formatting stack-local, with no thread handoff or
ownership transfer observed. Lock-shaped initialization/acquire/release is
the next scope; first-use concurrency safety is not assumed.

## Heartbeat startup failure retains held state until owner release

Thread construction raw `0x12BFE00` clears stop/started bytes
`+4/+5`; lock ctor `0x13092D0` sets `+0x1C=self`, held
`+0x38=0`. After rename acquisition, raw `0x1309560` sets held,
joins any prior thread via `0x12BFF70(0)`, clears stop, and invokes
virtual `+4`. False from that hook returns false without clearing held.
The later thread-create-shaped call likewise returns false on null HANDLE,
stored at `+0xC`; started byte reflects handle nonzero, ID is `+0x14`.
These terminal failures happen after claim, not because the retry loop gave up.

Join helper sets stop, waits only when started/handle are nonzero, maps timeout
zero to INFINITE, closes/nulls the handle and clears started.
Worker `0x1309880` can perform a final heartbeat open/close even when
stop is noticed during the wait.
Final owner release checks held independently of successful thread creation,
joins, removes the name and clears held. Other wrapper cleanup is gated.
Thread API names remain call-shape interpretations in this report; the next
scope verifies the PE import, start argument and virtual hook to close worker
entry linkage rather than assuming it from a function label.

## Default logger serializes its body with a Win32 critical section

PE evidence identifies Initialize/Enter/Leave/TryEnter/DeleteCriticalSection
IAT **VAs** `0x1AC4280/0x1AC4278/0x1AC427C/0x1AC4284/0x1AC4274`.
Logger globals are guard **VA** `0x1F11410`, critical section
`0x1F11414`, and depth DWORD `0x1F11444`.
Raw `0x12C161C` tests bit zero then uses a plain OR/store of the
guard **before** initializing the critical section, without atomic/interlocked
instructions in that branch. Concurrent first-use safety is therefore not
established; actual simultaneous first use was not observed.

Raw `0x12C164A..0x12C1677` tries entry, blocks via Enter on
failure, increments depth and sets unwind state zero. Formatting, CRT output
and flush run within the critical section. Raw `0x12C199C` decrements
depth and leaves normally.
Exit cleanup raw `0x16C17D0` asserts nonzero depth before deleting
the critical section. Registration resembles exit cleanup but its runtime-init
helper contract remains unverified.
The next narrow scope follows handler raw `0x16948DB` to its cleanup
action before assuming exceptional release.

## Heartbeat uses _beginthreadex and a virtual worker thunk

The interrupted follow-on verifies PE import **VA** `0x1AC4AB0`
as MSVCR120!_beginthreadex, correcting earlier CreateThread-shaped naming.
Raw `0x13095EA`, bytes `68 D0 09 6C 01`, supplies start
**VA** `0x16C09D0`, raw `0x12BFDD0`.
Thunk raw `0x12BFDD9..0x12BFDDE`, bytes
`8B 4D 08 8B 01 FF 50 0C`, calls argument virtual `+0xC`,
then _endthreadex(0). Startup hook at raw `0x13095D7`
uses virtual `+4`; false skips thread creation.
Exact lock vtable slots remain the narrow next check before joining this
thunk to worker `0x1309880`.

## Logger unwind metadata identifies one cleanup action

Handler raw `0x16948DB` loads metadata **VA** `0x1DAE670`
(raw `0x19AD270`), magic `0x19930522`, maxState 1, no try blocks.
Unwind map **VA** `0x1DAE694` (raw `0x19AD294`) has state
0->-1 cleanup **VA** `0x1A954D0`, raw `0x16948D0`.
This identifies the action but does not yet prove its instructions decrement
depth or leave the critical section; the next bounded task decodes that action.

## Lock vtable confirms heartbeat worker and identifies startup-hook target

Constructor raw `0x130930A` stores vptr **VA** `0x1C812B0`,
raw `0x187FEB0`. Slot `+0xC` contains **VA** `0x170A480`,
raw `0x1309880`, closing the _beginthreadex thunk-to-worker chain.
Slot `+4` contains **VA** `0x6EC4D0`, raw `0x2EB8D0`.
Raw `0x13095D7` is the already-known virtual-call site, not a proposed
hook implementation; the follow-on's wording to the contrary adds no gap.
The next narrow check decodes the actual slot-4 target.

## Logger cleanup selects a stack guard and tail-jumps

Raw `0x16948D0` bytes `8D 8D 7C F9 FF FF E9 F5 85 C5 FE`
select `[ebp-0x684]` in ECX and jump onward.
The report did not resolve that target; its numeric target requires an
exact rel32 arithmetic check before integration.
Absence of a direct LeaveCriticalSection in this small thunk neither proves
nor refutes exceptional release. Security-cookie calls in the main handler
do not resolve the guard cleanup and are not followed as a substitute.

## Heartbeat startup hook always succeeds for the concrete lock vtable

Raw `0x2EB8D0` (VA `0x6EC4D0`) consists of `B0 01 C3`,
mov al,1; ret, followed by padding.
The callsite raw `0x13095D7` loads slot +4, calls at `0x13095DA`,
and tests AL before the false branch to `0x130960B`.
For the proven lock vtable this gate therefore succeeds unconditionally;
the generic false-hook failure described earlier is not reachable with
this exact slot target. Thread creation can still fail independently.
The next scope checks wait-result handling in `0x12BFF70`, including
the timed join caller versus final owner's INFINITE wait.

## Logger cleanup relative-jump arithmetic resolves to raw 2ECED0

For the recorded cleanup bytes, E9 is at raw `0x16948D6`, next IP
`0x16948DB`; signed displacement `F5 85 C5 FE` is
`-0x13A7A0B`. The target is consequently raw `0x2ECED0`,
VA `0x6EDAD0`, not the earlier reported `0x2F6ECB`.
The follow-on mistakenly inspected a different numeric address and supplies
no target-body evidence. The final narrow read uses IDB raw `0x2ECED0`
directly; exceptional release is still unproven pending that read.

## Logger cleanup conditionally decrements its guarded object's counter

Correct raw target `0x2ECED0` tests guard byte `[ecx+4]`, returning
if zero; otherwise it loads object `[ecx]`, pushes it, decrements
`[object+0x30]` and calls LeaveCriticalSection through **VA** `0x1AC427C`.
The guard is stack `[ebp-0x684]`. This resolves the cleanup implementation.
The report's claim that it cannot decrement global `0x1F11444` is not
supported: if its object is `0x1F11414`, object+0x30 is precisely that
global. The next check verifies the guard's initialization/activation.

## Timed thread join clears ownership even after timeout or wait failure

Raw `0x12BFFB8..0x12BFFC5` maps timeout zero to `0xFFFFFFFF`
and calls IAT **VA** `0x1AC42C8`. Result `0x102` is treated
as timeout; other nonzero results take a diagnostic path.
With nonzero handle, the wrapper subsequently closes/nulls `+0xC` and
clears started byte `+5`, including after timeout/failure.
It returns close result, not wait status; without a handle it returns the
initial virtual-call result.
Caller raw `0x1309900` uses timeout 10000 if started, then closes
separate field `+8`; worker termination is not established by that timed path.
Import names need direct PE verification, not empty IDA import enumeration.
The report's VA for raw `0x12BFF70` was arithmetically wrong:
correct text VA is `0x16C0B70`, not `0x16CCB70`.

## Logger guard initialization closes the exceptional-release chain

Raw `0x12C164F` stores critical-section address **VA** `0x1F11414`
at `[ebp-0x684]`; `0x12C1657` sets guard byte `+4` to 1.
The previously recorded EH map selects this same stack guard through
cleanup `0x16948D0 -> 0x2ECED0`.
Together these observations establish conditional unwind decrement of
`[0x1F11414+0x30]`, namely depth **VA** `0x1F11444`,
then LeaveCriticalSection. Normal acquire increments at `0x12C1671`;
normal release decrements at `0x12C199C`.
The follow-on's uninspected-handler caveat does not reopen the already
recorded handler/map/action evidence. Exact new opcode-byte strings were
not returned; binding relies on its cited operand inspection.

## Wait import remains unresolved after failed IDA helper; local parsing follows

Wait call raw `0x12BFFC5` is confirmed as bytes
`FF 15 C8 42 AC 01`, operand **VA** `0x1AC42C8`.
The bounded IDA-side PE parser failed due to helper scope; no DLL/name
identity is inferred from that failure or from empty import enumeration.
The next check parses the immutable local PE with a normal inspection
script, avoiding repeated IDA helper-scope failures.
Separately, timed caller `0x1309900`'s concrete object identity is checked
before assigning its ten-second behavior to the heartbeat lock itself.

## Immutable PE inspection verifies WaitForSingleObject

Local standard-library PE parsing confirms **KERNEL32.dll!WaitForSingleObject**:
IAT RVA `0x16C42C8`, VA `0x1AC42C8`, raw `0x16C2EC8`,
bytes `DA FB 9B 01`, pointing to import-name RVA `0x19BFBDA`
(raw `0x19BE7DA`). This independently resolves call bytes at raw
`0x12BFFC5` without relying on raw-IDB import enumeration.
The first inspected window did not contain the later CloseHandle call;
that final import operand is checked with a narrowly extended window.

## Thread join's later CloseHandle import is independently verified

Extended immutable-file inspection finds raw `0x12C003B` bytes
`FF 15 88 43 AC 01`, calling **KERNEL32.dll!CloseHandle**.
IAT **VA** `0x1AC4388` is RVA `0x16C4388`, raw `0x16C2F88`;
hint/name RVA is `0x19BF84E`, hint `0x52`.
This supplies direct import evidence for the previously traced handle cleanup,
separate from whether waiting succeeded or the worker actually terminated.

## Ten-second join belongs to a distinct wrapper, not the heartbeat lock

Raw `0x1309900` resets this vptr to **VA** `0x1C80C5C`,
distinct from heartbeat lock **VA** `0x1C812B0`.
Its started-byte check gates a join at raw `0x1309933`,
bytes `E8 38 66 FB FF`, with timeout 10000 pushed at `0x130992C`.
Regardless of that condition, raw `0x1309938/0x130993A` pushes
field `+8` and calls IAT **VA** `0x1AC4940`.
That separate resource's type must not be inferred as a thread HANDLE
or this call confused with the verified CloseHandle for wrapper `+0xC`.
Constructor candidate raw `0x13093E0` stores the same vptr at
`0x13094CE`; exact field-8 creation and its cleanup import are the
next narrow check. Concrete class name and complete caller chain remain open.

## Logger ignores normalized exit-registration result; target identity still open

Raw `0x12C161C..0x12C1647` sets the first-use guard, initializes
the critical section, and passes cleanup **VA** `0x1AC23D0`
(raw `0x16C17D0`) to the runtime-init registration helper.
The helper normalizes its result to 0/-1; this caller ignores it and continues.
A bootstrap state of -1 selects one indirect forwarding routine, other state
selects another. Neither routine's identity is established by this pass,
so no atexit/onexit/private-list or failure-branch label follows from the
helper's existing name. The next check resolves exact operands/imports.

## Timed wrapper field +8 is CRT-freed; initializer remains open

Read-only inspection of the active sample identifies IAT **VA** `0x1AC4940`
as **MSVCR120.dll!free**. Its backed raw slot is `0x16C3540`,
containing bytes `E2 0E 9C 01`. At raw `0x1309938`, bytes
`FF 76 08 FF 15 40 49 AC 01` push `[this+8]` and call that import.
This is heap deallocation, not CloseHandle, and does not identify the
allocation routine or concrete resource type.

Raw `0x13093E0` is another destructor: it releases reference-counted fields
before the conditional join and `free(this+8)`. Its shared vptr store is
not evidence of construction. The shared vptr **VA** `0x1C80C5C` maps to
backed raw `0x187F85C`; it remains distinct from heartbeat vptr
`0x1C812B0`. A bounded successor searches actual initialization stores
and one field-8 producer; no heartbeat allocation ownership follows yet.

The parallel logger follow-up only reconfirmed adjacent critical-section
imports, not the requested helper's forwarding targets. Those imports
do not resolve or refute exit registration. The next check starts from
the direct E8 displacement at raw `0x12C1642`, rather than the neighboring
indirect calls.

## Exit helper direct edge resolved; timed vptr search corrected

Immutable bytes at raw `0x12C1642` are `E8 E1 9F 0A 00`.
Signed displacement `+0xA9FE1` resolves to raw `0x136B628`
(code **VA** `0x176C228`). This thin helper calls raw `0x136B57F`
at `0x136B62E`; its own body has no indirect call. The unresolved
forwarding targets belong to the latter body, not the thin wrapper.
The preceding callback push at raw `0x12C163D` is
`68 D0 23 AC 01`. Existing runtime-init names do not establish its contract.

The timed-wrapper follow-up searched the wrong table offset and byte
pattern, so its negative results are not useful evidence of absence.
Arithmetic correction: vptr **VA** `0x1C80C5C` minus rdata mapping
`0x401400` is raw `0x187F85C`, not `0x188F85C`.
Its little-endian bytes are `5C 0C C8 01`, not `5C 0C 80 01`.
The next pass verifies the destructor's actual immediate before searching
its packed bytes for initialization stores. Allocation origin remains open.

## Corrected vptr search locates wrapper initializer and field-8 producer

The corrected `5C 0C C8 01` search finds the vptr immediate at raw
`0x12BFE0F`, in initializer raw `0x12BFE00` (code **VA** `0x16C0A00`).
It zeroes `this+8` at `0x12BFE1B`, then stores the return value of
IAT **VA** `0x1AC493C(a2)` into that field at `0x12BFE5F`.
The import's identity and argument semantics remain unresolved; the
assignment alone is not evidence of a particular allocator.

The same vptr immediate is stored at raw `0x1309907` in the previously
traced destructor, which calls `free(this+8)`. The store at `0x13094CE`
also belongs to teardown. This establishes a concrete producer/cleanup
pair without yet assigning a name or resource type to the field.

## Exit registration sentinel path is MSVCR120 _onexit

At raw `0x136B595`, bytes `8B 35 A8 43 AC 01 FF D6` load and call
**KERNEL32.dll!DecodePointer**, IAT **VA** `0x1AC43A8`
(backed raw `0x16C2FA8`). The decoded first global is compared with -1
at raw `0x136B5A0`, bytes `83 F8 FF`. The predicate is therefore not
simply an untransformed bootstrap-state integer.

For that sentinel, raw `0x136B5A8` calls
**MSVCR120.dll!_onexit**, IAT **VA** `0x1AC479C`, with callback
`[ebp+8]`. Otherwise the body decodes globals **VA** `0x1F1B474`
and `0x1F1B470`, calls a helper, then re-encodes/stores the pair.
**KERNEL32.dll!EncodePointer** is IAT **VA** `0x1AC43C4`
(backed raw `0x16C2FC4`); it is not itself the list-registration helper.
The follow-up did not return that helper's exact target or argument contract,
so only the sentinel forwarding path is fully identified here.
Import-slot file bytes are lookup RVAs, not runtime function addresses.

## Thread wrapper owns a duplicated diagnostic label

Immutable PE import parsing identifies **VA** `0x1AC493C` as
**MSVCR120.dll!_strdup**, paired with `free` at `0x1AC4940`.
Initializer raw `0x12BFE00` therefore owns a duplicate of argument `a2`
in `this+8`. Join helper raw `0x12BFF70` uses it in timeout/error logging;
destructor raw `0x1309900` conditionally waits 10000 ms, then frees it.
This field is neither a thread HANDLE nor worker data inferred from that name.

Concrete caller raw `0x2EA330` supplies label **VA** `0x1C5E964`,
backed raw `0x185D564`, bytes `DumpKeyComboThread\0`.
That identifies this caller's diagnostic label, not every base-wrapper caller
or the heartbeat wrapper. The next trace follows this concrete worker and
its stop/teardown ordering.

## Non-sentinel exit helper reaches IAT 1AC47A0

Raw `0x136B5DB..0x136B5E7` arranges the encoded callback and pointers to
locals holding decoded globals **VA** `0x1F1B474/0x1F1B470`.
Call raw `0x136B5E7`, bytes `E8 B0 01 00 00`, reaches stub
raw `0x136B79C`, bytes `FF 25 A0 47 AC 01`, jumping through IAT
**VA** `0x1AC47A0`. Return is retained in EDI at `0x136B5EF`
and returned in EAX at `0x136B614`. Its import identity remains pending
immutable PE parsing; empty raw-IDB import queries do not resolve it.

Cleanup raw `0x16C17D0` conditionally calls the already documented
assertion reporter through **VA** `0x1C15EA0`, then passes critical-section
address **VA** `0x1F11414` to IAT **VA** `0x1AC4274`.
That slot was previously verified as **KERNEL32.dll!DeleteCriticalSection**;
the follow-up's empty import query does not reopen its identity.
The cleanup's assertion predicate and non-sentinel registration contract
remain the next bounded checks.

## Exit list forwarding and logger teardown predicate resolved

Immutable PE parsing identifies IAT **VA** `0x1AC47A0` as
**MSVCR120.dll!__dllonexit**. In the non-sentinel branch of raw
`0x136B57F`, the forwarded arguments are
`(encoded callback, &local[ebp-0x1C], &local[ebp-0x20])`.
Those locals hold the decoded globals `0x1F1B474/0x1F1B470`;
the updated values are encoded and stored back after the call.
This establishes client-side forwarding/state updates, not CRT internals
or a demonstrated callback-execution order.

Cleanup raw `0x16C17D0` compares logger depth **VA** `0x1F11444` with zero.
Nonzero takes the assertion path with stack arguments
`(0x1B73D63, 0x1B73D6F, 0x7F, 0)`; caller removes `0x10` bytes.
Zero skips that reporter. Either returning path passes **VA** `0x1F11414`
to the verified DeleteCriticalSection import `0x1AC4274`.
The continuable assertion does not guarantee continuation: an unhandled
or non-continuing exception/termination remains outside this straight-line path.

## DumpKeyComboThread concrete worker and bootstrap teardown

Constructor raw `0x2EA330` installs subclass vptr **VA** `0x1C5EA48`
(backed raw `0x185D648`), whose byte-offset `+0xC` is
**VA** `0x6EAE00`, code raw `0x2EA200`.
The generic start thunk raw `0x12BFDD0` invokes that virtual slot.
The worker checks stop byte `this+4`; otherwise it sleeps 200 ms
and polls GetAsyncKeyState for `0x11/0x12/0x10/0x2E`
(Ctrl, Alt, Shift, Delete). Its byte `+0x70` gates a subsequent
matching-poll action; exact transitions/action are a separate continuation.

Bootstrap constructs the local object at raw `0x2EB14C`, starts it at
`0x2EB161`, calls ClientApplicationShutdown at `0x2EB1F9`,
then destroys the object at `0x2EB24A`.
Destructor raw `0x2EA520` restores the base vptr and joins with 10000 ms.
Join raw `0x12BFF70` requests stop through byte `+4`, waits, then
closes the handle and clears started state even on WAIT_TIMEOUT (258).
Consequently this teardown ordering does not prove worker termination.

## Dump-key arming byte and indirect action sequence

Worker raw `0x2EA200` tests the high bit of each GetAsyncKeyState result.
Constructor raw `0x2EA330` clears `this+0x70`; a failed chord test clears
it again. A matching poll with zero arms it without action; a matching
poll with it already set takes the action. If that action returns, the byte
is set to 1 again. These branches do not alone establish a rising-edge-only
trigger: subsequent matching polls can repeat the action if control returns.

Raw `0x2EA259..0x2EA28D` calls global **VA** `0x1E002B8`'s virtual
slot `+0xFC` with argument 4, then the previously identified assertion
reporter through **VA** `0x1C15EA0` with arguments
`(0, 0x1C5EA58, 203, 0x1C5E950)`, then IAT **VA** `0x1AC43A4`
with `(-1, -2)`. The import identity, assertion strings, and virtual target
remain pending. A target name inferred solely from the last arguments
would not establish termination or dump output.

The follow-up incorrectly reported `0x2EA200 + 0x400C00 = 0x6FAE00`;
the sum is `0x6EAE00`, consistent with the previously recorded table entry.
Its inability to resolve the thunk as an IDA function does not invalidate
the earlier byte-backed virtual dispatch evidence.

## CRT return-path targets pending PE verification

A bounded exit-list reference pass found registration references but no
concrete client-side drain. This is not proof that no computed drain exists.
After application return, the inspected CRT wrapper conditionally calls
IAT **VA** `0x1AC4828` and `0x1AC47E4`; exact names and branch ordering
are pending immutable PE parsing.

The pass also reported apparent `0x05050505` bytes for globals
**VA** `0x1F1B474/0x1F1B470`, without establishing file backing.
Those are not accepted as loader initial values until the PE section's
raw-size/virtual-size bounds are checked. Do not extrapolate raw-IDB
contents into virtual-only globals.

## CRT post-application exit boundary and loader-zero list globals

Immutable PE parsing resolves IAT **VA** `0x1AC4828` to
**MSVCR120.dll!exit** and `0x1AC47E4` to **MSVCR120.dll!_cexit**.
After application return, the CRT saves EAX in **VA** `0x1E36614`.
If **VA** `0x1E36618` is zero, call raw `0x136AEEE` passes that status
to `exit`. Otherwise it skips `exit`; call raw `0x136AF56` invokes
`_cexit` only if **VA** `0x1E36610` is zero, then follows local
cleanup/return. Code following the external `exit` call is reached only
if that call returns. These imports do not expose CRT-internal callback order.

Both successful initialization/runloop and initialization failure in
bootstrap raw `0x2EAE70` reach ClientApplicationShutdown at `0x2EB1F9`
before application return. Which CRT exit-mode branch this standalone
sample selects remains a flag-initialization continuation.

PE `.data` has RVA `0x19C5000`, VirtualSize `0x15647C`,
PointerToRawData `0x19C3400`, and SizeOfRawData `0x71800`.
List globals **VA** `0x1F1B470/0x1F1B474` have RVAs
`0x1B1B470/0x1B1B474`: inside its virtual range but beyond raw backing.
Their loader initial values are zero, not the previously observed
raw-IDB `0x05050505`. They have no corresponding backed raw file offsets.
