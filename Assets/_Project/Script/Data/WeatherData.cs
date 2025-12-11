using UnityEngine;
using _Project.Script.EventStruct;

namespace _Project.Script.Data
{
    /// <summary>
    /// 날씨 데이터
    /// </summary>
    [System.Serializable]
    public class WeatherData
    {
        [Header("날씨 정보")]
        public _Project.Script.EventStruct.WeatherType currentWeather;
        public float intensity;
        public float duration;
        public float remainingTime;

        [Header("시각 효과")]
        public Color skyColor = Color.white;
        public float windStrength;
        public bool isRaining;
        public bool isSnowing;

        [Header("이전 상태")]
        public _Project.Script.EventStruct.WeatherType previousWeather;
        public float transitionProgress;

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public WeatherData()
        {
            currentWeather = _Project.Script.EventStruct.WeatherType.Clear;
            intensity = 0f;
            duration = 0f;
            remainingTime = 0f;
            skyColor = Color.white;
            windStrength = 0f;
            isRaining = false;
            isSnowing = false;
            previousWeather = _Project.Script.EventStruct.WeatherType.Clear;
            transitionProgress = 0f;
        }

        /// <summary>
        /// 매개변수 생성자
        /// </summary>
        public WeatherData(_Project.Script.EventStruct.WeatherType weather, float inten, float dur, Color sky, float wind, bool rain, bool snow)
        {
            currentWeather = weather;
            intensity = inten;
            duration = dur;
            remainingTime = dur;
            skyColor = sky;
            windStrength = wind;
            isRaining = rain;
            isSnowing = snow;
            previousWeather = weather;
            transitionProgress = 0f;
        }

        /// <summary>
        /// 날씨 업데이트
        /// </summary>
        public void UpdateWeather(_Project.Script.EventStruct.WeatherType newWeather, float newIntensity, float newDuration)
        {
            currentWeather = newWeather;
            intensity = newIntensity;
            duration = newDuration;
            remainingTime = newDuration;

            // 시각 효과 업데이트
            UpdateVisualEffects();
        }

        /// <summary>
        /// 시간 경과 처리
        /// </summary>
        public void UpdateTime(float deltaTime)
        {
            if (remainingTime > 0f)
            {
                remainingTime -= deltaTime;
                if (remainingTime <= 0f)
                {
                    // 날씨 종료
                    currentWeather = _Project.Script.EventStruct.WeatherType.Clear;
                    intensity = 0f;
                    UpdateVisualEffects();
                }
            }
        }

        /// <summary>
        /// 시각 효과 업데이트
        /// </summary>
        private void UpdateVisualEffects()
        {
            switch (currentWeather)
            {
                case _Project.Script.EventStruct.WeatherType.Clear:
                    skyColor = Color.white;
                    windStrength = 0f;
                    isRaining = false;
                    isSnowing = false;
                    break;
                case _Project.Script.EventStruct.WeatherType.Rain:
                    skyColor = Color.gray;
                    windStrength = intensity * 0.5f;
                    isRaining = true;
                    isSnowing = false;
                    break;
                case _Project.Script.EventStruct.WeatherType.Snow:
                    skyColor = Color.white;
                    windStrength = intensity * 0.3f;
                    isRaining = false;
                    isSnowing = true;
                    break;
                case _Project.Script.EventStruct.WeatherType.Storm:
                    skyColor = Color.black;
                    windStrength = intensity;
                    isRaining = true;
                    isSnowing = false;
                    break;
            }
        }

        /// <summary>
        /// 데이터 복사
        /// </summary>
        public WeatherData Clone()
        {
            return new WeatherData(currentWeather, intensity, duration, skyColor, windStrength, isRaining, isSnowing);
        }
    }
}
