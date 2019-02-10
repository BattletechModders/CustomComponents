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
            var helper = new MechLabHelper(mechLab);
            foreach (var r_link in Links)
            {
<<<<<<< HEAD
                Control.LogDebug(DType.ComponentInstall, $"{r_link.ComponentDefId} from {r_link.Location}");
=======
                Control.Logger.LogDebug($"{r_link.ComponentDefId} from {r_link.Location}");
>>>>>>> 1e08dc5bf057d35d653da8490ae945a396efa9fe
                DefaultHelper.RemoveMechLab(r_link.ComponentDefId, Def.ComponentType, helper, r_link.Location);
            }
        }

        private bool ValidateMech(MechDef mechDef)
        {
            return Links == null || Links.All(link => mechDef.Inventory.Any(i => i.MountedLocation == link.Location && i.ComponentDefID == link.ComponentDefId));
        }

        public void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors, MechValidationLevel validationLevel, MechDef mechDef, MechComponentRef componentRef)
        {
            if (ValidateMech(mechDef))
            {
                return;
            }
            errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text($"{Def.Description.Name} have critical errors, reinstall it to fix"));
        }

        public bool ValidateMechCanBeFielded(MechDef mechDef, MechComponentRef componentRef)
        {
            return ValidateMech(mechDef);
        }

        public void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech)
        {
            Control.LogDebug(DType.ComponentInstall, $"- AutoLinked");

            if (order.PreviousLocation != ChassisLocations.None)
                foreach (var link in Links)
                {
                    Control.LogDebug(DType.ComponentInstall, $"-- removing {link.ComponentDefId} from {link.Location}");
                    DefaultHelper.RemoveInventory(link.ComponentDefId, mech, link.Location, Def.ComponentType);
                }

            if (order.DesiredLocation != ChassisLocations.None)
                foreach (var link in Links)
                {
                    Control.LogDebug(DType.ComponentInstall, $"-- adding {link.ComponentDefId} to {link.Location}");
                    DefaultHelper.AddInventory(link.ComponentDefId, mech, link.Location, Def.ComponentType, state);
                }

        }

        public IEnumerable<IChange> ValidateDropOnAdd(MechLabItemSlotElement item, LocationHelper locationHelper, MechLabHelper mechlab , List<IChange> changes)
        {
            Control.LogDebug(DType.ComponentInstall, "--- AutoLinked Add");

            if (Links == null || Links.Length == 0)
                yield break;

            foreach (var link in Links)
            {
                Control.LogDebug(DType.ComponentInstall, $"---- {link.ComponentDefId} to {link.Location}");
                var slot = DefaultHelper.CreateSlot(link.ComponentDefId, Def.ComponentType, mechlab.MechLab);

                if (slot != null)
                {
                    Control.LogDebug(DType.ComponentInstall, $"----- added");
                    yield return new AddDefaultChange(link.Location, slot);
                }
                else
                    Control.LogDebug(DType.ComponentInstall, $"----- not found");
            }

        }

        public IEnumerable<IChange> ValidateDropOnRemove(MechLabItemSlotElement item, LocationHelper locationHelper, MechLabHelper mechlab, List<IChange> changes)
        {
            Control.LogDebug(DType.ComponentInstall, "--- AutoLinked Remove");

            if (Links == null || Links.Length == 0)
                yield break;

            foreach (var link in Links)
            {
                var widget = mechlab.GetLocationWidget(link.Location);
                if (widget == null)
                {
<<<<<<< HEAD
                    Control.LogDebug(DType.ComponentInstall, $"---- {link.ComponentDefId} from {link.Location}");
                    var remove = new LocationHelper(widget).LocalInventory.FirstOrDefault(e =>
                        e?.ComponentRef?.ComponentDefID == link.ComponentDefId);
                    if (remove != null)
                    {
                        Control.LogDebug(DType.ComponentInstall, $"----- removed");
                        yield return new RemoveChange(link.Location, remove);

                    }
                    else
                        Control.LogDebug(DType.ComponentInstall, $"----- not found");
=======
                    continue;
>>>>>>> 1e08dc5bf057d35d653da8490ae945a396efa9fe
                }

                Control.Logger.LogDebug($"---- {link.ComponentDefId} from {link.Location}");
                var remove = new LocationHelper(widget).LocalInventory.FirstOrDefault(e =>
                    e?.ComponentRef?.ComponentDefID == link.ComponentDefId);
                if (remove != null)
                {
                    Control.Logger.LogDebug($"----- removed");
                    yield return new RemoveChange(link.Location, remove);

                }
                else
                    Control.Logger.LogDebug($"----- not found");
            }
        }

        public void ClearInventory(MechDef mech, List<MechComponentRef> result, SimGameState state, MechComponentRef source)
        {
            foreach (var link in Links)
            {
                result.RemoveAll(item => item.ComponentDefID == link.ComponentDefId && item.MountedLocation == link.Location);
            }
        }
    }
}