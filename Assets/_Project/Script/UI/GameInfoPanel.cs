using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Project.Script.Generic;
using _Project.Script.EventStruct;
using _Project.Script.Data;
using _Project.Script.Manager;

namespace _Project.Script.UI
{
    /// <summary>
    /// 게임 정보 UI 패널
    /// 현재 시간, 날짜, 계절, 날씨 등을 표시
    /// </summary>
    public class GameInfoPanel : MonoBehaviour
    {
        [Header("시간 정보 UI")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI dayText;

        [Header("계절 정보 UI")]
        [SerializeField] private TextMeshProUGUI seasonText;
        [SerializeField] private Image seasonIcon;
        [SerializeField] private Sprite[] seasonIcons; // 봄, 여름, 가을, 겨울 아이콘

        [Header("날씨 정보 UI")]
        [SerializeField] private TextMeshProUGUI weatherText;
        [SerializeField] private Image weatherIcon;
        [SerializeField] private Sprite[] weatherIcons; // 맑음, 흐림, 비, 폭풍, 눈, 안개 아이콘

        [Header("온도 정보 UI")]
        [SerializeField] private TextMeshProUGUI temperatureText;
        [SerializeField] private Slider temperatureSlider;
        [SerializeField] private Image temperatureFillImage;

        [Header("색상 설정")]
        [SerializeField] private Color hotColor = Color.red;
        [SerializeField] private Color coldColor = Color.blue;
        [SerializeField] private Color normalColor = Color.green;

        private GameTimeData currentGameData;
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
            // 온도 슬라이더 초기화
            if (temperatureSlider != null)
            {
                temperatureSlider.minValue = -50f;
                temperatureSlider.maxValue = 50f;
                temperatureSlider.value = 20f;
            }

            isInitialized = true;
            Debug.Log("[GameInfoPanel] UI 초기화 완료");
        }

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            if (EventHub.Instance != null)
            {
                // 게임 정보 업데이트 이벤트
                EventHub.Instance.RegisterEvent<UIEvents.GameInfoUpdateEvent>(OnGameInfoUpdate);
                EventHub.Instance.RegisterEvent<UIEvents.GameTimeChangedEvent>(OnGameTimeChanged);
                EventHub.Instance.RegisterEvent<UIEvents.SeasonChangedEvent>(OnSeasonChanged);
                EventHub.Instance.RegisterEvent<UIEvents.WeatherChangedEvent>(OnWeatherChanged);
                EventHub.Instance.RegisterEvent<UIEvents.TemperatureChangedEvent>(OnTemperatureChanged);
                EventHub.Instance.RegisterEvent<UIEvents.UIRefreshEvent>(OnUIRefresh);
            }
        }

        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (EventHub.Instance != null)
            {
                EventHub.Instance.UnregisterEvent<UIEvents.GameInfoUpdateEvent>(OnGameInfoUpdate);
                EventHub.Instance.UnregisterEvent<UIEvents.GameTimeChangedEvent>(OnGameTimeChanged);
                EventHub.Instance.UnregisterEvent<UIEvents.SeasonChangedEvent>(OnSeasonChanged);
                EventHub.Instance.UnregisterEvent<UIEvents.WeatherChangedEvent>(OnWeatherChanged);
                EventHub.Instance.UnregisterEvent<UIEvents.TemperatureChangedEvent>(OnTemperatureChanged);
                EventHub.Instance.UnregisterEvent<UIEvents.UIRefreshEvent>(OnUIRefresh);
            }
        }

        /// <summary>
        /// 게임 정보 업데이트 이벤트 처리
        /// </summary>
        private void OnGameInfoUpdate(UIEvents.GameInfoUpdateEvent evt)
        {
            if (!isInitialized) return;

            UpdateGameTime(evt.gameTime);
            UpdateDayNumber(evt.dayNumber);
            UpdateSeason(evt.season);
            UpdateWeather(evt.weather);
            UpdateTemperature(evt.temperature);
        }

        /// <summary>
        /// 게임 시간 변경 이벤트 처리
        /// </summary>
        private void OnGameTimeChanged(UIEvents.GameTimeChangedEvent evt)
        {
            if (!isInitialized) return;
            UpdateGameTime(evt.newTime);
        }

        /// <summary>
        /// 계절 변경 이벤트 처리
        /// </summary>
        private void OnSeasonChanged(UIEvents.SeasonChangedEvent evt)
        {
            if (!isInitialized) return;
            UpdateSeason(evt.newSeason);
        }

        /// <summary>
        /// 날씨 변경 이벤트 처리
        /// </summary>
        private void OnWeatherChanged(UIEvents.WeatherChangedEvent evt)
        {
            if (!isInitialized) return;
            UpdateWeather(evt.newWeather);
        }

        /// <summary>
        /// 온도 변경 이벤트 처리
        /// </summary>
        private void OnTemperatureChanged(UIEvents.TemperatureChangedEvent evt)
        {
            if (!isInitialized) return;
            UpdateTemperature(evt.newTemperature);
        }

        /// <summary>
        /// UI 새로고침 이벤트 처리
        /// </summary>
        private void OnUIRefresh(UIEvents.UIRefreshEvent evt)
        {
            if (!isInitialized) return;

            if (evt.uiType == "GameInfo" || evt.uiType == "All")
            {
                // 현재 데이터로 UI 새로고침
                if (currentGameData != null)
                {
                    UpdateGameTime(currentGameData.gameTime);
                    UpdateDayNumber(currentGameData.dayNumber);
                    UpdateSeason((int)currentGameData.currentSeason);
                    UpdateWeather((int)currentGameData.currentWeather);
                    UpdateTemperature(20f); // 기본 온도 (실제로는 계산된 값이어야 함)
                }
            }
        }

        /// <summary>
        /// 게임 시간 업데이트
        /// </summary>
        private void UpdateGameTime(float gameTime)
        {
            // 게임 시간을 실제 시간으로 변환 (예: 1일 = 20분)
            float realTime = gameTime * 20f; // 20분 = 1일
            int hours = Mathf.FloorToInt(realTime / 60f);
            int minutes = Mathf.FloorToInt(realTime % 60f);

            if (timeText != null)
            {
                timeText.text = $"시간: {hours:D2}:{minutes:D2}";
            }
        }

        /// <summary>
        /// 날짜 업데이트
        /// </summary>
        private void UpdateDayNumber(int dayNumber)
        {
            if (dayText != null)
            {
                dayText.text = $"Day {dayNumber}";
            }

            if (dateText != null)
            {
                // 계절에 따른 날짜 표시
                string seasonName = GetSeasonName(GetCurrentSeason());
                dateText.text = $"{seasonName} {dayNumber}일";
            }
        }

        /// <summary>
        /// 계절 업데이트
        /// </summary>
        private void UpdateSeason(int season)
        {
            string seasonName = GetSeasonName(season);

            if (seasonText != null)
            {
                seasonText.text = $"계절: {seasonName}";
            }

            if (seasonIcon != null && seasonIcons != null && season >= 0 && season < seasonIcons.Length)
            {
                seasonIcon.sprite = seasonIcons[season];
            }
        }

        /// <summary>
        /// 날씨 업데이트
        /// </summary>
        private void UpdateWeather(int weather)
        {
            string weatherName = GetWeatherName(weather);

            if (weatherText != null)
            {
                weatherText.text = $"날씨: {weatherName}";
            }

            if (weatherIcon != null && weatherIcons != null && weather >= 0 && weather < weatherIcons.Length)
            {
                weatherIcon.sprite = weatherIcons[weather];
            }
        }

        /// <summary>
        /// 온도 업데이트
        /// </summary>
        private void UpdateTemperature(float temperature)
        {
            if (temperatureText != null)
            {
                temperatureText.text = $"온도: {temperature:F1}°C";
            }

            if (temperatureSlider != null)
            {
                temperatureSlider.value = temperature;
            }

            // 온도에 따른 색상 변경
            UpdateTemperatureColor(temperature);
        }

        /// <summary>
        /// 온도에 따른 색상 업데이트
        /// </summary>
        private void UpdateTemperatureColor(float temperature)
        {
            if (temperatureFillImage == null) return;

            Color targetColor;

            if (temperature >= 30f)
            {
                targetColor = hotColor;
            }
            else if (temperature <= 0f)
            {
                targetColor = coldColor;
            }
            else
            {
                targetColor = normalColor;
            }

            temperatureFillImage.color = targetColor;
        }

        /// <summary>
        /// 계절 이름 가져오기
        /// </summary>
        private string GetSeasonName(int season)
        {
            switch (season)
            {
                case 0: return "봄";
                case 1: return "여름";
                case 2: return "가을";
                case 3: return "겨울";
                default: return "알 수 없음";
            }
        }

        /// <summary>
        /// 날씨 이름 가져오기
        /// </summary>
        private string GetWeatherName(int weather)
        {
            switch (weather)
            {
                case 0: return "맑음";
                case 1: return "흐림";
                case 2: return "비";
                case 3: return "폭풍";
                case 4: return "눈";
                case 5: return "안개";
                default: return "알 수 없음";
            }
        }

        /// <summary>
        /// 현재 계절 가져오기 (임시)
        /// </summary>
        private int GetCurrentSeason()
        {
            // 실제로는 게임 데이터에서 가져와야 함
            return 0; // 봄
        }

        /// <summary>
        /// 수동으로 게임 정보 설정 (테스트용)
        /// </summary>
        public void SetGameInfo(float gameTime, int dayNumber, int season, int weather, float temperature)
        {
            UpdateGameTime(gameTime);
            UpdateDayNumber(dayNumber);
            UpdateSeason(season);
            UpdateWeather(weather);
            UpdateTemperature(temperature);
        }

        /// <summary>
        /// 모든 정보 리셋
        /// </summary>
        public void ResetAllInfo()
        {
            SetGameInfo(0f, 1, 0, 0, 20f);
        }
    }
}
