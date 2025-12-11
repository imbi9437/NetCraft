using UnityEngine;
using _Project.Script.StateMachine.Mono;
using _Project.Script.Interface;
using Photon.Pun;

namespace _Project.Script.Character.Player.States
{
    /// <summary>
    /// 플레이어 공격 상태
    /// 공격 애니메이션 및 데미지 처리
    /// </summary>
    public class PlayerAttackState : MonoState
    {
        public override int index => (int)PlayerStateType.Attack;

        private PlayerStateMachine playerMachine;
        private bool hasAttacked = false;
        
        public override void Initialize(MonoStateMachine machine)
        {
            base.Initialize(machine);
            playerMachine = machine as PlayerStateMachine;
        }

        public override void OnEnable()
        {
            playerMachine.Animator.SetTrigger(PlayerStateMachine.IsAttack);

            if (PhotonNetwork.IsConnected && playerMachine.PhotonView.IsMine)
            {
                playerMachine.PhotonView.RPC(nameof(playerMachine.PlayAttackAnimationRPC), RpcTarget.Others);
            }

            hasAttacked = false;
            playerMachine.isOtherAction = true;
        }

        public override void Update()
        {
            var curState = playerMachine.Animator.GetCurrentAnimatorStateInfo(0);
            
            if (hasAttacked == false && curState.IsName("Attack") == false) return;

            if (curState.normalizedTime >= 0.5f && hasAttacked == false)
            {
                if (PhotonNetwork.IsMasterClient)
                    playerMachine.PerformAttack();
                else 
                    playerMachine.PhotonView.RPC(nameof(playerMachine.PerformAttackRPC), RpcTarget.MasterClient);
                
                hasAttacked = true;
            }
            
            if (hasAttacked && curState.IsName("Attack") == false)
                playerMachine.ChangeState(PlayerStateType.Idle);
        }

        public override void OnDisable()
        {
            playerMachine.isOtherAction = false;
        }

        
    }
}
