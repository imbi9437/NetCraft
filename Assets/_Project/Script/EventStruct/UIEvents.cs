using UnityEngine;
using _Project.Script.Interface;

namespace _Project.Script.EventStruct
{
    /// <summary>
    /// UI 관련 이벤트 구조체들
    /// </summary>

    public static class UIEvents
    {
        /// <summary>
        /// 플레이어 상태 업데이트 이벤트
        /// </summary>
        public struct PlayerStatusUpdateEvent : IEvent
        {
            // 현재값
            public float health;
            public float sanity;
            public float hunger;
            public float thirst;
            public float cold;

            // 최대값 (게이지 설정용)
            public float maxHealth;
            public float maxSanity;
            public float maxHunger;
            public float maxThirst;
            public float maxCold;

            public int actorNumber;
        }

        /// <summary>
        /// 게임 정보 업데이트 이벤트
        /// </summary>
        public struct GameInfoUpdateEvent : IEvent
        {
            public float gameTime;
            public int dayNumber;
            public int season;
            public int weather;
            public float temperature;
        }

        /// <summary>
        /// 체력 변경 이벤트
        /// </summary>
        public struct HealthChangedEvent : IEvent
        {
            public float oldHealth;
            public float newHealth;
            public int actorNumber;
        }

        /// <summary>
        /// 정신력 변경 이벤트
        /// </summary>
        public struct SanityChangedEvent : IEvent
        {
            public float oldSanity;
            public float newSanity;
            public int actorNumber;
        }

        /// <summary>
        /// 배고픔 변경 이벤트
        /// </summary>
        public struct HungerChangedEvent : IEvent
        {
            public float oldHunger;
            public float newHunger;
            public int actorNumber;
        }

        /// <summary>
        /// 수분 변경 이벤트
        /// </summary>
        public struct ThirstChangedEvent : IEvent
        {
            public float oldThirst;
            public float newThirst;
            public int actorNumber;
        }

        /// <summary>
        /// 추위 변경 이벤트
        /// </summary>
        public struct ColdChangedEvent : IEvent
        {
            public float oldCold;
            public float newCold;
            public int actorNumber;
        }

        /// <summary>
        /// 게임 시간 변경 이벤트
        /// </summary>
        public struct GameTimeChangedEvent : IEvent
        {
            public float oldTime;
            public float newTime;
        }

        /// <summary>
        /// 계절 변경 이벤트
        /// </summary>
        public struct SeasonChangedEvent : IEvent
        {
            public int oldSeason;
            public int newSeason;
        }

        /// <summary>
        /// 날씨 변경 이벤트
        /// </summary>
        public struct WeatherChangedEvent : IEvent
        {
            public int oldWeather;
            public int newWeather;
        }

        /// <summary>
        /// 온도 변경 이벤트
        /// </summary>
        public struct TemperatureChangedEvent : IEvent
        {
            public float oldTemperature;
            public float newTemperature;
        }

        /// <summary>
        /// UI 새로고침 이벤트
        /// </summary>
        public struct UIRefreshEvent : IEvent
        {
            public string uiType; // "PlayerStatus", "GameInfo", "All"
        }
    }

}
