using BattleTech;

namespace CustomComponents
{
    internal class IsDestroyed
    {
        internal static void IsDestroyedChecks(AbstractActor actor, ref bool __result)
        {
            Control.LogDebug(DType.IsDestroyed, $"Check if dead {actor.DisplayName}{actor.UnitName}");

            if (__result)
            {
                Control.LogDebug(DType.IsDestroyed, $"- Vanila destroyed");
                return;
            }

            if (IsActorDestroyed(actor))
            {
                __result = true;
            }
        }

        internal static bool IsActorDestroyed(AbstractActor actor)
        {
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