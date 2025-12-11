using UnityEngine;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using _Project.Script.Data;

namespace _Project.Script.Game
{
    /// <summary>
    /// 계절 시스템 관리 클래스 (돈스타브 실제)
    /// 계절별 시간 비율, 시각 효과, 환경 효과 담당
    /// </summary>
    public class SeasonSystem
    {
        [Header("계절 설정")]
        [SerializeField] private float seasonDuration = 20f; // 계절 길이 (일)

        [Header("계절별 시간 비율 (돈스타브 실제)")]
        [SerializeField] private SeasonTimeRatio springTimeRatio = new SeasonTimeRatio(0.5f, 0.3f, 0.2f);
        [SerializeField] private SeasonTimeRatio summerTimeRatio = new SeasonTimeRatio(0.7f, 0.2f, 0.1f);
        [SerializeField] private SeasonTimeRatio autumnTimeRatio = new SeasonTimeRatio(0.4f, 0.3f, 0.3f);
        [SerializeField] private SeasonTimeRatio winterTimeRatio = new SeasonTimeRatio(0.3f, 0.2f, 0.5f);

        [Header("계절별 시각 효과")]
        [SerializeField] private Color springColor = new Color(0.8f, 1f, 0.8f, 1f);
        [SerializeField] private Color summerColor = new Color(1f, 0.9f, 0.7f, 1f);
        [SerializeField] private Color autumnColor = new Color(1f, 0.8f, 0.6f, 1f);
        [SerializeField] private Color winterColor = new Color(0.9f, 0.9f, 1f, 1f);

        // 계절 상태
        private Season currentSeason = Season.Autumn;
        private Season previousSeason = Season.Autumn;
        private int seasonDay = 1;
        private bool hasSeasonChanged = false;

        public Season CurrentSeason => currentSeason;
        public Season PreviousSeason => previousSeason;
        public int SeasonDay => seasonDay;
        public bool HasSeasonChanged => hasSeasonChanged;

        /// <summary>
        /// 계절 변화 체크
        /// </summary>
        public void CheckSeasonChange(int dayNumber)
        {
            Season newSeason = GetSeasonFromDayNumber(dayNumber);

            if (newSeason != currentSeason)
            {
                previousSeason = currentSeason;
                currentSeason = newSeason;
                seasonDay = 1;
                hasSeasonChanged = true;

                OnSeasonChanged();
            }
            else
            {
                hasSeasonChanged = false;
                seasonDay = GetSeasonDayFromDayNumber(dayNumber);
            }
        }

        /// <summary>
        /// 계절 변화 이벤트
        /// </summary>
        private void OnSeasonChanged()
        {
            // 계절 변화 이벤트 발생
            EventHub.Instance.RaiseEvent(new PunEvents.OnSeasonChangedEvent
            {
                newSeason = currentSeason,
                previousSeason = previousSeason,
                transitionDuration = 5f
            });

            // 계절별 시각 효과 적용
            ApplySeasonVisualEffects();

            Debug.Log($"[SeasonSystem] 계절 변화 - {previousSeason} → {currentSeason}");
        }

        /// <summary>
        /// 계절별 시각 효과 적용
        /// </summary>
        private void ApplySeasonVisualEffects()
        {
            Color seasonColor = GetSeasonColor(currentSeason);

            EventHub.Instance.RaiseEvent(new PunEvents.OnSeasonVisualEffectEvent
            {
                seasonColor = seasonColor,
                seasonName = currentSeason.ToString(),
                transitionDuration = 5f
            });
        }

        /// <summary>
        /// 현재 계절의 시간 비율 반환
        /// </summary>
        public SeasonTimeRatio GetCurrentSeasonTimeRatio()
        {
            switch (currentSeason)
            {
                case Season.Spring: return springTimeRatio;
                case Season.Summer: return summerTimeRatio;
                case Season.Autumn: return autumnTimeRatio;
                case Season.Winter: return winterTimeRatio;
                default: return autumnTimeRatio;
            }
        }

        /// <summary>
        /// 일수에서 계절 계산
        /// </summary>
        private Season GetSeasonFromDayNumber(int dayNumber)
        {
            int seasonCycle = dayNumber % (int)(seasonDuration * 4);

            if (seasonCycle < seasonDuration) return Season.Spring;
            else if (seasonCycle < seasonDuration * 2) return Season.Summer;
            else if (seasonCycle < seasonDuration * 3) return Season.Autumn;
            else return Season.Winter;
        }

        /// <summary>
        /// 계절별 색상 반환
        /// </summary>
        private Color GetSeasonColor(Season season)
        {
            switch (season)
            {
                case Season.Spring: return springColor;
                case Season.Summer: return summerColor;
                case Season.Autumn: return autumnColor;
                case Season.Winter: return winterColor;
                default: return autumnColor;
            }
        }

        /// <summary>
        /// 계절 내 일수 계산
        /// </summary>
        private int GetSeasonDayFromDayNumber(int dayNumber)
        {
            int seasonStartDay = ((int)currentSeason - 1) * (int)seasonDuration + 1;
            return dayNumber - seasonStartDay + 1;
        }

        /// <summary>
        /// 현재 계절 설정
        /// </summary>
        public void SetCurrentSeason(Season season)
        {
            if (season != currentSeason)
            {
                previousSeason = currentSeason;
                currentSeason = season;
                hasSeasonChanged = true;
            }
        }

        /// <summary>
        /// 계절 일수 설정
        /// </summary>
        public void SetSeasonDay(int day)
        {
            seasonDay = day;
        }
    }
}
