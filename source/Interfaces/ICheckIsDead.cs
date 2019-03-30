using BattleTech;

namespace CustomComponents
{
    public interface ICheckIsDead
    {
        bool IsMechDestroyed(MechComponentRef item, Mech mech);
        bool IsVechicleDestroyed(VehicleComponentRef item, Vehicle mech);
        bool IsTurretDestroyed(TurretComponentRef item, Turret mech);
    }
}