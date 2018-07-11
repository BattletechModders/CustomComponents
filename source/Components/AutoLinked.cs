using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using System.Linq;

namespace CustomComponents
{
    public class Link
    {
        public ChassisLocations Location;
        public string ComponentDefId;
    }

    [CustomComponent("Linked")]
    public class AutoLinked : SimpleCustomComponent, IOnItemGrabbed, IMechValidate, IOnInstalled, IAdjustValidateDrop
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
                    Control.Logger.LogDebug($"{r_link.ComponentDefId} from {r_link.Location}");
                    var location = new LocationHelper(target);

                    var remove = location.LocalInventory.FirstOrDefault(e =>
                        e?.ComponentRef?.ComponentDefID == r_link.ComponentDefId);

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
                    i.MountedLocation == link.Location && i.ComponentDefID == link.ComponentDefId)) == true)
            {
                errors[MechValidationType.InvalidInventorySlots].Add($"{Def.Description.Name} have critical errors, reinstall it to fix");
            }
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            return Links == null || Links.All(link => mechDef.Inventory.Any(i => i.MountedLocation == link.Location && i.ComponentDefID == link.ComponentDefId));
        }

        public void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech)
        {
            Control.Logger.LogDebug($"- AutoLinked");

            if (order.PreviousLocation != ChassisLocations.None)
                foreach (var link in Links)
                {
                    Control.Logger.LogDebug($"-- removing {link.ComponentDefId} from {link.Location}");
                    DefaultHelper.RemoveDefault(link.ComponentDefId, mech, link.Location, Def.ComponentType );
                }

            if (order.DesiredLocation != ChassisLocations.None)
                foreach (var link in Links)
                {
                    Control.Logger.LogDebug($"-- adding {link.ComponentDefId} to {link.Location}");
                    DefaultHelper.AddDefault(link.ComponentDefId, mech, link.Location, Def.ComponentType, state);
                }

        }

        public IEnumerable<IChange> ValidateDropOnAdd(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab)
        {
            Control.Logger.LogDebug("-- AutoLinked Add");

            if (Links == null || Links.Length == 0)
                yield break;

            foreach(var link in Links)
            {
                Control.Logger.LogDebug($"--- {link.ComponentDefId} to {link.Location}");
                var cref = CreateHelper.Ref(link.ComponentDefId, item.ComponentRef.ComponentDefType,
                    location.mechLab.dataManager, location.mechLab.sim);
                if (cref != null)
                {
                    Control.Logger.LogDebug($"---- added");
                    var slot = CreateHelper.Slot(location.mechLab, cref, link.Location);
                    yield return new AddChange(link.Location, slot);
                }
                else
                    Control.Logger.LogDebug($"---- not found");
            }

        }

        public IEnumerable<IChange> ValidateDropOnRemove(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab)
        {

            Control.Logger.LogDebug("-- AutoLinked Remove");

            if (Links == null || Links.Length == 0)
                yield break;

            foreach (var link in Links)
            {
                var widget = mechlab.GetLocationWidget(link.Location);
                if (widget != null)
                {
                    Control.Logger.LogDebug($"--- {link.ComponentDefId} from {link.Location}");
                    var remove = new LocationHelper(widget).LocalInventory.FirstOrDefault(e =>
                        e?.ComponentRef?.ComponentDefID == link.ComponentDefId);
                    if (remove != null)
                    {
                        Control.Logger.LogDebug($"---- removed");
                        yield return new RemoveChange(link.Location, remove);

                    }
                    else
                        Control.Logger.LogDebug($"---- not found");
                }
            }
        }
    }
}