using BattleTech;

namespace CustomComponents
{
    internal class IsDestroyed
    {
        internal static bool IsActorDestroyed(AbstractActor actor)
        {
            Control.LogDebug(DType.IsDestroyed, $"Check if dead Nickname={actor.Nickname} UnitName={actor.UnitName}");
            if (actor.IsDead)
            {
                Control.LogDebug(DType.IsDestroyed, $"- Vanilla destroyed");
                return true;
            }

            foreach (var component in actor.allComponents)
            {
                var componentRef = component.baseComponentRef;

                if (componentRef.Is<ICheckIsDead>(out var d) && d.IsActorDestroyed(component, actor))
                {
                    Control.LogDebug(DType.IsDestroyed,
                        $"- Destroyed by IsActorDestroyed {componentRef.ComponentDefID} of {d.GetType()}");
                    return true;
                }

                if (Control.Settings.CheckCriticalComponent && componentRef.Def.CriticalComponent &&
                    component.DamageLevel == ComponentDamageLevel.Destroyed)
                {
                    Control.LogDebug(DType.IsDestroyed, $"- Destroyed by CriticalComponent {componentRef.ComponentDefID}");
                    return true;
                }
            }

            Control.LogDebug(DType.IsDestroyed, $"- not destroyed");
            return false;
        }
    }
}