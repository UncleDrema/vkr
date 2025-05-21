using Game.MapGraph.Components;
using Game.MapGraph.Requests;
using Scellecs.Morpeh;
using UnityEngine;

namespace Game.MapGraph.Entities
{
    public class GraphEntity : HierarchyCodeUniversalProvider
    {
        protected override void RegisterTypes()
        {
            RegisterType<InitializeGraphRequest>();
        }
        
#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (Entity == default)
                return;
            
            ref var cGraph = ref Entity.GetComponent<GraphComponent>();
            
            foreach (var vertex in cGraph.Vertices)
            {
                ref var cVertex = ref vertex.GetComponent<GraphVertexComponent>();
                var neighbors = cVertex.Neighbors;
                
                foreach (var neighbor in neighbors)
                {
                    var neighborPosition = neighbor.GetComponent<GraphVertexComponent>().Position;
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(cVertex.Position, neighborPosition);
                }
                
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(cVertex.Position, 0.25f);
                // draw cube with height = danger
                Gizmos.DrawCube(cVertex.Position, new Vector3(0.1f, cVertex.Threat, 0.1f));
            }
        }
#endif
    }
}