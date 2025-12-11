using UnityEngine;
using _Project.Script.StateMachine.Mono;
using _Project.Script.Manager;
using _Project.Script.EventStruct;

namespace _Project.Script.Character.Player.States
{
    /// <summary>
    /// 플레이어 이동 상태
    /// WASD 입력에 따른 이동 처리
    /// </summary>
    public class PlayerMoveState : MonoState
    {
        public override int index => (int)PlayerStateType.Move;

        private PlayerStateMachine playerMachine;
        
        private float footstepTimer = 0f;
        private float footstepInterval = 0.5f;  // 0.5초마다 발소리

        public override void Initialize(MonoStateMachine machine)
        {
            base.Initialize(machine);
            playerMachine = machine as PlayerStateMachine;
        }

        public override void OnEnable()
        {
            playerMachine.Animator.SetBool(PlayerStateMachine.IsMove, true);
            footstepTimer = 0f;  // 상태 진입 시 초기화 
        }

        public override void Update()
        {
            // 여기서는 Move 상태에만 필요한 특수 로직 작성
            // 예: 발자국 소리, 먼지 파티클 등
            
            if (playerMachine.isMoving == false) playerMachine.ChangeState(PlayerStateType.Idle);
            
            // 타이머 증가
            footstepTimer += Time.deltaTime;

            // 일정 시간마다 발소리 재생
            if (footstepTimer >= footstepInterval)
            {
                footstepTimer = 0f;  // 타이머 리셋

                EventHub.Instance.RaiseEvent(new RequestPlaySoundEvent
                {
                    id = "Player_Walk",
                    position = playerMachine.transform.position,
                    pitch = UnityEngine.Random.Range(0.9f, 1.1f),  // 약간의 변화
                    spatialBlend = 1f,
                    loop = false,
                    mixerGroupName = "SFXVolum"
                });

                // EventHub.Instance.RaiseEvent(new RequestPlayEffectEvent // 먼지 이펙트 , 발자국 이펙트 등 이벤트
                // {
                //     id = "Player_Footstep",
                //     position = playerMachine.transform.position,
                //     rotation = Quaternion.identity,
                //     parent = playerMachine.transform,
                //     duration = 0.5f,
                //     worldSpace = true,
                //     scale = Vector3.one
                // });
            }
        }

        public override void OnDisable()
        {
            playerMachine.Animator.SetBool(PlayerStateMachine.IsMove, false);
            footstepTimer = 0f;
        }
    }
}
