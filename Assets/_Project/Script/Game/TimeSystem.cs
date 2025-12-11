using UnityEngine;
using _Project.Script.EventStruct;
using _Project.Script.Manager;
using _Project.Script.Data;

namespace _Project.Script.Game
{
    /// <summary>
    /// 시간 시스템 관리 클래스 (돈스타브 실제)
    /// 16세그먼트 기반 시간 시스템 담당
    /// </summary>
    public class TimeSystem
    {
        [Header("시간 설정")]
        [SerializeField] private float dayDuration = 8f; // 하루 길이 (8분)
        [SerializeField] private float segmentDuration = 30f; // 1세그먼트 = 30초
        [SerializeField] private int totalSegments = 16; // 하루 총 세그먼트 수

        // 시간 상태
        private int currentSegment = 0;
        private float segmentTimer = 0f;
        private float gameTime = 0f;
        private int dayNumber = 1;

        public int CurrentSegment => currentSegment;
        public float SegmentTimer => segmentTimer;
        public float GameTime => gameTime;
        public int DayNumber => dayNumber;
        public float DayProgress => (float)currentSegment / totalSegments;

        /// <summary>
        /// 시간 업데이트
        /// </summary>
        public void UpdateTime()
        {
            segmentTimer += Time.deltaTime;

            if (segmentTimer >= segmentDuration)
            {
                segmentTimer = 0f;
                currentSegment++;

                if (currentSegment >= totalSegments)
                {
                    currentSegment = 0;
                    dayNumber++;
                    OnDayCompleted();
                }

                OnSegmentChanged();
            }

            gameTime = (currentSegment * segmentDuration) + segmentTimer;
        }

        /// <summary>
        /// 세그먼트 변화 이벤트
        /// </summary>
        private void OnSegmentChanged()
        {
            EventHub.Instance.RaiseEvent(new PunEvents.OnSegmentChangedEvent
            {
                currentSegment = currentSegment,
                totalSegments = totalSegments,
                segmentProgress = DayProgress
            });
        }

        /// <summary>
        /// 하루 완료 이벤트
        /// </summary>
        private void OnDayCompleted()
        {
            Debug.Log($"[TimeSystem] 하루 완료 - Day {dayNumber}");
        }

        /// <summary>
        /// 시간대 계산 (돈스타브 실제)
        /// </summary>
        public TimeOfDay GetTimeOfDay(SeasonTimeRatio seasonRatio)
        {
            float dayProgress = DayProgress;

            if (dayProgress < seasonRatio.dayRatio)
                return TimeOfDay.Day;
            else if (dayProgress < seasonRatio.dayRatio + seasonRatio.eveningRatio)
                return TimeOfDay.Evening;
            else
                return TimeOfDay.Night;
        }

        /// <summary>
        /// 게임 시간 설정
        /// </summary>
        public void SetGameTime(float time)
        {
            gameTime = time;
        }

        /// <summary>
        /// 일수 설정
        /// </summary>
        public void SetDayNumber(int day)
        {
            dayNumber = day;
        }

        /// <summary>
        /// 현재 세그먼트 설정
        /// </summary>
        public void SetCurrentSegment(int segment)
        {
            currentSegment = segment;
        }
    }
}
