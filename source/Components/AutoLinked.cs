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
        public ComponentType? ComponentDefType = null;
    }

    [CustomComponent("Linked")]
    public class AutoLinked : SimpleCustomComponent, IOnItemGrabbed, IMechValidate, IOnInstalled, IAdjustValidateDrop, IClearInventory
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
            foreach (var r_link in Links)
            {
                Control.LogDebug(DType.ComponentInstall, $"{r_link.ComponentDefId} from {r_link.Location}");
                DefaultHelper.RemoveMechLab(r_link.ComponentDefId, r_link.ComponentDefType ?? Def.ComponentType, r_link.Location);
            }
        }

        public void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors, MechValidationLevel validationLevel, MechDef mechDef, MechComponentRef componentRef)
        {
            if (Links?.Any(link => !mechDef.Inventory.Any(i =>
                    i.MountedLocation == link.Location && i.ComponentDefID == link.ComponentDefId)) == true)
            {
                errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text($"{Def.Description.Name} have critical errors, reinstall it to fix"));
            }
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            return Links == null || Links.All(link => mechDef.Inventory.Any(i => i.MountedLocation == link.Location && i.ComponentDefID == link.ComponentDefId));
        }

        public void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech)
        {
            Control.LogDebug(DType.ComponentInstall, $"- AutoLinked");

            if (order.PreviousLocation != ChassisLocations.None)
                foreach (var link in Links)
                {
                    Control.LogDebug(DType.ComponentInstall, $"-- removing {link.ComponentDefId} from {link.Location}");
                    DefaultHelper.RemoveInventory(link.ComponentDefId, mech, link.Location, link.ComponentDefType ?? Def.ComponentType);
                }

            if (order.DesiredLocation != ChassisLocations.None)
                foreach (var link in Links)
                {
                    Control.LogDebug(DType.ComponentInstall, $"-- adding {link.ComponentDefId} to {link.Location}");
                    DefaultHelper.AddInventory(link.ComponentDefId, mech, link.Location, link.ComponentDefType ?? Def.ComponentType, state);
                }

        }

        public void ClearInventory(MechDef mech, List<MechComponentRef> result, SimGameState state, MechComponentRef source)
        {
            foreach (var l in Links)
            {
                result.RemoveAll(item => item.ComponentDefID == l.ComponentDefId && item.MountedLocation == l.Location);
            }
        }

        public void ValidateDropOnAdd(MechLabItemSlotElement item, ChassisLocations location, Queue<IChange> changes)
        {
            Control.LogDebug(DType.ComponentInstall, "--- AutoLinked Add");

            if (Links == null || Links.Length == 0)
                return;

            foreach (var link in Links)
            {
                Control.LogDebug(DType.ComponentInstall, $"---- {link.ComponentDefId} to {link.Location}");
                var slot = DefaultHelper.CreateSlot(link.ComponentDefId, link.ComponentDefType ?? Def.ComponentType);

                if (slot != null)
                {
                    Control.LogDebug(DType.ComponentInstall, $"----- added");
                    changes.Enqueue(new AddDefaultChange(link.Location, slot));
                }
                else
                    Control.LogDebug(DType.ComponentInstall, $"----- not found");
            }
        }

        public void ValidateDropOnRemove(MechLabItemSlotElement item, ChassisLocations location, Queue<IChange> changes)
        {
            Control.LogDebug(DType.ComponentInstall, "--- AutoLinked Remove");

            if (Links == null || Links.Length == 0)
                return;

            foreach (var link in Links)
            {
                var widget = MechLabHelper.CurrentMechLab.GetLocationWidget(link.Location);
                if (widget != null)
                {
                    Control.LogDebug(DType.ComponentInstall, $"---- {link.ComponentDefId} from {link.Location}");
                    var remove = new LocationHelper(widget).LocalInventory.FirstOrDefault(e =>
                        e?.ComponentRef?.ComponentDefID == link.ComponentDefId);
                    if (remove != null)
                    {
                        Control.LogDebug(DType.ComponentInstall, $"----- removed");
                        changes.Enqueue(new RemoveChange(link.Location, remove));

                    }
                    else
                        Control.LogDebug(DType.ComponentInstall, $"----- not found");
                }
            }
        }
    }
}