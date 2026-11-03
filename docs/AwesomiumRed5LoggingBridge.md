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
The mapped path creates a
12-byte record holding a retained lookup result, a `JSValue`, and a `JSArray`.
It examines the final array argument, not the first. When that value is an
object, raw `0xD64320` applies a cached JavaScript predicate whose ASCII
source is at VA `0x1BD4570`:
`(function (object) { return !!(object && object.constructor && object.call && object.apply); })`.
The helper invokes the predicate's `call` method (method-name literal
`call` at VA `0x1BD4544`) with `null` and the candidate object. If the result
is defined and truthy, the handler stores that trailing value separately and
pops it from the copied argument array; otherwise it preserves the full
array. Raw `0xD66B70` appends the record to the handler's collection at
`this+0x34`. The later consumer of this queue remains unresolved.

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
object VA `0x1E68668`, through the helper at raw `0x2FCC60`. The mapped
callback's queue consumer and the JavaScript code that invokes `Red5.errorlog`
remain untraced; the callback's routing, callable-like argument handling,
and errorlog string-to-logger handoff are established by the native
instructions.

The separate `WebPreferences::WebPreferences` import has one direct call
site at raw `0xCBC1B1` in function `0xCBC180`. That routine then initializes
several stack fields and delegates to `0xCBC270`; this observed call does
not, by itself, establish any logging preference or connect it to `errorlog`.

The source identity used for the call-site checks is
`FirefallClient.exe.original`, 40,197,848 bytes, SHA-256
`E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235`.
The IDA-analyzed client image is SHA-256
`97362D668FF64778193A64759265E4AE90842043C4A15B627DF9902FD9135445`;
the callback instructions, vtable entries, imports, and string constants
were checked against the original sample.
