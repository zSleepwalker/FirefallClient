# Awesomium conversion-diagnostic routing

Static inspection of `awesomium.dll`, 23,470,048 bytes, SHA256
`DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E`.
Addresses are preferred PE VAs (base `0x10000000`).
`src\Awesomium\ConversionDiagnosticNativeLayout.cs` saves inert
severity-three routing and display decisions. It does not emit a log,
display UI, spawn a helper or invoke callbacks.

This refines the diagnostic boundary in
[switch-name conversion](AwesomiumSwitchNames.md). The optional
non-ASCII check at `0x1029AA0E` supplies severity **3**, not 4, to
constructor `0x10289D30`. The constructor stores severity at object +0,
source filename at +0xA4, line at +0xA8, and a GetLastError observation at
+0xAC. Converter call `0x1029AA58` invokes destructor `0x10289EC0`.

## Early interceptor precedes severity dispatch

The destructor finishes/extracts its message, then checks callback pointer
`0x11596B1C` at `0x10289EEC`. If present it calls the callback at
`0x10289F12` with severity, source filename, line, object +0xA0 and the
message string object. A nonzero returned AL skips subsequent built-in
logging and severity handling, joining cleanup at `0x1028A0D3` or
`0x1028A0DB`.

The saved route therefore distinguishes an observed consumed result from
both custom severity handling and default handling. It does not predict
whether any callback exists, returns, or consumes this diagnostic.

## Severity 3 is not the severity-4 path

When not consumed, the destructor performs configured logging before
severity dispatch. The check at `0x1028A042` handles severity 4 separately.
The converter's severity 3 reaches `0x1028A07A`.

If pointer `0x11596B18` is nonnull, the destructor copies its message and
calls that custom handler at `0x1028A09A`. Otherwise it passes the copied
message to default helper `0x10288EA0` at `0x1028A0AC`. Both call sites have
normal cleanup continuations; neither return value is used as a decision.
The custom handler's implementation and behavior remain unknown.

For ordinary returning paths the destructor restores its saved LastError
through imported SetLastError during cleanup, before destroying its stream
state. This is not a guarantee of preserved error state when a callback
does not return.

## Default helper: optional display, not unconditional termination

At `0x10288EBC` and `0x10288EC6`, the default helper returns immediately if
the original narrow message is empty or global enable byte `0x11596B13`
is zero. Otherwise it obtains the current module filename, truncates
after its final backslash when one is found, and appends the wide literal
`debug_message.exe` at `0x1127B0C0`.

It converts the message using `0x10296210`; an empty converted string
also returns without display (`0x10288F46` onward).
[Message-conversion evidence](AwesomiumDiagnosticEncoding.md) captures
scalar validation, U+FFFD replacement and ignored aggregate-error status
without implementing the full byte decoder.

For a nonempty converted message it attempts `CreateProcessW` at
`0x10288FA9`, using the constructed helper path as the application and
the converted message as the command line. Inheritance and creation flags
are zero on this path. There is no added quoting policy in the capture.

| Observed creation result | Native continuation |
|---|---|
| Nonzero | `WaitForSingleObject(process, INFINITE)` at `0x10288FBA`, then CloseHandle on thread and process at `0x10288FCB`/`0x10288FD2` |
| Zero | `MessageBoxW` at `0x10288FF2`, title `Fatal error` (`0x1127B0A8`), flags `0x40010`, null owner |

Wait, close and message-box return values are not interpreted before
cleanup. There is no observed exit-code query or unconditional process
termination call in this default helper. Successful child creation can
block indefinitely waiting for that helper, and a dialog can wait for
interaction. Thus the original converter must still **not** be described
as guaranteed to continue for non-ASCII input. A consumed interceptor,
disabled display, or a normally returning default helper does have a
normal path back to its low-byte conversion.

## Evidence and bounds

Hash-guarded companion checks bind severity construction, LastError
imports/storage, early-interceptor branches, distinct severity checks,
handler/default call sites, display gates, helper filename, creation/wait/
close calls and fallback UI arguments. MessageBoxW's slot `0x1157BDC8`
is a USER32 delay import in this sample; ordinary import checks cover the
kernel APIs, with literal instruction guards for the delayed call.
Pure cases exhaust the routing/display boolean combinations and both
creation outcomes.

No sample, helper, callback, UI or API was executed. Callback installation,
full logger sink behavior, path-building API failure handling,
`debug_message.exe` internals and diagnostic message encoding remain
outside this capture.
