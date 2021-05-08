using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using Harmony;
using Localize;

namespace CustomComponents
{
    internal class TagRestrictionsHandler
    {
        internal static TagRestrictionsHandler Shared = new TagRestrictionsHandler();

        private Dictionary<string, TagRestrictions> _restrictions { get; set;  }
        internal static Dictionary<string, TagRestrictions> Restrictions => Shared._restrictions;

        internal void Setup(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            Control.LogDebug(DType.CustomResource, " - TagRestriction");
            _restrictions = SettingsResourcesTools.Enumerate<TagRestrictions>("CCTagRestrictions", customResources)
                .ToDictionary(entry => entry.Tag);

            if (Control.Settings.DebugInfo.HasFlag(DType.CustomResource))
            {
                foreach (var pair in _restrictions)
                {
                    Control.LogDebug(DType.CustomResource, $" -- {pair.Key}");
                }
            }
        }

        internal bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            var checker = new TagsChecker(mechDef.Chassis, mechDef.Inventory.ToList());
            return checker.Validate() == null;
        }

        internal void ValidateMech(Dictionary<MechValidationType, List<Text>> errors, MechValidationLevel validationLevel, MechDef mechDef)
        {
            var checker = new TagsChecker(mechDef.Chassis, mechDef.Inventory.ToList(), errors);
            checker.Validate();
        }

        public string ValidateDrop(MechLabItemSlotElement drop_item, ChassisLocations location)
        {
            var mechDef = MechLabHelper.CurrentMechLab.ActiveMech;

            var checker = new TagsChecker(mechDef.Chassis, mechDef.Inventory.ToList());
            return checker.ValidateDrop(
                Control.Settings.TagRestrictionDropValidateRequiredTags,
                Control.Settings.TagRestrictionDropValidateIncompatibleTags,
                drop_item.ComponentRef,
                location
            );
        }
    }
}