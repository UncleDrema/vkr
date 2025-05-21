using Game.SceneManagement.Api;
using UnityEngine;
using VContainer;

namespace Game.GameStartup
{
    public class GameStartup : MonoBehaviour
    {
        private ISceneLoader _sceneLoader;
        
        [Inject]
        private void Construct(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }
        
        private async void Start()
        {
            await _sceneLoader.LoadGameScene();
        }
    }
}