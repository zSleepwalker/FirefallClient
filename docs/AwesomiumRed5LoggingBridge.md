# Red5 JavaScript logging bridge

Static inspection of `FirefallClient.exe.original`; no browser, script,
logger, or callback was run. Code addresses below are raw file offsets;
import/string addresses are loaded-image VAs.

The client initializer at raw code offset `0xD668D0` uses bytes `this+0x72`
and `this+0x73` as one-shot setup state. A nonzero success byte at `+0x73`
returns ready. If that byte is zero but `+0x72` is nonzero, setup returns
unavailable without trying again. Only when both bytes are zero does it make
the setup attempt.

That attempt constructs the wide string `Red5`, then calls the object at
`this+0x20` through vtable offset `0x120`. If the result passes the imported
validity check, it stores the returned object at `this+0x28`, calls
`JSObject::SetCustomMethod` with wide method name `errorlog` and trailing
argument `0`, then sets `this+0x73`. The meaning of the trailing zero is not
established here. This confirms registration of a custom method on the
`Red5` JavaScript object; the native callback implementation and the
JavaScript caller are not established by this initializer.

If object creation fails, the initializer reads a failure code through
vtable offset `0x11C`, clamps values above `7` to `7`, uses that value to
select a string, emits `Unable to create Red5 JS object: %s`, and sets
`this+0x72`. The recorded failure flag prevents a later retry through this
entry point. The meanings of the eight failure strings remain unresolved.

The two observed `SetCustomMethod` call sites are raw function `0xD64530`,
using a caller-supplied method name and trailing zero, and the initializer
above. Raw function `0xD651B0` is a distinct, setup-gated call to
`JSObject::SetPropertyAsync` through import VA `0x1AC4EF0`, not a third
`SetCustomMethod` site. The caller-supplied property semantics are not
assigned.

## Native callback

The RTTI-associated `JSMethodHandler` subobject vtable is at VA `0x1C7656C`;
its first entry resolves to raw function `0xD65A60` (VA `0x1166660`). The
handler obtains the saved `JSObject` remote ID through the imported
`remote_id` slot at VA `0x1AC4F00` and compares it with the callback's
incoming remote ID. A mismatch goes to the unknown-callback log path.

When the remote ID matches, the handler performs an ordered, string-keyed
lookup rooted at offset `+0x474` of the enclosing object, using the incoming
method name. The lookup helper at raw `0xCCD640` uses `strcmp`; an exact
match enters the mapped callback path, while a miss (or the helper's
additional sentinel condition) falls through to the special-method check.
The mapped path creates a 12-byte record holding retained method-name text,
a `JSValue`, and a `JSArray`; the lookup result itself is not retained in the
record.
It examines the final array argument, not the first. When that value is an
object, raw `0xD64320` applies a cached JavaScript predicate whose ASCII
source is at VA `0x1BD4570`:
`(function (object) { return !!(object && object.constructor && object.call && object.apply); })`.
The helper invokes the predicate's `call` method (method-name literal
`call` at VA `0x1BD4544`) with `null` and the candidate object. If the result
is defined and truthy, the handler stores that trailing value separately and
pops it from the copied argument array; otherwise it preserves the full
array. Raw `0xD66B70` appends the record to the handler's collection at
`this+0x34`.

The queue consumer is raw function `0xD64AE0` (VA `0x11656E0`), virtual
slot20 of the primary `uiWebFrameInternal_17` vtable at VA `0x1C765FC`.
Its RTTI complete-object locator at VA `0x1CD0FE0` identifies that class at
object offset0. The consumer checks the pending count at `this+0x5C`, takes a
record from the collection at `this+0x4C`, and uses the record's first field
as the method-name key for a fresh lookup in the ordered tree rooted at
`this+0x474`. Thus the queued field is the retained method name, not a
pre-resolved tree entry. If the mapping is no longer found, that queued
callback is discarded rather than invoked.

## Native Lua callback registration and dispatch

The Lua binding at raw `0xCCAD70` implements the `AddWebCallback` method
described by the usage string at VA `0x1BC766C`. It checks that the callback
argument is function-like, obtains a callback reference through
`0xC56170`, and calls registration helper `0xCCC130` with the method name,
the Lua state, and that reference.

The registration helper requires a nonzero field at `this+0x418` and rejects
reference values `-1` and `-2`. It rejects method names containing an
apostrophe or backslash. The method map is the ordered tree rooted at
`this+0x474`, also used by the JavaScript method handler and queue consumer.
When replacing a name, it releases the old callback reference through
`0xC56250` and removes the old tree node. It then stores the Lua state and new
reference in the map entry and calls virtual slot `+0x30` on the object at
`this+0x41C`.

A vtable at VA `0x1C765FC` has the same RTTI complete-object locator as the
previously identified `uiWebFrameInternal_17` vtable and contains raw
`0xD64530` at slot VA `0x1C7662C` (`+0x30`). That method calls
`JSObject::SetCustomMethod` (IAT VA `0x1AC4EE0`) with the supplied name and
trailing argument `0`, gated by successful bridge setup. This agrees with the
registration path; the dynamic vptr assignment for the particular
`this+0x41C` field was not independently located.

On queue consumption, the selected map entry supplies the registered Lua
state and callback reference. The consumer converts each JavaScript argument
to the Lua-side value representation through raw `0xCBBE00`, then dispatches
the registered Lua callback through adapter `0x1029540` inside wrapper
`0xCD6C10`. The `JSValue` retained at record offset `+4` is instead the
optional trailing JavaScript continuation separated by the method handler.
If it is undefined, tested through imported `JSValue::IsUndefined` (IAT VA
`0x1AC4EE8`), only that continuation is skipped; the mapped Lua callback
has already been dispatched. Otherwise, Lua return values are marshalled
into a `JSArray`, and the saved JavaScript function's `call` method (UTF-16
literal at VA `0x1BD47B4`) is invoked with a null receiver and those values.
A state check at global VA `0x1E686C4` offset `+8` selects imported
`JSObject::InvokeAsync` (IAT VA `0x1AC4EEC`) when nonzero, and
`JSObject::Invoke` (IAT VA `0x1AC4EDC`) otherwise. Conditional debug logs
include `Executing callback '%s' from JavaScript`, `   #%d: %s`, and
`Javascript callback returned: %s`.

## Lua-to-JavaScript values and function calls

The Lua WebFrame registration builder at raw `0xCCB9A0` installs twelve
named entries. Each closure uses a small forwarding stub; the `jmp` targets
identify these implementation entry points:

| Lua method | Stub (raw) | Implementation (raw) |
|---|---:|---:|
| `LoadUrl` | `0xCCE8B0` | `0xCCABA0` |
| `GetUrl` | `0xCCE8C0` | `0xCCACC0` |
| `AddWebCallback` | `0xCCE8D0` | `0xCCAD70` |
| `AddWebValue` | `0xCCE8E0` | `0xCCAF10` |
| `CallWebFunc` | `0xCCE8F0` | `0xCCB060` |
| `SetUrlFilters` | `0xCCE900` | `0xCCB1C0` |
| `LogWebRequests` | `0xCCE910` | `0xCCB3D0` |
| `SetNaturalDims` | `0xCCE920` | `0xCCB5B0` |
| `ClearCookies` | `0xCCE930` | `0xCCB4E0` |
| `GetNaturalDims` | `0xCCE940` | `0xCCB710` |
| `EnableTypeTarget` | `0xCCE950` | `0xCCB7C0` |
| `IsTypeTarget` | `0xCCE960` | `0xCCB8C0` |

The registration table and forwarding stubs establish the names and entry
points. The following traces cover the URL, dimensions, and type-target
bindings; `ClearCookies`'s indirect target remains only partially understood.

The Lua binding at raw `0xCCAF10` implements `AddWebValue`, using the usage
string at VA `0x1BC7734`. It requires the frame, property name, and value
arguments. The property name may be a string or number; the numeric path goes
through raw `0xCD6550`. It dispatches through virtual slot `+0x54` on the
frame-associated object. The matching vtable entry at VA `0x1C76650` points to
raw `0xD651B0`, which marshals the Lua value through `0xCBBDB0` and calls
`JSObject::SetPropertyAsync` (IAT VA `0x1AC4EF0`) when its web object is
available. The property is set asynchronously; this path is distinct from
custom-method registration.

The binding at raw `0xCCB060` implements `CallWebFunc`, with usage string at
VA `0x1BC761C`. It requires a method name in addition to the frame argument,
and accepts a string or numeric name. It dispatches through virtual slot
`+0x58`; the matching entry at VA `0x1C76654` points to raw `0xD65250`.
That routine resolves the JavaScript expression
`typeof Red5Callbacks == "undefined" ? null : Red5Callbacks`, checks the
requested name with `JSObject::HasMethod` (IAT VA `0x1AC4EF4`), and stops
without invocation if the callbacks object or function is unavailable. For a
found function, it marshals Lua stack arguments starting at index 3 through
`0xCBBDB0` into a `JSArray`. The shared state at VA `0x1E686C4` offset `+8`
selects `JSObject::InvokeAsync` (IAT VA `0x1AC4EEC`) when nonzero and
`JSObject::Invoke` (IAT VA `0x1AC4EDC`) otherwise. The synchronous return
value is used for the function-return diagnostic; it is not shown being
returned to Lua. Diagnostics include the missing-function and unavailable-
object formats at VAs `0x1BD49A8` and `0x1BD4A10`.

`LoadUrl` at raw `0xCCABA0` accepts a string directly or converts a numeric
Lua value to text. After validating the frame, it checks the associated object
at offset `+0x418`; when present, it calls that object's virtual slot `+0x5C`
with the URL string and pushes Lua `true`. If the associated object is null,
the function returns without pushing a value. The invalid-frame branch uses
`Invalid Element - Element Destroyed?` at VA `0x1BC7484`. In the identified
WebFrame vtable at VA `0x1C765FC`, slot `+0x5C` points to raw `0xD65690`.
That routine returns if its nested view pointer at `+0x20` is null, otherwise
calls the nested view's virtual slot `+0x74` before preparing the URL. A
case-sensitive eight-byte comparison against `local://` (VA `0x1BD49F8`)
selects a special branch that strips the prefix and passes the suffix, a base
string at `this+0x4C`, and `/` (VA `0x1BD4988`) through string/path helpers.
Other URLs go through raw `0x2EE810`. Both branches pass the resulting
temporary to the IAT helper at VA `0x1AC4EF8` and then call nested-view slot
`+0x64`. The meanings of the preliminary `+0x74` call and the exact temporary
URL type remain unresolved.

`GetUrl` at raw `0xCCACC0` reads the current URL pointer at frame offset
`+0x448`, substituting the empty string at VA `0x1C5EA68` when the pointer is
null, then returns one Lua string. `SetNaturalDims` at raw `0xCCB5B0`
converts two arguments to floats and compares them with the stored width and
height at `+0x45C` and `+0x460`. It writes both fields and calls raw `0xCCC5C0`
only if either differs. The comparison is ordinary floating-point inequality,
so equal NaN values still take the update path. `GetNaturalDims` at raw
`0xCCB710` returns those same two fields as Lua numbers.

The `SetNaturalDims` notification helper, raw `0xCCC5C0`, derives a fit scale
from viewport dimensions at `+0x420/+0x424` and natural dimensions at
`+0x45C/+0x460`. For dimensions at least `1`, it uses the smaller applicable
viewport-to-natural ratio; if neither natural dimension reaches `1`, it
starts from `1.0`. The ratio is clamped to `[0.2, 5.0]`, multiplied by `100`,
rounded by adding `0.5` before integer conversion, and stored at `+0x464`.
When the associated object at `+0x418` exists, the helper reads its current
value through slot `+0x38` and writes the new integer through slot `+0x3C`
only when they differ. The API behind those slots is not named by this sample.

`EnableTypeTarget` at raw `0xCCB7C0` converts its extra Lua argument using
truthiness and calls raw `0xCCD0F0`. That helper does nothing when the stored
flag at frame offset `+0x459` already matches; otherwise it updates the byte.
If the pointer at `+0x398` is nonzero and equals the associated object at
`+0x418`, the helper also calls the global target dispatcher with the
associated object plus `0x730` when enabling, or null when disabling.
`IsTypeTarget` at raw `0xCCB8C0` returns whether the byte at `+0x459` is
nonzero. Its referenced usage string at VA `0x1BC7834` says
`FRAME:IsTypeTarget(boolean)`, although the implementation reads no
additional Lua argument; this appears to be a misleading usage string.

The `SetUrlFilters` binding at raw `0xCCB1C0` requires the frame and at least
one filter. It accepts string and numeric values; numbers pass through the
Lua number-to-text helper. The resulting list is passed to raw `0xCCCB20`,
which replaces the list stored at frame offset `+0x468`. Each nonempty filter
is retained only when `strncmp(last_character, "*", 1)` returns zero; other
values are skipped and logged with `SetUrlFilters: Illegal URL filter: %s`.
The usage example at VA `0x1BC7698` ends in `*`, consistent with this check.
The stored patterns are consumed by raw `0xCCCC10`: it iterates the list at
`+0x468`, computes each pattern length minus one, and compares that many
leading bytes against candidate text with case-sensitive `strncmp` (IAT VA
`0x1AC4894`). The first equal prefix is a match. Thus the trailing `*` is
treated as a prefix wildcard, not as a general glob; `*` alone has a zero-byte
prefix and matches any candidate. An empty pattern does not match because
its length-minus-one underflows the unsigned bound check, although the setter
rejects it before storage.

The callback at raw `0xCBDDE0` prepares candidate text via helpers including
the call at `0x1AC4E98` and raw `0xCBB5E0`, then invokes `0xCCCC10`. If its
context key is unchanged it returns false before matching. Otherwise it
returns the inverse of the matcher result: false for a matching filter and
true when no filter matches. The surrounding virtual callback's externally
visible meaning is not named by this sample, so the return is documented
without inferring whether it means allow, cancel, or redirect. Constructor
raw `0xCBC410` writes vtable VA `0x1C747CC` at object offset `0` and VA
`0x1C747B8` at object offset `+0x04`. The secondary table is at file offset
`0x18733B8`; its first entry is VA `0x10BDEC0` (raw `0xCBD2C0`) and its
second entry at table `+0x04` is VA `0x10BE9E0` (raw `0xCBDDE0`). This proves
the filter callback is installed in the object's secondary vtable, but the
owner's API type and callback contract remain unidentified. The first entry
`0xCBD2C0` performs substantial work on an incoming object, but its normal
epilogue always zeros `EAX` and returns after consuming one stack argument;
its external callback meaning remains unknown. Setup routine raw
`0xBFB310` calls factory `0xCBB7A0` with values from owner fields `+0x9A4`
and `+0x9A8`, stores the returned object at `+0xA04`, then calls its primary
vtable slot `+0x08` with a computed flag. The factory allocates `0x6C` bytes
and invokes constructor `0xCBC410` when allocation succeeds. The two input
fields' application-level meaning and the flag's meaning are not established.
Owner cleanup routine `0xBF8AC0` invokes the object at `+0xA04` through its
vtable slot `+0x10` with argument `1`, then clears that field. It also
releases the values at `+0x9A8` and `+0x9A4` through the shared-text release
path and clears both fields, supporting their identification as
reference-counted text values but not their application-level purpose.
Before building the first factory argument, setup copies its incoming path
value into owner field `+0x5C8`. For nonempty input, it ensures a trailing
slash or backslash and converts forward slashes to backslashes; for empty
input, it uses `"./"` at VA `0x1BBA0FC`. If field `+0x9A4` is empty, the
literal `"web"` is composed with this base path. This supports a root-path
and web-subdirectory interpretation, while the precise API contract remains
unidentified.
When those fields are empty, setup uses fallback addresses `0x1BBA15C` and
`0x1BBA160`, which contain `"web"` and `"./"` respectively. The first value
is path-composed with owner field `+0x5C8` before it is passed to the factory.
These strings suggest path components, but their application-level purpose
is not established.
The primary table is at file offset `0x18733CC`; its `+0x08` entry is VA
`0x10BCD80` (raw `0xCBC180`). That method first calls raw `0xCBC270`, then
uses the helper object at member `+0x08` twice to populate members `+0x0C`
and `+0x10` from the configuration at `+0x14`. When its flag is nonzero, it
also calls primary-vtable slot `+0x04`. This identifies the immediate
initialization work, but not the user-facing meaning of the helper fields.
Setup computes that flag as `2` only when both its stack-byte argument and
owner byte `+0x5C` are nonzero; otherwise it passes `0`. Primary slot `+0x00`
at VA `0x1C747CC` points to raw `0xCBC150`, which calls the helper at object
`+0x08`'s virtual slot `+0x18`. Primary slot `+0x04` points to raw `0xCBC160`,
which similarly calls virtual slot `+0x18` on nonnull objects at `+0x0C` and
`+0x10`. These methods appear to release helper objects, but their allocator
or interface semantics are not established.
The constructor initializes a string-like member at `+0x24` with capacity
field `+0x2C` equal to `7`. `CBC180` calls `CBC270` before producing its two
results. `CBC270` returns unless mode and storage pointer `+0x28` are both
nonzero; for mode other than `1`, it sets pending byte `+0x68` when helper
`+0x08` exists and result `+0x0C` is absent. Reset routine `CBC510`, called
through wrapper `CBC4D0`, checks that byte, calls `CBC270` with mode `2`,
clears the byte, and releases/reset fields including the string-like member.
This supports a deferred state-update/cleanup interpretation, but the
application-level meaning of that state is still unknown.

`LogWebRequests` is registered to raw `0xCCB3D0` and takes a value after the
frame. Although its usage string at VA `0x1BC7584` says `boolean`, the
implementation applies Lua truthiness without a boolean-type check and stores
the result in the byte at frame offset `+0x458`.

`ClearCookies` is registered to raw `0xCCB4E0`. Its binding checks the frame
and calls virtual slot `+0x68` on the internal object at frame offset
`+0x41C`. The initializer at raw `0xCCBD90` constructs that object through
raw `0xD64060` and stores the result at `+0x41C`; the constructor sets its
vptr to VA `0x1C765FC`. The vtable entry at VA `0x1C76664` (slot `+0x68`)
points to the eight-byte forwarding thunk at raw `0xD65A10`. That thunk loads
a nested pointer from internal-object offset `+0x1C`, reads its vptr, then
calls that object's virtual slot `+0x04`. The constructor initializes the
nested pointer from the result of global object's virtual slot `+0x0C`.
This resolves the first dispatch and confirms the thunk target statically;
the nested object's runtime type and slot `+0x04` semantics remain unresolved.
The binding's missing-argument string at VA `0x1BC75D4` says
`usage: FRAME:LogWebRequests()`, which conflicts with the registered
`ClearCookies` name and is treated as a stale or copied usage string, not
evidence to rename the method.

The installed `WebTest.lua`/HTML pair defines
`Red5Callbacks.test_from_lua` and calls it through `CallWebFunc`, corroborating
the native target selection. The evidence establishes the generic
`Red5Callbacks` dispatch, not any call to `Red5.errorlog`.

## Bridge teardown

The bridge's deleting-destructor wrapper is raw `0xD64180`; it calls
`0xD64250` and performs the conditional storage release. The destructor calls
cleanup routine `0xD64710` before destroying its member containers. Cleanup
drains the pending queue at `this+0x4C` while its count at `this+0x5C` is
nonzero. For each queued record it calls raw `0xD648A0`, which destroys the
record's `JSArray` at `+8` and `JSValue` at `+4`, then decrements the retained
method-name text reference at `+0`; zero-reference storage is released through
the corresponding allocator path. The queue item is then freed. This cleanup
path does not call the callback `Invoke` or `InvokeAsync` imports.

Before queue draining, cleanup resets a field at `this+0x24` to `-1` and
checks the pointer at `this+4` against a shared sentinel stored at
`dword_1E002B8+0x5D0`. It calls the pointed object's virtual slot zero with
argument `1` only when the pointer is non-null and differs from the sentinel,
then clears the field. A pointer equal to the sentinel is neither released nor
cleared. The inert model captures these three pointer-cleanup outcomes; the
pointer's semantic type remains unidentified.

If lookup falls through, the handler compares the callback method name
against the UTF-16 literal `errorlog` at VA `0x1BD4994` using the imported
`WebString::operator==` at VA `0x1AC4EA4`. A different method name is logged
as an unknown callback. For `errorlog`, an empty argument array or a
non-string first argument returns without emitting a message. Otherwise the
handler reads argument zero, verifies it is a string through the imported
`JSValue::IsString`, converts it with `JSValue::ToString`, and passes it to
the logger helper at raw `0x2FC4F0`. The format at VA `0x1BD498C` is
`%s - %s`; the first value is the frame label at `[frame+0x1C]+0x70` when
non-null, otherwise an empty-string pointer at VA `0x1C5EA68`. The logger
object is at VA `0x1E68688`.

The unrelated/unknown callback message uses format VA `0x1BD48F0`
(`uiWebFrame='%s', unknown callback was used by Javascript: %s`) and logger
object VA `0x1E68668`, through the helper at raw `0x2FCC60`. The JavaScript
code that invokes `Red5.errorlog` remains untraced; the callback's routing,
callable-like argument handling, mapped callback queue dispatch, and
errorlog string-to-logger handoff are established by the native
instructions.

The separate `WebPreferences::WebPreferences` import has one direct call
site at raw `0xCBC1B1` in function `0xCBC180`. That routine then initializes
several stack fields and delegates to `0xCBC270`; this observed call does
not, by itself, establish any logging preference or connect it to `errorlog`.

## Lua/web bridge corroboration

The installed GUI test harness demonstrates the generic bidirectional bridge.
`system\gui\components\Test\WebTest.lua` loads
`local://awesomium_test.html`, exposes values with `AddWebValue`, and registers
`myfunc` and `MoveMe` with `AddWebCallback` (lines 20, 43-50). Its
`TestCallback` reads a string and nested array/object arguments, then returns
a Lua table with nested values (lines 24-39). The paired
`system\gui\web\awesomium_test.html` calls `Red5.myfunc(...)` with a trailing
JavaScript callback and calls `Red5.MoveMe()` (lines 58 and 90); it also
defines `Red5Callbacks.test_from_lua`, which the Lua side calls through
`CallWebFunc` (Lua line 54; HTML line 65). This confirms a generic
JavaScript-to-Lua callback, its optional JavaScript continuation, and the
Lua-to-JavaScript function path. The native
`AddWebCallback` registration and Lua callback dispatch are now traced above,
but this test does not identify a call to `Red5.errorlog`.

The main UI Lua source gets `ingame_host` from an operator setting
(`components\MainUI\Web\Mainframe\Mainframe.lua`, line 91) and loads
`host .. "/panelmanager"` (line 716); panel URLs are also loaded by that
frame. A case-insensitive search of the installed `system\gui` text/script
sources found no `errorlog` call. Therefore the exact caller is not present
in the inspected local GUI sources; dynamically loaded page code remains a
possible source, not a confirmed caller.

The source identity used for the call-site checks is
`FirefallClient.exe.original`, 40,197,848 bytes, SHA-256
`E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235`.
The IDA-analyzed client image is SHA-256
`97362D668FF64778193A64759265E4AE90842043C4A15B627DF9902FD9135445`;
the callback instructions, vtable entries, imports, and string constants
were checked against the original sample.
