using Cysharp.Threading.Tasks;

namespace Game.SceneManagement.Api
{
    public interface ISceneLoader
    {
        public bool IsGameLoaded { get; }

        UniTask LoadGameScene();
        UniTask LoadScene(string scenePath, bool isActive = true);
        UniTask UnloadScene(string scenePath);
        UniTask UnloadActiveScene();
        UniTask UnloadActiveSceneNextFrame();
        UniTask ReloadActiveScene();
        UniTask UnloadActiveThenLoadScene(string scenePath, bool isActive = true);
    }
}