using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using Localize;

namespace CustomComponents
{
    public class CCFlags
    {
        [CustomFlag("autorepair")]
        public bool AutoRepair { get; set; } = false;
        [CustomFlag("no_remove")]
        public bool NoRemove { get; set; } = false;
        [CustomFlag("hide")]
        public bool HideFromInv { get; set; } = false;
        [CustomFlag("no_salvage")]
        public bool NoSalvage { get; set; } = false;
        [CustomFlag("default")]
        [SubFlags("autorepair", "no_remove", "hide", "no_salvage")]
        public bool Default { get; set; } = false;

        [CustomFlag("not_broken")]
        public bool NotBroken { get; set; } = false;
        [CustomFlag("vital")]
        public bool Vital { get; set; } = false;
        [CustomFlag("not_destroyed")]
        public bool NotDestroyed { get; set; } = false;
        [CustomFlag("invalid")]
        public bool Invalid { get; set; } = false;


        [CustomSetter("default")]
        private bool SetDefault(MechComponentDef item)
        {
            return item.Is<IDefaultComponent>();
        }


        internal static bool CanBeFielded(MechDef mechDef)
        {
            foreach (var item in mechDef.Inventory)
            {
                var f = item.Flags<CCFlags>();

                if (f.Invalid)
                    return false;

                if (item.DamageLevel == ComponentDamageLevel.Destroyed && (f.NotBroken || f.NotDestroyed))
                    return false;

                if (item.DamageLevel == ComponentDamageLevel.Penalized && f.NotBroken)
                    return false;
            }
            return true;
        }

        internal static void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationLevel, MechDef mechDef)
        {
            foreach (var item in mechDef.Inventory)
            {
                var f = item.Flags<CCFlags>();

                if (f.Invalid)
                    errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text(
                        Control.Settings.Message.Flags_InvaildComponent, item.Def.Description.Name, item.Def.Description.UIName));

                if (item.DamageLevel == ComponentDamageLevel.Destroyed && (f.NotBroken || f.NotDestroyed))
                {
                    errors[MechValidationType.StructureDestroyed].Add(new Localize.Text(
                        Control.Settings.Message.Flags_DestroyedComponent, item.Def.Description.Name, item.Def.Description.UIName));
                }

                if (item.DamageLevel == ComponentDamageLevel.Penalized && f.NotBroken)
                {
                    errors[MechValidationType.StructureDestroyed].Add(new Localize.Text(
                        Control.Settings.Message.Flags_DamagedComponent, item.Def.Description.Name, item.Def.Description.UIName));
                }
            }
        }

        public override string ToString()
        {
            var result = "";
            if (Default)
                result += "Default ";
            if (NoRemove)
                result += "NoRemove ";
            if (Vital)
                result += "Vital ";
            if (AutoRepair)
                result += "AutoRepair ";
            if (HideFromInv)
                result += "HideFromInv ";
            if (NotBroken)
                result += "NotBroken ";
            if (NotDestroyed)
                result += "NotDestroyed ";
            if (Invalid)
                result += "Invalid ";
            if (NoSalvage)
                result += "NoSalvage ";

            return result;
        }
    }
}
