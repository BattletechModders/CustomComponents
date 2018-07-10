using System;
using System.Collections.Generic;
using System.Text;
using BattleTech.UI;

namespace CustomComponents
{
    [CustomComponent("Replace")]
    public class AutoReplace : SimpleCustomComponent, IOnItemGrabbed
    {
        public string ReplaceID { get; set; }

        public void OnItemGrabbed(IMechLabDraggableItem item, MechLabPanel mechLab, MechLabLocationWidget widget)
        {
            if (string.IsNullOrEmpty(ReplaceID))
                return;

            var new_ref = CreateHelper.Ref(ReplaceID, item.ComponentRef.ComponentDefType, mechLab.dataManager, mechLab.sim);
            if (new_ref == null || new_ref.Def == null)
            {
                Control.Logger.LogError($"Replacement {ReplaceID} for {Def.Description.Id} not found, skipping");
                return;
            }
            if(!new_ref.Def.Is<Flags>(out var f) || !f.Default)
            {
                Control.Logger.LogError($"Replacement {ReplaceID} for {Def.Description.Id} is not default, not implemented");
                return;
            }

            var slot = CreateHelper.Slot(mechLab, new_ref, widget.loadout.Location);

            widget.OnAddItem(slot, false);
            mechLab.ValidateLoadout(false);
        }
    }
}
