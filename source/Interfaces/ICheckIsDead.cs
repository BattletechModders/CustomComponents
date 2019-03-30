using BattleTech;

namespace CustomComponents
{
    public interface ICheckIsDead
    {
        bool IsActorDestroyed(MechComponent component, AbstractActor actor);
    }
}