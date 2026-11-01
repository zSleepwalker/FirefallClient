namespace Firefall.Reconstruction.Startup;

public readonly record struct CharacterKeyedSixDwordPayload(
    uint Dword0,
    uint Dword1,
    uint Dword2,
    uint Dword3,
    uint Dword4,
    uint Dword5);

public readonly record struct CharacterKeyedConditionalRecordSnapshot(
    byte PresenceByte,
    CharacterKeyedSixDwordPayload Payload);

public readonly record struct CharacterKeyedConditionalRecordInput(
    byte SourcePresenceByte,
    CharacterKeyedSixDwordPayload Payload);

public readonly record struct CharacterKeyedThreeDwordPayload(
    uint Dword0,
    uint Dword1,
    uint Dword2);

public readonly record struct CharacterKeyedDescriptorSnapshot(
    string TextAt4CFC,
    uint DwordAt4D00,
    CharacterKeyedThreeDwordPayload DwordsAt4D04,
    byte ByteAt4D10,
    byte ControlByteAt4C70);

public readonly record struct CharacterKeyedDescriptorInput(
    string SelectedText,
    uint DwordAt4D00,
    CharacterKeyedThreeDwordPayload ResolvedDwordsAt4D04,
    byte SourceByteAt4D10);

/// <summary>Pure field updates after native lookup and descriptor inputs are already resolved.</summary>
public static class CharacterKeyedRecipientFallbackModel
{
    public static CharacterKeyedConditionalRecordSnapshot ApplyConditionalRecord(
        CharacterKeyedConditionalRecordSnapshot current,
        CharacterKeyedConditionalRecordInput input)
    {
        if (input.SourcePresenceByte == 0)
            return current;

        return new CharacterKeyedConditionalRecordSnapshot(
            current.PresenceByte == 0 ? (byte)1 : current.PresenceByte,
            input.Payload);
    }

    public static CharacterKeyedDescriptorSnapshot ApplyDescriptorFields(
        CharacterKeyedDescriptorSnapshot current,
        CharacterKeyedDescriptorInput input)
    {
        ArgumentNullException.ThrowIfNull(input.SelectedText);

        byte controlByte = current.ControlByteAt4C70;
        if ((input.DwordAt4D00 & CharacterLocalControllerApplicationNativeLayout.Message79FallbackDescriptorClearMask) != 0)
            controlByte = 0;
        else if ((input.DwordAt4D00 & CharacterLocalControllerApplicationNativeLayout.Message79FallbackDescriptorSetMask) != 0)
            controlByte = 1;

        byte byteAt4D10 = current.ByteAt4D10;
        if ((current.DwordAt4D00 & 0x02) == 0 && (input.DwordAt4D00 & 0x02) != 0)
            byteAt4D10 = input.SourceByteAt4D10;

        return new CharacterKeyedDescriptorSnapshot(
            input.SelectedText,
            input.DwordAt4D00,
            input.ResolvedDwordsAt4D04,
            byteAt4D10,
            controlByte);
    }
}
