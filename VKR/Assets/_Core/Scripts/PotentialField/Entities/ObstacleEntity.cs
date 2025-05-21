using Game.PotentialField.Components;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Transform.Components;
using TriInspector;
using UnityEngine;

namespace Game.PotentialField.Entities
{
    public class ObstacleEntity : HierarchyCodeUniversalProvider
    {
        public BoxCollider Collider;
        
        protected override void RegisterTypes()
        {
            RegisterType<TransformComponent>();
            RegisterType<ObstacleComponent>();
        }

        [Button, ShowInEditMode]
        public void UpdateFromCollider()
        {
            Debug.Log($"Starting update with {serializedComponents.Length} components");
            for (var i = 0; i < this.serializedComponents.Length; i++) {
                var component = this.serializedComponents[i];
                Debug.Log($"Updating {component.GetType()}");
                if (component is ObstacleComponent)
                {
                    var cObstacle = (ObstacleComponent)component;
                    cObstacle.Bounds.extents = Collider.bounds.extents;
                    serializedComponents[i] = cObstacle;
                }
            }
        }
    }
}