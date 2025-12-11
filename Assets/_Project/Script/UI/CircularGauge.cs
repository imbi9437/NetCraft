using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Project.Script.UI
{
    /// <summary>
    /// 원형 게이지 UI 컴포넌트
    /// 위에서 아래로 깎이는 원형 게이지
    /// </summary>
    public class CircularGauge : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Image gaugeImage;
        [SerializeField] private Text valueText;
        [SerializeField] private Text labelText;

        [Header("게이지 설정")]
        [SerializeField] private float minValue = 0f;
        [SerializeField] private float maxValue = 100f;
        [SerializeField] private float currentValue = 100f;


        [Header("애니메이션 설정")]
        [SerializeField] private bool enableSmoothTransition = true;
        [SerializeField] private float transitionSpeed = 5f;

        private float targetValue;
        private Color targetColor;
        private bool isInitialized = false;

        private void Start()
        {
            InitializeGauge();
        }

        private void Update()
        {
            if (enableSmoothTransition && isInitialized)
            {
                UpdateSmoothTransition();
            }
        }

        /// <summary>
        /// 게이지 초기화
        /// </summary>
        private void InitializeGauge()
        {
            if (gaugeImage != null)
            {
                // 원형 마스크 설정
                gaugeImage.type = Image.Type.Filled;
                gaugeImage.fillMethod = Image.FillMethod.Radial360;
                gaugeImage.fillOrigin = 2; // 위에서 시작 (12시 방향)
                gaugeImage.fillClockwise = false; // 시계 반대 방향으로 채움
                gaugeImage.fillAmount = 1f; // 처음에는 가득 참
            }

            // 초기값 설정
            targetValue = currentValue;

            // 게이지 및 텍스트 초기화
            UpdateGaugeVisual();

            isInitialized = true;

            Debug.Log($"[CircularGauge] 초기화 완료 - currentValue: {currentValue}, fillAmount: {gaugeImage?.fillAmount}, valueText: {valueText?.text}");
        }

        /// <summary>
        /// 부드러운 전환 업데이트
        /// </summary>
        private void UpdateSmoothTransition()
        {
            if (Mathf.Abs(currentValue - targetValue) > 0.01f)
            {
                currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * transitionSpeed);
                UpdateGaugeVisual();
            }
        }

        /// <summary>
        /// 게이지 시각적 업데이트
        /// </summary>
        private void UpdateGaugeVisual()
        {
            if (gaugeImage == null) return;

            // 값 범위 정규화 (0-1)
            float normalizedValue = Mathf.Clamp01((currentValue - minValue) / (maxValue - minValue));

            // 위에서 아래로 깎이도록 fillAmount 설정
            // 1.0 = 가득 참, 0.0 = 비어있음
            gaugeImage.fillAmount = normalizedValue;

            // 값 텍스트도 함께 업데이트
            UpdateValueText();
        }

        /// <summary>
        /// 값 설정
        /// </summary>
        public void SetValue(float value)
        {
            targetValue = Mathf.Clamp(value, minValue, maxValue);

            if (!enableSmoothTransition)
            {
                currentValue = targetValue;
                UpdateGaugeVisual();
            }
        }

        /// <summary>
        /// 즉시 값 설정 (애니메이션 없이)
        /// </summary>
        public void SetValueImmediate(float value)
        {
            targetValue = Mathf.Clamp(value, minValue, maxValue);
            currentValue = targetValue;
            UpdateGaugeVisual();
        }

        /// <summary>
        /// 최대값 설정
        /// </summary>
        public void SetMaxValue(float max)
        {
            maxValue = max;
            UpdateGaugeVisual();
        }

        /// <summary>
        /// 최소값 설정
        /// </summary>
        public void SetMinValue(float min)
        {
            minValue = min;
            UpdateGaugeVisual();
        }


        /// <summary>
        /// 라벨 텍스트 설정
        /// </summary>
        public void SetLabel(string label)
        {
            if (labelText != null)
            {
                labelText.text = label;
            }
        }

        /// <summary>
        /// 값 텍스트 업데이트
        /// </summary>
        private void UpdateValueText()
        {
            if (valueText != null)
            {
                valueText.text = $"{currentValue:F0}";
            }
        }

        /// <summary>
        /// 현재 값 가져오기
        /// </summary>
        public float GetCurrentValue()
        {
            return currentValue;
        }

        /// <summary>
        /// 정규화된 값 가져오기 (0-1)
        /// </summary>
        public float GetNormalizedValue()
        {
            return Mathf.Clamp01((currentValue - minValue) / (maxValue - minValue));
        }

        /// <summary>
        /// 게이지 리셋
        /// </summary>
        public void ResetGauge()
        {
            SetValueImmediate(maxValue);
        }

        /// <summary>
        /// 게이지 비우기
        /// </summary>
        public void EmptyGauge()
        {
            SetValueImmediate(minValue);
        }

        /// <summary>
        /// 애니메이션 설정
        /// </summary>
        public void SetSmoothTransition(bool enable, float speed = 5f)
        {
            enableSmoothTransition = enable;
            transitionSpeed = speed;
        }

        /// <summary>
        /// 게이지 완전히 업데이트
        /// </summary>
        public void RefreshGauge()
        {
            UpdateGaugeVisual();
            UpdateValueText();
        }
    }
}
