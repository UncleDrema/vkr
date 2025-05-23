using System.Collections.Generic;
using UnityEngine;

namespace Game.MapGraph
{
    [ExecuteInEditMode]
    public class LevelGraph : MonoBehaviour
    {
        public List<Vector3> nodes = new List<Vector3>();
        public List<Edge> edges = new List<Edge>();

        public float handleSize = 0.5f;
        public Color nodeColor = Color.cyan;
        public Color edgeColor = Color.white;
        public Color selectedColor = Color.yellow;
    }
}