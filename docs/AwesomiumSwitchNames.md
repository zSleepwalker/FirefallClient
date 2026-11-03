# Awesomium switch-name conversion and ASCII value getter

Static inspection of `awesomium.dll`, 23,470,048 bytes, SHA256
`DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E`.
Addresses are preferred PE VAs (image base `0x10000000`).
`src\Awesomium\SwitchNameNativeLayout.cs` saves per-code-unit/byte
transforms and predicates, not an argv parser or Unicode codec.

## Parsed name path

For tokens already admitted by the [switch-scanning state](AwesomiumSwitchAdmission.md),
splitter `0x1028BC60` separates the wide name and value at the first
equals sign. Parsed-switch loop call `0x1028DDCF` converts the name with
`0x1029A9C0`, then call `0x1028DDDC` supplies that narrow name and the
separate original wide value to map builder `0x1028D7F0`.

The builder copies the name and calls lowercase helper `0x10007AB0` at
`0x1028D82F`. That helper iterates the string's stored length, not an
early-NUL scan. The bytes `0x41`-`0x5A` alone become `0x61`-`0x7A`;
punctuation, NUL and bytes >= `0x80` remain unchanged. The byte-range
test and add are at `0x10007AD6`-`0x10007AE6`. It is not locale-aware
lowercasing, Unicode case folding or a Win32 character conversion.

The builder obtains the recognized prefix length through `0x1028BB70`
at `0x1028D86D`, copies the folded name starting after that prefix at
`0x1028D893`, and looks up/inserts that remaining key at `0x1028D8A0`.
The existing duplicate-key capture still applies after this normalization:
the last admitted value for the same key wins.

The lookup path also lowercases its supplied narrow key:
`0x1028C480` copies the query name, calls `0x10007AB0` at `0x1028C4C4`,
and searches the map at `0x1028C4D4`.

Thus admitted ASCII name variants `--TYPE` and `--type` can address the
same stored `type` key. This does **not** make their values case-insensitive.
At `0x1028D8A5`-`0x1028D8AB` the map builder copies the original wide
value directly into the selected value slot; it does not apply the name
lowercase helper to that value.

## Narrow conversion and its diagnostic boundary

Converter `0x1029A9C0` optionally checks that each source WORD is <= `0x7F`.
The diagnostic path is enabled only when the observed global at
`0x11596AF8` equals 1 and helper `0x10288E40` returns a signed value <= 3.
A non-ASCII WORD then constructs the diagnostic
`Check failed: IsStringASCII(wide). ` from `string_util.cc`, line 423,
and calls its cleanup/destructor at `0x1029AA58`.

Whether that diagnostic cleanup can terminate or otherwise interrupt
execution has not been traced. The capture records **admission to that
diagnostic**, not a guarantee of continuation for non-ASCII names.

If control reaches `0x1029AA5D` onward, it calculates source end as
base + 2*stored length and calls `0x1000B350` at `0x1029AA99`. That range
copy reads one low byte at `0x1000B3A0`, appends it at `0x1000B3F2`, and
advances the source by two bytes at `0x1000B40B`. No UTF-8 encoding or
replacement-character conversion occurs on this returning path.
`CaptureNarrowedCodeUnit` therefore preserves just the low 8 bits of a
supplied WORD. A synthetic WORD `0x0141` would narrow to `0x41`, which
the subsequent name-fold helper would change to `0x61`; this is an
arithmetic example **conditional on returning from any diagnostic**, not
a promised usable non-ASCII switch alias.

## ASCII getter for the child-role value

The child-type path uses `0x1028C8A0`, not a generic wide-value getter.
It first retrieves the stored wide value via `0x1028C480`, then calls the
ASCII predicate `0x10299E00` at `0x1028C8CF`. The predicate compares
**whole WORDs** against `0x7F` at `0x10299E1D`, accepts an empty string,
and rejects any non-ASCII code unit, including surrogates.

On rejection, `0x1028C8DB`-`0x1028C8E9` constructs an empty narrow
result. On admission, `0x1028C8FF` calls the narrowing converter. ASCII
values therefore retain their bytes and case; non-ASCII values become
empty rather than being low-byte-truncated by this getter. The existing
child-process nonempty-value gate then rejects that empty result.

This distinction is important: the generic converter's optional diagnostic
and low-byte copy do not describe the ASCII value getter's rejection rule.
Neither should be replaced with Unicode lowercasing or UTF-8 conversion.

## Evidence and bounds

Companion checks bind name conversion/folding/prefix removal, original-wide
value assignment, lookup folding, diagnostic guards, low-byte loads and
two-byte stride, whole-WORD ASCII admission and empty-result branches to
the hash-guarded sample. Pure cases exhaust all 256 byte-fold inputs and
cover non-ASCII WORD boundaries, embedded NUL, surrogates and diagnostic
thresholds.

No sample was executed or modified. Full tokenization, all command-line
consumers, malformed string-object state, allocation failures and the
diagnostic destructor's behavior remain outside this capture.
