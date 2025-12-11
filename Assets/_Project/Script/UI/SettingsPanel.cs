using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Project.Script.Manager;
using _Project.Script.UI.Main;
using _Project.Script.EventStruct;
using TMPro;
using Synty.Interface.FantasyMenus.Samples;

namespace _Project.Script.UI
{
    /// <summary>
    /// 환경설정 UI 패널 관리
    /// MainPanel을 상속받아 EventHub를 통해 SoundManager와 분리된 구조로 동작
    /// </summary>
    public class SettingsPanel : MainPanel
    {
        public override MainMenuPanelType UIType => MainMenuPanelType.Settings;

        [Header("음향 설정")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider uiVolumeSlider;
        [SerializeField] private Slider micVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [SerializeField] private TextMeshProUGUI musicVolumeText;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;
        [SerializeField] private TextMeshProUGUI uiVolumeText;
        [SerializeField] private TextMeshProUGUI micVolumeText;

        [Header("음성 채팅 설정")]
        [SerializeField] private Button microphoneButton;
        [SerializeField] private Image microphoneButtonImage;
        [SerializeField] private Sprite micOnSprite;
        [SerializeField] private Sprite micOffSprite;
        [SerializeField] private TextMeshProUGUI microphoneStatusText;

        [Header("플레이어 개별 음량 조절")]
        [SerializeField] private GameObject playerVolumeCardPrefab;
        [SerializeField] private Transform playerCardContainer;

        private bool isMicrophoneOn = true;
        private readonly Dictionary<int, GameObject> playerVolumeCards = new Dictionary<int, GameObject>();

        [Header("화면 설정")]
        [SerializeField] private SampleSettingsArray resolutionArray;
        [SerializeField] private SampleSettingsArray windowModeArray;

        [Header("버튼")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button closeButton;

        // 임시 저장 변수
        private int pendingResolutionIndex;
        private int pendingWindowModeIndex;

        // 믹서 그룹 이름 상수
        private const string MASTER_VOLUME = "Master";
        private const string MUSIC_VOLUME = "MusicVolume";
        private const string SFX_VOLUME = "SFXVolume";
        private const string UI_VOLUME = "UIVolume";
        private const string MIC_VOLUME = "MicVolume";

        private void Start()
        {
            SetupEventListeners();
        }

        private void OnEnable()
        {
            // 패널이 활성화될 때마다 현재 설정값 로드
            LoadCurrentVolumes();
            LoadMicrophoneState();
            LoadScreenSettings();
            RefreshPlayerVolumeCards();
        }

        private void OnDisable()
        {
            // 패널 비활성화 시 플레이어 카드 정리
            ClearPlayerVolumeCards();
        }

        /// <summary>
        /// 현재 볼륨값을 SoundManager에서 가져와 UI에 반영
        /// </summary>
        private void LoadCurrentVolumes()
        {
            if (SoundManager.Instance == null) return;

            // Master Volume
            if (masterVolumeSlider != null)
            {
                float volume = SoundManager.Instance.GetVolume(MASTER_VOLUME);
                masterVolumeSlider.SetValueWithoutNotify(volume);
                UpdateVolumeText(masterVolumeText, volume);
            }

            // Music Volume
            if (musicVolumeSlider != null)
            {
                float volume = SoundManager.Instance.GetVolume(MUSIC_VOLUME);
                musicVolumeSlider.SetValueWithoutNotify(volume);
                UpdateVolumeText(musicVolumeText, volume);
            }

            // SFX Volume
            if (sfxVolumeSlider != null)
            {
                float volume = SoundManager.Instance.GetVolume(SFX_VOLUME);
                sfxVolumeSlider.SetValueWithoutNotify(volume);
                UpdateVolumeText(sfxVolumeText, volume);
            }

            // UI Volume
            if (uiVolumeSlider != null)
            {
                float volume = SoundManager.Instance.GetVolume(UI_VOLUME);
                uiVolumeSlider.SetValueWithoutNotify(volume);
                UpdateVolumeText(uiVolumeText, volume);
            }

            // Mic Volume
            if (micVolumeSlider != null)
            {
                float volume = SoundManager.Instance.GetVolume(MIC_VOLUME);
                micVolumeSlider.SetValueWithoutNotify(volume);
                UpdateVolumeText(micVolumeText, volume);
            }

        }

        /// <summary>
        /// 현재 화면 설정을 SampleSettingsArray에 반영
        /// </summary>
        private void LoadScreenSettings()
        {
            // 현재 해상도 찾기
            if (resolutionArray != null)
            {
                string currentResolution = $"{Screen.width}x{Screen.height}";
                for (int i = 0; i < resolutionArray.options.Length; i++)
                {
                    if (resolutionArray.options[i] == currentResolution)
                    {
                        resolutionArray.SetOption(i);
                        pendingResolutionIndex = i;
                        break;
                    }
                }
            }

            // 현재 윈도우 모드 설정
            if (windowModeArray != null)
            {
                // 0: Fullscreen, 1: Windowed
                int modeIndex = Screen.fullScreen ? 0 : 1;
                windowModeArray.SetOption(modeIndex);
                pendingWindowModeIndex = modeIndex;
            }
        }

        /// <summary>
        /// 이벤트 리스너 설정
        /// </summary>
        private void SetupEventListeners()
        {
            // 음향 설정 슬라이더
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            if (uiVolumeSlider != null)
                uiVolumeSlider.onValueChanged.AddListener(OnUIVolumeChanged);
            if (micVolumeSlider != null)
                micVolumeSlider.onValueChanged.AddListener(OnMicVolumeChanged);

            // 화면 설정 - SampleSettingsArray 이벤트
            if (resolutionArray != null)
                resolutionArray.onValueChanged.AddListener(OnResolutionArrayChanged);
            if (windowModeArray != null)
                windowModeArray.onValueChanged.AddListener(OnWindowModeArrayChanged);

            // 버튼 이벤트
            if (applyButton != null)
                applyButton.onClick.AddListener(OnApplyButtonClick);
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseButtonClick);

            // 음성 채팅 설정
            if (microphoneButton != null)
                microphoneButton.onClick.AddListener(OnMicrophoneButtonClick);
        }

        #region 음향 설정 이벤트

        /// <summary>
        /// Master 볼륨 변경 - 모든 사운드를 한번에 조절
        /// </summary>
        private void OnMasterVolumeChanged(float value)
        {
            UpdateVolumeText(masterVolumeText, value);
            EventHub.Instance?.RaiseEvent(new RequestChangeVolumeEvent(MASTER_VOLUME, value));
        }

        /// <summary>
        /// Music 볼륨 변경 - EventHub를 통해 SoundManager에 전달
        /// </summary>
        private void OnMusicVolumeChanged(float value)
        {
            UpdateVolumeText(musicVolumeText, value);
            EventHub.Instance?.RaiseEvent(new RequestChangeVolumeEvent(MUSIC_VOLUME, value));
        }

        /// <summary>
        /// SFX 볼륨 변경 - EventHub를 통해 SoundManager에 전달
        /// </summary>
        private void OnSfxVolumeChanged(float value)
        {
            UpdateVolumeText(sfxVolumeText, value);
            EventHub.Instance?.RaiseEvent(new RequestChangeVolumeEvent(SFX_VOLUME, value));
        }

        /// <summary>
        /// UI 볼륨 변경 - EventHub를 통해 SoundManager에 전달
        /// </summary>
        private void OnUIVolumeChanged(float value)
        {
            UpdateVolumeText(uiVolumeText, value);
            EventHub.Instance?.RaiseEvent(new RequestChangeVolumeEvent(UI_VOLUME, value));
        }

        /// <summary>
        /// Mic 볼륨 변경 - EventHub를 통해 SoundManager에 전달
        /// </summary>
        private void OnMicVolumeChanged(float value)
        {
            UpdateVolumeText(micVolumeText, value);
            EventHub.Instance?.RaiseEvent(new RequestChangeVolumeEvent(MIC_VOLUME, value));
        }

        /// <summary>
        /// 볼륨 텍스트 업데이트
        /// </summary>
        private void UpdateVolumeText(TextMeshProUGUI textComponent, float volume)
        {
            if (textComponent != null)
            {
                textComponent.text = Mathf.RoundToInt(volume).ToString();
            }
        }

        #endregion

        #region 음성 채팅 설정

        /// <summary>
        /// 현재 마이크 상태 로드 - NetworkManagerVoice를 통해 직접 확인
        /// </summary>
        private void LoadMicrophoneState()
        {
            // NetworkManagerVoice.AllPlayerVoices에서 로컬 플레이어 찾기
            foreach (var playerVoice in VoiceChat.NetworkManagerVoice.AllPlayerVoices)
            {
                if (playerVoice != null && playerVoice.photonView != null && playerVoice.photonView.IsMine)
                {
                    isMicrophoneOn = playerVoice.voiceRecorder.TransmitEnabled;
                    UpdateMicrophoneStatusText(isMicrophoneOn);
                    return;
                }
            }

            // 로컬 플레이어를 못 찾으면 기본값
            isMicrophoneOn = true;
            UpdateMicrophoneStatusText(isMicrophoneOn);
        }

        /// <summary>
        /// 마이크 버튼 클릭 - 토글 방식으로 작동
        /// </summary>
        private void OnMicrophoneButtonClick()
        {
            // 상태 토글
            isMicrophoneOn = !isMicrophoneOn;

            // NetworkManagerVoice.AllPlayerVoices에서 로컬 플레이어 찾아서 제어
            foreach (var playerVoice in VoiceChat.NetworkManagerVoice.AllPlayerVoices)
            {
                if (playerVoice != null && playerVoice.photonView != null && playerVoice.photonView.IsMine)
                {
                    playerVoice.voiceRecorder.TransmitEnabled = isMicrophoneOn;
                    Debug.Log($"[SettingsPanel] 마이크 {(isMicrophoneOn ? "켜짐" : "꺼짐")}");
                    break;
                }
            }

            UpdateMicrophoneStatusText(isMicrophoneOn);
        }

        /// <summary>
        /// 마이크 상태 텍스트 및 이미지 업데이트
        /// </summary>
        private void UpdateMicrophoneStatusText(bool isEnabled)
        {
            if (microphoneStatusText != null)
            {
                microphoneStatusText.text = isEnabled ? "Mic On" : "Mic Off";
            }

            // 이미지 null 체크 추가
            if (microphoneButtonImage != null)
            {
                microphoneButtonImage.sprite = isEnabled ? micOnSprite : micOffSprite;
            }
        }

        /// <summary>
        /// 플레이어 음량 카드 갱신 - 현재 접속한 플레이어들 표시
        /// </summary>
        private void RefreshPlayerVolumeCards()
        {
            if (playerVolumeCardPrefab == null || playerCardContainer == null) return;

            // 기존 카드 모두 제거
            ClearPlayerVolumeCards();

            // NetworkManagerVoice.AllPlayerVoices에서 원격 플레이어만 카드 생성
            foreach (var playerVoice in VoiceChat.NetworkManagerVoice.AllPlayerVoices)
            {
                if (playerVoice != null && playerVoice.photonView != null && !playerVoice.photonView.IsMine)
                {
                    CreatePlayerVolumeCard(playerVoice);
                }
            }
        }

        /// <summary>
        /// 개별 플레이어 음량 조절 카드 생성
        /// </summary>
        private void CreatePlayerVolumeCard(VoiceChat.PlayerVoice playerVoice)
        {
            if (playerVolumeCards.ContainsKey(playerVoice.photonView.Owner.ActorNumber)) return;

            GameObject card = Instantiate(playerVolumeCardPrefab, playerCardContainer);
            card.SetActive(true);

            // PlayerVolumeSlider 컴포넌트 찾아서 설정
            var sliderScript = card.GetComponent<PlayerVolumeSlider>();
            if (sliderScript != null)
            {
                // PlayerPrefs에서 저장된 볼륨 불러오기 (기본값 0.7)
                float savedVolume = PlayerPrefs.GetFloat($"PlayerVolume_{playerVoice.photonView.Owner.ActorNumber}", 0.7f);

                // Setup 호출 (VoiceUI 대신 직접 관리)
                sliderScript.Setup(playerVoice.photonView.Owner, null, savedVolume);

                // 슬라이더 이벤트 직접 연결
                var slider = card.GetComponentInChildren<Slider>();
                if (slider != null)
                {
                    int actorNumber = playerVoice.photonView.Owner.ActorNumber;
                    slider.onValueChanged.AddListener((volume) => OnPlayerVolumeChanged(actorNumber, volume));
                }

                // 초기 볼륨 적용
                playerVoice.SetSpeakerVolume(savedVolume);
            }

            playerVolumeCards[playerVoice.photonView.Owner.ActorNumber] = card;
            Debug.Log($"[SettingsPanel] {playerVoice.photonView.Owner.NickName} 플레이어 카드 생성");
        }

        /// <summary>
        /// 플레이어 음량 변경 핸들러
        /// </summary>
        private void OnPlayerVolumeChanged(int actorNumber, float volume)
        {
            // NetworkManagerVoice.AllPlayerVoices에서 해당 플레이어 찾아서 볼륨 적용
            foreach (var playerVoice in VoiceChat.NetworkManagerVoice.AllPlayerVoices)
            {
                if (playerVoice != null && playerVoice.photonView.Owner.ActorNumber == actorNumber)
                {
                    playerVoice.SetSpeakerVolume(volume);

                    // PlayerPrefs에 저장
                    PlayerPrefs.SetFloat($"PlayerVolume_{actorNumber}", volume);
                    PlayerPrefs.Save();

                    Debug.Log($"[SettingsPanel] {playerVoice.photonView.Owner.NickName} 볼륨: {volume}");
                    break;
                }
            }
        }

        /// <summary>
        /// 모든 플레이어 카드 제거
        /// </summary>
        private void ClearPlayerVolumeCards()
        {
            foreach (var card in playerVolumeCards.Values)
            {
                if (card != null)
                {
                    Destroy(card);
                }
            }
            playerVolumeCards.Clear();
        }

        #endregion

        #region 화면 설정 이벤트

        /// <summary>
        /// 해상도 배열 변경 - 임시 저장만 (Apply 버튼으로 적용)
        /// </summary>
        private void OnResolutionArrayChanged(int index, string value)
        {
            pendingResolutionIndex = index;
            Debug.Log($"[SettingsPanel] 해상도 선택: {value} (Apply 버튼으로 적용)");
        }

        /// <summary>
        /// 윈도우 모드 배열 변경 - 임시 저장만 (Apply 버튼으로 적용)
        /// </summary>
        private void OnWindowModeArrayChanged(int index, string value)
        {
            pendingWindowModeIndex = index;
            Debug.Log($"[SettingsPanel] 윈도우 모드 선택: {value} (Apply 버튼으로 적용)");
        }

        #endregion

        #region 버튼 기능

        /// <summary>
        /// Apply 버튼 - 화면 설정 적용
        /// </summary>
        private void OnApplyButtonClick()
        {
            // 해상도 적용
            if (resolutionArray != null && pendingResolutionIndex >= 0 && pendingResolutionIndex < resolutionArray.options.Length)
            {
                string resolutionText = resolutionArray.options[pendingResolutionIndex];
                string[] resolution = resolutionText.Split('x');

                if (resolution.Length == 2 && int.TryParse(resolution[0], out int width) && int.TryParse(resolution[1], out int height))
                {
                    bool isFullscreen = pendingWindowModeIndex == 0; // 0: Fullscreen, 1: Windowed
                    Screen.SetResolution(width, height, isFullscreen);
                    Debug.Log($"[SettingsPanel] 화면 설정 적용: {width}x{height}, Fullscreen={isFullscreen}");
                }
            }

            // PlayerPrefs에 저장
            PlayerPrefs.SetInt("ScreenWidth", Screen.width);
            PlayerPrefs.SetInt("ScreenHeight", Screen.height);
            PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
            PlayerPrefs.Save();

            Debug.Log("[SettingsPanel] 화면 설정이 적용되었습니다!");
        }

        private void OnCloseButtonClick()
        {
            Hide();
        }

        #endregion

        private void OnDestroy()
        {
            // 이벤트 리스너 해제
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            if (uiVolumeSlider != null)
                uiVolumeSlider.onValueChanged.RemoveListener(OnUIVolumeChanged);
            if (micVolumeSlider != null)
                micVolumeSlider.onValueChanged.RemoveListener(OnMicVolumeChanged);

            if (resolutionArray != null)
                resolutionArray.onValueChanged.RemoveListener(OnResolutionArrayChanged);
            if (windowModeArray != null)
                windowModeArray.onValueChanged.RemoveListener(OnWindowModeArrayChanged);

            if (microphoneButton != null)
                microphoneButton.onClick.RemoveListener(OnMicrophoneButtonClick);

            if (applyButton != null)
                applyButton.onClick.RemoveListener(OnApplyButtonClick);
            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }
    }
}