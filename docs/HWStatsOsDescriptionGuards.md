# HWStats OS-description admission and API guards

Static inspection of `HWStats.exe`, 594,648 bytes, SHA256
`0AE559DA740DE2C51A77A652906F021833A17E2A634757C15A6ADE5BEB9D679D`.
Addresses are preferred PE VAs (base `0x400000`).
`src\HWStats\OsDescriptionNativeLayout.cs` saves pure availability and
admission decisions, not a Windows-version query or stream formatter.

## Entry and version fields

Builder `0x403A20` saves its caller's wide destination pointer at
`0x403A4B`, initializes the product-code slot to zero at `0x403A59`,
clears the 284-byte extended version structure and sets its size to 284.
It calls `GetVersionExW` through `0x440160` at `0x403A9F`. A zero BOOL
branches to `0x403F41`, returning false without writing the destination.

After a successful version query, it obtains `kernel32.dll` with
`GetModuleHandleW`, resolves `GetNativeSystemInfo`, and uses that export
when nonnull. Otherwise `0x403ADA` calls imported `GetSystemInfo`. The
first module-handle result is forwarded directly to `GetProcAddress`;
there is no separate module rejection before this fallback decision.

Only then does `0x403AE0` require platform ID 2, and `0x403AED` require an
**unsigned** major version above 4. Both rejection branches return false
without copying the destination. The source models observed API fields,
not an assertion that these fields identify the machine's actual current
Windows version.

## Major-six product-information gate

`0x403B1F`-`0x403B26` restricts the `GetProductInfo` lookup to major
version exactly 6. Other admitted majors, including values above 6, skip
this lookup; that does not prove that the formatter recognizes their
marketing names.

For major 6, `0x403B90` obtains `kernel32.dll` again. A null handle
(`0x403B96`) or missing `GetProductInfo` export (`0x403BAA`) branches to
`0x403CBA`: destroy the temporary stream, return false, and do not copy the
partially constructed description to the caller.

If present, `0x403BC9` calls the export with observed major/minor, **zero
service-pack major/minor arguments**, and the product-code output slot.
It ignores the API BOOL and immediately dispatches on the observed output
code at `0x403BCB`. There is no failure-result rejection at this point.
Although the slot initially contains zero, a failed call is not modeled
as necessarily preserving it.

The pure `CaptureGate` returns `ContinueFormatting`, not "success", after
the known guards pass. It does not simulate API execution, allocation,
CRT failures, edition-name selection or the remaining stream operations.

## Final copy and unchecked caller

Normal formatting reaches `0x403F0C`, imported `wcscpy_s`, with capacity
argument `0x1000` wide characters and the saved caller destination.
Its return is not tested before setting the builder's return byte to 1.
The System-section caller uses a source buffer at `[ebp-0x1298]`, followed
by its next wide buffer at `[ebp-0x298]`: a 4096-byte / **2048-character**
region. The supplied copy capacity is therefore not the actual size of
that caller region. Exact maximum formatted length has not been
established; this capture records the mismatch without implementing that
copy or asserting a demonstrated overflow.

At `0x40404A` the System section calls the builder, then `0x40404F`
immediately clears EAX rather than testing AL. Its subsequent wide format
call at `0x4040FC` consumes the source buffer even when the builder returned
false. That buffer is not initialized before the call. A rejected build
therefore does not justify an empty or "Unknown Windows" description in
the reconstruction.

## Evidence and bounds

Hash-guarded companion checks cover structure size, API imports, both
module literals, export names, fallback/rejection branches, major-six
restriction, product-call arguments and unchecked result, copy capacity,
return markers and unchecked caller. Pure cases cover guard precedence,
old/non-NT versions, missing module/export, other admitted majors, and
native-info fallback.

Exact OS/edition/service-pack/build text and CRT locale-dependent output
remain separate follow-ons. No target was executed, no host OS or registry
was queried, and no caller buffer was written.
