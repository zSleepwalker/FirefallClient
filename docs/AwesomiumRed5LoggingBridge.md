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

For a selected entry, the consumer obtains the separately stored callback
value and assembled argument array. When the callback value is undefined,
as tested through imported `JSValue::IsUndefined` (IAT VA `0x1AC4EE8`), it
skips invocation. Otherwise it converts the value to a JSObject and invokes
that object's `call` method
(UTF-16 literal at VA `0x1BD47B4`) with a null receiver followed by the
prepared callback arguments. A state check at global VA `0x1E686C4` offset
`+8` selects imported `JSObject::InvokeAsync` (IAT VA `0x1AC4EEC`) when
nonzero, and `JSObject::Invoke` (IAT VA `0x1AC4EDC`) otherwise. The
synchronous path can log the return value. Conditional debug logs use
`Executing callback '%s' from JavaScript` and `   #%d: %s`; this path's
observed return format is `Javascript callback returned: %s`.

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
JavaScript-to-Lua callback and Lua-to-JavaScript function path, but it does
not identify a call to `Red5.errorlog` or prove which native method implements
`AddWebCallback`.

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
