# Awesomium verbose rule source and precedence

Static inspection of `awesomium.dll`, 23,470,048 bytes, SHA-256
`DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E`.
Addresses are preferred PE VAs (image base `0x10000000`).
`VerboseRuleNativeLayout.cs` saves source-byte slicing, path/module choice
and selection from **already observed** match results. It does not parse a
`vmodule` string or implement the native wildcard matcher.

## Stored rule and lookup order

Constructor `0x102B5720` builds rules in its parsed-pair iteration order,
appending through `0x102B5650`; that helper copies a narrow string, Int32
level at +`0x1C`, and source-kind DWORD at +`0x20`, then advances vector end
by36 bytes. The constructor sets source-kind1 when the pattern contains
either slash or backslash (find helper `0x1012F1D0`, call `0x102B58A7`,
result converted to a Boolean at `0x102B58B1` onward).

Lookup `0x102B53A0` scans from vector begin toward end in36-byte steps.
For source-kind exactly1 it passes the original source span to the matcher;
for every other DWORD it passes a derived module span. First successful
match immediately returns that rule's level; it does not search for a more
specific or later matching rule. Empty/no-matching rules return unchecked
negation of the current minimum-severity DWORD reached through object+`0x10`,
not a cached constructor-time default.

The [logging-setup trace](AwesomiumLoggingSetup.md) separately records the
minimum-threshold pointer and the later `log-level` override. This lookup
does not undo that override or average/merge rule levels.

## Derived module span

Helper `0x102B5210` uses the last occurrence of either separator, retains
only the following bytes, strips from the **last** dot onward, then removes
an exact trailing four-byte `-inl` suffix if present. All operations use
explicit byte spans. Search helpers `0x102993E0` and `0x10299370` search
backward; slice helper `0x102994D0` clamps an absent-dot sentinel to span
length. The suffix comparator `0x1000A5D0` compares bytes exactly, without
case folding.

| Observed source bytes as ASCII | Module bytes |
|---|---|
| `dir\widget-inl.cc` | `widget` |
| `dir/widget.test.cc` | `widget.test` |
| `widget-INL.cc` | `widget-INL` |
| `.hidden` | Empty |
| `dir/` | Empty |
| `widget-inl` | `widget` |

No filesystem path normalization occurs; a slash only determines the
span's basename boundary. High bytes and embedded NUL are ordinary stored
bytes here, not decoded Unicode or a new terminator.

## Matcher evidence, not a replacement implementation

Matcher `0x102B52B0` compares literal bytes case-sensitively at
`0x102B52F6`. Pattern `/` or `\` accepts either source separator.
`?` consumes one source byte; `*` skips repeated stars and recursively
tries the remaining pattern at successive source positions. It is not
Win32 filename matching, a case-insensitive glob, or a Unicode-aware matcher.
There is no separator exclusion on its star retry path; full-path star
matching is not bounded to a single directory component.

This static observation does not add a runnable matcher, resource bound or
complete malformed-span model. Saved rule selection consumes supplied
match observations, so it must not be used to claim actual wildcard matches
that were not established independently.

## Evidence and bounds

Hash-guarded instruction checks bind reverse searches, last-dot slice,
`-inl` comparison, source-kind switch,36-byte iteration, first-match return,
and live fallback negation. Additional guards record matcher literal,
separator, question-mark and recursive-star branches. Pure cases exercise
only the saved byte transforms and selection, including first-match wins
and Int32 minimum negation wraparound.

No sample, logger, wildcard parser, file API or callback was executed.
Rule-text split/duplicate handling and dynamic updates/concurrent mutation
remain outside this capture.
