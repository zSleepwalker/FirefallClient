# DeploymentCommon native window and shell utility boundaries

Read-only metadata/IL analysis of the identified DeploymentCommon.dll
(76800 bytes, SHA256
`33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0`).
System.Reflection.Metadata and PEReader inspect its file bytes; the target
assembly is never loaded or executed.

## Imports, not reconstructed callers

| MethodDef token | Module / import | Signature blob |
|---|---|---|
| `0x06000104` | user32.dll / ClientToScreen | `00 02 02 18 10 11 75` |
| `0x06000105` | user32.dll / MoveWindow | `00 06 02 18 08 08 08 08 02` |
| `0x06000127` | shell32.dll / SHGetSpecialFolderPath | `00 04 02 18 12 65 08 02` |

These are static P/Invokes, with RVA0 and ImplMap flags `0x0100`
(WinApi calling convention). No SetLastError or explicit charset flags
are recorded for these rows. ClientToScreen's second parameter is a
by-ref `System.Drawing.Point` (TypeRef row29); its metadata does not
mark it Out. SHGetSpecialFolderPath's StringBuilder path parameter is
marked Out; its parameters are hwndOwner, lpszPath, nFolder, fCreate.

A bounded scan of every managed method body's IL found no direct
call/callvirt/ldftn/ldvirtftn byte candidates for the two window-import
tokens. This does not exclude reflection, other assemblies, or
unexamined dynamic usage. No window-positioning algorithm is inferred
merely because those imports exist.

## GetCommonPath preserves the native BOOL-discard behavior

`Utils.GetCommonPath` is MethodDef `0x06000128`, body RVA `0x6A10`
(file offset `0x4C10`), with12-byte fat header
`1B 30 04 00 5D 00 00 00 47 00 00 11`.
Its93 IL bytes begin at RVA `0x6A1C` / file offset `0x4C1C`.

The method constructs a StringBuilder with capacity260, then calls
SHGetSpecialFolderPath(IntPtr.Zero, builder, directory, false). The
returned BOOL is immediately popped, not tested. If no managed exception
is thrown, it returns builder.ToString() even when the native API returned
false. Therefore the managed SpecialFolder backup is **not** a normal
native-failure fallback.

The method body's System.Exception catch region covers IL `[0,34)` and
has handler `[34,84)`. The handler logs exception details through two
managed logging calls, then reaches
Environment.GetFolderPath(specialFolderBackup). The normal path's
leave instead selects the already captured builder string. This is
observed behavior, not a recommendation to discard native failures.

The shortcut routines pass the following pairs to GetCommonPath:

| Shell directory argument | Managed backup argument | Observed callsites |
|---|---|---|
|25 (common desktop directory) |16 (DesktopDirectory) | CreateShortcut `0x06000125`, RemoveShortcuts `0x06000126` |
|22 (common Start Menu) |11 (StartMenu) | Same methods |

Corresponding IL sequences are `1F 19 1F 10 28 28 01 00 06`
and `1F 16 1F 0B 28 28 01 00 06`.
This establishes inputs, not complete shortcut creation/removal semantics.
Those routines also have user-specific folder paths and COM interactions
which remain separate traces.

`NativeUtilityNativeLayout.cs` preserves identities and the result-source
decision without importing user32/shell32, reading paths, or creating
folders. CompanionTools checks the imports' metadata signatures/flags,
the complete helper IL, its catch region, and pure native-BOOL/exception
combinations. The shared metadata checker also verifies the previously
traced [WOW64 imports](DeploymentCommonWow.md).
