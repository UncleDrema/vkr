using System;

namespace Game.MapGraph
{
    [Serializable]
    public struct Edge
    {
        public int a;
        public int b;
        
        public Edge(int a, int b)
        {
            this.a = a;
            this.b = b;
        }
    }
}