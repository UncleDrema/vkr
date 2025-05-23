using Game.Common.Components;
using Game.PotentialField.Components;
using Scellecs.Morpeh;

namespace Game.PotentialField.Entities
{
    public class DynamicObstacleEntity : HierarchyCodeUniversalProvider
    {
        protected override void RegisterTypes()
        {
            RegisterType<DynamicObstacleComponent>();
            RegisterType<GameObjectComponent>();
            RegisterType<MapPositionComponent>();
        }
    }
}