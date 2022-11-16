using System.Collections.Generic;
using System.Linq;
using BattleTech;
using CustomComponents.Changes;

namespace CustomComponents;

public class InventoryOperationState
{
    private List<IChange_Apply > done_changes;
    private Queue<IChange > pending_changes;
    public List<InvItem> Inventory { get; private set; }

    public MechDef Mech { get; private set; }

    public InventoryOperationState(Queue<IChange> start_changes, MechDef mech, IEnumerable<InvItem> inventory = null)
    {
        done_changes = new();
        Mech = mech;
        Inventory = (inventory ?? mech.Inventory.ToInvItems()).ToList();

        pending_changes = new();
        foreach (var change in start_changes)
        {
            change.Initial = true;
            pending_changes.Enqueue(change);
        }
    }

    public void DoChanges()
    {
        Log.InventoryOperations.Trace?.Log($"DoChanges for {Mech.Description.Id}");
        Log.InventoryOperations.Trace?.Log("- Initial preview");

        foreach (var change in pending_changes)
        {
            if (change is IChange_Apply iichange)
            {
                Log.InventoryOperations.Trace?.Log($"-- {change}");
                iichange.PreviewApply(this);
            }
        }

        Log.InventoryOperations.Trace?.Log("- iteration");
        while (pending_changes.Count > 0)
        {
            var change = pending_changes.Dequeue();
            if (change is IChange_Adjust adj &&
                pending_changes.Any(i => i is IChange_Adjust adj2 && adj2.ChangeID == adj.ChangeID))
            {
                Log.InventoryOperations.Trace?.Log($"-- Skip {change}");
                continue;
            }

            Log.InventoryOperations.Trace?.Log($"-- Adjust {change}");
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
        Log.InventoryOperations.Trace?.Log($"--- {change}, ");

        change.Initial = false;
        pending_changes.Enqueue(change);
        if(change is IChange_Apply iichange)
            iichange.PreviewApply(this);
    }

    public void ApplyInventory()
    {
        Log.ComponentInstall.Trace?.Log("ApplyInventory");
        var inv = Mech.Inventory.ToList();
        foreach (var change in done_changes)
        {
            Log.ComponentInstall.Trace?.Log($"- {change}");
            change.ApplyToInventory(Mech, inv);
        }
        Mech.SetInventory(inv.ToArray());
    }

    public void ApplyMechlab()
    {
        Log.ComponentInstall.Trace?.Log("ApplyMechlab");
        foreach (var change in done_changes)
        {
            Log.ComponentInstall.Trace?.Log($"- {change}");
            change.ApplyToMechlab();
        }

        MechLabHelper.CurrentMechLab.RefreshHardpoints();
    }
}