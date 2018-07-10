using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using System.Linq;

namespace CustomComponents
{
    public class Link
    {
        public ChassisLocations Location;
        public string ApendixID;
        public ComponentType BaseType;
    }

    [CustomComponent("Linked")]
    public class AutoLinked : SimpleCustomComponent, IOnItemGrabbed, IMechValidate, IOnInstalled, IDefaultValidateDrop
    {
        public Link[] Links { get; set; }

        public void OnItemGrabbed(IMechLabDraggableItem item, MechLabPanel mechLab, MechLabLocationWidget w)
        {
            if (Links != null && Links.Length > 0)
            {
                RemoveLinked(item, mechLab);
            }
        }

        public void RemoveLinked(IMechLabDraggableItem item, MechLabPanel mechLab)
        {
            var helper = new MechLabHelper(mechLab);
            foreach (var r_link in Links)
            {
                var target = helper.GetLocationWidget(r_link.Location);
                if (target != null)
                {
                    Control.Logger.LogDebug($"{r_link.ApendixID} from {r_link.Location}");
                    var location = new LocationHelper(target);

                    var remove = location.LocalInventory.FirstOrDefault(e =>
                        e?.ComponentRef?.ComponentDefID == r_link.ApendixID);

                    if (remove != null)
                    {
                        target.OnRemoveItem(remove, true);
                        if (remove.ComponentRef.Is<Flags>(out var f) && f.Default)
                        {
                            remove.thisCanvasGroup.blocksRaycasts = true;
                            mechLab.dataManager.PoolGameObject(MechLabPanel.MECHCOMPONENT_ITEM_PREFAB, item.GameObject);
                        }
                        else
                        {
                            Control.Logger.LogDebug($"removed");
                            mechLab.ForceItemDrop(remove);
                            helper.SetDragItem(item as MechLabItemSlotElement);
                        }
                    }
                    else
                        Control.Logger.LogDebug($"not found");
                }
            }
        }

        public void ValidateMech(Dictionary<MechValidationType, List<string>> errors, MechValidationLevel validationLevel, MechDef mechDef)
        {
            if (Links?.Any(link => !mechDef.Inventory.Any(i =>
                    i.MountedLocation == link.Location && i.ComponentDefID == link.ApendixID)) == true)
            {
                errors[MechValidationType.InvalidInventorySlots].Add($"{Def.Description.Name} have critical errors, reinstall it to fix");
            }
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            return Links == null || Links.All(link => mechDef.Inventory.Any(i => i.MountedLocation == link.Location && i.ComponentDefID == link.ApendixID));
        }

        public void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech)
        {
            Control.Logger.LogDebug($"- AutoLinked");

            if (order.PreviousLocation != ChassisLocations.None)
                foreach (var link in Links)
                {
                    Control.Logger.LogDebug($"-- removing {link.ApendixID} from {link.Location}");
                    DefaultHelper.RemoveDefault(link.ApendixID, mech, link.Location, link.BaseType);
                }

            if (order.DesiredLocation != ChassisLocations.None)
                foreach (var link in Links)
                {
                    Control.Logger.LogDebug($"-- adding {link.ApendixID} to {link.Location}");
                    DefaultHelper.AddDefault(link.ApendixID, mech, link.Location, link.BaseType, state);
                }

        }

        public IValidateDropResult DefaultValidateDrop(MechLabItemSlotElement element, LocationHelper location,
            IValidateDropResult last_result)
        {
            if (last_result is ValidateDropChange changes)
            {
                var list = new List<IChange>();
                var helper = new MechLabHelper(location.mechLab);
                foreach (var change in changes.Changes.OfType<SlotChange>())
                {
                    if (change.item.ComponentRef.Is<AutoLinked>(out var l) && l.Links != null && l.Links.Length > 0)
                    {
                        if (change is AddChange)
                        {
                            Control.Logger.LogDebug($"Need to add for {change.item.ComponentRef.ComponentDefID}");

                            foreach (var a_link in l.Links)
                            {
                                Control.Logger.LogDebug($"{a_link.ApendixID} to {a_link.Location}");
                                var cref = CreateHelper.Ref(a_link.ApendixID, a_link.BaseType,
                                    location.mechLab.dataManager, location.mechLab.sim);
                                if (cref != null)
                                {
                                    Control.Logger.LogDebug($"added");
                                    var slot = CreateHelper.Slot(location.mechLab, cref, a_link.Location);
                                    list.Add(new AddChange(a_link.Location, slot));
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
                if (list.Count > 0)
                    changes.Changes.AddRange(list);
            }


            if (Links != null)
            {
                foreach (var a_link in Links)
                {
                    var cref = CreateHelper.Ref(a_link.ApendixID, a_link.BaseType,
                        location.mechLab.dataManager, location.mechLab.sim);

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
    }
}