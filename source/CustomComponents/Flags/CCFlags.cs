using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Localize;

namespace CustomComponents;

public class CCFlags
{
    public bool InvUnlimited { get; }

    public bool Default { get; }
    public bool AutoRepair { get; }
    public bool NoRemove { get; }
    public bool NoSalvage { get; }
    public bool HideFromInv { get; }

    public bool HideFromEquip { get; }
    public bool HideFromCombat { get; }
    public bool NotBroken { get; }
    public bool Vital { get; }
    public bool NotDestroyed { get; }
    public bool Invalid { get; }

    internal CCFlags()
    {
    }

    internal CCFlags(Flags flags)
    {
        InvUnlimited = flags.IsSet("inv_unlimited");

        Default = flags.IsSet("default");
        AutoRepair = Default || flags.IsSet("autorepair");
        NoRemove = Default || flags.IsSet("no_remove");
        NoSalvage = Default || InvUnlimited || flags.IsSet("no_salvage");
        HideFromInv = Default || flags.IsSet("hide");

        HideFromEquip = flags.IsSet("hide_equip");
        HideFromCombat = flags.IsSet("hide_combat");
        NotBroken = flags.IsSet("not_broken");
        Vital = flags.IsSet("vital");
        NotDestroyed = flags.IsSet("not_destroyed");
        Invalid = flags.IsSet("invalid");
    }

    internal static bool CanBeFielded(MechDef mechDef)
    {
        foreach (var item in mechDef.Inventory)
        {
            var f = item.Def.CCFlags();

            if (f.Invalid)
            {
                return false;
            }

            if (item.DamageLevel == ComponentDamageLevel.Destroyed && (f.NotBroken || f.NotDestroyed))
            {
                return false;
            }

            if (item.DamageLevel == ComponentDamageLevel.Penalized && f.NotBroken)
            {
                return false;
            }
        }
        return true;
    }

    internal static void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationLevel, MechDef mechDef)
    {
        foreach (var item in mechDef.Inventory)
        {
            var f = item.Def.CCFlags();

            if (f.Invalid)
            {
                errors[MechValidationType.InvalidInventorySlots].Add(new(
                    Control.Settings.Message.Flags_InvaildComponent, item.Def.Description.Name, item.Def.Description.UIName));
            }

            if (item.DamageLevel == ComponentDamageLevel.Destroyed && (f.NotBroken || f.NotDestroyed))
            {
                errors[MechValidationType.StructureDestroyed].Add(new(
                    Control.Settings.Message.Flags_DestroyedComponent, item.Def.Description.Name, item.Def.Description.UIName));
            }

            if (item.DamageLevel == ComponentDamageLevel.Penalized && f.NotBroken)
            {
                errors[MechValidationType.StructureDestroyed].Add(new(
                    Control.Settings.Message.Flags_DamagedComponent, item.Def.Description.Name, item.Def.Description.UIName));
            }
        }
    }

    public override string ToString()
    {
        var flagsActive = GetType()
            .GetProperties()
            .Where(propertyInfo => (bool)propertyInfo.GetValue(this))
            .Select(propertyInfo => propertyInfo.Name)
            .ToList();
        return string.Join(" ", flagsActive);
    }
}