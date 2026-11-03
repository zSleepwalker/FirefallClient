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

## Caller extent and no-configuration fallback

Caller helper `0x1028A790` first tests diagnostic mode exactly1 and signed
minimum severity<=3. With supplied extent0 it can construct severity3
diagnostic `N > 0U` (`logging.cc`, line424), then continues on returning
paths. It does not convert extent0 into a safe empty span or return early.
After such a diagnostic it reloads the minimum threshold, so a callback's
possible state changes must not be hidden by a stale modeled observation.

With configuration pointer `0x11596AFC` nonzero, instructions
`0x1028A814-0x1028A81E` decrement the supplied extent modulo32 and pass
that explicit source byte length to rule lookup `0x102B53A0`. It does not
recompute strlen. A synthetic extent0 therefore yields `0xFFFFFFFF`, not0;
no source access safety is implied.

One representative real caller at `0x1006D799` supplies literal
`browser\download\download_manager_impl.cc` (41 bytes) and extent42,
supporting interpretation as a source-literal extent including NUL.
That caller tests the returned signed level>=20 and constructs severity-20
logging when admitted. This is representative evidence, not a claim that
every caller uses a valid literal extent.

With no configuration, `neg eax; cmp eax,-1; jg` at
`0x1028A82B-0x1028A846` returns **max(unchecked(-minimum),-1)**.
This is different from the configured lookup's raw fallback negation.
For minimum3 the no-configuration result is-1 but an empty configured rule
vector returns-3. For minimum `Int32.MinValue`, negation wraps back to
that minimum: no configuration returns-1, configured no-match returns
`Int32.MinValue`. The later signed CMP replaces flags from NEG; do not
interpret NEG overflow as changing that comparison.

`VerboseLevelNativeLayout.cs` saves these arithmetic/admission boundaries
from supplied observations only. It never dereferences a source pointer,
constructs diagnostics or assumes callbacks return.

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
[Rule construction](AwesomiumVerboseConstruction.md) separately records
comma/equals boundaries, retained outputs on error, and duplicate append
order. Dynamic updates/concurrent mutation remain outside this capture.
