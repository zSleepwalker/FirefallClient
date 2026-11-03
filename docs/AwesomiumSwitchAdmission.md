# Awesomium switch admission around positionals

Static, read-only analysis of the identified `awesomium.dll` (23,470,048
bytes, SHA-256
`DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E`,
preferred image base `0x10000000`). The DLL was not executed or loaded.

An ordinary positional argument does **not** disable later switch parsing.
The exact `--` argument does: it is sent to the positional-argument path, and
the parser's sticky switch-scan state remains off for every later argument.
Thus `--type=renderer positional --type=plugin` admits the later `type`
switch, while `--type=renderer -- --type=plugin` does not. This is limited to
the switch-admission loop; it is not a general argv parser or a statement about
other command-line consumers.

## Evidence

All addresses are preferred-image VAs, not file offsets. `SampleImage` maps
them through the PE image base and section table.

1. The caller at `0x1028DFA6` invokes `sub_1028DCD0`. That function walks the
   argument-record array in 28-byte steps, beginning after its first record
   (`0x1028DD27`, `0x1028DD32`, `0x1028DD4F`). It initializes one byte of
   switch state to 1 before the loop at `0x1028DD0F`; it does not reinitialize
   that state for each argument.
2. Each argument is copied and passed through `sub_1029B520` at
   `0x1028DD5E`. The loop then compares the whole resulting string against the
   UTF-16 literal `--` through `sub_10014B20` at `0x1028DD95`. The comparator
   returns zero only for an equal-length, equal-content match: for a string
   longer than `--`, equal leading characters still return nonzero because
   the compared lengths differ; any differing character also returns
   nonzero. The instructions at `0x1028DD9A` (`test eax,eax; setnz dl`),
   `0x1028DD9F` (`and` into the saved state), and `0x1028DDA3` (conditional
   branch) therefore clear the state only for the exact marker, and never
   restore it. If the state is clear, control branches around switch splitting
   to the positional append at `0x1028DE03`.
3. If scanning remains enabled, `sub_1028BC60` is called at `0x1028DDB4`.
   Its prefix validator `sub_1028BB70` is called at `0x1028BCAD`. That helper
   checks the table at `0x1127B404`, whose UTF-16 strings are `--`, `-`, and
   `/`; it returns zero when none matches. A non-switch positional token
   therefore falls through to the same positional append at `0x1028DE03`,
   **without** changing the scan state. A later `--type` token passes the
   validator, then reaches name conversion at `0x1028DDCF` and the parsed
   switch map builder at `0x1028DDDC`.
4. For the exact `--` token, the whole-string comparison clears the state
   before the prefix validator can run. That marker itself is appended as a
   positional argument; the persisted zero state sends all following tokens,
   including `--type`, down the positional path instead of to the map.

The comparison follows `sub_10014B20`'s instructions as well as its
decompilation. The marker is UTF-16 at `0x1127B3F8`; the prefix pointer table
at `0x1127B404` contains pointers to the three UTF-16 prefix literals. The
captured methods in `SwitchAdmissionNativeLayout` model only this state
transition and the admitted/not-admitted decision once the caller has already
classified an exact marker and a recognized switch prefix. They do not parse
or normalize strings.

## Pure capture cases

```csharp
SwitchAdmissionNativeLayout.CaptureNextSwitchScanningState(true, false) == true
&& SwitchAdmissionNativeLayout.CaptureCurrentSwitchMapAdmission(true, false, false) == false
&& SwitchAdmissionNativeLayout.CaptureCurrentSwitchMapAdmission(true, false, true) == true
&& SwitchAdmissionNativeLayout.CaptureNextSwitchScanningState(true, true) == false
&& SwitchAdmissionNativeLayout.CaptureCurrentSwitchMapAdmission(true, true, true) == false
&& SwitchAdmissionNativeLayout.CaptureNextSwitchScanningState(false, false) == false
&& SwitchAdmissionNativeLayout.CaptureCurrentSwitchMapAdmission(false, false, true) == false
```

In particular, after a plain positional argument, call the state capture with
`(true, false)` and then test a later recognized `--type` with
`(true, false, true)`. After the exact marker, the subsequent switch is tested
with scanning state `false`.

## `SampleImage` evidence checks

These are inert PE-byte/call-site checks for `CompanionTools`; they do not
execute the sample. `browser` is a `SampleImage` constructed with the DLL size
and SHA-256 stated above.

```csharp
browser.Calls(0x1028DFA6, SwitchAdmissionNativeLayout.ParsedSwitchLoopVa)
&& browser.Matches(0x1028DD0F, "C644240B01")
&& browser.Calls(SwitchAdmissionNativeLayout.EndOfSwitchComparisonCallVa, 0x10014B20)
&& browser.Matches(0x1028DD9A, "85C00F95C2205424137455")
&& browser.Calls(SwitchAdmissionNativeLayout.SwitchSplitCallVa, 0x1028BC60)
&& browser.Calls(SwitchAdmissionNativeLayout.PrefixValidatorCallVa, 0x1028BB70)
&& browser.Calls(0x1028BBF4, 0x10014B20)
&& browser.Matches(SwitchAdmissionNativeLayout.EndOfSwitchMarkerVa, "2D002D000000")
&& browser.Matches(SwitchAdmissionNativeLayout.SwitchPrefixTableVa, "F0B32711ECB32711E8B32711")
&& browser.Matches(0x1127B3E8, "2F0000002D0000002D002D000000")
&& browser.Calls(0x1028DDCF, 0x1029A9C0)
&& browser.Calls(SwitchAdmissionNativeLayout.SwitchMapBuilderCallVa, 0x1028D7F0)
&& browser.Calls(SwitchAdmissionNativeLayout.PositionalAppendCallVa, 0x10155F50)
```
