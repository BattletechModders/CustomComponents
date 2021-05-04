using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using BattleTech;
using CustomComponents.Changes;

namespace CustomComponents
{
    public class InventoryOperationState
    {
        private List<IChange_Apply> done_changes;
        private Queue<IChange> pending_changes;
        public List<InvItem> Inventory { get; private set; }

        public MechDef Mech { get; private set; }

        public InventoryOperationState(Queue<IChange> start_changes, MechDef mech, IEnumerable<InvItem> inventory = null)
        {
            done_changes = new List<IChange_Apply>();
            Mech = mech;
            Inventory = (inventory ?? mech.Inventory.ToInvItems()).ToList();

            pending_changes = start_changes;

        }

        public void DoChanges()
        {
            foreach (var change in pending_changes)
            {
                if (change is IChange_Apply iichange)
                    iichange.PreviewApply(this);
            }

            while (pending_changes.Count > 0)
            {
                var change = pending_changes.Dequeue();
                if (change is IChange_Adjust adj && pending_changes.Any(i => i is IChange_Adjust adj2 && adj2.ChangeID == adj.ChangeID))
                    continue;

                change.AdjustChange(this);

                if (change is IChange_Apply iichange)
                {
                    done_changes.Add(iichange);
                    if(iichange is IChange_Optimize ioc)
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
            pending_changes.Enqueue(change);
            if(change is IChange_Apply iichange)
                iichange.PreviewApply(this);
        }

        public void ApplyInventory()
        {
            var inv = Mech.Inventory.ToList();
            foreach (var change in done_changes)
            {
                change.ApplyToInventory(Mech, inv);
            }
            Mech.SetInventory(inv.ToArray());
        }

        public void ApplyMechlab()
        {
            foreach (var changeApply in done_changes)
            {
                changeApply.ApplyToMechlab();
            }
        }
    }
}