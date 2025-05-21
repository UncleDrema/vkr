using Cysharp.Threading.Tasks;
using Game.SceneManagement.Api;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Game.SceneManagement.Core
{
    public class SceneLoader : ISceneLoader
    {
        private readonly LifetimeScope _currentScope;
        private readonly SceneRepository _sceneRepository;

        public bool IsGameLoaded => IsSceneLoaded(_sceneRepository.GameScenePath);

        public SceneLoader(LifetimeScope currentScope, SceneRepository sceneRepository)
        {
            _currentScope = currentScope;
            _sceneRepository = sceneRepository;
        }
        
        public async UniTask LoadGameScene()
        {
            await LoadSceneAsync(_sceneRepository.GameScenePath, true);
        }
        
         public async UniTask LoadScene(string scenePath, bool setActive = true)
        {
            await LoadSceneAsync(scenePath, setActive);
        }

        public async UniTask UnloadScene(string scenePath)
        {
            await UnLoadSceneAsync(scenePath);
        }

        public async UniTask UnloadActiveScene()
        {
            await UniTask.DelayFrame(5);
            await UnLoadActiveSceneAsync();
        }

        public async UniTask UnloadActiveSceneNextFrame()
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            await UnLoadActiveSceneAsync();
        }

        private async UniTask LoadSceneAsync(string scenePath, bool setActive)
        {
            using (LifetimeScope.EnqueueParent(_currentScope))
            {
                var handler =  SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
                handler.completed += _ =>
                {
                    var loadedScene = SceneManager.GetSceneByPath(scenePath);
                    if (setActive)
                    {
                        SceneManager.SetActiveScene(loadedScene);
                    }
                };

                await handler;
            }
        }

        private async UniTask UnLoadSceneAsync(string scenePath)
        {
            using (LifetimeScope.EnqueueParent(_currentScope))
            {
                var handler = SceneManager.UnloadSceneAsync(scenePath);
                await handler;
            }
        }

        private async UniTask UnLoadActiveSceneAsync()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            if (activeScene.path == _sceneRepository.BootScenePath)
                return;

            using (LifetimeScope.EnqueueParent(_currentScope))
            {
                var handler = SceneManager.UnloadSceneAsync(activeScene);
                await handler;
            }
        }

        private bool IsSceneLoaded(string path)
        {
            return SceneManager.GetSceneByPath(path).isLoaded;
        }
    }
}