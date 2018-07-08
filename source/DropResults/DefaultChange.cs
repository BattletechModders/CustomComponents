using BattleTech;

namespace CustomComponents
{
    public abstract class DefaultChange : IChange
    {
        public MechComponentRef Main;
        public string DefaultID;
        public ChassisLocations Location;

        public abstract void DoChange(MechLabHelper mechLab, LocationHelper loc);
    }

    public class DefautAddWith : DefaultChange
    {
        public override void DoChange(MechLabHelper mechLab, LocationHelper loc)
        {
            DefaultHelper.DefaultAddWith(Main, DefaultID, Location);
        }
    }

    public class DefautRemoveWith : DefaultChange
    {
        public override void DoChange(MechLabHelper mechLab, LocationHelper loc)
        {
            DefaultHelper.DefaultRemoveWith(Main, DefaultID, Location);
        }
    }

    public class DefautReplaceBase : DefaultChange
    {
        public override void DoChange(MechLabHelper mechLab, LocationHelper loc)
        {
            DefaultHelper.DefaultReplaceBase(Main, DefaultID, Location);
        }
    }

    public class DefautReplaceDefault : DefaultChange
    {
        public override void DoChange(MechLabHelper mechLab, LocationHelper loc)
        {
            DefaultHelper.DefautReplaceDefault(Main, DefaultID, Location);
        }
    }
}