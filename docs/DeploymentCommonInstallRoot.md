# DeploymentCommon product identity and install root

Static, read-only analysis of the identified DeploymentCommon.dll
(76800 bytes, SHA256
`33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0`).
These methods were read through PEReader/System.Reflection.Metadata.
No WPF application was started and no actual registry values were read.

| Method | MethodDef token | Body RVA | Direct observation |
|---|---|---|---|
| get_ProductIdentifier | `0x0600002A` | `0x260E` | Constant `Firefall_Beta` |
| get_ProductIdentifierDisplay | `0x0600002B` | `0x2615` | Constant `Firefall` |
| GetInstallLocation | `0x06000031` | `0x2658` | WPF property override, then registry |
| GetInstallRegistryKey | `0x06000044` | `0x2A0C` | HKLM product subkey and string-only GetValue |

This sample's registry/product identity is therefore **not** its display
name. The [shortcut templates](DeploymentCommonShortcuts.md) resolve
to `Play Firefall.lnk` and `Uninstall Firefall.lnk` for this getter,
while the registry accessor opens
`HKLM\Software\Red 5 Studios\Firefall_Beta\`.

## Override and registry precedence

GetInstallLocation checks Application.Current.Properties for `installDir`
at IL `0x0000..0014`. A present value is cast to string (castclass,
not conversion). A nonnull/nonempty value is selected and returned.
Otherwise the method calls GetInstallRegistryKey("InstallLocation") at
IL `0x005F..0069`.

The registry helper formats `Software\Red 5 Studios\{0}\` using the
constant product identity and calls Registry.LocalMachine.OpenSubKey.
Missing key returns null. A present key's GetValue result is processed
with isinst string: missing/nonstring values return null, rather than
being formatted or throwing a cast exception. A nonstring WPF override,
in contrast, can throw InvalidCastException. There is no catch in these
methods to silently replace that error with a registry fallback.

The selected nonempty string has **every exact case-sensitive occurrence**
of ` --culture` removed using String.Replace. This is not command-line
parsing: `C:\override --culture=en-US` becomes
`C:\override=en-US`; the value after `=` is not consumed. Multiple
occurrences are all removed, differently cased ` --Culture` survives,
and no separate whitespace trim occurs.

Override emptiness is tested **before** replacement. A nonempty override
consisting only of the removable substring can therefore return empty
without falling back to registry. Registry null remains null; registry
empty remains empty.

## Scope of the saved pure model

`InstallLocationNativeLayout.cs` preserves constants and a
single-observation selection/transformation model. Inputs represent
already observed property/registry values; it does not fetch them.
The native IL reads Application.Current.Properties repeatedly, and the
saved model does not simulate concurrent changes between those reads,
an absent Application.Current, registry access errors, or COM consumers.
Those are not silently caught by the reconstructed routine.

CompanionTools uses only synthetic paths and integer values, verifies
the differing string-cast behaviors and replacement edge cases, and
checks all four complete method IL bodies and their user-string tokens
against the hash-guarded assembly.
