namespace Firefall.Reconstruction.Startup;

public readonly record struct UiManagerAngleCorrectionInput(
    float OriginalSum,
    float CapturedRemainder,
    byte InputFlag,
    byte ParentFlag);

public readonly record struct UiManagerAngleCorrectionProjection(
    float ProjectedValue,
    bool ProjectedFlag);

public static class UiManagerAngleCorrectionProjectionRules
{
    public const uint DerivedStateMethodFileOffset = 0xCCA190;
    public const uint AngleCorrectionStubFileOffset = 0x159DD00;
    public const string AngleCorrectionStubBytes = "FF 25 20 48 AC 01";
    public const uint AngleCorrectionIatVirtualAddress = 0x1AC4820;
    public const uint AngleCorrectionIatFileOffset = 0x16C3420;
    public const string AngleCorrectionImport = "MSVCR120.dll!_CIfmod";
    public const uint TwoPiConstantVirtualAddress = 0x1C5E1C4;
    public const uint TwoPiConstantFileOffset = 0x185CDC4;
    public const uint TwoPiConstantBits = 0x40C90FDB;
    public const uint ZeroConstantVirtualAddress = 0x1C817A4;
    public const uint ZeroConstantFileOffset = 0x18803A4;
    public const uint OneConstantVirtualAddress = 0x1C81BE8;
    public const uint OneConstantFileOffset = 0x18807E8;
    public const int FirstValueInputOffset = 8;
    public const int ParentValueInputOffset = 0x14;
    public const int ProjectedValueOffset = 0x14;
    public const int InputFlagOffset = 1;
    public const int ParentFlagOffset = 0x24;
    public const int ProjectedFlagOffset = 0x24;
    public const float TwoPi = 6.2831855f;

    public static UiManagerAngleCorrectionProjection Project(
        UiManagerAngleCorrectionInput input)
    {
        float projectedValue = input.OriginalSum < 0f
            && input.CapturedRemainder != 0f
                ? input.CapturedRemainder + TwoPi
                : input.CapturedRemainder;

        return new(
            projectedValue,
            input.InputFlag != 0 && input.ParentFlag != 0);
    }
}
