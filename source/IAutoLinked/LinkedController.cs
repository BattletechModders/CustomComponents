using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    internal class LinkedController
    {
        public static IValidateDropResult ValidateDrop(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result)
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


            if (element.ComponentRef.Is<AutoLinked>(out var link) && link.Links != null)
            {
                foreach (var a_link in link.Links)
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