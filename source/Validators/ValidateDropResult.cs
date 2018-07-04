using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using UnityEngine;

namespace CustomComponents
{
    public enum ValidateDropStatus
    {
        Continue, Handled
    }

    public interface IValidateDropResult
    {
        ValidateDropStatus Status { get; } // not really necessary, but nice for semantics
    }


    public interface IChange
    {
        void DoChange(MechLabHelper mechLab, LocationHelper loc);
    }

    public abstract class SlotChange : IChange
    {
        public ChassisLocations location;
        public MechLabItemSlotElement item;

        public abstract void DoChange(MechLabHelper mechLab, LocationHelper loc);
    }


    public class AddChange : SlotChange
    {
        public override void DoChange(MechLabHelper mechLab, LocationHelper loc)
        {
            var widget = location == loc.widget.loadout.Location ? loc.widget : mechLab.GetLocationWidget(location);


            if (widget == null)
                return;

            widget.OnAddItem(item, false);
        }

        public AddChange(ChassisLocations location, MechLabItemSlotElement item)
        {
            this.location = location;
            this.item = item;
        }
    }

    public class RemoveChange : SlotChange
    {
        public override void DoChange(MechLabHelper mechLab, LocationHelper loc)
        {
            var widget = location == loc.widget.loadout.Location ? loc.widget : mechLab.GetLocationWidget(location);
            if (widget == null)
                return;

            widget.OnRemoveItem(item, true);
            if (item.ComponentRef.Def is IDefault)
                GameObject.Destroy(item.gameObject);
            else
                mechLab.MechLab.ForceItemDrop(item);
        }

        public RemoveChange(ChassisLocations location, MechLabItemSlotElement item)
        {
            this.location = location;
            this.item = item;
        }
    }



    public class ValidateDropChange : IValidateDropResult
    {
        public ValidateDropStatus Status => ValidateDropStatus.Continue;

        public List<IChange> Changes = new List<IChange>();

        protected ValidateDropChange()
        {
        }

        public static ValidateDropChange AddOrCreate(IValidateDropResult old_result, IChange change)
        {
            if(!(old_result is ValidateDropChange new_result))
                new_result = new ValidateDropChange();
            new_result.Changes.Add(change);
            return new_result;
        }

    }

    public class ValidateDropHandled : IValidateDropResult
    {
        public ValidateDropStatus Status => ValidateDropStatus.Handled;
    }

    /// <summary>
    /// special behaviour for "upgrede kit" items
    /// </summary>
    public class ValidateDropRemoveDragItem : ValidateDropHandled
    {
        public bool ShowMessage = false;
        public string Message = "";

        public ValidateDropRemoveDragItem()
        {

        }

        public ValidateDropRemoveDragItem(string message)
        {
            ShowMessage = true;
            Message = message;
        }
    }

    public class ValidateDropError : ValidateDropHandled
    {
        public string ErrorMessage { get; }

        public ValidateDropError(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
