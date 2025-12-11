using UnityEngine;
using _Project.Script.EventStruct;
using _Project.Script.Manager;

namespace _Project.Script.Game
{
    /// <summary>
    /// 날씨 시스템 관리 클래스 (돈스타브 실제)
    /// 날씨 변화, 계절별 날씨 패턴 담당
    /// </summary>
    public class WeatherSystem
    {
        [Header("날씨 설정")]
        [SerializeField] private float weatherChangeInterval = 30f; // 날씨 변화 간격 (분)
        [SerializeField] private float weatherTransitionDuration = 5f; // 날씨 전환 시간

        // 날씨 상태
        private WeatherType currentWeather = WeatherType.Clear;
        private WeatherType previousWeather = WeatherType.Clear;
        private float weatherTimer = 0f;
        private float weatherIntensity = 0f;
        private float transitionProgress = 0f;
        private float weatherDuration = 0f;
        private bool hasWeatherChanged = false;

        public WeatherType CurrentWeather => currentWeather;
        public float WeatherIntensity => weatherIntensity;
        public float TransitionProgress => transitionProgress;
        public float WeatherDuration => weatherDuration;
        public bool HasWeatherChanged => hasWeatherChanged;

        /// <summary>
        /// 날씨 업데이트
        /// </summary>
        public void UpdateWeather(Season currentSeason)
        {
            weatherTimer += Time.deltaTime;

            if (weatherTimer >= weatherChangeInterval * 60f) // 분을 초로 변환
            {
                weatherTimer = 0f;
                ChangeWeather(currentSeason);
            }
        }

        /// <summary>
        /// 날씨 변화
        /// </summary>
        private void ChangeWeather(Season season)
        {
            WeatherType newWeather = GetRandomWeatherForSeason(season);

            if (newWeather != currentWeather)
            {
                previousWeather = currentWeather;
                currentWeather = newWeather;
                weatherDuration = Random.Range(10f, 30f); // 10-30분 지속
                hasWeatherChanged = true;

                OnWeatherChanged();
            }
            else
            {
                hasWeatherChanged = false;
            }
        }

        /// <summary>
        /// 계절별 랜덤 날씨 생성
        /// </summary>
        private WeatherType GetRandomWeatherForSeason(Season season)
        {
            float random = Random.Range(0f, 1f);

            switch (season)
            {
                case Season.Spring:
                    if (random < 0.4f) return WeatherType.Rain;
                    else if (random < 0.7f) return WeatherType.Cloudy;
                    else return WeatherType.Clear;

                case Season.Summer:
                    if (random < 0.1f) return WeatherType.Rain;
                    else if (random < 0.3f) return WeatherType.Cloudy;
                    else return WeatherType.Clear;

                case Season.Autumn:
                    if (random < 0.2f) return WeatherType.Rain;
                    else if (random < 0.5f) return WeatherType.Cloudy;
                    else return WeatherType.Clear;

                case Season.Winter:
                    if (random < 0.3f) return WeatherType.Snow;
                    else if (random < 0.6f) return WeatherType.Cloudy;
                    else return WeatherType.Clear;

                default:
                    return WeatherType.Clear;
            }
        }

        /// <summary>
        /// 날씨 변화 이벤트
        /// </summary>
        private void OnWeatherChanged()
        {
            EventHub.Instance.RaiseEvent(new PunEvents.OnWeatherChangedEvent
            {
                newWeather = currentWeather,
                previousWeather = previousWeather
            });

            Debug.Log($"[WeatherSystem] 날씨 변화 - {previousWeather} → {currentWeather}");
        }

        /// <summary>
        /// 현재 날씨 설정
        /// </summary>
        public void SetCurrentWeather(WeatherType weather)
        {
            if (weather != currentWeather)
            {
                previousWeather = currentWeather;
                currentWeather = weather;
                hasWeatherChanged = true;
            }
        }

        /// <summary>
        /// 날씨 강도 설정
        /// </summary>
        public void SetWeatherIntensity(float intensity)
        {
            weatherIntensity = Mathf.Clamp01(intensity);
        }
    }
}
