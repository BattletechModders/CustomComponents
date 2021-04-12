﻿using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public abstract class SlotChange : IApplyChange
    {
        public ChassisLocations location;
        public MechLabItemSlotElement item;

        public abstract void DoChange();
        public abstract void PreviewChange(List<SlotInvItem> inventory);

        public abstract bool DoAdjust(Queue<IChange> changes, List<SlotInvItem> inventory);
    }
}