using System;
using System.Linq;
using BattleTech;
using fastJSON;

namespace CustomComponents
{
    /// <summary>
    /// component use category logic
    /// </summary>
    [CustomComponent("Category")]
    public class Category : SimpleCustomComponent, IAfterLoad, IOnInstalled
    {
        /// <summary>
        /// name of category
        /// </summary>
        public string CategoryID { get; set; }
        /// <summary>
        /// optional tag for AllowMixTags, if not set defid will used
        /// </summary>
        string Tag;

        public string GetTag()
        {
            if (string.IsNullOrEmpty(Tag))
                return Def.Description.Id;
            else
                return Tag;
        }

        [JsonIgnore]
        public CategoryDescriptor CategoryDescriptor { get; set; }

        public void OnLoaded()
        {
            CategoryDescriptor = Control.GetCategory(CategoryID);
        }

        public void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech)
        {

            Control.Logger.LogDebug($"- Category");

            if (CategoryDescriptor.AutoReplace || CategoryDescriptor.MaxEquiped < 0 && CategoryDescriptor.MaxEquipedPerLocation < 0)
            {
                var c1 = CategoryDescriptor;
                Control.Logger.LogDebug($"-- {c1.DisplayName} r:{c1.AutoReplace}  max:{c1.MaxEquiped} mpr:{c1.MaxEquipedPerLocation} = not requre replace");
            }

            if (order.DesiredLocation == ChassisLocations.None)
            {
                Control.Logger.LogDebug("-- removing, no additional actions");
                return;
            }


            int n1 = mech.Inventory.Count(i => i.Is<Category>(out var cat) && CategoryID == cat.CategoryID);
            int n2 = mech.Inventory.Count(i => i.MountedLocation == order.DesiredLocation && i.Is<Category>(out var cat)
                   && CategoryID == cat.CategoryID);

            Control.Logger.LogDebug($"-- total {n1}/{CategoryDescriptor.MaxEquiped}  location: {n2}/{CategoryDescriptor.MaxEquipedPerLocation}");

            var replace = mech.Inventory.FirstOrDefault(i => i.MountedLocation == order.DesiredLocation && i.Is<Category>(out var cat)
                   &&CategoryID == cat.CategoryID && i.Is<Flags>(out var f) && f.Default);

            Control.Logger.LogDebug($"-- possible replace: {(replace == null ? "not found" : replace.ComponentDefID)}");

            if (replace == null)
                return;

            bool need_replace = (CategoryDescriptor.MaxEquiped > 0 && n1 > CategoryDescriptor.MaxEquiped) ||
                (CategoryDescriptor.MaxEquipedPerLocation > 0 && n2 > CategoryDescriptor.MaxEquipedPerLocation);

            Control.Logger.LogDebug($"-- need_repalce: {need_replace}");

            if (need_replace)
                DefaultHelper.RemoveDefault(replace.ComponentDefID, mech, order.DesiredLocation, replace.ComponentDefType);

        }
    }
}