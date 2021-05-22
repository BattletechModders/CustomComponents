using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using BattleTech;
using CustomComponents.Changes;

namespace CustomComponents
{
    public class InventoryOperationState
    {
        private List<IChange_Apply > done_changes;
        private Queue<IChange > pending_changes;
        public List<InvItem> Inventory { get; private set; }

        public MechDef Mech { get; private set; }

        public InventoryOperationState(Queue<IChange> start_changes, MechDef mech, IEnumerable<InvItem> inventory = null)
        {
            done_changes = new List<IChange_Apply>();
            Mech = mech;
            Inventory = (inventory ?? mech.Inventory.ToInvItems()).ToList();

            pending_changes = new Queue<IChange>();
            foreach (var change in start_changes)
            {
                change.Initial = true;
                pending_changes.Enqueue(change);
            }
        }

        public void DoChanges()
        {
            Control.LogDebug(DType.InventoryOperaions, "DoChanges for {0}", Mech.Description.Id);
            Control.LogDebug(DType.InventoryOperaions, "- Initial preview");

            foreach (var change in pending_changes)
            {
                if (change is IChange_Apply iichange)
                {
                    Control.LogDebug(DType.InventoryOperaions, "-- {0}", change);
                    iichange.PreviewApply(this);
                }
            }

            Control.LogDebug(DType.InventoryOperaions, "- iteration");
            while (pending_changes.Count > 0)
            {
                var change = pending_changes.Dequeue();
                if (change is IChange_Adjust adj &&
                    pending_changes.Any(i => i is IChange_Adjust adj2 && adj2.ChangeID == adj.ChangeID))
                {
                    Control.LogDebug(DType.InventoryOperaions, "-- Skip {0}", change);
                    continue;
                }

                Control.LogDebug(DType.InventoryOperaions, "-- Adjust {0}", change);
                change.AdjustChange(this);

                if (change is IChange_Apply iichange)
                {
                    done_changes.Add(iichange);
                    if (iichange is IChange_Optimize ioc)
                        ioc.DoOptimization(done_changes);
                }
            }
        }

        public IEnumerable<IChange_Apply> GetResults()
        {
            return done_changes;
        }

        public void AddChange(IChange change)
        {
            Control.LogDebug(DType.InventoryOperaions, "--- {0}, ", change);

            change.Initial = false;
            pending_changes.Enqueue(change);
            if(change is IChange_Apply iichange)
                iichange.PreviewApply(this);
        }

        public void ApplyInventory()
        {
            Control.LogDebug(DType.ComponentInstall, "ApplyInventory");
            var inv = Mech.Inventory.ToList();
            foreach (var change in done_changes)
            {
                Control.LogDebug(DType.ComponentInstall, "- {0}", change);
                change.ApplyToInventory(Mech, inv);
            }
            Mech.SetInventory(inv.ToArray());
        }

        public void ApplyMechlab()
        {
            Control.LogDebug(DType.ComponentInstall, "ApplyMechlab");
            foreach (var change in done_changes)
            {
                Control.LogDebug(DType.ComponentInstall, "- {0}", change);
                change.ApplyToMechlab();
            }
        }
    }
}