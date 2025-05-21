using Game.Common.Components;
using Scellecs.Morpeh;

namespace Game.PotentialField.Old
{
    public class LineFieldSourceEntity : HierarchyCodeUniversalProvider
    {
        protected override void RegisterTypes()
        {
            RegisterType<GameObjectComponent>();
            RegisterType<LineFieldSourceComponent>();
        }

        protected override void Initialize()
        {
            base.Initialize();
            var cSource = Entity.GetComponent<LineFieldSourceComponent>();
            cSource.DecaySpeed = 0.2f;
        }
    }
}