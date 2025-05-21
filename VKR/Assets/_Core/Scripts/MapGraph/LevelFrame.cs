using System.Collections.Generic;
using UnityEngine;

namespace Game.MapGraph
{
    public class LevelFrame : MonoBehaviour
    {
        public List<Vector3> points = new List<Vector3> {
            new Vector3(-5, 0, -5),
            new Vector3( 5, 0, -5),
            new Vector3( 5, 0,  5),
            new Vector3(-5, 0,  5)
        };
        
        public float handleSize = 0.5f;
        public Color lineColor   = Color.green;
        public Color pointColor  = Color.yellow;
    }
}