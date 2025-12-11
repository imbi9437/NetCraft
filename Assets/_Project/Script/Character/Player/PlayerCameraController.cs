using UnityEngine;
using Cinemachine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace _Project.Script.Character.Player
{
    public class PlayerCameraController : MonoBehaviour
    {
        private Vector3 cameraOffset = new(0, 10, -10);  // 고정 오프셋
        
        [Header("시네머신 설정")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        private CinemachineFramingTransposer virtualCameraTransposer;
        private CinemachineComposer virtualCameraComposer;

        [Header("회전 설정값")]
        [SerializeField] private int rotateMaxIndex = 4;
        [SerializeField] private float rotateTime = 0.5f;
        private int curRotateIndex;
        private float currentRotation;
        private TweenerCore<Quaternion, Quaternion, NoOptions> tween;
        

        [Header("줌 설정값")] 
        [SerializeField] private Vector2 zoomRange;
        [SerializeField] private int zoomRangeCount = 3;
        [SerializeField] private float zoomSpeed = 0.5f;
        private int curZoomIndex = 2;
        
        public float CurrentRotation => currentRotation;  // 카메라 기준 이동을 위한 현재 회전 각도
        
        public void Initialize(bool isLocalPlayer = false)
        {
            virtualCamera ??= GetComponent<CinemachineVirtualCamera>();
            virtualCameraTransposer ??= virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            virtualCameraComposer ??= virtualCamera.GetCinemachineComponent<CinemachineComposer>();
            
            virtualCamera.enabled = isLocalPlayer;

            if (isLocalPlayer == false) return;
            
            SetupCinemachineCamera();
        }

        /// <summary> 시네머신 카메라 설정 </summary>
        private void SetupCinemachineCamera()
        {
            if (virtualCamera == null) return;
            
            virtualCamera.Follow = cameraTarget;
            virtualCamera.LookAt = cameraTarget;
            
            if (virtualCameraTransposer != null)
            {
                virtualCameraTransposer.m_TrackedObjectOffset = cameraOffset;
                virtualCameraTransposer.m_CameraDistance = CalculateZoomValue(curZoomIndex);
                
                virtualCameraTransposer.m_XDamping = 0f;
                virtualCameraTransposer.m_YDamping = 0f;
                virtualCameraTransposer.m_ZDamping = 0f;
            }
            
            if (virtualCameraComposer != null)
            {
                virtualCameraComposer.m_TrackedObjectOffset = Vector3.zero;
                
                virtualCameraComposer.m_HorizontalDamping = 0f;
                virtualCameraComposer.m_VerticalDamping = 0f;
            }
            
            virtualCamera.transform.parent = null;
            cameraTarget.transform.parent = null;
        }
        
        /// <summary> 카메라 회전 </summary>
        public void RotateCamera(bool isRight)
        {
            if (cameraTarget == null) return;
            
            curRotateIndex = isRight ? curRotateIndex + 1 : curRotateIndex - 1;
            curRotateIndex %= rotateMaxIndex;

            currentRotation = curRotateIndex * (360f / rotateMaxIndex);
            Quaternion quaternion = Quaternion.Euler(0, currentRotation, 0);
            
            if (tween.IsActive()) tween.Kill();
            tween = cameraTarget.DORotateQuaternion(quaternion, rotateTime);
        }
        
        
        /// <summary> 카메라 줌 </summary>
        public void ZoomCamera(bool isScrollDown)
        {
            curZoomIndex = isScrollDown ? curZoomIndex + 1 : curZoomIndex - 1;
            curZoomIndex = Mathf.Clamp(curZoomIndex, 0, zoomRangeCount);
            virtualCameraTransposer.m_CameraDistance = CalculateZoomValue(curZoomIndex);;
        }

        private float CalculateZoomValue(int index)
        {
            float alpha = index / (float)zoomRangeCount;
            float result = Mathf.Lerp(zoomRange.x, zoomRange.y, alpha);
            return result;
        }
    }
}