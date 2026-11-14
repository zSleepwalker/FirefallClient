# Reverse-engineering handoff archive 06

## Admitted login request can reach WinHTTP construction conditionally

The tracker trace extends the ignored signing-preparation result to an
actual static network-construction path. Code addresses below are raw
offsets in the active sample; API operand addresses, where used, are PE
VAs, not raw IDB addresses.

`0xBC3F40` clears request `+0x64` at `0xBC3F6C` and enqueues it
through `0x12CE310` with tracker `+0x10` at `0xBC3F73`.
Enqueue increments queue `+0x10` at `0x12CE32A`, so tracker `+0x20`
is the pending queue count. It stores the request in node `+4` at
`0x12CE3E5`, then publishes the next-node pointer at `0x12CE3ED`.

Pump `0xBC3F80` calls existing workers at `0xBC3FCB`. Its growth branch
requires positive pending count, completion count at least worker count,
and worker count below manager `+0x24` (`0xBC408C..0xBC40AC`).
Construction at `0xBC4101` supplies tracker `+0x10`; worker stores the
queue at `+0` at `0xBC27CE`. Constructor call
`0xBC2846 -> 0xBC38F0` reaches `WinHttpConnect` at `0xBC392A`
and stores its result in worker `+0x20`.

Worker `0xBC2B30` requires a nonempty queue and completion `+0x1C=0`,
then dequeues at `0xBC2BBC`. Helper `0x12CE400` decrements queue count
at `0x12CE47D`, extracts node `+4` at `0x12CE485`, and recycles the
queue node, not the request. Response `+4` receives the request at
`0xBC2C77`; callback context `+0x18` receives the response at
`0xBC2D47`. A future retry deadline instead requeues at `0xBC2BF0`.
Otherwise, nonzero worker connection handle (`0xBC2D7A`) permits
`WinHttpOpenRequest` at `0xBC2DB7`; type 2 selects UTF-16 `POST`.

No signing-context readiness or named-signature-header requirement was
found along this selected route before request construction. Generic header
serialization follows construction and is skipped when request `+0x24=0`
at `0xBC2F65`. Setup-error context `+0x14` gates the eventual
`WinHttpSendRequest` branch at `0xBC31D8`, call `0xBC3213`.
This is conditional static reachability, not observed traffic, successful
handle creation, absent/preexisting signature contents, server acceptance,
or exploitability. Next work follows failed-handle/setup-error completion
and ownership after dequeue.

## UI event delivery is deferred and retains payload ownership

`0xC38400` does not invoke registered callback code inline.
It obtains the registration pointer at `0xC3844C`, copies/retains the
payload pair through `0xC22760` at `0xC38466` (atomic control-block
`+4` increment at `0xC2277F`), then queues through `0xC42180`.
That helper allocates/reuses a 12-byte slot, copies the registration pointer
unchanged at `0xC421DF..0xC421E1`, and retains payload ownership at
`0xC421F7`. The caller releases its temporary pair at
`0xC38488..0xC384B1`. The queue owns a payload reference but only
borrows the registration-record pointer; record lifetime across unregister
or replacement is unresolved.

Scheduling at `0xC384D3` inserts the listener into manager `+0x5EC`;
`0xC53B90` suppresses duplicate listener-pointer entries.
Manager routine raw `0xC02730` is vtable slot `+0x40` (raw table
entry `0x1872AFC`, value VA `0x1003330`).
It requires manager `+0x672 != 0` at `0xC0276B..0xC02772`,
snapshots scheduled listeners at `0xC034B0..0xC034CF`, clears the
scheduling collection at `0xC034D7..0xC0353F`, and calls listener
drain `0xC38CA0` at `0xC0357B`.

Drain loads registration fields `+0/+8` and payload at
`0xC38D38..0xC38D59`, calling `0xC3B3C0` at `0xC38D5C`.
That requires listener `+0x7C` nonzero, context `+0x78` nonnull,
and nonempty callback text (`0xC3B40F..0xC3B42F`). Its path through
`0xC289C0`, `0xCD7890`, `0xCD6C10`, thunk `0xC28940`, and
`0xCD7110` ends at unresolved runtime-selected `call edi` at
`0xCD728C`. No concrete script/event-83 identity is established.

After the invocation attempt, `0xC38D88..0xC38DA6` decrements payload
ownership counters and conditionally calls virtual cleanup; then
`0xC38DA9..0xC38DB7` removes the front queue entry. Continuation
requires processed count at most manager `+0x50` and a nonempty queue
(`0xC38DBB..0xC38DC8`). Listener destruction instead calls
`0xC422C0` at `0xC33ABE`, releasing pending payloads and freeing queue
storage without invoking callbacks.

Delivery thus needs a subsequent manager-processing pass. Its frame-call
provenance, execution success, registration lifetime, and concrete callback
target remain unresolved; none establishes request cancellation.

## Failed HTTP construction: retry versus terminal completion

All code addresses here are raw offsets. API operands `0x1AC4CFC`
(`WinHttpOpenRequest`) and `0x1AC4CF8` (`WinHttpCloseHandle`) are
PE IAT VAs; names follow the existing subsystem documentation.
After a null open result, `test esi,esi` at `0xBC2DE4` branches to
`0xBC3236`. Context `+0` remains its initialized zero.
At `0xBC3264`, context `+0x14` receives `0xBC1C40`'s normalized
negative error: positive codes become `0x8007xxxx`, and other nonnegative
results become `0x80004005`. Context `+0x18` receives the response at
`0xBC3270`, response `+8=4` at `0xBC3273`, and worker `+0x1C`
receives context at `0xBC3280`.

A nonzero setup error at `0xBC31D8` instead branches to `0xBC321B`,
closes/zeros context handle `+0`, and joins the same response-kind-4
completion setup. In the same worker call, the dequeue loop exits on
nonzero completion (`0xBC2BB0`). `0xBC32A4..0xBC32BE` processes
only an existing context whose handle is zero and clears worker `+0x1C`;
negative context error leads to logging via `0xBC33CD`.

Retry guard `0xBC358F` requires request counter `+0x3E` below limit
`+0x3D` and `0xBC1130(response)` true. That helper accepts response
kinds 2 and 4, so the failed-open response satisfies its kind predicate,
but retry still depends on the counter. Retry increments `+0x3E`,
sets request `+0x60=1`, and writes a backoff deadline at `+0x48/+0x4C`.
Call `0xBC3612 -> 0x12CE310` puts the same request back on the worker's
queue. If the count has now reached the limit, the path closes/zeros
worker connection `+0x20` and calls reconnect helper `0xBC38F0`.
It clears response `+4` at `0xBC367B`, returns the response to the
worker reuse list through `0xBC5250`, and frees context through
`0x12C8D60`, without invoking the request callback.

When the counter is already at the limit (`0xBC3595`), terminal completion
records a tick at request `+0x74` (`0xBC37D5`), acquires the callback
at `0xBC37E2`, and calls its slot `+4(response)` at `0xBC37FC`.
Request slot zero with flag one follows at `0xBC380C`. Constructor
`0x3843F0` installs vtable VA `0x1C60A78` (raw `0x185F678`);
its slot zero points to VA `0x723D00`, raw `0x323100`, which runs
destructor `0x322FF0` and frees via `0x12C8D60`. The terminal path
also clears response `+4`, returns response to the reuse list
(`0xBC3866`), frees context (`0xBC386A`), and releases the callback pair.
Thus responses are recycled, contexts freed, and requests either requeued
or destroyed after terminal callback. Login callback identity and its
response-kind-4 handling are the next bounded check.

## Login response callback is weakly acquired and translates kind 4

The concrete callback wrapper is built at raw `0x636743..0x63679D`,
size `0x20`, owner `+4=tfLogin`, target `+0x10=VA 0xA3C3C0`,
adjustment `+0x14=0`. RTTI descriptor raw `0x1A10C78` (name
`0x1A10C80`) identifies
`sl::member_callback<tfLogin, HTTP::Client::Response const &>`.
Vtable VA `0x1C674A8`, raw `0x18660A8`, slot `+4` contains
VA `0xA43F80`, raw `0x643380`. Its trampoline preserves the response
argument and tail-jumps to raw `0x63B7C0`.

Control-block setup `0x6456FA..0x64570E` initializes counters
`+4/+8=1`, wrapper at `+0xC`; `0x645744..0x645747` installs the
Login-owned pair at `+0x9D8/+0x9DC`. Request preparation increments
control-block `+8` at `0x63B632` before copying the pair at
`0x63B653/0x63B659`: this request reference is weak, not ownership of
Login. Terminal completion `0xBC37E2 -> 0xBC4A80 -> 0x327730`
acquires only while strong count `+4` remains nonzero.
Null/expired callbacks skip invocation at `0xBC37F4..0xBC37F6`;
request destruction still follows. Acquired-pair release is at
`0xBC3876..0xBC38A2`.

Response-kind switch `0x63B864..0x63B88B`, table raw `0x63C5C4`,
maps kind 4 to `0x63B8A7`, loading `WinHTTP Error` (raw text
`0x17B3EE0`). Formatting at `0x63B8B3..0x63B8C0` produces
`Connection failure: type=WinHTTP Error`. The non-HTTP/error flag
skips body-error parsing at `0x63BF4D..0x63BF54`; metadata gets
`error`, request `url`, and `status=0` at `0x63BF32..0x63BF48`.
The path dispatches event `0x145` at `0x63C1EE..0x63C1F5`, clears
Login `+0x9E0` at `0x63C20D` and `+0x9E1` at `0x63C405`.
Nonempty error prevents state `+0xA48` advancing from 1 to 2.

If option `+0x9E2` is set, the path decrements `+0x9E4`; a positive
remainder waits 500 ms then calls the auto-login entry
(`0x63C460..0x63C480`), otherwise it calls `0x63E1E0`.
Finally it clears `+0x9E2` at `0x63C4C6`. Actual callback execution,
UI interpretation, and retry eligibility after logout remain unproven;
the weak acquisition is a concrete lifetime gate, not evidence that every
logout has already expired the callback.

## Registration removal: record release without local queue invalidation

The worker corrected its initial VA label: all code addresses in this
trace are raw-file/rawbasezero IDB offsets, not runtime VAs. PE metadata
gives image base `0x400000`, `.text` RVA `0x1000`, raw start `0x400`;
raw `0xC35D10` has prologue `55 8B EC 83 EC 10` and maps to
VA `0x1036910`.

Script removal entry `0xC32680` calls `0xC35D10` at `0xC32764`.
The helper resolves the dynamic ID through manager `+0x6E8`, rejects
`-1` at `0xC35D55..0xC35D58`, and exits if listener registration
collection `+0x168` lacks an entry (`0xC35D62..0xC35D76`).
For an existing entry it unsubscribes through `0xBF9BD0` at
`0xC35D80`, destroys the record at `0xC35D8E`, frees through
`0x12C8D60` at `0xC35D95`, nulls the node's record pointer at
`0xC35D9E`, then erases the node at `0xC35DAC`.

Inspected unsubscribe `0xBF9BD0` only unlinks matching listener pointers
and frees manager `+0x6F0` subscriber-list nodes
(`0xBF9C00..0xBF9C27`); destructor `0xC33E60` releases record
text fields `+0/+8`. Neither inspected helper clears listener queue
`+0x4C4`, and no drain intervenes in this local removal path.
Replacement through `0xC35BB0` instead updates the existing record's
`+8` in place (`0xC35C29..0xC35C5D`).

Outstanding-queue reachability and script-call scheduling remain unresolved;
no runtime safety or vulnerability conclusion follows. Follow-on work
traces scheduling, not merely the presence of a borrowed pointer.

## Script unbind is immediate; callback execution precedes queue removal

The scheduling trace identifies the binding rather than assuming a
script API name: raw `0xC3AE3B..0xC3AE57` registers
`Component.UnbindEvent` with target VA `0x104E880`, raw `0xC4DC80`.
Its wrapper reaches `0xC32680`, which calls removal `0xC35D10`
immediately at `0xC32764`; no defer/isolated-phase guard was found on
this path. Drain invokes the VM synchronously at `0xC38D5C`;
protected thunk `0xC28940` enters `0xCD7110`, whose native dispatch
calls at `0xCD728C`. Queue removal at `0xC38DA9` follows callback
return. Thus native unbind is statically callable during callback
execution, not established as restricted to a separate phase. Concrete
script invocation, actual outstanding registrations, and runtime outcomes
remain unobserved. This does not establish a concurrency scenario.

## Signing reset alone does not disable callback-level login retry

Cleanup `0x63A9B0` cleans Login `+0x1C` and conditionally resets
signing at `0x63AA08`; the inspected path does not clear retry eligibility.
Auto-login entry `0x63DE80` resets signing again before selecting
credentials. Queue entry `0x63B550` checks in-flight byte `+0x9E0`
and endpoint availability, not a logout-state predicate on that path.
Mode replacement logic `0x30D850`, comparison at `0x30D93F`, skips
replacement when requested mode equals current mode; requesting Login
mode therefore does not necessarily destroy a current Login object.

Actual destruction releases callback control at
`0x636EC2..0x636EEC`; weak acquisition
`0xBC4A80 -> 0x327730` rejects zero strong count, but the inspected
control block demonstrably protects the callback wrapper, not independent
ownership of Login. A surviving eligible callback has a static retry
path after reset. Scheduler ordering/interleaving and concrete runtime
occurrence remain unproven. Next work follows credential selection and
whether signing is reestablished before queuing that retry.

## Bind replacement affects already-queued deliveries

Record constructor raw `0xC53E50` sets `{+0 event text, +4 event ID,
+8 callback text}`, retaining each nonnull shared text through its
`[text-4]` counter. Existing-record replacement at
`0xC35C29..0xC35C5D` releases old callback text `+8`, assigns/retains
the replacement, and preserves record identity.
Queue copy `0xC421DF..0xC421F7` stores only the record pointer and
retained payload pair, not a callback-text snapshot.
Drain `0xC38D38..0xC38D5C` rereads record `+0/+8` at delivery.
Therefore replacement before drain changes which callback text an already
queued delivery uses. Entry `0xC32460` identifies
`Component.BindEvent(event, function)`; callback strings remain dynamic,
not identified concrete script handlers.

## Auto-login restores readiness before its selected queue invocation

Raw `0x63DEAE -> 0x63A9B0` performs the previously described
readiness-gated reset. Credential selection then follows this order:

| Branch | Acquisition guard | Setup call and return guard |
| --- | --- | --- |
| Steam | Virtual `+0x0C` true at `0x63DED0/0x63DED2`, virtual `+0x1C` nonzero 64-bit ID at `0x63DEE5/0x63DEE7`, ticket from virtual `+0x24` succeeds at `0x63DF04/0x63DF06` | `0x63DF13 -> 0x12E3960`, selector 1; false at `0x63DF18/0x63DF1A` goes to branch failure |
| Youxi | Otherwise nonempty shared text at global VA `0x1E4B2DC`, copied through `0x2FCB30`, acquisition guarded at `0x63DF99/0x63DF9B` | `0x63DFA8 -> 0x12E3960`, selector 2; false at `0x63DFAD/0x63DFAF` fails |
| 369 | Otherwise global byte VA `0x1E4B2E1` nonzero; `0x2FD5A0` obtains text from global VA `0x1E4B2E4`, acquisition guarded at `0x63E017/0x63E019` | `0x63E026 -> 0x12E3960`, selector 3; false at `0x63E02B/0x63E02D` fails |
| Reboot/cache | Otherwise `0x635E00` succeeds; `0x63B700` reads `LoginName`, overridden by nonempty Login `+0xA10`; empty account rejected at `0x63E146/0x63E148` | `0x63E167 -> 0x63AEF0(account, empty, empty, 1)`; false at `0x63E16C/0x63E16E` leaves success unset |

A selected ticket branch's acquisition/setup failure does not fall through
to another provider. Provider-specific diagnostics identify these branch
names; nonempty error text reaches `0x63E0C1 -> 0x63E1E0`.

Ticket setup rejects null/empty input at `0x12E39B5/0x12E39B7` and
`0x12E39BD/0x12E39C0`. After derivation and wrapper initialization,
`0x12E3B3D` stores parent readiness `+0x34=1` and `0x12E3B41`
stores selector `+0x38`; the false path is `0x12E3B46`.
Cache setup `0x63AEF0` attempts file recovery; `0x12E4AC0` calls
`CryptUnprotectData`. Nonempty recovered secret reaches
`0x63B102 -> 0x12E3B80`, guarded at `0x63B107/0x63B109`;
`0x12E3D50` sets readiness and `0x12E3D56` selector zero.
This is a DPAPI-protected cached secret; specifically calling it a password
or token is not established.

Missing/empty recovered secret takes `0x63B1BE -> 0x12E3DA0`,
guarded at `0x63B1C3/0x63B1C5`. That setup requires nonempty account
and second text input; this auto-login path supplies empty second input,
so it reaches reset `0x12E40C5` and returns false. The setup's general
successful path would set readiness at `0x12E40B4`.

Successful setup sets `BL=1`; `0x63E1A1/0x63E1A3` gates
`0x63E1AD -> 0x63B550(1)`. Unlike the earlier generic ignored-prep
case, this selected auto-login route restores readiness before queue
invocation. It nevertheless ignores the queue call's return and sets
success at `0x63E1B2`: that is not proof of manager admission or login
success. Follow-on work traces its caller-visible result/state.

## Null event payload produces a fresh callback table with event text

The shared default at **PE VA** `0x1E68660` is a zero-initialized
`{object,control}` pair in `.data`'s virtual-only tail, not an allocated
payload. No constructor/writer xrefs were found within the bounded trace.
Raw file offset `0x1E68660` is unrelated; its bytes must not be interpreted
as this global's initializer.

Raw `0xBF9E21` copies the default pair. Retain `0xC2277F` acts only
on nonnull controls, as does the queued copy at `0xC421F7`; the zero
default therefore retains nothing. Conversion at
`0xC25E89 -> 0xCD4550` allocates a fresh table per callback, and
`0xC25F43..0xC25F95` inserts `"event"` with the registration's
event text. Thus the default callback argument is not nil or empty.
Table mutations are not shared across recipients through this default:
there is no shared default payload object. Event 83's text is still
unresolved; arbitrary nonnull payload conversion is a separate next trace.

## Auto-login return reports setup/attempt, not manager acceptance

The bounded queue-result report confirms that submission failure
(`0x63B668..0x63B66A`) destroys the request at `0x63B672`, logs at
`0x63B687`, returns zero, and bypasses the in-flight/retry-option
writes at `0x63B698/0x63B69F`. Auto-login ignores that result at
`0x63E1AD`, sets `BL=1` at `0x63E1B2`, and returns it in `AL`
at `0x63E10E` or `0x63E1C5`; shared-text cleanup preserves it.

Update routine raw `0x6385C0` is a direct caller gated by
Login `+0xA4C` nonzero (`0x638BCD`), parent readiness false
(`0x638BEE..0x638BFD`), global flag **VA** `0x1E4B0B0` clear
(`0x638BFF`), and helper `0x63DDC0` true (`0x638C07..0x638C0E`).
A returned one branches at `0x638C19`, bypassing error dispatch
`0x638C35`. The worker corrected its initial VA label: code addresses
are raw offsets. Bytes `38 1D B0 B0 E4 01` at raw `0x638BFF`
independently establish that `0x1E4B0B0` is an absolute PE VA operand.
No new callback retry is armed by failed submission: `+0x9E2` remains
unchanged and no newly in-flight request is established. Later update
eligibility cannot be inferred merely from unchanged `+0xA4C`, because
the successful setup also restored readiness, another update guard.
The next task traces that guard and helper explicitly.

## Successful preparation blocks later update attempts after queue rejection

The update eligibility helper raw `0x63DDC0` returns true for any of:
the object at **VA** `0x1E46774` has virtual `+0x0C` true and virtual
`+0x1C` nonzero 64-bit result (`0x63DDC0..0x63DDE0`); shared text
at **VA** `0x1E4B2DC` has nonzero length
(`0x63DDE2..0x63DDFD`); or byte at **VA** `0x1E4B2E1` is
nonzero (`0x63DDFF..0x63DE0D`). Shared-text length uses DWORD
`[p-0xC]` when WORD `[p-8]==0`, otherwise unsigned WORD `[p-6]`.

Login constructor clears its `+0xA4C` at `0x63691A`, then calls
`0x63DE10` at `0x636AB0`. That helper enables only if Login
`+0xA4C==0`, application byte `+0xA45!=0`, and application DWORD
`+0xA4C!=0`, using app global **VA** `0x1DFFD98`.
It sets Login `+0xA4C=1` at `0x63DE39` before testing parent
readiness. If readiness is false, `0x63DE6A` tail-calls auto-login
without the update caller's eligibility/global guards. No subsequent
explicit clear of Login `+0xA4C` was found in the inspected displacement
references; this is a bounded search result.

Successful credential setup sets readiness at `0x12E3B3D` before queue
submission. Submission rejection does not roll that back. Later updates
exit at readiness guard `0x638BFD`, so they do not reach the setup/reset
entry again even if Login `+0xA4C` remains set. The update auto branch
also requires cached Login `+0x1C==0` at `0x6388D2`.
External resets can re-enable attempts. No fixed retry timer or update
frequency was established; the conditional update path is not a proven
repeated-attempt loop.

## Nonnull event payload layout and VM conversion

This is an in-memory x86 layout, not established wire framing. All code
addresses below are raw offsets in the identified active sample.
Converter `0xC25E30` reads payload `{+0 begin, +4 end}` directly,
iterating eight-byte `{name_pointer,value_pointer}` entries
(`0xC25EA6..0xC25EB3`, `0xC25F0C..0xC25F28`), not a payload
vtable. Nonempty names use explicit shared-text header lengths as above;
null/empty names receive integer keys 1, 2, ... counting only unnamed
entries (`0xC25EF7..0xC25F08`).

Value pointer `+0` selects tags 0 through 8 via table raw `0xC25AB8`,
whose entries are PE VAs. Null value pointers take a diagnostic/assertion
path and are still dereferenced if it returns; they are not ordinary null
encodings. The following are observed internal VM tags, not assumed
version-specific Lua constants.

| Input tag | Raw target | Observed conversion |
| --- | --- | --- |
| 0 | `0xC258DD` | String pointer `value+4`, or fallback VA `0x1C5EA68`; NUL-terminated push through `0xC27FE0` |
| 1 | `0xC25906` | Copy DWORD at `value+0x10`, VM tag 19; no floating conversion |
| 2 | `0xC25930` | Widen binary32 at `value+0x14` to binary64 with `CVTPS2PD`, VM tag 3 |
| 3 | `0xC2594E` | Normalize byte `value+0x18` to 0/1, VM tag 1 |
| 4 | `0xC2597D` | QWORD at `value+8` through object-registry lookup `0xC54160`; wrapper or VM null, not numeric conversion |
| 5 | `0xC259A2` | QWORD at `value+8` through `0xC549A0`; zero gives VM tag 0, otherwise userdata tag 71/subtype 1, exact QWORD at userdata `+0x20` |
| 6 | `0xC259C7` | Recurse through `0xC25E30(vm,[value+0x1C],0)` |
| 7 | `0xC259EB` | Stream from `value+0x20`, parsed through `0xC25790`, forwarding normalized byte `+0x18`; format/result unresolved |
| 8 | `0xC25A6C` | QWORD conversion through `0xC548C0`, like tag 5 but userdata subtype 2 |
| >8 | `0xC25A91` | VM tag 0 |

Fresh table allocation `0xCD4550` is pushed with VM tag 69 at
`0xC25E91..0xC25E9A`. Payload entries are inserted first.
If event text is nonnull, the subsequent reserved `"event"` insertion
at `0xC25F43..0xC25F95` uses `0xC285C0(vm,-3)`, overwriting
the entire existing 16-byte value and popping key/value
(`0xC285D6..0xC285F7`, `0xC2862B`). Short strings are
content-interned by `0xCD4E50`; thus reserved event text wins over a
payload `"event"` entry. With null event text this insertion is skipped.
Duplicate payload keys are last-write-wins. No runtime samples or malformed
payload execution were used; recursive/cyclic bounds and tag-7 parsing
remain separate unresolved questions.

## Submission rejection gates do not establish normal-startup failure

Wrapper raw `0x6499F0` gets manager from
`*(global at VA 0x1DFFF70 + 0xA0)`. Submission `0xBC43C0`
returns zero before route lookup/enqueue for manager `+0x21==0`
(`0xBC440E..0xBC4412`), manager `+0x20!=0`
(`0xBC4418..0xBC441C`), or validation `0xBC1380` false at
`0xBC4424`. Accepted linking is at `0xBC4503`.

Validation calls URL predicate `0xBCDF20(request+0xC)`. It requires
nonnull/nonempty host at request `+0x10`
(`0xBCDF20..0xBCDF39`), signed port `+0x14` in 1..65535
(`0xBCDF3B..0xBCDF47`), and nonempty path `+0x18` whose first byte
is `/` (`0xBCDF4F..0xBCDF63`). Content-type text at `+0x28`
and body logical length must agree on emptiness. Body length comes from
request stream `+0x2C`, slot `+0xC`, at
`0xBC13B4/0xBC13DC`; slBlob slot maps to `0x12CB9C0`.
These validators do not check method `+8`, scheme `+0xC`, or a literal
MIME value. Type 2 alone is not a rejection predicate.

Manager constructor `0xBC4190` clears both flag bytes at `0xBC4222`
(`66 C7 46 20 00 00`). Initializer `0xBC39B0` enables `+0x21`
at `0xBC3AB2` only after successful `WinHttpOpen`.
Global setup `0xBBD110` constructs at `0xBBD37B`, publishes at
`0xBBD38D`, initializes at `0xBBD393`, and succeeds only if
initialization succeeds. Normal app startup call `0x307208` takes a
failure branch on zero rather than continuing to mode construction.
Manager `+0x20` is set as drain-in-progress at `0xBC4673` in
`0xBC4620`, with observed caller application shutdown `0x3097B7`;
it is cleared after drain completion/timeout.

Neither flag establishes a normal-startup constructor rejection: enable is
already set after successful startup and drain begins clear. Validation
failure depends on actual request fields, not just auto-login invocation.
Malformed URL/body or shutdown-reentrant constructor reachability was not
established. The ignored submission result remains a precise code fact,
not a demonstrated startup failure.

## Tag 7 is permissive JSON-like text parsing, not binary framing

Raw `0xC259EB..0xC25A14` wraps the stream at value `+0x20`;
byte `+0x18!=0` enables extensions. Parser wrapper
`0xC257C5..0xC2581D` obtains stream length, appends a NUL if empty
or unterminated, then supplies adjusted length minus one to its parser.
There is no length prefix. It parses one value without checking exhaustion
of trailing input.

Dispatcher table raw `0xC256F8`, selector `0xC2570C`, handles
NUL as nil, either quote byte (`0x22/0x27`) as string,
`[` as array, `{` as object, and other input as scalar token.
Whitespace and commas (`0x20/0x09/0x0D/0x0A/0x2C`) are skipped;
commas are not strictly enforced. Object keys require colon and collections
use `]`/`}`; nesting limit is 32 (helpers `0x12E6250`,
`0x322F50`, `0x384040`).

String parser `0x12E6841..0x12E69DD` accepts both quote styles,
copies unescaped bytes without UTF-8 validation, and supports
`b/f/n/r/t/v`, octal-like escapes, `x` plus two hex digits, and `u`
plus four. Hex/Unicode escapes use UTF-8-style encoder `0x12CFEA0`;
surrogate-pair combination was not shown. Reaching NUL without a closing
quote is accepted by the inspected loop
(`0x12E6822..0x12E69F2`).

Bare tokens admit alphanumerics and `_ + - .`, with buffer at most 255
characters; exact `true/false/null` literals were verified in raw rdata.
Numeric-looking tokens become float32-rounded VM numbers only if absolute
converted value is strictly below 4,194,304
(`0xC2567F..0xC256C6`); larger values remain strings. Full-token
numeric validation was not established. Extension mode enables `0x...`,
`sdb`, and `mat` branches; some invalid/unresolved extension values yield
successful nil.

Normal return is one with a 16-byte VM value: nil tag 0, Boolean 1,
number 3, table `0x45`, and one-based integer array keys tag `0x13`.
EH metadata verifies exception handler `0xC25837`: it logs
`JSON error: %s`, resumes at `0xC25856`, and returns zero.
Caller `0xC25A3D..0xC25A4B` then appends nil; rollback was not
visible in this bounded trace and is the next explicit stack-contract
check. Empty input fails, whereas embedded NUL can select successful nil.
Malformed input is not uniformly failure, and failure-nil must remain
distinct from successful nil.

## Application shutdown drain pumps; timeout does not itself cancel

Raw `0x3097B7` passes 3000 ms to `0xBC4620`, ignores its Boolean
result, then calls `0xBBD570(3000)` at `0x3097C7`. This is application
shutdown, not GUI logout `0x63F5C0`.
Drain sets manager `+0x20=1` at `0xBC4673`, blocking new submissions.
After sampling `timeGetTime` at `0xBC467E`, each iteration aggregates
route pending count `+0x20` and workers with nonzero completion pointer
`+0x1C` (`0xBC46AD..0xBC46D8`).
Empty route tree (`0xBC46A1`) or aggregate zero (`0xBC4720`)
returns one and clears the drain flag at `0xBC476C`: accounting drained,
not proof that HTTP operations succeeded.

Nonzero aggregate calls `0xBC4728 -> 0xBBDB60`, reaching route pump
at `0xBBDD98`. It then calls IAT **VA** `0x1AC4188` at
`0xBC472D`; zero triggers `Sleep(1)` at `0xBC4736`. Import identity
of the former remains unresolved pending direct PE import parsing.
Negative timeout loops indefinitely (`0xBC4741`); otherwise unsigned
32-bit elapsed ticks must be strictly less than timeout to continue
(`0xBC474F`). Equality expires, returns zero, and clears the flag at
`0xBC475A`. There is no recount after pumping and before this deadline
test, so work finishing in that pump can still yield timeout return zero.

Drain itself neither dequeues/destroys requests nor closes HTTP handles
on exit. Normal worker completion follows the previously traced callbacks,
request destruction, response recycling, and context free.
On shutdown's final-release arm, `0xBBD602 -> 0xBC4280` destroys the
manager, then its global field is zeroed at `0xBBD60E`.
Manager destructor closes session handle at `0xBC42BB`. Route
destructor `0xBC3B80` tears down workers and dequeues pending requests,
calling indirect deleting-destructor slot zero at
`0xBC3C1A..0xBC3C29`, without visible ordinary completion callbacks.
Worker teardown marks active context `+0x11=1` at `0xBC28CB`,
closes connection `+0x20` at `0xBC28D7`, and destroys queued
requests/pooled responses.

Detached context/response destruction belongs to the previously traced
HANDLE_CLOSING callback. No direct close of completion request handle
`+0` was visible in this teardown scope; descendant-handle closure and
indirect destructor guarantees remain unresolved. Timeout therefore leaves
residual work to subsequent teardown, not an internal cancellation branch.

## Parser exception restores machine frame, not output-stack cursor

Raw `0xC257BE` saves machine ESP at `[ebp-0x10]`; this is distinct
from output cursor `[a1+8]`, advanced by 16 bytes by parser value writes.
Handler `0xC25837` logs and returns continuation **VA**
`0x1026456` at `0xC25850`, mapping to raw `0xC25856`.
There `xor eax,eax` produces status zero before common frame epilogue.
No output-cursor restoration was found in that handler.

Tag-7 caller `0xC25A14` consumes the EAX status, logs if greater than
one, and for nonpositive status appends a nil at the current output cursor
(`0xC25A3B..0xC25A49`). Positive status skips the fallback write.
Thus any values appended before an exception remain before fallback nil;
the nil is not rollback. The exact outer table insertion/stack effect of
a concrete partial parse remains the next bounded check. Machine-stack
exception unwinding must not be conflated with VM output-stack cleanup.

## Drain scheduling call is SwitchToThread, not a message pump

Direct PE import/OFT parsing resolves the previously unnamed drain call.
All three entries are named imports, not ordinals:

| DLL / import | Hint | OFT RVA / raw offset | IAT VA / raw offset |
| --- | --- | --- | --- |
| KERNEL32.dll / SwitchToThread | 1212 | `0x19BD1D4 / 0x19BBDD4` | `0x1AC4188 / 0x16C2D88` |
| KERNEL32.dll / Sleep | 1202 | `0x19BD3C0 / 0x19BBFC0` | `0x1AC4374 / 0x16C2F74` |
| WINMM.dll / timeGetTime | 148 | `0x19BDD70 / 0x19BC970` | `0x1AC4D24 / 0x16C3924` |

Raw `0xBC472D` (PE VA `0xFC532D`) has bytes
`FF 55 F0 85 C0 75 05 6A 01 FF 55 EC`: the saved SwitchToThread
pointer is called, its nonzero result skips Sleep, and zero calls Sleep(1).
This is thread scheduling, not application-message pumping. The distinct
preceding `0xBBDB60` call is the established route-pump path.

## Partial tag-7 failure can redirect the immediate table insertion

A concrete static path refines the previous "extra values plus fallback
nil" finding. Consider one payload entry with key `K`, tag-7 text bytes
`5B 22 5C 75 00` (array opening, quote, backslash, `u`, NUL), and no
event-text insertion (`a3=0`). This is a hypothetical byte fixture traced
through instructions, not an executed sample.

Array parsing pushes table `A` at raw `0xC252BE..0xC252CC`,
rejects the immediate closing-bracket probe at
`0xC252E0..0xC252ED`, then pushes index 1 and recurses at
`0xC252EF..0xC25313`. Quoted parsing consumes its opening quote
at `0x12E67DC..0x12E67E3`, finds insufficient bytes for the Unicode
escape at `0x12E6838..0x12E686D`, and constructs/throws at
`0x12E6AF4..0x12E6B3B`, before pushing a string value.
The exception bypasses normal array insertion and the two-slot removal
at `0xC25318..0xC25333`.

Let VM top before outer conversion be `T`; each slot is `0x10` bytes:

| Point | Stack suffix | Top |
| --- | --- | --- |
| Outer table push `0xC25E91..0xC25E9A` | `O` | `T+0x10` |
| Outer key push | `O,K` | `T+0x20` |
| Partial array push | `O,K,A` | `T+0x30` |
| Array index push | `O,K,A,1` | `T+0x40` |
| Throw/catch returning status zero | unchanged | `T+0x40` |
| Tag-7 fallback nil `0xC25A41..0xC25A4B` | `O,K,A,1,nil` | `T+0x50` |
| Outer insertion returns | `O,K,A` | `T+0x30` |

Outer `0xC25F15` supplies index -3. Resolver
`0xC27730..0xC27736` uses the current top, making `A` the target;
`0xC285CA..0xC285D6` selects integer key 1, and
`0xC285EF..0xC285F7` selects nil value. Cleanup at `0xC2862B`
pops exactly two slots. This performs `A[1]=nil`, not `O[K]=nil`
or `O[K]=A`; the outer key remains stranded.

With one entry and `a3=0`, local conversion returns success with three
slots `O,K,A` instead of one. Its epilogue and immediate wrapper
`0xC25FB0..0xC25FC8` do not restore VM top. This is bounded to the
established catch behavior and normal insertion return. Higher callback
argument construction/protected-call restoration is the next explicit
check; no observed runtime outcome or end-to-end vulnerability is claimed.

## Final network release destroys the manager before its timed pump

The bounded final-release trace corrects any implication that the
`BBD570(3000)` timeout gates manager destruction: the final-release arm
destroys/frees the manager and clears network owner `+0xA0` **before**
its timed tracking-vector pump. The worker corrected its initial label:
all code addresses in this trace are raw offsets. Instruction raw
`0x307200` encodes global operand VA `0x1DFFF70`, not a raw offset.

Network count is owner `+8`, reached through global **VA** `0x1DFFF70`.
First acquisition sets owner-thread ID `+0xBC` and count one.
Subsequent acquisition uses an ordinary non-atomic increment; acquisition
and release assert current-thread ID equals the owner rather than guarding
the count with a mutex. Zero count release returns; count greater than one
decrements and returns without teardown. At exactly one, teardown runs
while count remains one, then stores zero at the end. The release function
does not return a success/failure Boolean.

After manager destruction it pumps while tracking vector `+0xA4/+0xA8`
is nonempty, sleeping 10 ms between attempts while unsigned elapsed ticks
are at most the supplied 3000 ms. Expiration proceeds to cleanup, not
failure; pump/sleep duration is not a strict wall-clock bound.
The subsequent thread helper receives timeout zero, which it translates
to INFINITE for WaitForSingleObject. Thus 3000 ms does not bound the join.

Normal startup acquires once with flag one; another observed direct caller
acquires only when count was zero. This supports an ordinary single-ref
path, not an unconditional guarantee: app shutdown does not force/check
count one before release, and only the final-reference arm tears down.
The next task identifies tracking-vector elements and removal after manager
destruction.

Exact final-release sites: first acquisition sets owner ID at `0xBBD1BC`
and count one at `0xBBD1C2`; later acquisition increments at `0xBBD1AE`.
Release zero check is `0xBBD578`; decrement-only path
`0xBBD5AD..0xBBD5BA`; final zero store `0xBBD6E0`.
Manager destruction is `0xBBD602 -> 0xBC4280`, global-owner `+0xA0`
clear at `0xBBD60E`. Tracking pump call is `0xBBD642`, elapsed
comparison `0xBBD669`. Join `0xBBD670 -> 0x12BFF70(owner+0x48,0)`
maps zero to INFINITE at `0x12BFFC0`, calls WaitForSingleObject at
`0x12BFFC5`, and converges on handle/running cleanup at
`0x12C003B..0x12C0048`, including diagnostic paths.

## Enclosing callback restores top but does not repair partial-parse selection

The bounded higher-caller trace uses the stipulated one-entry fixture with
null event-text parameter (`a5=0`); the actual nonnull-event branch remains
the next check. Let entry VM top be `T0` and each slot occupy 16 bytes.
Lookup at raw `0xC3B46C` pushes handler `H` and nonnil result `F`.
Conversion at `0xC3B4C1` leaves `H,F,O,K,A` on the previously traced
failure path. Its Boolean success becomes argument count **one** at
`0xC3B4CD`, not the actual three-slot growth.

Protected call `0xC3B551` requests one result and uses saved-top-plus-one
as error-handler index (selecting `H`). Selection at `0xC289F1`
uses current top minus `16*(nargs+1)`, selecting `K` as the callable
with argument `A`, not intended `F(O)`.
On a returned success (`0xCD7370`) or protected failure
(`0xCD7890 -> 0xCD6B00`), top becomes K-slot plus 16,
leaving `H,F,O,result-or-error`. Cleanup
`0xC3B5BC -> 0xC278F3` resets top exactly to `T0`.

Thus returned-path cleanup bounds leftover live slots; it does not repair
the earlier callable selection. The runtime `__call` target for `K`
through `0xCD7080` remains unresolved; this trace does not establish
reaching native indirect `0xCD728C`, actual execution, or an end-to-end
vulnerability.

## Post-manager tracking vector owns socket handlers, not HTTP contexts

The network owner's `+0xA4/+0xA8/+0xAC` are pointer-vector
begin/end/capacity. Representative producer raw `0xBE1040` allocates
`0xA0` bytes, constructs via `0xBEC2E0`, and registers through
`0xBE10CE -> 0xBBD730`. Constructor logging identifies UDP with
label `SimpleSocketReplayReader`; the vector also includes Matrix game
UDP sockets. Append `0xBBD7E3 -> 0xBBF410` stores the pointer at
`0xBBF467` and advances end four bytes at `0xBBF469`.

Registration calls virtual `+8`, resolved to `0xBEC280`, storing network
back-pointer at socket `+0x3C`; it does not AddRef the socket.
Vtable **VA** `0x1C73D78`, raw `0x1872978`, slot `+0xC` resolves
to `0xBEC2B0`: nonnull `this` invokes deleting-destructor slot zero
with flag one. This is owning polymorphic deletion, not refcount release.

Reader destruction `0xBED1C0` clears socket owner `+0x40` and changes
open states to 4, requesting close through `0xBED0F0`.
Close completion `0xBED590` enqueues sentinel 1; pump virtual `+0x14`,
raw `0xBEDAA0`, consumes it, changes state 4 to 5, clears queues, and
returns false. Network pump destroys at `0xBBDD39`, compacts the vector
and decrements end at `0xBBDD55..0xBBDD5B`.

Post-manager pumping explicitly skips HTTP manager work if `+0xA0==0`
at `0xBBDD8E..0xBBDD98`, while socket handlers/network state remain.
This establishes structural separation, not complete callback-lifetime
safety: owner callbacks and virtual network close operations remain
indirect. Remaining-vector cleanup after timeout and ordering against the
infinite thread join are the next bounded questions.

## Actual event callback also redirects partial-parse insertion

The actual drain at raw `0xC38D38..0xC38D5C` calls
`0xC3B3C0(handler=record[8] or "", payload=entry[4], result=NULL,
eventtext=record[0] or "")`. Fallback is a verified NUL string;
event text is nonnull even when empty. It is forwarded into
`0xC25E30` at `0xC3B4B9..0xC3B4C1`.

On normal conversion the suffix is `H,F,O`, and reserved insertion
sets `O.event=eventtext`; the protected call selects `F(O)`.
On the established partial-parse fixture the suffix is `H,F,O,K,A`.
Reserved insertion pushes `"event"` and event text, then resolves
`-3` relative to current top; it sets **`A.event=eventtext`**, pops
those two slots, and leaves `H,F,O,K,A`. Conversion still reports true
and the caller supplies nargs one, selecting **`K(A)`**.

This path requires dispatcher enabled, VM/context present, nonempty handler,
nonnil callback lookup, and successful conversion stack reservation.
Protected returned paths still restore entry top through
`0xC3B5BC -> 0xC278F3`. The actual native target and runtime occurrence
remain unresolved; static wrong-selection bookkeeping alone is not an
end-to-end security claim.

## Residual socket deletion follows the worker join

Raw `0xBBD670 -> 0x12BFF70` first calls thread virtual `+8`,
resolved here to bare-return `0x2EB8C0`, sets network owner stop byte
`+0x4C=1`, and waits INFINITE. Returned join closes thread handle
`+0x54` and clears running byte `+0x4D`.
Only afterward does `0xBBD690..0xBBD6B0` invoke virtual `+0xC`
on each residual socket-vector entry; `0xBBD6B8` resets end to begin,
retaining capacity.

Representative deleting chain is `0xBEC2B0 -> slot0(1) ->
0xBEC440 -> 0xBEC480`. Call `0xBEC4FF -> 0xBE36B0` performs
`shutdown(socket,2)`, `closesocket`, and handle invalidation before
object cleanup/free. Then final release closes IOCP at `0xBBD6CA`
and calls WSACleanup at `0xBBD6DA`.

No explicit pre-join close-all was found in this path. Worker `0xBBDE6C`
can request socket virtual `+0x10(1)`, but universal pre-join closure is
unproven. A blocking-join conclusion cannot be inferred merely from that
ordering; the next task traces stop-byte reads, IOCP wait timeout, and wake
mechanisms.

## Extension dispatch: numeric handles and table lookup, format mapping pending

Raw `0xC25790` forwards its third-argument extension flag to
`0xC25190` at `0xC25822..0xC2582D`. Standard true/false/null
handling precedes the extension gate; false flag skips these branches.
The bounded extension report establishes:

- `0xC2548D..0xC254EE` tests case-sensitive prefix `0x`, scans one
  64-bit value initialized to zero, logs on scan failure but continues.
  `0xC549A0` yields userdata VM tag 71 for nonzero or nil tag 0 for zero.
- `0xC254F3..0xC25616` matches whole token `sdb`, then scans its next
  lexeme into a 128-byte text buffer and DWORD, requiring two conversions.
  Failed conversion or unknown table key in a 52-entry lookup yields nil
  (`0xC2557C..0xC25597`). A normal nonzero result yields userdata;
  zero yields nil. Key index 14 uses a separate string/node path.
- `0xC25621..0xC256D0` matches whole token `mat` and passes its next
  lexeme through `0xC27FE0`.

The worker read operands `0x1BBACAC/0x1BBACD0` as nontext without
establishing their raw mapping. Their format strings and the `mat`
helper interpretation remain withheld pending a focused VA-to-raw check;
the latter report conflicts with the earlier NUL-string-push identification.
Do not treat this partial report as exact extension delimiters or confirmed
material-object semantics.

## Extension formats resolved; mat is text, not a material wrapper

Direct PE mapping corrects the unresolved reads above. Image base is
`0x400000`, `.rdata` RVA/raw `0x16C4000/0x16C2C00`.
Format operand **VA** `0x1BBACAC` maps to raw `0x17B98AC`,
bytes `%127[^:]:%u\0`; **VA** `0x1BBACD0` maps to raw
`0x17B98D0`, bytes `0x%llx\0`. Both parser calls target imported
cdecl `sscanf`. Reading the operands as raw offsets caused the earlier
nontext result; it was not missing/dynamically decoded format data.

Case-sensitive `0x` scans a zero-initialized uint64, logs scan failure but
continues, and passes the result to `0xC549A0` for nil or userdata
tag `0x47`. Exact `sdb` consumes another lexeme, scans a table name
up to 127 bytes before colon and an unsigned ID, and requires two
conversions. A form such as `sdb "ItemType:123"` is needed because
the unquoted token alphabet excludes colon. The 52-name lookup is
case-sensitive; index 14 is `LocalizedString`, whose special output path
is the next bounded trace.

Exact `mat` consumes another lexeme and pushes its text through
`0xC27FE0` (ECX=VM, EDX=C string). That helper produces string tags
`0x44/0x54`, or nil for null. The previous material-derived-object
interpretation is superseded; no material lookup was established.

## IOCP worker observes stop through finite dequeue passes

Let network owner be `N`, embedded PumpThread `T=N+0x48`.
Join writes `T+4=N+0x4C`; worker reads the same byte at raw
`0xBBC878` and `0xBBC9CE`. Thread handle is `T+0xC=N+0x54`,
running byte `T+5=N+0x4D`.
Dequeue at `0xBBC911` calls GetQueuedCompletionStatus through
IAT **VA** `0x1AC432C`, raw `0x16C2F2C`, using IOCP from
`[global VA 0x1DFFF70]+0xDC`.

Each outer pass sets `EBX=500` at `0xBBC8B6` and initial timeout zero
at `0xBBC8DB` (pushed `0xBBC8F0`). A failed dequeue with null
OVERLAPPED enters `0xBBC949`: first occurrence schedules 500 ms and
clears EBX; another such failure exits the inner pass. Any dispatched
completion resets next timeout to zero at `0xBBC9BB`.
Inner passes cap at 100 dequeue attempts (`0xBBC9C1..0xBBC9C5`),
then reread stop at `0xBBC9CE`. Thus at most one 500-ms dequeue wait
occurs per pass, not 100 times 500 ms; a quiet worker needs no external
completion to observe stop.

PumpThread vtable **VA** `0x1C734D4`, slot `+8`, contains
**VA** `0x6EC4C0`, raw `0x2EB8C0`, a bare return. Join calls it
before setting stop (`0x12BFF99..0x12BFF9F`); it posts nothing.
A real post at `0xBBDA03..0xBBDA10`, after socket closure in
`0xBBD940`, sends `(port,0,key,NULL)` and dispatches key virtual
`+0x1C`: a close notification, not stop sentinel.

Finite waits establish a path to stop observation, not a universal bound
on the infinite join: maintenance takes critical section `N+0x6C` and
calls socket virtual `+0x10(1)` at `0xBBDE6C`; completion callbacks
can also delay progress. Intervening work returning and scheduling/visibility
remain assumptions. The next trace follows keyed-close ownership and
failed-post behavior, rather than claiming deadlock from the join alone.

## LocalizedString extension returns text through unresolved service slots

Raw `0xC255B5..0xC25603` handles lookup index 14 through singleton
virtual `+0xD0`, then returned-object virtual
`+0x108(this,&temporary,uint32_id)`. Concrete targets remain unresolved.
The result object's first DWORD supplies a NUL-terminated byte string;
null selects verified empty string **VA** `0x1C5EA58`, raw
`0x185D658`. `0xC27FE0 -> 0xCD5020` interns through strlen/strcmp:
the output is text, not userdata, but its Unicode encoding is unproven.
Scan failure/unknown key yields nil at `0xC25608` while reporting parse
success. Ordinary SDB references produce subtype-2 userdata; the separate
`0xC549A0` handle path produces subtype 1. Zero handles yield nil.
Missing-row/localization fallback semantics require resolving service slots.

## Keyed close notification borrows the vector-owned socket pointer

Reader RTTI is `nl::SimpleSocketReplayReader`; destructor callsites
raw `0xBE1214/0xBE12A4` load socket from reader `+0xC` and call
`0xBED1C0`. That helper locks socket `+0x88`, clears owner `+0x40`,
and branches on state `+0x54`: states 1/2 become 4 at `0xBED241`
and request close; state 3 becomes 5 and drains through `0xBEC5B0`
(diagnosing a noninvalid handle); state 0 becomes 5 directly. Other states
reach diagnostic `0xBED29B`, without a retry branch.

Close `0xBED0F0` acts only if socket byte `+0x81==0`; it sets the
byte **before** calling network virtual `+8(socket,&handle)` at
`0xBED16F`. It diagnoses missing network back-pointer `+0x3C` and
handle remaining valid afterward. The network target `0xBBD940`
diagnoses invalid inputs/port, closes the handle, stores -1, then posts
`(port,0,socket,NULL)` at `0xBBDA10`.
The key is the socket pointer with no extra AddRef; lifetime comes from
the owning network vector.

Successful dequeue invokes socket slot `+0x1C` at `0xBBC9B8`.
ManagedUDP vtable **VA** `0x1C73D78` resolves it to `0xBED590`,
which diagnoses valid handle or already-set byte `+0x80`, sets that
byte, and enqueues sentinel 1. `0xBEDAA0` consumes 1, changes
state 4 to 5, drains, and returns false; vector pump deletes at
`0xBBDD39` through `0xBEC2B0 -> slot0(0xBEC440,1) ->
0xBEC480 -> free`.

Post return BOOL propagates out of `0xBBD940`, but `0xBED0F0`
ignores it. No result-based diagnostic, retry, rollback, reference
adjustment, or fallback was found in that local path; the pre-post port
diagnostic is not failure handling. Without another event, state 4 remains
retained and `+0x81` suppresses another close/post. Alternative maintenance
or pump transitions are the next bounded check; diagnostic termination and
global absence of recovery remain unresolved.

## ManagedUDP maintenance does not resolve closing state in inspected paths

Vtable raw `0x1872978` resolves slot `+0x10` to `0xBED960`
and `+0x14` to `0xBEDAA0`. Network maintenance passes one at
`0xBBDE6A..0xBBDE6C`; that argument immediately skips work at
`0xBED98E..0xBED992`. Argument zero processes only states 1/2
(`0xBED9B6..0xBED9C1`), excluding closing state 4.

In the socket pump, empty queue with state 4 returns true at
`0xBEDC84..0xBEDC93`, retaining the handler. Ordinary packets skip
delivery outside states 1/2. Sentinel 2 calls close helper `0xBED0F0`,
whose already-set `+0x81` suppresses another post
(`0xBED135..0xBED142`). Sentinel 1 diagnoses a noninvalid handle,
then changes 4 to 5 (`0xBEDBF8..0xBEDC20`).
Neither inspected maintenance nor empty-queue branch supplies independent
timeout/fallback completion after failed posting. Other sentinel producers
remain the next bounded question, not a basis for a universal no-recovery
claim.

## Localization accessor is uiManager +0x974

The parser singleton is the established uiManager global **VA**
`0x1E002B8`, not a separate unknown singleton. Its vtable
**VA** `0x1C73EBC`, slot `+0xD0` at raw `0x1872B8C`,
contains **VA** `0x1024560`, raw `0xC23960`.
That function returns `[this+0x974]` with a null diagnostic guard.
The bounded trace did not resolve assignment/type of `+0x974`,
service slot `+0x108`, or missing-ID behavior. Follow-on scope is those
field stores, not repeating the already verified singleton bootstrap.

## ManagedUDP sentinel producers narrowed

The queue at socket `+8` uses raw helper `0x12CE310`.
Close callback `0xBED590` checks handle `+0x44==-1` and flag
`+0x80==0` diagnostically, sets the flag, and enqueues 1 at
`0xBED5E2`. Error callback `0xBED5F0` optionally logs a nonzero
error then unconditionally enqueues 2 at `0xBED66D`; receive-error
dispatch uses virtual `+0x20`. Ordinary receive allocations enqueue at
`0xBED839`.

Forced state-5 stores `0xBED255/0xBED281` in `0xBED1C0` lie in a
switch restricted to incoming states 0..3, not a state-4 fallback.
No additional sentinel-1 producer was found in the inspected references;
indirect coverage is incomplete. This narrows the no-fallback result
without converting it into a global absence claim. Next work follows
ManagedUDP's distinct receive/error path rather than extrapolating
GameSocket semantics.

## Localization field assignment and service allocation

Raw `0xBFB51A` (PE VA `0xFFC11A`) stores `ESI` into
`[EDI+0x974]`, sourced from uiManager initializer's third argument.
Caller `0x30B1EE` pushes service global **VA** `0x1DFFFC0`;
`0x30B200` calls uiManager slot `+0x1C`.
Service creation requests `0xA5C` bytes at `0x310C76`, invokes
constructor raw `0x3DA9D0` (VA `0x7DB5D0`) at `0x310C93`,
and stores its result into wrapper `+0x10` at `0x310C9C`, backing
that global.

Concrete service vptr and slot `+0x108` remain unresolved. The next
check is constructor `0x3DA9D0`'s vptr stores and any this adjustment,
not uiManager bootstrap again.

## ManagedUDP error suppression differs by failure layer

The active-sample instruction trace separates these cases:

| Failure layer / raw range | Observed handling |
| --- | --- |
| IOCP dequeue with nonnull OVERLAPPED, `0xBBC973..0xBBC983` | Error 995 skips callback entirely; all others, including 996 and 10038, reach slot `+0x20` unchanged |
| Receive WSAGetOverlappedResult(FALSE), `0xBED738..0xBED766` | Error 996 returns without callback/repost; 995/10038 become zero before slot `+0x20`; others pass unchanged |
| ManagedUDP slot `+0x20`, `0xBED5F0` | Nonzero logs, zero suppresses logging only; both enqueue sentinel 2 at `0xBED66D` |

Socket `+0x98` owns persistent `0xA0`-byte receive operation with
zero-initialized first `0x14` bytes OVERLAPPED, sockaddr at operation
`+0x18`, and address length `+0x98`, reset to `0x80`.
Socket `+0x50` is receive buffer and `+0x4C` its capacity, default
`0x4000`. Receive `0xBED690` locks socket `+0x88` and skips
everything if closing byte `+0x81` is set.

Only successful result retrieval reaches WSARecvFrom repost at
`0xBED8F5`, reusing the operation/buffer even for zero-byte success.
Nonempty data first enqueues a separate `0x38`-byte packet at
`0xBED839`; its payload ownership is the next check before assuming
independence from the reused receive buffer. Neither error layer frees or
replaces persistent receive storage; destructor `0xBEC480` frees it.
Socket `+0x48` is a distinct retained send item, not this operation.
These are ManagedUDP-specific findings, not inherited GameSocket behavior.

## Localization service slot +0x108 resolves to 0x3E0620

Constructor raw `0x3DA9D0` stores its primary vptr at `0x3DAA03`:
**VA** `0x1C63014`, raw `0x1861C14`. Slot `+0x108` at raw
`0x1861D1C` / **VA** `0x1C6311C` points directly to raw
`0x3E0620`, **VA** `0x7E1220`, not a this-adjusting thunk.
Constructor returns its original pointer.

Target `0x3E0620` calls provider global **VA** `0x1E00100`,
virtual `+0(out,id,0)`, at raw `0x3E065C`.
Null returned text selects empty **VA** `0x1C5EA68`, raw
`0x185D668`. This fallback is distinct from the parser's outer empty
string address. Missing-ID-to-null behavior remains unproven until provider
dispatch is resolved; the next scope is that global's assignment/slot zero,
not the already resolved service/bootstrap chain.

## ManagedUDP queued packets snapshot receive bytes and lend the blob

Raw `0xBED796..0xBED839` allocates `0x38`-byte packet records:

| Offset | Observed field |
| --- | --- |
| `+0` | 64-bit timestamp |
| `+8` | Endpoint: flags `+8`, IPv4 `+0xC`, IPv6 `+0x10..+0x1F`, port `+0x20` |
| `+0x28` | Blob vptr |
| `+0x2C/+0x30` | Backing descriptor / ownership control |
| `+0x34` | Alignment, initially zero |

`0xBED7E6` calls blob slot `+0x1C(0)`; `0xBED7F6` calls slot
`+0x10(socket.recvBuffer,receivedLength)`, target `0x12CB9D0`.
That helper resizes fresh backing and copies exactly receivedLength bytes
to descriptor data at `0x12CB9F0`. This is a byte snapshot, not an alias
of the persistent receive buffer. Queue `0xBED839 -> 0x12CE310`
stores the packet pointer without cloning/AddRef; only then does
`0xBED8F5` rearm reception.

Dequeue `0xBEDAF9 -> 0x12CE400` recycles the queue node and returns
the packet. States 1/2 deliver through owner slot `+4` at
`0xBEDB3C..0xBEDB4E` with borrowed blob pointer, endpoint pointer,
and timestamp low/high. Reader construction installs reader `+8` as owner
at `0xBE110F`; interface vtable **VA** `0x1C73C88`, slot `+4`,
resolves to `0xBE1450`. Matching endpoint forwards the borrowed blob
synchronously to `0xBE0110`; mismatches only log. Retention beyond that
call is the next explicit check.

After callback, or immediately outside states 1/2,
`0xBEDB51..0xBEDB86` decrements packet control strong `+4`;
zero calls control slot zero, then weak `+8` is decremented and slot
`+4` called on zero. Packet storage is freed afterward.
Queue drain `0xBEC674..0xBEC6BC` applies the same release/free without
callbacks. Owner `+0x40` is assertion-checked, not safely null-guarded;
delivery has no explicit surrounding lock in this inspected path.
Concurrent shutdown/callback safety remains unresolved rather than inferred
from the assertion.

## Localization provider is tfGlobalizationManager

Provider global **VA** `0x1E00100`, raw `0x19FE500`, is singleton
wrapper **VA** `0x1E000F0` plus `0x10`.
Assignment raw `0x312799` / VA `0x713399` stores the constructed
`0x24`-byte provider through wrapper `+0x10`.
RTTI identifies `tfGlobalizationManager`; constructor raw `0x38FC50`,
VA `0x790850`, writes vptr at `0x38FC58`:
**VA** `0x1C629B8`, raw `0x18615B8`.

Slot zero points to **VA** `0x7912F0`, raw `0x3906F0`, leading
through `0x3909C0` to `0x6A8AB0`.
ID zero bypasses lookup with initial verified empty text.
Raw `0x6A8B3C` tests null lookup output, and
`0x6A8B5E..0x6A8B6D` checks selector/AL before capacity handling.
Lookup `0x6A8BE0`, actual missing-ID outcome, and language/fallback
selection remain unresolved. The next scope is that lookup's internals,
not the now-complete provider bootstrap.

## ReplayReader discriminator 1 copies into reader-owned blob storage

Raw `0xBE1508 -> 0xBE0110` enters the parser synchronously.
At `0xBE0140..0xBE0192`, packet bytes become a stack-local borrowed
cursor and the discriminator is consumed. The discriminator-1 branch
`0xBE01F9..0xBE023F` selects a reader-owned blob through `0xBDFA90`
(container at reader `+0xC`), grows it, gets writable data plus old
length, and copies remaining packet bytes. Explicit memcpy occurs at
`0x336B72..0x336B79` through `0x159D8F4`.

Destination helper `0x12CBCB0` reuses suitable unique backing or allocates
and installs new backing at `0x12CBE07/0x12CBE15`.
No packet-control increment or persistent packet-pointer store was found
on this path. Its stored bytes therefore survive independently of immediate
packet release. Other discriminators, their length checks, and their
ownership behavior remain separate follow-on scope.

## Localization ID remap, language fields, and English fallback

All code addresses below are raw offsets; table addresses are explicitly
PE VAs in runtime storage, not file-backed strings.
Lookup `0x6A6B00` uses unsigned 32-bit ID against records at **VA**
`0x1E55E48`, count **VA** `0x1E55E4C`, stride `0x1C`, key
at record `+0x18`. Lower-bound helper `0x6AA040` compares unsigned;
`0x6A6B60..0x6A6B69` rejects end/nonmatching keys.

Optional remap `0x6A8BEE..0x6A8C1F` runs only when the flag's low
byte is zero, using pointer/count **VA** `0x1E55DD8/0x1E55DDC`,
stride 8, original ID `+4`, replacement ID `+0`. No match preserves
the original ID.
Manager `+4` supplies language selector at `0x3909D9`; byte-verified
language table **VA** `0x1DE0868` establishes:

| Selector | Language | Text pointer offset in record |
| --- | --- | --- |
| 0 | en | `+0x10` |
| 1 | ko | `+0xC` |
| 2 | de | `+0` |
| 3 | fr | `+4` |
| 4 | zh | `+0x14` |
| 5 | ru | `+8` |

Null/empty selected text falls back to English `+0x10` at
`0x6A8C6F..0x6A8C84`; out-of-range selectors directly select English.
Missing ID sets `*out=0`, returns `AL=0`. Existing record returns
`AL=1` even if English fallback is null/empty, so result existence and
nonempty translated text are distinct.
Actual IDs/text depend on loaded runtime data. Producer/loading, remap
ordering, and table ownership are the next bounded questions.

## ReplayReader outer discriminator and truncation ledger

Raw `0xBE018C` reads one unsigned discriminator byte after shortage
check `0xBE0170..0xBE0180`; cursor advances at
`0xBE018E..0xBE0192` and remaining length decrements at
`0xBE01C0..0xBE01C3`. Comparisons `0xBE01C6..0xBE01D0`
recognize only 0 and 1. Let supplied byte length be N.

| Discriminator | Raw branch | Reads / effects | Successful consumption |
| --- | --- | --- | --- |
| 0 | `0xBE0258` | Copy 24 bytes into `this+0xB0..0xC7` via `0x336B50`; `0x12EE170` reads LE16 L and supplies L bytes to embedded destination `this+0xC8`, slot `+0x10` | `27+L`; trailing bytes ignored here |
| 1 | `0xBE01F9` | `0xBDFA90` selects reader-owned blob with argument N-1; nonnull destination follows established append/copy at `0xBE023F` | N; null result logs and consumes only discriminator |
| 2..255 | `0xBE01D2` | Log unsigned discriminator at `0xBE01DD`, return through `0xBE01E5` | 1; no further owner-field writes observed |

Borrowed cursor shortage slot at raw `0x1860D2C` resolves to
`0x3570F0 -> 0x159E27A -> _CxxThrowException`: shortage is not
a clean unknown-case return. Fixed 24-byte check
`0x336B5D..0x336B70`, missing LE16 check
`0x12EE17E..0x12EE18F`, and missing L-byte payload check
`0x12EE1CD..0x12EE1DA` invoke it before their respective reads/call.
Case 0 is not transactional: its fixed 24-byte write precedes later
length/payload checks. L zero skips destination invocation at
`0x12EE1C8..0x12EE1CB`, leaving prior destination bytes unchanged.

After case-0 success, nonzero `this+0xD8` is cleared at `0xBE0288`;
the 24 bytes propagate to `[[this+4]+0x90..+0xA7]` and owner
byte `+0x8E=1` at `0xBE02C3..0xBE02E4`.
Zero `+0xD8` skips propagation. Destination type and enable-state
meaning are the next bounded check; field semantics remain unassigned.

## Localization metadata initializer found; row source/order unresolved

Raw `0x32C070` resolves fields from a generic DWORD-pointer object
through hash lookup `0x1310670`, then stores table owner/data/count.
Remap metadata assignment `0x32C191..0x32C1A0` uses hash
`0xD1ACBFD5`; primary metadata assignment
`0x32C462..0x32C481` uses hash `0x97DFAFF9`.
This is evidence of generic object-backed metadata binding, not a proven
SDB-specific row loader.

Consumer `0x669090` uses primary count/stride `0x1C`, performs key
lookup, and copies selected text. It is not the producer. Small assignment
helpers `0x7403D0/0x740450` have no direct xrefs in the bounded IDA
search. Row origins, copying, sort validation, and owner-retain/release
remain unresolved; lower-bound lookup alone does not establish that a
loader enforces ordering. Next work verifies exact owner fields and their
replacement/lifetime mechanism.

## Replay case-0 destination replaces bytes; request arms propagation

Raw constructor `0xBDFBC0` installs slBlob vptr **VA**
`0x1C80CC4` at reader `+0xC8`, zeros `+0xCC..+0xD7`,
and clears `+0xD8`. Slot `+0x10`, **VA** `0x16CC5D0`
(raw `0x12CB9D0`), resizes to L then copies at offset zero:
case-0 payload handling is replacement, not append. Destructor
`0xBDFCB0` releases its reference-counted backing.

Entry `0xBDFD80` arms `+0xD8` before forwarding through
`[this+8]` slot `+8`; case-0 success clears it before publishing to
the owner. Concrete forwarded operation/caller is the next scope.
The fixed `+0xB0..+0xC7` remains an opaque 24-byte record.
Zero-length destination preservation and nontransactional earlier writes
remain as previously established.
