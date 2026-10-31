using System.Buffers.Binary;
using System.Text;
using Firefall.Reconstruction.Startup;

internal static class StartupBinaryEvidence
{
    public static void Verify(string path, Action<string, bool> check)
    {
        using var stream = File.OpenRead(path);

        byte[] ReadAt(long offset, int count)
        {
            stream.Position = offset;
            var bytes = new byte[count];
            stream.ReadExactly(bytes);
            return bytes;
        }

        uint ReadUInt32(long offset) =>
            BinaryPrimitives.ReadUInt32LittleEndian(ReadAt(offset, 4));

        bool Matches(long offset, string hex)
        {
            var expected = Convert.FromHexString(hex);
            return ReadAt(offset, expected.Length).SequenceEqual(expected);
        }

        uint peOffset = ReadUInt32(0x3C);
        ushort sectionCount = BinaryPrimitives.ReadUInt16LittleEndian(ReadAt(peOffset + 6, 2));
        ushort optionalHeaderSize = BinaryPrimitives.ReadUInt16LittleEndian(ReadAt(peOffset + 20, 2));
        uint imageBase = ReadUInt32(peOffset + 24 + 28);
        var sections = Enumerable.Range(0, sectionCount).Select(i =>
        {
            long offset = peOffset + 24 + optionalHeaderSize + i * 40;
            return (Rva: ReadUInt32(offset + 12), RawSize: ReadUInt32(offset + 16),
                RawOffset: ReadUInt32(offset + 20));
        }).ToArray();
        long VirtualAddressToFileOffset(uint address)
        {
            uint rva = checked(address - imageBase);
            foreach (var section in sections)
                if (rva >= section.Rva && rva - section.Rva < section.RawSize)
                    return (long)section.RawOffset + rva - section.Rva;
            throw new InvalidDataException($"Virtual address {address:X8} has no file-backed PE section.");
        }
        bool ImportSlotNames(uint slotVirtualAddress, string expectedName)
        {
            uint hintNameRva = ReadUInt32(VirtualAddressToFileOffset(slotVirtualAddress));
            var name = Encoding.ASCII.GetBytes(expectedName + "\0");
            return ReadAt(VirtualAddressToFileOffset(checked(imageBase + hintNameRva)) + 2, name.Length)
                .SequenceEqual(name);
        }
        string ReadCStringAscii(uint virtualAddress, int maximumLength)
        {
            byte[] bytes = ReadAt(VirtualAddressToFileOffset(virtualAddress), maximumLength);
            int terminator = Array.IndexOf(bytes, (byte)0);
            if (terminator < 0)
                throw new InvalidDataException($"String at VA {virtualAddress:X8} has no terminator in the bounded read.");
            return Encoding.ASCII.GetString(bytes, 0, terminator);
        }
        bool DecodedAdapterLiteral(uint address, int length, int step, int direction, byte key, string expected)
        {
            byte[] bytes = ReadAt(VirtualAddressToFileOffset(address), length);
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (byte)(unchecked((byte)(bytes[i] + direction * i * step)) ^ key);
            return bytes.SequenceEqual(Encoding.ASCII.GetBytes(expected));
        }
        bool ContainsConstructorCall(uint offset, uint target)
        {
            var bytes = ReadAt(offset, 96);
            for (int i = 0; i <= bytes.Length - 5; i++)
                if (bytes[i] == 0xE8
                    && (long)offset + i + 5 + BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(i + 1)) == target)
                    return true;
            return false;
        }
        string VtableRttiName(uint vtableVa)
        {
            uint col = ReadUInt32(VirtualAddressToFileOffset(vtableVa) - 4);
            uint td = ReadUInt32(VirtualAddressToFileOffset(col) + 12);
            var raw = ReadAt(VirtualAddressToFileOffset(td) + 8, 40);
            int end = Array.IndexOf(raw, (byte)0);
            return Encoding.ASCII.GetString(raw, 0, end < 0 ? raw.Length : end);
        }        check("Binary world scene factory resolves all three named scenes, builds and returns its root, and is stored by tfWorld",
            Matches(SceneFactoryNativeLayout.FactoryFileOffset + 0x2D, "68A0000000")
            && Matches(SceneFactoryNativeLayout.FactoryFileOffset + 0x4D, "E8AE0E0000")
            && Matches(0x9B7D22, "6828D1B9018B108B4230FFD08907")
            && Matches(0x9B7D41, "6838D2B9018B018B4030FFD0894704")
            && Matches(0x9B7D61, "6848D2B9018B108B4230FFD0833F00894708")
            && ReadAt(0x179BD28, 12).SequenceEqual("RenderScene\0"u8.ToArray())
            && ReadAt(0x179BE38, 14).SequenceEqual("WorldMapScene\0"u8.ToArray())
            && ReadAt(0x179BE48, 18).SequenceEqual("SinvironmentScene\0"u8.ToArray())
            && Matches(SceneFactoryNativeLayout.RootReturnFileOffset,
                "8BC78B4DF464890D00000000595F5E8BE55DC3")
            && Matches(SceneFactoryNativeLayout.TfWorldInitializeFileOffset + 0xA0,
                "E8EBBE360083C408894714"));
        check("Binary oRenderSystem init slot +0x34 has one direct caller and publishes oD3DRenderAPI plus texture/shader/model managers",
            Matches(SceneFactoryNativeLayout.RenderSystemInitCallSiteFileOffset - 0x26, "A170E9DF01")
            && Matches(SceneFactoryNativeLayout.RenderSystemInitCallSiteFileOffset, "FF5034")
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.RenderSystemInitialVtableVirtualAddress) + 0x34)
                == SceneFactoryNativeLayout.RenderSystemInitFileOffset + 0x400C00
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.RenderSystemInitialVtableVirtualAddress) + 0x38)
                == SceneFactoryNativeLayout.RenderSystemShutdownFileOffset + 0x400C00
            && Matches(0xED180E, "A300 06DE01".Replace(" ", ""))
            && Matches(0xED1840, "A304 06DE01".Replace(" ", ""))
            && Matches(0xED1869, "A308 06DE01".Replace(" ", ""))
            && Matches(SceneFactoryNativeLayout.RenderSystemShutdownFileOffset + 0x38, "8B0DFC05DE016A008B01FF500C")
            && VtableRttiName(SceneFactoryNativeLayout.TextureManagerVtableVirtualAddress) == SceneFactoryNativeLayout.TextureManagerRttiName
            && VtableRttiName(SceneFactoryNativeLayout.ShaderManagerVtableVirtualAddress) == SceneFactoryNativeLayout.ShaderManagerRttiName
            && VtableRttiName(SceneFactoryNativeLayout.ModelManagerVtableVirtualAddress) == SceneFactoryNativeLayout.ModelManagerRttiName);
        check("Binary oEngineSystem is built by factory slot 1 (0xE4E660) from a static factory registered in a list that bootstrap walks",
            Matches(SceneFactoryNativeLayout.EngineSystemConstructorFileOffset + 0x3E, "C7072C7CC701")
            && Matches(SceneFactoryNativeLayout.EngineSystemConstructorCallSiteFileOffset, "E8B8F8FFFF")
            && Matches(SceneFactoryNativeLayout.EngineSystemFactoryStaticInitFileOffset + 0x20, "C705A4B0E4016004E001")
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.EngineSystemFactoryVtableVirtualAddress) + 4)
                == SceneFactoryNativeLayout.EngineSystemFactoryCreateFileOffset + 0x400C00
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.EngineSystemFactoryGlobalVirtualAddress)) == 0x1C77C1C
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.EngineSystemVtableVirtualAddress))
                == SceneFactoryNativeLayout.EngineSystemInitializeSlotFileOffset + 0x400C00
            && Matches(SceneFactoryNativeLayout.FactoryListWalkerFileOffset + 0x43, "7414807E040075078B068BCEFF5004")
            && Matches(SceneFactoryNativeLayout.FactoryListWalkerCallSiteFileOffset, "E8408BFD00"));
        check("Binary factory instance list (head 0x1E4B0A8) create slots link into it and the teardown walker calls factory slot +8",
            Matches(SceneFactoryNativeLayout.FactoryListTeardownWalkerFileOffset + SceneFactoryNativeLayout.FactoryListTeardownDispatchCheckOffset, "85C9740F8B01FF50088B0DA8B0E40185C975F1")
            && Matches(SceneFactoryNativeLayout.EngineSystemFactoryCreateFileOffset + 0x8C, "894610A1A8B0E40189460C8935A8B0E401C64604018B".Substring(0, 28))
            && Matches(SceneFactoryNativeLayout.EngineSystemFactoryCreateFileOffset + 0x8C, "894610A1A8B0E40189460C8935A8B0E401"));        check("Binary render init args come from oEngineSystem window state (+0x130/0x138/0x13C/0x140) and reach oD3DRenderAPI slot +8; oShaderBuilderSystem is a static global",
            Matches(SceneFactoryNativeLayout.RenderSystemInitCallSiteFileOffset - 0x30,
                "FF75E083BF3001000001A170E9DF0151FFB7400100000F94C3B970E9DF01FFB73C010000885DDCFFB738010000FF75DCFF5034")
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.RenderApiVtableVirtualAddress) + 8)
                == SceneFactoryNativeLayout.RenderApiInitSlotFileOffset + 0x400C00
            && VtableRttiName(SceneFactoryNativeLayout.ShaderBuilderSystemInitialVtableVirtualAddress)
                == SceneFactoryNativeLayout.ShaderBuilderSystemRttiName
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.ShaderBuilderSystemGlobalVirtualAddress))
                == SceneFactoryNativeLayout.ShaderBuilderSystemInitialVtableVirtualAddress
            && VtableRttiName(SceneFactoryNativeLayout.EngineSystemVtableVirtualAddress)
                == SceneFactoryNativeLayout.EngineSystemRttiName
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.EngineSystemVtableVirtualAddress))
                == SceneFactoryNativeLayout.EngineSystemInitializeSlotFileOffset + 0x400C00);        check("Binary scene factory's named lookup goes through a statically initialized oRenderSystem getter to an oD3DRenderAPI create-scene slot",
            ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.SceneProviderGlobalVirtualAddress))
                == SceneFactoryNativeLayout.RenderSystemInitialVtableVirtualAddress
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.RenderSystemInitialVtableVirtualAddress)
                + SceneFactoryNativeLayout.SceneProviderGetterVtableOffset)
                == SceneFactoryNativeLayout.RenderApiGetterFileOffset + 0x400C00
            && ReadUInt32(VirtualAddressToFileOffset(ReadUInt32(
                VirtualAddressToFileOffset(SceneFactoryNativeLayout.RenderSystemInitialVtableVirtualAddress) - 4))
                + 12) == SceneFactoryNativeLayout.RenderSystemTypeDescriptorVirtualAddress
            && ReadAt(VirtualAddressToFileOffset(SceneFactoryNativeLayout.RenderSystemTypeDescriptorVirtualAddress) + 8,
                SceneFactoryNativeLayout.RenderSystemRttiName.Length + 1)
                .SequenceEqual(Encoding.ASCII.GetBytes(SceneFactoryNativeLayout.RenderSystemRttiName + "\0"))
            && Matches(SceneFactoryNativeLayout.RenderApiGetterFileOffset, "A1FC05DE01C3")
            && Matches(SceneFactoryNativeLayout.RenderSystemAtExitResetFileOffset, "C70570E9DF018487C701B870E9DF01C3")
            && Matches(SceneFactoryNativeLayout.RenderApiPublishFileOffset, "A3FC05DE01")
            && Matches(0xED17AA, "68B8490000E8FC673E00")
            && Matches(SceneFactoryNativeLayout.RenderApiConstructorFileOffset + 0x3C, "C707D49EC701")
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.RenderApiVtableVirtualAddress)
                + SceneFactoryNativeLayout.SceneLookupVtableOffset)
                == SceneFactoryNativeLayout.RenderApiCreateSceneFileOffset + 0x400C00
            && ReadAt(VirtualAddressToFileOffset(SceneFactoryNativeLayout.RenderApiTypeDescriptorVirtualAddress) + 8,
                SceneFactoryNativeLayout.RenderApiRttiName.Length + 1)
                .SequenceEqual(Encoding.ASCII.GetBytes(SceneFactoryNativeLayout.RenderApiRttiName + "\0"))
            && Matches(SceneFactoryNativeLayout.RenderApiCreateSceneFileOffset + 0x26, "68488F0100E8E0293900")
            && Matches(SceneFactoryNativeLayout.SceneConstructorFileOffset + 0x46, "C70734A3C701")
            && Matches(0xF104C2, "893C85D8ACED0189877C020000"));
        check("Binary oD3DScene constructor forwards its name to oScene's shared-text field and the render API reset restores a static placeholder",
            Matches(SceneFactoryNativeLayout.SceneCreateConstructorCallFileOffset, "FF75088BC856E8E4A6FEFF")
            && Matches(SceneFactoryNativeLayout.SceneConstructorNameForwardFileOffset, "8BF98B450C5089BD5CFFFFFFE83121F0FF")
            && Matches(SceneFactoryNativeLayout.SceneBaseConstructorFileOffset + 4,
                "FF75088BF18D4E08C706947AC701C7460400000000E8F2B34DFF")
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.SceneVtableVirtualAddress) - 4)
                == SceneFactoryNativeLayout.SceneCompleteObjectLocatorVirtualAddress
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.SceneBaseVtableVirtualAddress) - 4)
                == SceneFactoryNativeLayout.SceneBaseCompleteObjectLocatorVirtualAddress
            && ReadAt(VirtualAddressToFileOffset(ReadUInt32(VirtualAddressToFileOffset(
                SceneFactoryNativeLayout.SceneCompleteObjectLocatorVirtualAddress) + 12)) + 8,
                SceneFactoryNativeLayout.SceneRttiName.Length + 1)
                .SequenceEqual(Encoding.ASCII.GetBytes(SceneFactoryNativeLayout.SceneRttiName + "\0"))
            && ReadAt(VirtualAddressToFileOffset(ReadUInt32(VirtualAddressToFileOffset(
                SceneFactoryNativeLayout.SceneBaseCompleteObjectLocatorVirtualAddress) + 12)) + 8,
                SceneFactoryNativeLayout.SceneBaseRttiName.Length + 1)
                .SequenceEqual(Encoding.ASCII.GetBytes(SceneFactoryNativeLayout.SceneBaseRttiName + "\0"))
            && ReadAt(VirtualAddressToFileOffset(SceneFactoryNativeLayout.SceneDefaultCubeTexturePathVirtualAddress),
                SceneFactoryNativeLayout.SceneDefaultCubeTexturePath.Length + 1)
                .SequenceEqual(Encoding.ASCII.GetBytes(SceneFactoryNativeLayout.SceneDefaultCubeTexturePath + "\0"))
            && ReadAt(VirtualAddressToFileOffset(SceneFactoryNativeLayout.SceneRegistryFullMessageVirtualAddress),
                SceneFactoryNativeLayout.SceneRegistryFullMessage.Length + 1)
                .SequenceEqual(Encoding.ASCII.GetBytes(SceneFactoryNativeLayout.SceneRegistryFullMessage + "\0"))
            && Matches(SceneFactoryNativeLayout.RenderApiShutdownDeleteFileOffset,
                "8B0DFC05DE01C7050006DE01E030E70185C974068B016A01FF10A114BCE601C705FC05DE01F82DE701")
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.RenderApiGlobalVirtualAddress))
                == SceneFactoryNativeLayout.RenderApiStaticPlaceholderVirtualAddress);
        check("Binary oRenderSystem vtable slot +0x38 is a shutdown that deletes the render API objects and restores the static placeholder; 0xED196B is in the +0x34 init failure path",
            ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.RenderSystemInitialVtableVirtualAddress)
                + SceneFactoryNativeLayout.RenderApiInitializeVtableOffset)
                == SceneFactoryNativeLayout.RenderApiInitializeFileOffset + 0x400C00
            && ReadUInt32(VirtualAddressToFileOffset(SceneFactoryNativeLayout.RenderSystemInitialVtableVirtualAddress)
                + SceneFactoryNativeLayout.RenderSystemShutdownVtableOffset)
                == SceneFactoryNativeLayout.RenderSystemShutdownFileOffset + 0x400C00
            && Matches(SceneFactoryNativeLayout.RenderSystemShutdownFileOffset, "803D7F9BE40100578BF90F84CF000000")
            && Matches(SceneFactoryNativeLayout.RenderSystemShutdownPlaceholderStoreFileOffset - 4,
                "568B7704C705FC05DE01F82DE701C6057F9BE4010085F6")
            && Matches(SceneFactoryNativeLayout.RenderApiShutdownResetFileOffset + 10,
                "8B401C85C00F45F8A1A40EE60157684C93BE016A00"));
        check("Binary scene factory owns a RTTI-identified tfPhysicsDrawInterface and a disposable registration handle",
            Matches(0x9B8241, "6888010000E865FD8F00")
            && Matches(0x9B8255, "85C0740BFF378BC8E8DED1FFFF")
            && Matches(SceneFactoryNativeLayout.PhysicsDrawInterfaceConstructorFileOffset + 0x6A,
                "C7066809C701")
            && ReadUInt32(VirtualAddressToFileOffset(
                SceneFactoryNativeLayout.PhysicsDrawInterfaceVtableVirtualAddress) - 4)
                == SceneFactoryNativeLayout.PhysicsDrawInterfaceCompleteObjectLocatorVirtualAddress
            && ReadUInt32(VirtualAddressToFileOffset(
                SceneFactoryNativeLayout.PhysicsDrawInterfaceCompleteObjectLocatorVirtualAddress) + 12)
                == SceneFactoryNativeLayout.PhysicsDrawInterfaceTypeDescriptorVirtualAddress
            && VirtualAddressToFileOffset(
                SceneFactoryNativeLayout.PhysicsDrawInterfaceTypeDescriptorVirtualAddress) + 8
                == SceneFactoryNativeLayout.PhysicsDrawInterfaceRttiNameFileOffset
            && ReadAt(SceneFactoryNativeLayout.PhysicsDrawInterfaceRttiNameFileOffset,
                SceneFactoryNativeLayout.PhysicsDrawInterfaceRttiName.Length + 1)
                .SequenceEqual(Encoding.ASCII.GetBytes(SceneFactoryNativeLayout.PhysicsDrawInterfaceRttiName + "\0"))
            && Matches(0x9B7FE2, "518B0DA001E0018D45EC5068FCC4DF01E879BE3E00894710")
            && Matches(0x9B8502, "C7461400000000"));
        check("Binary tfPhysicsDrawInterface vtable has 14 in-text slots, a forwarding thunk, a model-release log and a global scene publish",
            Enumerable.Range(0, SceneFactoryNativeLayout.PhysicsDrawInterfaceVtableSlotCount).All(i =>
                ReadUInt32(SceneFactoryNativeLayout.PhysicsDrawInterfaceVtableFileOffset + i * 4)
                    == SceneFactoryNativeLayout.PhysicsDrawInterfaceVtableSlots[i] + 0x400C00)
            && Matches(SceneFactoryNativeLayout.PhysicsDrawInterfaceConstructorFileOffset + 0x2C, "C706B809C701")
            && Matches(SceneFactoryNativeLayout.PhysicsDrawInterfaceConstructorFileOffset + 0x67, "894644C7066809C701")
            && Matches(SceneFactoryNativeLayout.PhysicsDrawInterfaceConstructorFileOffset + 0xB6,
                "C7866C010000FFFF7F7F")
            && Matches(SceneFactoryNativeLayout.PhysicsDrawInterfaceConstructorFileOffset + 0xE8,
                "C78680010000FFFF7FFF")
            && Matches(SceneFactoryNativeLayout.PhysicsDrawInterfaceForwardThunkFileOffset, "558BEC8B015DFF6020")
            && Matches(SceneFactoryNativeLayout.PhysicsDrawInterfaceDestructorFileOffset + 0x29, "C7066809C701")
            && Matches(SceneFactoryNativeLayout.PhysicsDrawInterfaceReleaseModelFileOffset + 0x0D,
                "683408C701")
            && ReadAt(VirtualAddressToFileOffset(
                SceneFactoryNativeLayout.PhysicsDrawInterfaceReleaseModelMessageVirtualAddress),
                SceneFactoryNativeLayout.PhysicsDrawInterfaceReleaseModelMessage.Length + 1)
                .SequenceEqual(Encoding.ASCII.GetBytes(
                    SceneFactoryNativeLayout.PhysicsDrawInterfaceReleaseModelMessage + "\0"))
            && Matches(SceneFactoryNativeLayout.GlobalActiveSceneStoreFileOffset,
                "8B08A16000E0018988F8000000"));        foreach (var type in MatrixServerMessageNativeTypes.Entries)
        {
            var name = Encoding.ASCII.GetBytes(type.RttiText + "\0");
            check($"Binary incoming{type.MessageId} factory/constructor RTTI is {type.QualifiedName}",
                ReadUInt32(MatrixServerMessageNativeTypes.FactoryJumpTableFileOffset + (type.MessageId - 34) * 4)
                    == type.FactoryCaseFileOffset + 0x400C00
                && ContainsConstructorCall(type.FactoryCaseFileOffset, type.ConstructorFileOffset)
                && ReadAt(type.VtableStoreFileOffset, 1)[0] == 0xC7
                && ReadUInt32(type.VtableStoreFileOffset + 2) == type.VtableVirtualAddress
                && ReadUInt32(VirtualAddressToFileOffset(type.VtableVirtualAddress) - 4)
                    == type.CompleteObjectLocatorVirtualAddress
                && ReadUInt32(VirtualAddressToFileOffset(type.CompleteObjectLocatorVirtualAddress) + 12)
                    == type.TypeDescriptorVirtualAddress
                && VirtualAddressToFileOffset(type.TypeDescriptorVirtualAddress) + 8 == type.RttiNameFileOffset
                && ReadAt(type.RttiNameFileOffset, name.Length).SequenceEqual(name));
        }
        check("Binary SynchronizationRequest57 requires20 bytes and compares four trailing MD5 DWORDs",
            Matches(0x124B46F, "83F914")
            && Matches(0x124B4AF, "C7459C01234567C745A089ABCDEFC745A4FEDCBA98C745A876543210")
            && Matches(0x124B4DE, "E8ED920300")
            && Matches(0x124B4EC, "E8AF930300")
            && Matches(0x124B503, "3B02")
            && Matches(0x124B511, "83EF04")
            && Matches(0x124B563, "E8580CFBFF"));
        check("Binary native MD5 padding starts80 then63 zero bytes",
            ReadAt(VirtualAddressToFileOffset(0x1C0E4A0), 64)
                .SequenceEqual(new byte[] { 0x80 }.Concat(new byte[63])));
        const uint sdbCatalogEntryCount = 0x23F;
        const uint sdbCatalogEntryArrayVirtualAddress = 0x1DD9EF0;
        int sdbCatalogHexPairs = 0;
        bool sdbCatalogExceptionsMatch = true;
        if (sdbCatalogExceptionsMatch)
        {
            long entryArrayOffset = VirtualAddressToFileOffset(sdbCatalogEntryArrayVirtualAddress);
            for (uint index = 0; index < sdbCatalogEntryCount; index++)
            {
                uint entryVirtualAddress = ReadUInt32(entryArrayOffset + index * 4);
                long entryOffset = VirtualAddressToFileOffset(entryVirtualAddress);
                uint field28VirtualAddress = ReadUInt32(entryOffset + 0x28);
                uint field2CRaw = ReadUInt32(entryOffset + 0x2C);
                if (index == 10)
                {
                    sdbCatalogExceptionsMatch &= ReadUInt32(entryOffset + 4) == 0x69CE585D
                        && ReadCStringAscii(field28VirtualAddress, 16) == "hands"
                        && ReadCStringAscii(field2CRaw, 16) == "backpack";
                }
                else if (index == 19)
                {
                    sdbCatalogExceptionsMatch &= ReadUInt32(entryOffset + 4) == 0xFDA606A8
                        && field28VirtualAddress == uint.MaxValue
                        && field2CRaw == uint.MaxValue;
                }
                else
                {
                    string text = ReadCStringAscii(field28VirtualAddress, 16);
                    uint parsed = 0;
                    bool parsedAsHex = text.Length == 10
                        && text.StartsWith("0x", StringComparison.Ordinal)
                        && uint.TryParse(text.AsSpan(2), System.Globalization.NumberStyles.HexNumber,
                            System.Globalization.CultureInfo.InvariantCulture, out parsed);
                    if (parsedAsHex && parsed == field2CRaw)
                        sdbCatalogHexPairs++;
                    else
                        sdbCatalogExceptionsMatch = false;
                }
            }
        }
        check("Binary compiled SDB catalog tail has573 hex-string/DWORD pairs and two distinct exceptions",
            sdbCatalogExceptionsMatch && sdbCatalogHexPairs == 573);
        check("Binary synchronization mode0 reads two DWORDs before requiring key1 and second DWORD8",
            Matches(0x124E6F5, "E82684FBFF")
            && Matches(0x124E705, "E81684FBFF")
            && Matches(0x1206B4C, "8B068B04075F890383410804")
            && Matches(0x124B806, "833801")
            && Matches(0x124B810, "6837CBFA0F")
            && Matches(0x124B827, "83780408")
            && Matches(0x124B832, "68C08FA90F")
            && Matches(0x124B9D2, "C6461C01"));
        check("Binary synchronization registered dispatch invokes callback slot0, not queue insertion",
            Matches(0x124C030, "3906750A5F32C0")
            && Matches(0x124C03E, "8B480CFF750C578B018B00FFD0"));
        check("Binary synchronization first DWORD seeds a624-word generator",
            Matches(0x127FFD6, "894108C7410401000000")
            && Matches(0x127FFEF, "69C26589076C")
            && Matches(0x127FFFE, "81790470020000"));
        check("Binary synchronization key hash uses native split-bucket comparison/fold",
            Matches(0x1248097, "3956187710D1EF83C9FF2BCF5F5E8D0411"));
        check("Binary native MT19937 vtable output entry and624-word regeneration gate",
            ReadUInt32(VirtualAddressToFileOffset(NativeMt19937.NativeVtableVirtualAddress))
                == NativeMt19937.NextUInt32MethodFileOffset + 0x400C00
            && Matches(0x127FEA3, "817E04700200007505E86F000000")
            && Matches(0x127FEB1, "8B46048B4C860840894604"));
        check("Binary native MT19937 output shifts and effective tempering masks match standard32-bit algorithm",
            Matches(0x127FEBE, "C1E80B33C88BC125AD583AFFC1E007")
            && Matches(0x127FED1, "258CDFFFFFC1E00F33C88BC1C1E81233C1")
            && ReadUInt32(VirtualAddressToFileOffset(0x1C7EBA4)) == 0
            && ReadUInt32(VirtualAddressToFileOffset(0x1C7EBA4) + 4) == 0x9908B0DF);
        check("Binary MT array initialization uses19650218, max624/keycount and standard two mixing multipliers",
            Matches(0x1280016, "68AAD62B01")
            && Matches(0x1280031, "3BDA0F42DA")
            && Matches(0x128004E, "69C90D661900")
            && Matches(0x128009B, "69C9658B585D")
            && Matches(0x12800C5, "C7460800000080"));
        check("Binary generic inclusive UInt32 range draws before unsigned bounds check then wrapping width/division",
            Matches(0x127FD84, "8B0156FF10")
            && Matches(0x127FD8F, "3BF1771C2BCE33D241F7F1")
            && Matches(0x127FD9E, "8D0416")
            && Matches(0x127FDCB, "C74424182119271C"));
        check("Binary byte-vector comparator takes minimum byte length, compares unsigned bytes and returns false without a length tie-break",
            Matches(NativeByteVectorHeap.LessMethodFileOffset,
                "558BEC8B11568B71048B4D082BF28B018B49042BC83BCE0F42F183EE04721290"
                + "8B0A3B08751083C20483C00483EE0473EF83FEFC743E8A0A3A08752783FEFD7433"
                + "8A4A013A4801751A83FEFE74268A4A023A4802750D83FEFF74198A4A033A48037411"
                + "1BC933C083C90185C95E0F98C05DC2040033C933C085C95E0F98C05DC20400"));
        check("Binary heap records use owning byte-buffer copy construction and self-assignment-aware assignment",
            Matches(0x3C60C0, "558BEC568B7508578BF9C70700000000C7470400000000C74708000000008B46042B0650")
            && Matches(0x3C7AF0, "558BEC83EC08538BD9568B7508895DF83BDE0F84C40000008B16"));
        check("Binary heap adjust compares right against left, chooses right on ties and copies the selected12-byte header's buffer",
            Matches(0x127FAAA, "E841CEFFFF84C074014E8D04768D0487508D045B8D0C87E82A8014FF"));
        check("Binary heap push compares parent against saved value, copies parents downward and respects the top boundary",
            Matches(0x127FBF9, "E8F2CCFFFF84C074268B4DFC8D045B578D0C81E8DF7E14FF")
            && Matches(0x127FC21, "D1FE395D087CC8"));
        check("Binary make/sort heap save buffer contents in temporaries and sort moves root to tail before reducing the heap by12 bytes",
            Matches(0x127F59B, "04768D3C8183EF0C8D4DE4574EE8136B14FF")
            && Matches(0x127F657, "8D73F403F78D4DE856E85B6A14FF538BCEC745FC00000000E87C8414FF")
            && Matches(0x127F67A, "83C7F4"));
        check("Binary byte-vector introsort checks count32 first, then positive ideal and halves plus quarters its budget",
            Matches(0x127E8C1, "83F8200F8EA5000000")
            && Matches(0x127E8D0, "85F60F8EB3000000")
            && Matches(0x127E8E6, "8BC683C408992BC28BC8D1F98BC1992BC2D1F803C8"));
        check("Binary byte-vector partition receives a hidden output pair and writes its two native pointer boundaries",
            Matches(0x127EB8B, "8BDA895DDC894DC88B7D08")
            && Matches(0x127F18C, "8B4DC88B45EC89018BC1897104"));
        check("Binary byte-vector introsort recurses on the smaller side and otherwise makes/sorts the heap",
            Matches(0x127E92B, "3BC68B7508567D0F8B55F88BCBE863FFFFFF8B5DFCEB0D8B4DFC8BD7E854FFFFFF8B7DF8")
            && Matches(0x127E9A4, "83F8017E1283EC088BD78BCBFF750CE8980B000083C40CFF750C8BD78BCBE8490C0000"));
        check("Binary byte-vector pivot samples use last-first in12-byte strides and select ninther only above40 last-index difference",
            Matches(0x127F3D0,
                "558BEC83EC0853568BD9B8ABAAAA2A8B4D088BF157FF750C8BFA895DF82BF3897DFCF7EE"
                + "D1FA8BC2C1E81F03C283F8287E6C"));
        check("Binary byte-vector insertion first-element branch compares saved bytes then copies the entire prefix backward",
            Matches(0x127F757, "E894D1FFFF84C0741A8BF73BDF741083EE0C568D4E0CE87E8314FF3BF375F0"));
        check("Binary byte-vector sort wrappers initialize ideal from element count rather than a logarithmic depth",
            Matches(0x127E4F0,
                "558BEC83EC088B550CB8ABAAAA2A8B4D082BD1F7EAC645FC00FF75FCD1FA8BC2C1E81F03C28B550C50")
            && Matches(0x127E6D0,
                "558BEC518B550CB8ABAAAA2A8B4D082BD1FF7510F7EAD1FA8BC2C1E81F03C28B550C50E8A8010000"));
        check("Binary adapter address fingerprint binds GetAdaptersInfo and case-insensitive name comparison through original PE IAT",
            ImportSlotNames(NativeAdapterAddressFingerprint.GetAdaptersInfoSlotVirtualAddress, "GetAdaptersInfo")
            && ImportSlotNames(0x1AC48A8, "_stricmp")
            && ImportSlotNames(0x1AC4028, "RegOpenKeyA")
            && ImportSlotNames(0x1AC4070, "RegQueryValueExA")
            && ImportSlotNames(0x1AC4064, "RegCloseKey"));
        check("Binary awesomium.dll is a static 86-entry import; WebCore init sub_CBCA50 calls WebConfig ctor and Initialize through IAT",
            ImportSlotNames(0x1AC4E00, "?ToBoolean@JSValue@Awesomium@@QBE_NXZ")
            && ImportSlotNames(0x1AC4F54, "?ToString@JSValue@Awesomium@@QBE?AVWebString@2@XZ")
            && ImportSlotNames(0x1AC4E8C, "??0WebConfig@Awesomium@@QAE@XZ")
            && ImportSlotNames(0x1AC4E94, "?Initialize@WebCore@Awesomium@@SAPAV12@ABUWebConfig@2@@Z")
            && Matches(0xCBCA50, "558BEC6AFF68C5FCA401")
            && Matches(0xCBCA96, "FF158C4EAC01")
            && Matches(0xCBCF0E, "FF15944EAC01")
            && Matches(0xCBCAD0, "68B060BC01") && Matches(0xCBCD3C, "686860BC01") && Matches(0xCBCDF1, "688460BC01"));
        check("Binary adapter registry root, Connection suffix, PnpInstanceID and PCI/USB/BTH literals decode exactly",
            DecodedAdapterLiteral(0x1C7E908, 79, 79, -1, 0xB1, NativeAdapterAddressFingerprint.RegistryRoot)
            && DecodedAdapterLiteral(0x1C7E668, 11, 83, 1, 0xAD, NativeAdapterAddressFingerprint.ConnectionSuffix)
            && DecodedAdapterLiteral(0x1C7E970, 13, 61, 1, 0x3D, NativeAdapterAddressFingerprint.PnpValueName)
            && DecodedAdapterLiteral(0x1C7E47C, 3, 97, -1, 0x61, "pci")
            && DecodedAdapterLiteral(0x1C7E6E0, 3, 36, -1, 0x24, "usb")
            && DecodedAdapterLiteral(0x1C7E560, 3, 102, -1, 0x66, "bth"));
        check("Binary adapter registry filter allows missing keys but rejects bad size/type/data queries",
            Matches(0x127CECD, "B301E926010000")
            && Matches(0x127CF14, "FFD685C00F85AE00000083BD6CFFFFFF010F85A1000000")
            && Matches(0x127CF98, "85F6740432DBEB0A8D4DD8E878FBFFFF8AD8")
            && Matches(0x127CFCC, "FFB574FFFFFFFF156440AC0132DB"));
        check("Binary adapter fingerprint requires initial error111, sorts captured buffers and feeds exact bytes to MD5 before writing16-byte output",
            Matches(0x127D10D, "FFD683F86F7407")
            && Matches(0x127D24C, "E84F160000")
            && Matches(0x127D290, "8B46048D4D948B162BC250E83075000083C60C")
            && Matches(0x127D2B0, "E8EB750000")
            && Matches(0x127D2BB, "F30F6F4584C6857FFFFFFF01F30F7F00"));
        check("Binary adapter type6/71 and unsigned address length>=6 precede registry filtering and address copy",
            Matches(0x127D171, "8B86A001000083F806740983F8470F858700000083BE90010000060F827A0000008BCEE837FBFFFF84C0746F"));
        check("Binary mode-zero initializes both digest slots to zero and conditionally copies separate helper results",
            Matches(0x124B8B0, "0F57C0")
            && Matches(0x124B8BB, "F30F7F442438")
            && Matches(0x124B8C9, "F30F7F442448")
            && Matches(0x124B921, "E80A0E030084C0740CF30F6F442414F30F7F442438")
            && Matches(0x124B93A, "E88117030084C0740CF30F6F442414F30F7F442448"));
        check("Binary Windows-drive fingerprint imports directory/file/device APIs and uses the raw UTF16 device prefix",
            ImportSlotNames(NativeWindowsDriveSerialFingerprint.WindowsDirectorySlotVirtualAddress, "GetWindowsDirectoryW")
            && ImportSlotNames(NativeWindowsDriveSerialFingerprint.CreateFileSlotVirtualAddress, "CreateFileW")
            && ImportSlotNames(NativeWindowsDriveSerialFingerprint.DeviceIoControlSlotVirtualAddress, "DeviceIoControl")
            && ImportSlotNames(NativeWindowsDriveSerialFingerprint.CloseHandleSlotVirtualAddress, "CloseHandle")
            && ReadAt(VirtualAddressToFileOffset(0x1C7EB8C), 10)
                .SequenceEqual(Encoding.Unicode.GetBytes(NativeWindowsDriveSerialFingerprint.DevicePathPrefix + "\0")));
        check("Binary Windows-drive directory query uses260 UTF16 units, checks only zero and truncates at code unit2",
            Matches(0x127C75F, "68040100008D85E8FDFFFF50FF15C441AC0185C00F8483000000")
            && Matches(0x127C79D, "668985ECFDFFFF"));
        check("Binary Windows-drive file open uses read access, share3, open3, zero optional parameters and only FFFFFFFF invalid handle",
            Matches(0x127C5C8, "6A006A006A036A006A03680000008050FF154843AC018BF883FFFF0F94C3"));
        check("Binary Windows-drive storage query zeroes its12-byte input and requires successful header ioctl with exactly8 returned",
            Matches(0x127C616, "0F57C050660FD645E48945EC8945E48945E8")
            && Matches(0x127C638, "6800142D0057FFD385C07465837DC808755F"));
        check("Binary Windows-drive second storage ioctl requires exact size then reads offset24 without a zero-offset special case",
            Matches(0x127C66A, "566A0C506800142D0057FFD385C074288B45C43B45C87520")
            && Matches(0x127C682, "8B46188B4DBC03C650E8805F07FF"));
        check("Binary Windows-drive successful serial path uses existing MD5 update/finalize and copies sixteen output bytes",
            Matches(0x127C869, "E8627F000083C4048D954CFDFFFF8D8D5CFDFFFFE81E80000083BDE4FDFFFF10F30F6F854CFDFFFFF30F7F06"));
        check("Binary Windows version code imports GetVersionExW, requests284 bytes and packs major/minor/service-pack by shifting and OR",
            ImportSlotNames(NativeWindowsVersionCode.GetVersionExSlotVirtualAddress, "GetVersionExW")
            && Matches(0x128010A, "C785E0FEFFFF1C010000508AD9FF15E842AC0185C0")
            && Matches(0x1280138, "0FB74DF48B85E4FEFFFFC1E0080B85E8FEFFFFC1E0080BC1")
            && Matches(0x1280277, "B9000206003BC85B1BC083E00D")
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12800F0, 0x1CA)))
                == "C7A5AB06AEEFA60BFC2402E4F03719ECEE8DCED0DF19D59491AB161AA4A26837");
        check("Binary mode-zero descriptor starts operation2, serializes native fields, compresses before appending MD5",
            Matches(0x124E788, "C745EC02000000E88CDCFAFF")
            && Matches(0x124E7A5, "8B7DE48BCFE861A1F9FF84C0742B8D77048BCEE853D4FFFF")
            && Matches(0x124FFE9, "FF75088BF98B0B8D77045657FF710883C104E8408617FF8BD68BCBE8D70400008D57148BCBE8CD040000"));
        check("Binary descriptor opaque metadata begins24 and40, each followed by ushort-length narrow bytes",
            Matches(0x1250013, "FF75088B0B83C7248D77045657FF710883C104E8158617FF8BD68BCBE8BCBEFBFF")
            && Matches(0x1250034, "FF75088B0B8D471C8D70045650FF710883C104E8F48517FF8BD68BCBE89BBEFBFF")
            && Matches(0x120BF0B, "817E10FFFF000077700FB74610"));
        check("Binary descriptor compression uses zlib1.2.8, window15/memory8/default strategy and appends original DWORD length",
            ReadAt(VirtualAddressToFileOffset(0x1C7DF28), 6).SequenceEqual("1.2.8\0"u8.ToArray())
            && Matches(0x12563F0, "6828DFC7016A006A086A0F6A08E84EA7040083C418")
            && Matches(0x11E8953, "C745E814DFC70150C745ECFFFFFFFFE809DA0600")
            && Matches(0x11E89D8, "FF75F08D45F48BCE508D45F050FF7308E853FC1DFF"));
        check("Binary inflater allocates final DWORD length, prefills FF and requires stream end plus no unused input",
            Matches(0x120C2CF, "FF741FFCE8880B0000")
            && Matches(0x120C2ED, "5068FF00000051E801163900")
            && Matches(0x1256B8C, "83FF017507837C2414007464"));
        check("Binary metadata tail uses dots marker, locale towlower and captured temporary/shell-folder provider APIs",
            ReadAt(VirtualAddressToFileOffset(NativeMetadataTextTail.ShorteningLiteralVirtualAddress), 8)
                .SequenceEqual(Encoding.Unicode.GetBytes("...\0"))
            && ImportSlotNames(0x1AC4ABC, "towlower")
            && ImportSlotNames(0x1AC41F8, "GetTempPathW")
            && ImportSlotNames(0x1AC4B20, "SHGetFolderPathW"));
        check("Binary metadata shortening and strict UTF16 range encoder retain complete native function fingerprints",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1283720, 0x113)))
                == "95363B1F69E9A7C901FAF04D0EB2455CD5CB33FE54C069290923C1A7058FAD48"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1258C70, 0x110)))
                == "5ED68BB88E015CDD44B68A24B6734902AFAFE2DBD8AEDC069F0ADC580ED2AD57"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1258E90, 0xF0)))
                == "24A46815C2BD6AE09AD4BD29FDF5C4974FE4567C71AA9E054043D5B9C8E4988F");
        byte[] folderOrder = new byte[NativeFolderPrefixAlias.FolderOrder.Length * 4];
        for (int i = 0; i < NativeFolderPrefixAlias.FolderOrder.Length; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(folderOrder.AsSpan(i * 4), NativeFolderPrefixAlias.FolderOrder[i]);
        check("Binary folder substitution order and decimal-token delimiters match saved static data",
            ReadAt(VirtualAddressToFileOffset(NativeFolderPrefixAlias.FolderOrderVirtualAddress), folderOrder.Length)
                .SequenceEqual(folderOrder)
            && ReadAt(VirtualAddressToFileOffset(0x1C7EA1C), 4).SequenceEqual(Encoding.Unicode.GetBytes("<\0"))
            && ReadAt(VirtualAddressToFileOffset(0x1C7EA18), 4).SequenceEqual(Encoding.Unicode.GetBytes(">\0"))
            && ImportSlotNames(0x1AC4AC8, "towupper"));
        check("Binary folder prefix static initializer stores exactly four UTF16 units of extended-path prefix",
            ReadAt(VirtualAddressToFileOffset(NativeFolderPrefixAlias.ExtendedPrefixLiteralVirtualAddress), 10)
                .SequenceEqual(Encoding.Unicode.GetBytes(NativeFolderPrefixAlias.ExtendedPrefix + "\0"))
            && Matches(0x2E33D0, "6A046848EAC701B9A8EFDF01E85FD89D00"));
        check("Binary folder alias control flow, one-separator removal and captured locale-prefix comparison preserve complete functions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x126DAC0, 0x2C2)))
                == "A3551192E02FCD9BC9579E8545AE834A0B43CCF5D787C7D3D0279A62B1B617CA"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x126D750, 0x7A)))
                == "1CEBFD42B803446568027FE56B14D67E4BFA9509F1D378ECC2E52439663D339E"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x126D7D0, 0x65)))
                == "1E5F795E7A5BE2A9409159E2604FFE77A534F12600E3ACFEE8BE914CC0201DF7"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1270D40, 0x5D)))
                == "8DEEDC7F08E8FB5E1D67B791ADA5D9063FC3FFDE7F332ED67099ED837DB85D84");
        check("Binary module path provider binds filename/last-error/secure splitter and its four selector targets map through the PE text section",
            ImportSlotNames(NativeModulePathProvider.GetModuleFilenameSlotVirtualAddress, "GetModuleFileNameW")
            && ImportSlotNames(NativeModulePathProvider.GetLastErrorSlotVirtualAddress, "GetLastError")
            && ImportSlotNames(NativeModulePathProvider.SplitPathSlotVirtualAddress, "_wsplitpath_s")
            && ReadUInt32(VirtualAddressToFileOffset(0x166DAE0)) == 0x166DA53
            && ReadUInt32(VirtualAddressToFileOffset(0x166DAE0) + 4) == 0x166D9EF
            && ReadUInt32(VirtualAddressToFileOffset(0x166DAE0) + 8) == 0x166DA01
            && ReadUInt32(VirtualAddressToFileOffset(0x166DAE0) + 12) == 0x166DA27);
        check("Binary module path retry/error gates, selector enum and secure split component copies preserve complete functions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x126CBD0, 0x30A)))
                == "D9E1617308ED760CD21D4236D52D0D31CA8AACB81A9E43F96CD260AED4A440B0"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x126D9E0, 0xDC)))
                == "954055CAC3A39B42E2091B9CD5D8249470F936FDDC5F6F3969542BB5A2F0F25F");
        check("Binary generated provider constructor, generator and descriptor helpers match bounded original-file fingerprints",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1285060, 0x78)))
                == "E65D5B518CD1EAC806799D6E89A7E1FE0613BE9A51976289057D6DF121566A9E"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1286F00, 0x3BA)))
                == "B6B31A2A71DAC8E33318A6E387AC821D4799236B525FE9DFE7E8138C6E520006"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12867D0, 0xA3)))
                == "25527D6741BC391C758A7C3DDC376FE53912861DAC442E23D7D378B58D1519B4"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12866C0, 0xBF)))
                == "CC5D5E67B4BCE8DBD55078686E5CFC05F68908C2D296E455EA33A79F426CED1A"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12868D0, 0x623)))
                == "791A1C28CBB53EDAA18E4235AA01F9757E507845A1C87C987583AF33D5EF32F2");
        check("Binary recovered generated boundary selectors preserve original bytes through explicit throw-tail bounds",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1285290, 0x6DC)))
                == "331513F1FA06248D730333703A9709064012A6421C1DB11F9FE74ADD0A988AE7"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1285970, 0x4B7)))
                == "C3965E02B32184C12381F50978429FCF08DF20A554044E98DF187A6A6A931D5D");
        check("Binary generated operand classification and sparse operation factories match recovered bounded function regions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x128A840, 0x1E2)))
                == "8DC15CA83CEE60705FFD0C3AE959E5C7E43A30EBCC5B5FD084FFFD15DAA43B0C"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12506F0, 0x3C)))
                == "9EBBA2233386F23DF8B61E277614AEA2041B6ED3F1FA5188B0DD4A5C0CC04E62"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1284F90, 0xBC)))
                == "35F882BEB55CB38ECE84954FF182109D4F74B20A0EB0948BF95508A29E777120"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1250850, 0xB0)))
                == "D424706CFCE7E9E8FC01887A3C229A41D334A0719C6C5FF9EBF2B94179A6C650"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1250780, 0xC8)))
                == "D210DAF3ABBE192A96008030C434F45CB232F795A3EA96361E0FDFBE1F4D8A7C");
        check("Binary generated boundary predicates, emitters, ModRM helpers and static template routing match original regions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12997C0, 0x1420)))
                == "DC57FA6AA83C53B21991A0E3D5C9166E2CD00316AF19CD4885E2DC425F13CDA3"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1290C40, 0x8C0)))
                == "9AED146B5932EDEC397B859D1CF80E34D65FE2806BA145161CE0FCCD0192637D"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1291E30, 0x400)))
                == "DBF610A2D51B81AD93C983853EE7278F663B383756232C829CE0B45E4B4CC550"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1292CE0, 0x190)))
                == "0F46E9EB5F51BC47FA033EE58637BBF70F2BFC1D7A191584394387BBF7026882"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x180DF58, 0x210)))
                == "B1E41769C1C94DB83754CBA03A1A44137BAFB8B5A2C84F212AB554DD9A84FE33"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x19FD400, 0x200)))
                == "2620214C41C932819598ECD6D9859D8B7299CE58B354DE12A6B1B7409ED6DAE5"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x180D000, 0xF58)))
                == "E1F0D53C1D8B7B154E1DC061C412C741BA192A1C54C75EFB58B28573FAF74841"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x187ED00, 0x700)))
                == "326EA6BD1A8318F4D4DDB6AC3AA8E30E9446D7CF0FFEEB9FD0FCE611396ED54C");
        check("Binary immediate operand constructor and conditional RNG-discard helper preserve complete original functions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1284EF0, 0x50)))
                == "8BBFA988168385F56B267F176DA19F16E867E1CB0D0F3A48B1168AEBE05D0061"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1286780, 0x50)))
                == "B1B2AE140E9C9318A29FC99C7300A22E78A99A2B98D6E958B70D6E1F2D897225");
        uint[] generatedPairs = [1, 41, 41, 1, 6, 5, 5, 6, 34, 35, 35, 34, 43, 43];
        bool generatedPairTableMatches = true;
        long generatedPairOffset = VirtualAddressToFileOffset(NativeGeneratedBodyDecisionRules.PairTableVirtualAddress);
        for (int i = 0; i < generatedPairs.Length; i++)
            generatedPairTableMatches &= ReadUInt32(generatedPairOffset + i * 4) == generatedPairs[i];
        check("Binary generated seven forward/reverse operation pairs agree with PE-mapped static DWORD table",
            generatedPairTableMatches);
        check("Binary generated unary, rotation and arithmetic body decision helpers preserve original functions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1285E30, 0x88)))
                == "86A10CA81209FA176C54023893E31B12E15D285A1A13C6E717A91477DCE3F5E8"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1285EC0, 0x23F)))
                == "6FA21943451B70BE5215D0434955C20271A6E55C5DB83F4CB47F8AA9D61FF3BD"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1286100, 0x5B0)))
                == "69E1675EE12D032EBDF01F57E0C342D2364C828461DC2C590D8B458F84FF7F2F");
        check("Binary conditional branch gates/emission and wrapper OR/INC/DEC templates preserve original regions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12900D0, 0x220)))
                == "39E2B4F0034C678055920CA873DBD12B60304BC893496840176273BA337932F1"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1290850, 0x230)))
                == "FDC0B0ACA41B5C3AAB0CF4A648D95DD78F79B3E7BD56FC2CB1587227495FA6ED"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1291550, 0x8E0)))
                == "7E0908009939E1379F82BE85E7C2DF85FAFA156FBF381EEB6A2E6943215230CE"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x128E750, 0x4E0)))
                == "B2A4E60AE4B747F2BDB4239E889BAE70777F7F0498B95CE291D75E5B62D166B2"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1296880, 0x1F0)))
                == "565B2228DDE1B54B80BBD57AF7B6B5FAD11444CA844C701BF06E7D62439D45FD");
        check("Binary byte rotation candidate predicates, emitted groups and implicit-one/CL variants preserve original regions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1292E70, 0x930)))
                == "F34608F374AB273D661A1E569BA4E9A1BFD44A15C74CC59A00434F6E6A17A5FB"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1298410, 0x4D0)))
                == "75A010D81B7BEC8CF127438B2C396E6C91AC952EF221C9E3C5E027EE812A2F35");
        check("Binary immediate MOV index/width and C6/C7 group constructors match original functions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1299680, 0x63)))
                == "1A3BD481B8B0567C0D4BD5E6CE11F7159F546D80B303A0E521F00151F54AA2E6"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1299890, 0x60)))
                == "263609EF9A1CFE663567ABF2B12A678216337E6485775026514F06544030D258"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1297750, 0x50)))
                == "E1FCEE0D825DB231D0B74EE02CDF451CA165F5117165A3EAF12CAC9BC9B894A7"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12977A0, 0x50)))
                == "F3077B0953935CED5CCA8ABE8A638853B427C3DE9B6FC9BDA1507F37B7D7F784");
        check("Binary ADD/SUB/XOR full candidate predicates and emitters match original regions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x128DA90, 0x812)))
                == "ED96ED04390BA874FF2CB6481F0D8CDD999372F0FDE94B34E15ECDB6565EE9EF"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1294BB0, 0x812)))
                == "BCB36784A96BAF64E8722FCD651CAB2624AFEF3FAF00675FE32F49D362A41A14"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1295490, 0x812)))
                == "C948A2A5E5113C1BA9D68D9C0015ED606425AB7D79ED1F25CF5FAD866E69B689"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1295DC0, 0x840)))
                == "D4B5F949C657B5D372323D9D37B97A8E8D7D2A56CD70A7677638A751A12FFA62"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1299310, 0xB7)))
                == "328C1B484487153FA6F11F51DB4CBE567BAED4BB34661E43243227533A15876C"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12995A0, 0xB7)))
                == "103F2F242EAA6014C1ED4F0D14BC7AE70FE41387C403943A03646A8504A7800F");
        check("Binary complete generated output pair and selected-descriptor RNG dispatch preserve original functions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12866C0, 0xBF)))
                == "CC5D5E67B4BCE8DBD55078686E5CFC05F68908C2D296E455EA33A79F426CED1A"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1286F00, 0x3BA)))
                == "B6B31A2A71DAC8E33318A6E387AC821D4799236B525FE9DFE7E8138C6E520006");
        check("Binary generated arena allocation, best-fit reuse, region growth and block return preserve original functions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1250F90, 0x5C)))
                == "6B6ED864D17E67676A0AD6F210E669ED4B6405A68F9096EAF3AD3B31B680B958"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1251050, 0x9A)))
                == "F6624940EB51DAE6262E6528231D1107FE76EA9B5E9E8AC9483A1DC589382F95"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12510F0, 0x61)))
                == "50551CC1B132C78B76F087E1C4538C013F26D60F26AD6A4CE83544654E3E20EB"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1251160, 0xBE)))
                == "E2A11A31C38BFC1EF3D093819A138FED81561B9BFC7459D0FAC18265E8645CBE"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1251350, 0x130)))
                == "7A953CCD5ECC8A51B85F6F80B9943BC34F0121ECAAFC0888C344D2494319C591"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1257690, 0x82)))
                == "23106B19B3AF7EFAE7F0FF141AF284669EC540764709DD81277FFB3D8314BA2F");
        check("Binary generated arena import slots resolve VirtualAlloc, VirtualFree and critical-section functions from original PE data",
            ImportSlotNames(0x1AC42B8, "VirtualAlloc")
            && ImportSlotNames(NativeGeneratedArenaPolicy.VirtualFreeImportSlotVirtualAddress, "VirtualFree")
            && ImportSlotNames(0x1AC4278, "EnterCriticalSection")
            && ImportSlotNames(0x1AC427C, "LeaveCriticalSection")
            && ImportSlotNames(0x1AC4280, "InitializeCriticalSection"));
        check("Binary provider release order, two output tail-jumps and replacement cleanup preserve original functions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12850F0, 0x2E)))
                == "F2AC4AE86DF565ACA5C5E3031FCA097896BC7D1194F71803D67A29E2DC6CE8A9"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1285120, 9)))
                == "BAD635AA6810029341F072BC3B18D570175CE29AFE36CBBD311DB1DA6C4B45CD"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1285130, 9)))
                == "7F4FD4B745740F5778D93D3ABEC3A2387A5DE72B464ED437CA154E1E42D01386"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1285140, 0x2E)))
                == "03D69D411AF55187EA3F00FA74C4C15D465A5059726C3973AE8327A33634B471"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x124B1A0, 0xA2)))
                == "0DFC87D55B60364DCA01F5817369174144191B7B1975A29354E926D62434751C");
        check("Binary direct provider callback calls and following reload/queue-lock preparation preserve original bytes",
            Matches(NativeGeneratedProviderLifetimePlan.IncomingCallbackCallFileOffset, "FFD08B4D0C")
            && Matches(NativeGeneratedProviderLifetimePlan.OutgoingCallbackCallFileOffset, "FFD08D73045689750C"));
        check("Binary tagged reference release routes shared-text nodes back through the allocator free stack",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x424790, 0x70)))
                == "3093A4B83C89492FD5BDCFF60B76545C46E557E345CA85E35F123E731D319341"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12B9F70, 0x40)))
                == "9F3358793B48E00216CE459ED698CB4DC5B3A4CDD89D9EFBC47F4C7A845AFCBC"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12CDEC0, 0x3F)))
                == "6ABFCE0731ADC2584EB89DF9F08D1D68D683F9F3AFD02E1B7627956EC605DF7E"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x12CDA60, 0x76)))
                == "C83EC17B90BB2FA55931A1EBED457B34A2E5780F1C473C356E1623F4F577D89B");
        check("Binary tfWorld nested value cleanup and tree-like node removal helpers match bounded original regions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x975020, 0x1A2)))
                == "6E8A75F6021DCA0B3CB1885E22A7B05C4BA722A0406EFE889F91D8476ADA56F5"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x9751F0, 0xE0)))
                == "812F8349E49CC6827FE4147BFB4FEB90DEF84B8758AA9F7CC799C31097ADC00B"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x88AD60, 0xD4)))
                == "17B206C911AE9E0BFB14382797912173C7E26934585FB046578EA41C7B1ED5D6"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x97AE50, 0x9E)))
                == "4A663C923CE53A8403E231B78C27314E47CF6307320477F85757C1D7A508762F"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x97AA70, 0x9E)))
                == "B9B051B9EFDA64096D84B820CE5B9E41B6089421F3CC9835C551D15CDEBDF3FB"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x97BCC0, 0x244)))
                == "30D11055E73D913F94C64B4643F05E1E47AE3E62D46C4A54B32195BDED8FD046"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x978440, 0x244)))
                == "9DECBA672F9DD47999DB2BF80EC5C529511B24CDA9BFE6F33C6CC918D439C28E"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x979A20, 0x6B)))
                == "5A9B857CDFB3368824B4B826DF005D2DE7DEFB91591FCCF9D63EDCE08D891F9F");
        check("Binary tfWorld nested range clear, recursive node release and partial erase paths match bounded original regions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x3F72C0, 0x35)))
                == "2ADD837001B16E0E30F0779D61FB1B7BAA1E5D36A8F5A851CF25CC0DC794B2C7"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x3F7300, 0x46)))
                == "39F204C0CD7BFDD2CEEF661252A4EAF5014B0AD2803C998E3BE8AE304F3CECA0"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x3F7430, 0x244)))
                == "08C2BB5951A49BEA1637A5E7EF5731BB1454FB2C3A32068F644617D80471F361"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x3F7680, 0x9E)))
                == "6389E2C2AB9B68C09A7A66781F0D886C8AAAA2EE64DE39B5D290A6234CC15AFA"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x9795B0, 0x59)))
                == "540EB0C3C7269115BA5035092FF7384236DA5F2E56A93C134BA8A7D1398D30A4");
        check("Binary alternate nested cleanup callers pass the same value slot with distinct null/pending guards",
            Matches(0x976248, "8B46108945F085C0740F")
            && Matches(TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerOneCallFileOffset, "E8C7EDFFFF")
            && Matches(0x977F3F, "8B7E1057E8D8FEFFFF")
            && Matches(0x977F55, "837F0400741B")
            && Matches(TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoCallFileOffset, "E8A3D0FFFF")
            && Matches(TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoWaitCallFileOffset, "E8C0F6FFFF")
            && Matches(TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoWaitRemovalCallFileOffset, "E8150D0000")
            && Matches(0x977F70, "837F040075EA")
            && Matches(0x978672, "48894304")
            && Matches(TfWorldZoneCleanupNativeLayout.CleanupAdditionalValuePreCleanEventPushFileOffset,
                "68690100008B01FF90AC000000"));
        check("Binary tfWorld cleanup caller and guarded auxiliary dispatch match bounded original regions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x64E210, 0x1AC)))
                == "61E33C4847FE9313EA4C44AAC375DAC1523F5D3E4B73C20C903BF61ADD91101A"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x494B00, 0xE6)))
                == "CDA2F6536AA971DD4CA77E86D123B0C654F7798251D80C0C918DBCDBF86213AF"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x369A30, 0x99)))
                == "4D91FFD1C67674BD5873134DDE1D90E67EB3BAF23629A0BBF772A27863DE56D5"
            && Matches(TfWorldZoneCleanupNativeLayout.EnteredZoneCleanupCallFileOffset, "E82E961B00")
            && Matches(0x64E275, "E8B6B7D1FF")
            && Matches(0x64E3B3, "C683F102000000"));
        check("Binary released-object factory/getter calls and +0x32C slot-zero invocation match original bytes",
            Matches(TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerInitializeFileOffset,
                "C7832C03000000000000")
            && Matches(TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupResourceGetterSetupFileOffset,
                "8B8E980B00008B01FF5024")
            && Matches(TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupResourceGetterCallFileOffset,
                "FF5024")
            && Matches(TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupFactorySetupFileOffset,
                "8B4E108D9510FFFFFF528B01FF5040")
            && Matches(TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerStoreFileOffset,
                "89872C030000")
            && Matches(TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerFactoryCallFileOffset,
                "FF5040F6450C0289872C030000")
            && Matches(0x8610C4, "8B016A01FF10"));
        check("Binary released-object manager cleanup and nested resource release match original functions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x861020, 0xCA)))
                == "A6CB493C6F27860DE8EFE93DD75E92BAC0D2EA647B424A3D5FB8B37C5F1B67C9"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0xD74A20, 0x1B3)))
                == "913650BB63D692EB3218F46F5ACAFA458F064A1E770A5313A4C64B1A26D0524C");
        check("Binary mode1 direct/compressed scale tables preserve original PE targets and byte-index mapping",
            Matches(0x129A4C1, "FF248584B16901")
            && Matches(0x129A584, "C8B06901CDB06901E4B06901D7B06901E4B06901E4B06901E4B06901E1B06901")
            && Matches(0x129A734, "FF2485D8B36901")
            && Matches(0x129A7D8, "5AB369013BB369016EB36901")
            && Matches(0x129A7E4, "0001020102020201")
            && Matches(0x129A73B, "837B18007519")
            && Matches(0x129A75A, "8B4DFC8A45AB33CD5F5E5BE82C050D008BE55DC3")
            && Matches(0x129A7F0, "80792001751083791800750A33C0807911040F95C0C332C0C3"));
        check("Binary mode1 stack guards, no-operand RET and unusual no-index SIB preserve original instruction facts",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x128AF40, 0x5B)))
                == "399CDCC7863DA48AC30E278C903E44FA08B7C179CFBE47B184D2C374E80FDF08"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1292DC0, 0x33)))
                == "6AEA8BCE5932B960D393D36821DB5BFC7300220F9A215CC4A1548CA1377F752C"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1292E00, 0x6D)))
                == "416A720B95B98C21CAA953451712BCB61CEAC82261B91947C3379ED4E978F211"
            && Matches(0x129A4AE, "8A0324070CE08803"));
        check("Binary mode1 ModRM base-bit writer, REX slot and prefix insertion preserve complete original functions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x129ADA0, 0x31)))
                == "B62A82B22AA833731D01D20F12CA1BF312ADAAFAF256B0ECF8B91AD9E2F03DEB"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1299F10, 0x58)))
                == "7626677F06D453680E0B08F1128AC7AD78EA0A800994A3283A470418FC6BE143"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x129ABE0, 0xBB)))
                == "71358D278970F612536C55118C28EFFBDB1D57CA166FC89A015A8F7ACAC1CF31");
        check("Binary mode1 immediate-MOV gates/emitters and signed32-fit predicate preserve original regions",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x1291150, 0x400)))
                == "0A798DC81D34DB98A8C91F8C512727463BBC62C4629235A35164BB49B2155791"
            && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(ReadAt(0x128AD30, 0x50)))
                == "8E75F2624CC38B61B3C774431381CAABDAFAC91741F61BE83AB88BA2545A4016");
        check("Binary profiler names60 submits both lists then stores wrapping tick minus5000",
            Matches(0x4967CF, "E82CE29C00")
            && Matches(0x4967DA, "2D8813000089868C010000"));
        check("Binary profiler names60 hashes both key bytes and compares/stores complete WORD keys",
            Matches(0xE671CB, "0FB607")
            && Matches(0xE671D9, "0FB64701")
            && Matches(0xE67221, "668B4808663B0F")
            && Matches(0xE6AF9A, "668B0966890AC7420400000000"));
        check("Binary profiler frame61 missing-name throttle is unsigned elapsed>5000 and updates marker before endpoint lookup",
            Matches(0xE6531C, "2B8F8C01000081F988130000762B89878C010000")
            && Matches(0xE65339, "FF506085C07415")
            && Matches(0xE6534A, "FFB7D00100008BC8FF5208"));
        check("Binary profiler frame61 refresh suppression is independent and recursive collector preserves owner",
            Matches(0xE65355, "803D5A9AE40100751E")
            && Matches(0xE6536A, "E8D1800200")
            && Matches(0xE65377, "E884870200")
            && Matches(0xE66CD5, "8B4DE08D45C45750E8AEFEFFFF"));
        var halfTable = ReadAt(VirtualAddressToFileOffset(NativeProfilerSamples.HalfLookupVirtualAddress), 65536 * 4);
        bool halfTableMatches = true;
        bool halfExpansionMatches = true;
        for (int word = 0; word < 65536; word++)
        {
            float decoded = (float)BitConverter.UInt16BitsToHalf((ushort)word);
            uint bits = BinaryPrimitives.ReadUInt32LittleEndian(halfTable.AsSpan(word * 4));
            halfTableMatches &= float.IsFinite(decoded)
                ? bits == unchecked((uint)BitConverter.SingleToInt32Bits(decoded))
                : !float.IsFinite(BitConverter.Int32BitsToSingle(unchecked((int)bits)));
            float nativeValue = BitConverter.Int32BitsToSingle(unchecked((int)bits));
            bool supported = NativeProfilerSamples.TryExpandFiniteHalf((ushort)word, out uint expanded);
            halfExpansionMatches &= float.IsFinite(nativeValue)
                ? supported && expanded == unchecked((uint)(long)(nativeValue * 65536.0f))
                : !supported;
        }
        check("Binary profiler65536-entry float table exactly expands every finite binary16 value and classifies all nonfinite values",
            halfTableMatches
            && ReadUInt32(VirtualAddressToFileOffset(NativeProfilerSamples.ScaleLiteralVirtualAddress)) == 0x47800000);
        check("Binary profiler finite metric projection matches scaled native table for every WORD and declines all nonfinite entries",
            halfExpansionMatches);
        check("Binary profiler node decoder copies three raw WORD fields and scales five lookup floats by65536",
            Matches(0x130302A, "0FB746106689070FB74618668947100FB7461A66894712")
            && Matches(0x1303041, "0FB74612F30F100485B865C101F30F5905D421C801")
            && Matches(0x13030B5, "0FB7461EF30F100485B865C101F30F5905D421C801"));
        check("Binary profiler exclusive sample saturates child subtraction and uses unsigned divisor",
            Matches(0xE8D87C, "3BCF760433FFEB022BF9")
            && Matches(0xE8D8F9, "33D28BC7F7F18BF8"));
        check("Binary profiler history resets next-slot and fills eighty28-byte samples",
            Matches(0xE8E0E5, "89B9D4080000BF50000000")
            && Matches(0xE8D939, "8B83D408000040F7F68993D4080000"));
        check("Binary profiler root/node merges search only forward through next-sibling links",
            Matches(0xE8D500, "663B86DC08000074078B760885F675F0")
            && Matches(0xE8D190, "663B86DC08000074078B760885F675F0"));
        check("Binary profiler kinds root1/node2/exclusive3 retain full WORD identity",
            Matches(0xE8D5A9, "C786D8080000010000006689BEDC080000")
            && Matches(0xE8D239, "C786D8080000020000006689BEDC080000")
            && Matches(0xE8D074, "83BFD808000002")
            && Matches(0xE8D0F0, "83BED8080000037510668B86DC080000663B87DC080000"));
        check("Binary profiler selection searches kind/full WORD ID with child-first recursive preorder",
            Matches(0xE8D3FA, "3999D808000075110FB781DC0800003BC77506")
            && Matches(0xE8D420, "578BD38BCEE8C6FFFFFF")
            && Matches(0xE8D431, "8B760885F675E8"));
        check("Binary visible-row lookup gates expansion, compares/increments row cursor and descends before next sibling",
            Matches(0xE72483, "807A1000")
            && Matches(0xE724A0, "8B073BC37422408BD6578BCB8907E8CDFFFFFF")
            && Matches(0xE724BA, "8B760885F675DF"));
        check("Binary profiler new nodes begin collapsed, exclusive visitation retains selection and recovery expands ancestors only",
            Matches(0xE8E0B4, "66C741100001")
            && Matches(0xE8D128, "3970080F94C00801")
            && Matches(0xE8D6F0, "8BC1C64010018B480485C975F3"));
        check("Binary profiler frame fills null selected-row pointer by visible-row zero lookup without allocation",
            Matches(0xE6537C, "837F0C00751B")
            && Matches(0xE6538B, "C7450800000000E8E9D00000")
            && Matches(0xE6539A, "89470C"));
        check("Binary profiler root refresh loads first stored child and advances positionally without child-ID comparison",
            Matches(0xE8DB77, "8B5E0C")
            && Matches(0xE8DBF0, "8B570403D18BCBE864FBFFFF")
            && Matches(0xE8DC57, "8B5B08"));
        check("Binary profiler nested refresh searches forward by full WORD ID and skips missing children without recursive update",
            Matches(0xE8D837, "85F60F8481000000")
            && Matches(0xE8D842, "663986DC08000074098B760885F675F0EB6C")
            && Matches(0xE8D854, "8BD38BCEE803FFFFFF"));
        check("Binary profiler name getter virtual slot and cached nonempty-text precedence are explicit",
            ReadUInt32(VirtualAddressToFileOffset(NativeProfilerDisplayNames.NativeVtableVirtualAddress) + 0x10)
                == NativeProfilerDisplayNames.GetterFileOffset + 0x400C00
            && Matches(0xE8CD70, "8B81E408000085C07425")
            && Matches(0xE8CD8A, "85C07411"));
        check("Binary profiler display-name branches select node/root lookups and exact fallback labels",
            Matches(0xE8CDAA, "B87086C701C3")
            && Matches(0xE8CDC3, "E8587BFDFF")
            && Matches(0xE8CDE8, "E8F37AFDFF")
            && ReadAt(VirtualAddressToFileOffset(0x1C78650), 8).SequenceEqual("<Error>\0"u8.ToArray())
            && ReadAt(VirtualAddressToFileOffset(0x1C78658), 21).SequenceEqual("<Waiting For Server>\0"u8.ToArray())
            && ReadAt(VirtualAddressToFileOffset(0x1C78670), 12).SequenceEqual("[Exclusive]\0"u8.ToArray()));
        check("Binary profiler snapshot writes child subtrees before raw expansion byte and iterates eighty physical samples",
            Matches(0xE72715, "8B068BCE57FF50048B760885F675F1")
            && Matches(0xE72724, "8D43108BCF50E8A14C4BFF")
            && Matches(0xE8DD18, "BB50000000")
            && Matches(0xE8DD75, "83C71C4B75A5"));
        check("Binary profiler snapshot writes sample scalar fields rather than native padding and appends raw next-slot DWORD",
            Matches(0xE8DD20, "8D47F88BCE50E8E59449FF")
            && Matches(0xE8DD49, "8D47088BCE50E8BC9449FF")
            && Matches(0xE8DD54, "8D470A8BCE50E8B19449FF")
            && Matches(0xE8DD80, "05D408000050E8859549FF"));
        check("Binary concrete tfServerProfiler vtable slot8 is the dual-family request method with exact original RTTI",
            ReadUInt32(VirtualAddressToFileOffset(NativeProfilerNameRequests.EndpointVtableVirtualAddress) + 8)
                == NativeProfilerNameRequests.EndpointMethodFileOffset + 0x400C00
            && ReadUInt32(VirtualAddressToFileOffset(0x1C61B4C)) == 0x1C95220
            && ReadUInt32(VirtualAddressToFileOffset(0x1C95220) + 12) == 0x1E0391C
            && ReadAt(VirtualAddressToFileOffset(0x1E03924), ".?AVtfServerProfiler@@\0"u8.Length)
                .SequenceEqual(".?AVtfServerProfiler@@\0"u8.ToArray()));
        check("Binary profiler requester checks mode2 first and its two observer gates use different signed thresholds/log masks",
            Matches(0x6AAB54, "83B8800B0000020F85DC000000")
            && Matches(0x6AADCF, "837E0C037E2BF6461401")
            && Matches(0x6AAD30, "8B460C83F8047E1C")
            && Matches(0x6AAD75, "F6461402"));
        check("Binary firefall18 request scales UInt32 tick to64bits and stamps both observer routing words without type-byte forcing",
            Matches(0x6AAD62, "B9E8030000F7E18986B00000008996B4000000")
            && Matches(0x6AAD95, "8B8E780800008B967C0800005189481889501C"));
        check("Binary matrix30 profiler request emits header30 and both descriptor WORD lists",
            Matches(0x6AB0B5, "C745EC1E000000")
            && Matches(0x3461A9, "8D5728E87F2E0000")
            && Matches(0x3461B4, "8D5734E8742E0000")
            && Matches(0x6AAEE9, "6A01E8308C5200"));
        check("Binary firefall18 profiler request emits raw routing QWORD before compact header",
            Matches(0x6AB201, "8D450C8BCE50E8D4A9C7FF568D4D08E8EB56C7FF"));

        check("Binary character-refresh pair loads use Login +9E8/+9EC",
            Matches(0x63ED7D, "8B87EC0900008B8FE8090000"));
        check("Binary character-refresh pair stores use request +50/+54",
            Matches(0x63EDBC, "8946548B45E8894650"));
        check("Binary zone-list builder attaches Login +A00 pair to type-2 request",
            Matches(0x63E553, "8D86000A0000")
            && Matches(0x63E562, "8D4B50C7430802000000E86F630000"));
        check("Binary auto-login state 2 reaches the traced branch",
            ReadUInt32(LoginModeNativeLayout.AutoLoginStateJumpTableFileOffset + 2 * 4)
                == 0xA3F054);
        check("Binary worker reads type +8 and branches through the translated verb table",
            Matches(0xBC2C81, "8B410883C0FE83F8037737FF2485C044FC00")
            && Matches(0xBC2DB7, "FF15FC4CAC01"));

        uint[] cases = [0xBC2C93, 0xBC2C9F, 0xBC2CAB, 0xBC2CB7];
        uint[] literals = [0x1C735E8, 0x1C735E0, 0x1C73554, 0x1C73548];
        string[] verbs = ["POST", "PUT", "DELETE", "HEAD"];
        for (int i = 0; i < cases.Length; i++)
        {
            // These tables are in .text; the wide literals are in .rdata.
            long literalRaw = 0x16C2C00L + literals[i] - 0x400000 - 0x16C4000;
            check($"Binary request type {i + 2} selects {verbs[i]}",
                ReadUInt32(ClientApiRequestNativeLayout.MethodJumpTableFileOffset + i * 4)
                    == cases[i] + 0x400C00
                && ReadUInt32(cases[i] + 6) == literals[i]
                && ReadAt(literalRaw, (verbs[i].Length + 1) * 2)
                    .SequenceEqual(Encoding.Unicode.GetBytes(verbs[i] + "\0")));
        }

        check("Binary default request type selects the UTF-16 GET literal",
            ReadUInt32(0xBC2CC3 + 6) == 0x1C735A8
            && ReadAt(0x18721A8, 8).SequenceEqual(Encoding.Unicode.GetBytes("GET\0")));
        check("Binary completion gate requires a cleared request handle",
            Matches(ClientApiCallbackDeliveryNativeLayout.CompletionGateFileOffset,
                "833E000F85E7050000"));
        check("Binary worker acquires request +50 and invokes callback slot +4",
            Matches(0xBC37DF, "83C150E899120000")
            && Matches(0xBC37F8, "8B01FF37FF5004"));
        check("Binary strong-reference gate rejects a zero count",
            Matches(0x327734, "8B0E85C97419"));
        check("Binary outgoing runtime receiver is translated to zero-initialized .data",
            Matches(0x63CEF2, "8B0DB802E001")
            && ReadUInt32(LoginModeNativeLayout.CharacterRefreshRequestReceiverGlobalFileOffset) == 0);
        check("Binary Login response dispatches message 0x145 through the shared runtime endpoint",
            Matches(LoginModeNativeLayout.LoginResponseNotificationCallSiteFileOffset,
                "8B0DB802E0018D55846A00526845010000"));
        check("Binary tfLogin update queues World request pair {2,0}",
            Matches(LoginModeNativeLayout.LoginUpdateMethodFileOffset,
                "558BEC83E4F8")
            && Matches(LoginModeNativeLayout.VtableFileOffset + LoginModeNativeLayout.LoginUpdateMethodVtableOffset,
                "C091A300")
            && Matches(LoginModeNativeLayout.LoginUpdateWorldModeQueueCallSiteFileOffset,
                "C744241C02000000C7442420000000008D44241CC7842430010000060000008B0D98FDDF0150E8FC52CDFF"));
        check("Binary tfLogin update {2,0} queue is guarded by byte [dword_1E5CAA4+0x40]==0 and queue push uses +0xB84",
            Matches(LoginModeNativeLayout.LoginUpdateDefaultCaseFileOffset,
                "C7461C00000000A1A4CAE5018A404084C00F84BD000000A18C0CE501")
            && Matches(LoginModeNativeLayout.PendingRequestEnqueueFlagCheckFileOffset,
                "8BD9807B0C008B4D080F84230100008339010F851A")
            && Matches(LoginModeNativeLayout.PendingRequestEnqueuePushFileOffset,
                "8B71048B018945DCC745E00000000085F674178D4DE0E80BA5FA008975E08D4EFCB801000000F00FC1018D45DCC745FC00000000508D8B840B0000E8167000"));
        check("Binary tfLogin +0x20 is a ctor-supplied pointer cell (App+0x9E8), fetched/dispatched by update; case1 sends 0x146 with node+0x10",
            Matches(LoginModeNativeLayout.LoginConstructorCellStoreFileOffset,
                "8B45088946208B450C89462433C0C7461C00000000C6")
            && Matches(LoginModeNativeLayout.LoginUpdateMessageFetchFileOffset,
                "8B4E208B0185C0742D8B561C3BC2742685D2741A6A006805020000689561B401689661B401FF15A05EC10183C4108B4E208B0189461C8B461C85")
            && Matches(LoginModeNativeLayout.LoginUpdateMessageDispatchFileOffset,
                "8B4E208B0189461C8B461C85C00F84F50200008B500C83FA040F87")
            && Matches(LoginModeNativeLayout.LoginUpdateMessageCase1FileOffset,
                "8B461C8B401085C00F45C85168781AB6018D8C24C4000000E8C5B35E008B0DB802E0018D9424BC0000006A005268460100008B01FF90AC00")
            && Matches(LoginModeNativeLayout.LoginUpdateMessageCase1ReleaseFileOffset,
                "8B4E1C85C974068B016A01FF1083BE480A000006C7461C0000000075")
            && Matches(LoginModeNativeLayout.AppModeLoginCtorCallArgumentsFileOffset,
                "FFB6280A00008D86E809000050E88C893200EB"));
        check("Binary App+0x9E8 is a {ptr, refcount-block} cell: ctor/mode-apply zero it, dtor releases it, no other direct store",
            Matches(LoginModeNativeLayout.AppCtorCellZeroFileOffset,
                "C787E809000000000000C787EC09000000000000")
            && Matches(LoginModeNativeLayout.AppModeApplyCellZeroFileOffset, "C786E809000000000000")
            && Matches(LoginModeNativeLayout.AppDtorCellReleaseFileOffset,
                "8B4DF081C1E8090000E9396A03FF")
            && Matches(LoginModeNativeLayout.RefCountedCellReleaseHelperFileOffset,
                "568B710485F674295783CFFF8D4E048BC7F00FC10175198B068BCEFF108D4608F00FC1384F75098B068BCE5F5EFF6004")
            && Matches(LoginModeNativeLayout.AppModeLoginCellPassedToOtherModeCtorsFileOffset, "FFB6E8090000"));
        check("Binary tfLogin update switch selects on [node+0xC] with a five-entry table (case1 -> message 0x146)",
            Matches(LoginModeNativeLayout.LoginUpdateSwitchSelectorLoadFileOffset,
                "8B500C83FA040F87CF010000FF24951C99A300")
            && ReadUInt32(LoginModeNativeLayout.LoginUpdateSwitchJumpTableFileOffset) == 0xA3983A
            && ReadUInt32(LoginModeNativeLayout.LoginUpdateSwitchJumpTableFileOffset + 4) == 0xA395F2
            && ReadUInt32(LoginModeNativeLayout.LoginUpdateSwitchJumpTableFileOffset + 8) == 0xA3983A
            && ReadUInt32(LoginModeNativeLayout.LoginUpdateSwitchJumpTableFileOffset + 12) == 0xA3983A
            && ReadUInt32(LoginModeNativeLayout.LoginUpdateSwitchJumpTableFileOffset + 16) == 0xA3983A
            && Matches(LoginModeNativeLayout.LoginUpdateSwitchDefaultTargetFileOffset, "8901C7461C00000000A1A4CAE501")
            && Matches(LoginModeNativeLayout.LoginUpdateCase1MessageSendFileOffset, "6846010000" + "8B01FF90AC000000"));
        check("Binary worker registers the concrete WinHTTP status callback",
            Matches(0xBC31DE, "6A006AFF68E032FC00FF37FF150C4DAC01")
            && Matches(0xBC278E, "C21400"));
        check("Binary worker passes completion context directly to WinHttpSendRequest",
            Matches(0xBC31F8, "57")
            && Matches(0xBC3213, "FF15104DAC01")
            && Matches(0xBC2D47, "894718")
            && Matches(0xBC2DEC, "8937")
            && Matches(0xBC3280, "89781C"));
        check("Binary HANDLE_CLOSING preserves assigned results and clears request handle",
            Matches(0xBC270C, "81FA00080000")
            && Matches(0xBC2714, "8B4618837808007507C7400803000000")
            && Matches(0xBC2728, "C70600000000"));
        check("Binary terminal callback delegates status handling then closes WinHTTP handle",
            Matches(0xBC275F, "E83CF6FFFF")
            && Matches(0xBC2785, "FF15F84CAC01")
            && Matches(0xBC322E, "C70700000000"));
        var zoneKeys = new (uint Address, string Key)[]
        {
            (0x1B62160, "zone_list"), (0x1C6775C, "zone_id"),
            (0x1C67440, "zone_name"), (0x1C67468, "protocol_version"),
            (0x1C67460, "build"), (0x1C67454, "matrix_url"), (0x1C6744C, "players")
        };
        check("Binary zone-list JSON key literals match the field catalog",
            zoneKeys.All(item =>
                ReadAt(item.Address - 0x401400, item.Key.Length + 1)
                    .SequenceEqual(Encoding.ASCII.GetBytes(item.Key + "\0"))));
        check("Binary zone response gates state 3 and requires protocol 4C5F with nonzero players",
            Matches(0x63D2A9, "83B8480A000003")
            && Matches(0x63D3DD, "B85F4C000066394608750766837E1400751C"));
        check("Binary zone response increments the auto-login state at +A48",
            Matches(0x63D652, "FF80480A0000"));
        check("Binary auto-login state 5 reaches selected-character connection preparation",
            ReadUInt32(LoginModeNativeLayout.AutoLoginStateJumpTableFileOffset + 5 * 4) == 0xA3F490
            && Matches(0x63E919, "8D87C8000000"));
        var cleanupSetter = ReadAt(ClientApiWinHttpCompletionNativeLayout.MarkCallbackOwnedCleanupFileOffset, 4);
        check("Binary worker teardown marks completion byte +11 for callback-owned cleanup",
            cleanupSetter[0] == 0xC6 && (cleanupSetter[1] & 0xF8) == 0x40
            && cleanupSetter[2] == 0x11 && cleanupSetter[3] == 1);
        check("Binary oracle callback wrapper has owner +4, target +8, and adjustment +C",
            Matches(0x48F0BE, "C700D841C601895804C7400860608900C7400C00000000")
            && ReadUInt32(MatrixConnectionNativeLayout.OracleCallbackVtableFileOffset + 4) == 0x8979F0
            && Matches(0x496DF3, "8BC18B480C0348048B40085DFFE0"));
        check("Binary oracle-ticket endpoint and response-key literals match reconstruction",
            ReadAt(0x1B520E8 - 0x401400, MatrixConnectionNativeLayout.EndpointFormat.Length + 1)
                .SequenceEqual(Encoding.ASCII.GetBytes(MatrixConnectionNativeLayout.EndpointFormat + "\0"))
            && new (uint Address, string Key)[]
            {
                (0x1B52244, "matrix_url"), (0x1B52250, "ticket"), (0x1B52258, "datacenter"),
                (0x1B52360, "operator_override"), (0x1B52374, "session_id"), (0x1B52380, "hostname")
            }.All(item => ReadAt(item.Address - 0x401400, item.Key.Length + 1)
                .SequenceEqual(Encoding.ASCII.GetBytes(item.Key + "\0"))));
        check("Binary oracle request sets POST type and attaches the traced callback pair",
            Matches(0x495101, "C7470802000000")
            && Matches(0x49510C, "E86F350000")
            && Matches(0x49540C, "C6431C01")
            && Matches(0x4954A0, "C6461C00"));
        check("Binary matrix transport-open success moves connection to state 3",
            Matches(0x4962C3, "C7430C03000000"));
        check("Binary matrix socket factory requests overlapped UDP",
            Matches(0xBE354A, "6A016A006A006A11B8170000006A020F44C150FF15684DAC01"));
        check("Binary matrix local binding uses seven or two attempts and final-two fallback",
            Matches(0xBD0C66, "BA020000000FB701B9070000006685C08BD80F44CA")
            && Matches(0xBD0C91, "8D51FE33C93BC28B1570FFDF010F4DD9"));
        check("Binary matrix socket posts WSARecvFrom and has a separate opening state",
            Matches(MatrixTransportNativeLayout.InitialReceiveCallFileOffset, "FF15544DAC01")
            && Matches(0xBD1023, "C7870801000001000000"));
        check("Binary initial application header and success-path literal match packet reconstruction",
            Matches(0x498A35, "C745EC11000000")
            && Matches(0x49638B, "68AA07000050E8BA1EEAFF"));
        check("Binary initial packet encoder calls the observed byte, DWORD and pair primitives",
            Matches(0x349E6D, "8D432850E85AD5FDFF8D432C8BCF50E88FD4FDFF")
            && Matches(0x349EBD, "8D43388BCF50E818BDFDFF")
            && Matches(0x349EDE, "8D46088BCF50E827D3FDFF"));
        check("Binary optional field emits one-byte presence and trailing blob reserves then copies",
            Matches(0x349F46, "8D450B807B58008BCF507436C6450B01E875D4FDFF")
            && Matches(0x349F88, "C6450B00E83FD4FDFF")
            && Matches(0x349F9E,
                "8BCF53E85A6BFDFF8B4E0485C974058B4904EB0233C985C0740F85C9740B535150E83039250183C40C"));
        check("Binary transport submission switch translates to separate channel 0, 1/2 and 3 paths",
            ReadUInt32(0xBD3E3C) == 0xFD4819
            && ReadUInt32(0xBD3E40) == 0xFD483C
            && ReadUInt32(0xBD3E44) == 0xFD483C
            && ReadUInt32(0xBD3E48) == 0xFD49C6);
        check("Binary game socket constructor initializes the segment limit to 1388 bytes",
            Matches(0xBD012C, "C7860201000000006C05"));
        check("Binary payload attempt-mask table has repeated zero/FF/AA/CC DWORD pairs",
            ReadAt(MatrixDatagramFragmentEncoder.PayloadMaskTableFileOffset, 32)
                .SequenceEqual(Convert.FromHexString(
                    "0000000000000000FFFFFFFFFFFFFFFFAAAAAAAAAAAAAAAACCCCCCCCCCCCCCCC")));
        check("Binary GameSocket attaches Network and associates IOCP completion key with the socket",
            Matches(0xBCFDF3, "8B450889410885C0")
            && Matches(0xBBD8AB, "6A00FF7508FFB3DC00000056FF153443AC01"));
        check("Binary IOCP success selects receive or close slots from the completion-key object",
            Matches(0xBBC9AB, "837DF4008B017405FF5018EB03FF501C")
            && Matches(0x1872674, "303AFD001039FD007039FD00"));
        check("Binary receive callback decodes retained sender address and reaches packet entry",
            Matches(0xBD2F0B,
                "8B8EF40D00008D4424345083C118E8E20A010050FF7424148BCEE806010000")
            && Matches(0xBD2FDC, "FFB618010000FF15544DAC01"));
        check("Binary IOCP import calls and cached completion-dequeue call match exact instruction starts",
            Matches(NetworkIoCompletionNativeLayout.CreatePortCallFileOffset, "FF153443AC01")
            && Matches(NetworkIoCompletionNativeLayout.AssociateSocketCallFileOffset, "FF153443AC01")
            && Matches(0xBBC882, "A12C43AC01")
            && Matches(NetworkIoCompletionNativeLayout.DequeueCompletionCallFileOffset, "FF55E8")
            && Matches(GameSocketReceiveNativeLayout.GetOverlappedResultCallFileOffset, "FF15644DAC01")
            && Matches(GameSocketReceiveNativeLayout.RepostReceiveCallFileOffset, "FF15544DAC01"));
        check("Binary Network and GameSocket shutdown ownership and distinct WinHTTP close helpers are fingerprinted",
            ReadUInt32(VirtualAddressToFileOffset(0x1C734B4)) == 0x00FBD900
            && ReadUInt32(VirtualAddressToFileOffset(0x1C734B4) + 8) == 0x00FBE540
            && ReadUInt32(VirtualAddressToFileOffset(0x1C73A5C)) == 0x00FD0FD0
            && ReadUInt32(VirtualAddressToFileOffset(0x1C73A5C) + 0x1C) == 0x00FD3910
            && Matches(0xBBD940, "558BEC837D0800578BF9")
            && Matches(0xBBD9C7, "6A02FF36FF15D44DAC01FF36FF15C84DAC01C706FFFFFFFF")
            && Matches(0xBD048E, "83BE18010000FF8D8E180100007405E80E320100")
            && Matches(0xBE36E3, "6A02FF36FF15D44DAC01FF36FF15C84DAC01C706FFFFFFFF")
            && Matches(0xBC3970, "568BF18B462085C0740E50FF15F84CAC01C74620000000005EC3")
            && Matches(0xBC3990, "568BF18B0685C0740D50FF15F84CAC01C706000000005EC3")
            && ImportSlotNames(0x1AC4CF8, "WinHttpCloseHandle")
            && Matches(0xBBDA20, "558BEC83EC14535657")
            && ReadUInt32(0x183CCFD) == 0x00BBDA20
            && ReadUInt32(0x183CE7D) == 0x00BBE620);
        check("Binary oracle submission calls the shared matrix request wrapper",
            Matches(MatrixConnectionNativeLayout.OracleRequestSubmissionFileOffset, "E80C5B0000"));
        check("Binary packet admission uses unsigned count-minus-five and a network-order context word",
            Matches(0xBD303C, "8D47FB3D730500000F87C1000000")
            && Matches(0xBD3050, "FF15944DAC013B8614010000")
            && Matches(0xBD3074, "66837B1800747F"));
        check("Binary zero-context handshake response magic and abort mapping match reconstruction",
            ReadAt(0x17B5BCC, 4).SequenceEqual("HEHE"u8.ToArray())
            && ReadAt(0x17B5BD4, 4).SequenceEqual("HUGG"u8.ToArray())
            && ReadAt(0x17B5CC4, 4).SequenceEqual("ABRT"u8.ToArray())
            && Enumerable.Range(0, 6).Select(i => ReadUInt32(0x1872638 + 4 * i))
                .SequenceEqual(new uint[] { 0, 12, 12, 10, 11, 13 }));
        string[] stateNames = Enum.GetNames<NativeGameSocketState>();
        check("Binary GameSocket state-name pointer table matches all ten logged states",
            stateNames.Select((name, index) => new
            {
                Name = name,
                Address = ReadUInt32(GameSocketStateAndControlCatalog.StateNameTableFileOffset + 4 * index)
            }).All(item => ReadAt(item.Address - 0x401400, item.Name.Length + 1)
                .SequenceEqual(Encoding.ASCII.GetBytes(item.Name + "\0"))));
        uint[] tickTargets =
        [
            0xBD4C72, 0xBD4D5E, 0xBD4F66, 0xBD4F93, 0xBD4FB4,
            0xBD5028, 0xBD5028, 0xBD4FCD, 0xBD4FFD
        ];
        check("Binary locked tick table and context-ready handshake branches match reconstruction",
            tickTargets.Select((target, i) =>
                ReadUInt32(GameSocketTickNativeLayout.JumpTableFileOffset + 4 * i) == target + 0x400C00).All(v => v)
            && Matches(0xBD4C72, "83BE24020000000F84AE000000")
            && Matches(0xBD4D17, "8BCEC7860801000002000000E8A8D9FFFF")
            && Matches(0xBD4D5E, "83BE14010000000F84FE00000083BE780D0000000F84E3000000"));
        check("Binary reliable activation sets sequence seed, seed-minus-one frontiers and 1000ms retry",
            Matches(0xBE3FAB,
                "C6470C01668977286689772A8D46FFC74718E80300006689472E6689472C66894730"));
        uint[] queuedTargets = [0xBD4141, 0xBD415A, 0xBD418D, 0xBD41C0];
        check("Binary queued receive channel switch and Connecting control-only gate match reconstruction",
            queuedTargets.Select((target, i) =>
                ReadUInt32(GameSocketQueuedReceiveNativeLayout.ChannelJumpTableFileOffset + 4 * i)
                    == target + 0x400C00).All(v => v)
            && Matches(0xBD40FB, "83BE08010000037527B800C000006685070F8502010000578BCE"));
        check("Binary owner packet callbacks use slots 4/8 and disconnect uses slot C",
            Matches(0xBD5253, "FF5004") && Matches(0xBD533F, "FF5008")
            && Matches(0xBD5491, "FF5008") && Matches(0xBD55F1, "FF500C")
            && ReadUInt32(MatrixConnectionNativeLayout.TransportHandlerVtableFileOffset + 4)
                == MatrixConnectionNativeLayout.ReceiveChannel1MethodFileOffset + 0x400C00
            && ReadUInt32(MatrixConnectionNativeLayout.TransportHandlerVtableFileOffset + 8)
                == MatrixConnectionNativeLayout.ReceiveOtherChannelsMethodFileOffset + 0x400C00
            && ReadUInt32(MatrixConnectionNativeLayout.TransportHandlerVtableFileOffset + 12)
                == MatrixConnectionNativeLayout.TransportDisconnectedMethodFileOffset + 0x400C00);
        check("Binary secondary receiver dispatches all 35 descriptor IDs through the recorded handlers",
            MatrixServerMessageCatalog.Entries.Select((entry, i) =>
            {
                uint caseOffset = i < 31 ? 0x33CE7D + (uint)i * 40 : 0x33D355 + (uint)(i - 31) * 43;
                uint logAddress = ReadUInt32(caseOffset + 8);
                return ReadUInt32(MatrixServerMessageCatalog.DispatchJumpTableFileOffset + 4 * i)
                        == caseOffset + 0x400C00
                    && ReadUInt32(MatrixServerMessageCatalog.ReceiverVtableFileOffset + entry.ReceiverVirtualOffset)
                        == entry.HandlerFileOffset + 0x400C00
                    && ReadAt(logAddress - 0x401400, entry.NativeLogName.Length + 1)
                        .SequenceEqual(Encoding.ASCII.GetBytes(entry.NativeLogName + "\0"));
            }).All(v => v)
            && Matches(0x33CE67, "8B471083C0DE83F8220F878B050000FF248524E07300")
            && Matches(0x4904A3, "8B450883C104FF30E890C9EAFF"));
        check("Binary msg-35 copies its two words and advances secondary receiver state to 4",
            Matches(0x490B14, "89865C0800008B412C898660080000")
            && Matches(0x490BB4, "C7460804000000C6863908000000")
            && ReadAt(0x1B51C10 - 0x401400, 20).SequenceEqual("Connected to matrix\0"u8.ToArray()));
        check("Binary EnterZone handler requires state 4 and writes state 6 with the named log",
            Matches(0x490C8A, "837E0804")
            && Matches(0x490CAA, "C7460806000000")
            && ReadAt(0x1B51BD4 - 0x401400, MatrixEnterZoneNativeLayout.NativeLogFormat.Length + 1)
                .SequenceEqual(Encoding.ASCII.GetBytes(MatrixEnterZoneNativeLayout.NativeLogFormat + "\0")));
        check("Binary reliable receive uses the shifted 16-bit distance gate",
            Matches(0xBE5237, "2BC3C74508FF7E00000FB7D88D8300FFFFFF663B45087740"));
        check("Binary matrix constructor installs distinct channel-one and other-channel decoders",
            Matches(0x48F12C, "C78328080000E0EE7300B868B9000066898324080000B85F4C00006689832C080000")
            && Matches(0x48F14E, "C78330080000A027C200"));
        check("Binary compact header rejects noncanonical extended values and calls the descriptor factory",
            Matches(0x34A0C8, "3DFF000000770AEB118B4DFC0FB6C38901")
            && Matches(0x33E3E6, "8B45108D4DDC518BD48902E8BAF0FFFF"));
        check("Binary msg-35 body reads its QWORD and two blobs at the verified descriptor offsets",
            Matches(0x3570BA, "8D462850E8BDFEFFFF8D56308BCFE8A370F9008D56408BCFE89970F900")
            && Matches(0x356F9D, "8B56048B4D088B0289018B420489410483460408")
            && Matches(0x12EE192, "8B47040FB73083C002837F0802894704"));
        check("Binary msg-37 factory allocates C8 bytes and invokes the verified body reader",
            Matches(0x33D640, "68C8000000E866A9F700")
            && Matches(0x33D675, "E8E6970100")
            && Matches(0x356E6B, "538D4F28E8FCF8FFFF")
            && Matches(0x356E98, "538D8F90000000E8CCFCFFFF8D97C00000008BCBE83FFEFFFF"));
        check("Binary optional metadata tests presence and parses the two adjacent temporary maps",
            Matches(0x35663E, "84C90F84BB000000")
            && Matches(0x356680, "8D54241CC7442438010000008BCFE8ADEAFFFF")
            && Matches(0x356693, "8D5424248BCFE8B2F7FFFF")
            && Matches(0x35669E, "8D44241C8BCB50E836FDFFFF"));
        check("Binary metadata maps read NUL byte keys then raw WORD or DWORD values",
            Matches(0x3551F7, "E8148EF900")
            && Matches(0x355213, "8B46040FB73883C002")
            && Matches(0x35524D, "E84EFEFFFFC745FCFFFFFFFF668938")
            && Matches(0x355F07, "E80481F900")
            && Matches(0x355F23, "8B46048B3883C004")
            && Matches(0x355F5C, "E84FFEFFFFC745FCFFFFFFFF8938"));
        check("Binary metadata maps continue only after a full 255-entry chunk",
            Matches(0x3552B9, "80FBFF0F84CEFEFFFF")
            && Matches(0x355FC7, "80FBFF0F84D0FEFFFF"));
        check("Binary metadata lookup compares unsigned key bytes and returns existing value slots",
            Matches(0x3550D0, "8A013A02751A84C07412")
            && Matches(0x3550F0, "1BC083C80185C0780A8D4614"));
        check("Binary optional metadata copy preserves both maps and normalizes presence",
            Matches(0x35644F, "518D4608C645FC01508D4B08E8E0270000C60701"));
        check("Binary EnterZone suffix normalizes its boolean without serializing padding",
            Matches(0x356C35, "FF4E0884DB0F95C0884728"));
        check("Binary EnterZone initializes the embedded clock, samples connection time and advances it",
            Matches(0x490D42, "538D8E14090000E8D2984600")
            && Matches(0x490D4E, "8D4EFCE8FA280000")
            && Matches(0x490D56, "52508D8E14090000E89D994600"));
        check("Binary clock initialization copies a double rate and seeds current time with an offset",
            Matches(0x8FA656, "F20F104210F20F114610")
            && Matches(0x8FA660, "8B4A20030A8B4224134204894E1889461C")
            && Matches(0x8FA684, "8A4228884630807E3000"));
        check("Binary clock scale branch uses 0.001 tolerance around 1.0",
            Matches(0x8FA797, "F20F104A10F20F5C0DA01DC801")
            && Matches(0x8FA7B0, "660F2FC17329")
            && ReadAt(0x1C81B68 - 0x401400, 8).SequenceEqual(Convert.FromHexString("FCA9F1D24D62503F"))
            && ReadAt(0x1C81DA0 - 0x401400, 8).SequenceEqual(Convert.FromHexString("000000000000F03F")));
        check("Binary clock rate converts unsigned elapsed ticks, multiplies a double and converts back",
            Matches(0x8FA7B6, "8B7A082BCF8B720C1BC68BD0E8B739CA00")
            && Matches(0x8FA7C7, "8B45FCF20F594010E88C38CA00")
            && Matches(0x8FA7DB, "03CF13C6034A28"));
        check("Binary unsigned conversion uses 2^64 and negative 2^63 bounds",
            Matches(0x159E060, "B904000000F20F102D30F0B301")
            && Matches(0x159E15D, "660F2FC50F8394FEFFFF")
            && Matches(0x159E167, "F20F101520F0B301660F2FC20F8282FEFFFF")
            && ReadAt(0x1B3F030 - 0x401400, 8).SequenceEqual(Convert.FromHexString("000000000000F043"))
            && ReadAt(0x1B3F020 - 0x401400, 8).SequenceEqual(Convert.FromHexString("000000000000E0C3")));
        check("Binary paused event tests the clock limit flag and posts runtime message 110",
            Matches(0x490D63, "8A864409000084C07463")
            && Matches(0x490D76, "6A0168941CB501")
            && Matches(0x490D9E, "6A6E8B01FF90AC000000")
            && ReadAt(0x1B51C94 - 0x401400, 7).SequenceEqual("paused\0"u8.ToArray()));
        check("Binary downstream activation and independent message-24 request byte match the saved plan",
            Matches(0x490F87, "837F2800")
            && Matches(0x490FAA, "C647180180BE900B000000")
            && Matches(0x490FF0, "8D4EFCE858730000"));
        check("Binary message-24 gate requires transport and signed connection state greater than 4",
            Matches(0x498357, "83BE200800000074248B460C83F8047E1C")
            && Matches(0x49838C, "FF15244DAC01B9E8030000F7E18986E00000008996E4000000")
            && Matches(0x4983C2, "8B45088B8E780800008B967C0800005189481889501C"));
        check("Binary message-24 encoder writes context before its compact code and submits channel 2",
            Matches(0x4998CB, "FF701CFF7018518BC4C70018000000E8410C0000")
            && Matches(0x49A591, "8D450C8BCF50E844B6E8FF578D4D08E85B63E8FF")
            && Matches(0x499903, "83EC088BCEFF75E06A02E80EA27300"));
        check("Binary mode-specific getter selects existing Login/World/LookingGlass object pointers",
            Matches(0x6F4588, "8B4068")
            && Matches(0x6F45D5, "8B86E8280000")
            && Matches(0x6F4607, "8B4014")
            && Matches(0x49101D, "C780E001000001000000")
            && Matches(0x491027, "80B8E801000000")
            && Matches(0x491039, "C780E401000001000000"));
        check("Binary synchronized clock converts elapsed u64 through double and single, then scales a float",
            Matches(0xBE844B, "2B47188BC81B571CE8265D9B00")
            && Matches(0xBE8458, "F20F5AC0F30F594714E8195A9B00"));
        check("Binary synchronized clock adds its base and returns a monotonic EDX:EAX sample",
            Matches(0xBE846B, "035F208B4764135724")
            && Matches(0xBE8498, "895F6089576489542410")
            && Matches(0xBE84AC, "8B5424108BC35F5E5B8BE55DC3"));
        check("Binary msg-38 factory and packed raw8/raw4/raw4 reader match its scalar-update model",
            Matches(0x33D6A6, "6A38E803A9F700")
            && Matches(0x33D6C2, "E879CCFFFF")
            && Matches(0x33D6D8, "E8F36E0100")
            && Matches(0x3545DD, "56E86D2200008D46088BCF50E8F22200008D460C8BCF50E8E7220000"));
        check("Binary msg-38 copies descriptor+28's 16 bytes into connection+8D0",
            Matches(0x491386, "F30F6F4028F30F7F81CC080000"));
        check("Binary msg-39 factory reads a byte and its handler applies it before storing connection+8A0",
            Matches(0x33D709, "6A30E8A0A8F700")
            && Matches(0x33D725, "E8F6CCFFFF")
            && Matches(0x33D730, "8D5628C7442420FFFFFFFF8BCFE83E9AFFFF")
            && Matches(0x4913D9, "0FB6462850E81D55EFFF")
            && Matches(0x4913E3, "8A46285F5E88839C080000"));
        check("Binary queue-status factory and body read the two DWORDs reflected in runtime properties",
            Matches(0x33DF8E, "6A30E81BA0F700")
            && Matches(0x33DFAA, "E861E3FFFF")
            && Matches(0x33DFC0, "E84BD60000")
            && Matches(0x34B61A, "8D462850E87DBAFEFF8D462C8BCF50E872BAFEFF"));
        check("Binary queue-status handler posts runtime295 with original property labels",
            Matches(0x4911BC, "FF762868381DB501E847307900")
            && Matches(0x4911C9, "FF762C8D4D9468B01CB501E837307900")
            && Matches(0x4911E5, "68270100008B01FF90AC000000")
            && ReadAt(0x1B51D38 - 0x401400, 15).SequenceEqual("queue_position\0"u8.ToArray())
            && ReadAt(0x1B51CB0 - 0x401400, 13).SequenceEqual("queue_length\0"u8.ToArray()));
        check("Binary ExitZone factory is header-only and original handler log identifies msg40",
            Matches(0x33D76E, "6A28E83BA8F700")
            && Matches(0x33D78E, "E84DCDFFFF")
            && ReadAt(0x1B51CC0 - 0x401400, 18).SequenceEqual("Received ExitZone\0"u8.ToArray()));
        check("Binary ExitZone cleanup accepts state6/7 and branch code writes state1 or4",
            Matches(0x494B80, "8B470C83F806741F83F807741A")
            && Matches(0x491277, "80BF900B000000")
            && Matches(0x4912C9, "C7470801000000")
            && Matches(0x491363, "C7470804000000"));
        check("Binary logged ExitZoneAck uses code19 and its encoder writes only that compact header",
            ReadAt(0x1B51CEC - 0x401400, 20).SequenceEqual("Sending ExitZoneAck\0"u8.ToArray())
            && Matches(0x4999A5, "C745EC13000000")
            && Matches(0x498DF6, "E8750B0000"));
        check("Binary ExitZone state5 and outgoing ack header/channel are explicit instruction selections",
            Matches(0x494BA7, "C7470C05000000")
            && Matches(0x4999E4, "E8176FE8FF")
            && Matches(0x498E27, "6A01"));
        check("Binary socket destroy switch separates active-close, active-deferred and already-disconnected states",
            ReadAt(GameSocketDestroyNativeLayout.StateIndexTableFileOffset, 10)
                .SequenceEqual(new byte[] { 0, 1, 1, 2, 1, 2, 0, 0, 3, 3 })
            && ReadUInt32(0xBD2408) == 0xFD2ECE && ReadUInt32(0xBD240C) == 0xFD2D60
            && ReadUInt32(0xBD2410) == 0xFD2E1C && ReadUInt32(0xBD2414) == 0xFD2F98
            && Matches(0xBD2109, "C7860C01000000000000")
            && Matches(0xBD2113, "BA06000000")
            && Matches(0xBD211A, "F00FB111"));
        check("Binary socket destroy writes Destroyed/Closed and dispatches close/channel cleanup without freeing itself",
            Matches(0xBD2206, "C7860801000008000000")
            && Matches(0xBD2212, "E819020000")
            && Matches(0xBD22BF, "C7860801000008000000")
            && Matches(0xBD232A, "E881130100")
            && Matches(0xBD2387, "E824E3FFFF")
            && Matches(0xBD238C, "C7860801000009000000"));
        check("Binary world cleanup posts null-body runtime97 and resets raw member bytes",
            Matches(0x64E288, "6A006A006A61")
            && Matches(0x64E290, "FF90AC000000")
            && Matches(0x64E34E, "C680AB01000000C680A901000001")
            && Matches(0x64E395, "C7809055000000000000")
            && Matches(0x64E3B3, "C683F102000000"));
        check("Binary Destroyed-state tick selects keepalive/retries/assembler and assembler tail requests pump close",
            ReadUInt32(GameSocketCloseCompletionNativeLayout.TickStateTableFileOffset + 7 * 4) == 0xFD5BCD
            && Matches(0xBD4FCD, "80BE950D000000")
            && Matches(0xBD4FD8, "E8D3DBFFFF")
            && Matches(0xBD4FE7, "E8E4DAFFFF")
            && Matches(0xBD4FF6, "E845F3FFFF")
            && Matches(0xBD475A, "83BE0801000008")
            && Matches(0xBD4765, "E8C6DCFFFF"));
        check("Binary close callback sets D94 and enqueues token1, whose dispatcher closes Destroyed or notifies active state",
            Matches(0xBD2D13, "83BE18010000FF")
            && Matches(0xBD2D36, "80BE940D000000")
            && Matches(0xBD2D59, "6A018D8E60020000C686940D000001")
            && Matches(0xBD3F64, "83F808")
            && Matches(0xBD4004, "E8A7C6FFFF")
            && Matches(0xBD4009, "C7860801000009000000")
            && Matches(0xBD40B0, "C7860801000006000000"));
        check("Binary messages41-43 factory sizes, constructors and body calls match the saved formats",
            Matches(0x33D7BD, "6A58") && Matches(0x33D7D9, "E882CDFFFF") && Matches(0x33D7EF, "E8FC6B0100")
            && Matches(0x33D820, "6A38") && Matches(0x33D83C, "E89FCFFFFF") && Matches(0x33D852, "E8795A0100")
            && Matches(0x33D883, "6A30") && Matches(0x33D89F, "E88CD0FFFF") && Matches(0x33D8B5, "E8D6490100"));
        check("Binary msg41 reads two DWORDs, bytes/WORD, optional list and word-length blob",
            Matches(0x354401, "E80AEFFFFF") && Matches(0x35440C, "E8FFEEFFFF")
            && Matches(0x354416, "E8652DFEFF") && Matches(0x354420, "E85B2DFEFF")
            && Matches(0x35442B, "E89026FEFF") && Matches(0x354435, "E8462DFEFF")
            && Matches(0x35443F, "E8FCFCFFFF") && Matches(0x354449, "E8229DF900"));
        check("Binary msg42 reads a record vector and trailing DWORD; msg43 reads a DWORD then WORD",
            Matches(0x3532DB, "8D5628E8DDFBFFFF")
            && Matches(0x3532E3, "8D46348BCF50E8B23DFEFF")
            && Matches(0x35229A, "8D462850E8FD4DFEFF")
            && Matches(0x3522A3, "8D462C8BCF50E81248FEFF"));
        check("Binary msg42 reaches PvP queue consumer and original logs establish its domain/property",
            Matches(0x49194F, "FF703483C02850E805BC3C00")
            && ReadAt(0x1B8D764 - 0x401400, 23).SequenceEqual("PvP Queue succeeded.%s\0"u8.ToArray())
            && ReadAt(0x1B8D77C - 0x401400, 7).SequenceEqual("queued\0"u8.ToArray())
            && Matches(0x491524, "80E101"));
        check("Binary message41/42 list counts repeat at255 and PvP matching/runtime ID are literal",
            Matches(0x353AD3, "80FAFF0F8408FDFFFF")
            && Matches(0x3531FF, "80FBFF0F840FFDFFFF")
            && Matches(0x85D5C0, "83F809")
            && Matches(0x85D5C5, "8B4B783BC1")
            && Matches(0x85D86B, "6821010000")
            && Matches(0x85D872, "FF90AC000000"));
        check("Binary messages44-47 factory/body readers match the saved field widths",
            Matches(0x33D918, "E873480100") && Matches(0x33D97B, "E8B0470100")
            && Matches(0x33D9DE, "E88D350100") && Matches(0x33DA41, "E85A340100")
            && Matches(0x3521A0, "E8FB4EFEFF") && Matches(0x3521AB, "E8D04D0000")
            && Matches(0x3521B5, "E8C64FFEFF") && Matches(0x3521E1, "E8DA48FEFF")
            && Matches(0x35213F, "E83C4E0000") && Matches(0x352172, "E899FAFFFF")
            && Matches(0x35217D, "E81E4FFEFF")
            && Matches(0x350F7F, "E8FC5F0000") && Matches(0x350F8A, "E8F15F0000")
            && Matches(0x350F94, "E877D0F900")
            && Matches(0x350EAF, "E8CC600000") && Matches(0x350EBA, "E8C1600000")
            && Matches(0x350EC4, "E847D1F900"));
        check("Binary invitation46/47 differ by literal0/1 and target the same current-mode component",
            Matches(0x491B00, "6A00") && Matches(0x491C10, "6A01")
            && Matches(0x491B13, "8D8B80000000E842BE4600")
            && Matches(0x491C23, "8D8B80000000E832BD4600"));
        check("Binary challenge join handler normalizes raw flags and consumer emits runtime269/270/277",
            Matches(0x491A52, "807A3200") && Matches(0x491A5D, "0F95C0")
            && Matches(0x491A60, "807A3100") && Matches(0x491A73, "807A3000")
            && Matches(0x491A86, "E825BC4600")
            && Matches(0x8FD7F8, "680D010000") && Matches(0x8FD8CD, "680E010000")
            && Matches(0x8FD8E4, "6815010000")
            && ReadAt(0x1B94CA0 - 0x401400, 6).SequenceEqual("Squad\0"u8.ToArray())
            && ReadAt(0x1B94E50 - 0x401400, 8).SequenceEqual("Created\0"u8.ToArray())
            && ReadAt(0x1B94E44 - 0x401400, 9).SequenceEqual("Accepted\0"u8.ToArray()));
        check("Binary found-match consumer scales the trailing WORD by1000 before timestamp addition",
            Matches(0x85DC30, "0FB7CF69C9E803000003C189466C"));
        check("Binary messages48-50 readers preserve QWORD and NUL-text fields in the saved order",
            Matches(0x33DAA7, "E8D4940100")
            && Matches(0x350D8F, "E8EC610000") && Matches(0x350D99, "E872D2F900")
            && Matches(0x350DA3, "E8D863FEFF")
            && Matches(0x350CAF, "E8CC620000") && Matches(0x350CB9, "E852D3F900")
            && Matches(0x350CC4, "E8B7620000") && Matches(0x350CCE, "E83DD3F900"));
        check("Binary kicked handler requires mode E4 and both exact challenge-ID words before forwarding",
            Matches(0x491CFC, "8B88E4000000")
            && Matches(0x491D0B, "3B4228") && Matches(0x491D13, "3B422C")
            && Matches(0x491D24, "E8778E4600"));
        check("Binary challenge response/kicked consumers select runtime272/274/275 and eight-reason table",
            Matches(0x8FE481, "80FB08") && Matches(0x8FE489, "8B1485C0FCC601")
            && Matches(0x8FE4C6, "6810010000") && Matches(0x8FAC9A, "6812010000")
            && Matches(0x8FAD16, "6813010000")
            && ReadAt(0x1B95018 - 0x401400, 9).SequenceEqual("accepted\0"u8.ToArray())
            && ReadAt(0x1B949DC - 0x401400, 7).SequenceEqual("kicker\0"u8.ToArray())
            && ReadAt(0x1B949D4 - 0x401400, 7).SequenceEqual("kickee\0"u8.ToArray())
            && ReadAt(0x1B95CB8 - 0x401400, 8).SequenceEqual("UNKNOWN\0"u8.ToArray()));
        uint[] responseReasonVas = [0x1B95CEC, 0x1B95D08, 0x1B95CF8, 0x1B95C88,
            0x1B95C7C, 0x1B95CA8, 0x1B95C90, 0x1B95CC0];
        for (int i = 0; i < responseReasonVas.Length; i++)
        {
            string reason = MatrixChallengeFollowupNativeLayout.ResponseReasons[i];
            check($"Binary challenge response status {i} maps to {reason}",
                ReadUInt32(MatrixChallengeFollowupNativeLayout.ResponseReasonTableFileOffset + i * 4) == responseReasonVas[i]
                && ReadAt(responseReasonVas[i] - 0x401400, reason.Length + 1)
                    .SequenceEqual(Encoding.ASCII.GetBytes(reason + "\0")));
        }
        check("Binary challenge51/53/55 share the factory QWORD body;52/54 read vector/WORD32 suffixes",
            Matches(0x33DBC5, "E9CFFEFFFF") && Matches(0x33DC6B, "E929FEFFFF")
            && Matches(0x33DD11, "E983FDFFFF")
            && Matches(0x350B6F, "E80C640000") && Matches(0x350B79, "E8A2FCFFFF")
            && Matches(0x34FB9E, "E8DD730000") && Matches(0x34FBA9, "E8F274FEFF"));
        check("Binary challenge52/53/55 select distinct null-body runtime273/276/278",
            Matches(0x491DE8, "6A006A006811010000")
            && Matches(0x491E3E, "6A006A006814010000")
            && Matches(0x491EEE, "6A006A006816010000"));
        check("Binary challenge selected word stores at14, clears index24 to all-FF and scans16-byte records",
            Matches(0x8FAD9E, "894614C74624FFFFFFFF")
            && Matches(0x8FADC0, "39397413")
            && Matches(0x8FADCB, "83C110")
            && Matches(0x8FADD7, "895624")
            && Matches(0x8FADEC, "6815010000"));
        check("Binary server56 reads byte28, chunked QWORD2C and DWORD38;59 reads QWORD list28 and DWORD34",
            Matches(0x34FAAB, "8D5628E8CD76FEFF") && Matches(0x34FAB8, "E813FDFFFF")
            && Matches(0x34FABD, "8D4638") && Matches(0x34FAC3, "E8D875FEFF")
            && Matches(0x34F0DD, "E87EFDFFFF") && Matches(0x34F0E2, "8D4634")
            && Matches(0x34F0E8, "E8B37FFEFF"));
        check("Binary server57 captures entire remaining stream size and advances/subtracts that size",
            Matches(0x34F38D, "8B7E0885FF") && Matches(0x34F394, "8B0157FF7604FF5010017E04")
            && Matches(0x34F3C2, "297E08"));
        check("Binary server58 reuses five-QWORD/normalized-boolean reader;60 reads two lists;61 recursively reads nodes",
            Matches(0x33DE26, "E8458D0100")
            && Matches(0x33DEEE, "E80D080100") && Matches(0x33DEF3, "8D5634")
            && Matches(0x33DEF8, "E803080100") && Matches(0x34E883, "80FBFF")
            && Matches(0x33DF5D, "E86EF70000")
            && Matches(0x34D7FA, "E881E6FFFF") && Matches(0x34C22D, "E84EFCFFFF"));
        check("Binary clock update stores parameters8E0, emits110 and separately sets/rebases running limit",
            Matches(0x4936EB, "8A86080900003A4728") && Matches(0x49372A, "6A6E")
            && Matches(0x493760, "F30F7F86E0080000")
            && Matches(0x4937A0, "C6864809000001")
            && Matches(0x4937D8, "2BC8898620090000")
            && Matches(0x4937E6, "1BC2C6864809000000")
            && Matches(0x4937F5, "F20F118628090000")
            && Matches(0x49382F, "3B8600090000")
            && Matches(0x49383D, "3B8604090000"));
        check("Binary recursive server61 consumer remains a guarded tail call rather than a decoded event name",
            Matches(0x4967FD, "834508285DE989EA9C00"));
        uint[] unqueueReasonVas =
        [
            0x1B95B6C, 0x1B95B5C, 0x1B95B80, 0x1B95B74, 0x1B95B9C,
            0x1B95B8C, 0x1B95BC0, 0x1B95BB0, 0x1B95AE0, 0x1B95AC4,
            0x1B95B04, 0x1B95AF4, 0x1B95B28, 0x1B95B14, 0x1B95B48,
            0x1B95B34, 0x1B95C30, 0x1B95C20, 0x1B95C50, 0x1B95C40
        ];
        for (int i = 0; i < unqueueReasonVas.Length; i++)
        {
            uint va = unqueueReasonVas[i];
            var expectedText = Encoding.ASCII.GetBytes(MatrixForceUnqueueNativeLayout.GetReason((byte)i) + "\0");
            check($"Binary ForceUnqueue reason{i} preserves its original table pointer/text",
                ReadUInt32(0x186E910 + i * 4) == va
                && ReadAt(checked((int)(va - 0x401400)), expectedText.Length).SequenceEqual(expectedText));
        }
        check("Binary server63/64 read two/seven raw WORD fields;64 uses descriptor28 through34",
            Matches(0x34B57E, "E83DB5FEFF") && Matches(0x34B589, "E832B5FEFF")
            && Matches(0x34B480, "E83BB6FEFF") && Matches(0x34B4BC, "8D4734")
            && Matches(0x34B4C2, "E8F9B5FEFF"));
        check("Binary LFG65 body reads QWORD, DWORD, QWORD, text and text vector",
            Matches(0x34B42F, "E84CBB0000") && Matches(0x34B43A, "E861BCFEFF")
            && Matches(0x34B445, "E836BB0000") && Matches(0x34B44F, "E8BC2BFA00")
            && Matches(0x34B459, "E892FDFFFF"));
        check("Binary LFG66 body reads two QWORDs/text; leader change matches both ID words before dispatch",
            Matches(0x34AF7F, "E8FCBF0000") && Matches(0x34AF8A, "E8F1BF0000")
            && Matches(0x34AF94, "E87730FA00")
            && Matches(0x492521, "3B4728") && Matches(0x492529, "3B472C")
            && Matches(0x492535, "E836272600"));
        check("Binary original logs identify LFGMatchFound and LFGLeaderChange, not quick chat",
            ReadAt(0x1B51D48 - 0x401400, 13).SequenceEqual(Encoding.ASCII.GetBytes("LFGMatchFound"))
            && ReadAt(0x1B51E60 - 0x401400, 15).SequenceEqual(Encoding.ASCII.GetBytes("LFGLeaderChange")));
        check("Binary server67 reads QWORD/two NUL texts/DWORD;68 reads two maps then copies into primary8BC optional",
            Matches(0x34AE9F, "E8DCC00000") && Matches(0x34AEA9, "E86231FA00")
            && Matches(0x34AEB3, "E85831FA00") && Matches(0x34AEBE, "E8DDC1FEFF")
            && Matches(0x33E214, "E8276F0100") && Matches(0x33E21E, "E82D7C0100")
            && Matches(0x4925F3, "8345082881C1B80800005DE9DD3DECFF"));
        check("Binary server34 reads WORD-length blob28/raw byte38 and normalizes flag only in handler",
            Matches(0x33D53D, "E83E9C0100") && Matches(0x35718D, "E8DE6FF900")
            && Matches(0x357192, "8D5638") && Matches(0x357197, "E8E4FFFDFF")
            && Matches(0x490AA6, "807938000F95C00FB6C0")
            && Matches(0x490ABB, "E860E3E7FF"));
        check("Binary server34 always invokes buffer virtual18; nonzero snapshot sets C3C pair and C38 pending",
            Matches(0x30EE49, "FF5718") && Matches(0x30EE4C, "807D0C00746B")
            && Matches(0x30EE61, "89863C0C0000") && Matches(0x30EEA7, "E848EA2801")
            && Matches(0x30EEB3, "FF5014C686380C000001"));
        check("Binary server36 reads two NUL texts and filters sysmsg before broadcast display",
            Matches(0x33D605, "E8060AFB00") && Matches(0x33D60F, "E8FC09FB00")
            && Matches(0x491442, "B9FC1DB501")
            && Matches(0x491489, "68041EB501") && Matches(0x491491, "E8CA1F2500")
            && ReadAt(0x1B51DFC - 0x401400, 7).SequenceEqual(Encoding.ASCII.GetBytes("sysmsg\0"))
            && ReadAt(0x1B51E04 - 0x401400, 10).SequenceEqual(Encoding.ASCII.GetBytes("broadcast\0")));
        check("Binary zone chat67 generates fixed category1 label through80CC90 and lowercases before6E3460",
            Matches(0x6E2621, "E86AA61200") && Matches(0x6E2654, "E8C773BD00")
            && Matches(0x6E2678, "E8E30D0000")
            && ReadAt(0x1B8B8EC - 0x401400, 5).SequenceEqual(Encoding.ASCII.GetBytes("ZONE\0")));
        check("Binary shared chat display emits runtime335 before post-notification consumer6F46F0",
            Matches(0x6E35FE, "684F010000") && Matches(0x6E362E, "E8BD100100"));
        (int offset, string text)[] chatProperties =
        [
            (0x1B6E2E8, "channel"), (0x1B6E2E0, "author"), (0x1B6E2D8, "text"),
            (0x1B6E2CC, "author_id"), (0x1B6E3B8, "dev"), (0x1B6E3B0, "mentor"),
            (0x1B6E3A8, "ranger"), (0x1B6E39C, "publisher"), (0x1B6E388, "use_encounter_icon")
        ];
        foreach (var (va, text) in chatProperties)
        {
            byte[] literal = Encoding.ASCII.GetBytes(text + "\0");
            check($"Binary chat335 property {text} preserves its original literal",
                ReadAt(va - 0x401400, literal.Length).SequenceEqual(literal));
        }
        check("Binary raw byte hash uses FNV1a seed/prime followed by wrapping13/7/3/17/5 final mix",
            Matches(0x12F178E, "BEC59D1C81") && Matches(0x12F17A0, "0FB601")
            && Matches(0x12F17A9, "69F093010001")
            && Matches(0x12F17B3, "69C601200000")
            && Matches(0x12F17BE, "C1E90733C88D04C9")
            && Matches(0x12F17C8, "C1E91133C88BC1C1E00503C1"));
        check("Binary chat log masks author type byte and hashes prefix-NUL/id/static25/mode-text clamp32",
            Matches(0x6F4733, "83781800") && Matches(0x6F48C0, "81E100FFFFFF")
            && Matches(0x6F493F, "F30F6F05347CC601")
            && Matches(0x6F494D, "F30F7E05447CC601")
            && Matches(0x6F495B, "A04C7CC601")
            && Matches(0x6F4964, "83C3218B4110")
            && Matches(0x6F4982, "BE200000003BC60F42F0")
            && ReadAt(0x1866834, 25).AsSpan().SequenceEqual(MatrixChatLogNativeLayout.FooterBytes));
        check("Binary chat log folds FNV32 to16bits, formats %04X newline and calls native fputs",
            Matches(0x6F49D2, "E8A9CDBF00")
            && Matches(0x6F49D7, "8BC80FB7C0C1E91033C8")
            && Matches(0x6F49E5, "6858FCB601")
            && Matches(0x6F4A0A, "FF150049AC01")
            && ReadAt(0x1B6FC58 - 0x401400, 6).SequenceEqual(Encoding.ASCII.GetBytes("%04X\n\0")));
        (int va, string text)[] chatLogFormats =
        [
            (0x1B6FBD8, "%c %Z"), (0x1B6FBD4, "%X"), (0x1B6FBD0, " "),
            (0x1B6FC60, "%s %s %s: %s%s"), (0x1B6FC70, "%s %s: %s%s"),
            (0x1B6FC7C, "%s %s%s")
        ];
        foreach (var (va, text) in chatLogFormats)
        {
            byte[] literal = Encoding.ASCII.GetBytes(text + "\0");
            check($"Binary chat log format at VA {va:X}",
                ReadAt(va - 0x401400, literal.Length).SequenceEqual(literal));
        }
        (int iatRaw, uint thunk, int nameRaw, string name)[] chatImports =
        [
            (0x16C34A8, 0x19C0CD8, 0x19BF8DA, "_stricmp"),
            (0x16C34D8, 0x19C0D90, 0x19BF992, "_time64"),
            (0x16C3500, 0x19C0DF0, 0x19BF9F2, "fputs"),
            (0x16C3530, 0x19C0EB4, 0x19BFAB6, "_localtime64"),
            (0x16C3568, 0x19C0F4C, 0x19BFB4E, "strftime"),
            (0x16C356C, 0x19C0F58, 0x19BFB5A, "sprintf"),
            (0x16C346C, 0x19C0C36, 0x19BF838, "strncpy"),
            (0x16C3304, 0x19C16DE, 0x19C02E0, "_wfopen"),
            (0x16C35A8, 0x19C102C, 0x19BFC2E, "fclose"),
            (0x16C34B4, 0x19C0CFA, 0x19BF8FC, "fflush"),
            (0x16C2F94, 0x19BF814, 0x19BE416, "MultiByteToWideChar"),
            (0x16C371C, 0x19C0BDC, 0x19BF7DE, "ShellExecuteA")
        ];
        foreach (var (iat, thunk, rawName, name) in chatImports)
        {
            byte[] literal = Encoding.ASCII.GetBytes(name + "\0");
            check($"Binary chat import {name} retains the original PE thunk/name",
                ReadUInt32(iat) == thunk && ReadAt(rawName, literal.Length).SequenceEqual(literal));
        }
        check("Binary tfChat RTTI identifies the shared current-mode chat component",
            ReadUInt32(0x18668D8) == 0x1CA7544
            && ReadUInt32(0x18A6144 + 12) == MatrixChatFileNativeLayout.RttiTypeDescriptorVirtualAddress
            && ReadAt(0x1A11300, 13).SequenceEqual(Encoding.ASCII.GetBytes(".?AVtfChat@@\0")));
        check("Binary successful World setup copies owner +848/+84C as type1 chat ID and +850 as chat text",
            Matches(0x64B59C, "8B7710")
            && Matches(0x64B5A4, "8B8650080000")
            && Matches(0x64B5CF, "8B8E480800008B97E828000081E101FFFFFF8B864C08000083C901")
            && Matches(0x64B5EB, "894A088D4A1089420CE89716CBFF"));
        check("Binary Login chat initialization gates on sampled owner34 then copies generated pair/caller text",
            Matches(0x63B2A4, "8BB6280A0000")
            && Matches(0x63B2BF, "8A5E34")
            && Matches(0x63B2CC, "84DB7517")
            && Matches(0x63B320, "E8FBE658008B4F142501FFFFFF")
            && Matches(0x63B333, "89410889510C83C110E84F19CCFF"));
        check("Binary generated Login pair is 64-bit EDX:EAX from lock-xadd old counter shifted8 and ORed base",
            Matches(0xBC9A23, "BE01000000B888C5DF01F00FC130")
            && Matches(0xBC9A31, "8B0DF887E40133FF8B15FC87E401")
            && Matches(0xBC9A41, "0FA4F7088BDAC1E6080BFA0BF1")
            && Matches(0xBC9A74, "81E600FFFFFF8BD75F8BC65E5BC3"));
        check("Binary manager+C44 embeds RTTI slBlob with append/clear/length/data virtual slots",
            Matches(0x30342E, "C787440C0000C40CC801")
            && ReadAt(0x1A34398, 13).SequenceEqual(Encoding.ASCII.GetBytes(".?AVslBlob@@\0"))
            && ReadUInt32(0x187F8C4 + 0x18) == 0x16CC610
            && ReadUInt32(0x187F8C4 + 0x14) == 0x16CC600
            && ReadUInt32(0x187F8C4 + 0x0C) == 0x16CC5C0
            && ReadUInt32(0x187F8C4 + 4) == 0x16CC560);
        check("Binary slBlob append preserves current bytes, grows logical length and memcpy appends incoming bytes",
            Matches(0x12CBA1A, "FF500C8B5D0C8BF86A018BCE8D141F52E881020000")
            && Matches(0x12CBA39, "53FF750803C750E8AF1E2D00")
            && Matches(0x12CBA00, "6A006A00E8A7020000C3"));
        check("Binary snapshot v14 clears with no length argument and returned snapshot helper deep-copies pair/bytes",
            Matches(0x30EEAC, "8B0383C40C8BCBFF5014")
            && Matches(0x30EDA3, "85FF750433C05FC3")
            && Matches(0x30EDBF, "8B4F04894E04FF7704FF3750E824EB2801"));
        byte[] requestSigscanName = Encoding.ASCII.GetBytes(".?AVRequestSigscan@matrix_fury@@\0");
        byte[] sigscanDataName = Encoding.ASCII.GetBytes(".?AVSigscanData@matrix_fury@@\0");
        check("Binary RTTI names outgoing32 RequestSigscan and incoming34 SigscanData",
            ReadAt(0x1A032C0, requestSigscanName.Length).SequenceEqual(requestSigscanName)
            && ReadAt(0x1A031E8, sigscanDataName.Length).SequenceEqual(sigscanDataName)
            && ReadUInt32(0x1860F84 + 4) == 0x746920);
        check("Binary RequestSigscan32 reserves6 then emits compact32/descriptor DWORD28/byte2C only",
            Matches(0x31E7E5, "C745EC20000000")
            && Matches(0x31E80C, "83F906")
            && Matches(0x31E824, "E8D7200000")
            && Matches(0x31E82E, "8D472850E8D98A0000")
            && Matches(0x31E837, "8D472C8BCE50E88E8B0000"));
        check("Binary RequestSigscan32 body reader mirrors DWORD28/byte2C and sender uses channel1",
            Matches(0x345D29, "8D472850E86E13FFFF")
            && Matches(0x345D35, "8D572CE84314FFFF")
            && Matches(0x31D409, "6A01E810678B00"));
        check("Binary tfChat constructor selects wt/at by prior attempt, stores FILE and sets attempt flag",
            Matches(0x6E29B2, "BAF0E1B601")
            && Matches(0x6E29BE, "803DA4B0ED0100B854E3B6010F44D0")
            && Matches(0x6E29CD, "E85E76BD00894318")
            && Matches(0x6E2A17, "837B1800")
            && Matches(0x6E2A38, "6A016810E3B601")
            && Matches(0x6E2A4D, "C605A4B0ED0101"));
        check("Binary tfChat destructor logs short-time shutdown before fclose and zeroing FILE",
            Matches(0x6E2CBC, "6A0068F0E2B601")
            && Matches(0x6E2CCA, "E8211A0100")
            && Matches(0x6E2CD7, "FF15A849AC01")
            && Matches(0x6E2CE0, "C7431800000000"));
        check("Binary UTF8 file-open wrapper uses CP65001 path/mode conversions and _wfopen",
            Matches(0x12BA057, "68E9FD0000FF159443AC01")
            && Matches(0x12BA07C, "68E9FD0000FF159443AC01")
            && Matches(0x12BA09B, "FF150447AC01"));
        check("Binary chat flush leaf and OpenChatLog have distinct null-FILE and null-chat gates",
            Matches(0x6F46E0, "8B411885C0740850FF15B448AC0159C3")
            && Matches(0x6ACA64, "E8D77A040085C07516")
            && Matches(0x6ACA83, "8B401885C0740A")
            && Matches(0x6ACA8B, "FF15B448AC01")
            && Matches(0x6ACA9D, "6A016A006A00")
            && Matches(0x6ACAB0, "68C8C7B6016A00FF151C4BAC01"));
        (int va, string text)[] chatFileTexts =
        [
            (0x1B6FBE0, MatrixChatFileNativeLayout.RelativeLogPath),
            (0x1B6E1F0, MatrixChatFileNativeLayout.LaterAttemptOpenMode),
            (0x1B6E354, MatrixChatFileNativeLayout.FirstAttemptOpenMode),
            (0x1B6E328, MatrixChatFileNativeLayout.OpenFailureText),
            (0x1B6E310, MatrixChatFileNativeLayout.SessionStartedText),
            (0x1B6E2F0, MatrixChatFileNativeLayout.SessionShutdownText),
            (0x1B6C7C8, MatrixChatFileNativeLayout.ShellOperation),
            (0x1B6C7D0, MatrixChatFileNativeLayout.ChatNotInitializedText)
        ];
        foreach (var (va, text) in chatFileTexts)
        {
            byte[] literal = Encoding.ASCII.GetBytes(text + "\0");
            check($"Binary chat file text at VA {va:X}",
                ReadAt(va - 0x401400, literal.Length).SequenceEqual(literal));
        }
        check("Binary crash handler strings, SIGABRT/terminate/purecall installs and full-memory dump flag match",
            Matches(CrashHandlerNativeLayout.DbgHelpAnsiNameVirtualAddress - 0x401400, Convert.ToHexString(Encoding.ASCII.GetBytes(CrashHandlerNativeLayout.DbgHelpDllName + "\0")))
            && Matches(CrashHandlerNativeLayout.DumpFormatAnsiVirtualAddress - 0x401400, Convert.ToHexString(Encoding.ASCII.GetBytes(CrashHandlerNativeLayout.DumpNameFormat + "\0")))
            && Matches(CrashHandlerNativeLayout.MiniDumpExportAnsiVirtualAddress - 0x401400, Convert.ToHexString(Encoding.ASCII.GetBytes(CrashHandlerNativeLayout.MiniDumpExportName + "\0")))
            && Matches(CrashHandlerNativeLayout.PureCallStubFileOffset, "BA0C60C101B90060C101E9")
            && Matches(CrashHandlerNativeLayout.TerminateStubFileOffset, "BA545FC101B9485FC101E9"));
        check("Binary crash hook setter 12C38D0, installer wrapper 12C38B0, RedHanded call site 2EAEBF and fatal hook read match",
            Matches(CrashHandlerNativeLayout.HookSetterFileOffset, "890D9CB0E401C3")
            && Matches(CrashHandlerNativeLayout.HookInstallWrapperFileOffset, "8B1598B0E4018A0D94B0E401E90FFFFFFF")
            && Matches(CrashHandlerNativeLayout.RedHandedLauncherCallSiteFileOffset, "E8CCFAFFFF")
            && CrashHandlerNativeLayout.RedHandedLauncherCallSiteFileOffset + 5 + BitConverter.ToInt32(ReadAt(CrashHandlerNativeLayout.RedHandedLauncherCallSiteFileOffset + 1, 4)) == CrashHandlerNativeLayout.RedHandedLauncherFileOffset);
        check("Binary engine-system slot cell 1DFC82C, accessor and slot +0x58/+0x60/+0x44/+4 call sequences match",
            Matches(0x1DFC82C - 0x401C00, "7004E001")
            && Matches(SceneFactoryNativeLayout.EngineSystemSlotAddressAccessorFileOffset, "B87004E001C3")
            && Matches(0x46CA62, "A12CC8DF018B088B01FF5058")
            && Matches(0xE651CC, "A12CC8DF01578B088B01FF5060")
            && Matches(0xF5FF5E, "A12CC8DF018B088D45F0508B01FF5044")
            && Matches(0x30C15F, "A1A40EE6018B088B01FF5004"));
        check("Binary engine-system vtable slots 17/21/22/23/24/25 (+0x44/+0x54/+0x58/+0x5C/+0x60/+0x64) point at the verified accessor bodies and the +0x64 setter has init/shutdown call sites",
            ReadUInt32(SceneFactoryNativeLayout.EngineSystemVtableFileOffset + 17 * 4) == SceneFactoryNativeLayout.EngineSystemSlotBlockAddressFileOffset + 0x400C00
            && ReadUInt32(SceneFactoryNativeLayout.EngineSystemVtableFileOffset + 21 * 4) == SceneFactoryNativeLayout.EngineSystemSlotDisplayModeApplyFileOffset + 0x400C00
            && ReadUInt32(SceneFactoryNativeLayout.EngineSystemVtableFileOffset + 22 * 4) == SceneFactoryNativeLayout.EngineSystemSlotDisplayModeGetFileOffset + 0x400C00
            && ReadUInt32(SceneFactoryNativeLayout.EngineSystemVtableFileOffset + 23 * 4) == SceneFactoryNativeLayout.EngineSystemSlotSizeTripleGetFileOffset + 0x400C00
            && ReadUInt32(SceneFactoryNativeLayout.EngineSystemVtableFileOffset + 24 * 4) == SceneFactoryNativeLayout.EngineSystemSlotHandleGetFileOffset + 0x400C00
            && ReadUInt32(SceneFactoryNativeLayout.EngineSystemVtableFileOffset + 25 * 4) == SceneFactoryNativeLayout.EngineSystemSlotHandleSetFileOffset + 0x400C00
            && Matches(SceneFactoryNativeLayout.EngineSystemSlotBlockAddressFileOffset, "8D81B8000000C3")
            && Matches(SceneFactoryNativeLayout.EngineSystemSlotDisplayModeGetFileOffset, "8B8130010000C3")
            && Matches(SceneFactoryNativeLayout.EngineSystemSlotHandleGetFileOffset, "8B81B4000000C3")
            && Matches(SceneFactoryNativeLayout.EngineSystemSlotHandleSetFileOffset, "558BEC8B45088981B40000005DC20400")
            && Matches(SceneFactoryNativeLayout.EngineSystemSlotSizeTripleGetFileOffset, "558BEC8B550885D274088B814C01000089028B550C85D274088B815001000089028B551085D274088B815401000089025DC20C00")
            && Matches(SceneFactoryNativeLayout.EngineSystemSlotDisplayModeApplyFileOffset + 0x14, "803DDE95E40100")
            && Matches(SceneFactoryNativeLayout.EngineSystemHandleSetInitCallSiteFileOffset, "A12CC8DF01FF352801E0018B088B01FF5064")
            && Matches(SceneFactoryNativeLayout.EngineSystemHandleSetShutdownCallSiteFileOffset, "A12CC8DF01C786800B0000000000006A008B088B01FF5064"));        check("Binary client main loop: bootstrap calls Initialize/RunLoop/Shutdown and RunLoop quit flag, timer, engine-step slot +0x20, mode apply, service slot +8 and App+0xB98 slot +0x24 sequences match",
            Matches(ClientMainLoopNativeLayout.BootstrapInitializeCallSiteFileOffset, "E83CB2010084C0740D8B0D98FDDF01E81DE70100")
            && Matches(ClientMainLoopNativeLayout.BootstrapShutdownCallSiteFileOffset, "E8D2E30100E87D030100")
            && Matches(ClientMainLoopNativeLayout.InitializeFileOffset, "558BEC6AFF68E04E9D01")
            && Matches(ClientMainLoopNativeLayout.ShutdownFileOffset, "558BEC6AFF6878519D01")
            && Matches(ClientMainLoopNativeLayout.RunLoopFileOffset, "558BEC83E4F86AFF68C8519D01")
            && Matches(ClientMainLoopNativeLayout.RunLoopQuitCheckSiteFileOffset, "803DB0B0E401000F858E060000")
            && Matches(ClientMainLoopNativeLayout.RunLoopQuitCheckTailSiteFileOffset, "803DB0B0E40100")
            && Matches(ClientMainLoopNativeLayout.RunLoopFrameTimerCallSiteFileOffset, "8D8E880A0000E8F7A401008B0D70FFDF01")
            && Matches(ClientMainLoopNativeLayout.RunLoopEngineStepSlotCallSiteFileOffset, "A1A40EE6018B08F30F2CC08B11508B4220")
            && Matches(ClientMainLoopNativeLayout.RunLoopApplyPendingModesCallSiteFileOffset, "8BCEE83D3E0000")
            && Matches(0x309B08, "E8C3EE0100")
            && Matches(ClientMainLoopNativeLayout.RunLoopServiceSlot2CallSiteFileOffset, "8B0D7467E4018B01FF50")
            && Matches(0x309B18, "8BCEE8B1460000")
            && Matches(0x309B1F, "83BE640C000000")
            && Matches(ClientMainLoopNativeLayout.RunLoopCurrentModeSlot24CallSiteFileOffset, "85C90F84430200008B018B4024FFD0")
            && Matches(ClientMainLoopNativeLayout.ShutdownCurrentModeDeleteSiteFileOffset, "8B016A01FF108BCFC786980B00000000"));
        check("Binary client main loop round 2: Initialize/engine step/helpers", Matches(ClientMainLoopNativeLayout.InitializeFileOffset, "558BEC6AFF68E04E9D0164A1")
            && Matches(ClientMainLoopNativeLayout.InitializeTailFileOffset, "83EC188BC4C7001407C601C7")
            && Matches(ClientMainLoopNativeLayout.PlatformStatePumpFileOffset, "538BDC83EC0883E4F083C404")
            && Matches(ClientMainLoopNativeLayout.StatusTextBuilderFileOffset, "558BEC83EC2CA14C35E70153")
            && Matches(ClientMainLoopNativeLayout.LoadStateStepFileOffset, "558BEC5153568BF157837E14")
            && Matches(ClientMainLoopNativeLayout.EngineStepTargetFileOffset, "558BEC6AFF6870F9A50164A100000000")
            && Matches(ClientMainLoopNativeLayout.EngineStepPeekMessageLoopFileOffset, "6A006A006A006A008D45C850FFD685C0")
            && ReadUInt32(ClientMainLoopNativeLayout.EngineSystemVtableSlot8FileOffset) == 0x1256590);
        check("Binary client main loop round 3: Initialize helpers and engine-step frame limiter (QPC + Sleep)", Matches(ClientMainLoopNativeLayout.InitCommandLineLikeStateFn304170FileOffset, "558BEC83E4F86AFF68784B9D")
            && Matches(ClientMainLoopNativeLayout.InitPathNormalizeFn3047E0FileOffset, "558BEC6AFF68D04B9D0164A1")
            && Matches(ClientMainLoopNativeLayout.InitRegistryInstallFn304BB0FileOffset, "558BEC6AFF68184C9D0164A1")
            && Matches(ClientMainLoopNativeLayout.InitStringMapFindOrInsertFn325350FileOffset, "558BEC6AFF6820689D0164A1")
            && Matches(ClientMainLoopNativeLayout.EngineStepFrameLimiterFileOffset, "558BEC83E4F8A100FEED0183")
            && Matches(ClientMainLoopNativeLayout.EngineStepFpsCounterFileOffset, "558BEC83E4F883EC2CA1F0FD")
            && Matches(ClientMainLoopNativeLayout.EngineStepLimiterCallSiteFileOffset, "E83D000000")
            && Matches(ClientMainLoopNativeLayout.EngineStepFpsCallSiteFileOffset, "E8FEFAFFFF")
            && Matches(ClientMainLoopNativeLayout.EngineStepSleepCallSiteFileOffset, "8BF03B5C241872798B44241077043BF0766F8BCE2BC8B8D34D6210F7E1C1EA0652FF1574")
            && Matches(ClientMainLoopNativeLayout.InitializeTailEngineSlot54CallSiteFileOffset, "FF5054"));
        check("Binary startup config pipeline round 4 (argv, ini, asset strings)",
            Matches(ClientMainLoopNativeLayout.ArgvSwitchCheckFn2EA6F0FileOffset, "558BEC51A17448AC015356BE")
            && Matches(ClientMainLoopNativeLayout.CommandLineDiagnosticsFn2EA990FileOffset, "558BECB8B4440000E8632F2B")
            && Matches(ClientMainLoopNativeLayout.IniContainerLoaderFn12DA710FileOffset, "558BEC6AFF68F45EA90164A1")
            && Matches(ClientMainLoopNativeLayout.ArgvFirstArgDirReaderFnE51590FileOffset, "558BEC81EC04040000A10050")
            && Matches(ClientMainLoopNativeLayout.AssetDbSearchPathsLoggerFn1318400FileOffset, "558BEC6AFF685885A90164A1")
            && Matches(ClientMainLoopNativeLayout.FirefallIniStringFileOffset, "666972656661 6C6C2E696E69".Replace(" ",""))
            && Matches(ClientMainLoopNativeLayout.LocalAssetsFirefallIniStringFileOffset, "6C6F63616C6173736574732F6669726566616C6C2E696E69")
            && Matches(ClientMainLoopNativeLayout.HotfixIniStringFileOffset, "686F74666978 2E696E69".Replace(" ",""))
            && Matches(ClientMainLoopNativeLayout.LowMipTexturesPakStringFileOffset, "73797374656D2F76742F6C6F776D697074657874757265732E70616B")
            && Matches(ClientMainLoopNativeLayout.BigDumpSwitchStringFileOffset, "2D2D62696764756D70"));
        check("Binary startup round 5 (cfg loader, LMTD pak, hotfix, plain strings)",
            Matches(ClientMainLoopNativeLayout.CfgFileExpanderFn1361890FileOffset, "558BEC83E4F86AFF68") &&
            Matches(ClientMainLoopNativeLayout.LowMipTexturePakLoaderFnEB38B0FileOffset, "558BEC6AFF68") &&
            Matches(ClientMainLoopNativeLayout.DirectoryListFn12B6C60FileOffset, "558BEC6AFF68") &&
            Matches(ClientMainLoopNativeLayout.EngineInitFn10D8A40FileOffset, "558BEC6AFF68") &&
            Matches(ClientMainLoopNativeLayout.HotfixDownloadFn386D90FileOffset, "558BEC83E4F06AFF68") &&
            Matches(ClientMainLoopNativeLayout.TextRegistryLookupFn12BE6E0FileOffset, "8B15FC04E001") &&
            Matches(ClientMainLoopNativeLayout.LmtdMagicStringFileOffset, "4C4D5444") &&
            Matches(ClientMainLoopNativeLayout.CfgPatternStringFileOffset, "2A2E636667") &&
            Matches(ClientMainLoopNativeLayout.AdditionalIniStringFileOffset, "6164646974696F6E616C696E69") &&
            Matches(ClientMainLoopNativeLayout.ConsoleStringFileOffset, "636F6E736F6C65") &&
            Matches(ClientMainLoopNativeLayout.SwitchNameDirFileOffset, "64697200") &&
            Matches(ClientMainLoopNativeLayout.SwitchNamePlatformFileOffset, "706C6174666F726D") &&
            Matches(ClientMainLoopNativeLayout.SwitchNameDumpConfigFileOffset, "64756D702D636F6E666967"));
        check("Binary startup round 6 (uri registration, hotfix, AssetDB paths)",
            Matches(ClientMainLoopNativeLayout.UriProtocolRegisterFn304BB0FileOffset, "558BEC6AFF68184C9D0164A1") &&
            Matches(ClientMainLoopNativeLayout.HotfixDownloadFn386D90FileOffset, "558BEC83E4F06AFF6889B39D") &&
            Matches(ClientMainLoopNativeLayout.HotfixHttpTransportFn388330FileOffset, "558BEC6AFF6812B59D0164A1") &&
            Matches(ClientMainLoopNativeLayout.AssetDbSearchPathsLoggerFn1318400FileOffset, "558BEC6AFF685885A90164A1") &&
            Matches(ClientMainLoopNativeLayout.OpenSwitchStringFileOffset, "6F70656E") &&
            Matches(ClientMainLoopNativeLayout.HotfixDownloadPathKeyStringFileOffset, "446F776E6C6F61642D50617468"));
        check("Binary startup round 7 (asset search roots, hotfix fetchers)",
            Matches(ClientMainLoopNativeLayout.FileSearchRootSetterFn1317810FileOffset, "558BEC51568B7508578BF985") &&
            Matches(ClientMainLoopNativeLayout.FileSearchRootInitFn13178B0FileOffset, "558BEC6AFF688883A90164A1") &&
            Matches(ClientMainLoopNativeLayout.SearchPathResolveDirsFn1319520FileOffset, "558BEC6AFF68C987A90164A1") &&
            Matches(ClientMainLoopNativeLayout.Utf8FileOpenFn12BA030FileOffset, "558BEC81EC040A0000A10050") &&
            Matches(ClientMainLoopNativeLayout.HotfixHttpFetchFn388BD0FileOffset, "558BEC6AFF68A3B59D0164A1") &&
            Matches(ClientMainLoopNativeLayout.HotfixLocalCopyFn388800FileOffset, "558BEC6AFF6851B59D0164A1") &&
            Matches(ClientMainLoopNativeLayout.HotfixBadMd5StringFileOffset, "426164206D64357375"));
        check("Binary startup round 8 (AssetDB path lists, Twitch worker, config strings)",
            Matches(ClientMainLoopNativeLayout.TwitchWorkerFn3289D0FileOffset, "538BDC83EC0883E4F083C404") &&
            Matches(ClientMainLoopNativeLayout.SearchPathAddOrdinaryFn1317D00FileOffset, "558BEC6AFF684984A90164A1") &&
            Matches(ClientMainLoopNativeLayout.SearchPathAddAssetDbFn1318080FileOffset, "558BEC6AFF68D984A90164A1") &&
            Matches(ClientMainLoopNativeLayout.SearchPathRemoveOrdinaryFn1317F80FileOffset, "558BEC83EC088BC18945FC53") &&
            Matches(ClientMainLoopNativeLayout.SearchPathRemoveAssetDbFn1318300FileOffset, "558BEC83EC088BC18945FC53") &&
            Matches(ClientMainLoopNativeLayout.SearchPathNormalizeFn1317AD0FileOffset, "558BEC6AFF680984A90164A1") &&
            Matches(ClientMainLoopNativeLayout.AssetPathConfigLoaderFn38B310FileOffset, "558BEC6AFF68EFB89D0164A1") &&
            Matches(ClientMainLoopNativeLayout.HotfixIncludeFormatStringFileOffset, "2E696E636C75646520") &&
            Matches(ClientMainLoopNativeLayout.HotfixDownloadPathKeyStringFileOffset, "446F776E6C6F61642D50") &&
            Matches(ClientMainLoopNativeLayout.AssetPathConfigDefaultStringFileOffset, "73797374656D2F617373") &&
            Matches(ClientMainLoopNativeLayout.AssetPathConfigInstallDirStringFileOffset, "496E7374616C6C446972"));
        check("Binary startup round 9 (ini directives, hotfix reload)",
            Matches(ClientMainLoopNativeLayout.IniDirectiveDispatchFn12DD300FileOffset, "558BEC6AFF681862A90164A1") &&
            Matches(ClientMainLoopNativeLayout.HotfixApplyReloadFn3865C0FileOffset, "558BEC6AFF68B8B29D0164A1") &&
            Matches(ClientMainLoopNativeLayout.HotfixApplyCallerFn386430FileOffset, "558BEC5153568BF157837E14") &&
            Matches(ClientMainLoopNativeLayout.IniIncludeDirectiveStringFileOffset, "696E636C75646520") &&
            Matches(ClientMainLoopNativeLayout.IniIncludeAllDirectiveStringFileOffset, "696E636C7564652D616C6C20") &&
            Matches(ClientMainLoopNativeLayout.IniIncludeOptionalDirectiveStringFileOffset, "696E636C7564653F20") &&
            Matches(ClientMainLoopNativeLayout.HotfixLastLevelKeyStringFileOffset, "4C617374486F746669784C65") &&
            Matches(ClientMainLoopNativeLayout.HotfixAppliedLevelStringFileOffset, "486F746669783A206170706C"));
        check("Binary startup round 10 (engine start slot 0, locale gate)",
            Matches(ClientMainLoopNativeLayout.OEngineSystemStartFnE54BF0FileOffset, "558BEC6AFF6840F9A50164A1") &&
            Matches(ClientMainLoopNativeLayout.IniLocaleGateFn12B78F0FileOffset, "558BEC81EC38080000A10050") &&
            Matches(ClientMainLoopNativeLayout.EngineInitBannerStringFileOffset, "2D2D2D20496E697469616C69") &&
            Matches(ClientMainLoopNativeLayout.EngineInitMainWindowStringFileOffset, "496E697469616C697A696E67") &&
            Matches(ClientMainLoopNativeLayout.EngineAutodetectDirStringFileOffset, "656E67696E652F6175746F64"));    }
}
