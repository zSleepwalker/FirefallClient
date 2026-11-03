# Awesomium diagnostic message conversion boundary

Static inspection of `awesomium.dll`, 23,470,048 bytes, SHA256
`DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E`.
Addresses are preferred PE VAs (image base `0x10000000`).
`src\Awesomium\DiagnosticEncodingNativeLayout.cs` captures scalar
validation/replacement and UTF-16 append decisions, not a byte-stream
decoder, parser or implementation of the complete conversion.

## Wrapper does not reject the conversion-error flag

Default diagnostic display helper `0x10288EA0` calls `0x10296210` at
`0x10288F3E`, supplying a narrow data pointer and stored length.
The wrapper initializes an empty wide string, calls reservation helper
`0x102B7A10`, then conversion loop `0x10295F10` at `0x1029623E`.
It ignores that loop's returned success byte and returns the output
string object. The display caller tests the resulting wide string length,
not the conversion-success flag.

The reservation helper clears output length and establishes a capacity
hint based on byte length and whether the first byte has its high bit
set. It is not the text conversion. The loop separately starts an
aggregate success byte at 1 and a cursor at zero. At `0x10295F21` it
uses a **signed-positive** length test; zero and synthetic DWORD lengths
with their high bit set do not enter the loop. Allocation behavior for
such synthetic lengths is not modeled or guaranteed to reach that test.

## Scalar reader and malformed-input boundary

Loop `0x10295F30` calls scalar reader `0x102B78A0`. It reads ASCII
directly; other leading bytes are screened at `0x102B78B2`-`0x102B78BC`,
with admitted leads delegated to `0x102B7D10` in mode -1. The shared
multibyte decoder uses lead-byte counts, six-bit continuation assembly
and minimum-value checks: this is a UTF-8 decoding path, not the
switch-name low-byte truncation helper.

This capture does not implement that decoder or claim exact replacement
counts for arbitrary malformed byte sequences. The shared decoder can
advance the cursor across multiple bytes, and those error-consumption
rules remain outside the saved model.

After decoding, `0x102B78DE`-`0x102B78F1` accepts exactly:

```text
scalar < 0xD800 OR (scalar >= 0xE000 AND scalar <= 0x10FFFF)
```

The surrogate range and values above `0x10FFFF`, including failure
sentinel `0xFFFFFFFF`, are rejected. The predicate alone does not reject
Unicode noncharacters such as `0xFDD0`, `0xFFFE` or `0xFFFF`.
Byte-sequence validity remains the reader's separate responsibility;
passing an arbitrary synthetic scalar to the capture does not prove a
particular byte sequence would decode to it.

At `0x10295F58`-`0x10295F65`, a rejected scalar appends **U+FFFD** and
sets the aggregate success byte to zero. The loop continues rather than
clearing the wide result or aborting. Later accepted scalars do not restore
that success flag. A malformed positive-length message can therefore
produce nonempty replacement-containing output even though the loop
reports failure, and the wrapper still returns it.

## UTF-16 emission

Append helper `0x102B7C10` emits an accepted BMP scalar as one WORD.
For a supplementary scalar the two stores at `0x102B7CA7` and
`0x102B7CBF` use:

```text
high = (scalar >> 10) - 0x2840
low  = (scalar & 0x3FF) | 0xDC00
```

For example `0x10000` gives `D800 DC00`, `0x1F600` gives `D83D DE00`,
and `0x10FFFF` gives `DBFF DFFF`. Replacement emits just WORD `FFFD`.
An accepted scalar zero is a stored zero WORD, not a guarantee of
preserved suffix text when a later null-terminated API consumes it.

## Evidence and bounds

Hash-guarded companion checks bind wrapper initialization/calls, ignored
aggregate return, signed loop admission, scalar validity, U+FFFD branch
and sticky flag, BMP append and pair arithmetic to native instructions.
Pure cases cover Unicode boundaries, surrogate rejection, noncharacters,
supplementary pairs, failure sentinel and signed length interpretation.

No sample, codec, helper process or display API was executed. Full
malformed-byte cursor behavior, allocation failure, callback installation
and helper command-line interpretation remain unresolved.
