using System.Collections.Generic;
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
            RegisterType<InitializeGraphFromUnityRequest>();
        }
        
#if UNITY_EDITOR
        private Dictionary<int, Color> _zoneColors = new Dictionary<int, Color>()
        {
            {0, Color.red},
            {1, Color.green},
            {2, Color.blue},
            {3, Color.yellow},
            {4, Color.cyan},
            {5, Color.magenta},
            {6, Color.white},
            {7, new Color(1f, 0.5f, 0f)},
            {8, new Color(0.5f, 0f, 1f)},
            {9, new Color(0.5f, 0.5f, 0f)},
            {10, new Color(0f, 0.5f, 0.5f)},
            {11, new Color(0.5f, 0f, 0.5f)},
        };
        
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
                    Gizmos.color = Color.black;
                    Gizmos.DrawLine(cVertex.Position, neighborPosition);
                }

                if (cVertex.Zone != default)
                {
                    ref var cZone = ref cVertex.Zone.GetComponent<ZoneComponent>();
                    if (_zoneColors.TryGetValue(cZone.ZoneId, out var color))
                    {
                        Gizmos.color = color;
                    }
                    else
                    {
                        Gizmos.color = Color.white;
                    }
                }
                else
                {
                    Gizmos.color = Color.white;
                }
                
                Gizmos.DrawSphere(cVertex.Position, 0.25f);
                // draw cube with height = danger
                Gizmos.DrawCube(cVertex.Position, new Vector3(0.1f, cVertex.Threat, 0.1f));
            }
        }
#endif
    }
}