using System;
using System.Collections.Generic;
using System.Text;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    [CustomComponent("Replace")]
    public class AutoReplace : SimpleCustomComponent, IOnItemGrabbed, IOnInstalled
    {
        public string ComponentDefId { get; set; }

        public void OnItemGrabbed(IMechLabDraggableItem item, MechLabPanel mechLab, MechLabLocationWidget widget)
        {
            Control.Logger.LogDebug($"- AutoReplace");
            Control.Logger.LogDebug($"-- search replace for {item.ComponentRef.ComponentDefID}");
            if (string.IsNullOrEmpty(ComponentDefId))
            {
                Control.Logger.LogDebug($"-- no replacement, skipping");
                return;
            }

            DefaultHelper.AddMechLab(ComponentDefId, Def.ComponentType, new MechLabHelper(mechLab), widget.loadout.Location);
            Control.Logger.LogDebug($"-- added {ComponentDefId}");
            mechLab.ValidateLoadout(false);
        }

        public void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech)
        {
            Control.Logger.LogDebug($"- AutoReplace");
            Control.Logger.LogDebug($"-- search replace for {order.MechComponentRef.ComponentDefID}");
            if (order.PreviousLocation != ChassisLocations.None)
            {
                Control.Logger.LogDebug($"-- found, adding {ComponentDefId} to {order.PreviousLocation}");
                DefaultHelper.AddInventory(ComponentDefId, mech, order.PreviousLocation, order.MechComponentRef.ComponentDefType, state);
            }
            else
            {
                Control.Logger.LogDebug($"-- new component, not replacement needed");
            }
        }
    }
}
