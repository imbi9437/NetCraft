using UnityEngine;
using _Project.Script.StateMachine.Mono;
using Photon.Pun;

namespace _Project.Script.Character.Player.States
{
    /// <summary>
    /// 플레이어 상호작용 상태
    /// 아이템 줍기, 구조물 조작 등
    /// </summary>
    public class PlayerInteractState : MonoState
    {
        public override int index => (int)PlayerStateType.Interact;

        private PlayerStateMachine playerMachine;
        private bool hasInteracted;
        private Animator animator;
        private float interactionTime = 1f; // 상호작용 시간
        private float currentTime = 0f;

        public override void Initialize(MonoStateMachine machine)
        {
            base.Initialize(machine);
            playerMachine = machine as PlayerStateMachine;
        }

        public override void OnEnable()
        {
            playerMachine.Animator.SetTrigger(PlayerStateMachine.IsInteract);

            if (PhotonNetwork.IsConnected && playerMachine.PhotonView.IsMine)
            {
                playerMachine.PhotonView.RPC(nameof(playerMachine.PlayInteractAnimationRPC), RpcTarget.Others);
            }
            
            hasInteracted = false;
            playerMachine.isOtherAction = true;
        }

        public override void Update()
        {
            var curState = playerMachine.Animator.GetCurrentAnimatorStateInfo(0);

            if (hasInteracted == false &&curState.IsName("Interact") == false) return;

            if (curState.normalizedTime >= 0.5f && hasInteracted == false)
            {
                if (PhotonNetwork.IsMasterClient) playerMachine.PerformInteract();
                else playerMachine.PhotonView.RPC(nameof(playerMachine.PerformInteractRPC), RpcTarget.MasterClient);
            }
            
            if (hasInteracted && curState.IsName("Interact") == false)
                playerMachine.ChangeState(PlayerStateType.Idle);
        }

        public override void OnDisable()
        {
            playerMachine.isOtherAction = false;
        }

        
    }
}
