using Game.Common.Components;
using Scellecs.Morpeh;

namespace Game.PotentialField.Old
{
    public class PointFieldSourceEntity : HierarchyCodeUniversalProvider
    {
        protected override void RegisterTypes()
        {
            RegisterType<GameObjectComponent>();
            RegisterType<PointFieldSourceComponent>();
        }
        
        protected override void Initialize()
        {
            base.Initialize();
            var cSource = Entity.GetComponent<PointFieldSourceComponent>();
            cSource.DecaySpeed = 0.2f;
        }
    }
}