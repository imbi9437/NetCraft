using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Project.Script.Generic;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using _Project.Script.Data;
using _Project.Script.Character.Player;

namespace _Project.Script.UI
{
    /// <summary>
    /// 플레이어 상태 UI 패널
    /// 체력, 정신력, 배고픔, 수분 등을 원형 게이지로 표시
    /// </summary>
    public class PlayerStatusPanel : MonoBehaviour
    {
        [Header("플레이어 상태 UI - 원형 게이지")]
        [SerializeField] private CircularGauge healthGauge;
        [SerializeField] private CircularGauge sanityGauge; // 정신력
        [SerializeField] private CircularGauge hungerGauge; // 배고픔
        
        [Header("임계값 설정")]
        [SerializeField] private float dangerThreshold = 20f;

        private bool isInitialized = false;

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // 원형 게이지 초기화
            InitializeGauges();

            isInitialized = true;
            Debug.Log("[PlayerStatusPanel] UI 초기화 완료");
        }

        /// <summary>
        /// 원형 게이지들 초기화
        /// </summary>
        private void InitializeGauges()
        {
            // 라벨만 먼저 설정 (최대값은 PlayerData 받은 후 설정)
            if (healthGauge != null)
            {
                healthGauge.SetLabel("체력");
                healthGauge.SetMaxValue(100f); // 임시 기본값
                healthGauge.SetMinValue(0f);
                healthGauge.SetValueImmediate(100f);
            }

            if (sanityGauge != null)
            {
                sanityGauge.SetLabel("정신력");
                sanityGauge.SetMaxValue(100f); // 임시 기본값
                sanityGauge.SetMinValue(0f);
                sanityGauge.SetValueImmediate(100f);
            }

            if (hungerGauge != null)
            {
                hungerGauge.SetLabel("배고픔");
                hungerGauge.SetMaxValue(100f); // 임시 기본값
                hungerGauge.SetMinValue(0f);
                hungerGauge.SetValueImmediate(100f);
            }
        }

        /// <summary>
        /// 이벤트로부터 받은 최대값으로 게이지 업데이트
        /// </summary>
        private void UpdateGaugeMaxValuesFromEvent(EventStruct.UIEvents.PlayerStatusUpdateEvent evt)
        {
            // 최대값이 0이면 아직 설정 안 된 것이므로 업데이트하지 않음
            if (evt.maxHealth > 0 && healthGauge != null)
            {
                healthGauge.SetMaxValue(evt.maxHealth);
            }

            if (evt.maxSanity > 0 && sanityGauge != null)
            {
                sanityGauge.SetMaxValue(evt.maxSanity);
            }

            if (evt.maxHunger > 0 && hungerGauge != null)
            {
                hungerGauge.SetMaxValue(evt.maxHunger);
            }
        }


        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            if (EventHub.Instance != null)
            {
                // 플레이어 상태 업데이트 이벤트
                EventHub.Instance.RegisterEvent<EventStruct.UIEvents.PlayerStatusUpdateEvent>(OnPlayerStatusUpdate);
                EventHub.Instance.RegisterEvent<EventStruct.UIEvents.HealthChangedEvent>(OnHealthChanged);
                EventHub.Instance.RegisterEvent<EventStruct.UIEvents.SanityChangedEvent>(OnSanityChanged);
                EventHub.Instance.RegisterEvent<EventStruct.UIEvents.HungerChangedEvent>(OnHungerChanged);
                EventHub.Instance.RegisterEvent<EventStruct.UIEvents.ThirstChangedEvent>(OnThirstChanged);
                //EventHub.Instance.RegisterEvent<EventStruct.UIEvents.ColdChangedEvent>(OnColdChanged);
                EventHub.Instance.RegisterEvent<EventStruct.UIEvents.UIRefreshEvent>(OnUIRefresh);
            }
        }

        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (EventHub.Instance != null)
            {
                EventHub.Instance.UnregisterEvent<EventStruct.UIEvents.PlayerStatusUpdateEvent>(OnPlayerStatusUpdate);
                EventHub.Instance.UnregisterEvent<EventStruct.UIEvents.HealthChangedEvent>(OnHealthChanged);
                EventHub.Instance.UnregisterEvent<EventStruct.UIEvents.SanityChangedEvent>(OnSanityChanged);
                EventHub.Instance.UnregisterEvent<EventStruct.UIEvents.HungerChangedEvent>(OnHungerChanged);
                EventHub.Instance.UnregisterEvent<EventStruct.UIEvents.ThirstChangedEvent>(OnThirstChanged);
                //EventHub.Instance.UnregisterEvent<EventStruct.UIEvents.ColdChangedEvent>(OnColdChanged);
                EventHub.Instance.UnregisterEvent<EventStruct.UIEvents.UIRefreshEvent>(OnUIRefresh);
            }
        }

        /// <summary>
        /// 플레이어 상태 업데이트 이벤트 처리
        /// </summary>
        private void OnPlayerStatusUpdate(EventStruct.UIEvents.PlayerStatusUpdateEvent evt)
        {
            if (!isInitialized) return;

            // 로컬 플레이어의 데이터만 업데이트
            if (evt.actorNumber == Photon.Pun.PhotonNetwork.LocalPlayer.ActorNumber)
            {
                // 이벤트에서 받은 최대값으로 게이지 업데이트
                UpdateGaugeMaxValuesFromEvent(evt);

                // 현재값으로 게이지 업데이트
                UpdateHealth(evt.health);
                UpdateSanity(evt.sanity);
                UpdateHunger(evt.hunger);
            }
        }

        /// <summary>
        /// 체력 변경 이벤트 처리
        /// </summary>
        private void OnHealthChanged(EventStruct.UIEvents.HealthChangedEvent evt)
        {
            if (!isInitialized) return;
            UpdateHealth(evt.newHealth);
        }

        /// <summary>
        /// 정신력 변경 이벤트 처리
        /// </summary>
        private void OnSanityChanged(EventStruct.UIEvents.SanityChangedEvent evt)
        {
            if (!isInitialized) return;
            UpdateSanity(evt.newSanity);
        }

        /// <summary>
        /// 배고픔 변경 이벤트 처리
        /// </summary>
        private void OnHungerChanged(EventStruct.UIEvents.HungerChangedEvent evt)
        {
            if (!isInitialized) return;
            UpdateHunger(evt.newHunger);
        }

        /// <summary>
        /// 수분 변경 이벤트 처리
        /// </summary>
        private void OnThirstChanged(EventStruct.UIEvents.ThirstChangedEvent evt)
        {
            if (!isInitialized) return;
        }

        // /// <summary>
        // /// 추위 변경 이벤트 처리
        // /// </summary>
        // private void OnColdChanged(EventStruct.UIEvents.ColdChangedEvent evt)
        // {
        //     if (!isInitialized) return;
        //     UpdateCold(evt.newCold);
        // }

        /// <summary>
        /// UI 새로고침 이벤트 처리
        /// </summary>
        private void OnUIRefresh(EventStruct.UIEvents.UIRefreshEvent evt)
        {
            if (!isInitialized) return;

            if (evt.uiType == "PlayerStatus" || evt.uiType == "All")
            {
                RefreshAllGauges();
            }
        }

        /// <summary>
        /// 체력 업데이트
        /// </summary>
        private void UpdateHealth(float health)
        {
            if (healthGauge != null)
            {
                healthGauge.SetValue(health);
            }

            // 위험 수준 알림
            if (health <= dangerThreshold)
            {
                Debug.LogWarning($"[PlayerStatusPanel] 체력 위험 수준: {health:F0}%");
            }
        }

        /// <summary>
        /// 정신력 업데이트
        /// </summary>
        private void UpdateSanity(float sanity)
        {
            if (sanityGauge != null)
            {
                sanityGauge.SetValue(sanity);
            }

            if (sanity <= dangerThreshold)
            {
                Debug.LogWarning($"[PlayerStatusPanel] 정신력 위험 수준: {sanity:F0}%");
            }
        }

        /// <summary>
        /// 배고픔 업데이트
        /// </summary>
        private void UpdateHunger(float hunger)
        {
            if (hungerGauge != null)
            {
                hungerGauge.SetValue(hunger);
            }

            if (hunger <= dangerThreshold)
            {
                Debug.LogWarning($"[PlayerStatusPanel] 배고픔 위험 수준: {hunger:F0}%");
            }
        }

        /// <summary>
        /// 수동으로 플레이어 상태 설정 (테스트용)
        /// </summary>
        public void SetPlayerStatus(float health, float sanity, float hunger, float thirst, float cold)
        {
            UpdateHealth(health);
            UpdateSanity(sanity);
            UpdateHunger(hunger);
        }
        

        /// <summary>
        /// 게이지 애니메이션 설정
        /// </summary>
        public void SetGaugeAnimation(bool enable, float speed = 5f)
        {
            if (healthGauge != null) healthGauge.SetSmoothTransition(enable, speed);
            if (sanityGauge != null) sanityGauge.SetSmoothTransition(enable, speed);
            if (hungerGauge != null) hungerGauge.SetSmoothTransition(enable, speed);
            //if (coldGauge != null) coldGauge.SetSmoothTransition(enable, speed);
        }

        /// <summary>
        /// 모든 게이지 새로고침
        /// </summary>
        public void RefreshAllGauges()
        {
            if (healthGauge != null) healthGauge.RefreshGauge();
            if (sanityGauge != null) sanityGauge.RefreshGauge();
            if (hungerGauge != null) hungerGauge.RefreshGauge();
            //if (coldGauge != null) coldGauge.RefreshGauge();
        }
    }
}
