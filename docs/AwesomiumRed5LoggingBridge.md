# Red5 JavaScript logging-method setup

Static inspection of `FirefallClient.exe` at its original loaded image
addresses; no browser, script, logger, or callback was run.

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

The `SetCustomMethod` import has three observed call sites: this `errorlog`
registration, a registration in raw function `0xD64530` using a caller-
supplied method name and trailing zero, and one in raw function `0xD651B0`
using caller-supplied strings. Both latter functions require the `Red5`
setup helper to report ready; their higher-level method semantics are not
assigned.

The separate `WebPreferences::WebPreferences` import has one direct call
site at raw `0xCBC1B1` in function `0xCBC180`. That routine then initializes
several stack fields and delegates to `0xCBC270`; this observed call does
not, by itself, establish any logging preference or connect it to `errorlog`.

The source identity used for the call-site checks is
`FirefallClient.exe.original`, 40,197,848 bytes, SHA-256
`E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235`.
The IDA-analyzed client image is SHA-256
`97362D668FF64778193A64759265E4AE90842043C4A15B627DF9902FD9135445`;
the recorded differences do not overlap this batch's code or string evidence.
