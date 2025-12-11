using UnityEngine;

namespace _Project.Script.Data
{
    /// <summary>
    /// 계절별 시간 비율 데이터
    /// </summary>
    [System.Serializable]
    public class SeasonTimeRatio
    {
        [Header("계절별 시간 비율")]
        [Range(0f, 1f)]
        public float dayRatio = 0.5f;

        [Range(0f, 1f)]
        public float eveningRatio = 0.3f;

        [Range(0f, 1f)]
        public float nightRatio = 0.2f;

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public SeasonTimeRatio()
        {
            dayRatio = 0.5f;
            eveningRatio = 0.3f;
            nightRatio = 0.2f;
        }

        /// <summary>
        /// 매개변수 생성자
        /// </summary>
        public SeasonTimeRatio(float day, float evening, float night)
        {
            dayRatio = day;
            eveningRatio = evening;
            nightRatio = night;
        }

        /// <summary>
        /// 비율이 유효한지 확인
        /// </summary>
        public bool IsValid()
        {
            return Mathf.Approximately(dayRatio + eveningRatio + nightRatio, 1f);
        }

        /// <summary>
        /// 비율 정규화
        /// </summary>
        public void Normalize()
        {
            float total = dayRatio + eveningRatio + nightRatio;
            if (total > 0f)
            {
                dayRatio /= total;
                eveningRatio /= total;
                nightRatio /= total;
            }
        }
    }
}
