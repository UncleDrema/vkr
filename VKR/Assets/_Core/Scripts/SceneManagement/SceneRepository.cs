using TriInspector;
using UnityEngine;

namespace Game.SceneManagement
{
    [CreateAssetMenu(menuName = "Scene Management/Create SceneRepository", fileName = "SceneRepository", order = 0)]
    public class SceneRepository : ScriptableObject
    {
        [field: SerializeField, Scene]
        public string BootScenePath { get; private set; }

        [field: SerializeField, Scene]
        public string GameScenePath { get; private set; }
    }
}