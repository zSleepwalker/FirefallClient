# Awesomium exported child entry and role dispatch

Static, read-only cross-binary trace of the identified awesomium.dll and
FirefallProcess.exe samples. Hashes/sizes and preferred-VA conventions
are recorded in [the companion ledger](CompanionBinaryConnections.md).
The DLL image base is `0x10000000`; the EXE base is `0x400000`.
No browser subprocess or target code was executed.

## Export identity and entry gate

The export is **not** literally named AwesomiumProcessMain:

| Export | Ordinal | RVA | Relationship |
|---|---|---|---|
| `?ChildProcessMain@Awesomium@@YAHPAUHINSTANCE__@@@Z` |217 | `0x3A710` | C++ entry imported directly by FirefallProcess |
| `_Awe_ChildProcessMain@4` |339 | `0x4C840` | C wrapper calling the C++ entry |

Neither export is forwarded. The wrapper's call is at `0x1004C845`;
its complete stub bytes at `0x1004C840` are
`8B 44 24 04 50 E8 C6 DE FE FF 83 C4 04 C2 04 00`.
FirefallProcess calls IAT slot `0x4060DC` at `0x40103E`
(`FF 15 DC 60 40 00`).

ChildProcessMain calls IsChildProcess (`0x1003A630`) at `0x1003A719`.
The helper obtains the parsed `type` switch value through `0x1028C8A0`
and checks its length, not merely whether the switch exists.
The zero-length comparison starts at **`0x1003A6B1`**
(`83 7C 24 30 00 74 25`). The entry's result test starts at
**`0x1003A721`** (`84 C0 74 45`); false reaches `0x1003A76A`
(`33 C0 5E 83 C4 60 C3`) and returns zero before content main.
The sample-byte checks corrected one-byte address transcription errors
in the initial analysis report.

Thus absent/empty parsed type returns zero through either export
wrapper. This does not imply that every nonempty value is a known role.
Switch syntax/tokenization and other command-line preprocessing are
outside the saved decision model.

The [switch-value follow-up](AwesomiumSwitchValuePolicy.md) establishes
that bare `--type` and explicit `--type=` both supply empty values and
do not consume a following argv token as that value. It is not a full
argv grammar or duplicate-switch policy.

## Content-main role table

Dispatcher `0x10064940` uses the table at `0x1124D400`.
Each entry is8 bytes: key VA DWORD at `+0`, callback VA DWORD at `+4`.

| Index | Entry VA | Key VA / exact key | Callback VA |
|---|---|---|---|
|0 | `0x1124D400` | `0x11240E81` / empty | `0x100AF940` |
|1 | `0x1124D408` | `0x11265E38` / `renderer` | `0x101BCD70` |
|2 | `0x1124D410` | `0x11265CEC` / `plugin` | `0x1019E1D0` |
|3 | `0x1124D418` | `0x11265FBC` / `worker` | `0x1020A320` |
|4 | `0x1124D420` | `0x11265D88` / `ppapi` | `0x101A4FA0` |
|5 | `0x1124D428` | `0x11265D0C` / `ppapi-broker` | `0x101A5190` |
|6 | `0x1124D430` | `0x11265F34` / `utility` | `0x10209660` |
|7 | `0x1124D438` | `0x11265BB4` / `gpu-process` | `0x1019B1C0` |

The first two callbacks contain BrowserMain and RendererMain log labels.
Other callback internals were not traced, so their roles are identified
only by the exact table keys, not invented implementations.

The byte comparator at `0x1000A5D0` is called at `0x10064997`.
The surrounding `0x1006499F..0x100649AF` tests both bytes and equal
full lengths before loading the callback at key-slot+4 and calling it
at `0x100649F7` (`FF D0`). Matching is exact and case-sensitive:
`Renderer`, `renderer `, and `renderer-extra` do not select renderer.

The empty-key BrowserMain mapping is real in the content dispatcher,
but the exported child entry's nonempty-type gate excludes it.
This is not a claim that BrowserMain is unreachable through every DLL
entry point.

## Unknown types and delegate boundary

On an unmatched nonempty type, `0x100649C5` tests the supplied delegate.
A nonnull delegate selects its vtable slot `+0x10`
(`0x100649D0..0x100649D7`), with no known callback or fixed return value
assigned by the saved model.

Without a delegate, `0x10064A07` sets the return value to1 and the
epilogue at `0x10064A70` returns it. This path contains the diagnostic
`Check failed: false. Unknown process type: ` and source-file text
`app\content_main.cc`. Consequently unknown types do not invariably
produce that no-delegate result.

`src\Awesomium\ChildProcessRoleNativeLayout.cs` saves the eight pointer
entries and separate exported-entry/content-dispatch decisions. It
does not parse argv, resolve callbacks, load a DLL, or call a delegate.
CompanionTools checks both export identities, the FirefallProcess IAT,
entry bytes, every table key/pointer, comparison branches, fallback
branches, and pure exact-match/unknown/empty decisions.
