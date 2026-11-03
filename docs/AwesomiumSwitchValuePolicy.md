# Awesomium bare and explicit-empty switch values

Static, read-only follow-up on the identified awesomium.dll
(23470048 bytes, SHA256
`DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E`,
preferred base `0x10000000`). No DLL code, browser subprocess, or live
command line was executed or captured.

The [exported-child gate](AwesomiumChildProcessRoles.md) depends on the
**parsed value length** of key `type`, not mere switch presence.
The key literal is VA `0x11265DD8`. This trace resolves one earlier
unknown: how an isolated bare or empty `type` switch gets its value.

Command initialization calls GetCommandLineW at `0x1028E234`
(`FF 15 EC 04 24 11`) and CommandLineToArgvW at `0x1028E19E`
(`FF 15 98 BC 57 11`). The argv parser recognizes the wide double-dash
literal at `0x1127B3F8` and uses splitter `0x1028BC60`, searching for
the wide equals literal at `0x1127B400`.

The splitter initializes value length field `+0x10` to zero at
`0x1028BC98` (`89 75 10`, from the zero-initialized register).
If equals is absent, `0x1028BD26` (`83 FE FF 74 44`) bypasses suffix
copying while retaining successful switch recognition. If equals is
the final character, the copied suffix has zero length. The resulting
value is passed into the map-value copy sequence at `0x1028D8A5`
(`6A FF 53 57 8B C8 E8 F0 A6 D7 FF`).

| Recognized isolated token pattern | Stored switch value | Following argv token |
|---|---|---|
| `--type` | Empty | Not consumed as its value |
| `--type=` | Empty | Not consumed as its value |
| `--type=renderer` | Suffix `renderer` | Not consumed as its value |

Consequently, a bare `--type` followed by a separate `renderer` token
does not acquire renderer as its value. With no other `type` declaration,
the existing length gate at `0x1003A6B1` rejects that empty
value and the exported entry returns zero.

This is not a complete command-line specification. Other recognized
prefixes, quoting behavior supplied by CommandLineToArgvW, duplicate-key
precedence, and name normalization have not been reconstructed here.
It does not establish that double-dash is the only accepted prefix.

`CommandSwitchValueNativeLayout.cs` saves only the length/next-token
policy once a switch and separator have already been recognized.
It implements no argv parser and performs no native calls.
CompanionTools verifies the field store, absent-separator branch,
value-copy sequence, literals, initialization calls, and synthetic
suffix-length cases. The role table and delegate decisions remain
separate from this value-extraction boundary.
