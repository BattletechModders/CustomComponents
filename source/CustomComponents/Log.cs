#nullable enable
using HBS.Logging;
using NullableLogging;

namespace CustomComponents;

internal static class Log
{
    private const string Name = nameof(CustomComponents);
    internal static readonly NullableLogger Main = NullableLogger.GetLogger(Name, LogLevel.Debug);

    private static NullableLogger GetSubLogger(string subName)
    {
        return NullableLogger.GetLogger(Name + "." + subName);
    }

    internal static readonly NullableLogger AutoFix = GetSubLogger(nameof(AutoFix));
    internal static readonly NullableLogger AutoFixBase = GetSubLogger(nameof(AutoFixBase));
    internal static readonly NullableLogger AutoFixFAKE = GetSubLogger(nameof(AutoFixFAKE));
    internal static readonly NullableLogger AutofixValidate = GetSubLogger(nameof(AutofixValidate));
    internal static readonly NullableLogger CCLoading = GetSubLogger(nameof(CCLoading));
    internal static readonly NullableLogger CCLoadingSummary = GetSubLogger(nameof(CCLoadingSummary));
    internal static readonly NullableLogger ClearInventory = GetSubLogger(nameof(ClearInventory));
    internal static readonly NullableLogger Color = GetSubLogger(nameof(Color));
    internal static readonly NullableLogger ComponentInstall = GetSubLogger(nameof(ComponentInstall));
    internal static readonly NullableLogger CustomResource = GetSubLogger(nameof(CustomResource));
    internal static readonly NullableLogger DefaultsBuild = GetSubLogger(nameof(DefaultsBuild));
    internal static readonly NullableLogger DefaultHandle = GetSubLogger(nameof(DefaultHandle));
    internal static readonly NullableLogger Filter = GetSubLogger(nameof(Filter));
    internal static readonly NullableLogger FixedCheck = GetSubLogger(nameof(FixedCheck));
    internal static readonly NullableLogger Flags = GetSubLogger(nameof(Flags));
    internal static readonly NullableLogger Hardpoints = GetSubLogger(nameof(Hardpoints));
    internal static readonly NullableLogger Icons = GetSubLogger(nameof(Icons));
    internal static readonly NullableLogger InstallCost = GetSubLogger(nameof(InstallCost));
    internal static readonly NullableLogger InventoryOperations = GetSubLogger(nameof(InventoryOperations));
    internal static readonly NullableLogger MechValidation = GetSubLogger(nameof(MechValidation));
    internal static readonly NullableLogger PrefabOverrideCache = GetSubLogger(nameof(PrefabOverrideCache));
    internal static readonly NullableLogger SalvageProcess = GetSubLogger(nameof(SalvageProcess));
    internal static readonly NullableLogger UnitType = GetSubLogger(nameof(UnitType));
    internal static readonly NullableLogger WeaponDefaults = GetSubLogger(nameof(WeaponDefaults));
}