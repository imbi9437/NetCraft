using UnityEngine;

namespace _Project.Script.Extensions
{
    public static partial class Extensions
    {
        private const float SceneProgress = 0.9f;

        public static bool WaitUntilSceneLoaded(this AsyncOperation operation)
        {
            return operation.progress >= SceneProgress;
        }
    }
}
