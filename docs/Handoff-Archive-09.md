# Reverse-engineering handoff archive 09

## Dump-key action reaches TerminateProcess if assertion reporting returns

Immutable PE parsing resolves IAT **VA** `0x1AC43A4` to
**KERNEL32.dll!TerminateProcess**. The worker passes pseudo-handle -1
and exit code -2 after the assertion-reporting call.
The fourth reporter argument **VA** `0x1C5E950` is
`"Dump key pressed"`; its second argument **VA** `0x1C5EA58`
points to zero-initialized data in the inspected mapping.
This identifies a termination request, not a proven dump-file write.
The continuable reporter can divert control; neither reaching the
TerminateProcess call nor returning from it is unconditional.

The worker's first virtual action, global **VA** `0x1E002B8` slot `+0xFC`
with argument 4, resolves to **VA** `0x1006BF0`, code raw `0xC05FF0`.
That wrapper conditionally forwards to a list-logging routine; the
list identity, meaning of argument 4, and concrete output are still open.
The table/worker mapping is confirmed: **VA** `0x6EAE00` maps to
raw `0x2EA200`. State byte `+0x70` allows repeated action on every
continuing matching iteration after the first, not merely a rising edge.

## Standalone native CRT return selects exit(status)

The sample's CLR directory is `(RVA=0, size=0)`. CRT exit-mode word
**VA** `0x1E36618` is file-backed at raw `0x1A34A18`, initially zero.
The bounded reference pass found its comparison but no in-image writer.
At raw `0x136AEE4`, bytes
`83 3D 18 66 E3 01 00 75 60 50 FF 15 28 48 AC 01`
select the fall-through `exit(EAX)` call for zero.
This supports the normal standalone native path, not an unconditional
claim against external modification.

Word **VA** `0x1E36610` is also file-backed and initially zero.
The observed write at raw `0x136AE4B` sets it to one only when
CRT initialization state is already nonzero and not one.
That state starts in the loader-zero-filled tail; first entry does not
take that write. Nonzero mode still follows the separately recorded
conditional `_cexit`/local-return branch.

## Dump-key logging wrapper traverses a diagnostic list, not a dump file

The supplied virtual target raw `0xC05FF0` conditionally forwards only if
`this+0xA00` is nonnull, calling raw `0xC294A0` with
`(4, *(this+0xA00+0x78))`. The latter walks a sentinel-terminated list:
head `+0x10`, sentinel `+0x30`, node next link `+8`.
Formatter raw `0xCCFB20` converts entries. Leading formatted entries
starting with `'C'` are skipped; after the first non-`'C'` entry,
subsequent entries are sent to the VIBE logger.
The list's semantic type and producer remain open.

Logging wrapper raw `0xA69E00` emits only when its numeric level is
`>= logger[4]`; argument 4 is this level, not a worker status.
Emitter raw `0x12C2030` dispatches through the configured callback
referenced by logger `+0x14`, passing context from `+8`.
This chain establishes formatted diagnostic logging, not dump-file output.
The follow-up relied on the previously supplied virtual-target resolution;
empty IDA data xrefs neither independently confirm nor disprove that slot.

## Application shutdown entry resolved; console close ordering remains open

Call raw `0x2EB1F9` has bytes `E8 D2 E3 01 00`, resolving to
ClientApplicationShutdown raw `0x3095D0`.
The inspected body drains queued objects before subsystem cleanup.
Concrete edges include call raw `0x309678 -> 0x30D150`,
virtual slots `+0x10/+0x14` through global **VA** `0x1E002B8`,
and call raw `0x309804 -> 0x10D8E30`; it returns at `0x3098BA`.

No direct console-object reference or callback-slot restoration was found
in that body. This does not establish transitive absence through its callees.
Console close raw `0xE5EEE0` itself does not restore callback slot
**VA** `0x1DF96F8`, but its actual shutdown caller and ordering relative
to application return remain unresolved. Consequently a blanket claim that
the inspected shutdown leaves the callback unchanged after file close would
overstate the evidence. The successor follows the concrete service teardown
edges rather than repeating direct-reference scans.

## Diagnostic list payload is tagged; virtual binding needs table-byte verification

In raw `0xC294A0`, the current node is saved at `[ebp-0x10]`;
formatter raw `0xCCFB20` reaches that local through its output-buffer
argument plus `0x60`. The node's first field points to a 16-byte tagged
value. For `(value+8) & 0x1F == 6`, it passes the first DWORD to
raw `0xCCF8C0`; otherwise that argument is zero. This representation
does not identify the semantic meaning of tag 6 or prove UI-event storage.
Raw `0xC293D0` manipulates 16-byte values with low-nibble tags 3 and 4,
normalizes tag-3 values via `0xCD6550`, and calls `0xC294A0(5, context)`.
The insertion path remains untraced.

The follow-up did not verify that uiManager's vptr **VA** `0x1C73EBC`
has `0xC05FF0` at slot `+0xFC`. Earlier statements resolving that virtual
edge are therefore provisional until exact table bytes are checked;
the independently inspected `0xC05FF0 -> 0xC294A0` chain remains valid
conditional on dispatch to that wrapper.
Formatter constants **VA** `0x1BBAD6C/0x1BBAEC4` also require
PE-aware string mapping before declaring them unreadable. The successor
checks those two concrete mapping-sensitive points.

## UI traceback virtual binding and format strings confirmed

PE mapping places uiManager vptr **VA** `0x1C73EBC` at raw `0x1872ABC`.
Slot `+0xFC`, **VA** `0x1C73FB8`, raw `0x1872BB8`, contains
`F0 6B 00 01`, namely **VA** `0x1006BF0`, code raw `0xC05FF0`.
Initializer store raw `0xBF7ABB` is `C7 03 BC 3E C7 01`;
another store occurs at `0xBF8AED`. This closes the provisional virtual edge.

Mapped format **VA** `0x1BBAEC4` is `"    at %s(%d): %s"`;
formatter control string **VA** `0x1BBAD6C` is `"lnS"`.
Together with the traversal this establishes stack-frame/traceback-style
logging. The earlier unreadable-string result arose from failing to map
VA constants before inspection. A nearby Lua 5.3 marker alone does not
identify the node type; exact frame enumeration/field semantics remain open.

## Shutdown service-cell slot +4 reaches console fclose before service return

Helper raw `0x30D150` tail-calls uiManager virtual `+0x34`;
shutdown's subsequent calls are slots `+0x10/+0x14` at
`0x309685/0x309690`. At `0x30979A`, service virtual `+0x64(0)`
resolves to raw `0xE4FB00`, storing zero into service `+0xB4`.
This is not a store to the separate category callback slot.

The aliases carry the address of the service pointer cell, not the static owner.
Raw `0x2D2D60..0x2D2D65` copies the DWORD at **VA** `0x1DFC82C`
(backed raw `0x19FAC2C`, bytes `70 04 E0 01`, value **VA**
`0x1E00470`) to alias **VA** `0x1E60E3C`; raw `0x2D2D70..0x2D2D75`
copies that DWORD to alias **VA** `0x1E60EA4`. The latter is in the
virtual-only data tail, so its value is established by these copy instructions,
not by reading an unrelated raw offset.

The exact shutdown operand sequence at raw `0x10D8F6A..0x10D8F73` is
`A1 A4 0E E6 01 8B 08 8B 01 FF 50 04`: load the alias value (the address
of cell `0x1E00470`), load the service object from that cell into ECX, load
its vptr, then call slot byte-offset `+4` with the service object as receiver.
This is not a call on the static owner at **VA** `0x1E00460`.

The service constructor raw `0xE4DFA0` installs object vptr **VA**
`0x1C77C2C`; the owner itself has a different vptr, **VA** `0x1C77C1C`.
With the `.rdata` mapping, the service vtable's slot `+4` is at raw
`0x1876830`; exact bytes `F0 5F 25 01` encode target **VA** `0x1255FF0`,
which maps to code raw `0xE553F0`. That method's normal path calls the
console object's virtual slot `+8` at raw `0xE55541`. Console vptr **VA**
`0x1C77EA8` has slot `+8` at raw `0x1876AB0`, bytes `20 FB 25 01`,
target **VA** `0x125FB20` / raw `0xE5EF20`.

At raw `0xE5EF68`, that method loads its receiver's vptr and calls slot
`+0xD0`. Console vtable raw `0x1876B78` contains bytes `E0 FA 25 01`,
target **VA** `0x125FAE0` / raw `0xE5EEE0`. The close body conditionally
passes the two global FILE handles **VA** `0x1E49A18` and `0x1E49A14`
to `fclose` through IAT **VA** `0x1AC49A8` (raw `0x16C35A8`), then
zeros each corresponding global. The handle closes are conditional on each
pointer being nonnull.

Thus the synchronous order is app shutdown raw `0x309804` calling
`0x10D8E30`, service slot `+4` -> `0xE553F0`, console slot `+8` ->
`0xE5EF20`, then console slot `+0xD0` -> `0xE5EEE0`/`fclose`.
This occurs before the service method returns and before app shutdown's later
generic nested-container drain call raw `0x309883 -> 0x12DB320`; bootstrap
cleanup then continues past shutdown and returns at `0x2EB2FF`.
`0xE5EF20` continues after the close call, so this proves the close operation's
position in the chain, not that the FILE globals remain null after every later
operation. The separate service `+0x64` clearing `+0xB4` is not restoration of
category callback slot **VA** `0x1DF96F8`.

## Traceback enumerates VM frames and requests line/source/name metadata

The raw `0xC294A0` traversal is a VM-context frame walk, not an OS stack
walk. For control string `"lnS"`, formatter raw `0xCCFB20` dispatches
`l` to raw `0xCCF500`: it computes a line-table index from node PC `+0x14`
relative to prototype code `+0x30`, then reads prototype line table `+0x38`.
`S` calls raw `0xCCF840` for source metadata from the value's prototype;
`n` calls raw `0xCCFF50` under node-flag conditions.

Correction to the earlier formatter summary: raw `0xCCF8C0` is gated
by uppercase `L`, absent from `"lnS"`. Appending the value is separately
gated by `f`, also absent here. Thus the generic tag-6/helper path is not
evidence that this traceback requests it.
The leading-`C` filter examines the local metadata field returned by this
formatting path; later `C` entries are emitted after the first non-`C`.
Exact field layout and frame insertion/removal remain separate questions.
The concrete metadata operations support active script-frame interpretation,
but do not by themselves establish Lua version or complete frame lifetime.

## Service deinitialization includes a conditional post-close logging attempt

Raw `0xE553F0..0xE55571` begins with logger helper `0x322C40`
at `0xE553FE`, console `+0xBC` at `0xE55410`, conditional console
`+0xEC` at `0xE5543E`, and entry `+0x7C` callbacks at
`0xE55444..0xE55480`. Registry/list cleanup includes `0xE54AB0`
and `0xDE12B0`, followed by console `+0x14` at `0xE55534`
and console `+8` at `0xE55541`.

Console deinit raw `0xE5EF20` closes/nulls the two FILE globals through
`+0xD0` at `0xE5EF68`, then calls console `+0xC` at `0xE5EF78`.
Later path processing invokes IAT **VA** `0x1AC471C` and conditionally
reports a result neither zero nor -1 (`0xE5F4A9..0xE5F511`).
Its import identity is pending; this is not evidence of reopening a log.
It clears byte `this+4` at `0xE5F597`.

After console deinit returns, service shutdown calls `0xE6F7C0`
at `0xE55546`, console `+0xC0` at `0xE55555`, then `0x322C40`
at `0xE55565`. That helper conditionally emits through console `+0xB0`
(raw `0xE5AB80`) with severity 2 when object/threshold checks pass.
Emitter `0x12C2030` uses the current category callback.
This proves an attempted post-close logging path, not unconditional output.
The sink's file writer is conditional on a FILE being nonnull.

An unclassified reference to callback slot **VA** `0x1DF96F8`
at raw `0x12C1A4D` is the next concrete check; do not conclude that
the four known setter sites exhaust possible callback changes.

## VM frame traversal verified; producer/pop remains a bounded stop

Raw `0xC05FF3/0xC05FFC/0xC06005` gates/loads the manager's `+0xA00`
object and its `+0x78` context. Raw `0xC294C6` starts from context
`+0x10`, `0xC294CB..0xC294E5` tests context `+0x30` sentinel,
and `0xC294D4` advances through node `+8`.
No insertion/pop or sentinel initialization was established by the bounded
follow-up. Therefore current-to-parent interpretation remains conditional
on the head being the active frame. Further repeated producer scans are
parked; the next task addresses concrete formatter fields and special cases.

## Callback reference is slot indirection; deinit path unlinks rather than reopens

Raw `0x12C1A4D` is inside the immediate of instruction `0x12C1A4A`,
`mov [ebx+0x14], offset 0x1DF96F8`, in constructor raw `0x12C1A20`.
It stores the address of the callback slot in a category object, not a new
callback into that slot. Emitter raw `0x12C2030` checks both that pointer
and the pointed-to callback before invocation. Setter raw `0x12C19F0`
changes the slot; exact selection of its fallback remains distinct from
the constructor's late-bound indirection.

Immutable PE parsing resolves IAT **VA** `0x1AC471C` as
**MSVCR120.dll!_wunlink**. Path processing at raw `0xE5F4A9`
converts UTF-8 via MultiByteToWideChar at `0xE5F4D9`
(code page `0xFDE9`, source length -1, buffer capacity `0x400` wide chars).
Call `0xE5F4EE` unlinks that path; results 0 and -1 pass the local check.
This is not a reopen/rename. At `0xE5EF78`, the receiver is the separately
loaded global **VA** `0x1E69348`, rather than merely the current
receiver's field. Later raw `0xE6F7C0` releases list `+0x48` entries'
shared-text members `+0x14` and resets list state.

The latest follow-up disputes the previously reported final-call target:
raw `0xE55565` was reported as `0x321C40`, not `0x322C40`.
It did not return exact displacement bytes, so the claimed post-close
logging attempt is provisional pending immutable-byte arbitration.
The independent `0xE5AB80 -> 0x12C2030` logging path does not by itself
prove that this service-deinit call reaches it.

## Traceback output fields and Lua/C metadata branches resolved

Raw `0xC294A0` supplies short source at formatter output `+0x24`,
current line at `+0x14`, and name at `+4` to
`"    at %s(%d): %s"`. Raw `0xCCF500` computes
`((nodePC - prototypeCode) >> 2) - 1` for the prototype line-table
index; absent line table returns -1.

Raw `0xCCF840` writes classification `what` at output `+0xC`,
source at `+0x10`, and line bounds at `+0x18/+0x1C`.
Mapped literals include `"Lua"`, `"main"`, `"C"`, `"=[C]"`, and `"=?"`.
The native branch sets `what="C"`, source `"=[C]"`, bounds -1.
The caller's leading-C suppression tests `what`, not the source/name;
after the first non-C frame it emits later C frames too. This is concrete
Lua/native classification evidence, not proof of a particular Lua version.

When output name `+4` is null, the caller uses **VA** `0x1E4891C`
if nonzero, otherwise `"~unnamed~"` (**VA** `0x1BBAE04`,
backed raw `0x17B9A04`). Raw `0xCCFF50`'s flag-4 case returns
`"hook"` and supplies `"?"` via its out-pointer. Native frames admitted
after the initial filter can therefore be emitted with line -1 and a
fallback name. The global fallback name's producer/lifetime is the next
bounded question.

## Immutable rel32 arbitration confirms post-close Shutdown Finished attempt

Python signed-rel32 decoding and IDA instruction-length checks agree:

| Raw call | Exact bytes | Raw target | Code VA |
|---|---|---|---|
| `0xE553FE` | `E8 3D D8 4C FF` | `0x322C40` | `0x723840` |
| `0xE55546` | `E8 75 A2 01 00` | `0xE6F7C0` | `0x12703C0` |
| `0xE55565` | `E8 D6 D6 4C FF` | `0x322C40` | `0x723840` |

This supersedes the disputed `0x321C40` target. That address is inside
an instruction beginning at `0x321C3F`, whereas `0x322C40` starts
`55 8B EC` and implements the object/threshold-gated logging helper.
The early message is `"--- Shutting Down ---"`; the final call passes
console **VA** `0x1E69348` and **VA** `0x1BDF628`,
`"--- Shutdown Finished ---"`, with severity 2.
Console slot `+0xB0` at raw `0x1876B58` contains
`80 B7 25 01`, target **VA** `0x125B780`, raw `0xE5AB80`.
Thus a conditional post-close logging attempt is established, not withdrawn.
Actual runtime emission still depends on helper/logger gates and current sink.

Console slot `+0xC0` at raw `0x1876B68` contains
`D0 BC 25 01`, target **VA** `0x125BCD0`, raw `0xE5B0D0`.
It returns when receiver byte `+4` is zero; otherwise it decrements/clamps
indentation depth **VA** `0x1E49A10`. This resolves that final virtual edge.

## Traceback fallback name is borrowed per-dispatch state with normal save/restore

Raw `0xC3B3C0` takes original argument `a2` into EBX.
At `0xC3B50A` it saves global **VA** `0x1E4891C` in ESI;
at `0xC3B514` it installs EBX in that global.
After dispatch raw `0xC289C0` and helper `0xBF7860`, raw `0xC3B582`
restores the saved pointer. This is a temporary borrowed-pointer override,
not an allocation/copy or clear-to-zero. Normal nested/reentrant returns
restore the prior value; exceptional restoration remains unverified.

The global lies in `.data`'s virtual tail at delta `0x8391C`,
beyond raw size `0x71800` and within VirtualSize `0x15647C`.
Its loader initial value is zero; raw-IDB apparent bytes are not initialization.
Traceback consumers raw `0xC294A0` and `0xC2C767` select the global
when nonzero, otherwise a `"~unnamed~"` literal (the latter at
**VA** `0x1BBB50C`, backed raw `0x17BA10C`).

Caller raw `0xC38CA0` supplies `a2` from per-entry field `v8[2]`
with a default when null, separately from `*v8`.
Format **VA** `0x1BBD77C` is `"%s - %s"`: the object's `+0x70`
label is first, this argument second. The argument also reaches string-cache/
push calls `0xC281A0 -> 0xCD5020` and callback dispatch.
The supported interpretation is a per-entry name/source used during dispatch,
not a proven application-specific script/command identifier.
Five explicit address references cover the two readers and save/install/restore;
this bounded scan does not exclude computed or aliased writes.

## Console deinit disables the final shutdown-message dispatch gate

Raw `0x322C40` forwards severity 2 only when console byte `+4` is nonzero,
byte `+5` is zero, and mutable threshold **VA** `0x1E6B5E8 <= 2`.
It otherwise returns without calling console `+0xB0`.
Therefore the earlier clear of `+4` at raw `0xE5F597` disables not just
indentation decrement but this message's own logging dispatch as well.
The final `"--- Shutdown Finished ---"` helper call remains present;
on the established state path it is gated off, absent intervening re-enabling.
This refines the earlier conditional-attempt description rather than claiming
an actual post-close sink invocation.

Category emitter raw `0x12C2030` invokes the current callback through the
slot pointer and does not itself choose a default sink.
Setter raw `0x12C19F0` selects **VA** `0x14D9440`, or default
**VA** `0x16C21B0` if the former address is null. The former is a fixed
nonnull image address here, so the alternate branch is not a normal
default-restore path. Separate routines inline the assignment; absent direct
xrefs to this setter do not prove the callback cannot change elsewhere.
Threshold initialization and byte `+5` producers remain a bounded continuation;
neither is inferred from arbitrary raw-IDB bytes.

## Console emission guard and logger threshold producers identified

Console constructor zeroes the `+4/+5` word. Config loader raw `0xE4E210`
sets byte `+5` at `0xE4E263`, then clears it at `0xE4E32D`;
the interval includes config dispatch and callbacks. This is scoped emission
suppression, not a threshold update. Calling it a reentrancy guard requires
more than these writes; exact failure/nesting behavior is the next check.

Raw `0x12C1F90` updates logger-state **VA** `0x1E6B5D8` field `+0x10`,
the threshold **VA** `0x1E6B5E8`, and can log a change message.
Other helpers temporarily assign 4 and restore a saved threshold.
The field is in `.data`'s virtual tail, initially loader-zero-filled;
0 is its loader initial value, not a claim about later runtime state.
No console-variable registry/config-key identity was established.

## Dispatch unwind funclet observed; state-to-action binding still required

The bounded EH follow-up reported pushed **VA** `0x1A4AA4B`
(code raw `0x1609E4B`) leading to local-wrapper cleanup via
raw `0x60C960` with `[ebp-0xA8]`, not an observed global-name restore.
It also identified state 0/-1 writes at `0xC3B544/0xC3B56F`.
However, it did not decode the exact handler/FuncInfo/unwind-map routing
or establish whether that pushed target is a handler versus a cleanup funclet.
Therefore normal-path restoration is proven, but a blanket exceptional
non-restoration conclusion remains provisional pending that binding.
Absence of IDA-decoded EH maps does not prevent reading their underlying bytes.

## Config-load suppression is unconditional normal-path mute, not a nesting barrier

Raw `0xE4E210` calls its severity-2 helper at `0xE4E256`,
then sets console byte `+5` at `0xE4E263`, before path construction
and BufferFile at `0xE4E2BB`. It does not test BufferFile's return.
A normal false return still reaches buffer cleanup, the pump at
`0xE4E327`, and the clear at `0xE4E32D`; no ordinary early return
bypasses that clear.

The code saves no previous byte value: it assigns 1 then 0.
Consequently nested calls could clear an outer suppression interval;
this is not a reentrancy barrier or nesting-preserving guard.
The inspected EH entry **VA** `0x1A5F460`, raw `0x165E860`,
and nearby cleanup stubs yielded no guard-reset action. Normal clear is
proven; exceptional restoration was not established by that bounded inspection.

Sampled helpers raw `0x322C40` and `0x6E1A80` test `+5`,
but sink raw `0xE5AC70` has no corresponding entry check.
Therefore suppression cannot be generalized to all output routes without
following a concrete command/caller; that is the next bounded task.

## Dispatch EH map resolved: state-0 cleanup does not restore fallback global

Correction: handler **VA** `0x1A4AA4B` maps to raw `0x1649E4B`,
not `0x1609E4B`. The latter was an unrelated cleanup thunk.
Prologue raw `0xC3B3C0` begins
`55 8B EC 6A FF 68 4B AA A4 01 64 A1 00 00 00 00 50 81 EC 9C 00 00 00`,
registering the actual handler and initializing EH state -1.
The handler checks the frame cookie, loads FuncInfo **VA** `0x1D5E1E4`,
and jumps to raw `0x159DE38`, whose import stub reaches
**MSVCR120.dll!__CxxFrameHandler3**, IAT **VA** `0x1AC4814`.

FuncInfo at backed raw `0x195CDE4` has magic `0x19930522`,
maxState 1, unwind-map **VA** `0x1D5E208`, and zero try-block/IP-map
counts/pointers. Its sole entry, raw `0x195CE08`, is
`toState=-1, action=VA 0x1A4AA40`.
Action raw `0x1649E40`, bytes
`8D 8D 68 FF FF FF E9 15 DA 5A FF`, passes `[ebp-0x98]`
to raw `0xBF7860`.

State becomes 0 at raw `0xC3B544` before dispatch `0xC289C0`,
and -1 at `0xC3B56F` before normal wrapper cleanup.
The map's state-0 action performs that local cleanup, not restoration of
global **VA** `0x1E4891C`; the explicit normal restore is `0xC3B582`.
This resolves that specific unwind action, not every possible exception path
or whether ordinary VM errors escape through C++ unwinding.

## Print command binding parked; parser dispatch and mute are distinct

A bounded command search found `"PrintLn"` and `"Print"` candidates
but did not bind their registration/callbacks. Their output behavior is
unresolved; further identical string/reference scans are parked.
The pass observed console `+5` checks around selected diagnostic calls
in raw `0xE89C50`, while indirect command-dispatch calls
`0xE89DE1/0xE89E1A` have no such check at those sites.
This does not prove a specific command bypasses suppression, but provides
a concrete parser branch to inspect next.

The follow-up's `0xB4E5AC70` unmapped-address observation is irrelevant:
`+0xB4` was a vtable byte offset, not a prefix to code raw `0xE5AC70`.
It does not invalidate the earlier independently traced sink.

## Protected VM dispatch returns status and has a C++ error-transfer path

At raw `0xC3B551`, caller `0xC3B3C0` invokes `0xC289C0`
with VM state in ECX, the Boolean result of `0xC25E30` in EDX,
and stack arguments `(1, savedTopIndex+1, 0, 0)`.
The final zero selects protected wrapper `0xCD7890`, bypassing
the direct `0xCD7110` branch. That wrapper invokes callback
**VA** `0x1029540` through raw `0xCD6C10`.

Raw `0xCD6C10` saves VM error-frame pointer `+0x2C`, installs
a local frame with status `+8` zero, invokes the callback, then restores
the prior pointer and saved VM counter. Raw `0xCD7890` handles nonzero
status by unwinding to the call slot, placing the error value there,
and restoring VM state. Success returns zero.
Caller `0xC3B3C0` tests the status at `0xC3B592`, returns `status==0`,
and populates its optional result byte from VM-top truthiness only on success.
The output byte begins zero; `0xC278C0` restores the saved VM top.

Raiser raw `0xCD6B70`, when an error frame exists, writes its status
at frame `+8` and calls thunk raw `0x159E27A` to `_CxxThrowException`.
Thus this error transfer is C++-based, not a demonstrated C longjmp.
The protected wrapper's exact catch/type routing remains unbound; status
plumbing alone does not prove which native exceptions it catches or lets escape.
Nor does every nonzero status necessarily invoke the traceback logger.
During the dispatch interval, the borrowed global supplies fallback names
only for traceback frames whose own name is null.

## Parser diagnostic suppression does not gate matched command callbacks

The follow-up addresses here are raw IDB offsets, despite its report's
generic VA label. In raw `0xE89C50`, the clear
**VA** `0x1E49A59` branch looks up variables through slot `+0x50`;
a match invokes callback field `+0x1C`. Otherwise command lookup uses
slot `+0x4C`. Single-token dispatch invokes `+0x38` at `0xE89DE1`;
multi-token dispatch invokes `+0x98` at `0xE89E1A` with parsed arguments.
Neither dispatch-site call is guarded by console byte `+5`.
On failed command lookup, a leading `':'` suppresses the diagnostic call
at `0xE89E53..0xE89E6A`; otherwise its helper applies the emission gates.
Assignment-related diagnostics in `0xE41360` use the same helper;
independent flag branches can still bypass assignment handling.

Raw `0xE89C50` returns consumed input length; its caller `0xE89EC0`
separately updates `_` through console `+0x98` when the statement value
changes. ExecSkip remains a distinct dispatch-bypass mechanism.
Diagnostic format operands **VA** `0x1BE26D4/0x1BE26EC` require
PE-aware string mapping before their text is assigned to these branches.
The follow-up's unreadable-at-VA result does not establish missing strings.
Console `+0xB0` is the previously byte-verified `0xE5AB80`, distinct
from sink slot `+0xB4` / `0xE5AC70`; the two should not be conflated.

## Parser diagnostic text and assignment feedback mapped from the PE

Format **VA** `0x1BE26D4`, backed raw `0x17E12D4`, is
`Unknown command "%s"`. Raw `0xE89E5E..0xE89E6A` supplies token 0
after lookup failure unless the input starts `':'`.
When global **VA** `0x1E49A59` is set, the alternate branch bypasses
lookup and reports token 0, later tokens with `" %s"`
(**VA** `0x1BE26EC`, raw `0x17E12EC`), then a CR-only string.
The first token's exact format/argument forwarding and PrintConfig's mode
lifetime are the next concrete check; do not assume literal-string escaping.

Setter raw `0xE41360`, when `a4` is set, rejects metadata flag `0x200`
with `"\"%s\" is a protected value and cannot be changed"`
(**VA** `0x1BDDDA4`, raw `0x17DC9A4`), and flag `0x10`
with the analogous read-only message (**VA** `0x1BDDD6C`,
raw `0x17DC96C`). Each passes the variable name and returns before assignment.
Mapped `%f` constants at raw `0x17DC964/0x17DC968/0x17DC99C`
format numeric corrections.
The deferred flag-`0x400` path stores/marks a latched value and can report
`"The value has been latched"` (**VA** `0x1BDDD18`, raw `0x17DC918`),
with no explicit third helper argument at that call.
Immediate application has no explicit success message in this function.

## Protected VM wrapper catches C++ error transfer and returns status

Raw `0xCD6C10` registers handler **VA** `0x1A508B0`,
code raw `0x164FCB0`, which loads FuncInfo **VA** `0x1D648CC`
and reaches MSVCR120 `__CxxFrameHandler3` via IAT `0x1AC4814`.
The single try block covers states 0 through 0, catch-high 1, one handler.
HandlerType **VA** `0x1D64914` has null type pointer and handler
**VA** `0x10D786C`, raw `0xCD6C6C`: a C++ `catch(...)`.
The catch preserves nonzero status, substitutes -1 for zero, and resumes at
`0xCD6C7F`, the shared VM error-frame/counter restoration path.

Raiser raw `0xCD6B70` calls MSVCR120 `_CxxThrowException`,
IAT **VA** `0x1AC4770`, with ThrowInfo **VA** `0x1DB81FC`.
Catchable type descriptors are `.PAUlua_longjmp@@` and `.PAX`,
both 4-byte pointer types. The raiser first writes its error code to the
installed frame's status slot. This error transfer is therefore caught and
converted to the protected wrapper's returned status, rather than necessarily
escaping the caller's borrowed-name interval.
The metadata alone does not establish foreign/SEH exception translation or
asynchronous-exception handling; those remain outside this resolved contract.

## PrintConfig echo formatting and normal mode lifetime resolved

Addresses here remain raw IDB/file offsets: the follow-up's subtraction
of the code bias from `0xE8A9F0` was incorrect.
At raw `0xE8AB23..0xE8AB45`, the successful setup path invokes console
`+0xF4(1)`, pumps its local buffer via `0xE89EC0` with flag 1,
then invokes `+0xF4(0)`. Setter raw `0xE89EB0` stores the low argument
byte into **VA** `0x1E49A59` at `0xE89EB3..0xE89EBB`.
The ordinary sequence does not preserve prior mode; exceptional restoration
remains an EH-map continuation.

In raw `0xE89C50`, nonzero token count and that mode select echo instead
of lookup. At `0xE89CCA..0xE89CD8`, token 0 is supplied directly as
the format to console `+0xB0`, severity 2, null varargs pointer.
There is no caller-side `"%s"` wrapper for this token.
Later tokens use raw `0x322C40` with the already PE-verified `" %s"`
format; the final output fragment is CR-only at `0xE89D2F..0xE89D3C`.
Each path applies console enabled/suppression/threshold gates.
This describes forwarding only, not a demonstrated runtime outcome.

Pump flag 1 bypasses wait/deadline checks but not ExecSkip's positive-count
segment consumption (`0xE89F70..0xE89F91`). Consequently PrintConfig
mode does not guarantee every parsed segment reaches the echo branch.

## Protected VM error recovery normalizes one error slot and restores call state

Raw `0xCD6B00..0xCD6B5A` selects error values by status: 4 uses
the value at `[VM+0x0C]+0x90`; 6 interns 23 bytes from
**VA** `0x1BC7E6C`; other nonzero statuses copy the 16-byte value at
`[VM+8]-16`. All branches leave top `[VM+8]` at error slot +16.
The reported control-byte contents at `0x1BC7E6C` came from reading a VA
as raw and are not accepted as that message. Exact message and cached-value
ownership are the next check. Raw `0xCD5AB0` processes open entries at or
above the error slot before normalization.

For the one-argument protected call, the function slot is `[VM+8]-32`.
The saved index +1 resolves to a stack-relative slot temporarily installed
at `[VM+0x58]`; raw `0xCD7890` restores the prior field.
Failure restores `[VM+0x10]`, byte `+0x6D`, word `+0x68`,
and invokes `0xCD6EB0`, leaving the normalized error as the top value.
The outer `0xC278C0(state,savedIndex)` restores the original entry top;
nonzero status makes `0xC3B3C0` return false.
The shared catch cleanup at `0xCD6C7F` restores error-frame pointer
`[VM+0x2C]` and word `+0x6A` before returning status.

## PrintConfig EH cleanup releases locals but does not reset echo mode

Raw `0xE8A9F0` registers handler **VA** `0x1A61950`
(raw `0x1660D50`), loading FuncInfo **VA** `0x1D76214`
(raw `0x1974E14`). Magic is `0x19930522`, maxState 4;
try-block/IP-map counts are zero. Unwind map **VA** `0x1D76238`
(raw `0x1974E38`) contains:

| State | Next | Action VA |
|---|---|---|
| 0 | -1 | `0x1A61930` |
| 1 | 0 | `0x1A61938` |
| 2 | 0 | `0x1A61940` |
| 3 | 2 | `0x1A61948` |

State returns to 2 at raw `0xE8AACE`, before setter `0xE8AB2B`
and pump `0xE8AB3A`. Its unwind action passes `[ebp-0x44]`
to `0xDF2F60`, then state 0 passes `[ebp-0x50]` to `0x3A59D0`.
These release the holder/shared-text locals, not the global mode flag.
The setter with 0 at `0xE8AB45` is normal-return-only on this path;
the decoded state-2 unwind does not reset it.

## VM fixed errors are interned strings; status 4 reuses an allocation-failure cache

PE-mapped **VA** `0x1BC7E6C`, raw `0x17C6A6C`, contains the
23 bytes `"error in error handling"`. Status 6 requests its interning
via raw `0xCD4E50` at `0xCD6B30..0xCD6B35`.
Status 4 instead reuses `[VM+0x0C]+0x90`; initializer raw `0xCD4D40`
interns `"not enough memory"` from **VA** `0x1BC7C94`,
raw `0x17C6894`, and stores that object at `+0x90` at `0xCD4D6C`.
State setup raw `0xCD5460` calls that initializer.
Allocator helper raw `0xD675A0` raises status 4 via `0xCD6B70`
after allocation failure, concretely linking that code to the cached message.

Normalizer `0xCD6B00` writes the object pointer at slot `+0`,
tag at `+8` as object type byte `+4 | 0x40` (string type 4 gives `0x44`),
and advances top by 16. Other statuses copy the complete prior top value.
Before normalization, `0xCD5AB0` unlinks open entries at/above the slot,
copying their 16-byte values into entry-owned storage. Flagged values may
take `0xCD7CF0`; another branch releases the 24-byte entry via `0xD675A0`.
The fixed values are VM-interned objects, not caller-owned text buffers;
status 6 may obtain an already interned object.

## PrintConfig admits only successful whole-file loads into its local buffer

Addresses below are raw file/IDB offsets; several VAs in the follow-up
were arithmetically inconsistent and are deliberately not propagated.
PrintConfig raw `0xE8A9F0` resolves its path through `0x1318870`
and takes a shared-text reference through `0x3A5910`.
The resolver rejects input `strlen > 0x200`; leading `!` strips one
character and bypasses ordinary root-candidate search.

Loader raw `0x12BB660` uses UTF-8/wide wrapper `0x12BA030` and
the verified `_wfopen` import with `"rb"` (**VA** `0x1C15568`,
raw `0x1814168`). The wrapper converts path/mode with CP_UTF8,
source length -1, without checking conversion results.
The loader uses `_fileno` and a stat helper, fails/closes on stat error,
and reads a 32-bit length at stat buffer `+0x18`.
For nonempty input it reserves length+1 and requires exact-length
`fread(buffer,1,length,file)`. A short read resets the buffer, closes,
and returns false. Empty input skips reserve/read and succeeds with the
initial NUL buffer. Exact stat import/layout and capacity arithmetic are
the next bounded checks, not inferred from the helper's name.

The local facade starts with a one-byte heap buffer via `0x12B8030`;
successful reads append NUL, and PrintConfig releases the allocation.
Embedded NULs are loaded but scanner raw `0xE89790` stops at the first,
so later file bytes are not parsed. Open/stat/read failures precede the
mode setter; successful load alone reaches echo-mode set/pump/normal clear.
Loader's known direct callers include PrintConfig and BufferFile,
not the previously named ProcessFile entry.

## VM allocator retries nonzero-size failure once behind a context gate

Raw `0xD675A0` is fastcall: ECX VM state, EDX pointer, stack old/new sizes.
With context `[VM+0x0C]`, it calls `[ctx]` as
`(userdata=[ctx+4], pointer, oldSize, newSize)`.
If allocation returns null with nonzero new size, context `+0x8C` nonnull
permits one `0xCD9EC0` retry-helper call and a second allocation attempt;
no gate or second failure raises status 4 through `0xCD6B70`.
The retry helper's exact effects and gate identity remain open.

On return, accounting `[ctx+0x0C]` changes by
`newSize - (pointer ? oldSize : 0)`.
Zero new size does not retry/raise; a null result still subtracts old size
for a nonnull pointer. Context owns callback/opaque-userdata references
in this path; the wrapper neither copies nor frees userdata.
Setup raw `0xCD5460` writes gate `+0x8C`, but callback/userdata
initialization was not established.

## VM allocation retry requests a collection pass; marker identity remains open

On eligible allocation failure, raw `0xD675A0` calls `0xCD9EC0`
with VM/context in ECX and flag 1 in EDX, then repeats the allocator
callback with the same arguments. Raw `0xCD9EC0` sets context byte
`+0x3E` for the pass and clears it afterward. If phase byte `+0x3D <= 1`,
it advances to 2 and runs `0xCD9110`, which unlinks/retains list entries
and dispatches cleanup; four `0xCD9BE0` phase/mask steps follow.
These concrete effects support collection-retry interpretation.
The flag toggle alone does not establish a read-side reentrancy barrier.

Raw `0xCD5460` writes **VA** `0x1C740F8` into context `+0x8C`
after initialization calls. The marker's apparent raw-IDB fill bytes were
read without mapping that VA and are not accepted as its contents.
Its exact object/type and the allocator/userdata producer remain unresolved.
No static xrefs to setup were found by this pass; callback/indirect setup
remains possible and is the next concrete continuation.

## Shared loader truncates stat size to DWORD and has no reserve-result check

Immutable PE parsing identifies loader imports `_fileno` at **VA**
`0x1AC4A8C`, `_fstat64` at `0x1AC4700`, `fread` at `0x1AC490C`,
and `fclose` at `0x1AC49A8`, all MSVCR120.
The stat buffer is `[ebp-0x48]`; size read `[ebp-0x30]` is offset `+0x18`.
No upper-DWORD read/validation occurs. A zero low DWORD succeeds as empty;
nonzero uses 32-bit `length+1` via LEA without an overflow branch,
then `fread` count equal to that low DWORD.
Exact read appends NUL at data+returnedCount; short read resets the buffer.
Stat/read failures close and return false; close result is ignored.
The import proves `_fstat64`, not a target-local complete structure declaration.

Both caller facades use vptr **VA** `0x1C77A30`, raw `0x1876630`.
Slot `+0x1C` resolves to raw `0xDF47A0 -> 0xDF43F0`;
accessor `+4` is `0xDF47F0`, returning backing pointer plus offset;
reset `+0x14` is `0xDF4350`, clearing length fields and first byte.
Growth uses raw `0xDF2DC0`; the observed reserve caller stores its result
and updates capacity/length without a failure result checked by the loader.
The exact underlying allocation/reporting policy is a follow-up, not a
demonstrated runtime consequence of null data.

## Concrete VM creation installs allocator/userdata before protected initialization

Creator raw `0xC347E0` calls `0xCD5720` at `0xC34834`,
supplying allocator **VA** `0x103CCB0` and incoming `a1` as userdata.
The creator allocates 912 bytes, storing allocator/userdata at result
`+0x78/+0x7C` (`0xCD576C/0xCD576E`; the earlier 120/124 were decimal).
These are not the separate initialization callback.
At `0xCD577F`, **VA** `0x10D6060` (raw `0xCD5460`) is loaded
into EDX; protected-call helper `0xCD6C10` is invoked at `0xCD58A4`
and dispatches that callback at `0xCD6C65`.

Initializer `0xCD5460` reads the context from argument `+0x0C`.
After helper `0xD67690`, raw `0xCD5492..0xCD5496` sets context byte
`+0x3F` to 1 and `+0x8C` to **VA** `0x1C740F8`.
Creation initially zeroes `+0x8C` at `0xCD57C7`, supporting a
late-initialization/retry-eligibility gate rather than a thread identifier.
PE mapping places that marker at raw `0x1872CF8`, whose leading byte is
NUL; this proves neither a source-level name nor overall initialization success.
The concrete allocator callback's implementation/userdata use is next.

## Buffer growth copies capacity and can discard old storage on returned failure

Raw `0xDF43F0` uses pointer `+8`, capacity `+0x10`, used length `+0x14`,
and shrink floor `+0x0C`. Growth passes old capacity, not used length,
to `0xDF2DC0`. Successful replacement copies
`min(oldCapacity,newCapacity)` bytes, then frees old storage.
If allocation returns null, that helper still frees old storage and returns null;
reserve stores the result and updates capacity/length without a null-result branch.
With no old pointer it returns the allocation directly; old pointer plus zero
new size frees and returns null.

Shrink at/below the floor only changes used length; otherwise it reallocates
to `max(requestedSize,floor)` and sets capacity/length to that value.
Allocator wrapper `0x12B8030` calls `0x12C8A20` and reports a null result
through **VA** `0x1C15EA0` before returning zero if reporting returns.
That reporter's continuable assertion mechanism is already documented;
the follow-up's empty target resolution does not reopen its identity.
Exact allocation-call flags/report arguments remain a bounded continuation.
All null-result continuation claims are conditional on reporter return.

## Concrete VM allocator uses global heap and ignores userdata on inspected paths

Allocator **VA** `0x103CCB0` maps to raw `0xC3C0B0`.
Initial creation calls `(userdata,NULL,8,912)`; callback/userdata are stored
at allocated base `+0x78/+0x7C`, correcting the earlier decimal-offset
misreading as hex `+0x120/+0x124`.
Owner `0xC347E0` retains the VM at its `+0x78`.
Initial null allocation returns null; the owner conditionally logs and returns false.
Later initialization failure uses `0xCD5510` to release VM allocations and
invokes `(userdata,base,912,0)` for the outer block.

Callback raw `0xC3C0B0` frees zero-size requests through `0x12CA6F0`;
nonzero goes through `0x12CAE30`, global heap **VA** `0x1E48918`,
and `0x12CA410`. Resize tries in-place adjustment `0x12C94D0`
before allocate/copy/free fallback. Inspected paths do not dereference
userdata or oldSize, so selection is global rather than userdata-specific.
A reported null-result path logs diagnostic 4528, frees a nonnull prior
pointer, and returns null. Its exact interaction with the VM wrapper's
same-pointer retry remains a pointer-lifetime follow-up, not a runtime finding.

## VM failed-resize callback releases the pointer before the wrapper's same-value retry

Raw `0xD675C0` saves the incoming pointer. Eligible recovery calls
`0xCD9EC0(VM,1)` at `0xD675E3..0xD675E8`, then
`0xD675ED..0xD675FA` reloads the same saved pointer and sizes for retry.
Callback raw `0xC3C0B0`, after `0x12CAE30` returns null, logs 4528
at `0xC3C120` and passes its original pointer to `0x12CA6F0`
at `0xC3C13E..0xC3C140` before returning null.

The inner resize helper first attempts `0x12C94D0`, then allocation
`0x12CA410`. Successful fallback copies/frees old storage at
`0x12CAF14..0x12CAF20`; failed fallback branches to null return
at `0x12CAEEA/0x12CAF25..0x12CAF2D`, preserving old storage there.
Thus the callback, not that failed inner allocation branch, releases the old
allocation before the wrapper retries with its saved pointer value.
This is a static resource-contract ledger, not proof of runtime reuse,
dereference, or exploitability. Zero-size requests bypass wrapper retry.

Layout is now explicit: allocation base B, VM=B+4, embedded context
S=B+0x78=VM+0x74; `0xCD54B0` sets `[VM+0x0C]=S`.
At `0xCD576C/0xCD576E`, bytes `89 06` and `89 5E 04`
store callback/userdata at `[ESI]/[ESI+4]`, with ESI=S.
Raw `0xCD5771` sets `[S+0x88]=VM`, and `0xCD57C7` clears
retry gate `[S+0x8C]`. This context is not the process-global heap object.

## Buffer OOM wrapper uses the known continuable assertion reporter

Raw `0x12B8030` calls allocator `0x12C8A20`; null formats
`"OOM allocating %lu bytes."` from **VA** `0x1C5EBB8`,
raw `0x185D7B8`, with requested size.
It calls reporter slot **VA** `0x1C15EA0` with the same
`0x1C5EA58` pointer twice, line `0xDA` (218), and the formatted buffer.
The previously verified reporter raises code `0x12ED502` with flags 1;
line 218 is not that flags argument. If reporting returns, this wrapper
returns null, preserving the conditional continuation in the buffer ledger.

The allocator's region-growth call at raw `0x12C771A` resolves to
`VirtualAlloc(NULL,size,0x3000,4)` (reserve+commit, read/write).
A failed call returns zero with no retry/fallback observed in that path.
This identifies one region-growth boundary, not all small-block allocation
policy; region metadata and matching release are the next task.

## Heap free coalesces chunk metadata and conditionally takes an internal lock

Raw `0x12CA6F0` returns immediately for a null pointer, without locking.
For nonnull input it uses chunk header `pointer-8`, coalesces adjacent free
chunks, and updates boundary tags, free-list/bitmap state, and heap top fields.
Insertion decrements heap `+0x20`, calling raw `0x12C78A0` at zero;
the direct-release path subtracts released bytes from heap `+0x1B0`.
Exact region-release ownership is a separate active trace.

Internal synchronization tests heap flag `+0x1BC & 2` and conditionally
enters/leaves the critical section at `+0x1C0`. The observed initializer
clears that flag; this alone does not establish a complete concurrency policy
or exclude outer locking/later reconfiguration.
Raw `0xC327D0` publishes raw `0x12CA210`'s allocator-object result
into global pointer slot **VA** `0x1E48918`; the slot is virtual-tail data,
not the raw-IDB bytes at its numeric VA. Callers load that pointer into ECX.
Concrete creation options and surrounding synchronization are the next check.

## Heap region growth publishes metadata only after successful VirtualAlloc

Addresses here are raw IDB/file offsets. The follow-up incorrectly treated
raw `0x12C771A` as a VA and subtracted the code bias; that alternative
`0xEC6B1A` is not accepted as the call site.
Allocation raw `0x12C8A20` first searches existing bins/free space.
Its request normalization uses 16 for n<11, otherwise `(n+11)&~7`,
rejecting requests at or above `0xFFFFFFC0`.
This is not a VirtualAlloc call for every allocation.

Initialization raw `0x12C6A90` obtains GetSystemInfo and uses allocation
granularity **VA** `0x1EDAFC0`, checked as a power of two.
Region helper raw `0x12C7690` computes
`(internalRequest + granularity + 0x2F) & ~(granularity-1)`,
checks wrap and configured heap limit `+0x1B8`, then requests reserve+commit,
read/write. Successful allocation updates total `+0x1B0` and high-water
`+0x1B4`; failure precedes those updates and metadata publication.

Inline region record `+0x1D8` contains base, size, next, flags.
Raw `0x12C7400` copies the prior 16-byte record near the old top region's
end and publishes the new inline record. Free-side trim raw `0x12C7BF0`
and collection raw `0x12C78A0` use VirtualQuery validation and
`VirtualFree(address,0,0x8000)` release.
PE-verified IAT VAs are `0x1AC42A8` VirtualQuery,
`0x1AC42B4` VirtualFree, and `0x1AC42B8` VirtualAlloc.
The linked traversal begins at `+0x1E0`; inline-seed retention,
flag-bit gates, and partial-release failure bookkeeping remain unresolved.

## Global VM heap disables its internal lock bit after creation

Raw `0xC327D0` conditionally creates the heap only when global pointer
**VA** `0x1E48918` is zero, publishing the result at
`0xC32BF4..0xC32C01`; otherwise it reuses the pointer.
Raw `0x12C6A90` initializes config flags to 7, and `0x12CA110`
copies/ORs 4 into heap `+0x1BC` and initializes CS storage `+0x1C0`
through IAT **VA** `0x1AC41BC` with `0x80001020`.
The import's exact name remains a PE-parser continuation.
Factory `0x12CA210` then clears mask 2, yielding flags 5,
and sets heap `+0x1E4` to 1.

Allocation/resize/free internal lock branches test mask 2 and are skipped
for this initial flags value. The inspected VM callback has no visible outer
heap lock. A separate factory `0x12CA280` can set/clear mask 2;
that does not establish reconfiguration of this concrete global heap.
No teardown/reset was established, and external serialization is unknown.
These observations describe flag selection, not overall thread safety.
The earlier follow-up's unresolved VirtualAlloc label does not reopen the
already PE-verified IAT **VA** `0x1AC42B8`.

## Region collector retains the inline root and commits accounting after full release

Raw `0x12C78A0` starts candidates at heap `+0x1E0`, with predecessor
record at `+0x1D8`: the inline record is the root, not itself traversed.
Raw `0x12C7400` copies the prior 16-byte root record into region metadata
before replacing the inline base/size/link/flags; the growth caller supplies
flags 1. Eligible linked records require bit 0 set, bit 3 clear, and the
observed free-block/boundary checks. No broader flag names follow yet.

Each release requires VirtualQuery success, BaseAddress and AllocationBase
equal to the query address, committed state `0x1000`, and RegionSize no
larger than remaining bytes, then VirtualFree(address,0,0x8000).
Only complete record release adds to returned bytes, subtracts heap `+0x1B0`,
and unlinks via predecessor `+8` (`0x12C7AB2..0x12C7ABF`).
On query/free failure, the record/counter remain, free-block metadata is
reinserted (`0x12C7AC4`), and traversal continues.
Heap `+0x20` becomes visited-record count capped at `0xFFF`, not released count.

Trim raw `0x12C7BF0` selects the top-containing record and avoids trimming
one containing the inline-root address. It uses the same flag gate and releases
an aligned tail. Record-size/counter/top updates occur only after the full
requested trim succeeds (`0x12C7CE0..0x12C7CE6`).
If a later span fails after earlier OS releases, no rollback of those releases
or partial bookkeeping update was observed. The separate collector can still
release other records; outer success reflects a nonzero reported released total.
This is static branch/bookkeeping evidence, not an observed partial failure.

## Global heap lock initialization import and construction chain identified

Immutable PE parsing resolves IAT **VA** `0x1AC41BC`
(raw `0x16C2DBC`) as **KERNEL32.dll!InitializeCriticalSectionAndSpinCount**.
Global allocator config setup raw `0x12C6A90`, call `0x12C6B14`,
passes CS **VA** `0x1EDAF88` and `0x80001020`;
heap initialization separately initializes its `+0x1C0` CS.
The internal heap lock mask is **2** (bit index 1), not mask 4:
factory clearing that mask leaves flags 5.

Observed construction chain is raw
`0xBFCA30 -> 0xC34100 -> 0xC327D0 -> 0x12CA210`.
Raw `0xC34100` allocates `0x500` bytes and calls the owner constructor
at `0xC3414D..0xC3416B`.
Global-heap publication remains null-guarded; a direct appinit/CRT caller,
external callback serialization, and balanced heap teardown were not established.
The next task follows the concrete UI-owner field rather than repeating
an unsuccessful direct-caller search for `0xBFCA30`.

## Automatic heap trim and periodic collection triggers identified

In raw `0x12CA6F0`, a free merging into the top chunk grows heap `+0x0C`.
If that size is strictly greater than threshold `+0x1C`, it calls
`0x12C7BF0(0)` at `0x12CA8A3..0x12CA8B0`.
Initialization `0x12C6A90` sets global **VA** `0x1EDAFC8` to
2 MiB; `0x12CA110` copies it to heap `+0x1C`.
Regular-bin insertion instead decrements countdown `+0x20` and calls
`0x12C78A0` when the prior value is 1 (`0x12CAD7B..0x12CAD82`).
Top-chunk/direct-release paths bypass that decrement.
Factory countdown starts at `0xFFF`.

Trim also invokes collection when its size argument is below `0xFFFFFFC0`
and a top chunk exists, even if its own trim reported zero.
Other observed trim callers `0x12C8D60/0x12C9C20/0x12CB030`
are not expanded here.
The new report disputes two prior details: trim VirtualFree may decommit
rather than release, and countdown reset may be max rather than min of
`0xFFF` and visited count. Neither conflicting interpretation is treated
as settled until exact call arguments and cmp/cmov bytes are recorded.

## Scripting-object VM initialization follows construction; UI +A00 binding remains open

Addresses below are raw IDB offsets, not the follow-up's generic VA label.
Raw `0xC34100`, after allocating/constructing the `0x500`-byte object,
calls `0xC34450` at `0xC3417B`. That routine invokes `0xC347E0`
at `0xC346BB`; the latter checks object `+0x78`, creates the VM through
`0xCD5720`, and stores the result at `0xC3480E..0xC3483C`.
Null creation and later chunk-load failure return failure; those later
failure paths do not locally clear the newly stored VM pointer.
Success reaches `0xC34BE1`.

On initialization failure, `0xC34100` calls `0xC341C0` with the newly
constructed object and original owner, then returns zero
(`0xC34186..0xC3418D`). Nonnull cleanup proceeds through `0xC33060`;
exact VM release/reset in that downstream path is the next task.
The null-object branch reports an assertion then calls `0xBFCF30(0)`.

UI-manager constructor raw `0xBF7A80` clears `+0xA00/+0xA04/+0xA08`
at `0xBF8186..0xBF819A`. Destructor `0xBF8AC0` checks `+0xA04`,
calls that object's virtual `+0x10`, then clears `+0xA04`
at `0xBF8AFA..0xBF8B0B`.
Caller `0xBFCA30` consumes its created object's `+0x70` label and a
collection at its own `+0x5E4`, not a proven `+0xA00` publication.
The missing UI-context identity edge remains a bounded stop; these objects
must not be merged solely from their shared VM field offset.

## Exact heap release operands and signed countdown reset supersede prior conflicts

The immutable-byte arbitration finds three IAT `0x1AC42B4` calls,
all `FF 15 B4 42 AC 01`, with these arguments:

| Raw call | Pushes immediately before call | Arguments |
|---|---|---|
| `0x12C7A91` | `68 00 80 00 00 6A 00 57` | `(EDI,0,0x8000)` |
| `0x12C7CC1` | `68 00 80 00 00 6A 00 56` | `(ESI,0,0x8000)` |
| `0x12C7DB5` | `68 00 80 00 00 6A 00 56` | `(ESI,0,0x8000)` |

The previously PE-verified VirtualFree import therefore receives MEM_RELEASE,
not MEM_DECOMMIT, including in the disputed trim path.
The third call belongs to raw `0x12C7D30..0x12C83A0`, whose role is
a new bounded continuation. No fixed 4096-byte RegionSize is inferred from
the reported `0x1000` query check without decoding its structure field.

At raw `0x12C7B4E..0x12C7B5C`, bytes
`B9 FF 0F 00 00 8B C6 3B F9 0F 4F CF 89 4B 20`
load ECX=`0xFFF`, compare EDI with ECX, select EDI with signed CMOVG,
and store ECX at heap `+0x20`.
Thus reset is `max(0xFFF, (int32_t)visitedCount)`, not a cap/minimum.
Earlier capped-count statements are superseded by these exact instructions.

## Shared chunk free differs from collection and trim; query State check clarified

Raw `0x12C7D30` is a shared free/coalescing routine, not inherently a
destructor. Its direct region-release branch requires chunk-header low two
bits clear. Batch-free caller `0x12C9C20` clears a nonnull pointer slot
before passing chunk header/size; adjacent entries may coalesce.
Splitting caller `0x12C9860` tags split chunks with mask 2, selecting the
ordinary free-list path instead of this VirtualFree branch.

Its query buffer has size `0x1C`. Offsets `+0/+4` (BaseAddress/
AllocationBase) must match ESI; `+0x10` (State) must equal `0x1000`;
`+0x0C` (RegionSize) must not exceed remaining extent.
This corrects the arbitration report's mistaken fixed-RegionSize wording.
Only after complete release at `0x12C7DB5` does it subtract
`previousSize + a3 + 16` from heap `+0x1B0`.
Query/free failure leaves accounting unchanged; a query-success/field-mismatch
path can still return nonzero, so that raw return is not blanket release success.

## Scripting initialization failure closes VM before freeing object and removing registry identity

Caller raw `0xBFCA30` derives a string prefix by splitting its input at `'.'`
and passes that string to `0xC34100`, not an owner object pointer.
The newly constructed object is ECX for `0xC34450`; the string is a
separate stack argument. On failure it calls `0xC341C0`, and the caller
does not insert the new object into its success-only structures.

For nonnull cleanup, `0xC341C0` calls `0xC33060(object)`, releases
the object through `0x12C8D60`, then calls `0xBFCF30` through global
**VA** `0x1E002B8` with the object's pointer value.
Raw `0xC33060` removes matching references via `0xBF9BD0`
(global-context list `+0x6F0`), performs `0xC3E990(object,1)`,
and releases removed-entry references through `0xC33E60`.

At `0xC33281`, bytes
`8B 43 78 85 C0 74 0E 8B 40 0C 8B 88 88 00 00 00 E8 7A 22 0A 00`
follow object `+0x78` through the VM context's `+0x88` main VM pointer
and invoke `0xCD5510`. That close drains/finalizes VM queues, releases
buffers, and frees base `VM-4`, size `0x390`, through the stored allocator.
No object `+0x78` clear is needed to describe this observed sequence:
the enclosing object is freed next, not shown reused.
The later `0xBFCF30` registry pointer usage remains a separate bounded check;
the order alone does not establish a stale-pointer dereference.

## Batch free consumes pointer slots in array order and may merge adjacent entries

Raw `0x12C9C20` receives heap in ECX, pointer array in EDX, count on stack;
veneer `0x12CB020` tail-jumps without changing those arguments.
The plain return leaves stack cleanup to the caller.
Null entries are skipped; nonnull entries are cleared at `0x12C9C65`
before header validation and handoff to `0x12C7D30` at `0x12C9CC4`.
Exactly adjacent next entries can be merged, updating the header and replacing
the next slot with the merged pointer for its subsequent iteration.
The routine does not sort the array.

Normal completion returns zero, not a released-byte/failed-entry count.
It conditionally trims at `0x12C9CDD..0x12C9CE3` when top size
exceeds threshold, then releases the optional lock.
The validation branch `0x12C9C6E..0x12C9C80` follows slot clearing;
its error call at `0x12C9D06` remains unidentified. INT3 padding alone
does not establish the external error callee's return/termination contract.
No higher-level direct caller beyond the veneer was established.

## Registry cleanup uses pointer identity; missing-key erase claim needs exact range binding

Raw `0xBFCF30` conditionally clears manager `+0x574` when equal to
the supplied pointer, removes the first matching pointer from array
`[+0x68C,+0x690)` by shifting the tail, then processes hash container `+0x5EC`.
Lookup `0xC0A2A0` receives a stack copy of the pointer, hashes its four bytes
through `0xC1F4D0`, and compares node `+8` keys.
This does not dereference the released target object's fields.
Range erase `0xC0A1B0` disposes registry nodes through `0x12C8D60`.

The follow-up reported sentinel/sentinel lookup results on miss and interpreted
an erase optimization as unconditional full clear via `0xC07560`.
Exact first/last versus head/begin comparisons were not returned; that
surprising miss behavior remains provisional until range arguments and branch
bytes are bound. Do not equate empty range with full range from symbol names.
Zero input reaches assertions (caller line 2392, cleanup line 1491) before
continuation if reporting returns; the known reporter is not newly unresolved.

## Registry miss is a no-op in a nonempty container, not a full clear

The follow-up supplied useful instruction bytes but mislabeled IDB offsets
as VAs and subtracted the code bias. Addresses below retain raw IDB offsets;
its alternative `0x7FC3E2/0x809729/0x8095BE` mappings are not propagated.
At raw `0xBFCFE2`, the erase call receives registry `this+0x5EC` in ECX
and stack arguments `(&key,begin,end)`, pushed end, begin, key pointer.
On miss, raw `0xC0A329` bytes `8B 45 08 89 38 89 78 04`
store the head into both endpoints.

Raw `0xC0A1BE`, bytes `3B 01 75 19 3B D1 75 15`, compares
begin with `head->next`, then end with head. Only that full-range pair
takes `0xC07560`; otherwise equal endpoints return without erasing.
Therefore an absent key in a nonempty registry is a no-op.
For an empty self-linked registry, head/head also denotes the full empty
range, and clearing merely resets the already empty container.
This supersedes the earlier missing-key/full-clear interpretation.
Container fields are head `+0`, count `+4`, bucket storage `+8`;
nodes use next `+0`, previous `+4`, pointer-value key `+8`.

## Batch-free validation calls CRT abort, not the continuable project reporter

The guard at raw `0x12C9C6E..0x12C9C80` checks chunk header
`h=pointer-8`: unsigned h below heap `+0x10`, or
`(DWORD[h+4] & 3)==1`, takes the error branch.
The lower-bound field is maintained from VirtualAlloc bases at `0x12C6E70`.
This guard does not compare a magic constant or establish full owner identity.
Slot clearing precedes it; there is no upper-bound check in this guard.

Raw `0x12C9D06`, bytes `FF 15 24 4A AC 01`, calls
**MSVCR120.dll!abort**, PE-verified IAT **VA** `0x1AC4A24`,
raw `0x16C3624`, without arguments.
With internal locking enabled, this branch precedes normal LeaveCriticalSection
at `0x12C9CF8` and has no local unlock. This is the external CRT abort
boundary, not an inference from the following INT3 padding.
The header's first-word interpretation remains a producer-side follow-up.

## Scripting success publishes into distinct prefix tree and pointer hash set

Raw `0xBFCA30` indexes manager `+0x5E4` by the created object's
shared-text prefix at `+0x70`. Helper `0xC07380` returns its mapped-value
slot; `0xBFCC05` stores the object pointer there. Insertion helpers
`0xC09C10 -> 0xC173D0/0xC17480` use node shared-text key `+0x10`
and pointer value `+0x14`, retaining the key and initially nulling the value.
The stored object pointer has no visible AddRef in this path.

Duplicate-prefix precheck at `0xBFCBAE` reports/asserts. If reporting
returns, lookup reuses the existing node and replaces its pointer value;
this is not duplicate-node insertion. Prior-object lifetime in that case
was not established.
Success path `0xC38400 -> 0xC53B90` separately inserts the raw object
pointer into hash set `+0x5EC`, distinct from the prefix-keyed tree.
No direct success-path assignment to selection slot `+0x574` was found
in the inspected routines; its removal-time equality clear does not prove one.

Raw `0xBFCF30` removes pointer-set/array entries but does not yet establish
prefix-tree removal. The next bounded trace follows that tree's lifetime
relative to object destruction. The containers' raw-pointer storage alone
does not establish the manager's complete ownership/refcount policy.

## Chunk first-word meaning differs between direct mappings and boundary-tag chunks

Direct-mapping helper raw `0x12C6E70` computes
`R=(granularity+request+30)&~(granularity-1)`, obtains a region,
sets `pad=(-base)&7`, header `h=base+pad`, and stores pad at h,
size `S=R-pad-16` at h+4 with low flags clear.
It writes fence values 7 at h+S+4 and 0 at h+S+8, returning h+8
(`0x12C6EB9..0x12C6F2B`). Paired raw `0x12C8D60`
recovers base by subtracting pad and extent `pad+S+16`.
Here the first word is mapping-base padding, not previous-chunk size.

Ordinary allocation raw `0x12CA410` uses chunk size
`max(16,(request+11)&~7)`, returns h+8, and stores allocated size|3.
For ordinary chunks with previous-in-use clear, h holds previous-size
boundary metadata; free/coalescing writes size into the following chunk's
first word while clearing that flag (`0x12C8218..0x12C8226`).
Thus the first word must not be universally labeled alignment adjustment.

Aligned helper raw `0x12C9860` rounds alignment to a power of two,
requests need+alignment+12, aligns the user pointer, and releases a large
enough prefix through ordinary coalescing. Its header uses
remainingSize|oldPreviousInUse|2 (`0x12C994D..0x12C996B`).
Alignment/remaining-size checks can reach abort.
Direct-mapping selection in `0x12C7690` is flag/threshold gated;
exact configuration/fallback is the next bounded question.
