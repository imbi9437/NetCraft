using _Project.Script.EventStruct;
using _Project.Script.Character.Network;
using _Project.Script.Manager;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace _Project.Script.UI
{
    /// <summary>
    /// 네트워크 플레이어 상호작용 UI 패널
    /// 아이템 공유, 단순한 상호작용을 시각적으로 관리
    /// </summary>
    public class NetworkInteractionPanel : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Transform playerList;
        [SerializeField] private GameObject playerEntryPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;

        [Header("상호작용 버튼")]
        [SerializeField] private Button shareItemButton;
        [SerializeField] private Button attackPlayerButton;

        [Header("설정")]
        [SerializeField] private float interactionRange = 5f;

        // UI 상태
        private Dictionary<int, GameObject> playerEntries = new Dictionary<int, GameObject>();

        // 선택된 플레이어
        private int selectedPlayerActorNumber = -1;

        private void Start()
        {
            // 이벤트 구독
            EventHub.Instance.RegisterEvent<PunEvents.OnItemSharedEvent>(OnItemShared);
            EventHub.Instance.RegisterEvent<PunEvents.OnPlayerDamagedEvent>(OnPlayerDamaged);

            // UI 초기화
            InitializeUI();
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (EventHub.Instance != null)
            {
                EventHub.Instance.UnregisterEvent<PunEvents.OnItemSharedEvent>(OnItemShared);
                EventHub.Instance.UnregisterEvent<PunEvents.OnPlayerDamagedEvent>(OnPlayerDamaged);
            }
        }

        #region UI 초기화

        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 버튼 이벤트 연결
            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);
            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshPlayerList);
            if (shareItemButton != null)
                shareItemButton.onClick.AddListener(OnShareItemClicked);
            if (attackPlayerButton != null)
                attackPlayerButton.onClick.AddListener(OnAttackPlayerClicked);

            // 초기 플레이어 목록 생성
            RefreshPlayerList();
        }

        #endregion

        #region 이벤트 처리

        /// <summary>
        /// 아이템 공유 이벤트 처리
        /// </summary>
        private void OnItemShared(PunEvents.OnItemSharedEvent shareEvent)
        {
            Debug.Log($"[NetworkInteractionPanel] 아이템 공유: {shareEvent.item.itemData.name} x{shareEvent.item.count}");
        }

        /// <summary>
        /// 플레이어 데미지 이벤트 처리 (단순한 PvP)
        /// </summary>
        private void OnPlayerDamaged(PunEvents.OnPlayerDamagedEvent damageEvent)
        {
            Debug.Log($"[NetworkInteractionPanel] 플레이어 {damageEvent.targetActorNumber} 데미지: {damageEvent.damage}");
        }

        #endregion

        #region 플레이어 목록 관리

        /// <summary>
        /// 플레이어 목록 새로고침
        /// </summary>
        private void RefreshPlayerList()
        {
            // 기존 엔트리 제거
            foreach (var entry in playerEntries.Values)
            {
                if (entry != null)
                    Destroy(entry);
            }
            playerEntries.Clear();

            // 현재 룸의 플레이어들 추가
            if (PhotonNetwork.CurrentRoom != null)
            {
                foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                {
                    if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        AddPlayerEntry(player.ActorNumber, player.NickName);
                    }
                }
            }
        }

        /// <summary>
        /// 플레이어 엔트리 추가
        /// </summary>
        private void AddPlayerEntry(int actorNumber, string playerName)
        {
            if (playerList == null || playerEntryPrefab == null) return;

            GameObject entry = Instantiate(playerEntryPrefab, playerList);
            entry.name = $"PlayerEntry_{actorNumber}";

            // 플레이어 정보 설정
            SetupPlayerEntry(entry, actorNumber, playerName);

            playerEntries[actorNumber] = entry;
        }

        /// <summary>
        /// 플레이어 엔트리 설정
        /// </summary>
        private void SetupPlayerEntry(GameObject entry, int actorNumber, string playerName)
        {
            // 플레이어 이름
            Text nameText = entry.transform.Find("Name_Text")?.GetComponent<Text>();
            if (nameText != null)
            {
                nameText.text = playerName;
            }

            // 선택 버튼
            Button selectButton = entry.GetComponentInChildren<Button>();
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(() => SelectPlayer(actorNumber));
            }
        }

        /// <summary>
        /// 플레이어 선택
        /// </summary>
        private void SelectPlayer(int actorNumber)
        {
            selectedPlayerActorNumber = actorNumber;
            UpdateInteractionButtons();
            Debug.Log($"[NetworkInteractionPanel] 플레이어 {actorNumber} 선택됨");
        }

        #endregion

        #region 상호작용 버튼 관리

        /// <summary>
        /// 상호작용 버튼 업데이트
        /// </summary>
        private void UpdateInteractionButtons()
        {
            bool hasSelectedPlayer = selectedPlayerActorNumber != -1;

            // 아이템 공유 버튼 (드롭 방식으로 변경)
            if (shareItemButton != null)
            {
                shareItemButton.interactable = hasSelectedPlayer;
                shareItemButton.GetComponentInChildren<Text>().text = "아이템 드롭";
            }

            // 공격 버튼 (단순한 PvP)
            if (attackPlayerButton != null)
            {
                attackPlayerButton.interactable = hasSelectedPlayer;
            }
        }

        #endregion

        #region 상호작용 처리

        /// <summary>
        /// 아이템 공유 버튼 클릭 (드롭 방식)
        /// </summary>
        private void OnShareItemClicked()
        {
            if (selectedPlayerActorNumber == -1) return;

            Debug.Log($"[NetworkInteractionPanel] 아이템 공유는 이제 바닥에 드롭하는 방식입니다");
            // 실제 아이템 드롭은 NetworkPlayer.cs의 DropItemToWorld() 메서드 사용
        }

        /// <summary>
        /// 플레이어 공격 버튼 클릭 (단순한 PvP)
        /// </summary>
        private void OnAttackPlayerClicked()
        {
            if (selectedPlayerActorNumber == -1) return;

            if (NetworkPlayerInteraction.Instance != null)
            {
                NetworkPlayerInteraction.Instance.AttackPlayer(selectedPlayerActorNumber);
                Debug.Log($"[NetworkInteractionPanel] 플레이어 {selectedPlayerActorNumber} 공격 실행");
            }
        }

        #endregion

        #region 패널 관리

        /// <summary>
        /// 패널 닫기
        /// </summary>
        private void ClosePanel()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 패널 열기
        /// </summary>
        public void OpenPanel()
        {
            gameObject.SetActive(true);
            RefreshPlayerList();
        }

        #endregion
    }
}