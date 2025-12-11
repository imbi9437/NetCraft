using Photon.Pun;
using UnityEngine;

namespace _Project.Script.Manager
{
    /// <summary>
    /// 씬 전환 관련 로직을 담당하는 클래스
    /// PUN2 씬 동기화와 SceneController 연동 처리
    /// </summary>
    public class SceneTransitionHandler
    {
        private static string _targetGameScene = "";

        /// <summary>
        /// 게임 씬 전환 시작 (로딩 화면 포함)
        /// </summary>
        /// <param name="sceneName">전환할 씬 이름</param>
        /// <returns>전환 성공 여부</returns>
        public bool StartGameSceneTransition(string sceneName)
        {
            try
            {
                Debug.Log($"[SceneTransitionHandler] PUN2 멀티플레이어 씬 전환 준비: {sceneName}");

                // 목표 씬 저장 (Loading.cs에서 사용)
                _targetGameScene = sceneName;

                // 씬 동기화 활성화
                PhotonNetwork.AutomaticallySyncScene = true;

                // 로딩 씬으로 이동 (SceneController 활용)
                return ShowLoadingUI();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SceneTransitionHandler] 게임 시작 중 오류: {ex.Message}");
                // 오류 발생 시 직접 PUN2 씬 전환
                return DirectSceneTransition(sceneName);
            }
        }

        /// <summary>
        /// 로딩 UI 표시 (SceneController 활용)
        /// </summary>
        /// <returns>성공 여부</returns>
        private bool ShowLoadingUI()
        {
            try
            {
                // SceneController를 통해 로딩 씬 표시
                if (SceneController.Instance != null)
                {
                    Debug.Log("[SceneTransitionHandler] SceneController를 통해 로딩 UI 표시");

                    // 로딩 씬으로 이동 (목표 씬은 _targetGameScene에 저장됨)
                    SceneController.Instance.ChangeScene("03.Loading");
                    return true;
                }
                else
                {
                    Debug.LogWarning("[SceneTransitionHandler] SceneController를 찾을 수 없습니다.");
                    // SceneController가 없으면 직접 PUN2 씬 전환
                    return DirectSceneTransition(_targetGameScene);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SceneTransitionHandler] 로딩 UI 표시 중 오류: {ex.Message}");
                // 오류 발생 시 직접 PUN2 씬 전환
                return DirectSceneTransition(_targetGameScene);
            }
        }

        /// <summary>
        /// 직접 씬 전환 (PUN2 LoadLevel 사용)
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        /// <returns>성공 여부</returns>
        private bool DirectSceneTransition(string sceneName)
        {
            try
            {
                Debug.Log($"[SceneTransitionHandler] 직접 씬 전환: {sceneName}");

                // 씬 동기화 활성화
                PhotonNetwork.AutomaticallySyncScene = true;

                // PUN2를 통한 씬 전환
                PhotonNetwork.LoadLevel(sceneName);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SceneTransitionHandler] 직접 씬 전환 실패: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// PUN2 게임 씬 전환 (Loading.cs에서 호출)
        /// </summary>
        public static void LoadGameScene()
        {
            if (!string.IsNullOrEmpty(_targetGameScene))
            {
                Debug.Log($"[SceneTransitionHandler] PUN2 게임 씬 전환: {_targetGameScene}");

                // 중요: 모든 클라이언트가 동일한 씬을 로드하도록 설정
                PhotonNetwork.AutomaticallySyncScene = true;

                PhotonNetwork.LoadLevel(_targetGameScene);
                _targetGameScene = ""; // 사용 후 초기화
            }
            else
            {
                Debug.LogError("[SceneTransitionHandler] 목표 게임 씬이 설정되지 않았습니다.");
            }
        }

        /// <summary>
        /// 로비로 돌아가기
        /// </summary>
        public void ReturnToLobby()
        {
            Debug.Log("[SceneTransitionHandler] 인게임에서 로비로 돌아가기");

            // PUN2를 통해 방 나가기 (로비로 자동 이동)
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                Debug.LogWarning("[SceneTransitionHandler] 방에 입장하지 않은 상태입니다.");
            }
        }

        /// <summary>
        /// 메인 메뉴로 돌아가기
        /// </summary>
        public bool ReturnToMainMenu()
        {
            Debug.Log("[SceneTransitionHandler] 인게임에서 메인 메뉴로 돌아가기");

            try
            {
                // PUN2 연결 해제
                if (PhotonNetwork.IsConnected)
                {
                    PhotonNetwork.Disconnect();
                }

                // SceneController를 통해 메인 메뉴로 이동
                if (SceneController.Instance != null)
                {
                    SceneController.Instance.ChangeScene("02.Main");
                    return true;
                }
                else
                {
                    Debug.LogError("[SceneTransitionHandler] SceneController를 찾을 수 없습니다.");
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SceneTransitionHandler] 메인 메뉴로 돌아가기 중 오류: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 씬 동기화 활성화
        /// </summary>
        public void EnableSceneSynchronization()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            Debug.Log("[SceneTransitionHandler] 씬 동기화 활성화");
        }

        /// <summary>
        /// 씬 동기화 비활성화
        /// </summary>
        public void DisableSceneSynchronization()
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            Debug.Log("[SceneTransitionHandler] 씬 동기화 비활성화");
        }

        /// <summary>
        /// 현재 목표 씬 가져오기
        /// </summary>
        /// <returns>목표 씬 이름</returns>
        public static string GetTargetGameScene()
        {
            return _targetGameScene;
        }

        /// <summary>
        /// 목표 씬 초기화
        /// </summary>
        public static void ClearTargetGameScene()
        {
            _targetGameScene = "";
        }
    }
}
