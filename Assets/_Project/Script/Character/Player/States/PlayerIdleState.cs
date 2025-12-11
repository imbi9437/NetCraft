using UnityEngine;
using _Project.Script.StateMachine.Mono;

namespace _Project.Script.Character.Player.States
{
    /// <summary>
    /// 플레이어 대기 상태
    /// 이동 입력이 없을 때 활성화
    /// </summary>
    public class PlayerIdleState : MonoState
    {
        public override int index => (int)PlayerStateType.Idle;

        private PlayerStateMachine playerMachine;

        public override void Initialize(MonoStateMachine machine)
        {
            base.Initialize(machine);
            playerMachine = machine as PlayerStateMachine;
        }

        public override void OnEnable()
        {
            playerMachine.Animator.SetBool(PlayerStateMachine.IsMove, false);
            
            playerMachine.Animator.ResetTrigger(PlayerStateMachine.IsAttack);
            playerMachine.Animator.ResetTrigger(PlayerStateMachine.IsInteract);
            playerMachine.Animator.ResetTrigger(PlayerStateMachine.IsDie);
            playerMachine.Animator.ResetTrigger(PlayerStateMachine.IsHit);
        }

        public override void Update()
        {
            if (playerMachine.isMoving) playerMachine.ChangeState(PlayerStateType.Move);
        }
    }
}
