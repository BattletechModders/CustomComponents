using BattleTech;

namespace CustomComponents
{
    public interface IOnInstalled
    {
        void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech);
    }
}