using UnityEngine;
using UnityEngine.UI;
using _Project.Script.Manager;
using Cysharp.Threading.Tasks;
using _Project.Script.EventStruct;

namespace _Project.Script.UI
{
    /// <summary>
    /// 통합 UI 상태 표시 매니저
    /// 모든 UI 패널의 상태 메시지를 중앙에서 관리 (텍스트만 표시)
    /// </summary>
    public class StatusManager : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Text statusText;

        [Header("설정")]
        // 자동 숨김 기능 비활성화 - 메시지가 계속 표시됨

        private bool _isDisplaying = false;

        private void Start()
        {
            InitializeStatusManager();
        }

        /// <summary>
        /// 상태 매니저 초기화
        /// </summary>
        private void InitializeStatusManager()
        {
            // 이벤트 구독
            EventHub.Instance.RegisterEvent<PunEvents.OnUIStatusUpdateEvent>(OnStatusUpdate);

            // 초기 상태 설정
            if (statusText != null)
            {
                statusText.text = "";
                statusText.color = Color.white;
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제 (null 체크)
            if (EventHub.Instance != null)
            {
                EventHub.Instance.UnregisterEvent<PunEvents.OnUIStatusUpdateEvent>(OnStatusUpdate);
            }
        }

        /// <summary>
        /// 상태 업데이트 이벤트 처리
        /// </summary>
        private void OnStatusUpdate(PunEvents.OnUIStatusUpdateEvent evt)
        {
            Debug.Log($"[StatusManager] 상태 업데이트: {evt.message}");
            ShowStatus(evt.message, evt.color);
        }

        /// <summary>
        /// 상태 메시지 표시 (자동 숨김 없음)
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        /// <param name="color">텍스트 색상</param>
        public void ShowStatus(string message, Color color)
        {
            if (statusText == null) return;

            // 상태 표시 (텍스트만, 자동 숨김 없음)
            statusText.text = message;
            statusText.color = color;
        }

        /// <summary>
        /// 상태 메시지 즉시 숨김
        /// </summary>
        public void HideStatus()
        {
            if (statusText != null)
            {
                statusText.text = "";
            }
        }

        /// <summary>
        /// 상태 메시지 표시 (편의 메서드) - 자동 숨김 없음
        /// </summary>
        public static void ShowMessage(string message, Color color = default)
        {
            if (EventHub.Instance == null)
            {
                return;
            }

            if (color == default)
                color = Color.white;

            EventHub.Instance.RaiseEvent(new PunEvents.OnUIStatusUpdateEvent
            {
                message = message,
                color = color,
                displayTime = 0f // 자동 숨김 안함
            });
        }

        /// <summary>
        /// 성공 메시지 표시 - 자동 숨김 없음
        /// </summary>
        public static void ShowSuccess(string message)
        {
            ShowMessage(message, Color.green);
        }

        /// <summary>
        /// 오류 메시지 표시 - 자동 숨김 없음
        /// </summary>
        public static void ShowError(string message)
        {
            ShowMessage(message, Color.red);
        }

        /// <summary>
        /// 경고 메시지 표시 - 자동 숨김 없음
        /// </summary>
        public static void ShowWarning(string message)
        {
            ShowMessage(message, Color.yellow);
        }

        /// <summary>
        /// 정보 메시지 표시 - 자동 숨김 없음
        /// </summary>
        public static void ShowInfo(string message)
        {
            ShowMessage(message, Color.white);
        }
    }
}
