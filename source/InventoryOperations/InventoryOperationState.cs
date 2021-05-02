using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using BattleTech;
using CustomComponents.Changes;

namespace CustomComponents
{
    public class InventoryOperationState
    {
        private List<IInvChange> done_changes;
        private Queue<IChange> pending_changes;
        public List<InvItem> Inventory { get; private set; }

        public MechDef Mech { get; private set; }

        public InventoryOperationState(Queue<IChange> start_changes, MechDef mech)
        {
            done_changes = new List<IInvChange>();
            Mech = mech;
            Inventory = mech.Inventory.ToInvItems().ToList();

            pending_changes = start_changes;

        }

        public void DoChanges()
        {
            foreach (var change in pending_changes)
            {
                if (change is IInvChange iichange)
                    iichange.PreviewApply(this);
            }

            while (pending_changes.Count > 0)
            {
                var change = pending_changes.Dequeue();
                if (change is IAdjustChange adj && pending_changes.Any(i => i is IAdjustChange adj2 && adj2.ChangeID == adj.ChangeID))
                    continue;

                change.AdjustChange(this);

                if (change is IInvChange iichange)
                {
                    done_changes.Add(iichange);
                    if(iichange is IOptimizableChange ioc)
                        ioc.DoOptimization(done_changes);
                }
            }
        }

        public IEnumerable<IInvChange> GetResults()
        {
            return done_changes;
        }

        public void AddChange(IChange change)
        {
            pending_changes.Enqueue(change);
            if(change is IInvChange iichange)
                iichange.PreviewApply(this);
        }
    }
}