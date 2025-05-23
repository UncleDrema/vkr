using UnityEngine;

namespace Game.PotentialField
{
    [CreateAssetMenu(fileName = "AgentConfig", menuName = "Game/AgentConfig")]
    public class AgentConfig : ScriptableObject
    {
        [field: SerializeField]
        public GameObject AgentPrefab { get; private set; }
    }
}