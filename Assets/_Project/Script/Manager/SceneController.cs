using System;
using _Project.Script.Extensions;
using _Project.Script.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Script.Manager
{
    // TODO : 포톤 LoadLevel 사용시 문제 발생 요지 있음 조사 필요
    // TODO : 씬 이동 매개변수가 string은 불안한 부분이 있음 추후 열거형 혹은 정적 데이터화 필요
    /// <summary>
    /// 씬 관리용 매니저
    /// </summary>
    [DefaultExecutionOrder(-99)]
    public class SceneController : MonoSingleton<SceneController>
    {
        private const float DefaultDelayTime = 1f;

        private string _targetSceneName;

        //씬 다이렉트 이동        
        public void ChangeScene(string sceneName) => LoadSceneAsync(sceneName);
        public void ChangeScene(string sceneName, float delay) => LoadSceneAsyncWithDelay(sceneName, delay);

        //씬 이동 (로딩 거치는 버젼)
        public void ChangeSceneWithLoading(string sceneName)
        {
            _targetSceneName = sceneName;
            LoadSceneAsync("03.Loading");
        }

        //설정된 씬으로 이동 (주로 로딩에서 목표한 씬 이동시 사용)
        public void LoadTargetScene() => LoadSceneAsync(_targetSceneName);
        public void LoadTargetScene(float delay) => LoadSceneAsyncWithDelay(_targetSceneName, delay);

        /// <summary>
        /// 비동기 씬 로딩
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        private async UniTask LoadSceneAsync(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            var token = this.GetCancellationTokenOnDestroy();
            await UniTask.WaitUntil(operation.WaitUntilSceneLoaded, cancellationToken: token);

            operation.allowSceneActivation = true;
        }

        /// <summary>
        /// 비동기 씬 로딩 (로딩 최소 시간 추가 버전)
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        /// <param name="delay">딜레이</param>
        private async UniTask LoadSceneAsyncWithDelay(string sceneName, float delay)
        {
            float t = Mathf.Max(delay, DefaultDelayTime);

            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            var token = this.GetCancellationTokenOnDestroy();
            var loadTask = UniTask.WaitUntil(operation.WaitUntilSceneLoaded, cancellationToken: token);
            var delayTask = UniTask.Delay(TimeSpan.FromSeconds(t), cancellationToken: token);

            await UniTask.WhenAll(loadTask, delayTask);

            operation.allowSceneActivation = true;
        }
    }
}
