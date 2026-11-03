# HWStats MAC-address source and selection

Static inspection of `HWStats.exe`, 594,648 bytes, SHA256
`0AE559DA740DE2C51A77A652906F021833A17E2A634757C15A6ADE5BEB9D679D`.
Addresses are preferred PE VAs (base `0x400000`).
`src\HWStats\MacAddressNativeLayout.cs` saves inert request facts and
first-success selection over supplied return bytes. It neither calls
Netbios nor parses an NCB or adapter-status buffer.

## Request sequence

Report function `0x404340` calls helper `0x4041D0` at `0x4043C2`, with a
stack destination at `[ebp-0x14]`. The helper resolves `NETAPI32!Netbios`
through IAT `0x440584` and performs these synchronous requests:

| Stage | Native evidence | Request facts |
|---|---|---|
| Enumerate | `0x4041E6`-`0x404227` | Clear 64-byte NCB; command `0x37` (NCBENUM), stack buffer, length 256 |
| Each enumerated LANA | `0x404240`-`0x40426E` | Clear NCB; load enumeration byte at index+1; command `0x32` (NCBRESET) |
| Adapter status | `0x404270`-`0x4042C8` | Clear NCB again; same LANA; command `0x33` (NCBASTAT), wildcard call name, shared buffer VA `0x48EFC0`, length 600 |

Observed NCB field offsets are command 0, buffer pointer 4, WORD length 8,
call name 10 and LANA byte 48. The call-name source at `0x443C6C` is an
asterisk followed by fourteen spaces and a NUL; `strcpy_s` is given a
16-byte destination capacity.

The enumeration and reset API return bytes are ignored. The enumeration
buffer itself is not cleared before the call. The loop trusts its observed
first byte as the count; an enumeration failure must not be reconstructed
as a reliable empty list.

## First successful status, not a preferred adapter

At `0x4042CA` the helper tests **Netbios's returned AL byte**, not a
separate NCB return-code field. Zero branches to the success copy at
`0x4042F1`. Nonzero advances the enumeration index and compares against the
count at `0x4042CE`-`0x4042D8`.

There is no physical/virtual-adapter preference or address-content filter
on this path. The first adapter-status call returning zero wins, in the
enumerated order. The pure source takes already-observed ASTAT return
bytes and returns that attempt's index, or null when none succeeds. It
does not fabricate enumeration or reset results.

Success copies exactly six bytes from `0x48EFC0`-`0x48EFC5` into destination
offsets 0-5 (`0x4042F1`-`0x404322`), then returns 1. Zero count or exhausted
attempts reaches `0x4042DE`, returning 0 **without clearing or writing the
destination**.

## Unchecked caller output

The report's zeroed state does not cover its six-byte MAC destination:
the XMM stores at `0x404373`-`0x404387` end at `[ebp-0x1D]`, while the MAC
starts at `[ebp-0x14]`. The helper result is ignored; the next instruction
at `0x4043C7` starts loading destination bytes for printing. Thus a failed
helper does not establish a deterministic all-zero address.

All six bytes are zero-extended into variadic integer arguments and passed
to `0x406A20` at `0x4043EE`. The literal at `0x443BB4` is:

```text
macaddr = %02x:%02x:%02x:%02x:%02x:%02x\n
```

The code records the exact format and source-copy policy but does not
reproduce an output line from unknown failure-path stack bytes.

## Evidence and bounds

Hash-guarded companion checks verify the import, NCB clear size, commands,
buffer lengths, LANA loads, wildcard, AL success branch, index/count loop,
six individual copies, unchecked caller and output literal. Pure cases
cover empty/all-failed attempts and multiple zero returns to preserve
first-success precedence.

No sample was executed, no adapters were enumerated or reset, and no
actual hardware address was collected. The capture does not infer a
stable address across enumeration order or a successful result on any
particular machine.
