using System.Collections.Generic;
using BattleTech;
using System.Linq;
using CustomComponents.Changes;
using Localize;

namespace CustomComponents
{
    public class Link
    {
        public ChassisLocations Location;
        public string ComponentDefId;
        public ComponentType? ComponentDefType = null;
    }

    [CustomComponent("Linked")]
    public class AutoLinked : SimpleCustomComponent, IOnAdd, IOnRemove
    {
        public Link[] Links { get; set; }

        
        public static void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors, MechValidationLevel validationLevel, MechDef mechDef)
        {
            var linked = mechDef.Inventory
                .Select(i => i.GetComponent<AutoLinked>())
                .Where(i => i != null && i.Links != null)
                .SelectMany(i => i.Links, (a,b) => new { custom = a, link = b}).ToList();

            if (linked.Count > 0)
            {
                var inv = mechDef.Inventory.ToList();

                foreach (var item in linked)
                {
                    var found = inv.FirstOrDefault(i =>
                        i.ComponentDefID == item.link.ComponentDefId && i.MountedLocation == item.link.Location);

                    if (found == null)
                        errors[MechValidationType.InvalidInventorySlots].Add(new Text(
                            Control.Settings.Message.Linked_Validate,
                            mechDef.Description.UIName, item.custom.Def.Description.Name,
                            item.custom.Def.Description.UIName,
                            item.link.Location));
                    else
                        inv.Remove(found);
                }
            }
        }

        public static bool CanBeFielded(MechDef mechDef)
        {
            var linked = mechDef.Inventory
                .Select(i => i.GetComponent<AutoLinked>())
                .Where(i => i != null && i.Links != null)
                .SelectMany(i => i.Links, (a, b) => new { custom = a, link = b }).ToList();

            if (linked.Count > 0)
            {
                var inv = mechDef.Inventory.ToList();

                foreach (var item in linked)
                {
                    var found = inv.FirstOrDefault(i =>
                        i.ComponentDefID == item.link.ComponentDefId && i.MountedLocation == item.link.Location);

                    if (found == null)
                        return false;

                }
            }

            return true;
        }

        
        public void OnAdd(ChassisLocations location, InventoryOperationState state)
        {
            if (Links == null || Links.Length == 0)
                return;

            foreach (var link in Links)
                state.AddChange(new Change_Add(link.ComponentDefId, link.ComponentDefType.HasValue ? link.ComponentDefType.Value : Def.ComponentType, link.Location ));
        }

        public void OnRemove(ChassisLocations location, InventoryOperationState state)
        {
            if (Links == null || Links.Length == 0)
                return;

            foreach (var link in Links)
                state.AddChange(new Change_Remove(link.ComponentDefId, link.Location));
        }
    }
}