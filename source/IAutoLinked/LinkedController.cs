using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using CustomComponents;
using UnityEngine;

namespace CustomComponents
{
    internal class LinkedController
    {
        public static void ValidateMech(Dictionary<MechValidationType, List<string>> errors, MechValidationLevel validationlevel, MechDef mechdef)
        {
            foreach (var linked_item in mechdef.Inventory.Select(i => i.Def).OfType<IAutoLinked>().Where(i => i.Links != null && i.Links.Length > 0))
            {
                foreach (var link in linked_item.Links)
                {
                    if (!mechdef.Inventory.Any(i =>
                        i.MountedLocation == link.Location && i.ComponentDefID == link.ApendixID))
                    {
                        errors[MechValidationType.InvalidInventorySlots].Add($"{(linked_item as MechComponentDef).Description.Name} have critical errors, reinstall it to fix");
                    }
                }
            }
        }

        public static bool ValidateMechCanBeFielded(MechDef mechdef)
        {
            foreach (var linked_item in mechdef.Inventory.Select(i => i.Def).OfType<IAutoLinked>().Where(i => i.Links != null && i.Links.Length > 0))
            {
                foreach (var link in linked_item.Links)
                {
                    if (!mechdef.Inventory.Any(i =>
                        i.MountedLocation == link.Location && i.ComponentDefID == link.ApendixID))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static IValidateDropResult ValidateDrop(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result)
        {
            if (last_result is ValidateDropChange changes)
            {
                var list = new List<IChange>();
                var helper = new MechLabHelper(location.mechLab);
                foreach (var change in changes.Changes.OfType<SlotChange>())
                {
                    if (change.item.ComponentRef.Def is IAutoLinked l && l.Links != null && l.Links.Length > 0)
                    {
                        if (change is AddChange)
                        {
                            Control.Logger.LogDebug($"Need to add for {change.item.ComponentRef.ComponentDefID}");

                            foreach (var a_link in l.Links)
                            {
                                Control.Logger.LogDebug($"{a_link.ApendixID} to {a_link.Location}");
                                var cref = CreateHelper.Ref(a_link.ApendixID, a_link.BaseType,
                                    location.mechLab.dataManager);
                                if (cref != null)
                                {
                                    Control.Logger.LogDebug($"added");
                                    var slot = CreateHelper.Slot(location.mechLab, cref, a_link.Location);
                                    list.Add( new AddChange(a_link.Location, slot));
                                }
                                else
                                    Control.Logger.LogDebug($"not found");

                            }
                        }

                        else if (change is RemoveChange)
                        {
                            Control.Logger.LogDebug($"Need to remove for {change.item.ComponentRef.ComponentDefID}");
                            foreach (var r_link in l.Links)
                            {
                                var widget = helper.GetLocationWidget(r_link.Location);
                                if (widget != null)
                                {
                                    Control.Logger.LogDebug($"{r_link.ApendixID} from {r_link.Location}");
                                    var remove = new LocationHelper(widget).LocalInventory.FirstOrDefault(e =>
                                        e?.ComponentRef?.ComponentDefID == r_link.ApendixID);
                                    if (remove != null)
                                    {
                                        Control.Logger.LogDebug($"removed");
                                        list.Add(new RemoveChange(r_link.Location, remove));
                                    }
                                    else
                                        Control.Logger.LogDebug($"not found");
                                }
                            }
                        }

                    }
                }
                if(list.Count > 0)
                    changes.Changes.AddRange(list);
            }


            if (element.ComponentRef.Def is IAutoLinked link && link.Links != null)
            {
                foreach (var a_link in link.Links)
                {
                    var cref = CreateHelper.Ref(a_link.ApendixID, a_link.BaseType,
                        location.mechLab.dataManager);

                    if (cref == null)
                    {
                        return new ValidateDropError($"Cannot Add {element.ComponentRef.Def.Description.Name} - Linked element not exist");
                    }
                        var slot = CreateHelper.Slot(location.mechLab, cref, a_link.Location);
                    last_result = ValidateDropChange.AddOrCreate(last_result, new AddChange(a_link.Location, slot));
                }
            }

            return last_result;
        }

        public static void RemoveLinked(MechLabPanel mechlab, IMechLabDraggableItem item, IAutoLinked linked)
        {
            var helper = new MechLabHelper(mechlab);
            foreach (var r_link in linked.Links)
            {
                var widget = helper.GetLocationWidget(r_link.Location);
                if (widget != null)
                {
                    Control.Logger.LogDebug($"{r_link.ApendixID} from {r_link.Location}");
                    var location = new LocationHelper(widget);
                    var remove = location.LocalInventory.FirstOrDefault(e =>
                        e?.ComponentRef?.ComponentDefID == r_link.ApendixID);

                    if (remove != null)
                    {
                        Control.Logger.LogDebug($"removed");
                        widget.OnRemoveItem(remove, true);

                        if (item.ComponentRef.Def is IDefault)
                            GameObject.Destroy(remove.gameObject);
                        else
                        {
                            var temp = mechlab.DragItem as MechLabItemSlotElement;
                            mechlab.ForceItemDrop(remove);
                            helper.SetDragItem(temp);
                        }
                    }
                    else
                        Control.Logger.LogDebug($"not found");
                }
            }
        }
    }
}