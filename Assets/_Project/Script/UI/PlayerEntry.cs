using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using _Project.Script.Manager;

namespace _Project.Script.UI
{
    /// <summary>
    /// 플레이어 목록 아이템 관리 (방 내부)
    /// 협업 시 플레이어 아이템 UI 로직을 분리하여 관리
    /// </summary>
    public class PlayerEntry : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Text playerNameText;
        [SerializeField] private Toggle readyToggle;
        [SerializeField] private Image playerAvatar;
        [SerializeField] private GameObject readyLabel;

        [Header("레디 상태 이미지")]
        [SerializeField] private Image readyStatusImage;
        [SerializeField] private Sprite readySprite;
        [SerializeField] private Sprite waitingSprite;


        private Player playerInfo;
        private bool isReady = false;

        /// <summary>
        /// 내 플레이어인지 확인
        /// </summary>
        public bool IsMine => playerInfo == PhotonNetwork.LocalPlayer;

        /// <summary>
        /// 플레이어 정보 설정
        /// </summary>
        public void SetupPlayer(Player player)
        {
            this.playerInfo = player;

            // UI 업데이트
            if (playerNameText != null)
            {
                string displayName = string.IsNullOrEmpty(player.NickName) ? "Unknown" : player.NickName;
                playerNameText.text = displayName;

                // 방장 표시 추가
                if (player.IsMasterClient)
                {
                    playerNameText.text = $"{displayName} (방장)";
                }
            }

            // 커스텀 프로퍼티 초기화
            InitializeCustomProperties();

            // 방장 여부에 따른 UI 설정
            if (player.IsMasterClient)
            {
                // 방장은 레디 토글 비활성화
                if (readyToggle != null)
                {
                    readyToggle.interactable = false;
                    readyToggle.gameObject.SetActive(false); // 방장은 토글 숨김
                }

                // 방장 표시
                SetMasterClientDisplay();
            }
            else
            {
                // 일반 플레이어는 레디 토글 활성화 (내 플레이어만)
                if (readyToggle != null)
                {
                    readyToggle.gameObject.SetActive(true);
                    if (IsMine)
                    {
                        readyToggle.interactable = true;
                        readyToggle.onValueChanged.AddListener(OnReadyToggleChanged);
                    }
                    else
                    {
                        readyToggle.interactable = false;
                    }
                }

                // 준비 상태 표시
                UpdateReadyStatus();
            }

            // 아바타 설정 (추후 구현)
            if (playerAvatar != null)
            {
                // 여기에 아바타 이미지 설정 로직 추가
            }
        }

        /// <summary>
        /// 커스텀 프로퍼티 초기화 (트래픽 최적화)
        /// </summary>
        private void InitializeCustomProperties()
        {
            if (IsMine)
            {
                Hashtable cp = PhotonNetwork.LocalPlayer.CustomProperties;
                bool needsUpdate = false;

                // 레디 상태 초기화
                if (!cp.ContainsKey("Ready"))
                {
                    cp.Add("Ready", false);
                    needsUpdate = true;
                }

                // 캐릭터 선택 초기화 (추후 확장용)
                if (!cp.ContainsKey("CharSel"))
                {
                    cp.Add("CharSel", 0);
                    needsUpdate = true;
                }

                // 한 번에 모든 속성을 업데이트 (트래픽 절약)
                if (needsUpdate)
                {
                    PhotonNetwork.LocalPlayer.SetCustomProperties(cp);
                    Debug.Log("[PlayerEntry] 플레이어 속성 초기화 완료");
                }
            }
        }

        /// <summary>
        /// 레디 토글 변경
        /// </summary>
        private void OnReadyToggleChanged(bool isReady)
        {
            if (playerInfo == null || playerInfo.IsMasterClient || !IsMine) return;

            // 네트워크 연결 상태 확인
            if (!(PhotonNetwork.IsConnected && PhotonNetwork.InRoom))
            {
                Debug.LogWarning("[PlayerEntry] 네트워크 연결이 불안정하여 레디 상태 변경을 취소합니다.");
                // 토글 상태를 원래대로 되돌리기
                RevertToggleState();
                return;
            }

            // 준비 상태 업데이트
            this.isReady = isReady;

            try
            {
                // 플레이어 속성에 준비 상태 저장 (NetworkExample 방식 사용)
                Hashtable cp = PhotonNetwork.LocalPlayer.CustomProperties;
                cp["Ready"] = isReady;
                PhotonNetwork.LocalPlayer.SetCustomProperties(cp);

                Debug.Log($"[PlayerEntry] 준비 상태 변경: {isReady}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[PlayerEntry] 레디 상태 변경 중 오류 발생: {ex.Message}");
                // 오류 발생 시 토글 상태를 원래대로 되돌리기
                this.isReady = !isReady; // 내부 상태도 되돌리기
                RevertToggleState();
                return;
            }

            // 즉시 UI 업데이트
            UpdateReadyStatusDisplay();
        }

        /// <summary>
        /// 준비 상태 업데이트
        /// </summary>
        private void UpdateReadyStatus()
        {
            if (playerInfo == null) return;

            // 플레이어 속성에서 준비 상태 확인
            bool newReadyState = false;
            if (playerInfo.CustomProperties.ContainsKey("Ready"))
            {
                newReadyState = (bool)playerInfo.CustomProperties["Ready"];
            }

            // 상태가 변경된 경우에만 업데이트
            if (isReady != newReadyState)
            {
                isReady = newReadyState;
                Debug.Log($"[PlayerEntry] {playerInfo.NickName} 레디 상태 업데이트: {isReady}");

                // 토글 상태 업데이트 (이벤트 발생하지 않도록)
                UpdateToggleWithoutEvent();

                // 레디 상태 표시 업데이트
                UpdateReadyStatusDisplay();
            }
            else
            {
                Debug.Log($"[PlayerEntry] {playerInfo.NickName} 레디 상태 변경 없음: {isReady}");
            }
        }

        /// <summary>
        /// 이벤트 발생 없이 토글 상태 업데이트 (중복 코드 통합)
        /// </summary>
        private void UpdateToggleWithoutEvent(bool targetState = false)
        {
            if (readyToggle == null) return;

            // 이벤트 리스너 일시 제거
            readyToggle.onValueChanged.RemoveListener(OnReadyToggleChanged);

            // 토글 상태 업데이트 (기본값은 현재 isReady 상태)
            readyToggle.isOn = targetState == false ? this.isReady : targetState;

            // 이벤트 리스너 다시 추가 (내 플레이어만)
            if (IsMine && !playerInfo.IsMasterClient)
            {
                readyToggle.onValueChanged.AddListener(OnReadyToggleChanged);
            }
        }

        /// <summary>
        /// 토글 상태를 현재 내부 상태로 되돌리기
        /// </summary>
        private void RevertToggleState()
        {
            UpdateToggleWithoutEvent(); // 현재 isReady 상태로 되돌리기
        }

        /// <summary>
        /// 레디 상태 표시 업데이트
        /// </summary>
        private void UpdateReadyStatusDisplay()
        {
            if (playerInfo == null) return;

            // 방장 표시
            if (playerInfo.IsMasterClient)
            {
                SetMasterClientDisplay();
                return;
            }

            // 레디 라벨 표시
            if (readyLabel != null)
            {
                readyLabel.SetActive(isReady);
            }

            // 레디 상태 이미지 업데이트
            if (readyStatusImage != null)
            {
                if (isReady)
                {
                    if (readySprite != null)
                        readyStatusImage.sprite = readySprite;
                    readyStatusImage.color = Color.green;
                }
                else
                {
                    if (waitingSprite != null)
                        readyStatusImage.sprite = waitingSprite;
                    readyStatusImage.color = Color.gray;
                }
            }
        }

        /// <summary>
        /// 방장 표시 설정
        /// </summary>
        private void SetMasterClientDisplay()
        {
            if (readyLabel != null)
            {
                readyLabel.SetActive(true);
                Text labelText = readyLabel.GetComponent<Text>();
                if (labelText != null)
                    labelText.text = "방장";
            }

            if (readyStatusImage != null)
            {
                readyStatusImage.sprite = null;
                readyStatusImage.color = Color.yellow;
            }
        }
        /// <summary>
        /// 플레이어 속성 업데이트 시 호출
        /// </summary>
        public void OnPlayerPropertiesUpdate()
        {
            Debug.Log($"[PlayerEntry] {playerInfo?.NickName} 속성 업데이트 호출");
            UpdateReadyStatus();
        }

        /// <summary>
        /// 플레이어의 레디 상태를 설정 (외부에서 호출)
        /// </summary>
        public void SetReadyState(bool isReady)
        {
            // 내 플레이어도 외부에서 상태 업데이트가 필요할 수 있음 (다른 클라이언트에서 변경)
            this.isReady = isReady;

            // 토글 상태도 업데이트 (이벤트 발생하지 않도록)
            UpdateToggleWithoutEvent();

            // 레디 상태 표시 업데이트
            UpdateReadyStatusDisplay();

            Debug.Log($"[PlayerEntry] {playerInfo?.NickName} 레디 상태 외부 설정: {isReady}");
        }

        /// <summary>
        /// 현재 플레이어의 준비 상태 반환
        /// </summary>
        public bool IsReady => isReady;

        /// <summary>
        /// 플레이어 정보 반환
        /// </summary>
        public Player PlayerInfo => playerInfo;
    }
}
