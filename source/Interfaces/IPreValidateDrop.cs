using BattleTech.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents
{
    public interface IPreValidateDrop
    {
        string PreValidateDrop(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab);
    }

    public delegate string PreValidateDropDelegate(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab);

}
