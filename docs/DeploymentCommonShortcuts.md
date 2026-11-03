# DeploymentCommon shortcut property plans and cleanup

Static, read-only IL reconstruction of the identified DeploymentCommon.dll
(76800 bytes, SHA256
`33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0`).
The methods were inspected as file bytes, never invoked. No COM objects,
shortcuts, registry entries, files, or directories were created/deleted.

| Method | MethodDef token | Body RVA / file offset | IL RVA / length |
|---|---|---|---|
| Utils.CreateShortcut | `0x06000125` | `0x66A0 / 0x48A0` | `0x66AC / 423` |
| Utils.RemoveShortcuts | `0x06000126` | `0x6854 / 0x4A54` | `0x6860 / 429` |

These are relative to the same `.text` mapping used in the
[managed native-utility trace](DeploymentCommonNativeUtilities.md).
The scratch IL inspector resolves framework/COM member and string tokens
without loading the target assembly. Complete method IL byte comparisons
are saved in the CompanionTools checks.

## Creation

CreateShortcut queries GetInstallLocation and combines it with
`system\bin` (`IL_000A..0019`), then constructs WshShellClass. It gets
the selected desktop via GetCommonPath(25,16) and the selected Start Menu
via GetCommonPath(22,11). Those calls have the previously documented
native-BOOL-discard and exception-only backup behavior.

The method performs three IWshShell3.CreateShortcut calls, casts each
result to IWshShortcut, assigns properties, and calls Save:

| Location / name | Target relative to install root | Working directory | Explicit icon | Explicit arguments |
|---|---|---|---|---|
| Selected desktop / `Play {display}.lnk` | `system\bin\Launcher.exe` | `system\bin` | `system\bin\FirefallClient.exe` | Not assigned |
| Selected Start Menu / `Red 5 Studios\{display}\Play {display}.lnk` | `system\bin\Launcher.exe` | `system\bin` | `system\bin\FirefallClient.exe` | Not assigned |
| Same Start Menu product folder / `Uninstall {display}.lnk` | `uninstall\FirefallInstaller.exe` | `uninstall` | Not assigned | `/uninstall` |

Here `{display}` is BuildInformation.ProductIdentifierDisplay.
Descriptions are `Play {display}` / `Uninstall {display}`.
The uninstall root is built from a second GetInstallLocation call at
`IL_014B`, not from the earlier bin directory.

The Start Menu product folder is created only when Directory.Exists
returns false (`IL_00B2..00C2`). The explicit COM property setters and
Save sites are desktop `IL_004B..008E`, Start Menu play
`IL_00E5..0128`, and uninstall `IL_015C..019B`.
No icon setter occurs for uninstall, and no arguments setter occurs for
the play links. **Not assigned does not mean cleared**: existing shortcut
behavior or COM defaults are not inferred from the absence of a setter.

`ShortcutNativeLayout.CaptureCreation` therefore returns property
descriptors rather than invoking COM or manufacturing absolute paths.
Its null icon/arguments fields mean the setter is absent. Relative
paths describe the observed install-root composition, not a path
sanitizer or a complete general Path.Combine implementation.

## Removal is broader than creation

RemoveShortcuts first checks/removes the selected common-or-backup
desktop play link (`IL_000A..0035`), then the corresponding Start Menu
play/uninstall links. It removes the product folder only if it exists
and GetFileSystemEntries returns an empty array, then separately applies
the same check to the `Red 5 Studios` parent (`IL_00A6..00D7`).

It then repeats link and empty-folder cleanup for the **explicit current
user** DesktopDirectory (16) and StartMenu (11), obtained directly via
Environment.GetFolderPath (`IL_00D8..01A1`). Thus it also removes older
user-specific links even if the first pass selected common folders.
There is no deduplication if fallback caused the first pass to select
the same user paths.

Each File.Delete is guarded by File.Exists. Each Directory.Delete
passes recursive=false and is guarded by existence and zero remaining
entries. Other products or unrelated files prevent parent deletion.
There is no method-local catch region in either routine; failures in
COM/filesystem calls can propagate and prevent later steps.

Pure reconstruction saves property plans, existence gates, and
nonrecursive empty-directory decisions. It does not execute this cleanup.
The [product/install-root follow-up](DeploymentCommonInstallRoot.md)
resolves this sample's display name to constant `Firefall` and registry
identity to `Firefall_Beta`, and records installDir precedence and the
literal culture-substring replacement. The descriptors accept an explicit
display input for isolated template testing; those synthetic alternatives
do not establish alternate native getter results.
