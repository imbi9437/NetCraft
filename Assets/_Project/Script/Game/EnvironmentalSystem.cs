using UnityEngine;
using _Project.Script.EventStruct;
using _Project.Script.Manager;

namespace _Project.Script.Game
{
    /// <summary>
    /// 환경 효과 시스템 관리 클래스 (돈스타브 실제)
    /// 정신력, 체온, 젖음 상태 등 환경 효과 담당
    /// </summary>
    public class EnvironmentalSystem
    {
        [Header("환경 효과 설정")]
        [SerializeField] private float temperatureChangeRate = 0.1f;
        [SerializeField] private float wetnessChangeRate = 0.05f;

        /// <summary>
        /// 시간대별 정신력 감소 효과 적용
        /// </summary>
        public void ApplySanityEffects(TimeOfDay timeOfDay)
        {
            float sanityLossPerMinute = 0f;
            string reason = "";

            switch (timeOfDay)
            {
                case TimeOfDay.Day:
                    sanityLossPerMinute = 0f;
                    reason = "낮 시간 - 정신력 감소 없음";
                    break;
                case TimeOfDay.Evening:
                    sanityLossPerMinute = -5f;
                    reason = "저녁 시간 - 정신력 분당 5 감소";
                    break;
                case TimeOfDay.Night:
                    sanityLossPerMinute = -5f;
                    reason = "밤 시간 - 정신력 분당 5 감소 (어둠 속에서는 50 감소)";
                    break;
            }

            if (sanityLossPerMinute != 0f)
            {
                EventHub.Instance.RaiseEvent(new PunEvents.OnSanityChangeEvent
                {
                    sanityLossPerMinute = sanityLossPerMinute,
                    reason = reason
                });
            }
        }

        /// <summary>
        /// 체온 변화 효과 적용
        /// </summary>
        public void ApplyTemperatureEffects(Season season, TimeOfDay timeOfDay)
        {
            float temperatureChange = 0f;
            string reason = "";

            // 계절별 체온 효과
            switch (season)
            {
                case Season.Spring:
                    temperatureChange = -0.1f;
                    reason = "봄 - 약간 추운 날씨";
                    break;
                case Season.Summer:
                    temperatureChange = 0.2f;
                    reason = "여름 - 더운 날씨";
                    break;
                case Season.Autumn:
                    temperatureChange = 0f;
                    reason = "가을 - 적당한 날씨";
                    break;
                case Season.Winter:
                    temperatureChange = -0.5f;
                    reason = "겨울 - 매우 추운 날씨";
                    break;
            }

            // 시간대별 체온 효과
            switch (timeOfDay)
            {
                case TimeOfDay.Day:
                    temperatureChange += 0.1f;
                    break;
                case TimeOfDay.Evening:
                    temperatureChange += 0f;
                    break;
                case TimeOfDay.Night:
                    temperatureChange -= 0.2f;
                    break;
            }

            if (temperatureChange != 0f)
            {
                EventHub.Instance.RaiseEvent(new PunEvents.OnTemperatureChangeEvent
                {
                    temperatureChange = temperatureChange,
                    reason = reason
                });
            }
        }

        /// <summary>
        /// 젖음 상태 효과 적용
        /// </summary>
        public void ApplyWetnessEffects(Season season, WeatherType weather)
        {
            float wetnessChange = 0f;
            string reason = "";

            // 계절별 젖음 효과
            switch (season)
            {
                case Season.Spring:
                    wetnessChange = 0.1f;
                    reason = "봄 - 자주 내리는 비";
                    break;
                case Season.Summer:
                    wetnessChange = 0f;
                    reason = "여름 - 건조한 날씨";
                    break;
                case Season.Autumn:
                    wetnessChange = 0.05f;
                    reason = "가을 - 가끔 내리는 비";
                    break;
                case Season.Winter:
                    wetnessChange = 0.2f;
                    reason = "겨울 - 눈과 습기";
                    break;
            }

            // 날씨별 젖음 효과
            switch (weather)
            {
                case WeatherType.Rain:
                    wetnessChange += 0.3f;
                    break;
                case WeatherType.Snow:
                    wetnessChange += 0.1f;
                    break;
                case WeatherType.Clear:
                    wetnessChange -= 0.1f;
                    break;
            }

            if (wetnessChange != 0f)
            {
                EventHub.Instance.RaiseEvent(new PunEvents.OnWetnessChangeEvent
                {
                    wetnessChange = wetnessChange,
                    reason = reason
                });
            }
        }
    }
}
