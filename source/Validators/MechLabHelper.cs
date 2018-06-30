using BattleTech.UI;

namespace CustomComponents
{
    public class MechLabHelper
    {
        public MechLabPanel MechLab { get; private set; }

        public MechLabHelper(MechLabPanel mechLab)
        {
            MechLab = mechLab;
        }
    }
}