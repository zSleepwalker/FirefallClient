# Awesomium duplicate switch values

Static, read-only analysis of the identified `awesomium.dll` (23,470,048
bytes, SHA-256
`DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E`,
preferred image base `0x10000000`). No DLL code was executed or loaded.

For switch entries that reach the parsed switch map with the same normalized
key bytes, **the later entry replaces the earlier value**. Thus repeated
`--type` entries that map to the same `type` key use the value from the last
such entry admitted to the map. A later empty value replaces a nonempty one.
This complements [the switch-value extraction trace](AwesomiumSwitchValuePolicy.md),
which establishes how each individual token's value is split.

## Evidence chain

All addresses below are preferred-image VAs backed by the PE mapping used by
`SampleImage`.

1. `sub_1028DEB0` calls the switch-collection routine `sub_1028DCD0` at
   `0x1028DFA6`. That routine walks the supplied argument records in order,
   handles entries admitted through its `--` check, splits them with
   `sub_1028BC60` (`0x1028DDB4`), converts the name with
   `sub_1029A9C0` (`0x1028DDCF`), then calls map builder `sub_1028D7F0`
   (`0x1028DDDC`).
2. The builder calls `sub_1028D310` at `0x1028D8A0` to locate the key's map
   slot. Before lookup it preprocesses the key with `sub_10007AB0` at
   `0x1028D82F`. That routine calls tree lookup `sub_100F7110`; when the
   candidate is not the end sentinel, it compares the requested key with the
   stored key through `sub_1000A900` at `0x1028D34E`. The comparator uses the
   bytewise `sub_1000A5D0` comparison and lengths. For identical normalized
   key bytes, the comparison is equal: the `jns` at `0x1028D355` takes the
   existing-entry path, which returns the node's value slot at
   `0x1028D444` (`lea eax,[ebp+28h]`) rather than inserting another node.
3. Back in `sub_1028D7F0`, the builder unconditionally calls
   `sub_10007FA0` at `0x1028D8AB`, copying the current value into the slot
   returned by lookup. The argument loop's order therefore makes the last
   admitted assignment the map's stored value.
4. The `type` accessor confirms this map is the source used for the child
   process type value: `Awesomium::IsChildProcess` at `0x1003A630` constructs
   key `"type"` (VA `0x11265DD8`) and calls `sub_1028C8A0` at `0x1003A694`.
   That getter reaches `sub_1028C480`, which applies the same
   `sub_10007AB0` preprocessing at `0x1028C4C4`, searches the same tree, and
   copies the mapped value from node offset `+0x28`.

This conclusion is intentionally limited to repeated entries that are
actually admitted to the map and produce identical normalized key bytes.
It does not specify aliases, differently normalized names, or how tokens
after a non-switch argument are handled; it is not a general argv parser or
a Chromium role/runtime analysis.

## Suggested `SampleImage` checks

These are sample-evidence guards for `CompanionTools`; they do not execute the
sample. The source-level capture in `DuplicateSwitchNativeLayout` is only the
pure, same-normalized-key replacement decision.

```csharp
sample.Calls(0x1028DFA6, 0x1028DCD0)
&& sample.Calls(0x1028DDB4, 0x1028BC60)
&& sample.Calls(0x1028DDCF, 0x1029A9C0)
&& sample.Calls(0x1028DDDC, 0x1028D7F0)
&& sample.Matches(0x1028DE67, "FF442414")
&& sample.Matches(0x1028DE80, "83C51C")
&& sample.Matches(0x1028D332, "3B6E047424837D20108B4D1C8D450C72028B00")
&& sample.Calls(0x1028D34E, 0x1000A900)
&& sample.Matches(0x1028D353, "85C00F89E3000000")
&& sample.Matches(0x1028D444, "8D4528")
&& sample.Calls(0x1028D82F, 0x10007AB0)
&& sample.Calls(0x1028D8A0, 0x1028D310)
&& sample.Calls(0x1028D8AB, 0x10007FA0)
&& sample.Calls(0x1003A694, 0x1028C8A0)
&& sample.Ascii(0x11265DD8) == "type"
&& sample.Calls(0x1028C4C4, 0x10007AB0)
&& sample.Calls(0x1028C4D4, 0x100F7110)
&& sample.Calls(0x1028C585, 0x10007FA0)
```

Pure cases for parent integration:

```csharp
DuplicateSwitchNativeLayout.CaptureSameNormalizedKeyValue("renderer", "plugin") == "plugin"
&& DuplicateSwitchNativeLayout.CaptureSameNormalizedKeyValue("renderer", "") == ""
```
