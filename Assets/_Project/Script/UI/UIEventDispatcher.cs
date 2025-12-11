using UnityEngine;
using _Project.Script.EventStruct;
using _Project.Script.Manager;

namespace _Project.Script.UI
{
    /// <summary>
    /// UI 이벤트 발송 유틸리티 클래스
    /// 데이터 변경 시 UI 업데이트 이벤트를 자동으로 발송
    /// </summary>
    public static class UIEventDispatcher
    {
        /// <summary>
        /// 플레이어 상태 업데이트 이벤트 발송
        /// </summary>
        public static void DispatchPlayerStatusUpdate(float health, float sanity, float hunger, float thirst, float cold, int actorNumber)
        {
            if (EventHub.Instance != null)
            {
                var evt = new UIEvents.PlayerStatusUpdateEvent
                {
                    health = health,
                    sanity = sanity,
                    hunger = hunger,
                    thirst = thirst,
                    cold = cold,
                    actorNumber = actorNumber
                };
                EventHub.Instance.RaiseEvent(evt);
            }
        }

        /// <summary>
        /// 체력 변경 이벤트 발송
        /// </summary>
        public static void DispatchHealthChanged(float oldHealth, float newHealth, int actorNumber)
        {
            if (EventHub.Instance != null)
            {
                var evt = new UIEvents.HealthChangedEvent
                {
                    oldHealth = oldHealth,
                    newHealth = newHealth,
                    actorNumber = actorNumber
                };
                EventHub.Instance.RaiseEvent(evt);
            }
        }

        /// <summary>
        /// 정신력 변경 이벤트 발송
        /// </summary>
        public static void DispatchSanityChanged(float oldSanity, float newSanity, int actorNumber)
        {
            if (EventHub.Instance != null)
            {
                var evt = new UIEvents.SanityChangedEvent
                {
                    oldSanity = oldSanity,
                    newSanity = newSanity,
                    actorNumber = actorNumber
                };
                EventHub.Instance.RaiseEvent(evt);
            }
        }

        /// <summary>
        /// 배고픔 변경 이벤트 발송
        /// </summary>
        public static void DispatchHungerChanged(float oldHunger, float newHunger, int actorNumber)
        {
            if (EventHub.Instance != null)
            {
                var evt = new UIEvents.HungerChangedEvent
                {
                    oldHunger = oldHunger,
                    newHunger = newHunger,
                    actorNumber = actorNumber
                };
                EventHub.Instance.RaiseEvent(evt);
            }
        }

        /// <summary>
        /// 수분 변경 이벤트 발송
        /// </summary>
        public static void DispatchThirstChanged(float oldThirst, float newThirst, int actorNumber)
        {
            if (EventHub.Instance != null)
            {
                var evt = new UIEvents.ThirstChangedEvent
                {
                    oldThirst = oldThirst,
                    newThirst = newThirst,
                    actorNumber = actorNumber
                };
                EventHub.Instance.RaiseEvent(evt);
            }
        }

        /// <summary>
        /// 추위 변경 이벤트 발송
        /// </summary>
        public static void DispatchColdChanged(float oldCold, float newCold, int actorNumber)
        {
            if (EventHub.Instance != null)
            {
                var evt = new UIEvents.ColdChangedEvent
                {
                    oldCold = oldCold,
                    newCold = newCold,
                    actorNumber = actorNumber
                };
                EventHub.Instance.RaiseEvent(evt);
            }
        }

        /// <summary>
        /// 게임 정보 업데이트 이벤트 발송
        /// </summary>
        public static void DispatchGameInfoUpdate(float gameTime, int dayNumber, int season, int weather, float temperature)
        {
            if (EventHub.Instance != null)
            {
                var evt = new UIEvents.GameInfoUpdateEvent
                {
                    gameTime = gameTime,
                    dayNumber = dayNumber,
                    season = season,
                    weather = weather,
                    temperature = temperature
                };
                EventHub.Instance.RaiseEvent(evt);
            }
        }

        /// <summary>
        /// 게임 시간 변경 이벤트 발송
        /// </summary>
        public static void DispatchGameTimeChanged(float oldTime, float newTime)
        {
            if (EventHub.Instance != null)
            {
                var evt = new UIEvents.GameTimeChangedEvent
                {
                    oldTime = oldTime,
                    newTime = newTime
                };
                EventHub.Instance.RaiseEvent(evt);
            }
        }

        /// <summary>
        /// 계절 변경 이벤트 발송
        /// </summary>
        public static void DispatchSeasonChanged(int oldSeason, int newSeason)
        {
            if (EventHub.Instance != null)
            {
                var evt = new UIEvents.SeasonChangedEvent
                {
                    oldSeason = oldSeason,
                    newSeason = newSeason
                };
                EventHub.Instance.RaiseEvent(evt);
            }
        }

        /// <summary>
        /// 날씨 변경 이벤트 발송
        /// </summary>
        public static void DispatchWeatherChanged(int oldWeather, int newWeather)
        {
            if (EventHub.Instance != null)
            {
                var evt = new UIEvents.WeatherChangedEvent
                {
                    oldWeather = oldWeather,
                    newWeather = newWeather
                };
                EventHub.Instance.RaiseEvent(evt);
            }
        }

        /// <summary>
        /// 온도 변경 이벤트 발송
        /// </summary>
        public static void DispatchTemperatureChanged(float oldTemperature, float newTemperature)
        {
            if (EventHub.Instance != null)
            {
                var evt = new UIEvents.TemperatureChangedEvent
                {
                    oldTemperature = oldTemperature,
                    newTemperature = newTemperature
                };
                EventHub.Instance.RaiseEvent(evt);
            }
        }

        /// <summary>
        /// UI 새로고침 이벤트 발송
        /// </summary>
        public static void DispatchUIRefresh(string uiType = "All")
        {
            if (EventHub.Instance != null)
            {
                var evt = new UIEvents.UIRefreshEvent
                {
                    uiType = uiType
                };
                EventHub.Instance.RaiseEvent(evt);
            }
        }
    }
}
